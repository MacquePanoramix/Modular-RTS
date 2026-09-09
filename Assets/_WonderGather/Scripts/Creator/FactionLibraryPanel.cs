using System.Collections.Generic;
using UnityEngine;
namespace WonderGather
{
    // Transient UI choices live here; persistence and dirty-state rules live in the workspace.
    public sealed class FactionLibraryPanel
    {
        private enum Page { Closed, Library, Copy, Rename, Open, Delete }
        private Page page;
        private List<FactionEntry> entries=new List<FactionEntry>();
        private FactionEntry selected;
        private string name="",pendingId;
        private Vector2 listScroll,detailsScroll;
        public void OpenLibrary(FactionWorkspace workspace){workspace.ClearMessage();entries=workspace.List();selected=null;page=Page.Library;}
        public void Copy(FactionWorkspace workspace)
        {workspace.ClearMessage();name=workspace.Draft.Definition.DisplayName;name=name.Substring(0,Mathf.Min(59,name.Length))+" copy";page=Page.Copy;}
        private void Refresh(FactionWorkspace workspace)
        {entries=workspace.List();selected=null;}
        private static void Fill(Rect rect,Color color)
        {var old=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=old;}
        private void RequestOpen(FactionWorkspace workspace,string id)
        {
            workspace.ClearMessage();pendingId=id;
            if(workspace.IsDirty){page=Page.Open;return;}
            if(id==null?workspace.New():workspace.Open(id)) page=Page.Closed;
        }
        private void CompleteOpen(FactionWorkspace workspace,bool save)
        {
            if(save && !workspace.Save()) return;
            if(pendingId==null?workspace.New(true):workspace.Open(pendingId,true)) page=Page.Closed;
        }
        public bool Draw(FactionCreator creator,GUIStyle title,GUIStyle body,GUIStyle small,GUIStyle button,GUIStyle field,Color panel)
        {
            var workspace=creator.Workspace;
            if(page==Page.Closed && !creator.QuitPending) return false;
            GUI.enabled=true;
            if(page==Page.Library && !creator.QuitPending)
            {
                GUI.Label(new Rect(28,18,850,42),"Faction library",title);
                GUI.Label(new Rect(28,66,1050,35),"Saved on this computer. Opening a faction keeps its blueprint choices.",body);
                if(GUI.Button(new Rect(1090,22,166,40),"Back to draft",button)){page=Page.Closed;return true;}
                Fill(new Rect(24,112,560,584),panel);Fill(new Rect(600,112,656,584),panel);
                GUILayout.BeginArea(new Rect(40,128,528,552));
                GUILayout.BeginHorizontal();
                if(GUILayout.Button("New faction",button)) RequestOpen(workspace,null);
                if(GUILayout.Button("Refresh",button)) Refresh(workspace);
                GUILayout.EndHorizontal();GUILayout.Space(12);
                listScroll=GUILayout.BeginScrollView(listScroll);
                if(entries.Count==0) GUILayout.Label("No saved factions yet. Return to your draft and choose Save.",body);
                foreach(var entry in entries)
                {
                    var old=GUI.backgroundColor;GUI.backgroundColor=selected==entry?new Color(.67f,.83f,.75f):Color.white;
                    string summary=entry.CanOpen?entry.Record.Workers+" units / "+entry.Record.Supplies+" supplies":"Needs attention";
                    if(GUILayout.Button(entry.Name+"\n"+summary,button,GUILayout.MinHeight(64))){selected=entry;detailsScroll=Vector2.zero;}
                    GUI.backgroundColor=old;
                }
                GUILayout.EndScrollView();GUILayout.EndArea();
                GUILayout.BeginArea(new Rect(620,130,616,546));detailsScroll=GUILayout.BeginScrollView(detailsScroll);
                if(selected==null) GUILayout.Label("Select a saved faction to open, rename or delete it.",body);
                else
                {
                    GUILayout.Label(selected.Name,body);GUILayout.Space(14);
                    if(selected.CanOpen)
                    {
                        var record=selected.Record;
                        GUILayout.Label("Starting units: "+record.Workers+"   Supplies: "+record.Supplies,body);
                        foreach(var unit in record.Units)
                            GUILayout.Label(unit.Name+" — "+unit.Start+" at start; gather "+(unit.Gathers?"yes":"no")+", build "+(unit.Builds?"yes":"no")+(record.TrainedId==unit.Id?"; workshop trains this unit":""),body);
                    }
                    else GUILayout.Label(selected.Problem,body);
                    GUILayout.Space(20);GUI.enabled=selected.CanOpen;
                    if(GUILayout.Button("Open faction",button)) RequestOpen(workspace,selected.Id);
                    GUI.enabled=selected.CanOpen && !(selected.Id==workspace.CurrentId && workspace.IsDirty);
                    if(GUILayout.Button("Rename saved faction",button)){workspace.ClearMessage();name=selected.Name;page=Page.Rename;}
                    GUI.enabled=selected.Id!=null;
                    if(GUILayout.Button("Delete saved faction",button)){workspace.ClearMessage();page=Page.Delete;}
                    GUI.enabled=true;
                    if(selected.Id==workspace.CurrentId && workspace.IsDirty) GUILayout.Label("Save your draft before renaming its library entry.",small);
                }
                GUILayout.Space(20);GUILayout.Label(workspace.Message,body);
                GUILayout.EndScrollView();GUILayout.EndArea();return true;
            }
            Fill(new Rect(300,130,680,460),panel);
            string heading=creator.QuitPending?"Save before exiting?":page==Page.Open?"Keep your draft changes?":page==Page.Delete?"Delete saved faction?":page==Page.Copy?"Save a separate copy":"Rename saved faction";
            GUI.Label(new Rect(326,150,628,46),heading,title);
            if(creator.QuitPending)
            {
                GUI.Label(new Rect(326,212,628,70),"Your faction has unsaved changes. Choose whether to save them before closing the game.",body);
                if(GUI.Button(new Rect(326,318,628,42),"Save and exit",button)) creator.ConfirmQuit(true);
                if(GUI.Button(new Rect(326,370,628,42),"Exit without saving",button)) creator.ConfirmQuit(false);
                if(GUI.Button(new Rect(326,422,628,42),"Cancel",button)) creator.CancelQuit();
            }
            else if(page==Page.Open)
            {
                GUI.Label(new Rect(326,212,628,70),"Opening another faction will replace the edits in your current draft.",body);
                if(GUI.Button(new Rect(326,318,628,42),"Save and continue",button)) CompleteOpen(workspace,true);
                if(GUI.Button(new Rect(326,370,628,42),"Discard edits and continue",button)) CompleteOpen(workspace,false);
                if(GUI.Button(new Rect(326,422,628,42),"Cancel",button)) page=Page.Library;
            }
            else if(page==Page.Delete)
            {
                GUI.Label(new Rect(326,212,628,110),"Remove “"+selected.Name+"” from the library? Your open draft will remain intact. A recovery copy is kept on this computer.",body);
                if(GUI.Button(new Rect(326,350,304,42),"Delete saved faction",button) && workspace.Delete(selected)){Refresh(workspace);page=Page.Library;}
                if(GUI.Button(new Rect(650,350,304,42),"Cancel",button)) page=Page.Library;
            }
            else
            {
                GUI.Label(new Rect(326,212,628,36),"Faction name",body);name=GUI.TextField(new Rect(326,256,628,42),name,64,field);
                GUI.enabled=!string.IsNullOrWhiteSpace(name);
                if(GUI.Button(new Rect(326,328,304,42),page==Page.Copy?"Save copy":"Rename",button))
                {
                    bool success=page==Page.Copy?workspace.Save(true,name):workspace.Rename(selected.Id,name);
                    if(success){if(page==Page.Copy) page=Page.Closed;else{Refresh(workspace);page=Page.Library;}}
                }
                GUI.enabled=true;
                if(GUI.Button(new Rect(650,328,304,42),"Cancel",button)) page=page==Page.Copy?Page.Closed:Page.Library;
            }
            GUI.Label(new Rect(326,480,628,100),workspace.Message,small);return true;
        }
    }
}
