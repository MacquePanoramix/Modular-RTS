# Handoff: where the work stands, and how to resume it

**Last updated:** November 13, 2026 (afternoon), by Claude Opus 5.5, which carries the work on from Fable 5.2. Kept
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
2. **The review** (item 3 of "In hand"): done on November 12 and acted on (`d0f8c22`). K8 and R1's addition stand
   reviewed; what it left open is in item 3.
3. **The rest of step 0** ("Next steps", item 2), each fact with its bar set before its run and read before it is
   claimed; then step 1; then **step 2, whose clips are the first thing Luis sees** (he has seen no motion from this
   work since October 30). **Where it stopped** (November 13, afternoon): the five other facts of step 0 have had one
   run (three meet their bars). The two misses (K4(a), K5) came from test rigs that ran the solver at 6 and 1 turns,
   not 14 and 4: **turns set on an articulation's root before its links are added are lost**, and the game's own
   bodies have the same fault (`OwnBody.cs:760`; `OwnBodyTests.EachMinersOwnBodyRunsAtItsOwnTurns` fails, all three
   miners at 6 and 1). The midday reading of K4(a) ("K8's law fails under a body that tips") is withdrawn; at the
   true 14 and 4 the ankle's share is 0.74 to 0.89 of the law's ("Next steps", item 2, "Where it stopped (November 13,
   afternoon)"). Begin there: the rigs given their turns after their links and step 0's misses rerun; the cost of 14/4
   over 6/1 measured; one independent review; then the report to Luis, who decides the game's fix.

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

## In hand: all done, the review too (items 1, 2 and 4 are history; item 3 is the stiff ankle, answered and reviewed)

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
     the same way; less with more turns of place, not with a stiffer ankle: the tip is the ground's (the review). Its
     cause is a reading only. Wherever the design asks a sole to be flat (R2, R5, the gates), "flat" is within 6
     degrees, one number in the design and the test; look at the soles in step 2's clips.
     (iv) The engine clamps a drive's target to the joint's limits: nothing asked through the target presses a joint
     into its stop (§2.3's note).
     (v) TGS is no cure: boots fly at landings there and every free spring acts 1.29 times as strong as set.
     (vi) More turns of the solver raise the share and lower the tip at a measured cost (a lying body's step costs
     1.5 times as much at 28 turns of place, 2.7 to 2.8 times at 60): a lever kept, not used.
   - **Corrected against what this page said before:** sideways the drive's reading is not "the boot rolls a little";
     it is the law's share plus what the drive gives against the boot's own tip with nothing asked (within 5% on all
     twelve readings). The test bars the reading towards the toe at an eighth over the law, and the tip at 6 degrees.
   - **The making-up** (R1's addition; mine, reviewed; **not built and not run**). Each step, for each axis: what
     the spring and damper said is `k e - (dt k + c) w` (plus `c` times the wanted speed), `e` the error at the step's
     start and `w` the joint's real turning over the step (its change of position over `dt`, an approximation on a
     ball joint; never the joint speed the engine tells, which is not motion for a held part). The share read is
     `driveForce` over that, "said" taken as what the drive's limit lets through. The share moves towards the reading
     by smooth weights and rates only (R1 has the form): quickly when it says the joint is free, slowly when held,
     less and less as little is said, as the told force nears the limit, or as a limited axis nears its stop; it is
     kept between the law's share for that axis and 1. The target is written at `q + (goal + torque / k - q) / share`,
     its offset from the joint never longer than `eFull + dt wmax` (1.65, 1.53, 1.61 times eFull at the three
     ankles), measured on the three axes together; the drive's limit stays the joint's strength. At that offset a part
     let go tends to its top speed and no faster (the fixed point `k e / (dt k + c)`). It is never reckoned against
     the wanted torque alone: that would undo K5's damper. Its linear part does not feed back on itself; three
     non-linear paths can (the force clamp, a joint on its stop, a hold that comes and goes at an edge): the first two
     are weighed smoothly to nothing, the third is read at step 2 on a sole on an edge, the share read step by step.
     Within the bound a held ankle can be given all its strength on Small, 0.81 (toe) and 0.61 (sideways) of it on
     Long, 0.72 and 0.56 on Round; a flat sole bears only 0.20 to 0.55 of the ankle's strength. Whether a rise needs
     the making-up at all, and whether it is better done as a trim on the told force, is measured at step 2, with it
     and without.
   - **The work of one step, settled by the review and written into R1:** with the target at the bound a drive works
     at most `S eFull b (1 + b) / 4` on a part in one step, `b = dt wmax / eFull`: 0.54, 0.40 and 0.49 of the
     spring's stroke (`S eFull / 2`) at the three ankles, past it only above `eFull / dt`, about 30.5 rad/s (Fable
     5.2's first `S eFull b (1 + b)^2 / 4` is true but looser). Over many steps no stroke bounds it while the target
     is rewritten: a part let go is bounded by its energy at top speed, `I wmax^2 / 2`, 1.4 to 1.6 J for the boots.
   - **The review is done and acted on** (November 12, evening; one Opus reviewer at medium effort:
     [review.md](Reviews/2026-11-12_StiffAnkle_K8/review.md) beside its brief; its verdict is advice, not Luis's). The
     law holds at the design's stiffness; its nine changes to the design text are made, with three things for step 2
     to read (`d0f8c22`; the validation record says which). Before it ran it was cut off twice by the plan's limit
     (three reviewers at once).
   - **Open, and not needed now:** why the boot tips; what a landing's one-step gain comes of (3.0 J at most; my
     reading: the ground pushing a sunk sole back out); why a stiffer spring gives a held boot more than the law (1.1
     to 2.8 times it at three and ten times the spring; the making-up's reading meets it); several parts held at once,
     a part half held, a held case on a diagonal (step 2 shows them); Round's
     unexplained course; the shove test (`ShovedAgainAndAgainNothingOfItFlies`) still fails as known and its bar is
     still on the speed the engine tells: the standing balance is not mended, by decision ("Next steps", 1); the
     whole PlayMode suite was not run again (the facts fixture was: 13 of 13).
   - **Usage, November 12:** Fable's week at 96% at midday and spent by the evening, so the work is Opus 5.5's from
     here. All models at 79% of the week in the evening, after the review and its changes (the five-hour window at
     52%; a whole window is about a fifth of the week, so about one window is left until the reset); it resets on
     Monday, November 16 (17:00 local). The plan is Pro and a five-hour window is small: three reviewers on Opus at
     high effort at once used a whole window in about five minutes and returned nothing. One agent at a time, a brief
     that names the files and the lines and holds the numbers, medium effort, and the agent writes its file early.
     Unity's runs cost nothing of the plan; reading them does, so read them through filters.
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

   **Where it stopped (November 12, night: Luis had to pack his computer).** The other facts are designed, each bar
   set before any run; nothing is written yet (`D:/Dev/WG` is at `582c022`, its test file unchanged). They go at the
   end of `ArticulationFactsTests.cs`, reusing its helpers (`Root`, `Link`, `FloatingLeg`'s pattern, `Held` and
   `TrulyGoes`, `Whirled`'s count, `RotationVector`, `Put`, `AsMade`'s way, `Stood`'s ground and links, `Shod`):
   - **K3, the reading about two axes.** A link on a free ball joint, fixed root, gravity off, the same turning weight
     about every line (as `Spun`), moved only by setting its joint's speeds each step, so its pose follows the asked
     path exactly: (a) spun about an oblique line (x and y together) to 175 degrees; (b) 60 degrees about x, then 60
     about its own y; (c) a swing of 60 degrees whose direction goes once round and back to its start. Bar: in all
     three the reading is the rotation vector of the joint's turn (the parent's anchor to the child's, in the anchor's
     axes, signed as `MadeLyingStaysAsMade` fixes it) within 0.02 rad (0.05 along the spin of (a)): a function of the
     pose, not the sum of the turns ((b) would read 1.05, 1.05, 0 if summed; (c) would gather a twist of half a turn
     about x if integrated). If it fails, joint_model §1.1 is wrong for two axes and the design changes (a joint's
     limits act on its reading). From the engine's source (a PhysX 4.1 copy: `DyFeatherstoneForwardDynamic.cpp`,
     `computeSphericalJointPositions` and the ball joint's case where the links' poses are worked out after a step):
     a ball joint with three free axes reads the rotation vector of its pose kept to the short side, so it would jump
     at a half turn, and one with fewer free axes adds up its speeds; the project's engine was measured (November 4)
     to run on past a half turn with all three free, so it differs from that copy here, and the run decides.
   - **K3, a drive across a half turn.** The same rig with Small's thigh (2.38 kg, its centre 0.13 m below the joint,
     turning weight 0.0131, 0.0029, 0.0131) and the design's drive (k = 130/0.611 = 212.8 N m a radian, c = 130/24.9
     = 5.22, limit 130), spun to 170 degrees about x and stopped; the goal 20 degrees on about x; a second case, the
     goal 30 degrees about its own y from there. Written by K3's rule at each step (the reading, plus the shortest turn
     from the pose to the goal in the child's anchor axes, at most eFull; no load). Bar: within 1 degree of the goal
     after a second, never more than 25 degrees from the start (the second case 45). Control: the goal written as its
     own rotation vector from zero must go the long way (more than 90 degrees from the start), or the fact proves
     nothing.
   - **K4(b).** `Hung`'s two links, once on a fixed root and once on a floating one, the same pose. From the source
     (`DyFeatherstoneInverseDynamic.cpp`, `getGeneralizedGravityForce`: a floating root goes through
     `inverseDynamicFloatingBase` with no joint accelerations, the body falling freely, which asks no joint torque):
     expected about nothing. Bar: every floating value under 1% of the largest fixed one.
   - **K5, the speed sweep.** For each miner a floating leg as `FloatingLeg`, built from that miner's parts (below):
     the hips the body less one leg (35.7, 49.1, 77.3 kg), their turning weight Small's (1.2, 0.4, 1.2) scaled by the
     weight and by the square of the leg's length (a stand-in, said so); each part's centre at its joint's distance
     below that joint, the next joint at twice it; the boot's centre at minus its joint's place. The drives of §2.3 on
     every axis (k = S/eFull, c = S over the top speed, limit S), target zero; gravity off; each link's
     maxAngularVelocity raised out of the way (the game's 20 is a drag of its own and would hide a gain). Set turning
     as `Whirled` sets it, at 10, 15, 20, 25, 30, 35, 40, 45 and 50 rad/s, a second each. It fails at a speed if it
     ever holds more than after its first step (beyond 1.0001 of it), or if in any step it gains beyond the count
     (what it holds, less what the drives were counted to give: drive force times the mean joint speed over the
     step) more than half of what its dampers took (per axis min(c |v|, S) |v| dt, v the mean speed over the step;
     under a floor of a thousandth of what it began with nothing counts). Each joint's bound: its top speed, or the
     lowest failing speed over 1.5 if that is lower. Bar: every miner holds at 10 (so every bound is at least 6.7
     rad/s, above the 5 a rise asks at its quickest). Control: left alone at 30 it must fail.
   - **K4(a), the load through the target, all three miners.** A planar rig with the root on top, as in the game: the
     weight (the body less the leg) floating; the thigh on a hinge (the hip), the shin (the knee), the boot (the
     ankle) with its sole on `Stood`'s ground, every hinge about the line across; the knee bent so the whole body's
     centre is 5 to 6 cm ahead of the ankle, inside the sole; the pose written before the first step (`AsMade`'s
     way). Each step, at each joint: τ = the moment of gravity of all on the root's side of that joint, about it,
     along its axis; target = goal + τ/k (in degrees for the drive). Both ways: without the making-up, and with it at
     the ankle by K8's law (Small 0.70, Long 0.53, Round 0.45). The knee's and the hip's shares are not known: the
     run reads each joint's (1 − its offset × k / τ). Bar (K4(a)): with the making-up, the pose within 2 degrees at
     every joint after 2 s. Expected without: the ankle off by τ/k (1/share − 1), about 5 degrees for Small. Reckon
     each miner's margin before the run: with τ worked out from the pose each step, only share × k × (goal − pose)
     holds a joint, against (1 − share) m g h of what stands on it when nothing makes up (Small's ankle about 70
     against 86 with the weight at the hip). So the run without may not stand at all, which would be the finding, not
     a fault of the rig; or the weight is set lower. Decide that before the run, and say why.

   Each miner's leg (`D:/Dev/WG/TestRuns/parts_r4.log`, OWN_SHOVE_PARTS, November 11; part kg, turning weight kg m²,
   strength N m, its joint's place in the part's frame, top speed rad/s):
   Small (40.7 kg): thigh 2.38, (0.0131, 0.0029, 0.0131), 130, (0, −0.136, 0), 24.9; shin 1.35, (0.0074, 0.0011,
   0.0074), 87, (0, −0.136, 0), 29.9; boot 1.29, (0.0030, 0.0037, 0.0015), 61, (0, 0.055, −0.006), 19.9.
   Long (58.5 kg): thigh 4.87, (0.0701, 0.0077, 0.0701), 303, (0, −0.220, 0), 20.1; shin 2.74, (0.0400, 0.0028,
   0.0400), 202, (0, −0.220, 0), 24.2; boot 1.83, (0.0059, 0.0069, 0.0021), 141, (0, 0.055, −0.007), 16.1.
   Round (87.6 kg): thigh 5.60, (0.0430, 0.0137, 0.0430), 331, (0, −0.160, 0), 23.2; shin 3.14, (0.0239, 0.0050,
   0.0239), 220, (0, −0.160, 0), 27.8; boot 1.55, (0.0043, 0.0051, 0.0018), 154, (0, 0.055, −0.007), 18.5.
   Soles 0.100 across and 0.060 high; 0.157, 0.187 and 0.172 long.

   Then: run only these (`bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> <filter>` in `D:/Dev/WG`), read the
   FACTS lines through a filter, write them into `Validation.md`, one independent review, and commit and push both
   branches. Still to tell Luis in the next report: the stiff-ankle test's bar was reshaped after its first run; more
   turns of the solver are a lever with a known cost (1.5 times at 28, 2.7 to 2.8 at 60); the review's verdicts; the
   pace of the plan's usage (about one five-hour window left before it resets on November 16).

   **Where it stopped (November 13, morning; Opus 5.5).** The five facts are written at the end of
   `ArticulationFactsTests.cs` in `D:/Dev/WG`, not committed yet. They compiled clean and have had one run
   (`TestRuns/step0rest.log` and `step0rest_r.xml`). Weekly usage then: 88%.
   - **K3, the reading** (`ABallJointReadsItsPoseAboutTwoAxes`): **passes.**
     - In all three cases the reading is the pose's rotation vector to 0.0000 rad.
     - The joint's speeds are counted in the child's own axes: (b) read (0.946, 0.946, 0.546).
     - In (c) the reading came back to (0, 1.047, 0), while the summed turns gathered −π about x.
   - **K3, the drive across a half turn** (`ADriveWrittenByTheShortestTurnCrossesAHalfTurn`): **passes.**
     - The shortest-turn writer reached both goals (0.00 degrees left; at most 20.1 and 30.0 degrees from the start).
     - The reading ran on past a half turn without a jump (3.317 rad).
     - The control about x went the long way (167.6 degrees).
     - The control about y, logged only, was still 90.8 degrees off after a second. So a far goal written as its own
       rotation vector does not merely go the long way: it does not arrive.
   - **K4(b), gravity on a floating root** (`GravityForcesOnAFloatingRootAreAboutNothing`): **passes as written, but
     the test checks less than its bar says.**
     - The fixed root read A2 16.508 and B2 2.775. The floating root read 0.000 at every index checked.
     - The floating root's list holds 12 values (6 of them the root's) against the fixed root's 6, and only 6 were
       checked. The bar says "every value", so check all 12 and log the start indices.
   - **K5, the speed sweep** (`EachMinersLegHoldsItsDrivesAtTheSpeedsARiseAsks`): **fails its bar.**
     - All three miners "fail at 10 rad/s", each by the second clause: a step's gain beyond the count was more than
       half of what its dampers took. At 10 rad/s:

       | Miner | Gain beyond the count (J) | Dampers' take (J) | Step |
       |---|---|---|---|
       | Small | 0.0108 | 0.0109 | 3 |
       | Long | 0.0443 | 0.0376 | 4 |
       | Round | 0.0225 | 0.0256 | 3 |

     - By the first clause (never holding more than after its first step), the driven legs hold up to 40 rad/s
       (Small), 35 (Long) and 45 (Round). They first rise at 45 (1.0197), 40 (1.0383) and 50 (1.0109).
     - The control, left alone at 30, fails as it must: 1.32 of its first step's after one step, then 1e21 to 1e30.
     - Reading, not yet shown: at 10 rad/s the drives stop the leg within two or three steps, and the clause trips on
       what is left, about 0.2% of what the leg began with. The gain beyond the count grows with speed (Small: about
       0.2% of its first step's at 10, 3% at 30, 9% at 50).
     - The bounds this run's rule gives are 6.7 rad/s everywhere. That is not shown, because nothing below 10 was
       run.
     - **To do:** a diagnostic with no bar, at 5, 10, 20 and 30 rad/s. It logs each of the first six steps, and the
       whole second summed: the gain beyond the count, what the dampers took, and what the leg holds against its first
       step. Then a re-specified bar, put to an independent reviewer before its run.
   - **K4(a), the load through the target** (`TheLoadGoesThroughTheTargetOnEachMinersLeg`): **fails its bar.**
     - Every case fell, with the making-up as well as without (without was predicted):

       | Miner | Fell with the making-up | Fell without |
       |---|---|---|
       | Small | 0.48 s | 0.40 s |
       | Long | 0.82 s | 0.68 s |
       | Round | 0.38 s | 0.12 s |

     - As made, the joints read (0, 0, 0). The engine reads a link made posed as being at zero, as
       `MadeLyingStaysAsMade` found. The pose is then written AsMade's way.
     - The ankle's load at the first step matched the hand-reckoned stance (Small 21.88 N m).
     - Round fell in six steps. That needs hundreds of N m, far beyond gravity's imbalance of tens, so **suspect the
       rig before the design**.
     - The shares logged after 2 s were read off the fallen body and mean nothing.
     - **To do:** a trace of each step up to the fall: the three joints' offsets, drive forces and loads; the weight's
       drop and pitch; the boot's pitch. With two controls: gravity off (it must stand exactly), and drives ten times
       as stiff.
     - One more effect to read in the trace. A one-step lag in the load put through the target acts as a negative
       damping of about m·g·h·dt, against the damping s·c:

       | Joint | m·g·h·dt | s·c |
       |---|---|---|
       | Small's ankle | 5.7 | 2.1 |
       | Long's ankle | 13.1 | 4.7 |
       | Round's ankle | 14.8 | 3.8 |
       | Round's knee | 9.2 | 7.9 |

       That growth is slow (seconds), so it is not this fall.

   **Where it stopped (November 13, midday; Opus 5.5).** The diagnostics are written (`ArticulationFactsTests.cs`:
   K4(b)'s check of all 12 values; `TheSweepsCountReadStepByStep`; `TheStandingLegsFallTraced`) and have had one run,
   all three passing as tests (`TestRuns/step0diag.log`, 72 FACTS lines). Weekly usage then: 90%.
   - **K4(b): passes its full bar.** Start indices: fixed root 0,0,3; floating root 0,6,9 (the root's own 6 first).
     All 12 floating values 0.0000 against 16.508 fixed.
   - **K5's count, read step by step.** The leg's energy never ends near zero: it falls to about 0.83 of its first
     step's in two or three steps and stays there (0.80 to 0.92 after the second), because the floating leg keeps its
     whole spin (no drive can take that) and only its inner motion is damped. Per step, the gain beyond the count runs
     negative in the first step and positive in the next two or three (a lag of the count, not a gain): summed over the
     second it is about nothing at 5 rad/s (−0.04%, −0.01%, 0.00% of the first step's, Small, Long, Round), small at 10
     (+0.44%, +0.76%, +0.41%), and grows (20: +3.7%, +5.1%, +3.4%; 30: +6.2%, +8.2%, +6.1%). Against what the dampers
     took over the second: at 10: 0.035, 0.063, 0.029; at 20: 0.30, 0.43, 0.24; at 30: 0.49, 0.69, 0.42. Never above
     the first step's (1.0000) at any of these speeds. **So the first run's second clause tripped on the count's lag.**
     Proposed new bar (to be reviewed before its run): the clause taken over the whole second (the summed gain beyond
     the count at most half the summed dampers' take), the first clause as it was; by these numbers every miner holds
     at 10 and Long fails at 30.
   - **K4(a)'s fall traced** (*withdrawn in the afternoon block below: this rig and K5's ran at 6 and 1 turns, not 14
     and 4; the numbers stand as a record of a leg at 7 turns*)**: the rig is sound, the fall is the solver's.** The control, Round with gravity off, stood
     exactly (0.001 degrees, 0.0 mm); after the first step every part was where it was made (0.0 mm, 0.00 degrees), so
     the hinge's reading runs as assumed and the pose is written right. With gravity, the **ankle's drive gives only a
     small part of what it asks**: Round without the making-up, 8.9 of 47.2 N m in the first step, then 12 to 14 while
     asking 78 to 119 (about 0.10 to 0.19; K8's law says 0.45); Small with the making-up, 7.5 of 31.4, then about 9.4
     of 46 to 54 (about 0.17 to 0.24; the law says 0.70). The hip gives about what it asks (0.7 to 1.1); the knee
     about 0.8 of it, and buckles as the body tips (off +12 degrees in 8 steps). **The boot rocks toe-up and its heel
     sinks:** Round's boot tilted −2.0 degrees in the first step and −8 by the fifth, its centre 7.5 mm down, its heel
     about 19 mm into the ground by reckoning; the weight dropped 2.6 mm in the first step (two-thirds of a free fall's
     step) and about 22 mm in eight. Round fell at step 6, Small at step 24. So under 14 and 4 turns the ground does not
     hold a light boot that bears a whole body, and the ankle's share is a third to a quarter of the law's. **K8's law
     was matched where the weight above never turned (the stiff-ankle rig's slide); it does not hold under a body that
     tips.**
   - **Next (decided before the compaction, not yet done):** (1) commit the diagnostics and this block (doing now);
     (2) one independent review (opus, medium) of K5's new bar and of this reading of K4(a); (3) a small lever run on
     the traced rig, Round with and without the making-up: the solver at 14/4, 28/4 and 60/4 (the law's share for
     each), and TGS at 14/4, to learn whether more turns or the other solver give the ankle its share and hold the
     heel; (4) then the report to Luis, since the drive writer's making-up by K8's law does not hold the stance and the
     lever has a cost he should weigh (1.5 times at 28, 2.7 to 2.8 at 60).

   **Where it stopped (November 13, afternoon; Opus 5.5): the midday reading was wrong; the cause is the solver's
   turns, and it is in the game.** The 25th outlook is taken. Three runs were made, all read:
   - `TheStandingLegsShareUnderTheSolversTurns` (`TestRuns/levers_pgs.log`): setting the turns again after the leg was
     built gave the ankle's first-step share near K8's law: at "14/4", Small 0.514, Long 0.471, Round 0.395 (the law
     0.697, 0.532, 0.451); at 28/4, 0.691, 0.637, 0.560 (law 0.880, 0.741, 0.656); at 60/4, 0.881, 0.829, 0.764 (law
     0.986, 0.933, 0.882). So measured over the law is 0.74 to 0.89, everywhere. Without the making-up: at 14/4 all
     fell (steps 49, 66, 34); at 60/4 Small and Long stood two seconds and Round fell at step 79. The heel did not sink
     there (0 to 2.4 mm at 14/4 for Small and Long; 6.0 for Round).
   - `TheSolversTurnsSetBeforeTheLinksAreKept` (`TestRuns/turnskept.log`): **turns set on an articulation's root
     before its links are added are lost.** The root reads back 6 and 1 (the project's defaults,
     `DynamicsManager.asset`); the leg as made and the leg set again to 6 and 1 run the same course to the last bit;
     set again to 14 and 4 it does not (Round's share 0.189 against 0.395; it falls at step 6 against 34). So
     `StandingLeg` (K4(a), the trace) and `BareLeg` (K5) ran at 7 turns, and the midday reading ("K8's law fails under
     a body that tips") is withdrawn: at 7 turns the law gives 0.21 at Round's ankle and the trace read 0.19. The
     stiff-ankle rig set its turns after its links and was right.
   - `OwnBodyTests.EachMinersOwnBodyRunsAtItsOwnTurns` (bar set before the run: 14 and 4; `TestRuns/ownturns.log`):
     **fails. Each miner's own body in the game runs at 6 and 1** ("OWN_TURNS Small: its hips the root, 6 and 1"; the
     same for Long and Round). The cause is `OwnBody.cs:760`, which sets the turns on the hips in the loop that adds
     the links, before the others exist. `BodysOwnBench.cs:317` does the same, so the benches ran at 6 and 1 too. The
     game's `PhysicalFall` parts are Rigidbodies and are not affected. The cost measurement (`OwnBodyTests` near line
     1011: 14/4, 28/4, 60/4 on copies with the turns set after) never measured 6/1, so what 14/4 costs over what the
     game runs now is not known.
   - **What it means.** Every own-body behaviour tuned since `OwnBody` was written (the standing balance, the step, the
     shove test, the getting up, the benches' searches) was tuned at 6 and 1, believed to be 14 and 4. The fix is one
     line (set the turns on the root after the loop), but it changes how every own body behaves and costs CPU: so it is
     put to Luis, not made silently. It is consequential: an independent review of the finding before the report.
   - **Next:** (1) commit the three tests (done with this block) and push; (2) give `StandingLeg` and `BareLeg` their
     turns after the links (keep a `turnsFirst` way for `TheSolversTurnsSetBeforeTheLinksAreKept`), and rerun K4(a),
     K5, the K5 count read, the trace and the lever run at the true 14 and 4; (3) measure what 14/4 costs over 6/1 on
     the game's body; (4) one independent review (opus, medium); (5) the report to Luis: the finding, the fix and its
     cost, and that step 0's two misses must be read again at the true turns. Weekly usage: 91%.
   **Where it stopped (November 13, late afternoon; Opus 5.5): step 0's misses rerun at the true turns; the cost
   measured.** All of it is in `Validation.md` ("step 0's other facts; the solver's turns lost ...").
   - The rigs now set their turns after their links (`StandingLeg(..., turnsFirst)` keeps the old way for the turns
     test, which gained a fourth way: made with the turns after, it reads back 14 and 4 and runs the same course to the
     last bit as one set again to 14 and 4). Run `step0true`: K4(a) and K5 fail their bars, the four diagnostics pass.
   - **K4(a) at 14 and 4 still falls**, two to six times later (with the making-up: Small 1.04 s, Long 1.54, Round
     1.16). The ankle gives about 0.66 of the law's share when made up (0.46 of its ask for Small), and less as it asks
     more, so a making-up by the law's share, and R1's read share floored at the law's, fall short. **K5 is unchanged by
     the turns**: its per-step clause trips on the count's lag and on rounding at every speed (even at 5 rad/s); the
     summed clause would hold every miner at 10 and 20 rad/s and fail Small (0.52) and Long (0.69) at 30.
   - **Cost** (`turnscost.log`): a body a step at 6/1 against 14/4: Small 62.3 against 110.5 microseconds, Long 70.1
     against 123.1, Round 62.2 against 106.8. So the fix costs 1.72 to 1.77 times, about 2.2 to 2.7 ms more a body for
     each second played.
   - **Luis's word (November 13, evening; [verbatim](Correspondence/2026-11-13_FIX_THE_TURNS_AND_THE_WEEK.md)): "1. Yes
     please." to fixing the turns in the game behind a switch; and the week's limit need not hold the work back (he has
     a reset to use once it is full; the five-hour window still applies). So, now:** (1) the switch, in `OwnBody`: a
     static `TurnsAsMeant` (on: 14 and 4; off: 6 and 1, as tuned and played until now), checked each physics step against
     what the root reads back and set again when it differs (so a link added later, a toe, cannot lose it silently);
     `BodysOwnBench` follows it; a toggle on the miner panel (`MinerPanel.cs`, beside "Stands by its own joints") so
     Luis can flip it live while playing. (2) `EachMinersOwnBodyRunsAtItsOwnTurns` passes with it on, and reads 6 and 1
     with it off. (3) One review workflow (two opus reviewers, medium): the finding and the fix; K4(a)'s reading, R1's
     floor and K5's summed bar. (4) `OwnBodyTests` as a whole, on and off, read side by side; the flows as played. (5)
     A build for Luis with the switch, its default and its place named in the report's first line.
   - **The report went to Luis (November 13, late afternoon), before the review**, because the weekly allowance stood at
     93%: he is asked (a) whether to fix the turns in the game, outright or as a switch to compare, and (b) whether to
     spend what is left of the week on the review now or wait for the reset (Monday, November 16, 16:00 UTC). Wait for
     his word; record it verbatim in `Correspondence`.
   - **Next:** (1) one independent review (opus, medium): the turns finding and the game's fix; K4(a)'s reading and
     what R1's making-up should floor at; K5's summed bar. (2) The report to Luis (sent): the finding; the fix, its cost, and
     that it changes how every own body moves (his to decide, perhaps as a switch to compare); step 0's results. (3)
     Then, by his word: the game's fix with its own review, `OwnBodyTests` as a whole at the true turns, and R1's
     making-up changed and K4(a) run again.
   - Then: the November 13 entry in `Validation.md` (written) and the top of `CURRENT_STATE.md`. The report to Luis also carries
     the earlier items: the stiff-ankle bar reshaped after its first run; the reading that runs past π; that naive goal
     writing does not arrive (the y control); the floating gravity forces; the K3b y-case control.
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
