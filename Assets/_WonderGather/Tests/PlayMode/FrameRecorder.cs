using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WonderGather.Tests
{
    // Every frame as played (Luis, October 30, 2026, after playing the learnt getting up: "much more visual checks for
    // all the what's happening in the game of every moment ... from all angles"; he saw twitches, arms bent as if broken
    // and a limb flicking out of frame for a moment, none of which the judges' pictures every 0.08 s could show). Written
    // at every rendered frame, after the model's bones are turned: what is drawn (the model's bones), what the body is
    // drawn from (the solved segments), and the physics (each part where it is, how fast it turns, and its joint's
    // turns). Read by framescan.py. And, at the frames asked for, pictures from four sides.
    [DefaultExecutionOrder(30000)]
    internal sealed class FrameRecorder : MonoBehaviour
    {
        public const int Version = 1, Head = 9, Bones = 15, Segments = 13, Parts = 13;
        public const int PerFrame = Head + 7 * Bones + 7 * Segments + 13 * Parts;

        public MinerBody Miner;
        public OwnBody Own;
        public PhysicalFall Fall;
        public Camera Pictures;
        public string PicturesTo;
        public int PictureSize = 256;
        // (Frames to picture: each a first and last frame, both included, and every how many.)
        public readonly List<Vector3Int> Pictured = new List<Vector3Int>();
        // Pictured as a fault happens (runs do not repeat frame for frame, so a fault found afterwards cannot be pictured
        // again): the frame it is seen in and so many after, at most so many times for each kind of fault.
        public bool AsItHappens;
        public int FaultFor = 10, FaultsOfAKind = 10;
        private int faultUntil = -1;
        private readonly Dictionary<string, int> faults = new Dictionary<string, int>(), faultNext = new Dictionary<string, int>();
        private readonly Quaternion[] limbWas = new Quaternion[8];
        private readonly Vector3[] endWas = new Vector3[4];
        private bool hadLimbs;

        private BinaryWriter data;
        private StreamWriter events;
        private int frame, steps;
        private string stateWas = "";
        private readonly float[] row = new float[PerFrame];

        public static FrameRecorder On(GameObject unit, string path)
        {
            var recorder = new GameObject("Frame recorder").AddComponent<FrameRecorder>();
            recorder.Miner = unit.GetComponent<MinerBody>();
            recorder.Own = unit.GetComponent<OwnBody>();
            recorder.Fall = unit.GetComponent<PhysicalFall>();
            recorder.data = new BinaryWriter(new FileStream(path + ".bin", FileMode.Create));
            recorder.events = new StreamWriter(path + ".txt");
            recorder.events.WriteLine("FRAMES version " + Version + " floats " + PerFrame + " fixed " + Time.fixedDeltaTime + " capture " + Time.captureFramerate);
            return recorder;
        }

        private void FixedUpdate() => steps++;

        private int k;
        private void Put(float x) => row[k++] = x;
        private void Put(Transform t)
        {
            if (t == null) { for (int i = 0; i < 7; i++) Put(float.NaN); return; }
            Vector3 p = t.position; Quaternion q = t.rotation;
            Put(p.x); Put(p.y); Put(p.z); Put(q.x); Put(q.y); Put(q.z); Put(q.w);
        }

        private void LateUpdate()
        {
            if (data == null || Miner == null) return;
            if (Fall == null) Fall = Miner.GetComponent<PhysicalFall>();
            if (Own == null) Own = Miner.GetComponent<OwnBody>();
            k = 0;
            Put(frame); Put(Time.time); Put(Time.fixedTime); Put(steps);
            Put(Fall != null ? (int)Fall.Now : -1);
            bool stands = Own != null && Own.Stands;
            Put(stands ? 1 : 0); Put(Own != null && Own.GettingUp ? 1 : 0); Put(Own != null && Own.Rising ? 1 : 0); Put(Time.deltaTime);
            // What is drawn: the model's bones.
            var b = Miner.Rig;
            foreach (var t in new[] { b.pelvis, b.chest, b.head }) Put(t);
            for (int i = 0; i < 2; i++) Put(At(b.upperArms, i));
            for (int i = 0; i < 2; i++) Put(At(b.forearms, i));
            for (int i = 0; i < 2; i++) Put(At(b.hands, i));
            for (int i = 0; i < 2; i++) Put(At(b.thighs, i));
            for (int i = 0; i < 2; i++) Put(At(b.shins, i));
            for (int i = 0; i < 2; i++) Put(At(b.feet, i));
            // What it is drawn from: the solved segments.
            var s = Miner.Solved;
            foreach (var t in new[] { s.pelvis, s.torso, s.head }) Put(t);
            for (int i = 0; i < 2; i++) Put(At(s.upperArms, i));
            for (int i = 0; i < 2; i++) Put(At(s.forearms, i));
            for (int i = 0; i < 2; i++) Put(At(s.thighs, i));
            for (int i = 0; i < 2; i++) Put(At(s.shins, i));
            for (int i = 0; i < 2; i++) Put(At(s.feet, i));
            // The physics: its own body's links while it stands by its own joints (their joints' turns, radians, from
            // the pose it was made in), else the let-go body's parts.
            for (int i = 0; i < Parts; i++)
            {
                Transform t = null; Vector3 turning = new Vector3(float.NaN, float.NaN, float.NaN);
                float j0 = float.NaN, j1 = float.NaN, j2 = float.NaN;
                if (stands)
                {
                    var link = Own.Part(i);
                    if (link != null)
                    {
                        t = link.transform; turning = link.angularVelocity;
                        if (i > 0)
                        {
                            var at = link.jointPosition;
                            if (at.dofCount > 0) j0 = at[0];
                            if (at.dofCount > 1) j1 = at[1];
                            if (at.dofCount > 2) j2 = at[2];
                        }
                    }
                }
                else if (Fall != null && Fall.Now != PhysicalFall.State.Up && i < PhysicalFall.Count)
                {
                    var part = Fall.Part(i);
                    if (part != null) { t = part.transform; turning = part.angularVelocity; }
                }
                Put(t); Put(turning.x); Put(turning.y); Put(turning.z); Put(j0); Put(j1); Put(j2);
            }
            for (int i = 0; i < PerFrame; i++) data.Write(row[i]);
            // What it is doing, when that changes.
            string state = (Fall != null ? Fall.Now.ToString() + (Fall.StageNow.Length > 0 ? ":" + Fall.StageNow : "") : "none")
                + (stands ? " own" + (Own.GettingUp ? " getting up" + (Own.Riser != null ? ":" + Own.Riser.StageNow : "") : "") + (Own.Rising ? " rising" : "") : "");
            if (state != stateWas)
            {
                events.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1:0.000} {2}", frame, Time.time, state));
                stateWas = state;
            }
            string fault = Fault(stands);
            if (AsItHappens && fault != null && frame > faultUntil)
            {
                string kind = fault.Split(' ')[0];
                faults.TryGetValue(kind, out int seen);
                // (Not the same kind again for two seconds: one long fault is pictured once, and others are seen.)
                faultNext.TryGetValue(kind, out int next);
                if (seen < FaultsOfAKind && frame >= next)
                {
                    faults[kind] = seen + 1; faultNext[kind] = frame + 2 * Mathf.Max(1, Time.captureFramerate);
                    faultUntil = frame + FaultFor - 1; Note("FAULT " + fault + " [" + state + "]");
                }
            }
            if (Pictures != null && PicturesTo != null && (Wanted(frame) || frame <= faultUntil)) Picture();
            frame++;
        }

        // What is wrong in this frame, if anything: a drawn limb turned more than 30 degrees since the last frame; a drawn
        // hand or foot moved more than 6 cm since it; and, standing by its own joints, an elbow or knee bent the way it does
        // not bend (more than 8 degrees) or folded past 150, or a part turning faster than 30 rad/s.
        private string Fault(bool stands)
        {
            var b = Miner.Rig;
            string why = null;
            var limbs = new[] { At(b.upperArms, 0), At(b.upperArms, 1), At(b.forearms, 0), At(b.forearms, 1), At(b.thighs, 0), At(b.thighs, 1), At(b.shins, 0), At(b.shins, 1) };
            string[] limbNames = { "upperArmL", "upperArmR", "forearmL", "forearmR", "thighL", "thighR", "shinL", "shinR" };
            var ends = new[] { At(b.hands, 0), At(b.hands, 1), At(b.feet, 0), At(b.feet, 1) };
            string[] endNames = { "handL", "handR", "footL", "footR" };
            for (int i = 0; i < limbs.Length; i++)
            {
                if (limbs[i] == null) continue;
                float turned = hadLimbs ? Quaternion.Angle(limbWas[i], limbs[i].rotation) : 0;
                if (turned > 30 && why == null) why = $"spin {limbNames[i]} {turned:0}";
                limbWas[i] = limbs[i].rotation;
            }
            for (int i = 0; i < ends.Length; i++)
            {
                if (ends[i] == null) continue;
                float moved = hadLimbs ? Vector3.Distance(endWas[i], ends[i].position) : 0;
                if (moved > .06f && why == null) why = $"jump {endNames[i]} {moved:0.00}";
                endWas[i] = ends[i].position;
            }
            hadLimbs = true;
            if (stands)
            {
                // (A limb's part is made with its length up and the side its elbow or knee sticks out forward: bent as it
                // bends, the far part swings away from that side.)
                int[,] pairs = { { OwnBody.UpperArm, OwnBody.Forearm }, { OwnBody.UpperArm + 1, OwnBody.Forearm + 1 }, { OwnBody.Thigh, OwnBody.Shin }, { OwnBody.Thigh + 1, OwnBody.Shin + 1 } };
                string[] joints = { "elbowL", "elbowR", "kneeL", "kneeR" };
                for (int k = 0; k < 4; k++)
                {
                    var near = Own.Part(pairs[k, 0]); var far = Own.Part(pairs[k, 1]);
                    if (near == null || far == null) continue;
                    Vector3 along = near.transform.up, point = near.transform.forward, on = far.transform.up;
                    float bent = Mathf.Atan2(Vector3.Dot(on, -point), Vector3.Dot(on, along)) * Mathf.Rad2Deg;
                    if (why == null && bent < -8 && bent > -150) why = $"wrongway {joints[k]} {-bent:0}";
                    if (why == null && Mathf.Abs(bent) > 150) why = $"overfold {joints[k]} {Mathf.Abs(bent):0}";
                }
                for (int i = 1; i < Parts; i++)
                {
                    var link = Own.Part(i);
                    if (link != null && why == null && link.angularVelocity.magnitude > 30) why = $"whip {i} {link.angularVelocity.magnitude:0}";
                }
            }
            return why;
        }

        public void Note(string what)
        {
            if (events != null) events.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1:0.000} NOTE {2}", frame, Time.time, what));
        }

        private bool Wanted(int f)
        {
            foreach (var r in Pictured) if (f >= r.x && f <= r.y && (f - r.x) % Mathf.Max(1, r.z) == 0) return true;
            return false;
        }

        // Four sides of the body as it is drawn: from two sides at its height, from above, and close from a third.
        private void Picture()
        {
            var b = Miner.Rig;
            if (b.pelvis == null) return;
            Vector3 at = b.pelvis.position;
            var sides = new[]
            {
                (Vector3.forward * 1.7f + Vector3.up * .35f, "a"),
                (Vector3.right * 1.7f + Vector3.up * .35f, "b"),
                (Vector3.up * 2.2f + Vector3.forward * .05f, "c"),
                ((Vector3.back + Vector3.left).normalized * 1.0f + Vector3.up * .3f, "d"),
            };
            foreach (var (from, name) in sides)
            {
                Vector3 where = at + from;
                Pictures.transform.SetPositionAndRotation(where, Quaternion.LookRotation(at - where, Mathf.Abs(Vector3.Dot((at - where).normalized, Vector3.up)) > .95f ? Vector3.forward : Vector3.up));
                CaptureTools.Render(Pictures, Path.Combine(PicturesTo, $"f{frame:000000}{name}"), PictureSize, PictureSize);
            }
        }

        private static Transform At(Transform[] all, int i) => all != null && i < all.Length ? all[i] : null;

        private void OnDestroy() => Close();

        public void Close()
        {
            if (data != null) { data.Flush(); data.Close(); data = null; }
            if (events != null) { events.Flush(); events.Close(); events = null; }
        }
    }
}
