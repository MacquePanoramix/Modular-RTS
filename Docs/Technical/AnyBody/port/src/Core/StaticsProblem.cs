// The interface between the body/fitter (src/Body) and the reserve LP (src/Reserve): see port/INTERFACE.md.
// Everything the LP needs about a fitted pose and its supports, in world coordinates, as b3.py builds it
// (all_contacts, equilibrium, torque_maps). Plain data plus small helpers that reproduce the prototype's matrices.
using System;

namespace WonderGather.AnyBody
{
    /// <summary>The 15 links, in b3.py's LINKS order. A link set is a bit mask (1 &lt;&lt; index).</summary>
    public static class BodyLinks
    {
        public const int Hips = 0, Trunk = 1, Head = 2, UaL = 3, FaL = 4, UaR = 5, FaR = 6,
            ThL = 7, ShL = 8, FtL = 9, ToL = 10, ThR = 11, ShR = 12, FtR = 13, ToR = 14;

        public const int Count = 15;

        public static readonly string[] Names =
            { "hips", "trunk", "head", "uaL", "faL", "uaR", "faR", "thL", "shL", "ftL", "toL", "thR", "shR", "ftR", "toR" };

        public static int Index(string name) => Array.IndexOf(Names, name);

        /// <summary>The forearm links (a ground contact on one of them is a hand/forearm load: the hands' share).</summary>
        public static bool IsForearm(int link) => link == FaL || link == FaR;
    }

    /// <summary>The support kinds of b3.py's contact_points.</summary>
    public enum SupportKind
    {
        Hand, Forearm, Shin, Knee, Calf, Sole, Heel, Ball, Toes, Seat, ThighFront, Chest, HandOnThigh
    }

    /// <summary>One contact point, as one tuple (point, link, normal, mu, other) of b3.py's all_contacts.</summary>
    public struct ContactPoint
    {
        /// <summary>World position (m).</summary>
        public Vec3 Point;

        /// <summary>Unit normal of the force pushing on Link (+Z on the ground; from the thigh's axis to the wrist on a thigh).</summary>
        public Vec3 Normal;

        /// <summary>Friction coefficient (0.6 on the ground, 0.5 hand on thigh).</summary>
        public double Mu;

        /// <summary>The link the force acts on (BodyLinks index).</summary>
        public int Link;

        /// <summary>-1 for the ground; otherwise the link that takes the reaction (a self-contact: hand on thigh).</summary>
        public int Other;

        /// <summary>The kind of the support this point belongs to.</summary>
        public SupportKind Kind;

        /// <summary>Index of the support (in the stance's support list) this point belongs to.</summary>
        public int Support;

        /// <summary>A ground contact enters the whole-body equilibrium; a self-contact does not.</summary>
        public bool OnGround => Other < 0;
    }

    /// <summary>One joint the LP checks, as one tuple (name, origin, axis, subtree, cap) of b3.py's K['joints'].</summary>
    public sealed class JointLoad
    {
        public string Name;

        /// <summary>The joint centre (world).</summary>
        public Vec3 Origin;

        /// <summary>A hinge is checked about Axis only (|axis . tau|); a ball joint in the 13 BallDirections.</summary>
        public bool IsHinge;

        /// <summary>The hinge axis (unit, world); unused (zero) for a ball joint.</summary>
        public Vec3 Axis;

        /// <summary>The capacity (N m) at the body's present strength.</summary>
        public double Capacity;

        /// <summary>The links the joint holds (its subtree), as a BodyLinks bit mask.</summary>
        public int SubtreeMask;

        /// <summary>Total mass of the subtree (kg).</summary>
        public double SubMass;

        /// <summary>First mass moment of the subtree, sum of m_k c_k (kg m, world).</summary>
        public Vec3 SubMoment;

        /// <summary>Per contact: +1 when its Link is in the subtree, -1 when its Other is, 0 otherwise (or both).</summary>
        public sbyte[] ContactSign;

        /// <summary>tau0 = G [S'.y, -S'.x, 0], S' = SubMoment - SubMass (Origin - shift): the gravity torque the joint
        /// must hold when the whole body's mass is displaced horizontally by shift (b3.py torque_maps(shift)).</summary>
        public Vec3 GravityTorque(Vec3 shift)
        {
            var s = SubMoment - SubMass * Origin + SubMass * shift;
            return new Vec3(StaticsProblem.Gravity * s.Y, -StaticsProblem.Gravity * s.X, 0.0);
        }

        public Vec3 GravityTorque() => GravityTorque(Vec3.Zero);
    }

    /// <summary>A fitted pose with its supports, ready for the reserve LP (and the fitter's effort split).
    /// The joint torque for contact forces f_i (world, N, each acting on Contacts[i].Link) is
    /// tau_j = tau0_j - sum_i ContactSign[j][i] (p_i - o_j) x f_i; equilibrium: sum over ground contacts f_i = (0, 0, M g),
    /// sum over ground contacts p_i x f_i = g (S.y, -S.x, 0) with S = M (Com + shift).</summary>
    public sealed class StaticsProblem
    {
        public const double Gravity = 9.81;

        /// <summary>Total mass (kg).</summary>
        public double Mass;

        /// <summary>Whole-body centre of mass (world).</summary>
        public Vec3 Com;

        /// <summary>Contact points, in b3.py's all_contacts order.</summary>
        public ContactPoint[] Contacts;

        /// <summary>Joints in b3.py's order: back, neck, then for L and R: shoulder, elbow, hip, knee, ankle, toe.</summary>
        public JointLoad[] Joints;

        public double Weight => Mass * Gravity;

        /// <summary>b3.py's DIRS: the 13 directions (normalised) a ball joint's torque is checked in.</summary>
        public static readonly Vec3[] BallDirections = MakeDirs();

        static Vec3[] MakeDirs()
        {
            double[,] d =
            {
                { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 }, { 1, 1, 0 }, { 1, -1, 0 }, { 1, 0, 1 }, { 1, 0, -1 }, { 0, 1, 1 },
                { 0, 1, -1 }, { 1, 1, 1 }, { 1, 1, -1 }, { 1, -1, 1 }, { 1, -1, -1 }
            };
            var o = new Vec3[13];
            for (int k = 0; k < 13; k++)
                o[k] = new Vec3(d[k, 0], d[k, 1], d[k, 2]).Unit();
            return o;
        }

        /// <summary>b3.py equilibrium(K, C, shift): A (6 x 3nc) f = b (6). Self-contacts get zero columns.</summary>
        public void Equilibrium(Vec3 shift, double[,] a, double[] b)
        {
            int nc = Contacts.Length;
            if (a.GetLength(0) < 6 || a.GetLength(1) < 3 * nc || b.Length < 6)
                throw new ArgumentException("A must be at least 6 x 3nc and b at least 6");
            for (int r = 0; r < 6; r++)
                for (int c = 0; c < 3 * nc; c++)
                    a[r, c] = 0.0;
            for (int i = 0; i < nc; i++)
            {
                if (!Contacts[i].OnGround) continue;
                var p = Contacts[i].Point;
                int c = 3 * i;
                a[0, c] = 1; a[1, c + 1] = 1; a[2, c + 2] = 1;
                // skew(p)
                a[3, c + 1] = -p.Z; a[3, c + 2] = p.Y;
                a[4, c] = p.Z; a[4, c + 2] = -p.X;
                a[5, c] = -p.Y; a[5, c + 1] = p.X;
            }
            var s = Mass * (Com + shift);
            b[0] = 0; b[1] = 0; b[2] = Mass * Gravity;
            b[3] = Gravity * s.Y; b[4] = -Gravity * s.X; b[5] = 0;
        }

        /// <summary>b3.py torque_maps for joint j: tau = t0 - B f, B (3 x 3nc).</summary>
        public Vec3 TorqueMap(int j, Vec3 shift, double[,] bMat)
        {
            var jt = Joints[j];
            int nc = Contacts.Length;
            if (bMat.GetLength(0) < 3 || bMat.GetLength(1) < 3 * nc)
                throw new ArgumentException("B must be at least 3 x 3nc");
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3 * nc; c++)
                    bMat[r, c] = 0.0;
            for (int i = 0; i < nc; i++)
            {
                int sg = jt.ContactSign[i];
                if (sg == 0) continue;
                var r = (Contacts[i].Point - jt.Origin) * sg;
                int c = 3 * i;
                bMat[0, c + 1] = -r.Z; bMat[0, c + 2] = r.Y;
                bMat[1, c] = r.Z; bMat[1, c + 2] = -r.X;
                bMat[2, c] = -r.Y; bMat[2, c + 1] = r.X;
            }
            return jt.GravityTorque(shift);
        }

        /// <summary>b3.py generators(n, mu): the four edges of the friction pyramid (unit vectors).</summary>
        public static void PyramidEdges(Vec3 n, double mu, Vec3[] edges4)
        {
            var t1 = Vec3.Cross(n, Math.Abs(n.X) < 0.9 ? Vec3.UnitX : Vec3.UnitY).Unit();
            var t2 = Vec3.Cross(n, t1);
            edges4[0] = (n + mu * t1).Unit();
            edges4[1] = (n - mu * t1).Unit();
            edges4[2] = (n + mu * t2).Unit();
            edges4[3] = (n - mu * t2).Unit();
        }

        /// <summary>A copy that keeps only the contacts for which keep(contact) is true (for "if this support let go").</summary>
        public StaticsProblem Subset(Func<ContactPoint, bool> keep)
        {
            int n = 0;
            foreach (var c in Contacts) if (keep(c)) n++;
            var idx = new int[n];
            n = 0;
            for (int i = 0; i < Contacts.Length; i++) if (keep(Contacts[i])) idx[n++] = i;
            var o = new StaticsProblem { Mass = Mass, Com = Com, Contacts = new ContactPoint[n], Joints = new JointLoad[Joints.Length] };
            for (int k = 0; k < n; k++) o.Contacts[k] = Contacts[idx[k]];
            for (int j = 0; j < Joints.Length; j++)
            {
                var a = Joints[j];
                var sg = new sbyte[n];
                for (int k = 0; k < n; k++) sg[k] = a.ContactSign[idx[k]];
                o.Joints[j] = new JointLoad
                {
                    Name = a.Name, Origin = a.Origin, IsHinge = a.IsHinge, Axis = a.Axis, Capacity = a.Capacity,
                    SubtreeMask = a.SubtreeMask, SubMass = a.SubMass, SubMoment = a.SubMoment, ContactSign = sg
                };
            }
            return o;
        }
    }
}
