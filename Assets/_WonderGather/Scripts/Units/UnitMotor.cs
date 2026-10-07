using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class UnitMotor : MonoBehaviour
    {
        private NavMeshAgent agent;
        private float baseSpeed;
        private bool speedCaptured;
        public void SetMovementRate(float rate)
        {
            if(!float.IsFinite(rate)||rate<=0) throw new System.ArgumentOutOfRangeException(nameof(rate));
            if(agent==null) agent=GetComponent<NavMeshAgent>();
            if(!speedCaptured){baseSpeed=agent.speed;speedCaptured=true;}
            agent.speed=baseSpeed*rate;
        }
        public Vector3 Destination { get; private set; }
        public bool IsMoving => away == Away.Going || away == Away.Coming || hasPending
            || (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh
                && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + .05f));
        private void Awake() { agent = GetComponent<NavMeshAgent>(); }

        // Off the walked ground. The ground that can be walked keeps its distance from anything solid (half a metre
        // in the Ordinary Place), so it stops short of places a body can well stand: at the foot of a rock it is to
        // work on. A unit takes the last short way to such a place on its own feet, in a straight line, with short
        // careful steps; and takes it back, to where it left the walked ground, before it goes anywhere else.
        private enum Away { No, Going, There, Coming }
        private Away away;
        private Vector3 left, offTo, pending;
        private float ridesUp;
        private bool hasPending;
        private int pendingTries;
        // How far off the walked ground it will go (metres), and the share of its walking pace at which it does.
        public const float OffAtMost = .9f, OffPace = .6f;
        private const int Ground = 1 << 6;
        // It is off the walked ground (going, standing there, or coming back); it stands there.
        public bool IsOff => away != Away.No;
        public bool StandsOff => away == Away.There;

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        private static bool GroundAt(Vector3 place, out float height)
        {
            bool found = Physics.Raycast(new Vector3(place.x, place.y + 3, place.z), Vector3.down, out var hit, 8, Ground);
            height = found ? hit.point.y : place.y;
            return found;
        }

        // It goes to a place a little off the walked ground. False if it is not on the walked ground to begin with, or
        // the place is too far from where it left it.
        public bool StepOff(Vector3 place)
        {
            if (!isActiveAndEnabled || agent == null || !IsFinite(place)) return false;
            if (away == Away.No)
            {
                if (!agent.isActiveAndEnabled || !agent.isOnNavMesh || Flat(place - transform.position).magnitude > OffAtMost) return false;
                Stop();
                left = transform.position;
                // It rides as high over the ground as it does on the walked ground.
                ridesUp = GroundAt(left, out float under) ? left.y - under : 0;
                agent.enabled = false;
            }
            else if (Flat(place - left).magnitude > OffAtMost) return false;
            offTo = place; away = Away.Going; hasPending = false;
            return true;
        }

        private void Update()
        {
            if (away == Away.No)
            {
                // Just back on the walked ground with somewhere to go: it goes as soon as it can.
                if (hasPending && (TryMove(pending) || ++pendingTries > 10)) hasPending = false;
                return;
            }
            if (away == Away.There) return;
            Vector3 to = Flat((away == Away.Going ? offTo : left) - transform.position);
            float dt = Time.deltaTime;
            if (to.magnitude > .002f)
            {
                // It turns the way it goes, then goes.
                Quaternion heading = Quaternion.LookRotation(to.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, heading, agent.angularSpeed * dt);
                if (Quaternion.Angle(transform.rotation, heading) > 50) return;
                Vector3 next = transform.position + Vector3.ClampMagnitude(to, agent.speed * OffPace * dt);
                if (GroundAt(next, out float under)) next.y = under + ridesUp;
                transform.position = next;
                return;
            }
            if (away == Away.Going) { away = Away.There; return; }
            // Back where it left the walked ground: it walks as it does again.
            transform.position = left;
            agent.enabled = true;
            away = Away.No;
            pendingTries = 0;
        }

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
            if (away != Away.No)
            {
                // Off the walked ground: the way is planned from where it left it.
                if (!isActiveAndEnabled || !IsFinite(destination) || !NavMesh.SamplePosition(destination, out var there, .8f, agent.areaMask)) return false;
                plannedPath = new NavMeshPath();
                if (!NavMesh.CalculatePath(left, there.position, agent.areaMask, plannedPath) || plannedPath.status != NavMeshPathStatus.PathComplete) return false;
                resolved = there.position;
                return true;
            }
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
            if (away != Away.No)
            {
                // It comes back to the walked ground first, and goes on from there.
                if (!isActiveAndEnabled) return false;
                pending = resolved; hasPending = true; pendingTries = 0;
                away = Away.Coming;
                Destination = resolved;
                return true;
            }
            if (!isActiveAndEnabled || !agent.isActiveAndEnabled || !agent.isOnNavMesh || !agent.SetPath(plannedPath)) return false;
            agent.isStopped = false;
            Destination = resolved;
            return true;
        }
        public void Stop()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) { agent.isStopped = true; agent.ResetPath(); agent.velocity = Vector3.zero; }
            // Off the walked ground it stops where it is; coming back to it, it comes back and stays.
            if (away == Away.Going) away = Away.There;
            hasPending = false;
            Destination = transform.position;
        }
        private static bool IsFinite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        // Work orders turn a stopped root; normal navigation owns rotation again on departure.
        public bool Face(Vector3 point, float deltaTime)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude < .0001f) return true;
            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, agent.angularSpeed * deltaTime);
            return Quaternion.Angle(transform.rotation, target) < 8;
        }
        private void OnDisable()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) { agent.isStopped = true; agent.ResetPath(); agent.velocity = Vector3.zero; }
        }
    }
}
