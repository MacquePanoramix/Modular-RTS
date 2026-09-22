using UnityEngine;

namespace WonderGather
{
    public sealed class LivingBodyDemo : MonoBehaviour
    {
        [SerializeField] private RtsInput input;
        [SerializeField] private SelectionController selection;
        [SerializeField] private SelectableUnit[] units;
        [SerializeField] private Vector3[] flatTargets,slopeTargets;
        private Vector3[] starts;
        private Rect panel=new Rect(18,18,330,330);
        private Vector2 scroll;
        private GUIStyle label,button;
        public void Configure(RtsInput source,SelectionController controller,SelectableUnit[] actors,Vector3[] flat,Vector3[] slope)
        {input=source;selection=controller;units=actors;flatTargets=flat;slopeTargets=slope;}
        public bool ContainsScreenPoint(Vector2 point)=>panel.Contains(new Vector2(point.x,Screen.height-point.y));
        private void Start()
        {
            starts=new Vector3[units.Length];for(int i=0;i<units.Length;i++) starts[i]=units[i].transform.position;
            input.SetInterfaceBlocker(ContainsScreenPoint);selection.Select(units[0]);
        }
        private void OnEnable(){if(input!=null) input.SetInterfaceBlocker(ContainsScreenPoint);}
        private void OnDisable(){if(input!=null) input.SetInterfaceBlocker(null);}
        public bool WalkRoute(bool slope)=>Order(slope?slopeTargets:flatTargets);
        public bool ReturnToStart()=>Order(starts);
        private bool Order(Vector3[] targets)
        {
            if(targets==null||targets.Length!=units.Length) return false;
            bool accepted=true;var dispatcher=new CommandDispatcher();
            for(int i=0;i<units.Length;i++) if(units[i]!=null) accepted&=dispatcher.Dispatch(new MoveCommand(targets[i]),units[i].Motor);
            selection.ReportStatus(accepted?"Walking comparison route.":"Some units could not reach the route.");return accepted;
        }
        public void StopWalkers()
        {foreach(var unit in units) if(unit!=null) unit.Motor.Stop();selection.ReportStatus("Stopped. Feet settle into a standing pose.");}
        private void OnGUI()
        {
            label??=new GUIStyle(GUI.skin.label){wordWrap=true,fontSize=14};
            button??=new GUIStyle(GUI.skin.button){wordWrap=true,fontSize=14,padding=new RectOffset(8,8,7,7)};
            float width=Mathf.Min(330,Screen.width-36),height=Mathf.Max(1,Screen.height-36);
            GUILayout.BeginArea(new Rect(18,18,width,height));scroll=GUILayout.BeginScrollView(scroll,GUILayout.MaxHeight(height));GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("WONDER GATHER / THE LIVING BODY",label);
            GUILayout.Label("Two walking paces; one provisional body.\nMeasured starts left / Brisk starts right",label);
            GUILayout.Label("Click / drag: select   Shift: add\nRight click: move   F: focus\nWASD: pan   Q/E: rotate   Wheel: zoom",label);
            if(GUILayout.Button("Walk flat route",button)) WalkRoute(false);
            if(GUILayout.Button("Walk up the slope",button)) WalkRoute(true);
            if(GUILayout.Button("Walk back to start",button)) ReturnToStart();
            if(GUILayout.Button("Stop both",button)) StopWalkers();
            GUILayout.Label("Selected: "+(selection.Count==1?selection.Selected.name:selection.Count.ToString()),label);
            GUILayout.Label(selection.Status,label);
            GUILayout.EndVertical();GUILayout.EndScrollView();
            if(Event.current.type==EventType.Repaint) panel=new Rect(18,18,width,GUILayoutUtility.GetLastRect().height);
            GUILayout.EndArea();
            if(selection.IsDragging)
            {
                var box=selection.DragRect;box.y=Screen.height-box.yMax;var old=GUI.color;GUI.color=new Color(.5f,1,.7f,.18f);GUI.DrawTexture(box,Texture2D.whiteTexture);GUI.color=old;
            }
        }
    }
}
