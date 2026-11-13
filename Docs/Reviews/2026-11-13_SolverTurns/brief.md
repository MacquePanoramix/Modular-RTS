# Brief: an independent review of the solver's turns, the switch, and step 0 read again (November 13, 2026)

For two reviewers who did not write what is reviewed, each on its own part, **A** or **B**. Sized to finish inside a
small usage window: **create your own file beside this brief first (`A.md` or `B.md`), with a heading, and add each
finding to it as soon as it is reached**, so that a cut-off loses nothing. Read-only otherwise: no Unity run, no edit of
code, tests or other documents, no commit, nothing sent anywhere. The verdicts are advice to whoever builds, not the
Game Director's.

## Common ground

- Unity 6000.6.0f1, PhysX articulations (`ArticulationBody`), the PGS solver, dt = 0.02 s; the project's default solver
  turns are 6 of place and 1 of speed (`ProjectSettings/DynamicsManager.asset`).
- The work: the branch `claude/wip-round` in `D:/Dev/WG`, at `2ed1c7c`, plus its uncommitted diff, read with
  `git -C D:/Dev/WG diff -- Assets/_WonderGather` (the switch). Leave out ProjectSettings and untracked files.
- The record, in this project (`D:/Programs/Unity/Projects/Wonder Gather/The Wanderer`): `Docs/Validation.md`, from
  the heading "S3b step 2 — step 0's other facts; the solver's turns lost when set before the links (November 13)"
  (about line 5008) to its end; `Docs/Handoff.md`, the block "Luis's word (November 13, evening)"; Luis's message,
  `Docs/Correspondence/2026-11-13_FIX_THE_TURNS_AND_THE_WEEK.md`.
- The runs, in `D:/Dev/WG/TestRuns/`. The logs are large: read them with grep, never whole. `turnskept.log`
  (`FACTS turns kept`), `ownturns.log` (`OWN_TURNS`), `step0true.log` (`FACTS`), `turnscost.log` (`OWN_COST` or
  `turns`), and the switch's first run, `turnsswitch.log` (`FACTS turns kept`, `OWN_TURNS`) with `turnsswitch_r.xml`.
- The engine's source is not needed. If a reviewer cites PhysX, cite functions and its own reasoning, never pasted text.

## A: the finding and the fix

1. **The finding.** Turns set on an articulation's root before its child links are added are lost, and it runs at 6
   and 1. Does the evidence hold it (what the root reads back; courses identical to the last bit between the ways of
   `ArticulationFactsTests.TheSolversTurnsSetBeforeTheLinksAreKept`)? Is there another explanation?
2. **Other settings lost the same way.** Which of the root's settings in `OwnBody.Make`
   (`D:/Dev/WG/Assets/_WonderGather/Scripts/Units/OwnBody.cs`: the build loop near line 760, the toes after it) belong
   to the whole articulation rather than to the link, and are set before the links: the sleep threshold (OwnBody sets
   none), `immovable`, and the root link's own mass, damping, `maxAngularVelocity`, `maxDepenetrationVelocity`,
   collision mode. The switch's run tells some of them as read back (`OWN_TURNS`; the sleep threshold in `FACTS turns
   kept`). Which could matter, and how would one check?
3. **The fix.** `OwnBody.TurnsAsMeant`; `KeepTurns` once all the parts are joined, the toes included, and its check in
   each `Advance`; `MinerPanel.cs` (the toggle, and what the chosen miner's root reads back beside it);
   `Tests/PlayMode/BodysOwnBench.cs` (follows the switch; its rig near line 587; is the one near line 660 kept?);
   `Tests/PlayMode/TestArguments.cs` (`-turnsAsMeant 1`); the tests changed (`OwnBodyTests.EachMinersOwnBodyRunsAtItsOwnTurns`;
   the facts test's fifth way, which is to show that the switch off is the game as it was played). Is it correct and
   complete for every path that makes or remakes the own body (`Begin`, `BeginLow`, `BeginDown`, `Remake`), for a bench
   that steps the physics itself, for the switch turned while it stands? Does anything at runtime still run at turns
   other than the switch's?
4. **The project's rules** in the diff: runtime code free of `UnityEditor`; gameplay under `Assets/_WonderGather`;
   asset GUIDs untouched.

## B: step 0 read again at the true turns, and what it changes

1. **K4(a) at 14 and 4** (the record's rerun, `step0true`): the stances fell at 1.04, 1.54 and 1.16 s with the
   making-up and 0.98, 1.32 and 0.68 s without; Small's ankle gave 0.46 of its ask, 0.66 of K8's law's share; Round's
   share fell as the ask grew. Is the record's reading right? Is the proposed change to R1's making-up sound: the share
   read from what the drive gave (its `driveForce` against what was asked) rather than floored at the law's share?
   What could make that loop unstable (its lag of a step, noise when the ask is small, the ask changing sign), and what
   bound or filter must be set before its run?
2. **K5's per-step clause** trips at 10 rad/s by small amounts (steps 3 to 4), and at 5 rad/s at steps 19 to 25 on
   about a ten-thousandth of a joule. Is the proposed bar, the gain summed over the second at most half of what the
   dampers took over it, a sound test that the step adds no energy, or could it hide a real injection (gained early,
   paid back later)? Propose the bar to set before the run, with its numbers.
3. **The design** (`Docs/Design/AnyBody/motion_design.md` §6.2: K4, K5, K8, R1): what do the true turns make wrong,
   or require to be said again?

## The output

Each reviewer, in its file: a verdict line first (sound / sound with changes / not sound), then numbered findings,
each with the claim, its evidence (file and line, or the log line), its weight (blocks / should / note) and what to
do; at the end, what was not checked.
