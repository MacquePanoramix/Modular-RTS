using UnityEngine;

namespace WonderGather.Tests
{
    // The stance solver's body (layer 1 of the getting up for any body) read off a miner as the game makes it
    // (Docs/Technical/AnyBody/INTEGRATION.md 2.1 and 2.2): its saved proportions, its weighed parts, its limbs' thickness
    // and its strengths as a body made standing would have them, fresh. In the tests for now, so that nothing of the game
    // changes (the design's check, C1); OwnBody's own numbers it needs are copied here, and BodyFrameTests guards them.
    internal static class BodyFrame
    {
        // OwnBody's (private there): what the hip and the ankle give against the knee, and the neck's margin over its head.
        internal const float HipGives = 1.5f, AnkleGives = .7f, NeckHolds = 3;
        // PhysicalBody's knee under the rule before (Need), from the saved proportions (its own reads the live ones).
        internal const float KneeHolds = 1.6f;

        public static AnyBody.MinerSource SourceOf(GameObject unit)
        {
            var biped = unit.GetComponent<ProceduralBiped>();
            var physical = unit.GetComponent<PhysicalBody>();
            var p = biped.SavedProportions;
            float g = Physics.gravity.magnitude;
            double head = Weighs(physical, "Head") * OwnBody.HeadTimes;
            double knee = PhysicalBody.Rule == PhysicalBody.Rules.Muscle
                ? physical.KneeCapacity
                : KneeHolds * Mass(physical) * g * .5f * p.legSegment * physical.Strength;
            var s = new AnyBody.MinerSource
            {
                Name = unit.name,
                H = p.hipHeight, WH = p.hipWidth, Leg = p.legSegment, A = p.ankleHeight,
                Hl = p.heelLength, Bl = p.ballLength, Tl = p.toeLength,
                HW = p.waistRise, TR = p.torsoRise, HeadRise = p.headRise,
                Sx = p.shoulder.x, Sy = p.shoulder.y, Sz = p.shoulder.z,
                Lu = p.upperArm, Lf = p.forearm, BodyFront = p.bodyFront, HeadHalf = p.headHalf,
                Ra = physical.ArmRadius, Rl = physical.LegRadius,
                Height = unit.TryGetComponent(out CapsuleCollider capsule) ? capsule.height : 0,
                MHips = Weighs(physical, "Pelvis"), MTrunk = Weighs(physical, "Spine", "Chest", "Neck"), MHead = head,
                MUa = new[] { Weighs(physical, "UpperArm.L"), Weighs(physical, "UpperArm.R") },
                MFa = new[] { Weighs(physical, "Forearm.L", "Hand.L"), Weighs(physical, "Forearm.R", "Hand.R") },
                MTh = new[] { Weighs(physical, "Thigh.L"), Weighs(physical, "Thigh.R") },
                MSh = new[] { Weighs(physical, "Shin.L"), Weighs(physical, "Shin.R") },
                MFt = new[] { Weighs(physical, "Foot.L"), Weighs(physical, "Foot.R") },
                MTo = new[] { Weighs(physical, "Toe.L"), Weighs(physical, "Toe.R") },
                CBack = physical.BackCapacity,
                CNeck = NeckHolds * head * g * Mathf.Max(.03f, p.headRise - 2 * p.torsoRise) * physical.Strength * physical.MuscleGives(PhysicalBody.Regions.Neck),
                CShoulder = physical.ShoulderCapacity * OwnBody.ArmsTimes,
                CElbow = physical.ElbowCapacity * OwnBody.ArmsTimes,
                CKnee = knee, CHip = knee * HipGives, CAnkle = knee * AnkleGives, CToe = knee * AnkleGives * OwnBody.ToeGives,
            };
            return s;
        }

        // The body the solver fits stances to, as this unit is.
        public static AnyBody.BodyModel FromUnit(GameObject unit) =>
            AnyBody.BodyFactory.Make(SourceOf(unit), new AnyBody.BodyVariant { Name = unit.name, Source = unit.name });

        // As OwnBody weighs a part: the weighed bones whose names begin so, at least 0.1 kg.
        private static double Weighs(PhysicalBody physical, params string[] bones)
        {
            float kilograms = 0;
            for (int i = 0; i < physical.PartCount; i++)
            {
                var part = physical.PartAt(i);
                if (part.bone == null) continue;
                foreach (string bone in bones) if (part.bone.name.StartsWith(bone)) { kilograms += part.mass; break; }
            }
            return Mathf.Max(.1f, kilograms);
        }

        private static float Mass(PhysicalBody physical)
        {
            float kilograms = 0;
            for (int i = 0; i < physical.PartCount; i++) kilograms += physical.PartAt(i).mass;
            return kilograms;
        }
    }
}
