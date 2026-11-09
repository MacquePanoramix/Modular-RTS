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
        // PhysicalBody's strength from muscle: what a shoulder, an elbow and a knee give at the reference thicknesses.
        internal const float ShoulderMuscle = 121.3f, ElbowMuscle = 104.5f, KneeMuscle = 137.5f, ArmReference = .047f, LegReference = .046f;

        // Bodies between and beyond the three miners, for the stance sheets (INTEGRATION.md 2.6): data bodies (no maker of
        // bodies exists yet), each from a miner's numbers, or two mixed half and half, by the solver's rules for lengths,
        // masses and shapes; their strengths then by the game's rule now: the arms and legs by muscle from their own
        // thickness (a body scaled by k gets k cubed: a giant is weaker for its weight, Luis, November 3), the back by its
        // need as the game reckons it, the neck by OwnBody's formula. Made since the muscle rule, they keep nothing of the
        // rule before (unlike the three miners), so they show their build. miner: a miner's numbers by name.
        public static System.Collections.Generic.List<(string Name, AnyBody.BodyModel Body)> MadeBodies(System.Func<string, AnyBody.MinerSource> miner)
        {
            var made = new System.Collections.Generic.List<(string, AnyBody.BodyModel)>();
            void Add(string name, AnyBody.MinerSource source, AnyBody.BodyVariant v, double strength = 1)
            {
                v.Name = name;
                v.Source = source.Name;
                made.Add((name, WithGameStrength(AnyBody.BodyFactory.Make(source, v), source, strength)));
            }
            var small = miner("Small");
            var lean = miner("Long");
            var round = miner("Round");
            Add("Small-Long", Mixed(small, lean), new AnyBody.BodyVariant());
            Add("Long-Round", Mixed(lean, round), new AnyBody.BodyVariant());
            Add("Round-Small", Mixed(round, small), new AnyBody.BodyVariant());
            Add("Small x0.7", small, new AnyBody.BodyVariant { Scale = .7 });
            Add("Toddler-like", small, new AnyBody.BodyVariant { LegMul = .85, MassMul = new System.Collections.Generic.Dictionary<string, double> { ["head"] = 1.3 } });
            Add("Long x1.25", lean, new AnyBody.BodyVariant { Scale = 1.25 });
            Add("Long, legs x1.2", lean, new AnyBody.BodyVariant { LegMul = 1.2 });
            Add("Round, trunk x1.5", round, new AnyBody.BodyVariant { MassMul = new System.Collections.Generic.Dictionary<string, double> { ["trunk"] = 1.5 } });
            Add("Small at S0.5", small, new AnyBody.BodyVariant(), .5);
            Add("Round at S0.3", round, new AnyBody.BodyVariant(), .3);
            return made;
        }

        // Every number of two miners, half and half.
        private static AnyBody.MinerSource Mixed(AnyBody.MinerSource a, AnyBody.MinerSource b)
        {
            double M(double x, double y) => .5 * (x + y);
            double[] Ms(double[] x, double[] y) => new[] { M(x[0], y[0]), M(x[1], y[1]) };
            return new AnyBody.MinerSource
            {
                Name = a.Name + "-" + b.Name,
                H = M(a.H, b.H), WH = M(a.WH, b.WH), Leg = M(a.Leg, b.Leg), A = M(a.A, b.A), Hl = M(a.Hl, b.Hl), Bl = M(a.Bl, b.Bl), Tl = M(a.Tl, b.Tl),
                HW = M(a.HW, b.HW), TR = M(a.TR, b.TR), HeadRise = M(a.HeadRise, b.HeadRise), Sx = M(a.Sx, b.Sx), Sy = M(a.Sy, b.Sy), Sz = M(a.Sz, b.Sz),
                Lu = M(a.Lu, b.Lu), Lf = M(a.Lf, b.Lf), BodyFront = M(a.BodyFront, b.BodyFront), HeadHalf = M(a.HeadHalf, b.HeadHalf),
                Ra = M(a.Ra, b.Ra), Rl = M(a.Rl, b.Rl), Height = M(a.Height, b.Height),
                MHips = M(a.MHips, b.MHips), MTrunk = M(a.MTrunk, b.MTrunk), MHead = M(a.MHead, b.MHead),
                MUa = Ms(a.MUa, b.MUa), MFa = Ms(a.MFa, b.MFa), MTh = Ms(a.MTh, b.MTh), MSh = Ms(a.MSh, b.MSh), MFt = Ms(a.MFt, b.MFt), MTo = Ms(a.MTo, b.MTo),
                CBack = M(a.CBack, b.CBack), CNeck = M(a.CNeck, b.CNeck), CShoulder = M(a.CShoulder, b.CShoulder), CElbow = M(a.CElbow, b.CElbow),
                CHip = M(a.CHip, b.CHip), CKnee = M(a.CKnee, b.CKnee), CAnkle = M(a.CAnkle, b.CAnkle), CToe = M(a.CToe, b.CToe),
            };
        }

        // A body's strengths by the game's rule now, from its own numbers (see MadeBodies). The back: the source's own back
        // over what the solver reckons it asks, times what this body asks.
        private static AnyBody.BodyModel WithGameStrength(AnyBody.BodyModel b, AnyBody.MinerSource source, double strength)
        {
            double arms = System.Math.Pow(b.ArmRadius / ArmReference, 3), legs = System.Math.Pow(b.ShinRadius / LegReference, 3);
            double knee = KneeMuscle * legs * strength;
            var asks = AnyBody.BodyFactory.PresentCaps(b);
            var asksOfSource = AnyBody.BodyFactory.PresentCaps(AnyBody.BodyFactory.Make(source, new AnyBody.BodyVariant { Name = source.Name, Source = source.Name }));
            b.CapBack = source.CBack / asksOfSource["back"] * asks["back"] * strength;
            b.CapNeck = asks["neck"] * strength;
            for (int s = 0; s < 2; s++)
            {
                b.CapShoulder[s] = ShoulderMuscle * arms * strength;
                b.CapElbow[s] = ElbowMuscle * arms * strength;
                b.CapKnee[s] = knee;
                b.CapHip[s] = HipGives * knee;
                b.CapAnkle[s] = AnkleGives * knee;
                b.CapToe[s] = AnkleGives * OwnBody.ToeGives * knee;
            }
            b.Strength = strength;
            return b;
        }

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
