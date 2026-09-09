using System;
using System.Collections.Generic;
using UnityEngine;
namespace WonderGather
{
    [Serializable]
    public struct StartingUnit
    {
        public UnitBlueprint blueprint;
        [Min(1)] public int count;
        public StartingUnit(UnitBlueprint type,int quantity){blueprint=type;count=quantity;}
    }
    [CreateAssetMenu(menuName="Wonder Gather/Civilization/Civilization")]
    public sealed class CivilizationDefinition : ScriptableObject
    {
        [SerializeField] private string displayName="Little Settlement";
        [SerializeField] private BuildingBlueprint startingBase;
        [SerializeField,Min(0)] private int startingSupplies;
        [SerializeField] private StartingUnit[] startingUnits=Array.Empty<StartingUnit>();
        [SerializeField] private UnitBlueprint[] units=Array.Empty<UnitBlueprint>();
        [SerializeField] private BuildingBlueprint[] buildings=Array.Empty<BuildingBlueprint>();
        public string DisplayName=>displayName;
        public BuildingBlueprint StartingBase=>startingBase;
        public int StartingSupplies=>startingSupplies;
        public IReadOnlyList<StartingUnit> StartingUnits=>startingUnits;
        public IReadOnlyList<UnitBlueprint> Units=>units;
        public IReadOnlyList<BuildingBlueprint> Buildings=>buildings;
        public void Configure(string label,BuildingBlueprint home,int supplies,StartingUnit[] starts,UnitBlueprint[] roster,BuildingBlueprint[] structures)
        {displayName=label;startingBase=home;startingSupplies=supplies;startingUnits=starts??Array.Empty<StartingUnit>();units=roster??Array.Empty<UnitBlueprint>();buildings=structures??Array.Empty<BuildingBlueprint>();}
    }
}
