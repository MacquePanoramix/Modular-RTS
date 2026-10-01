using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // Plants and draws the Ordinary Place's grass on the GPU: dense blades plus taller
    // seed heads, chunked for culling. Each chunk's blades are shuffled, so drawing only
    // the first part of a chunk thins it evenly with distance.
    [ExecuteAlways]
    public sealed class GrassField : MonoBehaviour
    {
        [SerializeField] private OrdinaryGround ground;
        [SerializeField] private Material bladeMaterial, plumeMaterial;
        [SerializeField] private float bladesPerSquareMetre = 48, plumesPerSquareMetre = .35f;
        [SerializeField] private float chunkSize = 10, fullDensityDistance = 16, drawDistance = 75;
        [SerializeField] private Vector2 bladeHeight = new Vector2(.3f, .78f), bladeWidth = new Vector2(.035f, .07f);
        [SerializeField] private Vector2 plumeHeight = new Vector2(.95f, 1.45f);
        [SerializeField] private int seed = 11;

        private sealed class Chunk
        {
            public Bounds Bounds;
            public GraphicsBuffer Blades, Arguments;
            public int Count;
            public MaterialPropertyBlock Properties;
            public bool Plume;
        }

        private readonly List<Chunk> chunks = new List<Chunk>();
        private readonly GraphicsBuffer.IndirectDrawIndexedArgs[] arguments = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        private readonly Vector4[] movers = new Vector4[4];
        private readonly List<Transform> bodies = new List<Transform>();
        private Mesh bladeMesh, plumeMesh;
        private float nextBodyScan;
        public int BladeCount { get; private set; }
        public int DrawnLastFrame { get; private set; }

        public void Configure(OrdinaryGround land, Material blade, Material plume)
        {
            ground = land;
            bladeMaterial = blade;
            plumeMaterial = plume;
            if (!isActiveAndEnabled) return;
            Release();
            Build();
        }

        private void OnEnable()
        {
            Build();
            RenderPipelineManager.beginCameraRendering += Draw;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= Draw;
            Release();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            Release();
            Build();
        }

        private void Release()
        {
            foreach (var chunk in chunks)
            {
                chunk.Blades?.Release();
                chunk.Arguments?.Release();
            }
            chunks.Clear();
            BladeCount = 0;
        }

        private static Mesh BladeMesh()
        {
            // A tapering strip; the shader narrows it towards the tip and bends it.
            var vertices = new List<Vector3>();
            for (int i = 0; i < 4; i++)
            {
                float y = i / 4f;
                vertices.Add(new Vector3(-.5f, y, 0));
                vertices.Add(new Vector3(.5f, y, 0));
            }
            vertices.Add(new Vector3(0, 1, 0));
            var triangles = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                int a = i * 2;
                triangles.AddRange(new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });
            }
            triangles.AddRange(new[] { 6, 8, 7 });
            var mesh = new Mesh { name = "Grass blade", vertices = vertices.ToArray(), triangles = triangles.ToArray() };
            mesh.bounds = new Bounds(Vector3.up * .5f, Vector3.one * 2);
            return mesh;
        }

        private static Mesh PlumeMesh()
        {
            // A thin stem with a feathery head: several narrow cards fanning out near the top.
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            void Card(float x0, float y0, float y1, float width, float lean)
            {
                int start = vertices.Count;
                for (int i = 0; i <= 3; i++)
                {
                    float y = Mathf.Lerp(y0, y1, i / 3f);
                    float taper = 1 - y * .85f;
                    float w = width * Mathf.Sin((i / 3f) * Mathf.PI * .9f + .15f) / taper;
                    float x = (x0 + lean * (y - y0)) / taper;
                    vertices.Add(new Vector3(x - w, y, 0));
                    vertices.Add(new Vector3(x + w, y, 0));
                }
                for (int i = 0; i < 3; i++)
                {
                    int a = start + i * 2;
                    triangles.AddRange(new[] { a, a + 2, a + 1, a + 1, a + 2, a + 3 });
                }
            }
            // A thin stem, then a spray of fine strands arching out of its top.
            Card(0, 0, .74f, .35f, 0);
            for (int s = 0; s < 6; s++)
            {
                float spread = (s - 2.5f) / 2.5f;
                Card(spread * .15f, .64f + Mathf.Abs(spread) * .04f, 1f - Mathf.Abs(spread) * .08f, .55f, spread * 1.4f);
            }
            var mesh = new Mesh { name = "Grass plume", vertices = vertices.ToArray(), triangles = triangles.ToArray() };
            mesh.bounds = new Bounds(Vector3.up * .5f, Vector3.one * 2);
            return mesh;
        }

        private void Build()
        {
            if (ground == null || bladeMaterial == null) return;
            bladeMesh ??= BladeMesh();
            plumeMesh ??= PlumeMesh();
            float half = ground.InnerHalf;
            int cells = Mathf.CeilToInt(half * 2 / chunkSize);
            for (int j = 0; j < cells; j++)
            for (int i = 0; i < cells; i++)
            {
                float x0 = -half + i * chunkSize, z0 = -half + j * chunkSize;
                var random = new System.Random(seed + j * 7919 + i * 104729);
                AddChunk(random, x0, z0, false);
                if (plumeMaterial != null) AddChunk(random, x0, z0, true);
            }
        }

        private void AddChunk(System.Random random, float x0, float z0, bool plume)
        {
            float rate = plume ? plumesPerSquareMetre : bladesPerSquareMetre;
            int candidates = Mathf.RoundToInt(rate * chunkSize * chunkSize);
            var data = new List<Vector4>(candidates * 2);
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int k = 0; k < candidates; k++)
            {
                float x = x0 + (float)random.NextDouble() * chunkSize;
                float z = z0 + (float)random.NextDouble() * chunkSize;
                // Clumps: grass gathers in tufts and drifts rather than spreading evenly.
                float clump = Mathf.PerlinNoise(x * .21f + seed, z * .21f - seed);
                float density = ground.GrassDensity(x, z) * Mathf.Lerp(.55f, 1.25f, clump);
                if (plume) density *= Mathf.SmoothStep(0, 1, (Mathf.PerlinNoise(x * .07f + 40, z * .07f + 9) - .35f) * 3);
                if ((float)random.NextDouble() > density) continue;
                float y = ground.Height(x, z);
                float r = (float)random.NextDouble();
                float tall = Mathf.Lerp(.7f, 1.55f, clump * clump) * Mathf.Lerp(.5f, 1, 1 - ground.PathWeight(x, z));
                float height = plume ? Mathf.Lerp(plumeHeight.x, plumeHeight.y, r) : Mathf.Lerp(bladeHeight.x, bladeHeight.y, r) * tall;
                float width = plume ? .03f : Mathf.Lerp(bladeWidth.x, bladeWidth.y, (float)random.NextDouble());
                float bend = Mathf.Lerp(.15f, plume ? .4f : .95f, (float)random.NextDouble());
                data.Add(new Vector4(x, y - .02f, z, (float)random.NextDouble() * Mathf.PI * 2));
                data.Add(new Vector4(height, width, bend, (float)random.NextDouble()));
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y + height);
            }
            int count = data.Count / 2;
            if (count == 0) return;
            // Shuffle so any prefix of the list is an even sample of the chunk.
            for (int k = count - 1; k > 0; k--)
            {
                int m = random.Next(k + 1);
                (data[k * 2], data[m * 2]) = (data[m * 2], data[k * 2]);
                (data[k * 2 + 1], data[m * 2 + 1]) = (data[m * 2 + 1], data[k * 2 + 1]);
            }
            var chunk = new Chunk
            {
                Count = count,
                Plume = plume,
                Blades = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.Count, 16),
                Arguments = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size),
                Properties = new MaterialPropertyBlock()
            };
            chunk.Blades.SetData(data);
            var center = new Vector3(x0 + chunkSize * .5f, (minY + maxY) * .5f, z0 + chunkSize * .5f);
            chunk.Bounds = new Bounds(center, new Vector3(chunkSize + 2, maxY - minY + 2, chunkSize + 2));
            chunk.Properties.SetBuffer("_Blades", chunk.Blades);
            chunk.Properties.SetFloat("_HeightScale", 1);
            chunk.Properties.SetVectorArray("_Movers", movers);
            chunk.Properties.SetFloat("_MoverCount", 0);
            chunks.Add(chunk);
            BladeCount += count;
        }

        private void ScanBodies()
        {
            if (Time.realtimeSinceStartup < nextBodyScan) return;
            nextBodyScan = Time.realtimeSinceStartup + 1;
            bodies.Clear();
            foreach (var unit in FindObjectsByType<SelectableUnit>()) bodies.Add(unit.transform);
        }

        private void Draw(ScriptableRenderContext context, Camera camera)
        {
            if (chunks.Count == 0 || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            ScanBodies();
            int moverCount = 0;
            foreach (var body in bodies)
            {
                if (body == null || moverCount == movers.Length) continue;
                movers[moverCount++] = new Vector4(body.position.x, body.position.y, body.position.z, .55f);
            }
            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var eye = camera.transform.position;
            int drawn = 0;
            foreach (var chunk in chunks)
            {
                if (!GeometryUtility.TestPlanesAABB(planes, chunk.Bounds)) continue;
                float distance = Mathf.Sqrt(chunk.Bounds.SqrDistance(eye));
                if (distance > drawDistance) continue;
                // Full density nearby, thinning smoothly to a sparse sample at the edge of the range.
                float keep = distance <= fullDensityDistance ? 1 : Mathf.Lerp(1, .16f, Mathf.Pow(Mathf.InverseLerp(fullDensityDistance, drawDistance, distance), .6f));
                if (chunk.Plume) keep = Mathf.Max(keep, .5f);
                int count = Mathf.Max(1, Mathf.RoundToInt(chunk.Count * keep));
                var mesh = chunk.Plume ? plumeMesh : bladeMesh;
                arguments[0].indexCountPerInstance = mesh.GetIndexCount(0);
                arguments[0].instanceCount = (uint)count;
                arguments[0].startIndex = 0;
                arguments[0].baseVertexIndex = 0;
                arguments[0].startInstance = 0;
                chunk.Arguments.SetData(arguments);
                // Thinned blades grow slightly taller so the far meadow keeps its coverage.
                chunk.Properties.SetFloat("_HeightScale", Mathf.Lerp(1.4f, 1, keep));
                chunk.Properties.SetVectorArray("_Movers", movers);
                chunk.Properties.SetFloat("_MoverCount", moverCount);
                var parameters = new RenderParams(chunk.Plume ? plumeMaterial : bladeMaterial)
                {
                    worldBounds = chunk.Bounds,
                    matProps = chunk.Properties,
                    shadowCastingMode = ShadowCastingMode.Off,
                    receiveShadows = true,
                    camera = camera,
                    layer = gameObject.layer
                };
                Graphics.RenderMeshIndirect(parameters, mesh, chunk.Arguments);
                drawn += count;
            }
            DrawnLastFrame = drawn;
        }
    }
}
