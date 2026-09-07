using UnityEngine;

namespace WonderGather
{
    public sealed class WandererHud : MonoBehaviour
    {
        [SerializeField] private SelectionController selection;
        [SerializeField] private ResourceDepot depot;
        [SerializeField] private ResourceNode resource;
        public void ConfigureEconomy(ResourceDepot home, ResourceNode node) { depot = home; resource = node; }
        private Rect PanelRect => new Rect(18, 18, 520, depot != null ? 215 : 165);
        public bool ContainsScreenPoint(Vector2 point) => PanelRect.Contains(new Vector2(point.x, Screen.height - point.y));
        public void Configure(SelectionController value) => selection = value;
        private void OnGUI()
        {
            if (selection.IsDragging)
            {
                var rect = selection.DragRect;
                rect.y = Screen.height - rect.yMax;
                var color = GUI.color;
                GUI.color = new Color(.5f, 1, .7f, .2f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = new Color(.5f, 1, .7f, .9f);
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2, rect.width, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(rect.x, rect.y, 2, rect.height), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(rect.xMax - 2, rect.y, 2, rect.height), Texture2D.whiteTexture);
                GUI.color = color;
            }
            GUILayout.BeginArea(PanelRect, GUI.skin.box);
            GUILayout.Label(depot != null ? "WONDER GATHER  /  THE GATHERER" : "WONDER GATHER  /  THE GROUP");
            GUILayout.Label("WASD / Arrows: pan     Q / E: rotate     Wheel: zoom");
            GUILayout.Label("Click / drag: select     Shift: toggle click / add box");
            GUILayout.Label("Right click: move     Esc: clear     F: focus group");
            GUILayout.Label("Selected: " + selection.Count);
            if (depot != null)
            {
                int carried = 0, working = 0;
                foreach (var unit in selection.SelectedUnits)
                    if (unit.TryGetComponent<Gatherer>(out var worker)) { carried += worker.Carried; if (worker.State != Gatherer.Activity.Idle) working++; }
                GUILayout.Label("Supplies stored: " + depot.Stored + "   Remaining: " + (resource != null ? resource.Remaining : 0));
                GUILayout.Label("Selected workers active: " + working + "   Carrying: " + carried);
                GUILayout.Label("Right-click green supplies: gather / blue depot: deliver");
            }
            GUILayout.Space(8);
            GUILayout.Label(selection.Status);
            GUILayout.EndArea();
        }
    }
}
