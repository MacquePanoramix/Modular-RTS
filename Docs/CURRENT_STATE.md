# Wonder Gather — Current State

**Updated:** September 29, 2026.
**Reviewed baseline:** 5f61351, matching GitHub main during this review.
**Current implementation:** The Living Worker.
**Lifecycle:** Provisionally accepted by Luis on September 28.
**Next recommendation:** The Equipped Worker; scope proposed in
[NextMilestonePlan.md](NextMilestonePlan.md), not yet implemented.

## Current design question

How do we turn equipment and physical capability into visible, meaningful
actions on the path to a highly polished one-worker showcase?

## Latest Game Director direction

Luis considers The Living Worker good for this prototype. Acceptance does
not finalize its body, collecting gesture, generic supplies, numerical
controls or physical model. On September 28 he described a later prototype
that is small in scale but deep and polished enough to share with testers:
one customized worker in a landscape, a mineral/boulder target, an equipped
pickaxe, credible procedural movement, physical strikes and real handling
of tools and extracted materials.

Character creation should have character-creator-level depth, connected to
the civilization blueprints and similarly meaningful building customization.
Strength and equipment/load demands should influence how work is performed.
One-handed carrying, dragging and carts/bags are examples to explore; their
thresholds and rules are not final. Strike quality affecting yield remains
open. Full reference: [WorkerShowcaseVision.md](WorkerShowcaseVision.md).

## What exists and what it proves

- Unity 6000.6.0f1, URP, RTS controls, economy/construction/production,
  editable faction graphs and version-4 faction saves.
- Starting and trained faction workers have procedural bodies, reserved
  work positions, work/carry poses, real cargo counts and delivery.
- The September 26 validation passed 67 PlayMode tests, reviewed seven
  rendered captures and built Windows x64. Exact evidence: Validation.md.
- Gathering still awards supplies on a timer. The current work-contact flag
  means a reserved position; it does not prove a physical strike.
- Equipment, physical strength/load handling, carts and detailed body creation
  are not implemented. Capacity is still an integer supply count.

The September 29 review inspected source, configuration, documents and Git
history. It changed documentation only and did not rerun Unity validation.
The prototype is not yet the polished showcase or proof of RTS-scale performance.

## Recommended next action

Build one blueprint-selected pickaxe and a constrained procedural strike
whose actual valid contact gates extraction. Include clear grips, tool
handling during cargo transport, responsive cancellation and safe persistence.
Then test strength-dependent handling and material transport aids.

Three Temperaments remains a possible later experiment, not the automatic
next step. The full showcase is a staged target, not a commitment to implement
every character system in the next increment. Luis remains the authority on
aesthetics, feel and acceptance of each concrete slice.

## Context router

- Design authority and open decisions: Docs/GAME_VISION.md
- Detailed small-scene target: Docs/WorkerShowcaseVision.md
- Proposed implementation and repository evidence: Docs/NextMilestonePlan.md
- Design meaning: Docs/DESIGN_RATIONALE.md
- Collaboration: Docs/PROJECT_CULTURE.md
- Architecture: Docs/AI/UnityProjectContext.md
- Validation history: Docs/Validation.md
- Accepted prototype guide: Docs/LivingWorkerPlaytest.md
- Archived implementation scope: Docs/Plans/LivingWorker.md
