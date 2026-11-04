using System;
using UnityEngine;

namespace WonderGather
{
    // What a body weighs and how strong it is (S3: Docs/Design/ThePhysicalBody.md).
    //
    // Its weight is measured on its model, part by part (Art/Blender/Worker/weights.py): each part's mass sits at its
    // own place in its own bone, so the body's centre of mass is wherever its pose puts it.
    //
    // Its strength is the most each working joint can give. A joint's capacity is built from the limb's own thickness
    // (a muscle's force grows with its cross-section, and its leverage with its breadth), times one number, Strength,
    // which is 1 for an ordinary body of that build. The figures for an ordinary grown person's arm are the
    // reference; they are a first calibration, to be set on the bench.
    public sealed class PhysicalBody : MonoBehaviour
    {
        [Serializable]
        public struct Part
        {
            public Transform bone;
            public float mass;
            // Its centre, in the bone's own space.
            public Vector3 centre;
        }

        // An ordinary grown person's arm: its thickness as the models measure a limb (a radius), and what its joints
        // can give at most, held still: the shoulder and the elbow in newton metres, the wrist, and the hand's hold
        // in newtons.
        private const float ArmReference = .047f, ShoulderReference = 70, ElbowReference = 60, WristReference = 12, HoldReference = 400;
        // An ordinary back holds up about this many times its own upper body bent level (a grown person's upper body
        // bent level asks about 130 N m of a back that can give about 300).
        private const float BackHolds = 2.4f;
        // An ordinary knee holds up this many times its share of its own body with the thigh level: legs are as strong
        // as the body they carry needs, whatever their thickness (as the back is). A first setting.
        private const float KneeHolds = 1.6f;

        [SerializeField] private Part[] parts = new Part[0];
        // The arms' parts (upper arm, forearm, hand; left then right), as indices into parts.
        [SerializeField] private int[] arms = new int[0];
        [SerializeField] private float mass, armRadius, legRadius;
        // The parts the back carries (everything above the hips), as indices into parts; and what they ask of the
        // back when the body is bent level: their mass times how far from the hips they are, standing (kg m).
        [SerializeField] private int[] upper = new int[0];
        [SerializeField] private float upperAsks;
        [SerializeField, Range(.1f, 4)] private float strength = 1;

        public float Mass => mass;
        public float ArmRadius => armRadius;
        public float LegRadius => legRadius;
        public int PartCount => parts.Length;
        public Part PartAt(int index) => parts[index];
        public bool Ready => parts.Length > 0 && arms.Length == 6 && upper.Length > 0 && mass > 0 && armRadius > 0;
        // Tiredness (S3, step 5). Each group of muscles that works has a share of itself that is spent for now. Work
        // spends it, the harder the faster; rest brings it back, the sooner the less the muscles are doing. What a
        // group can give is what is not spent. The paces here are a first setting: all-out work spends a third of a
        // group in about ten seconds, and a spent third comes back in about half a minute of rest.
        public enum Muscles { LeftArm, RightArm, Back, Legs }
        private const float Tires = .04f, Restores = .012f, AtRest = 3.5f, MostSpent = .85f;
        // A muscle working at less than this share of what it has does not tire: it can go on all day.
        private const float Sustains = .18f;
        private readonly float[] spent = new float[4];
        private readonly bool[] worked = new bool[4];
        private readonly float[] gave = new float[4];
        public static Muscles Arm(int side) => side == 0 ? Muscles.LeftArm : Muscles.RightArm;
        // The share of a group that is spent (0: fresh), and the share that is not.
        public float Spent(Muscles muscles) => spent[(int)muscles];
        public float Fresh(Muscles muscles) => 1 - spent[(int)muscles];
        // The share of what it has now that a group gave at the last step (0: it rested).
        public float Effort(Muscles muscles) => gave[(int)muscles];
        // Said once a physics step by whatever works a group: the share of what it has now that it gave.
        public void Worked(Muscles muscles, float effort, float dt)
        {
            int i = (int)muscles;
            worked[i] = true;
            gave[i] = Mathf.Clamp01(effort);
            Tire(i, gave[i], dt);
        }
        private void Tire(int i, float effort, float dt)
        {
            float rests = Restores * (1 + (AtRest - 1) * (1 - effort) * (1 - effort)) * spent[i];
            float tires = Tires * Mathf.Max(0, effort - Sustains) / (1 - Sustains) * (1 - spent[i]);
            spent[i] = Mathf.Clamp(spent[i] + (tires - rests) * dt, 0, MostSpent);
        }
        // Fresh again, at once (a new day, a new try).
        public void Refresh() => Array.Clear(spent, 0, spent.Length);
        // Where this body carries its weight when it stands at ease, as a share of the way from its heels to its
        // toes: its own way of standing, measured the first time it has stood still for a moment with nothing asked
        // of it. NaN until then.
        public float StandsOn { get; private set; } = float.NaN;
        private ProceduralBiped stance;
        private int stood;

        private void MeasureStance()
        {
            if (stance == null && !TryGetComponent(out stance)) return;
            bool still = stance.Ready && stance.CurrentGait == ProceduralBiped.Gait.Standing && stance.FootPlanted(0) && stance.FootPlanted(1)
                && Mathf.Abs(stance.BowNow) < .5f && stance.LeanNow.sqrMagnitude < 1e-8f && stance.SinkNow < .001f && !stance.Guided;
            stood = still ? stood + 1 : 0;
            if (stood < 15) return;
            stance.Sole(0, out Vector3 leftHeel, out Vector3 leftToe, out _);
            stance.Sole(1, out Vector3 rightHeel, out Vector3 rightToe, out _);
            Vector3 heels = (leftHeel + rightHeel) * .5f, along = (leftToe + rightToe) * .5f - heels;
            along.y = 0;
            Vector3 from = CentreOfMass() - heels;
            from.y = 0;
            StandsOn = Mathf.Clamp(Vector3.Dot(from, along) / Mathf.Max(1e-6f, along.sqrMagnitude), .15f, .65f);
        }

        private void FixedUpdate()
        {
            if (float.IsNaN(StandsOn) && Ready) MeasureStance();
            // Muscles nothing worked at the last step are resting.
            for (int i = 0; i < spent.Length; i++)
            {
                if (!worked[i]) { gave[i] = 0; Tire(i, 0, Time.fixedDeltaTime); }
                worked[i] = false;
            }
        }

        // 1: ordinary for this body's build.
        public float Strength { get => strength; set => strength = Mathf.Clamp(value, .1f, 4); }
        // How much of each region is muscle, against an ordinary body of its size (1): its arms, legs, back and neck
        // (Luis, November 3: "a strengh stat ... how much of it is muscle", "maybe even for body regions"). Each region's
        // joints give so much more or less; Strength is the whole body's. A first form, for the creator to set later
        // (with the fat and muscle a region shows: Luis, the same day); 1 everywhere until then.
        public enum Regions { Arms, Legs, Back, Neck }
        [SerializeField] private float armsMuscle = 1, legsMuscle = 1, backMuscle = 1, neckMuscle = 1;
        public float MuscleOf(Regions region) => region == Regions.Arms ? armsMuscle : region == Regions.Legs ? legsMuscle : region == Regions.Back ? backMuscle : neckMuscle;
        // What the region's share does to its strength under the rule in use: the share under strength from muscle; nothing
        // under the rule before (Need), which is as it was without them (so that -strengthRule need compares with it:
        // November 4, the miners' shares had made Round's arms a fifth weaker under it).
        public float MuscleGives(Regions region) => Rule == Rules.Muscle ? MuscleOf(region) : 1;
        // What a body made before strength from muscle keeps of the rule before, joint by joint: the three miners, whose
        // limbs' thickness was drawn, not weighed as muscle. Each is what makes strength from muscle give that joint what
        // the rule before gave it, so that they keep their abilities exactly, and only bodies made since show their build.
        // 1 for a body made since. Apart from the creator's share of muscle for each region, which is 1 for the three.
        // (November 4: one share for both arm joints, as first done, left each within 0.3 to 0.8% and moved two of Round's
        // swing tests across their thresholds; the reviewers asked for the cause to be mended, not the tests.)
        [SerializeField] private float shoulderKept = 1, elbowKept = 1, kneeKept = 1;
        public float ShoulderKept => shoulderKept;
        public float ElbowKept => elbowKept;
        public float KneeKept => kneeKept;
        // Called by MinerSetup when a miner is made, and on the three as they are.
        public void KeepsWhatItHad()
        {
            armsMuscle = legsMuscle = backMuscle = neckMuscle = 1;
            shoulderKept = elbowKept = kneeKept = 1;
            var was = Rule;
            float shoulder, elbow;
            try { Rule = Rules.Need; shoulder = ShoulderCapacity; elbow = ElbowCapacity; }
            finally { Rule = was; }
            // (The knee's need from the body's own saved measures: until a body wakes, its posed body still holds the
            // first body's, a 2.2 m one's, and the knee came out more than twice as strong, November 4.)
            if (stance == null) TryGetComponent(out stance);
            float thigh = stance != null ? stance.SavedProportions.legSegment : .4f;
            float knee = KneeHolds * mass * Physics.gravity.magnitude * .5f * thigh * strength;
            float shoulderMuscle = ShoulderMuscle * Build * strength, elbowMuscle = ElbowMuscle * Build * strength, kneeMuscle = KneeMuscle * LegBuild * strength;
            if (shoulderMuscle > 0) shoulderKept = shoulder / shoulderMuscle;
            if (elbowMuscle > 0) elbowKept = elbow / elbowMuscle;
            if (kneeMuscle > 0) kneeKept = knee / kneeMuscle;
            if (!(shoulderKept > .1f && shoulderKept < 10 && elbowKept > .1f && elbowKept < 10 && kneeKept > .1f && kneeKept < 10))
                throw new InvalidOperationException($"{name} would keep {shoulderKept:F2}, {elbowKept:F2} and {kneeKept:F2} of strength from muscle at its shoulder, elbow and knee: its body is not as weighed.");
        }

        public void SetMuscle(Regions region, float share)
        {
            share = Mathf.Clamp(share, .1f, 4);
            if (region == Regions.Arms) armsMuscle = share; else if (region == Regions.Legs) legsMuscle = share; else if (region == Regions.Back) backMuscle = share; else neckMuscle = share;
        }

        // upperParts: the parts above the hips; hips: where the hips are now (the body standing as modelled).
        public void Configure(Part[] weighed, int[] armParts, int[] upperParts, Vector3 hips, float arm, float leg)
        {
            if (weighed == null || weighed.Length == 0 || armParts == null || armParts.Length != 6 || upperParts == null || upperParts.Length == 0 || !(arm > 0) || !(leg > 0))
                throw new ArgumentException("A body's weights need its parts, its arms' parts, its upper body's parts and its limbs' thickness.");
            foreach (var p in weighed) if (p.bone == null || !(p.mass > 0)) throw new ArgumentException("A part of the body has no bone or no weight.");
            foreach (int i in armParts) if (i < 0 || i >= weighed.Length) throw new ArgumentException("An arm's part is missing from the body's weights.");
            foreach (int i in upperParts) if (i < 0 || i >= weighed.Length) throw new ArgumentException("A part of the upper body is missing from the body's weights.");
            parts = weighed; arms = armParts; upper = upperParts; armRadius = arm; legRadius = leg;
            shoulderNeeds = elbowNeeds = -1;
            mass = 0;
            foreach (var p in parts) mass += p.mass;
            upperAsks = 0;
            foreach (int i in upper) upperAsks += parts[i].mass * Vector3.Distance(parts[i].bone.TransformPoint(parts[i].centre), hips);
        }

        // The upper body as the back carries it now: its mass, where its weight is, and how hard it is to turn about
        // the hips.
        public void UpperBody(Vector3 hips, out float kilograms, out Vector3 centre, out float inertia)
        {
            kilograms = 0; inertia = 0;
            Vector3 sum = Vector3.zero;
            foreach (int i in upper)
            {
                var p = parts[i];
                Vector3 at = p.bone.TransformPoint(p.centre);
                kilograms += p.mass; sum += at * p.mass; inertia += p.mass * (at - hips).sqrMagnitude;
            }
            centre = kilograms > 0 ? sum / kilograms : hips;
            inertia = Mathf.Max(inertia, 1e-3f);
        }

        // The most the back gives, in newton metres: fresh, and now.
        public float BackCapacity => BackHolds * upperAsks * Physics.gravity.magnitude * strength * MuscleGives(Regions.Back);
        public float BackNow => BackCapacity * Fresh(Muscles.Back);

        public Vector3 CentreOfMass()
        {
            Vector3 sum = Vector3.zero;
            foreach (var p in parts) sum += p.bone.TransformPoint(p.centre) * p.mass;
            return mass > 0 ? sum / mass : transform.position;
        }

        // How a limb's thickness scales what its joints give (torque), and what its hand holds (force).
        private float Build => Mathf.Pow(armRadius / ArmReference, 3);
        private float LegBuild => Mathf.Pow(legRadius / LegReference, 3);
        // But a miner's arms are as strong as its own body needs, as its legs are (Luis, October 14: "we can go with
        // A and adapt the rest to it as necessary"): an arm's shoulder holds ArmHolds times its share of its upper
        // body (half of everything above its hips) with its upper arm level, and its elbow the same at the length of
        // its forearm. That is what pushing its chest off the ground asks, with the margin its knees have (KneeHolds).
        // Never less than its thickness gives. (Until then an arm's strength came from its thickness alone: Small's
        // gave a third of an ordinary arm's, its head is a quarter of its weight, and it could not push its chest off
        // the ground. ArmsAsBodyNeeds false: so again, to compare.)
        private const float ArmHolds = 1.6f;
        public static bool ArmsAsBodyNeeds = true;
        // Strength from muscle (Luis, November 3, 2026: "my intention was always the alternative"; Docs/Design/AnyBody/
        // body_map.md, 1.5): a limb's joints give what its thickness gives (the cube of its radius: a muscle's cross-section
        // times its lever), times the creator's strength, whatever the body it carries, so that a heavy body on thin limbs,
        // or a giant, is weaker for its weight. The references are set so that the three miners' mean is what it was under
        // the rule before (Need: as strong as its own body needs). The back and the neck stay as before under both, until
        // their thickness is modelled. (The three miners, made before it, keep what each joint had: KeepsWhatItHad.
        // Need: the rule before, to compare.)
        public enum Rules { Need, Muscle }
        public static Rules Rule = Rules.Muscle;
        private const float ShoulderMuscle = 121.3f, ElbowMuscle = 104.5f, KneeMuscle = 137.5f, LegReference = .046f;
        private float shoulderNeeds = -1, elbowNeeds = -1;
        private void ArmNeeds()
        {
            if (shoulderNeeds >= 0 || !Ready) return;
            float upperKilograms = 0;
            foreach (int i in upper) upperKilograms += parts[i].mass;
            float upperArm = 0, forearm = 0;
            for (int side = 0; side < 2; side++)
            {
                Vector3 shoulder = parts[arms[side * 3]].bone.position, elbow = parts[arms[side * 3 + 1]].bone.position, hand = parts[arms[side * 3 + 2]].bone.position;
                upperArm += .5f * Vector3.Distance(shoulder, elbow); forearm += .5f * Vector3.Distance(elbow, hand);
            }
            float share = .5f * upperKilograms * Physics.gravity.magnitude;
            shoulderNeeds = ArmHolds * share * upperArm;
            elbowNeeds = ArmHolds * share * forearm;
        }
        // What an arm's thickness alone gives, and what its body needs (newton metres): for whoever tells of it.
        public float ShoulderOfThickness => ShoulderReference * Build * strength;
        public float ElbowOfThickness => ElbowReference * Build * strength;
        public float ShoulderNeeds { get { ArmNeeds(); return Mathf.Max(0, shoulderNeeds) * strength; } }
        public float ElbowNeeds { get { ArmNeeds(); return Mathf.Max(0, elbowNeeds) * strength; } }
        public float ShoulderCapacity => Rule == Rules.Muscle ? shoulderKept * armsMuscle * ShoulderMuscle * Build * strength : ArmsAsBodyNeeds ? Mathf.Max(ShoulderOfThickness, ShoulderNeeds) : ShoulderOfThickness;
        public float ElbowCapacity => Rule == Rules.Muscle ? elbowKept * armsMuscle * ElbowMuscle * Build * strength : ArmsAsBodyNeeds ? Mathf.Max(ElbowOfThickness, ElbowNeeds) : ElbowOfThickness;
        // (The wrist and the hand's hold are the arm's muscle too: its share counts for them under strength from muscle.)
        public float WristCapacity => WristReference * Build * strength * MuscleGives(Regions.Arms);
        public float HoldCapacity => HoldReference * Mathf.Pow(armRadius / ArmReference, 2) * strength * MuscleGives(Regions.Arms);
        // The most a knee gives, in newton metres: fresh, and now.
        public float KneeCapacity
        {
            get
            {
                if (stance == null) TryGetComponent(out stance);
                if (Rule == Rules.Muscle) return kneeKept * legsMuscle * KneeMuscle * LegBuild * strength;
                float thigh = stance != null ? stance.BodyProportions.legSegment : .4f;
                return KneeHolds * mass * Physics.gravity.magnitude * .5f * thigh * strength;
            }
        }
        public float KneeNow => KneeCapacity * Fresh(Muscles.Legs);
        // What an arm's joints give now, as tired as that arm is.
        public float ShoulderOf(int side) => ShoulderCapacity * Fresh(Arm(side));
        public float ElbowOf(int side) => ElbowCapacity * Fresh(Arm(side));
        public float WristOf(int side) => WristCapacity * Fresh(Arm(side));
        // The most this hand's hold gives now, in newtons.
        public float HoldOf(int side) => HoldCapacity * Fresh(Arm(side));

        // One arm's own weight: each of its three parts' mass and where it is now.
        public void ArmParts(int side, Span<Vector3> at, Span<float> kilograms)
        {
            for (int k = 0; k < 3; k++)
            {
                var p = parts[arms[side * 3 + k]];
                at[k] = p.bone.TransformPoint(p.centre);
                kilograms[k] = p.mass;
            }
        }
    }
}
