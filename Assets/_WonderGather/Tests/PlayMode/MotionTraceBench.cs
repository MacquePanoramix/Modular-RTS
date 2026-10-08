using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // Where every part of a miner is at every frame of a movement, as it is shown: its place in the world, its
    // hips, chest and head, each foot and whether it is planted. Taken after the body is posed, at the frame rate
    // that is asked for (Luis plays at about a hundred frames a second; the physics steps fifty times). What a person
    // sees as a stutter or a teleport is a break in these lines, and can be found in them (Art/Review/motion_breaks.py).
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MotionTraceBench -traceOut <folder>
    //   [-traceMiner Small|Long|Round (all)] [-traceWhat walk,turn,turns (all)] [-traceFrames 100] [-traceTool 1]
    //   [-traceSize 0: pictures this many pixels square, every frame] [-traceView 90,8,3.4]
    public sealed class MotionTraceBench
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

        private static float Yaw(Quaternion turn) => Mathf.Atan2((turn * Vector3.forward).x, (turn * Vector3.forward).z) * Mathf.Rad2Deg;

        [UnityTest, Explicit, Timeout(7200000)]
        public IEnumerator Trace()
        {
            string folder = CaptureTools.Argument("-traceOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Trace");
            string who = CaptureTools.Argument("-traceMiner");
            string what = CaptureTools.Argument("-traceWhat") ?? "walk,turn,turns";
            int frames = (int)Numbers("-traceFrames", 100)[0], size = (int)Numbers("-traceSize", 0)[0];
            bool tool = Numbers("-traceTool", 0)[0] > 0;
            float[] view = Numbers("-traceView", 90, 8, 3.4f);
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            int miners = Object.FindAnyObjectByType<MinerChoice>().Count;
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            try
            {
                for (int index = 0; index < miners; index++)
                    foreach (string movement in what.Split(','))
                    {
                        Time.captureFramerate = 0;
                        yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
                        yield return null;
                        Time.captureFramerate = frames;
                        var choice = Object.FindAnyObjectByType<MinerChoice>();
                        var look = Object.FindAnyObjectByType<MinerWorkPreview>();
                        var time = Object.FindAnyObjectByType<TimeOfDay>();
                        var camera = Camera.main;
                        if (camera.TryGetComponent<RtsCamera>(out var rig)) rig.enabled = false;
                        time.Hour = 15;
                        choice.Choose(index);
                        yield return null;
                        yield return null;
                        var unit = choice.Current;
                        string name = unit.name.Replace("Miner_", "").Replace("(Clone)", "").Trim();
                        if (!string.IsNullOrEmpty(who) && !string.Equals(who, name, System.StringComparison.OrdinalIgnoreCase)) continue;
                        var body = unit.GetComponent<ProceduralBiped>();
                        var pelvis = (Transform)typeof(ProceduralBiped).GetField("pelvis", flags).GetValue(body);
                        var torso = (Transform)typeof(ProceduralBiped).GetField("torso", flags).GetValue(body);
                        var head = (Transform)typeof(ProceduralBiped).GetField("head", flags).GetValue(body);
                        var feet = (Transform[])typeof(ProceduralBiped).GetField("feet", flags).GetValue(body);
                        var shins = (Transform[])typeof(ProceduralBiped).GetField("shins", flags).GetValue(body);
                        if (!tool && look != null && look.Showing) { look.Toggle(); yield return null; }
                        for (float until = Time.time + 1.5f; Time.time < until;) yield return null;

                        string label = $"{name.ToLowerInvariant()}_{movement}{(tool ? "_tool" : "")}";
                        var lines = new StringBuilder();
                        lines.AppendLine("t,dt,order,root_x,root_y,root_z,root_yaw,hips_x,hips_y,hips_z,hips_yaw,chest_yaw,head_x,head_y,head_z,head_yaw,"
                            + "footL_x,footL_y,footL_z,footL_yaw,footR_x,footR_y,footR_z,footR_yaw,plantedL,plantedR,kneeL_x,kneeL_y,kneeL_z,kneeR_x,kneeR_y,kneeR_z,gait,steps,sink,speed,swingL,swingR,cadence,pitchL,pitchR");
                        int order = 0, shot = 0;
                        bool tracing = false;
                        Vector3 centre = unit.transform.position;
                        var after = new GameObject("After everything").AddComponent<AfterEverything>();
                        after.Then = () =>
                        {
                            if (!tracing) return;
                            Vector3 r = unit.transform.position, h = pelvis.position, k = head.position, l = feet[0].position, f = feet[1].position, a = shins[0].position, b = shins[1].position;
                            lines.AppendLine(string.Format(culture,
                                "{0:0.0000},{1:0.00000},{2},{3:0.00000},{4:0.00000},{5:0.00000},{6:0.000},{7:0.00000},{8:0.00000},{9:0.00000},{10:0.000},{11:0.000},{12:0.00000},{13:0.00000},{14:0.00000},{15:0.000},"
                                + "{16:0.00000},{17:0.00000},{18:0.00000},{19:0.000},{20:0.00000},{21:0.00000},{22:0.00000},{23:0.000},{24},{25},{26:0.00000},{27:0.00000},{28:0.00000},{29:0.00000},{30:0.00000},{31:0.00000},{32},{33},{34:0.00000},{35:0.0000},{36:0.000},{37:0.000},{38:0.000},{39:0.00},{40:0.00}",
                                Time.time, Time.deltaTime, order, r.x, r.y, r.z, Yaw(unit.transform.rotation), h.x, h.y, h.z, Yaw(pelvis.rotation), Yaw(torso.rotation), k.x, k.y, k.z, Yaw(head.rotation),
                                l.x, l.y, l.z, Yaw(feet[0].rotation), f.x, f.y, f.z, Yaw(feet[1].rotation), body.FootPlanted(0) ? 1 : 0, body.FootPlanted(1) ? 1 : 0, a.x, a.y, a.z, b.x, b.y, b.z,
                                (int)body.CurrentGait, body.StepCount, body.SinkNow, body.VelocityNow.magnitude, body.SwingProgress(0), body.SwingProgress(1), body.Cadence, body.FootPitch(0), body.FootPitch(1)));
                            if (size <= 0) return;
                            Vector3 target = unit.transform.position + Vector3.up * body.StandingHipHeight * .95f;
                            Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * Vector3.forward;
                            dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                            camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                            CaptureTools.Render(camera, Path.Combine(folder, $"{label}_{shot:0000}"), size, size);
                            shot++;
                        };

                        // Places to walk to: a straight line over the walked ground, and back the way it came.
                        Vector3 start = unit.transform.position, way = unit.transform.forward;
                        Vector3 Reachable(Vector3 wanted) => NavMesh.SamplePosition(wanted, out var hit, 3, NavMesh.AllAreas) ? hit.position : wanted;
                        Vector3 ahead = Reachable(start + way * 7), behind = Reachable(start - way * 5);
                        tracing = true;
                        if (movement == "walk")
                        {
                            // Standing; then a walk of seven metres; then standing again.
                            for (float until = Time.time + .5f; Time.time < until;) yield return null;
                            order = 1;
                            Assert.That(unit.Motor.TryMove(ahead), Is.True, name + " could not be sent ahead");
                            for (float until = Time.time + 12; Time.time < until && (unit.Motor.IsMoving || Time.time < until - 11);) yield return null;
                            order = 2;
                            for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                        }
                        else if (movement == "turn")
                        {
                            // Standing, sent straight behind it: it turns round on the spot and walks.
                            for (float until = Time.time + .5f; Time.time < until;) yield return null;
                            order = 1;
                            Assert.That(unit.Motor.TryMove(behind), Is.True, name + " could not be sent behind");
                            for (float until = Time.time + 10; Time.time < until && (unit.Motor.IsMoving || Time.time < until - 9);) yield return null;
                            order = 2;
                            for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                        }
                        else
                        {
                            // Walking, sent back the way it came in mid-stride; and again.
                            Assert.That(unit.Motor.TryMove(ahead), Is.True, name + " could not be sent ahead");
                            for (float until = Time.time + 2.2f; Time.time < until;) yield return null;
                            order = 1;
                            unit.Motor.TryMove(behind);
                            for (float until = Time.time + 3.5f; Time.time < until;) yield return null;
                            order = 2;
                            unit.Motor.TryMove(ahead);
                            for (float until = Time.time + 3.5f; Time.time < until;) yield return null;
                            order = 3;
                            unit.Motor.Stop();
                            for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                        }
                        tracing = false;
                        Object.Destroy(after.gameObject);
                        File.WriteAllText(Path.Combine(folder, label + ".csv"), lines.ToString());
                        Debug.Log(string.Format(culture, "TRACE {0}: {1} frames at {2} a second, {3} steps, {4:0.00} m from where it began; {5} pictures", label, lines.ToString().Split('\n').Length - 2, frames, body.StepCount,
                            Vector3.Distance(unit.transform.position, start), shot));
                    }
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
