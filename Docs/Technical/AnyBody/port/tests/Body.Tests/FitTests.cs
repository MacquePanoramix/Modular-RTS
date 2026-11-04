// The fitter: exact Jacobians against finite differences, warm starts from the prototype's poses, fits from the
// stances' start poses compared with the prototype's saved fits, and timings.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WonderGather.AnyBody.Tests
{
    public static class FitTests
    {
        // ------------------------------------------------------------------ Jacobians
        public static void JacobianMatches()
        {
            Console.WriteLine("== jac: AD Jacobian vs central differences (h 1e-6), every 5th saved fit, near the saved pose and near x0");
            Console.WriteLine("   (each pose moved by N(0, 0.01) so no tie or band edge sits exactly on it; hull rounding off)");
            Margin.RoundHull = false;
            var rng = new Random(7);
            var cases = Ref.Cases();
            var r = new List<D>();
            var rp = new List<D>();
            var rm = new List<D>();
            long entries = 0, off = 0;
            double worstAll = 0;
            string worstAt = "";
            int nCases = 0;
            var errs = new List<double>();
            var byLabel = new Dictionary<string, int>();
            for (int ci = 0; ci < cases.Count; ci += 5)
            {
                var cs = cases[ci];
                var b = ExactTests.Body(cs.Body);
                var st = StanceLibrary.Make(b, cs.Family, cs.Args);
                var p = new StanceProblem(b, st);
                p.Labels = new List<string>();
                foreach (var x0 in new[] { p.FreeOf(cs.Q), p.StartVector() })
                {
                    var x = (double[])x0.Clone();
                    for (int kk = 0; kk < x.Length; kk++)
                    {
                        double u1 = 1.0 - rng.NextDouble(), u2 = rng.NextDouble();
                        x[kk] = Math.Min(Math.Max(x[kk] + 0.01 * Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2), p.Lo[kk]), p.Hi[kk]);
                    }
                    p.Evaluate(x, true, r);
                    var labels = p.Labels.ToArray();
                    int m = r.Count, n = p.N;
                    var J = new double[m * n];
                    for (int i = 0; i < m; i++) p.Tape.Grad(r[i], J, i * n);
                    for (int kk = 0; kk < n; kk++)
                    {
                        double h = 1e-6;
                        var xp = (double[])x.Clone();
                        var xm = (double[])x.Clone();
                        xp[kk] += h;
                        xm[kk] -= h;
                        p.Evaluate(xp, false, rp);
                        var vp = rp.Select(d => d.V).ToArray();
                        p.Evaluate(xm, false, rm);
                        var vm = rm.Select(d => d.V).ToArray();
                        if (vp.Length != m || vm.Length != m) continue;
                        for (int i = 0; i < m; i++)
                        {
                            double fd = (vp[i] - vm[i]) / (2 * h);
                            double ad = J[i * n + kk];
                            double err = Math.Abs(fd - ad) / Math.Max(1.0, Math.Abs(fd));
                            entries++;
                            errs.Add(err);
                            if (err > 1e-4)
                            {
                                off++;
                                byLabel[labels[i]] = byLabel.TryGetValue(labels[i], out var c0) ? c0 + 1 : 1;
                            }
                            if (err > worstAll)
                            {
                                worstAll = err;
                                worstAt = $"{cs.Body} / {cs.Stance}: residual {i} ({labels[i]}), var {Pose.Names[p.Free[kk]]}: AD {ad:G8}, FD {fd:G8}";
                            }
                        }
                    }
                }
                nCases++;
            }
            errs.Sort();
            double p50 = errs[errs.Count / 2], p999 = errs[(int)(errs.Count * 0.999)];
            Console.WriteLine($"   {nCases} fits x 2 poses, {entries} entries: median rel err {p50:E1}, 99.9th pct {p999:E1}, " +
                              $"{off} entries ({100.0 * off / entries:F3}%) above 1e-4 (kinks: band edges, min/hull switches inside +-h)");
            Console.WriteLine("   worst: " + worstAt);
            Console.WriteLine("   off entries by residual kind: " + string.Join(", ", byLabel.OrderByDescending(kv => kv.Value).Select(kv => kv.Key + " " + kv.Value)));
            Margin.RoundHull = true;
            Check.That(p999 < 1e-5 && off < 0.0005 * entries, "AD Jacobian agrees with finite differences");
        }

        // ------------------------------------------------------------------ warm start from the prototype's own optimum
        public static void WarmFromSaved(List<string> opts)
        {
            Console.WriteLine("== warm: LM started at the prototype's saved poses (the 126 miner fits): it must not get worse");
            var cases = Ref.Cases().Where(c => c.File == "res_miners2.json").ToList();
            int better = 0, same = 0, worse = 0;
            double totalMs = 0, worstDq = 0;
            var fo = new FitOptions { Restarts = 0 };
            foreach (var cs in cases)
            {
                var b = ExactTests.Body(cs.Body);
                var st = StanceLibrary.Make(b, cs.Family, cs.Args);
                fo.Start = cs.Q;
                var fit = StanceFitter.Fit(b, st, fo);
                totalMs += fit.Millis;
                double dq = PoseDiffDeg(fit.Q, cs.Q, out _);
                if (fit.Cost < cs.Cost * (1 - 1e-6) - 1e-12) better++;
                else if (fit.Cost <= cs.Cost * (1 + 1e-9) + 1e-12) same++;
                else
                {
                    worse++;
                    Check.That(false, $"{cs.Body} / {cs.Stance}: warm fit cost {fit.Cost:G6} > saved {cs.Cost:G6}");
                }
                worstDq = Math.Max(worstDq, dq);
            }
            Console.WriteLine($"   {cases.Count} fits: lower {better}, equal {same}, higher {worse}; {totalMs / cases.Count:F1} ms per fit; " +
                              $"largest pose change {worstDq:F1} deg");
        }

        public static double PoseDiffDeg(double[] a, double[] b, out string worstVar)
        {
            double w = 0;
            worstVar = "";
            for (int i = 3; i < Pose.Count; i++)
            {
                double d = Math.Abs(a[i] - b[i]) / Pose.D2R;
                if (d > w)
                {
                    w = d;
                    worstVar = Pose.Names[i];
                }
            }
            return w;
        }

        public static double PoseRmsDeg(double[] a, double[] b)
        {
            double s = 0;
            int n = 0;
            for (int i = 3; i < Pose.Count; i++)
            {
                double d = (a[i] - b[i]) / Pose.D2R;
                s += d * d;
                n++;
            }
            return Math.Sqrt(s / n);
        }

        // ------------------------------------------------------------------ fits from the start pose, against the saved fits
        public static readonly string[] FitBodies =
        {
            "Small", "Long", "Round", "Small S0.5", "Toddler-like (Small, head x1.3, legs x0.85)", "Giant (Long x1.6, strength ~ L^3)",
            "Long, arms x0.65", "Round, stiff knees (100)", "Heavy trunk (Round, trunk+hips x1.5, S0.7, lean 30)", "Long, right leg 0.4"
        };

        public sealed class Row
        {
            public string Body, Stance, Verdict, Status;
            public double Cost, SavedCost, DqMax, DqRms, Dz, ContactErr, SavedContactErr, Pen, SavedPen, Margin, SavedMargin, Ms, PySecs;
            public double? RelMargin, SavedRelMargin;
            public int Starts, Iter, Evals;
            public string DqVar;
            public double WarmCost = double.NaN;
        }

        public static void FitsMatch(List<string> opts)
        {
            var bodies = FitBodies.ToList();
            var bodyOpt = opts.FirstOrDefault(o => o.StartsWith("--bodies="));
            if (bodyOpt != null) bodies = bodyOpt.Substring(9).Split('|').ToList();
            var famOpt = opts.FirstOrDefault(o => o.StartsWith("--families="));
            var fams = famOpt?.Substring(11).Split('|').ToHashSet();
            var outOpt = opts.FirstOrDefault(o => o.StartsWith("--csv="));
            Console.WriteLine($"== fit: LM from each stance's start pose, {bodies.Count} bodies x the prototype's 45 stances, against the saved fits");
            var cases = Ref.Cases();
            var rows = new List<Row>();
            var fo = new FitOptions();
            foreach (var o in opts)
            {
                // --maxit=N --restarts=N --restartcost=X --xtol=X --ftol=X: the fitter's settings, to read their effect
                var kv = o.TrimStart('-').Split('=');
                if (kv.Length != 2) continue;
                if (!double.TryParse(kv[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double val)) continue;
                switch (kv[0])
                {
                    case "maxit": fo.MaxIterations = (int)val; break;
                    case "restarts": fo.Restarts = (int)val; break;
                    case "restartcost": fo.RestartCost = val; break;
                    case "xtol": fo.XTol = val; break;
                    case "ftol": fo.FTol = val; break;
                }
            }
            Console.WriteLine($"   fitter: max {fo.MaxIterations} iterations per start, {fo.Restarts} extra start(s) when the cost >= {fo.RestartCost}, " +
                              $"ftol {fo.FTol:G2}, xtol {fo.XTol:G2}");
            // --atlas: a body made from a miner starts each stance from that miner's fit of it (the atlas idea of
            // stances_strategy.md 2.3: warm starts from a neighbouring body), then the usual extra starts
            // --atlas-extra: the stance's own start first, the miner's fit as the first extra start (one start more)
            bool atlasExtra = opts.Contains("--atlas-extra");
            bool atlas = opts.Contains("--atlas") || atlasExtra;
            var minerFits = new Dictionary<string, double[]>();
            if (atlas) Console.WriteLine(atlasExtra
                ? "   atlas-extra: a body made from a miner gets the miner's fit of the stance as its first extra start (the miners first)"
                : "   atlas: each body made from a miner starts from the miner's fit of the stance (the miners first)");
            foreach (var bn in bodies)
            {
                var b = ExactTests.Body(bn);
                var variant = BodyFactory.PrototypeBodies.FirstOrDefault(v => v.Name == bn);
                string src = variant != null ? variant.Source : bn;
                foreach (var cs in cases.Where(c => c.Body == bn))
                {
                    if (fams != null && !fams.Contains(cs.Family)) continue;
                    var st = StanceLibrary.Make(b, cs.Family, cs.Args);
                    string key = cs.Family + "|" + string.Join("|", cs.Args.Select(a => Convert.ToString(a, CultureInfo.InvariantCulture)));
                    var foHere = fo;
                    if (atlas && src != bn && minerFits.TryGetValue(src + "#" + key, out var qMiner))
                        foHere = atlasExtra
                            ? new FitOptions
                            {
                                MaxIterations = fo.MaxIterations, Restarts = fo.Restarts + 1, RestartCost = fo.RestartCost, FTol = fo.FTol, XTol = fo.XTol,
                                ExtraStarts = new[] { qMiner }
                            }
                            : new FitOptions { MaxIterations = fo.MaxIterations, Restarts = fo.Restarts, RestartCost = fo.RestartCost, FTol = fo.FTol, XTol = fo.XTol, Start = qMiner };
                    var fit = StanceFitter.Fit(b, st, foHere);
                    if (src == bn) minerFits[bn + "#" + key] = fit.Q;
                    var rep = Statics.Report(b, st, fit.Q);
                    var row = new Row
                    {
                        Body = bn, Stance = st.Name, Cost = fit.Cost, SavedCost = cs.Cost, DqMax = PoseDiffDeg(fit.Q, cs.Q, out var dv), DqVar = dv,
                        DqRms = PoseRmsDeg(fit.Q, cs.Q), Dz = 1000 * (fit.Q[Pose.Z] - cs.Q[Pose.Z]), ContactErr = rep.ContactErrMm,
                        SavedContactErr = cs.ContactErrMm, Pen = rep.PenetrationMm, SavedPen = cs.PenetrationMm, Margin = rep.BaseMargin,
                        SavedMargin = cs.SpMargin, RelMargin = rep.ReleaseMargin, SavedRelMargin = cs.ReleaseMargin, Ms = fit.Millis,
                        PySecs = cs.Secs ?? 0, Starts = fit.Starts, Iter = fit.Iterations, Evals = fit.JacobianEvaluations, Status = fit.Status
                    };
                    row.Verdict = Verdict(row);
                    if (row.Verdict == "worse")
                    {
                        // the cause: started at the prototype's own optimum, does the C# stay there (same problem,
                        // another basin reached from the start pose) or leave it (a different problem)?
                        var warmOpt = new FitOptions { MaxIterations = fo.MaxIterations, Restarts = 0, Start = cs.Q };
                        row.WarmCost = StanceFitter.Fit(b, st, warmOpt).Cost;
                    }
                    rows.Add(row);
                }
            }
            Summarise(rows);
            var worse = rows.Where(r => r.Verdict == "worse").ToList();
            if (worse.Count > 0)
            {
                int stays = worse.Count(r => r.WarmCost <= r.SavedCost * (1 + 1e-9) + 1e-12);
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "   the {0} worse fits, started at the prototype's optimum instead: {1} stay at or below its cost (mean {2:F4} vs saved {3:F4}): " +
                    "the same problem, another basin reached from the start pose", worse.Count, stays, worse.Average(r => r.WarmCost), worse.Average(r => r.SavedCost)));
                Check.That(stays == worse.Count, "every worse fit is a basin, not a different problem (warm from the saved pose never ends higher)");
            }
            int matchOrBetter = rows.Count(r => r.Verdict == "match" || r.Verdict == "better");
            Check.That(rows.Count == 0 || rows.Count(r => r.Verdict == "worse") <= 0.15 * rows.Count, $"at most 15% of the fits worse than the prototype's ({rows.Count(r => r.Verdict == "worse")} of {rows.Count})");
            Check.That(rows.All(r => r.ContactErr <= Math.Max(2.0, r.SavedContactErr + 1.0) || r.Verdict != "match"), "every matching fit has its contacts within 2 mm");
            Console.WriteLine($"   match or better: {matchOrBetter} of {rows.Count}");
            if (outOpt != null) WriteCsv(rows, outOpt.Substring(6));
        }

        /// <summary>"match": the same quality of fit as the prototype's (cost within 10% or 0.02, contacts within 2 mm,
        /// no new penetration over 2 mm, base margin within 10 mm); "better"/"worse": cost lower/higher beyond that.</summary>
        public static string Verdict(Row r)
        {
            double tol = Math.Max(0.02, 0.1 * r.SavedCost);
            bool costSame = Math.Abs(r.Cost - r.SavedCost) <= tol;
            bool geomSame = r.ContactErr <= Math.Max(2.0, r.SavedContactErr + 1.0) && r.Pen <= r.SavedPen + 2.0 &&
                            Math.Abs(r.Margin - r.SavedMargin) <= 0.010;
            if (costSame && geomSame) return "match";
            if (r.Cost < r.SavedCost - tol) return "better";
            if (r.Cost > r.SavedCost + tol) return "worse";
            return "same cost, other optimum";
        }

        static void Summarise(List<Row> rows)
        {
            Console.WriteLine($"   {"body",-10} {"stance",-60} {"cost",8} {"saved",8} {"dq max",7} {"var",-8} {"rms",5} {"dz mm",6} {"cerr",5} {"pen",5} {"mrg mm",7} {"saved",6} {"ms",6} {"st",2} {"verdict"}");
            foreach (var r in rows)
            {
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "   {0,-10} {1,-60} {2,8:F4} {3,8:F4} {4,7:F1} {5,-8} {6,5:F1} {7,6:F0} {8,5:F1} {9,5:F1} {10,7:F1} {11,6:F1} {12,6:F0} {13,2} {14}",
                    Short(r.Body), Trim(r.Stance, 60), r.Cost, r.SavedCost, r.DqMax, r.DqVar, r.DqRms, r.Dz, r.ContactErr, r.Pen, 1000 * r.Margin,
                    1000 * r.SavedMargin, r.Ms, r.Starts, r.Verdict));
            }
            var groups = rows.GroupBy(r => r.Verdict).OrderBy(g => g.Key);
            Console.WriteLine("   verdicts: " + string.Join(", ", groups.Select(g => $"{g.Key} {g.Count()}")));
            foreach (var g in rows.GroupBy(r => r.Body))
            {
                var l = g.ToList();
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "   {0,-10}: {1} fits, match {2}, better {3}, worse {4}, other {5}; mean {6:F1} ms (median {7:F1}, max {8:F0}); " +
                    "mean cost {9:F3} vs saved {10:F3}; prototype mean {11:F0} s",
                    Short(g.Key), l.Count, l.Count(r => r.Verdict == "match"), l.Count(r => r.Verdict == "better"), l.Count(r => r.Verdict == "worse"),
                    l.Count(r => r.Verdict.StartsWith("same")), l.Average(r => r.Ms), Median(l.Select(r => r.Ms)), l.Max(r => r.Ms),
                    l.Average(r => Math.Min(r.Cost, 10)), l.Average(r => Math.Min(r.SavedCost, 10)), l.Average(r => r.PySecs)));
            }
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "   all: {0} fits, mean {1:F1} ms per fit (median {2:F1}), {3} needed a second start",
                rows.Count, rows.Average(r => r.Ms), Median(rows.Select(r => r.Ms)), rows.Count(r => r.Starts > 1)));
        }

        static double Median(IEnumerable<double> v)
        {
            var a = v.OrderBy(x => x).ToArray();
            return a.Length == 0 ? 0 : a[a.Length / 2];
        }

        public static string Short(string b) =>
            b.Replace("Toddler-like (Small, head x1.3, legs x0.85)", "Toddler").Replace("Giant (Long x1.6, strength ~ L^3)", "Giant")
                .Replace("Small, arms by thickness", "SmArmThk").Replace("Long, arms x0.65", "LgArm.65")
                .Replace("Long, stiff ankles (dorsiflexion 5)", "LgAnk5").Replace("Round, stiff knees (100)", "RdKnee100")
                .Replace("Heavy trunk (Round, trunk+hips x1.5, S0.7, lean 30)", "HvTrunk").Replace("Long, right leg 0.4", "LgRleg.4");

        static string Trim(string s, int n) => s.Length <= n ? s : s.Substring(0, n);

        static void WriteCsv(List<Row> rows, string path)
        {
            using var w = new StreamWriter(path);
            w.WriteLine("body,stance,verdict,status,cost,saved_cost,dq_max_deg,dq_var,dq_rms_deg,dz_mm,contact_err_mm,saved_contact_err_mm,pen_mm,saved_pen_mm,margin_mm,saved_margin_mm,release_margin_mm,saved_release_margin_mm,ms,starts,iterations,jacobians,python_s");
            foreach (var r in rows)
                w.WriteLine(string.Join(",", new[]
                {
                    Q(r.Body), Q(r.Stance), Q(r.Verdict), Q(r.Status), F(r.Cost), F(r.SavedCost), F(r.DqMax), r.DqVar, F(r.DqRms), F(r.Dz), F(r.ContactErr),
                    F(r.SavedContactErr), F(r.Pen), F(r.SavedPen), F(1000 * r.Margin), F(1000 * r.SavedMargin),
                    r.RelMargin.HasValue ? F(1000 * r.RelMargin.Value) : "", r.SavedRelMargin.HasValue ? F(1000 * r.SavedRelMargin.Value) : "",
                    F(r.Ms), r.Starts.ToString(), r.Iter.ToString(), r.Evals.ToString(), F(r.PySecs)
                }));
            Console.WriteLine("   wrote " + path);
        }

        static string Q(string s) => "\"" + s.Replace("\"", "'") + "\"";
        static string F(double v) => v.ToString("G6", CultureInfo.InvariantCulture);

        // ------------------------------------------------------------------ timing
        public static void Timing(List<string> opts)
        {
            Console.WriteLine("== time: one residual evaluation and one Jacobian, per stance, Small");
            var b = ExactTests.Body("Small");
            var r = new List<D>();
            foreach (var (fam, args) in StanceLibrary.PrototypeBatch.Take(45))
            {
                var st = StanceLibrary.Make(b, fam, args);
                var p = new StanceProblem(b, st);
                var x = p.StartVector();
                for (int i = 0; i < 20; i++) p.Evaluate(x, true, r);
                var sw = Stopwatch.StartNew();
                int n = 200;
                for (int i = 0; i < n; i++) p.Evaluate(x, false, r);
                double tv = sw.Elapsed.TotalMilliseconds * 1000 / n;
                sw.Restart();
                for (int i = 0; i < n; i++) p.Evaluate(x, true, r);
                double tj = sw.Elapsed.TotalMilliseconds * 1000 / n;
                Console.WriteLine($"   {Trim(st.Name, 60),-60} m {r.Count,4}  value {tv,7:F1} us  jacobian {tj,7:F1} us");
            }
        }
    }
}
