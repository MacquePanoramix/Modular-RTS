// Wonder Gather breath in cold air (S3b): a few soft painted puffs at a mouth. Each puff is put where
// it is by BreathInAir (they are on no clock of the shader's: a puff is breathed out, drifts, swells and
// thins). Drawn with no mesh: six vertices a puff, as the chimney's smoke is.
Shader "Wonder Gather/Breath"
{
    Properties
    {
        _Lit("Lit colour", Color) = (0.93, 0.94, 0.97, 1)
        _Shade("Shade colour", Color) = (0.62, 0.66, 0.78, 1)
        _Opacity("Opacity", Range(0, 1)) = 0.55
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Breath"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "WGCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Lit, _Shade;
                float _Opacity;
            CBUFFER_END
            float4 _WG_BreathAt[8];   // xyz: where a puff is; w: how large it is (metres)
            float4 _WG_BreathIs[8];   // x: how old (0 to 1); y: a number of its own; z: how much of it shows

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 puff : TEXCOORD2;   // age 0..1, seed, shows
            };

            Varyings Vert(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                static const float2 corners[6] = { float2(-1, -1), float2(1, -1), float2(1, 1), float2(-1, -1), float2(1, 1), float2(-1, 1) };
                float4 at = _WG_BreathAt[instanceID % 8];
                float4 state = _WG_BreathIs[instanceID % 8];
                float2 corner = corners[vertexID % 6];
                float3 right = UNITY_MATRIX_V[0].xyz;
                float3 up = UNITY_MATRIX_V[1].xyz;
                float3 positionWS = at.xyz + (right * corner.x + up * corner.y) * at.w;
                Varyings o;
                o.positionWS = positionWS;
                o.positionCS = TransformWorldToHClip(positionWS);
                o.uv = corner;
                o.puff = state.xyz;
                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float age = input.puff.x;
                float2 uv = input.uv;
                // A soft blob with a brushed, irregular edge (as the smoke's).
                float edge = WG_Fbm(float3(uv * 1.7 + input.puff.y * 13, age * 2)) * 0.55;
                float shape = smoothstep(1.0, 0.3, length(uv) + edge - 0.25);
                // It is there at once, and thins away.
                float thins = smoothstep(0.0, 0.1, age) * pow(saturate(1 - age), 1.4);
                float alpha = shape * thins * _Opacity * input.puff.z;
                Light light = GetMainLight();
                float lit = saturate(0.55 + uv.y * 0.3 + dot(float3(0, 1, 0), light.direction) * 0.2);
                float3 ambient = WG_LookActive() ? _WG_Sky.rgb : SampleSH(float3(0, 1, 0));
                float3 color = lerp(_Shade.rgb, _Lit.rgb, lit) * (ambient * 1.7 + light.color * lit * 0.3);
                color = WG_ApplyFog(color, input.positionWS, 0);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
