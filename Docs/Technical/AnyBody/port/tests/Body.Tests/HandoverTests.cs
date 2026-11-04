// The hand-over to the reserve LP: Statics.Build at each saved miner pose (res_miners.json and res_miners2.json, 252
// fits) against the StaticsProblem the prototype gives there (reference/reserve_cases.json, written by
// export_reserve.py from b3.fk + all_contacts + the joints' subtrees): mass, centre, every contact (point, normal, mu,
// link, other, kind, support) and every joint (origin, hinge, axis, capacity, subtree, its mass and moment, the
// contacts' signs), and the let-go subsets (release.keep).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace WonderGather.AnyBody.Tests
{
    public static class HandoverTests
    {
        public static void StaticsMatch()
        {
            Console.WriteLine("== handover: Statics.Build at the 252 saved miner poses against the prototype's LP input (reserve_cases.json)");
            string path = Path.Combine(Ref.Dir, "reserve_cases.json");
            if (!File.Exists(path))
            {
                Console.WriteLine("   (no reserve_cases.json: run reference/export_reserve.py)");
                return;
            }
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            string proto = Path.GetFullPath(Path.Combine(Ref.Dir, "..", "..", "proto"));
            var files = new Dictionary<string, JsonDocument>();
            int n = 0, nRel = 0, structBad = 0;
            double wCom = 0, wP = 0, wN = 0, wO = 0, wAx = 0, wCap = 0, wSm = 0, wSM = 0, wMass = 0;
            foreach (var e in doc.RootElement.GetProperty("cases").EnumerateArray())
            {
                string file = e.GetProperty("file").GetString();
                if (!files.TryGetValue(file, out var fd))
                    files[file] = fd = JsonDocument.Parse(File.ReadAllText(Path.Combine(proto, file)).Replace("-Infinity", "-1e308").Replace("Infinity", "1e308").Replace("NaN", "null"));
                var saved = fd.RootElement[e.GetProperty("index").GetInt32()];
                var q = Ref.Arr(saved.GetProperty("q"));
                string bn = e.GetProperty("body").GetString();
                var b = ExactTests.Body(bn);
                var st = StanceLibrary.Make(b, e.GetProperty("family").GetString(), Ref.Args(e.GetProperty("args")));
                string tag = bn + " / " + st.Name;
                var p = Statics.Build(b, q, st.Supports);
                wMass = Math.Max(wMass, Math.Abs(p.Mass - e.GetProperty("mass").GetDouble()));
                wCom = Math.Max(wCom, (p.Com - Ref.V3(e.GetProperty("com"))).Length);
                var cs = e.GetProperty("contacts");
                if (!Check.That(cs.GetArrayLength() == p.Contacts.Length, $"{tag}: {p.Contacts.Length} contacts, want {cs.GetArrayLength()}")) { structBad++; continue; }
                int i = 0;
                foreach (var c in cs.EnumerateArray())
                {
                    var pc = p.Contacts[i++];
                    wP = Math.Max(wP, (pc.Point - Ref.V3(c.GetProperty("p"))).Length);
                    wN = Math.Max(wN, (pc.Normal - Ref.V3(c.GetProperty("n"))).Length);
                    bool same = pc.Mu == c.GetProperty("mu").GetDouble() && pc.Link == c.GetProperty("link").GetInt32() &&
                                pc.Other == c.GetProperty("other").GetInt32() && pc.Kind.ToString() == c.GetProperty("kind").GetString() &&
                                pc.Support == c.GetProperty("support").GetInt32();
                    if (!Check.That(same, $"{tag}: contact {i - 1} mu/link/other/kind/support")) structBad++;
                }
                int j = 0;
                foreach (var jt in e.GetProperty("joints").EnumerateArray())
                {
                    var pj = p.Joints[j++];
                    bool same = pj.Name == jt.GetProperty("name").GetString() && pj.IsHinge == jt.GetProperty("hinge").GetBoolean() &&
                                pj.SubtreeMask == jt.GetProperty("mask").GetInt32();
                    var sg = jt.GetProperty("sign").EnumerateArray().Select(x => x.GetInt32()).ToArray();
                    same &= sg.Length == pj.ContactSign.Length && sg.Select((v, k) => v == pj.ContactSign[k]).All(x => x);
                    if (!Check.That(same, $"{tag}: joint {pj.Name} name/hinge/mask/signs")) structBad++;
                    wO = Math.Max(wO, (pj.Origin - Ref.V3(jt.GetProperty("origin"))).Length);
                    if (pj.IsHinge) wAx = Math.Max(wAx, (pj.Axis - Ref.V3(jt.GetProperty("axis"))).Length);
                    wCap = Math.Max(wCap, Math.Abs(pj.Capacity - jt.GetProperty("cap").GetDouble()) / Math.Max(1.0, pj.Capacity));
                    wSm = Math.Max(wSm, Math.Abs(pj.SubMass - jt.GetProperty("subMass").GetDouble()));
                    wSM = Math.Max(wSM, (pj.SubMoment - Ref.V3(jt.GetProperty("subMoment"))).Length);
                }
                // the let-go: the supports of the dropped kinds left out = the full problem's contacts at release.keep
                var rel = e.GetProperty("release");
                if (rel.ValueKind == JsonValueKind.Object)
                {
                    var drop = new HashSet<string>(rel.GetProperty("drop").EnumerateArray().Select(x => x.GetString()));
                    var keep = rel.GetProperty("keep").EnumerateArray().Select(x => x.GetInt32()).ToArray();
                    var p2 = Statics.Build(b, q, st.Supports, s => !drop.Contains(s.Kind.ToString()));
                    bool same = p2.Contacts.Length == keep.Length;
                    for (int k = 0; same && k < keep.Length; k++)
                        same &= (p2.Contacts[k].Point - p.Contacts[keep[k]].Point).Length < 1e-12 && p2.Contacts[k].Link == p.Contacts[keep[k]].Link;
                    if (!Check.That(same, $"{tag}: let go of {string.Join("+", drop)}: the kept contacts")) structBad++;
                    nRel++;
                }
                n++;
            }
            foreach (var fd in files.Values) fd.Dispose();
            Console.WriteLine($"   {n} poses ({nRel} let-go subsets): max |diff| mass {wMass:E1} kg, centre {wCom:E1} m, contact points {wP:E1} m, " +
                              $"normals {wN:E1}, joint origins {wO:E1} m, hinge axes {wAx:E1}, capacities {wCap:E1} (rel), subtree masses {wSm:E1} kg, " +
                              $"subtree moments {wSM:E1} kg m; {structBad} structural mismatches");
            Check.That(wMass < 1e-9 && wCom < 1e-12 && wP < 1e-12 && wN < 1e-12 && wO < 1e-12 && wAx < 1e-12, "Statics.Build geometry within 1e-12");
            Check.That(wCap < 1e-12 && wSm < 1e-9 && wSM < 1e-9, "Statics.Build capacities and subtrees within 1e-9");
        }
    }
}
