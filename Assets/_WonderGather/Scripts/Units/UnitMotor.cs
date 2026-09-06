using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class UnitMotor : MonoBehaviour
    {
        private NavMeshAgent agent;
        private NavMeshPath path;
        public Vector3 Destination { get; private set; }
        public bool IsMoving => agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh
            && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + .05f);
        private void Awake() { agent = GetComponent<NavMeshAgent>(); path = new NavMeshPath(); }

        public bool TryMove(Vector3 destination)
        {
            if (!isActiveAndEnabled || !agent.isActiveAndEnabled || !agent.isOnNavMesh
                || !IsFinite(destination)) return false;
            if (!NavMesh.SamplePosition(destination, out var hit, .8f, agent.areaMask)) return false;
            if (!agent.CalculatePath(hit.position, path) || path.status != NavMeshPathStatus.PathComplete) return false;
            if (!agent.SetPath(path)) return false;
            agent.isStopped = false;
            Destination = hit.position;
            return true;
        }
        private static bool IsFinite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        private void OnDisable()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) agent.ResetPath();
        }
    }
}
