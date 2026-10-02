using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace WonderGather
{
    // Screen-space look candidates for the S1c comparison: a paint filter, loose ink contours
    // and paper grain; since S1e also the painting pass (strokes that follow the forms) and a
    // painter's palette grade. Passes are injected only by this component, so maps without it
    // render exactly as before and the shared URP renderer asset is untouched.
    [ExecuteAlways]
    public sealed class LookPostEffects : MonoBehaviour
    {
        [SerializeField] private Shader shader;
        [Range(0, 1)] public float Paint;
        [Range(1, 6)] public int PaintRadius = 4;
        [Range(0, 1)] public float Ink;
        public Color InkColor = new Color(.30f, .22f, .20f);
        [Range(.5f, 3)] public float InkWidth = 1.2f;
        [Range(0, 1)] public float Grain;
        [Tooltip("Paint with strokes that follow the forms (the S1e painting pass) instead of the classic filter.")]
        public bool Painting;
        [Range(2, 10)] public float PaintingRadius = 5;
        [Range(2, 16)] public float PaintingSharpness = 8;
        [Tooltip("How far the painter's palette for the hour (set by TimeOfDay) is applied.")]
        [Range(0, 1)] public float Grade;
        [Tooltip("Ink only what is drawn (characters), leaving the painted world without contours.")]
        public bool InkOnlyDrawn;

        private Material material;
        private BlitPass paint, grain;
        private InkPass ink;
        private PaintingPass painting;

        public void Configure(Shader post) => shader = post;

        private void OnEnable()
        {
            if (shader == null) return;
            material = CoreUtils.CreateEngineMaterial(shader);
            paint = new BlitPass("Wonder Gather paint", material, 0, RenderPassEvent.BeforeRenderingPostProcessing);
            ink = new InkPass(material);
            painting = new PaintingPass(material);
            grain = new BlitPass("Wonder Gather grain", material, 2, RenderPassEvent.AfterRenderingPostProcessing);
            RenderPipelineManager.beginCameraRendering += Inject;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= Inject;
            CoreUtils.Destroy(material);
            material = null;
        }

        private void Inject(ScriptableRenderContext context, Camera camera)
        {
            if (material == null || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            var data = camera.GetUniversalAdditionalCameraData();
            if (data == null || data.scriptableRenderer == null) return;
            Shader.SetGlobalVector("_WG_PostParams", new Vector4(Paint, Ink, Grain, PaintRadius));
            Shader.SetGlobalVector("_WG_InkColor", new Vector4(InkColor.r, InkColor.g, InkColor.b, InkWidth));
            Shader.SetGlobalVector("_WG_PaintShape", new Vector4(PaintingRadius, PaintingSharpness, Grade, InkOnlyDrawn ? .2f : .7f));
            if (Paint > 0) data.scriptableRenderer.EnqueuePass(Painting ? painting : paint);
            if (Ink > 0) data.scriptableRenderer.EnqueuePass(ink);
            if (Grain > 0 || Grade > 0) data.scriptableRenderer.EnqueuePass(grain);
        }

        private sealed class BlitData
        {
            public TextureHandle Source;
            public Material Material;
            public int Pass;
        }

        // Reads the camera colour through the material into a new texture that becomes the camera colour.
        private sealed class BlitPass : ScriptableRenderPass
        {
            private readonly Material material;
            private readonly int pass;
            private readonly string name;

            public BlitPass(string label, Material effect, int shaderPass, RenderPassEvent when)
            {
                name = label;
                material = effect;
                pass = shaderPass;
                renderPassEvent = when;
                requiresIntermediateTexture = true;
            }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
            {
                var resources = frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer) return;
                var source = resources.activeColorTexture;
                var description = graph.GetTextureDesc(source);
                description.name = name;
                description.clearBuffer = false;
                var destination = graph.CreateTexture(description);
                using (var builder = graph.AddRasterRenderPass<BlitData>(name, out var data))
                {
                    data.Source = source;
                    data.Material = material;
                    data.Pass = pass;
                    builder.UseTexture(source);
                    builder.SetRenderAttachment(destination, 0);
                    builder.SetRenderFunc((BlitData d, RasterGraphContext context) =>
                        Blitter.BlitTexture(context.cmd, d.Source, new Vector4(1, 1, 0, 0), d.Material, d.Pass));
                }
                resources.cameraColor = destination;
            }
        }

        // The painting: the image's structure is measured and softened, then each pixel is painted
        // with strokes laid along it (shader passes 3 to 6).
        private sealed class PaintingPass : ScriptableRenderPass
        {
            private static readonly int Structure = Shader.PropertyToID("_WG_Structure");
            private readonly Material material;

            private sealed class PaintData
            {
                public TextureHandle Source, Structure;
                public Material Material;
            }

            public PaintingPass(Material effect)
            {
                material = effect;
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                requiresIntermediateTexture = true;
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }

            private void Step(RenderGraph graph, string name, TextureHandle from, TextureHandle to, int pass)
            {
                using var builder = graph.AddRasterRenderPass<BlitData>(name, out var data);
                data.Source = from;
                data.Material = material;
                data.Pass = pass;
                builder.UseTexture(from);
                builder.SetRenderAttachment(to, 0);
                builder.SetRenderFunc((BlitData d, RasterGraphContext context) =>
                    Blitter.BlitTexture(context.cmd, d.Source, new Vector4(1, 1, 0, 0), d.Material, d.Pass));
            }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
            {
                var resources = frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer) return;
                var source = resources.activeColorTexture;
                var description = graph.GetTextureDesc(source);
                description.clearBuffer = false;
                description.name = "Wonder Gather structure";
                var structure = graph.CreateTexture(description);
                var across = graph.CreateTexture(description);
                var softened = graph.CreateTexture(description);
                Step(graph, "Wonder Gather structure", source, structure, 3);
                Step(graph, "Wonder Gather structure across", structure, across, 4);
                Step(graph, "Wonder Gather structure down", across, softened, 5);

                description.name = "Wonder Gather painting";
                var destination = graph.CreateTexture(description);
                using (var builder = graph.AddRasterRenderPass<PaintData>("Wonder Gather painting", out var data))
                {
                    data.Source = source;
                    data.Structure = softened;
                    data.Material = material;
                    builder.UseTexture(source);
                    builder.UseTexture(softened);
                    if (resources.cameraDepthTexture.IsValid()) builder.UseTexture(resources.cameraDepthTexture);
                    builder.UseAllGlobalTextures(true);
                    builder.SetRenderAttachment(destination, 0);
                    builder.SetRenderFunc((PaintData d, RasterGraphContext context) =>
                    {
                        RTHandle structureHandle = d.Structure;
                        d.Material.SetTexture(Structure, structureHandle);
                        Blitter.BlitTexture(context.cmd, d.Source, new Vector4(1, 1, 0, 0), d.Material, 6);
                    });
                }
                resources.cameraColor = destination;
            }
        }

        private sealed class InkData
        {
            public Material Material;
        }

        // Multiplies contour lines over the camera colour, reading scene depth and normals.
        private sealed class InkPass : ScriptableRenderPass
        {
            private readonly Material material;

            public InkPass(Material effect)
            {
                material = effect;
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
                requiresIntermediateTexture = true;
                ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
            }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
            {
                var resources = frameData.Get<UniversalResourceData>();
                if (resources.isActiveTargetBackBuffer) return;
                using (var builder = graph.AddRasterRenderPass<InkData>("Wonder Gather ink", out var data))
                {
                    data.Material = material;
                    if (resources.cameraDepthTexture.IsValid()) builder.UseTexture(resources.cameraDepthTexture);
                    if (resources.cameraNormalsTexture.IsValid()) builder.UseTexture(resources.cameraNormalsTexture);
                    builder.UseAllGlobalTextures(true);
                    builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                    builder.SetRenderFunc((InkData d, RasterGraphContext context) =>
                        context.cmd.DrawProcedural(Matrix4x4.identity, d.Material, 1, MeshTopology.Triangles, 3));
                }
            }
        }
    }
}
