using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WonderGather
{
    // A look at the miners at work with real weight (S3: Docs/Design/ThePhysicalBody.md), in play. With K a plain block
    // of rock stands before the chosen miner, and it swings the pickaxe made for it down on the block, by its own
    // strength: the pickaxe is a real body, the arms and the back give what they can, they tire, and the body keeps its
    // own balance (PhysicalHands, PhysicalBack, PhysicalBalance, PhysicalSwing). The comma and the full stop make the miner weaker and stronger; minus and equals
    // make its pickaxe lighter and heavier. K again (or walking away, or choosing another miner) puts the block and
    // the pickaxe away.
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
        private GameObject rock;
        private Coroutine setting;
        private float strength = 1, weight = 1;
        private GUIStyle note;
        public bool Showing => working != null || again != null;
        // The swing being shown (its results, its phase), once the miner has taken up its pickaxe.
        public PhysicalSwing Swing => swing;
        public bool Swinging => swing != null && hands != null && hands.Held != null;
        public float Strength => strength;
        public float Weight => weight;

        private void Awake()
        {
            choice = GetComponent<MinerChoice>();
            toggle = new InputAction("Watch the miner work", InputActionType.Button, "<Keyboard>/k");
            weaker = new InputAction("Weaker", InputActionType.Button, "<Keyboard>/comma");
            stronger = new InputAction("Stronger", InputActionType.Button, "<Keyboard>/period");
            lighter = new InputAction("Lighter pickaxe", InputActionType.Button, "<Keyboard>/minus");
            heavier = new InputAction("Heavier pickaxe", InputActionType.Button, "<Keyboard>/equals");
        }
        private void OnEnable() { toggle?.Enable(); weaker?.Enable(); stronger?.Enable(); lighter?.Enable(); heavier?.Enable(); }
        private void OnDisable() { toggle?.Disable(); weaker?.Disable(); stronger?.Disable(); lighter?.Disable(); heavier?.Disable(); End(); }
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
            if (!Showing) return;
            // Chosen another, or sent somewhere: the look is over. A step it takes to keep its feet is not walking away.
            if (choice.Current != working || !working.gameObject.activeInHierarchy || (working.Motor != null && working.Motor.IsMoving)) { End(); return; }
            if (weaker.WasPressedThisFrame()) SetStrength(strength / 1.25f);
            if (stronger.WasPressedThisFrame()) SetStrength(strength * 1.25f);
            if (lighter.WasPressedThisFrame()) SetWeight(weight / 1.25f);
            if (heavier.WasPressedThisFrame()) SetWeight(weight * 1.25f);
        }

        // K: begin the look, or end it.
        public void Toggle()
        {
            if (Showing) End();
            else Begin(choice != null ? choice.Current : null);
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
            if (unit.Motor != null) unit.Motor.Stop();
            working = unit;
            physical = weighed;
            physical.Strength = strength;
            physical.Refresh();
            hands = unit.gameObject.AddComponent<PhysicalHands>();
            back = unit.gameObject.AddComponent<PhysicalBack>();
            balance = unit.gameObject.AddComponent<PhysicalBalance>();
            swing = unit.gameObject.AddComponent<PhysicalSwing>();
            swing.hands = hands; swing.back = back; swing.body = biped; swing.tool = body.Pickaxe;
            setting = StartCoroutine(Set(unit, body.Pickaxe));
        }

        // The miner bows to its work; the block is put under where the pick's head then rests; it takes up the pickaxe.
        private IEnumerator Set(SelectableUnit unit, ToolDefinition pickaxe)
        {
            balance.Brace(PhysicalSwing.StanceWider, PhysicalSwing.StanceStagger);
            back.Want(PhysicalSwing.RestBow);
            yield return new WaitForSeconds(1.3f);
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
            hands.Take(pickaxe, at, turned, weight);
            setting = null;
        }

        public void End()
        {
            again = null;
            if (setting != null) { StopCoroutine(setting); setting = null; }
            if (hands != null)
            {
                var was = hands.Drop();
                if (was != null) Destroy(was.gameObject);
            }
            if (swing != null) Destroy(swing);
            if (back != null) Destroy(back);
            if (balance != null) Destroy(balance);
            if (hands != null) Destroy(hands);
            if (rock != null) Destroy(rock);
            if (working != null && working.TryGetComponent<ProceduralBiped>(out var biped)) { biped.Bow(0); biped.Sink(0); biped.SetBack(0); }
            if (physical != null) { physical.Strength = 1; physical.Refresh(); }
            working = null; physical = null; hands = null; back = null; balance = null; swing = null; rock = null;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || choice == null || choice.Open || choice.Count == 0) return;
            note ??= new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.UpperCenter };
            var place = new Rect(Screen.width * .5f - 330, Screen.height - 48, 660, 20);
            if (!Showing) { GUI.Label(place, "K: watch it work, with real weight", note); return; }
            string line = $"K: put the pickaxe away   , . strength {strength:0.00}   - = pickaxe x{weight:0.00}";
            if (Swinging)
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
