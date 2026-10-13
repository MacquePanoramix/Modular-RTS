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
        private GameObject floorLaid;

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
        public void Restore() { OwnBody.Told = null; OwnBody.StaysPut = false; OwnBody.AnyGround = false; Time.captureFramerate = 0; OwnBody.Alive = true; OwnBody.BreathShown = OwnBody.BreathDrawn; OwnBody.BreathShoulders = 1; OwnBody.BreathSeenInAir = true; OwnBody.BreathSize = OwnBody.BreathSizeAtFirst; }

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

        // The miner put on the path in its plain stand, the body's own not yet wanted: its feet set as they are made
        // (a fall and getting up leave them set apart), and a moment for its place to settle on the walked ground
        // (put there by hand it is 2 to 3 cm low at first, and a body's own made then keeps that as its stand; the
        // game never puts a miner anywhere by hand).
        private IEnumerator StandPlainly(SelectableUnit unit, System.Action<Vector3, Vector3> at)
        {
            look.SetOwn(false);
            unit.GetComponent<ProceduralBiped>().SetStance(0, 0);
            yield return Stand(unit, at);
            yield return Wait(1.5f);
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
            // (The keeper alone, with no life in the body: what it keeps still.)
            OwnBody.Alive = false;
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
                Assert.That(look.Status(), Does.StartWith("It stands by its own joints."));
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
            // (The keeper alone: with life in it, it is nudged in the test of that.)
            OwnBody.Alive = false;
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
            OwnBody.Told = what => Debug.Log("OWN_TOLD " + what);
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
                // Set going backwards at 2 m/s: far more than it can stand, or catch with a step. (It was 1.2 m/s until
                // October 13: with the step on, Long catches itself at that.)
                look.Nudge(-away, 2);
                float began = Time.time;
                while (own.WentDown == 0 && Time.time - began < 4) yield return null;
                Assert.That(own.WentDown, Is.EqualTo(1), name + ", set going at 2 m/s, did not go down.");
                Assert.That(own.Stands, Is.False);
                var fall = unit.GetComponent<PhysicalFall>();
                Assert.That(fall != null && fall.Now != PhysicalFall.State.Up, Is.True, name + " went down, and the fall did not take it.");
                float down = Time.time - began;
                while (fall.Now != PhysicalFall.State.Up && Time.time - began < 60) yield return null;
                Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), name + " did not get up.");
                float up = Time.time - began;
                while (!own.Stands && Time.time - began < 70) yield return null;
                Debug.Log($"OWN_FELL {name}, set going backwards at 2 m/s: it went down {down:F1} s after, into the fall; it was up {up:F1} s after, and stood by its own joints again {Time.time - began:F1} s after");
                Assert.That(own.Stands, Is.True, name + " did not stand by its own joints again after getting up: " + look.Status());
                yield return Wait(2);
                Assert.That(own.Stands && own.WentDown == 1, Is.True, name + " went down again after getting up.");
                // (And shoved from behind where it stands, as it got up, it catches itself: it stands by its own joints
                // from its plain stand, not as its getting up left its feet.)
                {
                    Transform left = own.Part(OwnBody.Foot).transform, right = own.Part(OwnBody.Foot + 1).transform;
                    float apart = Flat(right.position - left.position).magnitude;
                    look.Nudge(unit.transform.forward, MinerWorkPreview.ShoveSpeed);
                    for (float until = Time.time + 6; Time.time < until && own.Stands && own.WentDown == 1;) yield return new WaitForFixedUpdate();
                    Debug.Log($"OWN_FELL {name}, shoved from behind at {MinerWorkPreview.ShoveSpeed:F1} m/s after it had got up (its boots {apart * 1000:F0} mm apart): " + (own.Stands && own.WentDown == 1 ? $"it stands, after {own.Stepped} steps" : "it went down"));
                    Assert.That(own.Stands && own.WentDown == 1, Is.True, name + ", shoved after it had got up, went down.");
                }
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
            OwnBody.Told = what => Debug.Log("OWN_TOLD " + what);
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

        // The life in it: standing a minute it breathes, its weight goes from leg to leg, its head looks about; and
        // it is still stable (it does not go down, its head keeps within a few centimetres, nothing trembles).
        [UnityTest, Timeout(1800000)]
        public IEnumerator StandingAMinuteItIsAliveAndDoesNotWobble()
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
                Vector3 right = Vector3.Cross(Vector3.up, away);
                OwnBody own = null;
                yield return Own(unit, name, o => own = o);
                yield return Wait(1.5f);
                Transform hips = own.Part(OwnBody.Hips).transform, trunk = own.Part(OwnBody.Trunk).transform, head = own.Part(OwnBody.Head).transform;
                Vector3 hipsBegan = hips.position, headBegan = head.position;
                float headFaced = Vector3.SignedAngle(away, Flat(head.forward), Vector3.up);
                int breathsBegan = own.BreathsTaken;
                // What is measured over the minute.
                float leanFull = 0, leanEmpty = 0; int full = 0, empty = 0;
                float hipsRight = 0, hipsLeft = 0, rollRight = 0, rollLeft = 0, kneeFree = 0, kneeStood = 0; int onRight = 0, onLeft = 0;
                float headWent = 0, lowest = float.MaxValue, lookedMost = 0, lookOff = 0; int lookSeen = 0;
                float headAcross = 0, headAlong = 0, hipsAcross = 0, hipsAlong = 0, chestAcross = 0, chestAlong = 0, hipsTurned = 0;
                Vector3 chestBegan = trunk.position; float hipsFaced = Vector3.SignedAngle(away, Flat(hips.forward), Vector3.up);
                int shifts = 0, turnsBack = 0; float favoured = 0, lastSide = 0, lastHead = 0, wayHead = 0;
                float Knee(int i) => Vector3.Angle(own.Part(OwnBody.Thigh + i).transform.up, own.Part(OwnBody.Shin + i).transform.up);
                for (float until = Time.time + 60; Time.time < until;)
                {
                    Assert.That(own.Stands, Is.True, $"{name} did not go on standing with life in it ({60 - (until - Time.time):F1} s): {look.Status()}");
                    float lean = Mathf.Asin(Mathf.Clamp(Vector3.Dot(trunk.up, away), -1, 1)) * Mathf.Rad2Deg;
                    if (own.BreathFull > .8f) { leanFull += lean; full++; } else if (own.BreathFull < .2f) { leanEmpty += lean; empty++; }
                    float aside = Vector3.Dot(hips.position - hipsBegan, right), roll = Mathf.Asin(Mathf.Clamp(hips.right.y, -1, 1)) * Mathf.Rad2Deg;
                    if (own.Favours > .5f) { hipsRight += aside; rollRight += roll; kneeFree += Knee(0); kneeStood += Knee(1); onRight++; }
                    else if (own.Favours < -.5f) { hipsLeft += aside; rollLeft += roll; kneeFree += Knee(1); kneeStood += Knee(0); onLeft++; }
                    float side = own.Favours > .3f ? 1 : own.Favours < -.3f ? -1 : lastSide;
                    if (side != lastSide && lastSide != 0) shifts++;
                    lastSide = side;
                    favoured = Mathf.Max(favoured, Mathf.Abs(own.Favours));
                    headWent = Mathf.Max(headWent, Flat(head.position - headBegan).magnitude);
                    headAcross = Mathf.Max(headAcross, Mathf.Abs(Vector3.Dot(head.position - headBegan, right))); headAlong = Mathf.Max(headAlong, Mathf.Abs(Vector3.Dot(head.position - headBegan, away)));
                    hipsAcross = Mathf.Max(hipsAcross, Mathf.Abs(Vector3.Dot(hips.position - hipsBegan, right))); hipsAlong = Mathf.Max(hipsAlong, Mathf.Abs(Vector3.Dot(hips.position - hipsBegan, away)));
                    chestAcross = Mathf.Max(chestAcross, Mathf.Abs(Vector3.Dot(trunk.position - chestBegan, right))); chestAlong = Mathf.Max(chestAlong, Mathf.Abs(Vector3.Dot(trunk.position - chestBegan, away)));
                    hipsTurned = Mathf.Max(hipsTurned, Mathf.Abs(Mathf.DeltaAngle(hipsFaced, Vector3.SignedAngle(away, Flat(hips.forward), Vector3.up))));
                    lowest = Mathf.Min(lowest, head.position.y);
                    // A tremble: the head going back and forth across the body (turnings of more than a third of a millimetre).
                    float across = Vector3.Dot(head.position - headBegan, right);
                    if (wayHead >= 0 && across < lastHead - .0003f) { if (wayHead > 0) turnsBack++; wayHead = -1; lastHead = across; }
                    else if (wayHead <= 0 && across > lastHead + .0003f) { if (wayHead < 0) turnsBack++; wayHead = 1; lastHead = across; }
                    else if (wayHead > 0) lastHead = Mathf.Max(lastHead, across); else if (wayHead < 0) lastHead = Mathf.Min(lastHead, across);
                    float faces = Mathf.DeltaAngle(headFaced, Vector3.SignedAngle(away, Flat(head.forward), Vector3.up));
                    lookedMost = Mathf.Max(lookedMost, Mathf.Abs(own.Looks.x));
                    lookOff += Mathf.Abs(Mathf.DeltaAngle(faces, own.Looks.x)); lookSeen++;
                    yield return new WaitForFixedUpdate();
                }
                int breaths = own.BreathsTaken - breathsBegan;
                float straightens = full > 0 && empty > 0 ? leanEmpty / empty - leanFull / full : 0;
                Debug.Log($"OWN_ALIVE {name}, a minute: {breaths} breaths, its back {straightens:F2} degrees straighter full than empty; its weight went to the other leg {shifts} times (favouring one by {favoured:F2} at most): on its right leg its hips are {(onRight > 0 ? hipsRight / onRight * 1000 : 0):F0} mm to the right and roll {(onRight > 0 ? rollRight / onRight : 0):F1} degrees (right side up), on its left {(onLeft > 0 ? hipsLeft / onLeft * 1000 : 0):F0} mm and {(onLeft > 0 ? rollLeft / onLeft : 0):F1}; the knee of the leg it stands on is bent {(onRight + onLeft > 0 ? kneeStood / (onRight + onLeft) : 0):F1} degrees, the other {(onRight + onLeft > 0 ? kneeFree / (onRight + onLeft) : 0):F1}; its head went {headWent * 1000:F0} mm at most ({headAcross * 1000:F0} across, {headAlong * 1000:F0} along; its chest {chestAcross * 1000:F0} and {chestAlong * 1000:F0}; its hips {hipsAcross * 1000:F0} and {hipsAlong * 1000:F0}, turning {hipsTurned:F1} degrees) and {(headBegan.y - lowest) * 1000:F0} mm lower, turned back and forth across it {turnsBack / 60f:F2} times a second, looked {lookedMost:F0} degrees aside at most and was {lookOff / Mathf.Max(1, lookSeen):F1} degrees from where it meant to look, on average. The panel says: {look.Status()}");
                Assert.That(own.WentDown, Is.EqualTo(0), name + " went down, standing with life in it.");
                Assert.That(breaths, Is.InRange(11, 19), name + " does not breathe as a body at rest does.");
                Assert.That(straightens, Is.GreaterThan(.15f), name + ": its back does not straighten as its chest fills.");
                Assert.That(shifts, Is.GreaterThanOrEqualTo(1), name + ": its weight did not go from leg to leg.");
                Assert.That(onRight > 0 && onLeft > 0 && hipsRight / onRight - hipsLeft / onLeft > .015f, Is.True, name + ": its hips do not go over the leg it favours.");
                Assert.That(onRight > 0 && onLeft > 0 && rollRight / onRight - rollLeft / onLeft > 1.5f, Is.True, name + ": the hip of the leg it stands on does not rise.");
                Assert.That(kneeFree - kneeStood, Is.GreaterThan(0), name + ": the knee of the leg it does not stand on does not ease.");
                // (Its weight going over a leg takes its hips, chest and head with it by some centimetres: that is meant.
                // What is not meant is more than that, or a tremble.)
                Assert.That(headAcross, Is.LessThan(.07f), name + ": its head goes further across it than its weight going over a leg takes it.");
                Assert.That(Mathf.Max(chestAcross, chestAlong), Is.LessThan(.06f), name + ": its chest wanders.");
                Assert.That(headBegan.y - lowest, Is.LessThan(.04f), name + " sinks.");
                Assert.That(turnsBack / 60f, Is.LessThan(2), name + " trembles.");
                Assert.That(lookedMost, Is.GreaterThan(10), name + " did not look about.");

                // It looks at a place it is asked to look at: forty degrees to its right, at the height of its head.
                Vector3 place = head.position + Quaternion.AngleAxis(40, Vector3.up) * away * 3;
                own.LookAt(place, 4);
                yield return Wait(2.5f);
                float turned = Mathf.DeltaAngle(headFaced, Vector3.SignedAngle(away, Flat(head.forward), Vector3.up));
                Debug.Log($"OWN_LOOKS {name}: asked to look 40 degrees to its right, its head is turned {turned:F1} degrees to its right, {Vector3.Angle(head.forward, place - head.position):F1} degrees from the place");
                Assert.That(turned, Is.InRange(25, 50), name + " did not turn its head to where it was asked to look.");
                yield return Wait(2);

                // Favouring a leg wholly, nudged each way, it keeps its feet.
                for (int d = 0; d < 4; d++)
                {
                    own.Favour(d < 2 ? 1 : -1);
                    float askedOf = 0;
                    for (float until = Time.time + 3; Time.time < until;) { askedOf = Mathf.Max(askedOf, own.Asked(d < 2 ? 1 : 0)); yield return new WaitForFixedUpdate(); }
                    float favouredBy = own.Favours;
                    Vector3 before = own.HeadAt;
                    look.Nudge(Quaternion.AngleAxis(d * 90 + 45, Vector3.up) * away, .15f);
                    float furthest = 0;
                    for (float until = Time.time + 5; Time.time < until && own.Stands;)
                    {
                        furthest = Mathf.Max(furthest, Flat(own.HeadAt - before).magnitude);
                        yield return new WaitForFixedUpdate();
                    }
                    Debug.Log($"OWN_ALIVE_NUDGED {name}, on its {(d < 2 ? "right" : "left")} leg (it favoured it by {favouredBy:F2}, that leg's joints asked for {askedOf * 100:F0}% of what they have at most), set going at 0.15 m/s {d * 90 + 45} degrees from ahead: its head went {furthest * 1000:F0} mm at most; it favours a leg by {own.Favours:F2} five seconds after");
                    Assert.That(own.Stands && own.WentDown == 0, Is.True, $"{name}, on one leg and nudged {d * 90 + 45} degrees from ahead, went down.");
                }

                // Tired at once, it breathes faster, and stands.
                look.Tire();
                yield return Wait(1);
                float tired = own.BreathsAMinute;
                yield return Wait(8);
                Debug.Log($"OWN_TIRED {name}: tired at once it breathes {tired:F0} times a minute; nine seconds after, {own.BreathsAMinute:F0}");
                Assert.That(tired, Is.GreaterThan(25), name + " does not breathe faster, tired.");
                Assert.That(own.Stands && own.WentDown == 0, Is.True, name + " went down, tired.");
                unit.GetComponent<PhysicalBody>().Refresh();
                look.SetOwn(false);
                yield return Wait(.6f);
            }
        }

        // Two more looks of breath (Luis, October 10): its shoulders drawn rising; and its breath seen in the air, from
        // dusk to dawn and not by day.
        [UnityTest, Timeout(900000)]
        public IEnumerator ItsBreathHasTwoMoreLooks()
        {
            Time.captureFramerate = 50;
            var time = Object.FindAnyObjectByType<TimeOfDay>();
            float hour = time.Hour;
            Assert.That(BreathInAir.Look() != null, Is.True, "The look of breath in the air (Resources/BreathInAir) was not found.");
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                yield return Stand(unit, (s, a) => { });
                OwnBody own = null;
                yield return Own(unit, name, o => own = o);
                var rig = unit.GetComponent<MinerBody>().Rig;
                // How high its shoulders are drawn over its chest, with its chest full and empty.
                float Shoulders() => .5f * (rig.upperArms[0].position.y + rig.upperArms[1].position.y) - rig.chest.position.y;
                float rises = 0;
                IEnumerator Breathes(float seconds)
                {
                    float full = 0, empty = 0; int fulls = 0, empties = 0;
                    for (float until = Time.time + seconds; Time.time < until;)
                    {
                        if (own.BreathFull > .9f) { full += Shoulders(); fulls++; } else if (own.BreathFull < .1f) { empty += Shoulders(); empties++; }
                        yield return null;
                    }
                    rises = fulls > 0 && empties > 0 ? full / fulls - empty / empties : 0;
                }
                look.SetBreathLook(1);
                yield return Breathes(9);
                float plain = rises;
                look.SetBreathLook(2);
                Assert.That(look.BreathLook, Is.EqualTo("in its shoulders"));
                yield return Breathes(9);
                float shrugged = rises;
                // In the air: nothing by day; puffs after dusk.
                look.SetBreathLook(3);
                time.Hour = 13;
                // (Every miner breathes all the while, this one too before its turn: what counts is what is breathed
                // out from now.)
                var air = unit.GetComponent<BreathInAir>();
                int before = air != null ? air.Puffed : 0;
                yield return Wait(9);
                air = unit.GetComponent<BreathInAir>();
                int byDay = (air != null ? air.Puffed : 0) - before;
                time.Hour = 21;
                yield return Wait(9);
                air = unit.GetComponent<BreathInAir>();
                int byNight = air != null ? air.Puffed - byDay - before : 0;
                Debug.Log($"OWN_LOOKS_OF_BREATH {name}: its shoulders are drawn {plain * 1000:F1} mm higher over its chest full than empty as it was, and {shrugged * 1000:F1} mm with the look in its shoulders; in the air it breathed {byDay} puffs in nine seconds by day and {byNight} after dusk (cold {(air != null ? air.Cold : 0):F2}); it stands: {own.Stands}");
                Assert.That(own.Stands && own.WentDown == 0, Is.True, name + " went down.");
                Assert.That(shrugged - plain, Is.GreaterThan(.006f), name + ": its shoulders are not drawn rising with its breath.");
                Assert.That(byDay, Is.EqualTo(0), name + ": its breath is seen in the air by day.");
                Assert.That(byNight, Is.GreaterThanOrEqualTo(3), name + ": its breath is not seen in the air after dusk.");
                time.Hour = hour;
                look.SetBreathLook(0);
                look.SetOwn(false);
                yield return Wait(.6f);
            }
            time.Hour = hour;
        }

        // Luis, October 10: the body standing by its own joints is accepted, and its switch is on from the start.
        [UnityTest, Timeout(300000)]
        public IEnumerator TheSwitchIsOnFromTheStart()
        {
            Time.captureFramerate = 50;
            Assert.That(look.StandsByItsOwn, Is.True, "The switch is not on from the start.");
            choice.Choose(0);
            var unit = choice.Current;
            yield return Stand(unit, (s, a) => { });
            OwnBody own = null;
            for (float began = Time.time; Time.time - began < 5 && (own == null || !own.Stands);) { unit.TryGetComponent(out own); yield return null; }
            Assert.That(own != null && own.Stands, Is.True, "A miner at ease does not stand by its own joints without being asked: " + look.Status());
            look.SetOwn(false);
            yield return Wait(.6f);
            Assert.That(own.Stands, Is.False, "The body as it was is not one click away.");
        }

        // S3b, step 3 (October 13; for Luis to judge, not accepted): the step is on from the start, and a miner
        // standing at ease takes none: its breath, its weight going from leg to leg and its looking about do not
        // set it stepping.
        [UnityTest, Timeout(900000)]
        public IEnumerator AtEaseItTakesNoStep()
        {
            Time.captureFramerate = 50;
            Assert.That(look.StepsToCatch && OwnKeeper.Steps, Is.True, "The step is not on from the start.");
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                yield return StandPlainly(unit, (s, a) => { });
                OwnBody own = null;
                yield return Own(unit, name, o => own = o);
                Vector3 began = unit.transform.position;
                for (float until = Time.time + 45; Time.time < until && own.Stands;) yield return new WaitForFixedUpdate();
                Debug.Log($"OWN_AT_EASE {name}, three quarters of a minute at ease with the step on: {own.Stepped} steps; down {own.WentDown} times; its place moved {Flat(unit.transform.position - began).magnitude * 1000:F0} mm");
                Assert.That(own.Stands && own.WentDown == 0, Is.True, name + ", at ease with the step on, went down.");
                Assert.That(own.Stepped, Is.EqualTo(0), name + ", at ease, took a step.");
                look.SetOwn(false);
                yield return Wait(.6f);
            }
        }

        // Shoved harder than it can stand through where it is (set going at half a metre a second, four ways), it
        // catches itself with a step, and its place goes with its feet.
        [UnityTest, Timeout(1800000)]
        public IEnumerator ShovedItCatchesItselfWithAStep()
        {
            Time.captureFramerate = 50;
            OwnBody.Told = what => Debug.Log("OWN_TOLD " + what);
            // (-ownStill 1: with no life in it, to see what the life in it does to a step. Not the test.)
            if (CaptureTools.Argument("-ownStill") == "1") OwnBody.Alive = false;
            // (-ownOnly Small: one miner. -ownStaysPut 1: its place does not go with it. -ownAnyGround 1: no ground asked for.
            // -ownStepSet "LandsBy=3": the keeper's settings, by name. Not the test: what is changed stays changed for the run.)
            // -ownWay 90: one way only. -ownTick 1: every step of the physics told, as the bench tells it.)
            string onlyOne = CaptureTools.Argument("-ownOnly"), onlyWay = CaptureTools.Argument("-ownWay");
            bool ticks = CaptureTools.Argument("-ownTick") == "1";
            // (-ownShove .7: shoved at another speed, to see how far what it does reaches. Not the test.)
            float shove = CaptureTools.Argument("-ownShove") != null ? float.Parse(CaptureTools.Argument("-ownShove"), System.Globalization.CultureInfo.InvariantCulture) : MinerWorkPreview.ShoveSpeed;
            OwnBody.StaysPut = CaptureTools.Argument("-ownStaysPut") == "1"; OwnBody.AnyGround = CaptureTools.Argument("-ownAnyGround") == "1";
            foreach (string pair in (CaptureTools.Argument("-ownStepSet") ?? "").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                var sides = pair.Split('=');
                var field = typeof(OwnKeeper).GetField(sides[0].Trim());
                Assert.That(field != null, Is.True, sides[0]);
                field.SetValue(null, float.Parse(sides[1], System.Globalization.CultureInfo.InvariantCulture));
            }
            Assert.That(look.StepsToCatch && OwnKeeper.Steps, Is.True, "The step is not on from the start.");
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                if (!string.IsNullOrEmpty(onlyOne) && onlyOne != name) continue;
                int stood = 0, stepped = 0;
                string told = "";
                for (int d = 0; d < 4; d++)
                {
                    if (!string.IsNullOrEmpty(onlyWay) && onlyWay != (d * 90).ToString()) continue;
                    Vector3 away = default, spotAt = default;
                    yield return StandPlainly(unit, (s, a) => { away = a; spotAt = s; });
                    if (told == "")
                    {
                        // (How the ground lies there: its height, in millimetres over where it stands, a step away each way.)
                        string lies = "";
                        for (int w = 0; w < 4; w++)
                        {
                            Vector3 to = Quaternion.AngleAxis(w * 90, Vector3.up) * away;
                            lies += $" [{w * 90}:";
                            foreach (float far in new[] { .15f, .3f, .45f }) lies += $" {(ground.Height(spotAt.x + to.x * far, spotAt.z + to.z * far) - ground.Height(spotAt.x, spotAt.z)) * 1000:F0}";
                            lies += "]";
                        }
                        Debug.Log($"OWN_GROUND where {name} stands, the ground 0.15, 0.3 and 0.45 m away, each way from ahead (mm over where it stands):{lies}");
                    }
                    // (-ownFloor 1: a level floor laid where it stands, at the ground's own height, to see what the meadow's
                    // own ground does to a step. Not the test.)
                    if (CaptureTools.Argument("-ownFloor") == "1" && floorLaid == null && Physics.Raycast(spotAt + Vector3.up * 2, Vector3.down, out var under, 5, 1 << 6))
                    {
                        floorLaid = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        floorLaid.layer = 6;
                        floorLaid.transform.localScale = new Vector3(8, .4f, 8);
                        floorLaid.transform.position = under.point + Vector3.down * .199f;
                        floorLaid.GetComponent<Renderer>().enabled = false;
                        Physics.SyncTransforms();
                    }
                    if (CaptureTools.Argument("-ownWaits") != null) { look.SetOwn(false); yield return Wait(float.Parse(CaptureTools.Argument("-ownWaits"), System.Globalization.CultureInfo.InvariantCulture)); }
                    OwnBody own = null;
                    yield return Own(unit, name, o => own = o);
                    yield return Wait(2);
                    int downs = own.WentDown;
                    Vector3 way = Quaternion.AngleAxis(d * 90, Vector3.up) * away, from = unit.transform.position;
                    Debug.Log($"OWN_TOLD {name} is shoved {d * 90} degrees from ahead at {Time.time:F2}");
                    look.Nudge(way, shove);
                    int most = 0;
                    Vector3 hipsFrom = own.Part(OwnBody.Hips).transform.position; float shovedAt = Time.time;
                    for (float until = Time.time + 7; Time.time < until && own.WentDown == downs && own.Stands;)
                    {
                        most = Mathf.Max(most, own.Stepped);
                        yield return new WaitForFixedUpdate();
                        if (ticks) Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "OWN_TICK {0:0.00} {1}", Time.time - shovedAt, own.Tick(way.normalized, hipsFrom)));
                    }
                    if (own.Stands && own.WentDown == downs)
                    {
                        stood++;
                        if (most > 0) stepped++;
                        Transform left = own.Part(OwnBody.Foot).transform, right = own.Part(OwnBody.Foot + 1).transform;
                        float place = Flat(unit.transform.position - .5f * (left.position + right.position)).magnitude;
                        float tipped = Mathf.Max(Vector3.Angle(Vector3.up, left.up), Vector3.Angle(Vector3.up, right.up));
                        float went = Flat(unit.transform.position - from).magnitude;
                        told += $" [{d * 90}: {most} steps, went {went * 1000:F0} mm, its place {place * 1000:F0} mm from between its boots, a boot tipped {tipped:F0} deg at most, boots {Flat(right.position - left.position).magnitude * 1000:F0} mm apart]";
                        Assert.That(place, Is.LessThan(.15f), $"{name}, shoved {d * 90} degrees from ahead: its place did not go with its feet.");
                        look.SetOwn(false);
                        yield return Wait(.6f);
                    }
                    else
                    {
                        told += $" [{d * 90}: down after {most} steps]";
                        // (It gets itself up, as it does; then it is stood on the path again.)
                        look.SetOwn(false);
                        var fall = unit.GetComponent<PhysicalFall>();
                        for (float until = Time.time + 60; Time.time < until && fall != null && fall.Now != PhysicalFall.State.Up;) yield return null;
                        yield return Wait(1);
                    }
                }
                Debug.Log($"OWN_SHOVED {name}, set going at {shove:F2} m/s four ways: stood {stood} of 4, stepping in {stepped}:{told}");
                Assert.That(stood, Is.GreaterThanOrEqualTo(3), name + ", shoved four ways, went down more than once.");
                Assert.That(stepped, Is.GreaterThanOrEqualTo(1), name + ", shoved four ways, never took a step.");
            }
        }

        // Pulled at its chest with 0.08 of its weight for two and a half seconds, it leans (or steps) and stands.
        // (Small and Long are held to it. Round is told and not yet held to it: on October 13 it went down in 2 runs of
        // 7, falling forward out of its second catch while the pull was still on.)
        [UnityTest, Timeout(900000)]
        public IEnumerator PulledGentlyItStands()
        {
            Time.captureFramerate = 50;
            OwnBody.Told = what => Debug.Log("OWN_TOLD " + what);
            // (-ownPull .12: with another share of its weight, as the panel's "Pull it" gives. Not the test.)
            float share = CaptureTools.Argument("-ownPull") != null ? float.Parse(CaptureTools.Argument("-ownPull"), System.Globalization.CultureInfo.InvariantCulture) : .08f;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                Vector3 away = default;
                yield return StandPlainly(unit, (s, a) => { away = a; });
                OwnBody own = null;
                yield return Own(unit, name, o => own = o);
                yield return Wait(2);
                Vector3 before = own.HeadAt;
                look.Pull(away, share, MinerWorkPreview.PullFor, MinerWorkPreview.PullOver);
                Assert.That(own.Pulled, Is.True, name + " is not pulled.");
                float furthest = 0;
                for (float until = Time.time + 7; Time.time < until && own.Stands;) { furthest = Mathf.Max(furthest, Flat(own.HeadAt - before).magnitude); yield return new WaitForFixedUpdate(); }
                Debug.Log($"OWN_PULLED {name}, pulled forwards at its chest with {share:F2} of its weight for {MinerWorkPreview.PullFor} s: {own.Stepped} steps; its head went {furthest * 1000:F0} mm at most; down {own.WentDown} times");
                if (name != "Round" && share <= .08f) Assert.That(own.Stands && own.WentDown == 0, Is.True, name + ", pulled with 0.08 of its weight, went down.");
                else if (!own.Stands || own.WentDown > 0)
                {
                    // (It gets itself up, as it does.)
                    Debug.Log($"OWN_PULLED {name} went down: it is not yet held to standing this pull.");
                    var fall = unit.GetComponent<PhysicalFall>();
                    for (float until = Time.time + 60; Time.time < until && fall != null && fall.Now != PhysicalFall.State.Up;) yield return null;
                }
                look.SetOwn(false);
                yield return Wait(.6f);
            }
        }
    }
}
