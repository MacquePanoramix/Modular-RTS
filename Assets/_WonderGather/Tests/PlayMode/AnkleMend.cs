using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WonderGather.Tests
{
    // The ankle mend's switches, for the tests (Docs/Research/2026-11-15_NaturalRangesAndTheAnkle/research.md §7.3;
    // build.md beside it; November 17, 2026). Test code only: nothing in the game.
    //
    //  -ownUnheard 0|1: RiseAgent.Unheard (true by default from November 17: the heard run is -ownUnheard 0);
    //  -riseSet Name=value,...: any public static number or switch of OwnRiser (AnkleLimit, AnkleSpeed, ...), as
    //    OwnRiseBench.cs:395-402 sets them;
    //  -ownBodySet Name=value,...: the same for OwnBody (BootsFromOwnShin=1);
    //  -liveStrength s: the chosen miner's strength, through MinerWorkPreview.SetStrength (read by the test itself).
    // Every static set is put back as it was found when the test ends (Restore).
    internal static class AnkleMendArguments
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static bool unheardWas, saved;
        private static readonly List<KeyValuePair<FieldInfo, object>> was = new List<KeyValuePair<FieldInfo, object>>();

        public static void Save() { unheardWas = RiseAgent.Unheard; was.Clear(); saved = true; ForgetSaid(); }

        // OwnRiser says each wrong ankle setting once (ankleLimitSaid, ankleSpeedSaid, ankleTurnsSaid): forgotten when a
        // test begins and when it ends, so that every test hears its own (a check that expects the message gets it).
        private static readonly FieldInfo[] saidOnce =
        {
            typeof(OwnRiser).GetField("ankleLimitSaid", BindingFlags.NonPublic | BindingFlags.Static),
            typeof(OwnRiser).GetField("ankleSpeedSaid", BindingFlags.NonPublic | BindingFlags.Static),
            typeof(OwnRiser).GetField("ankleTurnsSaid", BindingFlags.NonPublic | BindingFlags.Static),
        };

        private static void ForgetSaid()
        {
            foreach (var f in saidOnce)
            {
                Assert.That(f, Is.Not.Null, "OwnRiser's once-said ankle flags are not where AnkleMendArguments looks for them.");
                f.SetValue(null, false);
            }
        }

        // Sets what the arguments say; returns what it set.
        public static string Apply()
        {
            var said = new StringBuilder();
            string unheard = CaptureTools.Argument("-ownUnheard");
            if (!string.IsNullOrEmpty(unheard))
            {
                // (Only 0 or 1: an argument left without its value takes the next one's name.)
                Assert.That(unheard == "0" || unheard == "1", Is.True, "-ownUnheard takes 0 (the learnt model heard) or 1 (unheard), not " + unheard + ".");
                RiseAgent.Unheard = unheard == "1";
                said.Append("RiseAgent.Unheard = " + RiseAgent.Unheard + "; ");
            }
            Set(typeof(OwnRiser), "-riseSet", said);
            Set(typeof(OwnBody), "-ownBodySet", said);
            return said.Length > 0 ? said.ToString() : "nothing set";
        }

        private static void Set(Type on, string argument, StringBuilder said)
        {
            foreach (string word in (CaptureTools.Argument(argument) ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var bits = word.Split('=');
                Assert.That(bits.Length, Is.EqualTo(2), argument + ": not Name=value: " + word);
                var field = on.GetField(bits[0], BindingFlags.Public | BindingFlags.Static);
                Assert.That(field, Is.Not.Null, on.Name + " has no public static " + bits[0]);
                Assert.That(field.IsInitOnly || field.IsLiteral, Is.False, on.Name + "." + bits[0] + " cannot be set");
                was.Add(new KeyValuePair<FieldInfo, object>(field, field.GetValue(null)));
                float value = float.Parse(bits[1], Inv);
                if (field.FieldType == typeof(bool)) field.SetValue(null, value > .5f);
                else if (field.FieldType == typeof(int)) field.SetValue(null, (int)value);
                else field.SetValue(null, value);
                said.Append(on.Name + "." + bits[0] + " = " + Convert.ToString(field.GetValue(null), Inv) + "; ");
            }
        }

        public static void Restore()
        {
            if (!saved) return;
            for (int k = was.Count - 1; k >= 0; k--) was[k].Key.SetValue(null, was[k].Value);
            was.Clear();
            RiseAgent.Unheard = unheardWas;
            saved = false;
            ForgetSaid();
        }

        // The switches as the game reads them now.
        public static string ReadBack() => string.Format(Inv,
            "RiseAgent.Unheard {0}; OwnRiser.AnkleLimit {1}, AnkleSpeed {2}, AnkleSpeedHeight {3}, AnkleAtMost {4}, AnkleTurnsAtMost {5}; OwnBody.BootsFromOwnShin {6}; PhysicalFall.GetsUpByItsOwn {7}",
            RiseAgent.Unheard, OwnRiser.AnkleLimit, OwnRiser.AnkleSpeed, OwnRiser.AnkleSpeedHeight, OwnRiser.AnkleAtMost, OwnRiser.AnkleTurnsAtMost, OwnBody.BootsFromOwnShin, PhysicalFall.GetsUpByItsOwn);
    }

    // A boot's or a toe's touches over a step of the physics (the engine tells of them after the step).
    internal sealed class BootTouch : MonoBehaviour
    {
        public bool Touched;
        private void OnCollisionEnter(Collision touch) => Touched = true;
        private void OnCollisionStay(Collision touch) => Touched = true;
    }

    // What the ankle mend asks to be read on the running body (research.md §7.3, "per step, the mechanism"), after
    // everything of each step of the physics has been asked (after OwnBody and CoverPair). Reads only; writes nothing to
    // the body (but a BootTouch on each boot and toe, which only listens to the engine's contacts).
    //  - the ankle target actually written each step (the drive's targets, degrees), and its jumps over 60 degrees in a
    //    step (the targets' difference, as the engine sees it, and the turn between them);
    //  - each boot's joint speed per axis (jointVelocity, rad/s) above 30, split by whether the boot or its toe touched
    //    anything that step, and by whether the riser had the body;
    //  - per make of a body that gets up: lighter[foot], |toStood[Foot]|, the riser's height, the gains written, the
    //    boot's mass and its turning weight about each of its joint's axes (part 1 of build.md);
    //  - the switches read back from what the body does: the written target against today's form of the same ask
    //    (AnkleLimit), the written target's distance from the joint's reading against E = force limit / stiffness
    //    (AnkleSpeed), the written damping against today's form and the smooth maximum (AnkleSpeed), and how far each
    //    boot's ankle is from its middle at each make (BootsFromOwnShin).
    [DefaultExecutionOrder(30002)]
    internal sealed class AnkleWatch : MonoBehaviour
    {
        private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static readonly FieldInfo lighterF = typeof(OwnRiser).GetField("lighter", Hidden), toStoodF = typeof(OwnRiser).GetField("toStood", Hidden),
            tallF = typeof(OwnRiser).GetField("tall", Hidden), springOfF = typeof(OwnRiser).GetField("springOf", Hidden), hasOfF = typeof(OwnRiser).GetField("hasOf", Hidden),
            drivenNowF = typeof(OwnRiser).GetField("drivenNow", Hidden), slackNowF = typeof(OwnRiser).GetField("slackNow", Hidden);
        public const float JumpOver = 60, FastOver = 30;
        private const int LinesAtMost = 60;

        public OwnBody Own;
        public string Who = "";
        public readonly List<string> Lines = new List<string>();
        public int Steps, RiserSteps, Makes, Jumps, TurnJumps, FastTouched, FastFree, FastFreeRiser, LinesDropped;
        public float JumpMost, TurnJumpMost, FastestTouched, FastestFree, FastestFreeRiser;
        // (The read-back: steps compared, how many wrote today's form of the ask within 0.01 degrees and the most any
        // differed; the most the written target was from the reading, in shares of E; damping steps that matched today's
        // form and B's.)
        public int Compared, AsToday, AsTodayKind, DampTodaySame, DampBSame, DampCompared, OffCompared;
        public float FromTodayMost, OffOverEMost, ReplicaUnsure;

        private OwnRiser riserSeen;
        private bool makeSaid;
        private ArticulationBody bootSeen0, bootSeen1;
        private readonly BootTouch[] bootTouch = new BootTouch[2], toeTouch = new BootTouch[2];
        private readonly Vector3[] lastTarget = new Vector3[2], standAnchor = new Vector3[2];
        private readonly bool[] standKnown = new bool[2];
        // (BootsFromOwnShin, read back: each boot's ankle in the boot, at each make of a body that gets up, against where
        // it was in the body made standing. On, they are the same; off, the make's follows the pose.)
        public float AnchorFromStandingMost;
        public int AnchorsCompared;
        // (BootsFromOwnShin, read back as it goes down: the point the let-go body's boot is slackened about, from what
        // the fall was told (its footAtTo), against the two it could be: where its own body had the ankle (off) and the
        // end of the let-go body's own shin (on); and how far apart those two are.)
        private static readonly FieldInfo ownAnkleAtF = typeof(OwnBody).GetField("ankleAt", Hidden), ownSlackByF = typeof(OwnBody).GetField("slackBy", Hidden),
            fallWasF = typeof(PhysicalFall).GetField("footAtWas", Hidden), fallToF = typeof(PhysicalFall).GetField("footAtTo", Hidden);
        public int Downs, SlackAboutOwnAnkle, SlackAboutShinEnd, SlackNeither;
        public float PivotsApartMost;
        private bool stoodLast;
        private int wentDownWas;
        public int PushedDowns;
        private readonly bool[] lastKnown = new bool[2];
        private float[] lighter, springOf, hasOf, slackNow;
        private Quaternion[] toStood;
        private Vector3[] drivenNow;
        private float tall;

        public static bool Reads(out string why)
        {
            why = null;
            foreach (var f in new[] { lighterF, toStoodF, tallF, springOfF, hasOfF, drivenNowF, slackNowF }) if (f == null) why = "OwnRiser lacks a field the ankle watch reads";
            foreach (var f in new[] { ownAnkleAtF, ownSlackByF, fallWasF, fallToF }) if (f == null) why = "OwnBody or PhysicalFall lacks a field the ankle watch reads";
            return why == null;
        }

        private void Said(string line)
        {
            if (Lines.Count < LinesAtMost) { Lines.Add(line); Debug.Log(line); }
            else LinesDropped++;
        }

        private void FixedUpdate()
        {
            if (Own == null) return;
            bool stands = Own.Stands;
            // (Only a going down of its own (OwnBody.GoDown, which slackens the boots); a push from the panel lets the body
            // go without it.)
            if (stoodLast && !stands && Own.WentDown > wentDownWas) WentDown();
            if (stoodLast && !stands && Own.WentDown == wentDownWas) PushedDowns++;
            wentDownWas = Own.WentDown;
            stoodLast = stands;
            if (!stands) return;
            var boot0 = Own.Part(OwnBody.Foot);
            if (boot0 == null) return;
            if (boot0 != bootSeen0 || Own.Part(OwnBody.Foot + 1) != bootSeen1) Listen();
            Steps++;
            var riser = Own.Riser;
            if (riser != riserSeen)
            {
                riserSeen = riser; makeSaid = false; lastKnown[0] = lastKnown[1] = false;
                if (riser != null)
                {
                    lighter = (float[])lighterF.GetValue(riser); toStood = (Quaternion[])toStoodF.GetValue(riser); tall = (float)tallF.GetValue(riser);
                    springOf = (float[])springOfF.GetValue(riser); hasOf = (float[])hasOfF.GetValue(riser); drivenNow = (Vector3[])drivenNowF.GetValue(riser);
                    slackNow = (float[])slackNowF.GetValue(riser);
                    Makes++;
                }
            }
            if (riser != null) RiserSteps++;
            for (int leg = 0; leg < 2; leg++)
            {
                var boot = Own.Part(OwnBody.Foot + leg);
                if (riser == null && !Own.Rising && !standKnown[leg]) { standAnchor[leg] = boot.anchorPosition; standKnown[leg] = true; }
                // Its speed per axis, from the step just taken, and whether it or its toe touched anything in it.
                var v = boot.jointVelocity;
                float fastest = 0;
                for (int d = 0; d < v.dofCount; d++) fastest = Mathf.Max(fastest, Mathf.Abs(v[d]));
                bool touched = (bootTouch[leg] != null && bootTouch[leg].Touched) || (toeTouch[leg] != null && toeTouch[leg].Touched);
                if (bootTouch[leg] != null) bootTouch[leg].Touched = false;
                if (toeTouch[leg] != null) toeTouch[leg].Touched = false;
                if (touched) FastestTouched = Mathf.Max(FastestTouched, fastest); else FastestFree = Mathf.Max(FastestFree, fastest);
                if (!touched && riser != null) FastestFreeRiser = Mathf.Max(FastestFreeRiser, fastest);
                if (fastest > FastOver)
                {
                    if (touched) FastTouched++; else { FastFree++; if (riser != null) FastFreeRiser++; }
                    Said(string.Format(Inv, "ANKLE_FAST {0} t {1:0.00} boot {2}: {3:0.0} rad/s on an axis ({4:0.0} {5:0.0} {6:0.0}), {7}, {8}", Who, Time.time, leg, fastest,
                        v.dofCount > 0 ? v[0] : 0, v.dofCount > 1 ? v[1] : 0, v.dofCount > 2 ? v[2] : 0, touched ? "touching" : "touching nothing", riser != null ? "getting up" : "its keeper's"));
                }
                if (riser == null) continue;
                // The target written for the next step (degrees), its jump from the last.
                Vector3 target = new Vector3(boot.xDrive.target, boot.yDrive.target, boot.zDrive.target);
                if (lastKnown[leg])
                {
                    float jump = (target - lastTarget[leg]).magnitude;
                    float turn = Quaternion.Angle(Turn(lastTarget[leg]), Turn(target));
                    JumpMost = Mathf.Max(JumpMost, jump); TurnJumpMost = Mathf.Max(TurnJumpMost, turn);
                    if (jump > JumpOver) Jumps++;
                    if (turn > JumpOver) TurnJumps++;
                    if (jump > JumpOver || turn > JumpOver)
                        Said(string.Format(Inv, "ANKLE_JUMP {0} t {1:0.00} boot {2} stage {3}: target ({4:0.0} {5:0.0} {6:0.0}) from ({7:0.0} {8:0.0} {9:0.0}) degrees: {10:0.0} as written, {11:0.0} as a turn",
                            Who, Time.time, leg, riser.StageNow, target.x, target.y, target.z, lastTarget[leg].x, lastTarget[leg].y, lastTarget[leg].z, jump, turn));
                }
                lastTarget[leg] = target; lastKnown[leg] = true;
                ReadBack(riser, boot, leg, target);
            }
            if (riser != null && !makeSaid && springOf[OwnBody.Foot] > 0) { makeSaid = true; SayMake(riser); }
        }

        private static Quaternion Turn(Vector3 by)
        {
            float angle = by.magnitude;
            return angle > 1e-6f ? Quaternion.AngleAxis(angle, by / angle) : Quaternion.identity;
        }

        // The switches, read back from what the riser wrote this step.
        private void ReadBack(OwnRiser riser, ArticulationBody boot, int leg, Vector3 target)
        {
            int j = OwnBody.Foot + leg;
            // Today's form of the same ask (Aims and AnkleCan as they were): equal with AnkleLimit 0 and AnkleSpeed 0.
            if (OwnRiser.AnkleTurnsAtMost > 0) ReplicaUnsure++;
            Quaternion own = riser.Aim(j);
            float size = own.x * own.x + own.y * own.y + own.z * own.z + own.w * own.w;
            if (!float.IsNaN(size) && size >= .5f && size <= 1.5f)
            {
                own = Quaternion.Normalize(own);
                if (OwnRiser.AnkleAtMost > 0)
                {
                    Quaternion stood = toStood[j], fromStood = Quaternion.Inverse(stood) * own;
                    fromStood.ToAngleAxis(out float far, out Vector3 about);
                    if (far > 180) far -= 360;
                    if (Mathf.Abs(far) > OwnRiser.AnkleAtMost && !float.IsNaN(about.x)) fromStood = Quaternion.AngleAxis(Mathf.Sign(far) * OwnRiser.AnkleAtMost, about);
                    own = stood * fromStood;
                }
                own.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180) angle -= 360;
                if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.right; angle = 0; }
                Vector3 today = angle * axis + drivenNow[j];
                float from = (target - today).magnitude;
                Compared++;
                if (from < .01f) AsToday++;
                FromTodayMost = Mathf.Max(FromTodayMost, from);
                // (Check 7: today's form of the ask, slowed as B slows it (AnkleSlows, from the same reading and E): what
                // is written with AnkleLimit 0 and B on. A step written unlike both is A's.)
                bool kind = from < .01f;
                var atNow = boot.jointPosition;
                var dr = boot.xDrive;
                if (!kind && OwnRiser.AnkleSpeed > 0 && atNow.dofCount == 3 && dr.stiffness > 0 && dr.forceLimit > 0)
                {
                    float most = dr.forceLimit / dr.stiffness;
                    Vector3 at0 = new Vector3(atNow[0], atNow[1], atNow[2]), off = today * Mathf.Deg2Rad - at0;
                    float far = off.magnitude;
                    if (far > 1e-9f) off *= most * (float)System.Math.Tanh(far / most) / far;
                    kind = (target - (at0 + off) * Mathf.Rad2Deg).magnitude < .01f;
                }
                if (kind) AsTodayKind++;
            }
            // How far the written target is from where the joint is, in shares of E (radians; never wrapped).
            var at = boot.jointPosition;
            var drive = boot.xDrive;
            if (at.dofCount == 3 && drive.stiffness > 0 && drive.forceLimit > 0)
            {
                Vector3 now = new Vector3(at[0], at[1], at[2]);
                float off = (target * Mathf.Deg2Rad - now).magnitude, most = drive.forceLimit / drive.stiffness;
                OffOverEMost = Mathf.Max(OffOverEMost, off / most);
                OffCompared++;
            }
            // The damping written, against today's form and B's (the gains the drive holds now).
            if (drive.stiffness > 0)
            {
                float k = drive.stiffness, has = drive.forceLimit, today = OwnRiser.Damped * 2 * Mathf.Sqrt(Mathf.Max(k, 1e-3f) * lighter[j]);
                float b = today;
                if (OwnRiser.AnkleSpeed > 0)
                {
                    float speed = OwnRiser.AnkleSpeed * Mathf.Sqrt(OwnRiser.AnkleSpeedHeight / tall), floor = Mathf.Max(0, has / speed - k * Time.fixedDeltaTime);
                    b = Mathf.Pow(Mathf.Pow(today, 4) + Mathf.Pow(floor, 4), .25f);
                }
                DampCompared++;
                if (Mathf.Abs(drive.damping - today) <= 1e-4f * Mathf.Max(1, today)) DampTodaySame++;
                if (Mathf.Abs(drive.damping - b) <= 1e-4f * Mathf.Max(1, b)) DampBSame++;
            }
        }

        // Once a make's riser has written its gains: what part 1 of build.md takes from the running body.
        private void SayMake(OwnRiser riser)
        {
            var sb = new StringBuilder();
            var physical = Own.GetComponent<PhysicalBody>();
            sb.AppendFormat(Inv, "ANKLE_MAKE {0} make {1} t {2:0.00}: riser's height (tall) {3:0.000000} m; strength {4:0.###}; ", Who, Makes, Time.time, tall, physical != null ? physical.Strength : float.NaN);
            for (int leg = 0; leg < 2; leg++)
            {
                int j = OwnBody.Foot + leg;
                var boot = Own.Part(j);
                var drive = boot.xDrive;
                float stoodBy = Quaternion.Angle(Quaternion.identity, toStood[j]);
                Vector3 ankle = boot.anchorPosition, centre = boot.centerOfMass;
                Vector3 axes = AboutAxes(boot);
                if (standKnown[leg])
                {
                    float apart = (ankle - standAnchor[leg]).magnitude;
                    AnchorFromStandingMost = Mathf.Max(AnchorFromStandingMost, apart); AnchorsCompared++;
                    sb.AppendFormat(Inv, "boot {0}: its ankle {1:0.000000} m from where it was in the body made standing ({2:0.0000} {3:0.0000} {4:0.0000}); ", leg, apart, standAnchor[leg].x, standAnchor[leg].y, standAnchor[leg].z);
                }
                sb.AppendFormat(Inv, "boot {0}: lighter {1:0.000000} kg m2, |toStood| {2:0.00} deg, springOf {3:0.000} hasOf {4:0.000} slack {5:0.###}, written k {6:0.000} c {7:0.0000} limit {8:0.000}; mass {9:0.0000} kg, its middle {10:0.0000} m from its ankle (ankle at ({11:0.0000} {12:0.0000} {13:0.0000}) in the boot), turning weight about its joint's X Y Z ({14:0.000000} {15:0.000000} {16:0.000000}) kg m2, inertia tensor ({17:0.000000} {18:0.000000} {19:0.000000}) turned ({20:0.0} {21:0.0} {22:0.0}); ",
                    leg, lighter[j], stoodBy, springOf[j], hasOf[j], slackNow[j], drive.stiffness, drive.damping, drive.forceLimit, boot.mass, (centre - ankle).magnitude, ankle.x, ankle.y, ankle.z,
                    axes.x, axes.y, axes.z, boot.inertiaTensor.x, boot.inertiaTensor.y, boot.inertiaTensor.z, boot.inertiaTensorRotation.eulerAngles.x, boot.inertiaTensorRotation.eulerAngles.y, boot.inertiaTensorRotation.eulerAngles.z);
            }
            Said(sb.ToString());
        }

        // A boot's turning weight about each axis of its joint (anchorRotation is the identity: the boot's own axes),
        // about the ankle: the engine's tensor, turned, plus its mass at its middle's distance from the axis.
        private static Vector3 AboutAxes(ArticulationBody boot)
        {
            Vector3 d = boot.centerOfMass - boot.anchorPosition, I = boot.inertiaTensor;
            Quaternion r = boot.anchorRotation;
            var about = new Vector3();
            for (int a = 0; a < 3; a++)
            {
                Vector3 e = r * (a == 0 ? Vector3.right : a == 1 ? Vector3.up : Vector3.forward);
                float sum = 0;
                for (int k = 0; k < 3; k++)
                {
                    Vector3 axis = boot.inertiaTensorRotation * (k == 0 ? Vector3.right : k == 1 ? Vector3.up : Vector3.forward);
                    float c = Vector3.Dot(e, axis);
                    sum += I[k] * c * c;
                }
                float along = Vector3.Dot(d, e);
                about[a] = sum + boot.mass * (d.sqrMagnitude - along * along);
            }
            return about;
        }

        // Its own body just went down and the fall took it: about which point was each boot slackened?
        private void WentDown()
        {
            if (!Own.TryGetComponent<PhysicalFall>(out var fall) || fall.Now != PhysicalFall.State.Falling) return;
            if (!Own.TryGetComponent<MinerBody>(out var miner) || miner.Solved.shins == null || miner.Solved.shins.Length < 2) return;
            var ankleAt = (Vector3[])ownAnkleAtF.GetValue(Own);
            var slackBy = (Quaternion[])ownSlackByF.GetValue(Own);
            var was = (Vector3[])fallWasF.GetValue(fall);
            var to = (Vector3[])fallToF.GetValue(fall);
            Downs++;
            var sb = new StringBuilder();
            sb.AppendFormat(Inv, "ANKLE_DOWN {0} t {1:0.00}: ", Who, Time.time);
            for (int leg = 0; leg < 2; leg++)
            {
                var part = fall.Part(PhysicalFall.Shin + leg);
                if (part == null) continue;
                var shin = part.transform;
                Vector3 end = shin.TransformPoint(new Vector3(0, miner.Solved.shins[leg].localScale.y, 0));
                Quaternion turns = Quaternion.Inverse(shin.rotation) * slackBy[leg] * shin.rotation;
                Vector3 Expect(Vector3 world) { Vector3 about = shin.InverseTransformPoint(world); return about + turns * (was[leg] - about); }
                float fromOwn = (to[leg] - Expect(ankleAt[leg])).magnitude, fromEnd = (to[leg] - Expect(end)).magnitude, apart = (ankleAt[leg] - end).magnitude;
                PivotsApartMost = Mathf.Max(PivotsApartMost, apart);
                if (fromOwn < 1e-4f && fromOwn < fromEnd) SlackAboutOwnAnkle++;
                else if (fromEnd < 1e-4f && fromEnd < fromOwn) SlackAboutShinEnd++;
                else SlackNeither++;
                sb.AppendFormat(Inv, "boot {0}: its own body's ankle {1:0.000000} m from the let-go shin's end, turned {2:0.0} degrees; slackened as about its own ankle to {3:0.000000} m, as about the shin's end to {4:0.000000} m; ",
                    leg, apart, Quaternion.Angle(Quaternion.identity, slackBy[leg]), fromOwn, fromEnd);
            }
            Said(sb.ToString());
        }

        // A BootTouch on each boot and its toe, once for each body made.
        private void Listen()
        {
            bootSeen0 = Own.Part(OwnBody.Foot); bootSeen1 = Own.Part(OwnBody.Foot + 1);
            for (int leg = 0; leg < 2; leg++)
            {
                var boot = leg == 0 ? bootSeen0 : bootSeen1;
                bootTouch[leg] = toeTouch[leg] = null;
                if (boot == null) continue;
                bootTouch[leg] = boot.TryGetComponent(out BootTouch has) ? has : boot.gameObject.AddComponent<BootTouch>();
                foreach (var link in boot.GetComponentsInChildren<ArticulationBody>())
                    if (link != boot) toeTouch[leg] = link.TryGetComponent(out BootTouch toeHas) ? toeHas : link.gameObject.AddComponent<BootTouch>();
            }
        }

        // Check 7 (forced_checks.md): for each switch, whether the run went through what it changes, with the counts
        // behind it. A: targets written unlike today's form of the ask (and, with B on, unlike today's form slowed as B
        // slows it). B: damping written unlike today's form. C: boots slackened about the let-go shin's end at a going down
        // by itself. A switch that is off says so.
        public string[] Exercised()
        {
            string Line(string name, bool on, bool exercised, string counts) =>
                (on ? exercised ? "LIVE_EXERCISED " : "LIVE_NOT_EXERCISED " : "LIVE_SWITCH_OFF ") + name + " " + Who + ": " + counts;
            int unlike = Compared - AsTodayKind, dampUnlike = DampCompared - DampTodaySame;
            return new[]
            {
                Line("AnkleLimit", OwnRiser.AnkleLimit == 1, unlike > 0, string.Format(Inv, "{0} of {1} boot-steps getting up written unlike today's form of the ask{2} ({3} as today's form itself)",
                    unlike, Compared, OwnRiser.AnkleSpeed > 0 ? " or that form slowed as B slows it" : "", AsToday)),
                Line("AnkleSpeed", OwnRiser.AnkleSpeed > 0, dampUnlike > 0, string.Format(Inv, "{0} of {1} boot-steps getting up with damping unlike today's form ({2} as B's smooth maximum)",
                    dampUnlike, DampCompared, DampBSame)),
                Line("BootsFromOwnShin", OwnBody.BootsFromOwnShin, SlackAboutShinEnd > 0, string.Format(Inv, "gone down by itself {0} times watched: boots slackened about the let-go shin's end {1}, about their own body's ankle {2}, neither {3}; let go by a push {4} times",
                    Downs, SlackAboutShinEnd, SlackAboutOwnAnkle, SlackNeither, PushedDowns)),
            };
        }

        public string Summary() => string.Format(Inv,
            "ANKLE_DONE {0}: {1} steps of its own body, {2} getting up, {3} makes that got up; target jumps over {4} degrees: {5} as written (most {6:0.0}), {7} as a turn (most {8:0.0}); "
            + "boots over {9} rad/s on an axis: {10} touching, {11} touching nothing ({12} of them getting up); fastest touching {13:0.0}, touching nothing {14:0.0}, touching nothing getting up {15:0.0}; "
            + "read back: written as today's form of the ask {16} of {17} boot-steps (most apart {18:0.000} degrees{19}); written within {20:0.000} E of the reading at most ({21} compared); damping as today's form {22} of {23}, as the smooth maximum {24} of {25}; boots' ankles at a make, from where they were standing: at most {27:0.000000} m ({28} compared); let go by a push {34} times (no slackening asked); gone down by itself {29} times: boots slackened about their own body's ankle {30}, about the let-go shin's end {31}, neither {32} (the two at most {33:0.000000} m apart); lines not said {26}",
            Who, Steps, RiserSteps, Makes, JumpOver, Jumps, JumpMost, TurnJumps, TurnJumpMost, FastOver, FastTouched, FastFree, FastFreeRiser, FastestTouched, FastestFree, FastestFreeRiser,
            AsToday, Compared, FromTodayMost, ReplicaUnsure > 0 ? "; unsure: AnkleTurnsAtMost is on" : "", OffOverEMost, OffCompared, DampTodaySame, DampCompared, DampBSame, DampCompared, LinesDropped, AnchorFromStandingMost, AnchorsCompared, Downs, SlackAboutOwnAnkle, SlackAboutShinEnd, SlackNeither, PivotsApartMost, PushedDowns);
    }

    // The switches set from the arguments and read back from a running body: each miner made, its own body standing,
    // pushed over, made down to get up by its own body, watched a while, and taken back (build.md part 4). Explicit: a
    // read-back, not a measure of the get-up; no course is counted.
    //   -liveWho Small|Long|Round (all three if left out); -riseSet, -ownBodySet, -ownUnheard, -liveStrength as above;
    //   -ankleWatch seconds of getting up watched (4); -ankleWays Name (a rig: every miner given Name's ways up).
    public sealed class AnkleMendTests
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private MinerChoice choice;
        private MinerWorkPreview look;
        private AnkleWatch watch;
        private bool ownWas;
        private int rateWas;
        private OwnRiser.Way[] waysWas;

        [UnitySetUp]
        public IEnumerator Load()
        {
            rateWas = Time.captureFramerate;
            ownWas = PhysicalFall.GetsUpByItsOwn;
            waysWas = OwnBody.RiseWays;
            AnkleMendArguments.Save();
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
        }

        [TearDown]
        public void Restore()
        {
            if (watch != null) { Object.Destroy(watch.gameObject); watch = null; }
            PhysicalFall.GetsUpByItsOwn = ownWas;
            if (waysWas != null) OwnBody.RiseWays = waysWas;
            Time.captureFramerate = rateWas;
            AnkleMendArguments.Restore();
        }

        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        [UnityTest, Explicit, Timeout(1800000)]
        public IEnumerator TheSwitchesReadBack()
        {
            Assert.That(AnkleWatch.Reads(out string unread), Is.True, unread);
            Time.captureFramerate = 50;
            PhysicalFall.GetsUpByItsOwn = true;
            LogAssert.ignoreFailingMessages = true;
            string set = AnkleMendArguments.Apply();
            Debug.Log("ANKLE_SET " + set);
            Debug.Log("ANKLE_SWITCHES at the start: " + AnkleMendArguments.ReadBack());
            string whoArg = CaptureTools.Argument("-liveWho");
            string[] whos = string.IsNullOrEmpty(whoArg) ? new[] { "Small", "Long", "Round" } : whoArg.Split(',');
            string strengthArg = CaptureTools.Argument("-liveStrength");
            string watchArg = CaptureTools.Argument("-ankleWatch");
            float watchFor = string.IsNullOrEmpty(watchArg) ? 4 : float.Parse(watchArg, Inv);
            // (-ankleWays Name: a rig, not the game: every miner given that miner's ways up, Resources/RiseWays/Name.txt,
            // as a bench gives them (OwnBody.RiseWays). In the game only Small knows a way up; Long and Round get up the
            // old way.)
            string waysArg = CaptureTools.Argument("-ankleWays");
            if (!string.IsNullOrEmpty(waysArg))
            {
                var text = Resources.Load<TextAsset>("RiseWays/" + waysArg);
                Assert.That(text, Is.Not.Null, "No Resources/RiseWays/" + waysArg + ".txt.");
                OwnBody.RiseWays = OwnRiser.Read(text.text);
                Debug.Log("ANKLE_RIG every miner given " + waysArg + "'s ways up (" + OwnBody.RiseWays.Length + " ways): OwnBody.RiseWays, not the game");
            }
            int madeDown = 0;
            foreach (string who in whos)
            {
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == who) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner called " + who + ".");
                choice.Choose(index);
                yield return Wait(.3f);
                var unit = choice.Current;
                if (!string.IsNullOrEmpty(strengthArg)) look.SetStrength(float.Parse(strengthArg, Inv));
                look.SetOwn(true);
                OwnBody own = null;
                for (float until = Time.time + 20; (own == null || !own.Stands) && Time.time < until;) { unit.TryGetComponent(out own); yield return null; }
                Assert.That(own != null && own.Stands, Is.True, who + " did not come to stand by its own joints.");
                for (float until = Time.time + 15; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                yield return Wait(1.5f);
                watch = new GameObject("Ankle watch").AddComponent<AnkleWatch>();
                watch.Own = own; watch.Who = who;
                // (A few steps of it standing watched, for where its boots' ankles are in the body made standing.)
                yield return Wait(.3f);
                for (float until = Time.time + 20; !look.CanPushOver && Time.time < until;) yield return null;
                Assert.That(look.CanPushOver, Is.True, who + " cannot be pushed over.");
                look.PushOver(unit.transform.forward);
                // (Made down to get up by its own body, then watched so long.)
                for (float until = Time.time + 40; own.Riser == null && Time.time < until;) yield return null;
                bool down = own.Riser != null;
                if (down) madeDown++;
                Debug.Log(string.Format(Inv, "ANKLE_RUNNING {0}: made down to get up by its own body {1}; strength {2:0.###} (the panel's {3:0.###}); {4}", who, down,
                    unit.TryGetComponent<PhysicalBody>(out var physical) ? physical.Strength : float.NaN, look.Strength, AnkleMendArguments.ReadBack()));
                if (down) yield return Wait(watchFor);
                Debug.Log(watch.Summary());
                Object.Destroy(watch.gameObject); watch = null;
                // (Taken back, so that the next miner is the only one getting up.)
                var fall = unit.GetComponent<PhysicalFall>();
                if (own.Stands) own.Drop();
                if (fall != null) fall.TakeBack();
                for (float until = Time.time + 20; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
            }
            Debug.Log("ANKLE_SWITCHES at the end: " + AnkleMendArguments.ReadBack());
            Debug.Log(string.Format(Inv, "ANKLE_READBACK_DONE {0} of {1} made down", madeDown, whos.Length));
            Assert.That(madeDown, Is.EqualTo(whos.Length), "Not every miner was made down to get up by its own body.");
        }
    }
}
