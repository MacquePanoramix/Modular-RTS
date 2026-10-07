using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WonderGather
{
    // The interaction click (S3, step 8: Docs/Design/ThePhysicalBody.md, section 6). The ordinary clicks keep doing the
    // ordinary RTS things. With the space bar held (or tapped, for the next click), what can be interacted with shows
    // itself softly; a click on one of them opens its options beside it, in a small box, with a cancel. Escape, or a
    // click elsewhere, closes it.
    //
    // Each option is an order to the chosen miner, done by its body. What the options are is asked of the look at the
    // work (MinerWorkPreview), which is what gives a miner its physical work in this prototype.
    //
    // The key is one setting (Space is a proposal, not yet Luis's choice).
    [RequireComponent(typeof(MinerChoice), typeof(MinerWorkPreview))]
    public sealed class InteractionClick : MonoBehaviour
    {
        // How near the pointer a thing must be to be the one clicked, in pixels; and how short a press of the key is a
        // tap (seconds).
        private const float Near = 48, Tap = .3f;
        public struct Option
        {
            public string label;
            public System.Action act;
        }

        private MinerChoice choice;
        private MinerWorkPreview look;
        private RtsInput input;
        private Camera view;
        private InputAction key, click, escape, point;
        private bool latched, usedWhileHeld;
        private float pressedAt;
        private Component target;
        private string title;
        private readonly List<Option> options = new List<Option>();
        private readonly List<Component> things = new List<Component>();
        private Rect box;
        private GUIStyle panel, heading, button, soft;

        // The key is held, or was tapped and waits for its click.
        public bool Armed => latched || (key != null && key.IsPressed());
        public bool IsOpen => target != null;
        public Component Target => target;
        public int Count => options.Count;
        public string Label(int index) => options[index].label;
        // While it is armed or open, the ordinary clicks do nothing.
        public bool Blocks => Armed || IsOpen;

        private void Awake()
        {
            choice = GetComponent<MinerChoice>();
            look = GetComponent<MinerWorkPreview>();
            key = new InputAction("Interact", InputActionType.Button, "<Keyboard>/space");
            click = new InputAction("Interaction click", InputActionType.Button, "<Mouse>/leftButton");
            escape = new InputAction("Close", InputActionType.Button, "<Keyboard>/escape");
            point = new InputAction("Pointer", InputActionType.PassThrough, "<Mouse>/position");
        }

        private void OnEnable()
        {
            key?.Enable(); click?.Enable(); escape?.Enable(); point?.Enable();
            input = FindAnyObjectByType<RtsInput>();
            if (input != null) input.SetInterfaceBlocker(_ => Blocks);
        }

        private void OnDisable()
        {
            key?.Disable(); click?.Disable(); escape?.Disable(); point?.Disable();
            if (input != null) input.SetInterfaceBlocker(null);
            Close(); latched = false;
        }

        private void OnDestroy() { key?.Dispose(); click?.Dispose(); escape?.Dispose(); point?.Dispose(); }

        // For the next click, as a tap of the key does.
        public void Arm(bool on) { latched = on; }

        // What can be interacted with now: the pickaxes in the world (held or lying), and the chosen miner.
        public IReadOnlyList<Component> Things()
        {
            things.Clear();
            foreach (var thing in FindObjectsByType<HeldThing>(FindObjectsSortMode.None)) if (thing.Tool != null) things.Add(thing);
            if (choice != null && choice.Current != null) things.Add(choice.Current);
            return things;
        }

        // Where a thing is, for the eye and the pointer.
        public static Vector3 Place(Component thing)
        {
            if (thing is HeldThing held && held.TryGetComponent<Rigidbody>(out var body)) return body.worldCenterOfMass;
            if (thing is SelectableUnit unit && unit.TryGetComponent<ProceduralBiped>(out var biped) && biped.Ready) return biped.HipsNow;
            return thing.transform.position;
        }

        public string Named(Component thing)
        {
            if (thing is HeldThing held) return held.Tool != null ? held.Tool.DisplayName.ToLowerInvariant() : "tool";
            if (thing is SelectableUnit && choice != null) return choice.NameOf(choice.Chosen);
            return thing.name;
        }

        // The thing nearest a point of the screen, if one is near enough. A thing is preferred to the miner that holds it.
        public Component Pick(Vector2 screen)
        {
            if (view == null) view = Camera.main;
            if (view == null) return null;
            Component best = null;
            float nearest = Near;
            foreach (var thing in Things())
            {
                Vector3 at = view.WorldToScreenPoint(Place(thing));
                if (at.z <= 0) continue;
                float away = Vector2.Distance(new Vector2(at.x, at.y), screen) * (thing is HeldThing ? .7f : 1);
                if (away < nearest) { nearest = away; best = thing; }
            }
            return best;
        }

        // Opens a thing's options. False if it has none.
        public bool OpenOn(Component thing)
        {
            Close();
            if (thing == null) return false;
            options.Clear();
            if (thing is HeldThing held) look.OptionsFor(held, options);
            else if (thing is SelectableUnit unit && unit == choice.Current) look.OptionsForMiner(options);
            if (options.Count == 0) return false;
            target = thing;
            title = Named(thing);
            latched = false;
            return true;
        }

        // Gives the order an option names, and closes. False if there is no such option.
        public bool Choose(string label)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].label != label) continue;
                var act = options[i].act;
                Close();
                act?.Invoke();
                return true;
            }
            return false;
        }

        public void Close() { target = null; options.Clear(); }

        private void Update()
        {
            if (key.WasPressedThisFrame()) { pressedAt = Time.unscaledTime; usedWhileHeld = false; }
            if (key.WasReleasedThisFrame()) latched = !usedWhileHeld && !IsOpen && Time.unscaledTime - pressedAt < Tap;
            if (escape.WasPressedThisFrame()) { Close(); latched = false; }
            if (target == null && IsOpen) Close();
            if (!click.WasPressedThisFrame()) return;
            Vector2 pointer = point.ReadValue<Vector2>();
            if (IsOpen)
            {
                // A click elsewhere closes it (the box's own buttons answer for themselves).
                if (!box.Contains(new Vector2(pointer.x, Screen.height - pointer.y))) Close();
                return;
            }
            if (!Armed) return;
            usedWhileHeld = true;
            if (!OpenOn(Pick(pointer))) latched = false;
        }

        private void Styles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box) { padding = new RectOffset(8, 8, 6, 8) };
            heading = new GUIStyle(GUI.skin.label) { fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            button = new GUIStyle(GUI.skin.button) { fontSize = 13, fixedHeight = 26 };
            soft = new GUIStyle(GUI.skin.box) { fontSize = 11, alignment = TextAnchor.MiddleCenter };
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || choice == null || choice.Open) return;
            if (view == null) view = Camera.main;
            if (view == null) return;
            Styles();
            if (IsOpen)
            {
                Vector3 at = view.WorldToScreenPoint(Place(target));
                float width = 150, height = 30 + (options.Count + 1) * 30;
                box = new Rect(Mathf.Clamp(at.x + 28, 4, Screen.width - width - 4), Mathf.Clamp(Screen.height - at.y - height * .5f, 4, Screen.height - height - 4), width, height);
                GUI.Box(box, GUIContent.none, panel);
                GUI.Label(new Rect(box.x, box.y + 4, box.width, 20), title, heading);
                float y = box.y + 28;
                for (int i = 0; i < options.Count; i++, y += 30)
                    if (GUI.Button(new Rect(box.x + 8, y, box.width - 16, 26), options[i].label, button)) { Choose(options[i].label); return; }
                if (GUI.Button(new Rect(box.x + 8, y, box.width - 16, 26), "Cancel", button)) Close();
                return;
            }
            if (!Armed) return;
            // What can be interacted with shows itself softly.
            foreach (var thing in Things())
            {
                Vector3 at = view.WorldToScreenPoint(Place(thing));
                if (at.z <= 0) continue;
                string name = Named(thing);
                float width = 18 + name.Length * 7;
                GUI.Box(new Rect(at.x - width * .5f, Screen.height - at.y - 34, width, 20), name, soft);
            }
        }
    }
}
