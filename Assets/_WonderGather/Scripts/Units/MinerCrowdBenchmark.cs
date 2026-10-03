using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // What the miners cost in a crowd (S1d). Started with -wgcrowd: fills the meadow with 0, 25, 50 and then 100
    // miners (the three in turn), each walking between random places, and holds the Strategy camera's view and a
    // close view at each count. Writes miner-crowd-benchmark.csv beside the player log, then quits.
    public sealed class MinerCrowdBenchmark : MonoBehaviour
    {
        [SerializeField] private GameObject[] miners;
        [SerializeField] private float settleSeconds = 2, sampleSeconds = 3, radius = 22;
        private readonly List<UnitMotor> crowd = new List<UnitMotor>();
        private System.Random random;

        public void Configure(GameObject[] prefabs) => miners = prefabs;

        private void Start()
        {
            if (!Environment.GetCommandLineArgs().Contains("-wgcrowd") || miners == null || miners.Length == 0) { enabled = false; return; }
            random = new System.Random(7);
            StartCoroutine(Run());
        }

        private Vector3 Somewhere(Vector3 around)
        {
            for (int tries = 0; tries < 20; tries++)
            {
                double angle = random.NextDouble() * Math.PI * 2, distance = Math.Sqrt(random.NextDouble()) * radius;
                var point = around + new Vector3((float)(Math.Cos(angle) * distance), 0, (float)(Math.Sin(angle) * distance));
                if (NavMesh.SamplePosition(point + Vector3.up * 5, out var hit, 10, NavMesh.AllAreas)) return hit.position;
            }
            return around;
        }

        private IEnumerator Run()
        {
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            var choice = FindAnyObjectByType<MinerChoice>();
            if (choice != null) choice.enabled = false;
            var time = FindAnyObjectByType<TimeOfDay>();
            if (time != null) time.Hour = 10;
            var centre = choice != null && choice.Current != null ? choice.Current.transform.position : Vector3.zero;
            var timings = new FrameTiming[1];
            var report = new StringBuilder("miners,view,cpu_ms_mean,cpu_ms_p95,gpu_ms_mean,frames\n");
            Debug.Log($"Wonder Gather crowd benchmark: {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceName}, {SystemInfo.processorType}");
            var views = new (string Name, Vector3 Eye, Vector3 Target)[]
            {
                ("strategy", centre + new Vector3(-6, 16, -16), centre + new Vector3(0, 0, 2)),
                ("close", centre + new Vector3(-3, 1.6f, -6), centre + new Vector3(0, 1, 2)),
            };
            foreach (int count in new[] { 0, 25, 50, 100 })
            {
                while (crowd.Count < count)
                {
                    var unit = Instantiate(miners[crowd.Count % miners.Length], Somewhere(centre), Quaternion.Euler(0, (float)random.NextDouble() * 360, 0));
                    unit.name = "Crowd miner " + crowd.Count;
                    crowd.Add(unit.GetComponent<UnitMotor>());
                }
                foreach (var view in views)
                {
                    camera.transform.SetPositionAndRotation(view.Eye, Quaternion.LookRotation(view.Target - view.Eye));
                    float settle = Time.realtimeSinceStartup + settleSeconds;
                    while (Time.realtimeSinceStartup < settle) { Wander(centre); yield return null; }
                    var cpu = new List<float>();
                    var gpu = new List<double>();
                    float end = Time.realtimeSinceStartup + sampleSeconds;
                    while (Time.realtimeSinceStartup < end)
                    {
                        Wander(centre);
                        yield return null;
                        cpu.Add(Time.unscaledDeltaTime * 1000);
                        FrameTimingManager.CaptureFrameTimings();
                        if (FrameTimingManager.GetLatestTimings(1, timings) > 0 && timings[0].gpuFrameTime > 0) gpu.Add(timings[0].gpuFrameTime);
                    }
                    cpu.Sort();
                    float p95 = cpu[Mathf.Clamp(Mathf.CeilToInt(cpu.Count * .95f) - 1, 0, cpu.Count - 1)];
                    var invariant = CultureInfo.InvariantCulture;
                    report.AppendLine(string.Join(",", count.ToString(invariant), view.Name, cpu.Average().ToString("F2", invariant),
                        p95.ToString("F2", invariant), gpu.Count > 0 ? gpu.Average().ToString("F2", invariant) : "", cpu.Count.ToString(invariant)));
                }
            }
            string path = Path.Combine(Path.GetDirectoryName(Application.consoleLogPath) ?? Application.persistentDataPath, "miner-crowd-benchmark.csv");
            File.WriteAllText(path, report.ToString());
            Debug.Log("Wonder Gather crowd benchmark written to " + path);
            Application.Quit();
        }

        // Each idle miner sets off somewhere new.
        private void Wander(Vector3 centre)
        {
            foreach (var motor in crowd)
                if (motor != null && !motor.IsMoving && random.NextDouble() < .02) motor.TryMove(Somewhere(centre));
        }
    }
}
