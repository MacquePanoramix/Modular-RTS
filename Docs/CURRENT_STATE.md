# Wonder Gather — Current State

**Updated:** October 1, 2026.

- **Latest implementation:** the grounded body (roadmap stage S0, formerly
  Strength and Burden checkpoint A), plus the arrival fix. Luis judged the
  walk "much better".
- **Current step:** S1, the worker model and art direction. It is proposed in
  [NextMilestonePlan.md](NextMilestonePlan.md) and awaits Luis's choice of art
  direction and stage order.
- **Overall plan:** [ShowcaseRoadmap.md](ShowcaseRoadmap.md).

## What the project is building now

A small-scale, high-fidelity **Worker Showcase** for outside playtesters:

- one worker, made in a video-game style character creator;
- a small landscape with one mineral boulder and a delivery point;
- physical mining and real hauling;
- models made to high quality in Blender.

The civilization/faction systems remain in the repository for the full game.
The showcase flow does not use them.

## Latest Game Director direction (October 1)

Recorded verbatim in
[Correspondence/2026-10-01_CHECKPOINT_A_FEEDBACK_AND_SHOWCASE_SCOPE.md](Correspondence/2026-10-01_CHECKPOINT_A_FEEDBACK_AND_SHOWCASE_SCOPE.md):

- **Walk.** It is much better. Arrival must not re-shuffle the feet to the
  exact spot. This is fixed: the last steps land where the body stops, at most
  one closing step follows, and the body then stands still.
- **Speed.** Movement speed is constant for now and is not a creator option.
- **Scope.**
  - One worker (maybe more later), with a character creator in place of the
    civilization creator.
  - Always a worker.
  - It sets the quality bar for the final unit customization.
- **Hauling.** Load in one hand and pickaxe in the other if strong enough, or
  a backpack, sack or cart. No back-stowed pickaxe, and leaving the pickaxe
  behind is not the default. This revises D5.
- **Models.** Much better quality, using Blender (4.4 and 4.5 are installed).
- **Process.** Upload to GitHub, with organized documentation at every step.

Still valid from September 30:

- **D1:** a physics-informed body (no active ragdoll).
- **D2:** a natural constant pace.
- **D3/D4:** strength bands and light/standard/heavy pickaxes.

## What exists and what it proves

- **Foundation.** Unity 6000.6.0f1 / URP. RTS controls, economy, construction,
  production, the faction creator and version-5 faction saves are still
  present for the full game.
- **Equipment.** Blueprint pickaxe equipment. Mining requires a valid head
  strike against the mineral, and material is conserved through delivery.
- **The grounded body.**
  - A phase-based gait scaled to hip height: about 2 steps/s and a 1.85 m
    stride at 1.8 m/s.
  - A jog above about 2.8 m/s.
  - Heel/toe feet, weight transfer, counter-rotation, pendulum arms and a
    level head.
  - Arrival finishes the stride; a settled stance tolerates small drift.
- **Shared stations.** Workers that run out of work step clear of their
  station, and claims skip positions a body still occupies. This fixed an
  intermittent deadlock that the slower pace had exposed (see the erratum in
  [Validation.md](Validation.md)).
- **Evidence.** The full PlayMode suite passes, and Windows builds succeed.
  See [Validation.md](Validation.md).
- **Not yet built:**
  - a real character model (the body is still primitive segments with
    exaggerated 2.2 m proportions);
  - the character creator;
  - an effort-driven swing (the swing is still the canned curve and clips);
  - strength, mass and hauling gear;
  - the landscape, sound and final art.

## Recommended next action

Luis chooses:

- **O1 — art direction.** Pick from rendered Blender style studies, or give
  references.
- **O4 — stage order.** Model first is recommended.

S1 then builds the worker model in Blender, drives its bones with the
procedural body and takes the body's dimensions from the model.

## Housekeeping

- **Branch.** Work is pushed to the `claude/worker-showcase` branch. Luis
  decides whether future work goes there or straight to `main`.
- **Luis's uncommitted local changes** are never included in commits:
  TheGroup.unity, two ProjectSettings files and `Assets/_Recovery/`.
- **`codex/agent-bootstrap-draft`** is an unmerged, stale automation proposal.
- **`docs/wonder-gather-continuity-2026-09-23`** is already merged as d03e045
  and can be deleted.

## Context router

- Showcase plan and open decisions: Docs/ShowcaseRoadmap.md
- Next concrete milestone: Docs/NextMilestonePlan.md
- Design authority: Docs/GAME_VISION.md
- Showcase reference: Docs/WorkerShowcaseVision.md and Docs/Correspondence/
- Latest playtest guide: Docs/GroundedBodyPlaytest.md
- Technical contracts: Docs/EquipmentArchitecture.md and Docs/AI/UnityProjectContext.md
- Design meaning and collaboration: Docs/DESIGN_RATIONALE.md and Docs/PROJECT_CULTURE.md
- Evidence: Docs/Validation.md
- Completed plans: Docs/Plans/
