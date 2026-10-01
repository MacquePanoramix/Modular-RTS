using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // Moves a small sphere through the world without entering solid geometry.
    // Static scenery blocks and the sphere slides along it; living bodies only nudge.
    public sealed class CameraCollision
    {
        private const float Skin = .004f;
        private readonly RaycastHit[] hits = new RaycastHit[32];
        private readonly Collider[] overlaps = new Collider[32];
        private readonly Dictionary<Collider, bool> bodies = new Dictionary<Collider, bool>();
        public int Mask = Physics.DefaultRaycastLayers & ~(1 << 2) & ~(1 << 5);
        public int GroundMask = 1 << 6;

        // Living bodies move into the camera; the camera yields softly instead of blocking them.
        public bool IsBody(Collider collider)
        {
            if (collider == null) return false;
            if (!bodies.TryGetValue(collider, out bool body))
            {
                body = collider.GetComponentInParent<SelectableUnit>() != null || collider.GetComponentInParent<NavMeshAgent>() != null;
                bodies[collider] = body;
            }
            return body;
        }

        // Sweep and slide: travel until contact, then continue along the surface.
        public Vector3 Move(Vector3 position, Vector3 delta, float radius)
        {
            for (int pass = 0; pass < 4 && delta.sqrMagnitude > 1e-10f; pass++)
            {
                float distance = delta.magnitude;
                var direction = delta / distance;
                if (!Nearest(position, direction, distance + Skin, radius, out var contact))
                {
                    position += delta;
                    break;
                }
                float travel = Mathf.Max(0, contact.distance - Skin);
                position += direction * travel;
                delta = Vector3.ProjectOnPlane(direction * (distance - travel), contact.normal);
            }
            return position;
        }

        public bool Nearest(Vector3 origin, Vector3 direction, float distance, float radius, out RaycastHit nearest)
        {
            nearest = default;
            float best = float.MaxValue;
            int count = Physics.SphereCastNonAlloc(origin, radius, direction, hits, distance, Mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                // Zero-distance hits are initial overlaps; Blocked() resolves those.
                if (hit.distance <= 0 || hit.distance >= best || IsBody(hit.collider)) continue;
                best = hit.distance;
                nearest = hit;
            }
            return best < float.MaxValue;
        }

        // True when the sphere overlaps static scenery at this position.
        public bool Blocked(Vector3 position, float radius)
        {
            int count = Physics.OverlapSphereNonAlloc(position, radius, overlaps, Mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++) if (!IsBody(overlaps[i])) return true;
            return false;
        }

        // How far a body's personal space pushes the camera this instant (zero when clear).
        public Vector3 BodyPush(Vector3 position, float radius, float personalSpace)
        {
            var push = Vector3.zero;
            float reach = radius + personalSpace;
            int count = Physics.OverlapSphereNonAlloc(position, reach, overlaps, Mask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var body = overlaps[i];
                if (!IsBody(body)) continue;
                var closest = body.ClosestPoint(position);
                var away = position - closest;
                float distance = away.magnitude;
                if (distance < 1e-4f)
                {
                    // Inside the body: leave sideways from its centre.
                    away = position - body.bounds.center;
                    away.y = 0;
                    if (away.sqrMagnitude < 1e-6f) away = Vector3.right;
                    push += away.normalized * reach;
                }
                else if (distance < reach) push += away / distance * (reach - distance);
            }
            return push;
        }

        // Height of walkable ground beneath a point, searching from far above it.
        public bool Ground(Vector3 position, out float height)
        {
            height = 0;
            if (!Physics.Raycast(new Vector3(position.x, position.y + 400, position.z), Vector3.down, out var hit, 2000, GroundMask, QueryTriggerInteraction.Ignore))
                return false;
            height = hit.point.y;
            return true;
        }

        // Distance to the nearest surface along a few directions; drives proximity-scaled speed.
        public float Clearance(Vector3 position, Vector3 heading, float radius, float limit)
        {
            float clearance = limit;
            if (Ground(position, out float ground)) clearance = Mathf.Min(clearance, Mathf.Max(0, position.y - ground));
            if (Physics.Raycast(position, Vector3.down, out var below, limit, Mask, QueryTriggerInteraction.Ignore) && !IsBody(below.collider))
                clearance = Mathf.Min(clearance, below.distance);
            if (heading.sqrMagnitude > 1e-6f && Nearest(position, heading.normalized, limit, radius, out var ahead))
                clearance = Mathf.Min(clearance, ahead.distance);
            return clearance;
        }
    }
}
