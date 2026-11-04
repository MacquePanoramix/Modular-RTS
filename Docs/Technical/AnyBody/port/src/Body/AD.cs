// Forward-mode automatic differentiation for the fitter's exact Jacobians.
// A D is a value plus a gradient row over the N free pose variables. Gradient rows live in one Tape buffer (bump
// allocation, reset per evaluation), so an evaluation allocates nothing on the GC heap once the tape has grown.
// With N = 0 (or a null tape) every D is a plain value: the same code then evaluates residuals without derivatives.
using System;

namespace WonderGather.AnyBody
{
    /// <summary>Storage for gradient rows of N doubles each. Not thread-safe: one per worker.</summary>
    public sealed class Tape
    {
        internal double[] Buf;
        internal int Top;

        /// <summary>Number of derivative directions (0: values only).</summary>
        public int N { get; private set; }

        public Tape(int n, int capacityRows = 8192)
        {
            N = n;
            Buf = new double[Math.Max(1, n) * capacityRows];
        }

        /// <summary>Start a new evaluation with n directions; earlier D values become invalid.</summary>
        public void Reset(int n)
        {
            N = n;
            Top = 0;
        }

        internal int Alloc()
        {
            int o = Top;
            Top += N;
            if (Top > Buf.Length)
            {
                var nb = new double[Math.Max(Buf.Length * 2, Top + 1024 * Math.Max(1, N))];
                Array.Copy(Buf, nb, o);
                Buf = nb;
            }
            return o;
        }

        /// <summary>The independent variable k with value v.</summary>
        public D Var(double v, int k)
        {
            if (N == 0) return new D(v);
            int o = Alloc();
            Array.Clear(Buf, o, N);
            Buf[o + k] = 1.0;
            return new D(this, v, o);
        }

        /// <summary>A D whose gradient row is given (copied).</summary>
        public D Make(double v, double[] grad, int offset = 0)
        {
            if (N == 0) return new D(v);
            int o = Alloc();
            Array.Copy(grad, offset, Buf, o, N);
            return new D(this, v, o);
        }

        /// <summary>Copies d's gradient into dst[offset .. offset + N) (zeros for a constant).</summary>
        public void Grad(D d, double[] dst, int offset = 0)
        {
            if (d.T == null) Array.Clear(dst, offset, N);
            else Array.Copy(Buf, d.G, dst, offset, N);
        }

        public double GradAt(D d, int k) => d.T == null ? 0.0 : Buf[d.G + k];
    }

    /// <summary>A scalar with its gradient (forward-mode AD).</summary>
    public readonly struct D
    {
        public readonly double V;
        internal readonly Tape T; // null: a constant
        internal readonly int G;

        public D(double v)
        {
            V = v;
            T = null;
            G = -1;
        }

        internal D(Tape t, double v, int g)
        {
            V = v;
            T = t;
            G = g;
        }

        public bool IsConst => T == null;

        public static implicit operator D(double v) => new D(v);

        public override string ToString() => V.ToString("G9", System.Globalization.CultureInfo.InvariantCulture);

        // ---- the gradient kernels: out = ca*ga (+ cb*gb (+ cc*gc))
        static D Lin1(Tape t, double v, double ca, in D a)
        {
            if (a.T == null) return new D(v);
            int o = t.Alloc();
            var buf = t.Buf;
            int n = t.N, ga = a.G;
            for (int k = 0; k < n; k++) buf[o + k] = ca * buf[ga + k];
            return new D(t, v, o);
        }

        static D Lin2(Tape t, double v, double ca, in D a, double cb, in D b)
        {
            if (a.T == null) return Lin1(t, v, cb, b);
            if (b.T == null) return Lin1(t, v, ca, a);
            int o = t.Alloc();
            var buf = t.Buf;
            int n = t.N, ga = a.G, gb = b.G;
            for (int k = 0; k < n; k++) buf[o + k] = ca * buf[ga + k] + cb * buf[gb + k];
            return new D(t, v, o);
        }

        static D Lin3(Tape t, double v, double ca, in D a, double cb, in D b, double cc, in D c)
        {
            if (a.T == null) return Lin2(t, v, cb, b, cc, c);
            if (b.T == null) return Lin2(t, v, ca, a, cc, c);
            if (c.T == null) return Lin2(t, v, ca, a, cb, b);
            int o = t.Alloc();
            var buf = t.Buf;
            int n = t.N, ga = a.G, gb = b.G, gc = c.G;
            for (int k = 0; k < n; k++) buf[o + k] = ca * buf[ga + k] + cb * buf[gb + k] + cc * buf[gc + k];
            return new D(t, v, o);
        }

        static Tape TapeOf(in D a, in D b) => a.T ?? b.T;

        // ---- arithmetic
        public static D operator +(D a, D b)
        {
            var t = TapeOf(a, b);
            double v = a.V + b.V;
            if (t == null) return new D(v);
            if (a.T == null) return new D(t, v, b.G);
            if (b.T == null) return new D(t, v, a.G);
            return Lin2(t, v, 1.0, a, 1.0, b);
        }

        public static D operator -(D a, D b)
        {
            var t = TapeOf(a, b);
            double v = a.V - b.V;
            if (t == null) return new D(v);
            if (b.T == null) return new D(t, v, a.G);
            return Lin2(t, v, 1.0, a, -1.0, b);
        }

        public static D operator -(D a) => a.T == null ? new D(-a.V) : Lin1(a.T, -a.V, -1.0, a);

        public static D operator +(D a, double s) => a.T == null ? new D(a.V + s) : new D(a.T, a.V + s, a.G);
        public static D operator +(double s, D a) => a + s;
        public static D operator -(D a, double s) => a.T == null ? new D(a.V - s) : new D(a.T, a.V - s, a.G);
        public static D operator -(double s, D a) => a.T == null ? new D(s - a.V) : Lin1(a.T, s - a.V, -1.0, a);

        public static D operator *(D a, D b)
        {
            var t = TapeOf(a, b);
            double v = a.V * b.V;
            if (t == null) return new D(v);
            return Lin2(t, v, b.V, a, a.V, b);
        }

        public static D operator *(D a, double s) => a.T == null ? new D(a.V * s) : Lin1(a.T, a.V * s, s, a);
        public static D operator *(double s, D a) => a * s;

        public static D operator /(D a, D b)
        {
            var t = TapeOf(a, b);
            double v = a.V / b.V;
            if (t == null) return new D(v);
            double ib = 1.0 / b.V;
            return Lin2(t, v, ib, a, -v * ib, b);
        }

        public static D operator /(D a, double s) => a.T == null ? new D(a.V / s) : Lin1(a.T, a.V / s, 1.0 / s, a);

        public static D operator /(double s, D b)
        {
            double v = s / b.V;
            return b.T == null ? new D(v) : Lin1(b.T, v, -v / b.V, b);
        }

        /// <summary>a*b + c*d in one gradient row.</summary>
        public static D MulAdd(in D a, in D b, in D c, in D d)
        {
            var t = a.T ?? b.T ?? c.T ?? d.T;
            double v = a.V * b.V + c.V * d.V;
            if (t == null) return new D(v);
            if (a.T == null && c.T == null) return Lin2(t, v, a.V, b, c.V, d);
            if (b.T == null && d.T == null) return Lin2(t, v, b.V, a, d.V, c);
            int o = t.Alloc();
            var buf = t.Buf;
            int n = t.N;
            double ca = b.V, cb = a.V, cc = d.V, cd = c.V;
            Accum(buf, o, n, ca, a, true);
            Accum(buf, o, n, cb, b, false);
            Accum(buf, o, n, cc, c, false);
            Accum(buf, o, n, cd, d, false);
            return new D(t, v, o);
        }

        /// <summary>a0*b0 + a1*b1 + a2*b2 in one gradient row.</summary>
        public static D Dot3(in D a0, in D b0, in D a1, in D b1, in D a2, in D b2)
        {
            var t = a0.T ?? b0.T ?? a1.T ?? b1.T ?? a2.T ?? b2.T;
            double v = a0.V * b0.V + a1.V * b1.V + a2.V * b2.V;
            if (t == null) return new D(v);
            int o = t.Alloc();
            var buf = t.Buf;
            int n = t.N;
            Array.Clear(buf, o, n);
            Accum(buf, o, n, b0.V, a0, false);
            Accum(buf, o, n, a0.V, b0, false);
            Accum(buf, o, n, b1.V, a1, false);
            Accum(buf, o, n, a1.V, b1, false);
            Accum(buf, o, n, b2.V, a2, false);
            Accum(buf, o, n, a2.V, b2, false);
            return new D(t, v, o);
        }

        /// <summary>c0*a0 + c1*a1 + c2*a2 (+ k) with constant coefficients, in one gradient row.</summary>
        public static D Comb(double c0, in D a0, double c1, in D a1, double c2, in D a2, double k = 0.0)
        {
            var t = a0.T ?? a1.T ?? a2.T;
            double v = c0 * a0.V + c1 * a1.V + c2 * a2.V + k;
            if (t == null) return new D(v);
            return Lin3(t, v, c0, a0, c1, a1, c2, a2);
        }

        static void Accum(double[] buf, int o, int n, double c, in D a, bool first)
        {
            if (first)
            {
                if (a.T == null || c == 0.0)
                {
                    Array.Clear(buf, o, n);
                    return;
                }
                int ga = a.G;
                for (int k = 0; k < n; k++) buf[o + k] = c * buf[ga + k];
                return;
            }
            if (a.T == null || c == 0.0) return;
            int g = a.G;
            for (int k = 0; k < n; k++) buf[o + k] += c * buf[g + k];
        }

        /// <summary>f(a) with f'(a) given: the chain rule.</summary>
        public static D Chain(in D a, double fv, double dfda) => a.T == null ? new D(fv) : Lin1(a.T, fv, dfda, a);

        // ---- functions
        public static D Sqrt(D a)
        {
            double v = Math.Sqrt(a.V);
            return Chain(a, v, v > 0 ? 0.5 / v : 0.0);
        }

        public static D Sin(D a) => Chain(a, Math.Sin(a.V), Math.Cos(a.V));
        public static D Cos(D a) => Chain(a, Math.Cos(a.V), -Math.Sin(a.V));

        /// <summary>asin of a clamped to [-1, 1] (gradient 0 outside, as the clamp's).</summary>
        public static D AsinClamped(D a)
        {
            if (a.V >= 1.0) return new D(Math.PI / 2);
            if (a.V <= -1.0) return new D(-Math.PI / 2);
            return Chain(a, Math.Asin(a.V), 1.0 / Math.Sqrt(1.0 - a.V * a.V));
        }

        public static D Atan2(D y, D x)
        {
            var t = TapeOf(y, x);
            double v = Math.Atan2(y.V, x.V);
            if (t == null) return new D(v);
            double r2 = x.V * x.V + y.V * y.V;
            if (r2 <= 0) return new D(t, v, Zero(t));
            return Lin2(t, v, x.V / r2, y, -y.V / r2, x);
        }

        static int Zero(Tape t)
        {
            int o = t.Alloc();
            Array.Clear(t.Buf, o, t.N);
            return o;
        }

        public static D Abs(D a) => a.V >= 0 ? a : -a;
        public static D Max(D a, D b) => a.V >= b.V ? a : b;
        public static D Min(D a, D b) => a.V <= b.V ? a : b;

        /// <summary>max(lo, min(hi, a)), the gradient of whichever is taken.</summary>
        public static D Clamp(D a, double lo, double hi) => a.V < lo ? new D(lo) : (a.V > hi ? new D(hi) : a);

        /// <summary>b3.sp: k * logaddexp(0, x / k), numerically stable; derivative sigmoid(x / k).</summary>
        public static D Softplus(D x, double k)
        {
            double z = x.V / k;
            double v, s;
            if (z > 0)
            {
                double e = Math.Exp(-z);
                v = k * (z + Log1p(e));
                s = 1.0 / (1.0 + e);
            }
            else
            {
                double e = Math.Exp(z);
                v = k * Log1p(e);
                s = e / (1.0 + e);
            }
            return Chain(x, v, s);
        }

        /// <summary>log(1 + x) accurate for small x (netstandard2.1 has no Math.Log1P).</summary>
        public static double Log1p(double x)
        {
            double u = 1.0 + x;
            if (u == 1.0) return x;
            return Math.Log(u) * x / (u - 1.0);
        }
    }

    /// <summary>A vector of D.</summary>
    public readonly struct DV3
    {
        public readonly D X, Y, Z;

        public DV3(D x, D y, D z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public DV3(Vec3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public static implicit operator DV3(Vec3 v) => new DV3(v);

        public D this[int i] => i == 0 ? X : (i == 1 ? Y : Z);
        public Vec3 V => new Vec3(X.V, Y.V, Z.V);

        public static DV3 operator +(DV3 a, DV3 b) => new DV3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static DV3 operator -(DV3 a, DV3 b) => new DV3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static DV3 operator -(DV3 a) => new DV3(-a.X, -a.Y, -a.Z);
        public static DV3 operator *(DV3 a, double s) => new DV3(a.X * s, a.Y * s, a.Z * s);
        public static DV3 operator *(double s, DV3 a) => new DV3(a.X * s, a.Y * s, a.Z * s);
        public static DV3 operator *(DV3 a, D s) => new DV3(a.X * s, a.Y * s, a.Z * s);
        public static DV3 operator *(D s, DV3 a) => new DV3(a.X * s, a.Y * s, a.Z * s);
        public static DV3 operator /(DV3 a, D s) => a * (1.0 / s);
        public static DV3 operator +(DV3 a, Vec3 b) => new DV3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static DV3 operator -(DV3 a, Vec3 b) => new DV3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static D Dot(in DV3 a, in DV3 b) => D.Dot3(a.X, b.X, a.Y, b.Y, a.Z, b.Z);
        public static D Dot(in DV3 a, Vec3 b) => D.Comb(b.X, a.X, b.Y, a.Y, b.Z, a.Z);

        public static DV3 Cross(in DV3 a, in DV3 b) => new DV3(
            D.MulAdd(a.Y, b.Z, -a.Z, b.Y),
            D.MulAdd(a.Z, b.X, -a.X, b.Z),
            D.MulAdd(a.X, b.Y, -a.Y, b.X));

        public static DV3 Cross(in DV3 a, Vec3 b) => new DV3(
            D.Comb(b.Z, a.Y, -b.Y, a.Z, 0.0, default),
            D.Comb(b.X, a.Z, -b.Z, a.X, 0.0, default),
            D.Comb(b.Y, a.X, -b.X, a.Y, 0.0, default));

        public D Length() => D.Sqrt(Dot(this, this));

        /// <summary>b3.unit: v/|v|, or (1,0,0) when |v| &lt;= 1e-12.</summary>
        public DV3 Unit()
        {
            var n = Length();
            if (n.V <= 1e-12) return new DV3(Vec3.UnitX);
            return new DV3(X / n, Y / n, Z / n);
        }

        /// <summary>a + s*b (s constant) per component in one row each.</summary>
        public static DV3 AddScaled(in DV3 a, double s, in DV3 b) => new DV3(
            D.Comb(1.0, a.X, s, b.X, 0.0, default),
            D.Comb(1.0, a.Y, s, b.Y, 0.0, default),
            D.Comb(1.0, a.Z, s, b.Z, 0.0, default));

        public static DV3 Lerp(in DV3 a, in DV3 b, double t) => new DV3(
            D.Comb(1.0 - t, a.X, t, b.X, 0.0, default),
            D.Comb(1.0 - t, a.Y, t, b.Y, 0.0, default),
            D.Comb(1.0 - t, a.Z, t, b.Z, 0.0, default));
    }

    /// <summary>A rotation (or any 3x3) of D, row-major. Columns are the rotated frame's axes.</summary>
    public readonly struct DM3
    {
        public readonly D M00, M01, M02, M10, M11, M12, M20, M21, M22;

        public DM3(D m00, D m01, D m02, D m10, D m11, D m12, D m20, D m21, D m22)
        {
            M00 = m00; M01 = m01; M02 = m02;
            M10 = m10; M11 = m11; M12 = m12;
            M20 = m20; M21 = m21; M22 = m22;
        }

        public static DM3 FromColumns(in DV3 c0, in DV3 c1, in DV3 c2) =>
            new DM3(c0.X, c1.X, c2.X, c0.Y, c1.Y, c2.Y, c0.Z, c1.Z, c2.Z);

        public static readonly DM3 Identity = new DM3(1, 0, 0, 0, 1, 0, 0, 0, 1);

        public DV3 Col0 => new DV3(M00, M10, M20);
        public DV3 Col1 => new DV3(M01, M11, M21);
        public DV3 Col2 => new DV3(M02, M12, M22);
        public DV3 Column(int j) => j == 0 ? Col0 : (j == 1 ? Col1 : Col2);

        public Mat3 V => new Mat3(M00.V, M01.V, M02.V, M10.V, M11.V, M12.V, M20.V, M21.V, M22.V);

        /// <summary>this * v for a constant v.</summary>
        public DV3 Mul(Vec3 v) => new DV3(
            D.Comb(v.X, M00, v.Y, M01, v.Z, M02),
            D.Comb(v.X, M10, v.Y, M11, v.Z, M12),
            D.Comb(v.X, M20, v.Y, M21, v.Z, M22));

        /// <summary>p + this * v for a constant v.</summary>
        public DV3 Apply(in DV3 p, Vec3 v) => p + Mul(v);

        /// <summary>this * Rx(a): col1' = c col1 + s col2, col2' = -s col1 + c col2.</summary>
        public DM3 PostRotX(D a)
        {
            if (a.IsConst && a.V == 0.0) return this;
            var c = D.Cos(a);
            var s = D.Sin(a);
            var c1 = Col1;
            var c2 = Col2;
            var n1 = new DV3(D.MulAdd(c, c1.X, s, c2.X), D.MulAdd(c, c1.Y, s, c2.Y), D.MulAdd(c, c1.Z, s, c2.Z));
            var ms = -s;
            var n2 = new DV3(D.MulAdd(ms, c1.X, c, c2.X), D.MulAdd(ms, c1.Y, c, c2.Y), D.MulAdd(ms, c1.Z, c, c2.Z));
            return FromColumns(Col0, n1, n2);
        }

        /// <summary>this * Ry(a): col0' = c col0 - s col2, col2' = s col0 + c col2.</summary>
        public DM3 PostRotY(D a)
        {
            if (a.IsConst && a.V == 0.0) return this;
            var c = D.Cos(a);
            var s = D.Sin(a);
            var c0 = Col0;
            var c2 = Col2;
            var ms = -s;
            var n0 = new DV3(D.MulAdd(c, c0.X, ms, c2.X), D.MulAdd(c, c0.Y, ms, c2.Y), D.MulAdd(c, c0.Z, ms, c2.Z));
            var n2 = new DV3(D.MulAdd(s, c0.X, c, c2.X), D.MulAdd(s, c0.Y, c, c2.Y), D.MulAdd(s, c0.Z, c, c2.Z));
            return FromColumns(n0, Col1, n2);
        }

        /// <summary>this * Rz(a): col0' = c col0 + s col1, col1' = -s col0 + c col1.</summary>
        public DM3 PostRotZ(D a)
        {
            if (a.IsConst && a.V == 0.0) return this;
            var c = D.Cos(a);
            var s = D.Sin(a);
            var c0 = Col0;
            var c1 = Col1;
            var ms = -s;
            var n0 = new DV3(D.MulAdd(c, c0.X, s, c1.X), D.MulAdd(c, c0.Y, s, c1.Y), D.MulAdd(c, c0.Z, s, c1.Z));
            var n1 = new DV3(D.MulAdd(ms, c0.X, c, c1.X), D.MulAdd(ms, c0.Y, c, c1.Y), D.MulAdd(ms, c0.Z, c, c1.Z));
            return FromColumns(n0, n1, Col2);
        }
    }
}
