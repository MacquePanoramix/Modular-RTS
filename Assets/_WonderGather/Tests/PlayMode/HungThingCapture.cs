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
    // Pictures of S3's step 8, second half: a miner takes what hangs on it by a handle in its hand (Small its lantern,
    // Long its mug), walks a little with it, and hangs it back. The orders are given as the interaction click's box
    // gives them.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.HungThingCapture -hungOut <folder>
    //   [-hungMiner Small] [-hungView 215,8,1.7] [-hungAim .5] [-hungSize 360] [-hungEvery 2] [-hungHour 13]
    public sealed class HungThingCapture
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
            string folder = CaptureTools.Argument("-hungOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Hung");
            string who = CaptureTools.Argument("-hungMiner") ?? "Small";
            float[] view = Numbers("-hungView", 215, 8, 1.7f);
            float aim = Numbers("-hungAim", .5f)[0];
            int size = (int)Numbers("-hungSize", 360)[0], every = Mathf.Max(1, (int)Numbers("-hungEvery", 2)[0]);
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            Time.captureFramerate = 50;
            try
            {
                var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                var click = UnityEngine.Object.FindAnyObjectByType<InteractionClick>();
                var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                var camera = Camera.main;
                var rig = camera.GetComponent<RtsCamera>();
                if (rig != null) rig.enabled = false;
                camera.nearClipPlane = .01f;
                time.Hour = Numbers("-hungHour", 13)[0];
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
                for (float until = Time.time + .8f; Time.time < until;) yield return null;
                float height = miner.Rig.head.position.y - unit.transform.position.y;
                var has = ThingsInHand.Of(unit);
                Assert.That(has != null && has.Count > 0, Is.True, who + " has nothing a hand can take.");
                var thing = has.Thing(0);
                int k = thing.Index;

                int shot = 0, frame = 0;
                string part = "take";
                bool taking = false;
                var after = new GameObject("After everything").AddComponent<AfterEverything>();
                after.Then = () =>
                {
                    if (!taking || frame++ % every != 0) return;
                    Vector3 target = unit.transform.position + Vector3.up * height * aim;
                    Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * away;
                    dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                    camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                    CaptureTools.Render(camera, Path.Combine(folder, $"{who.ToLowerInvariant()}_{part}_{shot:000}"), size, size);
                    shot++;
                };
                void Say(string what)
                {
                    int hand = has.Hand;
                    string fingers = "";
                    if (hand >= 0)
                    {
                        miner.HandleIn(hand, miner.Things[k].grip, out var inFingers, out var along);
                        float angle = Vector3.Angle(miner.HangingBar(k), along);
                        fingers = string.Format(culture, "; handle {0:0.0} mm from the fingers, bar {1:0.0} deg askew, arm {2:0} mm short of straight, closed {3:0}%",
                            Vector3.Distance(miner.HangsFrom(k), inFingers) * 1000, Mathf.Min(angle, 180 - angle), (biped.ArmReach - Vector3.Distance(biped.ShoulderNow(hand), has.WristAsked)) * 1000, miner.Held(hand) * 100);
                    }
                    Debug.Log(string.Format(culture, "HUNG {0} {1:0.00}s {2}: {3}, in hand {4}, taken {5:0.00}, {6:0} mm from its hook, hangs {7:0.0} deg from down, {8:0.0} mm into the body{9}",
                        who, Time.time, what, has.Now, miner.InHand(k), miner.Taken(k), Vector3.Distance(miner.HangsFrom(k), miner.Hook(k)) * 1000,
                        Vector3.Angle(miner.HangingWay(k), Vector3.down), miner.HangingIntoBody(k) * 1000, fingers));
                }

                // Standing, then: take in hand.
                taking = true;
                for (float until = Time.time + .4f; Time.time < until;) yield return null;
                Say("hanging");
                Assert.That(click.OpenOn(thing) && click.Choose("Take in hand"), Is.True);
                float began = Time.time, said = Time.time;
                while (has.Now != ThingsInHand.Phase.Carried && Time.time - began < 6)
                {
                    if (Time.time - said > .2f) { said = Time.time; Say("taking"); }
                    yield return null;
                }
                for (float until = Time.time + 1; Time.time < until;) yield return null;
                Say("taken");
                taking = false;
                Debug.Log(string.Format(culture, "HUNG {0}: taken ({1} pictures)", who, shot));

                // It walks a little with it.
                part = "walk"; shot = 0; taking = true;
                Assert.That(unit.Motor.TryMove(OnGround(unit.transform.position + away * 2.2f)), Is.True);
                began = Time.time; said = Time.time;
                while ((unit.Motor.IsMoving || Time.time - began < .5f) && Time.time - began < 10)
                {
                    if (Time.time - said > .25f) { said = Time.time; Say("walking"); }
                    yield return null;
                }
                for (float until = Time.time + 1.2f; Time.time < until;) yield return null;
                Say("walked");
                taking = false;
                Debug.Log(string.Format(culture, "HUNG {0}: walked ({1} pictures)", who, shot));

                // It hangs it back.
                part = "back"; shot = 0; taking = true;
                Assert.That(click.OpenOn(thing) && click.Choose("Hang it back"), Is.True);
                began = Time.time; said = Time.time;
                while (has.Now != ThingsInHand.Phase.Hung && Time.time - began < 6)
                {
                    if (Time.time - said > .2f) { said = Time.time; Say("hanging back"); }
                    yield return null;
                }
                for (float until = Time.time + .8f; Time.time < until;) yield return null;
                Say("hung");
                taking = false;
                Debug.Log(string.Format(culture, "HUNG {0}: hung back ({1} pictures)", who, shot));
                after.Then = null;
                UnityEngine.Object.Destroy(after.gameObject);
                for (float until = Time.time + .2f; Time.time < until;) yield return null;
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
