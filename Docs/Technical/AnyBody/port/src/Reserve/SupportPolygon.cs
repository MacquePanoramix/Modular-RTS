// The base margin of b3.py: the convex hull of the ground contacts (hull) and the signed distance of the centre of
// mass from it, positive inside (poly_margin). Points are rounded to 1e-9 m and de-duplicated before the hull, as
// b3.py does; collinear points are dropped (Andrew's monotone chain, popping on cross <= 0).
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody
{
    public static class SupportPolygon
    {
        /// <summary>run3.py's sp_margin: poly_margin of the ground contacts' (x, y) around Com.(x, y); -1 when nothing
        /// touches the ground.</summary>
        public static double Margin(StaticsProblem p) => Margin(p, Vec3.Zero);

        /// <summary>The same with the centre of mass shifted (horizontally) by shift.</summary>
        public static double Margin(StaticsProblem p, Vec3 shift)
        {
            int n = 0;
            foreach (var c in p.Contacts)
                if (c.OnGround) n++;
            if (n == 0) return -1.0;
            var xy = new double[2 * n];
            int k = 0;
            foreach (var c in p.Contacts)
            {
                if (!c.OnGround) continue;
                xy[k++] = c.Point.X;
                xy[k++] = c.Point.Y;
            }

            var com = p.Com + shift;
            return Margin(xy, n, com.X, com.Y);
        }

        /// <summary>b3.poly_margin(points, c): xy holds n points as x0, y0, x1, y1, ...</summary>
        public static double Margin(double[] xy, int n, double cx, double cy)
        {
            if (n <= 0) throw new ArgumentException("no points");
            var h = Hull(xy, n);
            int m = h.Count;
            if (m == 1) return -Dist(cx - h[0].x, cy - h[0].y);
            if (m == 2)
            {
                double ax = h[0].x, ay = h[0].y, ex = h[1].x - ax, ey = h[1].y - ay;
                double t = ((cx - ax) * ex + (cy - ay) * ey) / Math.Max(1e-12, ex * ex + ey * ey);
                t = Math.Max(0.0, Math.Min(1.0, t));
                return -Dist(cx - (ax + t * ex), cy - (ay + t * ey));
            }

            bool inside = true;
            double dmin = double.PositiveInfinity;
            for (int k = 0; k < m; k++)
            {
                double ax = h[k].x, ay = h[k].y;
                double ex = h[(k + 1) % m].x - ax, ey = h[(k + 1) % m].y - ay;
                double L = Dist(ex, ey);
                // outward normal of a counter-clockwise hull
                double nx = ey / L, ny = -ex / L;
                double dn = (cx - ax) * nx + (cy - ay) * ny;
                if (dn > 0) inside = false;
                double t = ((cx - ax) * ex + (cy - ay) * ey) / (L * L);
                t = Math.Max(0.0, Math.Min(1.0, t));
                dmin = Math.Min(dmin, Dist(cx - (ax + t * ex), cy - (ay + t * ey)));
            }

            return inside ? dmin : -dmin;
        }

        /// <summary>b3.hull: the convex hull, counter-clockwise, from the lowest-leftmost point; 1 or 2 points when
        /// the set is that small or collinear.</summary>
        public static List<(double x, double y)> Hull(double[] xy, int n)
        {
            var pts = new List<(double x, double y)>(n);
            for (int i = 0; i < n; i++) pts.Add((Math.Round(xy[2 * i], 9), Math.Round(xy[2 * i + 1], 9)));
            pts.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            // de-duplicate (sorted)
            int w = 0;
            for (int i = 0; i < pts.Count; i++)
                if (w == 0 || pts[i] != pts[w - 1])
                    pts[w++] = pts[i];
            pts.RemoveRange(w, pts.Count - w);
            if (pts.Count <= 2) return pts;
            var lower = new List<(double x, double y)>();
            var upper = new List<(double x, double y)>();
            foreach (var q in pts)
            {
                while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], q) <= 0) lower.RemoveAt(lower.Count - 1);
                lower.Add(q);
            }

            for (int i = pts.Count - 1; i >= 0; i--)
            {
                var q = pts[i];
                while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], q) <= 0) upper.RemoveAt(upper.Count - 1);
                upper.Add(q);
            }

            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);
            return lower;
        }

        static double Cross((double x, double y) o, (double x, double y) a, (double x, double y) c) =>
            (a.x - o.x) * (c.y - o.y) - (a.y - o.y) * (c.x - o.x);

        static double Dist(double x, double y) => Math.Sqrt(x * x + y * y);
    }
}
