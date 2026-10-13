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
            public OwnRiser.Way[] ways;
            public string ended;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        // Ways from a file. A line "way <name> <front least> <front most> gives|turns", then a line for each pose:
        //   name | over atLeast within | trunk head twist | shoulder out elbow | shoulder out elbow |
        //   hip out knee ankle | hip out knee ankle | soles>0.9 bears>0.8 ahead<0.05 keep=0.02
        // (the leading side first; turns from standing, in degrees).
        internal static OwnRiser.Way[] Read(string file)
        {
            var ways = new List<OwnRiser.Way>();
            var stages = new List<OwnRiser.Stage>();
            OwnRiser.Way way = null;
            void Close() { if (way != null) { way.stages = stages.ToArray(); ways.Add(way); stages.Clear(); } }
            foreach (string raw in File.ReadAllLines(file))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                if (line.StartsWith("way "))
                {
                    Close();
                    var w = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    way = new OwnRiser.Way { name = w[1], frontLeast = float.Parse(w[2], Plain), frontMost = float.Parse(w[3], Plain), gives = w.Length > 4 && w[4] == "gives" };
                    continue;
                }
                var f = line.Split('|');
                float[] N(string s) => Array.ConvertAll(s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), x => float.Parse(x, Plain));
                var times = N(f[1]);
                var stage = new OwnRiser.Stage { name = f[0].Trim(), over = times[0], atLeast = times[1], within = times[2] };
                var numbers = new List<float>();
                for (int k = 2; k <= 6; k++) numbers.AddRange(N(f[k]));
                Assert.That(numbers.Count, Is.EqualTo(OwnRiser.Turns), "A pose of " + stage.name + " has " + numbers.Count + " turns.");
                stage.pose = numbers.ToArray();
                var asked = new List<OwnRiser.Asked>();
                if (f.Length > 7)
                    foreach (string word in f[7].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (word.StartsWith("keep=")) { stage.keeps = float.Parse(word.Substring(5), Plain); continue; }
                        // (sit=45,0.5,0.05,100: it sits back at up to so many degrees a second, its hips so much of its knees,
                        // until its weight is no further ahead than that, and no more than so many degrees.)
                        if (word.StartsWith("sit="))
                        {
                            var sits = word.Substring(4).Split(',');
                            stage.sits = float.Parse(sits[0], Plain);
                            if (sits.Length > 1) stage.sitHips = float.Parse(sits[1], Plain);
                            if (sits.Length > 2) stage.sitAhead = float.Parse(sits[2], Plain);
                            if (sits.Length > 3) stage.sitMost = float.Parse(sits[3], Plain);
                            continue;
                        }
                        // (step=lead,0.2,0.4,0.06: that leg's boot put so far ahead of its hip and so far out, lifted so far.)
                        if (word.StartsWith("step="))
                        {
                            var put = word.Substring(5).Split(',');
                            stage.steps = put[0] == "lead" ? 0 : 1;
                            stage.stepAhead = float.Parse(put[1], Plain); stage.stepOut = float.Parse(put[2], Plain);
                            if (put.Length > 3) stage.stepLifts = float.Parse(put[3], Plain);
                            continue;
                        }
                        int cut = word.IndexOfAny(new[] { '<', '>' });
                        var what = (OwnRiser.Asks)Enum.Parse(typeof(OwnRiser.Asks), word.Substring(0, cut), true);
                        float value = float.Parse(word.Substring(cut + 1), Plain);
                        asked.Add(word[cut] == '>' ? new OwnRiser.Asked { what = what, least = value, most = float.PositiveInfinity } : new OwnRiser.Asked { what = what, least = float.NegativeInfinity, most = value });
                    }
                stage.through = asked.ToArray();
                stages.Add(stage);
            }
            Close();
            return ways.ToArray();
        }

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
                    if (!float.IsNaN(stage.sits)) text.Append(string.Format(Plain, " sit={0:0},{1:0.00},{2:0.00},{3:0}", stage.sits, stage.sitHips, stage.sitAhead, stage.sitMost));
                    if (stage.steps >= 0) text.Append(string.Format(Plain, " step={0},{1:0.00},{2:0.00},{3:0.00}", stage.steps == 0 ? "lead" : "other", stage.stepAhead, stage.stepOut, stage.stepLifts));
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
                    string how = asked.Split('@')[0];
                    int on = asked.Contains("@") ? int.Parse(asked.Split('@')[1], Plain) : 0;
                    for (int n = 0; n < bodies.Count; n++)
                    {
                        var body = bodies[n];
                        Put(body.fall, body.standing);
                        Vector3 own = Spot(n, out Vector3 ownAway);
                        Quaternion ownTurn = Quaternion.LookRotation(ownAway);
                        if (on != 0)
                        {
                            Vector3 there = Spot(n + on, out Vector3 thereAway);
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
                        Lay(body.miner, body.starts[j]);
                        body.own.Wanted = true;
                        OwnBody.RiseWays = body.ways;
                        body.done = !body.own.BeginDown();
                        body.up = body.onFeet = false; body.feetAt = body.upAt = -1; body.peak = body.stoodFor = body.risen = body.feetFor = 0; body.ended = body.done ? "could not be made" : "";
                    }
                    Physics.SyncTransforms();
                    int shot = 0;
                    for (int step = 0; step * dt < lasts; step++)
                    {
                        bool going = false;
                        foreach (var body in bodies)
                        {
                            if (body.done) continue;
                            var own = body.own;
                            own.Advance(dt);
                            if (!own.Stands) { body.done = true; body.ended = own.GaveUpGettingUp > 0 && !body.onFeet ? "gave up" : "went down"; continue; }
                            for (int i = OwnBody.Hips; i <= OwnBody.Head; i++) body.peak = Mathf.Max(body.peak, own.Part(i).GetPointVelocity(own.Part(i).worldCenterOfMass).magnitude);
                            if (own.GettingUp)
                            {
                                var riser = own.Riser;
                                body.hipsHigh = riser.Measure(OwnRiser.Asks.Hips); body.ahead = riser.Measure(OwnRiser.Asks.Ahead); body.soles = riser.Measure(OwnRiser.Asks.Soles);
                                body.upright = riser.Measure(OwnRiser.Asks.Upright); body.bears = riser.Measure(OwnRiser.Asks.Bears); body.front = riser.Measure(OwnRiser.Asks.Front);
                                body.ended = riser.WayNow + " " + riser.StageNow;
                            }
                            else if (!body.onFeet) { body.onFeet = true; body.feetAt = step * dt; }
                            if (body.onFeet)
                            {
                                body.feetFor += dt; body.risen = Mathf.Max(body.risen, own.Risen);
                                // (-riseTick 1: what the keeper reckons, every tenth step of the physics.)
                                if (ticks && step % 10 == 0) Debug.Log(string.Format(Plain, "RISE_TICK {0} {1:0.00} s risen {2:0.00} | {3}", body.name, step * dt, own.Risen, own.Tick(Flat(own.Part(OwnBody.Hips).transform.forward).normalized, new Vector3(own.Part(OwnBody.Foot).transform.position.x, body.starts[j].at[0].y - .2f, own.Part(OwnBody.Foot).transform.position.z))));
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
                        Physics.Simulate(dt);
                        if (pictured != null && step % every == 0)
                        {
                            yield return null;
                            if (pictured.own.Stands)
                            {
                                Vector3 hipsAt = pictured.own.Part(OwnBody.Hips).transform.position;
                                Vector3 away = pictured.unit.transform.forward;
                                float tall = 1.3f * pictured.unit.GetComponent<ProceduralBiped>().StandingHipHeight + .5f;
                                Vector3 looksAt = new Vector3(hipsAt.x, pictured.starts[j].at[0].y + .3f * tall - .1f, hipsAt.z);
                                Vector3 right = pictured.starts[j].side;
                                Vector3 fromWhere = looksAt + right * (4.2f * tall) + Vector3.up * (.5f * tall);
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
                    body.name, from[j], body.ended, body.feetAt, body.upAt, body.peak, body.hipsHigh, body.ahead, body.soles, body.upright, body.bears, body.feetFor, body.risen);

                if (string.IsNullOrEmpty(search))
                {
                    for (int j = 0; j < from.Length; j++)
                    {
                        Body pictured = null;
                        foreach (var body in bodies) if (!string.IsNullOrEmpty(shots) && string.Equals(shots, body.name, StringComparison.OrdinalIgnoreCase)) pictured = body;
                        yield return Try(j, pictured, pictured != null ? pictured.name.ToLowerInvariant() + "_" + from[j] + j : "");
                        foreach (var body in bodies) Debug.Log(Told(body, j));
                        yield return null;
                    }
                }
                else
                {
                    // A search of one way's poses: varied towards those that leave the bodies best.
                    int rounds = (int)Number("-riseRounds", 40), many = (int)Number("-riseMany", 32), keep = (int)Number("-riseKeep", 6), seed = (int)Number("-riseSeed", 1), first = (int)Number("-riseFirst", 0);
                    float spread = Number("-riseSpread", 15), hurry = Number("-riseHurry", .05f);
                    int at = -1;
                    for (int i = 0; i < ways.Length; i++) if (ways[i].name == search) at = i;
                    Assert.That(at, Is.GreaterThanOrEqualTo(0), "No way called " + search + " in " + file);
                    var way = ways[at];
                    const int Each = OwnRiser.Turns + 2;
                    int stages = way.stages.Length, count = stages * Each;
                    Vector2 Limits(int k)
                    {
                        switch (k)
                        {
                            case 0: return new Vector2(-25, 90);
                            case 1: return new Vector2(-45, 45);
                            case 2: return new Vector2(-30, 30);
                            case 3: case 6: return new Vector2(-50, 170);
                            case 4: case 7: return new Vector2(-30, 95);
                            case 5: case 8: return new Vector2(0, 145);
                            case 9: case 13: return new Vector2(-20, 125);
                            case 10: case 14: return new Vector2(-20, 50);
                            case 11: case 15: return new Vector2(-25, 125);
                            case 12: case 16: return new Vector2(-45, 45);
                            case OwnRiser.Turns: return new Vector2(.5f, 2.5f);
                            default: return new Vector2(0, 1.5f);
                        }
                    }
                    bool same = Number("-riseSame", 1) > 0;
                    float[] mean = new float[count], wide = new float[count];
                    for (int s = 0; s < stages; s++)
                    {
                        for (int k = 0; k < OwnRiser.Turns; k++) mean[s * Each + k] = way.stages[s].pose[k];
                        mean[s * Each + OwnRiser.Turns] = way.stages[s].over;
                        mean[s * Each + OwnRiser.Turns + 1] = Mathf.Max(0, way.stages[s].atLeast - way.stages[s].over);
                        for (int k = 0; k < Each; k++) wide[s * Each + k] = s < first ? 0 : k >= OwnRiser.Turns ? .2f * spread / 15 : spread;
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
                        }
                    }
                    // How well a body was left.
                    float Worth(Body body)
                    {
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
                        total = 0; stood = 0;
                        for (int j = 0; j < from.Length; j++)
                        {
                            yield return Try(j, null, "");
                            foreach (var body in bodies) { total += Worth(body); if (body.ended == "stands") stood++; }
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
                        for (int n = 0; n < many; n++)
                        {
                            var numbers = new float[count];
                            for (int k = 0; k < count; k++) numbers[k] = mean[k] + wide[k] * Bell();
                            if (n == 0) Array.Copy(bestNumbers, numbers, count);
                            yield return All(numbers);
                            worths[n] = total; numbersOf[n] = numbers; order[n] = n;
                            if (total > best) { best = total; bestStood = stood; Array.Copy(numbers, bestNumbers, count); }
                        }
                        Array.Sort(order, (x, y) => worths[y].CompareTo(worths[x]));
                        for (int k = 0; k < count; k++)
                        {
                            if (k / Each < first) continue;
                            float middle = 0;
                            for (int n = 0; n < keep; n++) middle += numbersOf[order[n]][k];
                            middle /= keep;
                            float differ = 0;
                            for (int n = 0; n < keep; n++) differ += (numbersOf[order[n]][k] - middle) * (numbersOf[order[n]][k] - middle);
                            differ = Mathf.Sqrt(differ / keep);
                            float least = k % Each >= OwnRiser.Turns ? .02f : 1.5f;
                            mean[k] = Mathf.Lerp(mean[k], middle, .7f);
                            wide[k] = Mathf.Max(least, Mathf.Lerp(wide[k], differ, .7f));
                        }
                        Into((float[])bestNumbers.Clone());
                        Write(ways, Path.Combine(folder, "best.txt"), string.Format(Plain, "found for {0} after {1} rounds: worth {2:0.00}, {3} of {4} stand", string.Join(",", who), round + 1, best, bestStood, tries));
                        Debug.Log(string.Format(Plain, "RISE_SEARCH round {0}: best {1:0.00} ({2} of {3} stand), this round's best {4:0.00}; {5:0.0} s a round", round + 1, best, bestStood, tries, worths[order[0]], clock.Elapsed.TotalSeconds / (round + 1)));
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
                OwnBody.FallsBack = fellBack;
                OwnBody.ArmsTimes = armsWere;
                OwnBody.RiseWays = waysWere;
                foreach (var body in bodies) { if (body.own != null && body.own.Stands) body.own.Drop(); if (body.fall != null) { body.fall.Gives = true; body.fall.GetsUp = true; } }
            }
            yield return null;
        }
    }
}
