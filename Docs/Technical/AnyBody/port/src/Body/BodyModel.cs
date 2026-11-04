// The body: b3.py's MINERS, make_body and present_caps, number for number.
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody
{
    /// <summary>The joint ranges' keys (b3.RANGES), in degrees.</summary>
    public enum RangeKey
    {
        Yaw, Pitch, Roll, TFlex, TLat, TTw, NFlex, NLat, NRot, ShFlex, ShAbd, ShRot, Elb, HFlex, HAbd, HRot, Knee, Dorsi, Inv, Toe
    }

    /// <summary>A body as b3.make_body builds it: lengths (m), masses (kg), capacities (N m, strength applied), ranges (deg).
    /// Side arrays are [L, R]. Field comments give the prototype's key.</summary>
    public sealed class BodyModel
    {
        public string Name;

        // ---- lengths
        /// <summary>lT: thigh length.</summary>
        public double ThighLength;
        /// <summary>lS: shin length.</summary>
        public double ShinLength;
        /// <summary>a: ankle height above the sole.</summary>
        public double AnkleHeight;
        /// <summary>hl: heel length behind the ankle.</summary>
        public double HeelLength;
        /// <summary>bl: ankle to ball.</summary>
        public double BallLength;
        /// <summary>tl: toe length.</summary>
        public double ToeLength;
        /// <summary>lu: upper arm.</summary>
        public double UpperArmLength;
        /// <summary>lf: forearm (elbow to wrist).</summary>
        public double ForearmLength;
        /// <summary>ra: arm radius.</summary>
        public double ArmRadius;
        /// <summary>rl: shin (leg) radius.</summary>
        public double ShinRadius;
        /// <summary>rT: thigh radius (1.25 rl).</summary>
        public double ThighRadius;
        /// <summary>wH: half the hip joints' spacing.</summary>
        public double HipHalfWidth;
        /// <summary>wS: half the shoulders' spacing (sx).</summary>
        public double ShoulderHalfWidth;
        /// <summary>dS: the shoulders' forward offset (sz).</summary>
        public double ShoulderForward;
        /// <summary>hW: pelvis to waist (the back joint).</summary>
        public double WaistHeight;
        /// <summary>hS: waist to neck (sy).</summary>
        public double TrunkLength;
        /// <summary>tR: waist to the trunk's centre (hS / 2).</summary>
        public double TrunkCentre;
        /// <summary>lN: neck to the head's centre.</summary>
        public double NeckLength;
        /// <summary>rH: head radius.</summary>
        public double HeadRadius;
        /// <summary>H: hip height, a + 0.956 (lT + lS).</summary>
        public double HipHeight;
        /// <summary>gH: the hips capsule's radius.</summary>
        public double HipsRadius;
        /// <summary>gT: the trunk capsule's radius.</summary>
        public double TrunkRadius;
        /// <summary>fw: foot half width.</summary>
        public double FootHalfWidth;
        /// <summary>fh: foot box height.</summary>
        public double FootHeight;
        /// <summary>tw: toe half width.</summary>
        public double ToeHalfWidth;
        /// <summary>th: toe box height.</summary>
        public double ToeHeight;
        /// <summary>height: standing height.</summary>
        public double Height;

        // ---- masses
        public double MassHips, MassTrunk, MassHead;
        public double[] MassUpperArm = new double[2], MassForearm = new double[2], MassThigh = new double[2],
            MassShin = new double[2], MassFoot = new double[2], MassToes = new double[2];

        /// <summary>M: total mass.</summary>
        public double Mass;

        // ---- capacities (strength applied)
        public double CapBack, CapNeck;
        public double[] CapShoulder = new double[2], CapElbow = new double[2], CapHip = new double[2],
            CapKnee = new double[2], CapAnkle = new double[2], CapToe = new double[2];

        /// <summary>The joint ranges in degrees, (lo, hi) per RangeKey.</summary>
        public double[] RangeLo = new double[20], RangeHi = new double[20];

        public double Strength;

        // ---- the body's own units (lib3.U)
        /// <summary>lA = lu + lf + ra.</summary>
        public double ArmUnit => UpperArmLength + ForearmLength + ArmRadius;
        /// <summary>lL = lT + lS + a.</summary>
        public double LegUnit => ThighLength + ShinLength + AnkleHeight;
        /// <summary>foot = hl + bl.</summary>
        public double FootUnit => HeelLength + BallLength;

        public BodyModel Clone()
        {
            var b = (BodyModel)MemberwiseClone();
            b.MassUpperArm = (double[])MassUpperArm.Clone();
            b.MassForearm = (double[])MassForearm.Clone();
            b.MassThigh = (double[])MassThigh.Clone();
            b.MassShin = (double[])MassShin.Clone();
            b.MassFoot = (double[])MassFoot.Clone();
            b.MassToes = (double[])MassToes.Clone();
            b.CapShoulder = (double[])CapShoulder.Clone();
            b.CapElbow = (double[])CapElbow.Clone();
            b.CapHip = (double[])CapHip.Clone();
            b.CapKnee = (double[])CapKnee.Clone();
            b.CapAnkle = (double[])CapAnkle.Clone();
            b.CapToe = (double[])CapToe.Clone();
            b.RangeLo = (double[])RangeLo.Clone();
            b.RangeHi = (double[])RangeHi.Clone();
            return b;
        }

        /// <summary>A scalar by the prototype's key (for tests): lengths, "M", "strength".</summary>
        public double Get(string key)
        {
            switch (key)
            {
                case "lT": return ThighLength;
                case "lS": return ShinLength;
                case "a": return AnkleHeight;
                case "hl": return HeelLength;
                case "bl": return BallLength;
                case "tl": return ToeLength;
                case "lu": return UpperArmLength;
                case "lf": return ForearmLength;
                case "ra": return ArmRadius;
                case "rl": return ShinRadius;
                case "rT": return ThighRadius;
                case "wH": return HipHalfWidth;
                case "wS": return ShoulderHalfWidth;
                case "dS": return ShoulderForward;
                case "hW": return WaistHeight;
                case "hS": return TrunkLength;
                case "tR": return TrunkCentre;
                case "lN": return NeckLength;
                case "rH": return HeadRadius;
                case "H": return HipHeight;
                case "gH": return HipsRadius;
                case "gT": return TrunkRadius;
                case "fw": return FootHalfWidth;
                case "fh": return FootHeight;
                case "tw": return ToeHalfWidth;
                case "th": return ToeHeight;
                case "height": return Height;
                case "M": return Mass;
                case "strength": return Strength;
                default: throw new ArgumentException("unknown key " + key);
            }
        }

        public static readonly string[] ScalarKeys =
        {
            "lT", "lS", "a", "hl", "bl", "tl", "lu", "lf", "ra", "rl", "rT", "wH", "wS", "dS", "hW", "hS", "tR", "lN", "rH", "H",
            "gH", "gT", "fw", "fh", "tw", "th", "height", "M", "strength"
        };
    }

    /// <summary>A miner's source numbers (b3.MINERS).</summary>
    public sealed class MinerSource
    {
        public string Name;
        public double H, WH, Leg, A, Hl, Bl, Tl, HW, TR, HeadRise, Sx, Sy, Sz, Lu, Lf, BodyFront, HeadHalf, Ra, Rl, Height;
        public double MHips, MTrunk, MHead;
        public double[] MUa, MFa, MTh, MSh, MFt, MTo;
        public double CBack, CNeck, CShoulder, CElbow, CHip, CKnee, CAnkle, CToe;
    }

    /// <summary>How a variant body is made from a miner (b3.make_body's arguments).</summary>
    public sealed class BodyVariant
    {
        public string Name;
        public string Source = "Small";
        public double Strength = 1.0;
        public double Scale = 1.0;
        /// <summary>Capacity multipliers by key: back, neck, shoulder, elbow, hip, knee, ankle, toe.</summary>
        public Dictionary<string, double> CapMul;
        /// <summary>Per-side capacity multipliers by key: (L, R).</summary>
        public Dictionary<string, (double L, double R)> SideMul;
        /// <summary>Range overrides by key, degrees.</summary>
        public Dictionary<RangeKey, (double Lo, double Hi)> Ranges;
        /// <summary>Mass multipliers by key: hips, trunk, head, ua, fa, th, sh, ft, to.</summary>
        public Dictionary<string, double> MassMul;
        /// <summary>Length multipliers: leg, arm, trunk.</summary>
        public double LegMul = 1.0, ArmMul = 1.0, TrunkMul = 1.0;
        /// <summary>"present" (capacity follows need, PhysicalBody's rules) or "cube" (the base body's x scale^3).</summary>
        public string CapRule = "present";
    }

    public static class BodyFactory
    {
        public const double G = 9.81;

        /// <summary>b3.RANGES (degrees), in RangeKey order.</summary>
        public static readonly double[,] DefaultRanges =
        {
            { -180, 180 }, { -180, 180 }, { -180, 180 }, { -20, 75 }, { -25, 25 }, { -30, 30 }, { -40, 45 }, { -30, 30 }, { -50, 50 },
            { -50, 170 }, { -10, 120 }, { -70, 70 }, { -2, 145 }, { -18, 120 }, { -20, 45 }, { -35, 40 }, { -2, 150 }, { -50, 30 },
            { -20, 20 }, { -20, 80 }
        };

        public static readonly MinerSource Small = new MinerSource
        {
            Name = "Small", H = 0.605714, WH = 0.068, Leg = 0.27234, A = 0.085, Hl = 0.07242, Bl = 0.08449, Tl = 0.055522,
            HW = 0.1904, TR = 0.10295, HeadRise = 0.3805, Sx = 0.145, Sy = 0.2059, Sz = -0.01, Lu = 0.2343, Lf = 0.1988,
            BodyFront = 0.1452, HeadHalf = 0.163, Ra = 0.033, Rl = 0.041, Height = 1.42,
            MHips = 7.88, MTrunk = 8.96, MHead = 9.96, MUa = new[] { 1.12, 1.11 }, MFa = new[] { 0.80, 0.80 },
            MTh = new[] { 2.38, 2.38 }, MSh = new[] { 1.35, 1.35 }, MFt = new[] { 1.07, 1.07 }, MTo = new[] { 0.21, 0.21 },
            CBack = 257, CNeck = 51, CShoulder = 42, CElbow = 36, CHip = 130, CKnee = 87, CAnkle = 61, CToe = 18
        };

        public static readonly MinerSource Long = new MinerSource
        {
            Name = "Long", H = 0.9257484, WH = 0.082, Leg = 0.439722, A = 0.085, Hl = 0.08649, Bl = 0.100905, Tl = 0.066309,
            HW = 0.1967, TR = 0.14415, HeadRise = 0.49010634, Sx = 0.165, Sy = 0.2883, Sz = 0.032, Lu = 0.3255, Lf = 0.279,
            BodyFront = 0.1706, HeadHalf = 0.1329, Ra = 0.036, Rl = 0.046, Height = 1.86,
            MHips = 9.68, MTrunk = 17.04, MHead = 6.48, MUa = new[] { 1.82, 1.82 }, MFa = new[] { 1.38, 1.38 },
            MTh = new[] { 4.87, 4.87 }, MSh = new[] { 2.74, 2.74 }, MFt = new[] { 1.52, 1.52 }, MTo = new[] { 0.30, 0.30 },
            CBack = 347, CNeck = 38, CShoulder = 76, CElbow = 65, CHip = 303, CKnee = 202, CAnkle = 141, CToe = 42
        };

        public static readonly MinerSource Round = new MinerSource
        {
            Name = "Round", H = 0.6975703, WH = 0.095, Leg = 0.320382, A = 0.085, Hl = 0.07956, Bl = 0.09282, Tl = 0.060996,
            HW = 0.2072, TR = 0.1443, HeadRise = 0.3875711, Sx = 0.19, Sy = 0.2886, Sz = -0.035, Lu = 0.2496, Lf = 0.2184,
            BodyFront = 0.1963, HeadHalf = 0.1976, Ra = 0.045, Rl = 0.058, Height = 1.56,
            MHips = 23.63, MTrunk = 26.20, MHead = 9.82, MUa = new[] { 2.17, 2.17 }, MFa = new[] { 1.53, 1.53 },
            MTh = new[] { 5.60, 5.60 }, MSh = new[] { 3.14, 3.14 }, MFt = new[] { 1.30, 1.30 }, MTo = new[] { 0.26, 0.26 },
            CBack = 462, CNeck = 29, CShoulder = 85, CElbow = 74, CHip = 331, CKnee = 220, CAnkle = 154, CToe = 46
        };

        public static MinerSource Miner(string name)
        {
            switch (name)
            {
                case "Small": return Small;
                case "Long": return Long;
                case "Round": return Round;
                default: throw new ArgumentException("unknown miner " + name);
            }
        }

        public static BodyModel Make(string miner, double strength = 1.0) =>
            Make(new BodyVariant { Name = miner, Source = miner, Strength = strength });

        /// <summary>b3.make_body.</summary>
        public static BodyModel Make(BodyVariant v)
        {
            var s0 = Miner(v.Source);
            double k = v.Scale;
            double lmLeg = v.LegMul, lmArm = v.ArmMul, lmTrunk = v.TrunkMul;
            var b = new BodyModel { Name = v.Name ?? v.Source };
            b.ThighLength = b.ShinLength = s0.Leg * k * lmLeg;
            b.AnkleHeight = s0.A * k;
            b.HeelLength = s0.Hl * k;
            b.BallLength = s0.Bl * k;
            b.ToeLength = s0.Tl * k;
            b.UpperArmLength = s0.Lu * k * lmArm;
            b.ForearmLength = s0.Lf * k * lmArm;
            b.ArmRadius = s0.Ra * k;
            b.ShinRadius = s0.Rl * k;
            b.ThighRadius = 1.25 * b.ShinRadius;
            b.HipHalfWidth = s0.WH * k;
            b.ShoulderHalfWidth = s0.Sx * k;
            b.ShoulderForward = s0.Sz * k;
            b.WaistHeight = s0.HW * k * lmTrunk;
            b.TrunkLength = s0.Sy * k * lmTrunk;
            b.TrunkCentre = b.TrunkLength / 2;
            b.NeckLength = Math.Max(0.03 * k, s0.HeadRise * k * lmTrunk - b.TrunkLength);
            b.HeadRadius = Math.Max(0.06, s0.HeadHalf * k);
            b.HipHeight = b.AnkleHeight + 0.956 * (b.ThighLength + b.ShinLength);
            b.HipsRadius = Math.Min(Math.Max(s0.BodyFront * k, 1.5 * b.ShinRadius), 0.3 * b.HipHeight);
            b.TrunkRadius = Math.Max(s0.Sx * k - b.ArmRadius, 0.5 * s0.Sx * k);
            b.FootHalfWidth = 0.05 * k;
            b.FootHeight = 0.06 * k;
            b.ToeHalfWidth = 0.05 * k;
            b.ToeHeight = 0.05 * k;
            b.Height = s0.Height * k;

            // masses: x k^3, limbs by their length multiplier, trunk by its, then the mass multipliers
            double k3 = Math.Pow(k, 3);
            b.MassHips = s0.MHips * k3;
            b.MassTrunk = s0.MTrunk * k3;
            b.MassHead = s0.MHead * k3;
            for (int i = 0; i < 2; i++)
            {
                b.MassUpperArm[i] = s0.MUa[i] * k3 * lmArm;
                b.MassForearm[i] = s0.MFa[i] * k3 * lmArm;
                b.MassThigh[i] = s0.MTh[i] * k3 * lmLeg;
                b.MassShin[i] = s0.MSh[i] * k3 * lmLeg;
                b.MassFoot[i] = s0.MFt[i] * k3;
                b.MassToes[i] = s0.MTo[i] * k3;
            }
            b.MassTrunk *= lmTrunk;
            if (v.MassMul != null)
            {
                foreach (var kv in v.MassMul)
                {
                    double f = kv.Value;
                    switch (kv.Key)
                    {
                        case "hips": b.MassHips *= f; break;
                        case "trunk": b.MassTrunk *= f; break;
                        case "head": b.MassHead *= f; break;
                        case "ua": b.MassUpperArm[0] *= f; b.MassUpperArm[1] *= f; break;
                        case "fa": b.MassForearm[0] *= f; b.MassForearm[1] *= f; break;
                        case "th": b.MassThigh[0] *= f; b.MassThigh[1] *= f; break;
                        case "sh": b.MassShin[0] *= f; b.MassShin[1] *= f; break;
                        case "ft": b.MassFoot[0] *= f; b.MassFoot[1] *= f; break;
                        case "to": b.MassToes[0] *= f; b.MassToes[1] *= f; break;
                        default: throw new ArgumentException("unknown mass key " + kv.Key);
                    }
                }
            }
            // M = sum over the dict in b3's order: hips, trunk, head, ua, fa, th, sh, ft, to (pairs summed first)
            b.Mass = TotalMass(b);

            // capacities
            var cap = new Dictionary<string, double>();
            var src = new Dictionary<string, double>
            {
                ["back"] = s0.CBack, ["neck"] = s0.CNeck, ["shoulder"] = s0.CShoulder, ["elbow"] = s0.CElbow,
                ["hip"] = s0.CHip, ["knee"] = s0.CKnee, ["ankle"] = s0.CAnkle, ["toe"] = s0.CToe
            };
            string[] order = { "back", "neck", "shoulder", "elbow", "hip", "knee", "ankle", "toe" };
            if (v.CapRule == "cube")
            {
                foreach (var kk in order) cap[kk] = src[kk] * k3;
            }
            else
            {
                var baseCaps = PresentCaps(MinerAsBuilt(s0));
                var now = PresentCaps(b);
                foreach (var kk in order) cap[kk] = src[kk] * now[kk] / baseCaps[kk];
            }
            foreach (var kk in order) cap[kk] = cap[kk] * v.Strength;
            if (v.CapMul != null)
                foreach (var kv in v.CapMul) cap[kv.Key] *= kv.Value;
            b.CapBack = cap["back"];
            b.CapNeck = cap["neck"];
            b.CapShoulder[0] = b.CapShoulder[1] = cap["shoulder"];
            b.CapElbow[0] = b.CapElbow[1] = cap["elbow"];
            b.CapHip[0] = b.CapHip[1] = cap["hip"];
            b.CapKnee[0] = b.CapKnee[1] = cap["knee"];
            b.CapAnkle[0] = b.CapAnkle[1] = cap["ankle"];
            b.CapToe[0] = b.CapToe[1] = cap["toe"];
            if (v.SideMul != null)
            {
                foreach (var kv in v.SideMul)
                {
                    var arr = SideCaps(b, kv.Key);
                    arr[0] *= kv.Value.L;
                    arr[1] *= kv.Value.R;
                }
            }
            for (int r = 0; r < 20; r++)
            {
                b.RangeLo[r] = DefaultRanges[r, 0];
                b.RangeHi[r] = DefaultRanges[r, 1];
            }
            if (v.Ranges != null)
            {
                foreach (var kv in v.Ranges)
                {
                    b.RangeLo[(int)kv.Key] = kv.Value.Lo;
                    b.RangeHi[(int)kv.Key] = kv.Value.Hi;
                }
            }
            b.Strength = v.Strength;
            return b;
        }

        static double[] SideCaps(BodyModel b, string key)
        {
            switch (key)
            {
                case "shoulder": return b.CapShoulder;
                case "elbow": return b.CapElbow;
                case "hip": return b.CapHip;
                case "knee": return b.CapKnee;
                case "ankle": return b.CapAnkle;
                case "toe": return b.CapToe;
                default: throw new ArgumentException("no per-side capacity " + key);
            }
        }

        static double TotalMass(BodyModel b) =>
            b.MassHips + b.MassTrunk + b.MassHead + (b.MassUpperArm[0] + b.MassUpperArm[1]) + (b.MassForearm[0] + b.MassForearm[1])
            + (b.MassThigh[0] + b.MassThigh[1]) + (b.MassShin[0] + b.MassShin[1]) + (b.MassFoot[0] + b.MassFoot[1])
            + (b.MassToes[0] + b.MassToes[1]);

        /// <summary>b3.make_body_caps_present's body: the unchanged miner as built here.</summary>
        static BodyModel MinerAsBuilt(MinerSource s0)
        {
            var b = new BodyModel();
            b.ThighLength = b.ShinLength = s0.Leg;
            b.UpperArmLength = s0.Lu;
            b.ForearmLength = s0.Lf;
            b.WaistHeight = s0.HW;
            b.TrunkLength = s0.Sy;
            b.TrunkCentre = b.TrunkLength / 2;
            b.NeckLength = Math.Max(0.03, s0.HeadRise - b.TrunkLength);
            b.MassHips = s0.MHips;
            b.MassTrunk = s0.MTrunk;
            b.MassHead = s0.MHead;
            for (int i = 0; i < 2; i++)
            {
                b.MassUpperArm[i] = s0.MUa[i];
                b.MassForearm[i] = s0.MFa[i];
                b.MassThigh[i] = s0.MTh[i];
                b.MassShin[i] = s0.MSh[i];
                b.MassFoot[i] = s0.MFt[i];
                b.MassToes[i] = s0.MTo[i];
            }
            b.Mass = TotalMass(b);
            return b;
        }

        /// <summary>b3.present_caps: PhysicalBody's rules (October 6 and 14).</summary>
        public static Dictionary<string, double> PresentCaps(BodyModel b)
        {
            double M = b.Mass;
            double knee = 1.6 * M * G * 0.5 * b.ThighLength;
            double ua = b.MassUpperArm[0] + b.MassUpperArm[1];
            double fa = b.MassForearm[0] + b.MassForearm[1];
            double upperKg = b.MassTrunk + b.MassHead + ua + fa;
            double share = 0.5 * upperKg * G;
            double sh = 1.6 * share * b.UpperArmLength;
            double el = 1.6 * share * b.ForearmLength;
            double asks = b.MassTrunk * (b.WaistHeight + b.TrunkCentre) + b.MassHead * (b.WaistHeight + b.TrunkLength + b.NeckLength)
                          + ua * (b.WaistHeight + b.TrunkLength - 0.5 * b.UpperArmLength)
                          + fa * (b.WaistHeight + b.TrunkLength - b.UpperArmLength - 0.5 * b.ForearmLength);
            double back = 2.4 * asks * G;
            double neck = 3 * b.MassHead * G * b.NeckLength;
            return new Dictionary<string, double>
            {
                ["hip"] = 1.5 * knee, ["knee"] = knee, ["ankle"] = 0.7 * knee, ["toe"] = 0.21 * knee, ["shoulder"] = sh,
                ["elbow"] = el, ["back"] = back, ["neck"] = neck
            };
        }

        /// <summary>run3.BODIES: the three miners and the fourteen invented bodies of the prototype's batch.</summary>
        public static readonly BodyVariant[] PrototypeBodies =
        {
            new BodyVariant { Name = "Small", Source = "Small" },
            new BodyVariant { Name = "Long", Source = "Long" },
            new BodyVariant { Name = "Round", Source = "Round" },
            new BodyVariant { Name = "Small S0.5", Source = "Small", Strength = 0.5 },
            new BodyVariant { Name = "Long S0.5", Source = "Long", Strength = 0.5 },
            new BodyVariant { Name = "Round S0.5", Source = "Round", Strength = 0.5 },
            new BodyVariant { Name = "Small S0.3", Source = "Small", Strength = 0.3 },
            new BodyVariant { Name = "Round S0.3", Source = "Round", Strength = 0.3 },
            new BodyVariant
            {
                Name = "Small, arms by thickness", Source = "Small",
                CapMul = new Dictionary<string, double> { ["shoulder"] = 24.0 / 42, ["elbow"] = 21.0 / 36 }
            },
            new BodyVariant
            {
                Name = "Toddler-like (Small, head x1.3, legs x0.85)", Source = "Small",
                MassMul = new Dictionary<string, double> { ["head"] = 1.3 }, LegMul = 0.85
            },
            new BodyVariant { Name = "Giant (Long x1.6, strength ~ L^3)", Source = "Long", Scale = 1.6, CapRule = "cube" },
            new BodyVariant { Name = "Long, arms x0.65", Source = "Long", ArmMul = 0.65 },
            new BodyVariant
            {
                Name = "Long, stiff ankles (dorsiflexion 5)", Source = "Long",
                Ranges = new Dictionary<RangeKey, (double, double)> { [RangeKey.Dorsi] = (-50, 5) }
            },
            new BodyVariant
            {
                Name = "Round, stiff knees (100)", Source = "Round",
                Ranges = new Dictionary<RangeKey, (double, double)> { [RangeKey.Knee] = (-2, 100) }
            },
            new BodyVariant
            {
                Name = "Heavy trunk (Round, trunk+hips x1.5, S0.7, lean 30)", Source = "Round",
                MassMul = new Dictionary<string, double> { ["trunk"] = 1.5, ["hips"] = 1.5 }, Strength = 0.7,
                Ranges = new Dictionary<RangeKey, (double, double)> { [RangeKey.TFlex] = (-20, 30) }
            },
            new BodyVariant
            {
                Name = "Long, right leg 0.4", Source = "Long",
                SideMul = new Dictionary<string, (double, double)>
                {
                    ["hip"] = (1, 0.4), ["knee"] = (1, 0.4), ["ankle"] = (1, 0.4), ["toe"] = (1, 0.4)
                }
            },
            new BodyVariant { Name = "Long S2", Source = "Long", Strength = 2.0 },
        };

        public static BodyModel MakePrototype(string name)
        {
            foreach (var v in PrototypeBodies)
                if (v.Name == name) return Make(v);
            throw new ArgumentException("unknown prototype body " + name);
        }
    }
}
