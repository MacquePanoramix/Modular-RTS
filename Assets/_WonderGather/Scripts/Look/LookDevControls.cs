using UnityEngine;
using UnityEngine.InputSystem;

namespace WonderGather
{
    // The Ordinary Place's look-test controls: switch rendering candidates live, move through
    // the day, and read the frame cost. Owns its own small action map, like RtsInput.
    [ExecuteAlways]
    public sealed class LookDevControls : MonoBehaviour
    {
        public struct Candidate
        {
            public string Name;
            public Vector4 Paint;   // variation, brush break, brush scale, baseline flag
            public float Filter, Ink, Grain;
        }

        public static readonly Candidate[] Candidates =
        {
            new Candidate { Name = "A  Baseline (plain light, for comparison)", Paint = new Vector4(0, 0, 3, 1) },
            new Candidate { Name = "B  Painted light", Paint = new Vector4(1, .5f, 3, 0), Grain = .5f },
            new Candidate { Name = "C  Painted light + paint filter", Paint = new Vector4(1, .5f, 3, 0), Filter = 1, Grain = .5f },
            new Candidate { Name = "D  Painted light + ink contours", Paint = new Vector4(1, .5f, 3, 0), Ink = 1, Grain = .5f },
            new Candidate { Name = "E  Painted light + paint filter + ink", Paint = new Vector4(1, .5f, 3, 0), Filter = .8f, Ink = .8f, Grain = .6f },
        };

        private static readonly (string Name, float Hour)[] Times =
        {
            ("Morning", 8.5f), ("Midday", 13), ("Golden hour", 17), ("Dusk", 18.6f), ("Blue hour", 19.7f), ("Night", 23),
        };

        [SerializeField] private TimeOfDay time;
        [SerializeField] private LookPostEffects post;
        [SerializeField] private GrassField grass;
        [SerializeField] private int candidate = 1;
        [SerializeField] private bool showPanel = true;
        private InputActionMap map;
        private InputAction next, scrub, lapse, hide;
        private readonly InputAction[] presets = new InputAction[6];
        private float smoothedFrame = 16, timeLapse;
        private readonly FrameTiming[] timings = new FrameTiming[1];
        private double gpuMilliseconds;

        public int CandidateIndex => candidate;

        public void Configure(TimeOfDay day, LookPostEffects effects, GrassField field)
        {
            time = day;
            post = effects;
            grass = field;
            Apply();
        }

        public void Select(int index)
        {
            candidate = (index % Candidates.Length + Candidates.Length) % Candidates.Length;
            Apply();
        }

        private void Apply()
        {
            var c = Candidates[Mathf.Clamp(candidate, 0, Candidates.Length - 1)];
            Shader.SetGlobalVector("_WG_Paint", c.Paint);
            if (post == null) return;
            post.Paint = c.Filter;
            post.Ink = c.Ink;
            post.Grain = c.Grain;
        }

        private void OnEnable()
        {
            Apply();
            if (!Application.isPlaying) return;
            map = new InputActionMap("Look test");
            next = map.AddAction("Next look", InputActionType.Button, "<Keyboard>/p");
            scrub = map.AddAction("Scrub time", InputActionType.Value);
            scrub.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftBracket").With("Positive", "<Keyboard>/rightBracket");
            lapse = map.AddAction("Time-lapse", InputActionType.Button, "<Keyboard>/l");
            hide = map.AddAction("Hide panel", InputActionType.Button, "<Keyboard>/h");
            string[] keys = { "1", "2", "3", "4", "5", "6" };
            for (int i = 0; i < presets.Length; i++) presets[i] = map.AddAction("Time " + keys[i], InputActionType.Button, "<Keyboard>/" + keys[i]);
            map.Enable();
        }

        private void OnDisable()
        {
            map?.Dispose();
            map = null;
        }

        private void OnValidate() => Apply();

        private void Update()
        {
            Apply();
            if (!Application.isPlaying || map == null || !Application.isFocused) return;
            if (next.WasPressedThisFrame()) Select(candidate + 1);
            if (hide.WasPressedThisFrame()) showPanel = !showPanel;
            if (time != null)
            {
                for (int i = 0; i < presets.Length; i++)
                    if (presets[i].WasPressedThisFrame()) { time.Hour = Times[i].Hour; timeLapse = 0; }
                float s = scrub.ReadValue<float>();
                if (Mathf.Abs(s) > .01f) time.Hour += s * 2 * Time.unscaledDeltaTime;
                if (lapse.WasPressedThisFrame()) timeLapse = timeLapse > 0 ? 0 : 30;
                time.MinutesPerSecond = timeLapse;
            }
            smoothedFrame = Mathf.Lerp(smoothedFrame, Time.unscaledDeltaTime * 1000, .05f);
            FrameTimingManager.CaptureFrameTimings();
            if (FrameTimingManager.GetLatestTimings(1, timings) > 0 && timings[0].gpuFrameTime > 0)
                gpuMilliseconds = gpuMilliseconds * .95 + timings[0].gpuFrameTime * .05;
        }

        private static string Clock(float hour)
        {
            int minutes = Mathf.FloorToInt(Mathf.Repeat(hour, 24) * 60);
            return $"{minutes / 60:00}:{minutes % 60:00}";
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !showPanel) return;
            var c = Candidates[candidate];
            string hour = time != null ? Clock(time.Hour) : "--";
            string gpu = gpuMilliseconds > 0 ? $"   GPU {gpuMilliseconds:F1} ms" : "";
            string text = "THE ORDINARY PLACE  ·  look test\n" +
                          $"Look: {c.Name}   (P: next)\n" +
                          $"Time: {hour}   (1–6: morning, midday, golden, dusk, blue hour, night;  [ ]: scrub;  L: time-lapse{(timeLapse > 0 ? " on" : "")})\n" +
                          $"Frame {smoothedFrame:F1} ms ({1000 / Mathf.Max(smoothedFrame, .01f):F0} fps){gpu}" +
                          (grass != null ? $"   grass drawn {grass.DrawnLastFrame / 1000}k of {grass.BladeCount / 1000}k" : "") +
                          "\nH: hide this panel";
            var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, wordWrap = true, fontSize = 13, padding = new RectOffset(10, 10, 8, 8) };
            float width = Mathf.Min(560, Screen.width - 36);
            float height = style.CalcHeight(new GUIContent(text), width);
            GUI.Box(new Rect(Screen.width - width - 18, 18, width, height), text, style);
        }
    }
}
