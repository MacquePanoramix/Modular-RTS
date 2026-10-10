using System.Collections;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The place at dusk, seen from the game's own camera at every step of its zoom (Luis, October 8: the fireflies
    // when the camera pulls back, and the cabin's light, "examine at each zoom out"). Time stands still while it is
    // taken, so that what changes from one picture to the next is the camera alone.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.DuskZoomCapture -zoomOut <folder>
    //   [-zoomHour 19.2] [-zoomSteps 40] [-zoomFrom 5] [-zoomTo 0 (the camera's own farthest)] [-zoomSize 960,540]
    //   [-zoomYaw 0] [-zoomAt door|miner] [-zoomDark 1: each step also without the fireflies, to tell them from the ground]
    //   [-zoomFlies seenFromFar=.1,fewestFrom=50: the fireflies' own settings, to try others]
    //   [-zoomShaded 1: the lamplight shaded by what stands in it] [-zoomReach 0: without ShadowReach]
    public sealed class DuskZoomCapture
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

        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator Sweep()
        {
            string folder = CaptureTools.Argument("-zoomOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Zoom");
            float hour = Numbers("-zoomHour", 19.2f)[0];
            int steps = (int)Numbers("-zoomSteps", 40)[0];
            float[] size = Numbers("-zoomSize", 960, 540);
            float yawGiven = Numbers("-zoomYaw", float.NaN)[0];
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            var time = Object.FindAnyObjectByType<TimeOfDay>();
            var choice = Object.FindAnyObjectByType<MinerChoice>();
            var flies = Object.FindAnyObjectByType<Fireflies>();
            bool dark = CaptureTools.Argument("-zoomDark") == "1";
            string tried = CaptureTools.Argument("-zoomFlies");
            if (flies != null && !string.IsNullOrEmpty(tried))
            {
                foreach (string one in tried.Split(','))
                {
                    var pair = one.Split('=');
                    var field = typeof(Fireflies).GetField(pair[0], System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Assert.That(field, Is.Not.Null, $"The fireflies have no setting called {pair[0]}.");
                    field.SetValue(flies, float.Parse(pair[1], CultureInfo.InvariantCulture));
                }
                flies.enabled = false;
                flies.enabled = true;
            }
            if (flies != null) Debug.Log(string.Format(CultureInfo.InvariantCulture, "ZOOM fireflies: {0} live in the meadow, one to {1:0} m2; seen from 20 m {2:0.00} of them at {3:0.00} of their light, from 40 m {4:0.00} at {5:0.00}, from 60 m {6:0.00} at {7:0.00}",
                flies.Homes.Count, flies.MeadowEach, flies.ShareSeen(20), flies.Brightness(20), flies.ShareSeen(40), flies.Brightness(40), flies.ShareSeen(60), flies.Brightness(60)));
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            float near = Numbers("-zoomFrom", 5)[0], far = Numbers("-zoomTo", 0)[0], yaw = 0;
            if (rig != null)
            {
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                float nearest = (float)typeof(RtsCamera).GetField("minDistance", flags).GetValue(rig), farthest = (float)typeof(RtsCamera).GetField("maxDistance", flags).GetValue(rig);
                yaw = rig.StrategyYaw;
                Debug.Log(string.Format(culture, "ZOOM the camera's own zoom goes from {0:0.0} to {1:0.0} m; its yaw {2:0}; it looks at {3}", nearest, farthest, yaw, rig.StrategyTarget.ToString("F1")));
                if (far <= 0) far = farthest;
                rig.enabled = false;
            }
            if (far <= 0) far = 42;
            if (!float.IsNaN(yawGiven)) yaw = yawGiven;
            time.Hour = hour;
            // What the camera looks at: the ground before the door, or the chosen miner.
            Vector3 door = Vector3.zero;
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (light.name.StartsWith("Door")) door = light.transform.position;
            Vector3 target = CaptureTools.Argument("-zoomAt") == "miner" && choice != null && choice.Current != null ? choice.Current.transform.position : new Vector3(door.x, 0, door.z);
            var ground = Object.FindAnyObjectByType<OrdinaryGround>();
            if (ground != null) target.y = ground.Height(target.x, target.z);
            Debug.Log(string.Format(culture, "ZOOM the door's light is at {0}; the camera looks at {1}; the hour {2:0.00}", door.ToString("F2"), target.ToString("F2"), hour));
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                Debug.Log(string.Format(culture, "ZOOM light {0}: {1}, range {2:0.0}, intensity {3:0.00}, at {4}", light.name, light.type, light.range, light.intensity, light.transform.position.ToString("F1")));
            if (CaptureTools.Argument("-zoomShaded") == "1") time.LampShadows = true;
            if (CaptureTools.Argument("-zoomReach") == "0")
                foreach (var reach in Object.FindObjectsByType<ShadowReach>(FindObjectsSortMode.None)) reach.enabled = false;
            // Let the place settle (the grass, the hour), then stop time.
            for (int i = 0; i < 30; i++) yield return null;
            Time.timeScale = 0;
            try
            {
                for (int step = 0; step < steps; step++)
                {
                    float range = near * Mathf.Pow(far / near, steps > 1 ? step / (float)(steps - 1) : 0);
                    float pitch = Mathf.Lerp(32, 62, Mathf.InverseLerp(near, far, range));
                    var rotation = Quaternion.Euler(pitch, yaw, 0);
                    camera.transform.SetPositionAndRotation(target - rotation * Vector3.forward * range, rotation);
                    yield return null;
                    CaptureTools.Render(camera, Path.Combine(folder, $"zoom_{step:000}"), (int)size[0], (int)size[1]);
                    if (flies != null && dark)
                    {
                        flies.enabled = false;
                        CaptureTools.Render(camera, Path.Combine(folder, $"dark_{step:000}"), (int)size[0], (int)size[1]);
                        flies.enabled = true;
                    }
                    Vector3 onScreen = camera.WorldToViewportPoint(new Vector3(door.x, target.y, door.z));
                    // How many metres a pixel is at the target, so that the same patch of ground can be read in each picture.
                    float metresHigh = 2 * range * Mathf.Tan(camera.fieldOfView * .5f * Mathf.Deg2Rad);
                    Debug.Log(string.Format(culture, "ZOOM_STEP {0} range {1:0.00} pitch {2:0.0} door at {3:0.0000},{4:0.0000} a picture's height is {5:0.00} m at the target; eye {6}",
                        step, range, pitch, onScreen.x, onScreen.y, metresHigh, camera.transform.position.ToString("F1")));
                }
            }
            finally
            {
                Time.timeScale = 1;
            }
        }
    }
}
