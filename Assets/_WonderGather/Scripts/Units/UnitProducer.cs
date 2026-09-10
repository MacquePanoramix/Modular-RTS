using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
namespace WonderGather
{
    [RequireComponent(typeof(BuildingSite))]
    public sealed class UnitProducer : MonoBehaviour
    {
        [SerializeField] private WorkerProductionDefinition definition;
        [SerializeField,Min(1)] private int queueCapacity=3;
        private sealed class Order
        {
            public UnitBlueprint Blueprint;
            public GameObject Prefab;
            public int Paid;
            public float Seconds;
            public string Name;
        }
        private readonly List<Order> queue=new List<Order>();
        private UnitBlueprint[] options=Array.Empty<UnitBlueprint>();
        private BuildingSite site;
        private ResourceDepot depot;
        private SelectionController roster;
        private Vector3 approach;
        private float elapsed,retryAfter;
        public IReadOnlyList<UnitBlueprint> Options=>options;
        public string UnitName=>options.Length>0?options[0].DisplayName:"Worker";
        public int QueueCount=>queue.Count;
        public int Capacity=>queueCapacity;
        public int Cost=>definition!=null?definition.Cost:0;
        public float Progress=>QueueCount==0?0:Mathf.Clamp01(elapsed/queue[0].Seconds);
        public bool WaitingForExit {get;private set;}
        public SelectableUnit LastProduced {get;private set;}
        public string Summary=>"Units queued: "+QueueCount+" / "+queueCapacity+(QueueCount==0?"":" | "+queue[0].Name+" "+Mathf.RoundToInt(Progress*100)+"%")+(WaitingForExit?" — exit blocked":"");
        public string QueueDescription=>string.Join(" → ",queue.Select(x=>x.Name));
        public bool CanTrain=>CanTrainUnit(null);
        private WorkerProductionDefinition Recipe(UnitBlueprint unit)=>unit==null?definition:Array.IndexOf(options,unit)>=0?unit.Production:null;
        public bool CanTrainUnit(UnitBlueprint unit)
        {
            var recipe=Recipe(unit);
            return recipe!=null && recipe.Prefab!=null && recipe.Cost>0 && float.IsFinite(recipe.Seconds) && recipe.Seconds>0 && isActiveAndEnabled && site!=null && site.isActiveAndEnabled && site.Complete && depot!=null && depot.isActiveAndEnabled && roster!=null && QueueCount<queueCapacity && depot.Stored>=recipe.Cost;
        }
        public void SetBlueprint(UnitBlueprint data)=>SetBlueprints(data!=null?new[]{data}:Array.Empty<UnitBlueprint>());
        public void SetBlueprints(IEnumerable<UnitBlueprint> data)
        {
            if(QueueCount>0) throw new InvalidOperationException("Cannot replace recipes while orders are queued.");
            var values=data.ToArray();
            if(values.Any(x=>x==null) || values.Distinct().Count()!=values.Length) throw new ArgumentException("Production options must be distinct blueprints.");
            options=values;definition=options.Length>0?options[0].Production:null;
        }
        public void SetDefinition(WorkerProductionDefinition data)
        {if(QueueCount>0) throw new InvalidOperationException("Cannot replace a recipe while orders are queued.");options=Array.Empty<UnitBlueprint>();definition=data;}
        private void Awake()=>site=GetComponent<BuildingSite>();
        public void Configure(ResourceDepot bank,SelectionController owner,Vector3 workPosition){depot=bank;roster=owner;approach=workPosition;}
        public bool Enqueue(UnitBlueprint unit=null)
        {
            if(!CanTrainUnit(unit)) return false;
            var recipe=Recipe(unit);if(!depot.TrySpend(recipe.Cost)) return false;
            var type=unit??options.FirstOrDefault();
            queue.Add(new Order{Blueprint=type,Prefab=recipe.Prefab,Paid=recipe.Cost,Seconds=recipe.Seconds,Name=type!=null?type.DisplayName:"Worker"});return true;
        }
        public bool CancelLast()
        {
            if(QueueCount==0 || depot==null || !depot.Deposit(queue[QueueCount-1].Paid)) return false;
            queue.RemoveAt(QueueCount-1);if(QueueCount==0){elapsed=0;retryAfter=0;WaitingForExit=false;}return true;
        }
        private bool FindExit(Order order,out Vector3 position)
        {
            position=default;var template=order.Prefab.GetComponent<NavMeshAgent>();
            var filter=new NavMeshQueryFilter{agentTypeID=template.agentTypeID,areaMask=template.areaMask};
            float radius=site.Definition.Size*.71f+template.radius+.6f;var path=new NavMeshPath();
            for(int ring=0;ring<2;ring++) for(int i=0;i<16;i++)
            {
                float angle=i*Mathf.PI/8;var point=transform.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(radius+ring*2);
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
            var order=queue[0];elapsed=Mathf.Min(order.Seconds,elapsed+Time.deltaTime);
            if(elapsed<order.Seconds || Time.time<retryAfter) return;retryAfter=Time.time+.25f;
            if(!FindExit(order,out var position)){WaitingForExit=true;return;}
            var worker=Instantiate(order.Prefab,position,Quaternion.identity);
            if(order.Blueprint!=null) UnitIdentity.Apply(worker,order.Blueprint);
            var agent=worker.GetComponent<NavMeshAgent>();
            if(!agent.isOnNavMesh || !agent.Warp(position)){worker.SetActive(false);Destroy(worker);WaitingForExit=true;return;}
            int slot=roster.AllocateWorkerSlot();worker.name=order.Name+" "+(slot+1);
            worker.GetComponent<Gatherer>().Configure(depot,Quaternion.Euler(0,slot%8*45,0)*Vector3.right*(3.4f+2.6f*(slot/8)));
            agent.avoidancePriority=30+slot%12*4;worker.GetComponent<ProducedWorker>().Configure(roster);
            LastProduced=worker.GetComponent<SelectableUnit>();queue.RemoveAt(0);elapsed=0;WaitingForExit=false;
        }
        private void OnDestroy(){if(depot!=null && QueueCount>0) depot.Deposit(queue.Sum(x=>x.Paid));queue.Clear();}
    }
}
