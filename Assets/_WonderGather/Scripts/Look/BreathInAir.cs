using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // Breath seen in cold air (S3b; one of the looks of breath for Luis to choose between). Whatever breathes
    // tells it of each breath out (Puff); it keeps a few soft puffs, each leaving the mouth, slowing, rising a
    // little, swelling and thinning away, and draws them as the chimney's smoke is drawn. Nothing shows by
    // day: the air is taken to be cold from dusk to dawn (as the fireflies are out then).
    public sealed class BreathInAir : MonoBehaviour
    {
        private const int Most = 8;
        // A puff lasts this long (seconds); it leaves the mouth at this speed (metres a second), loses it at this
        // rate (a second), and rises at this (metres a second); it grows from this to this (in the size it is
        // told, which is the head's).
        private const float Lasts = 1.7f, Leaves = .5f, Slows = 2.4f, Rises = .07f, SmallAs = .3f, LargeAs = 1.5f;
        private static Material look;
        private static bool sought;
        private readonly Vector4[] at = new Vector4[Most], state = new Vector4[Most];
        private readonly Vector3[] goes = new Vector3[Most];
        private readonly float[] age = new float[Most], shows = new float[Most], sized = new float[Most], own = new float[Most];
        private int next;
        private uint chance = 11;
        private TimeOfDay time;
        private MaterialPropertyBlock properties;

        // How many puffs it has breathed out, and how many show now (for whoever tests it).
        public int Puffed { get; private set; }
        public int Showing { get { int n = 0; for (int k = 0; k < Most; k++) if (age[k] < 1 && shows[k] > 0) n++; return n; } }
        // How cold the air is taken to be: nought by day, one by night.
        public float Cold => time == null ? 0 : 1 - Mathf.InverseLerp(-.12f, .1f, time.SunDirection(time.Hour).y);

        // (The look is an asset made by the editor: Resources/BreathInAir, of the shader "Wonder Gather/Breath".)
        public static Material Look()
        {
            if (!sought) { sought = true; look = Resources.Load<Material>("BreathInAir"); }
            return look;
        }

        private float Chance() { chance = chance * 1664525u + 1013904223u; return (chance >> 8) / 16777216f; }

        private void Awake()
        {
            time = FindAnyObjectByType<TimeOfDay>();
            for (int k = 0; k < Most; k++) age[k] = 1;
        }

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += Draw;
        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= Draw;

        // A breath goes out: from a place, a way, so strongly (one: a breath at rest), from a head of this size.
        public void Puff(Vector3 from, Vector3 way, float strength, float head)
        {
            float cold = Cold;
            if (cold <= .02f || way.sqrMagnitude < 1e-6f) return;
            int k = next;
            next = (next + 1) % Most;
            Vector3 aside = Vector3.Cross(Vector3.up, way).normalized;
            goes[k] = (way.normalized + aside * (.25f * (2 * Chance() - 1)) + Vector3.up * (.15f * Chance())) * (Leaves * Mathf.Sqrt(Mathf.Max(.2f, strength)) * Mathf.Lerp(.8f, 1.2f, Chance()));
            at[k] = from;
            age[k] = 0;
            shows[k] = cold * Mathf.Clamp01(.5f + .5f * strength);
            sized[k] = head;
            own[k] = Chance();
            Puffed++;
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, .05f);
            for (int k = 0; k < Most; k++)
            {
                if (age[k] >= 1) continue;
                age[k] = Mathf.Min(1, age[k] + dt / Lasts);
                goes[k] *= Mathf.Exp(-Slows * dt);
                Vector3 place = (Vector3)at[k] + (goes[k] + Vector3.up * Rises) * dt;
                at[k] = new Vector4(place.x, place.y, place.z, sized[k] * Mathf.Lerp(SmallAs, LargeAs, Mathf.Sqrt(age[k])));
            }
        }

        private void Draw(ScriptableRenderContext context, Camera camera)
        {
            var material = Look();
            if (material == null || Showing == 0 || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            properties ??= new MaterialPropertyBlock();
            for (int k = 0; k < Most; k++) state[k] = new Vector4(age[k], own[k], age[k] < 1 ? shows[k] : 0, 0);
            properties.SetVectorArray("_WG_BreathAt", at);
            properties.SetVectorArray("_WG_BreathIs", state);
            var parameters = new RenderParams(material)
            {
                worldBounds = new Bounds(transform.position + Vector3.up, new Vector3(6, 5, 6)),
                matProps = properties,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                camera = camera,
                layer = gameObject.layer
            };
            Graphics.RenderPrimitives(parameters, MeshTopology.Triangles, 6, Most);
        }
    }
}
