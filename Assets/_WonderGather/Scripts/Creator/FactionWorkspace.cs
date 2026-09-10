using System;
using System.Collections.Generic;
namespace WonderGather
{
    public sealed class FactionWorkspace : IDisposable
    {
        private readonly CivilizationDefinition template;
        private readonly FactionStore store;
        private string cleanState,token;
        public FactionDraft Draft {get;private set;}
        public string CurrentId {get;private set;}
        public string Message {get;private set;}="";
        public bool HasFile=>CurrentId!=null;
        public bool IsDirty=>State()!=cleanState;
        public string SaveState=>IsDirty?"Unsaved changes":HasFile?"All changes saved":"New faction — not saved yet";
        private string State()
        {
            var state=new System.Text.StringBuilder();
            void Part(string value){value=value??"";state.Append(value.Length).Append(':').Append(value);}
            Part(Draft.Definition.DisplayName);Part(Draft.Definition.StartingSupplies.ToString());
            Part(Draft.Units.Count.ToString());
            foreach(var unit in Draft.Units){Part(unit.Id);Part(unit.DisplayName);Part(Draft.StartingCount(unit).ToString());Part(unit.GathersSupplies.ToString());Part(unit.Builds.Count.ToString());foreach(var building in unit.Builds) Part(building.Id);}
            Part(Draft.Buildings.Count.ToString());
            foreach(var building in Draft.Buildings){Part(building.Id);Part(building.DisplayName);var options=new List<UnitBlueprint>(building.ProductionOptions);Part(options.Count.ToString());foreach(var unit in options) Part(unit.Id);}
            return state.ToString();
        }
        public FactionWorkspace(CivilizationDefinition example,FactionStore storage)
        {template=example;store=storage;Draft=new FactionDraft(example);cleanState=State();}
        public void ClearMessage()=>Message="";
        public List<FactionEntry> List()
        {
            try
            {
                var entries=store.List();
                foreach(var entry in entries) if(entry.CanOpen)
                {
                    try{using(var check=entry.Record.CreateDraft(template)){} }
                    catch(Exception error) when(FactionStore.IsStorageError(error)){entry.Problem=error.Message;}
                }
                return entries;
            }
            catch(Exception error) when(FactionStore.IsStorageError(error)){Message="Cannot read the faction library: "+error.Message;return new List<FactionEntry>();}
        }
        public bool Save(bool copy=false,string copyName=null)
        {
            try
            {
                var record=FactionRecord.Capture(Draft,copy||CurrentId==null?Guid.NewGuid().ToString("N"):CurrentId);
                if(copyName!=null) record.Name=copyName.Trim();
                var saved=store.Save(record,copy?null:token);
                Draft.SetFactionSetup(saved.Name,Draft.Definition.StartingSupplies);
                CurrentId=saved.Id;token=saved.Token;cleanState=State();Message=copy?"Saved a separate copy.":"Faction saved.";return true;
            }
            catch(Exception error) when(FactionStore.IsStorageError(error)){Message="Could not save. Your draft is intact. "+error.Message;return false;}
        }
        public bool Open(string id,bool discardChanges=false)
        {
            if(IsDirty && !discardChanges){Message="Save or confirm discarding your edits before opening another faction.";return false;}
            try
            {
                var entry=store.Read(id);var replacement=entry.Record.CreateDraft(template);
                var old=Draft;Draft=replacement;CurrentId=entry.Id;token=entry.Token;cleanState=State();old.Dispose();Message="Opened "+entry.Name+".";return true;
            }
            catch(Exception error) when(FactionStore.IsStorageError(error)){Message="Could not open this faction. Your draft is intact. "+error.Message;return false;}
        }
        public bool New(bool discardChanges=false)
        {
            if(IsDirty && !discardChanges){Message="Save or confirm discarding your edits before starting another faction.";return false;}
            var replacement=new FactionDraft(template);var old=Draft;Draft=replacement;CurrentId=null;token=null;cleanState=State();old.Dispose();Message="Started a new faction.";return true;
        }
        public bool Rename(string id,string name)
        {
            if(id==CurrentId && IsDirty){Message="Save your draft changes before renaming this saved faction.";return false;}
            try
            {
                var entry=store.Read(id);if(id==CurrentId && entry.Token!=token) throw new System.IO.IOException("This faction changed outside the creator. Open it again before renaming it.");entry.Record.Name=(name??"").Trim();var saved=store.Save(entry.Record,entry.Token);
                if(CurrentId==id){Draft.SetFactionSetup(saved.Name,Draft.Definition.StartingSupplies);token=saved.Token;cleanState=State();}
                Message="Saved faction renamed.";return true;
            }
            catch(Exception error) when(FactionStore.IsStorageError(error)){Message="Could not rename: "+error.Message;return false;}
        }
        public bool Delete(FactionEntry entry)
        {
            try
            {
                store.Delete(entry.Id,entry.Token);
                if(CurrentId==entry.Id){CurrentId=null;token=null;cleanState="";}
                Message="Saved faction moved to the Deleted recovery folder. Your open draft is unchanged.";return true;
            }
            catch(Exception error) when(FactionStore.IsStorageError(error)){Message="Could not delete: "+error.Message;return false;}
        }
        public void Dispose()=>Draft.Dispose();
    }
}
