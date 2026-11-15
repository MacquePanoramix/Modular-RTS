using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace WonderGather.Tests
{
    // The energy count of one articulated body, step by step (the blow-up's trace, November 15, 2026; the main project's
    // Docs/Validation.md, "The cut ring as played": one of Small's own get-ups blew up, an arm whirling to 1e8 rad/s, and
    // no energy was counted). A measure of the test, read only: it never writes to the body.
    //
    // What the body holds: each link's going and turning, and its weight lifted (m g h over Level, for a link the engine
    // pulls down). How fast a link's centre goes is worked out along the body from its root's going and each link's
    // turning (as ArticulationFactsTests.TrulyGoes): the engine's own telling of a fast-turning link's going is too great
    // (ADrivesForceIsWhatItTakes, November 10).
    //
    // What was given over a step: what the game's scripts put on its links before the step (the engine's own accumulated
    // force and torque on each link, read last thing before the step: the keeper's and the riser's torques, both, and any
    // push), each times the mean of how the link went or turned at the step's start and at its end; and what each joint's
    // drive gave, its force times the mean of how the joint turned (as OwnBodyTests' -ownShoveTrace counts it). The joints'
    // torques are found from the links' accumulated torques, leaves first (a joint's torque on its link, less what its
    // children's joints put back on it). The gain is what the body holds after the step, less what it held before it and
    // what was given: what the ground, the stops and the engine's own step gave or took.
    //
    // Call Before() at the end of the physics' FixedUpdate (after every script has pushed), After() after the step
    // (WaitForFixedUpdate, or straight after Physics.Simulate).
    internal sealed class LiveEnergy
    {
        public const int RingSteps = 32;
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public ArticulationBody[] Links { get; private set; } = new ArticulationBody[0];
        public string[] Names { get; private set; } = new string[0];
        // The height its weight lifted is reckoned from (metres, world).
        public float Level;

        // The last step counted.
        public float Held = float.NaN, Given = float.NaN, Gain = float.NaN, Fastest = float.NaN;
        public int FastestLink = -1;
        public string FastestName => FastestLink >= 0 && FastestLink < Names.Length ? Names[FastestLink] : "";

        private int[] up = new int[0];
        private Vector3[] wBefore, vBefore, force, torque, jointTorque, vAfter, wAfter;
        private float[][] jointBefore;
        private float heldBefore;
        private bool before;
        public float[] Speed, Turning, LinkGiven;

        // The ring of the last steps: per step its frame, time, label, held, given, gain; per link its true speed, its
        // turning and what was given to it (its joint's work, and any push on it).
        private struct Step { public int Frame; public float Time, Held, Given, Gain; public string Label; public float[] Speed, Turning, ToLinks; }
        private readonly Step[] ring = new Step[RingSteps];
        private int ringNext, ringCount;

        // The body under this root: its links in the hierarchy's order (each after what it hangs from), named by the given
        // known links' names where they are among them. False without a root. Taken again when the root changes or a link
        // is gone (the step's start then forgotten).
        public bool Take(ArticulationBody root, ArticulationBody[] known = null, string[] knownNames = null)
        {
            if (root == null) { Forget(); Links = new ArticulationBody[0]; return false; }
            if (Links.Length > 0 && Links[0] == root)
            {
                bool whole = true;
                for (int i = 0; i < Links.Length; i++) if (Links[i] == null) { whole = false; break; }
                if (whole) return true;
            }
            var all = root.GetComponentsInChildren<ArticulationBody>();
            if (all.Length == 0 || all[0] != root) { Forget(); Links = new ArticulationBody[0]; return false; }
            int n = all.Length;
            Links = all;
            Names = new string[n];
            up = new int[n];
            for (int i = 0; i < n; i++)
            {
                int k = known != null ? System.Array.IndexOf(known, all[i]) : -1;
                Names[i] = k >= 0 && knownNames != null && k < knownNames.Length ? knownNames[k] : all[i].gameObject.name.Replace(" ", "").Replace("(", "").Replace(")", "").Replace(",", ";");
                up[i] = -1;
                if (i > 0)
                {
                    var from = all[i].transform.parent != null ? all[i].transform.parent.GetComponentInParent<ArticulationBody>() : null;
                    up[i] = System.Array.IndexOf(all, from);
                }
            }
            wBefore = new Vector3[n]; vBefore = new Vector3[n]; force = new Vector3[n]; torque = new Vector3[n]; jointTorque = new Vector3[n];
            vAfter = new Vector3[n]; wAfter = new Vector3[n];
            jointBefore = new float[n][];
            Speed = new float[n]; Turning = new float[n]; LinkGiven = new float[n];
            for (int s = 0; s < RingSteps; s++) ring[s] = new Step { Speed = new float[n], Turning = new float[n], ToLinks = new float[n] };
            ringNext = ringCount = 0;
            Forget();
            return true;
        }

        public void Forget() { before = false; }

        private static Vector3 Turns(ArticulationBody link) => link.isRoot && link.immovable ? Vector3.zero : link.angularVelocity;

        // How fast each link's centre truly goes, along the body from its root.
        private void Truly(Vector3[] goes)
        {
            for (int i = 0; i < Links.Length; i++)
            {
                var link = Links[i];
                if (i == 0 || up[i] < 0) { goes[i] = link.isRoot && link.immovable ? Vector3.zero : link.linearVelocity; continue; }
                var from = Links[up[i]];
                Vector3 anchor = link.transform.TransformPoint(link.anchorPosition);
                goes[i] = goes[up[i]] + Vector3.Cross(Turns(from), anchor - from.worldCenterOfMass) + Vector3.Cross(Turns(link), link.worldCenterOfMass - anchor);
            }
        }

        // What it holds (joules), its links going as given.
        private float HeldBy(Vector3[] goes)
        {
            float held = 0, g = -Physics.gravity.y;
            for (int i = 0; i < Links.Length; i++)
            {
                var link = Links[i];
                Vector3 w = Quaternion.Inverse(link.transform.rotation * link.inertiaTensorRotation) * Turns(link), inertia = link.inertiaTensor;
                held += .5f * link.mass * goes[i].sqrMagnitude + .5f * (inertia.x * w.x * w.x + inertia.y * w.y * w.y + inertia.z * w.z * w.z);
                if (link.useGravity) held += link.mass * g * (link.worldCenterOfMass.y - Level);
            }
            return held;
        }

        // The step's start: everything pushed on it for this step, and how it goes.
        public void Before(float dt)
        {
            if (Links.Length == 0) { before = false; return; }
            for (int i = 0; i < Links.Length; i++)
            {
                var link = Links[i];
                wBefore[i] = Turns(link);
                force[i] = link.GetAccumulatedForce(dt);
                torque[i] = link.GetAccumulatedTorque(dt);
                int dofs = i == 0 ? 0 : link.dofCount;
                if (jointBefore[i] == null || jointBefore[i].Length != dofs) jointBefore[i] = new float[dofs];
                if (dofs > 0) { var jv = link.jointVelocity; for (int d = 0; d < dofs; d++) jointBefore[i][d] = jv[d]; }
            }
            Truly(vBefore);
            heldBefore = HeldBy(vBefore);
            before = true;
        }

        // The step's end: what it holds, what was given, what it gained; the ring kept. False without its start.
        public bool After(float dt, int frame, float time, string label)
        {
            if (!before || Links.Length == 0) return false;
            before = false;
            int n = Links.Length;
            for (int i = 0; i < n; i++) wAfter[i] = Turns(Links[i]);
            Truly(vAfter);
            Held = HeldBy(vAfter);
            // (Each joint's torque, leaves first: its link's accumulated torque, and what its children's joints put back.)
            for (int i = 0; i < n; i++) jointTorque[i] = torque[i];
            for (int i = n - 1; i >= 1; i--) if (up[i] >= 0) jointTorque[up[i]] += jointTorque[i];
            Given = 0; Fastest = 0; FastestLink = -1;
            for (int i = 0; i < n; i++)
            {
                var link = Links[i];
                Vector3 w = .5f * (wBefore[i] + wAfter[i]), v = .5f * (vBefore[i] + vAfter[i]);
                float by = Vector3.Dot(force[i], v) * dt;
                if (i == 0 || up[i] < 0) by += Vector3.Dot(jointTorque[i], w) * dt;
                else
                {
                    by += Vector3.Dot(jointTorque[i], w - .5f * (wBefore[up[i]] + wAfter[up[i]])) * dt;
                    int dofs = link.dofCount;
                    if (dofs > 0 && jointBefore[i] != null && jointBefore[i].Length == dofs)
                    {
                        var jv = link.jointVelocity; var push = link.driveForce;
                        for (int d = 0; d < dofs; d++) by += push[d] * .5f * (jv[d] + jointBefore[i][d]) * dt;
                    }
                }
                LinkGiven[i] = by;
                Given += by;
                Speed[i] = vAfter[i].magnitude;
                Turning[i] = wAfter[i].magnitude;
                if (!(Turning[i] <= Fastest)) { Fastest = Turning[i]; FastestLink = i; }
            }
            Gain = Held - heldBefore - Given;
            var s = ring[ringNext];
            s.Frame = frame; s.Time = time; s.Label = label; s.Held = Held; s.Given = Given; s.Gain = Gain;
            System.Array.Copy(Speed, s.Speed, n); System.Array.Copy(Turning, s.Turning, n); System.Array.Copy(LinkGiven, s.ToLinks, n);
            ring[ringNext] = s;
            ringNext = (ringNext + 1) % RingSteps;
            ringCount = Mathf.Min(RingSteps, ringCount + 1);
            return true;
        }

        // The ring as text, oldest step first.
        public string Trace(string heading)
        {
            var t = new StringBuilder(heading).AppendLine();
            t.AppendLine("(Each step: frame, time, what it does; held, given, gained (J); then each link: true speed v (m/s), turning w (rad/s), given to it g (J).)");
            for (int k = 0; k < ringCount; k++)
            {
                var s = ring[(ringNext - ringCount + k + RingSteps) % RingSteps];
                t.Append(string.Format(Inv, "frame {0} t {1:0.000} [{2}] held {3:0.###} given {4:+0.###;-0.###} gained {5:+0.###;-0.###} |", s.Frame, s.Time, s.Label, s.Held, s.Given, s.Gain));
                for (int i = 0; i < Links.Length && i < s.Speed.Length; i++)
                    t.Append(string.Format(Inv, " {0} v {1:0.##} w {2:0.#} g {3:+0.###;-0.###};", Names[i], s.Speed[i], s.Turning[i], s.ToLinks[i]));
                t.AppendLine();
            }
            return t.ToString();
        }
    }
}
