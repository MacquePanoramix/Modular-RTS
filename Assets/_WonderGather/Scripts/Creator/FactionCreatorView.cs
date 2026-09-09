using UnityEngine;
namespace WonderGather
{
    [RequireComponent(typeof(FactionCreator))]
    public sealed class FactionCreatorView : MonoBehaviour
    {
        private FactionCreator creator;
        private readonly FactionLibraryPanel library=new FactionLibraryPanel();
        private int selected=2;
        private FactionDraft shownDraft;
        private UnitBlueprint pendingRemove;
        private Vector2 unitScroll;
        private Vector2 detailScroll,warningScroll;
        private GUIStyle title,subtitle,body,small,card,button,field;
        private readonly Color background=new Color(.075f,.095f,.105f);
        private readonly Color panel=new Color(.12f,.145f,.155f);
        private readonly Color accent=new Color(.67f,.83f,.75f);
        private void Awake()=>creator=GetComponent<FactionCreator>();
        private void Styles()
        {
            if(title!=null) return;
            title=new GUIStyle(GUI.skin.label){fontSize=28,fontStyle=FontStyle.Bold};
            subtitle=new GUIStyle(GUI.skin.label){fontSize=20,fontStyle=FontStyle.Bold};
            body=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true};
            small=new GUIStyle(body){fontSize=15};
            card=new GUIStyle(GUI.skin.button){fontSize=20,wordWrap=true,alignment=TextAnchor.MiddleCenter};
            button=new GUIStyle(GUI.skin.button){fontSize=17,wordWrap=true,padding=new RectOffset(10,10,8,8)};
            field=new GUIStyle(GUI.skin.textField){fontSize=18,padding=new RectOffset(8,8,7,7)};
        }
        private static void Fill(Rect rect,Color color)
        {var previous=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=previous;}
        private void Label(Rect rect,string text,GUIStyle style=null)=>GUI.Label(rect,text,style??body);
        private static string ShortName(string value)=>value.Length>18?value.Substring(0,15)+"…":value;
        private void Card(Rect rect,int id,string text)
        {
            var previous=GUI.backgroundColor;GUI.backgroundColor=selected==id?accent:Color.white;
            if(GUI.Button(rect,text,card)){selected=id;detailScroll=Vector2.zero;GUI.FocusControl(null);}GUI.backgroundColor=previous;
        }
        private int Stepper(string label,int value,int step,int min,int max)
        {
            GUILayout.Label(label+": "+value,body);
            GUILayout.BeginHorizontal();GUI.enabled=!creator.Busy && value>min;
            if(GUILayout.Button("− "+step,button)) value=Mathf.Max(min,value-step);
            GUI.enabled=!creator.Busy && value<max;
            if(GUILayout.Button("+ "+step,button)) value=Mathf.Min(max,value+step);
            GUI.enabled=!creator.Busy;GUILayout.EndHorizontal();return value;
        }
        private bool Toggle(string label,bool value)
        {
            if(GUILayout.Button((value?"ON   ":"OFF   ")+label,button)) return !value;return value;
        }
        private void DrawRemove(FactionDraft draft)
        {
            Fill(new Rect(300,130,680,460),panel);
            Label(new Rect(326,150,628,46),"Remove unit blueprint?",title);
            Label(new Rect(326,212,628,170),"Remove “"+pendingRemove.DisplayName+"”? Starting units removed: "+draft.StartingCount(pendingRemove)+"."+(draft.Workshop.Produces==pendingRemove?" The workshop's training link will also be cleared.":"")+" Other blueprints keep their choices. Your saved faction changes only when you save.",body);
            if(GUI.Button(new Rect(326,410,304,46),"Remove blueprint",button)){draft.RemoveUnit(pendingRemove);pendingRemove=null;selected=2;detailScroll=Vector2.zero;}
            if(GUI.Button(new Rect(650,410,304,46),"Cancel",button)) pendingRemove=null;
        }
        private void OnGUI()
        {
            Styles();
            if(creator.Playing && !creator.QuitPending)
            {
                GUI.enabled=!creator.Busy;
                if(GUI.Button(creator.ReturnButton,creator.Busy?"Returning...":"Return to faction creator",button)) creator.ReturnToCreator();
                GUI.enabled=true;return;
            }
            var draft=creator.Draft;if(draft==null) return;
            if(shownDraft!=draft){shownDraft=draft;selected=2;pendingRemove=null;unitScroll=Vector2.zero;}
            selected=Mathf.Clamp(selected,0,draft.Units.Count+1);
            var oldMatrix=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            var offset=new Vector3((Screen.width-1280*scale)*.5f,(Screen.height-720*scale)*.5f,0);
            GUI.matrix=Matrix4x4.TRS(offset,Quaternion.identity,Vector3.one*scale);
            Fill(new Rect(0,0,1280,720),background);
            if(library.Draw(creator,title,body,small,button,field,panel)){GUI.matrix=oldMatrix;return;}
            if(pendingRemove!=null){DrawRemove(draft);GUI.matrix=oldMatrix;return;}
            Label(new Rect(28,18,630,42),"Create your faction",title);
            GUI.enabled=!creator.Busy;
            if(GUI.Button(new Rect(680,20,110,40),"Save",button)){GUI.FocusControl(null);creator.Workspace.Save();}
            if(GUI.Button(new Rect(800,20,160,40),"Save as copy",button)){GUI.FocusControl(null);library.Copy(creator.Workspace);}
            if(GUI.Button(new Rect(970,20,160,40),"Faction library",button)){GUI.FocusControl(null);library.OpenLibrary(creator.Workspace);}
            if(GUI.Button(new Rect(1140,20,116,40),"Exit",button)) creator.RequestQuit();
            Label(new Rect(28,64,1220,38),string.IsNullOrEmpty(creator.Workspace.Message)?creator.Status:creator.Workspace.Message,body);
            Fill(new Rect(24,112,246,460),panel);Fill(new Rect(286,112,622,460),panel);Fill(new Rect(924,112,332,460),panel);
            GUI.enabled=!creator.Busy;
            GUILayout.BeginArea(new Rect(40,128,214,428));
            GUILayout.Label("Starting setup",subtitle);GUILayout.Space(12);
            GUILayout.Label("Faction name",body);
            string name=GUILayout.TextField(draft.Definition.DisplayName,64,field);
            GUILayout.Space(18);
            int supplies=Stepper("Stored supplies",draft.Definition.StartingSupplies,10,0,120);
            if(name!=draft.Definition.DisplayName || supplies!=draft.Definition.StartingSupplies) draft.SetFactionSetup(name,supplies);
            GUILayout.Space(18);GUILayout.Label("Starting units: "+draft.TotalStartingUnits+" / "+FactionDraft.MaxStartingUnits,body);
            GUILayout.Label("Select a unit blueprint to set its starting count.",small);
            GUILayout.Space(18);GUILayout.Label("Starting base: 1",body);
            GUILayout.Label("One supply depot begins completed.",small);
            GUILayout.Space(12);GUILayout.Label("Map limits and shared worker visuals are provisional.",small);
            GUILayout.EndArea();

            Label(new Rect(306,128,560,34),"Blueprint relationships",subtitle);
            Card(new Rect(306,170,270,66),0,"Starting base\nSupply depot");
            Card(new Rect(594,170,294,66),1,"Workshop\n"+(draft.Workshop.Produces!=null?"Trains: "+ShortName(draft.Workshop.Produces.DisplayName):"Training disabled"));
            GUI.enabled=!creator.Busy && draft.Units.Count<FactionDraft.MaxUnitBlueprints;
            if(GUI.Button(new Rect(306,248,270,38),"Add worker blueprint",button))
            {draft.AddUnit();selected=draft.Units.Count+1;unitScroll.y=float.MaxValue;detailScroll=Vector2.zero;GUI.FocusControl(null);}
            GUI.enabled=!creator.Busy;
            Label(new Rect(594,252,290,32),draft.Units.Count+" / "+FactionDraft.MaxUnitBlueprints+" unit blueprints",small);
            GUILayout.BeginArea(new Rect(306,298,582,250));unitScroll=GUILayout.BeginScrollView(unitScroll);
            for(int i=0;i<draft.Units.Count;i++)
            {
                var unit=draft.Units[i];var previous=GUI.backgroundColor;GUI.backgroundColor=selected==i+2?accent:Color.white;
                string links="Base → "+draft.StartingCount(unit)+" at start"+(unit.CanBuild(draft.Workshop)?"   |   Builds workshop":"")+(draft.Workshop.Produces==unit?"   |   Workshop trains this":"");
                if(GUILayout.Button(unit.DisplayName+"\n"+links,button,GUILayout.Width(552),GUILayout.MinHeight(64)))
                {selected=i+2;detailScroll=Vector2.zero;GUI.FocusControl(null);}
                GUI.backgroundColor=previous;
            }
            GUILayout.EndScrollView();GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(940,128,300,428));detailScroll=GUILayout.BeginScrollView(detailScroll);
            GUILayout.Label(selected==0?"Starting base":selected==1?"Workshop":"Unit blueprint",subtitle);GUILayout.Space(12);
            if(selected==0)
            {
                GUILayout.Label("Your faction begins here.",body);GUILayout.Space(12);
                GUILayout.Label("Stores delivered supplies and starts completed. It does not train units in this prototype.",body);
                GUILayout.Space(16);GUILayout.Label("Set starting counts on each unit blueprint. The shared total is limited to eight for this map.",small);
            }
            else if(selected>=2)
            {
                var unit=draft.Units[selected-2];
                GUILayout.Label("Blueprint name",body);string unitName=GUILayout.TextField(unit.DisplayName,64,field,GUILayout.Width(278));
                int count=Stepper("At start",draft.StartingCount(unit),1,0,FactionDraft.MaxStartingUnits-draft.TotalStartingUnits+draft.StartingCount(unit));
                bool gathers=Toggle("Gather supplies",unit.GathersSupplies);
                bool builds=Toggle("Construct workshop",unit.CanBuild(draft.Workshop));
                if(unitName!=unit.DisplayName || gathers!=unit.GathersSupplies || builds!=unit.CanBuild(draft.Workshop)) draft.ConfigureUnit(unit,unitName,gathers,builds);
                if(count!=draft.StartingCount(unit)) draft.SetStartingCount(unit,count);
                GUILayout.Space(10);GUILayout.Label("Training: "+unit.Production.Cost+" supplies / "+unit.Production.Seconds+" seconds",small);
                GUILayout.Label(draft.Workshop.Produces==unit?"The workshop trains this blueprint.":"Select Workshop to change its trained blueprint.",small);
                GUI.enabled=!creator.Busy && draft.Units.Count<FactionDraft.MaxUnitBlueprints;
                if(GUILayout.Button("Duplicate blueprint",button)){draft.AddUnit(unit);selected=draft.Units.Count+1;unitScroll.y=float.MaxValue;GUI.FocusControl(null);}
                GUI.enabled=!creator.Busy && draft.Units.Count>1;
                if(GUILayout.Button("Remove blueprint…",button)){pendingRemove=unit;GUI.FocusControl(null);}
                GUI.enabled=!creator.Busy;
                if(draft.Units.Count==1) GUILayout.Label("Keep at least one blueprint. Its starting count may be zero.",small);
            }
            else
            {
                GUILayout.Label("Choose the blueprint this workshop trains.",body);GUILayout.Space(10);
                if(GUILayout.Button((draft.Workshop.Produces==null?"● ":"○ ")+"No training",button)) draft.SetProductionUnit(null);
                foreach(var unit in draft.Units)
                    if(GUILayout.Button((draft.Workshop.Produces==unit?"● ":"○ ")+unit.DisplayName,button)) draft.SetProductionUnit(unit);
                GUILayout.Space(14);GUILayout.Label("Construction: "+draft.Workshop.Construction.Cost+" supplies / "+draft.Workshop.Construction.Seconds+" seconds",small);
                GUILayout.Label("One trained blueprint per workshop for this slice. All trained units inherit that blueprint's capabilities.",small);
            }
            GUILayout.EndScrollView();GUILayout.EndArea();
            Fill(new Rect(24,588,890,108),panel);
            GUILayout.BeginArea(new Rect(40,598,858,90));warningScroll=GUILayout.BeginScrollView(warningScroll);
            GUILayout.Label(draft.Report.Errors.Count>0?"Setup needs attention":draft.Report.Warnings.Count>0?"Possible dead ends":"Starting network connected",subtitle);
            if(draft.Report.Errors.Count==0 && draft.Report.Warnings.Count==0) GUILayout.Label("A gathering and growth route exists. Test it to see how it plays.",small);
            foreach(var error in draft.Report.Errors) GUILayout.Label(error,small);
            foreach(var warning in draft.Report.Warnings) GUILayout.Label(warning,small);
            GUILayout.EndScrollView();GUILayout.EndArea();
            GUI.enabled=!creator.Busy && draft.Report.CanInstantiate;
            if(GUI.Button(new Rect(934,588,322,52),creator.Busy?"Opening playtest...":"Playtest faction  →",button)){GUI.FocusControl(null);creator.Playtest();}
            GUI.enabled=true;
            Label(new Rect(934,650,322,45),creator.Workspace.SaveState,small);
            GUI.matrix=oldMatrix;
        }
    }
}
