// Body.Tests: dotnet run -c Release -- [all | bodies mass fk handover resid jac warm fit anybody reserve time] [options]
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WonderGather.AnyBody.Tests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var which = new HashSet<string>(args.Where(a => !a.StartsWith("--")));
            var opts = args.Where(a => a.StartsWith("--")).ToList();
            if (which.Count == 0 || which.Contains("all")) which = new HashSet<string> { "bodies", "mass", "fk", "handover", "resid", "jac", "warm", "fit", "anybody", "reserve" };
            try
            {
                if (which.Contains("debugfk")) DebugRun.Fk0();
                if (which.Contains("bodies")) ExactTests.BodiesMatch();
                if (which.Contains("mass")) MassDataTests.Run();
                if (which.Contains("fk")) ExactTests.KinematicsMatch();
                if (which.Contains("handover")) HandoverTests.StaticsMatch();
                if (which.Contains("resid")) ExactTests.ResidualsMatch();
                if (which.Contains("jac")) FitTests.JacobianMatches();
                if (which.Contains("warm")) FitTests.WarmFromSaved(opts);
                if (which.Contains("fit")) FitTests.FitsMatch(opts);
                if (which.Contains("anybody")) AnyBodyTests.Run(opts);
                if (which.Contains("reserve")) ReserveAtFitsTests.Run(opts);
                if (which.Contains("time")) FitTests.Timing(opts);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION " + ex);
                Check.Failures++;
            }
            Console.WriteLine($"== {Check.Passes} checks passed, {Check.Failures} failed ({sw.Elapsed.TotalSeconds:F1} s)");
            return Check.Failures;
        }
    }
}
