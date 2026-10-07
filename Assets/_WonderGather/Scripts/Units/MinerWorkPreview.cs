using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WonderGather
{
    // A look at the miners at work with real weight (S3: Docs/Design/ThePhysicalBody.md), in play. With K a plain block
    // of rock stands before the chosen miner, and it swings the pickaxe made for it down on the block, by its own
    // strength: the pickaxe is a real body, the arms and the back give what they can, they tire, and the body keeps its
    // own balance (PhysicalHands, PhysicalBack, PhysicalBalance, PhysicalSwing). The comma and the full stop make the
    // miner weaker and stronger; minus and equals make its pickaxe lighter and heavier.
    //
    // Sent somewhere while it works, it takes its pickaxe with it, held as its strength allows (PhysicalCarry): in one
    // hand at its side, or dragged. K there puts it to work again, on a block where it stands. K at work (or choosing
    // another miner) puts the block and the pickaxe away.
    //
    // With the interaction click (InteractionClick: the space bar and a click) the pickaxe can be laid down on the
    // ground and picked up again, and the miner told to rest: each done by its body. A pickaxe that lies (laid down,
    // or left because it was too heavy) stays in the world.
    //
    // It is the bench, brought into the place so that it can be watched and tried. It is not the game's mining yet:
    // nothing is mined, and any boulder by a click is a later step.
    [RequireComponent(typeof(MinerChoice))]
    public sealed class MinerWorkPreview : MonoBehaviour
    {
        private MinerChoice choice;
        private InputAction toggle, weaker, stronger, lighter, heavier;
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
        private Coroutine setting;
        // What a miner with something in its hand was told to do with its pickaxe: done once that is hung back.
        private System.Action then;
        private SelectableUnit thenFor;
        private float strength = 1, weight = 1;
        private GUIStyle note;
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
            // The interaction click lives beside the look until the place is set up with it (step 11).
            if (GetComponent<InteractionClick>() == null) gameObject.AddComponent<InteractionClick>();
            toggle = new InputAction("Watch the miner work", InputActionType.Button, "<Keyboard>/k");
            weaker = new InputAction("Weaker", InputActionType.Button, "<Keyboard>/comma");
            stronger = new InputAction("Stronger", InputActionType.Button, "<Keyboard>/period");
            lighter = new InputAction("Lighter pickaxe", InputActionType.Button, "<Keyboard>/minus");
            heavier = new InputAction("Heavier pickaxe", InputActionType.Button, "<Keyboard>/equals");
        }
        private void OnEnable() { toggle?.Enable(); weaker?.Enable(); stronger?.Enable(); lighter?.Enable(); heavier?.Enable(); }
        private void OnDisable() { toggle?.Disable(); weaker?.Disable(); stronger?.Disable(); lighter?.Disable(); heavier?.Disable(); End(); ClearLying(); }
        private void OnDestroy() { toggle?.Dispose(); weaker?.Dispose(); stronger?.Dispose(); lighter?.Dispose(); heavier?.Dispose(); }

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
            if (toggle != null && toggle.WasPressedThisFrame()) Toggle();
            if (then != null)
            {
                var has = thenFor != null ? thenFor.GetComponent<ThingsInHand>() : null;
                if (choice.Current != thenFor) then = null;
                else if (has == null || (has.Has == null && !has.Busy)) { var act = then; then = null; act(); }
            }
            if (!Showing) return;
            // Chosen another: the look is over.
            if (choice.Current != working || !working.gameObject.activeInHierarchy) { End(); return; }
            // Sent somewhere while it works: it takes its pickaxe with it. (A step it takes to keep its feet is not
            // being sent anywhere.)
            if (!Carrying && working.Motor != null && working.Motor.IsMoving) { TakeAlong(); if (!Showing) return; }
            // Its pickaxe lies on the ground (laid down, left, or not reached), and it has stood up: it has nothing
            // more to do with it until it is told to pick it up.
            if (Carrying && hands != null && hands.Held == null && !carry.Fetching && !carry.Laying
                && working.TryGetComponent<ProceduralBiped>(out var stands) && Mathf.Abs(stands.BowNow) < 3 && stands.SinkNow < .01f) { End(); return; }
            if (weaker.WasPressedThisFrame()) SetStrength(strength / 1.25f);
            if (stronger.WasPressedThisFrame()) SetStrength(strength * 1.25f);
            if (lighter.WasPressedThisFrame()) SetWeight(weight / 1.25f);
            if (heavier.WasPressedThisFrame()) SetWeight(weight * 1.25f);
        }

        // K: begin the look; at work, end it; carrying, put the pickaxe to work where the miner stands.
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
            then = after; thenFor = unit;
            return true;
        }

        // What the miner itself offers: to rest from its work; to go back to it.
        public void OptionsForMiner(System.Collections.Generic.List<InteractionClick.Option> into)
        {
            if (Swinging) into.Add(new InteractionClick.Option { label = "Rest", act = Rest });
            else if (Carrying && hands != null && hands.Held != null && !carry.Laying && !carry.Fetching
                && !(working.Motor != null && working.Motor.IsMoving)) into.Add(new InteractionClick.Option { label = "Work here", act = WorkHere });
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

        // Where it has come to, it sets itself to work again: a block before it, and the pickaxe taken up from however
        // it was held.
        private void WorkHere()
        {
            if (working.Motor != null && working.Motor.IsMoving) return;
            carry.enabled = false;
            swing.enabled = true;
            swing.TakeUp();
            setting = StartCoroutine(Set(working, swing.tool, true));
        }

        // How strong the miner is (1: ordinary for its build). It takes effect at once.
        public void SetStrength(float value)
        {
            strength = Mathf.Clamp(value, .3f, 3);
            if (physical != null) physical.Strength = strength;
        }

        // The pickaxe's weight, as a share of its own. The miner takes it up afresh.
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
            // K makes a pickaxe for it: one that lay somewhere is put away.
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
            hands = unit.gameObject.AddComponent<PhysicalHands>();
            back = unit.gameObject.AddComponent<PhysicalBack>();
            balance = unit.gameObject.AddComponent<PhysicalBalance>();
            swing = unit.gameObject.AddComponent<PhysicalSwing>();
            swing.hands = hands; swing.back = back; swing.body = biped; swing.tool = pickaxe;
            carry = unit.gameObject.AddComponent<PhysicalCarry>();
            carry.enabled = false;
            carry.hands = hands; carry.back = back; carry.body = biped; carry.tool = pickaxe;
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
            if (physical != null) { physical.Strength = 1; physical.Refresh(); }
            working = null; physical = null; hands = null; back = null; balance = null; swing = null; carry = null; rock = null;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || choice == null || choice.Open || choice.Count == 0) return;
            note ??= new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.UpperCenter };
            var place = new Rect(Screen.width * .5f - 330, Screen.height - 48, 660, 20);
            if (!Showing)
            {
                GUI.Label(place, lying != null ? "K: watch it work, with real weight   ·   its pickaxe lies on the ground: Space and a click on it, to pick it up"
                    : "K: watch it work, with real weight", note);
                return;
            }
            string line = $"{(Carrying && hands != null && hands.Held != null ? "K: work here" : "K: put the pickaxe away")}   Space + click: its options   , . strength {strength:0.00}   - = pickaxe x{weight:0.00}";
            if (Carrying)
            {
                line += carry.way == PhysicalCarry.Way.Left ? "   it left the pickaxe where it lay: too heavy to move"
                    : carry.way == PhysicalCarry.Way.Dragged ? $" ({hands.ToolMass:0.0} kg)   too heavy for its hand: dragged, at {carry.Pace * 100:0}% of its pace"
                    : $" ({hands.ToolMass:0.0} kg)   carried in one hand ({carry.Asks * 100:0}% of its hold)";
            }
            else if (Swinging)
            {
                line += $" ({hands.ToolMass:0.0} kg)   spent {swing.Spent * 100:0}%";
                if (swing.results.Count > 0)
                {
                    var last = swing.results[swing.results.Count - 1];
                    line += last.struck ? $"   last blow {last.speed:0.0} m/s" : "   last swing did not strike";
                }
                if (swing.phase == PhysicalSwing.Phase.Rest) line += "   (resting)";
            }
            GUI.Label(place, line, note);
        }
    }
}
