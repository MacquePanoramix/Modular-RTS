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
        // Why it did not take up a tool it went for, if it did not.
        public string NotTaken { get; private set; } = "";

        private PhysicalBody physical;
        private PhysicalBalance balance;
        private UnitMotor motor;
        private float clock, stuck, bowFrom, drags, stoop;
        private Vector3 from, endFrom;
        private Quaternion fromTurn;
        private float endClock;
        // Laying it down, and going to take one up: the body goes down to the ground for both. Bent all a body
        // bends, it bows this far and its knees are bent to this share of the hips' height.
        private const float GroundBow = 78, GroundSink = .6f;
        // Standing up with what it took, its knees and its back straighten through the same stretch of time: this
        // long (seconds) for a body with its hips 0.7 m up, longer for a larger one.
        private const float StandsUpIn = 1.1f;
        private float sinkFrom;
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
            NotTaken = ""; shortFor = 0; kneesHeld = false; bentAll = false; beyond = false; beyondFor = 0;
            laying = false; Lies = null; stoop = 0; down = 0; downPace = 0;
        }

        // The body bends down until a shoulder is near enough a place for its hand to reach it. It is one movement:
        // the back bows and the knees bend through the same stretch of time, the bow a little ahead (the hips go back
        // before the knees come forward), beginning and ending little by little. (It bowed with its knees straight,
        // stopped, bent its knees, stopped, and bowed again. Luis, October 8: "more like an animation also in parts,
        // not a real physical thing".) How far it has to go is worked out from how it stands: the least that brings
        // the shoulder within the arm's reach of the place. Its knees go no deeper than one of them could hold the
        // body alone (PhysicalBalance); what that leaves is the back's, or is out of its reach.
        // down: how far down it is, from standing (0) to bent all a body bends (1). It gets there in about three
        // times GoesDownIn (seconds, for a body with its hips 0.7 m up).
        private float down, downPace;
        private const float GoesDownIn = .45f, DownAtMost = 2.4f, Spare = .006f, GoesFurther = .1f;
        private static float BowAt(float d) => GroundBow * (1 - Mathf.Pow(1 - Mathf.Clamp01(d), 1.5f));
        private float SinkAt(float d) => GroundSink * body.StandingHipHeight * Mathf.Clamp01(d);
        // Where a shoulder would be with the body that far down (its knees bent no deeper than `knees`), from how it
        // stands now.
        private Vector3 ShoulderAt(float d, float knees, Vector3 hips, Quaternion posture, Vector3 shoulder)
        {
            float bowNow = back != null ? back.BowNow : body.BowNow;
            return hips + Vector3.down * (Mathf.Min(SinkAt(d), knees) - body.SinkNow)
                   + Quaternion.AngleAxis(BowAt(d) - bowNow, posture * Vector3.right) * (shoulder - hips);
        }
        private float BendTo(Vector3 hips, Quaternion posture, Vector3 shoulder, Vector3 place, float dt, float further = 0)
        {
            float reach = Stretches * body.ArmReach;
            float lacks = Vector3.Distance(shoulder, place) - reach;
            float deepest = GroundSink * body.StandingHipHeight;
            float asked = KneesAsked;
            bool may = asked < PhysicalBalance.Raises;
            if (!may) kneesHeld = true;
            // (Asked more than they may be, the knees come up.)
            float knees = may ? deepest : asked > PhysicalBalance.Raises + KneesOver ? Mathf.Max(0, body.SinkNow - KneesComeUp * dt) : body.SinkNow;
            float Short(float d) => Vector3.Distance(ShoulderAt(d, knees, hips, posture, shoulder), place) - reach + Spare;
            float need = 1, all = Short(1);
            if (Short(0) <= 0) need = 0;
            else if (all <= 0)
            {
                float least = 0;
                for (int k = 0; k < 12; k++)
                {
                    float middle = (least + need) * .5f;
                    if (Short(middle) > 0) least = middle; else need = middle;
                }
            }
            // Its knees stopped well short of the deepest bend, and bent all it could it would still be short of the
            // place: it is out of this body's reach, and it does not bow down to make sure.
            beyondFor = !may && body.SinkNow < WellShort * deepest && all > OutOfReach ? beyondFor + dt : 0;
            if (beyondFor > .3f) beyond = true;
            need = Mathf.Min(1, need + further);
            if (beyond) need = Mathf.Min(need, down);
            down = Mathf.SmoothDamp(down, need, ref downPace, GoesDownIn * body.StepSize, DownAtMost, dt);
            stoop = BowAt(down);
            body.Sink(Mathf.Min(SinkAt(down), knees));
            bentAll = lacks >= .01f && down >= .985f && (!may || body.SinkNow >= deepest - .006f);
            return lacks;
        }
        // Its feet stay as they stand: it does not bring them together, and takes no step to catch itself. (If its
        // weight stays outside its feet, it still falls.)
        private void KeepsItsFeet()
        {
            body.StaysPut = true;
            if (balance != null) balance.KeepsFeet = true;
        }
        // Upright: its back straight, and its knees.
        private void Stands()
        {
            if (back != null) back.Want(0);
            body.Sink(0);
            stoop = 0; down = 0; downPace = 0;
        }

        // What its knees are asked now, as a share of what they have, with the tool it has or is taking up; and
        // whether they may bend deeper, of its own accord.
        private float KneesAsked => balance != null ? balance.KneesAsked(hands != null ? hands.ToolMass : 0) : 0;
        // Asked this much more than they may be, the knees come up, at this pace (metres a second).
        private const float KneesOver = .02f, KneesComeUp = .3f;
        // It has stood up when its knees are bent less than this share of its hips' height and it bows less than this
        // (degrees). With its knees bent more than the third share, it is bent to the ground.
        private const float StoodUpSink = .05f, StoodUpBow = 15, KeepsFeetBelow = .25f;
        // Bent as far as it can bend and still short of the tool, for this long (seconds), it gives it up. So it does
        // if its knees give way under it as it reaches, by this share of its hips' height.
        private const float GivesUpShort = 1.2f, GivesUnder = .12f;
        private float shortFor;
        // Whether it is bent all it can; whether its knees held it short of the deepest bend, going for this tool;
        // and whether the tool is beyond what bending all it can would reach (by more than this, metres).
        private bool bentAll, kneesHeld, beyond;
        private float beyondFor;
        // Going for a tool: whether its feet are set for the bend; for how long they have stood still together
        // (seconds), and how long it has been setting them. They are set when they have stood so for this long, the
        // body facing within this many degrees of the way it means to; it waits for that no longer than this.
        private bool feetSet, wasDown;
        private float setFor, settling;
        private const float SetsFor = .12f, SetsAtMost = 2.5f, FacesWithin = 3;
        private const float OutOfReach = .03f, WellShort = .7f;

        private void Awake()
        {
            physical = GetComponent<PhysicalBody>();
            motor = GetComponent<UnitMotor>();
        }

        private void OnEnable() { clock = -1; stuck = 0; Pull = 0; Pace = 1; drags = 0; stoop = 0; down = 0; downPace = 0; laying = false; fetching = null; }

        private void OnDisable()
        {
            if (balance != null) balance.KeepsFeet = false;
            if (body != null) { body.StaysPut = false; body.RegardNothing(); }
            wasDown = false;
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
            if (body != null) { bowFrom = body.BowNow; sinkFrom = body.SinkNow; }
            down = 0; downPace = 0;
        }

        // How far out from under its shoulder the hand carries a tool at the side (m): the arm hangs, as near to
        // straight down as the hip and whatever hangs there let it. (It was held a fifth of the arm's length out:
        // a long arm then held itself and the tool up all the while, and did not rest so. October 8.)
        public static float Aside(ProceduralBiped body) => .05f * body.ArmReach + body.BodyProportions.armCarry.x;
        // How much of its length the arm hangs at, carrying at the side.
        private const float Hangs = .995f;

        // The tool carried at the side in one hand (the left): level, its head ahead and its point down, the arm
        // hanging, and all but straight: a bent elbow holds the tool up by its own strength, a straight arm by its bones.
        public static void AtSide(ProceduralBiped body, PhysicalHands hands, Vector3 hips, Quaternion posture, out Vector3 position, out Quaternion rotation)
        {
            float reach = body.ArmReach;
            Vector3 shoulder = body.ShoulderFromHips;
            float aside = Aside(body), ahead = .06f * reach;
            float drop = Mathf.Sqrt(Mathf.Max(.01f, Hangs * reach * Hangs * reach - aside * aside - ahead * ahead));
            rotation = posture * Quaternion.LookRotation(Vector3.down, Vector3.forward);
            Vector3 hand = hips + posture * new Vector3(-(shoulder.x + aside), shoulder.y - drop, shoulder.z + ahead);
            position = hand - rotation * new Vector3(0, hands.GripAlong(0), 0);
        }

        private void FixedUpdate()
        {
            // A body that is down, or getting up, is not the carry's to stand up.
            if (fall == null && !lookedForFall) { fall = GetComponent<PhysicalFall>(); lookedForFall = true; }
            if (balance == null) balance = GetComponent<PhysicalBalance>();
            // (It looks at what it goes down for, and only then: below.)
            if (body != null) body.RegardNothing();
            if (fall != null && fall.Now != PhysicalFall.State.Up) { if (balance != null) balance.KeepsFeet = false; if (body != null) body.StaysPut = false; return; }
            // Bent to the ground (for a tool it takes up or lays down), its feet stay where they are, until it has
            // stood up again (or is sent somewhere).
            if (body != null)
            {
                float hipsUp = body.StandingHipHeight;
                if (body.SinkNow > KeepsFeetBelow * hipsUp) wasDown = true;
                else if ((body.SinkNow < StoodUpSink * hipsUp && Mathf.Abs(body.BowNow) < StoodUpBow) || (motor != null && motor.IsMoving)) wasDown = false;
                if (balance != null) balance.KeepsFeet = wasDown;
                // (Nor does it bring its feet together before it goes down, or while it comes up: below.)
                body.StaysPut = wasDown;
            }
            if (hands == null || hands.Held == null || body == null)
            {
                // The tool has slipped from its hand: it lies where it fell.
                if (hands != null && hands.Slipped != null && way != Way.Left && clock >= 0) { Lies = hands.Slipped; way = Way.Left; Pull = 0; Pace = 1; }
                if (motor != null) motor.SetMovementRate(1);
                // With nothing in its hands it stands up.
                laying = false; fetching = null; stoop = 0; down = 0; downPace = 0;
                if (back != null) back.Want(0);
                if (body != null) body.Sink(0);
                return;
            }
            float dt = Time.fixedDeltaTime, g = Physics.gravity.magnitude;
            // It is not set for work: its feet come together, once it stands up. (Coming from its work it is still
            // bowed and its knees bent, the tool out before it: its feet stay set apart under that until it is up.)
            if (balance == null) balance = GetComponent<PhysicalBalance>();
            if (balance != null && (body.SinkNow < StoodUpSink * body.StandingHipHeight && Mathf.Abs(body.BowNow) < StoodUpBow)) balance.Ease();
            // (The first two of these are the shoulders, the left then the right.)
            System.Span<Vector3> joints = stackalloc Vector3[4];
            body.StandsAt(Time.time, out Vector3 hips, out Quaternion posture, joints.Slice(0, 2), joints.Slice(2, 2));
            // How much of a hand's hold the tool's weight asks: that says how it is held.
            Asks = hands.ToolMass * g / Mathf.Max(1e-3f, physical.HoldOf(0));
            if (fetching != null) { Take(dt, joints, hips, posture); return; }
            if (clock < 0) Go(Asks > Carries ? Way.Dragged : Way.OneHand);
            else if (way == Way.OneHand && Asks > Carries) Go(Way.Dragged);
            else if (way == Way.Dragged && Asks < CarriesAgain) Go(Way.OneHand);
            // Told to lay down a tool it can only hold by its handle's end: that one lies already. It lets go.
            if (laying && way == Way.Dragged) { laying = false; Leave(); return; }
            clock += dt;
            // Carrying, it stands up as the tool comes with it; dragging, it stoops to it.
            drags = Mathf.MoveTowards(drags, way == Way.Dragged && motor != null && motor.IsMoving ? 1 : 0, dt / .5f);
            bool lays = laying && way == Way.OneHand && hands.Holds(0) && !hands.Holds(1);
            if (way != Way.Dragged && !lays) stoop = 0;
            float stood = Mathf.SmoothStep(0, 1, clock / (StandsUpIn * body.StepSize));
            if (back != null) back.Want(Mathf.Lerp(bowFrom, stoop, stood));

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
                BendTo(hips, posture, joints[0], hand, dt);
                body.Regard(hand);
                KeepsItsFeet();
                AtSide(body, hands, hips, posture, out var carried, out var carriedTurn);
                float down = Mathf.SmoothStep(0, 1, layClock / 1.5f);
                hands.Want(Vector3.Lerp(carried, lies, down), Quaternion.Slerp(carriedTurn, flat, down));
                bool there = hands.GripPlace(0).y - hand.y < .03f && hands.Held.linearVelocity.sqrMagnitude < .02f;
                if (layClock > 1.5f && (there || layClock > 5))
                {
                    // It lies: the hand opens, and then the arm takes it away.
                    hands.Open(0);
                    opened += dt;
                    if (opened > OpensIn)
                    {
                        Lies = hands.Drop();
                        way = Way.Left; laying = false;
                    }
                }
                else opened = 0;
                return;
            }

            if (way == Way.OneHand)
            {
                // (From the ground with what it took: its knees straighten as its back does.)
                body.Sink(Mathf.Lerp(sinkFrom, 0, stood));
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
            float knees = Mathf.Clamp(body.SinkNow + (stoop >= DragBow - .5f || lacks < 0 ? lacks * .5f : 0), 0, DragSink * body.StandingHipHeight);
            // (Its knees bend no deeper than one of them could hold alone.)
            body.Sink(balance == null ? knees : balance.KneesBendTo(knees, hands.ToolMass, dt));
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
        // that hand's side, looking at it; goes down, its hand going out to it as it does; takes hold, and then carries
        // it (or drags it).
        private float lacksFrom, opened, rolled;
        private const float OpensIn = .16f;
        private void Take(float dt, System.Span<Vector3> joints, Vector3 hips, Quaternion posture)
        {
            fetchClock += dt;
            Pull = 0; Pace = 1;
            if (motor != null) motor.SetMovementRate(1);
            float reach = body.ArmReach;
            float along = fetchHand == 0 ? hands.HighestGrip : hands.LowestGrip;
            Vector3 grip = hands.PlaceAlong(along);
            body.Regard(grip);
            if (!walked)
            {
                walked = true;
                Vector3 to = grip - transform.position;
                to.y = 0;
                // Bent over, its shoulder is ahead of its feet by a good half of its own height over the hips (its
                // hips go back as it bows): the place is to be under it, a little ahead. (Measured, October 8: 0.50
                // to 0.53 of that height for the three miners. It stood for 0.8, and reached forward for the rest.)
                float aside = body.ShoulderFromHips.x + .02f * reach, ahead = body.ShoulderFromHips.y * .6f;
                Vector3 toward = to.sqrMagnitude > .01f ? to.normalized : body.FacingNow * Vector3.forward;
                // It walks the way it will face there, so that it need not turn again when it arrives: the place is
                // to be to its hand's side of the line it walks, so it walks a little to the other side of it. (It
                // walked at the place, and then turned back: October 8.)
                if (to.magnitude > aside * 1.5f)
                    toward = Quaternion.AngleAxis(Mathf.Asin(aside / to.magnitude) * Mathf.Rad2Deg * (fetchHand == 0 ? 1 : -1), Vector3.up) * toward;
                Vector3 rightOfWay = Vector3.Cross(Vector3.up, toward);
                Vector3 stand = grip - toward * ahead + rightOfWay * (fetchHand == 0 ? aside : -aside);
                stand.y = transform.position.y;
                faces = toward;
                if (agent == null) agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && stoppedWithin < 0) { stoppedWithin = agent.stoppingDistance; agent.stoppingDistance = StandsWithin; }
                if (motor != null && Vector3.Distance(stand, transform.position) > StandsWithin + .06f) motor.TryMove(stand);
            }
            // (Its way done, it comes to rest before it does anything else: it was stopped where it was, at once.)
            bool going = motor != null && motor.IsMoving;
            if (going) rolled = 0;
            else if (!arrived && motor != null && rolled < 1 && !motor.ComeToRest()) { rolled += dt; going = true; }
            if (!arrived && motor != null && going)
            {
                // On its way: upright.
                fetchClock = 0;
                Stands();
                return;
            }
            if (!arrived) { arrived = true; if (motor != null) motor.Stop(); feetSet = false; setFor = 0; settling = 0; }
            Vector3 off = grip - joints[fetchHand];
            off.y = 0;
            if (!feetSet)
            {
                // There: still upright, it turns the way it came, and its feet come to rest (both down, none still to
                // be put in its place). It does not go down before that: a foot lifted while the knees are bent leaves
                // the other knee the whole body. And it does not turn again while it is down: turning lifts its feet.
                if (motor != null) motor.Face(transform.position + faces, dt);
                settling += dt;
                bool turned = Vector3.Angle(Vector3.ProjectOnPlane(transform.forward, Vector3.up), faces) < FacesWithin;
                Stands();
                // (As its feet stand when it gets there: it does not bring them together first.)
                KeepsItsFeet();
                setFor = turned && body.FootPlanted(0) && body.FootPlanted(1) && body.LiftDue < 0 ? setFor + dt : 0;
                if (setFor <= SetsFor && settling <= SetsAtMost) return;
                // Before it goes down: bent all it could, would its hand come to the place? If it would not, and the
                // place is not under where its shoulder would be, it steps nearer first.
                Vector3 would = ShoulderAt(1, GroundSink * body.StandingHipHeight, hips, posture, joints[fetchHand]);
                Vector3 still = grip - would;
                still.y = 0;
                if (turned && nearer < 3 && Vector3.Distance(would, grip) > Stretches * reach + .02f && still.magnitude > .09f
                    && motor != null && motor.TryMove(transform.position + still))
                {
                    nearer++; arrived = false; fetchClock = 0;
                    return;
                }
                feetSet = true; fetchClock = 0;
            }
            KeepsItsFeet();
            // Its hand goes out to the place as it comes down, and closes on it as it arrives.
            // (It is there when its arm, all but straight, has it. Until then the body is going a little further
            // down than it need, so that the hand arrives while the body still moves, and not at the end of a long
            // slowing; then it stops where it need be.)
            float straight = .5f * (1 - Stretches) * reach;
            bool has = hands.Holds(fetchHand);
            float lacks = BendTo(hips, posture, joints[fetchHand], grip, dt, has || Vector3.Distance(joints[fetchHand], grip) - Stretches * reach < straight + .004f ? 0 : GoesFurther);
            bool there = lacks < straight + .004f;
            if (back != null) back.Want(stoop);
            if (!has)
            {
                if (!hands.Reaching(fetchHand)) { hands.GraspLed(fetchHand, along); lacksFrom = Mathf.Max(lacks, straight + .03f); }
                hands.Lead(fetchHand, 1 - (lacks - straight) / (lacksFrom - straight));
            }
            // Down, a good while, and still the place is not under its shoulder: it stands up, steps nearer, and goes
            // down again.
            if (!has && !there && nearer < 3 && fetchClock > 1.8f && lacks > .02f && off.magnitude > .09f
                && motor != null && motor.TryMove(transform.position + off))
            {
                hands.Withdraw(fetchHand);
                nearer++; arrived = false; fetchClock = 0;
                return;
            }
            if (has)
            {
                // It has it: it carries it, or drags it.
                fetching = null;
                Walks();
                Go(fetchHand == 0 ? Way.OneHand : Way.Dragged);
                if (fetchHand == 1) { endFrom = hands.GripPlace(1); endClock = 0; hands.WantEnd(1, endFrom); }
                return;
            }
            // It cannot get to it: it leaves it. (Bent all it can and still short of it, it does not wait long.)
            bool giving = balance != null && balance.GaveNow > GivesUnder * body.StandingHipHeight;
            shortFor = !there && bentAll ? shortFor + dt : 0;
            if (fetchClock > 6 || shortFor > GivesUpShort || giving || (beyond && !there))
            {
                NotTaken = giving ? "Its knees gave under it as it reached for the pickaxe"
                    : kneesHeld ? "Its legs would not raise it again from as far down as the pickaxe lies" : "It could not reach the pickaxe";
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
