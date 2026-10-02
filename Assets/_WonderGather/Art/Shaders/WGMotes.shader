// Wonder Gather motes (S1e): the air is alive. Seeds and pollen drift with the wind and catch the
// sun only when the eye looks towards it; at dusk and night fireflies blink low over the meadow,
// warm near the house's lights. Drawn by WonderMotes as camera-facing quads, all on the GPU.
Shader "Wonder Gather/Motes"
{
    Properties
    {
        _Size("Size (m)", Float) = 0.035
        _Seed("Seed colour", Color) = (1.0, 0.95, 0.82, 1)
        _Firefly("Firefly colour", Color) = (0.85, 1.0, 0.45, 1)
        _Brightness("Brightness", Float) = 2.2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+10" "RenderPipeline" = "UniversalPipeline" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Motes"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "WGCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Size, _Brightness;
                float4 _Seed, _Firefly;
            CBUFFER_END

            float4 _WG_MoteVolume;   // xyz: centre of the drifting volume, w: half-size (m)
            float4 _WG_MoteShape;    // x: day seeds amount, y: fireflies amount, z: firefly floor height, w: count

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 corner : TEXCOORD0;
                float3 glow : TEXCOORD1;
            };

            float Hash1(float n) { return frac(sin(n * 12.9898) * 43758.5453); }

            Varyings Vert(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                Varyings o;
                uint id = vertexID / 6;
                uint v = vertexID % 6;
                float2 corners[6] = { float2(-1, -1), float2(1, -1), float2(1, 1), float2(-1, -1), float2(1, 1), float2(-1, 1) };
                float2 corner = corners[v];
                float fid = (float)id;
                float firefly = step(0.55, Hash1(fid * 1.7 + 3.1));

                // Drift with the wind and wander a little, wrapped in a box that follows the camera,
                // so the air is equally full wherever the eye goes.
                float extent = _WG_MoteVolume.w;
                float3 seed = float3(Hash1(fid), Hash1(fid + 17.3), Hash1(fid + 41.7));
                float t = _Time.y;
                float2 wind = normalize(_WG_Wind.xy + float2(1e-4, 0)) * (0.6 + _WG_Wind.z * 1.5);
                float3 drift = float3(wind.x, 0, wind.y) * t * lerp(0.4, 1.1, seed.y) * (1 - firefly * 0.85);
                drift += float3(sin(t * 0.7 + fid), sin(t * 0.43 + fid * 2.1) * 0.6, cos(t * 0.61 + fid * 1.3)) * lerp(0.4, 1.2, seed.z);
                float3 p = seed * (2 * extent) + drift;
                p = _WG_MoteVolume.xyz + (frac((p - _WG_MoteVolume.xyz) / (2 * extent) + 0.5) - 0.5) * (2 * extent);
                // Fireflies keep low over the meadow; seeds float at any height near the eye.
                p.y = lerp(p.y, _WG_MoteShape.z + 0.3 + seed.y * 1.6 + sin(t * 0.8 + fid) * 0.25, firefly);

                float3 view = p - _WorldSpaceCameraPos;
                float dist = length(view);
                float3 toEye = -view / max(dist, 1e-3);

                // Seeds: lit when backlit by the sun (light scattering through them), faint otherwise.
                float3 sun = normalize(_WG_SunDirection.xyz);
                float backlit = pow(saturate(dot(-toEye, sun)), 6) * 3 + 0.12;
                float3 seedGlow = _Seed.rgb * _WG_SunColor.rgb * backlit * _WG_MoteShape.x * (1 - firefly);
                // Fireflies: a slow blink, warm-green.
                float blink = smoothstep(0.55, 1, sin(t * lerp(0.6, 1.4, seed.x) + fid * 7.1) * 0.5 + 0.5);
                float3 fireflyGlow = _Firefly.rgb * blink * _WG_MoteShape.y * firefly * 2.5;
                float fade = smoothstep(extent, extent * 0.6, dist) * smoothstep(0.4, 1.5, dist);
                o.glow = (seedGlow + fireflyGlow) * fade * _Brightness;

                float size = _Size * lerp(0.6, 1.4, seed.z) * (1 + firefly * 0.6);
                float3 right = UNITY_MATRIX_V[0].xyz, up = UNITY_MATRIX_V[1].xyz;
                float3 world = p + (right * corner.x + up * corner.y) * size;
                o.positionCS = TransformWorldToHClip(world);
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
