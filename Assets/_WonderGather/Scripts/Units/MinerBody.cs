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
    public sealed class MinerBody : MonoBehaviour, IHandHolds
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
            // Optional: made to be taken off its hook by a hand (a lantern's bail, a mug's handle). The bar the
            // fingers close round: its direction in the bone's own space, and its radius. How far the thing reaches
            // below the place it hangs from, and to either side of the line it hangs along, square to its bar
            // (metres). No radius: it is not made to be taken.
            public Vector3 bar;
            public float grip, deep, wide;
            // Carried in a hand at the body's side: how far from the pelvis, to that side, the body stops its weight's
            // middle (the clothes' reach there, and the thing's own width).
            public float clear;
        }

        // A coat's or smock's skirt hangs from the hips in four flaps (front and back of each leg), each on a bone
        // of its own at its hip. A flap hangs by its own weight: straight down, whichever way the hips are turned
        // over it. Its thigh stops it: a front flap lies on the thigh that comes up under it (and slides down its
        // slope), a back flap is pushed back by a thigh that swings back; and both go out with a thigh that goes
        // out. When the thigh leaves it, it falls back a little late, as cloth does. So the leg never comes through
        // the cloth, and the cloth is never dragged after a leg that has left it.
        // (Until October 8 a flap hung along the pelvis and was turned by two angles of the thigh. Bowed, the back
        // of a coat stood out behind like a tail; squatting, the angles ran away as the thigh came level, and the
        // flaps stood out like wings. Luis: the clothes "start exploding and moving in very unnatural ways".)
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
            // The way the fingers leave the wrist, in the hand bone's space (square to the axis).
            public Vector3 along;
        }

        // How far a relaxed wrist turns with a swinging weight, in degrees.
        private const float WristGive = 32;
        // How long a thing takes to pass from its hook to the fingers that have closed on it, and back (seconds).
        private const float TakeTime = .12f;
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
        // What a tool has put in each hand this frame, and how far the hand has turned onto it.
        private readonly bool[] onHandle = new bool[2];
        private readonly Vector3[] handleGrip = new Vector3[2], handleWay = new Vector3[2], handleShoulder = new Vector3[2];
        private readonly Quaternion[] handleTurn = { Quaternion.identity, Quaternion.identity };
        private readonly float[] handleWeight = new float[2];
        // How far each modelled shoulder is from where the body solved it, in the solved chest's frame. The model's
        // trunk keeps its own bones and its own bearing, so its shoulders are near the solved ones, not on them
        // (Long's are 9 cm behind). A walking arm does not care; a hand that must lie on a handle does.
        private readonly Vector3[] shoulderOff = new Vector3[2];
        // The pickaxe made for this body's arms and hands.
        [SerializeField] private ToolDefinition pickaxe;
        public ToolDefinition Pickaxe => pickaxe;
        public void ConfigureTool(ToolDefinition tool) => pickaxe = tool;
        // How far a thigh swings, in degrees, before it reaches its front and its back flap (x, y): the room between
        // the leg and the cloth as they were modelled. A long coat hangs well clear of the legs, so only the end of
        // each stride moves it; cloth that is not touched hangs still.
        [SerializeField] private Vector2 flapSlack = Vector2.zero;
        private Quaternion[] flapRest = new Quaternion[0];
        // Which way each flap hangs now, in the world; which way of its bone points down it, as modelled; and how
        // far inside its thigh's outer side it hangs as modelled. The front of each thigh, in its bone's frame.
        private Vector3[] flapHangs = new Vector3[0], flapDown = new Vector3[0];
        private float[] flapInside = new float[0];
        private readonly Vector3[] thighFront = new Vector3[2];
        private float ankleUp, flapLong;
        private Vector3 pelvisForward, pelvisUp, pelvisAcross;
        private Vector3[] hangOffset = new Vector3[0], hangSpeed = new Vector3[0], hangFrom = new Vector3[0], hangShown = new Vector3[0];
        private Quaternion[] hangRest = new Quaternion[0];
        private Vector3[] hangWay = new Vector3[0];
        private float[] hangInto = new float[0], hangAskew = new float[0];
        private bool[] hangReady = new bool[0];
        // A hung thing taken off its hook: the hand that has it (-1: it hangs where it was hung), the hand that had it
        // last, how far it has passed from the hook to the hand, and which way round its bar lies in the fingers.
        private int[] hangHand = new int[0], hangLast = new int[0];
        private float[] hangTaken = new float[0], hangTurned = new float[0], hangClear = new float[0];
        // Where its hook is (the place it hangs from when nothing has taken it) and how its bar lies there; where the
        // hook was modelled, in the pelvis' space; and how its bar lies now.
        private Vector3[] hangHook = new Vector3[0], hangHookBar = new Vector3[0], hangHome = new Vector3[0], hangBar = new Vector3[0];
        // The thing being hung now is in a hand at this side of the body (-1 the left, 1 the right; 0: on its hook).
        private float stopSide;
        private bool ready;
        // Rest: each bone's rotation relative to the being's root, and for limbs the rest aim of the bone, in root space.
        private Quaternion pelvisRest, spineRest, chestRest, neckRest, headRest;
        private readonly Quaternion[] upperRest = new Quaternion[2], foreRest = new Quaternion[2], handRest = new Quaternion[2];
        private readonly Quaternion[] thighRest = new Quaternion[2], shinRest = new Quaternion[2], footRest = new Quaternion[2], toeRest = new Quaternion[2];
        private readonly Vector3[] upperAim = new Vector3[2], foreAim = new Vector3[2], thighAim = new Vector3[2], shinAim = new Vector3[2];
        private Vector3 pelvisOffset;
        // Breath: its chest is drawn fuller by this share (0: as modelled). What is on its chest (its neck, its
        // arms, what hangs there) is moved by that, and is not made larger.
        public float Swell { get; set; }
        private Vector3 chestSize;
        private float drawnFuller = 1;
        private Transform[] onChest;
        private Vector3[] onChestSize;

        public bool Ready => ready;
        public Bones Rig => bones;
        // The posed body's own segments, which the bones are turned to.
        public Solution Solved => solved;

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
        public void ConfigureThings(Hanging[] things) { hanging = things ?? new Hanging[0]; ready = false; CaptureRest(); }

        // Whether a hung thing is made to be taken in a hand.
        public bool Takes(int index) => index >= 0 && index < hanging.Length && hanging[index].grip > 0 && hanging[index].bone != null
                                        && hanging[index].bar != Vector3.zero;
        // Where its hook is now (it goes with the cloth it is sewn to), and how the thing's bar lies on it.
        public Vector3 Hook(int index) => hangHook[index];
        public Vector3 HookBar(int index) => hangHookBar[index];
        // Where its hook was modelled, as the hips are now: the cloth's own movement left out.
        public Vector3 HookHome(int index) => bones.pelvis.position + bones.pelvis.rotation * hangHome[index];
        // The way out from the body where it hangs (level), and how far out from its hook, that way, its handle must be
        // held for it to hang straight down, clear of the body.
        public Vector3 HookOut(int index) => bones.pelvis.rotation * hanging[index].stopNormal;
        public float HookClear(int index) => hangClear[index];
        // The place it hangs from now (its handle), and how its bar lies now.
        public Vector3 HangsFrom(int index) => hanging[index].bone.position;
        public Vector3 HangingBar(int index) => hangBar[index];
        // The hand that has it (0 the left, 1 the right), or -1: it hangs where it was hung. How far it has passed
        // into that hand (1: it hangs from the closed fingers).
        public int InHand(int index) => index >= 0 && index < hangHand.Length ? hangHand[index] : -1;
        public float Taken(int index) => hangTaken[index];
        public bool Carries(int hand)
        {
            for (int k = 0; k < hangHand.Length; k++) if (hangHand[k] == hand) return true;
            return false;
        }
        // A hand has closed on its handle and has it; or (-1) it is back on its hook, and the hand may let go. The arm
        // that brings the hand there is not this body's business (ThingsInHand).
        public void Carry(int index, int hand)
        {
            if (!Takes(index) || hand > 1 || (hand >= 0 && !CanHold(hand))) return;
            if (hand >= 0)
            {
                hangLast[index] = hand;
                Vector3 fingers = bones.hands[hand].TransformDirection(grips[hand].axis);
                hangTurned[index] = Vector3.Dot(fingers, hangBar[index]) < 0 ? -1 : 1;
            }
            hangHand[index] = hand;
        }
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

        // IHandHolds: a hand that closes is free to hold a tool; one that carries something is not.
        public bool HandFree(int hand) => CanHold(hand) && !Carries(hand);

        // How the hand's bone must be turned, in the world, to hold a handle: the handle runs through the closed
        // fingers with the tool's head on the thumb's side, and the fingers leave the wrist as nearly as may be the
        // way the arm reaches.
        private Quaternion HoldTurn(int hand, Vector3 grip, Vector3 handle, Vector3 shoulder)
        {
            Vector3 reach = Vector3.ProjectOnPlane(grip - shoulder, handle);
            if (reach.sqrMagnitude < 1e-8f) reach = Vector3.ProjectOnPlane(transform.forward, handle);
            if (reach.sqrMagnitude < 1e-8f) reach = Vector3.ProjectOnPlane(transform.up, handle);
            return Quaternion.LookRotation(handle, reach) * Quaternion.Inverse(Quaternion.LookRotation(grips[hand].axis, grips[hand].along));
        }

        public Vector3 WristFor(int hand, Vector3 grip, Vector3 handle, float radius, Vector3 shoulder)
        {
            if (!CanHold(hand) || handle.sqrMagnitude < 1e-8f) return grip;
            var held_ = grips[hand];
            Row(held_, radius, out int row, out int next, out float blend);
            Vector3 centre = Vector3.Scale(Vector3.Lerp(held_.centres[row], held_.centres[next], blend), bones.hands[hand].lossyScale);
            // The body will solve its arm from its own shoulder; the model's arm is that arm, moved to the model's
            // shoulder. So the body is asked for the wrist less that much, and the model's wrist lands on the place.
            Vector3 off = solved.torso != null ? solved.torso.rotation * shoulderOff[hand] : Vector3.zero;
            return grip - HoldTurn(hand, grip, handle, shoulder + off) * centre - off;
        }

        public void HoldHandle(int hand, bool on, Vector3 grip, Vector3 handle, float radius, Vector3 shoulder)
        {
            if (hand < 0 || hand > 1) return;
            on &= CanHold(hand);
            if (on) Hold(hand, radius);
            else if (onHandle[hand]) Release(hand);
            onHandle[hand] = on;
            handleGrip[hand] = grip;
            handleWay[hand] = handle;
            handleShoulder[hand] = shoulder;
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
            if (onChest == null)
            {
                chestSize = bones.chest.localScale;
                onChest = new Transform[bones.chest.childCount];
                onChestSize = new Vector3[onChest.Length];
                for (int k = 0; k < onChest.Length; k++) { onChest[k] = bones.chest.GetChild(k); onChestSize[k] = onChest[k].localScale; }
            }
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
            hangHand = new int[count];
            hangLast = new int[count];
            hangTaken = new float[count];
            hangTurned = new float[count];
            hangClear = new float[count];
            hangHook = new Vector3[count];
            hangHookBar = new Vector3[count];
            hangHome = new Vector3[count];
            hangBar = new Vector3[count];
            for (int k = 0; k < count; k++)
            {
                var thing = hanging[k];
                hangHand[k] = hangLast[k] = -1;
                hangTurned[k] = 1;
                if (thing.bone == null) continue;
                hangRest[k] = thing.bone.localRotation;
                hangHook[k] = thing.bone.position;
                hangHookBar[k] = hangBar[k] = thing.bone.rotation * thing.bar;
                hangHome[k] = Quaternion.Inverse(bones.pelvis.rotation) * (thing.bone.position - bones.pelvis.position);
                // Hanging straight down from its hook, its weight would be this far inside where the body stops it.
                hangClear[k] = thing.stopDistance > 0 ? thing.stopDistance - Vector3.Dot(hangHome[k], thing.stopNormal) : 0;
            }
            // The skirt's flaps as modelled, and the being's own directions in the pelvis' frame (the legs' swing is
            // measured there, so the hips' sway and roll do not count as a stride).
            Quaternion toPelvis = Quaternion.Inverse(bones.pelvis.rotation);
            pelvisForward = toPelvis * transform.forward;
            pelvisUp = toPelvis * transform.up;
            pelvisAcross = toPelvis * transform.right;
            int flapCount = flaps != null ? flaps.Length : 0;
            flapRest = new Quaternion[flapCount];
            flapHangs = new Vector3[flapCount];
            flapDown = new Vector3[flapCount];
            flapInside = new float[flapCount];
            for (int i = 0; i < 2; i++) thighFront[i] = Quaternion.Inverse(bones.thighs[i].rotation) * transform.forward;
            for (int k = 0; k < flapCount; k++)
            {
                flapRest[k] = flaps[k].bone != null ? flaps[k].bone.localRotation : Quaternion.identity;
                flapHangs[k] = -transform.up;
                flapDown[k] = flaps[k].bone != null ? Quaternion.Inverse(flaps[k].bone.rotation) * -transform.up : Vector3.down;
                flapInside[k] = Mathf.Min(0, Vector3.Dot(-transform.up, ThighOut(flaps[k].leg)));
            }
            // How high an ankle stands over the ground, and how long a flap is taken to be (most of a thigh).
            ankleUp = Mathf.Max(0, Mathf.Min(bones.feet[0].position.y, bones.feet[1].position.y) - transform.position.y);
            flapLong = .7f * Vector3.Distance(bones.thighs[0].position, bones.shins[0].position);
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
            // (Written only when breath changes it: a body that does not breathe so is not touched.)
            float fuller = 1 + Mathf.Clamp(Swell, 0, .16f);
            if (onChest != null && fuller != drawnFuller)
            {
                drawnFuller = fuller;
                bones.chest.localScale = chestSize * fuller;
                for (int k = 0; k < onChest.Length; k++) if (onChest[k] != null) onChest[k].localScale = onChestSize[k] / fuller;
            }
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
                // The hand carries on from the forearm. Holding a tool, it turns to lie on the handle (as it opens to
                // take it), and turns back when it lets go.
                Quaternion follows = fore * root * handRest[i];
                shoulderOff[i] = Quaternion.Inverse(solved.torso.rotation) * (bones.upperArms[i].position - shoulder);
                if (onHandle[i]) handleTurn[i] = HoldTurn(i, handleGrip[i], handleWay[i], bones.upperArms[i].position);
                handleWeight[i] = Mathf.MoveTowards(handleWeight[i], onHandle[i] ? 1 : 0, Mathf.Clamp(Time.deltaTime, 0, 1 / 20f) / (GripTime * GripOpening));
                bones.hands[i].rotation = handleWeight[i] > 0 ? Quaternion.Slerp(follows, handleTurn[i], Mathf.SmoothStep(0, 1, handleWeight[i])) : follows;
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

        // A thigh's line from hip to knee, and its outer side (square to that line), in the world.
        private Vector3 ThighAlong(int leg) => (bones.shins[leg].position - bones.thighs[leg].position).normalized;
        private Vector3 ThighOut(int leg)
        {
            Vector3 along = ThighAlong(leg);
            Vector3 side = Vector3.ProjectOnPlane(bones.pelvis.rotation * pelvisAcross * (leg == 0 ? -1 : 1), along);
            return side.sqrMagnitude > 1e-6f ? side.normalized : bones.pelvis.rotation * pelvisAcross * (leg == 0 ? -1 : 1);
        }

        // A direction kept on one side of a face (its component along the face no less than `least`): turned onto
        // it, down the face's slope, if it is not; and where there is no slope (the face lies level), along `lies`.
        private static Vector3 Stopped(Vector3 hangs, Vector3 face, float least, Vector3 lies)
        {
            float into = Vector3.Dot(hangs, face);
            if (into >= least) return hangs;
            Vector3 slides = hangs - into * face;
            // (Never back up the limb it lies on.)
            float backUp = Vector3.Dot(slides, lies);
            if (backUp < 0) slides -= backUp * lies;
            slides = slides.sqrMagnitude > 1e-4f ? slides.normalized : lies;
            return (slides * Mathf.Sqrt(Mathf.Max(0, 1 - least * least)) + face * least).normalized;
        }

        // The skirt's flaps: hanging by their own weight, stopped at once by the thigh that moves into them and by
        // the ground, and falling back after the thigh leaves.
        private void Drape()
        {
            if (flaps.Length == 0) return;
            float dt = Mathf.Clamp(Time.deltaTime, 0, 1 / 20f);
            float falls = 1 - Mathf.Exp(-12 * dt);
            float ground = Mathf.Min(bones.feet[0].position.y, bones.feet[1].position.y) - ankleUp;
            Vector3 ahead = Vector3.ProjectOnPlane(bones.pelvis.rotation * pelvisForward, Vector3.up);
            ahead = ahead.sqrMagnitude > 1e-4f ? ahead.normalized : transform.forward;
            for (int k = 0; k < flaps.Length; k++)
            {
                var flap = flaps[k];
                if (flap.bone == null) continue;
                // By its own weight, a little late.
                Vector3 hangs = Vector3.Slerp(flapHangs[k], Vector3.down, falls);
                // Stopped by its thigh: by the thigh's front (a front flap) or its back. The thigh counts by how far
                // it comes forward or goes back under the flap, not by how far it goes out to the side: a knee that
                // opens outwards goes out from under the cloth, which then hangs between the knees. (Laid along a
                // thigh that pointed out to the side, a long coat's front stood out there as a board: the sceptic,
                // October 8, "a stiff sheet of coat standing out on Long's right side".)
                Vector3 across = bones.pelvis.rotation * pelvisAcross;
                Vector3 along = Vector3.ProjectOnPlane(ThighAlong(flap.leg), across);
                along = along.sqrMagnitude > 1e-4f ? along.normalized : Vector3.down;
                Vector3 front = Vector3.Cross(across, along);
                if (Vector3.Dot(front, bones.pelvis.rotation * pelvisForward) < 0) front = -front;
                float slack = (flap.front ? flapSlack.x : flapSlack.y) * Mathf.Deg2Rad;
                hangs = Stopped(hangs, flap.front ? front : -front, -Mathf.Sin(slack), along);
                // Stopped by the ground: its hem lies on it, out the way it hangs (or the way it is worn).
                flap.bone.localRotation = flapRest[k];
                float room = (flap.bone.position.y - ground) / Mathf.Max(.01f, flapLong);
                if (-hangs.y > room && room < 1)
                {
                    Vector3 level = Vector3.ProjectOnPlane(hangs, Vector3.up);
                    level = level.sqrMagnitude > 1e-4f ? level.normalized : flap.front ? ahead : -ahead;
                    float drops = Mathf.Clamp(room, -.2f, 1);
                    hangs = (level * Mathf.Sqrt(1 - drops * drops) + Vector3.down * drops).normalized;
                }
                flapHangs[k] = hangs;
                flap.bone.rotation = Quaternion.FromToRotation(flap.bone.rotation * flapDown[k], hangs) * flap.bone.rotation;
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
            // Its hook, and how its bar lies on it. Taken in a hand, the thing hangs from its handle's place in the
            // closed fingers instead: it passes from the one to the other as the fingers close on it, and back.
            hangHook[k] = thing.bone.position;
            hangHookBar[k] = thing.bone.rotation * thing.bar;
            int by = hangHand[k] >= 0 ? hangHand[k] : hangLast[k];
            hangTaken[k] = Mathf.MoveTowards(hangTaken[k], hangHand[k] >= 0 ? 1 : 0, dt / TakeTime);
            Vector3 fingers = Vector3.zero, inFingers = Vector3.zero;
            bool taken = hangTaken[k] > 0 && by >= 0 && HandleIn(by, thing.grip, out inFingers, out fingers);
            if (taken) thing.bone.position = Vector3.Lerp(hangHook[k], inFingers, Mathf.SmoothStep(0, 1, hangTaken[k]));
            stopSide = taken ? (by == 0 ? -1 : 1) : 0;
            // Once the fingers have it, it is a thing carried in a hand: the wrist gives with its swing (below).
            bool inHand = taken && hangTaken[k] >= 1;
            if (inHand) { thing.hand = bones.hands[by]; thing.handle = grips[by].axis * hangTurned[k]; }
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
                if (inHand && HandleIn(by, thing.grip, out inFingers, out fingers)) thing.bone.position = inFingers;
                pivot = thing.bone.position;
                Vector3 held = thing.hand.rotation * thing.handle;
                Vector3 hang = Vector3.ProjectOnPlane(way, held);
                shown = Settled(thing, pivot, pivot + (hang.sqrMagnitude > 1e-8f ? hang.normalized : Vector3.down) * thing.length, held, out _);
            }
            hangShown[k] = shown;
            hangWay[k] = shown - pivot;
            hangInto[k] = thing.stopDistance > 0 ? thing.stopDistance - Vector3.Dot(shown - bones.pelvis.position, bones.pelvis.rotation * thing.stopNormal) : 0;
            // In a hand, the body is where both its front and its side would stop the thing.
            if (stopSide != 0 && thing.clear > 0)
                hangInto[k] = Mathf.Min(hangInto[k], thing.clear - Vector3.Dot(shown - bones.pelvis.position, bones.pelvis.rotation * pelvisAcross * stopSide));
            hangAskew[k] = thing.hand != null ? Mathf.Abs(Vector3.Dot(hangWay[k].normalized, thing.hand.rotation * thing.handle)) : 0;
            thing.bone.rotation = Quaternion.FromToRotation(thing.bone.rotation * thing.aim, (shown - pivot).normalized) * thing.bone.rotation;
            if (taken)
            {
                // Its bar lies in the fingers: it turns, about the way it hangs, until it does.
                Vector3 way = (shown - pivot).normalized;
                Vector3 has = Vector3.ProjectOnPlane(thing.bone.rotation * thing.bar, way), wants = Vector3.ProjectOnPlane(fingers * hangTurned[k], way);
                if (has.sqrMagnitude > 1e-6f && wants.sqrMagnitude > 1e-6f)
                    thing.bone.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(has.normalized, wants.normalized),
                        Mathf.SmoothStep(0, 1, hangTaken[k])) * thing.bone.rotation;
            }
            hangBar[k] = thing.bone.rotation * thing.bar;
            stopSide = 0;
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
            // In a hand it may be at the body's side as well as where it hung. There the body's side stops it: the body
            // is where both would stop it, and it is pushed out the nearer way.
            Vector3 aside = stop > 0 && stopSide != 0 && thing.clear > 0 ? bones.pelvis.rotation * pelvisAcross * stopSide : Vector3.zero;
            against = Vector3.zero;
            for (int pass = 0; pass < 2; pass++)
            {
                if (stop > 0)
                {
                    float clear = Vector3.Dot(tip - bones.pelvis.position, normal);
                    if (aside != Vector3.zero)
                    {
                        float beside = Vector3.Dot(tip - bones.pelvis.position, aside);
                        if (clear < stop && beside < thing.clear)
                        {
                            if (stop - clear < thing.clear - beside) { tip += normal * (stop - clear); against = normal; }
                            else { tip += aside * (thing.clear - beside); against = aside; }
                        }
                    }
                    else if (clear < stop) { tip += normal * (stop - clear); against = normal; }
                }
                Vector3 hang = tip - pivot;
                if (hang.sqrMagnitude < 1e-8f) hang = Vector3.down;
                tip = pivot + Vector3.RotateTowards(Vector3.down, hang.normalized, thing.limit * Mathf.Deg2Rad, 0) * thing.length;
            }
            return tip;
        }
    }
}
