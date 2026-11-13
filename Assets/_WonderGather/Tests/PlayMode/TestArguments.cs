using NUnit.Framework;

namespace WonderGather.Tests
{
    // Arguments that set how every test's bodies are made, for the whole suite at once (November 3, 2026):
    // -strengthRule need: strength as the body needs (PhysicalBody.Rule; the rule before November 3), to compare; else from
    // muscle, as the game has it.
    // -turnsAsMeant 0: every own body at the project's 6 and 1 turns of the solver, as the game was played until
    // November 13 (OwnBody.TurnsAsMeant off), to compare; else at the 14 and 4 its code means, as the game has them from
    // the start since Luis chose so (November 13, night). (Until then the other way round: -turnsAsMeant 1 for 14 and 4,
    // which still does so.)
    [SetUpFixture]
    public sealed class TestArguments
    {
        public static PhysicalBody.Rules StrengthRule => CaptureTools.Argument("-strengthRule") == "need" ? PhysicalBody.Rules.Need : PhysicalBody.Rules.Muscle;
        public static bool TurnsAsMeant => CaptureTools.Argument("-turnsAsMeant") != "0";
        // (What the game had before the suite set it, to give it back after.)
        private static bool turnsFound = true;

        [OneTimeSetUp] public void Begin() { PhysicalBody.Rule = StrengthRule; turnsFound = OwnBody.TurnsAsMeant; OwnBody.TurnsAsMeant = TurnsAsMeant; }
        [OneTimeTearDown] public void End() { PhysicalBody.Rule = PhysicalBody.Rules.Muscle; OwnBody.TurnsAsMeant = turnsFound; }
    }
}
