# Wonder Gather — Current State

**Updated:** October 2, 2026.

- **The base look:** the hand-painted pass (S1c, second pass) in look E,
  merged into `main` on October 2 (7f7fcc0). Luis's favourite frame is from
  it: the lit house at dusk, seen from low on the path.
- **Latest implementation:** the dusk details over that look (third pass).
  Luis tested them and "loved it", so all three are on by default: fireflies,
  the hearth's flicker and a window glow. Keys 7, 8 and 9 switch each one.
  After Luis's notes:
  - **Fewer fireflies**, with a small painted dot still marking them when the
    camera is far out.
  - **The lamplight close up** now matches the zoomed-out look Luis liked.

  See [OrdinaryPlaceLookTest.md](Playtests/OrdinaryPlaceLookTest.md#after-luiss-test-october-2);
  build `Builds/WindowsOrdinaryPlace`.
- **Now:** S1d, the worker's redesign, from the Visual Soul. Its principles
  are the base art style for every being and thing in the world, not one
  character's costume
  ([VisualSoul.md](ArtDirection/VisualSoul.md#the-language-for-every-being-and-thing)).
  - **The miners (second pass), for Luis's judgment:** Small, Long and Round
    remade at much higher quality, each with miner's hints
    ([TheMiners.md](ArtDirection/TheMiners.md)):
    - **The models.** Real hands, hair in locks, sculpted boots, fitted and
      painted clothes.
    - **Miner's hints.** Small carries a lantern, Long leans on a pickaxe,
      Round wears a lamp cap.
    - **Built from modules** (body, face, hair, garments, accessories) that
      fit any body, towards the character creator.
  - **First concepts:** superseded
    ([WorkerConcepts.md](ArtDirection/WorkerConcepts.md)). Luis ranked them
    Small, Long, Round and loved every face.
  - **Beings stay drawn.** A material switch keeps them clear of the paint
    filter. It is off for everything else, so the world's look is unchanged.
- **Latest experiment, not adopted:** S1e, the essence beyond the surface,
  first iteration ([TheEssencePlaytest.md](Playtests/TheEssencePlaytest.md)).
  After seeing its captures, Luis preferred the hand-painted pass. Its code is
  kept on the branch `claude/essence-exploration`. It contained:
  - **The world beyond the meadow:**
    - a bluff over a still lake that mirrors monumental clouds and mountains;
    - a water horizon, layered ranges and a snow peak;
    - aerial perspective, cloud shadows and a luminous daylight palette.
  - **Look F:**
    - a painting pass whose strokes follow the forms;
    - ink on characters only;
    - the hour's palette;
    - seeds and fireflies in the air.
  - **The research.** [TheEssence.md](ArtDirection/TheEssence.md) holds the
    study of Luis's references and the proposed language unique to Wonder
    Gather. It also reads what Luis's favourite frame teaches about his taste:
    intimacy, the lit house at dusk, a soft brushed sky, enclosure and a tonal
    palette, rather than vastness and brightness.
- **Earlier steps:**
  - **S0 (the grounded body), S1a (two cameras) and S1b/S1c (the Ordinary
    Place, first pass)** are in `main`.
  - **The S1c second pass (hand-painted surfaces)** is on the branch. Luis
    found it "already quite beautiful", but not yet the emotion he seeks.
- **Overall plan:** [ShowcaseRoadmap.md](ShowcaseRoadmap.md).

## What the project is building now

A small-scale, high-fidelity **Worker Showcase** for outside playtesters:

- one worker, made in a video-game style character creator;
- a small landscape with one mineral boulder and a delivery point;
- physical mining and real hauling;
- models made to high quality in Blender.

The civilization/faction systems remain in the repository for the full game.
The showcase flow does not use them.

## Latest Game Director direction (October 2, late evening)

Recorded verbatim in
[Correspondence/2026-10-02_WORKER_CONCEPTS_FEEDBACK.md](Correspondence/2026-10-02_WORKER_CONCEPTS_FEEDBACK.md):

- **Ranking of the concepts:** Small, then Long, then Round. Luis loved the
  face of each one, and the idea of every character: distinct, unique, not
  generic, with soul.
- **Quality.** All three can be much prettier and more pleasing to look at,
  with much higher model quality. Keep the stylization.
- **They are all the miner.** Give each some hints, even small ones.
- **Maybe a choice of characters** in the final prototype. A possibility, not
  a decision.
- **Modular, all the way down.** The final game lets players customize
  everything, units included, down to appearance, like detailed character
  creators.

## Game Director direction (October 2, evening)

Recorded verbatim in
[Correspondence/2026-10-02_DUSK_DETAILS_AND_THE_WORKER.md](Correspondence/2026-10-02_DUSK_DETAILS_AND_THE_WORKER.md):

- **The dusk details.** "Honestly I loved it." Leave all of them on by
  default.
- **Fireflies.**
  - Fewer of them. Near the ground they went "from gentle wonder inducing to a
    bit overwhelming".
  - Even when very zoomed out, there should still be some sign of them,
    stylized and cheap to render.
- **The lamplight.** Far away it looked strong and spread onto the terrain;
  closer in that brightness disappeared. Luis liked the far look, so the closer
  view should be adjusted to match it.
- **Then the worker.** "Let's finally move to the player remodel/redesign",
  using the Visual Soul and the principles for people, "with heart and soul".
- **For every being and thing.** The scarf was only one of the examples. The
  Visual Soul's principles are the base art style for all beings and things in
  the world, across every kind of unit.

## Game Director direction (October 2, afternoon)

Recorded verbatim in
[Correspondence/2026-10-02_THE_ESSENCE_BEYOND_THE_SURFACE.md](Correspondence/2026-10-02_THE_ESSENCE_BEYOND_THE_SURFACE.md):

- **The hand-painted pass.** It is "already quite beautiful" and matches much
  of his taste, but is not perfect yet. He would rather explore further than
  stop.
- **The aim.** "The breath-taking out of this world emotion": the stylistic and
  artistic essence, not the surface; "the image from my mind's eye straight out
  of a dream". He sent seven Ghibli film references, which are described but
  not committed.
- **Every frame a painting.** The rendering style, not only the environment,
  should make any frame beautiful and driven by light, while the game still
  runs well as a modular RTS.
- **Unique.** The Visual Soul PDF is the artistic soul. The result must go
  above and beyond it, unique to Wonder Gather: "My soul game and style."
- **Time.** A long checkpoint with research is welcome.
- **The verdict.** After seeing the first iteration, Luis said "honestly I think
  I like the before better", with his favourite frame from the hand-painted
  pass. The hand-painted pass stays the base. S1e's first iteration is
  archived, not adopted.
- **His answers.**
  - **Merge:** the hand-painted pass, into `main`. Done.
  - **Fireflies:** back in this scene.
  - **Next:** deepen the dusk mood in small steps, then move to the worker
    model.

## Earlier Game Director feedback (October 1, night)

Recorded verbatim in
[Correspondence/2026-10-01_ORDINARY_PLACE_FEEDBACK.md](Correspondence/2026-10-01_ORDINARY_PLACE_FEEDBACK.md):

- **The Ordinary Place.** It is going "really well" towards the Visual Soul,
  but is not there yet.
- **Rendering.**
  - Candidate E (painted light + paint filter + ink) is his favourite, and is
    now the working base look.
  - Hand-painted textures are the next exploration; Claude recommends them.
- **Cameras.** Accepted ("I really liked the camera").
- **Merge.** S0, S1a and the S1b/S1c first pass are merged into `main`.
- **Organization.** Everything, including docs, goes to GitHub in a very
  organized structure. The docs were reorganized into topic folders (see
  [Docs/README.md](README.md)).
- **Moon.** It was visible in daylight. Fixed: it rises opposite the sun and
  shows only at night.

## Earlier Game Director direction (October 1, evening)

Recorded verbatim in
[Correspondence/2026-10-01_VISUAL_SOUL_AND_TWO_CAMERAS.md](Correspondence/2026-10-01_VISUAL_SOUL_AND_TWO_CAMERAS.md):

- **Art direction.** The Visual Soul handoff ([VisualSoul.md](ArtDirection/VisualSoul.md)):
  - "Even an ordinary moment should feel worth pausing for";
  - lighting leads the emotion, surfaces feel alive, characters have a soul,
    and the wonder is playful and personal;
  - images A–F are approved, with no ranking;
  - proportions, architecture, setting and rendering technique remain open,
    to be decided after an in-engine test.
- **Cameras.** Two toggled systems:
  - a fast "best RTS possible" Strategy camera;
  - a free Explore camera, editor-like, cozy and soft, that can go very near
    characters, floors and buildings without passing through, and look at the
    sky.

## Earlier Game Director direction (October 1)

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

1. **Luis checks the dusk fixes** in the build:
   - fewer fireflies, still marked when far out;
   - the lamplight close up.
2. **Luis judges the miners** ([TheMiners.md](ArtDirection/TheMiners.md)):
   - are they pretty enough now;
   - what to refine;
   - whether to offer the choice of three.

   Then the chosen miners are rigged on the procedural biped, given lighter
   levels of detail, and proven in motion.
3. **Further explorations are additive and switchable**, and shown early as
   frames before being built out.

Luis's other choices on October 1:

- **Model first** (O4).
- **Strength** comes from the body plus training (O2).
- **Stage merges.** Each stage merges into `main` after his playtest.

## Housekeeping

- **Branch.** Work is pushed to the `claude/worker-showcase` branch. It merges
  into `main` after Luis playtests the stage (including the arrival fix).
- **Luis's uncommitted local changes** are never included in commits:
  TheGroup.unity, two ProjectSettings files and `Assets/_Recovery/`.
- **`codex/agent-bootstrap-draft`** is an unmerged, stale automation proposal.
- **`docs/wonder-gather-continuity-2026-09-23`** is already merged as d03e045
  and can be deleted.

## Context router

- Showcase plan and open decisions: Docs/ShowcaseRoadmap.md
- Next concrete milestone: Docs/NextMilestonePlan.md
- Design authority: Docs/GAME_VISION.md
- Showcase reference: Docs/Design/WorkerShowcaseVision.md and Docs/Correspondence/
- Latest playtest guide: Docs/Playtests/GroundedBodyPlaytest.md
- Technical contracts: Docs/Technical/EquipmentArchitecture.md and Docs/Technical/UnityProjectContext.md
- Design meaning and collaboration: Docs/DESIGN_RATIONALE.md and Docs/PROJECT_CULTURE.md
- Evidence: Docs/Validation.md
- Completed plans: Docs/Plans/
