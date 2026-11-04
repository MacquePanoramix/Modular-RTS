// reference/reserve_cases.json (export_reserve.py) read into StaticsProblems and the prototype's answers.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WonderGather.AnyBody.Tests
{
    sealed class ReserveCase
    {
        public string File, Body, Family, Stance, Args;
        public int Index;
        public double Cost, H;
        public StaticsProblem P;
        public int[] ReleaseKeep; // null: no let-go check

        // run3.job as saved in res_*.json (null r: cannot be held)
        public double SavedR, SavedHands, SavedThighs, SavedFr, SavedMargin, SavedReleaseR, SavedReleaseMargin;
        public string SavedWorst;

        // the prototype's code now, at the saved pose (HiGHS default = dual simplex)
        public double PyS, PyR, PyHands, PyThighs, PyFr, PyMargin, PyReleaseR, PyReleaseMargin, PyLpMs, PyFrMs;
        public string PyWorst;
        public Vec3[] PyForces;
        public (double x, double y)[] PyFrPts;
        public Dictionary<string, double> PyPer;

        // HiGHS interior point (crossover) at the same pose
        public double IpmR, IpmHands;
        public string IpmWorst;

        public bool Golden => File == "res_miners2.json";
        public string Name => Body + " | " + Stance;

        static double D(JsonElement e, string name, double ifNull = double.PositiveInfinity)
        {
            if (!e.TryGetProperty(name, out var v) || v.ValueKind == JsonValueKind.Null) return ifNull;
            return v.GetDouble();
        }

        static string S(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        static Vec3 V(JsonElement e)
        {
            var a = new double[3];
            int i = 0;
            foreach (var v in e.EnumerateArray()) a[i++] = v.GetDouble();
            return new Vec3(a[0], a[1], a[2]);
        }

        public static List<ReserveCase> Load(string path)
        {
            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(path));
            var root = doc.RootElement;
            double g = root.GetProperty("gravity").GetDouble();
            if (g != StaticsProblem.Gravity) throw new InvalidDataException("gravity differs");
            var list = new List<ReserveCase>();
            foreach (var c in root.GetProperty("cases").EnumerateArray())
            {
                var rc = new ReserveCase
                {
                    File = c.GetProperty("file").GetString(),
                    Index = c.GetProperty("index").GetInt32(),
                    Body = c.GetProperty("body").GetString(),
                    Family = c.GetProperty("family").GetString(),
                    Stance = c.GetProperty("stance").GetString(),
                    Args = c.GetProperty("args").GetRawText(),
                    Cost = c.GetProperty("cost").GetDouble(),
                    H = c.GetProperty("H").GetDouble()
                };
                var contacts = new List<ContactPoint>();
                foreach (var e in c.GetProperty("contacts").EnumerateArray())
                {
                    contacts.Add(new ContactPoint
                    {
                        Point = V(e.GetProperty("p")),
                        Normal = V(e.GetProperty("n")),
                        Mu = e.GetProperty("mu").GetDouble(),
                        Link = e.GetProperty("link").GetInt32(),
                        Other = e.GetProperty("other").GetInt32(),
                        Kind = Enum.Parse<SupportKind>(e.GetProperty("kind").GetString()),
                        Support = e.GetProperty("support").GetInt32()
                    });
                }

                var joints = new List<JointLoad>();
                foreach (var e in c.GetProperty("joints").EnumerateArray())
                {
                    var sg = new List<sbyte>();
                    foreach (var v in e.GetProperty("sign").EnumerateArray()) sg.Add((sbyte)v.GetInt32());
                    joints.Add(new JointLoad
                    {
                        Name = e.GetProperty("name").GetString(),
                        Origin = V(e.GetProperty("origin")),
                        IsHinge = e.GetProperty("hinge").GetBoolean(),
                        Axis = V(e.GetProperty("axis")),
                        Capacity = e.GetProperty("cap").GetDouble(),
                        SubtreeMask = e.GetProperty("mask").GetInt32(),
                        SubMass = e.GetProperty("subMass").GetDouble(),
                        SubMoment = V(e.GetProperty("subMoment")),
                        ContactSign = sg.ToArray()
                    });
                }

                rc.P = new StaticsProblem
                {
                    Mass = c.GetProperty("mass").GetDouble(),
                    Com = V(c.GetProperty("com")),
                    Contacts = contacts.ToArray(),
                    Joints = joints.ToArray()
                };
                var rel = c.GetProperty("release");
                if (rel.ValueKind != JsonValueKind.Null)
                {
                    var keep = new List<int>();
                    foreach (var v in rel.GetProperty("keep").EnumerateArray()) keep.Add(v.GetInt32());
                    rc.ReleaseKeep = keep.ToArray();
                    rc.ReleaseDrop = new HashSet<SupportKind>();
                    foreach (var v in rel.GetProperty("drop").EnumerateArray()) rc.ReleaseDrop.Add(Enum.Parse<SupportKind>(v.GetString()));
                }

                var sv = c.GetProperty("saved");
                rc.SavedR = D(sv, "r");
                rc.SavedWorst = S(sv, "worst");
                rc.SavedHands = D(sv, "hands_share", 0);
                rc.SavedThighs = D(sv, "on_thighs", 0);
                rc.SavedFr = D(sv, "fr_depth", -1);
                rc.SavedMargin = D(sv, "sp_margin", -1);
                rc.SavedReleaseR = D(sv, "release_r");
                rc.SavedReleaseMargin = D(sv, "release_sp_margin", double.NaN);
                var py = c.GetProperty("py");
                rc.PyS = D(py, "s");
                rc.PyR = D(py, "r");
                rc.PyWorst = S(py, "worst");
                rc.PyHands = D(py, "hands_share", 0);
                rc.PyThighs = D(py, "on_thighs", 0);
                rc.PyFr = D(py, "fr_depth", -1);
                rc.PyMargin = D(py, "sp_margin", -1);
                rc.PyReleaseR = D(py, "release_r");
                rc.PyReleaseMargin = D(py, "release_sp_margin", double.NaN);
                rc.PyLpMs = D(py, "lp_ms", double.NaN);
                rc.PyFrMs = D(py, "fr_ms", double.NaN);
                if (py.TryGetProperty("f", out var fs))
                {
                    var fl = new List<Vec3>();
                    foreach (var v in fs.EnumerateArray()) fl.Add(V(v));
                    rc.PyForces = fl.ToArray();
                }

                if (py.TryGetProperty("fr_pts", out var fp))
                {
                    var pl = new List<(double, double)>();
                    foreach (var v in fp.EnumerateArray())
                    {
                        var a = new List<double>();
                        foreach (var w in v.EnumerateArray()) a.Add(w.GetDouble());
                        pl.Add((a[0], a[1]));
                    }

                    rc.PyFrPts = pl.ToArray();
                }

                if (py.TryGetProperty("per", out var per))
                {
                    rc.PyPer = new Dictionary<string, double>();
                    foreach (var kv in per.EnumerateObject()) rc.PyPer[kv.Name] = kv.Value.GetDouble();
                }

                var ipm = py.GetProperty("alt").GetProperty("highs-ipm");
                rc.IpmR = D(ipm, "r");
                rc.IpmWorst = S(ipm, "worst");
                rc.IpmHands = D(ipm, "hands_share", 0);
                list.Add(rc);
            }

            return list;
        }

        /// <summary>The same pose with the let-go's support kinds dropped (run3's "if a support let go now").</summary>
        public StaticsProblem Released()
        {
            var q = P.Subset(c => !ReleaseDrop.Contains(c.Kind));
            if (q.Contacts.Length != ReleaseKeep.Length) throw new InvalidDataException("let-go subset differs from the export");
            return q;
        }

        public HashSet<SupportKind> ReleaseDrop;
    }
}
