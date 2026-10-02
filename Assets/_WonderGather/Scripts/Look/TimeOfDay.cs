using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // Drives the Ordinary Place's light from one palette: sun or moon, the painted sky and its
    // clouds, ambient and shade tints, the air (aerial perspective), the shadows of drifting
    // clouds, and the warmth of the house's lights. Keyframes follow the Visual Soul references
    // (golden hour, E's lilac dusk, A's deep navy night with warm windows); since S1e the day
    // is luminous after the essence study (Docs/ArtDirection/TheEssence.md): cerulean sky,
    // white clouds and bright, coloured shade.
    [ExecuteAlways]
    public sealed class TimeOfDay : MonoBehaviour
    {
        // Sky, light and glow colours are given as painters pick them (sRGB). Ambient, shade and
        // rim colours go straight to the shaders as linear values. The air takes the sky's own
        // horizon colour: AirDistance is how far (m) low air goes before it is 63% thick,
        // AirMax caps it, and HazeHeight (m) is how quickly it thins with altitude.
        [Serializable]
        public struct Palette
        {
            public float Hour;
            public Color Zenith, Horizon, Glow, Below;
            public float GlowStrength;
            public Color CloudLit, CloudShade, CloudEdge;
            public float CloudCover;
            public Color AmbientSky, AmbientGround, Shade;
            public float ShadeSaturation;
            public Color Rim;
            public float AirDistance, AirMax, HazeHeight;
            // The painter's palette for the hour (look F): the hue shadows and lights lean to, how
            // far, and a midtone lift (1 = none).
            public Color GradeShadow, GradeLight;
            public float GradeShadowAmount, GradeLightAmount, GradeLift;
            public float CloudShadowCover, CloudShadowDarkness;
            public Color Light;
            public float LightIntensity, Stars, HouseLights, Exposure;
        }

        [Range(0, 24)] [SerializeField] private float hour = 18.6f;
        [Tooltip("In-game minutes per real second while playing; 0 holds the hour.")]
        [SerializeField] private float minutesPerSecond;
        [SerializeField] private Light sunAndMoon;
        [SerializeField] private Material sky, glow;
        [SerializeField] private Color glowColor = new Color(1f, .52f, .18f);
        [SerializeField] private float glowIntensity = 1.3f;
        [SerializeField] private Light[] houseLights = new Light[0];
        [SerializeField] private float[] houseLightIntensity = new float[0];
        [Tooltip("The moon rides opposite the sun, turned by this many degrees so it is not exactly opposite.")]
        [SerializeField] private float maxSunElevation = 52, moonOffset = 25;
        [SerializeField] private Vector3 wind = new Vector3(.8f, .55f, .55f);
        [SerializeField] private float gustSpeed = .8f;
        [Tooltip("The altitude the air thins from (the water's level), and the size (m) and drift (m/s) of cloud shadows.")]
        [SerializeField] private float airBase = -26, cloudShadowSize = 140, cloudShadowDrift = 6;
        [SerializeField] private Palette[] palettes = DefaultPalettes();

        public float Hour { get => hour; set { hour = Mathf.Repeat(value, 24); Apply(); } }
        public Color GlowEmission => glowColor * glowIntensity;
        public void ResetPalettes() => palettes = DefaultPalettes();
        public float MinutesPerSecond { get => minutesPerSecond; set => minutesPerSecond = value; }

        public void Configure(Light light, Material skyMaterial, Material glowMaterial, Light[] warmLights)
        {
            sunAndMoon = light;
            sky = skyMaterial;
            glow = glowMaterial;
            houseLights = warmLights;
            houseLightIntensity = new float[warmLights.Length];
            for (int i = 0; i < warmLights.Length; i++) houseLightIntensity[i] = warmLights[i].intensity;
            Apply();
        }

        private static Color C(float r, float g, float b) => new Color(r, g, b);

        public static Palette[] DefaultPalettes()
        {
            var night = new Palette
            {
                Hour = 0, Zenith = C(.12f, .18f, .40f), Horizon = C(.26f, .32f, .52f), Glow = C(.58f, .36f, .46f), GlowStrength = .3f,
                Below = C(.03f, .04f, .07f), CloudLit = C(.32f, .38f, .58f), CloudShade = C(.10f, .14f, .30f), CloudEdge = C(.58f, .44f, .58f),
                CloudCover = .42f, AmbientSky = C(.07f, .10f, .22f), AmbientGround = C(.025f, .03f, .045f), Shade = C(.78f, .88f, 1.18f),
                ShadeSaturation = .4f, Rim = C(.10f, .15f, .34f), AirDistance = 3200, AirMax = .45f, HazeHeight = 300,
                GradeShadow = C(.20f, .26f, .62f), GradeShadowAmount = .05f, GradeLight = C(1f, .72f, .42f), GradeLightAmount = .12f, GradeLift = 1,
                Light = C(.46f, .56f, .95f), LightIntensity = .32f, Stars = 1, HouseLights = 1, Exposure = 1.15f
            };
            var blueHour = night;
            blueHour.Hour = 5.6f; blueHour.Zenith = C(.18f, .24f, .48f); blueHour.Horizon = C(.52f, .48f, .64f); blueHour.Glow = C(.85f, .50f, .52f);
            blueHour.GlowStrength = .6f; blueHour.AmbientSky = C(.16f, .19f, .34f); blueHour.AirDistance = 3000; blueHour.AirMax = .76f;
            blueHour.CloudLit = C(.70f, .55f, .65f); blueHour.CloudShade = C(.20f, .22f, .38f); blueHour.Stars = .25f; blueHour.HouseLights = .8f;
            blueHour.Light = C(.75f, .62f, .78f); blueHour.LightIntensity = .25f;
            var sunrise = new Palette
            {
                Hour = 6.6f, Zenith = C(.30f, .45f, .74f), Horizon = C(.98f, .78f, .62f), Glow = C(1f, .62f, .42f), GlowStrength = .8f,
                Below = C(.12f, .13f, .12f), CloudLit = C(1f, .82f, .68f), CloudShade = C(.52f, .52f, .66f), CloudEdge = C(1f, .70f, .55f),
                CloudCover = .28f, AmbientSky = C(.40f, .44f, .60f), AmbientGround = C(.15f, .13f, .10f), Shade = C(.86f, .90f, 1.12f),
                ShadeSaturation = .4f, Rim = C(.20f, .22f, .32f), AirDistance = 3400, AirMax = .8f, HazeHeight = 360,
                CloudShadowCover = .3f, CloudShadowDarkness = .4f,
                GradeShadow = C(.42f, .34f, .78f), GradeShadowAmount = .06f, GradeLight = C(1f, .76f, .52f), GradeLightAmount = .12f, GradeLift = .95f,
                Light = C(1f, .74f, .50f), LightIntensity = 1.9f, Stars = 0, HouseLights = .25f, Exposure = 1
            };
            // Day: a cerulean sky over a pale cyan horizon, white cumulus with lilac-blue shade,
            // bright sky light in every shadow, and far ranges dissolving into the horizon's blue.
            var day = new Palette
            {
                Hour = 9, Zenith = C(.16f, .57f, .88f), Horizon = C(.66f, .84f, .90f), Glow = C(1f, .97f, .88f), GlowStrength = .1f,
                Below = C(.16f, .19f, .15f), CloudLit = C(1f, .98f, .93f), CloudShade = C(.50f, .57f, .76f), CloudEdge = C(1f, .98f, .92f),
                CloudCover = .3f, AmbientSky = C(.55f, .68f, .85f), AmbientGround = C(.22f, .24f, .15f), Shade = C(.84f, .94f, 1.12f),
                ShadeSaturation = .45f, Rim = C(.16f, .22f, .30f), AirDistance = 4200, AirMax = .82f, HazeHeight = 420,
                CloudShadowCover = .32f, CloudShadowDarkness = .55f,
                GradeShadow = C(.30f, .42f, .86f), GradeShadowAmount = .04f, GradeLight = C(1f, .94f, .76f), GradeLightAmount = .06f, GradeLift = .94f,
                Light = C(1f, .95f, .84f), LightIntensity = 2.4f, Stars = 0, HouseLights = 0, Exposure = 1.05f
            };
            var noon = day; noon.Hour = 13;
            var golden = sunrise;
            golden.Hour = 17; golden.Horizon = C(1f, .80f, .60f); golden.Zenith = C(.18f, .40f, .80f); golden.GlowStrength = .7f; golden.Light = C(1f, .78f, .50f); golden.LightIntensity = 2.3f;
            golden.HouseLights = .15f; golden.CloudLit = C(1f, .86f, .70f); golden.CloudShade = C(.56f, .52f, .68f); golden.AmbientSky = C(.46f, .52f, .70f);
            golden.AirDistance = 3800; golden.CloudShadowCover = .3f; golden.CloudShadowDarkness = .5f;
            var dusk = new Palette
            {
                Hour = 18.6f, Zenith = C(.27f, .27f, .58f), Horizon = C(.96f, .56f, .46f), Glow = C(1f, .46f, .30f), GlowStrength = 1f,
                Below = C(.08f, .08f, .11f), CloudLit = C(.98f, .62f, .56f), CloudShade = C(.30f, .28f, .52f), CloudEdge = C(1f, .56f, .40f),
                CloudCover = .32f, AmbientSky = C(.24f, .27f, .46f), AmbientGround = C(.08f, .09f, .10f), Shade = C(.84f, .90f, 1.14f),
                ShadeSaturation = .4f, Rim = C(.24f, .22f, .40f), AirDistance = 3200, AirMax = .8f, HazeHeight = 340,
                CloudShadowCover = .2f, CloudShadowDarkness = .25f,
                GradeShadow = C(.30f, .24f, .62f), GradeShadowAmount = .06f, GradeLight = C(1f, .62f, .48f), GradeLightAmount = .1f, GradeLift = 1,
                Light = C(1f, .56f, .36f), LightIntensity = .9f, Stars = .12f, HouseLights = .8f, Exposure = 1.05f
            };
            var evening = blueHour; evening.Hour = 19.7f; evening.HouseLights = 1;
            var lateNight = night; lateNight.Hour = 21;
            var midnight = night; midnight.Hour = 24;
            return new[] { night, blueHour, sunrise, day, noon, golden, dusk, evening, lateNight, midnight };
        }

        private void OnEnable() => Apply();
        private void OnValidate() => Apply();

        private void Update()
        {
            if (Application.isPlaying && minutesPerSecond > 0) hour = Mathf.Repeat(hour + minutesPerSecond / 60 * Time.deltaTime, 24);
            Apply();
        }

        private Palette Sample(float h)
        {
            if (palettes == null || palettes.Length == 0) palettes = DefaultPalettes();
            int next = 0;
            while (next < palettes.Length && palettes[next].Hour < h) next++;
            if (next == 0) return palettes[0];
            if (next == palettes.Length) return palettes[^1];
            var a = palettes[next - 1];
            var b = palettes[next];
            float t = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a.Hour, b.Hour, h));
            return Blend(a, b, t);
        }

        private static Palette Blend(Palette a, Palette b, float t)
        {
            Color L(Color x, Color y) => Color.Lerp(x, y, t);
            float F(float x, float y) => Mathf.Lerp(x, y, t);
            return new Palette
            {
                Zenith = L(a.Zenith, b.Zenith), Horizon = L(a.Horizon, b.Horizon), Glow = L(a.Glow, b.Glow), Below = L(a.Below, b.Below),
                GlowStrength = F(a.GlowStrength, b.GlowStrength), CloudLit = L(a.CloudLit, b.CloudLit), CloudShade = L(a.CloudShade, b.CloudShade),
                CloudEdge = L(a.CloudEdge, b.CloudEdge), CloudCover = F(a.CloudCover, b.CloudCover), AmbientSky = L(a.AmbientSky, b.AmbientSky),
                AmbientGround = L(a.AmbientGround, b.AmbientGround), Shade = L(a.Shade, b.Shade), ShadeSaturation = F(a.ShadeSaturation, b.ShadeSaturation),
                Rim = L(a.Rim, b.Rim), AirDistance = F(a.AirDistance, b.AirDistance), AirMax = F(a.AirMax, b.AirMax), HazeHeight = F(a.HazeHeight, b.HazeHeight),
                CloudShadowCover = F(a.CloudShadowCover, b.CloudShadowCover), CloudShadowDarkness = F(a.CloudShadowDarkness, b.CloudShadowDarkness),
                GradeShadow = L(a.GradeShadow, b.GradeShadow), GradeLight = L(a.GradeLight, b.GradeLight), GradeShadowAmount = F(a.GradeShadowAmount, b.GradeShadowAmount),
                GradeLightAmount = F(a.GradeLightAmount, b.GradeLightAmount), GradeLift = F(a.GradeLift, b.GradeLift),
                Light = L(a.Light, b.Light), LightIntensity = F(a.LightIntensity, b.LightIntensity), Stars = F(a.Stars, b.Stars),
                HouseLights = F(a.HouseLights, b.HouseLights), Exposure = F(a.Exposure, b.Exposure)
            };
        }

        public Vector3 SunDirection(float h)
        {
            float elevation = maxSunElevation * Mathf.Sin(Mathf.PI * (h - 6) / 12);
            float azimuth = 90 + (h - 6) * 15;
            return Quaternion.Euler(-elevation, azimuth, 0) * Vector3.forward;
        }

        // Rises as the sun sets and crosses the night sky.
        public Vector3 MoonDirection(float h) => Quaternion.Euler(0, moonOffset, 0) * SunDirection(h + 12);

        // Only at night: the moon fades out as the sun rises and below its own horizon.
        public float MoonVisibility(float h)
            => Mathf.InverseLerp(.1f, -.1f, SunDirection(h).y) * Mathf.InverseLerp(-.04f, .1f, MoonDirection(h).y);

        private void Apply()
        {
            var p = Sample(hour);
            var sun = SunDirection(hour);
            var moon = MoonDirection(hour);
            float sunUp = Mathf.InverseLerp(-.06f, .05f, sun.y);
            // Daylight comes from the sun; once it has set, the moon lights the night.
            var lightDirection = sunUp > 0 ? sun : moon;
            float handover = sunUp > 0 ? sunUp : Mathf.InverseLerp(-.06f, -.2f, sun.y);

            Shader.SetGlobalVector("_WG_Sky", new Vector4(p.AmbientSky.r, p.AmbientSky.g, p.AmbientSky.b, isActiveAndEnabled ? 1 : 0));
            Shader.SetGlobalVector("_WG_Ground", p.AmbientGround);
            Shader.SetGlobalVector("_WG_Shade", new Vector4(p.Shade.r, p.Shade.g, p.Shade.b, p.ShadeSaturation));
            Shader.SetGlobalVector("_WG_Rim", new Vector4(p.Rim.r, p.Rim.g, p.Rim.b, 3));
            // The air: clear for the first 15 m, then thickening with distance towards the sky's
            // own horizon colour, thinner with altitude above the water.
            Shader.SetGlobalVector("_WG_FogShape", new Vector4(15, p.AirMax, 0, 6));
            Shader.SetGlobalVector("_WG_Air", new Vector4(Mathf.Max(1, p.AirDistance), p.AirMax, Mathf.Max(1, p.HazeHeight), airBase));
            // Cloud shadows only while the sun is up; the moon's are too faint to paint.
            Shader.SetGlobalVector("_WG_CloudShadow", new Vector4(p.CloudShadowCover, p.CloudShadowDarkness * sunUp, cloudShadowSize, cloudShadowDrift));
            Shader.SetGlobalVector("_WG_Wind", new Vector4(wind.x, wind.y, wind.z, gustSpeed));
            Shader.SetGlobalVector("_WG_LightPool", new Vector4(.75f, 1, 0, 0));

            // Sky values go out as globals. Colours are converted from sRGB here, as Material.SetColor would,
            // so neither the sky nor the glow material asset changes as time passes.
            Shader.SetGlobalColor("_WG_SkyZenith", (p.Zenith).linear);
            Shader.SetGlobalColor("_WG_SkyHorizon", (p.Horizon).linear);
            Shader.SetGlobalColor("_WG_SkyGlow", (p.Glow).linear);
            Shader.SetGlobalFloat("_WG_SkyGlowStrength", p.GlowStrength);
            Shader.SetGlobalColor("_WG_SkyBelow", (p.Below).linear);
            Shader.SetGlobalVector("_WG_SunDirection", sun);
            Shader.SetGlobalColor("_WG_SunColor", (p.Light * Mathf.Lerp(.2f, 1, sunUp)).linear);
            Shader.SetGlobalVector("_WG_MoonDirection", moon);
            Shader.SetGlobalFloat("_WG_MoonVisibility", MoonVisibility(hour));
            Shader.SetGlobalFloat("_WG_Stars", p.Stars);
            Shader.SetGlobalFloat("_WG_Galaxy", p.Stars * .45f);
            Shader.SetGlobalColor("_WG_CloudLit", (p.CloudLit).linear);
            Shader.SetGlobalColor("_WG_CloudShade", (p.CloudShade).linear);
            Shader.SetGlobalColor("_WG_CloudEdge", (p.CloudEdge).linear);
            Shader.SetGlobalFloat("_WG_CloudCover", p.CloudCover);
            Shader.SetGlobalFloat("_WG_SkyExposure", p.Exposure);
            // The grade works on the displayed image, so its hues stay as a painter picks them (sRGB).
            Shader.SetGlobalVector("_WG_GradeShadow", new Vector4(p.GradeShadow.r, p.GradeShadow.g, p.GradeShadow.b, p.GradeShadowAmount));
            Shader.SetGlobalVector("_WG_GradeLight", new Vector4(p.GradeLight.r, p.GradeLight.g, p.GradeLight.b, p.GradeLightAmount));
            Shader.SetGlobalFloat("_WG_GradeLift", p.GradeLift > 0 ? p.GradeLift : 1);
            if (sky != null && RenderSettings.skybox != sky) RenderSettings.skybox = sky;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = p.AmbientSky;
            RenderSettings.ambientEquatorColor = Color.Lerp(p.AmbientSky, p.AmbientGround, .5f);
            RenderSettings.ambientGroundColor = p.AmbientGround;
            // For any shader outside the look: plain fog towards the horizon colour.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = p.Horizon * p.Exposure;
            RenderSettings.fogDensity = 1 / Mathf.Max(1, p.AirDistance);

            if (sunAndMoon != null)
            {
                sunAndMoon.transform.rotation = Quaternion.LookRotation(-lightDirection);
                sunAndMoon.color = p.Light;
                sunAndMoon.intensity = p.LightIntensity * Mathf.SmoothStep(0, 1, handover);
                sunAndMoon.shadowStrength = sunUp > 0 ? .9f : .7f;
            }
            for (int i = 0; i < houseLights.Length; i++)
            {
                if (houseLights[i] == null) continue;
                float baseIntensity = i < houseLightIntensity.Length ? houseLightIntensity[i] : 1;
                houseLights[i].intensity = baseIntensity * p.HouseLights;
                houseLights[i].enabled = p.HouseLights > .01f;
            }
            // The glow material keeps a fixed emission (GlowEmission); the time of day only scales it.
            // The 2.2 power matches what scaling the colour before its sRGB conversion used to do.
            Shader.SetGlobalFloat("_WG_GlowScale", Mathf.Pow(Mathf.Max(.08f, p.HouseLights), 2.2f));
        }

        private void OnDisable() => Shader.SetGlobalVector("_WG_Sky", Vector4.zero);
    }
}
