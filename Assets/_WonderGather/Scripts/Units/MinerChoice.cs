using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace WonderGather
{
    // The choice of miner for this prototype (Luis, October 3): Small, Long or Round. One stands in the world at a
    // time; choosing another puts it where the last one stood, selected if the last one was. The choice opens when
    // the place loads and with M; this viewer's last choice is remembered.
    public sealed class MinerChoice : MonoBehaviour
    {
        private const string Remembered = "WonderGather.Miner";
        [SerializeField] private SelectableUnit[] miners;
        [SerializeField] private string[] names;
        [SerializeField] private string[] notes;
        [SerializeField] private SelectionController selection;
        [SerializeField] private int chosen;
        [SerializeField] private bool openOnStart = true;
        private bool open;
        private InputAction toggle;
        private GUIStyle panel, title, button, chosenButton, note;

        public SelectableUnit Current => miners != null && miners.Length > 0 ? miners[chosen] : null;
        public int Chosen => chosen;
        public int Count => miners != null ? miners.Length : 0;
        public bool Open => open;
        public string NameOf(int index) => names[index];

        public void Configure(SelectableUnit[] units, string[] labels, string[] descriptions, SelectionController controller, int initial)
        {
            miners = units;
            names = labels;
            notes = descriptions;
            selection = controller;
            chosen = Mathf.Clamp(initial, 0, units.Length - 1);
            for (int i = 0; i < miners.Length; i++) miners[i].gameObject.SetActive(i == chosen);
            if (selection != null) selection.ConfigureUnits(new[] { miners[chosen] });
        }

        private void Awake()
        {
            int remembered = chosen;
            try { remembered = PlayerPrefs.GetInt(Remembered, chosen); } catch (System.Exception) { }
            if (miners != null && remembered >= 0 && remembered < miners.Length && remembered != chosen) Choose(remembered, false);
            open = openOnStart;
            toggle = new InputAction("Choose miner", InputActionType.Button, "<Keyboard>/m");
        }

        private void OnEnable() => toggle?.Enable();
        private void OnDisable() => toggle?.Disable();
        private void OnDestroy() => toggle?.Dispose();

        private void Update()
        {
            if (toggle != null && toggle.WasPressedThisFrame()) open = !open;
        }

        public void Choose(int index) => Choose(index, true);

        private void Choose(int index, bool remember)
        {
            if (miners == null || index < 0 || index >= miners.Length) return;
            var from = miners[chosen];
            var to = miners[index];
            bool wasSelected = selection != null && from != null && selection.SelectedUnits.Contains(from);
            if (index != chosen && from != null && to != null)
            {
                var position = from.transform.position;
                var rotation = from.transform.rotation;
                from.gameObject.SetActive(false);
                to.transform.SetPositionAndRotation(position, rotation);
                to.gameObject.SetActive(true);
                if (to.TryGetComponent<NavMeshAgent>(out var agent) && agent.isOnNavMesh) agent.Warp(position);
            }
            else if (to != null) to.gameObject.SetActive(true);
            chosen = index;
            if (selection != null)
            {
                selection.ConfigureUnits(new[] { to });
                if (wasSelected) selection.Select(to);
            }
            if (remember)
            {
                try { PlayerPrefs.SetInt(Remembered, index); PlayerPrefs.Save(); } catch (System.Exception) { }
            }
        }

        private void Styles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box) { padding = new RectOffset(14, 14, 10, 12) };
            title = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            button = new GUIStyle(GUI.skin.button) { fontSize = 15, fixedHeight = 34 };
            chosenButton = new GUIStyle(button) { fontStyle = FontStyle.Bold };
            chosenButton.normal = chosenButton.active;
            note = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, alignment = TextAnchor.UpperCenter };
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || miners == null || miners.Length == 0) return;
            Styles();
            if (!open)
            {
                GUI.Label(new Rect(Screen.width * .5f - 120, Screen.height - 30, 240, 24), $"{names[chosen]}   ·   M: choose your miner", note);
                return;
            }
            float column = 150, width = column * miners.Length + 28 + 10 * (miners.Length - 1), height = 118;
            var area = new Rect((Screen.width - width) * .5f, Screen.height - height - 18, width, height);
            GUI.Box(area, GUIContent.none, panel);
            GUI.Label(new Rect(area.x, area.y + 8, area.width, 22), "Choose your miner", title);
            for (int i = 0; i < miners.Length; i++)
            {
                var cell = new Rect(area.x + 14 + i * (column + 10), area.y + 36, column, 34);
                if (GUI.Button(cell, names[i], i == chosen ? chosenButton : button)) { Choose(i); open = false; }
                if (notes != null && i < notes.Length) GUI.Label(new Rect(cell.x, cell.yMax + 4, column, 40), notes[i], note);
            }
        }
    }
}
