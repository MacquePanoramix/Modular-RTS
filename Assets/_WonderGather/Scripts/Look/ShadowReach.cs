using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace WonderGather
{
    // Shadows reach as far as the lamps that are seen. The pipeline draws no shadow further than
    // its shadow distance from the eye, and fades them out over the last of it: from further than
    // that a lamp inside a house lights the ground through the walls, so the house's light "gets
    // suddenly much stronger" as the camera pulls back (Luis, October 2 and 8; measured: between
    // 46 and 51 m from the door, with the distance at 50). While a lamp that casts shadows is lit,
    // the distance is kept beyond all that it lights. The nearer cascades keep the metres they
    // had, so nothing changes close by; and by day, with no lamp lit, nothing changes at all.
    // The pipeline's own settings are put back after each camera has drawn.
    public sealed class ShadowReach : MonoBehaviour
    {
        // No further than this, however far the camera is (m); a little beyond what a lamp lights (m);
        // how often the lamps of the place are looked for again (s).
        private const float AtMost = 260, Beyond = 2, LooksEvery = 1;

        private Light[] lamps = new Light[0];
        private float looked = float.MinValue;
        private UniversalRenderPipelineAsset changed;
        private float distanceWas, twoWas;
        private Vector2 threeWas;
        private Vector3 fourWas;

        // The pipeline's own shadow distance (as it is between frames), and the one last drawn with.
        public static float PipelineDistance => UniversalRenderPipeline.asset != null ? UniversalRenderPipeline.asset.shadowDistance : 0;
        public float LastReach { get; private set; }

        // How far the shadows have to reach for this eye: 0 when no lamp that casts shadows is lit.
        public float Needed(Vector3 eye, float border)
        {
            if (Time.unscaledTime - looked > LooksEvery)
            {
                looked = Time.unscaledTime;
                lamps = FindObjectsByType<Light>(FindObjectsSortMode.None);
            }
            float reach = 0;
            foreach (var lamp in lamps)
            {
                if (lamp == null || !lamp.isActiveAndEnabled || lamp.type == LightType.Directional || lamp.shadows == LightShadows.None || lamp.intensity <= 0) continue;
                reach = Mathf.Max(reach, Vector3.Distance(eye, lamp.transform.position) + lamp.range);
            }
            // The shadows fade over the last part of the distance (the pipeline's border): all a lamp lights is kept short of that.
            return reach <= 0 ? 0 : Mathf.Min(reach / Mathf.Max(.5f, 1 - border) + Beyond, AtMost);
        }

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += Before;
            RenderPipelineManager.endCameraRendering += After;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= Before;
            RenderPipelineManager.endCameraRendering -= After;
            PutBack();
        }

        private void Before(ScriptableRenderContext context, Camera camera)
        {
            PutBack();
            var pipeline = UniversalRenderPipeline.asset;
            if (pipeline == null || camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            float needed = Needed(camera.transform.position, pipeline.cascadeBorder);
            LastReach = Mathf.Max(needed, pipeline.shadowDistance);
            if (needed <= pipeline.shadowDistance) return;
            changed = pipeline;
            distanceWas = pipeline.shadowDistance;
            twoWas = pipeline.cascade2Split;
            threeWas = pipeline.cascade3Split;
            fourWas = pipeline.cascade4Split;
            float keep = distanceWas / needed;
            pipeline.shadowDistance = needed;
            pipeline.cascade2Split = twoWas * keep;
            pipeline.cascade3Split = threeWas * keep;
            pipeline.cascade4Split = fourWas * keep;
        }

        private void After(ScriptableRenderContext context, Camera camera) => PutBack();

        private void PutBack()
        {
            if (changed == null) return;
            changed.shadowDistance = distanceWas;
            changed.cascade2Split = twoWas;
            changed.cascade3Split = threeWas;
            changed.cascade4Split = fourWas;
            changed = null;
        }
    }
}
