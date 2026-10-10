using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The bench of S3's step 2 (Docs/NextMilestonePlan.md): one miner standing in the Ordinary Place with a pickaxe
    // that is a real body, at several strengths and with the pickaxe at several weights. Each tries the same thing:
    // raise the pick over the shoulder as high as it will go, bring it down on a block, and take it back. What it
    // manages comes out of the weights and of what its arms can give (PhysicalHands). The plan of the swing here is a
    // rough one, for the bench: the swing itself is step 4.
    //
    // It writes what each one managed to the log (BENCH ...), and with -benchFrames every frame of each from the
    // side, for a moving picture.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.PhysicalBench -benchOut <folder>
    //   [-benchMiner Round] [-benchStrengths 0.5,1,2] [-benchWeights 0.5,1,2] [-benchFrames 110]
    public sealed class PhysicalBench
    {
        private static float[] Numbers(string argument, params float[] otherwise)
        {
            string given = CaptureTools.Argument(argument);
            if (string.IsNullOrEmpty(given)) return otherwise;
            var parts = given.Split(',');
            var values = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++) values[i] = float.Parse(parts[i], CultureInfo.InvariantCulture);
            return values;
        }

        [UnityTest, Explicit, Timeout(7200000)]
        public IEnumerator Bench()
        {
            string folder = CaptureTools.Argument("-benchOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Bench");
            string who = CaptureTools.Argument("-benchMiner") ?? "Round";
            float[] strengths = Numbers("-benchStrengths", .5f, 1, 2), weights = Numbers("-benchWeights", .5f, 1, 2);
            // -benchMarks: heights to aim the blow at, as shares of the miner's height (0: where the pick rests unaimed).
            float[] marks = Numbers("-benchMarks", 0);
            // -benchSwings: how many swings each makes (2). -benchFrom: the frames begin at that swing (0).
            int swings = (int)Numbers("-benchSwings", 2)[0], framesFrom = (int)Numbers("-benchFrom", 0)[0];
            // -benchView: where the pictures are taken from, in degrees round the miner (0: before it, 90: its right,
            // 270: its left) and above level.
            float[] view = Numbers("-benchView", 100, 8);
            int frames = (int)Numbers("-benchFrames", 0)[0];
            // -benchBalance measure|on: the miner keeps its own balance (or only measures it). Pulling a miner is the
            // balance's own bench (PhysicalBalanceBench).
            string balancing = CaptureTools.Argument("-benchBalance");
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            // The body is posed once a frame and the physics steps fifty times a second: the bench runs frame for step,
            // and keeps every other frame for the moving picture (25 a second).
            Time.captureFramerate = (int)Numbers("-benchRate", 50)[0];
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                var camera = Camera.main;
                var rig = camera.GetComponent<RtsCamera>();
                if (rig != null) rig.enabled = false;
                camera.nearClipPlane = .01f;
                time.Hour = 13;
                Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
                var a = ground.Path[4];
                var b = ground.Path[5];
                var spot = OnGround(new Vector3(a.x, 0, a.y));
                var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
                away.y = 0;
                away.Normalize();
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (string.Equals(choice.NameOf(i), who, StringComparison.OrdinalIgnoreCase)) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner is called " + who);
                var culture = CultureInfo.InvariantCulture;

                foreach (float strength in strengths)
                foreach (float weight in weights)
                foreach (float mark in marks)
                {
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    var miner = unit.GetComponent<MinerBody>();
                    var biped = unit.GetComponent<ProceduralBiped>();
                    var agent = unit.GetComponent<NavMeshAgent>();
                    var physical = unit.GetComponent<PhysicalBody>();
                    Assert.That(physical, Is.Not.Null, who + " has not been weighed.");
                    unit.Motor.Stop();
                    agent.Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    biped.ResetPose();
                    for (float until = Time.time + .6f; Time.time < until;) yield return null;
                    physical.Strength = strength;
                    var definition = miner.Pickaxe;
                    float height = miner.Rig.head.position.y - unit.transform.position.y;
                    var hands = unit.gameObject.AddComponent<PhysicalHands>();
                    var swing = unit.gameObject.AddComponent<PhysicalSwing>();
                    var back = unit.gameObject.AddComponent<PhysicalBack>();
                    swing.hands = hands; swing.back = back; swing.body = biped; swing.tool = definition;
                    PhysicalBalance balance = null;
                    if (balancing != null)
                    {
                        balance = unit.gameObject.AddComponent<PhysicalBalance>();
                        balance.Acts = balancing != "measure";
                        balance.Brace(PhysicalSwing.StanceWider, PhysicalSwing.StanceStagger);
                    }
                    Vector3 across = Vector3.Cross(Vector3.up, away);
                    // It bows to its work first, and the block is put under where the pick's head then rests.
                    back.Want(PhysicalSwing.RestBow);
                    for (float until = Time.time + (balance != null ? 1.3f : .6f); Time.time < until;) yield return null;
                    swing.Intend(PhysicalSwing.Rest, out var at, out var turned);
                    // A block under where the pick's head rests: what the swing comes down on. Aimed, the block's top
                    // is at a height of its own, as far ahead, and the body takes the stance that reaches it.
                    Vector3 rests = at + turned * definition.Head;
                    if (mark > 0)
                    {
                        rests.y = unit.transform.position.y + mark * height + definition.HeadRadius + .015f;
                        swing.Aim(rests - Vector3.up * .015f);
                        back.Want(swing.RestBowNow);
                        biped.Sink(swing.SinkFor);
                        for (float until = Time.time + .8f; Time.time < until;) yield return null;
                        swing.Intend(swing.RestLean, out at, out turned);
                        Debug.Log(string.Format(culture, "BENCH {0}: aimed at {1:0.00} of its height: bows {2:0} degrees, sinks {3:0.000} m, the tool leaning {4:0}; the aim is {5:0} mm off",
                            who, mark, swing.RestBowNow, swing.SinkFor, swing.RestLean + 6, swing.AimMiss * 1000));
                    }
                    float top = rests.y - definition.HeadRadius - .015f, floor = ground.Height(rests.x, rests.z) - .1f;
                    var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rock.name = "Bench block";
                    rock.transform.SetPositionAndRotation(new Vector3(rests.x, (top + floor) * .5f, rests.z), Quaternion.LookRotation(away));
                    rock.transform.localScale = new Vector3(.5f, top - floor, .5f);
                    var paint = Shader.Find("Wonder Gather/Painted");
                    if (paint != null)
                    {
                        var stone = new Material(paint) { name = "Bench block" };
                        stone.SetColor("_BaseColor", new Color(.36f, .38f, .42f));
                        rock.GetComponent<MeshRenderer>().sharedMaterial = stone;
                    }
                    hands.Take(definition, at, turned, weight);
                    PhysicalHands.TimedTicks = 0; PhysicalHands.TimedSteps = 0; PhysicalHands.Timed = true;
                    string tag = string.Format(culture, mark > 0 ? "{0}_s{1:0.0#}_w{2:0.0#}_m{3:0.00}" : "{0}_s{1:0.0#}_w{2:0.0#}", who.ToLowerInvariant(), strength, weight, mark);
                    float miss = 0, tilt = 0;
                    string when = "";
                    int shot = 0, wanted = swings, frame = 0;
                    physical.Refresh();
                    if (balance != null) balance.Mark();
                    float began = Time.time;
                    if (CaptureTools.Argument("-benchTrace") != null) swing.trace = new List<string>();
                    Debug.Log(string.Format(culture, "BENCH {0}: the block's top is {1:0.000} m up, its middle {2:0.000} m ahead, {3:0.00} m across", who, top - unit.transform.position.y,
                        Vector3.Dot(rock.transform.position - unit.transform.position, away), rock.transform.localScale.x));
                    // Measures and pictures are taken when the frame's bodies have all been posed: the body is drawn after
                    // the physics has moved the tool.
                    var after = new GameObject("After everything").AddComponent<AfterEverything>();
                    after.Then = () =>
                    {
                        if (hands.Held == null) return;
                        for (int hand = 0; hand < 2; hand++)
                        {
                            if (hands.Miss(hand) <= miss) continue;
                            miss = hands.Miss(hand);
                            when = string.Format(culture, "{0} hand, {1}, the tool leaning {2:0}, the wrist {3:0.000} m from the shoulder of an arm {4:0.000} m long",
                                hand == 0 ? "left" : "right", swing.phase, swing.Lean(), Vector3.Distance(miner.Rig.hands[hand].position, biped.ShoulderNow(hand)), biped.ArmReach);
                        }
                        tilt = Mathf.Max(tilt, Vector3.Angle(hands.Held.rotation * Vector3.right, unit.transform.right));
                        // From its side, every other frame from a little before the first lift on.
                        if (frames > 0 && shot < frames && swing.results.Count >= framesFrom && (swing.phase != PhysicalSwing.Phase.Ready || swing.results.Count > 0 || Time.time - began > .7f) && frame++ % 2 == 0)
                        {
                            Vector3 target = unit.transform.position + Vector3.up * height * .55f + away * .3f;
                            Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * away;
                            dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                            camera.transform.SetPositionAndRotation(target + dir * 3f, Quaternion.LookRotation(-dir));
                            CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{shot:000}"), 360, 360);
                            if (balance != null)
                            {
                                // What it stands on and where its weight is, from above, in its own first place's frame
                                // (to its right, ahead), for a drawing beside the picture.
                                var line = new System.Text.StringBuilder();
                                line.AppendFormat(culture, "BALANCE {0} {1:000}:", tag, shot);
                                for (int f = 0; f < 2; f++)
                                {
                                    biped.Sole(f, out Vector3 heel, out Vector3 toe, out float half);
                                    line.AppendFormat(culture, " {0} {1:0.000} {2:0.000} {3:0.000} {4:0.000} {5:0.000}", biped.FootPlanted(f) ? 1 : 0,
                                        Vector3.Dot(heel - spot, across), Vector3.Dot(heel - spot, away), Vector3.Dot(toe - spot, across), Vector3.Dot(toe - spot, away), half);
                                }
                                foreach (var at in new[] { balance.Weight, balance.WeightPoint, balance.Presses, biped.HipsNow })
                                    line.AppendFormat(culture, " {0:0.000} {1:0.000}", Vector3.Dot(at - spot, across), Vector3.Dot(at - spot, away));
                                line.AppendFormat(culture, " {0:0.000} {1}", balance.Margin, swing.phase);
                                Debug.Log(line.ToString());
                            }
                            shot++;
                        }
                    };
                    while ((swing.results.Count < wanted || (frames > 0 && shot < frames)) && Time.time - began < 40 + swings * 12) yield return null;
                    after.Then = null;
                    UnityEngine.Object.Destroy(after.gameObject);
                    float carried = hands.ToolMass;
                    if (swing.trace != null) foreach (string line in swing.trace) Debug.Log("BENCHTRACE " + line);
                    for (int k = 0; k < swing.results.Count; k++)
                    {
                        var r = swing.results[k];
                        Debug.Log(string.Format(culture,
                            "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: swing {3}: upper hand {9:0}% of the way to the head; raised {4:0.00} m ({5:0}% of the way) in {6:0.00} s, its hardest joint at {8:0}% of what it has on average, its back at {10:0}%; {7}",
                            who, strength, carried, k + 1, r.lifted, r.reached * 100, r.liftTime,
                            r.struck ? string.Format(culture, "struck at {0:0.0} m/s, {1:0} J", r.speed, r.energy) : "did not strike", r.liftEffort * 100, r.choked * 100, r.backEffort * 100)
                            + string.Format(culture, "; {0:0}% spent when it began", r.spent * 100));
                    }
                    Debug.Log(string.Format(culture, "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: {3} swings in {4:0.0} s; hands at most {5:0.0} mm off the handle ({11}); the tool leaned at most {6:0} degrees aside; shoulder {7:0} Nm, elbow {8:0} Nm, wrist {9:0.0} Nm, hold {10:0} N",
                        who, strength, carried, swing.results.Count, Time.time - began, miss * 1000, tilt, physical.ShoulderCapacity, physical.ElbowCapacity, physical.WristCapacity, physical.HoldCapacity, when));
                    PhysicalHands.Timed = false;
                    if (balance != null)
                        Debug.Log(string.Format(culture, "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: balance ({6}): its weight's point came no nearer the edge of its feet than {3:0} mm (negative: outside them); its hips leaned {4:0} mm at most; it took {5} steps to catch itself; its feet bear {7:0} N; its legs are {8:0}% spent and gave way {9:0} mm",
                            who, strength, carried, balance.LeastMargin * 1000, balance.MostLean * 1000, balance.Steps, balance.Acts ? "acting" : "measuring", balance.Bears,
                            physical.Spent(PhysicalBody.Muscles.Legs) * 100, balance.GaveWay * 1000));
                    Debug.Log(string.Format(culture, "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: rested {3} times; at the end its arms are {4:0}% and {5:0}% spent, its back {6:0}%",
                        who, strength, hands.ToolMass, swing.rests, physical.Spent(PhysicalBody.Muscles.LeftArm) * 100, physical.Spent(PhysicalBody.Muscles.RightArm) * 100, physical.Spent(PhysicalBody.Muscles.Back) * 100));
                    if (PhysicalHands.TimedSteps > 0)
                        Debug.Log(string.Format(culture, "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: its hands' step took {3:0.0} microseconds on average, over {4} steps",
                            who, strength, carried, PhysicalHands.TimedTicks * 1e6 / System.Diagnostics.Stopwatch.Frequency / PhysicalHands.TimedSteps, PhysicalHands.TimedSteps));
                    var was = hands.Drop();
                    if (was != null) UnityEngine.Object.Destroy(was.gameObject);
                    UnityEngine.Object.Destroy(swing);
                    UnityEngine.Object.Destroy(hands);
                    UnityEngine.Object.Destroy(rock);
                    UnityEngine.Object.Destroy(back);
                    if (balance != null) UnityEngine.Object.Destroy(balance);
                    biped.Sink(0);
                    physical.Strength = 1;
                    for (float until = Time.time + .1f; Time.time < until;) yield return null;
                }
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
