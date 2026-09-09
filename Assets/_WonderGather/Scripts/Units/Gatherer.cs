using UnityEngine;
namespace WonderGather
{
    [RequireComponent(typeof(UnitMotor))]
    public sealed class Gatherer : MonoBehaviour
    {
        public enum Activity { Idle, ToResource, Gathering, ToDepot }
        [SerializeField] private ResourceDepot depot;
        [SerializeField] private Vector3 workOffset;
        [SerializeField, Min(1)] private int capacity = 5;
        [SerializeField, Min(.05f)] private float secondsPerUnit = .5f;
        private UnitMotor motor;
        private ResourceNode resource;
        private float timer;
        public int Carried { get; private set; }
        public Activity State { get; private set; }
        public void Configure(ResourceDepot home, Vector3 offset) { depot = home; workOffset = offset; }
        private void Awake() => motor = GetComponent<UnitMotor>();
        public bool Gather(ResourceNode node)
        {
            if(TryGetComponent<UnitIdentity>(out var identity) && (identity.Blueprint==null || !identity.Blueprint.GathersSupplies)) return false;
            if (!isActiveAndEnabled || node == null || !node.isActiveAndEnabled || node.Remaining <= 0 || depot == null || !depot.isActiveAndEnabled) return false;
            var target = Carried >= capacity ? depot.transform.position : node.transform.position;
            if (!motor.TryMove(target + workOffset)) return false;
            if(TryGetComponent<Builder>(out var builder)) builder.CancelOrder();
            resource = node; timer = 0;
            State = Carried >= capacity ? Activity.ToDepot : Activity.ToResource;
            return true;
        }
        public bool ReturnToDepot(ResourceDepot home)
        {
            if (!isActiveAndEnabled || home == null || !home.isActiveAndEnabled || Carried == 0
                || !motor.TryMove(home.transform.position + workOffset)) return false;
            if(TryGetComponent<Builder>(out var builder)) builder.CancelOrder();
            depot = home; resource = null; timer = 0; State = Activity.ToDepot;
            return true;
        }
        public void CancelOrder() { State = Activity.Idle; resource = null; timer = 0; }
        private bool TravelHome()
        {
            if (depot == null || !depot.isActiveAndEnabled || !motor.TryMove(depot.transform.position + workOffset))
            { CancelOrder(); return false; }
            State = Activity.ToDepot;
            return true;
        }
        private void Update()
        {
            if (State == Activity.Idle) return;
            if (depot == null || !depot.isActiveAndEnabled) { CancelOrder(); return; }
            if (State != Activity.ToDepot && (resource == null || !resource.isActiveAndEnabled || resource.Remaining == 0))
            { if (Carried > 0) TravelHome(); else CancelOrder(); return; }
            if (State == Activity.ToResource || State == Activity.ToDepot)
            {
                if ((transform.position - motor.Destination).sqrMagnitude > .4f * .4f) return;
                if (State == Activity.ToResource) { State = Activity.Gathering; timer = 0; }
                else
                {
                    if (Carried > 0 && depot.Deposit(Carried)) Carried = 0;
                    if (resource == null || !Gather(resource)) CancelOrder();
                }
                return;
            }
            timer += Time.deltaTime;
            if (timer < secondsPerUnit) return;
            timer -= secondsPerUnit;
            Carried += resource.Take(1);
            if (Carried >= capacity || resource.Remaining == 0) TravelHome();
        }
        private void OnDisable() { CancelOrder(); if (motor != null) motor.Stop(); }
    }
}
