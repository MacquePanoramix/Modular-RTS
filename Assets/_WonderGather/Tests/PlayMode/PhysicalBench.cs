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
            int frames = (int)Numbers("-benchFrames", 0)[0];
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
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
                    swing.hands = hands; swing.body = biped; swing.tool = definition;
                    // It bows to its work first, and the block is put under where the pick's head then rests.
                    biped.Bow(PhysicalSwing.RestBow);
                    for (float until = Time.time + .6f; Time.time < until;) yield return null;
                    swing.Intend(PhysicalSwing.Rest, out var at, out var turned);
                    // A low block under where the pick's head rests: what the swing comes down on.
                    Vector3 rests = at + turned * definition.Head;
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
                    string tag = string.Format(culture, "{0}_s{1:0.0#}_w{2:0.0#}", who.ToLowerInvariant(), strength, weight);
                    float miss = 0, tilt = 0;
                    string when = "";
                    int shot = 0, wanted = 2, frame = 0;
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
                        if (frames > 0 && shot < frames && (swing.phase != PhysicalSwing.Phase.Ready || swing.results.Count > 0 || Time.time - began > .7f) && frame++ % 2 == 0)
                        {
                            Vector3 target = unit.transform.position + Vector3.up * height * .55f + away * .3f;
                            Vector3 dir = Quaternion.AngleAxis(100, Vector3.up) * away;
                            dir = dir * Mathf.Cos(8 * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(8 * Mathf.Deg2Rad);
                            camera.transform.SetPositionAndRotation(target + dir * 3f, Quaternion.LookRotation(-dir));
                            CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{shot:000}"), 360, 360);
                            shot++;
                        }
                    };
                    while ((swing.results.Count < wanted || (frames > 0 && shot < frames)) && Time.time - began < 40) yield return null;
                    after.Then = null;
                    UnityEngine.Object.Destroy(after.gameObject);
                    float carried = hands.ToolMass;
                    if (swing.trace != null) foreach (string line in swing.trace) Debug.Log("BENCHTRACE " + line);
                    for (int k = 0; k < swing.results.Count; k++)
                    {
                        var r = swing.results[k];
                        Debug.Log(string.Format(culture,
                            "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: swing {3}: raised {4:0.00} m ({5:0}% of the way) in {6:0.00} s, its hardest joint at {8:0}% of what it has on average; {7}",
                            who, strength, carried, k + 1, r.lifted, r.reached * 100, r.liftTime,
                            r.struck ? string.Format(culture, "struck at {0:0.0} m/s, {1:0} J", r.speed, r.energy) : "did not strike", r.liftEffort * 100));
                    }
                    Debug.Log(string.Format(culture, "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: {3} swings in {4:0.0} s; hands at most {5:0.0} mm off the handle ({11}); the tool leaned at most {6:0} degrees aside; shoulder {7:0} Nm, elbow {8:0} Nm, wrist {9:0.0} Nm, hold {10:0} N",
                        who, strength, carried, swing.results.Count, Time.time - began, miss * 1000, tilt, physical.ShoulderCapacity, physical.ElbowCapacity, physical.WristCapacity, physical.HoldCapacity, when));
                    PhysicalHands.Timed = false;
                    if (PhysicalHands.TimedSteps > 0)
                        Debug.Log(string.Format(culture, "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: its hands' step took {3:0.0} microseconds on average, over {4} steps",
                            who, strength, carried, PhysicalHands.TimedTicks * 1e6 / System.Diagnostics.Stopwatch.Frequency / PhysicalHands.TimedSteps, PhysicalHands.TimedSteps));
                    var was = hands.Drop();
                    if (was != null) UnityEngine.Object.Destroy(was.gameObject);
                    UnityEngine.Object.Destroy(swing);
                    UnityEngine.Object.Destroy(hands);
                    UnityEngine.Object.Destroy(rock);
                    biped.Bow(0);
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
