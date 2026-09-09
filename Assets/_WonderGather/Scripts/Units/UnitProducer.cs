using UnityEngine;
using UnityEngine.AI;
namespace WonderGather
{
    [RequireComponent(typeof(BuildingSite))]
    public sealed class UnitProducer : MonoBehaviour
    {
        [SerializeField] private WorkerProductionDefinition definition;
        [SerializeField,Min(1)] private int queueCapacity=3;
        private UnitBlueprint blueprint;
        private BuildingSite site;
        public string UnitName=>blueprint!=null?blueprint.DisplayName:"Worker";
        public void SetBlueprint(UnitBlueprint data)
        {
            if(QueueCount>0) throw new System.InvalidOperationException("Cannot replace a recipe while orders are queued.");
            blueprint=data;definition=data!=null?data.Production:null;
        }
        private ResourceDepot depot;
        private SelectionController roster;
        private Vector3 approach;
        private float elapsed, retryAfter;
        public int QueueCount {get;private set;}
        public int Capacity=>queueCapacity;
        public int Cost=>definition!=null?definition.Cost:0;
        public float Progress=>QueueCount==0?0:Mathf.Clamp01(elapsed/definition.Seconds);
        public bool WaitingForExit {get;private set;}
        public bool CanTrain=>definition!=null && isActiveAndEnabled && site!=null && site.isActiveAndEnabled && site.Complete && depot!=null && depot.isActiveAndEnabled && roster!=null && QueueCount<queueCapacity && depot.Stored>=Cost;
        public SelectableUnit LastProduced {get;private set;}
        public string Summary=>"Workers queued: "+QueueCount+" / "+queueCapacity+(QueueCount==0?"":" | "+Mathf.RoundToInt(Progress*100)+"%")+(WaitingForExit?" — exit blocked":"");
        public void SetDefinition(WorkerProductionDefinition data)=>definition=data;
        private void Awake()=>site=GetComponent<BuildingSite>();
        public void Configure(ResourceDepot bank,SelectionController owner,Vector3 workPosition){depot=bank;roster=owner;approach=workPosition;}
        public bool Enqueue()
        {
            if(!CanTrain || !depot.TrySpend(Cost)) return false;
            QueueCount++;return true;
        }
        public bool CancelLast()
        {
            if(QueueCount==0 || depot==null || !depot.Deposit(Cost)) return false;
            QueueCount--;if(QueueCount==0){elapsed=0;WaitingForExit=false;}return true;
        }
        private bool FindExit(out Vector3 position)
        {
            position=default;
            var template=definition.Prefab.GetComponent<NavMeshAgent>();
            var filter=new NavMeshQueryFilter{agentTypeID=template.agentTypeID,areaMask=template.areaMask};
            float radius=site.Definition.Size*.71f+template.radius+.6f;
            var path=new NavMeshPath();
            for(int ring=0;ring<2;ring++) for(int i=0;i<16;i++)
            {
                float angle=i*Mathf.PI/8;
                var point=transform.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(radius+ring*2);
                if(!NavMesh.SamplePosition(point,out var hit,.3f,filter)) continue;
                if(Physics.CheckCapsule(hit.position+Vector3.up*(template.radius+.1f),hit.position+Vector3.up*(template.height-template.radius),template.radius+.15f,1,QueryTriggerInteraction.Ignore)) continue;
                if(!NavMesh.CalculatePath(approach,hit.position,filter,path)||path.status!=NavMeshPathStatus.PathComplete) continue;
                position=hit.position;return true;
            }
            return false;
        }
        private void Update()
        {
            if(QueueCount==0 || !site.Complete || !site.isActiveAndEnabled || depot==null || !depot.isActiveAndEnabled || roster==null) return;
            elapsed=Mathf.Min(definition.Seconds,elapsed+Time.deltaTime);
            if(elapsed<definition.Seconds || Time.time<retryAfter) return;
            retryAfter=Time.time+.25f;
            if(!FindExit(out var position)){WaitingForExit=true;return;}
            var worker=Instantiate(definition.Prefab,position,Quaternion.identity);
            if(blueprint!=null) UnitIdentity.Apply(worker,blueprint);
            var agent=worker.GetComponent<NavMeshAgent>();
            if(!agent.isOnNavMesh || !agent.Warp(position)){worker.SetActive(false);Destroy(worker);WaitingForExit=true;return;}
            int slot=roster.AllocateWorkerSlot();
            worker.name=UnitName+" "+(slot+1);
            var offset=Quaternion.Euler(0,slot%8*45,0)*Vector3.right*(3.4f+2.6f*(slot/8));
            worker.GetComponent<Gatherer>().Configure(depot,offset);
            agent.avoidancePriority=30+slot%12*4;
            worker.GetComponent<ProducedWorker>().Configure(roster);
            LastProduced=worker.GetComponent<SelectableUnit>();
            QueueCount--;elapsed=0;WaitingForExit=false;
        }
        private void OnDestroy()
        {
            if(depot!=null && QueueCount>0) depot.Deposit(QueueCount*Cost);
            QueueCount=0;
        }
    }
}
