using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, step 10: the fall, and getting up. A body that cannot keep its feet is let go into the physics, with the
    // weights it was weighed at; it holds itself with what strength it has, lies, gathers itself, and gets up.
    public sealed class PhysicalFallTests
    {
        private MinerChoice choice;
        private MinerWorkPreview look;
        private InteractionClick click;
        private OrdinaryGround ground;

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
            click = Object.FindAnyObjectByType<InteractionClick>();
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

        // While it is down: how it lies, and whether its body holds together.
        private struct Down
        {
            public bool lay, sound;
            public float deepest, furthest, headLowest;
            public Vector3 where;
        }

        private IEnumerator Watch(PhysicalFall fall, float atMost, System.Action<Down> seen, System.Func<bool> each = null)
        {
            var down = new Down { sound = true, headLowest = float.MaxValue };
            float began = Time.time;
            while (fall.Now != PhysicalFall.State.Up && fall.Now != PhysicalFall.State.Rising && Time.time - began < atMost)
            {
                if (each != null) each();
                if (fall.Now == PhysicalFall.State.Lying) { down.lay = true; down.where = fall.HipsAt; }
                for (int i = 0; i < PhysicalFall.Count && fall.Part(i) != null; i++)
                {
                    Vector3 at = fall.Part(i).position;
                    down.sound &= float.IsFinite(at.x) && float.IsFinite(at.y) && float.IsFinite(at.z);
                    down.deepest = Mathf.Max(down.deepest, ground.Height(at.x, at.z) - at.y);
                    down.furthest = Mathf.Max(down.furthest, Vector3.Distance(at, fall.HipsAt));
                }
                if (fall.Part(PhysicalFall.Head) != null) down.headLowest = Mathf.Min(down.headLowest, fall.HeadAt.y - ground.Height(fall.HeadAt.x, fall.HeadAt.z));
                yield return new WaitForFixedUpdate();
            }
            seen(down);
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator AMinerLetGoFallsLiesAndGetsUp()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var miner = unit.GetComponent<MinerBody>();
                var biped = unit.GetComponent<ProceduralBiped>();
                var physical = unit.GetComponent<PhysicalBody>();
                var fall = unit.gameObject.AddComponent<PhysicalFall>();
                // Shoved forwards, then backwards.
                foreach (float way in new[] { 0f, 180f })
                {
                    Vector3 spot = default, away = default;
                    yield return Stand(unit, (s, a) => { spot = s; away = a; });
                    float tall = miner.Rig.head.position.y - unit.transform.position.y;
                    int fell = fall.Falls, got = fall.GotUp;
                    fall.LetGo();
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Falling), name + " was not let go.");
                    Assert.That(unit.Motor.IsOff, Is.True, "A body that falls is off the walked ground.");
                    // Its parts weigh what the body weighs.
                    float weighs = 0;
                    for (int i = 0; i < PhysicalFall.Count; i++) weighs += fall.Part(i).mass;
                    Assert.That(weighs, Is.EqualTo(physical.Mass).Within(.05f * physical.Mass), name + "'s let-go body does not weigh what it weighs.");
                    float began = Time.time;
                    Vector3 push = Quaternion.AngleAxis(way, Vector3.up) * away * (3 * physical.Mass);
                    Down down = default;
                    // (Since October 9 it lies by how hard it came down: up to ten seconds, where it lay 1.2.)
                    yield return Watch(fall, 24, d => down = d, () => { if (Time.time - began < .3f) fall.Push(push, fall.Part(PhysicalFall.Trunk).worldCenterOfMass); return true; });
                    float taken = Time.time - began;
                    Assert.That(down.sound, Is.True, name + "'s body came apart.");
                    Assert.That(down.lay, Is.True, name + " did not come to lie.");
                    Assert.That(down.deepest, Is.LessThan(.03f), name + " went into the ground.");
                    Assert.That(down.furthest, Is.LessThan(tall), name + "'s body did not hold together.");
                    Assert.That(down.headLowest, Is.LessThan(.45f * tall), name + " did not go down.");
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Rising).Or.EqualTo(PhysicalFall.State.Up), name + " did not begin to get up.");
                    // It gets up, and stands as it stood.
                    began = Time.time;
                    while ((fall.Now != PhysicalFall.State.Up || biped.SinkNow > .02f || Mathf.Abs(biped.BowNow) > 3) && Time.time - began < 8) yield return null;
                    yield return Wait(.5f);
                    float stands = miner.Rig.head.position.y - unit.transform.position.y;
                    float from = Flat(unit.transform.position - down.where).magnitude;
                    Debug.Log($"FALL_LETGO {name}, shoved {(way == 0 ? "forwards" : "backwards")}: down {taken:F1} s (lying, then gathering itself); its head no lower than {down.headLowest:F2} m over the ground, no part more than {down.deepest * 1000:F0} mm into it; up and standing {Time.time - began - .5f:F1} s later, {from:F2} m from where it lay, its head {stands:F2} m up (it is {tall:F2} m tall); it rose from a crouch {fall.RoseFrom:F2} m deep");
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), name + " did not get up.");
                    Assert.That(fall.Falls, Is.EqualTo(fell + 1));
                    Assert.That(fall.GotUp, Is.EqualTo(got + 1));
                    Assert.That(stands, Is.EqualTo(tall).Within(.05f * tall), name + " does not stand upright again.");
                    // (Since October 8 it turns over and draws its knees under it before it is up: it does not get up
                    // on the spot where it lay, but it does not go far.)
                    Assert.That(from, Is.LessThan(1.2f), name + " got up far from where it lay.");
                    Assert.That(unit.GetComponent<Collider>().enabled, Is.True);
                    // Sent somewhere, it goes: back to the walked ground first.
                    Vector3 to = OnGround(spot + away * 2);
                    Assert.That(unit.Motor.TryMove(to), Is.True, name + " would not walk after its fall.");
                    began = Time.time;
                    while (unit.Motor.IsMoving && Time.time - began < 15) yield return null;
                    Assert.That(unit.Motor.IsOff, Is.False);
                    Assert.That(Flat(unit.transform.position - to).magnitude, Is.LessThan(.6f), name + " did not go where it was sent after its fall.");
                }
                Object.Destroy(fall);
                yield return null;
            }
        }

        // Getting up is the let-go body's own doing (Luis, October 8: "getting up after a fall was just an animation...
        // supernatural"). Laid on its back, its front and either side, each miner turns itself over and draws its
        // knees under it while it is still the let-go body, and only then does the posed body take over: it is on its
        // front and on its knees then, not lying as it fell. It does not come to the old way (curled where it lies, the
        // posed body taking over from there), which is kept only for a body that has given up three times running.
        [UnityTest, Timeout(1800000)]
        public IEnumerator LaidDownAnyWayItGetsUpByItsOwnStrength()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var miner = unit.GetComponent<MinerBody>();
                var biped = unit.GetComponent<ProceduralBiped>();
                var fall = unit.gameObject.AddComponent<PhysicalFall>();
                foreach (string how in new[] { "back", "front", "left", "right" })
                {
                    yield return Stand(unit, (s, a) => { });
                    float tall = miner.Rig.head.position.y - unit.transform.position.y;
                    int got = fall.GotUp, old = fall.GotUpTheOldWay, gaveUp = fall.GaveUpGetting;
                    fall.LetGo();
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Falling), name + " was not let go.");
                    PhysicalFallBench.Lay(fall, unit.transform, ground, how);
                    float began = Time.time, lay = -1, roused = -1, over = -1, taken = -1, front = -2, steep = -2, highest = 0, fastest = 0, laidFront = -2;
                    bool sound = true;
                    while (fall.Now != PhysicalFall.State.Up && Time.time - began < 40)
                    {
                        if (fall.Part(PhysicalFall.Hips) != null)
                        {
                            if (lay < 0 && fall.Now == PhysicalFall.State.Lying) { lay = Time.time - began; laidFront = fall.Measure(PhysicalFall.Asks.Front); }
                            if (fall.Now == PhysicalFall.State.Gathering)
                            {
                                if (roused < 0) roused = Time.time - began;
                                // As it is now: when the posed body takes over, this is how it was the step before.
                                front = fall.Measure(PhysicalFall.Asks.Front); steep = fall.Measure(PhysicalFall.Asks.Steep);
                                if (over < 0 && front >= .6f) over = Time.time - began;
                                highest = Mathf.Max(highest, fall.Measure(PhysicalFall.Asks.Hips));
                                for (int i = PhysicalFall.Hips; i <= PhysicalFall.Head; i++) fastest = Mathf.Max(fastest, fall.Part(i).linearVelocity.magnitude);
                            }
                            for (int i = 0; i < PhysicalFall.Count; i++)
                            {
                                Vector3 at = fall.Part(i).position;
                                sound &= float.IsFinite(at.x) && float.IsFinite(at.y) && float.IsFinite(at.z);
                            }
                        }
                        else if (taken < 0 && fall.Now == PhysicalFall.State.Rising) taken = Time.time - began;
                        yield return new WaitForFixedUpdate();
                    }
                    float up = Time.time - began;
                    while ((biped.SinkNow > .02f || Mathf.Abs(biped.BowNow) > 3) && Time.time - began < 46) yield return null;
                    yield return Wait(.5f);
                    float stands = miner.Rig.head.position.y - unit.transform.position.y;
                    Debug.Log($"FALL_GETUP {name}, laid on its {how} (it lay with its chest to the ground by {laidFront:F2}): it began to get up at {roused:F1} s, had turned onto its front at {over:F1} s, and the posed body took over at {taken:F1} s, from a body with its chest to the ground by {front:F2} and its shins {steep:F2} steep; it was up at {up:F1} s and stands {stands:F2} m tall (of {tall:F2}); getting up, its hips were never higher than {highest:F2} of their standing height and no part of its trunk or head faster than {fastest:F1} m/s; it gave up {fall.GaveUpGetting - gaveUp} time(s) on the way, and got up the old way {fall.GotUpTheOldWay - old} time(s)");
                    Assert.That(sound, Is.True, name + "'s body came apart.");
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), $"{name}, laid on its {how}, did not get up in forty seconds.");
                    Assert.That(fall.GotUp, Is.EqualTo(got + 1));
                    Assert.That(fall.GotUpTheOldWay, Is.EqualTo(old), $"{name}, laid on its {how}, did not get up by its own strength: it came to the old way.");
                    Assert.That(front, Is.GreaterThanOrEqualTo(.2f), $"{name}, laid on its {how}: the posed body took over from a body that was not on its front.");
                    Assert.That(steep, Is.GreaterThanOrEqualTo(-.3f), $"{name}, laid on its {how}: the posed body took over from a body whose feet were in the air.");
                    Assert.That(stands, Is.EqualTo(tall).Within(.05f * tall), $"{name}, laid on its {how}, does not stand upright again.");
                    Assert.That(unit.GetComponent<Collider>().enabled, Is.True);
                }
                Object.Destroy(fall);
                yield return null;
            }
        }

        // The panel's "Push it over": the chosen miner, with nothing in its hands and no work, is let go with a shove,
        // falls, and gets itself up its own way; while it is down it cannot be pushed again.
        [UnityTest, Timeout(900000)]
        public IEnumerator PushedOverFromThePanelItFallsAndGetsItselfUp()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var biped = unit.GetComponent<ProceduralBiped>();
                Vector3 away = default;
                yield return Stand(unit, (s, a) => { away = a; });
                Assert.That(look.CanPushOver, Is.True, name + " cannot be pushed over as it stands.");
                look.PushOver(-away);
                var fall = unit.GetComponent<PhysicalFall>();
                Assert.That(fall, Is.Not.Null);
                Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Falling), name + " was not let go by the push.");
                Assert.That(look.CanPushOver, Is.False, name + " can be pushed over again while it is down.");
                float began = Time.time;
                bool lay = false;
                string said = "";
                while (fall.Now != PhysicalFall.State.Up && Time.time - began < 40)
                {
                    lay |= fall.Now == PhysicalFall.State.Lying;
                    if (fall.Now == PhysicalFall.State.Gathering) said = look.Status();
                    yield return null;
                }
                float up = Time.time - began;
                while ((biped.SinkNow > .02f || Mathf.Abs(biped.BowNow) > 3) && Time.time - began < 46) yield return null;
                Debug.Log($"FALL_PUSHED {name}, pushed over backwards from the panel: it lay, and was up {up:F1} s after the push; it gave up {fall.GaveUpGetting} time(s), and got up the old way {fall.GotUpTheOldWay} time(s). Getting up, the panel said: {said}");
                Assert.That(lay, Is.True, name + " did not come to lie.");
                Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), name + " did not get up after the push.");
                Assert.That(fall.GotUpTheOldWay, Is.EqualTo(0), name + " did not get up by its own strength.");
                Assert.That(said, Is.EqualTo("It gets itself up."));
                Assert.That(look.CanPushOver, Is.True);
                Object.Destroy(fall);
                yield return null;
            }
        }

        // How long a fallen miner lies before it begins to get up (Luis, October 9: by "how strong was the force that
        // pushed it down, its current stamina and anything else relevant"). Pushed down harder, it comes down harder
        // and lies longer; spent, it lies longer than fresh after the same push; and it never lies less than a moment
        // or longer than ten seconds.
        [UnityTest, Timeout(900000)]
        public IEnumerator AHarderFallAndASpentBodyLieLonger()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var physical = unit.GetComponent<PhysicalBody>();
                var fall = unit.gameObject.AddComponent<PhysicalFall>();
                fall.GetsUp = false;
                float[] blow = new float[3], lies = new float[3];
                // Pushed backwards lightly, hard, and lightly again with its legs and its back spent.
                for (int k = 0; k < 3; k++)
                {
                    Vector3 away = default;
                    yield return Stand(unit, (s, a) => { away = a; });
                    physical.Refresh();
                    if (k == 2)
                    {
                        physical.Worked(PhysicalBody.Muscles.Legs, 1, 60);
                        physical.Worked(PhysicalBody.Muscles.Back, 1, 60);
                    }
                    Vector3 push = -away * (k == 1 ? 12 : 3) * physical.Mass;
                    fall.LetGo();
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Falling), name + " was not let go.");
                    float began = Time.time;
                    while (fall.Now != PhysicalFall.State.Lying && Time.time - began < 10)
                    {
                        if (Time.time - began < .3f) fall.Push(push, fall.Part(PhysicalFall.Trunk).worldCenterOfMass);
                        yield return new WaitForFixedUpdate();
                    }
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Lying), name + " did not come to lie.");
                    blow[k] = fall.Blow; lies[k] = fall.LiesFor;
                    Debug.Log($"FALL_LIES {name}, pushed {(k == 1 ? "hard (12 N a kilogram)" : "lightly (3 N a kilogram)")}{(k == 2 ? ", its legs and back spent" : "")}: the blow {fall.Blow:F1} m/s (its head {fall.HeadStruck:F1}); shaken {fall.Shaken:F2}, dazed {fall.Dazed:F2}, out of breath {fall.OutOfBreath:F2}; it lies {fall.LiesFor:F1} s");
                    Assert.That(fall.LiesFor, Is.InRange(.5f, 10.01f), name + " lies less than a moment, or longer than ten seconds.");
                    fall.TakeBack();
                    yield return Wait(.3f);
                }
                physical.Refresh();
                Assert.That(blow[1], Is.GreaterThan(blow[0] + .5f), name + ", pushed four times as hard, did not come down harder.");
                Assert.That(lies[1], Is.GreaterThan(lies[0] + .5f), name + ", having come down harder, does not lie longer.");
                Assert.That(lies[2], Is.GreaterThan(lies[0] + 1), name + ", spent, does not lie longer than fresh.");
                Object.Destroy(fall);
                yield return null;
            }
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator AHardPullThrowsItDownAndALightOneDoesNot()
        {
            Time.captureFramerate = 50;
            // A pull at the chest for a second and a half: a quarter of its weight, then four fifths of it.
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var miner = unit.GetComponent<MinerBody>();
                var biped = unit.GetComponent<ProceduralBiped>();
                var physical = unit.GetComponent<PhysicalBody>();
                foreach (float share in new[] { .25f, .8f })
                {
                    Vector3 spot = default, away = default;
                    yield return Stand(unit, (s, a) => { spot = s; away = a; });
                    float tall = miner.Rig.head.position.y - unit.transform.position.y;
                    var fall = unit.gameObject.AddComponent<PhysicalFall>();
                    var balance = unit.gameObject.AddComponent<PhysicalBalance>();
                    yield return Wait(.8f);
                    balance.Mark();
                    float newtons = share * physical.Mass * Physics.gravity.magnitude, began = Time.time;
                    var pull = unit.gameObject.AddComponent<BalancePull>();
                    pull.balance = balance;
                    pull.force = () => Time.time - began > 1.85f ? Vector3.zero : away * newtons * Mathf.Clamp01((Time.time - began) / .35f);
                    pull.at = () => biped.HipsNow + Vector3.up * (.3f * tall);
                    float fellAt = -1;
                    while (Time.time - began < 4.5f)
                    {
                        if (fellAt < 0 && fall.Now != PhysicalFall.State.Up) fellAt = Time.time - began;
                        yield return null;
                    }
                    Debug.Log($"FALL_PULLED {name} ({physical.Mass:F0} kg) pulled with {newtons:F0} N ({share * 100:F0}% of its weight) for 1.85 s: {(fellAt < 0 ? "kept its feet" : $"fell after {fellAt:F2} s ({balance.Fell})")}; it took {balance.Steps} steps, {balance.MostShort} in a row landing short at most");
                    if (share < .5f)
                    {
                        Assert.That(fall.Falls, Is.EqualTo(0), name + " fell to a pull of a quarter of its weight.");
                        Assert.That(miner.Rig.head.position.y - unit.transform.position.y, Is.GreaterThan(.85f * tall), name + " is not on its feet.");
                    }
                    else
                    {
                        Assert.That(fall.Falls, Is.GreaterThanOrEqualTo(1), name + " kept its feet against four fifths of its weight.");
                        Assert.That(balance.Fell, Is.Not.Empty);
                        // It gets up again.
                        float waited = Time.time;
                        while ((fall.Now != PhysicalFall.State.Up || biped.SinkNow > .02f) && Time.time - waited < 30) yield return null;
                        Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), name + " did not get up after being pulled down.");
                    }
                    Object.Destroy(pull);
                    Object.Destroy(balance);
                    Object.Destroy(fall);
                    yield return Wait(.3f);
                }
            }
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator AMinerThatFallsAtWorkLetsItsPickaxeGo()
        {
            Time.captureFramerate = 50;
            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Round") index = i;
            choice.Choose(index);
            yield return Wait(.3f);
            var unit = choice.Current;
            yield return Stand(unit, (s, a) => { });
            look.Toggle();
            float began = Time.time;
            while (!look.Swinging && Time.time - began < 8) yield return null;
            Assert.That(look.Swinging, Is.True);
            var pickaxe = unit.GetComponent<PhysicalHands>().Thing;
            Assert.That(look.Fall, Is.Not.Null, "A miner at its physical work has a body that can be let go.");
            yield return Wait(1);
            look.Fall.LetGo();
            yield return Wait(2.5f);
            // Its pickaxe has left its hands and lies in the world; its work is over.
            Assert.That(pickaxe != null && pickaxe.Holder == null, Is.True, "The pickaxe is still in its hands.");
            Assert.That(look.Lying, Is.EqualTo(pickaxe), "The look does not know where the pickaxe lies.");
            Assert.That(pickaxe.GetComponent<Rigidbody>().linearVelocity.magnitude, Is.LessThan(.3f), "The pickaxe did not come to lie.");
            Assert.That(look.Swinging, Is.False);
            Assert.That(GameObject.Find("Block (a look at the work)"), Is.Null);
            // It gets up; then the look ends by itself, and the pickaxe can be picked up.
            began = Time.time;
            while (look.Showing && Time.time - began < 30) yield return null;
            Assert.That(look.Showing, Is.False, "The look did not end when it was up again.");
            Assert.That(look.Fall == null || look.Fall.Now == PhysicalFall.State.Up, Is.True);
            yield return Wait(.6f);
            Assert.That(look.Lying, Is.EqualTo(pickaxe));
            Assert.That(click.OpenOn(pickaxe) && click.Choose("Pick it up"), Is.True);
            began = Time.time;
            PhysicalHands hands = null;
            float said = Time.time;
            while (Time.time - began < 30)
            {
                hands = unit.GetComponent<PhysicalHands>();
                if (hands != null && look.Carrying && !look.Carry.Fetching && hands.Thing == pickaxe && hands.Holds(0)) break;
                if (Time.time - said > 2)
                {
                    said = Time.time;
                    Debug.Log($"FALL_AT_WORK   {Time.time - began:F0}s: showing {look.Showing}, carrying {look.Carrying}, fetching {look.Carry != null && look.Carry.Fetching}, way {(look.Carry != null ? look.Carry.way.ToString() : "-")}, hands {(hands != null ? (hands.Thing == pickaxe ? "on it" : hands.Thing == null ? "empty" : "other") : "none")}, moving {unit.Motor.IsMoving}, off {unit.Motor.IsOff}, {Vector3.Distance(unit.transform.position, pickaxe.transform.position):F2} m from the pickaxe, fall {(look.Fall != null ? look.Fall.Now.ToString() : "-")}, bow {unit.GetComponent<ProceduralBiped>().BowNow:F0}, sink {unit.GetComponent<ProceduralBiped>().SinkNow:F2}");
                }
                yield return null;
            }
            Assert.That(hands != null && hands.Thing == pickaxe && hands.Holds(0), Is.True, "It did not pick its pickaxe up after its fall.");
            Debug.Log($"FALL_AT_WORK Round, let go at its work: its pickaxe lay in the world, the look ended when it was up again, and it picked the pickaxe up {Time.time - began:F1} s after the order");
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator AWeakMinerWithAPickaxeTooHeavyGoesDownAndGetsUp()
        {
            Time.captureFramerate = 50;
            int index = -1;
            // (Round: since step 11 a body rests with its knees bent no further than they can hold, and Small, as weak
            // and as laden, no longer falls in a minute and a half. Round's legs still give way, later than Small's
            // did.)
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Round") index = i;
            choice.Choose(index);
            yield return Wait(.3f);
            var unit = choice.Current;
            var biped = unit.GetComponent<ProceduralBiped>();
            yield return Stand(unit, (s, a) => { });
            // Half its strength, and a pickaxe three times its weight: its legs give way under the work.
            look.SetStrength(.5f); look.SetWeight(3);
            look.Toggle();
            float began = Time.time, fellAt = -1;
            string why = "";
            float said = 0;
            string gaveUp = "";
            while (Time.time - began < 100 && look.Showing)
            {
                if (look.Fall != null && look.Fall.Now != PhysicalFall.State.Up) { fellAt = Time.time - began; why = unit.GetComponent<PhysicalBalance>() != null ? unit.GetComponent<PhysicalBalance>().Fell : ""; break; }
                string says = look.Status();
                if (says.Contains("slipped from its hands") || says.Contains("cannot get its strength back")) gaveUp = says;
                if (Time.time - began > said + 10) { said = Time.time - began; Debug.Log($"FALL_WEAK {said:F0} s: {says}"); }
                yield return null;
            }
            if (fellAt < 0)
            {
                // It did not go down. Since October 8 a body knows when holding its tool is no rest for it, and a tool
                // too heavy for the hold its hands have left slips from them: its work ends by itself before its
                // legs give way, its pickaxe lies in the world, and the panel has said why.
                Assert.That(look.Showing, Is.False, $"In a hundred seconds it neither fell nor gave its work up; the panel says: {look.Status()}");
                Assert.That(gaveUp, Is.Not.Empty, "Its work ended, and the panel did not say why.");
                Assert.That(look.Lying, Is.Not.Null, "Its pickaxe does not lie in the world.");
                Debug.Log($"FALL_WEAK Round, at half its strength with a pickaxe three times its weight: it did not fall; its work ended after {Time.time - began:F1} s, the pickaxe lying in the world. The panel said: {gaveUp}");
                look.SetStrength(1); look.SetWeight(1);
                yield break;
            }
            Assert.That(why, Is.EqualTo("its legs cannot bear it"));
            var fall = look.Fall;
            began = Time.time;
            while ((fall.Now != PhysicalFall.State.Up || biped.SinkNow > .02f) && Time.time - began < 40) yield return null;
            float upAfter = Time.time - began;
            yield return Wait(2);
            Debug.Log($"FALL_WEAK Round, at half its strength with a pickaxe three times its weight: its legs gave way after {fellAt:F1} s of work; it was up again {upAfter:F1} s later, from a crouch {fall.RoseFrom:F2} m deep, having lain down again {fall.LayDownAgain} time(s); it fell {fall.Falls} time(s)");
            Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), "It did not get up.");
            Assert.That(fall.Falls, Is.LessThanOrEqualTo(2), "It went on falling.");
            // Up, its hands empty, the look ends by itself when it stands straight. A body this weak, its back spent by
            // the work, may not get its trunk up again: it stands bent, and the panel says so. Made as strong as it was
            // built, it straightens, and the look ends.
            if (look.Showing)
            {
                Assert.That(biped.BowNow, Is.GreaterThan(30), $"The look did not end when it was up again, and it does not stand bent: it bows {biped.BowNow:F1} degrees, its knees are bent {biped.SinkNow:F3} m; the panel says: {look.Status()}");
                Assert.That(look.Status(), Does.Contain("its back does not raise it"), "The panel does not say why it stands bent.");
                Debug.Log($"FALL_WEAK up again, it stands bent {biped.BowNow:F0} degrees: its back, at half its strength and spent, does not raise it. The panel says: {look.Status()}");
                look.SetStrength(1);
                began = Time.time;
                while (look.Showing && Time.time - began < 30) yield return null;
                Debug.Log($"FALL_WEAK as strong as it was built again, it stood straight and the look ended {Time.time - began:F1} s later");
            }
            Assert.That(look.Showing, Is.False, $"The look did not end when it was up again: it bows {biped.BowNow:F1} degrees, its knees are bent {biped.SinkNow:F3} m; the panel says: {look.Status()}");
            Assert.That(look.Lying, Is.Not.Null, "Its pickaxe does not lie in the world.");
            look.SetStrength(1); look.SetWeight(1);
        }
    }
}
