using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // (c)(a), November 13, 2026 (afternoon). Luis saw a leg looking detached while Small got up by its own body. The
    // investigation's first candidate (Docs/Research/2026-11-13_DetachedLeg/investigation.md): Small's trousers are cut
    // off at 0.486 m, under the skirt, and a thigh that leaves the skirt's cover shows the open ring. Two routes there:
    // the lying body's free hips (a leg spread wide) and the flaps' stop face, which the sceptic worked out turns to the
    // thigh's underside past 90 degrees of hip bend (MinerBody.Drape). The sceptic's cheapest decisive step, done here:
    // Small's drawn model, no physics, held lying on its back by the atlas's poser, one leg through a grid of hip bend,
    // spread and twist, its knee straight or bent, the other foot down or raised; at each pose, held until the flaps have
    // settled, the cut ring is looked for from the game camera's heights (RtsCamera: 32 to 62 degrees down) on eight
    // sides and from straight above, through the baked skin of every garment. A point of the ring counts as seen from a
    // way when nothing drawn lies between it and the eye: a face counts if it is drawn on both sides, or if its front
    // faces the eye (the loose count: anything in the way hides it). Bars set before the run: the ring seen from no way
    // in any pose, on either leg. Controls: the standing miner (the skirt hangs over the ring: none seen), and the
    // trousers alone (the measure can see an open ring). Nothing of the game is changed.
    // Run on its own: -testFilter WonderGather.Tests.DrawnCoverGridTests [-coverOut <folder>] [-coverLeg L|R]
    // [-coverQuick 1].
    [Explicit]
    public sealed class DrawnCoverGridTests
    {
        private const int Tile = 640;
        private const float FieldOfView = 20, Distance = 3.2f;
        private static readonly float[] Elevations = { 32, 47, 62 };
        private static readonly string[] Sides = { "feet", "feet and right", "right", "head and right", "head", "head and left", "left", "feet and left" };
        private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly List<GameObject> made = new List<GameObject>();
        private readonly List<Cover> covers = new List<Cover>();
        private readonly Dictionary<Collider, Cover> coverOf = new Dictionary<Collider, Cover>();
        private readonly bool[] layerWasIgnored = new bool[32];
        private int layer = -1, mask, mismatches, faceChecks;
        private bool layersSet, backfacesWere, seamsWere = MinerBody.FlapsOnTheirSeams;
        private MeshCollider groundSolid;
        private Mesh aloneMesh;
        private Vector3[] ways;
        private string[] wayNames;

        private sealed class Cover
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

        private struct Seen
        {
            public int Points, Open, OpenLoose, Views;
            public int[] ByWay;
        }

        private sealed class Row
        {
            public float Other, Knee, Rot, Abd, Flex;
            public Seen Moving, Still;
            public bool FaceAgrees;
            public float FrontOn, BackOn, GroundMm;
            public int Worst;
        }

        [TearDown]
        public void End()
        {
            foreach (var g in made) if (g != null) UnityEngine.Object.Destroy(g);
            made.Clear();
            foreach (var c in covers) if (c.World != null) UnityEngine.Object.Destroy(c.World);
            covers.Clear();
            coverOf.Clear();
            if (aloneMesh != null) UnityEngine.Object.Destroy(aloneMesh);
            aloneMesh = null;
            Time.captureFramerate = 0;
            GrassField.NotFor = null;
            Physics.queriesHitBackfaces = backfacesWere;
            MinerBody.FlapsOnTheirSeams = seamsWere;
            if (layersSet) for (int i = 0; i < 32; i++) Physics.IgnoreLayerCollision(layer, i, layerWasIgnored[i]);
            layersSet = false;
        }

        [UnityTest, Timeout(3600000)]
        public IEnumerator SmallsCutTrousersStayCovered()
        {
            bool quick = CaptureTools.Argument("-coverQuick") == "1";
            int leg = CaptureTools.Argument("-coverLeg") == "R" ? 1 : 0, still = 1 - leg;
            string folder = CaptureTools.Argument("-coverOut") ?? Path.Combine("Captures", "CoverGrid", DateTime.Now.ToString("yyyy-MM-dd_HHmm", CultureInfo.InvariantCulture) + (quick ? "_quick" : ""));
            Directory.CreateDirectory(folder);
            // (-flapSeam 1 or 0: the flaps on their seams, MinerBody.FlapsOnTheirSeams, November 14; left out, as set.)
            string seams = CaptureTools.Argument("-flapSeam");
            if (seams != null) MinerBody.FlapsOnTheirSeams = seams == "1";
            backfacesWere = Physics.queriesHitBackfaces;
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            Time.captureFramerate = 30;

            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            camera.fieldOfView = FieldOfView;
            camera.nearClipPlane = .02f;
            GrassField.NotFor = camera;
            time.Hour = 13;
            Light sun = RenderSettings.sun;
            if (sun == null) foreach (var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional) { sun = l; break; }
            time.enabled = false;
            // The atlas's stage: on the dirt path, along one of its long straight stretches, level, its top just above the ground.
            Vector2 from = ground.Path[4], to = ground.Path[5];
            Vector3 along = new Vector3(to.x - from.x, 0, to.y - from.y).normalized;
            Vector3 spot = new Vector3(from.x, 0, from.y);
            float top = -1e9f;
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI / 12;
                for (float r = 0; r <= 1.6f; r += .4f) top = Mathf.Max(top, ground.Height(spot.x + r * Mathf.Cos(a), spot.z + r * Mathf.Sin(a)));
            }
            spot.y = top + .002f;
            var stage = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            UnityEngine.Object.Destroy(stage.GetComponent<Collider>());
            stage.name = "Cover stage";
            stage.transform.localScale = new Vector3(3.2f, .05f, 3.2f);
            stage.transform.position = spot - new Vector3(0, .05f, 0);
            stage.GetComponent<Renderer>().sharedMaterial = Clay(new Color(.55f, .46f, .36f));
            made.Add(stage);
            Quaternion facing = Quaternion.LookRotation(along);

            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Small") index = i;
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner is called Small.");
            choice.Choose(index);
            for (int k = 0; k < 30; k++) yield return null;
            var unit = choice.Current;
            unit.SetSelected(false);
            var miner = unit.GetComponent<MinerBody>();
            var biped = unit.GetComponent<ProceduralBiped>();
            var agent = unit.GetComponent<NavMeshAgent>();
            // (The pickaxe is no garment: not drawn, and nothing it would hide is counted as hidden.)
            foreach (var held in UnityEngine.Object.FindObjectsByType<HeldThing>(FindObjectsSortMode.None))
                foreach (var r in held.GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name.StartsWith("Pickaxe")) foreach (var r in t.GetComponentsInChildren<Renderer>(true)) r.enabled = false;

            // The garments' solids, on a layer of their own that touches nothing.
            for (int i = 31; i >= 8 && layer < 0; i--) if (string.IsNullOrEmpty(LayerMask.LayerToName(i))) layer = i;
            Assert.That(layer, Is.GreaterThanOrEqualTo(0), "No spare layer for the garments' solids.");
            for (int i = 0; i < 32; i++) { layerWasIgnored[i] = Physics.GetIgnoreLayerCollision(layer, i); Physics.IgnoreLayerCollision(layer, i, true); }
            layersSet = true;
            mask = 1 << layer;

            var log = new StringBuilder();
            log.AppendLine($"COVER_SWITCH the flaps on their seams (MinerBody.FlapsOnTheirSeams): {MinerBody.FlapsOnTheirSeams}");
            // Drawn at its finest: the skins of the first level of detail, and those in no level at all.
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
                var go = new GameObject("Cover " + skin.name) { layer = layer };
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
                covers.Add(c);
                coverOf[c.Solid] = c;
                log.AppendLine($"COVER_PART {skin.name}: mesh {shared.name}, {c.Rest.Length} vertices, {c.Tris.Length / 3} triangles, {c.Bones.Length} bones, up to {c.Limit} a point as drawn; {read}; {sides}");
            }
            // The drawn body is one mesh (rigging.py joins every part into it), so the trousers are found in it: the pieces
            // (whole once their seams are welded) that a thigh carries and that no bone outside the trousers' own moves.
            // rigging.py skins the trousers to the pelvis, the spine and the legs only; the skirt hangs from the pelvis and
            // its flaps' bones, the top from the trunk and the arms, the boots from the shins, the feet and the toes.
            Cover body = null;
            foreach (var c in covers) if (body == null || c.Rest.Length > body.Rest.Length) body = c;
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
            Vector3 feet = unit.transform.position;
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
                ring[Vector3.Dot(body.Verts[v] - feet, unit.transform.right) < 0 ? 0 : 1].Add(v);
            }
            log.AppendLine($"COVER_RING the trousers: {trouserTris.Count / 3} triangles (in the body's part {trouserSub}), {openWelded.Count} open points; the cut ring: {ring[0].Count} on the left leg, {ring[1].Count} on the right, {1000 * ringLow:F0} to {1000 * ringTop:F0} mm above the feet as it stands");
            Assert.That(ring[0].Count >= 8 && ring[1].Count >= 8, Is.True, "The cut ring was not found on both legs:\n" + log);
            // The trousers alone, for the control: their own solid, from the body's baked points.
            var alone = new Cover { Name = "trousers alone", Skin = body.Skin, Tris = trouserTris.ToArray(), SubEnd = new[] { trouserTris.Count / 3 }, TwoSided = new[] { body.TwoSided[trouserSub] } };
            var aloneObject = new GameObject("Cover trousers alone") { layer = layer };
            made.Add(aloneObject);
            alone.Solid = aloneObject.AddComponent<MeshCollider>();
            alone.World = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            aloneMesh = alone.World;
            alone.Solid.enabled = false;
            coverOf[alone.Solid] = alone;

            // The ways it is looked at from: the game camera's heights, on eight sides of the body, and straight above.
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
            ways = wl.ToArray();
            wayNames = wn.ToArray();

            // The controls, standing: every garment (the skirt hangs over the ring), and the trousers alone (an open ring is seen).
            var ground0 = new GameObject("Cover ground") { layer = layer };
            made.Add(ground0);
            var quad = new Mesh { vertices = new[] { new Vector3(-6, 0, -6), new Vector3(-6, 0, 6), new Vector3(6, 0, 6), new Vector3(6, 0, -6) }, triangles = new[] { 0, 1, 2, 0, 2, 3 } };
            groundSolid = ground0.AddComponent<MeshCollider>();
            groundSolid.sharedMesh = quad;
            ground0.transform.position = spot;
            groundSolid.enabled = false;
            BakeAll();
            var standAll = new Seen[2];
            var standAlone = new Seen[2];
            Physics.queriesHitBackfaces = true;
            for (int s = 0; s < 2; s++) standAll[s] = Look(body, ring[s]);
            alone.Verts = body.Verts;
            alone.World.vertices = body.Verts;
            alone.World.triangles = alone.Tris;
            alone.World.RecalculateBounds();
            alone.Solid.sharedMesh = alone.World;
            foreach (var c in covers) c.Solid.enabled = false;
            alone.Solid.enabled = true;
            Physics.SyncTransforms();
            for (int s = 0; s < 2; s++) standAlone[s] = Look(body, ring[s]);
            alone.Solid.enabled = false;
            foreach (var c in covers) c.Solid.enabled = true;
            Physics.SyncTransforms();
            Physics.queriesHitBackfaces = backfacesWere;
            log.AppendLine($"COVER_CONTROL standing, every garment: ring points seen {standAll[0].Open}+{standAll[1].Open} of {standAll[0].Points}+{standAll[1].Points} (loose {standAll[0].OpenLoose}+{standAll[1].OpenLoose}); the trousers alone: {standAlone[0].Open}+{standAlone[1].Open} (loose {standAlone[0].OpenLoose}+{standAlone[1].OpenLoose})");

            // Held still as the atlas holds it: the posed body lets its segments be, its balance and its breath are off.
            unit.Motor.Stop();
            if (agent != null && agent.isOnNavMesh) agent.Warp(new Vector3(spot.x, ground.Height(spot.x, spot.z), spot.z));
            if (agent != null) agent.enabled = false;
            unit.transform.SetPositionAndRotation(spot, facing);
            biped.LetGo = true;
            if (unit.TryGetComponent(out PhysicalBalance balance)) balance.Acts = false;
            if (unit.TryGetComponent(out MinerBreath breath)) breath.enabled = false;
            miner.Swell = 0; miner.Shrug = 0;
            var poser = unit.gameObject.AddComponent<AtlasPoser>();
            poser.Miner = miner;
            poser.Origin = spot;
            poser.Facing = facing;
            groundSolid.enabled = true;

            var flaps = (MinerBody.Flap[])Field("flaps").GetValue(miner);
            var acrossAt = (Vector3)Field("pelvisAcross").GetValue(miner);
            var forwardAt = (Vector3)Field("pelvisForward").GetValue(miner);
            var hangsField = Field("flapHangs");
            float ankleUp = (float)Field("ankleUp").GetValue(miner);
            log.AppendLine($"COVER_FLAPS {flaps.Length} flaps: " + string.Join(", ", Array.ConvertAll(flaps, f => (f.leg == 0 ? "left " : "right ") + (f.front ? "front" : "back"))));

            // Lying on its back, face up (the solver's pitch of -90 degrees), the arms out on the ground clear of the hips, the
            // back and the head on the stage.
            var b = BodyFrame.FromUnit(unit.gameObject);
            var stand = AnyBody.StanceFitter.Fit(b, AnyBody.StanceLibrary.Make(b, "stand"));
            var rest = (double[])stand.Q.Clone();
            rest[AnyBody.Pose.X] = 0; rest[AnyBody.Pose.Y] = 0; rest[AnyBody.Pose.Z] = 0;
            rest[AnyBody.Pose.Yaw] = 0; rest[AnyBody.Pose.Pitch] = -Math.PI / 2; rest[AnyBody.Pose.Roll] = 0;
            rest[AnyBody.Pose.TFlex] = rest[AnyBody.Pose.TLat] = rest[AnyBody.Pose.TTw] = 0;
            rest[AnyBody.Pose.NFlex] = rest[AnyBody.Pose.NLat] = rest[AnyBody.Pose.NRot] = 0;
            for (int s = 0; s < 2; s++)
            {
                int o = 4 * s;
                rest[AnyBody.Pose.ShFlexL + o] = 0; rest[AnyBody.Pose.ShAbdL + o] = R(70); rest[AnyBody.Pose.ShRotL + o] = 0; rest[AnyBody.Pose.ElbL + o] = R(10);
            }
            SetLeg(rest, leg, 0, 0, 0, 0);
            SetLeg(rest, still, 0, 10, 0, 5);
            var k0 = AtlasPoser.Kin(b, rest);
            double low = k0.LowestZ[10].V;
            for (int c = 0; c < 10; c++) if (k0.CapLink[c] == AnyBody.BodyLinks.Hips || k0.CapLink[c] == AnyBody.BodyLinks.Trunk) low = Math.Min(low, k0.LowestZ[c].V);
            rest[AnyBody.Pose.Z] = -low;
            var now = (double[])rest.Clone();
            poser.Pose(b, now);
            for (int k = 0; k < 45; k++) yield return null;
            var restRow = Measure(miner, body, ring, leg, still, flaps, acrossAt, forwardAt, hangsField, ankleUp, spot.y);
            log.AppendLine($"COVER_REST lying on its back, legs straight: ring seen {restRow.Moving.Open}+{restRow.Still.Open} (loose {restRow.Moving.OpenLoose}+{restRow.Still.OpenLoose}); the flap's face agrees with the thigh's front: {restRow.FaceAgrees}; Drape's ground {restRow.GroundMm:F0} mm from the stage");

            // The control for why: the flaps held as worn standing (their rest turn under the pelvis, MinerBody's flapRest),
            // the body lying as before. Lying straight with them held, the body is the standing body turned onto its back, so
            // what is seen then is what a standing miner shows to the same ways turned with it (straight above is its front,
            // level; from its feet is from below, up the skirt); what Drape's hang adds lying is the difference. The ring's
            // points are marked in red in its pictures: a mark the garments hide is not drawn.
            var marks = new List<GameObject>();
            var markRed = Clay(new Color(.9f, .08f, .08f));
            foreach (int s in new[] { leg, still })
                foreach (int i in ring[s])
                {
                    var m = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.DestroyImmediate(m.GetComponent<Collider>());
                    m.name = "Cover ring mark";
                    m.transform.localScale = Vector3.one * .008f;
                    m.transform.position = body.Verts[i];
                    m.GetComponent<Renderer>().sharedMaterial = markRed;
                    marks.Add(m);
                }
            log.AppendLine("COVER_FLAP_WAYS draped: " + FlapWays(miner, flaps, facing));
            int fromFeet = 0;
            for (int w = 0; w < ways.Length - 1; w++) if (wayNames[w] == $"{Elevations[0]:F0} degrees down, from its feet") fromFeet = w;
            foreach (var picture in Control(camera, sun, miner.Rig.thighs[leg].position, facing, folder, "rest_flaps_draped", new[] { ways.Length - 1, fromFeet })) yield return picture;
            var holder = unit.gameObject.AddComponent<CoverFlapHolder>();
            holder.Flaps = flaps;
            holder.Rest = (Quaternion[])Field("flapRest").GetValue(miner);
            for (int k = 0; k < 3; k++) yield return null;
            var heldRow = Measure(miner, body, ring, leg, still, flaps, acrossAt, forwardAt, hangsField, ankleUp, spot.y);
            log.AppendLine("COVER_FLAP_WAYS held: " + FlapWays(miner, flaps, facing));
            foreach (var picture in Control(camera, sun, miner.Rig.thighs[leg].position, facing, folder, "rest_flaps_held", new[] { ways.Length - 1, fromFeet })) yield return picture;
            UnityEngine.Object.Destroy(holder);
            foreach (var m in marks) UnityEngine.Object.Destroy(m);
            log.AppendLine($"COVER_FLAPS_HELD lying, legs straight, the flaps held as worn standing: ring seen {heldRow.Moving.Open}+{heldRow.Still.Open} (loose {heldRow.Moving.OpenLoose}+{heldRow.Still.OpenLoose}), against {restRow.Moving.Open}+{restRow.Still.Open} draped");
            var byWay = new List<string>();
            for (int w = 0; w < ways.Length; w++)
            {
                int dr = restRow.Moving.ByWay[w] + restRow.Still.ByWay[w], he = heldRow.Moving.ByWay[w] + heldRow.Still.ByWay[w];
                if (dr + he > 0) byWay.Add($"{wayNames[w]} {dr}/{he}");
            }
            log.AppendLine("COVER_WAYS ring points seen from each way, both legs, draped/held: " + (byWay.Count > 0 ? string.Join("; ", byWay) : "none"));
            for (int k = 0; k < 3; k++) yield return null;

            // The skin worked out by hand, against the graphics card's own: the body is held still, so both are of one pose.
            camera.transform.position = miner.Rig.thighs[leg].position + Vector3.up * Distance;
            camera.transform.rotation = Quaternion.LookRotation(Vector3.down, facing * Vector3.forward);
            yield return null;
            CaptureTools.Render(camera, Path.Combine(folder, "skin_check"), 96, 96);
            yield return null;
            CaptureTools.Render(camera, Path.Combine(folder, "skin_check"), 96, 96);
            BakeAll();
            string gpu = GpuCheck(body, out float gpuApart);
            log.AppendLine("COVER_GPU " + gpu);
            if (gpuApart >= 0) Assert.That(gpuApart, Is.LessThan(.002f), "The skin worked out by hand is not where the graphics card draws it:\n" + log);

            // The grid, gone through as one continuous movement (each row back the way the last came), each pose held until
            // the flaps have settled (they fall by 1 - e^(-12 dt) a frame: at 30 a second, 1/3000 of the way left after 20).
            float[] flexes, abds, rots, knees;
            if (quick) { flexes = new float[] { -20, 10, 40, 70, 100, 130 }; abds = new float[] { 0, 45, 90 }; rots = new float[] { 0 }; knees = new float[] { 0, 90 }; }
            else
            {
                flexes = new float[16]; for (int i = 0; i < 16; i++) flexes[i] = -20 + 10 * i;
                abds = new float[] { 0, 15, 30, 45, 60, 75, 90 }; rots = new float[] { -40, 0, 40 }; knees = new float[] { 0, 90 };
            }
            int others = quick ? 1 : 2;
            var grid = new List<float[]>();
            int c1 = 0, c2 = 0, c3 = 0, c4 = 0;
            for (int o = 0; o < others; o++)
                foreach (int ki in Snake(knees.Length, c1++))
                    foreach (int ri in Snake(rots.Length, c2++))
                        foreach (int ai in Snake(abds.Length, c3++))
                            foreach (int fi in Snake(flexes.Length, c4++))
                                grid.Add(new[] { o, knees[ki], rots[ri], abds[ai], flexes[fi] });
            var rows = new List<Row>();
            bool outwardSeen = false, outwardRight = false;
            var target = new double[now.Length];
            foreach (var p in grid)
            {
                Array.Copy(rest, target, rest.Length);
                SetLeg(target, leg, p[4], p[3], p[2], p[1]);
                if (p[0] == 0) SetLeg(target, still, 0, 10, 0, 5); else SetLeg(target, still, 80, 10, 0, 90);
                yield return Move(poser, b, now, target, 20);
                var row = Measure(miner, body, ring, leg, still, flaps, acrossAt, forwardAt, hangsField, ankleUp, spot.y);
                row.Other = p[0]; row.Knee = p[1]; row.Rot = p[2]; row.Abd = p[3]; row.Flex = p[4];
                rows.Add(row);
                // (The angles' sense, once: a leg spread 45 degrees with its hip about straight goes out to its own side; the
                // quick grid's nearest bend to straight is 10 degrees.)
                if (!outwardSeen && p[3] >= 45 && Mathf.Abs(p[4]) <= 10 && Mathf.Approximately(p[2], 0))
                {
                    outwardSeen = true;
                    var rb = miner.Rig;
                    outwardRight = Vector3.Dot(rb.shins[leg].position - rb.thighs[leg].position, facing * (leg == 0 ? Vector3.left : Vector3.right)) > 0;
                }
            }

            // What the grid saw.
            var csv = new StringBuilder("other_foot,knee,twist,spread,bend,ring_moving,seen_moving,seen_moving_loose,views_moving,worst_way,ring_still,seen_still,seen_still_loose,face_agrees,front_flap_on_front,back_flap_on_back,drape_ground_mm\n");
            int open = 0, openLoose = 0, openStill = 0, bentOpen = 0, bentAll = 0, straightOpen = 0, straightAll = 0, wideOpen = 0, wideAll = 0, narrowOpen = 0, narrowAll = 0;
            int agreeBelow = 0, below = 0, agreePast = 0, past = 0;
            foreach (var r in rows)
            {
                csv.Append(string.Join(",", r.Other == 0 ? "down" : "raised", F(r.Knee), F(r.Rot), F(r.Abd), F(r.Flex), r.Moving.Points, r.Moving.Open, r.Moving.OpenLoose, r.Moving.Views,
                    r.Moving.Views > 0 ? "\"" + wayNames[r.Worst] + "\"" : "", r.Still.Points, r.Still.Open, r.Still.OpenLoose, r.FaceAgrees ? 1 : 0, F(r.FrontOn), F(r.BackOn), F(r.GroundMm))).Append('\n');
                bool seen = r.Moving.Open > 0;
                if (seen) open++;
                if (r.Moving.OpenLoose > 0) openLoose++;
                if (r.Still.Open > 0) openStill++;
                if (r.Flex > 90) { bentAll++; if (seen) bentOpen++; past++; if (r.FaceAgrees) agreePast++; }
                else { straightAll++; if (seen) straightOpen++; below++; if (r.FaceAgrees) agreeBelow++; }
                if (r.Abd > 45) { wideAll++; if (seen) wideOpen++; } else { narrowAll++; if (seen) narrowOpen++; }
            }
            File.WriteAllText(Path.Combine(folder, "grid.csv"), csv.ToString());
            log.AppendLine($"COVER_GRID Small, the {(leg == 0 ? "left" : "right")} leg moved, {rows.Count} poses: its cut ring seen in {open} (in {openLoose} by the loose count); " +
                $"hip bent 90 degrees or less: {straightOpen} of {straightAll}; past 90: {bentOpen} of {bentAll}; spread 45 or less: {narrowOpen} of {narrowAll}; past 45: {wideOpen} of {wideAll}; " +
                $"the other leg's ring seen in {openStill}; the flap's stop face agrees with the thigh's front in {agreeBelow} of {below} poses bent 90 or less and in {agreePast} of {past} past 90; " +
                $"faces whose plane missed the hit: {mismatches} of {faceChecks}; the leg spread goes out to its own side: {(outwardSeen ? outwardRight.ToString() : "not seen")}");

            // Pictures: the poses where most of the ring is seen, each from the way it is seen most and from straight above,
            // and the legs straight for comparison.
            var picks = new List<Row> { restRow };
            restRow.Other = 0; restRow.Worst = ways.Length - 1;
            var order = new List<Row>(rows);
            order.Sort((x, y) => y.Moving.Views.CompareTo(x.Moving.Views));
            foreach (var r in order)
            {
                if (picks.Count >= 7 || r.Moving.Views == 0) break;
                bool near = false;
                foreach (var q in picks) if (q != restRow && q.Other == r.Other && q.Knee == r.Knee && q.Rot == r.Rot && Mathf.Abs(q.Flex - r.Flex) <= 20 && Mathf.Abs(q.Abd - r.Abd) <= 15) near = true;
                if (!near) picks.Add(r);
            }
            // (-coverShow "knee,twist,spread,bend;...": these poses pictured instead, every one from the same three ways, so
            // that runs with the flaps either way show the same; November 14, for the aesthetic judges.)
            string show = CaptureTools.Argument("-coverShow");
            int[] showWays = null;
            if (show != null)
            {
                picks.Clear();
                picks.Add(restRow);
                foreach (string one in show.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var p = one.Split(',');
                    Assert.That(p.Length, Is.EqualTo(4), "A pose to show is knee,twist,spread,bend: " + one);
                    float k = float.Parse(p[0], CultureInfo.InvariantCulture), t = float.Parse(p[1], CultureInfo.InvariantCulture);
                    float s = float.Parse(p[2], CultureInfo.InvariantCulture), f = float.Parse(p[3], CultureInfo.InvariantCulture);
                    var match = rows.Find(q => q.Other == 0 && q.Knee == k && q.Rot == t && q.Abd == s && q.Flex == f);
                    Assert.That(match, Is.Not.Null, "No pose " + one + " in this grid.");
                    picks.Add(match);
                }
                showWays = new[] { Array.IndexOf(wayNames, "47 degrees down, from its feet and left"), Array.IndexOf(wayNames, "47 degrees down, from its head and right"), ways.Length - 1 };
                Assert.That(showWays, Has.None.EqualTo(-1), "A way to show from is missing.");
            }
            int shot = 0;
            foreach (var r in picks)
            {
                Array.Copy(rest, target, rest.Length);
                SetLeg(target, leg, r.Flex, r.Abd, r.Rot, r.Knee);
                if (r.Other == 0) SetLeg(target, still, 0, 10, 0, 5); else SetLeg(target, still, 80, 10, 0, 90);
                yield return Move(poser, b, now, target, 25);
                Vector3 aim = miner.Rig.thighs[leg].position;
                string name = $"{shot:D2}_{(r.Other == 0 ? "down" : "raised")}_knee{r.Knee:F0}_twist{r.Rot:F0}_spread{r.Abd:F0}_bend{r.Flex:F0}";
                foreach (int w in showWays ?? new[] { r.Worst, ways.Length - 1 })
                {
                    Vector3 d = ways[w];
                    camera.transform.position = aim + d * Distance;
                    camera.transform.rotation = w == ways.Length - 1 ? Quaternion.LookRotation(Vector3.down, facing * Vector3.forward) : Quaternion.LookRotation(-d, Vector3.up);
                    Vector3 looks = -d;
                    if (sun != null) sun.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(40, Vector3.up) * Quaternion.AngleAxis(50, Vector3.Cross(Vector3.up, looks)) * looks, Vector3.up);
                    yield return null;
                    string seenFrom = w == ways.Length - 1 ? "_above" : showWays == null ? "_seen" : "_" + Sides[w % 8].Replace(" and ", "_");
                    CaptureTools.Render(camera, Path.Combine(folder, name + seenFrom), Tile, Tile);
                }
                log.AppendLine($"COVER_PICTURE {name}: ring seen {r.Moving.Open} of {r.Moving.Points} points, {r.Moving.Views} views, most {wayNames[r.Worst]}");
                shot++;
            }

            UnityEngine.Object.Destroy(poser);
            biped.LetGo = false;
            if (balance != null) balance.Acts = true;
            if (breath != null) breath.enabled = true;
            foreach (var g in unit.GetComponentsInChildren<LODGroup>()) g.ForceLOD(-1);
            if (agent != null) agent.enabled = true;
            File.WriteAllText(Path.Combine(folder, "log.txt"), log.ToString());
            Debug.Log(log.ToString());

            // The measure's own checks first, then the bar.
            Assert.That(standAlone[0].Open + standAlone[1].Open, Is.GreaterThan(0), "The measure saw no open ring on the trousers alone:\n" + log);
            Assert.That(standAll[0].Open + standAll[1].Open, Is.EqualTo(0), "The ring was seen with the miner standing:\n" + log);
            Assert.That(outwardSeen && outwardRight, Is.True, "A spread leg did not go out to its own side:\n" + log);
            Assert.That(mismatches, Is.EqualTo(0), "Hits off the plane of the face they were counted on:\n" + log);
            Assert.That(open + openStill, Is.EqualTo(0), "The cut ring was seen:\n" + log);
        }

        private FieldInfo Field(string name)
        {
            var f = typeof(MinerBody).GetField(name, Hidden);
            Assert.That(f, Is.Not.Null, "MinerBody has no field " + name);
            return f;
        }

        private static double R(double degrees) => degrees * Math.PI / 180;

        private static void SetLeg(double[] q, int side, double bend, double spread, double twist, double knee)
        {
            int o = 4 * side;
            q[AnyBody.Pose.HFlexL + o] = R(bend);
            q[AnyBody.Pose.HAbdL + o] = R(spread);
            q[AnyBody.Pose.HRotL + o] = R(twist);
            q[AnyBody.Pose.KneeL + o] = R(knee);
        }

        private static IEnumerable<int> Snake(int n, int call)
        {
            for (int i = 0; i < n; i++) yield return call % 2 == 0 ? i : n - 1 - i;
        }

        // From the pose it is in to the next, at no more than 8 degrees a frame (240 a second), then held.
        private static IEnumerator Move(AtlasPoser poser, AnyBody.BodyModel b, double[] now, double[] to, int hold)
        {
            double most = 0;
            for (int i = 0; i < now.Length; i++) most = Math.Max(most, Math.Abs(to[i] - now[i]));
            int frames = Mathf.Clamp(Mathf.CeilToInt((float)(most * 180 / Math.PI / 8)), 1, 40);
            var q = new double[now.Length];
            for (int f = 1; f <= frames; f++)
            {
                double u = (double)f / frames;
                for (int i = 0; i < q.Length; i++) q[i] = now[i] + (to[i] - now[i]) * u;
                poser.Pose(b, q);
                yield return null;
            }
            Array.Copy(to, now, now.Length);
            poser.Pose(b, now);
            for (int f = 0; f < hold; f++) yield return null;
        }

        // One pose: the garments baked as drawn this frame; the ring looked for on both legs; the moving leg's flaps.
        private Row Measure(MinerBody miner, Cover body, List<int>[] ring, int leg, int still, MinerBody.Flap[] flaps, Vector3 acrossAt, Vector3 forwardAt, FieldInfo hangsField, float ankleUp, float stageY)
        {
            BakeAll();
            var row = new Row();
            Physics.queriesHitBackfaces = true;
            row.Moving = Look(body, ring[leg]);
            row.Still = Look(body, ring[still]);
            Physics.queriesHitBackfaces = backfacesWere;
            int worst = 0;
            for (int w = 1; w < ways.Length; w++) if (row.Moving.ByWay[w] > row.Moving.ByWay[worst]) worst = w;
            row.Worst = worst;
            // The flap's stop face as Drape works it out (MinerBody.cs, Drape), against the thigh's own front.
            var bones = miner.Rig;
            Vector3 across = bones.pelvis.rotation * acrossAt, forward = bones.pelvis.rotation * forwardAt;
            Vector3 thigh = (bones.shins[leg].position - bones.thighs[leg].position).normalized;
            Vector3 flat = Vector3.ProjectOnPlane(thigh, across);
            flat = flat.sqrMagnitude > 1e-4f ? flat.normalized : Vector3.down;
            Vector3 face = Vector3.Cross(across, flat);
            // (On their seams, Drape takes the face's side from the thigh's own front, as set when the body was made.)
            Vector3 frontWay = forward;
            if (MinerBody.FlapsOnTheirSeams) frontWay = bones.thighs[leg].rotation * ((Vector3[])Field("thighFront").GetValue(miner))[leg];
            if (Vector3.Dot(face, frontWay) < 0) face = -face;
            Vector3 front = Vector3.ProjectOnPlane(miner.Solved.thighs[leg].forward, across);
            front = front.sqrMagnitude > 1e-6f ? front.normalized : Vector3.zero;
            row.FaceAgrees = Vector3.Dot(face, front) > 0;
            var hangs = (Vector3[])hangsField.GetValue(miner);
            row.FrontOn = row.BackOn = float.NaN;
            for (int k = 0; k < flaps.Length && k < hangs.Length; k++)
                if (flaps[k].leg == leg)
                {
                    float on = Vector3.Dot(hangs[k], front);
                    if (flaps[k].front) row.FrontOn = on; else row.BackOn = -on;
                }
            row.GroundMm = 1000 * (Mathf.Min(bones.feet[0].position.y, bones.feet[1].position.y) - ankleUp - stageY);
            return row;
        }

        private void BakeAll()
        {
            foreach (var c in covers) Bake(c);
            Physics.SyncTransforms();
        }

        // A garment as drawn this frame, in the world, and its solid made from it (standing at the world's origin). The
        // skin is worked out by hand as the renderer works it out: each point carried by its bones (at most Limit of them,
        // the heaviest, their weights made whole again), each bone's way being its pose times its bind pose.
        private void Bake(Cover c)
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
        private static string ReadDrawn(Mesh mesh, Cover c)
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
        private static float Value(byte[] b, int at, int k, VertexAttributeFormat f)
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

        // The graphics card's own skin of a garment (the first three numbers of each of its points) against the skin worked
        // out by hand: the furthest two points apart, read as the world, as the renderer's own space and as its root bone's.
        private static string GpuCheck(Cover c, out float best)
        {
            best = -1;
            using (var buf = c.Skin.GetVertexBuffer())
            {
                if (buf == null) return "not checked: the renderer gave no skinned buffer";
                long bytes = (long)buf.count * buf.stride;
                int n = c.Rest.Length;
                if (n == 0 || bytes % n != 0 || bytes / n % 4 != 0) return $"not checked: {buf.count} x {buf.stride} bytes do not divide into {n} points";
                int step = (int)(bytes / n / 4);
                var data = new float[bytes / 4];
                buf.GetData(data);
                var root = c.Skin.rootBone != null ? c.Skin.rootBone : c.Skin.transform;
                var spaces = new[] { ("the world", Matrix4x4.identity), ("the renderer's own space", c.Skin.transform.worldToLocalMatrix), ("its root bone's (" + root.name + ")", root.worldToLocalMatrix) };
                var said = new StringBuilder($"{n} points of {4 * step} bytes:");
                foreach (var (name, m) in spaces)
                {
                    float worst = 0;
                    for (int v = 0; v < n; v++)
                    {
                        var g = new Vector3(data[v * step], data[v * step + 1], data[v * step + 2]);
                        worst = Mathf.Max(worst, Vector3.Distance(g, m.MultiplyPoint3x4(c.Verts[v])));
                    }
                    said.Append($" as {name} at most {1000 * worst:F2} mm apart;");
                    if (best < 0 || worst < best) best = worst;
                }
                return said.ToString();
            }
        }

        // How much of a ring can be seen, from each way and from any.
        private Seen Look(Cover body, List<int> ring)
        {
            var seen = new Seen { Points = ring.Count, ByWay = new int[ways.Length] };
            foreach (int v in ring)
            {
                Vector3 p = body.Verts[v];
                bool strict = false, loose = false;
                for (int w = 0; w < ways.Length; w++)
                {
                    if (!Covered(p, ways[w], out bool anyHit)) { seen.ByWay[w]++; seen.Views++; strict = true; }
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
        private bool Covered(Vector3 p, Vector3 d, out bool anyHit)
        {
            anyHit = false;
            Vector3 from = p + d * .002f;
            for (int n = 0; n < 32; n++)
            {
                if (!Physics.Raycast(from, d, out RaycastHit hit, 4f, mask, QueryTriggerInteraction.Ignore)) return false;
                anyHit = true;
                if (hit.collider == groundSolid || !coverOf.TryGetValue(hit.collider, out Cover c) || hit.triangleIndex < 0 || 3 * hit.triangleIndex + 2 >= c.Tris.Length) return true;
                int t = hit.triangleIndex, s = 0;
                while (s < c.SubEnd.Length - 1 && t >= c.SubEnd[s]) s++;
                Vector3 a = c.Verts[c.Tris[3 * t]], b = c.Verts[c.Tris[3 * t + 1]], e = c.Verts[c.Tris[3 * t + 2]];
                Vector3 face = Vector3.Cross(b - a, e - a);
                if (face.sqrMagnitude > 1e-14f)
                {
                    faceChecks++;
                    if (Mathf.Abs(Vector3.Dot(hit.point - a, face.normalized)) > .001f) mismatches++;
                }
                if (c.TwoSided[s] || Vector3.Dot(face, d) > 0) return true;
                from = hit.point + d * .0005f;
            }
            return true;
        }

        // A material's culling: 0 draws both sides, 2 the front only (URP's _Cull); -1 when the material does not say.
        private static int Cull(Material m)
        {
            if (m == null) return -1;
            if (m.HasProperty("_Cull")) return Mathf.RoundToInt(m.GetFloat("_Cull"));
            if (m.HasProperty("_CullMode")) return Mathf.RoundToInt(m.GetFloat("_CullMode"));
            return -1;
        }

        private static Material Clay(Color colour)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = colour };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", colour);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .1f);
            return material;
        }

        // Where each flap points (its bone's turn applied to the way it hangs when worn) and where Drape last meant it to
        // hang, and where the thighs point: up, toward the lying body's feet and to the stage's right.
        private string FlapWays(MinerBody miner, MinerBody.Flap[] flaps, Quaternion facing)
        {
            var down = (Vector3[])Field("flapDown").GetValue(miner);
            var hangs = (Vector3[])Field("flapHangs").GetValue(miner);
            Vector3 feet = facing * Vector3.forward, right = facing * Vector3.right;
            string Say(Vector3 v) => $"up {v.y:F2} feet {Vector3.Dot(v, feet):F2} right {Vector3.Dot(v, right):F2}";
            var say = new List<string>();
            for (int k = 0; k < flaps.Length && k < down.Length && k < hangs.Length; k++)
                if (flaps[k].bone != null)
                    say.Add($"{(flaps[k].leg == 0 ? "left" : "right")} {(flaps[k].front ? "front" : "back")} points {Say((flaps[k].bone.rotation * down[k]).normalized)} (meant {Say(hangs[k])}), its hinge {1000 * (flaps[k].bone.position.y - miner.Rig.thighs[flaps[k].leg].position.y):F0} mm above the hip");
            var rig = miner.Rig;
            for (int s = 0; s < 2; s++) say.Add($"{(s == 0 ? "left" : "right")} thigh {Say((rig.shins[s].position - rig.thighs[s].position).normalized)}");
            return string.Join("; ", say);
        }

        // Pictures of one held pose from the given ways (straight above with the body's facing up the picture), lit as the
        // grid's pictures are.
        private IEnumerable Control(Camera camera, Light sun, Vector3 aim, Quaternion facing, string folder, string name, int[] which)
        {
            foreach (int w in which)
            {
                Vector3 d = ways[w];
                camera.transform.position = aim + d * Distance;
                camera.transform.rotation = w == ways.Length - 1 ? Quaternion.LookRotation(Vector3.down, facing * Vector3.forward) : Quaternion.LookRotation(-d, Vector3.up);
                Vector3 looks = -d;
                if (sun != null) sun.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(40, Vector3.up) * Quaternion.AngleAxis(50, Vector3.Cross(Vector3.up, looks)) * looks, Vector3.up);
                yield return null;
                CaptureTools.Render(camera, Path.Combine(folder, name + (w == ways.Length - 1 ? "_above" : "_from_feet")), Tile, Tile);
            }
        }

        private static string F(float v) => float.IsNaN(v) || float.IsInfinity(v) ? "" : v.ToString("0.###", CultureInfo.InvariantCulture);
    }

    // The cover grid's control: the skirt's flaps held as worn standing (their rest turn under the pelvis), after MinerBody
    // has draped them each frame, so that the drawn skin and the hand skin both see them held.
    [DefaultExecutionOrder(60)]
    internal sealed class CoverFlapHolder : MonoBehaviour
    {
        public MinerBody.Flap[] Flaps;
        public Quaternion[] Rest;

        private void LateUpdate()
        {
            if (Flaps == null || Rest == null) return;
            for (int k = 0; k < Flaps.Length && k < Rest.Length; k++)
                if (Flaps[k].bone != null) Flaps[k].bone.localRotation = Rest[k];
        }
    }
}
