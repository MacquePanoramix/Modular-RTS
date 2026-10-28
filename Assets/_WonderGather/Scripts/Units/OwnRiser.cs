using UnityEngine;

namespace WonderGather
{
    // S3b, step 2 (Docs/Design/TheBodysOwn.md): getting up, on the body that stands by its own joints.
    //
    // Luis, October 13: "I want the character to get up naturally with their own limbs and strength in a natural
    // manner and the character shouldn't look dead and then start moving to get up after a timer."
    //
    // The body that stands by its own joints (thirteen weighted parts, each boot on an ankle of its own) is made as
    // it lies, and from there on nothing poses it. This holds its joints towards one pose after another (a way: a
    // row of poses, each a set of turns from how the body stands), each joint with no more than it has. What moves
    // the body is its limbs and its weight on the ground. At the end of a way that brings it onto its feet (its
    // soles flat, its weight over them) the keeper that stands it takes it, and raises it by its legs (OwnKeeper,
    // rising).
    //
    // One piece of code, used by the body in the game (OwnBody) and by the bench that searches its ways.
    public sealed class OwnRiser
    {
        public const int Hips = OwnKeeper.Hips, Trunk = OwnKeeper.Trunk, Head = OwnKeeper.Head, UpperArm = OwnKeeper.UpperArm, Forearm = OwnKeeper.Forearm, Thigh = OwnKeeper.Thigh, Shin = OwnKeeper.Shin, Foot = OwnKeeper.Foot, Count = OwnKeeper.Count;

        public enum Does { Gathers, Ready, GaveUp }
        // What is asked of the body before it goes on from one pose to the next: how far its chest faces the ground
        // (1: flat on its front; -1: on its back); how high its hips are over the ground, how far they are from over
        // its feet, and how far its weight is ahead of the middle between its soles (shares of its hips' standing
        // height); how steep its shins stand and how flat its soles lie (1: as standing); how upright its trunk is
        // (1); how long it has been still (seconds); how high its head is (a share of its hips' height); and what
        // share of its weight the ground bore its boots with at the last step of the physics.
        public enum Asks { Front, Hips, Off, Ahead, Steep, Soles, Upright, Still, Head, Bears }
        public struct Asked { public Asks what; public float least, most; }
        // One pose: come to from the one before over so long (seconds), kept at least so long, given up after so
        // long. Its turns from standing, in degrees: the trunk's bow, the head's, the trunk's twist (its leading
        // side going forward); then, for the side that leads (the uppermost when it began) and for the other, an
        // arm (the shoulder forward, the arm out from the body, the elbow bent) and then a leg (the hip bent, the
        // thigh out, the knee bent, the ankle bent up).
        public const int Turns = 17;
        public sealed class Stage
        {
            public string name;
            public float over = .8f, atLeast = .8f, within = 4;
            public float[] pose = new float[Turns];
            public Asked[] through = new Asked[0];
            // If it keeps its weight over its feet in this pose: how far ahead of the middle between its soles (a
            // share of its hips' height). It does so by bowing more or less at the hips.
            public float keeps = float.NaN;
            // If a foot is put somewhere in this pose (0: the leading leg's foot; 1: the other; -1: none): its boot is
            // carried, lifted so far in the middle (metres), to lie flat on the ground so far ahead of its own hip
            // and so far out from it (shares of the leg's length), the way the body lies (from its hips towards its
            // head). That leg's hip, knee and ankle are then asked for what puts the boot there, whatever the ground
            // and however the body lies, and not for the pose's turns. The pose is through only once that boot
            // bears on the ground.
            public int steps = -1;
            public float stepAhead, stepOut, stepLifts = .06f;
            // (stepUnder above nothing: its boot goes forward by way of under its hip, so high there (a share of its
            // hip's height over the ground), the way a leg is brought forward from kneeling: its knee comes forward
            // under its chest and its foot passes under it, not over the ground behind. Its boot turns from how it was
            // to flat over the whole of it.)
            public float stepUnder;
            // If it sits back in this pose (NaN: not): its knees bend further at up to so many degrees a second (and
            // its hips by sitHips times as much) for as long as its weight is further ahead of the middle between its
            // soles than sitAhead (a share of its hips' height), and come back when it is behind that; never more
            // than sitMost degrees beyond the pose. So it sits back as far as brings its weight over its feet on the
            // ground it is on, and no further.
            public float sits = float.NaN, sitHips = .5f, sitAhead = .05f, sitMost = 100;
            // If it walks its hands back in this pose (handStep NaN: not): one hand at a time, the one further from its
            // feet, is lifted (so high, metres) and put down handStep metres nearer its feet, over handOver seconds;
            // its arms' joints asked for what puts each hand where it is to be, the one on the ground held where it is.
            // It goes on until the pose is through (its weight over its feet), and puts no hand nearer its feet than
            // handNearest of its leg's length. (How people on their hands and feet get up; a rise planned by where the
            // weight, the hands and the feet go: Research, October 14.)
            public float handStep = float.NaN, handOver = .6f, handLift = .05f, handNearest = .25f;
            // If it keeps its weight over what it rests on in this pose (balance NaN: not): its hands, knees and boots
            // push the ground so as to carry that share of its weight, its weight held over the middle of them (or
            // balanceFeet of the way from there to over its boots, when they bear it; or, if it keeps its weight over
            // its feet in this pose, there); and before a hand or a foot is lifted, its weight goes over the others.
            public float balance = float.NaN, balanceFeet;
            // (balanceLead: and so far again towards over the boot furthest ahead: up over the front foot.)
            public float balanceLead;
            // (letsHandsGo: its weight goes off both hands first, then they let go of the ground, and its arms are as the
            // pose has them. risesHips above nothing: its hips are turned towards how they are standing, at up to so
            // many degrees a second.)
            public bool letsHandsGo;
            public float risesHips;
            // If it holds itself on what bears it in this pose (holdHigh NaN: not, unless it is moving its weight off a
            // limb): its hands, knees and boots that bear it are kept where they are, and its hips are asked to be
            // holdHigh high (a share of their standing height; NaN: as high as they are), going there at up to holdRate
            // of it a second.
            public float holdHigh = float.NaN, holdRate = .3f;
            // (flattens above nothing: its boots kept on the ground are let down flat, turning about their toes, at up
            // to so many degrees a second.)
            public float flattens;
            // (kneels above nothing: its knees are drawn up under it along the ground, each ankle going from where it
            // lay to so many of its shin's lengths behind its own hip, over the pose's time; its feet as the pose has
            // them.)
            public float kneels;
            // (laysBoots: kneeling so, its boots laid on their tops, toes back, its shins flat on the ground.)
            public bool laysBoots;
            // (holdsFeet: on its feet alone, its hands let go.)
            public bool holds, holdsFeet;
        }
        // One way, for a body whose chest faces the ground by between this and this much. At its end the body is on
        // its feet and the keeper takes it (gives); or it only lies another way, and chooses its way again.
        public sealed class Way
        {
            public string name;
            public float frontLeast = -1, frontMost = 1;
            public bool gives;
            public Stage[] stages = new Stage[0];
        }

        // Each joint is held towards its pose with all it has when it is FullAt degrees from it (never stiffer than
        // the keeper's rule for the step of the physics), damped by Damped of what just stops it swinging. Keeping
        // its weight over its feet, its hips bend Balances degrees more for each second its weight is a whole hip's
        // height too far back, and no more than BalancesAtMost more or less than the pose has them.
        public static float FullAt = 35, Damped = .7f, Balances = 150, BalancesAtMost = 70;
        // The keeper's rule caps a joint's spring by the turning weight of the lighter side of it (a stiff spring between
        // a heavy body and a light boot did not hold at the game's step). Standing, that never mattered: what holds the
        // body up is the keeper's own torques, and the springs only hold its pose softly. Getting up, the springs are
        // all there is, and for an elbow or an ankle the cap is far under what the joint has (Small's elbow: about 11 N m
        // a radian, for 36 N m it has). RuleTimes: so many times the keeper's rule. (October 14: at 1, Small on its knees
        // could not lift its chest off the ground with arms that have what pushing it up asks; at 10 it pushes itself
        // up onto its hands and knees, its head up, and nothing shakes.)
        public static float RuleTimes = 10;
        // (Its ankles and toes: so many times the keeper's rule. At RuleTimes a boot, the lightest part on a strong joint,
        // sometimes began to spin, 36 then 120 radians a second, and its shin and then its whole body flew apart:
        // October 20.)
        public static float RuleTimesFeet = 10;
        // (An ankle is asked to be no further than AnkleAtMost degrees from how it stands, and what it is asked moves no
        // faster than AnkleTurnsAtMost radians a second: asked for nearly half a turn, the turn was written the other way
        // round from one step to the next, and the boot whipped round at up to 200 radians a second. October 22.
        // AnkleAtMost 0: as before.)
        public static float AnkleAtMost = 110, AnkleTurnsAtMost = 0;
        private readonly Quaternion[] ankleAsked = { Quaternion.identity, Quaternion.identity };
        // (Where a leg reached for the ground bends its knee: 0, the way the body lies along the ground; 1, the way the
        // front of its hips faces; between, so much of the way from one to the other.)
        public static float KneeFaces = .759f;
        private const float StillUnder = .2f, StillTurning = 1.2f;
        // Balance on hands, knees and feet (Luis, October 14: "I like your idea to implement balance"): the keeper on
        // more than two feet. It works out where the ground must push each hand, knee and boot that bears it, so that
        // its weight is carried and goes towards where it is meant to be (as briskly as BalanceQuick, without
        // overshoot); and each joint between there and its hips gives what makes the ground push so, with no more than
        // it has left over from holding its pose. The ground can push only where they are, and no nearer the edge of
        // what they span than BalanceMargin (metres), and no more aslant than BalanceSlides of its weight. What its pose
        // holds back it learns: it pushes BalanceLeans more for each second its weight stays a metre from where it is
        // meant to be, up to BalanceLeansAtMost metres' worth. Before it lifts a hand or a foot, it moves its weight
        // over the others, giving that limb's share to them over BalanceUnloads seconds, and lifts it once its weight is
        // over them (by BalanceMargin) and the limb bears less than BalanceLifts of it; if that has not come in
        // BalanceShiftsWithin seconds, it lifts it all the same.
        public static float BalanceQuick = 3.36f, BalanceMargin = .021f, BalanceSlides = .3f, BalanceLeans = .6f, BalanceLeansAtMost = .12f;
        // (What bears each part is felt over BalanceFeels seconds; a part bears it once that is BalanceBorne of its
        // weight.)
        public static float BalanceFeels = .08f, BalanceBorne = .02f;
        // (Not moving off a limb, its weight only as far as over what bears it, not to their middle. October 14: held
        // after one step at 5 of 36 places so, at 12 of 36 to their middle; off.)
        public static bool BalanceLeastMove = false;
        public static float BalanceUnloads = .429f, BalanceLifts = .15f, BalanceShiftsWithin = 2.5f;
        // Moving its weight off a limb, while that limb is lifted, and once it is put down, the hands and boots that
        // bear it stay where they are on the ground (each arm's and leg's joints asked for what keeps it there), and
        // its body is asked to be where its weight is meant to be: where it is asked to be goes there at up to BalanceShiftsAt metres a second, and it is
        // asked to be no more than BalanceShiftsAtMost from where it is. (Pushing with its limbs alone fought its pose,
        // which held its body where it was: October 14.)
        public static float BalanceShiftsAt = .122f, BalanceShiftsAtMost = .04f;
        // (Asked to be where it is going to be in BalanceDamps seconds less far: so it comes there without overshoot.
        // BalancePushes: how much of the push that would bring its weight there its limbs give besides, from where they
        // bear; 0: they only bear it up.)
        public static float BalanceDamps = .19f, BalancePushes = .13f;
        // (An arm whose hand is that share of its length or more from its shoulder when the hand is put is kept straight.)
        public static float ArmStraight = .985f;
        // Moving its weight off a limb: to the nearest place over the others deep enough (true), or along the line
        // towards their middle (false).
        public static bool BalanceShiftsNearest = true;
        // Its hold learns what its pose holds back: asked BalanceHoldLearns metres further for each second its weight
        // stays a metre short of where it is asked to be (up to BalanceShiftsAtMost).
        public static float BalanceHoldLearns = 1.862f;
        // A leg folded so that its ankle is within FoldedUnder of a leg's length of its hip keeps its knee facing the
        // way it faces.
        public static float FoldedUnder = .163f;
        // Before it lifts a limb its weight goes so far within the others (metres; 0: twice the margin), and the
        // limb is lifted once it is three quarters of the way there: a shift a person can see.
        public static float BalanceShiftsDeep = .04f;
        private float ShiftDeep => Mathf.Max(2 * BalanceMargin, BalanceShiftsDeep);
        // (How deep its weight is asked to go now: less over an outline too thin for ShiftDeep.)
        private float shiftDeepNow = .04f;
        // Its knee bends the way its hip's bend takes it (true), or between the way it lies and the way its hips face
        // (false: with its hips facing the ground on hands and knees, that turned over once a foot went ahead of its
        // hip, and its knee was asked to bend backwards; KneeFaces says how far between).
        public static bool KneeBendsAsHips = true;
        private Vector3 holdMore;
        private bool planting, landed, handsLet;
        private float limbAtShift;
        private Vector3 askedBy, hipsAtPlant;
        // How much of what it bore when it began to move its weight off it the limb still bears (1: all of it).
        private float OffLoad()
        {
            if ((offLeg < 0 && offHand < 0) || limbAtShift <= 1e-3f) return 1;
            float limbNow = offLeg >= 0 ? leanFelt[Thigh + offLeg] + leanFelt[Shin + offLeg] + leanFelt[Foot + offLeg]
                : offHand == 2 ? leanFelt[Forearm] + leanFelt[Forearm + 1] : leanFelt[Forearm + offHand];
            return Mathf.Clamp01(limbNow / limbAtShift);
        }
        // (How its hips are turned standing, from the way it faces: given by whoever makes it.)
        public Quaternion StoodHips = Quaternion.identity;
        private readonly bool[] plantedHand = new bool[2], plantedLeg = new bool[2], plantedStraight = new bool[2], plantedKnee = new bool[2];
        private readonly Vector3[] plantHand = new Vector3[2], plantSole = new Vector3[2], plantKnee = new Vector3[2];
        private float highTo;
        private Quaternion hipsHeld = Quaternion.identity;
        private readonly Quaternion[] plantBoot = new Quaternion[2];
        private Vector3 weightTo, bodyTo;
        // What it leans on: its forearms (its hands, or its elbows), its thighs and shins (its knees: kneeling, the end
        // of its thigh is what touches the ground as often as its shin) and its boots (with their toes).
        private static readonly int[] Leans = { Forearm, Forearm + 1, Thigh, Thigh + 1, Shin, Shin + 1, Foot, Foot + 1 };
        // (What the ground bears its forearms, thighs and shins with, where it has them.)
        public GroundTouch[] PartTouches;
        private readonly float[] leanUp = new float[Count], leanFelt = new float[Count];
        private readonly Vector3[] leanAt = new Vector3[Count], leanFeltAt = new Vector3[Count];
        private readonly float[] springOf = new float[Count], hasOf = new float[Count];
        private readonly Vector3[] torque = new Vector3[Count];
        private readonly Vector3[] sup = new Vector3[16], supAt = new Vector3[16], outline = new Vector3[33], sorted = new Vector3[16];
        private readonly int[] supPart = new int[16];
        private readonly float[] supW = new float[16], supF = new float[16], shareW = new float[16];
        // (Of the limb it moves its weight off: never one of the others its weight goes over, whatever it still bears.)
        private readonly bool[] supOff = new bool[16];
        // (Each boot's sole, as the keeper has it: a box about the boot's middle, so wide, high and long.)
        public Vector3[] SoleSize;
        // (A boot that bears it and lies this flat or flatter bears it over its whole sole: its four corners.)
        private const float SoleFlat = .85f;
        // The limb it moves its weight off (a leg, or a hand; -1: none), and how much of its share it still gives it.
        private int offLeg = -1, offHand = -1;
        private float offW = 1, shiftT, liftAt, lastStep = .02f;
        private Vector3 leansMore;
        // How far within what bears it its weight is (metres; less than nothing outside; NaN: nothing bears it it
        // leans on), on how many, and how often it moved its weight off a limb before lifting it (and lifted it all
        // the same, its weight not over the others).
        public float BalanceDepth { get; private set; } = float.NaN;
        public int Supports { get; private set; }
        public int Shifts { get; private set; }
        public int ShiftsFailed { get; private set; }

        public Way[] Ways = new Way[0];
        // (Its toes' touches, if it has toes: what the ground bears them with counts as its boots'.)
        public GroundTouch[] ToeTouches;
        // How hard it holds itself (1: with all the strength it has).
        public float Tone = 1;

        private readonly ArticulationBody[] parts;
        private readonly int[] from;
        private readonly float[] masses, strength, lighter;
        private readonly bool[] hinge;
        private readonly GroundTouch[] touches;
        private readonly Vector3[] soleAt;
        private readonly Quaternion[] toStood, rested, stoodBoot;
        private readonly Vector3[] anchors;
        private readonly float[] bears = new float[2];
        private Vector3 stepFrom, stepTo;
        private Quaternion stepBoot, stepBootFrom = Quaternion.identity;
        private Vector3 stepLies;
        private float stepReach;
        // (How far forward the furthest foot it put down went, in its leg's lengths: for whoever measures its steps.)
        public float StepFurthest { get; private set; }
        private bool stepBegun, stepAimed;
        private readonly Quaternion[] aimFrom = new Quaternion[Count], aimNow = new Quaternion[Count];
        private readonly float tall, whole;
        private readonly int ground;
        private Way way;
        private Stage stage;
        private int at, leads, chosen;
        private float got, still, leant, bore, toned = -1, sat;
        // The hands' walk: which hand goes, how far along, from where and to where; where each hand is held.
        private int walkHand = -1;
        private float walkT;
        private readonly Vector3[] handAt = new Vector3[2];
        private Vector3 walkFrom, walkTo;
        public int HandsMoved { get; private set; }

        public string WayNow => way != null ? way.name : "";
        // Which of its way's poses it is at, of how many.
        public int StageAt => at;
        public int Stages => way != null ? way.stages.Length : 0;
        public string StageNow => stage != null ? stage.name : "";
        // What a joint is asked for now: its turn, in its own frame, from how it was made.
        public Quaternion Aim(int j) => aimNow[j];
        public float Lasted { get; private set; }

        // It breathes while it gets up (G1, G8): its back straightens a little as its chest fills, by BreathBends
        // degrees now (given by its body, from the miner's own breath: deeper and quicker the harder it has worked, and
        // after a fall). And while it holds itself on what bears it, its weight wanders a little and slowly (up to
        // WandersBy metres, changing its way about every WandersEvery seconds): a body holding still is never quite
        // still.
        public float BreathBends;
        public static float WandersBy = .012f, WandersEvery = 2.5f;
        private Vector3 wander, wanderFrom, wanderTo;
        private float wanderAlong = 1;
        private uint wanderSeed = 2463534242u;
        private float WanderChance() { wanderSeed ^= wanderSeed << 13; wanderSeed ^= wanderSeed >> 17; wanderSeed ^= wanderSeed << 5; return (wanderSeed & 0xFFFFFF) / 16777216f; }

        // rested: each part's turn against what it hangs from, as it was made. anchors: where each part's joint is, in
        // the part. stoodBoot: how each boot is turned standing, from the way the body faces.
        public OwnRiser(ArticulationBody[] parts, int[] from, float[] masses, float[] strength, bool[] hinge, GroundTouch[] touches, Vector3[] soleAt, Quaternion[] toStood, float[] lighter, float tall, int ground,
            Quaternion[] rested, Vector3[] anchors, Quaternion[] stoodBoot)
        {
            this.parts = parts; this.from = from; this.masses = masses; this.strength = strength; this.hinge = hinge;
            this.touches = touches; this.soleAt = soleAt; this.toStood = toStood; this.lighter = lighter; this.tall = Mathf.Max(.1f, tall); this.ground = ground;
            this.rested = rested; this.anchors = anchors; this.stoodBoot = stoodBoot;
            float all = 0;
            for (int i = 0; i < Count; i++) { all += masses[i]; aimNow[i] = aimFrom[i] = Quaternion.identity; }
            whole = all;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        public Vector3 Weight()
        {
            Vector3 c = Vector3.zero;
            for (int i = 0; i < Count; i++) c += parts[i].worldCenterOfMass * masses[i];
            return c / whole;
        }

        private float GroundAt(Vector3 place) => Physics.Raycast(place + Vector3.up * .5f, Vector3.down, out var below, 4, ground) ? below.point.y : place.y;

        public float Measure(Asks what)
        {
            Vector3 hips = parts[Hips].transform.position;
            Vector3 feet = .5f * (parts[Foot].transform.TransformPoint(soleAt[0]) + parts[Foot + 1].transform.TransformPoint(soleAt[1]));
            switch (what)
            {
                case Asks.Front: return Vector3.Dot(parts[Trunk].transform.forward, Vector3.down);
                case Asks.Hips: return (hips.y - GroundAt(hips)) / tall;
                case Asks.Off: return Flat(hips - feet).magnitude / tall;
                case Asks.Ahead:
                    Vector3 toes = Flat(parts[Foot].transform.forward + parts[Foot + 1].transform.forward);
                    if (toes.sqrMagnitude < 1e-6f) return 0;
                    return Vector3.Dot(Flat(Weight() - feet), toes.normalized) / tall;
                case Asks.Steep: return Mathf.Min(-parts[Shin].transform.up.y, -parts[Shin + 1].transform.up.y);
                case Asks.Soles: return Mathf.Min(parts[Foot].transform.up.y, parts[Foot + 1].transform.up.y);
                case Asks.Upright: return parts[Trunk].transform.up.y;
                case Asks.Head:
                    Vector3 head = parts[Head].transform.position;
                    return (head.y - GroundAt(head)) / tall;
                case Asks.Bears: return bore;
                default: return still;
            }
        }

        // Each joint's spring, by how hard it holds itself now.
        private void Springs(float step)
        {
            toned = Tone; strongerWas = StrongerWhileLearning;
            for (int j = 1; j < Count; j++)
            {
                float has = strength[j] * Mathf.Clamp01(Tone) * StrongerWhileLearning;
                float spring = Mathf.Min(has / (FullAt * Mathf.Deg2Rad), (j >= Foot ? RuleTimesFeet : RuleTimes) * OwnKeeper.Rule * OwnKeeper.Rule / (step * step) * lighter[j]);
                springOf[j] = spring; hasOf[j] = has;
                Drive(j, 1);
            }
        }

        // A joint's spring, so much of what it holds itself with (1: all; less, slack).
        private readonly float[] slackNow = new float[Count];
        private void Drive(int j, float share)
        {
            slackNow[j] = share;
            float spring = springOf[j] * share, has = hasOf[j] * Mathf.Max(share, .3f);
            float damper = Damped * 2 * Mathf.Sqrt(Mathf.Max(spring, 1e-3f) * lighter[j]);
            var drive = parts[j].xDrive; drive.stiffness = spring; drive.damping = damper; drive.forceLimit = has; parts[j].xDrive = drive;
            if (hinge[j]) return;
            drive = parts[j].yDrive; drive.stiffness = spring; drive.damping = damper; drive.forceLimit = has; parts[j].yDrive = drive;
            drive = parts[j].zDrive; drive.stiffness = spring; drive.damping = damper; drive.forceLimit = has; parts[j].zDrive = drive;
        }

        // Moving its weight off a limb, that limb goes slack as it gives its share to the others (down to LetsGo of how it
        // held itself); every other joint, and that limb once it is lifted, holds itself as before.
        public static float LetsGo = .357f;
        private void Slacken(int leg, int hand, float share)
        {
            for (int j = 1; j < Count; j++)
            {
                bool limb = (leg >= 0 && (j == Thigh + leg || j == Shin + leg || j == Foot + leg)) || (hand >= 0 && (j == UpperArm + hand || j == Forearm + hand || (hand == 2 && j >= UpperArm && j < Thigh)));
                float want = limb ? Mathf.Lerp(LetsGo, 1, share) : 1;
                if (Mathf.Abs(want - slackNow[j]) > .01f) Drive(j, want);
            }
        }

        private void Aims(int j, Quaternion own)
        {
            // (Nothing asked of a joint from a turn that is not one: a part gone wild leaves its joint as it was.)
            float size = own.x * own.x + own.y * own.y + own.z * own.z + own.w * own.w;
            if (float.IsNaN(size) || size < .5f || size > 1.5f) return;
            own = Quaternion.Normalize(own);
            if (j >= Foot && AnkleAtMost > 0) own = AnkleCan(j - Foot, own);
            own.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.right; angle = 0; }
            Vector3 by = angle * axis;
            wayAsks[j] = by;
            by += drivenNow[j];
            parts[j].SetDriveTarget(ArticulationDriveAxis.X, by.x);
            if (hinge[j]) return;
            parts[j].SetDriveTarget(ArticulationDriveAxis.Y, by.y);
            parts[j].SetDriveTarget(ArticulationDriveAxis.Z, by.z);
        }

        // What an ankle can be asked: no further from how it stands than AnkleAtMost, and moving towards it no faster
        // than AnkleTurnsAtMost (a turn from how it was made, in its own frame).
        private Quaternion AnkleCan(int leg, Quaternion own)
        {
            Quaternion stood = toStood[Foot + leg], fromStood = Quaternion.Inverse(stood) * own;
            fromStood.ToAngleAxis(out float far, out Vector3 about);
            if (far > 180) far -= 360;
            if (Mathf.Abs(far) > AnkleAtMost && !float.IsNaN(about.x)) fromStood = Quaternion.AngleAxis(Mathf.Sign(far) * AnkleAtMost, about);
            // (AnkleTurnsAtMost above nothing: what it is asked moves no faster. Off: a boot kept flat on the ground is asked
            // to turn against its shin as fast as its shin turns, and slowed so it lost its footing: 0 of 36 stood.)
            Quaternion can = stood * fromStood;
            if (AnkleTurnsAtMost > 0) can = Quaternion.RotateTowards(ankleAsked[leg], can, AnkleTurnsAtMost * Mathf.Rad2Deg * Mathf.Max(lastStep, 1e-3f));
            ankleAsked[leg] = can;
            return can;
        }

        // A joint's turn from how it stands, in its own frame, for the pose it is at. (A limb's part lies along its
        // own second line, pointing away from the body, and bends about its first: a thigh goes forward by turning
        // one way about it, a shin back by turning the other; an arm's parts are made facing back, so they go the
        // other way round from a leg's.)
        private Quaternion Turn(int j)
        {
            float[] p = stage.pose;
            if (j == Trunk) return Learned != null ? Quaternion.Euler(p[0] + Learned[0], (p[2] + Learned[2]) * (leads == 1 ? -1 : 1), 0) : Quaternion.Euler(p[0], p[2] * (leads == 1 ? -1 : 1), 0);
            if (j == Head) return Quaternion.Euler(p[1] + (Learned != null ? Learned[1] : 0), 0, 0);
            int side = (j - UpperArm) % 2, n = side == leads ? 0 : 1, a = 3 + 3 * n, l = 9 + 4 * n;
            switch (j - side)
            {
                case UpperArm: return Quaternion.AngleAxis(-p[a], Vector3.right) * Quaternion.AngleAxis(p[a + 1] * (side == 0 ? 1 : -1), Vector3.forward);
                case Forearm: return Quaternion.AngleAxis(-p[a + 2], Vector3.right);
                case Thigh: return Quaternion.AngleAxis(Mathf.Clamp(p[l] + leant + (float.IsNaN(stage.sits) ? 0 : stage.sitHips * sat), -20, 130), Vector3.right) * Quaternion.AngleAxis(p[l + 1] * (side == 0 ? -1 : 1), Vector3.forward);
                case Shin: return Quaternion.AngleAxis(-(p[l + 2] + (float.IsNaN(stage.sits) ? 0 : sat)), Vector3.right);
                default: return Quaternion.AngleAxis(-p[l + 3], Vector3.right);
            }
        }

        // It chooses its way, by how it lies. The side that is uppermost leads.
        private bool Choose()
        {
            float front = Measure(Asks.Front);
            way = null;
            if (++chosen > 4) return false;
            foreach (var one in Ways)
                if (one != null && one.stages.Length > 0 && front >= one.frontLeast && front <= one.frontMost) { way = one; break; }
            if (way == null) return false;
            leads = Vector3.Dot(parts[Trunk].transform.right, Vector3.up) >= 0 ? 1 : 0;
            leant = 0; leansMore = Vector3.zero; holdMore = Vector3.zero;
            Begin(0);
            return true;
        }

        private Vector3 JointAt(int j) => parts[j].transform.TransformPoint(anchors[j]);

        // The way the body lies along the ground: from its hips towards its head when it is bowed over or lying;
        // the way its hips face when its trunk is up.
        // (From one to the other smoothly, between a sixth and a third of its height from its hips to its head: at a
        // quarter it jumped from one to the other, and every limb reaching for the ground jumped with it.)
        public Vector3 Lies()
        {
            Vector3 along = Flat(parts[Head].transform.position - parts[Hips].transform.position), faces = Flat(parts[Hips].transform.forward);
            float share = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.15f, .35f, along.magnitude / tall));
            Vector3 alongWay = along.sqrMagnitude > 1e-8f ? along.normalized : Vector3.zero, facesWay = faces.sqrMagnitude > 1e-6f ? faces.normalized : alongWay;
            Vector3 lies = Vector3.Lerp(facesWay, alongWay, share);
            if (lies.sqrMagnitude > 1e-4f) return lies.normalized;
            return share >= .5f && alongWay != Vector3.zero ? alongWay : facesWay != Vector3.zero ? facesWay : Vector3.forward;
        }

        // A foot is put somewhere: its boot carried there, lifted in the middle, and laid flat; its leg's joints asked
        // for what puts it there.
        // (aimed: where the boot goes is chosen once, when it is lifted, and kept, as a foot is put on a spot; else it is
        // reckoned from where its hip is at each step.)
        private void Steps(int leg, float along, bool aimed = false)
        {
            Vector3 lies = Lies(), aside = Vector3.Cross(Vector3.up, lies) * (leg == 1 ? 1 : -1);
            Vector3 hip = JointAt(Thigh + leg);
            float thighLong = (JointAt(Shin + leg) - hip).magnitude, shinLong = (JointAt(Foot + leg) - JointAt(Shin + leg)).magnitude, reach = thighLong + shinLong;
            float stepAhead = stage.stepAhead + (Learned != null && Learned.Length > 7 ? Learned[6] : 0), stepOut = stage.stepOut + (Learned != null && Learned.Length > 7 ? Learned[7] : 0);
            Vector3 to = Flat(hip) + lies * (stepAhead * reach) + aside * (stepOut * reach);
            Quaternion boot = Quaternion.LookRotation(lies, Vector3.up) * stoodBoot[leg];
            if (aimed && !stepAimed) { stepAimed = true; stepTo = to; stepBoot = boot; }
            if (aimed) { to = stepTo; boot = stepBoot; }
            // (Once it is there it is pressed down a little: laid exactly on the ground, a boot bore nothing, and the
            // pose that waits for it to bear waited for ever.)
            // (Keeping its balance, it is pressed only a little, and kept where it is as soon as it bears anything:
            // pressed three centimetres, a leg out to the side shoved its body over onto its other arm.)
            to.y = GroundAt(new Vector3(to.x, hip.y, to.z)) - (along >= 1 ? (aimed ? .008f : .03f) : 0);
            Vector3 sole;
            if (stage.stepUnder > 0)
            {
                // (A curve from where it was to where it goes that passes under its hip, so high, half way.)
                float ground = GroundAt(hip);
                Vector3 under = new Vector3(hip.x, ground + stage.stepUnder * (hip.y - ground), hip.z);
                Vector3 bend = 2 * under - .5f * (stepFrom + to);
                float u = along;
                sole = (1 - u) * (1 - u) * stepFrom + 2 * u * (1 - u) * bend + u * u * to;
                boot = BootTurn(stepBootFrom, boot, Mathf.SmoothStep(0, 1, along));
            }
            else sole = Vector3.Lerp(stepFrom, to, along) + Vector3.up * (stage.stepLifts * Mathf.Sin(Mathf.PI * along));
            LegTo(leg, sole, boot);
        }

        // A boot turned from one way to another, so far along (0 to 1), as a foot turns: more than 120 degrees to go,
        // by way of its toes pointing down (half a turn apart, which way round was left to chance, and the boot whipped).
        private static Quaternion BootTurn(Quaternion from, Quaternion to, float along)
        {
            if (Quaternion.Angle(from, to) <= 120) return Quaternion.Slerp(from, to, along);
            // (Half way, its toes pointing down: turned from where it was by the least turn that does that, which for a
            // boot lying on its top, toes back, is a quarter turn about the line across its foot.)
            Quaternion down = Quaternion.FromToRotation(from * Vector3.forward, Vector3.down) * from;
            return along < .5f ? Quaternion.Slerp(from, down, 2 * along) : Quaternion.Slerp(down, to, 2 * along - 1);
        }

        // A leg's joints asked for what puts its boot's sole at a place, the boot turned so.
        private void LegTo(int leg, Vector3 sole, Quaternion boot) => LegFrom(leg, JointAt(Thigh + leg), parts[Hips].transform.rotation, sole, boot);

        // The same, from a hip at a place, of hips turned so (where its hips are asked to be).
        private void LegFrom(int leg, Vector3 hip, Quaternion hips, Vector3 sole, Quaternion boot) => AnkleFrom(leg, hip, hips, sole + boot * (anchors[Foot + leg] - soleAt[leg]), boot, true);

        // A leg's hip and knee asked for what puts its ankle at a place, from a hip at a place, of hips turned so; its
        // boot turned so (turnsBoot), or left as asked.
        private void AnkleFrom(int leg, Vector3 hip, Quaternion hips, Vector3 ankleAt, Quaternion boot, bool turnsBoot)
        {
            Vector3 lies = Lies();
            float thighLong = (JointAt(Shin + leg) - JointAt(Thigh + leg)).magnitude, shinLong = (JointAt(Foot + leg) - JointAt(Shin + leg)).magnitude, reach = thighLong + shinLong;
            // Its hip and knee, for its ankle to be there. (Until October 14 its knee bent "up and ahead" in the world:
            // bowed over, with its foot still behind it, that sent its knee up over its back.)
            Vector3 ankle = ankleAt - hip;
            float far = Mathf.Clamp(ankle.magnitude, Mathf.Abs(thighLong - shinLong) + .01f, reach * .995f);
            // (Its knee goes the way the body lies along the ground, towards its head: kneeling, that is down and
            // ahead; a foot brought forward under it, its knee comes up towards its chest. Until October 14, evening,
            // the way the front of its hips faced: bowed over, that is down, and a foot brought forward came with its
            // knee on the ground and its shin flat.)
            Vector3 line = ankle.normalized, faces = Vector3.Lerp(lies, parts[Hips].transform.forward, KneeFaces);
            // (Or the way its hip's bend takes it: in the plane its hips bow in, a quarter turn on from the way its leg
            // goes, on the side its hips face. Standing, a leg straight down: ahead. On hands and knees, a thigh straight
            // down: towards its head. A knee drawn up to its chest: towards its head and up. It never turns over.)
            if (KneeBendsAsHips)
            {
                Vector3 front = hips * Vector3.forward, up = hips * Vector3.up;
                Vector3 bends = -Vector3.Dot(line, up) * front + Vector3.Dot(line, front) * up;
                if (bends.sqrMagnitude > 1e-4f) faces = bends;
            }
            Vector3 pole = Vector3.ProjectOnPlane(faces, line);
            pole = pole.sqrMagnitude > 1e-6f ? pole.normalized : Vector3.ProjectOnPlane(parts[Hips].transform.forward, line).normalized;
            float opens = Mathf.Acos(Mathf.Clamp((thighLong * thighLong + far * far - shinLong * shinLong) / (2 * thighLong * far), -1, 1));
            Vector3 thighWay = line * Mathf.Cos(opens) + pole * Mathf.Sin(opens);
            // (A leg folded tight, its ankle by its hip, says nothing by the way from one to the other of where its
            // thigh goes or which way its knee faces: built from ways lying almost along one another, they were left to
            // chance, and its leg kicked up behind it as it was lifted. Folded so, its thigh stays where it is and faces
            // the way it faces: wholly with its ankle within FoldedUnder of a leg's length of its hip, not at all from
            // twice that.)
            float folded = Mathf.InverseLerp(2 * FoldedUnder, FoldedUnder, ankle.magnitude / Mathf.Max(.05f, reach));
            Vector3 thighNow = (JointAt(Shin + leg) - JointAt(Thigh + leg)).normalized;
            if (folded > 0 && thighNow.sqrMagnitude > .5f) thighWay = Vector3.Slerp(thighWay, thighNow, folded).normalized;
            Vector3 kneeAt = hip + thighWay * thighLong, shinWay = ankleAt - kneeAt;
            shinWay = shinWay.sqrMagnitude > 1e-8f ? shinWay.normalized : -thighWay;
            // (Its knee faces away from where its shin goes, as a hinge does; where its shin goes along its thigh, the
            // way its pose says, or, folded, the way it faces now.)
            Vector3 aside = Vector3.ProjectOnPlane(shinWay, thighWay), poseFaces = Vector3.ProjectOnPlane(pole, thighWay).normalized;
            Vector3 nowFaces = Vector3.ProjectOnPlane(parts[Thigh + leg].transform.forward, thighWay).normalized;
            Vector3 otherwise = Vector3.Slerp(poseFaces, nowFaces.sqrMagnitude > .5f ? nowFaces : poseFaces, folded);
            Vector3 kneeFaces = -aside + (1 - aside.magnitude) * otherwise;
            kneeFaces = kneeFaces.sqrMagnitude > 1e-8f ? kneeFaces.normalized : poseFaces;
            Quaternion thigh = Quaternion.LookRotation(kneeFaces, thighWay);
            // (Its shin turned about its knee's hinge alone, so far that it points at its ankle.)
            Quaternion unbent = thigh * rested[Shin + leg];
            Vector3 wants = Quaternion.Inverse(unbent) * shinWay;
            Quaternion shin = unbent * Quaternion.AngleAxis(Mathf.Atan2(wants.z, wants.y) * Mathf.Rad2Deg, Vector3.right);
            aimNow[Thigh + leg] = Quaternion.Inverse(hips * rested[Thigh + leg]) * thigh;
            aimNow[Shin + leg] = Quaternion.Inverse(unbent) * shin;
            if (turnsBoot) aimNow[Foot + leg] = Quaternion.Inverse(shin * rested[Foot + leg]) * boot;
            for (int j = Thigh + leg; j <= Foot + leg; j += 2) Aims(j, aimNow[j]);
        }

        // Its knees drawn up under it along the ground.
        private readonly Vector3[] kneelFrom = new Vector3[2];
        private readonly Quaternion[] kneelBootFrom = { Quaternion.identity, Quaternion.identity };
        // (How far each boot rolls over its toes, not lying on its top: wholly while its knee bears nothing.)
        private readonly float[] bootRolls = { 1, 1 };
        // (Not the leg being stepped with: the other one's shin stays on the ground while it steps.)
        private void Kneels(float share, int stepping)
        {
            Vector3 lies = Lies();
            for (int leg = 0; leg < 2; leg++)
            {
                if (leg == stepping) continue;
                Vector3 hip = JointAt(Thigh + leg);
                float shinLong = (JointAt(Foot + leg) - JointAt(Shin + leg)).magnitude;
                Vector3 to = Flat(hip) - lies * (stage.kneels * shinLong);
                // (At a boot's height over the ground, as an ankle is with its boot lying on the ground: kept at the
                // height it began at, it stayed in the air if its feet had risen before.)
                float over = SoleSize != null ? 1.2f * SoleSize[leg].y : .08f;
                // (Its boot laid on its top, toes back, if the pose says: its ankle as high as that puts it, its shin's
                // thickness at least.)
                Quaternion laid = Quaternion.LookRotation(lies, Vector3.up) * Quaternion.AngleAxis(180, Vector3.right) * stoodBoot[leg];
                if (stage.laysBoots && SoleSize != null)
                {
                    var rod = parts[Shin + leg].GetComponent<CapsuleCollider>();
                    over = Mathf.Max(rod != null ? rod.radius : .03f, .5f * SoleSize[leg].y - anchors[Foot + leg].y) + .005f;
                }
                to.y = GroundAt(new Vector3(to.x, hip.y, to.z)) + over;
                Vector3 ankle = Vector3.Lerp(kneelFrom[leg], to, share) + Vector3.up * (.02f * Mathf.Sin(Mathf.PI * share));
                // (Its boots laid: down to the ground first, then drawn along it.)
                if (stage.laysBoots) ankle.y = Mathf.Lerp(kneelFrom[leg].y, to.y, Mathf.SmoothStep(0, 1, Mathf.Clamp01(3 * share)));
                if (stage.laysBoots)
                {
                    // (Rolled over its toes: the tip of its toes, laid, is so far below and behind its ankle; with its
                    // ankle higher than that, its toes go down about its ankle until their tip is on the ground.)
                    Vector3 half = .5f * SoleSize[leg], a = anchors[Foot + leg];
                    float below = half.y - a.y, behind = half.z - a.z, far = Mathf.Sqrt(below * below + behind * behind);
                    float high = ankle.y - GroundAt(ankle), slant = Mathf.Atan2(behind, below);
                    float down = far > 1e-4f ? slant - Mathf.Acos(Mathf.Clamp(high / far, -1, 1)) : 0;
                    // (Only while its knee bears nothing: rolled whatever, its toes bore its weight and held its knee off
                    // the ground. Once its knee bears, it lies.)
                    bool kneeBears = leanFelt[Thigh + leg] + leanFelt[Shin + leg] >= BalanceBorne;
                    bootRolls[leg] = Mathf.MoveTowards(bootRolls[leg], kneeBears ? 0 : 1, 4 * lastStep);
                    Quaternion rolled = Quaternion.AngleAxis(bootRolls[leg] * Mathf.Clamp(down, 0, slant) * Mathf.Rad2Deg, Vector3.Cross(lies, Vector3.up).normalized) * laid;
                    AnkleFrom(leg, hip, parts[Hips].transform.rotation, ankle, Quaternion.Slerp(kneelBootFrom[leg], rolled, Mathf.SmoothStep(0, 1, Mathf.Clamp01(4 * share))), true);
                }
                else AnkleFrom(leg, hip, parts[Hips].transform.rotation, ankle, Quaternion.identity, false);
            }
        }

        // Where a hand's end is (the end of its forearm, which goes on past the wrist).
        private float ArmRadius(int i) { var rod = parts[Forearm + i].GetComponent<CapsuleCollider>(); return rod != null ? rod.radius : .04f; }
        private Vector3 HandEnd(int i) => parts[Forearm + i].transform.TransformPoint(new Vector3(0, -anchors[Forearm + i].y + ArmRadius(i), 0));

        // An arm's joints asked for what puts its hand's end at a place: its elbow bends back, towards its feet.
        // (straight: its elbow kept straight, the arm pointed at the place: a straight arm bears its weight through its
        // bones, and asked to bend a little it gave way.)
        private void Reach(int i, Vector3 end, bool straight = false) => ArmFrom(i, JointAt(UpperArm + i), parts[Trunk].transform.rotation, end, straight);

        // The same, from a shoulder at a place, of a trunk turned so (where its hips are asked to be).
        private void ArmFrom(int i, Vector3 shoulder, Quaternion trunk, Vector3 end, bool straight)
        {
            Vector3 elbow = JointAt(Forearm + i);
            float upper = (elbow - JointAt(UpperArm + i)).magnitude, fore = (HandEnd(i) - elbow).magnitude;
            Vector3 to = end - shoulder;
            // (A straight arm stays straight while its shoulder is as far from its hand as it reaches, and bends only once
            // its body comes nearer: held straight whatever, it held its body off its hands and its weight could not go
            // over them.)
            float far = Mathf.Clamp(to.magnitude, Mathf.Abs(upper - fore) + .01f, (upper + fore) * (straight ? 1 : .995f));
            Vector3 line = to.normalized, outward = Vector3.Cross(Vector3.up, Lies()) * (i == 1 ? 1 : -1);
            Vector3 pole = Vector3.ProjectOnPlane(-Lies() + .5f * outward, line);
            pole = pole.sqrMagnitude > 1e-6f ? pole.normalized : Vector3.ProjectOnPlane(Vector3.up, line).normalized;
            float opens = Mathf.Acos(Mathf.Clamp((upper * upper + far * far - fore * fore) / (2 * upper * far), -1, 1));
            Vector3 upperWay = line * Mathf.Cos(opens) + pole * Mathf.Sin(opens);
            Vector3 foreWay = (line * far - upperWay * upper).normalized;
            Quaternion upperR = Quaternion.LookRotation(Vector3.ProjectOnPlane(pole, upperWay).normalized, upperWay);
            Quaternion foreR = Quaternion.LookRotation(Vector3.ProjectOnPlane(pole, foreWay).normalized, foreWay);
            aimNow[UpperArm + i] = Quaternion.Inverse(trunk * rested[UpperArm + i]) * upperR;
            aimNow[Forearm + i] = Quaternion.Inverse(upperR * rested[Forearm + i]) * foreR;
            Aims(UpperArm + i, aimNow[UpperArm + i]);
            Aims(Forearm + i, aimNow[Forearm + i]);
        }

        // One step of the hands' walk.
        private void WalksHands(float step)
        {
            Vector3 lies = Lies(), feet = .5f * (parts[Foot].transform.TransformPoint(soleAt[0]) + parts[Foot + 1].transform.TransformPoint(soleAt[1]));
            float reach = (JointAt(Shin) - JointAt(Thigh)).magnitude + (JointAt(Foot) - JointAt(Shin)).magnitude;
            if (walkHand < 0)
            {
                // (The hand further from its feet goes; none is put nearer its feet than so much.)
                float a0 = Vector3.Dot(Flat(HandEnd(0) - feet), lies), a1 = Vector3.Dot(Flat(HandEnd(1) - feet), lies);
                int goes = a0 >= a1 ? 0 : 1;
                float from = Mathf.Max(a0, a1), nearest = stage.handNearest * reach;
                if (from - stage.handStep < nearest) { for (int i = 0; i < 2; i++) Reach(i, handAt[i]); return; }
                // (Keeping its balance, its weight goes off that hand first.)
                if (!float.IsNaN(stage.balance) && !ShiftedOff(-1, goes, step)) { for (int i = 0; i < 2; i++) Reach(i, handAt[i]); return; }
                walkHand = goes;
                walkFrom = HandEnd(walkHand);
                walkTo = walkFrom - lies * stage.handStep;
                if (Physics.Raycast(walkTo + Vector3.up, Vector3.down, out var below, 3, ground)) walkTo.y = below.point.y + ArmRadius(walkHand) - .015f;
                walkT = 0;
            }
            walkT += step / Mathf.Max(.1f, stage.handOver);
            offHand = walkHand; offW = 0;
            float u = Mathf.Clamp01(walkT), along = u * u * (3 - 2 * u);
            Vector3 end = Vector3.Lerp(walkFrom, walkTo, along) + Vector3.up * (stage.handLift * Mathf.Sin(Mathf.PI * u));
            Reach(walkHand, end);
            Reach(1 - walkHand, handAt[1 - walkHand]);
            if (walkT >= 1) { handAt[walkHand] = walkTo; walkHand = -1; HandsMoved++; if (!float.IsNaN(stage.balance)) Plant(-1, -1); }
        }

        // A hand walked nearer its feet while it waits to let go: false if none can go nearer.
        private bool WalkBack(float step)
        {
            Vector3 lies = Lies(), feet = .5f * (parts[Foot].transform.TransformPoint(soleAt[0]) + parts[Foot + 1].transform.TransformPoint(soleAt[1]));
            float reach = (JointAt(Shin) - JointAt(Thigh)).magnitude + (JointAt(Foot) - JointAt(Shin)).magnitude;
            if (walkHand < 0)
            {
                float a0 = Vector3.Dot(Flat(HandEnd(0) - feet), lies), a1 = Vector3.Dot(Flat(HandEnd(1) - feet), lies);
                int goes = a0 >= a1 ? 0 : 1;
                if (Mathf.Max(a0, a1) - ReleaseWalks < ReleaseNearest * reach) return false;
                // (Its weight off that hand first, over its boots and the other hand.)
                if (!ShiftedOff(-1, goes, step)) { for (int i = 0; i < 2; i++) Reach(i, handAt[i]); return true; }
                walkHand = goes;
                walkFrom = HandEnd(walkHand);
                walkTo = walkFrom - lies * ReleaseWalks;
                if (Physics.Raycast(walkTo + Vector3.up, Vector3.down, out var below, 3, ground)) walkTo.y = below.point.y + ArmRadius(walkHand) - .015f;
                walkT = 0;
            }
            walkT += step / .5f;
            offHand = walkHand; offW = 0;
            float u = Mathf.Clamp01(walkT), along = u * u * (3 - 2 * u);
            Reach(walkHand, Vector3.Lerp(walkFrom, walkTo, along) + Vector3.up * (.04f * Mathf.Sin(Mathf.PI * u)));
            Reach(1 - walkHand, handAt[1 - walkHand]);
            if (walkT >= 1) { handAt[walkHand] = walkTo; walkHand = -1; HandsMoved++; Plant(-1, -1); ReleaseWaited = 0; shiftT = 0; }
            return true;
        }

        private void Begin(int index)
        {
            at = index; stage = way.stages[index]; got = 0; still = 0; slowFor = 0; movedIn = false; ReleaseWaited = 0; walksBack = cannotWalkBack = false; hipsBegan = Measure(Asks.Hips); downFor = 0; stepBegun = false; sat = 0; shiftT = 0; liftAt = 0; planting = false; landed = false; stepAimed = false; handsLet = false;
            walkHand = -1; for (int i = 0; i < 2; i++) { handAt[i] = HandEnd(i); kneelFrom[i] = JointAt(Foot + i); kneelBootFrom[i] = parts[Foot + i].transform.rotation; }
            for (int j = 1; j < Count; j++) aimFrom[j] = aimNow[j];
        }

        // (Gone down: up off the ground when a pose began, its hips UpFrom of their standing height or more, and now its
        // hips as low as lying, under DownUnder of it, for DownFor seconds. It gives up that try at once: fallen as it
        // rolled up onto its feet, it lay in its pose until the pose's time was out, twenty seconds. (Taken first: its
        // hips lower than when the pose began, by .15; but rolling up from its bottom-up stance, a body brings its hips
        // down into a squat on the way. Then also its boots bearing little on their sides; but kneeling, its boots lie
        // on their tops bearing nothing.) October 22.)
        public static float UpFrom = .4f, DownUnder = .3f, DownFor = .8f;
        // (The last pose of a way that ends on its feet is through as soon as all it asks is so, and no sooner than
        // ReadyAfter seconds: rolling up, it was ready 0.28 s in and waited out the pose's 1.8 s, and sat back onto its
        // heels in the wait.)
        public static float ReadyAfter = .2f;
        // (Any pose whose ends are all so goes on once the body has been slow in it GoesOnStill seconds (no part faster
        // than GoesOnBelow), not waiting out the rest of its time: a body still in its pose until a timer is what Luis
        // called looking dead, October 13, and he asked that holds be charged, October 28. GoesOnAfterMoving 1: only
        // once it has moved in that pose. GoesOnStill 0: each pose its time, as searched.)
        public static float GoesOnStill = 0, GoesOnBelow = .12f;
        public static int GoesOnAfterMoving = 0;
        private float hipsBegan, downFor, slowFor;
        private bool movedIn;

        // One step of getting up.
        public Does Step(float step)
        {
            float g = Physics.gravity.magnitude;
            lastStep = step; offLeg = offHand = -1; offW = 1;
            // What the ground bore each part it leans on with, and where (its boots with their toes).
            for (int k = 0; k < Leans.Length; k++) { leanUp[Leans[k]] = 0; leanAt[Leans[k]] = Vector3.zero; }
            if (PartTouches != null)
                for (int j = 0; j < Foot && j < PartTouches.Length; j++)
                    if (PartTouches[j] != null) { leanUp[j] = PartTouches[j].Up; leanAt[j] = PartTouches[j].Where; PartTouches[j].Clear(); }
            for (int i = 0; i < 2; i++)
            {
                float up = touches[i].Up;
                Vector3 where = touches[i].Where;
                touches[i].Clear();
                if (ToeTouches != null && ToeTouches[i] != null) { up += ToeTouches[i].Up; where += ToeTouches[i].Where; ToeTouches[i].Clear(); }
                bears[i] = up / Mathf.Max(1e-6f, whole * g * step);
                leanUp[Foot + i] = up; leanAt[Foot + i] = where;
            }
            {
                float keeps = Mathf.Exp(-step / Mathf.Max(1e-3f, BalanceFeels));
                foreach (int j in Leans)
                {
                    float bearsIt = leanUp[j] / Mathf.Max(1e-6f, whole * g * step);
                    if (leanUp[j] > 0) leanFeltAt[j] = leanFelt[j] < 1e-4f ? leanAt[j] / leanUp[j] : Vector3.Lerp(leanAt[j] / leanUp[j], leanFeltAt[j], keeps);
                    leanFelt[j] = keeps * leanFelt[j] + (1 - keeps) * bearsIt;
                }
            }
            bore = bears[0] + bears[1];
            bool moving = false;
            for (int i = Hips; i <= Head && !moving; i++)
                moving = parts[i].GetPointVelocity(parts[i].worldCenterOfMass).sqrMagnitude > StillUnder * StillUnder || parts[i].angularVelocity.sqrMagnitude > StillTurning * StillTurning;
            still = moving ? 0 : still + step;
            if (GoesOnStill > 0)
            {
                float fastest = 0;
                for (int i = 0; i < Count; i++) fastest = Mathf.Max(fastest, parts[i].linearVelocity.magnitude);
                slowFor = fastest < GoesOnBelow ? slowFor + step : 0;
                movedIn |= fastest >= GoesOnBelow;
            }
            if (toned != Tone || strongerWas != StrongerWhileLearning) Springs(step);
            // (What learnt says of each joint, eased in.)
            {
                float k = 1 - Mathf.Exp(-step / Mathf.Max(1e-3f, DrivenEases));
                for (int j = 1; j < Count; j++)
                    drivenNow[j] = Vector3.Lerp(drivenNow[j], Driven != null && Driven.Length >= 3 * Count ? new Vector3(Driven[3 * j], Driven[3 * j + 1], Driven[3 * j + 2]) : Vector3.zero, k);
            }
            if (way == null && !Choose()) return Does.GaveUp;
            got += step; Lasted += step;
            if (float.IsNaN(stage.keeps)) leant = Mathf.MoveTowards(leant, 0, 60 * step);
            else leant = Mathf.Clamp(leant + Balances * (stage.keeps - Measure(Asks.Ahead)) * step, -BalancesAtMost, BalancesAtMost);
            if (!float.IsNaN(stage.sits)) sat = Mathf.Clamp(sat + stage.sits * Mathf.Clamp((Measure(Asks.Ahead) - stage.sitAhead) / .1f, -1, 1) * step, 0, stage.sitMost);
            bool through = got >= stage.atLeast * PaceOf(stage) || (way.gives && at == way.stages.Length - 1 && ReadyAfter > 0 && got >= ReadyAfter)
                || GoesOnStill > 0 && slowFor >= GoesOnStill && (GoesOnAfterMoving <= 0 || movedIn);
            for (int k = 0; through && k < stage.through.Length; k++)
            {
                float it = Measure(stage.through[k].what);
                through = it >= stage.through[k].least && it <= stage.through[k].most;
            }
            // (A foot being put somewhere: only once its boot bears on the ground.)
            int stepping = stage.steps < 0 ? -1 : stage.steps == 0 ? leads : 1 - leads;
            if (stepping >= 0 && bears[stepping] < .02f) through = false;
            if (stepping >= 0 && !float.IsNaN(stage.balance) && !(stepBegun && got - liftAt >= stage.over)) through = false;
            if (stage.letsHandsGo && !handsLet) through = false;
            if (through)
            {
                if (at + 1 < way.stages.Length) Begin(at + 1);
                else if (way.gives)
                {
                    int n = Supporting(false), m = Outline(sup, n);
                    HandedOver = Heading(m, "handed over");
                    return Does.Ready;
                }
                else if (!Choose()) return Does.GaveUp;
            }
            else if (got > stage.within) return Does.GaveUp;
            if (hipsBegan >= UpFrom && Measure(Asks.Hips) < DownUnder) downFor += step; else downFor = 0;
            if (downFor > DownFor) return Does.GaveUp;
            // The pose it is at, come to from where its joints were asked to be before.
            float share = stage.over > 0 ? Mathf.SmoothStep(0, 1, got / (stage.over * (PoseOvers.TryGetValue(stage.name, out float overs) ? overs : 1))) : 1;
            stepping = stage.steps < 0 ? -1 : stage.steps == 0 ? leads : 1 - leads;
            bool walks = !float.IsNaN(stage.handStep);
            walksBack = ReleaseWalks > 0 && ReleaseWhenOver && stage.letsHandsGo && !handsLet && !cannotWalkBack && !float.IsNaN(stage.balance)
                && (walkHand >= 0 || (ReleaseWaited > ReleaseWalkAfter && Measure(Asks.Ahead) > ReleaseAhead));
            walks |= walksBack;
            for (int j = 1; j < Count; j++)
            {
                if (stepping >= 0 && j >= Thigh && (j - Thigh) % 2 == stepping) continue;
                if (walks && j >= UpperArm && j < Thigh) continue;
                // (A leg whose hip's turn is not given is left as it is asked now: a foot that was put somewhere stays.)
                if (j >= Thigh && float.IsNaN(stage.pose[9 + 4 * ((j - Thigh) % 2 == leads ? 0 : 1)])) continue;
                aimNow[j] = Quaternion.Slerp(aimFrom[j], toStood[j] * Turn(j), share);
                Aims(j, aimNow[j]);
            }
            // (Its breath, on its trunk.)
            Aims(Trunk, aimNow[Trunk] * Quaternion.AngleAxis(-BreathBends, Vector3.right));
            if (stage.holds && !float.IsNaN(stage.balance) && !planting) Plant(-1, -1);
            // (Its weight off both hands first; then they let go.)
            if (walksBack && !WalkBack(step)) { cannotWalkBack = true; walksBack = false; }
            if (!walksBack && stage.letsHandsGo && !handsLet && !float.IsNaN(stage.balance) && ShiftedOff(-1, 2, step)) { handsLet = true; Plant(-1, -1); }
            if (ReleaseWhenOver && !handsLet && ReleaseWaited > ReleaseWaitsAtMost) return Does.GaveUp;
            if (stepping >= 0)
            {
                bool balancing = !float.IsNaN(stage.balance);
                // (Keeping its balance, its weight goes off that leg first; the leg is held as it is meanwhile.)
                if (stepBegun || !balancing || ShiftedOff(stepping, -1, step))
                {
                    if (!stepBegun)
                    {
                        stepBegun = true; liftAt = got; stepFrom = parts[Foot + stepping].transform.TransformPoint(soleAt[stepping]); stepBootFrom = parts[Foot + stepping].transform.rotation;
                        stepLies = Lies(); stepReach = (JointAt(Shin + stepping) - JointAt(Thigh + stepping)).magnitude + (JointAt(Foot + stepping) - JointAt(Shin + stepping)).magnitude;
                    }
                    float along = !balancing ? share : stage.over > 0 ? Mathf.SmoothStep(0, 1, (got - liftAt) / stage.over) : 1;
                    if (balancing && along < 1) { offLeg = stepping; offW = 0; }
                    // (Once it is down and bears, it is kept where it is with the rest that bears it.)
                    else if (balancing && !landed && leanFelt[Foot + stepping] >= .25f * BalanceBorne)
                    {
                        landed = true; Plant(-1, -1);
                        float forward = Vector3.Dot(Flat(parts[Foot + stepping].transform.TransformPoint(soleAt[stepping]) - stepFrom), stepLies);
                        StepFurthest = Mathf.Max(StepFurthest, forward / Mathf.Max(.05f, stepReach));
                    }
                    Steps(stepping, along, balancing);
                }
            }
            if (walks && !walksBack) WalksHands(step);
            if (stage.kneels > 0) Kneels(share, stepping);
            if (!float.IsNaN(stage.balance))
            {
                Balance(step);
                if (planting) HoldPlanted(step);
                bool shifting = (offLeg >= 0 && !stepBegun) || (offHand >= 0 && walkHand < 0);
                // (A limb goes slack only as fast as its weight really leaves it: an arm let go slack while it still
                // bore its share gave way, and the body fell on its face.)
                float still = 1;
                // (Arms only: a knee bears its share however slack its leg, and a leg kept stiff by its load held its
                // weight where it was; it goes slack by the clock, as before.)
                if (shifting && offHand >= 0 && limbAtShift > 1e-3f)
                {
                    float limbNow = offLeg >= 0 ? leanFelt[Thigh + offLeg] + leanFelt[Shin + offLeg] + leanFelt[Foot + offLeg]
                        : offHand == 2 ? leanFelt[Forearm] + leanFelt[Forearm + 1] : leanFelt[Forearm + offHand];
                    still = Mathf.Clamp01(limbNow / limbAtShift);
                }
                Slacken(shifting ? offLeg : -1, shifting ? offHand : -1, shifting ? Mathf.Max(offW, still) : 1);
            }
            else { BalanceDepth = float.NaN; Supports = 0; Slacken(-1, -1, 1); }
            return Does.Gathers;
        }

        // Where its weight is going.
        private Vector3 WeightGoes()
        {
            Vector3 v = Vector3.zero;
            for (int i = 0; i < Count; i++) v += parts[i].GetPointVelocity(parts[i].worldCenterOfMass) * masses[i];
            return v / whole;
        }

        // What bears it of what it leans on: each part the ground bears with a hundredth of its weight or more, where
        // (flat on the ground, the middle of where it touches), and how much of a share it is given (the limb it moves
        // its weight off: offW; its parts left out if that is nothing, or with all = false). How many.
        private int Supporting(bool all)
        {
            int n = 0;
            foreach (int j in Leans)
            {
                if (leanFelt[j] < BalanceBorne) continue;
                bool off = (offLeg >= 0 && (j == Thigh + offLeg || j == Shin + offLeg || j == Foot + offLeg)) || (offHand >= 0 && (j == Forearm + offHand || (offHand == 2 && j >= Forearm && j < Thigh)));
                // (The limb it moves its weight off is given what it really still bears, not what the clock says: given
                // nothing while its knee still bore a third of it, the others pushed as if it bore nothing, and pushed
                // its weight back onto it.)
                float w = off ? (all ? Mathf.Max(offW, OffLoad()) : 0) : 1;
                if (w <= 0) continue;
                Transform boot = j >= Foot ? parts[j].transform : null;
                if (boot != null && SoleSize != null && boot.up.y >= SoleFlat)
                {
                    Vector3 half = .5f * SoleSize[j - Foot];
                    for (int c = 0; c < 4; c++)
                    {
                        supAt[n] = boot.TransformPoint(new Vector3((c & 1) == 0 ? -half.x : half.x, -half.y, (c & 2) == 0 ? -half.z : half.z));
                        sup[n] = Flat(supAt[n]); supPart[n] = j; supW[n] = w; supOff[n] = off; n++;
                    }
                    continue;
                }
                supAt[n] = leanFeltAt[j]; sup[n] = Flat(leanFeltAt[j]); supPart[n] = j; supW[n] = w; supOff[n] = off; n++;
            }
            return n;
        }

        // The middle of what bears it, limb by limb (a boot's corners together are one): of all but the limb it moves its
        // weight off (full), or of all; of its boots only (boots).
        private Vector3 MiddleOf(int n, bool full, bool boots, out int limbs)
        {
            Vector3 sum = Vector3.zero;
            limbs = 0;
            for (int i = 0; i < n; i++)
            {
                if (full && supOff[i]) continue;
                if (boots && supPart[i] < Foot) continue;
                // (A limb's points together count once.)
                int same = 0; Vector3 at = Vector3.zero;
                bool first = true;
                for (int k = 0; k < n; k++)
                {
                    if (supPart[k] != supPart[i] || (full && supOff[k])) continue;
                    if (k < i) { first = false; break; }
                    at += sup[k]; same++;
                }
                if (!first) continue;
                sum += at / same; limbs++;
            }
            return limbs > 0 ? sum / limbs : Vector3.zero;
        }

        // The flat outline round some points (anticlockwise seen from above, as x and z), into outline: how many corners.
        private int Outline(Vector3[] points, int n)
        {
            for (int i = 0; i < n; i++) sorted[i] = points[i];
            for (int i = 1; i < n; i++)
                for (int k = i; k > 0 && (sorted[k].x < sorted[k - 1].x || (sorted[k].x == sorted[k - 1].x && sorted[k].z < sorted[k - 1].z)); k--)
                    (sorted[k], sorted[k - 1]) = (sorted[k - 1], sorted[k]);
            if (n < 3) { for (int i = 0; i < n; i++) outline[i] = sorted[i]; return n; }
            float Turns(Vector3 o, Vector3 a, Vector3 b) => (a.x - o.x) * (b.z - o.z) - (a.z - o.z) * (b.x - o.x);
            int m = 0;
            for (int i = 0; i < n; i++)
            {
                while (m >= 2 && Turns(outline[m - 2], outline[m - 1], sorted[i]) <= 0) m--;
                outline[m++] = sorted[i];
            }
            for (int i = n - 2, low = m + 1; i >= 0; i--)
            {
                while (m >= low && Turns(outline[m - 2], outline[m - 1], sorted[i]) <= 0) m--;
                outline[m++] = sorted[i];
            }
            return m - 1;
        }

        // How far a flat point is within the outline (metres; less than nothing outside: with one corner, how far it is
        // from it; with two, from the line between), and the nearest point on its edge.
        private float Within(int m, Vector3 p, out Vector3 nearest)
        {
            nearest = m > 0 ? outline[0] : p;
            if (m == 0) return float.NegativeInfinity;
            if (m == 1) return -(p - outline[0]).magnitude;
            float depth = float.PositiveInfinity, closest = float.PositiveInfinity;
            int edges = m == 2 ? 1 : m;
            for (int i = 0; i < edges; i++)
            {
                Vector3 a = outline[i], b = outline[(i + 1) % m], ab = b - a;
                float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(1e-9f, ab.sqrMagnitude));
                Vector3 q = a + t * ab;
                float far = (p - q).magnitude;
                if (far < closest) { closest = far; nearest = q; }
                if (m >= 3)
                {
                    Vector3 inward = new Vector3(-ab.z, 0, ab.x) / Mathf.Max(1e-6f, ab.magnitude);
                    depth = Mathf.Min(depth, Vector3.Dot(p - a, inward));
                }
            }
            return m >= 3 ? depth : -closest;
        }

        // The nearest place to a flat point as deep as d within the outline: on one of its edges' lines moved d inwards,
        // or where two of them cross (false if no place is that deep).
        private bool Nearest(int m, Vector3 p, float d, out Vector3 q)
        {
            q = p;
            if (m < 3) return false;
            Vector3 Inward(int i)
            {
                Vector3 ab = outline[(i + 1) % m] - outline[i];
                return new Vector3(-ab.z, 0, ab.x) / Mathf.Max(1e-6f, ab.magnitude);
            }
            bool Deep(Vector3 x)
            {
                for (int i = 0; i < m; i++) if (Vector3.Dot(x - outline[i], Inward(i)) < d - 1e-4f) return false;
                return true;
            }
            float best = float.PositiveInfinity;
            for (int i = 0; i < m; i++)
            {
                Vector3 ni = Inward(i);
                Vector3 x = p + (d - Vector3.Dot(p - outline[i], ni)) * ni;
                if ((x - p).sqrMagnitude < best && Deep(x)) { best = (x - p).sqrMagnitude; q = x; }
                for (int k = i + 1; k < m; k++)
                {
                    Vector3 nk = Inward(k);
                    float det = ni.x * nk.z - ni.z * nk.x;
                    if (Mathf.Abs(det) < 1e-6f) continue;
                    float ci = ni.x * outline[i].x + ni.z * outline[i].z + d, ck = nk.x * outline[k].x + nk.z * outline[k].z + d;
                    Vector3 y = new Vector3((ci * nk.z - ck * ni.z) / det, p.y, (ni.x * ck - nk.x * ci) / det);
                    if ((y - p).sqrMagnitude < best && Deep(y)) { best = (y - p).sqrMagnitude; q = y; }
                }
            }
            return best < float.PositiveInfinity;
        }

        // (What it was like when both hands let go of the ground, and when it was handed to its standing balance: told,
        // for the bench. October 22.)
        public string Released { get; private set; } = "";
        public string HandedOver { get; private set; } = "";
        // What it senses, for what learns (Senses numbers, each roughly between -1 and 1): its trunk's up and forward, the
        // way it lies and across (its leading side positive); its hips' and head's height; where its weight is from the
        // middle of its soles and how it goes (the way it lies, across, up); how deep within what bears it its weight is,
        // and where it is heading; how far ahead of its soles, how flat they lie, what its boots bear; what each hand,
        // knee and boot bears; whether its hands have let go; at which of the stages it learns at it is, and how far
        // into it; how much it has waited to let go.
        public void Sense(System.Collections.Generic.List<float> into)
        {
            Vector3 lies = Lies(), across = Vector3.Cross(Vector3.up, lies) * (leads == 1 ? 1 : -1);
            float L(Vector3 v) => Vector3.Dot(v, lies);
            float A(Vector3 v) => Vector3.Dot(v, across);
            Vector3 up = parts[Trunk].transform.up, ahead = parts[Trunk].transform.forward;
            into.Add(L(up)); into.Add(A(up)); into.Add(up.y);
            into.Add(L(ahead)); into.Add(A(ahead)); into.Add(ahead.y);
            into.Add(Mathf.Clamp(Measure(Asks.Hips), 0, 1.5f)); into.Add(Mathf.Clamp(Measure(Asks.Head), 0, 1.5f));
            Vector3 feet = .5f * (parts[Foot].transform.TransformPoint(soleAt[0]) + parts[Foot + 1].transform.TransformPoint(soleAt[1]));
            Vector3 off = Flat(Weight() - feet) / tall, goes = WeightGoes();
            into.Add(Mathf.Clamp(L(off), -2, 2)); into.Add(Mathf.Clamp(A(off), -2, 2));
            into.Add(Mathf.Clamp(L(goes), -2, 2)); into.Add(Mathf.Clamp(A(goes), -2, 2)); into.Add(Mathf.Clamp(goes.y, -2, 2));
            float depth = float.IsNaN(BalanceDepth) ? -.2f : Mathf.Clamp(BalanceDepth, -.2f, .2f);
            into.Add(depth * 5);
            {
                int n = Supporting(false), m = Outline(sup, n);
                Vector3 w = Weight();
                float high = Mathf.Max(.05f, w.y - GroundAt(w));
                Vector3 heads = Flat(w) + Flat(goes) * Mathf.Sqrt(high / Physics.gravity.magnitude);
                float d = n > 0 ? Within(m, heads, out _) : -.2f;
                into.Add(Mathf.Clamp(float.IsInfinity(d) || float.IsNaN(d) ? -.2f : d, -.2f, .2f) * 5);
            }
            into.Add(Mathf.Clamp(Measure(Asks.Ahead), -2, 2)); into.Add(Measure(Asks.Soles)); into.Add(Mathf.Clamp(bore, 0, 1.5f));
            for (int i = 0; i < 2; i++) into.Add(Mathf.Clamp(leanFelt[Forearm + i], 0, 1.5f));
            for (int i = 0; i < 2; i++) into.Add(Mathf.Clamp(leanFelt[Thigh + i] + leanFelt[Shin + i], 0, 1.5f));
            for (int i = 0; i < 2; i++) into.Add(Mathf.Clamp(leanFelt[Foot + i], 0, 1.5f));
            into.Add(handsLet ? 1 : 0);
            string now = stage != null ? stage.name : "";
            into.Add(now == "footB" ? 1 : 0); into.Add(now == "pike" ? 1 : 0); into.Add(now == "rollup" ? 1 : 0);
            into.Add(stage != null ? Mathf.Clamp01(got / Mathf.Max(.1f, stage.atLeast * PaceOf(stage))) : 0);
            into.Add(Mathf.Clamp01(ReleaseWaited / Mathf.Max(.1f, ReleaseWaitsAtMost)));
        }

        // (Where its weight is heading for: its capture point, its weight's place and its speed times the root of its
        // height over gravity, flat.)
        private string Heading(int m, string what)
        {
            Vector3 w = Weight(), v = WeightGoes();
            float high = Mathf.Max(.05f, w.y - GroundAt(w));
            Vector3 heads = Flat(w) + Flat(v) * Mathf.Sqrt(high / Physics.gravity.magnitude);
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0} at {1:0.00} s of {2}: weight {3:0} mm within, heading {4:0} mm within; moving {5:0.00} m/s ({6:0.00} up); hands bore {7:0.00}, boots {8:0.00}",
                what, got, stage != null ? stage.name : "", Within(m, Flat(w), out _) * 1000, Within(m, heads, out _) * 1000, Flat(v).magnitude, v.y,
                leanFelt[Forearm] + leanFelt[Forearm + 1], bore);
        }

        // (Both hands let go only once its weight is over the rest, never all the same: every try that let go with its
        // weight outside its boots failed, October 22. False: as before.)
        public static bool ReleaseWhenOver = true;

        // ---- The third path (Luis, October 22: a learned controller for the moment from hands and feet to the squat;
        // help only while it learns, none in the game). What learns sets Learned in the stages it learns at, from what
        // Sense tells it; its own joints do all of it: the pose's trunk bow, head bow and twist turned so much more
        // (degrees); its hips held so much higher (a share of their standing height; held where they were kept, if the
        // pose does not say how high); where its weight is meant to be moved so far (metres, the way it lies, and across
        // it, its leading side positive). Null: as searched.
        // (And where a foot is put: so much further ahead of its hip and out from it, shares of the leg's length; the
        // third version, October 23.)
        public const int Learns = 8;
        public float[] Learned;
        // (Whether its hands have let go of the ground in this pose: for what learns.)
        public bool HandsLet => handsLet;
        // (The stages what learns acts in.)
        public static bool LearnsAt(string stage) => stage == "footB" || stage == "pike" || stage == "rollup";
        // How many numbers Sense tells.
        public const int Senses = 30;
        // (What learns may also pace its way, Luis, October 27: each pose is kept at least so many times as long as its
        // way says before it goes on to the next, in every pose of the way; how fast each pose is come to is as searched.
        // The searched times held it still in the squat and lying with a boot raised, 1.4 to 2.8 s, which read as
        // scripted to all three judges. 1: as searched.)
        public float Pace = 1;
        // (And a pose by its name kept so many times as long as its way says, for every body, -risePoses hands=0.5: to
        // learn how much standing asks of each pose's time, October 28. Empty: as searched.)
        public static readonly System.Collections.Generic.Dictionary<string, float> PosePaces = new System.Collections.Generic.Dictionary<string, float>();
        private float PaceOf(Stage of) => Pace * (of != null && PosePaces.TryGetValue(of.name, out float times) ? times : 1);
        // (And come to in so many times as long, -risePoseOvers hands=0.5.)
        public static readonly System.Collections.Generic.Dictionary<string, float> PoseOvers = new System.Collections.Generic.Dictionary<string, float>();

        // ---- Option 1 (Luis, October 26: "option 1"): what learns sets every joint's target itself at the hard moment.
        // Driven: for each joint, so many degrees more about each of its axes than its way asks (three a joint, its hinge's
        // first only for a hinge; Count of them, the hips' and the head's unused), eased in over DrivenEases seconds. Null:
        // as searched. Its own joints do all of it.
        public float[] Driven;
        public static float DrivenEases = .08f;
        private readonly Vector3[] drivenNow = new Vector3[Count], wayAsks = new Vector3[Count];
        // (The joints it sets: all but the hips (the root) and the head; a forearm's and a shin's are hinges.)
        public static readonly int[] DrivenJoints = { Trunk, UpperArm, UpperArm + 1, Forearm, Forearm + 1, Thigh, Thigh + 1, Shin, Shin + 1, Foot, Foot + 1 };
        public static int AxesOf(int j) => j == Forearm || j == Forearm + 1 || j == Shin || j == Shin + 1 ? 1 : 3;
        public const int Drives = 25;
        // What it senses of its joints, for what sets them (JointSenses numbers): of each it sets, how far it is turned
        // from how it was made (radians), how fast (tenths of radians a second) and what its way asks of it (radians); and
        // how its hips go and turn (the way it lies, across it, up).
        public const int JointSenses = 3 * Drives + 6;
        public void SenseJoints(System.Collections.Generic.List<float> into)
        {
            foreach (int j in DrivenJoints)
            {
                var p = parts[j].jointPosition;
                for (int d = 0; d < AxesOf(j); d++) into.Add(d < p.dofCount ? Mathf.Clamp(p[d], -Mathf.PI, Mathf.PI) : 0);
            }
            foreach (int j in DrivenJoints)
            {
                var v = parts[j].jointVelocity;
                for (int d = 0; d < AxesOf(j); d++) into.Add(d < v.dofCount ? Mathf.Clamp(.1f * v[d], -3, 3) : 0);
            }
            foreach (int j in DrivenJoints)
                for (int d = 0; d < AxesOf(j); d++) into.Add(Mathf.Clamp(wayAsks[j][d] * Mathf.Deg2Rad, -Mathf.PI, Mathf.PI));
            Vector3 lies = Lies(), across = Vector3.Cross(Vector3.up, lies) * (leads == 1 ? 1 : -1);
            Vector3 goes = parts[Hips].GetPointVelocity(parts[Hips].worldCenterOfMass), turns = parts[Hips].angularVelocity;
            into.Add(Mathf.Clamp(Vector3.Dot(goes, lies), -3, 3)); into.Add(Mathf.Clamp(Vector3.Dot(goes, across), -3, 3)); into.Add(Mathf.Clamp(goes.y, -3, 3));
            into.Add(Mathf.Clamp(.2f * Vector3.Dot(turns, lies), -3, 3)); into.Add(Mathf.Clamp(.2f * Vector3.Dot(turns, across), -3, 3)); into.Add(Mathf.Clamp(.2f * turns.y, -3, 3));
        }
        // Only while it is taught, never in the game (Luis, October 22: help that exists only while it learns): its joints
        // so many times as strong as they are. RiseSchool sets it, fading to 1 by the trainer's lessons.
        public static float StrongerWhileLearning = 1;
        private float strongerWas = 1;
        // (Raised by its standing balance after its own getting up, its trunk and head come upright only after so much of
        // the rise, kept forward over its feet while its legs straighten: all together, four of nine that came onto their
        // feet fell as they rose. 0: as before. October 22.)
        // (Of those handed over, stood: 54% all together; trunk after 0.25 of the rise 57%, after 0.4 79%, after 0.6 71%;
        // 72 chance starts each.)
        public static float RiseTrunkAfter = .4f;
        // (Tried and taken out, October 22: its joints held as its getting up held them and eased to how softly it stands
        // over the rise. Held so, 1 of 72 stood, against 26 with the legs first alone: its standing balance needs its
        // softness to balance with.)
        // (Waited so long without its weight coming over, it gives that try up, and tries again, rather than wait out the
        // pose's time.)
        public static float ReleaseWaitsAtMost = 4;
        // (Waiting so long with its weight ahead of its boots, more than ReleaseAhead of its hips' height, it walks the hand
        // further from its feet ReleaseWalks metres nearer them, and tries again; none nearer its feet than ReleaseNearest
        // of a leg's length. ReleaseWalks 0: not. Off: with it, 15 of 72 stood, without it 13, the hands let go in 32 and 33;
        // its weight was 0.2 to 1 hip's height ahead of its boots, more than a hand's walk brings back. October 22.)
        public static float ReleaseWalkAfter = .6f, ReleaseAhead = .02f, ReleaseWalks = 0, ReleaseNearest = .3f;
        private bool walksBack, cannotWalkBack;
        // (How long it has been moving its weight off both hands without its weight coming over the rest.)
        public float ReleaseWaited { get; private set; }

        // Before it lifts a limb, its weight goes over the others: true once it is (or it has tried long enough).
        private bool ShiftedOff(int leg, int hand, float step)
        {
            offLeg = leg; offHand = hand;
            if (shiftT <= 0)
            {
                Plant(leg, hand);
                limbAtShift = leg >= 0 ? leanFelt[Thigh + leg] + leanFelt[Shin + leg] + leanFelt[Foot + leg] : hand == 2 ? leanFelt[Forearm] + leanFelt[Forearm + 1] : leanFelt[Forearm + hand];
            }
            shiftT += step;
            offW = Mathf.Clamp01(1 - shiftT / Mathf.Max(.05f, BalanceUnloads));
            int n = Supporting(false), m = Outline(sup, n);
            float depth = Within(m, Flat(Weight()), out _);
            float limb = leg >= 0 ? leanFelt[Thigh + leg] + leanFelt[Shin + leg] + leanFelt[Foot + leg] : hand == 2 ? leanFelt[Forearm] + leanFelt[Forearm + 1] : leanFelt[Forearm + hand];
            // (Its weight over the others is what counts: a knee on the ground bears its share however slack its leg,
            // until it is lifted.)
            bool over = depth >= Mathf.Min(Mathf.Max(BalanceMargin, .75f * BalanceShiftsDeep), .75f * shiftDeepNow) && (limb < BalanceLifts || shiftT >= BalanceUnloads) && shiftT >= .5f * BalanceUnloads;
            if (hand == 2) ReleaseWaited = over ? 0 : ReleaseWaited + step;
            if (!over && (shiftT < BalanceShiftsWithin || (hand == 2 && ReleaseWhenOver))) return false;
            if (over) Shifts++; else ShiftsFailed++;
            if (hand == 2) Released = Heading(m, over ? "hands let go, its weight over the rest" : "hands let go all the same") + string.Format(System.Globalization.CultureInfo.InvariantCulture, " (they bore {0:0.00} as it began)", limbAtShift);
            shiftT = 0;
            return true;
        }

        // What bears it, of its hands and boots (but the limb it moves its weight off), is kept where it is now.
        private void Plant(int leg, int hand)
        {
            planting = true;
            for (int i = 0; i < 2; i++)
            {
                plantedHand[i] = (hand == 2 || i != hand) && !handsLet && leanFelt[Forearm + i] >= BalanceBorne && !stage.holdsFeet;
                if (plantedHand[i])
                {
                    plantHand[i] = HandEnd(i);
                    // (An arm nearly straight when its hand is put is kept straight. One bent a little, asked to be
                    // straight, pushed its shoulder up and tipped it over its hands.)
                    float upper = (JointAt(Forearm + i) - JointAt(UpperArm + i)).magnitude, fore = (HandEnd(i) - JointAt(Forearm + i)).magnitude;
                    plantedStraight[i] = (plantHand[i] - JointAt(UpperArm + i)).magnitude >= ArmStraight * (upper + fore);
                }
                plantedLeg[i] = i != leg && (leanFelt[Foot + i] >= BalanceBorne || (landed && stage.steps >= 0 && i == (stage.steps == 0 ? leads : 1 - leads) && leanFelt[Foot + i] > 0));
                if (plantedLeg[i]) { plantSole[i] = parts[Foot + i].transform.TransformPoint(soleAt[i]); plantBoot[i] = parts[Foot + i].transform.rotation; }
                // (A leg that bears it on its knee, not its boot: its knee kept where it is.)
                plantedKnee[i] = i != leg && !plantedLeg[i] && leanFelt[Thigh + i] + leanFelt[Shin + i] >= BalanceBorne;
                if (plantedKnee[i]) plantKnee[i] = JointAt(Shin + i);
            }
            bodyTo = Flat(Weight());
            highTo = parts[Hips].transform.position.y;
            hipsAtPlant = parts[Hips].transform.position;
            // (Its hips kept turned as they are, but level across.)
            Transform hips = parts[Hips].transform;
            Vector3 level = Flat(hips.right);
            hipsHeld = level.sqrMagnitude > 1e-6f ? Quaternion.FromToRotation(hips.right, level.normalized) * hips.rotation : hips.rotation;
        }

        // A leg's thigh asked to point at a place for its knee, from a hip at a place, of hips turned so; its shin and
        // boot left as asked.
        private void KneeFrom(int leg, Vector3 hip, Quaternion hips, Vector3 knee)
        {
            Transform thigh = parts[Thigh + leg].transform;
            Vector3 now = JointAt(Shin + leg) - JointAt(Thigh + leg), want = knee - hip;
            if (now.sqrMagnitude < 1e-8f || want.sqrMagnitude < 1e-8f) return;
            Quaternion turned = Quaternion.FromToRotation(now, want) * thigh.rotation;
            aimNow[Thigh + leg] = Quaternion.Inverse(hips * rested[Thigh + leg]) * turned;
            Aims(Thigh + leg, aimNow[Thigh + leg]);
        }

        // Its body asked to be over where its weight is meant to be, its hands and boots kept where they are.
        private void HoldPlanted(float step)
        {
            Vector3 at = Flat(Weight());
            bodyTo = Vector3.MoveTowards(bodyTo, Flat(weightTo), BalanceShiftsAt * step);
            wanderAlong += step / Mathf.Max(.2f, WandersEvery);
            if (wanderAlong >= 1)
            {
                wanderAlong = 0; wanderFrom = wander;
                float turn = WanderChance() * 2 * Mathf.PI, far = WandersBy * Mathf.Sqrt(WanderChance());
                wanderTo = new Vector3(Mathf.Cos(turn), 0, Mathf.Sin(turn)) * far;
            }
            wander = Vector3.Lerp(wanderFrom, wanderTo, Mathf.SmoothStep(0, 1, wanderAlong));
            holdMore = Vector3.ClampMagnitude(holdMore + BalanceHoldLearns * Flat(bodyTo + wander - at) * step, BalanceShiftsAtMost);
            Vector3 by = Vector3.ClampMagnitude(bodyTo + wander - at - BalanceDamps * Flat(WeightGoes()) + holdMore, BalanceShiftsAtMost);
            // (Its boots let down flat about their toes, if the pose says.)
            if (stage.flattens > 0 && SoleSize != null)
                for (int i = 0; i < 2; i++)
                {
                    if (!plantedLeg[i] || i == offLeg) continue;
                    Quaternion flat = Quaternion.FromToRotation(plantBoot[i] * Vector3.up, Vector3.up) * plantBoot[i];
                    Quaternion turned = Quaternion.RotateTowards(plantBoot[i], flat, stage.flattens * step);
                    Vector3 toe = new Vector3(0, 0, .5f * SoleSize[i].z) + Vector3.Scale(soleAt[i], new Vector3(1, 1, 0));
                    Vector3 toeAt = plantSole[i] + plantBoot[i] * (toe - soleAt[i]);
                    plantSole[i] = toeAt - turned * (toe - soleAt[i]);
                    plantBoot[i] = turned;
                }
            // Where its hips are asked to be: there; so high, if the pose says; and turned as they were, level across.
            Transform hips = parts[Hips].transform, trunk = parts[Trunk].transform;
            float up = 0;
            float holdHigh = stage.holdHigh;
            // (What learns moves how high they are held only where the stage holds them. It used to hold them where they
            // were planted where the stage does not, even saying nothing: so, in the second step and the roll-up, the hips
            // were held down as they should rise: what learns, saying nothing, stood 26 of 400 falls in the teaching, and
            // 37 and 38 of 400 without that hold. October 27.)
            if (Learned != null && !float.IsNaN(holdHigh)) holdHigh += Learned[3];
            if (!float.IsNaN(holdHigh))
            {
                highTo = Mathf.MoveTowards(highTo, GroundAt(hips.position) + holdHigh * tall, stage.holdRate * tall * step);
                up = Mathf.Clamp(highTo - hips.position.y - BalanceDamps * parts[Hips].velocity.y, -BalanceShiftsAtMost, BalanceShiftsAtMost);
            }
            Vector3 hipsTo = hips.position + by + Vector3.up * up;
            askedBy = by;
            // (Turned as they were when what bears it was put: nothing else keeps its hips from pitching or rolling,
            // its limbs reaching for where they are kept from wherever its hips are.)
            // (Only once no hand is kept on the ground: turned upright with its hands still down, its arms held its
            // shoulders where they were and fought the turn.)
            if (stage.risesHips > 0 && !plantedHand[0] && !plantedHand[1])
            {
                Vector3 faces = Flat(Lies());
                if (faces.sqrMagnitude > 1e-6f) hipsHeld = Quaternion.RotateTowards(hipsHeld, Quaternion.LookRotation(faces.normalized, Vector3.up) * StoodHips, stage.risesHips * step);
            }
            Quaternion hipsTurn = hipsHeld;
            Quaternion fromHips = Quaternion.Inverse(hips.rotation);
            Vector3 There(Vector3 point) => hipsTo + hipsTurn * (fromHips * (point - hips.position));
            Quaternion trunkTurn = hipsTurn * rested[Trunk] * aimNow[Trunk];
            Vector3 waist = There(JointAt(Trunk));
            for (int i = 0; i < 2; i++)
            {
                if (plantedHand[i] && i != offHand)
                    ArmFrom(i, waist + trunkTurn * (Quaternion.Inverse(trunk.rotation) * (JointAt(UpperArm + i) - JointAt(Trunk))), trunkTurn, plantHand[i], plantedStraight[i]);
                if (i == offLeg) continue;
                if (plantedLeg[i]) LegFrom(i, There(JointAt(Thigh + i)), hipsTurn, plantSole[i], plantBoot[i]);
                else if (plantedKnee[i]) KneeFrom(i, There(JointAt(Thigh + i)), hipsTurn, plantKnee[i]);
            }
        }

        // The ground's push shared between what bears it: shares that add up to one, whose middle is at cop, as even as
        // they can be (each by how much of a share it is given), none less than nothing.
        private void Shares(int n, Vector3 cop)
        {
            var w = shareW;
            for (int i = 0; i < n; i++) { w[i] = supW[i]; supF[i] = 0; }
            for (int round = 0; round <= n; round++)
            {
                double m00 = 0, m01 = 0, m02 = 0, m11 = 0, m12 = 0, m22 = 0, sw = 0;
                for (int i = 0; i < n; i++)
                {
                    double dx = sup[i].x - cop.x, dz = sup[i].z - cop.z, wi = w[i];
                    m00 += wi; m01 += wi * dx; m02 += wi * dz; m11 += wi * dx * dx; m12 += wi * dx * dz; m22 += wi * dz * dz; sw += wi;
                }
                if (sw <= 0) break;
                m11 += 1e-4 * sw; m22 += 1e-4 * sw;
                // (Three sums in three unknowns, by Cramer: [m00 m01 m02; m01 m11 m12; m02 m12 m22] l = [1 0 0].)
                double det = m00 * (m11 * m22 - m12 * m12) - m01 * (m01 * m22 - m12 * m02) + m02 * (m01 * m12 - m11 * m02);
                if (System.Math.Abs(det) < 1e-12) break;
                double l0 = (m11 * m22 - m12 * m12) / det, l1 = -(m01 * m22 - m02 * m12) / det, l2 = (m01 * m12 - m02 * m11) / det;
                int worst = -1; float least = 0;
                for (int i = 0; i < n; i++)
                {
                    supF[i] = (float)(w[i] * (l0 + l1 * (sup[i].x - cop.x) + l2 * (sup[i].z - cop.z)));
                    if (supF[i] < least) { least = supF[i]; worst = i; }
                }
                if (worst < 0) break;
                w[worst] = 0;
            }
            float sum = 0;
            for (int i = 0; i < n; i++) { supF[i] = Mathf.Max(0, supF[i]); sum += supF[i]; }
            if (sum > 1e-6f) for (int i = 0; i < n; i++) supF[i] /= sum;
            else
            {
                // (Nothing to share it by: the nearest takes it.)
                int nearest = 0;
                for (int i = 1; i < n; i++) if ((sup[i] - cop).sqrMagnitude < (sup[nearest] - cop).sqrMagnitude) nearest = i;
                for (int i = 0; i < n; i++) supF[i] = i == nearest ? 1 : 0;
            }
        }

        // A joint's turn now, in its own frame, from how it was made (as its aim is).
        private Quaternion JointNow(int j) => Quaternion.Inverse(parts[from[j]].transform.rotation * rested[j]) * parts[j].transform.rotation;

        // One step of its balance.
        private void Balance(float step)
        {
            float g = Physics.gravity.magnitude;
            int n = Supporting(true);
            Supports = n;
            Vector3 c = Weight(), at = Flat(c);
            if (n == 0) { BalanceDepth = float.NaN; return; }
            {
                int all = Outline(sup, n);
                BalanceDepth = Within(all, at, out _);
            }
            // Where its weight is meant to be: over the middle of what bears it (moving off a limb: of the others);
            // or so far towards over its boots; or, keeping its weight over its feet, there.
            Vector3 to = MiddleOf(n, true, false, out int fullLimbs), boots = MiddleOf(n, false, true, out int onBoots);
            int full = 0;
            for (int i = 0; i < n; i++) if (!supOff[i]) full++;
            if (fullLimbs == 0) to = MiddleOf(n, false, false, out _);
            bool moving = offLeg >= 0 || offHand >= 0;
            // (Moving off a limb: only as far as over the others, by twice the margin, from where it is towards their
            // middle.)
            if (full >= 3 && (moving || BalanceLeastMove))
            {
                int others = 0;
                for (int i = 0; i < n; i++) if (!supOff[i]) sorted[others++] = sup[i];
                int m0 = Outline(sorted, others);
                float now = Within(m0, at, out _);
                // (As deep as the outline allows, if it is thinner than that.)
                float deep = ShiftDeep;
                Vector3 near = at;
                bool found = false;
                if (BalanceShiftsNearest)
                    for (int k = 0; k < 8 && !(found = Nearest(m0, at, deep, out near)); k++) deep *= .7f;
                shiftDeepNow = found ? deep : ShiftDeep;
                if (found) to = now < deep ? near : at;
                else if (now < ShiftDeep)
                {
                    // (Along the line to their middle, the nearest place that deep.)
                    float lo = 0, hi = 1;
                    for (int k = 0; k < 12; k++)
                    {
                        float mid = .5f * (lo + hi);
                        if (Within(m0, Vector3.Lerp(at, to, mid), out _) >= ShiftDeep) hi = mid; else lo = mid;
                    }
                    to = Vector3.Lerp(at, to, hi);
                }
                else to = at;
            }
            if (!moving && onBoots > 0)
            {
                Vector3 toes = Flat(parts[Foot].transform.forward + parts[Foot + 1].transform.forward);
                Vector3 feet = Flat(.5f * (parts[Foot].transform.TransformPoint(soleAt[0]) + parts[Foot + 1].transform.TransformPoint(soleAt[1])));
                if (!float.IsNaN(stage.keeps) && onBoots == 2 && toes.sqrMagnitude > 1e-6f) to = feet + toes.normalized * (stage.keeps * tall);
                else if (stage.balanceFeet > 0) to = Vector3.Lerp(to, boots, stage.balanceFeet);
                if (stage.balanceLead > 0)
                {
                    // (The boot furthest ahead, the way it lies.)
                    Vector3 lies = Lies();
                    int front = -1; float furthest = float.NegativeInfinity;
                    for (int i = 0; i < 2; i++)
                    {
                        if (leanFelt[Foot + i] < BalanceBorne) continue;
                        float ahead = Vector3.Dot(Flat(parts[Foot + i].transform.TransformPoint(soleAt[i])), lies);
                        if (ahead > furthest) { furthest = ahead; front = i; }
                    }
                    if (front >= 0) to = Vector3.Lerp(to, Flat(parts[Foot + front].transform.TransformPoint(soleAt[front])), stage.balanceLead);
                }
            }
            if (Learned != null)
            {
                Vector3 lies = Lies();
                to += lies * Learned[4] + Vector3.Cross(Vector3.up, lies) * (Learned[5] * (leads == 1 ? 1 : -1));
            }
            weightTo = to;
            // What would bring it there without overshoot, with what it has learnt its pose holds back; and so where the
            // ground must push, within what bears it.
            Vector3 short_ = to - at;
            leansMore = Vector3.ClampMagnitude(leansMore + BalanceLeans * short_ * step, BalanceLeansAtMost);
            float high = Mathf.Max(.1f, c.y - GroundAt(c));
            Vector3 wants = BalanceQuick * BalanceQuick * (short_ + leansMore) - 2 * BalanceQuick * Flat(WeightGoes());
            Vector3 cop = at - wants * (high / g);
            int m = Outline(sup, n);
            float inside = Within(m, cop, out Vector3 edge);
            if (inside < BalanceMargin)
            {
                if (m >= 3)
                {
                    Vector3 middle = Vector3.zero;
                    for (int i = 0; i < m; i++) middle += outline[i];
                    middle /= m;
                    Vector3 inward = middle - edge;
                    float room = inward.magnitude;
                    cop = room > 1e-5f ? edge + inward / room * Mathf.Min(BalanceMargin, .5f * room) : edge;
                }
                else cop = edge;
            }
            Vector3 sideways = BalancePushes * Vector3.ClampMagnitude((at - cop) * (g / high), BalanceSlides * g);
            Shares(n, cop);
            // What each joint between there and its hips gives to make the ground push so.
            for (int j = 0; j < Count; j++) torque[j] = Vector3.zero;
            Vector3 push = stage.balance * whole * (sideways + Vector3.up * g);
            for (int i = 0; i < n; i++)
            {
                Vector3 pushes = supF[i] * push, where = supAt[i];
                for (int j = supPart[i]; j != Hips; j = from[j]) torque[j] -= Vector3.Cross(where - JointAt(j), pushes);
            }
            for (int j = 1; j < Count; j++)
            {
                if (torque[j] == Vector3.zero) continue;
                // (A hinge gives only about its own line.)
                if (hinge[j]) { Vector3 line = parts[j].transform.right; torque[j] = line * Vector3.Dot(line, torque[j]); }
                // (No more than it has left over from holding its pose.)
                float holds = Mathf.Min(hasOf[j], springOf[j] * Quaternion.Angle(JointNow(j), aimNow[j]) * Mathf.Deg2Rad);
                torque[j] = Vector3.ClampMagnitude(torque[j], Mathf.Max(0, hasOf[j] - holds));
                if (float.IsNaN(torque[j].x) || float.IsNaN(torque[j].y) || float.IsNaN(torque[j].z)) continue;
                // Equal and opposite, on the two parts the joint joins.
                parts[j].AddTorque(torque[j]);
                parts[from[j]].AddTorque(-torque[j]);
            }
        }

        // What it does now, for whoever tells of it.
        // (Where its weight is from the middle of what bears it, and from where it is meant to be: mm along the way it
        // lies, and to its right.)
        private string Where()
        {
            int n = Supporting(true);
            if (n == 0) return "";
            Vector3 middle = Vector3.zero;
            for (int i = 0; i < n; i++) middle += sup[i];
            middle /= n;
            Vector3 lies = Lies(), right = Vector3.Cross(Vector3.up, lies), at = Flat(Weight());
            // (Its elbows' and knees' bend now and as asked, degrees; its shoulders' and hips' turn from as asked.)
            float Bend(int j) => Vector3.Angle(parts[from[j]].transform.up, parts[j].transform.up);
            float Off(int j) => Quaternion.Angle(JointNow(j), aimNow[j]);
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "; weight from their middle {0:0} ahead {1:0} right, to go {2:0} ahead {3:0} right; head {4:0.00}, hips {5:0.00} high; elbows {6:0} {7:0} (off {8:0} {9:0}), shoulders off {10:0} {11:0}, knees {12:0} {13:0} (off {14:0} {15:0}), hips off {16:0} {17:0}",
                Vector3.Dot(at - middle, lies) * 1000, Vector3.Dot(at - middle, right) * 1000, Vector3.Dot(Flat(weightTo) - at, lies) * 1000, Vector3.Dot(Flat(weightTo) - at, right) * 1000, Measure(Asks.Head), Measure(Asks.Hips),
                Bend(Forearm), Bend(Forearm + 1), Off(Forearm), Off(Forearm + 1), Off(UpperArm), Off(UpperArm + 1), Bend(Shin), Bend(Shin + 1), Off(Shin), Off(Shin + 1), Off(Thigh), Off(Thigh + 1));
        }

        // Ways as written (each miner's in Resources/RiseWays, a bench's in its file). A line "way <name> <front least>
        // <front most> gives|turns", then a line for each pose:
        //   name | over atLeast within | trunk head twist | shoulder out elbow | shoulder out elbow |
        //   hip out knee ankle | hip out knee ankle | soles>0.9 bears>0.8 ahead<0.05 keep=0.02
        // (the leading side first; turns from standing, in degrees). Read here, so the game and a bench read them alike.
        private static readonly System.Globalization.CultureInfo Plain = System.Globalization.CultureInfo.InvariantCulture;
        public static Way[] Read(string text)
        {
            var ways = new System.Collections.Generic.List<Way>();
            var stages = new System.Collections.Generic.List<Stage>();
            Way way = null;
            void Close() { if (way != null) { way.stages = stages.ToArray(); ways.Add(way); stages.Clear(); } }
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                if (line.StartsWith("way "))
                {
                    Close();
                    var w = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    way = new Way { name = w[1], frontLeast = float.Parse(w[2], Plain), frontMost = float.Parse(w[3], Plain), gives = w.Length > 4 && w[4] == "gives" };
                    continue;
                }
                var f = line.Split('|');
                float[] N(string s) => System.Array.ConvertAll(s.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries), x => float.Parse(x, Plain));
                var times = N(f[1]);
                var stage = new Stage { name = f[0].Trim(), over = times[0], atLeast = times[1], within = times[2] };
                var numbers = new System.Collections.Generic.List<float>();
                for (int k = 2; k <= 6; k++) numbers.AddRange(N(f[k]));
                if (numbers.Count != Turns) throw new System.FormatException("A pose of " + stage.name + " has " + numbers.Count + " turns, not " + Turns + ".");
                stage.pose = numbers.ToArray();
                var asked = new System.Collections.Generic.List<Asked>();
                if (f.Length > 7)
                    foreach (string word in f[7].Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (word.StartsWith("keep=")) { stage.keeps = float.Parse(word.Substring(5), Plain); continue; }
                        // (bal=1,0.3: it keeps its weight over what bears it, carrying so much of it by the balance, so far
                        // from over the middle of them towards over its boots.)
                        // (hold=0.6,0.3: it holds itself on what bears it, its hips asked to be so high (NaN: as they are), going
                        // there at so much of their height a second.)
                        // (flat=30: its boots kept on the ground let down flat about their toes, so many degrees a second.)
                        if (word.StartsWith("flat=")) { stage.flattens = float.Parse(word.Substring(5), Plain); continue; }
                        // (hands=off: its weight off both hands, then they let go. rights=40: its hips turned towards standing.)
                        if (word == "hands=off") { stage.letsHandsGo = true; continue; }
                        if (word.StartsWith("rights=")) { stage.risesHips = float.Parse(word.Substring(7), Plain); continue; }
                        // (kneel=0.9: its knees drawn up under it along the ground, its ankles so many shins behind its hips.)
                        if (word.StartsWith("kneel=")) { stage.kneels = float.Parse(word.Substring(6), Plain); continue; }
                        // (laid: kneeling so, its boots laid on their tops, toes back.)
                        if (word == "laid") { stage.laysBoots = true; continue; }
                        if (word.StartsWith("hold="))
                        {
                            var hold = word.Substring(5).Split(',');
                            stage.holds = true;
                            stage.holdHigh = float.Parse(hold[0], Plain);
                            if (hold.Length > 1) stage.holdRate = float.Parse(hold[1], Plain);
                            stage.holdsFeet = hold.Length > 2 && hold[2] == "feet";
                            continue;
                        }
                        if (word.StartsWith("bal="))
                        {
                            var bal = word.Substring(4).Split(',');
                            stage.balance = float.Parse(bal[0], Plain);
                            if (bal.Length > 1) stage.balanceFeet = float.Parse(bal[1], Plain);
                            if (bal.Length > 2) stage.balanceLead = float.Parse(bal[2], Plain);
                            continue;
                        }
                        // (sit=45,0.5,0.05,100: it sits back at up to so many degrees a second, its hips so much of its knees,
                        // until its weight is no further ahead than that, and no more than so many degrees.)
                        if (word.StartsWith("sit="))
                        {
                            var sits = word.Substring(4).Split(',');
                            stage.sits = float.Parse(sits[0], Plain);
                            if (sits.Length > 1) stage.sitHips = float.Parse(sits[1], Plain);
                            if (sits.Length > 2) stage.sitAhead = float.Parse(sits[2], Plain);
                            if (sits.Length > 3) stage.sitMost = float.Parse(sits[3], Plain);
                            continue;
                        }
                        // (walk=0.1,0.6,0.05,0.25: its hands walked back so far each, over so long, lifted so high, no nearer its
                        // feet than so much of its leg.)
                        if (word.StartsWith("walk="))
                        {
                            var walk = word.Substring(5).Split(',');
                            stage.handStep = float.Parse(walk[0], Plain);
                            if (walk.Length > 1) stage.handOver = float.Parse(walk[1], Plain);
                            if (walk.Length > 2) stage.handLift = float.Parse(walk[2], Plain);
                            if (walk.Length > 3) stage.handNearest = float.Parse(walk[3], Plain);
                            continue;
                        }
                        // (step=lead,0.2,0.4,0.06: that leg's boot put so far ahead of its hip and so far out, lifted so far.)
                        if (word.StartsWith("step="))
                        {
                            var put = word.Substring(5).Split(',');
                            stage.steps = put[0] == "lead" ? 0 : 1;
                            stage.stepAhead = float.Parse(put[1], Plain); stage.stepOut = float.Parse(put[2], Plain);
                            if (put.Length > 3) stage.stepLifts = float.Parse(put[3], Plain);
                            // (a fifth: by way of under its hip, so high there.)
                            if (put.Length > 4) stage.stepUnder = float.Parse(put[4], Plain);
                            continue;
                        }
                        int cut = word.IndexOfAny(new[] { '<', '>' });
                        var what = (Asks)System.Enum.Parse(typeof(Asks), word.Substring(0, cut), true);
                        float value = float.Parse(word.Substring(cut + 1), Plain);
                        asked.Add(word[cut] == '>' ? new Asked { what = what, least = value, most = float.PositiveInfinity } : new Asked { what = what, least = float.NegativeInfinity, most = value });
                    }
                stage.through = asked.ToArray();
                stages.Add(stage);
            }
            Close();
            return ways.ToArray();
        }


        public string Tick() => Told() + Where() + Slips() + string.Format(System.Globalization.CultureInfo.InvariantCulture, "; breath {0:0.0} deg; soles {1:0.00} flat, weight {2:0.00} ahead of them", BreathBends, Measure(Asks.Soles), Measure(Asks.Ahead));

        // (How far each thing kept on the ground has slid from where it was put, mm, flat; - where nothing is kept.)
        private string Slips()
        {
            if (!planting) return "";
            Vector3 lies0 = Lies(), right0 = Vector3.Cross(Vector3.up, lies0), moved = Flat(parts[Hips].transform.position - hipsAtPlant);
            string asked = string.Format(System.Globalization.CultureInfo.InvariantCulture, "; hips asked {0:0} ahead {1:0} right, gone {2:0} ahead {3:0} right since kept", Vector3.Dot(askedBy, lies0) * 1000, Vector3.Dot(askedBy, right0) * 1000, Vector3.Dot(moved, lies0) * 1000, Vector3.Dot(moved, right0) * 1000);
            string One(bool kept, Vector3 put, Vector3 now) => kept ? (Flat(now - put).magnitude * 1000).ToString("0", System.Globalization.CultureInfo.InvariantCulture) : "-";
            string Thighs()
            {
                string said = "";
                for (int i = 0; i < 2; i++)
                {
                    Quaternion asks = parts[Hips].transform.rotation * rested[Thigh + i] * aimNow[Thigh + i], now = parts[Thigh + i].transform.rotation;
                    said += string.Format(System.Globalization.CultureInfo.InvariantCulture, " {0:0}/{1:0}", Vector3.Angle(asks * Vector3.up, now * Vector3.up), Quaternion.Angle(asks, now));
                }
                return "; thighs off (along/all):" + said;
            }
            return asked + Thighs() + "; slid: hands " + One(plantedHand[0], plantHand[0], HandEnd(0)) + " " + One(plantedHand[1], plantHand[1], HandEnd(1))
                + ", boots " + One(plantedLeg[0], plantSole[0], parts[Foot].transform.TransformPoint(soleAt[0])) + " " + One(plantedLeg[1], plantSole[1], parts[Foot + 1].transform.TransformPoint(soleAt[1]))
                + ", knees " + One(plantedKnee[0], plantKnee[0], JointAt(Shin)) + " " + One(plantedKnee[1], plantKnee[1], JointAt(Shin + 1));
        }
        private string Told() => string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} {1:0.00} s: on {2} (hands {11:0.00} {12:0.00} knees {13:0.00} {14:0.00} boots {15:0.00} {16:0.00}), its weight {3:0} mm within them; moving off leg {4} hand {5} (giving it {6:0.00}); moved off {7} times, lifted all the same {8}; learnt {9:0} mm; bears {10:0.00}",
            StageNow, got, Supports, BalanceDepth * 1000, offLeg, offHand, offW, Shifts, ShiftsFailed, leansMore.magnitude * 1000, bore,
            leanFelt[Forearm], leanFelt[Forearm + 1], leanFelt[Thigh] + leanFelt[Shin], leanFelt[Thigh + 1] + leanFelt[Shin + 1], leanFelt[Foot], leanFelt[Foot + 1]);
    }
}
