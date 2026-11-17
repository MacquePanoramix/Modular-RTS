using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WonderGather.Tests
{
    // The two models of the Ordinary Place exported again (November 17, 2026): a dump of each FBX's import, to diff
    // the old file against the new, and a still of the place before and after.
    //   -fbxDumpOut <file.txt>   where the import dump is written (Dump)
    //   -fbxStillOut <prefix>    where the stills are written, as <prefix>_<view>.ppm (Still)
    // Both are Explicit: they run only when named.
    public sealed class FbxReexportCheck
    {
        static readonly string[] Models =
        {
            "Assets/_WonderGather/Art/OrdinaryPlace/House.fbx",
            "Assets/_WonderGather/Art/OrdinaryPlace/Nature.fbx",
        };

        static string F(float v) => v.ToString("0.#####", CultureInfo.InvariantCulture);
        static string V(Vector3 v) => "(" + F(v.x) + "," + F(v.y) + "," + F(v.z) + ")";
        static string Q(Quaternion q) => "(" + F(q.x) + "," + F(q.y) + "," + F(q.z) + "," + F(q.w) + ")";

#if UNITY_EDITOR
        [Test, Explicit]
        public void Dump()
        {
            string output = CaptureTools.Argument("-fbxDumpOut");
            Assert.That(output, Is.Not.Null.And.Not.Empty, "Give -fbxDumpOut <file>.");
            var text = new StringBuilder();
            var guids = new Dictionary<string, string>();
            var ids = new Dictionary<string, HashSet<long>>();
            foreach (string path in Models)
            {
                string guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
                guids[guid] = path;
                // 100100000 is the id the model prefab (the file as one asset) has always had in references; the first
                // run of this dump listed it as missing, so it is counted by name.
                ids[guid] = new HashSet<long> { 100100000 };
                text.AppendLine("file " + path + " guid=" + guid);
                var lines = new List<string>();
                foreach (var asset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset == null) { lines.Add("  asset <null>"); continue; }
                    UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string g, out long id);
                    ids[guid].Add(id);
                    string line = "  asset " + asset.GetType().Name + " '" + asset.name + "' id=" + id;
                    if (asset is Mesh mesh)
                    {
                        line += " verts=" + mesh.vertexCount + " submeshes=" + mesh.subMeshCount + " bounds=" + V(mesh.bounds.center) + V(mesh.bounds.size);
                        var sub = new List<string>();
                        for (int s = 0; s < mesh.subMeshCount; s++) sub.Add(mesh.GetSubMesh(s).indexCount.ToString(CultureInfo.InvariantCulture));
                        line += " indices=[" + string.Join(",", sub) + "]";
                        var positions = new List<Vector3>();
                        mesh.GetVertices(positions);
                        var normals = new List<Vector3>();
                        mesh.GetNormals(normals);
                        var uvs = new List<Vector2>();
                        mesh.GetUVs(0, uvs);
                        var idx = new List<string>();
                        for (int s = 0; s < mesh.subMeshCount; s++) idx.Add(string.Join(",", mesh.GetIndices(s)));
                        line += " indexHash=" + Hash(idx);
                        line += " posHash=" + Hash(positions.Select(V)) + " normalHash=" + Hash(normals.Select(V)) + " uvHash=" + Hash(uvs.Select(u => F(u.x) + "," + F(u.y)));
                    }
                    if (asset is Material material)
                    {
                        var held = new List<string>();
                        foreach (string property in material.GetTexturePropertyNames())
                        {
                            var held1 = material.GetTexture(property);
                            if (held1 != null) held.Add(property + "=" + held1.name);
                        }
                        line += " shader=" + (material.shader != null ? material.shader.name : "none") + " mainTex=" + (material.mainTexture != null ? material.mainTexture.name : "none") + " textures=[" + string.Join(",", held) + "] color=" + material.color;
                    }
                    lines.Add(line);
                }
                lines.Sort(System.StringComparer.Ordinal);
                foreach (var line in lines) text.AppendLine(line);
                var root = UnityEditor.AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                text.AppendLine("  hierarchy");
                if (root != null) Hierarchy(root.transform, 2, text);
            }
            // Which of those IDs the project's scenes, prefabs and other assets reference.
            var reference = new Regex(@"fileID: (-?\d+), guid: ([0-9a-f]{32})");
            var found = new SortedSet<string>(System.StringComparer.Ordinal);
            int lost = 0;
            foreach (string file in Directory.GetFiles("Assets", "*.*", SearchOption.AllDirectories))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".unity" && ext != ".prefab" && ext != ".asset" && ext != ".mat" && ext != ".controller") continue;
                string body = File.ReadAllText(file);
                foreach (var g in guids.Keys)
                {
                    if (!body.Contains(g)) continue;
                    foreach (Match m in reference.Matches(body))
                    {
                        if (m.Groups[2].Value != g) continue;
                        long id = long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                        bool present = ids[g].Contains(id);
                        if (!present) lost++;
                        found.Add("ref " + file.Replace('\\', '/') + " -> " + Path.GetFileName(guids[g]) + " guid=" + g + " id=" + id + (present ? " present" : " MISSING"));
                    }
                }
            }
            foreach (var line in found) text.AppendLine(line);
            text.AppendLine("references=" + found.Count + " missing=" + lost);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            File.WriteAllText(output, text.ToString());
            Debug.Log("FBXDUMP written " + output + " references=" + found.Count + " missing=" + lost);
            Assert.That(lost, Is.EqualTo(0), "Some references point at IDs the import no longer has.");
        }

        static void Hierarchy(Transform t, int depth, StringBuilder text)
        {
            string pad = new string(' ', depth * 2);
            var parts = new List<string> { t.name, "pos=" + V(t.localPosition), "rot=" + Q(t.localRotation), "scale=" + V(t.localScale) };
            var filter = t.GetComponent<MeshFilter>();
            if (filter != null) parts.Add("mesh=" + (filter.sharedMesh != null ? filter.sharedMesh.name : "none"));
            var renderer = t.GetComponent<Renderer>();
            if (renderer != null) parts.Add("materials=[" + string.Join(",", renderer.sharedMaterials.Select(m => m != null ? m.name : "none")) + "]");
            text.AppendLine(pad + string.Join(" ", parts));
            foreach (Transform child in t) Hierarchy(child, depth + 1, text);
        }
#endif

        static string Hash(IEnumerable<string> items)
        {
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(string.Join(";", items));
                return System.BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").Substring(0, 16).ToLowerInvariant();
            }
        }

        [UnityTest, Explicit, Timeout(600000)]
        public IEnumerator Still()
        {
            string prefix = CaptureTools.Argument("-fbxStillOut");
            Assert.That(prefix, Is.Not.Null.And.Not.Empty, "Give -fbxStillOut <prefix>.");
            Directory.CreateDirectory(Path.GetDirectoryName(prefix));
            Random.InitState(1);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            var time = Object.FindAnyObjectByType<TimeOfDay>();
            // The wind moves the grass and the foliage by the clock (Time.time at the moment the picture is taken
            // differs from run to run: two runs of the same files differed in a third of their pixels). Its strength
            // is set to zero here so that a still of the same files is the same still.
            if (time != null)
                typeof(TimeOfDay).GetField("wind", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(time, new Vector3(.8f, .55f, 0f));
            Vector3 door = Vector3.zero;
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (light.name.StartsWith("Door")) door = light.transform.position;
            for (int i = 0; i < 30; i++) yield return null;
            Time.timeScale = 0;
            try
            {
                foreach (var (view, hour, offset) in new[] { ("day", 14f, new Vector3(-12, 7, -16)), ("dusk", 19.2f, new Vector3(-12, 7, -16)), ("wide", 14f, new Vector3(30, 22, -45)) })
                {
                    if (time != null) time.Hour = hour;
                    camera.transform.SetPositionAndRotation(door + offset, Quaternion.LookRotation(door - (door + offset)));
                    yield return null;
                    CaptureTools.Render(camera, prefix + "_" + view, 960, 540);
                }
            }
            finally { Time.timeScale = 1; }
            Debug.Log("FBXSTILL windStrength=" + Shader.GetGlobalVector("_WG_Wind").z + " written " + prefix + " door at " + V(door) + " time " + (time != null));
        }
    }
}
