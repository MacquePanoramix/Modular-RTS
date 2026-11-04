using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // Layer 0 of the getting up for any body (Docs/Design/AnyBody/joint_model.md, step 0; Luis decided the direction on
    // November 3, 2026): what the engine does with an articulation's joints, tested on bare bodies before anything is built
    // on it. Each test makes its own small body away from everything else, steps the physics itself, and takes it away.
    public sealed class ArticulationFactsTests
    {
        private const float Dt = .02f;
        private readonly List<GameObject> made = new List<GameObject>();
        private SimulationMode modeWas;

        [SetUp] public void Begin() { modeWas = Physics.simulationMode; Physics.simulationMode = SimulationMode.Script; }

        [TearDown]
        public void End()
        {
            foreach (var g in made) if (g != null) Object.DestroyImmediate(g);
            made.Clear();
            Physics.simulationMode = modeWas;
        }

        // A fixed root far from the place, and on it links as asked.
        private ArticulationBody Root(Vector3 at)
        {
            var g = new GameObject("Facts root");
            g.transform.position = at;
            made.Add(g);
            var root = g.AddComponent<ArticulationBody>();
            root.immovable = true;
            root.useGravity = false;
            return root;
        }

        private static ArticulationBody Link(Transform parent, string name, Vector3 at, Quaternion turned, float kilograms, Vector3 centre, Vector3 inertia)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.SetPositionAndRotation(at, turned);
            var link = g.AddComponent<ArticulationBody>();
            link.mass = kilograms;
            link.automaticCenterOfMass = false; link.centerOfMass = centre;
            link.automaticInertiaTensor = false; link.inertiaTensor = inertia; link.inertiaTensorRotation = Quaternion.identity;
            link.linearDamping = 0; link.angularDamping = 0; link.jointFriction = 0;
            return link;
        }

        private static void Steps(int n) { for (int i = 0; i < n; i++) Physics.Simulate(Dt); }

        // A drive's force limit is a torque in newton metres (not an impulse a step): an arm held level by a drive whose
        // limit is a tenth above what its weight asks stays level; a tenth below, it falls.
        [UnityTest] public IEnumerator ForceLimitATenthAboveHolds() { yield return HeldBy(1.1f, true); }
        [UnityTest] public IEnumerator ForceLimitATenthBelowFalls() { yield return HeldBy(.9f, false); }

        private IEnumerator HeldBy(float share, bool holds)
        {
            var root = Root(new Vector3(500, 50, 500));
            // A hinge about the world's forward axis, the arm along the world's right, its weight half a metre out.
            var arm = Link(root.transform, "Arm", root.transform.position, Quaternion.identity, 2, new Vector3(.5f, 0, 0), new Vector3(.01f, .01f, .01f));
            arm.jointType = ArticulationJointType.RevoluteJoint;
            arm.anchorPosition = Vector3.zero; arm.anchorRotation = Quaternion.Euler(0, -90, 0);
            arm.twistLock = ArticulationDofLock.FreeMotion;
            float weighs = 2 * Physics.gravity.magnitude * .5f;
            arm.xDrive = new ArticulationDrive { stiffness = 1e5f, damping = 1e3f, forceLimit = share * weighs, target = 0, driveType = ArticulationDriveType.Force };
            yield return null;
            Steps(100);
            float sank = Mathf.Abs(arm.jointPosition[0] * Mathf.Rad2Deg);
            Debug.Log($"FACTS force limit {share:0.0} of its weight: turned {sank:0.0} degrees in 2 s");
            // (A tenth too weak, it sinks towards where its weight's turning, which falls as it sinks, meets the drive's full
            // torque: cos 25.8 degrees = 0.9. Were the limit an impulse a step, it would be fifty times weaker and the arm
            // would hang straight down.)
            if (holds) Assert.That(sank, Is.LessThan(5), "A drive a tenth stronger than the weight it holds let it fall.");
            else Assert.That(sank, Is.InRange(3f, 27f), "A drive a tenth weaker than its weight did not sink towards 26 degrees.");
        }

        // A body made lying, with each joint's zero at its standing relation and not at how it was made, keeps how it was
        // made when its joints' positions are written before the first step: no snap to standing (October 14).
        [UnityTest] public IEnumerator MadeLyingStaysAsMade() { yield return MadeLying(false); }
        [UnityTest] public IEnumerator MadeLyingStaysAsMadeUnderGravity() { yield return MadeLying(true); }

        private IEnumerator MadeLying(bool gravity)
        {
            {
                var root = Root(new Vector3(-500, 50, 500));
                // Two links hung from the root, made turned 70 degrees and then 40 more from their standing relation.
                Quaternion madeA = Quaternion.Euler(70, 10, 0), madeB = Quaternion.Euler(40, 0, 20);
                var a = Link(root.transform, "A", root.transform.position, root.transform.rotation * madeA, 3, new Vector3(0, -.2f, 0), new Vector3(.02f, .005f, .02f));
                var b = Link(a.transform, "B", a.transform.TransformPoint(new Vector3(0, -.4f, 0)), a.transform.rotation * madeB, 2, new Vector3(0, -.2f, 0), new Vector3(.015f, .004f, .015f));
                foreach (var link in new[] { a, b })
                {
                    link.useGravity = gravity;
                    link.jointType = ArticulationJointType.SphericalJoint;
                    link.twistLock = link.swingYLock = link.swingZLock = ArticulationDofLock.FreeMotion;
                    link.matchAnchors = false;
                    link.anchorPosition = Vector3.zero; link.anchorRotation = Quaternion.identity;
                    var parent = link.transform.parent;
                    link.parentAnchorPosition = parent.InverseTransformPoint(link.transform.position);
                    link.parentAnchorRotation = Quaternion.identity;   // the standing relation: zero
                    var hold = new ArticulationDrive { stiffness = 0, damping = 0, forceLimit = 0, driveType = ArticulationDriveType.Force };
                    link.xDrive = hold; link.yDrive = hold; link.zDrive = hold;
                }
                Vector3 aAt = a.transform.position, bAt = b.transform.position;
                Quaternion aTurn = a.transform.rotation, bTurn = b.transform.rotation;
                yield return null;
                // The joints' positions as made: each the rotation vector of its turn from zero, in its own frame.
                var starts = new List<int>(); root.GetDofStartIndices(starts);
                var q = new List<float>(); root.GetJointPositions(q);
                Debug.Log($"FACTS made lying, gravity {gravity}: before the write the engine reads A {Vector(q, starts[a.index])} B {Vector(q, starts[b.index])}");
                Put(q, starts[a.index], RotationVector(madeA));
                Put(q, starts[b.index], RotationVector(madeB));
                root.SetJointPositions(q);
                var zeros = new List<float>(new float[q.Count]); root.SetJointVelocities(zeros);
                foreach (int n in new[] { 1, 4, 45 })
                {
                    Steps(n);
                    // (Under gravity, with no drive to hold it, it falls: only the first step says whether it snapped.)
                    if (gravity)
                    {
                        if (n == 1) Assert.That(Mathf.Max(Quaternion.Angle(a.transform.rotation, aTurn), Quaternion.Angle(b.transform.rotation, bTurn)), Is.LessThan(1), "It snapped from how it was made in its first step.");
                        continue;
                    }
                    Assert.That(Vector3.Distance(a.transform.position, aAt), Is.LessThan(.001f), "A moved from where it was made.");
                    Assert.That(Vector3.Distance(b.transform.position, bAt), Is.LessThan(.001f), "B moved from where it was made.");
                    Assert.That(Quaternion.Angle(a.transform.rotation, aTurn), Is.LessThan(.2f), "A turned from how it was made.");
                    Assert.That(Quaternion.Angle(b.transform.rotation, bTurn), Is.LessThan(.2f), "B turned from how it was made.");
                }
                Debug.Log($"FACTS made lying, gravity {gravity}: after 50 steps A {Quaternion.Angle(a.transform.rotation, aTurn):0.0} and B {Quaternion.Angle(b.transform.rotation, bTurn):0.0} degrees from as made");
            }
        }

        private static Vector3 RotationVector(Quaternion q)
        {
            q.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            return axis * (angle * Mathf.Deg2Rad);
        }
        private static void Put(List<float> q, int at, Vector3 v) { q[at] = v.x; q[at + 1] = v.y; q[at + 2] = v.z; }
        private static string Vector(List<float> q, int at) => $"({q[at]:0.000}, {q[at + 1]:0.000}, {q[at + 2]:0.000})";

        // A joint turns no faster than its maxJointVelocity, whatever drives it; a link's maxAngularVelocity is told of too.
        [UnityTest]
        public IEnumerator JointSpeedBoundHolds()
        {
            var root = Root(new Vector3(500, 50, -500));
            var wheel = Link(root.transform, "Wheel", root.transform.position, Quaternion.identity, 1, Vector3.zero, new Vector3(.01f, .01f, .01f));
            wheel.useGravity = false;
            wheel.jointType = ArticulationJointType.RevoluteJoint;
            wheel.twistLock = ArticulationDofLock.FreeMotion;
            // (A drive's target speed is in degrees a second: 40 rad/s asked.)
            wheel.xDrive = new ArticulationDrive { stiffness = 0, damping = 50, forceLimit = 50, targetVelocity = 40 * Mathf.Rad2Deg, driveType = ArticulationDriveType.Force };
            wheel.maxJointVelocity = 5;
            yield return null;
            Steps(100);
            float driven = Mathf.Abs(wheel.jointVelocity[0]);
            Debug.Log($"FACTS bounded at 5 rad/s and driven at 40 by its drive: turns at {driven:0.00} rad/s");
            // (And pushed by a force on the joint, no drive.)
            wheel.xDrive = new ArticulationDrive { stiffness = 0, damping = 0, forceLimit = 0, driveType = ArticulationDriveType.Force };
            wheel.jointVelocity = new ArticulationReducedSpace(0f);
            for (int i = 0; i < 100; i++) { wheel.jointForce = new ArticulationReducedSpace(5f); Physics.Simulate(Dt); }
            float pushed = Mathf.Abs(wheel.jointVelocity[0]);
            Debug.Log($"FACTS bounded at 5 rad/s and pushed by 5 N m: turns at {pushed:0.00} rad/s");
            wheel.xDrive = new ArticulationDrive { stiffness = 0, damping = 50, forceLimit = 50, targetVelocity = 40 * Mathf.Rad2Deg, driveType = ArticulationDriveType.Force };
            wheel.maxJointVelocity = 100;
            wheel.maxAngularVelocity = 2;
            Steps(100);
            Debug.Log($"FACTS maxAngularVelocity 2, driven at 40: turns at {Mathf.Abs(wheel.jointVelocity[0]):0.00} rad/s (what the link's own bound does)");
            wheel.maxAngularVelocity = 1000;
            Steps(100);
            Debug.Log($"FACTS unbounded, driven at 40: turns at {Mathf.Abs(wheel.jointVelocity[0]):0.00} rad/s");
            // (What is held here is what the engine does, read and written down; the bound's own test is the pushed wheel.)
            Assert.That(pushed, Is.LessThan(5.3f), "maxJointVelocity did not bound a joint pushed by a force.");
        }

        // The engine's own sum of what gravity asks of each joint, put on the joints each step, holds a hung body where it is.
        [UnityTest] public IEnumerator GravityForcesHoldAHungBody() { yield return Hung(true); }
        [UnityTest] public IEnumerator AHungBodyFallsWithoutThem() { yield return Hung(false); }

        private IEnumerator Hung(bool held)
        {
            var root = Root(new Vector3(-500, 50, -500));
            var a = Link(root.transform, "A", root.transform.position, Quaternion.Euler(0, 0, 90), 3, new Vector3(0, -.2f, 0), new Vector3(.02f, .005f, .02f));
            var b = Link(a.transform, "B", a.transform.TransformPoint(new Vector3(0, -.4f, 0)), a.transform.rotation * Quaternion.Euler(0, 0, -45), 2, new Vector3(0, -.2f, 0), new Vector3(.015f, .004f, .015f));
            foreach (var link in new[] { a, b })
            {
                link.jointType = ArticulationJointType.SphericalJoint;
                link.twistLock = link.swingYLock = link.swingZLock = ArticulationDofLock.FreeMotion;
                link.anchorPosition = Vector3.zero; link.anchorRotation = Quaternion.identity; link.matchAnchors = true;
                var none = new ArticulationDrive { stiffness = 0, damping = 0, forceLimit = 0, driveType = ArticulationDriveType.Force };
                link.xDrive = none; link.yDrive = none; link.zDrive = none;
            }
            Quaternion aTurn = a.transform.rotation, bTurn = b.transform.rotation;
            yield return null;
            var g = new List<float>();
            for (int i = 0; i < 100; i++)
            {
                if (held) { root.GetJointGravityForces(g); root.SetJointForces(g); }
                Physics.Simulate(Dt);
            }
            float driftA = Quaternion.Angle(a.transform.rotation, aTurn), driftB = Quaternion.Angle(b.transform.rotation, bTurn);
            Debug.Log($"FACTS {(held ? "held by gravity's own forces" : "left to fall")} for 2 s: A drifted {driftA:0.00}, B {driftB:0.00} degrees");
            if (held) Assert.That(Mathf.Max(driftA, driftB), Is.LessThan(2), "The engine's gravity forces, put on the joints, did not hold the body.");
            else Assert.That(Mathf.Max(driftA, driftB), Is.GreaterThan(20), "The hung body did not fall without the forces: the test proves nothing.");
        }

        // Where a ball joint's reading jumps by a whole turn, and whether the two links' body frames (each one's centre of
        // mass and inertia axes) set it, as joint_model.md §1.1 read in the engine's source (on October 30 the boots'
        // readings jumped so, then turned at 74 to 168 rad/s). A link spun about its joint's own axis, its inertia the same
        // about every axis (so how its body frame is turned changes nothing of how it moves), gravity off. Found (November
        // 4): the reading runs on with the turn, about the joint's axis in the joint's frame; and with the spun link's body
        // frame turned a third of a turn about the same axis it reads the same: the body frames do not set where it jumps.
        [UnityTest]
        public IEnumerator ABallJointReadsOnWithItsTurn()
        {
            float jumped = 0, turned = 0;
            Vector3 end = default;
            yield return Spun(Quaternion.identity, 175, (j, at90, e, t) => { jumped = j; end = e; turned = t; });
            Assert.That(jumped, Is.LessThan(0), $"The reading jumped at {jumped:0} degrees, before half a turn.");
            Assert.That(Mathf.Abs(end.x - turned * Mathf.Deg2Rad), Is.LessThan(.05f), "The reading is not the turn about the joint's axis.");
        }

        [UnityTest]
        public IEnumerator ItsBodyFrameDoesNotMoveWhereTheReadingJumps()
        {
            float lined = 0, turned = 0;
            Vector3 linedAt90 = default, turnedAt90 = default, linedEnd = default, turnedEnd = default;
            yield return Spun(Quaternion.identity, 350, (j, at90, e, t) => { lined = j; linedAt90 = at90; linedEnd = e; });
            yield return Spun(Quaternion.Euler(120, 0, 0), 350, (j, at90, e, t) => { turned = j; turnedAt90 = at90; turnedEnd = e; });
            Assert.That(turned, Is.EqualTo(lined).Within(3f), $"The body frame turned a third of a turn moved where the reading jumps: {lined:0} degrees lined up, {turned:0} turned.");
            Assert.That(Vector3.Distance(turnedAt90, linedAt90), Is.LessThan(.01f), "The body frame turned a third of a turn changed the reading at a quarter turn.");
            Assert.That(Vector3.Distance(turnedEnd, linedEnd), Is.LessThan(.05f), "The body frame turned a third of a turn changed the reading near a whole turn.");
        }

        // A link spun about its joint's axis until it has turned so far; what its reading did.
        private IEnumerator Spun(Quaternion bodyFrame, float until, System.Action<float, Vector3, Vector3, float> done)
        {
            var root = Root(new Vector3(0, 50, 900));
            root.automaticCenterOfMass = false; root.centerOfMass = Vector3.zero;
            root.automaticInertiaTensor = false; root.inertiaTensor = new Vector3(.01f, .01f, .01f); root.inertiaTensorRotation = Quaternion.identity;
            var spun = Link(root.transform, "Spun", root.transform.position, root.transform.rotation, 1, Vector3.zero, new Vector3(.01f, .01f, .01f));
            spun.inertiaTensorRotation = bodyFrame;
            spun.useGravity = false;
            spun.jointType = ArticulationJointType.SphericalJoint;
            spun.twistLock = spun.swingYLock = spun.swingZLock = ArticulationDofLock.FreeMotion;
            spun.anchorPosition = Vector3.zero; spun.anchorRotation = Quaternion.identity; spun.matchAnchors = true;
            var none = new ArticulationDrive { stiffness = 0, damping = 0, forceLimit = 0, driveType = ArticulationDriveType.Force };
            spun.xDrive = none; spun.yDrive = none; spun.zDrive = none;
            yield return null;
            spun.jointVelocity = new ArticulationReducedSpace(1f, 0f, 0f);
            Quaternion was = spun.transform.rotation;
            Vector3 read = Vector3.zero, readAt90 = Vector3.zero;
            float turned = 0, jumpedAt = -1, biggest = 0;
            while (turned < until)
            {
                Physics.Simulate(Dt);
                Quaternion now = spun.transform.rotation;
                float before = turned;
                turned += Quaternion.Angle(was, now); was = now;
                var jp = spun.jointPosition;
                Vector3 reads = new Vector3(jp[0], jp[1], jp[2]);
                float step = Vector3.Distance(reads, read);
                if (step > biggest) biggest = step;
                if (step > 1 && jumpedAt < 0) jumpedAt = turned;
                if (before < 90 && turned >= 90) readAt90 = reads;
                read = reads;
            }
            Debug.Log($"FACTS spun to {turned:0} degrees, its body frame turned {Quaternion.Angle(Quaternion.identity, bodyFrame):0} degrees from the joint's: the reading's largest step {biggest:0.00} rad, its first jump {(jumpedAt < 0 ? "none" : "at " + jumpedAt.ToString("0") + " degrees")}; at 90 degrees it read {readAt90.x:0.000}, {readAt90.y:0.000}, {readAt90.z:0.000}, at the end {read.x:0.000}, {read.y:0.000}, {read.z:0.000}");
            done(jumpedAt, readAt90, read, turned);
        }
    }
}
