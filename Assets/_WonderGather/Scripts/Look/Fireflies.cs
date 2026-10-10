using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // Fireflies over the meadow at dusk and night (see WGFireflies.shader). They come out as the
    // sun sets and are gone by day. Each is its own thing in the world: it has a home in the
    // meadow, found once, and roams round it whatever the camera does. What the camera changes is
    // only what is seen of them: from close all that are out, at their brightest; from further
    // fewer of them, and fainter (Luis, October 8: "its own entities", "they appear based on your
    // distance from them, and when it's very far away, there are less of them that you see, and
    // they are less bright").
    [ExecuteAlways]
    public sealed class Fireflies : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private TimeOfDay time;
        [SerializeField] private OrdinaryGround ground;
        [Tooltip("How much meadow there is to each firefly (m2). They live where the grass grows.")]
        [SerializeField] private float meadowEach = 20;
        [Tooltip("Without a ground to live on: the half-size (m) of the meadow, and its level.")]
        [SerializeField] private float halfWithoutGround = 60, levelWithoutGround;
        [Tooltip("From this near (m) all that are out are seen, at their brightest; from this far the fewest and faintest.")]
        [SerializeField] private float allSeenWithin = 12, fewestFrom = 50;
        [Tooltip("From the far distance: the share of them still seen, and how bright those are.")]
        [Range(0, 1)] [SerializeField] private float seenFromFar = .08f, brightFromFar = .15f;
        [Tooltip("Between these distances (m) the last of them fade from sight.")]
        [SerializeField] private float fadeFrom = 85, goneBy = 110;
        [SerializeField] private int seed = 31;

        private readonly List<Vector4> homes = new List<Vector4>();
        private MaterialPropertyBlock properties;
        private GraphicsBuffer buffer;
        private Bounds lives;

        public void Configure(Material fireflies, TimeOfDay day, OrdinaryGround meadow = null)
        {
            material = fireflies;
            time = day;
            ground = meadow;
            Forget();
        }

        // How many are out at this hour: none by day, all once the sun is well down.
        public float Presence => time == null ? 1 : 1 - Mathf.InverseLerp(-.12f, .1f, time.SunDirection(time.Hour).y);

        // Where each lives (xyz, the ground under it) and the number it is told apart by (w).
        public IReadOnlyList<Vector4> Homes
        {
            get
            {
                if (homes.Count == 0) Settle();
                return homes;
            }
        }

        public float MeadowEach => meadowEach;

        // Of those that are out, the share seen from this far, and how bright those are (1 from close).
        public float ShareSeen(float distance) => Mathf.Lerp(1, seenFromFar, Far(distance)) * Last(distance);
        public float Brightness(float distance) => Mathf.Lerp(1, brightFromFar, Far(distance)) * Last(distance);
        private float Far(float distance) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(allSeenWithin, fewestFrom, distance));
        private float Last(float distance) => 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(fadeFrom, goneBy, distance));

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += Draw;

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= Draw;
            Forget();
        }

        private void OnValidate() => Forget();

        private void Forget()
        {
            homes.Clear();
            buffer?.Release();
            buffer = null;
        }

        // The homes: one to each patch of meadow, somewhere in it, where grass grows (not on the
        // path, not under the house, thinning at the meadow's edge). The same every time.
        private void Settle()
        {
            homes.Clear();
            if (ground == null) ground = FindAnyObjectByType<OrdinaryGround>();
            float half = ground != null ? ground.InnerHalf : halfWithoutGround;
            float patch = Mathf.Sqrt(Mathf.Max(1, meadowEach));
            int across = Mathf.CeilToInt(2 * half / patch);
            var chance = new System.Random(seed);
            float top = float.MinValue, bottom = float.MaxValue;
            for (int i = 0; i < across; i++)
                for (int j = 0; j < across; j++)
                {
                    float x = -half + (i + (float)chance.NextDouble()) * patch, z = -half + (j + (float)chance.NextDouble()) * patch;
                    float grows = (float)chance.NextDouble(), told = (float)chance.NextDouble();
                    if (ground != null && grows >= ground.GrassDensity(x, z)) continue;
                    float level = ground != null ? ground.Height(x, z) : levelWithoutGround;
                    homes.Add(new Vector4(x, level, z, told));
                    top = Mathf.Max(top, level);
                    bottom = Mathf.Min(bottom, level);
                }
            if (homes.Count == 0) return;
            lives = new Bounds(new Vector3(0, (top + bottom) * .5f + 1, 0), new Vector3(2 * half + 12, top - bottom + 8, 2 * half + 12));
        }

        private void Draw(ScriptableRenderContext context, Camera camera)
        {
            if (material == null || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            float presence = Presence;
            if (presence <= 0) return;
            if (homes.Count == 0) Settle();
            if (homes.Count == 0) return;
            if (buffer == null || !buffer.IsValid() || buffer.count != homes.Count)
            {
                buffer?.Release();
                buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, homes.Count, 16);
                buffer.SetData(homes);
            }
            properties ??= new MaterialPropertyBlock();
            properties.SetBuffer("_WG_FireflyHomes", buffer);
            properties.SetVector("_WG_FireflySight", new Vector4(allSeenWithin, fewestFrom, seenFromFar, brightFromFar));
            properties.SetVector("_WG_FireflyOut", new Vector4(presence, fadeFrom, goneBy, 0));
            var parameters = new RenderParams(material)
            {
                worldBounds = lives,
                matProps = properties,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                camera = camera,
                layer = gameObject.layer
            };
            Graphics.RenderPrimitives(parameters, MeshTopology.Triangles, homes.Count * 6, 1);
        }
    }
}
