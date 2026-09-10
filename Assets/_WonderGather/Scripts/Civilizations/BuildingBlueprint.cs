using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace WonderGather
{
    [CreateAssetMenu(menuName="Wonder Gather/Civilization/Building blueprint")]
    public sealed class BuildingBlueprint : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private BuildingSite prefab;
        // Preserve the authored one-recipe field and its existing references.
        [SerializeField] private UnitBlueprint produces;
        [SerializeField] private UnitBlueprint[] additionalProduction=Array.Empty<UnitBlueprint>();
        [SerializeField] private string displayName;
        [SerializeField] private bool customName;
        public string Id=>id;
        public BuildingSite Prefab=>prefab;
        public BuildingDefinition Construction=>prefab!=null?prefab.Definition:null;
        public string DisplayName=>customName?displayName:(Construction!=null?Construction.DisplayName:name);
        public UnitBlueprint Produces=>produces;
        public IEnumerable<UnitBlueprint> ProductionOptions
        {
            get {if(produces!=null) yield return produces;foreach(var unit in additionalProduction??Array.Empty<UnitBlueprint>()) yield return unit;}
        }
        public void Configure(string key,BuildingSite template,UnitBlueprint unit=null)
        {id=key;prefab=template;produces=unit;additionalProduction=Array.Empty<UnitBlueprint>();displayName=null;customName=false;}
        public void ConfigureOptions(string key,BuildingSite template,string label,IEnumerable<UnitBlueprint> units)
        {
            var options=units.ToArray();id=key;prefab=template;displayName=label;customName=true;
            produces=options.FirstOrDefault();additionalProduction=options.Skip(1).ToArray();
        }
    }
}
