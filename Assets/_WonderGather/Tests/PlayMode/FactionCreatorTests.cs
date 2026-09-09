using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace WonderGather.Tests
{
    public sealed class FactionCreatorTests
    {
        private FactionCreator creator;
        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");yield return null;
            creator=Object.FindAnyObjectByType<FactionCreator>();Assert.That(creator.Draft,Is.Not.Null);
        }
        private IEnumerator WaitForTransition()
        {float deadline=Time.realtimeSinceStartup+20;while(creator.Busy && Time.realtimeSinceStartup<deadline) yield return null;Assert.That(creator.Busy,Is.False);}
        [UnityTest] public IEnumerator DraftSurvivesRepeatedPlaytestsWhileMapStartsFresh()
        {
            var draft=creator.Draft;draft.SetStartingSetup("Quiet Garden",3,40);
            Assert.That(creator.Playtest(),Is.True);Assert.That(creator.Playtest(),Is.False);yield return WaitForTransition();
            Assert.That(creator.Playing,Is.True);Assert.That(creator.Bridge.Session.Definition,Is.SameAs(draft.Definition));
            Assert.That(creator.Bridge.Session.Depot.Stored,Is.EqualTo(40));Assert.That(Object.FindObjectsByType<SelectableUnit>().Length,Is.EqualTo(3));
            creator.Bridge.Session.Depot.TrySpend(10);
            Assert.That(creator.ReturnToCreator(),Is.True);Assert.That(creator.ReturnToCreator(),Is.False);yield return WaitForTransition();
            Assert.That(creator.Playing,Is.False);Assert.That(creator.Draft,Is.SameAs(draft));
            Assert.That(Object.FindObjectsByType<SelectableUnit>(),Is.Empty);Assert.That(SceneManager.sceneCount,Is.EqualTo(1));
            draft.SetStartingSetup("Quiet Garden",2,60);Assert.That(creator.Playtest(),Is.True);yield return WaitForTransition();
            Assert.That(creator.Bridge.Session.Depot.Stored,Is.EqualTo(60));Assert.That(Object.FindObjectsByType<SelectableUnit>().Length,Is.EqualTo(2));
            Assert.That(creator.ReturnToCreator(),Is.True);yield return WaitForTransition();
        }
        [UnityTest] public IEnumerator LinkChangesAffectPlayAndDoNotModifyExampleAssets()
        {
            var draft=creator.Draft;draft.SetStartingSetup("Builders",1,40);draft.SetWorkerPermissions(false,true);draft.SetProduction(false);
            Assert.That(draft.Report.Warnings,Is.Not.Empty);Assert.That(draft.Report.CanInstantiate,Is.True);
            Assert.That(creator.Playtest(),Is.True);yield return WaitForTransition();
            var unit=Object.FindAnyObjectByType<SelectableUnit>();var selection=Object.FindAnyObjectByType<SelectionController>();
            Assert.That(unit.GetComponent<Gatherer>().Gather(Object.FindAnyObjectByType<ResourceNode>()),Is.False);
            selection.Select(unit);var construction=Object.FindAnyObjectByType<ConstructionController>();
            Assert.That(construction.BeginPlacement(),Is.True);Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);
            float deadline=Time.time+25;while(!construction.LastSite.Complete && Time.time<deadline) yield return null;
            Assert.That(construction.LastSite.Complete,Is.True);Assert.That(construction.LastSite.GetComponent<UnitProducer>().CanTrain,Is.False);
            Assert.That(creator.ReturnToCreator(),Is.True);yield return WaitForTransition();
            yield return SceneManager.LoadSceneAsync("TheCivilization");yield return null;
            var session=Object.FindAnyObjectByType<CivilizationSession>();
            Assert.That(session.Definition.Units[0].GathersSupplies,Is.True);
            Assert.That(session.Definition.Buildings[1].Produces,Is.Not.Null);Assert.That(session.Depot.Stored,Is.Zero);
        }
        [UnityTest] public IEnumerator EmptyChallengeCanPlayButBlankNameCannot()
        {
            creator.Draft.SetStartingSetup("   ",8,0);Assert.That(creator.Playtest(),Is.False);
            creator.Draft.SetStartingSetup("Silent Beginning",0,0);
            Assert.That(creator.Draft.Report.Warnings,Is.Not.Empty);Assert.That(creator.Playtest(),Is.True);yield return WaitForTransition();
            Assert.That(creator.Playing,Is.True);Assert.That(Object.FindObjectsByType<SelectableUnit>(),Is.Empty);
            Assert.That(creator.ReturnToCreator(),Is.True);yield return WaitForTransition();
        }
        [Test] public void DraftClampsMapLimitsAndRemovingBuildLinkWarns()
        {
            creator.Draft.SetStartingSetup(new string('x',80),100,999);
            Assert.That(creator.Draft.Definition.DisplayName.Length,Is.EqualTo(64));Assert.That(creator.Draft.StartingWorkers,Is.EqualTo(8));
            Assert.That(creator.Draft.Definition.StartingSupplies,Is.EqualTo(120));
            creator.Draft.SetWorkerPermissions(true,false);
            Assert.That(creator.Draft.Worker.Builds,Is.Empty);Assert.That(creator.Draft.Report.Warnings,Is.Not.Empty);
        }
    }
}
