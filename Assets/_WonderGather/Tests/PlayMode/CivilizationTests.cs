using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class CivilizationTests
    {
        private CivilizationSession session;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheCivilization");yield return null;
            session=Object.FindAnyObjectByType<CivilizationSession>();
            Assert.That(session.Initialized,Is.True);
        }
        [UnityTest] public IEnumerator StartingSetupAndVariantComeFromData()
        {
            Assert.That(Object.FindObjectsByType<SelectableUnit>().Length,Is.EqualTo(8));Assert.That(session.Depot.Stored,Is.Zero);
            Assert.That(session.Depot.GetComponent<BuildingSite>().Complete,Is.True);
            Assert.That(CivilizationValidator.Validate(session.Definition).Warnings,Is.Empty);
            Assert.That(session.Initialize(),Is.False);
            yield return SceneManager.LoadSceneAsync("TheProvisionedCivilization");yield return null;
            session=Object.FindAnyObjectByType<CivilizationSession>();
            Assert.That(session.Initialized,Is.True);Assert.That(session.Depot.Stored,Is.EqualTo(40));
            Assert.That(Object.FindObjectsByType<SelectableUnit>().Length,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator BlueprintWorkerGathersBuildsAndProducesItsOwnBlueprint()
        {
            var selection=Object.FindAnyObjectByType<SelectionController>();var construction=Object.FindAnyObjectByType<ConstructionController>();
            var worker=Object.FindObjectsByType<SelectableUnit>()[0];selection.Select(worker);
            Assert.That(selection.GatherSelection(Object.FindAnyObjectByType<ResourceNode>()),Is.EqualTo(1));
            float timeout=Time.time+90;while(session.Depot.Stored<20 && Time.time<timeout) yield return null;
            Assert.That(session.Depot.Stored,Is.GreaterThanOrEqualTo(20),"Gathering state: "+worker.GetComponent<Gatherer>().State+"; cargo: "+worker.GetComponent<Gatherer>().Carried+"; position: "+worker.transform.position+"; destination: "+worker.Motor.Destination);
            Assert.That(construction.BeginPlacement(),Is.True);Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);
            Assert.That(construction.LastSite.Blueprint,Is.SameAs(worker.GetComponent<UnitIdentity>().Blueprint.Builds[0]));
            timeout=Time.time+25;while(!construction.LastSite.Complete && Time.time<timeout) yield return null;
            Assert.That(construction.LastSite.Complete,Is.True);
            session.Depot.Deposit(10);selection.SelectBuilding(construction.LastSite);Assert.That(selection.OrderProduction(),Is.True);
            var producer=selection.SelectedProducer;timeout=Time.time+12;while(producer.LastProduced==null && Time.time<timeout) yield return null;
            Assert.That(producer.LastProduced,Is.Not.Null);
            Assert.That(producer.LastProduced.GetComponent<UnitIdentity>().Blueprint,Is.SameAs(worker.GetComponent<UnitIdentity>().Blueprint));
            Assert.That(producer.LastProduced.GetComponent<Builder>().CanBuild(construction.LastSite.Blueprint),Is.True);
        }
        [Test] public void UnseededCyclesWarnAndMissingLinksAreErrors()
        {
            var data=Object.Instantiate(session.Definition);
            try
            {
                data.Configure("No seed",data.StartingBase,0,System.Array.Empty<StartingUnit>(),data.Units.ToArray(),data.Buildings.ToArray());
                var report=CivilizationValidator.Validate(data);
                Assert.That(report.CanInstantiate,Is.True);Assert.That(report.ReachableUnits,Is.Empty);
                Assert.That(report.Warnings.Any(w=>w.Contains("unreachable")),Is.True);
                data.Configure("Missing roster",data.StartingBase,0,new[]{new StartingUnit(session.Definition.Units[0],1)},System.Array.Empty<UnitBlueprint>(),data.Buildings.ToArray());
                Assert.That(CivilizationValidator.Validate(data).Errors,Is.Not.Empty);
            }
            finally{Object.DestroyImmediate(data);}
        }
        [Test] public void StartingProductionSeedsCyclesAndDuplicateIdsFail()
        {
            var data=Object.Instantiate(session.Definition);
            var home=Object.Instantiate(data.StartingBase);
            var workshop=data.Buildings[1];
            try
            {
                // A production-capable starting base can seed the same worker/workshop cycle.
                home.Configure("starting-producer",workshop.Prefab,data.Units[0]);
                var template=Object.Instantiate(workshop.Prefab.gameObject);
                template.AddComponent<ResourceDepot>();
                try
                {
                    home.Configure("starting-producer",template.GetComponent<BuildingSite>(),data.Units[0]);
                    data.Configure("Seeded",home,40,System.Array.Empty<StartingUnit>(),data.Units.ToArray(),new[]{home,workshop});
                    var report=CivilizationValidator.Validate(data);
                    Assert.That(report.CanInstantiate,Is.True);Assert.That(report.ReachableUnits,Does.Contain(data.Units[0]));
                    Assert.That(report.ReachableBuildings,Does.Contain(workshop));
                    home.Configure(data.Units[0].Id,template.GetComponent<BuildingSite>(),data.Units[0]);
                    Assert.That(CivilizationValidator.Validate(data).Errors.Any(e=>e.Contains("Duplicate blueprint ID")),Is.True);
                }
                finally{Object.DestroyImmediate(template);}
            }
            finally{Object.DestroyImmediate(home);Object.DestroyImmediate(data);}
        }
        [Test] public void PermissionsAreEnforcedAndEconomyWarningsDoNotForbidChallengeDesigns()
        {
            var data=Object.Instantiate(session.Definition);var unit=Object.Instantiate(data.Units[0]);
            var worker=Object.FindObjectsByType<SelectableUnit>()[0];
            try
            {
                unit.Configure("visitor","Visitor",unit.Production,false);
                UnitIdentity.Apply(worker.gameObject,unit);
                Assert.That(worker.GetComponent<Gatherer>().Gather(Object.FindAnyObjectByType<ResourceNode>()),Is.False);
                Assert.That(worker.GetComponent<Builder>().CanBuild(data.Buildings[1]),Is.False);
                var selection=Object.FindAnyObjectByType<SelectionController>();selection.Select(worker);session.Depot.Deposit(100);
                Assert.That(Object.FindAnyObjectByType<ConstructionController>().BeginPlacement(),Is.False);
                data.Configure("Challenge",data.StartingBase,0,new[]{new StartingUnit(unit,1)},new[]{unit},new[]{data.StartingBase});
                var report=CivilizationValidator.Validate(data);Assert.That(report.CanInstantiate,Is.True);
                Assert.That(report.Warnings.Any(w=>w.Contains("No reachable unit can gather")),Is.True);
            }
            finally{Object.DestroyImmediate(unit);Object.DestroyImmediate(data);}
        }
    }
}
