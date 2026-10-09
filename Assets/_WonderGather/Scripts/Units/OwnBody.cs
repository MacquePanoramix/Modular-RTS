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
    // What it does not do yet: step to catch itself; walk; breathe or shift its weight (the rest of step 1).
    [DefaultExecutionOrder(41)]
    [RequireComponent(typeof(ProceduralBiped), typeof(PhysicalBody), typeof(MinerBody))]
    public sealed class OwnBody : MonoBehaviour
    {
        // The parts, in this order (the left of a pair first), and what each hangs from.
        public const int Hips = 0, Trunk = 1, Head = 2, UpperArm = 3, Forearm = 5, Thigh = 7, Shin = 9, Foot = 11, Count = 13;
        private static readonly int[] From = { -1, Hips, Trunk, Trunk, Trunk, UpperArm, UpperArm + 1, Hips, Hips, Thigh, Thigh + 1, Shin, Shin + 1 };

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
        public static float Rights = 5.3963f, RightsDamped = .05f, Edge = .00435f, Rises = 224.2f, RisesDamped = 14.277f;
        // An ankle gives this share of what a knee gives; a hip, this many times a knee; a neck, this many times the
        // head's own turning weight. (The last two as in the let-go body.)
        private const float AnkleGives = .7f, HipGives = 1.5f, NeckHolds = 3;
        // It is down, and goes over into the fall, when its weight is lower than this share of how high it stood, or
        // its hips lean further than this (degrees). The posed body takes it back over this long (seconds). It is at
        // ease when the posed body moves slower than this (metres a second), sunk less than this and bowed less.
        private const float DownAt = .8f, DownLeaning = 35, GivesBackIn = .25f, AtEase = .02f, SunkAtMost = .02f, BowedAtMost = 3;
        // And it must have been at ease so, both boots on the ground, for this long (seconds): begun as the posed body
        // came to rest from a walk, one boot still in the air on its closing step, Small went down at once.
        private const float AtEaseFor = .4f;
        private float easeFor;

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
        // The keeper's own state.
        private bool begun, hasBefore;
        private Vector3 rest, holds, felt, wentBefore, weightBefore, actedBefore, realAt, shortBy, acted;
        private Quaternion upright;
        private float height, whole;
        private bool realKnown;
        private readonly Vector3[] torque = new Vector3[Count], sole = new Vector3[2], at = new Vector3[2];
        private readonly float[] gave = new float[Count];
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
        public float Gave(int index) => gave[index];
        public float Outside { get; private set; }
        // How far it leans against what it feels (metres from where it rests), and what it feels (m/s2).
        public Vector3 Leaning => holds - rest;
        public Vector3 Feeling => felt;

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
            whole = 0;
            for (int i = 0; i < Count; i++)
            {
                whole += masses[i];
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
                    float spring = Mathf.Min(strength[i] / (FullAt * Mathf.Deg2Rad), Rule * Rule / (step * step) * lighter);
                    float damper = Damped * 2 * Mathf.Sqrt(spring * lighter);
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
                wasTurned[i] = nowTurned[i] = segmentsMade[i].transform.rotation;
            }

            balance = GetComponent<PhysicalBalance>();
            balanced = balance != null && balance.Acts;
            if (balance != null) balance.Acts = false;
            body.LetGo = true;
            begun = false; hasBefore = false; realKnown = false;
            felt = shortBy = Vector3.zero; nudgeFor = 0; giving = 0;
            Stands = true;
            Began++;
            return true;
        }

        // Its parts are put away, and nothing else: for whoever takes the body next.
        private void PutAway()
        {
            if (held != null) { held.SetActive(false); Destroy(held); }
            held = null;
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
            Keep(Time.fixedDeltaTime);
        }

        // One step of keeping itself up.
        private void Keep(float step)
        {
            float g = Physics.gravity.magnitude;
            Vector3 weight = Weight(), goes = WeightGoes();
            // Where the ground really bore it in the last step.
            {
                Vector3 bore = Vector3.zero; float lifted = 0;
                for (int i = 0; i < 2; i++) { bore += touches[i].Where; lifted += touches[i].Up; touches[i].Clear(); }
                realKnown = lifted > 1e-6f;
                if (realKnown) realAt = bore / lifted;
            }
            for (int i = 0; i < 2; i++) sole[i] = parts[Foot + i].transform.TransformPoint(soleAt[i]);
            float ground = .5f * (sole[0].y + sole[1].y), high = weight.y - ground;
            if (!begun)
            {
                begun = true; height = high; upright = parts[Hips].transform.rotation;
                rest = Flat(weight); holds = rest;
            }
            // Down: it goes over into the fall that is built.
            if (float.IsNaN(high) || high < DownAt * height || Vector3.Angle(Vector3.up, parts[Hips].transform.up) > DownLeaning) { GoDown(); return; }

            float rise = Mathf.Clamp(Rises * (height - high) - RisesDamped * goes.y, -.5f * g, g);
            // A standing body falls away from where the ground pushes it, at a rate of its own (the root of gravity
            // over its height). So what matters is where its weight is going: its place, and its speed over that rate.
            float falls = Mathf.Sqrt((g + rise) / Mathf.Max(.1f, high));
            if (hasBefore && Feels > 0)
            {
                // What pushes it from outside: how its weight really went, beyond what the ground's push gave it.
                Vector3 pushedAt = realKnown && Tracks > 0 ? realAt : actedBefore;
                Vector3 seen = Flat(goes - wentBefore) / step - falls * falls * Flat(weightBefore - pushedAt);
                felt = Vector3.ClampMagnitude(Vector3.Lerp(felt, seen, Mathf.Clamp01(step / Feels)), 4);
            }
            if (hasBefore && realKnown && Tracks > 0) shortBy = Vector3.ClampMagnitude(shortBy + Tracks * step * Flat(actedBefore - realAt), .05f);
            Vector3 going = Flat(weight) + Flat(goes) / falls;
            Vector3 wants = going + Quick / falls * (going - holds) + felt / (falls * falls);
            Vector3 acts = new Vector3(wants.x, ground, wants.z);

            // The push is shared between the boots by how far across it lies, and each boot bears its share along
            // its own middle line; only a push wanted outside both is taken to a boot's outer side.
            Vector3 over = Flat(sole[1] - sole[0]);
            Vector3 beside = over.sqrMagnitude > 1e-8f ? over.normalized : Vector3.right;
            for (int i = 0; i < 2; i++)
            {
                Transform foot = parts[Foot + i].transform;
                float across = Vector3.Dot(Flat(acts - sole[i]), beside);
                float outward = i == 0 ? Mathf.Min(across, 0) : Mathf.Max(across, 0);
                Vector3 local = foot.InverseTransformPoint(acts - beside * (across - outward));
                Vector3 half = .5f * soleSize[i];
                local = new Vector3(Mathf.Clamp(local.x, -half.x + Edge, half.x - Edge), -half.y, Mathf.Clamp(local.z, -half.z + Edge, half.z - Edge));
                at[i] = foot.TransformPoint(local);
            }
            float share = Mathf.Clamp01(Vector3.Dot(Flat(acts - sole[0]), over) / Mathf.Max(1e-6f, over.sqrMagnitude));
            acted = Vector3.Lerp(at[0], at[1], share);
            Outside = Flat(acts - acted).magnitude;
            // The push can only act where the soles are: it pushes sideways only as a push from there, through its
            // weight, does.
            Vector3 sideways = Flat(weight - acted) * ((g + rise) / Mathf.Max(.1f, high));
            Vector3 push = whole * (sideways + Vector3.up * (g + rise));

            // It leans against what it feels, for what it cannot take standing as it is, keeping sole behind its weight.
            {
                Vector3 asked = felt / (falls * falls);
                Vector3 leansTo = rest;
                if (asked.magnitude > Comfort)
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
                holds = Vector3.Lerp(holds, leansTo, Mathf.Clamp01(Leans * step));
            }
            wentBefore = goes; weightBefore = weight; actedBefore = acted; hasBefore = true;

            // What each leg's joints give to make its boot push the ground so.
            for (int j = 0; j < Count; j++) torque[j] = Vector3.zero;
            for (int i = 0; i < 2; i++)
            {
                Vector3 pushes = (i == 0 ? 1 - share : share) * push;
                for (int j = Thigh + i; j <= Foot + i; j += 2)
                    torque[j] -= Vector3.Cross(at[i] + shortBy - JointAt(j), pushes);
            }
            // Its hips keep its trunk as upright as it began.
            {
                Quaternion off = upright * Quaternion.Inverse(parts[Hips].transform.rotation);
                off.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180) angle -= 360;
                if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.zero; angle = 0; }
                float spring = Rights * whole;
                Vector3 rights = spring * (angle * Mathf.Deg2Rad) * axis - RightsDamped * spring * parts[Hips].angularVelocity;
                torque[Thigh] -= (1 - share) * rights;
                torque[Thigh + 1] -= share * rights;
            }
            for (int j = Thigh; j < Count; j++)
            {
                // (A hinge gives only about its own line; what is across it is borne by the joint itself.)
                if (hinge[j]) { Vector3 line = parts[j].transform.right; torque[j] = line * Vector3.Dot(line, torque[j]); }
                torque[j] = Vector3.ClampMagnitude(torque[j], strength[j]);
                if (float.IsNaN(torque[j].x) || float.IsNaN(torque[j].y) || float.IsNaN(torque[j].z)) torque[j] = Vector3.zero;
                gave[j] = torque[j].magnitude;
                // Equal and opposite, on the two parts the joint joins: nothing pushes the body from nowhere.
                parts[j].AddTorque(torque[j]);
                parts[From[j]].AddTorque(-torque[j]);
            }
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
