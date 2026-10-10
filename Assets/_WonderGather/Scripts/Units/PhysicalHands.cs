using System;
using UnityEngine;

namespace WonderGather
{
    // A real object in a body's hands (S3: Docs/Design/ThePhysicalBody.md).
    //
    // The object is a body in the physics, with its own weight, balance and resistance to turning. The hands do not
    // place it. They push and turn it towards where it is meant to be, and each can give only what its arm can:
    // a push at the hand asks a turning force of the shoulder and of the elbow, on top of what the arm's own weight
    // asks, and neither joint gives more than its capacity (PhysicalBody). So what an arm can do depends on how it is
    // held: little at full stretch in front of the body, much with the load hanging or drawn in close. A wrist turns
    // the object only as far as a wrist can. Two hands turn it by pushing opposite ways, the more easily the further
    // apart they are.
    //
    // The arms then follow the object (IArmGuide): where it is, there the hands are. An arm's length is the one thing
    // the object cannot get past: a link from each shoulder keeps the held place within reach.
    [DefaultExecutionOrder(500)]
    [RequireComponent(typeof(ProceduralBiped))]
    public sealed class PhysicalHands : MonoBehaviour, IArmGuide
    {
        // How briskly the muscles answer (1/s): a held thing closes on where it is meant to be at this rate, without
        // overshooting, when nothing limits them. About a fifth of a second.
        private const float Quick = 14, QuickTurn = 16;
        // An all-out push is as urgent as the physics' clock allows: it asks for more than the arms have, so they give
        // all they have.
        private const float Hard = 30;
        // A muscle gives less the faster it shortens, and nothing at its fastest. The fastest a hand moves, in arm's
        // lengths a second.
        private const float Fastest = 14;
        // A muscle that is being forced back resists with more than it can push with.
        private const float Braking = 1.5f;
        // How fast a hand slides along a handle, in metres a second.
        private const float SlidePace = 1.6f;
        // The arm's own mass that rides on the handle with each hand: the hand, and these shares of the forearm and
        // of the upper arm (the rest turns about the elbow and the shoulder, and hardly moves with the hand). It is
        // held up with the tool, and falls with it in a blow.
        private const float ForearmRides = .5f, UpperArmRides = .12f;
        private ProceduralBiped body;
        private IHandHolds holds;
        private PhysicalBody physical;
        private ToolDefinition tool;
        private Rigidbody held;
        private Transform model;
        private HeldThing thing;
        private readonly bool[] on = new bool[2];
        // Where each hand holds, on the handle, in the tool's own space.
        private readonly Vector3[] grip = new Vector3[2];
        private readonly Rigidbody[] anchors = new Rigidbody[2];
        private readonly ConfigurableJoint[] links = new ConfigurableJoint[2];
        private readonly float[] effort = new float[2], miss = new float[2], rides = new float[2], slideTo = new float[2], reach = new float[2];
        // A reach for the handle: how long it takes, and where the wrist was when it began.
        private const float ReachTime = .35f;
        private readonly Vector3[] reachFrom = new Vector3[2];
        // The span of the handle a hand's middle can hold (from above its foot to below its head), a fist's
        // half-width, and the share of the tool's own weight it was taken at.
        private float lowest, highest, fist, ofItsWeight = 1;
        // The tool's own weight (what is held weighs more: the arms' mass rides on it).
        private float toolMass;
        public float ToolMass => toolMass;
        private readonly Vector3[] push = new Vector3[2];
        private Vector3 wantPosition, lastWantCentre;
        private Quaternion wantRotation = Quaternion.identity, lastWantRotation = Quaternion.identity;
        private bool wanting, wantedBefore, stood, hard;
        // The share of the thing's weight the hands mean to carry (1: all of it; less when it rests on something).
        private float carry = 1;

        public Rigidbody Held => held;
        public HeldThing Thing => thing;
        public ToolDefinition Tool => tool;
        public bool Holds(int hand) => held != null && on[hand];
        // Where along the handle a hand holds (the tool's own y), and the span it can hold.
        public float GripAlong(int hand) => grip[hand].y;
        public float LowestGrip => lowest;
        public float HighestGrip => highest;
        // A hand slides along the handle to another place, at its own pace. While it slides it holds loosely: it
        // steadies the handle but does not pull along it.
        public void Slide(int hand, float along) { if (held != null && on[hand]) slideTo[hand] = Mathf.Clamp(along, lowest, highest); }
        public bool Sliding(int hand) => held != null && on[hand] && Mathf.Abs(slideTo[hand] - grip[hand].y) > .002f;
        // How thick the handle is where a hand holds it: it tapers evenly between the two places it was measured at.
        public float RadiusAt(float along)
        {
            float a = tool.PrimaryGrip.y, b = tool.SecondaryGrip.y, ra = tool.GripRadius(1), rb = tool.GripRadius(0);
            float r = Mathf.LerpUnclamped(ra, rb, (along - a) / (b - a));
            return Mathf.Clamp(r, Mathf.Min(ra, rb) * .85f, Mathf.Max(ra, rb) * 1.12f);
        }
        // The share of its capacity the hardest-worked joint of this arm gave at the last step (1: all it has).
        public float Effort(int hand) => effort[hand];
        // What that effort was made of: the shares its shoulder, its elbow, its wrist and its hold gave.
        public Vector4 EffortOf(int hand) => efforts[hand];
        private readonly Vector4[] efforts = new Vector4[2];
        // What this hand pushed the object with at the last step, in newtons.
        public Vector3 Push(int hand) => push[hand];
        // The share of its hold this hand gave at the last step (1: all it has).
        public float Hold(int hand) => hold[hand];
        private readonly float[] hold = new float[2];
        // A hold asked for more than it has (even braced against a pull) for longer than this is overcome: the thing
        // slips from that hand. From its last hand, it falls, and lies where it falls.
        private const float SlipsAfter = .3f;
        private readonly float[] slips = new float[2];
        public Rigidbody Slipped { get; private set; }
        // All this arm gave the object at the last step: its push, and what the arm's own length held it back with
        // (an object hanging from a straight arm is held by the arm's bones, not by a push).
        public Vector3 Gives(int hand) => held != null && on[hand] ? push[hand] + (links[hand] != null ? links[hand].currentForce * LinkGives : Vector3.zero) : Vector3.zero;
        // The engine reports a link's force as it acts on the object (measured: a weight hanging from such a link
        // reports its own weight, upwards).
        private const float LinkGives = 1;
        // The arm's own mass that rides on the handle with this hand, in kilograms.
        public float Rides(int hand) => held != null && on[hand] ? rides[hand] : 0;
        // How far the hand's own hold was from the place it holds on the handle, when last drawn.
        public float Miss(int hand) => miss[hand];
        public Vector3 HeadPosition => held != null ? held.position + held.rotation * tool.Head : transform.position;
        // Where a hand holds, in the world (as the physics has the object now).
        public Vector3 GripPlace(int hand) => held != null ? held.position + held.rotation * grip[hand] : transform.position;
        // For measuring what this costs: the time spent in the steps of every body's hands since it was last cleared.
        public static bool Timed;
        public static long TimedTicks;
        public static int TimedSteps;
        public Vector3 HeadVelocity => held != null ? held.GetPointVelocity(HeadPosition) : Vector3.zero;

        private void Awake()
        {
            body = GetComponent<ProceduralBiped>();
            holds = GetComponent<IHandHolds>();
            physical = GetComponent<PhysicalBody>();
        }

        // A tool made as a real body in the world, at a place and a turn (the tool's own origin), in no hand: its
        // solid, its own weight, and what it is. weight: a share of its own weight, for trying the same tool lighter or
        // heavier.
        public static HeldThing Make(ToolDefinition definition, Vector3 position, Quaternion rotation, float weight = 1)
        {
            if (definition == null || !definition.HasWeight) throw new ArgumentException("Only a tool that has been weighed can be made.");
            var go = new GameObject(definition.DisplayName + " (held)");
            go.transform.SetPositionAndRotation(position, rotation);
            var model = Instantiate(definition.Prefab, go.transform).transform;
            model.localPosition = Vector3.zero;
            model.localRotation = Quaternion.identity;
            // Its solid: the handle from foot to top, and the head from point to point.
            var surface = new PhysicsMaterial("Tool") { bounciness = .1f, dynamicFriction = .5f, staticFriction = .6f, bounceCombine = PhysicsMaterialCombine.Minimum };
            var handle = go.AddComponent<CapsuleCollider>();
            handle.direction = 1;
            handle.radius = Mathf.Max(definition.GripRadius(0), definition.GripRadius(1));
            handle.height = definition.Top - definition.Foot;
            handle.center = new Vector3(0, (definition.Top + definition.Foot) * .5f, 0);
            handle.sharedMaterial = surface;
            var head = go.AddComponent<CapsuleCollider>();
            Vector3 point = definition.Point;
            float back = -point.z * .8f;
            head.direction = 2;
            head.radius = definition.HeadRadius;
            head.height = point.z - back;
            head.center = new Vector3(0, point.y, (point.z + back) * .5f);
            head.sharedMaterial = surface;
            var solid = go.AddComponent<Rigidbody>();
            solid.interpolation = RigidbodyInterpolation.Interpolate;
            // A pick's head moves fast by the tool's turning, not by its travelling: it is watched for ahead of each
            // step, so it is stopped at a surface and not found inside it afterwards.
            solid.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            solid.linearDamping = 0;
            solid.angularDamping = .05f;
            solid.maxAngularVelocity = 60;
            // On its own it weighs what it weighs.
            solid.mass = definition.Mass * weight;
            solid.centerOfMass = definition.Centre;
            solid.inertiaTensor = definition.Inertia * weight;
            solid.inertiaTensorRotation = definition.InertiaTurn;
            var made = go.AddComponent<HeldThing>();
            made.Head = head;
            made.Tool = definition; made.Weight = weight;
            return made;
        }

        // Takes a tool into the free hands, at a place and a turn (the tool's own origin). weight: a share of its own
        // weight, for trying the same tool lighter or heavier.
        public Rigidbody Take(ToolDefinition definition, Vector3 position, Quaternion rotation, float weight = 1)
        {
            Drop();
            Slipped = null;
            if (definition == null || !definition.HasWeight) throw new ArgumentException("Only a tool that has been weighed can be held.");
            if (holds == null || physical == null || !physical.Ready) throw new InvalidOperationException("This body has no hands of its own, or has not been weighed.");
            if (!holds.HandFree(0) && !holds.HandFree(1)) throw new InvalidOperationException("This body has no free hand.");
            thing = Make(definition, position, rotation, weight);
            var go = thing.gameObject;
            held = go.GetComponent<Rigidbody>();
            model = go.transform.GetChild(0);
            toolMass = definition.Mass * weight;
            ofItsWeight = weight;
            fist = Mathf.Max(definition.GripRadius(0), definition.GripRadius(1)) * 2.5f;
            lowest = definition.Foot + fist + .01f;
            highest = definition.Top - .07f * (definition.Top - definition.Foot) - fist;
            foreach (var mine in GetComponentsInChildren<Collider>(true))
                foreach (var its in go.GetComponents<Collider>()) Physics.IgnoreCollision(mine, its);
            thing.Holder = this;
            tool = definition;
            for (int i = 0; i < 2; i++)
            {
                on[i] = holds.HandFree(i);
                grip[i] = i == 0 ? definition.SecondaryGrip : definition.PrimaryGrip;
                slideTo[i] = grip[i].y;
                rides[i] = 0;
                reach[i] = 0;
                if (!on[i]) continue;
                Link(i, stood ? body.ShoulderNow(i) : position);
            }
            Weigh(weight);
            wanting = wantedBefore = false;
            body.GuideArms(this);
            return held;
        }

        // A tool that lies in the world (let go, or laid down) is to be taken up: no hand has it yet. A hand then
        // reaches for it (Grasp), and holds it once it is there.
        public void Adopt(HeldThing lying)
        {
            Drop();
            Slipped = null;
            if (lying == null || lying.Tool == null) throw new ArgumentException("Only a tool that knows what it is can be taken up.");
            if (holds == null || physical == null || !physical.Ready) throw new InvalidOperationException("This body has no hands of its own, or has not been weighed.");
            held = lying.GetComponent<Rigidbody>();
            model = held.transform.GetChild(0);
            thing = lying; tool = lying.Tool;
            thing.Holder = this;
            toolMass = tool.Mass * lying.Weight;
            ofItsWeight = lying.Weight;
            fist = Mathf.Max(tool.GripRadius(0), tool.GripRadius(1)) * 2.5f;
            lowest = tool.Foot + fist + .01f;
            highest = tool.Top - .07f * (tool.Top - tool.Foot) - fist;
            foreach (var mine in GetComponentsInChildren<Collider>(true))
                foreach (var its in held.GetComponents<Collider>()) Physics.IgnoreCollision(mine, its);
            for (int i = 0; i < 2; i++)
            {
                on[i] = false;
                grip[i] = i == 0 ? tool.SecondaryGrip : tool.PrimaryGrip;
                slideTo[i] = grip[i].y;
                rides[i] = 0; reach[i] = 0; led[i] = false;
            }
            Weigh(ofItsWeight);
            wanting = wantedBefore = false; trailing = false;
            body.GuideArms(this);
        }
        // Where a hand would hold it, along its handle, in the world (whether or not a hand is there).
        public Vector3 PlaceAlong(float along) => held != null ? held.position + held.rotation * new Vector3(0, along, 0) : transform.position;

        // A hand that holds: the arm's mass that rides on the handle with it, and the link from its shoulder (an arm's
        // length is the one thing the object cannot get past).
        private void Link(int i, Vector3 shoulder)
        {
            Span<Vector3> partAt = stackalloc Vector3[3];
            Span<float> partMass = stackalloc float[3];
            physical.ArmParts(i, partAt, partMass);
            rides[i] = partMass[2] + partMass[1] * ForearmRides + partMass[0] * UpperArmRides;
            var anchor = new GameObject(i == 0 ? "Left shoulder (link)" : "Right shoulder (link)").AddComponent<Rigidbody>();
            anchor.isKinematic = true;
            anchor.position = shoulder;
            anchors[i] = anchor;
            var link = held.gameObject.AddComponent<ConfigurableJoint>();
            link.autoConfigureConnectedAnchor = false;
            link.connectedBody = anchor;
            link.anchor = grip[i];
            link.connectedAnchor = Vector3.zero;
            link.xMotion = link.yMotion = link.zMotion = ConfigurableJointMotion.Limited;
            link.linearLimit = new SoftJointLimit { limit = body.ArmReach - .004f, contactDistance = .005f };
            link.angularXMotion = link.angularYMotion = link.angularZMotion = ConfigurableJointMotion.Free;
            links[i] = link;
        }

        // One hand lets go, and the other goes on holding.
        public void Release(int hand)
        {
            if (held == null || !on[hand] || !on[1 - hand]) return;
            holds.HoldHandle(hand, false, Vector3.zero, Vector3.up, 0, Vector3.zero);
            if (links[hand] != null) Destroy(links[hand]);
            if (anchors[hand] != null) Destroy(anchors[hand].gameObject);
            links[hand] = null; anchors[hand] = null; on[hand] = false;
            rides[hand] = 0; effort[hand] = 0; push[hand] = Vector3.zero; miss[hand] = 0;
            Weigh(ofItsWeight);
        }

        // A free hand reaches for the handle and takes hold of it at a place along it. The reach takes a moment; the
        // hand holds, and pushes, only once it is there.
        public void Grasp(int hand, float along)
        {
            if (held == null || on[hand] || reach[hand] > 0 || !holds.HandFree(hand)) return;
            // The first hand to take a thing that lies: until it is there the thing is no one's to push.
            if (!on[1 - hand]) { wanting = wantedBefore = false; trailing = false; }
            grip[hand] = new Vector3(0, Mathf.Clamp(along, lowest, highest), 0);
            slideTo[hand] = grip[hand].y;
            reachFrom[hand] = body.HandPosition(hand);
            reach[hand] = 1e-4f; reachWas[hand] = 0; reachStepped = Time.time;
            led[hand] = false; opening[hand] = false;
        }
        public bool Reaching(int hand) => reach[hand] > 0;

        // A reach the body leads. Bending down to a thing, the hand goes out to it as the body comes down: as far
        // along its way as the body says (Lead: 0 to 1), never back, and no faster than a hand reaches. Its fingers
        // open on the way and close as it arrives (from this share of the way), and it holds once they have closed
        // (this long after it is there, seconds). (The hand waited until the body was all the way down, and then
        // went out: Luis, October 8.)
        private readonly bool[] led = new bool[2];
        private readonly float[] reachWas = new float[2];
        private float reachStepped;
        // A hand that holds opens, to let go: its fingers open while it is still where it held. (What lets go then
        // drops the thing: Drop.)
        private readonly bool[] opening = new bool[2];
        public void Open(int hand) { if (on[hand]) opening[hand] = true; }
        private readonly float[] ledTo = new float[2];
        private const float LedCloses = .7f, LedHolds = .12f;
        public void GraspLed(int hand, float along)
        {
            Grasp(hand, along);
            if (reach[hand] <= 0) return;
            led[hand] = true; ledTo[hand] = 0;
        }
        public void Lead(int hand, float share) { if (led[hand]) ledTo[hand] = Mathf.Max(ledTo[hand], Mathf.Clamp01(share)); }
        // A hand that was reaching gives it up.
        public void Withdraw(int hand)
        {
            if (reach[hand] <= 0) return;
            reach[hand] = 0; led[hand] = false;
            if (holds != null) holds.HoldHandle(hand, false, Vector3.zero, Vector3.up, 0, Vector3.zero);
        }

        // What is held weighs what the tool does and what rides on it with each hand: one mass, one centre, and its
        // resistance to turning about that centre.
        private void Weigh(float weight)
        {
            float m = toolMass, total = m + rides[0] + rides[1];
            Vector3 c = tool.Centre, centre = (c * m + grip[0] * rides[0] + grip[1] * rides[1]) / total;
            // The tool's own, in its own axes, moved to the new centre; then each riding mass at its hand.
            var turn = Matrix4x4.Rotate(tool.InertiaTurn);
            var inertia = new float[3, 3];
            Vector3 own = tool.Inertia * weight;
            for (int a = 0; a < 3; a++)
                for (int b = 0; b < 3; b++)
                    inertia[a, b] = turn[a, 0] * own.x * turn[b, 0] + turn[a, 1] * own.y * turn[b, 1] + turn[a, 2] * own.z * turn[b, 2];
            void Add(float mass, Vector3 at)
            {
                Vector3 d = at - centre;
                for (int a = 0; a < 3; a++)
                    for (int b = 0; b < 3; b++)
                        inertia[a, b] += mass * ((a == b ? d.sqrMagnitude : 0) - d[a] * d[b]);
            }
            Add(m, c);
            for (int i = 0; i < 2; i++) if (rides[i] > 0) Add(rides[i], grip[i]);
            Principal(inertia, out Vector3 moments, out Quaternion axes);
            held.mass = total;
            held.centerOfMass = centre;
            held.inertiaTensor = moments;
            held.inertiaTensorRotation = axes;
        }

        // A symmetric 3x3 matrix's principal values and axes (Jacobi's turns).
        private static void Principal(float[,] m, out Vector3 values, out Quaternion axes)
        {
            var v = new float[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            for (int sweep = 0; sweep < 24; sweep++)
            {
                float off = Mathf.Abs(m[0, 1]) + Mathf.Abs(m[0, 2]) + Mathf.Abs(m[1, 2]);
                if (off < 1e-12f) break;
                for (int p = 0; p < 2; p++)
                    for (int q = p + 1; q < 3; q++)
                    {
                        if (Mathf.Abs(m[p, q]) < 1e-14f) continue;
                        float theta = (m[q, q] - m[p, p]) / (2 * m[p, q]);
                        float tan = Mathf.Sign(theta) / (Mathf.Abs(theta) + Mathf.Sqrt(theta * theta + 1));
                        if (theta == 0) tan = 1;
                        float cos = 1 / Mathf.Sqrt(tan * tan + 1), sin = tan * cos;
                        for (int k = 0; k < 3; k++)
                        {
                            float kp = m[k, p], kq = m[k, q];
                            m[k, p] = cos * kp - sin * kq; m[k, q] = sin * kp + cos * kq;
                        }
                        for (int k = 0; k < 3; k++)
                        {
                            float pk = m[p, k], qk = m[q, k];
                            m[p, k] = cos * pk - sin * qk; m[q, k] = sin * pk + cos * qk;
                        }
                        for (int k = 0; k < 3; k++)
                        {
                            float kp = v[k, p], kq = v[k, q];
                            v[k, p] = cos * kp - sin * kq; v[k, q] = sin * kp + cos * kq;
                        }
                    }
            }
            values = new Vector3(Mathf.Max(m[0, 0], 1e-7f), Mathf.Max(m[1, 1], 1e-7f), Mathf.Max(m[2, 2], 1e-7f));
            Vector3 x = new Vector3(v[0, 0], v[1, 0], v[2, 0]), y = new Vector3(v[0, 1], v[1, 1], v[2, 1]);
            Vector3 z = Vector3.Cross(x, y);
            if (Vector3.Dot(z, new Vector3(v[0, 2], v[1, 2], v[2, 2])) < 0) { /* the third axis is its mirror: the same axis */ }
            axes = Quaternion.LookRotation(z, y);
        }

        // Lets go: the object stays in the world, on its own.
        public Rigidbody Drop()
        {
            var was = held;
            for (int i = 0; i < 2; i++)
            {
                if ((on[i] || reach[i] > 0) && holds != null) holds.HoldHandle(i, false, Vector3.zero, Vector3.up, 0, Vector3.zero);
                if (links[i] != null) Destroy(links[i]);
                if (anchors[i] != null) Destroy(anchors[i].gameObject);
                on[i] = false; links[i] = null; anchors[i] = null; effort[i] = 0; push[i] = Vector3.zero; miss[i] = 0; reach[i] = 0; led[i] = false; opening[i] = false;
                rides[i] = 0;
            }
            // On its own it weighs what it weighs: no arm rides on it any more.
            if (held != null && tool != null) Weigh(ofItsWeight);
            if (thing != null) thing.Holder = null;
            held = null; model = null; thing = null; tool = null; wanting = false; trailing = false;
            if (body != null) body.GuideArms(null);
            return was;
        }

        private void OnDisable()
        {
            var was = Drop();
            if (was != null) Destroy(was.gameObject);
        }

        // Where the tool is meant to be (its own origin and turn, in the world). The hands push it there as hard as
        // they can, and no harder.
        // all: an all-out push, as in a blow. bears: the share of its weight the hands mean to carry (less than all
        // of it when the thing is to rest its weight on what is under it).
        public void Want(Vector3 position, Quaternion rotation, bool all = false, float bears = 1)
        {
            wantPosition = position; wantRotation = rotation; wanting = true; hard = all; carry = Mathf.Clamp01(bears);
            trailing = false;
        }

        // One hand holds the thing by where it grips, and means that place to be here. It does not turn the thing: the
        // rest of it hangs, trails or lies as it will (a tool dragged by its handle's end). The other hand lets go.
        public void WantEnd(int hand, Vector3 place)
        {
            if (held == null || !on[hand]) return;
            if (on[1 - hand]) Release(1 - hand);
            if (!trailing || trailHand != hand) endBefore = false;
            trailing = true; trailHand = hand; endWanted = place; wanting = false; wantedBefore = false;
        }
        public bool Trailing => held != null && trailing;
        private bool trailing, endBefore;
        private int trailHand;
        private Vector3 endWanted, endLast;

        // The hands stop pushing: they only keep hold.
        public void Slacken() { wanting = wantedBefore = false; trailing = false; }

        public void Stands(Vector3 hips, Quaternion posture, Vector3 leftShoulder, Vector3 rightShoulder) => stood = true;
        public bool Guides(int hand) => held != null && (on[hand] || reach[hand] > 0);
        public Vector3 Wrist(int hand, Vector3 shoulder)
        {
            Vector3 place = model.TransformPoint(grip[hand]), way = model.up;
            float radius = RadiusAt(grip[hand].y);
            if (reach[hand] > 0)
            {
                // Reaching: the wrist goes from where it was to where it will hold; the fingers close as it arrives.
                // (Drawn between the physics' steps: from how far along it was at the step before to how far it is.)
                float along = Mathf.Lerp(reachWas[hand], reach[hand], Mathf.Clamp01((Time.time - reachStepped) / Mathf.Max(1e-4f, Time.fixedDeltaTime)));
                float t = Mathf.Clamp01(along / ReachTime);
                holds.HoldHandle(hand, t > (led[hand] ? LedCloses : .6f), place, way, radius, shoulder);
                // (Led, it is as far along as the body has brought it: the body's own going down has the ease in it.)
                return Vector3.Lerp(reachFrom[hand], holds.WristFor(hand, place, way, radius, shoulder), led[hand] ? t : Mathf.SmoothStep(0, 1, t));
            }
            // (A hand that is opening to let go stays where it held while its fingers open.)
            holds.HoldHandle(hand, !opening[hand], place, way, radius, shoulder);
            return holds.WristFor(hand, place, way, radius, shoulder);
        }

        // The most of a push (0 to 1) a joint can carry: the push asks it for `asked`, its limb's own weight for `own`,
        // and it gives `capacity` at most.
        private static float Share(Vector3 asked, Vector3 own, float capacity)
        {
            float aa = asked.sqrMagnitude, ab = Vector3.Dot(asked, own), bb = own.sqrMagnitude;
            if (bb >= capacity * capacity) return 0;
            if (aa < 1e-10f) return 1;
            return Mathf.Clamp01((-ab + Mathf.Sqrt(Mathf.Max(0, ab * ab - aa * (bb - capacity * capacity)))) / aa);
        }

        private void FixedUpdate()
        {
            if (held == null || !stood) return;
            long began = Timed ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
            Step();
            if (Timed) { TimedTicks += System.Diagnostics.Stopwatch.GetTimestamp() - began; TimedSteps++; }
        }

        private void Step()
        {
            float dt = Time.fixedDeltaTime;
            // A hand that reaches takes hold when it arrives.
            for (int i = 0; i < 2; i++)
            {
                if (reach[i] <= 0) continue;
                reachWas[i] = reach[i]; reachStepped = Time.time;
                float there = ReachTime;
                if (led[i])
                {
                    there = ReachTime + LedHolds;
                    float until = ledTo[i] >= 1 ? there : ledTo[i] * ReachTime;
                    reach[i] = Mathf.Max(reach[i], Mathf.Min(reach[i] + dt, until));
                }
                else reach[i] += dt;
                if (reach[i] < there) continue;
                reach[i] = 0; led[i] = false;
                on[i] = true;
                Link(i, body.ShoulderNow(i));
                Weigh(ofItsWeight);
            }
            // A hand that slides moves along the handle, no nearer the other than a fist and a half.
            bool slid = false;
            for (int i = 0; i < 2; i++)
            {
                if (!on[i] || Mathf.Abs(slideTo[i] - grip[i].y) <= .0005f) continue;
                float to = Mathf.MoveTowards(grip[i].y, slideTo[i], SlidePace * dt);
                int other = 1 - i;
                if (on[other]) to = i == 0 ? Mathf.Max(to, grip[other].y + fist * 1.5f) : Mathf.Min(to, grip[other].y - fist * 1.5f);
                if (Mathf.Abs(to - grip[i].y) < 1e-5f) { slideTo[i] = grip[i].y; continue; }
                grip[i].y = to;
                links[i].anchor = grip[i];
                slid = true;
            }
            if (slid) Weigh(ofItsWeight);
            // The body as it will stand when this step is over: that is where the arms' links must have the thing by
            // then (a body that walks is a step further on).
            Span<Vector3> shoulders = stackalloc Vector3[2];
            Span<Vector3> elbows = stackalloc Vector3[2];
            body.StandsAt(Time.time + dt, out _, out _, shoulders, elbows);
            for (int i = 0; i < 2; i++)
            {
                if (anchors[i] == null) continue;
                // The wrist must stay within an arm's length of the shoulder. The held place is a palm away from the
                // wrist: the link is measured from the shoulder moved by that palm.
                Vector3 place = held.position + held.rotation * grip[i], way = held.rotation * Vector3.up;
                Vector3 palm = place - holds.WristFor(i, place, way, RadiusAt(grip[i].y), shoulders[i]);
                anchors[i].MovePosition(shoulders[i] + palm);
            }
            effort[0] = effort[1] = 0;
            push[0] = push[1] = Vector3.zero;
            if (trailing && !on[trailHand]) trailing = false;
            if (!wanting && !trailing) return;
            Vector3 centre = held.worldCenterOfMass, velocity = held.linearVelocity, spin = held.angularVelocity;
            Vector3 wantCentre = wantPosition + wantRotation * held.centerOfMass;
            // How the place it is meant to be is itself moving: nothing is held back from following that.
            Vector3 wantVelocity = Vector3.zero, wantSpin = Vector3.zero;
            if (wantedBefore)
            {
                wantVelocity = Vector3.ClampMagnitude((wantCentre - lastWantCentre) / dt, 25);
                (wantRotation * Quaternion.Inverse(lastWantRotation)).ToAngleAxis(out float moved, out Vector3 about);
                if (moved > 180) moved -= 360;
                if (float.IsFinite(about.x) && float.IsFinite(moved)) wantSpin = Vector3.ClampMagnitude(about * (moved * Mathf.Deg2Rad / dt), 50);
            }
            lastWantCentre = wantCentre; lastWantRotation = wantRotation; wantedBefore = true;
            (wantRotation * Quaternion.Inverse(held.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            Vector3 turn = float.IsFinite(axis.x) && float.IsFinite(angle) ? axis * (angle * Mathf.Deg2Rad) : Vector3.zero;
            // What would bring it there without overshoot (briskly, or as urgently as can be), and holds it up meanwhile.
            float quick = hard ? Hard : Quick, quickTurn = hard ? Hard : QuickTurn;
            Vector3 force = held.mass * (quick * quick * (wantCentre - centre) + 2 * quick * (wantVelocity - velocity) - Physics.gravity * carry);
            Quaternion principal = held.rotation * held.inertiaTensorRotation;
            Vector3 torque = principal * Vector3.Scale(held.inertiaTensor, Quaternion.Inverse(principal) * (quickTurn * quickTurn * turn + 2 * quickTurn * (wantSpin - spin)));

            // Shared between the hands that hold. Two hands turn it by pushing opposite ways across the handle; what
            // is left (a twist about the handle itself) is the wrists'.
            Span<Vector3> at = stackalloc Vector3[2];
            Span<Vector3> f = stackalloc Vector3[2];
            Span<Vector3> t = stackalloc Vector3[2];
            int count = 0, only = -1;
            for (int i = 0; i < 2; i++)
            {
                if (!on[i]) continue;
                at[i] = held.position + held.rotation * grip[i];
                count++; only = i;
            }
            if (trailing)
            {
                // Held by one end: the hand brings its end to where it means it, and holds up that end's share of the
                // weight (all of it while the thing hangs; less when its far end lies on the ground).
                int h = trailHand;
                Vector3 far = held.position + held.rotation * tool.Head;
                Vector3 toCentre = centre - far, toHand = at[h] - far;
                toCentre.y = 0; toHand.y = 0;
                float span = toHand.magnitude;
                float bears = span > .08f ? Mathf.Clamp01(Vector3.Dot(toCentre, toHand) / (span * span)) : 1;
                Vector3 endVelocity = endBefore ? Vector3.ClampMagnitude((endWanted - endLast) / dt, 8) : Vector3.zero;
                endLast = endWanted; endBefore = true;
                f[h] = held.mass * .5f * (Quick * Quick * (endWanted - at[h]) + 2 * Quick * (endVelocity - held.GetPointVelocity(at[h]))) - Physics.gravity * (held.mass * bears);
                t[h] = Vector3.zero;
                f[1 - h] = Vector3.zero; t[1 - h] = Vector3.zero;
            }
            else if (count == 2)
            {
                Vector3 between = at[0] - at[1];
                Vector3 need = torque - Vector3.Cross((at[0] + at[1]) * .5f - centre, force);
                Vector3 along = between.normalized, twist = along * Vector3.Dot(need, along);
                Vector3 couple = Vector3.Cross(need - twist, between) / between.sqrMagnitude;
                f[0] = force * .5f + couple; f[1] = force * .5f - couple;
                t[0] = t[1] = twist * .5f;
            }
            else
            {
                f[only] = force;
                t[only] = torque - Vector3.Cross(at[only] - centre, force);
            }

            // A sliding hand holds loosely: what it would have pulled along the handle is the other hand's to give.
            if (count == 2 && !trailing)
            {
                Vector3 handle = held.rotation * Vector3.up;
                for (int i = 0; i < 2; i++)
                {
                    if (Mathf.Abs(slideTo[i] - grip[i].y) <= .002f) continue;
                    float pulls = Vector3.Dot(f[i], handle);
                    f[i] -= handle * pulls;
                    f[1 - i] += handle * pulls;
                }
            }

            // What the arms can give of it. Each arm has its own most; both then give the same share of what was
            // asked, the lesser of the two, so that what they give together still points the way that was meant.
            Span<Vector3> partAt = stackalloc Vector3[3];
            Span<float> partMass = stackalloc float[3];
            Span<Vector3> asked = stackalloc Vector3[2];
            Span<Vector3> ownShoulder = stackalloc Vector3[2];
            Span<float> bends = stackalloc float[2];
            Span<float> ownBend = stackalloc float[2];
            Span<float> gain = stackalloc float[2];
            Span<float> grips = stackalloc float[2];
            float share = 1;
            for (int i = 0; i < 2; i++)
            {
                if (!on[i]) continue;
                Vector3 shoulder = shoulders[i], elbow = elbows[i];
                physical.ArmParts(i, partAt, partMass);
                // What the arm's own weight asks of the shoulder and of the elbow: the share of it that does not ride
                // on the handle (that share is part of what the hands hold up, and falls with it in a blow).
                Vector3 ownElbow = Vector3.zero;
                ownShoulder[i] = Vector3.zero;
                for (int k = 0; k < 3; k++)
                {
                    Vector3 weight = Physics.gravity * (partMass[k] * (k == 0 ? 1 - UpperArmRides : k == 1 ? 1 - ForearmRides : 0));
                    ownShoulder[i] -= Vector3.Cross(partAt[k] - shoulder, weight);
                    if (k > 0) ownElbow -= Vector3.Cross(partAt[k] - elbow, weight);
                }
                // The elbow is a hinge: only what would bend or straighten it is its muscles' to give.
                Vector3 hinge = Vector3.Cross(elbow - shoulder, at[i] - elbow);
                hinge = hinge.sqrMagnitude > 1e-8f ? hinge.normalized : Vector3.zero;
                asked[i] = Vector3.Cross(at[i] - shoulder, f[i]);
                // A muscle gives less the faster it shortens (the faster the hand already moves the way it pushes, the
                // less it can add), and more when it is being forced back.
                Vector3 moving = held.GetPointVelocity(at[i]);
                bool gives = Vector3.Dot(f[i], moving) > 0;
                gain[i] = gives ? 1 : Braking;
                float can = Share(asked[i], ownShoulder[i], physical.ShoulderOf(i) * gain[i]);
                bends[i] = Vector3.Dot(Vector3.Cross(at[i] - elbow, f[i]), hinge);
                ownBend[i] = Vector3.Dot(ownElbow, hinge);
                float bend = physical.ElbowOf(i) * gain[i];
                if (Mathf.Abs(bends[i]) > 1e-6f) can = Mathf.Min(can, Mathf.Clamp01(bends[i] > 0 ? (bend - ownBend[i]) / bends[i] : (-bend - ownBend[i]) / bends[i]));
                if (gives) can *= 1 - Mathf.Clamp01(moving.magnitude / (Fastest * body.ArmReach));
                // A hand holds no harder than its hold.
                grips[i] = f[i].magnitude;
                if (grips[i] > 1e-4f) can = Mathf.Min(can, physical.HoldOf(i) * gain[i] / grips[i]);
                share = Mathf.Min(share, can);
            }
            for (int i = 0; i < 2; i++)
            {
                if (!on[i]) continue;
                f[i] *= share;
                t[i] = Vector3.ClampMagnitude(t[i] * share, physical.WristOf(i));
                efforts[i] = new Vector4((asked[i] * share + ownShoulder[i]).magnitude / physical.ShoulderOf(i), Mathf.Abs(bends[i] * share + ownBend[i]) / physical.ElbowOf(i),
                    t[i].magnitude / physical.WristOf(i), grips[i] * share / physical.HoldOf(i));
                effort[i] = Mathf.Max(Mathf.Max(efforts[i].x, efforts[i].y), Mathf.Max(efforts[i].z, efforts[i].w));
                hold[i] = grips[i] * share / physical.HoldOf(i);
                // Work tires: the arm is told what it gave.
                physical.Worked(PhysicalBody.Arm(i), effort[i], dt);
                push[i] = f[i];
                held.AddForceAtPosition(f[i], at[i]);
                held.AddTorque(t[i]);
            }
            for (int i = 0; i < 2; i++)
            {
                if (!on[i]) { slips[i] = 0; continue; }
                slips[i] = Gives(i).magnitude > physical.HoldOf(i) * Braking ? slips[i] + dt : 0;
                if (slips[i] < SlipsAfter) continue;
                slips[i] = 0;
                if (on[1 - i]) Release(i);
                else { Slipped = Drop(); return; }
            }
        }

        // After the body is drawn: how far each hand's own hold is from the place it holds.
        private void LateUpdate()
        {
            if (held == null || !(holds is MinerBody miner)) return;
            for (int i = 0; i < 2; i++)
            {
                miss[i] = 0;
                if (!on[i] || miner.Held(i) < 1 || !miner.HandleIn(i, RadiusAt(grip[i].y), out var point, out _)) continue;
                miss[i] = Vector3.Distance(point, model.TransformPoint(grip[i]));
            }
        }
    }
}
