// Wonder Gather screen-space look candidates (S1c): a paint filter, loose ink contours and
// paper grain. LookPostEffects injects these passes only in scenes that ask for them.
Shader "Hidden/Wonder Gather/Look Post"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

        float4 _WG_PostParams;   // x paint strength, y ink strength, z grain strength, w paint radius in pixels
        float4 _WG_InkColor;     // rgb ink tint (multiplied with the scene), a: line width in pixels

        float Hash(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        float ValueNoise(float2 p)
        {
            float2 i = floor(p), f = frac(p);
            f = f * f * (3 - 2 * f);
            return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x), lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), f.x), f.y);
        }
        ENDHLSL

        // 0: Paint filter. A four-sector Kuwahara: each pixel takes the calmest neighbouring
        // patch, so fine detail becomes flat strokes while edges stay sharp.
        Pass
        {
            Name "Paint"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 texel = 1.0 / _ScreenParams.xy;
                int radius = (int)clamp(_WG_PostParams.w, 1, 6);
                float3 mean[4];
                float3 sq[4];
                [unroll] for (int k = 0; k < 4; k++) { mean[k] = 0; sq[k] = 0; }
                for (int y = -radius; y <= radius; y++)
                for (int x = -radius; x <= radius; x++)
                {
                    float3 c = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(x, y) * texel, 0).rgb;
                    // Compress highlights so bright windows do not dominate the sector choice.
                    float3 m = c / (1 + c);
                    if (x <= 0 && y <= 0) { mean[0] += m; sq[0] += m * m; }
                    if (x >= 0 && y <= 0) { mean[1] += m; sq[1] += m * m; }
                    if (x <= 0 && y >= 0) { mean[2] += m; sq[2] += m * m; }
                    if (x >= 0 && y >= 0) { mean[3] += m; sq[3] += m * m; }
                }
                float n = (radius + 1) * (radius + 1);
                float best = 1e9;
                float3 result = 0;
                [unroll] for (int s = 0; s < 4; s++)
                {
                    float3 mu = mean[s] / n;
                    float3 variance = abs(sq[s] / n - mu * mu);
                    float v = variance.r + variance.g + variance.b;
                    if (v < best) { best = v; result = mu; }
                }
                result = result / max(1e-4, 1 - result);
                float3 original = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
                // Drawn beings (normals alpha -1) keep their few marks: only a touch of paint.
                float drawn = step(SAMPLE_TEXTURE2D_X_LOD(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv, 0).a, -0.5);
                return half4(lerp(original, result, saturate(_WG_PostParams.x) * (1 - drawn * 0.85)), 1);
            }
            ENDHLSL
        }

        // 1: Ink. Multiplied over the scene: loose contours at silhouettes and creases,
        // wobbling slightly in width, fading with distance, and never drawn over grass
        // (grass writes 1 into the normals texture's alpha).
        Pass
        {
            Name "Ink"
            Blend DstColor Zero
            HLSLPROGRAM
            #pragma vertex InkVert
            #pragma fragment InkFrag

            struct InkVaryings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            InkVaryings InkVert(uint id : SV_VertexID)
            {
                InkVaryings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(id);
                o.uv = GetFullScreenTriangleTexCoord(id);
                return o;
            }

            float Depth(float2 uv) { return LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams); }

            half4 InkFrag(InkVaryings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 texel = 1.0 / _ScreenParams.xy;
                float centre = Depth(uv);
                // Wobble: the line's width breathes along the screen like a loose brush.
                float wobble = lerp(0.55, 1.35, ValueNoise(uv * _ScreenParams.xy / 9));
                float width = _WG_InkColor.a * wobble;
                float2 o = texel * width;
                float d1 = Depth(uv + float2(o.x, 0)), d2 = Depth(uv - float2(o.x, 0));
                float d3 = Depth(uv + float2(0, o.y)), d4 = Depth(uv - float2(0, o.y));
                float depthEdge = (abs(d1 - centre) + abs(d2 - centre) + abs(d3 - centre) + abs(d4 - centre)) / max(centre, 0.1);
                float silhouette = smoothstep(0.06, 0.16, depthEdge);
                float3 n0 = SampleSceneNormals(uv);
                float3 n1 = SampleSceneNormals(uv + float2(o.x, 0));
                float3 n2 = SampleSceneNormals(uv + float2(0, o.y));
                float crease = smoothstep(0.35, 0.7, (1 - dot(n0, n1)) + (1 - dot(n0, n2)));
                float stroke = max(silhouette, crease * 0.55);
                float grass = SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv).a;
                stroke *= 1 - saturate(grass);
                // Contours belong to near things; distance dissolves them.
                stroke *= 1 - smoothstep(18, 45, centre);
                stroke *= saturate(_WG_PostParams.y);
                float3 ink = lerp(float3(1, 1, 1), _WG_InkColor.rgb, stroke);
                return half4(ink, 1);
            }
            ENDHLSL
        }

        // 2: Paper grain: fibres and a faint mottling, so the image sits on a surface.
        Pass
        {
            Name "Grain"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float3 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
                float2 px = uv * _ScreenParams.xy;
                float fibres = ValueNoise(float2(px.x * 0.9, px.y * 0.12)) * 0.6 + ValueNoise(float2(px.x * 0.13, px.y * 0.8)) * 0.4;
                float mottle = ValueNoise(px / 140) * 0.7 + ValueNoise(px / 37) * 0.3;
                float grain = (fibres - 0.5) * 0.6 + (mottle - 0.5) * 0.4 + (Hash(px) - 0.5) * 0.35;
                float strength = _WG_PostParams.z * 0.07;
                color *= 1 + grain * strength;
                // A soft vignette keeps the eye in the frame.
                float2 c = uv - 0.5;
                color *= lerp(1, 1 - dot(c, c) * 0.55, saturate(_WG_PostParams.z));
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
