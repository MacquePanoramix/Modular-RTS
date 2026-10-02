// Wonder Gather painted cumulus (S1e). Clouds as architecture: warm crowns, lilac-grey bellies,
// painted bands of light broken by a brush, folds kept in shadow, a silver lining towards the
// sun, and silhouettes that dissolve softly into the sky behind them. Colours come from the
// time of day (globals in WGCommon); meshes come from Art/Blender/OrdinaryPlace/clouds.py, with
// occlusion in the red vertex channel and height through the cloud in the green.
Shader "Wonder Gather/Cloud"
{
    Properties
    {
        _Brightness("Brightness", Range(0.5, 2)) = 1.25
        _Softness("Light edge softness", Range(0.02, 0.4)) = 0.07
        _Fold("Shadow in the folds", Range(0, 1)) = 0.45
        _Silver("Silver lining", Range(0, 3)) = 1.4
        _EdgeFade("Silhouette softness", Range(0, 1)) = 0.35
        _Boil("Slow boiling", Range(0, 0.06)) = 0.015
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry+10" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "WGCommon.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float _Brightness, _Softness, _Fold, _Silver, _EdgeFade, _Boil;
        CBUFFER_END

        // The domes swell and settle very slowly, the crown more than the base.
        float3 Boiled(float3 positionOS, float3 normalOS, float height)
        {
            float n = WG_Noise(positionOS * 2.6 + _Time.y * 0.025) - 0.5;
            return positionOS + normalOS * n * _Boil * smoothstep(0.05, 0.6, height);
        }
        ENDHLSL

        Pass
        {
            Name "Cloud"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 data : TEXCOORD2;     // occlusion, height through the cloud
                float3 positionOS : TEXCOORD3;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 p = Boiled(input.positionOS.xyz, input.normalOS, input.color.g);
                output.positionWS = TransformObjectToWorld(p);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.data = input.color.rg;
                output.positionOS = p;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 n = normalize(input.normalWS);
                float3 v = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 l = normalize(_MainLightPosition.xyz);
                float occlusion = input.data.x;
                float height = input.data.y;

                // Painted bands: shadow, half-tone, light, their borders broken by a brush.
                float brush = WG_Fbm(input.positionOS * 5.0) - 0.5;
                float wrap = dot(n, l) * 0.5 + 0.5 + brush * 0.22;
                float halfTone = smoothstep(0.3 - _Softness, 0.3 + _Softness, wrap);
                float lit = smoothstep(0.56 - _Softness, 0.56 + _Softness, wrap);
                float3 shade = _WG_CloudShade.rgb;
                float3 light = _WG_CloudLit.rgb;
                float3 mid = lerp(shade, light, 0.5) * float3(1.0, 0.97, 1.03);
                float3 color = lerp(shade, mid, halfTone);
                color = lerp(color, light, lit);

                // Folds between domes stay in shadow; the flat belly is cooler and darker.
                color = lerp(color, shade * 0.88, saturate(1 - occlusion) * _Fold);
                color *= lerp(0.84, 1.04, smoothstep(0.0, 0.4, height));
                // Sky light on the upper sides.
                color += _WG_SkyZenith.rgb * 0.12 * saturate(n.y);

                // The silver lining: the cloud's edge glows when the light is behind it.
                float rim = pow(1 - saturate(dot(n, v)), 3);
                float towards = pow(saturate(dot(-v, l)), 6);
                color += _WG_CloudEdge.rgb * rim * (towards * _Silver + lit * 0.25);
                color *= _Brightness;

                // The silhouette dissolves into the sky behind, as a painted cloud's edge does.
                float3 behind = WG_SkyGradient(-v) * _WG_SkyExposure;
                float facing = smoothstep(0.0, 0.35, saturate(dot(n, v)));
                color = lerp(behind, color, lerp(1 - _EdgeFade, 1, facing));

                color = WG_ApplyFog(color, input.positionWS, 0);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag

            struct DepthAttributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 color : COLOR; };

            float4 DepthVert(DepthAttributes input) : SV_POSITION
            {
                return TransformObjectToHClip(Boiled(input.positionOS.xyz, input.normalOS, input.color.g));
            }

            half DepthFrag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
