# Wonder Gather — Current State

**Updated:** October 1, 2026.

- **Latest implementation:** the grounded body (roadmap stage S0, formerly
  Strength and Burden checkpoint A), plus the arrival fix. Luis judged the
  walk "much better".
- **Current step:** S1 revised, the Ordinary Place and two cameras
  ([NextMilestonePlan.md](NextMilestonePlan.md)):
  - Luis set aside the Blender style studies and supplied the
    [Visual Soul](ArtDirection/VisualSoul.md) art direction.
  - He asked for a Strategy camera plus a free Explore camera.
  - S1a (two cameras) is built and tested, and waiting for his playtest
    ([TwoCamerasPlaytest.md](Playtests/TwoCamerasPlaytest.md)). The build is
    `Builds/WindowsTwoCameras`.
  - S1b/S1c (the Ordinary Place) has its first in-engine pass:
    - a lit house, grassland, path, trees, sky and time of day;
    - five rendering candidates that switch live;
    - matched captures beside the references, and measured costs.

    See [OrdinaryPlaceLookTest.md](Playtests/OrdinaryPlaceLookTest.md); the build is
    `Builds/WindowsOrdinaryPlace`. It waits for Luis's judgment of direction
    and candidates.
- **Overall plan:** [ShowcaseRoadmap.md](ShowcaseRoadmap.md).

## What the project is building now

A small-scale, high-fidelity **Worker Showcase** for outside playtesters:

- one worker, made in a video-game style character creator;
- a small landscape with one mineral boulder and a delivery point;
- physical mining and real hauling;
- models made to high quality in Blender.

The civilization/faction systems remain in the repository for the full game.
The showcase flow does not use them.

## Latest Game Director feedback (October 1, night)

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

1. **S1c, second pass: hand-painted surfaces.** Paint the Ordinary Place's
   models with procedurally painted textures from their Blender scripts, on
   top of look E ([plan](NextMilestonePlan.md)).
2. **S1d, the worker model**, in that language.

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
