using UnityEngine;

namespace WonderGather
{
    // The keeper of a body of its own (S3b; Docs/Design/TheBodysOwn.md): what keeps thirteen weighted parts on
    // joints standing, by what the legs' joints give against the ground and nothing else. One piece of code, used
    // by the body in the game (OwnBody) and by the bench that searches its settings (BodysOwnBench), so that what
    // is searched is what is played. Each step of the physics:
    //  - what the ground should push the body with is reckoned: its weight, a little more or less to keep its
    //    height, and sideways as much as brings its weight back from where it is going;
    //  - where on its soles that push must act, no further than the soles reach; where the ground really bore it
    //    in the last step is read from what touched its boots, and what it means is corrected by how far that
    //    fell short, but never off a sole;
    //  - each leg's hip, knee and ankle give what makes its boot push the ground so, and no more than they have;
    //  - its hips keep its trunk upright;
    //  - what pushes it from outside is felt (by how its weight really went), and it leans against it.
    // Every torque is put on the two parts a joint joins, equal and opposite. Whoever owns the body may live in
    // the middle of each step (Life): it says how far from where it stands the weight is held (Shift) and how its
    // hips roll (Roll).
    public sealed class OwnKeeper
    {
        // The parts, in this order (the left of a pair first).
        public const int Hips = 0, Trunk = 1, Head = 2, UpperArm = 3, Forearm = 5, Thigh = 7, Shin = 9, Foot = 11, Count = 13;
        // It is down when its weight is lower than this share of how high it stood, or its hips lean further than
        // this (degrees).
        public const float DownAt = .8f, DownLeaning = 35;

        // ---- Its settings. Found by a search on the three miners together (BodysOwnBench.Search, at the game's
        // fifty steps of the physics a second), not set by hand: change what the search is asked, run it, and put
        // what it finds here.
        // How quick it brings its weight back (a second); over how long it comes to feel a push from outside
        // (seconds); how fast it leans against one (a second), by what share of what would ease it, only for what
        // is beyond Comfort (metres from where its push acts at ease), and never leaving less sole than KeepsBehind
        // behind its weight; how fast it learns how far the ground's real push falls short of the one it meant (a
        // second).
        public static float Quick = 6.33f, Feels = .04005f, Leans = 2.9021f, LeansBy = .94264f, Comfort = .03266f, KeepsBehind = .02071f, Tracks = 6.8665f;
        // Each joint's spring gives all the joint's strength when it is FullAt degrees from its pose, but is no
        // stiffer than makes its own rate, times the step, Rule (by the turning weight of the lighter side of the
        // joint: a stiff spring between a heavy body and a light boot does not hold at the game's step); its damper
        // is Damped of what just stops that side swinging.
        public static float FullAt = 92.743f, Rule = .64038f, Damped = .38328f;
        // How hard its hips right its trunk, for each kilogram of it, and the share of that which damps it; how near
        // the edge of a sole its push may act (metres); how hard it keeps its height, and how that is damped.
        public static float Rights = 5.3963f, RightsDamped = .05f, Edge = .00435f, Rises = 224.2f, RisesDamped = 14.277f;

        private readonly ArticulationBody[] parts;
        private readonly int[] from;
        private readonly float[] masses, strength;
        private readonly bool[] hinge;
        private readonly Vector3[] anchors, soleAt, soleSize;
        private readonly GroundTouch[] touches;
        private readonly float whole;
        // Its state.
        private bool begun, hasBefore, realKnown;
        private Vector3 rest, holds, felt, wentBefore, weightBefore, actedBefore, realAt, shortBy, acted, stands;
        private Vector2 standsBy;
        private Quaternion upright;
        private float height;
        private readonly Vector3[] torque = new Vector3[Count], sole = new Vector3[2], at = new Vector3[2];
        private readonly float[] gave = new float[Count];

        // What whoever owns the body says: its life, lived in the middle of each step (the step's length; whether
        // it is being pushed; how its boots stand apart); how far from where it stands its weight is held; how its
        // hips roll from upright; and how far towards the middle of its boots it holds its weight (0: where the
        // posed body had it, near its heels).
        public System.Action<float, bool, Vector3> Life;
        public Vector3 Shift;
        public Quaternion Roll = Quaternion.identity;
        public float StandsMid;

        // What a joint's worked-out torque was in the last step (newton metres); how far outside its soles the
        // push it wanted lay (metres); how far it leans against what it feels, and what it feels (m/s2); how its
        // hips were turned when it began.
        public float[] Gave => gave;
        public float Outside { get; private set; }
        public Vector3 Leaning => holds - rest;
        public Vector3 Felt => felt;
        public Quaternion Upright => upright;
        public float Whole => whole;

        public OwnKeeper(ArticulationBody[] parts, int[] from, float[] masses, float[] strength, bool[] hinge, Vector3[] anchors, GroundTouch[] touches, Vector3[] soleAt, Vector3[] soleSize)
        {
            this.parts = parts; this.from = from; this.masses = masses; this.strength = strength; this.hinge = hinge; this.anchors = anchors;
            this.touches = touches; this.soleAt = soleAt; this.soleSize = soleSize;
            float all = 0;
            for (int i = 0; i < Count; i++) all += masses[i];
            whole = all;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        private Vector3 Weight()
        {
            Vector3 c = Vector3.zero;
            for (int i = 0; i < Count; i++) c += parts[i].worldCenterOfMass * masses[i];
            return c / whole;
        }

        private Vector3 WeightGoes()
        {
            Vector3 v = Vector3.zero;
            for (int i = 0; i < Count; i++) v += parts[i].GetPointVelocity(parts[i].worldCenterOfMass) * masses[i];
            return v / whole;
        }

        private Vector3 JointAt(int j) => parts[j].transform.TransformPoint(anchors[j]);

        // One step of keeping itself up.
        public bool Keep(float step)
        {
            float g = Physics.gravity.magnitude;
            Vector3 weight = Weight(), goes = WeightGoes();
            // Where the ground really bore it in the last step.
            {
                Vector3 bore = Vector3.zero; float lifted = 0;
                for (int i = 0; i < 2; i++) { bore += touches[i].Where; lifted += touches[i].Up; touches[i].Clear(); }
                realKnown = lifted > 1e-6f;
                if (realKnown) realAt = bore / lifted;
            }
            for (int i = 0; i < 2; i++) sole[i] = parts[Foot + i].transform.TransformPoint(soleAt[i]);
            float ground = .5f * (sole[0].y + sole[1].y), high = weight.y - ground;
            if (!begun)
            {
                begun = true; height = high; upright = parts[Hips].transform.rotation;
                rest = Flat(weight); holds = rest;
                Vector3 between = Flat(.5f * (sole[0] + sole[1])), beside0 = Flat(sole[1] - sole[0]).normalized;
                standsBy = new Vector2(Vector3.Dot(rest - between, beside0), (1 - StandsMid) * Vector3.Dot(rest - between, Vector3.Cross(beside0, Vector3.up)));
            }
            // Down: it goes over into the fall that is built.
            if (float.IsNaN(high) || high < DownAt * height || Vector3.Angle(Vector3.up, parts[Hips].transform.up) > DownLeaning) return false;

            float rise = Mathf.Clamp(Rises * (height - high) - RisesDamped * goes.y, -.5f * g, g);
            // A standing body falls away from where the ground pushes it, at a rate of its own (the root of gravity
            // over its height). So what matters is where its weight is going: its place, and its speed over that rate.
            float falls = Mathf.Sqrt((g + rise) / Mathf.Max(.1f, high));
            if (hasBefore && Feels > 0)
            {
                // What pushes it from outside: how its weight really went, beyond what the ground's push gave it.
                Vector3 pushedAt = realKnown && Tracks > 0 ? realAt : actedBefore;
                Vector3 seen = Flat(goes - wentBefore) / step - falls * falls * Flat(weightBefore - pushedAt);
                felt = Vector3.ClampMagnitude(Vector3.Lerp(felt, seen, Mathf.Clamp01(step / Feels)), 4);
            }
            if (hasBefore && realKnown && Tracks > 0) shortBy = Vector3.ClampMagnitude(shortBy + Tracks * step * Flat(actedBefore - realAt), .05f);
            // The life in it: where it holds its weight between its boots, and what its joints are asked for.
            // (Boots that slid under a push take where it stands with them.)
            Vector3 apart = Flat(sole[1] - sole[0]);
            stands = Flat(.5f * (sole[0] + sole[1])) + apart.normalized * standsBy.x + Vector3.Cross(apart.normalized, Vector3.up) * standsBy.y;
            Life?.Invoke(step, felt.magnitude / (falls * falls) > Comfort, apart);
            rest = stands + Shift;
            Vector3 going = Flat(weight) + Flat(goes) / falls;
            Vector3 wants = going + Quick / falls * (going - holds) + felt / (falls * falls);
            Vector3 acts = new Vector3(wants.x, ground, wants.z);

            // The push is shared between the boots by how far across it lies, and each boot bears its share along
            // its own middle line; only a push wanted outside both is taken to a boot's outer side.
            Vector3 over = Flat(sole[1] - sole[0]);
            Vector3 beside = over.sqrMagnitude > 1e-8f ? over.normalized : Vector3.right;
            for (int i = 0; i < 2; i++)
            {
                Transform foot = parts[Foot + i].transform;
                float across = Vector3.Dot(Flat(acts - sole[i]), beside);
                float outward = i == 0 ? Mathf.Min(across, 0) : Mathf.Max(across, 0);
                Vector3 local = foot.InverseTransformPoint(acts - beside * (across - outward));
                Vector3 half = .5f * soleSize[i];
                local = new Vector3(Mathf.Clamp(local.x, -half.x + Edge, half.x - Edge), -half.y, Mathf.Clamp(local.z, -half.z + Edge, half.z - Edge));
                at[i] = foot.TransformPoint(local);
            }
            float share = Mathf.Clamp01(Vector3.Dot(Flat(acts - sole[0]), over) / Mathf.Max(1e-6f, over.sqrMagnitude));
            acted = Vector3.Lerp(at[0], at[1], share);
            Outside = Flat(acts - acted).magnitude;
            // The push can only act where the soles are: it pushes sideways only as a push from there, through its
            // weight, does.
            Vector3 sideways = Flat(weight - acted) * ((g + rise) / Mathf.Max(.1f, high));
            Vector3 push = whole * (sideways + Vector3.up * (g + rise));

            // It leans against what it feels, for what it cannot take standing as it is, keeping sole behind its weight.
            {
                Vector3 asked = felt / (falls * falls);
                Vector3 leansTo = rest;
                if (asked.magnitude > Comfort)
                {
                    Vector3 way = -asked.normalized;
                    float reach = 0;
                    for (int i = 0; i < 2; i++)
                        for (int c = 0; c < 4; c++)
                        {
                            Vector3 corner = parts[Foot + i].transform.TransformPoint(new Vector3((c & 1) == 0 ? -.5f * soleSize[i].x : .5f * soleSize[i].x, -.5f * soleSize[i].y, (c & 2) == 0 ? -.5f * soleSize[i].z : .5f * soleSize[i].z));
                            reach = Mathf.Max(reach, Vector3.Dot(Flat(corner) - rest, way));
                        }
                    leansTo = rest + way * Mathf.Min(LeansBy * (asked.magnitude - Comfort), Mathf.Max(0, reach - KeepsBehind));
                }
                holds = Vector3.Lerp(holds, leansTo, Mathf.Clamp01(Leans * step));
            }
            wentBefore = goes; weightBefore = weight; actedBefore = acted; hasBefore = true;

            // What each leg's joints give to make its boot push the ground so.
            for (int j = 0; j < Count; j++) torque[j] = Vector3.zero;
            for (int i = 0; i < 2; i++)
            {
                Vector3 pushes = (i == 0 ? 1 - share : share) * push;
                // (What it has learnt of the ground's push falling short moves where it means the push to act, but
                // never off the sole: a push asked for beyond a sole's edge only tips the boot.)
                Transform foot = parts[Foot + i].transform;
                Vector3 means = foot.InverseTransformPoint(at[i] + shortBy), half = .5f * soleSize[i];
                means = foot.TransformPoint(new Vector3(Mathf.Clamp(means.x, -half.x + Edge, half.x - Edge), -half.y, Mathf.Clamp(means.z, -half.z + Edge, half.z - Edge)));
                for (int j = Thigh + i; j <= Foot + i; j += 2)
                    torque[j] -= Vector3.Cross(means - JointAt(j), pushes);
            }
            // Its hips keep its trunk as upright as it began (rolled, as it favours a leg).
            {
                Quaternion off = Roll * upright * Quaternion.Inverse(parts[Hips].transform.rotation);
                off.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180) angle -= 360;
                if (float.IsNaN(axis.x) || float.IsInfinity(axis.x)) { axis = Vector3.zero; angle = 0; }
                float spring = Rights * whole;
                Vector3 rights = spring * (angle * Mathf.Deg2Rad) * axis - RightsDamped * spring * parts[Hips].angularVelocity;
                torque[Thigh] -= (1 - share) * rights;
                torque[Thigh + 1] -= share * rights;
            }
            for (int j = Thigh; j < Count; j++)
            {
                // (A hinge gives only about its own line; what is across it is borne by the joint itself.)
                if (hinge[j]) { Vector3 line = parts[j].transform.right; torque[j] = line * Vector3.Dot(line, torque[j]); }
                torque[j] = Vector3.ClampMagnitude(torque[j], strength[j]);
                if (float.IsNaN(torque[j].x) || float.IsNaN(torque[j].y) || float.IsNaN(torque[j].z)) torque[j] = Vector3.zero;
                gave[j] = torque[j].magnitude;
                // Equal and opposite, on the two parts the joint joins: nothing pushes the body from nowhere.
                parts[j].AddTorque(torque[j]);
                parts[from[j]].AddTorque(-torque[j]);
            }
            return true;
        }
    }
}
