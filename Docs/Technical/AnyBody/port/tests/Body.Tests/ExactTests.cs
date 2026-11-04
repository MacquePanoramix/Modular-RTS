// The port against the prototype number for number: bodies, kinematics, contacts, effort split, residual vectors.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace WonderGather.AnyBody.Tests
{
    public static class ExactTests
    {
        static readonly Dictionary<string, BodyModel> Bodies = new Dictionary<string, BodyModel>();

        public static BodyModel Body(string name)
        {
            if (!Bodies.TryGetValue(name, out var b)) Bodies[name] = b = BodyFactory.MakePrototype(name);
            return b;
        }

        // ------------------------------------------------------------------ bodies
        public static void BodiesMatch()
        {
            Console.WriteLine("== bodies: make_body for the 17 prototype bodies");
            using var doc = Ref.Load("body_ref_bodies.json");
            var root = doc.RootElement;
            int n = 0;
            double worst = 0.0;
            foreach (var nameEl in root.GetProperty("order").EnumerateArray())
            {
                string name = nameEl.GetString();
                var e = root.GetProperty("bodies").GetProperty(name);
                var b = BodyFactory.MakePrototype(name);
                foreach (var key in BodyModel.ScalarKeys)
                {
                    double want = e.GetProperty(key).GetDouble();
                    double got = b.Get(key);
                    worst = Math.Max(worst, Math.Abs(got - want) / Math.Max(1e-12, Math.Abs(want)));
                    Check.Near(got, want, 1e-12, $"{name}.{key}");
                }
                var m = e.GetProperty("m");
                Check.Near(b.MassHips, m.GetProperty("hips").GetDouble(), 1e-12, name + ".m.hips");
                Check.Near(b.MassTrunk, m.GetProperty("trunk").GetDouble(), 1e-12, name + ".m.trunk");
                Check.Near(b.MassHead, m.GetProperty("head").GetDouble(), 1e-12, name + ".m.head");
                var pairs = new (string, double[])[]
                {
                    ("ua", b.MassUpperArm), ("fa", b.MassForearm), ("th", b.MassThigh), ("sh", b.MassShin), ("ft", b.MassFoot), ("to", b.MassToes)
                };
                foreach (var (k, arr) in pairs)
                    for (int s = 0; s < 2; s++) Check.Near(arr[s], m.GetProperty(k)[s].GetDouble(), 1e-12, $"{name}.m.{k}[{s}]");
                var cap = e.GetProperty("cap");
                Check.Near(b.CapBack, cap.GetProperty("back").GetDouble(), 1e-12, name + ".cap.back");
                Check.Near(b.CapNeck, cap.GetProperty("neck").GetDouble(), 1e-12, name + ".cap.neck");
                var cp = new (string, double[])[]
                {
                    ("shoulder", b.CapShoulder), ("elbow", b.CapElbow), ("hip", b.CapHip), ("knee", b.CapKnee), ("ankle", b.CapAnkle), ("toe", b.CapToe)
                };
                foreach (var (k, arr) in cp)
                    for (int s = 0; s < 2; s++)
                    {
                        double want = cap.GetProperty(k)[s].GetDouble();
                        worst = Math.Max(worst, Math.Abs(arr[s] - want) / Math.Max(1e-12, Math.Abs(want)));
                        Check.Near(arr[s], want, 1e-12, $"{name}.cap.{k}[{s}]");
                    }
                var rg = e.GetProperty("ranges");
                var keys = new[] { "yaw", "pitch", "roll", "tflex", "tlat", "ttw", "nflex", "nlat", "nrot", "shflex", "shabd", "shrot", "elb", "hflex", "habd", "hrot", "knee", "dorsi", "inv", "toe" };
                for (int r = 0; r < keys.Length; r++)
                {
                    Check.Near(b.RangeLo[r], rg.GetProperty(keys[r])[0].GetDouble(), 0, $"{name}.range.{keys[r]}.lo");
                    Check.Near(b.RangeHi[r], rg.GetProperty(keys[r])[1].GetDouble(), 0, $"{name}.range.{keys[r]}.hi");
                }
                n++;
            }
            Console.WriteLine($"   {n} bodies, worst relative difference {worst:E2}");
            // the restart noise
            var noise = Ref.Arr(root.GetProperty("noise1"));
            for (int i = 0; i < noise.Length; i++) Check.Near(StanceFitter.Noise1[i], noise[i], 0, "noise1");
        }

        // ------------------------------------------------------------------ kinematics, contacts, effort
        static double MaxDiff(Vec3 a, Vec3 b) => Math.Max(Math.Abs(a.X - b.X), Math.Max(Math.Abs(a.Y - b.Y), Math.Abs(a.Z - b.Z)));

        public static void KinematicsMatch()
        {
            Console.WriteLine("== fk: kinematics, solids, contacts, effort split and margin at the 126 saved miner poses");
            using var doc = Ref.Load("body_ref_fk.json");
            double wPts = 0, wR = 0, wCom = 0, wLow = 0, wCon = 0, wF = 0, wEr = 0, wMg = 0;
            int n = 0;
            var tape = new Tape(0);
            foreach (var e in doc.RootElement.EnumerateArray())
            {
                string bname = e.GetProperty("body").GetString();
                var b = Body(bname);
                var st = StanceLibrary.Make(b, e.GetProperty("family").GetString(), Ref.Args(e.GetProperty("args")));
                // body_ref_fk.json holds the res_miners2 fits in the cases' order (names alone are not unique: L/R)
                var cs = Ref.Cases()[n];
                Check.That(cs.Body == bname && cs.Stance == st.Name && cs.File == "res_miners2.json", "fk case order");
                var q = cs.Q;
                var k = new Kin();
                var qd = new D[Pose.Count];
                for (int i = 0; i < Pose.Count; i++) qd[i] = q[i];
                Kinematics.Forward(b, qd, k);
                string tag = bname + " / " + st.Name;
                // points
                foreach (var p in e.GetProperty("pts").EnumerateObject())
                {
                    var want = Ref.V3(p.Value);
                    var got = k[Kin.Point(p.Name)].V;
                    wPts = Math.Max(wPts, MaxDiff(got, want));
                }
                foreach (var p in e.GetProperty("R").EnumerateObject())
                {
                    var arr = Ref.Arr(p.Value);
                    var R = k.R[BodyLinks.Index(p.Name)].V;
                    for (int i = 0; i < 9; i++) wR = Math.Max(wR, Math.Abs(R[i / 3, i % 3] - arr[i]));
                }
                wCom = Math.Max(wCom, MaxDiff(k.Com.V, Ref.V3(e.GetProperty("com"))));
                Check.Near(k.M, e.GetProperty("M").GetDouble(), 1e-14, tag + " M");
                foreach (var c in e.GetProperty("coms").EnumerateArray())
                {
                    int l = BodyLinks.Index(c[0].GetString());
                    Check.Near(k.LinkMass[l], c[1].GetDouble(), 1e-14, tag + " link mass");
                    wCom = Math.Max(wCom, MaxDiff(k.LinkCom[l].V, new Vec3(c[2].GetDouble(), c[3].GetDouble(), c[4].GetDouble())));
                }
                int ji = 0;
                foreach (var j in e.GetProperty("joints").EnumerateArray())
                {
                    var kj = k.Joints[ji++];
                    Check.That(kj.Name == j[0].GetString(), tag + " joint name " + kj.Name);
                    wPts = Math.Max(wPts, MaxDiff(kj.Origin.V, new Vec3(j[1].GetDouble(), j[2].GetDouble(), j[3].GetDouble())));
                    int len = j.GetArrayLength();
                    Check.That(kj.IsHinge == (len == 8), tag + " joint hinge " + kj.Name);
                    if (kj.IsHinge) wR = Math.Max(wR, MaxDiff(kj.Axis.V, new Vec3(j[4].GetDouble(), j[5].GetDouble(), j[6].GetDouble())));
                    Check.Near(kj.Capacity, j[len - 1].GetDouble(), 1e-12, tag + " joint cap " + kj.Name);
                }
                int li = 0;
                foreach (var lw in e.GetProperty("lowest").EnumerateArray())
                {
                    Check.That(BodyLinks.Names[k.LowestLink[li]] == lw[0].GetString(), tag + " lowest link order");
                    wLow = Math.Max(wLow, Math.Abs(k.LowestZ[li].V - lw[1].GetDouble()));
                    li++;
                }
                var C = new List<DContact>();
                Supports.Contacts(b, k, st.Supports, C);
                var cons = e.GetProperty("contacts");
                Check.That(C.Count == cons.GetArrayLength(), $"{tag} contact count {C.Count} vs {cons.GetArrayLength()}");
                for (int i = 0; i < Math.Min(C.Count, cons.GetArrayLength()); i++)
                {
                    var c = cons[i];
                    wCon = Math.Max(wCon, MaxDiff(C[i].P.V, new Vec3(c[0].GetDouble(), c[1].GetDouble(), c[2].GetDouble())));
                    Check.That(BodyLinks.Names[C[i].Link] == c[3].GetString(), tag + " contact link");
                    wCon = Math.Max(wCon, MaxDiff(C[i].N.V, new Vec3(c[4].GetDouble(), c[5].GetDouble(), c[6].GetDouble())));
                    Check.Near(C[i].Mu, c[7].GetDouble(), 0, tag + " mu");
                    string other = c[8].ValueKind == JsonValueKind.Null ? null : c[8].GetString();
                    Check.That((other == null && C[i].Other < 0) || (other != null && C[i].Other >= 0 && BodyLinks.Names[C[i].Other] == other), tag + " other");
                }
                // effort split
                var rr = new List<D>();
                var f = new Vec3[C.Count];
                tape.Reset(0);
                Effort.Append(k, C, 1.0, null, new EffortWork(), rr, f);
                var ef = Ref.Arr(e.GetProperty("effort_f"));
                var er = Ref.Arr(e.GetProperty("effort_r"));
                double W = k.M * BodyFactory.G;
                for (int i = 0; i < C.Count; i++)
                    wF = Math.Max(wF, MaxDiff(f[i], new Vec3(ef[3 * i], ef[3 * i + 1], ef[3 * i + 2])) / W);
                for (int i = 0; i < er.Length; i++) wEr = Math.Max(wEr, Math.Abs(rr[i].V - er[i]));
                // margin
                if (e.GetProperty("margin").ValueKind != JsonValueKind.Null)
                {
                    var p = Statics.Build(b, q, st.Supports);
                    wMg = Math.Max(wMg, Math.Abs(Statics.BaseMargin(p) - e.GetProperty("margin").GetDouble()));
                }
                n++;
            }
            Console.WriteLine($"   {n} poses. max |diff|: points {wPts:E2} m, rotations {wR:E2}, centres {wCom:E2} m, lowest {wLow:E2} m, " +
                              $"contacts {wCon:E2}, effort forces {wF:E2} W, effort residuals {wEr:E2}, base margin {wMg:E2} m");
            Check.That(wPts < 1e-12 && wR < 1e-12 && wCom < 1e-12 && wLow < 1e-12 && wCon < 1e-12, "fk within 1e-12");
            Check.That(wF < 1e-8 && wEr < 1e-8, $"effort within 1e-8 (forces {wF:E2}, residuals {wEr:E2})");
            Check.That(wMg < 1e-9, $"margin within 1e-9 ({wMg:E2})");
        }

        // ------------------------------------------------------------------ residual vectors
        public static void ResidualsMatch(string only = null)
        {
            Console.WriteLine("== resid: start vectors, bounds and residual vectors (at the saved pose and at x0) for the 765 saved fits");
            var cases = Ref.Cases();
            int n = 0, lenBad = 0, bad = 0;
            double wSaved = 0, wX0 = 0, wCost = 0, wStart = 0, wBounds = 0;
            string worstCase = "";
            var r = new List<D>();
            foreach (var cs in cases)
            {
                if (only != null && !cs.Body.StartsWith(only, StringComparison.Ordinal)) continue;
                var b = Body(cs.Body);
                var st = StanceLibrary.Make(b, cs.Family, cs.Args);
                if (!Check.That(st.Name == cs.Stance, $"stance name '{st.Name}' vs '{cs.Stance}'")) continue;
                var p = new StanceProblem(b, st);
                var x0 = p.StartVector();
                Check.That(x0.Length == cs.X0.Length, "free count");
                for (int i = 0; i < x0.Length; i++)
                {
                    wStart = Math.Max(wStart, Math.Abs(x0[i] - cs.X0[i]));
                    wBounds = Math.Max(wBounds, Math.Max(Math.Abs(p.Lo[i] - cs.Lo[i]), Math.Abs(p.Hi[i] - cs.Hi[i])));
                }
                foreach (var (x, want, label) in new[] { (p.FreeOf(cs.Q), cs.RSaved, "saved"), (x0, cs.RX0, "x0") })
                {
                    p.Evaluate(x, false, r);
                    if (r.Count != want.Length)
                    {
                        lenBad++;
                        Check.That(false, $"{cs.Body} / {cs.Stance} @{label}: {r.Count} residuals, want {want.Length}");
                        continue;
                    }
                    double w = 0;
                    int iw = -1;
                    for (int i = 0; i < want.Length; i++)
                    {
                        double d = Math.Abs(r[i].V - want[i]) / Math.Max(1.0, Math.Abs(want[i]));
                        if (d > w)
                        {
                            w = d;
                            iw = i;
                        }
                    }
                    if (label == "saved") wSaved = Math.Max(wSaved, w);
                    else wX0 = Math.Max(wX0, w);
                    if (w > 1e-8)
                    {
                        bad++;
                        Check.That(false, $"{cs.Body} / {cs.Stance} @{label}: residual {iw} got {r[iw].V:G10} want {want[iw]:G10} (of {want.Length})");
                    }
                    if (w > 1e-12 && w > (label == "saved" ? wSaved : wX0) - 1e-30) worstCase = $"{cs.Body} / {cs.Stance} @{label} #{iw}";
                    if (label == "saved")
                        wCost = Math.Max(wCost, Math.Abs(StanceProblem.Cost(r) - cs.Cost) / Math.Max(1e-9, cs.Cost));
                }
                n++;
            }
            Console.WriteLine($"   {n} fits: start vector max |diff| {wStart:E2}, bounds {wBounds:E2}; residuals max rel diff at the saved pose " +
                              $"{wSaved:E2}, at x0 {wX0:E2}; cost rel diff {wCost:E2}; length mismatches {lenBad}, value mismatches {bad}");
            if (worstCase.Length > 0) Console.WriteLine("   largest difference: " + worstCase);
            Check.That(wStart < 1e-12 && wBounds < 1e-12, "start vectors and bounds within 1e-12");
        }
    }
}
