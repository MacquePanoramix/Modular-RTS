using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // Chimney smoke: a few soft puffs drawn procedurally above this transform.
    [ExecuteAlways]
    public sealed class SmokePlume : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private int puffs = 14;
        private MaterialPropertyBlock properties;

        public void Configure(Material smoke) => material = smoke;

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += Draw;
        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= Draw;

        private void Draw(ScriptableRenderContext context, Camera camera)
        {
            if (material == null || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            properties ??= new MaterialPropertyBlock();
            var origin = transform.position;
            properties.SetVector("_SmokeOrigin", new Vector4(origin.x, origin.y, origin.z, puffs));
            var parameters = new RenderParams(material)
            {
                worldBounds = new Bounds(origin + Vector3.up * 5, new Vector3(16, 14, 16)),
                matProps = properties,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                camera = camera,
                layer = gameObject.layer
            };
            Graphics.RenderPrimitives(parameters, MeshTopology.Triangles, 6, puffs);
        }
    }
}
