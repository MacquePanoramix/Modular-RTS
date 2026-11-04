// relax: the free limbs' pull toward the relaxed pose (FitOptions.RelaxFreeLimbs, INTEGRATION.md step 9) against the fits
// without it, on the ten test bodies x the prototype's 45 stances: what it costs the rest of the fit, what it changes in
// the reserve, and what it does to the free limbs. The design's gate: the median cost without the pull rises by under
// 0.01, and the reserve S by under 0.01.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WonderGather.AnyBody.Tests
{
    public static class RelaxTests
    {
        public static void Run(List<string> opts)
        {
            var weightOpt = opts.FirstOrDefault(o => o.StartsWith("--relax="));
            double weight = weightOpt != null ? double.Parse(weightOpt.Substring(8), CultureInfo.InvariantCulture) : new FitOptions().RelaxWeight;
            var bodyOpt = opts.FirstOrDefault(o => o.StartsWith("--bodies="));
            var bodies = bodyOpt != null ? bodyOpt.Substring(9).Split('|').ToList() : FitTests.FitBodies.ToList();
            Console.WriteLine($"== relax: the free limbs pulled toward the relaxed pose (weight {weight} per radian), {bodies.Count} bodies x 45 stances");
            var solver = new ReserveSolver();
            var dCost = new List<double>();
            var dS = new List<double>();
            int notHeldBefore = 0, notHeldAfter = 0, n = 0;
            var lines = new List<string>();
            foreach (var bn in bodies)
            {
                var b = ExactTests.Body(bn);
                foreach (var (family, args) in StanceLibrary.PrototypeBatch)
                {
                    var st = StanceLibrary.Make(b, family, args);
                    var plain = StanceFitter.Fit(b, st);
                    var relaxed = StanceFitter.Fit(b, st, new FitOptions { RelaxFreeLimbs = true, RelaxWeight = weight });
                    double costWithout = CostAt(b, st, relaxed.Q);
                    var s0 = solver.Solve(Statics.Build(b, plain.Q, st.Supports));
                    var s1 = solver.Solve(Statics.Build(b, relaxed.Q, st.Supports));
                    n++;
                    if (!s0.Held) notHeldBefore++;
                    if (!s1.Held) notHeldAfter++;
                    dCost.Add(costWithout - plain.Cost);
                    if (s0.Held && s1.Held) dS.Add(s1.S - s0.S);
                    if (opts.Contains("--arms"))
                        lines.Add(string.Format(CultureInfo.InvariantCulture,
                            "   {0,-6} {1,-44} flex {2,4:F0} {3,4:F0} -> {4,4:F0} {5,4:F0}  abd {6,4:F0} {7,4:F0} -> {8,4:F0} {9,4:F0}  rot {10,4:F0} {11,4:F0} -> {12,4:F0} {13,4:F0}  elb {14,4:F0} {15,4:F0} -> {16,4:F0} {17,4:F0}  trunk {18,4:F0} -> {19,4:F0}",
                            bn, st.Name.Length > 44 ? st.Name.Substring(0, 44) : st.Name,
                            Deg(plain.Q[Pose.ShFlexL]), Deg(plain.Q[Pose.ShFlexR]), Deg(relaxed.Q[Pose.ShFlexL]), Deg(relaxed.Q[Pose.ShFlexR]),
                            Deg(plain.Q[Pose.ShAbdL]), Deg(plain.Q[Pose.ShAbdR]), Deg(relaxed.Q[Pose.ShAbdL]), Deg(relaxed.Q[Pose.ShAbdR]),
                            Deg(plain.Q[Pose.ShRotL]), Deg(plain.Q[Pose.ShRotR]), Deg(relaxed.Q[Pose.ShRotL]), Deg(relaxed.Q[Pose.ShRotR]),
                            Deg(plain.Q[Pose.ElbL]), Deg(plain.Q[Pose.ElbR]), Deg(relaxed.Q[Pose.ElbL]), Deg(relaxed.Q[Pose.ElbR]),
                            Deg(plain.Q[Pose.TFlex]), Deg(relaxed.Q[Pose.TFlex])));
                    else if (bn == "Long" || bn == "Small")
                        lines.Add(string.Format(CultureInfo.InvariantCulture,
                            "   {0,-6} {1,-58} cost {2,6:F3} -> {3,6:F3}  S {4,5} -> {5,5}  elbows {6,4:F0} {7,4:F0} -> {8,4:F0} {9,4:F0}  shoulders {10,4:F0} {11,4:F0} -> {12,4:F0} {13,4:F0}",
                            bn, st.Name.Length > 58 ? st.Name.Substring(0, 58) : st.Name, plain.Cost, costWithout,
                            s0.Held ? s0.S.ToString("F3", CultureInfo.InvariantCulture) : "inf", s1.Held ? s1.S.ToString("F3", CultureInfo.InvariantCulture) : "inf",
                            Deg(plain.Q[Pose.ElbL]), Deg(plain.Q[Pose.ElbR]), Deg(relaxed.Q[Pose.ElbL]), Deg(relaxed.Q[Pose.ElbR]),
                            Deg(plain.Q[Pose.ShFlexL]), Deg(plain.Q[Pose.ShFlexR]), Deg(relaxed.Q[Pose.ShFlexL]), Deg(relaxed.Q[Pose.ShFlexR])));
                }
            }
            foreach (var l in lines) Console.WriteLine(l);
            double medCost = Median(dCost), medS = Median(dS);
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "   {0} fits: the cost without the pull rises by a median {1:F4} (over 0.01 in {2}, the most {3:F3}); S by a median {4:F4} (over 0.01 in {5} of {6} held both ways, the most {7:F3}); not held {8} -> {9}",
                n, medCost, dCost.Count(d => d > .01), dCost.Max(), medS, dS.Count(d => Math.Abs(d) > .01), dS.Count, dS.Count > 0 ? dS.Max(d => Math.Abs(d)) : 0, notHeldBefore, notHeldAfter));
            Check.That(medCost < .01, $"the median cost without the pull rises by under 0.01 ({medCost:F4})");
            Check.That(Math.Abs(medS) < .01, $"the median reserve S moves by under 0.01 ({medS:F4})");
        }

        public static double CostAt(BodyModel b, Stance st, double[] q)
        {
            var p = new StanceProblem(b, st);
            var r = new List<D>();
            p.Evaluate(p.FreeOf(q), false, r);
            return StanceProblem.Cost(r);
        }

        static double Deg(double rad) => rad * 180.0 / Math.PI;

        static double Median(List<double> v)
        {
            if (v.Count == 0) return 0;
            var s = v.OrderBy(x => x).ToList();
            return s.Count % 2 == 1 ? s[s.Count / 2] : .5 * (s[s.Count / 2 - 1] + s[s.Count / 2]);
        }
    }
}
