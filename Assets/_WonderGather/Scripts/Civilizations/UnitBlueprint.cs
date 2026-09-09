using System;
using System.Collections.Generic;
using UnityEngine;
namespace WonderGather
{
    [CreateAssetMenu(menuName="Wonder Gather/Civilization/Unit blueprint")]
    public sealed class UnitBlueprint : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName="Worker";
        [SerializeField] private WorkerProductionDefinition production;
        [SerializeField] private bool gathersSupplies=true;
        [SerializeField] private BuildingBlueprint[] builds=Array.Empty<BuildingBlueprint>();
        public string Id=>id;
        public string DisplayName=>displayName;
        public WorkerProductionDefinition Production=>production;
        public bool GathersSupplies=>gathersSupplies;
        public IReadOnlyList<BuildingBlueprint> Builds=>builds;
        public bool CanBuild(BuildingBlueprint building)=>building!=null && Array.IndexOf(builds,building)>=0;
        public void Configure(string key,string label,WorkerProductionDefinition recipe,bool gather,params BuildingBlueprint[] permissions)
        {id=key;displayName=label;production=recipe;gathersSupplies=gather;builds=permissions??Array.Empty<BuildingBlueprint>();}
    }
}
