using NUnit.Framework;
using UnityEngine;

namespace WonderGather.Tests
{
    // Layer 0's joint frames (JointFrames.cs; Docs/Design/AnyBody/joint_model.md, step 1): a joint's turn read as anatomy
    // and back, and kept inside its range. Each joint's directions are checked against a movement built geometrically, not
    // with the same sums: an arm raised forward points forward, a leg taken out to its side points out, and so on.
    public sealed class JointFramesTests
    {
        // The far part's length (its own up), and the side its joint sticks out (its own forward), seen from the near part.
        private static Vector3 Length(Quaternion relation) => relation * Vector3.up;
        private static Vector3 Facing(Quaternion relation) => relation * Vector3.forward;
        private static Quaternion Pose(JointFrame f, float bend, float aside = 0, float twist = 0) => f.FromAnatomy(new Vector3(bend, aside, twist));

        private static void Near(Vector3 got, Vector3 want, string what, float degrees = 1)
            => Assert.That(Vector3.Angle(got, want), Is.LessThan(degrees), $"{what}: {got} is not along {want}.");

        [Test]
        public void AnatomyReadsBackWhatItWasGiven()
        {
            var random = new System.Random(3);
            foreach (JointKind kind in System.Enum.GetValues(typeof(JointKind)))
                foreach (int side in new[] { -1, 1 })
                {
                    var f = JointFrame.Of(kind, side);
                    for (int k = 0; k < 200; k++)
                    {
                        float u() => (float)random.NextDouble();
                        var r = f.Range;
                        var want = new Vector3(Mathf.Lerp(r.FlexMin, r.FlexMax, u()), f.Hinge ? 0 : Mathf.Lerp(r.AsideMin, r.AsideMax, u()) * .6f, f.Hinge ? 0 : Mathf.Lerp(r.TwistMin, r.TwistMax, u()));
                        var got = f.Anatomy(f.FromAnatomy(want));
                        Assert.That((got - want).magnitude, Is.LessThan(.05f), $"{kind} side {side}: {want} read back as {got}.");
                    }
                }
        }

        [Test]
        public void TheShoulderRaisesForwardAndOut()
        {
            var right = JointFrame.Of(JointKind.Shoulder, 1); var left = JointFrame.Of(JointKind.Shoulder, -1);
            Near(Length(Pose(right, 0)), Vector3.down, "a hanging arm");
            Near(Length(Pose(right, 90)), Vector3.forward, "the right arm raised forward");
            Near(Length(Pose(left, 90)), Vector3.forward, "the left arm raised forward");
            Near(Length(Pose(right, -40)), (Vector3.down + Vector3.back * Mathf.Tan(40 * Mathf.Deg2Rad)).normalized, "the right arm behind");
            Near(Length(Pose(right, 0, 90)), Vector3.right, "the right arm out to its side");
            Near(Length(Pose(left, 0, 90)), Vector3.left, "the left arm out to its side");
            // Turned in, with the elbow bent: its forearm (away from the elbow's side) swings across the belly.
            Near(-Facing(Pose(right, 0, 0, 90)), Vector3.left, "the right forearm turned in");
            Near(-Facing(Pose(left, 0, 0, 90)), Vector3.right, "the left forearm turned in");
        }

        [Test]
        public void TheHipRaisesForwardAndOut()
        {
            var right = JointFrame.Of(JointKind.Hip, 1); var left = JointFrame.Of(JointKind.Hip, -1);
            Near(Length(Pose(right, 0)), Vector3.down, "a standing leg");
            Near(Length(Pose(right, 90)), Vector3.forward, "the right thigh raised forward");
            Near(Length(Pose(left, 90)), Vector3.forward, "the left thigh raised forward");
            Near(Length(Pose(right, 0, 40)), (Vector3.down + Vector3.right * Mathf.Tan(40 * Mathf.Deg2Rad)).normalized, "the right leg out to its side");
            Near(Length(Pose(left, 0, 40)), (Vector3.down + Vector3.left * Mathf.Tan(40 * Mathf.Deg2Rad)).normalized, "the left leg out to its side");
            // Turned in: the knee's side turns towards the other leg.
            Near(Facing(Pose(right, 0, 0, 90)), Vector3.left, "the right knee turned in");
            Near(Facing(Pose(left, 0, 0, 90)), Vector3.right, "the left knee turned in");
        }

        [Test]
        public void ElbowsAndKneesFoldAwayFromTheirPoint()
        {
            foreach (var kind in new[] { JointKind.Elbow, JointKind.Knee })
            {
                var f = JointFrame.Of(kind, 1);
                Near(Length(Pose(f, 0)), Vector3.up, $"{kind} straight");
                Near(Length(Pose(f, 90)), Vector3.back, $"{kind} folded square, away from the side its point sticks out");
            }
        }

        [Test]
        public void TheBackAndNeckBendForward()
        {
            foreach (var kind in new[] { JointKind.Back, JointKind.Neck })
            {
                var f = JointFrame.Of(kind, 0);
                Near(Length(Pose(f, 0)), Vector3.up, $"{kind} upright");
                Near(Length(Pose(f, 30)), (Vector3.up + Vector3.forward * Mathf.Tan(30 * Mathf.Deg2Rad)).normalized, $"{kind} bowed forward");
                Near(Length(Pose(f, 0, 20)), (Vector3.up + Vector3.right * Mathf.Tan(20 * Mathf.Deg2Rad)).normalized, $"{kind} bent to the right");
            }
        }

        [Test]
        public void TheAnkleLiftsItsToesAndTurnsItsSoleIn()
        {
            var right = JointFrame.Of(JointKind.Ankle, 1); var left = JointFrame.Of(JointKind.Ankle, -1);
            // Seen from the shin, whose own up runs down the leg: the boot at zero has its own up as the shin's down.
            Near(Pose(right, 0) * Vector3.up, Vector3.down, "a boot's sole normal, square to the shin");
            Near(Facing(Pose(right, 20)), (Vector3.forward + Vector3.down * Mathf.Tan(20 * Mathf.Deg2Rad)).normalized, "the toes lifted (towards the knee)");
            // The sole turned in: the boot's own right (the outer edge of the right foot) dips towards the ground.
            Near(Pose(right, 0, 20) * Vector3.right, (Vector3.left + Vector3.up * Mathf.Tan(20 * Mathf.Deg2Rad)).normalized, "the right sole turned in");
            Near(Pose(left, 0, 20) * Vector3.right, (Vector3.left + Vector3.down * Mathf.Tan(20 * Mathf.Deg2Rad)).normalized, "the left sole turned in");
            // Toes turned in: towards the other foot.
            Near(Facing(Pose(right, 0, 0, 20)), (Vector3.forward + Vector3.right * Mathf.Tan(20 * Mathf.Deg2Rad)).normalized, "the right toes turned in");
        }

        [Test]
        public void TheClampKeepsAJointInsideItsRangeAndItsWay()
        {
            var hip = JointFrame.Of(JointKind.Hip, 1);
            // Inside: unchanged.
            var inside = Pose(hip, 60, 10, 5);
            Assert.That(Quaternion.Angle(hip.Clamp(inside), inside), Is.LessThan(.1f), "A pose inside the range was moved.");
            // Too far back (extension past the range): brought to its edge, nothing else changed.
            var back = hip.Anatomy(hip.Clamp(Pose(hip, -60)));
            Assert.That(back.x, Is.EqualTo(hip.Range.FlexMin).Within(.5f), "A thigh too far back was not brought to its range's edge.");
            // Too far forward and out together: kept on the ellipse, in the same direction.
            var corner = hip.Anatomy(hip.Clamp(Pose(hip, 150, 70)));
            float reach = (corner.x / hip.Range.FlexMax) * (corner.x / hip.Range.FlexMax) + (corner.y / hip.Range.AsideMax) * (corner.y / hip.Range.AsideMax);
            Assert.That(reach, Is.EqualTo(1).Within(.02f), "A thigh too far forward and out was not brought to its range's edge.");
            Assert.That(Mathf.Atan2(corner.y, corner.x), Is.EqualTo(Mathf.Atan2(70, 150)).Within(.02f), "Clamped, its way was changed.");
            // A knee bent the wrong way, or folded too far.
            var knee = JointFrame.Of(JointKind.Knee, 1);
            Assert.That(knee.Anatomy(knee.Clamp(Pose(knee, -20))).x, Is.EqualTo(0).Within(.5f), "A knee bent backwards was not straightened.");
            Assert.That(knee.Anatomy(knee.Clamp(Pose(knee, 175))).x, Is.EqualTo(knee.Range.FlexMax).Within(.5f), "A knee folded too far was not opened to its stop.");
            Assert.That(knee.Inside(Pose(knee, 160), .5f), Is.False, "A knee folded past its stop counted as inside.");
        }
    }
}
