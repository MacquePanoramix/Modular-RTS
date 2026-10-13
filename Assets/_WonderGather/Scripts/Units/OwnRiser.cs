using UnityEngine;

namespace WonderGather
{
    // S3b, step 2 (Docs/Design/TheBodysOwn.md): getting up, on the body that stands by its own joints.
    //
    // Luis, October 13: "I want the character to get up naturally with their own limbs and strength in a natural
    // manner and the character shouldn't look dead and then start moving to get up after a timer."
    //
    // The body that stands by its own joints (thirteen weighted parts, each boot on an ankle of its own) is made as
    // it lies, and from there on nothing poses it. This holds its joints towards one pose after another (a way: a
    // row of poses, each a set of turns from how the body stands), each joint with no more than it has. What moves
    // the body is its limbs and its weight on the ground. At the end of a way that brings it onto its feet (its
    // soles flat, its weight over them) the keeper that stands it takes it, and raises it by its legs (OwnKeeper,
    // rising).
    //
    // One piece of code, used by the body in the game (OwnBody) and by the bench that searches its ways.
    public sealed class OwnRiser
    {
        public const int Hips = OwnKeeper.Hips, Trunk = OwnKeeper.Trunk, Head = OwnKeeper.Head, UpperArm = OwnKeeper.UpperArm, Forearm = OwnKeeper.Forearm, Thigh = OwnKeeper.Thigh, Shin = OwnKeeper.Shin, Foot = OwnKeeper.Foot, Count = OwnKeeper.Count;

        public enum Does { Gathers, Ready, GaveUp }
        // What is asked of the body before it goes on from one pose to the next: how far its chest faces the ground
        // (1: flat on its front; -1: on its back); how high its hips are over the ground, how far they are from over
        // its feet, and how far its weight is ahead of the middle between its soles (shares of its hips' standing
        // height); how steep its shins stand and how flat its soles lie (1: as standing); how upright its trunk is
        // (1); how long it has been still (seconds); how high its head is (a share of its hips' height); and what
        // share of its weight the ground bore its boots with at the last step of the physics.
        public enum Asks { Front, Hips, Off, Ahead, Steep, Soles, Upright, Still, Head, Bears }
        public struct Asked { public Asks what; public float least, most; }
        // One pose: come to from the one before over so long (seconds), kept at least so long, given up after so
        // long. Its turns from standing, in degrees: the trunk's bow, the head's, the trunk's twist (its leading
        // side going forward); then, for the side that leads (the uppermost when it began) and for the other, an
        // arm (the shoulder forward, the arm out from the body, the elbow bent) and then a leg (the hip bent, the
        // thigh out, the knee bent, the ankle bent up).
        public const int Turns = 17;
        public sealed class Stage
        {
            public string name;
            public float over = .8f, atLeast = .8f, within = 4;
            public float[] pose = new float[Turns];
            public Asked[] through = new Asked[0];
            // If it keeps its weight over its feet in this pose: how far ahead of the middle between its soles (a
            // share of its hips' height). It does so by bowing more or less at the hips.
            public float keeps = float.NaN;
            // If a foot is put somewhere in this pose (0: the leading leg's foot; 1: the other; -1: none): its boot is
            // carried, lifted so far in the middle (metres), to lie flat on the ground so far ahead of its own hip
            // and so far out from it (shares of the leg's length), the way the body lies (from its hips towards its
            // head). That leg's hip, knee and ankle are then asked for what puts the boot there, whatever the ground
            // and however the body lies, and not for the pose's turns. The pose is through only once that boot
            // bears on the ground.
            public int steps = -1;
            public float stepAhead, stepOut, stepLifts = .06f;
            // If it sits back in this pose (NaN: not): its knees bend further at up to so many degrees a second (and
            // its hips by sitHips times as much) for as long as its weight is further ahead of the middle between its
            // soles than sitAhead (a share of its hips' height), and come back when it is behind that; never more
            // than sitMost degrees beyond the pose. So it sits back as far as brings its weight over its feet on the
            // ground it is on, and no further.
            public float sits = float.NaN, sitHips = .5f, sitAhead = .05f, sitMost = 100;
        }
        // One way, for a body whose chest faces the ground by between this and this much. At its end the body is on
        // its feet and the keeper takes it (gives); or it only lies another way, and chooses its way again.
        public sealed class Way
        {
            public string name;
            public float frontLeast = -1, frontMost = 1;
            public bool gives;
            public Stage[] stages = new Stage[0];
        }

        // Each joint is held towards its pose with all it has when it is FullAt degrees from it (never stiffer than
        // the keeper's rule for the step of the physics), damped by Damped of what just stops it swinging. Keeping
        // its weight over its feet, its hips bend Balances degrees more for each second its weight is a whole hip's
        // height too far back, and no more than BalancesAtMost more or less than the pose has them.
        public static float FullAt = 35, Damped = .7f, Balances = 150, BalancesAtMost = 70;
        private const float StillUnder = .2f, StillTurning = 1.2f;

        public Way[] Ways = new Way[0];
        // How hard it holds itself (1: with all the strength it has).
        public float Tone = 1;

        private readonly ArticulationBody[] parts;
        private readonly int[] from;
        private readonly float[] masses, strength, lighter;
        private readonly bool[] hinge;
        private readonly GroundTouch[] touches;
        private readonly Vector3[] soleAt;
        private readonly Quaternion[] toStood, rested, stoodBoot;
        private readonly Vector3[] anchors;
        private readonly float[] bears = new float[2];
        private Vector3 stepFrom;
        private bool stepBegun;
        private readonly Quaternion[] aimFrom = new Quaternion[Count], aimNow = new Quaternion[Count];
        private readonly float tall, whole;
        private readonly int ground;
        private Way way;
        private Stage stage;
        private int at, leads, chosen;
        private float got, still, leant, bore, toned = -1, sat;

        public string WayNow => way != null ? way.name : "";
        public string StageNow => stage != null ? stage.name : "";
        // What a joint is asked for now: its turn, in its own frame, from how it was made.
        public Quaternion Aim(int j) => aimNow[j];
        public float Lasted { get; private set; }

        // rested: each part's turn against what it hangs from, as it was made. anchors: where each part's joint is, in
        // the part. stoodBoot: how each boot is turned standing, from the way the body faces.
        public OwnRiser(ArticulationBody[] parts, int[] from, float[] masses, float[] strength, bool[] hinge, GroundTouch[] touches, Vector3[] soleAt, Quaternion[] toStood, float[] lighter, float tall, int ground,
            Quaternion[] rested, Vector3[] anchors, Quaternion[] stoodBoot)
        {
            this.parts = parts; this.from = from; this.masses = masses; this.strength = strength; this.hinge = hinge;
            this.touches = touches; this.soleAt = soleAt; this.toStood = toStood; this.lighter = lighter; this.tall = Mathf.Max(.1f, tall); this.ground = ground;
            this.rested = rested; this.anchors = anchors; this.stoodBoot = stoodBoot;
            float all = 0;
            for (int i = 0; i < Count; i++) { all += masses[i]; aimNow[i] = aimFrom[i] = Quaternion.identity; }
            whole = all;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        public Vector3 Weight()
        {
            Vector3 c = Vector3.zero;
            for (int i = 0; i < Count; i++) c += parts[i].worldCenterOfMass * masses[i];
            return c / whole;
        }

        private float GroundAt(Vector3 place) => Physics.Raycast(place + Vector3.up * .5f, Vector3.down, out var below, 4, ground) ? below.point.y : place.y;

        public float Measure(Asks what)
        {
            Vector3 hips = parts[Hips].transform.position;
            Vector3 feet = .5f * (parts[Foot].transform.TransformPoint(soleAt[0]) + parts[Foot + 1].transform.TransformPoint(soleAt[1]));
            switch (what)
            {
                case Asks.Front: return Vector3.Dot(parts[Trunk].transform.forward, Vector3.down);
                case Asks.Hips: return (hips.y - GroundAt(hips)) / tall;
                case Asks.Off: return Flat(hips - feet).magnitude / tall;
                case Asks.Ahead:
                    Vector3 toes = Flat(parts[Foot].transform.forward + parts[Foot + 1].transform.forward);
                    if (toes.sqrMagnitude < 1e-6f) return 0;
                    return Vector3.Dot(Flat(Weight() - feet), toes.normalized) / tall;
                case Asks.Steep: return Mathf.Min(-parts[Shin].transform.up.y, -parts[Shin + 1].transform.up.y);
                case Asks.Soles: return Mathf.Min(parts[Foot].transform.up.y, parts[Foot + 1].transform.up.y);
                case Asks.Upright: return parts[Trunk].transform.up.y;
                case Asks.Head:
                    Vector3 head = parts[Head].transform.position;
                    return (head.y - GroundAt(head)) / tall;
                case Asks.Bears: return bore;
                default: return still;
            }
        }

        // Each joint's spring, by how hard it holds itself now.
        private void Springs(float step)
        {
            toned = Tone;
            for (int j = 1; j < Count; j++)
            {
                float has = strength[j] * Mathf.Clamp01(Tone);
                float spring = Mathf.Min(has / (FullAt * Mathf.Deg2Rad), OwnKeeper.Rule * OwnKeeper.Rule / (step * step) * lighter[j]);
                float damper = Damped * 2 * Mathf.Sqrt(spring * lighter[j]);
                var drive = parts[j].xDrive; drive.stiffness = spring; drive.damping = damper; drive.forceLimit = has; parts[j].xDrive = drive;
                if (hinge[j]) continue;
                drive = parts[j].yDrive; drive.stiffness = spring; drive.damping = damper; drive.forceLimit = has; parts[j].yDrive = drive;
                drive = parts[j].zDrive; drive.stiffness = spring; drive.damping = damper; drive.forceLimit = has; parts[j].zDrive = drive;
            }
        }

        private void Aims(int j, Quaternion own)
        {
            own.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.right; angle = 0; }
            Vector3 by = angle * axis;
            parts[j].SetDriveTarget(ArticulationDriveAxis.X, by.x);
            if (hinge[j]) return;
            parts[j].SetDriveTarget(ArticulationDriveAxis.Y, by.y);
            parts[j].SetDriveTarget(ArticulationDriveAxis.Z, by.z);
        }

        // A joint's turn from how it stands, in its own frame, for the pose it is at. (A limb's part lies along its
        // own second line, pointing away from the body, and bends about its first: a thigh goes forward by turning
        // one way about it, a shin back by turning the other; an arm's parts are made facing back, so they go the
        // other way round from a leg's.)
        private Quaternion Turn(int j)
        {
            float[] p = stage.pose;
            if (j == Trunk) return Quaternion.Euler(p[0], p[2] * (leads == 1 ? -1 : 1), 0);
            if (j == Head) return Quaternion.Euler(p[1], 0, 0);
            int side = (j - UpperArm) % 2, n = side == leads ? 0 : 1, a = 3 + 3 * n, l = 9 + 4 * n;
            switch (j - side)
            {
                case UpperArm: return Quaternion.AngleAxis(-p[a], Vector3.right) * Quaternion.AngleAxis(p[a + 1] * (side == 0 ? 1 : -1), Vector3.forward);
                case Forearm: return Quaternion.AngleAxis(-p[a + 2], Vector3.right);
                case Thigh: return Quaternion.AngleAxis(Mathf.Clamp(p[l] + leant + (float.IsNaN(stage.sits) ? 0 : stage.sitHips * sat), -20, 130), Vector3.right) * Quaternion.AngleAxis(p[l + 1] * (side == 0 ? -1 : 1), Vector3.forward);
                case Shin: return Quaternion.AngleAxis(-(p[l + 2] + (float.IsNaN(stage.sits) ? 0 : sat)), Vector3.right);
                default: return Quaternion.AngleAxis(-p[l + 3], Vector3.right);
            }
        }

        // It chooses its way, by how it lies. The side that is uppermost leads.
        private bool Choose()
        {
            float front = Measure(Asks.Front);
            way = null;
            if (++chosen > 4) return false;
            foreach (var one in Ways)
                if (one != null && one.stages.Length > 0 && front >= one.frontLeast && front <= one.frontMost) { way = one; break; }
            if (way == null) return false;
            leads = Vector3.Dot(parts[Trunk].transform.right, Vector3.up) >= 0 ? 1 : 0;
            leant = 0;
            Begin(0);
            return true;
        }

        private Vector3 JointAt(int j) => parts[j].transform.TransformPoint(anchors[j]);

        // The way the body lies along the ground: from its hips towards its head when it is bowed over or lying;
        // the way its hips face when its trunk is up.
        public Vector3 Lies()
        {
            Vector3 along = Flat(parts[Head].transform.position - parts[Hips].transform.position), faces = Flat(parts[Hips].transform.forward);
            if (along.magnitude > .25f * tall) return along.normalized;
            return faces.sqrMagnitude > 1e-6f ? faces.normalized : Vector3.forward;
        }

        // A foot is put somewhere: its boot carried there, lifted in the middle, and laid flat; its leg's joints asked
        // for what puts it there.
        private void Steps(int leg, float along)
        {
            Vector3 lies = Lies(), aside = Vector3.Cross(Vector3.up, lies) * (leg == 1 ? 1 : -1);
            Vector3 hip = JointAt(Thigh + leg);
            float thighLong = (JointAt(Shin + leg) - hip).magnitude, shinLong = (JointAt(Foot + leg) - JointAt(Shin + leg)).magnitude, reach = thighLong + shinLong;
            Vector3 to = Flat(hip) + lies * (stage.stepAhead * reach) + aside * (stage.stepOut * reach);
            // (Once it is there it is pressed down a little: laid exactly on the ground, a boot bore nothing, and the
            // pose that waits for it to bear waited for ever.)
            to.y = GroundAt(new Vector3(to.x, hip.y, to.z)) - (along >= 1 ? .03f : 0);
            Vector3 sole = Vector3.Lerp(stepFrom, to, along) + Vector3.up * (stage.stepLifts * Mathf.Sin(Mathf.PI * along));
            Quaternion boot = Quaternion.LookRotation(lies, Vector3.up) * stoodBoot[leg];
            // Its hip and knee, for its ankle to be there: its knee bends the way the front of its hips faces (down,
            // when it is bowed over on its knees; ahead, when it is up). (Until October 14 "up and ahead" in the
            // world: bowed over, with its foot still behind it, that sent its knee up over its back.)
            Vector3 ankle = sole + boot * (anchors[Foot + leg] - soleAt[leg]) - hip;
            float far = Mathf.Clamp(ankle.magnitude, Mathf.Abs(thighLong - shinLong) + .01f, reach * .995f);
            Vector3 line = ankle.normalized, pole = Vector3.ProjectOnPlane(parts[Hips].transform.forward, line);
            pole = pole.sqrMagnitude > 1e-6f ? pole.normalized : Vector3.ProjectOnPlane(lies + Vector3.up, line).normalized;
            float opens = Mathf.Acos(Mathf.Clamp((thighLong * thighLong + far * far - shinLong * shinLong) / (2 * thighLong * far), -1, 1));
            Vector3 thighWay = line * Mathf.Cos(opens) + pole * Mathf.Sin(opens);
            Vector3 shinWay = (line * far - thighWay * thighLong).normalized;
            Quaternion thigh = Quaternion.LookRotation(Vector3.ProjectOnPlane(pole, thighWay).normalized, thighWay);
            Quaternion shin = Quaternion.LookRotation(Vector3.ProjectOnPlane(pole, shinWay).normalized, shinWay);
            aimNow[Thigh + leg] = Quaternion.Inverse(parts[Hips].transform.rotation * rested[Thigh + leg]) * thigh;
            aimNow[Shin + leg] = Quaternion.Inverse(thigh * rested[Shin + leg]) * shin;
            aimNow[Foot + leg] = Quaternion.Inverse(shin * rested[Foot + leg]) * boot;
            for (int j = Thigh + leg; j <= Foot + leg; j += 2) Aims(j, aimNow[j]);
        }

        private void Begin(int index)
        {
            at = index; stage = way.stages[index]; got = 0; still = 0; stepBegun = false; sat = 0;
            for (int j = 1; j < Count; j++) aimFrom[j] = aimNow[j];
        }

        // One step of getting up.
        public Does Step(float step)
        {
            float g = Physics.gravity.magnitude;
            for (int i = 0; i < 2; i++) { bears[i] = touches[i].Up / Mathf.Max(1e-6f, whole * g * step); touches[i].Clear(); }
            bore = bears[0] + bears[1];
            bool moving = false;
            for (int i = Hips; i <= Head && !moving; i++)
                moving = parts[i].GetPointVelocity(parts[i].worldCenterOfMass).sqrMagnitude > StillUnder * StillUnder || parts[i].angularVelocity.sqrMagnitude > StillTurning * StillTurning;
            still = moving ? 0 : still + step;
            if (toned != Tone) Springs(step);
            if (way == null && !Choose()) return Does.GaveUp;
            got += step; Lasted += step;
            if (float.IsNaN(stage.keeps)) leant = Mathf.MoveTowards(leant, 0, 60 * step);
            else leant = Mathf.Clamp(leant + Balances * (stage.keeps - Measure(Asks.Ahead)) * step, -BalancesAtMost, BalancesAtMost);
            if (!float.IsNaN(stage.sits)) sat = Mathf.Clamp(sat + stage.sits * Mathf.Clamp((Measure(Asks.Ahead) - stage.sitAhead) / .1f, -1, 1) * step, 0, stage.sitMost);
            bool through = got >= stage.atLeast;
            for (int k = 0; through && k < stage.through.Length; k++)
            {
                float it = Measure(stage.through[k].what);
                through = it >= stage.through[k].least && it <= stage.through[k].most;
            }
            // (A foot being put somewhere: only once its boot bears on the ground.)
            int stepping = stage.steps < 0 ? -1 : stage.steps == 0 ? leads : 1 - leads;
            if (stepping >= 0 && bears[stepping] < .02f) through = false;
            if (through)
            {
                if (at + 1 < way.stages.Length) Begin(at + 1);
                else if (way.gives) return Does.Ready;
                else if (!Choose()) return Does.GaveUp;
            }
            else if (got > stage.within) return Does.GaveUp;
            // The pose it is at, come to from where its joints were asked to be before.
            float share = stage.over > 0 ? Mathf.SmoothStep(0, 1, got / stage.over) : 1;
            stepping = stage.steps < 0 ? -1 : stage.steps == 0 ? leads : 1 - leads;
            for (int j = 1; j < Count; j++)
            {
                if (stepping >= 0 && j >= Thigh && (j - Thigh) % 2 == stepping) continue;
                // (A leg whose hip's turn is not given is left as it is asked now: a foot that was put somewhere stays.)
                if (j >= Thigh && float.IsNaN(stage.pose[9 + 4 * ((j - Thigh) % 2 == leads ? 0 : 1)])) continue;
                aimNow[j] = Quaternion.Slerp(aimFrom[j], toStood[j] * Turn(j), share);
                Aims(j, aimNow[j]);
            }
            if (stepping >= 0)
            {
                if (!stepBegun) { stepBegun = true; stepFrom = parts[Foot + stepping].transform.TransformPoint(soleAt[stepping]); }
                Steps(stepping, share);
            }
            return Does.Gathers;
        }
    }
}
