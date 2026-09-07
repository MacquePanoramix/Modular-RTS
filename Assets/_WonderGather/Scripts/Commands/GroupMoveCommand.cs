using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // Destination formation only; agents route independently around obstacles.
    public sealed class GroupMoveCommand
    {
        private readonly Vector3 destination;
        private readonly float spacing;
        public GroupMoveCommand(Vector3 destination, float spacing = 2.4f)
        { this.destination = destination; this.spacing = spacing; }

        public bool Execute(IReadOnlyList<SelectableUnit> units)
        {
            if (units == null || units.Count == 0 || !float.IsFinite(spacing) || spacing <= 0
                || !float.IsFinite(destination.x) || !float.IsFinite(destination.y) || !float.IsFinite(destination.z)) return false;
            var center = Vector3.zero;
            float gap = spacing;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] == null || !units[i].isActiveAndEnabled) return false;
                for (int j = 0; j < i; j++) if (units[j] == units[i]) return false;
                center += units[i].transform.position;
                // Leave a passage for an arriving agent between two settled neighbours.
                gap = Mathf.Max(gap, units[i].Motor.Radius * 4 + .4f);
            }
            center /= units.Count;
            Vector3 forward = destination - center;
            forward.y = 0;
            if (forward.sqrMagnitude < .001f) forward = Vector3.forward;
            var slots = CreateSlots(units.Count, destination, Quaternion.LookRotation(forward), gap);
            var paths = new NavMeshPath[units.Count];
            var targets = new Vector3[units.Count];
            var assigned = new bool[units.Count];
            // Closest remaining pair first reduces unnecessary crossing paths.
            for (int step = 0; step < units.Count; step++)
            {
                int unitIndex = -1, slotIndex = -1;
                float best = float.PositiveInfinity;
                for (int u = 0; u < units.Count; u++)
                {
                    if (assigned[u]) continue;
                    for (int s = 0; s < slots.Count; s++)
                    {
                        float cost = (units[u].transform.position - slots[s]).sqrMagnitude;
                        if (cost < best) { best = cost; unitIndex = u; slotIndex = s; }
                    }
                }
                if (unitIndex < 0 || !ReachableDestination.Plan(units[unitIndex].Motor, slots[slotIndex], out paths[unitIndex], out targets[unitIndex])) return false;
                var anchor = targets[unitIndex];
                bool fits = Fits(anchor, targets, assigned, gap);
                // Compact into reachable nearby ground while keeping room for arrivals.
                for (int ring = 1; !fits && ring <= units.Count + 2; ring++)
                    for (int direction = 0; !fits && direction < 16; direction++)
                    {
                        float angle = direction * Mathf.PI / 8;
                        var candidate = anchor + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (ring * gap);
                        if (units[unitIndex].Motor.TryPlanMove(candidate, out var route, out var point) && Fits(point, targets, assigned, gap))
                        { paths[unitIndex] = route; targets[unitIndex] = point; fits = true; }
                    }
                if (!fits) return false;
                assigned[unitIndex] = true;
                slots.RemoveAt(slotIndex);
            }
            // No frames elapse between validation and dispatch.
            for (int i = 0; i < units.Count; i++)
                {
                    if (!units[i].Motor.ApplyMove(paths[i], targets[i])) return false;
                    if (units[i].TryGetComponent<Gatherer>(out var worker)) worker.CancelOrder();
                }
            return true;
        }

        private static bool Fits(Vector3 point, Vector3[] targets, bool[] assigned, float gap)
        {
            for (int i = 0; i < targets.Length; i++)
                if (assigned[i] && (point-targets[i]).sqrMagnitude < gap * gap * .99f) return false;
            return true;
        }

        public static List<Vector3> CreateSlots(int count, Vector3 center, Quaternion orientation, float spacing)
        {
            var slots = new List<Vector3>(count);
            int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            int rows = Mathf.CeilToInt((float)count / Mathf.Max(1, columns));
            for (int row = 0; row < rows; row++)
            {
                int rowSize = Mathf.Min(columns, count - row * columns);
                for (int column = 0; column < rowSize; column++)
                    slots.Add(center + orientation * new Vector3((column - (rowSize - 1) * .5f) * spacing, 0, (row - (rows - 1) * .5f) * spacing));
            }
            return slots;
        }
    }
}
