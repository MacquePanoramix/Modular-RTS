using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class MultipleBlueprintTests
    {
        private FactionCreator creator;private FactionStore store;private string root;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");yield return null;
            creator=UnityEngine.Object.FindAnyObjectByType<FactionCreator>();
            root=Path.Combine(Path.GetTempPath(),"WonderGather-UnitsTests-"+Guid.NewGuid().ToString("N"));
            store=new FactionStore(root);creator.ConfigureStorage(store);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");yield return null;
            string resolved=Path.GetFullPath(root),prefix=Path.Combine(Path.GetFullPath(Path.GetTempPath()),"WonderGather-UnitsTests-");
            Assert.That(resolved.StartsWith(prefix,StringComparison.OrdinalIgnoreCase),Is.True);
            if(Directory.Exists(resolved)) Directory.Delete(resolved,true);
        }
        private IEnumerator Transition()
        {float deadline=Time.realtimeSinceStartup+20;while(creator.Busy && Time.realtimeSinceStartup<deadline) yield return null;Assert.That(creator.Busy,Is.False);}
        [Test] public void IndependentCopiesCountsAndRemovalPreserveOtherBlueprints()
        {
            var d=creator.Draft;d.SetStartingCount(d.Worker,2);var original=d.Worker;
            d.ConfigureUnit(original,"Gatherer",true,false);var copy=d.AddUnit(original);
            Assert.That(copy.Id,Is.Not.EqualTo(original.Id));Assert.That(d.StartingCount(copy),Is.Zero);
            Assert.That(copy.GathersSupplies,Is.True);Assert.That(copy.Builds,Is.Empty);
            d.ConfigureUnit(copy,"Builder",false,true);d.SetStartingCount(copy,99);
            Assert.That(d.TotalStartingUnits,Is.EqualTo(8));Assert.That(d.StartingCount(copy),Is.EqualTo(6));
            Assert.That(original.GathersSupplies,Is.True);Assert.That(original.Builds,Is.Empty);
            d.SetProductionUnit(copy);string originalId=original.Id;
            Assert.That(d.RemoveUnit(copy),Is.True);Assert.That(d.Workshop.Produces,Is.Null);
            Assert.That(d.TotalStartingUnits,Is.EqualTo(2));Assert.That(d.Worker.Id,Is.EqualTo(originalId));
            Assert.That(d.RemoveUnit(original),Is.False);Assert.That(d.Report.CanInstantiate,Is.True);
        }
        [Test] public void BlueprintLimitsNamesAndForeignReferencesAreValidated()
        {
            var d=creator.Draft;while(d.Units.Count<FactionDraft.MaxUnitBlueprints) Assert.That(d.AddUnit(),Is.Not.Null);
            Assert.That(d.AddUnit(),Is.Null);Assert.That(d.Units.Select(x=>x.Id).Distinct().Count(),Is.EqualTo(8));
            d.ConfigureUnit(d.Worker," ",true,true);Assert.That(d.Report.CanInstantiate,Is.False);Assert.That(creator.Workspace.Save(),Is.False);
            d.ConfigureUnit(d.Worker,"Worker",true,true);Assert.That(d.Report.CanInstantiate,Is.True);
            var foreign=ScriptableObject.CreateInstance<UnitBlueprint>();
            try{Assert.Throws<ArgumentException>(()=>d.SetProductionUnit(foreign));Assert.Throws<ArgumentException>(()=>d.AddUnit(foreign));}
            finally{UnityEngine.Object.Destroy(foreign);}
        }
        [Test] public void AllChoicesRoundTripAfterOriginalBlueprintIsRemoved()
        {
            var w=creator.Workspace;var d=w.Draft;d.SetStartingSetup("Mixed Garden",2,60);
            var unit=d.AddUnit();d.ConfigureUnit(unit,"Keeper",false,true);d.SetStartingCount(unit,3);d.SetProductionUnit(unit);
            string id=unit.Id;d.RemoveUnit(d.Worker);Assert.That(w.Save(),Is.True,w.Message);string faction=w.CurrentId;
            Assert.That(w.IsDirty,Is.False);d.ConfigureUnit(unit,"Unsaved",true,false);Assert.That(w.IsDirty,Is.True);
            Assert.That(w.Open(faction,true),Is.True,w.Message);Assert.That(w.Draft.Worker.Id,Is.EqualTo(id));
            Assert.That(w.Draft.Worker.DisplayName,Is.EqualTo("Keeper"));Assert.That(w.Draft.Worker.GathersSupplies,Is.False);
            Assert.That(w.Draft.Worker.CanBuild(w.Draft.Workshop),Is.True);Assert.That(w.Draft.StartingWorkers,Is.EqualTo(3));
            Assert.That(w.Draft.Workshop.Produces,Is.SameAs(w.Draft.Worker));Assert.That(w.IsDirty,Is.False);
            var added=w.Draft.AddUnit();Assert.That(w.IsDirty,Is.True);w.Draft.RemoveUnit(added);Assert.That(w.IsDirty,Is.False);
        }
        private string Legacy(string id)
        {
            var d=creator.Draft;
            return "{\"version\":1,\"template\":\"settlement-1\",\"id\":\""+id+"\",\"name\":\"Legacy garden\",\"base\":\""+d.Definition.StartingBase.Id+"\",\"worker\":\""+d.TemplateWorkerId+"\",\"workshop\":\""+d.Workshop.Id+"\",\"workers\":3,\"supplies\":40,\"gathers\":false,\"builds\":true,\"trains\":false}";
        }
        [Test] public void LegacySavesLoadWithoutRewritingAndUpgradeWithBackupOnlyOnSave()
        {
            Directory.CreateDirectory(root);string id=Guid.NewGuid().ToString("N"),old=Legacy(id);File.WriteAllText(store.FilePath(id),old);
            var w=creator.Workspace;Assert.That(w.Open(id),Is.True,w.Message);Assert.That(w.IsDirty,Is.False);
            Assert.That(File.ReadAllText(store.FilePath(id)),Is.EqualTo(old));Assert.That(w.Draft.StartingWorkers,Is.EqualTo(3));
            Assert.That(w.Draft.Worker.GathersSupplies,Is.False);Assert.That(w.Draft.Workshop.Produces,Is.Null);
            Assert.That(w.Save(),Is.True);Assert.That(File.ReadAllText(store.FilePath(id)+".bak"),Is.EqualTo(old));
            StringAssert.Contains("\"version\": 2",File.ReadAllText(store.FilePath(id)));
            Assert.That(w.Open(id),Is.True);Assert.That(w.Draft.StartingWorkers,Is.EqualTo(3));
        }
        [Test] public void InvalidLinksDuplicateIdsAndUnknownNestedDataAreRejected()
        {
            var record=FactionRecord.Capture(creator.Draft,Guid.NewGuid().ToString("N"));string valid=record.Encode();
            foreach(string bad in new[]{valid.Replace("\"start\": 8","\"start\": -1"),valid.Replace("\"start\": 8","\"extra\": 42, \"start\": 8"),valid.Replace("\"start\": 8","\"start\": 1, \"start\": 8"),valid.Replace("\"units\": [","\"units\": null, \"discarded\": [")})
                Assert.Throws<InvalidDataException>(()=>FactionRecord.Decode(bad));
            record.TrainedId="missing";Assert.Throws<InvalidDataException>(()=>record.Encode());
            record.TrainedId=null;record.Units=new[]{record.Units[0],record.Units[0]};Assert.Throws<InvalidDataException>(()=>record.Encode());
            string legacy=Legacy(record.Id).Replace("\"workers\":3","\"workers\":3,\"future\":42");Assert.Throws<InvalidDataException>(()=>FactionRecord.Decode(legacy));
        }
        [UnityTest] public IEnumerator MixedStartingUnitsAndWorkshopProductionKeepChosenCapabilities()
        {
            var d=creator.Draft;d.SetStartingSetup("Mixed Garden",1,60);d.ConfigureUnit(d.Worker,"Gatherer",true,false);
            var builder=d.AddUnit();d.ConfigureUnit(builder,"Builder",false,true);d.SetStartingCount(builder,1);d.SetProductionUnit(builder);
            Assert.That(creator.Workspace.Save(),Is.True);string id=creator.Workspace.CurrentId;
            Assert.That(creator.Workspace.Open(id),Is.True);d=creator.Draft;builder=d.Units.Single(x=>x.DisplayName=="Builder");
            Assert.That(creator.Playtest(),Is.True);yield return Transition();
            var units=UnityEngine.Object.FindObjectsByType<UnitIdentity>();Assert.That(units.Length,Is.EqualTo(2));
            var instance=units.Single(x=>x.Blueprint==builder);var node=UnityEngine.Object.FindAnyObjectByType<ResourceNode>();
            Assert.That(instance.GetComponent<Gatherer>().Gather(node),Is.False);
            var selection=UnityEngine.Object.FindAnyObjectByType<SelectionController>();selection.Select(instance.GetComponent<SelectableUnit>());
            var construction=UnityEngine.Object.FindAnyObjectByType<ConstructionController>();Assert.That(construction.BeginPlacement(),Is.True);
            Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);
            float deadline=Time.time+30;while(!construction.LastSite.Complete && Time.time<deadline) yield return null;
            Assert.That(construction.LastSite.Complete,Is.True);var producer=construction.LastSite.GetComponent<UnitProducer>();
            Assert.That(producer.UnitName,Is.EqualTo("Builder"));Assert.That(producer.Enqueue(),Is.True);
            deadline=Time.time+15;while(producer.LastProduced==null && Time.time<deadline) yield return null;
            Assert.That(producer.LastProduced,Is.Not.Null);Assert.That(producer.LastProduced.GetComponent<UnitIdentity>().Blueprint,Is.SameAs(builder));
            Assert.That(producer.LastProduced.GetComponent<Gatherer>().Gather(node),Is.False);
            Assert.That(creator.ReturnToCreator(),Is.True);yield return Transition();Assert.That(creator.Workspace.IsDirty,Is.False);
        }
    }
}
