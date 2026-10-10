using UnityEngine;

namespace WonderGather
{
    // How a body breathes: a clock of its own (Docs/Research/2026-10-09_AliveAndTheBodysOwn.md, part 6). At rest
    // its chest fills over the first two fifths of a breath, empties quicker at first and then slower, and waits a
    // moment empty. Out of breath it breathes faster and deeper, in and out alike, with no wait. No two breaths are
    // quite as long or as deep.
    public struct Breath
    {
        // Breaths a second at rest, and wholly out of breath; how many times deeper it breathes then.
        public const float AtRest = .25f, OutOfBreath = .7f, Deeper = 2.5f;
        // The share of a breath at rest that fills the chest, and that waits empty; how much one breath differs
        // from the next (a share, either way).
        private const float FillsAtRest = .4f, WaitsAtRest = .15f, Differs = .1f;
        private float along, pace, depth;
        private uint chance;

        // How full its chest is (0: empty; 1: full), how deep this breath is (1: a breath at rest), how many it
        // takes a minute now, and how many it has taken.
        public float Full { get; private set; }
        public float Deep { get; private set; }
        public float AMinute { get; private set; }
        public int Taken { get; private set; }

        private float Chance() { chance = chance * 1664525u + 1013904223u; return (chance >> 8) / 16777216f; }

        // A number of a body's own, from its name (the same each time the game is run).
        public static int SeedOf(string name)
        {
            int seed = 17;
            foreach (char letter in name) seed = unchecked(seed * 31 + letter);
            return seed;
        }

        // It begins somewhere in a breath of its own (so that no two bodies breathe together).
        public void Begin(int seed)
        {
            chance = unchecked((uint)seed * 2654435761u + 12345u);
            along = Chance();
            pace = depth = 1;
            Taken = 0;
            Goes(0, 0);
        }

        public void Goes(float dt, float outOfBreath)
        {
            float o = Mathf.Clamp01(outOfBreath);
            if (pace <= 0) pace = depth = 1;
            float rate = Mathf.Lerp(AtRest, OutOfBreath, o) * pace;
            AMinute = 60 * rate;
            along += rate * dt;
            if (along >= 1)
            {
                along -= Mathf.Floor(along);
                pace = 1 + Differs * (2 * Chance() - 1);
                depth = 1 + Differs * (2 * Chance() - 1);
                Taken++;
            }
            float fills = Mathf.Lerp(FillsAtRest, .5f, o), waits = Mathf.Lerp(WaitsAtRest, 0, o);
            if (along < fills) Full = Mathf.SmoothStep(0, 1, along / fills);
            else
            {
                float u = Mathf.Clamp01((along - fills) / Mathf.Max(.01f, 1 - fills - waits));
                Full = 1 - Mathf.SmoothStep(0, 1, 1 - Mathf.Pow(1 - u, 1.5f));
            }
            Deep = depth * Mathf.Lerp(1, Deeper, o);
        }
    }
}
