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
            // If it holds itself on what bears it in this pose (holdHigh NaN: not, unless it is moving its weight off a
            // limb): its hands, knees and boots that bear it are kept where they are, and its hips are asked to be
            // holdHigh high (a share of their standing height; NaN: as high as they are), going there at up to holdRate
            // of it a second.
            public float holdHigh = float.NaN, holdRate = .3f;
            // (flattens above nothing: its boots kept on the ground are let down flat, turning about their toes, at up
            // to so many degrees a second.)
            public float flattens;
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
        // (Where a leg reached for the ground bends its knee: 0, the way the body lies along the ground; 1, the way the
        // front of its hips faces; between, so much of the way from one to the other.)
        public static float KneeFaces = .74f;
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
        public static float BalanceQuick = 3.36f, BalanceMargin = .041f, BalanceSlides = .3f, BalanceLeans = .6f, BalanceLeansAtMost = .12f;
        // (What bears each part is felt over BalanceFeels seconds; a part bears it once that is BalanceBorne of its
        // weight.)
        public static float BalanceFeels = .08f, BalanceBorne = .02f;
        // (Not moving off a limb, its weight only as far as over what bears it, not to their middle. October 14: held
        // after one step at 5 of 36 places so, at 12 of 36 to their middle; off.)
        public static bool BalanceLeastMove = false;
        public static float BalanceUnloads = .61f, BalanceLifts = .15f, BalanceShiftsWithin = 2.5f;
        // Moving its weight off a limb, while that limb is lifted, and once it is put down, the hands and boots that
        // bear it stay where they are on the ground (each arm's and leg's joints asked for what keeps it there), and
        // its body is asked to be where its weight is meant to be: where it is asked to be goes there at up to BalanceShiftsAt metres a second, and it is
        // asked to be no more than BalanceShiftsAtMost from where it is. (Pushing with its limbs alone fought its pose,
        // which held its body where it was: October 14.)
        public static float BalanceShiftsAt = .43f, BalanceShiftsAtMost = .117f;
        // (Asked to be where it is going to be in BalanceDamps seconds less far: so it comes there without overshoot.
        // BalancePushes: how much of the push that would bring its weight there its limbs give besides, from where they
        // bear; 0: they only bear it up.)
        public static float BalanceDamps = .19f, BalancePushes = .13f;
        // (An arm whose hand is that share of its length or more from its shoulder when the hand is put is kept straight.)
        public static float ArmStraight = .985f;
        private bool planting, landed;
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
            toned = Tone;
            for (int j = 1; j < Count; j++)
            {
                float has = strength[j] * Mathf.Clamp01(Tone);
                float spring = Mathf.Min(has / (FullAt * Mathf.Deg2Rad), RuleTimes * OwnKeeper.Rule * OwnKeeper.Rule / (step * step) * lighter[j]);
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
        public static float LetsGo = .25f;
        private void Slacken(int leg, int hand, float share)
        {
            for (int j = 1; j < Count; j++)
            {
                bool limb = (leg >= 0 && (j == Thigh + leg || j == Shin + leg || j == Foot + leg)) || (hand >= 0 && (j == UpperArm + hand || j == Forearm + hand));
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
            own.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.right; angle = 0; }
            Vector3 by = angle * axis;
            parts[j].SetDriveTarget(ArticulationDriveAxis.X, by.x);
            if (hinge[j]) return;
            parts[j].SetDriveTarget(ArticulationDriveAxis.Y, by.y);
            parts[j].SetDriveTarget(ArticulationDriveAxis.Z, by.z);
        }

        // A joint's turn from how it stands, in its own frame, for the pose it is at. (A limb's part lies along its
        // own second line, pointing away from the body, and bends about its first: a thigh goes forward by turning
        // one way about it, a shin back by turning the other; an arm's parts are made facing back, so they go the
        // other way round from a leg's.)
        private Quaternion Turn(int j)
        {
            float[] p = stage.pose;
            if (j == Trunk) return Quaternion.Euler(p[0], p[2] * (leads == 1 ? -1 : 1), 0);
            if (j == Head) return Quaternion.Euler(p[1], 0, 0);
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
            leant = 0; leansMore = Vector3.zero;
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
            Vector3 to = Flat(hip) + lies * (stage.stepAhead * reach) + aside * (stage.stepOut * reach);
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
                boot = Quaternion.Slerp(stepBootFrom, boot, Mathf.SmoothStep(0, 1, along));
            }
            else sole = Vector3.Lerp(stepFrom, to, along) + Vector3.up * (stage.stepLifts * Mathf.Sin(Mathf.PI * along));
            LegTo(leg, sole, boot);
        }

        // A leg's joints asked for what puts its boot's sole at a place, the boot turned so.
        private void LegTo(int leg, Vector3 sole, Quaternion boot) => LegFrom(leg, JointAt(Thigh + leg), parts[Hips].transform.rotation, sole, boot);

        // The same, from a hip at a place, of hips turned so (where its hips are asked to be).
        private void LegFrom(int leg, Vector3 hip, Quaternion hips, Vector3 sole, Quaternion boot)
        {
            Vector3 lies = Lies();
            float thighLong = (JointAt(Shin + leg) - JointAt(Thigh + leg)).magnitude, shinLong = (JointAt(Foot + leg) - JointAt(Shin + leg)).magnitude, reach = thighLong + shinLong;
            // Its hip and knee, for its ankle to be there. (Until October 14 its knee bent "up and ahead" in the world:
            // bowed over, with its foot still behind it, that sent its knee up over its back.)
            Vector3 ankle = sole + boot * (anchors[Foot + leg] - soleAt[leg]) - hip;
            float far = Mathf.Clamp(ankle.magnitude, Mathf.Abs(thighLong - shinLong) + .01f, reach * .995f);
            // (Its knee goes the way the body lies along the ground, towards its head: kneeling, that is down and
            // ahead; a foot brought forward under it, its knee comes up towards its chest. Until October 14, evening,
            // the way the front of its hips faced: bowed over, that is down, and a foot brought forward came with its
            // knee on the ground and its shin flat.)
            Vector3 line = ankle.normalized, pole = Vector3.ProjectOnPlane(Vector3.Lerp(lies, parts[Hips].transform.forward, KneeFaces), line);
            pole = pole.sqrMagnitude > 1e-6f ? pole.normalized : Vector3.ProjectOnPlane(parts[Hips].transform.forward, line).normalized;
            float opens = Mathf.Acos(Mathf.Clamp((thighLong * thighLong + far * far - shinLong * shinLong) / (2 * thighLong * far), -1, 1));
            Vector3 thighWay = line * Mathf.Cos(opens) + pole * Mathf.Sin(opens);
            Vector3 shinWay = (line * far - thighWay * thighLong).normalized;
            Quaternion thigh = Quaternion.LookRotation(Vector3.ProjectOnPlane(pole, thighWay).normalized, thighWay);
            Quaternion shin = Quaternion.LookRotation(Vector3.ProjectOnPlane(pole, shinWay).normalized, shinWay);
            aimNow[Thigh + leg] = Quaternion.Inverse(hips * rested[Thigh + leg]) * thigh;
            aimNow[Shin + leg] = Quaternion.Inverse(thigh * rested[Shin + leg]) * shin;
            aimNow[Foot + leg] = Quaternion.Inverse(shin * rested[Foot + leg]) * boot;
            for (int j = Thigh + leg; j <= Foot + leg; j += 2) Aims(j, aimNow[j]);
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

        private void Begin(int index)
        {
            at = index; stage = way.stages[index]; got = 0; still = 0; stepBegun = false; sat = 0; shiftT = 0; liftAt = 0; planting = false; landed = false; stepAimed = false;
            walkHand = -1; for (int i = 0; i < 2; i++) handAt[i] = HandEnd(i);
            for (int j = 1; j < Count; j++) aimFrom[j] = aimNow[j];
        }

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
            if (toned != Tone) Springs(step);
            if (way == null && !Choose()) return Does.GaveUp;
            got += step; Lasted += step;
            if (float.IsNaN(stage.keeps)) leant = Mathf.MoveTowards(leant, 0, 60 * step);
            else leant = Mathf.Clamp(leant + Balances * (stage.keeps - Measure(Asks.Ahead)) * step, -BalancesAtMost, BalancesAtMost);
            if (!float.IsNaN(stage.sits)) sat = Mathf.Clamp(sat + stage.sits * Mathf.Clamp((Measure(Asks.Ahead) - stage.sitAhead) / .1f, -1, 1) * step, 0, stage.sitMost);
            bool through = got >= stage.atLeast;
            for (int k = 0; through && k < stage.through.Length; k++)
            {
                float it = Measure(stage.through[k].what);
                through = it >= stage.through[k].least && it <= stage.through[k].most;
            }
            // (A foot being put somewhere: only once its boot bears on the ground.)
            int stepping = stage.steps < 0 ? -1 : stage.steps == 0 ? leads : 1 - leads;
            if (stepping >= 0 && bears[stepping] < .02f) through = false;
            if (stepping >= 0 && !float.IsNaN(stage.balance) && !(stepBegun && got - liftAt >= stage.over)) through = false;
            if (through)
            {
                if (at + 1 < way.stages.Length) Begin(at + 1);
                else if (way.gives) return Does.Ready;
                else if (!Choose()) return Does.GaveUp;
            }
            else if (got > stage.within) return Does.GaveUp;
            // The pose it is at, come to from where its joints were asked to be before.
            float share = stage.over > 0 ? Mathf.SmoothStep(0, 1, got / stage.over) : 1;
            stepping = stage.steps < 0 ? -1 : stage.steps == 0 ? leads : 1 - leads;
            bool walks = !float.IsNaN(stage.handStep);
            for (int j = 1; j < Count; j++)
            {
                if (stepping >= 0 && j >= Thigh && (j - Thigh) % 2 == stepping) continue;
                if (walks && j >= UpperArm && j < Thigh) continue;
                // (A leg whose hip's turn is not given is left as it is asked now: a foot that was put somewhere stays.)
                if (j >= Thigh && float.IsNaN(stage.pose[9 + 4 * ((j - Thigh) % 2 == leads ? 0 : 1)])) continue;
                aimNow[j] = Quaternion.Slerp(aimFrom[j], toStood[j] * Turn(j), share);
                Aims(j, aimNow[j]);
            }
            if (stage.holds && !float.IsNaN(stage.balance) && !planting) Plant(-1, -1);
            if (stepping >= 0)
            {
                bool balancing = !float.IsNaN(stage.balance);
                // (Keeping its balance, its weight goes off that leg first; the leg is held as it is meanwhile.)
                if (stepBegun || !balancing || ShiftedOff(stepping, -1, step))
                {
                    if (!stepBegun) { stepBegun = true; liftAt = got; stepFrom = parts[Foot + stepping].transform.TransformPoint(soleAt[stepping]); stepBootFrom = parts[Foot + stepping].transform.rotation; }
                    float along = !balancing ? share : stage.over > 0 ? Mathf.SmoothStep(0, 1, (got - liftAt) / stage.over) : 1;
                    if (balancing && along < 1) { offLeg = stepping; offW = 0; }
                    // (Once it is down and bears, it is kept where it is with the rest that bears it.)
                    else if (balancing && !landed && leanFelt[Foot + stepping] >= .25f * BalanceBorne) { landed = true; Plant(-1, -1); }
                    Steps(stepping, along, balancing);
                }
            }
            if (walks) WalksHands(step);
            if (!float.IsNaN(stage.balance))
            {
                Balance(step);
                if (planting) HoldPlanted(step);
                bool shifting = (offLeg >= 0 && !stepBegun) || (offHand >= 0 && walkHand < 0);
                Slacken(shifting ? offLeg : -1, shifting ? offHand : -1, shifting ? offW : 1);
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
                bool off = (offLeg >= 0 && (j == Thigh + offLeg || j == Shin + offLeg || j == Foot + offLeg)) || (offHand >= 0 && j == Forearm + offHand);
                float w = off ? (all ? offW : 0) : 1;
                if (w <= 0) continue;
                Transform boot = j >= Foot ? parts[j].transform : null;
                if (boot != null && SoleSize != null && boot.up.y >= SoleFlat)
                {
                    Vector3 half = .5f * SoleSize[j - Foot];
                    for (int c = 0; c < 4; c++)
                    {
                        supAt[n] = boot.TransformPoint(new Vector3((c & 1) == 0 ? -half.x : half.x, -half.y, (c & 2) == 0 ? -half.z : half.z));
                        sup[n] = Flat(supAt[n]); supPart[n] = j; supW[n] = w; n++;
                    }
                    continue;
                }
                supAt[n] = leanFeltAt[j]; sup[n] = Flat(leanFeltAt[j]); supPart[n] = j; supW[n] = w; n++;
            }
            return n;
        }

        // The middle of what bears it, limb by limb (a boot's corners together are one): of those wholly given their
        // share (full), or of all; of its boots only (boots).
        private Vector3 MiddleOf(int n, bool full, bool boots, out int limbs)
        {
            Vector3 sum = Vector3.zero;
            limbs = 0;
            for (int i = 0; i < n; i++)
            {
                if (full && supW[i] < 1) continue;
                if (boots && supPart[i] < Foot) continue;
                // (A limb's points together count once.)
                int same = 0; Vector3 at = Vector3.zero;
                bool first = true;
                for (int k = 0; k < n; k++)
                {
                    if (supPart[k] != supPart[i] || (full && supW[k] < 1)) continue;
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

        // Before it lifts a limb, its weight goes over the others: true once it is (or it has tried long enough).
        private bool ShiftedOff(int leg, int hand, float step)
        {
            offLeg = leg; offHand = hand;
            if (shiftT <= 0) Plant(leg, hand);
            shiftT += step;
            offW = Mathf.Clamp01(1 - shiftT / Mathf.Max(.05f, BalanceUnloads));
            int n = Supporting(false), m = Outline(sup, n);
            float depth = Within(m, Flat(Weight()), out _);
            float limb = leg >= 0 ? leanFelt[Thigh + leg] + leanFelt[Shin + leg] + leanFelt[Foot + leg] : leanFelt[Forearm + hand];
            // (Its weight over the others is what counts: a knee on the ground bears its share however slack its leg,
            // until it is lifted.)
            bool over = depth >= BalanceMargin && (limb < BalanceLifts || shiftT >= BalanceUnloads) && shiftT >= .5f * BalanceUnloads;
            if (!over && shiftT < BalanceShiftsWithin) return false;
            if (over) Shifts++; else ShiftsFailed++;
            shiftT = 0;
            return true;
        }

        // What bears it, of its hands and boots (but the limb it moves its weight off), is kept where it is now.
        private void Plant(int leg, int hand)
        {
            planting = true;
            for (int i = 0; i < 2; i++)
            {
                plantedHand[i] = i != hand && leanFelt[Forearm + i] >= BalanceBorne && !stage.holdsFeet;
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
            Vector3 by = Vector3.ClampMagnitude(bodyTo - at - BalanceDamps * Flat(WeightGoes()), BalanceShiftsAtMost);
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
            if (!float.IsNaN(stage.holdHigh))
            {
                highTo = Mathf.MoveTowards(highTo, GroundAt(hips.position) + stage.holdHigh * tall, stage.holdRate * tall * step);
                up = Mathf.Clamp(highTo - hips.position.y - BalanceDamps * parts[Hips].velocity.y, -BalanceShiftsAtMost, BalanceShiftsAtMost);
            }
            Vector3 hipsTo = hips.position + by + Vector3.up * up;
            // (Turned as they were when what bears it was put: nothing else keeps its hips from pitching or rolling,
            // its limbs reaching for where they are kept from wherever its hips are.)
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
            for (int i = 0; i < n; i++) if (supW[i] >= 1) full++;
            if (fullLimbs == 0) to = MiddleOf(n, false, false, out _);
            bool moving = offLeg >= 0 || offHand >= 0;
            // (Moving off a limb: only as far as over the others, by twice the margin, from where it is towards their
            // middle.)
            if (full >= 3 && (moving || BalanceLeastMove))
            {
                int others = 0;
                for (int i = 0; i < n; i++) if (supW[i] >= 1) sorted[others++] = sup[i];
                int m0 = Outline(sorted, others);
                float now = Within(m0, at, out _);
                if (now < 2 * BalanceMargin)
                {
                    // (Along the line to their middle, the nearest place that deep.)
                    float lo = 0, hi = 1;
                    for (int k = 0; k < 12; k++)
                    {
                        float mid = .5f * (lo + hi);
                        if (Within(m0, Vector3.Lerp(at, to, mid), out _) >= 2 * BalanceMargin) hi = mid; else lo = mid;
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

        public string Tick() => Told() + Where();
        private string Told() => string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} {1:0.00} s: on {2} (hands {11:0.00} {12:0.00} knees {13:0.00} {14:0.00} boots {15:0.00} {16:0.00}), its weight {3:0} mm within them; moving off leg {4} hand {5} (giving it {6:0.00}); moved off {7} times, lifted all the same {8}; learnt {9:0} mm; bears {10:0.00}",
            StageNow, got, Supports, BalanceDepth * 1000, offLeg, offHand, offW, Shifts, ShiftsFailed, leansMore.magnitude * 1000, bore,
            leanFelt[Forearm], leanFelt[Forearm + 1], leanFelt[Thigh] + leanFelt[Shin], leanFelt[Thigh + 1] + leanFelt[Shin + 1], leanFelt[Foot], leanFelt[Foot + 1]);
    }
}
