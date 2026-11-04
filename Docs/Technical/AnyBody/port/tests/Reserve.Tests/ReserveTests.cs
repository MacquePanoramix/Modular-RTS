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

        public static void Run(Check t, List<ReserveCase> cases, bool quick, bool strictR = false)
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
                int nIpm = 0, ipmROk = 0, ipmW = 0, ipmH = 0;
                double maxDr = 0, maxDh = 0, maxPyfR = 0, maxPyfOpt = 0, maxIpmDr = 0;
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
                    if (res.WorstName == refW) wOk++;
                    else if (Kind(res.WorstName) == Kind(refW)) wSide++;
                    double dh = Math.Abs(res.HandsShare - refH);
                    maxDh = Math.Max(maxDh, dh);
                    if (dh <= 0.02) hOk++;
                    // the prototype's own forces, read by the C# code: same r, and optimal in the C# LP
                    bool sameLp = false;
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
                        sameLp = e1 < 1e-9 && e2 < 1e-6 && ev.Residual < 1e-6 && Math.Abs(c.PyR - refR) < 1e-9;
                    }

                    // r is vertex-dependent (INTERFACE.md): beyond 0.02 it passes only when the prototype's split is
                    // proven to be another optimal split of the same LP (--strict-r: the plain 0.02 gate)
                    t.True(dr <= 0.02 || (!strictR && sameLp), $"{c.Name}: r {F(res.R)} vs {F(refR)}" + (sameLp ? " (another optimal split)" : ""));
                    // HiGHS's own interior point against the same reference: the spread the prototype itself has
                    if (!double.IsPositiveInfinity(c.IpmR))
                    {
                        nIpm++;
                        double di = Math.Abs(c.IpmR - refR);
                        maxIpmDr = Math.Max(maxIpmDr, di);
                        if (di <= 0.02) ipmROk++;
                        if (c.IpmWorst == refW) ipmW++;
                        if (Math.Abs(c.IpmHands - refH) <= 0.02) ipmH++;
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
                Console.WriteLine($"     HiGHS's interior point against the same reference (the prototype's own spread): r within 0.02 " +
                                  $"{ipmROk}/{nIpm} (max |dr| {maxIpmDr:F4}); worst joint the same {ipmW}/{nIpm}; hands' share within 0.02 {ipmH}/{nIpm}");
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

        /// <summary>Warm starts (one Prepare, then a run of shifts of the body's mass, as the feasible region does)
        /// against cold solves of the same shifts by a fresh Prepare: the same verdict, the same S, a checked answer.</summary>
        public static void WarmStart(Check t, List<ReserveCase> cases, bool quick)
        {
            Console.WriteLine("-- warm starts (one Prepare, then 12 shifts of up to 0.5 m) against cold solves");
            var warm = new ReserveSolver();
            var cold = new ReserveSolver();
            var rng = new Random(7);
            int n = 0, wrong = 0, used = 0, held = 0;
            double maxD = 0, maxRes = 0;
            long warmPiv = 0, coldPiv = 0;
            var sw = new Stopwatch();
            var sc = new Stopwatch();
            foreach (var c in cases)
            {
                if (double.IsPositiveInfinity(c.PyS)) continue;
                if (quick && c.Index % 4 != 0) continue;
                warm.Prepare(c.P);
                for (int k = 0; k < 12; k++)
                {
                    double ang = rng.NextDouble() * 2 * Math.PI, rad = 0.5 * rng.NextDouble();
                    var shift = new Vec3(rad * Math.Cos(ang), rad * Math.Sin(ang), 0.0);
                    sw.Start();
                    var rw = warm.Solve(shift);
                    sw.Stop();
                    sc.Start();
                    var rc = cold.Solve(c.P, shift);
                    sc.Stop();
                    n++;
                    if (rw.WarmStarted) used++;
                    warmPiv += rw.Pivots;
                    coldPiv += rc.Pivots;
                    if (rw.Held != rc.Held)
                    {
                        wrong++;
                        t.True(false, $"{c.Name}: warm {rw.Status}, cold {rc.Status} at shift {shift}");
                        continue;
                    }

                    if (!rw.Held) continue;
                    held++;
                    double d = Math.Abs(rw.S - rc.S);
                    maxD = Math.Max(maxD, d);
                    maxRes = Math.Max(maxRes, rw.Residual);
                    t.True(d <= 1e-9 && rw.Residual < 1e-8, $"{c.Name}: warm S {rw.S:G10} vs cold {rc.S:G10} (residual {rw.Residual:G3}) at shift {shift}");
                }
            }

            Console.WriteLine($"   {n} shifts ({held} held, {n - held} not), warm start used in {used}; verdict the same in {n - wrong}/{n}; " +
                              $"max |S_warm - S_cold| {maxD:G3}, max residual {maxRes:G3}");
            Console.WriteLine($"   warm: {(double)warmPiv / n:F1} pivots and {sw.Elapsed.TotalMilliseconds * 1000 / n:F0} us per LP; " +
                              $"cold: {(double)coldPiv / n:F1} pivots and {sc.Elapsed.TotalMilliseconds * 1000 / n:F0} us per LP (with Prepare)");
        }

        /// <summary>Changed problems, each checked by the primal path (b3.py's own form, phase 1 + 2, another code path):
        /// the body's mass shifted (as a new StaticsProblem, not through the solver's shift), one support dropped, and
        /// every capacity scaled by k (then S must be exactly S / k).</summary>
        public static void Perturbed(Check t, List<ReserveCase> cases, bool quick)
        {
            Console.WriteLine("-- changed problems (mass shifted, a support dropped, strength scaled) against the primal path");
            var solver = new ReserveSolver();
            var lp = new DenseSimplex();
            var rng = new Random(11);
            int n = 0, wrong = 0, held = 0, nScale = 0;
            double maxD = 0, maxScale = 0;
            foreach (var c in cases)
            {
                if (double.IsPositiveInfinity(c.PyS)) continue;
                if (quick && c.Index % 4 != 0) continue;
                for (int v = 0; v < 2; v++)
                {
                    StaticsProblem q;
                    string what;
                    if (v == 0)
                    {
                        double ang = rng.NextDouble() * 2 * Math.PI, rad = 0.3 * rng.NextDouble();
                        var sh = new Vec3(rad * Math.Cos(ang), rad * Math.Sin(ang), 0.0);
                        q = Shifted(c.P, sh);
                        what = "shifted " + sh;
                    }
                    else
                    {
                        int ns = c.P.Contacts.Max(k => k.Support) + 1;
                        int drop = rng.Next(ns);
                        q = c.P.Subset(k => k.Support != drop);
                        if (q.Contacts.Length == 0) continue;
                        what = "support " + drop + " dropped";
                    }

                    var res = solver.Solve(q);
                    var pr = new PrimalReserve(q);
                    var x = new double[pr.N];
                    double sp = pr.Solve(lp, x, out var st);
                    n++;
                    bool ph = !double.IsPositiveInfinity(sp);
                    if (res.Held != ph)
                    {
                        wrong++;
                        t.True(false, $"{c.Name}, {what}: dual {res.Status}, primal {st}");
                        continue;
                    }

                    if (!res.Held) continue;
                    held++;
                    double d = Math.Abs(res.S - sp);
                    maxD = Math.Max(maxD, d);
                    t.True(d <= 1e-7 && res.Residual < 1e-8, $"{c.Name}, {what}: S {res.S:G10} vs primal {sp:G10}");
                }

                // strength scaled: S(k cap) = S / k
                double kk = 0.3 + 2.7 * rng.NextDouble();
                var s0 = solver.Solve(c.P);
                var sc = solver.Solve(Scaled(c.P, kk));
                if (s0.Held && sc.Held)
                {
                    nScale++;
                    double e = Math.Abs(sc.S * kk - s0.S) / Math.Max(1e-12, s0.S);
                    maxScale = Math.Max(maxScale, e);
                    t.True(e <= 1e-7, $"{c.Name}: S at strength x{kk:F2} = {sc.S:G10}, S/k {s0.S / kk:G10}");
                }
                else t.True(s0.Held == sc.Held, $"{c.Name}: held changed with strength x{kk:F2}");
            }

            Console.WriteLine($"   {n} changed problems ({held} held, {n - held} not): verdict as the primal path in {n - wrong}/{n}, " +
                              $"max |S - S_primal| {maxD:G3}; strength scaled ({nScale}): max relative |k S(k) - S| {maxScale:G3}");
        }

        /// <summary>An option, not the prototype's measure: ball joints checked in 200 directions on the half sphere.
        /// Certified: S_fine is at most the full-norm reserve R_true (the least worst |tau|/cap over all force splits),
        /// which is at most the full norm at any split: at its own (R_fine), the C# 13-direction one, the prototype's.</summary>
        public static void FullNorm(Check t, List<ReserveCase> cases, bool quick)
        {
            Console.WriteLine("-- option: the full-norm reserve (ball joints in 200 directions) against S and the prototype's r");
            var fine = new ReserveSolver { Directions = ReserveSolver.HalfSphereDirections(200) };
            var std = new ReserveSolver();
            int n = 0, within = 0;
            double maxWidth = 0, maxDev = 0, sumUp = 0, maxUp = 0, sumUnder = 0, maxUnder = 0;
            long piv = 0;
            var sw = new Stopwatch();
            foreach (var c in cases)
            {
                if (double.IsPositiveInfinity(c.PyS)) continue;
                if (quick && c.Index % 4 != 0) continue;
                double refR = c.Golden ? c.SavedR : c.PyR;
                var r13 = std.Solve(c.P);
                sw.Start();
                var rf = fine.Solve(c.P);
                sw.Stop();
                t.True(rf.Held, $"{c.Name}: held with 200 directions ({rf.Status})");
                if (!rf.Held) continue;
                n++;
                piv += rf.Pivots;
                const double tol = 1e-7;
                t.True(rf.S <= rf.R + tol && rf.S <= r13.R + tol && rf.S <= refR + tol && r13.S <= rf.R + tol && rf.Residual < 1e-8,
                    $"{c.Name}: full-norm bracket S13 {r13.S:F5} S_fine {rf.S:F5} R_fine {rf.R:F5} R13 {r13.R:F5} r_py {refR:F5}");
                maxWidth = Math.Max(maxWidth, rf.R - rf.S);
                double dev = Math.Abs(rf.S - refR);
                maxDev = Math.Max(maxDev, dev);
                if (dev <= 0.02) within++;
                sumUp += refR - rf.S;
                maxUp = Math.Max(maxUp, refR - rf.S);
                sumUnder += rf.S - r13.S;
                maxUnder = Math.Max(maxUnder, rf.S - r13.S);
            }

            Console.WriteLine($"   {n} fits: R_true lies in [S_fine, R_fine], width at most {maxWidth:F4}; S_fine within 0.02 of the prototype's r " +
                              $"{within}/{n} (max {maxDev:F4}); the prototype's r above it by mean {sumUp / Math.Max(1, n):F4}, max {maxUp:F4}; " +
                              $"the 13 directions' S below it by mean {sumUnder / Math.Max(1, n):F4}, max {maxUnder:F4}");
            Console.WriteLine($"   200 directions: {(double)piv / Math.Max(1, n):F0} pivots and {sw.Elapsed.TotalMilliseconds / Math.Max(1, n):F2} ms per LP");
        }

        static StaticsProblem Shifted(StaticsProblem p, Vec3 sh)
        {
            var q = p.Subset(k => true);
            q.Com = p.Com + sh;
            foreach (var j in q.Joints) j.SubMoment = j.SubMoment + j.SubMass * sh;
            return q;
        }

        static StaticsProblem Scaled(StaticsProblem p, double k)
        {
            var q = p.Subset(c => true);
            foreach (var j in q.Joints) j.Capacity *= k;
            return q;
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
                int wrong = 0, nr = 0, rOk = 0;
                double maxD = 0, maxDr = 0;
                long piv = 0;
                var sw = Stopwatch.StartNew();
                foreach (var c in cases)
                {
                    solver.Prepare(c.P);
                    var res = solver.Solve(Vec3.Zero);
                    piv += res.Pivots;
                    bool pyHeld = !double.IsPositiveInfinity(c.PyS);
                    if (res.Held != pyHeld) wrong++;
                    else if (res.Held)
                    {
                        maxD = Math.Max(maxD, Math.Abs(res.S - c.PyS));
                        // r (vertex-dependent) against the same reference as Run: saved for res_miners2, the code now otherwise
                        double refR = c.Golden ? c.SavedR : c.PyR;
                        if (!double.IsPositiveInfinity(refR))
                        {
                            nr++;
                            double dr = Math.Abs(res.R - refR);
                            maxDr = Math.Max(maxDr, dr);
                            if (dr <= 0.02) rOk++;
                        }
                    }
                }

                sw.Stop();
                bool ok = wrong == 0 && maxD < 1e-6;
                if (name.StartsWith("default")) t.True(ok, $"settings '{name}': {wrong} wrong verdicts, max |dS| {maxD:G3}");
                Console.WriteLine($"   {name,-46} wrong held/not held: {wrong}; max |S - s_py| {maxD:G3}; r within 0.02 {rOk}/{nr} " +
                                  $"(max {maxDr:F3}); pivots {piv}; rejections {solver.Simplex.Rejections}, restarts {solver.Simplex.Fallbacks}; " +
                                  $"{sw.Elapsed.TotalMilliseconds:F0} ms");
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
