using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The energy count of the live courses (LiveEnergy) checked on one part before it is read on a body (the blow-up's
    // trace, November 15, 2026; the memory engine-step-adds-energy: have an instrument checked on a one-part case before a
    // finding is called settled). One part, free in the air, away from everything, the physics stepped by the test:
    // spun and left alone it must hold what it holds; pushed, what it gains must be what it was given; on a damped hinge,
    // what it loses must be what its drive took. Each within a hundredth, over 50 steps of a fiftieth of a second.
    public sealed class LiveEnergyTests
    {
        private const float Dt = .02f;
        private const int Steps = 50;
        private readonly List<GameObject> made = new List<GameObject>();
        private SimulationMode modeWas;
        private Vector3 gravityWas;

        [SetUp] public void Begin() { modeWas = Physics.simulationMode; gravityWas = Physics.gravity; Physics.simulationMode = SimulationMode.Script; }

        [TearDown]
        public void End()
        {
            foreach (var g in made) if (g != null) Object.DestroyImmediate(g);
            made.Clear();
            Physics.simulationMode = modeWas;
            Physics.gravity = gravityWas;
        }

        // One free part as Small's thigh (its centre off its own origin, as a link's is), free in the air.
        private ArticulationBody FreePart(Vector3 at, bool falls)
        {
            var g = new GameObject("Energy part");
            g.transform.position = at;
            made.Add(g);
            var part = g.AddComponent<ArticulationBody>();
            part.immovable = false; part.useGravity = falls; part.mass = 2.38f;
            part.automaticCenterOfMass = false; part.centerOfMass = new Vector3(0, -.13f, 0);
            part.automaticInertiaTensor = false; part.inertiaTensor = new Vector3(.0131f, .0029f, .0131f); part.inertiaTensorRotation = Quaternion.identity;
            part.linearDamping = 0; part.angularDamping = 0;
            return part;
        }

        // The count stepped over the steps; each step's push put on before the count's start. What it held after the
        // first step, the most and least after, and the sums of what was given and gained.
        private static (float began, float most, float least, float given, float gained) Counted(LiveEnergy count, System.Action push)
        {
            float began = float.NaN, most = float.MinValue, least = float.MaxValue, given = 0, gained = 0;
            for (int k = 0; k < Steps; k++)
            {
                push?.Invoke();
                count.Before(Dt);
                Physics.Simulate(Dt);
                Assert.That(count.After(Dt, k, k * Dt, "step"), Is.True, "The count had no start for its step.");
                if (k == 0) began = count.Held;
                most = Mathf.Max(most, count.Held); least = Mathf.Min(least, count.Held);
                given += count.Given; gained += count.Gain;
            }
            return (began, most, least, given, gained);
        }

        // Spun at 30 rad/s about one of its own lines and going at 1.1 m/s, nothing on it and nothing pulling it down: it
        // keeps what it holds. (Its going is told at its centre by the engine; this checks the count's root.)
        [UnityTest]
        public IEnumerator AFreePartSpunInTheAirHoldsItsEnergy()
        {
            var part = FreePart(new Vector3(800, 60, 800), false);
            yield return null;
            part.angularVelocity = part.transform.rotation * new Vector3(30, 0, 0);
            part.linearVelocity = new Vector3(1, .5f, 0);
            var count = new LiveEnergy();
            Assert.That(count.Take(part), Is.True);
            var r = Counted(count, null);
            Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "ENERGY_CHECK a free part spun at 30 rad/s in the air: held {0:0.0000} J after the first step; over {1} steps at most {2:0.0000} and at least {3:0.0000} of that; given {4:+0.0000;-0.0000} J, gained {5:+0.0000;-0.0000} J; turning {6:0.00} rad/s at the end",
                r.began, Steps, r.most / r.began, r.least / r.began, r.given, r.gained, count.Fastest));
            Assert.That(r.most / r.began, Is.LessThan(1.01f), "A free part spun in the air gained more than a hundredth by the count.");
            Assert.That(r.least / r.began, Is.GreaterThan(.99f), "A free part spun in the air lost more than a hundredth by the count.");
            Assert.That(Mathf.Abs(r.given), Is.LessThan(1e-4f), "Nothing pushed it, but the count says something was given.");
        }

        // So, thrown up and pulled down: what it holds, its weight lifted with it (m g h), stays. The engine's step
        // itself loses half of m (g step)^2 a step (its going changed before its place: about 0.019 J a step for each
        // kilogram); the count may show that, and no more.
        [UnityTest]
        public IEnumerator AFreePartThrownUpHoldsItsEnergyLessTheStepsOwnLoss()
        {
            var part = FreePart(new Vector3(820, 60, 800), true);
            yield return null;
            part.angularVelocity = part.transform.rotation * new Vector3(30, 0, 0);
            part.linearVelocity = new Vector3(1, 4, 0);
            var count = new LiveEnergy { Level = part.worldCenterOfMass.y };
            Assert.That(count.Take(part), Is.True);
            var r = Counted(count, null);
            float g = -Physics.gravity.y, stepLoss = .5f * part.mass * g * g * Dt * Dt;
            Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "ENERGY_CHECK a free part spun at 30 rad/s and thrown up: held {0:0.0000} J after the first step; over {1} steps at most {2:0.0000} and at least {3:0.0000} of that; gained {4:+0.0000;-0.0000} J in all, {5:+0.00000;-0.00000} J a step, against the step's own loss of {6:0.00000} J a step",
                r.began, Steps, r.most / r.began, r.least / r.began, r.gained, r.gained / Steps, stepLoss));
            Assert.That(r.gained / Steps, Is.LessThan(.01f * stepLoss), "Thrown up, the part gained by the count.");
            Assert.That(r.gained / Steps, Is.GreaterThan(-1.01f * stepLoss), "Thrown up, the part lost more by the count than the step's own loss.");
        }

        // Pushed every step with a force and a torque about one of its own lines (as a script pushes, before the step):
        // the engine's accumulated force and torque are what was put on it, and what it gains is what they gave.
        [UnityTest]
        public IEnumerator AFreePartPushedGainsWhatItWasGiven()
        {
            var part = FreePart(new Vector3(840, 60, 800), false);
            yield return null;
            part.angularVelocity = part.transform.rotation * new Vector3(10, 0, 0);
            var count = new LiveEnergy();
            Assert.That(count.Take(part), Is.True);
            Vector3 force = new Vector3(3, 0, 1), torque = Vector3.zero;
            float forceOff = 0, torqueOff = 0;
            var r = Counted(count, () =>
            {
                torque = part.transform.rotation * new Vector3(.2f, 0, 0);
                part.AddForce(force);
                part.AddTorque(torque);
                forceOff = Mathf.Max(forceOff, (part.GetAccumulatedForce(Dt) - force).magnitude);
                torqueOff = Mathf.Max(torqueOff, (part.GetAccumulatedTorque(Dt) - torque).magnitude);
            });
            Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "ENERGY_CHECK a free part pushed with 3.2 N and 0.2 N m: held {0:0.0000} J after the first step, {1:0.0000} at most; given {2:+0.0000;-0.0000} J over {3} steps, gained beyond it {4:+0.0000;-0.0000} J; the accumulated force and torque at most {5:0.000000} N and {6:0.000000} N m off what was put on; turning {7:0.00} rad/s at the end",
                r.began, r.most, r.given, Steps, r.gained, forceOff, torqueOff, count.Fastest));
            Assert.That(forceOff, Is.LessThan(1e-4f), "The engine's accumulated force is not what was put on the part.");
            Assert.That(torqueOff, Is.LessThan(1e-4f), "The engine's accumulated torque is not what was put on the part.");
            Assert.That(r.given, Is.GreaterThan(1f), "The pushes gave it next to nothing: the check checks nothing.");
            Assert.That(Mathf.Abs(r.gained), Is.LessThan(.01f * r.given), "Pushed, the part gained other than it was given, by more than a hundredth.");
            // (Reading the accumulated force does not take it away: the next reading is the same.)
            part.AddForce(force);
            Vector3 once = part.GetAccumulatedForce(Dt), again = part.GetAccumulatedForce(Dt);
            Assert.That((once - again).magnitude, Is.LessThan(1e-6f), "Reading the accumulated force took it away.");
            Physics.Simulate(Dt);
        }

        // One part on a hinge from a fixed root, set turning at 20 rad/s and slowed by its drive's damper: what it loses
        // is what its drive took (the drive's force times the mean of its turning over each step).
        [UnityTest]
        public IEnumerator APartOnADampedHingeLosesWhatItsDriveTook()
        {
            var g = new GameObject("Energy root");
            g.transform.position = new Vector3(860, 60, 800);
            made.Add(g);
            var root = g.AddComponent<ArticulationBody>();
            root.immovable = true; root.useGravity = false;
            var h = new GameObject("Energy hinged");
            h.transform.SetParent(g.transform, false);
            var part = h.AddComponent<ArticulationBody>();
            part.mass = 2.38f; part.useGravity = false;
            part.automaticCenterOfMass = false; part.centerOfMass = new Vector3(0, -.13f, 0);
            part.automaticInertiaTensor = false; part.inertiaTensor = new Vector3(.0131f, .0029f, .0131f); part.inertiaTensorRotation = Quaternion.identity;
            part.linearDamping = 0; part.angularDamping = 0; part.jointFriction = 0;
            part.jointType = ArticulationJointType.RevoluteJoint;
            part.anchorPosition = Vector3.zero; part.anchorRotation = Quaternion.identity; part.matchAnchors = true;
            part.twistLock = ArticulationDofLock.FreeMotion;
            part.xDrive = new ArticulationDrive { stiffness = 0, damping = .05f, forceLimit = 130, driveType = ArticulationDriveType.Force };
            yield return null;
            part.jointVelocity = new ArticulationReducedSpace(20f);
            var count = new LiveEnergy();
            Assert.That(count.Take(root), Is.True);
            Assert.That(count.Links.Length, Is.EqualTo(2));
            var r = Counted(count, null);
            Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "ENERGY_CHECK a part on a damped hinge, set turning at 20 rad/s: held {0:0.0000} J after the first step, {1:0.0000} at the least; given {2:+0.0000;-0.0000} J over {3} steps, gained beyond it {4:+0.0000;-0.0000} J; turning {5:0.00} rad/s at the end",
                r.began, r.least, r.given, Steps, r.gained, count.Fastest));
            Assert.That(r.given, Is.LessThan(-.1f * r.began), "The damper took next to nothing: the check checks nothing.");
            Assert.That(Mathf.Abs(r.gained), Is.LessThan(.01f * Mathf.Abs(r.given)), "On a damped hinge, the part lost other than its drive took, by more than a hundredth.");
        }
    }
}
