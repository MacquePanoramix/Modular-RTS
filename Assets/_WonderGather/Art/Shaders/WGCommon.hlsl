#ifndef WONDER_GATHER_COMMON_INCLUDED
#define WONDER_GATHER_COMMON_INCLUDED

// Wonder Gather's painted light (S1b/S1c look test). Shared by the painted surface,
// grass and sky shaders. Every scene-wide value is a global set by the TimeOfDay
// component. When no Ordinary Place is loaded, _WG_Sky.a is 0 and surfaces fall back
// to URP's own ambient and fog, so other maps are unaffected.

float4 _WG_Sky;         // rgb: ambient from above; a: 1 when the look is active
float4 _WG_Ground;      // rgb: ambient from below
float4 _WG_Shade;       // rgb: tint where direct light is missing (cool); a: saturation kept in shade
float4 _WG_Rim;         // rgb: cool edge light; a: edge power
float4 _WG_FogShape;    // x: distance (m) the air stays clear, y: thickest air
float4 _WG_Paint;       // x: colour variation, y: brush break of light edges, z: brush scale, w: baseline (1 = plain Lambert)
float4 _WG_Wind;        // xy: direction, z: strength, w: gust speed
float4 _WG_LightPool;   // x: pool softness of local lights, y: warm boost of local lights
float _WG_GlowScale;    // how lit the house's windows are (0..1), set by TimeOfDay
float4 _WG_Air;         // x: distance (m) over which low air reaches 63% thickness, y: thickest air, z: haze height (m), w: haze base height
float4 _WG_CloudShadow; // x: cover (0..1), y: darkness, z: size (m), w: drift (m/s)

// The sky's colours, set every frame by TimeOfDay (linear). The sky, the clouds, the water and
// the aerial perspective all read them, so the air in every distance is the sky's own colour.
float4 _WG_SkyZenith, _WG_SkyHorizon, _WG_SkyGlow, _WG_SkyBelow, _WG_SunDirection, _WG_SunColor, _WG_MoonDirection;
float4 _WG_CloudLit, _WG_CloudShade, _WG_CloudEdge;
float _WG_SkyGlowStrength, _WG_MoonVisibility, _WG_Stars, _WG_Galaxy, _WG_CloudCover, _WG_SkyExposure;

// ---------------------------------------------------------------------------
// Noise
// ---------------------------------------------------------------------------
float WG_Hash(float3 p)
{
    p = frac(p * 0.3183099 + 0.1);
    p *= 17.0;
    return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
}

float WG_Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float WG_Noise(float3 x)
{
    float3 i = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(lerp(WG_Hash(i), WG_Hash(i + float3(1, 0, 0)), f.x),
                     lerp(WG_Hash(i + float3(0, 1, 0)), WG_Hash(i + float3(1, 1, 0)), f.x), f.y),
                lerp(lerp(WG_Hash(i + float3(0, 0, 1)), WG_Hash(i + float3(1, 0, 1)), f.x),
                     lerp(WG_Hash(i + float3(0, 1, 1)), WG_Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
}

float WG_Fbm(float3 p)
{
    return WG_Noise(p) * 0.5 + WG_Noise(p * 2.03) * 0.25 + WG_Noise(p * 4.01) * 0.125 + WG_Noise(p * 8.05) * 0.0625;
}

bool WG_LookActive() { return _WG_Sky.a > 0.5; }

// ---------------------------------------------------------------------------
// Painterly surface character
// ---------------------------------------------------------------------------

// Paint dabs: the surface is divided into elongated patches, each with its own tone, the way
// a brush lays paint. Light edges and colour both follow the dab shapes.
float2 WG_Hash22(float2 p)
{
    float3 q = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
    q += dot(q, q.yzx + 33.33);
    return frac((q.xx + q.yz) * q.zy);
}

float WG_Dabs(float2 uv)
{
    float2 cell = floor(uv);
    float2 f = frac(uv);
    float best = 8, second = 8, id = 0;
    [unroll] for (int j = -1; j <= 1; j++)
    [unroll] for (int i = -1; i <= 1; i++)
    {
        float2 g = float2(i, j);
        float2 r = g + WG_Hash22(cell + g) - f;
        float d = dot(r, r);
        if (d < best) { second = best; best = d; id = WG_Hash21(cell + g); }
        else if (d < second) second = d;
    }
    // Soften dab borders slightly so they read as overlapping strokes rather than tiles.
    float border = saturate((sqrt(second) - sqrt(best)) * 3);
    return lerp(0.5, id, 0.35 + 0.65 * border);
}

// Returns 0..1 around 0.5, tied to the object's surface.
float WG_Brush(float3 p, float3 n, float scale)
{
    float3 s = abs(n.y) > 0.92 ? float3(1, 0, 0) : normalize(cross(n, float3(0, 1, 0)));
    float3 t = cross(n, s);
    float a = dot(p, s), b = dot(p, t);
    // Long dabs along the surface, and a smaller rotated layer so no direction dominates.
    float large = WG_Dabs(float2(a * scale * 0.7, b * scale * 2.2));
    float small = WG_Dabs(float2((a + b) * scale * 2.4, (a - b) * scale * 1.1) + 17.3);
    return saturate(large * 0.68 + small * 0.32);
}

// Hue and value drift across a surface: warm in its lighter patches, cool in its darker ones.
float3 WG_PaintAlbedo(float3 albedo, float3 p, float brush, float variation)
{
    float low = WG_Fbm(p * 0.33);
    float mid = WG_Noise(p * 1.9 + 3.1);
    float v = (low - 0.47) * 1.8 + (mid - 0.5) * 0.8 + (brush - 0.5) * 0.25;
    v *= variation * _WG_Paint.x;
    float3 c = albedo * (1.0 + v);
    c *= lerp(float3(1, 1, 1), float3(1.05, 1.0, 0.93), saturate(v * 3));
    c *= lerp(float3(1, 1, 1), float3(0.94, 0.99, 1.07), saturate(-v * 3));
    return max(c, 0);
}

// ---------------------------------------------------------------------------
// Light
// ---------------------------------------------------------------------------
struct WGSurface
{
    float3 albedo;
    float3 normal;
    float3 position;
    float3 view;            // towards the camera
    float brush;            // 0..1 brush field
    float softness;         // width of the light/shade transition
    float translucency;     // light passing through thin things (grass, leaves, cloth)
    float gloss;            // small painted highlight
    float occlusion;
};

float3 WG_Ambient(float3 n)
{
    if (!WG_LookActive()) return SampleSH(n);
    return lerp(_WG_Ground.rgb, _WG_Sky.rgb, saturate(n.y * 0.5 + 0.5));
}

// Direct light from one source, with a soft, brush-broken edge between light and shade.
float3 WG_Direct(WGSurface s, Light light, bool local)
{
    float brushBreak = _WG_Paint.y * (1 - _WG_Paint.w);
    float ndl = dot(s.normal, light.direction);
    float edge = ndl + (s.brush - 0.5) * brushBreak;
    float lit = _WG_Paint.w > 0.5 ? saturate(ndl) : smoothstep(-s.softness, s.softness, edge);
    float shadow = light.shadowAttenuation;
    shadow = saturate((shadow - 0.5) * 1.5 + 0.5 + (s.brush - 0.5) * 0.4 * brushBreak);
    float through = saturate(-ndl) * s.translucency;
    float atten = light.distanceAttenuation;
    if (local)
    {
        // Pools of warmth: a broader, gentler falloff than physical light, warmed slightly.
        atten = pow(saturate(atten), _WG_LightPool.x > 0 ? _WG_LightPool.x : 1.0);
    }
    float3 color = light.color * atten;
    if (local) color *= 1.0 + _WG_LightPool.y * float3(0.12, 0.0, -0.1);
    float3 result = color * (lit + through) * shadow;
    if (s.gloss > 0)
    {
        float3 h = normalize(light.direction + s.view);
        float spec = pow(saturate(dot(s.normal, h)), 48);
        result += color * smoothstep(0.35, 0.55, spec) * s.gloss * shadow * lit;
    }
    return result;
}

float WG_Luma(float3 c) { return dot(c, float3(0.299, 0.587, 0.114)); }

// Combines ambient and direct light so that what light misses keeps colour:
// shade is tinted cool and stays saturated instead of turning grey or black.
float3 WG_Compose(WGSurface s, float3 direct, float3 ambient)
{
    float3 lightSum = ambient * s.occlusion + direct;
    float3 color = s.albedo * lightSum;
    if (WG_LookActive() && _WG_Paint.w < 0.5)
    {
        float directLuma = WG_Luma(direct);
        float shade = 1 - saturate(directLuma / (directLuma + WG_Luma(ambient) + 1e-4));
        color *= lerp(float3(1, 1, 1), _WG_Shade.rgb, shade);
        float luma = WG_Luma(color);
        color = max(0, lerp(luma.xxx, color, 1 + shade * _WG_Shade.a));
        // A cool edge light, strongest where the surface turns away from the camera.
        float rim = pow(1 - saturate(dot(s.normal, s.view)), max(_WG_Rim.a, 1));
        color += _WG_Rim.rgb * rim * s.albedo * 1.5;
    }
    return color;
}

// ---------------------------------------------------------------------------
// Sky and air
// ---------------------------------------------------------------------------

// The painted sky's gradient in a direction: zenith to horizon, a few broad strokes, the dusk
// band gathering on the sun's side, and the sun's halo. No clouds, stars or discs.
float3 WG_SkyGradient(float3 d)
{
    float up = d.y;
    float g = pow(saturate(up), 0.6);
    float3 sky = lerp(_WG_SkyHorizon.rgb, _WG_SkyZenith.rgb, g);
    // A painter does not let warm horizon and blue zenith average to grey: the middle keeps its colour.
    float keep = 4 * g * (1 - g);
    float luma = dot(sky, float3(0.299, 0.587, 0.114));
    sky = max(0, lerp(luma.xxx, sky, 1 + 0.6 * keep));
    float strokes = WG_Fbm(float3(d.x * 2.2, d.y * 7.0, d.z * 2.2)) - 0.47;
    sky *= 1 + strokes * 0.12;
    float3 sunDir = normalize(_WG_SunDirection.xyz);
    float2 flatD = normalize(d.xz + 1e-5), flatSun = normalize(sunDir.xz + 1e-5);
    float sunSide = pow(saturate(dot(flatD, flatSun) * 0.5 + 0.5), 2.2);
    float band = exp(-abs(up - 0.05) * 6) * (0.35 + 0.65 * sunSide);
    sky = lerp(sky, _WG_SkyGlow.rgb, saturate(band * _WG_SkyGlowStrength));
    sky += _WG_SunColor.rgb * pow(saturate(dot(d, sunDir)), 24) * 0.12 * saturate(sunDir.y * 6 + 0.4);
    return sky;
}

// The colour of the air looking along a direction: the sky a little above the horizon there,
// so far ranges turn blue and read as shapes against the paler sky right behind them.
float3 WG_AirColor(float3 dir)
{
    return WG_SkyGradient(normalize(float3(dir.x, max(dir.y, 0.0) * 0.5 + 0.12, dir.z))) * _WG_SkyExposure;
}

// Aerial perspective. Each farther layer takes more of the sky's own colour, until far ranges
// nearly dissolve into it. The air is thickest low down: haze gathers over the lake and in the
// valleys while the peaks stay clear.
float WG_AirAmount(float3 positionWS)
{
    float3 toPoint = positionWS - _WorldSpaceCameraPos;
    float dist = length(toPoint);
    float k = 1.0 / max(_WG_Air.z, 1);
    float h0 = max(_WorldSpaceCameraPos.y - _WG_Air.w, 0) * k;
    float h1 = max(positionWS.y - _WG_Air.w, 0) * k;
    float a = exp(-h0), b = exp(-h1);
    // The ray's mean density between the two heights.
    float density = abs(h1 - h0) > 1e-3 ? (a - b) / (h1 - h0) : a;
    float optical = max(0, dist - _WG_FogShape.x) / max(_WG_Air.x, 1) * density;
    return min(1 - exp(-optical), _WG_Air.y);
}

float3 WG_ApplyFog(float3 color, float3 positionWS, float urpFogCoord)
{
    if (!WG_LookActive()) return MixFog(color, urpFogCoord);
    float3 dir = normalize(positionWS - _WorldSpaceCameraPos);
    return lerp(color, WG_AirColor(dir), WG_AirAmount(positionWS));
}

// Shadows of clouds drifting over the land: soft-edged patches that bring the sky's movement to
// the ground, seen even from the Strategy camera. 1 in the open, lower under a cloud.
float WG_CloudShadow(float3 positionWS)
{
    if (!WG_LookActive() || _WG_CloudShadow.y <= 0) return 1;
    float3 l = _MainLightPosition.xyz;
    // Follow the light up to the cloud layer.
    float lift = (900 - positionWS.y) / max(l.y, 0.12);
    float2 q = positionWS.xz + l.xz * lift;
    float2 wind = normalize(_WG_Wind.xy + float2(1e-4, 0));
    q -= wind * _Time.y * _WG_CloudShadow.w;
    float2 uv = q / max(_WG_CloudShadow.z, 1);
    float2 warp = float2(WG_Noise(float3(uv * 0.6, 3.1)), WG_Noise(float3(uv * 0.6, 7.9))) - 0.5;
    float field = WG_Fbm(float3(uv + warp * 0.8, 1.3));
    // The field sits around 0.47; cover 0.3 leaves roughly a third of the land in shadow.
    float threshold = 0.66 - _WG_CloudShadow.x * 0.42;
    float shadow = smoothstep(threshold - 0.03, threshold + 0.05, field);
    return 1 - shadow * _WG_CloudShadow.y;
}

// ---------------------------------------------------------------------------
// Wind, shared by grass and foliage.
// ---------------------------------------------------------------------------
float2 WG_WindAt(float3 positionWS, float time)
{
    float2 dir = _WG_Wind.xy;
    if (dot(dir, dir) < 1e-4) dir = float2(0.8, 0.6);
    dir = normalize(dir);
    float along = dot(positionWS.xz, dir);
    float gust = WG_Noise(float3(positionWS.xz * 0.06 - dir * time * _WG_Wind.w * 0.35, time * 0.05));
    float wave = sin(along * 0.45 - time * (1.6 + _WG_Wind.w)) * 0.5 + 0.5;
    float strength = _WG_Wind.z * (0.35 + gust * 0.9) * (0.6 + wave * 0.4);
    float flutter = sin(time * 4.1 + positionWS.x * 3.7 + positionWS.z * 2.9) * 0.12;
    return dir * strength + float2(-dir.y, dir.x) * flutter * _WG_Wind.z;
}

#endif
