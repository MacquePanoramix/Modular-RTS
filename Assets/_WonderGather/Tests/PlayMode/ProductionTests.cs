using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class ProductionTests
    {
        private ResourceDepot depot;private SelectionController selection;private ConstructionController construction;private UnitProducer producer;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheProduction");yield return null;
            depot=Object.FindAnyObjectByType<ResourceDepot>();selection=Object.FindAnyObjectByType<SelectionController>();construction=Object.FindAnyObjectByType<ConstructionController>();
            selection.Select(Object.FindAnyObjectByType<SelectableUnit>());depot.Deposit(100);
            Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);producer=construction.LastSite.GetComponent<UnitProducer>();
        }
        private IEnumerator FinishBuilding()
        {
            float timeout=Time.time+22;while(!construction.LastSite.Complete&&Time.time<timeout) yield return null;
            Assert.That(construction.LastSite.Complete,Is.True);selection.SelectBuilding(construction.LastSite);
        }
        [UnityTest] public IEnumerator QueueChecksCompletionFundsCapacityAndRefunds()
        {
            Assert.That(producer.Enqueue(),Is.False);Assert.That(depot.Stored,Is.EqualTo(80));yield return FinishBuilding();
            for(int i=0;i<3;i++) Assert.That(selection.OrderProduction(),Is.True);
            Assert.That(selection.OrderProduction(),Is.False);Assert.That(depot.Stored,Is.EqualTo(50));
            yield return new WaitForSeconds(1);float progress=producer.Progress;
            Assert.That(selection.OrderProduction(true),Is.True);Assert.That(producer.Progress,Is.GreaterThanOrEqualTo(progress));Assert.That(depot.Stored,Is.EqualTo(60));
            Assert.That(producer.CancelLast(),Is.True);Assert.That(producer.CancelLast(),Is.True);Assert.That(producer.CancelLast(),Is.False);
            Assert.That(depot.Stored,Is.EqualTo(80));Assert.That(producer.Progress,Is.Zero);
            Assert.That(depot.TrySpend(75),Is.True);Assert.That(producer.Enqueue(),Is.False);Assert.That(depot.Stored,Is.EqualTo(5));
        }
        [UnityTest] public IEnumerator ProducedWorkerSelectsMovesGathersAndBuilds()
        {
            yield return FinishBuilding();Assert.That(selection.OrderProduction(),Is.True);
            float timeout=Time.time+12;while(producer.LastProduced==null&&Time.time<timeout) yield return null;
            var unit=producer.LastProduced;Assert.That(unit,Is.Not.Null);Assert.That(producer.QueueCount,Is.Zero);Assert.That(depot.Stored,Is.EqualTo(70));
            Assert.That(Object.FindObjectsByType<SelectableUnit>().Length,Is.EqualTo(9));
            var camera=Camera.main;camera.transform.position=unit.transform.position+new Vector3(0,12,-12);camera.transform.LookAt(unit.transform.position);
            var screen=camera.WorldToScreenPoint(unit.transform.position+Vector3.up);
            selection.SelectBox(new Rect(screen.x-3,screen.y-3,6,6),false);Assert.That(selection.SelectedUnits,Does.Contain(unit));
            selection.Select(unit);Assert.That(selection.MoveSelection(new Vector3(-15,0,0)),Is.True);
            Assert.That(selection.GatherSelection(Object.FindAnyObjectByType<ResourceNode>()),Is.EqualTo(1));
            timeout=Time.time+18;while(unit.GetComponent<Gatherer>().Carried==0&&Time.time<timeout) yield return null;
            Assert.That(unit.GetComponent<Gatherer>().Carried,Is.GreaterThan(0));
            Assert.That(construction.TryPlace(new Vector3(-20,0,-10)),Is.True);
            timeout=Time.time+25;while(!construction.LastSite.Complete&&Time.time<timeout) yield return null;
            Assert.That(construction.LastSite.Complete,Is.True);Assert.That(depot.Stored,Is.EqualTo(50));
            Object.Destroy(unit.gameObject);yield return null;
            selection.SelectBox(new Rect(0,0,Screen.width,Screen.height),false);Assert.That(selection.Count,Is.LessThanOrEqualTo(8));
        }
        [UnityTest] public IEnumerator BlockedExitWaitsThenSpawnsWithoutAnotherCharge()
        {
            yield return FinishBuilding();
            var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.position=producer.transform.position+Vector3.up;
            blocker.transform.localScale=new Vector3(16,2,16);Physics.SyncTransforms();
            Assert.That(producer.Enqueue(),Is.True);yield return new WaitForSeconds(7);
            Assert.That(producer.WaitingForExit,Is.True);Assert.That(producer.LastProduced,Is.Null);Assert.That(producer.QueueCount,Is.EqualTo(1));Assert.That(depot.Stored,Is.EqualTo(70));
            Object.Destroy(blocker);yield return null;
            float timeout=Time.time+3;while(producer.LastProduced==null&&Time.time<timeout) yield return null;
            Assert.That(producer.LastProduced,Is.Not.Null);Assert.That(depot.Stored,Is.EqualTo(70));
        }
        [UnityTest] public IEnumerator WorkshopsHaveIndependentQueuesAndDisablePausesTraining()
        {
            yield return FinishBuilding();var first=producer;
            selection.Select(Object.FindAnyObjectByType<SelectableUnit>());Assert.That(construction.TryPlace(new Vector3(-18,0,-8)),Is.True);
            yield return FinishBuilding();var second=construction.LastSite.GetComponent<UnitProducer>();
            Assert.That(first.Enqueue(),Is.True);Assert.That(second.QueueCount,Is.Zero);Assert.That(second.Enqueue(),Is.True);
            first.enabled=false;yield return new WaitForSeconds(1);Assert.That(first.Progress,Is.Zero);Assert.That(second.Progress,Is.GreaterThan(0));
            first.enabled=true;Object.Destroy(second.gameObject);yield return null;
            Assert.That(depot.Stored,Is.EqualTo(50));Assert.That(first.QueueCount,Is.EqualTo(1));
        }
    }
}
