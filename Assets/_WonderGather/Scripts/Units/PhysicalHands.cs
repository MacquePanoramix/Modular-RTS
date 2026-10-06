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
        private readonly float[] effort = new float[2], miss = new float[2], rides = new float[2];
        // The tool's own weight (what is held weighs more: the arms' mass rides on it).
        private float toolMass;
        public float ToolMass => toolMass;
        private readonly Vector3[] push = new Vector3[2];
        private Vector3 wantPosition, lastWantCentre;
        private Quaternion wantRotation = Quaternion.identity, lastWantRotation = Quaternion.identity;
        private bool wanting, wantedBefore, stood, hard;

        public Rigidbody Held => held;
        public HeldThing Thing => thing;
        public ToolDefinition Tool => tool;
        public bool Holds(int hand) => held != null && on[hand];
        // The share of its capacity the hardest-worked joint of this arm gave at the last step (1: all it has).
        public float Effort(int hand) => effort[hand];
        // What this hand pushed the object with at the last step, in newtons.
        public Vector3 Push(int hand) => push[hand];
        // How far the hand's own hold was from the place it holds on the handle, when last drawn.
        public float Miss(int hand) => miss[hand];
        public Vector3 HeadPosition => held != null ? held.position + held.rotation * tool.Head : transform.position;
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

        // Takes a tool into the free hands, at a place and a turn (the tool's own origin). weight: a share of its own
        // weight, for trying the same tool lighter or heavier.
        public Rigidbody Take(ToolDefinition definition, Vector3 position, Quaternion rotation, float weight = 1)
        {
            Drop();
            if (definition == null || !definition.HasWeight) throw new ArgumentException("Only a tool that has been weighed can be held.");
            if (holds == null || physical == null || !physical.Ready) throw new InvalidOperationException("This body has no hands of its own, or has not been weighed.");
            if (!holds.HandFree(0) && !holds.HandFree(1)) throw new InvalidOperationException("This body has no free hand.");
            var go = new GameObject(definition.DisplayName + " (held)");
            go.transform.SetPositionAndRotation(position, rotation);
            model = Instantiate(definition.Prefab, go.transform).transform;
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
            held = go.AddComponent<Rigidbody>();
            toolMass = definition.Mass * weight;
            held.interpolation = RigidbodyInterpolation.Interpolate;
            held.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            held.linearDamping = 0;
            held.angularDamping = .05f;
            held.maxAngularVelocity = 60;
            foreach (var mine in GetComponentsInChildren<Collider>(true))
                foreach (var its in go.GetComponents<Collider>()) Physics.IgnoreCollision(mine, its);
            thing = go.AddComponent<HeldThing>();
            thing.Head = head;
            tool = definition;
            float arm = body.ArmReach;
            Span<Vector3> partAt = stackalloc Vector3[3];
            Span<float> partMass = stackalloc float[3];
            for (int i = 0; i < 2; i++)
            {
                on[i] = holds.HandFree(i);
                grip[i] = i == 0 ? definition.SecondaryGrip : definition.PrimaryGrip;
                rides[i] = 0;
                if (!on[i]) continue;
                physical.ArmParts(i, partAt, partMass);
                rides[i] = partMass[2] + partMass[1] * ForearmRides + partMass[0] * UpperArmRides;
                // An arm's length is the one thing the object cannot get past.
                var anchor = new GameObject(i == 0 ? "Left shoulder (link)" : "Right shoulder (link)").AddComponent<Rigidbody>();
                anchor.isKinematic = true;
                anchor.position = stood ? body.ShoulderNow(i) : position;
                anchors[i] = anchor;
                var link = go.AddComponent<ConfigurableJoint>();
                link.autoConfigureConnectedAnchor = false;
                link.connectedBody = anchor;
                link.anchor = grip[i];
                link.connectedAnchor = Vector3.zero;
                link.xMotion = link.yMotion = link.zMotion = ConfigurableJointMotion.Limited;
                link.linearLimit = new SoftJointLimit { limit = arm - .004f, contactDistance = .005f };
                link.angularXMotion = link.angularYMotion = link.angularZMotion = ConfigurableJointMotion.Free;
                links[i] = link;
            }
            Weigh(weight);
            wanting = wantedBefore = false;
            body.GuideArms(this);
            return held;
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
                if (on[i] && holds != null) holds.HoldHandle(i, false, Vector3.zero, Vector3.up, 0, Vector3.zero);
                if (links[i] != null) Destroy(links[i]);
                if (anchors[i] != null) Destroy(anchors[i].gameObject);
                on[i] = false; links[i] = null; anchors[i] = null; effort[i] = 0; push[i] = Vector3.zero; miss[i] = 0;
            }
            held = null; model = null; thing = null; tool = null; wanting = false;
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
        // all: an all-out push, as in a blow.
        public void Want(Vector3 position, Quaternion rotation, bool all = false)
        {
            wantPosition = position; wantRotation = rotation; wanting = true; hard = all;
        }

        // The hands stop pushing: they only keep hold.
        public void Slacken() { wanting = wantedBefore = false; }

        public void Stands(Vector3 hips, Quaternion posture, Vector3 leftShoulder, Vector3 rightShoulder) => stood = true;
        public bool Guides(int hand) => held != null && on[hand];
        public Vector3 Wrist(int hand, Vector3 shoulder)
        {
            Vector3 place = model.TransformPoint(grip[hand]), way = model.up;
            holds.HoldHandle(hand, true, place, way, tool.GripRadius(hand), shoulder);
            return holds.WristFor(hand, place, way, tool.GripRadius(hand), shoulder);
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
            // The body as it stands at this step's own moment.
            Span<Vector3> shoulders = stackalloc Vector3[2];
            Span<Vector3> elbows = stackalloc Vector3[2];
            body.StandsAt(Time.time, out _, out _, shoulders, elbows);
            for (int i = 0; i < 2; i++)
            {
                if (anchors[i] == null) continue;
                // The wrist must stay within an arm's length of the shoulder. The held place is a palm away from the
                // wrist: the link is measured from the shoulder moved by that palm.
                Vector3 place = held.position + held.rotation * grip[i], way = held.rotation * Vector3.up;
                Vector3 palm = place - holds.WristFor(i, place, way, tool.GripRadius(i), shoulders[i]);
                anchors[i].MovePosition(shoulders[i] + palm);
            }
            effort[0] = effort[1] = 0;
            push[0] = push[1] = Vector3.zero;
            if (!wanting) return;
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
            Vector3 force = held.mass * (quick * quick * (wantCentre - centre) + 2 * quick * (wantVelocity - velocity) - Physics.gravity);
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
            if (count == 2)
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

            // What the arms can give of it. Each arm has its own most; both then give the same share of what was
            // asked, the lesser of the two, so that what they give together still points the way that was meant.
            Span<Vector3> partAt = stackalloc Vector3[3];
            Span<float> partMass = stackalloc float[3];
            Span<Vector3> asked = stackalloc Vector3[2];
            Span<Vector3> ownShoulder = stackalloc Vector3[2];
            Span<float> bends = stackalloc float[2];
            Span<float> ownBend = stackalloc float[2];
            Span<float> gain = stackalloc float[2];
            float share = 1, most = physical.ElbowCapacity;
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
                float can = Share(asked[i], ownShoulder[i], physical.ShoulderCapacity * gain[i]);
                bends[i] = Vector3.Dot(Vector3.Cross(at[i] - elbow, f[i]), hinge);
                ownBend[i] = Vector3.Dot(ownElbow, hinge);
                float bend = most * gain[i];
                if (Mathf.Abs(bends[i]) > 1e-6f) can = Mathf.Min(can, Mathf.Clamp01(bends[i] > 0 ? (bend - ownBend[i]) / bends[i] : (-bend - ownBend[i]) / bends[i]));
                if (gives) can *= 1 - Mathf.Clamp01(moving.magnitude / (Fastest * body.ArmReach));
                share = Mathf.Min(share, can);
            }
            for (int i = 0; i < 2; i++)
            {
                if (!on[i]) continue;
                f[i] *= share;
                t[i] = Vector3.ClampMagnitude(t[i] * share, physical.WristCapacity);
                effort[i] = Mathf.Max((asked[i] * share + ownShoulder[i]).magnitude / physical.ShoulderCapacity, Mathf.Abs(bends[i] * share + ownBend[i]) / most,
                    t[i].magnitude / physical.WristCapacity);
                push[i] = f[i];
                held.AddForceAtPosition(f[i], at[i]);
                held.AddTorque(t[i]);
            }
        }

        // After the body is drawn: how far each hand's own hold is from the place it holds.
        private void LateUpdate()
        {
            if (held == null || !(holds is MinerBody miner)) return;
            for (int i = 0; i < 2; i++)
            {
                miss[i] = 0;
                if (!on[i] || miner.Held(i) < 1 || !miner.HandleIn(i, tool.GripRadius(i), out var point, out _)) continue;
                miss[i] = Vector3.Distance(point, model.TransformPoint(grip[i]));
            }
        }
    }
}
