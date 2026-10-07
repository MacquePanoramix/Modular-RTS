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
    // The bench of S3's step 11: what the panel lets be tried, tried. Each miner, at several strengths, with each of
    // the panel's pickaxes: the pickaxe is put on the ground beside it, it is told to mine a boulder, and what it does
    // is written down (whether it picks the pickaxe up and how it then holds it, whether it keeps its feet, when it
    // comes to the rock, its blows, its rests), with pictures if they are asked for.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MinerPanelBench -panelOut <folder>
    //   [-panelMiner Round] [-panelStrengths 0.5,1,2] [-panelWeights 0.6,1,1.8] [-panelPickaxeOf Long] [-panelBoulder 4]
    //   [-panelFor 60] [-panelBlows 6] [-panelSize 0] [-panelView 250,12,3.2] [-panelOrder pick] [-panelPickAt 1] [-panelTrace 1] [-panelSend 0.4]
    public sealed class MinerPanelBench
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

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        [UnityTest, Explicit, Timeout(7200000)]
        public IEnumerator Bench()
        {
            string folder = CaptureTools.Argument("-panelOut") ?? Path.Combine(Application.dataPath, "..", "Captures", "Panel");
            string who = CaptureTools.Argument("-panelMiner"), of = CaptureTools.Argument("-panelPickaxeOf");
            float[] strengths = Numbers("-panelStrengths", .5f, 1, 2), weights = Numbers("-panelWeights", MinerPanel.Weights);
            float[] view = Numbers("-panelView", 250, 12, 3.2f);
            int size = (int)Numbers("-panelSize", 0)[0], blowsWanted = (int)Numbers("-panelBlows", 6)[0], which = (int)Numbers("-panelBoulder", 4)[0];
            float lasts = Numbers("-panelFor", 60)[0];
            var culture = CultureInfo.InvariantCulture;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            int miners = UnityEngine.Object.FindAnyObjectByType<MinerChoice>().Count;
            try
            {
                for (int index = 0; index < miners; index++)
                    foreach (float strength in strengths)
                        foreach (float weight in weights)
                        {
                            // Each try in a place loaded afresh: what one did is not the next one's.
                            Time.captureFramerate = 0;
                            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
                            yield return null;
                            Time.captureFramerate = 50;
                            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
                            string name = choice.NameOf(index);
                            if (!string.IsNullOrEmpty(who) && !string.Equals(who, name, StringComparison.OrdinalIgnoreCase)) continue;
                            var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
                            var click = UnityEngine.Object.FindAnyObjectByType<InteractionClick>();
                            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
                            var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
                            var camera = Camera.main;
                            var rig = camera.GetComponent<RtsCamera>();
                            if (rig != null) rig.enabled = false;
                            camera.nearClipPlane = .01f;
                            time.Hour = 13;
                            var boulder = Boulder.All()[which];
                            choice.Choose(index);
                            yield return null;
                            var unit = choice.Current;
                            var miner = unit.GetComponent<MinerBody>();
                            for (float until = Time.time + .5f; Time.time < until;) yield return null;
                            float tall = miner.Rig.head.position.y - unit.transform.position.y;
                            // A few steps from the boulder, on ground that can be walked.
                            Vector3 centre = boulder.Rock.bounds.center;
                            for (int k = 0; k < 12; k++)
                            {
                                Vector3 at = centre + Quaternion.Euler(0, k * 30, 0) * Vector3.forward * (Mathf.Max(boulder.Rock.bounds.extents.x, boulder.Rock.bounds.extents.z) + 2.5f);
                                if (!NavMesh.SamplePosition(at, out var walked, 1, NavMesh.AllAreas)) continue;
                                unit.Motor.Stop();
                                unit.GetComponent<NavMeshAgent>().Warp(walked.position);
                                unit.transform.rotation = Quaternion.LookRotation(Flat(centre - walked.position).normalized);
                                unit.GetComponent<ProceduralBiped>().ResetPose();
                                break;
                            }
                            for (float until = Time.time + .6f; Time.time < until;) yield return null;

                            // -panelPickAt: how strong it is until it has the pickaxe in its hands (then, as strong as the try says).
                            float pickAt = Numbers("-panelPickAt", strength)[0];
                            look.SetStrength(pickAt);
                            // The pickaxe: its own at a share of its weight, or another miner's own.
                            if (!string.IsNullOrEmpty(of))
                                for (int other = 0; other < choice.Count; other++)
                                {
                                    if (!string.Equals(of, choice.NameOf(other), StringComparison.OrdinalIgnoreCase)) continue;
                                    choice.Choose(other);
                                    yield return null;
                                    break;
                                }
                            var put = look.LayPickaxe(weight);
                            if (choice.Chosen != index) { choice.Choose(index); yield return null; yield return null; }
                            unit = choice.Current;
                            Assert.That(put, Is.Not.Null, "No pickaxe was put on the ground beside " + name);
                            float kilograms = put.GetComponent<Rigidbody>().mass;
                            string label = string.Format(culture, "{0}_s{1:0.00}_w{2:0.00}{3}", name.ToLowerInvariant(), strength, weight, string.IsNullOrEmpty(of) ? "" : "_of" + of.ToLowerInvariant());

                            int shot = 0, frame = 0;
                            bool taking = size > 0;
                            var after = new GameObject("After everything").AddComponent<AfterEverything>();
                            after.Then = () =>
                            {
                                if (!taking || frame++ % 2 != 0) return;
                                var fallen = unit.GetComponent<PhysicalFall>();
                                Vector3 body = fallen != null && fallen.Now != PhysicalFall.State.Up ? fallen.HipsAt : unit.transform.position + Vector3.up * tall * .5f;
                                // (Told only to pick the pickaxe up, the miner itself is what is looked at.)
                                Vector3 target = Vector3.Lerp(body, centre, CaptureTools.Argument("-panelOrder") == "pick" ? 0 : .25f);
                                Vector3 away = Vector3.ProjectOnPlane(unit.transform.position - centre, Vector3.up).normalized;
                                Vector3 dir = Quaternion.AngleAxis(view[0], Vector3.up) * -away;
                                dir = dir * Mathf.Cos(view[1] * Mathf.Deg2Rad) + Vector3.up * Mathf.Sin(view[1] * Mathf.Deg2Rad);
                                camera.transform.SetPositionAndRotation(target + dir * view[2], Quaternion.LookRotation(-dir));
                                CaptureTools.Render(camera, Path.Combine(folder, $"{label}_{shot:000}"), size, size);
                                shot++;
                            };
                            for (float until = Time.time + 1; Time.time < until;) yield return null;

                            // -panelOrder pick: it is only told to pick the pickaxe up, and then stands with it.
                            if (CaptureTools.Argument("-panelOrder") == "pick") Assert.That(click.OpenOn(put) && click.Choose("Pick it up"), Is.True, name + " was not offered to pick the pickaxe up");
                            else Assert.That(click.OpenOn(boulder) && click.Choose("Mine"), Is.True, name + " was not offered to mine the boulder");
                            float began = Time.time, picked = -1, atRock = -1, mostLegs = 0, legsFetching = 0, asks = 0, wayAt = -1, legsWorking = 0, kneesWorking = 0, kneesReady = 0, spotUp = -1, kneesPlanned = 0, bowPlanned = 0, leanPlanned = 0;
                            string way = "-", fell = "", doing = "";
                            int blows = boulder.Blows, stones = boulder.Stones.Count, falls = 0, rests = 0, ups = 0;
                            bool resting = false, restedGrounded = false, weakened = false;
                            var words = new List<string>();
                            while (boulder.Blows - blows < blowsWanted && Time.time - began < lasts)
                            {
                                float now = Time.time - began;
                                var hands = unit.GetComponent<PhysicalHands>();
                                var balance = unit.GetComponent<PhysicalBalance>();
                                var down = unit.GetComponent<PhysicalFall>();
                                bool fetching = look.Showing && look.Carrying && look.Carry.Fetching;
                                if (balance != null)
                                {
                                    mostLegs = Mathf.Max(mostLegs, balance.LegEffort);
                                    if (fetching) legsFetching = Mathf.Max(legsFetching, balance.LegEffort);
                                }
                                if (CaptureTools.Argument("-panelTrace") != null && balance != null && Time.frameCount % (int)Numbers("-panelTrace", 25)[0] == 0)
                                    Debug.Log(string.Format(culture, "PANEL_TRACE {0} {1:0.00}s {2}: sink {3:0.000}, its knees asked {4:0.00} (each holding half), legs {5:0}%, bow {6:0}, moving {7}, its legs {8:0}% fresh, its arms and back {9:0}% spent, {10} blows; on its left leg {11:0.00}; keeps its feet {12}, braced {13}, steps {14}, carrying {15}", label, now, doing, unit.GetComponent<ProceduralBiped>().SinkNow, balance.KneesAsked(hands != null ? hands.ToolMass : 0), balance.LegEffort * 100, unit.GetComponent<ProceduralBiped>().BowNow, unit.Motor.IsMoving, unit.GetComponent<PhysicalBody>().Fresh(PhysicalBody.Muscles.Legs) * 100, look.Swing != null ? look.Swing.Spent * 100 : 0, boulder.Blows - blows, balance.OnLeft, balance.KeepsFeet, balance.Braced, balance.Steps, look.Carrying));
                                if (picked < 0 && look.Showing && look.Carrying && !fetching && hands != null && hands.Held != null && (hands.Holds(0) || hands.Holds(1))) { picked = now; wayAt = now + 1.5f; }
                                // (As strong as the try says once it has stood up with its pickaxe: a body made weaker
                                // at the bottom of its squat cannot come up.)
                                if (picked >= 0 && !weakened && (look.Mining != null || (now > picked + 2.5f && unit.GetComponent<ProceduralBiped>().SinkNow < .02f))) { weakened = true; look.SetStrength(strength); }
                                if (wayAt > 0 && now >= wayAt && look.Carrying && way == "-")
                                {
                                    way = look.Carry.way == PhysicalCarry.Way.OneHand ? "one hand" : look.Carry.way == PhysicalCarry.Way.Dragged ? "by its end" : "left";
                                    asks = look.Carry.Asks;
                                }
                                if (atRock < 0 && look.AtRock) { atRock = now; spotUp = look.MiningPlan.height; kneesPlanned = look.MiningPlan.aimed.sink; bowPlanned = look.MiningPlan.aimed.bow; leanPlanned = look.MiningPlan.aimed.lean; }
                                if (balance != null && look.Swinging && look.AtRock && atRock >= 0 && now > atRock + 1.5f)
                                {
                                    legsWorking = Mathf.Max(legsWorking, balance.LegEffort);
                                    kneesWorking = Mathf.Max(kneesWorking, balance.KneesAsked(hands != null ? hands.ToolMass : 0));
                                    if (look.Swing.phase == PhysicalSwing.Phase.Ready) kneesReady = balance.KneesAsked(hands != null ? hands.ToolMass : 0);
                                }
                                if (down != null && down.Falls > falls)
                                {
                                    falls = down.Falls;
                                    fell += string.Format(culture, " [{0:0.0} s, {1}: {2}]", now, doing, balance != null ? balance.Fell : "?");
                                }
                                if (down != null) ups = down.GotUp;
                                bool rest = look.Swinging && look.Swing.phase == PhysicalSwing.Phase.Rest;
                                if (rest && !resting) { rests++; restedGrounded |= look.Swing.RestsOnGround; }
                                resting = rest;
                                doing = fetching ? "picking the pickaxe up" : look.Swinging ? "at its work" : look.Carrying ? (unit.Motor.IsMoving ? "walking with it" : "standing with it") : "with nothing";
                                string says = look.Status().Replace("\n", " / ");
                                if (words.Count == 0 || words[words.Count - 1] != says) { if (words.Count < 400) words.Add(says); }
                                yield return null;
                            }
                            float took = Time.time - began;
                            // -panelSend n: its blows done, it is sent a few steps off, n seconds after its last blow
                            // landed (whatever its swing is doing then), and watched.
                            float[] send = Numbers("-panelSend");
                            if (send.Length > 0 && look.Showing && look.AtRock)
                            {
                                for (float until = Time.time + send[0]; Time.time < until;) yield return null;
                                var plan = look.MiningPlan;
                                Vector3 to = plan.approach + Flat(plan.approach - centre).normalized * 3;
                                to.y = ground.Height(to.x, to.z);
                                string phase = look.Swing != null && look.Swinging ? look.Swing.phase.ToString() : "not swinging";
                                var fallen = unit.GetComponent<PhysicalFall>();
                                int fallsBefore = fallen != null ? fallen.Falls : 0;
                                bool went = unit.Motor.TryMove(to);
                                float sent = Time.time, mostLegsGoing = 0;
                                while (Time.time - sent < 14 && (unit.Motor.IsMoving || Time.time - sent < 1) && (fallen == null || fallen.Falls == fallsBefore))
                                {
                                    var legs = unit.GetComponent<PhysicalBalance>();
                                    if (legs != null) mostLegsGoing = Mathf.Max(mostLegsGoing, legs.LegEffort);
                                    yield return null;
                                }
                                var balanced = unit.GetComponent<PhysicalBalance>();
                                Debug.Log(string.Format(culture, "PANEL_SENT {0}: sent away {1:0.0} s after its last blow (its swing: {2}); the order was {3}; {4:0.0} s later it {5}, {6:0.00} m from where it was sent, {7} the walked ground; its legs were asked {8:0}% at most on the way{9}",
                                    label, send[0], phase, went ? "taken" : "refused", Time.time - sent, unit.Motor.IsMoving ? "is still going" : "stands",
                                    Flat(unit.transform.position - to).magnitude, unit.Motor.IsOff ? "off" : "on", mostLegsGoing * 100,
                                    fallen != null && fallen.Falls > fallsBefore ? "; IT FELL: " + (balanced != null ? balanced.Fell : "?") : ""));
                            }
                            for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                            taking = false;
                            after.Then = null;
                            UnityEngine.Object.Destroy(after.gameObject);
                            float energy = 0, speed = 0;
                            int struck = 0, swung = 0;
                            if (look.Swing != null)
                                foreach (var r in look.Swing.results)
                                {
                                    swung++;
                                    if (!r.struck) continue;
                                    struck++; energy += r.energy; speed += r.speed;
                                }
                            Debug.Log(string.Format(culture,
                                "PANEL_GRID {0}, strength {1:0.00}, pickaxe {2:0.00} kg (x{3:0.00}{4}): picked up {5}; held {6} ({7:0}% of its hold); legs at most {8:0}% picking it up, {9:0}% in all, {25:0}% at its work (each knee holding half: {30:0.00} at most, {31:0.00} ready to swing); at the rock {10} (its spot {26:0.00} m over the ground: knees bent {27:0.000} m, bow {28:0}, the tool leaning {29:0}); {11} blows on the rock in {12:0.0} s ({13} swings, {14} struck, {15:0.0} m/s and {16:0} J on average); {17} stones; rested {18} times{19}; fell {20} times{21}, got up {22}; at the end: {23} ({24} pictures)",
                                name, strength, kilograms, weight, string.IsNullOrEmpty(of) ? "" : ", " + of + "'s", picked < 0 ? "never" : picked.ToString("0.0", culture) + " s after the order", way, asks * 100,
                                legsFetching * 100, mostLegs * 100, atRock < 0 ? "never" : atRock.ToString("0.0", culture) + " s", boulder.Blows - blows, took, swung, struck,
                                struck > 0 ? speed / struck : 0, struck > 0 ? energy / struck : 0, boulder.Stones.Count - stones, rests, restedGrounded ? " (the head on the ground)" : "", falls, fell, ups,
                                look.Status().Replace("\n", " / "), shot, legsWorking * 100, spotUp, kneesPlanned, bowPlanned, leanPlanned, kneesWorking, kneesReady));
                            Debug.Log("PANEL_SAID " + label + ": " + string.Join(" | ", words.Count > 14 ? words.GetRange(words.Count - 14, 14) : words));
                            if (!string.IsNullOrEmpty(look.LeftRock)) Debug.Log("PANEL_LEFT " + label + ": " + look.LeftRock);
                        }
            }
            finally
            {
                Time.captureFramerate = 0;
            }
        }
    }
}
