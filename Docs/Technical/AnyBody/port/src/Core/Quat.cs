// Engine-independent unit quaternion (double), for handing poses to an engine later. The solver itself works in Mat3.
using System;

namespace WonderGather.AnyBody
{
    /// <summary>A rotation quaternion (W + Xi + Yj + Zk), Hamilton convention, same handedness as Mat3.</summary>
    public readonly struct Quat
    {
        public readonly double W, X, Y, Z;

        public Quat(double w, double x, double y, double z)
        {
            W = w; X = x; Y = y; Z = z;
        }

        public static readonly Quat Identity = new Quat(1, 0, 0, 0);

        public static Quat AxisAngle(Vec3 axis, double angle)
        {
            var u = axis.Unit();
            double s = Math.Sin(0.5 * angle);
            return new Quat(Math.Cos(0.5 * angle), u.X * s, u.Y * s, u.Z * s);
        }

        public static Quat operator *(Quat a, Quat b) => new Quat(
            a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
            a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
            a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
            a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W);

        public Quat Normalized()
        {
            double n = Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
            return n > 0 ? new Quat(W / n, X / n, Y / n, Z / n) : Identity;
        }

        public Mat3 ToMat3()
        {
            double xx = X * X, yy = Y * Y, zz = Z * Z, xy = X * Y, xz = X * Z, yz = Y * Z, wx = W * X, wy = W * Y, wz = W * Z;
            return new Mat3(
                1 - 2 * (yy + zz), 2 * (xy - wz), 2 * (xz + wy),
                2 * (xy + wz), 1 - 2 * (xx + zz), 2 * (yz - wx),
                2 * (xz - wy), 2 * (yz + wx), 1 - 2 * (xx + yy));
        }

        public static Quat FromMat3(Mat3 m)
        {
            double tr = m.M00 + m.M11 + m.M22;
            if (tr > 0)
            {
                double s = Math.Sqrt(tr + 1.0) * 2;
                return new Quat(0.25 * s, (m.M21 - m.M12) / s, (m.M02 - m.M20) / s, (m.M10 - m.M01) / s).Normalized();
            }
            if (m.M00 > m.M11 && m.M00 > m.M22)
            {
                double s = Math.Sqrt(1.0 + m.M00 - m.M11 - m.M22) * 2;
                return new Quat((m.M21 - m.M12) / s, 0.25 * s, (m.M01 + m.M10) / s, (m.M02 + m.M20) / s).Normalized();
            }
            if (m.M11 > m.M22)
            {
                double s = Math.Sqrt(1.0 + m.M11 - m.M00 - m.M22) * 2;
                return new Quat((m.M02 - m.M20) / s, (m.M01 + m.M10) / s, 0.25 * s, (m.M12 + m.M21) / s).Normalized();
            }
            {
                double s = Math.Sqrt(1.0 + m.M22 - m.M00 - m.M11) * 2;
                return new Quat((m.M10 - m.M01) / s, (m.M02 + m.M20) / s, (m.M12 + m.M21) / s, 0.25 * s).Normalized();
            }
        }

        public Vec3 Rotate(Vec3 v) => ToMat3() * v;

        public override string ToString() => FormattableString.Invariant($"({W:G6}; {X:G6}, {Y:G6}, {Z:G6})");
    }
}
