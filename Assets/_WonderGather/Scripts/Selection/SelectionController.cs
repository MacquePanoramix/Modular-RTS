using System.Collections.Generic;
using UnityEngine;

namespace WonderGather
{
    public sealed class SelectionController : MonoBehaviour
    {
        [SerializeField] private RtsInput input;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Transform destinationMarker;
        [SerializeField] private LayerMask worldMask = (1 << 0) | (1 << 6);
        [SerializeField] private SelectableUnit[] availableUnits = new SelectableUnit[0];
        [SerializeField] private float formationSpacing = 2.4f;
        private readonly List<SelectableUnit> selected = new List<SelectableUnit>();
        private readonly CommandDispatcher commands = new CommandDispatcher();
        private ConstructionController construction;
        private void Awake() => construction=GetComponent<ConstructionController>();
        private float markerUntil;
        private bool selecting, dragging, additiveAtPress;
        private Vector2 start, end;
        public IReadOnlyList<SelectableUnit> SelectedUnits => selected.AsReadOnly();
        public int Count => selected.Count;
        public SelectableUnit Selected => Count > 0 ? selected[0] : null;
        public Vector3 Center
        {
            get
            {
                var center = Vector3.zero;
                foreach (var unit in selected) if (unit != null) center += unit.transform.position;
                return Count > 0 ? center / Count : center;
            }
        }
        public bool IsDragging => selecting && dragging;
        public Rect DragRect => Rect.MinMaxRect(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y));
        public string Status { get; private set; } = "Click or drag to select units.";
        public void ReportStatus(string message) => Status=message;
        public void Configure(RtsInput source, Camera camera, Transform marker)
        { input = source; worldCamera = camera; destinationMarker = marker; }
        public void ConfigureUnits(SelectableUnit[] units) => availableUnits = units;

        public void Select(SelectableUnit unit) => Select(unit, false);
        public void Select(SelectableUnit unit, bool toggle)
        {
            if (!toggle) ClearSelection();
            if (unit != null && unit.isActiveAndEnabled)
            {
                if (toggle && selected.Remove(unit)) unit.SetSelected(false);
                else Add(unit);
            }
            SelectionChanged();
        }
        private void Add(SelectableUnit unit)
        {
            if (selected.Contains(unit)) return;
            selected.Add(unit);
            unit.SetSelected(true);
        }
        private void ClearSelection()
        {
            foreach (var unit in selected) if (unit != null) unit.SetSelected(false);
            selected.Clear();
        }
        private void SelectionChanged()
        {
            if (destinationMarker != null) destinationMarker.gameObject.SetActive(false);
            Status = Count == 0 ? "No units selected." : "Right-click open ground to move the selection.";
        }
        // Rect uses screen coordinates with bottom-left origin, independent of drag direction.
        public void SelectBox(Rect rect, bool additive)
        {
            if (!additive) ClearSelection();
            foreach (var unit in availableUnits)
            {
                if (unit == null || !unit.isActiveAndEnabled) continue;
                var point = worldCamera.WorldToScreenPoint(unit.transform.position + Vector3.up);
                if (point.z > 0 && point.x >= 0 && point.x < Screen.width && point.y >= 0 && point.y < Screen.height
                    && rect.Contains(new Vector2(point.x, point.y))) Add(unit);
            }
            SelectionChanged();
        }
        public bool MoveSelection(Vector3 destination)
        {
            if (Count == 0) return false;
            bool accepted = commands.Dispatch(new GroupMoveCommand(destination, formationSpacing), selected);
            if (destinationMarker != null)
            {
                destinationMarker.gameObject.SetActive(accepted);
                if (accepted)
                {
                    var resolved = Vector3.zero;
                    foreach (var unit in selected) resolved += unit.Motor.Destination;
                    destinationMarker.position = resolved / Count + Vector3.up * .07f;
                }
            }
            markerUntil = Time.unscaledTime + 2;
            Status = accepted ? "Moving toward the nearest available destinations." : "No reachable space for this group.";
            return accepted;
        }
        public int GatherSelection(ResourceNode resource)
        {
            int accepted = commands.Dispatch(new GatherCommand(resource), selected);
            SelectionChanged();
            Status = accepted > 0 ? accepted + " workers gathering. Deliveries repeat automatically." : "No selected worker can gather there.";
            return accepted;
        }
        private void Update()
        {
            if(construction!=null)
            {
                if(input.BuildPressed) {selecting=false;if(construction.Placing) construction.CancelPlacement();else construction.BeginPlacement();return;}
                if(construction.Placing) {selecting=false;construction.HandleInput(input,worldCamera);return;}
            }
            for (int i = selected.Count - 1; i >= 0; i--)
                if (selected[i] == null || !selected[i].isActiveAndEnabled) { selected.RemoveAt(i); SelectionChanged(); }
            if (input.ClearPressed) { selecting = false; Select(null); }
            if (destinationMarker != null && destinationMarker.gameObject.activeSelf && Time.unscaledTime > markerUntil)
                destinationMarker.gameObject.SetActive(false);
            if (input.SelectPressed)
            { selecting = true; dragging = false; additiveAtPress = input.Additive; start = end = input.Pointer; }
            if (selecting)
            {
                end = input.Pointer;
                dragging |= (end - start).sqrMagnitude > 36;
                if (input.SelectReleased)
                {
                    if (input.CanSelectWorld)
                    {
                        if (dragging) SelectBox(DragRect, additiveAtPress);
                        else
                        {
                            bool hit = Physics.Raycast(worldCamera.ScreenPointToRay(end), out var info, 500, worldMask, QueryTriggerInteraction.Ignore);
                            Select(hit ? info.collider.GetComponentInParent<SelectableUnit>() : null, additiveAtPress);
                        }
                    }
                    selecting = false;
                }
                else if (!input.SelectHeld) selecting = false;
            }
            if (input.MovePressed && Count > 0)
            {
                selecting = false;
                if (Physics.Raycast(worldCamera.ScreenPointToRay(input.Pointer), out var hit, 500, worldMask, QueryTriggerInteraction.Ignore))
                {
                    var node = hit.collider.GetComponentInParent<ResourceNode>();
                    if (node != null) GatherSelection(node);
                    else if (hit.collider.GetComponentInParent<ResourceDepot>() is ResourceDepot depot)
                    {
                        int accepted = commands.Dispatch(new ReturnSuppliesCommand(depot), selected);
                        SelectionChanged(); Status = accepted > 0 ? "Returning carried supplies." : "Selected workers have no supplies to return.";
                    }
                    else if(construction!=null && hit.collider.GetComponentInParent<BuildingSite>() is BuildingSite site) construction.Resume(site);
                    else MoveSelection(hit.point);
                }
                else { SelectionChanged(); Status = "Right-click terrain or a resource."; }
            }
        }
        private void OnApplicationFocus(bool focus) { if (!focus) selecting = false; }
        private void OnDisable() { selecting = false; ClearSelection(); SelectionChanged(); }
    }
}
