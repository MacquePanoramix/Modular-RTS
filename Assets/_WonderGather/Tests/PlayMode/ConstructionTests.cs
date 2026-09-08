using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class ConstructionTests
    {
        private ConstructionController construction;private ResourceDepot depot;private SelectionController selection;private SelectableUnit unit;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheSettlement");yield return null;
            construction=Object.FindAnyObjectByType<ConstructionController>();depot=Object.FindAnyObjectByType<ResourceDepot>();selection=Object.FindAnyObjectByType<SelectionController>();unit=Object.FindAnyObjectByType<SelectableUnit>();selection.Select(unit);
        }
        [UnityTest] public IEnumerator PlacementRequiresFundsClearGroundAndReachableWorker()
        {
            Assert.That(construction.TryPlace(new Vector3(-18,0,10)),Is.False);depot.Deposit(40);
            foreach(var point in new[]{new Vector3(4,0,0),new Vector3(39,0,0),new Vector3(32,0,0),new Vector3(float.NaN,0,0),unit.transform.position}) Assert.That(construction.TryPlace(point),Is.False,"Invalid footprint at " + point);
            Assert.That(depot.Stored,Is.EqualTo(40));Assert.That(construction.LastSite,Is.Null);
            Assert.That(construction.BeginPlacement(),Is.True);construction.CancelPlacement();Assert.That(depot.Stored,Is.EqualTo(40));Assert.That(construction.Placing,Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator WorkerTravelsBuildsAndSpendsExactlyOnce()
        {
            depot.Deposit(40);Assert.That(construction.TryPlace(new Vector3(-18,0,10)),Is.True);
            var site=construction.LastSite;Assert.That(depot.Stored,Is.EqualTo(20));Assert.That(site.Progress,Is.Zero);
            Assert.That(construction.TryPlace(site.transform.position),Is.False);Assert.That(depot.Stored,Is.EqualTo(20));
            float timeout=Time.time+22;while(!site.Complete && Time.time<timeout) yield return null;
            Assert.That(site.Complete,Is.True);Assert.That(site.Worker,Is.Null);Assert.That(depot.Stored,Is.EqualTo(20));
            Assert.That(site.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving,Is.True);
            yield return new WaitForSeconds(.5f);
            Assert.That(UnityEngine.AI.NavMesh.SamplePosition(site.transform.position,out _, .3f,UnityEngine.AI.NavMesh.AllAreas),Is.False,"The building footprint should be carved out of walkable ground.");
        }
        [UnityTest] public IEnumerator MovingPausesAndResumePreservesProgressAndFunds()
        {
            depot.Deposit(20);Assert.That(construction.TryPlace(new Vector3(-18,0,10)),Is.True);var site=construction.LastSite;
            float timeout=Time.time+15;while(site.Progress<.15f && Time.time<timeout) yield return null;Assert.That(site.Progress,Is.GreaterThan(0));
            Assert.That(selection.MoveSelection(new Vector3(-16,0,-3)),Is.True);float progress=site.Progress;
            yield return new WaitForSeconds(.5f);Assert.That(site.Progress,Is.EqualTo(progress));Assert.That(site.Worker,Is.Null);
            construction.Resume(site);timeout=Time.time+22;while(!site.Complete && Time.time<timeout) yield return null;
            Assert.That(site.Complete,Is.True);Assert.That(depot.Stored,Is.Zero);
        }
        [UnityTest] public IEnumerator GatheringReplacesConstructionAndDisableReleasesSite()
        {
            depot.Deposit(20);Assert.That(construction.TryPlace(new Vector3(-18,0,10)),Is.True);var site=construction.LastSite;
            Assert.That(selection.GatherSelection(Object.FindAnyObjectByType<ResourceNode>()),Is.EqualTo(1));Assert.That(site.Worker,Is.Null);
            construction.Resume(site);Assert.That(unit.GetComponent<Gatherer>().State,Is.EqualTo(Gatherer.Activity.Idle));
            unit.gameObject.SetActive(false);yield return null;Assert.That(site.Worker,Is.Null);
        }
    }
}
