using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // Where a body stands to mine a boulder, and where on it it strikes (S3, step 9). Both come from the rock's own
    // shape and the ground. The rock is felt from above along a line towards the body, which gives its outline on
    // that side. Spots are tried along that outline, from the rock's foot inwards, where the rock faces up and
    // nothing of it stands higher before them, no higher than half the body's own height. For each, the body would
    // stand with its boots clear of the rock's foot, on open ground no more than a few steps off the ground that can
    // be walked (UnitMotor.StepOff); the spot and the place taken are those from which the swing reaches most easily
    // (PhysicalSwing.AimFrom). A low spot is struck with the blow landing late in its arc, the pick's head below the
    // hands. It looks first on the side the body comes from, then round the rock to either side.
    public static class RockWork
    {
        public struct Plan
        {
            public bool found;
            // The spot on the rock the swing is aimed at, and the way out of the rock there; and where, on its way
            // down to that spot, the pick's head first meets the rock (a low spot is struck late in the blow's arc,
            // with the head coming back towards the body: on a rock that rises inwards it lands a little further in).
            public Vector3 spot, outward, lands;
            // Where to stand (the body's own place), and facing which way; and the place on the walked ground from
            // which it steps there (the same place, when the walked ground reaches it).
            public Vector3 stand, approach;
            public Quaternion facing;
            // How far round the rock from the side the body came from (degrees), how high the spot is over the ground
            // there, how far the stand is from the spot (level), and how the swing takes it.
            public float round, height, away;
            public PhysicalSwing.Aimed aimed;
        }

        // How far forward the tool may lean as its head lands on a rock (degrees from the body's upright).
        public const float LeansUpTo = 78;
        // The rock is felt every so far, and a spot tried every so far inwards (metres). A spot is rock at least this
        // far over the ground, faces up at least this much, and the rock does not rise before it (towards the body)
        // by more than this. The boots stay this far clear of the rock's foot. A miss of more than this is no place
        // to work from.
        private const float Feels = .03f, Tries = .09f, Low = .08f, FacesUp = .3f, Rises = .03f, Clear = .06f, Misses = .03f;
        private const int Ground = 1 << 6;
        // Why the last search found what it found, on the side the body came from: for the bench.
        public static string Last = "";

        public static Plan Find(Boulder boulder, PhysicalSwing swing, ProceduralBiped body, float tall, Vector3 from)
        {
            var rock = boulder.Rock;
            Vector3 centre = rock.bounds.center;
            Vector3 toward = Vector3.ProjectOnPlane(from - centre, Vector3.up);
            toward = toward.sqrMagnitude > 1e-4f ? toward.normalized : Vector3.forward;
            for (int turn = 0; turn < 12; turn++)
            {
                float round = (turn + 1) / 2 * 30 * (turn % 2 == 1 ? 1 : -1);
                var plan = Along(rock, swing, body, tall, Quaternion.AngleAxis(round, Vector3.up) * toward, turn == 0);
                if (!plan.found) continue;
                plan.round = round;
                return plan;
            }
            return default;
        }

        // Whether something solid, other than the ground, the body itself and the rock it means to work on, is where a
        // body would stand.
        private static readonly Collider[] inTheWay = new Collider[8];
        private static bool Occupied(Vector3 place, float tall, Transform self, Collider rock)
        {
            int count = Physics.OverlapCapsuleNonAlloc(place + Vector3.up * .3f * tall, place + Vector3.up * .85f * tall, .16f * tall, inTheWay, ~Ground, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (inTheWay[i] != rock && !inTheWay[i].transform.IsChildOf(self) && inTheWay[i].GetComponentInParent<HeldThing>() == null
                    && inTheWay[i].GetComponentInParent<LooseStone>() == null) return true;
            return false;
        }

        // The best place on one side of the rock, if there is one.
        private static Plan Along(Collider rock, PhysicalSwing swing, ProceduralBiped body, float tall, Vector3 side, bool says = false)
        {
            int spots = 0, turnedAway = 0, noGround = 0, tooNear = 0, missed = 0;
            float leastMiss = float.MaxValue;
            var bounds = rock.bounds;
            Vector3 centre = bounds.center;
            float reach = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.5f + .6f, top = bounds.max.y + .5f, deep = bounds.size.y + 1.5f;
            int count = Mathf.CeilToInt(reach / Feels);
            // The rock's outline on this side, from outside inwards: how high it is, and its way out. Its foot is where
            // it first stands out of the ground (the ground slopes: it is felt under each place).
            var high = new float[count + 1];
            var faces = new Vector3[count + 1];
            int foot = -1;
            float floor = bounds.min.y;
            for (int i = 0; i <= count; i++)
            {
                Vector3 over = centre + side * (reach - i * Feels);
                over.y = top;
                high[i] = float.NegativeInfinity;
                if (foot < 0 && Physics.Raycast(new Vector3(over.x, top + 5, over.z), Vector3.down, out var earth, 50, Ground)) floor = earth.point.y;
                if (!rock.Raycast(new Ray(over, Vector3.down), out var felt, deep)) continue;
                high[i] = felt.point.y; faces[i] = felt.normal;
                if (foot < 0 && felt.point.y > floor + Clear) foot = i;
            }
            if (foot < 0) { if (says) Last = "the rock does not stand out of the ground on this side"; return default; }
            var shape = body.BodyProportions;
            float toes = shape.ballLength + shape.toeLength + PhysicalSwing.StanceStagger * body.StandingHipHeight;
            float farthest = 1.6f * body.ArmReach + .2f;
            Plan best = default;
            float least = float.MaxValue, before = float.NegativeInfinity;
            int tried = -100000;
            for (int i = foot; i <= count; i++)
            {
                float was = before;
                if (float.IsNegativeInfinity(high[i])) continue;
                before = Mathf.Max(before, high[i]);
                float height = high[i] - floor, footOut = (i - foot) * Feels;
                if (height > .5f * tall || footOut + toes + Clear > farthest) break;
                if ((i - tried) * Feels < Tries || height < Low) continue;
                // It faces up, not away from the body; and nothing of the rock stands higher before it.
                if (faces[i].y < FacesUp || Vector3.Dot(faces[i], side) < -.2f || was > high[i] + Rises) { turnedAway++; continue; }
                tried = i; spots++;
                Vector3 spot = centre + side * (reach - i * Feels);
                spot.y = high[i];
                // From how far it is best struck: looked for roughly, from as near as the boots may come.
                float nearest = Mathf.Max(.3f, footOut + toes + Clear), roughest = float.MaxValue;
                Vector3 stand = default, approach = default;
                Quaternion facing = Quaternion.identity;
                bool any = false;
                for (int k = 0; k < 6; k++)
                {
                    float away = nearest + k * .08f;
                    if (away > farthest) break;
                    Vector3 wanted = spot + side * away;
                    wanted.y = floor;
                    // The ground it can walk on must be within a few steps of there, and the place itself open ground
                    // with nothing else solid where the body would be.
                    if (!NavMesh.SamplePosition(wanted, out var walked, UnitMotor.OffAtMost + .3f, NavMesh.AllAreas)) { noGround++; continue; }
                    float off = Vector3.ProjectOnPlane(walked.position - wanted, Vector3.up).magnitude;
                    if (off > UnitMotor.OffAtMost - .1f) { noGround++; continue; }
                    Vector3 place = walked.position;
                    if (off > .03f)
                    {
                        if (!Physics.Raycast(wanted + Vector3.up * 3, Vector3.down, out var under, 8, Ground)) { noGround++; continue; }
                        // It rides as high over the ground there as it does on the walked ground.
                        float rides = Physics.Raycast(walked.position + Vector3.up * 3, Vector3.down, out var beneath, 8, Ground) ? walked.position.y - beneath.point.y : 0;
                        place = new Vector3(wanted.x, under.point.y + rides, wanted.z);
                        if (Occupied(place, tall, body.transform, rock)) { tooNear++; continue; }
                    }
                    Vector3 looks = Vector3.ProjectOnPlane(spot - place, Vector3.up);
                    if (looks.magnitude < nearest - .01f) { tooNear++; continue; }
                    Quaternion turned = Quaternion.LookRotation(looks.normalized);
                    var rough = swing.AimFrom(place, turned, spot, LeansUpTo, true);
                    if (rough.cost >= roughest) continue;
                    roughest = rough.cost; stand = place; approach = walked.position; facing = turned; any = true;
                }
                if (!any || roughest > least + .05f) continue;
                var aimed = swing.AimFrom(stand, facing, spot, LeansUpTo);
                leastMiss = Mathf.Min(leastMiss, aimed.miss);
                if (aimed.miss > Misses) missed++;
                if (aimed.miss > Misses || aimed.cost >= least) continue;
                least = aimed.cost;
                best = new Plan
                {
                    found = true, spot = spot, outward = faces[i], stand = stand, approach = approach, facing = facing, height = height,
                    away = Vector3.ProjectOnPlane(spot - stand, Vector3.up).magnitude, aimed = aimed,
                };
            }
            if (best.found)
            {
                // Where the head first meets the rock as it comes down: its path against the rock's outline.
                best.lands = best.spot;
                float ball = swing.tool.HeadRadius;
                for (float lean = 0; lean <= Mathf.Min(best.aimed.lean + 38, PhysicalSwing.ThroughAtMost); lean += 1)
                {
                    Vector3 head = swing.HeadAt(best.stand, best.facing, best.aimed, lean);
                    float along = (reach - Vector3.Dot(head - centre, side)) / Feels;
                    int at = Mathf.RoundToInt(along);
                    if (at < 0 || at > count || float.IsNegativeInfinity(high[at]) || head.y - ball > high[at]) continue;
                    best.lands = centre + side * (reach - at * Feels);
                    best.lands.y = high[at];
                    best.outward = faces[at];
                    break;
                }
            }
            if (says)
                Last = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "its foot {0:0.00} m from its middle, the ground there {1:0.00} m under its top; {2} spots tried, {3} turned away or behind higher rock; places to stand: {4} too far from ground that can be walked, {5} too near or taken; {6} out of reach (the nearest miss {7:0} mm)",
                    reach - foot * Feels, top - .5f - floor, spots, turnedAway, noGround, tooNear, missed, leastMiss < float.MaxValue ? leastMiss * 1000 : -1);
            return best;
        }
    }
}
