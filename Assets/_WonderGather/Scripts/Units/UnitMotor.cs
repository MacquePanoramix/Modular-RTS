using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class UnitMotor : MonoBehaviour
    {
        private NavMeshAgent agent;
        private ProceduralBiped body;
        private float baseSpeed,turnPace;
        private int turnedAt=-10;
        private bool speedCaptured;
        public void SetMovementRate(float rate)
        {
            if(!float.IsFinite(rate)||rate<=0) throw new System.ArgumentOutOfRangeException(nameof(rate));
            if(agent==null) agent=GetComponent<NavMeshAgent>();
            if(!speedCaptured){baseSpeed=agent.speed;speedCaptured=true;}
            agent.speed=baseSpeed*rate;
        }
        // How a body turns to go somewhere (Luis, October 8: sent the opposite way, "the movement is not very human
        // or natural"). It turns with a turn that gathers pace and loses it (in this long, no faster than this many
        // degrees a second), not at one pace from the first frame. And it does not set off across its own feet:
        // turned further from its way than WaitsBeyond it all but stands (it creeps, at this share of its pace)
        // and turns; it walks as it comes round, at its whole pace once within WalksWithin. Half a turn takes it
        // about a second and a quarter. (People take about a second and a half, in two or three steps.) Its pace
        // itself is not changed for this (others set it, and read it): its going is held back.
        private const float TurnsIn = .2f, TurnsAtMost = 250, WaitsBeyond = 58, WalksWithin = 22, Creeps = .05f, WaitsForAStep = .14f;
        // How far it is turned from the way it means to go (degrees).
        public float TurnedFromItsWay { get; private set; }

        // The turn as its legs let it be: the body is told the way it means to face, and turns no further than its
        // feet on the ground allow (ProceduralBiped.MayFace); stopped by them, its turn has no pace to go on with.
        private float Turned(float now, float wanted, float dt)
        {
            turnedAt = Time.frameCount;
            if (body == null) return Mathf.SmoothDampAngle(now, wanted, ref turnPace, TurnsIn, TurnsAtMost, dt);
            body.MeansToFace(wanted);
            // As far towards the way it means to face as its feet let it go now: it eases up to that (it turned at
            // its whole pace until its feet stopped it, and stood so: "a pose held", the judges said).
            float aim = body.MayFace(now, now + Mathf.Clamp(Mathf.DeltaAngle(now, wanted), -90, 90));
            // A larger body turns more slowly, as it steps more slowly.
            float size = body.StepSize;
            float next = Mathf.SmoothDampAngle(now, aim, ref turnPace, TurnsIn * size, TurnsAtMost / size, dt);
            float may = body.MayFace(now, next);
            if (Mathf.Abs(Mathf.DeltaAngle(may, next)) > .001f) turnPace = dt > 0 ? Mathf.DeltaAngle(now, may) / dt : 0;
            return may;
        }

        private void Steer(float dt)
        {
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh || dt <= 0) return;
            Vector3 way = Flat(agent.desiredVelocity);
            bool going = !agent.isStopped && agent.hasPath && way.sqrMagnitude > 1e-5f;
            if (!going)
            {
                TurnedFromItsWay = 0;
                // Not going anywhere, and not being turned to face anything either: its turn has no pace left.
                // (Turned to face its work by another, it keeps the pace of that turn.)
                if (Time.frameCount - turnedAt > 1) turnPace = 0;
                return;
            }
            float wanted = Mathf.Atan2(way.x, way.z) * Mathf.Rad2Deg, now = transform.eulerAngles.y;
            TurnedFromItsWay = Mathf.Abs(Mathf.DeltaAngle(now, wanted));
            transform.rotation = Quaternion.Euler(0, Turned(now, wanted, dt), 0);
            float share = Mathf.Lerp(Creeps, 1, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(WaitsBeyond, WalksWithin, TurnedFromItsWay)));
            // And it does not walk out from under a foot that is in the air in a step taken standing.
            if (body != null && body.StepsStanding) share = Mathf.Min(share, WaitsForAStep);
            if (share < 1) agent.velocity = Vector3.ClampMagnitude(agent.velocity, agent.speed * share);
        }
        public Vector3 Destination { get; private set; }
        public bool IsMoving => away == Away.Going || away == Away.Coming || hasPending
            || (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh
                && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + .05f));
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            body = GetComponent<ProceduralBiped>();
            // It is turned here (Steer), not by the agent.
            agent.updateRotation = false;
        }

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
        // How it is moving now, in words (for what reports why a thing was given up).
        public string MovingHow => away != Away.No ? "off the walked ground: " + away : hasPending ? "waiting to go" : agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh ? (agent.pathPending ? "finding its way" : $"{agent.remainingDistance:0.00} m to go, stopping within {agent.stoppingDistance:0.00}") : "not on the walked ground";
        public bool StandsOff => away == Away.There;

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        private static bool GroundAt(Vector3 place, out float height)
        {
            bool found = Physics.Raycast(new Vector3(place.x, place.y + 3, place.z), Vector3.down, out var hit, 8, Ground);
            height = found ? hit.point.y : place.y;
            return found;
        }

        // It has been carried off the walked ground where it stands (a fall): it is off it from now, and will come back
        // to where it left it before it goes anywhere.
        public void CarriedOff()
        {
            if (away == Away.No)
            {
                if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
                Stop();
                left = transform.position;
                ridesUp = GroundAt(left, out float under) ? left.y - under : 0;
                agent.enabled = false;
            }
            away = Away.There; hasPending = false;
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
                Steer(Time.deltaTime);
                return;
            }
            if (away == Away.There) return;
            Vector3 to = Flat((away == Away.Going ? offTo : left) - transform.position);
            float dt = Time.deltaTime;
            if (to.magnitude > .002f)
            {
                // It turns the way it goes, then goes.
                Quaternion heading = Quaternion.LookRotation(to.normalized);
                float yaw = transform.eulerAngles.y, wanted = heading.eulerAngles.y;
                transform.rotation = Quaternion.Euler(0, Turned(yaw, wanted, dt), 0);
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
        // It stops walking and comes to rest as a walker does, losing its pace, not in one frame. True once it stands.
        public bool ComeToRest()
        {
            if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh || away != Away.No) { Stop(); return true; }
            agent.isStopped = true;
            if (agent.velocity.sqrMagnitude > .0016f) return false;
            Stop();
            return true;
        }
        private static bool IsFinite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        // Work orders turn a stopped root; normal navigation owns rotation again on departure.
        public bool Face(Vector3 point, float deltaTime)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude < .0001f) return true;
            Quaternion target = Quaternion.LookRotation(direction);
            float yaw = transform.eulerAngles.y, wanted = target.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0, Turned(yaw, wanted, deltaTime), 0);
            return Quaternion.Angle(transform.rotation, target) < 8;
        }
        private void OnDisable()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh) { agent.isStopped = true; agent.ResetPath(); agent.velocity = Vector3.zero; }
        }
    }
}
