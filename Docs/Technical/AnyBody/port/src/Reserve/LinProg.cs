// scipy.optimize.linprog's form on top of DenseSimplex, for general small problems (tests, and queries such as the
// range of the hands' share over the reserve LP's optimal forces):
//     minimise c.x  subject to  Aeq x = beq,  Aub x <= bub,  lo <= x <= hi.
// One slack per inequality; phase 1 finds a first vertex. The reserve LP itself goes through ReserveSolver (its dual),
// which is several times faster.
using System;

namespace WonderGather.AnyBody
{
    public static class LinProg
    {
        /// <summary>Dense row-major Aeq (meq x n) and Aub (mub x n); lo/hi may hold infinities (null: [0, +inf)).
        /// On Optimal, x (length n) holds the solution and fun the objective.</summary>
        public static LpStatus Solve(DenseSimplex lp, int n, double[] c, int meq, double[] aeq, double[] beq, int mub,
            double[] aub, double[] bub, double[] lo, double[] hi, double[] x, out double fun)
        {
            int m = meq + mub;
            int cols = n + mub;
            lp.Resize(m, cols);
            var A = lp.A;
            Array.Clear(A, 0, m * cols);
            for (int i = 0; i < meq; i++)
            {
                for (int j = 0; j < n; j++) A[i * cols + j] = aeq[i * n + j];
                lp.B[i] = beq[i];
            }

            for (int i = 0; i < mub; i++)
            {
                int r = meq + i;
                for (int j = 0; j < n; j++) A[r * cols + j] = aub[i * n + j];
                A[r * cols + n + i] = 1.0;
                lp.B[r] = bub[i];
            }

            for (int j = 0; j < n; j++)
            {
                lp.C[j] = c[j];
                lp.Lo[j] = lo != null ? lo[j] : 0.0;
                lp.Hi[j] = hi != null ? hi[j] : double.PositiveInfinity;
            }

            for (int j = n; j < cols; j++)
            {
                lp.C[j] = 0.0;
                lp.Lo[j] = 0.0;
                lp.Hi[j] = double.PositiveInfinity;
            }

            var st = lp.Solve();
            fun = double.NaN;
            if (st == LpStatus.Optimal)
            {
                double f = 0.0;
                for (int j = 0; j < n; j++)
                {
                    x[j] = lp.X[j];
                    f += c[j] * x[j];
                }

                fun = f;
            }

            return st;
        }
    }
}
