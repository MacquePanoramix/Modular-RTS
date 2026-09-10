using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace WonderGather
{
    public sealed class CivilizationReport
    {
        public readonly List<string> Errors=new List<string>();
        public readonly List<string> Warnings=new List<string>();
        public readonly HashSet<UnitBlueprint> ReachableUnits=new HashSet<UnitBlueprint>();
        public readonly HashSet<BuildingBlueprint> ReachableBuildings=new HashSet<BuildingBlueprint>();
        public bool CanInstantiate=>Errors.Count==0;
    }
    // Structural fixed point: cycles are allowed, but cannot seed themselves.
    // Economy warnings are deliberately not a solvability or balance proof.
    public static class CivilizationValidator
    {
        public static CivilizationReport Validate(CivilizationDefinition civilization)
        {
            var report=new CivilizationReport();
            if(civilization==null){report.Errors.Add("Choose a civilization asset.");return report;}
            var units=new HashSet<UnitBlueprint>();var buildings=new HashSet<BuildingBlueprint>();var ids=new HashSet<string>();
            foreach(var unit in civilization.Units)
            {
                if(unit==null){report.Errors.Add("The unit roster contains an empty reference.");continue;}
                if(!units.Add(unit)) report.Errors.Add("Duplicate unit in roster: "+unit.name);
                CheckId(unit.Id,unit.name,ids,report);
                var recipe=unit.Production;var prefab=recipe!=null?recipe.Prefab:null;
                if(prefab==null || !prefab.activeSelf || prefab.GetComponent<SelectableUnit>()==null || prefab.GetComponent<UnitMotor>()==null || prefab.GetComponent<NavMeshAgent>()==null || prefab.GetComponent<ProducedWorker>()==null || prefab.GetComponent<Gatherer>()==null || prefab.GetComponent<Builder>()==null)
                    report.Errors.Add(unit.name+": this prototype requires a worker prefab with selection, navigation, gathering, building and roster components.");
                if(recipe!=null && (recipe.Cost<=0 || !float.IsFinite(recipe.Seconds) || recipe.Seconds<=0)) report.Errors.Add(unit.name+": production cost and time must be positive.");
            }
            foreach(var building in civilization.Buildings)
            {
                if(building==null){report.Errors.Add("The building roster contains an empty reference.");continue;}
                if(!buildings.Add(building)) report.Errors.Add("Duplicate building in roster: "+building.name);
                CheckId(building.Id,building.name,ids,report);
                var data=building.Construction;
                if(building.Prefab==null || !building.Prefab.gameObject.activeSelf || !building.Prefab.enabled || !building.Prefab.HasVisual || data==null) report.Errors.Add(building.name+": missing or inactive site prefab, visual or construction definition.");
                else if(data.Cost<=0 || !float.IsFinite(data.Seconds) || data.Seconds<=0 || !float.IsFinite(data.Size) || data.Size<1) report.Errors.Add(building.name+": invalid construction cost, time or footprint.");
                var produced=new HashSet<UnitBlueprint>();
                foreach(var option in building.ProductionOptions)
                    if(option==null || !units.Contains(option) || !produced.Add(option) || building.Prefab==null || building.Prefab.GetComponent<UnitProducer>()==null)
                        report.Errors.Add(building.name+": production options must be distinct roster units and require a prefab with UnitProducer.");
            }
            if(civilization.StartingBase==null || !buildings.Contains(civilization.StartingBase) || civilization.StartingBase.Prefab==null || civilization.StartingBase.Prefab.GetComponent<ResourceDepot>()==null)
                report.Errors.Add("Starting base must belong to the building roster and have a supply depot.");
            else report.ReachableBuildings.Add(civilization.StartingBase);
            if(civilization.StartingSupplies<0) report.Errors.Add("Starting supplies cannot be negative.");
            foreach(var start in civilization.StartingUnits)
            {
                if(start.blueprint==null || !units.Contains(start.blueprint) || start.count<1) report.Errors.Add("Starting units need a roster blueprint and a positive count.");
                else report.ReachableUnits.Add(start.blueprint);
            }
            foreach(var unit in units) foreach(var building in unit.Builds)
                if(building==null || !buildings.Contains(building)) report.Errors.Add(unit.name+": a construction permission is missing or outside this civilization's building roster.");
            bool changed;
            do
            {
                changed=false;
                foreach(var unit in units) if(report.ReachableUnits.Contains(unit))
                    foreach(var building in unit.Builds) if(building!=null && buildings.Contains(building)) changed|=report.ReachableBuildings.Add(building);
                foreach(var building in buildings) if(report.ReachableBuildings.Contains(building))
                    foreach(var option in building.ProductionOptions) if(option!=null && units.Contains(option)) changed|=report.ReachableUnits.Add(option);
            }while(changed);
            bool gatherer=false;int highestCost=0;
            foreach(var unit in units)
            {
                if(!report.ReachableUnits.Contains(unit)) report.Warnings.Add(unit.DisplayName+": unreachable from the starting setup; add a starting unit or a reachable production link.");
                else {gatherer|=unit.GathersSupplies;if(unit.Production!=null) highestCost=Mathf.Max(highestCost,unit.Production.Cost);}
            }
            foreach(var building in buildings)
            {
                if(!report.ReachableBuildings.Contains(building)) report.Warnings.Add(building.DisplayName+": unreachable; no reachable builder can construct it.");
                else if(building!=civilization.StartingBase && building.Construction!=null) highestCost=Mathf.Max(highestCost,building.Construction.Cost);
            }
            if(!gatherer) report.Warnings.Add("No reachable unit can gather supplies. Growth depends on starting stock and may stop permanently.");
            if(!gatherer && highestCost>civilization.StartingSupplies) report.Warnings.Add("A reachable cost exceeds starting supplies, with no reachable supply gatherer.");
            bool initialGatherer=false;
            foreach(var start in civilization.StartingUnits) if(start.blueprint!=null && start.count>0) initialGatherer|=start.blueprint.GathersSupplies;
            if(!initialGatherer && civilization.StartingSupplies==0) report.Warnings.Add("No starting gatherer or supplies: paid progression cannot begin, even if links are structurally reachable.");
            return report;
        }
        private static void CheckId(string id,string label,HashSet<string> ids,CivilizationReport report)
        {
            if(string.IsNullOrWhiteSpace(id)) report.Errors.Add(label+": assign a stable ID.");
            else if(!ids.Add(id)) report.Errors.Add("Duplicate blueprint ID: "+id);
        }
    }
}
