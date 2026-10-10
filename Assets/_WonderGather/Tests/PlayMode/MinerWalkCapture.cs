using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // Not a test: frames of the rigged miners walking in the Ordinary Place, for judging by eye (S1d).
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MinerWalkCapture -captureOut <folder>
    [Explicit("A capture for review, not a test.")]
    public sealed class MinerWalkCapture
    {
        private static string Folder()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "-captureOut") return args[i + 1];
            return Path.Combine(Application.dataPath, "..", "Captures", "MinerWalk");
        }

        private static void Render(Camera camera, string path, int width, int height)
        {
            var target = new RenderTexture(width, height, 24);
            var previous = RenderTexture.active;
            camera.targetTexture = target;
            for (int i = 0; i < 3; i++) RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });
            RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            // A plain PPM image (the image conversion module is not part of this project); rows bottom to top.
            var pixels = texture.GetPixels32();
            using (var file = new FileStream(Path.ChangeExtension(path, ".ppm"), FileMode.Create))
            {
                var header = System.Text.Encoding.ASCII.GetBytes("P6" + (char)10 + width + " " + height + (char)10 + "255" + (char)10);
                file.Write(header, 0, header.Length);
                var row = new byte[width * 3];
                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var c = pixels[y * width + x];
                        row[x * 3] = c.r; row[x * 3 + 1] = c.g; row[x * 3 + 2] = c.b;
                    }
                    file.Write(row, 0, row.Length);
                }
            }
            camera.targetTexture = null;
            RenderTexture.active = previous;
            UnityEngine.Object.Destroy(texture);
            UnityEngine.Object.Destroy(target);
        }

        [UnityTest] public IEnumerator Capture()
        {
            string folder = Folder();
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            choice.enabled = false;
            var step = ground.Path[0];
            Vector3 OnGround(float x, float z) => new Vector3(x, ground.Height(x, z), z);
            var door = OnGround(step.x, step.y - .8f);
            var start = OnGround(step.x + .3f, step.y - 9f);

            foreach (var hour in new[] { 10f, 19.2f })
            {
                time.Hour = hour;
                string when = hour < 12 ? "day" : "dusk";
                for (int index = 0; index < choice.Count; index++)
                {
                    choice.enabled = true;
                    choice.Choose(index);
                    choice.enabled = false;
                    var unit = choice.Current;
                    var agent = unit.GetComponent<NavMeshAgent>();
                    agent.Warp(start);
                    unit.transform.rotation = Quaternion.LookRotation(door - start);
                    for (int k = 0; k < 10; k++) yield return null;
                    unit.Motor.TryMove(door);
                    string name = choice.NameOf(index).ToLowerInvariant();
                    float began = Time.time;
                    int shot = 0;
                    while (shot < 6 && Time.time - began < 20)
                    {
                        yield return null;
                        if (Time.time - began < 1.6f + shot * .23f) continue;
                        // From the side, following, low like the Explore camera; then a front three-quarter view.
                        var at = unit.transform.position;
                        var side = Vector3.Cross(Vector3.up, unit.transform.forward).normalized;
                        var eye = at + side * 3.2f + Vector3.up * 1.0f + unit.transform.forward * .4f;
                        camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(at + Vector3.up * .75f - eye));
                        Render(camera, Path.Combine(folder, $"{name}_{when}_side{shot}.png"), 640, 720);
                        if (shot == 2)
                        {
                            var front = at + unit.transform.forward * 3.0f - side * 1.2f + Vector3.up * 1.1f;
                            camera.transform.SetPositionAndRotation(front, Quaternion.LookRotation(at + Vector3.up * .8f - front));
                            Render(camera, Path.Combine(folder, $"{name}_{when}_front.png"), 900, 900);
                            var high = at + new Vector3(-5, 12, -12);
                            camera.transform.SetPositionAndRotation(high, Quaternion.LookRotation(at - high));
                            Render(camera, Path.Combine(folder, $"{name}_{when}_strategy.png"), 1200, 700);
                        }
                        shot++;
                    }
                    unit.Motor.Stop();
                }
            }

            // All three together, walking side by side towards the door at dusk.
            time.Hour = 19.2f;
            var all = UnityEngine.Object.FindObjectsByType<MinerBody>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OrderBy(x => Array.IndexOf(new[] { "Long", "Small", "Round" }, x.name.Replace("Miner ", "").Replace("(Clone)", "").Trim())).ToArray();
            for (int i = 0; i < all.Length; i++)
            {
                all[i].gameObject.SetActive(true);
                var offset = new Vector3((i - 1) * 1.3f, 0, 0);
                var from = OnGround(start.x + offset.x, start.z + 1.5f);
                all[i].GetComponent<NavMeshAgent>().Warp(from);
                all[i].transform.rotation = Quaternion.LookRotation(door - from);
            }
            for (int k = 0; k < 10; k++) yield return null;
            for (int i = 0; i < all.Length; i++)
            {
                var offset = new Vector3((i - 1) * 1.3f, 0, 0);
                all[i].GetComponent<UnitMotor>().TryMove(OnGround(door.x + offset.x, door.z - 1.2f));
            }
            float t0 = Time.time;
            while (Time.time - t0 < 2.6f) yield return null;
            var centre = all.Aggregate(Vector3.zero, (s, x) => s + x.transform.position) / all.Length;
            var group = centre + new Vector3(.4f, 1.0f, 5.5f);
            camera.transform.SetPositionAndRotation(group, Quaternion.LookRotation(centre + Vector3.up * .8f - group));
            Render(camera, Path.Combine(folder, "together_dusk.png"), 1600, 900);
            var groupHigh = centre + new Vector3(-6, 14, -10);
            camera.transform.SetPositionAndRotation(groupHigh, Quaternion.LookRotation(centre - groupHigh));
            Render(camera, Path.Combine(folder, "together_strategy.png"), 1600, 900);
            Debug.Log("MINER_WALK_CAPTURE_OK " + folder);
        }
    }
}
