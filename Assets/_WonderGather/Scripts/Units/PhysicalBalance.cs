using System;
using UnityEngine;

namespace WonderGather
{
    // How a body keeps its feet (S3, step 6: Docs/Design/ThePhysicalBody.md).
    //
    // The body is one weight on its legs. Where that weight is comes from its own parts as they are posed
    // (PhysicalBody). What loads it comes from what its hands give a held thing (PhysicalHands) and from whatever
    // pushes or pulls it (Push). From these, at each step of the physics, it works out where on the ground its feet
    // must press for it not to be turned over; and, from how fast its weight is already going, the point its feet
    // must be able to press on to stop it: the weight's point.
    //
    // The ladder, each rung used only when the one before is not enough:
    //   At ease. The weight's point is where a standing body carries its weight. Nothing moves: it stands as posed.
    //   Lean. The point is inside the feet but not at ease. The feet press nearer one edge or the other, the hips move
    //     until the weight, with its load, is carried at ease again, and the body inclines against a load that lasts.
    //   Brace. A planner says an effort is coming (Brace), or the point nears the edge of the feet: the feet go
    //     further apart, to widen what the body stands on, and the knees bend.
    //   Step. The point leaves the feet: no pressing can hold it. The foot that is behind goes to where the point
    //     will be when it lands, and the body goes over with it; again, if that was not enough.
    // The fall (when no step can catch it) is a later step of the plan; until then it goes on stepping.
    //
    // Nothing here is chance. A body is steadier the heavier, the lower and the wider-set it is, and the less its
    // load is for it.
    [DefaultExecutionOrder(520)]
    [RequireComponent(typeof(ProceduralBiped), typeof(PhysicalBody))]
    public sealed class PhysicalBalance : MonoBehaviour
    {
        // A standing body carries its weight where its own way of standing has it (PhysicalBody.StandsOn); until that
        // has been measured, a little behind the middle of its feet: this share of the way from heel to toe. It is at
        // ease with its weight anywhere within so far of that, as a share of a boot's length.
        private const float Carries = .44f, AtEase = .12f;
        // How firmly it brings its weight's point back to where it is at ease, in the body's own falling times.
        private const float Firm = 2;
        // The body's weight moves by this share of what its hips move by (its legs stay on the ground).
        private const float Follows = .85f;
        // It steps when the weight's point has been this near the edge of its feet (as a share of a boot's half-width)
        // for this long (it takes a moment to answer). The step takes so many of the body's own falling times, and
        // lands no further from the other foot than so far (in hip heights).
        private const float Near = .4f, Answers = .06f, StepTakes = 1.1f, StepReach = .75f;
        // How the pose's own movement of the weight is smoothed, in seconds; and over how long a load must last to
        // count as one the body must answer (a blow's jolt moves the body by what it gives it, and is gone).
        private const float Smooths = .05f, Lasts = .12f;
        // The body inclines against a load that lasts by this share of the angle the load asks, at most so many degrees.
        private const float Inclines = .6f, MostInclined = 12;
        // On one foot, the trunk and the arms thrown against the fall give the foot a little more to press with for
        // that moment: as if it reached this share of a boot's length further.
        private const float Throws = .3f;
        // Where a step has left its feet apart, the knees bend by this share of the hips' height. The feet come
        // together again when no load has shifted the weight by more than this share of a boot's length for this long.
        private const float Crouches = .05f, Light = .15f, Relaxes = 1.2f;
        // A foot that is to step to its place waits for the weight to come off it, no longer than this.
        private const float Waits = .8f;
        // The legs raise the body at their own pace while the knees give no more than this share of what they have,
        // and slower as they near all of it. Knees asked for more than they have give way, at this pace (metres a
        // second), and come up again when they are asked for less than this share.
        private const float Easy = .5f, GivesWay = .12f, ComesUp = .9f;

        // When it does not act it only measures: the body stands as it is posed.
        public bool Acts = true;

        private ProceduralBiped body;
        private PhysicalBody physical;
        private PhysicalHands hands;
        private PhysicalBack back;
        // Where the body's own weight is and how fast it is going, on the ground's plane (world x and z); and the hips'
        // place from where the stance alone would have them, which is what puts the weight there.
        private Vector2 stands, going, lean, posedBefore;
        private bool began;
        // How the pose itself is moving the weight (a bow, an arm thrown out), measured from pose to pose.
        private Vector2 intent, ownBefore, movedSince;
        // The same from the last pose to this one, unsmoothed: how the pose goes on until it is next drawn.
        private Vector2 drift;
        // How far its load shifts where the feet must press, from under the body's own weight: as it has lasted.
        private Vector2 steady;
        private float seenAt = -1, near, shiftTime, calm, waited, marginBefore;
        private int waiting = -1;
        private bool unloaded;
        private float gave;
        private Vector2 shiftLeft;
        private int next;
        // The stance a planner asked for (shares of the hips' width and height), and whether the feet are where a
        // step left them.
        private bool planned, stepped;
        private float plannedWider, plannedStagger;
        // What the body stands on: the corners of its planted boots' lines, the half-width of a boot round them,
        // where it carries its weight on them, a boot's length, and how high the ground is there.
        private readonly Vector2[] corners = new Vector2[4];
        private int cornerCount;
        private float around, ground, boot, carries = Carries;
        private Vector2 middle;
        // What pushes or pulls the body at this step, besides what it holds.
        private readonly Vector3[] pushForce = new Vector3[4], pushAt = new Vector3[4];
        private int pushes;

        // Where the feet must press for the body not to be turned over as it stands, with the load that lasts.
        public Vector3 Weight { get; private set; }
        // The weight's point: where the feet must be able to press to stop the body, going as it is.
        public Vector3 WeightPoint { get; private set; }
        // Where the feet press.
        public Vector3 Presses { get; private set; }
        // Where it carries its weight when it is at ease.
        public Vector3 Middle => Raised(middle);
        // How far inside its feet the weight's point is, in metres (negative: outside them).
        public float Margin { get; private set; }
        // The least that margin has been, the furthest the hips have leaned (metres), and the steps taken to keep or
        // catch its balance, since Mark.
        public float LeastMargin { get; private set; }
        public float MostLean { get; private set; }
        public int Steps { get; private set; }
        // What the feet bear, in newtons.
        public float Bears { get; private set; }
        // The hips' lean now, to the body's right (x) and ahead (y), in metres; and how far the body inclines, in
        // degrees, the same ways.
        public Vector2 Lean { get; private set; }
        public Vector2 Inclined { get; private set; }
        // The share of what the knees have that the harder-worked one gave at the last step (1: all it has), and how
        // far the legs have given way under what they bear, in metres.
        public float LegEffort { get; private set; }
        public float GaveWay => gave;
        // The feet are set for an effort, or where a step left them.
        public bool Braced => planned || stepped;
        public void Mark() { LeastMargin = float.MaxValue; MostLean = 0; Steps = 0; }

        // Something pushes or pulls the body with a force (newtons) at a place, for this step of the physics.
        public void Push(Vector3 force, Vector3 at)
        {
            if (pushes >= pushForce.Length) return;
            pushForce[pushes] = force; pushAt[pushes] = at; pushes++;
        }

        // An effort is coming (a lift, a swing): the body sets its feet for it, so much further apart than its hips
        // (a share of their width, each side) and the left so far ahead of the right (a share of the hips' height).
        public void Brace(float wider, float stagger) { planned = true; plannedWider = wider; plannedStagger = stagger; }
        // The effort is over: the feet come together again.
        public void Ease() { planned = false; }

        // What it stands on, for a picture: the corners of its feet's shape on the ground, and the half-width of a boot
        // round them.
        public int Outline(Span<Vector3> into, out float halfWidth)
        {
            halfWidth = around;
            for (int i = 0; i < cornerCount && i < into.Length; i++) into[i] = Raised(corners[i]);
            return Mathf.Min(cornerCount, into.Length);
        }

        private static Vector2 Flat(Vector3 v) => new Vector2(v.x, v.z);
        private Vector3 Raised(Vector2 v) => new Vector3(v.x, ground, v.y);
        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        private void Awake()
        {
            body = GetComponent<ProceduralBiped>();
            physical = GetComponent<PhysicalBody>();
            Mark();
        }

        private void OnEnable()
        {
            lean = going = intent = drift = movedSince = steady = Vector2.zero; began = false;
            seenAt = -1; near = 0; shiftTime = 0; calm = 0; pushes = 0; stepped = false; waiting = -1; waited = 0; gave = 0;
            body.MayLift = MayLift;
        }

        // A foot is due to step to its place in the stance: it may lift when the body's weight is off it.
        private bool MayLift(int foot)
        {
            if (!Acts || body.CurrentGait != ProceduralBiped.Gait.Standing) return true;
            return waiting == foot && (unloaded || waited > Waits);
        }

        private void OnDisable()
        {
            if (body == null) return;
            body.MayLift = null;
            body.LeanIs(new Vector2(float.NaN, 0));
            body.SetLean(Vector2.zero, 1);
            body.SetTilt(Vector2.zero, 60);
            body.Crouch(0);
            body.SetRise(1);
            body.SetStance(0, 0);
        }

        // What the body stands on now: its planted feet.
        private void Region()
        {
            Span<Vector2> points = stackalloc Vector2[4];
            int n = 0, planted = 0;
            bool any = body.FootPlanted(0) || body.FootPlanted(1);
            middle = Vector2.zero; ground = 0;
            carries = float.IsNaN(physical.StandsOn) ? Carries : physical.StandsOn;
            for (int i = 0; i < 2; i++)
            {
                if (any && !body.FootPlanted(i)) continue;
                body.Sole(i, out Vector3 heel, out Vector3 toe, out around);
                points[n++] = Flat(heel); points[n++] = Flat(toe);
                middle += Vector2.Lerp(Flat(heel), Flat(toe), carries); ground += (heel.y + toe.y) * .5f; planted++;
                boot = Vector3.Distance(heel, toe);
            }
            middle /= planted; ground /= planted;
            // The shape round those corners, counter-clockwise.
            if (n <= 2) { for (int i = 0; i < n; i++) corners[i] = points[i]; cornerCount = n; return; }
            int start = 0;
            for (int i = 1; i < n; i++) if (points[i].x < points[start].x || (points[i].x == points[start].x && points[i].y < points[start].y)) start = i;
            int count = 0, at = start;
            do
            {
                corners[count++] = points[at];
                int to = (at + 1) % n;
                for (int i = 0; i < n; i++)
                {
                    if (i == at || i == to) continue;
                    float side = Cross(points[to] - points[at], points[i] - points[at]);
                    if (side < 0 || (side == 0 && (points[i] - points[at]).sqrMagnitude > (points[to] - points[at]).sqrMagnitude)) to = i;
                }
                at = to;
            } while (at != start && count < n);
            cornerCount = count;
        }

        // How far inside what it stands on a point of the ground is (negative: outside), and the nearest point of what
        // it stands on to it.
        private float Inside(Vector2 x, out Vector2 nearest) => Inside(x, around, out nearest);

        // The same, with what it stands on taken to reach so far round its boots' lines.
        private float Inside(Vector2 x, float around, out Vector2 nearest)
        {
            float best = float.MaxValue;
            Vector2 on = corners[0];
            bool within = cornerCount >= 3;
            int edges = cornerCount >= 3 ? cornerCount : Mathf.Max(1, cornerCount - 1);
            for (int i = 0; i < edges; i++)
            {
                Vector2 a = corners[i], b = corners[cornerCount == 1 ? i : (i + 1) % cornerCount], ab = b - a;
                float t = ab.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector2.Dot(x - a, ab) / ab.sqrMagnitude) : 0;
                Vector2 c = a + ab * t;
                float d = (x - c).sqrMagnitude;
                if (d < best) { best = d; on = c; }
                if (within && Cross(ab, x - a) < 0) within = false;
            }
            best = Mathf.Sqrt(best);
            if (within) { nearest = x; return around + best; }
            float inside = around - best;
            nearest = inside >= 0 ? x : on + (x - on) / best * around;
            return inside;
        }

        // The nearest point of one planted boot to a point of the ground (the boot reaching so much further).
        private Vector2 OnSole(int foot, Vector2 x, float further)
        {
            body.Sole(foot, out Vector3 heel, out Vector3 toe, out float half);
            half += further;
            Vector2 a = Flat(heel), ab = Flat(toe) - a;
            Vector2 c = a + ab * Mathf.Clamp01(Vector2.Dot(x - a, ab) / Mathf.Max(1e-8f, ab.sqrMagnitude));
            float d = (x - c).magnitude;
            return d <= half ? x : c + (x - c) / d * half;
        }

        private void FixedUpdate()
        {
            if (!physical.Ready || !body.Ready) return;
            if (hands == null) hands = GetComponent<PhysicalHands>();
            if (back == null) back = GetComponent<PhysicalBack>();
            float dt = Time.fixedDeltaTime, g = Physics.gravity.magnitude;
            Region();
            Quaternion facing = body.FacingNow;
            Vector2 right = Flat(facing * Vector3.right), ahead = Flat(facing * Vector3.forward);

            // The body's own weight, where its pose has it; less what rides on a held thing with the hands (that is
            // part of what the hands hold up).
            float mass = physical.Mass;
            Vector3 centre = physical.CentreOfMass() * mass;
            // What is taken off its feet, and what turns it over: each force on the body, by where it acts.
            float lifts = 0;
            Vector2 turns = Vector2.zero;
            if (hands != null && hands.Held != null)
                for (int i = 0; i < 2; i++)
                {
                    if (!hands.Holds(i)) continue;
                    Vector3 at = hands.GripPlace(i);
                    float rides = hands.Rides(i);
                    mass -= rides; centre -= at * rides;
                    // What the arm gives the thing, the thing gives back to the body.
                    Vector3 force = -hands.Gives(i);
                    lifts += force.y;
                    turns += Flat(force) * (at.y - ground) - Flat(at) * force.y;
                }
            for (int i = 0; i < pushes; i++)
            {
                lifts += pushForce[i].y;
                turns += Flat(pushForce[i]) * (pushAt[i].y - ground) - Flat(pushAt[i]) * pushForce[i].y;
            }
            pushes = 0;
            centre /= mass;
            float bears = Mathf.Max(mass * g - lifts, .25f * mass * g);
            Bears = bears;

            // The body is drawn between the physics' steps. Its weight is read as it was last drawn, and moved by what
            // the lean has done since.
            Vector2 posed = right * body.LeanPosed.x + ahead * body.LeanPosed.y;
            Vector2 own = Flat(centre) - posed * Follows;
            if (body.PosedAt != seenAt)
            {
                float span = body.PosedAt - seenAt;
                if (seenAt >= 0 && span > 1e-5f && span < .25f)
                {
                    drift = Vector2.ClampMagnitude((own - ownBefore - movedSince * Follows) / span, 3);
                    intent = Vector2.Lerp(intent, drift, 1 - Mathf.Exp(-span / Smooths));
                }
                else intent = drift = Vector2.zero;
                ownBefore = own; seenAt = body.PosedAt; movedSince = Vector2.zero;
            }
            // Where the pose alone has the body's weight now, going on as it was going since it was last drawn; the
            // lean is whatever puts the weight where the physics has it. And how far what loads the body shifts where
            // the feet must press.
            Vector2 posedNow = own + movedSince * Follows;
            // The pose's fastest movement of the weight is the bow, and the back says how far it has bowed since the
            // body was last drawn: the upper body's weight has gone that much further forward.
            if (back != null && back.isActiveAndEnabled)
            {
                physical.UpperBody(body.HipsNow, out float upper, out Vector3 carried, out _);
                posedNow += ahead * (upper / mass * (carried.y - body.HipsNow.y) * (back.BowNow - body.BowNow) * Mathf.Deg2Rad);
            }
            if (!began) { stands = posedBefore = posedNow; going = intent; began = true; }
            Vector2 load = (Flat(centre) * (mass * g) + turns) / bears - Flat(centre);
            steady = Vector2.Lerp(steady, load, 1 - Mathf.Exp(-dt / Lasts));
            Vector2 weight = stands + load;
            float height = Mathf.Max(.2f, centre.y - ground), pace = Mathf.Sqrt(g / height);

            bool standing = body.CurrentGait == ProceduralBiped.Gait.Standing;
            bool holds = Acts && standing;
            if (!holds)
            {
                // Walking, the walk carries the body: the lean lets go and the feet are the walk's.
                going = intent;
                lean *= Mathf.Exp(-5 * dt);
                stands = posedNow + lean * Follows;
                near = 0; shiftTime = 0; calm = 0; stepped = false;
            }
            Vector2 point = stands + steady + going / pace;
            Margin = Inside(point, out _);
            // Where it would carry its weight standing as posed, if that is at ease; the nearest place at ease to that,
            // if not. It leans no more than it needs.
            float ease = AtEase * boot;
            Vector2 rest = middle + Vector2.ClampMagnitude(point - lean * Follows - middle, ease);
            // Under a load that lasts it keeps its own weight over its feet too, where it can: so that a load that
            // lets go does not topple it.
            Inside(rest - steady, 0, out Vector2 kept);
            rest = kept + steady;
            bool both = body.FootPlanted(0) && body.FootPlanted(1);
            // A foot that is due to step to its place waits: the weight goes over the other foot first.
            int due = both ? body.LiftDue : -1;
            if (due != waiting) { waiting = due; waited = 0; }
            unloaded = false;
            if (due >= 0)
            {
                waited += dt;
                body.Sole(1 - due, out Vector3 heel, out Vector3 toe, out _);
                rest = Vector2.Lerp(Flat(heel), Flat(toe), carries);
                unloaded = (point - OnSole(1 - due, point, Throws * boot)).sqrMagnitude < 1e-4f;
            }
            // On one foot it has a little more to press with, for that moment.
            Inside(point + Firm * (point - rest), both ? around : around + Throws * boot, out Vector2 press);
            if (holds)
            {
                // The weight falls away from where the feet press, as fast as its height makes it.
                going += pace * pace * (weight - press) * dt;
                stands += going * dt;
                lean = (stands - posedNow) / Follows;
                // The hips go no further than the legs allow. Held there, the weight is where that has it, and goes as
                // the pose takes it and no faster that way.
                float hip = body.StandingHipHeight;
                float aside = Vector2.Dot(lean, right), forth = Vector2.Dot(lean, ahead);
                float asideKept = Mathf.Clamp(aside, -ProceduralBiped.LeanAside * hip, ProceduralBiped.LeanAside * hip);
                float forthKept = Mathf.Clamp(forth, -ProceduralBiped.LeanBack * hip, ProceduralBiped.LeanAhead * hip);
                if (asideKept != aside || forthKept != forth)
                {
                    lean = right * asideKept + ahead * forthKept;
                    stands = posedNow + lean * Follows;
                    Vector2 carried = (posedNow - posedBefore) / dt;
                    if (asideKept != aside && (Vector2.Dot(going, right) - Vector2.Dot(carried, right)) * (aside - asideKept) > 0) going += right * Vector2.Dot(carried - going, right);
                    if (forthKept != forth && (Vector2.Dot(going, ahead) - Vector2.Dot(carried, ahead)) * (forth - forthKept) > 0) going += ahead * Vector2.Dot(carried - going, ahead);
                }
                Vector2 moves = going - intent;

                // A step that is being taken takes the root with it.
                if (shiftTime > 0)
                {
                    float share = Mathf.Min(1, dt / shiftTime);
                    Vector2 moved = Flat(body.Shift(new Vector3(shiftLeft.x * share, 0, shiftLeft.y * share)));
                    shiftLeft -= shiftLeft * share; shiftTime -= dt;
                    lean -= moved; movedSince += moved; posedNow += moved * Follows;
                }
                if (both && shiftTime <= 0)
                {
                    // Near the edge and not coming back from it.
                    near = Margin < Near * around && Margin <= marginBefore + 1e-5f ? near + dt : 0;
                    if (near >= Answers && Step(point, pace, right, ahead)) near = 0;
                }
                else if (!both) near = 0;

                // The feet come together again when nothing has loaded the body for a while.
                calm = steady.magnitude < Light * boot && Margin > around ? calm + dt : 0;
                if (stepped && calm > Relaxes && both && shiftTime <= 0) stepped = false;
                if (!stepped)
                {
                    if (planned) body.SetStance(plannedWider * body.BodyProportions.hipWidth, plannedStagger * hip);
                    else body.SetStance(0, 0);
                }
                // What the legs bear. Each knee holds up its share of what is on the feet, by how far it stands out
                // beyond a straight leg's knee: nothing standing straight, more the deeper it bends.
                Vector2 leftOn = OnSole(0, press, 0), rightOn = OnSole(1, press, 0);
                float toLeft = (press - leftOn).magnitude, toRight = (press - rightOn).magnitude;
                float onLeft = !body.FootPlanted(1) ? 1 : !body.FootPlanted(0) ? 0 : toLeft + toRight > 1e-5f ? toRight / (toLeft + toRight) : .5f;
                float knees = physical.KneeNow, straight = body.KneeOutStraight;
                LegEffort = Mathf.Max(bears * onLeft * Mathf.Max(0, body.KneeOut(0) - straight), bears * (1 - onLeft) * Mathf.Max(0, body.KneeOut(1) - straight)) / knees;
                physical.Worked(PhysicalBody.Muscles.Legs, LegEffort, dt);
                body.SetRise(Mathf.Clamp01((1 - LegEffort) / (1 - Easy)));
                if (LegEffort > 1) gave = Mathf.Min(gave + GivesWay * dt, .25f * hip);
                else if (LegEffort < ComesUp) gave = Mathf.Max(0, gave - GivesWay * Mathf.Clamp01((1 - LegEffort) / (1 - Easy)) * dt);
                // Where a step left its feet, the knees stay bent.
                body.Crouch((stepped ? Crouches * hip : 0) + gave);

                // It inclines against a load that lasts.
                float asks = Mathf.Min(Mathf.Atan2(steady.magnitude, height) * Mathf.Rad2Deg * Inclines, MostInclined);
                Vector2 against = steady.sqrMagnitude > 1e-8f ? -steady.normalized * asks : Vector2.zero;
                Inclined = new Vector2(Vector2.Dot(against, right), Vector2.Dot(against, ahead));
                body.SetTilt(Inclined, 40);

                // The body is drawn before the next step: it goes on to where the lean will be then.
                Vector2 soon = lean + moves * (dt / Follows);
                body.SetLean(new Vector2(Vector2.Dot(soon, right), Vector2.Dot(soon, ahead)), moves.magnitude / Follows + .02f);
                // What works on the physics' clock is told where the lean is now.
                body.LeanIs(new Vector2(Vector2.Dot(lean, right), Vector2.Dot(lean, ahead)));
            }
            else if (Acts)
            {
                body.SetLean(new Vector2(Vector2.Dot(lean, right), Vector2.Dot(lean, ahead)), 1);
                body.LeanIs(new Vector2(float.NaN, 0));
                body.SetTilt(Vector2.zero, 40);
                body.Crouch(0);
                body.SetRise(1);
                body.SetStance(0, 0);
                Inclined = Vector2.zero; LegEffort = 0; gave = 0;
            }

            marginBefore = Margin; posedBefore = posedNow;
            Weight = Raised(stands + steady); WeightPoint = Raised(point); Presses = Raised(press);
            Lean = new Vector2(Vector2.Dot(lean, right), Vector2.Dot(lean, ahead));
            if (standing)
            {
                if (body.FootPlanted(0) && body.FootPlanted(1)) LeastMargin = Mathf.Min(LeastMargin, Margin);
                MostLean = Mathf.Max(MostLean, lean.magnitude);
            }
        }

        // A step to keep or catch its balance. A foot steps to where the weight's point will be when it lands: while it
        // is in the air the other foot alone holds the body, and the point runs on from it. It lands on its own side
        // of that point, so the feet do not close on a line.
        private bool Step(Vector2 point, float pace, Vector2 right, Vector2 ahead)
        {
            Vector2 way = point - middle;
            if (way.sqrMagnitude < 1e-8f) return false;
            way.Normalize();
            // Going out past a foot's own side, that foot steps out, and the body ends set wide. Going between its feet,
            // the foot that is behind steps past the other; side by side, they take turns.
            Vector2 leftAt = Flat(body.FootPosition(0)), rightAt = Flat(body.FootPosition(1));
            float across = Vector2.Dot(point, right), leftAcross = Vector2.Dot(leftAt, right), rightAcross = Vector2.Dot(rightAt, right);
            int foot;
            if (across > Mathf.Max(leftAcross, rightAcross)) foot = rightAcross >= leftAcross ? 1 : 0;
            else if (across < Mathf.Min(leftAcross, rightAcross)) foot = leftAcross <= rightAcross ? 0 : 1;
            else
            {
                float left = Vector2.Dot(leftAt - middle, way), rightFoot = Vector2.Dot(rightAt - middle, way);
                foot = Mathf.Abs(left - rightFoot) < .03f ? next : left < rightFoot ? 0 : 1;
            }
            float takes = StepTakes / pace;
            Vector2 held = OnSole(1 - foot, point, Throws * boot);
            Vector2 lands = held + (point - held) * Mathf.Exp(pace * takes) + right * ((foot == 0 ? -1 : 1) * (around + .01f));
            // The foot's middle goes there: no nearer than a short step, and no further from the other foot than a step
            // reaches.
            float hip = body.StandingHipHeight;
            Vector2 from = Flat(body.FootPosition(foot)), other = Flat(body.FootPosition(1 - foot));
            Vector2 stride = lands - ahead * body.FootMiddle - from;
            if (stride.magnitude < .25f * boot) stride = (stride + way * .01f).normalized * (.25f * boot);
            Vector2 to = other + Vector2.ClampMagnitude(from + stride - other, StepReach * hip);
            if (!body.StepTo(foot, new Vector3(to.x, ground, to.y), takes, out Vector3 landing)) return false;
            // The feet stay where the step leaves them, and the body stands between them.
            Vector2 landed = Flat(landing), l = foot == 0 ? landed : other, r = foot == 0 ? other : landed;
            body.SetStance(Vector2.Dot(r - l, right) * .5f - body.BodyProportions.hipWidth, Vector2.Dot(l - r, ahead));
            shiftLeft = (l + r) * .5f - Flat(body.StanceMiddle);
            shiftTime = takes;
            stepped = true; calm = 0;
            next = 1 - foot;
            Steps++;
            return true;
        }
    }
}
