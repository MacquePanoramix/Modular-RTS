using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, step 8: the interaction click and the first small actions, each done by the miner's body. A pickaxe in the
    // hands offers to be laid down; one that lies offers to be picked up; a miner at work offers to rest. The orders
    // are given here as the box gives them (the key and the mouse themselves are not pressed in a test).
    public sealed class InteractionTests
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

        private Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        // How high a tool's lowest part is over the ground.
        private float Over(HeldThing thing)
        {
            float low = float.MaxValue;
            foreach (var solid in thing.GetComponents<Collider>()) low = Mathf.Min(low, solid.bounds.min.y);
            var body = thing.GetComponent<Rigidbody>();
            return low - ground.Height(body.worldCenterOfMass.x, body.worldCenterOfMass.z);
        }

        private bool Offers(string label)
        {
            for (int i = 0; i < click.Count; i++) if (click.Label(i) == label) return true;
            return false;
        }

        // The miner sets to work and strikes once.
        private IEnumerator Work(SelectableUnit unit, string name)
        {
            look.Toggle();
            float began = Time.time;
            while (!look.Swinging && Time.time - began < 6) yield return null;
            Assert.That(look.Swinging, Is.True, name + " did not take up its pickaxe.");
            int before = look.Swing.results.Count;
            while (look.Swing != null && look.Swing.results.Count <= before && Time.time - began < 30) yield return null;
            Assert.That(look.Swing.results.Count, Is.GreaterThan(before), name + " did not swing.");
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator ThePickaxeIsLaidDownAndPickedUpAgainByTheBody()
        {
            Assert.That(click, Is.Not.Null, "The Ordinary Place has no interaction click.");
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.4f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var biped = unit.GetComponent<ProceduralBiped>();
                yield return Work(unit, name);
                var thing = unit.GetComponent<PhysicalHands>().Thing;

                // In its hands, the pickaxe offers to be laid down.
                Assert.That(click.OpenOn(thing), Is.True, name + "'s pickaxe offered nothing.");
                Assert.That(Offers("Lay it down"), Is.True);
                Assert.That(Offers("Pick it up"), Is.False);
                Assert.That(click.Choose("Lay it down"), Is.True);
                float began = Time.time;
                while (look.Showing && Time.time - began < 15) yield return null;
                Assert.That(look.Showing, Is.False, name + " did not finish laying its pickaxe down.");
                yield return Wait(.6f);
                Assert.That(thing != null && look.Lying == thing, Is.True, name + "'s pickaxe is not lying in the world.");
                Assert.That(thing.Holder, Is.Null);
                Assert.That(unit.GetComponent<PhysicalHands>(), Is.Null);
                Assert.That(GameObject.Find("Block (a look at the work)"), Is.Null);
                var lies = thing.GetComponent<Rigidbody>();
                float away = Vector3.ProjectOnPlane(lies.worldCenterOfMass - unit.transform.position, Vector3.up).magnitude;
                Debug.Log($"INTERACTION_LAID {name}: laid down in {Time.time - began - .6f:F1} s; the pickaxe {Over(thing) * 1000:F0} mm over the ground, {away:F2} m from the miner, moving at {lies.linearVelocity.magnitude:F2} m/s; the miner bows {biped.BowNow:F1} degrees");
                Assert.That(Over(thing), Is.LessThan(.03f), name + "'s pickaxe is not on the ground.");
                Assert.That(lies.linearVelocity.magnitude, Is.LessThan(.1f), name + "'s pickaxe is not lying still.");
                Assert.That(away, Is.LessThan(1), name + " laid its pickaxe far from itself.");
                Assert.That(Mathf.Abs(biped.BowNow), Is.LessThan(3.5f), name + " did not stand up again.");
                Assert.That(lies.mass, Is.EqualTo(thing.Tool.Mass).Within(.01f), "A pickaxe that lies should weigh what it weighs.");

                // It walks off; the pickaxe stays.
                Vector3 lay = lies.worldCenterOfMass;
                Assert.That(unit.Motor.TryMove(OnGround(unit.transform.position + unit.transform.right * 2.5f)), Is.True);
                began = Time.time;
                while (unit.Motor.IsMoving && Time.time - began < 15) yield return null;
                Assert.That(Vector3.Distance(lies.worldCenterOfMass, lay), Is.LessThan(.05f), "The pickaxe moved when the miner left.");

                // Lying, it offers to be picked up; the miner goes to it, bends down and takes it.
                Assert.That(click.OpenOn(thing), Is.True, "A pickaxe that lies offered nothing.");
                Assert.That(Offers("Pick it up"), Is.True);
                Assert.That(click.Choose("Pick it up"), Is.True);
                began = Time.time;
                PhysicalHands hands = null;
                while (Time.time - began < 30)
                {
                    hands = unit.GetComponent<PhysicalHands>();
                    if (hands != null && look.Carrying && !look.Carry.Fetching && hands.Held == lies && hands.Holds(0)) break;
                    yield return null;
                }
                Assert.That(hands != null && hands.Held == lies && hands.Holds(0), Is.True, name + " did not take its pickaxe up.");
                float took = Time.time - began;
                // It stands up with it (how long that takes is told, and is no more than a few seconds).
                float tookHold = Time.time, mostLegs = 0;
                while ((Mathf.Abs(biped.BowNow) >= 3 || biped.SinkNow >= .02f) && Time.time - tookHold < 8)
                {
                    var legs = unit.GetComponent<PhysicalBalance>();
                    if (legs != null) mostLegs = Mathf.Max(mostLegs, legs.LegEffort);
                    if (!look.Showing || hands == null || hands.Held == null) break;
                    yield return null;
                }
                float stoodUp = Time.time - tookHold;
                var fallen = unit.GetComponent<PhysicalFall>();
                Assert.That(look.Showing && hands != null && hands.Held == lies, Is.True, $"{name} did not keep its pickaxe as it stood up ({stoodUp:F1} s after it took hold): its legs were asked {mostLegs * 100:F0}% at most; it fell {(fallen != null ? fallen.Falls : 0)} time(s) ({(unit.GetComponent<PhysicalBalance>() != null ? unit.GetComponent<PhysicalBalance>().Fell : "")}); it stands {Vector3.ProjectOnPlane(lies.worldCenterOfMass - unit.transform.position, Vector3.up).magnitude:F2} m from the pickaxe; the panel says: {look.Status()}");
                yield return Wait(.6f);
                Debug.Log($"INTERACTION_TAKEN {name}: taken up {took:F1} s after the order, and standing with it {stoodUp:F1} s later (its legs asked {mostLegs * 100:F0}% at most as it rose); then carried {look.Carry.way}, the pickaxe {Over(thing):F2} m over the ground; the miner bows {biped.BowNow:F1} degrees; hands {Mathf.Max(hands.Miss(0), hands.Miss(1)) * 1000:F0} mm off");
                Assert.That(stoodUp, Is.LessThan(5), name + " was slow to stand up with its pickaxe.");
                Assert.That(look.Carry.way, Is.EqualTo(PhysicalCarry.Way.OneHand));
                Assert.That(Over(thing), Is.GreaterThan(.12f), name + " did not lift its pickaxe.");
                Assert.That(Mathf.Abs(biped.BowNow), Is.LessThan(3.5f), name + " did not stand up with it.");

                // And it can go to work with it again.
                look.Toggle();
                began = Time.time;
                int before = look.Swing.results.Count;
                while ((!look.Swinging || look.Swing.results.Count <= before) && Time.time - began < 30) yield return null;
                Assert.That(look.Swing.results.Count, Is.GreaterThan(before), name + " did not work with the pickaxe it picked up.");
                Assert.That(look.Swing.results[look.Swing.results.Count - 1].struck, Is.True);
                look.Toggle();
                yield return Wait(.3f);
                Assert.That(look.Showing, Is.False);
            }
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator AMinerAtWorkRestsWhenToldAndGoesBackToIt()
        {
            Time.captureFramerate = 50;
            choice.Choose(2);
            yield return Wait(.4f);
            var unit = choice.Current;
            // Standing with nothing to do, the miner offers nothing.
            Assert.That(click.OpenOn(unit), Is.False);
            yield return Work(unit, "Round");
            Assert.That(click.OpenOn(unit), Is.True);
            Assert.That(Offers("Rest"), Is.True);
            Assert.That(click.Choose("Rest"), Is.True);
            yield return Wait(2.5f);
            var hands = unit.GetComponent<PhysicalHands>();
            Assert.That(look.Carrying, Is.True, "It did not stop its work.");
            Assert.That(look.Swinging, Is.False);
            Assert.That(hands.Held, Is.Not.Null, "It should keep its pickaxe while it rests.");
            Assert.That(GameObject.Find("Block (a look at the work)"), Is.Null);
            Assert.That(Mathf.Abs(unit.GetComponent<ProceduralBiped>().BowNow), Is.LessThan(3.5f), "Resting, it should stand up.");
            // With no boulder of its own it has nothing to go back to by the click: no block is made for it in play.
            Assert.That(click.OpenOn(unit), Is.False, "A resting miner with no boulder was offered work.");
            // (The bench's block, for the tests: back to work.)
            int before = look.Swing.results.Count;
            look.Toggle();
            float began = Time.time;
            while ((!look.Swinging || look.Swing.results.Count <= before) && Time.time - began < 30) yield return null;
            Assert.That(look.Swing.results.Count, Is.GreaterThan(before), "It did not go back to its work.");
            look.Toggle();
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator TheBoxOpensOnWhatIsNearThePointerAndStopsTheOrdinaryClicks()
        {
            choice.Choose(2);
            yield return Wait(.4f);
            var unit = choice.Current;
            var camera = Camera.main;
            Assert.That(click.Blocks, Is.False, "With nothing armed, the ordinary clicks should work.");
            click.Arm(true);
            Assert.That(click.Armed && click.Blocks, Is.True);
            click.Arm(false);
            yield return Work(unit, "Round");
            var thing = unit.GetComponent<PhysicalHands>().Thing;
            // What is near the pointer is what is clicked: the pickaxe where it is, the miner at its hips; nothing far off.
            yield return null;
            Vector3 at = camera.WorldToScreenPoint(InteractionClick.Place(thing));
            Assert.That(click.Pick(new Vector2(at.x, at.y)), Is.EqualTo(thing));
            // (Far off there may be a boulder under the pointer, which is a thing of its own: never the pickaxe or the miner.)
            var far = click.Pick(new Vector2(at.x + 400, at.y + 300));
            Assert.That(far == null || far is Boulder, Is.True);
            Assert.That(click.OpenOn(thing), Is.True);
            Assert.That(click.IsOpen && click.Blocks, Is.True);
            Assert.That(click.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(click.Choose("No such order"), Is.False);
            Assert.That(click.IsOpen, Is.True);
            click.Close();
            Assert.That(click.IsOpen || click.Blocks, Is.False, "Closed, it should let the ordinary clicks through.");
            look.Toggle();
        }
    }
}
