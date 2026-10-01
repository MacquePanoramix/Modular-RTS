// Wonder Gather chimney smoke (S1b): soft painted puffs that rise, swell, drift downwind
// and fade. Drawn procedurally by SmokePlume with no mesh: six vertices per puff.
Shader "Wonder Gather/Smoke"
{
    Properties
    {
        _Lit("Lit colour", Color) = (0.86, 0.84, 0.86, 1)
        _Shade("Shade colour", Color) = (0.45, 0.47, 0.58, 1)
        _Opacity("Opacity", Range(0, 1)) = 0.55
        _Rise("Rise height", Float) = 5
        _Drift("Drift", Float) = 3
        _Rate("Puffs per second", Float) = 0.12
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Smoke"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "WGCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Lit, _Shade;
                float _Opacity, _Rise, _Drift, _Rate;
            CBUFFER_END
            float4 _SmokeOrigin;   // xyz chimney top, w puff count

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 puff : TEXCOORD2;   // age 0..1, seed
            };

            Varyings Vert(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                static const float2 corners[6] = { float2(-1, -1), float2(1, -1), float2(1, 1), float2(-1, -1), float2(1, 1), float2(-1, 1) };
                float count = max(_SmokeOrigin.w, 1);
                float seed = WG_Hash(float3(instanceID, 3.7, 1.3));
                float age = frac(_Time.y * _Rate + instanceID / count);
                float2 wind = WG_WindAt(_SmokeOrigin.xyz, _Time.y) + normalize(_WG_Wind.xy + 1e-4) * 0.5;
                float3 centre = _SmokeOrigin.xyz;
                centre.y += age * _Rise + age * age * _Rise * 0.4;
                centre.xz += wind * age * age * _Drift + float2(sin(age * 6 + seed * 9), cos(age * 5 + seed * 7)) * 0.25 * age;
                float size = lerp(0.25, 1.6, sqrt(age)) * lerp(0.8, 1.2, seed);
                float2 corner = corners[vertexID % 6];
                float3 right = UNITY_MATRIX_V[0].xyz;
                float3 up = UNITY_MATRIX_V[1].xyz;
                float3 positionWS = centre + (right * corner.x + up * corner.y) * size;
                Varyings o;
                o.positionWS = positionWS;
                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = corner;
                o.puff = float2(age, seed);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float age = input.puff.x;
                float2 uv = input.uv;
                // A soft blob with a brushed, irregular edge.
                float edge = WG_Fbm(float3(uv * 1.7 + input.puff.y * 13, age * 2)) * 0.55;
                float shape = smoothstep(1.0, 0.35, length(uv) + edge - 0.25);
                float alpha = shape * sin(age * PI) * _Opacity;
                Light light = GetMainLight();
                float lit = saturate(0.5 + uv.y * 0.35 + dot(float3(0, 1, 0), light.direction) * 0.25);
                float3 ambient = WG_LookActive() ? _WG_Sky.rgb : SampleSH(float3(0, 1, 0));
                float3 color = lerp(_Shade.rgb, _Lit.rgb, lit) * (ambient * 1.6 + light.color * lit * 0.35);
                color = WG_ApplyFog(color, input.positionWS, 0);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
