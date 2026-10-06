using UnityEngine;

namespace WonderGather
{
    // S1d: a modelled being moved by the procedural body. ProceduralBiped solves its joints onto a set of
    // invisible segments (its solution); after it has posed, this turns the model's bones to match. Each bone
    // keeps its modelled length and rest shape and is turned by as much as its segment has turned from rest:
    // the pelvis, chest, head and feet follow their segments' whole frames; limbs aim at the solved joints and
    // roll so that knees and elbows bend the way the body bends them.
    // Index 0 of every pair is the being's left, as in ProceduralBiped.
    [DefaultExecutionOrder(50)]
    public sealed class MinerBody : MonoBehaviour
    {
        [System.Serializable]
        public struct Bones
        {
            public Transform pelvis, spine, chest, neck, head;
            public Transform[] upperArms, forearms, hands, thighs, shins, feet, toes;
        }

        [System.Serializable]
        public struct Solution
        {
            public Transform pelvis, torso, head;
            public Transform[] upperArms, forearms, thighs, shins, feet, toes;
        }

        // A carried thing on a bone of its own (a lantern or a mug in a hand, a satchel on its strap): a real object
        // that hangs from where it is held and answers to gravity and to the body's movement.
        [System.Serializable]
        public struct Hanging
        {
            // The thing's bone; it hangs from the bone's own position (the handle in the fingers, the strap's rings).
            public Transform bone;
            // From where it hangs to the middle of its weight: how far, and which way in the bone's own space
            // (as it was modelled: a lantern straight down, a satchel leaning on the hip).
            public float length;
            public Vector3 aim;
            // How much of its swing it keeps from one sixtieth of a second to the next (a heavy bag on a hip less
            // than a lantern on a wire), and how far it may swing from hanging straight, in degrees.
            public float damping, limit;
            // The body stops it: its weight's middle stays at least stopDistance from the pelvis along stopNormal
            // (in the pelvis' own space). A lantern never swings into the coat; a bag rests against the hip.
            public Vector3 stopNormal;
            public float stopDistance;
            // Optional: the hand that carries it, and its handle's direction in that hand's space. The relaxed
            // wrist gives with the weight, so the handle stays in the closed fingers as the thing swings.
            public Transform hand;
            public Vector3 handle;
            // Optional, with no hand: it hangs in a loop with its head across the loop (a hammer), and swings only
            // square to that head. The handle is then the head's direction, in the space of the bone's parent.
            public bool hinged;
            // Optional: the limb under the cloth that the thing rests on (a thigh under a coat's skirt). As that
            // limb moves out, the cloth does, and pushes the thing out with it: a point on the limb (in its own
            // space), where that point is at rest (in the pelvis' space), and how much of its movement the cloth takes.
            public Transform pusher;
            public Vector3 pushPoint, pushRest;
            public float pushShare;
            // Optional: the cloth it is sewn to (a loop on an apron). Cloth moves with several bones at once, so
            // the thing hangs from where that cloth is now: each bone's own view of the place, by its share.
            public Transform[] riders;
            public Vector3[] ridePoints;
            public float[] rideShares;
        }

        // A coat's or smock's skirt hangs from the hips in four flaps (front and back of each leg), each on a bone
        // of its own at its hip. A flap is pushed by its thigh when the thigh moves into it (a front flap as the leg
        // swings forward, a back flap as it swings back), and hangs when the thigh moves away, falling back a
        // little late, as cloth does. So the leg never comes through the cloth, and the cloth is never dragged
        // after a leg that has left it.
        [System.Serializable]
        public struct Flap
        {
            public Transform bone;
            // The leg under it: 0 the left, 1 the right; and which side of the leg it hangs on.
            public int leg;
            public bool front;
        }

        // A free hand that closes: its fingers' bones (the four fingers, then the thumb; three each, the knuckle
        // first), and how each turns, in its own frame, from its rest to the open hand and to the hand closed round
        // handles of several thicknesses. The model's build finds those turns on the skinned mesh, so that the
        // fingers lie on the handle and never enter it (Art/Blender/Worker/hands.py); the game only plays them.
        [System.Serializable]
        public struct Grip
        {
            public Transform[] joints;
            // The open hand, ready to take a handle: one turn for each joint.
            public Quaternion[] open;
            // The handles the table holds (radius, metres, ascending), and for each, one turn for each joint.
            public float[] radii;
            public Quaternion[] closed;
            // Where each of those handles lies in the closed hand: a point on its axis, in the hand bone's space;
            // and its direction there (through the closed fingers, towards the thumb).
            public Vector3[] centres;
            public Vector3 axis;
        }

        // How far a relaxed wrist turns with a swinging weight, in degrees.
        private const float WristGive = 32;
        // How long a hand takes to open and close round a handle (seconds), and how much of that is the opening.
        private const float GripTime = .3f, GripOpening = .35f;

        [SerializeField] private Bones bones;
        [SerializeField] private Solution solved;
        [SerializeField] private Hanging[] hanging = new Hanging[0];
        [SerializeField] private Flap[] flaps = new Flap[0];
        // The hands that close: 0 the left, 1 the right. A hand modelled closed round what it carries has no joints.
        [SerializeField] private Grip[] grips = new Grip[0];
        private Quaternion[][] gripRest = new Quaternion[0][];
        private readonly float[] held = new float[2], heldTarget = new float[2], heldRadius = new float[2], heldPosed = { -1, -1 };
        // How far a thigh swings, in degrees, before it reaches its front and its back flap (x, y): the room between
        // the leg and the cloth as they were modelled. A long coat hangs well clear of the legs, so only the end of
        // each stride moves it; cloth that is not touched hangs still.
        [SerializeField] private Vector2 flapSlack = Vector2.zero;
        private Quaternion[] flapRest = new Quaternion[0];
        private float[] flapTurn = new float[0], flapOut = new float[0];
        private readonly float[] thighRestSwing = new float[2], thighRestOut = new float[2];
        private Vector3 pelvisForward, pelvisUp, pelvisAcross;
        private Vector3[] hangOffset = new Vector3[0], hangSpeed = new Vector3[0], hangFrom = new Vector3[0], hangShown = new Vector3[0];
        private Quaternion[] hangRest = new Quaternion[0];
        private Vector3[] hangWay = new Vector3[0];
        private float[] hangInto = new float[0], hangAskew = new float[0];
        private bool[] hangReady = new bool[0];
        private bool ready;
        // Rest: each bone's rotation relative to the being's root, and for limbs the rest aim of the bone, in root space.
        private Quaternion pelvisRest, spineRest, chestRest, neckRest, headRest;
        private readonly Quaternion[] upperRest = new Quaternion[2], foreRest = new Quaternion[2], handRest = new Quaternion[2];
        private readonly Quaternion[] thighRest = new Quaternion[2], shinRest = new Quaternion[2], footRest = new Quaternion[2], toeRest = new Quaternion[2];
        private readonly Vector3[] upperAim = new Vector3[2], foreAim = new Vector3[2], thighAim = new Vector3[2], shinAim = new Vector3[2];
        private Vector3 pelvisOffset;

        public bool Ready => ready;
        public Bones Rig => bones;

        public void Configure(Bones rig, Solution solution, Hanging[] things = null, Flap[] skirt = null, Vector2? skirtSlack = null, Grip[] fingers = null)
        {
            bones = rig;
            solved = solution;
            hanging = things ?? new Hanging[0];
            flaps = skirt ?? new Flap[0];
            flapSlack = skirtSlack ?? Vector2.zero;
            grips = fingers ?? new Grip[0];
            ready = false;
            CaptureRest();
        }

        public System.Collections.Generic.IReadOnlyList<Hanging> Things => hanging;
        // Where a hanging thing's weight is now.
        public Vector3 HangingWeight(int index) => hangShown[index];
        // As it was last posed, all read at the same moment (the being moves on before anyone can look): the way it
        // hangs from the place it hangs from; how far inside where the body stops it its weight is (negative: clear
        // of the body); and how far its handle is from square to the way it hangs (0: square).
        public Vector3 HangingWay(int index) => hangWay[index];
        public float HangingIntoBody(int index) => hangInto[index];
        public float HangingAskew(int index) => hangAskew[index];
        public System.Collections.Generic.IReadOnlyList<Flap> Skirt => flaps;

        // Whether a hand can close round a handle (a hand that carries something is modelled closed, and cannot).
        public bool CanHold(int hand) => hand >= 0 && hand < grips.Length && hand < 2 && grips[hand].joints != null && grips[hand].joints.Length > 0
                                         && grips[hand].radii != null && grips[hand].radii.Length > 0;
        // Close a hand round a handle of this radius: it opens first, as a hand does to take something. Or let go.
        public void Hold(int hand, float radius)
        {
            if (!CanHold(hand)) return;
            heldRadius[hand] = radius;
            heldTarget[hand] = 1;
        }
        public void Release(int hand) { if (hand >= 0 && hand < 2) heldTarget[hand] = 0; }
        // 0: relaxed, as modelled. 1: closed round its handle.
        public float Held(int hand) => hand >= 0 && hand < 2 ? held[hand] : 0;
        public Grip Fingers(int hand) => grips[hand];
        // Where a handle of this radius lies in a hand, as the hand is now: a point on its axis, and its direction.
        public bool HandleIn(int hand, float radius, out Vector3 point, out Vector3 direction)
        {
            point = direction = Vector3.zero;
            if (!CanHold(hand)) return false;
            var grip = grips[hand];
            Row(grip, radius, out int row, out int next, out float blend);
            point = bones.hands[hand].TransformPoint(Vector3.Lerp(grip.centres[row], grip.centres[next], blend));
            direction = bones.hands[hand].TransformDirection(grip.axis).normalized;
            return true;
        }

        // The table's two rows a radius lies between, and how far from the first to the second.
        private static void Row(Grip grip, float radius, out int row, out int next, out float blend)
        {
            row = 0;
            for (int k = 0; k + 1 < grip.radii.Length; k++) if (radius >= grip.radii[k]) row = k;
            next = Mathf.Min(row + 1, grip.radii.Length - 1);
            blend = next > row ? Mathf.InverseLerp(grip.radii[row], grip.radii[next], radius) : 0;
        }

        private void Awake() => CaptureRest();

        private static bool Pair(Transform[] values) => values != null && values.Length == 2 && values[0] != null && values[1] != null;

        private bool Valid => bones.pelvis != null && bones.spine != null && bones.chest != null && bones.neck != null && bones.head != null
                              && Pair(bones.upperArms) && Pair(bones.forearms) && Pair(bones.hands) && Pair(bones.thighs) && Pair(bones.shins) && Pair(bones.feet)
                              && solved.pelvis != null && solved.torso != null && solved.head != null && Pair(solved.upperArms) && Pair(solved.forearms)
                              && Pair(solved.thighs) && Pair(solved.shins) && Pair(solved.feet);

        // The bones are in their rest pose when the being is made; remember it.
        private void CaptureRest()
        {
            if (!Valid) return;
            Quaternion toRoot = Quaternion.Inverse(transform.rotation);
            Quaternion Rest(Transform bone) => toRoot * bone.rotation;
            Vector3 Aim(Transform from, Transform to) => toRoot * (to.position - from.position).normalized;
            pelvisRest = Rest(bones.pelvis);
            spineRest = Rest(bones.spine);
            chestRest = Rest(bones.chest);
            neckRest = Rest(bones.neck);
            headRest = Rest(bones.head);
            for (int i = 0; i < 2; i++)
            {
                upperRest[i] = Rest(bones.upperArms[i]);
                foreRest[i] = Rest(bones.forearms[i]);
                handRest[i] = Rest(bones.hands[i]);
                thighRest[i] = Rest(bones.thighs[i]);
                shinRest[i] = Rest(bones.shins[i]);
                footRest[i] = Rest(bones.feet[i]);
                toeRest[i] = Pair(bones.toes) ? Rest(bones.toes[i]) : Quaternion.identity;
                upperAim[i] = Aim(bones.upperArms[i], bones.forearms[i]);
                foreAim[i] = Aim(bones.forearms[i], bones.hands[i]);
                thighAim[i] = Aim(bones.thighs[i], bones.shins[i]);
                shinAim[i] = Aim(bones.shins[i], bones.feet[i]);
            }
            // The pelvis bone relative to the middle of the hip joints, which is where the body solves the hips.
            Vector3 hips = (bones.thighs[0].position + bones.thighs[1].position) * .5f;
            pelvisOffset = toRoot * (bones.pelvis.position - hips);
            // Each hanging thing's bone as it was modelled, in its parent's frame: it is turned from there each frame.
            int count = hanging != null ? hanging.Length : 0;
            hangOffset = new Vector3[count];
            hangSpeed = new Vector3[count];
            hangReady = new bool[count];
            hangFrom = new Vector3[count];
            hangShown = new Vector3[count];
            hangWay = new Vector3[count];
            hangInto = new float[count];
            hangAskew = new float[count];
            hangRest = new Quaternion[count];
            for (int k = 0; k < count; k++) hangRest[k] = hanging[k].bone != null ? hanging[k].bone.localRotation : Quaternion.identity;
            // The skirt's flaps as modelled, and the being's own directions in the pelvis' frame (the legs' swing is
            // measured there, so the hips' sway and roll do not count as a stride).
            Quaternion toPelvis = Quaternion.Inverse(bones.pelvis.rotation);
            pelvisForward = toPelvis * transform.forward;
            pelvisUp = toPelvis * transform.up;
            pelvisAcross = toPelvis * transform.right;
            int flapCount = flaps != null ? flaps.Length : 0;
            flapRest = new Quaternion[flapCount];
            flapTurn = new float[flapCount];
            flapOut = new float[flapCount];
            for (int k = 0; k < flapCount; k++) flapRest[k] = flaps[k].bone != null ? flaps[k].bone.localRotation : Quaternion.identity;
            for (int i = 0; i < 2; i++) { thighRestSwing[i] = Swing(i); thighRestOut[i] = Out(i); }
            // The fingers as modelled: each is turned from there.
            gripRest = new Quaternion[grips != null ? grips.Length : 0][];
            for (int h = 0; h < gripRest.Length; h++)
            {
                var joints = grips[h].joints ?? new Transform[0];
                gripRest[h] = new Quaternion[joints.Length];
                for (int j = 0; j < joints.Length; j++) gripRest[h][j] = joints[j] != null ? joints[j].localRotation : Quaternion.identity;
            }
            heldPosed[0] = heldPosed[1] = -1;
            ready = true;
        }

        private static void Ends(Transform segment, out Vector3 from, out Vector3 to)
        {
            // ProceduralBiped places a segment at the middle of its joints, turned from up and half its length tall.
            Vector3 half = segment.rotation * Vector3.up * segment.localScale.y;
            from = segment.position - half;
            to = segment.position + half;
        }

        // The rotation taking a rest aim and reference to a current aim and reference.
        private static Quaternion Turn(Vector3 restAim, Vector3 restRef, Vector3 aim, Vector3 reference)
        {
            if (aim.sqrMagnitude < 1e-8f || restAim.sqrMagnitude < 1e-8f) return Quaternion.identity;
            return Quaternion.LookRotation(aim, reference) * Quaternion.Inverse(Quaternion.LookRotation(restAim, restRef));
        }

        // Which way a joint bends: from the middle of the limb towards the joint, steadied by a fallback when straight.
        private static Vector3 Bend(Vector3 from, Vector3 joint, Vector3 to, Vector3 fallback)
        {
            Vector3 axis = (to - from).normalized;
            Vector3 offset = Vector3.ProjectOnPlane(joint - (from + to) * .5f, axis);
            return Vector3.ProjectOnPlane(offset + Vector3.ProjectOnPlane(fallback, axis) * .02f, axis);
        }

        private void LateUpdate()
        {
            if (!ready) { CaptureRest(); if (!ready) return; }
            Quaternion root = transform.rotation;
            Vector3 forward = root * Vector3.forward;

            // Trunk: the pelvis and chest follow the solved frames; the spine and neck share the turn between them.
            Quaternion hipFrame = solved.pelvis.rotation * Quaternion.Inverse(root);
            Quaternion chestFrame = solved.torso.rotation * Quaternion.Inverse(root);
            Quaternion headFrame = solved.head.rotation * Quaternion.Inverse(root);
            bones.pelvis.SetPositionAndRotation(solved.pelvis.position + solved.pelvis.rotation * pelvisOffset, hipFrame * root * pelvisRest);
            bones.spine.rotation = Quaternion.Slerp(hipFrame, chestFrame, .5f) * root * spineRest;
            bones.chest.rotation = chestFrame * root * chestRest;
            bones.neck.rotation = Quaternion.Slerp(chestFrame, headFrame, .5f) * root * neckRest;
            bones.head.rotation = headFrame * root * headRest;

            for (int i = 0; i < 2; i++)
            {
                // Legs: knees bend forward.
                Ends(solved.thighs[i], out var hip, out var knee);
                Ends(solved.shins[i], out _, out var ankle);
                Vector3 kneeBend = Bend(hip, knee, ankle, solved.pelvis.forward);
                Vector3 restForward = root * Vector3.forward;
                Quaternion thigh = Turn(root * thighAim[i], restForward, knee - hip, kneeBend);
                bones.thighs[i].rotation = thigh * root * thighRest[i];
                Quaternion shin = Turn(root * shinAim[i], restForward, ankle - knee, kneeBend);
                bones.shins[i].rotation = shin * root * shinRest[i];
                // Feet and toes follow their soles, heel strike and toe-off included.
                bones.feet[i].rotation = solved.feet[i].rotation * Quaternion.Inverse(root) * root * footRest[i];
                if (Pair(bones.toes))
                    bones.toes[i].rotation = (Pair(solved.toes) ? solved.toes[i].rotation : solved.feet[i].rotation) * toeRest[i];

                // Arms: elbows bend back.
                Ends(solved.upperArms[i], out var shoulder, out var elbow);
                Ends(solved.forearms[i], out _, out var wrist);
                Vector3 back = -solved.torso.forward;
                Vector3 elbowBend = Bend(shoulder, elbow, wrist, back);
                Vector3 restBack = -forward;
                Quaternion upper = Turn(root * upperAim[i], restBack, elbow - shoulder, elbowBend);
                bones.upperArms[i].rotation = upper * root * upperRest[i];
                Quaternion fore = Turn(root * foreAim[i], restBack, wrist - elbow, elbowBend);
                bones.forearms[i].rotation = fore * root * foreRest[i];
                // The hand carries on from the forearm.
                bones.hands[i].rotation = fore * root * handRest[i];
            }
            for (int i = 0; i < 2; i++) Close(i);
            Drape();
            for (int k = 0; k < hanging.Length; k++) Hang(k);
        }

        // A hand closing round a handle or letting go: from relaxed, it opens (the fingers straighten, the thumb
        // lifts clear), then closes until the fingers lie on the handle.
        private void Close(int hand)
        {
            if (!CanHold(hand) || hand >= gripRest.Length) return;
            held[hand] = Mathf.MoveTowards(held[hand], heldTarget[hand], Mathf.Clamp(Time.deltaTime, 0, 1 / 20f) / GripTime);
            // A hand at rest costs nothing: its fingers are where they were put.
            if (Mathf.Approximately(held[hand], heldPosed[hand]) && (held[hand] <= 0 || held[hand] >= 1)) return;
            heldPosed[hand] = held[hand];
            var grip = grips[hand];
            var rest = gripRest[hand];
            float opening = Mathf.SmoothStep(0, 1, Mathf.Clamp01(held[hand] / GripOpening));
            float closing = Mathf.SmoothStep(0, 1, Mathf.Clamp01((held[hand] - GripOpening) / (1 - GripOpening)));
            Row(grip, heldRadius[hand], out int row, out int next, out float blend);
            int count = grip.joints.Length;
            for (int j = 0; j < count; j++)
            {
                if (grip.joints[j] == null) continue;
                Quaternion turn = Quaternion.Slerp(Quaternion.identity, grip.open[j], opening);
                if (closing > 0)
                    turn = Quaternion.Slerp(turn, Quaternion.Slerp(grip.closed[row * count + j], grip.closed[next * count + j], blend), closing);
                grip.joints[j].localRotation = rest[j] * turn;
            }
        }

        // How far forward a thigh points, as an angle from straight down in the pelvis' own frame (degrees).
        private float Swing(int leg)
        {
            Vector3 thigh = Quaternion.Inverse(bones.pelvis.rotation) * (bones.shins[leg].position - bones.thighs[leg].position);
            return Mathf.Atan2(Vector3.Dot(thigh, pelvisForward), -Vector3.Dot(thigh, pelvisUp)) * Mathf.Rad2Deg;
        }

        // How far out to its own side a thigh points, as an angle from straight down in the pelvis' frame (degrees).
        private float Out(int leg)
        {
            Vector3 thigh = Quaternion.Inverse(bones.pelvis.rotation) * (bones.shins[leg].position - bones.thighs[leg].position);
            return Mathf.Atan2(Vector3.Dot(thigh, pelvisAcross) * (leg == 0 ? -1 : 1), -Vector3.Dot(thigh, pelvisUp)) * Mathf.Rad2Deg;
        }

        // The skirt's flaps: pushed at once by the thigh that moves into them, falling back after it leaves.
        private void Drape()
        {
            if (flaps.Length == 0) return;
            float dt = Mathf.Clamp(Time.deltaTime, 0, 1 / 20f);
            Vector3 across = bones.pelvis.rotation * pelvisAcross, forward = bones.pelvis.rotation * pelvisForward;
            for (int k = 0; k < flaps.Length; k++)
            {
                var flap = flaps[k];
                if (flap.bone == null) continue;
                float swing = Swing(flap.leg) - thighRestSwing[flap.leg];
                float pushed = flap.front ? Mathf.Max(0, swing - flapSlack.x) : Mathf.Min(0, swing + flapSlack.y);
                // Pushed, the cloth goes with the leg at once; left, it falls back under its own weight.
                bool pushing = Mathf.Abs(pushed) > Mathf.Abs(flapTurn[k]);
                flapTurn[k] = pushing ? pushed : Mathf.Lerp(flapTurn[k], pushed, 1 - Mathf.Exp(-9 * dt));
                // A thigh that swings out to its side (in a turn, a sidestep) pushes both its flaps out.
                float outward = Mathf.Max(0, Out(flap.leg) - thighRestOut[flap.leg]);
                flapOut[k] = outward > flapOut[k] ? outward : Mathf.Lerp(flapOut[k], outward, 1 - Mathf.Exp(-9 * dt));
                flap.bone.localRotation = flapRest[k];
                // Forward is a turn about the being's right that carries down towards forward; out to the left is a
                // turn about forward that carries down towards the left.
                flap.bone.rotation = Quaternion.AngleAxis(flapOut[k] * (flap.leg == 0 ? -1 : 1), forward)
                                     * Quaternion.AngleAxis(-flapTurn[k], across) * flap.bone.rotation;
            }
        }

        // A hanging thing is a pendulum under the place it hangs from: pulled down by gravity, left behind as that
        // place moves, losing a little of its swing each moment, and stopped by the body, which it cannot enter.
        private void Hang(int k)
        {
            var thing = hanging[k];
            if (thing.bone == null) return;
            float dt = Mathf.Clamp(Time.deltaTime, 0, 1 / 20f);
            // From its modelled place under its parent, each frame (so no turn about its own axis ever builds up).
            thing.bone.localRotation = hangRest[k];
            if (thing.riders != null && thing.riders.Length > 0)
            {
                Vector3 place = Vector3.zero;
                float shares = 0;
                for (int r = 0; r < thing.riders.Length; r++)
                {
                    if (thing.riders[r] == null) continue;
                    place += thing.riders[r].TransformPoint(thing.ridePoints[r]) * thing.rideShares[r];
                    shares += thing.rideShares[r];
                }
                if (shares > 1e-4f) thing.bone.position = place / shares;
            }
            // Where the arm carries it. The pendulum hangs under this place, before the wrist gives: the give is the
            // hand answering the weight, and must not be fed back to the weight as if the arm had moved it.
            Vector3 pivot = thing.bone.position;
            if (!hangReady[k] || (pivot - hangFrom[k]).sqrMagnitude > 1)
            {
                // Just made, or moved far at once: it starts where it was modelled, at rest.
                hangOffset[k] = thing.bone.rotation * thing.aim * thing.length;
                hangSpeed[k] = Vector3.zero;
                hangFrom[k] = pivot;
                hangReady[k] = true;
            }
            // What hangs in a loop with its head across the loop swings only square to that head.
            Vector3 hinge = thing.hinged && thing.hand == null && thing.bone.parent != null ? thing.bone.parent.rotation * thing.handle : Vector3.zero;
            if (dt > 1e-6f)
            {
                // The weight is kept as where it is under the place it hangs from, and how fast it moves through the
                // world (small numbers, far from the world's middle too: at a very high frame rate gravity's pull in
                // one frame is smaller than a place in the world can be told apart).
                // Its own swing fades (a handle rubs in the hand, a strap on cloth); the movement it shares with the
                // place it hangs from does not: carried steadily along, it hangs straight, and it swings when that
                // place speeds up, slows or turns.
                Vector3 carried = (pivot - hangFrom[k]) / dt;
                Vector3 swing = (hangSpeed[k] - carried) * Mathf.Pow(Mathf.Clamp01(thing.damping), dt * 60) + Physics.gravity * dt;
                Vector3 offset = Settled(thing, pivot, pivot + hangOffset[k] + swing * dt, hinge, out var against) - pivot;
                swing = (offset - hangOffset[k]) / dt;
                // What the body stops rests against it: the push is not kept as swing, or it would bounce off the coat.
                if (against != Vector3.zero) swing -= against * Vector3.Dot(swing, against);
                hangSpeed[k] = carried + swing;
                hangOffset[k] = offset;
            }
            hangFrom[k] = pivot;
            Vector3 shown = pivot + hangOffset[k];
            if (thing.hand != null)
            {
                // The wrist gives with the weight: the hand turns so the handle stays square to the way the thing
                // hangs, as a relaxed hand does. The fingers stay closed on the handle; it is the wrist that bends.
                Vector3 way = hangOffset[k].normalized;
                Vector3 handle = thing.hand.rotation * thing.handle;
                Vector3 square = Vector3.ProjectOnPlane(handle, way);
                if (square.sqrMagnitude > 1e-6f)
                    thing.hand.rotation = Quaternion.RotateTowards(Quaternion.identity, Quaternion.FromToRotation(handle, square.normalized), WristGive) * thing.hand.rotation;
                // The hand has turned about the wrist, and what it holds with it: the thing is shown hanging the same
                // way from where the handle now is, square to it (past what a wrist can give, it swings no further
                // that way), and outside the body.
                pivot = thing.bone.position;
                Vector3 held = thing.hand.rotation * thing.handle;
                Vector3 hang = Vector3.ProjectOnPlane(way, held);
                shown = Settled(thing, pivot, pivot + (hang.sqrMagnitude > 1e-8f ? hang.normalized : Vector3.down) * thing.length, held, out _);
            }
            hangShown[k] = shown;
            hangWay[k] = shown - pivot;
            hangInto[k] = thing.stopDistance > 0 ? thing.stopDistance - Vector3.Dot(shown - bones.pelvis.position, bones.pelvis.rotation * thing.stopNormal) : 0;
            hangAskew[k] = thing.hand != null ? Mathf.Abs(Vector3.Dot(hangWay[k].normalized, thing.hand.rotation * thing.handle)) : 0;
            thing.bone.rotation = Quaternion.FromToRotation(thing.bone.rotation * thing.aim, (shown - pivot).normalized) * thing.bone.rotation;
        }

        // Where a hanging thing's weight comes to rest near a place: outside the body, within its swing and, given a
        // handle or a hinge, square to it. Each is put right in turn until all hold.
        private Vector3 Settled(Hanging thing, Vector3 pivot, Vector3 tip, Vector3 square, out Vector3 against)
        {
            against = Vector3.zero;
            for (int pass = 0; pass < 6; pass++)
            {
                Vector3 next = Held(thing, pivot, tip, out var stopped);
                if (stopped != Vector3.zero) against = stopped;
                if (square != Vector3.zero)
                {
                    Vector3 hang = Vector3.ProjectOnPlane(next - pivot, square);
                    if (hang.sqrMagnitude > 1e-8f) next = pivot + hang.normalized * thing.length;
                }
                bool settled = (next - tip).sqrMagnitude < 1e-10f;
                tip = next;
                if (settled) break;
            }
            return tip;
        }

        // Where a hanging thing's weight may be: at its length from where it hangs, no further from straight down
        // than it can swing, and outside the body, which pushes it out as the leg beneath the cloth moves.
        // How far from the pelvis, along its stop's way, the body keeps a thing's weight now: as modelled, and
        // further as the limb under the cloth it rests on pushes that cloth out.
        private float Allowed(Hanging thing)
        {
            float stop = thing.stopDistance;
            if (stop > 0 && thing.pusher != null)
                stop += Mathf.Max(0, Vector3.Dot(thing.pusher.TransformPoint(thing.pushPoint) - bones.pelvis.TransformPoint(thing.pushRest), bones.pelvis.rotation * thing.stopNormal)) * thing.pushShare;
            return stop;
        }

        private Vector3 Held(Hanging thing, Vector3 pivot, Vector3 tip, out Vector3 against)
        {
            float stop = Allowed(thing);
            Vector3 normal = stop > 0 ? bones.pelvis.rotation * thing.stopNormal : Vector3.zero;
            against = Vector3.zero;
            for (int pass = 0; pass < 2; pass++)
            {
                if (stop > 0)
                {
                    float clear = Vector3.Dot(tip - bones.pelvis.position, normal);
                    if (clear < stop) { tip += normal * (stop - clear); against = normal; }
                }
                Vector3 hang = tip - pivot;
                if (hang.sqrMagnitude < 1e-8f) hang = Vector3.down;
                tip = pivot + Vector3.RotateTowards(Vector3.down, hang.normalized, thing.limit * Mathf.Deg2Rad, 0) * thing.length;
            }
            return tip;
        }
    }
}
