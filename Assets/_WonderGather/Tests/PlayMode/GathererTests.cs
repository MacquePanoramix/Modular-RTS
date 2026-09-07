using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class GathererTests
    {
        [UnitySetUp] public IEnumerator Load() { yield return SceneManager.LoadSceneAsync("TheGatherer"); yield return null; }
        [UnityTest] public IEnumerator UnreachableGroupOrdersResolveToSeparateReachableGround()
        {
            var selection = Object.FindAnyObjectByType<SelectionController>();
            var units = Object.FindObjectsByType<SelectableUnit>();
            foreach(var unit in units) selection.Select(unit,true);
            foreach(var target in new[]{new Vector3(39,0,0),new Vector3(32,0,0),new Vector3(200,0,0),Vector3.zero})
            {
                Assert.That(selection.MoveSelection(target),Is.True);
                for(int i=0;i<units.Length;i++)
                {
                    Assert.That(units[i].Motor.TryPlanMove(units[i].Motor.Destination,out _,out _),Is.True);
                    for(int j=0;j<i;j++) Assert.That(Vector3.Distance(units[i].Motor.Destination,units[j].Motor.Destination),Is.GreaterThan(2.3f));
                }
            }
            Assert.That(selection.MoveSelection(new Vector3(39,0,0)),Is.True);
            float deadline=Time.time+30;
            bool arrived=false;
            while(Time.time<deadline)
            {
                arrived=true;
                foreach(var unit in units) arrived &= Vector3.Distance(unit.transform.position,unit.Motor.Destination)<.4f;
                if(arrived) break;
                yield return null;
            }
            Assert.That(arrived,Is.True,"Fallback group should reach the resolved destinations.");
        }
        [UnityTest] public IEnumerator WorkersDeliverRepeatAndConserveFiniteResource()
        {
            var selection = Object.FindAnyObjectByType<SelectionController>();
            var workers = Object.FindObjectsByType<Gatherer>();
            var node = Object.FindAnyObjectByType<ResourceNode>();
            var depot = Object.FindAnyObjectByType<ResourceDepot>();
            foreach(var unit in Object.FindObjectsByType<SelectableUnit>()) selection.Select(unit,true);
            Assert.That(selection.GatherSelection(node),Is.EqualTo(8));
            float deadline = Time.time+100;
            while(depot.Stored < 120 && Time.time < deadline)
            {
                int total = node.Remaining+depot.Stored;
                foreach(var worker in workers) { total+=worker.Carried; Assert.That(worker.Carried,Is.InRange(0,5)); }
                Assert.That(total,Is.EqualTo(120));
                yield return null;
            }
            Assert.That(depot.Stored,Is.EqualTo(120));
            Assert.That(node.Remaining,Is.Zero);
            yield return null;
            foreach(var worker in workers) Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Idle));
        }
        [UnityTest] public IEnumerator MoveCancelsGatheringWithoutDiscardingCargo()
        {
            var unit = Object.FindAnyObjectByType<SelectableUnit>();
            var worker = unit.GetComponent<Gatherer>();
            var node = Object.FindAnyObjectByType<ResourceNode>();
            var selection=Object.FindAnyObjectByType<SelectionController>(); selection.Select(unit);
            Assert.That(selection.GatherSelection(node),Is.EqualTo(1));
            float deadline=Time.time+15;
            while(worker.Carried==0 && Time.time<deadline) yield return null;
            Assert.That(worker.Carried,Is.GreaterThan(0));
            int cargo=worker.Carried;
            Assert.That(selection.MoveSelection(new Vector3(-16,0,0)),Is.True);
            yield return new WaitForSeconds(1);
            Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Idle)); Assert.That(worker.Carried,Is.EqualTo(cargo));
            var depot = Object.FindAnyObjectByType<ResourceDepot>();
            Assert.That(worker.ReturnToDepot(depot),Is.True);
            deadline = Time.time + 15;
            while (depot.Stored < cargo && Time.time < deadline) yield return null;
            Assert.That(depot.Stored,Is.EqualTo(cargo)); Assert.That(worker.Carried,Is.Zero);
            Assert.That(selection.GatherSelection(node),Is.EqualTo(1));
            Assert.That(new CommandDispatcher().Dispatch(new MoveCommand(new Vector3(-15,0,2)),unit.Motor),Is.True);
            Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Idle));
            Assert.That(selection.GatherSelection(node),Is.EqualTo(1));
            worker.enabled=false; yield return null;
            var stopped=worker.transform.position;
            yield return new WaitForSeconds(.5f);
            Assert.That(Vector3.Distance(stopped,worker.transform.position),Is.LessThan(.1f));
            Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Idle)); Assert.That(worker.Carried,Is.Zero);
        }
        [UnityTest] public IEnumerator TargetLossReturnsCarriedSupplies()
        {
            var worker=Object.FindAnyObjectByType<Gatherer>(); var node=Object.FindAnyObjectByType<ResourceNode>();
            var depot=Object.FindAnyObjectByType<ResourceDepot>(); Assert.That(worker.Gather(node),Is.True);
            float deadline=Time.time+15; while(worker.Carried==0 && Time.time<deadline) yield return null;
            int cargo=worker.Carried; Assert.That(cargo,Is.GreaterThan(0)); node.gameObject.SetActive(false);
            deadline=Time.time+15; while(depot.Stored<cargo && Time.time<deadline) yield return null;
            Assert.That(depot.Stored,Is.EqualTo(cargo)); Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Idle));
        }
    }
}
