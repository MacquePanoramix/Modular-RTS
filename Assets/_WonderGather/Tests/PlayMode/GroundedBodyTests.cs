using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // Strength and Burden, checkpoint A: the gait follows the body's dimensions and speed
    // rather than shuffling, feet roll from heel to toe, and only a jog leaves the ground.
    public sealed class GroundedBodyTests
    {
        private LivingBodyDemo demo;
        private ProceduralBiped[] bodies;

        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheLivingBody");
            yield return null;
            demo=Object.FindAnyObjectByType<LivingBodyDemo>();
            bodies=Object.FindObjectsByType<ProceduralBiped>().OrderBy(x=>x.transform.position.x).ToArray();
            Assert.That(demo,Is.Not.Null);
            Assert.That(bodies.Length,Is.EqualTo(2));
            float end=Time.time+2;
            while(bodies.Any(x=>!x.Ready)&&Time.time<end) yield return null;
            Assert.That(bodies.All(x=>x.Ready),Is.True);
            demo.StopWalkers();
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
        }

        private sealed class Gait
        {
            public int Steps;
            public float Seconds,MinPitch,MaxPitch,LongestFlight;
            public bool Walked,Jogged,Airborne;
            public readonly List<Vector3>[] Landings={new List<Vector3>(),new List<Vector3>()};
            public float StepsPerSecond=>Steps/Mathf.Max(Seconds,.001f);
            public override string ToString()=>$"steps/s {StepsPerSecond:F2}, stride {MeanStride:F2} m, pitch {MinPitch:F1}..{MaxPitch:F1}°, longest flight {LongestFlight:F3} s";
            public float MeanStride=>(float)Landings.Where(x=>x.Count>1)
                .SelectMany(x=>x.Zip(x.Skip(1),(a,b)=>(double)Vector3.ProjectOnPlane(b-a,Vector3.up).magnitude)).Average();
        }

        // Records the gait only while the root moves at its steady commanded pace.
        private static IEnumerator Observe(ProceduralBiped body,Vector3 target,Gait result,float timeout)
        {
            var agent=body.GetComponent<NavMeshAgent>();
            Assert.That(body.GetComponent<UnitMotor>().TryMove(target),Is.True);
            var planted=new[]{true,true};
            int firstStep=-1;float firstTime=0;
            float end=Time.time+timeout;
            while(Vector3.Distance(body.transform.position,target)>.4f&&Time.time<end)
            {
                yield return null;
                bool steady=agent.velocity.magnitude>agent.speed*.9f;
                for(int foot=0;foot<2;foot++)
                {
                    bool now=body.FootPlanted(foot);
                    if(steady&&!planted[foot]&&now) result.Landings[foot].Add(body.FootPosition(foot));
                    planted[foot]=now;
                    if(steady){result.MinPitch=Mathf.Min(result.MinPitch,body.FootPitch(foot));result.MaxPitch=Mathf.Max(result.MaxPitch,body.FootPitch(foot));}
                }
                if(steady)
                {
                    if(firstStep<0){firstStep=body.StepCount;firstTime=Time.time;}
                    result.Steps=body.StepCount-firstStep;result.Seconds=Time.time-firstTime;
                    result.Walked|=body.CurrentGait==ProceduralBiped.Gait.Walking;
                    result.Jogged|=body.CurrentGait==ProceduralBiped.Gait.Jogging;
                }
                result.Airborne|=body.Airborne;
                result.LongestFlight=Mathf.Max(result.LongestFlight,body.FlightTime);
                if(body.CurrentGait==ProceduralBiped.Gait.Walking)
                    Assert.That(body.FootPlanted(0)||body.FootPlanted(1),Is.True,"A walk must always keep a supporting foot.");
            }
            Assert.That(Vector3.Distance(body.transform.position,target),Is.LessThan(.4f),"The order must reach its destination.");
        }

        [UnityTest] public IEnumerator NaturalWalkUsesBodyScaledStridesInsteadOfShuffling()
        {
            var body=bodies[0];
            Assert.That(body.GetComponent<NavMeshAgent>().speed,Is.EqualTo(1.8f).Within(.01f));
            var gait=new Gait();
            yield return Observe(body,new Vector3(-11,0,-16),gait,15);
            TestContext.WriteLine("Walk: "+gait);
            Assert.That(gait.Walked&&!gait.Jogged&&!gait.Airborne,Is.True,"A natural pace is a walk with continuous support.");
            Assert.That(gait.Seconds,Is.GreaterThan(2),"Observe several steady steps.");
            // A human-like walking cadence, not the former five steps per second.
            Assert.That(gait.StepsPerSecond,Is.InRange(1.5f,2.4f));
            // Stride follows hip height: a full cycle covers roughly 1.3 hip heights.
            Assert.That(gait.MeanStride,Is.InRange(1.4f,2.3f));
            Assert.That(gait.MinPitch,Is.LessThan(-4),"Feet should strike with the heel.");
            Assert.That(gait.MaxPitch,Is.GreaterThan(12),"Feet should roll and push off from the ball.");
        }

        [UnityTest] public IEnumerator FastPaceJogsWithBriefFlightAndSettlesWhenStopped()
        {
            var body=bodies[1];var agent=body.GetComponent<NavMeshAgent>();
            // 200% of the natural walk, above the Froude walk-run threshold for this body.
            agent.speed=3.6f;agent.acceleration=6;
            var gait=new Gait();
            yield return Observe(body,new Vector3(12,0,4),gait,15);
            TestContext.WriteLine("Jog: "+gait);
            Assert.That(gait.Jogged,Is.True,"A pace above the walk-run threshold should become a jog.");
            Assert.That(gait.Airborne,Is.True,"A jog has a flight phase.");
            Assert.That(gait.LongestFlight,Is.GreaterThan(0).And.LessThan(.2f),"Flight must stay brief.");
            Assert.That(gait.StepsPerSecond,Is.InRange(2.2f,3.6f));
            Assert.That(gait.MeanStride,Is.InRange(1.8f,3f));
            body.GetComponent<UnitMotor>().Stop();
            yield return new WaitForSeconds(2);
            Assert.That(body.CurrentGait,Is.EqualTo(ProceduralBiped.Gait.Standing));
            Assert.That(body.FootPlanted(0)&&body.FootPlanted(1),Is.True,"A stopped body settles onto both feet.");
        }

        [UnityTest] public IEnumerator TurningBeforeDepartureHandsOverToTheGait()
        {
            // Turning triggers standing adjustment steps. Once the root is moving, those steps
            // must hand over to the gait instead of chasing the body with its feet behind it.
            var body=bodies[0];var agent=body.GetComponent<NavMeshAgent>();
            var target=body.transform.position-body.transform.forward*7;
            Assert.That(body.GetComponent<UnitMotor>().TryMove(target),Is.True);
            int moving=0,standing=0;
            float end=Time.time+10;
            while(Vector3.Distance(body.transform.position,target)>.4f&&Time.time<end)
            {
                yield return null;
                if(agent.velocity.magnitude<agent.speed*.9f) continue;
                moving++;
                if(body.CurrentGait==ProceduralBiped.Gait.Standing) standing++;
            }
            TestContext.WriteLine($"Steady frames {moving}, of which standing {standing}");
            Assert.That(moving,Is.GreaterThan(30),"Observe steady travel.");
            Assert.That(standing,Is.LessThan(moving/10),"A body travelling at its pace must be walking.");
        }

        [UnityTest] public IEnumerator NavigationCorrectionDoesNotStartAGait()
        {
            var body=bodies[0];
            body.ResetPose();
            yield return null;
            Assert.That(body.GetComponent<NavMeshAgent>().Warp(body.transform.position+Vector3.forward*1.5f),Is.True);
            for(int i=0;i<3;i++)
            {
                yield return null;
                Assert.That(body.CurrentGait,Is.EqualTo(ProceduralBiped.Gait.Standing),"A warp is a correction, not locomotion.");
                Assert.That(body.FootPlanted(0)||body.FootPlanted(1),Is.True);
            }
            yield return new WaitForSeconds(2);
            Assert.That(body.FootPlanted(0)&&body.FootPlanted(1),Is.True);
            for(int foot=0;foot<2;foot++)
            {
                var offset=Vector3.ProjectOnPlane(body.FootPosition(foot)-body.transform.position,Vector3.up);
                Assert.That(offset.magnitude,Is.LessThan(.35f),"Standing adjustment steps bring the feet back beneath the body.");
            }
        }
    }
}
