using NUnit.Framework;

namespace WonderGather.Tests
{
    // Arguments that set how every test's bodies are made, for the whole suite at once (November 3, 2026):
    // -strengthRule need: strength as the body needs (PhysicalBody.Rule; the rule before November 3), to compare; else from
    // muscle, as the game has it.
    // -turnsAsMeant 1: every own body at the solver's turns its code meant, 14 and 4 (OwnBody.TurnsAsMeant, November 13);
    // else the project's 6 and 1, as the game has them unless the switch is turned on.
    [SetUpFixture]
    public sealed class TestArguments
    {
        public static PhysicalBody.Rules StrengthRule => CaptureTools.Argument("-strengthRule") == "need" ? PhysicalBody.Rules.Need : PhysicalBody.Rules.Muscle;
        public static bool TurnsAsMeant => CaptureTools.Argument("-turnsAsMeant") == "1";

        [OneTimeSetUp] public void Begin() { PhysicalBody.Rule = StrengthRule; OwnBody.TurnsAsMeant = TurnsAsMeant; }
        [OneTimeTearDown] public void End() { PhysicalBody.Rule = PhysicalBody.Rules.Muscle; OwnBody.TurnsAsMeant = false; }
    }
}
