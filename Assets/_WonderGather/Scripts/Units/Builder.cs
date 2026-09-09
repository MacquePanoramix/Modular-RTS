using UnityEngine;
using UnityEngine.AI;
namespace WonderGather
{
    [RequireComponent(typeof(UnitMotor))]
    public sealed class Builder : MonoBehaviour
    {
        private UnitMotor motor;
        private Gatherer gatherer;
        public BuildingSite Site { get; private set; }
        private void Awake() { motor=GetComponent<UnitMotor>(); gatherer=GetComponent<Gatherer>(); }
        public bool Plan(Vector3 center, float size, out NavMeshPath path, out Vector3 point)
        {
            path=null;point=default;
            if(!isActiveAndEnabled) return false;
            float best=float.PositiveInfinity;
            for(int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI/4;
                var candidate=center+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*(size*.71f+motor.Radius+.5f);
                if(motor.TryPlanMove(candidate,out var route,out var resolved))
                {
                    float distance=(transform.position-resolved).sqrMagnitude;
                    if(distance<best) { best=distance;path=route;point=resolved; }
                }
            }
            return path!=null;
        }
        public bool CanBuild(BuildingBlueprint data)
        {
            return !TryGetComponent<UnitIdentity>(out var identity) || (identity.Blueprint!=null && identity.Blueprint.CanBuild(data));
        }
        public bool Build(BuildingSite site)
        {
            if(site==null || !CanBuild(site.Blueprint) || site.Complete || (site.Worker!=null && site.Worker!=this)
                || !Plan(site.transform.position,site.Definition.Size,out var path,out var point)) return false;
            if(!site.Claim(this)) return false;
            if(!motor.ApplyMove(path,point)) {site.Release(this);return false;}
            if(Site!=null && Site!=site) Site.Release(this);
            Site=site;
            if(gatherer!=null) gatherer.CancelOrder();
            return true;
        }
        public void CancelOrder() { if(Site!=null) Site.Release(this); Site=null; }
        private void Update()
        {
            if(Site==null) return;
            if(!Site.isActiveAndEnabled || Site.Complete) {CancelOrder();return;}
            if((transform.position-motor.Destination).sqrMagnitude>.4f*.4f) return;
            Site.Work(this,Time.deltaTime);
            if(Site.Complete) CancelOrder();
        }
        private void OnDisable() { CancelOrder(); if(motor!=null) motor.Stop(); }
    }
}
