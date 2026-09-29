using UnityEngine;

namespace WonderGather
{
    [RequireComponent(typeof(UnitMotor))]
    public sealed class Gatherer : MonoBehaviour
    {
        public enum Activity { Idle, ToResource, Gathering, ToDepot, WaitingForResource, WaitingForDepot, Depositing }
        [SerializeField] private ResourceDepot depot;
        [SerializeField] private Vector3 workOffset;
        [SerializeField, Min(1)] private int capacity = 5;
        [SerializeField, Min(.05f)] private float secondsPerUnit = .5f;
        private const float DepositSeconds = .4f;
        private UnitMotor motor;
        private ResourceNode resource;
        private ResourceWorkplace workplace;
        private bool needsWorkplace;
        private float timer, retryAfter, baseSeconds;
        private bool rateCaptured;
        private EquippedTool tool;
        private ulong lastStrike;
        public MineableResource MiningTarget => resource != null ? resource.GetComponent<MineableResource>() : null;
        public bool FacingWork { get; private set; }
        public string LastOrderFailure { get; private set; } = "";
        public int Capacity => capacity;
        public float SecondsPerUnit => secondsPerUnit;
        public int Carried { get; private set; }
        public Activity State { get; private set; }
        public bool HasWorkContact => workplace != null && workplace.isActiveAndEnabled && workplace.Owns(this)
            && (State == Activity.Gathering || State == Activity.Depositing);
        public Vector3 WorkContact => workplace != null ? workplace.Contact(this) : transform.position;
        public float ActionProgress => State == Activity.Gathering ? Mathf.Clamp01(timer / secondsPerUnit)
            : State == Activity.Depositing ? Mathf.Clamp01(timer / DepositSeconds) : 0;
        public string ActivityLabel => State == Activity.Gathering && MiningTarget != null
            ? (tool != null && tool.Busy ? "Mining: " + tool.Phase : tool != null ? tool.Status : "Missing pickaxe")
            : State == Activity.Idle && !string.IsNullOrEmpty(LastOrderFailure) ? LastOrderFailure : State switch
        {
            Activity.ToResource => "Walking to supplies",
            Activity.Gathering => "Gathering",
            Activity.ToDepot => "Carrying to depot",
            Activity.WaitingForResource => "Waiting for a gathering place",
            Activity.WaitingForDepot => "Waiting for a delivery place",
            Activity.Depositing => "Delivering",
            _ => Carried > 0 ? "Holding supplies" : "Idle"
        };

        public void SetPerformance(UnitPerformance value)
        {
            value.Validate();
            if (!rateCaptured) { baseSeconds = secondsPerUnit; rateCaptured = true; }
            capacity = value.capacity;
            secondsPerUnit = baseSeconds * 100f / value.gatheringPercent;
        }

        public void Configure(ResourceDepot home, Vector3 offset) { depot = home; workOffset = offset; }
        private void Awake() => motor = GetComponent<UnitMotor>();

        public bool Gather(ResourceNode node)
        {
            LastOrderFailure="";
            if (TryGetComponent<UnitIdentity>(out var identity) && (identity.Blueprint == null || !identity.Blueprint.GathersSupplies)) return false;
            if (!isActiveAndEnabled || node == null || !node.isActiveAndEnabled || node.Remaining <= 0 || depot == null || !depot.isActiveAndEnabled) return false;
            var mine=node.GetComponent<MineableResource>();
            tool=GetComponent<EquippedTool>();
            if(mine!=null)
            {
                if(!mine.Available) { LastOrderFailure="Mining surface unavailable"; return false; }
                if(tool==null || !tool.isActiveAndEnabled || !mine.Accepts(tool.Definition))
                { LastOrderFailure="Equip a pickaxe in the unit blueprint"; return false; }
                if(!tool.SupportsBodyScale) {LastOrderFailure="Tool/body scale unsupported"; return false;}
                if(!TryGetComponent<ProceduralBiped>(out var body) || !body.isActiveAndEnabled)
                { LastOrderFailure="Body cannot support the tool"; return false; }
            }
            bool full = Carried >= capacity;
            if (!TryTravel(full ? depot.transform : node.transform, full ? Activity.ToDepot : Activity.ToResource,
                full ? Activity.WaitingForDepot : Activity.WaitingForResource)) return false;
            if (TryGetComponent<Builder>(out var builder)) builder.CancelOrder();
            if(tool!=null) tool.CancelAttempt();
            resource = node;
            return true;
        }

        public bool ReturnToDepot(ResourceDepot home)
        {
            if (!isActiveAndEnabled || home == null || !home.isActiveAndEnabled || Carried == 0
                || !TryTravel(home.transform, Activity.ToDepot, Activity.WaitingForDepot)) return false;
            if (TryGetComponent<Builder>(out var builder)) builder.CancelOrder();
            if(tool!=null) tool.CancelAttempt();
            depot = home; resource = null;
            return true;
        }

        private void ReleaseWorkplace()
        {
            if (workplace != null) workplace.Release(this);
            workplace = null; needsWorkplace = false;
        }

        public void CancelOrder()
        {
            if(tool!=null) tool.CancelAttempt();
            ReleaseWorkplace();
            State = Activity.Idle; resource = null; timer = 0; retryAfter = 0; FacingWork=false;
        }
        public void InterruptMining(string reason)
        { CancelOrder(); LastOrderFailure=reason; if(motor!=null) motor.Stop(); }
        internal bool AcceptStrike(EquippedTool source, MineableResource mine, ulong id)
        {
            if(source!=tool || !source.isActiveAndEnabled || !source.OwnsAcceptedAttempt(mine,id) || id==lastStrike
                || !isActiveAndEnabled || State!=Activity.Gathering || !HasWorkContact || !FacingWork
                || mine!=MiningTarget || !mine.Accepts(source.Definition) || resource==null || Carried>=capacity) return false;
            lastStrike=id;
            int amount=resource.Take(Mathf.Min(1,capacity-Carried));
            Carried+=amount;
            return amount>0;
        }

        private bool TryTravel(Transform target, Activity travel, Activity wait)
        {
            var next = target.GetComponent<ResourceWorkplace>();
            if (next != null)
            {
                if (!next.isActiveAndEnabled) return false;
                bool alreadyOwned = next == workplace && next.Owns(this);
                if (next.TryClaim(this, motor, out var path, out var point))
                {
                    if (!motor.ApplyMove(path, point))
                    {
                        if (!alreadyOwned) next.Release(this);
                        return false;
                    }
                    if (workplace != next) ReleaseWorkplace();
                    workplace = next; needsWorkplace = true;
                    State = travel; timer = 0; retryAfter = 0;
                    return true;
                }
                // Wait away from the interaction surface, retaining the requested task.
                if (State != wait && !motor.TryMove(target.position + workOffset)) return false;
                ReleaseWorkplace();
                State = wait; timer = 0; retryAfter = Time.time + .35f;
                return true;
            }
            // Earlier prototype scenes retain their established offset-based interaction.
            if (!motor.TryMove(target.position + workOffset)) return false;
            ReleaseWorkplace(); State = travel; timer = 0;
            return true;
        }

        private bool TravelHome()
        {
            if (depot == null || !depot.isActiveAndEnabled || !TryTravel(depot.transform, Activity.ToDepot, Activity.WaitingForDepot))
            { CancelOrder(); return false; }
            return true;
        }

        private void Deliver()
        {
            if (Carried > 0 && depot.Deposit(Carried)) Carried = 0;
            ReleaseWorkplace();
            if (resource == null || !Gather(resource)) CancelOrder();
        }

        private void Update()
        {
            FacingWork=false;
            if (State == Activity.Idle) return;
            if (depot == null || !depot.isActiveAndEnabled) { CancelOrder(); return; }
            bool returning = State == Activity.ToDepot || State == Activity.WaitingForDepot || State == Activity.Depositing;
            var mine=MiningTarget;
            if(!returning && mine!=null)
            {
                if(tool==null || !tool.isActiveAndEnabled || !mine.Accepts(tool.Definition))
                { InterruptMining("Mining interrupted: tool or surface unavailable"); return; }
                if((Carried>=capacity || resource.Remaining==0) && (tool==null || !tool.Busy))
                { if(Carried>0) TravelHome(); else {CancelOrder(); motor.Stop();} return; }
            }
            if (!returning && (resource == null || !resource.isActiveAndEnabled || resource.Remaining == 0))
            {
                if(mine==null || tool==null || !tool.Busy || !resource.isActiveAndEnabled)
                { if (Carried > 0) TravelHome(); else { CancelOrder(); motor.Stop(); } return; }
            }
            if (needsWorkplace && (workplace == null || !workplace.isActiveAndEnabled || !workplace.Owns(this)))
            { CancelOrder(); motor.Stop(); return; }
            if (State == Activity.WaitingForResource || State == Activity.WaitingForDepot)
            {
                if (Time.time < retryAfter) return;
                bool home = State == Activity.WaitingForDepot;
                if (!TryTravel(home ? depot.transform : resource.transform, home ? Activity.ToDepot : Activity.ToResource,
                    home ? Activity.WaitingForDepot : Activity.WaitingForResource)) { CancelOrder(); motor.Stop(); }
                return;
            }
            if (State == Activity.ToResource || State == Activity.ToDepot)
            {
                float tolerance = needsWorkplace ? .16f : .4f;
                if ((transform.position - motor.Destination).sqrMagnitude > tolerance * tolerance) return;
                if (State == Activity.ToResource)
                { State = Activity.Gathering; timer = 0; if (needsWorkplace) motor.Stop(); }
                else if (needsWorkplace)
                { State = Activity.Depositing; timer = 0; motor.Stop(); }
                else Deliver();
                return;
            }
            if (HasWorkContact && !motor.Face(WorkContact, Time.deltaTime)) return;
            FacingWork=HasWorkContact;
            // Equipment owns attempts. The shared body/tool solve will validate physical
            // contact before calling AcceptStrike; the legacy timer cannot mine.
            if(State==Activity.Gathering && mine!=null) return;
            timer += Time.deltaTime;
            if (State == Activity.Depositing)
            { if (timer >= DepositSeconds) Deliver(); return; }
            if (timer < secondsPerUnit) return;
            timer -= secondsPerUnit;
            Carried += resource.Take(1);
            if (Carried >= capacity || resource.Remaining == 0) TravelHome();
        }

        private void OnDisable() { CancelOrder(); if (motor != null) motor.Stop(); }
    }
}
