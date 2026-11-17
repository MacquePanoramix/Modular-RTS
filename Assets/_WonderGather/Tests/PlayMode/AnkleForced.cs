using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WonderGather.Tests
{
    // Step 4a of the ankle build: the forced checks (Docs/Research/2026-11-15_NaturalRangesAndTheAnkle/research.md §7.2,
    // "Forced checks"; forced_checks.md beside it; November 17, 2026). Test code only: nothing in the game. Each check
    // makes its rare event happen, and says so.
    //
    // The rig (AnkleForcedTests.TheBootForced), a claim about this rig and not the game: each miner is given Small's ways
    // up (OwnBody.RiseWays, as -ankleWays Small does), pushed over and made down to get up by its own body; then its hips
    // are pinned (the root made immovable) and lifted 1.5 m, so that its boots touch nothing, and the riser's clock is held
    // (its time in the pose kept at nothing each step, OwnRiser.StuckAfter 0), so that it keeps its pose and never gives
    // up or hands over. The riser goes on writing every joint as it does; then, last of each step, the left boot's ask is
    // written again through the riser's own Aims (reflection), with the ask the check forces: the path the switches live
    // on, unchanged. Its shin is held by its own joints, the hips pinned.

    // (The riser's time in its pose kept at nothing, before it steps: it stays in its pose.)
    [DefaultExecutionOrder(-100)]
    internal sealed class RiserClockHeld : MonoBehaviour
    {
        public static readonly FieldInfo GotF = typeof(OwnRiser).GetField("got", BindingFlags.NonPublic | BindingFlags.Instance);
        public OwnRiser Riser;
        private void FixedUpdate() { if (Riser != null) GotF.SetValue(Riser, 0f); }
    }

    // What a check does in each step of the physics, after everything else of that step has been asked (after OwnBody,
    // 41, and the ankle watch, 30002), before the engine steps.
    [DefaultExecutionOrder(30003)]
    internal sealed class ForcedStep : MonoBehaviour
    {
        public Action<int> Do;
        public Func<bool> Lost;
        public int Steps, Step;
        public bool Done = true, WasLost;
        public void Begin(int steps, Action<int> act) { Steps = steps; Step = 0; Do = act; Done = false; WasLost = false; }
        private void FixedUpdate()
        {
            if (Done) return;
            if (Lost != null && Lost()) { WasLost = true; Done = true; return; }
            Do(Step);
            Step++;
            if (Step >= Steps) Done = true;
        }
    }

    // What a boot or its toe touched, by name, and how many steps it touched each (the rig's own listener).
    internal sealed class TouchNames : MonoBehaviour
    {
        public readonly Dictionary<string, int> Seen = new Dictionary<string, int>();
        private void Count(Collision touch)
        {
            string name = touch.collider != null ? touch.collider.name + (touch.collider.attachedArticulationBody != null ? "[" + touch.collider.attachedArticulationBody.name + "]" : "") : "?";
            Seen[name] = Seen.TryGetValue(name, out int n) ? n + 1 : 1;
        }
        private void OnCollisionEnter(Collision touch) => Count(touch);
        private void OnCollisionStay(Collision touch) => Count(touch);
        public string Said() { var parts = new List<string>(); foreach (var kv in Seen) parts.Add(kv.Key + " " + kv.Value); return parts.Count == 0 ? "nothing" : string.Join(", ", parts); }
    }

    public sealed class AnkleForcedTests
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly MethodInfo aimsM = typeof(OwnRiser).GetMethod("Aims", Hidden, null, new[] { typeof(int), typeof(Quaternion) }, null),
            driveM = typeof(OwnRiser).GetMethod("Drive", Hidden, null, new[] { typeof(int), typeof(float) }, null),
            goDownM = typeof(OwnBody).GetMethod("GoDown", Hidden);
        private static readonly FieldInfo toStoodF = typeof(OwnRiser).GetField("toStood", Hidden), restedF = typeof(OwnRiser).GetField("rested", Hidden),
            slackNowF = typeof(OwnRiser).GetField("slackNow", Hidden), lastStepF = typeof(OwnRiser).GetField("lastStep", Hidden),
            tallF = typeof(OwnRiser).GetField("tall", Hidden), lighterF = typeof(OwnRiser).GetField("lighter", Hidden),
            drivenNowF = typeof(OwnRiser).GetField("drivenNow", Hidden),
            bootShiftF = typeof(OwnBody).GetField("bootShift", Hidden), stoodAnkleF = typeof(OwnBody).GetField("stoodAnkle", Hidden);
        private const int Foot = OwnBody.Foot, Shin = OwnBody.Shin, Thigh = OwnBody.Thigh;

        private MinerChoice choice;
        private MinerWorkPreview look;
        private bool ownWas;
        private int rateWas;
        private float stuckWas;
        private OwnRiser.Way[] waysWas;
        private ForcedStep forcer;
        private RiserClockHeld clock;
        private AnkleWatch watch;
        private StreamWriter csv;

        [UnitySetUp]
        public IEnumerator Load()
        {
            rateWas = Time.captureFramerate;
            ownWas = PhysicalFall.GetsUpByItsOwn;
            waysWas = OwnBody.RiseWays;
            stuckWas = OwnRiser.StuckAfter;
            AnkleMendArguments.Save();
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
            forcer = new GameObject("Forced step").AddComponent<ForcedStep>();
            clock = new GameObject("Riser clock held").AddComponent<RiserClockHeld>();
        }

        [TearDown]
        public void Restore()
        {
            if (csv != null) { csv.Close(); csv = null; }
            if (forcer != null) Object.Destroy(forcer.gameObject);
            if (clock != null) Object.Destroy(clock.gameObject);
            if (watch != null) Object.Destroy(watch.gameObject);
            forcer = null; clock = null; watch = null;
            PhysicalFall.GetsUpByItsOwn = ownWas;
            if (waysWas != null) OwnBody.RiseWays = waysWas;
            OwnRiser.StuckAfter = stuckWas;
            Time.captureFramerate = rateWas;
            AnkleMendArguments.Restore();
        }

        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }
        private static string F(float x, string format = "0.000") => x.ToString(format, Inv);
        private static string V(Vector3 v, string format = "0.0") => "(" + F(v.x, format) + " " + F(v.y, format) + " " + F(v.z, format) + ")";

        // A turn as the riser writes it (degrees times axis, the angle in (-180, 180]), and back.
        private static Vector3 RV(Quaternion q)
        {
            q.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.right; angle = 0; }
            return angle * axis;
        }
        private static Quaternion FromRV(Vector3 by)
        {
            float angle = by.magnitude;
            return angle > 1e-6f ? Quaternion.AngleAxis(angle, by / angle) : Quaternion.identity;
        }
        // Today's form of an ankle's ask (Aims with AnkleCan, AnkleTurnsAtMost 0): what is written with every switch off.
        private static Vector3 TodayForm(Quaternion own, Quaternion stood, Vector3 driven)
        {
            own = Quaternion.Normalize(own);
            if (OwnRiser.AnkleAtMost > 0)
            {
                Quaternion fromStood = Quaternion.Inverse(stood) * own;
                fromStood.ToAngleAxis(out float far, out Vector3 about);
                if (far > 180) far -= 360;
                if (Mathf.Abs(far) > OwnRiser.AnkleAtMost && !float.IsNaN(about.x)) fromStood = Quaternion.AngleAxis(Mathf.Sign(far) * OwnRiser.AnkleAtMost, about);
                own = stood * fromStood;
            }
            return RV(own) + driven;
        }
        // B's writing, as research.md §4.2 has it: within E of the reading, E tanh(|e| / E), never wrapped. Degrees in and
        // out, the reading in radians.
        private static Vector3 Slowed(Vector3 byDegrees, Vector3 atRadians, float most)
        {
            Vector3 off = byDegrees * Mathf.Deg2Rad - atRadians;
            float far = off.magnitude;
            if (far > 1e-9f) off *= most * (float)Math.Tanh(far / most) / far;
            return (atRadians + off) * Mathf.Rad2Deg;
        }
        private static Vector3 Reading(ArticulationBody b)
        {
            var at = b.jointPosition;
            return at.dofCount >= 3 ? new Vector3(at[0], at[1], at[2]) : Vector3.zero;
        }
        private static Vector3 Speeds(ArticulationBody b)
        {
            var v = b.jointVelocity;
            return v.dofCount >= 3 ? new Vector3(v[0], v[1], v[2]) : Vector3.zero;
        }
        private static float PerAxis(Vector3 v) => Mathf.Max(Mathf.Abs(v.x), Mathf.Max(Mathf.Abs(v.y), Mathf.Abs(v.z)));
        private static Vector3 Target(ArticulationBody b) => new Vector3(b.xDrive.target, b.yDrive.target, b.zDrive.target);

        // A miner chosen, standing by its own joints; then pushed and made down to get up by its own body.
        private OwnBody own;
        private SelectableUnit unit;
        private IEnumerator Stand(string who)
        {
            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == who) index = i;
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner called " + who + ".");
            choice.Choose(index);
            yield return Wait(.3f);
            unit = choice.Current;
            string strengthArg = CaptureTools.Argument("-liveStrength");
            if (!string.IsNullOrEmpty(strengthArg)) look.SetStrength(float.Parse(strengthArg, Inv));
            look.SetOwn(true);
            own = null;
            for (float until = Time.time + 20; (own == null || !own.Stands) && Time.time < until;) { unit.TryGetComponent(out own); yield return null; }
            Assert.That(own != null && own.Stands, Is.True, who + " did not come to stand by its own joints.");
            for (float until = Time.time + 15; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
            yield return Wait(1.5f);
        }
        private IEnumerator PushedDown(string who)
        {
            for (float until = Time.time + 20; !look.CanPushOver && Time.time < until;) yield return null;
            Assert.That(look.CanPushOver, Is.True, who + " cannot be pushed over.");
            look.PushOver(unit.transform.forward);
            for (float until = Time.time + 40; own.Riser == null && Time.time < until;) yield return null;
        }
        private IEnumerator TakenBack()
        {
            var fall = unit.GetComponent<PhysicalFall>();
            if (own.Stands) own.Drop();
            if (fall != null) fall.TakeBack();
            for (float until = Time.time + 20; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
        }

        // ---- Checks 1, 2, 4, 5 and 6: the boot forced, on the rig ----

        private sealed class Prof
        {
            public string Name;
            public int Steps;
            public Func<int, Quaternion> Ask;          // the ankle's ask (a turn from how it was made, its own frame) at step i
            public Func<int, bool> Window;             // the steps of the crossing (check 1, 2): null, none
            public int FollowAt = -1;                  // the step at which the boot must be where it is asked (the lazy way's catch)
            public Action<int> Before, After;          // the rig's own writes, before and after the ask (check 5)
            public int[] TanhAt;                       // steps at which B's writing is compared with its formula (check 4)
            public bool Axes;                          // check 6: read at the end of each hold
            public int AxesHold;
        }

        private ArticulationBody boot, toe, shin;
        private BootTouch bootTouch, toeTouch;
        private TouchNames bootNames, toeNames;
        private string whoNow = "", configNow = "";
        private static readonly MethodInfo foldsM = typeof(OwnRiser).GetMethod("AnkleFolds", Hidden, null, new[] { typeof(int), typeof(Quaternion) }, null);
        private OwnRiser riser;
        private Quaternion stood;
        private Quaternion[] rested;
        private float tall, height;
        private Vector3 twistAxis, swingAxis, obliqueAxis;

        private float OmegaOf(float ankleSpeed) => ankleSpeed * Mathf.Sqrt(OwnRiser.AnkleSpeedHeight / tall);

        private IEnumerator Settle(int steps)
        {
            forcer.Lost = () => own == null || own.Riser != riser || !own.Stands;
            forcer.Begin(steps, i => aimsM.Invoke(riser, new object[] { Foot, stood }));
            while (!forcer.Done) yield return null;
        }

        private readonly List<string> results = new List<string>();
        private int profilesRun, profilesLost;

        private IEnumerator Run(string who, string config, Prof p, float omegaBar)
        {
            yield return Settle(25);
            if (forcer.WasLost) { profilesLost++; Debug.Log($"FORCED_LOST {who} {config} {p.Name}: the riser was lost while settling"); yield break; }
            // (What it reads, step by step.)
            float fastFree = 0, fastTouched = 0, fastVec = 0, shinFast = 0, jumpTurnMost = 0, jumpWrittenMost = 0, winTurnMost = 0, winAskAt = 0, askTurnMost = 0, offEMost = 0, followErr = float.NaN;
            int fastAt = -1, jumps60 = 0, winViolations = 0, winSteps = 0, touchedSteps = 0, tanhChecks = 0, tanhFails = 0, clampWouldFail = 0;
            float tanhErrMost = 0, clampDiffLeast = float.PositiveInfinity;
            Vector3 lastT = Vector3.zero; Quaternion lastAsk = Quaternion.identity; bool first = true;
            var axesLines = new List<string>();
            float rvErrMost = 0, readErrMost = 0, permErrLeast = float.PositiveInfinity, eulerErrMost = 0;
            var tanhLines = new StringBuilder();
            float followAsk = float.NaN;
            float preJumpMost = 0, formErrMost = 0; int formChecks = 0, formFails = 0; Vector3 lastPre = Vector3.zero, pre = Vector3.zero;
            bool limits = OwnRiser.AnkleLimit == 1, slows = OwnRiser.AnkleSpeed > 0;
            bootNames.Seen.Clear(); if (toeNames != null) toeNames.Seen.Clear();
            forcer.Lost = () => own == null || own.Riser != riser || !own.Stands;
            forcer.Begin(p.Steps, i =>
            {
                // (The step just taken: the boot's joint speed per axis, its shin, its touches.)
                Vector3 v = Speeds(boot);
                bool touched = (bootTouch != null && bootTouch.Touched) || (toeTouch != null && toeTouch.Touched);
                if (bootTouch != null) bootTouch.Touched = false;
                if (toeTouch != null) toeTouch.Touched = false;
                if (i > 0)
                {
                    float fast = PerAxis(v);
                    if (touched) { touchedSteps++; fastTouched = Mathf.Max(fastTouched, fast); }
                    else if (fast > fastFree) { fastFree = fast; fastAt = i; fastVec = v.magnitude; }
                    shinFast = Mathf.Max(shinFast, shin.angularVelocity.magnitude);
                }
                p.Before?.Invoke(i);
                Quaternion ask = p.Ask(i);
                aimsM.Invoke(riser, new object[] { Foot, ask });
                // (The form before B: A's fold of the ask (with AnkleLimit 1), today's clamp of it (0); and the target
                // written against it, slowed as B slows it when B is on: every step, the written form read back.)
                Vector3 driven = ((Vector3[])drivenNowF.GetValue(riser))[Foot];
                Vector3 r = Reading(boot);
                var drive = boot.xDrive;
                float most = drive.stiffness > 0 ? drive.forceLimit / drive.stiffness : float.NaN;
                pre = limits ? RV((Quaternion)foldsM.Invoke(riser, new object[] { 0, Quaternion.Normalize(ask) * FromRV(driven) })) : TodayForm(ask, stood, driven);
                {
                    Vector3 expect = slows && !float.IsNaN(most) ? Slowed(pre, r, most) : pre;
                    float err = (Target(boot) - expect).magnitude;
                    formChecks++; if (err > .05f) formFails++; formErrMost = Mathf.Max(formErrMost, err);
                }
                p.After?.Invoke(i);
                // (What was written for the step to come, and what the drive holds.)
                Vector3 T = Target(boot);
                if (!first)
                {
                    float jumpW = (T - lastT).magnitude, jumpT = Quaternion.Angle(FromRV(lastT), FromRV(T)), askT = Quaternion.Angle(lastAsk, ask);
                    jumpWrittenMost = Mathf.Max(jumpWrittenMost, jumpW); jumpTurnMost = Mathf.Max(jumpTurnMost, jumpT); askTurnMost = Mathf.Max(askTurnMost, askT);
                    if (jumpT > 60 || jumpW > 60) jumps60++;
                    float preJ = Quaternion.Angle(FromRV(lastPre), FromRV(pre));
                    preJumpMost = Mathf.Max(preJumpMost, preJ);
                    if (p.Window != null && p.Window(i))
                    {
                        winSteps++;
                        if (preJ > winTurnMost) { winTurnMost = preJ; winAskAt = askT; }
                        if (preJ > 3.7f * askT + 1) winViolations++;
                    }
                }
                first = false; lastT = T; lastAsk = ask; lastPre = pre;
                if (!float.IsNaN(most)) offEMost = Mathf.Max(offEMost, (T * Mathf.Deg2Rad - r).magnitude / most);
                if (i == p.FollowAt)
                {
                    Quaternion actual = Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation;
                    followAsk = Quaternion.Angle(actual, Quaternion.Normalize(ask));
                    followErr = Quaternion.Angle(actual, FromRV(T - driven));
                }
                if (p.TanhAt != null && Array.IndexOf(p.TanhAt, i) >= 0 && !float.IsNaN(most))
                {
                    Vector3 expect = Slowed(pre, r, most);
                    float m = (pre * Mathf.Deg2Rad - r).magnitude / most;
                    // (The lazy form a check must tell from B's: a hard clamp at E.)
                    Vector3 off = pre * Mathf.Deg2Rad - r; float far = off.magnitude;
                    Vector3 clamp = (r + (far > most ? off * (most / far) : off)) * Mathf.Rad2Deg;
                    float err = (T - expect).magnitude, clampDiff = (T - clamp).magnitude;
                    tanhChecks++;
                    if (err > .05f) tanhFails++;
                    tanhErrMost = Mathf.Max(tanhErrMost, err);
                    if (m >= .99f) { clampDiffLeast = Mathf.Min(clampDiffLeast, clampDiff); if (clampDiff <= .05f) clampWouldFail++; }
                    tanhLines.AppendFormat(Inv, "[e/E {0:0.000}: written {1:0.000} E from the reading, tanh {2:0.000}, B's form {3:0.0000} deg away, a hard clamp's {4:0.000} deg away] ",
                        m, (T * Mathf.Deg2Rad - r).magnitude / most, Math.Tanh(m), err, clampDiff);
                }
                if (p.Axes && (i + 1) % p.AxesHold == 0)
                {
                    Quaternion actual = Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation;
                    Vector3 got = RV(actual), readDeg = r * Mathf.Rad2Deg;
                    float rvErr = (got - T).magnitude, readErr = (readDeg - T).magnitude, perm = (got - new Vector3(T.y, T.x, T.z)).magnitude,
                        eul = Quaternion.Angle(actual, Quaternion.Euler(T)), readVsGot = (readDeg - got).magnitude;
                    rvErrMost = Mathf.Max(rvErrMost, rvErr); readErrMost = Mathf.Max(readErrMost, readErr); eulerErrMost = Mathf.Max(eulerErrMost, eul);
                    if (T.magnitude > 1) permErrLeast = Mathf.Min(permErrLeast, Mathf.Abs(T.x - T.y) > 1 ? perm : float.PositiveInfinity);
                    axesLines.Add(string.Format(Inv, "[written {0} deg; the joint reads {1} deg; the boot is turned {2} deg (as a turn, from how it was made): {3:0.00} from the written, the reading {4:0.00} from it, {5:0.00} from the written read as Euler angles, {6:0.00} from it with X and Y swapped]",
                        V(T, "0.00"), V(readDeg, "0.00"), V(got, "0.00"), rvErr, readVsGot, eul, perm));
                }
                if (csv != null)
                    csv.WriteLine(string.Join(",", who, config, p.Name, i.ToString(Inv), F(T.x, "0.###"), F(T.y, "0.###"), F(T.z, "0.###"), F(r.x * Mathf.Rad2Deg, "0.###"), F(r.y * Mathf.Rad2Deg, "0.###"), F(r.z * Mathf.Rad2Deg, "0.###"),
                        F(v.x, "0.###"), F(v.y, "0.###"), F(v.z, "0.###"), touched ? "1" : "0", F(drive.stiffness, "0.####"), F(drive.damping, "0.####"), F(drive.forceLimit, "0.####")));
            });
            while (!forcer.Done) yield return null;
            bool ended = !forcer.WasLost && forcer.Step == p.Steps;
            if (!ended) profilesLost++; else profilesRun++;
            var d = boot.xDrive;
            var sb = new StringBuilder();
            sb.AppendFormat(Inv, "FORCED {0} {1} {2}: {3} of {4} steps, ended by itself {5}; switches read back AnkleLimit {6}, AnkleSpeed {7} (Omega here {8:0.00} rad/s), BootsFromOwnShin {9}; the drive holds k {10:0.000} c {11:0.0000} limit {12:0.000} (E {13:0.0} deg); ",
                who, config, p.Name, forcer.Step, p.Steps, ended, F(OwnRiser.AnkleLimit, "0.######"), F(OwnRiser.AnkleSpeed, "0.######"), OwnRiser.AnkleSpeed != 0 ? OmegaOf(OwnRiser.AnkleSpeed) : 0, OwnBody.BootsFromOwnShin,
                d.stiffness, d.damping, d.forceLimit, d.stiffness > 0 ? d.forceLimit / d.stiffness * Mathf.Rad2Deg : float.NaN);
            sb.AppendFormat(Inv, "the ask moved at most {0:0.0} deg in a step; the target written moved at most {1:0.0} deg as a turn ({2:0.0} as written), {3} steps over 60; ", askTurnMost, jumpTurnMost, jumpWrittenMost, jumps60);
            sb.AppendFormat(Inv, "the boot fastest touching nothing {0:0.0} rad/s on an axis at step {1} ({2:0.0} as a vector), touching {3:0.0} ({4} steps touched); its shin at most {5:0.0} rad/s; written at most {6:0.000} E from the reading; ",
                fastFree, fastAt, fastVec, fastTouched, touchedSteps, shinFast, offEMost);
            if (!float.IsNaN(followErr)) sb.AppendFormat(Inv, "at step {0} the boot {1:0.0} deg from the target written, {2:0.0} from its ask; ", p.FollowAt, followErr, followAsk);
            sb.AppendFormat(Inv, "the form before B (A's fold, or today's clamp) moved at most {0:0.0} deg in a step; the written target against that form{1}: {2} steps, {3} more than 0.05 deg off (most {4:0.0000}); the boot touched: {5}; its toe touched: {6}; ",
                preJumpMost, slows ? " slowed as B slows it" : "", formChecks, formFails, formErrMost, bootNames.Said(), toeNames != null ? toeNames.Said() : "(no toe)");
            string barA = "n/a", barB, barF = float.IsNaN(followErr) ? "n/a" : followErr <= 5 ? "PASS" : "FAIL";
            if (p.Window != null)
            {
                sb.AppendFormat(Inv, "at the crossing ({0} steps): the form before B moved at most {1:0.0} deg in a step (the ask {2:0.0} then), {3} steps moving more than 3.7 times the ask and 1 deg; ", winSteps, winTurnMost, winAskAt, winViolations);
                barA = winViolations == 0 ? "PASS" : "FAIL";
                sb.AppendFormat(Inv, "BAR_A (no jump at the crossing) {0}; ", barA);
            }
            float fastAll = Mathf.Max(fastFree, fastTouched);
            barB = p.Name == "beyond" ? "n/a" : fastAll <= omegaBar ? "PASS" : "FAIL";
            sb.AppendFormat(Inv, "BAR_B (no faster than {0:0.00} rad/s on an axis, touching or not) {1}", omegaBar, barB);
            Debug.Log(string.Format(Inv, "FORCED_BARS {0} {1} {2}: ended by itself {3}; A {4}; B {5} (fastest {6:0.0} rad/s, touching nothing {7:0.0}, bound {8:0.00}); follow {9} ({10:0.0} deg from the target, {19:0.0} from the ask); form {11} ({12} steps, most {13:0.0000} deg off); tanh {14}; axes {15}; the target's largest step {16:0.0} deg as a turn, the form before B's {17:0.0}; touched {18} steps",
                who, config, p.Name, ended, barA, barB, fastAll, fastFree, omegaBar, barF, followErr, formFails == 0 ? "PASS" : "FAIL", formChecks, formErrMost,
                p.TanhAt == null ? "n/a" : tanhChecks > 0 && tanhFails == 0 ? "PASS" : "FAIL", !p.Axes ? "n/a" : rvErrMost <= 2 && readErrMost <= 2 ? "PASS" : "FAIL", jumpTurnMost, preJumpMost, touchedSteps, followAsk));
            if (!float.IsNaN(followErr)) sb.AppendFormat(Inv, "; BAR_FOLLOW (within 5 deg of the target written at step {0}) {1}", p.FollowAt, followErr <= 5 ? "PASS" : "FAIL");
            if (p.TanhAt != null)
                sb.AppendFormat(Inv, "; TANH {0} compared, {1} more than 0.05 deg from B's form (most {2:0.0000}); a hard clamp at E would be {3:0.000} deg away at least where e >= E ({4} within 0.05): BAR_TANH {5}; {6}",
                    tanhChecks, tanhFails, tanhErrMost, clampDiffLeast, clampWouldFail, tanhChecks > 0 && tanhFails == 0 ? "PASS" : "FAIL", tanhLines);
            if (p.Axes)
                sb.AppendFormat(Inv, "; AXES {0} holds: the boot's turn at most {1:0.00} deg from the written target read as a turn, the joint's reading at most {2:0.00} from it; read as Euler angles at most {3:0.00}; with X and Y swapped at least {4:0.00}: BAR_AXES {5}; {6}",
                    axesLines.Count, rvErrMost, readErrMost, eulerErrMost, permErrLeast, rvErrMost <= 2 && readErrMost <= 2 ? "PASS" : "FAIL", string.Join(" ", axesLines));
            Debug.Log(sb.ToString());
            results.Add(sb.ToString());
        }

        // Check 5: the boot whirled past half a turn with its drive off, then its drive given back and asked again
        // (B writes the reading plus e', which can be outside [-180, 180]); then targets outside written raw.
        private Prof Beyond()
        {
            var lines = new StringBuilder();
            float readMost = float.NegativeInfinity; int wraps = 0; float lastRead = float.NaN, targetMost = 0; int outside = 0;
            Vector3 endHold = Vector3.zero, endRaw1 = Vector3.zero, endRaw2 = Vector3.zero, gotHold = Vector3.zero, gotRaw1 = Vector3.zero, gotRaw2 = Vector3.zero;
            // (Then, from the joint brought to 150 degrees on its first axis by a raw target, raw targets a little past
            // half a turn, as B may write from a reading inside it: 190, then 210, each held 65 steps.)
            var past = new StringBuilder();
            float[] rawAt = { 150, 190, 210 };
            var p = new Prof { Name = "beyond", Steps = 460 };
            p.Ask = i => i >= 50 && i < 110 ? FromRV(new Vector3(170, 0, 0)) : stood;
            p.Before = i =>
            {
                if (i == 10)
                    foreach (var which in new[] { 0, 1, 2 })
                    {
                        var dr = which == 0 ? boot.xDrive : which == 1 ? boot.yDrive : boot.zDrive;
                        dr.stiffness = 0; dr.damping = 0;
                        if (which == 0) boot.xDrive = dr; else if (which == 1) boot.yDrive = dr; else boot.zDrive = dr;
                    }
                if (i >= 10 && i < 50)
                {
                    float x = Reading(boot).x;
                    if (!float.IsNaN(lastRead) && Mathf.Abs(x - lastRead) > Mathf.PI) wraps++;
                    lastRead = x; readMost = Mathf.Max(readMost, x);
                    if (x < 1.4f * Mathf.PI) { var w = boot.jointVelocity; w[0] = 30; w[1] = 0; w[2] = 0; boot.jointVelocity = w; }
                }
                if (i == 50)
                {
                    float x = Reading(boot).x; readMost = Mathf.Max(readMost, x);
                    lines.AppendFormat(Inv, "whirled with its drive off at 30 rad/s about the joint's first axis: the reading went to at most {0:0.0} deg, {1} times it jumped by more than half a turn in a step (a wrap); at the drive's return it reads {2} deg; ",
                        readMost * Mathf.Rad2Deg, wraps, V(Reading(boot) * Mathf.Rad2Deg));
                    float share = ((float[])slackNowF.GetValue(riser))[Foot];
                    driveM.Invoke(riser, new object[] { Foot, share });
                }
            };
            p.After = i =>
            {
                Vector3 T = Target(boot);
                if (i >= 50 && i < 110)
                {
                    float m = PerAxis(T); targetMost = Mathf.Max(targetMost, m); if (m > 180) outside++;
                    if (i == 51) lines.AppendFormat(Inv, "asked (170 0 0) deg (from how it was made), the first target written {0} deg; ", V(T));
                }
                if (i == 109) { endHold = Reading(boot) * Mathf.Rad2Deg; gotHold = RV(Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation); }
                if (i >= 135 && i < 200) { boot.SetDriveTarget(ArticulationDriveAxis.X, 250); boot.SetDriveTarget(ArticulationDriveAxis.Y, 0); boot.SetDriveTarget(ArticulationDriveAxis.Z, 0); }
                if (i == 199) { endRaw1 = Reading(boot) * Mathf.Rad2Deg; gotRaw1 = RV(Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation); }
                if (i >= 200 && i < 265) { boot.SetDriveTarget(ArticulationDriveAxis.X, -250); boot.SetDriveTarget(ArticulationDriveAxis.Y, 0); boot.SetDriveTarget(ArticulationDriveAxis.Z, 0); }
                if (i == 264)
                {
                    endRaw2 = Reading(boot) * Mathf.Rad2Deg; gotRaw2 = RV(Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation);
                    lines.AppendFormat(Inv, "in the 60 steps so asked the target written reached {0:0.0} deg on an axis, {1} steps outside [-180, 180]; after them the joint reads {2} deg, the boot turned {3} deg; ", targetMost, outside, V(endHold), V(gotHold));
                    lines.AppendFormat(Inv, "written raw (250 0 0) for 65 steps: the joint reads {0} deg, the boot turned {1} deg; written raw (-250 0 0) for 65 steps: the joint reads {2} deg, the boot turned {3} deg", V(endRaw1), V(gotRaw1), V(endRaw2), V(gotRaw2));
                }
                if (i >= 265)
                {
                    int k = (i - 265) / 65;
                    boot.SetDriveTarget(ArticulationDriveAxis.X, rawAt[k]); boot.SetDriveTarget(ArticulationDriveAxis.Y, 0); boot.SetDriveTarget(ArticulationDriveAxis.Z, 0);
                    if ((i - 265) % 65 == 64)
                        past.AppendFormat(Inv, "; written raw ({0:0} 0 0) for 65 steps: the joint reads {1} deg, the boot turned {2} deg, its joint at {3:0.00} rad/s on an axis at the end",
                            rawAt[k], V(Reading(boot) * Mathf.Rad2Deg), V(RV(Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation)), PerAxis(Speeds(boot)));
                    if (i == 459) Debug.Log("FORCED_BEYOND " + whoNow + " " + configNow + ": " + lines + past);
                }
            };
            return p;
        }

        private List<Prof> Profiles(string config)
        {
            var list = new List<Prof>();
            Quaternion At(Vector3 axis, float degrees) => stood * Quaternion.AngleAxis(degrees, axis);
            // (Held at nothing, at 60, then at 179, then at 181: the crossing in one step of 2 degrees.)
            Prof Instant(string name, Vector3 axis) => new Prof
            {
                Name = name, Steps = 85, FollowAt = 34, Window = i => i >= 60,
                Ask = i => At(axis, i < 10 ? 0 : i < 35 ? 60 : i < 60 ? 179 : 181),
            };
            // (Held at nothing, at 60, then turned at so many degrees a second to 300, and held.)
            Prof Ramp(string name, Vector3 axis, float perSecond)
            {
                int turning = Mathf.CeilToInt(240 / (perSecond * .02f));
                float Angle(int i) => i < 10 ? 0 : i < 35 ? 60 : Mathf.Min(300, 60 + perSecond * .02f * (i - 35));
                return new Prof
                {
                    Name = name, Steps = 35 + turning + 25, FollowAt = 34,
                    Window = i => { float a = Angle(i); float before = Angle(Mathf.Max(0, i - 10)); return (a >= 150 && a <= 210) || (before >= 150 && before <= 210); },
                    Ask = i => At(axis, Angle(i)),
                };
            }
            if (config != "edge")
            {
                list.Add(Instant("swing_instant", swingAxis));
                list.Add(Ramp("swing_90dps", swingAxis, 90));
                list.Add(Ramp("swing_360dps", swingAxis, 360));
                list.Add(Ramp("swing_1800dps", swingAxis, 1800));
                list.Add(Ramp("oblique_360dps", obliqueAxis, 360));
                list.Add(Instant("twist_instant", twistAxis));
                list.Add(Ramp("twist_360dps", twistAxis, 360));
                // (The Slerp from aimFrom, as OwnRiser.Step has it at a pose's middle: from how it stands to a pose turned
                // 170 to 190 degrees from it, a degree every three steps; its shortest way flips at 180.)
                float Dest(int i) => i < 10 ? 170 : Mathf.Min(190, 170 + 20f * (i - 10) / 60);
                list.Add(new Prof
                {
                    Name = "slerp_antipode", Steps = 95,
                    Window = i => { float a = Dest(i), b = Dest(Mathf.Max(0, i - 10)); return (a >= 176 && a <= 184) || (b >= 176 && b <= 184); },
                    Ask = i => i < 5 ? stood : stood * Quaternion.Slerp(Quaternion.identity, Quaternion.AngleAxis(Dest(i), swingAxis), .5f),
                });
            }
            else
            {
                list.Add(Instant("swing_instant", swingAxis));
                list.Add(Ramp("swing_360dps", swingAxis, 360));
            }
            if (config == "off")
            {
                // (Check 6: written targets about one axis, two and three, each held 35 steps.)
                var asked = new[] { new Vector3(40, 0, 0), new Vector3(0, 40, 0), new Vector3(0, 0, 40), new Vector3(30, 30, 0), new Vector3(30, 0, 30), new Vector3(0, 30, 30), new Vector3(30, 30, 30), new Vector3(-50, 20, -30), new Vector3(70, -40, 20) };
                Vector3 from = RV(stood);
                list.Add(new Prof { Name = "axes", Steps = 35 * asked.Length, Axes = true, AxesHold = 35, Ask = i => FromRV(from + asked[i / 35]) });
            }
            if (config == "B" || config == "edge")
            {
                // (Check 4 (B alone, so that what is slowed is today's form of the ask): asks so far from where the joint is, in shares of E, one step each, then back.)
                var shares = new[] { .25f, .5f, 1, 1.5f, 2, -.25f, -.5f, -1, -1.5f, -2 };
                var at = new int[shares.Length];
                for (int k = 0; k < shares.Length; k++) at[k] = 10 + 21 * k;
                Vector3 askedNow = Vector3.zero;
                var p = new Prof { Name = "tanh", Steps = 10 + 21 * shares.Length, TanhAt = at };
                p.Before = i =>
                {
                    int k = Array.IndexOf(at, i);
                    if (k < 0) return;
                    var d = boot.xDrive;
                    float most = d.forceLimit / d.stiffness * Mathf.Rad2Deg;
                    Vector3 r = Reading(boot) * Mathf.Rad2Deg;
                    // (Along the joint's first axis; no further than 170 degrees as written, so that it is not wrapped.)
                    float want = shares[k] * most;
                    want = Mathf.Clamp(r.x + want, -170, 170) - r.x;
                    askedNow = r + new Vector3(want, 0, 0);
                };
                p.Ask = i => Array.IndexOf(at, i) >= 0 ? FromRV(askedNow) : stood;
                list.Add(p);
            }
            if (config == "off" || config == "B" || config == "AB") list.Add(Beyond());
            return list;
        }

        // Checks 1, 2, 4, 5 and 6 on the rig, for each miner (-liveWho; all three if left out) and each set of switches
        // (-forcedConfigs, of off, A, B, AB, edge; all five if left out).
        [UnityTest, Explicit, Timeout(7200000)]
        public IEnumerator TheBootForced()
        {
            Assert.That(aimsM != null && foldsM != null && driveM != null && toStoodF != null && restedF != null && slackNowF != null && lastStepF != null && tallF != null && lighterF != null && drivenNowF != null && RiserClockHeld.GotF != null,
                Is.True, "OwnRiser lacks what the forced checks reach for.");
            Time.captureFramerate = 50;
            PhysicalFall.GetsUpByItsOwn = true;
            LogAssert.ignoreFailingMessages = true;
            Debug.Log("FORCED_SET " + AnkleMendArguments.Apply());
            var text = Resources.Load<TextAsset>("RiseWays/Small");
            Assert.That(text, Is.Not.Null, "No Resources/RiseWays/Small.txt.");
            OwnBody.RiseWays = OwnRiser.Read(text.text);
            OwnRiser.StuckAfter = 0;
            Debug.Log("FORCED_RIG every miner given Small's ways up (" + OwnBody.RiseWays.Length + " ways), OwnRiser.StuckAfter 0, the riser's clock held, the hips pinned 1.5 m up: a rig, not the game");
            string folder = CaptureTools.Argument("-forcedOut");
            if (string.IsNullOrEmpty(folder)) folder = Path.Combine(Application.dataPath, "..", "TestRuns", "Forced", "boot");
            folder = Path.GetFullPath(folder);
            Directory.CreateDirectory(folder);
            csv = new StreamWriter(Path.Combine(folder, "steps.csv"));
            csv.WriteLine("who,config,profile,step,target_x,target_y,target_z,reading_x,reading_y,reading_z,speed_x,speed_y,speed_z,touched,k,c,limit");
            string whoArg = CaptureTools.Argument("-liveWho");
            string[] whos = string.IsNullOrEmpty(whoArg) ? new[] { "Small", "Long", "Round" } : whoArg.Split(',');
            string configArg = CaptureTools.Argument("-forcedConfigs");
            string[] configs = string.IsNullOrEmpty(configArg) ? new[] { "off", "A", "B", "AB", "edge" } : configArg.Split(',');
            float limitWas = OwnRiser.AnkleLimit, speedWas = OwnRiser.AnkleSpeed;
            int miners = 0;
            foreach (string who in whos)
            {
                yield return Stand(who);
                yield return PushedDown(who);
                if (own.Riser == null) { Debug.Log($"FORCED_NOT_MADE {who}: not made down to get up by its own body within 40 s"); yield return TakenBack(); continue; }
                riser = own.Riser;
                boot = own.Part(Foot); shin = own.Part(Shin);
                toe = null;
                foreach (var link in boot.GetComponentsInChildren<ArticulationBody>()) if (link != boot) toe = link;
                bootTouch = boot.TryGetComponent(out BootTouch bt) ? bt : boot.gameObject.AddComponent<BootTouch>();
                toeTouch = toe == null ? null : toe.TryGetComponent(out BootTouch tt) ? tt : toe.gameObject.AddComponent<BootTouch>();
                bootNames = boot.gameObject.AddComponent<TouchNames>();
                toeNames = toe == null ? null : toe.gameObject.AddComponent<TouchNames>();
                // (A rig: the left boot and its toe are let pass through the rest of its own body, so that the boot is
                // free; in the first runs it lay against the other shin, Small's pose "plant".)
                var mine = new List<Collider>(boot.GetComponentsInChildren<Collider>());
                var ignored = new List<(Collider, Collider)>();
                var others = new HashSet<Collider>(unit.GetComponentsInChildren<Collider>(true));
                for (int k = 0; k < OwnBody.Count; k++) { var part = own.Part(k); if (part != null) foreach (var c in part.GetComponentsInChildren<Collider>(true)) others.Add(c); }
                var fallen = unit.GetComponent<PhysicalFall>();
                for (int k = 0; k < PhysicalFall.Count && fallen != null; k++) { var part = fallen.Part(k); if (part != null) foreach (var c in part.GetComponentsInChildren<Collider>(true)) others.Add(c); }
                foreach (var other in others)
                    if (!mine.Contains(other)) foreach (var c in mine) { Physics.IgnoreCollision(c, other, true); ignored.Add((c, other)); }
                Debug.Log($"FORCED_FREED {who}: the left boot's {mine.Count} colliders let pass through {ignored.Count / Mathf.Max(1, mine.Count)} others of its own body");
                stood = ((Quaternion[])toStoodF.GetValue(riser))[Foot];
                rested = (Quaternion[])restedF.GetValue(riser);
                tall = (float)tallF.GetValue(riser);
                // (The shin's long line in the frame of the boot as it stands, as AnkleFolds has it; across it, the boot's
                // own first line; and between that and the third.)
                twistAxis = (Quaternion.Inverse(rested[Foot] * stood) * Vector3.up).normalized;
                swingAxis = (Vector3.right - Vector3.Dot(Vector3.right, twistAxis) * twistAxis).normalized;
                obliqueAxis = (swingAxis + Vector3.Cross(twistAxis, swingAxis).normalized).normalized;
                clock.Riser = riser;
                var hips = own.Part(OwnBody.Hips);
                float hipsWas = hips.transform.position.y;
                hips.immovable = true;
                hips.TeleportRoot(hips.transform.position + Vector3.up * 1.5f, hips.transform.rotation);
                yield return Wait(.6f);
                miners++;
                Debug.Log(string.Format(Inv, "FORCED_PINNED {0}: the hips at {1:0.000} m before, {2:0.000} m after the lift (immovable {3}); the left boot at {4:0.000} m, its toe at {5:0.000} m",
                    who, hipsWas, hips.transform.position.y, hips.immovable, boot.transform.position.y, toe != null ? toe.transform.position.y : float.NaN));
                Debug.Log(string.Format(Inv, "FORCED_MADE {0}: riser's height {1:0.000000} m; |toStood| of the left boot {2:0.0} deg; the shin's line in the boot as it stands {3}; the swing axis {4}; stage {5}; lighter {6:0.000000} kg m2; {7}",
                    who, tall, Quaternion.Angle(Quaternion.identity, stood), V(twistAxis, "0.000"), V(swingAxis, "0.000"), riser.StageNow, ((float[])lighterF.GetValue(riser))[Foot], AnkleMendArguments.ReadBack()));
                foreach (string config in configs)
                {
                    if (own.Riser != riser) break;
                    float share = ((float[])slackNowF.GetValue(riser))[Foot];
                    // (Today's damper and gains, B off, as the edge case is set from them.)
                    OwnRiser.AnkleLimit = 0; OwnRiser.AnkleSpeed = 0;
                    driveM.Invoke(riser, new object[] { Foot, share });
                    var today = boot.xDrive;
                    float dt = (float)lastStepF.GetValue(riser);
                    float omegaEdge = today.forceLimit / (today.damping + today.stiffness * dt);
                    float omegaB = OmegaOf(20);
                    OwnRiser.AnkleLimit = config == "A" || config == "AB" ? 1 : 0;
                    OwnRiser.AnkleSpeed = config == "B" || config == "AB" ? 20 : config == "edge" ? omegaEdge / Mathf.Sqrt(OwnRiser.AnkleSpeedHeight / tall) : 0;
                    driveM.Invoke(riser, new object[] { Foot, share });
                    var now = boot.xDrive;
                    float bound = config == "edge" ? omegaEdge : omegaB;
                    float floor = OwnRiser.AnkleSpeed > 0 ? Mathf.Max(0, now.forceLimit / OmegaOf(OwnRiser.AnkleSpeed) - now.stiffness * dt) : 0;
                    float smooth = Mathf.Pow(Mathf.Pow(today.damping, 4) + Mathf.Pow(floor, 4), .25f);
                    Debug.Log(string.Format(Inv, "FORCED_CONFIG {0} {1}: {2}; slack share {3:0.###}; step {4:0.####} s; today's gains k {5:0.000} c {6:0.0000} limit {7:0.000}; the drive now holds k {8:0.000} c {9:0.0000} limit {10:0.000} on X, c {11:0.0000} on Y, {12:0.0000} on Z; B's floor {13:0.0000}, the smooth maximum {14:0.0000} (the plain maximum {15:0.0000}): held {16}; the edge's Omega {17:0.00} rad/s, B's at 20 {18:0.00}; the bound read here {19:0.00}",
                        who, config, AnkleMendArguments.ReadBack(), share, dt, today.stiffness, today.damping, today.forceLimit, now.stiffness, now.damping, now.forceLimit, boot.yDrive.damping, boot.zDrive.damping,
                        floor, smooth, Mathf.Max(today.damping, floor), Mathf.Abs(now.damping - (OwnRiser.AnkleSpeed > 0 ? smooth : today.damping)) <= 1e-4f * Mathf.Max(1, now.damping) ? "as written" : "NOT as written", omegaEdge, omegaB, bound));
                    whoNow = who; configNow = config;
                    foreach (var p in Profiles(config))
                    {
                        if (own.Riser != riser) break;
                        yield return Run(who, config, p, bound);
                    }
                }
                OwnRiser.AnkleLimit = limitWas; OwnRiser.AnkleSpeed = speedWas;
                if (own.Riser == riser) driveM.Invoke(riser, new object[] { Foot, ((float[])slackNowF.GetValue(riser))[Foot] });
                clock.Riser = null;
                if (hips != null) hips.immovable = false;
                foreach (var (a, b) in ignored) if (a != null && b != null) Physics.IgnoreCollision(a, b, false);
                Debug.Log($"FORCED_MINER_DONE {who}: {profilesRun} profiles run to their end so far, {profilesLost} lost");
                yield return TakenBack();
            }
            Debug.Log($"FORCED_DONE {miners} of {whos.Length} miners on the rig; {profilesRun} profiles run to their end, {profilesLost} lost; steps in {folder}");
            Assert.That(miners, Is.EqualTo(whos.Length), "Not every miner was made down on the rig.");
            Assert.That(profilesLost, Is.EqualTo(0), "A profile did not run to its end (its riser lost).");
        }

        // ---- Check 3: a going down while the boot whirls (C) ----

        // Each miner (-liveWho; all three if left out; -ankleWays is not needed: Small's ways are given to every miner,
        // a rig) is made down by a push, and then, getting up, made to go down: once plainly; once with its left boot
        // whirled (its joint's speed set to 65 rad/s on its first axis for five steps); once whipped (the boot so, its
        // knee at 25 rad/s and its hip at 15). At each make after, how far each boot was from its shin.
        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator TheWhirlingGoingDown()
        {
            Assert.That(goDownM != null && bootShiftF != null && stoodAnkleF != null && lighterF != null, Is.True, "OwnBody lacks what the forced going down reaches for.");
            Assert.That(AnkleWatch.Reads(out string unread), Is.True, unread);
            Time.captureFramerate = 50;
            PhysicalFall.GetsUpByItsOwn = true;
            LogAssert.ignoreFailingMessages = true;
            Debug.Log("FORCED_SET " + AnkleMendArguments.Apply());
            var text = Resources.Load<TextAsset>("RiseWays/Small");
            Assert.That(text, Is.Not.Null, "No Resources/RiseWays/Small.txt.");
            OwnBody.RiseWays = OwnRiser.Read(text.text);
            Debug.Log("FORCED_RIG every miner given Small's ways up (" + OwnBody.RiseWays.Length + " ways): a rig, not the game; " + AnkleMendArguments.ReadBack());
            string whoArg = CaptureTools.Argument("-liveWho");
            string[] whos = string.IsNullOrEmpty(whoArg) ? new[] { "Small", "Long", "Round" } : whoArg.Split(',');
            int makesRead = 0, makesMissed = 0;
            foreach (string who in whos)
            {
                yield return Stand(who);
                var standAnchor = new[] { own.Part(Foot).anchorPosition, own.Part(Foot + 1).anchorPosition };
                var stoodAnkle = (Vector3[])stoodAnkleF.GetValue(own);
                Debug.Log(string.Format(Inv, "FORCED_STANDING {0}: its boots' ankles in the body made standing {1} {2} m; OwnBody's stoodAnkle {3} {4} m", who, V(standAnchor[0], "0.0000"), V(standAnchor[1], "0.0000"), V(stoodAnkle[0], "0.0000"), V(stoodAnkle[1], "0.0000")));
                watch = new GameObject("Ankle watch").AddComponent<AnkleWatch>();
                watch.Own = own; watch.Who = who;
                yield return Wait(.3f);
                yield return PushedDown(who);
                string[] kinds = { "after the panel's push", "plain", "boot whirled", "whipped" };
                for (int n = 0; n < kinds.Length; n++)
                {
                    if (n > 0)
                    {
                        var was = own.Riser;
                        if (was == null) { Debug.Log($"FORCED_DOWN_SKIPPED {who} {kinds[n]}: no riser to go down from"); makesMissed++; break; }
                        yield return Wait(1);
                        if (own.Riser != was || own.Riser == null) { Debug.Log($"FORCED_DOWN_SKIPPED {who} {kinds[n]}: the riser ended by itself first"); makesMissed++; break; }
                        bool whirl = n >= 2, whip = n == 3;
                        float atGoing = 0, whirlBefore = 0, whirlAfter = 0; Vector3 atGoingV = Vector3.zero, kneeSpeed = Vector3.zero;
                        float gap = float.NaN;
                        bool sent = false;
                        forcer.Lost = () => own == null || (!sent && own.Riser != was);
                        // The whirl is set on every step up to and including the one of the going down (the drive damps
                        // it within a step, so a whirl ended before would be gone); the step after reads what the engine
                        // made of it.
                        forcer.Begin(whirl ? 7 : 1, i =>
                        {
                            var b = own.Part(Foot);
                            if (i == 6) { whirlAfter = b != null && b.isActiveAndEnabled ? PerAxis(Speeds(b)) : float.NaN; return; }
                            sent = i == 5 || !whirl;
                            if (whirl)
                            {
                                whirlBefore = PerAxis(Speeds(b));
                                var w = b.jointVelocity; w[0] = 65; b.jointVelocity = w;
                                if (whip)
                                {
                                    var k = own.Part(Shin); var kw = k.jointVelocity; kw[0] = 25; k.jointVelocity = kw;
                                    var h = own.Part(Thigh); var hw = h.jointVelocity; hw[0] = 15; h.jointVelocity = hw;
                                }
                                if (i < 5) return;
                            }
                            atGoingV = Speeds(b); atGoing = PerAxis(atGoingV);
                            var kv = own.Part(Shin).jointVelocity; kneeSpeed = new Vector3(kv.dofCount > 0 ? kv[0] : 0, 0, 0);
                            goDownM.Invoke(own, null);
                        });
                        while (!forcer.Done) yield return null;
                        Debug.Log(string.Format(Inv, "FORCED_GOING_DOWN {0} {1}: made to go down (OwnBody.GoDown) {2}; the left boot at {3:0.0} rad/s on an axis {4} as it went (read back after the set; the engine had it at {7:0.0} before the set, {8:0.0} the step after), its knee {5:0.0}; it has gone down by itself {6} times",
                            who, kinds[n], !forcer.WasLost, atGoing, V(atGoingV), kneeSpeed.x, own.WentDown, whirlBefore, whirlAfter));
                        for (float until = Time.time + 40; (own.Riser == null || own.Riser == was) && Time.time < until;) yield return null;
                        if (own.Riser == null || own.Riser == was) { Debug.Log($"FORCED_NOT_MADE {who} {kinds[n]}: not made down again within 40 s"); makesMissed++; break; }
                    }
                    else if (own.Riser == null) { Debug.Log($"FORCED_NOT_MADE {who}: not made down by the push within 40 s"); makesMissed++; break; }
                    // The make: each boot's ankle against the body made standing; how far the refit moved it (C on), what
                    // it weighs about its ankle.
                    var shift = (Vector3[])bootShiftF.GetValue(own);
                    float[] lighter = (float[])lighterF.GetValue(own.Riser);
                    var sb = new StringBuilder();
                    sb.AppendFormat(Inv, "FORCED_MAKE {0} {1}: BootsFromOwnShin {2}; ", who, kinds[n], OwnBody.BootsFromOwnShin);
                    for (int leg = 0; leg < 2; leg++)
                    {
                        var b = own.Part(Foot + leg);
                        float apart = (b.anchorPosition - standAnchor[leg]).magnitude;
                        // (Where the let-go body had the boot, against where a healthy one would be: with C on, how far the
                        // refit moved it; off, the make keeps it where it was, so its ankle's distance from the standing one.)
                        float before = OwnBody.BootsFromOwnShin ? shift[leg].magnitude : apart;
                        sb.AppendFormat(Inv, "boot {0}: its ankle {1:0.0000} m from where it was standing, the let-go body had it {2:0.0000} m off (refit moved it {3:0.0000} m), lighter {4:0.000000} kg m2; ",
                            leg, apart, before, shift[leg].magnitude, lighter[Foot + leg]);
                        if (n >= 2 && leg == 0) sb.AppendFormat(Inv, "BAR_C (the whirled boot's ankle within 0.01 m of the standing one at the make) {0}; ", apart <= .01f ? "PASS" : "FAIL");
                    }
                    Debug.Log(sb.ToString());
                    makesRead++;
                }
                Debug.Log(watch.Summary());
                foreach (var line in watch.Lines) if (line.StartsWith("ANKLE_DOWN")) Debug.Log("FORCED_WATCH " + line);
                Object.Destroy(watch.gameObject); watch = null;
                yield return TakenBack();
            }
            Debug.Log($"FORCED_DOWN_DONE {makesRead} makes read, {makesMissed} missed");
            Assert.That(makesMissed, Is.EqualTo(0), "A forced going down did not come to its make.");
        }
    }

    // ---- Check 8: the once-said messages are forgotten between tests ----
    // Two tests of one run each expect OwnRiser's once-said error for AnkleLimit 2 (AnkleMended, on a riser made without
    // its constructor: the switch's message is said before anything of the body is read). With AnkleMendArguments' Save
    // and Restore (commit e8bac96) both hear it. -onceSaidKeep 1, the known-bad: neither is called, the flag is not
    // forgotten, and the second test must fail.
    public sealed class AnkleOnceSaidTests
    {
        private static readonly MethodInfo mendedM = typeof(OwnRiser).GetMethod("AnkleMended", BindingFlags.NonPublic | BindingFlags.Instance);
        private float limitWas, speedWas;
        private bool keeps;

        [SetUp]
        public void Begin()
        {
            keeps = CaptureTools.Argument("-onceSaidKeep") == "1";
            limitWas = OwnRiser.AnkleLimit; speedWas = OwnRiser.AnkleSpeed;
            if (!keeps) AnkleMendArguments.Save();
        }

        [TearDown]
        public void End()
        {
            OwnRiser.AnkleLimit = limitWas; OwnRiser.AnkleSpeed = speedWas;
            if (!keeps) AnkleMendArguments.Restore();
        }

        private void SaysIt(string which)
        {
            Assert.That(mendedM, Is.Not.Null, "OwnRiser has no AnkleMended.");
            Debug.Log("ONCE_SAID " + which + (keeps ? " (-onceSaidKeep 1: the flags not forgotten)" : " (forgotten by AnkleMendArguments)"));
            var riser = (OwnRiser)FormatterServices.GetUninitializedObject(typeof(OwnRiser));
            OwnRiser.AnkleLimit = 2; OwnRiser.AnkleSpeed = 0;
            LogAssert.Expect(LogType.Error, new Regex(@"^OwnRiser\.AnkleLimit is 2: only 0 \(as before\) and 1"));
            bool wrote = (bool)mendedM.Invoke(riser, new object[] { OwnBody.Foot, Quaternion.identity });
            Assert.That(wrote, Is.False, "AnkleLimit 2 acted as on.");
        }

        [Test, Order(1)]
        public void TheFirstTestHearsIt() => SaysIt("the first test");

        [Test, Order(2)]
        public void TheSecondTestHearsItToo() => SaysIt("the second test");
    }
}
