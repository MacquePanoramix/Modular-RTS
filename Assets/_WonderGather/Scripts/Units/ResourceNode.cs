using UnityEngine;
namespace WonderGather
{
    public sealed class ResourceNode : MonoBehaviour
    {
        [SerializeField, Min(0)] private int remaining = 120;
        public int Remaining => remaining;
        public int Take(int requested)
        {
            if (!isActiveAndEnabled) return 0;
            int amount = Mathf.Clamp(requested, 0, remaining);
            remaining -= amount;
            return amount;
        }
    }
}
