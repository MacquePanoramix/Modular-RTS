// The reserve LP written directly as b3.py writes it (primal: 4 nc + 1 unknowns, 6 equalities, 2 rows per checked
// direction), from Core's helpers (StaticsProblem.Equilibrium, TorqueMap, PyramidEdges), solved by LinProg (phase 1 +
// 2). An independent path to S, and the way to ask for the range of the hands' share over all optimal force splits.
using System;

namespace WonderGather.AnyBody.Tests
{
    sealed class PrimalReserve
    {
        public int N, Meq, Mub;
        public double[] Aeq, Beq, Aub, Bub, Cost;
        public Vec3[] Gens;
        public double Weight;

        public PrimalReserve(StaticsProblem p)
        {
            int nc = p.Contacts.Length;
            Weight = p.Weight;
            N = 4 * nc + 1;
            Gens = new Vec3[4 * nc];
            var e4 = new Vec3[4];
            for (int i = 0; i < nc; i++)
            {
                StaticsProblem.PyramidEdges(p.Contacts[i].Normal, p.Contacts[i].Mu, e4);
                for (int k = 0; k < 4; k++) Gens[4 * i + k] = e4[k];
            }

            // equalities, unknowns in body weights (lambda = W mu), rows divided by W
            var a = new double[6, Math.Max(1, 3 * nc)];
            var b = new double[6];
            p.Equilibrium(Vec3.Zero, a, b);
            Meq = 6;
            Aeq = new double[6 * N];
            Beq = new double[6];
            for (int r = 0; r < 6; r++)
            {
                Beq[r] = b[r] / Weight;
                for (int i = 0; i < nc; i++)
                    for (int k = 0; k < 4; k++)
                    {
                        var g = Gens[4 * i + k];
                        Aeq[r * N + 4 * i + k] = a[r, 3 * i] * g.X + a[r, 3 * i + 1] * g.Y + a[r, 3 * i + 2] * g.Z;
                    }
            }

            // inequalities, divided by the capacity
            int rows = 0;
            foreach (var j in p.Joints) rows += 2 * (j.IsHinge ? 1 : StaticsProblem.BallDirections.Length);
            Mub = rows;
            Aub = new double[rows * N];
            Bub = new double[rows];
            var bm = new double[3, Math.Max(1, 3 * nc)];
            int q = 0;
            for (int jj = 0; jj < p.Joints.Length; jj++)
            {
                var jt = p.Joints[jj];
                var t0 = p.TorqueMap(jj, Vec3.Zero, bm);
                var dirs = jt.IsHinge ? new[] { jt.Axis } : StaticsProblem.BallDirections;
                double sc = 1.0 / jt.Capacity;
                foreach (var d in dirs)
                {
                    double c0 = Vec3.Dot(d, t0) * sc;
                    for (int sgn = -1; sgn <= 1; sgn += 2)
                    {
                        for (int i = 0; i < nc; i++)
                            for (int k = 0; k < 4; k++)
                            {
                                var g = Gens[4 * i + k];
                                double v = 0;
                                for (int r = 0; r < 3; r++)
                                    v += d[r] * (bm[r, 3 * i] * g.X + bm[r, 3 * i + 1] * g.Y + bm[r, 3 * i + 2] * g.Z);
                                Aub[q * N + 4 * i + k] = sgn * v * Weight * sc;
                            }

                        Aub[q * N + N - 1] = -1.0;
                        Bub[q] = sgn * c0;
                        q++;
                    }
                }
            }

            Cost = new double[N];
            for (int j = 0; j < N - 1; j++) Cost[j] = ReserveSolver.ForceCost;
            Cost[N - 1] = 1.0;
        }

        /// <summary>min s + 1e-4 sum: the reserve. Returns S (+inf when infeasible) and the unknowns.</summary>
        public double Solve(DenseSimplex lp, double[] x, out LpStatus st)
        {
            st = LinProg.Solve(lp, N, Cost, Meq, Aeq, Beq, Mub, Aub, Bub, null, null, x, out _);
            return st == LpStatus.Optimal ? x[N - 1] : double.PositiveInfinity;
        }

        /// <summary>The smallest and largest hands' share (body weights) over force splits with s <= sMax.</summary>
        public (double lo, double hi) HandsRange(StaticsProblem p, DenseSimplex lp, double sMax)
        {
            var c = new double[N];
            bool any = false;
            for (int i = 0; i < p.Contacts.Length; i++)
            {
                if (!p.Contacts[i].OnGround || !BodyLinks.IsForearm(p.Contacts[i].Link)) continue;
                for (int k = 0; k < 4; k++) c[4 * i + k] = Gens[4 * i + k].Z;
                any = true;
            }

            if (!any) return (0, 0);
            var hi = new double[N];
            var lo = new double[N];
            for (int j = 0; j < N; j++) hi[j] = double.PositiveInfinity;
            hi[N - 1] = sMax;
            var x = new double[N];
            var st1 = LinProg.Solve(lp, N, c, Meq, Aeq, Beq, Mub, Aub, Bub, lo, hi, x, out double fmin);
            for (int j = 0; j < N; j++) c[j] = -c[j];
            var st2 = LinProg.Solve(lp, N, c, Meq, Aeq, Beq, Mub, Aub, Bub, lo, hi, x, out double fmax);
            if (st1 != LpStatus.Optimal || st2 != LpStatus.Optimal) return (double.NaN, double.NaN);
            return (fmin, -fmax);
        }
    }
}
