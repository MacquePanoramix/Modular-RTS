using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace WonderGather
{
    // The Ordinary Place's land: one deterministic height field shared by the terrain mesh,
    // its collider, the grass and the path, so they always agree. Gentle meadow near the
    // house and a worn path to the door. Beyond the meadow lies the far world (S1e): slopes
    // falling to a lake that opens east to a water horizon, layered ranges closing the other
    // horizons, and one great snow peak north over the water. Only the meadow is walkable.
    // The meshes are rebuilt whenever the scene loads instead of being stored as large assets.
    [ExecuteAlways]
    public sealed class OrdinaryGround : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private int seed = 7;
        [Tooltip("Half-size of the walkable, explorable meadow, in metres.")]
        [SerializeField] private float innerHalf = 60, innerSpacing = .5f;
        [Tooltip("The land continues as scenery out to this distance.")]
        [SerializeField] private float outerHalf = 9000;
        [Tooltip("Rings of the far land, spaced geometrically out from the meadow.")]
        [SerializeField] private int outerRings = 170;
        [Tooltip("The still water's level in the valley below the meadow's bluff; the lake bed lies below it.")]
        [SerializeField] private float waterLevel = -26;
        [SerializeField] private Vector2[] path = new Vector2[0];
        [SerializeField] private float pathWidth = 1.7f;
        [SerializeField] private Vector2 flatCenter, flatSize = new Vector2(9, 7);
        [SerializeField] private float flatYaw;
        [SerializeField] private Color meadow = new Color(.13f, .20f, .11f), dirt = new Color(.40f, .31f, .21f), far = new Color(.09f, .13f, .11f);
        [SerializeField] private Color forest = new Color(.07f, .12f, .07f), rock = new Color(.30f, .29f, .31f), snow = new Color(.92f, .93f, .96f);
        [SerializeField] private Color shore = new Color(.42f, .38f, .26f), lakeBed = new Color(.16f, .19f, .14f);

        private GameObject generated;

        public float InnerHalf => innerHalf;
        public float WaterLevel => waterLevel;
        public IReadOnlyList<Vector2> Path => path;
        public MeshCollider Collider { get; private set; }

        public void Configure(Vector2[] pathPoints, Vector2 houseCenter, Vector2 houseSize, float houseYaw, Material groundMaterial = null)
        {
            path = pathPoints;
            flatCenter = houseCenter;
            flatSize = houseSize;
            flatYaw = houseYaw;
            if (groundMaterial != null) material = groundMaterial;
        }

        private void OnEnable() => Regenerate();
        private void OnDisable() => Clear();

        // The visible land (out to the far ranges) and the meadow's walkable collider.
        public void Regenerate()
        {
            Clear();
            if (material == null) return;
            generated = new GameObject("Ground (generated)") { hideFlags = HideFlags.DontSave };
            generated.transform.SetParent(transform, false);
            var render = BuildMesh(false);
            render.hideFlags = HideFlags.DontSave;
            generated.AddComponent<MeshFilter>().sharedMesh = render;
            generated.AddComponent<MeshRenderer>().sharedMaterial = material;
            var walkable = new GameObject("Meadow collider") { hideFlags = HideFlags.DontSave, layer = 6 };
            walkable.transform.SetParent(generated.transform, false);
            var shape = BuildMesh(true);
            shape.hideFlags = HideFlags.DontSave;
            Collider = walkable.AddComponent<MeshCollider>();
            Collider.sharedMesh = shape;
        }

        private void Clear()
        {
            if (generated == null) return;
            foreach (var filter in generated.GetComponentsInChildren<MeshFilter>()) Release(filter.sharedMesh);
            foreach (var collider in generated.GetComponentsInChildren<MeshCollider>()) Release(collider.sharedMesh);
            Release(generated);
            generated = null;
            Collider = null;
        }

        private static void Release(Object item)
        {
            if (item == null) return;
            if (Application.isPlaying) Destroy(item);
            else DestroyImmediate(item);
        }

        private float Noise(float x, float z, float scale, float offset)
            => Mathf.PerlinNoise(x * scale + offset + seed * 17.3f, z * scale - offset + seed * 31.1f) - .5f;

        private float Meadow(float x, float z)
            => Noise(x, z, .018f, 3) * 2.4f + Noise(x, z, .05f, 11) * .7f + Noise(x, z, .16f, 29) * .12f;

        // Signed distance (metres) to the shore of the still water: negative over the water.
        // A bay begins just beyond the meadow's north edge, straight beyond the house; the lake
        // spreads north of it under the great peak and opens east into water that reaches the
        // horizon. The shoreline wanders, less so close to the meadow.
        public float ShoreDistance(float x, float z)
        {
            static float Ellipse(float x, float z, float cx, float cz, float rx, float rz)
            {
                float u = (x - cx) / rx, v = (z - cz) / rz;
                return (Mathf.Sqrt(u * u + v * v) - 1) * Mathf.Min(rx, rz);
            }
            float bay = Ellipse(x, z, -20, 400, 380, 280);
            float lake = Ellipse(x, z, -250, 1500, 1700, 850);
            float open = Ellipse(x, z, 6200, 1200, 5800, 3000);
            float wander = Mathf.SmoothStep(.25f, 1, Mathf.InverseLerp(150, 700, Mathf.Sqrt(x * x + z * z)));
            return Mathf.Min(bay, Mathf.Min(lake, open)) + Noise(x, z, .003f, 97) * 260 * wander + Noise(x, z, .011f, 61) * 60;
        }

        // Ridged noise: sharp crests and soft valleys, for mountain ranges. About 0..1.9.
        private float Ridged(float x, float z, float scale, float offset)
        {
            float sum = 0, amplitude = 1, weight = 1, frequency = scale;
            for (int i = 0; i < 5; i++)
            {
                float n = 1 - Mathf.Abs(Mathf.PerlinNoise(x * frequency + offset + seed * 7.1f + i * 13.7f, z * frequency - offset + seed * 3.3f - i * 9.1f) * 2 - 1);
                n *= n * weight;
                weight = Mathf.Clamp01(n * 1.6f);
                sum += n * amplitude;
                amplitude *= .5f;
                frequency *= 2.03f;
            }
            return sum;
        }

        // Layers, as a painter stacks them: low ridges in the middle distance, ranges far off that
        // stand highest to the north-north-west and lowest to the south, and one great snow peak
        // beyond the lake. Each layer is lower than it is far, so the sky keeps the frame.
        private float Mountains(float x, float z, float r)
        {
            if (r < 1200) return 0;
            float bearing = Mathf.Atan2(x, z);
            float north = Mathf.Max(0, Mathf.Cos(bearing + 30 * Mathf.Deg2Rad));
            float ridges = Ridged(x, z, .0009f, 5) * (90 + 90 * north) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(1300, 2600, r));
            float ranges = Ridged(x, z, .00032f, 11) * (220 + 330 * north) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(3600, 6400, r));
            float dx = x + 900, dz = z - 5600;
            float peak = Mathf.Exp(-(dx * dx + dz * dz) / (1500f * 1500f)) * 1300 * (.78f + .22f * Ridged(x, z, .0011f, 29));
            return Mathf.Max(ridges + ranges, ranges * .5f + peak);
        }

        // Beyond the meadow: rolling hills, the land settling down to the water, the lake bed, and
        // the ranges. Nothing of it reaches inside the meadow's square, so the walkable ground (and
        // the navigation baked on it) stays exactly as it was. Towards the water the meadow ends on a
        // bluff and the land falls away to a valley lake, so the eye looks down over it to the water,
        // as from the castle's slope in Luis's first reference; elsewhere hills roll up as before.
        private float Far(float x, float z)
        {
            float r = Mathf.Sqrt(x * x + z * z);
            if (r <= innerHalf * 1.05f) return 0;
            float square = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
            float outside = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(innerHalf, innerHalf + 15, square));
            float rise = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(innerHalf * 1.05f, innerHalf * 4, r));
            float ridges = Mathf.Abs(Noise(x, z, .004f, 41)) * 2;
            float hills = (Noise(x, z, .009f, 53) + .5f) * 26 + (1 - ridges) * 22;
            float shoreDistance = ShoreDistance(x, z);
            float toWater = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0, 1500, shoreDistance));
            float settle = toWater * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(innerHalf, innerHalf * 2.1f, square));
            // The valley floor stays a little above the water and rises gently away from the shore.
            float floor = waterLevel + 2.5f + 9 * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(30, 700, shoreDistance));
            float land = Mathf.Lerp(rise * hills * (1 - toWater * outside), floor, settle);
            land += Mountains(x, z, r) * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(200, 1200, shoreDistance));
            float water = (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-90, 30, shoreDistance)))
                          * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(innerHalf, innerHalf * 1.5f, square));
            return Mathf.Lerp(land, waterLevel - 14, water);
        }

        public float FlatWeight(float x, float z)
        {
            var local = Quaternion.Euler(0, -flatYaw, 0) * new Vector3(x - flatCenter.x, 0, z - flatCenter.y);
            float dx = Mathf.Max(0, Mathf.Abs(local.x) - flatSize.x * .5f);
            float dz = Mathf.Max(0, Mathf.Abs(local.z) - flatSize.y * .5f);
            return 1 - Mathf.SmoothStep(0, 1, Mathf.Sqrt(dx * dx + dz * dz) / 3.5f);
        }

        public float PathDistance(float x, float z)
        {
            float best = float.MaxValue;
            var p = new Vector2(x, z);
            for (int i = 0; i + 1 < path.Length; i++)
            {
                var a = path[i];
                var b = path[i + 1];
                var ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-4f));
                best = Mathf.Min(best, Vector2.Distance(p, a + ab * t));
            }
            return best;
        }

        // 1 on the worn middle of the path, fading to 0 at its edges; the edge wanders a little.
        public float PathWeight(float x, float z)
        {
            if (path.Length < 2) return 0;
            float wander = Noise(x, z, .35f, 71) * .5f;
            return 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(pathWidth * .35f, pathWidth * .5f + .25f, PathDistance(x, z) + wander));
        }

        public float Height(float x, float z)
        {
            float h = Meadow(x, z) + Far(x, z);
            // The house site and the path only exist in and near the meadow.
            if (Mathf.Abs(x) > innerHalf * 2 || Mathf.Abs(z) > innerHalf * 2) return h;
            float flat = FlatWeight(x, z);
            if (flat > 0) h = Mathf.Lerp(h, Meadow(flatCenter.x, flatCenter.y), flat);
            h -= PathWeight(x, z) * .07f;
            return h;
        }

        // Grass grows everywhere in the meadow except on the path, under the house, and thins at the meadow's edge.
        public float GrassDensity(float x, float z)
        {
            float edge = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
            float fade = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(innerHalf * .82f, innerHalf * .98f, edge));
            var local = Quaternion.Euler(0, -flatYaw, 0) * new Vector3(x - flatCenter.x, 0, z - flatCenter.y);
            bool underHouse = Mathf.Abs(local.x) < flatSize.x * .5f - .9f && Mathf.Abs(local.z) < flatSize.y * .5f - .9f;
            float worn = 1 - PathWeight(x, z) * 1.15f;
            return underHouse ? 0 : Mathf.Clamp01(worn) * fade;
        }

        public Vector3 Normal(float x, float z) => Normal(x, z, .25f);

        // Normals measured at the mesh's own scale, so distant slopes shade as broad planes.
        public Vector3 Normal(float x, float z, float e)
            => new Vector3(Height(x - e, z) - Height(x + e, z), 2 * e, Height(x, z - e) - Height(x, z + e)).normalized;

        // The base colour of the land at a point, shared by the meadow grid and the far land so
        // they meet without a seam: meadow to far green, plus (well beyond the meadow) forests
        // below the tree line, rock on steep slopes, snow above the snow line, and the shore.
        private Color LandColor(float x, float z, float h, Vector3 normal, float r)
        {
            var ground = Color.Lerp(meadow, far, Mathf.InverseLerp(innerHalf * .8f, innerHalf * 3, r));
            float distant = Mathf.InverseLerp(innerHalf * 1.5f, innerHalf * 3, r);
            if (distant <= 0) return ground;
            float woods = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.02f, .16f, Noise(x, z, .0035f, 91) + Noise(x, z, .02f, 7) * .25f));
            woods *= 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(480, 820, h));
            ground = Color.Lerp(ground, forest, woods * distant);
            ground = Color.Lerp(ground, rock, Mathf.InverseLerp(.82f, .62f, normal.y) * distant);
            float snowLine = 860 + Noise(x, z, .002f, 13) * 260;
            float snowy = Mathf.InverseLerp(snowLine - 60, snowLine + 60, h) * Mathf.InverseLerp(.45f, .65f, normal.y);
            ground = Color.Lerp(ground, snow, snowy);
            ground = Color.Lerp(ground, shore, Mathf.InverseLerp(waterLevel + 1.2f, waterLevel + .1f, h) * distant);
            return Color.Lerp(ground, lakeBed, Mathf.InverseLerp(waterLevel - 1, waterLevel - 8, h));
        }

        // The meadow's grid coordinates, at the walkable spacing.
        private float[] Axis()
        {
            var values = new List<float>();
            int cells = Mathf.RoundToInt(innerHalf * 2 / innerSpacing);
            for (int i = 0; i <= cells; i++) values.Add(-innerHalf + i * innerSpacing);
            return values.ToArray();
        }

        // The meadow as a fine grid; for the visible land, the far land continues from its edge
        // as rings that round from the meadow's square into circles and widen geometrically.
        public Mesh BuildMesh(bool innerOnly)
        {
            var axis = Axis();
            int n = axis.Length;
            var vertices = new List<Vector3>(n * n);
            var normals = new List<Vector3>(n * n);
            var colors = new List<Color>(n * n);
            for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                float x = axis[i], z = axis[j];
                float h = Height(x, z);
                vertices.Add(new Vector3(x, h, z));
                if (innerOnly) continue;
                var normal = Normal(x, z);
                normals.Add(normal);
                float r = Mathf.Sqrt(x * x + z * z);
                var ground = LandColor(x, z, h, normal, r);
                float under = r < innerHalf ? GrassDensity(x, z) : 0;
                ground *= Mathf.Lerp(1, .5f, under);
                ground = Color.Lerp(ground, dirt, PathWeight(x, z));
                ground *= 1 + Noise(x, z, .3f, 83) * .25f;
                // Alpha: how much the soil sits in the grass's shade (1 = open sky).
                ground.a = Mathf.Lerp(1, .4f, under);
                colors.Add(ground);
            }
            var triangles = new List<int>((n - 1) * (n - 1) * 6);
            for (int j = 0; j < n - 1; j++)
            for (int i = 0; i < n - 1; i++)
            {
                int a = j * n + i, b = a + 1, c = a + n, d = c + 1;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
            if (!innerOnly) AppendFarLand(vertices, normals, colors, triangles, n);
            var mesh = new Mesh { name = innerOnly ? "Ordinary ground collider" : "Ordinary ground", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            if (innerOnly) mesh.RecalculateNormals();
            else
            {
                mesh.SetNormals(normals);
                mesh.SetColors(colors);
            }
            mesh.RecalculateBounds();
            return mesh;
        }

        private void AppendFarLand(List<Vector3> vertices, List<Vector3> normals, List<Color> colors, List<int> triangles, int n)
        {
            // The meadow grid's edge, anticlockwise seen from above, shared so the two never part.
            var edge = new List<int>();
            for (int i = 0; i < n - 1; i++) edge.Add(i);
            for (int j = 0; j < n - 1; j++) edge.Add(j * n + n - 1);
            for (int i = n - 1; i > 0; i--) edge.Add((n - 1) * n + i);
            for (int j = n - 1; j > 0; j--) edge.Add(j * n);
            int count = edge.Count;
            var previous = edge.ToArray();
            float growth = Mathf.Pow(outerHalf / (innerHalf * 1.2f), 1f / outerRings);
            for (int k = 1; k <= outerRings; k++)
            {
                var current = new int[count];
                float widen = Mathf.Pow(growth, k);
                float round = Mathf.SmoothStep(0, 1, Mathf.Clamp01(k / 18f));
                for (int s = 0; s < count; s++)
                {
                    var b = vertices[edge[s]];
                    var flat = new Vector2(b.x, b.z);
                    float length = flat.magnitude;
                    var direction = flat / length;
                    float radius = Mathf.Lerp(length, innerHalf * 1.2f, round) * widen;
                    float x = direction.x * radius, z = direction.y * radius;
                    float h = Height(x, z);
                    var normal = Normal(x, z, Mathf.Max(.25f, radius * .012f));
                    current[s] = vertices.Count;
                    vertices.Add(new Vector3(x, h, z));
                    normals.Add(normal);
                    // Broad patches of warmer and cooler green, as a painter varies a far field.
            float patch = Noise(x, z, .006f, 37) + Noise(x, z, .025f, 53) * .5f;
            var ground = LandColor(x, z, h, normal, radius) * (1 + Noise(x, z, .3f, 83) * .25f + patch * .35f);
            ground.b *= 1 - patch * .4f;
                    ground.a = 1;
                    colors.Add(ground);
                }
                for (int s = 0; s < count; s++)
                {
                    int s2 = (s + 1) % count;
                    int a = previous[s], b = previous[s2], c = current[s], d = current[s2];
                    triangles.Add(a); triangles.Add(b); triangles.Add(c);
                    triangles.Add(b); triangles.Add(d); triangles.Add(c);
                }
                previous = current;
            }
        }
    }
}
