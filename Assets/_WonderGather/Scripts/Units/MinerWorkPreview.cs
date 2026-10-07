using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WonderGather
{
    // The miners' physical work, in the place (S3: Docs/Design/ThePhysicalBody.md). The chosen miner works by its own
    // strength: its pickaxe is a real body, the arms and the back give what they can, they tire, and the body keeps its
    // own balance or loses it (PhysicalHands, PhysicalBack, PhysicalBalance, PhysicalSwing, PhysicalCarry,
    // PhysicalFall).
    //
    // Nothing appears in its hands. The panel (MinerPanel) puts a pickaxe on the ground beside it, lighter or heavier,
    // and sets how strong it is. With the interaction click (InteractionClick: the space bar and a click) it picks a
    // pickaxe up and lays it down, mines a boulder, rests and goes back to its work: each done by its body. Sent
    // somewhere with its pickaxe, it takes it along, held as its strength allows: in one hand at its side, or dragged.
    // A pickaxe that lies (laid down, left because it was too heavy, or fallen with it) stays in the world.
    //
    // The bench's block of rock is still here for the tests and the captures (Toggle): a plain block before the miner,
    // and its own pickaxe made in its hands, as the key K did until the panel took its place (step 11).
    [RequireComponent(typeof(MinerChoice))]
    public sealed class MinerWorkPreview : MonoBehaviour
    {
        private MinerChoice choice;
        private SelectableUnit working, again;
        private PhysicalBody physical;
        private PhysicalHands hands;
        private PhysicalBack back;
        private PhysicalBalance balance;
        private PhysicalSwing swing;
        private PhysicalCarry carry;
        private GameObject rock;
        // The miner's pickaxe, lying in the world: laid down, or left.
        private HeldThing lying;
        public HeldThing Lying => lying;
        // The pickaxes the panel has put on the ground.
        private readonly List<HeldThing> laid = new List<HeldThing>();
        // A pickaxe is put this far from the miner (metres), and then further: to its right first, then round it.
        private static readonly float[] LaidAway = { .6f, .9f };
        private static readonly float[] LaidRound = { 90, 45, 135, -90, -45, -135, 0, 180 };
        private const int Ground = 1 << 6;
        private readonly Collider[] inTheWay = new Collider[4];
        // What it last could not do, said for a few seconds.
        private string said = "";
        private float saidAt = -10;
        private const float SaidFor = 5;
        // The miner the panel's strength was last given to.
        private SelectableUnit strong;
        private Coroutine setting;
        // What a miner was told to do that must wait for something first (a thing in its hand hung back, its pickaxe
        // picked up): done when that is so, and forgotten if it can no longer be.
        private System.Action then;
        private System.Func<bool> thenWhen, thenWhile;
        private SelectableUnit thenFor;
        // The body that can be let go (the fall), the pickaxe it last had in its hands, and whether it is down now.
        private PhysicalFall fall;
        private HeldThing had;
        private bool down;
        public PhysicalFall Fall => fall;
        // The boulder it has been sent to mine, where it stands and strikes for it, and whether it has come there and
        // set to work.
        private Boulder mining;
        private RockWork.Plan plan;
        private bool atRock, tookThisSwing, kneesJudged;
        // For how long it has stood bent to its work, ready to swing, since it came to its place (seconds), and after
        // how long its knees are read.
        private float readyFor;
        private const float JudgesAfter = .3f;
        private int headBlows, tries;
        private Vector3 aimedAt, aimedFacing;
        private UnityEngine.AI.NavMeshAgent agent;
        private float stopsWithin = -1, tall = 1;
        // Going to a rock, it stops this near the place it means to stand (metres): nearer than a walk does.
        private const float StandsWithin = .03f;
        public Boulder Mining => mining;
        public RockWork.Plan MiningPlan => plan;
        // It has come to its place at the boulder and is at work on it.
        public bool AtRock => mining != null && atRock;
        private float strength = 1, weight = 1;
        public bool Showing => working != null || again != null;
        // The swing being shown (its results, its phase), once the miner has taken up its pickaxe.
        public PhysicalSwing Swing => swing;
        public bool Swinging => swing != null && swing.enabled && hands != null && hands.Held != null;
        // It has been sent somewhere and has its pickaxe with it (or has left it behind): how it holds it.
        public bool Carrying => carry != null && carry.enabled;
        public PhysicalCarry Carry => carry;
        public float Strength => strength;
        public float Weight => weight;

        private void Awake()
        {
            choice = GetComponent<MinerChoice>();
            // The interaction click and the panel live beside it: where it is, they are.
            if (GetComponent<InteractionClick>() == null) gameObject.AddComponent<InteractionClick>();
            if (GetComponent<MinerPanel>() == null) gameObject.AddComponent<MinerPanel>();
        }

        private void OnDisable()
        {
            End(); ClearLying();
            foreach (var thing in laid) if (thing != null && thing.Holder == null) Destroy(thing.gameObject);
            laid.Clear();
        }

        // A rock face to mine, for the first swing's tests and captures: a block whose near face stands `reach` in
        // front of a place to stand, facing it, with a place to deliver to. What is seen (if it is) and what is
        // struck are the same block.
        public static (ResourceNode node, ResourceDepot depot, GameObject rock) RockFace(Vector3 stand, Vector3 facing, float reach, Vector3 deliver, bool visible, float height)
        {
            var size = new Vector3(.7f, height, .8f);
            var rock = new GameObject("Rock face");
            rock.transform.SetPositionAndRotation(stand + facing * (reach + size.z * .5f), Quaternion.LookRotation(facing));
            var face = visible ? GameObject.CreatePrimitive(PrimitiveType.Cube) : new GameObject("Face");
            face.name = "Face";
            face.transform.SetParent(rock.transform, false);
            face.transform.localPosition = new Vector3(0, size.y * .5f - .2f, 0);
            face.transform.localScale = size + new Vector3(0, .4f, 0);
            var box = visible ? face.GetComponent<BoxCollider>() : face.AddComponent<BoxCollider>();
            var node = rock.AddComponent<ResourceNode>();
            var work = rock.AddComponent<ResourceWorkplace>();
            var mine = rock.AddComponent<MineableResource>();
            mine.Configure(box);
            var at = new GameObject("Stand").transform;
            at.SetParent(rock.transform, true);
            at.position = stand;
            var touch = new GameObject("Contact").transform;
            touch.SetParent(rock.transform, true);
            touch.position = stand + facing * reach + Vector3.up;
            work.Configure(new[] { at }, new[] { touch });
            var home = new GameObject("Delivery place");
            home.transform.position = deliver;
            return (node, home.AddComponent<ResourceDepot>(), rock);
        }

        // How far in front of a body the first swing's rock face stands: where the head's swing meets it, a little
        // short of the swing's farthest reach.
        public static float FaceDistance(ProceduralBiped body, ToolDefinition pickaxe)
            => (body.ToolHand.z + (pickaxe.Head - pickaxe.PrimaryGrip).magnitude) * .78f;

        private void Update()
        {
            if (again != null)
            {
                // What the last look added to the miner is gone by now: it begins afresh.
                var unit = again;
                again = null;
                if (choice.Current == unit) Begin(unit);
            }
            // The panel's strength is the chosen miner's; one that is no longer chosen is as strong as its build again.
            if (choice.Current != strong) Strengthen();
            if (then != null)
            {
                if (choice.Current != thenFor || (thenWhile != null && !thenWhile())) then = null;
                else if (thenWhen == null || thenWhen()) { var act = then; then = null; act(); }
            }
            if (!Showing) return;
            // It has fallen: its pickaxe has left its hands and lies where it fell, and its work is over. Nothing more
            // is asked of it until it is up again; then, its hands empty, the look ends by itself.
            if (hands != null && hands.Thing != null) had = hands.Thing;
            if (fall != null && fall.Now != PhysicalFall.State.Up)
            {
                if (!down) Fell();
                return;
            }
            down = false;
            // Chosen another: the look is over.
            if (choice.Current != working || !working.gameObject.activeInHierarchy) { End(); return; }
            // Sent somewhere while it works: it takes its pickaxe with it. (A step it takes to keep its feet is not
            // being sent anywhere.)
            if (!Carrying && working.Motor != null && working.Motor.IsMoving) { TakeAlong(); if (!Showing) return; }
            // Its pickaxe lies on the ground (laid down, left, or not reached), and it has stood up: it has nothing
            // more to do with it until it is told to pick it up.
            if (Carrying && hands != null && hands.Held == null && !carry.Fetching && !carry.Laying
                && working.TryGetComponent<ProceduralBiped>(out var stands) && Mathf.Abs(stands.BowNow) < 3 && stands.SinkNow < .01f)
            {
                // (If it went for one and did not take it, it says why.)
                if (!string.IsNullOrEmpty(carry.NotTaken)) Say(carry.NotTaken);
                End();
                return;
            }
            if (mining != null) AtTheRock();
        }

        // The bench's block, in the place (for the tests and the captures; the key K did this until step 11): its own
        // pickaxe is made in the miner's hands and a block put before it; at work, both are put away; carrying, the
        // pickaxe is put to work on a block where the miner stands.
        public void Toggle()
        {
            if (!Showing) Begin(choice != null ? choice.Current : null);
            else if (Carrying && hands != null && hands.Held != null) WorkHere();
            else End();
        }

        // Sent somewhere: the block is put away, and the miner goes with its pickaxe held as its strength allows.
        private void TakeAlong()
        {
            if (setting != null) { StopCoroutine(setting); setting = null; }
            // Not yet in its hands: there is nothing to take.
            if (hands == null || hands.Held == null) { End(); return; }
            if (rock != null) Destroy(rock);
            rock = null;
            swing.enabled = false;
            carry.enabled = true;
        }

        // What a pickaxe offers the interaction click: in this miner's hands, to be laid down; lying, to be picked up.
        public void OptionsFor(HeldThing thing, System.Collections.Generic.List<InteractionClick.Option> into)
        {
            if (thing == null || choice == null || choice.Current == null) return;
            if (thing.Holder != null)
            {
                if (hands != null && thing.Holder == hands && hands.Held != null && !(Carrying && (carry.Laying || carry.Fetching)))
                    into.Add(new InteractionClick.Option { label = "Lay it down", act = LayDown });
            }
            else if (!Showing || (hands != null && hands.Held == null))
                into.Add(new InteractionClick.Option { label = "Pick it up", act = () => PickUp(thing) });
        }

        // What a thing that hangs on the miner by a handle offers (its lantern, its mug): on its hook, to be taken in
        // the hand on its side, if that hand is free; in the hand, to be hung back. With its pickaxe the miner's hands
        // are not free for it.
        public void OptionsForHung(HungThing thing, System.Collections.Generic.List<InteractionClick.Option> into)
        {
            var has = thing != null ? thing.Owner : null;
            if (has == null || choice == null || choice.Current == null || has.gameObject != choice.Current.gameObject || has.Busy) return;
            if (has.Has == thing) into.Add(new InteractionClick.Option { label = "Hang it back", act = () => has.HangBack() });
            else if (!Showing && has.CanTake(thing)) into.Add(new InteractionClick.Option { label = "Take in hand", act = () => has.Take(thing) });
        }

        // A miner with something in its hand hangs it back before it takes up its pickaxe: its hands are for the tool.
        // What it was told is done when the thing hangs again.
        private bool HangsBackFirst(SelectableUnit unit, System.Action after)
        {
            var has = unit != null ? unit.GetComponent<ThingsInHand>() : null;
            if (has == null || (has.Has == null && !has.Busy)) return false;
            has.HangBack();
            After(unit, after, () => has == null || (has.Has == null && !has.Busy));
            return true;
        }

        // What it was told is done when something is so, and forgotten if something else stops being so.
        private void After(SelectableUnit unit, System.Action act, System.Func<bool> when, System.Func<bool> stillTo = null)
        {
            then = act; thenFor = unit; thenWhen = when; thenWhile = stillTo;
        }

        // What a boulder offers: to be mined by the chosen miner.
        public void OptionsForBoulder(Boulder boulder, System.Collections.Generic.List<InteractionClick.Option> into)
        {
            var unit = choice != null ? choice.Current : null;
            if (boulder == null || boulder.Rock == null || unit == null || !unit.TryGetComponent<MinerBody>(out var body) || body.Pickaxe == null || !body.Pickaxe.HasWeight) return;
            if (Showing && carry != null && (carry.Laying || carry.Fetching)) return;
            // It is at it already.
            if (mining == boulder && atRock && !Carrying) return;
            into.Add(new InteractionClick.Option { label = "Mine", act = () => Mine(boulder) });
        }

        // It goes to a boulder and mines it: it finds where to stand and where to strike from the rock's own shape
        // (RockWork), walks there with its pickaxe, turns to the spot, and swings at it. With something in its hand it
        // hangs that back first; with no pickaxe in its hands it first goes to the nearest one that lies in the world
        // and picks it up. If none lies anywhere, it has nothing to mine with: none is made for it.
        public void Mine(Boulder boulder)
        {
            var unit = choice != null ? choice.Current : null;
            if (boulder == null || boulder.Rock == null || unit == null) return;
            if (HangsBackFirst(unit, () => Mine(boulder))) return;
            if (!Showing)
            {
                var lies = NearestLying(unit.transform.position);
                if (lies == null) { Say("It has no pickaxe to mine with: put one on the ground by it"); return; }
                PickUp(lies);
                // (It has it when it stands with it: where to stand and strike at the rock is found for a body that
                // stands, not for one still bent to the ground.)
                if (Showing) After(unit, () => Mine(boulder), () => Carrying && hands != null && hands.Held != null && !carry.Fetching && (hands.Holds(0) || hands.Holds(1)) && StandsUp(unit), () => Showing);
                return;
            }
            if (hands == null || hands.Held == null || working != unit || (carry != null && (carry.Laying || carry.Fetching))) return;
            if (!working.TryGetComponent<ProceduralBiped>(out var biped)) return;
            var found = RockWork.Find(boulder, swing, biped, tall, working.transform.position);
            if (!found.found) { Say("It finds no place to stand and strike at that boulder"); return; }
            mining = boulder; plan = found; tries = 0;
            // (Its knees are read once for the order, the first time it bends to its work there: not again when it
            // goes back to the same rock after a rest, its legs a little spent.)
            kneesJudged = false; readyFor = 0;
            GoToRock();
        }

        private void Fell()
        {
            down = true;
            if (setting != null) { StopCoroutine(setting); setting = null; }
            if (rock != null) Destroy(rock);
            rock = null;
            LeaveRock("it fell");
            then = null;
            if (had != null && had.Holder == null) lying = had;
            if (swing != null) swing.enabled = false;
            if (carry != null) carry.enabled = true;
        }

        private static bool StandsUp(SelectableUnit unit)
            => unit != null && unit.TryGetComponent<ProceduralBiped>(out var biped) && Mathf.Abs(biped.BowNow) < 5 && biped.SinkNow < .02f;

        // The pickaxe that lies in the world nearest a place, if any lies.
        private HeldThing NearestLying(Vector3 from)
        {
            HeldThing best = null;
            float nearest = float.MaxValue;
            foreach (var thing in FindObjectsByType<HeldThing>(FindObjectsSortMode.None))
            {
                if (thing.Tool == null || thing.Holder != null) continue;
                float away = (thing.transform.position - from).sqrMagnitude;
                if (away < nearest) { nearest = away; best = thing; }
            }
            return best;
        }

        // The panel: a pickaxe of the chosen miner's own kind is put on the ground beside it, at a share of its own
        // weight, where the ground is open. Nothing is put in its hands: it picks it up itself.
        public HeldThing LayPickaxe(float share)
        {
            var unit = choice != null ? choice.Current : null;
            if (unit == null || !unit.TryGetComponent<MinerBody>(out var body) || body.Pickaxe == null || !body.Pickaxe.HasWeight) return null;
            var tool = body.Pickaxe;
            Vector3 ahead = Vector3.ProjectOnPlane(unit.transform.forward, Vector3.up).normalized;
            float rests = Mathf.Max(tool.HeadRadius, tool.GripRadius(0), tool.GripRadius(1));
            // What it takes up of the ground, lying flat: its handle's length one way and its head's the other.
            Vector3 middle = new Vector3(0, (tool.Top + tool.Foot) * .5f, tool.Point.z * .1f);
            Vector3 half = new Vector3(rests + .01f, (tool.Top - tool.Foot) * .5f + .03f, tool.Point.z * .9f + tool.HeadRadius + .03f);
            for (int pass = 0; pass < 2; pass++)
                foreach (float away in LaidAway)
                    foreach (float round in LaidRound)
                    {
                        Vector3 way = Quaternion.AngleAxis(round, Vector3.up) * ahead;
                        Vector3 at = unit.transform.position + way * away;
                        if (!Physics.Raycast(at + Vector3.up * 2, Vector3.down, out var ground, 6, Ground, QueryTriggerInteraction.Ignore)) continue;
                        // On the first pass, only where the miner can walk to it.
                        if (pass == 0 && !UnityEngine.AI.NavMesh.SamplePosition(ground.point, out _, .25f, UnityEngine.AI.NavMesh.AllAreas)) continue;
                        // Flat: its head away from the miner, the points of its head level.
                        Quaternion flat = Quaternion.LookRotation(way, Vector3.Cross(Vector3.up, way));
                        Vector3 place = ground.point + Vector3.up * (rests + .02f) - flat * new Vector3(0, tool.Centre.y, tool.Centre.z);
                        if (Physics.OverlapBoxNonAlloc(place + flat * middle + Vector3.up * .03f, half, inTheWay, flat, ~Ground, QueryTriggerInteraction.Ignore) > 0) continue;
                        var made = PhysicalHands.Make(tool, place, flat, Mathf.Clamp(share, .4f, 3));
                        laid.RemoveAll(x => x == null);
                        laid.Add(made);
                        return made;
                    }
            Say("There is no open ground beside it for a pickaxe");
            return null;
        }

        // What its own pickaxe weighs at a share of its weight (kilograms), for the panel's words.
        public float PickaxeWeighs(float share)
        {
            var unit = choice != null ? choice.Current : null;
            return unit != null && unit.TryGetComponent<MinerBody>(out var body) && body.Pickaxe != null ? body.Pickaxe.Mass * share : 0;
        }

        // The panel: the pickaxes that lie in the world, in no hand, are taken away.
        public void ClearLaid()
        {
            foreach (var thing in FindObjectsByType<HeldThing>(FindObjectsSortMode.None))
                if (thing.Tool != null && thing.Holder == null) Destroy(thing.gameObject);
            laid.Clear();
            lying = null;
        }

        private void Say(string what) { said = what; saidAt = Time.time; }

        // It walks to its place at the rock, its pickaxe held as its strength allows.
        private void GoToRock()
        {
            if (!Carrying) TakeAlong();
            if (!Showing || mining == null) return;
            atRock = false;
            if (agent == null || agent.gameObject != working.gameObject) agent = working.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null && stopsWithin < 0) { stopsWithin = agent.stoppingDistance; agent.stoppingDistance = StandsWithin; }
            var motor = working.Motor;
            if (motor == null) return;
            // Off the walked ground already (at this rock): it steps straight to its place if it can.
            if (motor.IsOff && motor.StepOff(plan.stand)) return;
            if (Vector3.ProjectOnPlane(plan.approach - working.transform.position, Vector3.up).magnitude > StandsWithin + .02f || motor.IsOff) motor.TryMove(plan.approach);
        }

        // It walks as it does again (it stops as near a place as a walk does).
        private void Walks()
        {
            if (agent != null && stopsWithin >= 0) agent.stoppingDistance = stopsWithin;
            stopsWithin = -1;
        }

        // It is no longer at, or going to, a boulder.
        private void LeaveRock(string why = null)
        {
            if (mining != null && why != null) LeftRock = why;
            mining = null; atRock = false;
            Walks();
        }
        // What its last blow at a rock landed on.
        public string StruckLast { get; private set; } = "";
        // Why it last gave a boulder up without being told to.
        public string LeftRock { get; private set; } = "";

        // Each frame while it has a boulder to mine: it comes to its place, turns to the spot and sets to work; at work,
        // each blow of the pick's head on the rock is the rock's to take, and before each swing it aims again from
        // where it then stands (it may have stepped to keep its feet).
        private void AtTheRock()
        {
            if (mining.Rock == null || hands == null || hands.Held == null || working == null) { LeaveRock("it has no pickaxe in its hands"); return; }
            var motor = working.Motor;
            if (motor == null) { LeaveRock("it cannot walk"); return; }
            Vector3 short_ = Vector3.ProjectOnPlane(plan.stand - working.transform.position, Vector3.up);
            if (!atRock)
            {
                if (!Carrying) return;
                if (motor.IsMoving)
                {
                    // Sent somewhere else on the way: it is not going to the rock any more.
                    // (Its own way goes to where the walked ground ends, and from there to its place.)
                    float elsewhere = Mathf.Min(Vector3.ProjectOnPlane(motor.Destination - plan.approach, Vector3.up).magnitude,
                        Vector3.ProjectOnPlane(motor.Destination - plan.stand, Vector3.up).magnitude);
                    if (elsewhere > .3f) LeaveRock(string.Format(System.Globalization.CultureInfo.InvariantCulture, "it was sent somewhere else ({0:0.00} m from where it was going)", elsewhere));
                    return;
                }
                if (short_.magnitude > .04f)
                {
                    // Not at its place yet. From where the walked ground ends it takes the last steps on its own feet;
                    // if it stopped short of there, it tries again, a few times.
                    bool near = Vector3.ProjectOnPlane(plan.approach - working.transform.position, Vector3.up).magnitude < .1f;
                    if (tries++ < 4 && (motor.IsOff || near ? motor.StepOff(plan.stand) : motor.TryMove(plan.approach))) return;
                    LeaveRock("it could not come to its place");
                    return;
                }
                motor.Stop();
                // It turns to the spot.
                Vector3 to = Vector3.ProjectOnPlane(plan.spot - working.transform.position, Vector3.up);
                if (Vector3.Angle(working.transform.forward, to) > 1) { motor.Face(plan.spot, Time.deltaTime); return; }
                Walks();
                // And sets to work on it: the swing is aimed at the spot from where it stands, and the pickaxe taken
                // up from however it was held.
                atRock = true;
                carry.enabled = false;
                swing.enabled = true;
                Aims();
                swing.TakeUp();
                headBlows = hands.Thing.HeadBlows; tookThisSwing = false;
                return;
            }
            // Resting from it, it is still its rock; sent somewhere, it is not.
            if (Carrying)
            {
                if (motor.IsMoving) LeaveRock("it was sent somewhere");
                return;
            }
            // Bent to its work for the first time at this boulder, before its first blow, its knees are read: asked too much
            // already, the blows would ask them more than they have (the lowest boulders, for the taller and the
            // lighter bodies). It does not work there, stands up with its pickaxe, and says why.
            if (!kneesJudged && swing.phase == PhysicalSwing.Phase.Ready)
            {
                readyFor += Time.deltaTime;
                if (readyFor > JudgesAfter)
                {
                    kneesJudged = true;
                    if (balance != null && balance.KneesAsked(hands.ToolMass) > PhysicalBalance.WorkRaises)
                    {
                        Say("It would have to bend its knees too deep to work at that boulder");
                        LeaveRock("its knees would be bent too deep for the work");
                        TakeAlong();
                        return;
                    }
                }
            }
            if (swing.phase == PhysicalSwing.Phase.Lift) tookThisSwing = false;
            // A blow of the head on the rock: the rock takes its energy, once for each swing, as the swing itself
            // measured it (the head's speed at the step before it struck).
            if (!tookThisSwing && swing.Current.struck && (swing.phase == PhysicalSwing.Phase.Drive || swing.phase == PhysicalSwing.Phase.Struck))
            {
                tookThisSwing = true;
                // Where it landed: the side of the head's striking ball that met the rock. (The engine finds a fast
                // contact a step ahead, and gives its place as where the head then was: up to a step's travel short.)
                Vector3 landed = swing.Current.landed - plan.outward * swing.tool.HeadRadius;
                var struck = hands.Thing.LastHeadBlow.struck;
                StruckLast = struck != null ? struck.name : "nothing";
                if (struck == mining.Rock) mining.Strike(landed, plan.outward, swing.Current.energy);
                // A piece that lies where the pick lands is knocked aside: the rock takes nothing of that blow.
                else if (struck != null && struck.GetComponentInParent<LooseStone>() is LooseStone piece) piece.Knocked(plan.outward, swing.Current.energy);
            }
            // Before a swing, if it has moved (a step to keep its feet), it aims again from where it stands now; if the
            // spot is out of its reach from there, it goes back to its place.
            if (swing.phase == PhysicalSwing.Phase.Ready
                && ((working.transform.position - aimedAt).sqrMagnitude > .0001f || Vector3.Angle(working.transform.forward, aimedFacing) > 1))
            {
                Aims();
                if (swing.AimMiss > .05f) GoToRock();
            }
        }

        private void Aims()
        {
            swing.Aim(plan.spot, RockWork.LeansUpTo);
            aimedAt = working.transform.position; aimedFacing = working.transform.forward;
        }

        // What the miner itself offers: to rest from its work; resting at its boulder, to go back to it.
        public void OptionsForMiner(System.Collections.Generic.List<InteractionClick.Option> into)
        {
            if (Swinging) into.Add(new InteractionClick.Option { label = "Rest", act = Rest });
            else if (mining != null && Carrying && hands != null && hands.Held != null && !carry.Laying && !carry.Fetching
                && !(working.Motor != null && working.Motor.IsMoving)) into.Add(new InteractionClick.Option { label = "Back to work", act = WorkHere });
        }

        // It stops its work and stands at ease, its pickaxe held as its strength allows.
        public void Rest()
        {
            if (!Showing || hands == null || hands.Held == null || Carrying) return;
            TakeAlong();
        }

        // It lays its pickaxe down on the ground beside it, and stands up without it.
        public void LayDown()
        {
            if (!Showing || hands == null || hands.Held == null) return;
            if (!Carrying) TakeAlong();
            if (Carrying) carry.LayDown();
        }

        // It goes to a pickaxe that lies in the world, bends down, and takes it up.
        public void PickUp(HeldThing thing)
        {
            if (thing == null || thing.Tool == null || thing.Holder != null) return;
            if (!Showing)
            {
                var unit = choice != null ? choice.Current : null;
                if (HangsBackFirst(unit, () => PickUp(thing))) return;
                if (unit == null || !unit.TryGetComponent<MinerBody>(out var body) || !unit.TryGetComponent<ProceduralBiped>(out var biped)
                    || !unit.TryGetComponent<PhysicalBody>(out var weighed) || !weighed.Ready || unit.GetComponent<PhysicalHands>() != null
                    || unit.GetComponent<EquippedTool>() != null) return;
                Equip(unit, biped, weighed, thing.Tool);
                swing.enabled = false;
                carry.enabled = true;
            }
            else if (hands == null || hands.Held != null || !Carrying) return;
            if (lying == thing) lying = null;
            carry.Fetch(thing);
        }

        private void ClearLying()
        {
            if (lying != null) Destroy(lying.gameObject);
            lying = null;
        }

        // It sets itself to work again: at its boulder, if it has one; or (the bench's block, for the tests) on a block
        // put before it where it has come to, the pickaxe taken up from however it was held.
        private void WorkHere()
        {
            if (working.Motor != null && working.Motor.IsMoving) return;
            // With a boulder to mine, its work is there.
            if (mining != null) { tries = 0; GoToRock(); return; }
            carry.enabled = false;
            swing.enabled = true;
            swing.TakeUp();
            setting = StartCoroutine(Set(working, swing.tool, true));
        }

        // How strong the chosen miner is (1: ordinary for its build). It takes effect at once, whether or not it has a
        // pickaxe in its hands.
        public void SetStrength(float value)
        {
            strength = Mathf.Clamp(value, MinerPanel.Weakest, MinerPanel.Strongest);
            Strengthen();
        }

        private void Strengthen()
        {
            var unit = choice != null ? choice.Current : null;
            if (strong != null && strong != unit && strong.TryGetComponent<PhysicalBody>(out var was)) was.Strength = 1;
            if (unit != null && unit.TryGetComponent<PhysicalBody>(out var now)) now.Strength = strength;
            strong = unit;
        }

        // The weight of the pickaxe the bench's block makes (Toggle), as a share of its own. The miner takes it up
        // afresh.
        public void SetWeight(float value)
        {
            value = Mathf.Clamp(value, .4f, 3);
            if (Mathf.Approximately(value, weight)) return;
            weight = value;
            if (!Showing) return;
            var unit = working;
            End();
            again = unit;
        }

        private void Begin(SelectableUnit unit)
        {
            if (unit == null || !unit.TryGetComponent<MinerBody>(out var body) || body.Pickaxe == null || !body.Pickaxe.HasWeight
                || !unit.TryGetComponent<ProceduralBiped>(out var biped) || !unit.TryGetComponent<PhysicalBody>(out var weighed) || !weighed.Ready
                || unit.GetComponent<PhysicalHands>() != null || unit.GetComponent<EquippedTool>() != null) return;
            if (HangsBackFirst(unit, () => Begin(unit))) return;
            if (unit.Motor != null) unit.Motor.Stop();
            // The bench makes a pickaxe for it: its own that lay somewhere is put away.
            ClearLying();
            Equip(unit, biped, weighed, body.Pickaxe);
            physical.Refresh();
            setting = StartCoroutine(Set(unit, body.Pickaxe, false));
        }

        // The miner is given what its physical work needs: hands, a back and a balance that work by its strength, and
        // the two plans of what to do with a tool (the swing, and the carry: one of them at a time).
        private void Equip(SelectableUnit unit, ProceduralBiped biped, PhysicalBody weighed, ToolDefinition pickaxe)
        {
            working = unit;
            physical = weighed;
            physical.Strength = strength;
            // How tall it stands (to its head's middle): what it can strike is measured against that.
            if (unit.TryGetComponent<MinerBody>(out var made) && made.Ready) tall = made.Rig.head.position.y - unit.transform.position.y;
            hands = unit.gameObject.AddComponent<PhysicalHands>();
            back = unit.gameObject.AddComponent<PhysicalBack>();
            balance = unit.gameObject.AddComponent<PhysicalBalance>();
            swing = unit.gameObject.AddComponent<PhysicalSwing>();
            swing.hands = hands; swing.back = back; swing.body = biped; swing.tool = pickaxe;
            carry = unit.gameObject.AddComponent<PhysicalCarry>();
            carry.enabled = false;
            carry.hands = hands; carry.back = back; carry.body = biped; carry.tool = pickaxe;
            // A body with real weights and a balance of its own can lose it: it can fall, and gets up.
            fall = unit.GetComponent<PhysicalFall>();
            if (fall == null) fall = unit.gameObject.AddComponent<PhysicalFall>();
            had = null; down = false;
        }

        // The miner bows to its work; the block is put under where the pick's head then rests; it takes up the pickaxe
        // (or, if it has it already, is by then bringing it to its work).
        private IEnumerator Set(SelectableUnit unit, ToolDefinition pickaxe, bool has)
        {
            balance.Brace(PhysicalSwing.StanceWider, PhysicalSwing.StanceStagger);
            back.Want(PhysicalSwing.RestBow);
            yield return new WaitForSeconds(has ? 1.1f : 1.3f);
            if (working != unit || swing == null) yield break;
            swing.Intend(PhysicalSwing.Rest, out var at, out var turned);
            Vector3 rests = at + turned * pickaxe.Head;
            float top = rests.y - pickaxe.HeadRadius - .015f;
            Vector3 facing = Vector3.ProjectOnPlane(unit.transform.forward, Vector3.up).normalized;
            rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = "Block (a look at the work)";
            rock.transform.SetPositionAndRotation(new Vector3(rests.x, top - .6f, rests.z), Quaternion.LookRotation(facing));
            rock.transform.localScale = new Vector3(.5f, 1.2f, .5f);
            var paint = Shader.Find("Wonder Gather/Painted");
            if (paint != null)
            {
                var stone = new Material(paint) { name = "Block (a look at the work)" };
                stone.SetColor("_BaseColor", new Color(.36f, .38f, .42f));
                rock.GetComponent<MeshRenderer>().sharedMaterial = stone;
            }
            if (!has) hands.Take(pickaxe, at, turned, weight);
            setting = null;
        }

        public void End()
        {
            again = null; then = null;
            LeaveRock();
            if (setting != null) { StopCoroutine(setting); setting = null; }
            if (hands != null)
            {
                // A pickaxe it was only reaching for is not its own yet: it stays where it lies.
                bool reaching = carry != null && carry.Fetching;
                var was = hands.Drop();
                if (was != null)
                {
                    if (reaching) lying = was.GetComponent<HeldThing>();
                    else Destroy(was.gameObject);
                }
            }
            if (carry != null)
            {
                // A pickaxe that lies stays in the world.
                if (carry.Lies != null) lying = carry.Lies.GetComponent<HeldThing>();
                Destroy(carry);
            }
            if (swing != null) Destroy(swing);
            if (back != null) Destroy(back);
            if (balance != null) Destroy(balance);
            if (hands != null) Destroy(hands);
            if (rock != null) Destroy(rock);
            if (working != null && working.TryGetComponent<ProceduralBiped>(out var biped)) { biped.Bow(0); biped.Sink(0); biped.SetBack(0); }
            if (physical != null) { physical.Strength = working == strong ? strength : 1; physical.Refresh(); }
            working = null; physical = null; hands = null; back = null; balance = null; swing = null; carry = null; rock = null;
        }

        // What the chosen miner is doing, in a line (the panel shows it). It is made once a frame.
        private string status = "";
        private int statusOf = -1;
        public string Status()
        {
            if (statusOf != Time.frameCount) { status = Doing(); statusOf = Time.frameCount; }
            return status;
        }

        private string Doing()
        {
            bool tells = Time.time - saidAt < SaidFor;
            if (!Showing)
            {
                if (tells) return said + ".";
                return NearestLying(Vector3.zero) != null ? "Nothing in its hands. A pickaxe lies on the ground: Space and a click on it, to pick it up."
                    : "Nothing in its hands, and no pickaxe on the ground.";
            }
            if (down) return "It fell: its pickaxe lies where it left its hands.";
            string kilograms = hands != null && hands.Held != null ? $"Pickaxe {hands.ToolMass:0.0} kg" : "Its pickaxe";
            string line;
            if (Carrying)
            {
                line = carry.Fetching ? "Going to pick a pickaxe up"
                    : carry.Laying ? kilograms + ": laying it down"
                    : hands == null || hands.Held == null ? (back != null && back.BowNow > 30 && back.Effort > .9f ? "Its pickaxe lies on the ground   ·   its back does not raise it: it is too weak, or too spent" : "Its pickaxe lies on the ground")
                    : carry.way == PhysicalCarry.Way.Left ? kilograms + ": left where it lay, too heavy to move"
                    : carry.way == PhysicalCarry.Way.Dragged ? $"{kilograms}: too heavy for its hand, dragged at {carry.Pace * 100:0}% of its pace"
                    : $"{kilograms}: carried in one hand ({carry.Asks * 100:0}% of its hold)";
                if (mining != null) line += atRock ? "   ·   resting at its boulder" : "   ·   going to a boulder";
            }
            else if (Swinging)
            {
                line = $"{kilograms}   ·   spent {swing.Spent * 100:0}%";
                if (swing.results.Count > 0)
                {
                    var last = swing.results[swing.results.Count - 1];
                    line += last.struck ? $"   ·   last blow {last.speed:0.0} m/s, {last.energy:0} J" : "   ·   last swing did not strike";
                }
                if (swing.phase == PhysicalSwing.Phase.Rest) line += "   ·   getting its breath";
                if (mining != null) line += $"   ·   {mining.Stones.Count} stones off this boulder";
            }
            else line = "Taking up its pickaxe";
            return tells ? line + "\n" + said + "." : line;
        }
    }
}
