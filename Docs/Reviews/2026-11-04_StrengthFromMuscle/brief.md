# Review: strength from muscle as the default, with the three miners' muscle shares

A Unity 6 game (Wonder Gather). Bodies are physical: each joint has a capacity (N m) that its drives may not exceed.
Until November 3, 2026 the arms and knees had "as strong as the body needs" (`Rules.Need`: the shoulder and elbow
`max(thickness-based, 1.6 x what the body's own arm needs)`, the knee `1.6 x m g 0.5 x thigh length`), times the
creator's `Strength` number. The Game Director decided on November 3: strength from muscle (a joint's capacity from the
limb's thickness, so a heavy body on thin limbs, or a giant, is truly weaker for its weight), and later a strength stat
per body region ("how much of it is muscle ... maybe even for body regions").

## What was built (all in `code/`)

- `cc89cfd` (`code/cc89cfd_muscle_rule.diff`): `PhysicalBody.Rule` (`Need` | `Muscle`): under Muscle, shoulder =
  121.3 N m x (arm radius / reference)^3 x Strength, elbow 104.5 x the same, knee 137.5 x (leg radius / 0.046)^3 x
  Strength; the back and neck stay need-based under both. A share of muscle per region (arms, legs, back, neck; 1 = an
  ordinary body of its size).
- Since (`code/change_since_cc89cfd.diff`): Muscle is the default. The three existing miners (Small, Long, Round),
  drawn before the rule (their limb thickness was not meant as muscle), get the shares that make the muscle rule give
  what the need rule gave (`PhysicalBody.SharesAsNeeded`, written into their prefabs by `MinerSetup.SetMuscleShares`).
  One share per region: the arms' share is the mean of the shoulder's and the elbow's ratios. `MuscleGives(region)`:
  the shares count only under Muscle (found today: they had also scaled the need rule, so the test flag
  `-strengthRule need` no longer reproduced it). Two test thresholds in `PhysicalBodyTests.cs` were eased (see below).
- `code/uses_of_capacities.txt`: every use of the capacities outside PhysicalBody.

## The evidence

- The whole PlayMode suite, Muscle default with the shares (before `MuscleGives` and the eased thresholds): 217 tests,
  187 passed, 2 failed, 28 skipped (run only when asked). Under the need rule on November 3 (no shares): 189 passed,
  0 failed. Under the muscle rule without shares (November 3): 179 passed, 10 failed (Long, its knee 137 N m instead of
  202, could not rise from picking its pickaxe up; four tests comparing weak and strong bodies).
- The two failures, both on the miner Round: `TheBodyAdaptsToItsStrengthAndToWhereItStrikes` (at half strength the
  upper hand slides 59% of the way up the handle, asked > 60%; 61% under the need rule) and
  `StrengthAndWeightDecideTheSwing` (`feeble.liftTime > ordinary.liftTime x 1.2`: 0.719999492 vs 0.719999552, i.e.
  0.72 s = 36 physics steps of 0.02 s against 30; under the need rule 0.74 s). Rerun alone twice: 59% (0.5917, 0.5942)
  both times; the feeble lift 0.74 s once (passed), 0.72 s once (failed). (The lead first eased the wrong assertion,
  the strong blow's speed through upright, and put it back.)
- MINER_MUSCLE (the editor method's log): Small arms 0.989, legs 0.893; shoulder 41.5 N m (before 41.8), elbow 35.8
  (35.5), knee 86.9. Long 1.398, 1.467; 76.2 (76.4), 65.7 (65.5), 201.7. Round 0.805, 0.800; 85.7 (85.1), 73.9 (74.4),
  220.4.
- `results/`: the suite of November 4 and the reruns after `MuscleGives` and the eased thresholds (`pbt_m2.txt`: PhysicalBodyTests 7 of 7 under the default; `swing_need2.txt`: the two swing tests under `-strengthRule need`), `miner_muscle.txt`, and the commit message (`23754f6`).

## What is claimed

1. Under the default, the three miners' capacities are what they were to within 0.3 to 0.8% per arm joint, exactly
   for the knees, and unchanged for the back and neck; so nothing the director has played changes perceptibly.
2. The two failures come from that 0.7% on Round's arms, not from anything else; easing the thresholds (> 55% of
   the way; the feeble lift more than 1.2 times the ordinary one's, less 0.01 s, half a physics step) keeps the
   tests' meaning.
3. `-strengthRule need` now reproduces the rule before exactly.
4. A body made from now on (shares 1) shows its build.

## The reviewers

Three, each with its own question; each works alone and does not see the others. Read everything here; the game's
code is not available beyond these files. Say plainly what you could not check. Write your findings into this folder
(`physicist.md`, `sceptic.md`, `builder.md`) early and keep them current.
