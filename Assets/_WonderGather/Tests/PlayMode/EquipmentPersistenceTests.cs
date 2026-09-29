using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class EquipmentPersistenceTests
    {
        private FactionCreator creator;
        private FactionStore store;
        private string root;
        private ToolDefinition Pickaxe=>creator.Draft.Tools.Single(x=>x.Id=="pickaxe");
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");yield return null;
            creator=UnityEngine.Object.FindAnyObjectByType<FactionCreator>();
            root=Path.Combine(Path.GetTempPath(),"WonderGather-EquipmentSaveTests-"+Guid.NewGuid().ToString("N"));
            store=new FactionStore(root);creator.ConfigureStorage(store);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");yield return null;
            string resolved=Path.GetFullPath(root),prefix=Path.Combine(Path.GetFullPath(Path.GetTempPath()),"WonderGather-EquipmentSaveTests-");
            Assert.That(resolved.StartsWith(prefix,StringComparison.OrdinalIgnoreCase),Is.True);
            if(Directory.Exists(resolved)) Directory.Delete(resolved,true);
        }
        private IEnumerator Transition()
        {float end=Time.realtimeSinceStartup+20;while(creator.Busy && Time.realtimeSinceStartup<end)yield return null;Assert.That(creator.Busy,Is.False);}
        [Test] public void EquipmentIsAnIndependentBlueprintChoiceAcrossDuplicateCopyAndOpen()
        {
            var w=creator.Workspace;var d=w.Draft;var tool=Pickaxe;
            Assert.That(d.Worker.Tool,Is.Null);Assert.That(w.IsDirty,Is.False);
            d.SetTool(d.Worker,tool);Assert.That(w.IsDirty,Is.True);
            d.SetTool(d.Worker,null);Assert.That(w.IsDirty,Is.False);
            d.SetTool(d.Worker,tool);var duplicate=d.AddUnit(d.Worker);
            Assert.That(duplicate.Id,Is.Not.EqualTo(d.Worker.Id));Assert.That(duplicate.Tool,Is.SameAs(tool));
            d.SetTool(duplicate,null);Assert.That(d.Worker.Tool,Is.SameAs(tool));
            d.ConfigureUnit(d.Worker,"Miner",false,false);d.SetPerformance(d.Worker,new UnitPerformance(150,3,75,200));
            d.SetFactionSetup("Equipped faction",60);Assert.That(d.Worker.Tool,Is.SameAs(tool));
            Assert.That(w.Save(),Is.True,w.Message);string original=w.CurrentId,originalText=File.ReadAllText(store.FilePath(original));
            Assert.That(w.Save(true,"Equipped copy"),Is.True,w.Message);string copy=w.CurrentId;
            Assert.That(copy,Is.Not.EqualTo(original));Assert.That(File.ReadAllText(store.FilePath(original)),Is.EqualTo(originalText));
            d.SetTool(d.Worker,null);Assert.That(w.IsDirty,Is.True);
            Assert.That(w.Open(copy,true),Is.True,w.Message);Assert.That(w.Draft.Worker.Tool,Is.SameAs(tool));
            Assert.That(w.Draft.Units[1].Tool,Is.Null);Assert.That(w.Draft.Worker.Performance.capacity,Is.EqualTo(3));Assert.That(w.IsDirty,Is.False);
            Assert.That(w.New(),Is.True);Assert.That(w.Draft.Worker.Tool,Is.Null,"Editing a draft cannot mutate the authored worker.");
            Assert.That(w.Draft.Tools.Single(),Is.SameAs(tool));Assert.That(tool.Id,Is.EqualTo("pickaxe"));
        }
        [Test] public void CatalogOwnershipAndStrictEquipmentFieldsAreValidated()
        {
            var d=creator.Draft;var foreign=UnityEngine.Object.Instantiate(Pickaxe);
            try
            {
                Assert.Throws<ArgumentException>(()=>d.SetTool(d.Worker,foreign));Assert.That(d.Worker.Tool,Is.Null);
                var invalidCatalog=UnityEngine.Object.Instantiate(d.Definition);
                try{invalidCatalog.ConfigureTools(Pickaxe,foreign);Assert.That(CivilizationValidator.Validate(invalidCatalog).CanInstantiate,Is.False);}
                finally{UnityEngine.Object.Destroy(invalidCatalog);}
                var source=Pickaxe;string longest=new string('x',64);
                foreign.Configure(longest,longest,source.Prefab,source.PrimaryGrip,source.SecondaryGrip,source.Head,source.HeadRadius);
                Assert.That(foreign.IsValid,Is.True);
                var boundaryCatalog=UnityEngine.Object.Instantiate(d.Definition);
                try
                {
                    boundaryCatalog.ConfigureTools(foreign);
                    using(var boundaryDraft=new FactionDraft(boundaryCatalog))
                    {
                        boundaryDraft.SetTool(boundaryDraft.Worker,foreign);
                        var record=FactionRecord.Decode(FactionRecord.Capture(boundaryDraft,Guid.NewGuid().ToString("N")).Encode());
                        Assert.That(record.Units[0].ToolId,Is.EqualTo(longest));
                        using(var reopened=record.CreateDraft(boundaryCatalog)) Assert.That(reopened.Worker.Tool,Is.SameAs(foreign));
                    }
                }
                finally{UnityEngine.Object.Destroy(boundaryCatalog);}
                foreach(string invalid in new[]{new string('x',65),"broken\nkey","broken\tkey"})
                {
                    Assert.Throws<ArgumentException>(()=>foreign.Configure(invalid,"Pickaxe",source.Prefab,source.PrimaryGrip,source.SecondaryGrip,source.Head,source.HeadRadius));
                    Assert.Throws<ArgumentException>(()=>foreign.Configure(longest,invalid,source.Prefab,source.PrimaryGrip,source.SecondaryGrip,source.Head,source.HeadRadius));
                    Assert.That(foreign.Id,Is.EqualTo(longest));Assert.That(foreign.IsValid,Is.True,"Rejected authoring must preserve the existing valid definition.");
                }
                // Serialized authoring bypasses Configure; admission must still fail safely.
                JsonUtility.FromJsonOverwrite("{\"id\":\""+new string('x',65)+"\"}",foreign);Assert.That(foreign.IsValid,Is.False);
                Assert.Throws<ArgumentException>(()=>d.Worker.SetTool(foreign));
                JsonUtility.FromJsonOverwrite("{\"id\":\"broken\\nkey\"}",foreign);Assert.That(foreign.IsValid,Is.False);
                Assert.Throws<ArgumentException>(()=>d.Worker.SetTool(foreign));
            }
            finally{UnityEngine.Object.Destroy(foreign);}
            string valid=FactionRecord.Capture(d,Guid.NewGuid().ToString("N")).Encode();
            const string field="\"tool\": \"\"";
            foreach(string bad in new[]{valid.Replace(field, "\"tool\": null"),valid.Replace(field,"\"tool\": 3"),valid.Replace(field+",", ""),valid.Replace(field,field+", "+field),valid.Replace(field,"\"tool\": \" \""),valid.Replace(field,"\"tool\": \""+new string('x',65)+"\"")})
                Assert.Throws<InvalidDataException>(()=>FactionRecord.Decode(bad));
        }
        [Test] public void UnknownToolCannotReplaceDraftOrRewriteStoredChoice()
        {
            var w=creator.Workspace;w.Draft.SetTool(w.Draft.Worker,Pickaxe);Assert.That(w.Save(),Is.True,w.Message);
            string id=w.CurrentId,path=store.FilePath(id);var external=store.Read(id);external.Record.Units[0].ToolId="future-tool";
            store.Save(external.Record,external.Token);string unknown=File.ReadAllText(path);var before=w.Draft;
            Assert.That(FactionRecord.Decode(unknown).Units[0].ToolId,Is.EqualTo("future-tool"));
            Assert.That(w.Open(id,true),Is.False);Assert.That(w.Draft,Is.SameAs(before));Assert.That(w.Draft.Worker.Tool,Is.SameAs(Pickaxe));
            Assert.That(w.List().Single().CanOpen,Is.False);Assert.That(w.Save(),Is.False);
            Assert.That(File.ReadAllText(path),Is.EqualTo(unknown));
        }
        [Test] public void VersionFourDefaultsToNoneAndExplicitUpgradeKeepsExactBackup()
        {
            var w=creator.Workspace;string id=Guid.NewGuid().ToString("N");
            w.Draft.SetPerformance(w.Draft.Worker,new UnitPerformance(150,3,75,200));
            string old=FactionRecord.Capture(w.Draft,id).Encode().Replace("\"version\": 5","\"version\": 4");
            old=Regex.Replace(old,",\\s*\"tool\"\\s*:\\s*\"[^\"]*\"","");
            Directory.CreateDirectory(root);File.WriteAllText(store.FilePath(id),old);
            Assert.That(w.Open(id,true),Is.True,w.Message);Assert.That(w.Draft.Worker.Tool,Is.Null);
            Assert.That(w.Draft.Worker.Performance,Is.EqualTo(new UnitPerformance(150,3,75,200)));Assert.That(w.IsDirty,Is.False);
            Assert.That(File.ReadAllText(store.FilePath(id)),Is.EqualTo(old));
            Assert.That(w.Save(),Is.True,w.Message);Assert.That(File.ReadAllText(store.FilePath(id)+".bak"),Is.EqualTo(old));
            string upgraded=File.ReadAllText(store.FilePath(id));StringAssert.Contains("\"version\": 5",upgraded);StringAssert.Contains("\"tool\": \"\"",upgraded);
            Assert.That(w.Open(id),Is.True);Assert.That(w.Draft.Worker.Tool,Is.Null);
        }
        [Test] public void FailedWriteKeepsEquippedDraftAndPriorNoneSave()
        {
            var w=creator.Workspace;Assert.That(w.Save(),Is.True);string path=store.FilePath(w.CurrentId),before=File.ReadAllText(path);
            Directory.CreateDirectory(path+".bak");w.Draft.SetTool(w.Draft.Worker,Pickaxe);
            Assert.That(w.Save(),Is.False);Assert.That(w.IsDirty,Is.True);Assert.That(w.Draft.Worker.Tool,Is.SameAs(Pickaxe));
            Assert.That(File.ReadAllText(path),Is.EqualTo(before));Assert.That(Directory.GetFiles(root,"*.tmp"),Is.Empty);
        }
        [UnityTest] public IEnumerator StartingAndTrainedWorkersShareSavedEquipmentAndReturnPreservesIt()
        {
            var d=creator.Draft;d.SetStartingSetup("Equipped worker",1,120);d.SetTool(d.Worker,Pickaxe);
            d.SetPerformance(d.Worker,new UnitPerformance(200,5,100,200));Assert.That(creator.Workspace.Save(),Is.True);
            string id=creator.Workspace.CurrentId;Assert.That(creator.Workspace.Open(id),Is.True);d=creator.Draft;var tool=d.Worker.Tool;
            Assert.That(creator.PlaytestEquipment(),Is.True);yield return Transition();Assert.That(creator.Playing,Is.True);
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("EquipmentPlaytest"));
            var unit=UnityEngine.Object.FindAnyObjectByType<SelectableUnit>();Assert.That(unit.GetComponent<EquippedTool>().Definition,Is.SameAs(tool));
            var selection=UnityEngine.Object.FindAnyObjectByType<SelectionController>();selection.Select(unit);
            var construction=UnityEngine.Object.FindAnyObjectByType<ConstructionController>();
            Assert.That(construction.ChooseBuilding(d.Workshop),Is.True);Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);
            float end=Time.time+25;while(!construction.LastSite.Complete && Time.time<end)yield return null;
            Assert.That(construction.LastSite.Complete,Is.True);var producer=construction.LastSite.GetComponent<UnitProducer>();
            Assert.That(producer.Enqueue(d.Worker),Is.True);end=Time.time+15;while(producer.LastProduced==null && Time.time<end)yield return null;
            Assert.That(producer.LastProduced,Is.Not.Null);Assert.That(producer.LastProduced.GetComponent<EquippedTool>().Definition,Is.SameAs(tool));
            var plain=UnityEngine.Object.Instantiate(d.Worker);
            try{plain.SetTool(null);UnitIdentity.Apply(producer.LastProduced.gameObject,plain);Assert.That(producer.LastProduced.GetComponent<EquippedTool>().Definition,Is.Null);}
            finally{UnityEngine.Object.Destroy(plain);}
            Assert.That(creator.ReturnToCreator(),Is.True);yield return Transition();
            Assert.That(creator.Draft.Worker.Tool,Is.SameAs(tool));Assert.That(creator.Workspace.IsDirty,Is.False);Assert.That(creator.Workspace.CurrentId,Is.EqualTo(id));
        }
    }
}
