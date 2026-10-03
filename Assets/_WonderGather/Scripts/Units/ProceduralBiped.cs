using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // Navigation owns the root. The body presents it with a phase-based gait whose stride,
    // cadence and walk/jog choice follow from hip height and actual displacement. Planted
    // feet keep fixed world-space support frames; heel strike, roll and toe-off only rotate
    // the rendered foot about those frames. Equipment consumes the supported torso pose
    // before the arms consume its solved grips.
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class ProceduralBiped : MonoBehaviour
    {
        public enum Gait { Standing, Walking, Jogging }
        // A body's measurements. The defaults are the 2.2 m test body; a modelled being (S1d) brings its own,
        // taken from its skeleton, so the solved joints land where its bones are.
        [System.Serializable]
        public struct Proportions
        {
            // Standing hip height, half the distance between the hip joints, and each leg segment's length.
            public float hipHeight, hipWidth, legSegment;
            // The ankle above the sole, and the foot's heel, ball and toe lengths from the ankle.
            public float ankleHeight, heelLength, ballLength, toeLength;
            // The waist above the hips, and the torso's and head's centres above the waist.
            public float waistRise, torsoRise, headRise;
            // The left shoulder from the waist in the chest's frame (x outwards), and the arm's two segments.
            public Vector3 shoulder;
            public float upperArm, forearm;
            // Where a relaxed hand hangs from its shoulder: outwards, down, forwards.
            public Vector3 armHang;
            // Carried loads, pumping arms, stance tolerances, rendered thickness and the ring's height scale with this.
            public float scale;
            // The walk's character, as multipliers of the original walk: how much the pelvis rises and falls, how
            // much it sways and rolls from side to side, and how far the arms swing.
            public float bounce, sway, armSwing;
            public static Proportions Default => new Proportions
            {
                hipHeight = 1.43f, hipWidth = .21f, legSegment = .68f, ankleHeight = .13f, heelLength = .07f, ballLength = .19f, toeLength = .11f,
                waistRise = .06f, torsoRise = .23f, headRise = .58f, shoulder = new Vector3(.34f, .43f, 0), upperArm = .44f, forearm = .43f,
                armHang = new Vector3(.06f, .83f, .05f), scale = 1, bounce = 1, sway = 1, armSwing = 1
            };
        }
        [SerializeField] private bool customProportions;
        [SerializeField] private Proportions proportions = Proportions.Default;
        [SerializeField] private Transform pelvis,torso,head;
        [SerializeField] private Transform selectionRing;
        public void ConfigureRing(Transform ring)=>selectionRing=ring;
        [SerializeField] private Transform[] thighs,shins,feet,upperArms,forearms;
        // Optional. Without toe segments the whole foot rolls about the ball.
        [SerializeField] private Transform[] toes;
        // stepReach: longest step as a fraction of hip height. footLift: swing clearance at
        // a walk. stepDuration: length of a standing adjustment step.
        [SerializeField,Range(.3f,1.2f)] private float stepReach=.65f;
        [SerializeField,Range(.05f,.3f)] private float footLift=.16f;
        [SerializeField,Range(.16f,.5f)] private float stepDuration=.34f;
        [SerializeField] private LayerMask groundMask=1<<6;
        [SerializeField] private Gatherer worker;
        [SerializeField] private Transform carriedBundle;
        [SerializeField] private Vector3 bundleScale=Vector3.one;
        private const float Gravity=9.81f;
        // The active proportions, and lengths derived from them (exactly the original constants by default).
        private Proportions P=Proportions.Default;
        private float HipHeight=1.43f,HipWidth=.21f,LegReach=1.33f,LegLimit=1.359f,NarrowStance=.14f,ClosedTolerance=.15f,SettleTolerance=.3f;
        private float AnkleHeight=.13f,HeelLength=.07f,BallLength=.19f,ToeLength=.11f;
        public Proportions BodyProportions=>P;
        public void SetProportions(Proportions value)
        {
            if(!(value.hipHeight>.2f&&value.hipWidth>.01f&&value.legSegment>.1f&&value.upperArm>.05f&&value.forearm>.05f&&value.scale>.1f&&value.armHang.y>.05f))
                throw new System.ArgumentOutOfRangeException(nameof(value));
            customProportions=true;proportions=value;ApplyProportions();initialized=false;
        }
        private void ApplyProportions()
        {
            P=customProportions?proportions:Proportions.Default;
            // A body saved before the walk had a character walks as the original did.
            if(P.bounce<=0) P.bounce=1;
            if(P.sway<=0) P.sway=1;
            if(P.armSwing<=0) P.armSwing=1;
            HipHeight=P.hipHeight;HipWidth=P.hipWidth;AnkleHeight=P.ankleHeight;HeelLength=P.heelLength;BallLength=P.ballLength;ToeLength=P.toeLength;
            if(!customProportions){LegReach=1.33f;LegLimit=1.359f;NarrowStance=.14f;ClosedTolerance=.15f;SettleTolerance=.3f;return;}
            // The same ratios as the original body: reach just short of a straight leg, a narrow stance at two thirds of the hips.
            LegReach=P.legSegment*2*(1.33f/1.36f);LegLimit=P.legSegment*2-.001f;NarrowStance=P.hipWidth*(2f/3f);
            ClosedTolerance=.15f*P.scale;SettleTolerance=.3f*P.scale;
        }
        // Froude number v²/(g·h): walking bipeds switch to running near 0.5.
        private const float JogAbove=.55f,WalkBelow=.45f,StartSpeed=.18f;
        private const float HeelStrike=-12,WalkToeOff=30,JogToeOff=38;
        private sealed class Foot
        {
            public Vector3 position,normal=Vector3.up,from,to,fromNormal,toNormal,ankle;
            public Quaternion rotation,fromRotation,toRotation;
            public float progress,rate,clearance,pitch,liftPitch,toeBend,lastPhase;
            public bool swinging,gaitSwing,liftedThisCycle;
        }
        private readonly Foot[] support={new Foot(),new Foot()};
        private readonly Vector3[] handPositions=new Vector3[2];
        private readonly Vector3[] handLocal=new Vector3[2];
        private readonly float[] handsBusy=new float[2];
        private NavMeshAgent agent;
        private Vector3 previousPosition,previousVelocity,velocity,acceleration;
        private Quaternion facing;
        private float pelvisY,hipLift,phase,cadence=1,duty=.6f,gaitWeight,jogWeight;
        private bool initialized;
        private bool handsInitialized;
        // The root has stopped and the feet are finishing the stride beneath it.
        private bool stopping;
        private int nextFoot;
        public Gait CurrentGait {get;private set;}
        // Only a jog has a flight phase; a walk always keeps one foot planted.
        public bool Airborne=>support[0].swinging&&support[1].swinging;
        public float Cadence=>CurrentGait==Gait.Standing?0:cadence;
        // Continuous time with both feet off the ground; only a jog's flight produces it.
        public float FlightTime {get;private set;}
        // Rendered sole pitch in degrees: negative toe-up at heel strike, positive heel-up at toe-off.
        public float FootPitch(int index)=>support[index].pitch;
        public int StepCount {get;private set;}
        public bool Ready=>initialized;
        public Vector3 FootPosition(int index)=>support[index].position;
        public bool FootPlanted(int index)=>!support[index].swinging;
        public Vector3 FootNormal(int index)=>support[index].normal;
        public bool CargoVisible=>carriedBundle!=null&&carriedBundle.gameObject.activeInHierarchy;
        // Report the currently rendered hand, including navigation displacement
        // since the last pose, just as the parented tool's TransformPoint does.
        public Vector3 HandPosition(int index)=>transform.TransformPoint(handPositions[index]);
        public void Configure(Transform hips,Transform chest,Transform skull,Transform[] upperLegs,Transform[] lowerLegs,Transform[] soles,Transform[] arms,Transform[] lowerArms)
        {pelvis=hips;torso=chest;head=skull;thighs=upperLegs;shins=lowerLegs;feet=soles;upperArms=arms;forearms=lowerArms;initialized=false;}
        public void ConfigureToes(Transform[] value){toes=value;initialized=false;}
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
        private void Awake(){agent=GetComponent<NavMeshAgent>();ApplyProportions();}
        private void OnEnable(){initialized=false;handsInitialized=false;}
        private void OnDisable()
        {
            initialized=false;handsInitialized=false;
            if(worker!=null && worker.MiningTarget!=null) worker.InterruptMining("Body cannot support the tool");
        }
        private static bool Pair(Transform[] values)=>values!=null&&values.Length==2&&values[0]!=null&&values[1]!=null;
        private bool RigValid=>pelvis!=null&&torso!=null&&head!=null&&Pair(thighs)&&Pair(shins)&&Pair(feet)&&Pair(upperArms)&&Pair(forearms);
        private Vector3 Home(int index)=>transform.position+facing*new Vector3(index==0?-HipWidth:HipWidth,0,.02f);
        private bool Ground(Vector3 point,out Vector3 position,out Vector3 normal)
        {
            if(Physics.Raycast(point+Vector3.up*3,Vector3.down,out var hit,6,groundMask,QueryTriggerInteraction.Ignore)&&hit.normal.y>.65f)
            {position=hit.point;normal=hit.normal;return true;}
            position=point;normal=Vector3.up;return false;
        }
        private Quaternion SoleRotation(Vector3 normal)=>Quaternion.LookRotation(Vector3.ProjectOnPlane(facing*Vector3.forward,normal).normalized,normal);
        public void ResetPose()
        {
            if(TryGetComponent<EquippedTool>(out var equipment)) equipment.CancelAttempt();
            initialized=false;
            if(!RigValid) return;
            facing=Quaternion.Euler(0,transform.eulerAngles.y,0);
            for(int i=0;i<2;i++)
            {
                if(!Ground(Home(i),out var point,out var normal)) return;
                var foot=support[i];foot.position=point;foot.normal=normal;foot.rotation=SoleRotation(normal);
                foot.swinging=foot.gaitSwing=foot.liftedThisCycle=false;foot.progress=foot.pitch=foot.toeBend=0;
            }
            previousPosition=transform.position;previousVelocity=velocity=acceleration=Vector3.zero;
            CurrentGait=Gait.Standing;stopping=false;gaitWeight=jogWeight=phase=FlightTime=0;
            pelvisY=transform.position.y+HipHeight;hipLift=HipHeight;nextFoot=0;initialized=true;
            handsInitialized=false;
            UpdateAnkles();
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
            measured.y=0;
            // A navigation correction is a discontinuity, not locomotion; it must not start a gait.
            if(measured.magnitude>Mathf.Max(8,(agent!=null?agent.speed:4)*2.5f)) measured=velocity;
            velocity=Vector3.Lerp(velocity,measured,1-Mathf.Exp(-12*dt));
            Vector3 change=(velocity-previousVelocity)/dt;previousVelocity=velocity;
            acceleration=Vector3.Lerp(acceleration,Vector3.ClampMagnitude(change,8),1-Mathf.Exp(-7*dt));
            Quaternion desiredFacing=Quaternion.Euler(0,transform.eulerAngles.y,0);
            if(Working)
            {
                var direction=Vector3.ProjectOnPlane(worker.WorkContact-transform.position,Vector3.up);
                if(direction.sqrMagnitude>.001f) desiredFacing=Quaternion.LookRotation(direction);
            }
            facing=Quaternion.RotateTowards(facing,desiredFacing,220*dt);
            UpdateGait(dt);
            UpdateFeet(dt);
            // Mutually unreachable contacts indicate a presentation discontinuity,
            // just like the large root correction handled above.
            if((ReachCenter(0)-ReachCenter(1)).sqrMagnitude>4*LegReach*LegReach){ResetPose();return;}
            Pose(dt);
        }
        private float Duty(float froude)=>CurrentGait==Gait.Jogging?.38f:Mathf.Lerp(.64f,.56f,Mathf.Clamp01(froude/.5f));
        private void UpdateGait(float dt)
        {
            float speed=velocity.magnitude,froude=speed*speed/(Gravity*HipHeight);
            bool moving=CurrentGait==Gait.Standing?speed>StartSpeed:speed>.08f;
            if(!moving)
            {
                // Finish the stride instead of freezing mid-stride: close the feet with ordinary
                // steps, then stand. Standing never shuffles towards the exact destination.
                if(CurrentGait!=Gait.Standing) stopping=true;
                // Never switch to a walk in mid-flight: a walk must always keep a support foot.
                if(stopping&&CurrentGait==Gait.Jogging&&!Airborne) CurrentGait=Gait.Walking;
                if(stopping&&!support[0].swinging&&!support[1].swinging&&StanceClosed()){CurrentGait=Gait.Standing;stopping=false;}
            }
            else if(CurrentGait==Gait.Standing)
            {
                // Begin from settled feet, stepping first with the foot due next.
                if(!support[0].swinging&&!support[1].swinging) StartGait(froude>JogAbove?Gait.Jogging:Gait.Walking,froude);
            }
            else
            {
                if(stopping) Resume(froude);
                if(CurrentGait==Gait.Jogging) {if(froude<WalkBelow&&!Airborne) CurrentGait=Gait.Walking;}
                else if(froude>JogAbove) CurrentGait=Gait.Jogging;
            }
            bool striding=CurrentGait!=Gait.Standing&&!stopping;
            gaitWeight=Mathf.MoveTowards(gaitWeight,striding?Mathf.Clamp01(speed/1.2f):0,dt*3);
            jogWeight=Mathf.MoveTowards(jogWeight,striding&&CurrentGait==Gait.Jogging?1:0,dt*3);
            if(!striding) return;
            // Alexander's biped relation, stride ≈ 2.3·h·Fr^0.3, slightly shortened for this
            // long-legged body and bounded by its step reach. Cadence then follows from speed.
            float stride=Mathf.Min(HipHeight*2.1f*Mathf.Pow(Mathf.Max(froude,.0001f),.3f),2*stepReach*HipHeight);
            cadence=Mathf.Clamp(speed/Mathf.Max(stride,.05f),.75f,1.8f);
            duty=Duty(froude);
            phase=Mathf.Repeat(phase+Mathf.Min(cadence*dt,.24f),1);
        }
        private void StartGait(Gait gait,float froude)
        {
            CurrentGait=gait;duty=Duty(froude);
            int lead=nextFoot;
            // The leading foot reaches lift-off on the next frame; the other stays planted.
            phase=Mathf.Repeat(duty-.5f*lead-.001f,1);
            for(int i=0;i<2;i++)
            {
                var foot=support[i];foot.lastPhase=Mathf.Repeat(phase+.5f*i,1);
                foot.liftedThisCycle=i!=lead&&foot.lastPhase>=duty;
            }
        }
        // Moving again before the stride closed: continue the cycle from the foot in the air.
        private void Resume(float froude)
        {
            stopping=false;duty=Duty(froude);
            int swinging=support[0].swinging?0:support[1].swinging?1:-1;
            if(swinging<0){StartGait(CurrentGait,froude);return;}
            phase=Mathf.Repeat(duty+support[swinging].progress*(1-duty)-.5f*swinging,1);
            for(int i=0;i<2;i++)
            {
                var foot=support[i];foot.lastPhase=Mathf.Repeat(phase+.5f*i,1);
                foot.liftedThisCycle=i==swinging||foot.lastPhase>=duty;
            }
        }
        private float HomeError(int index)=>Vector3.ProjectOnPlane(Home(index)-support[index].position,Vector3.up).magnitude;
        private float Turn(int index)=>Quaternion.Angle(support[index].rotation,SoleRotation(support[index].normal));
        private bool StanceClosed()=>HomeError(0)<ClosedTolerance&&HomeError(1)<ClosedTolerance&&Turn(0)<40&&Turn(1)<40;
        // Crossed or nearly touching feet cannot be held as a stance.
        private bool StanceNarrow()=>(Quaternion.Inverse(facing)*(support[1].position-support[0].position)).x<NarrowStance;
        private void UpdateFeet(float dt)
        {
            bool gaitActive=CurrentGait!=Gait.Standing&&!stopping;
            for(int i=0;i<2;i++)
            {
                var foot=support[i];
                float footPhase=Mathf.Repeat(phase+.5f*i,1);
                bool wrapped=gaitActive&&footPhase<foot.lastPhase-.5f;
                foot.lastPhase=footPhase;
                if(wrapped) foot.liftedThisCycle=false;
                if(!foot.swinging) continue;
                if(foot.gaitSwing&&gaitActive)
                {
                    foot.rate=cadence/Mathf.Max(.05f,1-duty);
                    foot.progress=Mathf.Max(foot.progress,wrapped?1:Mathf.Clamp01((footPhase-duty)/(1-duty)));
                }
                // A swing interrupted by stopping finishes on its own clock beneath the hips.
                else foot.progress=Mathf.Min(1,foot.progress+dt*foot.rate);
                Retarget(i);
                SwingPose(foot);
                if(foot.progress>=1) Land(i);
            }
            if(gaitActive)
            {
                for(int i=0;i<2;i++)
                {
                    var foot=support[i];
                    if(foot.swinging||foot.liftedThisCycle||foot.lastPhase<duty) continue;
                    if(CurrentGait==Gait.Walking&&support[1-i].swinging) continue;
                    Lift(i,true);
                }
            }
            // The closing step follows the last stride step at once, beneath the hips.
            else if(stopping)
            {
                if(!support[0].swinging&&!support[1].swinging&&!StanceClosed())
                    Lift(HomeError(0)+Turn(0)*.01f>=HomeError(1)+Turn(1)*.01f?0:1,true);
            }
            // Adjustment steps are for standing. Once the root moves, let the current step land
            // and hand over to the gait; chaining new adjustments would chase the body forever.
            else if(!support[0].swinging&&!support[1].swinging&&velocity.magnitude<=StartSpeed) Settle();
            float toeOff=Mathf.Lerp(WalkToeOff,JogToeOff,jogWeight),heelOff=Mathf.Lerp(.6f,.45f,jogWeight);
            float strike=Mathf.Lerp(HeelStrike,-4,jogWeight);
            for(int i=0;i<2;i++)
            {
                var foot=support[i];if(foot.swinging) continue;
                float target=0;
                if(gaitActive&&!(foot.liftedThisCycle&&foot.lastPhase>=duty))
                {
                    // Heel strike rolls to a flat foot, then the heel rises about the ball.
                    float p=Mathf.Clamp01(foot.lastPhase/duty);
                    target=p<.12f?Mathf.Lerp(strike,0,Mathf.SmoothStep(0,1,p/.12f))
                        :p<heelOff?0:toeOff*Mathf.Pow((p-heelOff)/(1-heelOff),1.6f);
                }
                foot.pitch=Mathf.MoveTowards(foot.pitch,target,(gaitActive?400:90)*dt);
                foot.toeBend=foot.pitch>0?-foot.pitch:0;
            }
            FlightTime=Airborne?FlightTime+dt:0;
            UpdateAnkles();
        }
        private void Lift(int index,bool gaitSwing)
        {
            var foot=support[index];
            foot.swinging=true;foot.gaitSwing=gaitSwing;foot.progress=0;foot.liftedThisCycle=true;
            foot.from=foot.to=foot.position;foot.fromNormal=foot.toNormal=foot.normal;foot.fromRotation=foot.toRotation=foot.rotation;
            foot.liftPitch=foot.pitch;
            foot.clearance=gaitSwing?footLift*Mathf.Lerp(.55f,.9f,jogWeight):.05f;
            foot.rate=gaitSwing?cadence/Mathf.Max(.05f,1-duty):1/stepDuration;
        }
        private void Retarget(int index)
        {
            var foot=support[index];
            Vector3 point=Home(index);
            if(foot.gaitSwing&&CurrentGait!=Gait.Standing&&!stopping)
            {
                // Land where the hips will pass over the foot at mid-stance.
                float remaining=(1-foot.progress)/Mathf.Max(foot.rate,.01f),stance=duty/cadence;
                point+=velocity*remaining+Vector3.ClampMagnitude(velocity*stance*.5f,stepReach*HipHeight*.55f);
                // Near the end of the path, never step past where the body will stop.
                if(agent!=null&&agent.isActiveAndEnabled&&agent.isOnNavMesh&&agent.hasPath&&!agent.pathPending
                    &&agent.remainingDistance<1.5f&&velocity.sqrMagnitude>.0001f)
                {
                    Vector3 travel=velocity.normalized;
                    Vector3 end=agent.pathEndPosition+facing*new Vector3(index==0?-HipWidth:HipWidth,0,.02f);
                    point-=travel*Mathf.Max(0,Vector3.Dot(point-end,travel));
                }
            }
            if(!Ground(point,out var ground,out var normal)||Mathf.Abs(ground.y-transform.position.y)>.8f) return;
            foot.to=ground;foot.toNormal=normal;foot.toRotation=SoleRotation(normal);
        }
        private static void SwingPose(Foot foot)
        {
            float s=foot.progress,e=s*s*(3-2*s);
            foot.position=Vector3.Lerp(foot.from,foot.to,e)+Vector3.up*(foot.clearance*Mathf.Sin(Mathf.PI*Mathf.Pow(s,.75f)));
            foot.normal=Vector3.Slerp(foot.fromNormal,foot.toNormal,e).normalized;
            foot.rotation=Quaternion.Slerp(foot.fromRotation,foot.toRotation,e);
            if(foot.gaitSwing)
            {
                // Toe-off carries through, the toes relax, then the foot prepares a heel strike.
                foot.pitch=Mathf.Lerp(foot.liftPitch,HeelStrike,e);
                foot.toeBend=foot.liftPitch>0?-foot.liftPitch*(1-Mathf.SmoothStep(0,1,s/.3f)):0;
            }
            else {foot.pitch=Mathf.Lerp(foot.liftPitch,0,e)+6*Mathf.Sin(Mathf.PI*s);foot.toeBend=0;}
        }
        private void Land(int index)
        {
            var foot=support[index];
            foot.swinging=false;foot.progress=0;
            foot.position=foot.to;foot.normal=foot.toNormal;foot.rotation=foot.toRotation;
            if(!foot.gaitSwing) foot.pitch=0;
            foot.gaitSwing=false;nextFoot=1-index;StepCount++;
        }
        // Standing adjustments: one foot at a time, preferring the foot due to step next. A
        // settled stance tolerates small drift; only a real turn, a push or a crossed stance
        // makes the body step.
        private void Settle()
        {
            bool narrow=StanceNarrow();
            for(int order=0;order<2;order++)
            {
                int i=(nextFoot+order)%2;var foot=support[i];
                if(HomeError(i)<SettleTolerance&&Turn(i)<40&&!narrow) continue;
                if(!Ground(Home(i),out var point,out var normal)||Mathf.Abs(point.y-transform.position.y)>.8f) return;
                Lift(i,false);foot.to=point;foot.toNormal=normal;foot.toRotation=SoleRotation(normal);
                return;
            }
        }
        // The rendered foot pitches about its heel (toe up) or its ball (heel up).
        private Vector3 FootPoint(Foot foot,Vector3 local)
        {
            var pivot=new Vector3(0,0,foot.pitch>0?BallLength:-HeelLength);
            return foot.position+foot.rotation*(pivot+Quaternion.Euler(foot.pitch,0,0)*(local-pivot));
        }
        private void UpdateAnkles(){for(int i=0;i<2;i++) support[i].ankle=FootPoint(support[i],new Vector3(0,AnkleHeight,0));}
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
        private Vector3 ReachCenter(int index)=>support[index].ankle-facing*new Vector3(index==0?-HipWidth:HipWidth,0,0);
        // Only planted feet support the pelvis. A swinging foot is carried by its leg instead.
        private Vector3 ReachableHips(Vector3 desired)
        {
            bool first=!support[0].swinging,second=!support[1].swinging;
            if(!first&&!second) return desired;
            if(first!=second)
            {
                Vector3 only=ReachCenter(first?0:1);
                return only+Vector3.ClampMagnitude(desired-only,LegReach);
            }
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
                selectionRing.position=transform.position+Vector3.up*(.14f*P.scale);
                selectionRing.rotation=Quaternion.FromToRotation(Vector3.up,(support[0].normal+support[1].normal).normalized);
            }
            float speed=velocity.magnitude,cycle=2*Mathf.PI*phase,walk=gaitWeight*(1-jogWeight),jog=gaitWeight*jogWeight;
            var equipment=GetComponent<EquippedTool>();
            // Carrying with both hands suppresses most of the torso's counter-rotation.
            bool burdened=(equipment!=null&&equipment.HandsOnTool)||(worker!=null&&worker.Carried>0);
            // Walking: the pelvis is lowest in double support and highest over the stance foot.
            // Jogging: it compresses through stance and rises in flight.
            float bounce=(-walk*.018f*Mathf.Cos(2*(cycle-Mathf.PI*(duty-.5f)))
                -jog*(.035f*Mathf.Cos(2*(cycle-Mathf.PI*duty))+.04f))*P.bounce;
            // The pelvis shifts over the stance foot, rotates with the forward leg and
            // drops slightly on the swing side.
            float sway=-gaitWeight*Mathf.Lerp(.025f,.012f,jogWeight)*Mathf.Cos(cycle-Mathf.PI*duty)*P.sway;
            float yaw=gaitWeight*Mathf.Lerp(5,6,jogWeight)*Mathf.Cos(cycle)*(burdened?.3f:1);
            float list=walk*2.5f*Mathf.Cos(cycle-Mathf.PI*(1+duty))*P.sway;
            float standingHeight=transform.position.y+HipHeight+bounce,height=standingHeight;
            bool horizontalOverreach=false;
            for(int i=0;i<2;i++)
            {
                if(support[i].swinging) continue;
                Vector3 hip=Home(i),ankle=support[i].ankle;
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
            float pitch=Mathf.Clamp(localAcceleration.z*1.8f+speed*1.2f+jog*3,-7,12),roll=Mathf.Clamp(-localAcceleration.x*1.6f,-6,6);
            if(equipment!=null && equipment.Busy) pitch+=Mathf.Sin(equipment.Progress*Mathf.PI)*4;
            // posture: the steady body frame used by tools and carried loads.
            Quaternion posture=facing*Quaternion.Euler(pitch,0,roll);
            Quaternion hipFrame=posture*Quaternion.Euler(0,yaw,list);
            Quaternion chest=posture*Quaternion.Euler(jog*3,-yaw*.8f,0);
            // pelvisY is the smoothed target height. The reach projection is solved from it every
            // frame and never written back: feeding the projected height into the next target
            // ratchets the pelvis toward the ground whenever both feet are out of reach.
            Vector3 hips=ReachableHips(transform.position+facing*new Vector3(sway,0,0)+Vector3.up*(pelvisY-transform.position.y));
            // When support passes to a foot that allows a higher pelvis, rise smoothly. This filters
            // the output only; lowering stays immediate so planted legs always reach.
            float lift=hips.y-transform.position.y;
            if(dt>0&&lift>hipLift) {lift=Mathf.Lerp(hipLift,lift,1-Mathf.Exp(-12*dt));hips.y=transform.position.y+lift;}
            hipLift=lift;
            Vector3 waist=hips+hipFrame*new Vector3(0,P.waistRise,0);
            pelvis.SetPositionAndRotation(hips,hipFrame);
            torso.SetPositionAndRotation(waist+chest*new Vector3(0,P.torsoRise,0),chest);
            // The head stays level and looks along the path while the chest twists beneath it.
            head.SetPositionAndRotation(waist+chest*new Vector3(0,P.headRise,0),Quaternion.Slerp(facing,posture,.4f));
            if(carriedBundle!=null)
            {
                bool stockpile=worker!=null && worker.MiningTarget!=null && worker.State==Gatherer.Activity.Gathering;
                carriedBundle.SetPositionAndRotation(stockpile ? transform.position+facing*(new Vector3(.55f,.18f,-.15f)*P.scale)
                    : hips+posture*(new Vector3(0,-.08f,.37f)*P.scale),posture);
                float fullness=worker!=null?Mathf.Clamp01((float)worker.Carried/Mathf.Max(1,worker.Capacity)):0;
                carriedBundle.localScale=bundleScale*Mathf.Lerp(.8f,1,fullness);
            }
            Vector3 leftShoulder=waist+chest*new Vector3(-P.shoulder.x,P.shoulder.y,P.shoulder.z),rightShoulder=waist+chest*new Vector3(P.shoulder.x,P.shoulder.y,P.shoulder.z);
            // One explicit update order: supported torso -> tool trajectory/contact -> grip IK.
            if(equipment!=null) equipment.SolveFrame(dt,hips,posture,leftShoulder,rightShoulder,!support[0].swinging&&!support[1].swinging);
            float armAmplitude=Mathf.Lerp(Mathf.Lerp(.05f,.22f,Mathf.Clamp01(speed/2.2f)),.16f,jogWeight)*gaitWeight*(P.armHang.y/.83f)*P.armSwing;
            bool hasToes=Pair(toes);
            for(int i=0;i<2;i++)
            {
                float side=i==0?-1:1;var foot=support[i];
                Vector3 hip=hips+hipFrame*new Vector3(side*HipWidth,0,0),ankle=foot.ankle,carry=Vector3.zero;
                if(foot.swinging&&(ankle-hip).sqrMagnitude>LegReach*LegReach) carry=hip+(ankle-hip).normalized*LegReach-ankle;
                ankle+=carry;
                Quaternion sole=foot.rotation*Quaternion.Euler(foot.pitch,0,0);
                var size=feet[i].localScale;
                feet[i].SetPositionAndRotation(FootPoint(foot,new Vector3(0,size.y*.5f,(hasToes?BallLength:BallLength+ToeLength)-size.z*.5f))+carry,sole);
                if(hasToes)
                {
                    Quaternion toe=sole*Quaternion.Euler(foot.toeBend,0,0);
                    toes[i].SetPositionAndRotation(FootPoint(foot,new Vector3(0,0,BallLength))+carry+toe*new Vector3(0,toes[i].localScale.y*.5f,ToeLength*.5f),toe);
                }
                Vector3 axis=(ankle-hip).normalized;float distance=Mathf.Min(Vector3.Distance(hip,ankle),LegLimit);
                Vector3 bend=Vector3.ProjectOnPlane(hipFrame*Vector3.forward,axis).normalized;
                Vector3 knee=(hip+ankle)*.5f+bend*Mathf.Sqrt(Mathf.Max(0,P.legSegment*P.legSegment-distance*distance*.25f));
                Segment(thighs[i],hip,knee,.19f*P.scale);Segment(shins[i],knee,ankle,.15f*P.scale);
                // Each arm swings against its own leg: back as that foot reaches forward.
                float swing=-armAmplitude*Mathf.Cos(cycle+Mathf.PI*i);
                Vector3 shoulder=i==0?leftShoulder:rightShoulder;
                // A relaxed arm hangs nearly straight and swings as a pendulum from the shoulder;
                // a jogging arm bends and pumps.
                float drop=P.armHang.y,angle=swing/drop;
                Vector3 relaxed=shoulder+chest*new Vector3(side*P.armHang.x,-drop*Mathf.Cos(angle),P.armHang.z+drop*Mathf.Sin(angle));
                Vector3 pumping=shoulder+chest*(new Vector3(side*.02f,-.34f,.24f)*P.scale+new Vector3(0,0,swing*.8f));
                Vector3 wrist=Vector3.Lerp(relaxed,pumping,jogWeight);
                bool busy=false;
                if(worker!=null)
                {
                    busy=worker.Carried>0||(equipment!=null&&equipment.HandsOnTool)||(i==1&&Working&&worker.MiningTarget==null);
                    if(worker.Carried>0) wrist=hips+posture*(new Vector3(side*.22f,-.12f,.37f)*P.scale);
                    if(i==1&&Working&&worker.MiningTarget==null)
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
                    if(equipment!=null && equipment.HandsOnTool)
                    {
                        wrist=equipment.GripPosition(i);
                        handLocal[i]=Quaternion.Inverse(posture)*(wrist-hips);
                    }
                }
                // Free arms point their elbows back; carrying and gripping turn them out and down.
                handsBusy[i]=dt>0&&handsInitialized?Mathf.MoveTowards(handsBusy[i],busy?1:0,dt*4):busy?1:0;
                SolveArm(i,Vector3.Lerp(new Vector3(side*.2f,-.1f,-1),new Vector3(side*.55f,-.6f,-.25f),handsBusy[i]),chest,shoulder,wrist);
            }
            handsInitialized=true;
        }
        private void SolveArm(int index,Vector3 bendHint,Quaternion posture,Vector3 shoulder,Vector3 target)
        {
            float upper=P.upperArm,lower=P.forearm;
            Vector3 delta=target-shoulder;
            Vector3 axis=delta.sqrMagnitude>.000001f?delta.normalized:posture*Vector3.down;
            float distance=Mathf.Clamp(delta.magnitude,.02f,upper+lower-.001f);
            Vector3 wrist=shoulder+axis*distance;
            float along=(upper*upper-lower*lower+distance*distance)/(2*distance);
            Vector3 bend=Vector3.ProjectOnPlane(posture*bendHint,axis).normalized;
            if(bend.sqrMagnitude<.001f) bend=Vector3.Cross(axis,posture*Vector3.forward).normalized;
            Vector3 elbow=shoulder+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,upper*upper-along*along));
            Segment(upperArms[index],shoulder,elbow,.13f*P.scale);Segment(forearms[index],elbow,wrist,.11f*P.scale);
            handPositions[index]=transform.InverseTransformPoint(wrist);
        }
    }
}
