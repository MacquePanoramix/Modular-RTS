// Regression checks for what the independent review (REVIEW.md, November 4) found and fixed in src/Reserve:
// a joint of zero capacity (its torque must be zero) made the dual simplex fail or say "not held" where HiGHS holds the
// pose, and non-finite input came back Optimal and held.
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody.Tests
{
    static class ReviewTests
    {
        static StaticsProblem ZeroCap(StaticsProblem p, int j)
        {
            var q = p.Subset(c => true);
            q.Joints[j].Capacity = 0.0;
            return q;
        }

        public static void Run(Check t, List<ReserveCase> cases, bool quick)
        {
            Console.WriteLine("-- review: a joint of zero capacity against the primal path; non-finite input refused");
            var solver = new ReserveSolver();
            var lp = new DenseSimplex();
            int n = 0, held = 0, same = 0;
            double maxD = 0;
            foreach (var c in cases)
            {
                if (double.IsPositiveInfinity(c.PyS)) continue;
                if (quick && c.Index % 4 != 0) continue;
                var res0 = solver.Solve(c.P);
                // the worst joint, and one by the case's index (the neck's torque cannot be taken by any contact, so a
                // weightless neck is a not-held case)
                foreach (int j in new[] { res0.Worst, c.Index % c.P.Joints.Length })
                {
                    var q = ZeroCap(c.P, j);
                    var res = solver.Solve(q);
                    var pr = new PrimalReserve(q);
                    var x = new double[pr.N];
                    double sp = pr.Solve(lp, x, out var st);
                    bool ph = !double.IsPositiveInfinity(sp);
                    n++;
                    t.True(res.Status == LpStatus.Optimal || res.Status == LpStatus.Infeasible,
                        $"{c.Name}, {q.Joints[j].Name} of zero capacity: status {res.Status}");
                    if (res.Held != ph)
                    {
                        t.True(false, $"{c.Name}, {q.Joints[j].Name} of zero capacity: dual {res.Status}, primal {st}");
                        continue;
                    }

                    same++;
                    if (!res.Held) continue;
                    held++;
                    double d = Math.Abs(res.S - sp);
                    maxD = Math.Max(maxD, d);
                    t.True(d <= 1e-7 && res.Residual < 1e-8, $"{c.Name}, {q.Joints[j].Name} of zero capacity: S {res.S:G10} vs primal {sp:G10}, residual {res.Residual:G3}");
                    t.True(res.S >= res0.S - 1e-9, $"{c.Name}, {q.Joints[j].Name} of zero capacity: S {res.S:G10} below the full-strength {res0.S:G10}");
                    t.True(!double.IsPositiveInfinity(res.R) && res.PerJoint[j] == 0.0,
                        $"{c.Name}, {q.Joints[j].Name} of zero capacity: held, so its share is 0 (got {res.PerJoint[j]:G3}) and R finite ({res.R:G3})");
                }
            }

            Console.WriteLine(FormattableString.Invariant(
                $"   {n} problems with one joint of zero capacity: verdict as the primal path in {same}/{n} ({held} held), max |S - S_primal| {maxD:G3}"));

            // non-finite input: refused (it used to come back Optimal, held, with a finite S or NaN)
            var p = cases.Find(k => !double.IsPositiveInfinity(k.PyS)).P;
            int refused = 0;
            var bad = new List<(string what, Func<StaticsProblem> make, Vec3 shift)>
            {
                ("a NaN centre of mass", () => { var q = p.Subset(c => true); q.Com = new Vec3(double.NaN, 0, 0); return q; }, Vec3.Zero),
                ("a NaN contact point", () =>
                {
                    var q = p.Subset(c => true);
                    var c0 = q.Contacts[0];
                    c0.Point = new Vec3(0, double.NaN, 0);
                    q.Contacts[0] = c0;
                    return q;
                }, Vec3.Zero),
                ("a NaN capacity", () => { var q = p.Subset(c => true); q.Joints[3].Capacity = double.NaN; return q; }, Vec3.Zero),
                ("a negative capacity", () => { var q = p.Subset(c => true); q.Joints[3].Capacity = -1.0; return q; }, Vec3.Zero),
                ("an infinite subtree moment", () => { var q = p.Subset(c => true); q.Joints[0].SubMoment = new Vec3(double.PositiveInfinity, 0, 0); return q; }, Vec3.Zero),
                ("a ContactSign too short", () => { var q = p.Subset(c => true); q.Joints[2].ContactSign = new sbyte[0]; return q; }, Vec3.Zero),
                ("a NaN shift", () => p, new Vec3(double.NaN, 0, 0)),
            };
            foreach (var (what, make, shift) in bad)
            {
                bool threw = false;
                try { solver.Solve(make(), shift); }
                catch (ArgumentException) { threw = true; }
                t.True(threw, "the reserve LP refuses " + what);
                if (threw) refused++;
            }

            Console.WriteLine($"   non-finite or malformed input refused (ArgumentException): {refused}/{bad.Count}");
            // and the solver still works afterwards
            var again = solver.Solve(p);
            t.True(again.Held && Math.Abs(again.S - cases.Find(k => k.P == p).PyS) < 1e-7, "the solver after refused input");
        }
    }
}
