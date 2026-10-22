using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The third path (Luis, October 22, 2026): teaching what learns the moment from hands and feet to the squat
    // (RiseAgent), in the editor, the way the bench runs: RiseSchool.Teach, with its numbers from the command line
    // (-trainWays <ways file>, and the rest there). Run with the trainer listening first (mlagents-learn; Docs/Technical/
    // RiseWays/README.md on the work branch); not a test that must pass.
    public sealed class RiseTraining
    {
        [UnityTest, Explicit, Timeout(86400000)]
        public IEnumerator Train()
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.That(RiseSchool.Argument("-trainWays"), Is.Not.Null.And.Not.Empty, "-trainWays <file>");
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            CaptureTools.AsItWas();
            yield return null;
            yield return RiseSchool.Teach(said => Debug.Log(said));
        }
    }
}
