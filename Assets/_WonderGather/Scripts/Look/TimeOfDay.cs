using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace WonderGather
{
    // Drives the Ordinary Place's light from one palette: sun or moon, the painted sky,
    // ambient and shade tints, aerial fog, and the warmth of the house's lights.
    // Keyframes follow the Visual Soul references: C's daylight, golden hour, E's lilac
    // dusk and A's deep navy night with warm windows.
    [ExecuteAlways]
    public sealed class TimeOfDay : MonoBehaviour
    {
        // Sky, light and glow colours are given as painters pick them (sRGB). Ambient, shade,
        // rim and fog colours go straight to the shaders as linear values.
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
            public Color Rim, Fog, FogSun;
            public float FogDensity;
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
        [SerializeField] private float maxSunElevation = 52, moonElevation = 38, moonAzimuth = 145;
        [SerializeField] private Vector3 wind = new Vector3(.8f, .55f, .55f);
        [SerializeField] private float gustSpeed = .8f;
        [SerializeField] private Palette[] palettes = DefaultPalettes();

        public float Hour { get => hour; set { hour = Mathf.Repeat(value, 24); Apply(); } }
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
                ShadeSaturation = .4f, Rim = C(.10f, .15f, .34f), Fog = C(.022f, .038f, .10f), FogSun = C(.04f, .05f, .12f), FogDensity = .0045f,
                Light = C(.46f, .56f, .95f), LightIntensity = .32f, Stars = 1, HouseLights = 1, Exposure = 1.15f
            };
            var blueHour = night;
            blueHour.Hour = 5.6f; blueHour.Zenith = C(.18f, .24f, .48f); blueHour.Horizon = C(.52f, .48f, .64f); blueHour.Glow = C(.85f, .50f, .52f);
            blueHour.GlowStrength = .6f; blueHour.AmbientSky = C(.16f, .19f, .34f); blueHour.Fog = C(.30f, .30f, .45f); blueHour.FogSun = C(.75f, .52f, .55f);
            blueHour.CloudLit = C(.70f, .55f, .65f); blueHour.CloudShade = C(.20f, .22f, .38f); blueHour.Stars = .25f; blueHour.HouseLights = .8f;
            blueHour.Light = C(.75f, .62f, .78f); blueHour.LightIntensity = .25f;
            var sunrise = new Palette
            {
                Hour = 6.6f, Zenith = C(.30f, .45f, .74f), Horizon = C(.98f, .78f, .62f), Glow = C(1f, .62f, .42f), GlowStrength = .8f,
                Below = C(.12f, .13f, .12f), CloudLit = C(1f, .82f, .68f), CloudShade = C(.52f, .52f, .66f), CloudEdge = C(1f, .70f, .55f),
                CloudCover = .44f, AmbientSky = C(.40f, .44f, .60f), AmbientGround = C(.15f, .13f, .10f), Shade = C(.86f, .90f, 1.12f),
                ShadeSaturation = .35f, Rim = C(.20f, .22f, .32f), Fog = C(.78f, .72f, .72f), FogSun = C(1f, .78f, .58f), FogDensity = .0045f,
                Light = C(1f, .74f, .50f), LightIntensity = 1.9f, Stars = 0, HouseLights = .25f, Exposure = 1
            };
            var day = new Palette
            {
                Hour = 9, Zenith = C(.26f, .48f, .84f), Horizon = C(.80f, .87f, .90f), Glow = C(1f, .94f, .82f), GlowStrength = .12f,
                Below = C(.16f, .19f, .15f), CloudLit = C(1f, .98f, .94f), CloudShade = C(.60f, .67f, .78f), CloudEdge = C(1f, .97f, .90f),
                CloudCover = .46f, AmbientSky = C(.44f, .54f, .72f), AmbientGround = C(.15f, .18f, .13f), Shade = C(.82f, .93f, 1.14f),
                ShadeSaturation = .3f, Rim = C(.16f, .22f, .30f), Fog = C(.72f, .81f, .88f), FogSun = C(.96f, .93f, .84f), FogDensity = .003f,
                Light = C(1f, .93f, .80f), LightIntensity = 2.1f, Stars = 0, HouseLights = 0, Exposure = 1
            };
            var noon = day; noon.Hour = 13;
            var golden = sunrise;
            golden.Hour = 17; golden.Horizon = C(1f, .80f, .58f); golden.Zenith = C(.32f, .48f, .78f); golden.Light = C(1f, .78f, .50f); golden.LightIntensity = 2.2f;
            golden.HouseLights = .15f;
            var dusk = new Palette
            {
                Hour = 18.6f, Zenith = C(.27f, .27f, .58f), Horizon = C(.96f, .56f, .46f), Glow = C(1f, .46f, .30f), GlowStrength = 1f,
                Below = C(.08f, .08f, .11f), CloudLit = C(.98f, .62f, .56f), CloudShade = C(.30f, .28f, .52f), CloudEdge = C(1f, .56f, .40f),
                CloudCover = .5f, AmbientSky = C(.24f, .27f, .46f), AmbientGround = C(.08f, .09f, .10f), Shade = C(.84f, .90f, 1.14f),
                ShadeSaturation = .4f, Rim = C(.24f, .22f, .40f), Fog = C(.46f, .42f, .60f), FogSun = C(.98f, .60f, .44f), FogDensity = .004f,
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
                Rim = L(a.Rim, b.Rim), Fog = L(a.Fog, b.Fog), FogSun = L(a.FogSun, b.FogSun), FogDensity = F(a.FogDensity, b.FogDensity),
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

        public Vector3 MoonDirection => Quaternion.Euler(-moonElevation, moonAzimuth, 0) * Vector3.forward;

        private void Apply()
        {
            var p = Sample(hour);
            var sun = SunDirection(hour);
            var moon = MoonDirection;
            float sunUp = Mathf.InverseLerp(-.06f, .05f, sun.y);
            // Daylight comes from the sun; once it has set, the moon lights the night.
            var lightDirection = sunUp > 0 ? sun : moon;
            float handover = sunUp > 0 ? sunUp : Mathf.InverseLerp(-.06f, -.2f, sun.y);

            Shader.SetGlobalVector("_WG_Sky", new Vector4(p.AmbientSky.r, p.AmbientSky.g, p.AmbientSky.b, isActiveAndEnabled ? 1 : 0));
            Shader.SetGlobalVector("_WG_Ground", p.AmbientGround);
            Shader.SetGlobalVector("_WG_Shade", new Vector4(p.Shade.r, p.Shade.g, p.Shade.b, p.ShadeSaturation));
            Shader.SetGlobalVector("_WG_Rim", new Vector4(p.Rim.r, p.Rim.g, p.Rim.b, 3));
            Shader.SetGlobalVector("_WG_Fog", new Vector4(p.Fog.r, p.Fog.g, p.Fog.b, p.FogDensity));
            Shader.SetGlobalVector("_WG_FogSun", new Vector4(p.FogSun.r, p.FogSun.g, p.FogSun.b, .045f));
            Shader.SetGlobalVector("_WG_FogShape", new Vector4(15, .86f, 0, 6));
            Shader.SetGlobalVector("_WG_Wind", new Vector4(wind.x, wind.y, wind.z, gustSpeed));
            Shader.SetGlobalVector("_WG_LightPool", new Vector4(.75f, 1, 0, 0));

            if (sky != null)
            {
                sky.SetColor("_Zenith", p.Zenith);
                sky.SetColor("_Horizon", p.Horizon);
                sky.SetColor("_Glow", p.Glow);
                sky.SetFloat("_GlowStrength", p.GlowStrength);
                sky.SetColor("_Below", p.Below);
                sky.SetVector("_SunDirection", sun);
                sky.SetColor("_SunColor", p.Light * Mathf.Lerp(.2f, 1, sunUp));
                sky.SetVector("_MoonDirection", moon);
                sky.SetFloat("_Stars", p.Stars);
                sky.SetFloat("_Galaxy", p.Stars * .45f);
                sky.SetColor("_CloudLit", p.CloudLit);
                sky.SetColor("_CloudShade", p.CloudShade);
                sky.SetColor("_CloudEdge", p.CloudEdge);
                sky.SetFloat("_CloudCover", p.CloudCover);
                sky.SetFloat("_Exposure", p.Exposure);
                RenderSettings.skybox = sky;
            }
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = p.AmbientSky;
            RenderSettings.ambientEquatorColor = Color.Lerp(p.AmbientSky, p.AmbientGround, .5f);
            RenderSettings.ambientGroundColor = p.AmbientGround;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = p.Fog;
            RenderSettings.fogDensity = p.FogDensity;

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
            if (glow != null) glow.SetColor("_EmissionColor", glowColor * (glowIntensity * Mathf.Max(.08f, p.HouseLights)));
        }

        private void OnDisable() => Shader.SetGlobalVector("_WG_Sky", Vector4.zero);
    }
}
