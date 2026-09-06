using UnityEngine;

namespace WonderGather
{
    [RequireComponent(typeof(UnitMotor))]
    public sealed class SelectableUnit : MonoBehaviour
    {
        [SerializeField] private GameObject selectionRing;
        public UnitMotor Motor { get; private set; }
        private void Awake() { Motor = GetComponent<UnitMotor>(); SetSelected(false); }
        public void Configure(GameObject ring) { selectionRing = ring; SetSelected(false); }
        public void SetSelected(bool selected) { if (selectionRing != null) selectionRing.SetActive(selected); }
        private void OnDisable() => SetSelected(false);
    }
}
