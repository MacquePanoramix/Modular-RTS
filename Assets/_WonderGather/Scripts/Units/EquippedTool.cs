using UnityEngine;

namespace WonderGather
{
    // Gameplay owns this cycle. The body calls SolveFrame once, after solving support/torso,
    // then consumes its grip positions. Contact uses exactly that reachable tool trajectory.
    public sealed class EquippedTool : MonoBehaviour
    {
        public enum StrikePhase { Rest, Preparation, Striking, Recovery }
        private ToolDefinition definition;
        private Transform model;
        private Gatherer worker;
        private ProceduralBiped body;
        private MineableResource target;
        private readonly RaycastHit[] hits = new RaycastHit[32];
        private readonly Collider[] overlaps = new Collider[32];
        private float progress, stoppedAngle, settle, stow;
        private bool spent, stopped;
        private Vector3 lastRoot;
        private ulong attempt, acceptedAttempt;
        private IHandHolds holds;
        private bool holdsLooked;
        public ToolDefinition Definition => definition;
        public StrikePhase Phase { get; private set; }
        public bool Busy => target != null;
        public bool HandsOnTool { get; private set; }
        public bool Stowed => stow > .99f;
        public bool SupportsBodyScale => (transform.lossyScale-Vector3.one).sqrMagnitude<.000001f
            && (model==null || (model.lossyScale-Vector3.one).sqrMagnitude<.000001f);
        public float Progress => progress;
        public ulong AttemptId => attempt;
        public int AcceptedStrikes { get; private set; }
        public string Status { get; private set; } = "No tool";
        public Vector3 HeadPosition => model != null ? model.TransformPoint(definition.Head) : transform.position;
        public Vector3 GripPosition(int index) => model != null ? model.TransformPoint(index == 0 ? definition.SecondaryGrip : definition.PrimaryGrip) : transform.position;

        private void Awake() { worker=GetComponent<Gatherer>(); body=GetComponent<ProceduralBiped>(); }
        public void SetDefinition(ToolDefinition value)
        {
            if (value != null && !value.IsValid) throw new System.ArgumentException("Invalid tool definition.");
            if (definition == value && (value == null || model != null)) return;
            CancelAttempt();
            if(worker!=null && worker.MiningTarget!=null) worker.InterruptMining("Equipment changed");
            if (model != null) { model.gameObject.SetActive(false); Destroy(model.gameObject); }
            definition=value; model=null; stow=0;
            if (value != null)
            {
                model=Instantiate(value.Prefab,transform).transform;
                model.name=value.DisplayName+" (equipped)";
                model.gameObject.SetActive(isActiveAndEnabled);
            }
            Status=value == null ? "No tool" : "Pickaxe ready";
        }
        public void CancelAttempt()
        {
            target=null; progress=0; settle=0; spent=false; stopped=false;
            Phase=StrikePhase.Rest; HandsOnTool=false;
        }
        private void OnEnable() { if (model != null) model.gameObject.SetActive(true); }
        private void OnDisable()
        {
            CancelAttempt();
            if (worker != null && worker.MiningTarget != null) worker.InterruptMining("Tool unavailable");
            if (model != null) model.gameObject.SetActive(false);
            TellHands(Vector3.zero,Vector3.zero);
        }
        // A body with hands of its own turns each hand onto the handle and closes its fingers, or lets go.
        private void TellHands(Vector3 left, Vector3 right)
        {
            if(Holds==null) return;
            for(int hand=0;hand<2;hand++)
            {
                bool on=HandsOnTool&&model!=null&&definition!=null&&Uses(hand);
                holds.HoldHandle(hand,on,on?GripPosition(hand):Vector3.zero,on?HandleDirection:Vector3.up,on?definition.GripRadius(hand):0,hand==0?left:right);
            }
        }
        private void Update()
        {
            if (Busy && (body == null || !body.isActiveAndEnabled || !body.Ready))
            { CancelAttempt(); if (worker != null) worker.InterruptMining("Body cannot support the tool"); }
        }
        private bool WorkValid(MineableResource mine) => worker != null && worker.isActiveAndEnabled
            && mine != null && mine.Accepts(definition) && worker.MiningTarget == mine && worker.HasWorkContact
            && worker.State == Gatherer.Activity.Gathering && worker.FacingWork;

        // A body with hands of its own says which are free and where a wrist must be to hold a handle. Without one
        // (the first body) both hands hold, each wrist at its grip.
        private IHandHolds Holds { get { if(!holdsLooked) { holds=GetComponent<IHandHolds>(); holdsLooked=true; } return holds; } }
        public bool Uses(int hand) => Holds==null || holds.HandFree(hand);
        // The tool's model as it is now (its own space is the tool's: y up the handle, z the way it strikes).
        public Transform Model => model;
        // The way the handle runs, towards the head.
        public Vector3 HandleDirection => model != null ? model.up : transform.up;
        private Vector3 Wrist(int hand, Vector3 position, Quaternion rotation, Vector3 shoulder)
        {
            Vector3 grip=position+rotation*(hand==0?definition.SecondaryGrip:definition.PrimaryGrip);
            return Holds==null ? grip : holds.WristFor(hand,grip,rotation*Vector3.up,definition.GripRadius(hand),shoulder);
        }
        // Where this hand's wrist must be to hold the tool as it is now.
        public Vector3 WristPosition(int hand, Vector3 shoulder) => model != null ? Wrist(hand,model.position,model.rotation,shoulder) : transform.position;
        // Every hand that holds must reach BEFORE contact, never clamped independently afterward. The lower grip
        // is the right hand's: without that hand the tool cannot be swung. The reach is the body's own.
        private bool ArmCanReach(float distance) => body != null ? distance >= body.ToolReachMin && distance <= body.ToolReachMax
            : distance >= .025f && distance <= .865f;
        private bool Reachable(Vector3 position, Quaternion rotation, Vector3 left, Vector3 right)
            => Uses(1) && (!Uses(0) || ArmCanReach(Vector3.Distance(Wrist(0,position,rotation,left),left)))
            && ArmCanReach(Vector3.Distance(Wrist(1,position,rotation,right),right));
        // How far the tool leans out over the right shoulder at the top of its backswing, in degrees.
        private const float OverShoulder = 14;
        private void PoseAt(float angle, Vector3 hips, Quaternion posture, out Vector3 position, out Quaternion rotation)
        {
            if(Holds!=null && body!=null)
            {
                // A body with a shape of its own cannot swing a tool through itself. The first body's swing turns the
                // tool about a hand held still in front of the hips, and at the top of the backswing the tool's head
                // is inside the chest. Here the same swing (the same lean of the tool at every moment) goes over the
                // right shoulder: as the tool leans back the hands go out to the right and up in front of that
                // shoulder, and the tool leans a little outwards, so its head passes above the shoulder and beside
                // the body's own head. As it comes forward the hands come back down the same way, and reach a
                // little further as it strikes. Everything is a share of the body's own arm.
                float reach=body.ArmReach;
                Vector3 ready=body.ToolHand,shoulder=body.ShoulderFromHips;
                var shape=body.BodyProportions;
                // Out past the head (hair and cap with it), and no nearer the chest than the ready hands are.
                Vector3 raised=new Vector3(Mathf.Max(shoulder.x+.04f*reach,shape.headHalf+.1f*reach),shoulder.y-.17f*reach,Mathf.Max(shoulder.z+.58f*reach,ready.z));
                Vector3 round=new Vector3(raised.x*1.1f,ready.y,ready.z+.1f*reach);
                float up=Mathf.Clamp01((20-angle)/72f),forth=Mathf.Clamp01((angle-20)/68f);
                // As it strikes the hands reach out, so the handle's foot swings clear of the body's front.
                Vector3 place=(1-up)*(1-up)*ready+2*up*(1-up)*round+up*up*raised+forth*new Vector3(0,-.05f*reach,.3f*reach);
                rotation=posture*Quaternion.Euler(0,-OverShoulder*up,0)*Quaternion.Euler(angle,0,0);
                position=hips+posture*place-rotation*definition.PrimaryGrip;
                return;
            }
            rotation=posture*Quaternion.Euler(angle,0,0);
            Vector3 hand=hips+posture*(body != null ? body.ToolHand : new Vector3(0,-.03f,.24f));
            position=hand-rotation*definition.PrimaryGrip;
        }
        private static float SwingAngle(float p)
        {
            if (p < .4f) return Mathf.Lerp(20,-52,Mathf.SmoothStep(0,1,p/.4f));
            return Mathf.Lerp(-52,88,Mathf.SmoothStep(0,1,(p-.4f)/.3f));
        }
        private bool Own(Collider c) => c != null && c.transform.IsChildOf(transform);
        private bool StartClear(Vector3 point)
        {
            int count=Physics.OverlapSphereNonAlloc(point,definition.HeadRadius,overlaps,~0,QueryTriggerInteraction.Ignore);
            if (count==overlaps.Length) return false;
            for(int i=0;i<count;i++) if(!Own(overlaps[i])) return false;
            return true;
        }
        // Returns the first physical obstruction; buffers saturating fail closed.
        private bool Sweep(Vector3 from, Vector3 to, out Collider collider, out float fraction)
        {
            collider=null; fraction=0;
            Vector3 delta=to-from; float distance=delta.magnitude;
            if(distance<.000001f) return false;
            int count=Physics.SphereCastNonAlloc(from,definition.HeadRadius,delta/distance,hits,distance,~0,QueryTriggerInteraction.Ignore);
            if(count==hits.Length) return true;
            float nearest=float.PositiveInfinity;
            for(int i=0;i<count;i++) if(!Own(hits[i].collider) && hits[i].distance<nearest)
            { nearest=hits[i].distance; collider=hits[i].collider; }
            if(collider==null) return false;
            fraction=Mathf.Clamp01(nearest/distance); return true;
        }

        // The body supplies its actual shoulders, including torso twist, so reach is judged
        // against the same joints the arm solve will use.
        public void SolveFrame(float dt, Vector3 hips, Quaternion posture, Vector3 left, Vector3 right, bool supported)
        {
            Solve(dt,hips,posture,left,right,supported);
            TellHands(left,right);
        }
        private void Solve(float dt, Vector3 hips, Quaternion posture, Vector3 left, Vector3 right, bool supported)
        {
            HandsOnTool=false;
            if(!isActiveAndEnabled || definition==null || model==null) return;
            if(!SupportsBodyScale)
            { CancelAttempt(); Status="Tool/body scale unsupported"; if(worker!=null && worker.MiningTarget!=null) worker.InterruptMining(Status); return; }
            var requested=worker != null ? worker.MiningTarget : null;
            bool ready=WorkValid(requested) && supported;
            if(Busy && (!ready || target!=requested || Vector3.Distance(lastRoot,transform.position)>.08f)) CancelAttempt();
            lastRoot=transform.position;
            bool mining=ready && (worker.Carried<worker.Capacity || Busy);
            bool shouldStow=worker!=null && !mining && (worker.Carried>0 || worker.State==Gatherer.Activity.Gathering || worker.State==Gatherer.Activity.Depositing);
            if(TryGetComponent<Builder>(out var builder) && builder.Site!=null) shouldStow=true;
            stow=Mathf.MoveTowards(stow,shouldStow?1:0,Mathf.Max(0,dt)*4);
            float angle=20;
            if(mining && stow<=0 && dt>0)
            {
                if(!Busy)
                {
                    settle+=dt;
                    if(settle>=.12f)
                    { target=requested; attempt++; acceptedAttempt=0; progress=0; spent=false; stopped=false; }
                }
                if(Busy)
                {
                    float duration=Mathf.Max(.8f,worker.SecondsPerUnit);
                    float end=Mathf.Min(1,progress+Mathf.Min(dt,.5f)/duration);
                    while(progress<end && Busy)
                    {
                        float next=Mathf.Min(end,Mathf.Min(progress+.02f,progress<.4f?.4f:progress<.7f?.7f:1));
                        float oldAngle=progress>=.7f?Mathf.Lerp(stopped?stoppedAngle:88,20,Mathf.SmoothStep(0,1,(progress-.7f)/.3f)):stopped?stoppedAngle:SwingAngle(progress);
                        angle=next>=.7f?Mathf.Lerp(stopped?stoppedAngle:88,20,Mathf.SmoothStep(0,1,(next-.7f)/.3f)):stopped?stoppedAngle:SwingAngle(next);
                        PoseAt(oldAngle,hips,posture,out var from,out var fromRotation);
                        PoseAt(angle,hips,posture,out var to,out var rotation);
                        if(!Reachable(from,fromRotation,left,right) || !Reachable(to,rotation,left,right))
                        { Status="Tool grip out of reach"; worker.InterruptMining(Status); angle=20; break; }
                        if(progress>=.4f && progress<.7f && !spent)
                        {
                            Vector3 a=from+fromRotation*definition.Head,b=to+rotation*definition.Head;
                            // Sphere casts omit colliders already overlapping the origin.
                            // Recheck every segment: an obstruction can move into the swing.
                            if(!StartClear(a)) { spent=true; stopped=true; stoppedAngle=oldAngle; angle=oldAngle; Status="Strike starts blocked"; }
                            if(!spent && Sweep(a,b,out var collision,out var fraction))
                            {
                                spent=true; stopped=true; stoppedAngle=Mathf.Lerp(oldAngle,angle,fraction); angle=stoppedAngle;
                                if(collision==target.Surface && WorkValid(target) && acceptedAttempt!=attempt)
                                {
                                    acceptedAttempt=attempt;
                                    if(worker.AcceptStrike(this,target,attempt)) { AcceptedStrikes++; Status="Valid strike"; }
                                }
                                else Status="Strike blocked";
                            }
                        }
                        progress=next;
                    }
                    if(Busy)
                    {
                        Phase=progress<.4f?StrikePhase.Preparation:progress<.7f?StrikePhase.Striking:StrikePhase.Recovery;
                        if(progress>=.7f && !spent) Status="Miss — no material collected";
                        if(progress>=1) { target=null; Phase=StrikePhase.Rest; settle=0; }
                    }
                }
            }
            else if(!Busy) settle=0;
            PoseAt(angle,hips,posture,out var heldPosition,out var heldRotation);
            // Put away, it goes round the side to the back: as far as suits the body's size.
            float size=body != null ? body.ToolScale : 1;
            Vector3 back=hips+posture*(new Vector3(0,.15f,-.38f)*size);
            Quaternion backRotation=posture*Quaternion.Euler(0,90,-20);
            float transfer=Mathf.SmoothStep(0,1,stow);
            Vector3 aroundSide=posture*Vector3.right*(Mathf.Sin(transfer*Mathf.PI)*.45f*size);
            model.SetPositionAndRotation(Vector3.Lerp(heldPosition,back,transfer)+aroundSide,Quaternion.Slerp(heldRotation,backRotation,transfer));
            HandsOnTool=stow==0 && Reachable(model.position,model.rotation,left,right);
        }
        internal bool OwnsAcceptedAttempt(MineableResource mine, ulong id) => Busy && target==mine && id==attempt && id==acceptedAttempt;
    }
}
