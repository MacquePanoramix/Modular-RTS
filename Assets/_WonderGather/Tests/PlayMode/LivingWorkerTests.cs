using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WonderGather.Tests
{
    public sealed class LivingWorkerTests
    {
        private FactionCreator creator;
        private string storageRoot;

        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");
            yield return null;
            creator=Object.FindAnyObjectByType<FactionCreator>();
            Assert.That(creator,Is.Not.Null);
            storageRoot=Path.Combine(Path.GetTempPath(),"WonderGather-LivingWorkerTests-"+Guid.NewGuid().ToString("N"));
            creator.ConfigureStorage(new FactionStore(storageRoot));
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
            if(storageRoot==null) yield break;
            string path=Path.GetFullPath(storageRoot);
            string prefix=Path.Combine(Path.GetFullPath(Path.GetTempPath()),"WonderGather-LivingWorkerTests-");
            Assert.That(path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase),Is.True);
            if(Directory.Exists(path)) Directory.Delete(path,true);
        }

        private IEnumerator Transition()
        {
            float deadline=Time.realtimeSinceStartup+20;
            while(creator.Busy&&Time.realtimeSinceStartup<deadline) yield return null;
            Assert.That(creator.Busy,Is.False,"The creator scene transition must complete.");
            yield return null;
            yield return null;
        }

        private IEnumerator Enter(int count,UnitPerformance performance,int supplies=0)
        {
            creator.Draft.SetStartingSetup("Living worker test",count,supplies);
            creator.Draft.SetPerformance(creator.Draft.Worker,performance);
            Assert.That(creator.Playtest(),Is.True);
            yield return Transition();
            Assert.That(creator.Playing,Is.True);
            Assert.That(Object.FindObjectsByType<Gatherer>(FindObjectsSortMode.None).Length,Is.EqualTo(count));
        }

        private static void Conserved(ResourceNode node,ResourceDepot depot,Gatherer[] workers,int total)
        {
            Assert.That(node.Remaining+depot.Stored+workers.Sum(x=>x.Carried),Is.EqualTo(total),
                "Each supply must remain in the resource, a worker's real cargo, or the depot.");
            foreach(var worker in workers)
                Assert.That(worker.Carried,Is.InRange(0,worker.Capacity));
        }

        private static void Supported(ProceduralBiped body)
        {
            Assert.That(body,Is.Not.Null,"Faction workers must use the articulated body.");
            Assert.That(body.Ready,Is.True);
            // A walk always keeps a supporting foot. Since Strength and Burden checkpoint A, fast
            // Movement % jogs; only a jog's flight phase may leave the ground, and only briefly.
            Assert.That(body.FootPlanted(0)||body.FootPlanted(1)||(body.CurrentGait!=ProceduralBiped.Gait.Walking&&body.FlightTime<.2f),
                Is.True,"Retain at least one supporting foot outside a brief jogging flight phase.");
            foreach(var part in body.GetComponentsInChildren<Transform>())
                if(part.name.EndsWith("thigh",StringComparison.Ordinal)||part.name.EndsWith("shin",StringComparison.Ordinal))
                    Assert.That(part.localScale.y*2,Is.EqualTo(.68f).Within(.008f),"Walking and working must not stretch a leg segment.");
            for(int i=0;i<2;i++)
            {
                Vector3 point=body.FootPosition(i);
                Assert.That(float.IsFinite(point.x)&&float.IsFinite(point.y)&&float.IsFinite(point.z),Is.True);
                if(!body.FootPlanted(i)) continue;
                Assert.That(Physics.Raycast(point+Vector3.up,Vector3.down,out var hit,2,1<<6,QueryTriggerInteraction.Ignore),Is.True);
                Assert.That(Vector3.Distance(point,hit.point),Is.LessThan(.025f),"A planted support must touch the terrain.");
            }
        }

        private static void NoWalkingContact(Gatherer worker)
        {
            if(worker.State==Gatherer.Activity.Idle||worker.State==Gatherer.Activity.ToResource||worker.State==Gatherer.Activity.ToDepot)
                Assert.That(worker.HasWorkContact,Is.False,"A walking or cancelled order must not retain an active work gesture.");
        }

        [UnityTest] public IEnumerator StartingWorkerReachesWorksCarriesAndDepositsWithGroundedSupport()
        {
            yield return Enter(1,UnitPerformance.Default);
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var body=worker.GetComponent<ProceduralBiped>();
            var node=Object.FindAnyObjectByType<ResourceNode>();
            var depot=creator.Bridge.Session.Depot;
            var workers=new[]{worker};
            int total=node.Remaining+depot.Stored;
            Assert.That(node.GetComponent<ResourceWorkplace>(),Is.Not.Null);
            Assert.That(depot.GetComponent<ResourceWorkplace>(),Is.Not.Null);
            Assert.That(Physics.Raycast(node.transform.position+new Vector3(0,5,1.4f),Vector3.down,out var supplyHit,10,~0,QueryTriggerInteraction.Ignore),Is.True);
            Assert.That(supplyHit.collider.GetComponentInParent<ResourceNode>(),Is.SameAs(node),"Visible supply boxes must accept resource clicks.");
            Assert.That(Physics.Raycast(depot.transform.position+new Vector3(-.75f,5,1.75f),Vector3.down,out var shelfHit,10,~0,QueryTriggerInteraction.Ignore),Is.True);
            Assert.That(shelfHit.collider.GetComponentInParent<ResourceDepot>(),Is.SameAs(depot),"Visible delivery shelves must accept depot clicks.");
            Supported(body);
            Assert.That(body.CargoVisible,Is.False);
            Assert.That(worker.Gather(node),Is.True);
            bool gathered=false,carried=false,deposited=false;
            float closestHand=float.PositiveInfinity;
            int stationarySupports=0;
            var previousFeet=new[]{body.FootPosition(0),body.FootPosition(1)};
            var previousPlanted=new[]{body.FootPlanted(0),body.FootPlanted(1)};
            Vector3 previousRoot=worker.transform.position;
            int previousCargo=worker.Carried;
            float deadline=Time.time+35;
            while(depot.Stored==0&&Time.time<deadline)
            {
                yield return null;
                Conserved(node,depot,workers,total);
                Supported(body);
                NoWalkingContact(worker);
                // Gameplay Update precedes presentation LateUpdate; compare only unchanged cargo samples.
                if(worker.Carried==previousCargo) Assert.That(body.CargoVisible,Is.EqualTo(worker.Carried>0));
                previousCargo=worker.Carried;
                if(worker.State==Gatherer.Activity.Gathering)
                {
                    gathered=true;
                    Assert.That(worker.HasWorkContact,Is.True);
                    Assert.That(worker.ActionProgress,Is.InRange(0f,1f));
                    closestHand=Mathf.Min(closestHand,Vector3.Distance(body.HandPosition(1),worker.WorkContact));
                    for(int i=0;i<2;i++)
                    {
                        if(previousPlanted[i]&&body.FootPlanted(i)&&Vector3.Distance(previousRoot,worker.transform.position)<.005f)
                        {
                            Assert.That(Vector3.Distance(previousFeet[i],body.FootPosition(i)),Is.LessThan(.002f));
                            stationarySupports++;
                        }
                    }
                }
                carried|=worker.Carried>0&&body.CargoVisible;
                deposited|=worker.State==Gatherer.Activity.Depositing;
                for(int i=0;i<2;i++){previousFeet[i]=body.FootPosition(i);previousPlanted[i]=body.FootPlanted(i);}
                previousRoot=worker.transform.position;
            }
            Assert.That(depot.Stored,Is.EqualTo(worker.Capacity),"The first delivery should contain the real full load.");
            Assert.That(gathered&&carried&&deposited,Is.True,"Observe the whole work, carry, and delivery sequence.");
            Assert.That(closestHand,Is.LessThan(.3f),"The collecting hand should reach the authored resource contact.");
            Assert.That(stationarySupports,Is.GreaterThan(5),"Observe stable planted support during actual work.");
            worker.CancelOrder();worker.GetComponent<UnitMotor>().Stop();
            yield return null;yield return null;
            Assert.That(body.CargoVisible,Is.False);
            Assert.That(worker.HasWorkContact,Is.False);
            Conserved(node,depot,workers,total);
        }

        [UnityTest] public IEnumerator NewlyProducedBlueprintWorkerCompletesTheSameVisibleLoop()
        {
            yield return Enter(1,new UnitPerformance(200,2,200,200),120);
            var starter=Object.FindAnyObjectByType<SelectableUnit>();
            var selection=Object.FindAnyObjectByType<SelectionController>();
            var construction=Object.FindAnyObjectByType<ConstructionController>();
            selection.Select(starter);
            Assert.That(construction.ChooseBuilding(creator.Draft.Workshop),Is.True);
            Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);
            var site=construction.LastSite;
            float deadline=Time.time+25;
            while(!site.Complete&&Time.time<deadline) yield return null;
            Assert.That(site.Complete,Is.True);
            var producer=site.GetComponent<UnitProducer>();
            Assert.That(producer.Enqueue(creator.Draft.Worker),Is.True);
            deadline=Time.time+15;
            while(producer.LastProduced==null&&Time.time<deadline) yield return null;
            Assert.That(producer.LastProduced,Is.Not.Null);
            yield return null;yield return null;
            var unit=producer.LastProduced;
            var worker=unit.GetComponent<Gatherer>();
            var body=unit.GetComponent<ProceduralBiped>();
            Assert.That(unit.GetComponent<UnitIdentity>().Blueprint,Is.SameAs(creator.Draft.Worker));
            // 200% of the worker's natural 1.8 m/s walk (decision D2, September 30).
            Assert.That(unit.GetComponent<NavMeshAgent>().speed,Is.EqualTo(3.6f).Within(.01f));
            Supported(body);
            var node=Object.FindAnyObjectByType<ResourceNode>();
            var depot=creator.Bridge.Session.Depot;
            var workers=Object.FindObjectsByType<Gatherer>(FindObjectsSortMode.None);
            int before=depot.Stored,total=node.Remaining+before+workers.Sum(x=>x.Carried);
            selection.Select(unit);
            Assert.That(selection.GatherSelection(node),Is.EqualTo(1));
            bool visibleCargo=false;
            deadline=Time.time+40;
            while(depot.Stored==before&&Time.time<deadline)
            {
                yield return null;
                Conserved(node,depot,workers,total);
                Supported(body);
                visibleCargo|=worker.Carried>0&&body.CargoVisible;
            }
            Assert.That(depot.Stored,Is.EqualTo(before+2));
            Assert.That(visibleCargo,Is.True,"Newly trained workers must present real cargo, just like starting workers.");
            worker.CancelOrder();worker.GetComponent<UnitMotor>().Stop();
            yield return null;yield return null;
            Assert.That(body.CargoVisible,Is.False);
        }

        [UnityTest] public IEnumerator RedirectDisableAndReturnPreservePartialCargoAndReleaseTheWorkPosition()
        {
            yield return Enter(1,new UnitPerformance(100,20,100,100));
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var body=worker.GetComponent<ProceduralBiped>();
            var node=Object.FindAnyObjectByType<ResourceNode>();
            var workplace=node.GetComponent<ResourceWorkplace>();
            var depot=creator.Bridge.Session.Depot;
            int total=node.Remaining;
            Assert.That(worker.Gather(node),Is.True);
            float deadline=Time.time+30;
            while(worker.Carried<2&&Time.time<deadline) yield return null;
            Assert.That(worker.Carried,Is.InRange(2,19));
            Assert.That(workplace.OccupiedCount,Is.EqualTo(1));
            int cargo=worker.Carried,remaining=node.Remaining;
            worker.enabled=false;
            Assert.That(workplace.OccupiedCount,Is.Zero,"Disabling an active gatherer releases its occupied station.");
            Assert.That(worker.HasWorkContact,Is.False);
            yield return null;yield return null;
            Assert.That(worker.Carried,Is.EqualTo(cargo));
            Assert.That(body.CargoVisible,Is.True);
            worker.enabled=true;
            Assert.That(worker.Gather(node),Is.True);
            Assert.That(workplace.OccupiedCount,Is.EqualTo(1),"A re-enabled worker can claim a work position again.");
            var selection=Object.FindAnyObjectByType<SelectionController>();
            selection.Select(worker.GetComponent<SelectableUnit>());
            Assert.That(selection.MoveSelection(new Vector3(-3,0,-4)),Is.True);
            Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Idle));
            Assert.That(worker.HasWorkContact,Is.False);
            Assert.That(workplace.OccupiedCount,Is.Zero,"A replacement movement order releases the previous work station immediately.");
            float end=Time.time+1.25f;
            while(Time.time<end)
            {
                yield return null;
                Assert.That(worker.Carried,Is.EqualTo(cargo));
                Assert.That(node.Remaining,Is.EqualTo(remaining));
                Assert.That(body.CargoVisible,Is.True,"Cancelling work must not hide supplies that are still carried.");
                NoWalkingContact(worker);
                Conserved(node,depot,new[]{worker},total);
            }
            worker.enabled=false;
            yield return null;yield return null;
            Assert.That(worker.Carried,Is.EqualTo(cargo));
            Assert.That(worker.HasWorkContact,Is.False);
            Assert.That(body.CargoVisible,Is.True);
            worker.enabled=true;
            Assert.That(worker.ReturnToDepot(depot),Is.True);
            bool deliveryObserved=false;
            deadline=Time.time+30;
            while(depot.Stored==0&&Time.time<deadline)
            {
                yield return null;
                Conserved(node,depot,new[]{worker},total);
                deliveryObserved|=worker.State==Gatherer.Activity.Depositing;
            }
            Assert.That(deliveryObserved,Is.True);
            Assert.That(depot.Stored,Is.EqualTo(cargo));
            Assert.That(worker.Carried,Is.Zero);
            yield return null;yield return null;
            Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Idle));
            Assert.That(body.CargoVisible,Is.False);
            Assert.That(depot.GetComponent<ResourceWorkplace>().OccupiedCount,Is.Zero);
        }

        [UnityTest] public IEnumerator SharedLimitedPositionsQueueAndDrainAResourceWithoutLosingSupplies()
        {
            yield return Enter(8,new UnitPerformance(200,2,200,100));
            var workers=Object.FindObjectsByType<Gatherer>(FindObjectsSortMode.None);
            var node=Object.FindAnyObjectByType<ResourceNode>();
            var depot=creator.Bridge.Session.Depot;
            var workplace=node.GetComponent<ResourceWorkplace>();
            var stands=new Transform[2];var contacts=new Transform[2];
            for(int i=0;i<2;i++)
            {
                float side=i==0?-1:1;
                Vector3 point=node.transform.position+Vector3.right*(2.05f*side);
                Assert.That(Physics.Raycast(point+Vector3.up*5,Vector3.down,out var hit,10,1<<6,QueryTriggerInteraction.Ignore),Is.True);
                stands[i]=new GameObject("Test resource station "+i).transform;
                contacts[i]=new GameObject("Test resource contact "+i).transform;
                stands[i].position=hit.point;
                contacts[i].position=new Vector3(node.transform.position.x+1.55f*side,hit.point.y+1.35f,node.transform.position.z);
            }
            workplace.Configure(stands,contacts);
            node.Take(node.Remaining-17);
            Assert.That(node.Remaining,Is.EqualTo(17));
            foreach(var worker in workers) Assert.That(worker.Gather(node),Is.True);
            bool queued=false;
            float deadline=Time.time+90;
            while((node.Remaining>0||workers.Any(x=>x.Carried>0))&&Time.time<deadline)
            {
                yield return null;
                Conserved(node,depot,workers,17);
                Assert.That(workplace.OccupiedCount,Is.InRange(0,2));
                queued|=workers.Any(x=>x.State==Gatherer.Activity.WaitingForResource);
                var active=workers.Where(x=>x.State==Gatherer.Activity.Gathering).ToArray();
                for(int i=0;i<active.Length;i++)
                {
                    Assert.That(active[i].HasWorkContact,Is.True);
                    for(int j=i+1;j<active.Length;j++)
                        Assert.That(Vector3.Distance(active[i].WorkContact,active[j].WorkContact),Is.GreaterThan(.2f),"Workers may not gather simultaneously from the same claimed station.");
                }
            }
            Assert.That(queued,Is.True,"More workers than resource positions must visibly wait and retry.");
            Assert.That(node.Remaining,Is.Zero);
            Assert.That(depot.Stored,Is.EqualTo(17));
            yield return null;yield return null;
            Assert.That(workplace.OccupiedCount,Is.Zero);
            Assert.That(depot.GetComponent<ResourceWorkplace>().OccupiedCount,Is.Zero);
            foreach(var worker in workers)
            {
                Assert.That(worker.Carried,Is.Zero);
                Assert.That(worker.HasWorkContact,Is.False);
                Assert.That(worker.GetComponent<ProceduralBiped>().CargoVisible,Is.False);
            }
        }

        [UnityTest] public IEnumerator SupportedPerformanceExtremesKeepTheBodyGroundedAndCargoAuthoritative()
        {
            foreach(var stats in new[]{new UnitPerformance(25,1,25,100),new UnitPerformance(200,20,200,100)})
            {
                yield return Enter(1,stats);
                var worker=Object.FindAnyObjectByType<Gatherer>();
                var body=worker.GetComponent<ProceduralBiped>();
                var node=Object.FindAnyObjectByType<ResourceNode>();
                var depot=creator.Bridge.Session.Depot;
                int total=node.Remaining;
                Assert.That(worker.GetComponent<NavMeshAgent>().speed,Is.EqualTo(1.8f*stats.movementPercent/100f).Within(.01f));
                Assert.That(worker.Capacity,Is.EqualTo(stats.capacity));
                Assert.That(worker.SecondsPerUnit,Is.EqualTo(.5f*100f/stats.gatheringPercent).Within(.001f));
                Assert.That(worker.Gather(node),Is.True);
                bool visibleCargo=false;
                // The 25% extreme walks at 0.45 m/s since the natural-pace decision (D2), so a
                // round trip of about 26 m needs a larger time budget than the former 0.8 m/s.
                float started=Time.time,deadline=started+120;
                while(depot.Stored==0&&Time.time<deadline)
                {
                    yield return null;
                    Supported(body);
                    NoWalkingContact(worker);
                    Conserved(node,depot,new[]{worker},total);
                    visibleCargo|=worker.Carried>0&&body.CargoVisible;
                }
                TestContext.WriteLine($"Movement {stats.movementPercent}%: first delivery after {Time.time-started:F1} s");
                Assert.That(depot.Stored,Is.EqualTo(stats.capacity),"The existing performance settings must still complete a full load.");
                Assert.That(body.StepCount,Is.GreaterThan(4));
                Assert.That(visibleCargo,Is.True);
                Assert.That(creator.ReturnToCreator(),Is.True);
                yield return Transition();
            }
        }
    }
}
