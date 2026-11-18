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
    // "Forced checks"; forced_checks.md beside it; November 17, 2026; mended the same day after three reviews,
    // review_forced_*.md). Test code only: nothing in the game. Each check makes its rare event happen, and says so.
    //
    // The rig (AnkleForcedTests.TheBootForced), a claim about this rig and not the game: each miner is given Small's ways
    // up (OwnBody.RiseWays, as -ankleWays Small does), pushed over and made down to get up by its own body; then its hips
    // are pinned (the root made immovable) and lifted 1.5 m, so that its boots touch nothing, and the riser's clock is held
    // (its time in the pose kept at nothing each step, OwnRiser.StuckAfter 0), so that it keeps its pose and never gives
    // up or hands over. The riser goes on writing every joint as it does; then, last of each step, the left boot's ask is
    // written again through the riser's own Aims (reflection), with the ask the check forces: the path the switches live
    // on, unchanged. Its shin is held by its own joints, the hips pinned.
    //
    // Every bar decides its test (ForcedBars): a bar that fails fails the test, with the bar's own line as the message.
    // A known-bad run is the same test with -forcedMustFail naming the bars (or bar@profile) that must fail: the test then
    // passes only if each named bar failed on every case it judged, on every miner, and says so (FORCED_KNOWN_BAD).

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
        public int Total;
        private void Count(Collision touch)
        {
            string name = touch.collider != null ? touch.collider.name + (touch.collider.attachedArticulationBody != null ? "[" + touch.collider.attachedArticulationBody.name + "]" : "") : "?";
            Seen[name] = Seen.TryGetValue(name, out int n) ? n + 1 : 1;
            Total++;
        }
        private void OnCollisionEnter(Collision touch) => Count(touch);
        private void OnCollisionStay(Collision touch) => Count(touch);
        public string Said() { var parts = new List<string>(); foreach (var kv in Seen) parts.Add(kv.Key + " " + kv.Value); return parts.Count == 0 ? "nothing" : string.Join(", ", parts); }
    }

    // Every bar judged in a test, and its verdict: each must pass, or, named by -forcedMustFail (a bar's name, or
    // name@profile), must have failed on every case it judged, on every miner run (the known-bad run).
    internal sealed class ForcedBars
    {
        private struct Judged { public string Bar, Who, Config, Prof, Said; public bool Pass; }
        private readonly List<Judged> all = new List<Judged>();
        public readonly List<string> Events = new List<string>();   // the forced event not made: always a failure
        public void Clear() { all.Clear(); Events.Clear(); }
        public void Judge(string bar, string who, string config, string prof, bool pass, string said)
        {
            all.Add(new Judged { Bar = bar, Who = who, Config = config, Prof = prof, Pass = pass, Said = said });
            Debug.Log("FORCED_BAR " + bar + " " + who + " " + config + " " + prof + ": " + (pass ? "PASS" : "FAIL") + "; " + said);
        }
        public void EventMissed(string said) { Events.Add(said); Debug.Log("FORCED_EVENT_MISSED " + said); }
        public static HashSet<string> MustFail()
        {
            var set = new HashSet<string>();
            foreach (var w in (CaptureTools.Argument("-forcedMustFail") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) set.Add(w.Trim());
            return set;
        }
        // The problems (empty: the test passes), each said in the log.
        public List<string> Verdict(HashSet<string> mustFail, IEnumerable<string> whos)
        {
            var problems = new List<string>(Events);
            foreach (var j in all)
            {
                bool named = mustFail.Contains(j.Bar) || mustFail.Contains(j.Bar + "@" + j.Prof);
                if (named && j.Pass) problems.Add(j.Bar + " " + j.Who + " " + j.Config + " " + j.Prof + " PASSED, but -forcedMustFail says it must fail (the known-bad not caught): " + j.Said);
                if (!named && !j.Pass) problems.Add(j.Bar + " " + j.Who + " " + j.Config + " " + j.Prof + " FAIL: " + j.Said);
            }
            foreach (var name in mustFail)
                foreach (var who in whos)
                {
                    int judged = 0, failed = 0;
                    var cases = new List<string>();
                    foreach (var j in all)
                        if ((j.Bar == name || j.Bar + "@" + j.Prof == name) && j.Who == who) { judged++; if (!j.Pass) failed++; cases.Add(j.Config + " " + j.Prof + (j.Pass ? " passed" : " failed")); }
                    if (judged == 0) problems.Add(name + " is named by -forcedMustFail but was never judged on " + who);
                    else if (failed == judged) Debug.Log("FORCED_KNOWN_BAD " + name + " " + who + ": failed on all " + judged + " cases it judged (" + string.Join(", ", cases) + "), as -forcedMustFail asks: this part of the run passes because the bar failed");
                }
            var groups = new SortedDictionary<string, int[]>();
            foreach (var j in all)
            {
                string key = j.Bar + " " + j.Config;
                if (!groups.TryGetValue(key, out var n)) groups[key] = n = new int[2];
                n[j.Pass ? 0 : 1]++;
            }
            foreach (var g in groups) Debug.Log("FORCED_VERDICT " + g.Key + ": " + (g.Value[0] + g.Value[1]) + " judged, " + g.Value[0] + " passed, " + g.Value[1] + " failed");
            Debug.Log("FORCED_TEST_VERDICT " + all.Count + " bars judged; must fail: " + (mustFail.Count == 0 ? "none (a known-good run)" : string.Join(",", mustFail)) + "; " + problems.Count + " problems" + (problems.Count == 0 ? "" : ": " + string.Join(" | ", problems)));
            return problems;
        }
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
            bootShiftF = typeof(OwnBody).GetField("bootShift", Hidden), stoodAnkleF = typeof(OwnBody).GetField("stoodAnkle", Hidden),
            ankleAtF = typeof(OwnBody).GetField("ankleAt", Hidden),
            bootsMadeF = typeof(PhysicalFall).GetField("bootsMade", Hidden), slackenedF = typeof(PhysicalFall).GetField("slackened", Hidden);
        private const int Foot = OwnBody.Foot, Shin = OwnBody.Shin, Thigh = OwnBody.Thigh;
        // (The fold's own slope against the ask's: at most 3.74 in the physicist's 200,000 random asks with the twist fold,
        // review_physicist.md:57 and :135; research.md §1.4 says 2.4 for the swing fold alone.)
        private const float FoldSlope = 3.75f;
        // The bars' numbers, each defined once (step 4a‴, November 18), its unit in its name. Where a number came from is
        // said beside it; forced_checks.md has the same list.
        private const float BarAMarginDeg = 1f;          // BAR_A, BAR_A_SLERP: the allowance's added degree
        private const float BarFormDeg = .05f;           // BAR_FORM and BAR_TANH: the target within so many degrees of the form
        private const float BarFollowDeg = 5f;           // BAR_FOLLOW: the boot within so many degrees of the target, at the hold's end
        private const float BarAxesDeg = 1f;             // BAR_AXES: the boot's turn and the joint's reading within so many degrees
        private const float AxesNearMostDeg = 110f;      // BAR_AXES judges targets out to it; BAR_AXES_READ past it
        private const float AxesFarFirstDeg = 115f;      // axes_far's sweep starts here (5 degrees past AxesNearMostDeg)
        // BAR_AXES_READ: 3 degrees, set from the data (forced_bad2's settled holds of 130-145 deg sagged 1.2-1.9 deg), not
        // from a model; the sag the boot's weight gives over the drive's stiffness is read beside it (FORCED_AXES_SAG).
        private const float BarAxesReadDeg = 3f;
        private const float SettledRadPerS = 1f;         // a hold "came to rest": under so many rad/s on every axis at its end
        private const int AxesReadLeastHolds = 5;        // fewer holds judged past AxesNearMostDeg: an event missed
        private const float TanhShareMissed = .03f;      // check 4: an ask that misses its share of E by more: an event missed
        private const float WrapOutsideDeg = 180.01f;    // BAR_WRAP: a target written outside half a turn (and a hundredth)
        private const float HalfTurnDeg = 180f;          // BAR_WRAP judges the steps whose reading is inside it
        private const double BarDampShare = 1e-4;        // BAR_DAMP: the damping held within this share of research's
        private const float PivotMovedLeastM = .01f;     // BAR_C_PIVOT judged where the slack moved a point at least so many metres
        private const float PivotShare = .25f;           // BAR_C_PIVOT: S moved at most this share of what A moved
        private const int PivotLeastCourses = 3;         // whirled courses judged per miner and setting, at least
        // (Floors, from the profiles' own lengths and not from any result: fewer judged is an event missed.)
        private const int LeastWindowSteps = 1;          // BAR_A: steps; the floor is every step the profile's window names (swing_1800dps' 4, swing_instant's 25), never below this
        private const int LeastSteps = 10;               // BAR_B, BAR_FORM, BAR_WRAP: the shortest profile has 67 steps
        private static string BarsSaid() => string.Format(Inv,
            "bars judged now: BAR_A (the form's step at most {0:0.00} x the ask's turn + {1:0} deg; at least {2} window steps), BAR_A_SLERP (a reading: the ask's own step + {1:0} deg), BAR_B (the boot at most Omega rad/s on an axis; at least {3} steps), BAR_FOLLOW ({4:0} deg), BAR_FORM ({5:0.00} deg; at least {3} steps), BAR_TANH ({5:0.00} deg; every ask), BAR_DAMP (within {6:0.0000} of research's c), BAR_WRAP (no target past {7:0.00} deg from a reading inside {8:0} deg; at least {3} steps), BAR_AXES ({9:0} deg, out to {10:0} deg), BAR_AXES_READ ({11:0} deg past {10:0} deg, holds under {12:0} rad/s, at least {13}); a NaN in any measure is a failure",
            FoldSlope, BarAMarginDeg, LeastWindowSteps, LeastSteps, BarFollowDeg, BarFormDeg, BarDampShare, WrapOutsideDeg, HalfTurnDeg, BarAxesDeg, AxesNearMostDeg, BarAxesReadDeg, SettledRadPerS, AxesReadLeastHolds);
        // (-forcedFeed BAR_X, a guard's known-bad (step 4a‴): of the cases BAR_X judges in the run, in order, the first is fed
        // one NaN into its measure and the second nothing at all (its values withheld); the test must fail on both, for those
        // reasons, and on nothing else. BAR_DAMP judges one value per config: its first case is fed a NaN, its second none.)
        private string feedBar = "";
        private int feedCount;
        private int FeedCase(string bar, bool judged) => feedBar == bar && judged ? feedCount++ : -1;
        private static readonly FieldInfo slackByF = typeof(OwnBody).GetField("slackBy", Hidden);

        private MinerChoice choice;
        private MinerWorkPreview look;
        private bool ownWas, bootsWas;
        private int rateWas;
        private float stuckWas, limitWas, speedWas, atMostWas;
        private OwnRiser.Way[] waysWas;
        private ForcedStep forcer;
        private RiserClockHeld clock;
        private AnkleWatch watch;
        private StreamWriter csv;
        private readonly ForcedBars bars = new ForcedBars();

        [UnitySetUp]
        public IEnumerator Load()
        {
            rateWas = Time.captureFramerate;
            ownWas = PhysicalFall.GetsUpByItsOwn;
            waysWas = OwnBody.RiseWays;
            stuckWas = OwnRiser.StuckAfter;
            limitWas = OwnRiser.AnkleLimit; speedWas = OwnRiser.AnkleSpeed; atMostWas = OwnRiser.AnkleAtMost; bootsWas = OwnBody.BootsFromOwnShin;
            AnkleMendArguments.Save();
            bars.Clear();
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
            forcer = new GameObject("Forced step").AddComponent<ForcedStep>();
            clock = new GameObject("Riser clock held").AddComponent<RiserClockHeld>();
        }

        // (Every switch the rig sets is put back here too, whatever ended the test; the tests themselves put them back in
        // a finally.)
        private void SwitchesBack()
        {
            OwnRiser.AnkleLimit = limitWas; OwnRiser.AnkleSpeed = speedWas; OwnRiser.AnkleAtMost = atMostWas; OwnBody.BootsFromOwnShin = bootsWas;
            OwnRiser.StuckAfter = stuckWas;
            if (waysWas != null) OwnBody.RiseWays = waysWas;
            PhysicalFall.GetsUpByItsOwn = ownWas;
        }

        [TearDown]
        public void Restore()
        {
            if (csv != null) { csv.Close(); csv = null; }
            if (forcer != null) Object.Destroy(forcer.gameObject);
            if (clock != null) Object.Destroy(clock.gameObject);
            if (watch != null) Object.Destroy(watch.gameObject);
            forcer = null; clock = null; watch = null;
            SwitchesBack();
            Time.captureFramerate = rateWas;
            AnkleMendArguments.Restore();
            Debug.Log("FORCED_RESTORED " + AnkleMendArguments.ReadBack());
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
        // B's target, written anew here from research.md §4.2 (lines 103-106), not OwnRiser.AnkleSlows: E = tau_cap / k
        // (the error at which the drive gives all it has); e = the form before B less the joint's present reading; the
        // error written e' = E tanh(|e| / E) e/|e|; the target = the reading + e'. In doubles: the form in degrees, the
        // reading in radians, the target out in degrees.
        private static Vector3 ResearchTarget(Vector3 formDeg, Vector3 readingRad, double tauCap, double k)
        {
            double E = tauCap / k, toRad = Math.PI / 180;
            double ex = formDeg.x * toRad - readingRad.x, ey = formDeg.y * toRad - readingRad.y, ez = formDeg.z * toRad - readingRad.z;
            double size = Math.Sqrt(ex * ex + ey * ey + ez * ez);
            double scale = size > 1e-12 ? E * Math.Tanh(size / E) / size : 1;
            return new Vector3((float)((readingRad.x + ex * scale) / toRad), (float)((readingRad.y + ey * scale) / toRad), (float)((readingRad.z + ez * scale) / toRad));
        }
        // B's damper, written anew from research.md §4.2 (line 105): c = (c_today^4 + c_floor^4)^(1/4), c_floor = max(0,
        // tau_cap / Omega - k dt), Omega = AnkleSpeed sqrt(AnkleSpeedHeight / the body's height).
        private static double ResearchDamper(double cToday, double tauCap, double k, double dt, double omega)
        {
            double floor = Math.Max(0, tauCap / omega - k * dt);
            return Math.Pow(Math.Pow(cToday, 4) + Math.Pow(floor, 4), .25);
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
        private static float Ax(Vector3 v, int a) => a == 0 ? v.x : a == 1 ? v.y : v.z;
        private static void DrivesOff(ArticulationBody b)
        {
            var d = b.xDrive; d.stiffness = 0; d.damping = 0; b.xDrive = d;
            d = b.yDrive; d.stiffness = 0; d.damping = 0; b.yDrive = d;
            d = b.zDrive; d.stiffness = 0; d.damping = 0; b.zDrive = d;
        }

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
            public bool AgainstAsk;                    // slerp_antipode: A's jump judged against the ask's own (BAR_A_SLERP)
            public int FollowAt = -1;                  // the step at which the boot must be where it is asked (the lazy way's catch)
            public Action<int> Before, After;          // the rig's own writes, before and after the ask (check 5, 6)
            public Action Start, End;                  // the rig's own settings for this profile (AnkleAtMost 0), put back at its end
            public Func<int, bool> Raw;                // check 5: the steps the rig writes raw targets (BAR_WRAP's known-bad)
            public int[] TanhAt;                       // steps at which B's writing is compared with its formula (check 4)
            public float[] TanhShare;                  // the share of E each of them asks
            public bool AxesRead;                      // check 6 past 110 deg: a reading (BAR_AXES not judged), every hold said
            public bool Axes;                          // check 6: read at the end of each hold
            public int AxesHold;
            public Vector3 AxisLocal;                  // check 2: the ask's own axis (the boot's frame as it stands), read against the boot's turning
            public Func<int, bool> AxisSample;         // the steps at which the boot's turning axis is read
        }

        private ArticulationBody boot, toe, shin;
        private BootTouch bootTouch, toeTouch;
        private TouchNames bootNames, toeNames;
        private string whoNow = "", configNow = "", profNow = "";
        private static readonly MethodInfo foldsM = typeof(OwnRiser).GetMethod("AnkleFolds", Hidden, null, new[] { typeof(int), typeof(Quaternion) }, null);
        private OwnRiser riser;
        private Quaternion stood;
        private Quaternion[] rested;
        private float tall, height;
        private Vector3 twistAxis, swingAxis, obliqueAxis, mixedAxis;
        private bool axesBad;
        // (The known-bads of the bars that had none (November 18): -forcedFormBad 1, BAR_FORM's: with A on, the riser
        // made to write with AnkleLimit 0 through each profile while the bar expects A's fold; -forcedFollowBad 1,
        // BAR_FOLLOW's: the boot's drive let go from step 10 to the step it is read, so that it cannot follow; -forcedWrapBad
        // 1, BAR_WRAP's: the rig's own raw writes outside half a turn judged as BAR_WRAP judges B's, as "beyond_raw".)
        private bool formBad, followBad, wrapBad;
        // (-forcedWrapBadB 1, BAR_WRAP's known-bad on B's own path (step 4a‴): in configs B, AB and edge, B's written target
        // replaced, after B and before BAR_WRAP's count reads it, by the same turn written the long way, outside half a
        // turn; read back from the drive by that count; then B's own target written back before the engine steps.)
        private bool wrapBadB;

        private float OmegaOf(float ankleSpeed) => ankleSpeed * Mathf.Sqrt(OwnRiser.AnkleSpeedHeight / tall);
        // (The shin's own long line, from its knee to its ankle: the two joints it is built between, read off the running
        // shin, in the world.)
        private Vector3 ShinLine() => (shin.transform.TransformPoint(boot.parentAnchorPosition) - shin.transform.TransformPoint(shin.anchorPosition)).normalized;

        private IEnumerator Settle(int steps)
        {
            forcer.Lost = () => own == null || own.Riser != riser || !own.Stands;
            forcer.Begin(steps, i => aimsM.Invoke(riser, new object[] { Foot, stood }));
            while (!forcer.Done) yield return null;
        }

        private int profilesRun, profilesLost;

        private IEnumerator Run(string who, string config, Prof p, float omegaBar)
        {
            profNow = p.Name;
            yield return Settle(25);
            if (forcer.WasLost) { profilesLost++; Debug.Log($"FORCED_LOST {who} {config} {p.Name}: the riser was lost while settling"); yield break; }
            p.Start?.Invoke();
            bool judgesA = config == "off" || config == "A" || config == "AB", judgesB = config == "off" || config == "B" || config == "AB" || config == "edge";
            bool judgesWrap = config == "B" || config == "AB" || config == "edge";
            // (The guards' known-bads, -forcedFeed: 0, this case is fed one NaN; 1, nothing; -1, as it is.)
            int fA = FeedCase("BAR_A", p.Window != null && judgesA && !p.AgainstAsk), fB = FeedCase("BAR_B", judgesB && p.Name != "beyond"),
                fF = FeedCase("BAR_FOLLOW", p.FollowAt > 0), fM = FeedCase("BAR_FORM", !p.Axes || !axesBad), fT = FeedCase("BAR_TANH", p.TanhAt != null && judgesB),
                fW = FeedCase("BAR_WRAP", judgesWrap), fX = FeedCase("BAR_AXES", p.Axes && !p.AxesRead), fR = FeedCase("BAR_AXES_READ", p.AxesRead);
            int winNaN = 0, bSteps = 0, bNaN = 0, formNaN = 0, tanhNaN = 0, wrapNaN = 0, axesNaN = 0, farNaN = 0, wrapBadBWrites = 0; bool followRead = false;
            float sagMost = 0, sagLeast = float.PositiveInfinity, drivenMost = 0; int sagN = 0;
            // (What it reads, step by step.)
            float fastFree = 0, fastTouched = 0, fastVec = 0, shinFast = 0, jumpTurnMost = 0, jumpWrittenMost = 0, askTurnMost = 0, offEMost = 0, followErr = float.NaN;
            int fastAt = -1, jumps60 = 0, winViolations = 0, winSteps = 0, touchedSteps = 0, tanhChecks = 0, tanhFails = 0, clampWouldFail = 0, unslowedWouldFail = 0, reachMissed = 0;
            float winRatioMost = 0, winPreAt = 0, winAskAt = 0, winAskVecAt = 0; int winAt = -1;
            float tanhErrMost = 0, clampDiffLeast = float.PositiveInfinity, unslowedDiffLeast = float.PositiveInfinity, reachMost = 0;
            Vector3 lastT = Vector3.zero, lastPre = Vector3.zero, pre = Vector3.zero, lastR = Vector3.zero, lastV = Vector3.zero; Quaternion lastAsk = Quaternion.identity; bool first = true;
            string fastWhy = "";
            var axesLines = new List<string>();
            float rvErrMost = 0, readErrMost = 0, permErrLeast = float.PositiveInfinity, eulerErrMost = 0, readVsGotMost = 0, readSwappedLeast = float.PositiveInfinity;
            int farSettled = 0, farHolds = 0; float farSettledMost = 0, farUnsettledLeast = float.PositiveInfinity;
            int farJudged = 0; float farErrMost = 0, farJudgedMost = 0, farSwappedLeast = float.PositiveInfinity;
            var tanhLines = new StringBuilder();
            float followAsk = float.NaN;
            float preJumpMost = 0, formErrMost = 0; int formChecks = 0, formFails = 0;
            int wrapViolations = 0, wrapJudged = 0, rawOutside = 0; float wrapTargetMost = 0;
            float axisAngleSum = 0, axisAngleMost = 0, axisShinMost = 0; int axisSamples = 0;
            bool limits = OwnRiser.AnkleLimit == 1, slows = OwnRiser.AnkleSpeed > 0;
            float lighterFoot = ((float[])lighterF.GetValue(riser))[Foot];
            bootNames.Seen.Clear(); if (toeNames != null) toeNames.Seen.Clear();
            // (BAR_FORM's known-bad: A on for the bar, the riser made to write as with A off.)
            bool flipped = formBad && limits;
            if (flipped) OwnRiser.AnkleLimit = 0;
            float limitInProfile = float.NaN;
            // (BAR_FOLLOW's known-bad: the drive let go each step from 10 to the step the boot is read, given back after.)
            Action<int> after = p.After;
            int followLetGo = 0; bool blowLogged = false;
            if (followBad && p.FollowAt > 0)
                after = i =>
                {
                    p.After?.Invoke(i);
                    if (i >= 10 && i <= p.FollowAt) { DrivesOff(boot); followLetGo++; }
                    else if (i == p.FollowAt + 1) driveM.Invoke(riser, new object[] { Foot, ((float[])slackNowF.GetValue(riser))[Foot] });
                };
            forcer.Lost = () => own == null || own.Riser != riser || !own.Stands;
            forcer.Begin(p.Steps, i =>
            {
                if (i == 1) limitInProfile = OwnRiser.AnkleLimit;
                // (The step just taken: the boot's joint speed per axis, its shin, its touches.)
                Vector3 v = Speeds(boot);
                bool touched = (bootTouch != null && bootTouch.Touched) || (toeTouch != null && toeTouch.Touched);
                if (bootTouch != null) bootTouch.Touched = false;
                if (toeTouch != null) toeTouch.Touched = false;
                var drive = boot.xDrive;
                if (i > 0 && fB != 1)
                {
                    // (BAR_B's measure: a NaN on any axis counted, never lost in a maximum; fed one at its first step.)
                    if (fB == 0 && bSteps == 0) v.x = float.NaN;
                    bSteps++;
                    if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z)) bNaN++;
                    float fast = PerAxis(v);
                    if (!blowLogged && fast > 100)
                    {
                        // (Step 4a‴: forced_good4's Small A swing cases read 1600-13412 rad/s where forced_good3's read
                        // 23-42; a reading of the first step past 100 rad/s, judging nothing.)
                        blowLogged = true;
                        var jp = boot.jointPosition; var jpS = new StringBuilder();
                        for (int d = 0; d < jp.dofCount; d++) jpS.Append(d > 0 ? " " : "").Append(jp[d].ToString("0.000", Inv));
                        Debug.Log(string.Format(Inv, "FORCED_BLOW {0} {1} {2}: first past 100 rad/s at step {3}: speeds ({4:0.0} {5:0.0} {6:0.0}) rad/s, the joint at ({7}) rad, the target written {8} deg, touching {9}",
                            who, config, p.Name, i, v.x, v.y, v.z, jp.dofCount > 0 ? jpS.ToString() : "-", Target(boot).ToString("0.0"), touched));
                    }
                    if (touched) { touchedSteps++; fastTouched = Mathf.Max(fastTouched, fast); }
                    else if (fast > fastFree)
                    {
                        fastFree = fast; fastAt = i; fastVec = v.magnitude;
                        // (What the implicit drive would make of the last step on that axis alone, with the riser's own
                        // inertia for the boot (lighter): research.md §4.2, line 86; a reading of why it is so fast.)
                        int a = Mathf.Abs(v.x) >= Mathf.Abs(v.y) && Mathf.Abs(v.x) >= Mathf.Abs(v.z) ? 0 : Mathf.Abs(v.y) >= Mathf.Abs(v.z) ? 1 : 2;
                        float dt = Time.fixedDeltaTime, I = lighterFoot, k = drive.stiffness, c = drive.damping, cap = drive.forceLimit;
                        float e = Ax(lastT, a) * Mathf.Deg2Rad - Ax(lastR, a), w0 = Ax(lastV, a);
                        float w1 = (I / dt * w0 + k * e) / (I / dt + k * dt + c), tq = I * (w1 - w0) / dt;
                        if (Mathf.Abs(tq) > cap) w1 = w0 + Mathf.Sign(tq) * cap * dt / I;
                        fastWhy = string.Format(Inv, "on axis {0}: the error at the step's start {1:0.0} deg, the speed then {2:0.0} rad/s; the implicit drive alone on that axis (I = lighter {3:0.000000}, k {4:0.000}, c {5:0.0000}, cap {6:0.000}) gives {7:0.0} rad/s, the engine read {8:0.0}",
                            a, e * Mathf.Rad2Deg, w0, I, k, c, cap, w1, Ax(v, a));
                    }
                    shinFast = Mathf.Max(shinFast, shin.angularVelocity.magnitude);
                }
                p.Before?.Invoke(i);
                Quaternion ask = p.Ask(i);
                aimsM.Invoke(riser, new object[] { Foot, ask });
                // (The form before B: A's fold of the ask (with AnkleLimit 1, the riser's own AnkleFolds: a check that the
                // path is wired, not of A's design), today's clamp of it (0); and the target written against it, slowed as
                // research.md §4.2 has it when B is on: every step, the written form read back.)
                Vector3 driven = ((Vector3[])drivenNowF.GetValue(riser))[Foot];
                Vector3 r = Reading(boot);
                drive = boot.xDrive;
                float most = drive.stiffness > 0 ? drive.forceLimit / drive.stiffness : float.NaN;
                pre = limits ? RV((Quaternion)foldsM.Invoke(riser, new object[] { 0, Quaternion.Normalize(ask) * FromRV(driven) })) : TodayForm(ask, stood, driven);
                Vector3 written = Target(boot);
                {
                    Vector3 expect = slows && !float.IsNaN(most) ? ResearchTarget(pre, r, drive.forceLimit, drive.stiffness) : pre;
                    float err = (written - expect).magnitude;
                    if (fM == 0 && formChecks == 0) err = float.NaN;
                    if (fM != 1)
                    {
                        formChecks++;
                        if (float.IsNaN(err)) formNaN++;
                        else { if (err > BarFormDeg) formFails++; formErrMost = Mathf.Max(formErrMost, err); }
                    }
                }
                after?.Invoke(i);
                // (What was written for the step to come, and what the drive holds.)
                Vector3 T = Target(boot);
                float rDeg = r.magnitude * Mathf.Rad2Deg;
                // (-forcedWrapBadB 1: B's target replaced by the same turn the long way round, outside half a turn, and read
                // back from the drive by the count below, the very count that judges B.)
                Vector3 bWrote = T; bool bReplaced = false;
                if (wrapBadB && judgesWrap && (p.Raw == null || !p.Raw(i)) && rDeg <= HalfTurnDeg && T.magnitude > 1)
                {
                    Vector3 longWay = -T.normalized * (360 - T.magnitude);
                    boot.SetDriveTarget(ArticulationDriveAxis.X, longWay.x); boot.SetDriveTarget(ArticulationDriveAxis.Y, longWay.y); boot.SetDriveTarget(ArticulationDriveAxis.Z, longWay.z);
                    T = Target(boot); bReplaced = true; wrapBadBWrites++;
                }
                if (p.Raw != null && p.Raw(i)) { if (rDeg <= HalfTurnDeg && T.magnitude > WrapOutsideDeg) rawOutside++; }
                else if (float.IsNaN(rDeg) || rDeg <= HalfTurnDeg)
                {
                    // (BAR_WRAP's measure: a NaN reading or target counted, not lost; fed one at its first step.)
                    float tm = fW == 0 && wrapJudged == 0 ? float.NaN : T.magnitude;
                    if (fW != 1)
                    {
                        wrapJudged++;
                        if (float.IsNaN(tm) || float.IsNaN(rDeg)) wrapNaN++;
                        else { wrapTargetMost = Mathf.Max(wrapTargetMost, tm); if (tm > WrapOutsideDeg) wrapViolations++; }
                    }
                }
                if (bReplaced)
                {
                    // (B's own target written back before the engine steps: only the count was shown the long way.)
                    boot.SetDriveTarget(ArticulationDriveAxis.X, bWrote.x); boot.SetDriveTarget(ArticulationDriveAxis.Y, bWrote.y); boot.SetDriveTarget(ArticulationDriveAxis.Z, bWrote.z);
                    T = Target(boot);
                }
                if (!first)
                {
                    float jumpW = (T - lastT).magnitude, jumpT = Quaternion.Angle(FromRV(lastT), FromRV(T)), askT = Quaternion.Angle(lastAsk, ask), askV = (RV(ask) - RV(lastAsk)).magnitude;
                    jumpWrittenMost = Mathf.Max(jumpWrittenMost, jumpW); jumpTurnMost = Mathf.Max(jumpTurnMost, jumpT); askTurnMost = Mathf.Max(askTurnMost, askT);
                    if (jumpT > 60 || jumpW > 60) jumps60++;
                    // (The form before B's step, as written: a vector, so that a flip of the written turn through half a
                    // turn, which a quaternion's angle hides, is a jump.)
                    float preJ = (pre - lastPre).magnitude;
                    preJumpMost = Mathf.Max(preJumpMost, preJ);
                    if (p.Window != null && p.Window(i) && fA != 1)
                    {
                        // (BAR_A's measure: a NaN step counted as a failure; fed one at the window's first step.)
                        if (fA == 0 && winSteps == 0) preJ = float.NaN;
                        winSteps++;
                        float allowed = p.AgainstAsk ? askV + BarAMarginDeg : FoldSlope * askT + BarAMarginDeg, ratio = preJ / allowed;
                        if (float.IsNaN(ratio)) winNaN++;
                        else
                        {
                            if (ratio > winRatioMost) { winRatioMost = ratio; winPreAt = preJ; winAskAt = askT; winAskVecAt = askV; winAt = i; }
                            if (preJ > allowed) winViolations++;
                        }
                    }
                }
                first = false; lastT = T; lastAsk = ask; lastPre = pre; lastR = r; lastV = v;
                if (!float.IsNaN(most)) offEMost = Mathf.Max(offEMost, (T * Mathf.Deg2Rad - r).magnitude / most);
                if (p.AxisSample != null && p.AxisSample(i))
                {
                    Vector3 rel = boot.angularVelocity - shin.angularVelocity;
                    if (rel.magnitude > 1)
                    {
                        Vector3 expectW = shin.transform.rotation * rested[Foot] * stood * p.AxisLocal;
                        float ang = Vector3.Angle(rel, expectW); ang = Mathf.Min(ang, 180 - ang);
                        float toShin = Vector3.Angle(rel, ShinLine()); toShin = Mathf.Min(toShin, 180 - toShin);
                        axisSamples++; axisAngleSum += ang; axisAngleMost = Mathf.Max(axisAngleMost, ang); axisShinMost = Mathf.Max(axisShinMost, toShin);
                    }
                }
                if (i == p.FollowAt && fF != 1)
                {
                    Quaternion actual = Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation;
                    followAsk = Quaternion.Angle(actual, Quaternion.Normalize(ask));
                    followErr = fF == 0 ? float.NaN : Quaternion.Angle(actual, FromRV(T - driven));
                    followRead = true;
                }
                int tk = p.TanhAt != null ? Array.IndexOf(p.TanhAt, i) : -1;
                if (tk >= 0 && !float.IsNaN(most) && fT != 1)
                {
                    Vector3 expect = ResearchTarget(pre, r, drive.forceLimit, drive.stiffness);
                    float m = (pre * Mathf.Deg2Rad - r).magnitude / most;
                    reachMost = Mathf.Max(reachMost, m);
                    if (Mathf.Abs(m - p.TanhShare[tk]) > TanhShareMissed) reachMissed++;
                    // (The lazy forms a check must tell from B's: a hard clamp at E, and no slowing at all.)
                    Vector3 off = pre * Mathf.Deg2Rad - r; float far = off.magnitude;
                    Vector3 clamp = (r + (far > most ? off * (most / far) : off)) * Mathf.Rad2Deg;
                    float err = (T - expect).magnitude, clampDiff = (expect - clamp).magnitude, unslowedDiff = (expect - pre).magnitude;
                    if (fT == 0 && tanhChecks == 0) err = float.NaN;
                    tanhChecks++;
                    if (float.IsNaN(err)) tanhNaN++;
                    else { if (err > BarFormDeg) tanhFails++; tanhErrMost = Mathf.Max(tanhErrMost, err); }
                    if (m >= .99f) { clampDiffLeast = Mathf.Min(clampDiffLeast, clampDiff); if (clampDiff <= BarFormDeg) clampWouldFail++; }
                    unslowedDiffLeast = Mathf.Min(unslowedDiffLeast, unslowedDiff); if (unslowedDiff <= BarFormDeg) unslowedWouldFail++;
                    tanhLines.AppendFormat(Inv, "[asked {0:0.00} E along {1}: e/E reached {2:0.000}, written {3:0.000} E from the reading (tanh {4:0.000}), {5:0.0000} deg from research's form; a hard clamp's form {6:0.000} deg from it, no slowing {7:0.000} deg] ",
                        p.TanhShare[tk], V((pre * Mathf.Deg2Rad - r).normalized, "0.00"), m, (T * Mathf.Deg2Rad - r).magnitude / most, Math.Tanh(m), err, clampDiff, unslowedDiff);
                }
                if (p.Axes && (i + 1) % p.AxesHold == 0 && fX != 1)
                {
                    // (Read against what the riser wrote, before the rig's own writes: with -forcedAxesBad 1 the rig writes
                    // it wrongly after, and the boot must then be found away from it.)
                    Quaternion actual = Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation;
                    Vector3 got = RV(actual), readDeg = r * Mathf.Rad2Deg;
                    Vector3 intended = written;
                    float rvErr = Quaternion.Angle(actual, FromRV(intended)), readErr = (readDeg - intended).magnitude, perm = Quaternion.Angle(actual, FromRV(new Vector3(intended.y, intended.x, intended.z))),
                        eul = Quaternion.Angle(actual, Quaternion.Euler(intended)), readVsGot = (readDeg - got).magnitude;
                    // (BAR_AXES' measure: a NaN hold counted, never lost in a maximum; fed one on its first hold.)
                    if (fX == 0 && axesLines.Count == 0) rvErr = float.NaN;
                    if (!p.AxesRead && (float.IsNaN(rvErr) || float.IsNaN(readErr))) axesNaN++;
                    else { rvErrMost = Mathf.Max(rvErrMost, rvErr); readErrMost = Mathf.Max(readErrMost, readErr); }
                    eulerErrMost = Mathf.Max(eulerErrMost, eul);
                    if (Mathf.Abs(intended.x - intended.y) > 1) permErrLeast = Mathf.Min(permErrLeast, perm);
                    // (The joint's reading against the boot's own turn, as turns: what the reading means, at every angle; the
                    // lazy reading, its X and Y swapped, shown apart from it.)
                    float readTurn = Quaternion.Angle(FromRV(readDeg), actual), readSwapped = Quaternion.Angle(FromRV(new Vector3(readDeg.y, readDeg.x, readDeg.z)), actual);
                    readVsGotMost = Mathf.Max(readVsGotMost, readTurn);
                    if (Mathf.Abs(readDeg.x - readDeg.y) > 5) readSwappedLeast = Mathf.Min(readSwappedLeast, readSwapped);
                    if (p.AxesRead)
                    {
                        float endSpeed = PerAxis(v);
                        bool settled = endSpeed < SettledRadPerS;
                        if (intended.magnitude > 1)
                        {
                            farHolds++;
                            if (settled) { farSettled++; farSettledMost = Mathf.Max(farSettledMost, intended.magnitude); }
                            else farUnsettledLeast = Mathf.Min(farUnsettledLeast, intended.magnitude);
                            // (BAR_AXES_READ: the holds past 110 deg where the boot came to rest, the boot's turn against the
                            // target the riser wrote (apart from the joint's reading: the bodies' turns against the written).)
                            if (settled && intended.magnitude > AxesNearMostDeg && fR != 1)
                            {
                                // (Its measure: a NaN hold counted as a failure, fed one on the first judged hold.)
                                float farErr = fR == 0 && farJudged == 0 ? float.NaN : rvErr;
                                farJudged++; farJudgedMost = Mathf.Max(farJudgedMost, intended.magnitude);
                                if (float.IsNaN(farErr)) farNaN++; else farErrMost = Mathf.Max(farErrMost, farErr);
                                if (Mathf.Abs(intended.x - intended.y) > 1) farSwappedLeast = Mathf.Min(farSwappedLeast, perm);
                                // (A reading beside the bar: the sag the boot's and toe's weight gives about the boot's joint over
                                // the drive's stiffness, read back from the running bodies, in degrees; and what the riser adds
                                // (drivenNow) to the target.)
                                Vector3 anchorW = boot.transform.TransformPoint(boot.anchorPosition), torque = Vector3.zero;
                                foreach (var part in boot.GetComponentsInChildren<ArticulationBody>()) torque += Vector3.Cross(part.worldCenterOfMass - anchorW, part.mass * Physics.gravity);
                                float kNow = boot.xDrive.stiffness, sag = kNow > 0 ? torque.magnitude / kNow * Mathf.Rad2Deg : float.NaN;
                                if (!float.IsNaN(sag)) { sagN++; sagMost = Mathf.Max(sagMost, sag); sagLeast = Mathf.Min(sagLeast, sag); }
                                drivenMost = Mathf.Max(drivenMost, driven.magnitude);
                            }
                        }
                        Debug.Log(string.Format(Inv, "FORCED_AXES_FAR {0} {1}: hold {2}, the riser wrote {3} deg (|{4:0.0}|); at the hold's end the joint reads {5} deg, {6:0.00} rad/s on an axis ({7}); the boot {8:0.00} deg from the written as a turn; the reading {9:0.00} deg from the boot's turn",
                            whoNow, configNow, (i + 1) / p.AxesHold - 1, V(intended, "0.0"), intended.magnitude, V(readDeg, "0.0"), endSpeed, settled ? "settled" : "still turning", rvErr, readTurn));
                    }
                    axesLines.Add(string.Format(Inv, "[the riser wrote {0} deg; the drive holds {1}; the joint reads {2} deg; the boot is turned {3} deg (as a turn, from how it was made): {4:0.00} deg from the written read as a turn, the reading {5:0.00} from the written and {6:0.00} from the boot's turn, {7:0.00} from the written read as Euler angles, {8:0.00} from it with X and Y swapped]",
                        V(intended, "0.00"), V(T, "0.00"), V(readDeg, "0.00"), V(got, "0.00"), rvErr, readErr, readVsGot, eul, perm));
                }
                if (csv != null)
                    csv.WriteLine(string.Join(",", who, config, p.Name, i.ToString(Inv), F(T.x, "0.###"), F(T.y, "0.###"), F(T.z, "0.###"), F(r.x * Mathf.Rad2Deg, "0.###"), F(r.y * Mathf.Rad2Deg, "0.###"), F(r.z * Mathf.Rad2Deg, "0.###"),
                        F(v.x, "0.###"), F(v.y, "0.###"), F(v.z, "0.###"), touched ? "1" : "0", F(drive.stiffness, "0.####"), F(drive.damping, "0.####"), F(drive.forceLimit, "0.####")));
            });
            while (!forcer.Done) yield return null;
            if (flipped) OwnRiser.AnkleLimit = 1;
            p.End?.Invoke();
            if (flipped) Debug.Log(string.Format(Inv, "FORCED_FORM_BAD {0} {1} {2}: -forcedFormBad 1: the riser wrote with AnkleLimit {3} (read back in the profile's second step), the bar expecting A's fold; AnkleLimit {4} after", who, config, p.Name, limitInProfile, OwnRiser.AnkleLimit));
            if (followBad && p.FollowAt > 0) Debug.Log(string.Format(Inv, "FORCED_FOLLOW_BAD {0} {1} {2}: -forcedFollowBad 1: the boot's drive let go on {3} steps (10 to {4}), given back at {5}", who, config, p.Name, followLetGo, p.FollowAt, p.FollowAt + 1));
            bool ended = !forcer.WasLost && forcer.Step == p.Steps;
            if (!ended) { profilesLost++; bars.EventMissed($"{who} {config} {p.Name}: did not run to its end ({forcer.Step} of {p.Steps} steps; the riser lost)"); yield break; }
            profilesRun++;
            var d = boot.xDrive;
            var sb = new StringBuilder();
            sb.AppendFormat(Inv, "FORCED {0} {1} {2}: {3} of {4} steps, ended by itself {5}; switches read back AnkleLimit {6}, AnkleSpeed {7} (Omega here {8:0.00} rad/s), AnkleAtMost {20}, BootsFromOwnShin {9}; the drive holds k {10:0.000} c {11:0.0000} limit {12:0.000} (E {13:0.0} deg); the ask moved at most {14:0.0} deg in a step; the target written moved at most {15:0.0} deg as a turn ({16:0.0} as written), {17} steps over 60; the form before B moved at most {18:0.0} deg as written; touched {19} steps",
                who, config, p.Name, forcer.Step, p.Steps, ended, F(OwnRiser.AnkleLimit, "0.######"), F(OwnRiser.AnkleSpeed, "0.######"), OwnRiser.AnkleSpeed != 0 ? OmegaOf(OwnRiser.AnkleSpeed) : 0, OwnBody.BootsFromOwnShin,
                d.stiffness, d.damping, d.forceLimit, d.stiffness > 0 ? d.forceLimit / d.stiffness * Mathf.Rad2Deg : float.NaN, askTurnMost, jumpTurnMost, jumpWrittenMost, jumps60, preJumpMost, touchedSteps, F(OwnRiser.AnkleAtMost, "0.###"));
            Debug.Log(sb.ToString());
            string tag = who + " " + config + " " + p.Name;
            // BAR_A (checks 1, 2): at the crossing no step where the form before B, as written, moves more than the fold's
            // own slope times the ask's turn, and a degree. Lazy way: a form that never crosses (caught: off fails it).
            // BAR_A_SLERP: where the ask itself jumps (slerp_antipode), A's step no more than the ask's own, as written. A
            // reading, not a bar with a known-bad (November 18): off passes it too (0.995, forced_bad2.log:2042), since
            // today's clamp only follows the ask's own jump, and no setting of the switches makes a form that jumps beyond
            // the ask; a form made up by the rig to jump would try only the bar's sum, which BAR_A's known-bad (off) already
            // tries on the same lines. Lazy way: an allowance as large as the ask's own jump (it is: the ask's step + 1 deg).
            // (Step 4a‴: no bar passes on a NaN or on nothing. Each says how many values it judged and how many were NaN; a
            // NaN fails the bar; fewer judged than its floor is an event missed, and fails the bar too.)
            if (fA >= 0 || fB >= 0 || fF >= 0 || fM >= 0 || fT >= 0 || fW >= 0 || fX >= 0 || fR >= 0)
                Debug.Log(string.Format(Inv, "FORCED_FEED {0}: -forcedFeed {1}: this case fed {2}", tag, feedBar, Mathf.Max(fA, Mathf.Max(fB, Mathf.Max(fF, Mathf.Max(fM, Mathf.Max(fT, Mathf.Max(fW, Mathf.Max(fX, fR))))))) == 0 ? "one NaN into its measure" : "nothing (its values withheld)"));
            if (p.Window != null && judgesA)
            {
                string bar = p.AgainstAsk ? "BAR_A_SLERP" : "BAR_A";
                // (The floor: every step the profile's own window names, counted from the profile, never fewer than
                // LeastWindowSteps. forced_good4 showed the fixed 5 wrong for swing_1800dps, whose window is 4 steps.)
                int winOwn = 0; for (int s = 1; s < p.Steps; s++) if (p.Window(s)) winOwn++;
                int winFloor = Mathf.Max(LeastWindowSteps, winOwn);
                if (winSteps < winFloor) bars.EventMissed($"{tag}: {bar} judged {winSteps} window steps, fewer than the {winFloor} its window names");
                bars.Judge(bar, who, config, p.Name, winSteps >= winFloor && winNaN == 0 && winViolations == 0, string.Format(Inv,
                    "at the crossing ({0} steps, {8} of them NaN) {1} steps over the allowance ({2}); the largest share of it {3:0.000} at step {4} (the form before B moved {5:0.0} deg as written, the ask {6:0.0} as a turn, {7:0.0} as written)",
                    winSteps, winViolations, p.AgainstAsk ? "the ask's own step as written, and 1 deg" : "3.75 times the ask's turn, and 1 deg", winRatioMost, winAt, winPreAt, winAskAt, winAskVecAt, winNaN));
            }
            // BAR_B (checks 1, 4): the boot no faster than Omega on any axis, any step, touching or not (the proof of
            // research.md §4.2 is per axis). Lazy way: a profile that never asks fast (caught: off fails it).
            float fastAll = Mathf.Max(fastFree, fastTouched);
            if (judgesB && p.Name != "beyond")
            {
                if (bSteps < LeastSteps) bars.EventMissed($"{tag}: BAR_B judged {bSteps} steps, fewer than {LeastSteps}");
                bars.Judge("BAR_B", who, config, p.Name, bSteps >= LeastSteps && bNaN == 0 && fastAll <= omegaBar, string.Format(Inv, "fastest {0:0.0} rad/s on an axis (touching nothing {1:0.0} at step {2}, {3:0.0} as a vector; touching {4:0.0}), bound {5:0.00}; {8} steps judged, {9} of them NaN; its shin at most {6:0.0} rad/s; {7}",
                    fastAll, fastFree, fastAt, fastVec, fastTouched, omegaBar, shinFast, fastWhy, bSteps, bNaN));
            }
            else Debug.Log(string.Format(Inv, "FORCED_READ {0}: fastest {1:0.0} rad/s on an axis (touching nothing {2:0.0} at step {3}, {4:0.0} as a vector; touching {5:0.0}); {6}", tag, fastAll, fastFree, fastAt, fastVec, fastTouched, fastWhy));
            // BAR_FOLLOW: the boot within 5 deg of the target written, at the step of the hold's end. Lazy way: a target
            // met by not moving (the boot is asked 60 deg away). Known-bad: -forcedFollowBad 1, the drive let go through
            // the hold, so that the boot hangs where gravity leaves it, about 60 deg from the target.
            if (p.FollowAt > 0)
            {
                if (!followRead) bars.EventMissed($"{tag}: BAR_FOLLOW's step {p.FollowAt} was not read");
                bars.Judge("BAR_FOLLOW", who, config, p.Name, followRead && !float.IsNaN(followErr) && followErr <= BarFollowDeg, string.Format(Inv, "at step {0} the boot {1:0.0} deg from the target written, {2:0.0} from its ask (read {3})", p.FollowAt, followErr, followAsk, followRead));
            }
            // BAR_FORM: on every step, the target the drive holds is the switch's form of the ask (today's clamp; A's fold
            // by the riser's own AnkleFolds; B's slowing written anew from research). Lazy way: a constant or stale target.
            // Known-bad: -forcedFormBad 1 with A, the riser writing today's clamp while the bar expects A's fold.
            if (!p.Axes || !axesBad)
            {
                if (formChecks < LeastSteps) bars.EventMissed($"{tag}: BAR_FORM judged {formChecks} steps, fewer than {LeastSteps}");
                bars.Judge("BAR_FORM", who, config, p.Name, formChecks >= LeastSteps && formNaN == 0 && formFails == 0, string.Format(Inv, "{0} steps, {3} of them NaN, {1} more than 0.05 deg off (most {2:0.0000})", formChecks, formFails, formErrMost, formNaN));
            }
            // BAR_TANH (check 4): where B writes, the target is research's E tanh(|e| / E) form, within 0.05 deg. Lazy
            // ways: a hard clamp at E, and no slowing (both computed and shown to differ); off fails it.
            if (p.TanhAt != null)
            {
                if (reachMissed > 0) bars.EventMissed($"{tag}: {reachMissed} of {tanhChecks} asks did not reach their share of E within 0.03 (most reached {reachMost:0.000})");
                if (clampWouldFail > 0 || unslowedWouldFail > 0) bars.EventMissed($"{tag}: a lazy form is not told from research's: a hard clamp within 0.05 deg on {clampWouldFail} asks, no slowing on {unslowedWouldFail}");
                if (judgesB)
                {
                    if (tanhChecks < p.TanhAt.Length) bars.EventMissed($"{tag}: BAR_TANH compared {tanhChecks} of its {p.TanhAt.Length} asks");
                    bars.Judge("BAR_TANH", who, config, p.Name, tanhChecks == p.TanhAt.Length && tanhNaN == 0 && tanhFails == 0, string.Format(Inv, "{0} compared of {7}, {8} of them NaN, {1} more than 0.05 deg from research's form (most {2:0.0000}); e/E reached at most {3:0.000}; where e >= E a hard clamp's form is at least {4:0.000} deg from research's, no slowing at least {5:0.000} deg; {6}",
                        tanhChecks, tanhFails, tanhErrMost, reachMost, clampDiffLeast, unslowedDiffLeast, tanhLines, p.TanhAt.Length, tanhNaN));
                }
            }
            // BAR_WRAP (check 5): from a reading inside half a turn, B never writes a target outside it (as a vector).
            // Lazy way: a profile that never asks far; its known-bads: the rig's own raw writes, judged alike as
            // "beyond_raw" with -forcedWrapBad 1 (a different count); and, on B's own count and verdict line, -forcedWrapBadB 1.
            if (wrapBadB && judgesWrap)
                Debug.Log(string.Format(Inv, "FORCED_WRAP_BAD_B {0}: -forcedWrapBadB 1: B's written target replaced by the same turn the long way round on {1} steps, read back by BAR_WRAP's count, then written back", tag, wrapBadBWrites));
            if (judgesWrap)
            {
                if (wrapJudged < LeastSteps) bars.EventMissed($"{tag}: BAR_WRAP judged {wrapJudged} steps, fewer than {LeastSteps}");
                bars.Judge("BAR_WRAP", who, config, p.Name, wrapJudged >= LeastSteps && wrapNaN == 0 && wrapViolations == 0, string.Format(Inv, "{0} steps from a reading inside 180 deg, {3} of them NaN, {1} with the target written outside (the largest {2:0.0} deg)", wrapJudged, wrapViolations, wrapTargetMost, wrapNaN));
            }
            if (p.Raw != null)
            {
                Debug.Log(string.Format(Inv, "FORCED_WRAP_KNOWN_BAD {0}: the rig's own raw writes outside 180 deg from a reading inside it, counted as BAR_WRAP counts: {1} steps", tag, rawOutside));
                if (rawOutside == 0) bars.EventMissed($"{tag}: the raw writes were not counted outside 180 deg: BAR_WRAP's counter is not shown to catch them");
                // (BAR_WRAP's known-bad, -forcedWrapBad 1: the raw writes judged as BAR_WRAP judges B's writes, as
                // "beyond_raw": a writer that writes outside half a turn from a reading inside it. It must fail.)
                if (wrapBad)
                    bars.Judge("BAR_WRAP", who, config, "beyond_raw", rawOutside == 0, string.Format(Inv, "-forcedWrapBad 1: the rig's own raw writes judged as B's: {0} steps from a reading inside 180 deg with the target written outside", rawOutside));
            }
            // BAR_AXES (check 6): at each hold's end the boot's turn within 1 deg of the target written read as a turn,
            // and the joint's reading within 1 deg of it; on targets out to 110 deg (past it: axes_far, a reading, below).
            // Lazy way: targets that never leave nothing (they go to 110 deg, on one, two and three axes).
            if (p.Axes && !p.AxesRead)
            {
                int holds = p.Steps / p.AxesHold;
                if (axesLines.Count < holds) bars.EventMissed($"{tag}: BAR_AXES read {axesLines.Count} of its {holds} holds");
                bars.Judge("BAR_AXES", who, config, p.Name, axesLines.Count == holds && axesNaN == 0 && rvErrMost <= BarAxesDeg && readErrMost <= BarAxesDeg, string.Format(Inv, "{0} holds of {7}, {8} of them NaN{1}: the boot's turn at most {2:0.00} deg from the target the riser wrote, read as a turn; the joint's reading at most {3:0.00} from it; read as Euler angles at most {4:0.00}; with X and Y swapped at least {5:0.00}; {6}",
                    axesLines.Count, p.After != null ? " (-forcedAxesBad 1: the rig wrote each wrongly after the riser)" : "", rvErrMost, readErrMost, eulerErrMost, permErrLeast, string.Join(" ", axesLines), holds, axesNaN));
            }
            // (The joint's reading against the boot's own turn: 0.00 deg on every hold of the second build, a quantity read
            // against itself (the engine's transforms are made from its reading), so a reading, not a bar (November 18).)
            if (p.Axes)
                Debug.Log(string.Format(Inv, "FORCED_READ {0}: {1} holds: the joint's reading at most {2:0.00} deg from the boot's turn (the engine's own consistency, a reading); with X and Y swapped at least {3:0.00}", tag, axesLines.Count, readVsGotMost, readSwappedLeast));
            // BAR_AXES_READ (check 6, past 110 deg; rewritten November 18 against the target written, apart from the
            // reading): on axes_far, at the end of each hold past 110 deg where the boot came to rest (under 1 rad/s), the
            // boot's turn (the bodies' transforms) within 3 deg of the target the riser wrote, read as a turn (3, not 1:
            // settled holds of 130-145 deg sag 1.2-1.9 deg in forced_bad2). Known-bad: axes_far_bad (-forcedAxesBad 1, the
            // target written again after the riser with X and Y swapped). Lazy ways: judging only holds that never come to
            // rest past 110 (answered: at least 5 judged, the count and the largest said), or a bar too wide to catch a
            // wrong axis (answered: with X and Y swapped the judged holds are said at least so far away).
            if (p.AxesRead)
            {
                if (farJudged < AxesReadLeastHolds) bars.EventMissed($"{tag}: only {farJudged} holds past 110 deg came to rest: BAR_AXES_READ has too little to judge");
                bars.Judge("BAR_AXES_READ", who, config, p.Name, farJudged >= AxesReadLeastHolds && farNaN == 0 && farErrMost <= BarAxesReadDeg, string.Format(Inv, "{0} holds past 110 deg came to rest, {5} of them NaN (the largest |{1:0.0}| deg){2}: the boot's turn at most {3:0.00} deg from the target the riser wrote, read as a turn; with X and Y swapped at least {4:0.00}",
                    farJudged, farJudgedMost, p.After != null ? " (-forcedAxesBad 1: the rig wrote each again with X and Y swapped after the riser)" : "", farErrMost, farSwappedLeast, farNaN));
                Debug.Log(string.Format(Inv, "FORCED_AXES_SAG {0}: on the {1} judged holds, the sag the boot's and toe's weight gives about its joint over the drive's stiffness (|r x m g| / k) {2:0.00} to {3:0.00} deg; the riser's own addition to the target (drivenNow) at most {4:0.00} deg; the bar {5:0} deg, the boot's turn from the written at most {6:0.00} deg (a reading)",
                    tag, sagN, sagN > 0 ? sagLeast : float.NaN, sagMost, drivenMost, BarAxesReadDeg, farErrMost));
            }
            if (p.AxesRead)
                Debug.Log(string.Format(Inv, "FORCED_AXES_FAR_SAID {0}: of {1} holds at a target past nothing, {2} settled (under 1 rad/s at the hold's end), the largest settled |{3:0.0}| deg; the smallest still turning |{4:0.0}| deg", tag, farHolds, farSettled, farSettledMost, farUnsettledLeast));
            if (axisSamples > 0)
                Debug.Log(string.Format(Inv, "FORCED_TWIST_AXIS {0}: over {1} steps of the turn the boot turned against its shin about a line {2:0.0} deg from the ask's own axis on average ({3:0.0} at most), {4:0.0} deg from the shin's own line at most", tag, axisSamples, axisAngleSum / axisSamples, axisAngleMost, axisShinMost));
        }

        // Check 5 (a reading, its mechanism read back): the boot whirled past half a turn with its drive off, then its drive
        // given back and asked again (B writes the reading plus e', which can be outside [-180, 180]); then, each from the
        // boot brought back to how it stands, targets written raw: 250, -250, 150, 190, 210 deg on its first axis.
        private Prof Beyond()
        {
            var lines = new StringBuilder();
            float readMost = float.NegativeInfinity; int wraps = 0; float lastRead = float.NaN, targetMost = 0; int outside = 0;
            Vector3 endHold = Vector3.zero, gotHold = Vector3.zero;
            float[] rawAt = { 250, -250, 150, 190, 210 };
            const int first = 155, block = 110, rawFor = 65;
            float segLast = float.NaN, segMin = 0, segMax = 0, segSpeedSum = 0; int segWraps = 0, segAtLimit = 0, segSpeedN = 0; Vector3 segFrom = Vector3.zero;
            var trace = new StringBuilder();
            var p = new Prof { Name = "beyond", Steps = first + block * rawAt.Length };
            p.Raw = i => i >= first && (i - first) % block < rawFor;
            p.Ask = i => i >= 50 && i < 110 ? FromRV(new Vector3(170, 0, 0)) : stood;
            p.Before = i =>
            {
                if (i == 10) DrivesOff(boot);
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
                if (i == 109)
                {
                    endHold = Reading(boot) * Mathf.Rad2Deg; gotHold = RV(Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation);
                    lines.AppendFormat(Inv, "in the 60 steps so asked the target written reached {0:0.0} deg on an axis, {1} steps outside [-180, 180]; after them the joint reads {2} deg, the boot turned {3} deg", targetMost, outside, V(endHold), V(gotHold));
                }
                if (i < first) return;
                int k = (i - first) / block, j = (i - first) % block;
                var drive = boot.xDrive;
                if (j < rawFor)
                {
                    boot.SetDriveTarget(ArticulationDriveAxis.X, rawAt[k]); boot.SetDriveTarget(ArticulationDriveAxis.Y, 0); boot.SetDriveTarget(ArticulationDriveAxis.Z, 0);
                    // (The step just taken: its reading, its speed, the torque the drive gave, read back from the joint.)
                    float x = Reading(boot).x, w = Speeds(boot).x, tq = boot.driveForce.dofCount > 0 ? boot.driveForce[0] : float.NaN;
                    if (j == 0) { segFrom = Reading(boot) * Mathf.Rad2Deg; segLast = x; segMin = segMax = x; segWraps = 0; segAtLimit = 0; segSpeedSum = 0; segSpeedN = 0; trace.Clear(); }
                    else
                    {
                        if (Mathf.Abs(x - segLast) > Mathf.PI) segWraps++;
                        segLast = x; segMin = Mathf.Min(segMin, x); segMax = Mathf.Max(segMax, x);
                        if (Mathf.Abs(tq) >= .98f * drive.forceLimit) segAtLimit++;
                        if (j >= rawFor - 20) { segSpeedSum += Mathf.Abs(w); segSpeedN++; }
                    }
                    if (j % 8 == 1) trace.AppendFormat(Inv, " [{0}: reads {1:0.0} deg, {2:0.0} rad/s, the drive's torque {3:0.0} N m]", j, x * Mathf.Rad2Deg, w, tq);
                    if (j == rawFor - 1)
                        Debug.Log(string.Format(Inv, "FORCED_BEYOND_RAW {0} {1} written raw ({2:0} 0 0) for {3} steps from a joint reading {4} deg: it reads {5} deg at the end, the boot turned {6} deg, {7:0.00} rad/s on an axis at the end ({8:0.00} on average over the last 20 steps on its first axis); the reading spanned {9:0.0} to {10:0.0} deg on its first axis, {11} wraps; the drive gave its whole limit ({12:0.0} N m) on {13} of {14} steps; the drive's own terminal speeds: limit / c {15:0.0}, limit / (c + k dt) {16:0.0} rad/s; read every 8 steps:{17}",
                            whoNow, configNow, rawAt[k], rawFor, V(segFrom), V(Reading(boot) * Mathf.Rad2Deg), V(RV(Quaternion.Inverse(shin.transform.rotation * rested[Foot]) * boot.transform.rotation)), PerAxis(Speeds(boot)), segSpeedN > 0 ? segSpeedSum / segSpeedN : float.NaN,
                            segMin * Mathf.Rad2Deg, segMax * Mathf.Rad2Deg, segWraps, drive.forceLimit, segAtLimit, rawFor - 1, drive.forceLimit / Mathf.Max(1e-6f, drive.damping), drive.forceLimit / Mathf.Max(1e-6f, drive.damping + drive.stiffness * Time.fixedDeltaTime), trace));
                }
                if (j == block - 1)
                    Debug.Log(string.Format(Inv, "FORCED_BEYOND_RESET {0} {1}: after {2} steps asked as it stands, the joint reads {3} deg, {4:0.00} rad/s on an axis", whoNow, configNow, block - rawFor, V(Reading(boot) * Mathf.Rad2Deg), PerAxis(Speeds(boot))));
                if (i == p.Steps - 1) Debug.Log("FORCED_BEYOND " + whoNow + " " + configNow + ": " + lines);
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
                Name = name, Steps = 85, FollowAt = 34, Window = i => i >= 60, AxisLocal = axis,
                Ask = i => At(axis, i < 10 ? 0 : i < 35 ? 60 : i < 60 ? 179 : 181),
            };
            // (Held at nothing, at 60, then turned at so many degrees a second to 300, and held.)
            Prof Ramp(string name, Vector3 axis, float perSecond)
            {
                int turning = Mathf.CeilToInt(240 / (perSecond * .02f));
                float Angle(int i) => i < 10 ? 0 : i < 35 ? 60 : Mathf.Min(300, 60 + perSecond * .02f * (i - 35));
                return new Prof
                {
                    Name = name, Steps = 35 + turning + 25, FollowAt = 34, AxisLocal = axis,
                    Window = i => { float a = Angle(i); float before = Angle(Mathf.Max(0, i - 10)); return (a >= 150 && a <= 210) || (before >= 150 && before <= 210); },
                    AxisSample = i => { float a = Angle(i); return a > 62 && a < 140; },
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
                // (Check 2: a pure twist about the shin's own line, read off its knee and ankle; and a turn half twist,
                // half swing.)
                list.Add(Instant("twist_instant", twistAxis));
                list.Add(Ramp("twist_360dps", twistAxis, 360));
                list.Add(Instant("mixed_instant", mixedAxis));
                list.Add(Ramp("mixed_360dps", mixedAxis, 360));
                // (The Slerp from aimFrom, as OwnRiser.Step has it at a pose's middle: from how it stands to a pose turned
                // 170 to 190 degrees from it, a degree every three steps; its shortest way flips at 180.)
                float Dest(int i) => i < 10 ? 170 : Mathf.Min(190, 170 + 20f * (i - 10) / 60);
                list.Add(new Prof
                {
                    Name = "slerp_antipode", Steps = 95, AgainstAsk = true,
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
                // (Check 6: written targets about one axis, two and three, from how it was made (AnkleAtMost 0 for the
                // profile, so that today's clamp does not cut them), out to 178 degrees; each held 45 steps.)
                // (The first run of the mended check, forced_bad, had them to 178 deg in one profile: past 110 the boot never
                // settled (forced_bad.log:1935). Now: out to 110 under BAR_AXES; past it, axes_far, a reading: the large ones,
                // and a sweep from 115 to 170 deg about each axis and two slants, each from nothing (a hold at nothing before).)
                var near = new[] { new Vector3(40, 0, 0), new Vector3(0, 40, 0), new Vector3(0, 0, 40), new Vector3(30, 30, 0), new Vector3(30, 0, 30), new Vector3(0, 30, 30), new Vector3(30, 30, 30), new Vector3(-50, 20, -30), new Vector3(70, -40, 20),
                    new Vector3(110, 0, 0), new Vector3(0, -110, 0), new Vector3(0, 0, 110), new Vector3(-60, 60, 60) };
                var far = new List<Vector3>();
                foreach (var big in new[] { new Vector3(0, -150, 0), new Vector3(0, 0, 175), new Vector3(100, -100, 50), new Vector3(-120, 60, 90), new Vector3(0, 120, -120), new Vector3(-178, 0, 0), new Vector3(60, 160, 0) }) { far.Add(Vector3.zero); far.Add(big); }
                foreach (var dir in new[] { Vector3.right, Vector3.up, Vector3.forward, new Vector3(1, 1, 1).normalized, new Vector3(1, -1, 0).normalized })
                    foreach (float size in new float[] { 115, 120, 125, 130, 135, 140, 145, 150, 160, 170 }) { far.Add(Vector3.zero); far.Add(size * dir); }
                const int hold = 45;
                Prof Axes(string name, bool bad, Vector3[] asked, bool read, bool swapAll = false) => new Prof
                {
                    Name = name, Steps = hold * asked.Length, Axes = true, AxesRead = read, AxesHold = hold,
                    Start = () => OwnRiser.AnkleAtMost = 0, End = () => OwnRiser.AnkleAtMost = atMostWas,
                    Ask = i => FromRV(asked[i / hold]),
                    // (The known-bad: what the riser wrote, written again wrongly: X and Y swapped on even holds, the turn's
                    // Euler angles on odd ones.)
                    After = !bad ? (Action<int>)null : i =>
                    {
                        Vector3 T = Target(boot), wrong;
                        if (swapAll || (i / hold) % 2 == 0) wrong = new Vector3(T.y, T.x, T.z);
                        else { Vector3 e = FromRV(T).eulerAngles; wrong = new Vector3(e.x > 180 ? e.x - 360 : e.x, e.y > 180 ? e.y - 360 : e.y, e.z > 180 ? e.z - 360 : e.z); }
                        boot.SetDriveTarget(ArticulationDriveAxis.X, wrong.x); boot.SetDriveTarget(ArticulationDriveAxis.Y, wrong.y); boot.SetDriveTarget(ArticulationDriveAxis.Z, wrong.z);
                    },
                };
                list.Add(Axes("axes", false, near, false));
                if (axesBad) list.Add(Axes("axes_bad", true, near, false));
                list.Add(Axes("axes_far", false, far.ToArray(), true));
                // (BAR_AXES_READ's known-bad (November 18): the far holds, each written again with X and Y swapped.)
                if (axesBad) list.Add(Axes("axes_far_bad", true, far.ToArray(), true, true));
            }
            if (config == "off" || config == "B" || config == "edge")
            {
                // (Check 4: from the joint held at -s E / 2 along a line, an ask at +s E / 2 along it: e = s E from where the
                // joint is, s from a quarter to two, along the joint's first axis, the other way along its second, and
                // aslant; AnkleAtMost 0 for the profile, so that today's clamp does not cut the ask. B alone (or off, its
                // known-bad), so that what is slowed is today's form of the ask.)
                var entries = new List<(float s, Vector3 dir)>();
                foreach (float s in new[] { .25f, .5f, 1, 1.5f, 2 }) entries.Add((s, Vector3.right));
                foreach (float s in new[] { .25f, .5f, 1, 1.5f, 2 }) entries.Add((s, Vector3.down));
                Vector3 slant = new Vector3(1, 1, 1).normalized;
                entries.Add((1.5f, slant)); entries.Add((2, slant)); entries.Add((2, -slant));
                const int each = 32, holdFor = 30;
                var at = new int[entries.Count]; var shares = new float[entries.Count];
                for (int k = 0; k < entries.Count; k++) { at[k] = 10 + each * k + holdFor; shares[k] = entries[k].s; }
                Vector3 askedNow = Vector3.zero;
                float ED() { var d = boot.xDrive; return d.forceLimit / d.stiffness * Mathf.Rad2Deg; }
                var p = new Prof { Name = "tanh", Steps = 10 + each * entries.Count + 10, TanhAt = at, TanhShare = shares,
                    Start = () => OwnRiser.AnkleAtMost = 0, End = () => OwnRiser.AnkleAtMost = atMostWas };
                p.Before = i =>
                {
                    int k = Array.IndexOf(at, i);
                    if (k < 0) return;
                    Vector3 r = Reading(boot) * Mathf.Rad2Deg;
                    askedNow = r + entries[k].s * ED() * entries[k].dir;
                    if (askedNow.magnitude > 179.5f) Debug.Log(string.Format(Inv, "FORCED_TANH_TOO_FAR {0} {1}: asked {2} deg, outside half a turn as written", whoNow, configNow, V(askedNow)));
                };
                p.Ask = i =>
                {
                    if (i < 10) return stood;
                    int k = (i - 10) / each, j = (i - 10) % each;
                    if (k >= entries.Count) return stood;
                    return j < holdFor ? FromRV(-.5f * entries[k].s * ED() * entries[k].dir) : FromRV(askedNow);
                };
                list.Add(p);
            }
            if (config == "off" || config == "B" || config == "AB") list.Add(Beyond());
            string only = CaptureTools.Argument("-forcedProfiles");
            if (!string.IsNullOrEmpty(only)) { var keep = new HashSet<string>(only.Split(',')); list.RemoveAll(q => !keep.Contains(q.Name)); }
            return list;
        }

        // Checks 1, 2, 4, 5 and 6 on the rig, for each miner (-liveWho; all three if left out) and each set of switches
        // (-forcedConfigs, of off, A, B, AB, edge; all five if left out; off alone is the known-bad of A and B: run it with
        // -forcedMustFail BAR_A,BAR_B,BAR_TANH,BAR_DAMP). -forcedProfiles keeps only the profiles named; -forcedAxesBad 1
        // adds axes_bad, the known-bad of check 6 (with -forcedMustFail ...,BAR_AXES@axes_bad).
        [UnityTest, Explicit, Timeout(7200000)]
        public IEnumerator TheBootForced()
        {
            Assert.That(aimsM != null && foldsM != null && driveM != null && toStoodF != null && restedF != null && slackNowF != null && lastStepF != null && tallF != null && lighterF != null && drivenNowF != null && RiserClockHeld.GotF != null,
                Is.True, "OwnRiser lacks what the forced checks reach for.");
            Time.captureFramerate = 50;
            PhysicalFall.GetsUpByItsOwn = true;
            LogAssert.ignoreFailingMessages = true;
            var mustFail = ForcedBars.MustFail();
            axesBad = CaptureTools.Argument("-forcedAxesBad") == "1";
            formBad = CaptureTools.Argument("-forcedFormBad") == "1";
            followBad = CaptureTools.Argument("-forcedFollowBad") == "1";
            wrapBad = CaptureTools.Argument("-forcedWrapBad") == "1";
            wrapBadB = CaptureTools.Argument("-forcedWrapBadB") == "1";
            feedBar = CaptureTools.Argument("-forcedFeed") ?? ""; feedCount = 0;
            string whoArg = CaptureTools.Argument("-liveWho");
            string[] whos = string.IsNullOrEmpty(whoArg) ? new[] { "Small", "Long", "Round" } : whoArg.Split(',');
            int miners = 0;
            try
            {
                Debug.Log("FORCED_SET " + AnkleMendArguments.Apply() + "; must fail: " + (mustFail.Count == 0 ? "none" : string.Join(",", mustFail)) + "; axes known-bad " + axesBad + ", form known-bad " + formBad + ", follow known-bad " + followBad + ", wrap known-bad " + wrapBad + ", wrap known-bad on B's own count " + wrapBadB + ", fed " + (feedBar == "" ? "nothing" : feedBar) + "; " + BarsSaid());
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
                string configArg = CaptureTools.Argument("-forcedConfigs");
                string[] configs = string.IsNullOrEmpty(configArg) ? new[] { "off", "A", "B", "AB", "edge" } : configArg.Split(',');
                foreach (string who in whos)
                {
                    yield return Stand(who);
                    yield return PushedDown(who);
                    if (own.Riser == null) { bars.EventMissed($"{who}: not made down to get up by its own body within 40 s"); yield return TakenBack(); continue; }
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
                    // (The shin's own long line, knee to ankle, in the frame of the boot as it stands: the twist axis (check
                    // 2), found apart from AnkleFolds' expression (the shin's local up), against which it is read.)
                    twistAxis = (Quaternion.Inverse(shin.transform.rotation * rested[Foot] * stood) * ShinLine()).normalized;
                    Vector3 foldsLine = (Quaternion.Inverse(rested[Foot] * stood) * Vector3.up).normalized;
                    float lineApart = Vector3.Angle(twistAxis, foldsLine); lineApart = Mathf.Min(lineApart, 180 - lineApart);
                    swingAxis = (Vector3.right - Vector3.Dot(Vector3.right, twistAxis) * twistAxis).normalized;
                    obliqueAxis = (swingAxis + Vector3.Cross(twistAxis, swingAxis).normalized).normalized;
                    mixedAxis = (twistAxis + swingAxis).normalized;
                    clock.Riser = riser;
                    var hips = own.Part(OwnBody.Hips);
                    float hipsWas = hips.transform.position.y;
                    try
                    {
                        hips.immovable = true;
                        hips.TeleportRoot(hips.transform.position + Vector3.up * 1.5f, hips.transform.rotation);
                        yield return Wait(.6f);
                        miners++;
                        Debug.Log(string.Format(Inv, "FORCED_PINNED {0}: the hips at {1:0.000} m before, {2:0.000} m after the lift (immovable {3}); the left boot at {4:0.000} m, its toe at {5:0.000} m",
                            who, hipsWas, hips.transform.position.y, hips.immovable, boot.transform.position.y, toe != null ? toe.transform.position.y : float.NaN));
                        Debug.Log(string.Format(Inv, "FORCED_MADE {0}: riser's height {1:0.000000} m; |toStood| of the left boot {2:0.0} deg; the shin's own line (knee to ankle) in the boot as it stands {3}, AnkleFolds' line (the shin's local up) {4}: {5:0.00} deg apart; the swing axis {6}; the mixed axis {7}; stage {8}; lighter {9:0.000000} kg m2; {10}",
                            who, tall, Quaternion.Angle(Quaternion.identity, stood), V(twistAxis, "0.000"), V(foldsLine, "0.000"), lineApart, V(swingAxis, "0.000"), V(mixedAxis, "0.000"), riser.StageNow, ((float[])lighterF.GetValue(riser))[Foot], AnkleMendArguments.ReadBack()));
                        foreach (string config in configs)
                        {
                            if (own.Riser != riser) break;
                            float share = ((float[])slackNowF.GetValue(riser))[Foot];
                            // (Today's damper and gains, B off, as the edge case and BAR_DAMP are set from them.)
                            OwnRiser.AnkleLimit = 0; OwnRiser.AnkleSpeed = 0;
                            driveM.Invoke(riser, new object[] { Foot, share });
                            var today = boot.xDrive;
                            float dt = Time.fixedDeltaTime;
                            float omegaEdge = today.forceLimit / (today.damping + today.stiffness * dt);
                            float omegaB = OmegaOf(20);
                            OwnRiser.AnkleLimit = config == "A" || config == "AB" ? 1 : 0;
                            OwnRiser.AnkleSpeed = config == "B" || config == "AB" ? 20 : config == "edge" ? omegaEdge / Mathf.Sqrt(OwnRiser.AnkleSpeedHeight / tall) : 0;
                            driveM.Invoke(riser, new object[] { Foot, share });
                            var now = boot.xDrive;
                            float bound = config == "edge" ? omegaEdge : omegaB;
                            whoNow = who; configNow = config; profNow = "config";
                            Debug.Log(string.Format(Inv, "FORCED_CONFIG {0} {1}: {2}; slack share {3:0.###}; step {4:0.####} s (the riser's own {5:0.####}); today's gains k {6:0.000} c {7:0.0000} limit {8:0.000} (E {9:0.0} deg); the drive now holds k {10:0.000} c {11:0.0000} limit {12:0.000} on X, c {13:0.0000} on Y, {14:0.0000} on Z; the edge's Omega {15:0.00} rad/s, B's at 20 {16:0.00}; the bound read here {17:0.00}",
                                who, config, AnkleMendArguments.ReadBack(), share, dt, (float)lastStepF.GetValue(riser), today.stiffness, today.damping, today.forceLimit, today.forceLimit / today.stiffness * Mathf.Rad2Deg, now.stiffness, now.damping, now.forceLimit, boot.yDrive.damping, boot.zDrive.damping, omegaEdge, omegaB, bound));
                            // BAR_DAMP (check 4): the damping held is research's smooth maximum for the Omega the config
                            // means (B's at 20 for off, its known-bad). Lazy way: a plain maximum (shown apart below).
                            if (config != "A")
                            {
                                double want = ResearchDamper(today.damping, today.forceLimit, today.stiffness, dt, bound);
                                double plain = Math.Max(today.damping, Math.Max(0, today.forceLimit / bound - today.stiffness * dt));
                                // (-forcedFeed BAR_DAMP: its first case judged on a NaN in place of X's damping; its second, a
                                // single value with no set to withhold, as it is.)
                                int fD = FeedCase("BAR_DAMP", true);
                                double cX = fD == 0 ? double.NaN : now.damping, cY = boot.yDrive.damping, cZ = boot.zDrive.damping;
                                if (fD >= 0) Debug.Log($"FORCED_FEED {who} {config} config: -forcedFeed BAR_DAMP: this case fed {(fD == 0 ? "one NaN (X's damping)" : "as it is (one value, nothing to withhold)")}");
                                bool nanD = double.IsNaN(cX) || double.IsNaN(cY) || double.IsNaN(cZ) || double.IsNaN(want);
                                bool same = !nanD && Math.Abs(cX - want) <= BarDampShare * Math.Max(1, want) && Math.Abs(cY - want) <= BarDampShare * Math.Max(1, want) && Math.Abs(cZ - want) <= BarDampShare * Math.Max(1, want);
                                bars.Judge("BAR_DAMP", who, config, "config", same, string.Format(Inv, "the drive holds c {0:0.0000} (X), {1:0.0000} (Y), {2:0.0000} (Z); research's smooth maximum for Omega {3:0.00} is {4:0.0000} (the plain maximum {5:0.0000}, {6:0.0}% apart; today's {7:0.0000}); a NaN among them {8}",
                                    cX, cY, cZ, bound, want, plain, 100 * Math.Abs(want - plain) / want, today.damping, nanD));
                            }
                            foreach (var p in Profiles(config))
                            {
                                if (own.Riser != riser) { bars.EventMissed($"{who} {config}: the riser was lost before {p.Name}"); break; }
                                yield return Run(who, config, p, bound);
                            }
                        }
                    }
                    finally
                    {
                        OwnRiser.AnkleLimit = limitWas; OwnRiser.AnkleSpeed = speedWas; OwnRiser.AnkleAtMost = atMostWas;
                        if (own != null && own.Riser == riser && riser != null) driveM.Invoke(riser, new object[] { Foot, ((float[])slackNowF.GetValue(riser))[Foot] });
                        clock.Riser = null;
                        if (hips != null) hips.immovable = false;
                        foreach (var (a, b) in ignored) if (a != null && b != null) Physics.IgnoreCollision(a, b, false);
                    }
                    Debug.Log($"FORCED_MINER_DONE {who}: {profilesRun} profiles run to their end so far, {profilesLost} lost; {AnkleMendArguments.ReadBack()}");
                    yield return TakenBack();
                }
            }
            finally
            {
                SwitchesBack();
                if (csv != null) { csv.Close(); csv = null; }
            }
            Debug.Log($"FORCED_DONE {miners} of {whos.Length} miners on the rig; {profilesRun} profiles run to their end, {profilesLost} lost");
            var problems = bars.Verdict(mustFail, whos);
            Assert.That(miners, Is.EqualTo(whos.Length), "Not every miner was made down on the rig.");
            Assert.That(problems, Is.Empty, "FORCED bars: " + string.Join(" | ", problems.GetRange(0, Math.Min(8, problems.Count))));
        }

        // ---- Check 3: a going down while the boot whirls (C) ----

        // Each miner (-liveWho; all three if left out; Small's ways are given to every miner, a rig) is made down by a push,
        // and then, getting up, made to go down: once plainly; once with its left boot whirled; once whipped (the boot so,
        // its knee and hip spun too). The whirl: once the boot and its toe have touched nothing for two steps (2 s at
        // most), its drive is let go (stiffness and damping 0, again each step) and its joint set spinning at -whirlSpeed
        // (65 rad/s) on its first axis (the whipped knee at 25 and hip at 15, on the first set only); the engine's own
        // speed is read the step after, and the body is made to go down (OwnBody.GoDown) in the step it reads 50-80 rad/s,
        // else the boot is set again (at most 30 times; see below). At each make after, each boot's ankle
        // against where the body made standing had it.
        // (November 18) Four whirled courses per miner and setting: the boot whirled about its joint's first, second and
        // third axis, and whipped (first axis, its knee and hip spun); and the plain make.
        // BAR_C_PIVOT (every make, the left boot): what the let-go body turned the boot about as it went slack, read from
        // the let-go boot's own transform against its shin, at the going down and when the slack is done: the boot's own
        // point that sat at the let-go shin's end (S) and its point at the own body's ankle (A). C turns it about S, so S
        // stays; off turns it about A, so A stays. Pass: S moved no more than a quarter of what A moved. Judged only when
        // the slack ran to its end and moved one of them at least 1 cm (else nothing tells the pivots apart: said, not
        // judged; fewer than 3 whirled courses judged on a miner is an event missed). Beside it, the move both pivots
        // predict, 2 |S - A| sin(turn / 2). Known-bad: C off with -forcedMustFail BAR_C_PIVOT (it must fail on every case
        // judged). Lazy ways: a slack that does not turn (not judged, counted), a gap |S - A| of nothing (the same).
        // READ_C (a reading, no longer a bar: down_c0e, C off, Long and Round whirled 0.0141 and 0.0148 m passed its 0.025
        // m, and down_c1, C on, Round whipped failed it at 0.0398 m): where the let-go body had the boot's ankle before
        // any refit against where the standing body had it, and after the make. READ_C_PLAIN, the same on the plain make
        // (a reading: nothing in A, B or C claims to bound the off path's own let-go offset, so no run fails it for a reason
        // of its own; it reached 0.0258 m with C off, down_c0d.log:1729).
        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator TheWhirlingGoingDown()
        {
            Assert.That(goDownM != null && bootShiftF != null && stoodAnkleF != null && lighterF != null && ankleAtF != null && bootsMadeF != null && slackenedF != null, Is.True, "OwnBody or PhysicalFall lacks what the forced going down reaches for.");
            Assert.That(AnkleWatch.Reads(out string unread), Is.True, unread);
            Time.captureFramerate = 50;
            PhysicalFall.GetsUpByItsOwn = true;
            LogAssert.ignoreFailingMessages = true;
            var mustFail = ForcedBars.MustFail();
            string barArg = CaptureTools.Argument("-barC"), speedArg = CaptureTools.Argument("-whirlSpeed");
            float barC = string.IsNullOrEmpty(barArg) ? .025f : float.Parse(barArg, Inv), whirlSpeed = string.IsNullOrEmpty(speedArg) ? 65 : float.Parse(speedArg, Inv);
            string whoArg = CaptureTools.Argument("-liveWho");
            string[] whos = string.IsNullOrEmpty(whoArg) ? new[] { "Small", "Long", "Round" } : whoArg.Split(',');
            int makesRead = 0, makesMissed = 0;
            try
            {
                string feedC = CaptureTools.Argument("-forcedFeed") ?? "";
                bool feedsPivot = feedC == "BAR_C_PIVOT";
                Debug.Log("FORCED_SET " + AnkleMendArguments.Apply() + "; must fail: " + (mustFail.Count == 0 ? "none" : string.Join(",", mustFail)) + string.Format(Inv,
                    "; bars judged now: BAR_C_PIVOT (S moved at most {2:0.00} of what A moved; judged where the slack ran to its end and moved a point at least {3:0.000} m; at least {4} whirled courses judged per miner; a NaN read fails it); READ_C and READ_C_PLAIN readings (the old bar's {0:0.000} m said beside, judging nothing); fed {5}; the whirl set at {1:0.0} rad/s at first, and gone down in the step the engine reads 50-80",
                    barC, whirlSpeed, PivotShare, PivotMovedLeastM, PivotLeastCourses, feedsPivot ? "BAR_C_PIVOT (the first make read is fed a NaN, every later make's slack withheld)" : "nothing"));
                var text = Resources.Load<TextAsset>("RiseWays/Small");
                Assert.That(text, Is.Not.Null, "No Resources/RiseWays/Small.txt.");
                OwnBody.RiseWays = OwnRiser.Read(text.text);
                Debug.Log("FORCED_RIG every miner given Small's ways up (" + OwnBody.RiseWays.Length + " ways): a rig, not the game; " + AnkleMendArguments.ReadBack());
                string cName = "C " + (OwnBody.BootsFromOwnShin ? "on" : "off");
                foreach (string who in whos)
                {
                    whoNow = who;
                    yield return Stand(who);
                    var standAnchor = new[] { own.Part(Foot).anchorPosition, own.Part(Foot + 1).anchorPosition };
                    var stoodAnkle = (Vector3[])stoodAnkleF.GetValue(own);
                    var solved = unit.GetComponent<MinerBody>().Solved;
                    Debug.Log(string.Format(Inv, "FORCED_STANDING {0}: its boots' ankles in the body made standing {1} {2} m; OwnBody's stoodAnkle {3} {4} m", who, V(standAnchor[0], "0.0000"), V(standAnchor[1], "0.0000"), V(stoodAnkle[0], "0.0000"), V(stoodAnkle[1], "0.0000")));
                    watch = new GameObject("Ankle watch").AddComponent<AnkleWatch>();
                    watch.Own = own; watch.Who = who;
                    yield return Wait(.3f);
                    yield return PushedDown(who);
                    // (Step 4a‴: nine whirled courses per miner and setting, each differing from the others in the whirl's axis
                    // (the joint's first, second or third), its sense, or the moment of the going down (0.6, 1 or 1.5 s after the
                    // riser began); the whipped course spins the knee and hip too. A course whose riser ended by itself before
                    // the going down (its own body went down while the rig waited for the boot to touch nothing:
                    // down_c1e.log:2787-2800) is tried again on the next getting up, at most 3 times.)
                    string[] kinds = { "after the panel's push", "plain", "boot whirled", "boot whirled about its second axis", "boot whirled about its third axis", "whipped",
                        "boot whirled the other way", "boot whirled about its second axis the other way", "boot whirled about its third axis the other way", "boot whirled sooner (0.6 s)", "boot whirled about its second axis later (1.5 s)" };
                    int[] axisOf = { 0, 0, 0, 1, 2, 0, 0, 1, 2, 0, 1 };
                    float[] senseOf = { 1, 1, 1, 1, 1, 1, -1, -1, -1, 1, 1 };
                    float[] waitOf = { 0, 1, 1, 1, 1, 1, 1, 1, 1, .6f, 1.5f };
                    int whirledJudged = 0, whirledCourses = kinds.Length - 2, tries = 0;
                    Quaternion plainOwnTurn = Quaternion.identity, plainMadeTurn = Quaternion.identity; bool plainRead = false;
                    for (int n = 0; n < kinds.Length; n++)
                    {
                        bool pivotJudged = false, pivotPass = false; string pivotSaid = "";
                        if (n > 0)
                        {
                            var was = own.Riser;
                            if (was == null) { bars.EventMissed($"{who} {kinds[n]}: no riser to go down from"); makesMissed++; break; }
                            int goneDownWas = own.WentDown;
                            yield return Wait(waitOf[n]);
                            bool lostEarly = own.Riser != was || own.Riser == null;
                            bool whirl = n >= 2, whip = n == 5;
                            int axis = axisOf[n];
                            if (lostEarly)
                            {
                                if (tries < 3 && own != null)
                                {
                                    tries++;
                                    Debug.Log(string.Format(Inv, "FORCED_RETRY {0} {1}: the riser ended by itself while the rig waited {2:0.0} s before the course (its own body went down {3} times meanwhile, OwnBody.WentDown {4} -> {5}); the course tried again on the next getting up (try {6} of 3)", who, kinds[n], waitOf[n], own.WentDown - goneDownWas, goneDownWas, own.WentDown, tries));
                                    for (float until = Time.time + 40; (own.Riser == null || own.Riser == was) && Time.time < until;) yield return null;
                                    if (own.Riser == null || own.Riser == was) { bars.EventMissed($"{who} {kinds[n]}: no getting up again within 40 s after its own going down"); makesMissed++; break; }
                                    n--; continue;
                                }
                                bars.EventMissed($"{who} {kinds[n]}: the riser ended by itself first, {tries} tries"); makesMissed++; break;
                            }
                            var b0 = own.Part(Foot);
                            ArticulationBody t0 = null;
                            foreach (var link in b0.GetComponentsInChildren<ArticulationBody>()) if (link != b0) t0 = link;
                            var names = new[] { b0.gameObject.AddComponent<TouchNames>(), t0 != null ? t0.gameObject.AddComponent<TouchNames>() : null };
                            int touchesWas = 0, freeFor = 0, waited = 0, j = -1, riserWrote = 0;
                            bool wentDown = false, startedFree = false, touchedDuring = false, afterNull = false, oldGone = false, oldActive = false;
                            ArticulationBody oldBoot = null;
                            var engine = new List<float>(); var engineRel = new List<float>(); var kneeEngine = new List<float>(); var sets = new List<float>(); var readings = new List<string>();
                            float setTo = senseOf[n] * whirlSpeed; string jointSaid = ""; int dofs = -1;
                            // (Item 2's reading: the own boot's turn against its own shin as it went down, the made boot's (the
                            // let-go body's) against the let-go shin, and the slack OwnBody asked (slackBy).)
                            Quaternion ownTurnAt = Quaternion.identity; float slackAsked = float.NaN; bool madeHasBody = false;
                            Vector3 atGoingV = Vector3.zero; float pivotGap = float.NaN, kneeAt = 0;
                            Vector3 pivotOwn = Vector3.zero, pivotLetGo = Vector3.zero;
                            // (The let-go boot, read from the running fall: its points at S and A in its own frame, and S, A
                            // and its turn in the let-go shin's frame, at the going down; then, each step until the make, how
                            // far those points have moved against the shin, and how far the slack has run.)
                            PhysicalFall fallNow = null; Transform madeBoot = null, fallShin = null;
                            Vector3 qS = Vector3.zero, qA = Vector3.zero, sLocal = Vector3.zero, aLocal = Vector3.zero; Quaternion turnStart = Quaternion.identity;
                            float dS = float.NaN, dA = float.NaN, slackTurn = float.NaN, slackShare = float.NaN; bool slackDone = false; int slackReads = 0;
                            void DoGoDown(ArticulationBody b)
                            {
                                oldBoot = b;
                                ownTurnAt = Quaternion.Inverse(own.Part(Shin).transform.rotation) * b.transform.rotation;
                                goDownM.Invoke(own, null);
                                wentDown = true;
                                if (slackByF != null) slackAsked = Quaternion.Angle(Quaternion.identity, ((Quaternion[])slackByF.GetValue(own))[0]);
                                // (Where each half of C turns the let-go boot about, read at once: the boot's own ankle, as off
                                // has it, and the let-go shin's end, as C has it.)
                                var fall = unit.GetComponent<PhysicalFall>();
                                pivotOwn = ((Vector3[])ankleAtF.GetValue(own))[0];
                                if (fall != null && fall.Part(Shin) != null)
                                {
                                    pivotLetGo = fall.Part(Shin).transform.TransformPoint(new Vector3(0, solved.shins[0].localScale.y, 0));
                                    pivotGap = (pivotOwn - pivotLetGo).magnitude;
                                    fallNow = fall; fallShin = fall.Part(Shin).transform;
                                    madeBoot = ((Transform[])bootsMadeF.GetValue(fall))[0];
                                    if (madeBoot != null)
                                    {
                                        qS = madeBoot.InverseTransformPoint(pivotLetGo); qA = madeBoot.InverseTransformPoint(pivotOwn);
                                        sLocal = fallShin.InverseTransformPoint(pivotLetGo); aLocal = fallShin.InverseTransformPoint(pivotOwn);
                                        turnStart = Quaternion.Inverse(fallShin.rotation) * madeBoot.rotation;
                                        madeHasBody = madeBoot.GetComponent<Rigidbody>() != null || madeBoot.GetComponent<ArticulationBody>() != null;
                                    }
                                }
                            }
                            void ReadSlack()
                            {
                                if (slackDone || fallNow == null || madeBoot == null || fallShin == null) return;
                                slackShare = ((float[])slackenedF.GetValue(fallNow))[0];
                                dS = (fallShin.InverseTransformPoint(madeBoot.TransformPoint(qS)) - sLocal).magnitude;
                                dA = (fallShin.InverseTransformPoint(madeBoot.TransformPoint(qA)) - aLocal).magnitude;
                                slackTurn = Quaternion.Angle(turnStart, Quaternion.Inverse(fallShin.rotation) * madeBoot.rotation);
                                slackReads++;
                                if (slackShare >= 1) slackDone = true;
                            }
                            forcer.Lost = () => own == null || (!wentDown && own.Riser != was);
                            forcer.Begin(200, i =>
                            {
                                var b = own.Part(Foot);
                                if (wentDown)
                                {
                                    // (The step after: what the test reads the boot through.)
                                    afterNull = b == null; oldGone = oldBoot == null; oldActive = !oldGone && oldBoot.gameObject.activeInHierarchy;
                                    forcer.Done = true; return;
                                }
                                if (b == null) { forcer.Done = true; return; }
                                int touches = (names[0] != null ? names[0].Total : 0) + (names[1] != null ? names[1].Total : 0);
                                bool touchedLast = touches != touchesWas; touchesWas = touches;
                                if (!whirl) { atGoingV = Speeds(b); DoGoDown(b); return; }
                                if (j < 0)
                                {
                                    freeFor = touchedLast ? 0 : freeFor + 1; waited++;
                                    if (freeFor >= 2 || waited >= 100) { j = 0; startedFree = freeFor >= 2; } else return;
                                }
                                Vector3 eng = Speeds(b);
                                if (j == 0)
                                    jointSaid = string.Format(Inv, "the boot's joint {0}, locks {1} {2} {3}, limits X {4:0}..{5:0} Y {6:0}..{7:0} Z {8:0}..{9:0} deg, maxJointVelocity {10:0.0}, jointFriction {11:0.###}, angularDamping {12:0.###}; its reading at the set {13} deg",
                                        b.jointType, b.twistLock, b.swingYLock, b.swingZLock, b.xDrive.lowerLimit, b.xDrive.upperLimit, b.yDrive.lowerLimit, b.yDrive.upperLimit, b.zDrive.lowerLimit, b.zDrive.upperLimit, b.maxJointVelocity, b.jointFriction, b.angularDamping, V(Reading(b) * Mathf.Rad2Deg));
                                if (j >= 1)
                                {
                                    // (The engine's own: nothing has set the boot's speed since the step before.)
                                    float read = PerAxis(eng);
                                    engine.Add(read); engineRel.Add((b.angularVelocity - own.Part(Shin).angularVelocity).magnitude);
                                    var kv = own.Part(Shin).jointVelocity; kneeEngine.Add(kv.dofCount > 0 ? kv[0] : 0);
                                    readings.Add(V(Reading(b) * Mathf.Rad2Deg));
                                    touchedDuring |= touchedLast;
                                    if (b.xDrive.stiffness > 0 || b.xDrive.damping > 0) riserWrote++;
                                    // (Gone down in the step the engine itself has the boot whirling at 50 to 80 rad/s: no set since
                                    // the step before. Else set again, at most 30 times (down_c1: Long's ten reads 47.1 to 108.1 missed
                                    // the band, log:1967): scaled by what the engine made of the last set,
                                    // below the joint's own maxJointVelocity (100: down_c0b's sets of 260 were read back at 7 to 41), and
                                    // the other way about when the engine kept less than half of it.)
                                    // (And at once when the engine reads more than twice the joint's maxJointVelocity: down_c0d's
                                    // Long whipped, set again 30 times with its knee and hip spun at each, read 1e12 rad/s, log:2571.)
                                    // (Step 4a‴: down_c1f's Small whipped read 102.7, 95.7, 251.5 (down_c1f.log, FORCED_WHIRL) and
                                    // went down at once at 2 x maxJointVelocity, the whirl not made; the sets could not go below
                                    // -whirlSpeed though the engine kept more than was set. Now: at once only past 10 x
                                    // maxJointVelocity (a blown step, as down_c0d's 1e12), and a set as small as 20 rad/s.)
                                    if ((read >= 50 && read <= 80) || j >= 30 || read > 10 * b.maxJointVelocity) { atGoingV = eng; kneeAt = kv.dofCount > 0 ? kv[0] : 0; DoGoDown(b); return; }
                                    float size = Mathf.Abs(setTo);
                                    bool flip = read < .5f * size;
                                    size = Mathf.Clamp(size * 65 / Mathf.Max(read, 1), 20, Mathf.Min(95, .95f * b.maxJointVelocity));
                                    setTo = (flip ? -Mathf.Sign(setTo) : Mathf.Sign(setTo)) * size;
                                }
                                DrivesOff(b);
                                var w = b.jointVelocity; dofs = w.dofCount; w[Mathf.Min(axis, w.dofCount - 1)] = setTo; b.jointVelocity = w; sets.Add(setTo);
                                if (whip && j == 0)
                                {
                                    var k = own.Part(Shin); var kw = k.jointVelocity; kw[0] = 25; k.jointVelocity = kw;
                                    var h = own.Part(Thigh); var hw = h.jointVelocity; hw[0] = 15; h.jointVelocity = hw;
                                }
                                j++;
                            });
                            while (!forcer.Done) yield return null;
                            string engineSaid = string.Join(", ", engine.ConvertAll(x => x.ToString("0.0", Inv)));
                            Debug.Log(string.Format(Inv, "FORCED_GOING_DOWN {0} {1}: made to go down (OwnBody.GoDown) {2}; {3}the left boot as the engine left it, step by step after the set: {4} rad/s on an axis (against its shin {5}), its knee {6}; at the going down {7} rad/s, the knee {8:0.0}; the riser wrote the boot's drive again on {9} of those steps (the rig let it go again each step); touched during the whirl {10}; the pivot of C off (the boot's own ankle) {11} and of C on (the let-go shin's end) {12}: {13:0.0000} m apart; the step after: OwnBody.Part(Foot) is null {14}, the boot it was destroyed {15}, active {16}; it has gone down by itself {17} times",
                                who, kinds[n], wentDown && !forcer.WasLost, whirl ? (startedFree ? $"after {waited} steps waiting, touching nothing for two; " : $"touching still after {waited} steps of waiting; ") : "",
                                whirl ? engineSaid : "(not whirled)", string.Join(", ", engineRel.ConvertAll(x => x.ToString("0.0", Inv))), string.Join(", ", kneeEngine.ConvertAll(x => x.ToString("0.0", Inv))),
                                V(atGoingV), kneeAt, riserWrote, touchedDuring, V(pivotOwn, "0.0000"), V(pivotLetGo, "0.0000"), pivotGap, afterNull, oldGone, oldActive, own.WentDown));
                            if (!wentDown && forcer.WasLost && own != null && tries < 3)
                            {
                                tries++;
                                Debug.Log(string.Format(Inv, "FORCED_RETRY {0} {1}: the riser ended by itself after {2} steps of waiting for the boot to touch nothing (touching still: {3}; its own body went down {4} times meanwhile, OwnBody.WentDown {5} -> {6}); the course tried again on the next getting up (try {7} of 3)",
                                    who, kinds[n], waited, !startedFree, own.WentDown - goneDownWas, goneDownWas, own.WentDown, tries));
                                for (float until = Time.time + 40; (own.Riser == null || own.Riser == was) && Time.time < until;) yield return null;
                                if (own.Riser == null || own.Riser == was) { bars.EventMissed($"{who} {kinds[n]}: no getting up again within 40 s after its own going down"); makesMissed++; break; }
                                n--; continue;
                            }
                            if (!wentDown) { bars.EventMissed($"{who} {kinds[n]}: the going down was not made ({tries} tries)"); makesMissed++; break; }
                            tries = 0;
                            if (whirl)
                                Debug.Log(string.Format(Inv, "FORCED_WHIRL {0} {1}: set at {2} rad/s on its joint's axis {6} (of {7} it has), the step after each the engine read {3} rad/s on an axis, the joint reading {4}; {5}",
                                    who, kinds[n], string.Join(", ", sets.ConvertAll(x => x.ToString("0.0", Inv))), engineSaid, string.Join(", ", readings), jointSaid, axis, dofs));
                            if (whirl && (engine.Count < 1 || engine[engine.Count - 1] < 50 || engine[engine.Count - 1] > 80))
                                bars.EventMissed($"{who} {kinds[n]}: the whirl was not made: the engine read the boot at {engineSaid} rad/s on an axis after each set, the last not 50-80");
                            // (-forcedFeed BAR_C_PIVOT: the first make read is fed a NaN (its A point), every later make's slack
                            // is withheld (not read): the bar must fail on the NaN, and the miner judge too few whirled courses.)
                            bool withheld = feedsPivot && makesRead > 1;
                            for (float until = Time.time + 40; (own.Riser == null || own.Riser == was) && Time.time < until;) { if (!withheld) ReadSlack(); yield return null; }
                            if (own.Riser == null || own.Riser == was) { bars.EventMissed($"{who} {kinds[n]}: not made down again within 40 s"); makesMissed++; break; }
                            bool fedNaN = feedsPivot && makesRead == 1 && slackDone;
                            if (fedNaN) dA = float.NaN;
                            if (feedsPivot) Debug.Log($"FORCED_FEED {who} {kinds[n]}: -forcedFeed BAR_C_PIVOT: this make fed {(fedNaN ? "one NaN (its A point's move)" : withheld ? "nothing (its slack not read)" : "as it is")}");
                            // (BAR_C_PIVOT: what the slack turned the let-go boot about. A NaN read where the slack ran to its end
                            // is judged, and fails: it is never "not judged".)
                            bool pivotNaN = slackDone && (float.IsNaN(dS) || float.IsNaN(dA) || float.IsNaN(slackTurn) || float.IsNaN(pivotGap));
                            float predicted = 2 * pivotGap * Mathf.Sin(.5f * slackTurn * Mathf.Deg2Rad), moved = Mathf.Max(dS, dA);
                            pivotJudged = slackDone && (pivotNaN || moved >= PivotMovedLeastM);
                            pivotPass = pivotJudged && !pivotNaN && dS <= PivotShare * dA;
                            pivotSaid = string.Format(Inv, "the slack ran {0:0.00} of its way ({1}, read on {2} steps before the make); it turned the boot {3:0.0} deg against its shin; the boot's point at the let-go shin's end moved {4:0.0000} m, its point at its own body's ankle {5:0.0000} m (|S - A| {6:0.0000} m; 2 |S - A| sin(turn / 2) {7:0.0000} m); {8}",
                                slackShare, slackDone ? "done" : "not done", slackReads, slackTurn, dS, dA, pivotGap, predicted,
                                pivotNaN ? "a NaN read: a failure" : !pivotJudged ? "not judged: " + (!slackDone ? "the slack did not run to its end" : "it moved neither point 1 cm, so nothing tells the pivots apart") : pivotPass ? "turned about the let-go shin's end (S stayed)" : "not turned about the let-go shin's end");
                            Debug.Log("FORCED_SLACK " + who + " " + kinds[n] + ": " + pivotSaid);
                            if (whirl && pivotJudged) whirledJudged++;
                            // (Item 2, a reading: what C acts on, and whether the whirl changes it. C acts on the made boot (the
                            // let-go body's) only: its pose against the let-go shin at the going down, and the slack's turn.)
                            Quaternion madeTurn = turnStart;
                            if (n == 1) { plainOwnTurn = ownTurnAt; plainMadeTurn = madeTurn; plainRead = true; }
                            Debug.Log(string.Format(Inv, "FORCED_C_ACTS {0} {1} {2}: as it went down the own boot was turned {3:0.0} deg against its own shin from where the plain course had it{4}, its joint at {5:0.0} rad/s on an axis; the made boot (the let-go body's: a body of its own {6}, so the whirl's speed is handed to {7}) turned {8:0.0} deg against the let-go shin from the plain course's made boot, {9:0.0} deg from the own boot's turn against its own shin; the slack OwnBody asked (slackBy) {10:0.0} deg, the slack turned it {11:0.0} deg against its shin; |S - A| {12:0.0000} m",
                                who, cName, kinds[n], plainRead ? Quaternion.Angle(plainOwnTurn, ownTurnAt) : float.NaN, plainRead ? "" : " (no plain course read)", PerAxis(atGoingV), madeHasBody, madeHasBody ? "it" : "nothing",
                                plainRead ? Quaternion.Angle(plainMadeTurn, madeTurn) : float.NaN, Quaternion.Angle(ownTurnAt, madeTurn), slackAsked, slackTurn, pivotGap));
                        }
                        else if (own.Riser == null) { bars.EventMissed($"{who}: not made down by the push within 40 s"); makesMissed++; break; }
                        // The make: each boot's ankle against the body made standing; how far the refit moved it (C on), what
                        // it weighs about its ankle.
                        var shift = (Vector3[])bootShiftF.GetValue(own);
                        float[] lighter = (float[])lighterF.GetValue(own.Riser);
                        var sb = new StringBuilder();
                        sb.AppendFormat(Inv, "FORCED_MAKE {0} {1}: BootsFromOwnShin {2}; ", who, kinds[n], OwnBody.BootsFromOwnShin);
                        float letGoLeft = float.NaN;
                        for (int leg = 0; leg < 2; leg++)
                        {
                            var b = own.Part(Foot + leg);
                            float apart = (b.anchorPosition - standAnchor[leg]).magnitude;
                            // (Where the let-go body had the boot's ankle against its shin's end, before any refit: off, the make
                            // keeps it, so its distance from the standing one; on, the refit moved it by just that much.)
                            float before = OwnBody.BootsFromOwnShin ? shift[leg].magnitude : apart;
                            if (leg == 0) letGoLeft = before;
                            sb.AppendFormat(Inv, "boot {0}: the let-go body had its ankle {1:0.0000} m from where it was standing; after the make it is {2:0.0000} m from it{3}; lighter {4:0.000000} kg m2; ",
                                leg, before, apart, OwnBody.BootsFromOwnShin ? string.Format(Inv, " (the refit moved it {0:0.0000} m: 0 after the refit by construction)", shift[leg].magnitude) : "", lighter[Foot + leg]);
                        }
                        Debug.Log(sb.ToString());
                        float madeLeft = (own.Part(Foot).anchorPosition - standAnchor[0]).magnitude;
                        if (n >= 1)
                            Debug.Log(string.Format(Inv, "FORCED_READ_C{0} {1} {2} {3}: the let-go body had the left boot's ankle {4:0.0000} m from where it was standing (before any refit); after the make {5:0.0000} m; the old bar {6:0.000} m would say {7}; research's centimetre at the make {8}",
                                n == 1 ? "_PLAIN" : "", who, cName, kinds[n], letGoLeft, madeLeft, barC, letGoLeft <= barC ? "PASS" : "FAIL", madeLeft <= .01f ? "met" : "not met"));
                        if (n >= 1 && pivotJudged) bars.Judge("BAR_C_PIVOT", who, cName, kinds[n], pivotPass, pivotSaid);
                        makesRead++;
                    }
                    if (whirledJudged < PivotLeastCourses) bars.EventMissed($"{who} {cName}: only {whirledJudged} of the {whirledCourses} whirled courses had a slack that tells the pivots apart (BAR_C_PIVOT needs {PivotLeastCourses})");
                    else Debug.Log($"FORCED_COURSES {who} {cName}: {whirledJudged} of the {whirledCourses} whirled courses judged by BAR_C_PIVOT, each a distinct course (axis, sense or moment), in this run");
                    Debug.Log(watch.Summary());
                    foreach (var line in watch.Lines) if (line.StartsWith("ANKLE_DOWN")) Debug.Log("FORCED_WATCH " + line);
                    Object.Destroy(watch.gameObject); watch = null;
                    yield return TakenBack();
                }
            }
            finally { SwitchesBack(); }
            Debug.Log($"FORCED_DOWN_DONE {makesRead} makes read, {makesMissed} missed");
            var problems = bars.Verdict(mustFail, whos);
            Assert.That(problems, Is.Empty, "FORCED bars: " + string.Join(" | ", problems.GetRange(0, Math.Min(8, problems.Count))));
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
