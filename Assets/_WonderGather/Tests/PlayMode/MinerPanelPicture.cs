using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // A picture of the panel as the screen shows it (S3, step 11). The panel is drawn by the engine's immediate
    // interface, which a camera's picture does not hold: this takes the screen itself, so it has to be run with a
    // window (without -batchmode).
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MinerPanelPicture -panelOut <folder>
    //   [-panelMiner Round]
    public sealed class MinerPanelPicture
    {
        [UnityTest, Explicit, Timeout(600000)]
        public IEnumerator Picture()
        {
            string folder = CaptureTools.Argument("-panelOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Panel");
            string who = CaptureTools.Argument("-panelMiner") ?? "Round";
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            var choice = Object.FindAnyObjectByType<MinerChoice>();
            var look = Object.FindAnyObjectByType<MinerWorkPreview>();
            var click = Object.FindAnyObjectByType<InteractionClick>();
            var time = Object.FindAnyObjectByType<TimeOfDay>();
            time.Hour = 13;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == who) choice.Choose(i);
            typeof(MinerChoice).GetField("open", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(choice, false);
            for (float until = Time.time + 1; Time.time < until;) yield return null;
            // From close by, so that what lies on the ground is seen.
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            var unit = choice.Current;
            Vector3 looksAt = unit.transform.position + Vector3.up * .45f + unit.transform.right * .25f;
            Vector3 from = looksAt + Quaternion.AngleAxis(-35, Vector3.up) * unit.transform.forward * 3.4f + Vector3.up * 1.5f;
            camera.transform.SetPositionAndRotation(from, Quaternion.LookRotation(looksAt - from));
            // The three pickaxes on the ground, as the panel's buttons put them.
            foreach (float share in MinerPanel.Weights) look.LayPickaxe(share);
            for (float until = Time.time + 2; Time.time < until;) yield return null;
            yield return Shot(Path.Combine(folder, "panel_" + who.ToLowerInvariant() + ".ppm"));
            // With the key held: what can be interacted with shows itself.
            click.Arm(true);
            yield return null;
            yield return Shot(Path.Combine(folder, "panel_" + who.ToLowerInvariant() + "_names.ppm"));
            click.Arm(false);
            Debug.Log($"PANEL_PICTURE the screen is {Screen.width} by {Screen.height}; pictures in {folder}");
        }

        private static IEnumerator Shot(string path)
        {
            yield return new WaitForEndOfFrame();
            // (What the screen shows at the end of the frame, the engine's own boxes and words with it.)
            var picture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            picture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            picture.Apply();
            int width = picture.width, height = picture.height;
            var pixels = picture.GetPixels32();
            using (var file = new FileStream(path, FileMode.Create))
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
            Object.Destroy(picture);
            yield return null;
        }
    }
}
