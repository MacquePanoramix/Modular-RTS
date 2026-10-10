// Wonder Gather fireflies: at dusk and night they roam and blink low over the meadow, warm
// green-gold. Drawn by Fireflies as camera-facing quads. Each has its own home in the world
// (Fireflies finds them once) and roams round it on the GPU; the camera decides nothing about
// where they are, only what is seen of them: from further away fewer, and fainter.
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

            StructuredBuffer<float4> _WG_FireflyHomes;   // xyz: its home, on the ground; w: the number it is told apart by (0..1)
            float4 _WG_FireflySight;    // x: all are seen from within (m), y: the fewest and faintest from (m), z: the share seen from there, w: how bright those are
            float4 _WG_FireflyOut;      // x: how many are out (0..1, by the hour), y..z: between these distances (m) the last fade from sight

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

                // It roams round its own home: the small circling it always had, and a slow wander wider.
                float4 home = _WG_FireflyHomes[id];
                float t = _Time.y;
                float3 p = home.xyz;
                p += float3(sin(t * 0.7 + fid), 0, cos(t * 0.61 + fid * 1.3)) * lerp(0.4, 1.2, seed.z);
                p += float3(sin(t * 0.13 + fid * 2.1), 0, cos(t * 0.11 + fid * 0.7)) * lerp(0.8, 2.4, seed.x);
                // Low over its own ground, rising and settling.
                p.y = home.y + 0.4 + seed.y * 1.4 + sin(t * 0.8 + fid) * 0.25;

                float3 view = p - _WorldSpaceCameraPos;
                float dist = length(view);
                // A slow blink; only some are lit at any moment.
                float blink = smoothstep(0.62, 1, sin(t * lerp(0.6, 1.4, seed.x) + fid * 7.1) * 0.5 + 0.5);
                // Close to the eye they fade and shrink, so none swells into a blot across the view.
                float fade = smoothstep(1.0, 3.0, dist);
                float out_ = step(seed.x * 0.999, _WG_FireflyOut.x);
                // From further away fewer of them are seen (each has its own place in the order they
                // drop out of sight), and those are fainter; beyond the last distance, none.
                float away = smoothstep(_WG_FireflySight.x, _WG_FireflySight.y, dist);
                float last = 1 - smoothstep(_WG_FireflyOut.y, _WG_FireflyOut.z, dist);
                float seen = saturate((lerp(1, _WG_FireflySight.z, away) * last - home.w) * 16 + 0.5);
                o.glow = _Colour.rgb * blink * fade * out_ * seen * lerp(1, _WG_FireflySight.w, away) * last * _Brightness;

                float size = _Size * lerp(0.8, 1.5, seed.z) * clamp(dist / 4, 0.5, 1.6);
                // Never drawn smaller than a dot about two pixels across (a smaller one would flicker
                // in and out between pixels); a dot widened so is dimmed by as much, so that it does
                // not gain light by it.
                float pixel = dist * 2.0 / (_ScreenParams.y * UNITY_MATRIX_P[1][1]);
                o.glow *= saturate(size / max(pixel * 1.6, 1e-5));
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
