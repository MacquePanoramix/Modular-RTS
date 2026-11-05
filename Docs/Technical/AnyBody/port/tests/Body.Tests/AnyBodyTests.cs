// Bodies the prototype never saw (no saved fits to compare with): the 45 stances fitted for each, read for what any
// fit must be (finite, contacts on the ground, nothing through it) and for cost against the nearest prototype body.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WonderGather.AnyBody.Tests
{
    public static class AnyBodyTests
    {
        public static readonly BodyVariant[] NewBodies =
        {
            new BodyVariant { Name = "Tiny (Small x0.6, S0.8)", Source = "Small", Scale = 0.6, Strength = 0.8 },
            new BodyVariant { Name = "Lanky (Long, legs x1.2, arms x1.15, trunk x0.9)", Source = "Long", LegMul = 1.2, ArmMul = 1.15, TrunkMul = 0.9 },
            new BodyVariant
            {
                Name = "Stocky (Round x1.2, legs x0.8, trunk mass x1.3, S1.3)", Source = "Round", Scale = 1.2, LegMul = 0.8, Strength = 1.3,
                MassMul = new Dictionary<string, double> { ["trunk"] = 1.3 }
            },
            new BodyVariant
            {
                Name = "Frail (Long, S0.45, knees x0.6)", Source = "Long", Strength = 0.45,
                CapMul = new Dictionary<string, double> { ["knee"] = 0.6 }
            },
        };

        public static void Run(List<string> opts)
        {
            Console.WriteLine("== anybody: 4 bodies the prototype never saw x its 45 stances (no saved fits: read for soundness)");
            var fo = new FitOptions { RelaxFreeLimbs = false };
            int bad = 0;
            foreach (var v in NewBodies)
            {
                var b = BodyFactory.Make(v);
                var costs = new List<double>();
                var ms = new List<double>();
                int cerr = 0, pen = 0, nonFinite = 0;
                double worstCerr = 0, worstPen = 0;
                foreach (var (fam, args) in StanceLibrary.PrototypeBatch)
                {
                    var st = StanceLibrary.Make(b, fam, args);
                    var fit = StanceFitter.Fit(b, st, fo);
                    if (double.IsNaN(fit.Cost) || double.IsInfinity(fit.Cost) || fit.Q.Any(x => double.IsNaN(x)))
                    {
                        nonFinite++;
                        continue;
                    }
                    var rep = Statics.Report(b, st, fit.Q);
                    costs.Add(fit.Cost);
                    ms.Add(fit.Millis);
                    if (rep.ContactErrMm > 2.0) cerr++;
                    if (rep.PenetrationMm > 2.0) pen++;
                    worstCerr = Math.Max(worstCerr, rep.ContactErrMm);
                    worstPen = Math.Max(worstPen, rep.PenetrationMm);
                }
                var sorted = costs.OrderBy(x => x).ToArray();
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "   {0,-55} M {1,5:F1} kg  H {2:F2} m: cost median {3:F3}, mean {4:F3} (max {5:F2}); contacts > 2 mm {6} (max {7:F1}), " +
                    "penetration > 2 mm {8} (max {9:F1}); non-finite {10}; {11:F0} ms per fit",
                    v.Name, b.Mass, b.Height, sorted.Length > 0 ? sorted[sorted.Length / 2] : double.NaN, costs.Count > 0 ? costs.Average() : double.NaN,
                    sorted.Length > 0 ? sorted[sorted.Length - 1] : double.NaN, cerr, worstCerr, pen, worstPen, nonFinite, ms.Count > 0 ? ms.Average() : 0));
                bad += nonFinite;
                Check.That(nonFinite == 0, v.Name + ": every fit finite");
                Check.That(cerr + pen <= 5, $"{v.Name}: at most 5 of 45 fits off the ground or through it by over 2 mm ({cerr} + {pen})");
            }
        }
    }
}
