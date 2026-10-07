using System.Collections.Generic;
using UnityEngine;

namespace WonderGather
{
    // The plan of a swing, as intentions: where the tool is meant to be at each moment (S3, steps 2 and 4).
    // It begins bowed, with the pick's head resting just over the point it is aimed at; the intention raises it over
    // the right shoulder as the back and the legs straighten, then means to bring it down through that point as hard
    // as the body can, bowing into the blow. The path over the shoulder is the one the miners' first swing took
    // (EquippedTool.PoseAt). What follows the intention is the physics: the arms (PhysicalHands) and the back
    // (PhysicalBack) give what they can, so how high the tool goes, how fast it comes down and whether it gets
    // there at all come out of the weights and the strength.
    // What adapts by itself: the upper hand goes up the handle towards the head for the lift by as much as the tool
    // feels heavy, and slides down to the lower hand in the blow; aimed at a point (Aim), the body bows and bends
    // its knees as that point needs.
    // Still by intention only: the legs (they bend and straighten at a set pace, whatever they carry).
    public sealed class PhysicalSwing : MonoBehaviour
    {
        public enum Phase { Ready, Lift, Top, Drive, Struck, Recover, Rest }

        public struct Result
        {
            // How far the head rose, the share of the way up it got, and how long that took.
            public float lifted, reached, liftTime;
            // How hard the hardest-worked joint worked while lifting, on average (1: all it has).
            public float liftEffort;
            public bool struck;
            // How hard the back worked while lifting, on average (1: all it has).
            public float backEffort;
            // Where the pick's head was (its striking ball's middle) when it struck.
            public Vector3 landed;
            // How spent the most spent of the arms and the back was when the lift began (0: fresh).
            public float spent;
            // Where the upper hand held for the lift, as a share of the way from its own place to the head.
            public float choked;
            // The head's speed as it struck, and the tool's energy then (taking all its weight to be at the head). It is
            // read at the last step before the blow: whether the engine finds the contact a step sooner or later
            // moves it by what the head gains in a step (about 0.7 m/s).
            public float speed, energy;
            // The head's speed as the tool came down through the body's own upright, on its way to the blow: read
            // between steps, so it does not depend on when the blow is found.
            public float upright;
        }

        // The tool's lean against the body at rest, at the top of the lift, and where the drive means to end (through
        // the block), in degrees from the body's own upright: forward is positive. With the trunk as it is, two hands
        // reach the handle only up to about 45 degrees forward.
        public const float Rest = 45, Raised = -52, Through = 80;
        // How far the body bows from the hips at rest (so the head is low), at the top, and into the blow.
        public const float RestBow = 38, RaisedBow = -4, BlowBow = 38;
        // The upper hand goes up the handle for the lift, as far as the body needs it there: not at all if holding
        // the tool at rest takes less than this share of what the arms have, all the way to the head at this share.
        private const float ChokeFrom = .28f, ChokeFull = .62f;
        // It rests after a blow when its arms or its back are this spent, and goes on when they are this fresh
        // again. Resting, it stands up straight with the tool held at ease across its thighs, and gets its breath.
        public const float RestsAt = .4f, GoesOnAt = .15f;
        // Held at the side in one hand, the arm hangs a little out from the body, and the shoulder holds the tool out
        // there. When that asks more than this share of what the shoulder has now, the arm does not get its strength
        // back so (what holding spends is more than rest gives back, before it is fresh enough to go on): the tool is
        // put down instead. Its head lies on the ground, and the lower hand keeps the end of the handle, hanging at
        // the side this share of the tool's length up; the knees give what the arm lacks.
        public const float RestsOnlyBelow = .27f, StoodUp = .97f;
        // The share of what its shoulder has now that holding the tool at its side would ask.
        public float HoldAsks
        {
            get
            {
                if (physical == null) physical = GetComponent<PhysicalBody>();
                float mass = hands != null && hands.Held != null ? hands.ToolMass : tool.Mass;
                float lever = .2f * body.ArmReach + body.BodyProportions.armCarry.x;
                return mass * Physics.gravity.magnitude * lever / Mathf.Max(1e-3f, physical.ShoulderOf(0));
            }
        }
        // It rests with the tool's head on the ground (it is too heavy for it to rest holding it).
        public bool RestsOnGround => grounded;
        private bool grounded;
        private float groundClock;
        private Vector3 endFrom;
        // The stance it takes for the work (PhysicalBalance.Brace): the feet so much further apart than the hips (a
        // share of their width, each side), and the left so far ahead of the right (a share of the hips' height).
        public const float StanceWider = .6f, StanceStagger = .1f;
        // The intention leads the tool along its path by so many degrees of lean, and no further: gently and at a
        // set pace for the lift, far ahead and with no pace set for the blow.
        private const float LiftLead = 22, LiftPace = (Rest - Raised) / .6f, BlowLead = 50;
        public PhysicalHands hands;
        public PhysicalBack back;
        public ProceduralBiped body;
        public ToolDefinition tool;
        public Phase phase;
        public readonly List<Result> results = new List<Result>();
        // For the bench: when set, what the tool did at every step of the first blow.
        public List<string> trace;
        // How long it stands ready before the first lift.
        public float settle = 1.2f;
        // How many times it has stopped to rest.
        public int rests;
        private PhysicalBody physical;
        private PhysicalBalance balance;
        // How spent the most spent of the arms and the back is.
        public float Spent
        {
            get
            {
                if (physical == null) physical = GetComponent<PhysicalBody>();
                return Mathf.Max(physical.Spent(PhysicalBody.Muscles.LeftArm), physical.Spent(PhysicalBody.Muscles.RightArm), physical.Spent(PhysicalBody.Muscles.Back));
            }
        }
        private float clock, angle = Rest, stalled, low, high, worked, bent, headSpeed, heavy = -1, leanBefore;
        // The swing as it is aimed: the tool's lean at rest and where the blow means to end, the bow at rest and into
        // the blow, and how far the hips sink for it. Unaimed, they are the bench's.
        private float rest = Rest, through = Through, restBow = RestBow, blowBow = BlowBow, sink;
        public float RestLean => rest;
        public float RestBowNow => restBow;
        public float SinkFor => sink;
        // How far from the point aimed at the pick's head can be brought, as the aim worked it out.
        public float AimMiss { get; private set; }
        // How heavy the tool felt at rest (the share of what the arms have that holding it took), and where the
        // upper hand went for it.
        public float Heavy => heavy;
        private int blows, steps;
        private Vector3 from;
        private Quaternion fromTurn;
        private Result now;

        // How heavy the tool feels is taken with the hands where they hold at rest, before the upper hand moves: what
        // the arms gave over a moment of holding it.
        private float feels;
        private int feelSteps;
        private bool chose;
        // A swing has been begun since the last was recorded.
        private bool swung;

        private void Go(Phase next)
        {
            phase = next; clock = 0; stalled = 0;
            if (next == Phase.Ready) { feels = 0; feelSteps = 0; chose = false; }
        }

        // Where the tool is meant to be when it leans by an angle: its own origin and turn, in the world.
        public void Intend(float lean, out Vector3 position, out Quaternion rotation) => Intend(lean, body.HipsNow, body.PostureNow, out position, out rotation);

        private void Intend(float lean, Vector3 hips, Quaternion posture, out Vector3 position, out Quaternion rotation)
            => Place(lean, rest, hips, posture, out position, out rotation);

        // The tool's place at a lean, for a body whose hips and posture are given. rests: the lean at which the
        // tool rests (the hands go out no further than they are there).
        private void Place(float lean, float rests, Vector3 hips, Quaternion posture, out Vector3 position, out Quaternion rotation)
        {
            float reach = body.ArmReach;
            Vector3 ready = body.ToolHand, shoulder = body.ShoulderFromHips;
            var shape = body.BodyProportions;
            Vector3 raised = new Vector3(Mathf.Max(shoulder.x + .04f * reach, shape.headHalf + .1f * reach), shoulder.y - .17f * reach, Mathf.Max(shoulder.z + .58f * reach, ready.z));
            Vector3 round = new Vector3(raised.x * 1.1f, ready.y, ready.z + .1f * reach);
            // The hands go out as the tool leans forward, as far as where it rests; past that (a blow going through)
            // they stay, and the head comes down.
            float up = Mathf.Clamp01((20 - lean) / 72f), forth = Mathf.Clamp01((Mathf.Min(lean, rests) - 20) / 68f);
            Vector3 place = (1 - up) * (1 - up) * ready + 2 * up * (1 - up) * round + up * up * raised + forth * new Vector3(0, -.05f * reach, .3f * reach);
            rotation = posture * Quaternion.Euler(0, -14 * up, 0) * Quaternion.Euler(lean, 0, 0);
            position = hips + posture * place - rotation * tool.PrimaryGrip;
        }

        // Aims the swing at a point in the world: the blow is to land there. The body finds how far to bow, how far
        // to bend its knees and how the tool must lean for the pick's head to be at that point with the hands where
        // they hold at rest, standing where it stands. It prefers to stand tall and to bow moderately.
        // leansUpTo: how far forward the tool may lean when its head is at the point. Up to where the tool rests
        // (RestsUpTo), the head waits just over the point and the blow lands as it comes to rest. Further, the blow
        // lands later in its arc, with the head below the hands: that is how a low spot is struck, and the tool then
        // waits well above it.
        public void Aim(Vector3 point, float leansUpTo = RestsUpTo)
        {
            var aimed = AimFrom(transform.position, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized), point, leansUpTo);
            AimMiss = aimed.miss;
            // At rest the head waits above the point; the blow means to go through it.
            restBow = blowBow = aimed.bow; sink = aimed.sink;
            rest = Mathf.Min(aimed.lean, RestsUpTo) - 6; through = Mathf.Min(aimed.lean + 38, ThroughAtMost);
            angle = rest;
        }

        // The tool rests leaning no further forward than this (the hands reach no further with the handle between
        // them); and a blow is never meant to go further through than this.
        public const float RestsUpTo = 46, ThroughAtMost = 104;

        // How a blow at a point would be taken: the bow, the tool's lean and how far the hips sink; how far from the
        // point the pick's head could be brought; and what the stance costs (that miss, and a little for bent knees and
        // for a bow far from an easy one).
        public struct Aimed
        {
            public float bow, lean, sink, miss, cost;
        }

        // The swing being made now, as far as it has got (whether it has struck, and how fast the head was going).
        public Result Current => now;

        // Where the pick's head (its striking ball's middle) would be with the tool at a lean, for a body standing
        // somewhere that takes a blow as aimed: the path the head comes down.
        public Vector3 HeadAt(Vector3 feet, Quaternion facing, Aimed aimed, float lean)
        {
            float hip = body.StandingHipHeight;
            Vector3 hips = feet + Vector3.up * (hip - aimed.sink) - facing * Vector3.forward * (.2f * hip * Mathf.Sin(Mathf.Max(0, aimed.bow) * Mathf.Deg2Rad));
            Place(lean, Mathf.Min(aimed.lean, RestsUpTo) - 6, hips, facing * Quaternion.Euler(aimed.bow, 0, 0), out var position, out var rotation);
            return position + rotation * tool.Head;
        }

        // The same, for a body standing somewhere, facing some way: so that a place to stand can be chosen for a point.
        // roughly: looked for in wide steps only, for choosing among many places (about a tenth of the work).
        public Aimed AimFrom(Vector3 feet, Quaternion facing, Vector3 point, float leansUpTo = RestsUpTo, bool roughly = false)
        {
            float hip = body.StandingHipHeight;
            float best = float.MaxValue, bow = RestBow, lean = Rest, down = 0, miss = 0;
            void Try(float s, float b, float l)
            {
                // The hips go back as the body bows (PhysicalBack): about a fifth of their height at a deep bow.
                Vector3 hips = feet + Vector3.up * (hip - s) - facing * Vector3.forward * (.2f * hip * Mathf.Sin(Mathf.Max(0, b) * Mathf.Deg2Rad));
                Place(l, Mathf.Min(l, RestsUpTo), hips, facing * Quaternion.Euler(b, 0, 0), out var position, out var rotation);
                float off = Vector3.Distance(position + rotation * tool.Head, point);
                float cost = off + .03f * (s / hip) + .0003f * Mathf.Abs(b - 28);
                if (cost < best) { best = cost; bow = b; lean = l; down = s; miss = off; }
            }
            if (roughly)
            {
                for (int s = 0; s <= 3; s++)
                    for (float b = 0; b <= 56; b += 8)
                        for (float l = 12; l <= leansUpTo; l += 6) Try(s * .1f * hip, b, l);
                return new Aimed { bow = bow, lean = lean, sink = down, miss = miss, cost = best };
            }
            for (int s = 0; s <= 6; s++)
                for (float b = 0; b <= 56; b += 4)
                    for (float l = 12; l <= leansUpTo; l += 2) Try(s * .05f * hip, b, l);
            float b0 = bow, l0 = lean, s0 = down;
            best = float.MaxValue;
            for (float s = Mathf.Max(0, s0 - .04f * hip); s <= s0 + .04f * hip; s += .01f * hip)
                for (float b = b0 - 4; b <= b0 + 4; b += 1)
                    for (float l = l0 - 2; l <= l0 + 2; l += .5f) Try(s, b, l);
            return new Aimed { bow = bow, lean = lean, sink = down, miss = miss, cost = best };
        }

        // The tool carried at the side in the upper hand alone (PhysicalCarry.AtSide).
        private void AtSide(Vector3 hips, Quaternion posture, out Vector3 position, out Quaternion rotation)
            => PhysicalCarry.AtSide(body, hands, hips, posture, out position, out rotation);

        // It takes up to its work a tool it was carrying: from however it is held now.
        public void TakeUp()
        {
            if (hands == null || hands.Held == null) return;
            from = hands.Held.position; fromTurn = hands.Held.rotation;
            Go(Phase.Recover);
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
            if (hands == null || hands.Held == null || back == null) return;
            float dt = Time.fixedDeltaTime;
            clock += dt;
            // It stands braced for the work, and at ease when it rests.
            if (balance == null) balance = GetComponent<PhysicalBalance>();
            if (balance != null)
            {
                if (phase == Phase.Rest) balance.Ease();
                else balance.Brace(StanceWider, StanceStagger);
            }
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
                    angle = rest;
                    back.Want(restBow);
                    body.Sink(sink);
                    // Holding it at rest tells the body how heavy it is for it: the upper hand goes up the handle
                    // towards the head by as much as that asks.
                    if (clock > .25f && !chose && !hands.Sliding(0))
                    {
                        feels += Mathf.Max(hands.Effort(0), hands.Effort(1)); feelSteps++;
                        if (feelSteps >= 12)
                        {
                            heavy = feels / feelSteps; chose = true;
                            if (hands.Holds(0) && hands.Holds(1))
                                hands.Slide(0, Mathf.Lerp(tool.SecondaryGrip.y, hands.HighestGrip, Mathf.InverseLerp(ChokeFrom, ChokeFull, heavy)));
                        }
                    }
                    if (clock >= settle && chose && !hands.Sliding(0) && hands.Holds(1))
                    {
                        low = high = headUp; worked = 0; bent = 0; steps = 0; now = default;
                        now.spent = Spent;
                        swung = true;
                        now.choked = hands.Holds(0) ? Mathf.InverseLerp(tool.SecondaryGrip.y, hands.HighestGrip, hands.GripAlong(0)) : 0;
                        if (trace != null)
                            trace.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                "Ready ends: it felt {0:0.00} heavy; efforts {1:0.00} {2:0.00}; wrists {3:0.000} {4:0.000} m from the shoulders of arms {5:0.000} m long; bow {6:0.0}; hips {7:0.000} m up, {8:0.000} m behind its feet; the tool leans {9:0.0}",
                                heavy, hands.Effort(0), hands.Effort(1), Vector3.Distance(body.HandPosition(0), body.ShoulderNow(0)), Vector3.Distance(body.HandPosition(1), body.ShoulderNow(1)), body.ArmReach,
                                body.BowNow, hips.y - transform.position.y, -Vector3.Dot(hips - transform.position, transform.forward), Lean(posture)));
                        Go(Phase.Lift);
                    }
                    break;
                case Phase.Lift:
                {
                    // The intention goes up only as fast as the tool follows it: as high as it will go.
                    float before = angle;
                    angle = Mathf.Min(angle, Mathf.Max(Mathf.MoveTowards(angle, Raised, LiftPace * dt), Lean(posture) - LiftLead));
                    stalled = before - angle > LiftPace * dt * .05f ? 0 : stalled + dt;
                    back.Want(Mathf.Lerp(restBow, RaisedBow, Mathf.InverseLerp(rest, Raised, angle)));
                    // The legs straighten under the lift.
                    body.Sink(Mathf.Lerp(sink, 0, Mathf.InverseLerp(rest, Raised, angle)));
                    high = Mathf.Max(high, headUp);
                    worked += Mathf.Max(hands.Effort(0), hands.Effort(1)); bent += back.Effort; steps++;
                    if (angle <= Raised + .1f || stalled > .8f || clock > 5)
                    {
                        now.liftTime = clock; now.reached = Mathf.InverseLerp(rest, Raised, angle); now.liftEffort = worked / Mathf.Max(1, steps); now.backEffort = bent / Mathf.Max(1, steps);
                        Go(Phase.Top);
                    }
                    break;
                }
                case Phase.Top:
                    high = Mathf.Max(high, headUp);
                    if (clock >= .12f) { now.lifted = high - low; blows = hands.Thing.HeadBlows; leanBefore = Lean(posture); headSpeed = hands.HeadVelocity.magnitude; Go(Phase.Drive); }
                    break;
                case Phase.Drive:
                    // The back goes first, and the arms follow when it is well on its way: the tool comes over last.
                    back.Want(blowBow);
                    body.Sink(sink);
                    // The upper hand slides down the handle to the lower one: the swing lengthens as it comes over.
                    if (hands.Holds(0) && hands.Holds(1)) hands.Slide(0, tool.SecondaryGrip.y);
                    if (back.BowNow < Mathf.Lerp(RaisedBow, blowBow, .6f)) break;
                    // All the arms have: the intention stays well ahead of the tool, along the same path, until the blow.
                    angle = Mathf.Min(through, Mathf.Max(angle, Lean(posture) + BlowLead)); hard = true;
                    if (hands.Thing.HeadBlows > blows)
                    {
                        // How fast the head was going at the step before it struck (the tool's weight is nearly all there).
                        now.struck = true; now.speed = headSpeed; now.energy = .5f * hands.ToolMass * now.speed * now.speed;
                        now.landed = hands.HeadPosition;
                    }
                    float leans = Lean(posture), speeds = hands.HeadVelocity.magnitude;
                    if (leanBefore < 0 && leans >= 0 && now.upright <= 0) now.upright = Mathf.Lerp(headSpeed, speeds, Mathf.InverseLerp(leanBefore, leans, 0));
                    leanBefore = leans;
                    headSpeed = speeds;
                    if (now.struck || clock > 1.5f)
                    {
                        // The blow is over. It means the tool to stay where the blow left its head, square in the hands
                        // again: it does not go on pushing into what it struck.
                        angle = Mathf.Clamp(Lean(posture), Raised, through);
                        Intend(angle, hips, posture, out _, out fromTurn);
                        from = hands.HeadPosition - fromTurn * tool.Head;
                        Go(Phase.Struck);
                    }
                    break;
                case Phase.Struck:
                    if (clock >= .35f)
                    {
                        if (Spent >= RestsAt) { rests++; grounded = HoldAsks > RestsOnlyBelow; Go(Phase.Rest); }
                        else Go(Phase.Recover);
                    }
                    break;
                case Phase.Rest:
                    // It stands up straight, which is what rests a back, and carries the tool as a tool is carried:
                    // in one hand at the side, held near its head where its weight is, the arm hanging.
                    // It straightens as the tool comes up with it: arms do not reach a tool on the block from upright.
                    back.Want(Mathf.Lerp(restBow, Mathf.Sin(clock * 2.4f) * 1.5f, Mathf.SmoothStep(0, 1, clock / 1.1f)));
                    if (grounded)
                    {
                        // Too heavy to rest holding: the lower hand goes to the end of the handle, the upper lets go,
                        // and the end is brought to hang at the side with the head left lying where it is, on the
                        // ground or on what it struck. The ground carries the tool; the arm only keeps it standing.
                        if (!hands.Trailing)
                        {
                            hands.Slide(1, hands.LowestGrip);
                            if (hands.Holds(0) && (hands.Sliding(1) || clock < .3f)) break;
                            endFrom = hands.GripPlace(1); groundClock = 0;
                        }
                        groundClock += dt;
                        float length = Vector3.Distance(tool.Head, new Vector3(0, hands.LowestGrip, 0));
                        Vector3 shoulder = unused[1];
                        Vector3 place = shoulder + body.FacingNow * Vector3.right * (.1f * body.ArmReach);
                        place.y = (body.FootPosition(0).y + body.FootPosition(1).y) * .5f + StoodUp * length;
                        // The knees give what the arm lacks to hold it there (a tall body with a short tool).
                        float lacks = shoulder.y - place.y - .93f * body.ArmReach;
                        body.Sink(Mathf.Clamp(body.SinkNow + lacks * .5f, 0, .2f * body.StandingHipHeight));
                        hands.WantEnd(1, Vector3.Lerp(endFrom, place, Mathf.SmoothStep(0, 1, groundClock / .9f)));
                    }
                    else
                    {
                        body.Sink(0);
                        if (hands.Holds(1))
                        {
                            hands.Slide(0, hands.HighestGrip);
                            if (clock > .25f && !hands.Sliding(0)) hands.Release(1);
                        }
                    }
                    if (clock >= 2.5f && Spent <= GoesOnAt)
                    {
                        // It takes the tool up to its work again from where it holds it.
                        from = hands.Held.position; fromTurn = hands.Held.rotation;
                        Go(Phase.Recover);
                    }
                    break;
                case Phase.Recover:
                    back.Want(restBow);
                    body.Sink(sink);
                    // After a rest the lower hand takes hold again, once the tool is back before the body.
                    if (!hands.Holds(1) && !hands.Reaching(1) && clock > .45f) hands.Grasp(1, tool.PrimaryGrip.y);
                    if (!hands.Holds(0) && !hands.Reaching(0) && clock > .45f) hands.Grasp(0, tool.SecondaryGrip.y);
                    // With both hands on it again, each goes back to its own place.
                    if (hands.Holds(0) && hands.Holds(1)) { hands.Slide(0, tool.SecondaryGrip.y); hands.Slide(1, tool.PrimaryGrip.y); }
                    if (clock >= .85f && hands.Holds(0) && hands.Holds(1) && !hands.Sliding(1)) { if (swung) results.Add(now); swung = false; angle = rest; settle = .3f; Go(Phase.Ready); }
                    break;
            }
            float bears = 1;
            if (phase == Phase.Struck) { position = from; rotation = fromTurn; }
            else if (phase == Phase.Rest && grounded)
            {
                // Until the lower hand has the end of the handle, the tool stays where the blow left it; after that it
                // is held by its end only.
                if (!hands.Trailing) hands.Want(from, fromTurn);
                return;
            }
            else if (phase == Phase.Rest)
            {
                AtSide(hips, posture, out var to, out var toTurn);
                float t = Mathf.SmoothStep(0, 1, clock / 1.3f);
                position = Vector3.Lerp(from, to, t); rotation = Quaternion.Slerp(fromTurn, toTurn, t);
            }
            else if (phase == Phase.Recover)
            {
                Intend(rest, hips, posture, out var to, out var toTurn);
                float t = Mathf.SmoothStep(0, 1, clock / .75f);
                position = Vector3.Lerp(from, to, t); rotation = Quaternion.Slerp(fromTurn, toTurn, t);
            }
            else Intend(angle, hips, posture, out position, out rotation);
            hands.Want(position, rotation, hard, bears);
            if (trace != null && results.Count == 0 && phase != Phase.Ready && phase != Phase.Recover)
                trace.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "{0} {1:0.00}s lean {2:0} wanted {3:0} bow {4:0} head {5:0.000} up {6:0.000} ahead at {7:0.0} m/s; blows {8} (head {9}); efforts {10:0.00} {11:0.00}; hips {12:0.000} behind, the back at {13:0.0}",
                    phase, clock, Lean(posture), angle, body.BowNow, hands.HeadPosition.y - transform.position.y,
                    Vector3.Dot(hands.HeadPosition - transform.position, transform.forward), hands.HeadVelocity.magnitude,
                    hands.Thing.Blows, hands.Thing.HeadBlows, hands.Effort(0), hands.Effort(1),
                    -Vector3.Dot(hips - transform.position, transform.forward), back.BowNow));
        }
    }
}
