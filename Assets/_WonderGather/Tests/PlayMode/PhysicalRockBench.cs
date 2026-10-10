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
    // The bench of S3's step 9: the place's boulders, and what each miner finds on each (where to stand, where to
    // strike). With -rockMine it is also sent to mine some of them, with pictures.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.PhysicalRockBench -rockOut <folder>
    //   [-rockMiner Round] [-rockMine 0,3,7] [-rockBlows 4] [-rockFor 90] [-rockView 250,12,3.2] [-rockSize 360] [-rockStrength 1]
    public sealed class PhysicalRockBench
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
        public IEnumerator Bench()
        {
            string folder = CaptureTools.Argument("-rockOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Rocks");
            string who = CaptureTools.Argument("-rockMiner");
            float[] mine = Numbers("-rockMine");
            float[] view = Numbers("-rockView", 250, 12, 3.2f);
            int size = (int)Numbers("-rockSize", 360)[0], blowsWanted = (int)Numbers("-rockBlows", 4)[0];
            // -rockFor: how long it is left at a boulder at most (seconds).
            float lasts = Numbers("-rockFor", 90)[0];
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = 50;
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
                var click = UnityEngine.Object.FindAnyObjectByType<InteractionClick>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                var camera = Camera.main;
                var rig = camera.GetComponent<RtsCamera>();
                if (rig != null) rig.enabled = false;
                camera.nearClipPlane = .01f;
                time.Hour = 13;
                var boulders = Boulder.All();
                Debug.Log(string.Format(culture, "ROCK the place has {0} boulders", boulders.Count));
                for (int b = 0; b < boulders.Count; b++)
                {
                    var bounds = boulders[b].Rock.bounds;
                    Debug.Log(string.Format(culture, "ROCK boulder {0}: {1} at ({2:0.0}, {3:0.0}), {4:0.00} x {5:0.00} x {6:0.00} m (wide, high, deep), its top {7:0.00} m over the ground at its middle",
                        b, boulders[b].Rock.name, bounds.center.x, bounds.center.z, bounds.size.x, bounds.size.y, bounds.size.z, bounds.max.y - ground.Height(bounds.center.x, bounds.center.z)));
                }
                for (int index = 0; index < choice.Count; index++)
                {
                    string name = choice.NameOf(index);
                    if (!string.IsNullOrEmpty(who) && !string.Equals(who, name, StringComparison.OrdinalIgnoreCase)) continue;
                    choice.Choose(index);
                    yield return null;
                    var unit = choice.Current;
                    var miner = unit.GetComponent<MinerBody>();
                    var biped = unit.GetComponent<ProceduralBiped>();
                    for (float until = Time.time + .5f; Time.time < until;) yield return null;
                    float tall = miner.Rig.head.position.y - unit.transform.position.y;
                    Debug.Log(string.Format(culture, "ROCK {0}'s pickaxe: its striking ball at {1} (radius {2:0.000}), its point at {3}, from foot {4:0.000} to top {5:0.000}, lower hand at {6:0.000}", name, miner.Pickaxe.Head.ToString("F3"), miner.Pickaxe.HeadRadius, miner.Pickaxe.Point.ToString("F3"), miner.Pickaxe.Foot, miner.Pickaxe.Top, miner.Pickaxe.PrimaryGrip.y));
                    // What it finds on each boulder, coming from where it stands and from four sides.
                    var plans = unit.gameObject.AddComponent<PhysicalSwing>();
                    plans.enabled = false; plans.body = biped; plans.tool = miner.Pickaxe;
                    Debug.Log(string.Format(culture, "ROCK {0}: holding its pickaxe at its side asks {1:0}% of its shoulder, fresh ({2:0}% when 40% spent): it rests {3}",
                        name, plans.HoldAsks * 100, plans.HoldAsks / .6f * 100, "in one hand"));
                    for (int b = 0; b < boulders.Count; b++)
                    {
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        var plan = RockWork.Find(boulders[b], plans, biped, tall, unit.transform.position);
                        watch.Stop();
                        string why = RockWork.Last;
                        float sides = plan.found ? Vector3.ProjectOnPlane(plan.approach - plan.stand, Vector3.up).magnitude : 0;
                        Debug.Log(plan.found
                            ? string.Format(culture, "ROCK {0} on boulder {1}: strikes {2:0.00} m up ({3:0}% of its height), {4:0} degrees round from where it came; stands {5:0.00} m from the spot; bow {6:0}, lean {7:0}, knees {8:0.00} m, miss {9:0} mm; the rock faces {10:0} degrees from up there; found in {11:0} ms; its place is {12:0.00} m off the walked ground; the head is to land {13:0} mm from the spot",
                                name, b, plan.height, plan.height / tall * 100, plan.round, plan.away, plan.aimed.bow, plan.aimed.lean, plan.aimed.sink, plan.aimed.miss * 1000,
                                Vector3.Angle(plan.outward, Vector3.up), watch.ElapsedMilliseconds, sides, Vector3.Distance(plan.lands, plan.spot) * 1000)
                            : string.Format(culture, "ROCK {0} on boulder {1}: no place found ({2} ms). From where it came: {4}", name, b, watch.ElapsedMilliseconds, sides, why));
                    }
                    UnityEngine.Object.Destroy(plans);
                    yield return null;

                    // Sent to mine some of them.
                    foreach (float which in mine)
                    {
                        var boulder = boulders[(int)which];
                        int shot = 0, frame = 0;
                        bool taking = true;
                        var after = new GameObject("After everything").AddComponent<AfterEverything>();
                        Vector3 middle = boulder.Rock.bounds.center;
                        after.Then = () =>
                        {
                            if (!taking || frame++ % 2 != 0) return;
                            Vector3 target = Vector3.Lerp(unit.transform.position + Vector3.up * tall * .5f, middle, .35f);
                            Vector3 away = Vector3.ProjectOnPlane(unit.transform.position - middle, Vector3.up).normalized;
                            Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * -away;
                            dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                            camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                            CaptureTools.Render(camera, Path.Combine(folder, $"{name.ToLowerInvariant()}_rock{(int)which}_{shot:000}"), size, size);
                            shot++;
                        };
                        look.SetStrength(Numbers("-rockStrength", 1)[0]);
                        // A pickaxe is put on the ground beside it, as the panel puts it: its own at a share of its
                        // weight (-rockWeight), or another miner's own (-rockPickaxeOf).
                        if (!look.Showing)
                        {
                            string of = CaptureTools.Argument("-rockPickaxeOf");
                            if (!string.IsNullOrEmpty(of))
                                for (int other = 0; other < choice.Count; other++)
                                {
                                    if (!string.Equals(of, choice.NameOf(other), StringComparison.OrdinalIgnoreCase)) continue;
                                    choice.Choose(other);
                                    yield return null;
                                    break;
                                }
                            var put = look.LayPickaxe(Numbers("-rockWeight", 1)[0]);
                            if (choice.Chosen != index) { choice.Choose(index); yield return null; }
                            Assert.That(put, Is.Not.Null, "No pickaxe was put on the ground beside " + name);
                            for (float until = Time.time + 1; Time.time < until;) yield return null;
                            Debug.Log(string.Format(culture, "ROCK {0} has a pickaxe of {1:0.00} kg ({2}) on the ground {3:0.00} m from it, {4:0.000} m over the ground",
                                name, put.GetComponent<Rigidbody>().mass, put.Tool.name, Vector3.ProjectOnPlane(put.GetComponent<Rigidbody>().worldCenterOfMass - unit.transform.position, Vector3.up).magnitude,
                                put.GetComponent<Rigidbody>().worldCenterOfMass.y - ground.Height(put.transform.position.x, put.transform.position.z)));
                        }
                        Assert.That(click.OpenOn(boulder) && click.Choose("Mine"), Is.True, name + " was not offered to mine boulder " + which);
                        float began = Time.time, said = Time.time;
                        int stones = boulder.Stones.Count, blows = boulder.Blows;
                        while (boulder.Blows - blows < blowsWanted && Time.time - began < lasts)
                        {
                            if (Time.time - said > 2)
                            {
                                said = Time.time;
                                Debug.Log(string.Format(culture, "ROCK {0} at boulder {1} {2:0.0}s: {20}; showing {3}, swinging {4}, carrying {5}, moving {6}, off the walked ground {10}, blows on it {7}, stones {8}, {9:0.00} m from where it means to stand; the swing {11}{17}, spent {12:0}%, its last blow on {19}; gave it up: {13}; right arm gives {18}; left arm gives {14} (shoulder, elbow, wrist, hold), its hand at {15:0.000} on a handle whose weight is at {16:0.000}",
                                    name, which, Time.time - began, look.Showing, look.Swinging, look.Carrying, unit.Motor.IsMoving, boulder.Blows - blows, boulder.Stones.Count - stones,
                                    look.Mining != null ? Vector3.ProjectOnPlane(unit.transform.position - look.MiningPlan.stand, Vector3.up).magnitude : -1, unit.Motor.IsOff,
                                    look.Swing != null ? look.Swing.phase.ToString() : "-", look.Swing != null ? look.Swing.Spent * 100 : 0, look.Mining == null ? look.LeftRock : "no",
                                    unit.GetComponent<PhysicalHands>() != null ? unit.GetComponent<PhysicalHands>().EffortOf(0).ToString("F2") : "-",
                                    unit.GetComponent<PhysicalHands>() != null ? unit.GetComponent<PhysicalHands>().GripAlong(0) : 0, miner.Pickaxe.Centre.y,
                                    "",
                                    unit.GetComponent<PhysicalHands>() != null ? unit.GetComponent<PhysicalHands>().EffortOf(1).ToString("F2") : "-", look.StruckLast, look.Status().Replace("\n", " / ")));
                            }
                            yield return null;
                        }
                        for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                        taking = false;
                        after.Then = null;
                        UnityEngine.Object.Destroy(after.gameObject);
                        var plan = look.MiningPlan;
                        string swings = "";
                        if (look.Swing != null)
                            foreach (var r in look.Swing.results)
                                swings += string.Format(culture, " [{0}, {1:0.0} m/s, {2:0} J, {3:0} mm from where it was to land]", r.struck ? "struck" : "missed", r.speed, r.energy, Vector3.Distance(r.landed, plan.lands) * 1000);
                        Debug.Log(string.Format(culture, "ROCK {0} mined boulder {1}: {2} blows on it in {3:0.0} s, {4} stones off it ({5} pictures); it stands {6:0.000} m from where it meant to; swings:{7}",
                            name, which, boulder.Blows - blows, Time.time - began - 1.5f, boulder.Stones.Count - stones, shot,
                            Vector3.ProjectOnPlane(unit.transform.position - plan.stand, Vector3.up).magnitude, swings));
                        string landed = "";
                        foreach (var point in boulder.Struck)
                        {
                            Vector3 off = Quaternion.Inverse(plan.facing) * (point - plan.lands) * 1000;
                            landed += string.Format(culture, " {0:0} ({1:0} right, {2:0} up, {3:0} ahead)", off.magnitude, off.x, off.y, off.z);
                        }
                        Debug.Log(string.Format(culture, "ROCK {0} on boulder {1}: its blows landed{2} mm from where the plan put them", name, which, landed));
                        foreach (var stone in boulder.Stones)
                            Debug.Log(string.Format(culture, "ROCK   a stone of {0:0.00} kg lies {1:0.00} m from the spot, {2:0.00} m over the ground, moving at {3:0.00} m/s",
                                stone.mass, Vector3.Distance(stone.position, plan.spot), stone.position.y - ground.Height(stone.position.x, stone.position.z), stone.linearVelocity.magnitude));
                        if (look.Showing) look.End();
                        look.SetStrength(1);
                        for (float until = Time.time + .5f; Time.time < until;) yield return null;
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
