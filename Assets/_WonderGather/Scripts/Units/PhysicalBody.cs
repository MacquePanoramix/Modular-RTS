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

        [SerializeField] private Part[] parts = new Part[0];
        // The arms' parts (upper arm, forearm, hand; left then right), as indices into parts.
        [SerializeField] private int[] arms = new int[0];
        [SerializeField] private float mass, armRadius, legRadius;
        [SerializeField, Range(.1f, 4)] private float strength = 1;

        public float Mass => mass;
        public float ArmRadius => armRadius;
        public float LegRadius => legRadius;
        public int PartCount => parts.Length;
        public Part PartAt(int index) => parts[index];
        public bool Ready => parts.Length > 0 && arms.Length == 6 && mass > 0 && armRadius > 0;
        // 1: ordinary for this body's build.
        public float Strength { get => strength; set => strength = Mathf.Clamp(value, .1f, 4); }

        public void Configure(Part[] weighed, int[] armParts, float arm, float leg)
        {
            if (weighed == null || weighed.Length == 0 || armParts == null || armParts.Length != 6 || !(arm > 0) || !(leg > 0))
                throw new ArgumentException("A body's weights need its parts, its arms' parts and its limbs' thickness.");
            foreach (var p in weighed) if (p.bone == null || !(p.mass > 0)) throw new ArgumentException("A part of the body has no bone or no weight.");
            foreach (int i in armParts) if (i < 0 || i >= weighed.Length) throw new ArgumentException("An arm's part is missing from the body's weights.");
            parts = weighed; arms = armParts; armRadius = arm; legRadius = leg;
            mass = 0;
            foreach (var p in parts) mass += p.mass;
        }

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
