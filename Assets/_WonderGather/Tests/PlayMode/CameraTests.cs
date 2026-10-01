using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S1a: a fast Strategy camera and a free Explore camera that approaches surfaces
    // closely without ever passing through floors, scenery or buildings.
    public sealed class CameraTests
    {
        private const float Dt = 1 / 60f;
        private RtsCamera rig;
        private Camera view;
        private SelectionController selection;
        private SelectableUnit wanderer;
        private Collider[] stones;

        [UnitySetUp] public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
            rig = Object.FindAnyObjectByType<RtsCamera>();
            view = rig.GetComponent<Camera>();
            selection = Object.FindAnyObjectByType<SelectionController>();
            wanderer = Object.FindAnyObjectByType<SelectableUnit>();
            stones = Object.FindObjectsByType<Collider>().Where(x => x.name.StartsWith("Stone")).ToArray();
            Assert.That(stones.Length, Is.EqualTo(4));
            // Drive the rig directly with intents instead of devices.
            rig.Configure(null, selection);
        }

        private void Run(CameraIntent intent, float seconds, System.Action<Vector3> check = null)
        {
            for (float t = 0; t < seconds; t += Dt)
            {
                rig.Step(intent, Dt);
                check?.Invoke(rig.transform.position);
            }
        }

        private void Explore(Vector3 position, Quaternion rotation)
        {
            rig.SetMode(CameraMode.Explore);
            rig.Explore.Begin(position, rotation);
            rig.Step(default, Dt);
        }

        [Test] public void TogglingIntoExploreDoesNotMoveTheView()
        {
            Run(default, 3);
            var position = rig.transform.position;
            var rotation = rig.transform.rotation;
            rig.Step(new CameraIntent { Toggle = true }, Dt);
            Assert.That(rig.Mode, Is.EqualTo(CameraMode.Explore));
            Assert.That(Vector3.Distance(rig.transform.position, position), Is.LessThan(.01f));
            Assert.That(Quaternion.Angle(rig.transform.rotation, rotation), Is.LessThan(.5f));
            Assert.That(view.nearClipPlane, Is.LessThan(.03f), "Explore needs a very near clip plane to approach surfaces.");
        }

        [Test] public void ExploreApproachesTheGroundButNeverPassesThrough()
        {
            Explore(new Vector3(0, 2, -10), Quaternion.Euler(30, 0, 0));
            float radius = rig.Explore.Radius, lowest = float.MaxValue;
            Run(new CameraIntent { Rotate = -1 }, 6, p => lowest = Mathf.Min(lowest, p.y));
            Run(new CameraIntent { Pan = new Vector2(0, 1) }, 4, p => lowest = Mathf.Min(lowest, p.y));
            TestContext.Out.WriteLine($"lowest eye height {lowest:F4} m, final {rig.transform.position.y:F4} m, radius {radius} m");
            Assert.That(lowest, Is.GreaterThanOrEqualTo(radius * .9f), "The camera dipped below the floor.");
            Assert.That(rig.transform.position.y, Is.LessThan(.15f), "The camera should get very near the floor.");
        }

        [Test] public void ExploreSlidesAlongSceneryWithoutEnteringIt()
        {
            // A long building wall: the camera flies at it at an angle, stops at its face and slides along it.
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Test wall";
            wall.transform.position = new Vector3(-12, 2, 4);
            wall.transform.localScale = new Vector3(.4f, 4, 16);
            Physics.SyncTransforms();
            var bounds = wall.GetComponent<Collider>().bounds;
            Explore(new Vector3(-16, 1.3f, -2), Quaternion.Euler(0, 60, 0));
            var start = rig.transform.position;
            Run(new CameraIntent { Pan = new Vector2(0, 1) }, 6, p =>
            {
                var inner = bounds;
                inner.Expand(-rig.Explore.Radius);
                Assert.That(inner.Contains(p), Is.False, $"Entered the wall at {p}.");
                Assert.That(p.x, Is.LessThan(bounds.min.x), $"Passed through the wall at {p}.");
            });
            var end = rig.transform.position;
            TestContext.Out.WriteLine($"gap to wall {bounds.min.x - end.x:F4} m, slid {end.z - start.z:F2} m along it");
            Assert.That(bounds.min.x - end.x, Is.LessThan(.15f), "The camera should come right up to the wall.");
            Assert.That(end.z - start.z, Is.GreaterThan(1.5f), "The camera should slide along the wall rather than stop dead.");
            foreach (var stone in stones)
            {
                Explore(stone.bounds.center - Vector3.right * 3, Quaternion.Euler(0, 90, 0));
                Run(new CameraIntent { Pan = new Vector2(0, 1), Fast = true }, 3, p =>
                    Assert.That(p.x, Is.LessThan(stone.bounds.min.x), $"Passed into {stone.name} at {p}."));
            }
        }

        [Test] public void ExploreCanLookStraightUpAtTheSky()
        {
            Explore(new Vector3(0, 1.6f, -6), Quaternion.identity);
            Run(new CameraIntent { Look = true, LookDragging = true, PointerDelta = new Vector2(0, 40) }, 1);
            Assert.That(rig.transform.forward.y, Is.GreaterThan(.99f));
        }

        [Test] public void ReturningToStrategyRecentersOnTheGroundInView()
        {
            Explore(new Vector3(-8, 3, -8), Quaternion.Euler(20, -20, 0));
            var seen = rig.transform.position + rig.transform.forward * (3 / Mathf.Sin(20 * Mathf.Deg2Rad));
            var before = rig.transform.position;
            rig.Step(new CameraIntent { Toggle = true }, Dt);
            Assert.That(rig.Mode, Is.EqualTo(CameraMode.Strategy));
            Assert.That(Vector2.Distance(new Vector2(rig.StrategyTarget.x, rig.StrategyTarget.z), new Vector2(seen.x, seen.z)), Is.LessThan(.1f));
            Assert.That(Mathf.DeltaAngle(rig.StrategyYaw, -20), Is.EqualTo(0).Within(.5f), "Strategy keeps the Explore heading.");
            Assert.That(Vector3.Distance(rig.transform.position, before), Is.LessThan(.5f), "The return glides instead of jumping.");
            Run(default, 1);
            Assert.That(rig.Blending, Is.False);
        }

        [UnityTest] public IEnumerator ExploreFollowKeepsAWalkingWorkerFramed()
        {
            selection.Select(wanderer);
            Explore(new Vector3(-10, 3, -12), Quaternion.Euler(15, 30, 0));
            rig.Step(new CameraIntent { Focus = true }, Dt);
            Assert.That(rig.Explore.Following, Is.True);
            Assert.That(wanderer.Motor.TryMove(new Vector3(-4, 0, 22)), Is.True);
            var start = wanderer.transform.position;
            float end = Time.time + 20;
            float worstAngle = 0, settledAt = Time.time + 2;
            while (Vector3.Distance(wanderer.transform.position, start) < 15 && Time.time < end)
            {
                yield return null;
                rig.Step(default, Time.deltaTime);
                if (Time.time < settledAt) continue;
                var chest = wanderer.transform.position + Vector3.up;
                worstAngle = Mathf.Max(worstAngle, Vector3.Angle(rig.transform.forward, chest - rig.transform.position));
            }
            Assert.That(Vector3.Distance(wanderer.transform.position, start), Is.GreaterThanOrEqualTo(15), "The worker did not walk.");
            Assert.That(rig.Explore.Following, Is.True);
            TestContext.Out.WriteLine($"worst off-centre angle {worstAngle:F1}°, final distance {Vector3.Distance(rig.transform.position, wanderer.transform.position):F2} m");
            Assert.That(worstAngle, Is.LessThan(15), "The followed worker drifted out of the centre of view.");
            Assert.That(Vector3.Distance(rig.transform.position, wanderer.transform.position), Is.LessThan(6), "The camera fell behind the worker.");
        }

        [Test] public void WorkersNudgeTheCameraAsideInsteadOfBeingEntered()
        {
            var body = wanderer.GetComponentInChildren<Collider>();
            var surface = body.ClosestPoint(body.bounds.center + Vector3.right * 5);
            Explore(surface + Vector3.right * .05f, Quaternion.Euler(0, -90, 0));
            Run(default, 1.5f);
            float clearance = Vector3.Distance(body.ClosestPoint(rig.transform.position), rig.transform.position);
            TestContext.Out.WriteLine($"clearance from the body after the nudge {clearance:F3} m (started at .05 m)");
            Assert.That(clearance, Is.GreaterThan(.2f), "The worker's personal space should push the camera back.");
        }

        [Test] public void StrategyZoomKeepsTheGroundUnderTheCursor()
        {
            Run(default, 3);
            var pointer = new Vector2(view.pixelWidth * .72f, view.pixelHeight * .35f);
            Assert.That(Physics.Raycast(view.ScreenPointToRay(pointer), out var hit, 500, 1 << 6), Is.True);
            Run(new CameraIntent { Zoom = 120, Pointer = pointer, PointerInWorld = true }, Dt);
            Run(default, 3);
            var screen = view.WorldToScreenPoint(hit.point);
            TestContext.Out.WriteLine($"screen {view.pixelWidth}x{view.pixelHeight}, anchor error {Vector2.Distance(screen, pointer):F2} px, camera distance to aim {Vector3.Distance(rig.transform.position, hit.point):F1} m");
            Assert.That(Vector2.Distance(screen, pointer), Is.LessThan(view.pixelHeight * .02f), $"The zoom anchor drifted to {screen}.");
        }

        [Test] public void MiddleDragMovesTheGroundWithTheCursor()
        {
            Run(default, 3);
            var start = new Vector2(view.pixelWidth * .5f, view.pixelHeight * .5f);
            Assert.That(Physics.Raycast(view.ScreenPointToRay(start), out var hit, 500, 1 << 6), Is.True);
            var drag = new Vector2(view.pixelWidth * .1f, view.pixelHeight * .08f);
            rig.Step(new CameraIntent { Middle = true, Pointer = start, PointerInWorld = true }, Dt);
            rig.Step(new CameraIntent { Middle = true, Pointer = start + drag, PointerDelta = drag, PointerInWorld = true }, Dt);
            var screen = view.WorldToScreenPoint(hit.point);
            TestContext.Out.WriteLine($"grab error {Vector2.Distance(screen, start + drag):F2} px for a {drag.magnitude:F0} px drag");
            Assert.That(Vector2.Distance(screen, start + drag), Is.LessThan(view.pixelHeight * .01f), $"The grabbed ground moved to {screen}.");
        }
    }
}
