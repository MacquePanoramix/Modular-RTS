using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // S3b, step 2, in the game (October 21): with the panel's "Gets up by its own body" on, a miner that knows a way up
    // (Small, so far), pushed over onto its front, lies where its fall left it, becomes its own and gets up by its own
    // limbs; gone down again on the way, its fall has it, it lies a while and tries again; lying as none of its ways is
    // for, or after so many tries, it gets up the old way. A measure, not a test that must pass: each try is partly
    // chance (on the bench about one in three stands), so it tells how often, and how long.
    public sealed class GettingUpByItsOwnTests
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
        public void Restore() { Time.captureFramerate = 0; PhysicalFall.GetsUpByItsOwn = false; OwnBody.Told = null; RiseAgent.Unheard = false; OwnBody.StoodHeightIs = 0; }

        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        [UnityTest, Explicit, Timeout(14400000)]
        public IEnumerator SmallPushedOverGetsUpByItsOwnBody()
        {
            Time.captureFramerate = 50;
            PhysicalFall.GetsUpByItsOwn = true;
            // (-ownUnheard 1: no learnt model given, to set the body as searched beside it, October 27.)
            RiseAgent.Unheard = CaptureTools.Argument("-ownUnheard") == "1";
            // (-ownStoodHeight 0.696: how high its weight rises to after getting up, as the teaching notes it, October 29.)
            string stoodHeight = CaptureTools.Argument("-ownStoodHeight");
            if (!string.IsNullOrEmpty(stoodHeight)) OwnBody.StoodHeightIs = float.Parse(stoodHeight, System.Globalization.CultureInfo.InvariantCulture);
            // (A measure, not a test that must pass: what the engine says of a body gone wild is told, and it goes on.
            // And what its own body says as it goes: where each try ended.)
            LogAssert.ignoreFailingMessages = true;
            // (And why a rise after getting up did not stand, as the teaching tells it, and where: its hips, whether the
            // walked ground is within a quarter of a metre, and how the ground slopes there. October 29.)
            OwnBody own = null;
            string Where()
            {
                if (own == null || !own.Stands) return "";
                Vector3 at = own.Part(OwnBody.Hips).transform.position;
                bool walked = UnityEngine.AI.NavMesh.SamplePosition(at, out _, .25f, UnityEngine.AI.NavMesh.AllAreas) || UnityEngine.AI.NavMesh.SamplePosition(new Vector3(at.x, at.y - .5f, at.z), out _, .25f, UnityEngine.AI.NavMesh.AllAreas);
                float slope = Physics.Raycast(at + Vector3.up, Vector3.down, out var hit, 4, 1 << 6, QueryTriggerInteraction.Ignore) ? Vector3.Angle(hit.normal, Vector3.up) : float.NaN;
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, " (hips at {0:0.00} {1:0.00}; walked ground {2}; slope {3:0} degrees)", at.x, at.z, walked, slope);
            }
            OwnBody.Told = said => { if (said.Contains("gave up") || said.Contains("on its feet") || said.Contains("goes down") || said.Contains("no ground") || said.Contains("could not") || said.Contains("risen,") || said.Contains("lost:") || said.Contains("down:")) Debug.Log("OWN_TOLD " + said + Where()); };
            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Small") index = i;
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner called Small.");
            choice.Choose(index);
            yield return Wait(.3f);
            var unit = choice.Current;
            look.SetOwn(true);
            // (Its body becomes its own once it stands at ease.)
            for (float until = Time.time + 20; (own == null || !own.Stands) && Time.time < until;) { unit.TryGetComponent(out own); yield return null; }
            Assert.That(own != null && own.Stands, Is.True, "Small did not come to stand by its own joints.");
            Assert.That(own.KnowsAWayUp, Is.True, "Small knows no way up (Resources/RiseWays/Small.txt).");
            float pushes = 8;
            string given = CaptureTools.Argument("-ownPushes");
            if (!string.IsNullOrEmpty(given)) pushes = float.Parse(given, System.Globalization.CultureInfo.InvariantCulture);
            int byItsOwn = 0, oldWay = 0, never = 0, triesAll = 0;
            for (int k = 0; k < pushes; k++)
            {
                // (Standing by its own joints before each push.)
                for (float until = Time.time + 15; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                yield return Wait(1.5f);
                var fall = unit.GetComponent<PhysicalFall>();
                int roseBefore = fall != null ? fall.RoseByItsOwn : 0, feetBefore = own.OnItsFeetFromLying, oldBefore = fall != null ? fall.GotUp : 0;
                // (Forward, so that it falls onto its front; a little aside, each push its own.)
                // (-ownPushBack 1: pushed the other way. -ownPushWays 0,180,90,270: so many degrees round from straight
                // ahead, one push after another in turn, each a little aside as well.)
                bool back = CaptureTools.Argument("-ownPushBack") == "1";
                float round = back ? 180 : 0;
                string ways = CaptureTools.Argument("-ownPushWays");
                if (!string.IsNullOrEmpty(ways))
                {
                    var each = ways.Split(',');
                    round = float.Parse(each[k % each.Length], System.Globalization.CultureInfo.InvariantCulture);
                }
                Vector3 way = Quaternion.AngleAxis(round + ((k / Mathf.Max(1, string.IsNullOrEmpty(ways) ? 1 : ways.Split(',').Length)) % 3 - 1) * 20, Vector3.up) * unit.transform.forward;
                // (-ownGoOn 1: waited for, and passed over if it still cannot be, so that the pushes go on.)
                if (CaptureTools.Argument("-ownGoOn") == "1")
                {
                    for (float until = Time.time + 20; !look.CanPushOver && Time.time < until;) yield return null;
                    if (!look.CanPushOver) { Debug.Log("OWN_UP push " + k + ": it could not be pushed over; passed over"); continue; }
                }
                Assert.That(look.CanPushOver, Is.True, "Small cannot be pushed over (push " + k + ").");
                look.PushOver(way);
                fall = unit.GetComponent<PhysicalFall>();
                float began = Time.time;
                bool lay = false;
                string lying = "";
                int triesSeen = 0;
                // (Up, by either body, and standing still: by its own joints, done getting up; or the posed body up.)
                bool Up() => fall.Now == PhysicalFall.State.Up && (own.Stands ? !own.GettingUp && !own.Rising : true);
                while (Time.time - began < 2 || (!Up() && Time.time - began < 300))
                {
                    lay |= fall.Now == PhysicalFall.State.Lying;
                    // (How it lay as each try began.)
                    if (fall.RoseByItsOwn - roseBefore > triesSeen)
                    {
                        triesSeen = fall.RoseByItsOwn - roseBefore;
                        lying += (lying.Length > 0 ? ", " : "") + fall.LayAtTry.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    yield return null;
                }
                int tries = fall.RoseByItsOwn - roseBefore, feet = own.OnItsFeetFromLying - feetBefore;
                // (Up by its own body only standing so at the end: on its feet once and down again, in a try at the end,
                // is not up. October 27.)
                bool itsOwn = feet > 0 && own.Stands && Up();
                bool old = !itsOwn && fall.GotUp > oldBefore;
                if (itsOwn) byItsOwn++; else if (old) oldWay++; else never++;
                triesAll += tries;
                Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "OWN_UP push {0} ({6:0} degrees): it lay {1}; tries by its own body {2} (its chest to the ground at each: {7}); {3} after {4:0.0} s; the fall said: {5}",
                    k, lay, tries, itsOwn ? "up by its own body" : old ? "up the old way" : "not up", Time.time - began, fall.ByItsOwnSaid, round, lying));
                // (-ownGoOn 1: not up at the end, the posed body takes it back where it lies and stands, so that the pushes
                // go on: to measure, not as the game does.)
                if (!Up() && CaptureTools.Argument("-ownGoOn") == "1")
                {
                    if (own.Stands) own.Drop();
                    fall.TakeBack();
                    for (float until = Time.time + 20; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                }
            }
            Debug.Log($"OWN_UP Small, {pushes} pushes: up by its own body {byItsOwn}, the old way {oldWay}, not up {never}; tries by its own body {triesAll}");
            Assert.That(never, Is.EqualTo(0), "Small did not get up after a push.");
        }
    }
}
