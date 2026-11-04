using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody.Tests
{
    /// <summary>Counting checks: a failed check prints and counts; the exit code is the number of failures.</summary>
    public static class Check
    {
        public static int Failures;
        public static int Passes;
        static readonly HashSet<string> Printed = new HashSet<string>();

        public static bool That(bool ok, string what, int printLimitPerKey = 5)
        {
            if (ok)
            {
                Passes++;
                return true;
            }
            Failures++;
            string key = what.Length > 40 ? what.Substring(0, 40) : what;
            int n = 0;
            foreach (var p in Printed) if (p.StartsWith(key, StringComparison.Ordinal)) n++;
            if (n < printLimitPerKey)
            {
                Printed.Add(key + "#" + n);
                Console.WriteLine("  FAIL " + what);
            }
            return false;
        }

        public static bool Near(double a, double b, double tol, string what)
        {
            bool ok = Math.Abs(a - b) <= tol * Math.Max(1.0, Math.Abs(b)) || (double.IsInfinity(a) && a == b);
            return That(ok, $"{what}: got {a:G12}, want {b:G12}");
        }
    }
}
