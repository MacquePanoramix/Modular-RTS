// b3.fk: the 15 links' frames, points, centres, joints and solids from the pose vector, in D (values and gradients).
using System;

namespace WonderGather.AnyBody
{
    /// <summary>The named points of b3.fk's K['pts'].</summary>
    public enum Pt
    {
        P, Waist, Neck, TrunkC, HeadC,
        ShL, ElL, WrL, HjL, KnL, AnL, BallL,
        ShR, ElR, WrR, HjR, KnR, AnR, BallR
    }

    /// <summary>One joint of K['joints'], in D.</summary>
    public struct KinJoint
    {
        public string Name;
        public DV3 Origin;
        public bool IsHinge;
        public DV3 Axis;
        public int SubtreeMask;
        public double Capacity;
    }

    /// <summary>The result of b3.fk (named points, link rotations, centres, joints, solids) for one pose.</summary>
    public sealed class Kin
    {
        public const int NPts = 19;
        public readonly DV3[] Pts = new DV3[NPts];

        /// <summary>Link rotations by BodyLinks index.</summary>
        public readonly DM3[] R = new DM3[BodyLinks.Count];

        /// <summary>Link centres by BodyLinks index, and their masses.</summary>
        public readonly DV3[] LinkCom = new DV3[BodyLinks.Count];
        public readonly double[] LinkMass = new double[BodyLinks.Count];

        /// <summary>K['M'] (summed in b3's coms order) and K['com'].</summary>
        public double M;
        public DV3 Com;

        /// <summary>K['joints'] order: back, neck, then L, R: shoulder, elbow, hip, knee, ankle, toe.</summary>
        public readonly KinJoint[] Joints = new KinJoint[14];

        /// <summary>The lowest z of each solid (b3.lowest order: 10 capsules, the head sphere, 4 boxes) and its link.</summary>
        public readonly D[] LowestZ = new D[15];
        public readonly int[] LowestLink = new int[15];

        /// <summary>The capsules' end points (b3 K['caps'] order) and radii, kept for the 'chest' support.</summary>
        public readonly DV3[] CapA = new DV3[10], CapC = new DV3[10];
        public readonly double[] CapR = new double[10];
        public readonly int[] CapLink = new int[10];

        public DV3 this[Pt p] => Pts[(int)p];

        public static Pt Point(string name)
        {
            switch (name)
            {
                case "P": return Pt.P;
                case "waist": return Pt.Waist;
                case "neck": return Pt.Neck;
                case "trunkC": return Pt.TrunkC;
                case "headC": return Pt.HeadC;
                case "shL": return Pt.ShL;
                case "elL": return Pt.ElL;
                case "wrL": return Pt.WrL;
                case "hjL": return Pt.HjL;
                case "knL": return Pt.KnL;
                case "anL": return Pt.AnL;
                case "ballL": return Pt.BallL;
                case "shR": return Pt.ShR;
                case "elR": return Pt.ElR;
                case "wrR": return Pt.WrR;
                case "hjR": return Pt.HjR;
                case "knR": return Pt.KnR;
                case "anR": return Pt.AnR;
                case "ballR": return Pt.BallR;
                default: throw new ArgumentException("unknown point " + name);
            }
        }

        public static string PointName(Pt p) => PointNames[(int)p];

        public static readonly string[] PointNames =
        {
            "P", "waist", "neck", "trunkC", "headC", "shL", "elL", "wrL", "hjL", "knL", "anL", "ballL",
            "shR", "elR", "wrR", "hjR", "knR", "anR", "ballR"
        };

        public static Pt Side(Pt left, int side) => side == 0 ? left : (Pt)((int)left + 7);
    }

    public static class Kinematics
    {
        /// <summary>b3.fk(b, q). q holds the 34 VARS as D (fixed ones constant). Fills k.</summary>
        public static void Forward(BodyModel b, D[] q, Kin k)
        {
            var P = new DV3(q[Pose.X], q[Pose.Y], q[Pose.Z]);
            var RH = DM3.Identity.PostRotZ(q[Pose.Yaw]).PostRotY(q[Pose.Pitch]).PostRotX(q[Pose.Roll]);
            var RT = RH.PostRotY(q[Pose.TFlex]).PostRotX(q[Pose.TLat]).PostRotZ(q[Pose.TTw]);
            var RN = RT.PostRotY(q[Pose.NFlex]).PostRotX(q[Pose.NLat]).PostRotZ(q[Pose.NRot]);
            var waist = P + RH.Mul(new Vec3(0, 0, b.WaistHeight));
            var neck = waist + RT.Mul(new Vec3(0, 0, b.TrunkLength));
            var trunkC = waist + RT.Mul(new Vec3(0, 0, b.TrunkCentre));
            var headC = neck + RN.Mul(new Vec3(0, 0, b.NeckLength));
            k.Pts[(int)Pt.P] = P;
            k.Pts[(int)Pt.Waist] = waist;
            k.Pts[(int)Pt.Neck] = neck;
            k.Pts[(int)Pt.TrunkC] = trunkC;
            k.Pts[(int)Pt.HeadC] = headC;
            k.R[BodyLinks.Hips] = RH;
            k.R[BodyLinks.Trunk] = RT;
            k.R[BodyLinks.Head] = RN;
            // each link's centre from the body's data (its own frame, from its origin): b3.fk's P, trunkC, headC for the
            // factory's bodies; the measured centres when the game sets them
            k.LinkCom[BodyLinks.Hips] = P + RH.Mul(b.LinkCentre[BodyLinks.Hips]);
            k.LinkCom[BodyLinks.Trunk] = waist + RT.Mul(b.LinkCentre[BodyLinks.Trunk]);
            k.LinkCom[BodyLinks.Head] = neck + RN.Mul(b.LinkCentre[BodyLinks.Head]);
            k.LinkMass[BodyLinks.Hips] = b.MassHips;
            k.LinkMass[BodyLinks.Trunk] = b.MassTrunk;
            k.LinkMass[BodyLinks.Head] = b.MassHead;
            k.Joints[0] = new KinJoint { Name = "back", Origin = waist, IsHinge = false, SubtreeMask = BackMask, Capacity = b.CapBack };
            k.Joints[1] = new KinJoint { Name = "neck", Origin = neck, IsHinge = false, SubtreeMask = NeckMask, Capacity = b.CapNeck };

            double hs = Math.Max(0.0, b.TrunkCentre - b.TrunkRadius / 2);
            var hipW = RH.Mul(new Vec3(0, b.HipHalfWidth, 0));
            SetCap(k, 0, BodyLinks.Hips, P + hipW, P - hipW, b.HipsRadius);
            var tz = RT.Mul(new Vec3(0, 0, hs));
            SetCap(k, 1, BodyLinks.Trunk, trunkC + tz, trunkC - tz, b.TrunkRadius);

            for (int s = 0; s < 2; s++)
            {
                double sg = s == 0 ? 1.0 : -1.0;
                int o4 = 4 * s, o2 = 2 * s;
                int ua = s == 0 ? BodyLinks.UaL : BodyLinks.UaR, fa = ua + 1;
                int th = s == 0 ? BodyLinks.ThL : BodyLinks.ThR, sh = th + 1, ft = th + 2, to = th + 3;
                var shp = waist + RT.Mul(new Vec3(b.ShoulderForward, sg * b.ShoulderHalfWidth, b.TrunkLength));
                var RU = RT.PostRotY(-q[Pose.ShFlexL + o4]).PostRotX(Sg(sg, q[Pose.ShAbdL + o4])).PostRotZ(Sg(sg, q[Pose.ShRotL + o4]));
                var el = shp + RU.Mul(new Vec3(0, 0, -b.UpperArmLength));
                var RF = RU.PostRotY(-q[Pose.ElbL + o4]);
                var wr = el + RF.Mul(new Vec3(0, 0, -b.ForearmLength));
                var hj = P + RH.Mul(new Vec3(0, sg * b.HipHalfWidth, 0));
                var RTh = RH.PostRotY(-q[Pose.HFlexL + o4]).PostRotX(Sg(sg, q[Pose.HAbdL + o4])).PostRotZ(Sg(sg, q[Pose.HRotL + o4]));
                var kn = hj + RTh.Mul(new Vec3(0, 0, -b.ThighLength));
                var RS = RTh.PostRotY(q[Pose.KneeL + o4]);
                var an = kn + RS.Mul(new Vec3(0, 0, -b.ShinLength));
                var RFt = RS.PostRotY(-q[Pose.DorsiL + o2]).PostRotX(Sg(sg, q[Pose.InvL + o2]));
                var ball = an + RFt.Mul(new Vec3(b.BallLength, 0, -b.AnkleHeight));
                var RTo = RFt.PostRotY(-q[Pose.ToeL + s]);
                k.R[ua] = RU;
                k.R[fa] = RF;
                k.R[th] = RTh;
                k.R[sh] = RS;
                k.R[ft] = RFt;
                k.R[to] = RTo;
                k.Pts[(int)Kin.Side(Pt.ShL, s)] = shp;
                k.Pts[(int)Kin.Side(Pt.ElL, s)] = el;
                k.Pts[(int)Kin.Side(Pt.WrL, s)] = wr;
                k.Pts[(int)Kin.Side(Pt.HjL, s)] = hj;
                k.Pts[(int)Kin.Side(Pt.KnL, s)] = kn;
                k.Pts[(int)Kin.Side(Pt.AnL, s)] = an;
                k.Pts[(int)Kin.Side(Pt.BallL, s)] = ball;
                // centres from the body's data (b3.fk: the segments' midpoints, the foot's and toes' box centres)
                k.LinkCom[ua] = shp + RU.Mul(b.LinkCentre[ua]);
                k.LinkCom[fa] = el + RF.Mul(b.LinkCentre[fa]);
                k.LinkCom[th] = hj + RTh.Mul(b.LinkCentre[th]);
                k.LinkCom[sh] = kn + RS.Mul(b.LinkCentre[sh]);
                k.LinkCom[ft] = an + RFt.Mul(b.LinkCentre[ft]);
                k.LinkCom[to] = ball + RTo.Mul(b.LinkCentre[to]);
                k.LinkMass[ua] = b.MassUpperArm[s];
                k.LinkMass[fa] = b.MassForearm[s];
                k.LinkMass[th] = b.MassThigh[s];
                k.LinkMass[sh] = b.MassShin[s];
                k.LinkMass[ft] = b.MassFoot[s];
                k.LinkMass[to] = b.MassToes[s];
                // names and subtree masks are constants (review, Nov 4: built per call, they were ~1 kB of garbage per evaluation)
                int j0 = 2 + 6 * s;
                k.Joints[j0] = new KinJoint { Name = JointNames[j0], Origin = shp, SubtreeMask = SideMasks[6 * s], Capacity = b.CapShoulder[s] };
                k.Joints[j0 + 1] = new KinJoint
                {
                    Name = JointNames[j0 + 1], Origin = el, IsHinge = true, Axis = RU.Col1, SubtreeMask = SideMasks[6 * s + 1], Capacity = b.CapElbow[s]
                };
                k.Joints[j0 + 2] = new KinJoint { Name = JointNames[j0 + 2], Origin = hj, SubtreeMask = SideMasks[6 * s + 2], Capacity = b.CapHip[s] };
                k.Joints[j0 + 3] = new KinJoint
                {
                    Name = JointNames[j0 + 3], Origin = kn, IsHinge = true, Axis = RTh.Col1, SubtreeMask = SideMasks[6 * s + 3], Capacity = b.CapKnee[s]
                };
                k.Joints[j0 + 4] = new KinJoint { Name = JointNames[j0 + 4], Origin = an, SubtreeMask = SideMasks[6 * s + 4], Capacity = b.CapAnkle[s] };
                k.Joints[j0 + 5] = new KinJoint
                {
                    Name = JointNames[j0 + 5], Origin = ball, IsHinge = true, Axis = RFt.Col1, SubtreeMask = SideMasks[6 * s + 5], Capacity = b.CapToe[s]
                };
                // capsules: ua, fa, th, sh
                int c0 = 2 + 4 * s;
                var du = (el - shp).Unit();
                double eu = Math.Min(b.ArmRadius, 0.5 * b.UpperArmLength);
                SetCap(k, c0, ua, DV3.AddScaled(shp, eu, du), DV3.AddScaled(el, -eu, du), b.ArmRadius);
                SetCap(k, c0 + 1, fa, el, wr, b.ArmRadius);
                var dt = (kn - hj).Unit();
                double et = Math.Min(b.ThighRadius, 0.5 * b.ThighLength);
                SetCap(k, c0 + 2, th, DV3.AddScaled(hj, et, dt), DV3.AddScaled(kn, -et, dt), b.ThighRadius);
                var ds = (an - kn).Unit();
                double es = Math.Min(b.ShinRadius, 0.5 * b.ShinLength);
                SetCap(k, c0 + 3, sh, DV3.AddScaled(kn, es, ds), DV3.AddScaled(an, -es, ds), b.ShinRadius);
                // boxes: the foot's and the toes' lowest corner (only z is needed)
                k.LowestZ[11 + 2 * s] = BoxLowZ(an, RFt, -b.HeelLength, b.BallLength, b.FootHalfWidth, -b.AnkleHeight, -b.AnkleHeight + b.FootHeight);
                k.LowestLink[11 + 2 * s] = ft;
                k.LowestZ[12 + 2 * s] = BoxLowZ(ball, RTo, 0.0, b.ToeLength, b.ToeHalfWidth, 0.0, b.ToeHeight);
                k.LowestLink[12 + 2 * s] = to;
            }
            // the head sphere
            k.LowestZ[10] = headC.Z - b.HeadRadius;
            k.LowestLink[10] = BodyLinks.Head;
            // capsules' lowest points
            for (int c = 0; c < 10; c++)
            {
                k.LowestZ[c] = D.Min(k.CapA[c].Z, k.CapC[c].Z) - k.CapR[c];
                k.LowestLink[c] = k.CapLink[c];
            }
            // mass and centre, summed in b3's coms order: hips, trunk, head, then per side ua, fa, th, sh, ft, to
            double M = 0.0;
            for (int i = 0; i < ComOrder.Length; i++) M += k.LinkMass[ComOrder[i]];
            k.M = M;
            D cx = 0.0, cy = 0.0, cz = 0.0;
            for (int i = 0; i < ComOrder.Length; i++)
            {
                int l = ComOrder[i];
                double m = k.LinkMass[l];
                var c = k.LinkCom[l];
                cx = D.Comb(1.0, cx, m, c.X, 0.0, default);
                cy = D.Comb(1.0, cy, m, c.Y, 0.0, default);
                cz = D.Comb(1.0, cz, m, c.Z, 0.0, default);
            }
            k.Com = new DV3(cx / M, cy / M, cz / M);
        }

        /// <summary>b3's K['coms'] order.</summary>
        public static readonly int[] ComOrder =
        {
            BodyLinks.Hips, BodyLinks.Trunk, BodyLinks.Head,
            BodyLinks.UaL, BodyLinks.FaL, BodyLinks.ThL, BodyLinks.ShL, BodyLinks.FtL, BodyLinks.ToL,
            BodyLinks.UaR, BodyLinks.FaR, BodyLinks.ThR, BodyLinks.ShR, BodyLinks.FtR, BodyLinks.ToR
        };

        static D Sg(double sg, D v) => sg > 0 ? v : -v;

        static int Mask(params int[] links)
        {
            int m = 0;
            foreach (var l in links) m |= 1 << l;
            return m;
        }

        /// <summary>K['joints'] names, in order.</summary>
        static readonly string[] JointNames =
        {
            "back", "neck", "shoulderL", "elbowL", "hipL", "kneeL", "ankleL", "toeL", "shoulderR", "elbowR", "hipR", "kneeR", "ankleR", "toeR"
        };

        static readonly int BackMask = Mask(BodyLinks.Trunk, BodyLinks.Head, BodyLinks.UaL, BodyLinks.FaL, BodyLinks.UaR, BodyLinks.FaR);
        static readonly int NeckMask = Mask(BodyLinks.Head);

        /// <summary>Per side (L then R): the subtrees of shoulder, elbow, hip, knee, ankle, toe.</summary>
        static readonly int[] SideMasks =
        {
            Mask(BodyLinks.UaL, BodyLinks.FaL), Mask(BodyLinks.FaL), Mask(BodyLinks.ThL, BodyLinks.ShL, BodyLinks.FtL, BodyLinks.ToL),
            Mask(BodyLinks.ShL, BodyLinks.FtL, BodyLinks.ToL), Mask(BodyLinks.FtL, BodyLinks.ToL), Mask(BodyLinks.ToL),
            Mask(BodyLinks.UaR, BodyLinks.FaR), Mask(BodyLinks.FaR), Mask(BodyLinks.ThR, BodyLinks.ShR, BodyLinks.FtR, BodyLinks.ToR),
            Mask(BodyLinks.ShR, BodyLinks.FtR, BodyLinks.ToR), Mask(BodyLinks.FtR, BodyLinks.ToR), Mask(BodyLinks.ToR)
        };

        static void SetCap(Kin k, int i, int link, DV3 a, DV3 c, double r)
        {
            k.CapA[i] = a;
            k.CapC[i] = c;
            k.CapR[i] = r;
            k.CapLink[i] = link;
        }

        /// <summary>min over the 8 corners (x in {x0,x1}, y in {-hw,hw}, z in {z0,z1}) of (o + R (x,y,z)).z.</summary>
        static D BoxLowZ(DV3 o, DM3 R, double x0, double x1, double hw, double z0, double z1)
        {
            D best = default;
            bool first = true;
            for (int ix = 0; ix < 2; ix++)
            {
                double x = ix == 0 ? x0 : x1;
                for (int iy = 0; iy < 2; iy++)
                {
                    double y = iy == 0 ? -hw : hw;
                    for (int iz = 0; iz < 2; iz++)
                    {
                        double z = iz == 0 ? z0 : z1;
                        // value first, so only the minimum gets a gradient row
                        double v = o.Z.V + (R.M20.V * x + R.M21.V * y + R.M22.V * z);
                        if (first || v < best.V)
                        {
                            best = o.Z + D.Comb(x, R.M20, y, R.M21, z, R.M22);
                            first = false;
                        }
                    }
                }
            }
            return best;
        }

        /// <summary>The pose vector as D: the free variables (free[k] = pose index) become tape variables k.</summary>
        public static D[] Lift(double[] q, int[] free, Tape tape, D[] dst = null)
        {
            dst = dst ?? new D[Pose.Count];
            for (int i = 0; i < Pose.Count; i++) dst[i] = q[i];
            if (tape != null && tape.N > 0)
                for (int k = 0; k < free.Length; k++) dst[free[k]] = tape.Var(q[free[k]], k);
            return dst;
        }
    }
}
