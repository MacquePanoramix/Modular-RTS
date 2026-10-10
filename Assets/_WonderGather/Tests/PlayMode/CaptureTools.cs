using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather.Tests
{
    // Shared by the review captures and recordings: command-line arguments, and frames written as PPM images
    // (the project has no image-encoding module).
    internal static class CaptureTools
    {
        // The Ordinary Place as it was before a miner at ease stood by its own joints (its switch is on from the
        // start since October 10): for every test that is of the body as it was.
        public static void AsItWas()
        {
            var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
            if (look != null) look.SetOwn(false);
        }

        public static string Argument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
        }

        // A rock face to mine, for tests and captures: the same plain block the look at the work shows in play
        // (MinerWorkPreview). It is made at need, not in the scene: where the miners will mine is Luis's choice (M1).
        public static (ResourceNode node, ResourceDepot depot, GameObject rock) RockFace(Vector3 stand, Vector3 facing, float reach, Vector3 deliver, bool visible = false, float height = 2.6f)
            => MinerWorkPreview.RockFace(stand, facing, reach, deliver, visible, height);

        public static void Render(Camera camera, string path, int width, int height)
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
    }
}
