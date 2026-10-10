using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, step 7: a miner holds and walks with its tool as its strength allows (PhysicalCarry). Its own pickaxe it
    // carries in one hand at its side. One too heavy for its hand's hold it drags by the end of its handle, and walks
    // slower for it. A tired arm drags what it carried. What it cannot move, it leaves.
    public sealed class PhysicalCarryTests
    {
        private MinerChoice choice;
        private OrdinaryGround ground;
        private Vector3 spot, away;

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
            var a = ground.Path[4];
            var b = ground.Path[5];
            spot = OnGround(new Vector3(a.x, 0, a.y));
            away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
            away.y = 0;
            away.Normalize();
        }

        [TearDown]
        public void Restore() { Time.captureFramerate = 0; }

        private Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        private int Index(string name)
        {
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == name) return i;
            Assert.Fail("No miner is called " + name);
            return -1;
        }

        private struct Carried
        {
            // How it held the tool when it set off, and when it had come to a stand.
            public PhysicalCarry.Way first, last;
            // How far it walked and how long that took; its own walking pace; the least share of it it kept; the most
            // it pulled with (newtons).
            public float covered, took, pace, slowest, pulled;
            // The furthest a hand was off the handle while it walked; how high the tool's lowest part was over the
            // ground, at its lowest and on average, while it walked; which hands held at the end; whether it still had
            // the tool; and the share of its hold the holding hand gave, on average.
            public float miss, headLowest, headMean;
            public bool left, right, has, lies;
            public float hold;
        }

        // A miner takes its pickaxe (at a share of its weight) as it would to carry it, and walks some metres with it.
        private IEnumerator Carry(string who, float strength, float weight, float walk, System.Action<Carried> done)
        {
            choice.Choose(Index(who));
            yield return Wait(.2f);
            var unit = choice.Current;
            var miner = unit.GetComponent<MinerBody>();
            var biped = unit.GetComponent<ProceduralBiped>();
            var agent = unit.GetComponent<NavMeshAgent>();
            var physical = unit.GetComponent<PhysicalBody>();
            unit.Motor.Stop();
            agent.Warp(spot);
            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
            biped.ResetPose();
            yield return Wait(.8f);
            physical.Strength = strength;
            physical.Refresh();
            var definition = miner.Pickaxe;
            var hands = unit.gameObject.AddComponent<PhysicalHands>();
            var back = unit.gameObject.AddComponent<PhysicalBack>();
            var balance = unit.gameObject.AddComponent<PhysicalBalance>();
            var swing = unit.gameObject.AddComponent<PhysicalSwing>();
            swing.enabled = false;
            swing.hands = hands; swing.back = back; swing.body = biped; swing.tool = definition;
            var carry = unit.gameObject.AddComponent<PhysicalCarry>();
            carry.enabled = false;
            carry.hands = hands; carry.back = back; carry.body = biped; carry.tool = definition;
            yield return Wait(.3f);
            swing.Intend(PhysicalSwing.Rest, out var at, out var turned);
            hands.Take(definition, at, turned, weight);
            carry.enabled = true;
            yield return Wait(2.4f);
            var carried = new Carried { first = carry.way, pace = agent.speed, slowest = 1, headLowest = float.MaxValue };
            Vector3 first = unit.transform.position;
            float began = Time.time, heads = 0, holds = 0;
            int seen = 0;
            var then = new GameObject("After everything").AddComponent<AfterEverything>();
            then.Then = () =>
            {
                if (hands.Held == null || !unit.Motor.IsMoving || Time.time - began < .8f) return;
                carried.miss = Mathf.Max(carried.miss, Mathf.Max(hands.Miss(0), hands.Miss(1)));
                // How high the tool's lowest part is over the ground.
                float low = float.MaxValue;
                foreach (var solid in hands.Held.GetComponents<Collider>()) low = Mathf.Min(low, solid.bounds.min.y);
                float up = low - ground.Height(hands.Held.worldCenterOfMass.x, hands.Held.worldCenterOfMass.z);
                carried.headLowest = Mathf.Min(carried.headLowest, up); heads += up; seen++;
                carried.slowest = Mathf.Min(carried.slowest, carry.Pace); carried.pulled = Mathf.Max(carried.pulled, carry.Pull);
                holds += Mathf.Max(hands.Hold(0), hands.Hold(1));
            };
            Assert.That(unit.Motor.TryMove(OnGround(spot + away * walk)), Is.True);
            while (Time.time - began < 40 && (unit.Motor.IsMoving || Time.time - began < .5f)) yield return null;
            carried.took = Time.time - began;
            carried.covered = Vector3.Distance(unit.transform.position, first);
            yield return Wait(1.5f);
            then.Then = null;
            Object.Destroy(then.gameObject);
            carried.last = carry.way;
            carried.headMean = seen > 0 ? heads / seen : 0;
            carried.hold = seen > 0 ? holds / seen : 0;
            carried.has = hands.Held != null;
            carried.left = hands.Holds(0); carried.right = hands.Holds(1);
            carried.lies = carry.Lies != null;
            unit.Motor.Stop();
            var was = hands.Drop();
            if (was != null) Object.Destroy(was.gameObject);
            if (carry.Lies != null) Object.Destroy(carry.Lies.gameObject);
            Object.Destroy(carry);
            Object.Destroy(swing);
            Object.Destroy(hands);
            Object.Destroy(back);
            Object.Destroy(balance);
            physical.Strength = 1;
            physical.Refresh();
            yield return Wait(.3f);
            done(carried);
        }

        // Its own pickaxe is nothing to its hand: carried in one hand at its side, at its own walking pace.
        [UnityTest, Timeout(300000)]
        public IEnumerator EachMinerWalksWithItsOwnPickaxeInOneHand()
        {
            Time.captureFramerate = 50;
            for (int index = 0; index < choice.Count; index++)
            {
                string name = choice.NameOf(index);
                Carried c = default;
                yield return Carry(name, 1, 1, 3.5f, r => c = r);
                Debug.Log($"CARRY_OWN {name}: {c.first}; walked {c.covered:F2} m in {c.took:F1} s (its pace {c.pace:F2} m/s); hands at most {c.miss * 1000:F0} mm off; its hand gave {c.hold:P0} of its hold on average; the tool {c.headLowest:F2} m over the ground at its lowest");
                Assert.That(c.first, Is.EqualTo(PhysicalCarry.Way.OneHand), name + " should carry its own pickaxe in one hand.");
                Assert.That(c.last, Is.EqualTo(PhysicalCarry.Way.OneHand));
                Assert.That(c.has && c.left && !c.right, Is.True, name + " should hold it in its left hand alone.");
                Assert.That(c.covered, Is.GreaterThan(3.2f), name + " did not get there.");
                Assert.That(c.took, Is.LessThan(3.5f / c.pace * 1.35f), name + " walked slower for a pickaxe it can carry.");
                Assert.That(c.slowest, Is.EqualTo(1).Within(.001f));
                Assert.That(c.miss, Is.LessThan(.015f), name + "'s hand came off the handle as it walked.");
                Assert.That(c.headLowest, Is.GreaterThan(.12f), name + "'s pickaxe touched the ground: it should be carried.");
                Assert.That(c.hold, Is.LessThan(.3f), name + "'s own pickaxe should be easy for its hand.");
            }
        }

        // A pickaxe too heavy for the hand's hold is dragged by the end of its handle, its head on the ground, and the
        // walk is slower for it.
        [UnityTest, Timeout(300000)]
        public IEnumerator AToolTooHeavyForItsHandIsDraggedAndSlowsTheWalk()
        {
            Time.captureFramerate = 50;
            Carried c = default;
            yield return Carry("Round", .35f, 3, 3.5f, r => c = r);
            Debug.Log($"CARRY_DRAGGED Round at 0.35 of its strength, its pickaxe three times as heavy: {c.first}; walked {c.covered:F2} m in {c.took:F1} s (its pace {c.pace:F2} m/s); slowest {c.slowest:P0} of its pace, pulling {c.pulled:F0} N; the tool's lowest part {c.headMean:F2} m over the ground on average; hands at most {c.miss * 1000:F0} mm off");
            Assert.That(c.first, Is.EqualTo(PhysicalCarry.Way.Dragged), "A pickaxe that asks more than half its hand's hold should be dragged.");
            Assert.That(c.has && c.right && !c.left, Is.True, "It should hold it by its right hand alone.");
            Assert.That(c.covered, Is.GreaterThan(3.2f), "It did not get there.");
            Assert.That(c.headMean, Is.LessThan(.05f), "The pickaxe should lie on the ground as it is dragged.");
            Assert.That(c.pulled, Is.GreaterThan(8), "Dragging should take a pull.");
            Assert.That(c.slowest, Is.LessThan(.8f), "Dragging should slow the walk.");
            Assert.That(c.took, Is.GreaterThan(3.5f / c.pace * 1.1f), "Dragging should slow the walk.");
        }

        // A tired arm holds less: what it carried at first, it drags in the end.
        [UnityTest, Timeout(300000)]
        public IEnumerator ATiredArmDragsWhatItCarried()
        {
            Time.captureFramerate = 50;
            Carried c = default;
            // (3.4 times, since October 8: the arm that carries hangs straight now, and tires more slowly: at 2.9
            // times it carried the six metres without needing to drag.)
            yield return Carry("Long", 1, 3.4f, 6, r => c = r);
            Debug.Log($"CARRY_TIRED Long, its pickaxe 3.4 times as heavy: {c.first}, then {c.last}; walked {c.covered:F2} m in {c.took:F1} s");
            Assert.That(c.first, Is.EqualTo(PhysicalCarry.Way.OneHand), "Fresh, it should carry it.");
            Assert.That(c.last, Is.EqualTo(PhysicalCarry.Way.Dragged), "Its arm tired, it should drag it.");
            Assert.That(c.has, Is.True);
        }

        // What it cannot move, it leaves where it lies, and walks on.
        [UnityTest, Timeout(300000)]
        public IEnumerator WhatItCannotMoveItLeaves()
        {
            Time.captureFramerate = 50;
            Carried c = default;
            yield return Carry("Long", .12f, 3, 3.5f, r => c = r);
            Debug.Log($"CARRY_LEFT Long at 0.12 of its strength, its pickaxe three times as heavy: {c.first}, then {c.last}; walked {c.covered:F2} m in {c.took:F1} s; has it {c.has}; it lies {c.lies}");
            Assert.That(c.last, Is.EqualTo(PhysicalCarry.Way.Left), "It should leave what it cannot move.");
            Assert.That(c.has, Is.False);
            Assert.That(c.lies, Is.True, "The pickaxe should lie where it was left.");
            Assert.That(c.covered, Is.GreaterThan(3.2f), "Free of it, it should walk on.");
        }
    }
}
