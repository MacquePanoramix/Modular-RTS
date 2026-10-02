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
