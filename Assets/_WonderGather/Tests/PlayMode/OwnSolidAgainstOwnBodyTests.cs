using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // (c), November 13, 2026 (afternoon). Luis saw a leg looking detached while Small got up by its own body. One cause
    // the sceptic named (Docs/Research/2026-11-13_DetachedLeg/investigation.md, "A cause missed", probably negative):
    // the unit's own solid, the capsule it is picked and avoided by, is told to ignore every part of its own body while
    // the solid is switched off (OwnBody's making, lying, before PhysicalFall switches the solid on again), and through a
    // try the solid is moved to the ground under the hips every frame. If that ignoring is lost, a post 0.43 m wide stands
    // in the hips and shoves the thighs apart. A measure through the game's own flow, pushing Small over as
    // GettingUpByItsOwnTests does (with "Gets up by its own body" on): through every try, the frames on which physics
    // does not ignore a pair of the solid and a part, how often the solid is in the body at all (so that no contact means
    // something), and the contacts between them, counted from both sides. Bars: 0 and 0. Before the pushes, a ball
    // dropped on the solid shows that contacts with it are seen at all; the body's contacts with anything (the ground)
    // show the same for its side.
    public sealed class OwnSolidAgainstOwnBodyTests
    {
        private MinerChoice choice;
        private MinerWorkPreview look;

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            choice = Object.FindAnyObjectByType<MinerChoice>();
            look = Object.FindAnyObjectByType<MinerWorkPreview>();
        }

        [TearDown]
        public void Restore() { Time.captureFramerate = 0; PhysicalFall.GetsUpByItsOwn = false; OwnSolidContacts.Reset(); }

        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator SmallsOwnSolidLeavesItsOwnBodyAlone()
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            string rate = CaptureTools.Argument("-ownRate");
            Time.captureFramerate = string.IsNullOrEmpty(rate) ? 50 : int.Parse(rate);
            PhysicalFall.GetsUpByItsOwn = true;
            // (A measure: what the engine says of a body gone wild is told, and it goes on.)
            LogAssert.ignoreFailingMessages = true;
            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Small") index = i;
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner called Small.");
            choice.Choose(index);
            yield return Wait(.3f);
            var unit = choice.Current;
            look.SetOwn(true);
            OwnBody own = null;
            for (float until = Time.time + 20; (own == null || !own.Stands) && Time.time < until;) { unit.TryGetComponent(out own); yield return null; }
            Assert.That(own != null && own.Stands, Is.True, "Small did not come to stand by its own joints.");
            Assert.That(own.KnowsAWayUp, Is.True, "Small knows no way up (Resources/RiseWays/Small.txt).");
            var mine = unit.GetComponents<Collider>();
            Assert.That(mine.Length, Is.GreaterThan(0), "Small has no solid of its own.");
            foreach (var m in mine)
            {
                string size = m is CapsuleCollider cap ? string.Format(inv, " radius {0:0.000} height {1:0.000} centre {2:0.000}", cap.radius, cap.height, cap.center.y) : "";
                Debug.Log("OWN_SOLID its solid: " + m.GetType().Name + size + (m.isTrigger ? ", a trigger" : ", solid") + ", enabled " + m.enabled
                    + (m.attachedRigidbody != null ? ", on a rigidbody" : "") + (m.attachedArticulationBody != null ? ", on an articulation" : ""));
            }

            // (The ball: dropped on the top of its solid, passing through everything else of it, so that it can strike
            // the solid alone.)
            var onSolid = unit.gameObject.AddComponent<OwnSolidContacts>();
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Ball on its solid";
            ball.transform.localScale = Vector3.one * .1f;
            var ballSolid = ball.GetComponent<Collider>();
            foreach (var c in unit.GetComponentsInChildren<Collider>()) if (System.Array.IndexOf(mine, c) < 0) Physics.IgnoreCollision(ballSolid, c);
            var standing = own.Part(OwnBody.Hips) != null ? own.Part(OwnBody.Hips).transform.root : null;
            if (standing != null) foreach (var c in standing.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(ballSolid, c);
            Bounds top = mine[0].bounds;
            ball.transform.position = new Vector3(top.center.x, top.max.y + .3f, top.center.z);
            ball.AddComponent<Rigidbody>().mass = .05f;
            onSolid.Counts = c => c == ballSolid;
            yield return Wait(1);
            int ballContacts = OwnSolidContacts.FromSolid;
            Debug.Log("OWN_SOLID the ball on its solid: " + ballContacts + " contacts seen from the solid's side");
            Object.Destroy(ball);
            OwnSolidContacts.Reset();
            onSolid.Counts = null;
            // (Not seen from the solid's side, the body's side still sees: its contacts with the ground, which is as
            // static as the solid, are its control.)
            if (ballContacts == 0) Debug.Log("OWN_SOLID contacts are not seen from the solid's side; the body's side alone counts");

            int pushes = 3;
            string given = CaptureTools.Argument("-ownPushes");
            if (!string.IsNullOrEmpty(given)) pushes = int.Parse(given, inv);
            int triesAll = 0, tryFrames = 0, solidOnFrames = 0, inBodyFrames = 0, notIgnoredFrames = 0, mostNotIgnored = 0;
            float deepest = 0;
            for (int k = 0; k < pushes; k++)
            {
                // (Standing by its own joints before each push.)
                for (float until = Time.time + 15; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                yield return Wait(1.5f);
                // (Its fall is added when it is first pushed over.)
                var fall = unit.GetComponent<PhysicalFall>();
                int roseBefore = fall != null ? fall.RoseByItsOwn : 0, triesSeen = 0;
                for (float until = Time.time + 20; !look.CanPushOver && Time.time < until;) yield return null;
                if (!look.CanPushOver) { Debug.Log("OWN_SOLID push " + k + ": it could not be pushed over; passed over"); continue; }
                // (Forward, so that it falls onto its front, as GettingUpByItsOwnTests; a little aside, each push its own.)
                Vector3 way = Quaternion.AngleAxis((k % 3 - 1) * 20, Vector3.up) * unit.transform.forward;
                look.PushOver(way);
                fall = unit.GetComponent<PhysicalFall>();
                Assert.That(fall, Is.Not.Null, "Small has no fall after it was pushed over.");
                float began = Time.time;
                Transform body = null;
                Collider[] solids = new Collider[0];
                onSolid.Counts = c => body != null && c.transform.IsChildOf(body);
                bool Up() => fall.Now == PhysicalFall.State.Up && (own.Stands ? !own.GettingUp && !own.Rising : true);
                while (Time.time - began < 2 || (!Up() && Time.time - began < 240))
                {
                    // (Each try, its own body made anew: the pairs, and a counter on each of its parts.)
                    if (fall.RoseByItsOwn - roseBefore > triesSeen)
                    {
                        triesSeen = fall.RoseByItsOwn - roseBefore; triesAll++;
                        var hips = own.Stands ? own.Part(OwnBody.Hips) : null;
                        body = hips != null ? hips.transform.root : null;
                        solids = body != null ? body.GetComponentsInChildren<Collider>() : new Collider[0];
                        foreach (var s in solids)
                        {
                            var link = s.attachedArticulationBody;
                            if (link == null || link.TryGetComponent<OwnSolidContacts>(out _)) continue;
                            var counter = link.gameObject.AddComponent<OwnSolidContacts>();
                            counter.Body = true;
                            counter.Counts = c => System.Array.IndexOf(mine, c) >= 0;
                        }
                        Debug.Log("OWN_SOLID push " + k + ", try " + triesSeen + ": its own body " + (body != null ? body.name : "none") + ", " + solids.Length + " solids");
                    }
                    // (Every frame of a try: whether its solid is on and in the body, and whether physics ignores each pair.)
                    var nowHips = own.Stands ? own.Part(OwnBody.Hips) : null;
                    if (body != null && nowHips != null && nowHips.transform.root == body)
                    {
                        tryFrames++;
                        bool on = false;
                        foreach (var m in mine) on |= m.enabled && m.gameObject.activeInHierarchy;
                        if (on) solidOnFrames++;
                        int notIgnored = 0;
                        bool inside = false;
                        foreach (var m in mine)
                            foreach (var s in solids)
                            {
                                if (s == null || !s.enabled) continue;
                                if (!Physics.GetIgnoreCollision(m, s)) notIgnored++;
                                if (on && Physics.ComputePenetration(m, m.transform.position, m.transform.rotation, s, s.transform.position, s.transform.rotation, out _, out float depth))
                                {
                                    inside = true;
                                    if (depth > deepest) deepest = depth;
                                }
                            }
                        if (inside) inBodyFrames++;
                        if (notIgnored > 0) notIgnoredFrames++;
                        if (notIgnored > mostNotIgnored) mostNotIgnored = notIgnored;
                    }
                    yield return null;
                }
                Debug.Log(string.Format(inv, "OWN_SOLID push {0}: tries by its own body {1}; {2} after {3:0.0} s; the fall said: {4}",
                    k, triesSeen, Up() ? (own.Stands ? "up by its own body" : "up the old way") : "not up", Time.time - began, fall.ByItsOwnSaid));
                // (Not up at the end: the posed body takes it back where it lies and stands, so that the pushes go on.)
                if (!Up())
                {
                    if (own.Stands) own.Drop();
                    fall.TakeBack();
                    for (float until = Time.time + 20; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                }
            }
            onSolid.Counts = null;
            Debug.Log(string.Format(inv,
                "OWN_SOLID Small, {0} pushes, {1} tries by its own body, {2} frames in a try: its solid on in {3}, in the body in {4} (deepest {5:0.000} m); "
                + "frames with a pair not ignored {6} (at most {7} pairs); contacts from the solid's side {8} on {9} steps, from the body's side {10} on {11} steps "
                + "(the body's contacts with anything, the ground too: {12}; the ball's with the solid: {14}); between: {13}",
                pushes, triesAll, tryFrames, solidOnFrames, inBodyFrames, deepest, notIgnoredFrames, mostNotIgnored,
                OwnSolidContacts.FromSolid, OwnSolidContacts.SolidSteps, OwnSolidContacts.FromBody, OwnSolidContacts.BodySteps, OwnSolidContacts.BodyAny,
                OwnSolidContacts.Names.Count > 0 ? string.Join(", ", OwnSolidContacts.Names) : "none", ballContacts));
            Assert.That(triesAll, Is.GreaterThan(0), "No try by its own body was seen.");
            Assert.That(OwnSolidContacts.BodyAny, Is.GreaterThan(0), "The body's contacts are not seen at all: this measure could not see its side.");
            Assert.That(notIgnoredFrames, Is.EqualTo(0), "Physics does not ignore its solid against its own body.");
            Assert.That(OwnSolidContacts.FromSolid + OwnSolidContacts.FromBody, Is.EqualTo(0), "Its solid touched its own body.");
        }
    }

    // Counts the contacts this object's colliders have with the colliders it is told to count, shared across the solid's
    // side and the body's side; and, on the body's side, every contact at all, to show that they are seen.
    internal sealed class OwnSolidContacts : MonoBehaviour
    {
        public static int FromSolid, FromBody, SolidSteps, BodySteps, BodyAny;
        public static readonly HashSet<string> Names = new HashSet<string>();
        private static int solidStep = -1, bodyStep = -1;
        public bool Body;
        public System.Func<Collider, bool> Counts;

        public static void Reset() { FromSolid = FromBody = SolidSteps = BodySteps = BodyAny = 0; solidStep = bodyStep = -1; Names.Clear(); }

        private void Seen(Collision c)
        {
            if (Body) BodyAny++;
            if (Counts == null || c.collider == null || !Counts(c.collider)) return;
            int step = Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime);
            if (Body) { FromBody++; if (step != bodyStep) { BodySteps++; bodyStep = step; } }
            else { FromSolid++; if (step != solidStep) { SolidSteps++; solidStep = step; } }
            string here = c.contactCount > 0 ? c.GetContact(0).thisCollider.name : name;
            if (Names.Count < 40) Names.Add(here + " - " + c.collider.name);
        }

        private void OnCollisionEnter(Collision c) => Seen(c);
        private void OnCollisionStay(Collision c) => Seen(c);
    }
}
