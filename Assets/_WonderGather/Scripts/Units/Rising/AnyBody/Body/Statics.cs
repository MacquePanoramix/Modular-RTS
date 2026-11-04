// The hand-over to the reserve LP: a fitted pose and its supports as a StaticsProblem (Core), plus the fit's
// geometric read-outs that run3.job reports (contact error, penetration, base margin, the let-go margin).
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody
{
    public sealed class FitReport
    {
        public double ContactErrMm;     // run3: max |z| of the ground contacts, mm
        public double PenetrationMm;    // run3: deepest solid below the ground (not a contact link, not skip_clear), mm
        public double BaseMargin;       // run3 sp_margin: poly_margin of the ground points around the COM, m (-1 if none)
        public double? ReleaseMargin;   // run3 release_sp_margin, when the family has a let-go rule
        public Vec3 Com;
        public double Cost;
    }

    public static class Statics
    {
        /// <summary>The LP input for pose q (34, radians/metres) with the given supports (b3 fk + all_contacts +
        /// the joints' subtree masses and moments). keep: optional filter on the supports (a let-go).</summary>
        public static StaticsProblem Build(BodyModel b, double[] q, SupportSpec[] supports, Func<SupportSpec, bool> keep = null)
        {
            var k = new Kin();
            var qd = new D[Pose.Count];
            for (int i = 0; i < Pose.Count; i++) qd[i] = q[i];
            Kinematics.Forward(b, qd, k);
            var C = new List<DContact>();
            Supports.Contacts(b, k, supports, C, keep);
            return FromKin(k, C);
        }

        public static StaticsProblem FromKin(Kin k, List<DContact> C)
        {
            var p = new StaticsProblem { Mass = k.M, Com = k.Com.V, Contacts = new ContactPoint[C.Count], Joints = new JointLoad[14] };
            for (int i = 0; i < C.Count; i++)
            {
                var c = C[i];
                p.Contacts[i] = new ContactPoint
                {
                    Point = c.P.V, Normal = c.N.V, Mu = c.Mu, Link = c.Link, Other = c.Other, Kind = c.Kind, Support = c.Support
                };
            }
            for (int j = 0; j < 14; j++)
            {
                var jt = k.Joints[j];
                double msum = 0.0;
                var S = Vec3.Zero;
                for (int l = 0; l < BodyLinks.Count; l++)
                {
                    if ((jt.SubtreeMask & (1 << l)) == 0) continue;
                    msum += k.LinkMass[l];
                    S = S + k.LinkMass[l] * k.LinkCom[l].V;
                }
                var sign = new sbyte[C.Count];
                for (int i = 0; i < C.Count; i++)
                {
                    var c = C[i];
                    sign[i] = (sbyte)(((jt.SubtreeMask & (1 << c.Link)) != 0 ? 1 : 0) - (c.Other >= 0 && (jt.SubtreeMask & (1 << c.Other)) != 0 ? 1 : 0));
                }
                p.Joints[j] = new JointLoad
                {
                    Name = jt.Name, Origin = jt.Origin.V, IsHinge = jt.IsHinge, Axis = jt.IsHinge ? jt.Axis.V : Vec3.Zero,
                    Capacity = jt.Capacity, SubtreeMask = jt.SubtreeMask, SubMass = msum, SubMoment = S, ContactSign = sign
                };
            }
            return p;
        }

        /// <summary>b3.poly_margin of the ground contacts around the COM (m); -1 when nothing is on the ground.</summary>
        public static double BaseMargin(StaticsProblem p)
        {
            var w = new MarginWork();
            foreach (var c in p.Contacts)
                if (c.OnGround) w.Add(new DV3(c.Point));
            if (w.Count == 0) return -1.0;
            return Margin.PolyMargin(w, p.Com.X, p.Com.Y).V;
        }

        /// <summary>run3.RELEASE: the support kinds a family's let-go drops.</summary>
        public static SupportKind[] ReleaseKinds(string family)
        {
            switch (family)
            {
                case "bear":
                case "squat":
                case "bear_ready":
                case "squat_hands_ready":
                case "crouchkneel_ready":
                case "quad_ready_kneel":
                    return new[] { SupportKind.Hand };
                case "halfkneel":
                    return new[] { SupportKind.Hand, SupportKind.HandOnThigh };
                case "stoopknees":
                case "halfkneel_up":
                    return new[] { SupportKind.HandOnThigh };
                default:
                    return new SupportKind[0];
            }
        }

        /// <summary>run3.job's geometric read-outs for a fitted pose.</summary>
        public static FitReport Report(BodyModel b, Stance st, double[] q)
        {
            var k = new Kin();
            var qd = new D[Pose.Count];
            for (int i = 0; i < Pose.Count; i++) qd[i] = q[i];
            Kinematics.Forward(b, qd, k);
            var C = new List<DContact>();
            Supports.Contacts(b, k, st.Supports, C);
            var rep = new FitReport { Com = k.Com.V };
            double err = 0.0;
            int contactLinks = 0;
            var w = new MarginWork();
            foreach (var c in C)
            {
                contactLinks |= 1 << c.Link;
                if (!c.OnGround) continue;
                err = Math.Max(err, Math.Abs(c.P.Z.V));
                w.Add(c.P);
            }
            rep.ContactErrMm = Math.Round(1000 * err, 2);
            // run3: lowest(K, skip=skip_clear) for links not among the contacts' links (self-contacts included)
            double pen = 0.0;
            for (int s = 0; s < 15; s++)
            {
                int l = k.LowestLink[s];
                if ((st.SkipClear & (1 << l)) != 0) continue;
                if ((contactLinks & (1 << l)) != 0) continue;
                pen = Math.Min(pen, k.LowestZ[s].V);
            }
            rep.PenetrationMm = Math.Round(-1000 * pen, 1);
            rep.BaseMargin = w.Count > 0 ? Margin.PolyMargin(w, k.Com.X, k.Com.Y).V : -1.0;
            var drop = ReleaseKinds(st.Family);
            bool has = false;
            foreach (var sp in st.Supports)
                if (Array.IndexOf(drop, sp.Kind) >= 0) has = true;
            if (drop.Length > 0 && has)
            {
                var w2 = new MarginWork();
                foreach (var c in C)
                {
                    if (!c.OnGround) continue;
                    if (Array.IndexOf(drop, st.Supports[c.Support].Kind) >= 0) continue;
                    w2.Add(c.P);
                }
                rep.ReleaseMargin = Margin.PolyMargin(w2, k.Com.X, k.Com.Y).V;
            }
            return rep;
        }
    }
}
