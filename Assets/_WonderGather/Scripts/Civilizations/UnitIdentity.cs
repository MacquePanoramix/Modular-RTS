using UnityEngine;
namespace WonderGather
{
    public sealed class UnitIdentity : MonoBehaviour
    {
        public UnitBlueprint Blueprint {get;private set;}
        public void Configure(UnitBlueprint data)
        {
            if(data==null) throw new System.ArgumentNullException(nameof(data));
            data.Performance.Validate();
            if(data.Tool!=null && !data.Tool.IsValid) throw new System.ArgumentException("The blueprint has an invalid tool definition.");
            Blueprint=data;
            var equipment=GetComponent<EquippedTool>();
            if(equipment==null && data.Tool!=null) equipment=gameObject.AddComponent<EquippedTool>();
            if(equipment!=null) equipment.SetDefinition(data.Tool);
            if(TryGetComponent<UnitMotor>(out var motor)) motor.SetMovementRate(data.Performance.movementPercent/100f);
            if(TryGetComponent<Gatherer>(out var gatherer)) gatherer.SetPerformance(data.Performance);
            if(TryGetComponent<Builder>(out var builder)) builder.SetWorkRate(data.Performance.constructionPercent/100f);
        }
        public static void Apply(GameObject unit,UnitBlueprint data)
        {
            var identity=unit.GetComponent<UnitIdentity>();
            if(identity==null) identity=unit.AddComponent<UnitIdentity>();
            identity.Configure(data);
            unit.name=data.DisplayName;
        }
    }
}
