using UnityEngine;

namespace WonderGather
{
    // A thing that can be held: it remembers what it last struck.
    public sealed class HeldThing : MonoBehaviour
    {
        public struct Blow
        {
            public Collider struck, with;
            public Vector3 point, velocity;
            public float impulse, time;
        }
        // The part of it that strikes (a pick's head).
        public Collider Head { get; set; }
        public Blow Last { get; private set; }
        public int Blows { get; private set; }
        // Blows given with the striking part.
        public int HeadBlows { get; private set; }
        public Blow LastHeadBlow { get; private set; }

        private void OnCollisionEnter(Collision collision)
        {
            var contact = collision.GetContact(0);
            var blow = new Blow
            {
                struck = collision.collider, with = contact.thisCollider, point = contact.point, velocity = collision.relativeVelocity,
                impulse = collision.impulse.magnitude, time = Time.time,
            };
            Last = blow; Blows++;
            if (Head != null && contact.thisCollider == Head) { LastHeadBlow = blow; HeadBlows++; }
        }
    }
}
