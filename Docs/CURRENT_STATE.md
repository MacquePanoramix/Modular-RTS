# Wonder Gather — Current State

**Updated:** September 26, 2026

**Reviewed baseline:** b9d2902, matching GitHub at milestone start.

**Current milestone:** The Living Worker

**Lifecycle:** Technical Ready — Game Director playtest pending

## The question we are asking now

Does a player-created worker feel grounded and worth watching while it
approaches supplies, collects them, carries a load and delivers it?

## Latest Game Director decisions

Luis provisionally accepted The Living Body on September 24 and approved
the Living Worker implementation plan on September 26. This approves the
bounded experiment, not a final aesthetic, species, body or resource model.

## What is ready to try

Open TheFactionCreator and launch a faction playtest. Starting and produced
workers now have procedural bodies, work/carry poses and visible real cargo.
The resource and base each have eight reserved interaction positions with
waiting/retry when occupied. Usual selection, orders, construction and
creator performance settings remain available. The selected worker HUD
shows activity and cargo. Guide: Docs/LivingWorkerPlaytest.md.

## Technically established

- Unity 6000.6.0f1; no package changes or new save format.
- Five focused integration tests passed; full PlayMode regression: 67/67.
- Rendered probe passed; seven final captures were visually inspected.
- Windows x64 development build passed: Builds/WindowsLivingWorker.
- Resource accounting remains in Gatherer/ResourceNode/ResourceDepot.
- Body and cargo presentation follow gameplay state; navigation owns movement.
- Starting/trained workers, interrupted partial loads, depleted supplies,
  limited shared work positions and current performance bounds were tested.

Exact evidence and limits: Docs/Validation.md. Standalone interactive
playtesting and an RTS-scale performance profile were not performed.

## Still open

The collecting gesture, bundle, station, shelves and body proportions are
provisional. Construction has no new body action. Physical balance, active
ragdolls, morale, combat, arbitrary anatomy, load penalties and multiplayer
are not established by this milestone. Faction saves remain version 4.

## Next action

Luis plays the integrated loop and judges reach, carrying support, motion,
transitions and readability from close and strategic views. Refine that
feedback before choosing the next milestone. Three Temperaments remains a
candidate follow-up, not an automatic implementation commitment.

## Context router

- Design authority and open questions: Docs/GAME_VISION.md
- Design meaning: Docs/DESIGN_RATIONALE.md
- Collaboration: Docs/PROJECT_CULTURE.md
- Technical architecture: Docs/AI/UnityProjectContext.md
- Test/build evidence: Docs/Validation.md
- Current playtest: Docs/LivingWorkerPlaytest.md
- Approved scope and original rationale: Docs/NextMilestonePlan.md
