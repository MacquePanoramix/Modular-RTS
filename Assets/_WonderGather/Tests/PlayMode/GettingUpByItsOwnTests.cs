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
        public void Restore() { Time.captureFramerate = 0; PhysicalFall.GetsUpByItsOwn = false; }

        private static IEnumerator Wait(float seconds) { for (float until = Time.time + seconds; Time.time < until;) yield return null; }

        [UnityTest, Explicit, Timeout(3600000)]
        public IEnumerator SmallPushedOverGetsUpByItsOwnBody()
        {
            Time.captureFramerate = 50;
            PhysicalFall.GetsUpByItsOwn = true;
            int index = -1;
            for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == "Small") index = i;
            Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner called Small.");
            choice.Choose(index);
            yield return Wait(.3f);
            var unit = choice.Current;
            look.SetOwn(true);
            // (Its body becomes its own once it stands at ease.)
            OwnBody own = null;
            for (float until = Time.time + 20; (own == null || !own.Stands) && Time.time < until;) { unit.TryGetComponent(out own); yield return null; }
            Assert.That(own != null && own.Stands, Is.True, "Small did not come to stand by its own joints.");
            Assert.That(own.KnowsAWayUp, Is.True, "Small knows no way up (Resources/RiseWays/Small.txt).");
            float pushes = 8;
            string given = CaptureTools.Argument("-ownPushes");
            if (!string.IsNullOrEmpty(given)) pushes = float.Parse(given, System.Globalization.CultureInfo.InvariantCulture);
            int byItsOwn = 0, oldWay = 0, never = 0;
            for (int k = 0; k < pushes; k++)
            {
                // (Standing by its own joints before each push.)
                for (float until = Time.time + 15; !(own.Stands && !own.GettingUp && !own.Rising) && Time.time < until;) yield return null;
                yield return Wait(1.5f);
                var fall = unit.GetComponent<PhysicalFall>();
                int roseBefore = fall != null ? fall.RoseByItsOwn : 0, feetBefore = own.OnItsFeetFromLying, oldBefore = fall != null ? fall.GotUp : 0;
                // (Forward, so that it falls onto its front; a little aside, each push its own.)
                // (-ownPushBack 1: pushed the other way.)
                bool back = CaptureTools.Argument("-ownPushBack") == "1";
                Vector3 way = Quaternion.AngleAxis((k % 3 - 1) * 20, Vector3.up) * (back ? -unit.transform.forward : unit.transform.forward);
                Assert.That(look.CanPushOver, Is.True, "Small cannot be pushed over (push " + k + ").");
                look.PushOver(way);
                fall = unit.GetComponent<PhysicalFall>();
                float began = Time.time;
                bool lay = false;
                // (Up, by either body, and standing still: by its own joints, done getting up; or the posed body up.)
                bool Up() => fall.Now == PhysicalFall.State.Up && (own.Stands ? !own.GettingUp && !own.Rising : true);
                while (Time.time - began < 2 || (!Up() && Time.time - began < 120))
                {
                    lay |= fall.Now == PhysicalFall.State.Lying;
                    yield return null;
                }
                int tries = fall.RoseByItsOwn - roseBefore, feet = own.OnItsFeetFromLying - feetBefore;
                bool itsOwn = feet > 0 && own.Stands;
                bool old = !itsOwn && fall.GotUp > oldBefore;
                if (itsOwn) byItsOwn++; else if (old) oldWay++; else never++;
                Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "OWN_UP push {0}: it lay {1}; tries by its own body {2}; {3} after {4:0.0} s; the fall said: {5}",
                    k, lay, tries, itsOwn ? "up by its own body" : old ? "up the old way" : "not up", Time.time - began, fall.ByItsOwnSaid));
            }
            Debug.Log($"OWN_UP Small, {pushes} pushes: up by its own body {byItsOwn}, the old way {oldWay}, not up {never}");
            Assert.That(never, Is.EqualTo(0), "Small did not get up after a push.");
        }
    }
}
