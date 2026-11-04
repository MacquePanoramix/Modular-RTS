using NUnit.Framework;

namespace WonderGather.Tests
{
    // Arguments that set how every test's bodies are made, for the whole suite at once (November 3, 2026):
    // -strengthRule need: strength as the body needs (PhysicalBody.Rule; the rule before November 3), to compare; else from
    // muscle, as the game has it.
    [SetUpFixture]
    public sealed class TestArguments
    {
        public static PhysicalBody.Rules StrengthRule => CaptureTools.Argument("-strengthRule") == "need" ? PhysicalBody.Rules.Need : PhysicalBody.Rules.Muscle;

        [OneTimeSetUp] public void Begin() => PhysicalBody.Rule = StrengthRule;
        [OneTimeTearDown] public void End() => PhysicalBody.Rule = PhysicalBody.Rules.Muscle;
    }
}
