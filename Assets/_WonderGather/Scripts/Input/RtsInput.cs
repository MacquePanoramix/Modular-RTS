using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace WonderGather
{
    // One action-map owner. World systems consume intent, never keyboard state.
    [DefaultExecutionOrder(-50)]
    public sealed class RtsInput : MonoBehaviour
    {
        // A right-button press that travels less than this is a click (an order), not a look.
        public const float ClickTravel = 6;
        private InputActionMap map;
        private WandererHud hud;
        private System.Func<Vector2,bool> interfaceBlocker;
        public void SetInterfaceBlocker(System.Func<Vector2,bool> blocksPointer)=>interfaceBlocker=blocksPointer;
        private InputAction pan, rotate, zoom, point, select, move, clear, focus, additive, build, train;
        private InputAction toggleCamera, delta, middle, alt, slow;
        private bool rightTracking, middleTracking, leftOrbit, captured, reportedWheel;
        private float rightTravel;
        private int rightClickFrame = -1, settleFrames;
        private Vector2 capturedAt;
        // Set by the camera: in Explore, a right click orders on release so a right drag can look.
        public bool ExploreMode { get; set; }
        public Vector2 Pan => Active ? pan.ReadValue<Vector2>() : Vector2.zero;
        public float Rotate => Active ? rotate.ReadValue<float>() : 0;
        public float Zoom => WorldPointer ? zoom.ReadValue<Vector2>().y : 0;
        public Vector2 Pointer => point.ReadValue<Vector2>();
        public bool SelectPressed => WorldPointer && select.WasPressedThisFrame() && !(ExploreMode && alt.IsPressed());
        public bool SelectReleased => Active && select.WasReleasedThisFrame();
        public bool SelectHeld => Active && select.IsPressed();
        public bool Additive => Active && additive.IsPressed();
        public bool CanSelectWorld => WorldPointer;
        public bool MovePressed => ExploreMode ? rightClickFrame == Time.frameCount && WorldPointer
                                               : WorldPointer && move.WasPressedThisFrame();
        public bool ClearPressed => Active && clear.WasPressedThisFrame();
        public bool TrainPressed=>Active && train.WasPressedThisFrame();
        public bool BuildPressed => Active && build.WasPressedThisFrame();
        public bool FocusPressed => Active && focus.WasPressedThisFrame();
        private bool Active => isActiveAndEnabled && Application.isFocused;
        private bool WorldPointer => Active && Pointer.x >= 0 && Pointer.y >= 0
            && Pointer.x < Screen.width && Pointer.y < Screen.height
            && !(interfaceBlocker!=null && interfaceBlocker(Pointer))
            && !(hud != null && hud.isActiveAndEnabled && hud.ContainsScreenPoint(Pointer))
            && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());
        // Edge scrolling only in full screen, where the cursor cannot leave the game.
        private bool PointerInWindow => Active && !Application.isBatchMode && Screen.fullScreen && Mouse.current != null
            && Pointer.x >= 0 && Pointer.y >= 0 && Pointer.x < Screen.width && Pointer.y < Screen.height;

        public CameraIntent ReadCameraIntent()
        {
            var intent = new CameraIntent();
            if (!Active) return intent;
            intent.Pan = pan.ReadValue<Vector2>();
            intent.Rotate = rotate.ReadValue<float>();
            intent.Zoom = Zoom;
            if (!reportedWheel && Mathf.Abs(intent.Zoom) > 0)
            {
                // Records the platform's wheel scale once, so zoom tuning can be checked from a player log.
                reportedWheel = true;
                Debug.Log("Wonder Gather camera: first wheel delta " + intent.Zoom);
            }
            intent.Pointer = Pointer;
            intent.PointerDelta = settleFrames > 0 ? Vector2.zero : delta.ReadValue<Vector2>();
            intent.PointerInWorld = WorldPointer;
            intent.PointerInWindow = PointerInWindow;
            intent.Look = rightTracking && move.IsPressed();
            intent.LookDragging = intent.Look && rightTravel >= ClickTravel;
            intent.Middle = middleTracking && middle.IsPressed();
            intent.Alt = alt.IsPressed();
            intent.AltOrbit = leftOrbit && select.IsPressed();
            intent.Fast = additive.IsPressed();
            intent.Slow = slow.IsPressed();
            intent.Toggle = toggleCamera.WasPressedThisFrame();
            intent.Focus = focus.WasPressedThisFrame();
            return intent;
        }

        // Hides and holds the cursor while looking, then returns it to where the look began.
        public void CaptureCursor(bool capture)
        {
            if (capture == captured || Application.isBatchMode) return;
            captured = capture;
            settleFrames = 2;
            if (capture)
            {
                capturedAt = Pointer;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (Mouse.current != null) Mouse.current.WarpCursorPosition(capturedAt);
            }
        }

        private void Awake()
        {
            hud = GetComponent<WandererHud>();
            map = new InputActionMap("Gameplay");
            pan = map.AddAction("Pan", InputActionType.Value);
            pan.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            pan.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            rotate = map.AddAction("Rotate", InputActionType.Value);
            rotate.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/q").With("Positive", "<Keyboard>/e");
            zoom = map.AddAction("Zoom", InputActionType.PassThrough, "<Mouse>/scroll");
            point = map.AddAction("Point", InputActionType.PassThrough, "<Mouse>/position");
            select = map.AddAction("Select", InputActionType.Button, "<Mouse>/leftButton");
            move = map.AddAction("Move", InputActionType.Button, "<Mouse>/rightButton");
            clear = map.AddAction("Clear", InputActionType.Button, "<Keyboard>/escape");
            train=map.AddAction("Train",InputActionType.Button,"<Keyboard>/t");
            build = map.AddAction("Build", InputActionType.Button, "<Keyboard>/b");
            focus = map.AddAction("Focus", InputActionType.Button, "<Keyboard>/f");
            additive = map.AddAction("Additive", InputActionType.Button, "<Keyboard>/leftShift");
            additive.AddBinding("<Keyboard>/rightShift");
            toggleCamera = map.AddAction("ToggleCamera", InputActionType.Button, "<Keyboard>/v");
            delta = map.AddAction("PointerDelta", InputActionType.PassThrough, "<Mouse>/delta");
            middle = map.AddAction("Middle", InputActionType.Button, "<Mouse>/middleButton");
            alt = map.AddAction("Alt", InputActionType.Button, "<Keyboard>/alt");
            slow = map.AddAction("Slow", InputActionType.Button, "<Keyboard>/ctrl");
        }

        private void Update()
        {
            if (settleFrames > 0) settleFrames--;
            if (!Active) { rightTracking = middleTracking = leftOrbit = false; return; }
            if (move.WasPressedThisFrame()) { rightTracking = WorldPointer; rightTravel = 0; }
            if (rightTracking && move.IsPressed()) rightTravel += delta.ReadValue<Vector2>().magnitude;
            if (move.WasReleasedThisFrame())
            {
                rightClickFrame = rightTracking && rightTravel < ClickTravel ? Time.frameCount : -1;
                rightTracking = false;
            }
            if (middle.WasPressedThisFrame()) middleTracking = WorldPointer;
            if (!middle.IsPressed()) middleTracking = false;
            if (select.WasPressedThisFrame()) leftOrbit = ExploreMode && alt.IsPressed() && WorldPointer;
            if (!select.IsPressed()) leftOrbit = false;
        }

        private void OnEnable() => map.Enable();
        private void OnDisable() { CaptureCursor(false); map.Disable(); }
        private void OnDestroy() => map.Dispose();
    }
}
