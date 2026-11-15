using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace WonderGather.Tests
{
    // (c)(c), step (b) (Docs/Research/2026-11-13_DetachedLeg/courses_plan.md, November 14, 2026): the game's own falls and
    // get-ups, the trousers' cut ring checked on the very frames as played, both ways. A course is one shove (PushOver,
    // as GettingUpByItsOwnTests), then the whole getting up, to standing or the cap, then taken back. Each measured
    // frame, after everything is drawn, the shared CoverMeter measures the ring as played; then the skirt's flaps are
    // laid the other way (MinerBody.FlapsOnTheirSeams not as played) from that way's own copy of Drape's only carried
    // state (flapHangs), measured again, and the played flaps put back exactly (CoverPair). With the other way equal to
    // the played way, the two measures must agree on every frame and the played flap bones come back bit for bit: the
    // pair's own check (SELF_CHECK), before any result counts. A measure, not a test that must pass (but for that check).
    //
    // Arguments: -liveWho Small|Round; -liveOwn 0|1 (PhysicalFall.GetsUpByItsOwn); -liveSeam 0|1 (the played way);
    // -liveOther 0|1 (the other way; the opposite of -liveSeam if left out); -ownPushes, -ownPushWays, -ownPushBack (as
    // GettingUpByItsOwnTests reads them); -ownRate (frames a second, 50 if left out); -liveCap seconds (240); -liveEvery
    // (1: every frame while down or getting up); -liveStandEvery (10); -liveOut folder; -livePics (4 a criterion a
    // course). Writes frames.csv (one row a measured frame), ways.csv (each course's 25 ways), events.txt, pictures.csv,
    // meter.txt and the pictures; per course FrameRecorder's course_<k>.bin/.txt. Read by
    // Docs/Technical/AnyBody/tools/live_cover_summary.py.
    public sealed class CoverRingLiveTests
    {
        private MinerChoice choice;
        private MinerWorkPreview look;
        private CoverMeter meter;
        private CoverPair pair;
        private FrameRecorder recorder;
        private Camera pictures;
        private readonly List<LODGroup> lods = new List<LODGroup>();
        private int rateWas;
        private bool ownWas, seamsWas, backfacesWas;
        private Camera grassWas;
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        [UnitySetUp]
        public IEnumerator Load()
        {
            rateWas = Time.captureFramerate;
            ownWas = PhysicalFall.GetsUpByItsOwn;
            seamsWas = MinerBody.FlapsOnTheirSeams;
            backfacesWas = Physics.queriesHitBackfaces;
            grassWas = GrassField.NotFor;
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
        }

        // Every global as it was: the pair first (it restores its skins' settings and closes its files), then the
        // recorder, the meter's layers and solids, the levels of detail, the switches and the frame rate.
        [TearDown]
        public void Restore()
        {
            if (pair != null) { pair.Close(); Object.Destroy(pair.gameObject); pair = null; }
            if (recorder != null) { recorder.Close(); Object.Destroy(recorder.gameObject); recorder = null; }
            if (meter != null) { meter.End(); meter = null; }
            foreach (var g in lods) if (g != null) g.ForceLOD(-1);
            lods.Clear();
            if (pictures != null) { Object.Destroy(pictures.gameObject); pictures = null; }
            GrassField.NotFor = grassWas;
            Time.captureFramerate = rateWas;
            PhysicalFall.GetsUpByItsOwn = ownWas;
            MinerBody.FlapsOnTheirSeams = seamsWas;
            Physics.queriesHitBackfaces = backfacesWas;
        }

        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        private static float Given(string name, float otherwise)
        {
            string a = CaptureTools.Argument(name);
            return string.IsNullOrEmpty(a) ? otherwise : float.Parse(a, Inv);
        }

        [UnityTest, Explicit, Timeout(14400000)]
        public IEnumerator TheCutRingAsPlayed()
        {
            string who = CaptureTools.Argument("-liveWho");
            if (string.IsNullOrEmpty(who)) who = "Small";
            bool ownWay = CaptureTools.Argument("-liveOwn") != "0";
            bool seam = CaptureTools.Argument("-liveSeam") == "1";
            string otherArg = CaptureTools.Argument("-liveOther");
            bool other = string.IsNullOrEmpty(otherArg) ? !seam : otherArg == "1";
            string rate = CaptureTools.Argument("-ownRate");
            Time.captureFramerate = string.IsNullOrEmpty(rate) ? 50 : int.Parse(rate, Inv);
            float cap = Given("-liveCap", 240);
            int every = Mathf.Max(1, (int)Given("-liveEvery", 1)), standEvery = Mathf.Max(1, (int)Given("-liveStandEvery", 10));
            int picsMost = Mathf.Max(0, (int)Given("-livePics", 4));
            // (-liveEnergy 1: the energy count of its own body every step, its columns in frames.csv and the steps before a
            // link whirls; 0, as before. The guard against a body gone bad is on either way.)
            bool energy = CaptureTools.Argument("-liveEnergy") == "1";
            string folder = CaptureTools.Argument("-liveOut");
            if (string.IsNullOrEmpty(folder))
                folder = Path.Combine(Application.dataPath, "..", "TestRuns", "CoverLive", $"{who}_own{(ownWay ? 1 : 0)}_seam{(seam ? 1 : 0)}_other{(other ? 1 : 0)}");
            folder = Path.GetFullPath(folder);
            Directory.CreateDirectory(folder);
            Directory.CreateDirectory(Path.Combine(folder, "pictures"));
            PhysicalFall.GetsUpByItsOwn = ownWay;
            MinerBody.FlapsOnTheirSeams = seam;
            // (A measure, not a test that must pass: what the engine says of a body gone wild is told, and it goes on.)
            LogAssert.ignoreFailingMessages = true;
            Debug.Log($"LIVE_RUN {who}, gets up by its own body {ownWay}, played way (flaps on their seams) {seam}, other way {other}, {Time.captureFramerate} frames a second, cap {cap.ToString(Inv)} s, every {every} down, every {standEvery} standing, energy counted {energy}, out {folder}");

            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == who) index = i;
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner called " + who + ".");
            choice.Choose(index);
            yield return Wait(.3f);
            var unit = choice.Current;
            look.SetOwn(true);
            OwnBody own = null;
            for (float until = Time.time + 20; (own == null || !own.Stands) && Time.time < until;) { unit.TryGetComponent(out own); yield return null; }
            Assert.That(own != null && own.Stands, Is.True, who + " did not come to stand by its own joints.");
            if (who == "Small" && ownWay) Assert.That(own.KnowsAWayUp, Is.True, "Small knows no way up (Resources/RiseWays/Small.txt).");

            // The meter, once, the miner standing at ease before the first shove: the garments' solids on a layer of
            // their own, the terrain's layer as cover, the first level of detail, the cut ring.
            for (float until = Time.time + 15; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
            yield return Wait(1);
            var miner = unit.GetComponent<MinerBody>();
            meter = new CoverMeter();
            meter.TakeLayer();
            meter.AddToMask(6);
            var log = new StringBuilder();
            log.AppendLine($"COVER_SWITCH played way (MinerBody.FlapsOnTheirSeams) {seam}, the other way {other}; cover: the garments' layer {meter.Layer} and the terrain's layer 6 ({LayerMask.LayerToName(6)})");
            lods.AddRange(unit.GetComponentsInChildren<LODGroup>(true));
            meter.AddSkins(unit, log);
            var ring = meter.FindRing(unit.transform, log);
            File.WriteAllText(Path.Combine(folder, "meter.txt"), log.ToString());
            Debug.Log(log.ToString());

            // The pictures' camera: the game camera's look, the meadow not drawn for it (so that a body lying in it is
            // seen; grass is not counted as cover either).
            pictures = new GameObject("Cover pictures").AddComponent<Camera>();
            pictures.CopyFrom(Camera.main);
            pictures.nearClipPlane = .02f; pictures.fieldOfView = 20;
            pictures.enabled = false;
            GrassField.NotFor = pictures;

            pair = new GameObject("Cover pair").AddComponent<CoverPair>();
            pair.Energy = energy;
            pair.Begin(meter, ring, miner, unit.transform, seam, other, every, standEvery, folder, pictures, picsMost, who);
            pair.SetCourse(-1, 0, Quaternion.LookRotation(Vector3.ProjectOnPlane(unit.transform.forward, Vector3.up)));

            float pushes = 8;
            string given = CaptureTools.Argument("-ownPushes");
            if (!string.IsNullOrEmpty(given)) pushes = float.Parse(given, Inv);
            int byItsOwn = 0, oldWay = 0, never = 0, triesAll = 0;
            for (int k = 0; k < pushes; k++)
            {
                // (Standing by its own joints before each push.)
                for (float until = Time.time + 15; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                yield return Wait(1.5f);
                var fall = unit.GetComponent<PhysicalFall>();
                // (Gone bad between the courses, standing: taken back before the push, as at the cap.)
                if (pair.WentBad)
                {
                    pair.WentBad = false;
                    if (own.Stands) own.Drop();
                    if (fall != null) fall.TakeBack();
                    pair.Event("taken back");
                    for (float until = Time.time + 20; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                    yield return Wait(1.5f);
                    fall = unit.GetComponent<PhysicalFall>();
                }
                int roseBefore = fall != null ? fall.RoseByItsOwn : 0, feetBefore = own.OnItsFeetFromLying, oldBefore = fall != null ? fall.GotUp : 0;
                // (As GettingUpByItsOwnTests: forward, or -ownPushBack 1 back, or -ownPushWays 0,180,90,270 in turn; each
                // round of ways turned a little aside.)
                bool back = CaptureTools.Argument("-ownPushBack") == "1";
                float round = back ? 180 : 0;
                string ways = CaptureTools.Argument("-ownPushWays");
                if (!string.IsNullOrEmpty(ways))
                {
                    var each = ways.Split(',');
                    round = float.Parse(each[k % each.Length], Inv);
                }
                float turned = round + ((k / Mathf.Max(1, string.IsNullOrEmpty(ways) ? 1 : ways.Split(',').Length)) % 3 - 1) * 20;
                Vector3 way = Quaternion.AngleAxis(turned, Vector3.up) * unit.transform.forward;
                for (float until = Time.time + 20; !look.CanPushOver && Time.time < until;) yield return null;
                Assert.That(look.CanPushOver, Is.True, who + " cannot be pushed over (push " + k + ").");
                // The course: its own recorder (FrameRecorder, every frame as played) and its ways, fixed to the world now.
                if (recorder != null) { recorder.Close(); Object.Destroy(recorder.gameObject); }
                recorder = FrameRecorder.On(unit.gameObject, Path.Combine(folder, $"course_{k:D2}"));
                recorder.Note($"push {k} {round.ToString(Inv)} turned {turned.ToString(Inv)}");
                pair.Recorder = recorder;
                pair.SetCourse(k, turned, Quaternion.LookRotation(Vector3.ProjectOnPlane(unit.transform.forward, Vector3.up)));
                look.PushOver(way);
                fall = unit.GetComponent<PhysicalFall>();
                Assert.That(fall, Is.Not.Null, who + " has no fall after it was pushed over.");
                pair.CourseBegan(fall, roseBefore);
                float began = Time.time;
                bool lay = false;
                bool Up() => fall.Now == PhysicalFall.State.Up && (own.Stands ? !own.GettingUp && !own.Rising : true);
                // (A body gone bad ends its course at once, as the cap does: CoverPair's guard.)
                while (!pair.WentBad && (Time.time - began < 2 || (!Up() && Time.time - began < cap)))
                {
                    lay |= fall.Now == PhysicalFall.State.Lying;
                    yield return null;
                }
                bool wentBad = pair.WentBad;
                pair.WentBad = false;
                int tries = fall.RoseByItsOwn - roseBefore, feet = own.OnItsFeetFromLying - feetBefore;
                bool itsOwn = !wentBad && feet > 0 && own.Stands && Up();
                bool old = !wentBad && !itsOwn && fall.GotUp > oldBefore;
                if (itsOwn) byItsOwn++; else if (old) oldWay++; else never++;
                triesAll += tries;
                string said = string.Format(Inv, "LIVE_COURSE {0} ({1:0} degrees): it lay {2}; tries by its own body {3}; {4} after {5:0.0} s; the fall said: {6}",
                    k, turned, lay, tries, itsOwn ? "up by its own body" : old ? "up the old way" : wentBad ? "gone bad (LIVE_BLOWUP)" : "not up", Time.time - began, fall.ByItsOwnSaid);
                Debug.Log(said);
                pair.Event(said);
                // (Not up at the cap, or gone bad: the posed body takes it back where it lies and stands, so that the
                // courses go on, as OwnSolidAgainstOwnBodyTests does.)
                if (wentBad || !Up())
                {
                    if (own.Stands) own.Drop();
                    fall.TakeBack();
                    pair.Event("taken back");
                    for (float until = Time.time + 20; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                }
            }
            if (recorder != null) { recorder.Close(); Object.Destroy(recorder.gameObject); recorder = null; }
            pair.Recorder = null;
            pair.Finish();
            Debug.Log($"LIVE_DONE {who}, {pushes} courses: up by its own body {byItsOwn}, the old way {oldWay}, not up {never}; tries by its own body {triesAll}; {pair.Measured} frames measured; out {folder}");
            Debug.Log(pair.TimingLine());
            Debug.Log(pair.PairLine());
            if (seam == other) Debug.Log(pair.SelfCheckLine());
            foreach (var line in pair.FirstDifferences) Debug.Log("LIVE_DIFFERENCE " + line);
            Debug.Log(pair.SkinLine());
            Debug.Log($"LIVE_METER hits off the plane of the face they were counted on: {meter.Mismatches} of {meter.FaceChecks}");
            Debug.Log(pair.EnergyLine());
            Assert.That(pair.Blowups, Is.EqualTo(0), $"{pair.Blowups} blow-up(s): its own body went bad (a link not finite, turning past {CoverPair.BadTurning} rad/s, or its root {CoverPair.BadAway} m from where the course began):" + pair.BlowupsSaid);
            Assert.That(pair.Measured, Is.GreaterThan(0), "No frame was measured.");
            Assert.That(pair.NotRestored, Is.EqualTo(0), "The played flaps did not come back bit for bit.");
            if (seam == other) Assert.That(pair.Differing, Is.EqualTo(0), "The two ways, set equal, measured differently:\n" + string.Join("\n", pair.FirstDifferences));
            Assert.That(meter.Mismatches, Is.EqualTo(0), "Hits off the plane of the face they were counted on.");
        }
    }

    // The paired measure, the last thing of each frame (after FrameRecorder, 30000): as played, then the other way laid
    // from its own state, measured, and the played way put back exactly. Also the state, the drawn hips and knees, the one
    // body, FrameRecorder's movement measures, a row a measured frame, and the pictures.
    [DefaultExecutionOrder(30001)]
    internal sealed class CoverPair : MonoBehaviour
    {
        private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private const int Tile = 480;
        private const float Distance = 3.2f;
        private const int Spacing = 50;
        // The game camera's ways among the meter's 25: 47 and 62 degrees down, and straight above.
        private const int GameFrom = 8;
        private static readonly string[] Criteria = { "ring_game", "ring_all", "face", "hip", "knee", "motion", "bodies" };

        public FrameRecorder Recorder;
        public int Measured, Differing, Laid, NotRestored;
        public readonly List<string> FirstDifferences = new List<string>();

        // The energy count (-liveEnergy 1) and the guard of the test against a body gone bad (on either way): both read
        // its own body after each step of the physics, and write nothing to it (the blow-up's trace, November 15).
        public bool Energy;
        // Gone bad: any link of its own body not finite, turning past BadTurning rad/s, or its root BadAway metres from
        // where the course began. Then LIVE_BLOWUP in events.txt, the course ended as at the cap (the test reads WentBad
        // and clears it), and the test fails at the end with how many.
        public const float BadTurning = 100, BadAway = 50, TraceTurning = 30;
        public bool WentBad;
        public int Blowups;
        public string BlowupsSaid = "";
        private readonly LiveEnergy energy = new LiveEnergy();
        private Vector3 courseFrom;
        private bool courseFromKnown;
        private long tracedTry = long.MinValue;
        private int traces, energySteps, energyStepsAll;
        // (The count since the last row, as the bar-6 columns: the most held, the step of the largest gain with what was
        // given on it, the gain summed, the fastest turning link.)
        private float eHeldMost = float.NaN, eGainMost = float.NaN, eGivenAtMost = float.NaN, eGainSum, fastestMost = float.NaN;
        private string fastestWhich = "";
        private float eGainMostAll = float.NaN, eGainLeastAll = float.NaN, fastestAll = float.NaN;
        private static readonly string[] OwnNames = { "Hips", "Trunk", "Head", "UpperArmL", "UpperArmR", "ForearmL", "ForearmR", "ThighL", "ThighR", "ShinL", "ShinR", "FootL", "FootR" };
        private readonly ArticulationBody[] ownKnown = new ArticulationBody[OwnBody.Count];

        private CoverMeter meter;
        private CoverMeter.Ring ring;
        private MinerBody miner;
        private Transform unit;
        private OwnBody own;
        private PhysicalBalance balance;
        private ProceduralBiped biped;
        private PhysicalFall fall;
        private bool seamPlayed, seamOther;
        private int every, standEvery, picsMost;
        private string folder, who;
        private Camera pictures;
        private StreamWriter rows, events, waysOut, picturesOut;

        private FieldInfo hangsField, thighFrontField, pelvisRestField;
        private MethodInfo drape;
        private MinerBody.Flap[] flaps;
        private Vector3 acrossAt, forwardAt;
        private float ankleUp;
        private Vector3[] otherHangs, playedWas, snapshot, before, replay;
        private int beforeFrame = -10;
        public int Relaid;
        public float NotRestoredMost;
        private Transform[] under;
        private Vector3[] underAt;
        private Quaternion[] underTurned, flapLocal;

        private int course = -1, frameInCourse, sinceRow = 1 << 20, roseBefore;
        private float round, courseStart;
        private string stateWas = "";

        // FrameRecorder's movement measures, the most since the last row (a standing row stands for its frames).
        private readonly Quaternion[] limbWas = new Quaternion[8];
        private readonly Vector3[] endWas = new Vector3[4];
        private bool hadLimbs;
        private float spinMost, jumpMost, whipOwnMost, whipFallMost;
        // The hip's reading checked (November 14, the review): the drawn reading as read (in (-180, 180]) before it is
        // taken the nearest whole turn to the last frame's; the fold as one angle; the own body's links read the same way.
        private readonly float[] flexWas = { float.NaN, float.NaN }, twistWas = { float.NaN, float.NaN };
        private readonly float[] linkFlexWas = { float.NaN, float.NaN }, linkTwistWas = { float.NaN, float.NaN };
        private readonly DrawnHip.Read[] hipsRaw = new DrawnHip.Read[2], hipsLink = new DrawnHip.Read[2];
        private readonly bool[] hasLink = new bool[2];
        private readonly float[] foldDrawn = new float[2], foldLink = new float[2];
        private string spinWhich = "", jumpWhich = "";

        // The criteria's pictures in this course.
        private readonly float[] pictured = new float[Criteria.Length];
        private readonly int[] picturedAt = new int[Criteria.Length], picturedCount = new int[Criteria.Length];

        // The pair's cost: the first 200 measured frames.
        private readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        private int timed;
        private double timedSum, timedMost;
        private int skinChecks, skinStale;
        private float skinBeforeMost, skinAfterMost;
        private readonly List<SkinnedMeshRenderer> skins = new List<SkinnedMeshRenderer>();
        private bool[] skinsForcedWas;

        private struct Faces
        {
            public bool[] Has, Agrees;
            public float[] FrontOn, BackOn;
            // How the thigh's front was taken (the knee's bend plane, or the drawing's pole), and the degrees between the
            // two where the knee is bent enough to tell (NaN where it is not).
            public bool[] ByBend;
            public float[] Split;
        }

        public void Begin(CoverMeter meter, CoverMeter.Ring ring, MinerBody miner, Transform unit, bool seamPlayed, bool seamOther, int every, int standEvery, string folder, Camera pictures, int picsMost, string who)
        {
            this.meter = meter; this.ring = ring; this.miner = miner; this.unit = unit;
            this.seamPlayed = seamPlayed; this.seamOther = seamOther; this.every = every; this.standEvery = standEvery;
            this.folder = folder; this.pictures = pictures; this.picsMost = picsMost; this.who = who;
            own = unit.GetComponent<OwnBody>();
            balance = unit.GetComponent<PhysicalBalance>();
            biped = unit.GetComponent<ProceduralBiped>();
            hangsField = Field("flapHangs");
            thighFrontField = Field("thighFront");
            pelvisRestField = Field("pelvisRest");
            drape = typeof(MinerBody).GetMethod("Drape", Hidden);
            Assert.That(drape, Is.Not.Null, "MinerBody has no Drape.");
            flaps = (MinerBody.Flap[])Field("flaps").GetValue(miner);
            acrossAt = (Vector3)Field("pelvisAcross").GetValue(miner);
            forwardAt = (Vector3)Field("pelvisForward").GetValue(miner);
            ankleUp = (float)Field("ankleUp").GetValue(miner);
            var all = new List<Transform>();
            flapLocal = new Quaternion[flaps.Length];
            foreach (var f in flaps) if (f.bone != null) all.AddRange(f.bone.GetComponentsInChildren<Transform>(true));
            under = all.ToArray();
            underAt = new Vector3[under.Length];
            underTurned = new Quaternion[under.Length];
            foreach (var c in meter.Covers) skins.Add(c.Skin);
            skinsForcedWas = new bool[skins.Count];
            for (int i = 0; i < skins.Count; i++) skinsForcedWas[i] = skins[i].forceMatrixRecalculationPerRender;

            rows = new StreamWriter(Path.Combine(folder, "frames.csv"));
            events = new StreamWriter(Path.Combine(folder, "events.txt"));
            waysOut = new StreamWriter(Path.Combine(folder, "ways.csv"));
            picturesOut = new StreamWriter(Path.Combine(folder, "pictures.csv"));
            waysOut.WriteLine("course,way,elevation,x,y,z");
            picturesOut.WriteLine("course,frame,criteria,values,way,which,view,file,skin_before_mm,skin_after_mm");
            rows.WriteLine(Header(Energy));
            events.WriteLine($"EVENTS {who}: played way {(seamPlayed ? "on" : "off")}, other way {(seamOther ? "on" : "off")}; {flaps.Length} flaps, {under.Length} flap bones and what they carry; ring {ring.Legs[0].Count}+{ring.Legs[1].Count} points");
            // (Its weight lifted is reckoned from where the miner stood as the run began.)
            energy.Level = unit.position.y;
            StartCoroutine(AfterEachStep());
        }

        // Last thing before each step of the physics (after every script has pushed on its own body): the count's start.
        private void FixedUpdate()
        {
            if (!Energy || own == null || !own.Stands) { energy.Forget(); return; }
            if (TakeOwn()) energy.Before(Time.fixedDeltaTime);
        }

        private bool TakeOwn()
        {
            for (int i = 0; i < OwnBody.Count; i++) ownKnown[i] = own.Part(i);
            return energy.Take(own.Part(OwnBody.Hips), ownKnown, OwnNames);
        }

        // After each step of the physics: the count's end (-liveEnergy 1), then the guard.
        private IEnumerator AfterEachStep()
        {
            var step = new WaitForFixedUpdate();
            while (true)
            {
                yield return step;
                if (own == null || !own.Stands || !TakeOwn()) { energy.Forget(); continue; }
                if (Energy)
                {
                    string label = Group() + (own.GettingUp && own.Riser != null ? ":" + own.Riser.StageNow : "") + (own.Rising ? ":rising" : "") + (fall != null ? " try " + (fall.RoseByItsOwn - roseBefore).ToString(Inv) : "");
                    if (energy.After(Time.fixedDeltaTime, Time.frameCount, Time.time, label)) Counted();
                }
                Guard();
            }
        }

        private void Counted()
        {
            energySteps++; energyStepsAll++;
            float held = energy.Held, gain = energy.Gain;
            if (float.IsNaN(eHeldMost) || held > eHeldMost) eHeldMost = held;
            if (float.IsNaN(eGainMost) || gain > eGainMost) { eGainMost = gain; eGivenAtMost = energy.Given; }
            eGainSum += gain;
            if (float.IsNaN(fastestMost) || energy.Fastest > fastestMost) { fastestMost = energy.Fastest; fastestWhich = energy.FastestName; }
            if (float.IsNaN(eGainMostAll) || gain > eGainMostAll) eGainMostAll = gain;
            if (float.IsNaN(eGainLeastAll) || gain < eGainLeastAll) eGainLeastAll = gain;
            if (float.IsNaN(fastestAll) || energy.Fastest > fastestAll) fastestAll = energy.Fastest;
            // (The first time in a try that a link turns past TraceTurning: the steps before it.)
            long tryNow = ((long)course << 32) + (fall != null ? fall.RoseByItsOwn : -1);
            if (tryNow != tracedTry && energy.Fastest > TraceTurning)
            {
                tracedTry = tryNow;
                WriteTrace(string.Format(Inv, "ENERGY_TRACE course {0} frame {1} t {2:0.000}: {3} turns at {4:0.0} rad/s, past {5} for the first time in this try", course, Time.frameCount, Time.time, energy.FastestName, energy.Fastest, TraceTurning));
            }
        }

        private void WriteTrace(string heading)
        {
            string path = Path.Combine(folder, string.Format(Inv, "energy_trace_c{0}_f{1}.txt", course, Time.frameCount));
            File.AppendAllText(path, energy.Trace(heading));
            traces++;
            Event(heading + " -> " + Path.GetFileName(path));
        }

        private static bool Finite(Vector3 v) => !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) || float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));

        // The guard of the test, not of the body: it reads, and says.
        private void Guard()
        {
            if (WentBad) return;
            var links = energy.Links;
            string part = null, why = null;
            for (int i = 0; i < links.Length && why == null; i++)
            {
                var link = links[i];
                Vector3 at = link.worldCenterOfMass, turns = link.angularVelocity, goes = link.linearVelocity;
                if (!Finite(at) || !Finite(turns) || !Finite(goes) || !Finite(link.transform.position)) { part = energy.Names[i]; why = "not finite"; }
                else if (turns.magnitude > BadTurning) { part = energy.Names[i]; why = string.Format(Inv, "turning at {0:0.0} rad/s", turns.magnitude); }
            }
            if (why == null && courseFromKnown && links.Length > 0)
            {
                float away = Vector3.Distance(links[0].worldCenterOfMass, courseFrom);
                if (away > BadAway) { part = energy.Names[0]; why = string.Format(Inv, "{0:0.0} m from where the course began", away); }
            }
            if (why == null) return;
            WentBad = true;
            Blowups++;
            string said = $"LIVE_BLOWUP {course} {Time.frameCount} {part} ({why})";
            if (BlowupsSaid.Length < 2000) BlowupsSaid += " " + said + ";";
            Event(said);
            Debug.Log(said);
            if (Energy) WriteTrace(string.Format(Inv, "ENERGY_TRACE course {0} frame {1} t {2:0.000}: gone bad, {3} {4}", course, Time.frameCount, Time.time, part, why));
        }

        public string EnergyLine() => string.Format(Inv, "LIVE_ENERGY counted {0}: {1} steps counted; gained in a step at most {2:+0.###;-0.###} J and at least {3:+0.###;-0.###} J; the fastest link {4:0.0} rad/s; {5} traces written; {6} blow-up(s){7}",
            Energy, energyStepsAll, eGainMostAll, eGainLeastAll, fastestAll, traces, Blowups, BlowupsSaid.Length > 0 ? ":" + BlowupsSaid : "");

        private FieldInfo Field(string name)
        {
            var f = typeof(MinerBody).GetField(name, Hidden);
            Assert.That(f, Is.Not.Null, "MinerBody has no field " + name);
            return f;
        }

        public void SetCourse(int k, float turned, Quaternion facing)
        {
            course = k; round = turned; frameInCourse = 0; courseStart = Time.time;
            // (Where the course began, for the guard; a new course is not yet gone bad.)
            courseFrom = own != null && own.Stands && own.Part(OwnBody.Hips) != null ? own.Part(OwnBody.Hips).worldCenterOfMass : unit.position;
            courseFromKnown = true;
            WentBad = false;
            for (int c = 0; c < Criteria.Length; c++) { pictured[c] = 0; picturedAt[c] = -1 << 20; picturedCount[c] = 0; }
            meter.SetWays(facing);
            for (int w = 0; w < meter.Ways.Length; w++)
            {
                float e = w == meter.Ways.Length - 1 ? 90 : CoverMeter.Elevations[w / 8];
                Vector3 d = meter.Ways[w];
                waysOut.WriteLine(string.Format(Inv, "{0},{1},{2},{3:0.######},{4:0.######},{5:0.######}", k, w, e, d.x, d.y, d.z));
            }
            waysOut.Flush();
            Event($"course {k}: pushed {turned.ToString("0", Inv)} degrees round; ways fixed to the world, its feet toward {facing * Vector3.forward}");
        }

        public void CourseBegan(PhysicalFall fall, int roseBefore) { this.fall = fall; this.roseBefore = roseBefore; }

        public void Event(string what)
        {
            if (events != null) events.WriteLine(string.Format(Inv, "{0} {1} {2:0.000} {3}", course, Time.frameCount, Time.time, what));
            if (Recorder != null) Recorder.Note(what);
        }

        private static float Excess(float v, float low, float high) => Mathf.Max(0, Mathf.Max(low - v, v - high));

        private string Group()
        {
            if (fall != null && (fall.Now == PhysicalFall.State.Falling || fall.Now == PhysicalFall.State.Lying)) return "lying";
            if (fall != null && (fall.Now == PhysicalFall.State.Gathering || fall.Now == PhysicalFall.State.Rising)) return "getting up";
            if (own != null && own.Stands && (own.GettingUp || own.Rising)) return "getting up";
            return "standing";
        }

        private void LateUpdate()
        {
            if (miner == null || !miner.isActiveAndEnabled || !miner.Ready || meter == null) return;
            if (fall == null) fall = unit.GetComponent<PhysicalFall>();
            frameInCourse++;
            Movement();
            int fallParts = 0, ownLinks = 0;
            for (int i = 0; fall != null && i < PhysicalFall.Count; i++) { var p = fall.Part(i); if (p != null && p.gameObject.activeInHierarchy && !p.isKinematic) fallParts++; }
            for (int i = 0; own != null && i < OwnBody.Count; i++) { var a = own.Part(i); if (a != null && a.enabled && a.gameObject.activeInHierarchy) ownLinks++; }
            bool two = fallParts > 0 && ownLinks > 0;
            string group = Group();
            string state = $"{group}: fall {(fall != null ? fall.Now.ToString() + (fall.StageNow.Length > 0 ? ":" + fall.StageNow : "") : "none")}; own stands {own != null && own.Stands}"
                + $"{(own != null && own.GettingUp ? " getting up" + (own.Riser != null ? ":" + own.Riser.StageNow : "") : "")}{(own != null && own.Rising ? " rising" : "")}"
                + $"; posed body let go {biped != null && biped.LetGo}; balance acts {balance != null && balance.Acts}; balance fell '{(balance != null ? balance.Fell : "")}'; bodies: fall {fallParts} parts, own {ownLinks} links{(two ? " TWO BODIES" : "")}";
            if (state != stateWas) { Event(state); stateWas = state; }

            var played = (Vector3[])hangsField.GetValue(miner);
            if (otherHangs == null || !ReferenceEquals(played, playedWas) || otherHangs.Length != played.Length)
            {
                // The other way's own state, begun from the played one (standing, at the start).
                otherHangs = (Vector3[])played.Clone();
                snapshot = new Vector3[played.Length];
                playedWas = played;
                Event($"the other way's flaps begun from the played way's ({played.Length} flaps)");
                Remember(played);
                return;
            }
            sinceRow++;
            bool measure = sinceRow >= (group == "standing" ? standEvery : every);
            double pictureMs = 0;
            watch.Restart();
            CoverMeter.Seen[] seenPlayed = null, seenOther = null;
            Faces facesPlayed = default, facesOther = default;
            if (measure) { seenPlayed = meter.MeasureBoth(ring); facesPlayed = FacesNow(); }
            for (int i = 0; i < under.Length; i++) { underAt[i] = under[i].position; underTurned[i] = under[i].rotation; }
            for (int k = 0; k < flaps.Length; k++) if (flaps[k].bone != null) flapLocal[k] = flaps[k].bone.localRotation;
            Array.Copy(played, snapshot, played.Length);
            var hips = new[] { DrawnHip.Of(miner, 0), DrawnHip.Of(miner, 1) };
            ReadHips(hips);
            string[] fire = null;
            float[] values = null;
            try
            {
                hangsField.SetValue(miner, otherHangs);
                MinerBody.FlapsOnTheirSeams = seamOther;
                drape.Invoke(miner, null);
                if (measure)
                {
                    seenOther = meter.MeasureBoth(ring);
                    facesOther = FacesNow();
                    values = Values(seenPlayed, seenOther, facesPlayed, facesOther, hips, two || (own != null && own.Stands && group == "lying"));
                    fire = Fire(values);
                    // The other way pictured while it is laid.
                    if (fire != null)
                    {
                        var t0 = watch.Elapsed.TotalMilliseconds;
                        Picture(fire, values, seamOther, "other");
                        pictureMs += watch.Elapsed.TotalMilliseconds - t0;
                    }
                }
            }
            finally
            {
                // The played way back exactly: the switch, and the flap bones laid again as MinerBody laid them this
                // frame - Drape run once more from the very state it began from (kept at the end of the last frame), on
                // a copy, so that the played array itself is never written. (Writing each bone's turn back instead was
                // not exact: a turn set is normalised again, a last bit off on 1295 of 3097 frames, cc_self_off,
                // November 14.) Without last frame's state (the first frame, or a frame missed), the turns written back.
                MinerBody.FlapsOnTheirSeams = seamPlayed;
                if (beforeFrame == Time.frameCount - 1)
                {
                    Array.Copy(before, replay, before.Length);
                    hangsField.SetValue(miner, replay);
                    drape.Invoke(miner, null);
                    Relaid++;
                }
                else
                    for (int k = 0; k < flaps.Length; k++) if (flaps[k].bone != null) flaps[k].bone.localRotation = flapLocal[k];
                hangsField.SetValue(miner, played);
            }
            bool exact = true;
            float off = 0;
            for (int i = 0; i < under.Length; i++)
            {
                if (!Same(under[i].position, underAt[i]) || !Same(under[i].rotation, underTurned[i])) exact = false;
                off = Mathf.Max(off, Mathf.Max(Quaternion.Angle(under[i].rotation, underTurned[i]), 1000 * Vector3.Distance(under[i].position, underAt[i])));
            }
            for (int k = 0; k < played.Length; k++) if (!Same(played[k], snapshot[k])) exact = false;
            // (Drape again from the same state gives the same state: what was laid is what MinerBody laid.)
            if (beforeFrame == Time.frameCount - 1) for (int k = 0; k < played.Length; k++) if (!Same(played[k], replay[k])) exact = false;
            Laid++;
            if (!exact) { NotRestored++; NotRestoredMost = Mathf.Max(NotRestoredMost, off); if (NotRestored <= 5) Event($"NOT RESTORED bit for bit (at most {off:R} degrees or mm off)"); }
            Remember(played);
            double ms = watch.Elapsed.TotalMilliseconds - pictureMs;
            if (fire != null)
            {
                // The played way pictured once it is back (its garments baked again for the skin's check).
                meter.Bake(ring.Body);
                Picture(fire, values, seamPlayed, "played");
                for (int c = 0; c < Criteria.Length; c++) if (Array.IndexOf(fire, Criteria[c]) >= 0) { pictured[c] = values[c]; picturedAt[c] = frameInCourse; picturedCount[c]++; }
            }
            if (!measure) return;
            sinceRow = 0;
            Measured++;
            if (timed < 200) { timed++; timedSum += ms; timedMost = Math.Max(timedMost, ms); }
            string differs = Differences(seenPlayed, seenOther, facesPlayed, facesOther);
            if (differs != null)
            {
                Differing++;
                if (FirstDifferences.Count < 10) FirstDifferences.Add($"course {course} frame {Time.frameCount} ({group}): {differs}");
            }
            Row(group, fallParts, ownLinks, two, hips, seenPlayed, seenOther, facesPlayed, facesOther, ms, differs == null, exact);
            spinMost = jumpMost = whipOwnMost = whipFallMost = 0; spinWhich = jumpWhich = "";
            eHeldMost = eGainMost = eGivenAtMost = fastestMost = float.NaN; eGainSum = 0; energySteps = 0; fastestWhich = "";
        }

        // FrameRecorder.Fault's measures, kept as the most since the last row: a drawn limb's turn and a drawn hand's or
        // foot's move since the last frame, and the fastest link of its own body (standing by its own joints) or, apart,
        // of the let-go body.
        private void Movement()
        {
            var b = miner.Rig;
            var limbs = new[] { At(b.upperArms, 0), At(b.upperArms, 1), At(b.forearms, 0), At(b.forearms, 1), At(b.thighs, 0), At(b.thighs, 1), At(b.shins, 0), At(b.shins, 1) };
            string[] limbNames = { "upperArmL", "upperArmR", "forearmL", "forearmR", "thighL", "thighR", "shinL", "shinR" };
            var ends = new[] { At(b.hands, 0), At(b.hands, 1), At(b.feet, 0), At(b.feet, 1) };
            string[] endNames = { "handL", "handR", "footL", "footR" };
            for (int i = 0; i < limbs.Length; i++)
            {
                if (limbs[i] == null) continue;
                float turned = hadLimbs ? Quaternion.Angle(limbWas[i], limbs[i].rotation) : 0;
                if (turned > spinMost) { spinMost = turned; spinWhich = limbNames[i]; }
                limbWas[i] = limbs[i].rotation;
            }
            for (int i = 0; i < ends.Length; i++)
            {
                if (ends[i] == null) continue;
                float moved = hadLimbs ? Vector3.Distance(endWas[i], ends[i].position) : 0;
                if (moved > jumpMost) { jumpMost = moved; jumpWhich = endNames[i]; }
                endWas[i] = ends[i].position;
            }
            hadLimbs = true;
            if (own != null && own.Stands)
                for (int i = 1; i < FrameRecorder.Parts && i < OwnBody.Count; i++) { var link = own.Part(i); if (link != null) whipOwnMost = Mathf.Max(whipOwnMost, link.angularVelocity.magnitude); }
            if (fall != null && fall.Now != PhysicalFall.State.Up)
                for (int i = 0; i < PhysicalFall.Count; i++) { var part = fall.Part(i); if (part != null) whipFallMost = Mathf.Max(whipFallMost, part.angularVelocity.magnitude); }
        }

        private static Transform At(Transform[] all, int i) => all != null && i < all.Length ? all[i] : null;

        // FrameRecorder's knee and elbow on its own body's links (degrees; bent the wrong way negative); NaN not standing.
        private float Bent(int near, int far)
        {
            if (own == null || !own.Stands) return float.NaN;
            var a = own.Part(near); var b = own.Part(far);
            if (a == null || b == null) return float.NaN;
            Vector3 along = a.transform.up, point = a.transform.forward, on = b.transform.up;
            return Mathf.Atan2(Vector3.Dot(on, -point), Vector3.Dot(on, along)) * Mathf.Rad2Deg;
        }

        // The drawn hip unwrapped (a fold past 180 reads past 180, the twist likewise), and beside it, where the own body
        // stands, the same angles from its articulation's parts (DrawnHip.OfLinks) and the fold as one angle both ways.
        private void ReadHips(DrawnHip.Read[] hips)
        {
            var bones = miner.Rig;
            Quaternion frame = bones.pelvis.rotation * Quaternion.Inverse((Quaternion)pelvisRestField.GetValue(miner));
            for (int l = 0; l < 2; l++)
            {
                hipsRaw[l] = hips[l];
                hips[l].Flex = DrawnHip.Unwrap(hips[l].Flex, flexWas[l]); flexWas[l] = hips[l].Flex;
                hips[l].Twist = DrawnHip.Unwrap(hips[l].Twist, twistWas[l]); twistWas[l] = hips[l].Twist;
                DrawnHip.DrawnThigh(miner, l, out var line, out _);
                foldDrawn[l] = DrawnHip.Fold(frame * Vector3.up, line);
                var hl = own != null && own.Stands ? own.Part(OwnBody.Hips) : null;
                var th = own != null && own.Stands ? own.Part(OwnBody.Thigh + l) : null;
                var sh = own != null && own.Stands ? own.Part(OwnBody.Shin + l) : null;
                hasLink[l] = hl != null && th != null && sh != null;
                if (hasLink[l])
                {
                    var r = DrawnHip.OfLinks(hl.transform, th.transform, sh.transform, l);
                    r.Flex = DrawnHip.Unwrap(r.Flex, linkFlexWas[l]); linkFlexWas[l] = r.Flex;
                    r.Twist = DrawnHip.Unwrap(r.Twist, linkTwistWas[l]); linkTwistWas[l] = r.Twist;
                    hipsLink[l] = r;
                    foldLink[l] = DrawnHip.Fold(hl.transform.up, th.transform.up);
                }
                else { linkFlexWas[l] = linkTwistWas[l] = float.NaN; }
            }
        }

        // Each flap's stop face as Drape works it out, against the thigh's own front (the cover grid's Measure, both
        // legs), and how each flap hangs on that front: read with the way laid now (the switch and flapHangs as they are).
        private Faces FacesNow()
        {
            var f = new Faces { Has = new bool[2], Agrees = new bool[2], FrontOn = new float[2], BackOn = new float[2], ByBend = new bool[2], Split = new float[2] };
            var bones = miner.Rig;
            var hangs = (Vector3[])hangsField.GetValue(miner);
            var thighFront = (Vector3[])thighFrontField.GetValue(miner);
            for (int leg = 0; leg < 2; leg++)
            {
                Vector3 across = bones.pelvis.rotation * acrossAt, forward = bones.pelvis.rotation * forwardAt;
                Vector3 thigh = (bones.shins[leg].position - bones.thighs[leg].position).normalized;
                Vector3 flat = Vector3.ProjectOnPlane(thigh, across);
                flat = flat.sqrMagnitude > 1e-4f ? flat.normalized : Vector3.down;
                Vector3 face = Vector3.Cross(across, flat);
                Vector3 frontWay = MinerBody.FlapsOnTheirSeams ? bones.thighs[leg].rotation * thighFront[leg] : forward;
                if (Vector3.Dot(face, frontWay) < 0) face = -face;
                // The thigh's true front (November 14, the review): not the solved thigh's forward (DrawnHip.TrueFront).
                Vector3 trueFront = DrawnHip.TrueFront(miner, leg, out f.ByBend[leg], out f.Split[leg]);
                Vector3 front = Vector3.ProjectOnPlane(trueFront, across);
                front = front.sqrMagnitude > 1e-6f ? front.normalized : Vector3.zero;
                f.Agrees[leg] = Vector3.Dot(face, front) > 0;
                f.FrontOn[leg] = f.BackOn[leg] = float.NaN;
                for (int k = 0; k < flaps.Length && k < hangs.Length; k++)
                    if (flaps[k].leg == leg && flaps[k].bone != null)
                    {
                        f.Has[leg] = true;
                        float on = Vector3.Dot(hangs[k], front);
                        if (flaps[k].front) f.FrontOn[leg] = on; else f.BackOn[leg] = -on;
                    }
            }
            return f;
        }

        // The criteria for pictures, the worse of the two ways: points seen from the worst game camera's way (both legs),
        // points seen from any way, legs whose flap face is wrong, degrees past the hip's and the knee's bars,
        // FrameRecorder's movement past its bars, and two bodies (or the fall's body let go while its own stands).
        private float[] Values(CoverMeter.Seen[] p, CoverMeter.Seen[] o, Faces fp, Faces fo, DrawnHip.Read[] hips, bool bodies)
        {
            float RingGame(CoverMeter.Seen[] s) { float t = 0; for (int l = 0; l < 2; l++) { int m = 0; for (int w = GameFrom; w < s[l].ByWay.Length; w++) m = Math.Max(m, s[l].ByWay[w]); t += m; } return t; }
            float FaceWrong(Faces f) { float t = 0; for (int l = 0; l < 2; l++) if (f.Has[l] && !f.Agrees[l]) t++; return t; }
            float hip = 0, knee = 0;
            for (int l = 0; l < 2; l++)
            {
                hip = Mathf.Max(hip, Mathf.Max(Excess(hips[l].Flex, -25, 125), Mathf.Max(Mathf.Max(0, Mathf.Abs(hips[l].Abd) - 50), Mathf.Max(0, Mathf.Abs(hips[l].Twist) - 45))));
                knee = Mathf.Max(knee, Excess(hips[l].Knee, -8, 150));
            }
            float motion = Mathf.Max(Mathf.Max(0, spinMost - 30), Mathf.Max(Mathf.Max(0, (jumpMost - .06f) * 500), Mathf.Max(0, whipOwnMost - 30)));
            return new[]
            {
                Mathf.Max(RingGame(p), RingGame(o)), Mathf.Max(p[0].Open + p[1].Open, o[0].Open + o[1].Open), Mathf.Max(FaceWrong(fp), FaceWrong(fo)),
                hip, knee, motion, bodies ? 1 : 0,
            };
        }

        // Which criteria are pictured now: a new most in this course, no sooner than Spacing frames after its last picture,
        // and no more than so many a course.
        private string[] Fire(float[] values)
        {
            List<string> fire = null;
            for (int c = 0; c < Criteria.Length; c++)
                if (values[c] > 0 && values[c] > pictured[c] && frameInCourse - picturedAt[c] >= Spacing && picturedCount[c] < picsMost)
                    (fire ?? (fire = new List<string>())).Add(Criteria[c]);
            return fire?.ToArray();
        }

        // Four views of the hips as drawn now, by the lying body's own axes: from its feet and left and from its head and
        // right at 32 degrees, straight above, and from its left side at the game camera's 47 degrees. (Standing, its
        // front stands for its feet.)
        private void Picture(string[] fire, float[] values, bool wayOn, string which)
        {
            var b = miner.Rig;
            Quaternion hips = b.pelvis.rotation * Quaternion.Inverse((Quaternion)pelvisRestField.GetValue(miner));
            Vector3 hx = hips * Vector3.forward, hz = hips * Vector3.up, hy = -Vector3.Cross(hz, hx).normalized;
            Vector3 feet = Vector3.ProjectOnPlane(-hz, Vector3.up);
            if (feet.sqrMagnitude < .25f) feet = Vector3.ProjectOnPlane(hx, Vector3.up);
            feet = feet.sqrMagnitude > 1e-6f ? feet.normalized : Vector3.forward;
            Vector3 left = Vector3.ProjectOnPlane(hy, Vector3.up);
            left = left.sqrMagnitude > 1e-4f ? left.normalized : Vector3.Cross(feet, Vector3.up).normalized;
            Vector3 aim = (b.thighs[0].position + b.thighs[1].position) * .5f;
            Vector3 Way(Vector3 flat, float e) => (Mathf.Cos(e * Mathf.Deg2Rad) * flat.normalized + Mathf.Sin(e * Mathf.Deg2Rad) * Vector3.up).normalized;
            var views = new[] { ("feetleft32", Way(feet + left, 32)), ("headright32", Way(-feet - left, 32)), ("above", Vector3.up), ("side47", Way(left, 47)) };
            string crit = string.Join("+", fire);
            var said = new StringBuilder();
            for (int c = 0; c < Criteria.Length; c++) if (Array.IndexOf(fire, Criteria[c]) >= 0) said.Append(said.Length > 0 ? "+" : "").Append(values[c].ToString("0.##", Inv));
            float before = SkinApart(), after = -1;
            for (int i = 0; i < skins.Count; i++) skins[i].forceMatrixRecalculationPerRender = true;
            try
            {
                for (int v = 0; v < views.Length; v++)
                {
                    var (name, d) = views[v];
                    pictures.transform.position = aim + d * Distance;
                    pictures.transform.rotation = name == "above" ? Quaternion.LookRotation(Vector3.down, feet) : Quaternion.LookRotation(-d, Vector3.up);
                    string file = $"c{course:D2}_f{Time.frameCount:D6}_{crit}_{(wayOn ? "on" : "off")}_{which}_{name}";
                    CaptureTools.Render(pictures, Path.Combine(folder, "pictures", file), Tile, Tile);
                    if (v == 0) after = SkinApart();
                    picturesOut.WriteLine(string.Format(Inv, "{0},{1},{2},{3},{4},{5},{6},{7},{8:0.###},{9:0.###}", course, Time.frameCount, crit, said, wayOn ? "on" : "off", which, name, file + ".ppm", before, after));
                }
            }
            finally
            {
                for (int i = 0; i < skins.Count; i++) skins[i].forceMatrixRecalculationPerRender = skinsForcedWas[i];
            }
            picturesOut.Flush();
            skinChecks++;
            if (before >= 0) skinBeforeMost = Mathf.Max(skinBeforeMost, before);
            if (after >= 0) skinAfterMost = Mathf.Max(skinAfterMost, after);
            if (after < 0 || after > .5f) skinStale++;
        }

        // How far apart (mm) the graphics card's skin of the drawn body and the meter's hand bake of it are, the nearest of
        // the spaces the card may keep it in (the cover grid's GpuCheck); -1 not read.
        private float SkinApart()
        {
            var c = ring.Body;
            using (var buf = c.Skin.GetVertexBuffer())
            {
                if (buf == null) return -1;
                long bytes = (long)buf.count * buf.stride;
                int n = c.Rest.Length;
                if (n == 0 || bytes % n != 0 || bytes / n % 4 != 0) return -1;
                int step = (int)(bytes / n / 4);
                var data = new float[bytes / 4];
                buf.GetData(data);
                var root = c.Skin.rootBone != null ? c.Skin.rootBone : c.Skin.transform;
                float best = -1;
                foreach (var m in new[] { Matrix4x4.identity, c.Skin.transform.worldToLocalMatrix, root.worldToLocalMatrix })
                {
                    float worst = 0;
                    for (int v = 0; v < n; v++) worst = Mathf.Max(worst, Vector3.Distance(new Vector3(data[v * step], data[v * step + 1], data[v * step + 2]), m.MultiplyPoint3x4(c.Verts[v])));
                    if (best < 0 || worst < best) best = worst;
                }
                return 1000 * best;
            }
        }

        // The played way's state as MinerBody's Drape will begin from it next frame.
        private void Remember(Vector3[] played)
        {
            if (before == null || before.Length != played.Length) { before = new Vector3[played.Length]; replay = new Vector3[played.Length]; }
            Array.Copy(played, before, played.Length);
            beforeFrame = Time.frameCount;
        }

        private static bool Same(float a, float b) => BitConverter.SingleToInt32Bits(a) == BitConverter.SingleToInt32Bits(b);
        private static bool Same(Vector3 a, Vector3 b) => Same(a.x, b.x) && Same(a.y, b.y) && Same(a.z, b.z);
        private static bool Same(Quaternion a, Quaternion b) => Same(a.x, b.x) && Same(a.y, b.y) && Same(a.z, b.z) && Same(a.w, b.w);

        // What differs between the two measures, if anything: every Seen field and each way's count, the flap face, and
        // how the flaps hang on it, both legs.
        private static string Differences(CoverMeter.Seen[] p, CoverMeter.Seen[] o, Faces fp, Faces fo)
        {
            var d = new StringBuilder();
            for (int l = 0; l < 2; l++)
            {
                string leg = l == 0 ? "L" : "R";
                if (p[l].Points != o[l].Points) d.Append($" {leg} points {p[l].Points}/{o[l].Points}");
                if (p[l].Open != o[l].Open) d.Append($" {leg} open {p[l].Open}/{o[l].Open}");
                if (p[l].OpenLoose != o[l].OpenLoose) d.Append($" {leg} loose {p[l].OpenLoose}/{o[l].OpenLoose}");
                if (p[l].Views != o[l].Views) d.Append($" {leg} views {p[l].Views}/{o[l].Views}");
                for (int w = 0; w < p[l].ByWay.Length; w++) if (p[l].ByWay[w] != o[l].ByWay[w]) d.Append($" {leg} way {w} {p[l].ByWay[w]}/{o[l].ByWay[w]}");
                if (fp.Has[l] != fo.Has[l]) d.Append($" {leg} has flaps {fp.Has[l]}/{fo.Has[l]}");
                if (fp.Agrees[l] != fo.Agrees[l]) d.Append($" {leg} face {fp.Agrees[l]}/{fo.Agrees[l]}");
                if (!Same(fp.FrontOn[l], fo.FrontOn[l])) d.Append($" {leg} front on {fp.FrontOn[l]:R}/{fo.FrontOn[l]:R}");
                if (!Same(fp.BackOn[l], fo.BackOn[l])) d.Append($" {leg} back on {fp.BackOn[l]:R}/{fo.BackOn[l]:R}");
            }
            return d.Length > 0 ? d.ToString().Trim() : null;
        }

        private static string F(float v) => float.IsNaN(v) || float.IsInfinity(v) ? "" : v.ToString("0.#####", Inv);
        private static string Clean(string s) => (s ?? "").Replace(',', ';').Replace('\n', ' ').Replace('\r', ' ');

        // (-liveEnergy 1 adds its columns at the end; without it the columns are as they were.)
        private static string Header(bool energy)
        {
            var h = new StringBuilder("course,push_way,frame,frame_in_course,t,state,fall_now,fall_stage,own_stands,own_getting_up,own_rising,riser_stage,own_feet_from_lying,tries,"
                + "posed_let_go,balance_acts,balance_fell,fall_parts,own_links,two_bodies,fall_root,own_root,"
                + "pelvis_x,pelvis_y,pelvis_z,hx_x,hx_y,hx_z,hy_x,hy_y,hy_z,hz_x,hz_y,hz_z");
            foreach (string leg in new[] { "L", "R" }) h.Append($",{leg}_flex,{leg}_abd,{leg}_twist,{leg}_knee,{leg}_locked");
            h.Append(",spin_most,spin_limb,jump_most,jump_end,own_knee_bent_L,own_knee_bent_R,own_elbow_bent_L,own_elbow_bent_R,whip_own_most,whip_fall_most,drape_ground_mm");
            foreach (string p in new[] { "p", "o" })
            {
                h.Append($",{p}_seam");
                foreach (string leg in new[] { "L", "R" })
                {
                    h.Append($",{p}_{leg}_points,{p}_{leg}_open,{p}_{leg}_loose,{p}_{leg}_views");
                    for (int w = 0; w < 25; w++) h.Append($",{p}_{leg}_w{w:D2}");
                    h.Append($",{p}_{leg}_has_flaps,{p}_{leg}_face,{p}_{leg}_front_on,{p}_{leg}_back_on");
                }
            }
            h.Append(",pair_ms,ways_same,restored_exact");
            foreach (string leg in new[] { "L", "R" })
                h.Append($",{leg}_flex_raw,{leg}_twist_raw,{leg}_fold,{leg}_link,{leg}_link_flex,{leg}_link_abd,{leg}_link_twist,{leg}_link_knee,{leg}_link_fold,{leg}_front_by_bend,{leg}_front_split");
            // (Since the last row: e_held the most its own body held (J, moving and lifted); e_gain the largest a step
            // gained beyond what was given, e_given what was given on that step; e_gain_sum the gains summed; e_steps the
            // steps counted; fastest_rads and fastest_part the fastest any link truly turned. Empty without a step counted.)
            if (energy) h.Append(",e_held,e_given,e_gain,e_gain_sum,e_steps,fastest_rads,fastest_part");
            return h.ToString();
        }

        private void Row(string group, int fallParts, int ownLinks, bool two, DrawnHip.Read[] hips, CoverMeter.Seen[] p, CoverMeter.Seen[] o, Faces fp, Faces fo, double ms, bool same, bool exact)
        {
            var b = miner.Rig;
            Quaternion frame = b.pelvis.rotation * Quaternion.Inverse((Quaternion)pelvisRestField.GetValue(miner));
            Vector3 hx = frame * Vector3.forward, hz = frame * Vector3.up, hy = -Vector3.Cross(hz, hx).normalized;
            var r = new StringBuilder();
            void V(string s) => r.Append(',').Append(s);
            void Vec(Vector3 v) { V(F(v.x)); V(F(v.y)); V(F(v.z)); }
            r.Append(course.ToString(Inv));
            V(F(round)); V(Time.frameCount.ToString(Inv)); V(frameInCourse.ToString(Inv)); V(F(Time.time - courseStart)); V(group);
            V(fall != null ? fall.Now.ToString() : "none"); V(Clean(fall != null ? fall.StageNow : ""));
            V(own != null && own.Stands ? "1" : "0"); V(own != null && own.GettingUp ? "1" : "0"); V(own != null && own.Rising ? "1" : "0");
            V(Clean(own != null && own.Riser != null ? own.Riser.StageNow : "")); V(own != null ? own.OnItsFeetFromLying.ToString(Inv) : "");
            V(fall != null ? (fall.RoseByItsOwn - roseBefore).ToString(Inv) : "0");
            V(biped != null && biped.LetGo ? "1" : "0"); V(balance != null && balance.Acts ? "1" : "0"); V(Clean(balance != null ? balance.Fell : ""));
            V(fallParts.ToString(Inv)); V(ownLinks.ToString(Inv)); V(two ? "1" : "0");
            V(Clean(fall != null && fall.Part(0) != null ? fall.Part(0).transform.root.name : ""));
            V(Clean(own != null && own.Part(OwnBody.Hips) != null ? own.Part(OwnBody.Hips).transform.root.name : ""));
            Vec(b.pelvis.position); Vec(hx); Vec(hy); Vec(hz);
            for (int l = 0; l < 2; l++) { V(F(hips[l].Flex)); V(F(hips[l].Abd)); V(F(hips[l].Twist)); V(F(hips[l].Knee)); V(hips[l].Locked ? "1" : "0"); }
            V(F(spinMost)); V(spinWhich); V(F(jumpMost)); V(jumpWhich);
            V(F(Bent(OwnBody.Thigh, OwnBody.Shin))); V(F(Bent(OwnBody.Thigh + 1, OwnBody.Shin + 1)));
            V(F(Bent(OwnBody.UpperArm, OwnBody.Forearm))); V(F(Bent(OwnBody.UpperArm + 1, OwnBody.Forearm + 1)));
            V(F(whipOwnMost)); V(F(whipFallMost));
            // Drape's ground (from the feet, as Drape takes it) against the terrain under the lower foot.
            int lower = b.feet[0].position.y <= b.feet[1].position.y ? 0 : 1;
            float drapeGround = b.feet[lower].position.y - ankleUp;
            Vector3 foot = b.feet[lower].position;
            V(Physics.Raycast(foot + Vector3.up * 2, Vector3.down, out var hit, 6, 1 << 6, QueryTriggerInteraction.Ignore) ? F(1000 * (drapeGround - hit.point.y)) : "");
            foreach (var (seen, f, on) in new[] { (p, fp, seamPlayed), (o, fo, seamOther) })
            {
                V(on ? "1" : "0");
                for (int l = 0; l < 2; l++)
                {
                    V(seen[l].Points.ToString(Inv)); V(seen[l].Open.ToString(Inv)); V(seen[l].OpenLoose.ToString(Inv)); V(seen[l].Views.ToString(Inv));
                    for (int w = 0; w < 25; w++) V(w < seen[l].ByWay.Length ? seen[l].ByWay[w].ToString(Inv) : "");
                    V(f.Has[l] ? "1" : "0"); V(f.Has[l] ? (f.Agrees[l] ? "1" : "0") : ""); V(F(f.FrontOn[l])); V(F(f.BackOn[l]));
                }
            }
            V(ms.ToString("0.###", Inv)); V(same ? "1" : "0"); V(exact ? "1" : "0");
            for (int l = 0; l < 2; l++)
            {
                V(F(hipsRaw[l].Flex)); V(F(hipsRaw[l].Twist)); V(F(foldDrawn[l])); V(hasLink[l] ? "1" : "0");
                if (hasLink[l]) { V(F(hipsLink[l].Flex)); V(F(hipsLink[l].Abd)); V(F(hipsLink[l].Twist)); V(F(hipsLink[l].Knee)); V(F(foldLink[l])); }
                else for (int k = 0; k < 5; k++) V("");
                V(fp.ByBend != null && fp.ByBend[l] ? "1" : "0"); V(fp.Split != null ? F(fp.Split[l]) : "");
            }
            if (Energy)
            {
                bool any = energySteps > 0;
                V(any ? F(eHeldMost) : ""); V(any ? F(eGivenAtMost) : ""); V(any ? F(eGainMost) : ""); V(any ? F(eGainSum) : "");
                V(energySteps.ToString(Inv)); V(any ? F(fastestMost) : ""); V(any ? Clean(fastestWhich) : "");
            }
            rows.WriteLine(r.ToString());
        }

        public void Finish()
        {
            Event($"done: {Measured} frames measured, {Differing} differing, {Laid} laid, {NotRestored} not restored bit for bit");
            if (rows != null) rows.Flush();
            if (events != null) events.Flush();
            if (picturesOut != null) picturesOut.Flush();
        }

        public string TimingLine() => string.Format(Inv, "LIVE_TIMING a measure pair (played, the other laid, measured, put back; pictures not counted) over the first {0} measured frames: mean {1:0.00} ms, largest {2:0.00} ms",
            timed, timed > 0 ? timedSum / timed : 0, timedMost);

        public string PairLine() => $"LIVE_PAIR played {(seamPlayed ? "on" : "off")}, other {(seamOther ? "on" : "off")}: {Measured} frames measured, {Differing} with the two measures differing; the other way laid on {Laid} frames, the played flaps back bit for bit on all but {NotRestored} (those at most {NotRestoredMost:R} degrees or mm off); laid again by Drape from last frame's state on {Relaid}, their turns written back on the rest";

        public string SelfCheckLine() => $"SELF_CHECK frames {Measured} mismatches {Differing} restored_exact {(NotRestored == 0 && Laid > 0 ? "true" : "false")}";

        public string SkinLine() => string.Format(Inv, "LIVE_SKIN {0} picture sets: the drawn body's graphics-card skin against the hand bake, before the render at most {1:0.###} mm, after it at most {2:0.###} mm; {3} sets with it more than 0.5 mm apart (or unread) after the render",
            skinChecks, skinBeforeMost, skinAfterMost, skinStale);

        public void Close()
        {
            if (skinsForcedWas != null) for (int i = 0; i < skins.Count; i++) if (skins[i] != null) skins[i].forceMatrixRecalculationPerRender = skinsForcedWas[i];
            if (rows != null) { rows.Flush(); rows.Close(); rows = null; }
            if (events != null) { events.Flush(); events.Close(); events = null; }
            if (waysOut != null) { waysOut.Flush(); waysOut.Close(); waysOut = null; }
            if (picturesOut != null) { picturesOut.Flush(); picturesOut.Close(); picturesOut = null; }
            meter = null;
        }

        private void OnDestroy() => Close();
    }
}
