using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3, step 9: any boulder of the Ordinary Place, by a click. A place to stand and a spot to strike are found from
    // the rock's own shape; the miner goes there (the last steps off the walked ground), strikes the rock, and
    // stones break off and lie. The orders are given as the interaction click's box gives them, and the pickaxe is put
    // on the ground as the panel puts it (step 11): none is made in a hand.
    public sealed class BoulderTests
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
        public void Restore()
        {
            Time.captureFramerate = 0;
            // (What a run with arguments may have changed is as it was for the tests after it.)
            MinerWorkPreview.RockTells = null; MinerWorkPreview.SwingsWithin = .06f;
            PhysicalSwing.KeepsFeetTakingUp = true; MinerBreath.OnEveryBody = false;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);
        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        private bool Offers(string label)
        {
            for (int i = 0; i < click.Count; i++) if (click.Label(i) == label) return true;
            return false;
        }

        private int Miner(string name)
        {
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == name) return i;
            Assert.Fail("No miner is called " + name);
            return -1;
        }

        // The miner is put on the walked ground a few steps from a boulder, on the side of the path.
        private IEnumerator PutNear(SelectableUnit unit, Boulder boulder, float away)
        {
            Vector3 centre = boulder.Rock.bounds.center;
            bool placed = false;
            for (int k = 0; k < 12 && !placed; k++)
            {
                Vector3 at = centre + Quaternion.Euler(0, k * 30, 0) * Vector3.forward * (Mathf.Max(boulder.Rock.bounds.extents.x, boulder.Rock.bounds.extents.z) + away);
                if (!NavMesh.SamplePosition(at, out var walked, 1, NavMesh.AllAreas)) continue;
                unit.Motor.Stop();
                unit.GetComponent<NavMeshAgent>().Warp(walked.position);
                unit.transform.rotation = Quaternion.LookRotation(Flat(centre - walked.position).normalized);
                unit.GetComponent<ProceduralBiped>().ResetPose();
                placed = true;
            }
            Assert.That(placed, Is.True, "No walked ground near the boulder.");
            yield return Wait(.6f);
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator EveryMinerFindsItsPlaceAtEveryBoulder()
        {
            var boulders = Boulder.All();
            Assert.That(boulders.Count, Is.EqualTo(10), "The Ordinary Place has ten boulders.");
            for (int index = 0; index < choice.Count; index++)
            {
                choice.Choose(index);
                yield return Wait(.5f);
                var unit = choice.Current;
                string name = choice.NameOf(index);
                var miner = unit.GetComponent<MinerBody>();
                var biped = unit.GetComponent<ProceduralBiped>();
                float tall = miner.Rig.head.position.y - unit.transform.position.y;
                var swing = unit.gameObject.AddComponent<PhysicalSwing>();
                swing.enabled = false; swing.body = biped; swing.tool = miner.Pickaxe;
                float lowest = float.MaxValue, highest = 0, farthestOff = 0, worstMiss = 0, slowest = 0;
                for (int b = 0; b < boulders.Count; b++)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var plan = RockWork.Find(boulders[b], swing, biped, tall, unit.transform.position);
                    watch.Stop();
                    Assert.That(plan.found, Is.True, $"{name} finds no place at boulder {b}: {RockWork.Last}");
                    float off = Flat(plan.approach - plan.stand).magnitude;
                    lowest = Mathf.Min(lowest, plan.height); highest = Mathf.Max(highest, plan.height);
                    farthestOff = Mathf.Max(farthestOff, off); worstMiss = Mathf.Max(worstMiss, plan.aimed.miss); slowest = Mathf.Max(slowest, watch.ElapsedMilliseconds);
                    Assert.That(plan.aimed.miss, Is.LessThan(.03f), $"{name}'s swing does not reach its spot on boulder {b}.");
                    Assert.That(plan.height, Is.InRange(.08f, .5f * tall + .01f), $"{name}'s spot on boulder {b} is not at a height it strikes.");
                    Assert.That(plan.away, Is.GreaterThan(.3f), $"{name} would stand on boulder {b}.");
                    Assert.That(off, Is.LessThan(UnitMotor.OffAtMost), $"{name}'s place at boulder {b} is too far off the walked ground.");
                    Assert.That(NavMesh.SamplePosition(plan.approach, out _, .05f, NavMesh.AllAreas), Is.True, "The way to its place does not begin on the walked ground.");
                    Assert.That(Vector3.Distance(plan.lands, plan.spot), Is.LessThan(.2f), $"On boulder {b} {name}'s pick would land far from its spot.");
                    Assert.That(Vector3.Angle(plan.facing * Vector3.forward, Flat(plan.spot - plan.stand)), Is.LessThan(1), "It would not face its spot.");
                }
                Debug.Log($"BOULDER_PLACES {name} ({tall:F2} m tall): a place at each of {boulders.Count} boulders; its spots from {lowest:F2} to {highest:F2} m over the ground; its places up to {farthestOff:F2} m off the walked ground; the swing misses its spot by {worstMiss * 1000:F0} mm at most; found in {slowest:F0} ms at most; holding its pickaxe at its side asks {swing.HoldAsks * 100:F0}% of its shoulder, fresh");
                Object.Destroy(swing);
                yield return null;
            }
        }

        [UnityTest, Timeout(900000)]
        public IEnumerator AMinerMinesABoulderByAClick()
        {
            Time.captureFramerate = 50;
            var boulders = Boulder.All();
            // Each miner a different boulder: a large one, a middling one, another large one.
            // (Until step 11 Small's was the lowest of the place, a dome 0.2 m high. Work at the two low domes is at
            // the edge of what the bodies can do: from one run to the next Small works there with ease, or is thrown
            // off its place by its first swings and falls. That is written up as a defect of the swing at low rock
            // (Docs/Design/ThePhysicalBody.md, step 11); this test is of mining a boulder by a click.)
            // (-breathOnAll 1: with breath drawn on every body, to look for the miss it brings on. Not the game.)
            if (CaptureTools.Argument("-breathOnAll") == "1") MinerBreath.OnEveryBody = true;
            string toldLast = "";
            MinerWorkPreview.RockTells = what => { if (toldLast == "") toldLast = what; Debug.Log("BOULDER_ROCK " + what); };
            // (-blowsAtLeast 16 -onlyWho Long: more blows, and one miner only, to look at a long stretch of work. Not the test.)
            int atLeast = int.TryParse(CaptureTools.Argument("-blowsAtLeast"), out int asked) ? asked : 4;
            string only = CaptureTools.Argument("-onlyWho");
            // (-rockStrength 0.6: every miner so strong, 1 being ordinary for its build. Not the test.)
            float strong = float.TryParse(CaptureTools.Argument("-rockStrength"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float given) ? given : -1;
            // (-rockShots <folder>: pictures from the side, each time it sets to work at its boulder, for two seconds.)
            string shotsTo = CaptureTools.Argument("-rockShots");
            RockShots shots = null;
            if (!string.IsNullOrEmpty(shotsTo))
            {
                System.IO.Directory.CreateDirectory(shotsTo);
                var grass = Object.FindAnyObjectByType<GrassField>();
                if (grass != null) grass.enabled = false;
                shots = new GameObject("RockShots").AddComponent<RockShots>();
                shots.folder = shotsTo;
                var tells = MinerWorkPreview.RockTells;
                MinerWorkPreview.RockTells = what => { tells(what); if (what.StartsWith("sets to work")) shots.Begin(); };
            }
            // (-rockAsItWas 1: without what keeps a miner's blows to its place at a boulder, to measure against.)
            if (CaptureTools.Argument("-rockAsItWas") == "1") { MinerWorkPreview.SwingsWithin = float.MaxValue; PhysicalSwing.KeepsFeetTakingUp = false; }
            if (CaptureTools.Argument("-stepsTakingUp") == "1") PhysicalSwing.KeepsFeetTakingUp = false;
            var tries = new (string who, int boulder)[] { ("Small", 1), ("Long", 3), ("Round", 4) };
            foreach (var (who, which) in tries)
            {
                if (!string.IsNullOrEmpty(only) && only != who) continue;
                choice.Choose(Miner(who));
                yield return Wait(.4f);
                if (strong > 0) look.SetStrength(strong);
                var unit = choice.Current;
                var boulder = boulders[which];
                // (-ownAsPlayed 1, the test as the game is played: it is put there as the body it was, which a test may
                // do and the game does not; then it stands by its own joints before it is told anything.)
                bool asPlayed = CaptureTools.Argument("-ownAsPlayed") == "1";
                if (asPlayed) { look.SetOwn(false); yield return Wait(.6f); }
                yield return PutNear(unit, boulder, 2.5f);
                if (asPlayed)
                {
                    look.SetOwn(true);
                    OwnBody own = null;
                    for (float from = Time.time; Time.time - from < 6 && (own == null || !own.Stands);) { unit.TryGetComponent(out own); yield return null; }
                    Assert.That(own != null && own.Stands, Is.True, who + " did not come to stand by its own joints by its boulder: " + look.Status());
                    yield return Wait(1);
                }
                var agent = unit.GetComponent<NavMeshAgent>();
                int blowsBefore = boulder.Blows, stonesBefore = boulder.Stones.Count;

                // A pickaxe is put on the ground beside it (the panel): nothing is in its hands.
                var pickaxe = look.LayPickaxe(1);
                Assert.That(pickaxe, Is.Not.Null, "No pickaxe was put on the ground beside " + who);
                yield return Wait(1);
                Assert.That(look.Showing || unit.GetComponent<PhysicalHands>() != null, Is.False, who + " has something in its hands.");

                // The boulder shows itself to the click and offers to be mined.
                bool shown = false;
                foreach (var thing in click.Things()) shown |= thing == boulder;
                Assert.That(shown, Is.True, "The boulder does not show itself.");
                Assert.That(click.OpenOn(boulder), Is.True, "The boulder offered nothing.");
                Assert.That(Offers("Mine"), Is.True);
                Assert.That(click.Choose("Mine"), Is.True);
                // With none in its hands, it goes to the one that lies and picks it up first.
                Assert.That(look.Showing && look.Carrying && look.Carry.Fetching, Is.True, who + " did not go to pick the pickaxe up.");
                float began = Time.time;
                while (look.Mining != boulder && look.Showing && Time.time - began < 25) yield return null;
                float pickedUp = Time.time - began;
                Assert.That(look.Showing && look.Carrying && look.Mining == boulder, Is.True, $"{who} did not set out for the boulder with its pickaxe ({Time.time - began:F1} s; showing {look.Showing}, carrying {look.Carrying}, fetching {look.Carry != null && look.Carry.Fetching}): {look.Status()}");
                Assert.That(unit.GetComponent<PhysicalHands>().Thing, Is.EqualTo(pickaxe), who + " does not have the pickaxe that lay beside it.");
                var plan = look.MiningPlan;
                if (shots != null) shots.Look(who, unit.transform, plan.stand, plan.spot);

                // It comes to its place (the last steps off the walked ground), turns to the spot, and sets to work.
                began = Time.time;
                while (!look.AtRock && look.Mining == boulder && Time.time - began < 30) yield return null;
                Assert.That(look.AtRock, Is.True, $"{who} did not come to its place at the boulder: {look.LeftRock}");
                float came = Time.time - began;
                float fromPlace = Flat(unit.transform.position - plan.stand).magnitude, off = Flat(plan.approach - plan.stand).magnitude;
                float turned = Vector3.Angle(unit.transform.forward, Flat(plan.spot - unit.transform.position));
                Assert.That(fromPlace, Is.LessThan(.02f), who + " does not stand at its place.");
                Assert.That(turned, Is.LessThan(2), who + " does not face its spot.");
                if (off > .05f) Assert.That(unit.Motor.IsOff, Is.True, "At a rock's foot it stands off the walked ground.");

                // It strikes the rock, and goes on until a piece breaks off (a weaker blow takes more of them).
                began = Time.time;
                while ((boulder.Blows - blowsBefore < atLeast || boulder.Stones.Count == stonesBefore) && look.Mining == boulder && Time.time - began < 90 + (atLeast - 4) * 10) yield return null;
                Assert.That(boulder.Blows - blowsBefore, Is.GreaterThanOrEqualTo(4), $"{who} did not strike the boulder four times: {look.LeftRock} (at its rock {look.AtRock}; swinging {look.Swinging}, {(look.Swing != null ? look.Swing.phase.ToString() : "no swing")}): {look.Status()}");
                float took = Time.time - began;
                int blowsTaken = boulder.Blows - blowsBefore;
                yield return Wait(1.5f);
                float farthest = 0;
                string each = "";
                for (int i = blowsBefore; i < boulder.Struck.Count; i++)
                {
                    Vector3 from = boulder.Struck[i] - plan.lands;
                    farthest = Mathf.Max(farthest, from.magnitude);
                    // (Each blow: how far from where the plan put it; of that, up, and along the way the miner faces.)
                    each += $" {from.magnitude * 1000:F0}({from.y * 1000:F0} up, {Vector3.Dot(from, unit.transform.forward) * 1000:F0} on, {Vector3.Dot(from, unit.transform.right) * 1000:F0} right)";
                }
                Debug.Log($"BOULDER_BLOWS {who}:{each}");
                int stones = boulder.Stones.Count - stonesBefore;
                string lying = "";
                for (int i = stonesBefore; i < boulder.Stones.Count; i++)
                {
                    var stone = boulder.Stones[i];
                    float over = stone.position.y - ground.Height(stone.position.x, stone.position.z);
                    lying += $" [{stone.mass:F2} kg, {Vector3.Distance(stone.position, plan.spot):F2} m from the spot, {over:F2} m over the ground, {stone.linearVelocity.magnitude:F2} m/s]";
                    Assert.That(stone.mass, Is.InRange(.2f, 4f));
                    Assert.That(stone.linearVelocity.magnitude, Is.LessThan(.15f), "A stone struck off should come to lie.");
                    Assert.That(Vector3.Distance(stone.position, plan.spot), Is.LessThan(2.5f), "A stone flew far.");
                    Assert.That(over, Is.InRange(-.05f, 1.2f), "A stone is not on the ground or the rock.");
                }
                var swing = look.Swing;
                string told = "";
                foreach (var r in swing.results) told += $" [{(r.struck ? "struck" : "did not strike")} {Vector3.Distance(r.landed, plan.lands) * 1000:F0} mm off, moved {Flat(r.stood - r.droveFrom).magnitude * 1000:F0} mm as the blow came down, turned {r.rolled:F0} deg round its handle, hands at {r.heldLow * 1000:F0} and {r.heldHigh * 1000:F0} mm, after {r.afterRests} rests; it stood {Flat(r.stood - plan.stand).magnitude * 1000:F0} mm from its place ({Vector3.Dot(r.stood - plan.stand, Flat(plan.spot - plan.stand).normalized) * 1000:F0} towards the rock) facing {Vector3.SignedAngle(Flat(plan.spot - plan.stand), Flat(r.faced), Vector3.up):F0} deg off]";
                Debug.Log($"BOULDER_SWINGS {who}:{told}");
                Debug.Log($"BOULDER_MINED {who} picked the pickaxe up {pickedUp:F1} s after the order; at boulder {which}: its spot {plan.height:F2} m over the ground, its place {plan.away:F2} m from it and {off:F2} m off the walked ground; there {came:F1} s after the order, {fromPlace * 1000:F0} mm from its place, facing its spot within {turned:F1} degrees; {blowsTaken} blows in {took:F1} s, landing at most {farthest * 1000:F0} mm from where the plan put them; {stones} stone(s) off it:{lying}");
                Assert.That(farthest, Is.LessThan(.2f), who + "'s blows land far from where the plan put them.");
                Assert.That(stones, Is.GreaterThanOrEqualTo(1), "Its blows should have broken a piece off by now.");
                int struck = 0;
                foreach (var result in swing.results) if (result.struck) struck++;
                Assert.That(struck, Is.GreaterThanOrEqualTo(3), who + " swung and did not strike.");

                // Told to rest, it stands at ease at its boulder, which is still its own; told to go back, it works on.
                if (look.Swing != null && look.Swing.phase == PhysicalSwing.Phase.Rest)
                {
                    began = Time.time;
                    while (look.Swing != null && look.Swing.phase == PhysicalSwing.Phase.Rest && Time.time - began < 60) yield return null;
                }
                // (Between two blows it may have taken a step to keep its feet, and be on its way back to its place:
                // it is offered a rest once it is at its work again.)
                for (float until = Time.time + 12; Time.time < until && look.Mining == boulder && !look.Swinging;) yield return null;
                Assert.That(click.OpenOn(unit) && click.Choose("Rest"), Is.True, $"{who} was not offered a rest at its boulder (its boulder {(look.Mining == boulder ? "is still its own" : "was given up: " + look.LeftRock)}; carrying {look.Carrying}; at work {look.Swinging}; the panel: {look.Status()}).");
                yield return Wait(2.5f);
                Assert.That(look.Carrying && look.Mining == boulder && !look.Swinging, Is.True, who + " did not rest at its boulder.");
                Assert.That(click.OpenOn(unit), Is.True, who + ", resting at its boulder, offered nothing.");
                Assert.That(Offers("Back to work"), Is.True);
                int blowsAtRest = boulder.Blows;
                Assert.That(click.Choose("Back to work"), Is.True);
                began = Time.time;
                // (Back at its work, a body its blows have spent may first rest from them of its own accord: that is
                // its work too. The wait is for its next blow, not counting such a rest. October 8: Long, in one run
                // of the whole suite, rested so for longer than the forty seconds this waited, and the failure then
                // gave a reason left over from another miner.)
                float restedThere = 0;
                while (boulder.Blows == blowsAtRest && look.Mining == boulder && Time.time - began - restedThere < 40 && Time.time - began < 240)
                {
                    if (look.Swinging && look.Swing.phase == PhysicalSwing.Phase.Rest) restedThere += Time.deltaTime;
                    yield return null;
                }
                Assert.That(look.Mining, Is.EqualTo(boulder), $"{who} gave its boulder up when it was told to go back to its work: {look.LeftRock}");
                Assert.That(boulder.Blows, Is.GreaterThan(blowsAtRest), $"{who} did not strike again when it was told to go back to its work ({Time.time - began:F0} s, {restedThere:F0} s of them resting of its own accord)");
                Debug.Log($"BOULDER_BACK {who} rested at its boulder when told, and struck it again {Time.time - began:F1} s after it was told to go back ({restedThere:F1} s of that resting of its own accord)");

                // Taken off its place, it steps back to it before it swings again, wherever it could reach from.
                // (Here it is carried a hand's length and a half towards the rock as a blow ends, as a step to catch
                // itself carries it: from there its spot is in reach. Its own step took a tall miner there, one time
                // in nine that it took its pickaxe up at a low boulder, and its blow from there landed 30 cm off.
                // October 10.)
                began = Time.time;
                while (look.Mining == boulder && !(look.Swinging && look.Swing.phase == PhysicalSwing.Phase.Struck) && Time.time - began < 240) yield return null;
                Assert.That(look.Mining == boulder && look.Swinging && look.Swing.phase == PhysicalSwing.Phase.Struck, Is.True, $"{who} did not strike again: {look.LeftRock}");
                int swungBefore = look.Swing.results.Count;
                began = Time.time; toldLast = "";
                // (Off the walked ground nothing of its walk moves it: it stays where it is carried.)
                Assert.That(unit.Motor.IsOff, Is.True, who + " is not off the walked ground at its boulder: it cannot be carried.");
                Vector3 towards = Flat(plan.spot - plan.stand).normalized;
                // (To a hand's length and a half nearer than its place, wherever the blow left it: the blow itself
                // may have taken it as far back.)
                for (int k = 0; k < 100; k++)
                {
                    Vector3 left = Flat(plan.stand + towards * .15f - unit.transform.position);
                    if (left.magnitude < .001f) break;
                    unit.transform.position += Vector3.ClampMagnitude(left, .6f * Time.deltaTime);
                    yield return null;
                }
                float wasNearer = Vector3.Dot(unit.transform.position - plan.stand, Flat(plan.spot - plan.stand).normalized);
                Assert.That(wasNearer, Is.GreaterThan(.12f), who + " was not carried nearer the rock.");
                // (The blow that was ending is told when the tool is at rest again; the next one is the one begun after.)
                while (look.Mining == boulder && (look.Swing == null || look.Swing.results.Count < swungBefore + 2) && Time.time - began < 240) yield return null;
                Assert.That(look.Mining, Is.EqualTo(boulder), $"{who}, taken off its place, gave its boulder up: {look.LeftRock}");
                Assert.That(look.Swing.results.Count, Is.GreaterThanOrEqualTo(swungBefore + 2), who + ", taken off its place, did not swing again.");
                var after = look.Swing.results[swungBefore + 1];
                float beganFrom = Flat(after.droveFrom - plan.stand).magnitude;
                Debug.Log($"BOULDER_PLACE {who}, carried {wasNearer * 1000:F0} mm nearer the rock as a blow ended (it {toldLast}), brought its next blow down from {beganFrom * 1000:F0} mm from its place, {Time.time - began:F1} s after; it {(after.struck ? "struck " + (Vector3.Distance(after.landed, plan.lands) * 1000).ToString("F0") + " mm from where the plan put it" : "did not strike")}");
                Assert.That(beganFrom, Is.LessThan(.06f), who + " swung from where it was taken to, not from its place.");
                Assert.That(after.struck, Is.True, who + ", back at its place, did not strike.");

                // Sent somewhere, it comes back to the walked ground, and walks there with its pickaxe.
                Vector3 to = plan.approach + Flat(plan.approach - boulder.Rock.bounds.center).normalized * 3;
                to.y = ground.Height(to.x, to.z);
                Assert.That(unit.Motor.TryMove(to), Is.True, who + " would not leave the boulder.");
                began = Time.time;
                while (unit.Motor.IsMoving && Time.time - began < 20) yield return null;
                yield return Wait(.3f);
                Assert.That(unit.Motor.IsMoving, Is.False, who + " did not arrive.");
                Assert.That(unit.Motor.IsOff, Is.False, who + " is still off the walked ground.");
                Assert.That(agent.isActiveAndEnabled && agent.isOnNavMesh, Is.True);
                Assert.That(Flat(unit.transform.position - to).magnitude, Is.LessThan(.6f), who + " did not go where it was sent.");
                Assert.That(look.Mining, Is.Null, "Sent somewhere, it is no longer mining.");
                Assert.That(look.Carrying, Is.True, who + " did not take its pickaxe along.");
                look.End();
                yield return Wait(.4f);
            }
        }

        // The place's lowest boulders are domes a fifth of a metre high. To strike one, a tall body bends its knees a
        // quarter of a metre and more, and the blows would ask them more than they have (Long's: 118 to 181%; it fell).
        // Bent to its work there, before its first blow, its knees are read. Asked too much, it looks for a place at the
        // same rock where it need not bend them so deep; and if there is none, it does not work there, stands up with
        // its pickaxe, and the panel says why. Either way it does not strike with its knees asked too much, and does
        // not fall.
        [UnityTest, Timeout(900000)]
        public IEnumerator ABodyDoesNotWorkWhereItsKneesWouldBeBentTooDeep()
        {
            Time.captureFramerate = 50;
            var boulders = Boulder.All();
            // The lowest of the place: its top nearest the ground.
            int low = 0;
            float lowest = float.MaxValue;
            for (int i = 0; i < boulders.Count; i++)
            {
                var bounds = boulders[i].Rock.bounds;
                float top = bounds.max.y - ground.Height(bounds.center.x, bounds.center.z);
                if (top < lowest) { lowest = top; low = i; }
            }
            choice.Choose(Miner("Long"));
            yield return Wait(.4f);
            var unit = choice.Current;
            yield return PutNear(unit, boulders[low], 2.5f);
            var pickaxe = look.LayPickaxe(1);
            Assert.That(pickaxe, Is.Not.Null);
            yield return Wait(1);
            Assert.That(click.OpenOn(boulders[low]) && click.Choose("Mine"), Is.True);
            float began = Time.time, firstAsked = -1, askedAtFirstBlow = -1;
            bool cameThere = false;
            int places = 0;
            Vector3 lastStand = Vector3.positiveInfinity;
            while (Time.time - began < 120)
            {
                var legs = unit.GetComponent<PhysicalBalance>();
                var hands = unit.GetComponent<PhysicalHands>();
                if (look.AtRock)
                {
                    cameThere = true;
                    if ((look.MiningPlan.stand - lastStand).sqrMagnitude > .01f) { places++; lastStand = look.MiningPlan.stand; }
                    if (legs != null && look.Swinging && look.Swing.phase == PhysicalSwing.Phase.Ready)
                    {
                        float asked = legs.KneesAsked(hands != null ? hands.ToolMass : 0);
                        if (firstAsked < 0 && asked > .05f) firstAsked = asked;
                        if (boulders[low].Blows == 0) askedAtFirstBlow = asked;
                    }
                }
                if (cameThere && look.Mining == null) break;
                if (boulders[low].Blows >= 3) break;
                yield return null;
            }
            float took = Time.time - began;
            var fall = unit.GetComponent<PhysicalFall>();
            bool left = look.Mining == null;
            Debug.Log($"BOULDER_LOW Long at boulder {low} (its top {lowest:F2} m over the ground): bent to its work standing at {places} place(s); at first each knee was asked {firstAsked * 100:F0}% of what it has (holding half); "
                + (left ? $"it left the rock {took:F1} s after the order with {boulders[low].Blows} blows struck: {look.LeftRock}. The panel says: {look.Status().Replace("\n", " / ")}"
                        : $"it found a way to stand to it where they are asked {askedAtFirstBlow * 100:F0}%, and struck the rock {boulders[low].Blows} times within {took:F1} s"));
            Assert.That(cameThere, Is.True, "Long did not come to its place at the lowest boulder: " + look.LeftRock);
            Assert.That(fall == null || fall.Falls == 0, Is.True, "Long fell.");
            Assert.That(firstAsked, Is.GreaterThan(PhysicalBalance.WorkRaises), "At the lowest boulder, from where it came, its knees were not asked too much: this is not the case the test is for.");
            if (left)
            {
                Assert.That(look.LeftRock, Does.Contain("knees"), "It left the boulder for another reason: " + look.LeftRock);
                Assert.That(look.Status(), Does.Contain("knees"), "The panel does not say why it left the boulder.");
                Assert.That(boulders[low].Blows, Is.EqualTo(0), "It struck the boulder with its knees asked too much.");
                yield return Wait(2.5f);
                Assert.That(look.Showing && look.Carrying && unit.GetComponent<PhysicalHands>().Thing == pickaxe, Is.True, "It did not stand up with its pickaxe.");
                Assert.That(unit.GetComponent<ProceduralBiped>().SinkNow, Is.LessThan(.03f), "It did not stand up.");
            }
            else
            {
                // (The other place may be the same spot, taken with its knees bent less and its back bowed more.)
                Assert.That(askedAtFirstBlow, Is.LessThan(PhysicalBalance.WorkRaises + .05f), "It struck the boulder with its knees asked too much.");
                Assert.That(askedAtFirstBlow, Is.LessThan(firstAsked), "It struck the boulder as it first stood to it.");
            }
            look.End();
            look.ClearLaid();
        }

        // A piece lying where the pick lands would stop every blow after it. A blow that lands on one knocks it aside.
        [UnityTest, Timeout(120000)]
        public IEnumerator ALoosePieceThatIsStruckIsKnockedAside()
        {
            var boulder = Boulder.All()[4];
            var bounds = boulder.Rock.bounds;
            Assert.That(boulder.Rock.Raycast(new Ray(new Vector3(bounds.center.x, bounds.max.y + 1, bounds.center.z), Vector3.down), out var top, bounds.size.y + 2), Is.True);
            // Less than a piece costs: nothing comes off. Then enough: one does, and lies.
            Assert.That(boulder.Strike(top.point, top.normal, Boulder.Breaks * .6f), Is.Null);
            var stone = boulder.Strike(top.point, top.normal, Boulder.Breaks * .6f);
            Assert.That(stone, Is.Not.Null, "The rock had taken what a piece costs.");
            Assert.That(boulder.Blows, Is.EqualTo(2));
            Assert.That(boulder.Taken, Is.EqualTo(Boulder.Breaks * .2f).Within(.01f), "What a blow gives beyond a piece's cost counts towards the next.");
            yield return Wait(2.5f);
            Assert.That(stone.linearVelocity.magnitude, Is.LessThan(.15f), "The piece did not come to lie.");
            Vector3 lay = stone.position;
            var piece = stone.GetComponent<LooseStone>();
            piece.Knocked(top.normal, 20);
            yield return Wait(2.5f);
            float moved = Vector3.Distance(stone.position, lay);
            Debug.Log($"BOULDER_PIECE a piece of {stone.mass:F2} kg, struck with 20 J where it lay, went {moved:F2} m and lies again (moving at {stone.linearVelocity.magnitude:F2} m/s)");
            Assert.That(piece.Knocks, Is.EqualTo(1));
            Assert.That(moved, Is.GreaterThan(.2f), "A struck piece should be knocked aside.");
            Assert.That(moved, Is.LessThan(4), "A struck piece flew far.");
            Assert.That(stone.linearVelocity.magnitude, Is.LessThan(.15f), "The knocked piece did not come to lie.");
        }

        [UnityTest, Timeout(600000)]
        public IEnumerator ToMineItHangsItsLanternBackAndPicksItsPickaxeUp()
        {
            Time.captureFramerate = 50;
            var boulders = Boulder.All();
            var boulder = boulders[1];
            choice.Choose(Miner("Small"));
            yield return Wait(.4f);
            var unit = choice.Current;
            yield return PutNear(unit, boulder, 2.5f);
            // A pickaxe lies on the ground beside it, and its lantern is in its hand.
            var pickaxe = look.LayPickaxe(1);
            Assert.That(pickaxe, Is.Not.Null, "No pickaxe was put on the ground.");
            yield return Wait(1);
            Assert.That(look.Showing, Is.False);
            float began;
            var has = ThingsInHand.Of(unit);
            var lantern = has.Thing(0);
            Assert.That(click.OpenOn(lantern) && click.Choose("Take in hand"), Is.True);
            began = Time.time;
            while (has.Now != ThingsInHand.Phase.Carried && Time.time - began < 5) yield return null;
            Assert.That(has.Has, Is.EqualTo(lantern));

            // Told to mine: it hangs the lantern back, picks its pickaxe up, goes to the boulder, and strikes it.
            int blows = boulder.Blows;
            Assert.That(click.OpenOn(boulder) && click.Choose("Mine"), Is.True);
            began = Time.time;
            bool withLantern = false;
            while (boulder.Blows == blows && Time.time - began < 60)
            {
                if (look.Showing && has.Has != null) withLantern = true;
                yield return null;
            }
            Assert.That(boulder.Blows, Is.GreaterThan(blows), $"Small did not come to strike the boulder: {look.LeftRock}");
            Assert.That(withLantern, Is.False, "It took its pickaxe with the lantern still in its hand.");
            Assert.That(has.Has, Is.Null, "The lantern is not back on its hook.");
            Assert.That(look.Lying, Is.Null, "Its pickaxe still lies on the ground.");
            Assert.That(unit.GetComponent<PhysicalHands>().Thing, Is.EqualTo(pickaxe), "It did not strike with the pickaxe that lay beside it.");
            Debug.Log($"BOULDER_CHAIN Small hung its lantern back, picked its pickaxe up, went to the boulder and struck it {Time.time - began:F1} s after the order");
        }
    }

    // Pictures of a miner at its boulder from the side, for whoever looks at how it takes its pickaxe up there.
    public sealed class RockShots : MonoBehaviour
    {
        public string folder;
        private Camera view;
        private string who;
        private Transform body;
        private float until = -1;
        private int taken, shot, frame;

        public void Look(string name, Transform miner, Vector3 stand, Vector3 spot)
        {
            who = name.ToLowerInvariant(); body = miner; taken = 0; until = -1;
            if (view == null)
            {
                view = new GameObject("RockShotsView").AddComponent<Camera>();
                view.CopyFrom(Camera.main);
                view.enabled = false;
            }
            Vector3 towards = Vector3.ProjectOnPlane(spot - stand, Vector3.up).normalized, aside = Vector3.Cross(Vector3.up, towards);
            Vector3 middle = stand + towards * .3f + Vector3.up * .8f;
            view.transform.position = middle + aside * 4.2f + Vector3.up * .2f;
            view.transform.rotation = Quaternion.LookRotation(middle - view.transform.position);
            view.fieldOfView = 30; view.nearClipPlane = .05f;
        }

        public void Begin() { if (body == null) return; taken++; shot = 0; frame = 0; until = Time.time + 2; }

        private void LateUpdate()
        {
            if (view == null || Time.time > until) return;
            if (frame++ % 4 == 0) CaptureTools.Render(view, System.IO.Path.Combine(folder, $"{who}_{taken}_{shot++:000}"), 480, 480);
        }
    }
}
