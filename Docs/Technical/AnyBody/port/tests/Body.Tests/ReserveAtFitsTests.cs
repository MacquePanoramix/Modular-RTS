// End to end: each of the 126 res_miners2.json stances (Small, Long, Round x 42) fitted from its start pose by the
// C# fitter, handed to the reserve LP (src/Reserve, the LP agent's), and S read against the prototype's s at its own
// saved pose (reserve_cases.json 'py.s', the LP optimum; unique, unlike r). Also S at the saved pose through the C#
// path, which must be the prototype's s (the hand-over and the LP together).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace WonderGather.AnyBody.Tests
{
    public static class ReserveAtFitsTests
    {
        public static void Run(List<string> opts)
        {
            Console.WriteLine("== reserve: the 126 miner stances fitted from x0, S by the reserve LP, against the prototype's s at its saved pose");
            string path = Path.Combine(Ref.Dir, "reserve_cases.json");
            if (!File.Exists(path))
            {
                Console.WriteLine("   (no reserve_cases.json: run reference/export_reserve.py)");
                return;
            }
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var cases = Ref.Cases().Where(c => c.File == "res_miners2.json").ToList();
            var solver = new ReserveSolver();
            var fo = new FitOptions { RelaxFreeLimbs = false };
            int n = 0, heldSame = 0, heldBoth = 0, savedPathBad = 0;
            var dS = new List<double>();
            var rows = new List<(string tag, double s0, double s1, double c0, double c1)>();
            foreach (var e in doc.RootElement.GetProperty("cases").EnumerateArray())
            {
                if (e.GetProperty("file").GetString() != "res_miners2.json") continue;
                string bn = e.GetProperty("body").GetString(), sn = e.GetProperty("stance").GetString();
                // by family and arguments: some stances share a name (the half-kneel's rise with either foot forward)
                string fam = e.GetProperty("family").GetString(), args = ArgKey(Ref.Args(e.GetProperty("args")));
                var cs = cases.FirstOrDefault(c => c.Body == bn && c.Family == fam && ArgKey(c.Args) == args);
                if (cs == null) continue;
                var py = e.GetProperty("py");
                double sPy = py.GetProperty("s").ValueKind == JsonValueKind.Number ? py.GetProperty("s").GetDouble() : double.PositiveInfinity;
                var b = ExactTests.Body(bn);
                var st = StanceLibrary.Make(b, cs.Family, cs.Args);
                // the C# path at the saved pose: must give the prototype's s
                var rSaved = solver.Solve(Statics.Build(b, cs.Q, st.Supports));
                bool samePath = (rSaved.Held == !double.IsInfinity(sPy)) && (!rSaved.Held || Math.Abs(rSaved.S - sPy) < 1e-6);
                if (!Check.That(samePath, $"{bn} / {sn}: S at the saved pose {rSaved.S:G6}, prototype {sPy:G6}")) savedPathBad++;
                // the C# fit from x0
                var fit = StanceFitter.Fit(b, st, fo);
                var r = solver.Solve(Statics.Build(b, fit.Q, st.Supports));
                bool heldPy = !double.IsInfinity(sPy);
                if (r.Held == heldPy) heldSame++;
                else
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "     held differs: {0} / {1}: C# fit {2} (S {3:G4}, cost {4:F3}), prototype {5} (s {6:G4}, cost {7:F3})",
                        bn, sn, r.Held ? "held" : "not held", r.S, fit.Cost, heldPy ? "held" : "not held", sPy, cs.Cost));
                if (r.Held && heldPy)
                {
                    heldBoth++;
                    dS.Add(r.S - sPy);
                    rows.Add((bn + " / " + sn, sPy, r.S, cs.Cost, fit.Cost));
                }
                n++;
            }
            var ad = dS.Select(Math.Abs).OrderBy(x => x).ToArray();
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "   {0} stances: S at the saved pose through the C# path = the prototype's s in {1}; held or not the same at the C# fit in {2}; " +
                "both held {3}: |S(C# fit) - s(prototype)| median {4:F3}, 90th pct {5:F3}, max {6:F3}; within 0.02: {7}, within 0.05: {8}; " +
                "mean S {9:F3} vs {10:F3}",
                n, n - savedPathBad, heldSame, heldBoth, ad.Length > 0 ? ad[ad.Length / 2] : 0, ad.Length > 0 ? ad[(int)(0.9 * ad.Length)] : 0,
                ad.Length > 0 ? ad[ad.Length - 1] : 0, ad.Count(x => x <= 0.02), ad.Count(x => x <= 0.05),
                rows.Count > 0 ? rows.Average(x => x.s1) : 0, rows.Count > 0 ? rows.Average(x => x.s0) : 0));
            foreach (var x in rows.OrderByDescending(x => Math.Abs(x.s1 - x.s0)).Take(6))
                Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "     {0,-75} S {1:F3} vs {2:F3}  (cost {3:F3} vs {4:F3})",
                    x.tag.Length > 75 ? x.tag.Substring(0, 75) : x.tag, x.s1, x.s0, x.c1, x.c0));
            Check.That(savedPathBad == 0, "S at the saved poses through Statics.Build + ReserveSolver = the prototype's s");
            Check.That(n == 126, $"all 126 res_miners2 stances found ({n})");
        }

        static string ArgKey(object[] a) => string.Join("|", a.Select(x => Convert.ToString(x, CultureInfo.InvariantCulture)));
    }
}
