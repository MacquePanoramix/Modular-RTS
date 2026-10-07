using System;
using UnityEngine;

namespace WonderGather
{
    // The back at work (S3: Docs/Design/ThePhysicalBody.md).
    //
    // The body bows from the hips because its back lets it down and brings it up again, and a back has a most it can
    // give. What it must hold up is real: the upper body's own weight, further out the further it bows, and whatever
    // the hands are holding up or pushing against (PhysicalHands), at the end of the arms. So a body straightens up
    // slowly under a heavy load, and a back that is too weak for the load does not straighten at all.
    //
    // An ordinary back holds up about two and a half times its own upper body bent level (PhysicalBody.BackCapacity).
    //
    // The hips go back as the body bows, by as much as keeps its weight (the tool's with it) over its feet: the
    // first rung of balance. Stepping and falling are later rungs (step 6 and step 10).
    [DefaultExecutionOrder(510)]
    [RequireComponent(typeof(ProceduralBiped), typeof(PhysicalBody))]
    public sealed class PhysicalBack : MonoBehaviour
    {
        // How briskly the back answers (1/s), the fastest it bends (degrees a second: a muscle gives less the faster
        // it shortens), and how much more it resists when forced the other way.
        private const float Quick = 9, Fastest = 320, Braking = 1.5f;
        // How much of the way its weight stands ahead of its feet the hips go back for, each step.
        private const float Settles = .5f;
        private ProceduralBiped body;
        private PhysicalBody physical;
        private PhysicalHands hands;
        private PhysicalBalance balance;
        private float wanted, angle, rate;
        private bool began;

        // The share of what the back has that it gave at the last step (1: all of it).
        public float Effort { get; private set; }
        // How far ahead of the middle of its feet the body's weight stands (the held thing's with it), in metres.
        public float Ahead { get; private set; }
        public float BowNow => angle;
        // How far the body means to bow from the hips, in degrees (forward is positive). It gets there as its back can.
        public void Want(float degrees) => wanted = Mathf.Clamp(degrees, -15, ProceduralBiped.MostBowed);
        // The back is bowed this far now, and still (the body has just been put so: getting up from a fall).
        public void Is(float degrees) { angle = Mathf.Clamp(degrees, -15, ProceduralBiped.MostBowed); rate = 0; began = true; }

        private void Awake()
        {
            body = GetComponent<ProceduralBiped>();
            physical = GetComponent<PhysicalBody>();
        }

        private void OnEnable() { began = false; }

        private void OnDisable()
        {
            if (body == null) return;
            body.BowIs(float.NaN);
            body.Bow(0);
            body.SetBack(0);
        }

        private void FixedUpdate()
        {
            if (!physical.Ready || !body.Ready) return;
            if (hands == null) hands = GetComponent<PhysicalHands>();
            if (balance == null) balance = GetComponent<PhysicalBalance>();
            if (!began) { angle = body.BowNow; rate = 0; began = true; }
            float dt = Time.fixedDeltaTime;
            Span<Vector3> unused = stackalloc Vector3[4];
            body.StandsAt(Time.time, out Vector3 hips, out Quaternion posture, unused.Slice(0, 2), unused.Slice(2, 2));
            Vector3 across = posture * Vector3.right;
            // What the back holds up: the upper body, and what the hands hold up or push against.
            physical.UpperBody(hips, out float mass, out Vector3 centre, out float inertia);
            float weighs = Vector3.Dot(Vector3.Cross(centre - hips, Physics.gravity * mass), across);
            float load = 0;
            bool holding = hands != null && hands.Held != null;
            if (holding)
                for (int i = 0; i < 2; i++)
                    if (hands.Holds(i)) load += Vector3.Dot(Vector3.Cross(hands.GripPlace(i) - hips, -hands.Push(i)), across);
            float capacity = physical.BackNow;
            float error = (wanted - angle) * Mathf.Deg2Rad, speed = rate * Mathf.Deg2Rad;
            // What would bring the bow to where it is meant to be without overshoot, and holds the weight up meanwhile.
            float asked = inertia * (Quick * Quick * error - 2 * Quick * speed) - weighs - load;
            bool gives = asked * speed > 0;
            float most = capacity * (gives ? 1 - Mathf.Clamp01(Mathf.Abs(rate) / Fastest) : Braking);
            float given = Mathf.Clamp(asked, -most, most);
            Effort = Mathf.Abs(given) / capacity;
            // Work tires: the back is told what it gave.
            physical.Worked(PhysicalBody.Muscles.Back, Effort, dt);
            rate += (given + weighs + load) / inertia * Mathf.Rad2Deg * dt;
            angle += rate * dt;
            if (angle < -15) { angle = -15; rate = Mathf.Max(rate, 0); }
            if (angle > ProceduralBiped.MostBowed) { angle = ProceduralBiped.MostBowed; rate = Mathf.Min(rate, 0); }
            // The body is drawn between the steps: it goes on to where the bow will be at the next one. What works on
            // the physics' clock is told where the bow is now.
            body.Bow(angle + rate * dt, Mathf.Abs(rate) + 30);
            body.BowIs(angle);

            // Its weight over its feet: the hips go back by a share of how far ahead of the feet the weight stands.
            Vector3 weight = physical.CentreOfMass() * physical.Mass;
            float total = physical.Mass;
            if (holding) { weight += hands.Held.worldCenterOfMass * hands.ToolMass; total += hands.ToolMass; }
            Vector3 feet = (body.FootPosition(0) + body.FootPosition(1)) * .5f;
            Vector3 forward = Vector3.ProjectOnPlane(posture * Vector3.forward, Vector3.up).normalized;
            // The middle of a foot is ahead of its ankle: that is where the weight is carried best.
            Ahead = Vector3.Dot(weight / total - feet, forward) - body.FootMiddle;
            // A body that keeps its own balance (PhysicalBalance) moves its hips by that.
            if (balance == null || !balance.isActiveAndEnabled || !balance.Acts) body.SetBack(body.BackWanted + Ahead * Settles);
            else body.SetBack(0);
        }
    }
}
