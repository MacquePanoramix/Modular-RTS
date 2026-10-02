using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // A soft warm halo just outside each lit window and the door (see WGWindowGlow.shader). The
    // halos sit at the house's light anchors, pushed out along the way each light shines.
    [ExecuteAlways]
    public sealed class WindowGlow : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private Light[] lights = new Light[0];
        [SerializeField] private float windowRadius = 1.1f, doorRadius = 1.5f, outward = .35f;
        private readonly Vector4[] halos = new Vector4[8];
        private MaterialPropertyBlock properties;

        public void Configure(Material glow, IEnumerable<Light> windowsAndDoor)
        {
            material = glow;
            lights = new List<Light>(windowsAndDoor).ToArray();
        }

        public int Count => Mathf.Min(lights.Length, halos.Length);

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += Draw;
        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= Draw;

        private void Draw(ScriptableRenderContext context, Camera camera)
        {
            if (material == null || lights.Length == 0 || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            int count = 0;
            var bounds = new Bounds();
            foreach (var light in lights)
            {
                if (light == null || !light.enabled || count == halos.Length) continue;
                var facing = light.transform.forward;
                facing.y = 0;
                var centre = light.transform.position + facing.normalized * outward;
                float radius = light.name.StartsWith("Door") ? doorRadius : windowRadius;
                halos[count++] = new Vector4(centre.x, centre.y, centre.z, radius);
                if (count == 1) bounds = new Bounds(centre, Vector3.one * radius * 2);
                else bounds.Encapsulate(new Bounds(centre, Vector3.one * radius * 2));
            }
            if (count == 0) return;
            properties ??= new MaterialPropertyBlock();
            properties.SetVectorArray("_WG_Halos", halos);
            properties.SetFloat("_WG_HaloCount", count);
            var parameters = new RenderParams(material)
            {
                worldBounds = bounds,
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
