// Wonder Gather window glow: a soft, warm halo of painted light around each lit window and the
// door, as a painter lets lamplight bloom into the dusk. It follows the house's lights through
// _WG_GlowScale (the hour and, when on, the hearth's flicker). Drawn by WindowGlow.
Shader "Wonder Gather/Window Glow"
{
    Properties
    {
        _Colour("Colour", Color) = (1.0, 0.58, 0.28, 1)
        _Intensity("Intensity", Float) = 0.55
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "WindowGlow"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Colour;
                float _Intensity;
            CBUFFER_END

            float4 _WG_Halos[8];      // xyz: centre, w: radius (m)
            float _WG_HaloCount;
            float _WG_GlowScale;      // how lit the house is, set by TimeOfDay

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 corner : TEXCOORD0;
                float eye : TEXCOORD1;
                float strength : TEXCOORD2;
            };

            Varyings Vert(uint vertexID : SV_VertexID)
            {
                Varyings o;
                uint id = vertexID / 6;
                uint v = vertexID % 6;
                float2 corners[6] = { float2(-1, -1), float2(1, -1), float2(1, 1), float2(-1, -1), float2(1, 1), float2(-1, 1) };
                float2 corner = corners[v];
                float4 halo = _WG_Halos[min(id, 7)];
                float3 right = UNITY_MATRIX_V[0].xyz, up = UNITY_MATRIX_V[1].xyz;
                float3 world = halo.xyz + (right * corner.x + up * corner.y) * halo.w;
                o.positionCS = TransformWorldToHClip(world);
                o.corner = corner;
                o.eye = -TransformWorldToView(world).z;
                // Fades as the camera comes right up to the window, so it never fills the view.
                float dist = length(halo.xyz - _WorldSpaceCameraPos);
                o.strength = (id < (uint)_WG_HaloCount ? 1 : 0) * _WG_GlowScale * smoothstep(0.8, 3.0, dist);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float r2 = dot(i.corner, i.corner);
                float glow = saturate((exp(-r2 * 4) - exp(-4)) / (1 - exp(-4)));
                // Soften where the halo meets the wall or the grass, instead of a hard line.
                float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
                float scene = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float soft = saturate((scene - i.eye) / 0.6);
                return half4(_Colour.rgb * glow * soft * i.strength * _Intensity, 0);
            }
            ENDHLSL
        }
    }
}
