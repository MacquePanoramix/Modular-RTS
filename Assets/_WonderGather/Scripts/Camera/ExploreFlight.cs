using System;
using UnityEngine;

namespace WonderGather
{
    // The free, editor-like Explore viewpoint. Soft by design: motion eases in and out,
    // speed shrinks near surfaces, and the camera slides along scenery instead of entering it.
    [Serializable]
    public sealed class ExploreFlight
    {
        [Tooltip("Radius of the camera's collision sphere, in metres.")]
        [SerializeField] private float radius = .06f;
        [SerializeField] private float nearClip = .02f;
        [Tooltip("Degrees per pixel of mouse travel.")]
        [SerializeField] private float lookSensitivity = .15f, orbitSensitivity = .3f;
        [SerializeField] private float lookResponse = 28, acceleration = 7, followResponse = 5;
        [Tooltip("Flight speed is baseSpeed × (0.5 + clearance in metres), clamped to the limits.")]
        [SerializeField] private float baseSpeed = 1.2f, minSpeed = .35f, maxSpeed = 45;
        [SerializeField] private float fastMultiplier = 3.5f, slowMultiplier = .25f;
        [SerializeField] private float focusDistance = 3.2f, focusPitch = 14, personalSpace = .22f, ceiling = 70;

        private Vector3 velocity, pivot, followOffset;
        private float targetYaw, targetPitch, clearance = 2, dolly, speedScale = 1, followHeight;
        private bool orbiting;

        public float Radius => radius;
        public float NearClip => nearClip;
        public Vector3 Position { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public Quaternion Rotation => Quaternion.Euler(Pitch, Yaw, 0);
        public Transform FollowTarget { get; private set; }
        public bool Following => FollowTarget != null;
        public float SpeedScale => speedScale;
        public float Clearance => clearance;

        public void Begin(Vector3 position, Quaternion rotation)
        {
            var euler = rotation.eulerAngles;
            Position = position;
            Yaw = targetYaw = euler.y;
            Pitch = targetPitch = Mathf.Clamp(Mathf.DeltaAngle(0, euler.x), -89, 89);
            velocity = Vector3.zero;
            dolly = 0;
            orbiting = false;
            FollowTarget = null;
        }

        // Glide to a close three-quarter view of the target and keep it framed as it moves.
        public void Focus(Transform target)
        {
            if (target == null) return;
            FollowTarget = target;
            followHeight = 1.2f;
            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                followHeight = Mathf.Clamp((bounds.max.y - target.position.y) * .62f, .4f, 3);
            }
            var heading = FollowPoint - Position;
            heading.y = 0;
            float yaw = heading.sqrMagnitude > .01f ? Quaternion.LookRotation(heading).eulerAngles.y : Yaw;
            followOffset = Quaternion.Euler(focusPitch, yaw, 0) * Vector3.back * focusDistance;
            velocity = Vector3.zero;
            dolly = 0;
        }

        public void StopFollowing() => FollowTarget = null;
        private Vector3 FollowPoint => FollowTarget.position + Vector3.up * followHeight;

        public void Step(in CameraIntent intent, float dt, CameraCollision collision, Bounds area)
        {
            if (FollowTarget != null && !FollowTarget.gameObject.activeInHierarchy) FollowTarget = null;
            bool flying = intent.Pan.sqrMagnitude > 1e-4f || Mathf.Abs(intent.Rotate) > 1e-3f;
            bool orbit = intent.AltOrbit || (intent.Middle && !intent.Fast);
            bool slide = intent.Middle && intent.Fast;
            // Flying or looking takes the camera back from the followed body; orbiting keeps it.
            if (flying || intent.Look || slide) FollowTarget = null;

            if (intent.Look)
            {
                targetYaw += intent.PointerDelta.x * lookSensitivity;
                targetPitch = Mathf.Clamp(targetPitch - intent.PointerDelta.y * lookSensitivity, -89, 89);
            }
            if (orbit && !orbiting) pivot = FollowTarget != null ? FollowPoint : OrbitPivot(collision);
            orbiting = orbit;
            if (orbit)
            {
                targetYaw += intent.PointerDelta.x * orbitSensitivity;
                targetPitch = Mathf.Clamp(targetPitch - intent.PointerDelta.y * orbitSensitivity, -60, 85);
            }
            float turn = 1 - Mathf.Exp(-lookResponse * dt);
            Yaw = Mathf.LerpAngle(Yaw, targetYaw, turn);
            Pitch = Mathf.Lerp(Pitch, targetPitch, turn);
            var forward = Rotation * Vector3.forward;
            var right = Rotation * Vector3.right;

            var wish = right * intent.Pan.x + forward * intent.Pan.y + Vector3.up * intent.Rotate;
            if (wish.sqrMagnitude > 1) wish.Normalize();
            float measured = collision.Clearance(Position, wish.sqrMagnitude > 1e-4f ? wish : forward, radius, 60);
            // Slow down at once when something comes close; speed up gently when space opens.
            clearance = Mathf.Lerp(clearance, measured, 1 - Mathf.Exp(-(measured < clearance ? 20 : 4) * dt));
            float speed = Mathf.Clamp(baseSpeed * (.5f + clearance), minSpeed, maxSpeed) * speedScale
                * (intent.Fast && !intent.Middle ? fastMultiplier : 1) * (intent.Slow ? slowMultiplier : 1);
            velocity = Vector3.Lerp(velocity, wish * speed, 1 - Mathf.Exp(-acceleration * dt));

            if (Mathf.Abs(intent.Zoom) > 1e-5f)
            {
                if (intent.Look) speedScale = Mathf.Clamp(speedScale * Mathf.Exp(intent.Zoom * .4f), .1f, 10);
                else dolly += intent.Zoom * Mathf.Max(.25f, speed * .8f);
            }
            float glide = dolly * (1 - Mathf.Exp(-9 * dt));
            dolly -= glide;
            if (Mathf.Abs(dolly) < 1e-4f) dolly = 0;

            Vector3 position = Position;
            if (orbit)
            {
                if (FollowTarget != null) pivot = FollowPoint;
                float distance = FollowTarget != null ? followOffset.magnitude : Vector3.Distance(pivot, position);
                distance = Mathf.Max(.3f, distance - glide);
                var back = -forward;
                if (collision.Nearest(pivot, back, distance + radius, radius, out var blocked))
                    distance = Mathf.Max(radius, blocked.distance - .01f);
                if (FollowTarget != null) followOffset = back * Mathf.Max(followOffset.magnitude - glide, .3f);
                position = collision.Move(Position, pivot + back * distance - Position, radius);
            }
            else if (FollowTarget != null)
            {
                followOffset = followOffset.normalized * Mathf.Max(.3f, followOffset.magnitude - glide);
                var point = FollowPoint;
                var offset = followOffset;
                if (collision.Nearest(point, offset.normalized, offset.magnitude + radius, radius, out var cover))
                    offset = offset.normalized * Mathf.Max(radius, cover.distance - .01f);
                var goal = point + offset;
                position = collision.Move(Position, (goal - Position) * (1 - Mathf.Exp(-followResponse * dt)), radius);
                var look = Quaternion.LookRotation(point - position).eulerAngles;
                targetYaw = look.y;
                targetPitch = Mathf.Clamp(Mathf.DeltaAngle(0, look.x), -89, 89);
            }
            else
            {
                var travel = velocity * dt + forward * glide;
                if (slide) travel += (-right * intent.PointerDelta.x - Rotation * Vector3.up * intent.PointerDelta.y) * Mathf.Max(.002f, clearance * .0025f);
                position = collision.Move(Position, travel, radius);
            }

            // Bodies walking into the camera nudge it aside rather than being blocked by it.
            var push = collision.BodyPush(position, radius, personalSpace);
            if (push.sqrMagnitude > 1e-8f) position = collision.Move(position, push * (1 - Mathf.Exp(-8 * dt)), radius);
            Position = Contain(position, collision, area);
        }

        // Never below walkable ground, never outside the world, never above a soft ceiling.
        private Vector3 Contain(Vector3 position, CameraCollision collision, Bounds area)
        {
            position.x = Mathf.Clamp(position.x, area.min.x, area.max.x);
            position.z = Mathf.Clamp(position.z, area.min.z, area.max.z);
            if (collision.Ground(position, out float ground) && position.y < ground + radius) position.y = ground + radius;
            float top = area.max.y + ceiling;
            if (position.y > top) position.y = Mathf.Lerp(position.y, top, .2f);
            // A residual overlap keeps the last clear position; a camera that began inside
            // scenery (for example after a mode switch) may still move out of it.
            if (collision.Blocked(position, radius * .9f) && !collision.Blocked(Position, radius * .9f)) return Position;
            return position;
        }

        private Vector3 OrbitPivot(CameraCollision collision)
        {
            var forward = Rotation * Vector3.forward;
            if (Physics.Raycast(Position, forward, out var hit, 60, collision.Mask, QueryTriggerInteraction.Ignore))
                return hit.point;
            return Position + forward * 8;
        }
    }
}
