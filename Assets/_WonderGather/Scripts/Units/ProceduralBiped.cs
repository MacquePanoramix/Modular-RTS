using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // Navigation owns the root. The body presents it with a phase-based gait whose stride,
    // cadence and walk/jog choice follow from hip height and actual displacement. Planted
    // feet keep fixed world-space support frames; heel strike, roll and toe-off only rotate
    // the rendered foot about those frames. Equipment consumes the supported torso pose
    // before the arms consume its solved grips.
    // A body with hands of its own (a modelled being). It says which hands are free to hold a tool, and where a
    // wrist must be for a hand to hold a handle; and it is told, each frame, what each hand holds, so that it can
    // turn the hand onto the handle and close the fingers. Hand 0 is the left, 1 the right.
    // Something that says where a hand must be (a real object in it, moved by forces): the body is told how it stands,
    // then asked for each guided hand's wrist. The arm follows; it does not place the object.
    public interface IArmGuide
    {
        void Stands(Vector3 hips, Quaternion posture, Vector3 leftShoulder, Vector3 rightShoulder);
        bool Guides(int hand);
        Vector3 Wrist(int hand, Vector3 shoulder);
    }

    public interface IHandHolds
    {
        bool HandFree(int hand);
        // The wrist's place for this hand to hold a handle of this radius whose axis passes through grip and
        // runs along handle (towards the tool's head), the arm reaching from shoulder.
        Vector3 WristFor(int hand, Vector3 grip, Vector3 handle, float radius, Vector3 shoulder);
        // This frame the hand holds such a handle (on), or nothing.
        void HoldHandle(int hand, bool on, Vector3 grip, Vector3 handle, float radius, Vector3 shoulder);
    }

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
            // Each arm's own carriage (x the left arm, y the right): how much further out than armHang it hangs (an
            // arm that carries a lantern holds it clear of the coat), and how much of the walk's swing it keeps
            // (a carrying arm swings less). Zero swing reads as 1, so a body saved before this walks as it did.
            public Vector2 armCarry, armSwingSide;
            // The body's own shape, for a tool swung in front of it (zero: not measured, as on the first body): how
            // far its front stands ahead of the hips between hips and shoulders, how far its face does, and its
            // head's half-width.
            public float bodyFront, faceFront, headHalf;
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
        // As they are saved with the body. (Until the body wakes, the active ones are still the first body's.)
        public Proportions SavedProportions=>customProportions?proportions:Proportions.Default;
        // For tools. The first body (2.2 m) and its pickaxe set the pattern: its arms reach 0.87 m, its lower hand
        // works 3 cm below the hips and 24 cm in front of them, and a wrist that holds a tool is between 2.5 and
        // 86.5 cm from its shoulder. A body with its own measurements keeps those proportions of its own arm; the
        // first body keeps the exact numbers.
        public float ArmReach=>P.upperArm+P.forearm;
        public float ToolScale=>customProportions?ArmReach/.87f:1;
        public float ToolReachMax=>customProportions?ArmReach-.005f*ToolScale:.865f;
        public float ToolReachMin=>customProportions?.025f*ToolScale:.025f;
        // Where the lower hand holds a tool at work, from the hips in the body's steady frame: below the shoulders
        // by the same share of the arm, and as far forward.
        // A body with a front of its own (a coat, a belly, an apron) holds the tool clear of it: by room for the
        // hand that is between the handle and the body.
        public Vector3 ToolHand=>customProportions?new Vector3(0,P.waistRise+P.shoulder.y-ArmReach*(.52f/.87f),Mathf.Max(ArmReach*(.24f/.87f),P.bodyFront+ArmReach*HandRoom)):new Vector3(0,-.03f,.24f);
        private const float HandRoom=.16f;
        // The right shoulder, from the hips in the body's steady frame.
        public Vector3 ShoulderFromHips=>new Vector3(P.shoulder.x,P.waistRise+P.shoulder.y,P.shoulder.z);
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
            if(P.armSwingSide.x<=0) P.armSwingSide.x=1;
            if(P.armSwingSide.y<=0) P.armSwingSide.y=1;
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
            // It goes to a place of the body's own choosing (StepTo), not to where the stance would have it.
            public bool own;
            // Which side of the standing boot this swing bows round (0 until it needs to).
            public float bow;
        }
        private readonly Foot[] support={new Foot(),new Foot()};
        private readonly Vector3[] handPositions=new Vector3[2];
        private readonly Vector3[] handLocal=new Vector3[2];
        private readonly float[] handsBusy=new float[2];
        private NavMeshAgent agent;
        private IHandHolds holds;
        private bool holdsLooked;
        private IHandHolds Holds { get { if(!holdsLooked) { holds=GetComponent<IHandHolds>(); holdsLooked=true; } return holds; } }
        private IArmGuide guide,also;
        // Set by what guides the arms when it begins and ends.
        public void GuideArms(IArmGuide value)=>guide=value;
        public bool Guided=>guide!=null;
        // A second guide, for a hand the first does not guide (something small taken in one hand).
        public void GuideAlso(IArmGuide value)=>also=value;
        // Where a hand's wrist would be with nothing to guide it, as the body was last posed.
        private readonly Vector3[] freeWrist=new Vector3[2];
        public Vector3 FreeWrist(int index)=>freeWrist[index];
        // An arm that carries something hanging from its hand (a lantern, a mug): it hangs this much further out
        // (metres), so that the thing clears the clothes, and keeps this share of its swing. (0 and 1: a free arm.)
        private readonly float[] carriesOut=new float[2],carriesKeep={1,1},carriedOut=new float[2],carriedKeep={1,1};
        public void CarryAtSide(int index,float metres,float keep){carriesOut[index]=Mathf.Max(0,metres);carriesKeep[index]=Mathf.Clamp01(keep);}
        // How the body stood when it was last posed: for what works on the physics' own clock.
        private Vector3 hipsNow;
        private Quaternion postureNow=Quaternion.identity;
        private readonly Vector3[] shoulderNow=new Vector3[2],elbowNow=new Vector3[2];
        public Vector3 HipsNow=>hipsNow;
        public Quaternion PostureNow=>postureNow;
        public Vector3 ShoulderNow(int index)=>shoulderNow[index];
        // The body is posed once a frame; the physics steps on its own clock, sometimes twice in a frame. Read as it
        // was last posed, the body would move in stairs. This is how it will stand at a moment a little after it was
        // last posed, going on as it was going.
        private Vector3 hipsBefore;
        private Quaternion postureBefore=Quaternion.identity;
        private readonly Vector3[] shoulderBefore=new Vector3[2],elbowBefore=new Vector3[2];
        private float posedAt=-1,posedBefore=-1;
        // What moves the body on the physics' own clock says where it has it now (the back its bow, in degrees; the
        // balance its lean, in metres to the right and ahead). The body as it was last drawn, moved by what they have
        // done since, is then how it stands at this step, whatever the frame rate. NaN: nothing says.
        private float bowIs=float.NaN,bowPosed;
        private Vector2 leanIs=new Vector2(float.NaN,0);
        public void BowIs(float degrees)=>bowIs=degrees;
        public void LeanIs(Vector2 metres)=>leanIs=metres;
        public void StandsAt(float time,out Vector3 hips,out Quaternion posture,System.Span<Vector3> shoulders,System.Span<Vector3> elbows)
        {
            bool bowKnown=!float.IsNaN(bowIs),leanKnown=!float.IsNaN(leanIs.x);
            if(bowKnown||leanKnown)
            {
                Vector3 shift=leanKnown?facing*new Vector3(leanIs.x-leanPosed.x,0,leanIs.y-leanPosed.y):Vector3.zero;
                Quaternion turn=bowKnown?Quaternion.AngleAxis(bowIs-bowPosed,postureNow*Vector3.right):Quaternion.identity;
                // Walking, the whole body goes on as it was going since it was last drawn.
                shift+=velocity*Mathf.Clamp(time-posedAt,0,.1f);
                hips=hipsNow+shift;posture=turn*postureNow;
                for(int i=0;i<2;i++)
                {
                    shoulders[i]=hips+turn*(shoulderNow[i]-hipsNow);
                    elbows[i]=hips+turn*(elbowNow[i]-hipsNow);
                }
                return;
            }
            float span=posedAt-posedBefore;
            float on=posedBefore>=0&&span>1e-6f?Mathf.Clamp((time-posedAt)/span,0,4):0;
            hips=hipsNow+(hipsNow-hipsBefore)*on;
            posture=on>0?Quaternion.SlerpUnclamped(postureBefore,postureNow,1+on):postureNow;
            for(int i=0;i<2;i++)
            {
                shoulders[i]=shoulderNow[i]+(shoulderNow[i]-shoulderBefore[i])*on;
                elbows[i]=elbowNow[i]+(elbowNow[i]-elbowBefore[i])*on;
            }
        }
        // The body bows from the hips by an angle (forward is positive), at its own pace: for work that needs the
        // hands low or the weight thrown forward. Asked for by what plans the work; 0 stands it up again.
        private float bowWanted,bow,bowPace=BowPace;
        private const float BowPace=150;
        public void Bow(float degrees)=>Bow(degrees,BowPace);
        // pace: degrees a second (a back that moves by its own strength says how fast it is going).
        public void Bow(float degrees,float pace){bowWanted=Mathf.Clamp(degrees,-15,MostBowed);bowPace=Mathf.Max(1,pace);}
        // A body bends over no further than this (to reach the ground).
        public const float MostBowed=80;
        public float BowNow=>bow;
        // The hips go back behind the feet by so much (forward is negative), and sink by so much (the knees bend),
        // each at its own pace: to keep the body's weight over its feet when it bows, and to reach low.
        private float backWanted,back,sinkWanted,sink;
        private const float BackPace=.5f,SinkPace=.6f;
        public void SetBack(float metres)=>backWanted=Mathf.Clamp(metres,-.08f*HipHeight,.28f*HipHeight);
        public void Sink(float metres)=>sinkWanted=Mathf.Clamp(metres,0,DeepestSink*HipHeight);
        // The hips sink no further than this share of their height (a deep squat, to reach the ground).
        public const float DeepestSink=.6f;
        public float BackWanted=>backWanted;
        public float BackNow=>back;
        public float SinkNow=>sink;
        public float StandingHipHeight=>HipHeight;
        // The body's lean: how far its hips stand from where its stance alone would put them, to its right (x) and
        // ahead (y), as its balance says (PhysicalBalance). The balance moves on the physics' clock and the body is
        // drawn between its steps, so it goes there at a pace it is given. The hips go no further than the legs
        // allow: these shares of the hips' height.
        public const float LeanAside=.16f,LeanBack=.28f,LeanAhead=.1f;
        private Vector2 leanWanted,lean;
        private float leanPace=1;
        public void SetLean(Vector2 metres,float pace)
        {
            leanWanted=new Vector2(Mathf.Clamp(metres.x,-LeanAside*HipHeight,LeanAside*HipHeight),Mathf.Clamp(metres.y,-LeanBack*HipHeight,LeanAhead*HipHeight));
            leanPace=Mathf.Max(.01f,pace);
        }
        public Vector2 LeanNow=>lean;
        // The lean the body was last drawn with.
        private Vector2 leanPosed;
        public Vector2 LeanPosed=>leanPosed;
        public Quaternion FacingNow=>facing;
        // How fast the body is going over the ground, as it measures it.
        public Vector3 VelocityNow=>velocity;
        // When the body was last posed.
        public float PosedAt=>posedAt;
        // The stance: how much further apart than the hips the feet stand (each side), and how far the left foot
        // stands ahead of the right. The feet step to it, one at a time.
        private float stanceWider,stanceStagger;
        private bool stanceDue;
        public void SetStance(float wider,float stagger)
        {
            // No further apart than the legs span.
            wider=Mathf.Clamp(wider,0,Mathf.Max(0,.5f*HipHeight-HipWidth));stagger=Mathf.Clamp(stagger,-HipHeight,HipHeight);
            if(Mathf.Abs(wider-stanceWider)<.005f&&Mathf.Abs(stagger-stanceStagger)<.005f) return;
            stanceWider=wider;stanceStagger=stagger;stanceDue=true;
        }
        public float StanceWider=>stanceWider;
        public float StanceStagger=>stanceStagger;
        // The feet are where the stance asked for has them.
        public bool StanceTaken=>!stanceDue&&!support[0].swinging&&!support[1].swinging;
        // A foot is due to step to its place in the stance, and waits for the body's weight to be off it: what keeps
        // the body's balance says when it may lift (PhysicalBalance). -1: none waits.
        public System.Func<int,bool> MayLift;
        private int liftDue=-1;
        public int LiftDue=>liftDue;
        // The middle of the stance: where the body stands between its feet's own places.
        public Vector3 StanceMiddle=>transform.position+facing*new Vector3(0,0,.02f);
        // The body inclines as a whole against what loads it (PhysicalBalance): to its right (x) and forwards (y), in
        // degrees, at a pace.
        private Vector2 tiltWanted,tilt;
        private float tiltPace=40;
        public void SetTilt(Vector2 degrees,float pace){tiltWanted=Vector2.ClampMagnitude(degrees,15);tiltPace=Mathf.Max(1,pace);}
        // Braced, the knees bend: the hips sink by so much more, whatever else asks them to sink.
        private float crouchWanted,crouch;
        public void Crouch(float metres)=>crouchWanted=Mathf.Clamp(metres,0,.3f*HipHeight);
        // How far each knee stands out from the line between its hip and its ankle, in metres (how bent the leg is);
        // and how far it stands out when the leg is as straight as it goes.
        private readonly float[] kneeOut=new float[2];
        public float KneeOut(int index)=>kneeOut[index];
        public float KneeOutStraight=>Mathf.Sqrt(Mathf.Max(0,P.legSegment*P.legSegment-LegReach*LegReach*.25f));
        // How fast the hips rise, as a share of their own pace: the legs' own strength says (PhysicalBalance).
        private float rises=1;
        public void SetRise(float share)=>rises=Mathf.Clamp01(share);
        // A boot's line on the ground, from heel to toe, and half its width: what the body stands on.
        public void Sole(int index,out Vector3 heel,out Vector3 toe,out float halfWidth)
        {
            var foot=support[index];
            Vector3 ahead=foot.rotation*Vector3.forward;
            heel=foot.position-ahead*HeelLength;toe=foot.position+ahead*(BallLength+ToeLength);halfWidth=BootHalfWidth;
        }
        // Where a foot stands when the body stands as its stance has it.
        public Vector3 FootHome(int index)=>Home(index);
        // A step taken at once to a place of the body's own choosing (to catch its balance): the foot goes there in
        // the time given, and stays there. Only from standing on both feet.
        public bool StepTo(int index,Vector3 point,float duration,out Vector3 lands)
        {
            lands=point;
            if(!initialized||CurrentGait!=Gait.Standing||stopping||support[0].swinging||support[1].swinging) return false;
            // It never lands on the standing boot: where it would, it lands clear of it.
            var other=support[1-index];
            Quaternion landing=SoleRotation(Vector3.up);
            if(BootGap(point,landing,other.position,other.rotation,out var away)<BootRoom)
            {
                Vector3 side=facing*new Vector3(index==0?-1:1,0,0);
                float straight=away!=Vector3.zero?StepClear(point,landing,other,away,BootRoom):-1,aside=StepClear(point,landing,other,side,BootRoom);
                if(straight>=0&&(aside<0||straight<=aside)) point+=away*straight;
                else if(aside>=0) point+=side*aside;
            }
            if(!Ground(point,out var ground,out var normal)||Mathf.Abs(ground.y-transform.position.y)>.8f) return false;
            var foot=support[index];
            Lift(index,false);
            foot.own=true;foot.to=ground;foot.toNormal=normal;foot.toRotation=SoleRotation(normal);
            foot.rate=1/Mathf.Clamp(duration,.12f,.6f);foot.clearance=.09f*HipHeight;
            lands=ground;
            return true;
        }
        // The root goes where the body's balance takes it (a step taken to catch itself). This is not walking, and
        // starts no gait. The hips stay where they are: the lean takes up what the root gave.
        public Vector3 Shift(Vector3 delta)
        {
            delta.y=0;
            Vector3 before=transform.position;
            if(agent!=null&&agent.isActiveAndEnabled&&agent.isOnNavMesh) agent.Move(delta);else transform.position+=delta;
            Vector3 moved=transform.position-before;
            previousPosition+=moved;
            Vector3 local=Quaternion.Inverse(facing)*moved;
            var took=new Vector2(local.x,local.z);
            lean-=took;leanWanted-=took;
            return moved;
        }
        // How far ahead of where a foot is placed its middle is: where it carries weight best.
        public float FootMiddle=>(BallLength+ToeLength-HeelLength)*.5f;
        public Vector3 ElbowNow(int index)=>elbowNow[index];
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
        // How far through its swing a foot is (0 lifting, 1 landing), or -1 when it is planted.
        public float SwingProgress(int index)=>support[index].swinging?support[index].progress:-1;
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
        private Vector3 Home(int index)=>transform.position+facing*new Vector3(index==0?-HipWidth-stanceWider:HipWidth+stanceWider,0,.02f+(index==0?.5f:-.5f)*stanceStagger);
        // Where a hip stands over the ground when the body stands upright where it is.
        private Vector3 HipOver(int index)=>transform.position+facing*new Vector3(index==0?-HipWidth:HipWidth,0,.02f);
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
            pelvisY=pelvisWas=transform.position.y+HipHeight;pelvisPace=0;hipLift=HipHeight;hipPace=0;comingDown=-1;nextFoot=0;initialized=true;
            handsInitialized=false;
            UpdateAnkles();
            Pose(0);
        }
        // Let go, the body is not posed: something else puts its segments where they are (PhysicalFall). Taken back,
        // it is posed afresh where it then stands (ResetPose).
        public bool LetGo {get;set;}
        private void LateUpdate()
        {
            RefreshCargo();
            if(LetGo){previousPosition=transform.position;return;}
            if(!initialized){ResetPose();return;}
            float dt=Time.deltaTime;if(dt<=0) return;
            frame=dt;
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
            liftDue=-1;
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
                SwingPose(foot,support[1-i],facing*new Vector3(i==0?-1:1,0,0));
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
            foot.swinging=true;foot.gaitSwing=gaitSwing;foot.progress=0;foot.liftedThisCycle=true;foot.bow=0;foot.own=false;
            foot.from=foot.to=foot.position;foot.fromNormal=foot.toNormal=foot.normal;foot.fromRotation=foot.toRotation=foot.rotation;
            foot.liftPitch=foot.pitch;
            foot.clearance=gaitSwing?footLift*Mathf.Lerp(.55f,.9f,jogWeight):.05f;
            foot.rate=gaitSwing?cadence/Mathf.Max(.05f,1-duty):1/stepDuration;
        }
        private void Retarget(int index)
        {
            var foot=support[index];
            if(foot.own) return;
            Vector3 point=Home(index);
            if(foot.gaitSwing&&CurrentGait!=Gait.Standing&&!stopping)
            {
                // Land where the hips will pass over the foot at mid-stance.
                float remaining=LandsIn(foot),stance=duty/cadence;
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
            // A foot never lands on the standing one: where its boot would overlap that boot (in a turn the two
            // point different ways), it lands beside it.
            var other=support[1-index];
            if(!other.swinging)
            {
                Quaternion landing=SoleRotation(Vector3.up);
                if(BootGap(point,landing,other.position,other.rotation,out var away)<BootRoom)
                {
                    // The shorter way clear: straight away from the other boot, or out to its own side.
                    Vector3 side=facing*new Vector3(index==0?-1:1,0,0);
                    float straight=away!=Vector3.zero?StepClear(point,landing,other,away,BootRoom):-1,aside=StepClear(point,landing,other,side,BootRoom);
                    if(straight>=0&&(aside<0||straight<=aside)) point+=away*straight;
                    else if(aside>=0) point+=side*aside;
                }
            }
            if(!Ground(point,out var ground,out var normal)||Mathf.Abs(ground.y-transform.position.y)>.8f) return;
            foot.to=ground;foot.toNormal=normal;foot.toRotation=SoleRotation(normal);
        }
        // A boot on the ground, seen from above: a line from heel to toe with half a boot's width round it.
        private float BootHalfWidth=>(HeelLength+BallLength+ToeLength)*.2f;
        // The room two boots need between their lines so that they do not touch.
        private float BootRoom=>BootHalfWidth*2+.012f;
        // How far apart two boots' lines are, and the way from the second to the first (zero where they cross).
        private float BootGap(Vector3 a,Quaternion ra,Vector3 b,Quaternion rb,out Vector3 away)
        {
            Vector3 fa=Vector3.ProjectOnPlane(ra*Vector3.forward,Vector3.up).normalized,fb=Vector3.ProjectOnPlane(rb*Vector3.forward,Vector3.up).normalized;
            float toe=BallLength+ToeLength;
            Vector3 a0=a-fa*HeelLength,a1=a+fa*toe,b0=b-fb*HeelLength,b1=b+fb*toe;
            a0.y=a1.y=b0.y=b1.y=0;
            Closest(a0,a1,b0,b1,out var onA,out var onB);
            away=onA-onB;
            float gap=away.magnitude;
            away=gap>.0005f?away/gap:Vector3.zero;
            return gap;
        }
        // The least step along a way that leaves a boot clear of the other by the room asked (0 when it already
        // is; -1 when no step within reach does). The room left grows steadily once the boot is past the other,
        // so the first clear step is found by walking out, then halving back.
        private float StepClear(Vector3 at,Quaternion turned,Foot other,Vector3 way,float room,float reach=.45f)
        {
            if(BootGap(at,turned,other.position,other.rotation,out _)>=room) return 0;
            const float stride=.02f;
            for(float d=stride;d<=reach;d+=stride)
            {
                if(BootGap(at+way*d,turned,other.position,other.rotation,out _)<room) continue;
                float low=d-stride,high=d;
                for(int k=0;k<5;k++)
                {
                    float mid=(low+high)*.5f;
                    if(BootGap(at+way*mid,turned,other.position,other.rotation,out _)>=room) high=mid;else low=mid;
                }
                return high;
            }
            return -1;
        }
        // The closest points of two segments.
        private static void Closest(Vector3 p1,Vector3 q1,Vector3 p2,Vector3 q2,out Vector3 c1,out Vector3 c2)
        {
            Vector3 d1=q1-p1,d2=q2-p2,r=p1-p2;
            float a=Vector3.Dot(d1,d1),e=Vector3.Dot(d2,d2),f=Vector3.Dot(d2,r),s,t;
            if(a<=1e-8f&&e<=1e-8f){c1=p1;c2=p2;return;}
            if(a<=1e-8f){s=0;t=Mathf.Clamp01(f/e);}
            else
            {
                float c=Vector3.Dot(d1,r);
                if(e<=1e-8f){t=0;s=Mathf.Clamp01(-c/a);}
                else
                {
                    float b=Vector3.Dot(d1,d2),denom=a*e-b*b;
                    s=denom>1e-8f?Mathf.Clamp01((b*f-c*e)/denom):0;
                    t=(b*s+f)/e;
                    if(t<0){t=0;s=Mathf.Clamp01(-c/a);}
                    else if(t>1){t=1;s=Mathf.Clamp01((b-c)/a);}
                }
            }
            c1=p1+d1*s;c2=p2+d2*t;
        }
        // How far apart the two boots are now, less the room they need: negative when they overlap.
        public float BootClearance=>BootGap(support[0].position,support[0].rotation,support[1].position,support[1].rotation,out _)-BootHalfWidth*2;
        private void SwingPose(Foot foot,Foot other,Vector3 ownSide)
        {
            float s=foot.progress,e=s*s*(3-2*s);
            Vector3 along=Vector3.Lerp(foot.from,foot.to,e);
            if(!other.swinging)
            {
                // The swinging boot goes round the standing one, never through it. Both its ends are clear of that
                // boot (a foot never lands on the other); where the straight path between them would pass too
                // close (in a sharp turn the feet's paths cross), it bows out sideways, round the nearer side (its
                // own side when there is little between the two), and stays on that side.
                Vector3 path=foot.to-foot.from;path.y=0;
                Vector3 aside=path.sqrMagnitude>1e-6f?Vector3.Cross(Vector3.up,path.normalized):ownSide;
                Quaternion turned=Quaternion.Slerp(foot.fromRotation,foot.toRotation,e);
                // Never more room than its own ends have: it leaves and lands exactly where its feet are.
                float room=Mathf.Min(BootRoom,Mathf.Min(BootGap(foot.from,foot.fromRotation,other.position,other.rotation,out _),
                    BootGap(foot.to,foot.toRotation,other.position,other.rotation,out _)));
                if(BootGap(along,turned,other.position,other.rotation,out _)<room)
                {
                    if(foot.bow==0)
                    {
                        // The nearer way round; its own side when there is little between them.
                        float one=StepClear(along,turned,other,aside,room),two=StepClear(along,turned,other,-aside,room);
                        float own=Vector3.Dot(aside,ownSide)>=0?1:-1;
                        foot.bow=one<0&&two<0?own:two<0?1:one<0?-1:Mathf.Abs(one-two)<.02f?own:one<two?1:-1;
                    }
                    float round=StepClear(along,turned,other,aside*foot.bow,room);
                    if(round>0) along+=aside*(foot.bow*round);
                }
            }
            foot.position=along+Vector3.up*(foot.clearance*Mathf.Sin(Mathf.PI*Mathf.Pow(s,.75f)));
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
            foot.swinging=false;foot.progress=0;foot.own=false;
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
            // A stance that was asked for is taken exactly; one that is only held tolerates drift.
            float tolerance=stanceDue?SettleTolerance*.12f:SettleTolerance;
            for(int order=0;order<2;order++)
            {
                int i=(nextFoot+order)%2;var foot=support[i];
                if(HomeError(i)<tolerance&&Turn(i)<40&&!narrow) continue;
                if(!Ground(Home(i),out var point,out var normal)||Mathf.Abs(point.y-transform.position.y)>.8f) return;
                // The body's weight comes off a foot before it lifts.
                if(MayLift!=null&&!MayLift(i)){liftDue=i;return;}
                Lift(i,false);foot.to=point;foot.toNormal=normal;foot.toRotation=SoleRotation(normal);
                return;
            }
            stanceDue=false;
        }
        // The rendered foot pitches about its heel (toe up) or its ball (heel up).
        private Vector3 FootPoint(Foot foot,Vector3 local)
        {
            var pivot=new Vector3(0,0,foot.pitch>0?BallLength:-HeelLength);
            return foot.position+foot.rotation*(pivot+Quaternion.Euler(foot.pitch,0,0)*(local-pivot));
        }
        private void UpdateAnkles(){for(int i=0;i<2;i++) support[i].ankle=FootPoint(support[i],new Vector3(0,AnkleHeight,0));}
        // Where the ankle of a foot in the air will be when it lands (as FootPoint will have it then).
        private Vector3 LandingAnkle(Foot foot)
        {
            float pitch=foot.gaitSwing?HeelStrike:0;
            var pivot=new Vector3(0,0,pitch>0?BallLength:-HeelLength);
            return foot.to+foot.toRotation*(pivot+Quaternion.Euler(pitch,0,0)*(new Vector3(0,AnkleHeight,0)-pivot));
        }
        // A foot in the air is on its way down. From this share of its swing on, the pelvis comes down to where
        // that leg will reach its landing, and is there when the foot lands. (Until October 8 only planted feet
        // held the pelvis down: it stayed up through the swing and fell onto the foot in the frame it landed, by
        // 5 to 13 cm going down a slope. Luis: "a slight stutter after each step", "teleported a little bit, like
        // to the ground".)
        private const float ComesDownFrom=.35f;
        // How long until a foot in the air lands. It lands in a frame, not between two: the first in which its swing
        // is done. Reckoned to that frame, where it will land does not move in its last frame in the air, and the
        // hips are where they were expected when it does. (Reckoned to the moment between, the foot jumped ahead
        // by up to a frame's walking as it landed, about a centimetre.)
        private float frame;
        private float LandsIn(Foot foot)
        {
            float left=(1-foot.progress)/Mathf.Max(foot.rate,.01f);
            return frame>0?Mathf.Max(0,Mathf.Ceil(left/frame-.001f))*frame:left;
        }
        // When the foot in the air began to come down: which foot it is, how far through its swing it was, how high
        // the hips stood over the body's place and how fast they were rising or falling (they come down from there,
        // at that pace to begin with), and how high the pelvis was meant to be.
        private int comingDown=-1;
        private float cameWhen,cameFrom,camePace,pelvisFrom,pelvisRose;
        // How fast the pelvis's meant height is rising (m/s), and what it was a frame ago.
        private float pelvisPace,pelvisWas;
        // How fast the hips are rising (m/s; falling is less than nothing), over the body's place; and how long they
        // take to come up to where they may be. They rise with a pace that is kept from frame to frame: let go by a
        // leg, they do not start up at once. (They did: a corner in their line at each foot's leaving the ground.)
        private float hipPace;
        private const float RisesIn=.09f;
        // From one height and pace to another, along a line without a corner: u from 0 to 1 over a time that lasts.
        private static float Comes(float from,float pace,float to,float lasts,float u)
        {
            float uu=u*u,uuu=uu*u;
            return (2*uuu-3*uu+1)*from+(uuu-2*uu+u)*pace*lasts+(3*uu-2*uuu)*to;
        }
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
        private Vector3 ReachableHips(Vector3 desired)=>Reachable(desired,!support[0].swinging,!support[1].swinging,ReachCenter(0),ReachCenter(1));
        // The nearest place to the one wanted from which the legs reach: a and b are where the pelvis would be
        // with the left or the right leg straight down to its ankle; first and second, whether each counts.
        private Vector3 Reachable(Vector3 desired,bool first,bool second,Vector3 a,Vector3 b)
        {
            if(!first&&!second) return desired;
            if(first!=second)
            {
                Vector3 only=first?a:b;
                return only+Vector3.ClampMagnitude(desired-only,LegReach);
            }
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
                Vector3 hip=HipOver(i),ankle=support[i].ankle;
                float horizontal=Vector3.ProjectOnPlane(hip-ankle,Vector3.up).sqrMagnitude;
                horizontalOverreach|=horizontal>LegReach*LegReach;
                height=Mathf.Min(height,ankle.y+Mathf.Sqrt(Mathf.Max(.01f,LegReach*LegReach-horizontal)));
            }
            // Lowering alone cannot repair horizontal overreach after a correction.
            // Let the full reach constraint shift the body back from standing height.
            if(horizontalOverreach) height=standingHeight;
            // The one foot in the air: where it will land, seen from where the hips will be then.
            int air=support[0].swinging==support[1].swinging?-1:support[0].swinging?0:1;
            float arrives=0,comesTo=float.MaxValue;
            Vector3 landing=default;
            if(air>=0&&support[air].progress>=ComesDownFrom)
            {
                var coming=support[air];
                landing=LandingAnkle(coming)-velocity*LandsIn(coming);
                // (Where they stood and how fast they moved are a frame old: the way down begins a frame back.)
                if(comingDown!=air){comingDown=air;cameWhen=Mathf.Max(0,coming.progress-coming.rate*dt);cameFrom=hipLift;camePace=hipPace;pelvisFrom=pelvisY;pelvisRose=pelvisPace;}
                arrives=Mathf.SmoothStep(0,1,Mathf.InverseLerp(cameWhen,1,coming.progress));
                float horizontal=Vector3.ProjectOnPlane(HipOver(air)-landing,Vector3.up).sqrMagnitude;
                // The pelvis is meant no higher than this leg will let it be, by the time the foot lands.
                if(!horizontalOverreach&&horizontal<LegReach*LegReach)
                    comesTo=Comes(pelvisFrom,pelvisRose,landing.y+Mathf.Sqrt(Mathf.Max(.01f,LegReach*LegReach-horizontal)),(1-cameWhen)/Mathf.Max(coming.rate,.01f),Mathf.InverseLerp(cameWhen,1,coming.progress));
            }
            else comingDown=-1;
            pelvisY=dt>0?Mathf.Lerp(pelvisY,height,1-Mathf.Exp(-16*dt)):height;
            // Reach is a hard constraint on lowering; only the upward recovery is smoothed.
            pelvisY=Mathf.Min(pelvisY,Mathf.Min(height,comesTo));
            if(dt>0) pelvisPace=(pelvisY-pelvisWas)/dt;
            pelvisWas=pelvisY;
            Vector3 localAcceleration=Quaternion.Inverse(facing)*acceleration;
            float pitch=Mathf.Clamp(localAcceleration.z*1.8f+speed*1.2f+jog*3,-7,12),roll=Mathf.Clamp(-localAcceleration.x*1.6f,-6,6);
            if(equipment!=null && equipment.Busy) pitch+=Mathf.Sin(equipment.Progress*Mathf.PI)*4;
            bow=dt>0?Mathf.MoveTowards(bow,bowWanted,bowPace*dt):bowWanted;
            back=dt>0?Mathf.MoveTowards(back,backWanted,BackPace*dt):backWanted;
            sink=dt>0?Mathf.MoveTowards(sink,sinkWanted,SinkPace*(sinkWanted<sink?rises:1)*dt):sinkWanted;
            lean=dt>0?Vector2.MoveTowards(lean,leanWanted,leanPace*dt):leanWanted;
            leanPosed=lean;bowPosed=bow;
            tilt=dt>0?Vector2.MoveTowards(tilt,tiltWanted,tiltPace*dt):tiltWanted;
            crouch=dt>0?Mathf.MoveTowards(crouch,crouchWanted,SinkPace*.5f*(crouchWanted<crouch?rises:1)*dt):crouchWanted;
            pitch+=bow+tilt.y;roll-=tilt.x;
            // posture: the steady body frame used by tools and carried loads.
            Quaternion posture=facing*Quaternion.Euler(pitch,0,roll);
            Quaternion hipFrame=posture*Quaternion.Euler(0,yaw,list);
            Quaternion chest=posture*Quaternion.Euler(jog*3,-yaw*.8f,0);
            // pelvisY is the smoothed target height. The reach projection is solved from it every
            // frame and never written back: feeding the projected height into the next target
            // ratchets the pelvis toward the ground whenever both feet are out of reach.
            Vector3 wanted=transform.position+facing*new Vector3(sway+lean.x,0,lean.y-back)+Vector3.up*(pelvisY-transform.position.y-sink-crouch);
            Vector3 hips=ReachableHips(wanted);
            // How high the hips may stand over the body's place: no higher than the legs on the ground reach.
            float may=hips.y-transform.position.y,held=float.MaxValue;
            if(air>=0&&comingDown==air)
            {
                // And they come within reach of both legs as the foot comes down: by the same reach that will hold
                // them once it has landed, so that nothing is left to change in the frame it lands. To the side and
                // along, little by little; in height, down from where they stood and at the pace they had, along
                // a line with no corner in it.
                var coming=support[air];
                Vector3 over=landing-facing*new Vector3(air==0?-HipWidth:HipWidth,0,0);
                Vector3 both=air==0?Reachable(wanted,true,true,over,ReachCenter(1)):Reachable(wanted,true,true,ReachCenter(0),over);
                hips.x=Mathf.Lerp(hips.x,both.x,arrives);hips.z=Mathf.Lerp(hips.z,both.z,arrives);
                held=Comes(cameFrom,camePace,both.y-transform.position.y,(1-cameWhen)/Mathf.Max(coming.rate,.01f),Mathf.InverseLerp(cameWhen,1,coming.progress));
            }
            // They come up to where they may be with a pace that is kept; they are never higher than they may be
            // (legs on the ground always reach), nor than the way down to a landing.
            float lift=may;
            if(dt>0)
            {
                lift=Mathf.Min(Mathf.SmoothDamp(hipLift,may,ref hipPace,RisesIn,float.MaxValue,dt),Mathf.Min(may,held));
                hipPace=(lift-hipLift)/dt;
            }
            hips.y=transform.position.y+lift;hipLift=lift;
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
            if(dt>0)
            {
                hipsBefore=hipsNow;postureBefore=postureNow;shoulderBefore[0]=shoulderNow[0];shoulderBefore[1]=shoulderNow[1];
                elbowBefore[0]=elbowNow[0];elbowBefore[1]=elbowNow[1];
                posedBefore=posedAt;posedAt=Time.time;
            }
            else posedBefore=-1;
            hipsNow=hips;postureNow=posture;shoulderNow[0]=leftShoulder;shoulderNow[1]=rightShoulder;
            if(guide!=null) guide.Stands(hips,posture,leftShoulder,rightShoulder);
            if(also!=null) also.Stands(hips,posture,leftShoulder,rightShoulder);
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
                kneeOut[i]=Mathf.Sqrt(Mathf.Max(0,P.legSegment*P.legSegment-distance*distance*.25f));
                Vector3 knee=(hip+ankle)*.5f+bend*kneeOut[i];
                Segment(thighs[i],hip,knee,.19f*P.scale);Segment(shins[i],knee,ankle,.15f*P.scale);
                // Each arm swings against its own leg: back as that foot reaches forward.
                float carryOut=i==0?P.armCarry.x:P.armCarry.y,swingSide=i==0?P.armSwingSide.x:P.armSwingSide.y;
                if(dt>0)
                {
                    carriedOut[i]=Mathf.MoveTowards(carriedOut[i],carriesOut[i],dt*.3f);
                    carriedKeep[i]=Mathf.MoveTowards(carriedKeep[i],carriesKeep[i],dt*2.5f);
                }
                carryOut+=carriedOut[i];swingSide*=carriedKeep[i];
                float swing=-armAmplitude*swingSide*Mathf.Cos(cycle+Mathf.PI*i);
                Vector3 shoulder=i==0?leftShoulder:rightShoulder;
                // A relaxed arm hangs nearly straight and swings as a pendulum from the shoulder;
                // a jogging arm bends and pumps.
                float drop=P.armHang.y,angle=swing/drop;
                Vector3 relaxed=shoulder+chest*new Vector3(side*(P.armHang.x+carryOut),-drop*Mathf.Cos(angle),P.armHang.z+drop*Mathf.Sin(angle));
                Vector3 pumping=shoulder+chest*(new Vector3(side*.02f,-.34f,.24f)*P.scale+new Vector3(0,0,swing*.8f));
                Vector3 wrist=Vector3.Lerp(relaxed,pumping,jogWeight);
                bool busy=false;
                if(worker!=null)
                {
                    // A hand that carries something of its own (a lantern, a mug) keeps to it: it takes no load.
                    bool free=Holds==null||holds.HandFree(i);
                    busy=(worker.Carried>0&&free)||(equipment!=null&&equipment.HandsOnTool&&equipment.Uses(i))||(i==1&&Working&&worker.MiningTarget==null);
                    if(worker.Carried>0&&free) wrist=hips+posture*(new Vector3(side*.22f,-.12f,.37f)*P.scale);
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
                    // A hand that holds the tool goes to where its wrist must be for that (the grip itself, for a
                    // body without hands of its own). A hand that is not free keeps to what it carries.
                    if(equipment!=null && equipment.HandsOnTool && equipment.Uses(i))
                    {
                        wrist=equipment.WristPosition(i,shoulder);
                        handLocal[i]=Quaternion.Inverse(posture)*(wrist-hips);
                    }
                }
                freeWrist[i]=wrist;
                // A hand on a real object goes where the object is.
                IArmGuide leads=guide!=null&&guide.Guides(i)?guide:also!=null&&also.Guides(i)?also:null;
                if(leads!=null)
                {
                    wrist=leads.Wrist(i,shoulder);
                    handLocal[i]=Quaternion.Inverse(posture)*(wrist-hips);
                    busy=true;
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
            elbowNow[index]=elbow;
            Segment(upperArms[index],shoulder,elbow,.13f*P.scale);Segment(forearms[index],elbow,wrist,.11f*P.scale);
            handPositions[index]=transform.InverseTransformPoint(wrist);
        }
    }
}
