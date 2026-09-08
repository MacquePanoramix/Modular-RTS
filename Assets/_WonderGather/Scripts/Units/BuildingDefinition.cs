using UnityEngine;
namespace WonderGather
{
    [CreateAssetMenu(menuName="Wonder Gather/Building")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Workshop";
        [SerializeField,Min(1)] private int cost = 20;
        [SerializeField,Min(.1f)] private float seconds = 8;
        [SerializeField,Min(1)] private float size = 3;
        public string DisplayName => displayName;
        public int Cost => cost;
        public float Seconds => seconds;
        public float Size => size;
    }
}
