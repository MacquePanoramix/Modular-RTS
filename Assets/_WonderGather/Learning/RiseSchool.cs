using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.AI;

namespace WonderGather
{
    // The third path (Luis, October 22, 2026: "I approve we try the third path"): where what learns the moment from hands
    // and feet to the squat (RiseAgent) is taught, as the bench tries the getting up. Small is laid down on its front by
    // chance along the path, gets up by its ways, and in the pike and the roll-up the agent says its corrections every few
    // steps of the physics. A try that stands is worth 1, one handed to its standing balance that then falls 0.3, any
    // other 0; the agent pays a little for every change in what it says. Its own body does all of it; nothing helps it.
    // Run by the bench's test (RiseTraining) in the editor, or in a build started with -riseTrain (many at once, by the
    // trainer: Docs/Technical/RiseWays/README.md on the work branch). Numbers from the command line:
    //   -trainWays <ways file> -trainHours 6 -trainTries 100000 -trainEvery 5 (steps of the physics between decisions)
    //   -trainFrames 1 (a game frame every so many steps) -trainFor 25 (seconds a try at most) -trainTurn 20 -trainMove 0.3
    //   (each try laid turned and moved by chance, at most so far) -trainSeed 1
    public sealed class RiseSchool : MonoBehaviour
    {
        private static readonly CultureInfo Plain = CultureInfo.InvariantCulture;
        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        public static string Argument(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++) if (args[i] == name) return args[i + 1];
            return null;
        }
        private static bool Flag(string name) => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), name) >= 0;
        private static float Number(string argument, float otherwise) { string given = Argument(argument); return string.IsNullOrEmpty(given) ? otherwise : float.Parse(given, Plain); }

        // A build started with -riseTrain teaches as soon as its place is loaded, and quits when the teaching is over.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Begin()
        {
            if (!Flag("-riseTrain") || Application.isEditor) return;
            var school = new GameObject("Rise school").AddComponent<RiseSchool>();
            school.StartCoroutine(school.Run());
        }
        private IEnumerator Run()
        {
            yield return null;
            yield return Teach(msg => Debug.Log(msg));
            Application.Quit();
        }

        // How the let-go body lies: its parts, its boots and its toes, and where its own place was.
        public const int Parts = OwnBody.Count, Kept = Parts + 2;
        public struct Lies { public Vector3[] at; public Quaternion[] turned; public Vector3 place; public Quaternion faces; }

        public static Lies Now(PhysicalFall fall)
        {
            var lies = new Lies { at = new Vector3[Kept], turned = new Quaternion[Kept] };
            for (int i = 0; i < PhysicalFall.Count; i++) { lies.at[i] = fall.Part(i).transform.position; lies.turned[i] = fall.Part(i).transform.rotation; }
            for (int i = 0; i < 2; i++) fall.BootNow(i, out lies.at[OwnBody.Foot + i], out lies.turned[OwnBody.Foot + i], out lies.at[Parts + i], out lies.turned[Parts + i]);
            return lies;
        }

        public static void Put(PhysicalFall fall, Lies lies)
        {
            for (int i = 0; i < PhysicalFall.Count; i++)
            {
                var part = fall.Part(i);
                part.transform.SetPositionAndRotation(lies.at[i], lies.turned[i]);
                part.linearVelocity = Vector3.zero; part.angularVelocity = Vector3.zero;
            }
        }

        // The posed body's segments put as the body lay, for the body's own to be made from.
        public static void Lay(MinerBody miner, Lies lies)
        {
            var s = miner.Solved;
            void To(Transform segment, int k) => segment.SetPositionAndRotation(lies.at[k], lies.turned[k]);
            To(s.pelvis, OwnBody.Hips); To(s.torso, OwnBody.Trunk); To(s.head, OwnBody.Head);
            for (int i = 0; i < 2; i++)
            {
                To(s.upperArms[i], OwnBody.UpperArm + i); To(s.forearms[i], OwnBody.Forearm + i);
                To(s.thighs[i], OwnBody.Thigh + i); To(s.shins[i], OwnBody.Shin + i); To(s.feet[i], OwnBody.Foot + i);
                if (s.toes != null && s.toes.Length == 2 && s.toes[i] != null) To(s.toes[i], Parts + i);
            }
        }

        // A let-go body laid on its front, every part turned as one about where it stood, just clear of the ground.
        public static void LayOnItsFront(PhysicalFall fall, Transform unit, OrdinaryGround ground)
        {
            Vector3 pivot = unit.position;
            Quaternion turn = Quaternion.AngleAxis(90, unit.right);
            for (int i = 0; i < PhysicalFall.Count; i++)
            {
                var part = fall.Part(i);
                part.transform.SetPositionAndRotation(pivot + turn * (part.transform.position - pivot), turn * part.transform.rotation);
                part.linearVelocity = Vector3.zero; part.angularVelocity = Vector3.zero;
            }
            Physics.SyncTransforms();
            float lowest = float.MaxValue;
            for (int i = 0; i < PhysicalFall.Count; i++)
                foreach (var solid in fall.Part(i).GetComponentsInChildren<Collider>())
                    lowest = Mathf.Min(lowest, solid.bounds.min.y - ground.Height(solid.bounds.center.x, solid.bounds.center.z));
            for (int i = 0; i < PhysicalFall.Count; i++) fall.Part(i).transform.position += Vector3.up * (.03f - lowest);
            Physics.SyncTransforms();
        }

        // The teaching itself, in the ordinary place as it is loaded.
        public static IEnumerator Teach(System.Action<string> tell)
        {
            string file = Argument("-trainWays");
            if (string.IsNullOrEmpty(file)) { tell("TRAIN no -trainWays <file>: nothing to teach with"); yield break; }
            int tries = (int)Number("-trainTries", 100000), every = Mathf.Max(1, (int)Number("-trainEvery", 5)), frames = (int)Number("-trainFrames", 1);
            float hours = Number("-trainHours", 6), lasts = Number("-trainFor", 25), turnBy = Number("-trainTurn", 20), moveBy = Number("-trainMove", .3f);
            var chance = new System.Random((int)Number("-trainSeed", 1));

            var choice = Object.FindAnyObjectByType<MinerChoice>();
            var look = Object.FindAnyObjectByType<MinerWorkPreview>();
            var ground = Object.FindAnyObjectByType<OrdinaryGround>();
            var grass = Object.FindAnyObjectByType<GrassField>();
            if (look != null) look.SetOwn(false);
            if (grass != null) grass.enabled = false;
            Vector3 Spot(int n, out Vector3 away)
            {
                var a = ground.Path[(3 + n) % (ground.Path.Count - 1)];
                var b = ground.Path[(3 + n) % (ground.Path.Count - 1) + 1];
                away = (new Vector3(b.x, 0, b.y) - new Vector3(a.x, 0, a.y)).normalized;
                return new Vector3(a.x, ground.Height(a.x, a.y), a.y);
            }

            // Small, at its place, standing by its own joints once so that how it stands is known (as the bench).
            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Small") index = i;
            if (index < 0) { tell("TRAIN no miner called Small"); yield break; }
            choice.Choose(index);
            yield return null;
            var unit = choice.Current;
            var miner = unit.GetComponent<MinerBody>();
            Vector3 spot = Spot(0, out Vector3 away);
            unit.Motor.Stop();
            unit.GetComponent<NavMeshAgent>().Warp(spot);
            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
            unit.GetComponent<ProceduralBiped>().SetStance(0, 0);
            unit.GetComponent<ProceduralBiped>().ResetPose();
            for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
            look.SetOwn(true);
            OwnBody own = null;
            for (float began = Time.time; Time.time - began < 5 && (own == null || !own.Stands);) { unit.TryGetComponent(out own); yield return null; }
            if (own == null || !own.Stands) { tell("TRAIN Small did not come to stand by its own joints"); yield break; }
            for (float until = Time.time + 1; Time.time < until;) yield return null;
            look.SetOwn(false);
            for (float until = Time.time + .8f; Time.time < until;) yield return null;
            if (!own.CanRise) { tell("TRAIN Small: how it stands is not known"); yield break; }
            unit.Motor.Stop();
            unit.GetComponent<NavMeshAgent>().Warp(spot);
            unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
            unit.GetComponent<ProceduralBiped>().ResetPose();
            for (float until = Time.time + 1; Time.time < until;) yield return null;
            var fall = unit.GetComponent<PhysicalFall>();
            if (fall == null) fall = unit.gameObject.AddComponent<PhysicalFall>();
            fall.GetsUp = false; fall.Gives = false;
            fall.LetGo();
            if (fall.Now != PhysicalFall.State.Falling) { tell("TRAIN Small was not let go"); yield break; }

            var mode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            float dt = Time.fixedDeltaTime;
            float timeWas = Time.captureDeltaTime;
            Time.captureDeltaTime = dt * Mathf.Max(1, frames);
            bool fellBack = OwnBody.FallsBack;
            var waysWere = OwnBody.RiseWays;
            try
            {
                // Its starts: laid on its front at eighteen places along the path, and the same turned a quarter. (-trainFalls
                // 1: instead, pushed over at eighteen places, four ways round and some way between by chance, and lying where
                // it fell, on its front, its back or a side, as in the game: taught from its front only, in the game it got
                // up at two pushes of six, October 23.)
                bool falls = Number("-trainFalls", 0) > 0;
                float weighs = unit.GetComponent<PhysicalBody>().Mass;
                var starts = new List<Lies>();
                var standing = Now(fall);
                Quaternion ownTurn = Quaternion.LookRotation(away);
                for (int s = 0; s < (falls ? 72 : 36); s++)
                {
                    int on = s % 18;
                    float turned = falls ? 0 : s < 18 ? 0 : 90;
                    Put(fall, standing);
                    Vector3 there = Spot(on, out Vector3 thereAway);
                    thereAway = Quaternion.AngleAxis(turned, Vector3.up) * thereAway;
                    Quaternion turn = Quaternion.LookRotation(thereAway) * Quaternion.Inverse(ownTurn);
                    for (int i = 0; i < PhysicalFall.Count; i++)
                    {
                        var part = fall.Part(i);
                        part.transform.SetPositionAndRotation(there + turn * (part.transform.position - spot), turn * part.transform.rotation);
                    }
                    unit.transform.SetPositionAndRotation(there, Quaternion.LookRotation(thereAway));
                    Physics.SyncTransforms();
                    fall.Fell();
                    float pushedTo = (s / 18) * 90 + (float)(chance.NextDouble() * 2 - 1) * 40;
                    if (!falls) LayOnItsFront(fall, unit.transform, ground);
                    for (int step = 0; step < 450; step++)
                    {
                        if (falls && step * dt < .3f) fall.Push(Quaternion.AngleAxis(pushedTo, Vector3.up) * unit.transform.forward * (6 * weighs), fall.Part(PhysicalFall.Trunk).worldCenterOfMass);
                        fall.Advance(dt); Physics.Simulate(dt);
                    }
                    var lies = Now(fall);
                    lies.place = unit.transform.position; lies.faces = unit.transform.rotation;
                    starts.Add(lies);
                }
                fall.Gives = true; fall.GetsUp = true; fall.TakeBack();
                yield return null;
                yield return null;
                OwnBody.FallsBack = false;
                look.SetOwn(true);
                var ways = OwnRiser.Read(System.IO.File.ReadAllText(file));

                Academy.Instance.AutomaticSteppingEnabled = false;
                // (Without a teacher, to learn what its corrections do: -riseAsks and -riseWanders, RiseAgent.Heuristic.)
                string asks = Argument("-riseAsks");
                if (!string.IsNullOrEmpty(asks)) { var each = asks.Split(','); for (int i = 0; i < each.Length && i < RiseAgent.Says; i++) RiseAgent.Asks[i] = float.Parse(each[i], Plain); }
                RiseAgent.Wanders = Number("-riseWanders", 0);
                // (-trainFull 1: it sets every joint itself, option 1, October 26.)
                bool full = Number("-trainFull", 0) > 0;
                // (-trainModel <name>: no teacher; a model built in, Resources/RiseModels/<name>, answers, its own best answer
                // as in the game and on the bench, or one drawn about it as it was taught with -trainDrawn 1. To measure what
                // was learnt on the teaching's own falls, October 27.)
                string modelName = Argument("-trainModel");
                var model = string.IsNullOrEmpty(modelName) ? null : Resources.Load<Unity.InferenceEngine.ModelAsset>("RiseModels/" + modelName);
                if (!string.IsNullOrEmpty(modelName) && model == null) { tell("TRAIN no model RiseModels/" + modelName); yield break; }
                var agent = RiseAgent.On(unit.gameObject, own, model, full);
                if (model != null)
                {
                    var behaviour = unit.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
                    behaviour.BehaviorType = Unity.MLAgents.Policies.BehaviorType.InferenceOnly;
                    behaviour.DeterministicInference = Number("-trainDrawn", 0) <= 0;
                    tell("TRAIN answered by RiseModels/" + modelName + (behaviour.DeterministicInference ? ", its best answer" : ", drawn"));
                }
                // The help it is given only while it is taught (Luis, October 22), by the trainer's lessons ("help", from 1
                // down to nothing; -trainHelp without a trainer), in the stages it learns at until it is handed to its
                // standing balance: its joints StrongerBy more as strong; its weight drawn towards a little ahead of the
                // middle of its boots, by up to PullsBy of its weight (on its hips and trunk, as they weigh), as its weight
                // is so far ahead or behind; and its hips lifted by LiftsBy of its weight.
                // (Measured without learning, 100 falls each, October 26: nothing 4 stood; stronger by 0.6, 3; lifted by
                // 0.3, none (rolling up asks its boots to bear 0.55 of it); by 0.1, 8. Most gave up rolling up, the weight
                // not over the boots: so it is drawn there. Drawn by 0.15, 18 of 200; 0.3, 12; 0.6, 5; nothing, 9.)
                // (-trainStrongerBy, -trainLiftsBy: other measures of it. -trainTell 1: where each try that did not stand
                // ended.)
                float StrongerBy = Number("-trainStrongerBy", .3f), PullsBy = Number("-trainPullsBy", .15f), LiftsBy = Number("-trainLiftsBy", 0);
                // (-trainNearer: how much it is told as its weight comes nearer over its soles; RiseAgent.Nearer.)
                RiseAgent.Nearer = Number("-trainNearer", RiseAgent.Nearer);
                System.Action<string> told = said =>
                {
                    if (!said.Contains("gave up")) return;
                    var r = own.Riser;
                    tell(r == null ? "TRAIN_TOLD " + said : string.Format(Plain,
                        "TRAIN_TOLD {0}; lasted {1:0.0} s, waited to let go {2:0.0} s, hands let {3}; weight ahead of its soles {4:0.00}, soles {5:0.00}, boots bear {6:0.00}, depth {7:0.000}, hips {8:0.00}, upright {9:0.00}",
                        said, r.Lasted, r.ReleaseWaited, r.HandsLet, r.Measure(OwnRiser.Asks.Ahead), r.Measure(OwnRiser.Asks.Soles), r.Measure(OwnRiser.Asks.Bears), r.BalanceDepth,
                        r.Measure(OwnRiser.Asks.Hips), r.Measure(OwnRiser.Asks.Upright)));
                };
                bool telling = Number("-trainTell", 0) > 0;
                if (telling) OwnBody.Told += told;
                // (Its weight as its physical body has it: its own body's parts are made only as it begins.)
                float whole = weighs;
                float helped = 0;
                var clock = System.Diagnostics.Stopwatch.StartNew();
                int stood = 0, handed = 0, acted = 0, lastStood = 0, lastActed = 0;
                bool everOn = false;
                int beganBetween = 0;
                // (-trainRetries 0.5: after a try that went down, so often the next begins where that one left it, its fall
                // having it with how its parts were going and lying still first, as in the game; at most -trainRetriesAtMost
                // in a row. In the game most tries are such: pushed over fifteen times, Small alone tried 159 times and
                // stood twice, the first try of each push none, October 27.)
                float retries = Number("-trainRetries", 0);
                int retriesAtMost = (int)Number("-trainRetriesAtMost", 4), inRow = 0, retried = 0, retriedStood = 0;
                Lies? again = null;
                OwnBody.FallsBack = retries > 0;
                for (int k = 0; k < tries && clock.Elapsed.TotalHours < hours; k++)
                {
                    // (Standing as a try begins: made its own between tries by something else. Told.)
                    if (own.Stands) { own.Drop(); beganBetween++; tell("TRAIN_BEGAN between tries, before try " + (k + 1) + " (" + beganBetween + " times)"); }
                    // (The game's time as the physics': the trainer sets its own time scale and frame rate when it comes, and
                    // at twenty times, what runs by the frame (its breath, on its trunk) ran twenty times as fast as the
                    // physics; 3 to 6% of tries stood, 25% without it. October 23.)
                    Time.timeScale = 1;
                    Time.captureDeltaTime = dt * Mathf.Max(1, frames);
                    // A start, turned and moved by chance (a way that serves only the very place it was taught at is not one);
                    // or where the last try left it.
                    bool retry = again.HasValue;
                    var laid = retry ? again.Value : starts[chance.Next(starts.Count)];
                    again = null;
                    if (retry) { retried++; inRow++; } else inRow = 0;
                    float yaw = retry ? 0 : (float)(chance.NextDouble() * 2 - 1) * turnBy;
                    Vector3 move = retry ? Vector3.zero : new Vector3((float)(chance.NextDouble() * 2 - 1), 0, (float)(chance.NextDouble() * 2 - 1)) * moveBy;
                    Vector3 hipsWere = laid.at[OwnBody.Hips];
                    float Under(Vector3 at) => Physics.Raycast(at + Vector3.up * 3, Vector3.down, out var hit, 8, 1 << 6) ? hit.point.y : at.y;
                    move.y = Under(hipsWere + move) - Under(hipsWere);
                    Quaternion yawTurn = Quaternion.AngleAxis(yaw, Vector3.up);
                    var at = (Vector3[])laid.at.Clone(); var turnedNow = (Quaternion[])laid.turned.Clone();
                    for (int i = 0; i < at.Length; i++) { at[i] = hipsWere + move + yawTurn * (laid.at[i] - hipsWere); turnedNow[i] = yawTurn * laid.turned[i]; }
                    laid.at = at; laid.turned = turnedNow;
                    laid.place = hipsWere + move + yawTurn * (laid.place - hipsWere); laid.faces = yawTurn * laid.faces;
                    unit.transform.SetPositionAndRotation(laid.place, laid.faces);
                    Lay(miner, laid);
                    own.Wanted = true;
                    OwnBody.RiseWays = ways;
                    float help = Mathf.Clamp01(Academy.Instance.EnvironmentParameters.GetWithDefault("help", Number("-trainHelp", 0)));
                    OwnRiser.StrongerWhileLearning = 1 + StrongerBy * help;
                    helped += help;
                    if (!own.BeginDown()) continue;
                    Physics.SyncTransforms();
                    agent.Afresh();
                    bool hasActed = false, onFeet = false, up = false;
                    float stoodFor = 0;
                    // (-trainTell 2: each stage it came to, with how each boot's sole faced (1: down on the ground; -1: up)
                    // and its hips' height as it came to it.)
                    var stages = Number("-trainTell", 0) >= 2 ? new System.Text.StringBuilder() : null;
                    string stageWas = "";
                    for (int step = 0; step * dt < lasts; step++)
                    {
                        var riser = own.Riser;
                        bool acting = own.GettingUp && riser != null && OwnRiser.LearnsAt(riser.StageNow);
                        if (acting && step % every == 0)
                        {
                            agent.RequestDecision();
                            Academy.Instance.EnvironmentStep();
                            hasActed = true;
                            everOn |= Academy.Instance.IsCommunicatorOn;
                        }
                        if (stages != null && own.GettingUp && own.Riser != null && own.Riser.StageNow != stageWas)
                        {
                            stages.AppendFormat(Plain, " | {0}@{1:0.0}s soles {2:0.00},{3:0.00} hips {4:0.00}", own.Riser.StageNow, step * dt,
                                own.Part(OwnBody.Foot).transform.up.y, own.Part(OwnBody.Foot + 1).transform.up.y, own.Riser.Measure(OwnRiser.Asks.Hips));
                            stageWas = own.Riser.StageNow;
                        }
                        own.Advance(dt);
                        if (own.Stands && float.IsNaN(own.Part(OwnBody.Hips).transform.position.x)) break;
                        if (!own.Stands) break;
                        if (help > 0 && own.GettingUp && own.Riser != null && OwnRiser.LearnsAt(own.Riser.StageNow))
                        {
                            float g = Physics.gravity.magnitude;
                            if (LiftsBy > 0) own.Part(OwnBody.Hips).AddForce(Vector3.up * (LiftsBy * help * whole * g));
                            Vector3 toes = Flat(own.Part(OwnBody.Foot).transform.forward + own.Part(OwnBody.Foot + 1).transform.forward);
                            float ahead = own.Riser.Measure(OwnRiser.Asks.Ahead);
                            if (PullsBy > 0 && toes.sqrMagnitude > 1e-6f && !float.IsNaN(ahead))
                            {
                                Vector3 pull = -toes.normalized * (PullsBy * help * whole * g * Mathf.Clamp((ahead - .08f) / .15f, -1, 1));
                                float hips = own.Part(OwnBody.Hips).mass, trunk = own.Part(OwnBody.Trunk).mass;
                                own.Part(OwnBody.Hips).AddForce(pull * (hips / (hips + trunk)));
                                own.Part(OwnBody.Trunk).AddForce(pull * (trunk / (hips + trunk)));
                            }
                        }
                        if (!own.GettingUp && !onFeet) onFeet = true;
                        if (onFeet && !own.Rising) { stoodFor += dt; if (stoodFor >= 1) { up = true; break; } }
                        Physics.Simulate(dt);
                        if (frames > 0 && step % frames == 0) yield return null;
                    }
                    if (stages != null) tell("TRAIN_STAGES " + (up ? "stood" : onFeet ? "fell" : "down") + stages);
                    if (own.Stands) own.Drop();
                    // (Gone down, its fall has it: lying still, the next try may begin there; if not, the posed body takes it
                    // back, and the fall's parts are gone by the frame's end.)
                    if (fall.Now == PhysicalFall.State.Falling || fall.Now == PhysicalFall.State.Lying)
                    {
                        if (!up && retries > 0 && inRow < retriesAtMost && chance.NextDouble() < retries)
                        {
                            fall.GetsUp = false;
                            for (int s = 0; s < 450 && fall.Now != PhysicalFall.State.Lying; s++) { fall.Advance(dt); Physics.Simulate(dt); }
                            for (int s = 0; s < 25; s++) { fall.Advance(dt); Physics.Simulate(dt); }
                            var lies = Now(fall);
                            lies.place = unit.transform.position; lies.faces = unit.transform.rotation;
                            if (!float.IsNaN(lies.at[OwnBody.Hips].x)) again = lies;
                            fall.GetsUp = true;
                        }
                        fall.TakeBack();
                    }
                    // (Not wanted between tries: wanted, a body whose posed body seems at ease is made its own as that posed
                    // body stands, and how it stands is noted afresh from however the try left it; five of twelve copies of
                    // the teaching went from a quarter standing to none so, October 26. The panel sets it wanted again every
                    // frame, so this alone did not keep it so; what keeps it is that its count of being at ease begins afresh
                    // each time it is made its own, OwnBody.Make, October 27.)
                    // (-trainKeepWanted 1: wanted all along, as before.)
                    if (Number("-trainKeepWanted", 0) <= 0) own.Wanted = false;
                    float reward = up ? 1 : onFeet ? .3f : 0;
                    if (up) stood++;
                    if (up && retry) retriedStood++;
                    if (onFeet) handed++;
                    if (hasActed)
                    {
                        acted++; lastActed++;
                        if (up) lastStood++;
                        agent.AddReward(reward);
                        agent.EndEpisode();
                        Academy.Instance.EnvironmentStep();
                    }
                    if ((k + 1) % 100 == 0)
                    {
                        tell(string.Format(Plain, "TRAIN {0} tries in {1:0.0} min: stood {2}, handed over {3}, the agent acted in {4}; of the last {5} it acted in, {6} stood; helped {7:0.00}",
                            k + 1, clock.Elapsed.TotalMinutes, stood, handed, acted, lastActed, lastStood, helped / 100)
                            + (retries > 0 ? string.Format(Plain, "; begun where the last left it {0}, of them stood {1}", retried, retriedStood) : ""));
                        lastStood = lastActed = 0; helped = 0;
                    }
                    if (everOn && !Academy.Instance.IsCommunicatorOn) { tell("TRAIN the trainer has gone: done"); break; }
                    yield return null;
                }
                if (telling) OwnBody.Told -= told;
                tell(string.Format(Plain, "TRAIN done after {0:0.0} min: stood {1}, handed over {2}, acted {3}", clock.Elapsed.TotalMinutes, stood, handed, acted));
            }
            finally
            {
                Physics.simulationMode = mode;
                Time.captureDeltaTime = timeWas;
                OwnRiser.StrongerWhileLearning = 1;
                OwnBody.FallsBack = fellBack;
                OwnBody.RiseWays = waysWere;
                if (own != null && own.Stands) own.Drop();
            }
        }
    }
}
