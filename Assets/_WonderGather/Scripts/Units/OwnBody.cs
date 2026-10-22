using UnityEngine;

namespace WonderGather
{
    // S3b, step 1 (Docs/Design/TheBodysOwn.md): the body's own body, standing.
    //
    // Luis, October 9: "I want stability in the body but with it still being the body's own. I don't want it to be
    // posed." Until now a miner stood because it was put standing: each frame its parts were placed where its
    // weights and strengths said they should be. Here it stands because its joints hold it up. Its body is thirteen
    // weighted parts joined at joints (the eleven of the let-go body, and each boot a part of its own on an ankle),
    // made as one jointed body that the physics solves as one (an articulation). Nothing pushes it but its weight,
    // the ground, and what its own joints give:
    //  - what the ground should push it with is reckoned each step of the physics: its weight, a little more or less
    //    to keep its height, and sideways as much as brings its weight back from where it is going;
    //  - where on its soles that push must act, no further than the soles reach; where the ground really bore it in
    //    the last step is read from what touched its boots, and what it means is corrected by how far that fell
    //    short;
    //  - each leg's hip, knee and ankle give what makes its boot push the ground so, and no more than they have;
    //  - its hips keep its trunk upright;
    //  - what pushes it from outside is felt (by how its weight really went), and it leans against it;
    //  - and every joint is held softly towards the pose it stands in by the joint's own spring.
    // Every torque is put on the two parts a joint joins, equal and opposite.
    //
    // It is beside the posed body, not in place of it (the plan: what Luis has stays, to switch to). A miner stands
    // so only while it is Wanted (the panel's switch), at ease, and with empty hands. Sent somewhere, or put to
    // work, it gives its body back to the posed one, which takes it over in a quarter of a second. Pushed harder
    // than it can stand, it goes over into the fall that is already built (PhysicalFall), and gets up as that does.
    //
    // And there is life in it (the rest of step 1; what real bodies do standing is in the research of October 9,
    // part 6). None of it is a pose put on the body: each is something its joints are asked for, and the body
    // answers as its weights let it.
    //  - It breathes (Breath): its back straightens a little and its arms are pushed out as its chest fills, and
    //    the chest is drawn fuller; faster and deeper the more its muscles have spent.
    //  - Its weight goes from one leg to the other now and then: the keeper holds its weight nearer one boot; its
    //    hips roll (the hip of the leg it stands on higher), its shoulders the other way, and the knee of the other
    //    leg eases, each a little after the one before. Pushed, it stands square again.
    //  - Its head looks somewhere else every few seconds, at another miner if one is near, and stays where it looks
    //    while the body under it moves.
    //
    // What it does not do yet: step to catch itself; walk. Its eyes are not modelled apart from its head.
    [DefaultExecutionOrder(41)]
    [RequireComponent(typeof(ProceduralBiped), typeof(PhysicalBody), typeof(MinerBody))]
    public sealed class OwnBody : MonoBehaviour
    {
        // The parts, in this order (the left of a pair first), and what each hangs from.
        public const int Hips = OwnKeeper.Hips, Trunk = OwnKeeper.Trunk, Head = OwnKeeper.Head, UpperArm = OwnKeeper.UpperArm, Forearm = OwnKeeper.Forearm, Thigh = OwnKeeper.Thigh, Shin = OwnKeeper.Shin, Foot = OwnKeeper.Foot, Count = OwnKeeper.Count;
        private static readonly int[] From = { -1, Hips, Trunk, Trunk, Trunk, UpperArm, UpperArm + 1, Hips, Hips, Thigh, Thigh + 1, Shin, Shin + 1 };

        // An ankle gives this share of what a knee gives; a hip, this many times a knee; a neck, this many times the
        // head's own turning weight. (The last two as in the let-go body.)
        private const float AnkleGives = .7f, HipGives = 1.5f, NeckHolds = 3;
        // (When it is down, and goes over into the fall, is the keeper's to say.) The posed body takes it back over
        // this long (seconds). It is at
        // ease when the posed body moves slower than this (metres a second), sunk less than this and bowed less.
        private const float GivesBackIn = .25f, AtEase = .02f, SunkAtMost = .02f, BowedAtMost = 3;
        // (And only from its plain stand: its feet set as they are made, to PlainWithin metres. Getting up from a
        // fall leaves them set apart and staggered; the posed balance brings them together again by itself, and
        // until it has the body's own does not take over: taken over astride, it kept that as its stand.)
        private const float PlainWithin = .02f;
        // And it must have been at ease so, both boots on the ground, for this long (seconds): begun as the posed body
        // came to rest from a walk, one boot still in the air on its closing step, Small went down at once.
        private const float AtEaseFor = .4f;
        private float easeFor;

        // ---- The life in it. Set by hand and by what the judges said, not by the search.
        public static bool Alive = true;
        // Its weight goes ShiftsBy of the way from between its boots to the one it favours (wholly favouring it),
        // over ShiftsIn seconds, first after ShiftsFirst seconds and then every so many (between the two). Its hips
        // roll by Rolls degrees then, RollsAfter seconds behind its weight; its trunk keeps upright, its shoulders
        // Shoulders degrees the other way; the knee of the other leg eases by KneeEases degrees, EasesAfter behind.
        public static float ShiftsBy = .28f, ShiftsIn = 1.6f, ShiftsFirst = 3, ShiftsAtLeast = 9, ShiftsAtMost = 26;
        public static float Rolls = 5, RollsAfter = .25f, Shoulders = 1.5f, KneeEases = 12, EasesAfter = .45f;
        // It goes over a leg only as far as that leg bears it with ease: when a joint of the leg gives more than
        // Bears of what it has, it goes no further and comes back a little. (A tired or a weak body shifts less.)
        public static float Bears = .5f;
        // Between, it is never quite still: where it holds its weight drifts by a few millimetres (Sways, metres),
        // slowly, as a standing body's does.
        public static float Sways = .004f;
        // It holds its weight this share of the way from where the posed body had it (near its heels) to the middle
        // of its boots: nudged forwards and coming back, it has sole behind it.
        public static float StandsMid = .5f;
        // A breath at rest straightens its back by BreathBack degrees, pushes each arm out by BreathArms, and draws
        // its chest fuller by BreathSwell (a share). All three times BreathShown: how breath is drawn is a look for
        // Luis to choose (1: as in life; more: drawn stronger; 0: not drawn). Drawn at the size of life it could
        // not be seen at all, nor at two and a half times that (three judges of three). (Tried and taken out:
        // breath lifting the whole body by its legs, 8 mm. Long, on one leg and nudged, went over.)
        public const float BreathDrawn = 4;
        public static float BreathBack = .25f, BreathArms = .625f, BreathSwell = .0225f, BreathShown = BreathDrawn;
        // Two more looks of breath (Luis, October 10: to try beside the others). Its shoulders drawn rising: by
        // ShouldersRise of its height for a breath at rest, times BreathShoulders (0: not drawn so). And its breath
        // seen in the cold air, from dusk to dawn (BreathInAir): so many puffs to a breath out, so far apart.
        public static float BreathShoulders = 1, ShouldersRise = .018f;
        public static bool BreathSeenInAir = true;
        // How large its breath is drawn (its chest and its shoulders; not what its joints are asked for): a size for
        // Luis to set by eye on the panel. Luis, October 10, of the size it had: "I could only really notice the
        // breathing when the character fell." At BreathSizeAtFirst a standing miner's chest at rest is drawn about as
        // full as the fallen one's Luis could see. Its chest is drawn no fuller than SwellAtMost times the size.
        public const float BreathSizeAtFirst = 1, SwellAtMost = .16f;
        public static float BreathSize = BreathSizeAtFirst;
        // Its head looks somewhere else every so many seconds (between the two), no further aside than LooksAside
        // degrees; a look takes LooksIn seconds and LooksInEach more for each degree. It looks at another miner
        // nearer than LooksAsFar metres about one time in three.
        public static float LooksAtLeast = 4, LooksAtMost = 11, LooksAside = 40, LooksIn = .25f, LooksInEach = .006f, LooksAsFar = 15;
        // Between looks its head is never quite fixed: it wanders by about this many degrees, slowly.
        public static float HeadWanders = 1.5f;
        // (Tried twice and taken out twice: its chest following a look aside by a fifth of it, a third of a second
        // behind its head. Its judges said a head that turns alone is "a head on a pivot"; but with it Long, on one
        // leg and nudged, went over. It waits on the step.)
        // (Which way round the engine counts a joint's turn from its pose.)
        private const float Turns = 1;
        private MinerBreath breath;
        private uint chance;
        private float life, favours, favoursFrom, favoursTo, shiftAlong = 1, shiftIn = 1, shiftsAt, rolled, eased;
        private Vector2 looks, looksFrom, looksTo;
        private float lookAlong = 1, lookIn = 1, looksAt, looksUntil;
        private Vector3 itsRight, itsAhead, lookAt, swayed;
        private float swaysFrom;
        private readonly Quaternion[] madeTurned = new Quaternion[Count];

        // How far it favours a leg (-1: wholly its left; 1: its right), where its head looks (degrees to its right
        // and down from straight ahead), and its breath.
        public float Favours => favours;
        // The largest share of what it has that a joint of a leg gave in the last step, and what a joint has.
        public float Asked(int leg) => Mathf.Max(keeper.Gave[Thigh + leg] / Mathf.Max(1e-3f, strength[Thigh + leg]), Mathf.Max(keeper.Gave[Shin + leg] / Mathf.Max(1e-3f, strength[Shin + leg]), keeper.Gave[Foot + leg] / Mathf.Max(1e-3f, strength[Foot + leg])));
        public float Has(int index) => strength[index];
        private float asked, rollMade, rollSeen;
        public Vector2 Looks => looks;
        public float BreathFull => breath.Full;
        public float BreathsAMinute => breath.AMinute;
        public int BreathsTaken => breath.Taken;
        // It is asked to favour a leg now, or to look at a place for some seconds (for whoever shows or tests it).
        public void Favour(float side) { favoursFrom = favours; favoursTo = Mathf.Clamp(side, -1, 1); shiftAlong = 0; shiftIn = ShiftsIn; shiftsAt = life + shiftIn + ShiftsAtMost; }
        public void LookAt(Vector3 place, float seconds) { lookAt = place; looksUntil = life + seconds; }

        private float Chance() { chance = chance * 1664525u + 1013904223u; return (chance >> 8) / 16777216f; }
        // One hump of speed, from nought to one.
        private static float Hump(float u) { u = Mathf.Clamp01(u); return u * u * u * (10 + u * (6 * u - 15)); }

        private ProceduralBiped body;
        private PhysicalBody physical;
        private MinerBody miner;
        private UnitMotor motor;
        private PhysicalBalance balance;
        private bool balanced;
        private GameObject held;
        private readonly ArticulationBody[] parts = new ArticulationBody[Count];
        private readonly Transform[] segments = new Transform[Count];
        private readonly Vector3[] anchors = new Vector3[Count];
        private readonly Quaternion[] rested = new Quaternion[Count];
        private readonly float[] strength = new float[Count], masses = new float[Count], lengths = new float[Count];
        // How hard it has been working its own body (0 to 1): the hardest any of its joints pushes for its strength,
        // taken up over ExertsIn seconds and let go over RecoversIn. Getting up, it is out of breath by so much
        // (MinerBreath). (Luis, October 15: after a fall and the work of getting up its breath shows more, then eases.)
        public float Exertion { get; private set; }
        public static float ExertsIn = 1.5f, RecoversIn = 20;
        private readonly bool[] hinge = new bool[Count];
        private readonly GroundTouch[] touches = new GroundTouch[2];
        private readonly Vector3[] soleAt = new Vector3[2], soleSize = new Vector3[2], toeAt = new Vector3[2];
        private readonly Quaternion[] toeTurn = new Quaternion[2];
        // Getting up, it has toes (Luis, October 14: "adding more muscles in another body part that doesn't yet have
        // muscles"): each a part of its own on a hinge at the ball of the foot, sprung flat with ToeGives of what its
        // ankle has, bending up to ToeBendsUp degrees and down to ToeBendsDown. Kneeling on tucked toes, or coming up
        // onto the balls of its feet, it stands on them as a person does, not on the front edge of a boot. Standing,
        // the boot ends at the ball of the foot and its toes are only drawn, as before. (ToesGettingUp false: none.)
        public static bool ToesGettingUp = true;
        public static float ToeGives = .3f, ToeBendsUp = 80, ToeBendsDown = 20;
        private readonly ArticulationBody[] toeLinks = new ArticulationBody[2];
        private readonly GroundTouch[] toeTouches = new GroundTouch[2];
        // (Getting up, what the ground bears its forearms, thighs and shins with: it leans on them.)
        private readonly GroundTouch[] leanTouches = new GroundTouch[Count];
        // Where each part was a step of the physics ago and is now: drawn between them (an articulation is not
        // smoothed between steps by the engine).
        private readonly Vector3[] wasAt = new Vector3[Count], nowAt = new Vector3[Count];
        private readonly Quaternion[] wasTurned = new Quaternion[Count], nowTurned = new Quaternion[Count];
        // Where the physics left each of the posed body's segments (and its feet and toes), when it took its body back.
        private readonly Vector3[] leftAt = new Vector3[Count + 2];
        private readonly Quaternion[] leftTurned = new Quaternion[Count + 2];
        private float giving;
        private Vector3 nudge;
        private int nudgeFor;
        // S3b, step 3: it catches itself with a step (OwnKeeper.Steps). Its place goes with its feet: where the
        // miner is lies so from the middle of its boots, and is turned so from its hips, as when it began.
        private UnityEngine.AI.NavMeshAgent agent;
        private Vector3 placeFromBoots;
        private Quaternion turnFromHips = Quaternion.identity;
        private int followed;
        private const int Ground = 1 << 6;
        // How often it has stepped since its body became its own, and whether it is stepping now.
        public int Stepped => keeper != null ? keeper.Stepped : 0;
        public bool Stepping => keeper != null && keeper.Stepping;
        public string Tick(Vector3 way, Vector3 from) => keeper != null ? keeper.Tick(way, from) : "";
        // A pull at its chest that is held: so many newtons one way, until a time, coming on over a time.
        private Vector3 pull;
        private float pullFrom, pullUntil = -1, pullOver;

        // S3b, step 2: how it stands, kept from when it last became its own from its plain stand: each part's turn
        // against what it hangs from, its hips' turn from the way they face, how high its weight was and where over
        // its boots it held it. Made low after a fall (BeginLow), it stands up to that.
        private bool stoodKnown, stoodNoted = true;
        private readonly Quaternion[] stoodLocal = new Quaternion[Count], toStood = new Quaternion[Count];
        // (Tried on October 14, and taken out: each joint's own rest given as how the body stands, however it is
        // made (its place and turn on the part it hangs from set by hand), so that what a joint is asked for would
        // be a turn from standing. The engine then puts every part where that rest has it, at once: a crouched body
        // was standing in the first step of the physics, its boots 10 cm in the air. A joint's rest stays how the
        // body was made, and what it is asked for is reckoned from there: toStood.)
        private Quaternion stoodHips = Quaternion.identity;
        private readonly Quaternion[] stoodBoot = { Quaternion.identity, Quaternion.identity };
        private float stoodHeight;
        private Vector2 stoodOver;
        // Whether it can be made low and stand up by its own legs (it has stood by its own joints before), and
        // whether it is doing so now.
        public bool CanRise => stoodKnown && !Stands && body.Ready && miner.Ready && physical.Ready;
        public bool Rising => Stands && riser == null && keeper != null && keeper.Rising;
        public float Risen => keeper != null ? keeper.Risen : 1;
        // Made as it lies (BeginDown), it gets up by a way: one pose after another its joints are held towards
        // (OwnRiser), until it is on its feet and the keeper raises it. The ways it knows; whether it is getting up
        // so now; how often a way did not serve.
        public static OwnRiser.Way[] RiseWays = new OwnRiser.Way[0];
        // The ways it knows itself: Resources/RiseWays/<its name, as the miners are named>.txt, read once (none: it
        // gets up the old way). A bench's RiseWays come first.
        private OwnRiser.Way[] waysOwn;
        public OwnRiser.Way[] WaysUp
        {
            get
            {
                if (RiseWays.Length > 0) return RiseWays;
                if (waysOwn == null)
                {
                    waysOwn = new OwnRiser.Way[0];
                    var choice = FindAnyObjectByType<MinerChoice>(FindObjectsInactive.Include);
                    string called = choice != null && TryGetComponent<SelectableUnit>(out var unit) ? choice.NameFor(unit) : null;
                    var text = Resources.Load<TextAsset>("RiseWays/" + (string.IsNullOrEmpty(called) ? name : called));
                    if (text != null)
                        try { waysOwn = OwnRiser.Read(text.text); }
                        catch (System.FormatException wrong) { Debug.LogError(name + ": its ways up cannot be read: " + wrong.Message); }
                }
                return waysOwn;
            }
        }
        public bool KnowsAWayUp => WaysUp.Length > 0;
        // (Whether one of its ways is for lying so: how far its chest faces the ground, -1 to 1, as the ways ask.)
        public bool KnowsAWayUpFrom(float front)
        {
            foreach (var way in WaysUp) if (way != null && way.stages.Length > 0 && front >= way.frontLeast && front <= way.frontMost) return true;
            return false;
        }
        private OwnRiser riser;
        private readonly Quaternion[] riseFrom = new Quaternion[Count];
        public bool GettingUp => Stands && riser != null;
        public OwnRiser Riser => riser;
        // (What it was like when its getting up handed it to its standing balance, as the riser told it: for the bench.)
        public string HandedOver { get; private set; } = "";
        // (Handed to its standing balance by its own getting up, not gathered into a crouch the old way.)
        private bool roseByItsOwn;
        public int GaveUpGettingUp { get; private set; }
        // (How often it has come onto its feet from lying, by its own body.)
        public int OnItsFeetFromLying { get; private set; }
        // (For a bench: a body that cannot get up, or goes down, is only put away; the fall does not take it.)
        public static bool FallsBack = true;
        // (For a bench, to try a thing, and 1 in the game: its arms made so many times as strong as they are. How
        // strong a miner's arms are is not this file's to say: PhysicalBody, and Luis.)
        public static float ArmsTimes = 1;

        // Whether it is to stand by its own joints when it can (the panel's switch).
        public bool Wanted { get; set; }
        // Whether its body is its own now.
        public bool Stands { get; private set; }
        public int Began { get; private set; }
        public int WentDown { get; private set; }
        public Vector3 HeadAt => Stands ? parts[Head].transform.position : miner.Solved.head.position;
        public Vector3 HipsAt => Stands ? parts[Hips].transform.position : miner.Solved.pelvis.position;
        public ArticulationBody Part(int index) => parts[index];
        // What a joint's worked-out torque was in the last step (newton metres), and how far outside its soles the
        // push it wanted lay (metres): for whoever watches it.
        public float Gave(int index) => keeper != null ? keeper.Gave[index] : 0;
        public float Outside => keeper != null ? keeper.Outside : 0;
        // How far it leans against what it feels (metres from where it rests), and what it feels (m/s2).
        public Vector3 Leaning => keeper != null ? keeper.Leaning : Vector3.zero;
        public Vector3 Feeling => keeper != null ? keeper.Felt : Vector3.zero;

        private void Awake()
        {
            body = GetComponent<ProceduralBiped>();
            physical = GetComponent<PhysicalBody>();
            miner = GetComponent<MinerBody>();
            motor = GetComponent<UnitMotor>();
            if (!TryGetComponent(out breath)) breath = gameObject.AddComponent<MinerBreath>();
        }

        // It can stand by its own joints when the posed body stands at ease with empty hands, and has not fallen.
        public bool CanStand
        {
            get
            {
                if (!body.Ready || !miner.Ready || !physical.Ready || body.LetGo) return false;
                if (body.VelocityNow.magnitude > AtEase || body.SinkNow > SunkAtMost || Mathf.Abs(body.BowNow) > BowedAtMost) return false;
                if (!body.FootPlanted(0) || !body.FootPlanted(1)) return false;
                if (body.StanceWider > PlainWithin || Mathf.Abs(body.StanceStagger) > PlainWithin) return false;
                return Free;
            }
        }

        private float toldWhyAt;
        private string WhyNot()
        {
            if (giving > 0) return "it is still giving its body back";
            if (!body.Ready || !miner.Ready || !physical.Ready) return "its body is not ready";
            if (body.LetGo) return "its body is let go";
            if (!Free) return "something else asks its body (sent somewhere, fallen, a thing in hand)";
            if (body.VelocityNow.magnitude > AtEase) return $"it is moving ({body.VelocityNow.magnitude:F3} m/s)";
            if (body.SinkNow > SunkAtMost) return $"it is sunk {body.SinkNow * 1000:F0} mm";
            if (Mathf.Abs(body.BowNow) > BowedAtMost) return $"it is bowed {body.BowNow:F1} degrees";
            if (!body.FootPlanted(0) || !body.FootPlanted(1)) return "a foot is in the air";
            if (body.StanceWider > PlainWithin || Mathf.Abs(body.StanceStagger) > PlainWithin) return $"its feet are set {body.StanceWider * 1000:F0} mm wider and {body.StanceStagger * 1000:F0} mm staggered; its balance acts: {balance != null && balance.Acts}";
            return "nothing: it is coming to ease";
        }

        // Nothing else asks anything of its body: it is not sent anywhere, has not fallen, holds nothing, and is not
        // taking a thing in hand, hanging it back, fetching a tool or laying one down.
        public bool Free
        {
            get
            {
                if (motor != null && motor.IsMoving) return false;
                if (TryGetComponent<PhysicalFall>(out var fall) && fall.Now != PhysicalFall.State.Up) return false;
                if (TryGetComponent<PhysicalHands>(out var hands) && hands.Held != null) return false;
                if (TryGetComponent<ThingsInHand>(out var things) && things.Now != ThingsInHand.Phase.Hung) return false;
                if (TryGetComponent<PhysicalCarry>(out var carry) && (carry.Laying || carry.Fetching)) return false;
                return true;
            }
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        // A limb's two parts bend one way (as in the let-go body).
        private static Vector3 Bends(Vector3 from, Vector3 joint, Vector3 to, Vector3 otherwise)
        {
            Vector3 axis = (to - from).normalized;
            Vector3 offset = Vector3.ProjectOnPlane(joint - (from + to) * .5f, axis);
            return Vector3.ProjectOnPlane(offset + Vector3.ProjectOnPlane(otherwise, axis) * .02f, axis);
        }

        private static void Ends(Transform segment, out Vector3 from, out Vector3 to)
        {
            Vector3 half = segment.rotation * Vector3.up * segment.localScale.y;
            from = segment.position - half; to = segment.position + half;
        }

        private static Quaternion Along(Transform segment, Vector3 bends)
        {
            Vector3 length = segment.rotation * Vector3.up;
            Vector3 facing = Vector3.ProjectOnPlane(bends, length);
            if (facing.sqrMagnitude < 1e-8f) facing = Vector3.ProjectOnPlane(Vector3.forward, length);
            return Quaternion.LookRotation(facing.normalized, length);
        }

        private float Weighs(params string[] bones)
        {
            float kilograms = 0;
            for (int i = 0; i < physical.PartCount; i++)
            {
                var part = physical.PartAt(i);
                if (part.bone == null) continue;
                foreach (string bone in bones) if (part.bone.name.StartsWith(bone)) { kilograms += part.mass; break; }
            }
            return Mathf.Max(.1f, kilograms);
        }

        // The parts as plain things, before and while they are jointed.
        private readonly GameObject[] segmentsMade = new GameObject[Count];

        private void Rod(int index, float halfLength, float radius, PhysicsMaterial surface)
        {
            var solid = segmentsMade[index].AddComponent<CapsuleCollider>();
            solid.direction = 1;
            solid.radius = radius;
            solid.height = Mathf.Max(2 * radius, 2 * halfLength);
            solid.sharedMaterial = surface;
            lengths[index] = solid.height;
        }

        // The turning weight (kg m2) of the lighter side of a joint, about the joint: of all that hangs beyond it, or
        // of all the rest, whichever is less. (Each part as a point at its middle, and a rod of its own length.)
        private float Lighter(int joint)
        {
            Vector3 about = segmentsMade[joint].transform.TransformPoint(anchors[joint]);
            float beyond = 0, others = 0;
            for (int k = 0; k < Count; k++)
            {
                float turning = masses[k] * ((segmentsMade[k].transform.position - about).sqrMagnitude + lengths[k] * lengths[k] / 12);
                bool hangs = false;
                for (int up = k; up >= 0; up = From[up]) if (up == joint) { hangs = true; break; }
                if (hangs) beyond += turning; else others += turning;
            }
            return Mathf.Max(1e-4f, Mathf.Min(beyond, others));
        }

        // The posed body, standing at ease, becomes its own: the parts are made where the posed body's are.
        public bool Begin()
        {
            if (Stands || !CanStand) return false;
            Make(false, true);
            return true;
        }

        // Crouched where a fall left it, it becomes its own, and stands up by what its legs give against the ground.
        public bool BeginLow()
        {
            if (!CanRise) return false;
            Make(true, false);
            return true;
        }

        // Lying where a fall left it, it becomes its own, and gets up.
        public bool BeginDown()
        {
            if (!CanRise) return false;
            Make(true, false, true);
            return true;
        }

        // Its own parts that strike one another when it lies and gets up (as in the let-go body, and its boots as
        // its shins): those that would fold through each other in a heap.
        private static bool Shinbone(int part) => part >= Shin;
        private static bool Lower(int part) => part == Forearm || part == Forearm + 1;
        private static bool Strike(int a, int b)
        {
            if (a < 0 || b < 0 || a == b) return false;
            if (a > b) (a, b) = (b, a);
            if (Shinbone(b)) return a == Trunk || a == Head || (Shinbone(a) && (a - b) % 2 != 0) || Lower(a) || a == UpperArm || a == UpperArm + 1;
            if (b == Thigh || b == Thigh + 1) return a == Head;
            if (Lower(b)) return a == Trunk || a == Head;
            return false;
        }
        private int Which(Collider solid)
        {
            for (int i = 0; i < Count; i++) if (segmentsMade[i] == solid.gameObject) return i;
            return -1;
        }

        // On its feet: the keeper takes it from the way that got it there, and raises it.
        private void OnItsFeet()
        {
            Told?.Invoke($"{name} at {Time.time:F2}: on its feet after {riser.Lasted:F2} s of getting up; the keeper takes it");
            OnItsFeetFromLying++;
            for (int j = 1; j < Count; j++) riseFrom[j] = riser.Aim(j);
            HandedOver = riser.HandedOver;
            roseByItsOwn = true;
            riser = null;
            keeper.Soften();
            Quaternion hipsFace = Quaternion.LookRotation(Flat(parts[Hips].transform.forward).normalized);
            keeper.RisesTo(stoodHeight, hipsFace * stoodHips, stoodOver);
            // (Its place is under its boots, facing as its hips do.)
            Vector3 between = .5f * (parts[Foot].transform.TransformPoint(soleAt[0]) + parts[Foot + 1].transform.TransformPoint(soleAt[1]));
            if (!StaysPut) transform.SetPositionAndRotation(new Vector3(between.x, transform.position.y, between.z), hipsFace);
            placeFromBoots = Quaternion.Inverse(hipsFace) * (Flat(transform.position) - Flat(between));
            turnFromHips = Quaternion.identity;
            itsRight = Flat(transform.right).normalized; itsAhead = Flat(transform.forward).normalized;
        }

        // Risen, it is made afresh as it stands, so that from here it stands as at any other time: its joints sprung
        // towards how it stands now, not towards the crouch it was made in.
        private void Remake()
        {
            Follows(true);
            for (int i = 0; i < Count; i++) segments[i].SetPositionAndRotation(parts[i].transform.position, parts[i].transform.rotation);
            PutAway();
            if (balance != null) balance.Acts = balanced;
            Make(false, false);
        }

        // Its parts are made where the posed body's segments are. (low: crouched, to stand up. plain: from its plain
        // stand, which is then kept as how it stands.)
        private void Make(bool low, bool plain, bool down = false)
        {
            var s = miner.Solved;
            roseByItsOwn = false;
            Told?.Invoke($"{name} takes over at {Time.time:F2}: the posed body sunk {body.SinkNow * 1000:F1} mm, bowed {body.BowNow:F1} deg, going {body.VelocityNow.magnitude:F3} m/s, crouched {body.CrouchNow * 1000:F1} mm, its feet set {body.StanceWider * 1000:F0} mm wider and {body.StanceStagger * 1000:F0} mm staggered, its boots at {s.feet[0].position.y:F3} and {s.feet[1].position.y:F3}; its hips {(s.pelvis.position.y - .5f * (s.feet[0].position.y + s.feet[1].position.y)) * 1000:F0} mm over its boots, knees bent {Vector3.Angle(s.thighs[0].up, s.shins[0].up):F0} and {Vector3.Angle(s.thighs[1].up, s.shins[1].up):F0} deg");
            var shape = body.BodyProportions;
            segments[Hips] = s.pelvis; segments[Trunk] = s.torso; segments[Head] = s.head;
            for (int i = 0; i < 2; i++)
            {
                segments[UpperArm + i] = s.upperArms[i]; segments[Forearm + i] = s.forearms[i];
                segments[Thigh + i] = s.thighs[i]; segments[Shin + i] = s.shins[i]; segments[Foot + i] = s.feet[i];
            }
            held = new GameObject(name + " (its own)");
            var surface = new PhysicsMaterial("Body") { dynamicFriction = .7f, staticFriction = .8f, bounciness = 0, bounceCombine = PhysicsMaterialCombine.Minimum };
            float arm = physical.ArmRadius, leg = physical.LegRadius;
            float knee = physical.KneeNow;
            Quaternion hips = s.pelvis.rotation, chest = s.torso.rotation;
            for (int i = 0; i < Count; i++) { parts[i] = null; lengths[i] = 0; }

            // (The parts are made first as plain things, each hung in the hierarchy from what it hangs from, as an
            // articulation asks; they are jointed once they all stand.)
            GameObject Made(int index, string called, Vector3 placed, Quaternion turned, float kilograms, float gives, bool hinges)
            {
                var part = new GameObject(called);
                part.transform.SetParent(From[index] >= 0 ? segmentsMade[From[index]].transform : held.transform, true);
                part.transform.SetPositionAndRotation(placed, turned);
                segmentsMade[index] = part;
                masses[index] = kilograms; strength[index] = gives; hinge[index] = hinges;
                return part;
            }

            // The hips: a rod across them.
            var pelvis = Made(Hips, "Hips", s.pelvis.position, hips, Weighs("Pelvis"), 0, false);
            float girth = Mathf.Clamp(shape.bodyFront, leg * 1.5f, .3f * shape.hipHeight);
            var across = pelvis.AddComponent<CapsuleCollider>();
            across.direction = 0; across.radius = girth; across.height = 2 * (shape.hipWidth + girth); across.sharedMaterial = surface;
            lengths[Hips] = across.height;
            // The trunk, from the waist to the neck.
            Made(Trunk, "Trunk", s.torso.position, chest, Weighs("Spine", "Chest", "Neck"), physical.BackNow, false);
            float breadth = Mathf.Max(shape.shoulder.x - arm, .5f * shape.shoulder.x);
            Rod(Trunk, shape.torsoRise + .5f * breadth, breadth, surface);
            anchors[Trunk] = new Vector3(0, -shape.torsoRise, 0);
            // The head.
            float headWeighs = Weighs("Head");
            float neckLever = Mathf.Max(.03f, shape.headRise - 2 * shape.torsoRise);
            var head = Made(Head, "Head", s.head.position, s.head.rotation, headWeighs, NeckHolds * headWeighs * Physics.gravity.magnitude * neckLever * physical.Strength, false);
            var skull = head.AddComponent<SphereCollider>();
            skull.radius = Mathf.Max(.06f, shape.headHalf); skull.sharedMaterial = surface;
            lengths[Head] = 2 * skull.radius;
            anchors[Head] = new Vector3(0, -neckLever, 0);

            for (int i = 0; i < 2; i++)
            {
                string side = i == 0 ? " (left)" : " (right)", l = i == 0 ? "L" : "R";
                // An arm: its elbow bends back.
                Ends(s.upperArms[i], out var shoulder, out var elbow);
                Ends(s.forearms[i], out _, out var wrist);
                Vector3 back = Bends(shoulder, elbow, wrist, -(chest * Vector3.forward));
                Made(UpperArm + i, "Upper arm" + side, s.upperArms[i].position, Along(s.upperArms[i], back), Weighs("UpperArm." + l), physical.ShoulderOf(i) * ArmsTimes, false);
                Rod(UpperArm + i, s.upperArms[i].localScale.y, arm, surface);
                anchors[UpperArm + i] = new Vector3(0, -s.upperArms[i].localScale.y, 0);
                Made(Forearm + i, "Forearm" + side, s.forearms[i].position, Along(s.forearms[i], back), Weighs("Forearm." + l, "Hand." + l), physical.ElbowOf(i) * ArmsTimes, true);
                Rod(Forearm + i, s.forearms[i].localScale.y + arm, arm, surface);
                anchors[Forearm + i] = new Vector3(0, -s.forearms[i].localScale.y, 0);

                // A leg: its knee bends forward.
                Ends(s.thighs[i], out var hip, out var kneeAt);
                Ends(s.shins[i], out _, out var ankle);
                Vector3 forward = Bends(hip, kneeAt, ankle, hips * Vector3.forward);
                Made(Thigh + i, "Thigh" + side, s.thighs[i].position, Along(s.thighs[i], forward), Weighs("Thigh." + l), knee * HipGives, false);
                Rod(Thigh + i, s.thighs[i].localScale.y, leg * 1.25f, surface);
                anchors[Thigh + i] = new Vector3(0, -s.thighs[i].localScale.y, 0);
                // (Its boot is a part of its own, with its own weight: in the let-go body it is one with the shin.)
                bool toed = down && ToesGettingUp && s.toes != null && s.toes.Length == 2 && s.toes[0] != null && s.toes[1] != null;
                float bootWeighs = toed ? Weighs("Foot." + l) : Weighs("Foot." + l, "Toe." + l), shinWeighs = Weighs("Shin." + l);
                Made(Shin + i, "Shin" + side, s.shins[i].position, Along(s.shins[i], forward), shinWeighs, knee, true);
                Rod(Shin + i, s.shins[i].localScale.y, leg, surface);
                anchors[Shin + i] = new Vector3(0, -s.shins[i].localScale.y, 0);
                var boot = Made(Foot + i, "Boot" + side, s.feet[i].position, s.feet[i].rotation, bootWeighs, knee * AnkleGives, false);
                var soleBox = boot.AddComponent<BoxCollider>();
                soleBox.size = s.feet[i].localScale; soleBox.sharedMaterial = surface;
                lengths[Foot + i] = Mathf.Max(soleBox.size.x, Mathf.Max(soleBox.size.y, soleBox.size.z));
                anchors[Foot + i] = boot.transform.InverseTransformPoint(segmentsMade[Shin + i].transform.TransformPoint(new Vector3(0, s.shins[i].localScale.y, 0)));
                soleAt[i] = new Vector3(0, -.5f * soleBox.size.y, 0); soleSize[i] = soleBox.size;
                if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null)
                {
                    toeAt[i] = boot.transform.InverseTransformPoint(s.toes[i].position);
                    toeTurn[i] = Quaternion.Inverse(boot.transform.rotation) * s.toes[i].rotation;
                }
            }

            // Jointed: one body, its hips the root; every other part on a joint to what it hangs from, sprung
            // softly towards the pose it is made in.
            for (int i = 0; i < Count; i++)
            {
                if (From[i] >= 0) rested[i] = Quaternion.Inverse(segmentsMade[From[i]].transform.rotation) * segmentsMade[i].transform.rotation;
            }
            for (int i = 0; i < Count; i++)
            {
                var link = segmentsMade[i].AddComponent<ArticulationBody>();
                link.mass = masses[i];
                // (Nothing slows its parts but its joints.)
                link.linearDamping = 0; link.angularDamping = 0; link.jointFriction = 0;
                link.useGravity = true;
                link.maxAngularVelocity = 20; link.maxDepenetrationVelocity = 1.5f;
                link.collisionDetectionMode = CollisionDetectionMode.Continuous;
                if (From[i] < 0) { link.immovable = false; link.solverIterations = 14; link.solverVelocityIterations = 4; }
                else
                {
                    link.jointType = hinge[i] ? ArticulationJointType.RevoluteJoint : ArticulationJointType.SphericalJoint;
                    link.anchorPosition = anchors[i]; link.anchorRotation = Quaternion.identity;
                    link.matchAnchors = true;
                    link.twistLock = ArticulationDofLock.FreeMotion;
                    if (!hinge[i]) { link.swingYLock = ArticulationDofLock.FreeMotion; link.swingZLock = ArticulationDofLock.FreeMotion; }
                    float lighter = Lighter(i), step = Time.fixedDeltaTime;
                    turning[i] = lighter;
                    float spring = Mathf.Min(strength[i] / (OwnKeeper.FullAt * Mathf.Deg2Rad), OwnKeeper.Rule * OwnKeeper.Rule / (step * step) * lighter);
                    float damper = OwnKeeper.Damped * 2 * Mathf.Sqrt(spring * lighter);
                    var drive = new ArticulationDrive { stiffness = spring, damping = damper, forceLimit = strength[i], target = 0, targetVelocity = 0, driveType = ArticulationDriveType.Force };
                    link.xDrive = drive;
                    if (!hinge[i]) { link.yDrive = drive; link.zDrive = drive; }
                    // (No joint turns faster than a joint can.)
                    if (OwnKeeper.Steps && OwnKeeper.TurnsAtMost > 0) link.maxJointVelocity = OwnKeeper.TurnsAtMost;
                }
                parts[i] = link;
            }
            for (int i = 0; i < 2; i++) { touches[i] = segmentsMade[Foot + i].AddComponent<GroundTouch>(); touches[i].StillOnly = down; }
            for (int i = 0; i < Count; i++) leanTouches[i] = null;
            if (down)
                foreach (int i in new[] { Forearm, Forearm + 1, Thigh, Thigh + 1, Shin, Shin + 1 }) { leanTouches[i] = segmentsMade[i].AddComponent<GroundTouch>(); leanTouches[i].StillOnly = true; }
            for (int i = 0; i < 2; i++) { toeLinks[i] = null; toeTouches[i] = null; }
            if (down && ToesGettingUp && s.toes != null && s.toes.Length == 2 && s.toes[0] != null && s.toes[1] != null)
                for (int i = 0; i < 2; i++)
                {
                    string l = i == 0 ? "L" : "R";
                    var toe = new GameObject("Toe" + (i == 0 ? " (left)" : " (right)"));
                    toe.transform.SetParent(segmentsMade[Foot + i].transform, true);
                    toe.transform.SetPositionAndRotation(s.toes[i].position, s.toes[i].rotation);
                    Vector3 size = s.toes[i].localScale;
                    var box = toe.AddComponent<BoxCollider>();
                    box.size = size; box.sharedMaterial = surface;
                    var link = toe.AddComponent<ArticulationBody>();
                    float kilograms = Weighs("Toe." + l);
                    link.mass = kilograms;
                    link.linearDamping = 0; link.angularDamping = 0; link.jointFriction = 0;
                    link.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    link.jointType = ArticulationJointType.RevoluteJoint;
                    // (It turns about the line across the ball of the foot, at the sole.)
                    link.anchorPosition = new Vector3(0, -.5f * size.y, -.5f * size.z); link.anchorRotation = Quaternion.identity;
                    link.matchAnchors = true;
                    link.twistLock = ArticulationDofLock.LimitedMotion;
                    float has = knee * AnkleGives * ToeGives, turns = kilograms * size.z * size.z / 3, step = Time.fixedDeltaTime;
                    float spring = Mathf.Min(has / (OwnRiser.FullAt * Mathf.Deg2Rad), OwnRiser.RuleTimesFeet * OwnKeeper.Rule * OwnKeeper.Rule / (step * step) * turns);
                    link.xDrive = new ArticulationDrive { stiffness = spring, damping = OwnRiser.Damped * 2 * Mathf.Sqrt(spring * turns), forceLimit = has, target = 0, lowerLimit = -ToeBendsUp, upperLimit = ToeBendsDown, driveType = ArticulationDriveType.Force };
                    toeLinks[i] = link;
                    toeTouches[i] = toe.AddComponent<GroundTouch>();
                    toeTouches[i].StillOnly = true;
                }
            // Its own parts do not strike one another while it only stands; nor the unit's own solid, which stays
            // where it is to be picked by.
            var solids = held.GetComponentsInChildren<Collider>();
            for (int a = 0; a < solids.Length; a++)
                for (int b = a + 1; b < solids.Length; b++)
                    if (!down || !Strike(Which(solids[a]), Which(solids[b]))) Physics.IgnoreCollision(solids[a], solids[b]);
            foreach (var mine in GetComponents<Collider>()) foreach (var solid in solids) Physics.IgnoreCollision(mine, solid);
            Physics.SyncTransforms();
            for (int i = 0; i < Count; i++)
            {
                wasAt[i] = nowAt[i] = segmentsMade[i].transform.position;
                wasTurned[i] = nowTurned[i] = madeTurned[i] = segmentsMade[i].transform.rotation;
            }
            itsRight = Flat(transform.right).normalized; itsAhead = Flat(transform.forward).normalized;
            chance = (uint)Breath.SeedOf(name) * 2654435761u + (uint)Began * 40503u + 7u;
            life = 0; favours = favoursFrom = favoursTo = rolled = eased = 0; shiftAlong = 1;
            shiftsAt = ShiftsFirst * Mathf.Lerp(.7f, 1.3f, Chance());
            swaysFrom = 60 * Chance(); swayed = Vector3.zero;
            looks = looksFrom = looksTo = Vector2.zero; lookAlong = 1; looksAt = Mathf.Lerp(1, 3, Chance()); looksUntil = 0;

            balance = GetComponent<PhysicalBalance>();
            balanced = balance != null && balance.Acts;
            if (balance != null) balance.Acts = false;
            body.LetGo = true;
            keeper = new OwnKeeper(parts, From, masses, strength, hinge, anchors, touches, soleAt, soleSize, turning) { StandsMid = Alive ? StandsMid : 0, Life = Lived, GroundUnder = AnyGround ? null : GroundFor, Tells = what => Told?.Invoke(name + " at " + Time.time.ToString("F2") + ": " + what) };
            {
                Vector3 between = Flat(.5f * (parts[Foot].transform.TransformPoint(soleAt[0]) + parts[Foot + 1].transform.TransformPoint(soleAt[1])));
                Quaternion hipsFace = Quaternion.LookRotation(Flat(parts[Hips].transform.forward).normalized);
                placeFromBoots = Quaternion.Inverse(hipsFace) * (Flat(transform.position) - between);
                turnFromHips = Quaternion.Inverse(hipsFace) * Quaternion.Euler(0, transform.eulerAngles.y, 0);
                followed = 0; pullUntil = -1;
            }
            rollMade = Mathf.Asin(Mathf.Clamp(parts[Hips].transform.right.y, -1, 1)) * Mathf.Rad2Deg; rollSeen = 0;
            nudgeFor = 0; giving = 0;
            Stands = true;
            Began++;
            stoodNoted = !plain;
            if (plain)
            {
                for (int j = 1; j < Count; j++) stoodLocal[j] = rested[j];
                Quaternion facesNow = Quaternion.LookRotation(Flat(parts[Hips].transform.forward).normalized);
                stoodHips = Quaternion.Inverse(facesNow) * parts[Hips].transform.rotation;
                for (int i = 0; i < 2; i++) stoodBoot[i] = Quaternion.Inverse(facesNow) * parts[Foot + i].transform.rotation;
            }
            if (low)
            {
                for (int j = 1; j < Count; j++) { toStood[j] = Quaternion.Inverse(rested[j]) * stoodLocal[j]; riseFrom[j] = Quaternion.identity; }
                if (down) riser = new OwnRiser(parts, From, masses, strength, hinge, touches, soleAt, toStood, turning, body.StandingHipHeight, Ground, rested, anchors, stoodBoot) { Ways = WaysUp, ToeTouches = toeLinks[0] != null ? toeTouches : null, PartTouches = leanTouches, SoleSize = soleSize, StoodHips = stoodHips };
                else keeper.RisesTo(stoodHeight, Quaternion.LookRotation(Flat(parts[Hips].transform.forward).normalized) * stoodHips, stoodOver);
                // (The posed body under it stands, for when it is given its body back.)
                body.Bow(0, 400); body.Sink(0); body.ResetPose();
            }
        }

        // Its parts are put away, and nothing else: for whoever takes the body next.
        private void PutAway()
        {
            if (held != null) { held.SetActive(false); Destroy(held); }
            held = null;
            keeper = null; riser = null; toeLinks[0] = toeLinks[1] = null;
            for (int i = 0; i < Count; i++) parts[i] = null;
            Stands = false;
        }

        // The posed body takes it back: over a quarter of a second each part goes from where the physics left it to
        // where the posed body has it.
        public void GiveBack()
        {
            if (!Stands) return;
            var s = miner.Solved;
            for (int i = 0; i < Count; i++) { leftAt[i] = segments[i].position; leftTurned[i] = segments[i].rotation; }
            for (int i = 0; i < 2; i++)
                if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null) { leftAt[Count + i] = s.toes[i].position; leftTurned[Count + i] = s.toes[i].rotation; }
            PutAway();
            if (balance != null) balance.Acts = balanced;
            body.LetGo = false;
            giving = GivesBackIn;
        }

        // Whatever takes the body at once (the fall): its parts are gone, and the posed body is left as it is.
        public void Drop()
        {
            if (!Stands) return;
            PutAway();
            if (balance != null) balance.Acts = balanced;
            body.LetGo = false;
            giving = 0;
        }

        // The ground where a boot is to be put, for the keeper: only where the walked ground is (within a quarter of
        // a metre: its place can go with it there), and as high as the ground lies there. NaN: none. (A miner that
        // has fallen and got up is "off the walked ground" until it is sent somewhere, though it stands on it: that
        // is not asked.)
        // (For whoever looks at why it does or does not step: a test.)
        public static System.Action<string> Told;
        // (And to try it without: its place left where it was; no ground asked for.)
        public static bool StaysPut, AnyGround;
        private bool described;
        private float GroundFor(Vector3 place)
        {
            bool walked = UnityEngine.AI.NavMesh.SamplePosition(place, out _, .25f, UnityEngine.AI.NavMesh.AllAreas);
            bool found = Physics.Raycast(place + Vector3.up * 1.5f, Vector3.down, out var hit, 4, Ground);
            if (Told != null && (!walked || !found)) Told($"no ground for a boot of {name} at {place.x:F2} {place.y:F3} {place.z:F2}: walked ground there {walked}; ground found {found}");
            if (!walked) return float.NaN;
            return found ? hit.point.y : float.NaN;
        }

        // Its place goes with its feet: once it has stepped and stands again (or at once, when its body is about
        // to be given back or let go), the miner is where its body is.
        private void Follows(bool atOnce = false)
        {
            if (StaysPut || keeper == null || keeper.Stepped == followed || (!atOnce && keeper.Stepping)) return;
            followed = keeper.Stepped;
            Quaternion hipsFace = Quaternion.LookRotation(keeper.Faces);
            Vector3 to = Flat(keeper.StandsAt) + hipsFace * placeFromBoots;
            to.y = transform.position.y;
            if (agent == null) TryGetComponent(out agent);
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && UnityEngine.AI.NavMesh.SamplePosition(to, out var walked, .5f, UnityEngine.AI.NavMesh.AllAreas)) agent.Warp(walked.position);
            else transform.position = to;
            transform.rotation = hipsFace * turnFromHips;
        }

        // It is pulled at its chest one way with a share of its weight, for some seconds, the pull coming on over
        // a time (as a rope draws tight).
        public void Pull(Vector3 way, float share, float seconds, float over)
        {
            if (!Stands) return;
            way = Flat(way);
            if (way.sqrMagnitude < 1e-6f) return;
            float whole = 0;
            for (int i = 0; i < Count; i++) whole += masses[i];
            pull = way.normalized * (share * whole * Physics.gravity.magnitude);
            pullFrom = Time.time; pullUntil = Time.time + seconds; pullOver = over;
        }
        public bool Pulled => Stands && Time.time < pullUntil;

        // Something pushes it: a force (newtons) at a place, for this step of the physics.
        public void Push(Vector3 force, Vector3 where)
        {
            if (!Stands) return;
            int nearest = Trunk;
            float least = float.MaxValue;
            for (int i = 0; i < Count; i++)
            {
                float far = (parts[i].worldCenterOfMass - where).sqrMagnitude;
                if (far < least) { least = far; nearest = i; }
            }
            parts[nearest].AddForceAtPosition(force, where);
        }

        // It is set going one way at a speed (metres a second), over a tenth of a second: a nudge.
        public void Nudge(Vector3 way, float speed)
        {
            if (!Stands) return;
            way = Flat(way);
            if (way.sqrMagnitude < 1e-6f) return;
            nudgeFor = Mathf.Max(1, Mathf.RoundToInt(.1f / Time.fixedDeltaTime));
            nudge = way.normalized * (speed / (nudgeFor * Time.fixedDeltaTime));
        }

        private void Update()
        {
            if (Stands)
            {
                // Sent somewhere, given something to do, or no longer wanted so, it gives its body back.
                if (!Wanted || !Free) { Follows(true); GiveBack(); }
            }
            else
            {
                // (Left astride by getting up, and no posed balance acting to bring its feet together: the posed body
                // is asked to set its feet as they are made. It steps them there, as it does whenever its stance is
                // changed, and the body's own takes over from the plain stand.)
                if (Wanted && giving <= 0 && body.Ready && !body.LetGo && (body.StanceWider > PlainWithin || Mathf.Abs(body.StanceStagger) > PlainWithin)
                    && Free && (!TryGetComponent<PhysicalBalance>(out var acting) || !acting.Acts)) body.SetStance(0, 0);
                easeFor = Wanted && giving <= 0 && CanStand ? easeFor + Time.deltaTime : 0;
                // (For whoever looks at why it does not stand by its own joints: what keeps it, every two seconds.)
                if (Told != null && Wanted && easeFor <= 0 && Time.time >= toldWhyAt) { toldWhyAt = Time.time + 2; Told(name + " at " + Time.time.ToString("F2") + " does not take over: " + WhyNot()); }
                if (easeFor >= AtEaseFor) { easeFor = 0; Begin(); }
            }
        }

        private void FixedUpdate()
        {
            // (A bench that steps the physics itself takes these steps itself.)
            if (Physics.simulationMode == SimulationMode.FixedUpdate) Advance(Time.fixedDeltaTime);
        }

        // One step of its own doing.
        public void Advance(float dt)
        {
            if (!Stands) return;
            // (Where it was and is, for drawing it between the steps.)
            for (int i = 0; i < Count; i++)
            {
                wasAt[i] = nowAt[i]; wasTurned[i] = nowTurned[i];
                nowAt[i] = parts[i].transform.position; nowTurned[i] = parts[i].transform.rotation;
            }
            if (nudgeFor > 0)
            {
                nudgeFor--;
                for (int i = 0; i < Count; i++) parts[i].AddForce(masses[i] * nudge);
            }
            if (Time.time < pullUntil) parts[Trunk].AddForceAtPosition(pull * (pullOver > 0 ? Mathf.Clamp01((Time.time - pullFrom) / pullOver) : 1), parts[Trunk].worldCenterOfMass);
            // (How hard it is working: getting up, the hardest of its joints for its strength; then it recovers.)
            {
                float effort = 0;
                if (riser != null)
                    for (int j = 1; j < Count; j++)
                    {
                        var push = parts[j].driveForce;
                        float most = 0;
                        for (int d = 0; d < push.dofCount; d++) most = Mathf.Max(most, Mathf.Abs(push[d]));
                        effort = Mathf.Max(effort, most / Mathf.Max(1e-3f, strength[j]));
                    }
                effort = Mathf.Clamp01(effort);
                Exertion = Mathf.MoveTowards(Exertion, effort, dt / (effort > Exertion ? ExertsIn : RecoversIn));
            }
            // (Getting up, it is the way's; the keeper has it only once it is on its feet.)
            if (riser != null)
            {
                riser.BreathBends = BreathBack * breath.Full * breath.Deep * BreathShown;
                var does = riser.Step(dt);
                if (does == OwnRiser.Does.Ready) OnItsFeet();
                else if (does == OwnRiser.Does.GaveUp)
                {
                    GaveUpGettingUp++;
                    Told?.Invoke($"{name} at {Time.time:F2}: gave up getting up, at {riser.WayNow} {riser.StageNow}");
                    GoDown();
                }
                return;
            }
            bool rose = keeper.Rising;
            if (!keeper.Keep(dt)) GoDown();
            else
            {
                Follows();
                // (How it stands, from its plain stand: kept for when it is made low.)
                if (!stoodNoted && keeper.Begun) { stoodNoted = true; stoodHeight = keeper.Height; stoodOver = keeper.StandsBy; stoodKnown = true; }
                if (rose && !keeper.Rising) Remake();
            }
            if (Told != null && !described && keeper != null) { described = true; Told(name + " is " + keeper.Describe()); }
        }

        // A joint is asked for a turn from the pose it was made in: a turn about the body's own lines as it began
        // (across it, up, ahead), of the part against what it hangs from.
        private void Asks(int j, Quaternion turn)
        {
            Aims(j, Quaternion.Inverse(madeTurned[j]) * turn * madeTurned[j]);
        }

        // (The turn in the part's own frame.)
        private void Aims(int j, Quaternion own)
        {
            own.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.right; angle = 0; }
            Vector3 by = Turns * angle * axis;
            parts[j].SetDriveTarget(ArticulationDriveAxis.X, by.x);
            if (hinge[j]) return;
            parts[j].SetDriveTarget(ArticulationDriveAxis.Y, by.y);
            parts[j].SetDriveTarget(ArticulationDriveAxis.Z, by.z);
        }

        // Its life is lived in the middle of each of the keeper's steps: it says where its weight is held, and how its
        // hips roll.
        private OwnKeeper keeper;
        private readonly float[] turning = new float[Count];
        private void Lived(float step, bool pushed, Vector3 apart)
        {
            // (Rising, its joints are asked for how it stands, by as far as it has risen; and nothing else of it.)
            if (keeper.Rising)
            {
                float up = Hump(keeper.Risen), trunkUp = roseByItsOwn ? Hump(Mathf.InverseLerp(OwnRiser.RiseTrunkAfter, 1, keeper.Risen)) : up;
                for (int j = 1; j < Count; j++) Aims(j, Quaternion.Slerp(riseFrom[j], toStood[j], j == Trunk || j == Head ? trunkUp : up));
                keeper.Shift = Vector3.zero; keeper.Roll = Quaternion.identity;
                return;
            }
            // (While it steps, and while its boots are astride after a step, it stands square: its weight is not
            // asked to a leg, its hips are not rolled, and its legs are the keeper's.)
            bool steps = keeper.Stepping || keeper.LegsHeld;
            if (Alive) Lives(step, pushed || steps, steps);
            else if (!steps) Still();
            keeper.Shift = steps ? Vector3.zero : apart * (.5f * ShiftsBy * favours) + swayed;
            keeper.Roll = steps ? Quaternion.identity : Quaternion.AngleAxis(Rolls * rolled, itsAhead);
        }

        // With no life in it, every joint is asked for the pose it was made in.
        private void Still()
        {
            favours = rolled = eased = 0; swayed = Vector3.zero;
            for (int j = 1; j < Count; j++) Aims(j, Quaternion.identity);
        }

        // One step of its life: its breath, the leg it favours, where it looks; and what its joints are asked for.
        private void Lives(float step, bool pushed, bool legsHeld = false)
        {
            life += step;
            // Its weight, from leg to leg. Pushed, it stands square.
            if (pushed)
            {
                if (favoursTo != 0 || shiftAlong >= 1 && favours != 0) { favoursFrom = favours; favoursTo = 0; shiftAlong = 0; shiftIn = .5f * ShiftsIn; }
                shiftsAt = life + Mathf.Lerp(4, 8, Chance());
            }
            else if (life >= shiftsAt && shiftAlong >= 1)
            {
                favoursFrom = favours;
                // To the other leg, mostly; now and then only a little further on or off the one it is on.
                float other = favours > 0 ? -1 : favours < 0 ? 1 : Chance() < .5f ? -1 : 1;
                favoursTo = favours != 0 && Chance() < .3f ? Mathf.Clamp(favours + .5f * (Chance() - .5f), -1, 1) : other * Mathf.Lerp(.55f, 1, Chance());
                shiftIn = ShiftsIn * Mathf.Lerp(.6f, 1, .5f * Mathf.Abs(favoursTo - favoursFrom)) * Mathf.Lerp(.9f, 1.15f, Chance());
                shiftAlong = 0;
                shiftsAt = life + shiftIn + Mathf.Lerp(ShiftsAtLeast, ShiftsAtMost, Chance());
            }
            // (As far as the leg bears it with ease, and no further.)
            asked = Mathf.Lerp(asked, Mathf.Abs(favours) > .05f ? Asked(favours > 0 ? 1 : 0) : 0, Mathf.Clamp01(step / .2f));
            if (asked > Bears && Mathf.Abs(favours) > .05f && Mathf.Abs(favoursTo) >= .9f * Mathf.Abs(favours) && favoursTo * favours >= 0)
            {
                favoursFrom = favours; favoursTo = .8f * favours; shiftAlong = 0; shiftIn = .5f * ShiftsIn; asked = 0;
            }
            if (shiftAlong < 1)
            {
                shiftAlong = Mathf.Min(1, shiftAlong + step / Mathf.Max(.1f, shiftIn));
                favours = Mathf.Lerp(favoursFrom, favoursTo, Hump(shiftAlong));
            }
            // Between, where it holds its weight drifts: two slow turns along it and two across, never in step.
            float t = life + swaysFrom;
            swayed = pushed ? Vector3.Lerp(swayed, Vector3.zero, Mathf.Clamp01(4 * step))
                : Sways * (itsAhead * (.6f * Mathf.Sin(.9f * t) + .4f * Mathf.Sin(.37f * t + 1.3f)) + itsRight * (.36f * Mathf.Sin(.53f * t + .7f) + .24f * Mathf.Sin(1.21f * t)));
            // (Its hips follow its weight, and the easing knee follows them.)
            rolled = Mathf.Lerp(rolled, favours, Mathf.Clamp01(step / RollsAfter));
            eased = Mathf.Lerp(eased, rolled, Mathf.Clamp01(step / EasesAfter));

            // Where it looks.
            if (life < looksUntil)
            {
                // (At a place it was asked to look at.)
                Vector3 to = lookAt - parts[Head].transform.position;
                Vector2 wanted = new Vector2(Mathf.Clamp(Vector3.SignedAngle(itsAhead, Flat(to), Vector3.up), -LooksAside, LooksAside),
                    Mathf.Clamp(-Mathf.Atan2(to.y, Mathf.Max(.01f, Flat(to).magnitude)) * Mathf.Rad2Deg, -20, 30));
                if ((wanted - looksTo).magnitude > 2) Glance(wanted);
                looksAt = Mathf.Max(looksAt, looksUntil + 1);
            }
            else if (pushed) { if (looksTo != new Vector2(0, 5)) Glance(new Vector2(0, 5)); looksAt = life + Mathf.Lerp(2, 4, Chance()); }
            else if (life >= looksAt && lookAlong >= 1)
            {
                float what = Chance();
                Vector2 wanted;
                if (what < .34f && Another(out Vector2 theirs)) wanted = theirs;
                else if (what < .7f) wanted = new Vector2(14 * (2 * Chance() - 1), Mathf.Lerp(-3, 8, Chance()));
                else wanted = new Vector2((Chance() < .5f ? -1 : 1) * Mathf.Lerp(18, LooksAside, Chance()), Mathf.Lerp(-2, 10, Chance()));
                Glance(wanted);
                looksAt = life + lookIn + Mathf.Lerp(LooksAtLeast, LooksAtMost, Chance());
            }
            if (lookAlong < 1)
            {
                lookAlong = Mathf.Min(1, lookAlong + step / Mathf.Max(.05f, lookIn));
                looks = Vector2.Lerp(looksFrom, looksTo, Hump(lookAlong));
            }

            // What its joints are asked for.
            float roll = Rolls * rolled, fills = breath.Full * breath.Deep * BreathShown;
            Quaternion rollsBack = Quaternion.AngleAxis(-roll, itsAhead);
            // Its trunk stays upright while its hips roll under it (its shoulders a little the other way): it is
            // asked against its hips for the roll they are seen to have, taken up slowly (asked to keep upright in
            // the world step by step, it threw its hips about when it was nudged). And its back straightens as its
            // chest fills. (Which way it faces now is its hips'.)
            Quaternion faces = Quaternion.AngleAxis(Vector3.SignedAngle(Flat(keeper.Upright * Vector3.forward), Flat(parts[Hips].transform.forward), Vector3.up), Vector3.up);
            rollSeen = Mathf.Lerp(rollSeen, Mathf.Asin(Mathf.Clamp(parts[Hips].transform.right.y, -1, 1)) * Mathf.Rad2Deg - rollMade, Mathf.Clamp01(step / .3f));
            Asks(Trunk, Quaternion.AngleAxis(-(Mathf.Clamp(rollSeen, -10, 10) + Shoulders * rolled), itsAhead) * Quaternion.AngleAxis(-BreathBack * fills, itsRight));
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1 : 1;
                // (Its arms hang as they did while its shoulders tip; breath pushes them out.)
                Asks(UpperArm + i, Quaternion.AngleAxis(Shoulders * rolled + side * BreathArms * fills, itsAhead));
                // The leg it does not stand on: its knee eases forward, its boot staying flat.
                float bends = KneeEases * Mathf.Clamp01(-side * eased);
                if (legsHeld) continue;
                Asks(Thigh + i, rollsBack * Quaternion.AngleAxis(-.5f * bends, itsRight));
                Asks(Shin + i, Quaternion.AngleAxis(bends, itsRight));
                Asks(Foot + i, Quaternion.AngleAxis(-.5f * bends, itsRight));
            }
            // Its head keeps to where it looks, whatever the body under it does.
            Vector2 wanders = HeadWanders * new Vector2(.65f * Mathf.Sin(.71f * t + 2) + .35f * Mathf.Sin(1.7f * t), .45f * Mathf.Sin(.93f * t + 1) + .25f * Mathf.Sin(2.3f * t + 4));
            Quaternion wants = faces * Quaternion.AngleAxis(looks.x + wanders.x, Vector3.up) * Quaternion.AngleAxis(looks.y + wanders.y, itsRight) * madeTurned[Head];
            Aims(Head, Quaternion.Inverse(parts[Trunk].transform.rotation * rested[Head]) * wants);
        }

        private void Glance(Vector2 wanted)
        {
            looksFrom = looks; looksTo = wanted; lookAlong = 0;
            lookIn = LooksIn + LooksInEach * (wanted - looks).magnitude;
        }

        // Another miner near enough, and not behind it: where its head is, as a look.
        private bool Another(out Vector2 theirs)
        {
            theirs = Vector2.zero;
            float nearest = LooksAsFar;
            bool any = false;
            Vector3 from = parts[Head].transform.position;
            foreach (var unit in FindObjectsByType<SelectableUnit>(FindObjectsSortMode.None))
            {
                if (unit == null || unit.gameObject == gameObject || !unit.TryGetComponent<MinerBody>(out var other) || !other.Ready) continue;
                Vector3 to = other.Solved.head.position - from;
                float far = Flat(to).magnitude, aside = Vector3.SignedAngle(itsAhead, Flat(to), Vector3.up);
                if (far < .5f || far > nearest || Mathf.Abs(aside) > 75) continue;
                nearest = far; any = true;
                theirs = new Vector2(Mathf.Clamp(aside, -LooksAside, LooksAside), Mathf.Clamp(-Mathf.Atan2(to.y, far) * Mathf.Rad2Deg, -20, 30));
            }
            return any;
        }

        // It cannot stand: its parts are put away, and the fall that is built takes the body as it is.
        private void GoDown()
        {
            WentDown++;
            Told?.Invoke(name + " at " + Time.time.ToString("F2") + ": goes down");
            if (!FallsBack) { Drop(); return; }
            if (riser == null) Follows(true);
            // (How each part is going is handed on: the let-go body's eleven parts are this one's first eleven.)
            for (int i = 0; i < Foot; i++) { goesOn[i] = parts[i].GetPointVelocity(parts[i].worldCenterOfMass); turnsOn[i] = parts[i].angularVelocity; }
            PutAway();
            if (balance != null) balance.Acts = balanced;
            body.LetGo = false;
            giving = 0;
            if (!TryGetComponent<PhysicalFall>(out var fall)) fall = gameObject.AddComponent<PhysicalFall>();
            fall.LetGo();
            if (fall.Now != PhysicalFall.State.Falling) return;
            for (int i = 0; i < PhysicalFall.Count && i < Foot; i++)
            {
                var part = fall.Part(i);
                if (part == null) continue;
                part.linearVelocity = goesOn[i]; part.angularVelocity = turnsOn[i];
            }
        }
        private readonly Vector3[] goesOn = new Vector3[Count], turnsOn = new Vector3[Count];

        // After the posed body would have posed, and before the model's bones are turned to it.
        private void LateUpdate()
        {
            var s = miner.Solved;
            if (Stands)
            {
                // (Getting up, its own place goes with its hips, as in the fall: on the ground under them.)
                if (riser != null && !StaysPut)
                {
                    Vector3 hipsAt = parts[Hips].transform.position;
                    if (Physics.Raycast(new Vector3(hipsAt.x, hipsAt.y + 2, hipsAt.z), Vector3.down, out var under, 8, Ground)) transform.position = new Vector3(hipsAt.x, under.point.y, hipsAt.z);
                }
                float between = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
                for (int i = 0; i < Count; i++)
                    segments[i].SetPositionAndRotation(Vector3.Lerp(wasAt[i], nowAt[i], between), Quaternion.Slerp(wasTurned[i], nowTurned[i], between));
                for (int i = 0; i < 2; i++)
                    if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null)
                    {
                        // (Toes of its own are drawn where they are.)
                        if (toeLinks[i] != null) s.toes[i].SetPositionAndRotation(toeLinks[i].transform.position, toeLinks[i].transform.rotation);
                        else s.toes[i].SetPositionAndRotation(segments[Foot + i].TransformPoint(toeAt[i]), segments[Foot + i].rotation * toeTurn[i]);
                    }
                return;
            }
            if (giving <= 0 || segments[Hips] == null) return;
            // The posed body has posed this frame: each segment is between where the physics left it and there.
            giving -= Time.deltaTime;
            float share = Mathf.SmoothStep(0, 1, 1 - Mathf.Clamp01(giving / GivesBackIn));
            for (int i = 0; i < Count; i++)
            {
                if (i >= UpperArm && i < Foot)
                {
                    // (A limb's segment is turned any way about its own length: only where it lies along counts.)
                    Vector3 was = leftTurned[i] * Vector3.up, will = segments[i].rotation * Vector3.up;
                    segments[i].SetPositionAndRotation(Vector3.Lerp(leftAt[i], segments[i].position, share), Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(was, will, share).normalized));
                }
                else segments[i].SetPositionAndRotation(Vector3.Lerp(leftAt[i], segments[i].position, share), Quaternion.Slerp(leftTurned[i], segments[i].rotation, share));
            }
            for (int i = 0; i < 2; i++)
                if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null)
                    s.toes[i].SetPositionAndRotation(Vector3.Lerp(leftAt[Count + i], s.toes[i].position, share), Quaternion.Slerp(leftTurned[Count + i], s.toes[i].rotation, share));
        }

        private void OnDisable() { if (Stands) Drop(); }
    }
}
