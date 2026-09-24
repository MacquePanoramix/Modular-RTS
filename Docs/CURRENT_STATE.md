# Wonder Gather — Current State

**Updated:** 2026-09-24

**Reviewed repository baseline:** `d03e0456f0399cd2ad9d1faecfce636f54fe6c47`

**Current milestone:** **The Living Body**

**Lifecycle:** **Provisionally accepted — next milestone under discussion**

This file is the project's front-door signpost. Keep it short. It does not replace `GAME_VISION.md`, code, tests, or playtest judgment.

## The question we are asking now

How should the accepted locomotion experiment connect to the civilization
and economy systems so a player-created unit becomes worth watching at work?

## Latest Game Director feedback

On September 24, Luis reported that The Living Body feels okay for the
prototype and asked for a review of the current state and next development
step. This is provisional acceptance; it does not finalize anatomy, movement
style, aesthetic, physical simulation depth, or the eventual customization model.

## Waiting on

The next milestone is a design discussion. The recommendation is **The Living
Worker**, connecting procedural bodies to gathering, carrying and delivery
inside the faction playtest. Scope and rationale: `Docs/NextMilestonePlan.md`.
This recommendation is **Proposed**, not an approved implementation task.

## Technically established

- Unity: `6000.6.0f1`.
- Latest full PlayMode regression: **61 passed, 0 failed**.
- Windows x64 development build for The Living Body: passed.
- The procedural biped uses world-space support feet, alternating steps, predictive placement, terrain-normal adaptation, constrained leg reach, body lean, and step-linked arm motion.
- The gameplay root is still authoritative NavMesh movement; the body is a presentation layer.
- Earlier movement, economy, construction, production, civilization graph, creator, persistence/migration, and unit-performance regressions remained green.

## Explicitly not established

This milestone does **not** prove or implement:

- active ragdolls or physical balance;
- falling/stumbling simulation;
- combat;
- courage, morale, personality, or autonomous disobedience;
- arbitrary creature anatomy;
- final art/body proportions;
- RTS-scale performance with many procedural bodies;
- multiplayer.

## Design status touched by this milestone

- **Locked:** physical expression should avoid slapstick; watching units closely is part of the intended experience.
- **Direction:** use as much procedural motion as remains grounded, controllable, readable, and performant.
- **Implemented:** the current NavMesh-root procedural-biped experiment.
- **Open:** how much physical authority bodies should ultimately receive.

## Next action

Review the Living Worker proposal with Luis. If chosen, implement one
integrated work loop and return it for experiential playtesting. Keep Three
Temperaments and later milestones adjustable to what this teaches us.

No later experiment should become "next" merely because an old roadmap once
listed it. Last execution evidence remains September 22; the September 24
review inspected source, documentation and Git history without rerunning Unity.

## Context router

Start here, then retrieve only what the task needs:

- What is the game trying to be? → `Docs/GAME_VISION.md`
- Why do these choices matter? → `Docs/DESIGN_RATIONALE.md`
- How do we collaborate without losing the project's character? → `Docs/PROJECT_CULTURE.md`
- How is the Unity project actually structured? → `Docs/AI/UnityProjectContext.md`
- What has really been tested, failed, and corrected? → `Docs/Validation.md`
- What should Luis inspect right now? → `Docs/LivingBodyPlaytest.md`
- What next step is proposed, and why? → `Docs/NextMilestonePlan.md`
