// b3.contact_points / all_contacts: where each support touches, in D.
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody
{
    /// <summary>One support of a stance (b3's contact spec): kind, side (0 L, 1 R, -1 none), and for a hand on a thigh,
    /// which thigh and where along it (0 hip .. 1 knee).</summary>
    public struct SupportSpec
    {
        public SupportKind Kind;
        public int Side;
        public int ThighSide;
        public double Frac;

        public SupportSpec(SupportKind kind, int side)
        {
            Kind = kind;
            Side = side;
            ThighSide = -1;
            Frac = 0;
        }

        public static SupportSpec HandOnThigh(int handSide, int thighSide, double frac) =>
            new SupportSpec { Kind = SupportKind.HandOnThigh, Side = handSide, ThighSide = thighSide, Frac = frac };

        public override string ToString() =>
            Kind + (Side >= 0 ? (Side == 0 ? " L" : " R") : "") + (Kind == SupportKind.HandOnThigh ? $" on {(ThighSide == 0 ? "L" : "R")} {Frac}" : "");
    }

    /// <summary>A contact point in D.</summary>
    public struct DContact
    {
        public DV3 P;
        public DV3 N;
        public double Mu;
        public int Link;
        public int Other;
        public SupportKind Kind;
        public int Support;
        public bool OnGround => Other < 0;
    }

    public static class Supports
    {
        public const double Mu = 0.6;

        /// <summary>b3.all_contacts(b, K, specs): appends the contacts of every support, in order.</summary>
        public static void Contacts(BodyModel b, Kin k, SupportSpec[] specs, List<DContact> outp, Func<SupportSpec, bool> keep = null)
        {
            for (int si = 0; si < specs.Length; si++)
            {
                if (keep != null && !keep(specs[si])) continue;
                ContactPoints(b, k, specs[si], si, outp);
            }
        }

        static void Add(List<DContact> o, DV3 p, int link, SupportKind kind, int si) =>
            o.Add(new DContact { P = p, N = new DV3(Vec3.UnitZ), Mu = Mu, Link = link, Other = -1, Kind = kind, Support = si });

        static DV3 Down(DV3 p, double r) => new DV3(p.X, p.Y, p.Z - r);

        /// <summary>b3.contact_points(b, K, spec).</summary>
        public static void ContactPoints(BodyModel b, Kin k, SupportSpec sp, int si, List<DContact> o)
        {
            int s = sp.Side;
            switch (sp.Kind)
            {
                case SupportKind.Hand:
                    Add(o, Down(k[Kin.Side(Pt.WrL, s)], b.ArmRadius), FaLink(s), sp.Kind, si);
                    break;
                case SupportKind.Forearm:
                    Add(o, Down(k[Kin.Side(Pt.ElL, s)], b.ArmRadius), FaLink(s), sp.Kind, si);
                    Add(o, Down(k[Kin.Side(Pt.WrL, s)], b.ArmRadius), FaLink(s), sp.Kind, si);
                    break;
                case SupportKind.Shin:
                case SupportKind.Knee:
                {
                    var kn = k[Kin.Side(Pt.KnL, s)];
                    var an = k[Kin.Side(Pt.AnL, s)];
                    var ds = (an - kn).Unit();
                    double e = Math.Min(b.ShinRadius, 0.5 * b.ShinLength);
                    Add(o, Down(DV3.AddScaled(kn, e, ds), b.ShinRadius), ShLink(s), sp.Kind, si);
                    if (sp.Kind == SupportKind.Shin)
                        Add(o, Down(DV3.AddScaled(an, -e, ds), b.ShinRadius), ShLink(s), sp.Kind, si);
                    break;
                }
                case SupportKind.Calf:
                {
                    var kn = k[Kin.Side(Pt.KnL, s)];
                    var an = k[Kin.Side(Pt.AnL, s)];
                    var ds = (an - kn).Unit();
                    double e = Math.Min(b.ShinRadius, 0.5 * b.ShinLength);
                    Add(o, Down(DV3.AddScaled(an, -e, ds), b.ShinRadius), ShLink(s), sp.Kind, si);
                    break;
                }
                case SupportKind.Sole:
                {
                    var an = k[Kin.Side(Pt.AnL, s)];
                    var R = k.R[FtLink(s)];
                    for (int ix = 0; ix < 2; ix++)
                    for (int iy = 0; iy < 2; iy++)
                        Add(o, an + R.Mul(new Vec3(ix == 0 ? -b.HeelLength : b.BallLength, iy == 0 ? -b.FootHalfWidth : b.FootHalfWidth,
                            -b.AnkleHeight)), FtLink(s), sp.Kind, si);
                    break;
                }
                case SupportKind.Heel:
                case SupportKind.Ball:
                {
                    var an = k[Kin.Side(Pt.AnL, s)];
                    var R = k.R[FtLink(s)];
                    double x = sp.Kind == SupportKind.Heel ? -b.HeelLength : b.BallLength;
                    for (int iy = 0; iy < 2; iy++)
                        Add(o, an + R.Mul(new Vec3(x, iy == 0 ? -b.FootHalfWidth : b.FootHalfWidth, -b.AnkleHeight)), FtLink(s), sp.Kind, si);
                    break;
                }
                case SupportKind.Toes:
                {
                    var ball = k[Kin.Side(Pt.BallL, s)];
                    var R = k.R[ToLink(s)];
                    for (int iy = 0; iy < 2; iy++)
                        Add(o, ball + R.Mul(new Vec3(b.ToeLength, iy == 0 ? -b.ToeHalfWidth : b.ToeHalfWidth, 0)), ToLink(s), sp.Kind, si);
                    break;
                }
                case SupportKind.Seat:
                {
                    var P = k[Pt.P];
                    var R = k.R[BodyLinks.Hips];
                    for (int isg = 0; isg < 2; isg++)
                        Add(o, Down(P + R.Mul(new Vec3(0, (isg == 0 ? 1.0 : -1.0) * b.HipHalfWidth, 0)), b.HipsRadius), BodyLinks.Hips, sp.Kind, si);
                    break;
                }
                case SupportKind.ThighFront:
                {
                    var hj = k[Kin.Side(Pt.HjL, s)];
                    var kn = k[Kin.Side(Pt.KnL, s)];
                    var dt = (kn - hj).Unit();
                    Add(o, Down(DV3.AddScaled(hj, b.ThighRadius, dt), b.ThighRadius), ThLink(s), sp.Kind, si);
                    Add(o, Down(DV3.AddScaled(kn, -b.ThighRadius, dt), b.ThighRadius), ThLink(s), sp.Kind, si);
                    break;
                }
                case SupportKind.Chest:
                    for (int c = 0; c < 10; c++)
                    {
                        if (k.CapLink[c] != BodyLinks.Trunk) continue;
                        Add(o, Down(k.CapA[c], k.CapR[c]), BodyLinks.Trunk, sp.Kind, si);
                        Add(o, Down(k.CapC[c], k.CapR[c]), BodyLinks.Trunk, sp.Kind, si);
                    }
                    break;
                case SupportKind.HandOnThigh:
                {
                    int ls = sp.ThighSide;
                    var hj = k[Kin.Side(Pt.HjL, ls)];
                    var kn = k[Kin.Side(Pt.KnL, ls)];
                    var wr = k[Kin.Side(Pt.WrL, s)];
                    var ax = DV3.AddScaled(hj, sp.Frac, kn - hj);
                    var n = (wr - ax).Unit();
                    o.Add(new DContact
                    {
                        P = DV3.AddScaled(wr, -b.ArmRadius, n), N = n, Mu = 0.5, Link = FaLink(s), Other = ThLink(ls), Kind = sp.Kind,
                        Support = si
                    });
                    break;
                }
                default:
                    throw new ArgumentException("unknown support " + sp.Kind);
            }
        }

        public static int FaLink(int s) => s == 0 ? BodyLinks.FaL : BodyLinks.FaR;
        public static int ThLink(int s) => s == 0 ? BodyLinks.ThL : BodyLinks.ThR;
        public static int ShLink(int s) => s == 0 ? BodyLinks.ShL : BodyLinks.ShR;
        public static int FtLink(int s) => s == 0 ? BodyLinks.FtL : BodyLinks.FtR;
        public static int ToLink(int s) => s == 0 ? BodyLinks.ToL : BodyLinks.ToR;
    }
}
