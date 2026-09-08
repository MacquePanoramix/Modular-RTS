using UnityEngine;
namespace WonderGather
{
    [CreateAssetMenu(menuName="Wonder Gather/Worker production")]
    public sealed class WorkerProductionDefinition : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField,Min(1)] private int cost=10;
        [SerializeField,Min(.1f)] private float seconds=6;
        public GameObject Prefab=>prefab;
        public int Cost=>cost;
        public float Seconds=>seconds;
        public void Configure(GameObject worker)=>prefab=worker;
    }
}
