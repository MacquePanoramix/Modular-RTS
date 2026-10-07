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

        private void Awake()
        {
            physical = GetComponent<PhysicalBody>();
            motor = GetComponent<UnitMotor>();
        }

        private void OnEnable() { clock = -1; stuck = 0; Pull = 0; Pace = 1; drags = 0; stoop = 0; }

        private void OnDisable()
        {
            if (motor != null) motor.SetMovementRate(1);
            Pace = 1; Pull = 0;
        }

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
            if (hands == null || hands.Held == null || body == null)
            {
                // The tool has slipped from its hand: it lies where it fell.
                if (hands != null && hands.Slipped != null && way != Way.Left && clock >= 0) { Lies = hands.Slipped; way = Way.Left; Pull = 0; Pace = 1; }
                if (motor != null) motor.SetMovementRate(1);
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
            if (clock < 0) Go(Asks > Carries ? Way.Dragged : Way.OneHand);
            else if (way == Way.OneHand && Asks > Carries) Go(Way.Dragged);
            else if (way == Way.Dragged && Asks < CarriesAgain) Go(Way.OneHand);
            clock += dt;
            // Carrying, it stands up as the tool comes with it; dragging, it stoops to it.
            drags = Mathf.MoveTowards(drags, way == Way.Dragged && motor != null && motor.IsMoving ? 1 : 0, dt / .5f);
            if (way != Way.Dragged) stoop = 0;
            if (back != null) back.Want(Mathf.Lerp(bowFrom, stoop, Mathf.SmoothStep(0, 1, clock / 1.1f)));

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
            float up = transform.position.y + Mathf.Lerp(HandUpStanding, HandUp, Mathf.SmoothStep(0, 1, drags)) * length;
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
