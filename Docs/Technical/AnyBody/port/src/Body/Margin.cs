// b3.hull + b3.poly_margin: the signed distance (positive inside) of the centre of mass to the convex hull of the
// ground points, in the horizontal plane, in D. The hull is chosen on values rounded to 1e-9 m, as the prototype's is.
using System;

namespace WonderGather.AnyBody
{
    /// <summary>Reusable buffers for PolyMargin (no allocation per call once grown).</summary>
    public sealed class MarginWork
    {
        internal double[] Rx = new double[32], Ry = new double[32];
        internal int[] Idx = new int[32], Hull = new int[64];
        internal DV3[] Pts = new DV3[32];
        internal int Count;

        public void Clear() => Count = 0;

        public void Add(in DV3 p)
        {
            if (Count == Pts.Length)
            {
                Array.Resize(ref Pts, Count * 2);
                Array.Resize(ref Rx, Count * 2);
                Array.Resize(ref Ry, Count * 2);
                Array.Resize(ref Idx, Count * 2);
                Array.Resize(ref Hull, Count * 4);
            }
            Pts[Count++] = p;
        }
    }

    public static class Margin
    {
        /// <summary>The prototype rounds the ground points to 1e-9 m before taking the hull (b3.hull). Tests of the
        /// Jacobian turn it off, since finite differences then see the rounding's steps.</summary>
        public static bool RoundHull = true;

        /// <summary>Python's round(v, 9) up to the last unit (half to even).</summary>
        public static double Round9(double v)
        {
            if (!RoundHull || Math.Abs(v) >= 1e6) return v;
            return Math.Round(v * 1e9, MidpointRounding.ToEven) / 1e9;
        }

        /// <summary>b3.poly_margin(points, c) over the points added to w (their x, y), c = (cx, cy).
        /// +infinity when there are no points (as the prototype's empty hull gives).</summary>
        public static D PolyMargin(MarginWork w, D cx, D cy)
        {
            int n = w.Count;
            var rx = w.Rx;
            var ry = w.Ry;
            var idx = w.Idx;
            for (int i = 0; i < n; i++)
            {
                rx[i] = Round9(w.Pts[i].X.V);
                ry[i] = Round9(w.Pts[i].Y.V);
                idx[i] = i;
            }
            // sorted(set(...)): sort by (x, y), drop exact duplicates (keep the first in input order)
            for (int i = 1; i < n; i++)
            {
                int t = idx[i];
                int j = i - 1;
                while (j >= 0 && Less(rx, ry, t, idx[j]))
                {
                    idx[j + 1] = idx[j];
                    j--;
                }
                idx[j + 1] = t;
            }
            int m = 0;
            for (int i = 0; i < n; i++)
            {
                if (m > 0 && rx[idx[i]] == rx[idx[m - 1]] && ry[idx[i]] == ry[idx[m - 1]]) continue;
                idx[m++] = idx[i];
            }
            var h = w.Hull;
            int nh;
            if (m <= 2)
            {
                for (int i = 0; i < m; i++) h[i] = idx[i];
                nh = m;
            }
            else
            {
                // Andrew's monotone chain, popping on cross <= 0 (collinear points dropped)
                int lo = 0;
                var lower = h; // first half of the buffer
                for (int i = 0; i < m; i++)
                {
                    int p = idx[i];
                    while (lo >= 2 && Cross(rx, ry, lower[lo - 2], lower[lo - 1], p) <= 0) lo--;
                    lower[lo++] = p;
                }
                int up0 = lo; // upper chain stored after the lower one
                int up = 0;
                for (int i = m - 1; i >= 0; i--)
                {
                    int p = idx[i];
                    while (up >= 2 && Cross(rx, ry, h[up0 + up - 2], h[up0 + up - 1], p) <= 0) up--;
                    h[up0 + up++] = p;
                }
                // lower[:-1] + upper[:-1]
                nh = 0;
                for (int i = 0; i < lo - 1; i++) h[nh++] = lower[i];
                for (int i = 0; i < up - 1; i++) h[nh++] = h[up0 + i];
            }
            if (nh == 0) return new D(double.PositiveInfinity);
            if (nh == 1)
            {
                var a = Vert(w, h[0]);
                return -Dist(cx - a.ax, cy - a.ay);
            }
            if (nh == 2)
            {
                var a = Vert(w, h[0]);
                var b = Vert(w, h[1]);
                return -SegDist(cx, cy, a, b, true);
            }
            bool inside = true;
            double dmin = double.PositiveInfinity;
            int kmin = -1;
            for (int k = 0; k < nh; k++)
            {
                int ia = h[k], ib = h[(k + 1) % nh];
                double ax = rx[ia], ay = ry[ia];
                double ex = rx[ib] - ax, ey = ry[ib] - ay;
                double L = Math.Sqrt(ex * ex + ey * ey);
                double nx = ey / L, ny = -ex / L;
                double dx = cx.V - ax, dy = cy.V - ay;
                double d = dx * nx + dy * ny;
                if (d > 0) inside = false;
                double t = Math.Max(0.0, Math.Min(1.0, (dx * ex + dy * ey) / (L * L)));
                double qx = cx.V - (ax + t * ex), qy = cy.V - (ay + t * ey);
                double dist = Math.Sqrt(qx * qx + qy * qy);
                if (dist < dmin)
                {
                    dmin = dist;
                    kmin = k;
                }
            }
            var va = Vert(w, h[kmin]);
            var vb = Vert(w, h[(kmin + 1) % nh]);
            var dd = SegDist(cx, cy, va, vb, false);
            return inside ? dd : -dd;
        }

        static bool Less(double[] rx, double[] ry, int a, int b) => rx[a] < rx[b] || (rx[a] == rx[b] && ry[a] < ry[b]);

        static double Cross(double[] rx, double[] ry, int o, int a, int c) =>
            (rx[a] - rx[o]) * (ry[c] - ry[o]) - (ry[a] - ry[o]) * (rx[c] - rx[o]);

        struct V2
        {
            public D ax, ay;
        }

        /// <summary>A hull vertex: the rounded value, with the original point's gradient.</summary>
        static V2 Vert(MarginWork w, int i) =>
            new V2 { ax = WithValue(w.Pts[i].X, w.Rx[i]), ay = WithValue(w.Pts[i].Y, w.Ry[i]) };

        static D WithValue(D d, double v) => d.T == null ? new D(v) : new D(d.T, v, d.G);

        static D Dist(D dx, D dy)
        {
            double v = Math.Sqrt(dx.V * dx.V + dy.V * dy.V);
            if (v <= 0) return new D(0.0);
            return WithValue(D.Sqrt(D.MulAdd(dx, dx, dy, dy)), v);
        }

        /// <summary>|c - (a + t e)|, t = clamp(((c - a).e) / (L L), 0, 1), as poly_margin computes it.</summary>
        static D SegDist(D cx, D cy, V2 a, V2 b, bool twoPoint)
        {
            var ex = b.ax - a.ax;
            var ey = b.ay - a.ay;
            var dx = cx - a.ax;
            var dy = cy - a.ay;
            double ee = ex.V * ex.V + ey.V * ey.V;
            double tv;
            if (twoPoint)
                tv = Math.Max(0.0, Math.Min(1.0, (dx.V * ex.V + dy.V * ey.V) / Math.Max(1e-12, ee)));
            else
            {
                double L = Math.Sqrt(ee);
                tv = Math.Max(0.0, Math.Min(1.0, (dx.V * ex.V + dy.V * ey.V) / (L * L)));
            }
            D t;
            if (tv <= 0.0) t = new D(0.0);
            else if (tv >= 1.0) t = new D(1.0);
            else t = WithValue(D.MulAdd(dx, ex, dy, ey) / D.MulAdd(ex, ex, ey, ey), tv);
            var qx = cx - (a.ax + t * ex);
            var qy = cy - (a.ay + t * ey);
            return Dist(qx, qy);
        }
    }
}
