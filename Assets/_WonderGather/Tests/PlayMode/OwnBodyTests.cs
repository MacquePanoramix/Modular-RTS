using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3b, step 1: the body's own body, standing (Docs/Design/TheBodysOwn.md). Switched on the panel, a miner at ease
    // stands by what its own joints give: still; it keeps its feet when it is nudged; pushed harder than it can
    // stand, it goes over into the fall that is built, gets up, and stands so again; and sent somewhere it gives its
    // body back to the posed one, walks, and stands so again where it comes to.
    public sealed class OwnBodyTests
    {
        private MinerChoice choice;
        private MinerWorkPreview look;
        private OrdinaryGround ground;

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
        }

        [TearDown]
        public void Restore() { Time.captureFramerate = 0; }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);
        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }
        private Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);

        // A miner stood on the path, facing along it.
        private IEnumerator Stand(SelectableUnit unit, System.Action<Vector3, Vector3> at)
        {
            var a = ground.Path[4];
            var b = ground.Path[5];
            Vector3 spot = OnGround(new Vector3(a.x, 0, a.y));
            Vector3 away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
            away.y = 0;
            away.Normalize();
            unit.Motor.Stop();
            unit.GetComponent<NavMeshAgent>().Warp(spot);
            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
            unit.GetComponent<ProceduralBiped>().ResetPose();
            yield return Wait(.8f);
            at(spot, away);
        }

        // The panel's switch is turned on, and the miner's body becomes its own.
        private IEnumerator Own(SelectableUnit unit, string name, System.Action<OwnBody> got)
        {
            look.SetOwn(true);
            float began = Time.time;
            OwnBody own = null;
            while (Time.time - began < 4 && (own == null || !own.Stands)) { unit.TryGetComponent(out own); yield return null; }
            Assert.That(own != null && own.Stands, Is.True, name + " did not come to stand by its own joints within four seconds of the switch: " + look.Status());
            got(own);
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator EachMinerStandsByItsOwnJointsStill()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var biped = unit.GetComponent<ProceduralBiped>();
                Vector3 spot = default;
                yield return Stand(unit, (s, a) => { spot = s; });
                Vector3 posedHead = unit.GetComponent<MinerBody>().Solved.head.position;
                OwnBody own = null;
                yield return Own(unit, name, o => own = o);
                Assert.That(biped.LetGo, Is.True, name + ": the posed body still poses.");
                // It settles; then it is watched for twenty seconds.
                yield return Wait(5);
                Vector3 settled = own.HeadAt;
                float wander = 0, lowest = float.MaxValue, mostOutside = 0;
                for (float until = Time.time + 20; Time.time < until;)
                {
                    wander = Mathf.Max(wander, (own.HeadAt - settled).magnitude);
                    lowest = Mathf.Min(lowest, own.HeadAt.y);
                    mostOutside = Mathf.Max(mostOutside, own.Outside);
                    yield return new WaitForFixedUpdate();
                }
                float went = Flat(settled - posedHead).magnitude, sank = posedHead.y - lowest;
                Debug.Log($"OWN_STANDS {name}: stood 25 s by its own joints; settling, its head went {went * 1000:F1} mm from where the posed body had it and {sank * 1000:F1} mm lower; then it kept within {wander * 1000:F2} mm; its joints give: an ankle {own.Gave(OwnBody.Foot):F0} and {own.Gave(OwnBody.Foot + 1):F0} N m, a knee {own.Gave(OwnBody.Shin):F0} and {own.Gave(OwnBody.Shin + 1):F0}, a hip {own.Gave(OwnBody.Thigh):F0} and {own.Gave(OwnBody.Thigh + 1):F0}; the push it wanted was at most {mostOutside * 1000:F0} mm outside its soles. The panel says: {look.Status()}");
                Assert.That(own.Stands, Is.True, name + " did not go on standing by its own joints.");
                Assert.That(own.WentDown, Is.EqualTo(0), name + " went down, standing.");
                Assert.That(went, Is.LessThan(.03f), name + " settled far from where the posed body stood.");
                Assert.That(sank, Is.LessThan(.03f), name + " sank.");
                Assert.That(wander, Is.LessThan(.002f), name + " does not stand still.");
                Assert.That(look.Status(), Is.EqualTo("It stands by its own joints."));
                // Switched off, the posed body has it again.
                look.SetOwn(false);
                yield return Wait(.6f);
                Assert.That(own.Stands, Is.False, name + " still stands by its own joints with the switch off.");
                Assert.That(biped.LetGo, Is.False, name + ": the posed body does not pose again.");
                float back = Flat(unit.GetComponent<MinerBody>().Solved.head.position - posedHead).magnitude;
                Assert.That(back, Is.LessThan(.02f), name + ": the posed body did not come back to where it stood.");
            }
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator NudgedItKeepsItsFeet()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                Vector3 away = default;
                yield return Stand(unit, (s, a) => { away = a; });
                OwnBody own = null;
                yield return Own(unit, name, o => own = o);
                yield return Wait(3);
                Assert.That(look.CanNudge, Is.True, name + " cannot be nudged from the panel.");
                for (int d = 0; d < 4; d++)
                {
                    Vector3 way = Quaternion.AngleAxis(d * 90, Vector3.up) * away;
                    Vector3 before = own.HeadAt;
                    look.Nudge(way, .15f);
                    float furthest = 0;
                    for (float until = Time.time + 5; Time.time < until;)
                    {
                        if (!own.Stands) break;
                        furthest = Mathf.Max(furthest, Flat(own.HeadAt - before).magnitude);
                        yield return new WaitForFixedUpdate();
                    }
                    Assert.That(own.Stands && own.WentDown == 0, Is.True, $"{name}, nudged at 0.15 m/s {d * 90} degrees from ahead, went down.");
                    float ended = Flat(own.HeadAt - before).magnitude;
                    Debug.Log($"OWN_NUDGED {name}, set going at 0.15 m/s {d * 90} degrees from ahead: its head went {furthest * 1000:F0} mm at most and is {ended * 1000:F1} mm from where it was, five seconds after");
                    Assert.That(ended, Is.LessThan(.03f), $"{name}, nudged {d * 90} degrees from ahead, did not come back to where it stood.");
                    Assert.That(own.HeadAt.y, Is.GreaterThan(before.y - .03f), name + " sank after a nudge.");
                }
                look.SetOwn(false);
                yield return Wait(.6f);
            }
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator PushedHarderThanItCanStandItFallsGetsUpAndStandsSoAgain()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                Vector3 away = default;
                yield return Stand(unit, (s, a) => { away = a; });
                OwnBody own = null;
                yield return Own(unit, name, o => own = o);
                yield return Wait(2);
                // Set going backwards at 1.2 m/s: far more than it can stand.
                look.Nudge(-away, 1.2f);
                float began = Time.time;
                while (own.WentDown == 0 && Time.time - began < 4) yield return null;
                Assert.That(own.WentDown, Is.EqualTo(1), name + ", set going at 1.2 m/s, did not go down.");
                Assert.That(own.Stands, Is.False);
                var fall = unit.GetComponent<PhysicalFall>();
                Assert.That(fall != null && fall.Now != PhysicalFall.State.Up, Is.True, name + " went down, and the fall did not take it.");
                float down = Time.time - began;
                while (fall.Now != PhysicalFall.State.Up && Time.time - began < 60) yield return null;
                Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), name + " did not get up.");
                float up = Time.time - began;
                while (!own.Stands && Time.time - began < 70) yield return null;
                Debug.Log($"OWN_FELL {name}, set going backwards at 1.2 m/s: it went down {down:F1} s after, into the fall; it was up {up:F1} s after, and stood by its own joints again {Time.time - began:F1} s after");
                Assert.That(own.Stands, Is.True, name + " did not stand by its own joints again after getting up: " + look.Status());
                yield return Wait(2);
                Assert.That(own.Stands && own.WentDown == 1, Is.True, name + " went down again after getting up.");
                look.SetOwn(false);
                yield return Wait(.6f);
            }
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator PushedOverFromThePanelItsBodyIsHandedToTheFall()
        {
            Time.captureFramerate = 50;
            choice.Choose(0);
            yield return Wait(.3f);
            var unit = choice.Current;
            string name = choice.NameOf(0);
            Vector3 away = default;
            yield return Stand(unit, (s, a) => { away = a; });
            OwnBody own = null;
            yield return Own(unit, name, o => own = o);
            yield return Wait(1);
            Assert.That(look.CanPushOver, Is.True);
            look.PushOver(-away);
            Assert.That(own.Stands, Is.False, name + " kept its own body while the fall took it.");
            var fall = unit.GetComponent<PhysicalFall>();
            Assert.That(fall != null && fall.Now == PhysicalFall.State.Falling, Is.True, name + " was not let go by the push.");
            float began = Time.time;
            while (fall.Now != PhysicalFall.State.Up && Time.time - began < 60) yield return null;
            Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), name + " did not get up.");
            while (!own.Stands && Time.time - began < 70) yield return null;
            Assert.That(own.Stands, Is.True, name + " did not stand by its own joints again.");
            look.SetOwn(false);
            yield return Wait(.6f);
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator SentSomewhereItGivesItsBodyBackWalksAndStandsSoAgain()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var biped = unit.GetComponent<ProceduralBiped>();
                Vector3 spot = default, away = default;
                yield return Stand(unit, (s, a) => { spot = s; away = a; });
                OwnBody own = null;
                yield return Own(unit, name, o => own = o);
                yield return Wait(2);
                Assert.That(unit.Motor.TryMove(OnGround(spot + away * 2.5f)), Is.True, name + " could not be sent along the path.");
                yield return null; yield return null;
                Assert.That(own.Stands, Is.False, name + " kept its own body when it was sent somewhere.");
                Assert.That(biped.LetGo, Is.False, name + ": the posed body does not walk.");
                float began = Time.time;
                while (unit.Motor.IsMoving && Time.time - began < 20) yield return null;
                Assert.That(Flat(unit.transform.position - spot).magnitude, Is.GreaterThan(2), name + " did not walk there.");
                while (!own.Stands && Time.time - began < 30) yield return null;
                Debug.Log($"OWN_WALKED {name}: sent 2.5 m along the path, it gave its body back, walked, and stood by its own joints again {Time.time - began:F1} s after it was sent");
                Assert.That(own.Stands, Is.True, name + " did not stand by its own joints again where it came to: " + look.Status());
                yield return Wait(3);
                Assert.That(own.Stands && own.WentDown == 0, Is.True, name + " went down after walking.");
                look.SetOwn(false);
                yield return Wait(.6f);
            }
        }
    }
}
