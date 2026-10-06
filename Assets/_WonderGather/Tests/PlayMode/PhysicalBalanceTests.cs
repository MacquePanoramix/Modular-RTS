using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, step 6: a miner keeps its own balance (PhysicalBalance). At ease it stands as it did. Pulled a little, it
    // leans against the pull and keeps its feet where they are. Pulled hard, it steps, and stands set against the
    // pull. A heavier body takes more pulling than a lighter one. Bent knees work the legs, and weak legs raise the
    // body slowly.
    public sealed class PhysicalBalanceTests
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

        [TearDown]
        public void Restore() { Time.captureFramerate = 0; }

        private Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        private int Index(string name)
        {
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == name) return i;
            Assert.Fail("No miner is called " + name);
            return -1;
        }

        private struct Pulled
        {
            // While it was pulled: the steps it took, how near the edge of its feet its weight's point came (metres;
            // negative: outside), the furthest its hips leaned, and how its hips leaned along the pull when the pull
            // had lasted (metres; negative: against it).
            public int steps;
            public float margin, leaned, along;
            // How far apart its feet stood while it was pulled, and standing at ease before; how far it had gone when
            // the pull let go (along the pull); and how far apart its feet were, and how far its hips leaned, some
            // seconds after.
            public float apart, apartAtEase, went, apartAfter, leanAfter;
        }

        // A miner stands, keeping its own balance; something pulls it at the chest, that hard (newtons), that way round
        // it (degrees: 0 forwards, 90 to its right), for two seconds and a half; then lets go.
        private IEnumerator Pull(string who, float newtons, float way, System.Action<Pulled> done)
        {
            choice.Choose(Index(who));
            yield return Wait(.2f);
            var unit = choice.Current;
            var miner = unit.GetComponent<MinerBody>();
            var biped = unit.GetComponent<ProceduralBiped>();
            unit.Motor.Stop();
            unit.GetComponent<NavMeshAgent>().Warp(spot);
            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
            biped.ResetPose();
            yield return Wait(.6f);
            float height = miner.Rig.head.position.y - unit.transform.position.y;
            var balance = unit.gameObject.AddComponent<PhysicalBalance>();
            yield return Wait(.8f);
            var pulled = new Pulled { apartAtEase = Vector3.Distance(biped.FootPosition(0), biped.FootPosition(1)) };
            balance.Mark();
            Vector3 towards = Quaternion.AngleAxis(way, Vector3.up) * away, first = unit.transform.position;
            float began = Time.time;
            var pull = unit.gameObject.AddComponent<BalancePull>();
            pull.balance = balance;
            pull.force = () => towards * (newtons * Mathf.Clamp01((Time.time - began) / .35f));
            pull.at = () => biped.HipsNow + Vector3.up * (.3f * height);
            yield return Wait(2.5f);
            pulled.steps = balance.Steps; pulled.margin = balance.LeastMargin; pulled.leaned = balance.MostLean;
            Vector2 lean = balance.Lean;
            Vector3 leans = unit.transform.right * lean.x + unit.transform.forward * lean.y;
            pulled.along = Vector3.Dot(leans, towards);
            pulled.apart = Vector3.Distance(biped.FootPosition(0), biped.FootPosition(1));
            pulled.went = Vector3.Dot(unit.transform.position - first, towards);
            Object.Destroy(pull);
            yield return Wait(4.5f);
            pulled.apartAfter = Vector3.Distance(biped.FootPosition(0), biped.FootPosition(1));
            pulled.leanAfter = balance.Lean.magnitude;
            Object.Destroy(balance);
            yield return Wait(.2f);
            done(pulled);
        }

        // At ease, keeping its own balance changes nothing: it stands as it is posed.
        [UnityTest]
        public IEnumerator AtEaseItStandsAsItDid()
        {
            for (int index = 0; index < choice.Count; index++)
            {
                Pulled none = default;
                string name = choice.NameOf(index);
                yield return Pull(name, 0, 0, p => none = p);
                Debug.Log($"BALANCE_EASE {name}: its hips leaned {none.leaned * 1000:F1} mm at most, its weight's point no nearer the edge of its feet than {none.margin * 1000:F0} mm, {none.steps} steps");
                Assert.That(none.steps, Is.EqualTo(0), name + " stepped with nothing pulling it.");
                Assert.That(none.leaned, Is.LessThan(.006f), name + " did not stand as it did.");
                Assert.That(none.margin, Is.GreaterThan(.04f), name + "'s weight is not well inside its feet standing at ease.");
                Assert.That(none.apartAfter, Is.EqualTo(none.apartAtEase).Within(.01f), name + "'s feet moved.");
            }
        }

        // The ladder: a light pull is leaned against, with the feet where they are; a hard one is stepped for, and the
        // body then stands set against it; when it lets go, the body comes back to standing at ease.
        [UnityTest, Timeout(300000)]
        public IEnumerator ALightPullIsLeanedAgainstAndAHardOneSteppedFor()
        {
            Pulled light = default, hard = default, aside = default;
            Time.captureFramerate = 50;
            yield return Pull("Round", 100, 0, p => light = p);
            yield return Pull("Round", 200, 0, p => hard = p);
            yield return Pull("Round", 180, 90, p => aside = p);
            Time.captureFramerate = 0;
            Debug.Log($"BALANCE_PULLED Round, 100 N forwards: {light.steps} steps, hips {light.along * 1000:F0} mm along the pull, margin {light.margin * 1000:F0} mm; 200 N forwards: {hard.steps} steps, went {hard.went:F2} m, feet {hard.apart:F2} m apart ({hard.apartAtEase:F2} at ease), {hard.apartAfter:F2} after; "
                + $"180 N to its right: {aside.steps} steps, went {aside.went:F2} m, feet {aside.apart:F2} m apart, {aside.apartAfter:F2} after, hips {aside.leanAfter * 1000:F0} mm off after");
            // A light pull: no step; the hips go back against it; the weight's point stays inside the feet.
            Assert.That(light.steps, Is.EqualTo(0), "It stepped for a light pull.");
            Assert.That(light.along, Is.LessThan(-.02f), "It did not lean against a light pull.");
            Assert.That(light.margin, Is.GreaterThan(0), "A light pull took its weight outside its feet.");
            Assert.That(light.leanAfter, Is.LessThan(.01f), "It did not come back to standing as it did.");
            // A hard pull: it steps, the way it is pulled, and stands with its feet apart while the pull lasts.
            Assert.That(hard.steps, Is.InRange(1, 3), "A hard pull should take a step or two.");
            Assert.That(hard.went, Is.GreaterThan(.03f), "It did not go the way it was pulled.");
            Assert.That(hard.apart, Is.GreaterThan(hard.apartAtEase + .05f), "It did not stand set against the pull.");
            // Let go, it brings its feet together again and stands at ease.
            Assert.That(hard.apartAfter, Is.EqualTo(hard.apartAtEase).Within(.04f), "Its feet did not come together again.");
            Assert.That(hard.leanAfter, Is.LessThan(.02f));
            // Sideways: it steps out to that side and stands wide.
            Assert.That(aside.steps, Is.InRange(1, 3));
            Assert.That(aside.went, Is.GreaterThan(.03f));
            Assert.That(aside.apart, Is.GreaterThan(aside.apartAtEase + .1f), "Pulled sideways, it did not set its feet wide.");
            Assert.That(aside.apartAfter, Is.EqualTo(aside.apartAtEase).Within(.04f));
        }

        // What makes one body steadier than another is real: the same pull moves a light body more than a heavy one.
        [UnityTest, Timeout(300000)]
        public IEnumerator AHeavierBodyIsSteadier()
        {
            Pulled small = default, round = default;
            Time.captureFramerate = 50;
            yield return Pull("Small", 110, 0, p => small = p);
            yield return Pull("Round", 110, 0, p => round = p);
            Time.captureFramerate = 0;
            Debug.Log($"BALANCE_BODIES 110 N forwards: Small {small.steps} steps, margin {small.margin * 1000:F0} mm, went {small.went:F2} m; Round {round.steps} steps, margin {round.margin * 1000:F0} mm, went {round.went:F2} m");
            Assert.That(round.steps, Is.EqualTo(0), "Round should hold this pull with its feet where they are.");
            Assert.That(small.steps, Is.GreaterThanOrEqualTo(1), "Small, less than half Round's weight, should have to step for it.");
            Assert.That(small.margin, Is.LessThan(round.margin));
        }

        // The legs have a strength of their own: bent knees work them, straight ones do not, and weaker legs raise the
        // body more slowly.
        [UnityTest, Timeout(300000)]
        public IEnumerator BentKneesWorkTheLegsAndWeakLegsRiseSlowly()
        {
            choice.Choose(Index("Round"));
            yield return Wait(.2f);
            var unit = choice.Current;
            var biped = unit.GetComponent<ProceduralBiped>();
            var physical = unit.GetComponent<PhysicalBody>();
            unit.Motor.Stop();
            unit.GetComponent<NavMeshAgent>().Warp(spot);
            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
            biped.ResetPose();
            yield return Wait(.6f);
            var balance = unit.gameObject.AddComponent<PhysicalBalance>();
            Time.captureFramerate = 50;
            float deep = .3f * biped.StandingHipHeight;
            var took = new float[2];
            var worked = new float[2];
            float standing = 0;
            for (int k = 0; k < 2; k++)
            {
                physical.Strength = k == 0 ? 1 : .5f;
                physical.Refresh();
                yield return Wait(.8f);
                if (k == 0) standing = balance.LegEffort;
                biped.Sink(deep);
                yield return Wait(2f);
                worked[k] = balance.LegEffort;
                Assert.That(biped.SinkNow, Is.EqualTo(deep).Within(.01f));
                biped.Sink(0);
                float began = Time.time;
                while (biped.SinkNow > .01f && Time.time - began < 8) yield return null;
                took[k] = Time.time - began;
            }
            Time.captureFramerate = 0;
            float spent = physical.Spent(PhysicalBody.Muscles.Legs);
            physical.Strength = 1;
            physical.Refresh();
            Object.Destroy(balance);
            Debug.Log($"BALANCE_LEGS Round standing: its knees give {standing:P0} of what they have; bent {deep:F2} m: {worked[0]:P0}, and at half strength {worked[1]:P0}; it rises in {took[0]:F2} s, and at half strength in {took[1]:F2} s; its legs were {spent:P0} spent");
            Assert.That(standing, Is.LessThan(.05f), "Standing straight should not work the legs.");
            Assert.That(worked[0], Is.InRange(.15f, .7f), "Bent knees should work the legs.");
            Assert.That(worked[1], Is.EqualTo(worked[0] * 2).Within(worked[0] * .35f), "Half as strong, the same bend should take twice the share.");
            Assert.That(took[1], Is.GreaterThan(took[0] * 1.25f), "Weaker legs should raise the body more slowly.");
            Assert.That(spent, Is.GreaterThan(.01f), "Working bent should tire the legs.");
        }
    }
}
