using UnityEngine;

namespace WonderGather
{
    // Something that hangs on a miner by a handle and can be taken in a hand (Small's lantern, Long's mug): what the
    // interaction click points at. It sits on the thing's own bone; ThingsInHand puts it there.
    public sealed class HungThing : MonoBehaviour
    {
        public ThingsInHand Owner { get; set; }
        public MinerBody Body { get; set; }
        public int Index { get; set; }
        // Where its weight is now, for the eye and the pointer.
        public Vector3 Place => Body != null && Body.Ready ? Body.HangingWeight(Index) : transform.position;
        public bool InHand => Body != null && Body.InHand(Index) >= 0;
    }
}
