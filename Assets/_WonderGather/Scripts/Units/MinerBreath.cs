using UnityEngine;

namespace WonderGather
{
    // Every miner breathes, whatever has its body: standing as it did, walking, at its work, standing by its own
    // joints, fallen. Luis, October 10: "to some degree... when they are not very tired or in a moment that
    // justifies a lot of breath the breath should be gentle"; "I would like them to scale appropriately with
    // situation". One clock for each miner (Breath). How fast and how deep comes of the situation: how much its
    // muscles have spent, and, down after a fall, how shaken it is. What is drawn of it: from dusk to dawn, its
    // breath in the air; and, while it stands by its own joints or is down, its chest fuller and its shoulders
    // higher (not yet on the body as it was: see below). What a body of its own asks of its joints for a breath (OwnBody) is read from
    // here. How it is drawn, and how large, are OwnBody's settings (the panel's Breath button and slider).
    [DefaultExecutionOrder(-20)]
    public sealed class MinerBreath : MonoBehaviour
    {
        // So many puffs to a breath out, so far apart (seconds), where it is seen in the air. Its shoulders are let
        // rise, or let down, over this long (seconds) as its body becomes its own, or is given back.
        private const int PuffsABreath = 3;
        private const float PuffsApart = .16f, ShouldersIn = .4f;
        private Breath breath;
        private MinerBody miner;
        private PhysicalBody physical;
        private PhysicalFall fall;
        private ProceduralBiped body;
        private OwnBody own;
        private BreathInAir air;
        private bool begun, emptying;
        private float stature, headHalf, fullBefore, puffIn, shoulders;
        private int puffsLeft;

        // How full its chest is (0: empty; 1: full), how deep this breath is (1: a breath at rest), how many it takes
        // a minute now, and how many it has taken.
        public float Full => breath.Full;
        public float Deep => breath.Deep;
        public float AMinute => breath.AMinute;
        public int Taken => breath.Taken;
        // How hard it breathes for the situation it is in (0: at rest; 1: wholly out of breath).
        public float Hard { get; private set; }

        private void Awake()
        {
            miner = GetComponent<MinerBody>();
            physical = GetComponent<PhysicalBody>();
            body = GetComponent<ProceduralBiped>();
        }

        private void Update()
        {
            if (miner == null || physical == null || body == null || !miner.Ready || !physical.Ready || !body.Ready) return;
            if (!begun)
            {
                begun = true;
                breath.Begin(Breath.SeedOf(name));
                headHalf = Mathf.Max(.06f, body.BodyProportions.headHalf);
                stature = miner.Solved.head.position.y - transform.position.y + headHalf;
            }
            // How hard: what its muscles have spent; and down after a fall, how shaken it is.
            float spent = Mathf.Max(Mathf.Max(physical.Spent(PhysicalBody.Muscles.Legs), physical.Spent(PhysicalBody.Muscles.Back)),
                Mathf.Max(physical.Spent(PhysicalBody.Muscles.LeftArm), physical.Spent(PhysicalBody.Muscles.RightArm))) / .85f;
            if (fall == null) TryGetComponent(out fall);
            Hard = Mathf.Clamp01(fall != null && fall.Now != PhysicalFall.State.Up ? Mathf.Max(spent, Mathf.Max(fall.OutOfBreath, fall.Shaken)) : spent);
            float dt = Mathf.Min(Time.deltaTime, .1f);
            breath.Goes(dt, Hard);

            // What is drawn of it.
            float size = OwnBody.BreathSize, fills = breath.Full * breath.Deep;
            // (Its chest and its shoulders are drawn so only while it stands by its own joints, or is down. Drawn on a
            // miner standing or walking as it did, its blows at a boulder afterwards landed thirty centimetres off in
            // five tries of fifteen, and in one of thirty without: a swing is planned from where its shoulders are
            // drawn, and something there turns on a centimetre. Until that is found, the body as it was shows its
            // breath only in the air.)
            if (own == null) TryGetComponent(out own);
            bool shown = (own != null && own.Stands) || (fall != null && fall.Now != PhysicalFall.State.Up);
            shoulders = Mathf.MoveTowards(shoulders, shown ? 1 : 0, dt / ShouldersIn);
            miner.Swell = shoulders * Mathf.Min(OwnBody.SwellAtMost * size, OwnBody.BreathSwell * fills * OwnBody.BreathShown * size);
            miner.Shrug = OwnBody.ShouldersRise * stature * OwnBody.BreathShoulders * size * shoulders * breath.Full * Mathf.Min(breath.Deep, 1.4f);

            // Seen in the air: as its chest begins to empty, a few puffs from its mouth, one after another.
            if (breath.Full > fullBefore) emptying = false;
            else if (breath.Full < fullBefore && !emptying)
            {
                emptying = true;
                if (OwnBody.BreathSeenInAir && OwnBody.BreathShown > 0 && fullBefore >= .9f) { puffsLeft = PuffsABreath; puffIn = 0; }
            }
            fullBefore = breath.Full;
            if (puffsLeft > 0 && (puffIn -= dt) <= 0)
            {
                puffsLeft--; puffIn = PuffsApart;
                if (air == null && !TryGetComponent(out air)) air = gameObject.AddComponent<BreathInAir>();
                Transform face = miner.Solved.head;
                air.Puff(face.position + face.forward * (1.05f * headHalf) - face.up * (.35f * headHalf), face.forward - .1f * face.up, .5f * breath.Deep * size, headHalf);
            }
        }
    }
}
