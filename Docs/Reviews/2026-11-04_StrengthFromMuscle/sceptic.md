# Sceptic review: "nothing the director has played changes perceptibly"

Reviewer: sceptic. Read only README.md, code/, results/. Nothing run in Unity. Arithmetic done by hand from the logged figures (no Python needed).

## Verdict
The claim is true only for the three miner prefabs, at their prefab proportions, as the editor computes them. It is not shown for: other bodies, live woken bodies as played, getting up, the 28 skipped tests, the final commit as a whole, or a build.

## Findings, most important first

1. **Scope hole: every PhysicalBody that is not one of the three prefabs now gets shares of 1 (likely).**
   Rule defaults to Muscle (PhysicalBody.cs, `Rule = Rules.Muscle`) and only `MinerSetup.SetMuscleShares` (loops "Small","Long","Round") writes shares. Any creator-made body, saved body, other prefab (the branch is `worker-showcase`), or scene instance keeps armsMuscle/legsMuscle = 1 and silently drops from "as strong as it needs" to "radius cubed". The Nov 3 run shows what that does: 10 failures, Long's knee 137 vs 202 N m, could not rise. README claim 4 says "a body made from now on", but a body made before today and still in use is changed too. Before believing: list every prefab and scene holding PhysicalBody (grep the script GUID), and every place that calls Configure at run time. This is a silent behaviour change of the kind AGENTS.md says deserves a separate reviewer.

2. **The "same to 0.3-0.8%" evidence is circular and is for the editor pose (sure that it is circular; likely that it is fine).**
   MINER_MUSCLE's "before" values are produced in the same editor call by the Need code (SetMuscleShares lines 532-534), at the prefab's bone positions (ArmNeeds reads world-space bone distances, cached on first use). Nothing compares a woken, spawned miner under Need vs Muscle. The knee is "exact" only against `SavedProportions.legSegment`; the old rule read live `stance.BodyProportions.legSegment`. The code's own comment says BodyProportions can differ from saved (a 2.2 m body's before waking). Anything that calls SetProportions at run time (creator, panel, spawn scale) moved the old knee and arm needs but does not move the new ones, because radii are fixed at Configure. Partial support: the Nov 3 figure for Long (~202 N m live) matches 201.7. No such figure for Small or Round. Round's knee is the least checked.

3. **The 0.7% explanation of the two failures is inferred, not isolated; the fix loosens tests to fit the result (likely).**
   - 61 to 59 percent is a 2-point move for a 0.7% shoulder change, a gain of about 3x. Run-to-run noise is only about 0.3 points (0.5917, 0.5942, 0.5940, 0.5942), so the shift is real and sensitivity near the limit is high. That is what the tests tell beyond the 0.7%: at half strength the system is near its limit, and small capacity changes show up as threshold or step flips (37 to 36 physics steps, 0.74 to 0.72 s, a 2.8% timing change from a 0.7% capacity change).
   - Shoulder and elbow move in opposite directions (Small -0.7%/+0.85%, Round +0.7%/-0.7%; Long -0.3%/+0.3%). So the shoulder:elbow torque ratio changes by about 1.6% (Small), 1.4% (Round), 0.6% (Long). That is a change of shape, not scale. Small's -0.72%/+0.85% slightly exceeds the stated "0.3 to 0.8%".
   - Cheaper and cleaner than easing the thresholds: keep the single public share but store a hidden per-joint trim so each joint is exact. Then neither test needs touching. The test for attribution: apply the trim to Round and see whether 61% and 0.74 s return. If they do, the cause is shown. If not, something else differs (for example the knee, point 2).
   - Easing: 0.60 to 0.55 widens the margin from 1 point to 4 points, chosen after the result. The lift test now accepts exactly 1.2x (36 steps), where the old test rejected it.
   - In swing_need2 (Need) the ordinary swing shows 29% effort, 7.6 m/s, 6 mm; under Muscle it shows 28%, 7.7 m/s, 4 mm. Rounded, but it is the same direction and about 1 point of effort. That is more than 0.7% relative; a question whether the arms' difference is the only one.

4. **Not tested (sure).**
   - The full suite was run only on the pre-`MuscleGives`, pre-easing tree. Re-run: PhysicalBodyTests only (7) and the two swing tests under need. The Muscle path itself is unchanged by `MuscleGives`, so the risk is low, but the final commit 23754f6 has no whole-suite result.
   - The 28 "run only when asked" tests are never run on the new default. Likely they hold the long benches (rise, keeper, stepping).
   - No Need vs Muscle comparison for Small and Long swing/own-pickaxe numbers (PHYSICAL_OWN Long: arms 41%, hands 32 mm off, tool 26 degrees: nearest its limit). Only Round has a need rerun.
   - Strengths other than 1: only Round at 0.5 and the strong/heavy/feeble tests. The rules are linear in Strength for arms and knees, so scaling should hold, but Strength combined with the clamp (0.1..4) and the share clamp is not run. The panel's strength slider is not exercised.
   - Getting up, let-go fall (PhysicalFall lines 409-442 use KneeNow, BackNow, ShoulderOf with `tone`), the keeper and riser. These are bifurcating, knife-edge behaviours, and a 0.7% arm change may flip a rise. Prior work shows Small's arms were once too weak to push the chest off the ground.
   - A player build: serialized fields present, static `Rule` default, no editor-only flip.

5. **Tiredness (likely small).**
   Efforts are 28-41% against `Sustains` 0.18. A 0.7% capacity change moves (effort - 0.18) by about 1.9% (Small, Round) and 1.2% (Long), so the tiring rate changes about 1-2%. Not perceptible alone, but the whole work-tire-rest cycle was not measured on the three miners (the memory note says Long fails first).

6. **Unrelated prefab change (a question).**
   All three prefab diffs add `StaysPut: 0` to a component above PhysicalBody, though SetMuscleShares claims "nothing else touched". Confirm the C# initializer is false, so that missing YAML previously loaded the same value. Otherwise this is an unreviewed behaviour change.

7. **Meaning of the shares (likely, design not defect).**
   Long carries arms 1.40 and legs 1.47 "muscle", Round 0.80. These are calibrations against the old geometry, not muscle. If a later step shows muscle on the body or reports "muscle %", the miners will contradict their look. `SetMuscleShares` / `SharesAsNeeded` re-run on remake also cancels any thickness revision (a cube-law change is silently neutralized for the miners).

8. **Quiet fallbacks in `SharesAsNeeded` (likely).**
   If `!Ready`, `ArmNeeds` leaves shoulderNeeds = -1, so Need = thickness only, and shares come out wrong without an error. A missing stance falls back to 0.4 m. Results are clamped 0.1..4 silently. It also toggles the static `Rule` and restores it (not exception safe).

## Checked and found sound
- Back and neck: `MuscleGives` returns the share only under Muscle, 1 under Need; the miners' back and neck shares are 1, so BackCapacity and the neck torque (OwnBody 635, PhysicalFall 410) are unchanged. OwnBody evaluates the neck at build time under the Rule then in force.
- HoldCapacity and WristCapacity do not depend on Rule, so the hands' hold is unchanged.
- Under Need after the fix, shares no longer apply (`-strengthRule need` claim 3 is consistent with the code; its test is only the two swing tests).
- Prefab shares (0.98920834/0.8930217, 1.3982192/1.4672036, 0.8052469/0.7995244) match the logged MINER_MUSCLE.

## What I would run before believing the claim
1. On woken miners in TheGroup, as played (`-ownAsPlayed 1`): log Shoulder/Elbow/Knee/Back capacity and the neck torque under both rules at Strength 0.5, 1, 2, and again after any panel/creator proportion change. Expect within 1%.
2. PHYSICAL_OWN, swing, and a full mine-tire-rest cycle for Small, Long, Round, under both rules, several repeats (count courses).
3. The rise/keeper/fall benches (the 28 skipped) for all three, both rules, repeated nudges.
4. The trim experiment in finding 3; also perturb Round's shoulder alone by +/-0.7% to see which joint drives the 2 points.
5. A scan of all prefabs, scenes and saves for PhysicalBody instances with shares of 1 (finding 1).
6. The whole suite on 23754f6 with both rules, then a built player.

## Could not check
The game's other code (OwnBody, PhysicalHands, PhysicalFall bodies, the creator, scenes, saves), what ProceduralBiped.SetProportions does at run time, which tests are the 28 skipped, the `StaysPut` initializer, anything at run time.
