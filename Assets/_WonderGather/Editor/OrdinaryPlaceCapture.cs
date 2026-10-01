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
            bool full = Argument("-captureSet") == "full";
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
