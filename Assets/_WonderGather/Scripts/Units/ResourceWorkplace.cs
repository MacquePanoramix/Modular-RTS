using System;
using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // Authored, reachable places to stand and make contact. Workers release
    // their reservation when departing, cancelling, or being disabled.
    public sealed class ResourceWorkplace : MonoBehaviour
    {
        [SerializeField] private Transform[] positions = Array.Empty<Transform>();
        [SerializeField] private Transform[] contacts = Array.Empty<Transform>();
        private Gatherer[] occupants;
        private readonly Collider[] nearby = new Collider[16];
        public int Capacity => positions.Length;
        public int OccupiedCount
        {
            get
            {
                EnsureOccupants();
                int count = 0;
                foreach (var worker in occupants) if (worker != null && worker.isActiveAndEnabled) count++;
                return count;
            }
        }
        public void Configure(Transform[] stands, Transform[] touchPoints)
        {
            if (stands == null || touchPoints == null || stands.Length == 0 || stands.Length != touchPoints.Length)
                throw new ArgumentException("Work positions and contact points must be matching nonempty arrays.");
            for (int i = 0; i < stands.Length; i++)
                if (stands[i] == null || touchPoints[i] == null) throw new ArgumentException("Workplace references cannot be empty.");
            if (OccupiedCount != 0) throw new InvalidOperationException("Cannot replace occupied work positions.");
            positions = (Transform[])stands.Clone(); contacts = (Transform[])touchPoints.Clone();
            occupants = new Gatherer[positions.Length];
        }
        private void EnsureOccupants()
        {
            if (occupants == null || occupants.Length != positions.Length) occupants = new Gatherer[positions.Length];
        }
        public bool Owns(Gatherer worker)
        {
            EnsureOccupants();
            return worker != null && Array.IndexOf(occupants, worker) >= 0;
        }
        public Vector3 Contact(Gatherer worker)
        {
            EnsureOccupants();
            int index = Array.IndexOf(occupants, worker);
            return index >= 0 && index < contacts.Length && contacts[index] != null ? contacts[index].position : worker.transform.position;
        }
        public bool TryClaim(Gatherer worker, UnitMotor motor, out NavMeshPath path, out Vector3 destination)
        {
            path = null; destination = default;
            if (!isActiveAndEnabled || worker == null || motor == null || positions.Length != contacts.Length) return false;
            EnsureOccupants();
            int existing = Array.IndexOf(occupants, worker), chosen = -1;
            float best = float.PositiveInfinity;
            for (int i = 0; i < positions.Length; i++)
            {
                if (existing >= 0 && i != existing) continue;
                if (positions[i] == null || contacts[i] == null) continue;
                var owner = occupants[i];
                if (owner != null && owner != worker && owner.isActiveAndEnabled) continue;
                if (Blocked(positions[i].position, worker)) continue;
                if (!motor.TryPlanMove(positions[i].position, out var route, out var point)) continue;
                // Generic orders can resolve nearby points; work must reach its actual port.
                if ((point - positions[i].position).sqrMagnitude > .12f * .12f) continue;
                float distance = (worker.transform.position - point).sqrMagnitude;
                if (distance >= best) continue;
                best = distance; chosen = i; path = route; destination = point;
            }
            if (chosen < 0) return false;
            occupants[chosen] = worker;
            return true;
        }
        // A released position can still hold another worker's body; only claim a place
        // a body can actually reach.
        private bool Blocked(Vector3 point, Gatherer claimant)
        {
            int count = Physics.OverlapSphereNonAlloc(point + Vector3.up * .5f, .3f, nearby, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var other = nearby[i].GetComponentInParent<Gatherer>();
                if (other != null && other != claimant && other.isActiveAndEnabled) return true;
            }
            return false;
        }
        public void Release(Gatherer worker)
        {
            EnsureOccupants();
            for (int i = 0; i < occupants.Length; i++) if (occupants[i] == worker) occupants[i] = null;
        }
        private void OnDisable()
        {
            if (occupants != null) Array.Clear(occupants, 0, occupants.Length);
        }
    }
}
