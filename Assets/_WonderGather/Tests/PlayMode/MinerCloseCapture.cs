using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // Not a test: the rigged miners seen as close as the Explore camera goes, standing and mid-stride, from every
    // side, for a careful look at every detail (S1d polish). Frames are PPM images.
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MinerCloseCapture -captureOut <folder> [-captureMiner Small]
    [Explicit("A capture for review, not a test.")]
    public sealed class MinerCloseCapture
    {
        private static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
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
            var pixels = texture.GetPixels32();
            using (var file = new FileStream(path + ".ppm", FileMode.Create))
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
            string folder = Argument("-captureOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "MinerClose");
            string only = Argument("-captureMiner");
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            var step = ground.Path[0];
            Vector3 OnGround(float x, float z) => new Vector3(x, ground.Height(x, z), z);
            var spot = OnGround(step.x + .2f, step.y - 5f);
            var door = OnGround(step.x, step.y - .8f);
            time.Hour = 19.2f;

            for (int index = 0; index < choice.Count; index++)
            {
                string name = choice.NameOf(index);
                if (only != null && !string.Equals(only, name, StringComparison.OrdinalIgnoreCase)) continue;
                choice.Choose(index);
                yield return null;
                var unit = choice.Current;
                var body = unit.GetComponent<MinerBody>();
                var agent = unit.GetComponent<NavMeshAgent>();
                agent.Warp(spot);
                unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(door - spot));
                unit.GetComponent<ProceduralBiped>().ResetPose();
                yield return null;
                for (int k = 0; k < 40; k++) yield return null;
                string tag = name.ToLowerInvariant();
                var biped = unit.GetComponent<ProceduralBiped>();
                Debug.Log($"MINER_CLOSE_DEBUG {name} unit {unit.transform.position} pelvis bone {body.Rig.pelvis.position} feet {body.Rig.feet[0].position} biped ready {biped.Ready} foot {biped.FootPosition(0)} active {unit.gameObject.activeInHierarchy}");
                float height = body.Rig.head.position.y - spot.y;
                Vector3 At(float up) => unit.transform.position + Vector3.up * up;
                var f = unit.transform.forward;
                var r = unit.transform.right;
                void Shot(string label, Vector3 eye, Vector3 target, int w = 900, int h = 900)
                {
                    camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye));
                    Render(camera, Path.Combine(folder, $"{tag}_{label}"), w, h);
                }
                // Standing, from every side, as close as the Explore camera goes.
                Shot("stand_front", At(height * .55f) + f * 2.2f, At(height * .5f), 800, 1000);
                Shot("stand_back", At(height * .55f) - f * 2.2f, At(height * .5f), 800, 1000);
                Shot("stand_left", At(height * .55f) - r * 2.2f, At(height * .5f), 800, 1000);
                Shot("stand_right", At(height * .55f) + r * 2.2f, At(height * .5f), 800, 1000);
                Shot("face", body.Rig.head.position + f * .75f + r * .2f, body.Rig.head.position - Vector3.up * .05f);
                Shot("neck_back", body.Rig.neck.position - f * .7f + Vector3.up * .25f, body.Rig.neck.position);
                Shot("neck_front", body.Rig.neck.position + f * .65f + Vector3.up * .05f, body.Rig.neck.position - Vector3.up * .05f);
                Shot("hands", At(height * .4f) + f * 1.0f + r * .25f, At(height * .4f));
                Shot("feet", At(.25f) + f * .9f + r * .3f + Vector3.up * .15f, At(.12f));
                Shot("feet_back", At(.25f) - f * .9f - r * .2f + Vector3.up * .15f, At(.12f));
                // Mid-stride.
                unit.Motor.TryMove(door);
                float began = Time.time;
                while (Time.time - began < 1.9f) yield return null;
                f = unit.transform.forward;
                r = unit.transform.right;
                Shot("walk_side", At(height * .5f) + r * 2.0f, At(height * .45f), 800, 1000);
                Shot("walk_front", At(height * .5f) + f * 2.2f - r * .3f, At(height * .45f), 800, 1000);
                Shot("walk_back", At(height * .6f) - f * 2.0f + r * .4f, At(height * .45f), 800, 1000);
                Shot("walk_feet", At(.3f) + r * 1.1f + f * .2f, At(.15f));
                unit.Motor.Stop();
                yield return null;
            }
            Debug.Log("MINER_CLOSE_CAPTURE_OK " + folder);
        }
    }
}
