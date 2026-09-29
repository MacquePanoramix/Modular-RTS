# Wonder Gather — Current State

**Updated:** September 29, 2026.
**Implementation baseline:** 1dce817, after the accepted Living Worker and showcase clarification.
**Current implementation:** The Equipped Worker.
**Lifecycle:** Implemented and technically validated; Luis's playtest acceptance is pending.
**Next discussion:** Tool handling/contact feedback, then strength and burden.

## Current design question

Does a blueprint-selected tool look held and useful, make readable physical
contact, and fit into the worker's work/carry/delivery routine?

## Latest Game Director direction

Luis approved implementing the Equipped Worker slice after accepting the Living
Worker as an early prototype. His later target remains a small landscape with
one deeply customized worker, a pickaxe and mineral/boulder, credible procedural
movement, physical work, and meaningful handling of tools and extracted material.
Character-creator depth connects to the faction blueprint and building systems.
The full reference is [WorkerShowcaseVision.md](WorkerShowcaseVision.md).

Strength-dependent one-handed handling, dragging, bags and carts are examples to
explore. Their thresholds and rules remain open. Impact quality affecting yield
also remains open. Technical validation does not approve aesthetic choices.

## What exists and what it proves

- Unity 6000.6.0f1 / URP; RTS controls, economy, construction, production,
  editable faction graphs and version-5 faction saves.
- Blueprint None/Pickaxe choices survive duplication, save/open and playtest
  return. Starting and trained workers use the same equipment application path.
- The creator opens either the existing supplies map or the new equipment map.
  Mining requires a pickaxe, reserved work position, supported reachable grips
  and a valid head strike against the actual mineral surface.
- One accepted attempt extracts one available supply. Misses, obstructions,
  cancelled attempts and unsupported tool/body scale cannot grant material.
- The tool arrests at contact and recovers. Cargo is represented beside the
  feet during mining, then held for transport with the pickaxe back-stowed.
- Eighteen focused tests, 85 full PlayMode regressions, seven reviewed rendered
  captures and a Windows x64 development build passed. Exact evidence and the
  initial failures/fixes are recorded in [Validation.md](Validation.md).

Tool geometry, two-handed poses, fixed yield, eight work positions, generic
supply counts and the short stow interpolation are provisional. No strength,
tool mass, load penalty, loose ore physics, bags/carts, arbitrary anatomy,
final art, networking or RTS-scale performance claim is established.

## Recommended next action

Playtest [The Equipped Worker](EquippedWorkerPlaytest.md), especially grip,
backswing/contact, interruption, material beside the feet and the transition
into carrying. Use Luis's feedback to choose any corrections before the next
strength-and-burden experiment. Three Temperaments remains a later possibility.

## Context router

- Design authority and open decisions: Docs/GAME_VISION.md
- Detailed small-scene target: Docs/WorkerShowcaseVision.md
- Approved scope: Docs/NextMilestonePlan.md
- Current technical contracts: Docs/EquipmentArchitecture.md
- Current playtest: Docs/EquippedWorkerPlaytest.md
- Design meaning: Docs/DESIGN_RATIONALE.md
- Collaboration: Docs/PROJECT_CULTURE.md
- Architecture/history: Docs/AI/UnityProjectContext.md
- Validation history: Docs/Validation.md
- Prior accepted prototype: Docs/LivingWorkerPlaytest.md
