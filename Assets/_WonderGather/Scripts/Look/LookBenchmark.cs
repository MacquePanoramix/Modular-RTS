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
    // Measures the Ordinary Place's runtime cost (S1c). Started with -wgbenchmark: holds each
    // matched view at each time of day in each rendering candidate, records frame times,
    // writes ordinary-place-benchmark.csv beside the player log, then quits.
    public sealed class LookBenchmark : MonoBehaviour
    {
        [SerializeField] private LookDevControls look;
        [SerializeField] private TimeOfDay time;
        [SerializeField] private float settleSeconds = 1, sampleSeconds = 2;

        public void Configure(LookDevControls controls, TimeOfDay day)
        {
            look = controls;
            time = day;
        }

        private void Start()
        {
            if (!Environment.GetCommandLineArgs().Contains("-wgbenchmark")) { enabled = false; return; }
            StartCoroutine(Run());
        }

        private static IEnumerable<(string Name, Vector3 Eye, Vector3 Target)> Views()
        {
            yield return ("overview", new Vector3(-10, 14, -12), new Vector3(0, 1, 5));
            yield return ("path", new Vector3(-6.5f, 1.25f, -5.5f), new Vector3(.5f, 2, 5.5f));
            yield return ("grass", new Vector3(-2.6f, .32f, -1.2f), new Vector3(.4f, 1.3f, 5));
        }

        private IEnumerator Run()
        {
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            var ground = FindAnyObjectByType<OrdinaryGround>();
            var timings = new FrameTiming[1];
            var report = new StringBuilder("view,time,look,cpu_ms_mean,cpu_ms_p95,gpu_ms_mean,frames\n");
            var hours = new[] { ("night", 23f), ("day", 10f) };
            Debug.Log($"Wonder Gather benchmark: {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceName}, {SystemInfo.processorType}");
            foreach (var view in Views())
            foreach (var (hourName, hour) in hours)
            for (int candidate = 0; candidate < LookDevControls.Candidates.Length; candidate++)
            {
                time.Hour = hour;
                look.Select(candidate);
                var eye = view.Eye + Vector3.up * (ground != null ? ground.Height(view.Eye.x, view.Eye.z) : 0);
                var target = view.Target + Vector3.up * (ground != null ? ground.Height(view.Target.x, view.Target.z) : 0);
                camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye));
                yield return new WaitForSecondsRealtime(settleSeconds);
                var cpu = new List<float>();
                var gpu = new List<double>();
                float end = Time.realtimeSinceStartup + sampleSeconds;
                while (Time.realtimeSinceStartup < end)
                {
                    yield return null;
                    cpu.Add(Time.unscaledDeltaTime * 1000);
                    FrameTimingManager.CaptureFrameTimings();
                    if (FrameTimingManager.GetLatestTimings(1, timings) > 0 && timings[0].gpuFrameTime > 0) gpu.Add(timings[0].gpuFrameTime);
                }
                cpu.Sort();
                float p95 = cpu[Mathf.Clamp(Mathf.CeilToInt(cpu.Count * .95f) - 1, 0, cpu.Count - 1)];
                var invariant = CultureInfo.InvariantCulture;
                string gpuMean = gpu.Count > 0 ? gpu.Average().ToString("F2", invariant) : "";
                report.AppendLine(string.Join(",", view.Name, hourName, "ABCDE"[candidate], cpu.Average().ToString("F2", invariant),
                    p95.ToString("F2", invariant), gpuMean, cpu.Count.ToString(invariant)));
            }
            string path = Path.Combine(Path.GetDirectoryName(Application.consoleLogPath) ?? Application.persistentDataPath, "ordinary-place-benchmark.csv");
            File.WriteAllText(path, report.ToString());
            Debug.Log("Wonder Gather benchmark written to " + path);
            Application.Quit();
        }
    }
}
