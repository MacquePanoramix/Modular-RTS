using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // Fireflies over the meadow at dusk and night (see WGFireflies.shader). They come out as the
    // sun sets and are gone by day; all motion is on the GPU. They live in a patch of meadow
    // around the point the camera looks at, which widens as the camera pulls back, so from far
    // above they thin into a sparse scatter of tiny lights instead of vanishing.
    [ExecuteAlways]
    public sealed class Fireflies : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private TimeOfDay time;
        [SerializeField] private int count = 600;
        [Tooltip("Half-size (m) of the patch they live in, close up and from far above.")]
        [SerializeField] private float extent = 22, farExtent = 75;
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
            // The patch sits on the meadow where the camera looks, and widens with the distance to it.
            var eye = camera.transform.position;
            var forward = camera.transform.forward;
            float along = forward.y < -.05f ? Mathf.Min((eye.y - floor - 1) / -forward.y, 200) : extent;
            var centre = eye + forward * Mathf.Max(along, 0);
            float reach = Mathf.Clamp(Vector3.Distance(eye, centre) * .9f, extent, farExtent);
            properties.SetVector("_WG_FireflyVolume", new Vector4(centre.x, centre.y, centre.z, reach));
            properties.SetVector("_WG_FireflyShape", new Vector4(presence, floor, 0, 0));
            var parameters = new RenderParams(material)
            {
                worldBounds = new Bounds(new Vector3(centre.x, floor + 1, centre.z), new Vector3(reach * 2.5f, 6, reach * 2.5f)),
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
