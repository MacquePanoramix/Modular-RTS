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
    // The bench of S3's step 6 (Docs/NextMilestonePlan.md): a miner standing in the Ordinary Place, keeping its own
    // balance (PhysicalBalance), and something pulling it at the chest: at several strengths of pull, one after the
    // other. What it does about each (nothing to see, a lean, a step, several) comes out of its weight, its height
    // and its feet.
    //
    // It writes what each came to (BENCH ...), and with -balanceFrames every other frame of each from the side, with
    // what it stands on and where its weight is at each (BALANCE ...), for a drawing beside the picture.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.PhysicalBalanceBench -balanceOut <folder>
    //   [-balanceMiner Round] [-balancePulls 60,120,240] [-balanceWay 0] [-balanceFor 2.5] [-balanceFrames 150] [-balanceView 100,8]
    public sealed class PhysicalBalanceBench
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
            string folder = CaptureTools.Argument("-balanceOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Balance");
            string who = CaptureTools.Argument("-balanceMiner") ?? "Round";
            // The pulls, in newtons; the way each pulls, in degrees round the miner (0: forwards, 90: to its right, 180:
            // backwards); how long each lasts; and how long the miner is watched after it lets go.
            float[] pulls = Numbers("-balancePulls", 60, 120, 240);
            float way = Numbers("-balanceWay", 0)[0], lasts = Numbers("-balanceFor", 2.5f)[0], after = Numbers("-balanceAfter", 2)[0];
            float[] view = Numbers("-balanceView", 100, 8, 2.7f);
            int frames = (int)Numbers("-balanceFrames", 0)[0];
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            Time.captureFramerate = (int)Numbers("-balanceRate", 50)[0];
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
                Vector3 across = Vector3.Cross(Vector3.up, away);
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (string.Equals(choice.NameOf(i), who, StringComparison.OrdinalIgnoreCase)) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner is called " + who);
                var culture = CultureInfo.InvariantCulture;

                foreach (float newtons in pulls)
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
                    for (float until = Time.time + .6f; Time.time < until;) yield return null;
                    float height = miner.Rig.head.position.y - unit.transform.position.y;
                    var balance = unit.gameObject.AddComponent<PhysicalBalance>();
                    for (float until = Time.time + .8f; Time.time < until;) yield return null;
                    balance.Mark();
                    float began = Time.time;
                    Vector3 towards = Quaternion.AngleAxis(way, Vector3.up) * away;
                    // It comes on over a third of a second, holds, and lets go at once.
                    float Now() => Time.time - began < .5f || Time.time - began > .5f + lasts ? 0 : newtons * Mathf.Clamp01((Time.time - began - .5f) / .35f);
                    Vector3 Chest() => biped.HipsNow + Vector3.up * (.3f * height);
                    var pull = unit.gameObject.AddComponent<BalancePull>();
                    pull.balance = balance; pull.force = () => towards * Now(); pull.at = Chest;
                    // What pulls it, to be seen: a rope from its chest, as long as the pull is strong.
                    var rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    rope.name = "Bench pull";
                    UnityEngine.Object.Destroy(rope.GetComponent<Collider>());
                    var paint = Shader.Find("Wonder Gather/Painted");
                    if (paint != null)
                    {
                        var hemp = new Material(paint) { name = "Bench pull" };
                        hemp.SetColor("_BaseColor", new Color(.75f, .6f, .35f));
                        rope.GetComponent<MeshRenderer>().sharedMaterial = hemp;
                    }
                    string tag = string.Format(culture, "{0}_p{1:0}_w{2:0}", who.ToLowerInvariant(), newtons, way);
                    int shot = 0, frame = 0;
                    Vector3 first = unit.transform.position;
                    var then = new GameObject("After everything").AddComponent<AfterEverything>();
                    then.Then = () =>
                    {
                        float force = Now();
                        rope.SetActive(force > 0);
                        if (force > 0)
                        {
                            Vector3 from = Chest(), to = from + towards * 1.6f;
                            rope.transform.SetPositionAndRotation((from + to) * .5f, Quaternion.FromToRotation(Vector3.up, towards));
                            rope.transform.localScale = new Vector3(.03f, .8f, .03f);
                        }
                        if (frames <= 0 || shot >= frames || frame++ % 2 != 0) return;
                        Vector3 target = first + Vector3.up * height * .5f + towards * .3f;
                        Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * away;
                        dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                        camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                        CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{shot:000}"), 360, 360);
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
                        line.AppendFormat(culture, " {0:0.000} {1:0}", balance.Margin, force);
                        Debug.Log(line.ToString());
                        shot++;
                    };
                    float watch = .5f + lasts + after;
                    while (Time.time - began < watch || (frames > 0 && shot < frames && Time.time - began < watch + 20)) yield return null;
                    then.Then = null;
                    UnityEngine.Object.Destroy(then.gameObject);
                    Vector3 went = unit.transform.position - first;
                    Debug.Log(string.Format(culture,
                        "BENCH {0} ({1:0.0} kg) pulled with {2:0} N towards {3:0} degrees for {4:0.0} s: its weight's point came no nearer the edge of its feet than {5:0} mm (negative: outside them); its hips leaned {6:0} mm at most; it took {7} steps to catch itself; it ended {8:0.00} m ahead and {9:0.00} m to its right of where it stood, its hips leaning {10:0} mm",
                        who, physical.Mass, newtons, way, lasts, balance.LeastMargin * 1000, balance.MostLean * 1000, balance.Steps,
                        Vector3.Dot(went, away), Vector3.Dot(went, across), balance.Lean.magnitude * 1000));
                    UnityEngine.Object.Destroy(pull);
                    UnityEngine.Object.Destroy(balance);
                    UnityEngine.Object.Destroy(rope);
                    for (float until = Time.time + .2f; Time.time < until;) yield return null;
                }
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
