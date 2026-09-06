using UnityEngine;

namespace WonderGather
{
    public sealed class SelectionController : MonoBehaviour
    {
        [SerializeField] private RtsInput input;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform destinationMarker;
        [SerializeField] private LayerMask worldMask = (1 << 0) | (1 << 6);
        private SelectableUnit selected;
        private readonly CommandDispatcher commands = new CommandDispatcher();
        private float markerUntil;
        public SelectableUnit Selected => selected != null && selected.isActiveAndEnabled ? selected : null;
        public string Status { get; private set; } = "Select the Wanderer to begin.";
        public void Configure(RtsInput source, Camera camera, Transform marker)
        { input = source; worldCamera = camera; destinationMarker = marker; }
        public void Select(SelectableUnit unit)
        {
            if (selected != null) selected.SetSelected(false);
            selected = unit != null && unit.isActiveAndEnabled ? unit : null;
            if (selected != null) selected.SetSelected(true);
            destinationMarker.gameObject.SetActive(false);
            Status = Selected == null ? "No unit selected." : "Wanderer selected. Right-click the ground to move.";
        }
        private void Update()
        {
            if (selected != null && !selected.isActiveAndEnabled) Select(null);
            if (input.ClearPressed) Select(null);
            if (destinationMarker.gameObject.activeSelf && Time.unscaledTime > markerUntil)
                destinationMarker.gameObject.SetActive(false);
            if (!input.SelectPressed && !input.MovePressed) return;
            bool hitWorld = Physics.Raycast(worldCamera.ScreenPointToRay(input.Pointer), out var hit, 500, worldMask, QueryTriggerInteraction.Ignore);
            if (input.SelectPressed) Select(hitWorld ? hit.collider.GetComponentInParent<SelectableUnit>() : null);
            if (!input.MovePressed || Selected == null) return;
            if (hitWorld && hit.collider.gameObject.layer == 6 && commands.Dispatch(new MoveCommand(hit.point), Selected.Motor))
            {
                destinationMarker.position = Selected.Motor.Destination + Vector3.up * .07f;
                destinationMarker.gameObject.SetActive(true);
                markerUntil = Time.unscaledTime + 2;
                Status = "Moving.";
            }
            else Status = "That destination is blocked or unreachable.";
        }
        private void OnDisable()
        {
            if (selected != null) selected.SetSelected(false);
            selected = null;
            if (destinationMarker != null) destinationMarker.gameObject.SetActive(false);
        }
    }
}
