# Wonder Gather: design continuity

> This early summary is retained for history. Use `GAME_VISION.md` as the current design source of truth.

Source: the referenced “Branch · Game Concept Development” conversation, recovered September 6, 2026. Its PDF dossier was mentioned but its contents were not available in the retrieved text. This brief preserves confirmed conversational decisions, not an invented replacement for that dossier.

## Pillars

- Slow, readable, fully 3D RTS play, inspired by the growth and expansion of Age of Empires, especially III.
- A grounded sense of wonder. Procedural movement and physical combat should feel expressive without becoming slapstick.
- Players design entire civilizations before matchmaking: their starting base, units, buildings, production relationships and custom progression.
- Meaningful aesthetic choices connect to mechanics and traits. A civilization is a network of connected blueprints.
- Distinct starting-point and in-match resource costs. Exact formulas and resources remain undecided.
- Personality, physical traits and possible individual variation support attachment to units. Future autonomy may sometimes override orders, such as fleeing under low courage.
- Main-base destruction is the current victory foundation, with potential customization around additional bases.
- Unproductive civilization designs may receive warnings rather than hard rejection; precise validation remains a design decision.

## Collaboration

The user leads creative direction, design decisions and playtesting. Codex implements code, technical structure, editor setup, debugging and verification, documenting assumptions rather than deciding unresolved creative questions silently.

## Milestone one: The Wanderer

Prove a pleasant camera and readable select/order/navigate loop for one unit. Build reusable boundaries for additional units and commands. Current placeholder capsule movement does not attempt the future procedural animation system.

Economy, civilization graphs, combat, personalities, ragdolls, multiplayer and custom progression belong to later milestones. The local command boundary is an extension point; it is not yet network synchronization, deterministic replay or an autonomy arbiter.

## Questions to answer through play

Does close zoom create interest in the individual? Is the strategic view high enough? Does the unit accelerate and turn at the right pace? Is selection unmistakable? Does the command marker provide enough feedback?
