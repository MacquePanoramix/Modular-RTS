using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The dusk details over the hand-painted look: fireflies come out only after sunset, the
    // hearth's flicker moves the house lights only when switched on, the window glow has a halo
    // for each window and the door, and each detail switches independently.
    public sealed class DuskDetailsTests
    {
        private TimeOfDay time;
        private LookDevControls look;

        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            time = Object.FindAnyObjectByType<TimeOfDay>();
            look = Object.FindAnyObjectByType<LookDevControls>();
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
        }

        [Test] public void FirefliesAreOnAndComeOutOnlyAfterSunset()
        {
            var flies = Object.FindAnyObjectByType<Fireflies>();
            Assert.That(flies, Is.Not.Null);
            Assert.That(look.FirefliesOn, Is.True, "Luis asked for the fireflies in this scene.");
            time.Hour = 12;
            Assert.That(flies.Presence, Is.EqualTo(0).Within(1e-4), "No fireflies by day.");
            time.Hour = 19.2f;
            Assert.That(flies.Presence, Is.GreaterThan(.9f), "Fireflies at dusk.");
            time.Hour = 23;
            Assert.That(flies.Presence, Is.EqualTo(1).Within(1e-4), "Fireflies at night.");
        }

        // Luis, October 8: "the points are not based on the camera lock. They're, like, its own entities".
        [UnityTest] public IEnumerator EachFireflyHasItsOwnHomeInTheMeadowWhateverTheCameraDoes()
        {
            var flies = Object.FindAnyObjectByType<Fireflies>();
            var ground = Object.FindAnyObjectByType<OrdinaryGround>();
            time.Hour = 23;
            yield return null;
            var before = flies.Homes.ToArray();
            Assert.That(before.Length, Is.GreaterThan(100), "There are fireflies in the meadow.");
            float meadow = 4 * ground.InnerHalf * ground.InnerHalf;
            Assert.That(before.Length, Is.LessThan(meadow / 18), "Far fewer than there were (one to every 3 m2 close by): no more than one to 18 m2 of meadow.");
            foreach (var home in before)
            {
                Assert.That(ground.GrassDensity(home.x, home.z), Is.GreaterThan(0), $"A firefly lives where grass grows; one is at {home.x:F1}, {home.z:F1}.");
                Assert.That(home.y, Is.EqualTo(ground.Height(home.x, home.z)).Within(.01f), "Its home is on its own ground.");
            }
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            camera.transform.SetPositionAndRotation(new Vector3(31, 40, -22), Quaternion.Euler(62, 140, 0));
            for (int i = 0; i < 5; i++) yield return null;
            Assert.That(flies.Homes.ToArray(), Is.EqualTo(before), "Moving the camera moved the fireflies' homes.");
            if (rig != null) rig.enabled = true;
        }

        // "They appear based on your distance from them, and when it's very far away, there are less of them that you see, and they are less bright."
        [Test] public void FromFurtherAwayFewerFirefliesAreSeenAndTheyAreFainter()
        {
            var flies = Object.FindAnyObjectByType<Fireflies>();
            Assert.That(flies.ShareSeen(5), Is.EqualTo(1).Within(1e-4), "From close, all that are out are seen.");
            Assert.That(flies.Brightness(5), Is.EqualTo(1).Within(1e-4), "From close, at their brightest.");
            for (float d = 5; d < 130; d += 2.5f)
            {
                Assert.That(flies.ShareSeen(d + 2.5f), Is.LessThanOrEqualTo(flies.ShareSeen(d) + 1e-5f), $"More are seen from {d + 2.5f} m than from {d} m.");
                Assert.That(flies.Brightness(d + 2.5f), Is.LessThanOrEqualTo(flies.Brightness(d) + 1e-5f), $"They are brighter from {d + 2.5f} m than from {d} m.");
            }
            // The camera's farthest view is 42 m from what it looks at: the ground it shows is 40 to 90 m from the eye.
            Assert.That(flies.ShareSeen(45), Is.LessThan(.2f).And.GreaterThan(0), "From the farthest view few are seen, but there is still a sign of them.");
            Assert.That(flies.Brightness(45), Is.LessThan(.3f).And.GreaterThan(0));
            Assert.That(flies.ShareSeen(130), Is.EqualTo(0).Within(1e-4), "Beyond the last distance none is seen.");
        }

        // Luis, October 2 and 8: from far away "the cabin light gets suddenly much stronger". The pipeline drew no
        // shadow further than 50 m from the eye, so past that the lamps lit the ground through the walls.
        [UnityTest] public IEnumerator ShadowsReachAsFarAsTheLampsAreSeenFrom()
        {
            time.Hour = 23;
            yield return null;
            var reach = Object.FindAnyObjectByType<ShadowReach>();
            Assert.That(reach, Is.Not.Null, "The place has nothing that keeps the lamps' shadows from far away.");
            var inside = Object.FindObjectsByType<Light>().First(x => x.name == "Interior light");
            Assert.That(inside.shadows, Is.Not.EqualTo(LightShadows.None), "The fire inside is stopped by the house's walls.");
            foreach (float away in new[] { 20f, 45f, 60f, 90f, 150f })
            {
                var eye = inside.transform.position + new Vector3(0, .6f, -.8f).normalized * away;
                float needed = reach.Needed(eye, .1f);
                Assert.That(needed * .9f, Is.GreaterThanOrEqualTo(away + inside.range), $"From {away} m the shadows end (at {needed:F0} m, fading from {needed * .9f:F0}) short of what the fire inside lights.");
            }
            float distance = ShadowReach.PipelineDistance;
            Assert.That(distance, Is.GreaterThan(0));
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            camera.transform.SetPositionAndRotation(inside.transform.position + new Vector3(0, 60, -50), Quaternion.Euler(50, 0, 0));
            yield return null;
            // A test run draws nothing by itself: one picture is asked for.
            var picture = RenderTexture.GetTemporary(96, 54, 24);
            camera.targetTexture = picture;
            camera.Render();
            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(picture);
            Assert.That(ShadowReach.PipelineDistance, Is.EqualTo(distance), "The pipeline's own setting was left changed after a frame.");
            Assert.That(reach.LastReach, Is.GreaterThan(distance), "From 78 m away the shadows were not made to reach further than the pipeline's own distance.");
            if (rig != null) rig.enabled = true;
            time.Hour = 12;
            yield return null;
            Assert.That(reach.Needed(camera.transform.position, .1f), Is.EqualTo(0), "By day, with no lamp lit, nothing is asked of the shadows.");
        }

        // Luis, October 2: "I really liked how it looked from far away": the lamplight spreading over the ground before the
        // house. That is now the look at every distance; the other (the lamplight shaded) is a switch away.
        [Test] public void TheLamplightSpreadsUnlessItsShadowsAreSwitchedOn()
        {
            time.Hour = 23;
            var door = Object.FindObjectsByType<Light>().First(x => x.name == "Door light");
            var inside = Object.FindObjectsByType<Light>().First(x => x.name == "Interior light");
            Assert.That(look.LampShadowsOn, Is.False, "The look Luis liked from far away is the one the place starts with.");
            Assert.That(door.shadowStrength, Is.EqualTo(0), "The door's light spreads.");
            Assert.That(inside.shadowStrength, Is.GreaterThan(.9f), "The fire inside stays behind its walls.");
            look.LampShadowsOn = true;
            Assert.That(door.shadowStrength, Is.EqualTo(1), "Switched on, the door's light is shaded by what stands in it.");
            look.LampShadowsOn = false;
            Assert.That(door.shadowStrength, Is.EqualTo(0));
        }

        [UnityTest] public IEnumerator TheHearthFlickersOnlyWhenSwitchedOn()
        {
            var interior = Object.FindObjectsByType<Light>().First(x => x.name == "Interior light");
            Assert.That(time.Hearth, Is.True, "Luis keeps the hearth on.");
            time.Hour = 23;
            time.Hearth = false;
            var calm = new List<float>();
            for (int i = 0; i < 20; i++) { yield return new WaitForSeconds(.05f); calm.Add(interior.intensity); }
            Assert.That(calm.Max() - calm.Min(), Is.LessThan(1e-4f), "Without the hearth the light is steady.");
            time.Hearth = true;
            var fire = new List<float>();
            for (int i = 0; i < 40; i++) { yield return new WaitForSeconds(.05f); fire.Add(interior.intensity); }
            Assert.That(fire.Max() - fire.Min(), Is.GreaterThan(.05f * calm[0]), "With the hearth the light should breathe.");
            Assert.That(fire.Min(), Is.GreaterThan(.5f * calm[0]), "The fire should flicker gently, never go out.");
            time.Hearth = true;
        }

        [Test] public void TheWindowGlowHasAHaloForEachWindowAndTheDoor()
        {
            var glow = Object.FindAnyObjectByType<WindowGlow>(FindObjectsInactive.Include);
            Assert.That(glow, Is.Not.Null);
            Assert.That(glow.Count, Is.EqualTo(4), "Three windows and the door.");
            Assert.That(look.WindowGlowOn, Is.True, "Luis keeps the window glow on.");
        }

        [Test] public void EachDuskDetailSwitchesIndependently()
        {
            look.SetDusk(false, true, true);
            Assert.That((look.FirefliesOn, look.HearthOn, look.WindowGlowOn), Is.EqualTo((false, true, true)));
            look.SetDusk(true, false, true);
            Assert.That((look.FirefliesOn, look.HearthOn, look.WindowGlowOn), Is.EqualTo((true, false, true)));
            look.SetDusk(true, false, false);
            Assert.That((look.FirefliesOn, look.HearthOn, look.WindowGlowOn), Is.EqualTo((true, false, false)));
        }
    }
}
