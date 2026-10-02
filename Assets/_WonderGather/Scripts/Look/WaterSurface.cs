using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace WonderGather
{
    // The still water beyond the Ordinary Place: one level plane out to the horizon, and a planar
    // reflection of the world above it, rendered before each game or scene view camera.
    // The reflection camera sits mirrored below the water and upside down, which keeps its
    // rotation a true rotation (no inverted culling); the water shader flips the image back.
    // Like the land, the plane is rebuilt when the scene loads rather than stored.
    [ExecuteAlways]
    public sealed class WaterSurface : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private float level = -36, extent = 9000;
        [Range(.25f, 1)] [SerializeField] private float reflectionScale = .5f;
        [Tooltip("Lifts the reflection's clip plane slightly so the shoreline does not leak.")]
        [SerializeField] private float clipOffset = .05f;

        private GameObject generated;
        private Mesh plane;
        private Camera mirror;
        private RenderTexture target;

        public float Level => level;
        public RenderTexture Reflection => target;
        public int ReflectionsRendered { get; private set; }

        public void Configure(Material water, float waterLevel)
        {
            material = water;
            level = waterLevel;
            if (isActiveAndEnabled) { Clear(); Build(); }
        }

        private void OnEnable()
        {
            Build();
            RenderPipelineManager.beginCameraRendering += Reflect;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= Reflect;
            Shader.SetGlobalFloat("_WG_ReflectionReady", 0);
            Clear();
        }

        private void Build()
        {
            if (material == null || generated != null) return;
            generated = new GameObject("Water (generated)") { hideFlags = HideFlags.DontSave };
            generated.transform.SetParent(transform, false);
            plane = new Mesh { name = "Water plane", hideFlags = HideFlags.DontSave };
            plane.vertices = new[]
            {
                new Vector3(-extent, level, -extent), new Vector3(extent, level, -extent),
                new Vector3(-extent, level, extent), new Vector3(extent, level, extent),
            };
            plane.triangles = new[] { 0, 2, 1, 1, 2, 3 };
            plane.normals = new[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            plane.RecalculateBounds();
            generated.AddComponent<MeshFilter>().sharedMesh = plane;
            var renderer = generated.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void Clear()
        {
            Release(generated);
            Release(plane);
            if (mirror != null) Release(mirror.gameObject);
            if (target != null) target.Release();
            Release(target);
            generated = null;
            plane = null;
            mirror = null;
            target = null;
        }

        private static void Release(Object item)
        {
            if (item == null) return;
            if (Application.isPlaying) Destroy(item);
            else DestroyImmediate(item);
        }

        private void Reflect(ScriptableRenderContext context, Camera camera)
        {
            if (material == null || camera == mirror) return;
            if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView) return;
            // Only the water's own side of the plane is mirrored; from below there is nothing to see.
            if (camera.transform.position.y < level) { Shader.SetGlobalFloat("_WG_ReflectionReady", 0); return; }
            Prepare(camera);

            var eye = camera.transform;
            static Vector3 Flip(Vector3 v) => new Vector3(v.x, -v.y, v.z);
            var position = eye.position;
            mirror.transform.SetPositionAndRotation(new Vector3(position.x, 2 * level - position.y, position.z),
                Quaternion.LookRotation(Flip(eye.forward), -Flip(eye.up)));
            mirror.fieldOfView = camera.fieldOfView;
            mirror.aspect = camera.aspect;
            mirror.nearClipPlane = camera.nearClipPlane;
            mirror.farClipPlane = camera.farClipPlane;
            mirror.ResetProjectionMatrix();
            // Keep only what lies above the water: an oblique near plane on the water's surface.
            var view = mirror.worldToCameraMatrix;
            var point = view.MultiplyPoint(new Vector3(0, level + clipOffset, 0));
            var normal = view.MultiplyVector(Vector3.up).normalized;
            mirror.projectionMatrix = mirror.CalculateObliqueMatrix(new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(point, normal)));

            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
            if (!RenderPipeline.SupportsRenderRequest(mirror, request)) return;
            RenderPipeline.SubmitRenderRequest(mirror, request);
            ReflectionsRendered++;
            Shader.SetGlobalTexture("_WG_Reflection", target);
            Shader.SetGlobalFloat("_WG_ReflectionReady", 1);
        }

        private void Prepare(Camera camera)
        {
            int width = Mathf.Max(64, Mathf.RoundToInt(camera.pixelWidth * reflectionScale));
            int height = Mathf.Max(64, Mathf.RoundToInt(camera.pixelHeight * reflectionScale));
            if (target == null || target.width != width || target.height != height)
            {
                if (target != null) { target.Release(); Release(target); }
                target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGBHalf)
                    { name = "Water reflection", hideFlags = HideFlags.DontSave, useMipMap = false };
                target.Create();
            }
            if (mirror != null) return;
            var holder = new GameObject("Water reflection camera") { hideFlags = HideFlags.HideAndDontSave };
            mirror = holder.AddComponent<Camera>();
            mirror.enabled = false;
            mirror.cameraType = CameraType.Reflection;
            mirror.clearFlags = CameraClearFlags.Skybox;
            var data = holder.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = false;
            data.renderShadows = false;
            data.requiresDepthOption = CameraOverrideOption.Off;
            data.requiresColorOption = CameraOverrideOption.Off;
            data.antialiasing = AntialiasingMode.None;
        }
    }
}
