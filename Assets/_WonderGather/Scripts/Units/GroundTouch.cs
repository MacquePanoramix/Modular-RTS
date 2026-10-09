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

        public void Clear() { Where = Vector3.zero; Up = 0; }

        private void Take(Collision touch)
        {
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
