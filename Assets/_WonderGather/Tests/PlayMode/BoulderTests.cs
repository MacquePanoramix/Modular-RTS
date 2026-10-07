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
    // stones break off and lie. The orders are given as the interaction click's box gives them.
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
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
            click = Object.FindAnyObjectByType<InteractionClick>();
            ground = Object.FindAnyObjectByType<OrdinaryGround>();
        }

        [TearDown]
        public void Restore() { Time.captureFramerate = 0; }

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
            // Each miner a different boulder: a small low one, a middling one, a large one.
            var tries = new (string who, int boulder)[] { ("Small", 0), ("Long", 3), ("Round", 4) };
            foreach (var (who, which) in tries)
            {
                choice.Choose(Miner(who));
                yield return Wait(.4f);
                var unit = choice.Current;
                var boulder = boulders[which];
                yield return PutNear(unit, boulder, 2.5f);
                var agent = unit.GetComponent<NavMeshAgent>();
                int blowsBefore = boulder.Blows, stonesBefore = boulder.Stones.Count;

                // The boulder shows itself to the click and offers to be mined.
                bool shown = false;
                foreach (var thing in click.Things()) shown |= thing == boulder;
                Assert.That(shown, Is.True, "The boulder does not show itself.");
                Assert.That(click.OpenOn(boulder), Is.True, "The boulder offered nothing.");
                Assert.That(Offers("Mine"), Is.True);
                Assert.That(click.Choose("Mine"), Is.True);
                Assert.That(look.Showing && look.Carrying && look.Mining == boulder, Is.True, who + " did not set out for the boulder with its pickaxe.");
                var plan = look.MiningPlan;

                // It comes to its place (the last steps off the walked ground), turns to the spot, and sets to work.
                float began = Time.time;
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
                while ((boulder.Blows - blowsBefore < 4 || boulder.Stones.Count == stonesBefore) && look.Mining == boulder && Time.time - began < 90) yield return null;
                Assert.That(boulder.Blows - blowsBefore, Is.GreaterThanOrEqualTo(4), $"{who} did not strike the boulder four times: {look.LeftRock}");
                float took = Time.time - began;
                int blowsTaken = boulder.Blows - blowsBefore;
                yield return Wait(1.5f);
                float farthest = 0;
                for (int i = blowsBefore; i < boulder.Struck.Count; i++) farthest = Mathf.Max(farthest, Vector3.Distance(boulder.Struck[i], plan.lands));
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
                Debug.Log($"BOULDER_MINED {who} at boulder {which}: its spot {plan.height:F2} m over the ground, its place {plan.away:F2} m from it and {off:F2} m off the walked ground; there {came:F1} s after the order, {fromPlace * 1000:F0} mm from its place, facing its spot within {turned:F1} degrees; {blowsTaken} blows in {took:F1} s, landing at most {farthest * 1000:F0} mm from where the plan put them; {stones} stone(s) off it:{lying}");
                Assert.That(farthest, Is.LessThan(.2f), who + "'s blows land far from where the plan put them.");
                Assert.That(stones, Is.GreaterThanOrEqualTo(1), "Its blows should have broken a piece off by now.");
                int struck = 0;
                foreach (var result in swing.results) if (result.struck) struck++;
                Assert.That(struck, Is.GreaterThanOrEqualTo(3), who + " swung and did not strike.");

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
            var boulder = boulders[0];
            choice.Choose(Miner("Small"));
            yield return Wait(.4f);
            var unit = choice.Current;
            yield return PutNear(unit, boulder, 2.5f);
            // Its pickaxe lies on the ground, and its lantern is in its hand.
            look.Toggle();
            float began = Time.time;
            while (!look.Swinging && Time.time - began < 8) yield return null;
            Assert.That(look.Swinging, Is.True);
            var pickaxe = unit.GetComponent<PhysicalHands>().Thing;
            Assert.That(click.OpenOn(pickaxe) && click.Choose("Lay it down"), Is.True);
            began = Time.time;
            while (look.Showing && Time.time - began < 15) yield return null;
            Assert.That(!look.Showing && look.Lying == pickaxe, Is.True, "The pickaxe was not laid down.");
            yield return Wait(.6f);
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
            Assert.That(unit.GetComponent<PhysicalHands>().Thing, Is.EqualTo(pickaxe), "It did not strike with the pickaxe it had laid down.");
            Debug.Log($"BOULDER_CHAIN Small hung its lantern back, picked its pickaxe up, went to the boulder and struck it {Time.time - began:F1} s after the order");
        }
    }
}
