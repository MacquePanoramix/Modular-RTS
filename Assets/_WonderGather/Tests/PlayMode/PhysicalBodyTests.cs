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
            public float miss, aside, sink, mark;
            public int swings, rests;
            // Its balance: how near the edge of its feet its weight's point came (metres; negative: outside), how far
            // its hips leaned, and the steps it took to keep its feet.
            public float margin, leaned;
            public int steps;
            public System.Collections.Generic.List<PhysicalSwing.Result> all;
        }

        private IEnumerator Try(float strength, float weight, System.Action<Tried> done) => Try("Round", strength, weight, 0, done);

        // A miner swings twice (or more), at a strength, with its pickaxe at a share of its weight. mark: the height
        // the blow is aimed at, as a share of the miner's height (0: where the pick rests unaimed).
        private IEnumerator Try(string who, float strength, float weight, float mark, System.Action<Tried> done, int swings = 2)
        {
            choice.Choose(Index(who));
            yield return Wait(.2f);
            var unit = choice.Current;
            var miner = unit.GetComponent<MinerBody>();
            var biped = unit.GetComponent<ProceduralBiped>();
            var physical = unit.GetComponent<PhysicalBody>();
            physical.Refresh();
            unit.Motor.Stop();
            unit.GetComponent<NavMeshAgent>().Warp(spot);
            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
            biped.ResetPose();
            yield return Wait(.6f);
            physical.Strength = strength;
            // Each try begins fresh.
            physical.Refresh();
            var definition = miner.Pickaxe;
            var hands = unit.gameObject.AddComponent<PhysicalHands>();
            var swing = unit.gameObject.AddComponent<PhysicalSwing>();
            var back = unit.gameObject.AddComponent<PhysicalBack>();
            var balance = unit.gameObject.AddComponent<PhysicalBalance>();
            swing.hands = hands; swing.back = back; swing.body = biped; swing.tool = definition;
            // It sets its feet for the work and bows to it.
            balance.Brace(PhysicalSwing.StanceWider, PhysicalSwing.StanceStagger);
            back.Want(PhysicalSwing.RestBow);
            yield return Wait(1.3f);
            swing.Intend(PhysicalSwing.Rest, out var at, out var turned);
            Vector3 rests = at + turned * definition.Head;
            var tried = new Tried();
            if (mark > 0)
            {
                float height = miner.Rig.head.position.y - unit.transform.position.y;
                rests.y = unit.transform.position.y + mark * height + definition.HeadRadius + .015f;
                swing.Aim(rests - Vector3.up * .015f);
                back.Want(swing.RestBowNow);
                biped.Sink(swing.SinkFor);
                yield return Wait(.8f);
                swing.Intend(swing.RestLean, out at, out turned);
                tried.sink = swing.SinkFor; tried.mark = rests.y - .015f;
            }
            float top = rests.y - definition.HeadRadius - .015f, floor = ground.Height(rests.x, rests.z) - .1f;
            var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.transform.SetPositionAndRotation(new Vector3(rests.x, (top + floor) * .5f, rests.z), Quaternion.LookRotation(away));
            rock.transform.localScale = new Vector3(.5f, top - floor, .5f);
            rock.GetComponent<MeshRenderer>().enabled = false;
            hands.Take(definition, at, turned, weight);
            var after = new GameObject("After everything").AddComponent<AfterEverything>();
            after.Then = () =>
            {
                if (hands.Held == null) return;
                tried.miss = Mathf.Max(tried.miss, Mathf.Max(hands.Miss(0), hands.Miss(1)));
                tried.aside = Mathf.Max(tried.aside, Vector3.Angle(hands.Held.rotation * Vector3.right, unit.transform.right));
            };
            float began = Time.time;
            while (swing.results.Count < swings && Time.time - began < 30 + swings * 12) yield return null;
            tried.swings = swing.results.Count;
            tried.rests = swing.rests;
            tried.all = new System.Collections.Generic.List<PhysicalSwing.Result>(swing.results);
            if (tried.swings > 0) tried.first = swing.results[0];
            if (tried.swings > 1) tried.second = swing.results[1];
            after.Then = null;
            Object.Destroy(after.gameObject);
            var was = hands.Drop();
            if (was != null) Object.Destroy(was.gameObject);
            Object.Destroy(swing);
            Object.Destroy(hands);
            Object.Destroy(rock);
            Object.Destroy(back);
            tried.margin = balance.LeastMargin; tried.steps = balance.Steps; tried.leaned = balance.MostLean;
            Object.Destroy(balance);
            biped.Sink(0);
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
            Assert.That(feeble.first.liftTime, Is.GreaterThan(ordinary.first.liftTime * 1.2f));
            Assert.That(feeble.first.liftEffort, Is.GreaterThan(.8f));
            Assert.That(feeble.first.speed, Is.LessThan(ordinary.first.speed * .7f));
        }

        // Each miner swings the pickaxe made for it, with both hands, by its own strength.
        [UnityTest, Timeout(600000)]
        public IEnumerator EachMinerSwingsItsOwnPickaxeByItsOwnStrength()
        {
            foreach (string who in new[] { "Small", "Long", "Round" })
            {
                Tried tried = default;
                yield return Try(who, 1, 1, 0, t => tried = t);
                Debug.Log($"PHYSICAL_OWN {who}: upper hand {tried.first.choked:P0} of the way to the head; raised {tried.first.lifted:F2} m in {tried.first.liftTime:F2} s, arms {tried.first.liftEffort:P0}, back {tried.first.backEffort:P0}; "
                    + $"struck at {tried.first.speed:F1} m/s; hands at most {tried.miss * 1000:F0} mm off, the tool {tried.aside:F0} degrees aside");
                Assert.That(tried.swings, Is.EqualTo(2), who + " did not swing twice.");
                Assert.That(tried.first.struck && tried.second.struck, Is.True, who + " did not strike.");
                Assert.That(tried.first.reached, Is.GreaterThan(.98f), who + " did not raise its pickaxe all the way.");
                Assert.That(tried.first.speed, Is.InRange(3.5f, 10f), "The blow of " + who);
                Assert.That(tried.first.liftEffort, Is.LessThan(.85f), who + " should have something to spare raising its own pickaxe.");
                Assert.That(tried.first.backEffort, Is.InRange(.1f, .7f), "The back of " + who);
                Assert.That(tried.miss, Is.LessThan(.035f), "A hand of " + who + " came off the handle.");
                Assert.That(tried.aside, Is.LessThan(45f), "The pickaxe turned aside in the hands of " + who);
            }
        }

        // The body adapts by itself. A body the tool is heavy for takes it nearer the head to raise it; one it is light
        // for does not. Aimed low, the body bends its knees and bows to reach, and the blow lands where it was aimed.
        [UnityTest, Timeout(600000)]
        public IEnumerator TheBodyAdaptsToItsStrengthAndToWhereItStrikes()
        {
            Tried weak = default, strong = default, low = default, high = default;
            yield return Try("Round", .5f, 1, 0, t => weak = t);
            yield return Try("Round", 2, 1, 0, t => strong = t);
            yield return Try("Round", 1, 1, .22f, t => low = t);
            yield return Try("Round", 1, 1, .5f, t => high = t);
            Debug.Log($"PHYSICAL_ADAPTS upper hand: weak {weak.first.choked:P0}, strong {strong.first.choked:P0} of the way to the head; aimed low: sinks {low.sink:F3} m, its head lands {(low.first.landed.y - low.mark) * 1000:F0} mm from the mark's height at {low.first.speed:F1} m/s; "
                + $"aimed high: sinks {high.sink:F3} m, lands {(high.first.landed.y - high.mark) * 1000:F0} mm from it at {high.first.speed:F1} m/s");
            Assert.That(weak.first.choked, Is.GreaterThan(.6f), "A body the pickaxe is heavy for should take it near the head.");
            Assert.That(strong.first.choked, Is.LessThan(.2f), "A body the pickaxe is light for has no need to.");
            Assert.That(weak.first.struck && strong.first.struck, Is.True);
            Assert.That(low.sink, Is.GreaterThan(.1f), "Aimed low, the knees should bend.");
            Assert.That(high.sink, Is.LessThan(low.sink - .08f), "Aimed higher, the knees bend less.");
            Assert.That(low.first.struck && high.first.struck, Is.True, "An aimed blow did not land.");
            Assert.That(low.first.landed.y, Is.EqualTo(low.mark).Within(.06f), "The low blow did not land at the height it was aimed at.");
            Assert.That(high.first.landed.y, Is.EqualTo(high.mark).Within(.06f), "The higher blow did not land at the height it was aimed at.");
        }

        // Tiredness by itself: hard work spends a muscle, the harder the faster; light work does not; rest brings it back.
        [UnityTest]
        public IEnumerator HardWorkTiresAMuscleAndRestBringsItBack()
        {
            choice.Choose(Index("Round"));
            yield return Wait(.2f);
            var physical = choice.Current.GetComponent<PhysicalBody>();
            physical.Refresh();
            var arm = PhysicalBody.Muscles.RightArm;
            float fresh = physical.ShoulderOf(1);
            for (int k = 0; k < 500; k++) physical.Worked(arm, 1, .02f);
            float afterTen = physical.Spent(arm);
            Assert.That(afterTen, Is.InRange(.2f, .45f), "Ten seconds of all-out work should spend about a third of an arm.");
            Assert.That(physical.ShoulderOf(1), Is.EqualTo(fresh * (1 - afterTen)).Within(.01f), "A spent arm gives less.");
            Assert.That(physical.Spent(PhysicalBody.Muscles.LeftArm), Is.EqualTo(0).Within(1e-4f), "Only the arm that worked is spent.");
            for (int k = 0; k < 1500; k++) physical.Worked(arm, 0, .02f);
            Assert.That(physical.Spent(arm), Is.LessThan(afterTen * .45f), "Half a minute of rest should bring most of it back.");
            physical.Refresh();
            for (int k = 0; k < 3000; k++) physical.Worked(arm, .15f, .02f);
            Assert.That(physical.Spent(arm), Is.LessThan(.01f), "Light work can go on: it does not tire.");
            for (int k = 0; k < 500; k++) physical.Worked(arm, .5f, .02f);
            float half = physical.Spent(arm);
            Assert.That(half, Is.GreaterThan(.05f).And.LessThan(afterTen), "Harder work tires faster.");
            physical.Refresh();
        }

        // A miner that goes on swinging tires: it lands its blows more weakly, takes the tool nearer the head, and in
        // the end stops to rest, standing up straight; rested, it goes on, stronger again.
        [UnityTest, Timeout(900000)]
        public IEnumerator ATiredMinerWeakensRestsAndGoesOn()
        {
            Tried tried = default;
            Time.captureFramerate = 50;
            try { yield return Try("Round", 1, 1, 0, t => tried = t, 32); }
            finally { Time.captureFramerate = 0; }
            Assert.That(tried.swings, Is.EqualTo(32), "It did not get through its swings.");
            Debug.Log($"PHYSICAL_TIRED it rested {tried.rests} times in {tried.swings} swings; its balance: least margin {tried.margin * 1000:F0} mm, {tried.steps} steps");
            Assert.That(tried.rests, Is.InRange(1, 2), "Thirty-two swings should ask for a rest or two.");
            int rested = tried.all.FindIndex(1, r => r.spent < tried.all[tried.all.IndexOf(r) - 1].spent - .1f);
            Assert.That(rested, Is.GreaterThan(10), "It rested too soon.");
            var first = tried.all[0];
            var last = tried.all[rested - 1];
            var after = tried.all[rested];
            Debug.Log($"PHYSICAL_TIRED first: {first.speed:F1} m/s, upper hand {first.choked:P0}, arms {first.liftEffort:P0}; swing {rested}, before its rest: {last.spent:P0} spent, {last.speed:F1} m/s, upper hand {last.choked:P0}, arms {last.liftEffort:P0}; "
                + $"after it: {after.spent:P0} spent, {after.speed:F1} m/s");
            Assert.That(last.spent, Is.InRange(PhysicalSwing.RestsAt - .06f, PhysicalSwing.RestsAt + .03f));
            Assert.That(last.speed, Is.LessThan(first.speed * .9f), "A tired blow should land more weakly.");
            Assert.That(last.choked, Is.GreaterThan(first.choked + .2f), "Tired, it should take the tool nearer the head.");
            Assert.That(last.liftEffort, Is.GreaterThan(first.liftEffort + .1f), "Tired, the same lift should take more of what is left.");
            Assert.That(after.spent, Is.LessThan(PhysicalSwing.GoesOnAt + .06f), "It went on before it was rested.");
            Assert.That(after.speed, Is.GreaterThan(last.speed + .2f), "Rested, it should strike harder again.");
            Assert.That(tried.all.TrueForAll(r => r.struck), Is.True, "A swing did not strike.");
            Assert.That(tried.miss, Is.LessThan(.04f), "A hand came off the handle.");
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
            Debug.Log($"PHYSICAL_RATE as fast as it runs: {free.first.liftEffort:P0}, through upright at {free.first.upright:F2} m/s, struck at {free.first.speed:F2} m/s, upper hand {free.first.choked:P0}, second blow {free.second.upright:F2} and {free.second.speed:F2} m/s; "
                + $"at 25 frames a second: {slow.first.liftEffort:P0}, through upright at {slow.first.upright:F2} m/s, struck at {slow.first.speed:F2} m/s, upper hand {slow.first.choked:P0}, second blow {slow.second.upright:F2} and {slow.second.speed:F2} m/s");
            Assert.That(slow.first.struck && free.first.struck, Is.True);
            Assert.That(slow.first.liftEffort, Is.EqualTo(free.first.liftEffort).Within(.06f), "Raising the pickaxe took a different effort at another frame rate.");
            // On its way down the tool goes as fast at any frame rate. Its speed as it strikes is read at the last step
            // before the blow, and can differ by what the head gains in a step.
            Assert.That(slow.first.upright, Is.EqualTo(free.first.upright).Within(.25f), "The tool came down at a different speed at another frame rate.");
            Assert.That(slow.second.upright, Is.EqualTo(free.second.upright).Within(.25f), "The tool came down at a different speed at another frame rate.");
            Assert.That(slow.first.speed, Is.EqualTo(free.first.speed).Within(.8f), "The blow arrived at a different speed at another frame rate.");
            Assert.That(slow.first.liftTime, Is.EqualTo(free.first.liftTime).Within(.08f));
        }
    }
}
