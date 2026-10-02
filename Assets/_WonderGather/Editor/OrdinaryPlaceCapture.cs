using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace WonderGather.Editor
{
    // Renders matched stills of The Ordinary Place for the S1c comparison: several camera
    // angles, near and far, at four times of day, in the rendering candidates.
    // Batch use (with graphics):
    //   Unity -batchmode -projectPath <project> -executeMethod WonderGather.Editor.OrdinaryPlaceCapture.Capture
    //         -captureOut <folder> [-captureSet quick|full] -quit
    public static class OrdinaryPlaceCapture
    {
        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
        }

        [MenuItem("Wonder Gather/Capture The Ordinary Place")]
        public static void Capture()
        {
            string folder = Argument("-captureOut") ?? "Captures/OrdinaryPlace";
            string set = Argument("-captureSet") ?? "quick";
            bool full = set == "full";
            if (set == "dusk") { Directory.CreateDirectory(folder); CaptureDusk(folder); return; }
            Directory.CreateDirectory(folder);
            bool previous = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            try
            {
                EditorSceneManager.OpenScene(OrdinaryPlaceSetup.ScenePath);
                var camera = Camera.main ?? throw new InvalidOperationException("The Ordinary Place has no main camera.");
                var time = Object.FindAnyObjectByType<TimeOfDay>();
                var look = Object.FindAnyObjectByType<LookDevControls>();
                var worker = Object.FindAnyObjectByType<SelectableUnit>();
                var w = worker != null ? worker.transform.position : new Vector3(-2, 0, -2);
                var views = new List<(string Name, Vector3 Eye, Vector3 Target)>
                {
                    ("overview", new Vector3(-10, 14, -12), new Vector3(0, 1, 5)),
                    ("path", new Vector3(-6.5f, 1.25f, -5.5f), new Vector3(.5f, 2.0f, 5.5f)),
                    ("grass", new Vector3(-2.6f, .32f, -1.2f), new Vector3(.4f, 1.3f, 5)),
                    ("door", new Vector3(2.6f, 1.6f, 1.2f), new Vector3(.3f, 1.4f, 5.8f)),
                    ("worker", w + new Vector3(1.8f, 1.6f, -2.6f), w + new Vector3(0, 1.3f, 0)),
                    ("sky", new Vector3(-5, 1.4f, -2), new Vector3(2, 9, 14)),
                    ("valley", new Vector3(6, 2.5f, 2), new Vector3(60, 10, -60)),
                };
                var hours = new List<(string Name, float Hour)> { ("night", 23), ("dusk", 18.6f), ("golden", 17), ("day", 10) };
                var report = new StringBuilder("view,time,look,milliseconds\n");
                foreach (var view in views)
                foreach (var hour in hours)
                {
                    var candidates = new List<int> { 1 };
                    if (full || view.Name == "path" || view.Name == "worker") candidates = new List<int> { 0, 1, 2, 3, 4 };
                    foreach (int candidate in candidates)
                    {
                        time.Hour = hour.Hour;
                        look.Select(candidate);
                        // Heights in the view list are above the ground beneath each point.
                        var eye = view.Eye + Vector3.up * Ground(view.Eye);
                        var target = view.Target;
                        target.y += view.Name == "worker" ? 0 : Ground(view.Target);
                        camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye));
                        string name = $"{view.Name}_{hour.Name}_{"ABCDE"[candidate]}.png";
                        double ms = Render(camera, Path.Combine(folder, name), 1600, 900);
                        report.AppendLine(string.Join(",", view.Name, hour.Name, "ABCDE"[candidate], ms.ToString("F1", CultureInfo.InvariantCulture)));
                    }
                }
                File.WriteAllText(Path.Combine(folder, "captures.csv"), report.ToString());
                Debug.Log("ORDINARY_PLACE_CAPTURE_OK " + folder);
            }
            finally
            {
                ShaderUtil.allowAsyncCompilation = previous;
            }
        }

        // Luis's favourite frame (October 2): look E at 19:12, low on the path in front of the lit
        // house, at his screenshot's wide aspect; and the doorway at night. Each dusk detail is
        // rendered off and on, so every step can be judged on exactly that frame.
        private static void CaptureDusk(string folder)
        {
            bool previous = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
            try
            {
                EditorSceneManager.OpenScene(OrdinaryPlaceSetup.ScenePath);
                var camera = Camera.main ?? throw new InvalidOperationException("The Ordinary Place has no main camera.");
                var time = Object.FindAnyObjectByType<TimeOfDay>();
                var look = Object.FindAnyObjectByType<LookDevControls>();
                var ground = Object.FindAnyObjectByType<OrdinaryGround>();
                var step = ground.Path[0];
                float Height(float x, float z) => ground.Height(x, z);
                var views = new List<(string Name, float Hour, Vector3 Eye, Vector3 Target, int Width, int Height)>
                {
                    ("favourite", 19.2f, new Vector3(step.x + .4f, Height(step.x + .4f, step.y - 7.2f) + .55f, step.y - 7.2f),
                        new Vector3(step.x - .1f, Height(step.x, step.y) + 3.4f, step.y + 4), 1600, 670),
                    ("doorway", 23, new Vector3(step.x + 2.4f, Height(step.x + 2.4f, step.y - 3.2f) + 1.5f, step.y - 3.2f),
                        new Vector3(step.x - .3f, Height(step.x, step.y) + 1.6f, step.y + 2.5f), 1600, 900),
                };
                // Luis's frame has no one in it; the worker steps out of the shot.
                var worker = Object.FindAnyObjectByType<SelectableUnit>();
                if (worker != null) worker.gameObject.SetActive(false);
                var details = new (string Name, bool Flies, bool Glow)[] { ("before", false, false), ("fireflies", true, false), ("glow", false, true), ("both", true, true) };
                foreach (var view in views)
                foreach (var detail in details)
                {
                    time.Hour = view.Hour;
                    look.Select(4);
                    look.SetDusk(detail.Flies, false, detail.Glow);
                    camera.transform.SetPositionAndRotation(view.Eye, Quaternion.LookRotation(view.Target - view.Eye));
                    Render(camera, Path.Combine(folder, $"{view.Name}_{detail.Name}.png"), view.Width, view.Height);
                }
                look.SetDusk(true, false, false);
                Debug.Log("ORDINARY_PLACE_CAPTURE_OK " + folder);
            }
            finally
            {
                ShaderUtil.allowAsyncCompilation = previous;
            }
        }

        private static float Ground(Vector3 point)
        {
            var ground = Object.FindAnyObjectByType<OrdinaryGround>();
            return ground != null ? ground.Height(point.x, point.z) : 0;
        }

        private static double Render(Camera camera, string path, int width, int height)
        {
            var target = new RenderTexture(width, height, 24);
            var previous = RenderTexture.active;
            camera.targetTexture = target;
            // Warm up shaders, shadows and history, then time the final frame.
            for (int i = 0; i < 3; i++) RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
            var clock = Stopwatch.StartNew();
            RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
            RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            double ms = clock.Elapsed.TotalMilliseconds;
            File.WriteAllBytes(path, texture.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(target);
            return ms;
        }
    }
}
