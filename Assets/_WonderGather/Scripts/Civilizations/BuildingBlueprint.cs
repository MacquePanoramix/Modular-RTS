using UnityEngine;
namespace WonderGather
{
    [CreateAssetMenu(menuName="Wonder Gather/Civilization/Building blueprint")]
    public sealed class BuildingBlueprint : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private BuildingSite prefab;
        [SerializeField] private UnitBlueprint produces;
        public string Id=>id;
        public BuildingSite Prefab=>prefab;
        public BuildingDefinition Construction=>prefab!=null?prefab.Definition:null;
        public string DisplayName=>Construction!=null?Construction.DisplayName:name;
        public UnitBlueprint Produces=>produces;
        public void Configure(string key,BuildingSite template,UnitBlueprint unit=null){id=key;prefab=template;produces=unit;}
    }
}
