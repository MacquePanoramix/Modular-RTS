# Handoff: where the work stands, and how to resume it

**Last updated:** November 11, 2026, by Claude Fable 5.2. Kept current at every milestone.

If you are a new session or another model: read `AGENTS.md`, the top entry of [CURRENT_STATE.md](CURRENT_STATE.md), then
this page. Luis's preferences and the lessons already paid for are in the assistant's memory directory (its index is
`MEMORY.md`; it is loaded into every session of this project).

## The arrangement

- **Luis is the Game Director.** His principle, in his words (November 9): "I prioritize quality above everything
  else. Speed of development should never sacrifice any of the quality of the project and of the ideas and goals and
  ambitions" ([the message](Correspondence/2026-11-09_QUALITY_ABOVE_EVERYTHING_AND_FABLE.md)).
- **The model.** Since November 9 the session runs on Claude Fable 5.2 at max thinking. It is included only up to half
  of the plan's limits, until November 16, and it draws usage much faster than Opus 5.5. After that, or whenever Luis
  says, the work goes on with Opus 5.5 at max thinking. Subagents take the session's model unless told otherwise: pass
  `model: 'opus'` for builders and reviewers and `model: 'sonnet'` for judges.
- **The conversation is kept small** (Luis, November 10: [his message](Correspondence/2026-11-10_COMPACT_WHEN_IT_COSTS.md)).
  Claude Code compacts it by itself at 250,000 tokens (`autoCompactWindow` in `.claude/settings.json` and
  `.claude/settings.local.json`). So everything that matters is written here and in the repository before a long
  stretch, and nothing is kept only in the conversation or the scratchpad.
- **What the stronger model is spent on:** the checked design of the motion (done: `Design/AnyBody/motion_design.md`,
  sections 6 and 7); root causes inside the engine (the flying boot: done, the design's K1); and the ground of the new
  controller (steps 0 and 2 of the design's section 6.3), where a wrong foundation would cost most. Everything else is
  written so that any model can take it up from the steps below.

## Where things are

| What | Where |
|---|---|
| Documents, designs, correspondence, reviews, pictures | the main project, `D:/Programs/Unity/Projects/Wonder Gather/The Wanderer`, branch `claude/worker-showcase` (pushed) |
| Code and tests | the worktree `D:/Dev/WG`, branch `claude/wip-round` (pushed). Unity (6000.6.0f1) is run in batch mode there only |
| Run scripts | `D:/Dev/WG/Docs/Technical/AnyBody/tools/` (`unity_tests.sh`, `shoves.sh`, `testresults.py`, a README with the runs used most); test output in `D:/Dev/WG/TestRuns/` (ignored by git) |
| The stance solver | `D:/Dev/WG/Assets/_WonderGather/Scripts/Units/Rising/AnyBody` (one source for Unity and for the console checks in `Docs/Technical/AnyBody/port/tests`) |
| The newest build Luis has | October 30 (`getting-up-2026-10-30`, a GitHub release). Nothing since changes what he plays |

Luis's own uncommitted files in the main project are never committed, reset or discarded:
`Assets/_WonderGather/Scenes/TheGroup.unity`, `ProjectSettings/PackageManagerSettings.asset`,
`ProjectSettings/URPProjectSettings.asset`, `Assets/_Recovery*`. In the worktree, `ProjectSettings/EditorBuildSettings.asset`
and `ProjectSettings.asset` stay out of commits. Stage by path.

## The task, and its state

S3b step 2, widened by Luis to the final game's getting up: any body the creator can make, by its own joints, never a
played animation ([the vision's rows of October 30 to November 9](GAME_VISION.md)). It is built in layers:

| Layer | State | Read |
|---|---|---|
| 0, the body (each joint's frame and range) | built behind `OwnBody.Anatomical` (off), for a body made standing only; 9 engine facts tested. The weighed centres (`OwnBody.Weighed`, off) break standing and wait | `Design/AnyBody/joint_model.md` (its head has the results) |
| 1, the stances fitted to a body, and the reserve S | in the game (nothing in play calls it); free limbs rest and resting hands, by Luis's word; drawn on the three miners and judged twice; ten made bodies fitted | `D:/Dev/WG/Docs/Technical/AnyBody/INTEGRATION.md`; `Reviews/2026-11-04_StanceAtlas_Judges`, `Reviews/2026-11-09_DrawnStances_Judges` |
| 2 and 3, the way up and the controller | **designed and checked, not built.** Sections 6 and 7 of the design stand over sections 0 to 5 | `Design/AnyBody/motion_design.md`; the two checks beside it (`motion_check_code.md`, `motion_check_decisions.md`) |
| Strength | from muscle, the default; the three miners keep each joint's strength exactly | `Design/AnyBody/body_map.md` §1.5; `Reviews/2026-11-04_StrengthFromMuscle` |

## Waiting on Luis

Nothing. On November 11 he answered the six questions of `motion_design.md` section 7: **yes to all six, as
recommended, "and please proceed"** ([his message](Correspondence/2026-11-11_YES_TO_THE_SIX_AND_PROCEED.md); the row of
November 11 in `GAME_VISION.md`). The design is agreed and may be built. He adds that "a miner falling forward should
still also be able to get up": the fall onto the front is built and tested as fully as the common one (forced in the
runs, since it is the rare fall). Luis waits on motion: the first clips come after step 2.

## In hand (November 10, evening): finish these first

1. **(Done, November 10, evening: the design's K1, K3, K5, its step 0 and its section 7 corrected.)** Two agents reviewed the flying boot's diagnosis
   ([their findings](Reviews/2026-11-10_FlyingBoot_K1/review.md)): the core holds (the count is coded rightly, it
   balances before a boot gives way, and the body then gains several times what its joints are counted to give, also in
   steps where nothing touches the ground), but K1 as committed in `66a602a` says more than the logs show. To change in
   `motion_design.md`: (a) the game's dampers alone do not hold a leg at 60 rad/s, its springs and dampers together do;
   (b) the pushed leg ran away at 30 rad/s but not at 60 or 120, so "a drive can take no more than its strength" is not
   shown; (c) 0.7·(speed × step)² is what the first step adds, not a steady rate (over a second: 0.4% a step at 10 rad/s,
   3% at 20, 16% at 30); (d) the worked-out torques do start it or feed it (up to 150 J a step; in one course 87 of the
   first 94 J); (e) the leg swings against its hip's drive, not against the hip's worked-out torque; (f) the third way in
   is "given 117 of 154 N m one step after it bore nothing", and the claim is limited to the courses checked (Round's
   course under the other solver is not explained); (g) R1: the target is the goal plus torque over spring and does not
   follow the joint, and R1 holds only together with K5's damper and R2; (h) R2: a share is planned by the design's §2.4
   and bounded by what the touch can bear, the pyramids at the real contact points, and `GroundTouch` must keep each
   contact's point, impulse and normal; (i) a rule R5 for the hand-over to the standing balance (§2.10): from there R1 to
   R3 do not hold, both soles flat is asked at the gate, and `BodyWatch` marks a fault there as the keeper's; (j) K5's
   speed bound as a test with stated quantities, and which speed the tempo rule uses; (k) section 7's paragraph for Luis:
   two guards on the standing balance and the other solver were tried, one run each; the engine adds energy to a fast
   leg that nothing damps. Then the validation log and the keeper's note at `OwnKeeper.Braced` to match.
2. **(Done, November 10, evening: the count takes true speeds and the mean of each step; `ADrivesForceIsWhatItTakes`
   added; Small's course recorded again; K1 holds the numbers.)** Make the count better (work branch): a torque's work as the torque times the mean of the joint's turning at the
   start and the end of the step (the end alone is off by tens of joules where a light part is pushed hard); the fastest
   of the other parts in the record; a fact that a drive's reported force is what it took when the drive is at its
   limit. Then record Small's course again.
3. **The stiff-ankle fact** (K1's R4; `AStifflySprungAnkleHoldsALightBoot` in the work branch's
   `ArticulationFactsTests.cs`: **run once under the game's solver, no bar yet**. First look: standing flat under
   Small's weight the boot stays within 0.1 degree and does not turn, with a stiff ankle (100 and 300 N m a radian) as
   with the soft one, asked for nothing, for 12 and 30 N m through its drive, and for the same put straight on it. But
   the drive gives only 4 to 26 N m of the 12 to 30 asked through it, and where the ground bears the boot was not read
   in those cases (the body was most likely asleep, which stops the touches being told: keep it awake): the load
   through the target is not yet shown to arrive.) Next: give it
   the cases and the bar the review proposes (its item "design 9": on an edge, off the ground, dropped; the three
   miners' boots; both solvers).
   **State on November 11 (night): the first run under the game's solver is read; the rig is being rebuilt once more
   before the second solver is run.** The test stands on the three miners' real boots (read from the game on
   November 11 by the shove test's `OWN_SHOVE_PARTS` line: Small's boot 1.29 kg, sole 10 x 6 x 15.7 cm, ankle 5.5 cm
   above the boot's centre and 6 mm behind its middle, 61 N m, top 19.9 rad/s; Long's 1.83 kg, sole 18.7 cm long,
   141 N m, 16.1 rad/s; Round's 1.55 kg, sole 17.2 cm, 154 N m, 18.5 rad/s), thirteen cases a miner, the ankle as
   the design has it (barred), three and ten times as stiff, and on Small the standing balance's own ankle with the
   torque put straight on it (told beside it). The bar is written in the test's own comment, set before the run.
   **What the run `facts_r4_pgs` showed** (`D:/Dev/WG/TestRuns/facts_r4_pgs.log`, lines `FACTS R4`; PGS, 14 and 4
   turns of the solver): it compiled; the rig's probes came out as expected (a target counted up turns the toe down
   and the right edge up, 10.00 degrees reached, a boot made bent 22 degrees stays as made, touches told within the
   step). **The boot is quiet at every stiffness** (design, 3x, 10x: no flying, never near its top speed on the
   ground): the stability half of R4 holds under PGS. **34 misses of the bar, of three causes:**
   (a) **A law (mine; it fits the nine cases asked towards the toe within about 1%):** against a part the ground
   holds still, a drive gives in a step only `1 - (1 - x)^n` of what its spring says, with
   `x = 1 / (1 + dt (dt k + c) / I)`, `I` the part's turning weight about its joint (about its centre plus mass times
   the distance squared: Small's boot 0.0069 kg m2 towards the toe, 0.0054 sideways; Long's 0.0115; Round's 0.0091)
   and `n = 14 + 4 = 18` (to be confirmed by changing the turns). Asked two thirds of what the sole bears towards the
   toe, the design's ankle gave 69% of it on Small (15.6 of 22.5 N m), 53% on Long (20.4 of 38.5), 45% on Round
   (23.8 of 53.3); three times as stiff 48 / 36 / 29%; ten times 23 / 18 / 14%. The stiffer and the more damped
   against a light part, the less arrives; the K5 damper (strength over top speed) is the larger share of it. The
   drive's told force and where the ground bears the boot agree, so the told force is true and the shortfall real.
   (b) **The rig pinned the ankle sideways** (the weight on an upright slide only): the ground's grip and the slide's
   lock then hold forces between them, so where the ground bears the boot is not settled by the torque alone (about
   10 mm across and 7 to 10 mm along with nothing asked), a boot made bent stays bent, and five thirds of what the
   sole bears does not roll it. Not an engine finding: the rig's fault.
   (c) **My count of energy used the told joint speed**, which for a part at rest on the ground is not real motion
   (0.1 to 1.5 rad/s told while the boot does not move): the drive's work has to be counted from the joint's real
   change of position.
   **Done since (November 11, late night): the rig rebuilt and run under both solvers; both runs read.** Worktree
   files still uncommitted: `ArticulationFactsTests.cs`, `OwnBodyTests.cs`, `Docs/Technical/AnyBody/tools/r4_table.py`.
   Logs: `D:/Dev/WG/TestRuns/facts_r4_pgs2.log` and `facts_r4_tgs.log` (lines `FACTS R4`; a table of either:
   `python -I Docs/Technical/AnyBody/tools/r4_table.py <log> flat|rest [filter]`). The rig now: the weight on three
   free slides, never turning; the drive's work counted from the joint's change of place; a probe of the told force
   (13.3 N m put straight on a boot held in the air); the flat cases also at (28, 4), (14, 18) and (60, 4) turns of
   the solver, told and not barred; "flat, all its strength sideways" in place of five thirds. The bar was not moved:
   the test fails it under both solvers (PGS 33 misses, TGS 50).
   **Read, PGS (the game's solver):**
   - The told force is true: in the air the boot gives way 7.63 degrees under 13.3 N m (the torque over the spring is
     7.63) and its drive tells 13.30.
   - **The law is confirmed by the turns:** (28, 4) and (14, 18) give the same (n is the sum of both kinds of turns)
     and (60, 4) follows it. Given towards the toe when asked two thirds of what the sole bears, design ankle: Small
     16.1 of 22.5 N m at 14+4, 20.1 at 28+4, 22.4 at 60+4; Long 21.7 / 29.3 / 36.6 of 38.5; Round 26.1 / 36.3 / 48.1
     of 53.3. (The told number is the size over three lines, so it carries what the ankle gives with nothing asked;
     where the ground bears the boot times the load, less its place with nothing asked, gives the law's number
     within about 2%.)
   - **New: with nothing asked a flat boot does not sit flat at 14+4 turns.** It tips 1.6 (Small), 1.7 (Long), 3.8
     (Round) degrees, its drive gives 2.1 / 3.5 / 6.1 N m, and a boot made bent 22 degrees comes back only to 3 to 6
     degrees from its zero. It shrinks with the turns of place only (28+4: 0.7 / 0.7 / 1.5 degrees; 60+4: 0.24 / 0.27
     / 0.58; 14+18: 1.5 / 1.6 / 3.3). My reading, not shown: the heavy weight on the light boot is not solved to the
     end in 14 turns of place (this kind of solver's known trouble with a heavy thing on a light one).
   - **Stable everywhere, at design, 3x and 10x:** never faster than the ankle's top speed (most at design 11.3
     rad/s, on a drop; at 10x 16.0 to 16.4 springing back from 22 degrees, Long's top being 16.1), never more than
     0.2 J gained in sum, the drive's work in a step at most 1.4 J (Small), 2.2 (Long), 3.0 (Round), far under the
     stroke. What the bar still counts as misses there: the tips with nothing asked (above); a told turning of 0.5
     to 0.9 rad/s at the end in five cases on an edge (told speed: whether the boot moves there was not read from
     its change of place); 1 to 3 J more in one step on Long and Round at a landing or when set on an edge (my
     reading, not shown: the ground pushing the body out of the millimetres it sank in; the bar's 1 J did not grow
     with the body's weight).
   **Read, TGS:**
   - It gives hardly more at the game's turns and gains less from more turns: towards the toe Small 17.3 of 22.5 N m
     (28+4: 19.3; 60+4: 20.8), Long 23.1 of 38.5 (28.0; 32.6), Round 28.8 of 53.3 (36.4; 43.1). Three and ten times
     as stiff changes little there (Small 16.8, 15.3): its law is another one, not worked out. (14, 18) gives the
     very same numbers as (28, 4) to the last digit (my recollection of the engine's notes: TGS turns the turns of
     speed beyond 4 into turns of place).
   - A flat boot sits flat (0.2 to 0.3 degrees with nothing asked, the drive gives 0.4 to 0.6 N m) and a bent one
     comes back to within 0.3 degrees.
   - **But the boot flies at landings:** told turning above the ankle's top speed in five of the design's cases
     (Round dropped 5 cm: 76.5 rad/s, 25.9 J more in one step, 14.7 J gained in all; Round dropped leant 50.7; Long
     dropped leant 32.7; on an edge pushed onto it 22.2 and 24.1), more at 3x and 10x (to 68.6). Told turning at
     rest is 0.7 to 4.3 rad/s and grows with the turns. In the air under 13.3 N m put straight on it the boot gives
     way 5.92 degrees, not 7.63, while telling 13.30 (my reading, not shown: the small steps move the light boot
     within the step before the drive stops it).
   **The engine's own source read (November 12, morning; by me, from the files the review's skeptic had fetched
   before it was cut off): PhysX 4.1, `DyFeatherstoneArticulation.cpp`** (public: `NVIDIAGameWorks/PhysX`, branch
   `4.1`, `physx/source/lowleveldynamics/src/`; the copy in the session's scratchpad is not kept. Unity's own build
   of the engine may differ, so the runs are the proof and the source the explanation). For each driven axis
   `setupInternalConstraintsRecursive` (about line 2590) reckons once a step `a = dt (dt k + c)` and
   `x = 1 / (1 + a R)`, `R` being the joint's response with nothing else holding it (for a light end part, 1 over
   its turning weight about the joint), and sets the drive's summed impulse to zero (nothing is carried over from
   the step before). `solveInternalConstraintRecursive` (about line 4170) then does, at every turn of the solver,
   turns of place and turns of speed alike:
   - **PGS:** `sum = (1 - x) sum + x (dt k error + dt c wantedSpeed - a jointSpeed)`, clamped to the force limit.
     For a free joint the first turn gives the whole impulse of an implicit spring and later turns change nothing
     (so K5's damper bounds a free part's speed as designed). For a part held still the sum creeps up: after `n`
     turns `1 - (1 - x)^n` of what the spring says. **This is the law, as it was fitted.**
   - **TGS:** `a` and `x` are made with the substep `h = dt / turns of place`, the error is read anew each substep,
     and `sum = sum + x (...)`, nothing taken off. So each substep gives its own impulse and each turn of speed one
     more: **a free spring acts `(place + speed) / place` times as strong as set** (at 14 and 4: 1.29 times; the
     rig's probe gave way 5.92 degrees, and 7.63 / 1.2857 = 5.93), and a held part gets
     `(place + speed) / place / (1 + h (h k + c) / I)`. Against the twelve flat readings of the TGS run this is
     within 1%: towards the toe at 14, 28, 60 turns, law / read: Small 17.4 / 17.3, 19.4 / 19.3, 20.9 / 20.8; Long
     23.3 / 23.1, 28.3 / 28.0, 32.7 / 32.6; Round 29.0 / 28.8, 36.5 / 36.4, 43.5 / 43.1; sideways at 14: 9.2 / 9.2,
     9.1 / 9.0, 12.6 / 12.8.
   Also in that source: **a drive's target is clamped to the joint's limits** (about line 2575), so near a limit
   what can be asked towards it falls to nothing however far the target is set. The share a held boot gets under
   PGS at 14 and 4, by the law: Small 0.70 towards the toe and 0.61 sideways, Long 0.53 and 0.40, Round 0.45 and
   0.35; read: 0.72 and 0.79, 0.56 and 0.58, 0.49 and 0.57 (more than the law, most of all sideways: the boot is
   not quite held, it rolls a little). The damper is the greater part of it: with none the law gives 0.94 / 0.88 /
   0.79 towards the toe. At 28 and 4 the law gives 0.88 / 0.74 / 0.66, at 60 and 4 0.99 / 0.93 / 0.88. For the
   body's other joints the law gives 1 (a shin with its boot, or a forearm, turns as 0.05 kg m² and more about its
   joint: reckoned, not run).

   **What more turns cost (November 11, night):** `OwnBodyTests.WhatMoreTurnsOfTheSolverCost` (explicit, a fact
   with no bar; `TestRuns/turns_cost.log`, lines `FACTS TURNS`): twenty copies of a miner's own body (13 parts)
   lying awake on the ground, the engine stepped by hand, in the editor. A body costs, in microseconds a step at
   14 and 4, 28 and 4, 60 and 4 turns: Small 111 / 172 / 304, Long 125 / 192 / 351, Round 106 / 163 / 293. So 60
   turns cost 2.7 to 2.8 times what 14 cost; twenty bodies at 60 turns take 6 to 7 ms of a step. (Its "with no
   body: 0.000 ms" only says that an empty scene costs nothing worth the name.) Not measured: standing bodies, a
   player build, what more turns do to the standing balance and its tests.

   **The review was cut off twice by the plan's limit and gave nothing** (November 11, 23:31, and November 12,
   04:01, when the workflow took itself up again after the reset): three reviewers on Opus at high effort at once
   use up a whole 5-hour window of this plan (Pro) in about five minutes, and a reviewer that does not finish
   leaves no word (only its searches, in its transcript: that is how the source above was found already fetched).
   **So a review is sized to finish:** one reviewer at a time, a brief that names the files and the lines and
   holds the numbers to be recomputed, medium effort, a few claims each.

   **So, for R4 (mine; the source and the runs agree; one reviewer still to read it before it goes into the
   design):** under the game's solver an ankle sprung stiffly enough for R1 holds a light boot under the body's
   weight: stable in every case tried, at the design's spring and at 3 and 10 times it. Two things are not as the
   design took them: a held boot gets only the law's share of what its spring says, and a loaded flat boot sits
   1.6 to 3.8 degrees off flat with nothing asked (not explained; less with more turns of place). Under TGS a boot
   flies at landings and every free spring is 1.29 times as strong as set: TGS is no way out. **What I propose:
   nothing of the engine changes** (PGS, 14 and 4 turns, the step, the true weights: so there is nothing to put to
   Luis on these numbers, only to tell him). The controller asks a joint's torque through the target as R1 says
   and, where a part is held, makes up for the share the drive really gave: the drive tells its force truly, so
   what was asked and what was given are compared each step and the target moved by the difference (smoothly; no
   threshold; kept between the law's share for that joint and the whole); the drive's force limit and the target's
   clamp at the joint's limit still bound what arrives. Whether that is done as a share that is read or as a trim
   on the told force is settled by measurement when the controller's first stance is built (step 2). When a body is
   built the law is reckoned for each joint from its spring, its damper and the lighter side's turning weight; a
   joint under 0.9 is marked (the three miners' ankles; a made body's light end parts; the creator's warning can
   use it). More turns for a body stay a lever with a known cost, not used now.
   **To do, in order:** (1) One reviewer, sized as above, on this reading: the two laws against the source's lines
   and the numbers; the proposal against `motion_design.md` §6.2 and Luis's standing decisions. (2) Reshape the
   test into what holds. Under PGS: the law as a fact at three counts of turns (towards the toe the read share
   never under the law and at most a tenth over it); the told force true (the probe in the air); stable at the
   design's spring, at 3 and at 10 times (no told turning over the ankle's top; no gain of energy over a run; a
   landing's one step allowed for by the body's weight; rest read from the change of place, not from told speed);
   the tip with nothing asked told as a fact with its numbers, not a bar. Under TGS the test is not part of the
   suite (the game does not use it); its findings stay in the log and the documents. Run it, read it. (3) Write
   R4's answer into `motion_design.md` (R4; step 0; R1's "through the target" with the share; K5's damper as the
   greater cause; the clamp at the limits), `Validation.md`, `CURRENT_STATE.md` and here; commit and push both
   branches by path (worktree: `ArticulationFactsTests.cs`, `OwnBodyTests.cs`, `tools/r4_table.py`). (4) The rest
   of step 0.
   **Usage on November 12 (morning):** the week's Fable share read 72% (all models 60%; both reset November 16,
   16:00 UTC); a 5-hour window of this plan holds little. Fable for the hardest thinking only, each turn made to
   count; the work may pass to Opus 5.5 at any moment, which is why this page is kept exact.
4. **(Done: reported on November 10; answered on November 11, above.)**

## Next steps, in order

Each step has its evidence in `motion_design.md` §6.3. Report a result only after reading its run.

1. **The flying boot: its cause found (November 10), the standing balance not mended.** Read `motion_design.md` K1
   first: it is the ground the new controller stands on. In short: a boot pushed through while it is not flat on the
   ground is set spinning within one step (the keeper puts its ankle's torque straight on a boot that turns as
   0.003 kg m²), and from there the engine's own step adds energy to a leg that turns fast and that nothing damps (a
   bare leg left alone holds 1900 times as much after a second at 30 rad/s: `ArticulationFactsTests`). Two guards on
   the keeper and the engine's other solver, alone and with both, did not meet the test's bar (`Braced`, `BySpeed`;
   one run each); the switches stay in the code, off.
   **Do not spend more on guarding the keeper.** It is mended by folding it into the new controller (design §2.10).
   The record of any course: `bash Docs/Technical/AnyBody/tools/shoves.sh <name> -ownShoveTrace 1`, then
   `TestRuns/<name>.trace.txt`.
2. **Step 0, the ground of the controller** (design §6.3, K1 to K4). **First** the fact K1's R4 asks for: whether an
   ankle sprung stiffly enough to carry its torque through the drive (full strength at 35 degrees) holds a light boot
   under the body's weight at the game's step, under both solvers (the project's solver is set in
   `ProjectSettings/DynamicsManager.asset`, `m_SolverType`: 0 the game's, 1 the other; change it for a run and put it
   back). If it holds under neither, stop and put the choice to Luis (the boot's turning weight, the step, or the
   solver). Then the other facts (a load carried through the drive's target on a floating body; what
   `GetJointGravityForces` gives on a floating root; a ball joint read about two axes and driven across a half turn);
   how fast each joint may turn (K5: from the count on the bare leg); layer 0 on a body made lying; the one drive
   writer; the smooth sharing of the ground's push (a non-negative least squares over the friction pyramids); touches
   on the trunk, hips, head and upper arms; `BodyWatch` with the count of what the body holds against what its joints
   gave.
3. **Step 1**: `BodyFrame` moved into the game with the pose read off the body, tested lying flat and at the hard poses.
4. **Step 2**: one stance held by its own joints, nudged; then **clips for Luis**.
5. Steps 3 to 8 as the design has them.

Also open, smaller: the bear crawl's stance (it folds into a pike); Long's coat and Round's apron in the deep stances;
the weighed centres' standing balance; the creator's warning; the mining skill (later, with its own research).

## Rules that have cost work before

- A script with any backslash is written as a file with the editor tool, never through a shell heredoc: the shell
  halves doubled backslashes (it bit again twice on November 10).
- No code or test file is edited while an editor run is going; nothing heavy runs beside the whole suite.
- Luis's messages are recorded verbatim in `Correspondence/`, his decisions in `GAME_VISION.md`.
- Every step is reviewed by agents who did not make it (judges for movement and looks; a physicist, a sceptic and a
  builder for technical steps). Their verdicts are not Luis's.
- The first line of a report to Luis names any switch that must be on. A result is said only after its run was read.
- An outlook of the whole project is due every 250 tool calls or so ([Outlook.md](Outlook.md)).
