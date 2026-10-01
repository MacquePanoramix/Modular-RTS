// Wonder Gather painted sky (S1b look test). A sky worth looking up at from the Explore
// camera: painted gradient and dusk band, soft cloud masses lit in bands, stars, a crescent
// moon and a faint galactic band at night. TimeOfDay drives every property.
Shader "Wonder Gather/Sky"
{
    Properties
    {
        _Zenith("Zenith", Color) = (0.20, 0.36, 0.62, 1)
        _Horizon("Horizon", Color) = (0.70, 0.78, 0.82, 1)
        _Glow("Dusk band", Color) = (1.0, 0.62, 0.42, 1)
        _GlowStrength("Dusk band strength", Range(0, 2)) = 0.3
        _Below("Below horizon", Color) = (0.20, 0.24, 0.22, 1)
        _SunDirection("Sun direction", Vector) = (0, 0.5, 0.8, 0)
        _SunColor("Sun colour", Color) = (1, 0.9, 0.7, 1)
        _SunSize("Sun size", Range(0.001, 0.1)) = 0.02
        _MoonDirection("Moon direction", Vector) = (0, 0.5, -0.8, 0)
        _MoonColor("Moon colour", Color) = (0.85, 0.88, 1.0, 1)
        _MoonSize("Moon size", Range(0.005, 0.12)) = 0.035
        _MoonPhase("Moon crescent", Range(0, 1)) = 0.65
        _MoonVisibility("Moon visibility", Range(0, 1)) = 1
        _Stars("Stars", Range(0, 2)) = 0
        _Galaxy("Galactic band", Range(0, 1)) = 0
        _CloudLit("Cloud light", Color) = (1, 0.95, 0.9, 1)
        _CloudShade("Cloud shade", Color) = (0.55, 0.6, 0.72, 1)
        _CloudEdge("Cloud edge light", Color) = (1, 0.75, 0.65, 1)
        _CloudCover("Cloud cover", Range(0, 1)) = 0.45
        _CloudSoftness("Cloud softness", Range(0.01, 0.5)) = 0.12
        _CloudScale("Cloud scale", Float) = 2.2
        _CloudSpeed("Cloud drift", Float) = 0.004
        _CloudOpacity("Cloud opacity", Range(0, 1)) = 0.9
        _Exposure("Exposure", Float) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "WGCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Zenith, _Horizon, _Glow, _Below, _SunDirection, _SunColor, _MoonDirection, _MoonColor;
                float4 _CloudLit, _CloudShade, _CloudEdge;
                float _GlowStrength, _SunSize, _MoonSize, _MoonPhase, _MoonVisibility, _Stars, _Galaxy;
                float _CloudCover, _CloudSoftness, _CloudScale, _CloudSpeed, _CloudOpacity, _Exposure;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            // Cloud density on a curved layer above the world, drifting with the wind.
            float CloudField(float2 uv, float time)
            {
                float2 drift = float2(1, 0.35) * time * _CloudSpeed;
                float2 warp = float2(WG_Fbm(float3(uv * 0.7 + drift, 1.7)), WG_Fbm(float3(uv * 0.7 - drift, 9.2))) - 0.47;
                float d = WG_Fbm(float3(uv + warp * 0.9 + drift, 0.0));
                d += (WG_Noise(float3(uv * 3.1 + drift * 2, 4.0)) - 0.5) * 0.12;
                return d;
            }

            float Stars(float3 d, float scale, float threshold, float time)
            {
                float3 p = d * scale;
                float3 cell = floor(p);
                float3 f = frac(p) - 0.5;
                float h = WG_Hash(cell);
                if (h < threshold) return 0;
                float3 offset = (float3(WG_Hash(cell + 3.1), WG_Hash(cell + 7.7), WG_Hash(cell + 1.3)) - 0.5) * 0.6;
                float r = length(f - offset);
                float twinkle = 0.7 + 0.3 * sin(time * (1.5 + h * 3) + h * 40);
                return smoothstep(0.22, 0.0, r) * twinkle * (h - threshold) / (1 - threshold);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 d = normalize(input.direction);
                float time = _Time.y;
                float3 sunDir = normalize(_SunDirection.xyz);
                float3 moonDir = normalize(_MoonDirection.xyz);
                float up = d.y;

                // Gradient, with a few broad painted strokes so the sky is not a perfect ramp.
                float t = saturate(up);
                float3 sky = lerp(_Horizon.rgb, _Zenith.rgb, pow(t, 0.45));
                float strokes = WG_Fbm(float3(d.x * 2.2, d.y * 7.0, d.z * 2.2)) - 0.47;
                sky *= 1 + strokes * 0.12;
                // The dusk band hugs the horizon and gathers towards the sun's side.
                float2 flatD = normalize(d.xz + 1e-5), flatSun = normalize(sunDir.xz + 1e-5);
                float sunSide = pow(saturate(dot(flatD, flatSun) * 0.5 + 0.5), 2.2);
                float band = exp(-abs(up - 0.05) * 6) * (0.35 + 0.65 * sunSide);
                sky = lerp(sky, _Glow.rgb, saturate(band * _GlowStrength));
                // Sun: a soft disc and halo.
                float sunDot = dot(d, sunDir);
                float sunDisc = smoothstep(cos(_SunSize * 1.15), cos(_SunSize), sunDot);
                sky += _SunColor.rgb * (pow(saturate(sunDot), 600) * 0.6 + pow(saturate(sunDot), 24) * 0.12) * saturate(sunDir.y * 6 + 0.4);
                sky += _SunColor.rgb * sunDisc * 4;

                // Night sky: stars and a faint galactic band.
                float night = _Stars * saturate(up * 4);
                if (night > 0)
                {
                    float stars = Stars(d, 260, 0.992, time) + Stars(d, 520, 0.996, time) * 0.6;
                    float3 axis = normalize(float3(0.35, 0.55, 0.76));
                    float galaxy = exp(-pow(dot(d, axis) * 5.5, 2)) * WG_Fbm(d * 9) * _Galaxy;
                    sky += (stars * float3(1, 0.96, 0.9) * 1.4 + galaxy * float3(0.55, 0.6, 0.95)) * night;
                    sky += Stars(d, 140, 0.9985, time) * float3(1, 0.85, 0.7) * 2 * night;
                }

                // Moon: a crescent with a soft halo.
                float moonDot = dot(d, moonDir);
                float disc = smoothstep(cos(_MoonSize * 1.08), cos(_MoonSize), moonDot);
                float3 side = normalize(cross(moonDir, float3(0, 1, 0)) + 1e-4);
                float3 shadowDir = normalize(moonDir + side * _MoonSize * 0.9 * _MoonPhase + float3(0, _MoonSize * 0.4, 0));
                float bite = smoothstep(cos(_MoonSize * 1.02), cos(_MoonSize * 0.95), dot(d, shadowDir)) * saturate(_MoonPhase * 1.5);
                float moon = saturate(disc - bite);
                sky += _MoonColor.rgb * (moon * 2.2 + pow(saturate(moonDot), 300) * 0.25 + pow(saturate(moonDot), 30) * 0.05) * saturate(moonDir.y * 5 + 0.3) * _MoonVisibility;

                // Clouds on a curved layer; lit in soft painted bands from the sun (or moon at night).
                if (up > -0.02 && _CloudOpacity > 0)
                {
                    float2 uv = d.xz / (up + 0.18) * _CloudScale;
                    float density = CloudField(uv, time);
                    float cover = 1 - _CloudCover;
                    float shape = smoothstep(cover - _CloudSoftness * 0.2, cover + _CloudSoftness, density);
                    float3 lightDir = sunDir.y > -0.05 ? sunDir : moonDir;
                    float2 toLight = normalize(lightDir.xz + 1e-4) * 0.09;
                    float towards = CloudField(uv + toLight, time);
                    float lit = saturate(0.5 + (density - towards) * 6);
                    lit = smoothstep(0.15, 0.85, lit);
                    float edge = saturate(1 - shape * 1.6) * shape * 3.5;
                    float3 cloud = lerp(_CloudShade.rgb, _CloudLit.rgb, lit);
                    cloud = lerp(cloud, _CloudEdge.rgb, saturate(edge * (0.4 + 0.6 * sunSide)) * 0.6);
                    float fade = smoothstep(-0.02, 0.18, up);
                    sky = lerp(sky, cloud, shape * fade * _CloudOpacity);
                }

                if (up < 0)
                    sky = lerp(sky, _Below.rgb, saturate(-up * 6));
                return half4(sky * _Exposure, 1);
            }
            ENDHLSL
        }
    }
}
