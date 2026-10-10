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
    // A search for a way of getting up (PhysicalFall's rows of poses). The miners are laid down, or thrown down, a
    // few different ways; from each, a row of poses is tried, the physics stepped by hand (many tries a second,
    // nothing drawn); and the poses are varied towards those that leave the bodies best: squatting on their feet,
    // their weight over them, still. What is found is written out as a file of ways, to be looked at on the bench, in
    // pictures, before it is believed.
    //
    // Run on its own: -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.GetUpSearch -searchOut <folder>
    //   -searchWays <file> -searchWay <name> [-searchFor squat|feet|front] [-searchMiner Small,Long,Round]
    //   [-searchStarts front,back,left,right,s0,s90,s180,s270] [-searchRounds 120] [-searchMany 40] [-searchKeep 8]
    //   [-searchSpread 20] [-searchSeed 1] [-searchSame 0] [-searchFrom 0]
    public sealed class GetUpSearch
    {
        private struct Lies { public string name; public Vector3[] at; public Quaternion[] turned; public float front; }
        private sealed class Body
        {
            public string name;
            public SelectableUnit unit;
            public PhysicalFall fall;
            public Lies standing;
            public List<Lies> starts = new List<Lies>();
            public float peak, settled, kicked;
            public bool lost, done;
        }

        private static readonly CultureInfo Plain = CultureInfo.InvariantCulture;
        // How far each turn of a pose may go (degrees): the trunk's bow, the head's, an arm (shoulder forward, across,
        // elbow), a leg (hip, across, knee), the trunk's twist; then the seconds a pose is come to over, and kept.
        private static readonly Vector2[] Arm = { new Vector2(-50, 170), new Vector2(-95, 95), new Vector2(0, 145) };
        private static readonly Vector2[] Leg = { new Vector2(-20, 120), new Vector2(-45, 45), new Vector2(0, 145) };
        private static Vector2 Limits(int k)
        {
            if (k == 0) return new Vector2(-25, 70);
            if (k == 1) return new Vector2(-40, 45);
            if (k == 14) return new Vector2(-30, 30);
            if (k == 15) return new Vector2(.3f, 1.6f);
            if (k == 16) return new Vector2(0, .8f);
            return k < 8 ? Arm[(k - 2) % 3] : Leg[(k - 8) % 3];
        }
        private const int Each = 17, Count = PhysicalFall.Count;

        internal static void Write(PhysicalFall.Way[] ways, string file, string note)
        {
            var text = new StringBuilder();
            text.AppendLine("# " + note);
            foreach (var way in ways)
            {
                text.AppendLine(string.Format(Plain, "way {0} {1:0.##} {2:0.##} {3}", way.name, way.frontLeast, way.frontMost, way.gives ? "gives" : "turns"));
                foreach (var stage in way.stages)
                {
                    var p = stage.pose;
                    text.Append(string.Format(Plain, "{0} | {1:0.00} {2:0.00} {3:0.0} | {4:0} {5:0} {6:0} | {7:0} {8:0} {9:0} | {10:0} {11:0} {12:0} | {13:0} {14:0} {15:0} | {16:0} {17:0} {18:0} |",
                        stage.name, stage.over, stage.atLeast, stage.within, p[0], p[1], p[14], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11], p[12], p[13]));
                    foreach (var asked in stage.through)
                        text.Append(string.Format(Plain, " {0}{1}{2:0.##}", asked.what.ToString().ToLowerInvariant(), float.IsNegativeInfinity(asked.least) ? "<" : ">", float.IsNegativeInfinity(asked.least) ? asked.most : asked.least));
                    if (!float.IsNaN(stage.keeps)) text.Append(string.Format(Plain, " keep={0:0.00}", stage.keeps));
                    text.AppendLine();
                }
            }
            File.WriteAllText(file, text.ToString());
        }

        private static Lies Now(PhysicalFall fall, string name)
        {
            var lies = new Lies { name = name, at = new Vector3[Count], turned = new Quaternion[Count] };
            for (int i = 0; i < Count; i++) { lies.at[i] = fall.Part(i).transform.position; lies.turned[i] = fall.Part(i).transform.rotation; }
            lies.front = fall.Measure(PhysicalFall.Asks.Front);
            return lies;
        }

        private static void Put(PhysicalFall fall, Lies lies)
        {
            for (int i = 0; i < Count; i++)
            {
                var part = fall.Part(i);
                part.transform.SetPositionAndRotation(lies.at[i], lies.turned[i]);
                part.linearVelocity = Vector3.zero; part.angularVelocity = Vector3.zero;
            }
        }

        [UnityTest, Explicit, Timeout(14400000)]
        public IEnumerator Search()
        {
            string folder = CaptureTools.Argument("-searchOut");
            string file = CaptureTools.Argument("-searchWays");
            string which = CaptureTools.Argument("-searchWay");
            Assert.That(folder, Is.Not.Null.And.Not.Empty, "-searchOut <folder>");
            Assert.That(file, Is.Not.Null.And.Not.Empty, "-searchWays <file>");
            string[] who = (CaptureTools.Argument("-searchMiner") ?? "Small").Split(',');
            string aim = CaptureTools.Argument("-searchFor") ?? "squat";
            string[] from = (CaptureTools.Argument("-searchStarts") ?? "front,back,left,right").Split(',');
            float Number(string argument, float otherwise) { string given = CaptureTools.Argument(argument); return string.IsNullOrEmpty(given) ? otherwise : float.Parse(given, Plain); }
            int rounds = (int)Number("-searchRounds", 120), many = (int)Number("-searchMany", 40), keep = (int)Number("-searchKeep", 8), seed = (int)Number("-searchSeed", 1);
            int first = (int)Number("-searchFrom", 0);
            float spread = Number("-searchSpread", 20);
            bool same = Number("-searchSame", 0) > 0;
            float hurry = Number("-searchHurry", .1f), kicks = Number("-searchKicks", 0);
            Directory.CreateDirectory(folder);
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            var choice = UnityEngine.Object.FindAnyObjectByType<MinerChoice>();
            var ground = UnityEngine.Object.FindAnyObjectByType<OrdinaryGround>();
            // The miners, each at its own place on the path, all there at once.
            var bodies = new List<Body>();
            foreach (string name in who)
            {
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (string.Equals(choice.NameOf(i), name, StringComparison.OrdinalIgnoreCase)) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner called " + name);
                choice.Choose(index);
                yield return null;
                bodies.Add(new Body { name = choice.NameOf(index), unit = choice.Current });
            }
            for (int n = 0; n < bodies.Count; n++)
            {
                var unit = bodies[n].unit;
                unit.gameObject.SetActive(true);
                var a = ground.Path[3 + n];
                var b = ground.Path[4 + n];
                Vector3 spot = new Vector3(a.x, ground.Height(a.x, a.y), a.y);
                Vector3 away = new Vector3(b.x, 0, b.y) - new Vector3(a.x, 0, a.y);
                unit.Motor.Stop();
                unit.GetComponent<NavMeshAgent>().Warp(spot);
                unit.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(away.normalized));
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
            try
            {
                void Step() { foreach (var body in bodies) body.fall.Advance(dt); Physics.Simulate(dt); }

                // The ways they come to lie.
                foreach (var body in bodies) body.standing = Now(body.fall, "standing");
                foreach (string how in from)
                {
                    foreach (var body in bodies)
                    {
                        var fall = body.fall;
                        Put(fall, body.standing);
                        fall.Fell();
                        if (!how.StartsWith("s")) PhysicalFallBench.Lay(fall, body.unit.transform, ground, how);
                    }
                    Physics.SyncTransforms();
                    for (int step = 0; step < 450; step++)
                    {
                        if (step * dt < .3f && how.StartsWith("s"))
                            foreach (var body in bodies)
                                body.fall.Push(Quaternion.AngleAxis(float.Parse(how.Substring(1), Plain), Vector3.up) * body.unit.transform.forward * (6 * body.unit.GetComponent<PhysicalBody>().Mass), body.fall.Part(PhysicalFall.Trunk).worldCenterOfMass);
                        Step();
                    }
                    foreach (var body in bodies)
                    {
                        var lies = Now(body.fall, how);
                        body.starts.Add(lies);
                        Debug.Log(string.Format(Plain, "SEARCH {0} lies ({1}): its chest to the ground by {2:0.00}, its hips {3:0.00} of their height up; state {4}", body.name, how, lies.front, body.fall.Measure(PhysicalFall.Asks.Hips), body.fall.Now));
                    }
                }
                yield return null;

                var ways = PhysicalFallBench.Read(file);
                int at = -1;
                for (int i = 0; i < ways.Length; i++) if (ways[i].name == which) at = i;
                Assert.That(at, Is.GreaterThanOrEqualTo(0), "No way called " + which + " in " + file);
                var way = ways[at];
                int stages = way.stages.Length, size = stages * Each;

                // A row of poses as numbers, and back.
                float[] mean = new float[size], wide = new float[size];
                for (int s = 0; s < stages; s++)
                {
                    for (int k = 0; k < 15; k++) mean[s * Each + k] = way.stages[s].pose[k];
                    mean[s * Each + 15] = way.stages[s].over;
                    mean[s * Each + 16] = Mathf.Max(0, way.stages[s].atLeast - way.stages[s].over);
                    for (int k = 0; k < Each; k++) wide[s * Each + k] = s < first ? 0 : k >= 15 ? .2f * spread / 20 : spread;
                }
                void Into(float[] numbers)
                {
                    for (int s = 0; s < stages; s++)
                    {
                        var stage = way.stages[s];
                        for (int k = 0; k < Each; k++) { var limits = Limits(k); numbers[s * Each + k] = Mathf.Clamp(numbers[s * Each + k], limits.x, limits.y); }
                        if (same && s >= first) for (int k = 0; k < 3; k++) { numbers[s * Each + 5 + k] = numbers[s * Each + 2 + k]; numbers[s * Each + 11 + k] = numbers[s * Each + 8 + k]; }
                        if (same && s >= first) numbers[s * Each + 14] = 0;
                        for (int k = 0; k < 15; k++) stage.pose[k] = numbers[s * Each + k];
                        stage.over = numbers[s * Each + 15];
                        stage.atLeast = stage.over + numbers[s * Each + 16];
                        stage.within = stage.atLeast + 3;
                        stage.through = new PhysicalFall.Asked[0];
                    }
                }

                // How well a body is left, and whether that is well enough.
                float Worth(Body body, Lies lies, out bool well, StringBuilder says)
                {
                    var fall = body.fall;
                    float front = fall.Measure(PhysicalFall.Asks.Front), high = fall.Measure(PhysicalFall.Asks.Hips), off = fall.Measure(PhysicalFall.Asks.Off);
                    float ahead = fall.Measure(PhysicalFall.Asks.Ahead), steep = fall.Measure(PhysicalFall.Asks.Steep), soles = fall.Measure(PhysicalFall.Asks.Soles), upright = fall.Measure(PhysicalFall.Asks.Upright);
                    float moving = 0, head = fall.Measure(PhysicalFall.Asks.Head);
                    for (int i = PhysicalFall.Hips; i <= PhysicalFall.Head; i++) moving = Mathf.Max(moving, fall.Part(i).linearVelocity.magnitude);
                    bool there = !body.lost && fall.Through;
                    float worth;
                    if (aim == "kneel")
                    {
                        // On its knees: its chest to the ground, its shins not up in the air behind it, its feet near
                        // under its hips.
                        worth = Mathf.Clamp(front, -1, .8f) + 2 * Mathf.Clamp(steep, -1, .1f) - Mathf.Min(off, 1) + Mathf.Clamp(high, 0, .5f);
                        well = there && front >= .5f && steep >= -.2f && off <= .6f && moving < .15f;
                    }
                    else if (aim == "crouch")
                    {
                        // Its feet under it and its weight over them, its chest still to the ground.
                        float over = soles > .3f ? Mathf.Exp(-Mathf.Pow(ahead / .15f, 2)) : 0;
                        worth = Mathf.Max(-.5f, steep) + Mathf.Max(-.5f, soles) + over - .5f * Mathf.Min(off, 1) + Mathf.Clamp(front, -1, .5f) + Mathf.Clamp(high, 0, .6f);
                        well = there && soles >= .6f && steep >= .6f && Mathf.Abs(ahead) <= .15f && front >= .3f && moving < .15f;
                    }
                    else if (aim == "seat")
                    {
                        // Sat back on its seat and its feet: its soles flat, its shins standing, its trunk up off its
                        // knees and its head up off the ground.
                        worth = Mathf.Max(-.5f, steep) + Mathf.Max(-.5f, soles) + 1.5f * Mathf.Clamp01((upright + .2f) / .7f) + Mathf.Clamp01((head - .3f) / .5f) - .5f * Mathf.Min(off, 1);
                        well = there && soles >= .85f && steep >= .8f && upright >= .2f && moving < .15f;
                    }
                    else if (aim == "rise")
                    {
                        // Up on its feet: its head up off the ground, its soles flat and its shins standing, its weight
                        // over its feet, its hips as high as they come.
                        float over = soles > .5f ? Mathf.Exp(-Mathf.Pow((ahead - .05f) / .15f, 2)) : 0;
                        worth = 2 * Mathf.Clamp01((head - .3f) / .7f) + Mathf.Max(-.5f, steep) + Mathf.Max(-.5f, soles) + over - .5f * Mathf.Min(off, 1) + 1.5f * Mathf.Clamp(high, 0, .75f);
                        well = there && soles >= .85f && steep >= .8f && Mathf.Abs(ahead - .05f) <= .15f && head >= .8f && moving < .15f;
                    }
                    else
                    if (aim == "feet")
                    {
                        float over = soles > .5f ? Mathf.Exp(-Mathf.Pow((ahead - .1f) / .15f, 2)) : 0;
                        worth = 3 * Mathf.Clamp(high, 0, .75f) + Mathf.Max(0, steep) + Mathf.Max(0, soles) - Mathf.Min(off, 1) + over + .5f * Mathf.Clamp01(upright * 2);
                        well = there && high >= .45f && soles >= .8f && steep >= .7f && Mathf.Abs(ahead - .1f) <= .15f && moving < .2f;
                    }
                    else if (aim == "squat")
                    {
                        // On its seat and its feet, its soles flat and its shins standing, its weight by its feet, its trunk
                        // not hanging head down.
                        float over = soles > .5f ? Mathf.Exp(-Mathf.Pow(ahead / .15f, 2)) : 0;
                        worth = Mathf.Max(-.5f, steep) + Mathf.Max(-.5f, soles) + over - .5f * Mathf.Min(off, 1) + Mathf.Clamp01((upright + .2f) / .4f);
                        well = there && soles >= .85f && steep >= .8f && Mathf.Abs(ahead) <= .15f && upright >= 0 && moving < .15f;
                    }
                    else
                    {
                        worth = front;
                        well = there && front >= .6f && moving < .2f;
                    }
                    worth -= .5f * Mathf.Max(0, body.peak - 2) + Mathf.Min(1, moving) + kicks * body.kicked;
                    if (body.lost) worth -= 2;
                    if (well) worth += 1;
                    if (says != null)
                        says.Append(string.Format(Plain, "\n    {0} {1}: front {2:0.00} hips {3:0.00} off {4:0.00} ahead {5:0.00} steep {6:0.00} soles {7:0.00} upright {8:0.00} head {12:0.00} fastest {9:0.0}{10}{11}",
                            body.name, lies.name, front, high, off, ahead, steep, soles, upright, body.peak, body.lost ? " LOST" : "", well ? " WELL" : "", head));
                    return worth;
                }
                float All(float[] numbers, out int wells, StringBuilder says = null)
                {
                    Into(numbers);
                    float sum = 0;
                    wells = 0;
                    for (int j = 0; j < from.Length; j++)
                    {
                        foreach (var body in bodies)
                        {
                            Put(body.fall, body.starts[j]);
                            body.fall.Ways = ways;
                            body.fall.LieStill();
                            body.fall.Rouse();
                            body.peak = 0; body.settled = 0; body.kicked = 0;
                            body.lost = body.fall.Now != PhysicalFall.State.Gathering;
                            body.done = body.lost;
                        }
                        Physics.SyncTransforms();
                        bool going = true;
                        for (int step = 0; step < 900 && going; step++)
                        {
                            Step();
                            going = false;
                            foreach (var body in bodies)
                            {
                                if (body.done) continue;
                                var fall = body.fall;
                                if (fall.Now != PhysicalFall.State.Gathering) { body.lost = body.done = true; fall.LieStill(); continue; }
                                for (int i = PhysicalFall.Hips; i <= PhysicalFall.Head; i++) body.peak = Mathf.Max(body.peak, fall.Part(i).linearVelocity.magnitude);
                                // (In the way that is looked for: how far its feet are thrown up behind it.)
                                if (fall.WayNow == which) body.kicked = Mathf.Max(body.kicked, -fall.Measure(PhysicalFall.Asks.Steep) - .3f);
                                if (fall.Through) body.settled += dt;
                                if (body.settled >= .6f) body.done = true; else going = true;
                            }
                        }
                        foreach (var body in bodies)
                        {
                            sum += Worth(body, body.starts[j], out bool well, says);
                            if (well) wells++;
                        }
                    }
                    // (Time counts against it: so much for each second the whole way takes.)
                    float takes = 0;
                    foreach (var stage in way.stages) takes += stage.atLeast;
                    return sum / (from.Length * bodies.Count) - hurry * takes;
                }

                int tries = from.Length * bodies.Count;
                var random = new System.Random(seed);
                float Bell() { double u = 1 - random.NextDouble(), v = random.NextDouble(); return (float)(Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v)); }
                var told = new StringBuilder();
                float best = All((float[])mean.Clone(), out int bestWells, told);
                float[] bestNumbers = (float[])mean.Clone();
                Debug.Log(string.Format(Plain, "SEARCH begins at {0:0.00} ({1} of {2} well){3}", best, bestWells, tries, told));
                var clock = System.Diagnostics.Stopwatch.StartNew();
                int tried = 0;
                var numbersOf = new float[many][];
                var worths = new float[many];
                var order = new int[many];
                for (int round = 0; round < rounds; round++)
                {
                    for (int n = 0; n < many; n++)
                    {
                        var numbers = new float[size];
                        for (int k = 0; k < size; k++) numbers[k] = mean[k] + wide[k] * Bell();
                        if (n == 0) Array.Copy(bestNumbers, numbers, size);
                        worths[n] = All(numbers, out int wells);
                        numbersOf[n] = numbers; order[n] = n;
                        tried += tries;
                        if (worths[n] > best) { best = worths[n]; bestWells = wells; Array.Copy(numbers, bestNumbers, size); }
                    }
                    Array.Sort(order, (x, y) => worths[y].CompareTo(worths[x]));
                    // The poses go towards the best of this round; how widely they are varied, towards how those differ.
                    for (int k = 0; k < size; k++)
                    {
                        if (k / Each < first) continue;
                        float middle = 0;
                        for (int n = 0; n < keep; n++) middle += numbersOf[order[n]][k];
                        middle /= keep;
                        float differ = 0;
                        for (int n = 0; n < keep; n++) differ += (numbersOf[order[n]][k] - middle) * (numbersOf[order[n]][k] - middle);
                        differ = Mathf.Sqrt(differ / keep);
                        float least = k % Each >= 15 ? .02f : 2;
                        mean[k] = Mathf.Lerp(mean[k], middle, .7f);
                        wide[k] = Mathf.Max(least, Mathf.Lerp(wide[k], differ, .7f));
                    }
                    if (round % 5 == 4 || round == rounds - 1)
                    {
                        told.Clear();
                        All((float[])bestNumbers.Clone(), out int wells, told);
                        Write(ways, Path.Combine(folder, "best.txt"), string.Format(Plain, "found for {0} after {1} rounds: worth {2:0.00}, {3} of {4} well", string.Join(",", who), round + 1, best, wells, tries));
                        Debug.Log(string.Format(Plain, "SEARCH round {0}: best {1:0.00} ({2} of {3} well), this round's best {4:0.00}; {5:0} tries a second{6}",
                            round + 1, best, wells, tries, worths[order[0]], tried / Math.Max(.001, clock.Elapsed.TotalSeconds), told));
                        yield return null;
                    }
                }
            }
            finally
            {
                Physics.simulationMode = mode;
                foreach (var body in bodies) { body.fall.Gives = true; body.fall.GetsUp = true; }
            }
            foreach (var body in bodies) body.fall.TakeBack();
            yield return null;
        }
    }
}
