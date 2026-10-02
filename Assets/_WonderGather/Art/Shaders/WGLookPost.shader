// Wonder Gather screen-space look candidates (S1c): a paint filter, loose ink contours and
// paper grain. Since S1e also the painting pass: an anisotropic Kuwahara filter whose strokes
// follow the forms (passes 3-6), and a painter's palette grade in the grain pass.
// LookPostEffects injects these passes only in scenes that ask for them.
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
        float4 _WG_PaintShape;   // x painting stroke radius (px), y sharpness, z grade strength, w ink reach (see Ink)
        float4 _WG_GradeShadow;  // rgb: the hue shadows lean to; a: how far
        float4 _WG_GradeLight;   // rgb: the hue lights lean to; a: how far
        float _WG_GradeLift;     // midtone lift (1 = none, lower = brighter middle values)

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
                return half4(lerp(original, result, saturate(_WG_PostParams.x)), 1);
            }
            ENDHLSL
        }

        // 1: Ink. Multiplied over the scene: loose contours at silhouettes and creases,
        // wobbling slightly in width, fading with distance, and never drawn over grass. The
        // normals texture's alpha says what each surface is: 0 drawn (characters), 0.5 the painted
        // world, 1 grass. _WG_PaintShape.w is the highest alpha that still takes ink.
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
                float kind = SAMPLE_TEXTURE2D_X(_CameraNormalsTexture, sampler_CameraNormalsTexture, uv).a;
                stroke *= 1 - smoothstep(_WG_PaintShape.w, _WG_PaintShape.w + 0.1, kind);
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
                // A painter's palette for the hour: shadows keep their colour (richer, never grey) and
                // lean a little to the hour's shade hue, lights lean to its light hue, and the middle
                // values open up in daylight.
                float grade = _WG_PaintShape.z;
                if (grade > 0)
                {
                    const float3 lumaWeights = float3(0.299, 0.587, 0.114);
                    float luma = dot(color, lumaWeights);
                    float shadows = 1 - smoothstep(0.05, 0.4, luma);
                    float lights = smoothstep(0.5, 0.95, luma);
                    color = max(0, lerp(luma.xxx, color, 1 + 0.3 * shadows * grade));
                    float3 shadowHue = _WG_GradeShadow.rgb - dot(_WG_GradeShadow.rgb, lumaWeights);
                    float3 lightHue = _WG_GradeLight.rgb - dot(_WG_GradeLight.rgb, lumaWeights);
                    color += (shadowHue * shadows * _WG_GradeShadow.a + lightHue * lights * _WG_GradeLight.a) * grade;
                    color = pow(max(color, 0), lerp(1, max(_WG_GradeLift, 0.5), grade));
                }
                // A soft vignette keeps the eye in the frame.
                float2 c = uv - 0.5;
                color *= lerp(1, 1 - dot(c, c) * 0.55, saturate(_WG_PostParams.z));
                return half4(color, 1);
            }
            ENDHLSL
        }
        // 3: The structure of the image: how colour changes around each pixel (a structure tensor),
        // which the painting pass reads to lay its strokes along the forms.
        Pass
        {
            Name "Tensor"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float3 Tap(float2 uv)
            {
                float3 c = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
                return c / (1 + c);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 d = 1.0 / _ScreenParams.xy;
                float3 a = Tap(uv + d * float2(-1, -1)), b = Tap(uv + d * float2(0, -1)), c = Tap(uv + d * float2(1, -1));
                float3 l = Tap(uv + d * float2(-1, 0)), r = Tap(uv + d * float2(1, 0));
                float3 e = Tap(uv + d * float2(-1, 1)), f = Tap(uv + d * float2(0, 1)), g = Tap(uv + d * float2(1, 1));
                float3 gx = (-a - 2 * l - e + c + 2 * r + g) * 0.25;
                float3 gy = (-a - 2 * b - c + e + 2 * f + g) * 0.25;
                return half4(dot(gx, gx), dot(gx, gy), dot(gy, gy), 1);
            }
            ENDHLSL
        }

        // 4, 5: The structure softened, across then down, so strokes flow instead of jittering.
        HLSLINCLUDE
        half4 SoftenStructure(float2 uv, float2 step)
        {
            const float w[5] = { 0.2270270, 0.1945946, 0.1216216, 0.0540541, 0.0162162 };
            float4 sum = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0) * w[0];
            [unroll] for (int i = 1; i < 5; i++)
            {
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + step * i, 0) * w[i];
                sum += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - step * i, 0) * w[i];
            }
            return sum;
        }
        ENDHLSL

        Pass
        {
            Name "SoftenAcross"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            half4 Frag(Varyings input) : SV_Target { return SoftenStructure(input.texcoord, float2(1.6 / _ScreenParams.x, 0)); }
            ENDHLSL
        }

        Pass
        {
            Name "SoftenDown"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            half4 Frag(Varyings input) : SV_Target { return SoftenStructure(input.texcoord, float2(0, 1.6 / _ScreenParams.y)); }
            ENDHLSL
        }

        // 6: The painting. Each pixel takes the calmest of eight overlapping sectors of an ellipse
        // laid along the local form (anisotropic Kuwahara with polynomial weights, after
        // Kyprianidis, Semmo, Kang and Dollner), so flat detail becomes strokes that follow the
        // shapes while edges stay crisp. Strokes broaden with distance and towards the frame's
        // edges, and stay finest where the eye lands.
        Pass
        {
            Name "Painting"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            TEXTURE2D_X(_WG_Structure);

            float3 Colour(float2 uv)
            {
                float3 c = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
                return c / (1 + c);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 texel = 1.0 / _ScreenParams.xy;
                float3 original = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;

                // Local orientation and how strongly directional the image is here.
                float3 st = SAMPLE_TEXTURE2D_X_LOD(_WG_Structure, sampler_LinearClamp, uv, 0).xyz;
                float root = sqrt(max(0, (st.x - st.z) * (st.x - st.z) + 4 * st.y * st.y));
                float l1 = 0.5 * (st.x + st.z + root), l2 = 0.5 * (st.x + st.z - root);
                float2 along = float2(l1 - st.x, -st.y);
                along = dot(along, along) > 1e-12 ? normalize(along) : float2(0, 1);
                float phi = -atan2(along.y, along.x);
                float anisotropy = (l1 + l2) > 1e-8 ? (l1 - l2) / (l1 + l2) : 0;

                // Stroke size: broader with distance and towards the edges of the frame.
                float eye = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float2 fromCentre = (uv - 0.5) * float2(_ScreenParams.x / _ScreenParams.y, 1);
                float radius = _WG_PaintShape.x * lerp(0.8, 1.3, smoothstep(10, 900, eye)) * lerp(0.85, 1.2, saturate(length(fromCentre) * 1.3));
                radius = clamp(radius, 2, 10);

                float a = radius * clamp(1 + anisotropy, 0.1, 2.0);
                float b = radius * clamp(1 / (1 + anisotropy), 0.1, 2.0);
                float sinPhi, cosPhi;
                sincos(phi, sinPhi, cosPhi);
                float2x2 rotate = float2x2(cosPhi, -sinPhi, sinPhi, cosPhi);
                float2x2 squash = float2x2(0.5 / a, 0, 0, 0.5 / b);
                float2x2 toEllipse = mul(squash, rotate);
                int maxX = (int)sqrt(a * a * cosPhi * cosPhi + b * b * sinPhi * sinPhi);
                int maxY = (int)sqrt(a * a * sinPhi * sinPhi + b * b * cosPhi * cosPhi);
                // Wide strokes are sampled every other pixel (each tap lands between pixels, so the
                // filtering still sees them all): a quarter of the work for the same stroke size.
                int stride = radius > 3.5 ? 2 : 1;
                float zeta = 1.0 / radius;
                const float zeroCrossing = 0.58;
                float eta = (zeta + cos(zeroCrossing)) / (sin(zeroCrossing) * sin(zeroCrossing));

                float4 m[8];
                float3 s[8];
                float3 centre = Colour(uv);
                [unroll] for (int k = 0; k < 8; k++) { m[k] = float4(centre * 0.125, 0.125); s[k] = centre * centre * 0.125; }

                for (int j = 0; j <= maxY; j += stride)
                for (int i = -maxX; i <= maxX; i += stride)
                {
                    if (j == 0 && i <= 0) continue;
                    float2 offset = float2(i, j) + (stride > 1 ? 0.5 : 0);
                    float2 v = mul(toEllipse, offset);
                    float vv = dot(v, v);
                    if (vv > 0.25) continue;
                    float3 c0 = Colour(uv + offset * texel);
                    float3 c1 = Colour(uv - offset * texel);
                    float w[8];
                    float sum = 0, z, vxx, vyy;
                    vxx = zeta - eta * v.x * v.x;
                    vyy = zeta - eta * v.y * v.y;
                    z = max(0, v.y + vxx); w[0] = z * z; sum += w[0];
                    z = max(0, -v.x + vyy); w[2] = z * z; sum += w[2];
                    z = max(0, -v.y + vxx); w[4] = z * z; sum += w[4];
                    z = max(0, v.x + vyy); w[6] = z * z; sum += w[6];
                    v = 0.70710678 * float2(v.x - v.y, v.x + v.y);
                    vxx = zeta - eta * v.x * v.x;
                    vyy = zeta - eta * v.y * v.y;
                    z = max(0, v.y + vxx); w[1] = z * z; sum += w[1];
                    z = max(0, -v.x + vyy); w[3] = z * z; sum += w[3];
                    z = max(0, -v.y + vxx); w[5] = z * z; sum += w[5];
                    z = max(0, v.x + vyy); w[7] = z * z; sum += w[7];
                    float g = exp(-3.125 * vv) / max(sum, 1e-6);
                    [unroll] for (int k = 0; k < 8; k++)
                    {
                        float wk = w[k] * g;
                        m[k] += float4(c0 * wk, wk);
                        s[k] += c0 * c0 * wk;
                        m[(k + 4) & 7] += float4(c1 * wk, wk);
                        s[(k + 4) & 7] += c1 * c1 * wk;
                    }
                }

                float4 result = 0;
                [unroll] for (int k = 0; k < 8; k++)
                {
                    float3 mean = m[k].rgb / m[k].w;
                    float3 variance = abs(s[k] / m[k].w - mean * mean);
                    float sigma2 = variance.r + variance.g + variance.b;
                    float w = 1 / (1 + pow(max(255 * sigma2, 1e-6), 0.5 * _WG_PaintShape.y));
                    result += float4(mean * w, w);
                }
                float3 painted = result.rgb / result.w;
                painted = painted / max(1e-4, 1 - painted);
                return half4(lerp(original, painted, saturate(_WG_PostParams.x)), 1);
            }
            ENDHLSL
        }
    }
}
