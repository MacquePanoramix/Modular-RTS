// Reading the Python reference exports (reference/body_ref_*.json, written by reference/export_body.py).
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WonderGather.AnyBody.Tests
{
    public static class Ref
    {
        static string _dir;

        public static string Dir
        {
            get
            {
                if (_dir != null) return _dir;
                var d = new DirectoryInfo(AppContext.BaseDirectory);
                while (d != null)
                {
                    var c = Path.Combine(d.FullName, "reference", "body_ref_bodies.json");
                    if (File.Exists(c)) return _dir = Path.Combine(d.FullName, "reference");
                    d = d.Parent;
                }
                throw new FileNotFoundException("reference/body_ref_bodies.json not found above " + AppContext.BaseDirectory);
            }
        }

        public static JsonDocument Load(string name) => JsonDocument.Parse(File.ReadAllText(Path.Combine(Dir, name)));

        public static double[] Arr(JsonElement e)
        {
            var o = new double[e.GetArrayLength()];
            int i = 0;
            foreach (var v in e.EnumerateArray()) o[i++] = v.GetDouble();
            return o;
        }

        public static Vec3 V3(JsonElement e) => new Vec3(e[0].GetDouble(), e[1].GetDouble(), e[2].GetDouble());

        public static object[] Args(JsonElement e)
        {
            var o = new List<object>();
            foreach (var v in e.EnumerateArray())
            {
                switch (v.ValueKind)
                {
                    case JsonValueKind.Number: o.Add(v.GetDouble()); break;
                    case JsonValueKind.String: o.Add(v.GetString()); break;
                    case JsonValueKind.True: o.Add(true); break;
                    case JsonValueKind.False: o.Add(false); break;
                    default: throw new ArgumentException("arg kind " + v.ValueKind);
                }
            }
            return o.ToArray();
        }

        public static double? NumOrNull(JsonElement e, string prop)
        {
            if (!e.TryGetProperty(prop, out var v) || v.ValueKind == JsonValueKind.Null) return null;
            return v.GetDouble();
        }

        /// <summary>One saved fit (body_ref_cases.json).</summary>
        public sealed class Case
        {
            public string File, Body, Family, Stance, Worst;
            public object[] Args;
            public double Cost, ContactErrMm, PenetrationMm, SpMargin, H, M;
            public double? ReleaseMargin, R, Secs;
            public double[] Q, X0, Lo, Hi, RSaved, RX0, Com;
        }

        static List<Case> _cases;

        public static List<Case> Cases()
        {
            if (_cases != null) return _cases;
            using var doc = Load("body_ref_cases.json");
            var o = new List<Case>();
            foreach (var e in doc.RootElement.EnumerateArray())
            {
                o.Add(new Case
                {
                    File = e.GetProperty("file").GetString(), Body = e.GetProperty("body").GetString(),
                    Family = e.GetProperty("family").GetString(), Stance = e.GetProperty("stance").GetString(),
                    Args = Args(e.GetProperty("args")), Cost = e.GetProperty("cost").GetDouble(), Q = Arr(e.GetProperty("q")),
                    X0 = Arr(e.GetProperty("x0")), Lo = Arr(e.GetProperty("lo")), Hi = Arr(e.GetProperty("hi")),
                    RSaved = Arr(e.GetProperty("r_saved")), RX0 = Arr(e.GetProperty("r_x0")),
                    ContactErrMm = e.GetProperty("contact_err_mm").GetDouble(), PenetrationMm = e.GetProperty("penetration_mm").GetDouble(),
                    SpMargin = e.GetProperty("sp_margin").GetDouble(), ReleaseMargin = NumOrNull(e, "release_sp_margin"),
                    R = NumOrNull(e, "r"), Worst = e.GetProperty("worst").ValueKind == JsonValueKind.String ? e.GetProperty("worst").GetString() : null,
                    Secs = NumOrNull(e, "secs"), Com = Arr(e.GetProperty("com")), H = e.GetProperty("H").GetDouble(), M = e.GetProperty("M").GetDouble()
                });
            }
            return _cases = o;
        }
    }
}
