using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // Candidate points on each baked triangle, nearest first. Path checks exclude islands.
    public static class ReachableDestination
    {
        public static bool Plan(UnitMotor motor, Vector3 requested, out NavMeshPath path, out Vector3 resolved)
        {
            if (motor.TryPlanMove(requested, out path, out resolved)) return true;
            if (!float.IsFinite(requested.x) || !float.IsFinite(requested.y) || !float.IsFinite(requested.z)) return false;
            var mesh = NavMesh.CalculateTriangulation();
            var candidates = new List<Vector3>(mesh.indices.Length / 3);
            for (int i = 0; i < mesh.indices.Length; i += 3)
                candidates.Add(Closest(requested, mesh.vertices[mesh.indices[i]], mesh.vertices[mesh.indices[i+1]], mesh.vertices[mesh.indices[i+2]]));
            candidates.Sort((a,b) => (a-requested).sqrMagnitude.CompareTo((b-requested).sqrMagnitude));
            foreach (var point in candidates)
                if (motor.TryPlanMove(point, out path, out resolved)) return true;
            return false;
        }
        private static Vector3 Edge(Vector3 p, Vector3 a, Vector3 b)
        {
            var d = b-a;
            return a + d * Mathf.Clamp01(Vector3.Dot(p-a,d) / Mathf.Max(d.sqrMagnitude, .000001f));
        }
        private static Vector3 Closest(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            var n = Vector3.Cross(b-a,c-a);
            var q = p - n * (Vector3.Dot(p-a,n) / Mathf.Max(n.sqrMagnitude, .000001f));
            if (n.sqrMagnitude > .000001f && Vector3.Dot(Vector3.Cross(b-a,q-a),n) >= 0
                && Vector3.Dot(Vector3.Cross(c-b,q-b),n) >= 0 && Vector3.Dot(Vector3.Cross(a-c,q-c),n) >= 0) return q;
            var best = Edge(p,a,b);
            var other = Edge(p,b,c); if ((other-p).sqrMagnitude < (best-p).sqrMagnitude) best = other;
            other = Edge(p,c,a); if ((other-p).sqrMagnitude < (best-p).sqrMagnitude) best = other;
            return best;
        }
    }
}
