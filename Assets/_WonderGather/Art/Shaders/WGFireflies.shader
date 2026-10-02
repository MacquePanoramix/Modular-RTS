// Wonder Gather fireflies: at dusk and night they drift and blink low over the meadow, warm
// green-gold. Drawn by Fireflies as camera-facing quads; all motion is computed on the GPU in a
// patch of meadow around where the camera looks, so it is alive wherever the eye goes. From far
// above each keeps at least a tiny painted dot, so they never simply vanish.
Shader "Wonder Gather/Fireflies"
{
    Properties
    {
        _Size("Size (m)", Float) = 0.035
        _Colour("Colour", Color) = (0.85, 1.0, 0.45, 1)
        _Brightness("Brightness", Float) = 6.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+10" "RenderPipeline" = "UniversalPipeline" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Fireflies"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "WGCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Size, _Brightness;
                float4 _Colour;
            CBUFFER_END

            float4 _WG_FireflyVolume;   // xyz: centre of the patch where the camera looks, w: half-size (m)
            float4 _WG_FireflyShape;    // x: how many are out (0..1, by the hour), y: height of the meadow floor

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 corner : TEXCOORD0;
                float3 glow : TEXCOORD1;
            };

            float Hash1(float n) { return frac(sin(n * 12.9898) * 43758.5453); }

            Varyings Vert(uint vertexID : SV_VertexID)
            {
                Varyings o;
                uint id = vertexID / 6;
                uint v = vertexID % 6;
                float2 corners[6] = { float2(-1, -1), float2(1, -1), float2(1, 1), float2(-1, -1), float2(1, 1), float2(-1, 1) };
                float2 corner = corners[v];
                float fid = (float)id;
                float3 seed = float3(Hash1(fid), Hash1(fid + 17.3), Hash1(fid + 41.7));

                // Wander slowly, drifting a little with the wind, wrapped in the box around the camera.
                float extent = _WG_FireflyVolume.w;
                float t = _Time.y;
                float2 wind = normalize(_WG_Wind.xy + float2(1e-4, 0)) * 0.15;
                float3 drift = float3(wind.x, 0, wind.y) * t;
                drift += float3(sin(t * 0.7 + fid), 0, cos(t * 0.61 + fid * 1.3)) * lerp(0.4, 1.2, seed.z);
                float3 p = seed * (2 * extent) + drift;
                p = _WG_FireflyVolume.xyz + (frac((p - _WG_FireflyVolume.xyz) / (2 * extent) + 0.5) - 0.5) * (2 * extent);
                // Low over the meadow, rising and settling.
                p.y = _WG_FireflyShape.y + 0.4 + seed.y * 1.4 + sin(t * 0.8 + fid) * 0.25;

                float3 view = p - _WorldSpaceCameraPos;
                float dist = length(view);
                // A slow blink; only some are lit at any moment.
                float blink = smoothstep(0.62, 1, sin(t * lerp(0.6, 1.4, seed.x) + fid * 7.1) * 0.5 + 0.5);
                // They thin out towards the patch's edge (so wrapping never pops), and close to the eye
                // they fade and shrink, so none swells into a blot across the view.
                float edge = length(p.xz - _WG_FireflyVolume.xz) / extent;
                float fade = smoothstep(1.0, 0.7, edge) * smoothstep(1.0, 3.0, dist);
                float out_ = step(seed.x * 0.999, _WG_FireflyShape.x);
                o.glow = _Colour.rgb * blink * fade * out_ * _Brightness;

                float size = _Size * lerp(0.8, 1.5, seed.z) * clamp(dist / 4, 0.5, 1.6);
                // Never smaller than a dot about two pixels across, however far the camera; such far dots
                // glow a little brighter, so the meadow still reads as alive from high above.
                float pixel = dist * 2.0 / (_ScreenParams.y * UNITY_MATRIX_P[1][1]);
                o.glow *= 1 + saturate(pixel * 1.6 / max(size, 1e-4) - 1) * 0.8;
                size = max(size, pixel * 1.6);
                float3 right = UNITY_MATRIX_V[0].xyz, up = UNITY_MATRIX_V[1].xyz;
                o.positionCS = TransformWorldToHClip(p + (right * corner.x + up * corner.y) * size);
                o.corner = corner;
                if (dot(o.glow, 1) < 1e-4) o.positionCS = float4(0, 0, -1, 1);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float r = length(i.corner);
                float disc = saturate(1 - r);
                float glow = disc * disc + smoothstep(0.35, 0, r) * 1.5;
                return half4(i.glow * glow, 0);
            }
            ENDHLSL
        }
    }
}
