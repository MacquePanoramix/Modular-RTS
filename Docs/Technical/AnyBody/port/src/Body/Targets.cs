// lib3.py's target functions (placement and posture bands), each appending its residuals in D.
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody
{
    /// <summary>One of a stance's targets: appends its residual(s).</summary>
    public abstract class Target
    {
        public abstract void Eval(Kin k, D[] q, List<D> r);

        public const double WPlace = 25.0; // per metre outside a placement band
        public const double WAng = 1.5;    // per radian outside an angle band
        public const double D2R = Math.PI / 180.0;
        public const double R2D = 180.0 / Math.PI;

        /// <summary>lib3.band: w * (max(0, lo - v) + max(0, v - hi)).</summary>
        public static D Band(D v, double lo, double hi, double w)
        {
            if (v.V < lo) return (lo - v) * w;
            if (v.V > hi) return (v - hi) * w;
            return new D(0.0);
        }

        /// <summary>lib3.elev: degrees above the horizontal of a unit vector's z.</summary>
        public static D ElevDeg(D z) => D.AsinClamped(z) * R2D;
    }

    /// <summary>lib3.qband: a pose variable (degrees) within [lo, hi].</summary>
    public sealed class QBand : Target
    {
        public readonly int Var;
        public readonly double Lo, Hi, W;

        public QBand(int v, double lo, double hi, double w = WAng)
        {
            Var = v;
            Lo = lo;
            Hi = hi;
            W = w;
        }

        public override void Eval(Kin k, D[] q, List<D> r) => r.Add(Band(q[Var], Lo * D2R, Hi * D2R, W));
    }

    /// <summary>lib3.head_look: the head's forward axis, degrees above the horizontal.</summary>
    public sealed class HeadLook : Target
    {
        public readonly double Lo, Hi, W;

        public HeadLook(double lo, double hi, double w = 1.0)
        {
            Lo = lo;
            Hi = hi;
            W = w;
        }

        public override void Eval(Kin k, D[] q, List<D> r) => r.Add(Band(ElevDeg(k.R[BodyLinks.Head].M20), Lo, Hi, W * D2R));
    }

    /// <summary>lib3.trunk_elev: the trunk's long axis, degrees above the horizontal.</summary>
    public sealed class TrunkElev : Target
    {
        public readonly double Lo, Hi, W;

        public TrunkElev(double lo, double hi, double w = 1.5)
        {
            Lo = lo;
            Hi = hi;
            W = w;
        }

        public override void Eval(Kin k, D[] q, List<D> r) => r.Add(Band(ElevDeg(k.R[BodyLinks.Trunk].M22), Lo, Hi, W * D2R));
    }

    /// <summary>lib3.foot_yaw: the foot's toe-out (outward positive), radians.</summary>
    public sealed class FootYaw : Target
    {
        public readonly int Side;
        public readonly double Lo, Hi, W;

        public FootYaw(int side, double lo = -0.35, double hi = 0.35, double w = 2.0)
        {
            Side = side;
            Lo = lo;
            Hi = hi;
            W = w;
        }

        public override void Eval(Kin k, D[] q, List<D> r)
        {
            var R = k.R[Side == 0 ? BodyLinks.FtL : BodyLinks.FtR];
            var yaw = D.Atan2(R.M10, R.M00);
            r.Add(Band(Side == 0 ? yaw : -yaw, Lo, Hi, W));
        }
    }

    /// <summary>lib3.pt: a point's coordinate within [lo, hi], absolute or relative to another point.</summary>
    public sealed class PtBand : Target
    {
        public readonly Pt Point;
        public readonly int Axis;
        public readonly double Lo, Hi, W;
        public readonly Pt? Rel;

        public PtBand(Pt point, int axis, double lo, double hi, Pt? rel = null, double w = WPlace)
        {
            Point = point;
            Axis = axis;
            Lo = lo;
            Hi = hi;
            Rel = rel;
            W = w;
        }

        public override void Eval(Kin k, D[] q, List<D> r)
        {
            var v = k[Point][Axis];
            if (Rel.HasValue) v = v - k[Rel.Value][Axis];
            else v = v - 0.0;
            r.Add(Band(v, Lo, Hi, W));
        }
    }

    /// <summary>lib3.rel_pts: mean of points a minus mean of points b along an axis within [lo, hi].</summary>
    public sealed class RelPts : Target
    {
        public readonly Pt[] A, B;
        public readonly int Axis;
        public readonly double Lo, Hi, W;

        public RelPts(Pt[] a, Pt[] b, int axis, double lo, double hi, double w = WPlace)
        {
            A = a;
            B = b;
            Axis = axis;
            Lo = lo;
            Hi = hi;
            W = w;
        }

        static D Mean(Kin k, Pt[] p, int axis)
        {
            // sum(...) / len: Python's sum starts from 0
            D s = 0.0;
            for (int i = 0; i < p.Length; i++) s = s + k[p[i]][axis];
            return s / p.Length;
        }

        public override void Eval(Kin k, D[] q, List<D> r) => r.Add(Band(Mean(k, A, Axis) - Mean(k, B, Axis), Lo, Hi, W));
    }

    /// <summary>lib3.sym: left matches right, w (qL - qR) for each named pair.</summary>
    public sealed class Sym : Target
    {
        public readonly int[] Left, Right;
        public readonly double W;

        public Sym(string[] names, double w = 0.6)
        {
            Left = new int[names.Length];
            Right = new int[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                Left[i] = Pose.Index(names[i] + "L");
                Right[i] = Pose.Index(names[i] + "R");
            }
            W = w;
        }

        public override void Eval(Kin k, D[] q, List<D> r)
        {
            for (int i = 0; i < Left.Length; i++) r.Add((q[Left[i]] - q[Right[i]]) * W);
        }
    }

    /// <summary>lib3.hand_on_thigh: the wrist resting on a thigh (distance, place along it, from above).</summary>
    public sealed class HandOnThigh : Target
    {
        public readonly int HandSide, ThighSide;
        public readonly double Lo, Hi, W, Touch;

        public HandOnThigh(BodyModel b, int handSide, int thighSide, double lo = 0.6, double hi = 0.95, double w = 300.0)
        {
            HandSide = handSide;
            ThighSide = thighSide;
            Lo = lo;
            Hi = hi;
            W = w;
            Touch = b.ArmRadius + b.ThighRadius;
        }

        public override void Eval(Kin k, D[] q, List<D> r)
        {
            var hj = k[Kin.Side(Pt.HjL, ThighSide)];
            var kn = k[Kin.Side(Pt.KnL, ThighSide)];
            var wr = k[Kin.Side(Pt.WrL, HandSide)];
            var d = kn - hj;
            var L = d.Length();
            var u = new DV3(d.X / L, d.Y / L, d.Z / L);
            var t = DV3.Dot(wr - hj, u) / L;
            var tc = D.Clamp(t, 0.0, 1.0);
            var ax = hj + d * tc;
            var v = wr - ax;
            var dist = v.Length();
            var den = D.Max(dist, new D(1e-9));
            var nz = v.Z / den;
            r.Add((dist - Touch) * W);
            r.Add(Band(t, Lo, Hi, 30.0));
            r.Add(Band(nz, 0.35, 1.0, 10.0));
        }
    }

    /// <summary>lib3.lifted: a point at least zmin above the ground.</summary>
    public sealed class Lifted : Target
    {
        public readonly Pt Point;
        public readonly double ZMin, W;

        public Lifted(Pt point, double zmin, double w = WPlace)
        {
            Point = point;
            ZMin = zmin;
            W = w;
        }

        public override void Eval(Kin k, D[] q, List<D> r) => r.Add(Band(k[Point].Z, ZMin, 9.0, W));
    }
}
