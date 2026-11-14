using System;
using System.Collections;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace WonderGather.Tests
{
    // The drawn hip's reader (DrawnHip) and the thigh's true front (DrawnHip.TrueFront) posed where the posed cover grid
    // never put them (November 14, 2026, the review of (c)(c) step (b)): both legs; flexion below zero and past 150 (to
    // 175); adduction; twists to 80 either way; the hips lying on their back, upright, face down (as the getting up's
    // footB and pike have them, the hips' forward straight down) and turned every way at once. Each pose is set by the
    // atlas's poser (the solver's own angles, Kinematics.Forward) and read back three ways: from the drawn bones
    // (DrawnHip.Of), from the posed segments as the own body's links are framed (DrawnHip.OfLinks), and the fold as one
    // angle against the one the set angles make. The front against the solver's thigh x (the posed segments' forward).
    // HIP_POSE lines, one a leg a pose; HIP_POSE_WORST at the end. Fails past 0.5 degrees (the twist not counted at the
    // gimbal lock) or a front more than 1 degree off.
    public sealed class DrawnHipPoseTests
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static double R(double degrees) => degrees * Math.PI / 180;

        // bend, spread, twist, knee.
        private static readonly double[][] Legs =
        {
            new double[] { 0, 0, 0, 0 }, new double[] { -20, 0, 0, 10 }, new double[] { -40, 5, 0, 20 }, new double[] { -15, -20, 30, 5 },
            new double[] { 30, -25, 0, 40 }, new double[] { 60, 20, 50, 60 }, new double[] { 90, 0, -60, 90 }, new double[] { 120, 30, 30, 100 },
            new double[] { 150, 10, -20, 110 }, new double[] { 170, 25, -10, 100 }, new double[] { 175, -15, 40, 30 }, new double[] { 45, 60, 0, 0 },
            new double[] { 10, 0, 80, 45 }, new double[] { 100, -10, -80, 140 }, new double[] { 160, 0, 0, 2 }, new double[] { -30, 40, -45, 70 },
        };

        // yaw, pitch, roll of the hips (the solver's; pitch -90 lying on the back, 90 face down).
        private static readonly double[][] Hips = { new double[] { 0, -90, 0 }, new double[] { 0, 0, 0 }, new double[] { 0, 90, 0 }, new double[] { 30, 60, 20 }, new double[] { -50, 170, -35 } };

        [UnityTest, Explicit, Timeout(1800000)]
        public IEnumerator TheDrawnHipReadsEveryWay()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            var choice = Object.FindAnyObjectByType<MinerChoice>();
            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Small") index = i;
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner is called Small.");
            choice.Choose(index);
            for (int k = 0; k < 30; k++) yield return null;
            var unit = choice.Current;
            unit.SetSelected(false);
            var miner = unit.GetComponent<MinerBody>();
            var biped = unit.GetComponent<ProceduralBiped>();
            var agent = unit.GetComponent<NavMeshAgent>();
            unit.Motor.Stop();
            if (agent != null) agent.enabled = false;
            Vector3 spot = unit.transform.position + Vector3.up * 3;
            Quaternion facing = unit.transform.rotation;
            biped.LetGo = true;
            if (unit.TryGetComponent(out PhysicalBalance balance)) balance.Acts = false;
            if (unit.TryGetComponent(out MinerBreath breath)) breath.enabled = false;
            miner.Swell = 0; miner.Shrug = 0;
            var poser = unit.gameObject.AddComponent<AtlasPoser>();
            poser.Miner = miner;
            poser.Origin = spot;
            poser.Facing = facing;

            var b = BodyFrame.FromUnit(unit.gameObject);
            var stand = AnyBody.StanceFitter.Fit(b, AnyBody.StanceLibrary.Make(b, "stand"));
            var q = (double[])stand.Q.Clone();
            q[AnyBody.Pose.X] = q[AnyBody.Pose.Y] = q[AnyBody.Pose.Z] = 0;
            q[AnyBody.Pose.TFlex] = q[AnyBody.Pose.TLat] = q[AnyBody.Pose.TTw] = 0;
            for (int s = 0; s < 2; s++) { int o = 4 * s; q[AnyBody.Pose.ShFlexL + o] = 0; q[AnyBody.Pose.ShAbdL + o] = R(70); q[AnyBody.Pose.ShRotL + o] = 0; q[AnyBody.Pose.ElbL + o] = R(10); }

            var log = new StringBuilder();
            float worstDrawn = 0, worstLinks = 0, worstFold = 0, worstFront = 0, worstSplit = 0;
            string worstDrawnAt = "", worstFrontAt = "";
            int poses = 0, byPole = 0;
            for (int h = 0; h < Hips.Length; h++)
                for (int p = 0; p < Legs.Length; p++)
                {
                    var set = new[] { Legs[p], Legs[(p + 5) % Legs.Length] };
                    q[AnyBody.Pose.Yaw] = R(Hips[h][0]); q[AnyBody.Pose.Pitch] = R(Hips[h][1]); q[AnyBody.Pose.Roll] = R(Hips[h][2]);
                    for (int s = 0; s < 2; s++)
                    {
                        int o = 4 * s;
                        q[AnyBody.Pose.HFlexL + o] = R(set[s][0]); q[AnyBody.Pose.HAbdL + o] = R(set[s][1]);
                        q[AnyBody.Pose.HRotL + o] = R(set[s][2]); q[AnyBody.Pose.KneeL + o] = R(set[s][3]);
                    }
                    poser.Pose(b, q);
                    for (int k = 0; k < 3; k++) yield return null;
                    var solved = miner.Solved;
                    for (int s = 0; s < 2; s++)
                    {
                        var drawn = DrawnHip.Of(miner, s);
                        var links = DrawnHip.OfLinks(solved.pelvis, solved.thighs[s], solved.shins[s], s);
                        // The fold the set angles make: the thigh's line Ry(-flex) Rx(sg abd) (0, 0, -1) against the hips' -z.
                        double f = R(set[s][0]), a = R(set[s][1]);
                        double lineZ = -Math.Cos(f) * Math.Cos(a);
                        float foldSet = (float)(Math.Acos(Math.Max(-1, Math.Min(1, -lineZ))) * 180 / Math.PI);
                        DrawnHip.DrawnThigh(miner, s, out var line, out _);
                        float foldDrawn = DrawnHip.Fold(solved.pelvis.up, line);
                        float eDrawn = Off(drawn, set[s]), eLinks = Off(links, set[s]), eFold = Mathf.Abs(foldDrawn - foldSet);
                        Vector3 truth = Vector3.ProjectOnPlane(solved.thighs[s].forward, line).normalized;
                        Vector3 front = DrawnHip.TrueFront(miner, s, out bool byBend, out float split);
                        float eFront = Vector3.Angle(front, truth);
                        if (!byBend) byPole++;
                        if (!float.IsNaN(split)) worstSplit = Mathf.Max(worstSplit, split);
                        string at = $"hips {Hips[h][0]:0}/{Hips[h][1]:0}/{Hips[h][2]:0} leg {(s == 0 ? "L" : "R")} set {set[s][0]:0} {set[s][1]:0} {set[s][2]:0} {set[s][3]:0}";
                        log.AppendLine(string.Format(Inv, "HIP_POSE {0}: drawn {1:0.00} {2:0.00} {3:0.00} {4:0.00}{5}; links {6:0.00} {7:0.00} {8:0.00} {9:0.00}; fold set {10:0.00} drawn {11:0.00}; front {12:0.00} deg off, by {13}",
                            at, drawn.Flex, drawn.Abd, drawn.Twist, drawn.Knee, drawn.Locked ? " (locked)" : "", links.Flex, links.Abd, links.Twist, links.Knee, foldSet, foldDrawn, eFront, byBend ? "the bend plane" : "the drawing's pole"));
                        if (eDrawn > worstDrawn) { worstDrawn = eDrawn; worstDrawnAt = at; }
                        worstLinks = Mathf.Max(worstLinks, eLinks);
                        worstFold = Mathf.Max(worstFold, eFold);
                        if (eFront > worstFront) { worstFront = eFront; worstFrontAt = at; }
                        poses++;
                    }
                }
            log.AppendLine(string.Format(Inv, "HIP_POSE_WORST {0} leg poses: drawn {1:0.000} deg off ({2}), links {3:0.000}, fold {4:0.000}; front {5:0.000} deg off ({6}), {7} by the drawing's pole, the bend plane and the pole at most {8:0.000} apart",
                poses, worstDrawn, worstDrawnAt, worstLinks, worstFold, worstFront, worstFrontAt, byPole, worstSplit));
            Debug.Log(log.ToString());
            Object.Destroy(poser);
            Assert.That(worstDrawn, Is.LessThan(.5f), "The drawn reading is off: " + worstDrawnAt);
            Assert.That(worstLinks, Is.LessThan(.5f), "The links' reading is off.");
            Assert.That(worstFold, Is.LessThan(.5f), "The fold is off.");
            Assert.That(worstFront, Is.LessThan(1f), "The thigh's front is off: " + worstFrontAt);
        }

        // The most any angle is off what was set (each the nearest whole turn; the twist not told at the gimbal lock, nor
        // the bend alone there).
        private static float Off(DrawnHip.Read r, double[] set)
        {
            float e = Mathf.Max(Mathf.Abs(Mathf.DeltaAngle(r.Abd, (float)set[1])), Mathf.Abs(Mathf.DeltaAngle(r.Knee, (float)set[3])));
            if (!r.Locked) e = Mathf.Max(e, Mathf.Max(Mathf.Abs(Mathf.DeltaAngle(r.Flex, (float)set[0])), Mathf.Abs(Mathf.DeltaAngle(r.Twist, (float)set[2]))));
            return e;
        }
    }
}
