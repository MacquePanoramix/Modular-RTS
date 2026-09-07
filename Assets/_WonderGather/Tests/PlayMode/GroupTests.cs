using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    public sealed class GroupTests
    {
        private SelectionController selection;
        private SelectableUnit[] units;
        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheGroup");
            yield return null;
            selection = Object.FindFirstObjectByType<SelectionController>();
            units = Object.FindObjectsByType<SelectableUnit>(FindObjectsSortMode.InstanceID);
            Assert.That(units.Length, Is.EqualTo(8));
        }
        private void SelectAll() { foreach (var unit in units) selection.Select(unit, true); }

        [UnityTest]
        public IEnumerator ClickToggleReplaceAndDisable()
        {
            selection.Select(units[0]);
            selection.Select(units[1], true);
            Assert.That(selection.Count, Is.EqualTo(2));
            selection.Select(units[0], true);
            Assert.That(selection.Selected, Is.EqualTo(units[1]));
            selection.Select(units[2]);
            Assert.That(selection.Count, Is.EqualTo(1));
            selection.Select(units[3], true);
            units[2].gameObject.SetActive(false);
            yield return null;
            Assert.That(selection.Selected, Is.EqualTo(units[3]));
            selection.enabled = false;
            Assert.That(selection.Count, Is.Zero);
            Assert.That(units[3].transform.Find("Selection ring").gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator BoxSelectAddsWithoutDuplicatesAndClearsEmptySpace()
        {
            var point = Camera.main.WorldToScreenPoint(units[0].transform.position + Vector3.up);
            var small = new Rect(point.x - 2, point.y - 2, 4, 4);
            selection.SelectBox(small, false);
            Assert.That(selection.Count, Is.EqualTo(1));
            Assert.That(selection.Selected, Is.EqualTo(units[0]));
            selection.SelectBox(new Rect(0, 0, Screen.width, Screen.height), true);
            Assert.That(selection.Count, Is.EqualTo(8));
            selection.SelectBox(new Rect(0, 0, Screen.width, Screen.height), true);
            Assert.That(selection.Count, Is.EqualTo(8));
            selection.SelectBox(new Rect(-100, -100, 1, 1), false);
            Assert.That(selection.Count, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BoxExcludesInactiveAndBehindCameraUnits()
        {
            units[0].gameObject.SetActive(false);
            var camera = Camera.main;
            camera.transform.rotation = Quaternion.LookRotation(Vector3.up);
            selection.SelectBox(new Rect(0, 0, Screen.width, Screen.height), false);
            Assert.That(selection.Count, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GroupRoutesAroundWallAndArrivesAtSeparateSlots()
        {
            SelectAll();
            Assert.That(selection.MoveSelection(new Vector3(12, 0, 0)), Is.True);
            for (int i = 0; i < units.Length; i++)
                for (int j = 0; j < i; j++)
                    Assert.That(Vector3.Distance(units[i].Motor.Destination, units[j].Motor.Destination), Is.GreaterThan(1.1f));
            float timeout = Time.time + 22;
            bool arrived = false;
            while (Time.time < timeout)
            {
                arrived = true;
                foreach (var unit in units)
                    arrived &= Vector3.Distance(unit.transform.position, unit.Motor.Destination) < .4f;
                if (arrived) break;
                yield return null;
            }
            var detail = "All eight agents should reach their own slot beyond the wall.";
            foreach (var unit in units) detail += "\n" + unit.name + " at " + unit.transform.position + " target " + unit.Motor.Destination;
            Assert.That(arrived, Is.True, detail);
            for (int i = 0; i < units.Length; i++)
                for (int j = 0; j < i; j++)
                    Assert.That(Vector3.Distance(units[i].transform.position, units[j].transform.position), Is.GreaterThan(.9f));
        }

        [UnityTest]
        public IEnumerator InvalidGroupOrderPreservesEveryExistingOrder()
        {
            SelectAll();
            Assert.That(selection.MoveSelection(new Vector3(-10, 0, 8)), Is.True);
            var destinations = new Vector3[units.Length];
            for (int i = 0; i < units.Length; i++) destinations[i] = units[i].Motor.Destination;
            foreach (var destination in new[] { new Vector3(float.PositiveInfinity, 0, 0), new Vector3(float.NaN, 0, 0) })
            {
                Assert.That(selection.MoveSelection(destination), Is.False);
                for (int i = 0; i < units.Length; i++) Assert.That(units[i].Motor.Destination, Is.EqualTo(destinations[i]));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SingleUnitOrderAndGroupCenter()
        {
            selection.Select(units[0]);
            var target = new Vector3(-10, 0, 9);
            Assert.That(selection.MoveSelection(target), Is.True);
            Assert.That(Vector2.Distance(new Vector2(units[0].Motor.Destination.x, units[0].Motor.Destination.z), new Vector2(target.x, target.z)), Is.LessThan(.05f));
            selection.Select(units[1], true);
            Assert.That(selection.Center, Is.EqualTo((units[0].transform.position + units[1].transform.position) / 2));
            yield return null;
        }

        [Test]
        public void FormationSupportsIncompleteRowsAndRotation()
        {
            foreach (int count in new[] { 1, 2, 5, 8, 9 })
            {
                var slots = GroupMoveCommand.CreateSlots(count, Vector3.zero, Quaternion.Euler(0, 75, 0), 1.8f);
                Assert.That(slots.Count, Is.EqualTo(count));
                for (int i = 0; i < slots.Count; i++)
                    for (int j = 0; j < i; j++) Assert.That(Vector3.Distance(slots[i], slots[j]), Is.GreaterThan(1.79f));
            }
        }
    }
}
