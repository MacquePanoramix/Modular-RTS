using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace WonderGather
{
    // The sky's architecture: towering cumulus on the horizon, heaps drifting at middle distance
    // and long low banks along the far edge. The layout is deterministic, composed so that great
    // towers stand behind the mountains and over the water, and every cloud drifts slowly with
    // the wind, forming as it enters the sky and dissolving as it leaves.
    // The clouds are rebuilt when the scene loads rather than stored.
    [ExecuteAlways]
    public sealed class CloudBank : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private Mesh[] towers = new Mesh[0], heaps = new Mesh[0], banks = new Mesh[0];
        [SerializeField] private int seed = 3;
        [Tooltip("Metres per second along the wind.")]
        [SerializeField] private float drift = 5;
        [SerializeField] private Vector2 driftDirection = new Vector2(.82f, .57f);
        [Tooltip("Clouds leave the sky beyond this distance and re-form on the far side.")]
        [SerializeField] private float skyRadius = 9400;
        [SerializeField] private int heapCount = 11, bankCount = 8;

        private sealed class Cloud
        {
            public Transform Body;
            public Vector3 Start;
            public Vector3 Scale;
        }

        private readonly List<Cloud> clouds = new List<Cloud>();
        private GameObject generated;
        private float elapsed;

        public int Count => clouds.Count;

        public void Configure(Material cloud, Mesh[] towerMeshes, Mesh[] heapMeshes, Mesh[] bankMeshes)
        {
            material = cloud;
            towers = towerMeshes;
            heaps = heapMeshes;
            banks = bankMeshes;
            if (isActiveAndEnabled) { Clear(); Build(); }
        }

        private void OnEnable() => Build();
        private void OnDisable() => Clear();

        private void Build()
        {
            if (material == null || generated != null || towers.Length == 0) return;
            generated = new GameObject("Clouds (generated)") { hideFlags = HideFlags.DontSave };
            generated.transform.SetParent(transform, false);
            var random = new System.Random(seed);
            float R(float a, float b) => a + (float)random.NextDouble() * (b - a);
            Vector3 At(float bearing, float distance, float altitude)
            {
                float a = bearing * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(a) * distance, altitude, Mathf.Cos(a) * distance);
            }

            // Towers: bearing (degrees from north, clockwise), distance, size. North looks over the
            // lake to the great peak; east opens to the water's horizon.
            var towerPlan = new (float Bearing, float Distance, float Size)[]
            {
                (18, 6600, 1150), (68, 7200, 1250), (-38, 6200, 900), (122, 7600, 1050), (205, 6400, 1000), (262, 7000, 850),
            };
            for (int i = 0; i < towerPlan.Length; i++)
            {
                var (bearing, distance, size) = towerPlan[i];
                Add(towers[i % towers.Length], At(bearing, distance, R(780, 880)), new Vector3(size, size * R(1.1f, 1.25f), size), R(0, 360));
            }
            for (int i = 0; i < heapCount; i++)
            {
                float size = R(330, 680);
                Add(heaps[i % heaps.Length], At(R(0, 360), R(1900, 5600), R(900, 1100)), new Vector3(size, size * R(.9f, 1.15f), size), R(0, 360));
            }
            for (int i = 0; i < bankCount; i++)
            {
                float size = R(300, 430);
                // Banks lie across the line of sight, so they read as long strokes on the horizon.
                float bearing = R(0, 360);
                Add(banks[i % banks.Length], At(bearing, R(7000, 8200), R(420, 560)), new Vector3(size, size * R(.8f, 1.1f), size), bearing + 90 + R(-12, 12));
            }
            Place();
        }

        private void Add(Mesh mesh, Vector3 position, Vector3 scale, float yaw)
        {
            if (mesh == null) return;
            var body = new GameObject(mesh.name) { hideFlags = HideFlags.DontSave };
            body.transform.SetParent(generated.transform, false);
            body.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            body.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = body.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            clouds.Add(new Cloud { Body = body.transform, Start = position, Scale = scale });
        }

        private void Clear()
        {
            clouds.Clear();
            if (generated == null) return;
            if (Application.isPlaying) Destroy(generated);
            else DestroyImmediate(generated);
            generated = null;
        }

        private void Update()
        {
            if (!Application.isPlaying) return;
            elapsed += Time.deltaTime;
            Place();
        }

        // Each cloud travels with the wind across a disc of sky. Past its edge it re-forms on the
        // far side; near the edge it shrinks, so clouds form and dissolve rather than pop.
        private void Place()
        {
            var wind = new Vector3(driftDirection.x, 0, driftDirection.y).normalized;
            float span = skyRadius * 2;
            foreach (var cloud in clouds)
            {
                var flat = new Vector3(cloud.Start.x, 0, cloud.Start.z) + wind * (elapsed * drift);
                // Wrap along the wind: measured from the disc's upwind edge.
                float along = Vector3.Dot(flat, wind);
                var across = flat - wind * along;
                along = Mathf.Repeat(along + skyRadius, span) - skyRadius;
                flat = wind * along + across;
                float presence = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(skyRadius, skyRadius * .9f, flat.magnitude));
                cloud.Body.position = new Vector3(flat.x, cloud.Start.y, flat.z);
                cloud.Body.localScale = new Vector3(cloud.Scale.x, cloud.Scale.y * Mathf.Max(presence, .001f), cloud.Scale.z) * Mathf.Lerp(.6f, 1, presence);
            }
        }
    }
}
