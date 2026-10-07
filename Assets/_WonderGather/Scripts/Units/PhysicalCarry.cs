using UnityEngine;

namespace WonderGather
{
    // How a body holds a tool it is not working with, standing or walking (S3, step 7: Docs/Design/ThePhysicalBody.md).
    //
    // Its strength says how. A hand's hold has a most it can give (PhysicalBody.HoldOf). While holding the tool's
    // weight asks no more than half of that, the tool is carried in one hand at the body's side, held at its balance
    // point, the arm hanging. A second hand would not share a pickaxe's weight (its weight is at its head: the hand
    // under the head carries it all), so a tool too heavy for one hand is not carried at all: a hand takes it by the
    // end of its handle and it is dragged, its head on the ground. Dragging, the hand bears little, and the body
    // pulls against the ground's hold on the head: the harder that is for it, the slower it walks. A tool it cannot
    // move, it leaves where it lies.
    //
    // A tired arm holds less: a tool carried far enough is dragged in the end.
    [RequireComponent(typeof(ProceduralBiped), typeof(PhysicalBody))]
    public sealed class PhysicalCarry : MonoBehaviour
    {
        public enum Way { OneHand, Dragged, Left }

        // A tool is carried in one hand while holding it asks no more than this share of the hand's hold; dragged, it
        // is taken up again when it asks less than that share.
        private const float Carries = .5f, CarriesAgain = .3f;
        // A body pulls, walking, with this share of its own weight at most (an ordinary body of its build, fresh). It
        // leaves what slows it to this share of its pace, after so long.
        private const float Pulls = .3f, Stuck = .12f, GivesUp = 2;
        // Dragging, the hand holds the handle's end this share of the tool's length above the ground (so the head lies
        // on the ground and bears the tool's weight). The body stoops from the back by as much as its arm is short of
        // that, this far at most; its knees bend only for what the stoop leaves, and no further than this share of the
        // hips' height. Standing still, it straightens, and the tool stands on its head under its hand.
        private const float HandUp = .8f, DragBow = 52, DragSink = .12f, HandUpStanding = .97f;

        public PhysicalHands hands;
        public PhysicalBack back;
        public ProceduralBiped body;
        public ToolDefinition tool;
        public Way way;
        // The share of one hand's hold that carrying the tool asks (1: all of it).
        public float Asks { get; private set; }
        // What the body pulls with along the ground, dragging, in newtons; and the share of its walking pace it keeps.
        public float Pull { get; private set; }
        public float Pace { get; private set; } = 1;
        // A tool it has left, lying where it left it.
        public Rigidbody Lies { get; private set; }

        private PhysicalBody physical;
        private PhysicalBalance balance;
        private UnitMotor motor;
        private float clock, stuck, bowFrom, drags, stoop;
        private Vector3 from, endFrom;
        private Quaternion fromTurn;
        private float endClock;
        // Laying it down, and going to take one up: the body goes down to the ground for both. It bows a little
        // first, then bends its knees (to this share of the hips' height), and bows further (this far) only for what
        // that leaves.
        private const float FirstBow = 42, GroundBow = 78, GroundSink = .6f;
        // Reaching the ground, the arm stretches to this share of its length.
        private const float Stretches = .96f;
        private bool arrived;
        private PhysicalFall fall;
        private bool lookedForFall;
        // How often it has stepped nearer a tool it could not reach.
        private int nearer;
        // Going to take a tool up, it stops this near the place it means to stand (metres): nearer than a walk does.
        private const float StandsWithin = .03f;
        private UnityEngine.AI.NavMeshAgent agent;
        private float stoppedWithin = -1;
        private bool laying, walked;
        private float layClock, fetchClock;
        private HeldThing fetching;
        private int fetchHand;
        private Vector3 faces;
        // It is laying its tool down; it is going to take one up.
        public bool Laying => laying;
        public bool Fetching => fetching != null;

        // It lays the tool it carries down on the ground beside it, lets go, and stands up. (A tool it drags lies
        // already: it only lets go.)
        public void LayDown()
        {
            if (hands == null || hands.Held == null || laying) return;
            if (way == Way.Dragged) { Leave(); return; }
            laying = true; layClock = 0;
        }

        // It goes to a tool that lies in the world, bends down, and takes it up: under its head, to carry it; by the
        // end of its handle if it is too heavy for its hand.
        public void Fetch(HeldThing lying)
        {
            if (lying == null || lying.Tool == null || hands == null || hands.Held != null || lying.Holder != null) return;
            hands.Adopt(lying);
            float asks = lying.Tool.Mass * lying.Weight * Physics.gravity.magnitude / Mathf.Max(1e-3f, physical.HoldOf(0));
            fetching = lying; fetchHand = asks > Carries ? 1 : 0; walked = false; arrived = false; fetchClock = 0; nearer = 0;
            laying = false; Lies = null; stoop = 0;
        }

        // The body bends down until a shoulder is near enough a place for its hand to reach it: from the back, then
        // from the knees.
        private float BendTo(Vector3 shoulder, Vector3 place, float dt)
        {
            float lacks = Vector3.Distance(shoulder, place) - Stretches * body.ArmReach;
            float deepest = GroundSink * body.StandingHipHeight;
            if (lacks > 0)
            {
                if (stoop < FirstBow) stoop = Mathf.Min(FirstBow, stoop + lacks * 320 * dt);
                else if (body.SinkNow < deepest - .004f) body.Sink(Mathf.Min(deepest, body.SinkNow + lacks * .6f));
                else stoop = Mathf.Min(GroundBow, stoop + lacks * 320 * dt);
            }
            else
            {
                // Length to spare: it comes up a little, back first.
                if (stoop > FirstBow) stoop = Mathf.Max(FirstBow, stoop + lacks * 320 * dt);
                else body.Sink(Mathf.Max(0, body.SinkNow + lacks * .6f));
            }
            return lacks;
        }

        private void Awake()
        {
            physical = GetComponent<PhysicalBody>();
            motor = GetComponent<UnitMotor>();
        }

        private void OnEnable() { clock = -1; stuck = 0; Pull = 0; Pace = 1; drags = 0; stoop = 0; laying = false; fetching = null; }

        private void OnDisable()
        {
            Walks();
            if (motor != null) motor.SetMovementRate(1);
            Pace = 1; Pull = 0;
        }

        // How high the ground is where the body stands: under its feet. (The unit's own place rides a little above it.)
        private float Ground => (body.FootPosition(0).y + body.FootPosition(1).y) * .5f;

        private void Go(Way next)
        {
            way = next; clock = 0; stuck = 0;
            if (hands != null && hands.Held != null) { from = hands.Held.position; fromTurn = hands.Held.rotation; }
            if (body != null) bowFrom = body.BowNow;
        }

        // The tool carried at the side in one hand (the left): level, its head ahead and its point down, the arm
        // hanging, the hand out past the hip and whatever hangs there.
        public static void AtSide(ProceduralBiped body, PhysicalHands hands, Vector3 hips, Quaternion posture, out Vector3 position, out Quaternion rotation)
        {
            float reach = body.ArmReach;
            Vector3 shoulder = body.ShoulderFromHips;
            float aside = .2f * reach + body.BodyProportions.armCarry.x, ahead = .06f * reach;
            float drop = Mathf.Sqrt(Mathf.Max(.01f, .94f * reach * .94f * reach - aside * aside - ahead * ahead));
            rotation = posture * Quaternion.LookRotation(Vector3.down, Vector3.forward);
            Vector3 hand = hips + posture * new Vector3(-(shoulder.x + aside), shoulder.y - drop, shoulder.z + ahead);
            position = hand - rotation * new Vector3(0, hands.GripAlong(0), 0);
        }

        private void FixedUpdate()
        {
            // A body that is down, or getting up, is not the carry's to stand up.
            if (fall == null && !lookedForFall) { fall = GetComponent<PhysicalFall>(); lookedForFall = true; }
            if (fall != null && fall.Now != PhysicalFall.State.Up) return;
            if (hands == null || hands.Held == null || body == null)
            {
                // The tool has slipped from its hand: it lies where it fell.
                if (hands != null && hands.Slipped != null && way != Way.Left && clock >= 0) { Lies = hands.Slipped; way = Way.Left; Pull = 0; Pace = 1; }
                if (motor != null) motor.SetMovementRate(1);
                // With nothing in its hands it stands up.
                laying = false; fetching = null; stoop = 0;
                if (back != null) back.Want(0);
                if (body != null) body.Sink(0);
                return;
            }
            float dt = Time.fixedDeltaTime, g = Physics.gravity.magnitude;
            // It is not set for work: its feet come together.
            if (balance == null) balance = GetComponent<PhysicalBalance>();
            if (balance != null) balance.Ease();
            // (The first two of these are the shoulders, the left then the right.)
            System.Span<Vector3> joints = stackalloc Vector3[4];
            body.StandsAt(Time.time, out Vector3 hips, out Quaternion posture, joints.Slice(0, 2), joints.Slice(2, 2));
            // How much of a hand's hold the tool's weight asks: that says how it is held.
            Asks = hands.ToolMass * g / Mathf.Max(1e-3f, physical.HoldOf(0));
            if (fetching != null) { Take(dt, joints); return; }
            if (clock < 0) Go(Asks > Carries ? Way.Dragged : Way.OneHand);
            else if (way == Way.OneHand && Asks > Carries) Go(Way.Dragged);
            else if (way == Way.Dragged && Asks < CarriesAgain) Go(Way.OneHand);
            clock += dt;
            // Carrying, it stands up as the tool comes with it; dragging, it stoops to it.
            drags = Mathf.MoveTowards(drags, way == Way.Dragged && motor != null && motor.IsMoving ? 1 : 0, dt / .5f);
            bool lays = laying && way == Way.OneHand && hands.Holds(0) && !hands.Holds(1);
            if (way != Way.Dragged && !lays) stoop = 0;
            if (back != null) back.Want(Mathf.Lerp(bowFrom, stoop, Mathf.SmoothStep(0, 1, clock / 1.1f)));

            if (lays)
            {
                // The tool is laid flat on the ground at the body's left: its handle along the way the body faces, the
                // points of its head level. The hand takes it down as the body bends, and lets go when it lies.
                layClock += dt;
                Pull = 0; Pace = 1;
                Quaternion facing = body.FacingNow;
                float rests = Mathf.Max(tool.HeadRadius, hands.RadiusAt(hands.GripAlong(0))) + .012f;
                Vector3 hand = joints[0] + facing * new Vector3(-.1f * body.ArmReach, 0, .05f * body.ArmReach);
                hand.y = Ground + rests;
                Quaternion flat = facing * Quaternion.LookRotation(Vector3.left, Vector3.forward);
                Vector3 lies = hand - flat * new Vector3(0, hands.GripAlong(0), 0);
                BendTo(joints[0], hand, dt);
                AtSide(body, hands, hips, posture, out var carried, out var carriedTurn);
                float down = Mathf.SmoothStep(0, 1, layClock / 1.5f);
                hands.Want(Vector3.Lerp(carried, lies, down), Quaternion.Slerp(carriedTurn, flat, down));
                bool there = hands.GripPlace(0).y - hand.y < .03f && hands.Held.linearVelocity.sqrMagnitude < .02f;
                if (layClock > 1.5f && (there || layClock > 5))
                {
                    Lies = hands.Drop();
                    way = Way.Left; laying = false;
                }
                return;
            }

            if (way == Way.OneHand)
            {
                body.Sink(0);
                Pull = 0; Pace = 1;
                if (motor != null) motor.SetMovementRate(1);
                if (!hands.Holds(0))
                {
                    // From dragging: the other hand takes it under its head, and then carries it.
                    if (!hands.Reaching(0)) hands.Grasp(0, hands.HighestGrip);
                    hands.WantEnd(1, hands.GripPlace(1));
                    from = hands.Held.position; fromTurn = hands.Held.rotation; clock = 0;
                    return;
                }
                if (hands.Holds(1))
                {
                    hands.Slide(0, hands.HighestGrip);
                    if (clock > .25f && !hands.Sliding(0)) hands.Release(1);
                }
                AtSide(body, hands, hips, posture, out var to, out var toTurn);
                float t = Mathf.SmoothStep(0, 1, clock / 1.3f);
                hands.Want(Vector3.Lerp(from, to, t), Quaternion.Slerp(fromTurn, toTurn, t));
                return;
            }

            // Dragged: the lower hand (the right) holds the end of the handle at the body's side, a little behind, and
            // the head lies on the ground behind it.
            if (!hands.Holds(1))
            {
                if (!hands.Reaching(1)) hands.Grasp(1, hands.LowestGrip);
                if (hands.Holds(0)) hands.Want(hands.Held.position, hands.Held.rotation);
                clock = 0;
                return;
            }
            // The hand goes to the end of the handle first, with the other still holding; then it takes the end down
            // to its side, and the other lets go.
            if (!hands.Trailing)
            {
                hands.Slide(1, hands.LowestGrip);
                if (hands.Holds(0) && (hands.Sliding(1) || clock < .3f))
                {
                    hands.Want(hands.Held.position, hands.Held.rotation);
                    return;
                }
                endFrom = hands.GripPlace(1); endClock = 0;
            }
            endClock += dt;
            // The hand hangs from the shoulder, a little out from the body, as low as lets the head lie on the ground.
            // The knees bend by as much as the arm is short of that.
            float reach = body.ArmReach;
            float length = Vector3.Distance(tool.Head, new Vector3(0, hands.LowestGrip, 0));
            float up = Ground + Mathf.Lerp(HandUpStanding, HandUp, Mathf.SmoothStep(0, 1, drags)) * length;
            Vector3 right = body.FacingNow * Vector3.right;
            Vector3 place = joints[1] + right * (.1f * reach);
            place.y = up;
            // The stoop grows while the arm is short of that, and eases when it has length to spare.
            float lacks = joints[1].y - up - .93f * reach;
            stoop = Mathf.Clamp(stoop + lacks * 320 * dt, 0, DragBow);
            body.Sink(Mathf.Clamp(body.SinkNow + (stoop >= DragBow - .5f || lacks < 0 ? lacks * .5f : 0), 0, DragSink * body.StandingHipHeight));
            hands.WantEnd(1, Vector3.Lerp(endFrom, place, Mathf.SmoothStep(0, 1, endClock / .9f)));

            // What it pulls with: what the hand gives the tool along the ground, the way the body is going.
            Vector3 gives = hands.Gives(1), going = body.VelocityNow;
            gives.y = 0; going.y = 0;
            float pulls = going.sqrMagnitude > .01f ? Mathf.Max(0, Vector3.Dot(gives, going.normalized)) : 0;
            Pull = Mathf.Lerp(Pull, pulls, 1 - Mathf.Exp(-dt / .3f));
            float can = Pulls * physical.Mass * g * physical.Strength * physical.Fresh(PhysicalBody.Muscles.Legs);
            Pace = Mathf.Clamp(1 - Pull / Mathf.Max(1, can), .05f, 1);
            if (motor != null)
            {
                motor.SetMovementRate(Pace);
                // What it cannot move, it leaves.
                stuck = Pace < Stuck && motor.IsMoving ? stuck + dt : 0;
                if (stuck > GivesUp) Leave();
            }
        }

        // Taking up a tool that lies: it walks to stand with the place its hand will take a little ahead of it and to
        // that hand's side, bends down until the hand reaches, takes hold, and then carries it (or drags it).
        private void Take(float dt, System.Span<Vector3> joints)
        {
            fetchClock += dt;
            Pull = 0; Pace = 1;
            if (motor != null) motor.SetMovementRate(1);
            float reach = body.ArmReach;
            float along = fetchHand == 0 ? hands.HighestGrip : hands.LowestGrip;
            Vector3 grip = hands.PlaceAlong(along);
            if (!walked)
            {
                walked = true;
                Vector3 to = grip - transform.position;
                to.y = 0;
                Vector3 toward = to.sqrMagnitude > .01f ? to.normalized : body.FacingNow * Vector3.forward;
                Vector3 rightOfWay = Vector3.Cross(Vector3.up, toward);
                // Bent over, its shoulder is ahead of its feet by most of its own height over the hips: the place is to
                // be under it.
                float aside = body.ShoulderFromHips.x + .02f * reach, ahead = body.ShoulderFromHips.y * .8f;
                Vector3 stand = grip - toward * ahead + rightOfWay * (fetchHand == 0 ? aside : -aside);
                stand.y = transform.position.y;
                faces = toward;
                if (agent == null) agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && stoppedWithin < 0) { stoppedWithin = agent.stoppingDistance; agent.stoppingDistance = StandsWithin; }
                if (motor != null && Vector3.Distance(stand, transform.position) > StandsWithin + .06f) motor.TryMove(stand);
            }
            if (!arrived && motor != null && motor.IsMoving)
            {
                // On its way: upright.
                fetchClock = 0;
                if (back != null) back.Want(0);
                body.Sink(0);
                return;
            }
            // There: it stays, turns the way it came, goes down, and reaches.
            if (!arrived) { arrived = true; if (motor != null) motor.Stop(); }
            if (motor != null) motor.Face(transform.position + faces, dt);
            if (back != null) back.Want(stoop);
            float lacks = BendTo(joints[fetchHand], grip, dt);
            if (!hands.Holds(fetchHand) && !hands.Reaching(fetchHand) && lacks < .01f) hands.Grasp(fetchHand, along);
            // Down as far as it bows at first, and the place is not under its shoulder: it steps nearer, and goes down
            // again.
            Vector3 off = grip - joints[fetchHand];
            off.y = 0;
            if (!hands.Holds(fetchHand) && !hands.Reaching(fetchHand) && nearer < 3 && fetchClock > .9f && lacks > .02f && off.magnitude > .09f
                && motor != null && motor.TryMove(transform.position + off))
            {
                nearer++; arrived = false; fetchClock = 0;
                return;
            }
            if (hands.Holds(fetchHand))
            {
                // It has it: it carries it, or drags it.
                fetching = null;
                Walks();
                Go(fetchHand == 0 ? Way.OneHand : Way.Dragged);
                if (fetchHand == 1) { endFrom = hands.GripPlace(1); endClock = 0; hands.WantEnd(1, endFrom); }
                return;
            }
            // It cannot get to it: it leaves it.
            if (fetchClock > 6)
            {
                var lying = fetching;
                fetching = null;
                Walks();
                hands.Drop();
                Lies = lying != null ? lying.GetComponent<Rigidbody>() : null;
                way = Way.Left;
            }
        }

        // It walks as it does again (it stops as near a place as a walk does).
        private void Walks()
        {
            if (agent != null && stoppedWithin >= 0) agent.stoppingDistance = stoppedWithin;
            stoppedWithin = -1;
        }

        // It lets go of the tool, which stays where it lies.
        public void Leave()
        {
            if (hands == null || hands.Held == null) return;
            Lies = hands.Drop();
            way = Way.Left; Pull = 0; Pace = 1;
            if (motor != null) motor.SetMovementRate(1);
        }
    }
}
