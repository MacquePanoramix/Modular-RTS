using UnityEngine;

namespace WonderGather
{
    // A piece struck off a boulder: a stone with its own weight, lying in the world.
    public sealed class LooseStone : MonoBehaviour
    {
        public Boulder From { get; private set; }
        private Collider solid;
        private float since;

        public void Came(Boulder from, Collider with) { From = from; solid = with; since = 0; }

        // A blow landed on it where it lay: this share of the blow's energy sends it off, mostly to one side, and no
        // faster than this (metres a second). How many times it has been knocked.
        public const float Takes = .3f, Fastest = 3;
        public int Knocks { get; private set; }
        public void Knocked(Vector3 outward, float energy)
        {
            var body = GetComponent<Rigidbody>();
            if (body == null) return;
            Knocks++;
            outward = Vector3.ProjectOnPlane(outward, Vector3.up);
            outward = outward.sqrMagnitude > 1e-6f ? outward.normalized : Vector3.forward;
            // To the side it already lies on, as seen from the way out of the rock.
            Vector3 aside = Vector3.Cross(Vector3.up, outward) * (Knocks % 2 == 0 ? 1 : -1);
            float speed = Mathf.Min(Fastest, Mathf.Sqrt(2 * Takes * Mathf.Max(0, energy) / Mathf.Max(.05f, body.mass)));
            body.WakeUp();
            body.linearVelocity = (aside * .8f + outward * .4f + Vector3.up * .45f).normalized * speed;
            body.angularVelocity = new Vector3(speed * 2, speed, -speed * 1.5f);
        }

        // Once it is clear of the rock it came from, the rock is solid to it again (and in any case after two seconds).
        private void FixedUpdate()
        {
            if (solid == null || From == null || From.Rock == null) { enabled = false; return; }
            since += Time.fixedDeltaTime;
            if (since < .15f) return;
            if (since > 2 || !Physics.ComputePenetration(solid, solid.transform.position, solid.transform.rotation, From.Rock, From.Rock.transform.position, From.Rock.transform.rotation, out _, out _))
            {
                Physics.IgnoreCollision(solid, From.Rock, false);
                enabled = false;
            }
        }
    }
}
