using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class BuildingBlueprintTests
    {
        private FactionCreator creator;private FactionStore store;private string root;
        private readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");yield return null;
            creator=UnityEngine.Object.FindAnyObjectByType<FactionCreator>();root=Path.Combine(Path.GetTempPath(),"WonderGather-BuildingsTests-"+Guid.NewGuid().ToString("N"));
            store=new FactionStore(root);creator.ConfigureStorage(store);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");yield return null;
            foreach(var item in owned) if(item!=null) UnityEngine.Object.Destroy(item);owned.Clear();
            string path=Path.GetFullPath(root),prefix=Path.Combine(Path.GetFullPath(Path.GetTempPath()),"WonderGather-BuildingsTests-");Assert.That(path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase),Is.True);
            if(Directory.Exists(path))Directory.Delete(path,true);
        }
        private IEnumerator Transition(){float end=Time.realtimeSinceStartup+20;while(creator.Busy && Time.realtimeSinceStartup<end)yield return null;Assert.That(creator.Busy,Is.False);}
        [Test] public void DuplicateAndRemoveCleanEveryLinkWithoutChangingOtherChoices()
        {
            var d=creator.Draft;var first=d.Workshop;var second=d.AddBuilding(first);d.RenameBuilding(second,"Garden Hall");
            Assert.That(second.Id,Is.Not.EqualTo(first.Id));Assert.That(second.ProductionOptions.Single(),Is.SameAs(d.Worker));Assert.That(d.Worker.CanBuild(second),Is.False);
            d.SetBuildPermission(d.Worker,second,true);var copy=d.AddUnit(d.Worker);Assert.That(copy.CanBuild(second),Is.True);
            d.SetTraining(second,copy,true);Assert.That(first.ProductionOptions.Contains(copy),Is.False);
            Assert.That(d.RemoveUnit(copy),Is.True);Assert.That(second.ProductionOptions.Count(),Is.EqualTo(1));
            Assert.That(d.RemoveBuilding(first),Is.True);Assert.That(d.Worker.Builds.Single(),Is.SameAs(second));Assert.That(d.Workshop,Is.SameAs(second));
            Assert.That(d.RemoveBuilding(second),Is.False);Assert.That(d.Report.CanInstantiate,Is.True);
        }
        [Test] public void ReachabilityFollowsAllProductionOptionsAcrossBuildingChains()
        {
            var d=creator.Draft;d.SetStartingSetup("Chain",1,0);var b=d.AddBuilding();var next=d.AddUnit();var last=d.AddUnit();
            d.SetBuildPermission(d.Worker,b,false);d.SetBuildPermission(next,d.Workshop,false);d.SetBuildPermission(next,b,true);d.SetBuildPermission(last,d.Workshop,false);
            d.SetTraining(d.Workshop,next,true);d.SetTraining(b,last,true);
            Assert.That(d.Report.ReachableBuildings,Does.Contain(b));Assert.That(d.Report.ReachableUnits,Does.Contain(last));
            d.SetTraining(d.Workshop,next,false);Assert.That(d.Report.ReachableBuildings.Contains(b),Is.False);Assert.That(d.Report.ReachableUnits.Contains(last),Is.False);
        }
        [Test] public void RoundTripPreservesEntireGraphAfterOriginalWorkshopRemoval()
        {
            var w=creator.Workspace;var d=w.Draft;var unit=d.AddUnit();var building=d.AddBuilding();d.RenameBuilding(building,"Hall");
            d.SetBuildPermission(d.Worker,building,true);d.SetTraining(building,d.Worker,true);d.SetTraining(building,unit,true);d.RemoveBuilding(d.Workshop);
            string key=building.Id;Assert.That(w.Save(),Is.True,w.Message);string id=w.CurrentId;
            d.RenameBuilding(building,"Unsaved");Assert.That(w.IsDirty,Is.True);Assert.That(w.Open(id,true),Is.True,w.Message);
            d=w.Draft;Assert.That(d.Workshop.Id,Is.EqualTo(key));Assert.That(d.Workshop.DisplayName,Is.EqualTo("Hall"));
            Assert.That(d.Workshop.ProductionOptions.Count(),Is.EqualTo(2));Assert.That(d.Worker.CanBuild(d.Workshop),Is.True);Assert.That(w.IsDirty,Is.False);
            var copy=d.AddBuilding(d.Workshop);Assert.That(w.IsDirty,Is.True);d.RemoveBuilding(copy);Assert.That(w.IsDirty,Is.False);
        }
        [Test] public void LimitsInvalidNamesAndBrokenOrDuplicateLinksAreRejected()
        {
            var d=creator.Draft;while(d.Buildings.Count<FactionDraft.MaxBuildingBlueprints)d.AddBuilding();Assert.That(d.AddBuilding(),Is.Null);
            Assert.Throws<ArgumentException>(()=>d.RemoveBuilding(d.Definition.StartingBase));
            d.RenameBuilding(d.Workshop," ");Assert.That(d.Report.CanInstantiate,Is.False);Assert.That(creator.Workspace.Save(),Is.False);d.RenameBuilding(d.Workshop,"Workshop");
            var record=FactionRecord.Capture(d,Guid.NewGuid().ToString("N"));record.Units[0].BuildIds=new[]{"missing"};Assert.Throws<InvalidDataException>(()=>record.Encode());
            record=FactionRecord.Capture(d,record.Id);record.Buildings[0].Trains=new[]{d.Worker.Id,d.Worker.Id};Assert.Throws<InvalidDataException>(()=>record.Encode());
            record=FactionRecord.Capture(d,record.Id);record.Buildings[1].Id=record.Units[0].Id;Assert.Throws<InvalidDataException>(()=>record.Encode());
            string valid=FactionRecord.Capture(d,record.Id).Encode();Assert.Throws<InvalidDataException>(()=>FactionRecord.Decode(valid.Replace("\"trains\": [","\"future\": 1, \"trains\": [")));
        }
        [Test] public void VersionTwoLoadsCompleteChoicesWithoutRewritingThenBacksUpOnUpgrade()
        {
            var d=creator.Draft;string id=Guid.NewGuid().ToString("N"),second="unit-second";
            string old="{\"version\":2,\"template\":\"settlement-1\",\"id\":\""+id+"\",\"name\":\"Old garden\",\"base\":\""+d.Definition.StartingBase.Id+"\",\"worker\":\""+d.TemplateWorkerId+"\",\"workshop\":\""+d.TemplateWorkshopId+"\",\"supplies\":40,\"trained\":\""+second+"\",\"units\":[{\"id\":\""+d.Worker.Id+"\",\"name\":\"Gatherer\",\"start\":1,\"gathers\":true,\"builds\":false},{\"id\":\""+second+"\",\"name\":\"Builder\",\"start\":2,\"gathers\":false,\"builds\":true}]}";
            Directory.CreateDirectory(root);File.WriteAllText(store.FilePath(id),old);var w=creator.Workspace;
            Assert.That(w.Open(id),Is.True,w.Message);Assert.That(File.ReadAllText(store.FilePath(id)),Is.EqualTo(old));
            Assert.That(w.Draft.TotalStartingUnits,Is.EqualTo(3));Assert.That(w.Draft.Worker.Builds,Is.Empty);Assert.That(w.Draft.Workshop.Produces.Id,Is.EqualTo(second));
            Assert.That(w.Save(),Is.True);Assert.That(File.ReadAllText(store.FilePath(id)+".bak"),Is.EqualTo(old));StringAssert.Contains("\"version\": 3",File.ReadAllText(store.FilePath(id)));
        }
        [UnityTest] public IEnumerator SeparateBuildingsAndMixedQueueProduceTheRequestedBlueprints()
        {
            var d=creator.Draft;d.SetStartingSetup("Network",1,120);var hall=d.AddBuilding();d.RenameBuilding(hall,"Training Hall");var other=d.AddUnit();d.ConfigureUnit(other,"Gatherer",true,false);
            d.SetBuildPermission(d.Worker,hall,true);d.SetTraining(hall,d.Worker,true);d.SetTraining(hall,other,true);
            Assert.That(creator.Playtest(),Is.True);yield return Transition();
            var selection=UnityEngine.Object.FindAnyObjectByType<SelectionController>();var construction=UnityEngine.Object.FindAnyObjectByType<ConstructionController>();selection.Select(UnityEngine.Object.FindAnyObjectByType<SelectableUnit>());
            Assert.That(construction.AvailableBuildings().Count,Is.EqualTo(2));Assert.That(construction.ChooseBuilding(hall),Is.True);Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);
            float end=Time.time+30;while(!construction.LastSite.Complete&&Time.time<end)yield return null;Assert.That(construction.LastSite.Complete,Is.True);Assert.That(construction.LastSite.DisplayName,Is.EqualTo("Training Hall"));
            selection.SelectBuilding(construction.LastSite);var producer=selection.SelectedProducer;
            Assert.That(selection.OrderProduction(false,other),Is.True);Assert.That(selection.OrderProduction(false,d.Worker),Is.True);
            end=Time.time+20;while(producer.QueueCount>0&&Time.time<end)yield return null;Assert.That(producer.QueueCount,Is.Zero);
            var identities=UnityEngine.Object.FindObjectsByType<UnitIdentity>();Assert.That(identities.Count(x=>x.Blueprint==other),Is.EqualTo(1));Assert.That(identities.Count(x=>x.Blueprint==d.Worker),Is.EqualTo(2));
            Assert.That(producer.LastProduced.GetComponent<UnitIdentity>().Blueprint,Is.SameAs(d.Worker));Assert.That(creator.ReturnToCreator(),Is.True);yield return Transition();
        }
        private UnitBlueprint RecipeUnit(string name,int cost,float seconds)
        {
            var recipe=UnityEngine.Object.Instantiate(creator.Draft.Worker.Production);owned.Add(recipe);
            typeof(WorkerProductionDefinition).GetField("cost",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(recipe,cost);
            typeof(WorkerProductionDefinition).GetField("seconds",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(recipe,seconds);
            var unit=ScriptableObject.CreateInstance<UnitBlueprint>();owned.Add(unit);unit.Configure(name,name,recipe,true);return unit;
        }
        [UnityTest] public IEnumerator MixedOrdersSnapshotCostsTimesAndRefundOnlyTheirOwnPayment()
        {
            creator.Draft.SetStartingSetup("Queue",0,60);Assert.That(creator.Playtest(),Is.True);yield return Transition();
            var site=UnityEngine.Object.Instantiate(creator.Draft.Workshop.Prefab,new Vector3(-18,0,8),Quaternion.identity);site.CompleteAtStart();
            var producer=site.GetComponent<UnitProducer>();var bank=creator.Bridge.Session.Depot;producer.Configure(bank,UnityEngine.Object.FindAnyObjectByType<SelectionController>(),new Vector3(-15,0,8));
            var first=RecipeUnit("First",3,.2f);var second=RecipeUnit("Second",7,2);producer.SetBlueprints(new[]{first,second});
            var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.position=site.transform.position+Vector3.up;blocker.transform.localScale=new Vector3(16,2,16);Physics.SyncTransforms();
            Assert.That(producer.Enqueue(first),Is.True);Assert.That(producer.Enqueue(second),Is.True);Assert.That(bank.Stored,Is.EqualTo(50));
            typeof(WorkerProductionDefinition).GetField("cost",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(second.Production,99);
            typeof(WorkerProductionDefinition).GetField("seconds",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(first.Production,99f);
            yield return new WaitForSeconds(.5f);Assert.That(producer.Progress,Is.EqualTo(1));Assert.That(producer.WaitingForExit,Is.True);
            Assert.That(producer.CancelLast(),Is.True);Assert.That(bank.Stored,Is.EqualTo(57));Assert.That(producer.Progress,Is.EqualTo(1));
            Assert.Throws<InvalidOperationException>(()=>producer.SetBlueprints(new[]{second}));
            UnityEngine.Object.Destroy(site.gameObject);yield return null;Assert.That(bank.Stored,Is.EqualTo(60));
            Assert.That(creator.ReturnToCreator(),Is.True);yield return Transition();
        }
    }
}
