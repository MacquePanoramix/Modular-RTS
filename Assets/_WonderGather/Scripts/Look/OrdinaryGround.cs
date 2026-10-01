using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace WonderGather
{
    // The Ordinary Place's land: one deterministic height field shared by the terrain mesh,
    // its collider, the grass and the path, so they always agree. Gentle meadow near the
    // house, a worn path to the door, and hills that rise with distance for aerial depth.
    // The meshes are rebuilt whenever the scene loads instead of being stored as large assets.
    [ExecuteAlways]
    public sealed class OrdinaryGround : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private int seed = 7;
        [Tooltip("Half-size of the walkable, explorable meadow, in metres.")]
        [SerializeField] private float innerHalf = 60, innerSpacing = .5f;
        [Tooltip("The land continues as scenery out to this distance.")]
        [SerializeField] private float outerHalf = 1500;
        [SerializeField] private int outerSteps = 36;
        [SerializeField] private Vector2[] path = new Vector2[0];
        [SerializeField] private float pathWidth = 1.7f;
        [SerializeField] private Vector2 flatCenter, flatSize = new Vector2(9, 7);
        [SerializeField] private float flatYaw;
        [SerializeField] private Color meadow = new Color(.13f, .20f, .11f), dirt = new Color(.40f, .31f, .21f), far = new Color(.09f, .13f, .11f);

        private GameObject generated;

        public float InnerHalf => innerHalf;
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

        // The visible land (out to the far hills) and the meadow's walkable collider.
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

        // Hills begin beyond the meadow and grow into distant ranges.
        private float Hills(float x, float z)
        {
            float r = Mathf.Sqrt(x * x + z * z);
            if (r <= innerHalf * 1.05f) return 0;
            float rise = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(innerHalf * 1.05f, innerHalf * 4, r));
            float ridges = Mathf.Abs(Noise(x, z, .004f, 41)) * 2;
            float hills = (Noise(x, z, .009f, 53) + .5f) * 26 + (1 - ridges) * 22;
            float ranges = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(260, 900, r)) * ((Noise(x, z, .0022f, 67) + .5f) * 140 + 30);
            return rise * hills + ranges;
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
            float h = Meadow(x, z) + Hills(x, z);
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

        public Vector3 Normal(float x, float z)
        {
            const float e = .25f;
            return new Vector3(Height(x - e, z) - Height(x + e, z), 2 * e, Height(x, z - e) - Height(x, z + e)).normalized;
        }

        // Grid coordinates: fine in the meadow, growing geometrically out to the far land.
        private float[] Axis(bool innerOnly)
        {
            var values = new List<float>();
            int cells = Mathf.RoundToInt(innerHalf * 2 / innerSpacing);
            for (int i = 0; i <= cells; i++) values.Add(-innerHalf + i * innerSpacing);
            if (innerOnly) return values.ToArray();
            float growth = Mathf.Pow(outerHalf / innerHalf, 1f / outerSteps);
            var outer = new List<float>();
            for (int i = 1; i <= outerSteps; i++) outer.Add(innerHalf * Mathf.Pow(growth, i));
            var result = new List<float>();
            for (int i = outer.Count - 1; i >= 0; i--) result.Add(-outer[i]);
            result.AddRange(values);
            result.AddRange(outer);
            return result.ToArray();
        }

        public Mesh BuildMesh(bool innerOnly)
        {
            var axis = Axis(innerOnly);
            int n = axis.Length;
            var vertices = new Vector3[n * n];
            var normals = new Vector3[n * n];
            var colors = new Color[n * n];
            for (int j = 0; j < n; j++)
            for (int i = 0; i < n; i++)
            {
                float x = axis[i], z = axis[j];
                int k = j * n + i;
                vertices[k] = new Vector3(x, Height(x, z), z);
                normals[k] = Normal(x, z);
                if (innerOnly) continue;
                float r = Mathf.Sqrt(x * x + z * z);
                var ground = Color.Lerp(meadow, far, Mathf.InverseLerp(innerHalf * .8f, innerHalf * 3, r));
                float under = r < innerHalf ? GrassDensity(x, z) : 0;
                ground *= Mathf.Lerp(1, .5f, under);
                ground = Color.Lerp(ground, dirt, PathWeight(x, z));
                float variation = Noise(x, z, .3f, 83) * .25f;
                ground *= 1 + variation;
                // Alpha: how much the soil sits in the grass's shade (1 = open sky).
                ground.a = Mathf.Lerp(1, .4f, under);
                colors[k] = ground;
            }
            var triangles = new int[(n - 1) * (n - 1) * 6];
            int t = 0;
            for (int j = 0; j < n - 1; j++)
            for (int i = 0; i < n - 1; i++)
            {
                int a = j * n + i, b = a + 1, c = a + n, d = c + 1;
                triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
            }
            var mesh = new Mesh { name = innerOnly ? "Ordinary ground collider" : "Ordinary ground", indexFormat = IndexFormat.UInt32 };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            if (innerOnly) mesh.RecalculateNormals();
            else
            {
                mesh.normals = normals;
                mesh.colors = colors;
            }
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
