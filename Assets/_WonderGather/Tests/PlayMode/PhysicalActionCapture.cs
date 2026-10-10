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
    // Pictures of the small actions of S3's step 8, each done by the miner's body: it works, lays its pickaxe down on
    // the ground, walks a little off, comes back to pick it up, and carries it. The orders are given as the
    // interaction click's box gives them.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.PhysicalActionCapture -actionOut <folder>
    //   [-actionMiner Round] [-actionView 250,10,2.6] [-actionStrength 1] [-actionWeight 1]
    public sealed class PhysicalActionCapture
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
        public IEnumerator Capture()
        {
            string folder = CaptureTools.Argument("-actionOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Actions");
            string who = CaptureTools.Argument("-actionMiner") ?? "Round";
            float[] view = Numbers("-actionView", 250, 10, 2.6f);
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
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
                Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
                var a = ground.Path[4];
                var b = ground.Path[5];
                var spot = OnGround(new Vector3(a.x, 0, a.y));
                var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
                away.y = 0;
                away.Normalize();
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (string.Equals(choice.NameOf(i), who, StringComparison.OrdinalIgnoreCase)) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner is called " + who);
                choice.Choose(index);
                yield return null;
                var unit = choice.Current;
                var miner = unit.GetComponent<MinerBody>();
                var biped = unit.GetComponent<ProceduralBiped>();
                unit.Motor.Stop();
                unit.GetComponent<NavMeshAgent>().Warp(spot);
                unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                biped.ResetPose();
                for (float until = Time.time + .6f; Time.time < until;) yield return null;
                float height = miner.Rig.head.position.y - unit.transform.position.y;
                look.SetStrength(Numbers("-actionStrength", 1)[0]);
                look.SetWeight(Numbers("-actionWeight", 1)[0]);

                int shot = 0, frame = 0;
                string part = "work";
                bool taking = false;
                var after = new GameObject("After everything").AddComponent<AfterEverything>();
                after.Then = () =>
                {
                    if (!taking || frame++ % 2 != 0) return;
                    Vector3 target = unit.transform.position + Vector3.up * height * .45f;
                    Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * away;
                    dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                    camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                    CaptureTools.Render(camera, Path.Combine(folder, $"{who.ToLowerInvariant()}_{part}_{shot:000}"), 360, 360);
                    shot++;
                };
                void Say(string what)
                {
                    var hands = unit.GetComponent<PhysicalHands>();
                    var carry = look.Carry;
                    Debug.Log(string.Format(culture, "ACTION {0} {1:0.0}s {2}: showing {3}, carrying {4}, way {5}, fetching {6}, laying {7}, holds {8}{9}, bow {10:0.0}, sink {11:0.000}, moving {12}, lying {13}",
                        who, Time.time, what, look.Showing, look.Carrying, carry != null ? carry.way.ToString() : "-", carry != null && carry.Fetching, carry != null && carry.Laying,
                        hands != null && hands.Holds(0) ? "L" : "-", hands != null && hands.Holds(1) ? "R" : "-", biped.BowNow, biped.SinkNow, unit.Motor.IsMoving, look.Lying != null)
                        + (hands != null && hands.Held != null ? string.Format(culture, "; the place to take it is {0} from the miner (right, up, ahead), its left shoulder {1}, {2:0.000} m apart; arm {3:0.000}",
                            (Quaternion.Inverse(biped.FacingNow) * (hands.PlaceAlong(hands.HighestGrip) - unit.transform.position)).ToString("F3"),
                            (Quaternion.Inverse(biped.FacingNow) * (biped.ShoulderNow(0) - unit.transform.position)).ToString("F3"),
                            Vector3.Distance(hands.PlaceAlong(hands.HighestGrip), biped.ShoulderNow(0)), biped.ArmReach) : ""));
                }

                // It works.
                look.Toggle();
                float began = Time.time;
                while ((!look.Swinging || look.Swing.results.Count < 1) && Time.time - began < 30) yield return null;
                var thing = unit.GetComponent<PhysicalHands>().Thing;
                Say("worked");

                // It lays its pickaxe down.
                part = "lay"; shot = 0; taking = true;
                Assert.That(click.OpenOn(thing) && click.Choose("Lay it down"), Is.True);
                began = Time.time;
                float said = Time.time;
                while (look.Showing && Time.time - began < 15)
                {
                    if (Time.time - said > 1) { said = Time.time; Say("laying"); }
                    yield return null;
                }
                for (float until = Time.time + 1; Time.time < until;) yield return null;
                taking = false;
                Say("laid");
                Debug.Log(string.Format(culture, "ACTION {0}: laid down in {1:0.0} s ({2} pictures)", who, Time.time - began - 1, shot));

                // It walks a little off, and is told to pick it up.
                Assert.That(unit.Motor.TryMove(OnGround(unit.transform.position - away * 1.6f)), Is.True);
                began = Time.time;
                while (unit.Motor.IsMoving && Time.time - began < 10) yield return null;
                for (float until = Time.time + .5f; Time.time < until;) yield return null;
                part = "take"; shot = 0; taking = true;
                Assert.That(click.OpenOn(thing) && click.Choose("Pick it up"), Is.True);
                began = Time.time; said = Time.time;
                while (Time.time - began < 25)
                {
                    var hands = unit.GetComponent<PhysicalHands>();
                    if (hands != null && look.Carrying && !look.Carry.Fetching && hands.Held != null && (hands.Holds(0) || hands.Holds(1))) break;
                    if (!look.Showing && Time.time - began > 1) break;
                    if (Time.time - said > 1) { said = Time.time; Say("taking"); }
                    yield return null;
                }
                float took = Time.time - began;
                for (float until = Time.time + 2.5f; Time.time < until;) yield return null;
                taking = false;
                Say("taken");
                Debug.Log(string.Format(culture, "ACTION {0}: taken up {1:0.0} s after the order ({2} pictures)", who, took, shot));
                after.Then = null;
                UnityEngine.Object.Destroy(after.gameObject);
                if (look.Showing) look.Toggle();
                look.SetStrength(1);
                look.SetWeight(1);
                for (float until = Time.time + .3f; Time.time < until;) yield return null;
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
