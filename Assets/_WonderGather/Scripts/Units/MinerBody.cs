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

        [SerializeField] private Bones bones;
        [SerializeField] private Solution solved;
        private bool ready;
        // Rest: each bone's rotation relative to the being's root, and for limbs the rest aim of the bone, in root space.
        private Quaternion pelvisRest, spineRest, chestRest, neckRest, headRest;
        private readonly Quaternion[] upperRest = new Quaternion[2], foreRest = new Quaternion[2], handRest = new Quaternion[2];
        private readonly Quaternion[] thighRest = new Quaternion[2], shinRest = new Quaternion[2], footRest = new Quaternion[2], toeRest = new Quaternion[2];
        private readonly Vector3[] upperAim = new Vector3[2], foreAim = new Vector3[2], thighAim = new Vector3[2], shinAim = new Vector3[2];
        private Vector3 pelvisOffset;

        public bool Ready => ready;
        public Bones Rig => bones;

        public void Configure(Bones rig, Solution solution)
        {
            bones = rig;
            solved = solution;
            ready = false;
            CaptureRest();
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
        }
    }
}
