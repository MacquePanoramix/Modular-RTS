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
        public static Muscles Arm(int side) => side == 0 ? Muscles.LeftArm : Muscles.RightArm;
        // The share of a group that is spent (0: fresh), and the share that is not.
        public float Spent(Muscles muscles) => spent[(int)muscles];
        public float Fresh(Muscles muscles) => 1 - spent[(int)muscles];
        // Said once a physics step by whatever works a group: the share of what it has now that it gave.
        public void Worked(Muscles muscles, float effort, float dt)
        {
            int i = (int)muscles;
            worked[i] = true;
            Tire(i, Mathf.Clamp01(effort), dt);
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
                if (!worked[i]) Tire(i, 0, Time.fixedDeltaTime);
                worked[i] = false;
            }
        }

        // 1: ordinary for this body's build.
        public float Strength { get => strength; set => strength = Mathf.Clamp(value, .1f, 4); }

        // upperParts: the parts above the hips; hips: where the hips are now (the body standing as modelled).
        public void Configure(Part[] weighed, int[] armParts, int[] upperParts, Vector3 hips, float arm, float leg)
        {
            if (weighed == null || weighed.Length == 0 || armParts == null || armParts.Length != 6 || upperParts == null || upperParts.Length == 0 || !(arm > 0) || !(leg > 0))
                throw new ArgumentException("A body's weights need its parts, its arms' parts, its upper body's parts and its limbs' thickness.");
            foreach (var p in weighed) if (p.bone == null || !(p.mass > 0)) throw new ArgumentException("A part of the body has no bone or no weight.");
            foreach (int i in armParts) if (i < 0 || i >= weighed.Length) throw new ArgumentException("An arm's part is missing from the body's weights.");
            foreach (int i in upperParts) if (i < 0 || i >= weighed.Length) throw new ArgumentException("A part of the upper body is missing from the body's weights.");
            parts = weighed; arms = armParts; upper = upperParts; armRadius = arm; legRadius = leg;
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
        public float BackCapacity => BackHolds * upperAsks * Physics.gravity.magnitude * strength;
        public float BackNow => BackCapacity * Fresh(Muscles.Back);

        public Vector3 CentreOfMass()
        {
            Vector3 sum = Vector3.zero;
            foreach (var p in parts) sum += p.bone.TransformPoint(p.centre) * p.mass;
            return mass > 0 ? sum / mass : transform.position;
        }

        // How a limb's thickness scales what its joints give (torque), and what its hand holds (force).
        private float Build => Mathf.Pow(armRadius / ArmReference, 3);
        public float ShoulderCapacity => ShoulderReference * Build * strength;
        public float ElbowCapacity => ElbowReference * Build * strength;
        public float WristCapacity => WristReference * Build * strength;
        public float HoldCapacity => HoldReference * Mathf.Pow(armRadius / ArmReference, 2) * strength;
        // The most a knee gives, in newton metres: fresh, and now.
        public float KneeCapacity
        {
            get
            {
                if (stance == null) TryGetComponent(out stance);
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
