// Wonder Gather — a drawn contour for beings (S1d concepts).
// An inverted hull: the back faces pushed out along the normals, about as wide on screen near or far,
// and wobbling along the form so the line reads as drawn by hand rather than traced.
// Used as an extra material on a being's renderers.
Shader "Wonder Gather/Outline"
{
    Properties
    {
        _Colour("Colour", Color) = (0.13, 0.08, 0.07, 1)
        _Width("Width (pixels at 1080p)", Range(0, 6)) = 1.6
        _Wobble("Wobble", Range(0, 1)) = 0.5
        _MaxWidth("Widest (metres)", Float) = 0.012
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Colour;
            float _Width;
            float _Wobble;
            float _MaxWidth;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float fog : TEXCOORD0; };

            float Hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            float Noise(float3 x)
            {
                float3 i = floor(x);
                float3 f = frac(x);
                f = f * f * (3 - 2 * f);
                return lerp(lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x), lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x), lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }

            Varyings vert(Attributes input)
            {
                Varyings o;
                float3 ws = TransformObjectToWorld(input.positionOS.xyz);
                float3 n = normalize(TransformObjectToWorldNormal(input.normalOS));
                // One pixel in metres at this distance, so the line keeps its width on screen.
                float pixel = length(GetCameraPositionWS() - ws) * 2 / (_ScreenParams.y * UNITY_MATRIX_P[1][1]);
                // The wobble follows the form (object space), so it does not swim as a being moves.
                float wobble = lerp(1, 0.3 + 1.4 * Noise(input.positionOS.xyz * 14), _Wobble);
                ws += n * min(_Width * (_ScreenParams.y / 1080.0) * pixel * wobble, _MaxWidth);
                o.positionCS = TransformWorldToHClip(ws);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(MixFog(_Colour.rgb, input.fog), 1);
            }
            ENDHLSL
        }
    }
}
