using UnityEngine;

namespace WonderGather
{
    // S3, step 10 (Docs/Design/ThePhysicalBody.md): the fall. While a body can keep its feet it is posed (the walk,
    // the stance, the steps of PhysicalBalance). When it cannot, it is let go: the whole body follows the physics.
    //
    // Let go, the body is eleven solid parts with the weights it was weighed at (PhysicalBody), jointed where a body
    // is jointed, each joint turning only as far as a joint turns: the hips, the trunk, the head, and for each side
    // an upper arm, a forearm with its hand, a thigh, and a shin with its foot. They are made where the posed body
    // is at that moment, moving as it was moving. From then on the posed body's own segments (what ProceduralBiped
    // solves, and MinerBody turns the model's bones to) are put where those parts are, so everything that follows
    // the body (the model, what hangs on it, its coat) follows the fall.
    //
    // Falling, each joint is held towards a pose that protects the body (it goes down into a crouch, its arms out
    // towards the ground, its head kept from it), with what strength that joint has left. Lying, it lets go.
    //
    // Then it gets up. Having lain a moment, it gathers itself: the same crouch, with its own strength, where it
    // lies. The posed body then takes over from there, crouched at that place, over a moment in which each part goes
    // from where the physics left it to where the crouch has it; and it stands up at its legs' own pace.
    [DefaultExecutionOrder(40)]
    [RequireComponent(typeof(ProceduralBiped), typeof(PhysicalBody), typeof(MinerBody))]
    public sealed class PhysicalFall : MonoBehaviour
    {
        public enum State { Up, Falling, Lying, Gathering, Rising }

        // The parts, in this order (the left of a pair first).
        public const int Hips = 0, Trunk = 1, Head = 2, UpperArm = 3, Forearm = 5, Thigh = 7, Shin = 9, Count = 11;
        // How far each joint turns, in degrees: bending (back or straightening, then forward or folding), twisting
        // about the part's own length, and to the side. A first setting, from how a body's joints turn.
        private static readonly Vector4 Waist = new Vector4(-25, 70, 30, 25), Neck = new Vector4(-40, 45, 50, 30),
            Shoulder = new Vector4(-170, 50, 70, 95), Elbow = new Vector4(-145, 3, 5, 3),
            Hip = new Vector4(-20, 120, 35, 45), Knee = new Vector4(-145, 3, 5, 3);
        // Falling, each joint is held towards a pose that protects the body, with what strength that joint has left:
        // all of it when the joint is this far from that pose (degrees), and it slows the joint's turning by this
        // share of that strength for each radian a second. Lying still, the body holds itself with only this share of
        // its strength, and comes to that over this long (seconds).
        private const float FullAt = 35, Slows = .12f, AtRest = .05f, LetsGoOver = 1;
        // The pose it falls in: it goes down into a crouch, which brings its weight low before it lands (the knees,
        // the hips and the trunk bent this far, in degrees), the elbows a little bent; the arms go out towards the
        // ground it falls to (forwards so far, backwards so far), and the head is kept from it by so much.
        private const float KneesBent = 100, HipsBent = 85, TrunkBowed = 25, ElbowsBent = 30, ArmsForward = 85, ArmsBack = 30, HeadKept = 35;
        // Lying still it lies easy: only this bent.
        private const float KneesEasy = 20, HipsEasy = 10, TrunkEasy = 5;
        // A neck holds this many times the head's own turning weight; a hip gives this many times what a knee gives.
        private const float NeckHolds = 3, HipGives = 1.5f;
        // It lies when its hips, its trunk and its head have not moved faster than this (metres a second), or turned
        // faster than this (radians a second), for this long. (Its limbs may still be finding their place.)
        private const float Still = .2f, StillTurning = 1.2f, LiesAfter = .5f;
        // Getting up. It lies this long (seconds) before it gathers itself; gathers itself for at least this long, and
        // until it is still again or this long has passed; and the posed body takes over in this long. Crouched, the
        // posed body's hips are this share of their height lower, and its back is bowed this far (degrees).
        private const float LiesFor = 1.2f, GathersAtLeast = .8f, GathersAtMost = 2.2f, TakesOver = .9f, CrouchSink = .5f, CrouchBow = 50;
        // The posed body takes over no deeper in its crouch than its legs can raise it from: where its knees would give
        // this share of what they have now. If that is not even this share of its hips' height, it lies down again,
        // and its legs rest this long (seconds) before it tries again.
        private const float RisesAt = .55f, LeastCrouch = .1f, RestsMore = 2.5f;
        // Once it lies, it is falling again only if its hips, its trunk or its head move faster than this (metres a
        // second): its limbs settling as it lets go are not a fall.
        private const float Thrown = 1.5f;
        private const int Ground = 1 << 6;

        private ProceduralBiped body;
        private PhysicalBody physical;
        private MinerBody miner;
        private UnitMotor motor;
        private GameObject held;
        private readonly Rigidbody[] parts = new Rigidbody[Count];
        private readonly Transform[] segments = new Transform[Count];
        private readonly Vector3[] before = new Vector3[Count], lately = new Vector3[Count];
        private readonly Vector3[] footAt = new Vector3[2], toeAt = new Vector3[2];
        private readonly Quaternion[] footTurn = new Quaternion[2], toeTurn = new Quaternion[2];
        private float beforeTime = -1, latelyTime = -1, still, ridesUp, tone = 1, neckLever, lain, gathered, risen;
        // Where the physics left each of the posed body's segments, when the posed body took over.
        private readonly Vector3[] leftAt = new Vector3[Count + 4];
        private readonly Quaternion[] leftTurned = new Quaternion[Count + 4];
        // Whether it gets up by itself after it has lain (it does, unless told not to: for the bench).
        public bool GetsUp = true;
        // How many times it has fallen, and got up.
        public int Falls { get; private set; }
        public int GotUp { get; private set; }
        private readonly ConfigurableJoint[] joints = new ConfigurableJoint[Count];
        // How hard it holds itself now (1: with all the strength it has left; little, lying still).
        public float Tone => tone;
        private Collider own;
        private PhysicalBalance balance;
        private bool balanced;

        public State Now { get; private set; }
        public float Since { get; private set; }
        public Rigidbody Part(int index) => parts[index];
        // Where its hips are, and its head, while it is let go.
        public Vector3 HipsAt => parts[Hips] != null ? parts[Hips].position : segments[Hips] != null ? segments[Hips].position : transform.position;
        public Vector3 HeadAt => parts[Head] != null ? parts[Head].position : segments[Head] != null ? segments[Head].position : transform.position;

        private void Awake()
        {
            body = GetComponent<ProceduralBiped>();
            physical = GetComponent<PhysicalBody>();
            miner = GetComponent<MinerBody>();
            motor = GetComponent<UnitMotor>();
        }

        private void Gather()
        {
            var s = miner.Solved;
            segments[Hips] = s.pelvis; segments[Trunk] = s.torso; segments[Head] = s.head;
            for (int i = 0; i < 2; i++)
            {
                segments[UpperArm + i] = s.upperArms[i]; segments[Forearm + i] = s.forearms[i];
                segments[Thigh + i] = s.thighs[i]; segments[Shin + i] = s.shins[i];
            }
        }

        // A limb's two parts bend one way: from the middle of the limb towards its joint (the knee, the elbow); where
        // the limb is straight, the way such a joint bends.
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

        // A limb part's own frame: along its length, and facing the way its limb bends.
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

        private Rigidbody Make(int index, string called, Vector3 at, Quaternion turned, float kilograms, Vector3 moves, PhysicsMaterial surface)
        {
            var part = new GameObject(called);
            part.transform.SetParent(held.transform, false);
            part.transform.SetPositionAndRotation(at, turned);
            var solid = part.AddComponent<Rigidbody>();
            solid.mass = kilograms;
            solid.linearDamping = .05f; solid.angularDamping = .6f;
            solid.interpolation = RigidbodyInterpolation.Interpolate;
            solid.collisionDetectionMode = CollisionDetectionMode.Continuous;
            solid.solverIterations = 14; solid.solverVelocityIterations = 4;
            solid.maxDepenetrationVelocity = 1.5f;
            solid.maxAngularVelocity = 20;
            solid.linearVelocity = moves;
            parts[index] = solid;
            return solid;
        }

        private static void Rod(Rigidbody part, float halfLength, float radius, PhysicsMaterial surface)
        {
            var solid = part.gameObject.AddComponent<CapsuleCollider>();
            solid.direction = 1;
            solid.radius = radius;
            solid.height = Mathf.Max(2 * radius, 2 * halfLength);
            solid.sharedMaterial = surface;
        }

        // A joint between a part and the part it hangs from. The joint's own rest is the body standing straight: the
        // part is put as it is then (rests, against what it hangs from) while the joint is made, and put back.
        private static ConfigurableJoint Join(Rigidbody part, Rigidbody from, Vector3 anchor, Quaternion rests, Vector4 turns)
        {
            Vector3 was = part.transform.position;
            Quaternion wasTurned = part.transform.rotation;
            Vector3 place = part.transform.TransformPoint(anchor);
            Quaternion standing = from.transform.rotation * rests;
            part.transform.SetPositionAndRotation(place - standing * anchor, standing);
            var joint = part.gameObject.AddComponent<ConfigurableJoint>();
            joint.axis = Vector3.right; joint.secondaryAxis = Vector3.up;
            joint.anchor = anchor;
            joint.autoConfigureConnectedAnchor = true;
            joint.connectedBody = from;
            joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
            // (The engine measures a joint's bending the other way round from the part's own turn.)
            joint.lowAngularXLimit = new SoftJointLimit { limit = -turns.y };
            joint.highAngularXLimit = new SoftJointLimit { limit = -turns.x };
            joint.angularYLimit = new SoftJointLimit { limit = turns.z };
            joint.angularZLimit = new SoftJointLimit { limit = turns.w };
            joint.enableCollision = false;
            joint.enablePreprocessing = false;
            joint.projectionMode = JointProjectionMode.PositionAndRotation;
            joint.projectionDistance = .02f; joint.projectionAngle = 5;
            joint.rotationDriveMode = RotationDriveMode.Slerp;
            part.transform.SetPositionAndRotation(was, wasTurned);
            return joint;
        }

        // A joint is held towards a turn from its rest (bent, twisted, to the side: degrees), with a strength (newton
        // metres) that is all given when it is FullAt from there.
        private static void Hold(ConfigurableJoint joint, float bent, float aside, float strength)
        {
            // (The engine takes the turn wanted the other way round.)
            joint.targetRotation = Quaternion.Inverse(Quaternion.Euler(bent, 0, aside));
            float spring = strength / (FullAt * Mathf.Deg2Rad);
            joint.slerpDrive = new JointDrive { positionSpring = spring, positionDamper = strength * Slows, maximumForce = strength };
        }

        // Each joint's strength now, and the pose it is held towards.
        private void Holds()
        {
            float strong = tone;
            float knee = physical.KneeNow * strong, hip = knee * HipGives, back = physical.BackNow * strong;
            float neck = NeckHolds * parts[Head].mass * Physics.gravity.magnitude * neckLever * physical.Strength * strong;
            // Which way the trunk is going down: its front to the ground (1), its back (-1).
            float front = Vector3.Dot(parts[Trunk].rotation * Vector3.forward, Vector3.down);
            // How far it is still falling (1), or lying easy (0).
            float falls = Mathf.InverseLerp(AtRest, 1, tone);
            Hold(joints[Trunk], Mathf.Lerp(TrunkEasy, TrunkBowed, falls), 0, back);
            // The head is kept from the ground: back when it falls on its front, forward when it falls on its back.
            Hold(joints[Head], -HeadKept * front, 0, neck);
            for (int i = 0; i < 2; i++)
            {
                // (An arm's bending forward is the other way round from a leg's: its rest faces back.)
                Hold(joints[UpperArm + i], Mathf.Lerp(ArmsBack, -ArmsForward, Mathf.InverseLerp(-1, 1, front)), 0, physical.ShoulderOf(i) * strong);
                Hold(joints[Forearm + i], -ElbowsBent, 0, physical.ElbowOf(i) * strong);
                Hold(joints[Thigh + i], Mathf.Lerp(HipsEasy, HipsBent, falls), 0, hip);
                Hold(joints[Shin + i], -Mathf.Lerp(KneesEasy, KneesBent, falls), 0, knee);
            }
        }

        // The body is let go: from now it follows the physics.
        public void LetGo()
        {
            if (Now != State.Up || !body.Ready || !miner.Ready || !physical.Ready) return;
            Gather();
            var s = miner.Solved;
            var shape = body.BodyProportions;
            // What it holds, it lets go of.
            if (TryGetComponent<PhysicalHands>(out var hands)) hands.Drop();
            if (TryGetComponent<ThingsInHand>(out var things)) things.enabled = false;
            balance = GetComponent<PhysicalBalance>();
            balanced = balance != null && balance.Acts;
            if (balance != null) balance.Acts = false;
            // Each part moves as the posed body was moving it.
            float dt = latelyTime - beforeTime;
            Vector3 Moves(int index) => dt > 1e-4f && beforeTime >= 0 ? Vector3.ClampMagnitude((lately[index] - before[index]) / dt, 6) : body.VelocityNow;

            held = new GameObject(name + " (let go)");
            var surface = new PhysicsMaterial("Body") { dynamicFriction = .7f, staticFriction = .8f, bounciness = 0, bounceCombine = PhysicsMaterialCombine.Minimum };
            float arm = physical.ArmRadius, leg = physical.LegRadius;
            Quaternion hips = s.pelvis.rotation, chest = s.torso.rotation;

            // The hips: a rod across them.
            var pelvis = Make(Hips, "Hips", s.pelvis.position, hips, Weighs("Pelvis"), Moves(Hips), surface);
            float girth = Mathf.Clamp(shape.bodyFront, leg * 1.5f, .3f * shape.hipHeight);
            var across = pelvis.gameObject.AddComponent<CapsuleCollider>();
            across.direction = 0; across.radius = girth; across.height = 2 * (shape.hipWidth + girth); across.sharedMaterial = surface;
            // The trunk, from the waist to the neck.
            var trunk = Make(Trunk, "Trunk", s.torso.position, chest, Weighs("Spine", "Chest", "Neck"), Moves(Trunk), surface);
            float breadth = Mathf.Max(shape.shoulder.x - arm, .5f * shape.shoulder.x);
            Rod(trunk, shape.torsoRise + .5f * breadth, breadth, surface);
            joints[Trunk] = Join(trunk, pelvis, new Vector3(0, -shape.torsoRise, 0), Quaternion.identity, Waist);
            // The head.
            var head = Make(Head, "Head", s.head.position, s.head.rotation, Weighs("Head"), Moves(Head), surface);
            var skull = head.gameObject.AddComponent<SphereCollider>();
            skull.radius = Mathf.Max(.06f, shape.headHalf); skull.sharedMaterial = surface;
            neckLever = Mathf.Max(.03f, shape.headRise - 2 * shape.torsoRise);
            joints[Head] = Join(head, trunk, new Vector3(0, -neckLever, 0), Quaternion.identity, Neck);

            for (int i = 0; i < 2; i++)
            {
                string side = i == 0 ? " (left)" : " (right)";
                // An arm: its elbow bends back.
                Ends(s.upperArms[i], out var shoulder, out var elbow);
                Ends(s.forearms[i], out _, out var wrist);
                Vector3 back = Bends(shoulder, elbow, wrist, -(chest * Vector3.forward));
                var upper = Make(UpperArm + i, "Upper arm" + side, s.upperArms[i].position, Along(s.upperArms[i], back), Weighs("UpperArm." + (i == 0 ? "L" : "R")), Moves(UpperArm + i), surface);
                Rod(upper, s.upperArms[i].localScale.y, arm, surface);
                joints[UpperArm + i] = Join(upper, trunk, new Vector3(0, -s.upperArms[i].localScale.y, 0), Quaternion.LookRotation(Vector3.back, Vector3.down), Shoulder);
                var fore = Make(Forearm + i, "Forearm" + side, s.forearms[i].position, Along(s.forearms[i], back),
                    Weighs("Forearm." + (i == 0 ? "L" : "R"), "Hand." + (i == 0 ? "L" : "R")), Moves(Forearm + i), surface);
                // The hand goes on past the wrist.
                Rod(fore, s.forearms[i].localScale.y + arm, arm, surface);
                joints[Forearm + i] = Join(fore, upper, new Vector3(0, -s.forearms[i].localScale.y, 0), Quaternion.identity, Elbow);

                // A leg: its knee bends forward.
                Ends(s.thighs[i], out var hip, out var knee);
                Ends(s.shins[i], out _, out var ankle);
                Vector3 forward = Bends(hip, knee, ankle, hips * Vector3.forward);
                var thigh = Make(Thigh + i, "Thigh" + side, s.thighs[i].position, Along(s.thighs[i], forward), Weighs("Thigh." + (i == 0 ? "L" : "R")), Moves(Thigh + i), surface);
                Rod(thigh, s.thighs[i].localScale.y, leg * 1.25f, surface);
                joints[Thigh + i] = Join(thigh, pelvis, new Vector3(0, -s.thighs[i].localScale.y, 0), Quaternion.LookRotation(Vector3.forward, Vector3.down), Hip);
                string l = i == 0 ? "L" : "R";
                var shin = Make(Shin + i, "Shin" + side, s.shins[i].position, Along(s.shins[i], forward), Weighs("Shin." + l, "Foot." + l, "Toe." + l), Moves(Shin + i), surface);
                Rod(shin, s.shins[i].localScale.y, leg, surface);
                joints[Shin + i] = Join(shin, thigh, new Vector3(0, -s.shins[i].localScale.y, 0), Quaternion.identity, Knee);
                // Its boot goes with the shin, as it stands.
                footAt[i] = shin.transform.InverseTransformPoint(s.feet[i].position);
                footTurn[i] = Quaternion.Inverse(shin.transform.rotation) * s.feet[i].rotation;
                var boot = new GameObject("Boot");
                boot.transform.SetParent(shin.transform, false);
                boot.transform.SetPositionAndRotation(s.feet[i].position, s.feet[i].rotation);
                var sole = boot.AddComponent<BoxCollider>();
                sole.size = s.feet[i].localScale; sole.sharedMaterial = surface;
                if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null)
                {
                    toeAt[i] = shin.transform.InverseTransformPoint(s.toes[i].position);
                    toeTurn[i] = Quaternion.Inverse(shin.transform.rotation) * s.toes[i].rotation;
                }
            }
            // Its own parts do not strike one another, but for those that would fold through each other in a heap and
            // stand well clear of each other in a standing body: a shin against the trunk, the head, the other shin
            // and the arms; a thigh against the head; a forearm against the trunk and the head.
            var solids = held.GetComponentsInChildren<Collider>();
            for (int a = 0; a < solids.Length; a++)
                for (int b = a + 1; b < solids.Length; b++)
                    if (!Strike(Which(solids[a]), Which(solids[b]))) Physics.IgnoreCollision(solids[a], solids[b]);
            tone = 1;
            Holds();
            // The unit's own solid (what it is picked and avoided by) is out of the way while it lies.
            own = GetComponent<Collider>();
            if (own != null) own.enabled = false;
            ridesUp = Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out var under, 6, Ground) ? transform.position.y - under.point.y : 0;
            if (motor != null) motor.CarriedOff();
            body.LetGo = true;
            Now = State.Falling; Since = 0; still = 0; lain = 0;
            Falls++;
        }

        // How deep the crouch was that it last got up from (metres its hips were lower than standing), and how many
        // times it has gathered itself and lain down again, its legs not yet able to raise it.
        public float RoseFrom { get; private set; }
        public int LayDownAgain { get; private set; }

        private float BowedAt(float low) => CrouchBow * Mathf.Lerp(.5f, 1, low / (CrouchSink * body.StandingHipHeight));

        // What its knees would be asked for (as a share of what they have now) to hold it posed in a crouch so deep:
        // each knee holds up half the body by how far it stands out beyond a straight leg's knee, as PhysicalBalance
        // has it. The posed body is posed so, to see.
        private float KneesAsked(float low)
        {
            body.Bow(BowedAt(low), 400); body.Sink(low);
            body.ResetPose();
            float stands = Mathf.Max(body.KneeOut(0), body.KneeOut(1)) - body.KneeOutStraight;
            return .5f * physical.Mass * Physics.gravity.magnitude * Mathf.Max(0, stands) / Mathf.Max(1e-3f, physical.KneeNow);
        }

        // The deepest crouch its legs can raise it from now, no deeper than the crouch it gathers into; less than
        // nothing if there is none worth the name.
        private float Crouch()
        {
            float deepest = CrouchSink * body.StandingHipHeight, least = LeastCrouch * body.StandingHipHeight;
            if (KneesAsked(deepest) <= RisesAt) return deepest;
            if (KneesAsked(least) > RisesAt) return -1;
            for (int k = 0; k < 6; k++)
            {
                float middle = (least + deepest) * .5f;
                if (KneesAsked(middle) <= RisesAt) least = middle; else deepest = middle;
            }
            return least;
        }

        // Gathered into its crouch where it lies, the body is given back to the posed body: crouched at that place,
        // facing the way the crouch faces. Each part then goes from where the physics left it to where the posed
        // crouch has it.
        private void GiveBack()
        {
            var s = miner.Solved;
            for (int i = 0; i < Count; i++) { leftAt[i] = parts[i].transform.position; leftTurned[i] = parts[i].transform.rotation; }
            for (int i = 0; i < 2; i++)
            {
                var shin = parts[Shin + i].transform;
                leftAt[Count + i] = shin.TransformPoint(footAt[i]); leftTurned[Count + i] = shin.rotation * footTurn[i];
                leftAt[Count + 2 + i] = shin.TransformPoint(toeAt[i]); leftTurned[Count + 2 + i] = shin.rotation * toeTurn[i];
            }
            // Which way it faces when it is up. Lying on its front it comes up towards its head; on its back, towards
            // its feet; on its side, or with its trunk upright, the way its chest faces.
            Vector3 spine = parts[Head].position - parts[Hips].position, chest = parts[Trunk].rotation * Vector3.forward;
            Vector3 along = Vector3.ProjectOnPlane(spine, Vector3.up), faces = Vector3.ProjectOnPlane(chest, Vector3.up);
            bool upright = spine.y > .7f * spine.magnitude;
            if (!upright && along.sqrMagnitude > 1e-4f && Mathf.Abs(chest.y) > .3f) faces = chest.y < 0 ? along : -along;
            if (faces.sqrMagnitude < 1e-6f) faces = transform.forward;
            Follow();
            transform.rotation = Quaternion.LookRotation(faces.normalized);
            // What its balance had done to its legs before it fell is over: its knees given way, its feet set apart.
            if (balance != null) balance.Afresh();
            body.Crouch(0); body.SetStance(0, 0);
            // The crouch its legs can raise it from. If there is none, it lies down again, and they rest.
            float low = Crouch();
            if (low < 0)
            {
                Now = State.Lying; lain = LiesFor - RestsMore;
                LayDownAgain++;
                return;
            }
            Destroy(held);
            held = null;
            for (int i = 0; i < Count; i++) { parts[i] = null; joints[i] = null; }
            // The posed body, crouched there at once.
            float bowed = BowedAt(low);
            RoseFrom = low;
            body.Bow(bowed, 400); body.Sink(low);
            body.LetGo = false;
            body.ResetPose();
            if (TryGetComponent<PhysicalBack>(out var back)) { back.Is(bowed); back.Want(bowed); }
            Now = State.Rising; risen = 0;
        }

        // The posed body has taken over: it stands up, at its own pace.
        private void StandUp()
        {
            if (own != null) own.enabled = true;
            if (balance != null) { balance.Afresh(); balance.Acts = balanced; }
            if (TryGetComponent<ThingsInHand>(out var things)) things.enabled = true;
            if (TryGetComponent<PhysicalBack>(out var back)) back.Want(0);
            body.Bow(0); body.Sink(0);
            Now = State.Up; beforeTime = latelyTime = -1;
            GotUp++;
        }

        private int Which(Collider solid)
        {
            var of = solid.attachedRigidbody;
            for (int i = 0; i < Count; i++) if (parts[i] == of) return i;
            return -1;
        }

        private static bool Shinbone(int part) => part == Shin || part == Shin + 1;
        private static bool Lower(int part) => part == Forearm || part == Forearm + 1;
        private static bool Strike(int a, int b)
        {
            if (a < 0 || b < 0 || a == b) return false;
            if (a > b) (a, b) = (b, a);
            if (Shinbone(b)) return a == Trunk || a == Head || Shinbone(a) || Lower(a) || a == UpperArm || a == UpperArm + 1;
            if (b == Thigh || b == Thigh + 1) return a == Head;
            if (Lower(b)) return a == Trunk || a == Head;
            return false;
        }

        // A force on the body while it is let go, at a place (a rope, a blow).
        public void Push(Vector3 force, Vector3 at)
        {
            // (Getting up, it is the posed body again: a force then is lost on it.)
            if (Now == State.Up || Now == State.Rising || parts[Trunk] == null) return;
            // On the part it is nearest to.
            int nearest = Trunk;
            float least = float.MaxValue;
            for (int i = 0; i < Count; i++)
            {
                float away = (parts[i].worldCenterOfMass - at).sqrMagnitude;
                if (away < least) { least = away; nearest = i; }
            }
            parts[nearest].AddForceAtPosition(force, at);
        }

        // The posed body takes over again, standing where the body lies. (Until getting up is built.)
        public void TakeBack()
        {
            if (Now == State.Up) return;
            if (Now == State.Rising)
            {
                // The posed body has it already: it only stands as it is posed.
                if (own != null) own.enabled = true;
                if (balance != null) balance.Acts = balanced;
                if (TryGetComponent<ThingsInHand>(out var had)) had.enabled = true;
                Now = State.Up; beforeTime = latelyTime = -1;
                return;
            }
            Follow();
            if (held != null) Destroy(held);
            held = null;
            for (int i = 0; i < Count; i++) { parts[i] = null; joints[i] = null; }
            if (own != null) own.enabled = true;
            if (balance != null) balance.Acts = balanced;
            if (TryGetComponent<ThingsInHand>(out var things)) things.enabled = true;
            body.LetGo = false;
            body.ResetPose();
            Now = State.Up; beforeTime = latelyTime = -1;
        }

        // The unit's own place goes with its hips.
        private void Follow()
        {
            // (When the place itself is being put away, its parts may be gone before it.)
            if (parts[Hips] == null) return;
            Vector3 at = parts[Hips].position;
            float ground = Physics.Raycast(new Vector3(at.x, at.y + 2, at.z), Vector3.down, out var under, 8, Ground) ? under.point.y : transform.position.y - ridesUp;
            transform.position = new Vector3(at.x, ground + ridesUp, at.z);
        }

        private void FixedUpdate()
        {
            if (Now == State.Up || Now == State.Rising) return;
            Since += Time.fixedDeltaTime;
            bool moving = false;
            for (int i = Hips; i <= Head && !moving; i++)
                moving = parts[i].linearVelocity.sqrMagnitude > Still * Still || parts[i].angularVelocity.sqrMagnitude > StillTurning * StillTurning;
            still = moving ? 0 : still + Time.fixedDeltaTime;
            if (Now == State.Falling && still >= LiesAfter) { Now = State.Lying; lain = 0; }
            else if (Now == State.Lying || Now == State.Gathering)
            {
                bool thrown = false;
                for (int i = Hips; i <= Head && !thrown; i++) thrown = parts[i].linearVelocity.sqrMagnitude > Thrown * Thrown;
                if (thrown) Now = State.Falling;
            }
            // Having lain a moment, it gathers itself: into its crouch, with its own strength, where it lies.
            if (Now == State.Lying)
            {
                lain += Time.fixedDeltaTime;
                if (GetsUp && lain >= LiesFor) { Now = State.Gathering; gathered = 0; }
            }
            else if (Now == State.Gathering)
            {
                gathered += Time.fixedDeltaTime;
                if ((gathered >= GathersAtLeast && still >= .3f) || gathered >= GathersAtMost) { GiveBack(); return; }
            }
            // Falling, and gathering itself, it holds itself with all it has; lying, it lets go.
            tone = Mathf.MoveTowards(tone, Now == State.Lying ? AtRest : 1, Time.fixedDeltaTime / LetsGoOver);
            Holds();
        }

        // After the posed body would have posed, and before the model's bones are turned to it.
        private void LateUpdate()
        {
            if (Now == State.Up)
            {
                // Where each part of the posed body is, this frame and the last: how it is moving, if it is let go.
                if (!body.Ready || !miner.Ready) return;
                if (segments[Hips] == null) Gather();
                for (int i = 0; i < Count; i++) { before[i] = lately[i]; lately[i] = segments[i].position; }
                beforeTime = latelyTime; latelyTime = Time.time;
                return;
            }
            var s = miner.Solved;
            if (Now == State.Rising)
            {
                // The posed body has posed its crouch this frame: each segment is between where the physics left it and
                // there, and is there when the moment is over. Then it stands up.
                risen += Time.deltaTime;
                float share = Mathf.SmoothStep(0, 1, risen / TakesOver);
                void Between(Transform segment, int k)
                {
                    segment.SetPositionAndRotation(Vector3.Lerp(leftAt[k], segment.position, share), Quaternion.Slerp(leftTurned[k], segment.rotation, share));
                }
                // (A limb's segment is turned any way about its own length: only where it lies along counts.)
                for (int i = 0; i < Count; i++)
                {
                    if (i >= UpperArm)
                    {
                        Vector3 was = leftTurned[i] * Vector3.up, will = segments[i].rotation * Vector3.up;
                        segments[i].SetPositionAndRotation(Vector3.Lerp(leftAt[i], segments[i].position, share),
                            Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(was, will, share).normalized));
                    }
                    else Between(segments[i], i);
                }
                for (int i = 0; i < 2; i++)
                {
                    Between(s.feet[i], Count + i);
                    if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null) Between(s.toes[i], Count + 2 + i);
                }
                if (risen >= TakesOver) StandUp();
                return;
            }
            Follow();
            for (int i = 0; i < Count; i++) segments[i].SetPositionAndRotation(parts[i].transform.position, parts[i].transform.rotation);
            for (int i = 0; i < 2; i++)
            {
                var shin = parts[Shin + i].transform;
                s.feet[i].SetPositionAndRotation(shin.TransformPoint(footAt[i]), shin.rotation * footTurn[i]);
                if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null) s.toes[i].SetPositionAndRotation(shin.TransformPoint(toeAt[i]), shin.rotation * toeTurn[i]);
            }
        }

        private void OnDisable() { if (Now != State.Up) TakeBack(); }
    }
}
