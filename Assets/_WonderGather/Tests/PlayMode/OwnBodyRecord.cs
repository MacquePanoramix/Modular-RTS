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
    // Pictures of the body's own body standing, for whoever judges it (not a test): each miner standing at ease for
    // half a minute; its breath from the side, at rest and tired; its weight going from one leg to the other, from
    // in front; nudged; and the three together from where the game is played, its chest empty and full.
    //   -ownShots <folder> [-ownShotMiner Round] [-ownShotSize 360] [-ownShotBreath 1] [-ownShotOnly stands,breath,shift,nudged,far]
    [Explicit("A recording for the judges, not a test.")]
    public sealed class OwnBodyRecord
    {
        private static float Number(string argument, float otherwise)
        {
            string given = CaptureTools.Argument(argument);
            return given != null && float.TryParse(given, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : otherwise;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);
        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        [UnityTest, Timeout(3600000)]
        public IEnumerator Record()
        {
            string folder = CaptureTools.Argument("-ownShots") ?? Path.Combine(Application.dataPath, "..", "Captures", "Own");
            string who = CaptureTools.Argument("-ownShotMiner"), only = CaptureTools.Argument("-ownShotOnly");
            int size = (int)Number("-ownShotSize", 360);
            float shown = Number("-ownShotBreath", OwnBody.BreathDrawn);
            bool Wanted(string what) => string.IsNullOrEmpty(only) || Array.IndexOf(only.Split(','), what) >= 0;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            Time.captureFramerate = 50;
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                var camera = Camera.main;
                var rig = camera.GetComponent<RtsCamera>();
                Vector3 gameAt = camera.transform.position; Quaternion gameTurned = camera.transform.rotation; float gameField = camera.fieldOfView;
                if (rig != null) rig.enabled = false;
                camera.nearClipPlane = .01f;
                time.Hour = 13;
                // (The meadow's grass stands between the camera and its boots from the side: it is not drawn.)
                var grass = UnityEngine.Object.FindAnyObjectByType<GrassField>();
                if (grass != null) grass.enabled = false;
                Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
                var a = ground.Path[4];
                var b = ground.Path[5];
                Vector3 spot = OnGround(new Vector3(a.x, 0, a.y));
                Vector3 away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
                away.y = 0;
                away.Normalize();
                Vector3 right = Vector3.Cross(Vector3.up, away);
                OwnBody.BreathShown = shown;

                for (int index = 0; index < choice.Count; index++)
                {
                    string name = choice.NameOf(index), tag = name.ToLowerInvariant();
                    if (!string.IsNullOrEmpty(who) && !string.Equals(who, name, StringComparison.OrdinalIgnoreCase)) continue;
                    choice.Choose(index);
                    yield return Wait(.3f);
                    var unit = choice.Current;
                    unit.Motor.Stop();
                    unit.GetComponent<NavMeshAgent>().Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    unit.GetComponent<ProceduralBiped>().ResetPose();
                    unit.GetComponent<PhysicalBody>().Refresh();
                    yield return Wait(.8f);
                    float tall = unit.GetComponent<MinerBody>().Solved.head.position.y - spot.y + .15f;
                    // The camera: so many degrees round from in front of it, so far off (in its heights), looking at
                    // so high a share of it.
                    void View(float round, float far, float high)
                    {
                        Vector3 from = spot + Quaternion.AngleAxis(round, Vector3.up) * away * (far * tall) + Vector3.up * (high * tall);
                        camera.transform.SetPositionAndRotation(from, Quaternion.LookRotation(spot + Vector3.up * (high * tall) - from));
                        camera.fieldOfView = 30;
                    }
                    look.SetOwn(true);
                    OwnBody own = null;
                    for (float began = Time.time; Time.time - began < 4 && (own == null || !own.Stands);) { unit.TryGetComponent(out own); yield return null; }
                    Assert.That(own != null && own.Stands, Is.True, name + " did not come to stand by its own joints.");
                    Transform hips = own.Part(OwnBody.Hips).transform, trunk = own.Part(OwnBody.Trunk).transform, head = own.Part(OwnBody.Head).transform;
                    Vector3 hipsBegan = hips.position, headBegan = head.position;
                    float Knee(int i) => Vector3.Angle(own.Part(OwnBody.Thigh + i).transform.up, own.Part(OwnBody.Shin + i).transform.up);
                    string Says(float t) => string.Format(CultureInfo.InvariantCulture,
                        "{0:F2} s: breath {1:F2}; favours {2:F2}; hips {3:F0} mm right, {4:F0} ahead, roll {5:F1}; chest leans {6:F1} ahead, {7:F1} right; shoulders roll {8:F1}; head {9:F0} mm right, {10:F0} ahead, {11:F0} up, looks {12:F0} right {13:F0} down; knees {14:F1} {15:F1}",
                        t, own.BreathFull, own.Favours, Vector3.Dot(hips.position - hipsBegan, right) * 1000, Vector3.Dot(hips.position - hipsBegan, away) * 1000,
                        Mathf.Asin(Mathf.Clamp(hips.right.y, -1, 1)) * Mathf.Rad2Deg, Mathf.Asin(Mathf.Clamp(Vector3.Dot(trunk.up, away), -1, 1)) * Mathf.Rad2Deg,
                        Mathf.Asin(Mathf.Clamp(Vector3.Dot(trunk.up, right), -1, 1)) * Mathf.Rad2Deg, Mathf.Asin(Mathf.Clamp(trunk.right.y, -1, 1)) * Mathf.Rad2Deg,
                        Vector3.Dot(head.position - headBegan, right) * 1000, Vector3.Dot(head.position - headBegan, away) * 1000, (head.position.y - headBegan.y) * 1000,
                        own.Looks.x, own.Looks.y, Knee(0), Knee(1));
                    // So many seconds, a picture every so many steps of the physics, and a line of figures with each.
                    IEnumerator Shoot(string what, float seconds, int every, Action<int> at = null)
                    {
                        int steps = Mathf.RoundToInt(seconds * 50), shot = 0;
                        for (int k = 0; k <= steps; k++)
                        {
                            at?.Invoke(k);
                            if (k % every == 0)
                            {
                                CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{what}_{shot:000}"), size, size);
                                Debug.Log($"OWNREC {name} {what} {shot:000} " + (own.Stands ? Says(k * .02f) : "it is down"));
                                shot++;
                            }
                            yield return new WaitForFixedUpdate();
                        }
                    }

                    // Standing at ease, half a minute, from in front and a little to one side: twice a second.
                    if (Wanted("stands")) { View(25, 2.3f, .5f); yield return Shoot("stands", 30, 25); }
                    // Its breath from the side, near: five times a second for ten seconds, at rest; then tired.
                    if (Wanted("breath"))
                    {
                        View(-90, 2.2f, .5f);
                        yield return Shoot("breath", 10, 10);
                        look.Tire();
                        yield return Wait(1);
                        yield return Shoot("tired", 6, 5);
                        unit.GetComponent<PhysicalBody>().Refresh();
                        yield return Wait(4);
                    }
                    // Its weight going onto its right leg, then its left: from straight in front, five times a second.
                    if (Wanted("shift"))
                    {
                        View(0, 2.3f, .5f);
                        own.Favour(0);
                        yield return Wait(3);
                        yield return Shoot("shift", 9, 10, k => { if (k == 25) own.Favour(1); if (k == 225) own.Favour(-1); });
                    }
                    // A look: asked to look forty degrees to its right, from in front: ten times a second.
                    if (Wanted("look"))
                    {
                        View(0, 2.3f, .5f);
                        own.Favour(0);
                        own.LookAt(head.position + away * 3, 3);
                        yield return Wait(3);
                        Vector3 place = head.position + Quaternion.AngleAxis(40, Vector3.up) * away * 3;
                        yield return Shoot("look", 2.4f, 5, k => { if (k == 15) own.LookAt(place, 3); });
                    }
                    // Nudged from in front (set going backwards at 0.15 m/s), from the side: ten times a second.
                    if (Wanted("nudged"))
                    {
                        View(-90, 2.5f, .5f);
                        own.Favour(0);
                        yield return Wait(3);
                        yield return Shoot("nudged", 3, 5, k => { if (k == 25) own.Nudge(-away, .15f); });
                    }
                    look.SetOwn(false);
                    yield return Wait(.6f);
                }

                // The three together, each standing by its own joints, from where the game is played: a picture with
                // their chests empty and one with them full, as breath is drawn now.
                if (Wanted("far"))
                {
                    var owns = new OwnBody[choice.Count];
                    for (int index = 0; index < choice.Count; index++)
                    {
                        choice.Choose(index);
                        yield return Wait(.2f);
                        var unit = choice.Current;
                        Vector3 place = OnGround(spot + right * (index - 1) * 1.1f);
                        unit.Motor.Stop();
                        unit.GetComponent<NavMeshAgent>().Warp(place);
                        unit.transform.SetPositionAndRotation(place, Quaternion.LookRotation(-away));
                        unit.GetComponent<ProceduralBiped>().ResetPose();
                        owns[index] = unit.GetComponent<OwnBody>();
                        if (owns[index] == null) owns[index] = unit.gameObject.AddComponent<OwnBody>();
                    }
                    choice.Choose(0);
                    yield return Wait(1);
                    look.SetOwn(true);
                    foreach (var one in owns) one.Wanted = true;
                    // (The look at the work keeps only the chosen one so: the others are asked again each frame.)
                    IEnumerator Keep(float seconds) { for (float until = Time.time + seconds; Time.time < until;) { foreach (var one in owns) one.Wanted = true; yield return null; } }
                    yield return Keep(3);
                    Vector3 from = spot - away * 7 + Vector3.up * 6.5f;
                    camera.transform.SetPositionAndRotation(from, Quaternion.LookRotation(spot + Vector3.up * .6f - from));
                    camera.fieldOfView = gameField;
                    for (int shot = 0; shot < 40; shot++)
                    {
                        CaptureTools.Render(camera, Path.Combine(folder, $"far_{shot:000}"), 960, 540);
                        string said = "";
                        for (int index = 0; index < owns.Length; index++) said += $" {choice.NameOf(index)} {(owns[index].Stands ? owns[index].BreathFull.ToString("F2", CultureInfo.InvariantCulture) : "posed")}";
                        Debug.Log($"OWNREC far {shot:000}{said}");
                        yield return Keep(.2f);
                    }
                    look.SetOwn(false);
                    foreach (var one in owns) one.Wanted = false;
                }
                camera.transform.SetPositionAndRotation(gameAt, gameTurned);
            }
            finally
            {
                Time.captureFramerate = 0; OwnBody.BreathShown = OwnBody.BreathDrawn; OwnBody.Alive = true;
                var grass = UnityEngine.Object.FindAnyObjectByType<GrassField>();
                if (grass != null) grass.enabled = true;
            }
        }
    }
}
