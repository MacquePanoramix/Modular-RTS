using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace WonderGather.Tests
{
    // The bridge from a miner as the game makes it to the stance solver's body (BodyFrame; Docs/Technical/AnyBody/
    // INTEGRATION.md 2.5, B1): for Small, Long and Round, the numbers it reads off the game against those the prototype
    // took from the same miners (AnyBody.BodyFactory.Small, Long, Round), field by field. The prototype's lengths have six
    // or seven figures, its masses two decimals and its strengths whole newton metres.
    public sealed class BodyFrameTests
    {
        private MinerChoice choice;

        [UnitySetUp]
        public IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("TheOrdinaryPlace");
            yield return null;
            CaptureTools.AsItWas();
            choice = Object.FindAnyObjectByType<MinerChoice>();
        }

        [UnityTest]
        public IEnumerator EachMinerReadsAsThePrototypeHasIt()
        {
            var far = new List<string>();
            foreach (string who in new[] { "Small", "Long", "Round" })
            {
                int index = -1;
                for (int i = 0; i < choice.Count; i++) if (choice.NameOf(i) == who) index = i;
                Assert.That(index, Is.GreaterThanOrEqualTo(0), "No miner is called " + who);
                choice.Choose(index);
                float until = Time.time + .2f;
                while (Time.time < until) yield return null;
                var game = BodyFrame.SourceOf(choice.Current.gameObject);
                var proto = AnyBody.BodyFactory.Miner(who);
                var rows = new List<string>();
                void Length(string field, double a, double b) { if (System.Math.Abs(a - b) > 2e-5) far.Add($"{who} {field}: game {a:0.######} against {b:0.######}"); rows.Add($"{field} {a:0.####}"); }
                void Mass(string field, double a, double b) { if (System.Math.Abs(a - b) > .006) far.Add($"{who} {field}: game {a:0.###} kg against {b:0.##}"); rows.Add($"{field} {a:0.###}"); }
                void Strength(string field, double a, double b) { if (System.Math.Abs(a - b) > System.Math.Max(.6, .012 * b)) far.Add($"{who} {field}: game {a:0.#} N m against {b:0}"); rows.Add($"{field} {a:0.#}"); }
                Length("H", game.H, proto.H); Length("WH", game.WH, proto.WH); Length("Leg", game.Leg, proto.Leg); Length("A", game.A, proto.A);
                Length("Hl", game.Hl, proto.Hl); Length("Bl", game.Bl, proto.Bl); Length("Tl", game.Tl, proto.Tl);
                Length("HW", game.HW, proto.HW); Length("TR", game.TR, proto.TR); Length("HeadRise", game.HeadRise, proto.HeadRise);
                Length("Sx", game.Sx, proto.Sx); Length("Sy", game.Sy, proto.Sy); Length("Sz", game.Sz, proto.Sz);
                Length("Lu", game.Lu, proto.Lu); Length("Lf", game.Lf, proto.Lf); Length("BodyFront", game.BodyFront, proto.BodyFront);
                Length("HeadHalf", game.HeadHalf, proto.HeadHalf); Length("Ra", game.Ra, proto.Ra); Length("Rl", game.Rl, proto.Rl);
                if (System.Math.Abs(game.Height - proto.Height) > .01) far.Add($"{who} Height: game {game.Height:0.###} against {proto.Height:0.##}");
                Mass("MHips", game.MHips, proto.MHips); Mass("MTrunk", game.MTrunk, proto.MTrunk); Mass("MHead", game.MHead, proto.MHead);
                for (int s = 0; s < 2; s++)
                {
                    string l = s == 0 ? "L" : "R";
                    Mass("MUa" + l, game.MUa[s], proto.MUa[s]); Mass("MFa" + l, game.MFa[s], proto.MFa[s]); Mass("MTh" + l, game.MTh[s], proto.MTh[s]);
                    Mass("MSh" + l, game.MSh[s], proto.MSh[s]); Mass("MFt" + l, game.MFt[s], proto.MFt[s]); Mass("MTo" + l, game.MTo[s], proto.MTo[s]);
                }
                Strength("CBack", game.CBack, proto.CBack); Strength("CNeck", game.CNeck, proto.CNeck); Strength("CShoulder", game.CShoulder, proto.CShoulder);
                Strength("CElbow", game.CElbow, proto.CElbow); Strength("CHip", game.CHip, proto.CHip); Strength("CKnee", game.CKnee, proto.CKnee);
                Strength("CAnkle", game.CAnkle, proto.CAnkle); Strength("CToe", game.CToe, proto.CToe);
                Debug.Log($"BODYFRAME {who}: " + string.Join(", ", rows));
                // (And the solver's body built from it.)
                var made = AnyBody.BodyFactory.Make(game, new AnyBody.BodyVariant { Name = who, Source = who });
                var expected = AnyBody.BodyFactory.Make(who);
                Assert.That(made.HipHeight, Is.EqualTo(expected.HipHeight).Within(1e-4), who + "'s hip height in the solver's body.");
                Assert.That(made.Mass, Is.EqualTo(expected.Mass).Within(.05), who + "'s mass in the solver's body.");
            }
            Assert.That(far, Is.Empty, "Read off the game, these differ from the prototype's: " + string.Join("; ", far));
        }

        // BodyFrame copies OwnBody's and PhysicalBody's own numbers (private there): they must still be theirs.
        [Test]
        public void TheNumbersBodyFrameCopiesAreTheGamesOwn()
        {
            Assert.That(Constant(typeof(OwnBody), "HipGives"), Is.EqualTo(BodyFrame.HipGives), "OwnBody.HipGives");
            Assert.That(Constant(typeof(OwnBody), "AnkleGives"), Is.EqualTo(BodyFrame.AnkleGives), "OwnBody.AnkleGives");
            Assert.That(Constant(typeof(OwnBody), "NeckHolds"), Is.EqualTo(BodyFrame.NeckHolds), "OwnBody.NeckHolds");
            Assert.That(Constant(typeof(PhysicalBody), "KneeHolds"), Is.EqualTo(BodyFrame.KneeHolds), "PhysicalBody.KneeHolds");
        }

        private static float Constant(System.Type type, string name)
        {
            var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            Assert.That(field, Is.Not.Null, type.Name + " has no " + name);
            return System.Convert.ToSingle(field.GetValue(null));
        }
    }
}
