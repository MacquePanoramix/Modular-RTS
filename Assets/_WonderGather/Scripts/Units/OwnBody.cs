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
        public static float BreathShoulders = 0, ShouldersRise = .018f;
        public static bool BreathSeenInAir;
        private const int PuffsABreath = 3;
        private const float PuffsApart = .16f;
        private BreathInAir air;
        private float stature, headHalf, fullBefore, puffIn, leftShrug;
        private int puffsLeft;
        private bool emptying;
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
        private Breath breath;
        private uint chance;
        private float life, favours, favoursFrom, favoursTo, shiftAlong = 1, shiftIn = 1, shiftsAt, rolled, eased;
        private Vector2 looks, looksFrom, looksTo;
        private float lookAlong = 1, lookIn = 1, looksAt, looksUntil;
        private Vector3 itsRight, itsAhead, lookAt, swayed;
        private float swaysFrom;
        private float leftSwell;
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
        private readonly bool[] hinge = new bool[Count];
        private readonly GroundTouch[] touches = new GroundTouch[2];
        private readonly Vector3[] soleAt = new Vector3[2], soleSize = new Vector3[2], toeAt = new Vector3[2];
        private readonly Quaternion[] toeTurn = new Quaternion[2];
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
        }

        // It can stand by its own joints when the posed body stands at ease with empty hands, and has not fallen.
        public bool CanStand
        {
            get
            {
                if (!body.Ready || !miner.Ready || !physical.Ready || body.LetGo) return false;
                if (body.VelocityNow.magnitude > AtEase || body.SinkNow > SunkAtMost || Mathf.Abs(body.BowNow) > BowedAtMost) return false;
                if (!body.FootPlanted(0) || !body.FootPlanted(1)) return false;
                return Free;
            }
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
            var s = miner.Solved;
            var shape = body.BodyProportions;
            headHalf = Mathf.Max(.06f, shape.headHalf);
            stature = s.head.position.y - transform.position.y + headHalf;
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
                Made(UpperArm + i, "Upper arm" + side, s.upperArms[i].position, Along(s.upperArms[i], back), Weighs("UpperArm." + l), physical.ShoulderOf(i), false);
                Rod(UpperArm + i, s.upperArms[i].localScale.y, arm, surface);
                anchors[UpperArm + i] = new Vector3(0, -s.upperArms[i].localScale.y, 0);
                Made(Forearm + i, "Forearm" + side, s.forearms[i].position, Along(s.forearms[i], back), Weighs("Forearm." + l, "Hand." + l), physical.ElbowOf(i), true);
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
                float bootWeighs = Weighs("Foot." + l, "Toe." + l), shinWeighs = Weighs("Shin." + l);
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
                }
                parts[i] = link;
            }
            for (int i = 0; i < 2; i++) touches[i] = segmentsMade[Foot + i].AddComponent<GroundTouch>();
            // Its own parts do not strike one another while it only stands; nor the unit's own solid, which stays
            // where it is to be picked by.
            var solids = held.GetComponentsInChildren<Collider>();
            for (int a = 0; a < solids.Length; a++)
                for (int b = a + 1; b < solids.Length; b++) Physics.IgnoreCollision(solids[a], solids[b]);
            foreach (var mine in GetComponents<Collider>()) foreach (var solid in solids) Physics.IgnoreCollision(mine, solid);
            Physics.SyncTransforms();
            for (int i = 0; i < Count; i++)
            {
                wasAt[i] = nowAt[i] = segmentsMade[i].transform.position;
                wasTurned[i] = nowTurned[i] = madeTurned[i] = segmentsMade[i].transform.rotation;
            }
            itsRight = Flat(transform.right).normalized; itsAhead = Flat(transform.forward).normalized;
            chance = (uint)Breath.SeedOf(name) * 2654435761u + (uint)Began * 40503u + 7u;
            breath.Begin(Breath.SeedOf(name) + Began);
            life = 0; favours = favoursFrom = favoursTo = rolled = eased = 0; shiftAlong = 1;
            fullBefore = 0; puffsLeft = 0; emptying = false;
            shiftsAt = ShiftsFirst * Mathf.Lerp(.7f, 1.3f, Chance());
            swaysFrom = 60 * Chance(); swayed = Vector3.zero;
            looks = looksFrom = looksTo = Vector2.zero; lookAlong = 1; looksAt = Mathf.Lerp(1, 3, Chance()); looksUntil = 0;

            balance = GetComponent<PhysicalBalance>();
            balanced = balance != null && balance.Acts;
            if (balance != null) balance.Acts = false;
            body.LetGo = true;
            keeper = new OwnKeeper(parts, From, masses, strength, hinge, anchors, touches, soleAt, soleSize, turning) { StandsMid = Alive ? StandsMid : 0, Life = Lived };
            rollMade = Mathf.Asin(Mathf.Clamp(parts[Hips].transform.right.y, -1, 1)) * Mathf.Rad2Deg; rollSeen = 0;
            nudgeFor = 0; giving = 0;
            Stands = true;
            Began++;
            return true;
        }

        // Its parts are put away, and nothing else: for whoever takes the body next.
        private void PutAway()
        {
            if (held != null) { held.SetActive(false); Destroy(held); }
            held = null;
            keeper = null;
            leftSwell = miner.Swell; miner.Swell = 0;
            leftShrug = miner.Shrug; miner.Shrug = 0;
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
            miner.Swell = leftSwell; miner.Shrug = leftShrug;
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
                if (!Wanted || !Free) GiveBack();
            }
            else
            {
                easeFor = Wanted && giving <= 0 && CanStand ? easeFor + Time.deltaTime : 0;
                if (easeFor >= AtEaseFor) { easeFor = 0; Begin(); }
            }
        }

        private void FixedUpdate()
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
            if (!keeper.Keep(Time.fixedDeltaTime)) GoDown();
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
            if (Alive) Lives(step, pushed);
            else Still();
            keeper.Shift = apart * (.5f * ShiftsBy * favours) + swayed;
            keeper.Roll = Quaternion.AngleAxis(Rolls * rolled, itsAhead);
        }

        // With no life in it, every joint is asked for the pose it was made in.
        private void Still()
        {
            favours = rolled = eased = 0; swayed = Vector3.zero;
            for (int j = 1; j < Count; j++) Aims(j, Quaternion.identity);
        }

        // One step of its life: its breath, the leg it favours, where it looks; and what its joints are asked for.
        private void Lives(float step, bool pushed)
        {
            life += step;
            float spent = Mathf.Max(Mathf.Max(physical.Spent(PhysicalBody.Muscles.Legs), physical.Spent(PhysicalBody.Muscles.Back)),
                Mathf.Max(physical.Spent(PhysicalBody.Muscles.LeftArm), physical.Spent(PhysicalBody.Muscles.RightArm)));
            breath.Goes(step, spent / .85f);
            // Its breath seen in the air: as its chest begins to empty, a few puffs from its mouth, one after another.
            if (breath.Full > fullBefore) emptying = false;
            else if (breath.Full < fullBefore && !emptying)
            {
                emptying = true;
                if (BreathSeenInAir && fullBefore >= .9f) { puffsLeft = PuffsABreath; puffIn = 0; }
            }
            fullBefore = breath.Full;
            if (puffsLeft > 0 && (puffIn -= step) <= 0)
            {
                puffsLeft--; puffIn = PuffsApart;
                if (air == null && !TryGetComponent(out air)) air = gameObject.AddComponent<BreathInAir>();
                Transform face = parts[Head].transform;
                air.Puff(face.position + face.forward * (1.05f * headHalf) - face.up * (.35f * headHalf), face.forward - .1f * face.up, .5f * breath.Deep, headHalf);
            }

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
                float between = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
                for (int i = 0; i < Count; i++)
                    segments[i].SetPositionAndRotation(Vector3.Lerp(wasAt[i], nowAt[i], between), Quaternion.Slerp(wasTurned[i], nowTurned[i], between));
                for (int i = 0; i < 2; i++)
                    if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null)
                        s.toes[i].SetPositionAndRotation(segments[Foot + i].TransformPoint(toeAt[i]), segments[Foot + i].rotation * toeTurn[i]);
                miner.Swell = Alive ? BreathSwell * breath.Full * breath.Deep * BreathShown : 0;
                miner.Shrug = Alive ? ShouldersRise * stature * BreathShoulders * breath.Full * Mathf.Min(breath.Deep, 1.4f) : 0;
                return;
            }
            if (giving <= 0 || segments[Hips] == null) return;
            // The posed body has posed this frame: each segment is between where the physics left it and there.
            giving -= Time.deltaTime;
            float share = Mathf.SmoothStep(0, 1, 1 - Mathf.Clamp01(giving / GivesBackIn));
            miner.Swell = leftSwell * (1 - share); miner.Shrug = leftShrug * (1 - share);
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
