using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, step 11: the plain panel of the Ordinary Place. It sets how strong the chosen miner is, and puts a pickaxe on
    // the ground beside it, lighter or heavier: nothing appears in a hand. The orders are given here as the panel's
    // buttons and the interaction click's box give them (the mouse itself is not pressed in a test).
    public sealed class MinerPanelTests
    {
        private MinerChoice choice;
        private MinerWorkPreview look;
        private InteractionClick click;
        private MinerPanel panel;
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
            panel = Object.FindAnyObjectByType<MinerPanel>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
        }

        [TearDown]
        public void Restore() { Time.captureFramerate = 0; }

        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }
        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        private int Miner(string name)
        {
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == name) return i;
            Assert.Fail("No miner is called " + name);
            return -1;
        }

        private bool Offers(string label)
        {
            for (int i = 0; i < click.Count; i++) if (click.Label(i) == label) return true;
            return false;
        }

        private static int Pickaxes()
        {
            int count = 0;
            foreach (var thing in Object.FindObjectsByType<HeldThing>(FindObjectsSortMode.None)) if (thing.Tool != null) count++;
            return count;
        }

        // How high a tool's lowest part is over the ground.
        private float Over(HeldThing thing)
        {
            float low = float.MaxValue;
            foreach (var solid in thing.GetComponents<Collider>()) low = Mathf.Min(low, solid.bounds.min.y);
            var body = thing.GetComponent<Rigidbody>();
            return low - ground.Height(body.worldCenterOfMass.x, body.worldCenterOfMass.z);
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator ThePanelIsWithTheMinersAndAClickOnItIsNotAClickInTheWorld()
        {
            Assert.That(panel, Is.Not.Null, "The place has no panel.");
            Assert.That(click, Is.Not.Null, "The place has no interaction click.");
            // The choice of miner is open when the place loads: the panel waits for it.
            choice.Choose(Miner("Round"));
            yield return Wait(.3f);
            var input = Object.FindAnyObjectByType<RtsInput>();
            Assert.That(input, Is.Not.Null);
            Vector2 on = new Vector2(60, 60), off = new Vector2(Screen.width - 40, Screen.height * .6f);
            if (choice.Open)
            {
                Assert.That(panel.Shown, Is.False, "The panel shows under the choice of miner.");
                Assert.That(panel.Over(on), Is.False);
            }
            // Closed (as choosing one in play closes it), the panel is there, at the screen's lower left.
            var open = typeof(MinerChoice).GetField("open", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            open.SetValue(choice, false);
            Assert.That(panel.Shown, Is.True, "With a miner chosen, the panel is not shown.");
            Assert.That(panel.Over(on), Is.True, "The panel is not at the screen's lower left.");
            Assert.That(panel.Over(off), Is.False, "The panel takes the clicks of the whole screen.");
            // A pointer on the panel is not in the world for the ordinary clicks either (what the input asks before a click).
            var asked = typeof(RtsInput).GetField("interfaceBlocker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var blocks = (System.Func<Vector2, bool>)asked.GetValue(input);
            Assert.That(blocks, Is.Not.Null, "Nothing keeps the ordinary clicks off the panel.");
            Assert.That(blocks(on), Is.True, "A click on the panel would also be a click in the world.");
            Assert.That(blocks(off), Is.False, "The ordinary clicks are stopped away from the panel.");
            // The three pickaxes it offers are lighter, its own, and heavier.
            Assert.That(MinerPanel.Weights.Length, Is.EqualTo(3));
            Assert.That(MinerPanel.Weights[0], Is.LessThan(1));
            Assert.That(MinerPanel.Weights[1], Is.EqualTo(1));
            Assert.That(MinerPanel.Weights[2], Is.GreaterThan(1));
            Assert.That(look.Status(), Does.Contain("Nothing in its hands"));
        }

        // Each miner, each of the panel's three pickaxes: put on the ground it lies there, in no hand, at its own weight;
        // the miner picks it up when told, and holds it as its strength allows.
        [UnityTest, Timeout(900000)]
        public IEnumerator APickaxeIsPutOnTheGroundAndTheMinerPicksItUpItself()
        {
            Time.captureFramerate = 50;
            foreach (string who in new[] { "Small", "Long", "Round" })
            {
                choice.Choose(Miner(who));
                yield return Wait(.4f);
                var unit = choice.Current;
                var tool = unit.GetComponent<MinerBody>().Pickaxe;
                for (int i = 0; i < MinerPanel.Weights.Length; i++)
                {
                    float share = MinerPanel.Weights[i];
                    Assert.That(Pickaxes(), Is.EqualTo(0), "A pickaxe is in the world before one was put there.");
                    var thing = look.LayPickaxe(share);
                    Assert.That(thing, Is.Not.Null, $"No {MinerPanel.Names[i]} pickaxe was put on the ground beside {who}.");
                    var body = thing.GetComponent<Rigidbody>();
                    // It is a thing of the world from the first moment: in no hand, and the miner has none.
                    Assert.That(thing.Holder, Is.Null);
                    Assert.That(look.Showing, Is.False, who + " was given the pickaxe.");
                    Assert.That(unit.GetComponent<PhysicalHands>(), Is.Null, who + " has hands at work with nothing in them.");
                    Assert.That(body.mass, Is.EqualTo(tool.Mass * share).Within(.005f), "It does not weigh what the panel says.");
                    Assert.That(look.PickaxeWeighs(share), Is.EqualTo(tool.Mass * share).Within(.001f));
                    yield return Wait(1.5f);
                    float away = Flat(body.worldCenterOfMass - unit.transform.position).magnitude, over = Over(thing);
                    Assert.That(body.linearVelocity.magnitude, Is.LessThan(.05f), "The pickaxe did not come to lie.");
                    Assert.That(over, Is.InRange(-.08f, .08f), "The pickaxe does not lie on the ground.");
                    Assert.That(away, Is.InRange(.35f, 1.2f), "The pickaxe does not lie beside the miner.");
                    Assert.That(look.Status(), Does.Contain("lies on the ground"));

                    // It shows itself to the click, says what it weighs, and offers to be picked up.
                    bool shown = false;
                    foreach (var one in click.Things()) shown |= one == thing;
                    Assert.That(shown, Is.True, "The pickaxe does not show itself.");
                    Assert.That(click.Named(thing), Does.Contain((tool.Mass * share).ToString("0.0") + " kg"));
                    Assert.That(click.OpenOn(thing), Is.True);
                    Assert.That(Offers("Pick it up"), Is.True);
                    Assert.That(click.Choose("Pick it up"), Is.True);
                    float began = Time.time, gaveUp = -1;
                    PhysicalHands hands = null;
                    string told = "";
                    var biped = unit.GetComponent<ProceduralBiped>();
                    var legs = unit.GetComponent<PhysicalBalance>();
                    float deepest = 0, asked = 0;
                    while (Time.time - began < 25)
                    {
                        hands = unit.GetComponent<PhysicalHands>();
                        if (legs == null) legs = unit.GetComponent<PhysicalBalance>();
                        deepest = Mathf.Max(deepest, biped.SinkNow);
                        if (legs != null && biped.SinkNow > .05f) asked = Mathf.Max(asked, legs.KneesAsked(body.mass));
                        string says = look.Status();
                        if (says.Contains("legs") || says.Contains("knees") || says.Contains("could not")) told = says;
                        if (!look.Showing && gaveUp < 0 && Time.time - began > .5f) { gaveUp = Time.time - began; break; }
                        if (look.Showing && look.Carrying && hands != null && hands.Held != null && !look.Carry.Fetching && (hands.Holds(0) || hands.Holds(1))
                            && unit.GetComponent<ProceduralBiped>().SinkNow < .02f) break;
                        yield return null;
                    }
                    float took = Time.time - began;
                    Assert.That(hands != null && hands.Thing == thing && (hands.Holds(0) || hands.Holds(1)), Is.True, $"{who} did not pick the {MinerPanel.Names[i]} pickaxe up ({took:F1} s; it said \"{told}\"; its knees bent {deepest:F2} m of {ProceduralBiped.DeepestSink * biped.StandingHipHeight:F2}, each asked {asked * 100:F0}%): {look.Status()}; it stands {Flat(thing.GetComponent<Rigidbody>().worldCenterOfMass - unit.transform.position).magnitude:F2} m from it");
                    Assert.That(hands.ToolMass, Is.EqualTo(tool.Mass * share).Within(.005f));
                    Assert.That(Pickaxes(), Is.EqualTo(1), "Picking one up made another.");
                    yield return Wait(1.5f);
                    var way = look.Carry.way;
                    Debug.Log($"PANEL_PICKAXE {who}, {MinerPanel.Names[i]} ({tool.Mass * share:F2} kg): put {away:F2} m from it, {over * 1000:F0} mm over the ground; picked up {took:F1} s after the order; then {(way == PhysicalCarry.Way.OneHand ? "carried in one hand" : way == PhysicalCarry.Way.Dragged ? "held by its end, the head on the ground" : "left")}, asking {look.Carry.Asks * 100:F0}% of its hold. The panel says: {look.Status().Replace("\n", " / ")}");
                    Assert.That(way, Is.Not.EqualTo(PhysicalCarry.Way.Left), who + " could not move it at all.");

                    // Laid down again, it lies, and the panel takes it away.
                    Assert.That(click.OpenOn(thing) && click.Choose("Lay it down"), Is.True);
                    began = Time.time;
                    while (look.Showing && Time.time - began < 15) yield return null;
                    Assert.That(look.Showing, Is.False, who + " did not lay it down.");
                    Assert.That(thing != null && thing.Holder == null, Is.True);
                    Assert.That(Pickaxes(), Is.EqualTo(1));
                    look.ClearLaid();
                    yield return null;
                    yield return null;
                    Assert.That(Pickaxes(), Is.EqualTo(0), "The panel did not take the pickaxe away.");
                    Assert.That(look.Lying, Is.Null);
                }
            }
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator ThePanelsStrengthIsTheChosenMinersWithOrWithoutAPickaxe()
        {
            Time.captureFramerate = 50;
            choice.Choose(Miner("Round"));
            yield return Wait(.4f);
            var round = choice.Current;
            var body = round.GetComponent<PhysicalBody>();
            Assert.That(body.Strength, Is.EqualTo(1).Within(1e-4f));
            // With nothing in its hands.
            look.SetStrength(.5f);
            Assert.That(look.Strength, Is.EqualTo(.5f).Within(1e-4f));
            Assert.That(body.Strength, Is.EqualTo(.5f).Within(1e-4f), "The strength did not reach a miner with nothing in its hands.");
            // The slider has its ends.
            look.SetStrength(10);
            Assert.That(body.Strength, Is.EqualTo(MinerPanel.Strongest).Within(1e-4f));
            look.SetStrength(0);
            Assert.That(body.Strength, Is.EqualTo(MinerPanel.Weakest).Within(1e-4f));
            look.SetStrength(1);
            // With a pickaxe in its hands: the same pickaxe asks more of a weaker hand and less of a stronger one.
            var thing = look.LayPickaxe(MinerPanel.Weights[2]);
            yield return Wait(1);
            look.PickUp(thing);
            float began = Time.time;
            while (!(look.Showing && look.Carrying && !look.Carry.Fetching && round.GetComponent<PhysicalHands>().Held != null) && Time.time - began < 25) yield return null;
            yield return Wait(2);
            Assert.That(round.GetComponent<PhysicalHands>().Thing, Is.EqualTo(thing), "Round did not pick the heavy pickaxe up.");
            Assert.That(body.Strength, Is.EqualTo(1).Within(1e-4f), "Taking a pickaxe up changed its strength.");
            float ordinary = look.Carry.Asks;
            look.SetStrength(.5f);
            yield return Wait(1.5f);
            float weak = look.Carry.Asks;
            look.SetStrength(2);
            yield return Wait(1.5f);
            float strong = look.Carry.Asks;
            var fall = round.GetComponent<PhysicalFall>();
            Debug.Log($"PANEL_STRENGTH Round with the heavy pickaxe ({round.GetComponent<PhysicalHands>().ToolMass:F2} kg) in one hand: it asks {ordinary * 100:F0}% of its hold at its own strength, {weak * 100:F0}% at half, {strong * 100:F0}% at twice");
            Assert.That(fall == null || fall.Falls == 0, Is.True, "It fell standing with its pickaxe.");
            Assert.That(weak, Is.EqualTo(ordinary * 2).Within(ordinary * .2f), "At half its strength the same pickaxe should ask twice as much of its hand.");
            Assert.That(strong, Is.EqualTo(ordinary * .5f).Within(ordinary * .1f), "At twice its strength the same pickaxe should ask half as much of its hand.");
            // Another miner chosen: the panel's strength is its strength now, and the one that left is as it was built.
            choice.Choose(Miner("Small"));
            yield return null;
            yield return null;
            var small = choice.Current;
            Assert.That(small.GetComponent<PhysicalBody>().Strength, Is.EqualTo(2).Within(1e-4f), "The newly chosen miner did not take the panel's strength.");
            Assert.That(body.Strength, Is.EqualTo(1).Within(1e-4f), "The miner that left kept the panel's strength.");
            look.SetStrength(1);
            Assert.That(small.GetComponent<PhysicalBody>().Strength, Is.EqualTo(1).Within(1e-4f));
        }

        // Below its ordinary strength a miner's knees do not let it down as far as a pickaxe lies: it bends as far as
        // one knee could hold it alone, does not reach, stands up again, and the panel says why. It does not fall.
        [UnityTest, Timeout(600000)]
        public IEnumerator AWeakMinerDoesNotGetDownToItsPickaxeAndStandsUpAgain()
        {
            Time.captureFramerate = 50;
            foreach (string who in new[] { "Small", "Long", "Round" })
            {
                choice.Choose(Miner(who));
                yield return Wait(.4f);
                var unit = choice.Current;
                var biped = unit.GetComponent<ProceduralBiped>();
                look.SetStrength(.6f);
                var thing = look.LayPickaxe(1);
                Assert.That(thing, Is.Not.Null);
                yield return Wait(1);
                Assert.That(click.OpenOn(thing) && click.Choose("Pick it up"), Is.True);
                float began = Time.time, deepest = 0, mostAsked = 0;
                bool said = false;
                while (look.Showing && Time.time - began < 20)
                {
                    deepest = Mathf.Max(deepest, biped.SinkNow);
                    var balance = unit.GetComponent<PhysicalBalance>();
                    if (balance != null && biped.SinkNow > .02f) mostAsked = Mathf.Max(mostAsked, balance.KneesAsked(thing.GetComponent<Rigidbody>().mass));
                    yield return null;
                }
                float took = Time.time - began;
                said = look.Status().Contains("legs");
                yield return null;
                yield return null;
                var fall = unit.GetComponent<PhysicalFall>();
                Debug.Log($"PANEL_WEAK {who} at 0.6 of its strength: bent its knees {deepest:F2} m at most (the deepest bend is {ProceduralBiped.DeepestSink * biped.StandingHipHeight:F2} m), each asked {mostAsked * 100:F0}% of what it has holding half; stood up again {took:F1} s after the order. The panel says: {look.Status()}");
                Assert.That(look.Showing, Is.False, who + " is still at it.");
                Assert.That(unit.GetComponent<PhysicalHands>(), Is.Null, who + " has the pickaxe.");
                Assert.That(thing != null && thing.Holder == null, Is.True, "The pickaxe does not lie where it lay.");
                Assert.That(fall == null || fall.Falls == 0, Is.True, who + " fell.");
                Assert.That(deepest, Is.LessThan(.8f * ProceduralBiped.DeepestSink * biped.StandingHipHeight), who + " bent its knees as deep as a strong body does.");
                Assert.That(mostAsked, Is.LessThan(.75f), who + "'s knees were bent far past what one could hold alone.");
                Assert.That(said, Is.True, "The panel does not say why it did not take the pickaxe: " + look.Status());
                Assert.That(Mathf.Abs(biped.BowNow) < 4 && biped.SinkNow < .02f, Is.True, who + " did not stand up again.");
                // As strong as it was built, it picks the same pickaxe up.
                look.SetStrength(1);
                yield return Wait(.5f);
                Assert.That(click.OpenOn(thing) && click.Choose("Pick it up"), Is.True);
                began = Time.time;
                PhysicalHands hands = null;
                while (Time.time - began < 25)
                {
                    hands = unit.GetComponent<PhysicalHands>();
                    if (look.Showing && hands != null && hands.Held != null && !look.Carry.Fetching && (hands.Holds(0) || hands.Holds(1))) break;
                    yield return null;
                }
                Assert.That(hands != null && hands.Thing == thing, Is.True, $"{who} did not pick it up at its own strength: {look.Status()}");
                look.End();
                look.ClearLaid();
                yield return Wait(.4f);
            }
            look.SetStrength(1);
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator WithNoPickaxeAnywhereAMinerToldToMineIsGivenNone()
        {
            choice.Choose(Miner("Round"));
            yield return Wait(.4f);
            var unit = choice.Current;
            var boulder = Boulder.All()[4];
            Assert.That(Pickaxes(), Is.EqualTo(0));
            Assert.That(click.OpenOn(boulder), Is.True, "The boulder offered nothing.");
            Assert.That(click.Choose("Mine"), Is.True);
            yield return Wait(1);
            Assert.That(look.Showing, Is.False, "It set to work with no pickaxe.");
            Assert.That(Pickaxes(), Is.EqualTo(0), "A pickaxe was made for it.");
            Assert.That(unit.GetComponent<PhysicalHands>(), Is.Null);
            Assert.That(unit.Motor.IsMoving, Is.False, "It set out for the boulder with nothing to mine with.");
            Assert.That(look.Status(), Does.Contain("no pickaxe"), "The panel does not say why it does nothing.");
        }
    }
}
