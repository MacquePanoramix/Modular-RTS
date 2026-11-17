# Project working agreement

Start with the "Start here" block at the top of `Docs/Handoff.md`: the next action, what is waiting, what must not be touched.
Then the top entry of `Docs/CURRENT_STATE.md`, the state log, newest first. It is long: read its top entry and search the rest.

For substantial work, retrieve the context the task actually needs:
- design authority and unresolved questions → `Docs/GAME_VISION.md`
- high-value design rationale / semantic drift warnings → `Docs/DESIGN_RATIONALE.md`
- collaboration roles and lightweight project culture → `Docs/PROJECT_CULTURE.md`
- technical architecture/history → `Docs/Technical/UnityProjectContext.md`
- execution evidence and prior failures → `Docs/Validation.md`
- current experiential acceptance → the current milestone playtest document

The user is the Game Director and final authority on design, priority, feel, and playtest acceptance.
Implementation does not silently promote an Open/Possible/Direction choice to Locked. Preserve unresolved questions.

Keep gameplay under `Assets/_WonderGather`. Keep runtime code free of `UnityEditor` references.
Preserve asset GUIDs. Prefer Unity authoring APIs for scenes, prefabs, and navigation.

## Protect human workspace state

Uncommitted human work is protected project state.
Before broad/destructive work, inspect the working copy and do not reset, clean, normalize, overwrite, or discard human modifications without explicit permission.
Prefer an isolated worktree/clone pinned to a known commit for destructive authoring, migration, or broad validation when local work could be endangered.

## Evidence boundaries

Validate runtime changes with focused Unity tests and report actual results.
Where relevant, run regressions and inspect rendered/runtime evidence.
State what was **not** tested.
Passing tests do not imply Game Director acceptance of feel, aesthetics, or design.

Claims and checks. Each line answers failures this project has paid for (the cases: `Docs/Research/2026-11-16_HarnessGuideReview/audit_process_failures.md`; why these lines, and what would drop them: `review.md` beside it):
- **Say the rung reached:** written, compiled, tested on a rig, tested as played, seen by the Game Director, accepted by him. Not run, did not finish, failed its bar, passed but not read, and passed and read are five different results; none is rounded to another.
- **A bench or rig result is a claim about that rig.** Before it is said of the game, run it as played: the game's own defaults (the test argument `-ownAsPlayed 1`), the meadow's uneven ground, and all three miners (Small, Long and Round). Say what the rig set up, pinned or left out.
- **A pass counts only if the run shows the thing was exercised:** quote the switch or setting as read back from the running body, show that the event was forced or counted, and that the run ended by itself, not at its time limit. Repeats of one identical course are one course: count courses, not runs.
- **A check that decides a claim is trusted after it has failed on purpose:** a detector, a bar or an instrument is first shown a known-bad case it must catch and a known-good one it must pass; a kind of judge is shown them once, not at every step. Beside each bar, write the lazy way to meet it.
- **Read, then write:** a result enters a message, a document or a commit only after its run was read to the end. Never chain a commit behind a test.

## Consequence-proportional review

Ordinary reversible experiments should stay lightweight.
For silent + consequential changes—especially save migrations, stable-ID/GUID semantics, civilization-graph meaning, destructive data transforms, or future multiplayer authority—consider a fresh independent reviewer who did not author the candidate.
By the Game Director's word (October 9, 2026), every step of the current work is also reviewed by agents who did not make it (judges for movement and looks; a physicist, a sceptic and a builder for technical steps); their verdicts are not his.
A review earns its cost by a different question, a different source or a different failure mode: give each reviewer its own, do not tell it what was hoped, and have it say what it was shown. Reviewers of one model agreeing are one opinion; where a measurement, a trace or the picture itself can answer, use that. "No finding" is a good result.

Do not introduce multiplayer, DOTS, active ragdolls, or another major civilization framework without a corresponding milestone request.

## Session cost

The Game Director watches the usage of his plan (November 10, 2026: "please automatically compact the session whenever it starts to cost more usage in a more noticeable way and make sure all future agents and versions of yourself or other models remember that").
The project's Claude Code settings compact a conversation by themselves once it reaches 250,000 tokens (`autoCompactWindow` in `.claude/settings.json`); keep that setting, and say so if a session is seen past it.
Keep the state of the work in files (`Docs/Handoff.md`, `Docs/CURRENT_STATE.md`, `Docs/Outlook.md`) so that a compaction or a fresh session loses nothing, and keep the conversation small: search before fetching a whole page, read logs through a filter, and give heavy reading to cheaper subagents.
Do not count on a sub-agent having the assistant's memory: its brief carries the rules its task needs.
Quality still comes before speed and before cost.
