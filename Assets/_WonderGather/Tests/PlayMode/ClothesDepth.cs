using System.Collections.Generic;
using UnityEngine;

namespace WonderGather.Tests
{
    // How far a thing that hangs on a miner has gone into its clothes: read from the miner's own skin as it is posed,
    // with nothing of how the game stops the thing (so it can judge that). For each point of the thing's skin, a line
    // is drawn from the line of the leg on its side (the pelvis, the hip, the knee) out through that point. If the
    // body's skin crosses that line further out than the point (by no more than a hand's breadth: the clothes there,
    // not a far limb), the point is under the clothes, by that much. Arms, hands, what hangs on the body and its
    // straps are not the body here. Ask it after the body is posed (AfterEverything).
    // (The game's skins are not readable: which of the skin is whose is found by moving a bone and seeing what goes
    // with it.)
    public sealed class ClothesDepth
    {
        private const float Beyond = .1f, Near = .3f;
        // Every so many of the thing's points is read (the measure is slow).
        public int Stride = 3;
        private Vector3[] centres = new Vector3[0];
        private float[] reach = new float[0];
        private readonly SkinnedMeshRenderer skin;
        private readonly Mesh baked = new Mesh();
        private readonly MinerBody miner;
        private readonly int side;
        private readonly int[] points, corners;
        // Which part of the body each of the body's triangles is of: 0 the hips (and the coat over them), 1 the coat's
        // skirt, 2 a leg (the thigh down). And, for the last question asked, how deep the thing was in each.
        private readonly int[] part;
        public static readonly string[] Parts = { "hips", "skirt", "leg" };
        public readonly float[] DeepIn = new float[3];
        public readonly int[] PointsIn = new int[3];
        // Of the thing's points above where it hangs from (the ends of its strap, folded through its rings), how deep the
        // deepest was: they are told apart, as a thing turned rigidly about its rings swings them the other way.
        public float DeepAbove;
        public Vector3 HangsFrom, HangsWay = Vector3.down;
        private readonly List<Vector3> at = new List<Vector3>();
        private readonly List<int> close = new List<int>();
        private Vector3[] world;

        public int Points => points.Length;
        // As last read (Deepest): where each of the thing's points is, and the body's triangles near a place (their
        // corners, and which part of the body each is of).
        public Vector3 Point(int n) => world[points[n]];
        public void NearBy(Vector3 place, float within, List<Vector3> triangles, List<int> parts)
        {
            triangles.Clear();
            parts.Clear();
            for (int c = 0; c + 2 < corners.Length; c += 3)
            {
                Vector3 ca = world[corners[c]];
                if ((ca - place).sqrMagnitude > within * within) continue;
                triangles.Add(ca); triangles.Add(world[corners[c + 1]]); triangles.Add(world[corners[c + 2]]);
                parts.Add(part[c / 3]);
            }
        }
        // Which way the skin's faces are turned, by the order of their corners (found on the body as it stands: 1 or -1).
        public float Facing = 1;
        public int Triangles => corners.Length / 3;

        // Which of the skin's points go (by at least `share` of it) with a move of these bones.
        private bool[] Moves(IEnumerable<Transform> moved, float share)
        {
            var before = new List<Vector3>();
            skin.BakeMesh(baked, true);
            baked.GetVertices(before);
            var kept = new List<(Transform t, Vector3 p)>();
            foreach (var t in moved) if (t != null) { kept.Add((t, t.position)); }
            foreach (var (t, p) in kept) t.position = p + Vector3.up;
            skin.BakeMesh(baked, true);
            var after = new List<Vector3>();
            baked.GetVertices(after);
            for (int i = kept.Count - 1; i >= 0; i--) kept[i].t.position = kept[i].p;
            var goes = new bool[before.Count];
            Vector3 up = Quaternion.Inverse(skin.transform.rotation) * Vector3.up;
            for (int v = 0; v < before.Count; v++) goes[v] = Vector3.Dot(after[v] - before[v], up) > share;
            return goes;
        }

        public ClothesDepth(MinerBody miner, int thing)
        {
            this.miner = miner;
            skin = miner.GetComponentsInChildren<SkinnedMeshRenderer>(true)[0];
            var bone = miner.Things[thing].bone;
            var rig = miner.Rig;
            side = Vector3.Dot(bone.position - rig.pelvis.position, miner.transform.right) < 0 ? 0 : 1;
            // Only the top of each part that is not the body is moved (what is below it goes with it). The body here is
            // the hips and legs and what hangs from the hips (the coat's skirt): the trunk, the arms and the head are
            // left out, so that a thing caught between a thigh and the chest (curled up) is not counted as in a coat.
            var notBody = new List<Transform>(rig.upperArms) { rig.spine };
            // (Nor the shins and feet: on hands and knees a shin folded under its thigh lies beyond what hangs by the
            // knee, and would be read as clothes over it.)
            notBody.AddRange(rig.shins);
            for (int k = 0; k < miner.Things.Count; k++) if (miner.Things[k].bone != null) notBody.Add(miner.Things[k].bone);
            foreach (var t in skin.bones) if (t != null && t.name.Contains("Strap")) notBody.Add(t);
            var mine = Moves(new[] { bone }, .99f);
            var away = Moves(notBody, .5f);
            var list = new List<int>();
            for (int v = 0; v < mine.Length; v++) if (mine[v]) list.Add(v);
            points = list.ToArray();
            skin.BakeMesh(baked, true);
            var tris = baked.triangles;
            list.Clear();
            for (int c = 0; c + 2 < tris.Length; c += 3)
                if (!away[tris[c]] && !away[tris[c + 1]] && !away[tris[c + 2]]) { list.Add(tris[c]); list.Add(tris[c + 1]); list.Add(tris[c + 2]); }
            corners = list.ToArray();
            var skirt = new List<Transform>();
            foreach (var t in skin.bones) if (t != null && t.name.StartsWith("Skirt")) skirt.Add(t);
            var inSkirt = Moves(skirt, .5f);
            var inLeg = Moves(rig.thighs, .5f);
            part = new int[corners.Length / 3];
            for (int c = 0; c + 2 < corners.Length; c += 3)
            {
                int s0 = corners[c], s1 = corners[c + 1], s2 = corners[c + 2];
                int skirts = (inSkirt[s0] ? 1 : 0) + (inSkirt[s1] ? 1 : 0) + (inSkirt[s2] ? 1 : 0);
                int legs = (inLeg[s0] ? 1 : 0) + (inLeg[s1] ? 1 : 0) + (inLeg[s2] ? 1 : 0);
                part[c / 3] = skirts >= 2 ? 1 : legs >= 2 ? 2 : 0;
            }
            // Which way the faces are turned: a line from the pelvis out through the hip on the thing's side leaves the
            // body's skin last through a face turned outwards.
            var v3 = new List<Vector3>();
            baked.GetVertices(v3);
            var toWorld = Matrix4x4.TRS(skin.transform.position, skin.transform.rotation, Vector3.one);
            Vector3 o = rig.pelvis.position, d = (miner.transform.right * (side == 0 ? -1 : 1)).normalized;
            float far = 0, turned = 0;
            for (int c = 0; c + 2 < corners.Length; c += 3)
            {
                Vector3 ca = toWorld.MultiplyPoint3x4(v3[corners[c]]), cb = toWorld.MultiplyPoint3x4(v3[corners[c + 1]]), cc = toWorld.MultiplyPoint3x4(v3[corners[c + 2]]);
                float t = Cross(o, d, ca, cb, cc);
                if (t > far && t < .5f) { far = t; turned = Vector3.Dot(Vector3.Cross(cb - ca, cc - ca), d); }
            }
            Facing = turned < 0 ? -1 : 1;
            Debug.Log($"CLOTHES_DEPTH {miner.name} {bone.name}: {points.Length} points of its own, {corners.Length / 3} triangles of the body; out through the hip the skin ends {far * 1000:F0} mm from the pelvis, its faces turned {(Facing > 0 ? "by the order of their corners" : "against it")}");
        }

        private static Vector3 OnSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float t = ab.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0;
            return a + ab * t;
        }

        // Möller and Trumbore: how far along the ray the triangle is crossed (or -1).
        private static float Cross(Vector3 o, Vector3 d, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 e1 = b - a, e2 = c - a, p = Vector3.Cross(d, e2);
            float det = Vector3.Dot(e1, p);
            if (Mathf.Abs(det) < 1e-12f) return -1;
            float inv = 1 / det;
            Vector3 s = o - a;
            float u = Vector3.Dot(s, p) * inv;
            if (u < 0 || u > 1) return -1;
            Vector3 q = Vector3.Cross(s, e1);
            float v = Vector3.Dot(d, q) * inv;
            if (v < 0 || u + v > 1) return -1;
            float t = Vector3.Dot(e2, q) * inv;
            return t > 0 ? t : -1;
        }

        // How far the deepest point of the thing is under the clothes (metres; 0: none is), how many of its points
        // are under them by more than `within`, where the deepest is, and how far below the ground its lowest point is
        // (given the ground's height there; negative: above it).
        public float Deepest(float within, out int under, out Vector3 deepestAt, System.Func<float, float, float> ground, out float belowGround)
        {
            skin.BakeMesh(baked, true);
            baked.GetVertices(at);
            var toWorld = Matrix4x4.TRS(skin.transform.position, skin.transform.rotation, Vector3.one);
            if (world == null || world.Length != at.Count) world = new Vector3[at.Count];
            for (int v = 0; v < at.Count; v++) world[v] = toWorld.MultiplyPoint3x4(at[v]);
            var rig = miner.Rig;
            Vector3 pelvis = rig.pelvis.position, hip = rig.thighs[side].position, knee = rig.shins[side].position;
            // The body's triangles near the thing.
            Vector3 middle = Vector3.zero;
            foreach (int v in points) middle += world[v];
            middle /= Mathf.Max(1, points.Length);
            close.Clear();
            for (int c = 0; c + 2 < corners.Length; c += 3)
                if ((world[corners[c]] - middle).sqrMagnitude < Near * Near) close.Add(c);
            if (centres.Length < close.Count) { centres = new Vector3[close.Count]; reach = new float[close.Count]; }
            for (int i = 0; i < close.Count; i++)
            {
                int c = close[i];
                Vector3 ca = world[corners[c]], cb = world[corners[c + 1]], cc = world[corners[c + 2]];
                centres[i] = (ca + cb + cc) / 3;
                reach[i] = Mathf.Sqrt(Mathf.Max((ca - centres[i]).sqrMagnitude, Mathf.Max((cb - centres[i]).sqrMagnitude, (cc - centres[i]).sqrMagnitude)));
            }
            float deepest = 0;
            under = 0;
            for (int i = 0; i < 3; i++) { DeepIn[i] = 0; PointsIn[i] = 0; }
            DeepAbove = 0;
            deepestAt = middle;
            belowGround = float.NegativeInfinity;
            for (int n = 0; n < points.Length; n += Mathf.Max(1, Stride))
            {
                int v = points[n];
                Vector3 p = world[v];
                if (ground != null) belowGround = Mathf.Max(belowGround, ground(p.x, p.z) - p.y);
                Vector3 a = OnSegment(p, pelvis, hip), b = OnSegment(p, hip, knee);
                Vector3 o = (a - p).sqrMagnitude < (b - p).sqrMagnitude ? a : b;
                Vector3 d = p - o;
                float r = d.magnitude;
                if (r < 1e-5f) continue;
                d /= r;
                // The furthest place within reach where the line leaves the body's skin (through a face turned outwards).
                float far = 0;
                int farPart = 0;
                Vector3 half = p + d * (Beyond * .5f);
                for (int i = 0; i < close.Count; i++)
                {
                    float within2 = Beyond * .5f + reach[i];
                    if ((centres[i] - half).sqrMagnitude > within2 * within2) continue;
                    int c = close[i];
                    Vector3 ca = world[corners[c]], cb = world[corners[c + 1]], cc = world[corners[c + 2]];
                    float t = Cross(o, d, ca, cb, cc);
                    if (t > r && t < r + Beyond && Vector3.Dot(Vector3.Cross(cb - ca, cc - ca), d) * Facing > 0 && t > far) { far = t; farPart = part[c / 3]; }
                }
                float depth = far > r ? far - r : 0;
                if (Vector3.Dot(p - HangsFrom, HangsWay) < 0) { DeepAbove = Mathf.Max(DeepAbove, depth); continue; }
                if (depth > within) { under++; PointsIn[farPart]++; }
                DeepIn[farPart] = Mathf.Max(DeepIn[farPart], depth);
                if (depth > deepest) { deepest = depth; deepestAt = p; }
            }
            return deepest;
        }
    }
}
