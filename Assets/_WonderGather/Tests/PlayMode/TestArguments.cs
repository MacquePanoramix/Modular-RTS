using NUnit.Framework;

namespace WonderGather.Tests
{
    // Arguments that set how every test's bodies are made, for the whole suite at once (November 3, 2026):
    // -strengthRule muscle: strength from muscle (PhysicalBody.Rule; Luis, November 3), to see what of the game it changes.
    [SetUpFixture]
    public sealed class TestArguments
    {
        public static PhysicalBody.Rules StrengthRule => CaptureTools.Argument("-strengthRule") == "muscle" ? PhysicalBody.Rules.Muscle : PhysicalBody.Rules.Need;

        [OneTimeSetUp] public void Begin() => PhysicalBody.Rule = StrengthRule;
        [OneTimeTearDown] public void End() => PhysicalBody.Rule = PhysicalBody.Rules.Need;
    }
}
