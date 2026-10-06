using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace WonderGather
{
    // A look at the miners at work, before where and how they will mine is decided (Luis's choices M1 to M4). With K a
    // plain block of rock stands in front of the chosen miner, and it mines it with the pickaxe made for it; K again
    // (or walking away, or choosing another miner) puts the rock and the pickaxe away. Nothing here is the game's
    // mining yet: there is no place for it, nothing is hauled, and where a pickaxe is kept is not decided. It is here
    // so that the swing and the hands can be seen in play.
    [RequireComponent(typeof(MinerChoice))]
    public sealed class MinerWorkPreview : MonoBehaviour
    {
        private MinerChoice choice;
        private InputAction toggle;
        private SelectableUnit working;
        private Gatherer gatherer;
        private EquippedTool tool;
        private GameObject rock, home;
        private ResourceNode node;
        private Vector3 stand;
        private SelectableUnit again;
        private GUIStyle note;
        public bool Showing => working != null;
        public EquippedTool Tool => tool;

        private void Awake()
        {
            choice = GetComponent<MinerChoice>();
            toggle = new InputAction("Watch the miner work", InputActionType.Button, "<Keyboard>/k");
        }
        private void OnEnable() => toggle?.Enable();
        private void OnDisable() { toggle?.Disable(); End(); }
        private void OnDestroy() => toggle?.Dispose();

        // A rock face to mine: a block whose near face stands `reach` in front of a place to stand, facing it, with
        // a place to deliver to. What is seen (if it is) and what is struck are the same block.
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

        // How far in front of a body its rock face stands: where the head's swing meets it, a little short of the
        // swing's farthest reach.
        public static float FaceDistance(ProceduralBiped body, ToolDefinition pickaxe)
            => (body.ToolHand.z + (pickaxe.Head - pickaxe.PrimaryGrip).magnitude) * .78f;

        private void Update()
        {
            if (toggle != null && toggle.WasPressedThisFrame()) Toggle();
            // Beginning again waits until the last look's own parts are gone (they go at the end of a frame).
            if (again != null && !Showing)
            {
                var unit = again;
                if (choice.Current != unit || !unit.gameObject.activeInHierarchy) again = null;
                else if (unit.GetComponent<Gatherer>() == null && unit.GetComponent<EquippedTool>() == null) { again = null; Begin(unit); }
            }
            if (!Showing) return;
            // Chosen another, sent elsewhere, or walked off: the look is over.
            if (choice.Current != working || !working.gameObject.activeInHierarchy || gatherer == null || gatherer.State == Gatherer.Activity.Idle
                || Vector3.ProjectOnPlane(working.transform.position - stand, Vector3.up).sqrMagnitude > .09f) { End(); return; }
            // Nothing is hauled yet: when its arms are full it puts everything down and begins again.
            if (gatherer.State == Gatherer.Activity.ToDepot || gatherer.State == Gatherer.Activity.WaitingForDepot)
            {
                var unit = working;
                End();
                if (unit.Motor != null) unit.Motor.Stop();
                again = unit;
            }
        }

        // K: begin the look, or end it.
        public void Toggle()
        {
            if (Showing || again != null) { again = null; End(); }
            else Begin(choice != null ? choice.Current : null);
        }

        private void Begin(SelectableUnit unit)
        {
            if (unit == null || !unit.TryGetComponent<MinerBody>(out var body) || body.Pickaxe == null
                || !unit.TryGetComponent<ProceduralBiped>(out var biped) || unit.GetComponent<Gatherer>() != null || unit.GetComponent<EquippedTool>() != null) return;
            if (unit.Motor != null) unit.Motor.Stop();
            Vector3 facing = Vector3.ProjectOnPlane(unit.transform.forward, Vector3.up).normalized;
            stand = unit.transform.position;
            gatherer = unit.gameObject.AddComponent<Gatherer>();
            tool = unit.gameObject.AddComponent<EquippedTool>();
            tool.SetDefinition(body.Pickaxe);
            biped.ConfigureWork(gatherer, null);
            float tall = body.Rig.head != null ? body.Rig.head.position.y - unit.transform.position.y : 1.2f;
            Vector3 behind = stand - facing * 3;
            if (NavMesh.SamplePosition(behind, out var hit, 3, NavMesh.AllAreas)) behind = hit.position;
            ResourceDepot depot;
            (node, depot, rock) = RockFace(stand, facing, FaceDistance(biped, body.Pickaxe), behind, true, tall * .95f);
            home = depot.gameObject;
            var paint = Shader.Find("Wonder Gather/Painted");
            var seen = rock.GetComponentInChildren<MeshRenderer>();
            if (paint != null && seen != null)
            {
                var stone = new Material(paint) { name = "Preview rock" };
                stone.SetColor("_BaseColor", new Color(.36f, .38f, .42f));
                seen.sharedMaterial = stone;
            }
            gatherer.SetPerformance(new UnitPerformance(100, 20, 100, 100));
            gatherer.Configure(depot, Vector3.zero);
            working = unit;
            if (!gatherer.Gather(node)) End();
        }

        public void End()
        {
            if (gatherer != null) gatherer.CancelOrder();
            if (working != null && working.TryGetComponent<ProceduralBiped>(out var biped)) biped.ConfigureWork(null, null);
            if (tool != null) Destroy(tool);
            if (gatherer != null) Destroy(gatherer);
            if (rock != null) Destroy(rock);
            if (home != null) Destroy(home);
            working = null; gatherer = null; tool = null; rock = null; home = null; node = null;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || choice == null || choice.Open || choice.Count == 0) return;
            note ??= new GUIStyle(GUI.skin.label) { fontSize = 11, alignment = TextAnchor.UpperCenter };
            GUI.Label(new Rect(Screen.width * .5f - 160, Screen.height - 48, 320, 20), Showing || again != null ? "K: put the pickaxe away" : "K: watch it mine (a first look)", note);
        }
    }
}
