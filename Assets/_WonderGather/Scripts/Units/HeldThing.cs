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
        // What it is, and the share of its own weight it was made at (for trying the same tool lighter or heavier).
        public ToolDefinition Tool { get; set; }
        public float Weight { get; set; } = 1;
        // The hands that hold it now, if any.
        public PhysicalHands Holder { get; set; }
        public Blow Last { get; private set; }
        public int Blows { get; private set; }
        // Blows given with the striking part.
        public int HeadBlows { get; private set; }
        public Blow LastHeadBlow { get; private set; }
        // The last few of them. (Its head may meet two things in one step of the physics, a rock and a bush that
        // grows against it: each is told on its own, in no order that means anything.)
        private readonly Blow[] headBlows = new Blow[6];
        private int headBlowsTold;

        // Whether its striking part has struck a thing since a time.
        public bool HeadStruck(Collider what, float since)
        {
            if (what == null) return false;
            foreach (var blow in headBlows) if (blow.struck == what && blow.time >= since) return true;
            return false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var contact = collision.GetContact(0);
            var blow = new Blow
            {
                struck = collision.collider, with = contact.thisCollider, point = contact.point, velocity = collision.relativeVelocity,
                impulse = collision.impulse.magnitude, time = Time.time,
            };
            Last = blow; Blows++;
            if (Head != null && contact.thisCollider == Head) { LastHeadBlow = blow; HeadBlows++; headBlows[headBlowsTold++ % headBlows.Length] = blow; }
        }
    }
}
