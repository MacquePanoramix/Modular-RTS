using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WonderGather
{
    // Owns editable blueprints; prefab and production recipe assets remain read-only.
    public sealed class FactionDraft : IDisposable
    {
        public const int MaxUnitBlueprints=8,MaxStartingUnits=8,MaxBuildingBlueprints=8;
        private readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        private readonly List<UnitBlueprint> units=new List<UnitBlueprint>();
        private readonly List<BuildingBlueprint> workshops=new List<BuildingBlueprint>();
        private readonly BuildingSite buildingRecipe;
        public string TemplateWorkshopId {get;}
        public IReadOnlyList<BuildingBlueprint> Buildings=>workshops;
        private readonly Dictionary<UnitBlueprint,int> starts=new Dictionary<UnitBlueprint,int>();
        private readonly WorkerProductionDefinition recipe;
        public string TemplateWorkerId {get;}
        public CivilizationDefinition Definition {get;}
        public IReadOnlyList<UnitBlueprint> Units=>units;
        public UnitBlueprint Worker=>units[0];
        public BuildingBlueprint Workshop=>workshops[0];
        public int StartingWorkers=>StartingCount(Worker);
        public int TotalStartingUnits=>starts.Values.Sum();
        public CivilizationReport Report {get;private set;}
        public FactionDraft(CivilizationDefinition source)
        {
            if(!CivilizationValidator.Validate(source).CanInstantiate) throw new ArgumentException("The creator example must be a valid civilization.");
            var copies=source.Units.ToDictionary(x=>x,x=>Copy(x));
            var buildings=source.Buildings.ToDictionary(x=>x,x=>Copy(x));
            foreach(var pair in copies) pair.Value.Configure(pair.Key.Id,pair.Key.DisplayName,pair.Key.Production,pair.Key.GathersSupplies,pair.Key.Builds.Select(b=>buildings[b]).ToArray());
            foreach(var pair in buildings) pair.Value.ConfigureOptions(pair.Key.Id,pair.Key.Prefab,pair.Key.DisplayName,pair.Key.ProductionOptions.Select(x=>copies[x]));
            Definition=Copy(source);units.AddRange(copies.Values);
            foreach(var unit in units) starts[unit]=0;
            foreach(var start in source.StartingUnits) starts[copies[start.blueprint]]+=start.count;
            Definition.Configure(source.DisplayName,buildings[source.StartingBase],source.StartingSupplies,Array.Empty<StartingUnit>(),units.ToArray(),buildings.Values.ToArray());
            workshops.AddRange(Definition.Buildings.Where(x=>x!=Definition.StartingBase));
            buildingRecipe=Workshop.Prefab;TemplateWorkshopId=Workshop.Id;
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
            if(duplicate!=null) unit.Configure(unit.Id,unit.DisplayName,recipe,duplicate.GathersSupplies,duplicate.Builds.ToArray());
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
            var permissions=unit.Builds.ToList();
            if(builds!=unit.CanBuild(Workshop)){permissions.Remove(Workshop);if(builds) permissions.Insert(0,Workshop);}
            unit.name=label;unit.Configure(unit.Id,label,unit.Production,gathers,permissions.ToArray());Refresh();
        }
        public bool RemoveUnit(UnitBlueprint unit)
        {
            Check(unit);if(units.Count==1) return false;
            foreach(var building in workshops) building.ConfigureOptions(building.Id,building.Prefab,building.DisplayName,building.ProductionOptions.Where(x=>x!=unit));
            units.Remove(unit);starts.Remove(unit);owned.Remove(unit);UnityEngine.Object.Destroy(unit);Refresh();return true;
        }
        public void SetWorkerPermissions(bool gathers,bool builds)=>ConfigureUnit(Worker,Worker.DisplayName,gathers,builds);
        public void SetProduction(bool enabled)=>SetProductionUnit(enabled?Worker:null);
        public void SetProductionUnit(UnitBlueprint unit)
        {if(unit!=null) Check(unit);Workshop.ConfigureOptions(Workshop.Id,Workshop.Prefab,Workshop.DisplayName,unit!=null?new[]{unit}:Array.Empty<UnitBlueprint>());Refresh();}
        private void CheckBuilding(BuildingBlueprint building)
        {if(building==null || !workshops.Contains(building)) throw new ArgumentException("Choose a building in this faction.");}
        public BuildingBlueprint AddBuilding(BuildingBlueprint duplicate=null)
        {
            if(duplicate!=null) CheckBuilding(duplicate);if(workshops.Count>=MaxBuildingBlueprints) return null;
            string label=duplicate!=null?duplicate.DisplayName:"Workshop";label=label.Substring(0,Mathf.Min(54,label.Length))+(duplicate!=null?" copy":" "+(workshops.Count+1));
            var building=CreateBuilding("building-"+Guid.NewGuid().ToString("N"),label);
            if(duplicate!=null) building.ConfigureOptions(building.Id,building.Prefab,label,duplicate.ProductionOptions);
            Refresh();return building;
        }
        private BuildingBlueprint CreateBuilding(string id,string label)
        {
            var building=ScriptableObject.CreateInstance<BuildingBlueprint>();owned.Add(building);workshops.Add(building);building.name=label;
            building.ConfigureOptions(id,buildingRecipe,label,Array.Empty<UnitBlueprint>());return building;
        }
        public void RenameBuilding(BuildingBlueprint building,string label)
        {
            CheckBuilding(building);label=label??"";label=label.Substring(0,Mathf.Min(64,label.Length));building.name=label;
            building.ConfigureOptions(building.Id,building.Prefab,label,building.ProductionOptions);Refresh();
        }
        public void SetBuildPermission(UnitBlueprint unit,BuildingBlueprint building,bool enabled)
        {
            Check(unit);CheckBuilding(building);var links=unit.Builds.ToList();links.Remove(building);if(enabled) links.Add(building);
            unit.Configure(unit.Id,unit.DisplayName,unit.Production,unit.GathersSupplies,links.ToArray());Refresh();
        }
        public void SetTraining(BuildingBlueprint building,UnitBlueprint unit,bool enabled)
        {
            CheckBuilding(building);Check(unit);var links=building.ProductionOptions.ToList();links.Remove(unit);if(enabled) links.Add(unit);
            building.ConfigureOptions(building.Id,building.Prefab,building.DisplayName,links);Refresh();
        }
        public bool RemoveBuilding(BuildingBlueprint building)
        {
            CheckBuilding(building);if(workshops.Count==1) return false;
            foreach(var unit in units) unit.Configure(unit.Id,unit.DisplayName,unit.Production,unit.GathersSupplies,unit.Builds.Where(x=>x!=building).ToArray());
            workshops.Remove(building);owned.Remove(building);UnityEngine.Object.Destroy(building);Refresh();return true;
        }
        internal void RestoreGraph(FactionUnitRecord[] records,FactionBuildingRecord[] buildings)
        {
            // Validated records are restored into a new, unexposed draft.
            foreach(var unit in units){owned.Remove(unit);UnityEngine.Object.Destroy(unit);}units.Clear();starts.Clear();
            foreach(var building in workshops){owned.Remove(building);UnityEngine.Object.Destroy(building);}workshops.Clear();
            foreach(var record in buildings) CreateBuilding(record.Id,record.Name);
            foreach(var record in records)
            {
                var unit=CreateUnit(record.Id,record.Name,record.Gathers,false,record.Start);
                unit.Configure(unit.Id,unit.DisplayName,recipe,record.Gathers,record.BuildIds.Select(id=>workshops.Single(x=>x.Id==id)).ToArray());
            }
            foreach(var record in buildings)
            {
                var building=workshops.Single(x=>x.Id==record.Id);
                building.ConfigureOptions(building.Id,building.Prefab,record.Name,record.Trains.Select(id=>units.Single(x=>x.Id==id)));
            }
            Refresh();
        }
        public void Refresh()
        {
            Definition.Configure(Definition.DisplayName,Definition.StartingBase,Definition.StartingSupplies,
                units.Where(x=>StartingCount(x)>0).Select(x=>new StartingUnit(x,StartingCount(x))).ToArray(),units.ToArray(),new[]{Definition.StartingBase}.Concat(workshops).ToArray());
            Report=CivilizationValidator.Validate(Definition);
            if(!ValidName(Definition.DisplayName)) Report.Errors.Add("Give your faction a name with 1–64 printable characters.");
            foreach(var unit in units) if(!ValidName(unit.DisplayName)) Report.Errors.Add("Give every unit blueprint a name with 1–64 printable characters.");
            foreach(var building in workshops) if(!ValidName(building.DisplayName)) Report.Errors.Add("Give every building blueprint a name with 1–64 printable characters.");
            if(workshops.Select(x=>x.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=workshops.Count) Report.Warnings.Add("Some building blueprints share a name; distinct names improve clarity.");
            if(units.Select(x=>x.DisplayName).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=units.Count) Report.Warnings.Add("Some unit blueprints share a name. Distinct names will make them easier to identify.");
        }
        public static bool ValidName(string value)=>!string.IsNullOrWhiteSpace(value) && value.Length<=64 && !value.Any(char.IsControl);
        public void Dispose(){foreach(var asset in owned) if(asset!=null) UnityEngine.Object.Destroy(asset);owned.Clear();}
    }
}
