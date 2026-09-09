using UnityEngine;
namespace WonderGather
{
    public sealed class UnitIdentity : MonoBehaviour
    {
        public UnitBlueprint Blueprint {get;private set;}
        public void Configure(UnitBlueprint data)=>Blueprint=data;
        public static void Apply(GameObject unit,UnitBlueprint data)
        {
            var identity=unit.GetComponent<UnitIdentity>();
            if(identity==null) identity=unit.AddComponent<UnitIdentity>();
            identity.Configure(data);
            unit.name=data.DisplayName;
        }
    }
}
