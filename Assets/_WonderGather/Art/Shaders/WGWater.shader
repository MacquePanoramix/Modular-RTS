// Wonder Gather still water (S1e). Water doubles the sky: a near-mirror of the clouds and
// mountains (a planar reflection rendered by WaterSurface), broken into painted horizontal
// streaks by slow ripples. Turquoise where shallow, deep blue-green further out, a light line
// where it meets the land, and sun glitter along the light's path.
Shader "Wonder Gather/Water"
{
    Properties
    {
        _Shallow("Shallow colour", Color) = (0.30, 0.66, 0.62, 1)
        _Deep("Deep colour", Color) = (0.05, 0.24, 0.32, 1)
        _DepthScale("Depth to deep (m)", Float) = 7
        _Mirror("Mirror", Range(0, 1)) = 0.9
        _Ripple("Ripple distortion", Range(0, 0.1)) = 0.03
        _Streaks("Painted streaks", Range(0, 1)) = 0.55
        _Shore("Shore line", Range(0, 1)) = 0.7
        _Glitter("Sun glitter", Range(0, 4)) = 1.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-50" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Water"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "WGCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Shallow, _Deep;
                float _DepthScale, _Mirror, _Ripple, _Streaks, _Shore, _Glitter;
            CBUFFER_END

            TEXTURE2D(_WG_Reflection);
            float _WG_ReflectionReady;

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 p = input.positionWS;
                float3 toPoint = p - _WorldSpaceCameraPos;
                float dist = length(toPoint);
                float3 ray = toPoint / max(dist, 1e-4);
                float3 v = -ray;
                float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);

                // How deep the water is under this point, from the scene behind it.
                float3 forward = -UNITY_MATRIX_V[2].xyz;
                float sceneEye = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float3 bed = _WorldSpaceCameraPos + ray * (sceneEye / max(dot(ray, forward), 1e-3));
                // Far off, depth precision is too coarse to trust: treat distant water as deep.
                float depth = max(max(0, p.y - bed.y), smoothstep(250, 700, dist) * 40);

                // Ripples drawn as long horizontal strokes across the view, moving slowly.
                float3 right = UNITY_MATRIX_V[0].xyz;
                float2 across = normalize(right.xz + 1e-5);
                float2 along = float2(-across.y, across.x);
                float t = _Time.y;
                float2 q = float2(dot(p.xz, across) * 0.035, dot(p.xz, along) * 0.55);
                float ripple = (WG_Noise(float3(q + float2(t * 0.03, t * 0.25), t * 0.07)) - 0.5)
                             + (WG_Noise(float3(q * 2.7 + float2(-t * 0.02, t * 0.18), 5 + t * 0.11)) - 0.5) * 0.5;

                // The body: turquoise in the shallows, deep blue-green further out, lit by the sky.
                float deep = 1 - exp(-depth / max(_DepthScale, 0.1));
                float3 lightAmount = _WG_Sky.rgb * 1.5 + _MainLightColor.rgb * 0.22;
                float3 body = lerp(_Shallow.rgb, _Deep.rgb, deep) * lightAmount;

                // The mirror: the reflection camera's image, flipped (it renders upside down).
                float2 wobble = float2(ripple * 0.25, ripple) * _Ripple / (1 + dist * 0.01);
                float2 mirrorUV = float2(uv.x, 1 - uv.y) + wobble;
                float3 up = float3(0, 1, 0);
                float3 reflected = reflect(ray, normalize(up + float3(ripple * 0.05, 0, ripple * 0.02)));
                float3 mirror = _WG_ReflectionReady > 0.5
                    ? SAMPLE_TEXTURE2D_LOD(_WG_Reflection, sampler_LinearClamp, mirrorUV, 0).rgb
                    : WG_SkyGradient(reflected) * _WG_SkyExposure;
                // The mirror is a little deeper in colour than what it reflects.
                mirror = lerp(mirror, mirror * lerp(_Shallow.rgb, _Deep.rgb, 0.5) * 2.2, 0.18) * 0.92;
                float facing = saturate(v.y);
                float fresnel = lerp(0.3, 1.0, pow(1 - facing, 3)) * _Mirror;
                // Patches ruffled by the wind drift across the water: there the mirror breaks into
                // a matte sheen of sky, in broad bands as a painter lays them.
                float2 drift = normalize(_WG_Wind.xy + float2(1e-4, 0)) * t * 1.5;
                float ruffle = smoothstep(0.5, 0.68, WG_Fbm(float3((p.xz - drift) * float2(0.0025, 0.006), 2.3)));
                fresnel *= 1 - ruffle * 0.45;
                float3 color = lerp(body, mirror, fresnel);
                color = lerp(color, WG_AirColor(reflected) * 0.95, ruffle * 0.35);

                // Painted streaks: light lines of sky where the ripples catch it.
                float streak = smoothstep(0.16, 0.36, ripple) * _Streaks * (0.6 + ruffle);
                color = lerp(color, WG_AirColor(reflected) * 1.1, streak * 0.5);

                // Sun glitter along the light's path.
                float3 l = _MainLightPosition.xyz;
                float sparkle = smoothstep(0.82, 0.95, WG_Noise(float3(p.xz * 0.9, t * 0.8)));
                float path = pow(saturate(dot(reflected, l)), 90);
                color += _MainLightColor.rgb * path * (0.3 + sparkle * 2.5) * _Glitter * saturate(l.y * 4);

                // A light line where the water meets the land; the very edge fades into it.
                float shore = smoothstep(0.7, 0.0, depth + ripple * 0.15);
                float3 foam = WG_AirColor(reflected) * 1.15;
                color = lerp(color, foam, shore * _Shore * 0.55);
                float alpha = smoothstep(0.0, 0.18, depth);

                color = WG_ApplyFog(color, p, 0);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
