# Handoff: where the work stands, and how to resume it

**Last updated:** November 10, 2026, by Claude Fable 5.2. Kept current at every milestone.

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

The six questions of `motion_design.md` section 7 (put to him on November 10). Steps 0 to 2 below do not depend on his
answers; the route work (steps 4 and 5) does.

## In hand (November 10, evening): finish these first

1. **Correct the design's K1 by its review.** Two agents reviewed the flying boot's diagnosis
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
2. **Make the count better** (work branch): a torque's work as the torque times the mean of the joint's turning at the
   start and the end of the step (the end alone is off by tens of joules where a light part is pushed hard); the fastest
   of the other parts in the record; a fact that a drive's reported force is what it took when the drive is at its
   limit. Then record Small's course again.
3. **The stiff-ankle fact** (K1's R4; `AStifflySprungAnkleHoldsALightBoot` in the worktree's
   `ArticulationFactsTests.cs`: drafted, **not committed, never run, no bar yet**). Run it as it is to see; then give it
   the cases and the bar the review proposes (its item "design 9": on an edge, off the ground, dropped; the three
   miners' boots; both solvers).
4. **Report to Luis** with the six questions of the design's section 7 (not yet put to him).

## Next steps, in order

Each step has its evidence in `motion_design.md` §6.3. Report a result only after reading its run.

1. **The flying boot: its cause found (November 10), the standing balance not mended.** Read `motion_design.md` K1
   first: it is the ground the new controller stands on. In short: the joints do not throw the leg. A boot pushed through
   while it is not flat on the ground is set spinning within one step (the keeper puts its ankle's torque straight on a
   boot that turns as 0.003 kg m²), and from there the engine's own step adds energy to whatever turns fast (a bare
   leg left alone gains a quarter of what it holds each step at 30 rad/s: `ArticulationFactsTests`). Four guards on the
   keeper did not meet the test's bar (`Braced`, `BySpeed`, the other solver, all together); they stay in the code, off.
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
