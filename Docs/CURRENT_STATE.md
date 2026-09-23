# Wonder Gather — Current State

**Updated:** 2026-09-23  
**Repository head when written:** `d9b5f4d4b15769132d781a9dcf3b0ea4e7876109`  
**Current milestone:** **The Living Body**  
**Lifecycle:** **Technical Ready — Game Director playtest pending**

This file is the project's front-door signpost. Keep it short. It does not replace `GAME_VISION.md`, code, tests, or playtest judgment.

## The question we are asking now

Can a navigation-driven procedural body make an RTS unit feel grounded, expressive, and worth watching without becoming slapstick or weakening strategic readability?

## Waiting on

Luis / Game Director needs to play `TheLivingBody` and judge:

- weight versus floatiness;
- stiffness versus life;
- starts, stops, turns, and slope transitions;
- whether close observation is pleasant;
- whether the movement still reads clearly from RTS distance;
- most importantly: whether this feels like **Wonder Gather**.

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

**Human playtest first.**

After that, either:

1. return The Living Body to the workshop for another focused iteration; or
2. deliberately choose the next milestone.

No later experiment should become "next" merely because an old roadmap once listed it.

## Context router

Start here, then retrieve only what the task needs:

- What is the game trying to be? → `Docs/GAME_VISION.md`
- Why do these choices matter? → `Docs/DESIGN_RATIONALE.md`
- How do we collaborate without losing the project's character? → `Docs/PROJECT_CULTURE.md`
- How is the Unity project actually structured? → `Docs/AI/UnityProjectContext.md`
- What has really been tested, failed, and corrected? → `Docs/Validation.md`
- What should Luis inspect right now? → `Docs/LivingBodyPlaytest.md`
