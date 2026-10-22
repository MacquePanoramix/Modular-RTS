using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The bench of S3b's step 2: getting up on the body that stands by its own joints (OwnRiser, then the keeper's
    // rise). The miners, each at its own place on the path, are let go and laid or thrown down a few ways, as the
    // let-go body of the fall; how each comes to lie is kept (or how it is once the fall's own ways have brought it
    // onto its knees). Then, from each of those, the body is made its own as it lies and gets up by the ways in a
    // file, the physics stepped by hand. What came of each is written to the log (RISE ...); pictures if asked.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.OwnRiseBench -riseOut <folder>
    //   -riseWays <file> [-riseMiner Small,Long,Round] [-riseStarts front,back,left,right,s0,s180] [-riseFrom lying|knees]
    //   [-riseFor 12] [-riseShots Small] [-riseEvery 10] [-riseSize 360]
    // And a search of one way's poses (as GetUpSearch searches the fall's): -riseSearch <way> [-riseRounds 40]
    //   [-riseMany 32] [-riseKeep 6] [-riseSpread 15] [-riseSeed 1] [-riseFirst 0]
    public sealed class OwnRiseBench
    {
        private const int Parts = OwnBody.Count, Kept = Parts + 2;
        private static readonly CultureInfo Plain = CultureInfo.InvariantCulture;
        private struct Lies { public string name; public Vector3[] at; public Quaternion[] turned; public Vector3 side, place; public Quaternion faces; }
        private sealed class Body
        {
            public string name;
            public SelectableUnit unit;
            public PhysicalFall fall;
            public OwnBody own;
            public MinerBody miner;
            public Lies standing;
            public List<Lies> starts = new List<Lies>();
            // What came of a try.
            public bool done, up, onFeet;
            public float feetAt, upAt, peak, stoodFor, hipsHigh, ahead, soles, upright, bears, risen, feetFor, front;
            // (How far within what bears it its weight was last; at which pose, of how many; how long it has kept its
            // weight within what bears it at the last pose.)
            public float depth = float.NaN, stageAt, stages, heldFor;
            public bool upOff;
            public float stepFurthest;
            // (How many times it moved its weight off a limb before lifting it, and how many it lifted it all the same.)
            public int shifts, shiftsFailed;
            // (When it first came to its last pose; NaN: not yet.)
            public float lastAt = float.NaN;
            // (The way it began by; when it went on by another, the way that way turns into the next; and how far onto
            // its front it came at most.)
            public string firstWay = "", lastStage = "", released = "", handedOver = "";
            public bool spinTold;
            public Vector3[] targetWas = new Vector3[2];
            public int spinAfter;
            public float turnedAt = float.NaN, frontMost = -1;
            // (-riseWildTrace 1: its last steps, told only if it goes wild.)
            public readonly Queue<string> recent = new Queue<string>();
            // (How fast its boots have turned at most, radians a second: a boot spinning is what went before a body
            // flew apart, October 20.)
            public float bootSpin;
            public string bootSpinAt = "";
            public Vector3 shakeFrom, shakeTo;
            public OwnRiser.Way[] ways;
            public string ended;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        // Ways from a file. A line "way <name> <front least> <front most> gives|turns", then a line for each pose:
        //   name | over atLeast within | trunk head twist | shoulder out elbow | shoulder out elbow |
        //   hip out knee ankle | hip out knee ankle | soles>0.9 bears>0.8 ahead<0.05 keep=0.02
        // (the leading side first; turns from standing, in degrees).
        // Ways from a file, read as the game reads them (OwnRiser.Read).
        internal static OwnRiser.Way[] Read(string file) => OwnRiser.Read(File.ReadAllText(file));

        internal static void Write(OwnRiser.Way[] ways, string file, string note)
        {
            var text = new StringBuilder();
            text.AppendLine("# " + note);
            foreach (var way in ways)
            {
                text.AppendLine(string.Format(Plain, "way {0} {1:0.##} {2:0.##} {3}", way.name, way.frontLeast, way.frontMost, way.gives ? "gives" : "turns"));
                foreach (var stage in way.stages)
                {
                    var p = stage.pose;
                    text.Append(string.Format(Plain, "{0} | {1:0.00} {2:0.00} {3:0.0} | {4:0} {5:0} {6:0} | {7:0} {8:0} {9:0} | {10:0} {11:0} {12:0} | {13:0} {14:0} {15:0} {16:0} | {17:0} {18:0} {19:0} {20:0} |",
                        stage.name, stage.over, stage.atLeast, stage.within, p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11], p[12], p[13], p[14], p[15], p[16]));
                    foreach (var asked in stage.through)
                        text.Append(string.Format(Plain, " {0}{1}{2:0.##}", asked.what.ToString().ToLowerInvariant(), float.IsNegativeInfinity(asked.least) ? "<" : ">", float.IsNegativeInfinity(asked.least) ? asked.most : asked.least));
                    if (!float.IsNaN(stage.keeps)) text.Append(string.Format(Plain, " keep={0:0.00}", stage.keeps));
                    if (!float.IsNaN(stage.balance)) text.Append(string.Format(Plain, " bal={0:0.00},{1:0.00}{2}", stage.balance, stage.balanceFeet, stage.balanceLead > 0 ? string.Format(Plain, ",{0:0.00}", stage.balanceLead) : ""));
                    if (stage.letsHandsGo) text.Append(" hands=off");
                    if (stage.risesHips > 0) text.Append(string.Format(Plain, " rights={0:0}", stage.risesHips));
                    if (stage.holds) text.Append(string.Format(Plain, " hold={0:0.00},{1:0.00}{2}", stage.holdHigh, stage.holdRate, stage.holdsFeet ? ",feet" : ""));
                    if (stage.flattens > 0) text.Append(string.Format(Plain, " flat={0:0}", stage.flattens));
                    if (stage.kneels > 0) text.Append(string.Format(Plain, " kneel={0:0.00}", stage.kneels));
                    if (stage.laysBoots) text.Append(" laid");
                    if (!float.IsNaN(stage.handStep)) text.Append(string.Format(Plain, " walk={0:0.000},{1:0.00},{2:0.000},{3:0.00}", stage.handStep, stage.handOver, stage.handLift, stage.handNearest));
                    if (!float.IsNaN(stage.sits)) text.Append(string.Format(Plain, " sit={0:0},{1:0.00},{2:0.00},{3:0}", stage.sits, stage.sitHips, stage.sitAhead, stage.sitMost));
                    if (stage.steps >= 0) text.Append(string.Format(Plain, " step={0},{1:0.00},{2:0.00},{3:0.00}{4}", stage.steps == 0 ? "lead" : "other", stage.stepAhead, stage.stepOut, stage.stepLifts, stage.stepUnder > 0 ? string.Format(Plain, ",{0:0.00}", stage.stepUnder) : ""));
                    text.AppendLine();
                }
            }
            File.WriteAllText(file, text.ToString());
        }

        // How the let-go body lies now: its eleven parts, its boots and its toes.
        private static Lies Now(PhysicalFall fall, string name)
        {
            var lies = new Lies { name = name, at = new Vector3[Kept], turned = new Quaternion[Kept] };
            for (int i = 0; i < PhysicalFall.Count; i++) { lies.at[i] = fall.Part(i).transform.position; lies.turned[i] = fall.Part(i).transform.rotation; }
            for (int i = 0; i < 2; i++)
                fall.BootNow(i, out lies.at[OwnBody.Foot + i], out lies.turned[OwnBody.Foot + i], out lies.at[Parts + i], out lies.turned[Parts + i]);
            return lies;
        }

        private static void Put(PhysicalFall fall, Lies lies)
        {
            for (int i = 0; i < PhysicalFall.Count; i++)
            {
                var part = fall.Part(i);
                part.transform.SetPositionAndRotation(lies.at[i], lies.turned[i]);
                part.linearVelocity = Vector3.zero; part.angularVelocity = Vector3.zero;
            }
        }

        // The posed body's segments put as the body lay, for the body's own to be made from.
        private static void Lay(MinerBody miner, Lies lies)
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

        [UnityTest, Explicit, Timeout(14400000)]
        public IEnumerator Bench()
        {
            // (A bench, not a test: what the engine says of a body gone wild is told, and the bench goes on.)
            LogAssert.ignoreFailingMessages = true;
            string folder = CaptureTools.Argument("-riseOut");
            string file = CaptureTools.Argument("-riseWays");
            Assert.That(folder, Is.Not.Null.And.Not.Empty, "-riseOut <folder>");
            Assert.That(file, Is.Not.Null.And.Not.Empty, "-riseWays <file>");
            string[] who = (CaptureTools.Argument("-riseMiner") ?? "Small,Long,Round").Split(',');
            string[] from = (CaptureTools.Argument("-riseStarts") ?? "front,back").Split(',');
            bool knees = (CaptureTools.Argument("-riseFrom") ?? "knees") == "knees";
            float Number(string argument, float otherwise) { string given = CaptureTools.Argument(argument); return string.IsNullOrEmpty(given) ? otherwise : float.Parse(given, Plain); }
            float lasts = Number("-riseFor", 12);
            string shots = CaptureTools.Argument("-riseShots");
            int every = (int)Number("-riseEvery", 10), size = (int)Number("-riseSize", 360);
            string search = CaptureTools.Argument("-riseSearch");
            bool ticks = CaptureTools.Argument("-riseTick") == "1";
            // (-riseNear 0.5: pictures from so much nearer; -riseAngle 45: from so many degrees round from its side,
            // towards its head.)
            float near = Number("-riseNear", 1), angle = Number("-riseAngle", 0);
            int frames = (int)Number("-riseFrames", 0);
            // (-riseTickEvery 1: what the getting up does at every step of the physics, not every tenth.)
            int tickEvery = Mathf.Max(1, (int)Number("-riseTickEvery", 10));
            float spinTrace = Number("-riseSpinTrace", 0);
            bool wildTrace = Number("-riseWildTrace", 0) > 0 || spinTrace > 0;
            // (-riseGoal turn: a search for a way that turns the body onto its front, which counts how soon it did.)
            bool turning = CaptureTools.Argument("-riseGoal") == "turn";
            float shake = Number("-riseShake", 0);
            var shakeChance = new System.Random(7);
            float jitterTurn = 0, jitterMove = 0;
            int jitterSeed = (int)Number("-riseSeed", 1);
            {
                var jitter = (CaptureTools.Argument("-riseJitter") ?? "").Split(',');
                if (jitter[0].Length > 0) jitterTurn = float.Parse(jitter[0], Plain);
                if (jitter.Length > 1) jitterMove = float.Parse(jitter[1], Plain);
            }
            // (-riseStrangers 1: what else, that moves, touches a body getting up: each pair once.)
            var strangers = new HashSet<string>();
            if (Number("-riseStrangers", 0) > 0)
                GroundTouch.Touched = (part, other) =>
                {
                    string what = part.name + " | " + (other.attachedRigidbody != null ? "body " : other.GetComponentInParent<ArticulationBody>() != null ? "articulation " : "") + other.name + " (" + (other.transform.root != null ? other.transform.root.name : "") + ")";
                    if (strangers.Add(what)) Debug.Log("RISE_STRANGER " + what + " at " + Time.time.ToString("0.00", Plain));
                };
            // (-riseTold 1: what the bodies say as they go, the standing balance's own telling among it: why it would not
            // rise, why it went down.)
            var toldWas = OwnBody.Told;
            if (Number("-riseTold", 0) > 0) OwnBody.Told = said => Debug.Log("RISE_TOLD " + said);
            // (-riseArms 3: to try a thing: their arms so many times as strong as they are.)
            float armsWere = OwnBody.ArmsTimes;
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var look = UnityEngine.Object.FindAnyObjectByType<MinerWorkPreview>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            var camera = Camera.main;
            var rig = camera.GetComponent<RtsCamera>();
            if (rig != null) rig.enabled = false;
            camera.nearClipPlane = .01f; camera.fieldOfView = 30;
            var grass = UnityEngine.Object.FindAnyObjectByType<GrassField>();
            if (grass != null) grass.enabled = false;
            var time = UnityEngine.Object.FindAnyObjectByType<TimeOfDay>();
            if (time != null) time.Hour = 13;

            // The miners, each at its own place on the path. Each stands by its own joints once, so that how it stands
            // is known; then all are there at once.
            var bodies = new List<Body>();
            Vector3 Spot(int n, out Vector3 away)
            {
                var a = ground.Path[(3 + n) % (ground.Path.Count - 1)];
                var b = ground.Path[(3 + n) % (ground.Path.Count - 1) + 1];
                away = (new Vector3(b.x, 0, b.y) - new Vector3(a.x, 0, a.y)).normalized;
                return new Vector3(a.x, ground.Height(a.x, a.y), a.y);
            }
            for (int n = 0; n < who.Length; n++)
            {
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (string.Equals(choice.NameOf(i), who[n], StringComparison.OrdinalIgnoreCase)) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner called " + who[n]);
                choice.Choose(index);
                yield return null;
                var unit = choice.Current;
                Vector3 spot = Spot(n, out Vector3 away);
                unit.Motor.Stop();
                unit.GetComponent<NavMeshAgent>().Warp(spot);
                unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                unit.GetComponent<ProceduralBiped>().SetStance(0, 0);
                unit.GetComponent<ProceduralBiped>().ResetPose();
                for (float until = Time.time + 1.5f; Time.time < until;) yield return null;
                look.SetOwn(true);
                OwnBody own = null;
                for (float began = Time.time; Time.time - began < 5 && (own == null || !own.Stands);) { unit.TryGetComponent(out own); yield return null; }
                Assert.That(own != null && own.Stands, Is.True, who[n] + " did not come to stand by its own joints.");
                for (float until = Time.time + 1; Time.time < until;) yield return null;
                look.SetOwn(false);
                for (float until = Time.time + .8f; Time.time < until;) yield return null;
                Assert.That(own.CanRise, Is.True, who[n] + ": how it stands is not known.");
                bodies.Add(new Body { name = choice.NameOf(index), unit = unit, own = own, miner = unit.GetComponent<MinerBody>() });
                var weighs = unit.GetComponent<PhysicalBody>();
                Debug.Log(string.Format(Plain, "RISE_ARMS {0}: {1:0.0} kg; shoulder {2:0} N m by its thickness, {3:0} as its body needs, {4:0} it has; elbow {5:0}, {6:0}, {7:0}", choice.NameOf(index), weighs.Mass,
                    weighs.ShoulderOfThickness, weighs.ShoulderNeeds, weighs.ShoulderCapacity, weighs.ElbowOfThickness, weighs.ElbowNeeds, weighs.ElbowCapacity));
            }
            for (int n = 0; n < bodies.Count; n++)
            {
                var unit = bodies[n].unit;
                unit.gameObject.SetActive(true);
                Vector3 spot = Spot(n, out Vector3 away);
                unit.Motor.Stop();
                unit.GetComponent<NavMeshAgent>().Warp(spot);
                unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away));
                unit.GetComponent<ProceduralBiped>().ResetPose();
            }
            for (float until = Time.time + 1; Time.time < until;) yield return null;
            foreach (var body in bodies)
            {
                body.fall = body.unit.GetComponent<PhysicalFall>();
                if (body.fall == null) body.fall = body.unit.gameObject.AddComponent<PhysicalFall>();
                body.fall.GetsUp = false; body.fall.Gives = false;
                body.fall.LetGo();
                Assert.That(body.fall.Now, Is.EqualTo(PhysicalFall.State.Falling), body.name + " was not let go.");
            }
            var mode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            float dt = Time.fixedDeltaTime;
            // (The game's frames, where there are frames, are as long as the physics between them, as in the game: so
            // what runs by the frame (its breath, its hands, what hangs on it) keeps the physics' time. Before October
            // 15 they kept the clock on the wall, and the bench ran faster or slower than that; -riseWallClock 1: so
            // again.)
            float timeWas = Time.captureDeltaTime;
            if (Number("-riseWallClock", 0) <= 0) Time.captureDeltaTime = dt * Mathf.Max(1, frames > 0 ? frames : every);
            bool fellBack = OwnBody.FallsBack;
            var waysWere = OwnBody.RiseWays;
            try
            {
                // The ways they come to lie (and, if asked, where the fall's own ways then bring them).
                foreach (var body in bodies) body.standing = Now(body.fall, "standing");
                foreach (string asked in from)
                {
                    // (front@3: laid at another place on the path, so many places on from its own, facing as the path goes
                    // there. The meadow's ground lies differently at each: a way found at one place is not yet one
                    // that serves at another.)
                    // (front@3~90: and turned so many degrees from the way the path goes there.)
                    string how = asked.Split('@')[0];
                    string onWord = asked.Contains("@") ? asked.Split('@')[1] : "0";
                    float turned = onWord.Contains("~") ? float.Parse(onWord.Split('~')[1], Plain) : 0;
                    int on = int.Parse(onWord.Split('~')[0], Plain);
                    for (int n = 0; n < bodies.Count; n++)
                    {
                        var body = bodies[n];
                        Put(body.fall, body.standing);
                        Vector3 own = Spot(n, out Vector3 ownAway);
                        Quaternion ownTurn = Quaternion.LookRotation(ownAway);
                        if (on != 0 || turned != 0)
                        {
                            Vector3 there = Spot(n + on, out Vector3 thereAway);
                            thereAway = Quaternion.AngleAxis(turned, Vector3.up) * thereAway;
                            Quaternion turn = Quaternion.LookRotation(thereAway) * Quaternion.Inverse(ownTurn);
                            for (int i = 0; i < PhysicalFall.Count; i++)
                            {
                                var part = body.fall.Part(i);
                                part.transform.SetPositionAndRotation(there + turn * (part.transform.position - own), turn * part.transform.rotation);
                            }
                            body.unit.transform.SetPositionAndRotation(there, Quaternion.LookRotation(thereAway));
                        }
                        else body.unit.transform.SetPositionAndRotation(own, ownTurn);
                        Physics.SyncTransforms();
                        body.fall.Fell();
                        if (!how.StartsWith("s")) PhysicalFallBench.Lay(body.fall, body.unit.transform, ground, how);
                    }
                    Physics.SyncTransforms();
                    for (int step = 0; step < 450; step++)
                    {
                        if (step * dt < .3f && how.StartsWith("s"))
                            foreach (var body in bodies)
                                body.fall.Push(Quaternion.AngleAxis(float.Parse(how.Substring(1), Plain), Vector3.up) * body.unit.transform.forward * (6 * body.unit.GetComponent<PhysicalBody>().Mass), body.fall.Part(PhysicalFall.Trunk).worldCenterOfMass);
                        foreach (var body in bodies) body.fall.Advance(dt);
                        Physics.Simulate(dt);
                    }
                    if (knees)
                    {
                        foreach (var body in bodies) { body.fall.LieStill(); body.fall.Rouse(); }
                        for (int step = 0; step < 900; step++)
                        {
                            bool going = false;
                            foreach (var body in bodies)
                            {
                                if (body.fall.Now != PhysicalFall.State.Gathering || body.fall.Through) continue;
                                going = true;
                            }
                            if (!going) break;
                            foreach (var body in bodies) body.fall.Advance(dt);
                            Physics.Simulate(dt);
                        }
                        // (A moment more, to be still.)
                        for (int step = 0; step < 25; step++) { foreach (var body in bodies) body.fall.Advance(dt); Physics.Simulate(dt); }
                    }
                    foreach (var body in bodies)
                    {
                        var lies = Now(body.fall, asked);
                        lies.side = Vector3.Cross(Vector3.up, Flat(body.unit.transform.forward).normalized);
                        lies.place = body.unit.transform.position; lies.faces = body.unit.transform.rotation;
                        body.starts.Add(lies);
                        Debug.Log(string.Format(Plain, "RISE {0} lies ({1}{2}): its chest to the ground by {3:0.00}, its hips {4:0.00} of their height up; the fall: {5} {6}", body.name, how, knees ? ", then onto its knees" : "",
                            body.fall.Measure(PhysicalFall.Asks.Front), body.fall.Measure(PhysicalFall.Asks.Hips), body.fall.Now, body.fall.Through ? "through" : "not through"));
                    }
                }
                foreach (var body in bodies) { body.fall.Gives = true; body.fall.GetsUp = true; body.fall.TakeBack(); }
                yield return null;
                yield return null;

                OwnBody.FallsBack = false;
                OwnBody.ArmsTimes = Number("-riseArms", 1);
                // (-riseHead 0.7: to learn what the head's weight does: its head so many times as heavy.)
                OwnBody.HeadTimes = Number("-riseHead", 1);
                // (-riseRule 10: the getting up's springs capped by a rule so many times looser.)
                OwnRiser.RuleTimes = Number("-riseRule", OwnRiser.RuleTimes);
                // (-riseBalQuick, -riseBalLeans, -riseBalMargin, -riseBalUnloads, -riseBalWithin: the balance's numbers.)
                OwnRiser.BalanceQuick = Number("-riseBalQuick", OwnRiser.BalanceQuick);
                OwnRiser.BalanceLeans = Number("-riseBalLeans", OwnRiser.BalanceLeans);
                OwnRiser.BalanceMargin = Number("-riseBalMargin", OwnRiser.BalanceMargin);
                OwnRiser.BalanceUnloads = Number("-riseBalUnloads", OwnRiser.BalanceUnloads);
                OwnRiser.BalanceShiftsWithin = Number("-riseBalWithin", OwnRiser.BalanceShiftsWithin);
                OwnRiser.BalanceDamps = Number("-riseBalDamps", OwnRiser.BalanceDamps);
                OwnRiser.KneeFaces = Number("-riseKneeFaces", OwnRiser.KneeFaces);
                // (-riseSet LetsGo=0.15,BalanceMargin=0.012: any of the getting up's own numbers, set so.)
                foreach (string word in (CaptureTools.Argument("-riseSet") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var bits = word.Split('=');
                    var field = typeof(OwnRiser).GetField(bits[0], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    Assert.That(field, Is.Not.Null, "No number called " + bits[0]);
                    if (field.FieldType == typeof(bool)) field.SetValue(null, float.Parse(bits[1], Plain) > .5f);
                    else field.SetValue(null, float.Parse(bits[1], Plain));
                    Debug.Log("RISE_SET " + bits[0] + " = " + bits[1]);
                }
                // (-riseQuiet PhysicalHands,Breath: those behaviours on each miner switched off, to find what frames change.)
                foreach (string quiet in (CaptureTools.Argument("-riseQuiet") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    foreach (var body in bodies)
                        foreach (var part in body.unit.GetComponentsInChildren<Behaviour>(true))
                            if (part.GetType().Name == quiet) { part.enabled = false; Debug.Log("RISE_QUIET " + body.name + " " + quiet); }
                // (-riseStaysPut 1: the unit's own place does not go with the body getting up, to find what frames change.)
                if (Number("-riseStaysPut", 0) > 0) OwnBody.StaysPut = true;
                OwnRiser.BalanceLeastMove = Number("-riseLeastMove", OwnRiser.BalanceLeastMove ? 1 : 0) > 0;
                OwnRiser.BalancePushes = Number("-riseBalPushes", OwnRiser.BalancePushes);
                OwnRiser.BalanceShiftsAt = Number("-riseBalShiftsAt", OwnRiser.BalanceShiftsAt);
                OwnRiser.BalanceShiftsAtMost = Number("-riseBalShiftsAtMost", OwnRiser.BalanceShiftsAtMost);
                // (The panel's switch on: with it off, the miner that is chosen is given back to the posed body at the
                // next frame.)
                look.SetOwn(true);
                // (-riseWays keep_{0}.txt: each miner its own file, its name in small letters where {0} stands.)
                foreach (var body in bodies) body.ways = Read(string.Format(file, body.name.ToLowerInvariant()));
                var ways = bodies[0].ways;

                // One try: from one way of lying, every body made its own as it lies; stepped until each stands, has gone
                // down, or the time is over. (pictures: of which body, if any.)
                IEnumerator Try(int j, Body pictured, string tag)
                {
                    foreach (var body in bodies)
                    {
                        if (body.own.Stands) body.own.Drop();
                        // (Its own place where this start was made: it goes with its body from there.)
                        body.unit.transform.SetPositionAndRotation(body.starts[j].place, body.starts[j].faces);
                        // (-riseJitter 20,0.3: each try laid turned up to so many degrees either way about its hips, and moved up to
                        // so far, by chance (the same chances for every candidate in a round of a search, others the next round):
                        // a way that serves only the very place it was searched at is not a way.)
                        var laid = body.starts[j];
                        if (jitterTurn > 0 || jitterMove > 0)
                        {
                            var chance = new System.Random(jitterSeed * 7919 + j * 31 + body.name.Length);
                            float yaw = (float)(chance.NextDouble() * 2 - 1) * jitterTurn;
                            Vector3 move = new Vector3((float)(chance.NextDouble() * 2 - 1), 0, (float)(chance.NextDouble() * 2 - 1)) * jitterMove;
                            Vector3 hipsWere = laid.at[OwnBody.Hips];
                            Vector3 hipsGo = hipsWere + move;
                            float Under(Vector3 at) => Physics.Raycast(at + Vector3.up * 3, Vector3.down, out var hit, 8, 1 << 6) ? hit.point.y : at.y;
                            move.y = Under(hipsGo) - Under(hipsWere);
                            Quaternion turn = Quaternion.AngleAxis(yaw, Vector3.up);
                            var at = (Vector3[])laid.at.Clone(); var turned = (Quaternion[])laid.turned.Clone();
                            for (int k = 0; k < at.Length; k++) { at[k] = hipsWere + move + turn * (laid.at[k] - hipsWere); turned[k] = turn * laid.turned[k]; }
                            laid.at = at; laid.turned = turned;
                            laid.place = hipsWere + move + turn * (laid.place - hipsWere); laid.faces = turn * laid.faces;
                            laid.side = turn * laid.side;
                        }
                        body.unit.transform.SetPositionAndRotation(laid.place, laid.faces);
                        Lay(body.miner, laid);
                        if (ticks) Debug.Log("RISE_BEGAN " + body.name + " made its own body " + body.own.Began + " times before try " + j);
                        body.own.Wanted = true;
                        OwnBody.RiseWays = body.ways;
                        body.done = !body.own.BeginDown();
                        body.up = body.onFeet = false; body.feetAt = body.upAt = -1; body.peak = body.stoodFor = body.risen = body.feetFor = 0; body.ended = body.done ? "could not be made" : "";
                        body.depth = float.NaN; body.stageAt = body.stages = body.heldFor = 0; body.upOff = false; body.stepFurthest = 0; body.shifts = body.shiftsFailed = 0; body.lastAt = float.NaN; body.recent.Clear(); body.bootSpin = 0; body.bootSpinAt = "";
                        body.firstWay = ""; body.lastStage = ""; body.released = ""; body.handedOver = ""; body.turnedAt = float.NaN; body.frontMost = -1; body.spinTold = false;
                    }
                    Physics.SyncTransforms();
                    int shot = 0;
                    // (-riseNear 1 and -riseSurvey 1: every collider within a metre and a half of a body getting up that is not
                    // its own, once, at the first try: what else is there that frames could move.)
                    if (j == 0 && Number("-riseSurvey", 0) > 0)
                        foreach (var body in bodies)
                        {
                            if (!body.own.Stands) continue;
                            foreach (var part in body.unit.GetComponentsInChildren<Behaviour>(true)) Debug.Log("RISE_SURVEY " + body.name + " has " + part.GetType().Name + " on " + part.name + (part.enabled ? "" : " (off)"));
                            Vector3 hipsAt = body.own.Part(OwnBody.Hips).transform.position;
                            foreach (var near in Physics.OverlapSphere(hipsAt, 1.5f, ~0, QueryTriggerInteraction.Collide))
                            {
                                if (near.transform.root.name.EndsWith("(its own)")) continue;
                                string path = near.name; for (var up = near.transform.parent; up != null; up = up.parent) path = up.name + "/" + path;
                                Debug.Log("RISE_SURVEY " + body.name + ": " + path + " " + near.GetType().Name + (near.isTrigger ? " trigger" : "") + (near.attachedRigidbody != null ? " body" + (near.attachedRigidbody.isKinematic ? " kinematic" : "") : "") + (near.enabled ? "" : " off"));
                            }
                        }
                    for (int step = 0; step * dt < lasts; step++)
                    {
                        bool going = false;
                        foreach (var body in bodies)
                        {
                            if (body.done) continue;
                            var own = body.own;
                            // (-riseShake 0.03: small pushes by chance on its trunk while it gets up, so much of its weight at most,
                            // changing their way every fifth of a second: a way that keeps it only so long as nothing at all
                            // disturbs it is not one.)
                            if (shake > 0 && own.GettingUp)
                            {
                                if (step % 10 == 0)
                                {
                                    body.shakeFrom = body.shakeTo;
                                    body.shakeTo = new Vector3((float)(shakeChance.NextDouble() * 2 - 1), (float)(shakeChance.NextDouble() * 2 - 1) * .5f, (float)(shakeChance.NextDouble() * 2 - 1));
                                }
                                float u = (step % 10) / 10f;
                                float weighs = 0;
                                for (int i = 0; i < OwnBody.Count; i++) weighs += own.Part(i).mass;
                                own.Part(OwnBody.Trunk).AddForce(Vector3.Lerp(body.shakeFrom, body.shakeTo, u * u * (3 - 2 * u)) * (shake * weighs * Physics.gravity.magnitude));
                            }
                            own.Advance(dt);
                            if (own.Stands && float.IsNaN(own.Part(OwnBody.Hips).transform.position.x))
                            {
                                body.done = true; body.ended = "went wild";
                                foreach (string was in body.recent) Debug.Log("RISE_BEFORE " + was);
                                body.recent.Clear();
                                Debug.Log("RISE_WILD " + body.name + " at " + (step * dt).ToString("0.00", Plain) + " s, " + (own.GettingUp ? own.Riser.WayNow + " " + own.Riser.StageNow : ""));
                                continue;
                            }
                            if (wildTrace && own.GettingUp)
                            {
                                int fastest = 0; float most = 0;
                                for (int i = 0; i < OwnBody.Count; i++)
                                {
                                    float v = own.Part(i).GetPointVelocity(own.Part(i).worldCenterOfMass).magnitude;
                                    if (v > most) { most = v; fastest = i; }
                                }
                                Vector3 spinL = own.Part(OwnBody.Foot).angularVelocity, spinR = own.Part(OwnBody.Foot + 1).angularVelocity;
                                string Local(int i, Vector3 spin)
                                {
                                    var ankle = own.Part(OwnBody.Foot + i).jointVelocity;
                                    string joint = "";
                                    for (int d = 0; d < ankle.dofCount; d++) joint += (d > 0 ? ", " : "") + ankle[d].ToString("0.0", Plain);
                                    var link = own.Part(OwnBody.Foot + i);
                                    Vector3 target = new Vector3(link.xDrive.target, link.yDrive.target, link.zDrive.target);
                                    float moved = (target - body.targetWas[i]).magnitude;
                                    body.targetWas[i] = target;
                                    return (Quaternion.Inverse(link.transform.rotation) * spin).ToString("0.0") + " ankle (" + joint + ") shin " + own.Part(OwnBody.Shin + i).angularVelocity.magnitude.ToString("0.0", Plain)
                                        + " asked " + target.ToString("0") + " moved " + moved.ToString("0", Plain) + " cap " + link.maxJointVelocity.ToString("0", Plain);
                                }
                                body.recent.Enqueue(string.Format(Plain, "{0} try {10} {1:0.00} s | fastest {2} {3:0.00} m/s, turning {4:0.0} rad/s | boots turning {6:0.0} {7} and {8:0.0} {9} rad/s (their own axes) | {5}", body.name, step * dt, own.Part(fastest).name, most, own.Part(fastest).angularVelocity.magnitude, own.Riser.Tick(),
                                    spinL.magnitude, Local(0, spinL), spinR.magnitude, Local(1, spinR), j));
                                while (body.recent.Count > 80) body.recent.Dequeue();
                                // (-riseSpinTrace 30: told once a try, when a boot first turns faster than that.)
                                if (spinTrace > 0 && !body.spinTold && Mathf.Max(spinL.magnitude, spinR.magnitude) > spinTrace)
                                {
                                    body.spinTold = true;
                                    foreach (string was in body.recent) Debug.Log("RISE_SPIN " + was);
                                    // (And what follows, for as long again.)
                                    body.spinAfter = 40;
                                }
                                else if (body.spinAfter > 0) { body.spinAfter--; Debug.Log("RISE_SPIN_AFTER " + body.recent.ToArray()[body.recent.Count - 1]); }
                            }
                            if (!own.Stands) { body.done = true; body.ended = own.GaveUpGettingUp > 0 && !body.onFeet ? "gave up" : "went down"; continue; }
                            for (int i = OwnBody.Hips; i <= OwnBody.Head; i++) body.peak = Mathf.Max(body.peak, own.Part(i).GetPointVelocity(own.Part(i).worldCenterOfMass).magnitude);
                            for (int i = 0; i < 2; i++)
                            {
                                float spin = own.Part(OwnBody.Foot + i).angularVelocity.magnitude;
                                if (spin > body.bootSpin) { body.bootSpin = spin; body.bootSpinAt = (step * dt).ToString("0.00", Plain) + " s, " + (own.GettingUp ? own.Riser.StageNow + " " + own.Riser.Measure(OwnRiser.Asks.Front).ToString("0.00", Plain) : "standing") + (i == 0 ? ", left" : ", right"); }
                            }
                            if (own.GettingUp)
                            {
                                var riser = own.Riser;
                                body.hipsHigh = riser.Measure(OwnRiser.Asks.Hips); body.ahead = riser.Measure(OwnRiser.Asks.Ahead); body.soles = riser.Measure(OwnRiser.Asks.Soles);
                                body.upright = riser.Measure(OwnRiser.Asks.Upright); body.bears = riser.Measure(OwnRiser.Asks.Bears); body.front = riser.Measure(OwnRiser.Asks.Front);
                                body.ended = riser.WayNow + " " + riser.StageNow;
                                body.lastStage = body.ended + string.Format(Plain, " {0:0.00} s", step * dt);
                                if (riser.Released.Length > 0) body.released = riser.Released;
                                if (riser.HandedOver.Length > 0) body.handedOver = riser.HandedOver;
                                body.depth = riser.BalanceDepth; body.stageAt = riser.StageAt; body.stages = riser.Stages; body.stepFurthest = riser.StepFurthest; body.shifts = riser.Shifts; body.shiftsFailed = riser.ShiftsFailed;
                                if (float.IsNaN(body.lastAt) && riser.Stages > 0 && riser.StageAt == riser.Stages - 1) body.lastAt = step * dt;
                                body.frontMost = Mathf.Max(body.frontMost, body.front);
                                if (body.firstWay.Length == 0) body.firstWay = riser.WayNow;
                                else if (float.IsNaN(body.turnedAt) && riser.WayNow != body.firstWay)
                                {
                                    body.turnedAt = step * dt;
                                    // (-riseGoal turn: the try is over once it has turned.)
                                    if (turning) { body.done = true; body.ended = "turned"; continue; }
                                }
                                // (Held only up off the ground: its head and hips up. Lying on its back with its hands and boots on
                                // the ground, its weight is within them too, and a search took that.)
                                body.upOff = riser.Measure(OwnRiser.Asks.Head) >= .5f && riser.Measure(OwnRiser.Asks.Hips) >= .3f;
                                if (riser.Stages > 0 && riser.StageAt == riser.Stages - 1 && riser.BalanceDepth > OwnRiser.BalanceMargin && body.upOff) body.heldFor += dt;
                                // (-riseTick 1: what the getting up does, every tenth step of the physics.)
                                if (ticks && step % tickEvery == 0) Debug.Log(string.Format(Plain, "RISE_GETTING {0} {1:0.00} s | {2}", body.name, step * dt, riser.Tick()));
                            }
                            else if (!body.onFeet)
                            {
                                body.onFeet = true; body.feetAt = step * dt;
                                // (Handed to its standing balance at the step before: what it was like then.)
                                if (own.HandedOver.Length > 0) body.handedOver = own.HandedOver;
                            }
                            if (body.onFeet)
                            {
                                body.feetFor += dt; body.risen = Mathf.Max(body.risen, own.Risen);
                                // (-riseTick 1: what the keeper reckons, every tenth step of the physics.)
                                if (ticks && step % tickEvery == 0) Debug.Log(string.Format(Plain, "RISE_TICK {0} {1:0.00} s risen {2:0.00} | {3}", body.name, step * dt, own.Risen, own.Tick(Flat(own.Part(OwnBody.Hips).transform.forward).normalized, new Vector3(own.Part(OwnBody.Foot).transform.position.x, body.starts[j].at[0].y - .2f, own.Part(OwnBody.Foot).transform.position.z))));
                            }
                            if (body.onFeet && !own.Rising)
                            {
                                if (!body.up) { body.up = true; body.upAt = step * dt; }
                                body.stoodFor += dt;
                                if (body.stoodFor >= 1) { body.done = true; body.ended = "stands"; continue; }
                            }
                            going = true;
                        }
                        if (!going) break;
                        int takenBefore = GroundTouch.Taken;
                        Physics.Simulate(dt);
                        int takenBySimulate = GroundTouch.Taken - takenBefore;
                        // (-riseFrames 5: a frame of the game every so many steps of the physics, pictures or not, as the game
                        // has them between its steps.)
                        if (frames > 0 && step % frames == 0 && !(pictured != null && step % every == 0))
                        {
                            int takenBeforeFrame = GroundTouch.Taken;
                            yield return null;
                            // (-riseSurvey 1: how many touches the physics told of, and how many more the frame did.)
                            if (j == 0 && step % 50 == 0 && Number("-riseSurvey", 0) > 0) Debug.Log("RISE_TAKEN step " + step + ": " + takenBySimulate + " by the physics, " + (GroundTouch.Taken - takenBeforeFrame) + " by the frame");
                        }
                        if (pictured != null && step % every == 0)
                        {
                            yield return null;
                            if (pictured.own.Stands)
                            {
                                Vector3 hipsAt = pictured.own.Part(OwnBody.Hips).transform.position;
                                Vector3 away = pictured.unit.transform.forward;
                                float tall = 1.3f * pictured.unit.GetComponent<ProceduralBiped>().StandingHipHeight + .5f;
                                Vector3 looksAt = new Vector3(hipsAt.x, pictured.starts[j].at[0].y + .3f * tall - .1f, hipsAt.z);
                                Vector3 right = Quaternion.AngleAxis(-angle, Vector3.up) * pictured.starts[j].side;
                                Vector3 fromWhere = looksAt + right * (4.2f * tall * near) + Vector3.up * (.5f * tall * near);
                                camera.transform.SetPositionAndRotation(fromWhere, Quaternion.LookRotation(looksAt - fromWhere));
                                CaptureTools.Render(camera, Path.Combine(folder, $"{tag}_{shot:000}"), size, size);
                            }
                            shot++;
                        }
                    }
                    foreach (var body in bodies)
                    {
                        if (!body.done) body.ended = body.onFeet ? "still rising when the time was over" : "not on its feet when the time was over, at " + body.ended;
                        if (body.own.Stands) body.own.Drop();
                    }
                }

                string Told(Body body, int j) => string.Format(Plain, "RISE {0} from {1}: {2}; on its feet at {3:0.00} s, up at {4:0.00} s; fastest {5:0.0} m/s; last, getting up: hips {6:0.00} high, weight {7:0.00} ahead of its soles, soles {8:0.00} flat, trunk {9:0.00} upright, boots bearing {10:0.00}; on its feet {11:0.00} s, risen {12:0.00}",
                    body.name, from[j], body.ended, body.feetAt, body.upAt, body.peak, body.hipsHigh, body.ahead, body.soles, body.upright, body.bears, body.feetFor, body.risen) + string.Format(Plain, "; boots turned at most {0:0.0} rad/s at {1}", body.bootSpin, body.bootSpinAt)
                    + "; last getting up at " + body.lastStage
                    + (body.released.Length > 0 ? "; RELEASED " + body.released : "") + (body.handedOver.Length > 0 ? "; HANDED " + body.handedOver : "")
                    + (float.IsNaN(body.turnedAt) ? string.Format(Plain, "; by {0} only, onto its front {1:0.00} at most", body.firstWay, body.frontMost) : string.Format(Plain, "; from {0} on by another way at {1:0.00} s", body.firstWay, body.turnedAt));

                if (string.IsNullOrEmpty(search))
                {
                    for (int j = 0; j < from.Length; j++)
                    {
                        Body pictured = null;
                        foreach (var body in bodies) if (!string.IsNullOrEmpty(shots) && string.Equals(shots, body.name, StringComparison.OrdinalIgnoreCase)) pictured = body;
                        yield return Try(j, pictured, pictured != null ? pictured.name.ToLowerInvariant() + "_" + from[j] + j : "");
                        foreach (var body in bodies) Debug.Log(Told(body, j));
                        foreach (var body in bodies) Debug.Log(string.Format(Plain, "RISE_HELD {0} from {1}: at pose {2} of {3}, kept its weight within what bears it there, up off the ground, {4:0.00} s; {5:0} mm within at the end, {6}; furthest foot {7:0.00} legs forward; moved off {8}, lifted all the same {9}", body.name, from[j], body.stageAt + 1, body.stages, body.heldFor, body.depth * 1000, body.upOff ? "up" : "down", body.stepFurthest, body.shifts, body.shiftsFailed));
                        yield return null;
                    }
                }
                else
                {
                    // A search of one way's poses: varied towards those that leave the bodies best.
                    int rounds = (int)Number("-riseRounds", 40), many = (int)Number("-riseMany", 32), keep = (int)Number("-riseKeep", 6), seed = (int)Number("-riseSeed", 1), first = (int)Number("-riseFirst", 0);
                    float spread = Number("-riseSpread", 15), hurry = Number("-riseHurry", .05f), holdHurry = Number("-riseHoldHurry", 0);
                    int at = -1;
                    for (int i = 0; i < ways.Length; i++) if (ways[i].name == search) at = i;
                    Assert.That(at, Is.GreaterThanOrEqualTo(0), "No way called " + search + " in " + file);
                    var way = ways[at];
                    // (Each pose's turns, how long it is come to over and kept; and where a foot is put (ahead, out) and how
                    // high its hips are held, where the pose has them.)
                    const int Each = OwnRiser.Turns + 7;
                    int stages = way.stages.Length, count = stages * Each;
                    // (-riseTune BalanceMargin:0.01:0.1,KneeFaces:0:1: the getting up's own numbers, varied too, each
                    // between so little and so much.)
                    var tunes = new List<(System.Reflection.FieldInfo field, float least, float most)>();
                    foreach (string word in (CaptureTools.Argument("-riseTune") ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var bits = word.Split(':');
                        var field = typeof(OwnRiser).GetField(bits[0], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        Assert.That(field, Is.Not.Null, "No number called " + bits[0]);
                        tunes.Add((field, float.Parse(bits[1], Plain), float.Parse(bits[2], Plain)));
                    }
                    int all = count + tunes.Count;
                    Vector2 Limits(int k)
                    {
                        switch (k)
                        {
                            case 0: return new Vector2(-25, 90);
                            case 1: return new Vector2(-45, 45);
                            // (Rolling over, its trunk twists further and an arm reaches across its chest: October 22.)
                            case 2: return new Vector2(-60, 60);
                            case 3: case 6: return new Vector2(-50, 170);
                            case 4: case 7: return new Vector2(-70, 95);
                            case 5: case 8: return new Vector2(0, 145);
                            case 9: case 13: return new Vector2(-20, 125);
                            case 10: case 14: return new Vector2(-45, 50);
                            case 11: case 15: return new Vector2(-25, 125);
                            case 12: case 16: return new Vector2(-45, 45);
                            case OwnRiser.Turns: return new Vector2(.5f, 2.5f);
                            case OwnRiser.Turns + 2: return new Vector2(.1f, .9f);
                            case OwnRiser.Turns + 3: return new Vector2(-.1f, .5f);
                            case OwnRiser.Turns + 4: return new Vector2(.2f, .8f);
                            case OwnRiser.Turns + 5: return new Vector2(0, .35f);
                            case OwnRiser.Turns + 6: return new Vector2(0, .9f);
                            default: return new Vector2(0, 1.5f);
                        }
                    }
                    bool same = Number("-riseSame", 1) > 0;
                    float[] mean = new float[all], wide = new float[all];
                    for (int t = 0; t < tunes.Count; t++)
                    {
                        mean[count + t] = Convert.ToSingle(tunes[t].field.GetValue(null));
                        wide[count + t] = .25f * (tunes[t].most - tunes[t].least);
                    }
                    for (int s = 0; s < stages; s++)
                    {
                        for (int k = 0; k < OwnRiser.Turns; k++) mean[s * Each + k] = way.stages[s].pose[k];
                        mean[s * Each + OwnRiser.Turns] = way.stages[s].over;
                        mean[s * Each + OwnRiser.Turns + 1] = Mathf.Max(0, way.stages[s].atLeast - way.stages[s].over);
                        var st = way.stages[s];
                        bool steps = st.steps >= 0, high = st.holds && !float.IsNaN(st.holdHigh);
                        mean[s * Each + OwnRiser.Turns + 2] = steps ? st.stepAhead : 0;
                        mean[s * Each + OwnRiser.Turns + 3] = steps ? st.stepOut : 0;
                        mean[s * Each + OwnRiser.Turns + 4] = high ? st.holdHigh : 0;
                        mean[s * Each + OwnRiser.Turns + 5] = steps ? st.stepLifts : 0;
                        mean[s * Each + OwnRiser.Turns + 6] = steps ? st.stepUnder : 0;
                        for (int k = 0; k < Each; k++)
                            wide[s * Each + k] = s < first ? 0 : k < OwnRiser.Turns ? spread : k < OwnRiser.Turns + 2 ? .2f * spread / 15
                                : k < OwnRiser.Turns + 4 ? (steps ? .08f * spread / 15 : 0) : k == OwnRiser.Turns + 4 ? (high ? .06f * spread / 15 : 0)
                                : (steps ? .05f * spread / 15 : 0);
                    }
                    void Into(float[] numbers)
                    {
                        for (int s = 0; s < stages; s++)
                        {
                            var stage = way.stages[s];
                            for (int k = 0; k < Each; k++) { var limits = Limits(k); numbers[s * Each + k] = Mathf.Clamp(numbers[s * Each + k], limits.x, limits.y); }
                            // (Both sides alike, and no twist.)
                            if (same && s >= first)
                            {
                                for (int k = 0; k < 3; k++) numbers[s * Each + 6 + k] = numbers[s * Each + 3 + k];
                                for (int k = 0; k < 4; k++) numbers[s * Each + 13 + k] = numbers[s * Each + 9 + k];
                                numbers[s * Each + 2] = 0;
                            }
                            for (int k = 0; k < OwnRiser.Turns; k++) stage.pose[k] = numbers[s * Each + k];
                            stage.over = numbers[s * Each + OwnRiser.Turns];
                            stage.atLeast = stage.over + numbers[s * Each + OwnRiser.Turns + 1];
                            if (stage.steps >= 0) { stage.stepAhead = numbers[s * Each + OwnRiser.Turns + 2]; stage.stepOut = numbers[s * Each + OwnRiser.Turns + 3]; }
                            if (stage.holds && !float.IsNaN(stage.holdHigh)) stage.holdHigh = numbers[s * Each + OwnRiser.Turns + 4];
                            if (stage.steps >= 0) { stage.stepLifts = numbers[s * Each + OwnRiser.Turns + 5]; stage.stepUnder = numbers[s * Each + OwnRiser.Turns + 6]; }
                        }
                        for (int t = 0; t < tunes.Count; t++)
                        {
                            numbers[count + t] = Mathf.Clamp(numbers[count + t], tunes[t].least, tunes[t].most);
                            if (tunes[t].field.FieldType == typeof(bool)) tunes[t].field.SetValue(null, numbers[count + t] > .5f);
                            else tunes[t].field.SetValue(null, numbers[count + t]);
                        }
                    }
                    string Tuned() { var told = new StringBuilder(); for (int t = 0; t < tunes.Count; t++) told.Append(string.Format(Plain, "{0}={1:0.000} ", tunes[t].field.Name, tunes[t].field.GetValue(null))); return told.ToString(); }
                    // How well a body was left.
                    // (-riseGoal hold: how well it came to its way's last pose and kept its weight within what bears it
                    // there: how far it came over onto its front, how far along its way it got, how long it kept its
                    // weight within what bears it at the last pose, and how far within at the end.)
                    bool holding = CaptureTools.Argument("-riseGoal") == "hold";
                    float Worth(Body body)
                    {
                        // (-riseGoal turn: onto its front, and how soon; else how far onto its front it came, and how far along
                        // its way.)
                        if (turning)
                        {
                            float calm = .5f * Mathf.Max(0, body.peak - 2.5f);
                            if (!float.IsNaN(body.turnedAt)) return 12 + 3 * Mathf.Clamp01((body.front + 1) / 1.6f) - hurry * body.turnedAt - calm;
                            return 3 * Mathf.Clamp01((body.frontMost + 1) / 1.6f) + 3 * body.stageAt / Mathf.Max(1, body.stages - 1) - calm;
                        }
                        if (holding)
                        {
                            float got = 3 * Mathf.Clamp01((body.front + 1) / 1.6f) + 3 * body.stageAt / Mathf.Max(1, body.stages - 1) + 3 * Mathf.Clamp01(body.heldFor / 2);
                            // (Most of all: at the last pose at the end, up off the ground, its weight within what bears it; and,
                            // when its way ends on its feet, on its feet, risen, and standing.)
                            if (body.stages > 0 && body.stageAt == body.stages - 1 && body.upOff && body.depth > OwnRiser.BalanceMargin) got += 5;
                            if (body.onFeet) got += 8 + 3 * Mathf.Clamp01(body.risen);
                            // (A foot put down counts by how far forward it went: a search kept its balance by hardly moving
                            // its foot at all.)
                            got += 4 * Mathf.Clamp01(body.stepFurthest / .6f);
                            // (A limb lifted with its weight over the others counts; one lifted all the same, with its weight
                            // still on it, does not.)
                            if (body.shifts + body.shiftsFailed > 0) got += 3f * body.shifts / (body.shifts + body.shiftsFailed);
                            // (-riseHoldHurry: quicker counts, by when it came to its last pose.)
                            if (holdHurry > 0) got -= holdHurry * (float.IsNaN(body.lastAt) ? 12 : body.lastAt);
                            if (body.ended == "stands") got += 10;
                            if (body.stages > 0 && body.stageAt == body.stages - 1 && !float.IsNaN(body.depth)) got += body.upOff ? 10 * Mathf.Clamp(body.depth, -.1f, .15f) : -1;
                            return got - .5f * Mathf.Max(0, body.peak - 2.5f);
                        }
                        if (body.ended == "stands") return 12 - .5f * Mathf.Max(0, body.peak - 1.5f) - hurry * body.upAt;
                        float worth = 0;
                        // (On its feet, and the keeper has it: how far it rose, and how long it kept its feet.)
                        if (body.onFeet) worth += 3 + 3 * Mathf.Clamp01(body.risen) + Mathf.Clamp01(body.feetFor / 3);
                        // (Towards being on its feet: its soles flat, its boots bearing it, its weight over them, its hips up.)
                        // (Its weight's way to over its soles counts all the way: asked for only when it is nearly there, a
                        // body with its soles flat, its hips up and its hands and head on the ground a leg's length ahead
                        // was as good as any.)
                        worth += Mathf.Clamp01(body.soles) + 2 * Mathf.Clamp01(body.bears) + 2 * (1 - Mathf.Clamp01(Mathf.Abs(body.ahead) / .7f)) + Mathf.Clamp(body.hipsHigh, 0, .6f) + 2 * Mathf.Clamp01((body.upright + .4f) / 1.2f);
                        // (And, before any of that, how far it has come over onto its front.)
                        worth += 3 * Mathf.Clamp01((body.front + 1) / 1.6f);
                        worth -= .5f * Mathf.Max(0, body.peak - 1.5f);
                        return worth;
                    }
                    float total = 0; int stood = 0;
                    IEnumerator All(float[] numbers)
                    {
                        Into(numbers);
                        // (The same chances of pushes for every candidate in a round.)
                        shakeChance = new System.Random(jitterSeed);
                        total = 0; stood = 0;
                        for (int j = 0; j < from.Length; j++)
                        {
                            yield return Try(j, null, "");
                            foreach (var body in bodies) { total += Worth(body); if (body.ended == "stands" || body.ended == "turned") stood++; }
                        }
                        total /= from.Length * bodies.Count;
                    }
                    int tries = from.Length * bodies.Count;
                    var random = new System.Random(seed);
                    float Bell() { double u = 1 - random.NextDouble(), v = random.NextDouble(); return (float)(Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v)); }
                    yield return All((float[])mean.Clone());
                    float best = total; int bestStood = stood;
                    float[] bestNumbers = (float[])mean.Clone();
                    Debug.Log(string.Format(Plain, "RISE_SEARCH begins at {0:0.00} ({1} of {2} stand)", best, bestStood, tries));
                    var clock = System.Diagnostics.Stopwatch.StartNew();
                    var numbersOf = new float[many][];
                    var worths = new float[many];
                    var order = new int[many];
                    for (int round = 0; round < rounds; round++)
                    {
                        jitterSeed = seed * 1000 + round + 1;
                        // (With chances that change from round to round, the best is the best of this round, the one before
                        // tried again among them: a lucky draw once is not kept for ever.)
                        if (jitterTurn > 0 || jitterMove > 0) best = float.NegativeInfinity;
                        for (int n = 0; n < many; n++)
                        {
                            var numbers = new float[all];
                            for (int k = 0; k < all; k++) numbers[k] = mean[k] + wide[k] * Bell();
                            if (n == 0) Array.Copy(bestNumbers, numbers, all);
                            yield return All(numbers);
                            worths[n] = total; numbersOf[n] = numbers; order[n] = n;
                            if (total > best) { best = total; bestStood = stood; Array.Copy(numbers, bestNumbers, all); }
                        }
                        Array.Sort(order, (x, y) => worths[y].CompareTo(worths[x]));
                        for (int k = 0; k < all; k++)
                        {
                            if (k < count && k / Each < first) continue;
                            float middle = 0;
                            for (int n = 0; n < keep; n++) middle += numbersOf[order[n]][k];
                            middle /= keep;
                            float differ = 0;
                            for (int n = 0; n < keep; n++) differ += (numbersOf[order[n]][k] - middle) * (numbersOf[order[n]][k] - middle);
                            differ = Mathf.Sqrt(differ / keep);
                            float least = k >= count ? .02f * (tunes[k - count].most - tunes[k - count].least) : k % Each >= OwnRiser.Turns ? (wide[k] > 0 ? .02f : 0) : 1.5f;
                            mean[k] = Mathf.Lerp(mean[k], middle, .7f);
                            wide[k] = Mathf.Max(least, Mathf.Lerp(wide[k], differ, .7f));
                        }
                        Into((float[])bestNumbers.Clone());
                        Write(ways, Path.Combine(folder, "best.txt"), string.Format(Plain, "found for {0} after {1} rounds: worth {2:0.00}, {3} of {4} stand", string.Join(",", who), round + 1, best, bestStood, tries));
                        Debug.Log(string.Format(Plain, "RISE_SEARCH round {0}: best {1:0.00} ({2} of {3} stand), this round's best {4:0.00}; {5:0.0} s a round; {6}", round + 1, best, bestStood, tries, worths[order[0]], clock.Elapsed.TotalSeconds / (round + 1), Tuned()));
                        yield return null;
                    }
                    Into((float[])bestNumbers.Clone());
                    for (int j = 0; j < from.Length; j++)
                    {
                        yield return Try(j, null, "");
                        foreach (var body in bodies) Debug.Log(Told(body, j));
                    }
                }
            }
            finally
            {
                Physics.simulationMode = mode;
                Time.captureDeltaTime = timeWas;
                OwnBody.FallsBack = fellBack;
                OwnBody.ArmsTimes = armsWere;
                OwnBody.HeadTimes = 1;
                OwnBody.RiseWays = waysWere;
                OwnBody.Told = toldWas;
                foreach (var body in bodies) { if (body.own != null && body.own.Stands) body.own.Drop(); if (body.fall != null) { body.fall.Gives = true; body.fall.GetsUp = true; } }
            }
            yield return null;
        }
    }
}
