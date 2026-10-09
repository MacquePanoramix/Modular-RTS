using System;
using System.Collections;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The bench of S3's step 10: a miner standing on the path is let go, with a shove, and falls. What its body does
    // is written to the log (FALL ...), with pictures from the side.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.PhysicalFallBench -fallOut <folder>
    //   [-fallMiner Round] [-fallShoves 300] [-fallWays 0,90,180] [-fallFor 4] [-fallView 100,8,3.2] [-fallSize 360]
    public sealed class PhysicalFallBench
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

        // Ways of getting up, from a file. A line "way <name> <front least> <front most> gives|turns", then a line for
        // each pose:  name | over atLeast within | trunk head | shoulder across elbow | shoulder across elbow |
        // hip across knee | hip across knee | front>0.6 still>0.2 ...   (the leading side first.)
        internal static PhysicalFall.Way[] Read(string file)
        {
            var culture = CultureInfo.InvariantCulture;
            var ways = new System.Collections.Generic.List<PhysicalFall.Way>();
            var stages = new System.Collections.Generic.List<PhysicalFall.Stage>();
            PhysicalFall.Way way = null;
            void Close() { if (way != null) { way.stages = stages.ToArray(); ways.Add(way); stages.Clear(); } }
            foreach (string raw in File.ReadAllLines(file))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                if (line.StartsWith("way "))
                {
                    Close();
                    var w = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    way = new PhysicalFall.Way { name = w[1], frontLeast = float.Parse(w[2], culture), frontMost = float.Parse(w[3], culture), gives = w[4] == "gives" };
                    continue;
                }
                var fields = line.Split('|');
                float[] Of(string text)
                {
                    var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var values = new float[words.Length];
                    for (int i = 0; i < words.Length; i++) values[i] = float.Parse(words[i], culture);
                    return values;
                }
                var stage = new PhysicalFall.Stage { name = fields[0].Trim() };
                var times = Of(fields[1]);
                stage.over = times[0]; stage.atLeast = times[1]; stage.within = times[2];
                // (The trunk's bow and the head's, and after them the trunk's twist if it is given.)
                var trunk = Of(fields[2]);
                stage.pose[0] = trunk[0]; stage.pose[1] = trunk[1];
                if (trunk.Length > 2) stage.pose[14] = trunk[2];
                int k = 2;
                for (int f = 3; f <= 6; f++) foreach (float value in Of(fields[f])) stage.pose[k++] = value;
                Assert.That(k, Is.EqualTo(14), "A pose of " + stage.name + " has not its turns.");
                var asked = new System.Collections.Generic.List<PhysicalFall.Asked>();
                if (fields.Length > 7)
                    foreach (string word in fields[7].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (word.StartsWith("keep=")) { stage.keeps = float.Parse(word.Substring(5), culture); continue; }
                        int sign = word.IndexOfAny(new[] { '>', '<' });
                        var what = (PhysicalFall.Asks)Enum.Parse(typeof(PhysicalFall.Asks), word.Substring(0, sign), true);
                        float value = float.Parse(word.Substring(sign + 1), culture);
                        asked.Add(new PhysicalFall.Asked { what = what, least = word[sign] == '>' ? value : float.NegativeInfinity, most = word[sign] == '<' ? value : float.PositiveInfinity });
                    }
                stage.through = asked.ToArray();
                stages.Add(stage);
            }
            Close();
            return ways.ToArray();
        }

        // A let-go body laid down: every part turned as one about where it stood (on its front, its back, its left
        // side or its right), and set just clear of the ground. (To try one part of getting up at a time.)
        internal static void Lay(PhysicalFall fall, Transform unit, OrdinaryGround ground, string how)
        {
            Vector3 pivot = unit.position;
            Quaternion turn = how == "front" ? Quaternion.AngleAxis(90, unit.right) : how == "back" ? Quaternion.AngleAxis(-90, unit.right)
                : how == "left" ? Quaternion.AngleAxis(90, unit.forward) : Quaternion.AngleAxis(-90, unit.forward);
            for (int i = 0; i < PhysicalFall.Count; i++)
            {
                var part = fall.Part(i);
                part.transform.SetPositionAndRotation(pivot + turn * (part.transform.position - pivot), turn * part.transform.rotation);
                part.linearVelocity = Vector3.zero; part.angularVelocity = Vector3.zero;
            }
            Physics.SyncTransforms();
            float lowest = float.MaxValue;
            for (int i = 0; i < PhysicalFall.Count; i++)
                foreach (var solid in fall.Part(i).GetComponentsInChildren<Collider>())
                    lowest = Mathf.Min(lowest, solid.bounds.min.y - ground.Height(solid.bounds.center.x, solid.bounds.center.z));
            for (int i = 0; i < PhysicalFall.Count; i++) fall.Part(i).transform.position += Vector3.up * (.03f - lowest);
            Physics.SyncTransforms();
        }

        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator Bench()
        {
            string folder = CaptureTools.Argument("-fallOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Fall");
            string who = CaptureTools.Argument("-fallMiner");
            float[] shoves = Numbers("-fallShoves", 300), ways = Numbers("-fallWays", 0, 90, 180), view = Numbers("-fallView", 100, 8, 3.2f);
            float lasts = Numbers("-fallFor", 12)[0];
            int size = (int)Numbers("-fallSize", 360)[0];
            // -fallScripts a.txt,b.txt: ways of getting up to try, one file after another. -fallEvery: a picture every so
            // many frames. -fallFollow: how far the camera goes with the body (1: all the way).
            string[] scripts = (CaptureTools.Argument("-fallScripts") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int every = (int)Numbers("-fallEvery", 2)[0];
            // -fallLays front,back,left,right: not shoved, but laid down so (to try one part of getting up at a time).
            string[] lays = (CaptureTools.Argument("-fallLays") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (lays.Length > 0) { ways = new float[lays.Length]; for (int i = 0; i < lays.Length; i++) ways[i] = i; shoves = new[] { 0f }; }
            float follows = Numbers("-fallFollow", .5f)[0];
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            Time.captureFramerate = 50;
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                var camera = Camera.main;
                var rig = camera.GetComponent<RtsCamera>();
                if (rig != null) rig.enabled = false;
                camera.nearClipPlane = .01f;
                time.Hour = 13;
                Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
                var a = ground.Path[4];
                var b = ground.Path[5];
                var spot = OnGround(new Vector3(a.x, 0, a.y));
                var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
                away.y = 0;
                away.Normalize();
                for (int index = 0; index < choice.Count; index++)
                {
                    string name = choice.NameOf(index);
                    if (!string.IsNullOrEmpty(who) && !string.Equals(who, name, StringComparison.OrdinalIgnoreCase)) continue;
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    var miner = unit.GetComponent<MinerBody>();
                    var biped = unit.GetComponent<ProceduralBiped>();
                    var fall = unit.GetComponent<PhysicalFall>();
                    if (fall == null) fall = unit.gameObject.AddComponent<PhysicalFall>();
                    // -fallLook strength,weight: in the look at the work, as weak as that and with a pickaxe that many
                    // times its own weight, for a minute: does it fall by itself?
                    float[] inLook = Numbers("-fallLook");
                    if (inLook.Length == 2)
                    {
                        var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
                        unit.Motor.Stop();
                        unit.GetComponent<NavMeshAgent>().Warp(spot);
                        unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                        biped.ResetPose();
                        for (float until = Time.time + .8f; Time.time < until;) yield return null;
                        float height = miner.Rig.head.position.y - unit.transform.position.y;
                        look.SetStrength(inLook[0]); look.SetWeight(inLook[1]);
                        look.Toggle();
                        int pictures = 0, frames = 0;
                        var watching = new GameObject("After everything").AddComponent<AfterEverything>();
                        watching.Then = () =>
                        {
                            if (frames++ % 2 != 0) return;
                            Vector3 target = unit.transform.position + Vector3.up * height * .4f;
                            Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * away;
                            dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                            camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                            CaptureTools.Render(camera, Path.Combine(folder, $"{name.ToLowerInvariant()}_look_{pictures:000}"), size, size);
                            pictures++;
                        };
                        float from = Time.time, fellAt = -1, upAt = -1, said = Time.time;
                        bool again = false;
                        int gotUp = 0;
                        float watchUntil = -1, watched = 0;
                        while (Time.time - from < lasts)
                        {
                            var down = look.Fall;
                            var balanced = unit.GetComponent<PhysicalBalance>();
                            if (down != null && fellAt < 0 && down.Now != PhysicalFall.State.Up) { fellAt = Time.time - from; Debug.Log(string.Format(culture, "FALL   {0} in the look fell at {1:0.0} s: {2}", name, fellAt, balanced != null ? balanced.Fell : "?")); }
                            if (fellAt >= 0 && upAt < 0 && down.Now == PhysicalFall.State.Up && biped.SinkNow < .02f) upAt = Time.time - from;
                            if (fellAt >= 0 && down.Falls > 1 && !again) { again = true; Debug.Log(string.Format(culture, "FALL   {0} in the look fell AGAIN at {1:0.0} s", name, Time.time - from)); }
                            // Just up again: what its knees are asked for as it tries to stand.
                            if (down != null && down.GotUp > gotUp) { gotUp = down.GotUp; watchUntil = Time.time + 2.2f; watched = Time.time; }
                            if (Time.time < watchUntil && Time.time - watched > .3f && balanced != null)
                            {
                                watched = Time.time;
                                Debug.Log(string.Format(culture, "FALL   {0} up again {1:0.0}s: state {2}, hips {3:0.000} m low (wanted {4}), knees out {5:0.000} and {6:0.000} (straight {7:0.000}, thigh {8:0.000}), legs giving {9:0.00}, bears {10:0} N of {11:0} N, knees have {12:0} N m, given way {13:0.000}",
                                    name, Time.time - from, down.Now, biped.SinkNow, "0", biped.KneeOut(0), biped.KneeOut(1), biped.KneeOutStraight, biped.BodyProportions.legSegment, balanced.LegEffort, balanced.Bears,
                                    unit.GetComponent<PhysicalBody>().Mass * 9.81f, unit.GetComponent<PhysicalBody>().KneeNow, balanced.GaveWay));
                            }
                            if (upAt >= 0 && Time.time - from > upAt + 1.5f) break;
                            if (Time.time - said > 4)
                            {
                                said = Time.time;
                                Debug.Log(string.Format(culture, "FALL   {0} in the look {1:0}s: showing {2}, swinging {3}, carrying {4}, swings {5}, steps {6}, short steps in a row {7}, legs giving {8:0.00} of what they have, knees given way {9:0.000} m, state {10}, legs {11:0}% spent, last rose from {12:0.00} m",
                                    name, Time.time - from, look.Showing, look.Swinging, look.Carrying, look.Swing != null ? look.Swing.results.Count : 0, balanced != null ? balanced.Steps : 0,
                                    balanced != null ? balanced.MostShort : 0, balanced != null ? balanced.LegEffort : 0, balanced != null ? balanced.GaveWay : 0, down != null ? down.Now.ToString() : "-",
                                    unit.GetComponent<PhysicalBody>().Spent(PhysicalBody.Muscles.Legs) * 100, down != null ? down.RoseFrom : 0));
                            }
                            yield return null;
                        }
                        watching.Then = null;
                        UnityEngine.Object.Destroy(watching.gameObject);
                        Debug.Log(string.Format(culture, "FALL {0} in the look, at {1:0.00} of its strength with a pickaxe {2:0.0} times its weight: {3}; the look {4}, its pickaxe {5} ({6} pictures)",
                            name, inLook[0], inLook[1], fellAt < 0 ? "it did not fall in " + lasts.ToString("0", culture) + " s" : string.Format(culture, "it fell after {0:0.0} s and was up again at {1:0.0} s", fellAt, upAt),
                            look.Showing ? "still shows" : "has ended", look.Lying != null ? "lies in the world" : "is gone", pictures)
                            + string.Format(culture, "; it fell {0} time(s), lay down again {1} time(s) before its legs could raise it, and rose from a crouch {2:0.00} m deep", look.Fall.Falls, look.Fall.LayDownAgain, look.Fall.RoseFrom));
                        if (look.Showing) look.End();
                        look.SetStrength(1); look.SetWeight(1);
                        for (float until = Time.time + .5f; Time.time < until;) yield return null;
                        continue;
                    }
                    var body = unit.GetComponent<PhysicalBody>();
                    Debug.Log(string.Format(culture, "FALL   {0}: {1:0.0} kg; a knee gives {2:0} N m, a hip {3:0}, its back {4:0}, a shoulder {5:0}, an elbow {6:0}; its hips stand {7:0.00} m up, a thigh is {8:0.00} m, an upper arm {9:0.00} m and a forearm {10:0.00} m",
                        name, body.Mass, body.KneeNow, body.KneeNow * 1.5f, body.BackNow, body.ShoulderOf(0), body.ElbowOf(0), biped.StandingHipHeight, biped.BodyProportions.legSegment,
                        biped.BodyProportions.upperArm, biped.BodyProportions.forearm));
                    foreach (string script in scripts.Length > 0 ? scripts : new[] { "" })
                    foreach (float shove in shoves)
                        foreach (float way in ways)
                        {
                            if (script.Length > 0) fall.Ways = Read(script);
                            unit.Motor.Stop();
                            unit.GetComponent<NavMeshAgent>().Warp(spot);
                            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                            biped.ResetPose();
                            for (float until = Time.time + .8f; Time.time < until;) yield return null;
                            float tall = miner.Rig.head.position.y - unit.transform.position.y;
                            string lay = lays.Length > 0 ? lays[(int)way] : "";
                            string tag = (script.Length > 0 ? Path.GetFileNameWithoutExtension(script) + "_" : "") + (lay.Length > 0 ? $"{name.ToLowerInvariant()}_{lay}" : $"{name.ToLowerInvariant()}_{shove:0}_{way:0}");
                            int shot = 0, frame = 0;
                            var after = new GameObject("After everything").AddComponent<AfterEverything>();
                            Vector3 middle = unit.transform.position;
                            // Once it lies, the camera looks from the side of how it lies (from its hips to its head).
                            Vector3 lies = away;
                            bool lain = false;
                            after.Then = () =>
                            {
                                if (follows >= 1 && !lain && fall.Now == PhysicalFall.State.Lying)
                                {
                                    lain = true;
                                    Vector3 spine = Vector3.ProjectOnPlane(fall.HeadAt - fall.HipsAt, Vector3.up);
                                    if (spine.sqrMagnitude > 1e-4f) lies = spine.normalized;
                                }
                                if (frame++ % every != 0) return;
                                Vector3 target = Vector3.Lerp(middle, unit.transform.position, follows) + Vector3.up * tall * .4f;
                                Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * lies;
                                dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                                camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                                CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{shot:000}"), size, size);
                                shot++;
                            };
                            for (float until = Time.time + .3f; Time.time < until;) yield return null;
                            // Let go, and shoved at the chest for a moment: forwards (0), to its right (90), backwards (180).
                            Vector3 push = Quaternion.AngleAxis(way, Vector3.up) * away * shove;
                            fall.LetGo();
                            Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Falling), name + " was not let go.");
                            if (lay.Length > 0) Lay(fall, unit.transform, ground, lay);
                            float began = Time.time, lay2 = -1, fastest = 0, furthest = 0, lowestHead = float.MaxValue, headStruck = 0, headBefore = 0;
                            bool sound = true, said = false;
                            float gathers = -1, takenOver = -1, stood = -1;
                            Vector3 layAt = Vector3.zero;
                            float told = -1;
                            string was = "";
                            while (Time.time - began < lasts)
                            {
                                // What it does as it gets up: at each new pose, and five times a second.
                                if (fall.Part(PhysicalFall.Hips) != null && (fall.Now == PhysicalFall.State.Gathering || fall.Now == PhysicalFall.State.Lying))
                                {
                                    string doing = fall.Now + " " + fall.WayNow + "/" + fall.StageNow;
                                    if (doing != was || Time.time - told >= .2f)
                                    {
                                        was = doing; told = Time.time;
                                        // (Its joints as they are: a hip bent forward and a knee bent are more than nothing.)
                                        var pelvis = fall.Part(PhysicalFall.Hips).transform;
                                        float Hip(int i) => -Vector3.SignedAngle(-pelvis.up, fall.Part(PhysicalFall.Thigh + i).transform.up, pelvis.right);
                                        float Knee(int i) => -Vector3.SignedAngle(fall.Part(PhysicalFall.Thigh + i).transform.up, fall.Part(PhysicalFall.Shin + i).transform.up, fall.Part(PhysicalFall.Thigh + i).transform.right);
                                        float waist = Vector3.SignedAngle(pelvis.up, fall.Part(PhysicalFall.Trunk).transform.up, pelvis.right);
                                        Debug.Log(string.Format(culture, "GETUP {0} {1:0.00} s {2}: front {3:0.00}, hips {4:0.00}, off {5:0.00}, ahead {6:0.00}, steep {7:0.00}, soles {8:0.00}, upright {9:0.00}, still {10:0.00}, hip {12:0} {13:0}, knee {14:0} {15:0}, waist {16:0}, picture {11}",
                                            tag, Time.time - began, doing, fall.Measure(PhysicalFall.Asks.Front), fall.Measure(PhysicalFall.Asks.Hips), fall.Measure(PhysicalFall.Asks.Off),
                                            fall.Measure(PhysicalFall.Asks.Ahead), fall.Measure(PhysicalFall.Asks.Steep), fall.Measure(PhysicalFall.Asks.Soles), fall.Measure(PhysicalFall.Asks.Upright),
                                            fall.Measure(PhysicalFall.Asks.Still), shot, Hip(0), Hip(1), Knee(0), Knee(1), waist));
                                    }
                                }
                                // It gets up by itself: when it gathers itself, when the posed body takes over, when it stands.
                                if (gathers < 0 && fall.Now == PhysicalFall.State.Gathering) { gathers = Time.time - began; layAt = fall.HipsAt; }
                                if (takenOver < 0 && fall.Now == PhysicalFall.State.Rising) takenOver = Time.time - began;
                                if (takenOver >= 0 && stood < 0 && fall.Now == PhysicalFall.State.Up && biped.SinkNow < .02f && Mathf.Abs(biped.BowNow) < 3) stood = Time.time - began;
                                if (stood >= 0 && Time.time - began > stood + .8f) break;
                                if (fall.Part(PhysicalFall.Hips) == null) { yield return new WaitForFixedUpdate(); continue; }
                                if (Time.time - began < .3f) fall.Push(push, fall.Part(PhysicalFall.Trunk).worldCenterOfMass);
                                for (int i = 0; i < PhysicalFall.Count; i++)
                                {
                                    var part = fall.Part(i);
                                    Vector3 at = part.position;
                                    sound &= float.IsFinite(at.x) && float.IsFinite(at.y) && float.IsFinite(at.z);
                                    fastest = Mathf.Max(fastest, part.linearVelocity.magnitude);
                                    furthest = Mathf.Max(furthest, Vector3.Distance(at, fall.HipsAt));
                                }
                                lowestHead = Mathf.Min(lowestHead, fall.HeadAt.y - ground.Height(fall.HeadAt.x, fall.HeadAt.z));
                                // How hard its head came down: the downward speed it lost in one step.
                                float down = -fall.Part(PhysicalFall.Head).linearVelocity.y;
                                headStruck = Mathf.Max(headStruck, headBefore - down);
                                headBefore = down;
                                if (lay2 < 0 && fall.Now == PhysicalFall.State.Lying) lay2 = Time.time - began;
                                // How its joints are turned, half a second into the fall: a knee and an elbow (bent is
                                // negative), a hip and a shoulder (the limb forward of the body is positive).
                                if (!said && Time.time - began >= .5f)
                                {
                                    said = true;
                                    var hipsPart = fall.Part(PhysicalFall.Hips).transform;
                                    var trunkPart = fall.Part(PhysicalFall.Trunk).transform;
                                    var thighPart = fall.Part(PhysicalFall.Thigh).transform;
                                    var shinPart = fall.Part(PhysicalFall.Shin).transform;
                                    var upperPart = fall.Part(PhysicalFall.UpperArm).transform;
                                    var forePart = fall.Part(PhysicalFall.Forearm).transform;
                                    Debug.Log(string.Format(culture, "FALL   {0} at 0.5 s: knee {1:0}, hip {2:0}, elbow {3:0}, shoulder {4:0}, trunk bowed {5:0}, tone {6:0.00}",
                                        tag, Vector3.SignedAngle(thighPart.up, shinPart.up, thighPart.right), Vector3.SignedAngle(-hipsPart.up, thighPart.up, hipsPart.right) * -1,
                                        Vector3.SignedAngle(upperPart.up, forePart.up, upperPart.right), Vector3.SignedAngle(-trunkPart.up, upperPart.up, trunkPart.right) * -1,
                                        Vector3.SignedAngle(hipsPart.up, trunkPart.up, hipsPart.right), fall.Tone));
                                }
                                yield return new WaitForFixedUpdate();
                            }
            Vector3 hipsAt = layAt, headAt = fall.HeadAt;
                            float under = 0;
                            Debug.Log(string.Format(culture, "FALL   {0}: it gathered itself {1:0.00} s after the shove, the posed body took over at {2:0.00} s, it stood upright at {3:0.00} s; it stands {4:0.00} m from where it lay, its head {5:0.00} m up (it is {6:0.00} m tall), bowed {7:0.0} degrees, its hips {8:0.000} m low; state {9}",
                                tag, gathers, takenOver, stood, Vector3.ProjectOnPlane(unit.transform.position - layAt, Vector3.up).magnitude,
                                miner.Rig.head.position.y - unit.transform.position.y, tall, biped.BowNow, biped.SinkNow, fall.Now));
                            Debug.Log(string.Format(culture, "FALL {0} shoved {1:0} N for 0.3 s, {2:0} degrees from ahead: lying after {3:0.00} s ({4}); its hips {5:0.00} m over the ground and {6:0.00} m from where it stood, its head {7:0.00} m over the ground (lowest {8:0.00}); the fastest part {9:0.0} m/s; the part furthest from its hips {10:0.00} m (it is {11:0.00} m tall); deepest under the ground {12:0.000} m; sound {13} ({14} pictures); its head struck the ground at {15:0.0} m/s at most",
                                name, shove, way, lay2, fall.Now, hipsAt.y - ground.Height(hipsAt.x, hipsAt.z), Vector3.ProjectOnPlane(hipsAt - spot, Vector3.up).magnitude,
                                headAt.y - ground.Height(headAt.x, headAt.z), lowestHead, fastest, furthest, tall, under, sound, shot, headStruck));
                            after.Then = null;
                            UnityEngine.Object.Destroy(after.gameObject);
                            fall.TakeBack();
                            for (float until = Time.time + .4f; Time.time < until;) yield return null;
                        }
                }
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
