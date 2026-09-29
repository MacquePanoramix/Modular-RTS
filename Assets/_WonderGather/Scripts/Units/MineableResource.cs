using UnityEngine;

namespace WonderGather
{
    // Explicit opt-in: legacy supplies retain their original collection contract.
    [RequireComponent(typeof(ResourceNode), typeof(ResourceWorkplace))]
    public sealed class MineableResource : MonoBehaviour
    {
        [SerializeField] private Collider surface;
        [SerializeField] private string requiredToolId = "pickaxe";
        public Collider Surface => surface;
        public bool Available => isActiveAndEnabled && surface != null && surface.enabled && surface.gameObject.activeInHierarchy;
        public bool Accepts(ToolDefinition tool) => Available && tool != null && tool.IsValid && tool.Id == requiredToolId;
        public void Configure(Collider value)
        {
            if (value == null || !value.transform.IsChildOf(transform))
                throw new System.ArgumentException("The mining surface must belong to this resource.");
            surface=value;
        }
    }
}
