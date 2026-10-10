using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S1b: The Ordinary Place loads whole, its worker can walk to the door, the Explore
    // camera can look into the house but not enter it, and the day turns the lights on and off.
    public sealed class OrdinaryPlaceTests
    {
        private const float Dt = 1 / 60f;
        private RtsCamera rig;
        private SelectableUnit worker;
        private OrdinaryGround ground;
        private TimeOfDay time;
        private Transform house;

        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            rig = Object.FindAnyObjectByType<RtsCamera>();
            worker = Object.FindAnyObjectByType<SelectableUnit>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
            time = Object.FindAnyObjectByType<TimeOfDay>();
            house = GameObject.Find("The lit house").transform;
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
        }

        [Test] public void ThePlaceHasItsHouseGrassWorkerAndLight()
        {
            Assert.That(rig, Is.Not.Null);
            Assert.That(worker, Is.Not.Null);
            Assert.That(time, Is.Not.Null);
            Assert.That(ground.Path.Count, Is.GreaterThan(3));
            var grass = Object.FindAnyObjectByType<GrassField>();
            Assert.That(grass.BladeCount, Is.GreaterThan(200000), "The meadow should be densely planted.");
            Assert.That(house.GetComponentsInChildren<Light>().Length, Is.GreaterThanOrEqualTo(5));
            Assert.That(Object.FindAnyObjectByType<SmokePlume>(), Is.Not.Null);
        }

        [UnityTest] public IEnumerator TheWorkerWalksToTheDoorstep()
        {
            var step = ground.Path[0];
            var doorstep = new Vector3(step.x, ground.Height(step.x, step.y), step.y);
            Assert.That(NavMesh.SamplePosition(doorstep, out var hit, 1.5f, NavMesh.AllAreas), Is.True, "The doorstep is off the navigation mesh.");
            Assert.That(worker.Motor.TryMove(hit.position), Is.True);
            float end = Time.time + 40;
            while (Vector3.Distance(worker.transform.position, hit.position) > .6f && Time.time < end) yield return null;
            Assert.That(Vector3.Distance(worker.transform.position, hit.position), Is.LessThan(.6f), "The worker did not reach the door.");
        }

        [Test] public void ExploreLooksIntoTheHouseButCannotEnterIt()
        {
            rig.Configure(null, null);
            var blocker = house.GetComponentsInChildren<BoxCollider>().Single(x => x.name == "Blocker_Door");
            var front = blocker.transform.position - house.position;
            front.y = 0;
            front = Vector3.Dot(house.forward, front) > 0 ? house.forward : -house.forward;
            float frontWall = Vector3.Dot(blocker.transform.position - house.position, front);
            foreach (var aim in new[] { blocker.transform.position, blocker.transform.position + Vector3.Cross(Vector3.up, front) * 2 })
            {
                var start = aim + front * 3 + Vector3.up * .2f;
                rig.SetMode(CameraMode.Explore);
                rig.Explore.Begin(start, Quaternion.LookRotation(-front));
                float deepest = float.MaxValue;
                for (float t = 0; t < 5; t += Dt)
                {
                    rig.Step(new CameraIntent { Pan = new Vector2(0, 1), Fast = true }, Dt);
                    deepest = Mathf.Min(deepest, Vector3.Dot(rig.transform.position - house.position, front));
                }
                Assert.That(deepest, Is.GreaterThan(frontWall - .25f), $"The camera entered the house aiming at {aim}.");
                Assert.That(deepest, Is.LessThan(frontWall + .6f), "The camera should come close to the front.");
                rig.SetMode(CameraMode.Strategy);
            }
        }

        [Test] public void NightLightsTheHouseAndDayRestsIt()
        {
            var lights = house.GetComponentsInChildren<Light>();
            var sky = RenderSettings.sun != null ? RenderSettings.sun : Object.FindObjectsByType<Light>().First(x => x.type == LightType.Directional);
            time.Hour = 23;
            Assert.That(lights.All(x => x.enabled && x.intensity > 0), Is.True, "Night should light every house light.");
            Assert.That(sky.intensity, Is.LessThan(.6f), "Moonlight should be dim.");
            Assert.That(Shader.GetGlobalFloat("_WG_MoonVisibility"), Is.GreaterThan(.9f), "The moon belongs to the night sky.");
            Assert.That(Shader.GetGlobalFloat("_WG_GlowScale"), Is.GreaterThan(.9f), "Night should light the windows fully.");
            foreach (float hour in new[] { 6.5f, 9f, 12f, 16f })
            {
                time.Hour = hour;
                Assert.That(Shader.GetGlobalFloat("_WG_MoonVisibility"), Is.EqualTo(0).Within(1e-4), $"The moon showed by day at {hour}.");
            }
            Assert.That(Shader.GetGlobalFloat("_WG_GlowScale"), Is.LessThan(.05f), "Day should leave the windows almost unlit.");
            time.Hour = 12;
            Assert.That(lights.All(x => !x.enabled), Is.True, "Day should rest the house lights.");
            Assert.That(sky.intensity, Is.GreaterThan(1.5f), "Daylight should be strong.");
        }

        [UnityTest] public IEnumerator EveryLookCandidateRunsWithoutErrors()
        {
            var look = Object.FindAnyObjectByType<LookDevControls>();
            for (int i = 0; i < LookDevControls.Candidates.Length; i++)
            {
                look.Select(i);
                Assert.That(look.CandidateIndex, Is.EqualTo(i));
                for (int frame = 0; frame < 5; frame++) yield return null;
            }
        }
    }
}
