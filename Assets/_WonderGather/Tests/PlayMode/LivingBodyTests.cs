using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace WonderGather.Tests
{
    public sealed class LivingBodyTests
    {
        private LivingBodyDemo demo;
        private SelectionController selection;
        private ProceduralBiped[] bodies;

        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheLivingBody");
            yield return null;
            demo=Object.FindAnyObjectByType<LivingBodyDemo>();
            selection=Object.FindAnyObjectByType<SelectionController>();
            bodies=Object.FindObjectsByType<ProceduralBiped>(FindObjectsSortMode.None)
                .OrderBy(x=>x.transform.position.x).ToArray();
            Assert.That(demo,Is.Not.Null);
            Assert.That(selection,Is.Not.Null);
            Assert.That(bodies.Length,Is.EqualTo(2));
            float end=Time.time+2;
            while(bodies.Any(x=>!x.Ready)&&Time.time<end) yield return null;
            Assert.That(bodies.All(x=>x.Ready),Is.True,"Both authored rigs must initialize on the ground.");
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
        }

        private static void Finite(Vector3 value)
        {
            Assert.That(float.IsFinite(value.x)&&float.IsFinite(value.y)&&float.IsFinite(value.z),Is.True);
        }

        private static bool At(ProceduralBiped body,Vector3 target)
        {
            return Vector3.Distance(body.transform.position,target)<.35f;
        }

        private static void Grounded(ProceduralBiped body)
        {
            Assert.That(body.Ready,Is.True);
            foreach(var part in body.GetComponentsInChildren<Transform>())
                if(part.name.EndsWith("thigh")||part.name.EndsWith("shin")) Assert.That(part.localScale.y*2,Is.EqualTo(.68f).Within(.008f),"Leg segments must retain their authored length.");
            Assert.That(body.FootPlanted(0)||body.FootPlanted(1),Is.True,"A walking biped must retain a support foot.");
            for(int foot=0;foot<2;foot++)
            {
                var point=body.FootPosition(foot);var normal=body.FootNormal(foot);
                Finite(point);Finite(normal);
                Assert.That(normal.magnitude,Is.EqualTo(1).Within(.01f));
                if(!body.FootPlanted(foot)) continue;
                Assert.That(Physics.Raycast(point+Vector3.up,Vector3.down,out var hit,2,1<<6,QueryTriggerInteraction.Ignore),Is.True,"Planted feet need terrain beneath them.");
                Assert.That(Vector3.Distance(point,hit.point),Is.LessThan(.025f));
                Assert.That(Vector3.Dot(normal,hit.normal),Is.GreaterThan(.995f));
            }
        }

        [UnityTest] public IEnumerator FlatOrdersAlternateSupportAndStopWithoutIdleFootDrift()
        {
            var targets=new[]{new Vector3(-2,0,-1.5f),new Vector3(2,0,-1.5f)};
            selection.Select(bodies[0].GetComponent<SelectableUnit>());
            Assert.That(selection.MoveSelection(targets[0]),Is.True);
            Assert.That(bodies[1].GetComponent<UnitMotor>().TryMove(targets[1]),Is.True);
            var previous=new Vector3[2,2];var planted=new bool[2,2];
            var lastSwing=new[]{-1,-1};var starts=new int[2];int stableSamples=0;
            for(int i=0;i<2;i++) for(int foot=0;foot<2;foot++)
            {previous[i,foot]=bodies[i].FootPosition(foot);planted[i,foot]=bodies[i].FootPlanted(foot);}
            float end=Time.time+12;
            while((!At(bodies[0],targets[0])||!At(bodies[1],targets[1]))&&Time.time<end)
            {
                // On the next coroutine update, these values describe the preceding LateUpdate.
                yield return null;
                for(int i=0;i<2;i++)
                {
                    var body=bodies[i];Grounded(body);
                    for(int foot=0;foot<2;foot++)
                    {
                        bool now=body.FootPlanted(foot);var point=body.FootPosition(foot);
                        if(planted[i,foot]&&now)
                        {Assert.That(Vector3.Distance(previous[i,foot],point),Is.LessThan(.001f),"A support point must remain fixed in world space.");stableSamples++;}
                        if(planted[i,foot]&&!now)
                        {
                            if(lastSwing[i]>=0) Assert.That(foot,Is.Not.EqualTo(lastSwing[i]),"Straight walking should alternate the swinging leg.");
                            lastSwing[i]=foot;starts[i]++;
                        }
                        previous[i,foot]=point;planted[i,foot]=now;
                    }
                }
            }
            for(int i=0;i<2;i++)
            {Assert.That(At(bodies[i],targets[i]),Is.True,"A navigation order must reach the flat destination.");Assert.That(starts[i],Is.GreaterThanOrEqualTo(4));Assert.That(bodies[i].StepCount,Is.GreaterThanOrEqualTo(4));}
            Assert.That(stableSamples,Is.GreaterThan(20));
            demo.StopWalkers();
            // Permit an in-flight step and the final standing adjustments to finish.
            yield return new WaitForSeconds(2);
            var roots=bodies.Select(x=>x.transform.position).ToArray();
            for(int i=0;i<2;i++) for(int foot=0;foot<2;foot++) previous[i,foot]=bodies[i].FootPosition(foot);
            end=Time.time+1;
            while(Time.time<end)
            {
                yield return null;
                for(int i=0;i<2;i++)
                {
                    Grounded(bodies[i]);Assert.That(Vector3.Distance(roots[i],bodies[i].transform.position),Is.LessThan(.01f));
                    for(int foot=0;foot<2;foot++)
                    {Assert.That(bodies[i].FootPlanted(foot),Is.True);Assert.That(Vector3.Distance(previous[i,foot],bodies[i].FootPosition(foot)),Is.LessThan(.002f));}
                }
            }
        }

        [UnityTest] public IEnumerator BothPacesClimbAndDescendWithFeetOnTheRamp()
        {
            var starts=bodies.Select(x=>x.transform.position).ToArray();
            var top=new[]{new Vector3(-2,2,14),new Vector3(2,2,14)};
            for(int route=0;route<2;route++)
            {
                Assert.That(route==0?demo.WalkRoute(true):demo.ReturnToStart(),Is.True);
                var targets=route==0?top:starts;var slopeContacts=new int[2];
                float end=Time.time+25;
                while((!At(bodies[0],targets[0])||!At(bodies[1],targets[1]))&&Time.time<end)
                {
                    yield return null;
                    for(int i=0;i<2;i++)
                    {
                        Grounded(bodies[i]);
                        for(int foot=0;foot<2;foot++)
                        {
                            var point=bodies[i].FootPosition(foot);
                            if(bodies[i].FootPlanted(foot)&&point.z>2&&point.z<8)
                            {Assert.That(bodies[i].FootNormal(foot).y,Is.InRange(.96f,.99f));slopeContacts[i]++;}
                        }
                    }
                }
                for(int i=0;i<2;i++)
                {Assert.That(At(bodies[i],targets[i]),Is.True,route==0?"Walker must reach the raised plateau.":"Walker must return to the flat start.");Assert.That(slopeContacts[i],Is.GreaterThan(5),"Check actual support contacts on the slope in each direction.");}
            }
        }

        [UnityTest] public IEnumerator RedirectAndReenableRestoreNearbySupportWithoutMovingTheRoot()
        {
            Assert.That(demo.WalkRoute(true),Is.True);
            yield return new WaitForSeconds(1.5f);
            var body=bodies[0];var motor=body.GetComponent<UnitMotor>();
            selection.Select(body.GetComponent<SelectableUnit>());
            var target=new Vector3(-6,0,-7);
            Assert.That(selection.MoveSelection(target),Is.True);
            yield return new WaitForSeconds(.5f);
            body.enabled=false;Assert.That(body.Ready,Is.False);
            yield return new WaitForSeconds(.8f);
            body.enabled=true;
            yield return null;yield return null;
            Assert.That(body.Ready,Is.True);
            foreach(var part in body.GetComponentsInChildren<Transform>())
                if(part.name.EndsWith("thigh")||part.name.EndsWith("shin")) Assert.That(part.localScale.y*2,Is.EqualTo(.68f).Within(.008f),"Leg segments must retain their authored length.");
            for(int foot=0;foot<2;foot++)
            {
                var offset=body.FootPosition(foot)-body.transform.position;offset.y=0;
                Assert.That(offset.magnitude,Is.LessThan(.8f),"Re-enabled support must be near the current root, not its old world position.");
            }
            float end=Time.time+10;
            while(!At(body,target)&&Time.time<end){yield return null;Grounded(body);}
            Assert.That(At(body,target),Is.True,"Redirected navigation must complete after presentation is re-enabled.");
            motor.Stop();var root=body.transform.position;
            body.ResetPose();Grounded(body);
            Assert.That(Vector3.Distance(root,body.transform.position),Is.LessThan(.0001f),"Resetting presentation must never move the navigation root.");
            for(int foot=0;foot<2;foot++) Assert.That(body.FootPlanted(foot),Is.True);
        }
    }
}
