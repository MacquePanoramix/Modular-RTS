using System.Collections.Generic;
using UnityEngine;

namespace WonderGather
{
    // A rough plan of one swing, as intentions: where the tool is meant to be at each moment (S3, step 2: the bench).
    // It begins bowed, with the pick's head low in front over a block; the intention raises it over the right shoulder
    // as the body straightens, then means to bring it down through the block as hard as the body can, bowing into
    // the blow. The path over the shoulder is the one the miners' first swing took (EquippedTool.PoseAt); what
    // follows it is the physics (PhysicalHands): how high the tool goes, how fast it comes down, whether it gets
    // there at all. The bow is by intention only, for now: the back's own strength is not yet counted.
    // The swing proper (its six parts, the sliding hand, the legs and the back) is step 4, and replaces this.
    public sealed class PhysicalSwing : MonoBehaviour
    {
        public enum Phase { Ready, Lift, Top, Drive, Struck, Recover }

        public struct Result
        {
            // How far the head rose, the share of the way up it got, and how long that took.
            public float lifted, reached, liftTime;
            // How hard the hardest-worked joint worked while lifting, on average (1: all it has).
            public float liftEffort;
            public bool struck;
            // The head's speed as it struck, and the tool's energy then (taking all its weight to be at the head).
            public float speed, energy;
        }

        // The tool's lean against the body at rest, at the top of the lift, and where the drive means to end (through
        // the block), in degrees from the body's own upright: forward is positive. With the trunk as it is, two hands
        // reach the handle only up to about 45 degrees forward.
        public const float Rest = 45, Raised = -52, Through = 80;
        // How far the body bows from the hips at rest (so the head is low), at the top, and into the blow.
        public const float RestBow = 38, RaisedBow = -4, BlowBow = 38;
        // The intention leads the tool along its path by so many degrees of lean, and no further: gently and at a
        // set pace for the lift, far ahead and with no pace set for the blow.
        private const float LiftLead = 22, LiftPace = (Rest - Raised) / .6f, BlowLead = 50;
        public PhysicalHands hands;
        public ProceduralBiped body;
        public ToolDefinition tool;
        public Phase phase;
        public readonly List<Result> results = new List<Result>();
        // For the bench: when set, what the tool did at every step of the first blow.
        public List<string> trace;
        // How long it stands ready before the first lift.
        public float settle = 1.2f;
        private float clock, angle = Rest, stalled, low, high, worked, headSpeed;
        private int blows, steps;
        private Vector3 from;
        private Quaternion fromTurn;
        private Result now;

        private void Go(Phase next) { phase = next; clock = 0; stalled = 0; }

        // Where the tool is meant to be when it leans by an angle: its own origin and turn, in the world.
        public void Intend(float lean, out Vector3 position, out Quaternion rotation) => Intend(lean, body.HipsNow, body.PostureNow, out position, out rotation);

        private void Intend(float lean, Vector3 hips, Quaternion posture, out Vector3 position, out Quaternion rotation)
        {
            float reach = body.ArmReach;
            Vector3 ready = body.ToolHand, shoulder = body.ShoulderFromHips;
            var shape = body.BodyProportions;
            Vector3 raised = new Vector3(Mathf.Max(shoulder.x + .04f * reach, shape.headHalf + .1f * reach), shoulder.y - .17f * reach, Mathf.Max(shoulder.z + .58f * reach, ready.z));
            Vector3 round = new Vector3(raised.x * 1.1f, ready.y, ready.z + .1f * reach);
            // The hands go out as the tool leans forward, as far as where it rests; past that (a blow going through)
            // they stay, and the head comes down.
            float up = Mathf.Clamp01((20 - lean) / 72f), forth = Mathf.Clamp01((Mathf.Min(lean, Rest) - 20) / 68f);
            Vector3 place = (1 - up) * (1 - up) * ready + 2 * up * (1 - up) * round + up * up * raised + forth * new Vector3(0, -.05f * reach, .3f * reach);
            rotation = posture * Quaternion.Euler(0, -14 * up, 0) * Quaternion.Euler(lean, 0, 0);
            position = hips + posture * place - rotation * tool.PrimaryGrip;
        }

        // How the tool leans now against the body's own upright, in degrees: forward is positive.
        public float Lean() => Lean(body.PostureNow);

        private float Lean(Quaternion posture)
        {
            Vector3 handle = Quaternion.Inverse(posture) * (hands.Held.rotation * Vector3.up);
            return Mathf.Atan2(handle.z, handle.y) * Mathf.Rad2Deg;
        }

        private void FixedUpdate()
        {
            if (hands == null || hands.Held == null) return;
            float dt = Time.fixedDeltaTime;
            clock += dt;
            // The body as it stands at this step's own moment (it is posed once a frame; this steps on the physics' clock).
            System.Span<Vector3> unused = stackalloc Vector3[4];
            body.StandsAt(Time.time, out Vector3 hips, out Quaternion posture, unused.Slice(0, 2), unused.Slice(2, 2));
            float headUp = hands.HeadPosition.y - transform.position.y;
            Vector3 position;
            Quaternion rotation;
            bool hard = false;
            switch (phase)
            {
                case Phase.Ready:
                    angle = Rest;
                    body.Bow(RestBow);
                    if (clock >= settle) { low = high = headUp; worked = 0; steps = 0; now = default; Go(Phase.Lift); }
                    break;
                case Phase.Lift:
                {
                    // The intention goes up only as fast as the tool follows it: as high as it will go.
                    float before = angle;
                    angle = Mathf.Min(angle, Mathf.Max(Mathf.MoveTowards(angle, Raised, LiftPace * dt), Lean(posture) - LiftLead));
                    stalled = before - angle > LiftPace * dt * .05f ? 0 : stalled + dt;
                    body.Bow(Mathf.Lerp(RestBow, RaisedBow, Mathf.InverseLerp(Rest, Raised, angle)));
                    high = Mathf.Max(high, headUp);
                    worked += Mathf.Max(hands.Effort(0), hands.Effort(1)); steps++;
                    if (angle <= Raised + .1f || stalled > .8f || clock > 5)
                    {
                        now.liftTime = clock; now.reached = Mathf.InverseLerp(Rest, Raised, angle); now.liftEffort = worked / Mathf.Max(1, steps);
                        Go(Phase.Top);
                    }
                    break;
                }
                case Phase.Top:
                    high = Mathf.Max(high, headUp);
                    if (clock >= .12f) { now.lifted = high - low; blows = hands.Thing.HeadBlows; Go(Phase.Drive); }
                    break;
                case Phase.Drive:
                    // The back goes first, and the arms follow when it is well on its way: the tool comes over last.
                    body.Bow(BlowBow);
                    if (body.BowNow < Mathf.Lerp(RaisedBow, BlowBow, .6f)) break;
                    // All the arms have: the intention stays well ahead of the tool, along the same path, until the blow.
                    angle = Mathf.Min(Through, Mathf.Max(angle, Lean(posture) + BlowLead)); hard = true;
                    if (hands.Thing.HeadBlows > blows)
                    {
                        // How fast the head was going at the step before it struck (the tool's weight is nearly all there).
                        now.struck = true; now.speed = headSpeed; now.energy = .5f * hands.ToolMass * now.speed * now.speed;
                    }
                    headSpeed = hands.HeadVelocity.magnitude;
                    if (now.struck || clock > 1.5f)
                    {
                        // The blow is over. It means the tool to stay where the blow left its head, square in the hands
                        // again: it does not go on pushing into what it struck.
                        angle = Mathf.Clamp(Lean(posture), Raised, Through);
                        Intend(angle, hips, posture, out _, out fromTurn);
                        from = hands.HeadPosition - fromTurn * tool.Head;
                        Go(Phase.Struck);
                    }
                    break;
                case Phase.Struck:
                    if (clock >= .35f) Go(Phase.Recover);
                    break;
                case Phase.Recover:
                    body.Bow(RestBow);
                    if (clock >= 1.1f) { results.Add(now); angle = Rest; settle = .5f; Go(Phase.Ready); }
                    break;
            }
            if (phase == Phase.Struck) { position = from; rotation = fromTurn; }
            else if (phase == Phase.Recover)
            {
                Intend(Rest, hips, posture, out var to, out var toTurn);
                float t = Mathf.SmoothStep(0, 1, clock / .9f);
                position = Vector3.Lerp(from, to, t); rotation = Quaternion.Slerp(fromTurn, toTurn, t);
            }
            else Intend(angle, hips, posture, out position, out rotation);
            hands.Want(position, rotation, hard);
            if (trace != null && results.Count == 0 && phase != Phase.Ready && phase != Phase.Recover)
                trace.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} {1:0.00}s lean {2:0} wanted {3:0} bow {4:0} head {5:0.000} up {6:0.000} ahead at {7:0.0} m/s; blows {8} (head {9}); efforts {10:0.00} {11:0.00}",
                    phase, clock, Lean(posture), angle, body.BowNow, hands.HeadPosition.y - transform.position.y,
                    Vector3.Dot(hands.HeadPosition - transform.position, transform.forward), hands.HeadVelocity.magnitude,
                    hands.Thing.Blows, hands.Thing.HeadBlows, hands.Effort(0), hands.Effort(1)));
        }
    }
}
