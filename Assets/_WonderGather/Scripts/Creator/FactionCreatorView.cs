using UnityEngine;
using System.Linq;
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
        private BuildingBlueprint pendingBuilding;
        private bool showBuildings;
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
            if(GUILayout.Button((value?"ON   ":"OFF   ")+label,button,GUILayout.Width(260))) return !value;return value;
        }
        private void DrawRemove(FactionDraft draft)
        {
            Fill(new Rect(300,130,680,460),panel);
            bool building=pendingBuilding!=null;
            Label(new Rect(326,150,628,46),building?"Remove building blueprint?":"Remove unit blueprint?",title);
            string message=building?"Remove “"+pendingBuilding.DisplayName+"”? Construction permissions to this building and all of its training links will be removed.":"Remove “"+pendingRemove.DisplayName+"”? Starting units removed: "+draft.StartingCount(pendingRemove)+". This unit will be removed from every building's training list.";
            Label(new Rect(326,212,628,170),message+" Other blueprints keep their choices. Your saved faction changes only when you save.",body);
            if(GUI.Button(new Rect(326,410,304,46),"Remove blueprint",button))
            {if(building) draft.RemoveBuilding(pendingBuilding);else draft.RemoveUnit(pendingRemove);pendingRemove=null;pendingBuilding=null;selected=2;detailScroll=Vector2.zero;}
            if(GUI.Button(new Rect(650,410,304,46),"Cancel",button)){pendingRemove=null;pendingBuilding=null;}
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
            if(shownDraft!=draft){shownDraft=draft;selected=2;pendingRemove=null;pendingBuilding=null;showBuildings=false;unitScroll=Vector2.zero;}
            selected=Mathf.Clamp(selected,0,(showBuildings?draft.Buildings.Count:draft.Units.Count)+1);
            var oldMatrix=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            var offset=new Vector3((Screen.width-1280*scale)*.5f,(Screen.height-720*scale)*.5f,0);
            GUI.matrix=Matrix4x4.TRS(offset,Quaternion.identity,Vector3.one*scale);
            Fill(new Rect(0,0,1280,720),background);
            if(library.Draw(creator,title,body,small,button,field,panel)){GUI.matrix=oldMatrix;return;}
            if(pendingRemove!=null || pendingBuilding!=null){DrawRemove(draft);GUI.matrix=oldMatrix;return;}
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
            GUILayout.Space(18);if(GUILayout.Button("Starting base: 1",button)) selected=0;
            GUILayout.Label("One supply depot begins completed.",small);
            GUILayout.Space(12);GUILayout.Label("Prototype values and visuals.",small);
            GUILayout.EndArea();

            Label(new Rect(306,128,560,34),"Blueprint relationships",subtitle);
            if(GUI.Button(new Rect(306,170,270,38),(showBuildings?"":"● ")+"Units ("+draft.Units.Count+")",button)){showBuildings=false;selected=2;unitScroll=detailScroll=Vector2.zero;GUI.FocusControl(null);}
            if(GUI.Button(new Rect(594,170,294,38),(showBuildings?"● ":"")+"Buildings ("+draft.Buildings.Count+")",button)){showBuildings=true;selected=2;unitScroll=detailScroll=Vector2.zero;GUI.FocusControl(null);}
            GUI.enabled=!creator.Busy && (showBuildings?draft.Buildings.Count<FactionDraft.MaxBuildingBlueprints:draft.Units.Count<FactionDraft.MaxUnitBlueprints);
            if(GUI.Button(new Rect(306,220,350,38),showBuildings?"Add workshop blueprint":"Add worker blueprint",button))
            {
                if(showBuildings){draft.AddBuilding();selected=draft.Buildings.Count+1;}else{draft.AddUnit();selected=draft.Units.Count+1;}
                unitScroll.y=float.MaxValue;detailScroll=Vector2.zero;GUI.FocusControl(null);
            }
            GUI.enabled=!creator.Busy;
            Label(new Rect(674,224,212,30),"Up to 8 of each type",small);
            GUILayout.BeginArea(new Rect(306,270,582,278));unitScroll=GUILayout.BeginScrollView(unitScroll);
            int count=showBuildings?draft.Buildings.Count:draft.Units.Count;
            for(int i=0;i<count;i++)
            {
                string label,links;
                if(showBuildings)
                {
                    var building=draft.Buildings[i];label=building.DisplayName;
                    links="Trains "+building.ProductionOptions.Count()+" types  |  Built by "+draft.Units.Count(x=>x.CanBuild(building))+" types";
                }
                else
                {
                    var unit=draft.Units[i];label=unit.DisplayName;
                    links=draft.StartingCount(unit)+" at start  |  Builds "+unit.Builds.Count+" types  |  Trained by "+draft.Buildings.Count(x=>x.ProductionOptions.Contains(unit))+" types";
                }
                var previous=GUI.backgroundColor;GUI.backgroundColor=selected==i+2?accent:Color.white;
                if(GUILayout.Button(label+"\n"+links,button,GUILayout.Width(552),GUILayout.MinHeight(64))){selected=i+2;detailScroll=Vector2.zero;GUI.FocusControl(null);}
                GUI.backgroundColor=previous;
            }
            GUILayout.EndScrollView();GUILayout.EndArea();
            GUILayout.BeginArea(new Rect(940,128,300,428));detailScroll=GUILayout.BeginScrollView(detailScroll);
            GUILayout.Label(selected==0?"Starting base":showBuildings?"Building blueprint":"Unit blueprint",subtitle);GUILayout.Space(12);
            if(selected==0)
            {
                GUILayout.Label("Your faction begins here.",body);GUILayout.Space(12);
                GUILayout.Label("Stores supplies and starts completed. The starting depot stays fixed in this prototype.",body);
            }
            else if(!showBuildings)
            {
                var unit=draft.Units[Mathf.Max(0,selected-2)];
                GUILayout.Label("Blueprint name",body);string label=GUILayout.TextField(unit.DisplayName,64,field,GUILayout.Width(260));
                int start=Stepper("At start",draft.StartingCount(unit),1,0,FactionDraft.MaxStartingUnits-draft.TotalStartingUnits+draft.StartingCount(unit));
                bool gathers=Toggle("Gather supplies",unit.GathersSupplies);
                if(label!=unit.DisplayName || gathers!=unit.GathersSupplies) draft.ConfigureUnit(unit,label,gathers,unit.CanBuild(draft.Workshop));
                if(start!=draft.StartingCount(unit)) draft.SetStartingCount(unit,start);
                GUILayout.Space(10);GUILayout.Label("Can construct",body);
                foreach(var building in draft.Buildings)
                {bool enabled=Toggle(building.DisplayName,unit.CanBuild(building));if(enabled!=unit.CanBuild(building)) draft.SetBuildPermission(unit,building,enabled);}
                GUILayout.Space(10);GUILayout.Label("Trained by",body);
                var producers=draft.Buildings.Where(x=>x.ProductionOptions.Contains(unit)).ToArray();
                if(producers.Length==0) GUILayout.Label("No building yet. Edit a building to add its training link.",small);
                foreach(var building in producers) GUILayout.Label(building.DisplayName,small);
                GUILayout.Label("Training: "+unit.Production.Cost+" supplies / "+unit.Production.Seconds+" seconds",small);
                GUI.enabled=!creator.Busy && draft.Units.Count<FactionDraft.MaxUnitBlueprints;
                if(GUILayout.Button("Duplicate blueprint",button)){draft.AddUnit(unit);selected=draft.Units.Count+1;unitScroll.y=float.MaxValue;GUI.FocusControl(null);}
                GUI.enabled=!creator.Busy && draft.Units.Count>1;
                if(GUILayout.Button("Remove blueprint…",button)){pendingRemove=unit;GUI.FocusControl(null);}
                GUI.enabled=!creator.Busy;
            }
            else
            {
                var building=draft.Buildings[Mathf.Max(0,selected-2)];
                GUILayout.Label("Blueprint name",body);string label=GUILayout.TextField(building.DisplayName,64,field,GUILayout.Width(260));
                if(label!=building.DisplayName) draft.RenameBuilding(building,label);
                GUILayout.Space(10);GUILayout.Label("Can train",body);
                foreach(var unit in draft.Units)
                {bool current=building.ProductionOptions.Contains(unit);bool enabled=Toggle(unit.DisplayName,current);if(enabled!=current) draft.SetTraining(building,unit,enabled);}
                GUILayout.Space(10);GUILayout.Label("Can be constructed by",body);
                var builders=draft.Units.Where(x=>x.CanBuild(building)).ToArray();
                if(builders.Length==0) GUILayout.Label("No unit yet. Edit a unit's construction permissions.",small);
                foreach(var unit in builders) GUILayout.Label(unit.DisplayName,small);
                GUILayout.Label("Construction: "+building.Construction.Cost+" supplies / "+building.Construction.Seconds+" seconds",small);
                GUI.enabled=!creator.Busy && draft.Buildings.Count<FactionDraft.MaxBuildingBlueprints;
                if(GUILayout.Button("Duplicate blueprint",button)){draft.AddBuilding(building);selected=draft.Buildings.Count+1;unitScroll.y=float.MaxValue;GUI.FocusControl(null);}
                GUI.enabled=!creator.Busy && draft.Buildings.Count>1;
                if(GUILayout.Button("Remove blueprint…",button)){pendingBuilding=building;GUI.FocusControl(null);}
                GUI.enabled=!creator.Busy;
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
