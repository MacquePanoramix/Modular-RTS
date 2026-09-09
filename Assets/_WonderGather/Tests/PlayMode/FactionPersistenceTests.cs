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
    public sealed class FactionPersistenceTests
    {
        private FactionCreator creator;private FactionStore store;private string root;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");yield return null;
            root=Path.Combine(Path.GetTempPath(),"WonderGather-LibraryTests-"+Guid.NewGuid().ToString("N"));
            store=new FactionStore(root);creator=UnityEngine.Object.FindAnyObjectByType<FactionCreator>();creator.ConfigureStorage(store);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");yield return null;
            string resolved=Path.GetFullPath(root),prefix=Path.Combine(Path.GetFullPath(Path.GetTempPath()),"WonderGather-LibraryTests-");
            Assert.That(resolved.StartsWith(prefix,StringComparison.OrdinalIgnoreCase),Is.True);
            if(Directory.Exists(resolved)) Directory.Delete(resolved,true);
        }
        [Test] public void RoundTripRestoresAllChoicesAndRejectsUnsavedReplacement()
        {
            var workspace=creator.Workspace;workspace.Draft.SetStartingSetup("Garden / ../ 🌿",3,40);workspace.Draft.SetWorkerPermissions(false,false);workspace.Draft.SetProduction(false);
            Assert.That(workspace.IsDirty,Is.True);Assert.That(workspace.Save(),Is.True,workspace.Message);Assert.That(workspace.IsDirty,Is.False);
            string id=workspace.CurrentId;Assert.That(Path.GetFileName(store.FilePath(id)),Is.EqualTo(id+".faction.json"));
            workspace.Draft.SetStartingSetup("Unsaved",8,0);var before=workspace.Draft;
            Assert.That(workspace.Open(id),Is.False);Assert.That(workspace.New(),Is.False);Assert.That(workspace.Draft,Is.SameAs(before));
            Assert.That(workspace.Open(id,true),Is.True);Assert.That(workspace.Draft.StartingWorkers,Is.EqualTo(3));Assert.That(workspace.Draft.Definition.StartingSupplies,Is.EqualTo(40));
            Assert.That(workspace.Draft.Worker.GathersSupplies,Is.False);Assert.That(workspace.Draft.Worker.Builds,Is.Empty);Assert.That(workspace.Draft.Workshop.Produces,Is.Null);
            Assert.That(workspace.Draft.Definition.DisplayName,Is.EqualTo("Garden / ../ 🌿"));Assert.That(workspace.IsDirty,Is.False);
        }
        [Test] public void CopyRenameDeletePreserveOriginalAndOpenDraft()
        {
            var workspace=creator.Workspace;Assert.That(workspace.Save(),Is.True);string original=workspace.CurrentId,originalText=File.ReadAllText(store.FilePath(original));
            Assert.That(workspace.Save(true,"Cave dwellers"),Is.True);string copy=workspace.CurrentId;Assert.That(copy,Is.Not.EqualTo(original));
            Assert.That(workspace.Rename(copy,"Moon garden"),Is.True);Assert.That(workspace.Draft.Definition.DisplayName,Is.EqualTo("Moon garden"));
            Assert.That(workspace.List().Count,Is.EqualTo(2));Assert.That(File.ReadAllText(store.FilePath(original)),Is.EqualTo(originalText));
            Assert.That(workspace.Delete(workspace.List().Single(x=>x.Id==copy)),Is.True);
            Assert.That(workspace.Draft.Definition.DisplayName,Is.EqualTo("Moon garden"));Assert.That(workspace.HasFile,Is.False);Assert.That(workspace.IsDirty,Is.True);
            Assert.That(workspace.List().Count,Is.EqualTo(1));Assert.That(Directory.GetFiles(Path.Combine(root,"Deleted")).Length,Is.EqualTo(1));
            Assert.That(workspace.Save(),Is.True);Assert.That(workspace.CurrentId,Is.Not.EqualTo(copy));
        }
        [Test] public void AtomicUpdatesKeepBackupsAndDetectExternalChanges()
        {
            var workspace=creator.Workspace;Assert.That(workspace.Save(),Is.True);string id=workspace.CurrentId;string first=File.ReadAllText(store.FilePath(id));
            workspace.Draft.SetStartingSetup("Second",3,20);Assert.That(workspace.Save(),Is.True,workspace.Message);Assert.That(File.ReadAllText(store.FilePath(id)+".bak"),Is.EqualTo(first));
            string second=File.ReadAllText(store.FilePath(id));workspace.Draft.SetStartingSetup("Third",4,30);Assert.That(workspace.Save(),Is.True,workspace.Message);
            Assert.That(File.ReadAllText(store.FilePath(id)+".bak"),Is.EqualTo(second));
            var external=store.Read(id);external.Record.Name="External edit";store.Save(external.Record,external.Token);
            string externalText=File.ReadAllText(store.FilePath(id));workspace.Draft.SetStartingSetup("Local edit",5,40);
            Assert.That(workspace.Save(),Is.False);Assert.That(workspace.IsDirty,Is.True);Assert.That(File.ReadAllText(store.FilePath(id)),Is.EqualTo(externalText));
            Assert.That(workspace.Save(true,"Preserved local copy"),Is.True);Assert.That(workspace.List().Count,Is.EqualTo(2));
        }
        [Test] public void UnsupportedIncompleteAndUnknownFilesAreLeftIntact()
        {
            var workspace=creator.Workspace;Assert.That(workspace.Save(),Is.True);string id=workspace.CurrentId,path=store.FilePath(id),valid=File.ReadAllText(path);
            foreach(var invalid in new[]{valid.Replace("\"version\": 2","\"version\": 99"),valid.Replace("\"start\": 8,",""),valid.Replace("{","{\"futureData\":42,"),"{ incomplete",valid.Replace("\"start\": 8","\"start\": -2")})
            {
                File.WriteAllText(path,invalid);var before=workspace.Draft;
                Assert.That(workspace.Open(id,true),Is.False);Assert.That(workspace.Draft,Is.SameAs(before));
                Assert.That(workspace.List().Single().CanOpen,Is.False);Assert.That(workspace.Save(),Is.False);
                Assert.That(File.ReadAllText(path),Is.EqualTo(invalid));
            }
        }
        [Test] public void MissingBlueprintIdsRefuseLoadWithoutChangingDraft()
        {
            var workspace=creator.Workspace;Assert.That(workspace.Save(),Is.True);string id=workspace.CurrentId,path=store.FilePath(id);
            var entry=store.Read(id);entry.Record.WorkerId="unknown-worker";store.Save(entry.Record,entry.Token);string text=File.ReadAllText(path);
            var before=workspace.Draft;Assert.That(workspace.Open(id,true),Is.False);Assert.That(workspace.Draft,Is.SameAs(before));
            Assert.That(workspace.List().Single().CanOpen,Is.False);Assert.That(File.ReadAllText(path),Is.EqualTo(text));
        }
        [Test] public void FailedReplacementPreservesPreviousSaveAndDirtyDraft()
        {
            var workspace=creator.Workspace;Assert.That(workspace.Save(),Is.True);string path=store.FilePath(workspace.CurrentId),before=File.ReadAllText(path);
            Directory.CreateDirectory(path+".bak");workspace.Draft.SetStartingSetup("Still unsaved",2,40);
            Assert.That(workspace.Save(),Is.False);Assert.That(workspace.IsDirty,Is.True);Assert.That(File.ReadAllText(path),Is.EqualTo(before));
            Assert.That(Directory.GetFiles(root,"*.tmp"),Is.Empty);
        }
        [UnityTest] public IEnumerator SavedFactionLoadsInFreshCreatorAndPlaytests()
        {
            creator.Workspace.Draft.SetStartingSetup("Persistent Garden",3,40);Assert.That(creator.Workspace.Save(),Is.True);string id=creator.Workspace.CurrentId;
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");yield return null;
            creator=UnityEngine.Object.FindAnyObjectByType<FactionCreator>();creator.ConfigureStorage(new FactionStore(root));
            Assert.That(creator.Workspace.Open(id),Is.True);Assert.That(creator.Playtest(),Is.True);
            float timeout=Time.realtimeSinceStartup+20;while(creator.Busy && Time.realtimeSinceStartup<timeout) yield return null;
            Assert.That(creator.Playing,Is.True);Assert.That(creator.Bridge.Session.Depot.Stored,Is.EqualTo(40));Assert.That(UnityEngine.Object.FindObjectsByType<SelectableUnit>().Length,Is.EqualTo(3));
            Assert.That(creator.ReturnToCreator(),Is.True);timeout=Time.realtimeSinceStartup+20;while(creator.Busy && Time.realtimeSinceStartup<timeout) yield return null;
            Assert.That(creator.Workspace.IsDirty,Is.False);Assert.That(creator.Workspace.CurrentId,Is.EqualTo(id));
        }
        [Test] public void ActiveRenameAndConcurrentWriteConflictsPreserveExternalEdits()
        {
            var workspace=creator.Workspace;Assert.That(workspace.Save(),Is.True);string id=workspace.CurrentId;
            var entry=store.Read(id);entry.Record.Units[0].Start=2;store.Save(entry.Record,entry.Token);
            string external=File.ReadAllText(store.FilePath(id));Assert.That(workspace.Rename(id,"Rename attempt"),Is.False);
            Assert.That(File.ReadAllText(store.FilePath(id)),Is.EqualTo(external));Assert.That(workspace.Open(id),Is.True);
            workspace.Draft.SetStartingSetup("Local change",4,40);
            using(var gate=new FileStream(store.FilePath(id)+".lock",FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None,1,FileOptions.DeleteOnClose))
                Assert.That(workspace.Save(),Is.False);
            Assert.That(workspace.IsDirty,Is.True);Assert.That(File.ReadAllText(store.FilePath(id)),Is.EqualTo(external));
            Assert.That(workspace.Save(),Is.True);Assert.That(Directory.GetFiles(root,"*.lock"),Is.Empty);
        }
        [Test] public void InvalidNamesAndStorageFailuresDoNotMarkDraftSaved()
        {
            creator.Workspace.Draft.SetStartingSetup(" ",3,20);Assert.That(creator.Workspace.Save(),Is.False);Assert.That(creator.Workspace.HasFile,Is.False);
            creator.Workspace.Draft.SetStartingSetup("A valid name",3,20);Directory.CreateDirectory(root);string blocked=Path.Combine(root,"occupied");File.WriteAllText(blocked,"occupied");
            using(var workspace=new FactionWorkspace(creator.Draft.Definition,new FactionStore(blocked)))
            {
                workspace.Draft.SetStartingSetup("Changed",4,30);Assert.That(workspace.Save(),Is.False);Assert.That(workspace.IsDirty,Is.True);Assert.That(workspace.HasFile,Is.False);
            }
        }
    }
}
