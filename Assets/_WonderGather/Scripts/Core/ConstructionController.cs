using UnityEngine;
using UnityEngine.AI;
namespace WonderGather
{
    public sealed class ConstructionController : MonoBehaviour
    {
        [SerializeField] private SelectionController selection;
        [SerializeField] private ResourceDepot depot;
        [SerializeField] private BuildingSite prefab;
        [SerializeField] private BuildingDefinition definition;
        [SerializeField] private Material validMaterial, invalidMaterial;
        private GameObject preview;
        private Renderer previewRenderer;
        private Vector3 candidate;
        private bool valid;
        private readonly CommandDispatcher commands=new CommandDispatcher();
        public bool Placing { get; private set; }
        public BuildingSite LastSite { get; private set; }
        private string status="Select a worker and press B to place a workshop.";
        public string Status {get=>status;private set {status=value;if(selection!=null) selection.ReportStatus(value);}}
        public string BuildHint => "B: place " + definition.DisplayName + " (" + definition.Cost + " supplies) / right-click site: resume";
        public string ProgressText => LastSite==null ? "" : LastSite.Complete ? "Workshop complete." : "Workshop: " + Mathf.RoundToInt(LastSite.Progress*100) + "%" + (LastSite.Worker==null ? " (paused; right-click to resume)" : "");
        public void Configure(SelectionController selected, ResourceDepot bank, BuildingSite building, BuildingDefinition data, Material good, Material bad)
        {selection=selected;depot=bank;prefab=building;definition=data;validMaterial=good;invalidMaterial=bad;}
        private Builder SelectedBuilder()
        {
            foreach(var unit in selection.SelectedUnits)
                if(unit!=null && unit.TryGetComponent<Builder>(out var worker) && worker.isActiveAndEnabled) return worker;
            return null;
        }
        public bool BeginPlacement()
        {
            if(SelectedBuilder()==null) {Status="Select a worker first.";return false;}
            if(depot==null || !depot.isActiveAndEnabled || depot.Stored<definition.Cost) {Status="Need " + definition.Cost + " stored supplies.";return false;}
            Placing=true;valid=false;Status="Left-click green preview to place. Esc / right-click cancels.";
            if(preview==null)
            {
                preview=GameObject.CreatePrimitive(PrimitiveType.Cube);preview.name="Building preview";
                var collider=preview.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
                previewRenderer=preview.GetComponent<Renderer>();
                preview.transform.localScale=new Vector3(definition.Size,.15f,definition.Size);
            }
            preview.SetActive(false);return true;
        }
        public bool CanPlace(Vector3 point)
        {
            if(!float.IsFinite(point.x)||!float.IsFinite(point.y)||!float.IsFinite(point.z)||depot==null||!depot.isActiveAndEnabled||depot.Stored<definition.Cost) return false;
            var worker=SelectedBuilder();if(worker==null) return false;
            float half=definition.Size*.5f;
            // Confirm the whole footprint rests on the flat walkable surface.
            for(int x=-1;x<=1;x++) for(int z=-1;z<=1;z++)
            {
                var sample=point+new Vector3(x*half,0,z*half);
                if(!NavMesh.SamplePosition(sample,out var hit,.2f,NavMesh.AllAreas) || (hit.position-sample).sqrMagnitude>.04f) return false;
            }
            if(Physics.CheckBox(point+Vector3.up*1.5f,new Vector3(half+.2f,1.4f,half+.2f),Quaternion.identity,1,QueryTriggerInteraction.Ignore)) return false;
            return worker.Plan(point,definition.Size,out _,out _);
        }
        public bool TryPlace(Vector3 point)
        {
            if(!CanPlace(point)) {Status="Cannot build here: check funds, open ground and access.";return false;}
            var worker=SelectedBuilder();
            if(!depot.TrySpend(definition.Cost)) return false;
            var site=Instantiate(prefab,point,Quaternion.identity);
            if(!commands.Dispatch(new BuildCommand(site),worker))
            {site.gameObject.SetActive(false);Destroy(site.gameObject);depot.Deposit(definition.Cost);return false;}
            if(site.TryGetComponent<UnitProducer>(out var producer)) producer.Configure(depot,selection,worker.GetComponent<UnitMotor>().Destination);
            LastSite=site;CancelPlacement();Status="Workshop placed. Worker assigned.";return true;
        }
        public void Resume(BuildingSite site)
        {
            var worker=SelectedBuilder();
            bool accepted=commands.Dispatch(new BuildCommand(site),worker);
            if(accepted) LastSite=site;
            Status=accepted ? "Worker assigned to construction." : "Select an available worker and an unfinished site.";
        }
        public void HandleInput(RtsInput input, Camera camera)
        {
            if(!Placing) return;
            if(input.ClearPressed || input.MovePressed) {CancelPlacement();return;}
            valid=false;
            if(input.CanSelectWorld && Physics.Raycast(camera.ScreenPointToRay(input.Pointer),out var hit,500,1<<6,QueryTriggerInteraction.Ignore))
            {
                candidate=hit.point;valid=CanPlace(candidate);
                preview.SetActive(true);preview.transform.position=candidate+Vector3.up*.12f;
                previewRenderer.sharedMaterial=valid?validMaterial:invalidMaterial;
            }
            else preview.SetActive(false);
            if(input.SelectPressed && valid) TryPlace(candidate);
        }
        public void CancelPlacement() {if(!Placing) return;Placing=false;if(preview!=null) preview.SetActive(false);Status="Placement cancelled; no supplies spent.";}
        private void OnApplicationFocus(bool focus) {if(!focus) CancelPlacement();}
        private void OnDisable() => CancelPlacement();
        private void OnDestroy() {if(preview!=null) Destroy(preview);}
    }
}
