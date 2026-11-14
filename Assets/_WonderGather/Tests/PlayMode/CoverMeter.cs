using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather.Tests
{
    // The posed cover grid's meter (DrawnCoverGridTests, November 13 and 14, 2026), moved here unchanged so that a live
    // test can measure with the very same code (Docs/Research/2026-11-13_DetachedLeg/courses_plan.md, step (a)): the drawn
    // garments' skins baked by hand as the renderer bakes them, each with a solid on a layer of its own that touches
    // nothing; the trousers' cut ring found in the drawn body as the miner stands; the 25 ways it is looked at from; and
    // whether anything drawn lies between a ring point and an eye far off along each way. The ground counts as cover
    // either as a collider handed in (the grid's quad) or by its layer added to the mask (the terrain, in a live scene):
    // any collider hit that is not a garment's solid counts as cover.
    internal sealed class CoverMeter
    {
        public static readonly float[] Elevations = { 32, 47, 62 };
        public static readonly string[] Sides = { "feet", "feet and right", "right", "head and right", "head", "head and left", "left", "feet and left" };

        public sealed class Cover
        {
            public string Name;
            public SkinnedMeshRenderer Skin;
            public Mesh World;
            public MeshCollider Solid;
            public int[] SubEnd;
            public bool[] TwoSided;
            public int[] Tris;
            public Vector3[] Rest, Verts;
            public BoneWeight[] Weights;
            public Transform[] Bones;
            public Matrix4x4[] Bind, Pose;
            public int Limit = 4;
        }

        public struct Seen
        {
            public int Points, Open, OpenLoose, Views;
            public int[] ByWay;
        }

        // The cut ring as found: the drawn body (the biggest skin), the ring's points on each leg (0 left, 1 right), and the
        // trousers' triangles and the body's part they are drawn in.
        public sealed class Ring
        {
            public Cover Body;
            public List<int>[] Legs;
            public List<int> TrouserTris;
            public int TrouserSub;
        }

        public readonly List<Cover> Covers = new List<Cover>();
        public readonly Dictionary<Collider, Cover> CoverOf = new Dictionary<Collider, Cover>();
        private readonly List<GameObject> made = new List<GameObject>();
        private readonly bool[] layerWasIgnored = new bool[32];
        private bool layersSet;

        public int Layer = -1, Mask, Mismatches, FaceChecks;
        public readonly bool BackfacesWere;
        public MeshCollider GroundSolid;
        public Vector3[] Ways;
        public string[] WayNames;

        public CoverMeter() { BackfacesWere = Physics.queriesHitBackfaces; }

        // The garments' solids, on a layer of their own that touches nothing.
        public void TakeLayer()
        {
            for (int i = 31; i >= 8 && Layer < 0; i--) if (string.IsNullOrEmpty(LayerMask.LayerToName(i))) Layer = i;
            Assert.That(Layer, Is.GreaterThanOrEqualTo(0), "No spare layer for the garments' solids.");
            for (int i = 0; i < 32; i++) { layerWasIgnored[i] = Physics.GetIgnoreLayerCollision(Layer, i); Physics.IgnoreLayerCollision(Layer, i, true); }
            layersSet = true;
            Mask = 1 << Layer;
        }

        // The ground as cover by its own layer (a live scene's terrain): every collider on it hides what lies beyond.
        public void AddToMask(int layer) => Mask |= 1 << layer;

        // Everything made, and the layers as they were.
        public void End()
        {
            foreach (var g in made) if (g != null) UnityEngine.Object.Destroy(g);
            made.Clear();
            foreach (var c in Covers) if (c.World != null) UnityEngine.Object.Destroy(c.World);
            Covers.Clear();
            CoverOf.Clear();
            if (layersSet) for (int i = 0; i < 32; i++) Physics.IgnoreLayerCollision(Layer, i, layerWasIgnored[i]);
            layersSet = false;
        }

        // Drawn at its finest: the skins of the first level of detail, and those in no level at all.
        public void AddSkins(Component unit, StringBuilder log)
        {
            var first = new HashSet<Renderer>();
            var later = new HashSet<Renderer>();
            foreach (var g in unit.GetComponentsInChildren<LODGroup>(true))
            {
                var lods = g.GetLODs();
                for (int i = 0; i < lods.Length; i++) foreach (var r in lods[i].renderers) if (r != null) (i == 0 ? first : later).Add(r);
                g.ForceLOD(0);
            }
            log.AppendLine($"COVER_QUALITY {QualitySettings.names[QualitySettings.GetQualityLevel()]}: skin weights {QualitySettings.skinWeights}");
            foreach (var skin in unit.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (!skin.enabled || skin.sharedMesh == null || (later.Contains(skin) && !first.Contains(skin))) continue;
                var shared = skin.sharedMesh;
                var c = new Cover { Name = skin.name, Skin = skin, Bones = skin.bones, Bind = shared.bindposes };
                string read = ReadDrawn(shared, c);
                // The bones a point follows as the renderer draws it (the renderer's own setting, or the quality's).
                int most = skin.quality == SkinQuality.Auto ? (int)QualitySettings.skinWeights : (int)skin.quality;
                c.Limit = Mathf.Clamp(most, 1, 4);
                skin.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
                var go = new GameObject("Cover " + skin.name) { layer = Layer };
                made.Add(go);
                c.Solid = go.AddComponent<MeshCollider>();
                c.TwoSided = new bool[c.SubEnd.Length];
                var mats = skin.sharedMaterials;
                var sides = new StringBuilder();
                for (int s = 0; s < c.SubEnd.Length; s++)
                {
                    var m = mats.Length == 0 ? null : mats[Mathf.Min(s, mats.Length - 1)];
                    int cull = Cull(m);
                    c.TwoSided[s] = cull == 0;
                    sides.Append(s == 0 ? "" : "; ").Append(m == null ? "no material" : $"{m.name} ({(m.shader != null ? m.shader.name : "no shader")}, cull {(cull < 0 ? "not said" : cull.ToString(CultureInfo.InvariantCulture))}, {c.SubEnd[s] - (s == 0 ? 0 : c.SubEnd[s - 1])} triangles)");
                }
                Covers.Add(c);
                CoverOf[c.Solid] = c;
                log.AppendLine($"COVER_PART {skin.name}: mesh {shared.name}, {c.Rest.Length} vertices, {c.Tris.Length / 3} triangles, {c.Bones.Length} bones, up to {c.Limit} a point as drawn; {read}; {sides}");
            }
        }

        // The cut ring, found with the miner standing (feet: the unit's position; left and right: its right).
        public Ring FindRing(Transform unit, StringBuilder log)
        {
            // The drawn body is one mesh (rigging.py joins every part into it), so the trousers are found in it: the pieces
            // (whole once their seams are welded) that a thigh carries and that no bone outside the trousers' own moves.
            // rigging.py skins the trousers to the pelvis, the spine and the legs only; the skirt hangs from the pelvis and
            // its flaps' bones, the top from the trunk and the arms, the boots from the shins, the feet and the toes.
            Cover body = null;
            foreach (var c in Covers) if (body == null || c.Rest.Length > body.Rest.Length) body = c;
            Assert.That(body, Is.Not.Null, "Small has no drawn skin.");
            var tv = body.Rest;
            var tt = body.Tris;
            var weld = new Dictionary<Vector3Int, int>();
            var welded = new int[tv.Length];
            var rep = new List<int>();
            for (int i = 0; i < tv.Length; i++)
            {
                var key = new Vector3Int(Mathf.RoundToInt(tv[i].x * 1e4f), Mathf.RoundToInt(tv[i].y * 1e4f), Mathf.RoundToInt(tv[i].z * 1e4f));
                if (!weld.TryGetValue(key, out int w)) { w = rep.Count; weld[key] = w; rep.Add(i); }
                welded[i] = w;
            }
            var parent = new int[rep.Count];
            for (int i = 0; i < parent.Length; i++) parent[i] = i;
            int Root(int x) { while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; } return x; }
            void Join(int x, int y) { x = Root(x); y = Root(y); if (x != y) parent[y] = x; }
            for (int i = 0; i + 2 < tt.Length; i += 3) { Join(welded[tt[i]], welded[tt[i + 1]]); Join(welded[tt[i]], welded[tt[i + 2]]); }
            var bones = body.Bones;
            var weights = body.Weights;
            Assert.That(weights != null && weights.Length == tv.Length, Is.True, "Small's drawn body carries no bone weights:\n" + log);
            // Each bone by the names rigging.py gives them: 2 a thigh, 1 another of the trousers' bones, 0 any other.
            var kind = new int[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                string n = bones[i] != null ? bones[i].name : "";
                kind[i] = n.Contains("Thigh") ? 2 : n.Contains("Pelvis") || n.Contains("Spine") || n.Contains("Shin") || n.Contains("Foot") ? 1 : 0;
            }
            var pieceOf = new int[tv.Length];
            var size = new Dictionary<int, int>();
            var thighs = new Dictionary<int, float>();
            var foreign = new Dictionary<int, float>();
            var foreignBone = new Dictionary<int, string>();
            for (int i = 0; i < tv.Length; i++)
            {
                int r = Root(welded[i]);
                pieceOf[i] = r;
                size[r] = size.TryGetValue(r, out int s0) ? s0 + 1 : 1;
                var bw = weights[i];
                int[] bi = { bw.boneIndex0, bw.boneIndex1, bw.boneIndex2, bw.boneIndex3 };
                float[] wt = { bw.weight0, bw.weight1, bw.weight2, bw.weight3 };
                float th = 0, fo = 0;
                string foName = null;
                for (int k = 0; k < 4; k++)
                {
                    if (wt[k] <= .01f || bi[k] < 0 || bi[k] >= kind.Length) continue;
                    if (kind[bi[k]] == 2) th += wt[k];
                    else if (kind[bi[k]] == 0 && wt[k] > fo) { fo = wt[k]; foName = bones[bi[k]] != null ? bones[bi[k]].name : "?"; }
                }
                thighs[r] = Mathf.Max(thighs.TryGetValue(r, out float t0) ? t0 : 0, th);
                if (!foreign.TryGetValue(r, out float f0) || fo > f0) { foreign[r] = fo; if (foName != null) foreignBone[r] = foName; }
            }

            // The drawn body as its bones put it this frame, worked out by hand as the renderer works it out (checked against
            // the graphics card's own skin once the body is held still, below).
            Bake(body);

            // The pieces a thigh carries, as the miner stands; the trousers among them.
            Vector3 feet = unit.position;
            var pieceLow = new Dictionary<int, float>();
            var pieceHigh = new Dictionary<int, float>();
            for (int i = 0; i < tv.Length; i++)
            {
                int r = pieceOf[i];
                float h = body.Verts[i].y - feet.y;
                pieceLow[r] = Mathf.Min(pieceLow.TryGetValue(r, out float l0) ? l0 : 1e9f, h);
                pieceHigh[r] = Mathf.Max(pieceHigh.TryGetValue(r, out float h0) ? h0 : -1e9f, h);
            }
            var isTrousers = new HashSet<int>();
            int pieces = 0;
            foreach (var kv in size)
            {
                pieces++;
                if (thighs[kv.Key] <= 0) continue;
                bool its = kv.Value >= 200 && thighs[kv.Key] >= .25f && foreign[kv.Key] <= .01f;
                if (its) isTrousers.Add(kv.Key);
                log.AppendLine($"COVER_PIECE {kv.Value} points, {1000 * pieceLow[kv.Key]:F0} to {1000 * pieceHigh[kv.Key]:F0} mm above the feet; a thigh carries up to {thighs[kv.Key]:F2} of a point, other bones up to {foreign[kv.Key]:F2}{(foreignBone.TryGetValue(kv.Key, out string fb) ? " (" + fb + ")" : "")}: {(its ? "trousers" : "not the trousers")}");
            }
            log.AppendLine($"COVER_PIECES {pieces} pieces in the drawn body; {isTrousers.Count} of them the trousers");
            Assert.That(isTrousers.Count, Is.GreaterThan(0), "No piece of Small's drawn body is its trousers:\n" + log);

            // The ring where the trousers are cut: their open edges (an edge of one face only, once the seams are welded),
            // the highest band of them as the miner stands (the bottom of the trousers is closed in the boot).
            var trouserTris = new List<int>();
            int trouserSub = -1;
            for (int i = 0; i + 2 < tt.Length; i += 3)
                if (isTrousers.Contains(pieceOf[tt[i]]))
                {
                    if (trouserSub < 0) { trouserSub = 0; while (trouserSub < body.SubEnd.Length - 1 && i / 3 >= body.SubEnd[trouserSub]) trouserSub++; }
                    trouserTris.Add(tt[i]); trouserTris.Add(tt[i + 1]); trouserTris.Add(tt[i + 2]);
                }
            var edges = new Dictionary<long, int>();
            for (int i = 0; i + 2 < trouserTris.Count; i += 3)
                for (int e = 0; e < 3; e++)
                {
                    int a = welded[trouserTris[i + e]], bb = welded[trouserTris[i + (e + 1) % 3]];
                    if (a == bb) continue;
                    long key = a < bb ? ((long)a << 32) | (uint)bb : ((long)bb << 32) | (uint)a;
                    edges[key] = edges.TryGetValue(key, out int n) ? n + 1 : 1;
                }
            var openWelded = new List<int>();
            var openSet = new HashSet<int>();
            foreach (var kv in edges)
                if (kv.Value == 1)
                {
                    int a = (int)(kv.Key >> 32), bb = (int)(kv.Key & 0xffffffffL);
                    if (openSet.Add(a)) openWelded.Add(a);
                    if (openSet.Add(bb)) openWelded.Add(bb);
                }
            Assert.That(openWelded.Count, Is.GreaterThan(0), "Small's trousers have no open edge:\n" + log);
            float ringTop = -1e9f;
            foreach (int w in openWelded) ringTop = Mathf.Max(ringTop, body.Verts[rep[w]].y - feet.y);
            var ring = new[] { new List<int>(), new List<int>() };
            float ringLow = 1e9f;
            foreach (int w in openWelded)
            {
                int v = rep[w];
                float h = body.Verts[v].y - feet.y;
                if (h < ringTop - .06f) continue;
                ringLow = Mathf.Min(ringLow, h);
                ring[Vector3.Dot(body.Verts[v] - feet, unit.right) < 0 ? 0 : 1].Add(v);
            }
            log.AppendLine($"COVER_RING the trousers: {trouserTris.Count / 3} triangles (in the body's part {trouserSub}), {openWelded.Count} open points; the cut ring: {ring[0].Count} on the left leg, {ring[1].Count} on the right, {1000 * ringLow:F0} to {1000 * ringTop:F0} mm above the feet as it stands");
            Assert.That(ring[0].Count >= 8 && ring[1].Count >= 8, Is.True, "The cut ring was not found on both legs:\n" + log);
            return new Ring { Body = body, Legs = ring, TrouserTris = trouserTris, TrouserSub = trouserSub };
        }

        // The ways it is looked at from: the game camera's heights, on eight sides of the body, and straight above.
        public void SetWays(Quaternion facing)
        {
            var wl = new List<Vector3>();
            var wn = new List<string>();
            foreach (float e in Elevations)
                for (int y = 0; y < 8; y++)
                {
                    Vector3 flat = facing * Quaternion.Euler(0, 45 * y, 0) * Vector3.forward;
                    wl.Add((Mathf.Cos(e * Mathf.Deg2Rad) * flat + Mathf.Sin(e * Mathf.Deg2Rad) * Vector3.up).normalized);
                    wn.Add($"{e:F0} degrees down, from its {Sides[y]}");
                }
            wl.Add(Vector3.up);
            wn.Add("straight above");
            Ways = wl.ToArray();
            WayNames = wn.ToArray();
        }

        // Both legs' rings as drawn this frame: every garment baked, then each ring looked for (faces seen from behind
        // looked through, so the rays hit back faces while they look).
        public Seen[] MeasureBoth(Ring ring)
        {
            BakeAll();
            var seen = new Seen[2];
            Physics.queriesHitBackfaces = true;
            for (int s = 0; s < 2; s++) seen[s] = Look(ring.Body, ring.Legs[s]);
            Physics.queriesHitBackfaces = BackfacesWere;
            return seen;
        }

        public void BakeAll()
        {
            foreach (var c in Covers) Bake(c);
            Physics.SyncTransforms();
        }

        // A garment as drawn this frame, in the world, and its solid made from it (standing at the world's origin). The
        // skin is worked out by hand as the renderer works it out: each point carried by its bones (at most Limit of them,
        // the heaviest, their weights made whole again), each bone's way being its pose times its bind pose.
        public void Bake(Cover c)
        {
            int n = c.Rest.Length;
            if (c.Pose == null) c.Pose = new Matrix4x4[c.Bones.Length];
            for (int i = 0; i < c.Bones.Length; i++) c.Pose[i] = c.Bones[i] != null ? c.Bones[i].localToWorldMatrix * c.Bind[i] : Matrix4x4.zero;
            if (c.Verts == null || c.Verts.Length != n) c.Verts = new Vector3[n];
            var j = new int[4];
            var w = new float[4];
            for (int v = 0; v < n; v++)
            {
                var bw = c.Weights[v];
                j[0] = bw.boneIndex0; j[1] = bw.boneIndex1; j[2] = bw.boneIndex2; j[3] = bw.boneIndex3;
                w[0] = bw.weight0; w[1] = bw.weight1; w[2] = bw.weight2; w[3] = bw.weight3;
                // The heaviest first (an insertion sort of four).
                for (int a = 1; a < 4; a++)
                    for (int k = a; k > 0 && w[k] > w[k - 1]; k--) { (w[k], w[k - 1]) = (w[k - 1], w[k]); (j[k], j[k - 1]) = (j[k - 1], j[k]); }
                float sum = 0;
                for (int k = 0; k < c.Limit; k++) sum += w[k];
                Vector3 p = Vector3.zero;
                if (sum > 0)
                    for (int k = 0; k < c.Limit; k++)
                        if (w[k] > 0 && j[k] >= 0 && j[k] < c.Pose.Length) p += w[k] / sum * c.Pose[j[k]].MultiplyPoint3x4(c.Rest[v]);
                c.Verts[v] = p;
            }
            if (c.World == null)
            {
                c.World = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                c.World.MarkDynamic();
                c.World.vertices = c.Verts;
                c.World.triangles = c.Tris;
            }
            else
            {
                c.World.vertices = c.Verts;
                c.World.RecalculateBounds();
            }
            c.Solid.sharedMesh = null;
            c.Solid.sharedMesh = c.World;
        }

        // The drawn mesh's points, triangles and bone weights. The game's miners are imported without Read/Write (the
        // renderer keeps them on the graphics card), so they are read here through the Editor, which reads any mesh;
        // nothing of the game or of its import settings is changed for it.
        public static string ReadDrawn(Mesh mesh, Cover c)
        {
#if UNITY_EDITOR
            using (var all = UnityEditor.MeshUtility.AcquireReadOnlyMeshData(mesh))
            {
                var data = all[0];
                int n = data.vertexCount;
                using (var at = new NativeArray<Vector3>(n, Allocator.Temp))
                {
                    data.GetVertices(at);
                    c.Rest = at.ToArray();
                }
                c.SubEnd = new int[data.subMeshCount];
                var tris = new List<int>();
                var shapes = new StringBuilder();
                for (int s = 0; s < data.subMeshCount; s++)
                {
                    var sub = data.GetSubMesh(s);
                    if (sub.topology != MeshTopology.Triangles) shapes.Append($" part {s} is drawn as {sub.topology};");
                    using (var idx = new NativeArray<int>(sub.indexCount, Allocator.Temp))
                    {
                        data.GetIndices(idx, s, true);
                        tris.AddRange(idx);
                    }
                    c.SubEnd[s] = tris.Count / 3;
                }
                c.Tris = tris.ToArray();
                c.Weights = new BoneWeight[n];
                bool hasW = data.HasVertexAttribute(VertexAttribute.BlendWeight), hasJ = data.HasVertexAttribute(VertexAttribute.BlendIndices);
                if (!hasJ) return "no bone indices;" + shapes;
                int js = data.GetVertexAttributeStream(VertexAttribute.BlendIndices), jo = data.GetVertexAttributeOffset(VertexAttribute.BlendIndices);
                int jd = data.GetVertexAttributeDimension(VertexAttribute.BlendIndices);
                var jf = data.GetVertexAttributeFormat(VertexAttribute.BlendIndices);
                var jb = data.GetVertexData<byte>(js).ToArray();
                int jStride = data.GetVertexBufferStride(js);
                int ws = -1, wo = 0, wd = 0, wStride = 0;
                var wf = VertexAttributeFormat.Float32;
                byte[] wb = null;
                if (hasW)
                {
                    ws = data.GetVertexAttributeStream(VertexAttribute.BlendWeight);
                    wo = data.GetVertexAttributeOffset(VertexAttribute.BlendWeight);
                    wd = data.GetVertexAttributeDimension(VertexAttribute.BlendWeight);
                    wf = data.GetVertexAttributeFormat(VertexAttribute.BlendWeight);
                    wb = data.GetVertexData<byte>(ws).ToArray();
                    wStride = data.GetVertexBufferStride(ws);
                }
                float lowSum = float.MaxValue, highSum = 0;
                var w = new float[4];
                var j = new int[4];
                for (int v = 0; v < n; v++)
                {
                    for (int k = 0; k < 4; k++) { w[k] = 0; j[k] = 0; }
                    for (int k = 0; k < Mathf.Min(jd, 4); k++) j[k] = Mathf.RoundToInt(Value(jb, v * jStride + jo, k, jf));
                    if (hasW) for (int k = 0; k < Mathf.Min(wd, 4); k++) w[k] = Value(wb, v * wStride + wo, k, wf);
                    else w[0] = 1;
                    float sum = w[0] + w[1] + w[2] + w[3];
                    lowSum = Mathf.Min(lowSum, sum);
                    highSum = Mathf.Max(highSum, sum);
                    c.Weights[v] = new BoneWeight { boneIndex0 = j[0], boneIndex1 = j[1], boneIndex2 = j[2], boneIndex3 = j[3], weight0 = w[0], weight1 = w[1], weight2 = w[2], weight3 = w[3] };
                }
                return $"read through the Editor: bone indices {jd} x {jf}, weights {(hasW ? wd + " x " + wf : "none (one bone a point)")}, " +
                       $"each point's weights summing to {lowSum:F3}..{highSum:F3}, {mesh.blendShapeCount} blend shapes, {c.Bind.Length} bind poses;" + shapes;
            }
#else
            Assert.Ignore("Reads the drawn mesh through the Editor.");
            return null;
#endif
        }

        // One number of a vertex's attribute, in the attribute's own format.
        public static float Value(byte[] b, int at, int k, VertexAttributeFormat f)
        {
            switch (f)
            {
                case VertexAttributeFormat.Float32: return BitConverter.ToSingle(b, at + 4 * k);
                case VertexAttributeFormat.Float16: return Mathf.HalfToFloat(BitConverter.ToUInt16(b, at + 2 * k));
                case VertexAttributeFormat.UNorm8: return b[at + k] / 255f;
                case VertexAttributeFormat.SNorm8: return Mathf.Max(-1, (sbyte)b[at + k] / 127f);
                case VertexAttributeFormat.UNorm16: return BitConverter.ToUInt16(b, at + 2 * k) / 65535f;
                case VertexAttributeFormat.SNorm16: return Mathf.Max(-1, BitConverter.ToInt16(b, at + 2 * k) / 32767f);
                case VertexAttributeFormat.UInt8: return b[at + k];
                case VertexAttributeFormat.SInt8: return (sbyte)b[at + k];
                case VertexAttributeFormat.UInt16: return BitConverter.ToUInt16(b, at + 2 * k);
                case VertexAttributeFormat.SInt16: return BitConverter.ToInt16(b, at + 2 * k);
                case VertexAttributeFormat.UInt32: return BitConverter.ToUInt32(b, at + 4 * k);
                case VertexAttributeFormat.SInt32: return BitConverter.ToInt32(b, at + 4 * k);
                default: throw new NotSupportedException("A vertex attribute in " + f);
            }
        }

        // How much of a ring can be seen, from each way and from any.
        public Seen Look(Cover body, List<int> ring)
        {
            var seen = new Seen { Points = ring.Count, ByWay = new int[Ways.Length] };
            foreach (int v in ring)
            {
                Vector3 p = body.Verts[v];
                bool strict = false, loose = false;
                for (int w = 0; w < Ways.Length; w++)
                {
                    if (!Covered(p, Ways[w], out bool anyHit)) { seen.ByWay[w]++; seen.Views++; strict = true; }
                    if (!anyHit) loose = true;
                }
                if (strict) seen.Open++;
                if (loose) seen.OpenLoose++;
            }
            return seen;
        }

        // Whether anything drawn lies between a point and an eye far off along d: a face counts if it is drawn on both
        // sides or its front faces the eye (Unity's front faces turn clockwise to the eye); one seen from behind, when
        // its material draws only its front, is looked through.
        public bool Covered(Vector3 p, Vector3 d, out bool anyHit)
        {
            anyHit = false;
            Vector3 from = p + d * .002f;
            for (int n = 0; n < 32; n++)
            {
                if (!Physics.Raycast(from, d, out RaycastHit hit, 4f, Mask, QueryTriggerInteraction.Ignore)) return false;
                anyHit = true;
                if (hit.collider == GroundSolid || !CoverOf.TryGetValue(hit.collider, out Cover c) || hit.triangleIndex < 0 || 3 * hit.triangleIndex + 2 >= c.Tris.Length) return true;
                int t = hit.triangleIndex, s = 0;
                while (s < c.SubEnd.Length - 1 && t >= c.SubEnd[s]) s++;
                Vector3 a = c.Verts[c.Tris[3 * t]], b = c.Verts[c.Tris[3 * t + 1]], e = c.Verts[c.Tris[3 * t + 2]];
                Vector3 face = Vector3.Cross(b - a, e - a);
                if (face.sqrMagnitude > 1e-14f)
                {
                    FaceChecks++;
                    if (Mathf.Abs(Vector3.Dot(hit.point - a, face.normalized)) > .001f) Mismatches++;
                }
                if (c.TwoSided[s] || Vector3.Dot(face, d) > 0) return true;
                from = hit.point + d * .0005f;
            }
            return true;
        }

        // A material's culling: 0 draws both sides, 2 the front only (URP's _Cull); -1 when the material does not say.
        public static int Cull(Material m)
        {
            if (m == null) return -1;
            if (m.HasProperty("_Cull")) return Mathf.RoundToInt(m.GetFloat("_Cull"));
            if (m.HasProperty("_CullMode")) return Mathf.RoundToInt(m.GetFloat("_CullMode"));
            return -1;
        }
    }
}
