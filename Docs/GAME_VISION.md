# Wonder Gather — Game Vision and Design Source of Truth

**Status:** Living document

**Last updated:** October 1, 2026

**Purpose:** Preserve the current game vision so future design and engineering work can distinguish decisions from possibilities.

**Working names:** **Wonder Gather** is the design and brand name; **WonderGatherRTS** is the Unity project name; **Modular-RTS** is the GitHub repository; **The Wanderer** is the first prototype slice. The final commercial title remains open.

This document synthesizes the complete recovered design conversation and the current prototype. It replaces short summaries as the primary design reference. When a future decision changes something here, update this file in the same commit as the affected work.

## How to read this document

- **Locked** — directly chosen by the game director. Treat it as part of the game's identity unless deliberately revisited.
- **Direction** — strongly intended, but its implementation or exact values remain open.
- **Possible** — discussed as a promising option. It is not a commitment.
- **Open** — needs design work, prototyping, or playtesting.
- **Implemented** — exists in the current prototype. Implementation does not automatically make a design choice permanent.

## The game in one paragraph

**Locked:** Wonder Gather is a slow-paced, fully 3D fantasy RTS in which players do not merely choose a faction: they design an entire civilization before the match. A civilization begins from a customized central base and unfolds as a connected network of units, buildings, resources, production relationships, and optional progression. Matches retain the gathering, growth, construction, production, and tactical command rhythm associated with *Age of Empires*, especially the more deliberate pace of *Age of Empires III*. Units and combat should be far more physically expressive and procedural, drawing inspiration from the emergence of *Totally Accurate Battle Simulator* while rejecting its slapstick tone in favor of grounded movement, readability, quiet awe, and wonder.

The intended pleasure is not only winning with a civilization. It is inventing one, bringing it into a match, and watching that handcrafted possibility slowly become alive.

## Core design pillars

### 1. Design a civilization

**Locked:** The pre-match creative act is central. The player's “deck” is an entire civilization: its starting base, available unit and building blueprints, production links, construction permissions, resource relationships, and progression structure.

**Locked:** Creation is modular but expansive enough to feel close to freeform. Modules exist to make choices flavorful, legible, balanceable, and mechanically meaningful rather than to force civilizations into narrow presets.

**Locked:** Aesthetic and mechanical decisions are connected. Players should often express gameplay through meaningful fictional or visual choices, rather than editing only abstract combat numbers.

### 2. Watch it become alive

**Locked:** The game should reward close observation. The player is meant to enjoy body language, movement, construction, small fights, and emergent moments instead of operating entirely from a numbers-focused strategic autopilot.

**Locked:** The camera must support a continuous relationship between strategic command and intimate observation: high enough to read armies, economy, and territory, and close enough to care about individual units.

### 3. Command beings with agency

**Locked:** Players mainly issue familiar, readable RTS orders such as move and attack, with only a small number of special commands where needed.

**Direction:** Units interpret orders through their bodies, capabilities, circumstances, and personalities. Autonomy can sometimes overrule the player's intention. A unit with little courage may flee despite an attack or movement order.

**Locked technical principle:** Keep player intention, issued order, unit interpretation, and performed action separate. This supports personality, morale, AI, replays, and eventual networking without changing the meaning of player input.

### 4. Physical expression without farce

**Locked:** Movement and combat should be dynamic and procedural, but grounded. The result must avoid the weightless, nonsensical, or deliberately goofy quality associated with comedy physics.

**Direction:** Favor the highest useful degree of procedural motion that remains visually convincing, controllable, readable, and performant at RTS scale.

### 5. Creative freedom through meaningful tradeoffs

**Locked:** Powerful, versatile, direct, or resilient starting options must consume more of the pre-match design budget. Players can instead reach power through longer, slower, or more fragile production chains.

**Locked:** The system should preserve strange and risky civilizations where possible. Validation should explain weaknesses and dead ends rather than automatically forbidding every unconventional design.

## The civilization as a graph

**Locked:** A civilization is a connected network whose entry point is its starting setup.

```mermaid
flowchart LR
    B[Starting base] --> I[Initial units and resources]
    B --> P[Direct production]
    I --> C[Construction capabilities]
    C --> S[Custom buildings]
    S --> U[Produced units]
    S --> R[Resource and research options]
    U --> N[Further construction and production]
    R --> N
```

A blueprint matters in a match only when the starting setup can reach it through production, construction, transformation, research, or another valid relationship. This reachability is the basis for design cost, progression, validation, and the future civilization editor.

### Starting base and starting setup

**Locked:** Every main match starts from a central initial base, in the broad tradition of *Age of Empires*. The starting setup can also contain builders, other initial units, and starting resources.

**Locked:** Players spend a limited design budget on properties of this initial possibility space. Relevant choices include:

- central-base properties such as durability and special capabilities;
- what the base can produce directly;
- which and how many units begin the match;
- which resources and how much of them begin available;
- which later blueprints are reachable through the starting network;
- redundancy, versatility, and alternative routes through the network.

Exact point values and balance formulas are **open**.

### Units

**Locked:** Unit types are customized blueprints rather than fixed faction rosters. A blueprint can combine body, appearance, equipment, attributes, personality, behavior, production cost, and capabilities.

**Locked:** Construction knowledge belongs to unit customization. Giving a unit permission to build one or more structures makes that unit more valuable and more expensive. Connecting it to the starting setup also affects the design budget because it unlocks downstream possibilities.

**Direction:** Physical and personality traits both play major roles. Strength and dexterity were named examples. Courage is the clearest established behavioral example.

**Possible traits discussed:** discipline, curiosity, loyalty, aggression, patience, protectiveness, independence, sociability, mass, reach, movement style, morale stability, carry capacity, accuracy, and formation role. This is a candidate vocabulary, not a final stat list.

### Buildings

**Locked:** Buildings are customizable and can serve production, defense, economy, research, and utility roles. Which units can build which buildings is part of civilization design.

**Direction:** Buildings and units can unlock one another through multi-stage relationships. Adding an intermediate dependency may make a powerful blueprint cheaper in starting-design value because it is less immediately accessible during the match.

### Progression

**Locked:** Players can fully design their own progression structure. A civilization may use a deep technology network, a small sequence, or no conventional technological progression at all.

**Locked:** Position within a progression chain is a balance tool. Requiring earlier units, buildings, or technologies can make a later option more efficient or less expensive than granting direct access from the starting base.

**Possible:** Hidden standardized power bands could help balance very different fictional progressions, while keeping their visible names, requirements, and presentation unique. This was proposed but not chosen.

**Open:** The exact meaning of technologies, unlock conditions, research behavior, and any limits on graph depth or cycles.

## Advanced prototype reference: one worker, deep interaction

**Direction, clarified by Luis on September 28, 2026:** Develop a later
prototype suitable for outside playtesting that is small in scene scale but
highly polished and deep in its represented systems. Its reference is one
worker in a landscape with a mineral-bearing boulder or ore deposit and an
equipped pickaxe. The current Living Worker is provisionally accepted as an
early foundation; it does not define the final movement, resource, body,
equipment, customization depth or aesthetic.

**Direction:** Unit creation should approach the depth and care of a
video-game character creator, with a strong HUD and meaningful links among
body, appearance, attributes, equipment and behavior. Dark Souls was Luis's
example of that level of authorship, not a selection of art style or exact
editor features. Comparable modular depth extends to buildings and the
civilization blueprint system. Current percentage sliders are scaffolding.

**Direction:** The worker's motion should be fully procedural in intent,
credible and restrained. The tool must physically reach and strike the
mineral. Strength, tool demands and material loads should affect handling,
locomotion and what work is possible. The ambition is coherent embodied
behavior, not instability or an implicit commitment to a specific physics
solver. Tool and cargo handling must be compatible within the same body.

**Illustrative possibilities:** A strong worker carries the pickaxe in one
hand while walking; another drags it, changing its movement; a less capable
worker needs assistance such as a small wooden tool cart. Mined material
also has carrying demands, potentially addressed through a bag or cart.
These examples express the depth of interaction. The exact strength bands,
mass/bulk model, carry modes, assistance rules and penalties remain **open**.

**Possible, not decided:** How or how well a strike lands could change
resource yield. Impact-dependent yield needs experimentation; fixed yield
per valid strike is only a possible first proof. Loose material, pickup,
loading and container handling also need design. Neither a timer nor a
simple bundle linked to cargo settles the intended physical economy.

The expanded reference, experiential criteria and proposed stages are in
[WorkerShowcaseVision.md](Design/WorkerShowcaseVision.md). Luis's original wording
is preserved in [Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md](Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md).
The Equipped Worker now exists as the bare starting point.

**Direction, clarified by Luis on October 1, 2026:**

- **Scope.** The showcase is one worker (maybe more later) with a video-game
  style character creator for that worker. In this prototype that creator
  replaces the civilization/faction creator, and it sets the quality bar for
  the final unit customization.
- **Always a worker.** The character always has basic worker abilities. In the
  full game, "worker" is not a unit type: units are customized into workers.
- **Speed.** Base movement speed stays constant and is not a creator option
  for now.
- **Hauling.** The worker gets material home in one of these ways:
  - load in one hand and pickaxe in the other, if strong enough;
  - a backpack;
  - a sack;
  - a cart.

  A back-stowed pickaxe "doesn't make sense", and leaving the pickaxe behind
  is "a bit strange".
- **Models.** They should be of much higher quality, made with Blender.
- **Audience.** Outside players will playtest it.

The staged plan is [ShowcaseRoadmap.md](ShowcaseRoadmap.md). Its sequence is a
recommendation, not a locked roadmap or approval of every detailed mechanic.

## The two-cost model

**Locked:** Wonder Gather has two related but distinct values.

### Design value

Paid before a match. It measures how much immediate possibility and resilience a civilization's starting setup provides. Direct production, extra starting units or resources, powerful base properties, versatile builders, broad access, and redundant progression routes can all raise it.

### In-match cost

Paid during play through resources, time, production capacity, prerequisites, or other match systems. It measures how difficult a specific unit, building, or effect is to create in that match.

**Locked:** These values must not be identical. A dragon at the far end of a fragile production chain can have a high in-match cost but a relatively modest design value. Producing that same dragon directly from the starting base would consume far more design value.

**Direction:** In-match costs should be calculated from customization choices rather than assigned arbitrarily to every handcrafted blueprint.

**Open:** Cost formulas, nonlinear interactions, synergy pricing, discounts for prerequisites, resilience premiums, population, production time, and how balance changes are versioned for saved civilizations.

## Resources and economy

**Direction:** The game will have a known set or framework of match resources, and unit/building customization will help determine their costs automatically.

**Possible:** Some civilization choices may unlock, create, gather, transform, or require unusual resources. Examples discussed include conventional food, wood, stone, and metal, as well as fantasy resources such as moonlight, fungi, animals, or memories.

**Open:** The base resource list; whether resources are globally shared, partially customizable, or produced from templates; storage and delivery; market or conversion systems; scarcity; map generation; population; and the exact gathering loop.

## Match structure and pacing

**Locked:** Matches follow a recognizable RTS growth arc: begin with the designed starting setup, gather or generate resources, expand the reachable civilization network, construct buildings, produce units, fight, and pursue victory.

**Locked:** The intended pace is slower and matches are expected to be long rather than brief tactical rounds. Small engagements should be readable and worth watching.

**Direction:** The playable army may need to be smaller than in mass-battle RTS games so procedural bodies, behavior, and individual readability remain practical. At the same time, maps and presentation should preserve grandeur and the feeling of a large world.

**Open:** Match length targets, maximum unit and building counts, map size, simulation tick rate, fog of war, territory, logistics, diplomacy, neutral creatures, and how much attention economy management requires.

## Control, autonomy, and personality

**Locked:** Direct orders should remain simple: primarily move, attack, and a limited set of contextual or special actions such as beginning a craft.

**Direction:** Subsequent behavior is shaped by autonomy. Units may choose routes, positioning, reactions, and moment-to-moment actions independently, and may refuse or abandon an order when personality and danger strongly demand it.

**Direction:** Disobedience must be readable. Players should be able to understand that a unit fled from fear, protected an ally, hesitated, or regrouped, rather than experiencing the AI as broken controls.

**Possible behavior model:** perception feeds stress and morale; personality modifies utility scores; a unit compares obeying, attacking, holding, protecting, regrouping, and fleeing. Courage, discipline, health, nearby allies, nearby enemies, safety, loyalty, and protectiveness could shape these scores.

**Open:** Exact AI model; trait inheritance from blueprints; autonomy limits; player feedback; group morale; experience; memory; relationships; and how much unpredictability is acceptable in competitive play.

## Individuals and attachment

**Direction:** Units should have enough personality and physical expression for the player to become attached to them.

**Possible:** A blueprint supplies the main attributes while individual instances receive small, visible variations in courage, appearance, tendencies, experience, injuries, or relationships. Important survivors could accumulate history during long matches.

**Open:** Whether individual variation ships in the core game; its magnitude; persistence between matches; naming; veterancy; permanent injury or death; and how competitive players inspect variation.

## Movement and combat

**Locked:** Units move and fight in three-dimensional space. Terrain, bodies, equipment, momentum, and nearby agents should matter visually.

**Direction:** Procedural animation and physics-influenced reactions should create variation and presence while authored constraints preserve intention and elegance.

**Possible techniques:** procedural foot placement, terrain adaptation, body lean and anticipation, active-ragdoll experiments, physically aimed attacks, weapon/body collision, balance and stumbling, pushes, momentum, formation-aware steering, and blended authored animation.

**Open:** The final simulation depth; whether damage uses hit volumes or abstract resolution; how active ragdolls blend with navigation; formation behavior; crowd collision; deterministic simulation; performance budget; and how combat remains legible from both camera extremes.

## Camera and visual space

**Locked:** The game is fully 3D, with a rotating RTS camera, terrain elevation, and the ability to zoom from strategic height to close observation of an individual fight or activity.

**Locked:** The camera embodies the “strategy ↔ observation” pillar and is a core experiential system rather than disposable infrastructure.

**Direction:** Motion should be smooth and deliberate, with acceleration/deceleration, comfortable rotation, terrain-aware limits, and close zoom that does not lose strategic orientation.

**Possible:** edge panning, middle-mouse rotation, zoom toward cursor, cinematic focus on selected units, and camera collision with uneven terrain. Current controls use keyboard panning, Q/E rotation, wheel zoom, and F to focus.

**Direction (Luis, October 1, 2026):** two camera systems the player toggles between ([correspondence](Correspondence/2026-10-01_VISUAL_SOUL_AND_TWO_CAMERAS.md)):

- **Strategy.** An "Age of Empires-like best RTS possible" camera for playing fast. The earlier slow, smoothed scrolling suited the vision but is inconvenient for strategy.
- **Explore.** A free POV camera like the Blender/Unity viewport. It can go anywhere, observe characters very closely from every angle and look up at the sky. It is "filled with wonder", "cozy to navigate and soft", and comes very near floors and buildings without passing through them. Buildings without decorated interiors stay solid.

Key bindings, control details and whether orders work in Explore are implementation defaults, listed in NextMilestonePlan.md (C1–C4). They are not Locked.

## World, tone, and art

**Locked:** The broad setting is fantasy and should allow wide creative freedom among civilizations.

**Locked:** The emotional tone is grounded, poetic, and wonder-filled: quiet awe, discovery, visual curiosity, and affection for a handcrafted living civilization. Comedy may emerge naturally, but deliberate nonsense is not the aesthetic foundation.

**Direction:** The world should feel large even if the simulated armies stay relatively intimate. A player should feel creative, clever, tactically engaged, and like a gentle commander or observer.

**Direction (Luis, October 1, 2026):** the [Visual Soul](ArtDirection/VisualSoul.md) handoff is the art direction.

- **The core.** "Even an ordinary moment should feel worth pausing for." Lighting leads the emotion, surfaces feel alive, characters have a soul, and the wonder is playful and personal.
- **The aim.** A strongly stylized, individual artistic world. Generic game-render looks were rejected.
- **References.** Six soul images, A–F, are approved with no ranking, under named influences: Studio Ghibli, Little Nightmares, Ranking of Kings and Mob Psycho 100, and Super Mario Galaxy.
- **Not chosen.** The earlier Blender style studies (soft, low-poly, grounded) were not chosen.

**Open:** Lore, species, cultures, world history, magic rules, technology range, environmental biomes, soundtrack and interface language. Within the Visual Soul direction, also open: final proportions, architecture, world setting (a literal space setting is still open), the intensity of the Little Nightmares influence, the final cast and costumes, and the rendering technique, which needs an in-engine test.

## Victory and defeat

**Locked:** Destroying the opponent's main initial base is the foundational victory condition.

**Direction:** Base customization may let a civilization establish additional civilization hearts or main bases. Spending design value on this resilience could require opponents to destroy every qualifying heart before victory.

**Possible alternate modes or standardized victories:** capture a central structure, control wonders, complete a monumental project, achieve a cultural or magical transformation, force surrender through territory or morale, survive a world event, or fulfill a faction-flavored aspiration.

**Open:** Whether any alternate victory enters the primary competitive mode, surrender rules, defeat after loss of production capability, and how extra civilization hearts are limited and communicated.

## Modes and multiplayer

**Locked:** The long-term game is multiplayer-focused.

**Locked:** The principal competitive format is **Open Workshop**: players create civilizations beforehand and bring them into matchmaking under the design budget and applicable validation rules.

**Direction:** The system should also support an appropriate single-player experience.

**Possible additional formats:** skirmish versus AI, campaign, co-op, a drafted workshop using a shared module pool, a curated or seasonal league, and mirrored construction where players receive the same components but build different civilizations.

**Open:** Player count, team modes, campaign structure, matchmaking rules, ranked legality, anti-cheat, mod/workshop sharing, version compatibility, deterministic or authoritative networking, and which mode is developed first after the core simulation is proven.

## Civilization validation

**Locked:** The editor should analyze whether a civilization can theoretically progress from its starting setup.

**Locked:** Warnings are preferred over hard blocking in order to protect creative freedom, though competitive queues can still require explicit legality rules.

**Possible validation language:**

- **Valid:** at least one functional route exists from the starting state.
- **Valid but vulnerable:** progression exists, but foreseeable losses or dependencies can permanently strand the civilization.
- **Incomplete:** no viable route exists to acquire a required resource, construct, produce, or advance.

Warnings should explain causes precisely, such as: “No reachable unit can gather stone, but three reachable structures require stone.”

**Open:** Which failures merely warn, which make a civilization ineligible for matchmaking, whether intentional challenge civilizations are supported, and how exhaustive the graph/economy simulation must be.

## Technical direction

**Locked for the current project:** Unity 6.6 with the Universal Render Pipeline. The project is organized under `Assets/_WonderGather`, separate from third-party content.

**Locked architectural principles:** modular components; data separate from runtime behavior; explicit ownership; command objects between input and action; editor automation for repetitive setup; automated tests for deterministic rules and important integration; Git/GitHub as the shared source of truth.

**Direction:** ScriptableObject-backed blueprints are a strong fit for unit, building, resource, personality, body, technology, and civilization definitions. A visual graph editor may eventually present their relationships, but the data model should be proven with simpler tools first.

**Direction:** Utility scoring is a strong candidate for personality and autonomous decisions after basic states and behavior loops are understood.

**Prototype foundations and possible later depth:** save/load formats for civilizations, graph validation, cost-calculation tools, procedural locomotion, active-ragdoll experiments, formation and steering systems, multiplayer authority and replication, performance profiling, content versioning, and creator-facing debugging explanations.

**Deferred intentionally:** DOTS/ECS, networking packages, final art assets, deep procedural combat, and a polished civilization graph editor are not foundations for the earliest prototypes. Adopt them only when measurements and milestone needs justify them.

## Current production state

**Current implementation:** the grounded body (Strength and Burden checkpoint A,
now roadmap stage S0).

- Luis judged it "much better" on October 1. The arrival shuffle Luis reported is
  fixed.
- The worker walks at a constant natural 1.8 m/s.

**Next:** S1, the worker model and art direction, proposed in
`Docs/NextMilestonePlan.md` and awaiting Luis's choice of art direction. The
showcase roadmap is `Docs/ShowcaseRoadmap.md`.
Historical entries
below preserve earlier scope and feedback; later entries supersede earlier
limits and proposed sequencing.

### Prototype 1.1 — The Wanderer

**Implemented:**

- a Unity 6.6 URP project and GitHub repository;
- a fully 3D test scene with one selectable Wanderer;
- smooth bounded camera pan, rotation, zoom, and selected-unit focus;
- mouse selection, deselection, order feedback, and destination marker;
- a command boundary from player input to movement;
- NavMesh navigation, obstacle routing, and rejection of unreachable destinations;
- a reusable unit prefab and separated runtime/editor/test assemblies;
- three passing PlayMode tests and a successful Windows development build.

**Awaiting design acceptance:** camera feel, zoom range, pan/rotation speed, unit acceleration and turning, selection clarity, and destination feedback after hands-on playtesting.

**First hands-on feedback:** the prototype's features and overall camera smoothing felt good. Mouse-wheel sensitivity increased from 0.0015 to 0.002. The retest initially accepted this as good enough; the user then chose and saved 0.005 as the preferred zoom sensitivity. The Wanderer is provisionally accepted as the foundation for group control.

### Prototype 1.2 — The Group

**Accepted in first hands-on playtest:** eight selectable units in TheGroup, click and Shift-click selection, drag-box and additive box selection, selection rings/count, group-center camera focus, and group orders with separate arrival slots. NavMesh local avoidance handles nearby agents. The user requested that unreachable clicks move toward the next best available place. Movement now resolves reachable alternatives and adjusts nearby slots while preserving separation. Invalid numerical orders and groups with no available space still preserve existing destinations. See `Docs/Playtests/GroupPlaytest.md`.

### The Gatherer — first step toward The Little Settlement

**Implemented for playtesting:** one placeholder supply resource, one drop-off point, eight workers, five-unit carry capacity, gathering and repeated delivery, finite resource accounting, stored-resource HUD, and cancellation/resumption through orders. The user approved this production step after accepting The Group. Exact values and resource fiction remain prototype choices; this does not lock the final resource list.

### Prototype 1.3 — The Little Settlement

**Planned direction:** one starting base, one builder, one resource, one constructible building, and one production path. This is the smallest slice that can test civilization reachability and the two-cost model.

### Later proof sequence

The list below records the earlier broad proof sequence. The September 28
clarification now favors equipment/contact, then strength/load and transport
experiments toward the one-worker showcase before Three Temperaments.
The current proposal is detailed in NextMilestonePlan.md and remains adjustable;
the earlier list must not be treated as an automatic implementation queue:

1. **Civilization data and graph:** data-driven base, unit, and building definitions; production/build links; reachability validation; two-cost foundation; first editor tooling.
2. **The Living Body:** a provisional articulated biped with procedural locomotion, planted feet and terrain adaptation, establishing a grounded movement reference before personality and combat.
3. **Three Temperaments:** one basic blueprint expressed through courage and discipline differences, proving readable orders, autonomy, and morale.
4. **Small physical battle:** a few units with readable attacks and reactions, measured against a performance budget and player comprehension.
5. **Vertical slice:** design one civilization before the match, begin from its starting setup, gather and progress, fight, and reach a main-base victory.
6. **Multiplayer prototype:** bring saved civilizations into Open Workshop matchmaking after the local vertical slice proves its rules.

This later sequence is a direction synthesized from the conversation, not locked milestone scope. The inaccessible earlier dossier reportedly contained a seven-stage roadmap, but its exact stages and exit criteria cannot be recovered and are not reconstructed here.

## Collaboration agreement

**Locked:** The user is the game director, lead designer, creative director, and primary judge of feel. The user defines the desired experience, makes creative choices, and playtests visual and emergent behavior.

**Locked:** Codex acts as technical co-developer and systems-design partner: architecture, C# implementation, editor tools, tests, debugging, documentation, Git workflow, performance investigation, and later networking code where possible.

The working loop is: describe an experience → design the rule together → implement the smallest coherent slice → playtest → translate observations into technical adjustments → repeat.

## Explicit open-decision register

These questions must remain visible rather than being silently answered by implementation:

- What are the standard match resources, and how can civilizations alter them?
- What exact attributes and aesthetic modules can players edit?
- How do strength, tool mass/geometry, hand availability and cargo determine handling, movement and the need for bags/carts?
- Which rendering technique, proportions, architecture and setting realize the Visual Soul direction (S1c in-engine test)?
- Does strike quality change mineral yield, and how do extraction, pickup and loading relate?
- How are design value and in-match cost calculated and balanced?
- What graph structures, dependencies, cycles, and transformations are legal?
- How much individual variation do instances receive?
- What is the practical unit count and target match duration?
- How physically simulated can combat become without losing control, readability, or performance?
- Where exactly is the boundary between the posed body and the physical one? (October 6, Luis: a mixture. Very stable, yet able to fall in an extreme situation. The proposed boundary is the ladder of balance in Design/ThePhysicalBody.md; the bench will show whether it holds.)
- Which key gives the interaction click, and which options does each thing offer?
- How fast does tiredness come and go, is there a longer-term tiredness, and what is the player shown of it?
- Does a rock visibly wear away as it is mined?
- What is the scale of strength, and how does a body's appearance change with it?
- What is a worker's natural pace relative to RTS movement speed, and does body or load set top speed?
- How does the game explain hesitation, refusal, fear, protection, and autonomous choices?
- Which single-player and secondary multiplayer modes support the central vision?
- Are victory conditions beyond civilization-heart destruction part of the main game?
- What visual style and lore best express grounded fantasy wonder?
- Which rules are warnings, and which are required for competitive matchmaking?

## Guardrails for future work

- Do not turn a tentative example into a permanent rule without recording the decision here.
- Do not reduce civilization creation to cosmetic faction skins or a conventional fixed tech tree.
- Do not let procedural physics become comedy at the expense of grounded wonder.
- Do not make autonomous behavior opaque; agency must be legible to the commander.
- Do not optimize only for top-down efficiency; close observation is part of the game.
- Develop creator interaction and previews alongside demonstrated body/equipment consequences; a limited early UI must not become a ceiling on character or building customization. Final editor polish should follow proven choices and Luis's aesthetic direction.
- A small public prototype may need deep, highly polished interactions. Do not equate few units with permanently shallow systems.
- Do not add large-scale technology because it sounds future-proof. Add it when a measured prototype needs it.

## Decision log

New ideas enter as **Possible**. Only an explicit design decision promotes them to **Locked**. A prototype can implement a hypothesis without making it permanent.

| Date | Topic | Status | Decision or hypothesis | What could change it |
|---|---|---|---|---|
| 2026-09-06 | Engine foundation | Locked for current project | Unity 6.6 and URP; migrated early from Unity 6.3 LTS | A demonstrated engine-level blocker or a deliberate future LTS transition |
| 2026-09-06 | First slice | Provisionally accepted | The Wanderer proves camera, selection, commands, and one-unit navigation | Hands-on playtest feedback |
| 2026-09-06 | Repository visibility | Production choice | Publish the source at `MacquePanoramix/Modular-RTS` | A later explicit decision to make it private |
| 2026-09-07 | Prototype camera tuning | Provisionally accepted | Preserve smooth wheel zoom and increase sensitivity from 0.0015 to 0.0020 | Hands-on retest feedback |
| 2026-09-21 | Proof sequence and physical expression | Approved prototype direction | Implement a small Living Body locomotion experiment before Three Temperaments; provisional biped and comparison scene | Movement playtest and the user's aesthetic direction |
| 2026-09-28 | Living Worker feedback | Provisionally accepted | The current implementation is good as an early prototype; its forms and rules remain replaceable | Further playtesting and the final vision |
| 2026-09-28 | Advanced one-worker showcase | Direction with open mechanics | Small landscape, customized worker, pickaxe/mineral contact, deep creation and physical handling relationships; detailed examples/yield rules remain open | Luis's design refinement and staged experiments |
| 2026-09-29 | Next implementation review | Approved and implemented | The Equipped Worker before strength/load and transport experiments; Three Temperaments remains later | Luis's feedback on scope and results |
| 2026-09-30 | Equipped Worker feedback | Provisionally accepted as a foundation | Blueprint tool choice, contact gate and work loop are the base to build on. The canned-looking strike, body clipping and goofy feet are not accepted | Improved motion in a later playtest |
| 2026-09-30 | Motion direction | Reaffirmed by Luis | Movement should be procedurally animated and grounded, without goofiness | Luis's playtest judgment |
| 2026-09-30 | Next implementation | Approved by Luis | Strength and Burden in three checkpoints: grounded body, effort-driven strike, strength-resolved handling | Checkpoint playtests |
| 2026-09-30 | Body technique (D1) | Chosen for this milestone | Physics-informed kinematic body; no active ragdoll | Checkpoint playtests; a later explicit physics experiment |
| 2026-09-30 | Worker pace (D2) | Chosen for this milestone | Default worker walks at a natural pace for its body; higher Movement % becomes a jog. Economy timing slows accordingly | Playtest of pace and economy feel |
| 2026-09-30 | Strength and tool weight (D3/D4) | Chosen for this milestone | One Strength value with named bands; authored light/standard/heavy pickaxes whose shape matches their weight | Checkpoint C playtest; a later body-build editor |
| 2026-09-30 | Tool while hauling (D5) | Superseded October 1 | Carry the tool in a free hand when strong enough; otherwise lean it at the worksite and collect it on return | Replaced by the October 1 hauling direction |
| 2026-10-01 | Grounded body (checkpoint A) | Provisionally accepted | The walk is "much better". Arrival must not re-shuffle the feet to the exact spot (fixed the same day) | Further playtesting on the real model |
| 2026-10-01 | Showcase scope | Direction, clarified by Luis | One worker (maybe more later) and a video-game style character creator for it, in place of the faction creator in this prototype; always a worker; high quality for outside playtesters | Luis's later refinement |
| 2026-10-01 | Movement speed | Direction, for now | Constant natural pace, not a creator option | Luis's ongoing thinking about speed |
| 2026-10-01 | Hauling (revises D5) | Direction, clarified by Luis | Load in one hand and pickaxe in the other if strong enough, or a backpack, sack or cart. No back-stow; leaving the pickaxe behind is not the default | S4 playtest |
| 2026-10-01 | Asset quality | Direction | Showcase models made to a much higher quality, using Blender | The chosen art direction (open) |
| 2026-10-01 | Art direction process (O1) | Chosen | Decide the showcase style from rendered Blender style studies | Luis's choice among the studies |
| 2026-10-01 | Stage order (O4) | Chosen | The worker model (S1) comes before the character creator (S2) | Pipeline findings during S1 |
| 2026-10-01 | Strength source (O2) | Chosen for the showcase | Body build sets a base strength; a separate training choice adjusts it within limits | S2/S4 playtests |
| 2026-10-01 | Blender style studies | Not chosen | The soft, low-poly and grounded studies "were not bad" but did not match what Luis imagines | — |
| 2026-10-01 | Visual Soul (art direction) | Direction | The Visual Soul handoff and its approved images A–F (no ranking). Proportions, architecture, setting and rendering technique remain open | In-engine look test (S1b/S1c) and Luis's judgment of real captures |
| 2026-10-01 | Two camera systems | Direction | A fast Strategy camera and a free, cozy Explore camera, toggled; Explore approaches surfaces closely without passing through floors or buildings | S1a playtest |
| 2026-10-01 | Ordinary Place, first pass | Provisionally accepted as the direction | "The direction is going really well into the Visual Soul… but I would be lying to say it's already" there | Further passes and Luis's judgment |
| 2026-10-01 | Rendering approach (V1) | Direction (working base) | Candidate E (painted light + paint filter + ink) is Luis's favourite; hand-painted textures are the next exploration | Comparison after the hand-painted pass |
| 2026-10-01 | Camera defaults (C1–C4) | Accepted | `V` toggle, orders in Explore, Unity-style fly plus Blender-style orbit, buildings solid without interiors | Later playtests |
| 2026-10-01 | Project organization | Working agreement, reaffirmed | Everything, including documentation, goes to GitHub, in a very organized structure (see Docs/README.md) | — |
| 2026-10-01 | S1 sequence | Revised from Luis's handoff | Cameras first (S1a), then the Ordinary Place (house, grassland, path, worker; day and night, S1b), rendering candidates compared in engine (S1c), then the worker model (S1d) | Playtest of each checkpoint |
| 2026-10-02 | Hand-painted pass | Judged by Luis | "Already quite beautiful… I can't exactly call it perfect yet"; explore further rather than stop | S1e |
| 2026-10-02 | The essence (S1e) | Direction, asked by Luis | Seek the breathtaking, out-of-this-world emotion and the stylistic essence, not the surface. Every frame a painting driven by light, while the RTS still runs well. A style above and beyond the Visual Soul, unique to Wonder Gather. A long checkpoint with research is welcome | Luis's playtests of each S1e iteration |
| 2026-10-02 | S1e first iteration (the beyond, look F) | Not adopted | Luis: "honestly I think I like the before better". His favourite frame: the lit house at dusk from low on the path, in look E. The hand-painted pass stays the base; the experiment is archived on `claude/essence-exploration` | — |
| 2026-10-02 | Hand-painted pass merged | Approved by Luis | The hand-painted Ordinary Place in look E is in `main` (7f7fcc0) | — |
| 2026-10-02 | Fireflies in the Ordinary Place | Approved by Luis | "Can be for this scene I actually do like that"; on by default, key 7 | — |
| 2026-10-02 | Dusk details | Direction, in small steps | Deepen the dusk mood in small switchable steps, then move to the worker model. The hearth's flicker (8) and window glow (9) were built off until judged (see the next rows) | Luis's judgment in the build |
| 2026-10-02 | Dusk details on by default | Approved by Luis | "Honestly I loved it"; fireflies, the hearth and window glow all start on (keys 7–9 still switch them) | — |
| 2026-10-02 | Fewer fireflies, still seen far out | Asked by Luis | 600 instead of 1,100, blinking more sparsely ("from gentle wonder inducing to a bit overwhelming" near the ground); a tiny painted dot keeps them visible when very zoomed out | Luis's check in the build |
| 2026-10-02 | Lamplight close up | Asked by Luis | The near view now matches the far look Luis liked: lamplight reaches the blade bodies and the soil | Luis's check in the build |
| 2026-10-02 | The Visual Soul's principles | Direction, clarified by Luis | The base art style for every being and thing in the world, across all kinds of units; details like the scarf were only examples ([VisualSoul.md](ArtDirection/VisualSoul.md#the-language-for-every-being-and-thing)) | — |
| 2026-10-02 | Worker redesign (S1d) | Started, asked by Luis | "Let's finally move to the player remodel/redesign", from the Visual Soul and the principles for people, "with heart and soul"; concept frames first | Luis's choice of a concept direction |
| 2026-10-02 | Worker concepts (S1d) | Proposed, awaiting Luis's choice | Three directions built on the Visual Soul's language: Round, Long and Small. Each has a painted few-mark face, its own gesture and a drawn outline, with beings kept clear of the paint filter ([WorkerConcepts.md](ArtDirection/WorkerConcepts.md)) | Luis's choice of a direction or mix |
| 2026-10-02 | Worker concepts judged | Luis's ranking | Small, then Long, then Round; loved every face and every character's idea; asked for much higher model quality, miner's hints, and the stylization kept | Luis's judgment of the miners |
| 2026-10-02 | A choice of characters | Possible ("maybe"), then chosen on October 3 (see below) | The final prototype might let the player choose between the three miners | Luis's decision |
| 2026-10-02 | Customization | Direction, from Luis | The final game lets players customize everything, units included, down to appearance, like detailed character creators. Beings are built from modules (body, face, hair, garments, accessories) that fit any body ([TheMiners.md](ArtDirection/TheMiners.md#built-from-modules-towards-a-character-creator)) | S2 (the creator) |
| 2026-10-02 | The miners (S1d, second pass) | Proposed, awaiting Luis's judgment | Small, Long and Round remade at higher quality, with miner's hints ([TheMiners.md](ArtDirection/TheMiners.md)) | Luis's judgment |
| 2026-10-03 | The miners | Approved in direction by Luis | "Almost perfect now! Honestly they are already adorable!"; polish the necks, feet and Round's hands, then rig them and make them light enough for the RTS ([TheMiners.md](ArtDirection/TheMiners.md)) | Luis's playtest of the rigged miners |
| 2026-10-03 | A choice of three miners | Chosen by Luis for this prototype | Small, Long and Round are offered as choices | — |
| 2026-10-03 | Rigging the miners | Built, awaiting Luis's playtest | One skeleton on the procedural body's joints. The body's proportions come from each model, and a rig adapter turns the bones to the solved joints. Three levels of detail and one atlas each. Hands are freed for work: Small's lantern goes to the belt, Long's pickaxe to the back ([MinersPlaytest.md](Playtests/MinersPlaytest.md)) | Luis's playtest |
| 2026-10-03 | Walking pace | Superseded the same day | One pace for all three miners (1.3 m/s) | — |
| 2026-10-03 | Natural walks | Direction from Luis | "Their bodies should walk in a way that feels natural to their appearance." Each walks at its own comfortable pace (Froude about 0.21–0.27) with its own bounce, sway and arm swing ([CharacterPractices.md](ArtDirection/CharacterPractices.md#3-walks-natural-to-each-body)) | Luis's playtest |
| 2026-10-03 | Proactive polish | Working agreement, from Luis | Every character passes a close audit in the engine before Luis sees it; the practices are recorded in [CharacterPractices.md](ArtDirection/CharacterPractices.md) | — |
| 2026-10-03 | Physical objects | Direction from Luis | Physicality and the reality of all movement are among the game's main focuses: anything that reads as an object behaves as a proper object that responds to its environment (held, hung, swinging with gravity), never floating | — |
| 2026-10-06 | The miners mine: a pickaxe for each body, swung over the shoulder | Built, awaiting Luis's playtest (`K` in the Ordinary Place) | Each miner's pickaxe is made for its arms and hands; the tool's solve reads the body, its shape included. The first body's swing put the tool through chest and head, so bodies with a shape swing over the right shoulder; the first body's swing and the rule of the strike are unchanged ([TheMiners.md](ArtDirection/TheMiners.md#a-pickaxe-for-each-and-the-first-swings-october-6)) | Luis |
| 2026-10-08 | No fake animations | Direction from Luis, after playing S3 | "I don't want any fake animations." "I don't want the animations to be just built-in animations. I want them to be more general animations that the character can adapt to any situation." "I don't want it to be necessarily real-life realistic, but I want it to feel like fantasy media realistic": a person watching must feel "Oh, yeah, that movement makes sense". It sharpens "every action is physical" of October 6: a movement done by the body that still reads as played back is also wrong ([message](Correspondence/2026-10-08_THE_PLAYTEST_NO_FAKE_ANIMATIONS.md)) | Luis's play of what the round builds |
| 2026-10-08 | The playtest round: Luis's ten notes on S3 | All ten looked into; for Luis to see | Before S4, by Luis's word. Done: the fireflies (each with a home in the meadow; fewer; fainter from far); the cabin's light (no jump at any distance); the stutter after each step (the hips fell 5 to 13 cm in one frame, now 2 to 3 mm); Long's rest that never ended. Made again in the second part: turning round and being sent back while walking; going down for the pickaxe (one movement); the clothes and the free arms (they hang by their own weight). The lantern, once held: carried as a lantern is. Getting up after a fall: by the body's own strength as far as its knees ([the round's page](Reviews/2026-10-08_ThePlaytestRound.md)) | Luis's play |
| 2026-10-09 | Getting up after a fall, by the body's own strength | Implemented in part; Luis to see | The let-go body turns itself over and draws its knees under it, its joints held towards poses with the strength each has; the posed body takes over only then, in half a second, and stands up. The poses were found by a search on the three miners together (`GetUpSearch`). **Not the body's own:** the half second from its knees onto its feet, and the standing up. A body that gives up three times running gets up the old way ([the round's page](Reviews/2026-10-08_ThePlaytestRound.md#l7-getting-up-after-a-fall)) | Luis's eye |
| 2026-10-09 | How long a fallen miner lies | Implemented on Luis's word; the settings are mine, for Luis to change | Luis: "research and look for what would be a reasonable time for the miners to lay down... dependent on factors such as how strong was the force that pushed it down, it's current stamina and anything else relevant". It lay 1.2 s whatever had happened. It lies now at least 0.8 s; up to 4.5 s more by how hard it came down; up to 2 s more by how hard its head struck; up to 3.5 s more by how spent its legs and back are; all of it less for a stronger body; never more than 10 s. Measured: 0.8 to 1.9 s after a light push, 1.9 to 6.7 s after a hard one. Set between the one to three seconds of games and the ten of old people who fall and get up ([the round of October 9](Reviews/2026-10-09_TheBodysOwn_Round.md#how-long-a-fallen-miner-lies); [the research](Research/2026-10-09_AliveAndTheBodysOwn.md#1-how-long-a-fallen-body-lies)). Whether a fall should cost anything more is still Luis's | Luis's eye |
| 2026-10-09 | The panel's "Push it over" | Implemented; Luis to say if it stays | A button on the panel of the Ordinary Place shoves the chosen miner over, the way the view looks, so that a fall and a getting up can be seen (by itself a miner falls only at work far too heavy for it). It is for looking; nothing in the game pushes a miner | Luis |
| 2026-10-09 | S4, carrying and equipment | Proposed; not approved, not begun | Ten steps, from a stone in the hand to "mine this and bring it home", through the heap by the cabin, the pack, the dragged sack, the cart and the strap. Six questions for Luis come first: posed or the body's own; Long's pickaxe on its back; where home is; what a load is; the order of the gear; how gear is chosen before the creator ([the proposal](Plans/S4_CarryingAndEquipment_Proposal.md)) | Luis's answers |
| 2026-10-09 | The body's own, not posed; and stable | Direction from Luis (it answers what was open: which movements are to be the body's own) | "I want stability in the body but with it still being the body's own. I don't want it to be posed like that." *Posed* was my word for the walk, the turn, going down for a tool and standing up: worked out each frame from the ground, the body's build and what its joints can give, and then put there, not pushed. *The body's own* is the fall and the turning over: parts pushed by their joints' strength. "Very stable, never naturally wobbly" (October 6) stands with it ([message](Correspondence/2026-10-09_THE_BODYS_OWN_NOT_POSED.md)). Nothing is built on it yet | A plan Luis approves |
| 2026-10-09 | Natural and alive, not robotic; studied before it is built | Direction from Luis | "I really do want movement to feel natural and alive rather than robotic. That's why it can't be posed and we must do media studies and research of what kind of movements feel natural for certain actions." A first study is written: eight things a living body does, and where the miners are in each ([the research](Research/2026-10-09_AliveAndTheBodysOwn.md#2-what-makes-a-movement-read-as-alive-and-not-robotic)). Before a movement is built, how real bodies do it and how films and games make it read are looked up and written down | — |
| 2026-10-09 | Judges for every step | Working agreement, from Luis | "good job with the agent reviews, please continue with that for further steps always". Every step is given to three judges who did not make it, before Luis sees it. They are now also asked whether it is alive ([`alive.md`](../Art/Review/judges/alive.md)). Their verdict is not Luis's ([the judging of movement](ArtDirection/MotionJudging.md)) | — |
| 2026-10-09 | Body technique: moved by its own joints (replaces D1 wholly) | Decided by Luis | Asked whether "no active ragdoll" (D1, September 30) is replaced by the direction of October 9, Luis: "Yes." On October 6 it had been replaced in part (very stable, and able to fall). Now every movement is to be the body's own ([the answers](Correspondence/2026-10-09_THE_BODYS_OWN_ANSWERS.md)) | — |
| 2026-10-09 | The order of stages: the body's own before S4 | Decided by Luis | "Yeah I also think so, we will probably end up doing a big rework I imagine for it." The order is S3, S3b (the body's own; the name is mine), S4, S2, S5 ([the plan](NextMilestonePlan.md#the-order-of-stages-october-9)) | — |
| 2026-10-09 | What "stable" is held to | Accepted by Luis as the measures; not Locked | "Yeah they looked pretty good for me." Standing at ease it breathes and shifts and its head wanders no more than a centimetre or two, with no tremble; it leans, steps and falls at the same pulls as today's body; it does not fall in ten minutes of walking the place with its tool, at half, ordinary and double strength; nor at its work at the ten boulders, except as today ([the table](NextMilestonePlan.md#how-it-is-made-stable)) | Luis's eye on the build |
| 2026-10-09 | Breathing | Direction from Luis; how it is shown is open | "Breathing is good to have while following the visual aesthetic of the game for it too." Drawn on the chest "if you think it will look nice/apparent"; "if not then we can think of other ways to represent it". To be tried, looked at, and shown to Luis as looks beside each other | Luis's eye |
| 2026-10-09 | S3b, the body's own: the plan | Approved by Luis (the four answers above); step 0 begun | Nothing pushes the body from nowhere; it keeps its feet by its ankles, its hips and a step; the poses are found by search on the three miners; "stable" is said in numbers, measured first on the body as it is; what Luis has stays beside it to switch to. Steps: the ground for it (ankles, measures, cost); standing with life in it; from the knees onto the feet; a step; walking; down to the ground and up; the work; then S4; and, at any point, the fall again (hands that reach for the ground) ([the plan](NextMilestonePlan.md)) | Luis sees each step |
| 2026-10-09 | S3b step 0: what the body's own body is made of | A bench and figures; nothing in the game changed. The decision is mine, reviewed; for Luis to know | An articulation (one jointed body solved as one) of thirteen parts, with ankles; its legs' torques worked out each step from the push it wants from the ground; its joints held softly towards their pose; nothing slowing its parts. Standing so, all three miners held their heads within 0.1 to 0.35 mm, twenty-five of twenty-five (at 200 steps a second in step 0; at the game's own 50 in step 1's first part, though less steadily). It does not lean against a pull yet: that is step 1 ([figures](Design/TheBodysOwn.md#step-0-the-ground-for-it-october-9)) | Step 1 |
| 2026-10-09 | How often the physics steps where a body's own is | Open: a trade, to be chosen when the body is in the game | Step 0 had all three miners standing only at 200 steps a second; its first writing said "four to ten times as fast", and three reviewers took that out. With each joint's spring set by the rule of the engine's makers, all three stand at the game's own 50; but a fourth reviewer showed "fifty will do" was too much: given a light shove (0.2 m/s), 3, 20 and 21 of 24 stay standing at 50, 19, 24 and 19 at 100, all at 200, and at 50 the body sags 47 mm into a poorer stance first. A body costs 0.05, 0.11 and 0.22 ms of each fiftieth of a second at those rates; above 50, what is built on the physics before is stepped finer too ([figures](Design/TheBodysOwn.md#step-1-first-part-how-often-the-physics-steps-october-9)) | Step 1, in the game |
| 2026-10-10 | S3b step 1: a miner standing by its own joints, with life in it | Built; off unless switched on; Luis to see | The panel of the Ordinary Place has a switch, "Stands by its own joints". Stable by its tests and by six judges of six; alive, by the same judges, only in its head. [Design](Design/TheBodysOwn.md#step-1-the-rest-of-it-the-body-in-the-game-and-the-life-in-it-october-9-and-10); [judges](Reviews/2026-10-10_StandingWithLife_Judges.md) |
| 2026-10-10 | How breath is shown | Direction from Luis, October 10 (B6): the shoulders rising and breath seen in the dusk air are tried next, beside the three looks. Which look: not chosen | Drawn on the chest it cannot be seen, even at four times life. Three looks are on the panel; others are named and not built |
| 2026-10-10 | What "its head wanders no more than a centimetre or two" is to mean | Decided by Luis, October 10 (B5): it holds between shifts of weight | Met between shifts of weight; not across them (2 to 4 cm) |
| 2026-10-10 | Whether a step comes before the knees onto the feet | Decided by Luis, October 10 (B7): the step first | Leaning as today's body does, a real shift of weight and standing up to a push all wait on the step |
| 2026-10-08 | Long's two pickaxes | Open: Luis's choice | Long was modelled with a pickaxe slung on its back (October 3); since S3 it also takes "its own pickaxe" in its hands, which is a second one. Two of three judges of the pick-up noticed. Whether the one on its back is the one it works with (taken off by its hands: carrying and equipment, S4), or is hidden while one is in hand, or stays, is Luis's. Nothing was changed ([the round's page](Reviews/2026-10-08_ThePlaytestRound.md#what-the-judges-of-the-pick-up-said-first-round)) | Luis |
| 2026-10-08 | How a pickaxe is carried in one hand | Implemented; Luis to see | It hangs from the hand, its head at the hand and its handle down behind (it was held level, since S3). Changed after three judges called the level carry "a placed carry pose, not a weight in a hand". The level carry is not kept as a switch: say so if it is wanted back | Luis's play |
| 2026-10-08 | A walk gathers its pace over its first steps; a walker sent back stops before it turns | Implemented; Luis to feel | Two changes to how a miner answers an order, both from making the body the cause of its movement. Its place goes no faster than its legs carry it: about a second to its whole pace, where it took half of one (its steady pace is what it was). Sent back while walking, it stops, turns as from standing and walks: about two seconds, where it turned as it slid. Both can be loosened if they feel slow in play ([the round's page](Reviews/2026-10-08_ThePlaytestRound.md#sent-back-while-it-walks-and-the-first-step-of-a-walk-second-part)) | Luis's play |
| 2026-10-08 | The lamplight at the house: spreading, or shaded | Open: Luis's choice | With the jump gone there are two steady looks, and Luis has liked both: the lamplight spreading over the ground (the look from far, liked on October 2; the place starts with it) or shaded by what stands in it (key `0`). Spreading gives up the shadows that lamplight throws ([the round's page](Reviews/2026-10-08_ThePlaytestRound.md#l2-the-cabins-light)) | Luis's word |
| 2026-10-08 | How a movement is checked | Working agreement, from Luis's request | Luis asked for ways "to check every animation, make everything natural, and debug everything", naming judges that judge each other. Three things, in order: the movement traced frame by frame at the rate Luis plays at and read for breaks; judges who did not make it, each with its own question; then Luis ([MotionJudging.md](ArtDirection/MotionJudging.md); the research behind it: [Research/](Research/2026-10-08_RealBodiesAndHowToJudgeThem.md)) | — |
| 2026-10-08 | S3 step 12: evidence | Gathered; Luis has not played the build | The plan's list of evidence, item by item: ten of twelve shown, one in part, one not (the pickaxe is not stopped at its bearer's body). The release build's own measures: one miner at its physical work costs nothing that shows in the frame; a hundred walking miners add about 2.5 ms. Running the build found that a light body fell on any long walk with its pickaxe (put right), and trying every boulder that eight of thirty miners and boulders gave their rock up for their knees: a miner now looks for a way to stand that bends them less ([the list](Design/ThePhysicalBody.md#step-12-evidence-october-8)). All of S3 stays Implemented, not Locked | Luis's play of the build |
| 2026-10-08 | S3 step 11: the panel in the Ordinary Place | Built; Luis has not seen it | A plain panel takes the keys' place: a strength slider, and a light, its own or a heavy pickaxe put on the ground beside the chosen miner (shares of its own pickaxe's weight). Nothing is made in a hand. Tried along the whole slider, it showed that the squat for a pickaxe sat at the edge of the knees: so a body now bends its knees no deeper than one could hold alone, places itself before it goes down, and has its knees read before it works at a rock ([design](Design/ThePhysicalBody.md#step-11-the-panel-in-the-ordinary-place-october-7)). Whether the keys stay, what is on the panel, the three pickaxes, what a body too weak to get down does, how careful it is with its knees, and work at low rock all stay Open | Luis's play of the build |
| 2026-10-07 | S3 step 10: the fall, and getting up | Built, shown to Luis as clips; not trusted yet | When its steps do not catch it, or its legs cannot bear it, the body is let go into the physics as eleven jointed parts of its own weights; it holds itself with its own strength, lies, gathers itself, and gets up from a crouch its legs can raise it from ([design](Design/ThePhysicalBody.md#step-10-the-fall-and-getting-up-october-7)). When it falls, how hard it holds itself, how long it lies, whether a fall costs it anything, and the getting up are first settings and stay Open. (The getting up was made again on October 9: see that row) | Luis's eye on the clips and in the build |
| 2026-10-07 | S3 step 9: any boulder, by a click | Built, shown to Luis | A boulder offers "Mine"; where to stand and where to strike come from the rock's own shape; the miner takes the last steps off the walked ground to the rock's foot; blows break pieces off, which fall and lie ([design](Design/ThePhysicalBody.md#step-9-any-boulder-by-a-click-october-7)). A tool too heavy to rest holding is put down to rest (a change to step 5's rest, for Long). What a piece costs (90 joules of blows), its size, whether a boulder gets smaller and runs out, whether "Mine" ends by itself, and the rest with the head down stay Open | Luis's eye; Luis's word on those |
| 2026-10-07 | S3 step 8, second half: the lantern and the mug taken in hand and hung back | Built, shown to Luis | The hand on the thing's side takes it off its hook by the handle and carries it at the side, the arm hanging and swinging less (the carry from before October 6, brought back); it is hung back the same way; a miner hangs it back before it takes its pickaxe ([design](Design/ThePhysicalBody.md#step-8-second-half-the-lantern-and-the-mug-taken-in-hand-and-hung-back-october-7)). Which hand carries what, and whether these are ever set down on the ground (they would have to be made objects of their own), stay Open | Luis's eye; Luis's word on setting them down |
| 2026-10-07 | S3 step 8, first half: the interaction click; the pickaxe laid down and picked up | Built, shown to Luis | The space bar and a click open a thing's options beside it, with a cancel; the pickaxe is laid down and picked up, and the miner rests, each by its body ([design](Design/ThePhysicalBody.md#step-8-first-half-the-interaction-click-and-the-pickaxe-laid-down-and-picked-up-october-7)). Space, the list of options and a knee's strength (now by the body it carries) are first settings and stay Open. The lantern and the mug are still to do | Luis's choice of the key; Luis's eye |
| 2026-10-07 | S3 step 7: holding and walking with the tool | Built, shown to Luis | How a tool is held comes from the hand's hold: carried in one hand at the side, dragged when it asks more than half the hold, left when it cannot be moved ([design](Design/ThePhysicalBody.md#step-7-holding-and-walking-with-the-tool-october-7)). "In two hands" from the plan was not built: a second hand does not share a pickaxe's weight. Carrying a heavy pickaxe over the shoulder is Possible, proposed to Luis. The thresholds are first settings and stay Open | Luis's eye; step 8 (laying down and taking up) |
| 2026-10-07 | S3 step 6: balance | Built; Luis on its clips: "looking pretty good" ([message](Correspondence/2026-10-07_THE_BALANCE_LOOKS_PRETTY_GOOD.md)) | The body keeps its own balance from its real weights and loads: at ease it stands as it did; it leans against a load; it sets its feet for the work; it steps when its weight's point leaves its feet. Each body's own way of standing is its ease. The knees have a strength of their own ([design](Design/ThePhysicalBody.md#step-6-balance-october-7)). How firm it is, how soon it steps and the stance for the work are first settings and stay Open. The fall is step 10 | Luis's eye; step 10 |
| 2026-10-06 | S3: the swing with real weight, to try in the build | Built; not yet tried by Luis | In the Ordinary Place `K` shows the swing with real weight on a block, in place of the old swing, with keys for strength and for the pickaxe's weight ([how to try it](Design/ThePhysicalBody.md#in-the-place-to-try-october-6)). An early part of step 11, brought forward so the work can be watched; the plan had the key retired, and whether it stays is Open | Luis's eye; the panel of step 11 |
| 2026-10-06 | S3 step 5: tiredness | Built on the bench, shown to Luis | Each arm and the back tire with hard work and recover with rest; light work does not tire; a tired body is a weaker body by the same rules. It rests by itself at 40% spent, standing up with the tool carried in one hand at its side, and goes on at 15% ([design](Design/ThePhysicalBody.md#step-5-tiredness-october-6)). The paces, a longer tiredness and what the player is shown stay Open | Luis's eye; step 11's panel |
| 2026-10-06 | S3 step 4: the swing | Built on the bench, shown to Luis | All three miners swing their own pickaxes by their own strength. By itself the body takes a heavy tool nearer its head to lift it, straightens as its back can, puts its hips back to keep its weight over its feet, and bows and bends its knees to reach where it aims ([design](Design/ThePhysicalBody.md#step-4-the-swing-october-6)). Long's pickaxe is heavy for Long: left as it is, for the creator's strength and tool choices | Luis's eye; steps 5, 6 and 9 |
| 2026-10-06 | S3 step 3: both hands free | Built, shown to Luis | Both hands of all three miners close. Small's lantern hangs from a hook on the coat in front of the left thigh; Long's mug in a loop of thong at the left hip. Where they hang is Implemented, not Locked ([TheMiners.md](ArtDirection/TheMiners.md#both-hands-free-the-lantern-and-the-mug-at-the-hip-october-6)) | Luis's eye |
| 2026-10-06 | Luis's look at the bench | Direction from Luis | "The clips are very promising so far. On the hands side it's already looking quite good"; what is wanted is "a full swing that will involve the body adapting to the strength capabilities and rock"; go on with the plan as it is ([correspondence](Correspondence/2026-10-06_THE_BENCH_IS_PROMISING.md)) | Steps 4, 6 and 9 |
| 2026-10-06 | S3 steps 1 and 2: weights, and the bench | Built; seen by Luis the same day (next row up) | Every part of every miner and every pickaxe is weighed on its model. On a bench, a pickaxe is a real body in Round's hands, moved only as hard as its arms' joints can; strength is built from the limbs' thickness times one number. What a swing manages comes out of weight and strength ([design](Design/ThePhysicalBody.md#9-what-is-built)). How the arms are built (forces at the hands, the arms following) is Implemented, not Locked | Luis's judgement of the clips; steps 3 and 4 |
| 2026-10-06 | The order of stages | Approved by Luis | S3 (weight and strength at the rock), S4 (carrying and equipment), S2 (the creator), S5. Where the new things below fit is left to Claude ([correspondence](Correspondence/2026-10-06_STABLE_BUT_ABLE_TO_FALL_AND_THE_INTERACTION_CLICK.md)) | S3's steps as they are shown |
| 2026-10-06 | Body technique (replaces D1) | Direction from Luis | "Very stable", never "naturally wobbly", yet able to trip or fall "in this, like, ragdoll-ish way if it's an extreme situation that calls for it" (a very strong pull, a thrust put in too hard, limbs giving out). Proposed to realize it: the ladder of balance, where real weights decide whether the body leans, braces, steps or falls ([design](Design/ThePhysicalBody.md#2-can-very-stable-and-able-to-fall-be-combined)) | The bench and the clips of each rung |
| 2026-10-06 | Stamina | Direction from Luis | "Some level of stamina for the muscles and tiredness", short-term at least, already in this prototype. Its pace, any longer-term tiredness and what the player is shown stay Open | S3 step 5 |
| 2026-10-06 | The lantern and the mug (choice B) | Chosen by Luis | They hang "somewhere … that makes physical sense, from the clothes". Hands are empty | S3 step 3, shown beside the present miners first |
| 2026-10-06 | The interaction click | Direction from Luis | The ordinary right-click stays the RTS order. A key and a click on something that can be interacted with show its options beside it, "MMO style", with a cancel. It is for actions an ordinary RTS has no click for, and its key would be "the most important" after moving the map. The key (Space is proposed) and the list of options are Open | S3 step 8 |
| 2026-10-06 | Every action is physical | Direction from Luis | "I want all the actions to feel physical and real … It needs to be all the actions": equipping, unequipping, strapping, putting into a bag are seen done by the body, believably, with real weight | S3 step 8; S4 |
| 2026-10-06 | Built to be built upon | Direction from Luis | What is made for the prototype is what the final game grows from: running and sword fights come later, on the same body | — |
| 2026-10-06 | What the prototype is for | Direction, restated by Luis | Bodies "react against real physics, with real strength in their limbs, and against objects with real weight"; "that's the whole point of it". Like ragdolls, without being goofy ([correspondence](Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md)) | — |
| 2026-10-06 | Any rock | Direction from Luis | The miner mines any of the map's boulders by a click, not a rock made for mining; the mining adapts to the rock. The prototype is small in scale and final in kind | — |
| 2026-10-06 | Body technique (D1) | Replaced the same day (see "Body technique (replaces D1)") | D1 (a body posed, not moved by forces) was chosen "for this milestone" on Claude's recommendation. Luis's message asks for real physics. Proposed in its place: physical work on planted legs, then the whole body as a marked experiment ([review](Reviews/2026-10-06_SamePageReview.md#6-how-a-body-can-answer-to-real-forces-without-being-goofy)) | Luis's choice A |
| 2026-10-06 | Strength | Possible (Luis's present thought) | One general number, "maybe" a slider, in place of named bands (D3) and of build plus training (O2); later perhaps by limb. It shows gently in the body's appearance | Luis's decision; the creator (S2) |
| 2026-10-06 | The hands | Possible (Luis's present thought: "I'm not sure yet") | Luis's first idea was that the miners hold nothing, so that the hands are about the pickaxe and strength. The lantern and the mug were allowed "for now" | Luis's choice B |
| 2026-10-06 | Equipment | Direction from Luis | A choice with consequences: no pickaxe (cannot mine), a pickaxe, a strap for the back (asks strength of the back), a backpack, a dragged sack, a wheeled cart. The rocks are brought back; "all of that needs logistics and real strength and weight against it" | S4 |
| 2026-10-06 | The creator | Direction, restated by Luis | Part of the hi-fi prototype; the three miners are its appearances | S2 |
| 2026-10-06 | Review of the whole project | Done, at Luis's request | The look is on Luis's page. The physical work and the creator are not begun, and this week's mining has no weight in it. Proposed order: S3 (weight and strength at the rock), S4, S2, S5 ([review](Reviews/2026-10-06_SamePageReview.md); [plan](Plans/S3_WeightAndStrengthAtTheRock.md)) | Luis's choices A, B, C |
| 2026-10-06 | Where and how the miners mine | Overtaken the same day (M1 answered: any boulder; M2 to M4 become choices of equipment) | The place, where a pickaxe is kept, what the lantern's or the mug's hand does, and what happens to what is mined. A look at the mining is in play to help choose ([the plan](Plans/S1_OrdinaryPlaceCamerasAndMiners.md#choices-for-luis)) | Luis |
| 2026-10-05 | How Long stands in the game | Open (P1) | Since October 3 the prefab stands each model with its head above its hips. Long was built upright with the head forward, so in the game Long leans back 11°. Found while fitting the closing hands; not changed ([pictures](ArtDirection/TheMiners.md#for-luis-how-long-stands-in-the-game)) | Luis |
| 2026-10-05 | Luis's verdict on the method's first round | Direction from Luis | "I really liked all the changes you did, except the shoulder strap." The strap on Small's shoulder is put back as it was; its ends stay on the bag's rings ([correspondence](Correspondence/2026-10-05_THE_SHOULDER_STRAP.md)) | — |
| 2026-10-05 | Change only what is asked | Working agreement (Claude's lesson) | A model Luis has seen and not faulted is liked as it is. A note changes what it names; the method's own rules do not restyle the rest | — |
| 2026-10-04 | The method's first round on the miners | Luis liked it, October 5 (all but the strap) | Luis's five notes on Small are answered (leg and boot, laces, the strap through rings on the bag, the hand round the lantern's handle, the bag hanging). All three were taken through the method; it found eighteen more ([TheMiners.md](ArtDirection/TheMiners.md#the-methods-first-round-october-3-and-4)) | Luis's playtest |
| 2026-10-04 | Keep what Luis liked | Working agreement (Claude's lesson) | A fix that would change a movement or shape Luis has praised is made beside it, not through it. Small's and Round's skirts keep the movement Luis liked; only Long's long coat waits for the leg to reach it | — |
| 2026-10-04 | The walk's high step in the sharpest turn | Open | For a frame, the knee shows under the coat's lifted hem. Calming it means changing the walk (less lift on a short step), which is Luis's to decide | Luis |
| 2026-10-03 | Model quality method | Working agreement, from Luis | Every model is checked and improved by [ModelQualityMethod.md](ArtDirection/ModelQualityMethod.md) (seven passes, automatic checks at rest and in motion, a capture matrix, a misses ledger) before Luis sees it | — |
| 2026-10-02 | Exploration process | Working agreement (Claude's lesson) | Explorations are added as switchable options beside what Luis loves, never replacing it, and shown early as frames | — |
| 2026-10-02 | Wonder Gather's own language | Proposed, not Locked | Thesis "warm lives, drawn by hand, in a breathing painted world that is always bigger than the frame", seven signature devices and a frame test ([TheEssence.md](ArtDirection/TheEssence.md)) | Luis's choice of devices |

Add future entries with the decision, its status, the evidence behind it, and what kind of playtest or new requirement would justify revisiting it.

## Provenance

This source of truth is based on the recovered “Branch · Game Concept Development” conversation and the current repository state. A prior 49-page dossier was referenced in that conversation, but the actual attachment contents were not available in this task. Ideas mentioned only by the assistant have been labeled **Possible**, **Direction**, or **Open** unless the user explicitly accepted them. Future user decisions take precedence over this document.


## Gatherer playtest follow-up — September 8, 2026

The user reported that the gathering loop worked. The requested HUD clipping correction is implemented. The next proposed step remains construction within The Little Settlement: spend gathered supplies to place a building and send a worker to complete it, followed by unit production. Resource names and balance values remain provisional.


## Construction step — September 8, 2026

The user approved proceeding from the accepted Gatherer to construction. Implemented for playtesting: one workshop definition, preview placement, validation and spending stored supplies, worker travel and timed construction, interruption/resumption, and navigation footprints. Prototype values are 20 supplies, eight seconds and a three-unit square footprint; these are not locked balance decisions. Unit production remains the next step toward the full Little Settlement loop.


## Unit production — September 8, 2026

The user accepted construction and approved unit production. Implemented for playtesting: selecting a completed workshop, training one worker type with stored supplies, a three-entry queue, cancellation/refunds, safe nearby spawning, and full selection/gathering/building integration for produced workers. The provisional worker costs 10 supplies and takes six seconds; these values and queue size are not locked design decisions. This completes the first gather → build → produce loop of The Little Settlement. Civilization data and production relationships remain the next planned foundation after playtest acceptance.


## Civilization blueprint foundation — September 9, 2026

The user accepted the production result and approved working together on the
civilization blueprint foundation. The gather → build → produce loop is now
provisionally accepted. This milestone adds editable starting-base, unit,
building and civilization assets, construction permissions, production links,
runtime starting setup and inheritance of produced workers' blueprints.
Inspector tooling checks structural reachability and reports missing supply
gathering. Warnings preserve intentional challenge designs; invalid runtime
references must be corrected before instantiation.

The standard sample preserves eight starting workers and zero stored supplies.
A provisioned sample uses three workers and 40 supplies to demonstrate that
data changes affect play. These are implementation examples, not locked
factions or balance choices. The central base is a completed depot placeholder.
Each building currently supports one produced unit type; the framework's first
unit prefab contract remains a worker with optional gather/build permissions.

Only supplies are implemented. The report does not simulate complete economic
chains or prove resilience, map feasibility, or competitive legality. The
two-cost distinction is preserved: existing in-match costs are reused; no
design-budget formula or inferred balance pricing is introduced. That design
work remains open, as do new resource types and the polished graph editor.

After playtesting this foundation, collaboratively choose a small second role
or production chain to test meaningful civilization variation, then develop
design-value rules with concrete examples before moving to Three Temperaments.
This is a proposed next refinement, not a newly locked roadmap.


**Collaboration clarification:** The user reaffirmed that their specific vision
and aesthetic guide must direct the work. Check in with concrete proposals
before introducing visual direction, new unit roles or meaningful design
changes. Technical sample assets are provisional. The current written guide
captures grounded fantasy, quiet awe, expressive bodies and rejection of
slapstick physics; the location of any more detailed aesthetic source has been
requested and must be reviewed before visual decisions.


## Player-facing faction creator — September 9, 2026

**User clarification:** The final game must let players design their own
factions through blueprints, supported by a high-quality creation interface.
Developer-facing Inspector assets are a foundation, not the intended final
player experience.

The user accepted the civilization foundation and approved a first functional
creator with a central blueprint graph, adjacent customization panel and
starting-setup summary. Implemented for review: faction naming, starting
worker/supply counts, editable gather/build/train permissions, live dependency
warnings, launch into the existing test map and return to the same draft.
Runtime draft copies protect authored example assets. Each playtest resets the
map while preserving the draft for this session.

The visual treatment is provisional and deliberately limited to a layout
prototype. No final art style, lore, new faction role or balance formula is
locked by this implementation. The 0–8 workers and 0–120 supplies controls are
test-map bounds. Save/load, broader blueprint content, full visual graph
editing and the final HUD design remain future work. Review clarity and feel
with the user before choosing the next creator refinement.


## Faction saving and library — September 9, 2026

The user provisionally accepted the first creator and approved saving,
loading and duplicating factions as the next implementation. Added for
playtesting: Save, Save as copy, library Open/Rename/Delete, unsaved-change
feedback and confirmation before replacing edits or normally closing the
standalone player. Editor Stop Play Mode still requires saving first.

Saved data records the current small creator's blueprint IDs, links, starting
setup and name. Files are local and versioned; failed writes preserve the
draft, previous saves receive a backup, and deletions retain a recovery file.
Unknown/incompatible data is explained rather than silently discarded.

This does not lock the final faction schema, balance formulas or interface
aesthetic. The next proposed design discussion remains multiple customizable
unit/building blueprints and meaningful production chains, with the user
choosing roles and capabilities before implementation. Cloud sharing and
running-match saves remain separate possibilities.


## Multiple unit blueprints — September 9, 2026

The user accepted the saving/library prototype and approved multiple editable
unit blueprints, starting from an editable worker template. This slice adds
Add/Duplicate/Rename/Remove, per-blueprint starting counts and gather/build
permissions, one selected trained blueprint per workshop, saving and playtests.
Copies get distinct IDs and zero starting units. Removing a blueprint also
removes its starting entries and clears its training link after confirmation.

Eight blueprints and eight total starting units are provisional test-map/UI
bounds. Each workshop still supports one trained type; multiple training
options in a building remain a later extension. All types share the existing
worker body and production cost/time. Example Gatherer and Builder names are
playtest suggestions, not fixed roles or faction templates. Blank-template
creation remains possible future work; this slice starts from editable workers.

Saved factions now use version 2; version-1 saves open without rewriting and
upgrade on explicit save with a backup. No aesthetic, lore, stat list, design
budget or final graph layout is locked. Review the editing experience together
before expanding buildings, training options or blueprint customization.


## Building blueprints and production networks — September 10, 2026

The user accepted the multiple-unit prototype and approved editable building
blueprints, per-unit construction links and multiple training choices per
building. The creator now has Units/Buildings rosters, building naming and
duplication/removal, editable incoming/outgoing links and reachability warnings.
The playtest names constructed blueprints and exposes each training choice.
Mixed queues preserve requested types, costs, duration and refund ownership.

This prototype retains the fixed starting depot, shared worker/workshop bodies
and existing in-match costs. Eight unit and eight building blueprints, eight
starting units and three queue entries are provisional test bounds. The last
editable blueprint of each kind is retained; its links may all be disabled.
Duplicates copy outgoing choices but gain no new incoming links automatically.
Building and unit examples remain player choices, not locked faction roles.

Save version 3 preserves the whole supported network. Versions 1 and 2 load
without file changes and upgrade on explicit save with a backup. This does not
lock a final faction schema, graph layout, building taxonomy or aesthetic.
After playtesting, the proposed next collaborative design step is to choose
small, meaningful customization tradeoffs as examples for design-value rules.
That design work should be agreed with the user before implementation.


## Prototype unit performance — September 10, 2026

**User clarification:** All current systems are provisional foundations. The
final versions must be much deeper and more complex. True customization must
develop alongside procedural unit action and movement; current controls do not
define the final body, equipment, personality or behavior model. The user
delegated the temporary choice between stat controls and example modules.

**Implemented for review:** direct per-blueprint movement, carrying capacity,
gathering and construction controls. Rate percentages compare with the current
worker: 100% preserves its behavior; 200% doubles its speed or work rate. Carry
capacity is a count of supplies. Rates of 25–200% and capacity of 1–20 are
temporary test bounds. Defaults are 100%, five supplies, 100%, 100%.

These controls expose outcomes for testing. Later physical capabilities,
equipment, personality and circumstances can determine those outcomes. Work
rates do not grant missing gather/construction permissions. No cost formula is
introduced: production still costs 10 supplies and six seconds in the example,
and design value remains distinct from in-match costs. This slice is not a
balanced tradeoff system, final stat list or final customization interface.

Version 4 saves include performance; versions 1–3 receive the previous worker
defaults in memory and upgrade only on explicit save/rename with a backup.
The next design discussion should revisit which small behavioral or physical
prototype best advances the living-unit vision, using the user's playtest.


## The Living Body — September 21, 2026

**User-approved direction:** after the accepted unit-performance prototype,
begin a small procedural movement experiment. The user approved implementing
the proposed simple test biped and asked that documentation and GitHub remain
current. This changes the proof sequence: the first Living Body experiment
precedes Three Temperaments. The goal remains the deeper civilization design
vision, with physical capabilities and behavior eventually informing unit
customization.

**Implemented for review:** a separate `TheLivingBody` comparison scene with
the same provisional articulated body at measured and brisk pace; procedural
stepping with world-planted supporting feet, two-bone legs, body response to
acceleration and turns, and a flat/ramp/plateau test course. Existing selection,
orders, navigation and RTS camera controls remain the command interface.
Navigation moves the unit; the articulated body presents that movement.

**Acceptance remains with the user:** does the body feel grounded, readable,
expressive and enjoyable to watch at both close and strategic distance?
Specific proportions, pace, posture, movement character, colors and anatomy
are provisional test choices. No species, culture or final visual style is
established by this rig. Gather and construction body actions, personalities,
combat, balance physics, active ragdolls and a player-facing body creator are
outside this milestone. The final systems remain substantially deeper than
these proof cases.

The faction creator remains a separate working prototype with version-4
faction files and its existing temporary performance controls. No final
cost formula or faction-body serialization is introduced. The next design
discussion should use the user's movement feedback before choosing whether
to refine locomotion, connect a body action, or begin personality expression.


## Living Body playtest acceptance and next-step review — September 24, 2026

The user reported that the movement feels okay for this prototype and asked
for a review of the project and a plan for the next step. The Living Body is
therefore provisionally accepted. Its current body, aesthetic, physical
authority and movement tuning do not become final decisions.

**Proposed, awaiting design choice:** The Living Worker, connecting an
articulated faction worker to gathering, visibly carrying supplies and
delivery. This would join the existing civilization/economy and locomotion
foundations before revisiting Three Temperaments. Scope, integration gaps
and acceptance checks are archived in `Docs/Plans/LivingWorker.md`. This review does
not authorize implementation or lock a revised long-term roadmap.


## The Living Worker — September 26, 2026

**User-approved scope:** Luis approved the Living Worker plan, including the
provisional collecting gesture and carried bundle. The existing faction
playtest now uses articulated workers for both starting and produced units.
Gathering and delivery use reachable reserved positions; visible cargo and
arm poses follow actual task state. Interrupted work retains carried supplies.
The existing creator controls and version-4 faction files retain their meaning.

**Implemented for review:** one restrained reach-and-collect action, carrying
support, a short delivery gesture, activity/cargo HUD feedback, eight places
at the supply station and base, and waiting when places are occupied. The
worker gait adapts to the existing movement range. Construction behavior is
preserved; a procedural construction gesture is outside this slice.

These are integration proof cases. They do not finalize the resource fiction,
station design, body, pace, species, equipment, autonomy or aesthetic. No
load penalty, cost formula, physics balance or save migration is introduced.
The next decision follows Luis's judgment of the integrated work routine;
Three Temperaments remains a possible follow-up after that playtest.


## Living Worker acceptance and showcase clarification — September 28, 2026

Luis accepted the latest prototype while emphasizing that its implementation
is a foundation for much deeper systems. His one-worker showcase reference
is now captured in the dedicated canonical section above and expanded in
Docs/Design/WorkerShowcaseVision.md. The September 29 source review recommends
blueprint equipment and contact-based mining next; it changes documentation
only. The September 26 technical evidence remains the latest executed validation.


## The Equipped Worker — September 29, 2026

**User-approved scope, implemented for review:** Luis approved the next
equipment/contact slice. Unit blueprints now choose None or an authored
pickaxe, and starting/produced workers receive that choice. The new equipment
playtest uses a mineral surface with reserved working positions. Supported,
reachable tool grips and the actual solved head strike gate one unit of
extraction per accepted attempt. Misses and interrupted work give no free
material; real cargo remains conserved through delivery.

**Temporary implementation choices:** one fixed-scale biped and primitive
pickaxe, a bounded preparation/strike/recovery motion, fixed yield, material
represented beside the feet during mining, and a short side-to-back stow
transition for transport. These are proof cases awaiting Luis's judgment,
not final body proportions, tool shape, animation character, resource fiction,
hauling rules or aesthetics. Saves extend deliberately to version 5; older
designs open with None without being rewritten just by opening.

Technical validation passed; Luis's playtest acceptance is pending. The
deeper character-creator and civilization/building modularity remain the
target. Strength-dependent tool handling, one-handed carry/dragging, bags,
carts and material burden remain the recommended following experiments.
Force/angle-dependent yield and their exact rules remain open.
Reference: Docs/Playtests/EquippedWorkerPlaytest.md and Docs/Technical/EquipmentArchitecture.md.


## Equipped Worker feedback and next proposal — September 30, 2026

Luis reviewed The Equipped Worker. The pickaxe "looks more or less held". The
motion, however, "looks just like an animation still", the tool clips the body,
and the feet still look goofy. Overall it "looks like the bare starting points"
of the hi-fi prototype. Luis restated that movement should be procedurally
animated without looking goofy, and re-supplied the original showcase brief,
now kept verbatim under `Docs/Correspondence/`.

A source review traced the three problems:

- **Feet.** The worker moves at 3.2 m/s, above the walk–run threshold for its
  leg length, with short reactive steps at about five per second.
- **Strike.** The swing is a fixed angle curve around a point in front of the
  belly, with no body participation.
- **Clipping.** The windup places the pickaxe head at the worker's own head.

**Proposed, not approved:** Strength and Burden, in three checkpoints each
returned for playtesting:

1. a grounded body with gait from body dimensions, rolling feet, weight
   transfer, a spine, balance and body volume;
2. an effort-driven strike shaped by tool mass and strength, keeping the
   existing contact contract and measuring impact without changing yield;
3. strength-resolved tool handling (one-handed, dragged or needing an aid),
   with load-dependent movement and readable inability.

The recommended technique is a physics-informed kinematic body rather than an
active ragdoll. Luis decides that choice (D1) and the other open choices
(D2–D9) in `Docs/NextMilestonePlan.md` before implementation. None of the
proposal's examples, thresholds or recommendations is locked.

## Strength and Burden, checkpoint A — September 30, 2026

**Implemented for review:**

- **Pace (D2).** The worker walks at 1.8 m/s by default; faster Movement % jogs.
- **Gait.** Stride and cadence follow hip height and speed through a
  Froude-number walk/jog threshold and a phase-based gait.
- **Feet.** Heel-to-ball feet with toes strike, roll and push off.
- **Body.** The pelvis sways, rotates and dips while the chest counter-rotates.
  The head stays level. Free arms swing as pendulums.

Navigation still owns the root, and nothing is an active ragdoll (D1).
Stride, sway and timing values are provisional first choices. Neither the
pace nor the gait character is locked until Luis judges them. The swing,
strength, mass and handling follow in Checkpoints B and C.

## Checkpoint A feedback and showcase scope — October 1, 2026

Luis's feedback is kept verbatim in
`Docs/Correspondence/2026-10-01_CHECKPOINT_A_FEEDBACK_AND_SHOWCASE_SCOPE.md`.

**Walk and arrival.** The walk is "much better". Arriving bodies used to stop
and then make separate adjustment steps toward the exact spot. Now the last
steps land where the body stops, at most one closing step follows without a
pause, and a settled stance tolerates small drift.

**Scope.** The rest of the feedback clarifies the showcase described in the
advanced prototype section above:

- one worker and a character creator rather than a civilization creator;
- a constant pace;
- real hauling options instead of a back-stowed pickaxe;
- Blender-quality models;
- organized documentation and GitHub uploads at every step.

**Roadmap.** `Docs/ShowcaseRoadmap.md` proposes stages S1–S5:

- S1: model and art direction;
- S2: worker creator;
- S3: effort-driven work;
- S4: strength and hauling;
- S5: world, presentation and playtest readiness.

The Strength and Burden plan is archived. Its checkpoints B and C continue as
S3 and S4.

**Still open:**

- art direction;
- whether strength is a separate value or comes from body build;
- whether mined material falls as loose pieces;
- the stage order;
- third-party asset sourcing.

## Real weight, real strength, any rock — October 6, 2026

After the report on the miners' first swings, Luis sent a spoken message
([verbatim](Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md)).
It answers the open choices in a wider way and restates what the hi-fi
prototype is for.

**Said plainly:**

- the miners mine any of the map's boulders, by a click; no rock is made
  for mining; the prototype is built the way the game itself will want it;
- the tool has real weight and the body really lifts it; bodies "react
  against real physics, with real strength in their limbs, and against
  objects with real weight"; this is "the whole point" of the prototype;
- like ragdolls, without being goofy;
- the character creator is part of the prototype, with the three miners as
  its appearances;
- equipment is a choice with consequences: no pickaxe, a pickaxe, a strap
  for the back, a backpack, a dragged sack, a wheeled cart; the rocks are
  brought back.

**Said as present thoughts** (kept as Possible): empty hands; strength as
one general number, maybe a slider, later perhaps by limb; strength showing
gently in the body.

**The review Luis asked for** is
[Reviews/2026-10-06_SamePageReview.md](Reviews/2026-10-06_SamePageReview.md).
Its findings: the look is on Luis's page; the documents hold what Luis
says; the work has not reached it. Since October 1 every commit concerned
the look, the cameras and the models. The mining built on October 6 is a
stored curve with no weight in it. Decision D1 (September 30) took the body
away from physics on an argument that joined "like a ragdoll" with "goofy",
which Luis keeps apart.

**Proposed, waiting for Luis:** S3 (weight and strength at the rock) next,
then S4 (carrying and equipment), S2 (the creator) and S5
([plan](Plans/S3_WeightAndStrengthAtTheRock.md)).
Choices A (how far the physics goes), B (the lantern and the mug) and C
(the order) are Luis's.

## Stable but able to fall, stamina, the interaction click — October 6, 2026 (second message)

Luis answered the review's three choices
([verbatim](Correspondence/2026-10-06_STABLE_BUT_ABLE_TO_FALL_AND_THE_INTERACTION_CLICK.md)).

- **The order is approved:** S3, S4, S2, S5.
- **The physics is a mixture.** Characters are very stable and never
  naturally wobbly, yet can trip or fall, ragdoll-ish, when an extreme
  situation calls for it. Luis asked whether the two can be combined. They
  can: it is how people stand (sway, bend, step, and only then fall) and
  how the games that do it well are built. The design is in
  [Design/ThePhysicalBody.md](Design/ThePhysicalBody.md).
- **The lantern and the mug** hang from the clothes; hands are empty.
- **Stamina** is wanted already: short-term tiredness of the muscles.
- **The interaction click** is new: a key and a click show a thing's
  options beside it, with a cancel, for actions an ordinary RTS has no
  click for. The ordinary right-click is unchanged.
- **Every action is physical,** not only the important ones.
- **Built to be built upon:** running and sword fights later use the same
  body.

The plan is [NextMilestonePlan.md](Plans/S3_WeightAndStrengthAtTheRock.md): twelve steps,
the bench first.
