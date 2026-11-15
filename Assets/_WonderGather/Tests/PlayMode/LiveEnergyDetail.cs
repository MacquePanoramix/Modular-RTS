using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WonderGather.Tests
{
    // The ground's touches on one link, gathered over a step of the physics (the ankle split, November 15, 2026). A
    // measure of the test: it reads what the engine says of each contact and pushes nothing. Cleared by whoever reads
    // it, before the step; the engine tells of the step's contacts after it (before WaitForFixedUpdate).
    internal sealed class LiveContacts : MonoBehaviour
    {
        // Contacts, of them with a part of an articulated body (its own, or another's), their impulses summed (N s, as
        // on this link), the deepest separation (metres; less than nothing is pressed in; +inf: none).
        public int Count, Own;
        public Vector3 Impulse;
        public float Deepest = float.PositiveInfinity;
        public readonly List<Vector3> Points = new List<Vector3>(), Impulses = new List<Vector3>();
        private readonly List<ContactPoint> got = new List<ContactPoint>();

        public void Clear() { Count = Own = 0; Impulse = Vector3.zero; Deepest = float.PositiveInfinity; Points.Clear(); Impulses.Clear(); }

        // Their work by estimate (J): each contact's impulse times how the link went at its point, the link going and
        // turning so (the step's means) with its centre where it was at the step's start (the engine finds its
        // contacts there, before it moves the link).
        public double Work(Vector3 goes, Vector3 turns, Vector3 centre)
        {
            double work = 0;
            for (int k = 0; k < Points.Count; k++) work += Vector3.Dot(Impulses[k], goes + Vector3.Cross(turns, Points[k] - centre));
            return work;
        }
        private void OnCollisionEnter(Collision touch) => Take(touch);
        private void OnCollisionStay(Collision touch) => Take(touch);
        private void Take(Collision touch)
        {
            int n = touch.GetContacts(got);
            bool own = touch.articulationBody != null;
            for (int k = 0; k < n; k++)
            {
                var p = got[k];
                Count++;
                if (own) Own++;
                Impulse += p.impulse;
                Deepest = Mathf.Min(Deepest, p.separation);
                Points.Add(p.point); Impulses.Add(p.impulse);
            }
        }
    }

    // What -liveEnergy 2 reads beside the energy count, each step of its ring (the ankle split, November 15, 2026; the
    // main project's Docs/Validation.md, "The energy count in the live courses": every blow-up began in a foot and its
    // toe while a foot was being placed, the foot's link given tens of joules in one step). Read only: reflection reads
    // the riser's and the keeper's fields and writes none; the contact gatherers push nothing.
    //
    //  - Whose torques: the riser's and the keeper's own torque across each joint for the step (each puts its torque on
    //    the two parts a joint joins, equal and opposite, so across a joint it is just that joint's entry), for the
    //    count's split. Each keeps its last torques in an array it writes only when it reckons them anew, and leaves
    //    them there when it does not; so a step's entry is taken as put on only if the array changed since the last
    //    step of the physics (or, for one made since, if it is not all nothing). A reckoning that came out exactly as
    //    the last one is then missed: such steps are counted (Unsure), and what they put on goes to "other".
    //  - The ankles and the toes (FootL, FootR, Toeleft, Toeright): each degree of freedom's drive target, position
    //    and velocity, stiffness, damping and force limit, drive force, limits and how far it is from each, and the
    //    joint's type and motion. What the step was given (targets, gains, limits) is read before it; where the joint
    //    went and the drive's force, after it.
    //  - Each link's contacts over the step: how many, the summed impulse, the deepest separation, and an estimate of
    //    their work: each contact's impulse times how the link went at that point, the mean of its start and its end
    //    (its centre's going and turning as the count has them, the point from its centre at the step's start).
    //  - What the riser asks of the feet: its stage, which leg steps, how far along, where each boot is aimed (its sole
    //    and its turn, the planted place or the step's aimed place) and how far it is from it, how far the ankle's asked
    //    turn moved since the step before, and how slack the ankle is.
    internal sealed class LiveEnergyDetail
    {
        private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        public static readonly string[] Watched = { "FootL", "FootR", "Toeleft", "Toeright" };
        public const string Legend =
            "(joint: each axis (x twist, y and z swing) not fixed: its motion; the drive's target and where the joint is (degrees) and how far off the target it is; how fast it turns (rad/s, at the step's end);"
            + " its stiffness k (N m/rad), damping c (N m s/rad) and force limit (N m), the drive's force over the step (N m); its limits (degrees) and how far it is inside each (less than nothing: past it). Targets, gains and limits as the step was given them.)\n"
            + "(contacts: each link touching anything over the step: how many contacts (of them with a part of a body), their summed impulse on the link (N s), the deepest separation (m; less than nothing: pressed in), and ~ their work by estimate (J): each impulse times the link's mean going at that point.)\n"
            + "(riser: its stage, the leg that leads and the one that steps, whether the step began and its place was aimed, how far along it is (-: not begun), landed, planting, the leg its weight is off; each boot: where it is held (planted, or the step's aimed place) and how far from it, how far its asked turn moved since the step before, the turn asked of its joint (degrees, the drive's target before what learns adds), and how much of its spring and strength it holds with.)";

        // Steps whose riser's (keeper's) torques were not all nothing but unchanged from the step before: taken as not
        // put on. And steps where the keeper's torques were taken as put on while the riser had the body.
        public int Unsure, KeeperWhileRiser, Steps;
        public int ContactsSeen;
        public float OtherMost;
        public double SplitOffMost, SplitShareMost;
        public string SplitOffSaid = "";
        public string OtherMostSaid = "";

        private readonly string[] ring = new string[LiveEnergy.RingSteps];
        private readonly int[] ringFrame = new int[LiveEnergy.RingSteps];
        private int ringNext, ringCount;

        private ArticulationBody[] linksSeen;
        private LiveContacts[] contacts = new LiveContacts[0];
        private int[] watched = new int[0];

        // ---- whose torques ----
        private static readonly FieldInfo riserTorque = typeof(OwnRiser).GetField("torque", Hidden), keeperOf = typeof(OwnBody).GetField("keeper", Hidden);
        private object riserSeen, keeperSeen;
        private readonly Vector3[] riserWas = new Vector3[OwnBody.Count], keeperWas = new Vector3[OwnBody.Count];
        private readonly Vector3[] riserPut = new Vector3[OwnBody.Count], keeperPut = new Vector3[OwnBody.Count];
        public bool RiserHad;

        public static bool Reads(out string why)
        {
            why = riserTorque == null ? "OwnRiser has no field torque" : keeperOf == null ? "OwnBody has no field keeper" : null;
            if (why == null) foreach (var f in RiserFields) if (typeof(OwnRiser).GetField(f, Hidden) == null) { why = "OwnRiser has no field " + f; break; }
            return why == null;
        }

        private static Vector3[] RiserTorques(object riser) => riser != null ? riserTorque.GetValue(riser) as Vector3[] : null;
        private static Vector3[] KeeperTorques(object keeper) => keeper is OwnKeeper k ? k.GaveAs : null;

        // One of them: what it put on this step (into put), from its array now against what it was the step before.
        private bool Put(object now, ref object seen, Vector3[] was, Vector3[] put, System.Func<object, Vector3[]> read, int from)
        {
            for (int j = 0; j < put.Length; j++) put[j] = Vector3.zero;
            bool any = false;
            if (seen != null && seen != now)
            {
                // (The one that had it before, if it put on anything before it was let go in this step.)
                var gone = read(seen);
                if (gone != null && Differs(gone, was)) { Add(put, gone, from); any = true; }
                seen = null;
            }
            if (now != null)
            {
                var a = read(now);
                if (a != null)
                {
                    bool fresh = seen != now;
                    if (fresh ? !AllNothing(a) : Differs(a, was)) { Add(put, a, from); any = true; }
                    else if (!fresh && !AllNothing(a)) Unsure++;
                    for (int j = 0; j < was.Length && j < a.Length; j++) was[j] = a[j];
                }
                seen = now;
            }
            return any;
        }

        private static bool Differs(Vector3[] a, Vector3[] b) { for (int j = 0; j < a.Length && j < b.Length; j++) if (a[j] != b[j]) return true; return false; }
        private static bool AllNothing(Vector3[] a) { foreach (var v in a) if (v != Vector3.zero && !float.IsNaN(v.x)) return false; return true; }
        private static void Add(Vector3[] put, Vector3[] a, int from)
        {
            for (int j = from; j < put.Length && j < a.Length; j++)
            {
                var v = a[j];
                // (A torque not finite is not put on by the riser: it goes on to the next joint.)
                if (float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z)) continue;
                put[j] += v;
            }
        }

        // Last thing before the step, every step of the physics (counted or not, so what was there before is known):
        // the riser's and the keeper's torques for this step.
        public void Track(OwnBody own)
        {
            object riser = own != null ? own.Riser : null, keeper = own != null ? keeperOf.GetValue(own) : null;
            RiserHad = riser != null;
            // (The riser puts its torques on joints 1 on; the keeper on the legs' joints only, Thigh on.)
            Put(riser, ref riserSeen, riserWas, riserPut, RiserTorques, 1);
            bool kept = Put(keeper, ref keeperSeen, keeperWas, keeperPut, KeeperTorques, OwnKeeper.Thigh);
            if (kept && riser != null) KeeperWhileRiser++;
        }

        // (And, for a step that is counted, onto the count's links.)
        public void Onto(LiveEnergy count, OwnBody own)
        {
            var links = count.Links;
            for (int i = 0; i < links.Length && i < count.ByRiser.Length; i++) { count.ByRiser[i] = Vector3.zero; count.ByKeeper[i] = Vector3.zero; }
            if (own == null) return;
            for (int p = 1; p < OwnBody.Count; p++)
            {
                int i = System.Array.IndexOf(links, own.Part(p));
                if (i <= 0 || i >= count.ByRiser.Length) continue;
                count.ByRiser[i] = riserPut[p];
                count.ByKeeper[i] = keeperPut[p];
            }
        }

        // ---- the ankles, the toes, the contacts, the riser's asks: before the step ----
        private struct Axis { public bool Has; public ArticulationDofLock Motion; public ArticulationDrive Drive; public int Dof; }
        private Axis[][] axes = new Axis[0][];
        private ArticulationJointType[] jointType = new ArticulationJointType[0];
        private string riserSaid = "";
        private readonly Quaternion[] askedWas = new Quaternion[2];
        private readonly bool[] askedWasKnown = new bool[2];
        private object askedOf;

        public void Before(LiveEnergy count, OwnBody own)
        {
            var links = count.Links;
            if (links != linksSeen)
            {
                linksSeen = links;
                contacts = new LiveContacts[links.Length];
                for (int i = 0; i < links.Length; i++)
                    if (links[i] != null && !links[i].TryGetComponent(out contacts[i])) contacts[i] = links[i].gameObject.AddComponent<LiveContacts>();
                watched = new int[Watched.Length];
                for (int w = 0; w < Watched.Length; w++) watched[w] = System.Array.IndexOf(count.Names, Watched[w]);
                axes = new Axis[Watched.Length][];
                jointType = new ArticulationJointType[Watched.Length];
            }
            foreach (var c in contacts) if (c != null) c.Clear();
            for (int w = 0; w < watched.Length; w++)
            {
                int i = watched[w];
                if (i <= 0 || links[i] == null) { axes[w] = null; continue; }
                var link = links[i];
                jointType[w] = link.jointType;
                var a = axes[w] ?? (axes[w] = new Axis[3]);
                bool spherical = link.jointType == ArticulationJointType.SphericalJoint;
                a[0] = new Axis { Has = link.jointType != ArticulationJointType.FixedJoint, Motion = link.twistLock, Drive = link.xDrive };
                a[1] = new Axis { Has = spherical, Motion = link.swingYLock, Drive = link.yDrive };
                a[2] = new Axis { Has = spherical, Motion = link.swingZLock, Drive = link.zDrive };
                // (The joint's degrees of freedom, in its reduced space: its axes not locked, twist, then swing Y, then
                // swing Z. That order is the engine's; the dof count is checked against it.)
                int dof = 0;
                for (int x = 0; x < 3; x++) { a[x].Dof = a[x].Has && a[x].Motion != ArticulationDofLock.LockedMotion ? dof++ : -1; }
                if (dof != link.dofCount) for (int x = 0; x < 3; x++) a[x].Dof = -2;
            }
            riserSaid = RiserAsks(own);
        }

        // ---- the riser's asks of the feet ----
        private static readonly string[] RiserFields = { "stage", "leads", "stepBegun", "stepAimed", "landed", "planting", "liftAt", "got", "offLeg", "stepTo", "stepBoot", "plantedLeg", "plantSole", "plantBoot", "aimNow", "wayAsks", "slackNow", "springOf", "hasOf", "soleAt" };
        private static readonly Dictionary<string, FieldInfo> riserField = new Dictionary<string, FieldInfo>();
        private static T Of<T>(object o, string name)
        {
            if (!riserField.TryGetValue(name, out var f)) riserField[name] = f = typeof(OwnRiser).GetField(name, Hidden);
            object v = f != null ? f.GetValue(o) : null;
            return v is T t ? t : default;
        }

        private string RiserAsks(OwnBody own)
        {
            var riser = own != null ? own.Riser : null;
            if (riser == null) { askedOf = null; return "  riser | none (the keeper has the body)\n"; }
            if (askedOf != riser) { askedOf = riser; askedWasKnown[0] = askedWasKnown[1] = false; }
            var stage = Of<OwnRiser.Stage>(riser, "stage");
            int leads = Of<int>(riser, "leads"), offLeg = Of<int>(riser, "offLeg");
            int stepping = stage == null || stage.steps < 0 ? -1 : stage.steps == 0 ? leads : 1 - leads;
            bool begun = Of<bool>(riser, "stepBegun"), aimed = Of<bool>(riser, "stepAimed"), landed = Of<bool>(riser, "landed"), planting = Of<bool>(riser, "planting");
            float liftAt = Of<float>(riser, "liftAt"), got = Of<float>(riser, "got");
            float along = stage != null && begun ? (stage.over > 0 ? Mathf.SmoothStep(0, 1, (got - liftAt) / stage.over) : 1) : float.NaN;
            var planted = Of<bool[]>(riser, "plantedLeg");
            var plantSole = Of<Vector3[]>(riser, "plantSole");
            var plantBoot = Of<Quaternion[]>(riser, "plantBoot");
            var aimNow = Of<Quaternion[]>(riser, "aimNow");
            var wayAsks = Of<Vector3[]>(riser, "wayAsks");
            var slack = Of<float[]>(riser, "slackNow");
            var springOf = Of<float[]>(riser, "springOf");
            var hasOf = Of<float[]>(riser, "hasOf");
            var soleAt = Of<Vector3[]>(riser, "soleAt");
            Vector3 stepTo = Of<Vector3>(riser, "stepTo");
            Quaternion stepBoot = Of<Quaternion>(riser, "stepBoot");
            var t = new StringBuilder();
            t.Append(string.Format(Inv, "  riser | {0} leads {1} steps {2} begun {3} aimed {4} along {5} landed {6} planting {7} off {8} in pose {9:0.00} s",
                stage != null ? stage.name : "?", Leg(leads), Leg(stepping), begun ? 1 : 0, aimed ? 1 : 0, float.IsNaN(along) ? "-" : along.ToString("0.00", Inv), landed ? 1 : 0, planting ? 1 : 0, Leg(offLeg), got));
            for (int i = 0; i < 2; i++)
            {
                int j = OwnBody.Foot + i;
                var boot = own.Part(j);
                t.Append(i == 0 ? " | FootL" : " | FootR");
                if (boot == null || soleAt == null) { t.Append(" ?;"); continue; }
                Vector3 sole = boot.transform.TransformPoint(soleAt[i]);
                Quaternion turned = boot.transform.rotation;
                bool isPlanted = planted != null && planted[i] && planting && i != offLeg;
                if (isPlanted && plantSole != null && plantBoot != null)
                    t.Append(string.Format(Inv, " planted, sole off {0:0.000} m (up {1:+0.000;-0.000}), boot off {2:0.0} deg", (sole - plantSole[i]).magnitude, sole.y - plantSole[i].y, Quaternion.Angle(turned, plantBoot[i])));
                else if (i == stepping && aimed)
                    t.Append(string.Format(Inv, " stepping, sole off aimed {0:0.000} m (up {1:+0.000;-0.000}), boot off aimed {2:0.0} deg", (sole - stepTo).magnitude, sole.y - stepTo.y, Quaternion.Angle(turned, stepBoot)));
                else t.Append(i == stepping ? " stepping, not yet aimed" : " posed");
                if (aimNow != null && j < aimNow.Length)
                {
                    float jump = askedWasKnown[i] ? Quaternion.Angle(askedWas[i], aimNow[j]) : float.NaN;
                    askedWas[i] = aimNow[j]; askedWasKnown[i] = true;
                    t.Append(string.Format(Inv, ", asked turn moved {0:0.00} deg", jump));
                }
                if (wayAsks != null && j < wayAsks.Length) t.Append(string.Format(Inv, ", target asked ({0:0.#}, {1:0.#}, {2:0.#}) deg", wayAsks[j].x, wayAsks[j].y, wayAsks[j].z));
                if (slack != null && springOf != null && hasOf != null && j < slack.Length)
                    t.Append(string.Format(Inv, ", held {0:0.00} of spring {1:0.#} N m/rad and {2:0.#} N m", slack[j], springOf[j], hasOf[j]));
                t.Append(';');
            }
            return t.Append('\n').ToString();
        }

        private static string Leg(int i) => i == 0 ? "L" : i == 1 ? "R" : i == 2 ? "both" : "-";

        // ---- after the step: the joints, the contacts, and the step's text into the ring ----
        public void After(LiveEnergy count, int frame)
        {
            Steps++;
            var links = count.Links;
            var t = new StringBuilder();
            for (int w = 0; w < watched.Length; w++)
            {
                int i = watched[w];
                if (i <= 0 || i >= links.Length || links[i] == null || axes[w] == null) { t.Append("  joint ").Append(Watched[w]).Append(" | not here\n"); continue; }
                var link = links[i];
                var pos = link.jointPosition; var vel = link.jointVelocity; var force = link.driveForce;
                t.Append("  joint ").Append(Watched[w]).Append(' ').Append(Kind(jointType[w]));
                for (int x = 0; x < 3; x++)
                {
                    var a = axes[w][x];
                    if (!a.Has) continue;
                    var d = a.Drive;
                    t.Append(" | ").Append(x == 0 ? "x" : x == 1 ? "y" : "z").Append(' ').Append(Motion(a.Motion));
                    if (a.Dof >= 0 && a.Dof < pos.dofCount)
                    {
                        float at = pos[a.Dof] * Mathf.Rad2Deg;
                        t.Append(string.Format(Inv, " target {0:0.0} at {1:0.0} deg (off {2:+0.0;-0.0}) going {3:+0.00;-0.00} rad/s", d.target, at, at - d.target, vel[a.Dof]));
                        t.Append(string.Format(Inv, " k {0:0.###} c {1:0.####} most {2:0.##} force {3:+0.###;-0.###}", d.stiffness, d.damping, d.forceLimit, force[a.Dof]));
                        if (a.Motion == ArticulationDofLock.LimitedMotion)
                            t.Append(string.Format(Inv, " limits {0:0.#} ({1:+0.0;-0.0}) to {2:0.#} ({3:+0.0;-0.0})", d.lowerLimit, at - d.lowerLimit, d.upperLimit, d.upperLimit - at));
                        if (d.targetVelocity != 0) t.Append(string.Format(Inv, " target going {0:0.###}", d.targetVelocity));
                    }
                    else if (a.Dof == -2) t.Append(string.Format(Inv, " (dofs not as read: {0})", link.dofCount));
                    else t.Append(string.Format(Inv, " k {0:0.###}", d.stiffness));
                }
                t.Append('\n');
            }
            t.Append("  contacts |");
            int touching = 0;
            for (int i = 0; i < links.Length && i < contacts.Length; i++)
            {
                var c = contacts[i];
                if (c == null || c.Count == 0) continue;
                touching++;
                ContactsSeen += c.Count;
                double work = c.Work(count.MeanGoes(i), count.MeanTurns(i), count.CentreBefore(i));
                t.Append(string.Format(Inv, " {0} n {1}{2} J ({3:0.###}, {4:0.###}, {5:0.###}) N s deepest {6:+0.0000;-0.0000} m work ~{7:+0.###;-0.###} J;", count.Names[i], c.Count, c.Own > 0 ? "(" + c.Own.ToString(Inv) + " body)" : "", c.Impulse.x, c.Impulse.y, c.Impulse.z, c.Deepest, work));
            }
            if (touching == 0) t.Append(" none");
            t.Append('\n');
            t.Append(riserSaid);
            if (count.SplitOff > SplitOffMost) { SplitOffMost = count.SplitOff; SplitOffSaid = string.Format(Inv, "frame {0}", frame); }
            SplitShareMost = System.Math.Max(SplitShareMost, count.SplitShare);
            for (int i = 1; i < links.Length && i < count.GivenTorques.Length; i++)
            {
                float other = Mathf.Abs(count.GivenTorques[i] - count.GivenRiser[i] - count.GivenKeeper[i]);
                if (other > OtherMost) { OtherMost = other; OtherMostSaid = string.Format(Inv, "{0} at frame {1}", count.Names[i], frame); }
            }
            ring[ringNext] = t.ToString();
            ringFrame[ringNext] = frame;
            ringNext = (ringNext + 1) % LiveEnergy.RingSteps;
            ringCount = Mathf.Min(LiveEnergy.RingSteps, ringCount + 1);
        }

        private static string Kind(ArticulationJointType k) => k == ArticulationJointType.SphericalJoint ? "spherical" : k == ArticulationJointType.RevoluteJoint ? "revolute" : k == ArticulationJointType.PrismaticJoint ? "prismatic" : "fixed";
        private static string Motion(ArticulationDofLock m) => m == ArticulationDofLock.LockedMotion ? "locked" : m == ArticulationDofLock.LimitedMotion ? "limited" : "free";

        // The k-th step of the ring, oldest first, if it is of that frame (the count's ring and this one are filled in
        // the same steps).
        public string Lines(int k, int frame)
        {
            int at = (ringNext - ringCount + k + LiveEnergy.RingSteps) % LiveEnergy.RingSteps;
            if (k < 0 || k >= ringCount || ringFrame[at] != frame || ring[at] == null) return "  (no detail for this step)\n";
            return ring[at];
        }

        public string Line() => string.Format(Inv, "LIVE_ENERGY_DETAIL {0} steps read; riser's or keeper's torques unsure in {1} steps; keeper's torques put on while the riser had the body in {2} steps; the most other torques gave a link in a step {3:0.###} J ({4}); {5} contacts read; the split's parts at most {6:0.0E+0} J off a link's given work ({7}), at most {8:0.0E+0} of the parts' sizes",
            Steps, Unsure, KeeperWhileRiser, OtherMost, OtherMostSaid, ContactsSeen, SplitOffMost, SplitOffSaid, SplitShareMost);
    }
}
