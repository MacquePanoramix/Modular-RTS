using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace WonderGather.Tests
{
    public sealed class EquippedWorkerTests
    {
        private FactionCreator creator;
        private string storageRoot;
        private float captureDelta;

        [UnitySetUp] public IEnumerator Load()
        {
            captureDelta=Time.captureDeltaTime;
            yield return SceneManager.LoadSceneAsync("TheFactionCreator");
            yield return null;
            creator=Object.FindAnyObjectByType<FactionCreator>();
            Assert.That(creator,Is.Not.Null);
            storageRoot=Path.Combine(Path.GetTempPath(),"WonderGather-EquippedWorkerTests-"+Guid.NewGuid().ToString("N"));
            creator.ConfigureStorage(new FactionStore(storageRoot));
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.captureDeltaTime=captureDelta;
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
            if(storageRoot==null) yield break;
            string path=Path.GetFullPath(storageRoot);
            string prefix=Path.Combine(Path.GetFullPath(Path.GetTempPath()),"WonderGather-EquippedWorkerTests-");
            Assert.That(path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase),Is.True);
            if(Directory.Exists(path)) Directory.Delete(path,true);
        }

        private static IEnumerator Until(Func<bool> condition,float seconds,string message)
        {
            float end=Time.time+seconds,wallEnd=Time.realtimeSinceStartup+Mathf.Max(15,seconds*2);
            while(!condition()&&Time.time<end&&Time.realtimeSinceStartup<wallEnd) yield return null;
            Assert.That(condition(),Is.True,message);
        }

        private IEnumerator Transition()
        {
            yield return Until(()=>!creator.Busy,20,"The creator scene transition must complete.");
            yield return null;yield return null;
        }

        private IEnumerator Enter(int count,UnitPerformance performance,bool equipped=true,int supplies=0)
        {
            creator.Draft.SetStartingSetup("Equipment integration test",count,supplies);
            creator.Draft.SetPerformance(creator.Draft.Worker,performance);
            Assert.That(creator.Draft.Tools.Count,Is.GreaterThan(0));
            creator.Draft.SetTool(creator.Draft.Worker,equipped?creator.Draft.Tools.Single(x=>x.Id=="pickaxe"):null);
            Assert.That(creator.PlaytestEquipment(),Is.True);
            yield return Transition();
            Assert.That(creator.Playing,Is.True);
            Assert.That(Object.FindObjectsByType<Gatherer>().Length,Is.EqualTo(count));
            Assert.That(Object.FindObjectsByType<MineableResource>().Length,Is.EqualTo(1));
        }

        private static void Conserved(ResourceNode node,ResourceDepot depot,Gatherer[] workers,int total)
        {
            Assert.That(node.Remaining+depot.Stored+workers.Sum(x=>x.Carried),Is.EqualTo(total),
                "Mining must conserve stock, cargo and delivered supplies.");
            foreach(var worker in workers) Assert.That(worker.Carried,Is.InRange(0,worker.Capacity));
        }

        private static void SupportedAndGripped(Gatherer worker)
        {
            var body=worker.GetComponent<ProceduralBiped>();
            var tool=worker.GetComponent<EquippedTool>();
            Assert.That(body.Ready,Is.True);
            Assert.That(body.FootPlanted(0)||body.FootPlanted(1),Is.True,"The worker must retain a supporting foot.");
            for(int i=0;i<2;i++)
            {
                Vector3 point=body.FootPosition(i);
                Assert.That(float.IsFinite(point.x)&&float.IsFinite(point.y)&&float.IsFinite(point.z),Is.True);
                if(body.FootPlanted(i))
                {
                    Assert.That(Physics.Raycast(point+Vector3.up,Vector3.down,out var hit,2,1<<6,QueryTriggerInteraction.Ignore),Is.True);
                    Assert.That(Vector3.Distance(point,hit.point),Is.LessThan(.025f));
                }
                if(tool.HandsOnTool)
                    Assert.That(Vector3.Distance(body.HandPosition(i),tool.GripPosition(i)),Is.LessThan(.015f),
                        "Both hands must reach the actual rigid tool grips without visual detachment.");
            }
            foreach(var part in body.GetComponentsInChildren<Transform>())
                if(part.name.EndsWith("thigh",StringComparison.Ordinal)||part.name.EndsWith("shin",StringComparison.Ordinal))
                    Assert.That(part.localScale.y*2,Is.EqualTo(.68f).Within(.008f));
        }

        private static IEnumerator Preparation(Gatherer worker)
        {
            var tool=worker.GetComponent<EquippedTool>();
            yield return Until(()=>tool.Phase==EquippedTool.StrikePhase.Preparation&&tool.Progress<.35f,35,
                "The equipped worker should arrive, settle and prepare an actual strike: "+tool.Status);
        }

        [UnityTest] public IEnumerator StartingBlueprintWorkerStrikesOncePerAttemptAndCompletesTheRealCargoLoop()
        {
            yield return Enter(1,UnitPerformance.Default);
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var tool=worker.GetComponent<EquippedTool>();
            var body=worker.GetComponent<ProceduralBiped>();
            var mine=Object.FindAnyObjectByType<MineableResource>();
            var node=mine.GetComponent<ResourceNode>();
            var depot=creator.Bridge.Session.Depot;
            int total=node.Remaining+depot.Stored;
            Assert.That(tool.Definition,Is.SameAs(creator.Draft.Worker.Tool));
            Assert.That(worker.Gather(node),Is.True);
            bool preparation=false,striking=false,recovery=false,carrying=false,stowed=false,delivering=false;
            ulong lastAcceptedAttempt=0;
            int previousAccepted=0,previousStock=node.Remaining,grippedSamples=0;
            float deadline=Time.time+40;
            while(depot.Stored==0&&Time.time<deadline)
            {
                yield return null;
                Conserved(node,depot,new[]{worker},total);
                SupportedAndGripped(worker);
                preparation|=tool.Phase==EquippedTool.StrikePhase.Preparation;
                striking|=tool.Phase==EquippedTool.StrikePhase.Striking;
                recovery|=tool.Phase==EquippedTool.StrikePhase.Recovery;
                if(tool.HandsOnTool) grippedSamples++;
                carrying|=worker.State==Gatherer.Activity.ToDepot&&worker.Carried>0&&body.CargoVisible;
                stowed|=worker.State==Gatherer.Activity.ToDepot&&tool.Stowed;
                delivering|=worker.State==Gatherer.Activity.Depositing;
                if(tool.AcceptedStrikes!=previousAccepted)
                {
                    Assert.That(tool.AcceptedStrikes,Is.EqualTo(previousAccepted+1));
                    Assert.That(node.Remaining,Is.EqualTo(previousStock-1),"One accepted hit extracts one available supply.");
                    Assert.That(tool.AttemptId,Is.GreaterThan(lastAcceptedAttempt),"A held contact must not repeat extraction in the same attempt.");
                    lastAcceptedAttempt=tool.AttemptId;
                }
                else Assert.That(node.Remaining,Is.EqualTo(previousStock),"A timer or work reservation alone cannot extract mineral.");
                previousAccepted=tool.AcceptedStrikes;previousStock=node.Remaining;
            }
            Assert.That(depot.Stored,Is.EqualTo(worker.Capacity),"A full real cargo load should reach the depot.");
            Assert.That(tool.AcceptedStrikes,Is.EqualTo(worker.Capacity));
            Assert.That(preparation&&striking&&recovery&&carrying&&stowed&&delivering,Is.True,
                "Observe preparation, strike, recovery, cargo transport, visible tool stow and delivery.");
            Assert.That(grippedSamples,Is.GreaterThan(5));
            worker.CancelOrder();worker.GetComponent<UnitMotor>().Stop();
            Conserved(node,depot,new[]{worker},total);
        }

        [UnityTest] public IEnumerator ProducedWorkerReceivesTheBlueprintPickaxeAndMinesThroughTheSameAction()
        {
            yield return Enter(1,new UnitPerformance(200,2,200,200),true,120);
            var selection=Object.FindAnyObjectByType<SelectionController>();
            var construction=Object.FindAnyObjectByType<ConstructionController>();
            selection.Select(Object.FindAnyObjectByType<SelectableUnit>());
            Assert.That(construction.ChooseBuilding(creator.Draft.Workshop),Is.True);
            Assert.That(construction.TryPlace(new Vector3(-18,0,8)),Is.True);
            var site=construction.LastSite;
            yield return Until(()=>site.Complete,25,"The worker must still construct while equipped.");
            var producer=site.GetComponent<UnitProducer>();
            Assert.That(producer.Enqueue(creator.Draft.Worker),Is.True);
            yield return Until(()=>producer.LastProduced!=null,15,"Training should create the chosen blueprint.");
            yield return null;yield return null;
            var worker=producer.LastProduced.GetComponent<Gatherer>();
            var tool=worker.GetComponent<EquippedTool>();
            Assert.That(tool.Definition,Is.SameAs(creator.Draft.Worker.Tool));
            Assert.That(worker.GetComponent<UnitIdentity>().Blueprint,Is.SameAs(creator.Draft.Worker));
            var node=Object.FindAnyObjectByType<MineableResource>().GetComponent<ResourceNode>();
            var depot=creator.Bridge.Session.Depot;
            var workers=Object.FindObjectsByType<Gatherer>();
            int before=depot.Stored,total=node.Remaining+before+workers.Sum(x=>x.Carried);
            selection.Select(producer.LastProduced);
            Assert.That(selection.GatherSelection(node),Is.EqualTo(1));
            float deadline=Time.time+40;
            while(depot.Stored==before&&Time.time<deadline)
            {
                yield return null;
                Conserved(node,depot,workers,total);
                SupportedAndGripped(worker);
            }
            Assert.That(depot.Stored,Is.EqualTo(before+worker.Capacity));
            Assert.That(tool.AcceptedStrikes,Is.EqualTo(worker.Capacity));
        }

        [UnityTest] public IEnumerator GatherPermissionWithoutPickaxeCannotMineOrReserveAStation()
        {
            yield return Enter(1,UnitPerformance.Default,false);
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var node=Object.FindAnyObjectByType<MineableResource>().GetComponent<ResourceNode>();
            int stock=node.Remaining;
            Assert.That(worker.Gather(node),Is.False);
            Assert.That(worker.LastOrderFailure,Does.Contain("pickaxe").IgnoreCase);
            Assert.That(node.GetComponent<ResourceWorkplace>().OccupiedCount,Is.Zero);
            float end=Time.time+1.25f;
            while(Time.time<end) yield return null;
            Assert.That(worker.Carried,Is.Zero);
            Assert.That(node.Remaining,Is.EqualTo(stock));
            Assert.That(worker.GetComponent<EquippedTool>().Definition,Is.Null);
        }

        [UnityTest] public IEnumerator RepeatedSwingsAtAnAbsentSurfaceDoNotReceiveTimerYield()
        {
            yield return Enter(1,new UnitPerformance(200,20,200,100));
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var tool=worker.GetComponent<EquippedTool>();
            var mine=Object.FindAnyObjectByType<MineableResource>();
            var node=mine.GetComponent<ResourceNode>();
            // Keep the authored navigation position and contact marker in place,
            // but move the actual rendered/collision surface beyond tool reach.
            mine.Surface.transform.position+=Vector3.up*8;
            Physics.SyncTransforms();
            int stock=node.Remaining;
            Assert.That(worker.Gather(node),Is.True);
            yield return Preparation(worker);
            ulong first=tool.AttemptId;
            yield return Until(()=>tool.AttemptId>=first+2,8,"A miss should recover and allow another deliberate attempt.");
            Assert.That(tool.AcceptedStrikes,Is.Zero);
            Assert.That(node.Remaining,Is.EqualTo(stock));
            Assert.That(worker.Carried,Is.Zero);
        }

        [UnityTest] public IEnumerator ACloserForeignColliderBlocksTheIntendedMineralContact()
        {
            yield return Enter(1,new UnitPerformance(200,20,200,100));
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var tool=worker.GetComponent<EquippedTool>();
            var mine=Object.FindAnyObjectByType<MineableResource>();
            var node=mine.GetComponent<ResourceNode>();
            Assert.That(worker.Gather(node),Is.True);
            yield return Preparation(worker);
            Vector3 outward=worker.transform.position-node.transform.position;outward.y=0;outward.Normalize();
            var obstruction=new GameObject("Test foreign obstruction");
            SceneManager.MoveGameObjectToScene(obstruction,node.gameObject.scene);
            obstruction.transform.SetPositionAndRotation(node.transform.position+outward*1.25f+Vector3.up,
                Quaternion.LookRotation(outward));
            obstruction.AddComponent<BoxCollider>().size=new Vector3(.8f,2,.18f);
            Physics.SyncTransforms();
            int stock=node.Remaining;
            ulong first=tool.AttemptId;
            bool blocked=false;
            float deadline=Time.time+8;
            while(tool.AttemptId<first+2&&Time.time<deadline)
            {
                yield return null;
                blocked|=tool.Status.IndexOf("blocked",StringComparison.OrdinalIgnoreCase)>=0;
                Assert.That(node.Remaining,Is.EqualTo(stock));
                Assert.That(worker.Carried,Is.Zero);
            }
            Assert.That(tool.AttemptId,Is.GreaterThanOrEqualTo(first+2));
            Assert.That(blocked,Is.True,"A physical obstruction should explain why it prevented extraction.");
            Assert.That(tool.AcceptedStrikes,Is.Zero);
        }

        [UnityTest] public IEnumerator UnreachableRigidGripCannotBeClampedIntoAFalseMiningSuccess()
        {
            yield return Enter(1,new UnitPerformance(200,20,200,100));
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var tool=worker.GetComponent<EquippedTool>();
            var original=tool.Definition;
            var model=worker.transform.Find(original.DisplayName+" (equipped)");
            var upperArm=worker.GetComponentsInChildren<Transform>().Single(x=>x.name=="Left upper arm");
            // A rigid grip at the actual shoulder is also impossible: the fixed
            // unequal arm segments have a minimum reach, not just a maximum.
            Vector3 shoulderGrip=model.InverseTransformPoint(upperArm.TransformPoint(Vector3.down));
            foreach(var grip in new[]{new Vector3(0,4,0),shoulderGrip})
            {
                var unreachable=ScriptableObject.CreateInstance<ToolDefinition>();
                unreachable.Configure(original.Id,"Unreachable test grip",original.Prefab,original.PrimaryGrip,
                    grip,original.Head,original.HeadRadius);
                try
                {
                    tool.SetDefinition(unreachable);
                    var node=Object.FindAnyObjectByType<MineableResource>().GetComponent<ResourceNode>();
                    int stock=node.Remaining;
                    Assert.That(worker.Gather(node),Is.True);
                    yield return Until(()=>worker.LastOrderFailure.IndexOf("reach",StringComparison.OrdinalIgnoreCase)>=0,30,
                        "The body must report an unreachable grip instead of manufacturing contact.");
                    Assert.That(worker.State,Is.EqualTo(Gatherer.Activity.Idle));
                    Assert.That(node.GetComponent<ResourceWorkplace>().OccupiedCount,Is.Zero);
                    Assert.That(tool.AcceptedStrikes,Is.Zero);
                    Assert.That(node.Remaining,Is.EqualTo(stock));
                    Assert.That(worker.Carried,Is.Zero);
                }
                finally { tool.SetDefinition(original);Object.Destroy(unreachable); }
                }
        }

        [UnityTest] public IEnumerator InterruptionsReleaseReservationsAndNeverReplayAStaleStrike()
        {
            yield return Enter(1,new UnitPerformance(200,20,200,100));
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var tool=worker.GetComponent<EquippedTool>();
            var body=worker.GetComponent<ProceduralBiped>();
            var mine=Object.FindAnyObjectByType<MineableResource>();
            var node=mine.GetComponent<ResourceNode>();
            var workplace=node.GetComponent<ResourceWorkplace>();
            var depot=creator.Bridge.Session.Depot;
            int total=node.Remaining;
            Assert.That(worker.Gather(node),Is.True);
            yield return Until(()=>worker.Carried>0,35,"Obtain real partial cargo before exercising interruption.");
            worker.CancelOrder();worker.GetComponent<UnitMotor>().Stop();
            for(int mode=0;mode<6;mode++)
            {
                Assert.That(worker.Gather(node),Is.True,"A fresh order after interruption must be supported.");
                yield return Preparation(worker);
                int stock=node.Remaining,cargo=worker.Carried,accepted=tool.AcceptedStrikes;
                ulong attempt=tool.AttemptId;
                switch(mode)
                {
                    case 0: worker.CancelOrder();worker.GetComponent<UnitMotor>().Stop();break;
                    case 1: worker.enabled=false;break;
                    case 2: body.enabled=false;break;
                    case 3: tool.enabled=false;break;
                    case 4: mine.enabled=false;break;
                    case 5: mine.Surface.enabled=false;break;
                }
                yield return null;yield return null;
                Assert.That(workplace.OccupiedCount,Is.Zero,"Interruption mode "+mode+" must release the station.");
                Assert.That(tool.Phase,Is.EqualTo(EquippedTool.StrikePhase.Rest));
                worker.enabled=true;body.enabled=true;tool.enabled=true;mine.enabled=true;mine.Surface.enabled=true;
                float end=Time.time+1.1f;
                while(Time.time<end)
                {
                    yield return null;
                    Assert.That(node.Remaining,Is.EqualTo(stock),"Interrupted attempts cannot extract after re-enable.");
                    Assert.That(worker.Carried,Is.EqualTo(cargo),"Cancellation retains partial cargo.");
                    Assert.That(tool.AcceptedStrikes,Is.EqualTo(accepted));
                    Assert.That(tool.AttemptId,Is.EqualTo(attempt),"Re-enable alone is not a replacement order.");
                    Conserved(node,depot,new[]{worker},total);
                }
            }
            int retained=worker.Carried;
            Assert.That(worker.ReturnToDepot(depot),Is.True);
            yield return Until(()=>depot.Stored==retained,30,"Interrupted partial cargo must still be deliverable.");
            Conserved(node,depot,new[]{worker},total);
        }

        [UnityTest] public IEnumerator TwoWorkersDepleteSharedStockWithoutDuplicateOrLostExtraction()
        {
            yield return Enter(2,new UnitPerformance(200,2,200,100));
            var workers=Object.FindObjectsByType<Gatherer>();
            var node=Object.FindAnyObjectByType<MineableResource>().GetComponent<ResourceNode>();
            var depot=creator.Bridge.Session.Depot;
            node.Take(node.Remaining-7);
            foreach(var worker in workers) Assert.That(worker.Gather(node),Is.True);
            float deadline=Time.time+65;
            while((node.Remaining>0||workers.Any(x=>x.Carried>0))&&Time.time<deadline)
            {
                yield return null;
                Conserved(node,depot,workers,7);
                Assert.That(workers.Sum(x=>x.GetComponent<EquippedTool>().AcceptedStrikes),Is.EqualTo(7-node.Remaining));
            }
            Assert.That(node.Remaining,Is.Zero);
            Assert.That(depot.Stored,Is.EqualTo(7));
            Assert.That(workers.Sum(x=>x.GetComponent<EquippedTool>().AcceptedStrikes),Is.EqualTo(7));
            yield return null;yield return null;
            Assert.That(node.GetComponent<ResourceWorkplace>().OccupiedCount,Is.Zero);
            Assert.That(depot.GetComponent<ResourceWorkplace>().OccupiedCount,Is.Zero);
            foreach(var worker in workers) Assert.That(worker.Carried,Is.Zero);
        }

        [UnityTest] public IEnumerator ObstructionEnteringAnActiveSwingCannotBecomeAnInvisibleHit()
        {
            yield return Enter(1,new UnitPerformance(200,20,100,100));
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var tool=worker.GetComponent<EquippedTool>();
            var node=Object.FindAnyObjectByType<MineableResource>().GetComponent<ResourceNode>();
            Assert.That(worker.Gather(node),Is.True);
            yield return Preparation(worker);
            Time.captureDeltaTime=.01f;
            GameObject obstruction=null;
            try
            {
                yield return Until(()=>tool.Phase==EquippedTool.StrikePhase.Striking&&tool.Progress>.42f,4,
                    "Observe the moving head after the strike phase has already begun.");
                Assert.That(tool.Progress,Is.LessThan(.5f),"Insert the blocker before the intended mineral impact.");
                Assert.That(tool.AcceptedStrikes,Is.Zero);
                ulong interrupted=tool.AttemptId;
                int stock=node.Remaining;
                obstruction=new GameObject("Test obstruction entering swing");
                SceneManager.MoveGameObjectToScene(obstruction,node.gameObject.scene);
                obstruction.transform.position=tool.HeadPosition;
                obstruction.AddComponent<BoxCollider>().size=Vector3.one*.3f;
                Physics.SyncTransforms();
                yield return Until(()=>tool.Phase==EquippedTool.StrikePhase.Recovery,3,
                    "The obstructed strike should arrest and recover.");
                Assert.That(tool.AttemptId,Is.EqualTo(interrupted));
                Assert.That(tool.Status,Does.Contain("blocked").IgnoreCase);
                Assert.That(tool.AcceptedStrikes,Is.Zero);
                Assert.That(node.Remaining,Is.EqualTo(stock));
                Assert.That(worker.Carried,Is.Zero);
                obstruction.SetActive(false);
                yield return Until(()=>tool.AcceptedStrikes>0,5,"A subsequent unobstructed attempt may mine normally.");
                Assert.That(tool.AttemptId,Is.GreaterThan(interrupted),"Removing the obstruction cannot revive the spent attempt.");
                Assert.That(tool.AcceptedStrikes,Is.EqualTo(1));
                Assert.That(node.Remaining,Is.EqualTo(stock-1));
                Assert.That(worker.Carried,Is.EqualTo(1));
            }
            finally
            {
                Time.captureDeltaTime=captureDelta;
                if(obstruction!=null) Object.Destroy(obstruction);
            }
        }

        [UnityTest] public IEnumerator ChangingWorkerOrToolScaleCancelsPreparedWorkWithoutExtraction()
        {
            yield return Enter(1,new UnitPerformance(200,20,200,100));
            var worker=Object.FindAnyObjectByType<Gatherer>();
            var tool=worker.GetComponent<EquippedTool>();
            var node=Object.FindAnyObjectByType<MineableResource>().GetComponent<ResourceNode>();
            var workplace=node.GetComponent<ResourceWorkplace>();
            var model=worker.transform.Find(tool.Definition.DisplayName+" (equipped)");
            Assert.That(model,Is.Not.Null,"The blueprint must have instantiated its visible tool.");
            for(int mode=0;mode<2;mode++)
            {
                Assert.That(worker.Gather(node),Is.True);
                yield return Preparation(worker);
                int stock=node.Remaining,cargo=worker.Carried,accepted=tool.AcceptedStrikes;
                var scaled=mode==0?worker.transform:model;
                Vector3 original=scaled.localScale;
                try
                {
                    scaled.localScale=Vector3.one*2;
                    yield return Until(()=>worker.State==Gatherer.Activity.Idle,3,
                        "Unsupported scale must cancel work rather than query a different path from the rendered tool.");
                    Assert.That(worker.LastOrderFailure,Does.Contain("scale").IgnoreCase);
                    Assert.That(workplace.OccupiedCount,Is.Zero);
                    Assert.That(tool.Phase,Is.EqualTo(EquippedTool.StrikePhase.Rest));
                    Assert.That(worker.Gather(node),Is.False,"A new order must reject the same unsupported scale.");
                    Assert.That(worker.LastOrderFailure,Does.Contain("scale").IgnoreCase);
                }
                finally { scaled.localScale=original; }
                float end=Time.time+1;
                while(Time.time<end)
                {
                    yield return null;
                    Assert.That(node.Remaining,Is.EqualTo(stock));
                    Assert.That(worker.Carried,Is.EqualTo(cargo));
                    Assert.That(tool.AcceptedStrikes,Is.EqualTo(accepted));
                }
            }
            Assert.That(worker.Gather(node),Is.True,"Restoring supported geometry allows a fresh explicit order.");
            yield return Until(()=>tool.AcceptedStrikes>0,10,"Restored scale should use the valid contact path again.");
        }

        [UnityTest] public IEnumerator ScaledToolPrefabCannotDefineMismatchedVisualAndContactGeometry()
        {
            var model=new GameObject("Test tool definition model");
            var definition=ScriptableObject.CreateInstance<ToolDefinition>();
            try
            {
                model.transform.localScale=Vector3.one*2;
                Assert.Throws<ArgumentException>(()=>definition.Configure("test-tool","Test tool",model,
                    new Vector3(0,-.2f,0),new Vector3(0,.12f,0),new Vector3(0,.55f,.26f),.055f));
                Assert.That(definition.IsValid,Is.False);
                model.transform.localScale=Vector3.one;
                definition.Configure("test-tool","Test tool",model,new Vector3(0,-.2f,0),
                    new Vector3(0,.12f,0),new Vector3(0,.55f,.26f),.055f);
                Assert.That(definition.IsValid,Is.True);
                model.transform.localScale=new Vector3(1,2,1);
                Assert.That(definition.IsValid,Is.False,"Later prefab scale edits must also invalidate its contact contract.");
            }
            finally { Object.Destroy(definition);Object.Destroy(model); }
            yield return null;
        }

        [UnityTest] public IEnumerator CoarseFramesAndSupportedRateExtremesStillRequireOneContactPerAttempt()
        {
            foreach(int rate in new[]{25,200})
            {
                yield return Enter(1,new UnitPerformance(200,20,rate,100));
                var worker=Object.FindAnyObjectByType<Gatherer>();
                var tool=worker.GetComponent<EquippedTool>();
                var node=Object.FindAnyObjectByType<MineableResource>().GetComponent<ResourceNode>();
                int stock=node.Remaining;
                Assert.That(worker.Gather(node),Is.True);
                yield return Preparation(worker);
                // Only coarsen the stationary action. Navigation/arrival is not the
                // subject here; the visible head sweep must cross a static surface.
                Time.captureDeltaTime=.2f;
                try
                {
                    yield return Until(()=>tool.AcceptedStrikes>=2,10,"Swept head contact must survive coarse solved frames.");
                    Assert.That(tool.AcceptedStrikes,Is.EqualTo(2));
                    Assert.That(node.Remaining,Is.EqualTo(stock-2));
                    Assert.That(worker.Carried,Is.EqualTo(2));
                    SupportedAndGripped(worker);
                }
                finally { Time.captureDeltaTime=captureDelta; }
                worker.CancelOrder();worker.GetComponent<UnitMotor>().Stop();
                Assert.That(creator.ReturnToCreator(),Is.True);
                yield return Transition();
            }
        }
    }
}
