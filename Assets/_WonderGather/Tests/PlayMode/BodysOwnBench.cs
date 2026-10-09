using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // What the ground does to a boot in a step of the physics: how hard it pushes (newton seconds, summed), and where
    // (the middle of that push). Cleared by whoever reads it.
    public sealed class Touching : MonoBehaviour
    {
        public Vector3 pushed, where;
        public float up;
        public void Clear() { pushed = Vector3.zero; where = Vector3.zero; up = 0; }
        private void Take(Collision touch)
        {
            for (int k = 0; k < touch.contactCount; k++)
            {
                var point = touch.GetContact(k);
                Vector3 gave = point.impulse;
                pushed += gave;
                float lifts = Mathf.Max(0, gave.y);
                where += point.point * lifts; up += lifts;
            }
        }
        private void OnCollisionEnter(Collision touch) => Take(touch);
        private void OnCollisionStay(Collision touch) => Take(touch);
    }

    // The bench of S3b's step 0 (Docs/NextMilestonePlan.md): what the body's own body is to be made of.
    //
    // A miner's body is made again as thirteen weighted parts (the eleven of the let-go body, and its two boots as
    // parts of their own, on ankles), each joint held towards the pose it stands in with no more than the strength
    // that joint has. It is made two ways: of rigid bodies and joints between them (as the let-go body is), and as an
    // articulation (one jointed body, solved as one). Each stands for a while, is pulled at the chest, is dropped
    // through the air while it moves its limbs (nothing may push it there but its weight), and stands in a crowd (what
    // it costs). Nothing here is in the game: it is to choose by.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.BodysOwnBench -ownOut <folder>
    //   [-ownMiner Round] [-ownFullAt 35,5] [-ownSlows 0.08] [-ownFor 30] [-ownPulls 50,100,200] [-ownCrowd 1,25,100]
    //   [-ownKinds joints,articulation] [-ownAnkle 0.7] [-ownSize 400]
    public sealed class BodysOwnBench
    {
        private const int Hips = 0, Trunk = 1, Head = 2, UpperArm = 3, Forearm = 5, Thigh = 7, Shin = 9, Foot = 11, Count = 13;
        private static readonly int[] From = { -1, Hips, Trunk, Trunk, Trunk, UpperArm, UpperArm + 1, Hips, Hips, Thigh, Thigh + 1, Shin, Shin + 1 };
        private static float Step = .02f;
        private static int Iterations = 14, VelocityIterations = 4;
        // (A boot's turning weight, so many times what its shape gives it; and its weight, if it is set.)
        private static float BootTurns = 1, BootWeighs = 0;
        // (A joint's spring set by rule: no stiffer than makes its own rate, times the step, this much, by the turning
        // weight of the lighter side of the joint; and damped this share of what just stops that side swinging. 0: not.)
        private static float Rule = 0, Damped = 1;
        // (Which law keeps it: 1, back to where its weight was; 2, from where its weight is going, feeling what
        // pushes it (over this long, seconds; 0: not) and leaning against it (this fast, a second; no further than
        // this, metres).)
        private static int Law = 2;
        private static float Feels = .1f, Leans = 3, LeansBy = 1, LeansAtMost = .1f, Tracks = 8, Comfort = .03f, KeepsBehind = .035f;
        // (How hard its hips right its trunk, for each kilogram of it, and how much that is damped; how near the edge
        // of a sole its push may act; how hard it keeps its height, and how much that is damped; how quick it is.)
        private static float Rights = 9, RightsDamped = .2f, Edge = .01f, Rises = 100, RisesDamped = 20, Quick = 4;

        private struct Solid
        {
            public int kind;            // 0 a rod, 1 a ball, 2 a box
            public Vector3 at; public Quaternion turned; public Vector3 size; public int direction; public float radius, height;
        }

        private sealed class Planned
        {
            public string name;
            public Vector3 at; public Quaternion turned;
            public float mass, strength;
            public Vector3 anchor;
            public bool hinge;
            public readonly List<Solid> solids = new List<Solid>();
        }

        // A body as it is built: its parts, of either kind.
        private sealed class Built
        {
            public GameObject root;
            public bool articulated;
            public readonly Rigidbody[] solid = new Rigidbody[Count];
            public readonly ArticulationBody[] link = new ArticulationBody[Count];
            public readonly ConfigurableJoint[] joint = new ConfigurableJoint[Count];
            public readonly Quaternion[] rested = new Quaternion[Count];
            public Transform Of(int i) => articulated ? link[i].transform : solid[i].transform;
            public float Mass(int i) => articulated ? link[i].mass : solid[i].mass;
            public Vector3 Centre(int i) => articulated ? link[i].worldCenterOfMass : solid[i].worldCenterOfMass;
            public Vector3 Goes(int i) => articulated ? link[i].GetPointVelocity(link[i].worldCenterOfMass) : solid[i].GetPointVelocity(solid[i].worldCenterOfMass);
            public Vector3 Turns(int i) => articulated ? link[i].angularVelocity : solid[i].angularVelocity;
            public Vector3 Inertia(int i) => articulated ? link[i].inertiaTensor : solid[i].inertiaTensor;
            public Quaternion InertiaTurned(int i) => articulated ? link[i].transform.rotation * link[i].inertiaTensorRotation : solid[i].rotation * solid[i].inertiaTensorRotation;
            public void Push(int i, Vector3 force, Vector3 at) { if (articulated) link[i].AddForceAtPosition(force, at); else solid[i].AddForceAtPosition(force, at); }
            public void Turn(int i, Vector3 torque) { if (articulated) link[i].AddTorque(torque); else solid[i].AddTorque(torque); }
            // Kept by its own torques: where its weight was and how high, when it began; and what each joint gave last.
            public bool keeps, begun;
            public Vector3 centre;
            // (The second law: where its weight is at ease, over the ground; the place it holds its weight now, which
            // drifts when it leans; what it feels pushing it from outside (m/s2); and the step before.)
            public Vector3 rest, held, felt, wentBefore, weightBefore, actedBefore;
            public bool hasBefore;
            public Quaternion upright;
            public float height;
            public readonly float[] gave = new float[Count];
            public float outside;
            public readonly Touching[] touches = new Touching[2];
            public Vector3 acted;
            // What the ground really gave it in the last step: how hard it bore the body up (newtons), where that
            // acted, and whether it bore it at all. And how far the real push has been falling short of the meant one.
            public Vector3 realAt, short_;
            public float realUp;
            public bool realKnown, atItsEdge;
            public void Ground()
            {
                Vector3 at = Vector3.zero; float up = 0;
                for (int i = 0; i < 2; i++) if (touches[i] != null) { at += touches[i].where; up += touches[i].up; touches[i].Clear(); }
                realUp = up / Step; realKnown = up > 1e-6f;
                if (realKnown) realAt = at / up;
            }
            public float Whole() { float m = 0; for (int i = 0; i < Count; i++) m += Mass(i); return m; }
            public Vector3 Weight() { Vector3 c = Vector3.zero; float m = 0; for (int i = 0; i < Count; i++) { c += Centre(i) * Mass(i); m += Mass(i); } return c / m; }
            public Vector3 WeightGoes() { Vector3 v = Vector3.zero; float m = 0; for (int i = 0; i < Count; i++) { v += Goes(i) * Mass(i); m += Mass(i); } return v / m; }
            // Its turning about its own weight (kg m2/s): what no joint of its own can change.
            public Vector3 Spin()
            {
                Vector3 c = Weight(), v = WeightGoes(), spin = Vector3.zero;
                for (int i = 0; i < Count; i++)
                {
                    spin += Vector3.Cross(Centre(i) - c, Mass(i) * (Goes(i) - v));
                    Quaternion q = InertiaTurned(i);
                    spin += q * Vector3.Scale(Inertia(i), Quaternion.Inverse(q) * Turns(i));
                }
                return spin;
            }
            // What a joint's own holding gives now (newton metres).
            public float Gives(int i)
            {
                if (From[i] < 0) return 0;
                if (keeps && i >= Thigh) return gave[i];
                if (!articulated) return joint[i].currentTorque.magnitude;
                var gives = link[i].driveForce;
                float sum = 0;
                for (int k = 0; k < gives.dofCount; k++) sum += gives[k] * gives[k];
                return Mathf.Sqrt(sum);
            }
            // How far a joint is from where it is held (degrees).
            public float Off(int i)
            {
                if (From[i] < 0) return 0;
                Quaternion now = Quaternion.Inverse(Of(From[i]).rotation) * Of(i).rotation;
                return Quaternion.Angle(rested[i], now);
            }
        }

        private static float[] Numbers(string argument, params float[] otherwise)
        {
            string given = CaptureTools.Argument(argument);
            if (string.IsNullOrEmpty(given)) return otherwise;
            var parts = given.Split(',');
            var values = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++) values[i] = float.Parse(parts[i], CultureInfo.InvariantCulture);
            return values;
        }

        private static Solid Read(Collider solid, Transform part)
        {
            var s = new Solid { at = part.InverseTransformPoint(solid.transform.position), turned = Quaternion.Inverse(part.rotation) * solid.transform.rotation };
            if (solid is CapsuleCollider rod) { s.kind = 0; s.direction = rod.direction; s.radius = rod.radius; s.height = rod.height; s.at = part.InverseTransformPoint(rod.transform.TransformPoint(rod.center)); }
            else if (solid is SphereCollider ball) { s.kind = 1; s.radius = ball.radius; s.at = part.InverseTransformPoint(ball.transform.TransformPoint(ball.center)); }
            else if (solid is BoxCollider box) { s.kind = 2; s.size = box.size; s.at = part.InverseTransformPoint(box.transform.TransformPoint(box.center)); }
            return s;
        }

        // The plan of the body, read from the let-go body as the game makes it, with the boots taken off the shins.
        private static Planned[] Plan(PhysicalFall fall, PhysicalBody physical, float ankleGives)
        {
            var plan = new Planned[Count];
            for (int i = 0; i < PhysicalFall.Count; i++)
            {
                var part = fall.Part(i);
                var p = new Planned { name = part.name, at = part.transform.position, turned = part.transform.rotation, mass = part.mass };
                var joint = part.GetComponent<ConfigurableJoint>();
                if (joint != null) p.anchor = joint.anchor;
                foreach (var solid in part.GetComponents<Collider>()) p.solids.Add(Read(solid, part.transform));
                plan[i] = p;
            }
            float knee = physical.KneeNow;
            plan[Trunk].strength = physical.BackNow;
            plan[Head].strength = 3 * plan[Head].mass * Physics.gravity.magnitude * Mathf.Abs(plan[Head].anchor.y) * physical.Strength;
            for (int i = 0; i < 2; i++)
            {
                plan[UpperArm + i].strength = physical.ShoulderOf(i);
                plan[Forearm + i].strength = physical.ElbowOf(i); plan[Forearm + i].hinge = true;
                plan[Thigh + i].strength = knee * 1.5f;
                plan[Shin + i].strength = knee; plan[Shin + i].hinge = true;
                // The boot: a part of its own, on an ankle at the shin's lower end.
                var shin = fall.Part(PhysicalFall.Shin + i);
                var boot = shin.transform.Find("Boot");
                string l = i == 0 ? "L" : "R";
                float weighs = 0;
                for (int k = 0; k < physical.PartCount; k++)
                {
                    var w = physical.PartAt(k);
                    if (w.bone != null && (w.bone.name.StartsWith("Foot." + l) || w.bone.name.StartsWith("Toe." + l))) weighs += w.mass;
                }
                weighs = Mathf.Clamp(weighs, .1f, plan[Shin + i].mass * .6f);
                plan[Shin + i].mass -= weighs;
                var foot = new Planned { name = "Foot" + (i == 0 ? " (left)" : " (right)"), at = boot.position, turned = boot.rotation, mass = weighs, strength = knee * ankleGives };
                Vector3 ankle = shin.transform.TransformPoint(-plan[Shin + i].anchor);
                foot.anchor = Quaternion.Inverse(boot.rotation) * (ankle - boot.position);
                foot.solids.Add(Read(boot.GetComponent<BoxCollider>(), boot));
                plan[Foot + i] = foot;
            }
            return plan;
        }

        private static void Shape(GameObject part, Planned p, PhysicsMaterial surface, bool seen)
        {
            foreach (var s in p.solids)
            {
                var holder = new GameObject("Solid");
                holder.transform.SetParent(part.transform, false);
                holder.transform.localPosition = s.at; holder.transform.localRotation = s.turned;
                Collider made;
                GameObject look = null;
                if (s.kind == 0)
                {
                    var rod = holder.AddComponent<CapsuleCollider>(); rod.direction = s.direction; rod.radius = s.radius; rod.height = s.height; made = rod;
                    if (seen)
                    {
                        look = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        look.transform.SetParent(holder.transform, false);
                        look.transform.localRotation = s.direction == 0 ? Quaternion.Euler(0, 0, 90) : s.direction == 2 ? Quaternion.Euler(90, 0, 0) : Quaternion.identity;
                        look.transform.localScale = new Vector3(2 * s.radius, Mathf.Max(s.height, 2 * s.radius) * .5f, 2 * s.radius);
                    }
                }
                else if (s.kind == 1)
                {
                    var ball = holder.AddComponent<SphereCollider>(); ball.radius = s.radius; made = ball;
                    if (seen) { look = GameObject.CreatePrimitive(PrimitiveType.Sphere); look.transform.SetParent(holder.transform, false); look.transform.localScale = Vector3.one * 2 * s.radius; }
                }
                else
                {
                    var box = holder.AddComponent<BoxCollider>(); box.size = s.size; made = box;
                    if (seen) { look = GameObject.CreatePrimitive(PrimitiveType.Cube); look.transform.SetParent(holder.transform, false); look.transform.localScale = s.size; }
                }
                made.sharedMaterial = surface;
                if (look != null) UnityEngine.Object.Destroy(look.GetComponent<Collider>());
            }
        }

        // The body, built at the plan's place and moved by so much. fullAt: a joint gives all it has when it is this
        // far (degrees) from where it is held; slows: its damping, in seconds (the damper over the spring).
        // The turning weight (kg m2) of the lighter side of a joint, about the joint: of all that hangs beyond it, or of
        // all the rest, whichever is less. (Each part as a point at its middle, and a rod of its own length.)
        private static float Lighter(Planned[] plan, int joint)
        {
            Vector3 about = plan[joint].at + plan[joint].turned * plan[joint].anchor;
            float beyond = 0, rest = 0;
            for (int k = 0; k < Count; k++)
            {
                float length = 0;
                foreach (var solid in plan[k].solids) length = Mathf.Max(length, solid.kind == 0 ? solid.height : solid.kind == 1 ? 2 * solid.radius : Mathf.Max(solid.size.x, Mathf.Max(solid.size.y, solid.size.z)));
                float turning = plan[k].mass * ((plan[k].at - about).sqrMagnitude + length * length / 12);
                bool hangs = false;
                for (int up = k; up >= 0; up = From[up]) if (up == joint) { hangs = true; break; }
                if (hangs) beyond += turning; else rest += turning;
            }
            return Mathf.Max(1e-4f, Mathf.Min(beyond, rest));
        }

        // A joint's spring and its damper: by its strength and how far it may be from its pose; and, if the rule is
        // on, no stiffer than the rule allows.
        private static void Sprung(Planned[] plan, int i, float fullAt, float slows, out float spring, out float damper)
        {
            spring = plan[i].strength / (fullAt * Mathf.Deg2Rad); damper = spring * slows;
            if (Rule <= 0) return;
            float lighter = Lighter(plan, i);
            spring = Mathf.Min(spring, Rule * Rule / (Step * Step) * lighter);
            damper = Damped * 2 * Mathf.Sqrt(spring * lighter);
        }

        private static Built Build(Planned[] plan, bool articulated, Vector3 moved, float fullAt, float slows, float drag, float turnDrag, bool seen, bool keeps = false)
        {
            var built = new Built { articulated = articulated, keeps = keeps, root = new GameObject(articulated ? "Body (articulation)" : "Body (joints)") };
            var surface = new PhysicsMaterial("Body") { dynamicFriction = .7f, staticFriction = .8f, bounciness = 0, bounceCombine = PhysicsMaterialCombine.Minimum };
            var made = new GameObject[Count];
            for (int i = 0; i < Count; i++)
            {
                made[i] = new GameObject(plan[i].name);
                // (An articulation's parts must hang from each other in the hierarchy too.)
                made[i].transform.SetParent(articulated && From[i] >= 0 ? made[From[i]].transform : built.root.transform, true);
                made[i].transform.SetPositionAndRotation(plan[i].at + moved, plan[i].turned);
                Shape(made[i], plan[i], surface, seen);
            }
            for (int i = 0; i < Count; i++)
            {
                var p = plan[i];
                float spring = 0, damper = 0;
                if (From[i] >= 0) Sprung(plan, i, fullAt, slows, out spring, out damper);
                if (From[i] >= 0) built.rested[i] = Quaternion.Inverse(made[From[i]].transform.rotation) * made[i].transform.rotation;
                if (articulated)
                {
                    var link = made[i].AddComponent<ArticulationBody>();
                    link.mass = p.mass;
                    link.linearDamping = drag; link.angularDamping = turnDrag; link.jointFriction = 0;
                    link.useGravity = true;
                    link.maxAngularVelocity = 20; link.maxDepenetrationVelocity = 1.5f;
                    link.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    if (From[i] < 0) { link.immovable = false; link.solverIterations = Iterations; link.solverVelocityIterations = VelocityIterations; }
                    else
                    {
                        link.jointType = p.hinge ? ArticulationJointType.RevoluteJoint : ArticulationJointType.SphericalJoint;
                        link.anchorPosition = p.anchor; link.anchorRotation = Quaternion.identity;
                        link.matchAnchors = true;
                        link.twistLock = ArticulationDofLock.FreeMotion;
                        if (!p.hinge) { link.swingYLock = ArticulationDofLock.FreeMotion; link.swingZLock = ArticulationDofLock.FreeMotion; }
                        var drive = new ArticulationDrive { stiffness = spring, damping = damper, forceLimit = p.strength, target = 0, targetVelocity = 0, driveType = ArticulationDriveType.Force };
                        link.xDrive = drive;
                        if (!p.hinge) { link.yDrive = drive; link.zDrive = drive; }
                    }
                    built.link[i] = link;
                }
                else
                {
                    var solid = made[i].AddComponent<Rigidbody>();
                    solid.mass = p.mass;
                    solid.linearDamping = drag; solid.angularDamping = turnDrag;
                    solid.interpolation = RigidbodyInterpolation.None;
                    solid.collisionDetectionMode = CollisionDetectionMode.Continuous;
                    solid.solverIterations = Iterations; solid.solverVelocityIterations = VelocityIterations;
                    solid.maxDepenetrationVelocity = 1.5f; solid.maxAngularVelocity = 20;
                    built.solid[i] = solid;
                }
            }
            if (!articulated)
                for (int i = 0; i < Count; i++)
                {
                    if (From[i] < 0) continue;
                    var p = plan[i];
                    var joint = made[i].AddComponent<ConfigurableJoint>();
                    joint.axis = Vector3.right; joint.secondaryAxis = Vector3.up;
                    joint.anchor = p.anchor;
                    joint.autoConfigureConnectedAnchor = true;
                    joint.connectedBody = built.solid[From[i]];
                    joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
                    joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
                    joint.lowAngularXLimit = new SoftJointLimit { limit = -150 };
                    joint.highAngularXLimit = new SoftJointLimit { limit = 150 };
                    joint.angularYLimit = new SoftJointLimit { limit = p.hinge ? 5 : 80 };
                    joint.angularZLimit = new SoftJointLimit { limit = p.hinge ? 3 : 80 };
                    joint.enableCollision = false; joint.enablePreprocessing = false;
                    joint.projectionMode = JointProjectionMode.PositionAndRotation;
                    joint.projectionDistance = .02f; joint.projectionAngle = 5;
                    joint.rotationDriveMode = RotationDriveMode.Slerp;
                    Sprung(plan, i, fullAt, slows, out float spring, out float damper);
                    joint.slerpDrive = new JointDrive { positionSpring = spring, positionDamper = damper, maximumForce = p.strength };
                    joint.targetRotation = Quaternion.identity;
                    built.joint[i] = joint;
                }
            if (BootTurns != 1)
                for (int i = Foot; i < Count; i++)
                {
                    if (articulated) built.link[i].inertiaTensor = built.link[i].inertiaTensor * BootTurns;
                    else built.solid[i].inertiaTensor = built.solid[i].inertiaTensor * BootTurns;
                }
            for (int i = 0; i < 2; i++) built.touches[i] = made[Foot + i].AddComponent<Touching>();
            // Its own parts do not strike one another (it only stands here).
            var solids = built.root.GetComponentsInChildren<Collider>();
            for (int a = 0; a < solids.Length; a++)
                for (int b = a + 1; b < solids.Length; b++) Physics.IgnoreCollision(solids[a], solids[b]);
            Physics.SyncTransforms();
            return built;
        }

        // Where a joint is held: turned so far from its rest (degrees, about the part's own axes).
        private static void Hold(Built body, int i, float bent)
        {
            if (body.articulated)
            {
                var drive = body.link[i].xDrive; drive.target = bent; body.link[i].xDrive = drive;
            }
            else body.joint[i].targetRotation = Quaternion.Inverse(Quaternion.Euler(bent, 0, 0));
        }

        // The body keeps itself up by what its own joints give. Each step:
        //  - what the ground should push it with is reckoned: its weight, more or less to keep its height, and
        //    sideways to bring its weight back over where it began (as fast as `quick`, radians a second, without
        //    swinging past);
        //  - where on its soles that push must act for the push to pass through its weight (no further than the
        //    soles reach);
        //  - each leg's hip, knee and ankle give what makes its boot push the ground so (the turning of that push
        //    about each joint), no more than the joint has;
        //  - its hips keep its trunk upright;
        //  - and every joint is held towards its pose by the engine's own spring, as before.
        // Every torque is put on the two parts a joint joins, equal and opposite: nothing pushes the body from
        // nowhere.
        private static void Keep(Built body, Planned[] plan, float quick, float poseFullAt, float poseSlows, float scaled = 1)
        {
            float g = Physics.gravity.magnitude, whole = body.Whole();
            Vector3 weight = body.Weight(), goes = body.WeightGoes();
            body.Ground();
            var sole = new Vector3[2];
            for (int i = 0; i < 2; i++)
            {
                var s = plan[Foot + i].solids[0];
                sole[i] = body.Of(Foot + i).TransformPoint(s.at + s.turned * new Vector3(0, -.5f * s.size.y, 0));
            }
            float ground = .5f * (sole[0].y + sole[1].y), high = weight.y - ground;
            if (!body.begun)
            {
                body.begun = true; body.centre = weight; body.height = high; body.upright = body.Of(Hips).rotation;
                body.rest = Vector3.ProjectOnPlane(weight, Vector3.up); body.held = body.rest;
            }
            // (A body that is down does not push as if it stood: below six tenths of its height it gives nothing here.
            // In the game that is where it is let go into the fall.)
            if (float.IsNaN(high) || high < .6f * body.height) { for (int j = 0; j < Count; j++) body.gave[j] = 0; body.outside = 0; return; }
            float rise = Mathf.Clamp(Rises * (body.height - high) - RisesDamped * goes.y, -.5f * g, g);
            Vector3 back, push, acts;
            if (Law < 2)
            {
                // The first law: back to where its weight was, as quick as `quick`, without swinging past.
                back = quick * quick * Vector3.ProjectOnPlane(body.centre - weight, Vector3.up) - 2 * quick * Vector3.ProjectOnPlane(goes, Vector3.up);
                // Where the push must act, on the ground under its weight.
                acts = weight - Vector3.up * high - back * (high / (g + rise));
            }
            else
            {
                // The second law. A standing body falls away from where the ground pushes it, at a rate of its own
                // (the root of gravity over its height). So what matters is where its weight is *going*: its place
                // and its speed over that rate. The push must act beyond that point to bring it back.
                float falls = Mathf.Sqrt((g + rise) / Mathf.Max(.1f, high));
                // What pushes it from outside: how its weight really went in the last step, beyond what its own
                // push from the ground gave it. Felt over a tenth of a second, not at once.
                // (Where the ground's push really acted in that step is known from what touched the boots; where it
                // was only meant to act is used when nothing is known.)
                if (body.hasBefore && Feels > 0)
                {
                    Vector3 pushedAt = body.realKnown && Tracks > 0 ? body.realAt : body.actedBefore;
                    Vector3 seen = Vector3.ProjectOnPlane(goes - body.wentBefore, Vector3.up) / Step - falls * falls * Vector3.ProjectOnPlane(body.weightBefore - pushedAt, Vector3.up);
                    body.felt = Vector3.ClampMagnitude(Vector3.Lerp(body.felt, seen, Mathf.Clamp01(Step / Feels)), 4);
                }
                // The real push falls short of the meant one (by a fifth or so: the legs are not weightless rods): it
                // means its push further by how far, learnt at Tracks a second, and not while the push is at the
                // edge of its soles (where the real one cannot follow).
                // (Where it wants the push is kept within its soles; what its legs are then asked for is reckoned as if
                // the push were to act further out by the shortfall, so that the real one comes to where it is wanted.)
                if (body.hasBefore && body.realKnown && Tracks > 0)
                    body.short_ = Vector3.ClampMagnitude(body.short_ + Tracks * Step * Vector3.ProjectOnPlane(body.actedBefore - body.realAt, Vector3.up), .05f);
                Vector3 going = Vector3.ProjectOnPlane(weight, Vector3.up) + Vector3.ProjectOnPlane(goes, Vector3.up) / falls;
                Vector3 wants = going + quick / falls * (going - body.held) + body.felt / (falls * falls);
                acts = new Vector3(wants.x, weight.y - high, wants.z);
            }
            var at = new Vector3[2];
            body.outside = 0;
            // (The push is shared between the two boots by how far across it lies, from one boot's middle to the
            // other's; and each boot carries its share along its own middle line, as far forward or back as the push
            // is wanted. Only a push wanted outside both boots is taken to a boot's outer side. Asked to carry it at
            // the point of its sole nearest the wanted place, each boot bore on its inner front corner, and tipped.)
            Vector3 over = Vector3.ProjectOnPlane(sole[1] - sole[0], Vector3.up);
            Vector3 side = over.sqrMagnitude > 1e-8f ? over.normalized : Vector3.right;
            for (int i = 0; i < 2; i++)
            {
                var s = plan[Foot + i].solids[0];
                Transform foot = body.Of(Foot + i);
                float beside = Vector3.Dot(Vector3.ProjectOnPlane(acts - sole[i], Vector3.up), side);
                float outward = i == 0 ? Mathf.Min(beside, 0) : Mathf.Max(beside, 0);
                Vector3 aimed = acts - side * (beside - outward);
                Vector3 local = Quaternion.Inverse(s.turned) * (foot.InverseTransformPoint(aimed) - s.at);
                Vector3 within = new Vector3(Mathf.Clamp(local.x, -.5f * s.size.x + Edge, .5f * s.size.x - Edge), -.5f * s.size.y, Mathf.Clamp(local.z, -.5f * s.size.z + Edge, .5f * s.size.z - Edge));
                at[i] = foot.TransformPoint(s.at + s.turned * within);
            }
            Vector3 across = sole[1] - sole[0];
            float share = Mathf.Clamp01(Vector3.Dot(acts - sole[0], across) / Mathf.Max(1e-6f, across.sqrMagnitude));
            Vector3 acted = Vector3.Lerp(at[0], at[1], share);
            body.outside = Vector3.ProjectOnPlane(acts - acted, Vector3.up).magnitude;
            body.acted = acted;
            body.atItsEdge = body.outside > .002f;
            // The push can only act where the soles are: so it pushes sideways only as much as a push from there,
            // through its weight, does. (Asked for more, its legs paw at the ground and throw its trunk the other way.)
            back = Vector3.ProjectOnPlane(weight - acted, Vector3.up) * ((g + rise) / Mathf.Max(.1f, high));
            push = scaled * whole * (back + Vector3.up * (g + rise));
            if (Law >= 2)
            {
                // It leans against what it feels: the place it holds its weight goes the other way from the push, by
                // this share of what would bring its own push back to where it acts at ease, and no further than
                // LeansAtMost (so that, let go, it still has ground behind its weight). It comes to that, and back
                // from it, at Leans a second. (A first way, the held place drifting until the push acted at ease,
                // took Round's weight to the edge of its heels against 30 N, ran away when the pull ended, and it
                // fell over backwards.)
                // It leans only for what it cannot take standing as it is (its push may act Comfort from where it
                // does at ease before it leans at all), and never so far that less than KeepsBehind of sole is left
                // behind its weight: pulled with 50 N and then let go at once, Round, leaning 40 mm, went over
                // backwards with 17 mm of heel behind its weight.
                float fallsAt = Mathf.Sqrt((g + rise) / Mathf.Max(.1f, high));
                Vector3 asked = body.felt / (fallsAt * fallsAt);
                Vector3 leansTo = body.rest;
                if (asked.magnitude > Comfort)
                {
                    Vector3 way = -asked.normalized;
                    float reach = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        var s = plan[Foot + i].solids[0];
                        for (int c = 0; c < 4; c++)
                        {
                            Vector3 corner = body.Of(Foot + i).TransformPoint(s.at + s.turned * new Vector3((c & 1) == 0 ? -.5f * s.size.x : .5f * s.size.x, -.5f * s.size.y, (c & 2) == 0 ? -.5f * s.size.z : .5f * s.size.z));
                            reach = Mathf.Max(reach, Vector3.Dot(Vector3.ProjectOnPlane(corner, Vector3.up) - body.rest, way));
                        }
                    }
                    leansTo = body.rest + way * Mathf.Min(LeansBy * (asked.magnitude - Comfort), Mathf.Min(LeansAtMost, Mathf.Max(0, reach - KeepsBehind)));
                }
                body.held = Vector3.Lerp(body.held, leansTo, Mathf.Clamp01(Leans * Step));
                body.wentBefore = goes; body.weightBefore = weight; body.actedBefore = acted; body.hasBefore = true;
            }
            var torque = new Vector3[Count];
            for (int i = 0; i < 2; i++)
            {
                Vector3 pushes = (i == 0 ? 1 - share : share) * push;
                foreach (int j in new[] { Thigh + i, Shin + i, Foot + i })
                    torque[j] -= Vector3.Cross(at[i] + (Law >= 2 ? body.short_ : Vector3.zero) - body.Of(j).TransformPoint(plan[j].anchor), pushes);
            }
            // Its hips keep its trunk as upright as it began: what turns the hips back is taken from the two thighs,
            // by how much each leg bears. (Nothing else holds the trunk over the legs: the push above passes
            // through the whole body's weight, not through the trunk's.)
            {
                Quaternion off = body.upright * Quaternion.Inverse(body.Of(Hips).rotation);
                off.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180) angle -= 360;
                if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.zero; angle = 0; }
                float spring = Rights * whole;
                Vector3 rights = scaled * (spring * (angle * Mathf.Deg2Rad) * axis - RightsDamped * spring * body.Turns(Hips));
                torque[Thigh] -= (1 - share) * rights;
                torque[Thigh + 1] -= share * rights;
            }
            // (Each joint is held towards its pose by the engine's own spring: that is steady however light the part
            // beyond the joint is, which a spring worked out here, a step late, is not. It is weak just where a foot
            // is on the ground under the whole body; and there the push above does the work.)
            foreach (int j in new[] { Thigh, Thigh + 1, Shin, Shin + 1, Foot, Foot + 1 })
            {
                // (A hinge gives only about its own line; what is across it is borne by the joint itself.)
                if (plan[j].hinge) { Vector3 line = body.Of(j).right; torque[j] = line * Vector3.Dot(line, torque[j]); }
                torque[j] = Vector3.ClampMagnitude(torque[j], plan[j].strength);
                if (float.IsNaN(torque[j].x) || float.IsNaN(torque[j].y) || float.IsNaN(torque[j].z)) torque[j] = Vector3.zero;
                body.gave[j] = torque[j].magnitude;
                body.Turn(j, torque[j]);
                body.Turn(From[j], -torque[j]);
            }
        }

        private static string Called(bool articulated, float fullAt) => (articulated ? "articulation" : "joints") + string.Format(CultureInfo.InvariantCulture, ", full at {0:0} degrees, {1:0} steps a second", fullAt, 1 / Step)
            + (Rule > 0 ? string.Format(CultureInfo.InvariantCulture, ", springs by rule {0:0.00} damped {1:0.0}", Rule, Damped) : "")
            + (Law >= 2 ? string.Format(CultureInfo.InvariantCulture, ", second law (feels over {0:0.00} s, leans {1:0.0} a second)", Feels, Leans) : "")
            + (BootTurns != 1 ? string.Format(CultureInfo.InvariantCulture, ", a boot's turning weight {0:0} times its own", BootTurns) : "") + (BootWeighs > 0 ? string.Format(CultureInfo.InvariantCulture, ", a boot of {0:0.0} kg", BootWeighs) : "");

        // What the engine's numbers mean: one arm, a kilogram with its weight half a metre from a hinge, held out level
        // from a post that does not move. With a spring of S newton metres a radian it should sag 4.9 / S radians.
        [UnityTest, Explicit, Timeout(600000)]
        public IEnumerator OneJoint()
        {
            var culture = CultureInfo.InvariantCulture;
            var mode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            try
            {
                foreach (bool articulated in new[] { false, true })
                    foreach (float spring in new[] { 20f, 100f, 1000f })
                        foreach (float damper in new[] { 0f, 8f })
                        {
                            Vector3 at = new Vector3(0, 500, 0);
                            var post = new GameObject("Post");
                            post.transform.position = at;
                            var arm = new GameObject("Arm");
                            arm.transform.SetParent(articulated ? post.transform : null, true);
                            arm.transform.position = at + Vector3.forward * .5f;
                            var box = arm.AddComponent<BoxCollider>(); box.size = new Vector3(.05f, .05f, .2f);
                            ArticulationBody link = null; Rigidbody solid = null;
                            if (articulated)
                            {
                                var root = post.AddComponent<ArticulationBody>(); root.immovable = true; root.solverIterations = 14; root.solverVelocityIterations = 4;
                                link = arm.AddComponent<ArticulationBody>();
                                link.mass = 1; link.linearDamping = 0; link.angularDamping = 0; link.jointFriction = 0;
                                link.jointType = ArticulationJointType.RevoluteJoint;
                                link.anchorPosition = new Vector3(0, 0, -.5f); link.anchorRotation = Quaternion.identity; link.matchAnchors = true;
                                link.twistLock = ArticulationDofLock.FreeMotion;
                                link.xDrive = new ArticulationDrive { stiffness = spring, damping = damper, forceLimit = 100000, target = 0, driveType = ArticulationDriveType.Force };
                            }
                            else
                            {
                                var held = post.AddComponent<Rigidbody>(); held.isKinematic = true;
                                solid = arm.AddComponent<Rigidbody>();
                                solid.mass = 1; solid.linearDamping = 0; solid.angularDamping = 0; solid.solverIterations = 14; solid.solverVelocityIterations = 4;
                                var joint = arm.AddComponent<ConfigurableJoint>();
                                joint.axis = Vector3.right; joint.secondaryAxis = Vector3.up;
                                joint.anchor = new Vector3(0, 0, -.5f); joint.autoConfigureConnectedAnchor = true; joint.connectedBody = held;
                                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
                                joint.angularXMotion = ConfigurableJointMotion.Free; joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Locked;
                                joint.rotationDriveMode = RotationDriveMode.Slerp;
                                joint.slerpDrive = new JointDrive { positionSpring = spring, positionDamper = damper, maximumForce = 100000 };
                                joint.angularXDrive = new JointDrive { positionSpring = spring, positionDamper = damper, maximumForce = 100000 };
                            }
                            Physics.SyncTransforms();
                            float after = 0, most = 0;
                            for (int k = 0; k < 500; k++)
                            {
                                Physics.Simulate(.02f);
                                float sag = Vector3.Angle(Vector3.forward, arm.transform.forward);
                                most = Mathf.Max(most, sag);
                                if (k == 9) after = sag;
                            }
                            float ends = Vector3.Angle(Vector3.forward, arm.transform.forward);
                            float gives = articulated ? link.driveForce[0] : arm.GetComponent<ConfigurableJoint>().currentTorque.magnitude;
                            Debug.Log(string.Format(culture, "OWN one joint, {0}, spring {1:0}, damper {2:0}: it should sag {3:0.00} degrees; it sags {4:0.00} degrees after ten seconds ({5:0.00} after 0.2 s, {6:0.00} at most); the engine says the joint gives {7:0.00}",
                                articulated ? "articulation" : "joints", spring, damper, 4.905f / spring * Mathf.Rad2Deg, ends, after, most, gives));
                            UnityEngine.Object.Destroy(post); if (!articulated) UnityEngine.Object.Destroy(arm);
                            yield return null;
                        }
            }
            finally { Physics.simulationMode = mode; }
        }

        // The plainest standing body: one heavy part (86 kg, its weight 0.75 m over an ankle) on one boot that lies on
        // a level floor, the ankle sprung stiffly enough to hold it by reckoning (its spring five times what its
        // weight's leaning asks). It begins leaning a degree. Does it stand? With a light boot and a heavy one; at
        // fifty steps a second and five hundred; hung from the heavy part, or the heavy part hung from the boot.
        [UnityTest, Explicit, Timeout(600000)]
        public IEnumerator OneAnkle()
        {
            var culture = CultureInfo.InvariantCulture;
            var mode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0, 799.5f, 0); floor.transform.localScale = new Vector3(20, 1, 20);
            try
            {
                foreach (float step in new[] { .02f, .01f, .005f, .004f, .002f })
                    foreach (float bootWeighs in new[] { 1.55f, 20f })
                        foreach (bool fromBoot in new[] { false, true })
                            foreach (float slows in new[] { 0f, .1f })
                            {
                                var boot = new GameObject("Boot");
                                var above = new GameObject("Above");
                                boot.transform.position = new Vector3(0, 800.04f + .001f, .03f);
                                var sole = boot.AddComponent<BoxCollider>(); sole.size = new Vector3(.1f, .08f, .18f);
                                Vector3 ankle = new Vector3(0, 800.08f + .001f, 0);
                                Quaternion leans = Quaternion.AngleAxis(1, Vector3.right);
                                above.transform.SetPositionAndRotation(ankle + leans * Vector3.up * .75f, leans);
                                var trunk = above.AddComponent<CapsuleCollider>(); trunk.radius = .12f; trunk.height = 1;
                                Physics.IgnoreCollision(sole, trunk);
                                if (fromBoot) above.transform.SetParent(boot.transform, true); else boot.transform.SetParent(above.transform, true);
                                var first = (fromBoot ? boot : above).AddComponent<ArticulationBody>();
                                var second = (fromBoot ? above : boot).AddComponent<ArticulationBody>();
                                first.immovable = false; first.solverIterations = 14; first.solverVelocityIterations = 4;
                                foreach (var link in new[] { first, second }) { link.linearDamping = 0; link.angularDamping = 0; link.jointFriction = 0; }
                                (fromBoot ? first : second).mass = bootWeighs; (fromBoot ? second : first).mass = 86;
                                second.jointType = ArticulationJointType.RevoluteJoint;
                                second.anchorPosition = second.transform.InverseTransformPoint(ankle); second.anchorRotation = Quaternion.Inverse(second.transform.rotation);
                                second.matchAnchors = true;
                                second.twistLock = ArticulationDofLock.FreeMotion;
                                float spring = 5 * 86 * 9.81f * .75f;
                                // (The joint rests as it is made, leaning a degree: so it is held towards upright, a degree away.)
                                second.xDrive = new ArticulationDrive { stiffness = spring, damping = spring * slows, forceLimit = 100000, target = fromBoot ? -1 : 1, driveType = ArticulationDriveType.Force };
                                Physics.SyncTransforms();
                                float least = 90, most = 0, fellAt = -1;
                                int steps = Mathf.RoundToInt(10 / step);
                                for (int k = 0; k < steps; k++)
                                {
                                    Physics.Simulate(step);
                                    float lean = Vector3.Angle(Vector3.up, above.transform.up);
                                    if (k * step > 1) { least = Mathf.Min(least, lean); most = Mathf.Max(most, lean); }
                                    if (lean > 30) { fellAt = k * step; break; }
                                }
                                Debug.Log(string.Format(culture, "OWN one ankle, {0:0} steps a second, a boot of {1:0.00} kg, {2}, damping {3:0.0}: {4}; after the first second it leaned between {5:0.00} and {6:0.00} degrees; its boot is tipped {7:0.00} degrees; the engine says the ankle gives {8:0.0} N m (its weight asks {9:0.0})",
                                    1 / step, bootWeighs, fromBoot ? "the heavy part hung from the boot" : "the boot hung from the heavy part", slows, fellAt < 0 ? "it stood ten seconds" : string.Format(culture, "IT FELL after {0:0.0} s", fellAt),
                                    least, most, Vector3.Angle(Vector3.up, boot.transform.up), second.driveForce[0],
                                    86 * 9.81f * .75f * Mathf.Sin(Vector3.Angle(Vector3.up, above.transform.up) * Mathf.Deg2Rad)));
                                UnityEngine.Object.Destroy(fromBoot ? boot : above);
                                yield return null;
                            }
            }
            finally { Physics.simulationMode = mode; UnityEngine.Object.Destroy(floor); }
        }

        // ---- The search for the keeper's settings.
        // What is searched, each with the least and the most it may be.
        private static readonly string[] Named = { "quick", "feels", "leans", "leansBy", "comfort", "keepsBehind", "tracks", "rule", "damped", "rights", "rightsDamped", "edge", "rises", "risesDamped", "fullAt" };
        private static readonly float[] Least = { 1, .02f, .5f, .2f, 0, .01f, 0, .4f, .2f, 3, .05f, .003f, 30, 5, 30 };
        private static readonly float[] Most = { 9, .3f, 10, 1.6f, .07f, .06f, 25, 1.8f, 2.5f, 25, .6f, .03f, 300, 60, 120 };
        private static void Set(float[] v)
        {
            Quick = v[0]; Feels = v[1]; Leans = v[2]; LeansBy = v[3]; Comfort = v[4]; KeepsBehind = v[5]; Tracks = v[6]; Rule = v[7]; Damped = v[8];
            Rights = v[9]; RightsDamped = v[10]; Edge = v[11]; Rises = v[12]; RisesDamped = v[13];
        }
        private static string Said(float[] v)
        {
            var text = new System.Text.StringBuilder();
            for (int k = 0; k < v.Length; k++) text.Append(string.Format(CultureInfo.InvariantCulture, "{0}{1} {2:0.####}", k > 0 ? ", " : "", Named[k], v[k]));
            return text.ToString();
        }

        // One of a crowd: how it is disturbed (set going at a speed, or pulled with a share of its weight for two and
        // a half seconds and let go; a way round the compass), and where its head was when that began.
        private sealed class Tried { public Built body; public float jolt, pull; public Vector3 way; public Vector3 headWas, madeAt; public float wander, settle; public Vector3 middle; public int seen; }

        // -ownSearch: the settings are searched. -searchRounds, -searchMany (tries a round), -searchKeep (the best kept),
        // -searchFrom "a,b,c,..." (where it begins: fifteen numbers), -searchOut <file>. At -ownStep.
        [UnityTest, Explicit, Timeout(14400000)]
        public IEnumerator Search()
        {
            string folder = CaptureTools.Argument("-ownOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Own");
            Step = Numbers("-ownStep", .02f)[0];
            Law = 2;
            int rounds = (int)Numbers("-searchRounds", 10)[0], many = (int)Numbers("-searchMany", 32)[0], keep = (int)Numbers("-searchKeep", 8)[0];
            float ankleGives = Numbers("-ownAnkle", .7f)[0];
            float[] jolts = Numbers("-searchJolts", .12f, .2f), pullShares = Numbers("-searchPulls", .05f, .09f);
            float lasts = Numbers("-searchFor", 8)[0];
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            Time.captureFramerate = 50;
            var mode = Physics.simulationMode;
            var plans = new List<Planned[]>();
            var names = new List<string>();
            var talls = new List<float>();
            var lowests = new List<float>();
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                Vector3 OnGround(Vector3 q) => new Vector3(q.x, ground.Height(q.x, q.z), q.z);
                var a = ground.Path[4];
                var b = ground.Path[5];
                var spot = OnGround(new Vector3(a.x, 0, a.y));
                var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
                away.y = 0;
                away.Normalize();
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.position = spot + Vector3.up * 200;
                floor.transform.localScale = new Vector3(200, 1, 200);
                float floorTop = floor.transform.position.y + .5f;
                for (int index = 0; index < choice.Count; index++)
                {
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    var miner = unit.GetComponent<MinerBody>();
                    var biped = unit.GetComponent<ProceduralBiped>();
                    var physical = unit.GetComponent<PhysicalBody>();
                    var fall = unit.GetComponent<PhysicalFall>();
                    if (fall == null) fall = unit.gameObject.AddComponent<PhysicalFall>();
                    fall.GetsUp = false;
                    unit.Motor.Stop();
                    unit.GetComponent<NavMeshAgent>().Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    biped.ResetPose();
                    for (float until = Time.time + .8f; Time.time < until;) yield return null;
                    talls.Add(miner.Rig.head.position.y - unit.transform.position.y);
                    fall.LetGo();
                    var plan = Plan(fall, physical, ankleGives);
                    fall.TakeBack();
                    yield return null;
                    float lowest = float.MaxValue;
                    foreach (var solid in plan[Foot].solids) lowest = Mathf.Min(lowest, (plan[Foot].at + plan[Foot].turned * solid.at).y - .5f * solid.size.y);
                    plans.Add(plan); names.Add(choice.NameOf(index)); lowests.Add(lowest);
                }
                foreach (var unit in UnityEngine.Object.FindObjectsByType<SelectableUnit>(FindObjectsSortMode.None)) unit.gameObject.SetActive(false);
                yield return null;
                Physics.simulationMode = SimulationMode.Script;

                // What one setting comes to: for each miner, so many bodies on the floor, each disturbed its own way.
                float[] scores = new float[3];
                string told = "";
                IEnumerator Try(float[] v)
                {
                    Set(v);
                    float fullAt = v[14];
                    told = "";
                    for (int m = 0; m < plans.Count; m++)
                    {
                        var plan = plans[m];
                        float whole = 0;
                        foreach (var part in plan) whole += part.mass;
                        var crowd = new List<Tried>();
                        void Add(float jolt, float pull, Vector3 way)
                        {
                            int n = crowd.Count;
                            Vector3 place = new Vector3((n % 8 - 4) * 2.5f, floorTop - lowests[m] + .002f, (n / 8 - 2) * 2.5f + m * 30);
                            crowd.Add(new Tried { body = Build(plan, true, place, fullAt, .05f, 0, 0, false, true), jolt = jolt, pull = pull, way = way });
                            crowd[crowd.Count - 1].madeAt = crowd[crowd.Count - 1].body.Of(Head).position;
                        }
                        Add(0, 0, away);
                        foreach (float jolt in jolts) for (int d = 0; d < 8; d++) Add(jolt, 0, Quaternion.AngleAxis(d * 45, Vector3.up) * away);
                        foreach (float share in pullShares) for (int d = 0; d < 4; d++) Add(0, share * whole * 9.81f, Quaternion.AngleAxis(d * 90, Vector3.up) * away);
                        int steps = Mathf.RoundToInt(lasts / Step), begins = Mathf.RoundToInt(1 / Step), ends = Mathf.RoundToInt(3.5f / Step);
                        for (int k = 0; k < steps; k++)
                        {
                            foreach (var one in crowd)
                            {
                                if (k == begins)
                                {
                                    one.headWas = one.body.Of(Head).position;
                                    if (one.jolt > 0) for (int i = 0; i < Count; i++) one.body.Push(i, one.body.Mass(i) * one.jolt / Step * one.way, one.body.Centre(i));
                                }
                                if (one.pull > 0 && k >= begins && k < ends) one.body.Push(Trunk, one.way * one.pull, one.body.Centre(Trunk));
                                Keep(one.body, plan, Quick, fullAt, .05f);
                            }
                            Physics.Simulate(Step);
                            // (The undisturbed one: how far its head goes in its first second, and how it wanders after.)
                            var still = crowd[0];
                            Vector3 head = still.body.Of(Head).position;
                            if (k == 0) still.middle = head;
                            if (k < begins) still.settle = Mathf.Max(still.settle, Vector3.ProjectOnPlane(head - still.middle, Vector3.up).magnitude);
                            if (k == begins * 3) { still.middle = head; }
                            if (k > begins * 3) still.wander = Mathf.Max(still.wander, (head - still.middle).magnitude);
                        }
                        float score = 0;
                        int stood = 0, jolted = 0, pulled = 0;
                        foreach (var one in crowd)
                        {
                            Vector3 head = one.body.Of(Head).position;
                            float off = Vector3.ProjectOnPlane(head - one.headWas, Vector3.up).magnitude;
                            // (And it must not have gone down before it was disturbed at all: a body that fell in its first
                            // second "stood as it stood" by where its head was after that second, and the search found it.)
                            bool asItStood = head.y - floorTop > .95f * (one.headWas.y - floorTop) && off < .1f * talls[m] && head.y - floorTop > .9f * (one.madeAt.y - floorTop);
                            if (asItStood) { stood++; score += 1 - Mathf.Clamp01(off / (.1f * talls[m])) * .3f; if (one.jolt > 0) jolted++; else if (one.pull > 0) pulled++; }
                        }
                        // (It must be still when nothing disturbs it: more than a third of a millimetre of wandering, or
                        // more than a centimetre and a half of settling, costs as much as bodies falling.)
                        var quiet = crowd[0];
                        float fidgets = Mathf.Clamp(quiet.wander * 1000 / .35f - 1, 0, 6) + Mathf.Clamp(quiet.settle * 1000 / 15 - 1, 0, 4);
                        score -= fidgets;
                        scores[m] = score;
                        told += string.Format(culture, " {0}: {1} of {2} stand ({3} of {4} set going, {5} of {6} pulled), still within {7:0.00} mm after settling {8:0.0} mm;", names[m], stood, crowd.Count, jolted, jolts.Length * 8, pulled, pullShares.Length * 4, quiet.wander * 1000, quiet.settle * 1000);
                        foreach (var one in crowd) UnityEngine.Object.Destroy(one.body.root);
                        yield return null;
                    }
                }
                float Whole() => scores[0] + scores[1] + scores[2] + 2 * Mathf.Min(scores[0], Mathf.Min(scores[1], scores[2]));

                int count = Named.Length;
                var middle = new float[count];
                var spread = new float[count];
                float[] from = Numbers("-searchFrom");
                float[] first = { Quick, Feels, Leans, LeansBy, Comfort, KeepsBehind, Tracks, 1, 1, Rights, RightsDamped, Edge, Rises, RisesDamped, 75 };
                for (int k = 0; k < count; k++) { middle[k] = from.Length == count ? from[k] : first[k]; spread[k] = (Most[k] - Least[k]) * Numbers("-searchSpread", .2f)[0]; }
                yield return Try(middle);
                float best = Whole();
                float[] bestOf = (float[])middle.Clone();
                Debug.Log(string.Format(culture, "OWNSEARCH begins at {0:0.00}:{1} [{2}]", best, told, Said(middle)));
                var random = new System.Random((int)Numbers("-searchSeed", 7)[0]);
                float Bell() { double u = 1 - random.NextDouble(), w = 1 - random.NextDouble(); return (float)(Math.Sqrt(-2 * Math.Log(u)) * Math.Sin(2 * Math.PI * w)); }
                for (int round = 0; round < rounds; round++)
                {
                    var tried = new List<(float score, float[] v, string told)>();
                    for (int n = 0; n < many; n++)
                    {
                        var v = new float[count];
                        for (int k = 0; k < count; k++) v[k] = Mathf.Clamp(middle[k] + spread[k] * Bell(), Least[k], Most[k]);
                        yield return Try(v);
                        tried.Add((Whole(), v, told));
                    }
                    tried.Sort((x, y) => y.score.CompareTo(x.score));
                    if (tried[0].score > best) { best = tried[0].score; bestOf = (float[])tried[0].v.Clone(); }
                    for (int k = 0; k < count; k++)
                    {
                        float mean = 0, wide = 0;
                        for (int n = 0; n < keep; n++) mean += tried[n].v[k] / keep;
                        for (int n = 0; n < keep; n++) wide += (tried[n].v[k] - mean) * (tried[n].v[k] - mean) / keep;
                        middle[k] = mean; spread[k] = Mathf.Max(Mathf.Sqrt(wide), (Most[k] - Least[k]) * .01f);
                    }
                    Debug.Log(string.Format(culture, "OWNSEARCH round {0}: the best of it {1:0.00}:{2} [{3}]; the best so far {4:0.00}", round + 1, tried[0].score, tried[0].told, Said(tried[0].v), best));
                }
                yield return Try(bestOf);
                Debug.Log(string.Format(culture, "OWNSEARCH found {0:0.00} at {1:0} steps a second:{2} [{3}]", Whole(), 1 / Step, told, Said(bestOf)));
                string file = CaptureTools.Argument("-searchOut");
                if (!string.IsNullOrEmpty(file))
                {
                    var numbers = new string[count];
                    for (int k = 0; k < count; k++) numbers[k] = bestOf[k].ToString("0.#####", culture);
                    File.WriteAllText(file, string.Join(",", numbers));
                }
                UnityEngine.Object.Destroy(floor);
            }
            finally
            {
                Physics.simulationMode = mode;
                Time.captureFramerate = 0;
            }
        }

        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator Bench()
        {
            string folder = CaptureTools.Argument("-ownOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Own");
            string who = CaptureTools.Argument("-ownMiner");
            float[] fulls = Numbers("-ownFullAt", 35, 5), pulls = Numbers("-ownPulls", 50, 100, 200), crowds = Numbers("-ownCrowd", 1, 25, 100), view = Numbers("-ownView", 100, 8, 3.2f);
            float slows = Numbers("-ownSlows", .08f)[0], lasts = Numbers("-ownFor", 30)[0], ankleGives = Numbers("-ownAnkle", .7f)[0], stronger = Numbers("-ownStrong", 1)[0];
            int size = (int)Numbers("-ownSize", 400)[0];
            float quick = Numbers("-ownQuick", 5)[0], crowdFor = Mathf.Max(6, Numbers("-ownCrowdFor", 6)[0]);
            // -ownJolt: each of a crowd is set going at this speed (m/s) as it begins, each a different way round the
            // compass (so that they are so many different starts, and not one start so many times).
            float jolt = Numbers("-ownJolt", 0)[0];
            Iterations = (int)Numbers("-ownIterations", 14, 4)[0]; VelocityIterations = (int)Numbers("-ownIterations", 14, 4)[1];
            Step = Numbers("-ownStep", .02f)[0];
            BootTurns = Numbers("-ownBootTurns", 1)[0]; BootWeighs = Numbers("-ownBootWeighs", 0)[0];
            Rule = Numbers("-ownRule", 0)[0]; Damped = Numbers("-ownDamped", 1)[0];
            Law = (int)Numbers("-ownLaw", 2)[0]; Feels = Numbers("-ownFeels", .1f)[0]; Leans = Numbers("-ownLeans", 3)[0]; LeansAtMost = Numbers("-ownLeansAtMost", .1f)[0]; LeansBy = Numbers("-ownLeansBy", 1)[0]; Comfort = Numbers("-ownComfort", .03f)[0]; KeepsBehind = Numbers("-ownKeepsBehind", .035f)[0]; Tracks = Numbers("-ownTracks", 8)[0];
            // -ownSet "a,b,...": the fifteen settings a search found, to measure them here.
            float[] found = Numbers("-ownSet");
            if (found.Length == Named.Length) { Set(found); quick = Quick; fulls = new[] { found[14] }; }
            bool said = Numbers("-ownSay", 0)[0] > 0;
            string kinds = CaptureTools.Argument("-ownKinds") ?? "joints,articulation";
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            Time.captureFramerate = 50;
            var mode = Physics.simulationMode;
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                var camera = Camera.main;
                var rig = camera.GetComponent<RtsCamera>();
                if (rig != null) rig.enabled = false;
                camera.nearClipPlane = .01f;
                time.Hour = 13;
                Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
                var a = ground.Path[4];
                var b = ground.Path[5];
                var spot = OnGround(new Vector3(a.x, 0, a.y));
                var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
                away.y = 0;
                away.Normalize();
                // A floor high over the place, level, for the crowd and for nothing else.
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.position = spot + Vector3.up * 200;
                floor.transform.localScale = new Vector3(80, 1, 80);
                float floorTop = floor.transform.position.y + .5f;

                for (int index = 0; index < choice.Count; index++)
                {
                    string name = choice.NameOf(index);
                    if (!string.IsNullOrEmpty(who) && !string.Equals(who, name, StringComparison.OrdinalIgnoreCase)) continue;
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    var miner = unit.GetComponent<MinerBody>();
                    var biped = unit.GetComponent<ProceduralBiped>();
                    var physical = unit.GetComponent<PhysicalBody>();
                    var fall = unit.GetComponent<PhysicalFall>();
                    if (fall == null) fall = unit.gameObject.AddComponent<PhysicalFall>();
                    fall.GetsUp = false;
                    unit.Motor.Stop();
                    unit.GetComponent<NavMeshAgent>().Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    biped.ResetPose();
                    for (float until = Time.time + .8f; Time.time < until;) yield return null;
                    float tall = miner.Rig.head.position.y - unit.transform.position.y;
                    // The plan, from the let-go body as the game makes it; then the game's own body is put away.
                    fall.LetGo();
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Falling), name + " was not let go.");
                    var plan = Plan(fall, physical, ankleGives);
                    foreach (var planned in plan) planned.strength *= stronger;
                    if (BootWeighs > 0) { plan[Foot].mass = BootWeighs; plan[Foot + 1].mass = BootWeighs; }
                    fall.TakeBack();
                    yield return null;
                    unit.gameObject.SetActive(false);
                    yield return null;
                    float lowest = float.MaxValue;
                    foreach (var s in plan[Foot].solids) lowest = Mathf.Min(lowest, (plan[Foot].at + plan[Foot].turned * s.at).y - .5f * s.size.y);
                    float whole = 0;
                    foreach (var p in plan) whole += p.mass;
                    Debug.Log(string.Format(culture, "OWN   {0}: {1:0.0} kg in thirteen parts (a boot {2:0.00} kg, a shin {3:0.00} kg); an ankle gives {4:0} N m, a knee {5:0}, a hip {6:0}, its back {7:0}, its neck {8:0}, a shoulder {9:0}, an elbow {10:0}; its head stands {11:0.00} m up; its soles are {12:0} mm from the ground as it is planned",
                        name, whole, plan[Foot].mass, plan[Shin].mass, plan[Foot].strength, plan[Shin].strength, plan[Thigh].strength, plan[Trunk].strength, plan[Head].strength, plan[UpperArm].strength, plan[Forearm].strength, tall,
                        (lowest - ground.Height(plan[Foot].at.x, plan[Foot].at.z)) * 1000));

                    if (Rule > 0)
                    {
                        var springs = new System.Text.StringBuilder();
                        foreach (int j in new[] { Trunk, Head, UpperArm, Forearm, Thigh, Shin, Foot })
                        {
                            Sprung(plan, j, fulls[0], slows, out float spring, out float damper);
                            springs.Append(string.Format(culture, " {0} {1:0} and {2:0.0} (lighter side {3:0.000} kg m2; by its strength alone {4:0})", plan[j].name.Replace(" (left)", ""), spring, damper, Lighter(plan, j), plan[j].strength / (fulls[0] * Mathf.Deg2Rad)));
                        }
                        Debug.Log(string.Format(culture, "OWN   {0}: its springs by rule (N m a radian) and dampers, at {1:0} steps a second:{2}", name, 1 / Step, springs));
                    }
                    Physics.simulationMode = SimulationMode.Script;
                    foreach (string kind in kinds.Split(','))
                    {
                        bool articulated = kind.Contains("articulation"), keeps = kind.Contains("torques");
                        foreach (float fullAt in fulls)
                        {
                            string called = Called(articulated, fullAt) + (keeps ? string.Format(culture, ", kept by its own torques (as quick as {0:0.0} a second)", quick) : "");
                            string tag = $"{name.ToLowerInvariant()}_{(articulated ? "a" : "j")}{(keeps ? "t" : "")}{fullAt:0}";
                            int shot = 0;
                            void Picture(Built shown)
                            {
                                Vector3 target = new Vector3(spot.x, spot.y + tall * .5f, spot.z);
                                Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * away;
                                dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                                camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                                CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{shot:000}"), size, size);
                                shot++;
                            }

                            // ---- 1. It stands.
                            var body = Build(plan, articulated, Vector3.up * .002f, fullAt, slows, 0, 0, true, keeps);
                            Vector3 headBegan = body.Of(Head).position, hipsBegan = body.Of(Hips).position;
                            int steps = Mathf.RoundToInt(lasts / Step), settle = Mathf.RoundToInt(5 / Step);
                            float furthest = 0, fastest = 0, speeds = 0, most = 0;
                            int counted = 0, crossings = 0;
                            float before = 0;
                            Vector3 headWas = headBegan;
                            bool fell = false;
                            float fellAt = -1;
                            Vector3 mean = Vector3.zero;
                            var seen = new List<Vector3>();
                            Picture(body);
                            {
                                // Where its weight is over its boots, as it begins (millimetres ahead of where it stands).
                                float back = float.MaxValue, front = float.MinValue;
                                for (int i = 0; i < 2; i++)
                                    foreach (var s in plan[Foot + i].solids)
                                        for (int c = 0; c < 8; c++)
                                        {
                                            Vector3 corner = plan[Foot + i].at + plan[Foot + i].turned * (s.at + s.turned * Vector3.Scale(s.size * .5f, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1)));
                                            float ahead = Vector3.Dot(corner - spot, away);
                                            back = Mathf.Min(back, ahead); front = Mathf.Max(front, ahead);
                                        }
                                Debug.Log(string.Format(culture, "OWN   {0}, {1}: its weight is {2:0} mm ahead of where it stands and {3:0} mm up; its boots reach from {4:0} to {5:0} mm; its ankles are at {6:0} mm",
                                    name, called, Vector3.Dot(body.Weight() - spot, away) * 1000, (body.Weight().y - spot.y) * 1000, back * 1000, front * 1000,
                                    Vector3.Dot(plan[Foot].at + plan[Foot].turned * plan[Foot].anchor - spot, away) * 1000));
                            }
                            float outside = 0;
                            for (int k = 0; k < steps; k++)
                            {
                                if (keeps) { Keep(body, plan, quick, fullAt, slows); outside = Mathf.Max(outside, body.outside); }
                                Physics.Simulate(Step);
                                Vector3 head = body.Of(Head).position;
                                if (said && k % Mathf.RoundToInt(.2f / Step) == 0 && k * Step < 2.01f)
                                    Debug.Log(string.Format(culture, "OWN   {0}, {1}, {2:0.0} s: its weight {3:0} mm ahead; its head {4:0} mm ahead and {5:0} mm to its right; a boot tipped {6:0.0} degrees; its ankle {7:0.0} degrees from where it is held, its knee {8:0.0}, its hip {9:0.0}, its waist {10:0.0}",
                                        name, called, (k + 1) * Step, Vector3.Dot(body.Weight() - spot, away) * 1000, Vector3.Dot(head - headBegan, away) * 1000, Vector3.Dot(head - headBegan, Vector3.Cross(Vector3.up, away)) * 1000,
                                        Quaternion.Angle(plan[Foot].turned, body.Of(Foot).rotation), body.Off(Foot), body.Off(Shin), body.Off(Thigh), body.Off(Trunk))
                                        + string.Format(culture, "; its joints give: ankle {0:0}, knee {1:0}, hip {2:0}, waist {3:0} N m; its hip joint is {4:0} mm ahead, its knee {5:0}, its ankle {6:0}; its hips {7:0} mm up, its knee {8:0}",
                                            body.Gives(Foot), body.Gives(Shin), body.Gives(Thigh), body.Gives(Trunk),
                                            Vector3.Dot(body.Of(Thigh).TransformPoint(plan[Thigh].anchor) - spot, away) * 1000, Vector3.Dot(body.Of(Shin).TransformPoint(plan[Shin].anchor) - spot, away) * 1000,
                                            Vector3.Dot(body.Of(Foot).TransformPoint(plan[Foot].anchor) - spot, away) * 1000,
                                            (body.Of(Thigh).TransformPoint(plan[Thigh].anchor).y - spot.y) * 1000, (body.Of(Shin).TransformPoint(plan[Shin].anchor).y - spot.y) * 1000));
                                if (said && k % Mathf.RoundToInt(.2f / Step) == 0 && k * Step < 2.01f)
                                {
                                    // Every joint: how far from where it is held, about the part's own right, up and forward
                                    // (degrees), and what it gives.
                                    var all = new System.Text.StringBuilder();
                                    for (int i = 1; i < Count; i++)
                                    {
                                        Quaternion turned = Quaternion.Inverse(body.rested[i]) * (Quaternion.Inverse(body.Of(From[i]).rotation) * body.Of(i).rotation);
                                        turned.ToAngleAxis(out float angle, out Vector3 axis);
                                        if (angle > 180) angle -= 360;
                                        Vector3 about = axis * angle;
                                        all.Append(string.Format(culture, " {0} ({1:0.0} {2:0.0} {3:0.0}; {4:0})", plan[i].name.Replace(" (left)", "L").Replace(" (right)", "R").Replace(" ", ""), about.x, about.y, about.z, body.Gives(i)));
                                    }
                                    Debug.Log(string.Format(culture, "OWN   {0}, {1}, {2:0.0} s joints:{3}; boots: left tipped {4:0.0}, right {5:0.0}; left ankle {6:0} mm ahead {7:0} up, right {8:0} ahead {9:0} up", name, called, (k + 1) * Step, all,
                                        Quaternion.Angle(plan[Foot].turned, body.Of(Foot).rotation), Quaternion.Angle(plan[Foot + 1].turned, body.Of(Foot + 1).rotation),
                                        Vector3.Dot(body.Of(Foot).TransformPoint(plan[Foot].anchor) - spot, away) * 1000, (body.Of(Foot).TransformPoint(plan[Foot].anchor).y - spot.y) * 1000,
                                        Vector3.Dot(body.Of(Foot + 1).TransformPoint(plan[Foot + 1].anchor) - spot, away) * 1000, (body.Of(Foot + 1).TransformPoint(plan[Foot + 1].anchor).y - spot.y) * 1000));
                                }
                                if (!fell && head.y - spot.y < .7f * (headBegan.y - spot.y)) { fell = true; fellAt = k * Step; }
                                if (fell) break;
                                if (k >= settle)
                                {
                                    seen.Add(head); mean += head; counted++;
                                    // (From where it is, step to step: the engine's own reading of a resting part's speed
                                    // did not agree with how far the part went.)
                                    Vector3 goes = (head - headWas) / Step;
                                    speeds += goes.magnitude; fastest = Mathf.Max(fastest, goes.magnitude);
                                    float ahead = Vector3.Dot(goes, away);
                                    if (counted > 1 && Mathf.Sign(ahead) != Mathf.Sign(before)) crossings++;
                                    before = ahead;
                                }
                                headWas = head;
                                furthest = Mathf.Max(furthest, Vector3.ProjectOnPlane(head - headBegan, Vector3.up).magnitude);
                                if (k == settle || k == steps / 2) Picture(body);
                            }
                            for (int i = 0; i < Count; i++) most = Mathf.Max(most, body.Off(i));
                            float wander = 0;
                            if (counted > 0) { mean /= counted; foreach (var h in seen) wander = Mathf.Max(wander, Vector3.ProjectOnPlane(h - mean, Vector3.up).magnitude); }
                            Picture(body);
                            Vector3 headNow = body.Of(Head).position, hipsNow = body.Of(Hips).position;
                            Debug.Log(string.Format(culture, "OWN {0} stands, {1}: {2}; its head went {3:0.0} mm from where it began at most and is {4:0.0} mm from there at the end (its hips {5:0.0} mm, and {6:0.0} mm lower); after five seconds its head wandered {7:0.00} mm about its middle, went {8:0.00} mm/s on average and {9:0.0} mm/s at most, and turned back {10:0.0} times a second; its joints are {11:0.00} degrees from where they are held at most",
                                name, called, fell ? string.Format(culture, "IT FELL after {0:0.0} s", fellAt) : string.Format(culture, "it stood {0:0} s", lasts),
                                furthest * 1000, Vector3.ProjectOnPlane(headNow - headBegan, Vector3.up).magnitude * 1000, Vector3.ProjectOnPlane(hipsNow - hipsBegan, Vector3.up).magnitude * 1000, (hipsBegan.y - hipsNow.y) * 1000,
                                wander * 1000, counted > 0 ? speeds / counted * 1000 : 0, fastest * 1000, counted > 1 ? crossings / (counted * Step) : 0, most));
                            if (keeps) Debug.Log(string.Format(culture, "OWN   {0}, {1}: standing, its joints give at the end: an ankle {2:0} and {3:0}, a knee {4:0} and {5:0}, a hip {6:0} and {7:0}, its waist {8:0} N m; the push it wanted was never more than {9:0} mm outside its soles",
                                name, called, body.gave[Foot], body.gave[Foot + 1], body.gave[Shin], body.gave[Shin + 1], body.gave[Thigh], body.gave[Thigh + 1], body.gave[Trunk], outside * 1000));
                            UnityEngine.Object.Destroy(body.root);
                            yield return null;

                            // (How far its boots reach, ahead of where it stands and behind.)
                            float toes = float.MinValue, heels = float.MaxValue;
                            for (int i = 0; i < 2; i++)
                                foreach (var s in plan[Foot + i].solids)
                                    for (int c = 0; c < 8; c++)
                                    {
                                        Vector3 corner = plan[Foot + i].at + plan[Foot + i].turned * (s.at + s.turned * Vector3.Scale(s.size * .5f, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1)));
                                        toes = Mathf.Max(toes, Vector3.Dot(corner - spot, away)); heels = Mathf.Min(heels, Vector3.Dot(corner - spot, away));
                                    }
                            // ---- 2. It is pulled at the chest, forwards, for two and a half seconds, and left three.
                            foreach (float pull in fell ? new float[0] : pulls)
                            {
                                body = Build(plan, articulated, Vector3.up * .002f, fullAt, slows, 0, 0, true, keeps);
                                for (int k = 0; k < Mathf.RoundToInt(2 / Step); k++) { if (keeps) Keep(body, plan, quick, fullAt, slows); Physics.Simulate(Step); }
                                Vector3 headBefore = body.Of(Head).position;
                                float leant = 0, settled = -1;
                                bool down = false;
                                int pulled = Mathf.RoundToInt(2.5f / Step), watched = Mathf.RoundToInt(5.5f / Step);
                                for (int k = 0; k < watched; k++)
                                {
                                    if (k < pulled) body.Push(Trunk, away * pull, body.Centre(Trunk));
                                    if (keeps) Keep(body, plan, quick, fullAt, slows);
                                    Physics.Simulate(Step);
                                    if (said && keeps && k % Mathf.RoundToInt(.25f / Step) == 0)
                                        Debug.Log(string.Format(culture, "OWN   {0} pulled {1:0} N, {2:0.00} s ground: it bore {3:0} N a step ago, {4:0} mm ahead of where the body stands (the keeper meant {5:0}; it has learnt the real one falls {6:0} mm short); its weight is {7:0} mm ahead of where it stands; its toes reach {8:0} and its heels {9:0} mm",
                                            name, pull, k * Step, body.realUp, Vector3.Dot(body.realAt - spot, away) * 1000, Vector3.Dot(body.actedBefore - spot, away) * 1000, Vector3.Dot(body.short_, away) * 1000,
                                            Vector3.Dot(body.Weight() - spot, away) * 1000, toes * 1000, heels * 1000));
                                    if (said && keeps && k % Mathf.RoundToInt(.25f / Step) == 0)
                                        Debug.Log(string.Format(culture, "OWN   {0} pulled {1:0} N, {2:0.00} s: its weight {3:0} mm ahead of where it began and going {4:0} mm/s; the push it wants is {5:0} mm outside its soles; its trunk leans {6:0.0} degrees; boots tipped {7:0.0} and {8:0.0}; an ankle gives {9:0} and {10:0}, a knee {11:0} and {12:0}, a hip {13:0} and {14:0}; it feels {15:0.00} m/s2 from outside and holds its weight {16:0} mm from where it rests",
                                            name, pull, k * Step, Vector3.Dot(body.Weight() - body.centre, away) * 1000, Vector3.Dot(body.WeightGoes(), away) * 1000, body.outside * 1000,
                                            Quaternion.Angle(body.upright, body.Of(Hips).rotation), Quaternion.Angle(plan[Foot].turned, body.Of(Foot).rotation), Quaternion.Angle(plan[Foot + 1].turned, body.Of(Foot + 1).rotation),
                                            body.gave[Foot], body.gave[Foot + 1], body.gave[Shin], body.gave[Shin + 1], body.gave[Thigh], body.gave[Thigh + 1],
                                            Vector3.Dot(body.felt, away), Vector3.Dot(body.held - body.rest, away) * 1000));
                                    Vector3 head = body.Of(Head).position;
                                    if (head.y - spot.y < .7f * (headBefore.y - spot.y)) { down = true; break; }
                                    leant = Mathf.Max(leant, Vector3.Dot(head - headBefore, away));
                                    if (k >= pulled && settled < 0 && body.Goes(Head).magnitude < .01f) settled = (k - pulled) * Step;
                                    if (k == pulled / 2 || k == pulled - 1) Picture(body);
                                }
                                Vector3 headAfter = body.Of(Head).position;
                                Debug.Log(string.Format(culture, "OWN {0} pulled with {1:0} N ({2:0}% of its weight), {3}: {4}; its head went {5:0} mm with the pull at most{6}",
                                    name, pull, 100 * pull / (whole * 9.81f), called, down ? "IT FELL" : "it stood",
                                    leant * 1000, down ? "" : string.Format(culture, ", was still again {0} after the pull ended, and ended {1:0.0} mm from where it had stood",
                                        settled < 0 ? "not within three seconds" : settled.ToString("0.00", culture) + " s", Vector3.ProjectOnPlane(headAfter - headBefore, Vector3.up).magnitude * 1000)));
                                UnityEngine.Object.Destroy(body.root);
                                yield return null;
                            }

                            // ---- 3. In the air, moving its limbs: nothing may push it but its weight. Once with nothing
                            // slowing its parts, and once slowed as the let-go body's parts are today.
                            for (int slowed = 0; slowed < 2; slowed++)
                            {
                                body = Build(plan, articulated, Vector3.up * 400, fullAt, slows, slowed == 1 ? .05f : 0, slowed == 1 ? .6f : 0, false, keeps);
                                Physics.Simulate(Step);
                                Vector3 wentAt = body.WeightGoes(), spun = body.Spin();
                                float offCourse = 0, offSpin = 0, spinSeen = 0;
                                for (int k = 1; k <= Mathf.RoundToInt(1.5f / Step); k++)
                                {
                                    float t = k * Step, swing = 40 * Mathf.Sin(2 * Mathf.PI * t);
                                    for (int i = 0; i < 2; i++)
                                    {
                                        Hold(body, UpperArm + i, i == 0 ? swing : -swing);
                                        Hold(body, Thigh + i, i == 0 ? -swing : swing);
                                        Hold(body, Shin + i, -20 * (1 - Mathf.Cos(2 * Mathf.PI * t)));
                                    }
                                    // (In the air there is no ground to push: a twentieth of the push, so that its legs do not spin.)
                                    if (keeps) Keep(body, plan, quick, fullAt, slows, .05f);
                                    Physics.Simulate(Step);
                                    offCourse = Mathf.Max(offCourse, (body.WeightGoes() - wentAt - Physics.gravity * t).magnitude);
                                    offSpin = Mathf.Max(offSpin, (body.Spin() - spun).magnitude);
                                    for (int i = 0; i < Count; i++) spinSeen = Mathf.Max(spinSeen, (body.InertiaTurned(i) * Vector3.Scale(body.Inertia(i), Quaternion.Inverse(body.InertiaTurned(i)) * body.Turns(i))).magnitude
                                        + Vector3.Cross(body.Centre(i) - body.Weight(), body.Mass(i) * (body.Goes(i) - body.WeightGoes())).magnitude);
                                }
                                Debug.Log(string.Format(culture, "OWN {0} in the air for 1.5 s, swinging its limbs, {1}, {2}: its weight left the course its weight alone gives it by {3:0.0000} m/s at most; its turning about itself changed by {4:0.0000} kg m2/s at most (a limb's own turning was up to {5:0.00})",
                                    name, called, slowed == 1 ? "its parts slowed as the let-go body's are (0.05 and 0.6)" : "nothing slowing its parts", offCourse, offSpin, spinSeen));
                                UnityEngine.Object.Destroy(body.root);
                                yield return null;
                            }

                            // ---- 4. What it costs: so many of it standing on a level floor, a step of the physics.
                            float empty = 0;
                            {
                                var watch = System.Diagnostics.Stopwatch.StartNew();
                                for (int k = 0; k < Mathf.RoundToInt(2 / Step); k++) Physics.Simulate(Step);
                                empty = (float)watch.Elapsed.TotalMilliseconds / 100;
                            }
                            foreach (float many in crowds)
                            {
                                var crowd = new List<Built>();
                                int side = Mathf.CeilToInt(Mathf.Sqrt(many));
                                for (int n = 0; n < (int)many; n++)
                                {
                                    Vector3 place = new Vector3((n % side - side / 2) * 1.6f, floorTop - spot.y - (lowest - spot.y) + .002f, (n / side - side / 2) * 1.6f);
                                    crowd.Add(Build(plan, articulated, place, fullAt, slows, 0, 0, false, keeps));
                                }
                                var stoodAt = new List<Vector3>();
                                foreach (var one in crowd) stoodAt.Add(one.Of(Head).position);
                                if (jolt > 0)
                                    for (int n = 0; n < crowd.Count; n++)
                                    {
                                        Vector3 way = Quaternion.AngleAxis(n * 360f / crowd.Count, Vector3.up) * away;
                                        for (int i = 0; i < Count; i++) crowd[n].Push(i, crowd[n].Mass(i) * jolt / Step * way, crowd[n].Centre(i));
                                    }
                                for (int k = 0; k < Mathf.RoundToInt(1 / Step); k++) { if (keeps) foreach (var one in crowd) Keep(one, plan, quick, fullAt, slows); Physics.Simulate(Step); }
                                var watch = System.Diagnostics.Stopwatch.StartNew();
                                for (int k = 0; k < Mathf.RoundToInt(5 / Step); k++) { if (keeps) foreach (var one in crowd) Keep(one, plan, quick, fullAt, slows); Physics.Simulate(Step); }
                                float each = (float)watch.Elapsed.TotalMilliseconds / 250;
                                watch.Stop();
                                // (And on to twenty seconds in all, to count who still stands.)
                                int atSix = 0;
                                foreach (var one in crowd) if (one.Of(Head).position.y - floorTop > .7f * tall) atSix++;
                                for (int k = 0; k < Mathf.RoundToInt((crowdFor - 6) / Step); k++) { if (keeps) foreach (var one in crowd) Keep(one, plan, quick, fullAt, slows); Physics.Simulate(Step); }
                                int standing = 0, asItStood = 0;
                                float furthestOff = 0;
                                for (int n = 0; n < crowd.Count; n++)
                                {
                                    Vector3 head = crowd[n].Of(Head).position;
                                    if (head.y - floorTop > .7f * tall) standing++;
                                    // (As it stood: its head no lower than nineteen twentieths of where it was, and within a
                                    // tenth of its height of there, over the ground.)
                                    float off = Vector3.ProjectOnPlane(head - stoodAt[n], Vector3.up).magnitude;
                                    if (head.y - floorTop > .95f * (stoodAt[n].y - floorTop) && off < .1f * tall) { asItStood++; furthestOff = Mathf.Max(furthestOff, off); }
                                }
                                Debug.Log(string.Format(culture, "OWN {0} cost, {1}: {2:0} of it standing on a level floor take {3:0.000} ms more than the place alone ({4:0.000} ms) for each fiftieth of a second, in the Editor; {5} of them still stand after six seconds and {6} after {7:0}; {8} stand as they stood (head no lower than 95% and within a tenth of its height of where it was; the furthest of those {9:0} mm off){10}",
                                    name, called, many, each - empty, empty, atSix, standing, crowdFor, asItStood, furthestOff * 1000, jolt > 0 ? string.Format(culture, "; each was set going at {0:0.00} m/s, each a different way", jolt) : ""));
                                foreach (var one in crowd) UnityEngine.Object.Destroy(one.root);
                                yield return null;
                            }
                        }
                    }
                    Physics.simulationMode = mode;
                    unit.gameObject.SetActive(true);
                    yield return null;
                }
                UnityEngine.Object.Destroy(floor);
            }
            finally
            {
                Physics.simulationMode = mode;
                Time.captureFramerate = 0;
            }
        }
    }
}
