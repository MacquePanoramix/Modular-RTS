using UnityEngine;
namespace WonderGather
{
    public sealed class ProducedWorker : MonoBehaviour
    {
        private SelectionController roster;
        private SelectableUnit unit;
        public void Configure(SelectionController owner)
        {roster=owner;unit=GetComponent<SelectableUnit>();roster.RegisterUnit(unit);}
        private void OnDestroy(){if(roster!=null) roster.UnregisterUnit(unit);}
    }
}
