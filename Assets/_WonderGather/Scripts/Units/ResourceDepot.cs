using UnityEngine;
namespace WonderGather
{
    public sealed class ResourceDepot : MonoBehaviour
    {
        public int Stored { get; private set; }
        public bool Deposit(int amount)
        {
            if (!isActiveAndEnabled || amount <= 0) return false;
            Stored += amount;
            return true;
        }
    }
}
