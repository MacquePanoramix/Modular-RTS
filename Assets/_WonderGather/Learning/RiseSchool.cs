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
                // Its starts: laid on its front at eighteen places along the path, and the same turned a quarter.
                var starts = new List<Lies>();
                var standing = Now(fall);
                Quaternion ownTurn = Quaternion.LookRotation(away);
                for (int s = 0; s < 36; s++)
                {
                    int on = s % 18;
                    float turned = s < 18 ? 0 : 90;
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
                    LayOnItsFront(fall, unit.transform, ground);
                    for (int step = 0; step < 450; step++) { fall.Advance(dt); Physics.Simulate(dt); }
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
                var agent = RiseAgent.On(unit.gameObject, own);
                var clock = System.Diagnostics.Stopwatch.StartNew();
                int stood = 0, handed = 0, acted = 0, lastStood = 0, lastActed = 0;
                bool everOn = false;
                for (int k = 0; k < tries && clock.Elapsed.TotalHours < hours; k++)
                {
                    if (own.Stands) own.Drop();
                    // A start, turned and moved by chance (a way that serves only the very place it was taught at is not one).
                    var laid = starts[chance.Next(starts.Count)];
                    float yaw = (float)(chance.NextDouble() * 2 - 1) * turnBy;
                    Vector3 move = new Vector3((float)(chance.NextDouble() * 2 - 1), 0, (float)(chance.NextDouble() * 2 - 1)) * moveBy;
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
                    if (!own.BeginDown()) continue;
                    Physics.SyncTransforms();
                    agent.Afresh();
                    bool hasActed = false, onFeet = false, up = false;
                    float stoodFor = 0;
                    for (int step = 0; step * dt < lasts; step++)
                    {
                        var riser = own.Riser;
                        bool acting = own.GettingUp && riser != null && (riser.StageNow == "pike" || riser.StageNow == "rollup");
                        if (acting && step % every == 0)
                        {
                            agent.RequestDecision();
                            Academy.Instance.EnvironmentStep();
                            hasActed = true;
                            everOn |= Academy.Instance.IsCommunicatorOn;
                        }
                        own.Advance(dt);
                        if (own.Stands && float.IsNaN(own.Part(OwnBody.Hips).transform.position.x)) break;
                        if (!own.Stands) break;
                        if (!own.GettingUp && !onFeet) onFeet = true;
                        if (onFeet && !own.Rising) { stoodFor += dt; if (stoodFor >= 1) { up = true; break; } }
                        Physics.Simulate(dt);
                        if (frames > 0 && step % frames == 0) yield return null;
                    }
                    if (own.Stands) own.Drop();
                    float reward = up ? 1 : onFeet ? .3f : 0;
                    if (up) stood++;
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
                        tell(string.Format(Plain, "TRAIN {0} tries in {1:0.0} min: stood {2}, handed over {3}, the agent acted in {4}; of the last {5} it acted in, {6} stood",
                            k + 1, clock.Elapsed.TotalMinutes, stood, handed, acted, lastActed, lastStood));
                        lastStood = lastActed = 0;
                    }
                    if (everOn && !Academy.Instance.IsCommunicatorOn) { tell("TRAIN the trainer has gone: done"); break; }
                    yield return null;
                }
                tell(string.Format(Plain, "TRAIN done after {0:0.0} min: stood {1}, handed over {2}, acted {3}", clock.Elapsed.TotalMinutes, stood, handed, acted));
            }
            finally
            {
                Physics.simulationMode = mode;
                Time.captureDeltaTime = timeWas;
                OwnBody.FallsBack = fellBack;
                OwnBody.RiseWays = waysWere;
                if (own != null && own.Stands) own.Drop();
            }
        }
    }
}
