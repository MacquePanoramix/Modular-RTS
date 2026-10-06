# Wonder Gather — Current State

**Updated:** October 6, 2026.

- **Where things stand (October 6, after Luis's message).** Luis restated
  what the prototype is for and asked for a review of the whole project
  ([message](Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md)).
  - **The review:** [Reviews/2026-10-06_SamePageReview.md](Reviews/2026-10-06_SamePageReview.md).
    The look is on Luis's page. The work on what the prototype is for (real
    weight, real strength, any rock, carrying, the creator) has not begun,
    and this week's mining is a drawn path with no weight in it
    ([a clip of it](Images/Miners/Work_Clip_Side.gif)).
  - **The proposal:** S3, weight and strength at the rock, comes next
    ([the plan](NextMilestonePlan.md#s3--weight-and-strength-at-the-rock-proposed-waiting-for-luis)),
    then carrying and equipment (S4), then the creator (S2).
  - **It waits for Luis:** choice A (how far the physics goes), B (the
    lantern and the mug), C (the order of stages).
  - **Nothing more is built until Luis answers.**
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
  - **Luis's verdict on that round (October 5):** "I really liked all the
    changes you did, except the shoulder strap."
    - **The strap** on Small's shoulder is put back as it was: out on the
      shoulder, soft on the coat, below the collar. Its ends stay on the
      bag's rings
      ([TheMiners.md](ArtDirection/TheMiners.md#after-luiss-look-october-5)).
    - **The method learned:** change only what a note or a failed check
      names.
    - **Next, on Luis's word to go on with the plan: the miners at work**
      ([NextMilestonePlan.md](NextMilestonePlan.md#s1d-last-part--the-miners-at-work-plan-waiting-for-luiss-choices)).
      - **Step 1, the sweep of extreme poses, is built.** Nothing fails in
        the poses the game uses. What fails beyond them (arms overhead, a
        deep bend in a coat, a deep bow) is logged with the movement that
        would need it
        ([TheMiners.md](ArtDirection/TheMiners.md#the-sweep-of-extreme-poses-october-5)).
      - **Step 2, hands that close, is built.** The free hands (Small's
        and Long's right, both of Round's) close round a handle of any
        thickness from 12 to 44 mm and open again, measured on the skin
        ([TheMiners.md](ArtDirection/TheMiners.md#hands-that-close-october-5)).
        Nothing shows in play until a tool is in the hand.
      - **A question for Luis came out of it:** in the game Long leans back
        11° compared with the model as built
        ([pictures](ArtDirection/TheMiners.md#for-luis-how-long-stands-in-the-game)).
        Not changed.
      - **Steps 3 and 4 are built (October 6): the miners mine.** Each has
        a pickaxe made for its arms and hands; the tool's solve reads the
        body (reach, a front to keep clear of, which hands are free); the
        swing goes over the shoulder; the mining is recorded and audited
        with the pickaxe in the hands
        ([TheMiners.md](ArtDirection/TheMiners.md#a-pickaxe-for-each-and-the-first-swings-october-6)).
        **To see it:** `K` in the Ordinary Place
        ([MinersPlaytest.md](Playtests/MinersPlaytest.md#a-first-look-at-the-work-october-6)).
      - **Step 5, the miners at the rock, is overtaken** by Luis's message
        of October 6: M1 is answered (any boulder, by a click), M2 to M4
        become choices of equipment, and the swing is to have real weight
        before it is taken to a rock.
  - **A method for model quality, and its first round (October 3 and 4).**
    Luis asked for a strategy, to refer to from now on, for making every
    model as polished as possible. It is
    [ModelQualityMethod.md](ArtDirection/ModelQualityMethod.md):
    - the standard (nothing floats, nothing clips, objects obey gravity);
    - four questions for every object;
    - seven passes, including automatic checks at rest and in the game's own
      movement;
    - a capture matrix of every angle and distance;
    - a ledger of every miss.

    **Its first round is done and waits for Luis's play**
    ([TheMiners.md](ArtDirection/TheMiners.md#the-methods-first-round-october-3-and-4);
    [MinersPlaytest.md](Playtests/MinersPlaytest.md#after-the-methods-first-round-october-4)):
    - **Luis's five notes on Small** are answered: the leg goes into the
      boot; laces lie on the leather; the strap's ends pass through rings on
      the bag; the hand closes round the lantern's handle; the bag hangs from
      its strap and rests on the hip.
    - **Every object is a proper object** on all three: held, hanging,
      swinging, stopped by the body. Long's pickaxe hangs in a sling; Round's
      hammer hangs in a loop on an apron that is tied on.
    - **Found by the method beyond the notes:** eighteen more, among them
      knees through the coats mid-stride, boots through each other in sharp
      turns, and a lantern that leaned even standing still.
    - **Left open, each with its reason:** four small audit checks (three of
      them single moments of the sharpest turn). The sweep of extreme poses
      was built on October 5.
  - **Polished after Luis's play (October 3, evening).**
    - **Small:** the lantern is carried in the hand and swings; the satchel
      hangs from its strap at the hip; legs line up with the boots; the neck
      rises into a closed collar.
    - **All three:** polished on a close audit in the game.
    - **Walks:** each body walks naturally at its own pace.
    - **Practices:** the character practices and the audit are in
      [CharacterPractices.md](ArtDirection/CharacterPractices.md).
  - **Rigged and in the game (October 3).**
    - **Walking.** Small, Long and Round walk the Ordinary Place on the
      procedural body, whose proportions now come from each model.
    - **The choice.** The player chooses one when the place loads (`M` to
      change).
    - **Light.** Three levels of detail and one texture each; 100 miners
      walking add about 2 ms a frame.

    See [MinersPlaytest.md](Playtests/MinersPlaytest.md).
  - **The miners: "almost perfect… already adorable"** (Luis, October 3).
    Polished on Luis's notes: the necks grow out of the clothes, the boots
    are single boots, Round's hands rest on the hips. Smaller flaws found on
    a careful look at every distance were also fixed
    ([TheMiners.md](ArtDirection/TheMiners.md#polish-after-luiss-word-october-3)).
    All three will be offered as choices in this prototype. They are Small,
    Long and Round, remade at much higher quality with miner's hints:
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
    Gather. It also reads what Luis's favourite frame teaches about Luis's taste:
    intimacy, the lit house at dusk, a soft brushed sky, enclosure and a tonal
    palette, rather than vastness and brightness.
- **Earlier steps:**
  - **S0 (the grounded body), S1a (two cameras) and S1b/S1c (the Ordinary
    Place, first pass)** are in `main`.
  - **The S1c second pass (hand-painted surfaces)** is on the branch. Luis
    found it "already quite beautiful", but not yet the emotion Luis seeks.
- **Overall plan:** [ShowcaseRoadmap.md](ShowcaseRoadmap.md).

## What the project is building now

A small-scale, high-fidelity **Worker Showcase** for outside playtesters:

- one worker, made in a video-game style character creator;
- a small landscape whose boulders can each be mined, by a click (October
  6), and a place to bring the stones;
- physical mining with real weight and real strength (October 6: "that's
  the whole point of it"), and real hauling;
- models made to high quality in Blender.

The civilization/faction systems remain in the repository for the full game.
The showcase flow does not use them.

## Latest Game Director direction (October 6)

Recorded verbatim in
[Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md](Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md),
with what was said plainly kept apart from what was said as a present
thought:

- **Any rock.** No special rock: the player clicks one of the map's big
  boulders and the miner mines it. Small in scale, final in kind.
- **Real weight and real strength, in this prototype.** The character
  really lifts the tool; bodies "react against real physics, with real
  strength in their limbs, and against objects with real weight". Like
  ragdolls, without being goofy.
- **The creator is part of the prototype;** the three miners are its
  appearances.
- **Equipment is a choice with consequences:** no pickaxe, a pickaxe, a
  strap for the back (asks strength of the back), a backpack, a dragged
  sack, a wheeled cart. The rocks are brought back.
- **Present thoughts, not decisions:** nothing held in the hands ("I'm not
  sure yet"); strength as one general number, maybe a slider; strength
  showing gently in the body.
- **The worry:** "I'm worried you lost a bit sight of what I actually want
  with this prototype."
- **Asked for:** a review of everything, research into how a pickaxe is
  really swung, and how to proceed. Written as
  [Reviews/2026-10-06_SamePageReview.md](Reviews/2026-10-06_SamePageReview.md).

## Game Director direction (October 3, late evening)

Recorded verbatim in
[Correspondence/2026-10-03_PHYSICAL_OBJECTS_AND_A_QUALITY_METHOD.md](Correspondence/2026-10-03_PHYSICAL_OBJECTS_AND_A_QUALITY_METHOD.md):

- **Small, still to fix:**
  - the leg still not lined up with the boot;
  - the laces' crosses floating off the boot;
  - the satchel's strap not joined to the bag;
  - the lantern floating up close;
  - the bag should hang with gravity.
- **The principle.** Physicality and the reality of all movement are among
  the game's main focuses. Anything that reads as an object behaves as a
  proper object that responds to its environment, never floating.
- **Liked:** how the coat's skirt, the lantern and the bag move with the
  walk.
- **First, a method.** A strategy document, to refer to from now on:
  - look at every object and place from all angles, exhaustively;
  - research how to reach the highest polish;
  - develop a method to raise model quality.

  Written as [ModelQualityMethod.md](ArtDirection/ModelQualityMethod.md).

## Game Director direction (October 3, evening)

Recorded verbatim in
[Correspondence/2026-10-03_SMALL_POLISH_AND_NATURAL_GAITS.md](Correspondence/2026-10-03_SMALL_POLISH_AND_NATURAL_GAITS.md):

- **Walks.** Each body walks in a way natural to its appearance; one shared
  speed is not needed.
- **Small, to fix:**
  - the lantern floating through the hand;
  - legs not lining up with the boots;
  - the neck;
  - the satchel clipping and not hanging from its strap.
- **Proactive polish** on all three, before Luis has to point out every
  detail. Luis will give notes on Long and Round later.
- **Research** good Blender practice.
- **Documentation.** Everything documented and pushed to GitHub is a top
  priority.

## Game Director direction (October 3)

Recorded verbatim in
[Correspondence/2026-10-03_THE_MINERS_FEEDBACK.md](Correspondence/2026-10-03_THE_MINERS_FEEDBACK.md):

- **The miners.** "Almost perfect now! Honestly they are already adorable!"
  Luis loves their designs.
- **To polish:**
  - necks that looked glued on up close;
  - the separated feet;
  - Round's hands;
  - anything else found on a careful look at every distance.
- **Then:**
  - rigging;
  - making them light enough to run well in the RTS.
- **All three in this prototype.** Luis decided to offer Small, Long and Round
  as choices.

## Game Director direction (October 2, late evening)

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
  of Luis's taste, but is not perfect yet. Luis would rather explore further
  than stop.
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
  I like the before better", with Luis's favourite frame from the hand-painted
  pass. The hand-painted pass stays the base. S1e's first iteration is
  archived, not adopted.
- **Luis's answers.**
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
  - Candidate E (painted light + paint filter + ink) is Luis's favourite, and is
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

**First (October 6):** Luis reads the review ([Reviews/2026-10-06_SamePageReview.md](Reviews/2026-10-06_SamePageReview.md))
and answers A, B and C. S3 begins on that word. The items below are still
open from before.

1. **Luis checks the dusk fixes** in the build:
   - fewer fireflies, still marked when far out;
   - the lamplight close up.
2. **Luis plays the polished miners** in the build
   ([MinersPlaytest.md](Playtests/MinersPlaytest.md)):
   - Small's fixes;
   - the natural walks;
   - notes on Long and Round.

   Then mining and hauling move onto the miners (the pickaxe sized to each
   body).
3. **Further explorations are additive and switchable**, and shown early as
   frames before being built out.

Luis's other choices on October 1:

- **Model first** (O4).
- **Strength** comes from the body plus training (O2). Overtaken on
  October 6 by Luis's present thought: one general number, maybe a slider.
- **Stage merges.** Each stage merges into `main` after Luis's playtest.

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
- Latest playtest guide: Docs/Playtests/MinersPlaytest.md
- Reviews of the whole project: Docs/Reviews/
- Technical contracts: Docs/Technical/EquipmentArchitecture.md and Docs/Technical/UnityProjectContext.md
- Design meaning and collaboration: Docs/DESIGN_RATIONALE.md and Docs/PROJECT_CULTURE.md
- Evidence: Docs/Validation.md
- Completed plans: Docs/Plans/
