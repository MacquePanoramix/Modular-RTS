using UnityEngine;
namespace WonderGather
{
    public sealed class FactionPlaytestBridge : MonoBehaviour
    {
        [SerializeField] private CivilizationSession session;
        [SerializeField] private RtsInput input;
        public CivilizationSession Session=>session;
        public void SetInputEnabled(bool enabled)=>input.enabled=enabled;
        public void Configure(CivilizationSession value,RtsInput controls){session=value;input=controls;}
        public bool Begin(CivilizationDefinition draft,System.Func<Vector2,bool> blocksPointer)
        {input.SetInterfaceBlocker(blocksPointer);return session.InitializeFrom(draft);}
    }
}
