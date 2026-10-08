using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WonderGather
{
    // What one miner's physical work costs (S3, step 12). Started with -wgwork: each of the three miners in turn is
    // measured standing, then has a pickaxe put on the ground beside it, is told to mine the nearest boulder of
    // ordinary height, and is measured again at its work, from the same close view. Writes miner-work-benchmark.csv
    // beside the player log, then quits. (The crowd, walking, is MinerCrowdBenchmark's: -wgcrowd.)
    [RequireComponent(typeof(MinerChoice), typeof(MinerWorkPreview))]
    public sealed class MinerWorkBenchmark : MonoBehaviour
    {
        // How long it settles before each measurement, how long each lasts, and how long it may take to come to its
        // work (seconds).
        private const float Settles = 2, Samples = 12, ComesWithin = 60;
        // A boulder of ordinary height: its top at least this far over the ground at its middle (metres).
        private const float OrdinaryTop = .5f;

        private void Start()
        {
            if (!Environment.GetCommandLineArgs().Contains("-wgwork")) { enabled = false; return; }
            // The game pauses when its window is not in front. A measurement must not.
            Application.runInBackground = true;
            StartCoroutine(Run());
        }

        private static IEnumerator Sample(float seconds, List<float> cpu, List<double> gpu)
        {
            var timings = new FrameTiming[1];
            cpu.Clear(); gpu.Clear();
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                yield return null;
                cpu.Add(Time.unscaledDeltaTime * 1000);
                FrameTimingManager.CaptureFrameTimings();
                if (FrameTimingManager.GetLatestTimings(1, timings) > 0 && timings[0].gpuFrameTime > 0) gpu.Add(timings[0].gpuFrameTime);
            }
        }

        private static string Line(string who, string what, List<float> cpu, List<double> gpu, string more)
        {
            var invariant = CultureInfo.InvariantCulture;
            var sorted = new List<float>(cpu);
            sorted.Sort();
            float p95 = sorted.Count > 0 ? sorted[Mathf.Clamp(Mathf.CeilToInt(sorted.Count * .95f) - 1, 0, sorted.Count - 1)] : 0;
            return string.Join(",", who, what, cpu.Count > 0 ? cpu.Average().ToString("F3", invariant) : "", p95.ToString("F3", invariant),
                gpu.Count > 0 ? gpu.Average().ToString("F3", invariant) : "", cpu.Count.ToString(invariant), more);
        }

        private IEnumerator Run()
        {
            var choice = GetComponent<MinerChoice>();
            var look = GetComponent<MinerWorkPreview>();
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            var time = FindAnyObjectByType<TimeOfDay>();
            if (time != null) time.Hour = 10;
            var invariant = CultureInfo.InvariantCulture;
            var report = new StringBuilder("miner,doing,cpu_ms_mean,cpu_ms_p95,gpu_ms_mean,frames,hands_microseconds_a_step,blows\n");
            Debug.Log($"Wonder Gather work benchmark: {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceName}, {SystemInfo.processorType}");
            var cpu = new List<float>();
            var gpu = new List<double>();
            yield return new WaitForSecondsRealtime(Settles);
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return new WaitForSecondsRealtime(1);
                var unit = choice.Current;
                string who = choice.NameOf(index);
                // The nearest boulder of ordinary height.
                Boulder boulder = null;
                float nearest = float.MaxValue;
                foreach (var one in Boulder.All())
                {
                    if (one.Rock == null) continue;
                    var bounds = one.Rock.bounds;
                    float top = bounds.max.y - unit.transform.position.y;
                    float away = Vector3.ProjectOnPlane(bounds.center - unit.transform.position, Vector3.up).sqrMagnitude;
                    if (top >= OrdinaryTop && away < nearest) { nearest = away; boulder = one; }
                }
                // Standing, from close by.
                void Look()
                {
                    Vector3 at = unit.transform.position + Vector3.up;
                    Vector3 from = at + (unit.transform.right * 2.2f - unit.transform.forward * 3 + Vector3.up * .6f);
                    camera.transform.SetPositionAndRotation(from, Quaternion.LookRotation(at - from));
                }
                Look();
                yield return new WaitForSecondsRealtime(Settles);
                yield return Sample(Samples * .5f, cpu, gpu);
                report.AppendLine(Line(who, "standing", cpu, gpu, ","));
                if (boulder == null) continue;
                // At its work.
                look.LayPickaxe(1);
                yield return new WaitForSecondsRealtime(1);
                look.Mine(boulder);
                float until = Time.realtimeSinceStartup + ComesWithin;
                while (!look.AtRock && Time.realtimeSinceStartup < until) yield return null;
                if (!look.AtRock)
                {
                    report.AppendLine(Line(who, "did not come to its work: " + look.LeftRock, cpu, gpu, ","));
                    look.End(); look.ClearLaid();
                    continue;
                }
                Look();
                yield return new WaitForSecondsRealtime(Settles);
                int blows = boulder.Blows;
                PhysicalHands.TimedTicks = 0; PhysicalHands.TimedSteps = 0; PhysicalHands.Timed = true;
                yield return Sample(Samples, cpu, gpu);
                PhysicalHands.Timed = false;
                double each = PhysicalHands.TimedSteps > 0 ? PhysicalHands.TimedTicks * 1e6 / System.Diagnostics.Stopwatch.Frequency / PhysicalHands.TimedSteps : 0;
                report.AppendLine(Line(who, "at its work", cpu, gpu, each.ToString("F1", invariant) + "," + (boulder.Blows - blows).ToString(invariant)));
                look.End(); look.ClearLaid();
                yield return new WaitForSecondsRealtime(1);
            }
            string path = Path.Combine(Path.GetDirectoryName(Application.consoleLogPath) ?? Application.persistentDataPath, "miner-work-benchmark.csv");
            File.WriteAllText(path, report.ToString());
            Debug.Log("Wonder Gather work benchmark written to " + path);
            Application.Quit();
        }
    }
}
