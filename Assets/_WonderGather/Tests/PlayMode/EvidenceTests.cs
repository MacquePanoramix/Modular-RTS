using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, step 12: what the plan's list of evidence asked for and no other test showed (Docs/NextMilestonePlan.md).
    public sealed class EvidenceTests
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

        private int Miner(string name)
        {
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == name) return i;
            Assert.Fail("No miner is called " + name);
            return -1;
        }

        private static int Pickaxes()
        {
            int count = 0;
            foreach (var thing in Object.FindObjectsByType<HeldThing>(FindObjectsSortMode.None)) if (thing.Tool != null) count++;
            return count;
        }

        // "Nothing appears in a hand or vanishes from one: every object is at every moment held, hanging, or lying."
        // One pickaxe and one lantern are followed, frame by frame, through everything a miner does with them: the
        // lantern taken in hand; told to mine, the lantern hung back, the pickaxe picked up from the ground, carried to
        // a boulder and worked with; a rest; and the pickaxe laid down. At every frame there is the one pickaxe, in this
        // miner's hands or in none, and it has not jumped; and the lantern is on the miner.
        [UnityTest, Timeout(900000)]
        public IEnumerator NothingAppearsInAHandOrVanishesFromOne()
        {
            Time.captureFramerate = 50;
            choice.Choose(Miner("Small"));
            yield return Wait(.4f);
            var unit = choice.Current;
            var boulder = Boulder.All()[1];
            Vector3 centre = boulder.Rock.bounds.center;
            for (int k = 0; k < 12; k++)
            {
                Vector3 at = centre + Quaternion.Euler(0, k * 30, 0) * Vector3.forward * (Mathf.Max(boulder.Rock.bounds.extents.x, boulder.Rock.bounds.extents.z) + 2.5f);
                if (!NavMesh.SamplePosition(at, out var walked, 1, NavMesh.AllAreas)) continue;
                unit.Motor.Stop();
                unit.GetComponent<NavMeshAgent>().Warp(walked.position);
                unit.transform.rotation = Quaternion.LookRotation(Flat(centre - walked.position).normalized);
                unit.GetComponent<ProceduralBiped>().ResetPose();
                break;
            }
            yield return Wait(.6f);
            var biped = unit.GetComponent<ProceduralBiped>();
            var has = ThingsInHand.Of(unit);
            var lantern = has.Thing(0);
            Assert.That(Pickaxes(), Is.EqualTo(0), "A pickaxe is in the world before one was put there.");
            var pickaxe = look.LayPickaxe(1);
            Assert.That(pickaxe, Is.Not.Null);
            var body = pickaxe.GetComponent<Rigidbody>();
            yield return Wait(1);

            int frames = 0, held = 0, lying = 0, inHand = 0;
            float furthest = 0, lanternFurthest = 0;
            Vector3 was = body.position;
            IEnumerator Watch(System.Func<bool> until, float atMost, string during)
            {
                float began = Time.time;
                while (!until() && Time.time - began < atMost)
                {
                    yield return null;
                    frames++;
                    Assert.That(pickaxe != null, Is.True, "The pickaxe vanished " + during + ".");
                    Assert.That(Pickaxes(), Is.EqualTo(1), "Another pickaxe appeared " + during + ".");
                    var hands = unit.GetComponent<PhysicalHands>();
                    if (pickaxe.Holder != null)
                    {
                        held++;
                        Assert.That(hands != null && pickaxe.Holder == hands && hands.Thing == pickaxe, Is.True, "The pickaxe is in hands that are not this miner's " + during + ".");
                    }
                    else
                    {
                        lying++;
                        Assert.That(hands == null || hands.Held == null, Is.True, "The miner's hands hold something that is not the pickaxe " + during + ".");
                    }
                    float moved = Vector3.Distance(body.position, was);
                    furthest = Mathf.Max(furthest, moved);
                    Assert.That(moved, Is.LessThan(.4f), "The pickaxe jumped " + during + ".");
                    was = body.position;
                    // The lantern: on the miner, at its hook or in its hand.
                    float off = Vector3.Distance(InteractionClick.Place(lantern), biped.HipsNow);
                    lanternFurthest = Mathf.Max(lanternFurthest, off);
                    Assert.That(off, Is.LessThan(1.2f), "The lantern left the miner " + during + ".");
                    if (has.Has == lantern) inHand++;
                }
                Assert.That(until(), Is.True, "It did not come about: " + during + " (" + look.Status().Replace("\n", " / ") + ")");
            }

            // The lantern in hand.
            Assert.That(click.OpenOn(lantern) && click.Choose("Take in hand"), Is.True);
            yield return Watch(() => has.Now == ThingsInHand.Phase.Carried, 6, "as it took its lantern in hand");
            // Told to mine: the lantern goes back, the pickaxe is picked up, carried, and worked with.
            int blows = boulder.Blows;
            Assert.That(click.OpenOn(boulder) && click.Choose("Mine"), Is.True);
            yield return Watch(() => boulder.Blows >= blows + 2, 80, "as it went to mine");
            Assert.That(has.Has, Is.Null, "The lantern is still in its hand at its work.");
            // What each blow was is measured, and shown.
            var last = look.Swing.results[look.Swing.results.Count - 1];
            Assert.That(last.struck && last.speed > 1 && last.energy > 1, Is.True, "The last blow's speed and energy were not measured.");
            Assert.That(look.Status(), Does.Contain("last blow").And.Contain("m/s").And.Contain(" J"), "The panel does not show the last blow.");
            // A rest at its boulder.
            Assert.That(click.OpenOn(unit) && click.Choose("Rest"), Is.True);
            yield return Watch(() => look.Carrying, 4, "as it stopped to rest");
            yield return Watch(() => Mathf.Abs(biped.BowNow) < 5 && biped.SinkNow < .02f, 6, "as it stood at ease");
            // The pickaxe laid down.
            Assert.That(click.OpenOn(pickaxe) && click.Choose("Lay it down"), Is.True);
            yield return Watch(() => !look.Showing, 15, "as it laid its pickaxe down");
            float settled = Time.time;
            yield return Watch(() => Time.time - settled > 1.5f, 3, "after it laid its pickaxe down");
            Assert.That(pickaxe.Holder, Is.Null);
            Assert.That(body.linearVelocity.magnitude, Is.LessThan(.1f), "The pickaxe does not lie still.");
            float low = float.MaxValue;
            foreach (var solid in pickaxe.GetComponents<Collider>()) low = Mathf.Min(low, solid.bounds.min.y);
            Assert.That(low - ground.Height(body.worldCenterOfMass.x, body.worldCenterOfMass.z), Is.InRange(-.08f, .15f), "The pickaxe does not lie on the ground.");
            Debug.Log($"EVIDENCE_OBJECTS one pickaxe and one lantern followed through {frames} frames ({frames / 50f:F0} s): the pickaxe in the miner's hands for {held}, lying in none for {lying}; it never moved more than {furthest * 100:F0} cm in a frame; the lantern in the hand for {inHand} frames, never more than {lanternFurthest:F2} m from the miner's hips");
            Assert.That(held, Is.GreaterThan(200));
            Assert.That(lying, Is.GreaterThan(100));
            Assert.That(inHand, Is.GreaterThan(20));
        }
    }
}
