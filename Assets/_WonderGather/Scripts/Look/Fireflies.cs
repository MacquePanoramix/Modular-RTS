using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // Fireflies over the meadow at dusk and night (see WGFireflies.shader). They come out as the
    // sun sets and are gone by day; all motion is on the GPU in a box that follows each camera.
    [ExecuteAlways]
    public sealed class Fireflies : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private TimeOfDay time;
        [SerializeField] private int count = 1100;
        [Tooltip("Half-size (m) of the box around the camera they live in.")]
        [SerializeField] private float extent = 22;
        [Tooltip("The meadow's ground level they keep above.")]
        [SerializeField] private float floor;
        private MaterialPropertyBlock properties;

        public void Configure(Material fireflies, TimeOfDay day)
        {
            material = fireflies;
            time = day;
        }

        // How many are out at this hour: none by day, all once the sun is well down.
        public float Presence => time == null ? 1 : 1 - Mathf.InverseLerp(-.12f, .1f, time.SunDirection(time.Hour).y);

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += Draw;
        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= Draw;

        private void Draw(ScriptableRenderContext context, Camera camera)
        {
            if (material == null || count <= 0 || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            float presence = Presence;
            if (presence <= 0) return;
            properties ??= new MaterialPropertyBlock();
            var centre = camera.transform.position + camera.transform.forward * extent * .5f;
            properties.SetVector("_WG_FireflyVolume", new Vector4(centre.x, centre.y, centre.z, extent));
            properties.SetVector("_WG_FireflyShape", new Vector4(presence, floor, 0, 0));
            var parameters = new RenderParams(material)
            {
                worldBounds = new Bounds(centre, Vector3.one * extent * 2.5f),
                matProps = properties,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                camera = camera,
                layer = gameObject.layer
            };
            Graphics.RenderPrimitives(parameters, MeshTopology.Triangles, count * 6, 1);
        }
    }
}
