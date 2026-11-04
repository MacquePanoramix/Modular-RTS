// The pose vector: b3.VARS (34 entries; radians, and metres for x, y, z).
using System;

namespace WonderGather.AnyBody
{
    public static class Pose
    {
        public const int X = 0, Y = 1, Z = 2, Yaw = 3, Pitch = 4, Roll = 5, TFlex = 6, TLat = 7, TTw = 8, NFlex = 9, NLat = 10,
            NRot = 11, ShFlexL = 12, ShAbdL = 13, ShRotL = 14, ElbL = 15, ShFlexR = 16, ShAbdR = 17, ShRotR = 18, ElbR = 19,
            HFlexL = 20, HAbdL = 21, HRotL = 22, KneeL = 23, HFlexR = 24, HAbdR = 25, HRotR = 26, KneeR = 27,
            DorsiL = 28, InvL = 29, DorsiR = 30, InvR = 31, ToeL = 32, ToeR = 33;

        public const int Count = 34;

        public static readonly string[] Names =
        {
            "x", "y", "z", "yaw", "pitch", "roll", "tflex", "tlat", "ttw", "nflex", "nlat", "nrot",
            "shflexL", "shabdL", "shrotL", "elbL", "shflexR", "shabdR", "shrotR", "elbR",
            "hflexL", "habdL", "hrotL", "kneeL", "hflexR", "habdR", "hrotR", "kneeR",
            "dorsiL", "invL", "dorsiR", "invR", "toeL", "toeR"
        };

        public static int Index(string name)
        {
            int i = Array.IndexOf(Names, name);
            if (i < 0) throw new ArgumentException("unknown pose variable " + name);
            return i;
        }

        /// <summary>x, y, z are lengths; every other variable is an angle.</summary>
        public static bool IsLength(int i) => i <= Z;

        /// <summary>The root's position and orientation (no comfort term).</summary>
        public static bool IsRoot(int i) => i <= Roll;

        /// <summary>b3.rkey: the range key of a variable.</summary>
        public static RangeKey RangeOf(int i)
        {
            switch (i)
            {
                case Yaw: return RangeKey.Yaw;
                case Pitch: return RangeKey.Pitch;
                case Roll: return RangeKey.Roll;
                case TFlex: return RangeKey.TFlex;
                case TLat: return RangeKey.TLat;
                case TTw: return RangeKey.TTw;
                case NFlex: return RangeKey.NFlex;
                case NLat: return RangeKey.NLat;
                case NRot: return RangeKey.NRot;
                case ShFlexL: case ShFlexR: return RangeKey.ShFlex;
                case ShAbdL: case ShAbdR: return RangeKey.ShAbd;
                case ShRotL: case ShRotR: return RangeKey.ShRot;
                case ElbL: case ElbR: return RangeKey.Elb;
                case HFlexL: case HFlexR: return RangeKey.HFlex;
                case HAbdL: case HAbdR: return RangeKey.HAbd;
                case HRotL: case HRotR: return RangeKey.HRot;
                case KneeL: case KneeR: return RangeKey.Knee;
                case DorsiL: case DorsiR: return RangeKey.Dorsi;
                case InvL: case InvR: return RangeKey.Inv;
                case ToeL: case ToeR: return RangeKey.Toe;
                default: throw new ArgumentException("no range for " + Names[i]);
            }
        }

        public const double D2R = Math.PI / 180.0;

        /// <summary>A pose in the prototype's units for display: degrees for angles, metres for x, y, z.</summary>
        public static double Display(int i, double v) => IsLength(i) ? v : v / D2R;
    }
}
