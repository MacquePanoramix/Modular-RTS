# Handoff: where the work stands, and how to resume it

**Last updated:** November 12, 2026 (evening), by Claude Opus 5.5, which carries the work on from Fable 5.2. Kept
current at every milestone.

If you are a new session or another model: read `AGENTS.md`, the top entry of [CURRENT_STATE.md](CURRENT_STATE.md), then
this page. Luis's preferences and the lessons already paid for are in the assistant's memory directory (its index is
`MEMORY.md`; it is loaded into every session of this project).

## Start here (November 12, 2026): the work passes from Fable 5.2 to Opus 5.5

Luis, November 12, midday: "have everything prepared so Opus5.5 on these same thinking max ultracode settings can pick
up your slack as much as they can"; and in the evening, after the switch: "Now Opus it's your time to continue this
journey together with me" ([his messages](Correspondence/2026-11-12_PASS_THE_TORCH_TO_OPUS.md)). From here the work is
Opus 5.5's, at max thinking with ultracode on. Everything it needs is in the repository. In order:

1. Read `AGENTS.md`, this page, then `Design/AnyBody/motion_design.md` sections 6 and 7 (they stand over sections 0
   to 5), above all §6.2 (K1 to K8, R1 to R5) and the step table of §6.3. The memory index loads by itself.
2. **The review that is owed** (item 3 of "In hand"): run it from its brief, or read it if it is there already, and
   act on it. Until then K8 and R1's addition are their author's own word.
3. **The rest of step 0** ("Next steps", item 2), each fact with its bar set before its run and read before it is
   claimed; then step 1; then **step 2, whose clips are the first thing Luis sees** (he has seen no motion from this
   work since October 30).

What binds every step: quality above speed; never a played animation; nothing that switches at a threshold; one body
for everything; every step reviewed by agents who did not make it; a result said only after its run was read; Luis's
own files never touched; no merge to main without his word; his messages recorded word for word. What ultracode means
on this plan: the plan is Pro and its five-hour window is small, so a workflow is sized to finish (one or two agents at
a time, a tight brief, their file written early), and mechanical steps are done alone.

## The arrangement

- **Luis is the Game Director.** His principle, in his words (November 9): "I prioritize quality above everything
  else. Speed of development should never sacrifice any of the quality of the project and of the ideas and goals and
  ambitions" ([the message](Correspondence/2026-11-09_QUALITY_ABOVE_EVERYTHING_AND_FABLE.md)).
- **The model.** From November 12 the session runs on **Opus 5.5 at max thinking, with ultracode on** (Luis moved it
  there when Fable 5.2's share of the plan was used up: [his messages](Correspondence/2026-11-12_PASS_THE_TORCH_TO_OPUS.md)).
  From November 9 to 12 it ran on Fable 5.2, which was included only up to half of the plan's limits and drew usage
  much faster. Subagents take the session's model unless told otherwise: pass `model: 'opus'` for builders and
  reviewers and `model: 'sonnet'` for judges.
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
runs, since it is the rare fall). Luis waits on motion: the first clips come after step 2. On November 12, Fable's share
of the plan being used up, he moved the session to Opus 5.5 ([his messages](Correspondence/2026-11-12_PASS_THE_TORCH_TO_OPUS.md));
nothing is asked of him.

## In hand: all done but the review (items 1, 2 and 4 are history; item 3 is where the work stands)

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
3. **The stiff-ankle fact (K1's R4): answered on November 12. It holds under the game's solver; nothing of the
   engine changes; nothing is put to Luis, he is only told.** The account is in `Design/AnyBody/motion_design.md`:
   R4 ("Answered"), R1 ("One thing is added (November 12"), the new K8, the two notes of §2.3, K4(a), K5, and row 0
   of §6.3. The numbers are in the last section of `Validation.md`. (The working notes this item held until then are
   in git: `git show 486ee5b:Docs/Handoff.md`.)
   - **The test** `ArticulationFactsTests.AStifflySprungAnkleHoldsALightBoot` (work branch, `582c022`) was reshaped to
     what holds and passes under PGS: three runs read (`D:/Dev/WG/TestRuns/facts_r4_pgs3.log`, `facts_r4_pgs4.log`,
     and the whole fixture, `facts_all_nov12.log`: 13 of 13). To run it, from `D:/Dev/WG`:
     `bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> WonderGather.Tests.ArticulationFactsTests.AStifflySprungAnkleHoldsALightBoot`
     (about ten minutes); its lines as a table:
     `python -I Docs/Technical/AnyBody/tools/r4_table.py TestRuns/<name>.log flat|rest|all [text the row must hold]`.
     Under TGS it fails and is meant to (not run again after the reshaping).
   - **What was found** (K8 has each with its numbers):
     (i) Under PGS a light part that the ground holds still gets only `1 - (1 - x)^n` of what its drive's spring
     says, `x = I / (I + dt (dt k + c))`, `n` the turns of place and of speed together (18). The three ankles: 0.70,
     0.53, 0.45 towards the toe (Small, Long, Round), 0.61, 0.40, 0.35 sideways. The other joints about 1 (a rough
     reckoning: to be done exactly, for each axis of each joint, when a body is made).
     (ii) A part nothing holds gets the implicit spring and damper exactly; a part let go turns at
     `(I w + dt k e) / (I + dt (dt k + c))` at most. The drive tells truly what it gave (`driveForce`).
     (iii) A flat boot under the body's weight sits 1.2 to 4.0 degrees off flat (4.7 after it was made bent), always
     the same way; less with more turns of place. Its cause is a reading only. Wherever the design asks a sole to be
     flat (R2, R5, the gates), about 5 degrees must be allowed; look at the soles in step 2's clips.
     (iv) The engine clamps a drive's target to the joint's limits: nothing asked through the target presses a joint
     into its stop (§2.3's note).
     (v) TGS is no cure: boots fly at landings there and every free spring acts 1.29 times as strong as set.
     (vi) More turns of the solver raise the share and lower the tip at a measured cost (a lying body's step costs
     1.5 times as much at 28 turns of place, 2.7 to 2.8 times at 60): a lever kept, not used.
   - **Corrected against what this page said before:** sideways the drive's reading is not "the boot rolls a little";
     it is the law's share plus what the drive gives against the boot's own tip with nothing asked (within 5% on all
     twelve readings). The test bars the reading towards the toe at an eighth over the law, and the tip at 6 degrees.
   - **The making-up** (R1's addition; mine; **not built and not run**). Each step, for each axis: what the spring
     and damper said is `k e - (dt k + c) w` (plus `c` times the wanted speed), `e` the error at the step's start and
     `w` the joint's real turning over the step (its change of position over `dt`; never the joint speed the engine
     tells, which is not motion for a held part). The share read is `driveForce` over that. It is weighed by how
     much was said against the joint's strength (nothing is learnt while little is said), kept between the law's
     share for that axis and 1, believed at once when it says the joint is free and slowly when it says it is held.
     The target is written at `q + (goal + torque / k - q) / share`, never further from the joint than
     `eFull + dt wmax` (1.65, 1.53, 1.61 times eFull at the three ankles), the drive's limit staying the joint's
     strength. It is never reckoned against the wanted torque alone: that would undo K5's damper. Reading the share
     does not feed back on itself: for a held part, told over said is the true share whatever was written. Within
     the bound a held ankle can be given all its strength on Small, 0.81 (toe) and 0.61 (sideways) of it on Long,
     0.72 and 0.56 on Round; a flat sole bears only 0.20 to 0.55 of the ankle's strength. Whether a rise needs the
     making-up at all, and whether it is better done as a trim on the told force, is measured at step 2, with it and
     without.
   - **One thing reckoned after the design text was written, to be settled by the review and then written into R1:**
     with the target at the bound, what a drive can work on a part in one step. Fable 5.2 reckoned it at most
     `S eFull b (1 + b)^2 / 4`, `b = dt wmax / eFull`: 0.89, 0.61 and 0.78 of the spring's stroke (`S eFull / 2`) at
     the three ankles, past the stroke only on a light end part whose top speed is over about 21 rad/s. Opus 5.5,
     checking it against the implicit law (`k e0 - (dt k + c) w_end`, the joint's turn `dt w_end`), gets
     `S eFull b (1 + b) / 4`: 0.54, 0.40 and 0.49 of the stroke, past it only above `eFull / dt`, about 30.5 rad/s;
     the first form leaves out the `dt k` part of the damping, so it is the looser bound. Either way R1's stroke bounds
     it at the three ankles.
   - **Owed: the review.** It was cut off twice by the plan's limit (three reviewers at once) and gave nothing. Its
     brief, sized to finish, is [Reviews/2026-11-12_StiffAnkle_K8/brief.md](Reviews/2026-11-12_StiffAnkle_K8/brief.md);
     the findings go to `review.md` beside it. If that file is there already, read it first (the review was
     started on November 12, in the evening, and may have finished). Then act on it in the design and the validation
     record. Its verdict is advice, not Luis's.
   - **Open, and not needed now:** why the boot tips; what a landing's one-step gain comes of (3.0 J at most; my
     reading: the ground pushing a sunk sole back out); several parts held at once (step 2 shows it); Round's
     unexplained course; the shove test (`ShovedAgainAndAgainNothingOfItFlies`) still fails as known and its bar is
     still on the speed the engine tells: the standing balance is not mended, by decision ("Next steps", 1); the
     whole PlayMode suite was not run again (the facts fixture was: 13 of 13).
   - **Usage, November 12:** Fable's week at 96% at midday and spent by the evening, so the work is Opus 5.5's from
     here. All models at 77% of the week in the evening; it resets on Monday, November 16 (17:00 local). The plan is
     Pro and a five-hour window is small: three reviewers on Opus at high effort at once used a whole window in about
     five minutes and returned nothing. One agent at a time, a brief that names the files and the lines and holds the
     numbers, medium effort, and the agent writes its file early.
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
2. **Step 0, the ground of the controller** (design §6.3, K1 to K4, K8). **Its first fact is answered** (November 12,
   item 3 above: the stiff ankle holds under the game's solver, and nothing is put to Luis), so what follows up to
   "Then the other facts" is history. As it was asked: **first** the fact K1's R4 asks for: whether an
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
