using UnityEngine;
namespace WonderGather
{
    [RequireComponent(typeof(FactionCreator))]
    public sealed class FactionCreatorView : MonoBehaviour
    {
        private FactionCreator creator;
        private readonly FactionLibraryPanel library=new FactionLibraryPanel();
        private int selected=1;
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
        private void Stroke(Vector2 a,Vector2 b)
        {
            var matrix=GUI.matrix;var delta=b-a;GUI.matrix=matrix*Matrix4x4.TRS(new Vector3(a.x,a.y,0),Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg),Vector3.one);
            Fill(new Rect(0,-1,delta.magnitude,2),accent);GUI.matrix=matrix;
        }
        private void Edge(Vector2 a,Vector2 b,string text,Rect label)
        {
            var direction=(b-a).normalized;var side=new Vector2(-direction.y,direction.x);
            Stroke(a,b);Stroke(b,b-direction*12+side*6);Stroke(b,b-direction*12-side*6);
            Label(label,text,small);
        }
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
            var oldMatrix=GUI.matrix;
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);
            var offset=new Vector3((Screen.width-1280*scale)*.5f,(Screen.height-720*scale)*.5f,0);
            GUI.matrix=Matrix4x4.TRS(offset,Quaternion.identity,Vector3.one*scale);
            Fill(new Rect(0,0,1280,720),background);
            if(library.Draw(creator,title,body,small,button,field,panel)){GUI.matrix=oldMatrix;return;}
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
            int workers=Stepper("Workers",draft.StartingWorkers,1,0,8);
            GUILayout.Space(12);
            int supplies=Stepper("Stored supplies",draft.Definition.StartingSupplies,10,0,120);
            if(name!=draft.Definition.DisplayName || workers!=draft.StartingWorkers || supplies!=draft.Definition.StartingSupplies)
                draft.SetStartingSetup(name,workers,supplies);
            GUILayout.Space(18);GUILayout.Label("Starting base: 1",body);
            GUILayout.Label("One supply depot begins completed.",small);
            GUILayout.Space(12);GUILayout.Label("Starting limits are provisional.",small);
            GUILayout.EndArea();

            Label(new Rect(306,130,560,34),"Blueprint relationships",subtitle);
            Label(new Rect(306,168,550,30),"Select a card to inspect and change its links.",small);
            Card(new Rect(326,224,170,98),0,"Starting base\nSupply depot");
            Card(new Rect(686,224,180,98),1,"Worker\n"+draft.StartingWorkers+" at start");
            Card(new Rect(686,420,180,98),2,"Workshop\n"+(draft.Workshop.Produces!=null?"Trains workers":"Training disabled"));
            if(draft.StartingWorkers>0) Edge(new Vector2(502,256),new Vector2(678,256),"Starts with",new Rect(524,222,140,30));
            else Label(new Rect(520,245,150,52),"No starting workers",small);
            if(draft.Worker.CanBuild(draft.Workshop)) Edge(new Vector2(726,330),new Vector2(726,412),"Builds",new Rect(657,362,65,30));
            if(draft.Workshop.Produces!=null) Edge(new Vector2(824,412),new Vector2(824,330),"Trains",new Rect(831,362,70,30));
            Label(new Rect(310,354,280,110),draft.Worker.GathersSupplies?"Worker gathers supplies.\nSupplies pay for construction and training.":"Supply gathering is disabled.\nYour starting stock may run out.",body);
            Label(new Rect(306,534,565,28),"Links show what can be reached, not guaranteed affordability.",small);

            GUILayout.BeginArea(new Rect(940,128,300,428));
            detailScroll=GUILayout.BeginScrollView(detailScroll);
            GUILayout.Label(selected==0?"Starting base":selected==1?"Worker":"Workshop",subtitle);
            GUILayout.Space(14);
            if(selected==0)
            {
                GUILayout.Label("Your faction begins here.",body);GUILayout.Space(12);
                GUILayout.Label("Stores delivered supplies and starts completed. It does not train units in this first example.",body);
                GUILayout.Space(16);GUILayout.Label("Adjust the starting workers and stock in the left panel.",small);
            }
            else if(selected==1)
            {
                GUILayout.Label("Capabilities",body);
                bool gathers=Toggle("Gather supplies",draft.Worker.GathersSupplies);
                bool builds=Toggle("Construct workshop",draft.Worker.CanBuild(draft.Workshop));
                if(gathers!=draft.Worker.GathersSupplies || builds!=draft.Worker.CanBuild(draft.Workshop)) draft.SetWorkerPermissions(gathers,builds);
                GUILayout.Space(18);
                GUILayout.Label("Training cost: "+draft.Worker.Production.Cost+" supplies",body);
                GUILayout.Label("Training time: "+draft.Worker.Production.Seconds+" seconds",body);
                GUILayout.Space(12);GUILayout.Label("Newly trained workers inherit these same capabilities.",small);
            }
            else
            {
                bool produces=Toggle("Train workers",draft.Workshop.Produces!=null);
                if(produces!=(draft.Workshop.Produces!=null)) draft.SetProduction(produces);
                GUILayout.Space(18);GUILayout.Label("Construction cost: "+draft.Workshop.Construction.Cost+" supplies",body);
                GUILayout.Label("Build time: "+draft.Workshop.Construction.Seconds+" seconds",body);
                GUILayout.Space(12);GUILayout.Label("Workers need construction permission to create this building. Training requires a completed workshop.",small);
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
