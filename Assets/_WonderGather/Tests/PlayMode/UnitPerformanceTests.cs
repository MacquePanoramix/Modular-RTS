using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class UnitPerformanceTests
    {
        private FactionCreator creator;private FactionStore store;private string root;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");yield return null;
            creator=UnityEngine.Object.FindAnyObjectByType<FactionCreator>();
            root=Path.Combine(Path.GetTempPath(),"WonderGather-StatsTests-"+Guid.NewGuid().ToString("N"));
            store=new FactionStore(root);creator.ConfigureStorage(store);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");yield return null;
            string path=Path.GetFullPath(root),prefix=Path.Combine(Path.GetFullPath(Path.GetTempPath()),"WonderGather-StatsTests-");
            Assert.That(path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase),Is.True);if(Directory.Exists(path))Directory.Delete(path,true);
        }
        private IEnumerator Transition(){float end=Time.realtimeSinceStartup+20;while(creator.Busy&&Time.realtimeSinceStartup<end)yield return null;Assert.That(creator.Busy,Is.False);}
        [Test] public void ChangesDuplicateAndRoundTripWithoutMutatingTemplate()
        {
            var w=creator.Workspace;var d=w.Draft;var stats=new UnitPerformance(150,12,75,200);
            Assert.That(d.Worker.Performance,Is.EqualTo(UnitPerformance.Default));
            d.SetPerformance(d.Worker,stats);Assert.That(w.IsDirty,Is.True);
            var copy=d.AddUnit(d.Worker);Assert.That(copy.Performance,Is.EqualTo(stats));
            d.SetPerformance(copy,UnitPerformance.Default);Assert.That(d.Worker.Performance,Is.EqualTo(stats));
            Assert.That(w.Save(),Is.True,w.Message);string id=w.CurrentId;Assert.That(w.IsDirty,Is.False);
            d.SetPerformance(d.Worker,UnitPerformance.Default);Assert.That(w.IsDirty,Is.True);
            Assert.That(w.Open(id,true),Is.True,w.Message);Assert.That(w.Draft.Worker.Performance,Is.EqualTo(stats));
            Assert.That(w.Draft.Units[1].Performance,Is.EqualTo(UnitPerformance.Default));Assert.That(w.IsDirty,Is.False);
            Assert.That(w.New(),Is.True);Assert.That(w.Draft.Worker.Performance,Is.EqualTo(UnitPerformance.Default));
        }
        [Test] public void InvalidStatsAndUnknownNestedDataCannotBeSavedOrLoaded()
        {
            var d=creator.Draft;
            foreach(var bad in new[]{new UnitPerformance(0,5,100,100),new UnitPerformance(100,21,100,100),new UnitPerformance(100,5,201,100),new UnitPerformance(100,5,100,-1)})
            {
                Assert.Throws<ArgumentOutOfRangeException>(()=>d.SetPerformance(d.Worker,bad));
                var record=FactionRecord.Capture(d,Guid.NewGuid().ToString("N"));record.Units[0].Performance=bad;Assert.Throws<InvalidDataException>(()=>record.Encode());
            }
            string valid=FactionRecord.Capture(d,Guid.NewGuid().ToString("N")).Encode();
            foreach(string replacement in new[]{"\"movement\": 0","\"movement\": 100, \"future\": 1","\"movement\": 100, \"movement\": 100","\"movement\": 50.5"})
                Assert.Throws<InvalidDataException>(()=>FactionRecord.Decode(valid.Replace("\"movement\": 100",replacement)));
        }
        [Test] public void VersionThreeDefaultsRemainReadOnlyUntilExplicitUpgrade()
        {
            var w=creator.Workspace;string id=Guid.NewGuid().ToString("N");
            string old=FactionRecord.Capture(w.Draft,id).Encode().Replace("\"version\": 4","\"version\": 3");
            old=Regex.Replace(old,",\\s*\"performance\"\\s*:\\s*\\{[^}]*\\}","");
            Directory.CreateDirectory(root);File.WriteAllText(store.FilePath(id),old);
            Assert.That(w.Open(id),Is.True,w.Message);Assert.That(w.Draft.Worker.Performance,Is.EqualTo(UnitPerformance.Default));
            Assert.That(File.ReadAllText(store.FilePath(id)),Is.EqualTo(old));Assert.That(w.Save(),Is.True,w.Message);
            Assert.That(File.ReadAllText(store.FilePath(id)+".bak"),Is.EqualTo(old));StringAssert.Contains("\"version\": 4",File.ReadAllText(store.FilePath(id)));
        }
        [UnityTest] public IEnumerator StartingWorkerUsesRatesAndCompletesFasterConstruction()
        {
            var d=creator.Draft;d.SetStartingSetup("Fast builder",1,120);d.SetPerformance(d.Worker,new UnitPerformance(200,2,200,200));
            Assert.That(creator.Playtest(),Is.True);yield return Transition();
            var unit=UnityEngine.Object.FindAnyObjectByType<SelectableUnit>();var agent=unit.GetComponent<NavMeshAgent>();var worker=unit.GetComponent<Gatherer>();
            float speed=agent.speed;Assert.That(speed,Is.EqualTo(6.4f).Within(.01f));
            UnitIdentity.Apply(unit.gameObject,d.Worker);Assert.That(agent.speed,Is.EqualTo(speed));Assert.That(worker.Capacity,Is.EqualTo(2));Assert.That(worker.SecondsPerUnit,Is.EqualTo(.25f));
            var selection=UnityEngine.Object.FindAnyObjectByType<SelectionController>();selection.Select(unit);
            var construction=UnityEngine.Object.FindAnyObjectByType<ConstructionController>();Assert.That(construction.ChooseBuilding(d.Workshop),Is.True);Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);
            var site=construction.LastSite;float end=Time.time+15;while(site.Progress==0&&Time.time<end)yield return null;Assert.That(site.Progress,Is.GreaterThan(0));
            float started=Time.time;end=started+6;while(!site.Complete&&Time.time<end)yield return null;
            Assert.That(site.Complete,Is.True);Assert.That(Time.time-started,Is.InRange(3.5f,4.5f));
            var producer=site.GetComponent<UnitProducer>();Assert.That(producer.Enqueue(d.Worker),Is.True);
            end=Time.time+12;while(producer.LastProduced==null&&Time.time<end)yield return null;Assert.That(producer.LastProduced,Is.Not.Null);
            Assert.That(producer.LastProduced.GetComponent<NavMeshAgent>().speed,Is.EqualTo(speed));Assert.That(producer.LastProduced.GetComponent<Gatherer>().Capacity,Is.EqualTo(2));Assert.That(producer.LastProduced.GetComponent<Builder>().WorkRate,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator GatheringUsesCapacityAndRateAndStillConservesSupplies()
        {
            var d=creator.Draft;d.SetStartingSetup("Gatherer",1,0);d.SetPerformance(d.Worker,new UnitPerformance(100,2,200,100));
            Assert.That(creator.Playtest(),Is.True);yield return Transition();
            var worker=UnityEngine.Object.FindAnyObjectByType<Gatherer>();var node=UnityEngine.Object.FindAnyObjectByType<ResourceNode>();var bank=creator.Bridge.Session.Depot;
            int total=node.Remaining;Assert.That(worker.Gather(node),Is.True);float end=Time.time+20;
            while(worker.State!=Gatherer.Activity.Gathering&&Time.time<end)yield return null;Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Gathering));
            float start=Time.time;end=start+2;while(worker.State==Gatherer.Activity.Gathering&&Time.time<end)yield return null;
            Assert.That(worker.Carried,Is.EqualTo(2));Assert.That(Time.time-start,Is.InRange(.4f,.8f));
            end=Time.time+20;while(bank.Stored==0&&Time.time<end){Assert.That(node.Remaining+worker.Carried+bank.Stored,Is.EqualTo(total));yield return null;}
            Assert.That(bank.Stored,Is.EqualTo(2));Assert.That(node.Remaining+worker.Carried+bank.Stored,Is.EqualTo(total));
        }
    }
}
