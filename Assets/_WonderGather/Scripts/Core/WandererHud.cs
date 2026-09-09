using UnityEngine;

namespace WonderGather
{
    public sealed class WandererHud : MonoBehaviour
    {
        [SerializeField] private SelectionController selection;
        [SerializeField] private ResourceDepot depot;
        [SerializeField] private ResourceNode resource;
        public void ConfigureEconomy(ResourceDepot home, ResourceNode node) { depot = home; resource = node; }
        private Rect panelRect = new Rect(18, 18, 520, 260);
        private ConstructionController construction;
        private CivilizationSession civilization;
        private void Awake(){construction=GetComponent<ConstructionController>();civilization=GetComponent<CivilizationSession>();}
        private GUIStyle wrappedLabel;
        private Rect PanelRect => panelRect;
        private void Label(string text) => GUILayout.Label(text, wrappedLabel);
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
            wrappedLabel ??= new GUIStyle(GUI.skin.label) { wordWrap = true };
            float width = Mathf.Max(1, Mathf.Min(520, Screen.width - 36));
            GUILayout.BeginArea(new Rect(18, 18, width, Mathf.Max(1, Screen.height - 36)));
            GUILayout.BeginVertical(GUI.skin.box);
            Label(civilization!=null && civilization.Definition!=null ? "WONDER GATHER / "+civilization.Definition.DisplayName : construction != null ? "WONDER GATHER  /  THE SETTLEMENT" : depot != null ? "WONDER GATHER  /  THE GATHERER" : "WONDER GATHER  /  THE GROUP");
            Label("WASD / Arrows: pan     Q / E: rotate     Wheel: zoom");
            Label("Click / drag: select     Shift: toggle click / add box");
            Label("Right click: move     Esc: clear     F: focus group");
            Label(selection.SelectedBuilding!=null?"Selected: "+selection.SelectedBuilding.Definition.DisplayName:"Selected: " + selection.Count);
            if (depot != null)
            {
                int carried = 0, working = 0;
                foreach (var unit in selection.SelectedUnits)
                    if (unit.TryGetComponent<Gatherer>(out var worker)) { carried += worker.Carried; if (worker.State != Gatherer.Activity.Idle) working++; }
                Label("Supplies stored: " + depot.Stored + "   Remaining: " + (resource != null ? resource.Remaining : 0));
                if(selection.SelectedBuilding==null)
                {
                    Label("Selected gatherers active: " + working + "   Carrying: " + carried);
                    Label("Right-click green supplies: gather / blue depot: deliver");
                }
            }
            if(construction!=null && selection.SelectedBuilding==null)
            {
                Label(construction.BuildHint);
                var choices=construction.AvailableBuildings();
                if(choices.Count>1) foreach(var choice in choices)
                    if(GUILayout.Button("Build "+choice.DisplayName+" ("+choice.Construction.Cost+" supplies)"))
                    {construction.ChooseBuilding(choice);construction.BeginPlacement();}
                if(!string.IsNullOrEmpty(construction.ProgressText)) Label(construction.ProgressText);
            }
            if(selection.SelectedBuilding!=null)
            {
                Label(selection.SelectedBuilding.Complete?selection.SelectedBuilding.Definition.DisplayName+" complete":"Construction: "+Mathf.RoundToInt(selection.SelectedBuilding.Progress*100)+"%");
                var producer=selection.SelectedProducer;
                if(producer!=null)
                {
                    Label(producer.Summary);
                    GUILayout.BeginHorizontal();
                    GUI.enabled=producer.CanTrain;
                    if(GUILayout.Button("Train "+producer.UnitName+" — "+producer.Cost+" supplies (T)")) selection.OrderProduction();
                    GUI.enabled=producer.QueueCount>0;
                    if(GUILayout.Button("Cancel last / refund")) selection.OrderProduction(true);
                    GUI.enabled=true;GUILayout.EndHorizontal();
                }
            }
            GUILayout.Space(8);
            Label(selection.Status);
            GUILayout.EndVertical();
            // Use the content's actual height for both the box and world-input exclusion.
            if (Event.current.type == EventType.Repaint)
                panelRect = new Rect(18, 18, width, GUILayoutUtility.GetLastRect().height);
            GUILayout.EndArea();
        }
    }
}
