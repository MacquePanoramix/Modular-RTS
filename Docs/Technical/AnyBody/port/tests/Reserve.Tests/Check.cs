// A minimal check/report helper for the console test runner.
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody.Tests
{
    sealed class Check
    {
        public int Passed, Failed;
        readonly List<string> failures = new List<string>();

        public void True(bool ok, string what)
        {
            if (ok) Passed++;
            else
            {
                Failed++;
                failures.Add(what);
                Console.WriteLine("  FAIL " + what);
            }
        }

        public void Near(double got, double want, double tol, string what)
        {
            bool ok = (double.IsPositiveInfinity(got) && double.IsPositiveInfinity(want)) || Math.Abs(got - want) <= tol;
            True(ok, ok ? what : FormattableString.Invariant($"{what}: got {got:G10}, want {want:G10} (tol {tol:G3})"));
        }

        public void Summary()
        {
            Console.WriteLine();
            Console.WriteLine(FormattableString.Invariant($"== {Passed} checks passed, {Failed} failed"));
            foreach (var f in failures) Console.WriteLine("   " + f);
        }

        public static string F(double v, int digits = 4) =>
            double.IsPositiveInfinity(v) ? "inf" : v.ToString("F" + digits, System.Globalization.CultureInfo.InvariantCulture);
    }
}
