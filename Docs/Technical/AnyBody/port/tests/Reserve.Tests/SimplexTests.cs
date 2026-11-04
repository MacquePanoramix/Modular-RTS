// DenseSimplex on small hand-made LPs (textbook, bounds, free variables, redundancy, cycling, Klee-Minty, infeasible,
// unbounded) and on random LPs with scipy/HiGHS's answers (reference/lp_random.json).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace WonderGather.AnyBody.Tests
{
    static class SimplexTests
    {
        const double Inf = double.PositiveInfinity;

        static LpStatus Run(DenseSimplex lp, double[] c, double[,] aeq, double[] beq, double[,] aub, double[] bub, double[] lo,
            double[] hi, double[] x, out double fun)
        {
            int n = c.Length;
            int meq = aeq?.GetLength(0) ?? 0, mub = aub?.GetLength(0) ?? 0;
            var ae = new double[meq * n];
            var au = new double[mub * n];
            for (int i = 0; i < meq; i++)
                for (int j = 0; j < n; j++) ae[i * n + j] = aeq[i, j];
            for (int i = 0; i < mub; i++)
                for (int j = 0; j < n; j++) au[i * n + j] = aub[i, j];
            return LinProg.Solve(lp, n, c, meq, ae, beq, mub, au, bub, lo, hi, x, out fun);
        }

        public static void Small(Check t)
        {
            Console.WriteLine("-- DenseSimplex: small LPs");
            var lp = new DenseSimplex();
            var x = new double[16];
            double fun;

            // textbook: min -x - y, x + 2y <= 4, 3x + y <= 6 -> (1.6, 1.2), -2.8
            var st = Run(lp, new[] { -1.0, -1 }, null, null, new double[,] { { 1, 2 }, { 3, 1 } }, new[] { 4.0, 6 }, null, null, x, out fun);
            t.True(st == LpStatus.Optimal, "textbook: optimal");
            t.Near(fun, -2.8, 1e-12, "textbook: objective");
            t.Near(x[0], 1.6, 1e-12, "textbook: x");
            t.Near(x[1], 1.2, 1e-12, "textbook: y");

            // upper bounds (bound flips): min -x - y, x + y <= 10, x <= 3, y <= 4 -> -7
            st = Run(lp, new[] { -1.0, -1 }, null, null, new double[,] { { 1, 1 } }, new[] { 10.0 }, new[] { 0.0, 0 }, new[] { 3.0, 4 }, x, out fun);
            t.True(st == LpStatus.Optimal, "bounded: optimal");
            t.Near(fun, -7, 1e-12, "bounded: objective");

            // a free variable: min x, -x <= 3 -> x = -3
            st = Run(lp, new[] { 1.0 }, null, null, new double[,] { { -1 } }, new[] { 3.0 }, new[] { -Inf }, new[] { Inf }, x, out fun);
            t.True(st == LpStatus.Optimal, "free: optimal");
            t.Near(x[0], -3, 1e-12, "free: x");

            // free variables in equalities: min |x| + |y| style: x - y free, u, v >= 0: x = u - v ... min u + v, x = u - v,
            // x + z = 2, z = 5 -> x = -3, u + v = 3
            st = Run(lp, new[] { 0.0, 0, 1, 1 }, new double[,] { { 1, 0, -1, 1 }, { 1, 1, 0, 0 }, { 0, 1, 0, 0 } }, new[] { 0.0, 2, 5 }, null, null,
                new[] { -Inf, -Inf, 0, 0 }, new[] { Inf, Inf, Inf, Inf }, x, out fun);
            t.True(st == LpStatus.Optimal, "free in equalities: optimal");
            t.Near(fun, 3, 1e-12, "free in equalities: objective");
            t.Near(x[0], -3, 1e-12, "free in equalities: x");

            // redundant equalities: x + y = 1, 2x + 2y = 2, min x -> (0, 1)
            st = Run(lp, new[] { 1.0, 0 }, new double[,] { { 1, 1 }, { 2, 2 } }, new[] { 1.0, 2 }, null, null, null, null, x, out fun);
            t.True(st == LpStatus.Optimal, "redundant: optimal");
            t.Near(x[0], 0, 1e-12, "redundant: x");
            t.Near(x[1], 1, 1e-12, "redundant: y");

            // infeasible: x + y = 1, x + y = 2
            st = Run(lp, new[] { 1.0, 1 }, new double[,] { { 1, 1 }, { 1, 1 } }, new[] { 1.0, 2 }, null, null, null, null, x, out fun);
            t.True(st == LpStatus.Infeasible, "infeasible equalities: " + st);
            // infeasible by bounds: x + y >= 5 with x, y in [0, 2]
            st = Run(lp, new[] { 1.0, 1 }, null, null, new double[,] { { -1, -1 } }, new[] { -5.0 }, new[] { 0.0, 0 }, new[] { 2.0, 2 }, x, out fun);
            t.True(st == LpStatus.Infeasible, "infeasible bounds: " + st);

            // unbounded: min -x, x - y <= 1
            st = Run(lp, new[] { -1.0, 0 }, null, null, new double[,] { { 1, -1 } }, new[] { 1.0 }, null, null, x, out fun);
            t.True(st == LpStatus.Unbounded, "unbounded: " + st);
            // unbounded free variable without rows
            st = Run(lp, new[] { 1.0 }, null, null, null, null, new[] { -Inf }, new[] { Inf }, x, out fun);
            t.True(st == LpStatus.Unbounded, "unbounded free, no rows: " + st);
            // no rows, a box: min x - y, x, y in [-1, 2] -> -3
            st = Run(lp, new[] { 1.0, -1 }, null, null, null, null, new[] { -1.0, -1 }, new[] { 2.0, 2 }, x, out fun);
            t.True(st == LpStatus.Optimal, "box only: optimal");
            t.Near(fun, -3, 1e-12, "box only: objective");

            // Beale's example, which cycles under the textbook rule: optimum -1/20 (with x3 = 1)
            st = Run(lp, new[] { -0.75, 150, -0.02, 6 }, null, null,
                new double[,] { { 0.25, -60, -0.04, 9 }, { 0.5, -90, -0.02, 3 }, { 0, 0, 1, 0 } }, new[] { 0.0, 0, 1 }, null, null, x, out fun);
            t.True(st == LpStatus.Optimal, "Beale: optimal");
            t.Near(fun, -0.05, 1e-12, "Beale: objective");

            // all-degenerate: every right-hand side zero, a box only from the cost's sign
            st = Run(lp, new[] { -1.0, -1, -1 }, null, null, new double[,] { { 1, -1, 0 }, { 0, 1, -1 }, { -1, 0, 1 }, { 1, 1, 1 } },
                new[] { 0.0, 0, 0, 3 }, null, null, x, out fun);
            t.True(st == LpStatus.Optimal, "degenerate cycle: optimal");
            t.Near(fun, -3, 1e-12, "degenerate cycle: objective");

            // Klee-Minty, n = 8 (2^8 - 1 vertices under Dantzig's rule): max sum 2^(n-j) x_j -> 5^n
            int km = 8;
            var ckm = new double[km];
            var akm = new double[km, km];
            var bkm = new double[km];
            for (int i = 0; i < km; i++)
            {
                ckm[i] = -Math.Pow(2, km - 1 - i);
                for (int j = 0; j < i; j++) akm[i, j] = Math.Pow(2, i - j + 1);
                akm[i, i] = 1;
                bkm[i] = Math.Pow(5, i + 1);
            }

            st = Run(lp, ckm, null, null, akm, bkm, null, null, x, out fun);
            t.True(st == LpStatus.Optimal, "Klee-Minty 8: optimal");
            t.Near(fun, -Math.Pow(5, km), 1e-6, "Klee-Minty 8: objective");
            Console.WriteLine(FormattableString.Invariant($"   Klee-Minty 8: {lp.Iterations} pivots"));
        }

        static double[] Arr(JsonElement e)
        {
            var a = new double[e.GetArrayLength()];
            int i = 0;
            foreach (var v in e.EnumerateArray()) a[i++] = v.ValueKind == JsonValueKind.Null ? double.NaN : v.GetDouble();
            return a;
        }

        static double[] Mat(JsonElement e, int n, out int rows)
        {
            rows = e.GetArrayLength();
            var a = new double[rows * n];
            int i = 0;
            foreach (var r in e.EnumerateArray())
            {
                int j = 0;
                foreach (var v in r.EnumerateArray()) a[i * n + j++] = v.GetDouble();
                i++;
            }

            return a;
        }

        public static void Random(Check t, string path)
        {
            Console.WriteLine("-- DenseSimplex: random LPs against scipy/HiGHS (" + Path.GetFileName(path) + ")");
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var lp = new DenseSimplex();
            var byKind = new SortedDictionary<string, int[]>(); // kind -> [cases, status agree, objective agree]
            double worstObj = 0, worstFeas = 0;
            long pivots = 0;
            var sw = new Stopwatch();
            int count = 0;
            foreach (var c in doc.RootElement.GetProperty("cases").EnumerateArray())
            {
                string kind = c.GetProperty("kind").GetString();
                var cc = Arr(c.GetProperty("c"));
                int n = cc.Length;
                var aeq = Mat(c.GetProperty("Aeq"), n, out int meq);
                var beq = Arr(c.GetProperty("beq"));
                var aub = Mat(c.GetProperty("Aub"), n, out int mub);
                var bub = Arr(c.GetProperty("bub"));
                var lo = Arr(c.GetProperty("lo"));
                var hi = Arr(c.GetProperty("hi"));
                for (int j = 0; j < n; j++)
                {
                    if (double.IsNaN(lo[j])) lo[j] = double.NegativeInfinity;
                    if (double.IsNaN(hi[j])) hi[j] = double.PositiveInfinity;
                }

                int want = c.GetProperty("status").GetInt32();
                var x = new double[n];
                sw.Start();
                var st = LinProg.Solve(lp, n, cc, meq, aeq, beq, mub, aub, bub, lo, hi, x, out double fun);
                sw.Stop();
                pivots += lp.Iterations;
                count++;
                if (!byKind.TryGetValue(kind, out var k)) byKind[kind] = k = new int[3];
                k[0]++;
                var wantSt = want == 0 ? LpStatus.Optimal : want == 2 ? LpStatus.Infeasible : want == 3 ? LpStatus.Unbounded : LpStatus.Numerical;
                bool stOk = st == wantSt;
                if (stOk) k[1]++;
                t.True(stOk, FormattableString.Invariant($"random {kind} #{count}: status {st}, scipy {wantSt}"));
                if (st == LpStatus.Optimal && want == 0)
                {
                    double wf = c.GetProperty("fun").GetDouble();
                    double err = Math.Abs(fun - wf) / Math.Max(1.0, Math.Abs(wf));
                    worstObj = Math.Max(worstObj, err);
                    if (err <= 1e-7) k[2]++;
                    t.True(err <= 1e-7, FormattableString.Invariant($"random {kind} #{count}: objective {fun:G12} vs {wf:G12}"));
                    // feasibility of our x in the original rows and bounds
                    double feas = 0;
                    for (int i = 0; i < meq; i++)
                    {
                        double r = -beq[i];
                        for (int j = 0; j < n; j++) r += aeq[i * n + j] * x[j];
                        feas = Math.Max(feas, Math.Abs(r) / Math.Max(1.0, Math.Abs(beq[i])));
                    }

                    for (int i = 0; i < mub; i++)
                    {
                        double r = -bub[i];
                        for (int j = 0; j < n; j++) r += aub[i * n + j] * x[j];
                        feas = Math.Max(feas, Math.Max(0, r) / Math.Max(1.0, Math.Abs(bub[i])));
                    }

                    for (int j = 0; j < n; j++) feas = Math.Max(feas, Math.Max(lo[j] - x[j], x[j] - hi[j]));
                    worstFeas = Math.Max(worstFeas, feas);
                    t.True(feas <= 1e-8, FormattableString.Invariant($"random {kind} #{count}: infeasible by {feas:G3}"));
                }
            }

            foreach (var kv in byKind)
                Console.WriteLine(FormattableString.Invariant($"   {kv.Key,-15} {kv.Value[0],4} LPs: status as scipy {kv.Value[1]}, objective within 1e-7 {kv.Value[2]}"));
            Console.WriteLine(FormattableString.Invariant(
                $"   worst relative objective error {worstObj:G3}, worst infeasibility {worstFeas:G3}; {pivots} pivots; {sw.Elapsed.TotalMilliseconds / count:F3} ms per LP (primal, phase 1 + 2)"));
        }
    }
}
