using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WonderGather
{
    // Owns editable blueprints; prefab and production recipe assets remain read-only.
    public sealed class FactionDraft : IDisposable
    {
        public const int MaxUnitBlueprints=8,MaxStartingUnits=8;
        private readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        private readonly List<UnitBlueprint> units=new List<UnitBlueprint>();
        private readonly Dictionary<UnitBlueprint,int> starts=new Dictionary<UnitBlueprint,int>();
        private readonly WorkerProductionDefinition recipe;
        public string TemplateWorkerId {get;}
        public CivilizationDefinition Definition {get;}
        public IReadOnlyList<UnitBlueprint> Units=>units;
        public UnitBlueprint Worker=>units[0];
        public BuildingBlueprint Workshop {get;}
        public int StartingWorkers=>StartingCount(Worker);
        public int TotalStartingUnits=>starts.Values.Sum();
        public CivilizationReport Report {get;private set;}
        public FactionDraft(CivilizationDefinition source)
        {
            if(!CivilizationValidator.Validate(source).CanInstantiate) throw new ArgumentException("The creator example must be a valid civilization.");
            var copies=source.Units.ToDictionary(x=>x,x=>Copy(x));
            var buildings=source.Buildings.ToDictionary(x=>x,x=>Copy(x));
            foreach(var pair in copies) pair.Value.Configure(pair.Key.Id,pair.Key.DisplayName,pair.Key.Production,pair.Key.GathersSupplies,pair.Key.Builds.Select(b=>buildings[b]).ToArray());
            foreach(var pair in buildings) pair.Value.Configure(pair.Key.Id,pair.Key.Prefab,pair.Key.Produces!=null?copies[pair.Key.Produces]:null);
            Definition=Copy(source);units.AddRange(copies.Values);
            foreach(var unit in units) starts[unit]=0;
            foreach(var start in source.StartingUnits) starts[copies[start.blueprint]]+=start.count;
            Definition.Configure(source.DisplayName,buildings[source.StartingBase],source.StartingSupplies,Array.Empty<StartingUnit>(),units.ToArray(),buildings.Values.ToArray());
            Workshop=Definition.Buildings.First(x=>x!=Definition.StartingBase);
            recipe=Worker.Production;TemplateWorkerId=source.Units[0].Id;Refresh();
        }
        private T Copy<T>(T source) where T:UnityEngine.Object
        {var clone=UnityEngine.Object.Instantiate(source);clone.name=source.name+" (draft)";owned.Add(clone);return clone;}
        private void Check(UnitBlueprint unit){if(unit==null || !units.Contains(unit)) throw new ArgumentException("Choose a unit in this faction.");}
        public int StartingCount(UnitBlueprint unit)=>starts.TryGetValue(unit,out int count)?count:0;
        public void SetFactionSetup(string name,int supplies)
        {
            name=name??"";
            Definition.Configure(name.Substring(0,Mathf.Min(64,name.Length)),Definition.StartingBase,Mathf.Clamp(supplies,0,120),Definition.StartingUnits.ToArray(),units.ToArray(),Definition.Buildings.ToArray());Refresh();
        }
        // Retained for the original one-unit creator and its regression tests.
        public void SetStartingSetup(string name,int workers,int supplies)
        {SetFactionSetup(name,supplies);SetStartingCount(Worker,workers);}
        public void SetStartingCount(UnitBlueprint unit,int count)
        {Check(unit);starts[unit]=Mathf.Clamp(count,0,MaxStartingUnits-(TotalStartingUnits-StartingCount(unit)));Refresh();}
        public UnitBlueprint AddUnit(UnitBlueprint duplicate=null)
        {
            if(duplicate!=null) Check(duplicate);
            if(units.Count>=MaxUnitBlueprints) return null;
            string label=duplicate!=null?duplicate.DisplayName:"Worker";
            label=label.Substring(0,Mathf.Min(54,label.Length))+(duplicate!=null?" copy":" "+(units.Count+1));
            var unit=CreateUnit("unit-"+Guid.NewGuid().ToString("N"),label,duplicate?.GathersSupplies??true,duplicate!=null?duplicate.CanBuild(Workshop):true,0);
            Refresh();return unit;
        }
        private UnitBlueprint CreateUnit(string id,string label,bool gathers,bool builds,int count)
        {
            var unit=ScriptableObject.CreateInstance<UnitBlueprint>();owned.Add(unit);units.Add(unit);starts.Add(unit,count);
            unit.name=label;unit.Configure(id,label,recipe,gathers,builds?new[]{Workshop}:Array.Empty<BuildingBlueprint>());return unit;
        }
        public void ConfigureUnit(UnitBlueprint unit,string label,bool gathers,bool builds)
        {
            Check(unit);label=label??"";label=label.Substring(0,Mathf.Min(64,label.Length));
            unit.name=label;unit.Configure(unit.Id,label,unit.Production,gathers,builds?new[]{Workshop}:Array.Empty<BuildingBlueprint>());Refresh();
        }
        public bool RemoveUnit(UnitBlueprint unit)
        {
            Check(unit);if(units.Count==1) return false;
            if(Workshop.Produces==unit) Workshop.Configure(Workshop.Id,Workshop.Prefab);
            units.Remove(unit);starts.Remove(unit);owned.Remove(unit);UnityEngine.Object.Destroy(unit);Refresh();return true;
        }
        public void SetWorkerPermissions(bool gathers,bool builds)=>ConfigureUnit(Worker,Worker.DisplayName,gathers,builds);
        public void SetProduction(bool enabled)=>SetProductionUnit(enabled?Worker:null);
        public void SetProductionUnit(UnitBlueprint unit)
        {if(unit!=null) Check(unit);Workshop.Configure(Workshop.Id,Workshop.Prefab,unit);Refresh();}
        internal void RestoreUnits(FactionUnitRecord[] records,string trained)
        {
            // The record validates the entire graph before calling this on a new, unexposed draft.
            foreach(var unit in units){owned.Remove(unit);UnityEngine.Object.Destroy(unit);}units.Clear();starts.Clear();
            foreach(var record in records) CreateUnit(record.Id,record.Name,record.Gathers,record.Builds,record.Start);
            Workshop.Configure(Workshop.Id,Workshop.Prefab,units.FirstOrDefault(x=>x.Id==trained));Refresh();
        }
        public void Refresh()
        {
            Definition.Configure(Definition.DisplayName,Definition.StartingBase,Definition.StartingSupplies,
                units.Where(x=>StartingCount(x)>0).Select(x=>new StartingUnit(x,StartingCount(x))).ToArray(),units.ToArray(),Definition.Buildings.ToArray());
            Report=CivilizationValidator.Validate(Definition);
            if(!ValidName(Definition.DisplayName)) Report.Errors.Add("Give your faction a name with 1–64 printable characters.");
            foreach(var unit in units) if(!ValidName(unit.DisplayName)) Report.Errors.Add("Give every unit blueprint a name with 1–64 printable characters.");
            if(units.Select(x=>x.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=units.Count) Report.Warnings.Add("Some unit blueprints share a name. Distinct names will make them easier to identify.");
        }
        public static bool ValidName(string value)=>!string.IsNullOrWhiteSpace(value) && value.Length<=64 && !value.Any(char.IsControl);
        public void Dispose(){foreach(var asset in owned) if(asset!=null) UnityEngine.Object.Destroy(asset);owned.Clear();}
    }
}
