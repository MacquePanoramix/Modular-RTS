using UnityEngine;

namespace WonderGather
{
    // What the ground does to a boot of a body of its own (OwnBody) in a step of the physics: how hard it bears it
    // up (newton seconds, summed over what touches it), and where (the middle of that bearing). Whoever reads it
    // clears it.
    public sealed class GroundTouch : MonoBehaviour
    {
        public Vector3 Where { get; private set; }
        public float Up { get; private set; }
        // (Getting up, only what does not move bears it: not another of its own parts, which strike each other then,
        // nor anything else that moves.)
        public bool StillOnly;
        // (For whoever looks at what else touches it: anything that moves.)
        public static System.Action<GroundTouch, Collider> Touched;

        public void Clear() { Where = Vector3.zero; Up = 0; }

        // (How many touches have been taken, ever: for whoever counts them.)
        public static int Taken;

        private void Take(Collision touch)
        {
            Taken++;
            if (StillOnly && touch.body != null) { Touched?.Invoke(this, touch.collider); return; }
            for (int k = 0; k < touch.contactCount; k++)
            {
                var point = touch.GetContact(k);
                float lifts = Mathf.Max(0, point.impulse.y);
                Where += point.point * lifts;
                Up += lifts;
            }
        }

        private void OnCollisionEnter(Collision touch) => Take(touch);
        private void OnCollisionStay(Collision touch) => Take(touch);
    }
}
