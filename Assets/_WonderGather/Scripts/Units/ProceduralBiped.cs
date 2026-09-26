using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // Presentation only: navigation owns the root. Feet retain world-space support points.
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class ProceduralBiped : MonoBehaviour
    {
        [SerializeField] private Transform pelvis,torso,head;
        [SerializeField] private Transform selectionRing;
        public void ConfigureRing(Transform ring)=>selectionRing=ring;
        [SerializeField] private Transform[] thighs,shins,feet,upperArms,forearms;
        [SerializeField,Range(.3f,1.2f)] private float stepReach=.65f;
        [SerializeField,Range(.05f,.3f)] private float footLift=.16f;
        [SerializeField,Range(.16f,.5f)] private float stepDuration=.34f;
        [SerializeField] private LayerMask groundMask=1<<6;
        [SerializeField] private Gatherer worker;
        [SerializeField] private Transform carriedBundle;
        [SerializeField] private Vector3 bundleScale=Vector3.one;
        private sealed class Foot
        {
            public Vector3 position,normal=Vector3.up,from,to,fromNormal,toNormal;
            public Quaternion rotation,fromRotation,toRotation;
            public float progress;
            public bool swinging;
        }
        private readonly Foot[] support={new Foot(),new Foot()};
        private readonly float[] armSwing=new float[2];
        private readonly Vector3[] handPositions=new Vector3[2];
        private readonly Vector3[] handLocal=new Vector3[2];
        private Vector3 previousPosition,previousVelocity,velocity,acceleration;
        private Quaternion facing;
        private float pelvisY;
        private float measuredSpeed;
        private bool initialized;
        private bool handsInitialized;
        private int nextFoot;
        public int StepCount {get;private set;}
        public bool Ready=>initialized;
        public Vector3 FootPosition(int index)=>support[index].position;
        public bool FootPlanted(int index)=>!support[index].swinging;
        public Vector3 FootNormal(int index)=>support[index].normal;
        public bool CargoVisible=>carriedBundle!=null&&carriedBundle.gameObject.activeInHierarchy;
        public Vector3 HandPosition(int index)=>handPositions[index];
        public void Configure(Transform hips,Transform chest,Transform skull,Transform[] upperLegs,Transform[] lowerLegs,Transform[] soles,Transform[] arms,Transform[] lowerArms)
        {pelvis=hips;torso=chest;head=skull;thighs=upperLegs;shins=lowerLegs;feet=soles;upperArms=arms;forearms=lowerArms;initialized=false;}
        public void ConfigureWork(Gatherer source,Transform bundle)
        {
            worker=source;
            if(carriedBundle!=bundle)
            {
                if(carriedBundle!=null) carriedBundle.gameObject.SetActive(false);
                carriedBundle=bundle;
                if(bundle!=null) bundleScale=bundle.localScale;
            }
            handsInitialized=false;
            RefreshCargo();
        }
        public void SetTuning(float reach,float lift,float duration)
        {
            if(!float.IsFinite(reach)||!float.IsFinite(lift)||!float.IsFinite(duration)||reach<.3f||reach>1.2f||lift<.05f||lift>.3f||duration<.16f||duration>.5f)
                throw new System.ArgumentOutOfRangeException(nameof(reach));
            stepReach=reach;footLift=lift;stepDuration=duration;
        }
        private void OnEnable(){initialized=false;handsInitialized=false;}
        private void OnDisable(){initialized=false;handsInitialized=false;}
        private static bool Pair(Transform[] values)=>values!=null&&values.Length==2&&values[0]!=null&&values[1]!=null;
        private bool RigValid=>pelvis!=null&&torso!=null&&head!=null&&Pair(thighs)&&Pair(shins)&&Pair(feet)&&Pair(upperArms)&&Pair(forearms);
        private Vector3 Home(int index)=>transform.position+facing*new Vector3(index==0?-.21f:.21f,0,.02f);
        private bool Ground(Vector3 point,out Vector3 position,out Vector3 normal)
        {
            if(Physics.Raycast(point+Vector3.up*3,Vector3.down,out var hit,6,groundMask,QueryTriggerInteraction.Ignore)&&hit.normal.y>.65f)
            {position=hit.point;normal=hit.normal;return true;}
            position=point;normal=Vector3.up;return false;
        }
        private Quaternion SoleRotation(Vector3 normal)=>Quaternion.LookRotation(Vector3.ProjectOnPlane(facing*Vector3.forward,normal).normalized,normal);
        public void ResetPose()
        {
            initialized=false;
            if(!RigValid) return;
            facing=Quaternion.Euler(0,transform.eulerAngles.y,0);
            for(int i=0;i<2;i++)
            {
                if(!Ground(Home(i),out var point,out var normal)) return;
                var foot=support[i];foot.position=point;foot.normal=normal;foot.rotation=SoleRotation(normal);foot.swinging=false;foot.progress=0;
            }
            previousPosition=transform.position;previousVelocity=velocity=acceleration=Vector3.zero;measuredSpeed=0;
            pelvisY=transform.position.y+1.43f;armSwing[0]=armSwing[1]=0;nextFoot=0;initialized=true;
            handsInitialized=false;
            Pose(0);
        }
        private void LateUpdate()
        {
            RefreshCargo();
            if(!initialized){ResetPose();return;}
            float dt=Time.deltaTime;if(dt<=0) return;
            if((transform.position-previousPosition).sqrMagnitude>4){ResetPose();return;}
            // Actual displacement includes avoidance and stopping, rather than the requested destination.
            Vector3 measured=(transform.position-previousPosition)/dt;previousPosition=transform.position;
            measured.y=0;measuredSpeed=measured.magnitude;velocity=Vector3.Lerp(velocity,measured,1-Mathf.Exp(-14*dt));
            Vector3 change=(velocity-previousVelocity)/dt;previousVelocity=velocity;
            acceleration=Vector3.Lerp(acceleration,Vector3.ClampMagnitude(change,8),1-Mathf.Exp(-7*dt));
            Quaternion desiredFacing=Quaternion.Euler(0,transform.eulerAngles.y,0);
            if(Working)
            {
                var direction=Vector3.ProjectOnPlane(worker.WorkContact-transform.position,Vector3.up);
                if(direction.sqrMagnitude>.001f) desiredFacing=Quaternion.LookRotation(direction);
            }
            facing=Quaternion.RotateTowards(facing,desiredFacing,220*dt);
            bool stepping=false;
            for(int i=0;i<2;i++)
            {
                var foot=support[i];if(!foot.swinging) continue;
                foot.progress=Mathf.Min(1,foot.progress+dt/CurrentStepDuration);
                float t=foot.progress,smooth=t*t*(3-2*t);
                foot.position=Vector3.Lerp(foot.from,foot.to,smooth)+Vector3.up*(Mathf.Sin(t*Mathf.PI)*footLift);
                foot.normal=Vector3.Slerp(foot.fromNormal,foot.toNormal,smooth);
                foot.rotation=Quaternion.Slerp(foot.fromRotation,foot.toRotation,smooth);
                if(t>=1){foot.swinging=false;foot.position=foot.to;foot.normal=foot.toNormal;nextFoot=1-i;StepCount++;}
                else stepping=true;
            }
            if(!stepping)
            {
                int choice=-1;float largest=0;bool moving=velocity.sqrMagnitude>.015f;
                for(int order=0;order<2;order++)
                {
                    int i=(nextFoot+order)%2;
                    float error=Vector3.ProjectOnPlane(Home(i)-support[i].position,Vector3.up).magnitude;
                    float turn=Quaternion.Angle(support[i].rotation,SoleRotation(support[i].normal));
                    float threshold=moving?(worker!=null?Mathf.Min(.16f,stepReach*.23f):stepReach*.42f):.075f;
                    float score=Mathf.Max(error/threshold,turn/35);
                    if(score>1&&score>largest){largest=score;choice=i;}
                }
                if(choice>=0) BeginStep(choice,moving);
            }
            // Mutually unreachable contacts indicate a presentation discontinuity,
            // just like the large root correction handled above.
            if((ReachCenter(0)-ReachCenter(1)).sqrMagnitude>4*LegReach*LegReach){ResetPose();return;}
            Pose(dt);
        }
        private void BeginStep(int index,bool moving)
        {
            // Shorter, earlier steps preserve support across the creator's speed range.
            // They change only presentation; authored movement speed remains authoritative.
            Vector3 lead=moving?(worker!=null
                ?Vector3.ClampMagnitude(velocity*CurrentStepDuration*1.1f,Mathf.Min(.55f,stepReach))
                :Vector3.ClampMagnitude(velocity*stepDuration*1.5f,stepReach*1.5f)):Vector3.zero;
            if(!Ground(Home(index)+lead,out var point,out var normal)||Mathf.Abs(point.y-transform.position.y)>.8f) return;
            var foot=support[index];foot.from=foot.position;foot.to=point;foot.fromNormal=foot.normal;foot.toNormal=normal;
            foot.fromRotation=foot.rotation;foot.toRotation=SoleRotation(normal);foot.progress=0;foot.swinging=true;
        }
        private float CurrentStepDuration=>worker!=null
            ?Mathf.Clamp(stepReach*.72f/Mathf.Max(Mathf.Max(measuredSpeed,velocity.magnitude),.1f),.055f,stepDuration)
            :stepDuration;
        private bool Working=>worker!=null&&worker.isActiveAndEnabled&&worker.HasWorkContact
            &&(worker.State==Gatherer.Activity.Gathering||worker.State==Gatherer.Activity.Depositing);
        private void RefreshCargo()
        {
            if(carriedBundle==null) return;
            bool visible=worker!=null&&worker.Carried>0;
            if(carriedBundle.gameObject.activeSelf!=visible) carriedBundle.gameObject.SetActive(visible);
        }
        private static void Segment(Transform part,Vector3 from,Vector3 to,float thickness)
        {
            var delta=to-from;part.position=(from+to)*.5f;
            if(delta.sqrMagnitude>.000001f) part.rotation=Quaternion.FromToRotation(Vector3.up,delta);
            part.localScale=new Vector3(thickness,delta.magnitude*.5f,thickness);
        }
        private const float LegReach=1.33f;
        private Vector3 ReachCenter(int index)=>support[index].position+support[index].normal*.13f
            -facing*new Vector3(index==0?-.21f:.21f,0,0);
        private Vector3 ReachableHips(Vector3 desired)
        {
            Vector3 a=ReachCenter(0),b=ReachCenter(1);
            float radiusSquared=LegReach*LegReach;
            Vector3 onA=a+Vector3.ClampMagnitude(desired-a,LegReach);
            if((onA-b).sqrMagnitude<=radiusSquared) return onA;
            Vector3 onB=b+Vector3.ClampMagnitude(desired-b,LegReach);
            if((onB-a).sqrMagnitude<=radiusSquared) return onB;
            // Both constraints are active: use the nearest point on the spheres'
            // intersection circle. Only the visual pelvis moves; feet/root do not.
            Vector3 between=b-a,axis=between.normalized,center=(a+b)*.5f;
            Vector3 radial=Vector3.ProjectOnPlane(desired-center,axis);
            if(radial.sqrMagnitude<.000001f) radial=Vector3.ProjectOnPlane(Vector3.up,axis);
            if(radial.sqrMagnitude<.000001f) radial=Vector3.ProjectOnPlane(Vector3.right,axis);
            return center+radial.normalized*Mathf.Sqrt(Mathf.Max(0,radiusSquared-between.sqrMagnitude*.25f));
        }
        private void Pose(float dt)
        {
            if(selectionRing!=null)
            {
                selectionRing.position=transform.position+Vector3.up*.14f;
                selectionRing.rotation=Quaternion.FromToRotation(Vector3.up,(support[0].normal+support[1].normal).normalized);
            }
            float speed=velocity.magnitude;
            float bob=0;for(int i=0;i<2;i++) if(support[i].swinging) bob=Mathf.Sin(support[i].progress*Mathf.PI)*.025f;
            float standingHeight=transform.position.y+1.43f+bob,height=standingHeight;
            bool horizontalOverreach=false;
            for(int i=0;i<2;i++)
            {
                Vector3 hip=Home(i),ankle=support[i].position+support[i].normal*.13f;
                float horizontal=Vector3.ProjectOnPlane(hip-ankle,Vector3.up).sqrMagnitude;
                horizontalOverreach|=horizontal>LegReach*LegReach;
                height=Mathf.Min(height,ankle.y+Mathf.Sqrt(Mathf.Max(.01f,LegReach*LegReach-horizontal)));
            }
            // Lowering alone cannot repair horizontal overreach after a correction.
            // Let the full reach constraint shift the body back from standing height.
            if(horizontalOverreach) height=standingHeight;
            pelvisY=dt>0?Mathf.Lerp(pelvisY,height,1-Mathf.Exp(-16*dt)):height;
            // Reach is a hard constraint on lowering; only the upward recovery is smoothed.
            pelvisY=Mathf.Min(pelvisY,height);
            Vector3 localAcceleration=Quaternion.Inverse(facing)*acceleration;
            float pitch=Mathf.Clamp(localAcceleration.z*1.8f+speed*1.2f,-7,9),roll=Mathf.Clamp(-localAcceleration.x*1.6f,-6,6);
            Quaternion posture=facing*Quaternion.Euler(pitch,0,roll);
            Vector3 hips=ReachableHips(new Vector3(transform.position.x,pelvisY,transform.position.z));
            pelvisY=hips.y;
            pelvis.SetPositionAndRotation(hips,posture);
            torso.SetPositionAndRotation(hips+posture*new Vector3(0,.29f,0),posture);
            head.SetPositionAndRotation(hips+posture*new Vector3(0,.64f,0),Quaternion.Slerp(facing,posture,.4f));
            if(carriedBundle!=null)
            {
                carriedBundle.SetPositionAndRotation(hips+posture*new Vector3(0,-.08f,.37f),posture);
                float fullness=worker!=null?Mathf.Clamp01((float)worker.Carried/Mathf.Max(1,worker.Capacity)):0;
                carriedBundle.localScale=bundleScale*Mathf.Lerp(.8f,1,fullness);
            }
            for(int i=0;i<2;i++)
            {
                float side=i==0?-1:1;var foot=support[i];
                feet[i].SetPositionAndRotation(foot.position+foot.normal*.07f,foot.rotation);
                Vector3 hip=hips+facing*new Vector3(side*.21f,0,0),ankle=foot.position+foot.normal*.13f;
                Vector3 axis=(ankle-hip).normalized;float distance=Mathf.Min(Vector3.Distance(hip,ankle),1.359f);
                Vector3 bend=Vector3.ProjectOnPlane(facing*Vector3.forward,axis).normalized;
                Vector3 knee=(hip+ankle)*.5f+bend*Mathf.Sqrt(Mathf.Max(0,.68f*.68f-distance*distance*.25f));
                Segment(thighs[i],hip,knee,.19f);Segment(shins[i],knee,ankle,.15f);
                float desiredSwing=Mathf.Clamp(-Vector3.Dot(foot.position-Home(i),facing*Vector3.forward)*.32f,-.18f,.18f)*Mathf.Min(speed,1);
                armSwing[i]=Mathf.Lerp(armSwing[i],desiredSwing,1-Mathf.Exp(-10*dt));float swing=armSwing[i];
                Vector3 shoulder=hips+posture*new Vector3(side*.34f,.49f,0);
                Vector3 elbow=shoulder+posture*new Vector3(side*.065f,-.40f,swing);
                Vector3 wrist=elbow+posture*new Vector3(side*.015f,-.34f,.075f+swing*.4f);
                if(worker!=null)
                {
                    if(worker.Carried>0) wrist=hips+posture*new Vector3(side*.22f,-.12f,.37f);
                    if(i==1&&Working)
                    {
                        float progress=Mathf.Clamp01(worker.ActionProgress);
                        float reach=Mathf.Sin(progress*Mathf.PI);
                        wrist=Vector3.Lerp(wrist,worker.WorkContact,reach);
                    }
                    // Smooth targets in body space: a held load should travel with the torso.
                    // On interruption the target immediately becomes carry/rest, never stale contact.
                    Vector3 local=Quaternion.Inverse(posture)*(wrist-hips);
                    handLocal[i]=handsInitialized&&dt>0?Vector3.Lerp(handLocal[i],local,1-Mathf.Exp(-18*dt)):local;
                    wrist=hips+posture*handLocal[i];
                    SolveArm(i,side,posture,shoulder,wrist);
                }
                else
                {
                    Segment(upperArms[i],shoulder,elbow,.13f);Segment(forearms[i],elbow,wrist,.11f);
                    handPositions[i]=wrist;
                }
            }
            handsInitialized=true;
        }
        private void SolveArm(int index,float side,Quaternion posture,Vector3 shoulder,Vector3 target)
        {
            const float upper=.44f,lower=.43f;
            Vector3 delta=target-shoulder;
            Vector3 axis=delta.sqrMagnitude>.000001f?delta.normalized:posture*Vector3.down;
            float distance=Mathf.Clamp(delta.magnitude,.02f,upper+lower-.001f);
            Vector3 wrist=shoulder+axis*distance;
            float along=(upper*upper-lower*lower+distance*distance)/(2*distance);
            Vector3 bend=Vector3.ProjectOnPlane(posture*new Vector3(side*.55f,-.6f,-.25f),axis).normalized;
            if(bend.sqrMagnitude<.001f) bend=Vector3.Cross(axis,posture*Vector3.forward).normalized;
            Vector3 elbow=shoulder+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
            Segment(upperArms[index],shoulder,elbow,.13f);Segment(forearms[index],elbow,wrist,.11f);
            handPositions[index]=wrist;
        }
    }
}
