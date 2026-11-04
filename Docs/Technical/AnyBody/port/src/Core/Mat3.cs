// Engine-independent 3x3 matrix (double), rotations as the prototype writes them (b3.py Rx, Ry, Rz).
using System;

namespace WonderGather.AnyBody
{
    /// <summary>A 3x3 matrix, row-major fields M[row][col]. A rotation's columns are the rotated frame's axes.</summary>
    public readonly struct Mat3
    {
        public readonly double M00, M01, M02, M10, M11, M12, M20, M21, M22;

        public Mat3(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22)
        {
            M00 = m00; M01 = m01; M02 = m02;
            M10 = m10; M11 = m11; M12 = m12;
            M20 = m20; M21 = m21; M22 = m22;
        }

        public static readonly Mat3 Identity = new Mat3(1, 0, 0, 0, 1, 0, 0, 0, 1);
        public static readonly Mat3 Zero = new Mat3(0, 0, 0, 0, 0, 0, 0, 0, 0);

        public static Mat3 RotX(double a)
        {
            double c = Math.Cos(a), s = Math.Sin(a);
            return new Mat3(1, 0, 0, 0, c, -s, 0, s, c);
        }

        public static Mat3 RotY(double a)
        {
            double c = Math.Cos(a), s = Math.Sin(a);
            return new Mat3(c, 0, s, 0, 1, 0, -s, 0, c);
        }

        public static Mat3 RotZ(double a)
        {
            double c = Math.Cos(a), s = Math.Sin(a);
            return new Mat3(c, -s, 0, s, c, 0, 0, 0, 1);
        }

        /// <summary>skew(r) so that skew(r) * f = r x f.</summary>
        public static Mat3 Skew(Vec3 r) => new Mat3(0, -r.Z, r.Y, r.Z, 0, -r.X, -r.Y, r.X, 0);

        public static Mat3 FromColumns(Vec3 c0, Vec3 c1, Vec3 c2) =>
            new Mat3(c0.X, c1.X, c2.X, c0.Y, c1.Y, c2.Y, c0.Z, c1.Z, c2.Z);

        public Vec3 Col0 => new Vec3(M00, M10, M20);
        public Vec3 Col1 => new Vec3(M01, M11, M21);
        public Vec3 Col2 => new Vec3(M02, M12, M22);
        public Vec3 Column(int j) => j == 0 ? Col0 : (j == 1 ? Col1 : Col2);
        public Vec3 Row(int i) => i == 0 ? new Vec3(M00, M01, M02) : (i == 1 ? new Vec3(M10, M11, M12) : new Vec3(M20, M21, M22));

        public double this[int i, int j]
        {
            get
            {
                switch (i * 3 + j)
                {
                    case 0: return M00; case 1: return M01; case 2: return M02;
                    case 3: return M10; case 4: return M11; case 5: return M12;
                    case 6: return M20; case 7: return M21; default: return M22;
                }
            }
        }

        public Mat3 Transpose() => new Mat3(M00, M10, M20, M01, M11, M21, M02, M12, M22);

        public static Vec3 operator *(Mat3 m, Vec3 v) => new Vec3(
            m.M00 * v.X + m.M01 * v.Y + m.M02 * v.Z,
            m.M10 * v.X + m.M11 * v.Y + m.M12 * v.Z,
            m.M20 * v.X + m.M21 * v.Y + m.M22 * v.Z);

        public static Mat3 operator *(Mat3 a, Mat3 b) => new Mat3(
            a.M00 * b.M00 + a.M01 * b.M10 + a.M02 * b.M20,
            a.M00 * b.M01 + a.M01 * b.M11 + a.M02 * b.M21,
            a.M00 * b.M02 + a.M01 * b.M12 + a.M02 * b.M22,
            a.M10 * b.M00 + a.M11 * b.M10 + a.M12 * b.M20,
            a.M10 * b.M01 + a.M11 * b.M11 + a.M12 * b.M21,
            a.M10 * b.M02 + a.M11 * b.M12 + a.M12 * b.M22,
            a.M20 * b.M00 + a.M21 * b.M10 + a.M22 * b.M20,
            a.M20 * b.M01 + a.M21 * b.M11 + a.M22 * b.M21,
            a.M20 * b.M02 + a.M21 * b.M12 + a.M22 * b.M22);

        public static Mat3 operator +(Mat3 a, Mat3 b) => new Mat3(
            a.M00 + b.M00, a.M01 + b.M01, a.M02 + b.M02, a.M10 + b.M10, a.M11 + b.M11, a.M12 + b.M12,
            a.M20 + b.M20, a.M21 + b.M21, a.M22 + b.M22);

        public static Mat3 operator *(Mat3 a, double s) => new Mat3(
            a.M00 * s, a.M01 * s, a.M02 * s, a.M10 * s, a.M11 * s, a.M12 * s, a.M20 * s, a.M21 * s, a.M22 * s);

        public override string ToString() => FormattableString.Invariant(
            $"[[{M00:G5}, {M01:G5}, {M02:G5}], [{M10:G5}, {M11:G5}, {M12:G5}], [{M20:G5}, {M21:G5}, {M22:G5}]]");
    }
}
