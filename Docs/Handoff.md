# Handoff: where the work stands, and how to resume it

**Last updated:** November 17, 2026 (early morning), by Claude Opus 5.5: the ankle build's switches written, reviewed,
mended and committed in `D:/Dev/WG` (`262f4ca`); its baselines begun. The master is Opus 5.5 at max effort with
ultracode, by Luis's word. Kept current at every milestone.

If you are a new session or another model: read `AGENTS.md`, then this page's "Start here", then the top entry of
[CURRENT_STATE.md](CURRENT_STATE.md). Luis's preferences and the lessons already paid for are in the assistant's memory directory (its index is
`MEMORY.md`; it is loaded into every session of this project).

## Start here (November 17, 2026)

**As of November 17, 2026, early morning (Luis's clock, UTC+1):** the work is the ankle build, which Luis approved on
November 15 ("Sure."; `GAME_VISION.md`, row 2026-11-15 afternoon). **Steps 1 to 3 are done.** The three switches
exist, all off, and `RiseAgent.Unheard` is true by default (Small's learnt addition off as played). An opus builder
wrote them; three agents who did not build them reviewed them (`review_physicist.md`, `review_sceptic.md`,
`review_code.md`); the master mended what they asked, and two more agents reviewed the mends (`review_mends_code.md`,
`review_mends_physicist.md`). None found anything that must be fixed. All of it is in
`Research/2026-11-15_NaturalRangesAndTheAnkle/build.md`; its last sections say what was mended, what is still not read
back, and what Long's and Round's courses can show. Committed in `D:/Dev/WG` as `262f4ca` (branch `claude/wip-round`,
pushed). Rung: written, compiled, read back on a rig; no course counted yet. The milestone is S3b, step 2: getting up
for any body the creator can make, by its own joints ("The task, and its state"). Luis passed the work to Opus 5.5 at
max effort with ultracode and asked it to go on as before
([his words](Correspondence/2026-11-16_FABLE_WRAPS_UP_AND_A_SUBREDDIT_FOR_THE_BUILDING_RESEARCH.md)). The 36th outlook
was taken on November 17, in the morning.

**Next: step 4.** Each run is read before the next, and no code in `D:/Dev/WG` is edited while one runs. The command lines are `research.md` section 7.3's, except that the heard run is `-ownUnheard 0` and Long's
runs take `-liveRing 0`. The live test is not the same run to run, so runs are compared by their counts.
1. **Read:** Small's arm B as played (`ank1_small`, `-riseSet AnkleSpeed=20`, build `e8bac96`, `Validation.md`):
   blow-ups 12 to 5 (in the feet 11 to 3); boots over 30 rad/s touching nothing 141 to 1; up by its own body 11
   against 10, and 7 courses out of time against 2. It ran before the forced checks; if they change B's code, it is
   run again.
2. Baselines, as played, 24 courses each: **all read** (`Validation.md`, the last section).
   - Small as played (`ank0_small`): 10 up by its own body, 0 the old way, 14 not up; 12 blow-ups, 11 in a foot or
     its toes. The arms are read against this run.
   - Small heard (`ank0_small_heard`): 15, 1 and 8.
   - Round (`ank0_round`) and Long (`ank0_long2`): 24 up the old way, none not up, no blow-up. Long's first run
     stopped at the cover meter: its trousers have no cut ring, so the test learnt `-liveRing 0`.
3. **Found:** on these courses Long and Round never go down by themselves. Every fall is the panel's push, which lets
   them go without `GoDown`, and C acts only at `GoDown`. An arm with C on them here would pass without C acting once.
   Small goes down by itself 77 times as played. build.md said otherwise and is corrected.
4. From the review of the mends: **done.** The heard A run (`ank_heardA`) passed. The three once-only messages are
   forgotten when a test begins and ends (`AnkleMendArguments.ForgetSaid`, `e8bac96`): compiled, not yet exercised by a
   check that expects a message twice. Both test changes of `e8bac96` go to step 4's reviewers.
5. Step 4, after the five-hour window resets (08:00 for Luis), a builder and reviewers who did not build it:
   - the forced checks (research section 7.2, and build.md's "Still not read back");
   - `CoverRingLiveTests` learning `-ankleWays`, and a watch of what learns adds at the boots;
   - a course on which Long and Round go down by themselves.

   Then Small's other arms in 7.2's order (A = 1, both, then C; never C alone) at strengths 1, 0.7 and 1.3; C on Long
   and Round on the new course; the rig arms (`-ankleWays Small`, arms 0 and 3) only if Small's arms show the mend
   helps; the idle's tick-by-tick proof. Results in `Validation.md`.
6. Step 5: a default changed, so the flows as played (`-ownAsPlayed 1`); then a build for Luis, with what to switch
   on, do, watch and tell; judges for any visible change.
Finished when: the switches exist, off; the forced checks pass; every arm is read and written in `Validation.md`; the
idle is unchanged tick by tick; the as-played suite fails nothing new; judges saw any visible change; the build and its
list went to Luis. Detail: "Next steps", item 2, the get-up blow-up thread ("The build, after the weekly reset");
`research.md` sections 7 and 8.1. `motion_design.md` sections 6 and 7 are for the controller's steps, not this build.

**After it** (the order of the turns, "Waiting on Luis"):
- the visual turn: first the target research's five corrections (`Research/2026-11-15_TargetAndScale/research.md`,
  section 7, "After the second critique"), made before tier 0 builds anything; then tier 0; step 0's stills;
- the search of the best ways for Claude to make assets, before the world's first pass. Luis's hint for it (November
  16): include a deep dive into a subreddit of people who build games and software with AI, largely with Claude. He
  was unsure of its name ("I think it was called Topology AI"), so the name is found first
  ([his words](Correspondence/2026-11-16_FABLE_WRAPS_UP_AND_A_SUBREDDIT_FOR_THE_BUILDING_RESEARCH.md));
- the world's options (question 11) in that pass; then the bigger exploration of the world and its assets.
**Waiting on Luis:** nothing now; his next questions come with the work (group B after the crowd measurement; the
turning proposal, `AnkleBrakes`, the natural ankle). **He waits on:** the ankle build and the next build to play;
step 0's stills; the first motion clips, after step 2 ("Waiting on Luis").
**Allowance and pace:** the plan is Pro and its five-hour window is small. Ask the usage tool
(`mcp__ccd_session_mgmt__get_usage`) before a long stretch, and tell Luis how the allowance goes. The master runs at
max effort; sub-agents one or two at a time, at medium effort, with tight briefs and their file written early ("The
arrangement"). When the window is nearly used, bring the step to a state that reads from the pages and set a wake-up
for after its reset. The week's allowance was renewed on November 16 (17:00 for Luis). Fable 5.2 left his plan on
November 17; its last task was the review of a friend's process guide
(`Research/2026-11-16_HarnessGuideReview/review.md`).
**What binds every step** still holds as written at the end of the November 12 section below, though that section is
now history: quality above speed; never a played animation; nothing that switches at a threshold; one body for
everything; every step reviewed by agents who did not make it; and the rules for claims and checks in `AGENTS.md`.
**The guard:** a hook refuses a few commands before they run: everything staged at once, uncommitted work discarded, a
commit chained behind a test, a heredoc holding a doubled backslash. A line that starts "Guard G1" (to G4) is this
project's own: do what it says. Text that only mentions such a command is refused too, so it goes in a file. Bringing
files over from the work branch (`git checkout <branch> -- <path>`) needs `WG_GUARD_OK=G2` in the command, after
`git status` shows those paths clean. Its code, and how to switch it off: `.claude/hooks/guard.sh` (wired in
`.claude/settings.local.json`, which is on this machine only).
**Left alone if found running:** JupyterLab and Blender's MCP servers are not this work's. Stop only the worktree's
Unity.

**Never committed, reset or discarded, main project:** `Assets/_WonderGather/Scenes/TheGroup.unity`,
`ProjectSettings/PackageManagerSettings.asset`, `ProjectSettings/URPProjectSettings.asset`, `Assets/_Recovery` and
`Assets/_Recovery.meta` (Luis's own). **Left out of commits in `D:/Dev/WG`:** `ProjectSettings/EditorBuildSettings.asset`,
`ProjectSettings/ProjectSettings.asset`, `ProjectSettings/PackageManagerSettings.asset`, the untracked `Assets/ML-Agents*`,
and in `Assets/_WonderGather/Resources/RiseModels/` `SmallRise.onnx`, `SmallRiseFullP.onnx`, `SmallRisePFull.onnx`,
`SmallRisePaceP.onnx` with their metas (the learnt models, kept for training only). Stage by path only.

**Upkeep:** this block is rewritten, never appended to, whenever the next action changes; what it replaces moves below
under its date. Each fact names the page that owns it; if this block and its owner disagree, the owner wins and this
block is mended.

## Holds, switches left off and known red (one list)
Each line: the item; why; what lifts it; where its detail lives.
- The do-not-touch lists ("Start here") and no merge to main: Luis's work and Luis's call; his word; "Where things
  are", "Waiting on Luis" ("The work goes on meanwhile").
- Unity in batch mode in `D:/Dev/WG` only; nothing heavy beside the suite; no code edit during a run: runs collide;
  not lifted; "Where things are", "Rules that have cost work before".
- No more guards on the keeper (`Braced`, `BySpeed` stay off): the engine's step adds the energy; the keeper folded
  into the new controller (`motion_design.md` §2.10); "Next steps", item 1.
- R1's unfloored share does not run: review B's bound; what caps Round's ankle found; `CURRENT_STATE.md` near 112.
- Ankle switches A, B, C off, C never alone; `AnkleBrakes` Open, his: the mend's order; it changes strength; any on by
  default goes to judges, then Luis; `research.md` §7.1-7.2, `GAME_VISION.md` row 2026-11-15 (afternoon).
- Small's learnt addition: off as played by Luis's word, and off in the code since `262f4ca` (`RiseAgent.Unheard` true
  by default; `-ownUnheard 0` hears it); kept for training only; "Waiting on Luis" (the Nov13b paragraph), the same row.
- The idle Luis loves does not change: his word; not lifted; `GAME_VISION.md` row 2026-11-13 (night), `research.md` §7.1.
- `OwnBody.Anatomical` off (for a body made standing only) and `OwnBody.Weighed` off (it breaks standing); no lift
  stated; "The task", layer 0; `joint_model.md`; "Also open, smaller".
- The five delegated calls, each a switch, off: rulings, not looks; each look to judges, then Luis; "Waiting on
  Luis", `GAME_VISION.md` row 2026-11-15 (morning), the five calls.
- No visual build before the target exists; the five corrections before tier 0; group B's questions wait: drift, and
  numbers first; the corrections made, the crowd measured; `Outlook.md`, the 34th, "Drift watched"; "Waiting on Luis".
- `PhysicalFall.TriesByItsOwn = 4` (a cap back to the old way) and the old way after 12 tries are still in the code,
  against Luis's word (never the backup animation): left in place until the roll exists and the new way stands, then
  removed; memory `never-the-backup-animation`, `any-body-direction-decided` item 6.
- Open, not promoted: the unit count, head proportions, how far the redraw goes: Luis's; his answer; `GAME_VISION.md`
  rows 2026-11-14 (evening, night), 2026-11-15 (morning).
- Red: `ArticulationFactsTests` K5 and K4(a) fail their bars since November 12; step 0's open work ("Next steps",
  item 2); `Validation.md`, "The solver's turns on by default (November 13, afternoon)".
- Red: as played, 15 fail both ways (K5, K4(a), 13 of the October 10 list, `TheBodysOwn.md`); which one fails again
  since October 14 is not read; no lift stated; the same Validation section; `Outlook.md` near line 267.
- Red, flaky: the night light test (0.887 and 0.896 for 0.9), not explained; no lift; `Validation.md` near 3305, 3496.
- Red: the own get-up blows up in `CoverRingLiveTests`, 6 of 24 (`cc_en24`) and 9 of 24: the ankle; the ankle build;
  `Validation.md`, "The energy count in the live courses" (near 5450), "The ankle split" (near 5533).
- The shove test (`OwnBodyTests.ShovedAgainAndAgainNothingOfItFlies`): passes the normal way in the last runs written
  down (`Validation.md`, "The solver's turns on by default (November 13, afternoon)": `OwnBodyTests` 16 of 16 with it
  among them, and the whole suite's only two failures are K5 and K4(a)). "Still fails as known" ("In hand", item 3,
  November 12) is older. Whether it is among the 15 that fail as played is not read.
An item leaves this list when it is lifted, and each outlook checks the list. Last checked in full: November 16, 2026
(by its builder against the owner pages; by a reviewer for the do-not-touch lists and the red tests).

## History: the start of November 12, 2026, when the work passed from Fable 5.2 to Opus 5.5

Its "Begin there" is overtaken, not all done: K5 and K4(a) are still red (the list of holds above). The present is in
"Start here" at the top of this page; "What binds every step", at this section's end, still holds.

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
  Fable 5.2 came back for one side task on November 15 and 16, the last days it was in his plan, and Luis then passed
  the work to **Opus 5.5 at max effort with ultracode** again
  ([his words](Correspondence/2026-11-16_FABLE_WRAPS_UP_AND_A_SUBREDDIT_FOR_THE_BUILDING_RESEARCH.md)). "Max" is the
  master's effort; sub-agents run at medium effort unless a task needs more.
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
| The newest build Luis has | `WindowsOrdinaryPlace_Nov13` (`D:/Dev/WG/Builds`), played by him both ways on November 13 (`Validation.md`, "The solver's turns on by default"; `GAME_VISION.md` row 2026-11-13, night). `WindowsOrdinaryPlace_Nov13b` went to him and is off his list by his word (November 15) |

Luis's own uncommitted files in the main project are never committed, reset or discarded:
`Assets/_WonderGather/Scenes/TheGroup.unity`, `ProjectSettings/PackageManagerSettings.asset`,
`ProjectSettings/URPProjectSettings.asset`, `Assets/_Recovery*`. What stays out of commits in the worktree is listed in full in "Start here" (three settings
files, `Assets/ML-Agents*`, the learnt models). Stage by path.

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

**Luis answered the twelve questions** (November 15, morning; his words are in
[Correspondence](Correspondence/2026-11-15_THE_TWELVE_QUESTIONS_ANSWERED.md)). **Processed** (November 15): four rows of
November 15 in `GAME_VISION.md`:
- his decisions (1, 2, 6, 8, 11, 12);
- the target computer and the scale (7): his computer at the best settings, "very well"; lower settings below; the
  numbers to be proposed by research, the unit count still Open;
- his new asks, ordered by Claude;
- **the five calls left to judgment** (3, 4, 5, 9, 10), made by the design principles, each an option behind a switch,
  off. Explained to him in plain words, **he approved all five** (November 15, early afternoon;
  [his words](Correspondence/2026-11-15_THE_FIVE_CALLS_APPROVED.md)). They are rulings, not looks: each look still goes
  to judges, then to him. The planned pre-review was not run; its two questions (the rest blinks never a timer; the
  garment shapes carried to every body) go to each item's judges when it is built.

The order of the turns (his question 12: they take turns):
1. the ankle mend: researched, proposed, and **approved by Luis** (November 15, afternoon;
   [his words](Correspondence/2026-11-15_THE_ANKLE_MEND_ANSWERED.md)). What he approved: the three switches built, off, and
   tested; Small's learnt addition off as played, kept for training. **Not built yet** (November 16, late evening):
   the weekly reset has passed and the build is the next action ("Start here"); what it holds is in "Next steps", the
   get-up blow-up thread;
2. the visual turn: tier 0 instruments, step 0's stills, the target-budget research. **The target research**
   (November 15, afternoon; `Research/2026-11-15_TargetAndScale/`, research only): written (opus); critiqued (sonnet,
   `critique.md`: a sound frame, not ready, six fixes). Revised in place (opus, its section 7); a fresh critic
   (`critique2.md`) finds it **sound to propose after five small corrections**. They are listed at the end of section
   7 and are made after the weekly reset, before tier 0 builds anything. Group A's four questions went to Luis in
   plain words (November 15, afternoon): which screen, the strokes' size, a steady 60 or higher, the lines that stop
   the visual work. **He answered all four as recommended**, and asked that every common screen size work, since other
   people will test and play the game ([his words](Correspondence/2026-11-15_THE_TARGET_QUESTIONS_ANSWERED.md)): tier 0
   measures the common sizes too. Groups B and C come after the crowd measurement and later;
3. the search of the best ways for Claude to make assets, before the world's first pass;
4. the world's options (11) in that pass;
5. the bigger exploration of the world and its assets.

Still waiting: nothing from Luis right now. His next questions come with the work: group B after the crowd
measurement, and the turning proposal, `AnkleBrakes` and the natural ankle when the ankle build reaches them.

The build `WindowsOrdinaryPlace_Nov13b` is off the list, by his word (November 15, afternoon;
[his words](Correspondence/2026-11-15_THE_NOV13B_BUILD_SKIPPED.md)). Nothing new was left to judge in it: it is the
November 13 build with his chosen turns on from the start, but it lacks the skirt's flaps on their seams (on since
November 14), and Small still uses its learnt addition when getting up by its own body (it carries the models and
`RiseAgent.Unheard` is false). The next build, after the reset, goes to him with a short list of what to switch on,
do, watch and tell.

He answered the target research's first four questions on November 15, afternoon.

His word on (b)'s build and his call on Small's learnt addition came on November 15, afternoon: build it, and the
addition off as played.

The work goes on meanwhile. No merge to main without his word.

Before that, on November 11, he answered the six questions of `motion_design.md` section 7: **yes to all six, as
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
     **Where it stands (November 13, night):** (1) and (2) done, uncommitted in `D:/Dev/WG` (OwnBody, MinerPanel,
     BodysOwnBench, TestArguments `-turnsAsMeant 1`, the turns test, the facts test's fifth way); the switch is **off
     from the start**; its run passed (`turnsswitch.log`; Validation, "the solver's turns fixed in the game, behind a
     switch"). (3) **done**: both reviewers "sound with changes" (`Docs/Reviews/2026-11-13_SolverTurns/review.md`,
     each finding and what was done). A's three: the sleep test (`EachMinersOwnBodyStandingStillIsNotPutToSleep`,
     passed: never asleep, either way), the bench reads its turns back (`-ownIterations` now kept), and `RISE_BITS` in
     `OwnRiseBench` to show the switch off is the game as played to the last bit: `TestRuns/turnsoff_new` (with the
     switch's code) read; `turnsoff_old` (at `2ed1c7c`, the six files set aside with `git stash` in `D:/Dev/WG`,
     since put back and checked the same as the copies kept beside) read: **the bits and the courses differ**, but
     `turnsoff_old2` (the old code again) **differs from itself too**: the bench does not repeat itself run to run
     (its waits before it steps by hand are in frames), so A's 7 is **still open**, shown on the facts' leg only.
     Next for it: both makings in one run from one kept start, the new one run twice first. B's are for the next
     step (B4 blocks the unfloored share of R1; K8's R; K5's count); B7 said in `motion_design.md` K8. (4) `ownoff`
     and `ownon` read: 14 of 15 each way, only the shove test failing (Small 25.0 and 22.7 m/s; Round 17.1 off, 4.2
     on). The decision is in `GAME_VISION.md` (2026-11-13 row). Records written (Validation, review.md); committed and
     pushed (`957a21b` on `claude/wip-round`, `38adadd` on `claude/worker-showcase`); the build
     `Builds/WindowsOrdinaryPlace_Nov13` in `D:/Dev/WG` made (Result: Success) and the report sent (November 13,
     night), its first line the switch, off by default. **Wait for Luis's word on which way the game plays**; then
     A's 7 as above, and the next step under B's bars (it waits on the week's reset as well).
     **Luis's first look (November 13, night; [verbatim](Correspondence/2026-11-13_WHAT_TO_JUDGE_AND_THE_TURNS.md)):**
     answered: the switch changes only the body standing by its own joints (at ease, the catch step, Small's own
     getting up when "Gets up by its own body" is ticked, off by default), not the walk (`ProceduralBiped`, unchanged
     since October 13); the getting up in the build is October's, the design's new one is not built (step 0 only).
     **His asks, the first work after the reset, before the step-1 work above unless he says otherwise:** (a) a deep
     research of how people turn while walking, sharp turns and turning right round (a workflow: studies and motion
     capture read frame by frame), then the walk's turns rebuilt from it; (b) the odd leg positions in turns and the
     rarer twitches to **zero**: found by reading every frame of many turns as played, with numbers (one lead, not
     checked: the walk turns the body at up to 420 degrees a second and holds it where a planted leg is twisted 62
     degrees in its hip, `MostTwist`, then lets it go on: a stop and start at a limit); (c) a leg seen detached while a
     miner lay trying to get up (his picture; with Gets up by its own body ticked, so the October way up by its own body, most likely Small: Luis, the same night): the fall's body is
     jointed rigid bodies whose joints can pull apart (projection at 2 cm and 5 degrees, `PhysicalFall.cs:388`), the
     standing body's cannot; or the drawn leg; not checked.
     **His word on the switch (the same night, [verbatim](Correspondence/2026-11-13_PREFERS_THE_NEW_AND_LOVES_THE_IDLE.md)):**
     he prefers the new one (on, 14 and 4), and "really really" likes the idle standing, its little movements and how
     the body answers them ("really alive"). **First after the reset:** the switch's default made on
     (`OwnBody.TurnsAsMeant = true` in `D:/Dev/WG`; the switch stays on the panel), `OwnBodyTests` and the flows as
     played (`-ownAsPlayed 1`) run and read before its commit; then (a) to (c) above. The idle standing at 14 and 4 is
     loved: every change to the own body keeps it, set side by side with this build, switch on.
     **And (the same night, [verbatim](Correspondence/2026-11-13_SETTLING_AFTER_A_SHOVE.md)):** (d) after a shove it
     catches itself well but does not settle back into its own comfortable stance, as people do once they have
     recovered; and people shift their stance now and then so as not to feel stiff (some of that is there). He asked
     whether it is worth exploring or already a later step's. Answered: worth it; its research joins (a)'s (moving
     between stances: turning, recovering, settling, shifting weight); built on the design's stances and steps,
     growing smoothly with how far from comfortable the body stands (no threshold), beside the loved idle, never over it.
     **After the reset (November 13, afternoon, Luis: "I did the reset"; the week and the five hours at 0%):** the
     settling question answered (worth it; not in the design as written: searched; its research joins (a)'s). The
     default made on, uncommitted in `D:/Dev/WG`: `OwnBody.TurnsAsMeant = true` (comment: Luis's word), and
     `TestArguments`: `-turnsAsMeant 0` now runs as before (6 and 1), anything else 14 and 4; the suite gives back the
     switch it found. **Running, one after the other (no edit in `D:/Dev/WG` until they are read):**
     `ownbody_default_on` (`OwnBodyTests`, class named), `asplayed_on` (the whole suite, `-ownAsPlayed 1`),
     `asplayed_off` (the same with `-turnsAsMeant 0`); then compare on and off as played, commit and push, a build.
     Beside them, the research of (a) and (d) as a workflow, written to `Docs/Research/2026-11-13_TurningAndSettling/`.
     (The workflow `wf_cb146d66-72d`: three researchers, R1 turning while walking, R2 turning right round and what
     looks wrong, R3 settling and shifting, then one sceptic who re-checks the load-bearing claims and writes
     `research.md` with a half page for Luis first.) **Next for (b), once the runs are read:** a turn bench in
     `D:/Dev/WG` (explicit, its bars set before its first run) that sends each miner round at 45, 90, 135 and 180
     degrees, from walking and from standing, both ways, at walking and jogging pace, many times, and reads every
     rendered frame (as Luis sees it): each hip's twist against `MostTwist`, the knees, each planted foot's slide and
     turn on the ground, the body's turning pace, and every bone's turn from frame to frame (a twitch is a spike there);
     then each fault found and fixed one at a time, to zero, all three miners. The turn itself rebuilt only after the
     research, in a design Luis agrees to.
     **The research done and Luis's word (November 13, afternoon):** `research.md` and its three notes committed
     (`604a1a3`) and sent to him with a recommendation on each of its six questions; **he approved all six**
     ([verbatim](Correspondence/2026-11-13_APPROVES_THE_SUGGESTIONS.md); the vision's row of the same afternoon): the
     body's turn tied to the steps; a planted leg stepping round at about 35 degrees of hip twist, once it is measured
     whether 62 is what bends the legs; the head a little ahead of the hips; the foot that is down choosing a wide side
     step or a crossing pivot, the swinging foot going round the standing leg; settling after a shove as an option
     beside what it does now; the loved idle unchanged. **Standing, both from the same afternoon:** research the best
     way forward before every step, and keep every step true to the game's design philosophies of mechanics and
     aesthetics; and "always check for movements that feel unnatural. Be very rigorous about trying to capture all
     kinds of unnatural movements." `ownbody_default_on` read: **16 of 16** (the shove test passing too). **The turn
     bench** is drafted and reviewed outside the worktree by a workflow (`wf_8f312c44-139`; no Unity, nothing written
     in `D:/Dev/WG`) while the suite runs: `TurnBench`, explicit, every frame, all three miners; its faults, bars set
     before its first run, each 0: F1 a planted hip twisted past `MostTwist` + 2 degrees; F2 a flick (a bone over 6
     degrees in a frame and back over 4 within 2); F3 a jump (a bone over 15 degrees in a frame); F4 the legs through
     each other; F5 a planted foot sliding over 2 cm or turning over 25 degrees in a stance; F6 a knee over 35 degrees
     off its foot. **To add before its first run, for Luis's second ask:** a catalogue of every kind of unnatural
     movement (studies of how people judge motion, animation and biomechanics measures, and every fault Luis has
     flagged), a detector for each with its bar, kept as one scan any bench can run (the walk, the turns, the catch,
     the idle, the getting up), at 50 frames a second and at the game's own frame rate (the memory "every frame as
     played"), the flagged frames shown to judges. **Next, in order:** read `asplayed_on` and `asplayed_off`, compare,
     commit the default and push, a build (`WindowsOrdinaryPlace_Nov13b`, the switch on from the start); the scan and
     the bench into `D:/Dev/WG` after their reviews, the baseline run on the walk as it is, read; then the turning
     design from the research, the baseline and the game's philosophies (each named, and how the design keeps it),
     checked by judge agents on mechanics and aesthetics, sent to Luis to agree before the turn is rebuilt, as a switch
     beside the walk as it is; (b)'s faults outside the turn's design (such as at the hand-overs) fixed one at a time
     meanwhile; then (c) and (d). **And (moments later, verbatim in the same file):** a deep research into all kinds
     of movement that can be seen as unnatural, for the rest of the project too: the workflow `wf_89a9400c-95e` (three
     researchers: how people perceive it; animation craft, motion-quality measures and biomechanics; the project's own
     record of every fault and detector; then a sceptic who re-opens the load-bearing sources and writes
     `Docs/Research/2026-11-13_UnnaturalMovement/catalogue.md`, each kind with a per-frame measure, a bar and its
     source). The scan's kinds and bars come from it, set before the bench's first run. **The bench's draft and its
     review came back** (`wf_8f312c44-139`): "ready after fixes", eleven of them; the two that matter at the
     hand-overs: a picture taken inside the bone loop corrupts that frame, and the top bone's turn must be read in the
     world (OwnBody.Follows turns the root under the drawn pelvis, so its local turn jumps falsely). Kept in this
     session's scratchpad, folder `turnbench` (`TurnBench.cs`, `suspects.md`, `review.md`), to go into `D:/Dev/WG`
     with the fixes and the catalogue's scan once the runs are read; a first quick run with `-turnCourses 32` (all four
     kinds; 480 courses are about 360k frames). A lead found on the way, not yet measured: the walk always points the
     knee along the pelvis (`ProceduralBiped.cs:1300`), so in a turn a planted knee may point up to 62 degrees off its
     own foot (F6 measures it): a likely cause of the odd legs.
     **Stopped at the usage limit (November 13, afternoon), in this order:** (1) the as-played runs read: of the 15
     that fail both ways, 13 are the October kind (tests that take the body by hand or measure a hand against the old
     body; Validation's October 10 and 14 entries, `TheBodysOwn.md` "The whole suite, as the game is played"); the
     other 2 are K5 and K4(a) (`ArticulationFactsTests`), **findings that have failed their bars since November 12**
     (Validation, "K5, the speed sweep" and "K4(a), the load through the target"; they build their own legs at 14 and 4
     and never read the switch). Run the normal way with the default on (`facts_default_on`): 20 of 22, the same two.
     (Said wrongly in the session, before the record was read: that they had passed on November 12 and 13.)
     **After the reset:** the whole suite the normal way with the default on (`normal_default_on`) running; the
     catalogue's workflow resumed (`wf_89a9400c-95e`; the three researches cached; the sceptic and writer continue from
     `sceptic_notes.md`; an independent reviewer added after it); (c) investigated read-only, nothing run (`wf_ac0e021a-bd0`, an
     investigator and a sceptic; [the write-up](Research/2026-11-13_DetachedLeg/investigation.md)): most likely **the
     cut trousers showing**, a drawing fault (Small's and Round's trouser mesh ends at 0.486 m under the coat, with no
     leg drawn above it, so a thigh out of the skirt shows an open tube end; the picture's tube length and square end
     fit), uncovered by the lying own body's free hips (no hip limits, `OwnBody.cs:763, 789-821`) or by the skirt's
     flaps turning to the thigh's wrong face past 90 degrees of hip bend (`MinerBody.cs:655-656`). Also confirmed in
     the code, effect unknown: during each try `PhysicalFall.cs:806-808` puts the posed body's and the balance's acting
     back on after `OwnBody.cs:881-883`, and `PhysicalBalance.Falls` (213-219) does not check the own body. Order to
     test: Small's drawn model posed without physics over a grid of hip bend, spread and twist, with a cover test of the
     cut ring seen from the game's camera; `Physics.GetIgnoreCollision` for the unit's capsule against the own body;
     then the getting up through the game's own flow on Small, with Round on the old way and Small with the switch off
     as controls. Any fix to the drawn body goes to aesthetic judges first. **Done since:** the default on committed (work
     branch `6773465`) after the whole suite the normal way, 206 passed, 2 failed (K5, K4(a)) (Validation, "The solver's
     turns on by default"); the build `WindowsOrdinaryPlace_Nov13b` made from it. **The catalogue is written and
     reviewed** ([catalogue.md](Research/2026-11-13_UnnaturalMovement/catalogue.md); its section 1 is for Luis): 108
     kinds in 11 families (C contact, B balance, T timing and effort, S smoothness, J joints, O coordination, W weight
     cause and reaction, L life and variation, H held things and clothing, Z body size, V the screen), each with a
     per-frame measure, a bar and where the bar comes from; Luis's flags mapped to kinds; the reviewer fixed false alarms
     (C1 had measured the foot's own rolling centre of pressure) and misses (the drawn body to be recorded at 100 fps),
     and added 12 kinds. **Its blocker:** the game records no contacts, segment masses, centre of mass, drive forces or
     controller mode changes, which about a third of the kinds need; a recorder for them comes before those kinds run.
     **For the turn bench:** F5 (planted-foot slide) becomes 1 cm per stance, 2 cm a fail; J4 flags a hip twist over 35
     degrees and faults it over 45, while the walk allows 62 (`MostTwist`): both bars kept, the choice Luis's. Its
     questions for Luis are in section 1 (how a bigger body slows, which camera views count, how long a get-up may take,
     among others). **Next, one agent at a time, medium effort** (two high-effort workflows side by side took the
     five-hour window from 9% to 53% in 25 minutes): a builder puts the review's eleven fixes and the catalogue's scan
     (the kinds today's trace supports, at 50 fixed and at the game's own rate, the drawn body at 100) into the turn
     bench in the scratchpad, a reviewer checks it, then it goes into `D:/Dev/WG`, a quick run with `-turnCourses 32`,
     read, then the full baseline; then the turning design for Luis. **Meanwhile, no agents before the 22:30 UTC
     reset:** (c)'s capsule check is done and **rules the capsule out**
     (`OwnSolidAgainstOwnBodyTests.SmallsOwnSolidLeavesItsOwnBodyAlone`, work branch, run `solid_vs_own2`, passed;
     Validation, "The unit's capsule against its own body": 25 tries, the capsule inside the body on 14,557 of 14,562
     frames, every pair ignored on every frame, 0 contacts from either side, while the controls saw contacts). Next in
     (c): (a), the posed cover grid. Small's drawn body is held still as the atlas does it (`AtlasPoser`), lying on its
     back, one leg through a grid of hip and knee angles, the other foot down or raised. The cut trousers' ring is cast
     toward the game camera and straight up through the baked skin, and the flaps' faces are set against the thigh's
     true front. Run alone, read, then pictures at the worst poses; any fix to the drawn body goes to aesthetic judges
     first.
     **Where (c)(a) stands (November 14, night):** run and read (`DrawnCoverGridTests`, work branch; Validation, "The
     posed cover grid"). The cut ring is seen in all 36 poses as until now; most of it was the flaps (three causes in
     `MinerBody.Drape`). Fixed behind `MinerBody.FlapsOnTheirSeams` (off by default; a toggle on the miner panel): a
     flap pressed onto a face with no way down turns toward its seam's line; standing and walking unchanged
     (`FlapSeamTests`, `seam_same2`, within 0.14 degrees). Lying, legs straight: 100 to 66 ring points seen. Left: from
     the feet and with a leg raised past square (the front flap falls toward the head); the root is the art cut
     (`Art/Blender/Worker/workers.py`, `hidden_above`, Small and Round), an exploration to propose to Luis. Next: the
     two picture runs with `-coverShow` (the same poses and ways, switch off and on), three aesthetic judges one at a
     time (brief in the scratchpad's `judges_flaps`, copied to `Art/Review/judges/aesthetic_flaps.md`), commit by
     path, push both branches, report (first line: the switch, off by default); then (c)(c). **Run and read
     (November 14, evening),** each under the runner's new wall-clock `LIMIT` (a picture run in batch mode had hung
     16.5 hours unnoticed; Validation): `panel_click` and `panel_picture` (`WINDOW=1`) passed; `cover_show_off` and
     `cover_show_on` (`-coverQuick 1 -flapSeam 0|1 -coverOut Captures/CoverGrid/show_off|show_on -coverShow
     "0,0,45,10;90,0,0,10;90,0,0,70;90,0,90,40;0,0,0,100;0,0,0,130"`) gave the rest as before (49+51; 36+30). The
     seven blind sheets: scratchpad `judges_flaps/pose_0..6.png` (made by `judge_sheets.py`; which row is which, in
     the scratchpad's `private/key_flaps.txt`, not shown to the judges). The pose pictures carry no red marks (only
     the control pictures do); the brief says so. **Judged (November 14, evening;
     `Reviews/2026-11-14_Flaps_Judges`):** the referee found the switch on truer in 6 of 7 poses, the same in pose 4
     (which leans on too, enlarged); left against it: a pointed front hem, and (both ways) a flap standing over a leg
     raised past square. **The report went to Luis (November 14, about 18:00 local)** with the picture `off_on.jpg`;
     asked: the default, the art-cut exploration, a stop for the raised leg's front flap; wait for his answers on
     those three. Meanwhile (c)(c), which needs none of them: a read-only map of the
     code for the game-flow courses with a cover check every frame is in the scratchpad's `cc_map.md`. **Planned and
     its bars set before any run** ([courses_plan.md](Research/2026-11-13_DetachedLeg/courses_plan.md), main `9cb85de`):
     the switch only changes what is drawn, so each frame is measured as played and again with the flaps laid the
     other way (paired), with a self-check that equal ways read equal. **Built (November 14, evening; wip-round
     `b80a7bd`, `46124ca`, `3261ae1`):**
     - (a) The grid's meter moved into `CoverMeter`. Its results are md5-equal before and after, both ways.
     - The drawn-hip reader `DrawnHip` is exact: `DrawnHipPoseTests` reads 160 poses exactly; live, it agrees with
       the own body's links to a median 0.4-0.5 degrees.
     - (b) `CoverRingLiveTests.TheCutRingAsPlayed`, with `live_cover_summary.py` and its `hip_check`. The
       self-checks `cc_self_off2` and `cc_self_on2` passed: 0 mismatches, the played flaps restored bit for bit, about
       35 ms a measured frame.
     - The review was "sound with changes". Its two must-fixes are done: the thigh's true front, and the hip reader
       checked live. The plan's amendment, written before any course ran, is in `courses_plan.md` (bar 5's third
       clause replaced: by design, `ByItsOwn` sets LetGo false).
     - Seen already in the self-checks:
       - the ring is seen from some way on 383 of 386 lying frames;
       - flaps on: 0 flap-face faults;
       - real hip folds past 125 degrees in the own get-up (footB, pike, rising): the own hip is a free spherical
         joint with no limits, and Luis sees the getting up's legs as unnatural.
     - **The flaps on by default:** wip-round `b7e24de`.
     - **Courses 1-3 and `FlapSeamTests` as played: run, read and recorded** (18:03 to 18:26 UTC; the runs `cc_c1`,
       `cc_c2`, `cc_c3`, `cc_seam_played` in `TestRuns/CoverLive/<run>`; Validation, "The cut ring as played"). Read
       by the workflow `read-cc-courses` (a reader, then an independent verifier with its own script; every number
       agreed), and the turn-free hip counts a third time by Claude. In short:
       - the cut ring is seen from the game camera's ways on nearly every lying and getting-up row, both ways: the
         art cut, so the models' redraw;
       - the flaps on their seams bring the wrong-face flaps from thousands to a handful;
       - `FlapSeamTests` as played passed;
       - one body always (bar 5: 0).
       **Found:**
       - course 4 of `cc_c1` (Small's own get-up) blew up: spin to 8e21 rad/s, then NaN. The test still passed,
         because failing log messages are ignored;
       - `cc_c1`'s own get-ups: up by its own body 7, not up 1;
       - the own get-up's hip folds to 172.4 degrees, past a human's range on 8146 of 19376 getting-up rows (turn-free;
         the old way 77 and 65). The own knee is past 150 degrees on 1574 leg-frames;
       - jumps and spins in the own get-up;
       - the drape under the ground on a third of the own get-up's rows.
       **The summary script's printed bar 3 was wrong** (the test's unwrap drifts by whole turns); fixed, it reads bar
       3 turn-free (wip-round `e9213d3`), checked row for row against the raw columns.
     - **Reported to Luis (November 14, late evening)** with the picture `Research/2026-11-13_DetachedLeg/courses_on_off.jpg` (the same frame both ways).
       Proposed order: (a) the blow-up's trace, and a test that fails on it; (b) natural joint ranges researched, then
       a switch, judged, proposed before it is built; (c) the clipping detector; (d) the models' research; (e) the
       flap's rest and the hem. Wait for his word on (b)'s build; (a) and the research may go on. **His word came**
       (November 15, afternoon): build it.
     - **The models' deep research, part one, done and committed (`6b48126`)** in
       `Docs/Research/2026-11-14_TheModelsRedrawn/`:
       - `internal.md`: the guides, the vision and the code;
       - `audit.md`, `pictures/` (sheets in `pictures/sheets/`) and `audit_tools/`: the three LOD0 miners rendered
         in Blender, at rest and in six bone-turned poses, and measured;
       - `external.md`: 44 tagged sources.

       What it found:
       - The guides never ask for a thin waist. It comes from the garments: the top is tucked to 0.82 and the
         skirt is fitted to that tuck.
       - The visible waists are about 0.09 H (natural is about 0.15-0.18 H). Round is the worst: her waist is 0.41
         of her chest and 0.30 of her hips, so she reads corseted.
       - Small's and Round's trousers end 13 cm and 7 cm below the hip joint (the floating legs). Long's trousers
         are whole, which is the pattern to follow.
       - The skirts act as four rigid boards that thighs pierce or push out.
       - Further faults: thin limbs with no elbows or kneecaps, flat side profiles, and the same front silhouette
         on all three.
       - To keep: the faces, the colours, the heights, the props, the boots and the gestures.
       - Natural waist-to-hip ratios are 0.5 and up. Tailoring makes a coat's waist with a seam, and the skirt
         flares below it.

       **Part two is done** (workflow `models-research-synthesis`): the synthesis `research.md`, revised after the
       critic's 21 points (`critique.md`; 20 were taken, 1 in part). Its findings:
       - Two causes. The garment's build makes the waist and the floating legs. The rigid flaps and the drape rule
         make the clipping. Long's thin waist is his own body numbers.
       - Reference E: the round woman's coat hangs straight over a barrel body.
       - The getting-up leg folds are most likely the physical body, not the model. Unconfirmed.

       Its path:
       - Step 0: Blender stills of each miner, three ways side by side (today's, "look only", "fuller body"). The
         aesthetic judges see them first, then Luis picks the depth. Nothing is built before that.
       - Then A1: static art (the waist from each body's own numbers plus looseness, whole trousers or a simple body
         under the coat, the coat's top weighted to the thighs).
       - Then A2: flaps that follow the thigh, with leg capsules, behind a version switch.
       - B (proportions) only with Luis's word.
       - Safeguards: the new script paths off by default; a check that the first version exports as shipped; a
         table of the physics numbers that stay and those that follow the cloth.

       **Sent to Luis (November 14, night) as a proposal**, with his questions (the research's section of
       questions). **Next:** after the reset, the energy count first (the check-in launches it), then step 0's
       stills (an opus builder in Blender, aesthetic judges on sonnet, before Luis), unless his answer changes
       the order.

       **Luis's answers (November 14, night; [his words](Correspondence/2026-11-14_THE_MODELS_PROPOSAL_ANSWERED.md)):**
       1. The order stands: the energy count first, then the pictures.
       2. He likes today's faces. Ways to make them even prettier, by the aesthetic guides, are to be explored. The
          stills show today's face beside the explored ones.
       3. The stills may try a head change. Head proportions stay Open until he picks.
       4. A super deep dive research, if worth it, into the best way of making the models and all the visual parts
          Claude can do, for the best quality possible, "no efforts saved". Claude is also to think proactively
          about quality everywhere.

       **After the reset (02:10 UTC, November 15), launched at 3% of the five hours and 62% of the week:**
       - **The energy count is built and reviewed: wip-round `34316c6`, pushed.** Only tests changed:
         - new: `LiveEnergy.cs` (the instrument) and `LiveEnergyTests.cs` (the one-part checks);
         - changed: `CoverRingLiveTests.cs`, behind `-liveEnergy`, which defaults to 0.

         What the build found:
         - The energy is reckoned from the root's going and each link's turning. What the joints give comes from
           the engine's accumulated force and torque plus each drive's work. The shove trace's way would have
           missed the riser's torques.
         - The one-part checks: 4 of 4 pass, within 0.0001 J.
         - The self-check with the count on passed. Standing still, the body loses 0.02 J a step. Getting up,
           single steps swing from +41.5 J to -44.3 J.
         - The self-check with the count off was failed by the guard, as built: **a second whirl, FootR at 108.8
           rad/s with the right knee folded past 180 degrees**, in footA of the third try. The course was taken
           back.

         A fresh opus reviewer found it **sound**, with only optional suggestions:
         - an off-axis two-part check;
         - an `e_gain_least` column;
         - guarding the let-go body too;
         - whether 100 rad/s should end a course or only log.

         The 100 rad/s line is kept: the onset trace is written at 30 rad/s either way, and a foot past 100 with
         the knee past 180 is already a fault. The report is in the scratchpad's `energy_build_report.md`, and the
         review in the workflow's journal.
       - **The long run, done and read** (`cc_en24`, Small's own get-up, 24 courses, `-liveEnergy 1`; Validation,
         "The energy count in the live courses"). The test failed, as the guard reports: **6 blow-ups in 24**, every
         one in a foot and its toe, while getting up (footA, footB, rollup).
         - In one 20 ms step the foot went from 6-37 rad/s to 102-128.
         - The energy came in through the ankle: the foot's link was counted as given 22-103 J in that step, and the
           toe's joint gave nothing.
         - Little came unaccounted, in 5 of the 6.
         - Short of the line, feet reached 64-99.7 rad/s in courses that went on to stand.

         The readers are `live_energy_read.py` and `live_energy_last.py` (wip-round `cb893d2`).

         **Next in this thread:** split the foot's given work into the ankle's drive, the keeper's or riser's
         torque, and a pull on the foot. Add to the ring the ankle's target, angle, limits and gains, and the foot's
         and toe's contacts. Then read the cause, and research the mend in the controller (stops and dampers, not
         guards; K1).
         - **The split is built and reviewed sound:** wip-round `1a9f803`, pushed. It sits behind
           `-liveEnergy 2`; level 1 is unchanged. Tests only.
           - Each ring step's given work is split into p (pushes), r (the riser's torque), k (the keeper's),
             o (other torques) and d (drive).
           - It also records the ankles' and toes' joints per axis, every link's contacts, and the riser's asks of
             each boot.
           - The brief is the scratchpad's `brief_ankle_split.md`, and the report `ankle_split_report.md`.
         - **The long run is done** (`cc_es24`, 24 courses, `-liveEnergy 2`, November 15, morning). **9 of 24 blew
           up**, all in a foot or a toe.
           - **Read and verified** by a fresh opus verifier (workflow `es24-verify`). It is recorded in Validation,
             "The ankle split in the live courses: what kicks the foot (November 15, morning)". In short:
             - In all ten blow-ups (and as_l2's), 0 to 3 steps before the guard, the same leg's ankle target turned
               140 to 153 degrees in one step.
             - In 8 of 9 (and as_l2), it is `OwnRiser.AnkleCan`'s clamp flipping from +110 to -110 degrees as the
               ask passes 180 degrees from stood (`AnkleAtMost` 110, rate limit off): 140 the short way, for any
               stood (reproduced in Python). c19 is the ask itself jumping, with the boot far from its aim.
             - The flip's step puts +12 to +38 J into the ankle's drive. In six of the ten, other flows were as large:
               the engine's gain, a knee or a hip.
             - The flip came 7 more times without a blow-up. Three of those had an ankle damper 5 to 8 times
               stronger. The damper comes from the riser's inertia estimate `lighter[foot]`, which differed about a
               hundredfold between tries. Why is not found yet: **a lead**.
             - As played, the learnt policy's additions (`drivenNow`) were 14 to 23 degrees in some courses. Whether
               that is meant was not checked.
           - The reader is `live_energy_split_read.py` in the tools folder (wip-round `3182e08`), with the verifier's
             nine faults mended. It gives the verifier's numbers.
           - **The mend, researched** (November 15, early afternoon; stops and dampers, not guards; K1), joined
             with the natural joint ranges: `Research/2026-11-15_NaturalRangesAndTheAnkle/`.
             - `research.md` (opus; workflow `ankle-mend-research`, run `wf_979da6b6-89d`). Its findings:
               - a second fault: in c0 and c8 a boot came 0.55-0.71 m off its shin. `GoDown` takes the ankle
                 point from the physics boot, while the let-go body is built from the lagging drawn pose. This,
                 not the ankle's range, explains the hundredfold `lighter`;
               - every clamp onto a ball of turns flips at 180°. The best fit is swing-twist with an elliptical
                 cone, smoothly saturated, folding back to neutral at the far side;
               - the laid-boot kneel needs about 90-130° of plantarflexion, where people give 50-63°. Form the
                 asks in joint space;
               - `drivenNow`: as played, Small's learnt models add up to 20° after the clamp, never cleared after
                 their stages. Long and Round get none. Whether to switch it off is Luis's call;
               - three switches, all off; 24 courses per arm per miner tell a full fix from chance.
             - `critique.md` (sonnet, 12 points). Two refutations:
               - a damper floor alone does not bound the boot's speed while the drive saturates;
               - the fold still jumps at a pure 180° twist.
               It also warns that the boot-off-shin fix removes the damper that made three flips harmless, so it
               must come after the others.
             - **Revised and checked again** (workflow `ankle-mend-revision`, run `wf_a12e80b8-163`):
               - the opus reviser answered all 12 points; `research.md` section 8 maps each one;
               - the fresh sonnet critic's `critique2.md` says **sound to propose, as switches, off**. It reproduced
                 the bound: Small at most 19.6 rad/s against 81 today, no overshoot, and the drive brakes;
               - its remaining points change nothing in the design. They are carried to the build in
                 `research.md` section 8.1. Read them before building.
             - **The proposal** (section 7). Three switches on the riser only, all off by default:
               - **A**, `OwnRiser.AnkleLimit`: the fold-safe limit (1 at today's 110°; 2 the natural ankle);
               - **B**, `OwnRiser.AnkleSpeed`: the error held within τcap/k of the joint's reading, with a smooth
                 damper floor;
               - **C**, `OwnBody.BootsFromOwnShin`: the boot put back on its own shin at the going down.
               - The order: B, then A = 1, then both, then C, never C alone. Then A = 2 with the asks formed in
                 joint space. The forced checks come first, then 24 courses per arm per miner, on Small, Long and
                 Round.
               - **Luis's call:** the learnt addition (`drivenNow`) on Small as played. The proposal is off, since only
                 Small has it; or kept for training, added before the limit and cleared after its stages.
             - **Sent to Luis in plain words** (November 15, afternoon). **He answered** "Sure." to the build, and
               "Yes option a sounds good." to the learnt addition: off as played, kept for training
               ([his words](Correspondence/2026-11-15_THE_ANKLE_MEND_ANSWERED.md)).
             - **The build, after the weekly reset** (November 16, about 17:00 his time; not begun that day, its steps
               are in "Start here"), in `D:/Dev/WG`:
               - first read `research.md` section 8.1, and work Long and Round numerically;
               - `RiseAgent.Unheard` true by default (the learnt addition off as played), and the live test taught
                 to read `-ownUnheard`. Small's baseline runs both ways once, to show what turning it off changes;
                 the mend's arms then run as played;
               - the three switches, off; the forced checks; then the arms in section 7.2's order, 24 courses per
                 arm per miner, on Small, Long and Round; the idle's tick-by-tick proof;
               - since a default changes, the flows as played (`-ownAsPlayed 1`) before any build goes to him.
       - **The deep dive:**
         - Part one is done and committed (`22ff7f3`: `inventory.md`, `modelling.md`, `faces_heads_hair.md`,
           `surfaces_light_render.md`, `world_and_props.md`).
         - **Part two is done and committed** (workflow `best-visual-quality-synthesis`, run `wf_e6ad801c-a2d`,
           resumed after the reset on November 15, morning). The files are:
           - `research.md`: the programme, by opus, revised against the critic;
           - `critique.md`: the critic's 32 points, by sonnet. The revision agreed with 28 and with 4 in part,
             saying why.

           The programme's order:
           - instruments first: a Blender stills kit, a Unity shot set (with the 42 m Strategy frame, CPU time and
             a 100-miner crowd), and a range-of-motion sweep of today's miners;
           - step 0's stills on today's pipeline, flags off;
           - small Unity switches, off by default;
           - a v2 'carrier' rung (smooth face normals, hair without facets, a face map, faint vertex AO);
           - the template trial while Luis looks;
           - after his picks: the v3 body, then v4 garments, gated on the get-up on all three miners and an
             unchanged idle;
           - then the world, in rank order.

           Its 12 questions for Luis are in `research.md` and in the message sent on November 15, morning.
           **Answered on November 15, morning** ("Waiting on Luis"). Not done by it: no Unity or Blender run, no pictures, no web search, and
           no re-read of the revision by another agent.
       - Then step 0's stills, built by the deep dive's findings: today's faces beside prettier explorations, and a
         head change tried. The aesthetic judges see them first, then Luis.
       - The weekly allowance is the binding limit (resetting November 16, 16:00 UTC). One five-hour window costs
         about 13 to 14 points of the week, so pace the work.

       The register rows for these answers are written in GAME_VISION (two rows, 2026-11-14 night).
     - Next, by his answers and the order below:
       - **the blow-up's energy count.** How it began is read from the frames (Validation): footB's fast left thigh
         and a knee at 165, a jump to 63 rad/s at the step to pike, then the right arm whirling to 1e8. To do:
         - checked: a course does not run again the same (`cc_det`, Validation). The blow-up cannot be replayed;
         - bring `OwnBodyTests`' `-ownShoveTrace` count (what the body holds against what its joints give, each step;
           see the memory "engine-step-adds-energy") into `CoverRingLiveTests`, kept for the steps before anything
           whirls;
         - make the courses fail on a body gone NaN or past the engine's bound, and end that course;
         - run Small's own get-up for many courses (say 24) until a blow-up comes again, and read which part gains
           first;
         - the build's brief is written (the scratchpad's `brief_energy_count.md`): an opus builder, then a fresh
           opus reviewer, launched after the five hours' reset (02:10 UTC, November 15; a check-in is set for just
           after it), as the allowance was at 60% with the models' research still running;
       - natural joint ranges for the own body, researched first (a switch, judged); the memory's K1 already names
         stops and dampers as the mend, so the ranges serve both the folds and the blow-up;
       - the skirt clipping detector;
       - the models' research, part two (launched at 71% of the five hours, see above), then the proposal to Luis.
     - The review's "should" items left open: the self-check compares counts, not points; the garment colliders
       stay in the scene; LOD0 is forced.
     - The thirtieth and thirty-first outlooks were taken (`Outlook.md`).
   - **Luis's answers (November 14, evening; [his words](Correspondence/2026-11-14_THE_FLAPS_ON_AND_THE_MODELS_REDRAWN.md),
     the decisions in GAME_VISION):** (1) the flaps on their seams **on by default**: change the default in
     `MinerBody` once the build workflow's runs are over (never edit the worktree during a run), then run the flows as
     played (`-ownAsPlayed 1`), and name the switch and its new default first in the report; (2) **the models
     redrawn**, a big step: Small's and Round's trousers continued under the coat, the waist less extreme, all three
     miners made much prettier and natural by the aesthetic guides, everything unnatural in every model looked for;
     how far (mesh only, or rig and body too) is open: **a really deep research first, as Luis asked a moment later**
     (like the turning research: the project's aesthetic vision and guides, `Art/Blender/Worker/workers.py`, what the
     physics body takes from the rig, an audit of all three with pictures, outside research on appealing natural
     stylised proportions and on garments over legs in motion, a sceptic's check), then a proposal he agrees to; a second version beside the
     first, aesthetic judges before Luis; (3) a raised leg's front flap given a rest and the hem's point rounded, the
     switch-on version only (Claude's call, as he left it), judged again. **New faults he sees:** the leg sometimes
     clips straight through the skirt (find when, and measure it: a detector beside the cut ring's); getting up still
     shows unnatural leg movements (the catalogue's scan on getting up; the live test's hip and knee bars are a first
     part). Order: the build and its courses (running, Unity only); the default; the models' research (a workflow,
     when the usage allows; it resets 21:10 UTC); the flap's rest and the hem; the clipping; then the turn bench.
   - *(The two items below are the morning's, kept as written, and done since: the review came back,
     `Reviews/2026-11-13_SolverTurns`, A and B "sound with changes", and A's sleep check passed (Validation); the
     report went; Luis played both ways and chose the new turns, now on by default. What B asks of R1's making-up and
     K5's bar stays open, under "Later".)*
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
- The first line of a report to Luis names any switch that must be on.
- An outlook of the whole project is due every 250 tool calls or so ([Outlook.md](Outlook.md)).
- The rules for claims and checks that every agent follows (a result said only after its run was read is one of
  them) are in `AGENTS.md`, "Evidence boundaries" (November
  16; why: [Research/2026-11-16_HarnessGuideReview/review.md](Research/2026-11-16_HarnessGuideReview/review.md)).
