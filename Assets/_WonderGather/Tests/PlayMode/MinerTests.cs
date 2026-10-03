using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S1d: the three miners in the Ordinary Place. One stands at a time and the choice swaps them in place; each
    // is light (three levels of detail, one painted material) and walks to the door on the procedural body, its
    // modelled bones staying on the body's solved joints.
    public sealed class MinerTests
    {
        private MinerChoice choice;
        private OrdinaryGround ground;

        [UnitySetUp] public IEnumerator Load()
        {
            PlayerPrefs.DeleteKey("WonderGather.Miner");
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            PlayerPrefs.DeleteKey("WonderGather.Miner");
            yield return SceneManager.LoadSceneAsync("TheWanderer");
            yield return null;
        }

        private static MinerBody[] All() => Object.FindObjectsByType<MinerBody>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        [Test] public void ThreeMinersAreOfferedAndOneStands()
        {
            Assert.That(choice, Is.Not.Null, "The Ordinary Place has no choice of miner.");
            Assert.That(choice.Count, Is.EqualTo(3));
            Assert.That(Enumerable.Range(0, 3).Select(choice.NameOf), Is.EqualTo(new[] { "Small", "Long", "Round" }));
            Assert.That(All().Count(x => x.gameObject.activeInHierarchy), Is.EqualTo(1), "Exactly one miner should stand in the world.");
            Assert.That(Object.FindAnyObjectByType<SelectableUnit>(), Is.SameAs(choice.Current));
        }

        [UnityTest] public IEnumerator ChoosingAnotherMinerPutsItWhereTheLastStood()
        {
            var first = choice.Current;
            var at = first.transform.position;
            var selection = Object.FindAnyObjectByType<SelectionController>();
            selection.Select(first);
            for (int next = 1; next < 3; next++)
            {
                choice.Choose(next);
                yield return null;
                Assert.That(choice.Current.gameObject.activeInHierarchy, Is.True);
                Assert.That(All().Count(x => x.gameObject.activeInHierarchy), Is.EqualTo(1));
                Assert.That(Vector3.Distance(choice.Current.transform.position, at), Is.LessThan(.05f), "The new miner should stand where the last one stood.");
                Assert.That(selection.Selected, Is.SameAs(choice.Current), "The new miner should take over the selection.");
                // Its body stands where it now is at once, not dragging its feet from wherever it last stood.
                yield return null;
                var rig = choice.Current.GetComponent<MinerBody>().Rig;
                var feet = (rig.feet[0].position + rig.feet[1].position) * .5f;
                Assert.That(Vector3.ProjectOnPlane(feet - at, Vector3.up).magnitude, Is.LessThan(.25f), "The new miner's feet should be under it.");
            }
        }

        [Test] public void EachMinerIsLightEnoughForTheRts()
        {
            foreach (var miner in All())
            {
                var lods = miner.GetComponentInChildren<LODGroup>(true);
                Assert.That(lods, Is.Not.Null, miner.name + " has no levels of detail.");
                var levels = lods.GetLODs();
                Assert.That(levels.Length, Is.EqualTo(3));
                int[] budget = { 20000, 6000, 2000 };
                for (int i = 0; i < 3; i++)
                {
                    var renderer = (SkinnedMeshRenderer)levels[i].renderers.Single();
                    int triangles = Enumerable.Range(0, renderer.sharedMesh.subMeshCount).Sum(s => (int)renderer.sharedMesh.GetIndexCount(s)) / 3;
                    Assert.That(triangles, Is.LessThanOrEqualTo(budget[i]), $"{miner.name} LOD{i} has {triangles} triangles.");
                    // One painted material for the body (and one for a lamp's glass), plus the outline on the nearer two.
                    Assert.That(renderer.sharedMaterials.Length, Is.LessThanOrEqualTo(i < 2 ? 3 : 2));
                    Assert.That(renderer.bones.Length, Is.LessThanOrEqualTo(24));
                }
            }
        }

        [UnityTest] public IEnumerator EachMinerWalksToTheDoorOnItsOwnBones()
        {
            var step = ground.Path[0];
            var doorstep = new Vector3(step.x, ground.Height(step.x, step.y), step.y);
            Assert.That(NavMesh.SamplePosition(doorstep, out var hit, 1.5f, NavMesh.AllAreas), Is.True);
            for (int index = 0; index < 3; index++)
            {
                choice.Choose(index);
                yield return null;
                yield return null;
                var unit = choice.Current;
                var start = unit.transform.position;
                var biped = unit.GetComponent<ProceduralBiped>();
                var body = unit.GetComponent<MinerBody>();
                float ankle = biped.BodyProportions.ankleHeight;
                Assert.That(unit.Motor.TryMove(hit.position), Is.True);
                float end = Time.time + 40, worst = 0;
                int frames = 0;
                while (Vector3.Distance(unit.transform.position, hit.position) > .6f && Time.time < end)
                {
                    yield return null;
                    if (!biped.Ready || !body.Ready) continue;
                    frames++;
                    // The modelled ankles stay on the body's solved feet: the rig and the solution agree.
                    for (int i = 0; i < 2; i++)
                    {
                        var solvedAnkle = biped.FootPosition(i) + biped.FootNormal(i) * ankle;
                        if (biped.FootPlanted(i)) worst = Mathf.Max(worst, Vector3.Distance(body.Rig.feet[i].position, solvedAnkle));
                    }
                }
                Assert.That(Vector3.Distance(unit.transform.position, hit.position), Is.LessThan(.6f), $"{choice.NameOf(index)} did not reach the door.");
                Assert.That(frames, Is.GreaterThan(30));
                Assert.That(worst, Is.LessThan(.09f), $"{choice.NameOf(index)}'s modelled ankle strayed {worst:F3} m from the planted foot.");
                Assert.That(biped.StepCount, Is.GreaterThan(4), $"{choice.NameOf(index)} should have walked, step by step.");
                // Back to the start for the next miner.
                unit.GetComponent<NavMeshAgent>().Warp(start);
                yield return null;
            }
        }

        [UnityTest] public IEnumerator TheMinersHeadsStayAboveTheirFeet()
        {
            for (int index = 0; index < 3; index++)
            {
                choice.Choose(index);
                for (int k = 0; k < 20; k++) yield return null;
                var body = choice.Current.GetComponent<MinerBody>();
                var feet = (body.Rig.feet[0].position + body.Rig.feet[1].position) * .5f;
                Assert.That(body.Rig.head.position.y - feet.y, Is.GreaterThan(.7f), $"{choice.NameOf(index)} is not standing up.");
                Assert.That(Vector3.ProjectOnPlane(body.Rig.head.position - feet, Vector3.up).magnitude, Is.LessThan(.35f), $"{choice.NameOf(index)} leans too far.");
            }
        }
    }
}
