using UnityEngine;

namespace WonderGather
{
    // The body for any body, layer 0 (Docs/Design/AnyBody/joint_model.md; Luis decided the direction on November 3, 2026):
    // each joint's own frame and its range, the same for any build. A joint's zero is the body's anatomical position: limbs
    // straight down, soles flat, trunk and head upright. Its turn from zero is read as three anatomical turns: the bend
    // (flexion +, extension -), aside (abduction or bending to the side +), and about the far part's length (inward +), as a
    // living joint has them, and its range is kept in those terms: a target is clamped inside the soft range, the engine's
    // own limits stand a little beyond it as a backstop only (at the stops, nothing rests: October 30, its limbs chattered).
    //
    // The parts are made as OwnBody makes them: each with its length up (towards the far end), and a limb's forward the side
    // its elbow or knee sticks out. In those frames, at zero, each joint's three axes are fixed by anatomy and side; they are
    // written here once, for any body.
    public enum JointKind { Back, Neck, Shoulder, Elbow, Hip, Knee, Ankle }

    public struct JointRange
    {
        // Degrees. The soft range (targets are kept inside it) and the hard one (the engine's backstop beyond it).
        public float FlexMin, FlexMax, AsideMin, AsideMax, TwistMin, TwistMax;
        public float HardMargin;
        // How fast it turns at most with nothing to push, radians a second, for a body whose hips stand 0.6 m high (Small);
        // a bigger body's joints are slower, as their size asks (Luis, November 3: "slower with size"; OwnBody.SpeedOf).
        // Its strength falls as it turns, to nothing at this speed (the drive's damping at least strength over speed).
        public float Speed;

        public static JointRange Of(JointKind kind)
        {
            switch (kind)
            {
                // (October 30 to November 3: the clinical tables, the let-go body's own, and one adult's six get-ups from
                // motion capture, whose deep squat bends the knee to 155-158 degrees.)
                case JointKind.Back: return new JointRange { FlexMin = -20, FlexMax = 75, AsideMin = -25, AsideMax = 25, TwistMin = -30, TwistMax = 30, HardMargin = 8, Speed = 20 };
                case JointKind.Neck: return new JointRange { FlexMin = -40, FlexMax = 45, AsideMin = -30, AsideMax = 30, TwistMin = -60, TwistMax = 60, HardMargin = 10, Speed = 20 };
                case JointKind.Shoulder: return new JointRange { FlexMin = -50, FlexMax = 150, AsideMin = -10, AsideMax = 150, TwistMin = -80, TwistMax = 70, HardMargin = 10, Speed = 30 };
                case JointKind.Elbow: return new JointRange { FlexMin = 0, FlexMax = 140, HardMargin = 4, Speed = 30 };
                case JointKind.Hip: return new JointRange { FlexMin = -15, FlexMax = 125, AsideMin = -20, AsideMax = 45, TwistMin = -40, TwistMax = 35, HardMargin = 8, Speed = 25 };
                case JointKind.Knee: return new JointRange { FlexMin = 0, FlexMax = 155, HardMargin = 5, Speed = 30 };
                default: return new JointRange { FlexMin = -50, FlexMax = 30, AsideMin = -15, AsideMax = 25, TwistMin = -15, TwistMax = 15, HardMargin = 8, Speed = 20 };
            }
        }
    }

    public sealed class JointFrame
    {
        public JointKind Kind;
        public int Side;            // +1 right, -1 left, 0 the middle
        public bool Hinge;
        // The far part's relation to the near one at zero; and, in the far part's frame at zero, the axis a positive bend,
        // a positive aside and a positive inward twist turn about.
        public Quaternion Zero;
        public Vector3 BendAxis, AsideAxis, TwistAxis;
        public JointRange Range;

        // Made for a joint of a body made as OwnBody makes it (its own frames, as above).
        public static JointFrame Of(JointKind kind, int side)
        {
            var f = new JointFrame { Kind = kind, Side = side, Range = JointRange.Of(kind), TwistAxis = Vector3.up };
            switch (kind)
            {
                case JointKind.Back:
                case JointKind.Neck:
                    // Upright on what it stands on. Bending forward tips its length towards its front.
                    f.Zero = Quaternion.identity; f.BendAxis = Vector3.right; f.AsideAxis = Vector3.back; break;
                case JointKind.Shoulder:
                    // Hanging: its length down, the side its elbow sticks out back. Raised forward, its length turns
                    // towards its front, away from the elbow's side.
                    f.Zero = Quaternion.AngleAxis(180, Vector3.right); f.BendAxis = Vector3.left;
                    f.AsideAxis = side > 0 ? Vector3.back : Vector3.forward; f.TwistAxis = side > 0 ? Vector3.up : Vector3.down; break;
                case JointKind.Hip:
                    // Standing: its length down, the knee's side forward (its own right is then the body's left).
                    f.Zero = Quaternion.AngleAxis(180, Vector3.forward); f.BendAxis = Vector3.right;
                    f.AsideAxis = side > 0 ? Vector3.forward : Vector3.back; f.TwistAxis = side > 0 ? Vector3.up : Vector3.down; break;
                case JointKind.Elbow:
                case JointKind.Knee:
                    // Straight. It folds away from the side its joint sticks out.
                    f.Hinge = true; f.Zero = Quaternion.identity; f.BendAxis = Vector3.left; f.AsideAxis = Vector3.forward; break;
                default:
                    // The boot square to the shin, its sole down, its toes forward (its own right the body's right). Toes up
                    // is a positive bend; the sole turned in is a positive aside; the toes turned in a positive twist.
                    f.Zero = Quaternion.AngleAxis(180, Vector3.forward); f.BendAxis = Vector3.left;
                    f.AsideAxis = side > 0 ? Vector3.back : Vector3.forward; f.TwistAxis = side > 0 ? Vector3.down : Vector3.up; break;
            }
            return f;
        }

        // The turn from zero, in the far part's own frame at zero, of a relation (far part in near part).
        public Quaternion FromZero(Quaternion relation) => Quaternion.Inverse(Zero) * relation;
        public Quaternion Relation(Quaternion fromZero) => Zero * fromZero;

        // Read as anatomy, degrees: x the bend, y aside, z inward twist. The turn is split into a twist about the far
        // part's length and a swing of that length; the swing is read as the rotation vector of its own turn.
        public Vector3 Anatomy(Quaternion relation)
        {
            Swing(FromZero(relation), out Quaternion swing, out float twist);
            Vector3 s = RotationVector(swing);
            return new Vector3(Vector3.Dot(s, BendAxis) * Mathf.Rad2Deg, Vector3.Dot(s, AsideAxis) * Mathf.Rad2Deg, twist);
        }

        public Quaternion FromAnatomy(Vector3 degrees)
        {
            Vector3 s = (BendAxis * degrees.x + AsideAxis * degrees.y) * Mathf.Deg2Rad;
            Quaternion swing = s.sqrMagnitude > 1e-12f ? Quaternion.AngleAxis(s.magnitude * Mathf.Rad2Deg, s.normalized) : Quaternion.identity;
            Quaternion twist = Quaternion.AngleAxis(degrees.z, TwistAxis);
            return Relation(swing * twist);
        }

        // A relation kept inside the range (soft, or hard plus nothing): the bend and aside held inside an ellipse with a
        // half-axis for each of the four ways (so a joint that bends far forward and little back keeps its shape), the twist
        // inside its own range; the direction of the swing kept.
        public Quaternion Clamp(Quaternion relation, bool hard = false)
        {
            Vector3 a = Anatomy(relation);
            float m = hard ? Range.HardMargin : 0;
            float twist = Hinge ? 0 : Mathf.Clamp(a.z, Range.TwistMin - m, Range.TwistMax + m);
            float bend = a.x, aside = Hinge ? 0 : a.y;
            if (Hinge) bend = Mathf.Clamp(bend, Range.FlexMin - m, Range.FlexMax + m);
            else
            {
                float bx = bend >= 0 ? Range.FlexMax + m : -(Range.FlexMin - m), by = aside >= 0 ? Range.AsideMax + m : -(Range.AsideMin - m);
                bx = Mathf.Max(bx, 1e-3f); by = Mathf.Max(by, 1e-3f);
                float reach = (bend / bx) * (bend / bx) + (aside / by) * (aside / by);
                if (reach > 1) { float k = 1 / Mathf.Sqrt(reach); bend *= k; aside *= k; }
            }
            return FromAnatomy(new Vector3(bend, aside, twist));
        }

        public bool Inside(Quaternion relation, float slack = .5f)
        {
            Vector3 a = Anatomy(relation);
            Vector3 c = Anatomy(Clamp(relation));
            return Mathf.Abs(a.x - c.x) <= slack && Mathf.Abs(a.y - c.y) <= slack && Mathf.Abs(Mathf.DeltaAngle(a.z, c.z)) <= slack;
        }

        // A turn split into a twist about the far part's length (degrees) and the swing that is left: turn = swing * twist.
        private void Swing(Quaternion turn, out Quaternion swing, out float twistDegrees)
        {
            Vector3 axis = TwistAxis;
            Vector3 v = new Vector3(turn.x, turn.y, turn.z);
            Vector3 p = Vector3.Dot(v, axis) * axis;
            var twist = new Quaternion(p.x, p.y, p.z, turn.w);
            float n = Mathf.Sqrt(twist.x * twist.x + twist.y * twist.y + twist.z * twist.z + twist.w * twist.w);
            if (n < 1e-9f) { twist = Quaternion.identity; }
            else twist = new Quaternion(twist.x / n, twist.y / n, twist.z / n, twist.w / n);
            swing = turn * Quaternion.Inverse(twist);
            twist.ToAngleAxis(out float angle, out Vector3 about);
            if (angle > 180) angle -= 360;
            twistDegrees = Vector3.Dot(about, axis) >= 0 ? angle : -angle;
        }

        // The engine's coordinates of a relation (its far part's frame, the joint's zero at Zero): the rotation vector of
        // the turn from zero, as the engine reads a ball joint (and a hinge, about its X), radians.
        public Vector3 Engine(Quaternion relation) => RotationVector(FromZero(relation));

        // The engine's own limits for this joint, degrees on each axis of the far part's frame: the range and its margin
        // along each axis (a box about the anatomical ranges, a backstop only), widened to take in a turn (radians) that a
        // fall may have left beyond it.
        public void Box(Vector3 takeIn, out Vector3 low, out Vector3 high)
        {
            var r = Range; float m = r.HardMargin;
            (float, float) Along(Vector3 axis, Vector3 unit, float min, float max)
            {
                float s = Vector3.Dot(axis, unit);
                return s >= 0 ? (min - m, max + m) : (-(max + m), -(min - m));
            }
            var (xl, xh) = Along(BendAxis, Vector3.right, r.FlexMin, r.FlexMax);
            var (yl, yh) = Hinge ? (0f, 0f) : Along(TwistAxis, Vector3.up, r.TwistMin, r.TwistMax);
            var (zl, zh) = Hinge ? (0f, 0f) : Along(AsideAxis, Vector3.forward, r.AsideMin, r.AsideMax);
            Vector3 at = takeIn * Mathf.Rad2Deg;
            low = new Vector3(Mathf.Min(xl, at.x - 2), Mathf.Min(yl, at.y - 2), Mathf.Min(zl, at.z - 2));
            high = new Vector3(Mathf.Max(xh, at.x + 2), Mathf.Max(yh, at.y + 2), Mathf.Max(zh, at.z + 2));
        }

        public static Vector3 RotationVector(Quaternion q)
        {
            if (q.w < 0) q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
            q.ToAngleAxis(out float angle, out Vector3 axis);
            if (float.IsNaN(axis.x) || angle < 1e-6f) return Vector3.zero;
            if (angle > 180) angle -= 360;
            return axis.normalized * (angle * Mathf.Deg2Rad);
        }
    }

    // A body's joints as JointFrame reads them, and how each was made: what a controller asks of a joint (a turn from how
    // it was made, in its far part's frame, as every Aims has always said it) becomes the engine's target (degrees on each
    // axis), kept inside the joint's soft range. One for the whole body, so that whatever asks a joint asks it alike.
    public sealed class JointAim
    {
        public readonly JointFrame[] Frames;
        public readonly Quaternion[] Made;
        public JointAim(JointFrame[] frames, Quaternion[] made) { Frames = frames; Made = made; }

        public Vector3 Target(int j, Quaternion own)
        {
            var f = Frames[j];
            if (f == null) return Vector3.zero;
            return f.Engine(f.Clamp(Made[j] * own)) * Mathf.Rad2Deg;
        }

        // The frames of a body made as OwnBody makes it (its parts in OwnBody's order: hips, trunk, head, upper arms,
        // forearms, thighs, shins, boots; each pair left then right).
        public static JointFrame[] OfOwnBody()
        {
            var f = new JointFrame[13];
            f[1] = JointFrame.Of(JointKind.Back, 0);
            f[2] = JointFrame.Of(JointKind.Neck, 0);
            for (int i = 0; i < 2; i++)
            {
                int side = i == 0 ? -1 : 1;
                f[3 + i] = JointFrame.Of(JointKind.Shoulder, side);
                f[5 + i] = JointFrame.Of(JointKind.Elbow, side);
                f[7 + i] = JointFrame.Of(JointKind.Hip, side);
                f[9 + i] = JointFrame.Of(JointKind.Knee, side);
                f[11 + i] = JointFrame.Of(JointKind.Ankle, side);
            }
            return f;
        }
    }
}
