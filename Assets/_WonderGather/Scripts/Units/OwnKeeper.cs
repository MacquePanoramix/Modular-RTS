using UnityEngine;

namespace WonderGather
{
    // The keeper of a body of its own (S3b; Docs/Design/TheBodysOwn.md): what keeps thirteen weighted parts on
    // joints standing, by what the legs' joints give against the ground and nothing else. One piece of code, used
    // by the body in the game (OwnBody) and by the bench that searches its settings (BodysOwnBench), so that what
    // is searched is what is played. Each step of the physics:
    //  - what the ground should push the body with is reckoned: its weight, a little more or less to keep its
    //    height, and sideways as much as brings its weight back from where it is going;
    //  - where on its soles that push must act, no further than the soles reach; where the ground really bore it
    //    in the last step is read from what touched its boots, and what it means is corrected by how far that
    //    fell short, but never off a sole;
    //  - each leg's hip, knee and ankle give what makes its boot push the ground so, and no more than they have;
    //  - its hips keep its trunk upright;
    //  - what pushes it from outside is felt (by how its weight really went), and it leans against it.
    // Every torque is put on the two parts a joint joins, equal and opposite. Whoever owns the body may live in
    // the middle of each step (Life): it says how far from where it stands the weight is held (Shift) and how its
    // hips roll (Roll).
    public sealed class OwnKeeper
    {
        // The parts, in this order (the left of a pair first).
        public const int Hips = 0, Trunk = 1, Head = 2, UpperArm = 3, Forearm = 5, Thigh = 7, Shin = 9, Foot = 11, Count = 13;
        // It is down when its weight is lower than this share of how high it stood, or its hips lean further than
        // this (degrees).
        public const float DownAt = .8f, DownLeaning = 35;

        // ---- Its settings. Found by a search on the three miners together (BodysOwnBench.Search, at the game's
        // fifty steps of the physics a second), not set by hand: change what the search is asked, run it, and put
        // what it finds here.
        // How quick it brings its weight back (a second); over how long it comes to feel a push from outside
        // (seconds); how fast it leans against one (a second), by what share of what would ease it, only for what
        // is beyond Comfort (metres from where its push acts at ease), and never leaving less sole than KeepsBehind
        // behind its weight; how fast it learns how far the ground's real push falls short of the one it meant (a
        // second).
        public static float Quick = 6.33f, Feels = .04005f, Leans = 2.9021f, LeansBy = .94264f, Comfort = .03266f, KeepsBehind = .02071f, Tracks = 6.8665f;
        // Each joint's spring gives all the joint's strength when it is FullAt degrees from its pose, but is no
        // stiffer than makes its own rate, times the step, Rule (by the turning weight of the lighter side of the
        // joint: a stiff spring between a heavy body and a light boot does not hold at the game's step); its damper
        // is Damped of what just stops that side swinging.
        public static float FullAt = 92.743f, Rule = .64038f, Damped = .38328f;
        // How hard its hips right its trunk, for each kilogram of it, and the share of that which damps it; how near
        // the edge of a sole its push may act (metres); how hard it keeps its height, and how that is damped.
        // (Edge was 0.00435 until October 13, found by a search on a level floor. On the meadow's ground, which slopes
        // a couple of degrees and is a mesh, a boot whose push is asked to act so near its edge tips over that edge.
        // The three miners, with life in them, shoved at 0.5 m/s four ways on the meadow: 11 of 12 stood with
        // 0.00435 (Small went down), 12 with 0.008 and with 0.012, 11 with 0.02 (Round, its soles then too narrow
        // to stand a sideways shove through, stepped late and went down). One run each: a narrow window.)
        public static float Rights = 5.3963f, RightsDamped = .05f, Edge = .01f, Rises = 224.2f, RisesDamped = 14.277f;

        private readonly ArticulationBody[] parts;
        private readonly int[] from;
        private readonly float[] masses, strength;
        private readonly bool[] hinge;
        private readonly Vector3[] anchors, soleAt, soleSize;
        private readonly GroundTouch[] touches;
        private readonly float whole;
        // Its state.
        private bool begun, hasBefore, realKnown;
        private Vector3 rest, holds, felt, wentBefore, weightBefore, actedBefore, realAt, shortBy, acted, stands;
        private Vector2 standsBy;
        private Quaternion upright;
        private float height;
        private readonly Vector3[] torque = new Vector3[Count], sole = new Vector3[2], at = new Vector3[2];
        private readonly float[] gave = new float[Count];

        // Where it wants the ground to push it is further the way its body tips: by Tilts of how far its hips have
        // tipped (times its height), and TiltsRate of how fast, over its rate of falling. (A pull above its weight
        // turns the whole body over, and a push that passes through its weight never turns it back.) Nought: not.
        public static float Tilts = 0.3658f, TiltsRate = 0.274f;

        // ---- The step (S3b, step 3; what was read for it: the research of October 9, part 7). Set by hand to begin
        // with; to be found by a search on the three miners together, as the settings above were.
        // Whether it steps at all (off: it stands as it did, and goes down when it cannot).
        public static bool Steps = false;
        // It steps when where its weight is going has been further than StepsBeyond (metres) outside what its soles
        // can bear for StepsAfter seconds. The leg is unloaded over UnloadsIn seconds; its boot swings for SwingsIn,
        // lifted Clears metres in the middle; to StepsAhead times as far from the boot it stands on as its weight is
        // going now, and StepsFurther metres beyond; no further from its hip than Reach of the leg's length. Sideways
        // (its fall more than Sideways across it) it steps with the leg on the side it falls to; else with the leg
        // that bears less.
        public static float StepsBeyond = 0.035f, StepsAfter = 0.0389f, UnloadsIn = 0.0123f, SwingsIn = 0.1205f, Clears = 0.0841f;
        // (Tried on October 13, and off: how long a boot swings going with the leg. SwingsIn would be for a leg of
        // SwingsLeg metres from hip to ankle (0.544: Small's), and a longer leg take longer, as the root of its
        // length, as a pendulum does: the judges saw legs of 0.54, 0.64 and 0.88 m all swing in the same time. And
        // the step that brings the other boot alongside taking ClosesSlower times as long (1.6): they saw that boot
        // skim the ground. In the game, the three miners shoved four ways at 0.5 and at 0.7 m/s, one run each, of
        // 12: as it is, 12 and 10 stand; the leg's length alone, 11 (Long down, and five steps where it took two);
        // the slower second step alone, 12 and 9; both, 11 and 10. A slower step leaves it longer on one boot than
        // it can yet stand. 0, and 1: as it is.)
        public static float SwingsLeg = 0;
        public static float ClosesSlower = 1;
        public static float StepsAhead = 1.7984f, StepsFurther = 0.0219f, Reach = 0.7771f, Sideways = 0.6257f;
        // (How much of what pushes it counts in whether it must step, and where to: a blow is felt as a great push
        // for a moment, and is not one that lasts.)
        public static float StepsFelt = 0.764f;
        // A swinging boot has landed when it bears on the ground; or, if it has not by then, at this many times the
        // time its swing takes (1: at the end of its swing whether it bears or not, as the step was first built).
        // Past the end of its swing it is put straight down from where it is, onto the ground it left: a boot
        // put further than its leg reaches from hips still so high hangs in the air until the body has fallen
        // onto it. (October 11: let go by the clock, a pulled miner's boot was 2 to 5 cm in the air; it was taken
        // for landed, and swung again or came down a hand's width nearer than it was put.)
        public static float LandsBy = 1.3392f;
        // What it feels counts towards a step (StepsFelt) only as far as it has lasted this long, seconds: the least
        // it has felt over that time. A shove is over in a step of the physics, and where its weight is going
        // already tells of it; a pull that is held goes on. 0: all it feels at once, as the step was first built.
        public static float StepsFeltOver = 0.1652f;
        // (Tried on October 13 in its place: a push known to last by being there at every step of the physics for
        // 0.05 s. Against shoves over in one step it was sooner, and a sixth of its weight was stood by 6 of 24
        // for 2. Against shoves that last a tenth of a second, as the game's do, it took a shove for a pull: 9 of
        // 24 stood a shove of 0.5 m/s, for 23. Taken out.)
        // Standing with one boot this far before the other (along the way it falls; a share of how far apart its
        // boots stand at ease), and not falling sideways across its body, the boot behind is the one that steps:
        // never the boot in front, which is holding it. 0: not so (the leg is chosen as for boots
        // side by side, as the step was first built). (October 11: with its boots astride after a first step, the
        // line between them is no longer across it, a fall forward was taken for a fall sideways, and it lifted the
        // boot in front.)
        public static float TrailsFrom = 1.0148f;
        // What five bodies pulled with a sixth of their weight showed, told at every step of the physics (October
        // 12): their joints were asked for 0.7 of what they have at the most; what they felt was not misjudged; and
        // their weight was over the boot they stood on. But every first step landed with where their weight was
        // going at or past what the landed boot could hold: the step really took 0.14 to 0.30 s, not the 0.14 its
        // length was reckoned for; the boot hung 3 to 4 cm over the ground for up to 0.12 s, its leg too short
        // to reach from hips still at standing height; and every step after it, the boot behind having to pass
        // the boot in front in the same 0.12 s, landed beside that boot or behind it. Three things were built for
        // it, and searched with and without. One earned its place, the sinking below (a pull of 0.12 of its weight
        // stood by 18 of 24, from 13). A far boot taking longer changed nothing; the step's length reckoned from
        // how long it takes made it worse (10 of 24, and hard shoves 9 from 18). Both are taken out again.
        // It sinks into a catching step: its hips come down by this share of what its leg needs to reach the ground
        // where its boot is to land (at most SinksAtMost of its height), and come up again over SinksBack
        // seconds once it has landed. 0: its hips are held at standing height through a step. (It comes down at
        // SinksDown metres a second, not all in one step of the physics; and no further than well short of where
        // it would be taken for fallen.)
        public static float Sinks = 0.2615f, SinksAtMost = .12f, SinksBack = .5f;
        private const float SinksDown = .5f;
        // How fast it lets itself come down, as a share of falling free (with the step off: a half, as it was).
        public static float SinksFree = 0.4797f;
        // The hip of a swinging leg is sprung as stiffly as its knee and its ankle are in the air. 0: it is left as
        // it stands (and the boot came 14 to 25 cm behind where it was wanted by the end of its swing).
        public static float SwingHip = 0.7069f;
        // Its trunk leans back against a push that lasts: so many degrees for each 10 cm the ground would have to
        // push beyond its ease, LeansBackAtMost at the most. 0: its trunk is held as it stands, and a pull at its
        // chest folds it forward.
        public static float LeansBack = 8.3633f, LeansBackAtMost = 22;
        private float sinksBy, sinkWants, leansNow, lastingAsked;
        private Vector3 lastingFelt;
        private bool leaned;
        private readonly Vector3[] feltLately = new Vector3[16];
        private int feltTold;
        private float leftGround;
        private bool putDown;
        // For this long after a boot lands (seconds), how its weight went is not taken for a push from outside.
        public static float LandsDeaf = 0.1131f;
        // A boot just put down takes up its share as the ground comes to bear it: for LoadsFor seconds after it
        // lands, its leg is asked for no more push than Loads times what the ground bore that boot with at the last
        // step of the physics, and LoadsFrom of its weight to find the ground with; what it cannot take yet, the
        // leg it stood on goes on taking. (Asked for its whole share at once, with no ground under it yet to push
        // back, the leg is only thrown: its knee snaps straight and its boot is driven on or flung up.) Loads 0:
        // its whole share at once, as it was until October 13.
        public static float Loads = 2, LoadsFrom = .25f, LoadsFor = .3f;
        // (Tried on October 13, and off: a boot just put down not lifted again for the next catch; for NotAgain
        // seconds (0.3) after it lands, the other leg takes it. In nine runs of the game's shove test ten catches
        // were begun by the boot that had landed within three tenths of a second, the one it then stood on, and
        // eight of them ended in a fall. But with the rule alone as many stand as without it (12 and 10 of 12, at
        // 0.5 and 0.7 m/s): those falls were under way already. With the two slower steps above it, 12 and 11.)
        public static float NotAgain = 0;
        // While its boot swings, that leg's joints give all they have at SwingFullAt degrees from where the boot
        // should be (never stiffer than the rule), damped by SwingDamped of what just stops them swinging.
        public static float SwingFullAt = 36.9916f, SwingDamped = 0.3692f;
        // Its boot is carried there by a pull for each kilogram of the leg: SwingStiff for each metre it is from where
        // it should be, SwingSlows for each metre a second it lags; no more than SwingAtMost times the leg's weight.
        public static float SwingStiff = 102.3059f, SwingSlows = 52.1183f, SwingAtMost = 1.4859f;
        // The hip of the leg it stands on takes up this share of what the swinging leg's hip gives; and its hips are
        // righted this many times as hard while it stands on one leg.
        public static float TakesUp = 1.1961f, StepRights = 1, SwingAnkle = 16.2657f;
        // Afterwards the other boot is brought alongside: when its boots stand more than Astride (metres) from how
        // they stood, and it has been at rest (its weight going slower than CalmUnder, metres a second) for
        // ClosesAfter seconds. That step is got ready: its weight goes onto the leg it will stand on, until the boot
        // to be lifted bears no more than LiftsUnder of its weight and the other at least LiftsOver of it (for three
        // steps of the physics: a boot that has bounced bears nothing either), for no longer than ReadiesAtMost
        // seconds. Not ready by then, it does not lift the boot: it stands as it is, tries once more when it is at
        // rest, and after that stands astride. The boot is brought no further than leaves where its weight is
        // going between its boots: short of alongside by up to ClosesShort of Astride.
        // After a catch it does not wait to be at rest: once nothing more needs catching, FollowsAfter seconds after
        // the catch's boot landed (less than nought: never), the boot behind follows, its weight still going on over
        // the boot it has put down. Pushed on while it gets ready, it leaves that and catches itself.
        // (Tried on October 13 for ready: where its weight is going within 13 mm of the middle of the sole it will
        // stand on, which astride it could not reach in a second, lifting the boot all the same; then within 13 mm
        // of that sole, which let it lift a boot bearing 45 hundredths of its weight. Both felled it.)
        public static float Astride = 0.1045f, CalmUnder = 0.0901f, ClosesAfter = 0.8898f, LiftsUnder = .12f, LiftsOver = .6f, ReadiesAtMost = 1, FollowsAfter = .1f, ClosesShort = .8f;
        // A step it has time for. A push that lasts moves the place where the ground would have to push it towards
        // the edge of what its soles can give. From how far that still is, the way it is pushed, and how fast it goes
        // there, it reckons how long it has. Under ReadiesAhead times the time it falls in (the root of its height
        // over gravity: 1.8 of those is half a second for Small, 0.57 s for Long), for three steps of the physics, it gets
        // ready: its weight goes towards the leg it will stand on, leaning against the push still, until the boot to
        // be lifted bears no more than ReadyUnder of its weight. That boot then goes where its weight will be going
        // when it lands (from how fast it falls and how long the swing takes), further by ReadyMore times what its
        // need already lies beyond the sole it stands on, and ReadyFurther metres: a short step for a small need, a
        // longer one for a greater. Out of time meanwhile, that boot goes at once. Shoved, it has no time at all,
        // and catches itself as before. So it has one step, got ready for by as much as there is time, and not one
        // answer below a line and another above it (Luis, October 13). ReadiesAhead 0: only the catch, as it was.
        // How it came to this, on October 13. First it waited until the boot to be lifted bore next to nothing: on
        // the bench (a level floor) that did what was meant, and in the game, on the meadow, it was worse than the
        // catch alone: on one leg against the pull, that one ankle was asked for what two shared (0.97 of what it
        // has), its boot tipped over its toe (28 degrees) and was thrown. So the boot is lifted while it still bears
        // ReadyUnder of its weight (for three steps of the physics): the getting ready and the step overlap, as a
        // person's do. Out of time before that, the boot it was getting ready goes at once, as that step (dropped
        // for a catch begun afresh, Long went down). And its boot goes ReadyMore times as far again as its need
        // already lies beyond the sole it stands on: a short step when it was ready in time, one as long as a catch
        // when it was not (reckoned from the fall alone it was 6 cm shorter than a catch for the same need, and
        // under a tenth of their weight Small and Long went down).
        // In the game, on the meadow, life in the miners, pulled at the chest for 2.5 s: with 0.06 of its weight
        // Small and Round lean and take no step, Long takes one of 9.5 cm; with 0.08 Small's steps are 8 and 12 cm
        // for 22 and 22 and its head goes 14 cm for 30, Round's 11 and 13 cm, and Long, out of time, 25 and 28; with
        // 0.10, 7 of 9 stand with it and 7 of 9 without, in more steps with it. Shoved, it never comes to it.
        // OFF AGAIN, the night of October 13, BECAUSE OF LONG. For Long the gentle pull is already more than its
        // feet can lean against (it is pulled higher up, and its soles are shorter for its height), so it is always
        // out of time; and half ready and out of time it catches itself worse than from standing square. Long alone,
        // pulled with 0.08, four runs each: with the rule 3 stand, in five to seven steps, its head going 43 to 45
        // cm; without it 4 stand, in two steps, 31 to 34 cm. (Its boot going where a catch would, once out of time:
        // 0 of 4, taken out.) It is right for Small and Round and wrong for Long, and stays off until it is right
        // for all three.
        // (Getting ready for it, where it holds its weight goes over ReadiesQuick times as fast as when it only
        // leans.)
        // (And it is a step: its boot goes at least ReadyAtLeast of its own sole's length. Pulled to its side, Small
        // had lifted a boot and put it down 10 mm away.)
        public static float ReadiesAhead = 0, ReadyFurther = .02f, ReadiesQuick = 3, ReadyUnder = .4f, ReadyMore = 1.6f, ReadyAtLeast = .5f;
        // (ReadiesNeeds: begun only while it still has so much time, in the time it falls in. Off at nought: what it
        // reckons it has when it begins does not tell who will be ready in time. Round began with 0.8 and was;
        // Long with 1.05 and was not; Small with 1.5 and was.)
        public static float ReadiesNeeds = 0;

        private readonly float[] lighter;
        private readonly Quaternion[] rested = new Quaternion[Count], madeTurned = new Quaternion[Count], madeLay = new Quaternion[Count];
        private readonly float[] stoodSpring = new float[Count], stoodDamper = new float[Count], thighLong = new float[2], shinLong = new float[2], bears = new float[2];
        private Vector2 madeApart;
        private int swings = -1, stoodLast = -1, closesTried, lightFor, landed = -1, hasFor;
        private bool prepared, hadNeeds;
        private Vector3 needsBefore;
        private float needsRate, hasNow;
        private bool followDue;
        private bool catching, readying, stiff;
        private float stepT, beyondFor, calmFor, readyFor, sinceLanded = 1e9f;
        private Vector3 stepFrom, stepTo, carries, wantedBefore;
        private int carrying = -1;
        private bool carried;

        // How many steps it has taken; whether a boot is off the ground or about to be; and which (-1: neither).
        public int Stepped { get; private set; }
        // Where it stands (the middle between its soles) and which way its hips face: for whoever moves its place
        // with it. And whether its legs are held as a catching step left them (its boots astride, until the other
        // boot has been brought alongside): whoever asks its joints for anything leaves its legs alone then.
        public Vector3 StandsAt => .5f * (sole[0] + sole[1]);
        public Vector3 Faces => Flat(parts[Hips].transform.forward).normalized;
        public bool LegsHeld { get; private set; }
        // How high the ground is at a place, by whoever knows the ground (NaN: none it can stand on). With it, a
        // boot is put only where there is ground within StepsOnLevel of where it stands. Without it (a level
        // floor), anywhere.
        public System.Func<Vector3, float> GroundUnder;
        // (For whoever looks at why it does or does not step: what it does about a step, in words.)
        public System.Action<string> Tells;
        // (What body it keeps, in figures: to set one made one way beside one made another.)
        public string Describe()
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            float all = 0; foreach (float m in masses) all += m;
            Vector3 apart = parts[Foot + 1].transform.TransformPoint(soleAt[1]) - parts[Foot].transform.TransformPoint(soleAt[0]);
            return string.Format(c, "{0:0.0} kg (hips {1:0.0}, trunk {2:0.0}, head {3:0.0}, arms {4:0.0} {5:0.0}, thigh {6:0.0}, shin {7:0.0}, boot {8:0.0}); soles {9:0.000} x {10:0.000} x {11:0.000}, {12:0.000} m apart; thigh {13:0.000} m, shin {14:0.000} m; its weight {15:0.000} m up; has: thigh {16:0}, shin {17:0}, boot {18:0}, trunk {19:0}; springs: thigh {20:0}, shin {21:0}, boot {22:0}, trunk {23:0}; lighter: thigh {24:0.000}, shin {25:0.000}, boot {26:0.000}",
                all, masses[Hips], masses[Trunk], masses[Head], masses[UpperArm], masses[Forearm], masses[Thigh], masses[Shin], masses[Foot], soleSize[0].x, soleSize[0].y, soleSize[0].z, Flat(apart).magnitude,
                thighLong[0], shinLong[0], height, strength[Thigh], strength[Shin], strength[Foot], strength[Trunk], stoodSpring[Thigh], stoodSpring[Shin], stoodSpring[Foot], stoodSpring[Trunk], lighter[Thigh], lighter[Shin], lighter[Foot]);
        }
        // (And what it reckons at this step of the physics, along a way and across it from a place, in millimetres:
        // to set a body in the game beside one on the bench, step by step.)
        public string Tick(Vector3 way, Vector3 from)
        {
            var c = System.Globalization.CultureInfo.InvariantCulture;
            Vector3 side = Vector3.Cross(Vector3.up, way);
            float A(Vector3 v) => Vector3.Dot(v - from, way) * 1000; float S(Vector3 v) => Vector3.Dot(v - from, side) * 1000;
            float R(int j) => asked[j] / Mathf.Max(1e-4f, strength[j]);
            Transform hp = parts[Hips].transform;
            // (How far a part's length is turned from the length of the part it hangs from, or from upright, in degrees.)
            float Bend(int j) => Vector3.Angle(j < Shin ? Vector3.up : parts[this.from[j]].transform.up, parts[j].transform.up);
            return string.Format(c, "sw{0} st{1}{2} | along: weight {3:0} going {4:0} wants {5:0} act {6:0} bore {7:0} soles {8:0} {9:0} | aside: weight {10:0} going {11:0} wants {12:0} act {13:0} bore {14:0} soles {15:0} {16:0} | bears {17:0.00} {18:0.00} | felt {19:0.00} {20:0.00} falls {21:0.00} | asked of has: L {22:0.00} {23:0.00} {24:0.00}, R {25:0.00} {26:0.00} {27:0.00} | hips {28:0} up, tipped {29:0.0} ahead {30:0.0} aside | rests {31:0} {32:0}, soles up {33:0} {34:0} | knees bent {35:0} {36:0} deg, thighs from upright {37:0} {38:0} deg, shins from upright {39:0} {40:0} deg, trunk {41:0.0} ahead, boots tipped {42:0} {43:0}, weight up {44:0}, sinks {45:0}",
                swings, Stepped, readying ? " readying" : "",
                A(weightBefore), A(GoingAt), A(WantedAt), A(acted), A(realAt), A(sole[0]), A(sole[1]),
                S(weightBefore), S(GoingAt), S(WantedAt), S(acted), S(realAt), S(sole[0]), S(sole[1]),
                bears[0], bears[1], Vector3.Dot(felt, way), Vector3.Dot(felt, side), FallsAt,
                R(Thigh), R(Shin), R(Foot), R(Thigh + 1), R(Shin + 1), R(Foot + 1),
                (hp.position.y - from.y) * 1000, Mathf.Asin(Mathf.Clamp(Vector3.Dot(hp.up, way), -1, 1)) * Mathf.Rad2Deg, Mathf.Asin(Mathf.Clamp(Vector3.Dot(hp.up, side), -1, 1)) * Mathf.Rad2Deg,
                A(rest), S(rest), (sole[0].y - from.y) * 1000, (sole[1].y - from.y) * 1000,
                Bend(Shin), Bend(Shin + 1), Bend(Thigh), Bend(Thigh + 1), Vector3.Angle(Vector3.up, parts[Shin].transform.up), Vector3.Angle(Vector3.up, parts[Shin + 1].transform.up),
                Mathf.Asin(Mathf.Clamp(Vector3.Dot(parts[Trunk].transform.up, way), -1, 1)) * Mathf.Rad2Deg, Vector3.Angle(Vector3.up, parts[Foot].transform.up), Vector3.Angle(Vector3.up, parts[Foot + 1].transform.up),
                (weightBefore.y - .5f * (sole[0].y + sole[1].y)) * 1000, sinksBy * 1000);
        }
        private const float StepsOnLevel = .05f;
        public bool Stepping => swings >= 0;
        public int Swings => swings;
        public Vector3 SwingWants, SwingIs, SwingTo, SwingFrom;

        // What whoever owns the body says: its life, lived in the middle of each step (the step's length; whether
        // it is being pushed; how its boots stand apart); how far from where it stands its weight is held; how its
        // hips roll from upright; and how far towards the middle of its boots it holds its weight (0: where the
        // posed body had it, near its heels).
        public System.Action<float, bool, Vector3> Life;
        public Vector3 Shift;
        public Quaternion Roll = Quaternion.identity;
        public float StandsMid;

        // What a joint's worked-out torque was in the last step (newton metres); how far outside its soles the
        // push it wanted lay (metres); how far it leans against what it feels, and what it feels (m/s2); how its
        // hips were turned when it began.
        public float[] Gave => gave;
        public float Outside { get; private set; }
        public Vector3 Leaning => holds - rest;
        public Vector3 Felt => felt;
        // (For whoever traces it: where its weight is, where it wanted the ground to push it at the last step, where
        // on its soles that push could act, where the ground really bore it, where it holds its weight, the middle
        // of each sole, and the rate it falls at.)
        public Vector3 WeightAt => weightBefore;
        public Vector3 WantedAt { get; private set; }
        public Vector3 GoingAt { get; private set; }
        public Vector3 ActedAt => acted;
        public Vector3 BoreAt => realAt;
        public Vector3 HoldsAt => holds;
        public Vector3 SoleAt(int i) => sole[i];
        public float FallsAt { get; private set; }
        // (And: what each joint was asked for at the last step, before it was held to what it has; what it has;
        // what pushed it as last reckoned, before that is smoothed; and how hard the ground bore each boot.)
        public float[] Asked => asked;
        public float Has(int j) => strength[j];
        public Vector3 SeenLast { get; private set; }
        public float Bore(int i) => bears[i];
        private readonly float[] asked = new float[Count];
        public Quaternion Upright => upright;
        public float Whole => whole;

        public OwnKeeper(ArticulationBody[] parts, int[] from, float[] masses, float[] strength, bool[] hinge, Vector3[] anchors, GroundTouch[] touches, Vector3[] soleAt, Vector3[] soleSize, float[] lighter = null)
        {
            this.parts = parts; this.from = from; this.masses = masses; this.strength = strength; this.hinge = hinge; this.anchors = anchors;
            this.touches = touches; this.soleAt = soleAt; this.soleSize = soleSize;
            float all = 0;
            for (int i = 0; i < Count; i++) all += masses[i];
            whole = all;
            // (For the step: how each part was turned, how each joint was sprung, and how its legs lay, as it was made.
            // The turning weight of the lighter side of each joint is whoever built it's to give; without it, no step.)
            this.lighter = lighter;
            Vector3 ahead = parts[Hips].transform.forward;
            for (int j = 1; j < Count; j++)
            {
                madeTurned[j] = parts[j].transform.rotation;
                rested[j] = Quaternion.Inverse(parts[from[j]].transform.rotation) * parts[j].transform.rotation;
                stoodSpring[j] = parts[j].xDrive.stiffness; stoodDamper[j] = parts[j].xDrive.damping;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector3 hip = JointAt(Thigh + i), knee = JointAt(Shin + i), ankle = JointAt(Foot + i);
                thighLong[i] = (knee - hip).magnitude; shinLong[i] = (ankle - knee).magnitude;
                Vector3 thighWay = (knee - hip).normalized, shinWay = (ankle - knee).normalized;
                madeLay[Thigh + i] = Quaternion.LookRotation(thighWay, Vector3.ProjectOnPlane(ahead, thighWay).normalized);
                madeLay[Shin + i] = Quaternion.LookRotation(shinWay, Vector3.ProjectOnPlane(ahead, shinWay).normalized);
            }
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        private Vector3 Weight()
        {
            Vector3 c = Vector3.zero;
            for (int i = 0; i < Count; i++) c += parts[i].worldCenterOfMass * masses[i];
            return c / whole;
        }

        private Vector3 WeightGoes()
        {
            Vector3 v = Vector3.zero;
            for (int i = 0; i < Count; i++) v += parts[i].GetPointVelocity(parts[i].worldCenterOfMass) * masses[i];
            return v / whole;
        }

        private Vector3 JointAt(int j) => parts[j].transform.TransformPoint(anchors[j]);

        // A joint is asked for a turn from the pose it was made in (the turn in the part's own frame).
        private void Aims(int j, Quaternion own)
        {
            own.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.right; angle = 0; }
            Vector3 by = angle * axis;
            parts[j].SetDriveTarget(ArticulationDriveAxis.X, by.x);
            if (hinge[j]) return;
            parts[j].SetDriveTarget(ArticulationDriveAxis.Y, by.y);
            parts[j].SetDriveTarget(ArticulationDriveAxis.Z, by.z);
        }

        // A leg's joints are sprung as they stand, or (while its boot swings) stiffer: all they have at SwingFullAt
        // degrees from where the boot should be, but never stiffer than the rule.
        private void Springs(int leg, bool swinging, float step)
        {
            for (int j = (SwingHip > .5f ? Thigh : Shin) + leg; j <= Foot + leg; j += 2)
            {
                float spring = stoodSpring[j], damper = stoodDamper[j];
                if (swinging)
                {
                    // (The rule is for a boot on the ground: in the air its ankle may be stiffer.)
                    spring = Mathf.Min(strength[j] / (SwingFullAt * Mathf.Deg2Rad), (j >= Foot ? SwingAnkle : 1) * Rule * Rule / (step * step) * lighter[j]);
                    damper = SwingDamped * 2 * Mathf.Sqrt(spring * lighter[j]);
                }
                var drive = parts[j].xDrive; drive.stiffness = spring; drive.damping = damper; parts[j].xDrive = drive;
                if (hinge[j]) continue;
                drive = parts[j].yDrive; drive.stiffness = spring; drive.damping = damper; parts[j].yDrive = drive;
                drive = parts[j].zDrive; drive.stiffness = spring; drive.damping = damper; parts[j].zDrive = drive;
            }
        }

        // Where on its soles a push wanted at a place can act: each boot bears its share along its own middle line
        // (only a push wanted outside both is taken to a boot's outer side); standing on one boot, anywhere on that
        // sole. The share its right boot bears is by how far across the push lies, or what a step says.
        private Vector3 Borne(Vector3 acts, int single, int stance, float unloaded, out float share)
        {
            Vector3 over = Flat(sole[1] - sole[0]);
            Vector3 beside = over.sqrMagnitude > 1e-8f ? over.normalized : Vector3.right;
            for (int i = 0; i < 2; i++)
            {
                Transform foot = parts[Foot + i].transform;
                float across = Vector3.Dot(Flat(acts - sole[i]), beside);
                float outward = i == 0 ? Mathf.Min(across, 0) : Mathf.Max(across, 0);
                Vector3 local = foot.InverseTransformPoint(single == i ? acts : acts - beside * (across - outward));
                Vector3 half = .5f * soleSize[i];
                local = new Vector3(Mathf.Clamp(local.x, -half.x + Edge, half.x - Edge), -half.y, Mathf.Clamp(local.z, -half.z + Edge, half.z - Edge));
                at[i] = foot.TransformPoint(local);
            }
            share = Mathf.Clamp01(Vector3.Dot(Flat(acts - sole[0]), over) / Mathf.Max(1e-6f, over.sqrMagnitude));
            if (stance >= 0) share = Mathf.Lerp(share, stance, unloaded);
            return Vector3.Lerp(at[0], at[1], share);
        }

        // One step of keeping itself up. (False: it is down.)
        public bool Keep(float step)
        {
            float g = Physics.gravity.magnitude;
            Vector3 weight = Weight(), goes = WeightGoes();
            // Where the ground really bore it in the last step.
            {
                Vector3 bore = Vector3.zero; float lifted = 0;
                for (int i = 0; i < 2; i++) { bore += touches[i].Where; lifted += touches[i].Up; bears[i] = touches[i].Up; touches[i].Clear(); }
                realKnown = lifted > 1e-6f;
                if (realKnown) realAt = bore / lifted;
            }
            for (int i = 0; i < 2; i++) sole[i] = parts[Foot + i].transform.TransformPoint(soleAt[i]);
            // (While a boot swings, the ground is where the other stands.)
            bool inAir = swings >= 0 && !readying && stepT >= UnloadsIn;
            float ground = inAir ? sole[1 - swings].y : .5f * (sole[0].y + sole[1].y), high = weight.y - ground;
            if (!begun)
            {
                begun = true; height = high; upright = parts[Hips].transform.rotation;
                rest = Flat(weight); holds = rest;
                Vector3 between = Flat(.5f * (sole[0] + sole[1])), beside0 = Flat(sole[1] - sole[0]).normalized;
                standsBy = new Vector2(Vector3.Dot(rest - between, beside0), (1 - StandsMid) * Vector3.Dot(rest - between, Vector3.Cross(beside0, Vector3.up)));
                Vector3 ahead0 = Flat(upright * Vector3.forward).normalized, across0 = Vector3.Cross(Vector3.up, ahead0);
                madeApart = new Vector2(Vector3.Dot(sole[1] - sole[0], across0), Vector3.Dot(sole[1] - sole[0], ahead0));
            }
            // Down: it goes over into the fall that is built. (With the step on, by its weight's height over its lower
            // boot: a boot off the ground, trailing behind it or flung up, is not the ground. Taken from the middle of
            // its two soles, a miner with a boot 0.2 m up read 0.1 m lower than it stood, and was taken for down.)
            float highOver = Steps ? weight.y - Mathf.Min(sole[0].y, sole[1].y) : high;
            if (float.IsNaN(high) || highOver < DownAt * height || Vector3.Angle(Vector3.up, parts[Hips].transform.up) > DownLeaning)
            {
                Tells?.Invoke($"down: its weight {highOver:F3} m over its lower boot ({ground:F3} the ground it reckons from; its soles at {sole[0].y:F3} and {sole[1].y:F3}), of {height:F3} standing; its hips tipped {Vector3.Angle(Vector3.up, parts[Hips].transform.up):F0} degrees; swinging leg {swings}");
                return false;
            }

            float rise = Mathf.Clamp(Rises * (height - sinksBy - high) - RisesDamped * goes.y, -(Steps ? Mathf.Clamp(SinksFree, .1f, .95f) : .5f) * g, g);
            // A standing body falls away from where the ground pushes it, at a rate of its own (the root of gravity
            // over its height). So what matters is where its weight is going: its place, and its speed over that rate.
            float falls = Mathf.Sqrt((g + rise) / Mathf.Max(.1f, high));
            sinceLanded += step;
            if (hasBefore && Feels > 0 && sinceLanded > LandsDeaf)
            {
                // What pushes it from outside: how its weight really went, beyond what the ground's push gave it.
                Vector3 pushedAt = realKnown && Tracks > 0 ? realAt : actedBefore;
                Vector3 seen = Flat(goes - wentBefore) / step - falls * falls * Flat(weightBefore - pushedAt);
                felt = Vector3.ClampMagnitude(Vector3.Lerp(felt, seen, Mathf.Clamp01(step / Feels)), 4);
                SeenLast = seen;
            }
            if (hasBefore && realKnown && Tracks > 0) shortBy = Vector3.ClampMagnitude(shortBy + Tracks * step * Flat(actedBefore - realAt), .05f);
            // The life in it: where it holds its weight between its boots, and what its joints are asked for.
            // (Boots that slid under a push take where it stands with them; while a boot swings, where it stands is
            // left where it was.)
            Vector3 apart = Flat(sole[1] - sole[0]);
            if (swings < 0) stands = Flat(.5f * (sole[0] + sole[1])) + apart.normalized * standsBy.x + Vector3.Cross(apart.normalized, Vector3.up) * standsBy.y;
            Life?.Invoke(step, felt.magnitude / (falls * falls) > Comfort, apart);
            rest = stands + Shift;
            Vector3 going = Flat(weight) + Flat(goes) / falls;

            // ---- The step: when where its weight is going cannot be kept within its soles, a leg is unloaded, lifted,
            // and its boot put down where its weight will be going. Afterwards, at rest, the other boot is brought
            // alongside (that one is got ready: its weight goes onto the leg it will stand on first).
            int single = -1, stance = -1;
            Vector3 needsNow = going;
            bool outOfTime = false;
            // (How low it means to be: in a catching step, by what its leg needs; standing, as it stands.)
            if (swings < 0) sinkWants = 0;
            if (sinksBy != sinkWants) sinksBy = Mathf.MoveTowards(sinksBy, sinkWants, (sinkWants > sinksBy ? SinksDown : SinksAtMost * height / Mathf.Max(.05f, SinksBack)) * step);
            carrying = -1;
            float unloaded = 0;
            // (The step switched off while one is under way: the leg is let go of, as it stands.)
            if (!Steps && swings >= 0) { Springs(swings, false, step); swings = -1; catching = false; readying = false; prepared = false; sinkWants = 0; LegsHeld = false; }
            if (Steps && lighter != null)
            {
                Vector3 aheadNow = Flat(parts[Hips].transform.forward).normalized, acrossNow = Vector3.Cross(Vector3.up, aheadNow);
                if (swings < 0 || readying)
                {
                    // (Where the ground would have to push it for it to stand as it is: where its weight is going, and
                    // further for what pushes it. Can its soles bear that?)
                    Vector3 lasting = felt;
                    if (StepsFeltOver > 0)
                    {
                        feltLately[feltTold++ % feltLately.Length] = felt;
                        int back = Mathf.Clamp(Mathf.RoundToInt(StepsFeltOver / step), 1, Mathf.Min(feltTold, feltLately.Length));
                        for (int b = 1; b <= back; b++)
                        {
                            Vector3 then = feltLately[(feltTold - b) % feltLately.Length];
                            if (then.sqrMagnitude < lasting.sqrMagnitude) lasting = then;
                        }
                    }
                    Vector3 needs = going + StepsFelt * lasting / (falls * falls);
                    lastingFelt = lasting; lastingAsked = lasting.magnitude / (falls * falls);
                    needsNow = needs;
                    // (How long it has: how far that place still is from the edge of what its soles can give, the way
                    // it is pushed, over how fast it goes there.)
                    float has = float.PositiveInfinity;
                    Vector3 pushWay = Flat(lasting).sqrMagnitude > 1e-8f ? Flat(lasting).normalized : Vector3.zero;
                    if (ReadiesAhead > 0 && pushWay != Vector3.zero)
                    {
                        Vector3 edge = Borne(new Vector3(needs.x + pushWay.x, ground, needs.z + pushWay.z), -1, -1, 0, out _);
                        float room = Vector3.Dot(Flat(edge - needs), pushWay);
                        needsRate = Mathf.Lerp(needsRate, hadNeeds ? Vector3.Dot(Flat(needs - needsBefore), pushWay) / step : 0, Mathf.Clamp01(step / .1f));
                        has = room <= 0 ? 0 : needsRate > 1e-3f ? room / needsRate : float.PositiveInfinity;
                    }
                    else needsRate = 0;
                    needsBefore = needs; hadNeeds = true;
                    // (Counted in the time this body falls in: a taller one needs longer to get its weight over.)
                    hasFor = lastingAsked > Comfort && has * falls < ReadiesAhead && has * falls > ReadiesNeeds ? hasFor + 1 : 0;
                    hasNow = has * falls;
                    Vector3 kept = Borne(new Vector3(needs.x, ground, needs.z), -1, -1, 0, out float bearing);
                    float beyond = Flat(needs - kept).magnitude;
                    beyondFor = beyond > StepsBeyond ? beyondFor + step : 0;
                    // (Out of time while it gets ready for a step it had time for: the boot it was getting ready goes at
                    // once, as that step.)
                    if (readying && prepared && beyondFor >= StepsAfter)
                    {
                        Tells?.Invoke($"out of time getting leg {swings} ready: its boot goes at once; it bears {bears[0]:F2} and {bears[1]:F2}");
                        outOfTime = true; beyondFor = 0;
                    }
                    // (Pushed on while it gets ready for a boot to come alongside: it leaves that, and catches itself.)
                    else if (readying && beyondFor >= StepsAfter)
                    {
                        Tells?.Invoke($"pushed on while leg {swings} got ready: it leaves that");
                        swings = -1; readying = false; prepared = false; Stepped--;
                    }
                    if (swings < 0)
                    {
                        Vector3 madeNow = acrossNow * madeApart.x + aheadNow * madeApart.y;
                        bool calm = beyond <= 1e-4f && Flat(goes).magnitude < CalmUnder && felt.magnitude / (falls * falls) < Comfort;
                        bool astride = (apart - madeNow).magnitude > Astride;
                        calmFor = calm && astride ? calmFor + step : 0;
                        // (After a catch the boot behind follows at once, its weight still going on over the boot put down.)
                        bool follows = FollowsAfter >= 0 && followDue && astride && beyond <= StepsBeyond && sinceLanded >= FollowsAfter;
                        bool mustCatch = beyondFor >= StepsAfter, hasTime = !mustCatch && ReadiesAhead > 0 && hasFor >= 3 && sinceLanded > LoadsFor;
                        if (mustCatch || hasTime)
                        {
                            // A catch, or a step it has time for. Sideways, the leg on the side it falls to; else the
                            // leg that bears less.
                            Vector3 way = mustCatch ? Flat(needs - kept).normalized : pushWay;
                            // (With the boot behind stepping, sideways is across its body: astride, the line between
                            // its boots is not.)
                            float aside = Vector3.Dot(way, TrailsFrom > 0 ? acrossNow : apart.normalized);
                            swings = Mathf.Abs(aside) > Sideways ? (aside > 0 ? 1 : 0) : (bearing > .5f ? 0 : 1);
                            float leads = Vector3.Dot(apart, way);
                            if (TrailsFrom > 0 && Mathf.Abs(aside) <= Sideways && Mathf.Abs(leads) > TrailsFrom * Mathf.Abs(madeApart.x)) swings = leads > 0 ? 0 : 1;
                            // (Not the boot it has just put down.)
                            if (NotAgain > 0 && landed == swings && sinceLanded < NotAgain) swings = 1 - swings;
                            Vector3 stood = Flat(sole[1 - swings]);
                            // (With time, where its boot goes is reckoned when it is ready.)
                            stepTo = mustCatch ? stood + (needs - stood) * StepsAhead + way * StepsFurther : needs + way * ReadyFurther;
                            catching = true; readying = hasTime; prepared = hasTime; hasFor = 0;
                            if (hasTime) { readyFor = 0; lightFor = 0; }
                            // (With time, and the boot that would step already about where its weight is going, as the
                            // boot on that side is when it is pulled to its side: no step is begun.)
                            if (hasTime && Vector3.Dot(stepTo - Flat(sole[swings]), way) < .5f * ReadyAtLeast * soleSize[swings].z) { swings = -1; catching = false; readying = false; prepared = false; }
                        }
                        else if (follows || (calmFor >= ClosesAfter && closesTried < 2))
                        {
                            // The other boot is brought alongside: the leg that stood through the last step.
                            swings = stoodLast >= 0 ? stoodLast : 0;
                            stepTo = Flat(sole[1 - swings]) + (swings == 1 ? madeNow : -madeNow);
                            catching = false; readying = true; readyFor = 0; lightFor = 0; followDue = false; closesTried++;
                        }
                        if (swings >= 0)
                        {
                            // (Its boots are not put across each other, nor further than the leg reaches.)
                            Vector3 own = swings == 1 ? acrossNow : -acrossNow, stood = Flat(sole[1 - swings]);
                            float clear = Vector3.Dot(stepTo - stood, own), least = .6f * Mathf.Abs(madeApart.x);
                            if (clear < least) stepTo += own * (least - clear);
                            Vector3 hipOver = Flat(JointAt(Thigh + swings));
                            stepTo = hipOver + Vector3.ClampMagnitude(stepTo - hipOver, Reach * (thighLong[swings] + shinLong[swings]));
                            // (Where its boot is to land must be ground it can stand on, level with where it stands: else
                            // it takes no step, and stands or falls as it can.)
                            if (GroundUnder != null)
                            {
                                // (The ground there against the ground under the boot it stands on: not against that boot,
                                // which may be tipped or coming down.)
                                float there = GroundUnder(new Vector3(stepTo.x, sole[1 - swings].y, stepTo.z)), here = GroundUnder(sole[1 - swings]);
                                if (float.IsNaN(there) || float.IsNaN(here) || Mathf.Abs(there - here) > StepsOnLevel)
                                {
                                    Tells?.Invoke($"no step by leg {swings}: the ground where its boot would go is {there:F3}, where it stands {here:F3}");
                                    swings = -1; catching = false; readying = false; beyondFor = 0; calmFor = 0;
                                }
                            }
                            if (swings >= 0)
                            {
                                stepFrom = Flat(sole[swings]);
                                stepT = 0; stiff = false; beyondFor = 0; calmFor = 0; carried = false;
                                leftGround = Mathf.Min(sole[0].y, sole[1].y); putDown = false;
                                Stepped++;
                                Tells?.Invoke($"step {Stepped} by leg {swings} ({(prepared ? $"it has time for, {hasNow:F2} of its falling times" : catching ? "a catch" : "alongside")}): its boot to go {Flat(stepTo - stepFrom).magnitude * 1000:F0} mm; it bears {bears[0]:F2} and {bears[1]:F2}");
                            }
                        }
                    }
                }
                if (swings >= 0)
                {
                    stance = 1 - swings;
                    if (readying)
                    {
                        // Got ready: its weight onto the leg it will stand on.
                        rest = Flat(sole[stance]);
                        readyFor += step;
                        // (Ready: the boot to be lifted bears next to nothing and the other most of its weight, and
                        // have for three steps of the physics.)
                        float bearsOf = bears[swings] / (whole * g * step);
                        lightFor = (prepared ? bearsOf < ReadyUnder : bearsOf < LiftsUnder && bears[stance] > LiftsOver * whole * g * step) ? lightFor + 1 : 0;
                        if (lightFor >= 3 || outOfTime)
                        {
                            readying = false;
                            Vector3 lead = Flat(sole[stance]), line = Flat(stepTo - sole[swings]);
                            if (catching)
                            {
                                // (A step it had time for. Its boot goes where its weight will be going when it lands:
                                // on one boot the ground can push it no further on than that sole reaches, and what
                                // lies beyond grows as it falls, for as long as the swing takes.)
                                float swingFor = UnloadsIn + SwingsIn * (SwingsLeg > 0 ? Mathf.Sqrt((thighLong[swings] + shinLong[swings]) / SwingsLeg) : 1);
                                Vector3 onSole = Flat(Borne(new Vector3(needsNow.x, ground, needsNow.z), stance, stance, 1, out _));
                                Vector3 beyondSole = Flat(needsNow) - onSole;
                                Vector3 wayNow = beyondSole.sqrMagnitude > 1e-8f ? beyondSole.normalized : line.normalized;
                                stepTo = onSole + beyondSole * (Mathf.Exp(falls * swingFor) + ReadyMore) + wayNow * ReadyFurther;
                                // (It is a step: no shorter than a share of its own boot.)
                                Vector3 goesTo = stepTo - Flat(sole[swings]);
                                float atLeast = ReadyAtLeast * soleSize[swings].z;
                                if (goesTo.magnitude < atLeast) stepTo = Flat(sole[swings]) + (goesTo.sqrMagnitude > 1e-8f ? goesTo.normalized : wayNow) * atLeast;
                                // (Its boots are not put across each other, nor further than the leg reaches.)
                                Vector3 own = swings == 1 ? acrossNow : -acrossNow;
                                float clear = Vector3.Dot(stepTo - lead, own), least = .6f * Mathf.Abs(madeApart.x);
                                if (clear < least) stepTo += own * (least - clear);
                                Vector3 hipOver = Flat(JointAt(Thigh + swings));
                                stepTo = hipOver + Vector3.ClampMagnitude(stepTo - hipOver, Reach * (thighLong[swings] + shinLong[swings]));
                                stepFrom = Flat(sole[swings]);
                                Tells?.Invoke($"its boot goes {Flat(stepTo - stepFrom).magnitude * 1000:F0} mm: its weight will be going {beyondSole.magnitude * Mathf.Exp(falls * swingFor) * 1000:F0} mm beyond the sole it stands on");
                            }
                            // (Brought no further than leaves where its weight is going between its boots.)
                            else if (ClosesShort > 0 && line.magnitude > 1e-3f)
                            {
                                Vector3 apartThen = (swings == 1 ? stepTo - lead : lead - stepTo).normalized;
                                Vector3 restThen = .5f * (lead + stepTo) + apartThen * standsBy.x + Vector3.Cross(apartThen, Vector3.up) * standsBy.y;
                                float shortBy = Mathf.Clamp(2 * Vector3.Dot(Flat(restThen - going), line.normalized), 0, Mathf.Min(line.magnitude, ClosesShort * Astride));
                                stepTo -= line.normalized * shortBy;
                                if (shortBy > 1e-3f) Tells?.Invoke($"its boot is brought {shortBy * 1000:F0} mm short of alongside: its weight has not come further");
                            }
                            Tells?.Invoke($"ready for leg {swings} to step in {readyFor:F2} s; it bears {bears[0]:F2} and {bears[1]:F2}");
                        }
                        else if (readyFor > ReadiesAtMost)
                        {
                            // (Not ready in time, it does not lift the boot: it stands as it is.)
                            Tells?.Invoke($"not ready for leg {swings} to step in {readyFor:F2} s: its boot still bears {bearsOf:F2} of its weight; it stands as it is");
                            swings = -1; stance = -1; readying = false; prepared = false; calmFor = 0; Stepped--;
                        }
                    }
                    else
                    {
                        if (!catching) rest = Flat(sole[stance]);
                        stepT += step;
                        unloaded = Mathf.Clamp01(stepT / Mathf.Max(1e-3f, UnloadsIn));
                        float swingsIn = SwingsIn * (SwingsLeg > 0 ? Mathf.Sqrt((thighLong[swings] + shinLong[swings]) / SwingsLeg) : 1) * (catching ? 1 : Mathf.Max(1, ClosesSlower));
                        float u = (stepT - UnloadsIn) / Mathf.Max(.05f, swingsIn);
                        if (u >= 0)
                        {
                            single = stance;
                            if (!stiff) { stiff = true; Springs(swings, true, step); }
                            // Where its boot should be now: along the ground in one hump of speed, lifted in the middle.
                            float uu = Mathf.Clamp01(u), along = uu * uu * uu * (10 + uu * (6 * uu - 15));
                            Vector3 soleWants = Vector3.Lerp(stepFrom, stepTo, along);
                            soleWants.y = sole[stance].y + Clears * Mathf.Sin(Mathf.PI * uu);
                            if (Sinks > 0 && catching)
                            {
                                // How low its hips must be for this leg to reach the ground where its boot is to land, from
                                // where that hip will be by then (it goes on as its weight goes).
                                Vector3 hipNow = JointAt(Thigh + swings);
                                Vector3 hipThen = Flat(hipNow) + Flat(goes) * (Mathf.Clamp01(1 - u) * swingsIn);
                                float ankleUp = Mathf.Abs(parts[Foot + swings].transform.TransformVector(anchors[Foot + swings] - soleAt[swings]).y);
                                float legLong = .97f * (thighLong[swings] + shinLong[swings]), away = Flat(stepTo - hipThen).magnitude;
                                float hipMay = ankleUp + Mathf.Sqrt(Mathf.Max(0, legLong * legLong - away * away));
                                sinkWants = Mathf.Max(sinkWants, Mathf.Min(SinksAtMost * height, Sinks * Mathf.Max(0, hipNow.y + sinksBy - leftGround - hipMay)));
                            }
                            if (u > 1)
                            {
                                // (Put straight down from where it is: the pull on it begins afresh.)
                                if (!putDown) { putDown = true; carried = false; }
                                soleWants = new Vector3(sole[swings].x, leftGround - .03f, sole[swings].z);
                            }
                            SwingWants = soleWants; SwingIs = sole[swings]; SwingTo = stepTo; SwingFrom = stepFrom;
                            // What carries its boot there: a pull on the boot towards where it should be, and against how
                            // it lags, and the leg's own weight; given by its hip and knee.
                            {
                                float leg = masses[Thigh + swings] + masses[Shin + swings] + masses[Foot + swings];
                                Vector3 goesNow = parts[Foot + swings].GetPointVelocity(sole[swings]);
                                Vector3 shouldGo = carried ? (soleWants - wantedBefore) / step : goesNow;
                                carries = Vector3.ClampMagnitude(leg * (SwingStiff * (soleWants - sole[swings]) + SwingSlows * (shouldGo - goesNow)), SwingAtMost * leg * g) + leg * g * Vector3.up;
                                carrying = swings; carried = true; wantedBefore = soleWants;
                            }
                            Quaternion faces = Quaternion.AngleAxis(Vector3.SignedAngle(Flat(upright * Vector3.forward), aheadNow, Vector3.up), Vector3.up);
                            Quaternion bootWants = faces * madeTurned[Foot + swings];
                            // Its hip and knee, for its ankle to be there: its knee bends forward.
                            Vector3 hip = JointAt(Thigh + swings), reach = soleWants + bootWants * (anchors[Foot + swings] - soleAt[swings]) - hip;
                            float l1 = thighLong[swings], l2 = shinLong[swings];
                            float far = Mathf.Clamp(reach.magnitude, Mathf.Abs(l1 - l2) + .01f, (l1 + l2) * .995f);
                            Vector3 line = reach.normalized, pole = Vector3.ProjectOnPlane(parts[Hips].transform.forward, line).normalized;
                            float opens = Mathf.Acos(Mathf.Clamp((l1 * l1 + far * far - l2 * l2) / (2 * l1 * far), -1, 1));
                            Vector3 thighWay = line * Mathf.Cos(opens) + pole * Mathf.Sin(opens);
                            Vector3 shinWay = (line * far - thighWay * l1).normalized;
                            Quaternion thighWants = Quaternion.LookRotation(thighWay, pole) * Quaternion.Inverse(madeLay[Thigh + swings]) * madeTurned[Thigh + swings];
                            Quaternion shinWants = Quaternion.LookRotation(shinWay, Vector3.ProjectOnPlane(parts[Hips].transform.forward, shinWay).normalized) * Quaternion.Inverse(madeLay[Shin + swings]) * madeTurned[Shin + swings];
                            Aims(Thigh + swings, Quaternion.Inverse(parts[Hips].transform.rotation * rested[Thigh + swings]) * thighWants);
                            Aims(Shin + swings, Quaternion.Inverse(thighWants * rested[Shin + swings]) * shinWants);
                            Aims(Foot + swings, Quaternion.Inverse(parts[Shin + swings].transform.rotation * rested[Foot + swings]) * bootWants);
                            // It lands: at the end of its swing, or sooner if its boot meets the ground on the way down.
                            if (u >= LandsBy || (u > .55f && bears[swings] > 1e-4f))
                            {
                                Springs(swings, false, step);
                                if (!catching)
                                    for (int i = 0; i < 2; i++)
                                        for (int j = Thigh + i; j <= Foot + i; j += 2) Aims(j, Quaternion.identity);
                                stoodLast = stance; LegsHeld = catching; followDue = catching; landed = swings; prepared = false;
                                if (catching) closesTried = 0;
                                Tells?.Invoke($"landed, leg {swings}: {Flat(sole[swings] - stepTo).magnitude * 1000:F0} mm from where it was to go, bearing {bears[swings]:F2}, {stepT:F2} s after it began");
                                swings = -1; single = -1; stance = -1; unloaded = 0; carrying = -1; carried = false;
                                holds = going; sinceLanded = 0;
                            }
                        }
                    }
                }
            }

            if (Steps && LeansBack > 0)
            {
                float leans = Mathf.Min(LeansBackAtMost, LeansBack * Mathf.Max(0, lastingAsked - Comfort) / .1f);
                leansNow = Mathf.MoveTowards(leansNow, leans, 90 * step);
                Vector3 from0 = Flat(lastingFelt);
                if (leansNow > 1e-3f && from0.sqrMagnitude > 1e-6f)
                {
                    // (Its top goes the way against the push, about the line across that way.)
                    Quaternion back = Quaternion.AngleAxis(leansNow, Vector3.Cross(Vector3.up, -from0.normalized));
                    Quaternion made = parts[Hips].transform.rotation * rested[Trunk];
                    Aims(Trunk, Quaternion.Inverse(made) * back * made);
                }
                else if (leansNow <= 1e-3f && leaned) Aims(Trunk, Quaternion.identity);
                leaned = leansNow > 1e-3f;
            }
            // Where it wants the ground to push it: beyond where its weight is going, by how far that is from where it
            // holds it (catching itself on one leg, as near where its weight is going as its sole reaches).
            // (Written in this order so that, with no step, it reckons exactly as it did.)
            // (Getting ready for a step it has time for, it still holds its weight: it is not yet on one leg.)
            Vector3 wants = swings >= 0 && catching && !readying ? going + felt / (falls * falls) : going + Quick / falls * (going - holds) + felt / (falls * falls);
            // (Its answer to its hips tipping is part of the step: with the step off, it stands exactly as it did.)
            if (Steps && (Tilts > 0 || TiltsRate > 0))
            {
                // (How its hips are tipped from how they should be, as a turn; the way their top has gone is that
                // turn across upright.)
                Quaternion tipped = parts[Hips].transform.rotation * Quaternion.Inverse(Roll * upright);
                tipped.ToAngleAxis(out float tip, out Vector3 about);
                if (tip > 180) tip -= 360;
                if (!float.IsNaN(about.x) && !float.IsInfinity(about.x))
                    wants += high * (Tilts * Flat(Vector3.Cross(about * (tip * Mathf.Deg2Rad), Vector3.up)) + TiltsRate / falls * Flat(Vector3.Cross(parts[Hips].angularVelocity, Vector3.up)));
            }
            Vector3 acts = new Vector3(wants.x, ground, wants.z);
            WantedAt = acts; FallsAt = falls; GoingAt = going;
            acted = Borne(acts, single, stance, unloaded, out float share);
            Outside = Flat(acts - acted).magnitude;
            // The push can only act where the soles are: it pushes sideways only as a push from there, through its
            // weight, does.
            Vector3 sideways = Flat(weight - acted) * ((g + rise) / Mathf.Max(.1f, high));
            Vector3 push = whole * (sideways + Vector3.up * (g + rise));

            // It leans against what it feels, for what it cannot take standing as it is, keeping sole behind its weight.
            {
                Vector3 asked = felt / (falls * falls);
                Vector3 leansTo = rest;
                if (asked.magnitude > Comfort && (swings < 0 || (readying && prepared)))
                {
                    Vector3 way = -asked.normalized;
                    float reach = 0;
                    for (int i = 0; i < 2; i++)
                        for (int c = 0; c < 4; c++)
                        {
                            Vector3 corner = parts[Foot + i].transform.TransformPoint(new Vector3((c & 1) == 0 ? -.5f * soleSize[i].x : .5f * soleSize[i].x, -.5f * soleSize[i].y, (c & 2) == 0 ? -.5f * soleSize[i].z : .5f * soleSize[i].z));
                            reach = Mathf.Max(reach, Vector3.Dot(Flat(corner) - rest, way));
                        }
                    leansTo = rest + way * Mathf.Min(LeansBy * (asked.magnitude - Comfort), Mathf.Max(0, reach - KeepsBehind));
                }
                holds = Vector3.Lerp(holds, leansTo, Mathf.Clamp01(Leans * (readying && prepared ? Mathf.Max(1, ReadiesQuick) : 1) * step));
            }
            wentBefore = goes; weightBefore = weight; actedBefore = acted; hasBefore = true;

            // What each leg's joints give to make its boot push the ground so.
            for (int j = 0; j < Count; j++) torque[j] = Vector3.zero;
            // (A boot just put down takes up its share as the ground comes to bear it.)
            float takes0 = 1 - share, takes1 = share;
            if (Steps && Loads > 0 && landed >= 0 && swings < 0 && sinceLanded < LoadsFor)
            {
                float most = (Loads * bears[landed] / step + LoadsFrom * whole * g) / Mathf.Max(1e-3f, push.magnitude);
                float lacks = Mathf.Max(0, (landed == 0 ? takes0 : takes1) - most);
                if (landed == 0) { takes0 -= lacks; takes1 += lacks; } else { takes1 -= lacks; takes0 += lacks; }
            }
            for (int i = 0; i < 2; i++)
            {
                Vector3 pushes = (i == 0 ? takes0 : takes1) * push;
                // (What it has learnt of the ground's push falling short moves where it means the push to act, but
                // never off the sole: a push asked for beyond a sole's edge only tips the boot.)
                Transform foot = parts[Foot + i].transform;
                Vector3 means = foot.InverseTransformPoint(at[i] + shortBy), half = .5f * soleSize[i];
                means = foot.TransformPoint(new Vector3(Mathf.Clamp(means.x, -half.x + Edge, half.x - Edge), -half.y, Mathf.Clamp(means.z, -half.z + Edge, half.z - Edge)));
                for (int j = Thigh + i; j <= Foot + i; j += 2)
                    torque[j] -= Vector3.Cross(means - JointAt(j), pushes);
                // (The leg whose boot swings: its hip and knee carry the boot.)
                if (carrying == i)
                    for (int j = Thigh + i; j <= Shin + i; j += 2) torque[j] += Vector3.Cross(sole[i] - JointAt(j), carries);
            }
            // Its hips keep its trunk as upright as it began (rolled, as it favours a leg).
            {
                Quaternion off = Roll * upright * Quaternion.Inverse(parts[Hips].transform.rotation);
                off.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180) angle -= 360;
                if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.zero; angle = 0; }
                float spring = Rights * whole * (carrying >= 0 ? StepRights : 1);
                Vector3 rights = spring * (angle * Mathf.Deg2Rad) * axis - RightsDamped * spring * parts[Hips].angularVelocity;
                torque[Thigh] -= (1 - share) * rights;
                torque[Thigh + 1] -= share * rights;
            }
            // (What the swinging leg's hip gives would turn its hips the other way: the hip of the leg it stands on
            // takes that up, into the ground.)
            if (carrying >= 0) torque[Thigh + 1 - carrying] -= TakesUp * torque[Thigh + carrying];
            for (int j = Thigh; j < Count; j++)
            {
                // (A hinge gives only about its own line; what is across it is borne by the joint itself.)
                if (hinge[j]) { Vector3 line = parts[j].transform.right; torque[j] = line * Vector3.Dot(line, torque[j]); }
                asked[j] = torque[j].magnitude;
                torque[j] = Vector3.ClampMagnitude(torque[j], strength[j]);
                if (float.IsNaN(torque[j].x) || float.IsNaN(torque[j].y) || float.IsNaN(torque[j].z)) torque[j] = Vector3.zero;
                gave[j] = torque[j].magnitude;
                // Equal and opposite, on the two parts the joint joins: nothing pushes the body from nowhere.
                parts[j].AddTorque(torque[j]);
                parts[from[j]].AddTorque(-torque[j]);
            }
            return true;
        }
    }
}
