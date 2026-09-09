using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace WonderGather
{
    public sealed class CivilizationSession : MonoBehaviour
    {
        [SerializeField] private CivilizationDefinition civilization;
        [SerializeField] private SelectionController selection;
        [SerializeField] private ConstructionController construction;
        [SerializeField] private WandererHud hud;
        [SerializeField] private ResourceNode resource;
        [SerializeField] private Vector3 basePosition=new Vector3(-10,0,-9);
        public CivilizationDefinition Definition=>civilization;
        public ResourceDepot Depot {get;private set;}
        public bool Initialized {get;private set;}
        public string Summary {get;private set;}="";
        public void Configure(CivilizationDefinition data,SelectionController owner,ConstructionController builder,WandererHud display,ResourceNode node)
        {civilization=data;selection=owner;construction=builder;hud=display;resource=node;}
        private void Start()=>Initialize();
        public bool Initialize()
        {
            if(Initialized) return false;
            var report=CivilizationValidator.Validate(civilization);
            if(!report.CanInstantiate){Summary="Civilization setup needs attention: "+string.Join(" ",report.Errors);selection.ReportStatus(Summary);return false;}
            // Plan every initial unit before mutating the world. Keep slots separate and on the map.
            var spawns=new List<(UnitBlueprint type,Vector3 point)>();int slot=0;
            foreach(var start in civilization.StartingUnits) for(int i=0;i<start.count;i++,slot++)
            {
                var agent=start.blueprint.Production.Prefab.GetComponent<NavMeshAgent>();
                var filter=new NavMeshQueryFilter{agentTypeID=agent.agentTypeID,areaMask=agent.areaMask};
                var desired=basePosition+Quaternion.Euler(0,slot%8*45,0)*Vector3.right*(5+3*(slot/8));
                if(!NavMesh.SamplePosition(desired,out var hit,.3f,filter) || Physics.CheckCapsule(hit.position+Vector3.up*(agent.radius+.1f),hit.position+Vector3.up*(agent.height-agent.radius),agent.radius+.15f,1,QueryTriggerInteraction.Ignore))
                {Summary="Starting units do not fit on this map. Reduce the starting count or adjust the starting position.";selection.ReportStatus(Summary);return false;}
                foreach(var other in spawns) if((other.point-hit.position).sqrMagnitude<4)
                {Summary="Starting unit positions overlap.";selection.ReportStatus(Summary);return false;}
                spawns.Add((start.blueprint,hit.position));
            }
            var home=Instantiate(civilization.StartingBase.Prefab,basePosition,Quaternion.identity);
            home.ApplyBlueprint(civilization.StartingBase);home.CompleteAtStart();
            Depot=home.GetComponent<ResourceDepot>();
            if(civilization.StartingSupplies>0) Depot.Deposit(civilization.StartingSupplies);
            if(home.TryGetComponent<UnitProducer>(out var producer)) producer.Configure(Depot,selection,basePosition+Vector3.right*5);
            foreach(var spawn in spawns)
            {
                var unit=Instantiate(spawn.type.Production.Prefab,spawn.point,Quaternion.identity);
                UnitIdentity.Apply(unit,spawn.type);
                int index=selection.AllocateWorkerSlot();
                unit.GetComponent<Gatherer>().Configure(Depot,Quaternion.Euler(0,index%8*45,0)*Vector3.right*(3.4f+2.6f*(index/8)));
                unit.GetComponent<NavMeshAgent>().avoidancePriority=30+index%12*4;
                unit.GetComponent<ProducedWorker>().Configure(selection);
            }
            construction.ConfigureCivilization(civilization,Depot);
            hud.ConfigureEconomy(Depot,resource);
            Summary=civilization.DisplayName+" | "+spawns.Count+" starting units | "+civilization.StartingSupplies+" starting supplies";
            Initialized=true;
            selection.ReportStatus(report.Warnings.Count>0?report.Warnings[0]:"Civilization ready. Select workers to gather and build.");
            return true;
        }
    }
}
