# Implementation plan — S1 (revised): the Ordinary Place and two cameras

**Revised:** October 1, 2026, from Luis's Visual Soul handoff and his
two-camera direction
([correspondence](Correspondence/2026-10-01_VISUAL_SOUL_AND_TWO_CAMERAS.md),
[VisualSoul.md](VisualSoul.md)).
**Baseline:** the grounded body (S0) and the arrival fix. Unity 6000.6.0f1 /
URP (Forward+, HDR); Blender 4.4/4.5.
**Status:** In progress. The plan follows the handoff's own instructions
("start with one ordinary place…", "feasibility needs an in-engine test"). It
splits the work into checkpoints Luis playtests one at a time:

1. **S1a, the two cameras.** Built and tested on October 1, and waiting
   for Luis's playtest ([TwoCamerasPlaytest.md](TwoCamerasPlaytest.md)).
2. **S1b, the Ordinary Place.**
3. **S1c, choosing the rendering approach.**
4. **S1d, the worker model.**

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
- **Luis's choice.** Luis picks or mixes the rendering approach in his
  playtest. Until then it stays Open.

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
| C1 | Camera toggle key | `V`, plus the on-screen hint |
| C2 | Can you give orders while in Explore? | Yes. The game keeps running and orders work as in Strategy |
| C3 | Explore controls | Unity-style fly (right mouse + WASD, Q/E) plus Blender-style orbit (middle mouse) |
| C4 | Can the Explore camera enter buildings? | No, while buildings have no interior |
| V1 | Rendering approach | Open until Luis compares the candidates in S1c |
| V2 | Proportions, architecture, setting | Open, as the handoff states. S1b uses a modest cottage and grassland |
| V3 | Is the house the worker's home and delivery point in the showcase? | Possible. It would join the Visual Soul place to the worker loop. Not built until Luis decides |
