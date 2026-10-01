# Wonder Gather — Current State

**Updated:** September 30, 2026.
**Implementation baseline:** 6131598, The Equipped Worker.
**Latest implementation:** The Equipped Worker, provisionally accepted by Luis on
September 30 as the bare starting point for the one-worker showcase.
**Current step:** Strength and Burden, approved September 30 with decisions
D1–D5 as recommended. **Checkpoint A (a grounded body) is implemented and
technically validated; Luis's playtest is pending.**

## Current design question

Can strength, tool weight and load visibly and credibly determine how a worker
walks, holds, drags and swings, with motion generated from physical
relationships rather than fixed curves?

## Latest Game Director direction

Luis reviewed The Equipped Worker. In his words, the tool "looks more or less
held", but the motion "looks just like an animation still", clips the body, and
the feet still look goofy. He wants movement procedurally animated and grounded,
without goofiness. He approved drafting the strength-and-burden plan and
re-supplied his original showcase brief, now kept verbatim in
[Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md](Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md).

His target is unchanged:

- one deeply customized worker in a small landscape;
- a pickaxe that really strikes a mineral;
- strength deciding one-handed carry, dragging or needing an aid such as a cart;
- stones that must be carried;
- everything coming from the modular creator.

The full reference is [WorkerShowcaseVision.md](WorkerShowcaseVision.md).

## What exists and what it proves

- **Foundation.** Unity 6000.6.0f1 / URP; RTS controls, economy, construction,
  production, editable faction graphs and version-5 faction saves.
- **Equipment.** Blueprint None/Pickaxe choices persist. Mining requires a
  pickaxe, a reserved work position, reachable grips and a valid head strike
  against the mineral surface. Each accepted attempt extracts one supply;
  misses and interruptions extract nothing.
- **Technical evidence (September 29).** 18 focused tests and 85 full
  PlayMode tests passed; seven rendered captures were reviewed; the Windows
  build succeeded. See [Validation.md](Validation.md).
- **Known limits of the current motion**, now diagnosed from source in
  [NextMilestonePlan.md](NextMilestonePlan.md):
  - the worker walks at running speed with rapid, short steps;
  - feet do not roll and hips do not transfer weight;
  - the strike is a fixed angle curve about a point in front of the belly;
  - the windup passes the pickaxe head through the worker's own head.
- **Not yet modeled:** strength, mass, burden, dragging, bags/carts, loose
  ore, impact-dependent yield, final art and networking.

## Recommended next action

Luis playtests Checkpoint A: [GroundedBodyPlaytest.md](GroundedBodyPlaytest.md).
It covers:

- gait scaled to the body, about 2 steps/s at the new 1.8 m/s default;
- a jog above about 150% Movement;
- heel strike, roll and toe-off;
- weight transfer and counter-rotation;
- pendulum arms and a level head.

The pickaxe swing is deliberately unchanged until Checkpoint B. After his
review, tune A or begin **Checkpoint B (effort-driven swing, body volumes, no
clipping)**.

Decisions chosen September 30:

- **D1:** physics-informed kinematic body.
- **D2:** natural walk.
- **D3/D4:** Strength bands and light/standard/heavy pickaxes.
- **D5:** free hand, or lean the tool at the worksite.

D6–D9 proceed on their recommendations until Luis says otherwise.

## Housekeeping awaiting Luis

- **Uncommitted local changes to preserve.** Unity 6.6 re-saved TheGroup.unity
  and two ProjectSettings files, and `Assets/_Recovery/` holds two
  crash-recovery scenes. Luis decides whether to keep or discard them.
- **`codex/agent-bootstrap-draft`** is an unmerged, now-stale automation
  proposal awaiting review.
- **`docs/wonder-gather-continuity-2026-09-23`** was already squash-merged as
  d03e045 and can be deleted.

## Context router

- Design authority and open decisions: Docs/GAME_VISION.md
- Detailed small-scene target: Docs/WorkerShowcaseVision.md
- Original showcase brief (verbatim): Docs/Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md
- Proposed scope and decisions: Docs/NextMilestonePlan.md
- Current technical contracts: Docs/EquipmentArchitecture.md
- Latest playtest and feedback: Docs/EquippedWorkerPlaytest.md
- Design meaning: Docs/DESIGN_RATIONALE.md
- Collaboration: Docs/PROJECT_CULTURE.md
- Architecture/history: Docs/AI/UnityProjectContext.md
- Validation history: Docs/Validation.md
- Completed plans: Docs/Plans/
