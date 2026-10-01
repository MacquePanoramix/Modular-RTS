# The one-worker showcase

**Source:** Luis's design clarification on September 28, 2026.
**Recorded:** September 29, 2026.
**Status:** Intended advanced prototype reference; not implemented or an
approved specification for every example below. GAME_VISION.md remains the
canonical design source; this document expands its showcase direction.
Luis's original wording is kept verbatim in
[Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md](Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md).

## Small in scale, deep in experience

Luis wants a later prototype that can be given to other people to try. Its
scope can be one worker, a landscape, and one mineral-bearing boulder or ore
deposit, while its movement, interactions and presentation are highly polished.
"Prototype" describes the small amount of the game represented. It does not
mean that every system in this showcase should remain a shallow placeholder.

The current Living Worker is accepted as a useful early prototype. Its
primitive biped, timed gathering, generic supplies, simple bundle, raw rate
sliders and fixed capacity are replaceable foundations. Acceptance does not
establish their final form or the intended ceiling of customization.

## What the player should be able to understand by watching

A worker designed through the modular unit-creation system has a pickaxe
equipped. It crosses the landscape, approaches the mineral, finds a workable
stance and uses the actual tool to strike it. The visible action connects to
the extraction of material. The worker then has to handle and transport what
it obtained, with the tool and the load both occupying the world.

The movement should be fully procedural in intent, credible, restrained and
responsive to circumstances. Weight, reach, grip, support, terrain and the
worker's capabilities should have visible consequences. Procedural does not
mean uncontrolled wobbling, and this reference does not select an active
ragdoll or require every joint or rock fragment to be a free rigid body.

Consistent with the established RTS controls, the working interpretation is
that a contextual order starts this activity and the unit performs the
strikes. This is not currently a proposal for a manual aiming/timing minigame.

## Character creation is a central promise

The intended unit editor approaches the depth and care of a video-game
character creator. Luis used Dark Souls as an example of that level of
character authorship. This comparison does not select its visual style,
interface layout, lore or exact features for Wonder Gather.

Players should shape bodies, appearance, attributes, equipment and behavior,
and see how those choices work together. A strong HUD should explain both
the chosen configuration and its consequences. A 3D preview that can show a
selected task is a proposed way to make those consequences legible; its
layout and interaction design remain open.

Luis's ambition is for this worker to showcase the character systems
together. The sequence below builds toward that integrated demonstration;
the narrow first equipment step does not redefine the eventual showcase as
only a pickaxe animation.

The worker must come from the same modular blueprint system used to design
the civilization. It should not become a special cinematic character whose
equipment and behavior cannot exist in player-made units. Comparable depth
and meaningful modular choices are intended for buildings and the broader
civilization blueprint editor, adapted to what each object does.

## Strength, equipment and load form one connected problem

**Direction:** A character's physical capabilities and the actual demands of
its equipment and cargo should change what it can do and how it moves. Tool
handling, gathering and transport should influence one another rather than
being unrelated stat multipliers.

Luis's examples express the desired depth; they are not locked thresholds
or a final exhaustive list of carrying modes:

| Example to explore | Experience it should communicate | Still open |
|---|---|---|
| A sufficiently strong worker carries a pickaxe in one hand while walking | The tool's weight is manageable for this body; another hand may be free | Strength measure, tool properties, grip and gait rules |
| A less capable worker drags the same tool | The ground supports some weight and the burden changes locomotion | When dragging is useful/possible, terrain response, speed and control |
| A worker cannot handle the tool unaided and uses a small wooden cart or another aid | Equipment can enable a task the body cannot perform alone | How an aid is acquired, attached, loaded and pulled; what happens without it |
| The mined stones must also be carried, perhaps in a bag or cart | Material has transport demands; tool use and carrying compete for capability and space | Mass, bulk, grip availability, container capacity and unloading |

Two-handed carrying, resting, repositioning and alternative grips may be
useful implementation experiments. They are proposed additions, not separate
user-locked requirements. Numerical strength bands, carrying formulas,
penalties, fatigue, breakage and economic prices all need design and testing.
One question to resolve is the distinction between transporting a heavy tool
and being able to use it: a cart can help bring it to the worksite without
automatically supplying the strength, grip or leverage needed for a strike.

## Physical contact and extraction

**Direction:** A pickaxe needs to physically reach and hit the mineral for
the mining action to succeed. A timer accompanied by a plausible gesture
does not by itself establish this relationship. Missing, being out of reach,
or interrupting before a valid strike should not silently count as mining.

**Possible:** The way the pickaxe hits could affect how much mineral is
obtained. Angle, effective force or precision are hypotheses to explore;
Luis explicitly left this relationship for experimentation. The first
contact-based test may use a fixed amount per valid strike. That temporary
rule must not settle the final yield model.

**Open:** Extraction and pickup may eventually be separate actions. Decide
whether material becomes loose pieces before loading, how tool stowage frees
hands, and how containers represent real cargo. The existing automatic
integer transfer and simple bundle linked to cargo do not settle those questions.

## What makes the advanced showcase ready for other players

This is an experiential target, not evidence that the current prototype is
ready for a public demonstration:

- The worker, tool and mineral make coherent contact; stance, grip, tool
  recovery and carrying transitions are convincing at close zoom.
- Meaningful configuration differences are visible and explainable. The
  player understands a limitation or need for assistance rather than seeing
  a unit appear broken. Invalid capabilities cannot grant invisible success.
- Tool placement, extracted material and delivered amounts remain consistent
  through interrupted orders, changed destinations and repeated work.
- A small landscape, lighting, materials, camera, sound and restrained impact
  feedback support the intended aesthetic. Their look and feel require Luis's
  direction; no final biome, species or resource fiction is chosen here.
- The selected subset of creation tools is clear enough for a new tester to
  try a configuration, observe it, change it and try again. It does not need
  every final creature, building or civilization option to demonstrate depth.
- The build has readable controls, reset/retry and feedback collection, with
  a measured performance target on the intended test hardware.

## Proposed route toward it

1. **The Equipped Worker (done, September 29–30):** blueprint-selected pickaxe
   and extraction gated by actual valid contact. Luis accepted it as the bare
   starting point. Its strike reads as a canned animation, the tool clips the
   body, and the feet still look goofy
   ([feedback](EquippedWorkerPlaytest.md#luiss-feedback--september-30-2026)).
2. **Strength and burden (proposed):** compare the same tool on workers with
   different physical capabilities; test supported carrying modes, movement
   changes and readable inability to perform an action. Because strength is
   expressed through motion, the proposal first grounds the body's gait and
   makes the strike effort-driven. See [NextMilestonePlan.md](NextMilestonePlan.md).
3. **Materials and transport aids:** give extracted material and containers
   meaningful load/capacity, then prove a bag and/or a cart with actual
   loading, hauling and unloading. Choose that order after the strength test.
4. **Polish the one-worker showcase:** improve the chosen body's motion,
   creator preview/HUD, environment, sound, feedback and onboarding together.
   Review its readiness with Luis before inviting outside playtesters.

Each step can be revised after playtesting. Personality, morale, combat,
building customization and the complete RTS remain part of the wider vision;
they are not erased by concentrating first on this worker. Which additional
character traits the public showcase should demonstrate remains open.
