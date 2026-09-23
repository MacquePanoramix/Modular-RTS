# Project working agreement

Start with `Docs/CURRENT_STATE.md`. It is the short current-state router.

For substantial work, retrieve the context the task actually needs:
- design authority and unresolved questions → `Docs/GAME_VISION.md`
- high-value design rationale / semantic drift warnings → `Docs/DESIGN_RATIONALE.md`
- collaboration roles and lightweight project culture → `Docs/PROJECT_CULTURE.md`
- technical architecture/history → `Docs/AI/UnityProjectContext.md`
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

## Consequence-proportional review

Ordinary reversible experiments should stay lightweight.
For silent + consequential changes—especially save migrations, stable-ID/GUID semantics, civilization-graph meaning, destructive data transforms, or future multiplayer authority—consider a fresh independent reviewer who did not author the candidate.

Do not introduce multiplayer, DOTS, active ragdolls, or another major civilization framework without a corresponding milestone request.
