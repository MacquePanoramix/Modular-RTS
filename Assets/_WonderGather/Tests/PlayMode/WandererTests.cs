using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    public sealed class WandererTests
    {
        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
        }
        [UnityTest]
        public IEnumerator MoveNavigatesAroundWallAndArrives()
        {
            var motor = Object.FindFirstObjectByType<UnitMotor>();
            var target = new Vector3(10, 0, 0);
            Assert.That(new CommandDispatcher().Dispatch(new MoveCommand(target), motor), Is.True);
            float timeout = Time.time + 15;
            while (Time.time < timeout && Vector3.Distance(motor.transform.position, target) > .35f) yield return null;
            Assert.That(Vector3.Distance(motor.transform.position, target), Is.LessThan(.35f));
        }
        [UnityTest]
        public IEnumerator InvalidOrdersPreserveCurrentDestination()
        {
            var motor = Object.FindFirstObjectByType<UnitMotor>();
            Assert.That(motor.TryMove(new Vector3(-8, 0, 5)), Is.True);
            var original = motor.Destination;
            Assert.That(motor.TryMove(new Vector3(39, 0, 0)), Is.False);
            Assert.That(motor.TryMove(new Vector3(200, 0, 0)), Is.False);
            Assert.That(motor.TryMove(new Vector3(float.NaN, 0, 0)), Is.False);
            Assert.That(motor.Destination, Is.EqualTo(original));
            yield return null;
        }
        [UnityTest]
        public IEnumerator SelectionCanClearReselectAndLoseDisabledUnit()
        {
            var selection = Object.FindFirstObjectByType<SelectionController>();
            var unit = Object.FindFirstObjectByType<SelectableUnit>();
            selection.Select(unit);
            Assert.That(selection.Selected, Is.EqualTo(unit));
            selection.Select(null);
            Assert.That(selection.Selected, Is.Null);
            selection.Select(unit);
            unit.gameObject.SetActive(false);
            yield return null;
            Assert.That(selection.Selected, Is.Null);
        }
    }
}
