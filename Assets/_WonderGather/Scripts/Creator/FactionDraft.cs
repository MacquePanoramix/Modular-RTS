using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WonderGather
{
    // Owns only editable copies. Prefabs and cost recipes remain shared, read-only references.
    public sealed class FactionDraft : IDisposable
    {
        private readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        public CivilizationDefinition Definition {get;}
        public UnitBlueprint Worker {get;}
        public BuildingBlueprint Workshop {get;}
        public int StartingWorkers {get;private set;}
        public CivilizationReport Report {get;private set;}
        public FactionDraft(CivilizationDefinition source)
        {
            if(!CivilizationValidator.Validate(source).CanInstantiate) throw new ArgumentException("The creator example must be a valid civilization.");
            var units=source.Units.ToDictionary(x=>x,x=>Copy(x));
            var buildings=source.Buildings.ToDictionary(x=>x,x=>Copy(x));
            foreach(var pair in units) pair.Value.Configure(pair.Key.Id,pair.Key.DisplayName,pair.Key.Production,pair.Key.GathersSupplies,pair.Key.Builds.Select(b=>buildings[b]).ToArray());
            foreach(var pair in buildings) pair.Value.Configure(pair.Key.Id,pair.Key.Prefab,pair.Key.Produces!=null?units[pair.Key.Produces]:null);
            Definition=Copy(source);
            Definition.Configure(source.DisplayName,buildings[source.StartingBase],source.StartingSupplies,
                source.StartingUnits.Select(x=>new StartingUnit(units[x.blueprint],x.count)).ToArray(),units.Values.ToArray(),buildings.Values.ToArray());
            Worker=Definition.Units[0];Workshop=Definition.Buildings.First(x=>x!=Definition.StartingBase);
            StartingWorkers=Definition.StartingUnits.Where(x=>x.blueprint==Worker).Sum(x=>x.count);
            Refresh();
        }
        private T Copy<T>(T source) where T:UnityEngine.Object
        {var clone=UnityEngine.Object.Instantiate(source);clone.name=source.name+" (draft)";owned.Add(clone);return clone;}
        public void SetStartingSetup(string name,int workers,int supplies)
        {
            // Bounds are for this small test map, not a faction design budget.
            StartingWorkers=Mathf.Clamp(workers,0,8);
            Definition.Configure((name??"").Substring(0,Mathf.Min(64,(name??"").Length)),Definition.StartingBase,Mathf.Clamp(supplies,0,120),
                StartingWorkers==0?Array.Empty<StartingUnit>():new[]{new StartingUnit(Worker,StartingWorkers)},Definition.Units.ToArray(),Definition.Buildings.ToArray());
            Refresh();
        }
        public void SetWorkerPermissions(bool gathers,bool builds)
        {Worker.Configure(Worker.Id,Worker.DisplayName,Worker.Production,gathers,builds?new[]{Workshop}:Array.Empty<BuildingBlueprint>());Refresh();}
        public void SetProduction(bool enabled)
        {Workshop.Configure(Workshop.Id,Workshop.Prefab,enabled?Worker:null);Refresh();}
        public void Refresh()
        {
            Report=CivilizationValidator.Validate(Definition);
            if(string.IsNullOrWhiteSpace(Definition.DisplayName)) Report.Errors.Add("Give your faction a name before playtesting.");
        }
        public void Dispose(){foreach(var asset in owned) if(asset!=null) UnityEngine.Object.Destroy(asset);owned.Clear();}
    }
}
