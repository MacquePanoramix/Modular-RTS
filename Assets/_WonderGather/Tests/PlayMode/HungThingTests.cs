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
            CaptureTools.AsItWas();
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
            click = Object.FindAnyObjectByType<InteractionClick>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
        }

        [TearDown]
        public void Restore() { Time.captureFramerate = 0; MinerBody.HungAsBefore = MinerBody.HungAsOn15 = false; }

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
                float worstOff = 0, worstInto = -1, mostLean = 0, handBack = float.MaxValue, handAhead = float.MinValue;
                while ((unit.Motor.IsMoving || Time.time - began < .5f) && Time.time - began < 15)
                {
                    // (How far ahead of the hips the carrying hand is: it goes with the walk.)
                    float ahead = HandFromHips(biped, miner, hand).z;
                    handBack = Mathf.Min(handBack, ahead); handAhead = Mathf.Max(handAhead, ahead);
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
                // (Until October 9 the hand that carried was held at one place before the hip while the body walked.)
                Debug.Log($"HUNG_WALKED {name}'s {thing.name}: walking, the hand that carries it went {(handAhead - handBack) * 1000:F0} mm forwards and back with the walk");
                Assert.That(handAhead - handBack, Is.InRange(.012f, .3f), "The hand that carries should go with the walk, a little: it is not held still.");
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

        // October 15: what hangs on a hook is stopped by the body however the body is turned. Small, pushed over onto
        // its front, its back, its left and its right, falls, lies, and gets up its fall's own way (curled, on hands
        // and knees). Read from the skin itself (ClothesDepth: how far the satchel's own points are under the clothes
        // of the hips, the thighs and the coat's skirt), never from how the game stops it. Until that day the swing's
        // limit had the last word and the stop turned only with the hips: bowed, the satchel went through the coat's
        // skirt (the judges of the getting up, finding 4), lying, into the hips. Then (one run each): through it in 23
        // of 170 looks onto its front, 17 of 128 onto its back, 25 of 149 onto its left; since, 1, 8 and 4. Pushed
        // onto its right side it lies on its satchel, which is caught under its hip and pressed into the ground: 50 of
        // 121, as before (52 of 134); that is told and not held, as it is not mended.
        // (-hungAsBefore 1: as it was, to see the test fail.)
        [UnityTest, Timeout(900000)]
        public IEnumerator WhatHangsOnAHookStaysOutOfTheCoatHoweverTheBodyIsTurned()
        {
            Time.captureFramerate = 50;
            MinerBody.HungAsBefore = CaptureTools.Argument("-hungAsBefore") == "1";
            try
            {
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Small") index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0));
                choice.Choose(index);
                yield return Wait(.4f);
                var unit = choice.Current;
                var miner = unit.GetComponent<MinerBody>();
                int satchel = -1;
                for (int k = 0; k < miner.Things.Count; k++) if (miner.Things[k].bone != null && miner.Things[k].bone.name == "Satchel") satchel = k;
                Assert.That(satchel, Is.GreaterThanOrEqualTo(0), "Small has no satchel.");
                var a = ground.Path[4];
                var b = ground.Path[5];
                var spot = OnGround(new Vector3(a.x, 0, a.y));
                var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
                away.y = 0;
                away.Normalize();
                foreach (string way in new[] { "front", "back", "left", "right" })
                {
                    unit.Motor.Stop();
                    unit.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    unit.GetComponent<ProceduralBiped>().ResetPose();
                    yield return Wait(1.2f);
                    var depth = new ClothesDepth(miner, satchel) { Stride = 3 };
                    int frame = 0, read = 0, through = 0;
                    float worst = 0;
                    var after = new GameObject("After everything").AddComponent<AfterEverything>();
                    after.Then = () =>
                    {
                        if (frame++ % 4 != 0) return;
                        depth.HangsFrom = miner.Things[satchel].bone.position;
                        depth.HangsWay = (miner.HangingWeight(satchel) - depth.HangsFrom).normalized;
                        float deep = depth.Deepest(.02f, out int under, out _, null, out _);
                        read++;
                        // Through the coat: a tenth or more of the satchel's points read, two centimetres or more under it.
                        if (under * 10 >= depth.Points / depth.Stride) through++;
                        worst = Mathf.Max(worst, deep);
                    };
                    look.PushOver(way == "front" ? unit.transform.forward : way == "back" ? -unit.transform.forward : way == "left" ? -unit.transform.right : unit.transform.right);
                    yield return null;
                    var fall = unit.GetComponent<PhysicalFall>();
                    Assert.That(fall, Is.Not.Null);
                    float began = Time.time;
                    while (fall.Now != PhysicalFall.State.Up && Time.time - began < 40) yield return null;
                    yield return Wait(1);
                    after.Then = null;
                    Object.Destroy(after.gameObject);
                    Debug.Log($"HUNG_TURNED Small pushed over onto its {way}: the satchel through its clothes in {through} of {read} looks (a tenth of it or more, 2 cm or more under them); its deepest point {worst * 1000:F0} mm under them; {(fall.Now == PhysicalFall.State.Up ? "it got up" : "it did not get up")}");
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), "Small did not get up.");
                    if (way != "right")
                        Assert.That(through * 10, Is.LessThanOrEqualTo(read), $"Pushed over onto its {way}, Small's satchel went through its clothes in {through} of {read} looks.");
                }
            }
            finally
            {
                MinerBody.HungAsBefore = false;
            }
        }

        // October 20: what hangs on a hook lies on the ground, not in it. Each miner pushed over onto its front and onto
        // the side its things hang on, falls, lies and gets up its fall's way; each thing's own skin is read against the
        // ground as it is drawn (its collider), never from how the game stops it. Until that day nothing stopped a hung
        // thing at the ground: face down with the knees tucked to get up, Small's lantern went 176 mm under it, Round's
        // hammer 146, Long's mug 79, Small's satchel 76 (one course each way). Since, over four courses each way, at most
        // 20 mm, in a frame or two, and 13 to 16 mm where the lantern's hook is itself pressed into the ground.
        // (-hungAsOn15 1: as it was, to see the test fail.)
        [UnityTest, Timeout(1800000)]
        public IEnumerator WhatHangsOnAHookLiesOnTheGroundNotInIt()
        {
            Time.captureFramerate = 50;
            MinerBody.HungAsOn15 = CaptureTools.Argument("-hungAsOn15") == "1";
            var a = ground.Path[4];
            var b = ground.Path[5];
            var spot = OnGround(new Vector3(a.x, 0, a.y));
            var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
            away.y = 0;
            away.Normalize();
            float Drawn(float x, float z) => HungThingBench.Drawn(ground, x, z);
            var told = new System.Text.StringBuilder();
            float worst = 0;
            int looks = 0, deep = 0;
            foreach (var (who, ways) in new[] { ("Small", new[] { "front", "left", "right" }), ("Long", new[] { "front", "left" }), ("Round", new[] { "front", "right" }) })
            {
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == who) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), who);
                choice.Choose(index);
                yield return Wait(.4f);
                var unit = choice.Current;
                var miner = unit.GetComponent<MinerBody>();
                foreach (string way in ways)
                {
                    unit.Motor.Stop();
                    unit.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(spot);
                    unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                    unit.GetComponent<ProceduralBiped>().ResetPose();
                    yield return Wait(1.2f);
                    var depths = new System.Collections.Generic.List<(int k, ClothesDepth d)>();
                    for (int k = 0; k < miner.Things.Count; k++)
                        if (miner.Things[k].bone != null && miner.Things[k].hand == null) depths.Add((k, new ClothesDepth(miner, k) { Stride = 3 }));
                    var worstHere = new float[miner.Things.Count];
                    int frame = 0;
                    var after = new GameObject("After everything").AddComponent<AfterEverything>();
                    after.Then = () =>
                    {
                        if (frame++ % 4 != 0) return;
                        foreach (var (k, d) in depths)
                        {
                            d.Deepest(1, out _, out _, Drawn, out float below);
                            looks++;
                            if (below > .01f) deep++;
                            worstHere[k] = Mathf.Max(worstHere[k], below);
                            worst = Mathf.Max(worst, below);
                        }
                    };
                    look.PushOver(way == "front" ? unit.transform.forward : way == "left" ? -unit.transform.right : unit.transform.right);
                    yield return null;
                    var fall = unit.GetComponent<PhysicalFall>();
                    Assert.That(fall, Is.Not.Null);
                    float began = Time.time;
                    while (fall.Now != PhysicalFall.State.Up && Time.time - began < 40) yield return null;
                    yield return Wait(1);
                    after.Then = null;
                    Object.Destroy(after.gameObject);
                    foreach (var (k, d) in depths) told.Append($" {who} onto its {way}: its {miner.Things[k].bone.name} {worstHere[k] * 1000:F0} mm;");
                    Assert.That(fall.Now, Is.EqualTo(PhysicalFall.State.Up), who + " did not get up.");
                }
            }
            Debug.Log($"HUNG_GROUND deepest below the ground:{told} in all {worst * 1000:F0} mm; more than 10 mm under it in {deep} of {looks} looks");
            Assert.That(worst, Is.LessThan(.035f), "What hangs on a hook went into the ground:" + told);
            Assert.That(deep * 20, Is.LessThanOrEqualTo(looks), $"What hangs on a hook was more than 10 mm under the ground in {deep} of {looks} looks.");
        }

        // Standing and walking, what hangs on a hook hangs and swings as it did before October 15: the body's last word
        // (the stops past what a stride does) moves it no more than a few millimetres, if at all.
        [UnityTest, Timeout(600000)]
        public IEnumerator StandingAndWalkingWhatHangsIsAsItWas()
        {
            Time.captureFramerate = 50;
            var a = ground.Path[4];
            var b = ground.Path[5];
            var spot = OnGround(new Vector3(a.x, 0, a.y));
            var away = OnGround(new Vector3(b.x, 0, b.y)) - spot;
            away.y = 0;
            away.Normalize();
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.4f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                unit.Motor.Stop();
                unit.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(spot);
                unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                unit.GetComponent<ProceduralBiped>().ResetPose();
                yield return Wait(1.5f);
                MinerBody.HungMovedMost = 0;
                yield return Wait(2);
                float standing = MinerBody.HungMovedMost;
                MinerBody.HungMovedMost = 0;
                Assert.That(unit.Motor.TryMove(OnGround(spot + away * 6)), Is.True);
                float began = Time.time;
                while ((unit.Motor.IsMoving || Time.time - began < .5f) && Time.time - began < 12) yield return null;
                Assert.That(unit.Motor.TryMove(OnGround(spot - away)), Is.True);
                began = Time.time;
                while ((unit.Motor.IsMoving || Time.time - began < .5f) && Time.time - began < 14) yield return null;
                yield return Wait(1);
                float walking = MinerBody.HungMovedMost;
                Debug.Log($"HUNG_AS_IT_WAS {name}: the body's last word moved what hangs on it {standing * 1000:F1} mm standing, {walking * 1000:F1} mm walking there and back");
                Assert.That(standing, Is.LessThan(.001f), name + ": standing, what hangs on it is held otherwise than it was.");
                Assert.That(walking, Is.LessThan(.005f), name + ": walking, what hangs on it is held otherwise than it was.");
            }
        }
    }
}
