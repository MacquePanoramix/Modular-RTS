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
                if (unitIndex < 0 || !units[unitIndex].Motor.TryPlanMove(slots[slotIndex], out paths[unitIndex], out targets[unitIndex])) return false;
                for (int other = 0; other < units.Count; other++)
                {
                    if (!assigned[other]) continue;
                    float clearance = units[unitIndex].Motor.Radius + units[other].Motor.Radius + .1f;
                    if ((targets[unitIndex] - targets[other]).sqrMagnitude < clearance * clearance) return false;
                }
                assigned[unitIndex] = true;
                slots.RemoveAt(slotIndex);
            }
            // No frames elapse between validation and dispatch.
            for (int i = 0; i < units.Count; i++)
                if (!units[i].Motor.ApplyMove(paths[i], targets[i])) return false;
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
