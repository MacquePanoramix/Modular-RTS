using UnityEngine;

namespace WonderGather
{
    // The keeper of a body of its own (S3b; Docs/Design/TheBodysOwn.md): what keeps thirteen weighted parts on
    // joints standing, by what the legs' joints give against the ground and nothing else. One piece of code, used
    // by the body in the game (OwnBody) and by the bench that searches its settings (BodysOwnBench), so that what
    // is searched is what is played. Each step of the physics:
    //  - what the ground should push the body with is reckoned: its weight, a little more or less to keep its
    //    height, and sideways as much as brings its weight back from where it is going;
    //  - where on its soles that push must act, no further than the soles reach; where the ground really bore it
    //    in the last step is read from what touched its boots, and what it means is corrected by how far that
    //    fell short, but never off a sole;
    //  - each leg's hip, knee and ankle give what makes its boot push the ground so, and no more than they have;
    //  - its hips keep its trunk upright;
    //  - what pushes it from outside is felt (by how its weight really went), and it leans against it.
    // Every torque is put on the two parts a joint joins, equal and opposite. Whoever owns the body may live in
    // the middle of each step (Life): it says how far from where it stands the weight is held (Shift) and how its
    // hips roll (Roll).
    public sealed class OwnKeeper
    {
        // The parts, in this order (the left of a pair first).
        public const int Hips = 0, Trunk = 1, Head = 2, UpperArm = 3, Forearm = 5, Thigh = 7, Shin = 9, Foot = 11, Count = 13;
        // It is down when its weight is lower than this share of how high it stood, or its hips lean further than
        // this (degrees).
        public const float DownAt = .8f, DownLeaning = 35;

        // ---- Its settings. Found by a search on the three miners together (BodysOwnBench.Search, at the game's
        // fifty steps of the physics a second), not set by hand: change what the search is asked, run it, and put
        // what it finds here.
        // How quick it brings its weight back (a second); over how long it comes to feel a push from outside
        // (seconds); how fast it leans against one (a second), by what share of what would ease it, only for what
        // is beyond Comfort (metres from where its push acts at ease), and never leaving less sole than KeepsBehind
        // behind its weight; how fast it learns how far the ground's real push falls short of the one it meant (a
        // second).
        public static float Quick = 6.33f, Feels = .04005f, Leans = 2.9021f, LeansBy = .94264f, Comfort = .03266f, KeepsBehind = .02071f, Tracks = 6.8665f;
        // Each joint's spring gives all the joint's strength when it is FullAt degrees from its pose, but is no
        // stiffer than makes its own rate, times the step, Rule (by the turning weight of the lighter side of the
        // joint: a stiff spring between a heavy body and a light boot does not hold at the game's step); its damper
        // is Damped of what just stops that side swinging.
        public static float FullAt = 92.743f, Rule = .64038f, Damped = .38328f;
        // How hard its hips right its trunk, for each kilogram of it, and the share of that which damps it; how near
        // the edge of a sole its push may act (metres); how hard it keeps its height, and how that is damped.
        public static float Rights = 5.3963f, RightsDamped = .05f, Edge = .00435f, Rises = 224.2f, RisesDamped = 14.277f;

        private readonly ArticulationBody[] parts;
        private readonly int[] from;
        private readonly float[] masses, strength;
        private readonly bool[] hinge;
        private readonly Vector3[] anchors, soleAt, soleSize;
        private readonly GroundTouch[] touches;
        private readonly float whole;
        // Its state.
        private bool begun, hasBefore, realKnown;
        private Vector3 rest, holds, felt, wentBefore, weightBefore, actedBefore, realAt, shortBy, acted, stands;
        private Vector2 standsBy;
        private Quaternion upright;
        private float height;
        private readonly Vector3[] torque = new Vector3[Count], sole = new Vector3[2], at = new Vector3[2];
        private readonly float[] gave = new float[Count];

        // Where it wants the ground to push it is further the way its body tips: by Tilts of how far its hips have
        // tipped (times its height), and TiltsRate of how fast, over its rate of falling. (A pull above its weight
        // turns the whole body over, and a push that passes through its weight never turns it back.) Nought: not.
        public static float Tilts = 0, TiltsRate = 0;

        // ---- The step (S3b, step 3; what was read for it: the research of October 9, part 7). Set by hand to begin
        // with; to be found by a search on the three miners together, as the settings above were.
        // Whether it steps at all (off: it stands as it did, and goes down when it cannot).
        public static bool Steps = false;
        // It steps when where its weight is going has been further than StepsBeyond (metres) outside what its soles
        // can bear for StepsAfter seconds. The leg is unloaded over UnloadsIn seconds; its boot swings for SwingsIn,
        // lifted Clears metres in the middle; to StepsAhead times as far from the boot it stands on as its weight is
        // going now, and StepsFurther metres beyond; no further from its hip than Reach of the leg's length. Sideways
        // (its fall more than Sideways across it) it steps with the leg on the side it falls to; else with the leg
        // that bears less.
        public static float StepsBeyond = 0.0297f, StepsAfter = 0.0192f, UnloadsIn = 0.02f, SwingsIn = 0.1636f, Clears = 0.0672f;
        public static float StepsAhead = 1.8677f, StepsFurther = 0.0382f, Reach = 0.841f, Sideways = 0.7088f;
        // (How much of what pushes it counts in whether it must step, and where to: a blow is felt as a great push
        // for a moment, and is not one that lasts.)
        public static float StepsFelt = 0;
        // For this long after a boot lands (seconds), how its weight went is not taken for a push from outside.
        public static float LandsDeaf = 0.1706f;
        // While its boot swings, that leg's joints give all they have at SwingFullAt degrees from where the boot
        // should be (never stiffer than the rule), damped by SwingDamped of what just stops them swinging.
        public static float SwingFullAt = 22.8128f, SwingDamped = 0.2644f;
        // Its boot is carried there by a pull for each kilogram of the leg: SwingStiff for each metre it is from where
        // it should be, SwingSlows for each metre a second it lags; no more than SwingAtMost times the leg's weight.
        public static float SwingStiff = 318.134f, SwingSlows = 33.7976f, SwingAtMost = 1;
        // The hip of the leg it stands on takes up this share of what the swinging leg's hip gives; and its hips are
        // righted this many times as hard while it stands on one leg.
        public static float TakesUp = 0.3f, StepRights = 1.2439f, SwingAnkle = 12.4616f;
        // Afterwards the other boot is brought alongside: when its boots stand more than Astride (metres) from how
        // they stood, and it has been at rest (its weight going slower than CalmUnder, metres a second) for
        // ClosesAfter seconds. That step is got ready: its weight goes onto the leg it will stand on, to within
        // ReadyWithin (metres), for no longer than ReadiesAtMost seconds.
        public static float Astride = 0.0851f, CalmUnder = 0.1074f, ClosesAfter = 0.5351f, ReadyWithin = 0.0198f, ReadiesAtMost = 1;

        private readonly float[] lighter;
        private readonly Quaternion[] rested = new Quaternion[Count], madeTurned = new Quaternion[Count], madeLay = new Quaternion[Count];
        private readonly float[] stoodSpring = new float[Count], stoodDamper = new float[Count], thighLong = new float[2], shinLong = new float[2], bears = new float[2];
        private Vector2 madeApart;
        private int swings = -1, stoodLast = -1;
        private bool catching, readying, stiff;
        private float stepT, beyondFor, calmFor, readyFor, sinceLanded = 1e9f;
        private Vector3 stepFrom, stepTo, carries, wantedBefore;
        private int carrying = -1;
        private bool carried;

        // How many steps it has taken; whether a boot is off the ground or about to be; and which (-1: neither).
        public int Stepped { get; private set; }
        public bool Stepping => swings >= 0;
        public int Swings => swings;
        public Vector3 SwingWants, SwingIs, SwingTo, SwingFrom;

        // What whoever owns the body says: its life, lived in the middle of each step (the step's length; whether
        // it is being pushed; how its boots stand apart); how far from where it stands its weight is held; how its
        // hips roll from upright; and how far towards the middle of its boots it holds its weight (0: where the
        // posed body had it, near its heels).
        public System.Action<float, bool, Vector3> Life;
        public Vector3 Shift;
        public Quaternion Roll = Quaternion.identity;
        public float StandsMid;

        // What a joint's worked-out torque was in the last step (newton metres); how far outside its soles the
        // push it wanted lay (metres); how far it leans against what it feels, and what it feels (m/s2); how its
        // hips were turned when it began.
        public float[] Gave => gave;
        public float Outside { get; private set; }
        public Vector3 Leaning => holds - rest;
        public Vector3 Felt => felt;
        public Quaternion Upright => upright;
        public float Whole => whole;

        public OwnKeeper(ArticulationBody[] parts, int[] from, float[] masses, float[] strength, bool[] hinge, Vector3[] anchors, GroundTouch[] touches, Vector3[] soleAt, Vector3[] soleSize, float[] lighter = null)
        {
            this.parts = parts; this.from = from; this.masses = masses; this.strength = strength; this.hinge = hinge; this.anchors = anchors;
            this.touches = touches; this.soleAt = soleAt; this.soleSize = soleSize;
            float all = 0;
            for (int i = 0; i < Count; i++) all += masses[i];
            whole = all;
            // (For the step: how each part was turned, how each joint was sprung, and how its legs lay, as it was made.
            // The turning weight of the lighter side of each joint is whoever built it's to give; without it, no step.)
            this.lighter = lighter;
            Vector3 ahead = parts[Hips].transform.forward;
            for (int j = 1; j < Count; j++)
            {
                madeTurned[j] = parts[j].transform.rotation;
                rested[j] = Quaternion.Inverse(parts[from[j]].transform.rotation) * parts[j].transform.rotation;
                stoodSpring[j] = parts[j].xDrive.stiffness; stoodDamper[j] = parts[j].xDrive.damping;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector3 hip = JointAt(Thigh + i), knee = JointAt(Shin + i), ankle = JointAt(Foot + i);
                thighLong[i] = (knee - hip).magnitude; shinLong[i] = (ankle - knee).magnitude;
                Vector3 thighWay = (knee - hip).normalized, shinWay = (ankle - knee).normalized;
                madeLay[Thigh + i] = Quaternion.LookRotation(thighWay, Vector3.ProjectOnPlane(ahead, thighWay).normalized);
                madeLay[Shin + i] = Quaternion.LookRotation(shinWay, Vector3.ProjectOnPlane(ahead, shinWay).normalized);
            }
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        private Vector3 Weight()
        {
            Vector3 c = Vector3.zero;
            for (int i = 0; i < Count; i++) c += parts[i].worldCenterOfMass * masses[i];
            return c / whole;
        }

        private Vector3 WeightGoes()
        {
            Vector3 v = Vector3.zero;
            for (int i = 0; i < Count; i++) v += parts[i].GetPointVelocity(parts[i].worldCenterOfMass) * masses[i];
            return v / whole;
        }

        private Vector3 JointAt(int j) => parts[j].transform.TransformPoint(anchors[j]);

        // A joint is asked for a turn from the pose it was made in (the turn in the part's own frame).
        private void Aims(int j, Quaternion own)
        {
            own.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.right; angle = 0; }
            Vector3 by = angle * axis;
            parts[j].SetDriveTarget(ArticulationDriveAxis.X, by.x);
            if (hinge[j]) return;
            parts[j].SetDriveTarget(ArticulationDriveAxis.Y, by.y);
            parts[j].SetDriveTarget(ArticulationDriveAxis.Z, by.z);
        }

        // A leg's joints are sprung as they stand, or (while its boot swings) stiffer: all they have at SwingFullAt
        // degrees from where the boot should be, but never stiffer than the rule.
        private void Springs(int leg, bool swinging, float step)
        {
            for (int j = Shin + leg; j <= Foot + leg; j += 2)
            {
                float spring = stoodSpring[j], damper = stoodDamper[j];
                if (swinging)
                {
                    // (The rule is for a boot on the ground: in the air its ankle may be stiffer.)
                    spring = Mathf.Min(strength[j] / (SwingFullAt * Mathf.Deg2Rad), (j >= Foot ? SwingAnkle : 1) * Rule * Rule / (step * step) * lighter[j]);
                    damper = SwingDamped * 2 * Mathf.Sqrt(spring * lighter[j]);
                }
                var drive = parts[j].xDrive; drive.stiffness = spring; drive.damping = damper; parts[j].xDrive = drive;
                if (hinge[j]) continue;
                drive = parts[j].yDrive; drive.stiffness = spring; drive.damping = damper; parts[j].yDrive = drive;
                drive = parts[j].zDrive; drive.stiffness = spring; drive.damping = damper; parts[j].zDrive = drive;
            }
        }

        // Where on its soles a push wanted at a place can act: each boot bears its share along its own middle line
        // (only a push wanted outside both is taken to a boot's outer side); standing on one boot, anywhere on that
        // sole. The share its right boot bears is by how far across the push lies, or what a step says.
        private Vector3 Borne(Vector3 acts, int single, int stance, float unloaded, out float share)
        {
            Vector3 over = Flat(sole[1] - sole[0]);
            Vector3 beside = over.sqrMagnitude > 1e-8f ? over.normalized : Vector3.right;
            for (int i = 0; i < 2; i++)
            {
                Transform foot = parts[Foot + i].transform;
                float across = Vector3.Dot(Flat(acts - sole[i]), beside);
                float outward = i == 0 ? Mathf.Min(across, 0) : Mathf.Max(across, 0);
                Vector3 local = foot.InverseTransformPoint(single == i ? acts : acts - beside * (across - outward));
                Vector3 half = .5f * soleSize[i];
                local = new Vector3(Mathf.Clamp(local.x, -half.x + Edge, half.x - Edge), -half.y, Mathf.Clamp(local.z, -half.z + Edge, half.z - Edge));
                at[i] = foot.TransformPoint(local);
            }
            share = Mathf.Clamp01(Vector3.Dot(Flat(acts - sole[0]), over) / Mathf.Max(1e-6f, over.sqrMagnitude));
            if (stance >= 0) share = Mathf.Lerp(share, stance, unloaded);
            return Vector3.Lerp(at[0], at[1], share);
        }

        // One step of keeping itself up. (False: it is down.)
        public bool Keep(float step)
        {
            float g = Physics.gravity.magnitude;
            Vector3 weight = Weight(), goes = WeightGoes();
            // Where the ground really bore it in the last step.
            {
                Vector3 bore = Vector3.zero; float lifted = 0;
                for (int i = 0; i < 2; i++) { bore += touches[i].Where; lifted += touches[i].Up; bears[i] = touches[i].Up; touches[i].Clear(); }
                realKnown = lifted > 1e-6f;
                if (realKnown) realAt = bore / lifted;
            }
            for (int i = 0; i < 2; i++) sole[i] = parts[Foot + i].transform.TransformPoint(soleAt[i]);
            // (While a boot swings, the ground is where the other stands.)
            bool inAir = swings >= 0 && !readying && stepT >= UnloadsIn;
            float ground = inAir ? sole[1 - swings].y : .5f * (sole[0].y + sole[1].y), high = weight.y - ground;
            if (!begun)
            {
                begun = true; height = high; upright = parts[Hips].transform.rotation;
                rest = Flat(weight); holds = rest;
                Vector3 between = Flat(.5f * (sole[0] + sole[1])), beside0 = Flat(sole[1] - sole[0]).normalized;
                standsBy = new Vector2(Vector3.Dot(rest - between, beside0), (1 - StandsMid) * Vector3.Dot(rest - between, Vector3.Cross(beside0, Vector3.up)));
                Vector3 ahead0 = Flat(upright * Vector3.forward).normalized, across0 = Vector3.Cross(Vector3.up, ahead0);
                madeApart = new Vector2(Vector3.Dot(sole[1] - sole[0], across0), Vector3.Dot(sole[1] - sole[0], ahead0));
            }
            // Down: it goes over into the fall that is built.
            if (float.IsNaN(high) || high < DownAt * height || Vector3.Angle(Vector3.up, parts[Hips].transform.up) > DownLeaning) return false;

            float rise = Mathf.Clamp(Rises * (height - high) - RisesDamped * goes.y, -.5f * g, g);
            // A standing body falls away from where the ground pushes it, at a rate of its own (the root of gravity
            // over its height). So what matters is where its weight is going: its place, and its speed over that rate.
            float falls = Mathf.Sqrt((g + rise) / Mathf.Max(.1f, high));
            sinceLanded += step;
            if (hasBefore && Feels > 0 && sinceLanded > LandsDeaf)
            {
                // What pushes it from outside: how its weight really went, beyond what the ground's push gave it.
                Vector3 pushedAt = realKnown && Tracks > 0 ? realAt : actedBefore;
                Vector3 seen = Flat(goes - wentBefore) / step - falls * falls * Flat(weightBefore - pushedAt);
                felt = Vector3.ClampMagnitude(Vector3.Lerp(felt, seen, Mathf.Clamp01(step / Feels)), 4);
            }
            if (hasBefore && realKnown && Tracks > 0) shortBy = Vector3.ClampMagnitude(shortBy + Tracks * step * Flat(actedBefore - realAt), .05f);
            // The life in it: where it holds its weight between its boots, and what its joints are asked for.
            // (Boots that slid under a push take where it stands with them; while a boot swings, where it stands is
            // left where it was.)
            Vector3 apart = Flat(sole[1] - sole[0]);
            if (swings < 0) stands = Flat(.5f * (sole[0] + sole[1])) + apart.normalized * standsBy.x + Vector3.Cross(apart.normalized, Vector3.up) * standsBy.y;
            Life?.Invoke(step, felt.magnitude / (falls * falls) > Comfort, apart);
            rest = stands + Shift;
            Vector3 going = Flat(weight) + Flat(goes) / falls;

            // ---- The step: when where its weight is going cannot be kept within its soles, a leg is unloaded, lifted,
            // and its boot put down where its weight will be going. Afterwards, at rest, the other boot is brought
            // alongside (that one is got ready: its weight goes onto the leg it will stand on first).
            int single = -1, stance = -1;
            carrying = -1;
            float unloaded = 0;
            if (Steps && lighter != null)
            {
                Vector3 aheadNow = Flat(parts[Hips].transform.forward).normalized, acrossNow = Vector3.Cross(Vector3.up, aheadNow);
                if (swings < 0)
                {
                    // (Where the ground would have to push it for it to stand as it is: where its weight is going, and
                    // further for what pushes it. Can its soles bear that?)
                    Vector3 needs = going + StepsFelt * felt / (falls * falls);
                    Vector3 kept = Borne(new Vector3(needs.x, ground, needs.z), -1, -1, 0, out float bearing);
                    float beyond = Flat(needs - kept).magnitude;
                    beyondFor = beyond > StepsBeyond ? beyondFor + step : 0;
                    Vector3 madeNow = acrossNow * madeApart.x + aheadNow * madeApart.y;
                    bool calm = beyond <= 1e-4f && Flat(goes).magnitude < CalmUnder && felt.magnitude / (falls * falls) < Comfort;
                    calmFor = calm && (apart - madeNow).magnitude > Astride ? calmFor + step : 0;
                    if (beyondFor >= StepsAfter)
                    {
                        // A catch. Sideways, the leg on the side it falls to; else the leg that bears less.
                        Vector3 way = Flat(needs - kept).normalized;
                        float aside = Vector3.Dot(way, apart.normalized);
                        swings = Mathf.Abs(aside) > Sideways ? (aside > 0 ? 1 : 0) : (bearing > .5f ? 0 : 1);
                        Vector3 stood = Flat(sole[1 - swings]);
                        stepTo = stood + (needs - stood) * StepsAhead + way * StepsFurther;
                        catching = true; readying = false;
                    }
                    else if (calmFor >= ClosesAfter)
                    {
                        // The other boot is brought alongside: the leg that stood through the last step.
                        swings = stoodLast >= 0 ? stoodLast : 0;
                        stepTo = Flat(sole[1 - swings]) + (swings == 1 ? madeNow : -madeNow);
                        catching = false; readying = true; readyFor = 0;
                    }
                    if (swings >= 0)
                    {
                        // (Its boots are not put across each other, nor further than the leg reaches.)
                        Vector3 own = swings == 1 ? acrossNow : -acrossNow, stood = Flat(sole[1 - swings]);
                        float clear = Vector3.Dot(stepTo - stood, own), least = .6f * Mathf.Abs(madeApart.x);
                        if (clear < least) stepTo += own * (least - clear);
                        Vector3 hipOver = Flat(JointAt(Thigh + swings));
                        stepTo = hipOver + Vector3.ClampMagnitude(stepTo - hipOver, Reach * (thighLong[swings] + shinLong[swings]));
                        stepFrom = Flat(sole[swings]);
                        stepT = 0; stiff = false; beyondFor = 0; calmFor = 0; carried = false;
                        Stepped++;
                    }
                }
                if (swings >= 0)
                {
                    stance = 1 - swings;
                    if (readying)
                    {
                        // Got ready: its weight onto the leg it will stand on.
                        rest = Flat(sole[stance]);
                        readyFor += step;
                        if (Flat(going - rest).magnitude < ReadyWithin || readyFor > ReadiesAtMost) readying = false;
                    }
                    else
                    {
                        if (!catching) rest = Flat(sole[stance]);
                        stepT += step;
                        unloaded = Mathf.Clamp01(stepT / Mathf.Max(1e-3f, UnloadsIn));
                        float u = (stepT - UnloadsIn) / Mathf.Max(.05f, SwingsIn);
                        if (u >= 0)
                        {
                            single = stance;
                            if (!stiff) { stiff = true; Springs(swings, true, step); }
                            // Where its boot should be now: along the ground in one hump of speed, lifted in the middle.
                            float uu = Mathf.Clamp01(u), along = uu * uu * uu * (10 + uu * (6 * uu - 15));
                            Vector3 soleWants = Vector3.Lerp(stepFrom, stepTo, along);
                            soleWants.y = sole[stance].y + Clears * Mathf.Sin(Mathf.PI * uu);
                            SwingWants = soleWants; SwingIs = sole[swings]; SwingTo = stepTo; SwingFrom = stepFrom;
                            // What carries its boot there: a pull on the boot towards where it should be, and against how
                            // it lags, and the leg's own weight; given by its hip and knee.
                            {
                                float leg = masses[Thigh + swings] + masses[Shin + swings] + masses[Foot + swings];
                                Vector3 goesNow = parts[Foot + swings].GetPointVelocity(sole[swings]);
                                Vector3 shouldGo = carried ? (soleWants - wantedBefore) / step : goesNow;
                                carries = Vector3.ClampMagnitude(leg * (SwingStiff * (soleWants - sole[swings]) + SwingSlows * (shouldGo - goesNow)), SwingAtMost * leg * g) + leg * g * Vector3.up;
                                carrying = swings; carried = true; wantedBefore = soleWants;
                            }
                            Quaternion faces = Quaternion.AngleAxis(Vector3.SignedAngle(Flat(upright * Vector3.forward), aheadNow, Vector3.up), Vector3.up);
                            Quaternion bootWants = faces * madeTurned[Foot + swings];
                            // Its hip and knee, for its ankle to be there: its knee bends forward.
                            Vector3 hip = JointAt(Thigh + swings), reach = soleWants + bootWants * (anchors[Foot + swings] - soleAt[swings]) - hip;
                            float l1 = thighLong[swings], l2 = shinLong[swings];
                            float far = Mathf.Clamp(reach.magnitude, Mathf.Abs(l1 - l2) + .01f, (l1 + l2) * .995f);
                            Vector3 line = reach.normalized, pole = Vector3.ProjectOnPlane(parts[Hips].transform.forward, line).normalized;
                            float opens = Mathf.Acos(Mathf.Clamp((l1 * l1 + far * far - l2 * l2) / (2 * l1 * far), -1, 1));
                            Vector3 thighWay = line * Mathf.Cos(opens) + pole * Mathf.Sin(opens);
                            Vector3 shinWay = (line * far - thighWay * l1).normalized;
                            Quaternion thighWants = Quaternion.LookRotation(thighWay, pole) * Quaternion.Inverse(madeLay[Thigh + swings]) * madeTurned[Thigh + swings];
                            Quaternion shinWants = Quaternion.LookRotation(shinWay, Vector3.ProjectOnPlane(parts[Hips].transform.forward, shinWay).normalized) * Quaternion.Inverse(madeLay[Shin + swings]) * madeTurned[Shin + swings];
                            Aims(Thigh + swings, Quaternion.Inverse(parts[Hips].transform.rotation * rested[Thigh + swings]) * thighWants);
                            Aims(Shin + swings, Quaternion.Inverse(thighWants * rested[Shin + swings]) * shinWants);
                            Aims(Foot + swings, Quaternion.Inverse(parts[Shin + swings].transform.rotation * rested[Foot + swings]) * bootWants);
                            // It lands: at the end of its swing, or sooner if its boot meets the ground on the way down.
                            if (u >= 1 || (u > .55f && bears[swings] > 1e-4f))
                            {
                                Springs(swings, false, step);
                                if (!catching)
                                    for (int i = 0; i < 2; i++)
                                        for (int j = Thigh + i; j <= Foot + i; j += 2) Aims(j, Quaternion.identity);
                                stoodLast = stance;
                                swings = -1; single = -1; stance = -1; unloaded = 0; carrying = -1; carried = false;
                                holds = going; sinceLanded = 0;
                            }
                        }
                    }
                }
            }

            // Where it wants the ground to push it: beyond where its weight is going, by how far that is from where it
            // holds it (catching itself on one leg, as near where its weight is going as its sole reaches).
            // (Written in this order so that, with no step, it reckons exactly as it did.)
            Vector3 wants = swings >= 0 && catching ? going + felt / (falls * falls) : going + Quick / falls * (going - holds) + felt / (falls * falls);
            if (Tilts > 0 || TiltsRate > 0)
            {
                // (How its hips are tipped from how they should be, as a turn; the way their top has gone is that
                // turn across upright.)
                Quaternion tipped = parts[Hips].transform.rotation * Quaternion.Inverse(Roll * upright);
                tipped.ToAngleAxis(out float tip, out Vector3 about);
                if (tip > 180) tip -= 360;
                if (!float.IsNaN(about.x) && !float.IsInfinity(about.x))
                    wants += high * (Tilts * Flat(Vector3.Cross(about * (tip * Mathf.Deg2Rad), Vector3.up)) + TiltsRate / falls * Flat(Vector3.Cross(parts[Hips].angularVelocity, Vector3.up)));
            }
            Vector3 acts = new Vector3(wants.x, ground, wants.z);
            acted = Borne(acts, single, stance, unloaded, out float share);
            Outside = Flat(acts - acted).magnitude;
            // The push can only act where the soles are: it pushes sideways only as a push from there, through its
            // weight, does.
            Vector3 sideways = Flat(weight - acted) * ((g + rise) / Mathf.Max(.1f, high));
            Vector3 push = whole * (sideways + Vector3.up * (g + rise));

            // It leans against what it feels, for what it cannot take standing as it is, keeping sole behind its weight.
            {
                Vector3 asked = felt / (falls * falls);
                Vector3 leansTo = rest;
                if (asked.magnitude > Comfort && swings < 0)
                {
                    Vector3 way = -asked.normalized;
                    float reach = 0;
                    for (int i = 0; i < 2; i++)
                        for (int c = 0; c < 4; c++)
                        {
                            Vector3 corner = parts[Foot + i].transform.TransformPoint(new Vector3((c & 1) == 0 ? -.5f * soleSize[i].x : .5f * soleSize[i].x, -.5f * soleSize[i].y, (c & 2) == 0 ? -.5f * soleSize[i].z : .5f * soleSize[i].z));
                            reach = Mathf.Max(reach, Vector3.Dot(Flat(corner) - rest, way));
                        }
                    leansTo = rest + way * Mathf.Min(LeansBy * (asked.magnitude - Comfort), Mathf.Max(0, reach - KeepsBehind));
                }
                holds = Vector3.Lerp(holds, leansTo, Mathf.Clamp01(Leans * step));
            }
            wentBefore = goes; weightBefore = weight; actedBefore = acted; hasBefore = true;

            // What each leg's joints give to make its boot push the ground so.
            for (int j = 0; j < Count; j++) torque[j] = Vector3.zero;
            for (int i = 0; i < 2; i++)
            {
                Vector3 pushes = (i == 0 ? 1 - share : share) * push;
                // (What it has learnt of the ground's push falling short moves where it means the push to act, but
                // never off the sole: a push asked for beyond a sole's edge only tips the boot.)
                Transform foot = parts[Foot + i].transform;
                Vector3 means = foot.InverseTransformPoint(at[i] + shortBy), half = .5f * soleSize[i];
                means = foot.TransformPoint(new Vector3(Mathf.Clamp(means.x, -half.x + Edge, half.x - Edge), -half.y, Mathf.Clamp(means.z, -half.z + Edge, half.z - Edge)));
                for (int j = Thigh + i; j <= Foot + i; j += 2)
                    torque[j] -= Vector3.Cross(means - JointAt(j), pushes);
                // (The leg whose boot swings: its hip and knee carry the boot.)
                if (carrying == i)
                    for (int j = Thigh + i; j <= Shin + i; j += 2) torque[j] += Vector3.Cross(sole[i] - JointAt(j), carries);
            }
            // Its hips keep its trunk as upright as it began (rolled, as it favours a leg).
            {
                Quaternion off = Roll * upright * Quaternion.Inverse(parts[Hips].transform.rotation);
                off.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180) angle -= 360;
                if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.zero; angle = 0; }
                float spring = Rights * whole * (carrying >= 0 ? StepRights : 1);
                Vector3 rights = spring * (angle * Mathf.Deg2Rad) * axis - RightsDamped * spring * parts[Hips].angularVelocity;
                torque[Thigh] -= (1 - share) * rights;
                torque[Thigh + 1] -= share * rights;
            }
            // (What the swinging leg's hip gives would turn its hips the other way: the hip of the leg it stands on
            // takes that up, into the ground.)
            if (carrying >= 0) torque[Thigh + 1 - carrying] -= TakesUp * torque[Thigh + carrying];
            for (int j = Thigh; j < Count; j++)
            {
                // (A hinge gives only about its own line; what is across it is borne by the joint itself.)
                if (hinge[j]) { Vector3 line = parts[j].transform.right; torque[j] = line * Vector3.Dot(line, torque[j]); }
                torque[j] = Vector3.ClampMagnitude(torque[j], strength[j]);
                if (float.IsNaN(torque[j].x) || float.IsNaN(torque[j].y) || float.IsNaN(torque[j].z)) torque[j] = Vector3.zero;
                gave[j] = torque[j].magnitude;
                // Equal and opposite, on the two parts the joint joins: nothing pushes the body from nowhere.
                parts[j].AddTorque(torque[j]);
                parts[from[j]].AddTorque(-torque[j]);
            }
            return true;
        }
    }
}
