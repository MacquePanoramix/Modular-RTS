using UnityEngine;

namespace WonderGather
{
    // S3, step 11 (Docs/Design/ThePhysicalBody.md): the plain panel of the Ordinary Place, for trying the chosen
    // miner's physical work. How strong it is (a slider), and a pickaxe put on the ground beside it: a light one, its
    // own, or a heavy one. Nothing is put in its hands: it picks a pickaxe up itself, by the interaction click, and
    // mines a boulder by the same click. The panel also says, in a line, what the miner is doing. And it can push the
    // miner over, to see it fall and get itself up; and (S3b) have it stand by its own joints, and nudge it.
    //
    // It takes the place of the keys of the look at the work (K, and the keys for strength and weight).
    [RequireComponent(typeof(MinerChoice), typeof(MinerWorkPreview))]
    public sealed class MinerPanel : MonoBehaviour
    {
        // The pickaxes it offers, as shares of the miner's own pickaxe's weight, and their names.
        public static readonly float[] Weights = { .6f, 1, 1.8f };
        public static readonly string[] Names = { "Light", "Its own", "Heavy" };
        // The least and the most strength the slider gives (1: ordinary for its build), and the step it moves by.
        public const float Weakest = .3f, Strongest = 3, Step = .05f;
        // Its size on the screen, and how far it stands from the screen's lower left corner.
        private const float Width = 400, Height = 228, Margin = 12;

        private MinerChoice choice;
        private MinerWorkPreview look;
        private GUIStyle box, title, text, small, button, toggle;

        // It is shown while a miner is chosen and the choice of miner is closed.
        public bool Shown => isActiveAndEnabled && choice != null && !choice.Open && choice.Count > 0 && choice.Current != null;
        private static Rect Area => new Rect(Margin, Screen.height - Height - Margin, Width, Height);
        // Whether a point of the screen (from its lower left) is on the panel: a click there is the panel's.
        public bool Over(Vector2 screen) => Shown && Area.Contains(new Vector2(screen.x, Screen.height - screen.y));

        private void Awake()
        {
            choice = GetComponent<MinerChoice>();
            look = GetComponent<MinerWorkPreview>();
        }

        private void Styles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            title = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            text = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            small = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true };
            button = new GUIStyle(GUI.skin.button) { fontSize = 11, fixedHeight = 24 };
            toggle = new GUIStyle(GUI.skin.toggle) { fontSize = 12 };
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !Shown) return;
            Styles();
            var area = Area;
            GUI.Box(area, GUIContent.none, box);
            float x = area.x + 10, y = area.y + 8, inner = Width - 20;
            GUI.Label(new Rect(x, y, inner, 20), choice.NameOf(choice.Chosen), title);
            // Pushed over, the way the view looks.
            bool could = GUI.enabled;
            GUI.enabled = could && look.CanPushOver;
            if (GUI.Button(new Rect(x + inner - 96, y - 2, 96, 22), "Push it over", button)) look.PushOver();
            GUI.enabled = could;
            y += 22;
            // How strong it is.
            GUI.Label(new Rect(x, y, 60, 20), "Strength", text);
            float chosen = GUI.HorizontalSlider(new Rect(x + 64, y + 5, inner - 64 - 96, 16), look.Strength, Weakest, Strongest);
            chosen = Mathf.Round(chosen / Step) * Step;
            if (Mathf.Abs(chosen - look.Strength) > Step * .5f) look.SetStrength(chosen);
            GUI.Label(new Rect(x + inner - 88, y, 36, 20), look.Strength.ToString("0.00"), text);
            if (GUI.Button(new Rect(x + inner - 48, y - 1, 48, 22), 1f.ToString("0.00"), button)) look.SetStrength(1);
            y += 26;
            // Standing by its own joints (S3b, step 1): the switch, and a nudge the way the view looks.
            bool own = GUI.Toggle(new Rect(x, y, inner - 104, 20), look.StandsByItsOwn, " Stands by its own joints (at ease, hands empty)", toggle);
            if (own != look.StandsByItsOwn) look.SetOwn(own);
            GUI.enabled = could && look.CanNudge;
            if (GUI.Button(new Rect(x + inner - 96, y - 2, 96, 22), "Nudge it", button)) look.Nudge();
            GUI.enabled = could;
            y += 26;
            // Its breath: how it is drawn (a look to choose between), and a miner tired at once to see it breathe so.
            GUI.Label(new Rect(x, y + 2, 50, 20), "Breath:", text);
            if (GUI.Button(new Rect(x + 52, y - 1, 130, 22), look.BreathLook, button)) look.NextBreathLook();
            if (GUI.Button(new Rect(x + inner - 96, y - 1, 96, 22), "Tire it", button)) look.Tire();
            y += 26;
            // A pickaxe put on the ground beside it.
            GUI.Label(new Rect(x, y, inner, 20), "Put a pickaxe on the ground beside it:", text);
            y += 20;
            const float away = 84, gap = 6;
            float each = (inner - away - gap * Weights.Length) / Weights.Length;
            for (int i = 0; i < Weights.Length; i++)
                if (GUI.Button(new Rect(x + i * (each + gap), y, each, 24), $"{Names[i]}  {look.PickaxeWeighs(Weights[i]):0.0} kg", button)) look.LayPickaxe(Weights[i]);
            if (GUI.Button(new Rect(x + inner - away, y, away, 24), "Take away", button)) look.ClearLaid();
            y += 30;
            GUI.Label(new Rect(x, y, inner, 32), look.Status(), small);
            y += 34;
            GUI.Label(new Rect(x, y, inner, 18), "Hold Space and click: a pickaxe, a boulder, the miner, its lantern or its mug", small);
        }
    }
}
