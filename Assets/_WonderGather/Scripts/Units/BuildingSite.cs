using UnityEngine;
namespace WonderGather
{
    public sealed class BuildingSite : MonoBehaviour
    {
        [SerializeField] private BuildingDefinition definition;
        [SerializeField] private Transform model;
        [SerializeField] private GameObject selectionRing;
        public void ConfigureSelection(GameObject ring)=>selectionRing=ring;
        public void SetSelected(bool selected){if(selectionRing!=null) selectionRing.SetActive(selected);}
        public bool HasVisual=>model!=null;
        public BuildingBlueprint Blueprint {get;private set;}
        public BuildingDefinition Definition => definition;
        public string DisplayName=>Blueprint!=null?Blueprint.DisplayName:definition.DisplayName;
        public void ApplyBlueprint(BuildingBlueprint data)
        {
            Blueprint=data;definition=data.Construction;gameObject.name=data.DisplayName;
            if(TryGetComponent<UnitProducer>(out var producer)) producer.SetBlueprints(data.ProductionOptions);
        }
        public void CompleteAtStart(){Progress=1;UpdateModel();}
        public float Progress { get; private set; }
        public bool Complete => Progress >= 1;
        public Builder Worker { get; private set; }
        public void Configure(BuildingDefinition data, Transform visual) { definition=data; model=visual; }
        public bool Claim(Builder worker)
        {
            if (!isActiveAndEnabled || Complete || (Worker != null && Worker != worker)) return false;
            Worker=worker; return true;
        }
        public void Release(Builder worker) { if(Worker==worker) Worker=null; }
        public void Work(Builder worker, float seconds)
        {
            if (!isActiveAndEnabled || Worker != worker || !worker.isActiveAndEnabled || seconds<=0) return;
            Progress=Mathf.Clamp01(Progress+seconds/definition.Seconds);
            UpdateModel();
        }
        private void Start() => UpdateModel();
        private void UpdateModel()
        {
            float height=Mathf.Lerp(.25f,2.5f,Progress);
            model.localScale=new Vector3(definition.Size,height,definition.Size);
            model.localPosition=Vector3.up*height*.5f;
        }
        private void OnDisable() { SetSelected(false); if(Worker!=null) Worker.CancelOrder(); }
    }
}
