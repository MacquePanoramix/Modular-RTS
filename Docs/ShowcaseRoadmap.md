# The Worker Showcase — roadmap to the playtest build

**Created:** October 1, 2026, from Luis's clarification of the showcase's scope.
**Status:** Living plan. On October 1 Luis chose:

- **O1:** render style studies to pick the art direction. Later that day the
  studies were set aside for Luis's Visual Soul handoff, and S1 was revised
  (two cameras, the Ordinary Place, rendering candidates, then the model).
- **O4:** the worker model first.
- **O2:** strength comes from the body plus training.
- **GitHub:** each stage merges into `main` after Luis's playtest.

Each stage still gets its own plan in [NextMilestonePlan.md](NextMilestonePlan.md).
**Sources:**

- [Original brief (Sept 28)](Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md)
- [Scope clarification (Oct 1)](Correspondence/2026-10-01_CHECKPOINT_A_FEEDBACK_AND_SHOWCASE_SCOPE.md)
- [WorkerShowcaseVision.md](Design/WorkerShowcaseVision.md)
- [GAME_VISION.md](GAME_VISION.md), which remains the design authority

## What the showcase is

A small-scale, high-fidelity prototype that outside players will try. It shows
what a single worker is like when made in Wonder Gather's character creator:

- **One worker** in a small landscape with one mineral boulder and a place to
  deliver what it mines. More workers may come later; the first version has one.
- **A character creator for that worker** in place of the civilization/faction
  creator. It should feel like a video-game character creator, and it sets the
  quality bar for the final unit customization.
- **Always a worker.** Gathering is built in; the creator decides how this
  particular worker looks, what it can handle, and how it does the job.
- **Physical work:**
  - the worker walks to the boulder and strikes it with a real pickaxe;
  - it gets the material home with its hands or with gear, depending on what
    it is strong enough to do;
  - it delivers the material and returns.
- **Very high quality.** Models (made in Blender), motion, interface, lighting
  and feedback should be good enough for strangers to judge the idea, not the
  placeholders.

## What it is not

- It is not a slice of the civilization game. There is no faction graph,
  production, construction or multiple unit types in the showcase flow. Those
  systems stay in the repository for the full game.
- It does not define "worker" as a unit type. In the full game, players
  customize units that become workers. The showcase isolates that one slice.
- It is not a movement-speed playground. Base speed stays constant and is not
  a creator option for now. Loads may still change how the worker moves.

## The player's journey

1. **Create.** A 3D worker stands in a softly lit preview. The player rotates and
   zooms it and works through categories:
   - body;
   - face and hair;
   - clothing;
   - tool;
   - hauling gear.

   A panel explains what this worker will be able to do, for example "Carries
   the pick in one hand" or "Too weak to lift the heavy pick — needs a cart."
2. **Enter the world.** The same worker appears in a small landscape, along with
   the boulder and the delivery point.
3. **Work.** The player orders the worker to mine. It walks with the grounded
   gait, finds a stance and strikes the stone with an effort-driven swing. The
   stone yields material.
4. **Haul.** The worker gets the material home in the way its body and gear
   allow:
   - load in one hand and pickaxe in the other;
   - backpack;
   - sack;
   - cart.

   Weight shows in its movement.
5. **Compare.** The player returns to the creator, changes the worker and tries
   again. Differences are visible and understandable without numbers.

## What carries over, and what changes

| Current system | In the showcase |
|---|---|
| Procedural gait (checkpoint A) | Kept. Driven by the real model's dimensions instead of fixed constants |
| Primitive biped segments | Replaced by a Blender-made, skinned worker; the primitive rig stays for tests and comparison |
| EquippedTool contact gate, Gatherer, ResourceNode | Kept as the authority for valid strikes and conserved material |
| Canned swing curve | Replaced by an effort-driven swing that stays out of the body |
| Back-stowed pickaxe, two-handed bundle | Replaced by strength- and gear-dependent hauling |
| Faction creator (IMGUI), faction saves | Not part of the showcase flow. Replaced there by a new character creator and its own character saves |
| Movement % and other rate sliders | Not in the showcase creator. Base pace is a constant natural walk |
| RTS camera and orders | Two toggled modes (October 1): a fast Strategy camera and a free Explore camera for close observation and wonder |

## Stages

Each stage ends with a Windows build, recorded evidence, documentation, a
GitHub push and Luis's playtest. The order is a recommendation.

### S0 — Grounded body (done, October 1)

Strength and Burden checkpoint A delivered:

- gait scaled to the body, plus a jog;
- heel/toe feet and weight transfer;
- a natural 1.8 m/s pace.

Luis judged it "much better". The arrival shuffle Luis reported is fixed: the
body finishes its stride and stands still. See
[GroundedBodyPlaytest.md](Playtests/GroundedBodyPlaytest.md).

### S1 — The Ordinary Place, two cameras and the worker model (in progress)

Revised on October 1. Luis set aside the three Blender style studies and
supplied the Visual Soul handoff ([VisualSoul.md](ArtDirection/VisualSoul.md)). Luis also
asked for two camera systems. The detailed plan is in
[NextMilestonePlan.md](NextMilestonePlan.md). It has four checkpoints:

- **S1a, two cameras.** A fast "best RTS possible" Strategy camera, and a
  free Explore camera. Explore is cozy and soft, gets very near things without
  passing through floors or buildings, and can look at the sky. `V` toggles
  between them.
- **S1b, the Ordinary Place.** A small house, grassland, path and the worker,
  by day and by night, in Unity.
- **S1c, rendering candidates.** Compared live in that scene, with matched
  captures and measured frame time. Luis chooses the rendering approach.
- **S1d, the worker model.** Made in the chosen language. This continues the
  original S1 scope below. October 2: three concepts
  ([WorkerConcepts.md](ArtDirection/WorkerConcepts.md)), then, after Luis's
  ranking, the miners remade at higher quality from modules
  ([TheMiners.md](ArtDirection/TheMiners.md)). October 3: polished, rigged,
  lightened (three levels of detail, one atlas) and walking in the Ordinary
  Place with the choice of three ([MinersPlaytest.md](Playtests/MinersPlaytest.md));
  mining with them is next.

The original S1 scope that continues as S1d:

- **Style studies.** Done on October 1, but not chosen. Superseded by the
  Visual Soul handoff (see [StyleStudies.md](ArtDirection/StyleStudies.md)).
- **The worker model.** Model a reproducible worker in Blender, with source and
  export scripts kept in the repository. It has a skeleton matching the
  procedural rig (pelvis, spine, chest, neck, head, arms, hands, legs, feet,
  toes) and weight-painted skin. It includes body-shape keys prepared for the
  creator.
- **A better pickaxe** whose grip and head points match its tool data.
- **Unity integration.**
  - A rig adapter applies the procedural solution to the model's bones.
  - Body dimensions come from the model's skeleton, so a human-proportioned
    body replaces today's 2.2 m test biped.
  - Navigation size and camera framing follow the new body.
- **Evidence.** Gait, support and grip tests pass on the model; rendered review;
  build.

### S2 — The worker creator

- **Creator scene.** A new entry scene with a 3D preview stage, orbit and zoom,
  and a creator interface built with Unity's UI Toolkit (not IMGUI).
- **Categories.** Body (height, build, strength), face and hair, clothing and
  colors, tool, hauling gear, and name. Presets and randomize are optional.
- **Consequences panel.** It shows what the current configuration can do.
  Its rules start simple and deepen in S4.
- **Showcase world shell.** One worker spawned from the profile, the boulder,
  the delivery point, and a way back to the creator.
- **Character saves.** A new, versioned character record, separate from
  faction saves, with the same read-without-rewrite and backup safeguards.

### S3 — Effort-driven work

This is the former checkpoint B.

- **The swing.** It is planned from the body's reach and kept clear of the
  body's own volume. Tool inertia, gravity and strength shape its speed. The
  whole body takes part.
- **Contact contract.** Unchanged: the displayed path is the swept path, and
  there is at most one extraction per attempt.
- **Yield.** Impact speed and energy are measured. Yield stays fixed until Luis
  decides (D6).
- **Material from the stone.** Decide with Luis whether material falls as loose
  pieces to pick up or goes straight into the worker's hands or gear (O3).

### S4 — Strength and hauling

This is the former checkpoint C, with D5 revised on October 1.

- **Strength.** A strength band, plus light, standard and heavy pickaxes with
  mass (D3/D4).
- **Hands as a resource.** Tools and loads compete for the two hands.
- **Hauling options from Luis's list:**
  - one-handed load plus pickaxe in the other hand, if strong enough;
  - a backpack (hands free);
  - a sack;
  - a cart (pulled, carrying tool and material).
- **Real loading.** Loading, carrying, unloading and delivery are actual
  actions. Load changes gait, lean and turning.
- **Readable inability.** The creator explains in advance what the worker
  cannot do. In the world, the worker shows that it cannot rather than failing
  silently.
- **Models.** Blender models for the sack, backpack, cart and pickaxe variants.

### S5 — World, presentation and playtest readiness

- **World.** A small landscape: terrain, vegetation, the boulder and the
  delivery point modeled in the chosen style.
- **Presentation.** Lighting and post-processing, sound (steps, strikes,
  ambience), restrained impact feedback and camera polish.
- **Playtest support.** Onboarding, reset and retry, an in-game feedback note,
  a measured performance target, and a standalone build for outside players.
- **Final review.** Luis's readiness review before anyone else plays it.

## How the Strength and Burden plan maps onto this

The approved Strength and Burden plan stays the source for S3 and S4. It is
archived in [Plans/StrengthAndBurden.md](Plans/StrengthAndBurden.md).

- Checkpoint A is done (S0).
- Checkpoint B becomes S3.
- Checkpoint C becomes S4. D5 is revised: no back-stow, and leaving the
  pickaxe behind is not the default.

D1 (physics-informed body), D2 (natural pace), D3/D4 (strength bands, three
pickaxes) still hold.

## Open decisions for Luis

| ID | Question | Recommendation |
|---|---|---|
| O1 | Art direction for the models (needed for S1) | **Set Oct 1:** the Visual Soul handoff (A–F approved), replacing the style studies. Its rendering technique, proportions, architecture and setting stay open (S1c) |
| O7 | Rendering approach | **Working base, Oct 1:** candidate E (painted light + paint filter + ink), with hand-painted textures explored next. Not Locked |
| C1–C4 | Camera toggle key, orders in Explore, Explore controls, entering buildings | **Accepted Oct 1:** `V`; orders allowed; Unity-style fly plus Blender-style orbit; buildings solid while they have no interior |
| V3 | Is the house the worker's home and delivery point? | Possible. It would connect the Ordinary Place to the worker loop |
| O2 | Is strength its own creator value, or does it come from the body build (musculature/mass)? | **Chosen Oct 1:** body build sets a base, and a separate training choice adjusts it within limits |
| O3 | Does mined material fall as loose pieces to pick up, or go straight to hands/gear? | Loose pieces, picked up into hands or gear. More physical, and it makes the hauling choice visible |
| O4 | Stage order: model first, or creator first? | **Chosen Oct 1:** model first |
| O5 | Character creator categories for the first version | Body, face and hair, clothing and colors, tool, hauling gear, name |
| O6 | Sources for sound and any third-party assets (for example MakeHuman/MPFB for a realistic body) | Ask before any download; record each license in a third-party list |
| D6–D9 | From the Strength and Burden plan | Unchanged recommendations: measure impact only, allow reduced swings, keep the capacity count until S4, replace primitives in S1 |

## Working agreement for this roadmap

- Every stage begins with a plan in NextMilestonePlan.md and Luis's approval.
- Work happens on a branch (currently `claude/worker-showcase`). A stage is
  merged into `main` after Luis playtests it (chosen October 1).
- Implementation and validation happen in an isolated copy while Luis's Editor
  stays open. Luis's uncommitted work is never included or discarded.
- Every stage ends with:
  - tests;
  - a rendered review;
  - a build;
  - updated docs (CURRENT_STATE, GAME_VISION, the stage playtest guide,
    Validation);
  - a GitHub push.

  Luis's feedback is then recorded verbatim under Docs/Correspondence when it
  changes direction.
- Implemented is not Locked. Visual style, tuning values and mechanics stay
  open until Luis judges them.
