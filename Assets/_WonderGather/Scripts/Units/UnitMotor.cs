using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class UnitMotor : MonoBehaviour
    {
        private NavMeshAgent agent;
        public Vector3 Destination { get; private set; }
        public bool IsMoving => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh
            && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + .05f);
        private void Awake() { agent = GetComponent<NavMeshAgent>(); }

        public bool TryMove(Vector3 destination)
        {
            return TryPlanMove(destination, out var plannedPath, out var resolved) && ApplyMove(plannedPath, resolved);
        }

        public float Radius => agent.radius;

        // Planning has no effect on the current order, allowing a group to validate first.
        public bool TryPlanMove(Vector3 destination, out NavMeshPath plannedPath, out Vector3 resolved)
        {
            plannedPath = null;
            resolved = default;
            if (!isActiveAndEnabled || !agent.isActiveAndEnabled || !agent.isOnNavMesh
                || !IsFinite(destination)) return false;
            if (!NavMesh.SamplePosition(destination, out var hit, .8f, agent.areaMask)) return false;
            plannedPath = new NavMeshPath();
            if (!agent.CalculatePath(hit.position, plannedPath) || plannedPath.status != NavMeshPathStatus.PathComplete) return false;
            resolved = hit.position;
            return true;
        }

        public bool ApplyMove(NavMeshPath plannedPath, Vector3 resolved)
        {
            if (!isActiveAndEnabled || !agent.isActiveAndEnabled || !agent.isOnNavMesh || !agent.SetPath(plannedPath)) return false;
            agent.isStopped = false;
            Destination = resolved;
            return true;
        }
        private static bool IsFinite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        private void OnDisable()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) agent.ResetPath();
        }
    }
}
