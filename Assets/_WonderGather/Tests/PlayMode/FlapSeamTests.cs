using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // November 14, 2026. MinerBody.FlapsOnTheirSeams (each flap swings on its seam) was made for the miner lying, sitting
    // and getting up (Luis, November 13: a leg seen detached while Small got up). Standing and walking, the flaps Luis has
    // seen must hang as they did. Here each miner, as the game starts it, stands, walks out, turns right round, walks
    // back and stands again; every frame its flaps are hung both ways from the same body: the way not drawn is worked out
    // beside the drawn one, from its own last state, and the drawn one is put back, so the body never knows. The angle
    // between the two ways is kept for every flap and frame, with the pelvis's roll at the worst. Bars set before the
    // run: standing, no flap more than 1 degree apart in any frame; walking and turning, no more than 2. Nothing of the
    // game is changed.
    // Run on its own: -testFilter WonderGather.Tests.FlapSeamTests
    [Explicit]
    public sealed class FlapSeamTests
    {
        private bool seamsWere = MinerBody.FlapsOnTheirSeams;

        [TearDown]
        public void End()
        {
            MinerBody.FlapsOnTheirSeams = seamsWere;
            Time.captureFramerate = 0;
        }

        [UnityTest, Timeout(1800000)]
        public IEnumerator StandingAndWalkingHangAsBefore()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = 100;
            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            Assert.That(choice, Is.Not.Null, "No miner to choose.");
            var log = new StringBuilder();
            var faults = new List<string>();
            foreach (string name in new[] { "Small", "Long", "Round" })
            {
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == name) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner is called " + name + ".");
                choice.Choose(index);
                for (int k = 0; k < 100; k++) yield return null;
                var unit = choice.Current;
                var miner = unit.GetComponent<MinerBody>();
                var look = unit.GetComponent<MinerWorkPreview>();
                log.AppendLine($"SEAM_MODE {name}: stands by its own {look != null && look.StandsByItsOwn}, gets up by its own {PhysicalFall.GetsUpByItsOwn}, solver turns as meant {OwnBody.TurnsAsMeant}; drawn with the seams {MinerBody.FlapsOnTheirSeams}, the other way worked out beside it");
                var shadow = unit.gameObject.AddComponent<FlapSeamShadow>();
                shadow.Miner = miner;
                Vector3 start = unit.transform.position, ahead = unit.transform.forward;
                Vector3 there = start + ahead * 8;
                if (NavMesh.SamplePosition(there, out var hit, 3, NavMesh.AllAreas)) there = hit.position;
                shadow.Phase = "standing";
                for (int k = 0; k < 500; k++) yield return null;
                shadow.Phase = "walking out";
                bool goes = new CommandDispatcher().Dispatch(new MoveCommand(there), unit.Motor);
                yield return Until(unit, there, 1500);
                float out1 = Flat(unit.transform.position - start);
                shadow.Phase = "turning round and walking back";
                bool comes = new CommandDispatcher().Dispatch(new MoveCommand(start), unit.Motor);
                yield return Until(unit, start, 1500);
                float back = Flat(unit.transform.position - start);
                shadow.Phase = "standing again";
                for (int k = 0; k < 300; k++) yield return null;
                log.AppendLine($"SEAM_COURSE {name}: sent out {goes} and back {comes}; out {F(out1)} m from the start, back within {F(back)} m");
                foreach (string phase in shadow.Order)
                {
                    var t = shadow.Phases[phase];
                    float bar = phase.StartsWith("standing") ? 1 : 2;
                    double mean = t.Frames > 0 ? t.Sum / (t.Frames * Mathf.Max(1, t.Flaps)) : 0;
                    log.AppendLine($"SEAM_SAME {name}, {phase}: {t.Frames} frames; the two ways at most {F(t.Most)} degrees apart (flap {t.MostFlap}, the pelvis rolled {F(t.MostRoll)} degrees then), on average {F((float)mean)}; frames with a flap over 1 degree apart {t.Over1}; bar {F(bar)}");
                    if (t.Most > bar) faults.Add($"{name} {phase}: {F(t.Most)} degrees");
                }
                UnityEngine.Object.Destroy(shadow);
                yield return null;
            }
            Debug.Log(log.ToString());
            Assert.That(faults, Is.Empty, "The flaps hang otherwise on their seams: " + string.Join("; ", faults));
        }

        private static IEnumerator Until(SelectableUnit unit, Vector3 goal, int frames)
        {
            for (int k = 0; k < frames && Flat(unit.transform.position - goal) > .6f; k++) yield return null;
            for (int k = 0; k < 150; k++) yield return null;
        }

        private static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;
        private static string F(float v) => v.ToString("0.00", CultureInfo.InvariantCulture);
    }

    // After the miner's body has hung its flaps (MinerBody, order 50), hangs them the other way from that way's own last
    // state, keeps the angle between the two, and puts the drawn way back as it was.
    [DefaultExecutionOrder(55)]
    internal sealed class FlapSeamShadow : MonoBehaviour
    {
        private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;

        internal sealed class Tally
        {
            public int Frames, Flaps, MostFlap = -1, Over1;
            public float Most, MostRoll;
            public double Sum;
        }

        public MinerBody Miner;
        public string Phase = "";
        public readonly List<string> Order = new List<string>();
        public readonly Dictionary<string, Tally> Phases = new Dictionary<string, Tally>();
        private Vector3[] other;

        private void LateUpdate()
        {
            if (Miner == null || !Miner.isActiveAndEnabled) return;
            var type = typeof(MinerBody);
            var flaps = (MinerBody.Flap[])type.GetField("flaps", Hidden).GetValue(Miner);
            var hangs = (Vector3[])type.GetField("flapHangs", Hidden).GetValue(Miner);
            var down = (Vector3[])type.GetField("flapDown", Hidden).GetValue(Miner);
            var across = (Vector3)type.GetField("pelvisAcross", Hidden).GetValue(Miner);
            int n = flaps.Length;
            if (n == 0 || hangs.Length != n || down.Length != n) return;
            if (other == null || other.Length != n) other = (Vector3[])hangs.Clone();
            var drawn = new Vector3[n];
            var turns = new Quaternion[n];
            var kept = (Vector3[])hangs.Clone();
            for (int k = 0; k < n; k++)
                if (flaps[k].bone != null) { drawn[k] = flaps[k].bone.rotation * down[k]; turns[k] = flaps[k].bone.localRotation; }
            bool was = MinerBody.FlapsOnTheirSeams;
            try
            {
                MinerBody.FlapsOnTheirSeams = !was;
                Array.Copy(other, hangs, n);
                type.GetMethod("Drape", Hidden).Invoke(Miner, null);
                if (!Phases.TryGetValue(Phase, out var tally)) { Phases[Phase] = tally = new Tally(); Order.Add(Phase); }
                tally.Frames++;
                tally.Flaps = n;
                float roll = Mathf.Asin(Mathf.Clamp(Vector3.Dot(Miner.Rig.pelvis.rotation * across, Vector3.up), -1, 1)) * Mathf.Rad2Deg;
                for (int k = 0; k < n; k++)
                {
                    if (flaps[k].bone == null) continue;
                    float apart = Vector3.Angle(drawn[k], flaps[k].bone.rotation * down[k]);
                    tally.Sum += apart;
                    if (apart > 1) tally.Over1++;
                    if (apart > tally.Most) { tally.Most = apart; tally.MostFlap = k; tally.MostRoll = roll; }
                }
                Array.Copy(hangs, other, n);
            }
            finally
            {
                MinerBody.FlapsOnTheirSeams = was;
                Array.Copy(kept, hangs, n);
                for (int k = 0; k < n; k++) if (flaps[k].bone != null) flaps[k].bone.localRotation = turns[k];
            }
        }
    }
}
