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

        // What the engine's step does to a leg that turns fast (November 10; the boot that flies when a miner is shoved
        // again and again: in the game, from the step its boot rolled over, the body gained 90, then 300 to 3600 joules a
        // step that none of its joints was counted to give). Hips afloat with a leg on them as Small's is (thigh on a ball
        // joint, shin on a hinge, boot on a ball joint; their weights and turning weights as the game's), no weight,
        // nothing to touch, no stops. Set turning at every joint and left alone, what it holds in its motion must stay
        // what it was; damped, it must lose, and the count of what its drives gave must say so; sprung and damped as the
        // game's joints are, or pushed with a torque on the thigh and its opposite on the hips, it may gain only what
        // those give it (each joint's drive force times how fast the joint turns, and the torque times how fast the thigh
        // turns against the hips, at the end of the step: the game's own count).
        private ArticulationBody[] FloatingLeg(Vector3 at, float[] spring, float[] damper, float[] limit)
        {
            var g = new GameObject("Facts hips");
            g.transform.position = at;
            made.Add(g);
            var hips = g.AddComponent<ArticulationBody>();
            hips.immovable = false; hips.useGravity = false; hips.mass = 35;
            hips.automaticCenterOfMass = false; hips.centerOfMass = new Vector3(0, .25f, 0);
            hips.automaticInertiaTensor = false; hips.inertiaTensor = new Vector3(1.2f, .4f, 1.2f); hips.inertiaTensorRotation = Quaternion.identity;
            hips.linearDamping = 0; hips.angularDamping = 0;
            var thigh = Link(hips.transform, "Thigh", at + new Vector3(.09f, 0, 0), Quaternion.identity, 2.38f, new Vector3(0, -.13f, 0), new Vector3(.0131f, .0029f, .0131f));
            var shin = Link(thigh.transform, "Shin", thigh.transform.position + new Vector3(0, -.27f, 0), Quaternion.identity, 1.35f, new Vector3(0, -.12f, 0), new Vector3(.0074f, .0011f, .0074f));
            var boot = Link(shin.transform, "Boot", shin.transform.position + new Vector3(0, -.25f, 0), Quaternion.identity, 1.29f, new Vector3(0, -.04f, .03f), new Vector3(.0030f, .0037f, .0015f));
            var parts = new[] { hips, thigh, shin, boot };
            for (int i = 1; i < 4; i++)
            {
                var link = parts[i];
                link.useGravity = false;
                link.jointType = i == 2 ? ArticulationJointType.RevoluteJoint : ArticulationJointType.SphericalJoint;
                link.anchorPosition = Vector3.zero; link.anchorRotation = Quaternion.identity; link.matchAnchors = true;
                link.twistLock = ArticulationDofLock.FreeMotion;
                if (i != 2) { link.swingYLock = ArticulationDofLock.FreeMotion; link.swingZLock = ArticulationDofLock.FreeMotion; }
                var drive = new ArticulationDrive { stiffness = spring[i], damping = damper[i], forceLimit = limit[i], target = 0, driveType = ArticulationDriveType.Force };
                link.xDrive = drive;
                if (i != 2) { link.yDrive = drive; link.zDrive = drive; }
            }
            return parts;
        }

        // What a body holds in its motion (joules): each part's going and its turning.
        // (How fast each part's centre truly goes is worked out along the chain, from the root's going and each part's
        // turning: what the engine tells of a part's own going is too great when the part turns fast, by the square root
        // of one and (its turning times the step) squared: ADrivesForceIsWhatItTakes, November 10. Told = true: by the
        // engine's own telling, to set beside it.)
        private static Vector3 TrulyGoes(ArticulationBody part, Dictionary<ArticulationBody, Vector3> known)
        {
            if (known.TryGetValue(part, out Vector3 goes)) return goes;
            if (part.isRoot) goes = part.immovable ? Vector3.zero : part.linearVelocity;
            else
            {
                var parent = part.transform.parent.GetComponentInParent<ArticulationBody>();
                Vector3 anchor = part.transform.TransformPoint(part.anchorPosition);
                Vector3 parentTurns = parent.immovable ? Vector3.zero : parent.angularVelocity;
                goes = TrulyGoes(parent, known) + Vector3.Cross(parentTurns, anchor - parent.worldCenterOfMass) + Vector3.Cross(part.angularVelocity, part.worldCenterOfMass - anchor);
            }
            known[part] = goes;
            return goes;
        }

        private static float Held(ArticulationBody[] parts, bool told = false)
        {
            float held = 0;
            var known = new Dictionary<ArticulationBody, Vector3>();
            foreach (var part in parts)
            {
                Vector3 goes = told ? part.GetPointVelocity(part.worldCenterOfMass) : TrulyGoes(part, known);
                Vector3 turns = Quaternion.Inverse(part.transform.rotation * part.inertiaTensorRotation) * part.angularVelocity, inertia = part.inertiaTensor;
                held += .5f * part.mass * goes.sqrMagnitude + .5f * (inertia.x * turns.x * turns.x + inertia.y * turns.y * turns.y + inertia.z * turns.z * turns.z);
            }
            return held;
        }

        // (Of the last leg set turning: what it held after its first step and the most it held, both by the engine's own
        // telling of how its parts go; and the fastest any of its joints turned, rad/s.)
        private (float toldBegan, float toldMost, float fastest) Whirl;

        // The leg set turning at so many radians a second at each joint, for a second. Which way: 0 left alone; 1 damped
        // only; 2 sprung and damped as the game's joints; 3 so, and pushed with 100 N m at the hip. What it held after
        // the first step; the most and the least it held after, as shares of that; what its drives and the torque were
        // counted to give over the second; and what it gained beyond that (joules).
        // (And how it is made or set going, to find what the gain comes of: 0 as the game's leg; 1 turning about one line
        // only, every joint about its own x, so the leg stays in a plane; 2 each part's turning weight the same about
        // every line of it, its largest; 3 the step halved, a hundredth of a second; 4 the step quartered.)
        private IEnumerator Whirled(float speed, int way, System.Action<float, float, float, float, float> done, int made0 = 0)
        {
            var none = new float[4];
            // (Ways 4 and 5: sprung as the game's joints but damped as the design of the motion has it, each joint's
            // strength over its top speed, 29, 34 and 23 rad/s for Small's hip, knee and ankle; 4 pushed, 5 not.)
            var dampers = way >= 4 ? new[] { 0, 4.5f, 2.6f, 2.65f } : way >= 1 ? new[] { 0, 1.2f, .5f, .05f } : none;
            var parts = FloatingLeg(new Vector3(-500, 50, -500), way >= 2 ? new[] { 0, 80f, 54f, 3f } : none, dampers, way >= 1 ? new[] { 0, 130f, 87f, 61f } : none);
            if (made0 == 2)
                for (int i = 1; i < 4; i++)
                {
                    Vector3 turning = parts[i].inertiaTensor;
                    float largest = Mathf.Max(turning.x, Mathf.Max(turning.y, turning.z));
                    parts[i].inertiaTensor = new Vector3(largest, largest, largest);
                }
            yield return null;
            parts[1].jointVelocity = made0 == 1 ? new ArticulationReducedSpace(speed, 0, 0) : new ArticulationReducedSpace(speed, .3f * speed, 0);
            parts[2].jointVelocity = new ArticulationReducedSpace(-speed);
            parts[3].jointVelocity = made0 == 1 ? new ArticulationReducedSpace(speed, 0, 0) : new ArticulationReducedSpace(.5f * speed, speed, 0);
            float dt = made0 == 3 ? .5f * Dt : made0 == 4 ? .25f * Dt : Dt;
            int steps = Mathf.RoundToInt(1 / dt);
            float held = 0, began = 0, most = 0, least = 0, beyond = 0, given = 0;
            Whirl = (0, 0, 0);
            // (What a torque or a drive gave over a step: itself times the mean of how the joint turned at the step's
            // start and at its end.)
            Vector3 againstBefore = default;
            var jointBefore = new float[4][];
            for (int k = 0; k <= steps; k++)
            {
                Vector3 push = (way == 3 || way == 4 ? 100 : 0) * Vector3.right;
                if (push.x != 0) { parts[1].AddTorque(push); parts[0].AddTorque(-push); }
                Physics.Simulate(dt);
                Vector3 against = parts[1].angularVelocity - parts[0].angularVelocity;
                float now = Held(parts), by = Vector3.Dot(push, k == 0 ? against : .5f * (against + againstBefore)) * dt;
                againstBefore = against;
                for (int i = 1; i < 4; i++)
                {
                    if (jointBefore[i] == null) jointBefore[i] = new float[parts[i].dofCount];
                    for (int d = 0; d < parts[i].dofCount; d++)
                    {
                        float turns = parts[i].jointVelocity[d];
                        by += parts[i].driveForce[d] * (k == 0 ? turns : .5f * (turns + jointBefore[i][d])) * dt;
                        jointBefore[i][d] = turns;
                    }
                }
                if (k == 0) { began = most = least = now; }
                else { most = Mathf.Max(most, now); least = Mathf.Min(least, now); beyond += now - held - by; given += by; }
                held = now;
                // (And by the engine's own telling of how its parts go; and the fastest any joint turns.)
                float toldNow = Held(parts, true), fastest = 0;
                for (int i = 1; i < 4; i++)
                    for (int d = 0; d < parts[i].dofCount; d++) fastest = Mathf.Max(fastest, Mathf.Abs(parts[i].jointVelocity[d]));
                Whirl = (k == 0 ? toldNow : Whirl.toldBegan, Mathf.Max(Whirl.toldMost, toldNow), Mathf.Max(Whirl.fastest, fastest));
            }
            made.Remove(parts[0].gameObject);
            Object.DestroyImmediate(parts[0].gameObject);
            done(began, most / Mathf.Max(1e-6f, began), least / Mathf.Max(1e-6f, began), given, beyond);
        }

        // What the gain comes of: the same leg left alone, made or set going each other way. Found (November 10, by true
        // speeds): it is not the parts' spinning about their own long lines (turning in one plane only, or with every
        // part's turning weight made the same about every line, it still gains 470 and 680 times at 30 rad/s, against
        // 1030); it is the size of the step against the speed (the step halved 3 times, quartered 1.3, at 30 rad/s). And
        // one step alone shows it: set turning at 60 rad/s, after its first step of a fiftieth of a second the leg holds
        // 745 J; of a two-hundredth, 460.
        [UnityTest]
        public IEnumerator WhatAFastLegsGainComesOf()
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            string[] hows = { "as the game's leg", "turning in one plane only", "its parts' turning weights made the same about every line", "the step halved (0.01 s)", "the step quartered (0.005 s)" };
            var mostBy = new Dictionary<(float, int), float>();
            var beganBy = new Dictionary<(float, int), float>();
            foreach (float speed in new[] { 10f, 15f, 20f, 30f, 60f, 120f })
                for (int how = 0; how < hows.Length; how++)
                {
                    float began = 0, most = 0, least = 0;
                    yield return Whirled(speed, 0, (b, m, l, g, e) => { began = b; most = m; least = l; }, how);
                    Debug.Log(string.Format(inv, "FACTS a leg left alone, set turning at {0:0} rad/s at each joint, {1}: it held {2:0.0} J after the first step; over the next second at most {3:0.00} and at least {4:0.00} of that; by the engine's own telling {5:0.0} J and at most {6:0.00}; its fastest joint turned at {7:0} rad/s", speed, hows[how], began, most, least, Whirl.toldBegan, Whirl.toldMost / Mathf.Max(1e-6f, Whirl.toldBegan), Whirl.fastest));
                    mostBy[(speed, how)] = most; beganBy[(speed, how)] = began;
                }
            Assert.That(mostBy[(30f, 1)], Is.GreaterThan(10f), "Turning in one plane only, at 30 rad/s, the leg kept what it held: the gain is of the parts' spinning after all.");
            Assert.That(mostBy[(30f, 2)], Is.GreaterThan(10f), "With its parts' turning weights made the same about every line, at 30 rad/s, the leg kept what it held.");
            Assert.That(mostBy[(30f, 4)], Is.LessThan(1.5f), "With the step quartered, at 30 rad/s, the leg still gained half again: the gain is not of the step's size.");
            Assert.That(beganBy[(60f, 0)] / beganBy[(60f, 4)], Is.GreaterThan(1.4f), "Set turning at 60 rad/s, the leg held no more after one step of a fiftieth of a second than after one of a two-hundredth: one step alone does not add what was found.");
        }

        // Found (November 10, the game's step of a fiftieth of a second, the game's solver; what it holds reckoned from
        // its parts' true speeds, TrulyGoes): left alone, the leg does NOT keep what it holds. Turning at 5 rad/s at each
        // joint it holds at most 1.07 of what it began with over a second; at 15 rad/s 1.55; at 30 rad/s a thousand times
        // as much, its joints by then at the engine's own bound of 100 rad/s. (By the engine's own telling of its
        // parts' speeds, which is too great when they turn fast: 1.46 and 1886; under the other solver, so told, 14.9
        // and 1359.) One part on one hinge keeps what it holds exactly (ADrivesForceIsWhatItTakes): it is the chain on
        // its floating root that gains. Sprung and damped as the game's joints are, or as the design of the motion has
        // them (each joint's strength over its top speed), it holds no more than it began with at 30 rad/s (at 60 the
        // game's 1.2 times, the design's no more); with the game's dampers and no springs it gains a third at 30 and 33
        // times at 60, and why the springs matter is not known. Pushed besides with a torque put straight on the thigh
        // (100 N m, as the standing balance puts its worked-out torques on the parts) it ran away once with the
        // design's dampers (13 times what it held, set turning at 30 rad/s; not at 15 or 60): a torque put straight on a
        // part makes it unpredictable. So no part of a body may be let turn
        // faster than its springs and dampers can answer for, and nothing may be able to set it turning so in one step.
        // (Reviewed: Docs/Reviews/2026-11-10_FlyingBoot_K1 in the main project.)
        [UnityTest]
        public IEnumerator AFastLegGainsByTheStepItself()
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            string[] ways = { "left alone", "damped only", "sprung and damped as the game's joints", "sprung, damped, and pushed with 100 N m at the hip", "sprung, damped as the design has it (strength over top speed), and pushed with 100 N m at the hip", "sprung, and damped as the design has it" };
            var mostBy = new Dictionary<(float, int), float>();
            foreach (float speed in new[] { 5f, 15f, 30f, 60f, 120f })
                for (int way = 0; way < ways.Length; way++)
                {
                    float began = 0, most = 0, least = 0, given = 0, beyond = 0;
                    yield return Whirled(speed, way, (b, m, l, g, e) => { began = b; most = m; least = l; given = g; beyond = e; });
                    Debug.Log(string.Format(inv, "FACTS a leg set turning at {0:0} rad/s at each joint, {1}: it held {2:0.0} J after the first step; over the next second at most {3:0.00} and at least {4:0.00} of that; its drives and the torque were counted to give it {5:+0.0;-0.0} J, and it gained {6:+0.0;-0.0} J beyond that; by the engine's own telling it held {7:0.0} J after the first step and at most {8:0.00} of that; its fastest joint turned at {9:0} rad/s",
                        speed, ways[way], began, most, least, given, beyond, Whirl.toldBegan, Whirl.toldMost / Mathf.Max(1e-6f, Whirl.toldBegan), Whirl.fastest));
                    mostBy[(speed, way)] = most;
                }
            Assert.That(mostBy[(5f, 0)], Is.LessThan(1.15f), "Left alone and turning slowly (5 rad/s), the leg gained more than a seventh: the step adds more than was found.");
            Assert.That(mostBy[(30f, 0)], Is.GreaterThan(10f), "Left alone at 30 rad/s the leg kept what it held: the engine's step no longer adds to a fast leg, and what was built on that (how fast a part may turn, how much its dampers must take) can be looked at again.");
            Assert.That(mostBy[(30f, 2)], Is.LessThan(1.05f), "Sprung and damped as the game's joints, at 30 rad/s, the leg gained.");
            Assert.That(mostBy[(30f, 5)], Is.LessThan(1.05f), "Sprung, and damped as the design has it, at 30 rad/s, the leg gained.");
        }

        // What a drive is counted to take is what it takes, also at its limit (the review of November 10 asked: every
        // count of what a body's joints gave rests on it). One part as Small's thigh on a hinge on a fixed root, no weight,
        // set turning at 40 rad/s: left alone it must keep what it holds (one part on one hinge has nothing to gain
        // from); damped with 4.5 N m s a radian and a limit of 130 N m (so at its limit above 29 rad/s) it must lose
        // just what its drive's force, times the mean of its turning over each step, comes to. Found with it (November
        // 10): what the engine tells of how fast a turning part's centre goes is too great, by the square root of one
        // and (its turning times the step) squared (at 50 rad/s, 9.19 m/s told for 6.5 m/s gone: the part's going is
        // carried a step along its old line and never turned with it), so what a body holds must be reckoned from its
        // root's going and its parts' turning (TrulyGoes), not from the parts' own told speeds. And set turning at
        // 60 rad/s and left alone, a part is brought down to the project's greatest turning speed (50 rad/s,
        // Physics.defaultMaxAngularSpeed; the game's own body sets 20 on every part), a quarter of what is over at each
        // step: a drag the count of what its joints gave does not show.
        [UnityTest]
        public IEnumerator ADrivesForceIsWhatItTakes()
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            float freeMost = 0, freeLeast = 0, lost = 0, counted = 0, endsAt = 0;
            for (int way = 0; way < 3; way++)
            {
                var root = Root(new Vector3(-900, 50, -500));
                var part = Link(root.transform, "Thigh", root.transform.position, Quaternion.identity, 2.38f, new Vector3(0, -.13f, 0), new Vector3(.0131f, .0029f, .0131f));
                part.useGravity = false;
                part.jointType = ArticulationJointType.RevoluteJoint;
                part.anchorPosition = Vector3.zero; part.anchorRotation = Quaternion.identity; part.matchAnchors = true;
                part.twistLock = ArticulationDofLock.FreeMotion;
                part.xDrive = new ArticulationDrive { stiffness = 0, damping = way == 1 ? 4.5f : 0, forceLimit = way == 1 ? 130 : 0, driveType = ArticulationDriveType.Force };
                yield return null;
                part.jointVelocity = new ArticulationReducedSpace(way == 2 ? 60f : 40f);
                var one = new[] { part };
                float began = 0, held = 0, most = 0, least = 0, turnedBefore = 0, by = 0;
                // (What the engine says of the part at each of its first steps, and what it truly does: how far it has
                // turned by its own drawn pose, against its joint's reading and the speeds the engine tells.)
                Quaternion turnedWas = part.transform.rotation;
                Vector3 centreWas = part.worldCenterOfMass;
                for (int k = 0; k <= 50; k++)
                {
                    Physics.Simulate(Dt);
                    float now = Held(one), turns = part.jointVelocity[0];
                    if (k < 6 || k % 10 == 0)
                    {
                        float trulyTurns = Quaternion.Angle(turnedWas, part.transform.rotation) * Mathf.Deg2Rad / Dt, trulyGoes = (part.worldCenterOfMass - centreWas).magnitude / Dt;
                        Debug.Log(string.Format(inv, "FACTS hinge {0} step {1}: its joint reads {2:0.00} rad at {3:0.00} rad/s; the engine says it turns at {4:0.00} rad/s and its centre goes at {5:0.00} m/s (its point speed there {6:0.00}); by its own pose it turned at {7:0.00} rad/s over the step and its centre went {8:0.00} m/s along the chord; it holds {9:0.0} J, and by the engine's own telling {10:0.0} J",
                            way == 0 ? "left alone" : way == 1 ? "damped" : "left alone at 60 rad/s", k, part.jointPosition[0], turns, part.angularVelocity.magnitude, part.linearVelocity.magnitude, part.GetPointVelocity(part.worldCenterOfMass).magnitude, trulyTurns, trulyGoes, now, Held(one, true)));
                    }
                    turnedWas = part.transform.rotation; centreWas = part.worldCenterOfMass;
                    if (k == 0) { began = most = least = now; }
                    else { most = Mathf.Max(most, now); least = Mathf.Min(least, now); by += part.driveForce[0] * .5f * (turns + turnedBefore) * Dt; }
                    held = now; turnedBefore = turns;
                }
                if (way == 0) { freeMost = most / began; freeLeast = least / began; }
                else if (way == 1) { lost = began - held; counted = -by; }
                else endsAt = Mathf.Abs(turnedBefore);
                Debug.Log(string.Format(inv, "FACTS one part on a hinge set turning, {0}: it held {1:0.0} J after the first step, at most {2:0.000} and at least {3:0.000} of that over the next second, {4:0.0} J at the end; its drive was counted to take {5:0.0} J", way == 0 ? "at 40 rad/s, left alone" : way == 1 ? "at 40 rad/s, damped, at its limit above 29 rad/s" : "at 60 rad/s, left alone", began, most / began, least / began, held, -by));
                made.Remove(root.gameObject);
                Object.DestroyImmediate(root.gameObject);
            }
            Assert.That(freeMost, Is.LessThan(1.01f), "One part on one hinge, left alone, gained what nothing gave it.");
            Assert.That(freeLeast, Is.GreaterThan(.99f), "One part on one hinge, left alone, lost what nothing took.");
            Assert.That(counted, Is.EqualTo(lost).Within(.03f * lost), "What the drive was counted to take is not what the part lost.");
            Assert.That(endsAt, Is.EqualTo(Physics.defaultMaxAngularSpeed).Within(.5f), "Set turning faster than the project's greatest turning speed and left alone, the part did not come down to it.");
        }

        // The first fact of step 0 of the motion (Docs/Design/AnyBody/motion_design.md, K1's R4; November 10 and 11): whether
        // an ankle sprung stiffly enough to carry its torque through its drive (all its strength at 35 degrees from its
        // goal, damped by its strength over its top speed: the design's K5) holds a light boot under a body's weight at
        // the game's step. The standing balance's ankle is sprung with about 3 N m a radian and given its torque straight
        // on the boot (as it was built: "a stiff spring between a heavy body and a light boot does not hold at the game's
        // step", OwnKeeper), and a boot pushed so while it does not lie flat is what flies (K1).
        // The rig: a miner's whole weight but its boot, on a slide that only goes up and down (so nothing has to balance),
        // and under it that miner's boot on a ball joint where its ankle is. The boot's weight, turning weight, sole and
        // ankle (where it is, its strength, its top speed) are the game's, read from the three miners on November 11 (the
        // shove test's OWN_SHOVE_PARTS line); the links are set as the game sets its own.
        private struct Shod
        {
            public string Name;
            // (The whole body and the boot, kg; the ankle's strength, N m, and its top speed, rad/s.)
            public float Body, Boot, Has, Top;
            // (The boot's turning weight about its own lines, kg m2; where its ankle is in it; its sole's box: across,
            // high, long. Its own centre is the box's.)
            public Vector3 Turning, Joint, Sole;
            public float Spring => Has / (35 * Mathf.Deg2Rad);
            public float Damper => Has / Top;
        }

        private static readonly Shod[] Shods =
        {
            new Shod { Name = "Small", Body = 40.7f, Boot = 1.29f, Has = 61, Top = 19.9f, Turning = new Vector3(.0030f, .0037f, .0015f), Joint = new Vector3(0, .055f, -.006f), Sole = new Vector3(.100f, .060f, .157f) },
            new Shod { Name = "Long", Body = 58.5f, Boot = 1.83f, Has = 141, Top = 16.1f, Turning = new Vector3(.0059f, .0069f, .0021f), Joint = new Vector3(0, .055f, -.007f), Sole = new Vector3(.100f, .060f, .187f) },
            new Shod { Name = "Round", Body = 87.6f, Boot = 1.55f, Has = 154, Top = 18.5f, Turning = new Vector3(.0043f, .0051f, .0018f), Joint = new Vector3(0, .055f, -.007f), Sole = new Vector3(.100f, .060f, .172f) },
        };

        private void TakeAway() { foreach (var g in made) if (g != null) Object.DestroyImmediate(g); made.Clear(); }

        // Which solver the project is set to (ProjectSettings/DynamicsManager.asset): the fact is run under both.
        private static string SolverInForce()
        {
            foreach (string line in System.IO.File.ReadAllLines("ProjectSettings/DynamicsManager.asset"))
                if (line.Contains("m_SolverType")) return line.Trim().EndsWith("1") ? "TGS" : "PGS";
            return "solver not known";
        }

        // The boot made under its weight. The weight leant so many degrees to the side (about the line along the sole,
        // its right side up), the boot at its ankle's zero under it, so equally on its edge; or the boot made bent so
        // many degrees at its ankle the same way; its lowest corner so far above the ground; its weight on its slide or
        // held where it is. The ankle's zero is the boot square under the weight, however it is made.
        private (ArticulationBody weight, ArticulationBody boot, GroundTouch touch) Stood(Shod of, Vector3 at, float spring, float damper, float leant, float bent, float lifted, bool held)
        {
            var ground = new GameObject("Facts ground");
            ground.transform.position = at + .5f * Vector3.down;
            made.Add(ground);
            var surface = new PhysicsMaterial("Facts") { dynamicFriction = .7f, staticFriction = .8f, bounciness = 0, bounceCombine = PhysicsMaterialCombine.Minimum };
            var slab = ground.AddComponent<BoxCollider>();
            slab.size = new Vector3(6, 1, 6); slab.sharedMaterial = surface;
            var root = Root(at + 1.5f * Vector3.up);
            Quaternion leans = Quaternion.AngleAxis(leant, Vector3.forward), turned = leans * Quaternion.AngleAxis(bent, Vector3.forward);
            float lowest = float.MaxValue;
            for (int c = 0; c < 8; c++)
                lowest = Mathf.Min(lowest, (turned * Vector3.Scale(.5f * of.Sole, new Vector3((c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1, (c & 4) == 0 ? -1 : 1))).y);
            Vector3 bootAt = at + new Vector3(0, lifted - lowest, 0), ankle = bootAt + turned * of.Joint;
            // (Not held: two light carriers, one sliding across and one along, and the weight sliding up and down on
            // them, however it leans: so it goes freely every way and never turns. Pinned sideways, as the first run had
            // it, the ground's grip and the slide's lock held forces between them, and where the ground bore the boot
            // was not settled by the torque alone. The carriers' own weight is borne by their slides, not by the boot.)
            Transform under = root.transform;
            if (!held)
                for (int way = 0; way < 2; way++)
                {
                    var carrier = Link(under, way == 0 ? "Across" : "Along", ankle + .5f * Vector3.up, Quaternion.identity, .2f, Vector3.zero, Vector3.one);
                    Slides(carrier, way == 0 ? Quaternion.identity : Quaternion.Euler(0, -90, 0));
                    under = carrier.transform;
                }
            var weight = Link(under, "Weight", ankle + .5f * Vector3.up, leans, of.Body - of.Boot, Vector3.zero, Vector3.one);
            if (held) weight.jointType = ArticulationJointType.FixedJoint;
            else Slides(weight, Quaternion.Inverse(leans) * Quaternion.Euler(0, 0, 90));
            var boot = Link(weight.transform, "Boot", bootAt, turned, of.Boot, Vector3.zero, of.Turning);
            var box = boot.gameObject.AddComponent<BoxCollider>();
            box.size = of.Sole; box.sharedMaterial = surface;
            boot.jointType = ArticulationJointType.SphericalJoint;
            boot.anchorPosition = of.Joint; boot.anchorRotation = Quaternion.identity;
            boot.matchAnchors = false;
            boot.parentAnchorPosition = weight.transform.InverseTransformPoint(ankle); boot.parentAnchorRotation = Quaternion.identity;
            boot.twistLock = boot.swingYLock = boot.swingZLock = ArticulationDofLock.FreeMotion;
            var drive = new ArticulationDrive { stiffness = spring, damping = damper, forceLimit = of.Has, target = 0, driveType = ArticulationDriveType.Force };
            boot.xDrive = drive; boot.yDrive = drive; boot.zDrive = drive;
            // (As the game sets its own links and its own body: OwnBody.)
            foreach (var link in new[] { weight, boot }) { link.maxAngularVelocity = 20; link.maxDepenetrationVelocity = 1.5f; link.collisionDetectionMode = CollisionDetectionMode.Continuous; }
            root.solverIterations = 14; root.solverVelocityIterations = 4;
            // (Never asleep: a body asleep tells no touches.)
            root.sleepThreshold = 0;
            var touch = boot.gameObject.AddComponent<GroundTouch>();
            return (weight, boot, touch);
        }

        // (A link that slides freely along one line, the line its own x turned so, and nothing else.)
        private static void Slides(ArticulationBody link, Quaternion along)
        {
            link.jointType = ArticulationJointType.PrismaticJoint;
            link.anchorPosition = Vector3.zero; link.anchorRotation = along; link.matchAnchors = true;
            link.linearLockX = ArticulationDofLock.FreeMotion; link.linearLockY = ArticulationDofLock.LockedMotion; link.linearLockZ = ArticulationDofLock.LockedMotion;
            link.xDrive = new ArticulationDrive { stiffness = 0, damping = 0, forceLimit = 0, driveType = ArticulationDriveType.Force };
        }

        // (A boot made bent at its ankle: its joint's position written as made, before its first step, or the engine
        // reads it as at zero and snaps it there: MadeLyingStaysAsMade.)
        private static void AsMade(ArticulationBody boot, float bent)
        {
            var root = boot.transform.root.GetComponent<ArticulationBody>();
            var starts = new List<int>(); root.GetDofStartIndices(starts);
            var q = new List<float>(); root.GetJointPositions(q);
            Put(q, starts[boot.index], RotationVector(Quaternion.AngleAxis(bent, Vector3.forward)));
            root.SetJointPositions(q);
            root.SetJointVelocities(new List<float>(new float[q.Count]));
        }

        // What the weight and its boot hold (joules): their going, the boot's turning, and their height above the ground.
        // (How the boot's centre goes is worked out from the weight's going and the boot's turning about its ankle: what
        // the engine tells of a fast-turning part's own going is too great, ADrivesForceIsWhatItTakes.)
        private static float Holds(ArticulationBody weight, ArticulationBody boot, float ground)
        {
            float g = -Physics.gravity.y;
            Vector3 goes = weight.jointType == ArticulationJointType.FixedJoint ? Vector3.zero : weight.linearVelocity;
            Vector3 turns = boot.angularVelocity, bootGoes = goes + Vector3.Cross(turns, boot.worldCenterOfMass - boot.transform.TransformPoint(boot.anchorPosition));
            Vector3 inBoot = Quaternion.Inverse(boot.transform.rotation) * turns, inertia = boot.inertiaTensor;
            return .5f * weight.mass * goes.sqrMagnitude + weight.mass * g * (weight.worldCenterOfMass.y - ground)
                + .5f * boot.mass * bootGoes.sqrMagnitude + boot.mass * g * (boot.worldCenterOfMass.y - ground)
                + .5f * (inertia.x * inBoot.x * inBoot.x + inertia.y * inBoot.y * inBoot.y + inertia.z * inBoot.z * inBoot.z);
        }

        // How the rig reads, before anything is measured on it. Which way a ball joint's drive turns the boot when its
        // target is counted up, about the line across the sole (x) and about the line along it (z): +1 as Unity's own
        // turn about that line (about x the toe goes down, about z the right edge goes up), and how far it came to of the
        // 10 degrees asked, on a boot held in the air. Whether a boot made bent 22 degrees at its ankle stays as made,
        // and what its joint reads then (degrees about the line along the sole). And whether the ground's touches are
        // told within the step itself (then nothing has to wait a frame for them).
        // And what the drive tells of its force against a torque that is known: 13.3 N m put straight on a boot held in
        // the air about the line along its sole, its target left at zero: how far it gives way (the torque over the
        // spring, in degrees, if the spring is as set) and what force the drive tells then (13.3 if it tells true).
        private IEnumerator AnkleRead(System.Action<Vector2, Vector2, float, float, bool, float, float> done)
        {
            Vector3 at = new Vector3(500, 20, -900);
            Vector2 turns = Vector2.zero, cameTo = Vector2.zero;
            for (int axis = 0; axis < 2; axis++)
            {
                var (weight, boot, _) = Stood(Shods[0], at, Shods[0].Spring, Shods[0].Damper, 0, 0, .4f, true);
                weight.useGravity = false; boot.useGravity = false;
                yield return null;
                var d = axis == 0 ? boot.xDrive : boot.zDrive;
                d.target = 10;
                if (axis == 0) boot.xDrive = d; else boot.zDrive = d;
                Steps(100);
                if (axis == 0) turns.x = boot.transform.forward.y < 0 ? 1 : -1; else turns.y = boot.transform.right.y > 0 ? 1 : -1;
                cameTo[axis] = Quaternion.Angle(boot.transform.rotation, weight.transform.rotation);
                TakeAway();
            }
            var (upright, bentBoot, _) = Stood(Shods[0], at, 0, 0, 0, 22, .4f, true);
            upright.useGravity = false; bentBoot.useGravity = false;
            yield return null;
            AsMade(bentBoot, 22);
            Steps(5);
            float stays = Quaternion.Angle(bentBoot.transform.rotation, upright.transform.rotation) * (bentBoot.transform.right.y > 0 ? 1 : -1);
            float reads = bentBoot.jointPosition[2] * Mathf.Rad2Deg;
            TakeAway();
            var (_, _, touch) = Stood(Shods[0], at, Shods[0].Spring, Shods[0].Damper, 0, 0, 0, false);
            yield return null;
            Steps(3);
            touch.Clear();
            Physics.Simulate(Dt);
            bool within = touch.Up > 0;
            TakeAway();
            var (fixedWeight, pushed, _) = Stood(Shods[0], at, Shods[0].Spring, Shods[0].Damper, 0, 0, .4f, true);
            fixedWeight.useGravity = false; pushed.useGravity = false;
            yield return null;
            for (int k = 0; k < 100; k++) { pushed.AddTorque(pushed.transform.forward * 13.3f); Physics.Simulate(Dt); }
            float gaveWay = Quaternion.Angle(pushed.transform.rotation, fixedWeight.transform.rotation), tells = 0;
            for (int d = 0; d < pushed.dofCount; d++) tells += pushed.driveForce[d] * pushed.driveForce[d];
            tells = Mathf.Sqrt(tells);
            TakeAway();
            done(turns, cameTo, stays, reads, within, gaveWay, tells);
        }

        // What a boot did, stood, hung or dropped for three seconds. After the first half second: how far from flat on
        // the ground it tipped at most (degrees), how fast it turned at most (rad/s), how far the weight moved (mm), what
        // its drive gave on average (N m), where the ground bore it on average from the point under its ankle (mm across,
        // mm along) and with how much (N). Over all of it: how fast it turned at most; the most its drive gave it in one
        // step (J); the most the two held more after a step than their joint and any torque put on them gave (J), and the
        // sum of that. At the end: how fast it turned at most in the last half second, how far it was from flat on the
        // ground, and how far from its ankle's zero.
        private struct Standing
        {
            public float Tipped, TurnedLate, Moved, Slid, Gave, Bore, TurnedEver, WorkMost, ElseMost, ElseSum, EndsTurning, EndsTipped, EndsBent, EndsMoved;
            public Vector2 Where;
        }

        // The boot's ankle sprung and damped as told, asked for a torque towards the toe (about the line across the sole)
        // and one sideways (about the line along it, its right edge up), either through its drive's target (the torque
        // over the spring: the design's way) or put straight on the boot with its opposite on the weight (the standing
        // balance's way).
        // (The solver's turns: of place and of speed; the game's own are 14 and 4. The drive's work is counted from the
        // joint's real change of position: the joint speed the engine tells of a part at rest on the ground is not
        // motion, the first run.)
        private IEnumerator Shoed(Shod of, float spring, float damper, float toToe, float sideways, bool straight, float leant, float bent, float lifted, bool held, Vector2 turns, bool within, int turnsOfPlace, int turnsOfSpeed, System.Action<Standing> done)
        {
            Vector3 at = new Vector3(500, 20, -900);
            var (weight, boot, touch) = Stood(of, at, spring, damper, leant, bent, lifted, held);
            var whole = boot.transform.root.GetComponent<ArticulationBody>();
            whole.solverIterations = turnsOfPlace; whole.solverVelocityIterations = turnsOfSpeed;
            Vector3 placeAt = Vector3.zero;
            yield return null;
            if (bent != 0) AsMade(boot, bent);
            if (!straight)
            {
                var d = boot.xDrive; d.target = turns.x * toToe / spring * Mathf.Rad2Deg; boot.xDrive = d;
                d = boot.zDrive; d.target = turns.y * sideways / spring * Mathf.Rad2Deg; boot.zDrive = d;
            }
            var s = new Standing();
            float heightAt = 0, bore = 0, heldBefore = Holds(weight, boot, at.y);
            Vector2 where = Vector2.zero;
            Quaternion lateAs = Quaternion.identity;
            int counted = 0, dofs = boot.dofCount;
            var jointBefore = new float[3];
            for (int k = 0; k < 150; k++)
            {
                Vector3 torque = Vector3.zero, turnedBefore = boot.angularVelocity;
                for (int d = 0; d < dofs; d++) jointBefore[d] = boot.jointPosition[d];
                if (straight)
                {
                    torque = boot.transform.right * toToe + boot.transform.forward * sideways;
                    boot.AddTorque(torque); weight.AddTorque(-torque);
                }
                touch.Clear();
                Physics.Simulate(Dt);
                if (!within) yield return null;
                float holds = Holds(weight, boot, at.y), byDrive = 0, force = 0;
                for (int d = 0; d < dofs; d++)
                {
                    byDrive += boot.driveForce[d] * (boot.jointPosition[d] - jointBefore[d]);
                    force += boot.driveForce[d] * boot.driveForce[d];
                }
                float byTorque = Vector3.Dot(torque, .5f * (turnedBefore + boot.angularVelocity)) * Dt;
                float other = holds - heldBefore - byDrive - byTorque, turning = boot.angularVelocity.magnitude;
                heldBefore = holds;
                s.TurnedEver = Mathf.Max(s.TurnedEver, turning);
                s.WorkMost = Mathf.Max(s.WorkMost, byDrive);
                s.ElseMost = Mathf.Max(s.ElseMost, other); s.ElseSum += other;
                if (k == 24) { heightAt = weight.transform.position.y; placeAt = weight.transform.position; }
                if (k >= 25)
                {
                    s.Slid = Mathf.Max(s.Slid, Vector3.ProjectOnPlane(weight.transform.position - placeAt, Vector3.up).magnitude * 1000);
                    s.Tipped = Mathf.Max(s.Tipped, Vector3.Angle(boot.transform.up, Vector3.up));
                    s.TurnedLate = Mathf.Max(s.TurnedLate, turning);
                    s.Moved = Mathf.Max(s.Moved, Mathf.Abs(weight.transform.position.y - heightAt) * 1000);
                    s.Gave += Mathf.Sqrt(force);
                    if (touch.Up > 1e-6f)
                    {
                        Vector3 local = boot.transform.InverseTransformPoint(touch.Where / touch.Up) - of.Joint;
                        where += new Vector2(local.x, local.z) * touch.Up; bore += touch.Up;
                    }
                    counted++;
                }
                // (Rest is read from where the boot is: how far it turns, in its last half second, from where it was.)
                if (k == 124) lateAs = boot.transform.rotation;
                if (k >= 125) { s.EndsTurning = Mathf.Max(s.EndsTurning, turning); s.EndsMoved = Mathf.Max(s.EndsMoved, Quaternion.Angle(lateAs, boot.transform.rotation)); }
            }
            s.Gave /= counted; s.Bore = bore / counted / Dt;
            s.Where = bore > 0 ? where / bore * 1000 : new Vector2(float.NaN, float.NaN);
            s.EndsTipped = Vector3.Angle(boot.transform.up, Vector3.up);
            s.EndsBent = Quaternion.Angle(boot.transform.rotation, weight.transform.rotation);
            TakeAway();
            done(s);
        }

        // The bar (November 12: reshaped after both runs and the engine's own source were read; the first bar, set on
        // November 11 before the run, and what of it was wrong, are in Docs/Validation.md). It says what holds under the
        // game's solver, PGS. Under TGS this test fails and is meant to: there a boot flies at landings.
        // The law (the design's ankle at 14 and 4, 28 and 4, 14 and 18, 60 and 4 turns; flat, asked a torque the sole
        // can bear): what the drive gives lies between what the engine's source says a part held still gets and the
        // whole of what was asked (a tenth over is allowed: the spring's answer to the boot's own tip is in the
        // reading; 2% over was found); and towards the toe, where the long sole holds the boot still, it is the held
        // part's within an eighth (9% over was found). (PGS: the share 1 - (1 - x)^n, x = 1 / (1 + dt (dt k + c) / I), I the boot's turning weight
        // about its ankle, n the turns of place and of speed together. TGS: (place + speed) / place over
        // 1 + h (h k + c) / I, h = dt / place.)
        // Where the sole can bear what is asked (kind 0), after the first half second: the boot tips no more than
        // 6 degrees (under PGS it sits 1.2 to 4.7 degrees off flat, and the same way every time; less with more turns
        // of place, 0.1 to 0.8 at 60, and no less with more turns of speed: what the solver's turns leave of the
        // ground's four pushes on a light sole under a heavy load, as it reads; not shown) and turns no faster than
        // 1 rad/s; the weight moves no more than 2 mm; asked for nothing, the drive
        // gives no more than a tenth of its strength; and the ground bears the boot within 10 mm of where the torque its
        // drive really gave puts it (that torque over the weight, from under the ankle).
        // Where it cannot (kind 1: more than the sole bears, on an edge under a leaning weight, dropped with all its
        // strength asked): it comes to rest (in its last half second it turns no more than 2 degrees from where it was:
        // the speed the engine tells of a part at rest on the ground is not motion) and is not turned over (asked with
        // all its strength onto its edge it ends on its side, 84 to 90 degrees from flat: over 100 is turned over).
        // (Where it rests is not barred: on an edge the ground's grip can hold it anywhere its spring and the weight
        // allow.)
        // Held in the air (kind 2): it comes to rest, and, asked about one line, within 2 degrees of its target.
        // In every case: the boot never turns faster than its ankle's top speed; its drive never gives more in a step
        // than the spring's whole stroke holds (half its strength times 35 degrees); over the three seconds the two gain
        // not half a joule in all that nothing gave them; and in one step they hold no more beyond the count than the
        // body's weight lifted 5 mm, a joule at least (at a landing the ground pushes a sunk sole back out: 2.0 to 3.5
        // mm's worth was found, and every sum over a run came out a loss).
        // Barred: each miner's ankle as the design has it (and the law at the other counts of turns). Told beside it,
        // not barred: the same ankle three and ten times as stiff (how near the edge the design's is), and on Small the
        // standing balance's own ankle with the torque put straight on it (what the fault was found in).
        [UnityTest]
        [Timeout(1800000)]
        public IEnumerator AStifflySprungAnkleHoldsALightBoot()
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            string solver = SolverInForce();
            bool tgs = solver == "TGS";
            float g = -Physics.gravity.y;
            var failed = new List<string>();
            Vector2 turns = default, cameTo = default;
            float stays = 0, reads = 0, gaveWay = 0, tells = 0;
            bool within = false;
            yield return AnkleRead((way, came, stayed, read, told, gave, force) => { turns = way; cameTo = came; stays = stayed; reads = read; within = told; gaveWay = gave; tells = force; });
            Debug.Log(string.Format(inv, "FACTS R4 ({0}) the rig, its drive against a known torque: 13.3 N m put straight on a boot held in the air, its target at zero: it gives way {1:0.00} degrees (the torque over the spring is {2:0.00}) and its drive tells {3:0.00} N m",
                solver, gaveWay, 13.3f / Shods[0].Spring * Mathf.Rad2Deg, tells));
            if (Mathf.Abs(tells - 13.3f) > .05f * 13.3f) failed.Add(string.Format(inv, "the rig: against 13.3 N m its drive told {0:0.00}", tells));
            // (Under TGS a free spring acts (place + speed) / place times as strong as set, here 18 / 14: the engine's
            // source, and the second run's 5.92 degrees.)
            if (Mathf.Abs(gaveWay - 13.3f / Shods[0].Spring * Mathf.Rad2Deg * (tgs ? 14f / 18 : 1)) > .4f) failed.Add(string.Format(inv, "the rig: under 13.3 N m the boot gave way {0:0.00} degrees", gaveWay));
            Debug.Log(string.Format(inv, "FACTS R4 ({0}) the rig: a ball joint's drive, its target counted up by 10 degrees: about the line across the sole the toe goes {1} and it comes to {2:0.00} degrees; about the line along it the right edge goes {3} and it comes to {4:0.00} degrees. A boot made bent 22 degrees at its ankle (its right edge up), its joint written as made, nothing driving it: after five steps it lies {5:0.00} degrees from its zero and its joint reads {6:0.00} degrees. The ground's touches are {7} within the step itself",
                solver, turns.x > 0 ? "down" : "up", cameTo.x, turns.y > 0 ? "up" : "down", cameTo.y, stays, reads, within ? "told" : "not told"));
            if (Mathf.Abs(cameTo.x - 10) > .5f || Mathf.Abs(cameTo.y - 10) > .5f) failed.Add(string.Format(inv, "the rig: a target of 10 degrees brought the boot to {0:0.00} and {1:0.00} degrees", cameTo.x, cameTo.y));
            if (Mathf.Abs(stays - 22) > .5f) failed.Add(string.Format(inv, "the rig: a boot made bent 22 degrees lay {0:0.00} degrees from its zero after five steps", stays));
            foreach (var of in Shods)
            {
                float bears = of.Body * g, across = bears * .5f * of.Sole.x, toToe = bears * (.5f * of.Sole.z - of.Joint.z), stroke = .5f * of.Has * 35 * Mathf.Deg2Rad;
                Debug.Log(string.Format(inv, "FACTS R4 ({0}) {1}'s boot: {2:0.00} kg under {3:0.0} kg; its sole bears {4:0.0} N m sideways and {5:0.0} towards the toe, its ankle has {6:0}; as the design has it, sprung {7:0} N m a radian and damped {8:0.00} N m s; top speed {9:0.0} rad/s; the spring's stroke holds {10:0.0} J",
                    solver, of.Name, of.Boot, of.Body - of.Boot, across, toToe, of.Has, of.Spring, of.Damper, of.Top, stroke));
                // (What it is called; asked towards the toe and sideways, N m; the weight leant and the boot bent,
                // degrees; lifted, m; held in the air; its kind; whether the standing balance's ankle is told beside it.)
                var cases = new (string called, float toToe, float sideways, float leant, float bent, float lifted, bool held, int kind, bool also)[]
                {
                    ("flat, nothing asked", 0, 0, 0, 0, 0, false, 0, false),
                    ("flat, 2/3 of what the sole bears sideways", 0, across * 2 / 3, 0, 0, 0, false, 0, false),
                    ("flat, 2/3 of what the sole bears towards the toe", toToe * 2 / 3, 0, 0, 0, 0, false, 0, false),
                    ("dropped flat from 2 cm, nothing asked", 0, 0, 0, 0, .02f, false, 0, false),
                    ("made bent 22 degrees at its ankle under an upright weight, nothing asked", 0, 0, 0, 22, 0, false, 0, false),
                    ("flat, all its strength sideways", 0, of.Has, 0, 0, 0, false, 1, true),
                    ("on its edge under a weight leant 22 degrees, nothing asked", 0, 0, 22, 0, 0, false, 1, false),
                    ("on its edge under a weight leant 22 degrees, all its strength further onto the edge", 0, of.Has, 22, 0, 0, false, 1, true),
                    ("on its edge under a weight leant 22 degrees, all its strength towards the toe", of.Has, 0, 22, 0, 0, false, 1, true),
                    ("dropped flat from 5 cm, all its strength towards the toe", of.Has, 0, 0, 0, .05f, false, 1, true),
                    ("dropped from 5 cm under a weight leant 22 degrees, all its strength towards the toe", of.Has, 0, 22, 0, .05f, false, 1, true),
                    ("held 34 cm off the ground, all its strength towards the toe", of.Has, 0, 0, 0, .34f, true, 2, true),
                    ("held 34 cm off the ground, all its strength towards the toe and sideways at once", of.Has * .7071f, of.Has * .7071f, 0, 0, .34f, true, 2, false),
                };
                // (Ankles 4 to 6: the design's own with other turns of the solver, on the three flat cases only: whether
                // what arrives follows the first run's law, 1 - (1 - x)^n with n the turns of place and of speed together.)
                for (int ankle = 0; ankle < 7; ankle++)
                {
                    if (ankle == 3 && of.Name != "Small") continue;
                    float spring = ankle == 3 ? 3 : (ankle == 1 ? 3 : ankle == 2 ? 10 : 1) * of.Spring, damper = ankle == 3 ? .07f : of.Damper;
                    int ofPlace = ankle == 4 ? 28 : ankle == 6 ? 60 : 14, ofSpeed = ankle == 5 ? 18 : 4;
                    string ankleCalled = ankle == 0 ? "design" : ankle == 1 ? "3x stiff" : ankle == 2 ? "10x stiff" : ankle == 3 ? "balance's, straight" : "design, " + ofPlace + " and " + ofSpeed + " turns";
                    for (int n = 0; n < cases.Length; n++)
                    {
                        var c = cases[n];
                        if ((ankle == 1 || ankle == 2) && c.kind == 2) continue;
                        if (ankle == 3 && !c.also) continue;
                        if (ankle >= 4 && n >= 3) continue;
                        Standing s = default;
                        yield return Shoed(of, spring, damper, c.toToe, c.sideways, ankle == 3, c.leant, c.bent, c.lifted, c.held, turns, within, ofPlace, ofSpeed, r => s = r);
                        float asked = Mathf.Sqrt(c.toToe * c.toToe + c.sideways * c.sideways);
                        Vector2 put = new Vector2(-c.sideways / bears, c.toToe / bears) * 1000;
                        // (The first run's law, for a part the ground holds still: the boot's turning weight about its
                        // ankle, about the line asked.)
                        float about = c.toToe != 0 ? of.Turning.x + of.Boot * (of.Joint.y * of.Joint.y + of.Joint.z * of.Joint.z) : of.Turning.z + of.Boot * (of.Joint.x * of.Joint.x + of.Joint.y * of.Joint.y);
                        // (What the engine's source says a part held still gets. PGS: at each turn, of place or of speed,
                        // the drive's sum goes the share x of the way to what the spring says, from nothing each step.
                        // TGS: each substep gives its own impulse and each turn of speed one more; and it takes the
                        // turns of speed over four as turns of place: its rows at 14 and 18 are those at 28 and 4.)
                        int place = tgs ? ofPlace + Mathf.Max(0, ofSpeed - 4) : ofPlace, speed = tgs ? Mathf.Min(ofSpeed, 4) : ofSpeed;
                        float inStep = Dt / place;
                        float law = asked * (tgs
                            ? (place + speed) / (float)place / (1 + inStep * (inStep * spring + damper) / about)
                            : 1 - Mathf.Pow(1 - 1 / (1 + Dt * (Dt * spring + damper) / about), place + speed));
                        Debug.Log(string.Format(inv, "FACTS R4 ({0}) {1} | {2} | {3} | asked {4:0.0} N m | after 0.5 s: tipped {5:0.00} deg, turned {6:0.00} rad/s, weight moved {7:0.00} mm, drive gave {8:0.0} N m, ground bore {9:0} N at {10:0.0} across {11:0.0} along mm (the torque puts it at {12:0.0}, {13:0.0}) | over all: turned at most {14:0.0} rad/s, drive gave at most {15:0.00} J a step, held at most {16:0.00} J more than given in a step and {17:0.00} J in sum | at the end: turning {18:0.00} rad/s, {19:0.0} deg from flat on the ground, {20:0.0} deg from its ankle's zero | weight slid {21:0.0} mm sideways | held still, the law gives {22:0.0} N m | in its last half second it turned {23:0.00} deg",
                            solver, of.Name, ankleCalled, c.called, asked, s.Tipped, s.TurnedLate, s.Moved, s.Gave, s.Bore, s.Where.x, s.Where.y, put.x, put.y, s.TurnedEver, s.WorkMost, s.ElseMost, s.ElseSum, s.EndsTurning, s.EndsTipped, s.EndsBent, s.Slid, law, s.EndsMoved));
                        string what = of.Name + ", " + ankleCalled + ", " + c.called + ": ";
                        if ((ankle == 0 || ankle >= 4) && c.kind == 0 && asked > 0)
                        {
                            if (s.Gave < .97f * law || s.Gave > 1.1f * asked) failed.Add(what + string.Format(inv, "its drive gave {0:0.0} N m: not between the held part's {1:0.0} and the {2:0.0} asked", s.Gave, law, asked));
                            else if (c.toToe != 0 && s.Gave > 1.125f * law) failed.Add(what + string.Format(inv, "its drive gave {0:0.0} N m, more than an eighth over the held part's {1:0.0}", s.Gave, law));
                        }
                        if (ankle != 0) continue;
                        if (c.kind == 0)
                        {
                            if (s.Tipped > 6) failed.Add(what + string.Format(inv, "it tipped {0:0.00} degrees", s.Tipped));
                            if (s.TurnedLate > 1) failed.Add(what + string.Format(inv, "it turned at {0:0.00} rad/s after the first half second", s.TurnedLate));
                            if (s.Moved > 2) failed.Add(what + string.Format(inv, "the weight moved {0:0.00} mm", s.Moved));
                            if (asked == 0 && s.Gave > .1f * of.Has) failed.Add(what + string.Format(inv, "asked for nothing, its drive gave {0:0.0} N m", s.Gave));
                            Vector2 given = asked > 0 ? put * (s.Gave / asked) : Vector2.zero;
                            if (!((s.Where - given).magnitude <= 10)) failed.Add(what + string.Format(inv, "the ground bore it at {0:0.0}, {1:0.0} mm, not near {2:0.0}, {3:0.0}, where what its drive gave puts it", s.Where.x, s.Where.y, given.x, given.y));
                        }
                        else
                        {
                            if (s.EndsMoved > 2) failed.Add(what + string.Format(inv, "it turned {0:0.00} degrees in its last half second", s.EndsMoved));
                            if (c.kind == 1 && s.EndsTipped > 100) failed.Add(what + string.Format(inv, "it ended turned over, {0:0.0} degrees from flat", s.EndsTipped));
                            if (c.kind == 2 && c.sideways == 0 && Mathf.Abs(s.EndsBent - 35) > 2) failed.Add(what + string.Format(inv, "it ended {0:0.0} degrees from its zero, not at its target of 35", s.EndsBent));
                        }
                        if (s.TurnedEver > of.Top) failed.Add(what + string.Format(inv, "the boot turned at {0:0.0} rad/s, faster than its ankle's {1:0.0}", s.TurnedEver, of.Top));
                        if (s.WorkMost > stroke) failed.Add(what + string.Format(inv, "its drive gave {0:0.0} J in a step, more than the stroke's {1:0.0}", s.WorkMost, stroke));
                        float landing = Mathf.Max(1, bears * .005f);
                        if (s.ElseMost > landing) failed.Add(what + string.Format(inv, "the two held {0:0.00} J more after a step than was given them (a landing is allowed {1:0.00})", s.ElseMost, landing));
                        if (s.ElseSum > .5f) failed.Add(what + string.Format(inv, "the two gained {0:0.00} J in all that nothing gave them", s.ElseSum));
                    }
                }
            }
            Debug.Log("FACTS R4 (" + solver + ") the stiff ankle: " + (failed.Count == 0 ? "every case within its bar" : failed.Count + " not within the bar: " + string.Join(" | ", failed)));
            Assert.That(failed, Is.Empty, "A stiffly sprung ankle did not hold a light boot under a body's weight (" + solver + "): " + string.Join(" | ", failed));
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

        // ---- The rest of step 0 (Docs/Handoff.md, "Next steps" item 2), each bar and decision set here before any run. ----

        private const float EFull = 35 * Mathf.Deg2Rad;

        // One part of a miner's leg as parts_r4.log has it: its weight, its turning weight about its centre, its strength S
        // (N m), where its joint sits in its own frame, and its top speed (rad/s); joint_model 2.3's drive k = S/eFull, c = S/top.
        private struct LegPart
        {
            public float Kg, Has, Top; public Vector3 Turning, Joint;
            public float Spring => Has / EFull; public float Damper => Has / Top;
            public LegPart(float kg, Vector3 turning, float has, Vector3 joint, float top) { Kg = kg; Turning = turning; Has = has; Joint = joint; Top = top; }
        }

        private struct Leg
        {
            public string Name; public float Body; public LegPart Thigh, Shin, Boot; public Vector3 Sole;
            public float Mass => Thigh.Kg + Shin.Kg + Boot.Kg;
            public float Length => 2 * (Mathf.Abs(Thigh.Joint.y) + Mathf.Abs(Shin.Joint.y));
        }

        private static readonly Leg[] Legs =
        {
            new Leg { Name = "Small", Body = 40.7f, Sole = new Vector3(.100f, .060f, .157f),
                Thigh = new LegPart(2.38f, new Vector3(.0131f, .0029f, .0131f), 130, new Vector3(0, -.136f, 0), 24.9f),
                Shin = new LegPart(1.35f, new Vector3(.0074f, .0011f, .0074f), 87, new Vector3(0, -.136f, 0), 29.9f),
                Boot = new LegPart(1.29f, new Vector3(.0030f, .0037f, .0015f), 61, new Vector3(0, .055f, -.006f), 19.9f) },
            new Leg { Name = "Long", Body = 58.5f, Sole = new Vector3(.100f, .060f, .187f),
                Thigh = new LegPart(4.87f, new Vector3(.0701f, .0077f, .0701f), 303, new Vector3(0, -.220f, 0), 20.1f),
                Shin = new LegPart(2.74f, new Vector3(.0400f, .0028f, .0400f), 202, new Vector3(0, -.220f, 0), 24.2f),
                Boot = new LegPart(1.83f, new Vector3(.0059f, .0069f, .0021f), 141, new Vector3(0, .055f, -.007f), 16.1f) },
            new Leg { Name = "Round", Body = 87.6f, Sole = new Vector3(.100f, .060f, .172f),
                Thigh = new LegPart(5.60f, new Vector3(.0430f, .0137f, .0430f), 331, new Vector3(0, -.160f, 0), 23.2f),
                Shin = new LegPart(3.14f, new Vector3(.0239f, .0050f, .0239f), 220, new Vector3(0, -.160f, 0), 27.8f),
                Boot = new LegPart(1.55f, new Vector3(.0043f, .0051f, .0018f), 154, new Vector3(0, .055f, -.007f), 18.5f) },
        };

        // The hips' stand-in (said so): the body less the leg; Small's turning weight (1.2, 0.4, 1.2) scaled by that weight and
        // by the square of the leg's length; and (decided here) FloatingLeg's centre 0.25 above the hip's line and hip 0.09
        // aside, both scaled by the leg's length.
        private static float LegScale(Leg of) => of.Length / Legs[0].Length;
        private static Vector3 HipsTurning(Leg of) => new Vector3(1.2f, .4f, 1.2f) * ((of.Body - of.Mass) / (Legs[0].Body - Legs[0].Mass) * LegScale(of) * LegScale(of));

        private static string Three(Vector3 v) => $"({v.x:0.000}, {v.y:0.000}, {v.z:0.000})";
        private static ArticulationDrive DriveOf(float spring, float damper, float limit, float target) => new ArticulationDrive { stiffness = spring, damping = damper, forceLimit = limit, target = target, driveType = ArticulationDriveType.Force };

        // The joint's turn: from the parent's anchor to the child's, in the parent anchor's axes; and the joint's reading.
        private static Quaternion JointTurn(ArticulationBody link)
        {
            var parent = link.transform.parent.GetComponentInParent<ArticulationBody>();
            return Quaternion.Inverse(parent.transform.rotation * link.parentAnchorRotation) * (link.transform.rotation * link.anchorRotation);
        }

        private static Vector3 Reading(ArticulationBody link) { var jp = link.jointPosition; return new Vector3(jp[0], jp[1], jp[2]); }

        // A free ball joint on a fixed root, gravity off, no drives, as Spun's.
        private ArticulationBody FreeBall(Vector3 at, string name, float kilograms, Vector3 centre, Vector3 inertia)
        {
            var root = Root(at);
            var link = Link(root.transform, name, at, Quaternion.identity, kilograms, centre, inertia);
            link.useGravity = false;
            link.jointType = ArticulationJointType.SphericalJoint;
            link.twistLock = ArticulationDofLock.FreeMotion; link.swingYLock = ArticulationDofLock.FreeMotion; link.swingZLock = ArticulationDofLock.FreeMotion;
            link.anchorPosition = Vector3.zero; link.anchorRotation = Quaternion.identity; link.matchAnchors = true;
            return link;
        }

        // K3, the reading about two axes. A link with the same turning weight about every line on a free ball joint, moved
        // only by setting its joint's speeds each step: (a) spun about an oblique line (x and y together) to 175 degrees;
        // (b) 60 degrees about x, then 60 about y; (c) swung 60 degrees about y, then the swing's direction taken once round
        // and back. Bar: at every step the reading is the rotation vector of the joint's turn (the parent's anchor to the
        // child's, signed as MadeLyingStaysAsMade fixes it) within 0.02 rad (0.05 along the spin of (a)): a function of the
        // pose, not the sum of the turns ((b) would read 1.05, 1.05, 0 summed; (c) would gather half a turn about x). If it
        // fails, joint_model 1.1 is wrong for two axes. (b) also tells in whose axes the joint's speeds are counted (x then
        // the child's own y gives (0.946, 0.946, 0.546); x then the parent's y gives (0.946, 0.946, -0.546)); (c) follows its
        // asked path in those axes, each step's speed the turn from where it is to where the path is next.
        [UnityTest]
        public IEnumerator ABallJointReadsItsPoseAboutTwoAxes()
        {
            var failed = new List<string>();
            var even = new Vector3(.01f, .01f, .01f);

            var link = FreeBall(new Vector3(40, 50, 900), "Read a", 1, Vector3.zero, even);
            yield return null;
            Vector3 line = new Vector3(1, 1, 0).normalized, was = Vector3.zero, summed = Vector3.zero;
            int n = 150; float speed = 175 * Mathf.Deg2Rad / (n * Dt), across = 0, along = 0, lagged = 0, worst = 0;
            for (int k = 0; k < n; k++)
            {
                link.jointVelocity = new ArticulationReducedSpace(speed * line.x, speed * line.y, speed * line.z);
                Physics.Simulate(Dt);
                Vector3 truth = RotationVector(JointTurn(link)), read = Reading(link), off = read - truth;
                float on = Vector3.Dot(off, line);
                along = Mathf.Max(along, Mathf.Abs(on)); across = Mathf.Max(across, (off - on * line).magnitude);
                lagged = Mathf.Max(lagged, (read - was).magnitude); was = truth;
            }
            Debug.Log($"FACTS ball reading (a), spun about (1, 1, 0) to {Quaternion.Angle(Quaternion.identity, JointTurn(link)):0.0} degrees: off the pose's rotation vector at most {across:0.0000} rad across the spin and {along:0.0000} along it (against the pose a step before, {lagged:0.0000}); at the end it read {Three(Reading(link))} for {Three(RotationVector(JointTurn(link)))}");
            if (across > .02f || along > .05f) failed.Add($"(a) off {across:0.000} across the spin, {along:0.000} along it");

            TakeAway();
            link = FreeBall(new Vector3(42, 50, 900), "Read b", 1, Vector3.zero, even);
            yield return null;
            n = 105; speed = 60 * Mathf.Deg2Rad / (n * Dt); lagged = 0; was = Vector3.zero;
            for (int k = 0; k < 2 * n; k++)
            {
                Vector3 v = k < n ? new Vector3(speed, 0, 0) : new Vector3(0, speed, 0);
                link.jointVelocity = new ArticulationReducedSpace(v.x, v.y, v.z);
                Physics.Simulate(Dt);
                summed += v * Dt;
                Vector3 truth = RotationVector(JointTurn(link)), read = Reading(link);
                worst = Mathf.Max(worst, (read - truth).magnitude); lagged = Mathf.Max(lagged, (read - was).magnitude); was = truth;
            }
            Quaternion ownY = Quaternion.AngleAxis(60, Vector3.right) * Quaternion.AngleAxis(60, Vector3.up), parentsY = Quaternion.AngleAxis(60, Vector3.up) * Quaternion.AngleAxis(60, Vector3.right);
            float offOwn = Quaternion.Angle(JointTurn(link), ownY), offParents = Quaternion.Angle(JointTurn(link), parentsY);
            bool childAxes = offOwn <= offParents;
            Debug.Log($"FACTS ball reading (b), 60 degrees about x and then 60 about y: the joint's speeds are counted in the {(childAxes ? "child's own" : "parent's")} axes (the pose {offOwn:0.00} degrees from x then its own y, {offParents:0.00} from x then the parent's y); off the pose's rotation vector at most {worst:0.0000} rad (against the pose a step before, {lagged:0.0000}); at the end it read {Three(Reading(link))} for {Three(RotationVector(JointTurn(link)))}, the turns summed {Three(summed)}");
            if (worst > .02f) failed.Add($"(b) off {worst:0.000}");

            TakeAway();
            link = FreeBall(new Vector3(44, 50, 900), "Read c", 1, Vector3.zero, even);
            yield return null;
            int round = 600; float strayed = 0; worst = 0; lagged = 0; was = Vector3.zero; summed = Vector3.zero;
            for (int k = 0; k < n + round; k++)
            {
                float f = 2 * Mathf.PI * (k + 1 - n) / round;
                Quaternion next = k < n ? Quaternion.AngleAxis(60f * (k + 1) / n, Vector3.up) : Quaternion.AngleAxis(60, new Vector3(0, Mathf.Cos(f), Mathf.Sin(f)));
                Quaternion now = JointTurn(link);
                Vector3 v = (childAxes ? RotationVector(Quaternion.Inverse(now) * next) : RotationVector(next * Quaternion.Inverse(now))) / Dt;
                link.jointVelocity = new ArticulationReducedSpace(v.x, v.y, v.z);
                Physics.Simulate(Dt);
                summed += v * Dt;
                Vector3 truth = RotationVector(JointTurn(link)), read = Reading(link);
                worst = Mathf.Max(worst, (read - truth).magnitude); lagged = Mathf.Max(lagged, (read - was).magnitude); was = truth;
                strayed = Mathf.Max(strayed, Quaternion.Angle(JointTurn(link), next));
            }
            Debug.Log($"FACTS ball reading (c), swung 60 degrees and its swing's direction taken once round: it strayed at most {strayed:0.00} degrees from the asked path; off the pose's rotation vector at most {worst:0.0000} rad (against the pose a step before, {lagged:0.0000}); at the end it read {Three(Reading(link))} for {Three(RotationVector(JointTurn(link)))}, the turns summed {Three(summed)}");
            if (worst > .02f) failed.Add($"(c) off {worst:0.000}");
            Assert.IsEmpty(failed, "the ball joint's reading is not its pose's rotation vector: " + string.Join("; ", failed));
        }

        // K3, a drive across a half turn: Small's thigh on a free ball joint, gravity off, spun to 170 degrees about x and
        // stopped; then the design's drive (k = 130/eFull = 212.8, c = 130/24.9 = 5.22, limit 130) written by K3's rule each
        // step: the reading plus the shortest turn from the pose to the goal in the child anchor's axes, at most eFull (no
        // load). Goals: 20 degrees on about x; 30 degrees about its own y. Bar: within 1 degree of the goal after a second,
        // never more than 25 degrees from the start (45 for the second). Control: the goal written as its own rotation vector
        // from zero must go the long way (more than 90 degrees from the start), or the fact proves nothing. Decided before the
        // run: that bar is on the first goal; the second goal's own rotation vector lies near the reading (about 44 degrees
        // off it, mostly about z), so its control need not go the long way, and it is logged, not barred.
        [UnityTest]
        public IEnumerator ADriveWrittenByTheShortestTurnCrossesAHalfTurn()
        {
            var failed = new List<string>();
            float spring = 130 / EFull, damper = 130 / 24.9f;
            for (int c = 0; c < 4; c++)
            {
                bool aboutY = c % 2 == 1, control = c >= 2;
                TakeAway();
                var thigh = FreeBall(new Vector3(50 + 2 * c, 50, 900), "Driven thigh", 2.38f, new Vector3(0, -.13f, 0), new Vector3(.0131f, .0029f, .0131f));
                yield return null;
                int n = 150; float speed = 170 * Mathf.Deg2Rad / (n * Dt);
                for (int k = 0; k < n; k++) { thigh.jointVelocity = new ArticulationReducedSpace(speed, 0, 0); Physics.Simulate(Dt); }
                thigh.jointVelocity = new ArticulationReducedSpace(0, 0, 0);
                Quaternion start = JointTurn(thigh), goal = start * Quaternion.AngleAxis(aboutY ? 30 : 20, aboutY ? Vector3.up : Vector3.right);
                Vector3 began = Reading(thigh), asItsOwn = RotationVector(goal) * Mathf.Rad2Deg;
                float farthest = 0;
                for (int k = 0; k < 50; k++)
                {
                    Vector3 target = asItsOwn;
                    if (!control)
                    {
                        Vector3 shortest = RotationVector(Quaternion.Inverse(JointTurn(thigh)) * goal);
                        if (shortest.magnitude > EFull) shortest = shortest.normalized * EFull;
                        target = (Reading(thigh) + shortest) * Mathf.Rad2Deg;
                    }
                    thigh.xDrive = DriveOf(spring, damper, 130, target.x); thigh.yDrive = DriveOf(spring, damper, 130, target.y); thigh.zDrive = DriveOf(spring, damper, 130, target.z);
                    Physics.Simulate(Dt);
                    farthest = Mathf.Max(farthest, Quaternion.Angle(start, JointTurn(thigh)));
                }
                float left = Quaternion.Angle(JointTurn(thigh), goal);
                Debug.Log($"FACTS drive across a half turn, the goal {(aboutY ? "30 degrees about its own y" : "20 degrees on about x")} from {Quaternion.Angle(Quaternion.identity, start):0.0} degrees, written {(control ? "as its own rotation vector from zero" : "by the shortest turn")}: after a second {left:0.00} degrees from the goal, at most {farthest:0.0} from the start; it read {Three(began)} at the start and {Three(Reading(thigh))} at the end");
                if (!control && (left > 1 || farthest > (aboutY ? 45 : 25))) failed.Add($"written by the shortest turn, the goal {(aboutY ? "about y" : "about x")}: {left:0.00} degrees left, {farthest:0.0} from the start");
                if (control && !aboutY && farthest <= 90) failed.Add($"the control went at most {farthest:0.0} degrees from the start, so the fact proves nothing");
            }
            Assert.IsEmpty(failed, string.Join("; ", failed));
        }

        // K4(b): the engine's gravity forces (GetJointGravityForces) of Hung's two links, on a fixed root and on a floating
        // one in the same pose. Expected from the engine's source (getGeneralizedGravityForce reckons a floating base with no
        // joint accelerations, all falling together): about nothing on the floating root, so the load through the target is
        // worked out from the pose. Bar: every floating value under 1% of the largest fixed one.
        [UnityTest]
        public IEnumerator GravityForcesOnAFloatingRootAreAboutNothing()
        {
            var roots = new ArticulationBody[2]; var links = new ArticulationBody[2, 2];
            for (int r = 0; r < 2; r++)
            {
                Vector3 at = new Vector3(-500 - 3 * r, 50, -520);
                ArticulationBody root;
                if (r == 0) root = Root(at);
                else
                {
                    var g = new GameObject("Facts floating root"); g.transform.position = at; made.Add(g);
                    root = g.AddComponent<ArticulationBody>();
                    root.mass = 35; root.automaticCenterOfMass = false; root.centerOfMass = Vector3.zero;
                    root.automaticInertiaTensor = false; root.inertiaTensor = new Vector3(1.2f, .4f, 1.2f); root.inertiaTensorRotation = Quaternion.identity;
                    root.linearDamping = 0; root.angularDamping = 0;
                }
                var a = Link(root.transform, "A", root.transform.position, Quaternion.Euler(0, 0, 90), 3, new Vector3(0, -.2f, 0), new Vector3(.02f, .005f, .02f));
                var b = Link(a.transform, "B", a.transform.TransformPoint(new Vector3(0, -.4f, 0)), a.transform.rotation * Quaternion.Euler(0, 0, -45), 2, new Vector3(0, -.2f, 0), new Vector3(.015f, .004f, .015f));
                foreach (var link in new[] { a, b })
                {
                    link.jointType = ArticulationJointType.SphericalJoint;
                    link.twistLock = ArticulationDofLock.FreeMotion; link.swingYLock = ArticulationDofLock.FreeMotion; link.swingZLock = ArticulationDofLock.FreeMotion;
                    link.anchorPosition = Vector3.zero; link.anchorRotation = Quaternion.identity; link.matchAnchors = true;
                }
                roots[r] = root; links[r, 0] = a; links[r, 1] = b;
            }
            yield return null;
            var forces = new[] { new List<float>(), new List<float>() }; var starts = new[] { new List<int>(), new List<int>() };
            for (int r = 0; r < 2; r++) { roots[r].GetJointGravityForces(forces[r]); roots[r].GetDofStartIndices(starts[r]); }
            float most = 0, floating = 0; string told = "";
            for (int i = 0; i < 2; i++)
                for (int d = 0; d < 3; d++)
                {
                    float f0 = forces[0][starts[0][links[0, i].index] + d], f1 = forces[1][starts[1][links[1, i].index] + d];
                    most = Mathf.Max(most, Mathf.Abs(f0)); floating = Mathf.Max(floating, Mathf.Abs(f1));
                    told += $" {(i == 0 ? "A" : "B")}{d} {f0:0.000} / {f1:0.000};";
                }
            Debug.Log($"FACTS gravity forces, fixed root / floating root ({forces[0].Count} and {forces[1].Count} values):{told} the largest floating {floating:0.0000} against the largest fixed {most:0.000}");
            Assert.Greater(most, 1f, "the fixed root's gravity forces were not read at all");
            Assert.Less(floating, .01f * most, "the floating root's gravity forces are not about nothing");
        }

        // A miner's floating leg as FloatingLeg, built from its parts: the hips the stand-in above; each part's centre its
        // joint's distance below that joint and the next joint twice it; the boot's centre at minus its joint's place; the
        // hip and ankle balls, the knee a hinge about the line across; gravity off; the solver's turns as the game's; each
        // link's top turning speed out of the way (the game's 20 is a drag of its own and would hide a gain). Driven: the
        // drives of joint_model 2.3 on every axis, target zero; else left alone.
        private ArticulationBody[] BareLeg(Leg of, Vector3 at, bool driven)
        {
            float scale = LegScale(of);
            var g = new GameObject("Facts hips"); g.transform.position = at; made.Add(g);
            var hips = g.AddComponent<ArticulationBody>();
            hips.useGravity = false; hips.mass = of.Body - of.Mass;
            hips.automaticCenterOfMass = false; hips.centerOfMass = new Vector3(0, .25f * scale, 0);
            hips.automaticInertiaTensor = false; hips.inertiaTensor = HipsTurning(of); hips.inertiaTensorRotation = Quaternion.identity;
            hips.linearDamping = 0; hips.angularDamping = 0; hips.maxAngularVelocity = 1000;
            hips.solverIterations = 14; hips.solverVelocityIterations = 4; hips.sleepThreshold = 0;
            LegPart[] parts = { of.Thigh, of.Shin, of.Boot };
            var links = new ArticulationBody[4]; links[0] = hips;
            Vector3 joint = at + new Vector3(.09f * scale, 0, 0);
            for (int i = 0; i < 3; i++)
            {
                var p = parts[i];
                Vector3 centre = i < 2 ? new Vector3(0, -Mathf.Abs(p.Joint.y), 0) : -p.Joint;
                var link = Link(links[i].transform, i == 0 ? "Thigh" : i == 1 ? "Shin" : "Boot", joint, Quaternion.identity, p.Kg, centre, p.Turning);
                link.useGravity = false; link.maxAngularVelocity = 1000;
                link.jointType = i == 1 ? ArticulationJointType.RevoluteJoint : ArticulationJointType.SphericalJoint;
                link.anchorPosition = Vector3.zero; link.anchorRotation = Quaternion.identity; link.matchAnchors = true;
                link.twistLock = ArticulationDofLock.FreeMotion;
                if (i != 1) { link.swingYLock = ArticulationDofLock.FreeMotion; link.swingZLock = ArticulationDofLock.FreeMotion; }
                var drive = driven ? DriveOf(p.Spring, p.Damper, p.Has, 0) : DriveOf(0, 0, 0, 0);
                link.xDrive = drive; if (i != 1) { link.yDrive = drive; link.zDrive = drive; }
                links[i + 1] = link;
                if (i < 2) joint += new Vector3(0, -2 * Mathf.Abs(p.Joint.y), 0);
            }
            return links;
        }

        private struct Swept { public bool Failed, Rose; public string Why; public float First, Most; }

        // One run of the sweep: set turning as Whirled sets it, a second, and the count as Whirled's: what it holds (the
        // motion's energy, true speeds) after each step, less what the drives were counted to give (each drive's force times
        // its joint's mean speed over the step). It fails if it ever holds more than after its first step (beyond 1.0001 of
        // it), or in any step gains beyond the count more than half of what its dampers took (per axis min(c|v|, S)|v|dt);
        // while it holds under a thousandth of what it began with, nothing counts.
        private IEnumerator SweepAt(Leg of, float speed, bool driven, Vector3 at, System.Action<Swept> done)
        {
            var parts = BareLeg(of, at, driven);
            yield return null;
            parts[1].jointVelocity = new ArticulationReducedSpace(speed, .3f * speed, 0);
            parts[2].jointVelocity = new ArticulationReducedSpace(-speed);
            parts[3].jointVelocity = new ArticulationReducedSpace(.5f * speed, speed, 0);
            float[] damper = { 0, driven ? of.Thigh.Damper : 0, driven ? of.Shin.Damper : 0, driven ? of.Boot.Damper : 0 }, has = { 0, of.Thigh.Has, of.Shin.Has, of.Boot.Has };
            var before = new float[4][];
            for (int i = 1; i < 4; i++) before[i] = new float[parts[i].dofCount];
            var r = new Swept();
            float held = 0;
            for (int k = 0; k <= Mathf.RoundToInt(1 / Dt); k++)
            {
                Physics.Simulate(Dt);
                float now = Held(parts), given = 0, took = 0;
                for (int i = 1; i < 4; i++)
                {
                    var v = parts[i].jointVelocity; var force = parts[i].driveForce;
                    for (int d = 0; d < before[i].Length; d++)
                    {
                        float mean = k == 0 ? v[d] : .5f * (v[d] + before[i][d]);
                        given += force[d] * mean * Dt;
                        took += Mathf.Min(damper[i] * Mathf.Abs(mean), has[i]) * Mathf.Abs(mean) * Dt;
                        before[i][d] = v[d];
                    }
                }
                if (k == 0) { r.First = r.Most = held = now; continue; }
                if (Mathf.Max(held, now) >= .001f * r.First)
                {
                    if (now > 1.0001f * r.First) { r.Rose = true; if (!r.Failed) { r.Failed = true; r.Why = $"held {now / r.First:0.0000} of its first step's at step {k}"; } }
                    float gained = now - held - given;
                    if (gained > .5f * took && !r.Failed) { r.Failed = true; r.Why = $"gained {gained:0.0000} J beyond the count at step {k}, its dampers took {took:0.0000}"; }
                }
                r.Most = Mathf.Max(r.Most, now);
                held = now;
            }
            TakeAway();
            done(r);
        }

        // K5, the speed sweep: each miner's floating leg, driven, set turning at 10 to 50 rad/s, a second each. Each joint's
        // bound: its top speed, or the lowest failing speed over 1.5 if lower. Bar: every miner holds at 10 (so every bound
        // is at least 6.7 rad/s, above the 5 a rise asks at its quickest). Control: left alone at 30, each must fail.
        [UnityTest]
        public IEnumerator EachMinersLegHoldsItsDrivesAtTheSpeedsARiseAsks()
        {
            var failed = new List<string>();
            float[] speeds = { 10, 15, 20, 25, 30, 35, 40, 45, 50 };
            for (int m = 0; m < Legs.Length; m++)
            {
                var of = Legs[m];
                float lowest = float.PositiveInfinity; string told = "";
                foreach (float speed in speeds)
                {
                    Swept r = default;
                    yield return SweepAt(of, speed, true, new Vector3(300 + 10 * m, 50, -900), x => r = x);
                    told += $" {speed:0}: {(r.Failed ? "fails, " + r.Why : "holds")}, most {r.Most / r.First:0.0000} of its first step's;";
                    if (r.Failed && speed < lowest) lowest = speed;
                }
                Swept alone = default;
                yield return SweepAt(of, 30, false, new Vector3(300 + 10 * m, 50, -900), x => alone = x);
                float hip = Mathf.Min(of.Thigh.Top, lowest / 1.5f), knee = Mathf.Min(of.Shin.Top, lowest / 1.5f), ankle = Mathf.Min(of.Boot.Top, lowest / 1.5f);
                Debug.Log($"FACTS speed sweep, {of.Name} (hips {of.Body - of.Mass:0.00} kg turning {Three(HipsTurning(of))}):{told} left alone at 30 it {(alone.Failed ? "fails, " + alone.Why : "holds")}, most {alone.Most / alone.First:0.0000} of its first step's; bounds: hip {hip:0.0}, knee {knee:0.0}, ankle {ankle:0.0} rad/s");
                if (lowest <= 10) failed.Add($"{of.Name} fails at 10 rad/s");
                if (!alone.Failed) failed.Add($"{of.Name} left alone at 30 holds, so the sweep proves nothing for it");
            }
            Assert.IsEmpty(failed, string.Join("; ", failed));
        }

        // K8's law: the share of a drive's asked force the solver gives a light part pressed on the ground: x = I/(I + a),
        // a = dt (dt k + c), share = 1 - (1 - x)^18 (the game's 14 + 4 turns of the solver); I the part's turning weight about
        // the joint's axis, its centre's distance from the joint counted in.
        private static float LawShare(float turning, float spring, float damper) { float a = Dt * (Dt * spring + damper), x = turning / (turning + a); return 1 - Mathf.Pow(1 - x, 18); }

        private static float AboutItsJoint(LegPart p, Vector3 centre) => p.Turning.x + p.Kg * (centre.y * centre.y + centre.z * centre.z);

        // The moment of gravity of all on the root's side of a joint, about it, along its axis (N m); and that side's m g h
        // over the joint (its load's gradient against a turn of it).
        private static float GravityLoad(ArticulationBody[] parts, int joint, out float mgh)
        {
            var child = parts[joint];
            Vector3 at = child.transform.TransformPoint(child.anchorPosition), axis = child.transform.rotation * child.anchorRotation * Vector3.right;
            float tau = 0; mgh = 0;
            for (int i = 0; i < joint; i++)
            {
                Vector3 arm = parts[i].worldCenterOfMass - at;
                tau += Vector3.Dot(Vector3.Cross(arm, parts[i].mass * Physics.gravity), axis);
                mgh += parts[i].mass * Physics.gravity.magnitude * Vector3.Dot(arm, Vector3.up);
            }
            return tau;
        }

        // K4(a)'s rig: the weight (the body less the leg) a floating root on top, as in the game, its centre at the stand-in
        // height over the hip and leaning 10 degrees forward; the thigh on a hinge (the hip), the shin (the knee), the boot (the
        // ankle) with its sole on Stood's ground and flat; every hinge about the line across, its zero the leg straight under
        // the weight; the knee bent 20 degrees, the shin forward and the thigh back so the whole body's centre is 5.5 cm ahead
        // of the ankle. Made in that pose; the pose is written again after (AsMade's way). Stood's link settings and solver.
        private (ArticulationBody[] parts, float[] goal) StandingLeg(Leg of, Vector3 at)
        {
            float scale = LegScale(of), hipsKg = of.Body - of.Mass, high = .25f * scale, lean = 10 * Mathf.Deg2Rad, bent = 20 * Mathf.Deg2Rad;
            float lt = 2 * Mathf.Abs(of.Thigh.Joint.y), ls = 2 * Mathf.Abs(of.Shin.Joint.y);
            Vector3 bootCentre = -of.Boot.Joint;
            System.Func<float, float> ahead = a =>
            {
                float b = bent - a, kneeZ = ls * Mathf.Sin(a), hipZ = kneeZ - lt * Mathf.Sin(b);
                return (hipsKg * (hipZ + high * Mathf.Sin(lean)) + of.Thigh.Kg * (hipZ + .5f * lt * Mathf.Sin(b)) + of.Shin.Kg * .5f * ls * Mathf.Sin(a) + of.Boot.Kg * bootCentre.z) / of.Body;
            };
            float lo = -30 * Mathf.Deg2Rad, hi = 60 * Mathf.Deg2Rad;
            for (int i = 0; i < 40; i++) { float mid = .5f * (lo + hi); if (ahead(mid) < .055f) lo = mid; else hi = mid; }
            float shinOn = .5f * (lo + hi), thighBack = bent - shinOn;
            Vector3 ankle = at + new Vector3(0, .5f * of.Sole.y - bootCentre.y, 0);
            Vector3 knee = ankle + ls * new Vector3(0, Mathf.Cos(shinOn), Mathf.Sin(shinOn)), hip = knee + lt * new Vector3(0, Mathf.Cos(thighBack), -Mathf.Sin(thighBack));

            var ground = new GameObject("Facts ground");
            ground.transform.position = at + .5f * Vector3.down;
            made.Add(ground);
            var surface = new PhysicsMaterial("Facts") { dynamicFriction = .7f, staticFriction = .8f, bounciness = 0, bounceCombine = PhysicsMaterialCombine.Minimum };
            var slab = ground.AddComponent<BoxCollider>();
            slab.size = new Vector3(6, 1, 6); slab.sharedMaterial = surface;

            var g = new GameObject("Facts weight"); g.transform.SetPositionAndRotation(hip, Quaternion.AngleAxis(lean * Mathf.Rad2Deg, Vector3.right)); made.Add(g);
            var weight = g.AddComponent<ArticulationBody>();
            weight.mass = hipsKg; weight.automaticCenterOfMass = false; weight.centerOfMass = new Vector3(0, high, 0);
            weight.automaticInertiaTensor = false; weight.inertiaTensor = HipsTurning(of); weight.inertiaTensorRotation = Quaternion.identity;
            weight.linearDamping = 0; weight.angularDamping = 0;
            weight.solverIterations = 14; weight.solverVelocityIterations = 4; weight.sleepThreshold = 0;
            var parts = new ArticulationBody[4]; parts[0] = weight;
            Vector3[] place = { hip, hip, knee, ankle };
            Quaternion[] turned = { Quaternion.identity, Quaternion.AngleAxis(-thighBack * Mathf.Rad2Deg, Vector3.right), Quaternion.AngleAxis(shinOn * Mathf.Rad2Deg, Vector3.right), Quaternion.identity };
            LegPart[] of4 = { default, of.Thigh, of.Shin, of.Boot };
            for (int i = 1; i < 4; i++)
            {
                var p = of4[i];
                var link = Link(parts[i - 1].transform, i == 1 ? "Thigh" : i == 2 ? "Shin" : "Boot", place[i], turned[i], p.Kg, i < 3 ? new Vector3(0, -Mathf.Abs(p.Joint.y), 0) : bootCentre, p.Turning);
                link.jointType = ArticulationJointType.RevoluteJoint; link.twistLock = ArticulationDofLock.FreeMotion;
                link.anchorPosition = Vector3.zero; link.anchorRotation = Quaternion.identity;
                link.matchAnchors = false;
                link.parentAnchorPosition = i == 1 ? Vector3.zero : new Vector3(0, -2 * Mathf.Abs(of4[i - 1].Joint.y), 0);
                link.parentAnchorRotation = Quaternion.identity;
                parts[i] = link;
            }
            var box = parts[3].gameObject.AddComponent<BoxCollider>();
            box.size = of.Sole; box.center = bootCentre; box.sharedMaterial = surface;
            foreach (var link in parts) { link.maxAngularVelocity = 20; link.maxDepenetrationVelocity = 1.5f; link.collisionDetectionMode = CollisionDetectionMode.Continuous; }
            return (parts, new[] { 0, -(thighBack + lean), bent, -shinOn });
        }

        // K4(a), the load through the target, all three miners: each step at each joint tau = the moment of gravity of all on
        // the root's side of it, about it, along its axis, and target = goal + tau/(k share) (in degrees for the drive), with
        // joint_model 2.3's drives. Both ways: share 1 everywhere, and the making-up at the ankle by K8's law (Small 0.70, Long
        // 0.53, Round 0.45). The knee's and hip's shares are not known; the run reads each joint's, both to first order
        // (1 - offset k / tau, the design's) and exactly (tau / (tau / its share + k offset)). Bar: with the making-up,
        // every joint within 2 degrees of its goal after 2 s.
        // Decided before the run, from the margins reckoned by hand: without the making-up a joint stands only if s k > (1 - s)
        // m g h, and at the ankle it does not for any miner at the stand-in height (Small about 70 against 87, Long 122 against
        // 307, Round 113 against 406). The weight is kept there all the same: lowering it far enough to stand would put it
        // below the knee for Long and Round, which is no stance. So the run without is expected to fall, and that is the
        // finding (the stance stands only through the making-up), not a fault of the rig. With the making-up the 2-degree bar
        // tests the law: a true share 5% below it leaves about 1 to 2 degrees, 10% below it 3.6 to 3.9.
        [UnityTest]
        public IEnumerator TheLoadGoesThroughTheTargetOnEachMinersLeg()
        {
            var failed = new List<string>();
            string[] named = { "", "hip", "knee", "ankle" };
            for (int m = 0; m < Legs.Length; m++)
                for (int way = 0; way < 2; way++)
                {
                    var of = Legs[m]; bool madeUp = way == 1;
                    TakeAway();
                    var (parts, goal) = StandingLeg(of, new Vector3(-300 - 10 * m, 20, 300 + 10 * way));
                    yield return null;
                    string atMaking = Three(new Vector3(parts[1].jointPosition[0], parts[2].jointPosition[0], parts[3].jointPosition[0]) * Mathf.Rad2Deg);
                    var starts = new List<int>(); parts[0].GetDofStartIndices(starts);
                    var q = new List<float>(); parts[0].GetJointPositions(q);
                    for (int j = 1; j < 4; j++) q[starts[parts[j].index]] = goal[j];
                    parts[0].SetJointPositions(q);
                    parts[0].SetJointVelocities(new List<float>(new float[q.Count]));
                    float[] k = { 0, of.Thigh.Spring, of.Shin.Spring, of.Boot.Spring }, c = { 0, of.Thigh.Damper, of.Shin.Damper, of.Boot.Damper }, has = { 0, of.Thigh.Has, of.Shin.Has, of.Boot.Has };
                    float[] law = { 0, LawShare(AboutItsJoint(of.Thigh, new Vector3(0, of.Thigh.Joint.y, 0)), k[1], c[1]), LawShare(AboutItsJoint(of.Shin, new Vector3(0, of.Shin.Joint.y, 0)), k[2], c[2]), LawShare(AboutItsJoint(of.Boot, of.Boot.Joint), k[3], c[3]) };
                    float[] share = { 1, 1, 1, madeUp ? law[3] : 1 }, tau = new float[4], mgh = new float[4];
                    Vector3 weightWas = parts[0].worldCenterOfMass; Quaternion turnWas = parts[0].transform.rotation;
                    float fellAt = -1, firstAnkle = 0;
                    for (int step = 0; step < 100; step++)
                    {
                        for (int j = 1; j < 4; j++)
                        {
                            tau[j] = GravityLoad(parts, j, out mgh[j]);
                            parts[j].xDrive = DriveOf(k[j], c[j], has[j], (goal[j] + tau[j] / (k[j] * share[j])) * Mathf.Rad2Deg);
                        }
                        if (step == 0) firstAnkle = tau[3];
                        Physics.Simulate(Dt);
                        if (fellAt < 0)
                        {
                            bool fell = parts[0].worldCenterOfMass.y < weightWas.y - .1f;
                            for (int j = 1; j < 4; j++) fell |= Mathf.Abs(parts[j].jointPosition[0] - goal[j]) > 15 * Mathf.Deg2Rad;
                            if (fell) fellAt = (step + 1) * Dt;
                        }
                    }
                    string told = "";
                    for (int j = 1; j < 4; j++)
                    {
                        float off = goal[j] - parts[j].jointPosition[0];
                        bool readable = Mathf.Abs(tau[j]) > .02f * has[j];
                        string shares = !readable ? "its share unreadable, the load too small" : (share[j] == 1 ? $"its share {1 - off * k[j] / tau[j]:0.000} to first order, {tau[j] / (tau[j] + k[j] * off):0.000} exactly" : $"its share {tau[j] / (tau[j] / share[j] + k[j] * off):0.000} exactly, {tau[j] / (tau[j] / share[j] + k[j] * off) / share[j]:0.000} of the law's");
                        told += $" {named[j]} goal {goal[j] * Mathf.Rad2Deg:0.00}, off by {-off * Mathf.Rad2Deg:0.00} degrees under a load of {tau[j]:0.00} N m (k {k[j]:0.0}, the law's share {law[j]:0.000}, s k {law[j] * k[j]:0.0} against (1 - s) m g h {(1 - law[j]) * mgh[j]:0.0}), {shares};";
                        if (madeUp && Mathf.Abs(off) > 2 * Mathf.Deg2Rad) failed.Add($"{of.Name}'s {named[j]} off by {-off * Mathf.Rad2Deg:0.00} degrees with the making-up");
                    }
                    if (madeUp && fellAt >= 0) failed.Add($"{of.Name} fell at {fellAt:0.00} s with the making-up");
                    string how = madeUp ? $"made up at the ankle by the law's share {law[3]:0.000}" : "without the making-up", ending = fellAt < 0 ? "it stood" : $"it fell at {fellAt:0.00} s";
                    Debug.Log($"FACTS load through the target, {of.Name}, {how}: {ending}; read at its making {atMaking} degrees, the ankle's load at the first step {firstAnkle:0.00} N m; after 2 s:{told} the weight moved {Three(parts[0].worldCenterOfMass - weightWas)} m and turned {Quaternion.Angle(turnWas, parts[0].transform.rotation):0.00} degrees, the boot tilted {Quaternion.Angle(Quaternion.identity, parts[3].transform.rotation):0.00}");
                }
            Assert.IsEmpty(failed, string.Join("; ", failed));
        }
    }
}
