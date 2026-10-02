using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S1e: the world beyond the Ordinary Place. The far land, water and clouds surround the
    // meadow without touching its walkable ground, the viewpoints keep the Explore camera inside
    // the world, cloud shadows and the air follow the day, and the clouds drift with the wind.
    public sealed class BeyondTests
    {
        private OrdinaryGround ground;
        private TimeOfDay time;
        private CloudBank clouds;
        private WaterSurface water;

        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
            time = Object.FindAnyObjectByType<TimeOfDay>();
            clouds = Object.FindAnyObjectByType<CloudBank>();
            water = Object.FindAnyObjectByType<WaterSurface>();
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
        }

        [Test] public void TheBeyondSurroundsTheMeadow()
        {
            Assert.That(clouds, Is.Not.Null);
            Assert.That(clouds.Count, Is.GreaterThanOrEqualTo(20), "The sky should hold towers, heaps and banks.");
            Assert.That(water, Is.Not.Null);
            Assert.That(water.Level, Is.EqualTo(ground.WaterLevel));
            Assert.That(ground.Height(-250, 1500), Is.LessThan(water.Level - 5), "The lake beyond the house should be water.");
            Assert.That(ground.Height(6000, 1000), Is.LessThan(water.Level - 5), "The water should open east to the horizon.");
            Assert.That(ground.Height(-900, 5600), Is.GreaterThan(900), "The great peak should stand beyond the lake.");
            Assert.That(Camera.main.farClipPlane, Is.GreaterThan(9000), "The camera should see out to the far ranges.");
        }

        [Test] public void TheMeadowIsUntouchedByTheBeyond()
        {
            var random = new System.Random(3);
            float half = ground.InnerHalf;
            for (int i = 0; i < 200; i++)
            {
                float x = (float)(random.NextDouble() * 2 - 1) * half;
                float z = (float)(random.NextDouble() * 2 - 1) * half;
                float h = ground.Height(x, z);
                Assert.That(h, Is.InRange(-3f, 3f), $"The meadow's ground moved at ({x:F0}, {z:F0}).");
                Assert.That(h, Is.GreaterThan(water.Level + 15), "The meadow must stay well above the water.");
                // The baked navigation still lies on the ground wherever the meadow is walkable.
                if (ground.GrassDensity(x, z) > .5f && Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) < half - 6
                    && NavMesh.SamplePosition(new Vector3(x, h, z), out var hit, 2, NavMesh.AllAreas))
                    Assert.That(Mathf.Abs(hit.position.y - ground.Height(hit.position.x, hit.position.z)), Is.LessThan(.6f),
                        $"The navigation mesh no longer matches the ground at ({x:F0}, {z:F0}).");
            }
        }

        [Test] public void TheViewpointsKeepTheExploreCameraInTheWorld()
        {
            var look = Object.FindAnyObjectByType<LookDevControls>();
            var rig = Object.FindAnyObjectByType<RtsCamera>();
            rig.Configure(null, null);
            for (int i = 0; i < LookDevControls.Viewpoints.Length; i++)
            {
                look.GoTo(i);
                Assert.That(rig.Mode, Is.EqualTo(CameraMode.Explore));
                for (int frame = 0; frame < 30; frame++) rig.Step(default, 1 / 60f);
                var p = rig.transform.position;
                Assert.That(Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.z)), Is.LessThanOrEqualTo(ground.InnerHalf + .01f), $"Viewpoint {i} left the meadow.");
                Assert.That(p.y, Is.GreaterThan(ground.Height(p.x, p.z)), $"Viewpoint {i} went under the ground.");
                Assert.That(Vector3.Distance(p, LookDevControls.Viewpoints[i].Eye + Vector3.up * ground.Height(p.x, p.z)), Is.LessThan(1.5f),
                    $"Viewpoint {i} did not hold its composed position.");
            }
        }

        [Test] public void CloudShadowsAndTheAirFollowTheDay()
        {
            time.Hour = 12;
            var shadows = Shader.GetGlobalVector("_WG_CloudShadow");
            Assert.That(shadows.y, Is.GreaterThan(.3f), "Clouds should shade the land by day.");
            Assert.That(Shader.GetGlobalVector("_WG_Air").x, Is.GreaterThan(1000), "The air should thicken only over kilometres.");
            time.Hour = 23;
            Assert.That(Shader.GetGlobalVector("_WG_CloudShadow").y, Is.EqualTo(0).Within(1e-4), "Cloud shadows belong to the sun.");
        }

        [UnityTest] public IEnumerator TheCloudsDriftWithTheWind()
        {
            var cloud = clouds.transform.GetChild(0).GetChild(0);
            var before = cloud.position;
            yield return new WaitForSeconds(1);
            var moved = cloud.position - before;
            Assert.That(new Vector2(moved.x, moved.z).magnitude, Is.GreaterThan(1), "The clouds should drift.");
            Assert.That(Vector2.Dot(new Vector2(moved.x, moved.z).normalized, new Vector2(.82f, .57f).normalized), Is.GreaterThan(.9f),
                "The clouds should drift with the wind.");
        }
    }
}
