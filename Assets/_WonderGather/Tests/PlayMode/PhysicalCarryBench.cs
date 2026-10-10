using System;
using System.Collections;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The bench of S3's step 7 (Docs/NextMilestonePlan.md): a miner in the Ordinary Place takes its pickaxe as it would
    // to carry it (PhysicalCarry) and walks some metres along the path with it, at several strengths and with the
    // pickaxe at several weights. How it holds it (in one hand at its side, or dragged by the end of its handle), how
    // fast it walks and whether it leaves it come out of the weights and of what its hand can hold.
    //
    // It writes what each came to (BENCH ...), and with -carryFrames every other frame of each from its side.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.PhysicalCarryBench -carryOut <folder>
    //   [-carryMiner Round] [-carryStrengths 1,0.4] [-carryWeights 1,3] [-carryWalk 4] [-carryFrames 150] [-carryView 100,8,3]
    public sealed class PhysicalCarryBench
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
            string folder = CaptureTools.Argument("-carryOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Carry");
            string who = CaptureTools.Argument("-carryMiner") ?? "Round";
            float[] strengths = Numbers("-carryStrengths", 1), weights = Numbers("-carryWeights", 1);
            float walk = Numbers("-carryWalk", 4)[0];
            float[] view = Numbers("-carryView", 100, 8, 3);
            int frames = (int)Numbers("-carryFrames", 0)[0];
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = (int)Numbers("-carryRate", 50)[0];
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
                    unit.Motor.Stop();
                    agent.Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    biped.ResetPose();
                    for (float until = Time.time + .8f; Time.time < until;) yield return null;
                    physical.Strength = strength;
                    physical.Refresh();
                    var definition = miner.Pickaxe;
                    float height = miner.Rig.head.position.y - unit.transform.position.y;
                    var hands = unit.gameObject.AddComponent<PhysicalHands>();
                    var back = unit.gameObject.AddComponent<PhysicalBack>();
                    var balance = unit.gameObject.AddComponent<PhysicalBalance>();
                    // The swing is only asked where the tool is held before the body: it does not run.
                    var swing = unit.gameObject.AddComponent<PhysicalSwing>();
                    swing.enabled = false;
                    swing.hands = hands; swing.back = back; swing.body = biped; swing.tool = definition;
                    var carry = unit.gameObject.AddComponent<PhysicalCarry>();
                    carry.enabled = false;
                    carry.hands = hands; carry.back = back; carry.body = biped; carry.tool = definition;
                    for (float until = Time.time + .3f; Time.time < until;) yield return null;
                    swing.Intend(PhysicalSwing.Rest, out var at, out var turned);
                    hands.Take(definition, at, turned, weight);
                    carry.enabled = true;
                    balance.Mark();
                    string tag = string.Format(culture, "{0}_s{1:0.0#}_w{2:0.0#}", who.ToLowerInvariant(), strength, weight);
                    int shot = 0, frame = 0;
                    float miss = 0, hold = 0, effort = 0, slowest = 1, pulled = 0, took0 = Time.time;
                    string when = "";
                    float lows = 0, handUps = 0;
                    int walked = 0;
                    int steps = 0;
                    var then = new GameObject("After everything").AddComponent<AfterEverything>();
                    then.Then = () =>
                    {
                        if (hands.Held != null)
                        {
                            float off = Mathf.Max(hands.Miss(0), hands.Miss(1));
                            if (off > miss)
                            {
                                miss = off;
                                when = string.Format(culture, "{0:0.0} s after it took it, {1}, going at {2:0.00} m/s, its {3} wrist {4:0.000} m from the shoulder of an arm {5:0.000} m long",
                                    Time.time - took0, unit.Motor.IsMoving ? "walking" : "standing", biped.VelocityNow.magnitude, hands.Miss(0) > hands.Miss(1) ? "left" : "right",
                                    Vector3.Distance(miner.Rig.hands[hands.Miss(0) > hands.Miss(1) ? 0 : 1].position, biped.ShoulderNow(hands.Miss(0) > hands.Miss(1) ? 0 : 1)), biped.ArmReach);
                            }
                            hold += Mathf.Max(hands.Hold(0), hands.Hold(1)); effort += Mathf.Max(hands.Effort(0), hands.Effort(1)); steps++;
                        }
                        slowest = Mathf.Min(slowest, carry.Pace); pulled = Mathf.Max(pulled, carry.Pull);
                        if (hands.Held != null && unit.Motor.IsMoving)
                        {
                            // How high the tool's lowest part and the holding hand are over the ground, while it walks.
                            float low = float.MaxValue;
                            foreach (var solid in hands.Held.GetComponents<Collider>()) low = Mathf.Min(low, solid.bounds.min.y);
                            lows += low - ground.Height(hands.Held.worldCenterOfMass.x, hands.Held.worldCenterOfMass.z);
                            int holding = hands.Holds(1) ? 1 : 0;
                            handUps += hands.GripPlace(holding).y - unit.transform.position.y; walked++;
                        }
                        if (frames <= 0 || shot >= frames || frame++ % 2 != 0) return;
                        Vector3 target = unit.transform.position + Vector3.up * height * .5f;
                        Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * away;
                        dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                        camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                        CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{shot:000}"), 360, 360);
                        shot++;
                    };
                    // It takes the tool as it will carry it, then walks.
                    for (float until = Time.time + 2.2f; Time.time < until;) yield return null;
                    var way = carry.way;
                    Vector3 first = unit.transform.position, goal = OnGround(spot + away * walk);
                    float began = Time.time;
                    bool went = unit.Motor.TryMove(goal);
                    while (went && Time.time - began < 30 && (unit.Motor.IsMoving || Time.time - began < .5f) && carry.way != PhysicalCarry.Way.Left) yield return null;
                    float took = Time.time - began, covered = Vector3.Distance(unit.transform.position, first);
                    for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                    while (frames > 0 && shot < frames && Time.time - began < 60) yield return null;
                    then.Then = null;
                    UnityEngine.Object.Destroy(then.gameObject);
                    Debug.Log(string.Format(culture,
                        "BENCH {0} strength {1:0.00} pickaxe {2:0.00} kg: holding it asks {3:0}% of one hand's hold: {4}{5}; it walked {6:0.00} m in {7:0.0} s ({8:0.00} m/s; its own pace is {9:0.00} m/s); slowest {10:0}% of its pace, pulling {11:0} N at most; "
                        + "its hand gave {12:0}% of its hold and its arm {13:0}% of what it has on average; hands at most {14:0.0} mm off the handle ({18}); its arms are {15:0}% and {16:0}% spent; its hips leaned {17:0} mm at most; walking, the tool's lowest part was {19:0.000} m over the ground and the hand that held it {20:0.000} m up, on average",
                        who, strength, hands.ToolMass > 0 ? hands.ToolMass : definition.Mass * weight, carry.Asks * 100, way, carry.way != way ? " then " + carry.way : "", covered, took, covered / Mathf.Max(.01f, took), agent.speed / Mathf.Max(.05f, carry.Pace),
                        slowest * 100, pulled, hold / Mathf.Max(1, steps) * 100, effort / Mathf.Max(1, steps) * 100, miss * 1000,
                        physical.Spent(PhysicalBody.Muscles.LeftArm) * 100, physical.Spent(PhysicalBody.Muscles.RightArm) * 100, balance.MostLean * 1000, when, lows / Mathf.Max(1, walked), handUps / Mathf.Max(1, walked)));
                    unit.Motor.Stop();
                    var was = hands.Drop();
                    if (was != null) UnityEngine.Object.Destroy(was.gameObject);
                    if (carry.Lies != null) UnityEngine.Object.Destroy(carry.Lies.gameObject);
                    UnityEngine.Object.Destroy(carry);
                    UnityEngine.Object.Destroy(swing);
                    UnityEngine.Object.Destroy(hands);
                    UnityEngine.Object.Destroy(back);
                    UnityEngine.Object.Destroy(balance);
                    physical.Strength = 1;
                    physical.Refresh();
                    for (float until = Time.time + .3f; Time.time < until;) yield return null;
                }
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
