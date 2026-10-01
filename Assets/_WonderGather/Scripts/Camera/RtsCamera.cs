using UnityEngine;

namespace WonderGather
{
    public enum CameraMode { Strategy, Explore }

    // The player's camera. Strategy is the fast RTS view for playing; Explore is a free,
    // collision-safe viewpoint for wandering close to the world. V toggles between them.
    // Existing serialized preferences (for example a map's zoom sensitivity) keep their meaning.
    [RequireComponent(typeof(Camera))]
    public sealed class RtsCamera : MonoBehaviour
    {
        [SerializeField] private RtsInput input;
        [SerializeField] private SelectionController selection;
        [SerializeField] private float panSpeed = 15, rotationSpeed = 80, zoomSensitivity = .002f, smoothing = 10;
        [SerializeField] private float minDistance = 5, maxDistance = 42, boundary = 25;
        [SerializeField] private bool terrainAware;
        [Tooltip("Scroll at the screen edge (full-screen only, so leaving a window never scrolls).")]
        [SerializeField] private bool edgeScroll = true;
        [SerializeField] private float edgeMargin = 8, panResponse = 16, modeBlendSeconds = .6f;
        [SerializeField] private ExploreFlight explore = new ExploreFlight();

        private readonly CameraCollision collision = new CameraCollision();
        private Camera view;
        private Vector3 target, current, blendFromPosition;
        private Quaternion blendFromRotation;
        private float yaw, currentYaw, distance = 23, currentDistance = 23, strategyNear = .1f, blend = 1, grabHeight;
        private float modeChangedAt = -100;
        private bool grabbing, hasArea;
        private Bounds area;

        public CameraMode Mode { get; private set; } = CameraMode.Strategy;
        public ExploreFlight Explore => explore;
        public Vector3 StrategyTarget => target;
        public float StrategyYaw => yaw;
        public bool Blending => blend < 1;

        public void Configure(RtsInput source, SelectionController selected) { input = source; selection = selected; }

        private void Awake()
        {
            view = GetComponent<Camera>();
            strategyNear = view.nearClipPlane;
            // Begin where the authored camera already looks, so the first frame does not jump.
            yaw = currentYaw = transform.eulerAngles.y;
        }

        private void LateUpdate()
        {
            if (input == null) return;
            Step(input.ReadCameraIntent(), Time.unscaledDeltaTime);
        }

        public void SetMode(CameraMode mode)
        {
            if (mode == Mode) return;
            if (view == null) Awake();
            if (mode == CameraMode.Explore)
            {
                // Explore starts exactly at the strategic view: nothing jumps.
                explore.Begin(transform.position, transform.rotation);
                view.nearClipPlane = explore.NearClip;
            }
            else
            {
                if (input != null) input.CaptureCursor(false);
                // Return over the ground the Explore camera was looking at, keeping its heading.
                var point = transform.position;
                if (Physics.Raycast(transform.position, transform.forward, out var seen, 400, collision.Mask, QueryTriggerInteraction.Ignore)
                    && !collision.IsBody(seen.collider)) point = seen.point;
                target = ClampTarget(new Vector3(point.x, 0, point.z));
                yaw = currentYaw = explore.Yaw;
                current = target;
                currentDistance = distance;
                blendFromPosition = transform.position;
                blendFromRotation = transform.rotation;
                blend = 0;
                view.nearClipPlane = strategyNear;
            }
            Mode = mode;
            modeChangedAt = Time.unscaledTime;
            if (input != null) input.ExploreMode = mode == CameraMode.Explore;
        }

        public void Step(in CameraIntent intent, float dt)
        {
            if (view == null) Awake();
            if (intent.Toggle) SetMode(Mode == CameraMode.Strategy ? CameraMode.Explore : CameraMode.Strategy);
            if (Mode == CameraMode.Explore) StepExplore(intent, dt);
            else StepStrategy(intent, dt);
        }

        private void StepExplore(in CameraIntent intent, float dt)
        {
            if (input != null) input.CaptureCursor(intent.Look && intent.LookDragging);
            if (intent.Focus && selection != null)
            {
                if (selection.Selected != null) explore.Focus(selection.Selected.transform);
                else if (selection.SelectedBuilding != null) explore.Focus(selection.SelectedBuilding.transform);
            }
            var scaled = intent;
            scaled.Zoom = intent.Zoom * zoomSensitivity;
            explore.Step(scaled, dt, collision, Area);
            transform.SetPositionAndRotation(explore.Position, explore.Rotation);
        }

        private void StepStrategy(in CameraIntent intent, float dt)
        {
            yaw += intent.Rotate * rotationSpeed * dt;
            if (intent.Middle && intent.Alt) yaw += intent.PointerDelta.x * .25f;
            var heading = Quaternion.Euler(0, yaw, 0);
            var pan = intent.Pan;
            if (edgeScroll && intent.PointerInWindow && !intent.Middle) pan += EdgeDirection(intent.Pointer);
            if (pan.sqrMagnitude > 1) pan.Normalize();
            // Faster than the original rig, and faster still when high above the map.
            target += heading * new Vector3(pan.x, 0, pan.y) * (panSpeed * Mathf.Lerp(.6f, 2.4f, distance / maxDistance) * dt);
            GrabPan(intent);
            if (intent.Focus && selection != null && selection.HasSelection) target = selection.Center;

            float before = distance;
            distance = Mathf.Clamp(distance * Mathf.Exp(-intent.Zoom * zoomSensitivity), minDistance, maxDistance);
            // Zoom in toward the cursor; zoom out from the centre.
            if (distance < before && intent.PointerInWorld && GroundUnderPointer(intent.Pointer, out var aim))
                AnchorZoom(intent.Pointer, aim);
            target = ClampTarget(target);
            if (terrainAware && Physics.Raycast(new Vector3(target.x, 40, target.z), Vector3.down, out var ground, 80, 1 << 6, QueryTriggerInteraction.Ignore))
                target.y = ground.point.y + 1.1f;

            float follow = 1 - Mathf.Exp(-Mathf.Max(smoothing, panResponse) * dt);
            float settle = 1 - Mathf.Exp(-smoothing * dt);
            current = Vector3.Lerp(current, target, follow);
            currentYaw = Mathf.LerpAngle(currentYaw, yaw, settle);
            currentDistance = Mathf.Lerp(currentDistance, distance, settle);
            var viewRotation = Quaternion.Euler(PitchAt(currentDistance), currentYaw, 0);
            var position = current - viewRotation * Vector3.forward * currentDistance;
            if (terrainAware && Physics.Raycast(new Vector3(position.x, 40, position.z), Vector3.down, out var below, 80, 1 << 6, QueryTriggerInteraction.Ignore))
                position.y = Mathf.Max(position.y, below.point.y + .6f);
            var rotation = terrainAware ? Quaternion.LookRotation(current - position) : viewRotation;
            if (blend < 1)
            {
                blend = Mathf.Min(1, blend + dt / Mathf.Max(.01f, modeBlendSeconds));
                float eased = blend * blend * (3 - 2 * blend);
                position = Vector3.Lerp(blendFromPosition, position, eased);
                rotation = Quaternion.Slerp(blendFromRotation, rotation, eased);
            }
            transform.SetPositionAndRotation(position, rotation);
        }

        private float PitchAt(float range) => Mathf.Lerp(32, 62, Mathf.InverseLerp(minDistance, maxDistance, range));

        // Places the target so that, once the zoom settles, the cursor's ray meets the same ground point.
        private void AnchorZoom(Vector2 pointer, Vector3 aim)
        {
            var rotation = Quaternion.Euler(PitchAt(distance), yaw, 0);
            var ray = rotation * (Quaternion.Inverse(transform.rotation) * view.ScreenPointToRay(pointer).direction);
            if (ray.y > -1e-3f) return;
            var back = rotation * Vector3.forward * distance;
            float along = (aim.y - target.y + back.y) / ray.y;
            var eye = aim - ray * along;
            target = new Vector3(eye.x + back.x, target.y, eye.z + back.z);
        }

        // Middle-drag grabs the ground: the point under the cursor stays under the cursor.
        private void GrabPan(in CameraIntent intent)
        {
            if (!intent.Middle || intent.Alt) { grabbing = false; return; }
            if (!grabbing) { grabbing = true; grabHeight = current.y; }
            var plane = new Plane(Vector3.up, new Vector3(0, grabHeight, 0));
            var from = view.ScreenPointToRay(intent.Pointer - intent.PointerDelta);
            var to = view.ScreenPointToRay(intent.Pointer);
            if (!plane.Raycast(from, out float a) || !plane.Raycast(to, out float b)) return;
            var shift = from.GetPoint(a) - to.GetPoint(b);
            shift.y = 0;
            target += shift;
            current += shift;
        }

        private Vector2 EdgeDirection(Vector2 pointer)
        {
            var direction = Vector2.zero;
            if (pointer.x <= edgeMargin) direction.x = -1;
            else if (pointer.x >= Screen.width - 1 - edgeMargin) direction.x = 1;
            if (pointer.y <= edgeMargin) direction.y = -1;
            else if (pointer.y >= Screen.height - 1 - edgeMargin) direction.y = 1;
            return direction;
        }

        private bool GroundUnderPointer(Vector2 pointer, out Vector3 point)
        {
            point = default;
            if (!Physics.Raycast(view.ScreenPointToRay(pointer), out var hit, 500, (1 << 0) | (1 << 6), QueryTriggerInteraction.Ignore)) return false;
            point = hit.point;
            return true;
        }

        private Vector3 ClampTarget(Vector3 value)
            => new Vector3(Mathf.Clamp(value.x, -boundary, boundary), 0, Mathf.Clamp(value.z, -boundary, boundary));

        // The walkable world, with room above it; Explore stays inside it.
        private Bounds Area
        {
            get
            {
                if (hasArea) return area;
                hasArea = true;
                bool any = false;
                foreach (var collider in FindObjectsByType<Collider>())
                {
                    if (collider.gameObject.layer != 6 || collider.isTrigger) continue;
                    if (!any) { area = collider.bounds; any = true; }
                    else area.Encapsulate(collider.bounds);
                }
                if (!any) area = new Bounds(Vector3.zero, new Vector3(boundary * 2 + 20, 20, boundary * 2 + 20));
                return area;
            }
        }

        private void OnGUI()
        {
            if (input == null || !input.isActiveAndEnabled) return;
            bool recent = Time.unscaledTime - modeChangedAt < 8;
            string text = Mode == CameraMode.Explore
                ? (recent ? "EXPLORE CAMERA   hold right mouse: look   WASD: fly   Q/E: down/up   Shift/Ctrl: faster/slower\nwheel: glide   middle drag: orbit   Shift + middle drag: slide   F: follow selection   V: strategy camera"
                          : "EXPLORE CAMERA   V: strategy camera")
                : (recent ? "STRATEGY CAMERA   WASD / screen edge / middle drag: pan   Q/E: rotate   wheel: zoom   F: focus   V: explore camera"
                          : "V: explore camera");
            var style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, wordWrap = true, fontSize = 13 };
            var size = style.CalcSize(new GUIContent(text));
            float width = Mathf.Min(Screen.width - 24, size.x + 24);
            float height = style.CalcHeight(new GUIContent(text), width) + 8;
            GUI.Box(new Rect((Screen.width - width) / 2, Screen.height - height - 14, width, height), text, style);
        }
    }
}
