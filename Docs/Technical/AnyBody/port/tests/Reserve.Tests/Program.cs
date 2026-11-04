// Console test runner for src/Reserve.
//   dotnet run -c Release --project tests/Reserve.Tests [-- [--quick] [--no-timing] [reference-dir]]
// Exit code: the number of failed checks.
using System;
using System.IO;
using System.Linq;

namespace WonderGather.AnyBody.Tests
{
    static class Program
    {
        static int Main(string[] args)
        {
            bool quick = args.Contains("--quick");
            bool timing = !args.Contains("--no-timing");
            int caseArg = Array.IndexOf(args, "--case") + 1;
            string refDir = args.Where((a, i) => !a.StartsWith("--") && i != caseArg).FirstOrDefault() ?? FindReference();
            Console.WriteLine("Reserve tests; reference data: " + refDir);
            var t = new Check();
            SimplexTests.Small(t);
            string lpPath = Path.Combine(refDir, "lp_random.json");
            if (File.Exists(lpPath)) SimplexTests.Random(t, lpPath);
            else Console.WriteLine("   (no lp_random.json: run reference/export_lp_random.py)");
            string casesPath = Path.Combine(refDir, "reserve_cases.json");
            int ci = Array.IndexOf(args, "--case");
            if (ci >= 0 && File.Exists(casesPath))
            {
                ReserveTests.Debug(ReserveCases(casesPath), args[ci + 1]);
                return 0;
            }

            if (File.Exists(casesPath))
            {
                var cases = ReserveCases(casesPath);
                ReserveTests.Settings(t, cases);
                ReserveTests.Run(t, cases, quick);
                if (timing) ReserveTests.Timings(cases, quick ? 3 : 20);
            }
            else
                t.True(false, "no reserve_cases.json: run reference/export_reserve.py");

            t.Summary();
            return t.Failed;
        }

        static System.Collections.Generic.List<ReserveCase> ReserveCases(string path)
        {
            var cases = ReserveCase.Load(path);
            Console.WriteLine($"   {cases.Count} saved fits ({cases.Count(c => c.Golden)} from res_miners2.json)");
            return cases;
        }

        static string FindReference()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var r = Path.Combine(dir.FullName, "reference");
                if (File.Exists(Path.Combine(r, "reserve_cases.json"))) return r;
                dir = dir.Parent;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "reference");
        }
    }
}
