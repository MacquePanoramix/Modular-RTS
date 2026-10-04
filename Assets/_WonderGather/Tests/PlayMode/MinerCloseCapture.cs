using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // Not a test: the capture matrix of the model quality method (Docs/ArtDirection/ModelQualityMethod.md, section 4).
    // The rigged miners in the game and its own light:
    //   orbit      the whole figure from 8 directions at ground level, eye level and from above;
    //   distances  from the Strategy camera's height down to macro, at dusk and midday;
    //   zones      every junction zone of appendix A in macro, from at least six directions, below and above;
    //   walk       one steady stride in 8 phases, from the sides, front and back, with macro on boots, hands and bag;
    //   motion     starting, stopping and turning (follow-through);
    //   fresh      the silhouette (black on white) and clay (no paint), for fresh eyes.
    // Frames are PPM images named <miner>_<set>_<view>.
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MinerCloseCapture -captureOut <folder>
    //   [-captureMiner Small] [-captureSets orbit,distances,zones,walk,motion,fresh]
    [Explicit("A capture for review, not a test.")]
    public sealed class MinerCloseCapture
    {
        private const int Size = 720;

        private struct View
        {
            public readonly float azimuth, elevation;
            public View(float azimuth, float elevation) { this.azimuth = azimuth; this.elevation = elevation; }
        }

        // Six directions round a zone a little above it, then from below and from above. Azimuth 0 is in front of the
        // miner, 90 its right, 270 its left.
        private static readonly View[] Round =
        {
            new View(0, 8), new View(60, 10), new View(120, 12), new View(180, 12), new View(240, 12), new View(300, 10),
            new View(20, -35), new View(200, 55),
        };

        // A zone at one side of the body, seen from that side (the body is in the way from the other):
        // outside is 90 for the right, 270 for the left.
        private static View[] Beside(float outside)
        {
            float s = outside < 180 ? 1 : -1;
            return new[]
            {
                new View(outside, 5), new View(outside - s * 50, 8), new View(outside + s * 50, 8), new View(outside - s * 85, 8),
                new View(outside + s * 85, 10), new View(outside - s * 20, -38), new View(outside + s * 20, 55), new View(outside + s * 30, 25),
            };
        }

        private static View[] Around(float elevation, int count = 8)
        {
            var views = new View[count];
            for (int i = 0; i < count; i++) views[i] = new View(i * 360f / count, elevation);
            return views;
        }

        private static Transform Find(Component root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) if (t.name == name) return t;
            return null;
        }

        [UnityTest, Timeout(7200000)]
        public IEnumerator Capture()
        {
            string folder = CaptureTools.Argument("-captureOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "MinerClose");
            string only = CaptureTools.Argument("-captureMiner");
            var sets = new HashSet<string>((CaptureTools.Argument("-captureSets") ?? "orbit,distances,zones,walk,motion,fresh").Split(','));
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            Time.captureFramerate = 30;
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                var camera = Camera.main;
                var rig = camera.GetComponent<RtsCamera>();
                if (rig != null) rig.enabled = false;
                camera.nearClipPlane = .01f;
                Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
                // On the dirt path, along one of its long straight stretches: nothing between the camera and the boots.
                var from = ground.Path[4];
                var to = ground.Path[5];
                var spot = OnGround(new Vector3(from.x, 0, from.y));
                var far = OnGround(new Vector3(to.x, 0, to.y));
                var away = far - spot;
                away.y = 0;
                away.Normalize();
                // The meadow grass would stand between a low camera and the feet: it is put away for the close sets.
                var grass = UnityEngine.Object.FindAnyObjectByType<GrassField>();
                time.Hour = 19.2f;

                for (int index = 0; index < choice.Count; index++)
                {
                    string name = choice.NameOf(index);
                    if (only != null && !string.Equals(only, name, StringComparison.OrdinalIgnoreCase)) continue;
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    var body = unit.GetComponent<MinerBody>();
                    var biped = unit.GetComponent<ProceduralBiped>();
                    var agent = unit.GetComponent<NavMeshAgent>();
                    string tag = name.ToLowerInvariant();

                    IEnumerator Place()
                    {
                        unit.Motor.Stop();
                        agent.Warp(spot);
                        unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                        biped.ResetPose();
                        for (int k = 0; k < 40; k++) yield return null;
                    }
                    yield return Place();

                    float height = body.Rig.head.position.y - unit.transform.position.y;
                    var head = body.Rig.head;
                    var neck = body.Rig.neck;
                    var chest = body.Rig.chest;
                    var pelvis = body.Rig.pelvis;
                    var lantern = Find(unit, "Lantern");
                    var mug = Find(unit, "Mug");
                    var satchel = Find(unit, "Satchel");

                    void Shot(string set, string label, Vector3 target, float distance, float azimuth, float elevation, int w = Size, int h = Size)
                    {
                        var forward = unit.transform.forward;
                        forward.y = 0;
                        forward.Normalize();
                        Vector3 dir = Quaternion.AngleAxis(azimuth, Vector3.up) * forward;
                        dir = dir * Mathf.Cos(elevation * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(elevation * Mathf.Deg2Rad);
                        var eye = target + dir * distance;
                        float floor = ground.Height(eye.x, eye.z) + .03f;
                        if (eye.y < floor) eye.y = floor;
                        camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(target - eye));
                        CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{set}_{label}"), w, h);
                    }
                    Vector3 Mid() => unit.transform.position + Vector3.up * height * .5f;
                    Vector3 Boot(int i)
                    {
                        var foot = body.Rig.feet[i].position;
                        return new Vector3(foot.x, ground.Height(foot.x, foot.z) + .09f, foot.z) + unit.transform.forward * .03f;
                    }
                    // The bag: its own bone when it has one; otherwise the right hip, where Small wears it.
                    Vector3 Bag() => satchel.position - Vector3.up * .07f;
                    bool hasBag = satchel != null;
                    Vector3 Held() => lantern != null ? lantern.position - Vector3.up * .07f : mug.position - Vector3.up * .05f;
                    bool holds = lantern != null || mug != null;
                    if (grass != null) grass.enabled = false;

                    if (sets.Contains("orbit"))
                    {
                        float low = Mathf.Asin(Mathf.Clamp((unit.transform.position.y + .15f - Mid().y) / 2.3f, -1, 1)) * Mathf.Rad2Deg;
                        foreach (var v in Around(0)) Shot("orbit", $"low_{v.azimuth:000}", Mid(), 2.3f, v.azimuth, low);
                        foreach (var v in Around(5)) Shot("orbit", $"eye_{v.azimuth:000}", Mid(), 2.3f, v.azimuth, v.elevation);
                        foreach (var v in Around(45)) Shot("orbit", $"high_{v.azimuth:000}", Mid(), 2.3f, v.azimuth, v.elevation);
                    }

                    if (sets.Contains("distances"))
                    {
                        if (grass != null) grass.enabled = true;
                        foreach (var (hour, light) in new[] { (19.2f, "dusk"), (12.5f, "midday") })
                        {
                            time.Hour = hour;
                            yield return null;
                            Shot("distances", $"{light}_strategy", unit.transform.position, 23f, 35, 50, 1280, 720);
                            Shot("distances", $"{light}_strategy_near", unit.transform.position, 8f, 35, 35, 1280, 720);
                            Shot("distances", $"{light}_explore", Mid(), 4f, 35, 15, 1280, 720);
                            Shot("distances", $"{light}_close", Mid(), 1.6f, 35, 8);
                            Shot("distances", $"{light}_macro", head.position + Vector3.up * .08f, .6f, 35, 5);
                        }
                        if (grass != null) grass.enabled = false;
                        time.Hour = 19.2f;
                        yield return null;
                    }

                    if (sets.Contains("zones"))
                    {
                        void Zone(string label, Vector3 target, float distance, View[] views)
                        {
                            foreach (var v in views)
                                Shot("zone", $"{label}_{Mathf.Repeat(v.azimuth, 360):000}_{(v.elevation < 0 ? "m" : "")}{Mathf.Abs(v.elevation):00}", target, distance, v.azimuth, v.elevation);
                        }
                        Zone("head", head.position + Vector3.up * .09f, .6f, Round);
                        Zone("neck", neck.position + Vector3.up * .05f, .45f, Round);
                        Zone("back", chest.position + Vector3.up * .05f, .7f, new[] { new View(180, 12), new View(135, 25), new View(225, 25), new View(90, 10), new View(270, 10), new View(180, 60), new View(0, 45) });
                        Zone("chest", chest.position - Vector3.up * .03f, .6f, new[] { new View(0, 2), new View(35, 5), new View(-35, 5), new View(90, 0), new View(270, 0), new View(0, -30), new View(0, 45) });
                        Zone("hem", pelvis.position - Vector3.up * .2f, .85f, new[] { new View(0, 0), new View(90, 0), new View(180, 0), new View(270, 0), new View(45, -20), new View(225, -20), new View(135, 30) });
                        for (int i = 0; i < 2; i++)
                        {
                            // Index 0 is the miner's left: its outside is at 270.
                            float outside = i == 0 ? 270 : 90, s = i == 0 ? -1 : 1;
                            var hand = body.Rig.hands[i];
                            Zone(i == 0 ? "handL" : "handR", hand.position - Vector3.up * .06f, .36f, Beside(outside));
                            var boot = new List<View>(Around(0));
                            boot.Add(new View(outside, 55));
                            boot.Add(new View(outside - s * 60, 35));
                            Zone(i == 0 ? "bootL" : "bootR", Boot(i), .42f, boot.ToArray());
                        }
                        if (holds) Zone(lantern != null ? "lantern" : "mug", Held(), .4f, Beside(270));
                        if (hasBag) Zone("bag", Bag(), .42f, Beside(60));
                    }

                    if (sets.Contains("walk"))
                    {
                        unit.Motor.TryMove(far);
                        float began = Time.time;
                        while (Time.time - began < 2.2f) yield return null;
                        float period = 1f / Mathf.Max(biped.Cadence, .5f);
                        float start = Time.time;
                        for (int k = 0; k < 8; k++)
                        {
                            while (Time.time - start < k * period / 8f) yield return null;
                            string p = $"p{k}";
                            Shot("walk", $"{p}_right", Mid(), 2.1f, 90, 5);
                            Shot("walk", $"{p}_left", Mid(), 2.1f, 270, 5);
                            Shot("walk", $"{p}_front", Mid(), 2.3f, 10, 5);
                            Shot("walk", $"{p}_back", Mid(), 2.3f, 190, 12);
                            Shot("walk", $"{p}_bootL", Boot(0), .5f, 250, 0);
                            Shot("walk", $"{p}_bootR", Boot(1), .5f, 110, 0);
                            if (holds) Shot("walk", $"{p}_held", Held(), .45f, 250, 8);
                            if (holds) Shot("walk", $"{p}_heldfront", Held(), .45f, 330, 5);
                            if (hasBag) Shot("walk", $"{p}_bag", Bag(), .5f, 70, 8);
                            Shot("walk", $"{p}_hem", pelvis.position - Vector3.up * .2f, 1.0f, 20, -8);
                        }
                        unit.Motor.Stop();
                        yield return Place();
                    }

                    if (sets.Contains("motion"))
                    {
                        float[] times = { 0, .13f, .27f, .4f, .6f, .9f };
                        IEnumerator Watch(string label)
                        {
                            float t0 = Time.time;
                            foreach (var t in times)
                            {
                                while (Time.time - t0 < t) yield return null;
                                Shot("motion", $"{label}_{t * 100:000}_side", Mid(), 2.2f, 90, 5);
                                Shot("motion", $"{label}_{t * 100:000}_three", Mid(), 2.2f, 300, 8);
                            }
                        }
                        unit.Motor.TryMove(far);
                        yield return Watch("start");
                        float began = Time.time;
                        while (Time.time - began < 2f) yield return null;
                        var turnTo = OnGround(unit.transform.position + Quaternion.AngleAxis(-100, Vector3.up) * away * 5f);
                        unit.Motor.TryMove(turnTo);
                        yield return Watch("turn");
                        began = Time.time;
                        while (Time.time - began < 1.2f) yield return null;
                        unit.Motor.Stop();
                        yield return Watch("stop");
                        yield return Place();
                    }

                    if (sets.Contains("fresh"))
                    {
                        time.Hour = 12.5f;
                        yield return null;
                        var renderers = unit.GetComponentsInChildren<Renderer>();
                        var materials = new Material[renderers.Length][];
                        var layers = new int[renderers.Length];
                        for (int r = 0; r < renderers.Length; r++) { materials[r] = renderers[r].sharedMaterials; layers[r] = renderers[r].gameObject.layer; }
                        var mask = camera.cullingMask;
                        var clear = camera.clearFlags;
                        var background = camera.backgroundColor;
                        void Dress(Material m)
                        {
                            for (int r = 0; r < renderers.Length; r++)
                            {
                                var list = new Material[materials[r].Length];
                                for (int k = 0; k < list.Length; k++) list[k] = m;
                                renderers[r].sharedMaterials = list;
                                renderers[r].gameObject.layer = 31;
                            }
                            camera.cullingMask = 1 << 31;
                            camera.clearFlags = CameraClearFlags.SolidColor;
                        }
                        var black = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                        black.SetColor("_BaseColor", Color.black);
                        camera.backgroundColor = Color.white;
                        Dress(black);
                        foreach (var v in Around(5)) Shot("fresh", $"silhouette_{v.azimuth:000}", Mid(), 2.3f, v.azimuth, v.elevation);
                        var clay = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        clay.SetColor("_BaseColor", new Color(.72f, .7f, .68f));
                        clay.SetFloat("_Smoothness", .15f);
                        camera.backgroundColor = new Color(.42f, .45f, .5f);
                        Dress(clay);
                        foreach (var v in Around(10)) Shot("fresh", $"clay_{v.azimuth:000}", Mid(), 2.1f, v.azimuth, v.elevation);
                        Shot("fresh", "clay_bootL", Boot(0), .45f, 250, 10);
                        Shot("fresh", "clay_bootR", Boot(1), .45f, 110, 10);
                        Shot("fresh", "clay_neck", neck.position + Vector3.up * .05f, .45f, 160, 15);
                        if (holds) Shot("fresh", "clay_held", Held(), .4f, 250, 8);
                        if (hasBag) Shot("fresh", "clay_bag", Bag(), .42f, 60, 8);
                        for (int r = 0; r < renderers.Length; r++) { renderers[r].sharedMaterials = materials[r]; renderers[r].gameObject.layer = layers[r]; }
                        camera.cullingMask = mask;
                        camera.clearFlags = clear;
                        camera.backgroundColor = background;
                        UnityEngine.Object.Destroy(black);
                        UnityEngine.Object.Destroy(clay);
                        time.Hour = 19.2f;
                        yield return null;
                    }
                }
            }
            finally
            {
                Time.captureFramerate = 0;
                var field = UnityEngine.Object.FindAnyObjectByType<GrassField>(FindObjectsInactive.Include);
                if (field != null) field.enabled = true;
            }
            Debug.Log("MINER_CLOSE_CAPTURE_OK " + folder);
        }
    }
}
