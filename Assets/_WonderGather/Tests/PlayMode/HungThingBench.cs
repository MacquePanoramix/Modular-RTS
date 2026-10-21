using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // What hangs on a hook against the miner's clothes, read from the skin (ClothesDepth), as each miner is pushed over,
    // lies and gets up its fall's own way (Bowed), goes down for a pickaxe and mines (AtWork), and stands and walks
    // (Walks). Written for the satchel that went through the coat when Small was bowed low (October 15). Each line:
    // BAG <miner> <way> <phase> <thing> <time> ... skin <deepest> mm (<points read over 3 mm>/<all>) and in which part
    // (hips, skirt, leg); BAGPATH and BAGMOVED for the walks. -hungAsBefore 1: what hangs is stopped as it was before.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.HungThingBench.Bowed
    //   [-bagMiner Small,Long,Round] [-bagWays front,back,left,right] [-bagOut <folder> -bagShots 1 -bagEvery 5
    //   -bagSize 320 -bagView 1.0 -bagFrom <s> -bagTo <s> -bagThing Lantern -bagViews 0,45,-110,below]
    // ...HungThingBench.AtWork [-bagMiner ...] [-bagBoulders 1,3,4] [-bagFor 25] [-bagOut <folder>] [-bagThing Satchel]
    // ...HungThingBench.Walks [-bagMiner ...] [-bagOwn 1]
    public sealed class HungThingBench
    {
        private static readonly CultureInfo Plain = CultureInfo.InvariantCulture;

        // A picture of what hangs first on its hook (or the one named), from its side of the body as the body is turned
        // now, a little above; or so many degrees round from there about the upright (towards the head, positive).
        public static void Shoot(Camera camera, MinerBody miner, string path, float view, int size, string which = null, float around = 0)
        {
            var pelvis = miner.Rig.pelvis;
            int k0 = -1;
            for (int k = 0; k < miner.Things.Count && k0 < 0; k++) if (miner.Things[k].hand == null && miner.Things[k].bone != null && (which == null || Array.IndexOf(which.Split(','), miner.Things[k].bone.name) >= 0)) k0 = k;
            if (k0 < 0) return;
            var thing = miner.Things[k0];
            Vector3 bodyRight = (miner.Rig.thighs[1].position - miner.Rig.thighs[0].position).normalized;
            int s = Vector3.Dot(thing.bone.position - pelvis.position, bodyRight) < 0 ? -1 : 1;
            // (-bagAim hips: aimed at the hips, not half way to the thing, so that two runs are framed alike.)
            Vector3 target = Vector3.Lerp(pelvis.position, miner.HangingWeight(k0), CaptureTools.Argument("-bagAim") == "hips" ? 0 : .5f);
            Vector3 dir = Vector3.ProjectOnPlane(bodyRight * s, Vector3.up);
            // (-bagSide root: from the miner's root's own side, which does not turn with a fallen body: the views stay
            // put as the body falls and rolls, where the hips' own right swings round, and stands near upright lying.)
            if (CaptureTools.Argument("-bagSide") == "root") dir = miner.transform.right * s;
            dir = (dir.sqrMagnitude > .04f ? dir.normalized : miner.transform.right * s) + Vector3.up * .35f;
            dir = Quaternion.AngleAxis(around * -s, Vector3.up) * dir.normalized;
            camera.transform.SetPositionAndRotation(target + dir * view, Quaternion.LookRotation(-dir));
            CaptureTools.Render(camera, path, size, size);
        }

        // A picture of the same thing from under the ground, looking up at it with the ground not drawn: what lies on
        // the ground, or is pressed into it, is seen from beneath.
        public static void Below(Camera camera, MinerBody miner, string path, float view, int size, string which = null)
        {
            int k0 = -1;
            for (int k = 0; k < miner.Things.Count && k0 < 0; k++) if (miner.Things[k].hand == null && miner.Things[k].bone != null && (which == null || Array.IndexOf(which.Split(','), miner.Things[k].bone.name) >= 0)) k0 = k;
            if (k0 < 0) return;
            Vector3 target = Vector3.Lerp(miner.Rig.pelvis.position, miner.HangingWeight(k0), .5f);
            Vector3 ahead = Vector3.ProjectOnPlane(miner.transform.forward, Vector3.up).normalized;
            Vector3 dir = (Vector3.down + ahead * .15f).normalized;
            var hidden = new List<Renderer>();
            foreach (var ground in UnityEngine.Object.FindObjectsByType<OrdinaryGround>(FindObjectsSortMode.None))
                foreach (var r in ground.GetComponentsInChildren<Renderer>())
                    if (r.enabled) { r.enabled = false; hidden.Add(r); }
            camera.transform.SetPositionAndRotation(target + dir * view, Quaternion.LookRotation(-dir, ahead));
            CaptureTools.Render(camera, path, size, size);
            foreach (var r in hidden) r.enabled = true;
        }

        // The ground as it is drawn (its collider has the same triangles in the meadow), not as its formula has it: they
        // differ by a few centimetres between the triangles' corners, most at the path's edge.
        public static float Drawn(OrdinaryGround ground, float x, float z)
            => Physics.Raycast(new Vector3(x, 60, z), Vector3.down, out var hit, 200, 1 << 6, QueryTriggerInteraction.Ignore) ? hit.point.y : ground.Height(x, z);

        // How far a thing's weight is from the line of the thigh on its side.
        public static float LegAt(MinerBody miner, int k)
        {
            var pelvis = miner.Rig.pelvis;
            int leg = Vector3.Dot(miner.HangingWeight(k) - pelvis.position, miner.Rig.thighs[1].position - miner.Rig.thighs[0].position) < 0 ? 0 : 1;
            Vector3 hip = miner.Rig.thighs[leg].position, ab = miner.Rig.shins[leg].position - hip;
            return Vector3.Distance(miner.HangingWeight(k), hip + ab * Mathf.Clamp01(Vector3.Dot(miner.HangingWeight(k) - hip, ab) / ab.sqrMagnitude));
        }

        // Every second frame, after the body is posed: each thing on its hook against the clothes, as BAG lines. Its frame
        // is the pelvis' own (right from hip to hip, up to the spine), so it can be told lying as standing.
        public static AfterEverything Watch(MinerBody miner, string name, string way, Func<string> phase, Func<float> clock, Func<float, float, float> ground,
            Dictionary<string, float> worst = null, Action<int> then = null)
        {
            var pelvis = miner.Rig.pelvis;
            Vector3 rightIn = (pelvis.InverseTransformPoint(miner.Rig.thighs[1].position) - pelvis.InverseTransformPoint(miner.Rig.thighs[0].position)).normalized;
            Vector3 upIn = Vector3.ProjectOnPlane(pelvis.InverseTransformPoint(miner.Rig.spine.position), rightIn).normalized;
            Vector3 forwardIn = Vector3.Cross(rightIn, upIn);
            // (Pelvis space has the bone's scale: directions are taken through its rotation alone.)
            rightIn = Quaternion.Inverse(pelvis.rotation) * pelvis.TransformDirection(rightIn).normalized;
            upIn = Quaternion.Inverse(pelvis.rotation) * pelvis.TransformDirection(upIn).normalized;
            forwardIn = Quaternion.Inverse(pelvis.rotation) * pelvis.TransformDirection(forwardIn).normalized;
            var depths = new List<(int k, ClothesDepth d)>();
            for (int k = 0; k < miner.Things.Count; k++)
                if (miner.Things[k].hand == null && miner.Things[k].bone != null)
                {
                    depths.Add((k, new ClothesDepth(miner, k)));
                    var t = miner.Things[k];
                    Debug.Log($"BAGTHING {name} {t.bone.name}: parent {t.bone.parent?.name}, pusher {t.pusher?.name} share {t.pushShare}, riders {(t.riders != null ? string.Join(",", Array.ConvertAll(t.riders, r => r != null ? r.name : "-")) : "-")} shares {(t.rideShares != null ? string.Join(",", t.rideShares) : "-")}, length {t.length}, limit {t.limit}, stop {t.stopDistance}, reach {miner.HangingReach(k):F3}, line room {miner.HangingLineRoom(k):F3}, skin points {(t.skin != null ? t.skin.Length : 0)}");
                }
            int frame = 0;
            var after = new GameObject("After everything").AddComponent<AfterEverything>();
            after.Then = () =>
            {
                if (frame++ % 2 != 0) return;
                Quaternion p = pelvis.rotation;
                Vector3 up = p * upIn, right = p * rightIn, fwd = p * forwardIn;
                float tilt = Vector3.Angle(up, Vector3.up);
                string now = phase();
                foreach (var (k, d) in depths)
                {
                    var thing = miner.Things[k];
                    int side = Vector3.Dot(thing.bone.position - pelvis.position, right) < 0 ? 0 : 1;
                    Vector3 thigh = miner.Rig.shins[side].position - miner.Rig.thighs[side].position;
                    float flex = Vector3.Angle(thigh, -up);
                    Vector3 w = miner.HangingWeight(k) - pelvis.position;
                    Vector3 inP = new Vector3(Vector3.Dot(w, right), Vector3.Dot(w, up), Vector3.Dot(w, fwd));
                    d.HangsFrom = thing.bone.position;
                    d.HangsWay = (miner.HangingWeight(k) - thing.bone.position).normalized;
                    float deep = d.Deepest(.003f, out int under, out var at, ground, out float below);
                    // Where on the thing its deepest point is: below where it hangs from (along the way it hangs), out
                    // from the body (along the hips' stop), and across.
                    Vector3 hangs = d.HangsWay;
                    Vector3 outward = pelvis.rotation * thing.stopNormal;
                    Vector3 across = Vector3.Cross(hangs, outward).normalized;
                    Vector3 outSquare = Vector3.Cross(across, hangs);
                    Vector3 rel = at - thing.bone.position;
                    string where = deep > 0 ? string.Format(Plain, " deepest at below {0:0} out {1:0} across {2:0} mm", Vector3.Dot(rel, hangs) * 1000, Vector3.Dot(rel, outSquare) * 1000, Vector3.Dot(rel, across) * 1000) : "";
                    if (worst != null) { string key = now + " " + thing.bone.name; worst[key] = Mathf.Max(worst.TryGetValue(key, out var was) ? was : 0, deep); }
                    Debug.Log(string.Format(Plain, "BAG {0} {1} {2} {3} {4:0.00}s tilt {5:0} flex {6:0} weight(r,u,f) {7:0.000} {8:0.000} {9:0.000} plane {10:0.0} mm skin {11:0.0} mm ({12}/{13} pts) hips {15:0.0}/{16} skirt {17:0.0}/{18} leg {19:0.0}/{20} ground {14:0.0} mm",
                        name, way, now, thing.bone.name, clock(), tilt, flex, inP.x, inP.y, inP.z, miner.HangingIntoBody(k) * 1000, deep * 1000, under, d.Points, below * 1000,
                        d.DeepIn[0] * 1000, d.PointsIn[0], d.DeepIn[1] * 1000, d.PointsIn[1], d.DeepIn[2] * 1000, d.PointsIn[2]) + where + string.Format(Plain, " above {0:0.0} legat {1:0.0} cloth {2:0.0} hook {3:0.0}", d.DeepAbove * 1000, LegAt(miner, k) * 1000, miner.ClothTurned(k),
                        ground != null ? (ground(thing.bone.position.x, thing.bone.position.z) - thing.bone.position.y) * 1000 : float.NaN)
                        + string.Format(Plain, " at {0:0.0000} {1:0.0000} {2:0.0000} hookat {3:0.0000} {4:0.0000} {5:0.0000}", miner.HangingWeight(k).x, miner.HangingWeight(k).y, miner.HangingWeight(k).z,
                            thing.bone.position.x, thing.bone.position.y, thing.bone.position.z));
                }
                then?.Invoke(frame);
            };
            return after;
        }

        // Standing and walking: how far the body's last word moves what hangs (MinerBody.HungMovedMost), and where each
        // thing's weight is in the pelvis' frame every frame (BAGPATH), to set beside a run with -hungAsBefore 1.
        // -bagMiner Small,Long,Round -bagOwn 1 (standing by its own joints, with life, as played)
        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator Walks()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            bool own = CaptureTools.Argument("-bagOwn") == "1";
            if (!own) CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = 50;
            MinerBody.HungAsBefore = CaptureTools.Argument("-hungAsBefore") == "1";
            MinerBody.HungAsOn15 = CaptureTools.Argument("-hungAsOn15") == "1";
            string[] who = (CaptureTools.Argument("-bagMiner") ?? "Small,Long,Round").Split(',');
            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
            var a = ground.Path[4];
            var b = ground.Path[5];
            var spot = OnGround(new Vector3(a.x, 0, a.y));
            var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
            away.y = 0;
            away.Normalize();
            try
            {
                foreach (string name in who)
                {
                    int index = -1;
                    for (int i = 0; i < choice.Count; i++) if (string.Equals(choice.NameOf(i), name, StringComparison.OrdinalIgnoreCase)) index = i;
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    var miner = unit.GetComponent<MinerBody>();
                    unit.Motor.Stop();
                    unit.GetComponent<NavMeshAgent>().Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    unit.GetComponent<ProceduralBiped>().ResetPose();
                    for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                    var pelvis = miner.Rig.pelvis;
                    string part = "stand";
                    float began = Time.time;
                    var into = new float[miner.Things.Count];
                    var after = new GameObject("After everything").AddComponent<AfterEverything>();
                    after.Then = () =>
                    {
                        for (int k = 0; k < miner.Things.Count; k++)
                        {
                            if (miner.Things[k].bone == null) continue;
                            into[k] = Mathf.Max(into[k], miner.HangingIntoBody(k));
                            Vector3 w = Quaternion.Inverse(pelvis.rotation) * (miner.HangingWeight(k) - pelvis.position);
                            // (Its leg: how far its weight is from that thigh's line, and how far the thigh is from the
                            // way the hips' own down is; how far the cloth under it has turned.)
                            int leg = Vector3.Dot(miner.HangingWeight(k) - pelvis.position, miner.Rig.thighs[1].position - miner.Rig.thighs[0].position) < 0 ? 0 : 1;
                            Vector3 hip = miner.Rig.thighs[leg].position, knee = miner.Rig.shins[leg].position, ab = knee - hip;
                            Vector3 near = hip + ab * Mathf.Clamp01(Vector3.Dot(miner.HangingWeight(k) - hip, ab) / ab.sqrMagnitude);
                            Vector3 down = -(miner.Rig.spine.position - pelvis.position);
                            Debug.Log(string.Format(Plain, "BAGPATH {0} {1} {2} {3:0.00} {4:0.0000} {5:0.0000} {6:0.0000} leg {7:0.0000} flex {8:0.0} cloth {9:0.0}", name, miner.Things[k].bone.name, part, Time.time - began, w.x, w.y, w.z,
                                Vector3.Distance(miner.HangingWeight(k), near), Vector3.Angle(ab, Vector3.ProjectOnPlane(down, miner.Rig.thighs[1].position - miner.Rig.thighs[0].position)), miner.ClothTurned(k)));
                        }
                    };
                    MinerBody.HungMovedMost = 0;
                    for (float until = Time.time + (own ? 6 : 3); Time.time < until;) yield return null;
                    Debug.Log(string.Format(Plain, "BAGMOVED {0} standing: the last word moved a thing {1:0.00} mm at most; into the body at most {2}", name, MinerBody.HungMovedMost * 1000, string.Join(", ", Array.ConvertAll(into, x => (x * 1000).ToString("0.0", Plain)))));
                    if (own) { after.Then = null; UnityEngine.Object.Destroy(after.gameObject); continue; }
                    // There and back along the path, turning round between.
                    for (int k = 0; k < into.Length; k++) into[k] = 0;
                    MinerBody.HungMovedMost = 0;
                    part = "walk";
                    unit.Motor.TryMove(OnGround(spot + away * 6));
                    for (float until = Time.time + 12; Time.time < until && (unit.Motor.IsMoving || Time.time - began < 4);) yield return null;
                    unit.Motor.TryMove(OnGround(spot - away * 1));
                    for (float until = Time.time + 14; Time.time < until && unit.Motor.IsMoving;) yield return null;
                    for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                    Debug.Log(string.Format(Plain, "BAGMOVED {0} walking: the last word moved a thing {1:0.00} mm at most; into the body at most {2}", name, MinerBody.HungMovedMost * 1000, string.Join(", ", Array.ConvertAll(into, x => (x * 1000).ToString("0.0", Plain)))));
                    after.Then = null;
                    UnityEngine.Object.Destroy(after.gameObject);
                }
            }
            finally
            {
                MinerBody.HungAsBefore = MinerBody.HungAsOn15 = false;
                Time.captureFramerate = 0;
            }
        }

        // At work: each miner goes for a pickaxe that lies by it, picks it up (bowing low) and mines a boulder.
        // -bagMiner Small -bagBoulders 1,3,4 -bagFor 25
        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator AtWork()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = 50;
            MinerBody.HungAsBefore = CaptureTools.Argument("-hungAsBefore") == "1";
            MinerBody.HungAsOn15 = CaptureTools.Argument("-hungAsOn15") == "1";
            string[] who = (CaptureTools.Argument("-bagMiner") ?? "Small,Long,Round").Split(',');
            string[] which = (CaptureTools.Argument("-bagBoulders") ?? "1,3,4").Split(',');
            float lasts = float.Parse(CaptureTools.Argument("-bagFor") ?? "25", Plain);
            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            var boulders = Boulder.All();
            if (CaptureTools.Argument("-bagOut") != null)
            {
                // (Pictures as the falls' are: the grass off, at midday.)
                foreach (var grass in UnityEngine.Object.FindObjectsByType<GrassField>(FindObjectsSortMode.None)) grass.enabled = false;
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                if (time != null) time.Hour = 13;
            }
            try
            {
                for (int m = 0; m < who.Length; m++)
                {
                    string name = who[m];
                    int index = -1;
                    for (int i = 0; i < choice.Count; i++) if (string.Equals(choice.NameOf(i), name, StringComparison.OrdinalIgnoreCase)) index = i;
                    choice.Choose(index);
                    for (float until = Time.time + .4f; Time.time < until;) yield return null;
                    var unit = choice.Current;
                    var miner = unit.GetComponent<MinerBody>();
                    var boulder = boulders[int.Parse(which[Mathf.Min(m, which.Length - 1)], Plain)];
                    Vector3 centre = boulder.Rock.bounds.center;
                    for (int k = 0; k < 12; k++)
                    {
                        Vector3 at = centre + Quaternion.Euler(0, k * 30, 0) * Vector3.forward * (Mathf.Max(boulder.Rock.bounds.extents.x, boulder.Rock.bounds.extents.z) + 2.5f);
                        if (!NavMesh.SamplePosition(at, out var walked, 1, NavMesh.AllAreas)) continue;
                        unit.Motor.Stop();
                        unit.GetComponent<NavMeshAgent>().Warp(walked.position);
                        Vector3 to = centre - walked.position; to.y = 0;
                        unit.transform.rotation = Quaternion.LookRotation(to.normalized);
                        unit.GetComponent<ProceduralBiped>().ResetPose();
                        break;
                    }
                    for (float until = Time.time + .6f; Time.time < until;) yield return null;
                    // A pickaxe on the ground beside it: it goes down for it first.
                    look.LayPickaxe(1);
                    for (float until = Time.time + 1; Time.time < until;) yield return null;
                    float began = Time.time;
                    string folder = CaptureTools.Argument("-bagOut");
                    string shootThing = CaptureTools.Argument("-bagThing");
                    var camera = Camera.main;
                    if (camera.TryGetComponent<RtsCamera>(out var rig)) rig.enabled = false;
                    camera.nearClipPlane = .01f;
                    if (folder != null) Directory.CreateDirectory(folder);
                    // (-bagFrom, -bagTo: every look between, from -bagViews, in the bench's other pictures' names.)
                    float shootFrom = float.Parse(CaptureTools.Argument("-bagFrom") ?? "-1", Plain), shootTo = float.Parse(CaptureTools.Argument("-bagTo") ?? "-1", Plain);
                    string[] views = (CaptureTools.Argument("-bagViews") ?? "0").Split(',');
                    int shot = 0;
                    var after = Watch(miner, name, "work", () => look.Carry != null && look.Carry.Fetching ? "pickup" : look.Mining != null ? "work" : "going", () => Time.time - began, (x, z) => Drawn(ground, x, z), null,
                        frame =>
                        {
                            if (folder == null) return;
                            float now = Time.time - began;
                            if (now >= shootFrom && now <= shootTo)
                            {
                                string stem = Path.Combine(folder, $"{name.ToLowerInvariant()}_work_{shot++:000}_{(look.Mining != null ? "work" : "pickup")}_{now:00.00}");
                                foreach (string v in views) Shoot(camera, miner, stem + "_" + v.Replace("-", "m"), .9f, 320, shootThing, float.Parse(v, Plain));
                            }
                            else if (shootFrom < 0 && frame % 10 == 1) Shoot(camera, miner, Path.Combine(folder, $"bag_{name.ToLowerInvariant()}_{now:00.00}"), .8f, 360, shootThing);
                        });
                    look.Mine(boulder);
                    for (float until = Time.time + lasts; Time.time < until;) yield return null;
                    after.Then = null;
                    UnityEngine.Object.Destroy(after.gameObject);
                    look.End();
                    look.ClearLaid();
                    for (float until = Time.time + 1; Time.time < until;) yield return null;
                }
            }
            finally
            {
                MinerBody.HungAsBefore = MinerBody.HungAsOn15 = false;
                Time.captureFramerate = 0;
            }
        }

        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator Bowed()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = 50;
            MinerBody.HungAsBefore = CaptureTools.Argument("-hungAsBefore") == "1";
            MinerBody.HungAsOn15 = CaptureTools.Argument("-hungAsOn15") == "1";
            string folder = CaptureTools.Argument("-bagOut");
            bool shots = CaptureTools.Argument("-bagShots") == "1" && folder != null;
            if (folder != null) Directory.CreateDirectory(folder);
            int every = int.Parse(CaptureTools.Argument("-bagEvery") ?? "5", Plain);
            int size = int.Parse(CaptureTools.Argument("-bagSize") ?? "320", Plain);
            float view = float.Parse(CaptureTools.Argument("-bagView") ?? "1.0", Plain);
            float shootFrom = float.Parse(CaptureTools.Argument("-bagFrom") ?? "-1", Plain), shootTo = float.Parse(CaptureTools.Argument("-bagTo") ?? "-1", Plain);
            string[] who = (CaptureTools.Argument("-bagMiner") ?? "Small,Long,Round").Split(',');
            string[] ways = (CaptureTools.Argument("-bagWays") ?? "front,back,left,right").Split(',');
            // Which thing is pictured (by its bone's name; the first on a hook if none), and from where: degrees round
            // from its side (towards the front, positive), or "below" (from under the ground, the ground not drawn).
            string shootThing = CaptureTools.Argument("-bagThing");
            string[] views = (CaptureTools.Argument("-bagViews") ?? "0").Split(',');
            foreach (var grass in UnityEngine.Object.FindObjectsByType<GrassField>(FindObjectsSortMode.None)) grass.enabled = false;
            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            camera.nearClipPlane = .01f;
            var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
            if (time != null) time.Hour = 13;
            Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
            var a = ground.Path[4];
            var b = ground.Path[5];
            var spot = OnGround(new Vector3(a.x, 0, a.y));
            var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
            away.y = 0;
            away.Normalize();
            try
            {
                foreach (string name in who)
                {
                    int index = -1;
                    for (int i = 0; i < choice.Count; i++) if (string.Equals(choice.NameOf(i), name, StringComparison.OrdinalIgnoreCase)) index = i;
                    Assert.That(index, Is.GreaterThanOrEqualTo(0), name);
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    var miner = unit.GetComponent<MinerBody>();
                    var biped = unit.GetComponent<ProceduralBiped>();
                    foreach (string way in ways)
                    {
                        unit.Motor.Stop();
                        unit.GetComponent<NavMeshAgent>().Warp(spot);
                        unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                        biped.ResetPose();
                        for (float until = Time.time + 1.2f; Time.time < until;) yield return null;
                        string phase = "stand";
                        float began = Time.time;
                        int shot = 0;
                        var worst = new Dictionary<string, float>();
                        var after = Watch(miner, name, way, () => phase, () => Time.time - began, (x, z) => Drawn(ground, x, z), worst, frame =>
                        {
                            float now = Time.time - began;
                            if (!shots || !(frame % every == 0 || (now >= shootFrom && now <= shootTo))) return;
                            string stem = Path.Combine(folder, $"{name.ToLowerInvariant()}_{way}_{shot++:000}_{phase}_{now:00.00}");
                            foreach (string v in views)
                            {
                                if (v == "below") { Below(camera, miner, stem + "_below", view, size, shootThing); continue; }
                                Shoot(camera, miner, stem + "_" + v.Replace("-", "m"), view, size, shootThing, float.Parse(v, Plain));
                            }
                        });
                        for (float until = Time.time + .6f; Time.time < until;) yield return null;
                        Vector3 push = way == "front" ? -unit.transform.forward : way == "back" ? unit.transform.forward : way == "left" ? unit.transform.right : -unit.transform.right;
                        // (front: it falls on its front.)
                        push = way == "front" ? unit.transform.forward : way == "back" ? -unit.transform.forward : way == "left" ? -unit.transform.right : unit.transform.right;
                        phase = "fall";
                        look.PushOver(push);
                        yield return null;
                        var fall = unit.GetComponent<PhysicalFall>();
                        Assert.That(fall, Is.Not.Null);
                        float start = Time.time;
                        while (fall.Now != PhysicalFall.State.Up && Time.time - start < 40)
                        {
                            phase = fall.Now == PhysicalFall.State.Gathering ? "gather-" + fall.WayNow + "-" + fall.StageNow : fall.Now.ToString().ToLowerInvariant();
                            yield return null;
                        }
                        phase = "up";
                        for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                        after.Then = null;
                        UnityEngine.Object.Destroy(after.gameObject);
                        foreach (var kv in worst) Debug.Log(string.Format(Plain, "BAGWORST {0} {1} {2}: {3:0.0} mm", name, way, kv.Key, kv.Value * 1000));
                        yield return null;
                    }
                }
            }
            finally
            {
                MinerBody.HungAsBefore = MinerBody.HungAsOn15 = false;
                Time.captureFramerate = 0;
            }
        }
    }
}
