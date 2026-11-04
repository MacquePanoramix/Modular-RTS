// lib3.py's stance library: goals written once for no body in particular, built for one body here.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace WonderGather.AnyBody
{
    /// <summary>A stance built for one body (lib3's dict): supports, targets, start pose, the weight rule.</summary>
    public sealed class Stance
    {
        public string Name;
        /// <summary>The library family (lib3 function name) and its arguments, e.g. "squat", [0.35].</summary>
        public string Family;
        public object[] Args = new object[0];
        public SupportSpec[] Supports;
        public List<Target> Targets = new List<Target>();
        /// <summary>skip_clear: links allowed to touch the ground beyond the supports (BodyLinks mask).</summary>
        public int SkipClear;
        /// <summary>q0: start values by pose index, in degrees (metres for x, y, z).</summary>
        public Dictionary<int, double> Q0 = new Dictionary<int, double>();
        /// <summary>Fixed variables (pose index -> value in degrees/metres); x, y, yaw are always fixed at 0 unless given.</summary>
        public Dictionary<int, double> Fixed = new Dictionary<int, double>();
        /// <summary>Extra bounds by pose index (degrees/metres), intersected with the body's ranges.</summary>
        public Dictionary<int, (double Lo, double Hi)> Bounds = new Dictionary<int, (double, double)>();
        /// <summary>m_req: the base margin wanted (m). NaN: the default 0.03 H.</summary>
        public double MReq = double.NaN;
        /// <summary>ready: the support kinds it must be able to let go of.</summary>
        public SupportKind[] Ready = new SupportKind[0];
        /// <summary>m_ready (m). NaN: the default 0.02 H.</summary>
        public double MReady = double.NaN;
        public double Effort = 1.0;
        /// <summary>The prototype's max_nfev.</summary>
        public int Nfev = 600;

        public Stance Clone()
        {
            var s = (Stance)MemberwiseClone();
            s.Targets = new List<Target>(Targets);
            s.Q0 = new Dictionary<int, double>(Q0);
            s.Fixed = new Dictionary<int, double>(Fixed);
            s.Bounds = new Dictionary<int, (double, double)>(Bounds);
            s.Supports = (SupportSpec[])Supports.Clone();
            return s;
        }

        public override string ToString() => Name;
    }

    /// <summary>The lib3.py families. Side arguments: 0 = L, 1 = R.</summary>
    public static class StanceLibrary
    {
        static SupportSpec S(SupportKind k, int side) => new SupportSpec(k, side);
        static SupportSpec S(SupportKind k) => new SupportSpec(k, -1);

        static int Mask(params string[] links)
        {
            int m = 0;
            foreach (var l in links) m |= 1 << BodyLinks.Index(l);
            return m;
        }

        static string LR(int s) => s == 0 ? "L" : "R";
        static Pt P(string n) => Kin.Point(n);
        static int V(string n) => Pose.Index(n);

        static Dictionary<int, double> Q(params (string, double)[] kv)
        {
            var d = new Dictionary<int, double>();
            foreach (var (n, v) in kv) d[V(n)] = v;
            return d;
        }

        static string F2(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);

        static readonly string[] SymBase = { "shflex", "elb", "hflex", "knee", "dorsi" };

        public static Stance Stand(BodyModel b)
        {
            double wH = b.HipHalfWidth, H = b.HipHeight;
            return new Stance
            {
                Name = "stand", Family = "stand",
                Supports = new[] { S(SupportKind.Sole, 0), S(SupportKind.Sole, 1) },
                Targets = new List<Target>
                {
                    new PtBand(Pt.AnL, 1, 0.8 * wH, 1.3 * wH), new PtBand(Pt.AnR, 1, -1.3 * wH, -0.8 * wH),
                    new RelPts(new[] { Pt.AnL }, new[] { Pt.AnR }, 0, -0.02 * H, 0.02 * H), new FootYaw(0, -0.1, 0.25),
                    new FootYaw(1, -0.1, 0.25), new TrunkElev(85, 90), new HeadLook(-10, 5), new QBand(V("shflexL"), -10, 20),
                    new QBand(V("shflexR"), -10, 20), new QBand(V("elbL"), 0, 30), new QBand(V("elbR"), 0, 30), new Sym(SymBase)
                },
                Q0 = Q(("z", H), ("hflexL", 5), ("hflexR", 5), ("kneeL", 8), ("kneeR", 8), ("dorsiL", 3), ("dorsiR", 3), ("elbL", 10),
                    ("elbR", 10)),
                MReq = 0.25 * b.FootUnit
            };
        }

        public static Stance Quad(BodyModel b, bool toes = false)
        {
            double lA = b.ArmUnit, lT = b.ThighLength, wS = b.ShoulderHalfWidth, wH = b.HipHalfWidth;
            var C = new List<SupportSpec> { S(SupportKind.Hand, 0), S(SupportKind.Hand, 1) };
            if (toes) C.AddRange(new[] { S(SupportKind.Knee, 0), S(SupportKind.Knee, 1), S(SupportKind.Toes, 0), S(SupportKind.Toes, 1) });
            else C.AddRange(new[] { S(SupportKind.Shin, 0), S(SupportKind.Shin, 1) });
            var T = new List<Target>
            {
                new PtBand(Pt.WrL, 0, -0.02 * lA, 0.15 * lA, Pt.ShL), new PtBand(Pt.WrR, 0, -0.02 * lA, 0.15 * lA, Pt.ShR),
                new PtBand(Pt.WrL, 1, wS - 0.05 * lA, wS + 0.2 * lA), new PtBand(Pt.WrR, 1, -wS - 0.2 * lA, -wS + 0.05 * lA),
                new PtBand(Pt.KnL, 0, -0.15 * lT, 0.10 * lT, Pt.HjL), new PtBand(Pt.KnR, 0, -0.15 * lT, 0.10 * lT, Pt.HjR),
                new PtBand(Pt.KnL, 1, wH, 2.0 * wH), new PtBand(Pt.KnR, 1, -2.0 * wH, -wH),
                new QBand(V("tflex"), -15, 15), new HeadLook(-60, -15), new QBand(V("elbL"), 0, 25), new QBand(V("elbR"), 0, 25),
                new Sym(SymBase)
            };
            if (!toes) T.AddRange(new Target[] { new QBand(V("dorsiL"), -50, -25), new QBand(V("dorsiR"), -50, -25) });
            return new Stance
            {
                Name = "hands and knees" + (toes ? " (toes tucked)" : ""), Family = "quad", Args = toes ? new object[] { true } : new object[0],
                Supports = C.ToArray(), Targets = T,
                SkipClear = toes ? 0 : Mask("ftL", "toL", "ftR", "toR"),
                Q0 = Q(("z", b.ThighLength + b.ShinRadius), ("pitch", 75), ("tflex", 0), ("nflex", -20), ("shflexL", 80), ("shflexR", 80),
                    ("elbL", 5), ("elbR", 5), ("hflexL", 80), ("hflexR", 80), ("kneeL", 90), ("kneeR", 90), ("dorsiL", -40), ("dorsiR", -40),
                    ("toeL", toes ? 40 : 0), ("toeR", toes ? 40 : 0)),
                MReq = 0.04 * b.HipHeight
            };
        }

        public static Stance Tripod(BodyModel b, int lift = 0)
        {
            int on = 1 - lift;
            var st = Quad(b);
            st.Name = string.Format("tripod (hands and one knee, {0} knee lifted)", lift == 0 ? "left" : "right");
            st.Family = "tripod";
            st.Args = new object[] { LR(lift) };
            st.Supports = new[] { S(SupportKind.Hand, 0), S(SupportKind.Hand, 1), S(SupportKind.Shin, on) };
            st.Targets.Add(new Lifted(P("kn" + LR(lift)), 0.04 * b.HipHeight));
            st.Targets.Add(new Lifted(P("an" + LR(lift)), 0.03 * b.HipHeight));
            st.MReq = 0.03 * b.HipHeight;
            return st;
        }

        public static Stance Bear(BodyModel b, double Dd, bool toes = false)
        {
            double lA = b.ArmUnit, lL = b.LegUnit, wS = b.ShoulderHalfWidth, wH = b.HipHalfWidth, H = b.HipHeight;
            var C = new List<SupportSpec> { S(SupportKind.Hand, 0), S(SupportKind.Hand, 1) };
            if (toes)
                C.AddRange(new[] { S(SupportKind.Ball, 0), S(SupportKind.Toes, 0), S(SupportKind.Ball, 1), S(SupportKind.Toes, 1) });
            else C.AddRange(new[] { S(SupportKind.Sole, 0), S(SupportKind.Sole, 1) });
            var T = new List<Target>
            {
                new PtBand(Pt.AnL, 1, wH, 2.2 * wH), new PtBand(Pt.AnR, 1, -2.2 * wH, -wH),
                new RelPts(new[] { Pt.AnL }, new[] { Pt.AnR }, 0, -0.03 * H, 0.03 * H), new FootYaw(0, -0.15, 0.3), new FootYaw(1, -0.15, 0.3),
                new RelPts(new[] { Pt.WrL, Pt.WrR }, new[] { Pt.BallL, Pt.BallR }, 0, (Dd - 0.04) * lL, (Dd + 0.04) * lL, 40.0),
                new PtBand(Pt.WrL, 1, wS - 0.1 * lA, wS + 0.2 * lA), new PtBand(Pt.WrR, 1, -wS - 0.2 * lA, -wS + 0.1 * lA),
                new QBand(V("nflex"), -25, 35), new QBand(V("elbL"), 0, 20), new QBand(V("elbR"), 0, 20), new Sym(SymBase)
            };
            return new Stance
            {
                Name = "hands and feet D=" + F2(Dd) + (toes ? " toes" : ""), Family = "bear",
                Args = toes ? new object[] { Dd, true } : new object[] { Dd },
                Supports = C.ToArray(), Targets = T,
                Q0 = Q(("z", 0.75 * lL), ("pitch", 100), ("tflex", 10), ("shflexL", 140), ("shflexR", 140), ("elbL", 5), ("elbR", 5),
                    ("hflexL", 100), ("hflexR", 100), ("kneeL", 25), ("kneeR", 25), ("dorsiL", !toes ? 15 : -25), ("dorsiR", !toes ? 15 : -25),
                    ("toeL", toes ? 40 : 0), ("toeR", toes ? 40 : 0)),
                MReq = 0.04 * H
            };
        }

        public static Stance Squat(BodyModel b, double h, bool toes = false, bool hands = false)
        {
            double lA = b.ArmUnit, H = b.HipHeight, wH = b.HipHalfWidth, wS = b.ShoulderHalfWidth;
            var C = new List<SupportSpec>();
            if (toes)
                C.AddRange(new[] { S(SupportKind.Ball, 0), S(SupportKind.Toes, 0), S(SupportKind.Ball, 1), S(SupportKind.Toes, 1) });
            else C.AddRange(new[] { S(SupportKind.Sole, 0), S(SupportKind.Sole, 1) });
            var T = new List<Target>
            {
                new PtBand(Pt.P, 2, h * H - 0.004 * H, h * H + 0.004 * H, null, 80.0),
                new PtBand(Pt.AnL, 1, wH, 2.4 * wH), new PtBand(Pt.AnR, 1, -2.4 * wH, -wH),
                new RelPts(new[] { Pt.AnL }, new[] { Pt.AnR }, 0, -0.03 * H, 0.03 * H), new FootYaw(0, -0.1, 0.35), new FootYaw(1, -0.1, 0.35),
                new HeadLook(-35, 10), new Sym(new[] { "shflex", "elb", "hflex", "knee", "dorsi", "habd", "hrot" })
            };
            if (hands)
            {
                C.AddRange(new[] { S(SupportKind.Hand, 0), S(SupportKind.Hand, 1) });
                T.AddRange(new Target[]
                {
                    new RelPts(new[] { Pt.WrL, Pt.WrR }, new[] { Pt.BallL, Pt.BallR }, 0, 0.0, 0.35 * lA),
                    new PtBand(Pt.WrL, 1, wS - 0.15 * lA, wS + 0.25 * lA), new PtBand(Pt.WrR, 1, -wS - 0.25 * lA, -wS + 0.15 * lA)
                });
            }
            var args = new List<object> { h };
            if (toes || hands) args.Add(toes);
            if (hands) args.Add(hands);
            return new Stance
            {
                Name = "squat h=" + F2(h) + " " + (toes ? "toes" : "flat") + (hands ? " hands down" : ""), Family = "squat",
                Args = args.ToArray(), Supports = C.ToArray(), Targets = T,
                Q0 = Q(("z", h * H), ("pitch", 35), ("tflex", 15), ("shflexL", 60), ("shflexR", 60), ("elbL", 20), ("elbR", 20),
                    ("hflexL", 95), ("hflexR", 95), ("kneeL", 110), ("kneeR", 110), ("dorsiL", toes ? -25 : 25), ("dorsiR", toes ? -25 : 25),
                    ("toeL", toes ? 45 : 0), ("toeR", toes ? 45 : 0), ("habdL", 10), ("habdR", 10)),
                MReq = (toes ? 0.06 : 0.1) * b.FootUnit
            };
        }

        public static Stance TallKneel(BodyModel b)
        {
            double wH = b.HipHalfWidth;
            return new Stance
            {
                Name = "tall kneel", Family = "tallkneel",
                Supports = new[] { S(SupportKind.Shin, 0), S(SupportKind.Shin, 1) },
                SkipClear = Mask("ftL", "toL", "ftR", "toR"),
                Targets = new List<Target>
                {
                    new PtBand(Pt.KnL, 1, wH, 2.0 * wH), new PtBand(Pt.KnR, 1, -2.0 * wH, -wH), new TrunkElev(70, 90),
                    new QBand(V("hflexL"), -10, 35), new QBand(V("hflexR"), -10, 35), new QBand(V("dorsiL"), -50, -25),
                    new QBand(V("dorsiR"), -50, -25), new HeadLook(-20, 10), new Sym(new[] { "hflex", "knee", "dorsi" })
                },
                Q0 = Q(("z", b.ThighLength + b.ShinRadius), ("pitch", 5), ("hflexL", 10), ("hflexR", 10), ("kneeL", 90), ("kneeR", 90),
                    ("dorsiL", -40), ("dorsiR", -40)),
                MReq = 0.02 * b.HipHeight
            };
        }

        public static Stance OneKneel(BodyModel b, int lift = 0)
        {
            int on = 1 - lift;
            double wH = b.HipHalfWidth, H = b.HipHeight;
            var st = TallKneel(b);
            st.Name = "one knee, the other leg lifted";
            st.Family = "onekneel";
            st.Args = new object[] { LR(lift) };
            st.Supports = new[] { S(SupportKind.Shin, on) };
            st.Targets = new List<Target>
            {
                new PtBand(P("kn" + LR(on)), 1, on == 1 ? -2.0 * wH : wH, on == 1 ? -wH : 2.0 * wH),
                new TrunkElev(65, 90), new Lifted(P("an" + LR(lift)), 0.06 * H), new Lifted(P("kn" + LR(lift)), 0.08 * H),
                new QBand(V("dorsi" + LR(on)), -50, -25), new HeadLook(-20, 10)
            };
            st.MReq = 0.0;
            return st;
        }

        public static Stance HalfKneel(BodyModel b, int front = 0, string hands = "none", string back = "laid")
        {
            double lA = b.ArmUnit, lL = b.LegUnit, lT = b.ThighLength, wH = b.HipHalfWidth, wS = b.ShoulderHalfWidth;
            int bk = 1 - front;
            double sgf = front == 0 ? 1 : -1;
            string f = LR(front), k = LR(bk);
            var C = new List<SupportSpec> { S(SupportKind.Sole, front) };
            if (back == "laid") C.Add(S(SupportKind.Shin, bk));
            else C.AddRange(new[] { S(SupportKind.Knee, bk), S(SupportKind.Toes, bk) });
            var T = new List<Target>
            {
                new PtBand(P("an" + f), 0, 0.25 * lL, 0.55 * lL, Pt.P),
                new PtBand(P("an" + f), 1, Math.Min(sgf * wH, sgf * 2.6 * wH), Math.Max(sgf * wH, sgf * 2.6 * wH)), new FootYaw(front, -0.2, 0.35),
                new PtBand(P("kn" + k), 0, -0.35 * lT, 0.10 * lT, Pt.P),
                new PtBand(P("kn" + k), 1, Math.Min(-sgf * 0.6 * wH, -sgf * 2.0 * wH), Math.Max(-sgf * 0.6 * wH, -sgf * 2.0 * wH)),
                new HeadLook(-30, 10)
            };
            if (back == "laid") T.Add(new QBand(V("dorsi" + k), -50, -25));
            if (hands == "thigh")
            {
                C.Add(SupportSpec.HandOnThigh(front, front, 0.8));
                T.Add(new HandOnThigh(b, front, front));
                T.Add(new TrunkElev(35, 90));
            }
            else if (hands == "both")
            {
                C.Add(SupportSpec.HandOnThigh(front, front, 0.85));
                C.Add(SupportSpec.HandOnThigh(bk, front, 0.7));
                T.Add(new HandOnThigh(b, front, front, 0.65, 0.97));
                T.Add(new HandOnThigh(b, bk, front, 0.5, 0.9));
                T.Add(new TrunkElev(30, 90));
            }
            else if (hands == "ground")
            {
                C.AddRange(new[] { S(SupportKind.Hand, 0), S(SupportKind.Hand, 1) });
                T.Add(new RelPts(new[] { Pt.WrL, Pt.WrR }, new[] { P("an" + f) }, 0, -0.15 * lA, 0.35 * lA));
                T.Add(new PtBand(Pt.WrL, 1, wS - 0.2 * lA, wS + 0.3 * lA));
                T.Add(new PtBand(Pt.WrR, 1, -wS - 0.3 * lA, -wS + 0.2 * lA));
            }
            else T.Add(new TrunkElev(55, 90));
            var q0 = new Dictionary<int, double>
            {
                [V("z")] = b.ThighLength + b.ShinRadius, [V("pitch")] = hands != "ground" ? 10 : 55, [V("hflex" + f)] = 80, [V("knee" + f)] = 90,
                [V("dorsi" + f)] = 10, [V("hflex" + k)] = 0, [V("knee" + k)] = 90, [V("dorsi" + k)] = back == "laid" ? -40 : -10,
                [V("toe" + k)] = back == "laid" ? 0 : 50, [V("shflexL")] = hands != "ground" ? 30 : 100,
                [V("shflexR")] = hands != "ground" ? 30 : 100, [V("elbL")] = 30, [V("elbR")] = 30
            };
            return new Stance
            {
                Name = string.Format("half-kneel ({0} foot forward), hands: {1}, back toes {2}", front == 0 ? "left" : "right", hands, back),
                Family = "halfkneel", Args = back == "laid" ? new object[] { f, hands } : new object[] { f, hands, back },
                Supports = C.ToArray(), Targets = T, Q0 = q0, MReq = 0.03 * b.HipHeight,
                SkipClear = back == "laid" ? Mask("ft" + k, "to" + k) : 0
            };
        }

        public static Stance HalfKneelUp(BodyModel b, double h, int front = 0, string hands = "none")
        {
            double lL = b.LegUnit, H = b.HipHeight, wH = b.HipHalfWidth;
            int bk = 1 - front;
            double sgf = front == 0 ? 1 : -1;
            string f = LR(front), k = LR(bk);
            var C = new List<SupportSpec> { S(SupportKind.Sole, front), S(SupportKind.Ball, bk), S(SupportKind.Toes, bk) };
            var T = new List<Target>
            {
                new PtBand(Pt.P, 2, h * H - 0.004 * H, h * H + 0.004 * H, null, 80.0),
                new PtBand(P("an" + f), 0, 0.12 * lL, 0.55 * lL, Pt.P),
                new PtBand(P("an" + f), 1, Math.Min(sgf * wH, sgf * 2.6 * wH), Math.Max(sgf * wH, sgf * 2.6 * wH)),
                new FootYaw(front, -0.2, 0.35), new PtBand(P("ball" + k), 0, -0.9 * lL, -0.15 * lL, Pt.P),
                new PtBand(P("ball" + k), 1, Math.Min(-sgf * 0.5 * wH, -sgf * 2.4 * wH), Math.Max(-sgf * 0.5 * wH, -sgf * 2.4 * wH)),
                new HeadLook(-30, 10)
            };
            if (hands == "thigh")
            {
                C.Add(SupportSpec.HandOnThigh(front, front, 0.8));
                T.Add(new HandOnThigh(b, front, front));
            }
            var q0 = new Dictionary<int, double>
            {
                [V("z")] = h * H, [V("pitch")] = 25, [V("hflex" + f)] = 70, [V("knee" + f)] = 70, [V("dorsi" + f)] = 15,
                [V("hflex" + k)] = -10, [V("knee" + k)] = 60, [V("dorsi" + k)] = -20, [V("toe" + k)] = 40, [V("shflexL")] = 30,
                [V("shflexR")] = 30, [V("elbL")] = 30, [V("elbR")] = 30
            };
            return new Stance
            {
                Name = string.Format("rising from the half-kneel h={0}, hands: {1}", F2(h), hands), Family = "halfkneel_up",
                Args = new object[] { h, f, hands }, Supports = C.ToArray(), Targets = T, Q0 = q0, MReq = 0.03 * H
            };
        }

        public static Stance StoopKnees(BodyModel b, double kf)
        {
            double H = b.HipHeight, wH = b.HipHalfWidth;
            return new Stance
            {
                Name = string.Format(CultureInfo.InvariantCulture, "stoop, hands on the knees, knees {0}", (int)kf), Family = "stoopknees",
                Args = new object[] { kf },
                Supports = new[]
                {
                    S(SupportKind.Sole, 0), S(SupportKind.Sole, 1), SupportSpec.HandOnThigh(0, 0, 0.85), SupportSpec.HandOnThigh(1, 1, 0.85)
                },
                Targets = new List<Target>
                {
                    new QBand(V("kneeL"), kf - 4, kf + 4, 3.0), new QBand(V("kneeR"), kf - 4, kf + 4, 3.0), new PtBand(Pt.AnL, 1, wH, 2.4 * wH),
                    new PtBand(Pt.AnR, 1, -2.4 * wH, -wH), new RelPts(new[] { Pt.AnL }, new[] { Pt.AnR }, 0, -0.03 * H, 0.03 * H),
                    new HandOnThigh(b, 0, 0, 0.65, 0.97), new HandOnThigh(b, 1, 1, 0.65, 0.97), new HeadLook(-45, 5), new Sym(SymBase)
                },
                Q0 = Q(("z", 0.8 * H), ("pitch", 40), ("tflex", 15), ("hflexL", 60), ("hflexR", 60), ("kneeL", kf), ("kneeR", kf),
                    ("dorsiL", 10), ("dorsiR", 10), ("shflexL", 40), ("shflexR", 40), ("elbL", 10), ("elbR", 10)),
                MReq = 0.1 * b.FootUnit
            };
        }

        public static Stance SitUp(BodyModel b, double ang, bool hands = false)
        {
            double lA = b.ArmUnit, wH = b.HipHalfWidth;
            var C = new List<SupportSpec> { S(SupportKind.Seat), S(SupportKind.Calf, 0), S(SupportKind.Calf, 1) };
            var T = new List<Target>
            {
                new TrunkElev(ang - 3, ang + 3, 4.0), new QBand(V("kneeL"), 0, 8), new QBand(V("kneeR"), 0, 8),
                new Sym(new[] { "hflex", "knee", "shflex", "elb" })
            };
            if (hands)
            {
                C.AddRange(new[] { S(SupportKind.Hand, 0), S(SupportKind.Hand, 1) });
                T.AddRange(new Target[]
                {
                    new RelPts(new[] { Pt.WrL, Pt.WrR }, new[] { Pt.P }, 0, -0.55 * lA, 0.05 * lA), new PtBand(Pt.WrL, 1, wH + 0.1 * lA, wH + 0.7 * lA),
                    new PtBand(Pt.WrR, 1, -wH - 0.7 * lA, -wH - 0.1 * lA)
                });
            }
            else T.AddRange(new Target[] { new QBand(V("shflexL"), 60, 110), new QBand(V("shflexR"), 60, 110) });
            return new Stance
            {
                Name = string.Format("sitting up, the trunk {0} deg from the ground, {1}", (int)ang, hands ? "hands behind" : "arms reaching forward"),
                Family = "situp", Args = hands ? new object[] { ang, true } : new object[] { ang },
                Supports = C.ToArray(), Targets = T, SkipClear = Mask("ftL", "ftR", "toL", "toR"),
                Q0 = Q(("z", b.HipsRadius), ("pitch", -(90 - ang)), ("hflexL", ang), ("hflexR", ang), ("shflexL", !hands ? 80 : -30),
                    ("shflexR", !hands ? 80 : -30), ("elbL", 10), ("elbR", 10), ("dorsiL", -10), ("dorsiR", -10)),
                MReq = 0.02 * b.HipHeight
            };
        }

        public static Stance Press(BodyModel b)
        {
            double lA = b.ArmUnit, wS = b.ShoulderHalfWidth;
            return new Stance
            {
                Name = "pushed up from the front (chest off the ground)", Family = "press",
                Supports = new[]
                {
                    S(SupportKind.Hand, 0), S(SupportKind.Hand, 1), S(SupportKind.Knee, 0), S(SupportKind.Knee, 1), S(SupportKind.Calf, 0),
                    S(SupportKind.Calf, 1)
                },
                Targets = new List<Target>
                {
                    new PtBand(Pt.WrL, 0, -0.25 * lA, 0.1 * lA, Pt.ShL), new PtBand(Pt.WrR, 0, -0.25 * lA, 0.1 * lA, Pt.ShR),
                    new PtBand(Pt.WrL, 1, wS - 0.05 * lA, wS + 0.3 * lA), new PtBand(Pt.WrR, 1, -wS - 0.3 * lA, -wS + 0.05 * lA),
                    new PtBand(Pt.Neck, 2, 0.55 * lA, 9.0), new HeadLook(-60, 0), new QBand(V("kneeL"), 0, 20), new QBand(V("kneeR"), 0, 20),
                    new QBand(V("dorsiL"), -50, -30), new QBand(V("dorsiR"), -50, -30), new Sym(new[] { "shflex", "elb", "hflex", "knee" })
                },
                SkipClear = Mask("ftL", "toL", "ftR", "toR", "hips"),
                Q0 = Q(("z", b.HipsRadius + 0.05), ("pitch", 75), ("tflex", -10), ("shflexL", 70), ("shflexR", 70), ("elbL", 30), ("elbR", 30),
                    ("hflexL", -10), ("hflexR", -10), ("kneeL", 5), ("kneeR", 5), ("dorsiL", -40), ("dorsiR", -40)),
                MReq = 0.04 * b.HipHeight
            };
        }

        public static Stance Forearms(BodyModel b)
        {
            double lu = b.UpperArmLength, lf = b.ForearmLength, wS = b.ShoulderHalfWidth;
            return new Stance
            {
                Name = "on its forearms", Family = "forearms",
                Supports = new[]
                {
                    S(SupportKind.Forearm, 0), S(SupportKind.Forearm, 1), S(SupportKind.Seat), S(SupportKind.Knee, 0), S(SupportKind.Knee, 1),
                    S(SupportKind.Calf, 0), S(SupportKind.Calf, 1)
                },
                Targets = new List<Target>
                {
                    new PtBand(Pt.ElL, 0, -0.2 * lu, 0.25 * lu, Pt.ShL), new PtBand(Pt.ElR, 0, -0.2 * lu, 0.25 * lu, Pt.ShR),
                    new PtBand(Pt.ElL, 1, wS - 0.1 * lu, wS + 0.3 * lu), new PtBand(Pt.ElR, 1, -wS - 0.3 * lu, -wS + 0.1 * lu),
                    new RelPts(new[] { Pt.WrL }, new[] { Pt.ElL }, 0, 0.7 * lf, 9.0), new RelPts(new[] { Pt.WrR }, new[] { Pt.ElR }, 0, 0.7 * lf, 9.0),
                    new HeadLook(-30, 15), new QBand(V("kneeL"), 0, 20), new QBand(V("kneeR"), 0, 20), new QBand(V("dorsiL"), -50, -30),
                    new QBand(V("dorsiR"), -50, -30), new Sym(new[] { "shflex", "elb", "hflex", "knee" })
                },
                SkipClear = Mask("ftL", "toL", "ftR", "toR", "hips", "uaL", "uaR"),
                Q0 = Q(("z", b.HipsRadius + 0.03), ("pitch", 75), ("tflex", -10), ("shflexL", 80), ("shflexR", 80), ("elbL", 90), ("elbR", 90),
                    ("hflexL", -5), ("hflexR", -5), ("kneeL", 5), ("kneeR", 5), ("dorsiL", -40), ("dorsiR", -40)),
                MReq = 0.04 * b.HipHeight
            };
        }

        public static Stance LongSit(BodyModel b)
        {
            double lA = b.ArmUnit, wH = b.HipHalfWidth;
            return new Stance
            {
                Name = "sitting, legs out, hands behind", Family = "longsit",
                Supports = new[] { S(SupportKind.Seat), S(SupportKind.Calf, 0), S(SupportKind.Calf, 1), S(SupportKind.Hand, 0), S(SupportKind.Hand, 1) },
                Targets = new List<Target>
                {
                    new TrunkElev(55, 85), new QBand(V("kneeL"), 0, 10), new QBand(V("kneeR"), 0, 10),
                    new RelPts(new[] { Pt.WrL, Pt.WrR }, new[] { Pt.P }, 0, -0.6 * lA, -0.05 * lA), new PtBand(Pt.WrL, 1, wH + 0.1 * lA, wH + 0.7 * lA),
                    new PtBand(Pt.WrR, 1, -wH - 0.7 * lA, -wH - 0.1 * lA), new Sym(new[] { "hflex", "knee", "shflex", "elb" })
                },
                SkipClear = Mask("ftL", "ftR", "toL", "toR"),
                Q0 = Q(("z", b.HipsRadius), ("pitch", -20), ("hflexL", 70), ("hflexR", 70), ("shflexL", -30), ("shflexR", -30), ("elbL", 5), ("elbR", 5)),
                MReq = 0.03 * b.HipHeight
            };
        }

        public static Stance OneKneelToes(BodyModel b, int lift = 0)
        {
            int on = 1 - lift;
            var st = OneKneel(b, lift);
            st.Name = "one knee and its toes, the other leg lifted";
            st.Family = "onekneel_toes";
            st.Supports = new[] { S(SupportKind.Knee, on), S(SupportKind.Toes, on) };
            var t = st.Targets.GetRange(0, st.Targets.Count - 2);
            t.Add(new HeadLook(-20, 10));
            st.Targets = t;
            st.Q0[V("dorsi" + LR(on))] = -10;
            st.Q0[V("toe" + LR(on))] = 50;
            st.SkipClear = Mask("ft" + LR(on));
            return st;
        }

        public static Stance Ready(Stance st, SupportKind[] kinds, double m = double.NaN)
        {
            var o = st.Clone();
            o.Ready = kinds;
            if (!double.IsNaN(m)) o.MReady = m;
            var names = new List<string>();
            foreach (var k in kinds) names.Add(KindName(k));
            o.Name = st.Name + " [ready to let go of: " + string.Join(",", names) + "]";
            return o;
        }

        public static string KindName(SupportKind k)
        {
            switch (k)
            {
                case SupportKind.ThighFront: return "thighF";
                case SupportKind.HandOnThigh: return "handthigh";
                default: return k.ToString().ToLowerInvariant();
            }
        }

        public static Stance SquatHandsReady(BodyModel b, double h)
        {
            var st = Ready(Squat(b, h, false, true), new[] { SupportKind.Hand }, 0.08 * (b.HeelLength + b.BallLength));
            st.Family = "squat_hands_ready";
            st.Args = new object[] { h };
            return st;
        }

        public static Stance CrouchKneelReady(BodyModel b, int front = 0)
        {
            var st = Ready(HalfKneel(b, front, "ground"), new[] { SupportKind.Hand }, 0.02 * b.HipHeight);
            st.Family = "crouchkneel_ready";
            st.Args = new object[] { LR(front) };
            return st;
        }

        public static Stance BearReady(BodyModel b, double Dd)
        {
            var st = Ready(Bear(b, Dd), new[] { SupportKind.Hand }, 0.08 * (b.HeelLength + b.BallLength));
            st.Family = "bear_ready";
            st.Args = new object[] { Dd };
            return st;
        }

        public static Stance QuadReadyKneel(BodyModel b)
        {
            var st = Ready(Quad(b), new[] { SupportKind.Hand }, 0.01 * b.HipHeight);
            st.Family = "quad_ready_kneel";
            st.Args = new object[0];
            return st;
        }

        static int Side(object a) => (string)a == "L" ? 0 : 1;
        static double Num(object a) => Convert.ToDouble(a, CultureInfo.InvariantCulture);

        /// <summary>A stance by its lib3 family name and arguments (as run3.STANCES writes them; sides "L"/"R").</summary>
        public static Stance Make(BodyModel b, string family, params object[] a)
        {
            switch (family)
            {
                case "stand": return Stand(b);
                case "quad": return Quad(b, a.Length > 0 && (bool)a[0]);
                case "tripod": return Tripod(b, a.Length > 0 ? Side(a[0]) : 0);
                case "quad_ready_kneel": return QuadReadyKneel(b);
                case "bear": return Bear(b, Num(a[0]), a.Length > 1 && (bool)a[1]);
                case "bear_ready": return BearReady(b, Num(a[0]));
                case "squat": return Squat(b, Num(a[0]), a.Length > 1 && (bool)a[1], a.Length > 2 && (bool)a[2]);
                case "squat_hands_ready": return SquatHandsReady(b, Num(a[0]));
                case "tallkneel": return TallKneel(b);
                case "onekneel": return OneKneel(b, a.Length > 0 ? Side(a[0]) : 0);
                case "onekneel_toes": return OneKneelToes(b, a.Length > 0 ? Side(a[0]) : 0);
                case "halfkneel":
                    return HalfKneel(b, a.Length > 0 ? Side(a[0]) : 0, a.Length > 1 ? (string)a[1] : "none", a.Length > 2 ? (string)a[2] : "laid");
                case "crouchkneel_ready": return CrouchKneelReady(b, a.Length > 0 ? Side(a[0]) : 0);
                case "halfkneel_up":
                    return HalfKneelUp(b, Num(a[0]), a.Length > 1 ? Side(a[1]) : 0, a.Length > 2 ? (string)a[2] : "none");
                case "stoopknees": return StoopKnees(b, Num(a[0]));
                case "situp": return SitUp(b, Num(a[0]), a.Length > 1 && (bool)a[1]);
                case "press": return Press(b);
                case "forearms": return Forearms(b);
                case "longsit": return LongSit(b);
                default: throw new ArgumentException("unknown stance family " + family);
            }
        }

        /// <summary>run3.STANCES (42) + run3b.EXTRA (3): the prototype's batch, as (family, args).</summary>
        public static readonly (string Family, object[] Args)[] PrototypeBatch =
        {
            ("stand", new object[0]), ("quad", new object[0]), ("quad", new object[] { true }), ("tripod", new object[] { "L" }),
            ("quad_ready_kneel", new object[0]),
            ("bear", new object[] { 0.8 }), ("bear", new object[] { 0.5 }), ("bear", new object[] { 0.3 }), ("bear", new object[] { 0.4, true }),
            ("bear_ready", new object[] { 0.25 }),
            ("squat", new object[] { 0.35 }), ("squat", new object[] { 0.5 }), ("squat", new object[] { 0.65 }), ("squat", new object[] { 0.8 }),
            ("squat", new object[] { 0.4, true }), ("squat", new object[] { 0.6, true }),
            ("squat_hands_ready", new object[] { 0.35 }), ("squat_hands_ready", new object[] { 0.5 }),
            ("tallkneel", new object[0]), ("onekneel_toes", new object[] { "L" }),
            ("halfkneel", new object[] { "L", "none" }), ("halfkneel", new object[] { "L", "thigh" }), ("halfkneel", new object[] { "L", "both" }),
            ("halfkneel", new object[] { "L", "ground" }),
            ("crouchkneel_ready", new object[] { "L" }),
            ("halfkneel_up", new object[] { 0.55, "L", "none" }), ("halfkneel_up", new object[] { 0.7, "L", "none" }),
            ("halfkneel_up", new object[] { 0.85, "L", "none" }),
            ("halfkneel_up", new object[] { 0.55, "L", "thigh" }), ("halfkneel_up", new object[] { 0.7, "L", "thigh" }),
            ("halfkneel", new object[] { "R", "none" }), ("halfkneel_up", new object[] { 0.55, "R", "none" }), ("crouchkneel_ready", new object[] { "R" }),
            ("stoopknees", new object[] { 30.0 }), ("stoopknees", new object[] { 60.0 }),
            ("situp", new object[] { 15.0 }), ("situp", new object[] { 35.0 }), ("situp", new object[] { 60.0 }), ("situp", new object[] { 25.0, true }),
            ("press", new object[0]), ("forearms", new object[0]), ("longsit", new object[0]),
            ("halfkneel_up", new object[] { 0.7, "R", "none" }), ("halfkneel_up", new object[] { 0.85, "R", "none" }), ("onekneel_toes", new object[] { "R" })
        };
    }
}
