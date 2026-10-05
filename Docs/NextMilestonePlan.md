# Implementation plan — S1 (revised): the Ordinary Place and two cameras

**Revised:** October 1, 2026, from Luis's Visual Soul handoff and
two-camera direction
([correspondence](Correspondence/2026-10-01_VISUAL_SOUL_AND_TWO_CAMERAS.md),
[VisualSoul.md](ArtDirection/VisualSoul.md)).
**Baseline:** the grounded body (S0) and the arrival fix. Unity 6000.6.0f1 /
URP (Forward+, HDR); Blender 4.4/4.5.
**Status:** In progress. The plan follows the handoff's own instructions
("start with one ordinary place…", "feasibility needs an in-engine test"). It
splits the work into checkpoints Luis playtests one at a time:

1. **S1a, the two cameras.** Built and tested on October 1, and waiting
   for Luis's playtest ([TwoCamerasPlaytest.md](Playtests/TwoCamerasPlaytest.md)).
2. **S1b, the Ordinary Place.** First in-engine pass built on October 1
   ([OrdinaryPlaceLookTest.md](Playtests/OrdinaryPlaceLookTest.md)).
3. **S1c, choosing the rendering approach.** Five candidates can be switched
   live in the build, with matched captures and measured costs. Luis chose E
   (painted light + paint filter + ink) as the working base and agreed that
   hand-painted textures come next. That is **S1c, second pass** (below). It was
   built on October 2. Luis found it "already quite beautiful" but not yet the
   emotion Luis seeks.
4. **S1e, the essence beyond the surface.** Luis asked for the breathtaking,
   out-of-this-world emotion, every frame a painting, and a style unique to
   Wonder Gather. The first iteration was built on October 2. Luis preferred
   the hand-painted pass, so it was archived on `claude/essence-exploration`
   and not adopted ([TheEssencePlaytest.md](Playtests/TheEssencePlaytest.md)).
5. **S1c, third pass: dusk details.** These are small switchable steps over
   the hand-painted look, judged on Luis's favourite frame: fireflies (on),
   the hearth's flicker and the window glow. Built on October 2. The
   hand-painted pass itself was merged into `main` (7f7fcc0).
6. **S1d, the worker model**, in the hand-painted language. Three miners,
   rigged and walking, taken through the model quality method; Luis liked the
   round on October 5 ([TheMiners.md](ArtDirection/TheMiners.md)).
7. **S1d, last part: the miners at work.** Planned on October 5
   ([below](#s1d-last-part--the-miners-at-work-plan-waiting-for-luiss-choices));
   its first step, the sweep of extreme poses, is built. Steps 2 to 4 do not
   depend on Luis's choices; step 5 needs M1 to M4.

The previous S1 plan (style studies, then the worker model) is archived in
[Plans/S1_WorkerModelAndStyleStudies.md](Plans/S1_WorkerModelAndStyleStudies.md).
Its model work continues as S1d.

## The question

Can Wonder Gather's visual soul (lighting that leads the emotion, surfaces that
feel alive, characters with a soul, playful wonder) be rendered in this Unity
project, in motion, by day and by night, at an acceptable runtime cost? And
can the player both command the world quickly and wander through it up close?

## S1a — Two camera systems

Luis wants an "Age of Empires-like best RTS possible camera" for playing, and
an Explore camera for wonder: free, like the Blender/Unity viewport, "cozy to
navigate and soft". It must not go through the floor or buildings, yet can get
"very near" them and look up at the sky.

The toggle and both modes go into the shared camera rig, so every existing map
gets them.

### Toggle

- **Key.** `V` switches modes. An on-screen hint shows the current mode and
  its controls.
- **Into Explore.** Explore starts exactly where the RTS view is, so nothing
  jumps.
- **Back to Strategy.** Strategy re-centers smoothly on the ground the Explore
  camera was looking at, keeping its heading.

### Strategy mode (RTS)

- **Pan.** WASD/arrow keys pan faster and more crisply than today: speed
  scales with height, and starts and stops are short.
- **Edge scrolling.** The screen scrolls at the screen edge, in the focused
  game window only.
- **Grab-pan.** Middle-mouse drag grabs the ground and moves it 1:1.
- **Zoom.** The wheel zooms in toward the cursor and out from the center. The
  view flattens when close and steepens when high.
- **Rotation.** Q/E rotate, as today. Alt + middle-mouse drag also rotates.
- **Focus.** F focuses the selection, as today.
- **Limits.** Map bounds and terrain-aware height, as today.
- **Existing preferences.** Per-map preferences such as the 0.005 zoom
  sensitivity are kept.

### Explore mode (free POV)

| Input | Action |
|---|---|
| Hold right mouse + move | Look around, including straight up at the sky. The cursor hides while looking |
| WASD | Fly along the view |
| Q / E | Down / up |
| Shift / Ctrl | Faster / slower |
| Wheel | Glide forward/back. With right mouse held, it changes the flight speed |
| Middle-mouse drag (or Alt + left drag) | Orbit around the point being looked at, or around the followed worker |
| Shift + middle-mouse drag | Slide sideways and up/down |
| F | Glide to a close, three-quarter view of the selection, then follow it as it walks. Moving or looking ends the follow |
| Left click / drag | Select, as in Strategy |
| Right click (without dragging) | Command, as in Strategy. A right-drag only looks |

**Softness:**

- Movement eases in and out.
- Speed scales with distance to the nearest surface: slow and precise near
  grass and walls, quick high up.
- Looking is lightly smoothed.

**Safeties:**

- The camera is a small sphere (about 6 cm) with a very near clipping plane. It
  can get within a few centimeters of the ground, walls and props, but never
  passes into them. It slides along them instead of stopping dead.
- Workers gently push the camera aside when they walk into it.
- A soft ceiling and the map's bounds keep the player in the world.

Buildings stay solid while they have no interior. A decorated interior may
later be made enterable (Possible).

**The game keeps running.** Explore is a viewpoint, not a pause, and orders
still work.

### Evidence for S1a

PlayMode tests:

- the ground, a wall and a building can't be penetrated, but can be
  approached to within about 15 cm;
- sliding along a wall;
- looking at the sky;
- the toggle has no jump, and the view re-centers on the way back;
- a followed worker stays framed;
- zoom keeps the point under the cursor fixed;
- a right-drag in Explore issues no command, and a right-click does.

Also: the full regression suite twice, a Windows build, and a playtest guide.

## S1b — The Ordinary Place (in-engine look test)

The handoff asks for "a small house, grassland, path and worker" in the
existing prototype, at night and by day. It's a new scene, *The Ordinary Place*,
playable with both cameras:

- **Land.** Gently rolling grassland with a worn dirt path, a few stones and
  layered distant hills for aerial perspective (A, E).
- **The house.**
  - Modeled in Blender from a reproducible script: plaster walls, timber
    frame, shingled roof, chimney, deep-set windows, an open door, a step,
    flower pots, a bench, firewood and a fence.
  - Warm window and door lights spill onto the wall, step, path and grass (A).
  - It has no interior, so it stays solid to the Explore camera.
- **Grass.**
  - Dense, wind-moved blades running from a dark blue-green root to warm
    tips, with taller seed-head grass that catches window light at night
    (A, D).
  - It must stay readable from strategic height and lush up close.
- **Sky and time of day.**
  - A painted sky: soft-edged clouds, a lilac and apricot dusk (E), and stars
    and a moon at night (A, F).
  - A time-of-day control moves through day, golden hour, dusk and night. It
    drives sun, moon, sky, ambient light, fog and the house lights together.
- **The worker.**
  - Today's procedural body walks the path and around the house under player
    orders, so moving characters and changing light can be judged.
  - The real character model is S1d. A placeholder scarf or coat may be added
    to test how cloth reads, without committing to a costume.

## S1c — Rendering candidates, compared in engine

The handoff leaves the rendering technique open and asks for real captures and
a measured cost. The same scene can switch live, with a key, between
candidates that share the same models:

1. **Painted light.**
   - A custom lit shader with a soft, brush-broken light/shadow edge.
   - Cool colored shadows from the sky instead of black.
   - Painterly hue and value variation per material.
   - Warm local lights, rim light, aerial fog, bloom and color grading.
2. **Painted light + paint filter.** Candidate 1 plus a screen-space paint
   filter that turns fine detail into brush-like patches.
3. **Painted light + ink.** Candidate 1 plus loose, varying contour lines,
   mainly on characters and silhouettes (B, E).

The candidates can be combined. A subtle paper or brush grain is evaluated
with each.

### Evidence for S1c

- **Matched captures.** Several angles, near and far, by day, at dusk and at
  night, set side by side with the references.
- **A short moving sequence** where possible.
- **Frame time** at 1920×1080, with representative grass and light density.
- **Luis's choice.** Luis picks or mixes the rendering approach in the
  playtest. Until then it stays Open.

## S1c, second pass — hand-painted surfaces

Luis's verdict on the first pass: "going really well into the Visual Soul",
but not there yet. He prefers E, the most illustrated candidate. The biggest
remaining gap was painterly surface. In the references the brushwork belongs
to the objects ("Brush marks, uneven plaster and flowing grass belong to the
objects", A). Screen filters only paint over the whole image.

**The approach.** Each Ordinary Place model gets a painted colour texture,
made reproducibly by its Blender script:

1. **Unwrap.** Every mesh is UV-unwrapped.
2. **Bake.** Cycles bakes surface information into the textures:
   - ambient occlusion for cavities;
   - edges and curvature;
   - upward-facing areas.
3. **Paint.** A painter's language is applied in shader nodes and baked into
   the colour texture:
   - broad warm-to-cool value shifts;
   - lit, worn edges;
   - cool coloured cavities;
   - directional brush strokes that follow the material (plaster daubs,
     wood grain along beams, shingle-by-shingle variation, stone outlines);
   - moss on upward stone and roof;
   - stains low on the walls.
4. **Import.** The models are exported with UVs, and the textures are
   imported into `Assets/_WonderGather/Art/OrdinaryPlace/Textures`.
5. **Shade.** The painted shader samples them. The in-shader dabs relax where
   a real painted texture exists, and the brush-broken light edge stays.

These textures are painted by script, not by hand. A human painter could
later paint over them in Blender, because the UVs and source files are kept.

**Base look:** E (painted light + paint filter + ink). The candidates stay
switchable for comparison.

**Evidence:**

- matched captures before and after, beside references A–F;
- frame cost in the benchmark;
- the full suite;
- a build.

## S1e — The essence beyond the surface

Luis on October 2: the hand-painted pass is "already quite beautiful", but Luis
wants "the breath-taking out of this world emotion", the stylistic essence
rather than the surface. Luis welcomes a long checkpoint and research. Two
follow-ups the same day raised the bar:
- every frame the camera lands on should be a painting, driven by light, while
  the RTS still runs well;
- the Visual Soul is the soul, and the result must go above and beyond it, to
  something unique to Wonder Gather
  ([correspondence](Correspondence/2026-10-02_THE_ESSENCE_BEYOND_THE_SURFACE.md)).

**The study.** [TheEssence.md](ArtDirection/TheEssence.md) reads why Luis's
references move Luis and measures them against our captures. It then proposes
Wonder Gather's own language: a thesis, seven signature devices and a frame
test.

**Outcome of the first iteration:** not adopted. Luis likes "the before"
better, and Luis's favourite frame shows why: the lit house at dusk, intimate,
enclosed and tonal. Any further iteration starts from the hand-painted pass,
adds options beside it instead of replacing it, and is shown early as frames.
The iterations below were the plan before the verdict.

**Iterations:**

1. **The beyond and the painting.** Built October 2 (archived):
   - **The beyond:**
     - the far world on a bluff above a valley lake;
     - monumental Blender-modelled clouds;
     - planar water reflections;
     - aerial perspective in the sky's colour;
     - cloud shadows;
     - the luminous palette.
   - **Look F:**
     - the painting pass (strokes along the forms);
     - ink on characters only;
     - the hour's palette;
     - seeds and fireflies.
   - **Viewpoint keys.**
2. **Forms that read as paint.** Painted tree clumps, painted far land and
   mountains, brushed cloud edges, and strokes that breathe with the wind.
3. **The meadow and the warm light.** Gusts, flowers, a stream, and the glow
   and motes of lamplight.
4. **The signature decisions** that need Luis: the curving world and cosmic
   night, and giant flowers and reeds.

**Constraints:**
- **The playable meadow and its navigation are untouched.** The far world is
  only scenery.
- **Frame cost** is measured in a release build at every iteration. The RTS
  must keep headroom for units.
- **References.** Luis's film references are described but never committed.

## S1d — The worker model (former S1 scope)

This step follows Luis's S1c choice, so the model is made for the chosen look:

- **The model.** A Blender worker in the Visual Soul language: distinctive
  silhouette and proportions, strong hair and coat shapes, and a simple drawn
  face. Proportions stay Open, so the body is built to vary in S2.
- **Rig and dimensions.**
  - A skeleton matching the procedural rig, with a rig adapter driving its
    bones.
  - Body dimensions read from the model, replacing the 2.2 m test biped.
  - Navigation and camera framing follow the body.
- **A better pickaxe**, whose grip and head points match its tool data.
- **Tests** for gait, support and grip on the model rig.

## S1d, last part — the miners at work (plan, waiting for Luis's choices)

**Written:** October 5, 2026, after Luis said to go on with the plan
([correspondence](Correspondence/2026-10-05_THE_SHOULDER_STRAP.md)).

**Why it needs a word from Luis first.** Every stage begins with a plan and
Luis's approval ([ShowcaseRoadmap.md](ShowcaseRoadmap.md#working-agreement-for-this-roadmap)).
The step also changes what the miners carry and do, which Luis has not yet
decided (M1 to M4 below). What does not depend on those choices is done
first.

**The goal.** The miners do what the 2.2 m test body does today: walk to the
rock and strike it with a pickaxe. The pickaxe is made for their size, and
their hands hold what they hold. This closes S1d's own list: "a better
pickaxe whose grip and head points match its tool data" and "tests for gait,
support and grip on the model rig".

**Not in this step:** the effort-driven swing (S3), strength and hauling
(S4), and the creator (S2). The swing keeps today's shape.

### Steps

| Step | What | Depends on Luis's choices? |
|---|---|---|
| 1 | **The sweep of extreme poses** (the method's pass 4): arms raised, elbows and knees bent deep, the waist bowed and twisted. Fix what it finds at the joints work will use | No. **Built on October 5** ([TheMiners.md](ArtDirection/TheMiners.md#the-sweep-of-extreme-poses-october-5)). Nothing fails in the poses today's swing uses, so nothing was changed. What fails beyond them is logged for the movements that will need it (an overhead swing in S3; a kneel) |
| 2 | **Hands that close.** A hand closes round a handle of a given thickness, and opens again. The audit checks it: fingers meet the handle and do not enter it | No |
| 3 | **A pickaxe made for each body.** One modelled pickaxe (the one Long carries), sized to the body. Its grips and its striking head are read from the model, so the tool's data and its shape cannot drift apart | No |
| 4 | **The tool's solve reads the body.** Today it assumes the test body's arms and height. It will take reach and hand places from each body, and work with one hand or two | No |
| 5 | **At the rock.** The miners walk to a mineral boulder and strike it, with today's rule that only a real contact of the pick's head counts | M1, M2, M3, M4 |
| 6 | **Evidence.** Tests on each miner; the mining recorded for the audit in motion; work views added to the capture; a build; a playtest guide | — |

### Choices for Luis

Each has a recommendation, marked as a default. Nothing here is Locked.

| ID | Question | Options | Recommended default |
|---|---|---|---|
| **M1** | Where do the miners mine? | (a) A mineral boulder a little way down the path in the Ordinary Place, out of Luis's favourite frame. (b) A small new place built from the same land, sky and light. (c) The grey equipment test map | (a). It keeps them in the place Luis likes, and leaves the house alone (V3 stays open) |
| **M2** | Where is a miner's pickaxe when it is not in use? | (a) On the back in a sling, as Long's is. Small and Round would each gain a strap and a pickaxe on the back. (b) Carried in the right hand. (c) It waits at the rock, leaning on it, and the miner takes it up there | (c) for Small and Round, so their looks do not change without Luis's word; Long uses the one already on the back |
| **M3** | What does a miner do with the lantern or the mug while working? | (a) Keeps holding it and swings a light pick with one hand. (b) Sets it down beside the rock, and picks it up after. Small's lantern would light the work. (c) Hangs it on the belt or the bag | (b). It is the most physical, and needs a small "put down, pick up" movement |
| **M4** | What happens to what is mined, until hauling exists (S4)? | (a) Pieces fall and lie in a heap by the rock; nothing is carried yet. (b) Today's stand-in: a bundle carried in both hands to a delivery place | (a). It matches the roadmap's O3 (loose pieces), and does not invent a carry that S4 will replace |

### Contracts to keep

- **The procedural body.** Planted feet, arrival without shuffling, grips at
  actual grips, navigation owning the root, no active ragdoll.
- **The contact gate.** Only the tool's solved head touching the mineable
  surface yields material; reach is judged before contact, never stretched
  to fit.
- **The test body and its maps** keep working as they do. The body-aware
  solve gives the same result for the test body as today's constants.
- **Change only what is asked.** The miners' looks change only where a
  choice above says so.
- **The method.** Every new pose and object goes through the model quality
  method before Luis sees it.

## Contracts to keep

- **The procedural body.** All S0 contracts hold: planted feet, arrival
  without shuffling, grips at actual grips, navigation owning the root, and no
  active ragdoll.
- **Shared camera rig.** Existing maps keep working, and their per-map camera
  preferences keep their values.
- **Protected files.** Luis's uncommitted files (TheGroup.unity, two
  ProjectSettings files, `_Recovery`) are never edited, committed or
  discarded.
- **Third-party material.** Anything downloaded or reused needs Luis's
  approval and a recorded license. Everything in S1 is planned to be made from
  scratch: Blender scripts, Unity shaders and generated textures.

## Decisions for Luis

These have defaults so the work can proceed. Each can be changed at the
S1a/S1b playtests.

| ID | Question | Default used |
|---|---|---|
| C1–C4 | Camera toggle key, orders while exploring, controls, entering buildings | **Accepted Oct 1** ("I really liked the camera") |
| V1 | Rendering approach | **Working base, Oct 1:** E, plus hand-painted textures (second pass). **Oct 2:** look F (the painting pass, ink on characters only, the hour's palette) tried in S1e; Luis preferred the hand-painted pass in E. Not Locked |
| V2 | Proportions, architecture, setting | Open, as the handoff states. S1b uses a modest cottage and grassland |
| V3 | Is the house the worker's home and delivery point in the showcase? | Possible. It would join the Visual Soul place to the worker loop. Not built until Luis decides |
