using System.Reflection;
using UnityEngine;

namespace WonderGather.Tests
{
    // The drawn hip and knee, read back as the solver's angles (November 14, 2026; the courses' bar 3 and 4,
    // Docs/Research/2026-11-13_DetachedLeg/courses_plan.md). The solver composes a thigh's turn on the hips' own frame
    // (AnyBody Kinematics.Forward: RTh = RH.PostRotY(-flex).PostRotX(sg abd).PostRotZ(sg twist), sg 1 for the left leg
    // and -1 for the right; the shin RS = RTh.PostRotY(knee)), in its axes x forward, y left, z up, a thigh at no angle
    // hanging straight down the hips' -z. Read here is the exact inverse, from the drawn bones (MinerBody.Rig):
    //   - the hips' frame from the drawn pelvis: MinerBody draws it as the solved pelvis' turn times its rest
    //     (bones.pelvis.rotation = solved.pelvis.rotation * pelvisRest), so its frame is pelvis.rotation * pelvisRest^-1,
    //     whose forward is the solver's x and up its z (OwnBody's frame of a link);
    //   - a thigh's (and a shin's) frame from its drawn bone: MinerBody turns it by the rotation taking its rest aim and
    //     the unit's rest forward to the knee's direction and the bend's way (bones.thighs[i].rotation = T * root *
    //     thighRest[i]), so T = thigh.rotation * (root * thighRest[i])^-1; the drawn thigh's line is T * root *
    //     thighAim[i] (the solver's -z) and the bend's way T * root * forward, square to it (the solver's x).
    // Then M = RH^T RTh = Ry(-flex) Rx(sg abd) Rz(sg twist), whose entries give flex = -atan2(M02, M22),
    // abd = sg atan2(-M12, |(M10, M11)|), twist = sg atan2(M10, M11); and the knee from N = RTh^T RS = Ry(knee):
    // atan2(N02, N22).
    // Signs as the solver's: flexion forward positive; abduction out to the leg's own side positive; the knee bent
    // positive (its shin back); the twist as the solver's sg-turned z.
    internal static class DrawnHip
    {
        private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
        private static FieldInfo pelvisRest, thighRest, thighAim, shinRest, shinAim;

        public struct Read
        {
            public float Flex, Abd, Twist, Knee;
            // At the gimbal lock (spread 90): the twist taken as 0, the bend then the two together.
            public bool Locked;
        }

        public static Read Of(MinerBody miner, int leg)
        {
            if (pelvisRest == null)
            {
                pelvisRest = typeof(MinerBody).GetField("pelvisRest", Hidden);
                thighRest = typeof(MinerBody).GetField("thighRest", Hidden);
                thighAim = typeof(MinerBody).GetField("thighAim", Hidden);
                shinRest = typeof(MinerBody).GetField("shinRest", Hidden);
                shinAim = typeof(MinerBody).GetField("shinAim", Hidden);
            }
            var bones = miner.Rig;
            Quaternion root = miner.transform.rotation;
            Quaternion hips = bones.pelvis.rotation * Quaternion.Inverse((Quaternion)pelvisRest.GetValue(miner));
            Frame(hips * Vector3.forward, hips * Vector3.up, out var px, out var py, out var pz);
            Limb(bones.thighs[leg].rotation, root, ((Quaternion[])thighRest.GetValue(miner))[leg], ((Vector3[])thighAim.GetValue(miner))[leg], out var tx, out var ty, out var tz);
            Limb(bones.shins[leg].rotation, root, ((Quaternion[])shinRest.GetValue(miner))[leg], ((Vector3[])shinAim.GetValue(miner))[leg], out var sx, out var sy, out var sz);
            float sg = leg == 0 ? 1 : -1;
            // M_ij = (hips' axis i) . (thigh's axis j).
            float m00 = Vector3.Dot(px, tx), m01 = Vector3.Dot(px, ty), m02 = Vector3.Dot(px, tz), m22 = Vector3.Dot(pz, tz);
            float m12 = Vector3.Dot(py, tz), m10 = Vector3.Dot(py, tx), m11 = Vector3.Dot(py, ty);
            float n02 = Vector3.Dot(tx, sz), n22 = Vector3.Dot(tz, sz);
            // (The spread by atan2, not asin: asin is blunt near 90 degrees, where a float's last bit is a fiftieth of a
            // degree. At a spread of 90 the bend and the twist turn about one axis (the composition's gimbal lock): only
            // bend - sg twist is told there, read with the twist taken as 0.)
            float across = Mathf.Sqrt(m10 * m10 + m11 * m11);
            float beta = Mathf.Atan2(-m12, across);
            float alpha, gamma;
            if (across < 1e-3f) { gamma = 0; alpha = Mathf.Atan2(Mathf.Sign(-m12) * m01, m00); }
            else { gamma = Mathf.Atan2(m10, m11); alpha = Mathf.Atan2(m02, m22); }
            return new Read
            {
                Flex = -alpha * Mathf.Rad2Deg,
                Abd = sg * beta * Mathf.Rad2Deg,
                Twist = sg * gamma * Mathf.Rad2Deg,
                Knee = Mathf.Atan2(n02, n22) * Mathf.Rad2Deg,
                Locked = across < 1e-3f,
            };
        }

        // A drawn limb bone's frame in the solver's axes: its line the solver's -z, the bend's way its x.
        private static void Limb(Quaternion drawn, Quaternion root, Quaternion rest, Vector3 aim, out Vector3 x, out Vector3 y, out Vector3 z)
        {
            Quaternion turn = drawn * Quaternion.Inverse(root * rest);
            Vector3 line = turn * (root * aim);
            Vector3 bend = turn * (root * Vector3.forward);
            Frame(Vector3.ProjectOnPlane(bend, line), -line, out x, out y, out z);
        }

        // The solver's x (forward), y (left) and z (up) as world directions, from a forward and an up: the solver's frame
        // maps onto Unity's as x to forward, y to left (-right) and z to up (AtlasPoser.W).
        private static void Frame(Vector3 forward, Vector3 up, out Vector3 x, out Vector3 y, out Vector3 z)
        {
            z = up.normalized;
            x = Vector3.ProjectOnPlane(forward, z).normalized;
            y = -Vector3.Cross(z, x);
        }
    }
}
