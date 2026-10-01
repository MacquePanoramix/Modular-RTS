# S1a playtest — two camera systems

**Built:** October 1, 2026, on branch `claude/worker-showcase`.
**Plan:** [NextMilestonePlan.md](../NextMilestonePlan.md), S1a.
**Request:** Luis's October 1 message
([correspondence](../Correspondence/2026-10-01_VISUAL_SOUL_AND_TWO_CAMERAS.md)).

Every map now has two camera modes. `V` switches between them. A small hint at
the bottom of the screen names the mode and, for a few seconds after switching,
lists its controls.

## Strategy camera (playing fast)

| Input | Action |
|---|---|
| WASD / arrow keys | Pan. Faster than before, and faster still when zoomed out |
| Screen edge | Pan (full-screen only, so leaving a window never scrolls) |
| Middle-mouse drag | Grab the ground and drag it. It moves 1:1 with the cursor |
| Wheel | Zoom in toward the cursor, zoom out from the centre |
| Q / E, or Alt + middle-mouse drag | Rotate |
| F | Focus the selection |

Your zoom sensitivity (0.005) and every map's other camera preferences are
kept.

## Explore camera (wandering close)

| Input | Action |
|---|---|
| Hold right mouse + move | Look around, including straight up at the sky. The cursor hides while you look and comes back where it was |
| WASD | Fly along the view |
| Q / E | Down / up |
| Shift / Ctrl | Faster / slower |
| Wheel | Glide forward or back. With right mouse held, it changes the flight speed instead |
| Middle-mouse drag, or Alt + left drag | Orbit around what you look at, or around the followed worker |
| Shift + middle-mouse drag | Slide sideways and up/down |
| F | Glide to a close three-quarter view of the selected worker and follow it as it walks. Flying or looking ends the follow; orbiting keeps it |
| Left click / drag | Select, as in Strategy |
| Right click (without dragging) | Order, as in Strategy. A right-drag only looks |

**What makes it soft:**

- Movement eases in and out, and looking is lightly smoothed.
- Speed shrinks near surfaces and grows high up, so you can drift a few
  centimetres from grass or plaster and still cross the map quickly from above.

**What keeps it safe:**

- The camera is a 6 cm sphere with a 2 cm near clipping plane. It can come
  within a few centimetres of the floor, walls and props, but never passes
  into them. It slides along them instead of stopping dead.
- A worker who walks into the camera gently pushes it aside.
- The camera stays inside the map, under a soft ceiling.
- The game keeps running while you explore.

**The way back:** pressing `V` in Explore glides back to Strategy, centred on
the ground you were looking at and keeping your heading.

## What to try

1. In any map, play a little in Strategy. Does it now feel quick enough for
   playing? Try edge scrolling (full screen), the middle-mouse grab and
   zoom-to-cursor.
2. Press `V`, hold the right mouse button and look up. Fly down to the grass,
   then to a wall, and try to get as close as you like. Does it feel cozy and
   soft, or too slow or too floaty?
3. Select a worker, press `F` in Explore, and watch it work. Orbit around it
   with the middle mouse.
4. Right-click to give an order while exploring.
5. Press `V` again. Does the return feel natural?

## Defaults to confirm or change (C1–C4)

| ID | Question | Default |
|---|---|---|
| C1 | Toggle key | `V` |
| C2 | Orders while exploring | Allowed |
| C3 | Explore controls | Unity-style fly plus Blender-style orbit |
| C4 | Entering buildings | No, while buildings have no interior |

## Evidence and limits

**Tested:** nine PlayMode tests in `CameraTests` (measured values are in
[Validation.md](../Validation.md)):

- the floor, a building wall and the stones can't be entered, but can be
  approached to about 6 cm;
- the camera slides 3.3 m along a wall;
- it looks straight up;
- the toggle doesn't jump, and the return re-centres;
- a walking worker stays within 3.5° of the centre;
- a worker's nudge moves the camera out to 28 cm;
- zoom-to-cursor and the middle-mouse grab hold their ground point exactly.

The full suite passes twice.

**Not tested by automation:**

- **The feel.** Speeds, softness and smoothing are first guesses for your
  playtest.
- **The right-click/right-drag split and cursor capture.** They depend on a
  real mouse.
- **Edge scrolling.** It only works full screen.
- **Mouse-wheel scale.** The build records the first wheel delta in its player
  log, in case wheel speed feels wrong.
