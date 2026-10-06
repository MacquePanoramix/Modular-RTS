using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S1d, the miners at work: each miner has a pickaxe made for its own arms and hands, and swings it at a rock
    // face with the same rule as the first body (only a real contact of the pick's head counts). The hands that
    // are free hold the handle, closed round it, the handle lying where the model's build measured the closed
    // hand on it; a hand that carries something keeps to it, and the pick is swung with the other.
    public sealed class MinerToolTests
    {
        private MinerChoice choice;
        private OrdinaryGround ground;

        [UnitySetUp] public IEnumerator Load()
        {
            PlayerPrefs.DeleteKey("WonderGather.Miner");
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            PlayerPrefs.DeleteKey("WonderGather.Miner");
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
        }

        private Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);

        [Test] public void EachMinerHasAPickaxeMadeForItsArmsAndHands()
        {
            float last = 0;
            foreach (int index in new[] { 0, 2, 1 })
            {
                choice.Choose(index);
                string name = choice.NameOf(index);
                var body = choice.Current.GetComponent<MinerBody>();
                var biped = choice.Current.GetComponent<ProceduralBiped>();
                var tool = body.Pickaxe;
                Assert.That(tool, Is.Not.Null, name + " has no pickaxe.");
                Assert.That(tool.IsValid, Is.True);
                Assert.That(tool.Id, Is.EqualTo("pickaxe"), "Every size of pickaxe answers to what a mineral asks for.");
                // Made for the arms: the same share of them as the first pickaxe is of the first body's.
                float between = Vector3.Distance(tool.PrimaryGrip, tool.SecondaryGrip);
                Assert.That(between / biped.ArmReach, Is.EqualTo(.32f / .87f).Within(.01f), name + "'s grips are not spaced for its arms.");
                Assert.That(biped.ArmReach, Is.GreaterThan(last), "Small, then Round, then Long: longer arms, a longer pickaxe.");
                last = biped.ArmReach;
                // Made for the hands: every hand that is free can close on the handle where it grips it.
                for (int hand = 0; hand < 2; hand++)
                {
                    if (!body.CanHold(hand)) continue;
                    var grip = body.Fingers(hand);
                    Assert.That(tool.GripRadius(hand), Is.InRange(grip.radii[0], grip.radii[grip.radii.Length - 1]),
                        $"{name}'s hand cannot close on a handle {tool.GripRadius(hand) * 2000:F0} mm thick.");
                }
                // The model in the tool's own space: its head up the handle and its point to the front.
                var model = tool.Prefab.GetComponentInChildren<MeshRenderer>();
                Assert.That(model, Is.Not.Null);
                Assert.That(tool.Head.y, Is.GreaterThan(tool.SecondaryGrip.y).And.GreaterThan(tool.PrimaryGrip.y));
                Assert.That(tool.Head.z, Is.GreaterThan(.05f));
            }
        }

        // The look at the work, in play: K puts a plain rock before the chosen miner, which mines it with its own
        // pickaxe; when its arms are full it begins again (nothing is hauled yet); K again, or walking away, puts
        // the rock and the pickaxe away and leaves the miner as it was.
        [UnityTest] public IEnumerator TheLookAtTheWorkBeginsAndEndsCleanly()
        {
            var look = Object.FindAnyObjectByType<MinerWorkPreview>();
            Assert.That(look, Is.Not.Null, "The Ordinary Place has no look at the miners' work.");
            for (int index = 0; index < 3; index++)
            {
                choice.Choose(index);
                for (int k = 0; k < 20; k++) yield return null;
                var unit = choice.Current;
                string name = choice.NameOf(index);
                look.Toggle();
                Assert.That(look.Showing, Is.True, name + " did not begin to mine.");
                var tool = look.Tool;
                float began = Time.time;
                while (tool != null && tool.AcceptedStrikes < 1 && Time.time - began < 30) yield return null;
                Assert.That(tool != null && tool.AcceptedStrikes >= 1, Is.True, name + " did not strike the rock in the look at its work.");
                look.Toggle();
                yield return null;
                yield return null;
                Assert.That(look.Showing, Is.False);
                Assert.That(unit.GetComponent<Gatherer>(), Is.Null, name + " kept a gatherer after the look.");
                Assert.That(unit.GetComponent<EquippedTool>(), Is.Null, name + " kept a tool after the look.");
                Assert.That(Object.FindObjectsByType<MineableResource>(FindObjectsSortMode.None).Length, Is.EqualTo(0), "The rock was left behind.");
                // And it walks on as before.
                var body = unit.GetComponent<MinerBody>();
                began = Time.time;
                while ((body.Held(0) > 0 || body.Held(1) > 0) && Time.time - began < 3) yield return null;
                Assert.That(body.Held(0) + body.Held(1), Is.EqualTo(0), name + "'s hands did not open again.");
                Assert.That(unit.Motor.TryMove(OnGround(unit.transform.position + unit.transform.forward * -2f)), Is.True);
                began = Time.time;
                var from = unit.transform.position;
                while (Vector3.Distance(unit.transform.position, from) < 1f && Time.time - began < 15) yield return null;
                Assert.That(Vector3.Distance(unit.transform.position, from), Is.GreaterThan(.9f), name + " did not walk on after the look.");
                unit.Motor.Stop();
            }
        }

        [UnityTest] public IEnumerator EachMinerSwingsItsOwnPickaxeAndTheHeadStrikesTheRock()
        {
            // On the dirt path, along one of its long straight stretches.
            var a = ground.Path[4];
            var b = ground.Path[5];
            Vector3 from = OnGround(new Vector3(a.x, 0, a.y)), facing = OnGround(new Vector3(b.x, 0, b.y)) - from;
            facing.y = 0;
            facing.Normalize();
            for (int index = 0; index < 3; index++)
            {
                choice.Choose(index);
                yield return null;
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var body = unit.GetComponent<MinerBody>();
                var biped = unit.GetComponent<ProceduralBiped>();
                var definition = body.Pickaxe;
                unit.Motor.Stop();
                unit.GetComponent<NavMeshAgent>().Warp(from);
                unit.transform.SetPositionAndRotation(from, Quaternion.LookRotation(facing));
                biped.ResetPose();
                var gatherer = unit.gameObject.AddComponent<Gatherer>();
                var tool = unit.gameObject.AddComponent<EquippedTool>();
                tool.SetDefinition(definition);
                biped.ConfigureWork(gatherer, null);
                for (int k = 0; k < 20; k++) yield return null;

                // The face stands where the head's swing meets it: a little short of the swing's farthest reach. The
                // miner already stands at its place: how exactly a body arrives at a place to work, and where that
                // place is for its size, belong to the rock (the next step), not to the tool.
                float swing = (definition.Head - definition.PrimaryGrip).magnitude;
                Vector3 stand = unit.transform.position;
                Assert.That(MinerWorkPreview.FaceDistance(biped, definition), Is.EqualTo((biped.ToolHand.z + swing) * .78f).Within(1e-4f));
                var (node, depot, rock) = CaptureTools.RockFace(stand, facing, MinerWorkPreview.FaceDistance(biped, definition), OnGround(stand - facing * 3));
                gatherer.Configure(depot, Vector3.zero);
                Assert.That(gatherer.Gather(node), Is.True, $"{name} could not be sent to mine: {gatherer.LastOrderFailure}");

                Assert.That(tool.Uses(1), Is.True, name + "'s right hand should hold the pickaxe.");
                Assert.That(tool.Uses(0), Is.EqualTo(body.CanHold(0)), name + "'s left hand holds the pickaxe only if it is free.");
                float began = Time.time, off = 0, askew = 0, unplanted = 0;
                int held = 0;
                while (tool.AcceptedStrikes < 2 && Time.time - began < 45)
                {
                    yield return null;
                    if (!tool.Busy || !tool.HandsOnTool) continue;
                    if (!biped.FootPlanted(0) || !biped.FootPlanted(1)) unplanted += Time.deltaTime;
                    for (int hand = 0; hand < 2; hand++)
                    {
                        if (!tool.Uses(hand) || body.Held(hand) < 1) continue;
                        // The handle lies in the closed hand where the build measured the fingers on it.
                        Assert.That(body.HandleIn(hand, definition.GripRadius(hand), out var point, out var way), Is.True);
                        off = Mathf.Max(off, Vector3.Distance(point, tool.GripPosition(hand)));
                        askew = Mathf.Max(askew, Vector3.Angle(way, tool.HandleDirection));
                        held++;
                    }
                }
                Assert.That(tool.AcceptedStrikes, Is.GreaterThanOrEqualTo(2), $"{name} did not strike the rock twice: {tool.Status}; {gatherer.ActivityLabel}; {gatherer.LastOrderFailure}; "
                    + $"it stands {Vector3.Dot(node.transform.position - unit.transform.position, facing) - .4f:F3} m from the face, its head reaches {biped.ToolHand.z + swing:F3} m; hands closed {held} frames");
                Debug.Log($"MINER_TOOL_HELD {name}: over {held} frames with a hand closed on it, the handle is at most {off * 1000:F1} mm from where the hand holds it and {askew:F1} degrees askew");
                Assert.That(gatherer.Carried, Is.GreaterThanOrEqualTo(2), name + "'s strikes should each yield.");
                Assert.That(held, Is.GreaterThan(30), name + "'s hands were hardly ever closed on the handle while it swung.");
                Assert.That(off, Is.LessThan(.008f), $"{name}: the handle is {off * 1000:F0} mm from where the closed hand holds it.");
                Assert.That(askew, Is.LessThan(4f), $"{name}: the handle lies {askew:F0} degrees askew in the closed hand.");
                Assert.That(unplanted, Is.LessThan(.1f), name + " should swing with both feet planted.");

                gatherer.CancelOrder();
                biped.ConfigureWork(null, null);
                Object.Destroy(tool);
                Object.Destroy(gatherer);
                Object.Destroy(rock);
                Object.Destroy(depot.gameObject);
                yield return null;
                yield return null;
            }
        }
    }
}
