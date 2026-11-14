# (c)(c): the game's own falls and get-ups, the cut ring checked every frame

Plan, November 14, 2026 (evening). **The bars below were set before any of these courses ran.** From the
investigation's plan ([investigation.md](investigation.md), "Runs" and "Bars", with its changes 1, 4 and 7) and a
read-only map of the code (the scratchpad's `cc_map.md`, summarised here where it matters).

## The question

In the game's own flow (a shove, the fall, lying, getting up by its own body or the old way, standing): is the
trousers' cut top (the ring) seen, in which states and from which views; does "Skirt's flaps on their seams"
(`MinerBody.FlapsOnTheirSeams`, off by default) take it away there as it did on the posed grid; and does anything else
on the investigation's list show (the flap face, the drawn hip and knee, two bodies, balance's fall during a try)?

## The paired measure

The switch changes only what is drawn (`MinerBody.Drape`), never the physics, so both ways can be measured on the very
same frames. Each measured frame, after everything is drawn, the meter measures the ring as played (switch S); then it
lays the flaps the other way (not S) from that way's own state, measures again, and puts the played flaps back exactly.
Drape's only state carried from frame to frame is `flapHangs` (each flap's bone is rebuilt from its rest and that
direction every frame), so the other way keeps its own copy of it. A course then answers off against on with no spread
from one course to another (courses do not repeat; side by side, tick by tick, is how a difference is found).

**Its own check, before any result counts:** with the other way set equal to the played way (off and off, on and on),
the two measures agree on every frame to the point and the way, and the played flap bones come back bit for bit.

## The courses

All with the game's look as played (`-ownAsPlayed 1`) and one frame a physics step (`-ownRate 50`). A course is one
shove (`PushOver`, as `GettingUpByItsOwnTests`), then the whole getting up, to standing or 240 s, then taken back.

0. **The self-check:** Small, by its own body on, one course capped at 60 s, the two ways equal (off/off, then on/on).
   It also times a measure.
1. **Small, "Gets up by its own body" on:** 8 courses, shoved from the front, back, left and right, two each (the
   second turned 20 degrees). Played on, the other way off.
2. **Small, by its own body off (the old way):** 4 courses, one from each side.
3. **Round (the old way; only Small has ways up):** 4 courses, one from each side. Its trousers are cut too.

Measured every frame while down or getting up (not standing on its own two feet), every 10th frame standing.

## The ways it is looked at from

The grid's 25: 32, 47 and 62 degrees down at 8 yaws, and straight above, fixed to the world when the course begins;
the analysis sorts each way by the lying body's own axes (from its feet, its head, its sides). The game camera's views:
47 and 62 degrees and above. 32 degrees stands for the lower views (the cinematic camera's). Cover is what the grid
counts (every drawn garment, faces seen from behind looked through) and the ground (the terrain's layer); LOD0.

## The bars (a frame failing any is a fault)

1. **The cut ring** (Small, Round): 0 points seen from the game camera's ways (47, 62 degrees, above); and 0 from all
   25 ways (reported apart: from the feet at 32 degrees the skirt's opening shows the art's cut, known).
2. **The flap face:** each flap's stop face on its thigh's true front or back, every frame.
3. **The drawn hip:** flexion -20 to 120 degrees, abduction at most 45 (5 degrees' tolerance each); twist flagged past
   35, a fault past 45 (J4, decided November 14).
4. **The knee:** -145 to +3 degrees (5 degrees' tolerance); never the wrong way by more than 8, never past 150
   (FrameRecorder's).
5. **One body:** no frame with two bodies; the fall body never let go while the own body stands; through every own
   try, the posed body let go and balance not acting. Logged with every change of `fall.Now` and `balance.Fell`: the
   question is whether balance's fall fires during a try.
6. **FrameRecorder's:** no limb turning more than 30 degrees in a frame, no hand or foot jumping more than 6 cm, no
   link faster than 30 rad/s.
7. **Logged beside, not a bar:** Drape's ground (from the feet) against the terrain; it is wrong by design whenever a
   foot is raised.

## What is read from it

Per course and per state (lying, getting up, standing): the frames failing each bar, the switch off and on side by
side; the seen ring by way group; the frames where on shows more than off; the worst frames, pictured both ways (from
the feet and left, from the head and right, straight above, and the game camera at 47 degrees from the side), a few
per course.

## Not in this step

The joint anchor gaps, projection and blends (step (d)); Long; strengths other than as played; frame rates other than
50 (120 and the game's own come next); stumbles (the game has none to call; only shoves); the LOD the game camera draws
far off (LOD0 measured); grass (drawn, not counted as cover); the cinematic camera itself.

## How it is built

- (a) The grid's meter moved into a shared `CoverMeter` in the test assembly, unchanged: checked by running the grid's
  quick run both ways before and after the move (the same numbers). A reader of the drawn hip (the inverse of the
  hip's turn in `Kinematics`), checked on the grid's posed poses (read as set, within a degree).
- (b) `CoverRingLiveTests`, the per-frame log, a summary script, the self-check run.
- (c) An independent review of both; then the courses, each under the runner's time limit.
