// The feasible-region depth of b3.py (feasible_region): how far the centre of mass may wander horizontally before no
// force split holds the pose at a reserve <= w. Along ndir directions from the present centre of mass the boundary is
// found by 11 bisections in [0, tmax] (or tmax itself if it still holds); the depth is the distance from the present
// centre of mass to the polygon through the boundary points. -1 when the pose itself needs more than w.
// Each direction's linear programs differ from the last only in the shift, so the solver re-uses its basis.
using System;

namespace WonderGather.AnyBody
{
    public sealed class FeasibleRegionResult
    {
        /// <summary>The reserve at the present centre of mass (b3: s0).</summary>
        public double S0;

        /// <summary>b3's depth: distance (m) from the centre of mass to the boundary polygon; -1 when S0 > w.</summary>
        public double Depth;

        /// <summary>The boundary points (x, y), relative to the centre of mass, one per direction (empty when S0 > w).</summary>
        public (double x, double y)[] Points = Array.Empty<(double, double)>();

        /// <summary>Linear programs solved, and their pivots.</summary>
        public int LpCount;

        public long Pivots;
    }

    public static class FeasibleRegion
    {
        /// <summary>b3.feasible_region(K, C, w, ndir, tmax), with a solver of its own.</summary>
        public static FeasibleRegionResult Compute(StaticsProblem p, double w = 1.0, int ndir = 12, double tmax = 1.5) =>
            Compute(new ReserveSolver(), p, w, ndir, tmax);

        /// <summary>b3.feasible_region(K, C, w, ndir, tmax), re-using solver (prepared here for p).</summary>
        public static FeasibleRegionResult Compute(ReserveSolver solver, StaticsProblem p, double w = 1.0, int ndir = 12,
            double tmax = 1.5, int bisections = 11)
        {
            solver.Prepare(p);
            long piv0 = solver.TotalPivots;
            var res = new FeasibleRegionResult();
            int lps = 0;
            double s0 = solver.Reserve(Vec3.Zero);
            lps++;
            res.S0 = s0;
            if (s0 > w)
            {
                res.Depth = -1.0;
                res.LpCount = lps;
                res.Pivots = solver.TotalPivots - piv0;
                return res;
            }

            var pts = new (double x, double y)[ndir];
            for (int k = 0; k < ndir; k++)
            {
                double ang = 2 * Math.PI * k / ndir;
                double ux = Math.Cos(ang), uy = Math.Sin(ang);
                double lo = 0.0, hi = tmax;
                lps++;
                if (solver.Reserve(new Vec3(hi * ux, hi * uy, 0.0)) <= w)
                {
                    pts[k] = (hi * ux, hi * uy);
                    continue;
                }

                for (int it = 0; it < bisections; it++)
                {
                    double mid = 0.5 * (lo + hi);
                    lps++;
                    if (solver.Reserve(new Vec3(mid * ux, mid * uy, 0.0)) <= w) lo = mid;
                    else hi = mid;
                }

                pts[k] = (lo * ux, lo * uy);
            }

            res.Points = pts;
            res.Depth = DistanceToPolygon(pts);
            res.LpCount = lps;
            res.Pivots = solver.TotalPivots - piv0;
            return res;
        }

        /// <summary>The distance from the origin to the closed polygon through pts (b3: dmin over its edges).</summary>
        public static double DistanceToPolygon((double x, double y)[] pts)
        {
            int n = pts.Length;
            double dmin = double.PositiveInfinity;
            for (int k = 0; k < n; k++)
            {
                var a = pts[k];
                var c = pts[(k + 1) % n];
                double ex = c.x - a.x, ey = c.y - a.y;
                double L2 = ex * ex + ey * ey;
                double d;
                if (L2 < 1e-12) d = Math.Sqrt(a.x * a.x + a.y * a.y);
                else
                {
                    double t = Math.Max(0.0, Math.Min(1.0, -(a.x * ex + a.y * ey) / L2));
                    double px = a.x + t * ex, py = a.y + t * ey;
                    d = Math.Sqrt(px * px + py * py);
                }

                dmin = Math.Min(dmin, d);
            }

            return dmin;
        }
    }
}
