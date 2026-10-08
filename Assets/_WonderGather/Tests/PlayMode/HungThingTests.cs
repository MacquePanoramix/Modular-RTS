using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, step 8 (second half): what hangs on a miner by a handle (Small's lantern, Long's mug) is taken in hand and
    // hung back, by the body. The orders are given as the interaction click's box gives them.
    public sealed class HungThingTests
    {
        private MinerChoice choice;
        private MinerWorkPreview look;
        private InteractionClick click;
        private OrdinaryGround ground;

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
            click = Object.FindAnyObjectByType<InteractionClick>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
        }

        [TearDown]
        public void Restore() { Time.captureFramerate = 0; }

        private Vector3 OnGround(Vector3 p) => new Vector3(p.x, ground.Height(p.x, p.z), p.z);
        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        private bool Offers(string label)
        {
            for (int i = 0; i < click.Count; i++) if (click.Label(i) == label) return true;
            return false;
        }

        private bool Shows(Component thing)
        {
            foreach (var shown in click.Things()) if (shown == thing) return true;
            return false;
        }

        // How far the thing's handle is from where it lies in the closed fingers; how far its bar is from lying along
        // them (degrees).
        private static void InFingers(MinerBody miner, int k, int hand, out float off, out float askew)
        {
            miner.HandleIn(hand, miner.Things[k].grip, out var inFingers, out var fingers);
            off = Vector3.Distance(miner.HangsFrom(k), inFingers);
            float angle = Vector3.Angle(miner.HangingBar(k), fingers);
            askew = Mathf.Min(angle, 180 - angle);
        }

        private static Vector3 HandFromHips(ProceduralBiped biped, MinerBody miner, int hand)
            => Quaternion.Inverse(biped.PostureNow) * (miner.Rig.hands[hand].position - biped.HipsNow);

        [UnityTest, Timeout(600000)]
        public IEnumerator WhatHangsByAHandleIsTakenInHandAndHungBack()
        {
            Assert.That(click, Is.Not.Null, "The Ordinary Place has no interaction click.");
            Time.captureFramerate = 50;
            int tried = 0;
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.6f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var miner = unit.GetComponent<MinerBody>();
                var biped = unit.GetComponent<ProceduralBiped>();
                var has = ThingsInHand.Of(unit);
                if (has == null)
                {
                    // Nothing on it is made to be taken: the click shows nothing of the kind.
                    foreach (var shown in click.Things()) Assert.That(shown is HungThing, Is.False, name + " shows a thing to take that it does not have.");
                    continue;
                }
                tried++;
                Assert.That(has.Count, Is.EqualTo(1));
                var thing = has.Thing(0);
                int k = thing.Index;
                var made = miner.Things[k];
                Debug.Log($"HUNG_THING {name}'s {thing.name}: handle {made.grip * 1000:F1} mm round, {made.deep * 1000:F0} mm deep, {made.wide * 1000:F0} mm to each side; at the body's side it is stopped {made.clear * 1000:F0} mm from the pelvis");
                Assert.That(made.clear, Is.InRange(.1f, .5f));
                Assert.That(made.grip, Is.InRange(.003f, .02f));
                Assert.That(made.deep, Is.InRange(.08f, .3f));
                Assert.That(made.wide, Is.InRange(.02f, .08f));

                // On its hook it shows itself to the click, and offers to be taken in hand.
                Assert.That(Shows(thing), Is.True, name + "'s " + thing.name + " does not show itself.");
                Assert.That(click.OpenOn(thing), Is.True, name + "'s " + thing.name + " offered nothing.");
                Assert.That(Offers("Take in hand"), Is.True);
                Assert.That(Offers("Hang it back"), Is.False);
                var atSide = new[] { HandFromHips(biped, miner, 0), HandFromHips(biped, miner, 1) };
                Assert.That(click.Choose("Take in hand"), Is.True);
                float began = Time.time;
                // The arm reaches the handle where it hangs without being stretched straight, and the fingers close on
                // the handle there.
                float spare = float.MaxValue, closedAt = -1;
                while (has.Now != ThingsInHand.Phase.Carried && Time.time - began < 5)
                {
                    if (has.Hand >= 0) spare = Mathf.Min(spare, biped.ArmReach - Vector3.Distance(biped.ShoulderNow(has.Hand), has.WristAsked));
                    if (closedAt < 0 && has.Now == ThingsInHand.Phase.Lifting)
                    {
                        miner.HandleIn(has.Hand, made.grip, out var closedOn, out _);
                        closedAt = Vector3.Distance(closedOn, miner.Hook(k));
                    }
                    yield return null;
                }
                Assert.That(has.Now, Is.EqualTo(ThingsInHand.Phase.Carried), name + " did not take its " + thing.name + " in hand.");
                float took = Time.time - began;
                yield return Wait(1.2f);
                int hand = has.Hand;
                InFingers(miner, k, hand, out float off, out float askew);
                float fromHook = Vector3.Distance(miner.HangsFrom(k), miner.Hook(k));
                float lean = Vector3.Angle(miner.HangingWay(k), Vector3.down);
                Vector3 carriedAt = HandFromHips(biped, miner, hand) - atSide[hand];
                Debug.Log($"HUNG_TAKEN {name}'s {thing.name}: in its {(hand == 0 ? "left" : "right")} hand {took:F2} s after the order; the fingers closed {closedAt * 1000:F1} mm from where its handle hung, the arm {spare * 1000:F0} mm short of straight at most; carried, the handle {off * 1000:F1} mm from its place in the fingers, its bar {askew:F1} degrees from lying along them; {fromHook * 1000:F0} mm from its hook; it hangs {lean:F1} degrees from straight down, {-miner.HangingIntoBody(k) * 1000:F0} mm clear of the body; the arm hangs {has.HangsOut * 1000:F0} mm further out than a free arm (the hand is {carriedAt.ToString("F3")} m from where it hung free)");
                Assert.That(spare, Is.GreaterThan(.003f), name + "'s arm is stretched straight to reach the handle.");
                Assert.That(closedAt, Is.InRange(0, .004f), name + "'s fingers did not close on the handle where it hung.");
                // (Until October 8 it was carried on a straight arm held out to the side, and this asked that the hand
                // be where it hung free. It is carried now as a lantern is: the upper arm hanging, the forearm raised
                // forward, the hand a little before the hip and higher than it hangs free.)
                var measures = biped.BodyProportions;
                Assert.That(carriedAt.y, Is.InRange(.1f * measures.forearm, .9f * measures.forearm), "Carried, the forearm should be raised, and the upper arm hang.");
                Assert.That(carriedAt.z, Is.InRange(.4f * measures.forearm, 1.05f * measures.forearm), "Carried, the hand should be before the hip.");
                Assert.That(Mathf.Abs(carriedAt.x), Is.LessThan(.06f), "Carried, the hand should be no further out than it hangs free.");
                Assert.That(has.Has, Is.EqualTo(thing));
                Assert.That(miner.InHand(k), Is.EqualTo(hand));
                Assert.That(miner.Held(hand), Is.GreaterThan(.99f), name + "'s hand is not closed on the handle.");
                Assert.That(miner.HandFree(hand), Is.False, "A hand with a thing in it is not free.");
                Assert.That(off, Is.LessThan(.004f), "The handle is not in the fingers.");
                Assert.That(askew, Is.LessThan(8), "The bar does not lie along the fingers.");
                Assert.That(fromHook, Is.GreaterThan(.03f), "It was not taken off its hook.");
                Assert.That(lean, Is.LessThan(12), "In the hand, standing, it should hang down.");
                Assert.That(miner.HangingIntoBody(k), Is.LessThan(.002f), "In the hand it is inside the body.");

                // In the hand it offers to be hung back.
                Assert.That(click.OpenOn(thing), Is.True);
                Assert.That(Offers("Hang it back"), Is.True);
                Assert.That(Offers("Take in hand"), Is.False);
                click.Close();

                // It walks with it: the handle stays in the fingers, the thing swings, and the body stops it.
                Assert.That(unit.Motor.TryMove(OnGround(unit.transform.position + unit.transform.right * 2.5f)), Is.True);
                began = Time.time;
                float worstOff = 0, worstInto = -1, mostLean = 0;
                while ((unit.Motor.IsMoving || Time.time - began < .5f) && Time.time - began < 15)
                {
                    InFingers(miner, k, hand, out off, out _);
                    worstOff = Mathf.Max(worstOff, off);
                    worstInto = Mathf.Max(worstInto, miner.HangingIntoBody(k));
                    mostLean = Mathf.Max(mostLean, Vector3.Angle(miner.HangingWay(k), Vector3.down));
                    yield return null;
                }
                yield return Wait(2);
                lean = Vector3.Angle(miner.HangingWay(k), Vector3.down);
                Debug.Log($"HUNG_WALKED {name}'s {thing.name}: walking, the handle at most {worstOff * 1000:F1} mm from the fingers; it swung to {mostLean:F1} degrees; at most {worstInto * 1000:F1} mm into the body; standing again it hangs {lean:F1} degrees from straight down");
                Assert.That(has.Has, Is.EqualTo(thing), "It did not keep the thing as it walked.");
                Assert.That(worstOff, Is.LessThan(.004f), "The handle left the fingers as it walked.");
                Assert.That(worstInto, Is.LessThan(.002f), "The thing went into the body as it walked.");
                Assert.That(mostLean, Is.GreaterThan(1.5f), "A thing carried by its handle should swing as the miner walks.");
                Assert.That(lean, Is.LessThan(12));

                // It hangs it back, and its arm hangs at its side again.
                Assert.That(click.OpenOn(thing) && click.Choose("Hang it back"), Is.True);
                began = Time.time;
                while (has.Now != ThingsInHand.Phase.Hung && Time.time - began < 5) yield return null;
                Assert.That(has.Now, Is.EqualTo(ThingsInHand.Phase.Hung), name + " did not hang its " + thing.name + " back.");
                float hung = Time.time - began;
                yield return Wait(.8f);
                fromHook = Vector3.Distance(miner.HangsFrom(k), miner.Hook(k));
                float strayed = Vector3.Distance(HandFromHips(biped, miner, hand), atSide[hand]);
                Debug.Log($"HUNG_BACK {name}'s {thing.name}: hung back {hung:F2} s after the order; {fromHook * 1000:F1} mm from its hook; the hand {strayed * 1000:F0} mm from where it hung before, closed {miner.Held(hand) * 100:F0}%; it hangs {Vector3.Angle(miner.HangingWay(k), Vector3.down):F1} degrees from straight down");
                Assert.That(has.Has, Is.Null);
                Assert.That(miner.InHand(k), Is.EqualTo(-1));
                Assert.That(fromHook, Is.LessThan(.001f), "It is not back on its hook.");
                Assert.That(miner.Held(hand), Is.LessThan(.01f), "The hand did not let go.");
                Assert.That(miner.HandFree(hand), Is.True);
                Assert.That(strayed, Is.LessThan(.03f), "The arm does not hang at the side again.");
                Assert.That(click.OpenOn(thing), Is.True);
                Assert.That(Offers("Take in hand"), Is.True);
                click.Close();
            }
            Assert.That(tried, Is.EqualTo(2), "Small's lantern and Long's mug should be things a hand can take.");
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator AMinerHangsItsThingBackBeforeItTakesUpItsPickaxe()
        {
            Time.captureFramerate = 50;
            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Small") index = i;
            Assert.That(index, Is.GreaterThanOrEqualTo(0));
            choice.Choose(index);
            yield return Wait(.6f);
            var unit = choice.Current;
            var miner = unit.GetComponent<MinerBody>();
            var has = ThingsInHand.Of(unit);
            Assert.That(has, Is.Not.Null);
            var thing = has.Thing(0);
            Assert.That(click.OpenOn(thing) && click.Choose("Take in hand"), Is.True);
            float began = Time.time;
            while (has.Now != ThingsInHand.Phase.Carried && Time.time - began < 5) yield return null;
            Assert.That(has.Has, Is.EqualTo(thing));

            // Set to work (the bench's block), with the lantern in its hand: it hangs the lantern back first, and only then takes up its pickaxe.
            look.Toggle();
            Assert.That(look.Showing, Is.False, "It took up its pickaxe with the lantern still in its hand.");
            began = Time.time;
            bool hungFirst = true;
            while (!look.Swinging && Time.time - began < 15)
            {
                if (look.Showing && has.Has != null) hungFirst = false;
                yield return null;
            }
            Assert.That(look.Swinging, Is.True, "It did not take up its pickaxe after hanging the lantern back.");
            Assert.That(hungFirst, Is.True, "The look began before the lantern hung again.");
            Assert.That(has.Has, Is.Null);
            Assert.That(miner.InHand(thing.Index), Is.EqualTo(-1));
            Debug.Log($"HUNG_THEN_WORK Small hung its lantern back and had its pickaxe in its hands {Time.time - began:F1} s after K");

            // With its pickaxe in its hands, the lantern offers nothing.
            Assert.That(click.OpenOn(thing), Is.False, "With the pickaxe in its hands the lantern should offer nothing.");
            look.Toggle();
            yield return Wait(.5f);
            Assert.That(look.Showing, Is.False);
            Assert.That(click.OpenOn(thing), Is.True);
            Assert.That(Offers("Take in hand"), Is.True);
            click.Close();
        }
    }
}
