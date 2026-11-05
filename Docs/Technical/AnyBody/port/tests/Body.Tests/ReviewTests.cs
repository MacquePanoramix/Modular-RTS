// Regression checks for what the independent review (REVIEW.md, November 4) found and fixed in src/Body: the effort
// split's Jacobian broke when the AD tape grew during it; Kinematics.Forward made ~1 kB of garbage per evaluation; a NaN
// pose threw IndexOutOfRange in the base margin.
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody.Tests
{
    public static class ReviewTests
    {
        public static void Run()
        {
            Console.WriteLine("== review: the tape growing during an evaluation, no garbage per evaluation, a NaN pose");
            TapeGrowth();
            NoGarbage();
            NanPose();
        }

        /// <summary>The effort split's values and gradient rows, from tapes started at every size from 16 to 2048 rows
        /// (so the tape grows inside it at some), against a tape that never grows: bitwise the same.</summary>
        static void TapeGrowth()
        {
            int runs = 0, same = 0;
            foreach (var (bn, fam, a) in new (string, string, object[])[]
                     {
                         ("Small", "stand", new object[0]), ("Round", "halfkneel", new object[] { "L", "both" }), ("Long", "forearms", new object[0])
                     })
            {
                var b = ExactTests.Body(bn);
                var st = StanceLibrary.Make(b, fam, a);
                var p = new StanceProblem(b, st);
                var q = p.Full(p.StartVector());
                double[] Rows(int capacity, out int count)
                {
                    var tape = new Tape(p.N, capacity);
                    tape.Reset(p.N);
                    var qd = Kinematics.Lift(q, p.Free, tape);
                    var k = new Kin();
                    Kinematics.Forward(b, qd, k);
                    var C = new List<DContact>();
                    Supports.Contacts(b, k, st.Supports, C);
                    var r = new List<D>();
                    Effort.Append(k, C, st.Effort, tape, new EffortWork(), r);
                    count = r.Count;
                    var o = new double[r.Count * (p.N + 1)];
                    for (int i = 0; i < r.Count; i++)
                    {
                        o[i * (p.N + 1)] = r[i].V;
                        tape.Grad(r[i], o, i * (p.N + 1) + 1);
                    }
                    return o;
                }

                var want = Rows(1 << 16, out _);
                for (int cap = 16; cap <= 2048; cap += 16)
                {
                    var got = Rows(cap, out int m);
                    runs++;
                    bool eq = got.Length == want.Length;
                    for (int i = 0; eq && i < got.Length; i++) eq = got[i].Equals(want[i]);
                    if (eq) same++;
                    else Check.That(false, $"{bn} / {st.Name}: effort split from a tape of {cap} rows differs");
                }
            }

            Console.WriteLine($"   effort split from tapes of 16..2048 rows (growing inside it at some): {same}/{runs} bitwise as an unbounded tape");
            Check.That(same == runs, "the Jacobian does not depend on where the tape grows");
        }

        /// <summary>After warm-up, Evaluate (with and without the Jacobian) allocates nothing on the GC heap.</summary>
        static void NoGarbage()
        {
            var b = ExactTests.Body("Round");
            var st = StanceLibrary.Make(b, "halfkneel", "L", "both");
            var p = new StanceProblem(b, st);
            var x = p.StartVector();
            var r = new List<D>(512);
            for (int i = 0; i < 3; i++)
            {
                p.Evaluate(x, false, r);
                p.Evaluate(x, true, r);
            }

            long a0 = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 20; i++)
            {
                p.Evaluate(x, false, r);
                p.Evaluate(x, true, r);
            }

            long bytes = GC.GetAllocatedBytesForCurrentThread() - a0;
            Console.WriteLine($"   40 evaluations after warm-up: {bytes} bytes allocated");
            Check.That(bytes == 0, $"Evaluate allocates nothing once warm ({bytes} B in 40)");
        }

        /// <summary>A NaN pose gives NaN residuals (no exception), and a fit from it ends with status "nan".</summary>
        static void NanPose()
        {
            var b = ExactTests.Body("Small");
            var st = StanceLibrary.Stand(b);
            var p = new StanceProblem(b, st);
            var x = p.StartVector();
            x[2] = double.NaN;
            var r = new List<D>();
            bool threw = false;
            try { p.Evaluate(x, false, r); }
            catch (Exception) { threw = true; }
            Check.That(!threw && double.IsNaN(StanceProblem.Cost(r)), "a NaN pose evaluates to NaN without an exception");
            var q = p.Full(p.StartVector());
            q[Pose.Pitch] = double.NaN;
            FitResult fit = null;
            threw = false;
            try { fit = StanceFitter.Fit(b, st, new FitOptions { Start = q, RelaxFreeLimbs = false }); }
            catch (Exception) { threw = true; }
            Check.That(!threw && fit.Status == "nan" && double.IsPositiveInfinity(fit.Cost), "a fit from a NaN start ends as 'nan', cost +inf");
            Console.WriteLine($"   a NaN pose: {(threw ? "THREW" : "residuals NaN")}; a fit from a NaN start: {(fit == null ? "threw" : fit.Status + ", cost " + fit.Cost)}");
        }
    }
}
