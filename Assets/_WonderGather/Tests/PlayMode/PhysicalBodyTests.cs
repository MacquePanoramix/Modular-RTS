using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, steps 1 and 2: every miner and every pickaxe has been weighed, and a pickaxe in a miner's hands is a real
    // body that the arms move only as hard as they can (PhysicalBody, PhysicalHands). The swing here is the bench's
    // rough one (PhysicalSwing); what is tested is what comes out of the weights and the strength.
    public sealed class PhysicalBodyTests
    {
        private MinerChoice choice;
        private OrdinaryGround ground;
        private Vector3 spot, away;

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
            var a = ground.Path[4];
            var b = ground.Path[5];
            spot = OnGround(new Vector3(a.x, 0, a.y));
            away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
            away.y = 0;
            away.Normalize();
        }

        private Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        private int Index(string name)
        {
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == name) return i;
            Assert.Fail("No miner is called " + name);
            return -1;
        }

        [UnityTest]
        public IEnumerator EveryMinerAndItsPickaxeHaveBeenWeighed()
        {
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var physical = unit.GetComponent<PhysicalBody>();
                var miner = unit.GetComponent<MinerBody>();
                Assert.That(physical, Is.Not.Null, name + " has no weights.");
                Assert.That(physical.Ready, Is.True, name + "'s weights are not complete.");
                Assert.That(physical.Mass, Is.InRange(25f, 120f), name + "'s weight.");
                float sum = 0;
                for (int k = 0; k < physical.PartCount; k++) sum += physical.PartAt(k).mass;
                Assert.That(sum, Is.EqualTo(physical.Mass).Within(.01f), name + "'s parts do not add up to its weight.");
                // Its centre is within it: between the hips and the chest, over its feet.
                Vector3 centre = physical.CentreOfMass(), feet = unit.transform.position;
                Assert.That(centre.y, Is.InRange(miner.Rig.pelvis.position.y - .1f, miner.Rig.chest.position.y + .1f), name + "'s centre of mass is not in its trunk.");
                Assert.That(Vector3.ProjectOnPlane(centre - feet, Vector3.up).magnitude, Is.LessThan(.12f), name + "'s centre of mass is not over its feet.");
                Assert.That(physical.ShoulderCapacity, Is.GreaterThan(5f));
                var tool = miner.Pickaxe;
                Assert.That(tool.HasWeight, Is.True, name + "'s pickaxe has not been weighed.");
                Assert.That(tool.Mass, Is.InRange(.5f, 6f), name + "'s pickaxe's weight.");
                // A pick's weight is nearly all at its head.
                Assert.That(Vector3.Distance(tool.Centre, tool.Head), Is.LessThan(.25f * (tool.Top - tool.Foot) + .1f), name + "'s pickaxe's weight is not near its head.");
                Assert.That(tool.Centre.y, Is.GreaterThan(Mathf.Lerp(tool.Foot, tool.Top, .7f)), name + "'s pickaxe's weight is not near its head.");
            }
        }

        private struct Tried
        {
            public PhysicalSwing.Result first, second;
            public float miss, aside;
            public int swings;
        }

        // Round (both hands free) swings the bench's swing twice, at a strength, with its pickaxe at a share of its weight.
        private IEnumerator Try(float strength, float weight, System.Action<Tried> done)
        {
            choice.Choose(Index("Round"));
            yield return Wait(.2f);
            var unit = choice.Current;
            var miner = unit.GetComponent<MinerBody>();
            var biped = unit.GetComponent<ProceduralBiped>();
            var physical = unit.GetComponent<PhysicalBody>();
            unit.Motor.Stop();
            unit.GetComponent<NavMeshAgent>().Warp(spot);
            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
            biped.ResetPose();
            yield return Wait(.6f);
            physical.Strength = strength;
            var definition = miner.Pickaxe;
            var hands = unit.gameObject.AddComponent<PhysicalHands>();
            var swing = unit.gameObject.AddComponent<PhysicalSwing>();
            swing.hands = hands; swing.body = biped; swing.tool = definition;
            biped.Bow(PhysicalSwing.RestBow);
            yield return Wait(.6f);
            swing.Intend(PhysicalSwing.Rest, out var at, out var turned);
            Vector3 rests = at + turned * definition.Head;
            float top = rests.y - definition.HeadRadius - .015f, floor = ground.Height(rests.x, rests.z) - .1f;
            var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.transform.SetPositionAndRotation(new Vector3(rests.x, (top + floor) * .5f, rests.z), Quaternion.LookRotation(away));
            rock.transform.localScale = new Vector3(.5f, top - floor, .5f);
            rock.GetComponent<MeshRenderer>().enabled = false;
            hands.Take(definition, at, turned, weight);
            var tried = new Tried();
            var after = new GameObject("After everything").AddComponent<AfterEverything>();
            after.Then = () =>
            {
                if (hands.Held == null) return;
                tried.miss = Mathf.Max(tried.miss, Mathf.Max(hands.Miss(0), hands.Miss(1)));
                tried.aside = Mathf.Max(tried.aside, Vector3.Angle(hands.Held.rotation * Vector3.right, unit.transform.right));
            };
            float began = Time.time;
            while (swing.results.Count < 2 && Time.time - began < 30) yield return null;
            tried.swings = swing.results.Count;
            if (tried.swings > 0) tried.first = swing.results[0];
            if (tried.swings > 1) tried.second = swing.results[1];
            after.Then = null;
            Object.Destroy(after.gameObject);
            var was = hands.Drop();
            if (was != null) Object.Destroy(was.gameObject);
            Object.Destroy(swing);
            Object.Destroy(hands);
            Object.Destroy(rock);
            biped.Bow(0);
            physical.Strength = 1;
            yield return Wait(.2f);
            done(tried);
        }

        // What a body manages with a tool comes out of its strength and the tool's weight: a heavier pickaxe is harder
        // to raise, a stronger body raises it more easily and brings it down faster, and a body too weak for its
        // pickaxe is slow to raise it and strikes feebly. Nothing here is set by a rule for the case.
        [UnityTest, Timeout(600000)]
        public IEnumerator StrengthAndWeightDecideTheSwing()
        {
            Tried ordinary = default, heavy = default, strong = default, feeble = default;
            yield return Try(1, 1, t => ordinary = t);
            yield return Try(1, 2, t => heavy = t);
            yield return Try(2, 1, t => strong = t);
            yield return Try(.5f, 2, t => feeble = t);
            Debug.Log($"PHYSICAL_SWING ordinary: raised {ordinary.first.lifted:F2} m in {ordinary.first.liftTime:F2} s at {ordinary.first.liftEffort:P0}, struck at {ordinary.first.speed:F1} m/s; "
                + $"heavy: {heavy.first.liftEffort:P0}, {heavy.first.speed:F1} m/s; strong: {strong.first.liftEffort:P0}, {strong.first.speed:F1} m/s; "
                + $"feeble: {feeble.first.lifted:F2} m in {feeble.first.liftTime:F2} s at {feeble.first.liftEffort:P0}, {feeble.first.speed:F1} m/s; "
                + $"hands at most {Mathf.Max(ordinary.miss, strong.miss, heavy.miss) * 1000:F0} mm off the handle");
            // An ordinary body with its own pickaxe swings it well, twice alike.
            Assert.That(ordinary.swings, Is.EqualTo(2));
            Assert.That(ordinary.first.struck && ordinary.second.struck, Is.True, "The ordinary swing did not strike.");
            Assert.That(ordinary.first.reached, Is.GreaterThan(.98f), "The ordinary swing did not raise the pickaxe all the way.");
            Assert.That(ordinary.first.liftEffort, Is.InRange(.2f, .7f), "Raising its own pickaxe should be work an ordinary body has to spare.");
            Assert.That(ordinary.first.speed, Is.InRange(4f, 10f), "The ordinary blow's speed.");
            Assert.That(ordinary.second.speed, Is.EqualTo(ordinary.first.speed).Within(.5f), "Two swings alike should strike alike.");
            // The tool stays square in the hands, and the hands on it.
            Assert.That(ordinary.aside, Is.LessThan(45f), "The pickaxe turned aside in the hands.");
            Assert.That(Mathf.Max(ordinary.miss, strong.miss, heavy.miss), Is.LessThan(.035f), "A hand came off the handle.");
            // Heavier: harder to raise, and it arrives slower, with more behind it.
            Assert.That(heavy.first.liftEffort, Is.GreaterThan(ordinary.first.liftEffort * 1.3f));
            Assert.That(heavy.first.speed, Is.LessThan(ordinary.first.speed));
            // Stronger: easier to raise, and it arrives faster.
            Assert.That(strong.first.liftEffort, Is.LessThan(ordinary.first.liftEffort * .7f));
            Assert.That(strong.first.speed, Is.GreaterThan(ordinary.first.speed * 1.1f));
            // Too weak for it: slow to raise, at all it has, and a feeble blow.
            Assert.That(feeble.first.liftTime, Is.GreaterThan(ordinary.first.liftTime * 1.5f));
            Assert.That(feeble.first.liftEffort, Is.GreaterThan(.9f));
            Assert.That(feeble.first.speed, Is.LessThan(ordinary.first.speed * .6f));
        }

        // The body is posed once a frame and the physics steps on its own clock: what comes out must not depend on how
        // many frames there are to a step.
        [UnityTest, Timeout(600000)]
        public IEnumerator TheSwingIsTheSameAtAnyFrameRate()
        {
            Tried free = default, slow = default;
            yield return Try(1, 1, t => free = t);
            Time.captureFramerate = 25;
            try { yield return Try(1, 1, t => slow = t); }
            finally { Time.captureFramerate = 0; }
            Debug.Log($"PHYSICAL_RATE as fast as it runs: {free.first.liftEffort:P0}, {free.first.speed:F2} m/s; at 25 frames a second: {slow.first.liftEffort:P0}, {slow.first.speed:F2} m/s");
            Assert.That(slow.first.struck && free.first.struck, Is.True);
            Assert.That(slow.first.liftEffort, Is.EqualTo(free.first.liftEffort).Within(.06f), "Raising the pickaxe took a different effort at another frame rate.");
            Assert.That(slow.first.speed, Is.EqualTo(free.first.speed).Within(.6f), "The blow arrived at a different speed at another frame rate.");
            Assert.That(slow.first.liftTime, Is.EqualTo(free.first.liftTime).Within(.08f));
        }
    }
}
