using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // The air is alive (S1e): seeds and pollen drifting on the wind that light up when the eye
    // looks towards the sun, and fireflies blinking low over the meadow at dusk and night. All
    // motion is computed on the GPU in a box that follows each camera (see WGMotes.shader).
    [ExecuteAlways]
    public sealed class WonderMotes : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private TimeOfDay time;
        [SerializeField] private int count = 2400;
        [Tooltip("Half-size (m) of the drifting volume around the camera.")]
        [SerializeField] private float extent = 22;
        [Tooltip("Height the fireflies keep above (the meadow's ground level).")]
        [SerializeField] private float fireflyFloor;
        private MaterialPropertyBlock properties;

        public void Configure(Material motes, TimeOfDay day)
        {
            material = motes;
            time = day;
        }

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += Draw;
        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= Draw;

        private void Draw(ScriptableRenderContext context, Camera camera)
        {
            if (material == null || count <= 0 || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            float sunHeight = time != null ? time.SunDirection(time.Hour).y : .5f;
            float seeds = Mathf.InverseLerp(-.05f, .15f, sunHeight);
            float fireflies = 1 - Mathf.InverseLerp(-.12f, .1f, sunHeight);
            properties ??= new MaterialPropertyBlock();
            var centre = camera.transform.position + camera.transform.forward * extent * .5f;
            properties.SetVector("_WG_MoteVolume", new Vector4(centre.x, centre.y, centre.z, extent));
            properties.SetVector("_WG_MoteShape", new Vector4(seeds, fireflies, fireflyFloor, count));
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
