using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace WonderGather
{
    // One action-map owner. World systems consume intent, never keyboard state.
    public sealed class RtsInput : MonoBehaviour
    {
        private InputActionMap map;
        private WandererHud hud;
        private InputAction pan, rotate, zoom, point, select, move, clear, focus, additive;
        public Vector2 Pan => Active ? pan.ReadValue<Vector2>() : Vector2.zero;
        public float Rotate => Active ? rotate.ReadValue<float>() : 0;
        public float Zoom => WorldPointer ? zoom.ReadValue<Vector2>().y : 0;
        public Vector2 Pointer => point.ReadValue<Vector2>();
        public bool SelectPressed => WorldPointer && select.WasPressedThisFrame();
        public bool SelectReleased => Active && select.WasReleasedThisFrame();
        public bool SelectHeld => Active && select.IsPressed();
        public bool Additive => Active && additive.IsPressed();
        public bool CanSelectWorld => WorldPointer;
        public bool MovePressed => WorldPointer && move.WasPressedThisFrame();
        public bool ClearPressed => Active && clear.WasPressedThisFrame();
        public bool FocusPressed => Active && focus.WasPressedThisFrame();
        private bool Active => isActiveAndEnabled && Application.isFocused;
        private bool WorldPointer => Active && Pointer.x >= 0 && Pointer.y >= 0
            && Pointer.x < Screen.width && Pointer.y < Screen.height
            && !(hud != null && hud.isActiveAndEnabled && hud.ContainsScreenPoint(Pointer))
            && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject());

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
            focus = map.AddAction("Focus", InputActionType.Button, "<Keyboard>/f");
            additive = map.AddAction("Additive", InputActionType.Button, "<Keyboard>/leftShift");
            additive.AddBinding("<Keyboard>/rightShift");
        }
        private void OnEnable() => map.Enable();
        private void OnDisable() => map.Disable();
        private void OnDestroy() => map.Dispose();
    }
}
