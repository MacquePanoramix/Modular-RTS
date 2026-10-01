// Wonder Gather grass (S1b look test): GPU-instanced blades and seed heads drawn by GrassField.
// Dark blue-green at the root, warm and translucent at the tip, moved by gusting wind,
// and parted around walking bodies. Instance data comes from _Blades (see GrassField.cs).
Shader "Wonder Gather/Grass"
{
    Properties
    {
        _Root("Root colour", Color) = (0.035, 0.085, 0.075, 1)
        _Mid("Middle colour", Color) = (0.12, 0.23, 0.15, 1)
        _Tip("Tip colour", Color) = (0.42, 0.50, 0.28, 1)
        _Dry("Dry colour", Color) = (0.62, 0.54, 0.33, 1)
        _DryAmount("Dry patches", Range(0, 1)) = 0.3
        _Translucency("Translucency", Range(0, 2)) = 0.8
        _Sway("Wind response", Range(0, 2)) = 1
        _Stiffness("Stiffness", Range(0, 1)) = 0.2
        _Softness("Light edge softness", Range(0.05, 1)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "WGCommon.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _Root, _Mid, _Tip, _Dry;
            float _DryAmount, _Translucency, _Sway, _Stiffness, _Softness;
        CBUFFER_END

        // Per blade: xyz root position, w yaw; x height, y width, z rest bend, w random 0..1.
        StructuredBuffer<float4> _Blades;
        float4 _Movers[4];      // xyz position of bodies walking through the grass, w radius
        float _MoverCount;
        float _HeightScale;     // set per draw by GrassField

        struct GrassVertex
        {
            float3 positionWS;
            float3 normalWS;
            float height01;
            float random;
            float3 rootWS;
        };

        // Bends the blade by its rest curve, the wind and nearby bodies, keeping its length.
        GrassVertex BuildBlade(float3 positionOS, uint instanceID, float time)
        {
            float4 a = _Blades[instanceID * 2];
            float4 b = _Blades[instanceID * 2 + 1];
            float3 root = a.xyz;
            float yaw = a.w;
            float height = b.x * _HeightScale;
            float width = b.y;
            float h = saturate(positionOS.y);
            float s, c;
            sincos(yaw, s, c);
            float3 across = float3(c, 0, -s);
            float3 facing = float3(s, 0, c);

            float2 wind = WG_WindAt(root, time) * _Sway * (1 - _Stiffness);
            float2 push = 0;
            for (int i = 0; i < (int)_MoverCount; i++)
            {
                float2 away = root.xz - _Movers[i].xz;
                float dist = length(away);
                float r = _Movers[i].w;
                push += (dist > 1e-3 ? away / dist : float2(1, 0)) * saturate(1 - dist / r) * 1.4;
            }
            float3 lean = facing * b.z + float3(wind.x + push.x, 0, wind.y + push.y);
            float bendAmount = h * h;
            float3 offset = lean * bendAmount * height;
            float3 up = float3(0, h * height, 0);
            // Keep the blade's length roughly constant as it leans.
            float drop = 1 - 0.35 * saturate(dot(lean, lean)) * bendAmount;
            up.y *= drop;

            GrassVertex v;
            v.positionWS = root + across * positionOS.x * width * (1 - h * 0.85) + up + offset;
            float3 tangent = normalize(float3(0, height, 0) + lean * height * 2 * h + 1e-4);
            float3 n = normalize(cross(across, tangent));
            // Blades share light like a lawn: normals lean towards the sky.
            v.normalWS = normalize(lerp(n, float3(0, 1, 0), 0.55));
            v.height01 = h;
            v.random = b.w;
            v.rootWS = root;
            return v;
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

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
            #pragma multi_compile_fog


            struct Attributes { float4 positionOS : POSITION; uint instanceID : SV_InstanceID; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 data : TEXCOORD2;   // height along blade, random
                float3 rootWS : TEXCOORD3;
                float fogCoord : TEXCOORD4;
            };

            Varyings Vert(Attributes input)
            {
                GrassVertex v = BuildBlade(input.positionOS.xyz, input.instanceID, _Time.y);
                Varyings output;
                output.positionWS = v.positionWS;
                output.positionCS = TransformWorldToHClip(v.positionWS);
                output.normalWS = v.normalWS;
                output.data = float2(v.height01, v.random);
                output.rootWS = v.rootWS;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float h = input.data.x;
                float rnd = input.data.y;
                float3 positionWS = input.positionWS;
                float3 n = normalize(input.normalWS);
                float3 v = GetWorldSpaceNormalizeViewDir(positionWS);

                // Colour: root to tip, varied per blade and by broad dry patches.
                float3 color = h < 0.45 ? lerp(_Root.rgb, _Mid.rgb, h / 0.45) : lerp(_Mid.rgb, _Tip.rgb, (h - 0.45) / 0.55);
                float patch = WG_Fbm(float3(input.rootWS.xz * 0.08, 2.0));
                float dry = saturate((patch - 0.5) * 4 + (rnd - 0.5) * 0.6) * _DryAmount;
                color = lerp(color, _Dry.rgb * lerp(0.55, 1.1, h), dry * smoothstep(0.1, 0.7, h));
                color *= lerp(0.82, 1.15, rnd);
                color = WG_LookActive() && _WG_Paint.w < 0.5 ? WG_PaintAlbedo(color, input.rootWS, 0.5, 0.5) : color;

                WGSurface s;
                s.albedo = color;
                s.normal = n;
                s.position = positionWS;
                s.view = v;
                s.brush = 0.5 + (rnd - 0.5) * 0.5;
                s.softness = _Softness;
                s.translucency = _Translucency * h;
                s.gloss = 0;
                s.occlusion = lerp(0.35, 1.0, h);

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
                    s.occlusion *= GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV).indirectAmbientOcclusion;
                #endif

                Light mainLight = GetMainLight(inputData.shadowCoord, positionWS, shadowMask);
                float3 direct = WG_Direct(s, mainLight, false);
                // Back light glowing through the blades' tips.
                float backGlow = pow(saturate(dot(-v, mainLight.direction)), 4) * h * h * _Translucency;
                direct += mainLight.color * backGlow * mainLight.shadowAttenuation * 0.8;

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
                        // Local warm light catches the tips most of all.
                        direct += WG_Direct(s, light, true) * (0.5 + h * 1.6);
                    LIGHT_LOOP_END
                #endif

                float3 result = WG_Compose(s, direct, WG_Ambient(float3(0, 1, 0)) * lerp(0.7, 1.0, h));
                result = WG_ApplyFog(result, positionWS, input.fogCoord);
                return half4(result, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            struct Attributes { float4 positionOS : POSITION; uint instanceID : SV_InstanceID; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings DepthVert(Attributes input)
            {
                GrassVertex v = BuildBlade(input.positionOS.xyz, input.instanceID, _Time.y);
                Varyings output;
                output.positionCS = TransformWorldToHClip(v.positionWS);
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

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex NormalsVert
            #pragma fragment NormalsFrag

            struct Attributes { float4 positionOS : POSITION; uint instanceID : SV_InstanceID; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; };

            Varyings NormalsVert(Attributes input)
            {
                GrassVertex v = BuildBlade(input.positionOS.xyz, input.instanceID, _Time.y);
                Varyings output;
                output.positionCS = TransformWorldToHClip(v.positionWS);
                output.normalWS = v.normalWS;
                return output;
            }
            // Alpha 1 marks grass so screen-space ink can leave it alone.
            half4 NormalsFrag(Varyings input) : SV_Target { return half4(NormalizeNormalPerPixel(normalize(input.normalWS)), 1); }
            ENDHLSL
        }
    }
}
