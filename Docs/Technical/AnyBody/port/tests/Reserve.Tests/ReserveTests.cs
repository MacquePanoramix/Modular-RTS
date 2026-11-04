// The C# reserve LP against the prototype, at every saved fit of the three miners (reference/reserve_cases.json):
// S (the LP optimum) against b3.reserve_lp's s; r (run3's max per-joint share), the worst joint and the hands' share
// against the saved values; the base margin, the feasible-region depth and the let-go reserve; and the timings.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace WonderGather.AnyBody.Tests
{
    static class ReserveTests
    {
        static string F(double v, int d = 4) => Check.F(v, d);

        public static void Run(Check t, List<ReserveCase> cases, bool quick)
        {
            var solver = new ReserveSolver();
            var lp = new DenseSimplex();

            // ---------------------------------------------------------------- S, and the primal path
            Console.WriteLine("-- reserve LP: S against b3.reserve_lp's s (HiGHS), and against the primal written as b3.py");
            double maxDs = 0, maxDsPrimal = 0, maxResid = 0;
            int nHeld = 0, nNot = 0;
            foreach (var c in cases)
            {
                var res = solver.Solve(c.P);
                bool pyHeld = !double.IsPositiveInfinity(c.PyS);
                t.True(res.Held == pyHeld, $"{c.Name}: held {res.Held} ({res.Status}), prototype {pyHeld}");
                if (res.Held && pyHeld)
                {
                    nHeld++;
                    maxDs = Math.Max(maxDs, Math.Abs(res.S - c.PyS));
                    t.Near(res.S, c.PyS, 1e-7, $"{c.Name}: S");
                    maxResid = Math.Max(maxResid, res.Residual);
                    t.True(res.Residual < 1e-8, $"{c.Name}: residual {res.Residual:G3}");
                }
                else nNot++;

                if (!quick)
                {
                    var pr = new PrimalReserve(c.P);
                    var x = new double[pr.N];
                    double sp = pr.Solve(lp, x, out var st);
                    t.True(double.IsPositiveInfinity(sp) == !res.Held, $"{c.Name}: primal path held {st}");
                    if (res.Held && !double.IsPositiveInfinity(sp))
                    {
                        maxDsPrimal = Math.Max(maxDsPrimal, Math.Abs(sp - res.S));
                        t.Near(sp, res.S, 1e-7, $"{c.Name}: S by the primal path");
                    }
                }
            }

            Console.WriteLine($"   {nHeld} held, {nNot} not held (as the prototype); max |S - s_py| = {maxDs:G3}; " +
                              (quick ? "" : $"max |S_primal - S_dual| = {maxDsPrimal:G3}; ") + $"max residual {maxResid:G3}");

            // ---------------------------------------------------------------- r, worst, hands' share
            Console.WriteLine("-- r = max per-joint share, the worst joint and the hands' share against the saved fits");
            Console.WriteLine("   (res_miners2.json: against its saved values, which the prototype reproduces; res_miners.json: against the");
            Console.WriteLine("    prototype's code now, as that file was written by older code)");
            foreach (var golden in new[] { true, false })
            {
                var set = cases.Where(c => c.Golden == golden).ToList();
                int n = 0, rOk = 0, wOk = 0, wSide = 0, hOk = 0, hIn = 0, hRange = 0, pyfOk = 0, pyfOpt = 0;
                double maxDr = 0, maxDh = 0, maxPyfR = 0, maxPyfOpt = 0;
                var rows = new List<string>();
                foreach (var c in set)
                {
                    double refR = golden ? c.SavedR : c.PyR;
                    string refW = golden ? c.SavedWorst : c.PyWorst;
                    double refH = golden ? c.SavedHands : c.PyHands;
                    var res = solver.Solve(c.P);
                    if (!res.Held || double.IsPositiveInfinity(refR)) continue;
                    n++;
                    double dr = Math.Abs(res.R - refR);
                    maxDr = Math.Max(maxDr, dr);
                    if (dr <= 0.02) rOk++;
                    t.True(dr <= 0.02, $"{c.Name}: r {F(res.R)} vs {F(refR)}");
                    if (res.WorstName == refW) wOk++;
                    else if (Kind(res.WorstName) == Kind(refW)) wSide++;
                    double dh = Math.Abs(res.HandsShare - refH);
                    maxDh = Math.Max(maxDh, dh);
                    if (dh <= 0.02) hOk++;
                    // the prototype's own forces, read by the C# code: same r, and optimal in the C# LP
                    if (c.PyForces != null)
                    {
                        var ev = new ReserveResult { S = res.S };
                        ReserveSolver.Evaluate(c.P, c.PyForces, Vec3.Zero, ev);
                        double e1 = Math.Abs(ev.R - c.PyR);
                        maxPyfR = Math.Max(maxPyfR, e1);
                        if (e1 < 1e-9) pyfOk++;
                        double e2 = Math.Abs(ev.PerJointLp.Max() - res.S);
                        maxPyfOpt = Math.Max(maxPyfOpt, Math.Max(e2, ev.Residual));
                        if (e2 < 1e-6 && ev.Residual < 1e-6) pyfOpt++;
                    }

                    string range = "";
                    if (!quick && (dh > 0.02 || dr > 0.02 || res.WorstName != refW))
                    {
                        var pr = new PrimalReserve(c.P);
                        var (lo, hi) = pr.HandsRange(c.P, lp, res.S * (1 + 1e-9) + 1e-9);
                        bool inside = refH >= lo - 1e-6 && refH <= hi + 1e-6 && res.HandsShare >= lo - 1e-6 && res.HandsShare <= hi + 1e-6;
                        hRange++;
                        if (inside) hIn++;
                        range = $" hands at the optimum {F(lo, 3)}..{F(hi, 3)}{(inside ? "" : " (OUTSIDE)")}";
                    }

                    if (dr > 0.02 || dh > 0.02 || res.WorstName != refW)
                        rows.Add($"   {c.Body,-6} {Trim(c.Stance, 58),-58} S {F(res.S, 3)} r {F(res.R, 3)}/{F(refR, 3)} " +
                                 $"(ipm {F(c.IpmR, 3)}) worst {res.WorstName}/{refW} hands {F(res.HandsShare, 2)}/{F(refH, 2)} " +
                                 $"(ipm {F(c.IpmHands, 2)}){range}");
                }

                string label = golden ? "res_miners2.json (saved)" : "res_miners.json (prototype now)";
                Console.WriteLine($"   {label}: {n} held fits. r within 0.02: {rOk}/{n} (max |dr| {maxDr:F4}); worst joint the same: " +
                                  $"{wOk}/{n} (+{wSide} the same joint on the other side); hands' share within 0.02: {hOk}/{n} (max |dh| {maxDh:F3})");
                Console.WriteLine($"     the prototype's forces read by the C# code: r the same (1e-9) in {pyfOk}/{n} (max {maxPyfR:G2}); " +
                                  $"optimal in the C# LP (1e-6) in {pyfOpt}/{n} (max {maxPyfOpt:G2})");
                if (!quick && hRange > 0)
                    Console.WriteLine($"     where they differ, both hands' shares lie in the range over the optimal force splits: {hIn}/{hRange}");
                foreach (var r in rows) Console.WriteLine(r);
            }

            // ---------------------------------------------------------------- base margin, let-go, feasible region
            Console.WriteLine("-- base margin, let-go, feasible-region depth");
            double maxDm = 0, maxDrel = 0, maxDrelM = 0, maxDfr = 0, maxDpts = 0;
            int nm = 0, nrel = 0, nfr = 0, frBad = 0;
            var frRows = new List<string>();
            var frSolver = new ReserveSolver();
            foreach (var c in cases)
            {
                double refM = c.Golden ? c.SavedMargin : c.PyMargin;
                double m = SupportPolygon.Margin(c.P);
                maxDm = Math.Max(maxDm, Math.Abs(m - refM));
                t.Near(m, refM, 1e-9, $"{c.Name}: base margin");
                nm++;
                if (c.ReleaseKeep != null)
                {
                    var q = c.Released();
                    double rr = new ReserveSolver().Reserve(q, Vec3.Zero);
                    double refRel = c.Golden ? c.SavedReleaseR : c.PyReleaseR;
                    double refRelM = c.Golden ? c.SavedReleaseMargin : c.PyReleaseMargin;
                    bool bothInf = double.IsPositiveInfinity(rr) && double.IsPositiveInfinity(refRel);
                    if (!bothInf) maxDrel = Math.Max(maxDrel, Math.Abs(rr - refRel));
                    t.Near(rr, refRel, 1e-7, $"{c.Name}: let-go r");
                    double mm = SupportPolygon.Margin(q);
                    maxDrelM = Math.Max(maxDrelM, Math.Abs(mm - refRelM));
                    t.Near(mm, refRelM, 1e-9, $"{c.Name}: let-go margin");
                    nrel++;
                }

                if (quick) continue;
                double refFr = c.Golden ? c.SavedFr : c.PyFr;
                if (double.IsPositiveInfinity(c.PyS)) continue; // run3 writes -1 without a region
                var fr = FeasibleRegion.Compute(frSolver, c.P, 1.0, 12);
                nfr++;
                double dfr = Math.Abs(fr.Depth - refFr);
                maxDfr = Math.Max(maxDfr, dfr);
                if (dfr > 1e-6)
                {
                    frBad++;
                    frRows.Add($"   {c.Body,-6} {Trim(c.Stance, 60),-60} depth {fr.Depth:F6} vs {refFr:F6}");
                }

                t.Near(fr.Depth, refFr, 1e-6, $"{c.Name}: feasible-region depth");
                if (c.PyFrPts != null && fr.Points.Length == c.PyFrPts.Length)
                    for (int k = 0; k < fr.Points.Length; k++)
                        maxDpts = Math.Max(maxDpts, Math.Max(Math.Abs(fr.Points[k].x - c.PyFrPts[k].x), Math.Abs(fr.Points[k].y - c.PyFrPts[k].y)));
            }

            Console.WriteLine($"   base margin: {nm} fits, max |d| {maxDm:G3} m;  let-go ({nrel} fits): max |d r| {maxDrel:G3}, " +
                              $"max |d margin| {maxDrelM:G3} m");
            if (!quick)
            {
                Console.WriteLine($"   feasible-region depth (w 1, 12 directions, 11 bisections): {nfr} fits, {nfr - frBad} within 1e-6 m, " +
                                  $"max |d| {maxDfr:G3} m; boundary points max |d| {maxDpts:G3} m");
                foreach (var r in frRows) Console.WriteLine(r);
            }
        }

        /// <summary>The same S under other solver settings: no cost jitter (b3.py's costs exactly), no small-pivot
        /// avoidance, Bland's rule at once or never, and tighter or looser pivot tolerances.</summary>
        public static void Settings(Check t, List<ReserveCase> cases)
        {
            Console.WriteLine("-- the reserve LP under other solver settings (S against the prototype's s, all fits)");
            var variants = new (string name, Action<ReserveSolver> set)[]
            {
                ("default (jitter 1e-4, small pivots avoided)", s => { }),
                ("no cost jitter (b3.py's costs)", s => s.CostJitter = 0),
                ("no small-pivot avoidance", s => s.Simplex.RelativePivotTol = 0),
                ("no jitter, no small-pivot avoidance", s => { s.CostJitter = 0; s.Simplex.RelativePivotTol = 0; }),
                ("Bland's rule at the first degenerate pivot", s => s.Simplex.DegenerateRunToBland = 1),
                ("Bland's rule never", s => s.Simplex.DegenerateRunToBland = int.MaxValue),
                ("pivot tolerance 1e-11", s => s.Simplex.PivotTol = 1e-11),
                ("pivot tolerance 1e-7", s => s.Simplex.PivotTol = 1e-7),
                ("jitter 1e-2", s => s.CostJitter = 1e-2),
            };
            foreach (var (name, set) in variants)
            {
                var solver = new ReserveSolver();
                set(solver);
                int wrong = 0;
                double maxD = 0;
                long piv = 0;
                var sw = Stopwatch.StartNew();
                foreach (var c in cases)
                {
                    solver.Prepare(c.P);
                    var res = solver.Solve(Vec3.Zero);
                    piv += res.Pivots;
                    bool pyHeld = !double.IsPositiveInfinity(c.PyS);
                    if (res.Held != pyHeld) wrong++;
                    else if (res.Held) maxD = Math.Max(maxD, Math.Abs(res.S - c.PyS));
                }

                sw.Stop();
                bool ok = wrong == 0 && maxD < 1e-6;
                if (name.StartsWith("default")) t.True(ok, $"settings '{name}': {wrong} wrong verdicts, max |dS| {maxD:G3}");
                Console.WriteLine($"   {name,-46} wrong held/not held: {wrong}; max |S - s_py| {maxD:G3}; pivots {piv}; " +
                                  $"rejections {solver.Simplex.Rejections}, restarts {solver.Simplex.Fallbacks}; {sw.Elapsed.TotalMilliseconds:F0} ms");
            }
        }

        public static void Timings(List<ReserveCase> cases, int reps)
        {
            Console.WriteLine("-- timings (Release; one thread)");
            var solver = new ReserveSolver();
            // warm the JIT
            foreach (var c in cases.Take(20)) { solver.Solve(c.P); FeasibleRegion.Compute(solver, c.P); }
            var cold = new List<double>();
            var pivots = new List<double>();
            var sw = new Stopwatch();
            foreach (var c in cases)
            {
                sw.Restart();
                for (int r = 0; r < reps; r++) solver.Solve(c.P);
                sw.Stop();
                cold.Add(sw.Elapsed.TotalMilliseconds / reps);
                pivots.Add(solver.Solve(c.P).Pivots);
            }

            var sOnly = new List<double>();
            foreach (var c in cases)
            {
                sw.Restart();
                for (int r = 0; r < reps; r++) solver.Reserve(c.P, Vec3.Zero);
                sw.Stop();
                sOnly.Add(sw.Elapsed.TotalMilliseconds / reps);
            }

            var frMs = new List<double>();
            var frLp = new List<double>();
            var frPerLp = new List<double>();
            var frPiv = new List<double>();
            foreach (var c in cases)
            {
                if (double.IsPositiveInfinity(c.PyS)) continue;
                sw.Restart();
                FeasibleRegionResult fr = null;
                int rr = Math.Max(1, reps / 10);
                for (int r = 0; r < rr; r++) fr = FeasibleRegion.Compute(solver, c.P);
                sw.Stop();
                double ms = sw.Elapsed.TotalMilliseconds / rr;
                frMs.Add(ms);
                frLp.Add(fr.LpCount);
                frPerLp.Add(ms / fr.LpCount);
                frPiv.Add((double)fr.Pivots / fr.LpCount);
            }

            var py = cases.Where(c => !double.IsNaN(c.PyLpMs)).Select(c => c.PyLpMs).ToList();
            var pyFr = cases.Where(c => !double.IsNaN(c.PyFrMs)).Select(c => c.PyFrMs).ToList();
            Console.WriteLine($"   one LP from scratch, full answer (Solve): median {Med(cold):F3} ms, mean {cold.Average():F3}, max {cold.Max():F3}; " +
                              $"pivots median {Med(pivots):F0}, max {pivots.Max():F0}");
            Console.WriteLine($"   one LP from scratch, S only (Reserve): median {Med(sOnly):F3} ms, max {sOnly.Max():F3}");
            Console.WriteLine($"   feasible region (12 directions): median {Med(frMs):F2} ms, max {frMs.Max():F2}; {Med(frLp):F0} LPs each, " +
                              $"warm-started: median {Med(frPerLp) * 1000:F1} us and {Med(frPiv):F1} pivots per LP");
            Console.WriteLine($"   the prototype (Python + HiGHS, same machine): one LP median {Med(py):F2} ms; feasible region median {Med(pyFr):F0} ms");
        }

        /// <summary>One case in detail (cases whose name contains the text).</summary>
        public static void Debug(List<ReserveCase> cases, string text)
        {
            foreach (var c in cases.Where(c => c.Name.Contains(text)))
            {
                Console.WriteLine($"== {c.File} #{c.Index} {c.Name}: nc {c.P.Contacts.Length}, py s {F(c.PyS, 6)} r {F(c.PyR, 6)} worst {c.PyWorst}");
                var solver = new ReserveSolver();
                if (Environment.GetEnvironmentVariable("LP_TRACE") == "1") solver.Simplex.Trace = s => Console.WriteLine("      " + s);
                var res = solver.Solve(c.P);
                solver.Simplex.Trace = null;
                Console.WriteLine($"   dual path: {res.Status}, S {F(res.S, 8)}, R {F(res.R, 6)}, pivots {res.Pivots}, residual {res.Residual:G3}");
                var pr = new PrimalReserve(c.P);
                var lp = new DenseSimplex();
                var x = new double[pr.N];
                double sp = pr.Solve(lp, x, out var st);
                Console.WriteLine($"   primal path: {st}, S {F(sp, 8)}, pivots {lp.Iterations}");
                foreach (var tol in new[] { 1e-7, 1e-8, 1e-10, 1e-11, 1e-12 })
                {
                    var s2 = new ReserveSolver();
                    s2.Simplex.PivotTol = tol;
                    var r2 = s2.Solve(c.P);
                    Console.WriteLine($"   dual path, pivot tol {tol:G2}: {r2.Status}, S {F(r2.S, 8)}, pivots {r2.Pivots}");
                }

                var s3 = new ReserveSolver();
                s3.Simplex.DegenerateRunToBland = 1000000;
                var r3 = s3.Solve(c.P);
                Console.WriteLine($"   dual path, no Bland: {r3.Status}, S {F(r3.S, 8)}, pivots {r3.Pivots}");
                for (int i = 0; i < c.P.Contacts.Length; i++)
                {
                    var k = c.P.Contacts[i];
                    Console.WriteLine($"   contact {i}: {k.Kind} link {k.Link} other {k.Other} p {k.Point} n {k.Normal} mu {k.Mu}" +
                                      (c.PyForces != null ? $" f_py {c.PyForces[i]}" : ""));
                }
            }
        }

        static double Med(List<double> v)
        {
            var a = v.OrderBy(x => x).ToList();
            return a.Count == 0 ? double.NaN : a.Count % 2 == 1 ? a[a.Count / 2] : 0.5 * (a[a.Count / 2 - 1] + a[a.Count / 2]);
        }

        static string Kind(string joint) => joint == null ? null : joint.TrimEnd('L', 'R');

        static string Trim(string s, int n) => s.Length <= n ? s : s.Substring(0, n);
    }
}
