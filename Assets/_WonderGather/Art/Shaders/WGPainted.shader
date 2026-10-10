// Wonder Gather painted surface (S1b look test): plaster, timber, stone, cloth, skin, ground.
// Light leads: a soft, brush-broken edge between light and shade, cool coloured shade,
// warm pools from local lights, a cool rim, and aerial perspective. See WGCommon.hlsl.
Shader "Wonder Gather/Painted"
{
    Properties
    {
        _BaseColor("Colour", Color) = (0.8, 0.75, 0.65, 1)
        _BaseMap("Colour texture", 2D) = "white" {}
        _Variation("Colour variation", Range(0, 1)) = 0.6
        _Brush("Brushwork", Range(0, 1)) = 0.6
        _BrushScale("Brush scale (per metre)", Float) = 3
        _Softness("Light edge softness", Range(0.02, 0.8)) = 0.2
        _Translucency("Translucency", Range(0, 1)) = 0
        _Gloss("Painted highlight", Range(0, 1)) = 0
        [HDR] _EmissionColor("Emission", Color) = (0, 0, 0, 1)
        [Toggle] _VertexColor("Use vertex colour", Float) = 0
        _Sway("Wind sway", Range(0, 1)) = 0
        _DirectOcclusion("Vertex alpha also shades direct light", Range(0, 1)) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
        [Toggle] _Drawn("Drawn being (kept clear of the paint filter)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _BaseMap_ST;
            float _Variation;
            float _Brush;
            float _BrushScale;
            float _Softness;
            float _Translucency;
            float _Gloss;
            float4 _EmissionColor;
            float _VertexColor;
            float _Sway;
            float _DirectOcclusion;
            float _Cull;
            float _Drawn;
        CBUFFER_END
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "WGCommon.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 color : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float fogCoord : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 Sway(float3 positionWS, float3 originWS)
            {
                if (_Sway <= 0) return positionWS;
                float lift = saturate((positionWS.y - originWS.y) / 1.2);
                float2 wind = WG_WindAt(positionWS, _Time.y) * _Sway * lift * lift * 0.25;
                return positionWS + float3(wind.x, 0, wind.y);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                positionWS = Sway(positionWS, TransformObjectToWorld(float3(0, 0, 0)));
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.color = _VertexColor > 0.5 ? input.color : float4(1, 1, 1, 1);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = input.positionWS;
                float3 n = normalize(input.normalWS) * (frontFace ? 1 : -1);
                float3 v = GetWorldSpaceNormalizeViewDir(positionWS);

                WGSurface s;
                float brushScale = _BrushScale * (_WG_Paint.z > 0 ? _WG_Paint.z : 1);
                s.brush = lerp(0.5, WG_Brush(positionWS, n, brushScale), _Brush);
                float3 baseColor = _BaseColor.rgb * input.color.rgb * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb;
                s.albedo = _WG_Paint.w > 0.5 ? baseColor : WG_PaintAlbedo(baseColor, positionWS, s.brush, _Variation);
                s.normal = n;
                s.position = positionWS;
                s.view = v;
                s.softness = _Softness;
                s.translucency = _Translucency;
                s.gloss = _Gloss;
                s.occlusion = input.color.a;

                InputData inputData = (InputData)0;
                inputData.positionWS = positionWS;
                inputData.normalWS = n;
                inputData.viewDirectionWS = v;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    inputData.shadowCoord = ComputeScreenPos(TransformWorldToHClip(positionWS));
                #else
                    inputData.shadowCoord = TransformWorldToShadowCoord(positionWS);
                #endif
                half4 shadowMask = half4(1, 1, 1, 1);

                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
                    s.occlusion *= aoFactor.indirectAmbientOcclusion;
                #endif

                Light mainLight = GetMainLight(inputData.shadowCoord, positionWS, shadowMask);
                // Ground under dense grass sits in the blades' shade from the sun and moon...
                float3 direct = WG_Direct(s, mainLight, false) * lerp(1, input.color.a, _DirectOcclusion);
                float3 local = 0;

                #if defined(_ADDITIONAL_LIGHTS)
                    uint lightCount = GetAdditionalLightsCount();
                    #if USE_CLUSTER_LIGHT_LOOP
                    [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
                    {
                        Light light = GetAdditionalLight(lightIndex, positionWS, shadowMask);
                        direct += WG_Direct(s, light, false);
                    }
                    #endif
                    LIGHT_LOOP_BEGIN(lightCount)
                        Light light = GetAdditionalLight(lightIndex, positionWS, shadowMask);
                        local += WG_Direct(s, light, true);
                    LIGHT_LOOP_END
                #endif

                // ...but lamplight from the house falls low between the blades and reaches the soil, so
                // its warm pool reads the same up close as from far away.
                direct += local * lerp(1, input.color.a, _DirectOcclusion * 0.35);
                float3 color = WG_Compose(s, direct, WG_Ambient(n));
                color += _EmissionColor.rgb * (WG_LookActive() ? _WG_GlowScale : 1);
                color = WG_ApplyFog(color, positionWS, input.fogCoord);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(positionCS);
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half DepthFrag(Varyings input) : SV_Target { return input.positionCS.z; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NormalsVert
            #pragma fragment NormalsFrag
            #pragma multi_compile_instancing

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; };

            Varyings NormalsVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 NormalsFrag(Varyings input, bool frontFace : SV_IsFrontFace) : SV_Target
            {
                float3 n = normalize(input.normalWS) * (frontFace ? 1 : -1);
                // The normals' alpha says what a pixel is: 0 the painted world, 1 grass, -1 a drawn being.
                return half4(NormalizeNormalPerPixel(n), _Drawn > 0.5 ? -1 : 0);
            }
            ENDHLSL
        }
    }
}
