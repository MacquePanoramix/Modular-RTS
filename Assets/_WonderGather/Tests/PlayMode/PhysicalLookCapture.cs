using System;
using System.Collections;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // Pictures of the look at the work as it is met in play (MinerWorkPreview's block of the bench): each miner where the place
    // puts it, the look begun as the key begins it, from its side and from the game's own camera.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.PhysicalLookCapture -lookOut <folder>
    //   [-lookFrames 90] [-lookStrength 1] [-lookWeight 1]
    public sealed class PhysicalLookCapture
    {
        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator Capture()
        {
            string folder = CaptureTools.Argument("-lookOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Look");
            var culture = CultureInfo.InvariantCulture;
            int frames = int.Parse(CaptureTools.Argument("-lookFrames") ?? "90", culture);
            float strength = float.Parse(CaptureTools.Argument("-lookStrength") ?? "1", culture);
            float weight = float.Parse(CaptureTools.Argument("-lookWeight") ?? "1", culture);
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            Time.captureFramerate = 50;
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                var camera = Camera.main;
                var rig = camera.GetComponent<RtsCamera>();
                time.Hour = 13;
                for (int index = 0; index < choice.Count; index++)
                {
                    choice.Choose(index);
                    for (float until = Time.time + .6f; Time.time < until;) yield return null;
                    var unit = choice.Current;
                    string who = choice.NameOf(index);
                    var miner = unit.GetComponent<MinerBody>();
                    float height = miner.Rig.head.position.y - unit.transform.position.y;
                    look.SetStrength(strength);
                    look.SetWeight(weight);
                    look.Toggle();
                    float began = Time.time;
                    while (!look.Swinging && Time.time - began < 5) yield return null;
                    Assert.That(look.Swinging, Is.True, who + " did not take up its pickaxe.");
                    // As the game's own camera shows it, when the look begins.
                    CaptureTools.Render(camera, Path.Combine(folder, $"{who.ToLowerInvariant()}_game"), 960, 540);
                    var seen = (camera.transform.position, camera.transform.rotation, camera.nearClipPlane);
                    if (rig != null) rig.enabled = false;
                    camera.nearClipPlane = .01f;
                    Vector3 away = Vector3.ProjectOnPlane(unit.transform.forward, Vector3.up).normalized;
                    int shot = 0, frame = 0;
                    var after = new GameObject("After everything").AddComponent<AfterEverything>();
                    after.Then = () =>
                    {
                        if (shot >= frames || frame++ % 2 != 0) return;
                        Vector3 target = unit.transform.position + Vector3.up * height * .55f + away * .3f;
                        Vector3 dir = Quaternion.AngleAxis(100, Vector3.up) * away;
                        dir = dir * Mathf.Cos(8 * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(8 * Mathf.Deg2Rad);
                        camera.transform.SetPositionAndRotation(target + dir * 3f, Quaternion.LookRotation(-dir));
                        CaptureTools.Render(camera, Path.Combine(folder, $"{who.ToLowerInvariant()}_{shot:000}"), 360, 360);
                        shot++;
                    };
                    while (shot < frames && Time.time - began < 60) yield return null;
                    after.Then = null;
                    UnityEngine.Object.Destroy(after.gameObject);
                    var swing = look.Swing;
                    for (int k = 0; swing != null && k < swing.results.Count; k++)
                    {
                        var r = swing.results[k];
                        Debug.Log(string.Format(culture, "LOOK {0} strength {1:0.00} pickaxe x{2:0.00}: swing {3}: {4}, upper hand {5:0}% of the way to the head, arms at {6:0}%, back at {7:0}%",
                            who, strength, weight, k + 1, r.struck ? string.Format(culture, "struck at {0:0.0} m/s", r.speed) : "did not strike", r.choked * 100, r.liftEffort * 100, r.backEffort * 100));
                    }
                    camera.transform.SetPositionAndRotation(seen.position, seen.rotation);
                    camera.nearClipPlane = seen.nearClipPlane;
                    if (rig != null) rig.enabled = true;
                    look.Toggle();
                    look.SetStrength(1);
                    look.SetWeight(1);
                    for (float until = Time.time + .3f; Time.time < until;) yield return null;
                }
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
