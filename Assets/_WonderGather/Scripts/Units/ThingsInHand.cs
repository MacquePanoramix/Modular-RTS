using System.Collections.Generic;
using UnityEngine;

namespace WonderGather
{
    // S3, step 8 (Docs/Design/ThePhysicalBody.md): what hangs on a miner by a handle (Small's lantern on its hook,
    // Long's mug in its loop) taken in hand and hung back, by the body. The hand on the thing's side reaches to the
    // handle where it hangs, closes on it, lifts it off, and carries it as a thing is carried by its handle: the arm
    // hanging at the side, far enough out for the thing to hang straight down clear of the clothes, and swinging
    // less than a free arm. To hang it back the hand brings the handle over its hook, lowers it on, lets go, and the
    // arm hangs free again.
    //
    // The thing is the same object throughout: it hangs and swings from the hand as it did from the hook, and the
    // body stops it (MinerBody). The arm follows (IArmGuide), whether the miner stands or walks. A hand with a
    // pickaxe in it is not free for this: the look at the work has the thing hung back first.
    [DefaultExecutionOrder(60)]
    [RequireComponent(typeof(MinerBody), typeof(ProceduralBiped))]
    public sealed class ThingsInHand : MonoBehaviour, IArmGuide
    {
        public enum Phase { Hung, Reaching, Lifting, Carried, Returning, Lowering, LettingGo }

        // How long the hand takes to reach the handle; to lift it off its hook, and to bring it to the side; to bring
        // it back over its hook, and to lower it on; to let go, and to hang at the side again (seconds). The fingers
        // begin to close this long before the hand is there.
        private const float Reaches = .55f, Lifts = .3f, Brings = .9f, Returns = .9f, Lowers = .3f, LetsGo = .25f, Hangs = .45f, Closes = .3f;
        // The hook holds the handle from below: it is lifted this far to come off, and this far out from the cloth
        // (metres).
        private const float Over = .03f, Off = .015f;
        // An arm that carries keeps this share of its swing (a lantern's flame, a mug's drink). Its hand comes in
        // towards the hips, as it swings, by about a tenth of the arm's drop times that; the arm hangs far enough out
        // for the thing to stay clear all through a stride, with this much to spare (metres).
        private const float Keeps = .45f, Spare = .012f;
        // Carrying, the forearm is raised forward by this much from hanging (degrees), and the upper arm hangs at
        // this share of its length (a little short of straight down: the elbow is not locked).
        private const float ForearmRaised = 52, UpperHangs = .97f;
        // At the hook the hand is brought onto the handle by what it sees: each frame it makes up this share of how
        // far the handle's place in its fingers is from the handle, for this long before it closes or lets go
        // (seconds); never by more than this (metres).
        private const float Follows = .5f, Settles = .1f, MostMadeUp = .04f;

        private MinerBody miner;
        private ProceduralBiped body;
        private readonly List<HungThing> things = new List<HungThing>();
        private Phase phase = Phase.Hung;
        private float clock;
        private int index = -1, hand = -1;
        private bool back;
        // The body as it was last posed, and in its frame: where the hook is, which way is out from the cloth there,
        // and how the bar lies on it (towards the thumb).
        private Vector3 hips, hookLocal, outLocal, barLocal;
        private Quaternion posture = Quaternion.identity;
        // What the hand has made up, and where it means the handle to be, in the body's frame; and whether it is
        // settling onto that now.
        private Vector3 madeUp, meantLocal;
        private bool settling;

        public int Count => things.Count;
        public HungThing Thing(int i) => things[i];
        public Phase Now => phase;
        // The thing it is taking, has, or is hanging back.
        public HungThing Handles => index >= 0 ? things.Find(t => t.Index == index) : null;
        // The thing in its hand: from the moment its fingers have it until it is back on its hook.
        public HungThing Has => index >= 0 && miner.InHand(index) >= 0 ? Handles : null;
        public int Hand => hand;
        // Its hand is on the way to the handle, or on the way back.
        public bool Busy => phase != Phase.Hung && phase != Phase.Carried;
        // Where it last asked its wrist to be.
        public Vector3 WristAsked { get; private set; }
        // How much further out than a free arm the arm that carries it hangs (metres).
        public float HangsOut { get; private set; }

        // The miner's own, made when first asked for. Null for a body with nothing that a hand can take.
        public static ThingsInHand Of(Component unit)
        {
            if (unit == null) return null;
            if (unit.TryGetComponent<ThingsInHand>(out var has)) return has;
            if (!unit.TryGetComponent<MinerBody>(out var miner) || !unit.TryGetComponent<ProceduralBiped>(out _)) return null;
            bool any = false;
            for (int k = 0; k < miner.Things.Count; k++) any |= miner.Takes(k);
            return any ? unit.gameObject.AddComponent<ThingsInHand>() : null;
        }

        private void Awake()
        {
            miner = GetComponent<MinerBody>();
            body = GetComponent<ProceduralBiped>();
            for (int k = 0; k < miner.Things.Count; k++)
            {
                if (!miner.Takes(k)) continue;
                var bone = miner.Things[k].bone;
                var thing = bone.GetComponent<HungThing>();
                if (thing == null) thing = bone.gameObject.AddComponent<HungThing>();
                thing.Owner = this; thing.Body = miner; thing.Index = k;
                things.Add(thing);
            }
        }

        private void OnEnable() { if (body != null) body.GuideAlso(this); }

        private void OnDisable()
        {
            if (body != null) body.GuideAlso(null);
            // Whatever it had goes back to where it hangs.
            if (index >= 0)
            {
                miner.Carry(index, -1);
                miner.HoldHandle(hand, false, Vector3.zero, Vector3.up, 0, Vector3.zero);
                if (body != null) body.CarryAtSide(hand, 0, 1);
            }
            phase = Phase.Hung; index = hand = -1; back = false;
            if (body != null) body.RegardNothing();
        }

        private void OnDestroy()
        {
            foreach (var thing in things) if (thing != null) Destroy(thing);
        }

        // The hand on the thing's own side.
        private int HandFor(int k) => Vector3.Dot(miner.HookHome(k) - body.HipsNow, body.FacingNow * Vector3.right) < 0 ? 0 : 1;

        // A hand is free for it if it closes, has nothing, and the body has no tool in its hands.
        private bool Free(int which) => miner.CanHold(which) && !miner.Carries(which)
                                        && GetComponent<PhysicalHands>() == null && GetComponent<EquippedTool>() == null;

        public bool CanTake(HungThing thing) => thing != null && thing.Owner == this && isActiveAndEnabled && phase == Phase.Hung
                                                && miner.Ready && body.Ready && miner.Takes(thing.Index) && Free(HandFor(thing.Index));

        // The hand on its side reaches for its handle, takes it off its hook, and carries it.
        public bool Take(HungThing thing)
        {
            if (!CanTake(thing)) return false;
            index = thing.Index; hand = HandFor(index); back = false;
            hips = body.HipsNow; posture = body.PostureNow;
            Place();
            // How far out the arm must hang for the thing to hang straight, clear of the body's side, all through a
            // stride: the relaxed hand hangs about under the shoulder's own reach to the side.
            var p = body.BodyProportions;
            float hangs = p.shoulder.x + p.armHang.x + (hand == 0 ? p.armCarry.x : p.armCarry.y);
            float comesIn = .1f * p.armHang.y * Keeps;
            HangsOut = Mathf.Max(0, miner.Things[index].clear + Spare + comesIn - hangs);
            madeUp = Vector3.zero; settling = false;
            WristAsked = body.FreeWrist(hand);
            Go(Phase.Reaching);
            return true;
        }

        // It hangs the thing in its hand back on its hook. Asked while the hand is still taking it, it does so as
        // soon as it has it.
        public bool HangBack()
        {
            if (index < 0 || phase == Phase.Hung) return false;
            if (phase == Phase.Reaching || phase == Phase.Lifting || phase == Phase.Carried) back = true;
            return true;
        }

        private void Go(Phase next)
        {
            phase = next; clock = 0;
            // The arm carries from the moment the thing is off its hook until it is over it again.
            if (next == Phase.Lifting) body.CarryAtSide(hand, 0, Keeps);
            if (next == Phase.Lowering) body.CarryAtSide(hand, 0, 1);
        }

        // The hook, in the body's own frame, as the body was last posed.
        private void Place()
        {
            if (index < 0) return;
            Quaternion from = Quaternion.Inverse(body.PostureNow);
            hookLocal = from * (miner.Hook(index) - body.HipsNow);
            outLocal = from * miner.HookOut(index);
            // The bar lies in the fingers towards the thumb: inward and ahead, for a hand that comes from above.
            Vector3 bar = miner.HookBar(index);
            Vector3 thumb = body.FacingNow * new Vector3(hand == 0 ? .7f : -.7f, 0, 1);
            barLocal = from * (Vector3.Dot(bar, thumb) < 0 ? -bar : bar);
        }

        private void Update()
        {
            if (phase == Phase.Hung) return;
            clock += Time.deltaTime;
            switch (phase)
            {
                case Phase.Reaching:
                    if (clock >= Reaches && miner.Held(hand) >= 1) { miner.Carry(index, hand); Go(Phase.Lifting); }
                    break;
                case Phase.Lifting:
                    if (clock >= Lifts + Brings) Go(Phase.Carried);
                    break;
                case Phase.Carried:
                    if (back) { back = false; Go(Phase.Returning); }
                    break;
                case Phase.Returning:
                    if (clock >= Returns) Go(Phase.Lowering);
                    break;
                case Phase.Lowering:
                    if (clock >= Lowers + Settles) { miner.Carry(index, -1); Go(Phase.LettingGo); }
                    break;
                case Phase.LettingGo:
                    if (clock >= LetsGo + Hangs) { phase = Phase.Hung; index = hand = -1; }
                    break;
            }
        }

        // After the body and its model have posed: where the hook is now, for the next pose; and, at the hook, how far
        // the handle's place in the fingers is from where it is meant to be.
        private void LateUpdate()
        {
            Place();
            if (index < 0 || !settling || !miner.HandleIn(hand, miner.Things[index].grip, out var inFingers, out _)) return;
            Vector3 meant = body.HipsNow + body.PostureNow * meantLocal;
            madeUp = Vector3.ClampMagnitude(madeUp + Quaternion.Inverse(body.PostureNow) * (meant - inFingers) * Follows, MostMadeUp);
        }

        public void Stands(Vector3 hipsNow, Quaternion postureNow, Vector3 leftShoulder, Vector3 rightShoulder)
        {
            hips = hipsNow; posture = postureNow;
        }

        public bool Guides(int which) => phase != Phase.Hung && which == hand;

        // Where the wrist is, carrying: under the shoulder by the upper arm's length and what the raised forearm
        // adds, ahead by what the forearm reaches forward, and as far out to the side as the free arm hangs.
        private Vector3 Carries(int which, Vector3 shoulder, Vector3 ahead)
        {
            var p = body.BodyProportions;
            float raised = ForearmRaised * Mathf.Deg2Rad;
            Vector3 aside = Vector3.Cross(Vector3.up, ahead) * (which == 0 ? -1 : 1);
            return shoulder + Vector3.down * (p.upperArm * UpperHangs + p.forearm * Mathf.Cos(raised)) + ahead * (p.forearm * Mathf.Sin(raised))
                   + aside * (p.armHang.x + (which == 0 ? p.armCarry.x : p.armCarry.y));
        }

        public Vector3 Wrist(int which, Vector3 shoulder)
        {
            float radius = miner.Things[index].grip;
            Vector3 hook = hips + posture * hookLocal, bar = posture * barLocal;
            Vector3 above = hook + Vector3.up * Over + posture * outLocal * Off;
            // Where the arm hangs with nothing to guide it: the hand goes out from there, and comes back to it.
            Vector3 free = body.FreeWrist(which);
            Vector3 ahead = Vector3.ProjectOnPlane(posture * Vector3.forward, Vector3.up).normalized;
            // Carried, the thing hangs from a hand held a little before the hip: the upper arm hanging, the forearm
            // raised forward, as a lantern is carried by its bail, and the bar lying ahead in the fingers (the thumb
            // forward). (It was carried on a straight arm held out to the side, far enough for the thing to hang
            // clear of the coat: a hold no arm keeps up, and Luis's "a bit uncanny", October 8.)
            Vector3 carried = Carries(which, shoulder, ahead);
            // The wrist's place for the handle to be somewhere, lying as it lies on its hook.
            Vector3 On(Vector3 handle)
            {
                meantLocal = Quaternion.Inverse(posture) * (handle - hips);
                return miner.WristFor(which, handle, bar, radius, shoulder) + posture * madeUp;
            }
            Vector3 wrist = free, way = bar;
            bool closes = true;
            settling = false;
            switch (phase)
            {
                case Phase.Reaching:
                    // (It looks at what it reaches for.)
                    body.Regard(hook);
                    wrist = Vector3.Lerp(free, On(hook), Mathf.SmoothStep(0, 1, clock / (Reaches - Settles)));
                    closes = clock > Reaches - Closes;
                    settling = clock >= Reaches - Settles;
                    break;
                case Phase.Lifting:
                    if (clock < Lifts) wrist = On(Vector3.Lerp(hook, above, Mathf.SmoothStep(0, 1, clock / Lifts)));
                    else
                    {
                        float brought = Mathf.SmoothStep(0, 1, (clock - Lifts) / Brings);
                        wrist = Vector3.Lerp(On(above), carried, brought);
                        way = Vector3.Slerp(bar, ahead, brought);
                    }
                    break;
                case Phase.Carried:
                    wrist = carried;
                    way = ahead;
                    break;
                case Phase.Returning:
                    float returned = Mathf.SmoothStep(0, 1, clock / Returns);
                    wrist = Vector3.Lerp(carried, On(above), returned);
                    way = Vector3.Slerp(ahead, bar, returned);
                    break;
                case Phase.Lowering:
                    body.Regard(hook);
                    wrist = On(Vector3.Lerp(above, hook, Mathf.SmoothStep(0, 1, clock / Lowers)));
                    settling = true;
                    break;
                case Phase.LettingGo:
                    wrist = Vector3.Lerp(On(hook), free, Mathf.SmoothStep(0, 1, (clock - LetsGo) / Hangs));
                    closes = false;
                    break;
            }
            if (phase != Phase.Reaching && phase != Phase.Lowering && !(phase == Phase.Returning && clock > Returns * .5f)) body.RegardNothing();
            else if (phase == Phase.Returning) body.Regard(hook);
            WristAsked = wrist;
            miner.HoldHandle(which, closes, wrist, way, radius, shoulder);
            return wrist;
        }
    }
}
