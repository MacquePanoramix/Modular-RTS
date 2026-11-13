# The catalogue of unnatural movement

November 13, 2026. Written by the sceptic and writer of this research, from three researchers' files in this folder
(`U1_perception.md`, `U2_craft_measures_biomechanics.md`, `U3_project_record.md`) and the check log in
`sceptic_notes.md`. For the whole project: walking, turning, standing at ease, catching a shove, falling, getting up,
settling, working with a tool, carrying, and any body proportion a player makes.

Nothing in this page is Game Director acceptance. No bar here has been run on the game yet. Passing every bar would
not mean a movement is natural; it would mean the catalogue found nothing. Luis's eye decides.

---

## 1. For Luis, first

**What it is.** A list of every kind of movement we know can look wrong in a body like the miners', gathered from
studies of how people see motion, from animators' craft, from biomechanics, and from every fault you have flagged and
every fault our own tests and judges have found. Each kind has a name, a short description of how it looks, why it
reads wrong, a way to measure it on every frame of the game, and a limit (a "bar") with where the limit comes from:
a study, you, or our own judgment.

**Its families.** Eleven: the feet and hands against the ground and the world; balance; timing, speed and effort;
smoothness (jumps, flicks, pops); joints (bent or twisted as no person can); coordination (what moves first, what
follows); weight, cause and reaction (things that move with nothing pushing them, or do not move when pushed); life
(too still, too regular, too even, repeated); held and hung things and loose clothing; body size; and the screen
itself (what is drawn against what the physics did, the frame rate, the camera).

**How it will be used.** One scan, run on every bench and every clip: the walk, the turns, the catch, the idle, the
fall, the getting up, and later the work. It reads every frame, the drawn body at a fixed 100 frames a second and the
physics at its own 50, and again at the game's own frame rate, from several cameras. The bars are written down before a run and never moved after seeing the
results. What numbers can catch, numbers catch; what they cannot (grace, intent, how alive it feels) goes to judges as
pictures. Any fault you see that the catalogue does not have is added to it, with your words.

**Questions that are yours to answer** (questions, not decisions; nothing below decides them):
1. **Size and speed.** Physics offers two ways a bigger body is slower: like a pendulum (a body twice as tall is about
   1.4 times slower), or slower still, keeping the same speed of the hands and feet (twice as tall, twice as slow).
   Real animals lie between. Which should the miners, and a player's own body, follow?
2. **The camera.** How far away and from what angle is a miner usually seen in play? Small errors (a foot sliding a
   centimetre, a gap at a hand) matter close up and vanish far away; timing errors do not shrink with distance. Which
   views should count as the main ones for the scan?
3. **How long a get-up may take.** You found it "so long to get up" (October 30). Is there a length of time, for a
   fresh and for a tired miner, that feels right to you, so that a bar can be set from it?
4. **More than the push, or exactly the push.** Viewers forgive a reaction that is a little bigger than the push
   deserves more readily than one that is smaller. Should the miners stay strictly true to the physics, or is a
   slightly larger answer welcome?
5. **True to people, or a little stylised.** The miners look stylised. Should every bar be a real person's, or may some
   (quickness, the size of the breath, arm swing) lean toward the look of the game?

---

## 2. How to use it

1. **Every bench and every clip runs the scan.** The scan is one program that takes a recorded course (every frame:
   drawn bones, physics bodies, contacts with impulses, centre of mass, support, events) and reports each kind's count
   of flagged frames, its worst value, and the frame numbers. Benches: the walk and turns (`MotionTraceBench`,
   `TurnBench`), the idle (a minute of life), the shove and catch, the fall and the get-up (`OwnRiseBench`), the
   arrival, the hung things, and later work, carrying and creator bodies. All three miners (Small, Long, Round), a
   whole work-tire-rest cycle, several strengths (memory "Measure all three miners"); and the flows as played with the
   defaults on (memory "Test what Luis plays"), naming the switch in the report's first line.
2. **Bars are set before a run and never moved after seeing results.** A bar that turns out wrong is changed only by a
   written reason that does not use the run's own numbers (a new study, Luis's word, or a judge's finding on pictures),
   and the run is repeated. The check log (section 7) and the bar's origin stay beside it.
3. **Three clocks.** The drawn body at a fixed 100 frames a second (the rate `MotionTraceBench` records and the rate
   the project's per-frame bars were set at, `-traceFrames 100`); the physics bodies and contacts at every physics
   step (50 a second, fixed timestep 0.02 s); and the game's own rendered frame rate, plus a sweep of frame caps (30,
   60, 120, uncapped) for the screen family. The October 30 faults were seen only at 120 frames a second; clips at
   25-30 hid the October 8 hip drop (U3 F14, F24). Per-frame bars given "at 100 fps" below are converted as stated in
   section 3.0, with the spike test there at rates below 100.
4. **Several cameras.** The player's camera, a side view, a front view, a top view, and one low close view of the
   feet. Front views hide leg errors and side views show them (Kuhlmann, de Lussanet, Lappe 2009 [WE]); camera motion
   hides foot slips (U1 S02 [U]). A fault counts if any camera would show it.
5. **Numbers for what numbers catch; pictures for the rest.** Flagged frames, with one second either side, go to judge
   agents as sheets of frames from several cameras, and to Luis when he asks. Judges also get unflagged stretches, so
   that what the numbers miss can still be seen (section 4). What is asked of judges gets gamed; they read pictures,
   not our verdicts (memory "Search the poses").
6. **A fault Luis sees that the catalogue lacks is added**, with his words, a new id, a measure and a bar, before the
   next run. Section 5 maps every fault he has named so far.
7. **No kind is a reason to enlarge what Luis already likes.** The idle and the gentle breath are the reference; the
   "too still" kinds are floors to catch a dead body, never targets (memories "Breath gentle and scaled", "Luis loves
   the idle").

---

## 3. The catalogue

### 3.0 Conventions for every measure

- **Data per frame.** Drawn bones: the rendered Transforms after interpolation, as the player sees them. Physics
  bodies: each rigid or articulation body's pose, velocity and angular velocity at the physics step. Contacts: each
  contact point with its normal impulse and tangential impulse (force = impulse / step). `c`: whole-body centre of mass
  (mass-weighted mean of the bodies, plus any held load where stated). `v_c`, `a_c` its velocity and acceleration.
  `BoS`: base of support, the convex hull of the active ground contacts, projected on the ground. `L`: leg length
  (hip joint to floor, standing). `H`: standing height. `M`: body mass. `g` = 9.81 m/s2. `dt`: the frame's own time
  (it varies at the game's own rate; use it, never a nominal one).
- **States.** A foot is **loaded** when its contacts carry more than 20% of body weight; **planted** when it is in
  contact on this frame and the last; a **stance phase** runs from the frame a foot is planted to the frame it lifts.
  A part is in **free flight** when no contact touches it or anything joined to it. An **event** is any logged cause:
  a shove (frame of first impulse), an order, a landing, a tool hit, a grip, a fall start, a rise start, a settle, a
  weight shift.
- **Body time.** `T0 = sqrt(L / g)`. Durations "in body time" are seconds divided by `T0`. For a 0.9 m human leg,
  `T0` = 0.30 s.
- **Per-frame bars and frame rate.** Where the project set a displacement bar per frame at 100 fps (for example 4 mm
  of second difference), the underlying limit is an acceleration: `a = d / (0.01 s)^2` (4 mm becomes 40 m/s2). At
  another rate the bar is `d' = a x dt'^2`. **Caution (review):** that conversion loosens the bar fast at low rates
  (4 mm at 100 fps is 16 mm at 50 and 36 mm at 30), so a real one-frame teleport of 2-3 cm would pass at 30 fps. So
  the position-jump kinds (S1, C5) are run on the drawn body recorded at 100 fps; at any lower rate they add a
  **spike test**: a frame's second difference more than 3 times both neighbouring frames' and over 2 mm is flagged
  whatever the converted bar says (judgment). Angle-per-rendered-frame bars (flicks, jumps) are about what the eye
  sees in one frame and stay per rendered frame, measured in the parent bone's frame (not in world) and only flagged
  when the change also exceeds that task's T8 speed cap times `dt`, so a normal fast swing at 30 fps is not a fault;
  their speed equivalent is noted where it matters.
- **Kinematic walk.** The walk (`ProceduralBiped`) has no contacts and no forces. There a foot is **planted** when the
  walk's own flag says so (`plantedL/R` in the trace) and its sole is within 5 mm of the ground; **loaded** is the
  same as planted; the BoS is the hull of the planted soles; the centre of mass needs a segment mass table per body
  (an anthropometric table scaled to the body, judgment), since the drawn bones carry no masses. Force measures (B4
  share, B5, B6, W forces) do not exist there and are not run.
- **Missing data (review).** The existing trace (`MotionTraceBench.cs:92`) has positions of root, hips, head, feet,
  knees, hands, shoulders and tool, a few yaws and pitches, the planted flags and the swing phase. It has no per-bone
  rotations beyond those, no elbows, no contacts, no centre of mass, no forces and no event log. `FrameRecorder.cs`
  (in `D:/Dev/WG` only) adds drawn bones, solved segments and physics parts with pose and world angular velocity, but
  still no contacts, centre of mass or forces. Before the scan can run, a recorder must add: contacts per body from
  the collision callbacks (point, normal, impulse; the code already reads `point.impulse` in `GroundTouch.cs:30`),
  every body's mass and centre of mass, each drive's `driveForce` (`OwnBody.cs:1097` reads it), and a log of the
  controllers' own mode changes (catch started, settle started, rise phase), which several kinds need (T2, B2, S8).
- **Marks.** **[WE]** well established (several studies agree). **[1S]** one study or one lab. **[U]** unclear
  (secondary, conflicting, not re-checked, or our inference). Source names are short here; full entries with links are
  in section 8.
- **Origin of a bar.** **study** (a number from a study, named), **physics** (a law; exact), **Luis** (his words or
  rule, dated), **project** (a bar the project already set in code or a page; an earlier judgment), **judgment** (ours,
  today; a starting value to be confirmed on pictures and by Luis). Judgment bars are set strict on purpose.
- **Detectors.** File and line in this working copy (branch `claude/worker-showcase`, November 13) with the test
  name, which does not move. U3's line numbers were read in `D:/Dev/WG` and differ (check log K32). "None" means no
  detector yet. **Where the benches run (review, checked read-only):** in `D:/Dev/WG` (branch `claude/wip-round`,
  957a21b) every `OwnBodyTests.cs` line is 141 higher (:93 is :234, :141 :282, :236 :377, :263 :404, :299 :440,
  :417 :558, :501 :642, :527 :668, :764 :908); `PhysicalFallTests`, `MinerTests`, `GroundedBodyTests` and
  `LivingBodyTests` have the same lines in both; `HungThingTests.cs:254`, `:327` and `:397` exist only in this copy.
  `FrameRecorder.cs`, `ArticulationFactsTests.cs` and `Docs/Technical/AnyBody/tools/` (`shoves.sh`) exist only in
  `D:/Dev/WG`; `motion_jolts.py`, `motion_dips.py`, `motion_frames.py` and the judges' `alive.md` exist only here.
  `TurnBench` exists in no repository copy: it is a reviewed draft in a session scratchpad, its bars F1-F6 written in
  `Docs/Handoff.md:526-530`.
- **Two earlier research files** are cited by short names: the **turning file** is
  `Docs/Research/2026-11-13_TurningAndSettling/research.md` (its numbers were checked there); the **twitches file** is
  `Docs/Research/research_notes/Getting up for any body/twitches_and_broken_limbs.md`.

### 3.1 Family C: contact with the ground and the world

**C1. A planted foot slides.**
*Looks:* the foot skates on the ground while it bears weight; the body seems to drift on ice.
*Why:* viewers saw foot slides smaller than 21 mm in most conditions, and preferred any cleanup to none (Prazak,
Hoyet, O'Sullivan 2011 [1S, abstract]). "Even small amounts of footskate can destroy the realism" (Johansen 2009, in
the turning file).
*Measure:* (corrected in review) per stance phase of each foot, the net horizontal drift, from the stance's first
frame, of the foot's own material points under contact (the sole point under each contact, moved with the foot's
pose, i.e. its slip velocity `v + w x r`), not of the force-weighted contact centroid: that centroid is the foot's
centre of pressure, which rolls heel to toe in every normal step and would flag every step. Net drift, not summed
path, so contact jitter over a long idle stance does not add up. Per frame, the horizontal slip speed of those points
while loaded, whether the foot is flat or on heel or ball. In the kinematic walk: the drawn sole's lowest points while
planted (3.0).
*Bar:* drift over 1 cm in a stance phase flagged; over 2 cm a fail (judgment, from the study: the 21 mm figure is an
upper figure that slides below it were still seen, so 2 cm is not safe). Per frame: over 1 mm a frame at 100 fps
(0.1 m/s) while loaded (project, `motion_breaks.py:26`). Note: `motion_breaks.py` checks the slide only while the
foot is flat (pitch under 1.5 degrees), so a sole sliding on its heel or ball is not caught there today. Research benchmarks use laxer values (0.2 m/s at toe height
under 5 cm, Kimodo; 2.5 cm a frame at under 5 cm, GMD): comparison only, not bars.
*Applies:* walk, turns, standing at ease, catch steps, settling, work stances, carrying.
*Detector:* `Art/Review/motion_breaks.py:26` (SLIDE); `TurnBench` F5 (2 cm per stance; see section 6, it should be
1 cm); `LivingBodyTests.FlatOrdersAlternateSupportAndStopWithoutIdleFootDrift` (`LivingBodyTests.cs:66`).
*Eye:* the low feet camera; does the sole stay on its spot while the body passes over it?

**C2. A planted foot spins.**
*Looks:* a loaded, flat foot turns on the spot like a turntable.
*Why:* real feet pivot only on the heel or the ball, mostly while unloading; in turns the swing foot is placed
already turned (Courtine and Schieppati 2003, turning file [1S, checked there]). Same footskate evidence as C1.
*Measure:* per frame, yaw change of a planted, loaded foot whose contacts cover more than half of the sole's length;
per stance phase, its total yaw.
*Bar:* over 1 degree a frame at 100 fps (project, `motion_breaks.py:28`); over 10 degrees in one stance while flat
and loaded (judgment). A pivot on heel or ball alone is allowed up to 25 degrees per stance (project, `TurnBench` F5).
*Applies:* turns, standing turns, settling, catch steps, tool stances.
*Detector:* `motion_breaks.py:28` (PIVOT); `TurnBench` F5.
*Eye:* in a turn, is the foot lifted and set down turned, or screwed round on the ground?

**C3. Moonwalking: steps that do not match the travel.**
*Looks:* the legs cycle faster or slower than the body moves over the ground.
*Why:* craft name for a clip played at the wrong ground speed (game practice [U]); in a body driven by its own feet
it shows as C1, in the kinematic walk it can come from the root moving apart from the feet.
*Measure:* per stride, centre of mass travel divided by the sum of the two step lengths (heel to heel).
*Bar:* outside 0.95 to 1.05 over a stride on level ground at steady speed (judgment).
*Applies:* the kinematic walk, starts and stops, turns.
*Detector:* none.
*Eye:* side camera, a whole stride: does the body travel exactly what the feet step?

**C4. A foot floats above or sinks into the ground.**
*Looks:* a planted foot hovers; a sole cuts into the ground.
*Why:* the gap between a foot and the ground is seen when it is visible (O'Sullivan et al. 2003, for gaps at contact
[1S]); research uses 5 mm as the tolerance for floating and penetration (PhysDiff convention, in the twitches file
[WE as convention]); the project's model method fails floating parts over 3 mm.
*Measure:* per frame, for each planted foot, the lowest point of the drawn sole minus the ground height under it.
*Bar:* outside plus or minus 5 mm on a planted foot (study convention); the drawn mesh above 3 mm for more than 2
frames (project, `ModelQualityMethod.md`). The physics body into the ground over 3 cm in a fall: fail (project).
*Applies:* everything with feet on the ground; slopes and ramps; lying (the whole body against the ground).
*Detector:* `PhysicalFallTests.AMinerLetGoFallsLiesAndGetsUp` (`PhysicalFallTests.cs:85`, into ground under 3 cm);
`LivingBodyTests.BothPacesClimbAndDescendWithFeetOnTheRamp` (`LivingBodyTests.cs:119`).
*Eye:* low camera along the ground, and the shadow under the foot.

**C5. A foot or the hips snap down at a landing.**
*Looks:* a small "teleport" to the ground at each step (Luis, October 8, L05).
*Why:* a one-frame change of motion with no visible cause is read as a jolt (section 3.4); the hips were dropping
47-128 mm in one frame at every landing (U3 F01).
*Measure:* in the frame a foot lands, its movement beyond what it was already doing (second difference); for hips and
head, the second difference of position in every frame.
*Bar:* landing 6 mm at 100 fps (600 m/s2 equivalent at other rates) (project, `motion_breaks.py:27`); hips and head
see S1.
*Applies:* walk, catch steps, settling steps, get-up when a foot is placed.
*Detector:* `motion_breaks.py:27` (LANDING), `:24` (BREAK).
*Eye:* the player's camera at full rate: does anything jump as each foot lands?

**C6. A foot that does not roll.**
*Looks:* the foot lands flat and leaves flat, like a stamp; no heel strike, no roll onto the toes.
*Why:* a normal walk rolls from heel to toe; viewers rated a strongly asymmetric roll-over shape as only slightly
impaired (Handzic and Reed 2015 [1S]); the absence of roll is a craft fault (practice [U]).
*Measure:* per stance phase, foot pitch at contact, at mid-stance and at lift; contact centroid travel from heel to
toe along the sole.
*Bar:* at a walk above 0.8 m/s, the foot pitches less than 5 degrees at both contact and lift, or the contact centroid
moves less than a third of the sole's length (judgment).
*Applies:* walk, turns at pace; not standing, not shuffling settling steps.
*Detector:* none.
*Eye:* side camera, feet close up.

**C7. A body or a thing in the world: passing into ground, walls, rock.**
*Looks:* a hand inside the rock, a knee into the ground, a tool through the wall.
*Why:* seeing a hand pass into an object is felt far more than seeing it stopped short (Burns et al.; Prachyabrued and
Borst 2016 [1S each, own hands in VR]); for a watched character no study was found [U].
*Measure:* per physics step, the penetration depth of every body and held collider into world colliders
(ComputePenetration); per rendered frame, the drawn mesh's deepest vertex inside world geometry.
*Bar:* drawn depth over 5 mm for more than 2 frames, or any depth over 2 cm (judgment).
*Applies:* falls, lying, getting up, work at the rock, carrying near walls; other miners' bodies count as world (two
miners passing into each other, review).
*Detector:* `PhysicalFallTests.cs:85` (ground only).
*Eye:* close camera at the contact; from the side and from above.

**C8. Contact without force, or force without contact.**
*Looks:* a hand rests on a handle it does not touch; a thing moves as if touched when nothing reached it.
*Why:* cause must be visible and at the place of contact (Michotte's launching effect, section 3.7 [WE]).
*Measure:* for every contact the logic claims (a planted foot, a grip, a hand on the ground in a rise), a physics
contact with normal force above zero on that frame; for every body whose velocity changes beyond gravity and its
joints (W11), a contact on that frame.
*Bar:* zero frames (physics).
*Applies:* getting up (hands and knees), grips, tool hits, shoves.
*Detector:* none.
*Eye:* does each hand that seems to push have something under it?

**C9. A gap or a late touch at a hit.**
*Looks:* a tool stops a centimetre short of the rock and the rock reacts; a hand pushes without touching.
*Why:* gaps at a collision were accepted when the view hid them, much less when they were visible (O'Sullivan et al.
2003 [1S]).
*Measure:* at the frame an object's response starts, the smallest distance between the drawn striking surface and
the drawn struck surface.
*Bar:* over 5 mm visible from the player's camera (judgment).
*Applies:* tool work, shoves between miners, picking up, hanging back.
*Detector:* none.
*Eye:* the frame of impact, close.
Review: also for H8, at the frame of a tool's impact the feet should be planted and braced (no foot in the air, no
C1 slide in the 0.2 s around it), judgment.

**C10. Self-penetration: limbs through the body.**
*Looks:* an arm through the chest, a thigh through the other, a hand through the head.
*Why:* craft fault [WE in craft]; generation research counts it as a separate failure (HumanScore 2026 [1S]).
*Measure:* per physics step, penetration depth between non-adjacent colliders (thigh-thigh, shin-shin,
forearm-trunk, hand-head); per frame, drawn mesh overlap of non-adjacent parts.
*Bar:* any depth over 1 cm, or drawn overlap visible for more than 2 frames (judgment); the legs through each other
in a turn, zero (project, `TurnBench` F4).
*Applies:* turns, crossover catch steps, getting up, kneeling, lying.
*Detector:* `TurnBench` F4.
*Eye:* front and top cameras in turns and crossovers.

**C11. The swinging foot cuts through the standing leg.**
*Looks:* in a sharp turn the swing foot flies straight between footprints, through the other shin.
*Why:* real feet go round the standing leg on an arc (Johansen 2009, section 7.3.4, turning file [1S, checked there]).
*Measure:* during each swing, the smallest distance between the two shins and between the boots.
*Bar:* boots overlapping beyond 4 mm, or shins touching: fail (project).
*Applies:* turns, crossover catches, settling steps.
*Detector:* `MinerTests.InASharpTurnTheSwingingFootGoesRoundTheStandingOne` (`MinerTests.cs:208`).
*Eye:* top camera, sharp turns, all three miners (Round fails first in turns, U3 L06).

**C12. Steps on a tightrope, or a straddle (added in review).**
*Looks:* the feet land one in front of the other on a line, or wide apart like a sailor's, in a plain straight walk.
*Why:* people choose a step width of about 0.13 of leg length, close to their foot width (0.11 L) and to the width
that costs least energy (0.12 L) (Donelan, Kram, Kuo 2001 [1S, abstract checked]); a tightrope or a straddle is a
craft fault (practice [U]).
*Measure:* per step on a straight level walk at steady speed, the sideways distance between the two heels at
successive landings, divided by `L`.
*Bar:* outside 0.05 L to 0.25 L over 5 consecutive steps (judgment around the study's 0.13 L). Not in turns,
crossover catches or settling steps.
*Applies:* the walk; any body proportion (scaled by its own `L`).
*Detector:* none; computable from the trace's foot positions (`MotionTraceBench.cs:92`).
*Eye:* front and top cameras on a straight walk.

**C13. The swing foot marches or scuffs (added in review).**
*Looks:* the swinging foot is lifted high like a march, or drags its toe along the ground.
*Why:* the toe clears level ground by only about 1-2 cm at mid-swing in healthy adults; reported values range about
0.9-3.5 cm with method (Winter's 1.29 cm as cited; reviews of minimum toe clearance [U: values differ by method,
no single agreed mean]).
*Measure:* per swing, the minimum height of the drawn toe above the ground during mid-swing (between 25% and 75% of
the swing), and the maximum height of the foot over the swing, in body height `H`.
*Bar:* minimum toe height under 3 mm (a scuff: the toe touching or cutting the ground, also C4) or over 6 cm at a
normal walk on level ground (a march) (judgment, strict either side of the study range). Steps over a stone or up a
step are exempt.
*Applies:* the walk, turns, catch steps (a catch step may lift higher: flag only, not fail).
*Detector:* none; computable from the trace's foot heights.
*Eye:* side camera, the feet, a whole stride.

### 3.2 Family B: balance and support

**B1. Standing where it cannot stand (a hidden force holds it).**
*Looks:* a body leans past its toes or heels and stays there, neither falling nor stepping.
*Why:* a body can stop without a new support only while its extrapolated centre of mass stays inside the base of
support (Hof, Gazendam, Sinke 2005 [WE]); a short time outside can be recovered by swinging the trunk or arms (same
work; Curtze, Buurke, McCrum 2024 caution [1S]).
*Measure:* per frame, the extrapolated centre of mass `XcoM = c + v_c / sqrt(g / l)`, with `l` the height of the
centre of mass above the ground (judgment; U2's "1.2 x leg length" not confirmed); the margin `b` = the smallest
distance from the XcoM's ground projection to the BoS edge, negative outside.
*Bar:* `b` below zero for more than 0.5 s with no step started and no trunk or arm counter-rotation over 30 degrees a
second (judgment from the study).
*Applies:* standing, settling, work stances, carrying, getting up (on hands and knees use all contacts as BoS).
*Detector:* none.
*Eye:* side and front cameras: would a person standing like that fall?

**B2. A step from nowhere.**
*Looks:* the body steps when nothing pushed it and nothing asked it to; or shuffles its feet after arriving (L01).
*Why:* a change of motion with no visible cause is read as the body's own intention, and an intention with no purpose
reads as odd (Tremoulet and Feldman 2000; Pratt et al. 2010 [WE as direction]).
*Measure:* for every foot lift, the margin `b` over the half second before, and the events in that time.
*Bar:* a lift with `b` above 2 cm throughout the half second before and no event (order, shove, settle, weight
shift, work) in the 2 s before (judgment). After arrival: at most one closing step, then still (project).
Review: an "event" that exempts a step must be an outside cause (an order, a shove, a landing, a tool hit) or a
scheduled shift or settle whose schedule is logged before the step; a step the controller labels a shift after the
fact does not exempt itself, and judges see every exempted step, so a buggy step called a shift cannot pass unseen.
*Applies:* standing at ease, arrival, settling, work.
*Detector:* `GroundedBodyTests.ArrivalFinishesTheStrideWithoutShuffling` (`GroundedBodyTests.cs:142`);
`OwnBodyTests.AtEaseItTakesNoStep` (`OwnBodyTests.cs:501`).
*Eye:* does each step have a reason one could name?

**B3. A catch that does not catch.**
*Looks:* the body steps after a shove, the foot lands, and it still topples or slides on; or the step lands where it
cannot help.
*Why:* a catching step must bring the base of support under the extrapolated centre of mass (Hof 2005 [WE]).
*Measure:* from the landing of each catch step, `b` against the new BoS.
*Bar:* `b` still below zero 0.3 s after the landing with no second step started; or a fall after a step taken
(judgment). The test's own bar: it catches itself with a step (project).
*Applies:* shoves, trips, being pulled.
*Detector:* `OwnBodyTests.ShovedItCatchesItselfWithAStep` (`OwnBodyTests.cs:527`);
`OwnBodyTests.NudgedItKeepsItsFeet` (`:141`, does not go down; back within 3 cm).
*Eye:* side camera: does the step land where the body is going?

**B4. Lifting a foot that still bears weight; stepping with no weight shift first.**
*Looks:* a foot leaves the ground while the body leans on it; the step starts with no lean toward the other foot.
*Why:* before a step, the centre of pressure first moves toward the leg to be unloaded and then to the stance leg,
about 237-254 ms and 4-5 cm before lift (Laudani 2021, turning file [1S, checked there]); a foot lifted under load
is a craft fault (practice [U]).
*Measure:* at the lift frame of each foot, that foot's share of total vertical contact force; over the 0.5 s before,
the path of the centre of pressure.
*Bar:* over 20% share at lift (judgment); no centre of pressure movement toward the stance foot in the 0.1 s before a
planned (not a catch) step (judgment from the study's 237 ms).
*Applies:* planned steps (walk start, settling, standing turns, weight shifts); not catch steps.
*Detector:* none.
*Eye:* front camera: does the body lean onto the other foot before the step?

**B5. The body is held up by something that is not its feet (Newton's residual).**
*Looks:* often invisible alone; shows as floating, a too-easy rise, or a body that will not fall.
*Why:* the net ground force must equal `M (a_c - g)`; anything else is a hidden force (physics); learned controllers
use such "residual forces" knowingly (Yuan and Kitani 2020 [1S]); Luis wants help only as the absolute last resort
(October 22, L19).
*Measure:* per physics step, `| M a_c - M g - sum of contact forces |` divided by body weight, over 3-step windows;
also a log of every non-joint, non-contact force applied by code (root forces, keepers, helpers).
*Bar:* residual over 5% of body weight over 3 steps (judgment; numerical noise to be measured first); any code force
in the game's own path: zero (Luis, October 22, "absolute last resort").
*Applies:* everything with the physics body: standing, catching, falling, getting up, settling.
*Detector:* none for the residual; `PhysicalFallTests.LaidDownAnyWayItGetsUpByItsOwnStrength` (`:158`, "no old way").
*Eye:* does the rise look lifted from above?

**B6. Centre of pressure where feet cannot put it.**
*Looks:* invisible alone; a body balancing on a foot edge as if glued.
*Why:* feet push, never pull, and only within friction (physics).
*Measure:* per step, centre of pressure (force-weighted contact point) against the BoS; any contact normal force below
zero; tangential force over 0.8 x normal force without slip (judgment for the friction figure).
*Bar:* centre of pressure outside BoS by over 1 cm, any pulling contact (physics).
*Applies:* the physics body everywhere; the kinematic walk does not have forces (use B1 instead).
*Detector:* none.
*Eye:* none; numbers only.

**B7. A wobbling stance.**
*Looks:* standing at ease, the body sways visibly more than a person does, or rocks at a quick beat.
*Why:* quiet stance centre of pressure moves at about 9.2 mm/s in young adults (Posturographic standards, J Hum Kinet
[1S]); 90-95% of sway power lies below 2 Hz, main frequency 0.30-0.45 Hz (Soames and Atha 1982 [1S]).
*Measure:* over 10 s windows at ease with no logged weight shift, settle or order inside them (a shift is a planned
movement and would inflate the speed, review), centre of mass sway speed (primary) and centre of pressure mean speed
from contacts; the power spectrum of both. The centre of pressure from PhysX contacts is noisy and quantised to a few
contact points per foot: before it is used, its noise is calibrated on a body held still by a fixed joint, and only
the excess over that floor counts (review). In the kinematic walk use the drawn hips and head instead.
*Bar:* centre of pressure speed over 30 mm/s (about three times the human mean), or more than 20% of power above
2 Hz (judgment from the studies). Also the project's: wander under 2 mm (`OwnBodyTests.cs:93`), does not wobble or
fall in 60 s (`:299`).
*Applies:* standing at ease, holding a tool still, waiting.
*Detector:* `OwnBodyTests.EachMinerStandsByItsOwnJointsStill` (`OwnBodyTests.cs:93`);
`OwnBodyTests.StandingAMinuteItIsAliveAndDoesNotWobble` (`:299`).
*Eye:* a minute at ease from the side: calm, or restless?

**B8. Not settling after a catch.**
*Looks:* after a shove the body catches itself and stays as the catch left it, feet wide or crossed, for good
(Luis, November 13, L23).
*Why:* people readjust after recovering and return toward their preferred stance, about 0.17-0.19 m between heels
(McIlroy and Maki 1997; Daffin 2024, turning file [1S each, checked there]); how long they take is mostly unmeasured
(turning file 2d).
*Measure:* from the catch's landing, stance width, foot angle and weight share every frame; the time until they
are back within the body's own preferred range.
*Bar:* still outside its preferred stance (width outside plus or minus 30% of the preferred, scaled to the body) 6 s
after a catch with no settling step (judgment; Luis's approved design of November 13 asks for settling; the 6 s
figure is from a source not opened, [U]).
*Applies:* after catches, after arrivals, after work.
*Detector:* none (researched in `2026-11-13_TurningAndSettling/`, not built).
*Eye:* does it look comfortable five seconds after the shove?

### 3.3 Family T: timing, speed and effort

**T1. A movement too quick or too slow for what it is.**
*Looks:* a turn, a kneel or a get-up that seems played at the wrong speed.
*Why:* for most film scenes speed changes up to about 2x pass, but for human bodies and falling objects the tolerated
range is clearly narrower (Kobayashi 2019 [1S, n=10, no number for bodies]); cadence off by 25% passed and by 50% did
not (Handzic and Reed 2015 [1S]).
*Measure:* duration of each action in body time (`T0`), against a reference from people for the same action: 90
degree walking turn 1.46 s; standing turn 90 / 135 / 180 degrees 0.95 / 1.91 / 2.31 s as fast as safe, 1.7-3.1 s for
180 at slow to fast pace (Madrid 2024; Khobkhun 2021, 2022, turning file [1S each, checked there]).
*Bar:* outside plus or minus 25% of the reference in body time (study for the 25%, Handzic; the transfer from
cadence to whole actions is judgment). For the get-up no reference is set yet (question 3 for Luis).
*Applies:* turns, kneel, reach, get-up, settling, tool swings.
*Detector:* none.
*Eye:* does it look like a person's tempo, for that size of body?

**T2. An answer sooner than a body could give it, or none in time.**
*Looks:* the body braces or steps before the push could have been felt; or stands passive too long, then acts.
*Why:* the passive give is at once (physics); automatic postural responses start about 80-140 ms after a push, catch
steps from about 160 ms and on average about 250 ms (McIlroy and Maki 1993 and review [WE, secondary]); viewers
accepted push reactions more than 150 ms early or late less than half the time, early and late alike (Hoyet,
McDonnell, O'Sullivan 2012 [1S, abstract]).
*Measure:* from each shove's first impulse frame: the first change of drive targets beyond the standing control
(first purposeful response) and the first purposeful foot lift; whether the XcoM left the BoS. "Beyond the standing
control" and "purposeful" are only codable if the controller logs its own mode changes (catch started, step
planned) with the frame (review); a foot lifted passively by a hard shove (no drive change, no mode change) is the
passive give and is not counted as an early lift.
*Bar:* a purposeful response before 80 ms, or a purposeful catch-step lift before 150 ms: flagged (study); no
response by 300 ms to a push that took the XcoM out of the BoS: flagged (judgment).
*Applies:* shoves, pulls, trips, being struck, a slip at work.
*Detector:* none (the shove tests check outcome, not timing).
*Eye:* frame by frame at 50 fps from the push: does the body first give, then answer?

**T3. The effect late after a visible cause.**
*Looks:* the pick hits and the rock reacts a moment later; the foot lands and the body takes its weight late.
*Why:* the impression that one thing caused the other holds for delays up to about 50 ms and sometimes 100 ms, and is
gone by about 100-200 ms depending on method (Michotte 1946/1963; Gruber, Fink, Damm 1957; Scholl and Nakayama
[WE]). A figure of about 60 ms accepted on average at a collision (O'Sullivan et al. 2003) could not be re-checked
here [U].
*Measure:* the frames between a contact's first impulse and the first visible response of the other body (speed or
shape change over a noise floor).
*Bar:* over 40 ms (two physics steps) (judgment, from the safe end of the study range).
*Applies:* tool hits, shoves between miners, landing loads, picking up, a hung thing knocked.
*Detector:* none.
*Eye:* the frame of contact and the two after it.

**T4. No slow-in and slow-out (robotic speed).**
*Looks:* a part starts at full speed and stops dead, like a machine.
*Why:* human point-to-point movement has a bell-shaped speed, close to minimum jerk (Flash and Hogan 1985 [WE]);
minimum jerk has peak/mean speed 1.875 (arithmetic, check log K21); the human figure of about 1.75 was not re-checked
[U]; viewers slightly preferred a little more roughness than minimum jerk (Hirose et al. 2020 [1S]), so perfect
smoothness is not the target.
*Measure:* for each discrete movement of a hand, foot, head or tool tip (from rest to rest), its peak speed over its
mean speed, and the time of peak speed as a share of the movement.
*Bar:* peak/mean below 1.3 (near constant speed), or peak speed in the first or last 15% of the movement (judgment).
*Applies:* reaches, placing a foot, kneel, getting up phases, tool swings, hanging a thing back.
*Detector:* none.
*Eye:* does each move ease in and out?

**T5. Constant speed through a bend.**
*Looks:* a hand, foot or tool tip goes round a curve as fast as along the straight; it looks driven.
*Why:* in human movement speed falls where the path bends (the two-thirds power law), and viewers see motion as even
only when it obeys the law (Viviani and Stucchi 1992 [WE]).
*Measure:* over each curved stroke (radius of curvature varying by at least three times), the slope of log speed
against log radius of curvature.
*Bar:* slope below 0.1 (judgment; the law gives about 0.33).
*Applies:* hand reaches round obstacles, tool swings, swing foot paths in turns, the centre of mass path in turns.
*Detector:* none.
*Eye:* does the part slow where its path turns?

**T6. No braking into a turn.**
*Looks:* the walker turns a corner at full pace and full step length.
*Why:* people slow into turns by shortening steps, the inside leg most: 1.13 / 0.99 / 0.88 m/s for 90 / 180 / 360
degree turns against 1.26-1.39 straight (Madrid 2024, turning file [1S, checked there]).
*Measure:* centre of mass speed and step length per leg through each turn, against the straight walk before it.
*Bar:* speed falls by less than 5% in a turn of 90 degrees or more, or the inside step is not shorter than the
outside (judgment from the study's 12%).
*Applies:* the walk's turns.
*Detector:* none.
*Eye:* top camera: does the body slow and shorten its inside step?

**T7. Turning faster than a body turns.**
*Looks:* the body spins round in a blink while the feet catch up.
*Why:* in an about-face from standing done fast (1.5 s), the head, the quickest part, peaks at about 270 degrees a
second; the pelvis is slower (Khobkhun 2021 [1S, checked in the turning file]); the walk turns the body at up to 420
degrees a second (U3 L24).
*Measure:* per frame, pelvis yaw speed and head yaw speed in world.
*Bar:* pelvis over 270 degrees a second, or head over 300 (judgment from the study; scale by `sqrt(0.9 m / L)`, the
adult leg of 3.0, until Luis answers question 1; corrected in review from `sqrt(1 m / L)`).
*Applies:* turns walking and standing, catch steps that turn, settling.
*Detector:* partial: `Art/Review/motion_breaks.py` already reports, for root, hips, chest and head, the fastest yaw
speed and the times each has turned 10% and 90% of any turn over 60 degrees (`turn_began`) (review; the catalogue
had "none"). `TurnBench` measures hip twist, not yaw speed.
*Eye:* top camera; does the body wait for the feet?

**T8. A joint faster than a person's.**
*Looks:* a limb whips round; a boot flies.
*Why:* walking joints peak near 5-6 rad/s (knee; old small source [U]); the fastest joint rotation recorded in people
is shoulder internal rotation in elite pitching, about 7,500 degrees a second (about 130 rad/s) (Fleisig review, via
search [U]); boots at 115-154 rad/s in a get-up (U3 F16) are beyond anything a person does there. U3's "a person is
rarely above 5 rad/s" is struck as a general bar: it is a walking figure (check log K31).
*Measure:* per physics step and per rendered frame, each body's angular speed relative to its parent and in world.
*Bar:* per task (judgment, to be confirmed on people's motion capture before the first run): walking, standing,
settling, turning over 10 rad/s; catching, kneeling, getting up over 15 rad/s; hand and tool in a swing over 30 rad/s;
anything over 30 rad/s outside a swing or the two steps after an impact (project, `FrameRecorder` whip bar, in
`D:/Dev/WG`). Scaled by `sqrt(0.9 m / L)` until question 1 is answered (corrected in review).
Review: the physics bodies are clamped at `maxAngularVelocity` 20 rad/s (`OwnBody.cs:535`, `PhysicalFall.cs:340`;
the hands at 60), so the 30 rad/s whip bar can never fire on them and a fault there is hidden by the clamp, not
absent. Add: **frames at the clamp**, any physics body whose angular speed is within 2% of its clamp for more than
one step is flagged (a hidden limit is acting; physics). The drawn bones are not clamped and keep the 30 rad/s bar.
*Applies:* everything.
*Detector:* `FrameRecorder.cs:191` (whip over 30 rad/s in world; in `D:/Dev/WG`, not in this working copy);
`OwnBodyTests.ShovedAgainAndAgainNothingOfItFlies` (`OwnBodyTests.cs:764`, fastest part in m/s).
*Eye:* sheets at full rate around each flagged frame.

**T9. Effort that does not match the result.**
*Looks:* a big wind-up and a feeble result; a heavy pick swung with no effort; a long strain that moves nothing.
*Why:* viewers accept a result larger than the effort shown far more readily than a smaller one (Vicovaro et al. 2014
[1S, direction re-checked; its percentages not re-checked, U]; O'Sullivan et al. 2003 [1S]); effort is read from the
motion and weight from the object's size, and a mismatch lowers naturalness (Yamac, O'Sullivan, Neff 2024 [1S]).
*Measure:* per action, the work done by the joints (each drive's real `driveForce` times its joint turn per step, as
`ArticulationFactsTests.cs:360` in `D:/Dev/WG` already computes; no estimate needed) against the
change in energy of the body and what it moves; per strike, the tool head's momentum before impact against the
struck thing's response.
*Bar:* a result made with joint work under 50% of the energy gained (help or engine energy, see W3), or joint work
over three times the energy gained for more than 2 s with nothing moving (a stalled body fighting itself, U3 L10)
(judgment).
*Applies:* tool work, lifting, carrying, getting up, pushing.
*Detector:* none.
*Eye:* does the effort shown match what happens?

**T10. A load lifted at the speed of a light one.**
*Looks:* a heavy sack or the pick raised as quickly as a mug.
*Why:* viewers judge a lifted weight from the lift's motion (Runeson and Frykholm 1981 [WE]), mostly from how long the
lift takes (Hamilton et al. 2005 [1S]).
*Measure:* lift duration (from grip to the load's top) per load, against load mass over the body's strength.
*Bar:* loads differing more than two times in mass over strength lifted within 10% of the same duration (judgment).
*Applies:* picking up, carrying, tool pick-up, hanging back.
*Detector:* none.
*Eye:* side by side, the light thing and the heavy one.

**T11. Full speed in one step, or a dead stop (added in review).**
*Looks:* the walker is at full pace on its first step, or halts from full pace in one step, as if switched on or off.
*Why:* people starting from standing reach steady walking speed in about three steps (several small studies agree on
about three, varying with footwear and slope; secondary summaries [U, not re-checked in the papers]); a stop from
pace takes a braking step or two (practice [U]).
*Measure:* from the first lift of a start, centre of mass speed (drawn hips in the kinematic walk) at the end of each
of the first three steps as a share of the steady speed; for a stop, the steps from the stop order (or the decision
to stop) to speed under 0.1 m/s.
*Bar:* over 90% of steady speed by the end of the first step, or a stop from over 1 m/s in under one step (judgment
from the studies' three steps).
*Applies:* walk starts and stops, arrivals, orders to go.
*Detector:* none; computable from the trace.
*Eye:* side camera, the start and the stop.

### 3.4 Family S: smoothness and continuity

**S1. A one-frame jump of a part.**
*Looks:* a stutter, a little teleport (Luis, October 8, L05; the legs "teleport" in turns, L06).
*Why:* a jump no cause explains reads as a glitch; the project found the hips dropping 47-128 mm in one frame and a
foot jumping 31-40 cm (U3 F01, F02).
*Measure:* per rendered frame, the second difference of each drawn bone's position: `| p(t+1) - 2 p(t) + p(t-1) |`.
*Bar:* hips and head 4 mm at 100 fps (40 m/s2); a foot in the air 12 mm at 100 fps (120 m/s2) (project,
`motion_breaks.py:24-25`), on the drawn body recorded at 100 fps; at lower rates the converted bar plus the spike test
of 3.0 (review). Impacts exempt for the frame of contact only.
*Applies:* everything.
*Detector:* `Art/Review/motion_breaks.py:24-25` (BREAK, FOOT_BREAK), fed by `MotionTraceBench.cs`;
`Art/Review/motion_jolts.py` (this copy only; hands too); `FrameRecorder.cs` "jump", a drawn hand or foot moving over
6 cm in a frame (`D:/Dev/WG` only, about `:168`) (review).
*Eye:* the flagged frame and its neighbours, at full rate.

**S2. A flick: out and back.**
*Looks:* a limb twitches out "for one millisecond" and returns (Luis, October 30, L20; "some weird twitches", L24).
*Why:* a change with no cause and no purpose reads as a twitch (Tremoulet and Feldman 2000 [WE as direction]).
*Measure:* per rendered frame, each drawn bone's rotation change; a flick is one frame over a bar followed within 2
frames by a change back.
*Bar:* over 6 degrees in a frame and back over 4 within 2 frames (project, `TurnBench` F2, set before its first run).
Per rendered frame on purpose; at 120 fps the 6 degrees is 12.6 rad/s, at 50 fps 5.2 rad/s. Review: measured in the
parent bone's frame, and the frame of an impact is exempt for the struck tool and the hands holding it (a real
rebound is out and back); otherwise a tool's rebound at a hit would always fail.
*Applies:* everything.
*Detector:* `TurnBench` F2 (drafted).
*Eye:* sheets of every frame around the flag.

**S3. A bone turning too far in one frame.**
*Looks:* a limb snaps to a new angle; drawn limbs "turned half round" (U3 F14: 1,036 times in 18 minutes, up to 176
degrees).
*Why:* as S1 and S2; over 90 degrees in a frame is a drawing fault by construction (twitches file).
*Measure:* per rendered frame, each drawn bone's rotation change (the shortest angle between the two rotations).
*Bar:* over 15 degrees in one frame (project, `TurnBench` F3), measured in the parent bone's frame and flagged only
when it also exceeds the task's T8 cap times `dt` (review: at 30 fps a normal 10 rad/s tool swing moves 19 degrees a
frame and would otherwise fail); the impact frame is exempt for the struck tool and the hands; over 90 degrees always
a fault, no exemption (judgment from the twitches file).
*Applies:* everything.
*Detector:* `TurnBench` F3 (a draft, 3.0); `FrameRecorder.cs:162` "spin", a drawn limb over 30 degrees in a frame
(`D:/Dev/WG` only, review); `TurnsAsSolved` mended the half-round turns (U3 F14).
*Eye:* as S2.

**S4. Shaking or chatter.**
*Looks:* a joint buzzes or chatters, often at a limit (U3 F17); "convulsing" on the ground (Luis, October 13, L16).
*Why:* plain noise on joints reads as shaking (Gielniak, Liu, Thomaz 2011 [1S]).
*Measure:* per joint over 1 s windows, the number of reversals of angular velocity with an amplitude over 0.5 degrees,
and the share of motion power above 6 Hz.
*Bar:* over 8 reversals a second, or over 10% of the joint's motion power above 6 Hz, outside impacts (judgment).
*Applies:* standing, lying, getting up, holding, joints near limits.
*Detector:* none.
*Eye:* close camera on hands and feet at rest.

**S5. Jerk spikes.**
*Looks:* a sudden lurch within otherwise smooth motion.
*Why:* jerk is motor research's measure of smoothness; spectral arc length is the valid one (Balasubramanian et al.
2015, twitches file [WE]); generation research flags "kinematic extremes" as velocity spikes (HumanScore 2026 [1S]).
*Measure:* per joint, angular acceleration per frame; a spike is a value over 5 times the joint's median absolute
deviation over the surrounding 2 s; spectral arc length of hand, foot and head speed over 2-4 s windows.
*Bar:* any spike outside an impact frame (judgment); spectral arc length against people's for the same movement, to
set on motion capture before the first run [U].
*Applies:* everything.
*Detector:* none.
*Eye:* as S2.

**S6. A reversal with no slowing.**
*Looks:* a backswing that turns round instantly at the top; a sway that bounces off its end.
*Why:* massive parts must slow before reversing (physics); craft calls it a missing ease (U2 C12 [WE]).
*Measure:* at each reversal of a part's velocity along its main axis, the speed in the frame before it against that
movement's peak.
*Bar:* over 20% of the movement's peak speed in the frame before reversal (judgment).
*Applies:* tool swings, sway, arm swing, settling.
*Detector:* none.
*Eye:* the top of the swing, frame by frame.

**S7. Snapping into a pose.**
*Looks:* the body hurries into a held pose and stops dead, "a tidy held pose" (U3 F12, the snap onto hands and knees).
*Why:* a pose coming on over a few frames is a posed body ("it can't be posed", Luis, October 9).
*Measure:* whole-body joint speed (root-mean-square) falling from over 50% of the action's peak to under 5% in under
0.1 s with no new contact.
*Bar:* any such frame (judgment, under Luis's rule).
*Applies:* getting up, kneeling, catches, settling.
*Detector:* none.
*Eye:* does it arrive, or snap?

**S8. A pop at a hand-over between modes.**
*Looks:* a jump when the walk gives way to standing, standing to the fall, the fall to the get-up, the get-up to the
walk.
*Why:* craft treats blends under about 0.2 s as pops (Jirik 2026, turning file [U, practice]); any switch of what
drives the bones can leave a jump.
*Measure:* in each frame where the driving mode changes, every drawn bone's position and velocity against the frames
either side.
*Bar:* S1 and S3 bars apply in the switch frame with no exemption; any part's velocity changing over 0.5 m/s in the
switch frame without contact (judgment).
*Applies:* every switch (walk, own body, fall, rise, work).
*Detector:* `OwnBodyTests.SentSomewhereItGivesItsBodyBackWalksAndStandsSoAgain` (`OwnBodyTests.cs:263`, outcome
only); `OwnBodyTests.PushedOverFromThePanelItsBodyIsHandedToTheFall` (`:236`).
*Eye:* the switch frames.

**S9. The long way round.**
*Looks:* a bone spins nearly a full turn to reach a nearby angle.
*Why:* interpolating rotations without the shortest-path check, or an Euler decomposition near 90 degrees, makes such
spins; PhysX joints have a singularity at 180 degrees of swing (twitches file [WE]).
*Measure:* per frame, the dot product of a bone's previous and current rotation (as quaternions) before any blend; the
swing angle in each joint frame.
*Bar:* any negative dot product used without flipping; swing over 150 degrees in any joint (judgment from the twitches
file).
*Applies:* everything drawn; joints of the physics body.
*Detector:* `ArticulationFactsTests.ADriveWrittenByTheShortestTurnCrossesAHalfTurn` (`ArticulationFactsTests.cs:1100`,
in `D:/Dev/WG`, not in this working copy).
*Eye:* as S3.

**S10. No arcs: a hand, foot or tool tip travels in a straight line (added in review).**
*Looks:* a reach or a swing goes from start to end along a ruler; it looks mechanical.
*Why:* limbs turn about joints, so the ends of limbs travel on arcs; "arcs" is one of the animators' principles (U2
A1, craft [WE in craft]); in reaching, the hand path is close to straight in people (Flash and Hogan 1985), so the
bar applies to swings and whole-limb movements, not to short reaches (judgment).
*Measure:* per discrete swing of a tool tip, a swinging foot in a turn, or the arm swing, the largest distance of the
path from the straight chord between its ends, over the chord's length.
*Bar:* under 3% of the chord for a tool swing or arm swing over 30 cm long (judgment). Reaches exempt.
*Applies:* tool swings, arm swing, swing foot in turns (with C11), getting up limb placements.
*Detector:* none; tool tip and hands are in the trace.
*Eye:* the tool tip's trail drawn on a still frame.

### 3.5 Family J: joints and anatomy

**J1. A joint past its range.**
*Looks:* an elbow bent backwards, a knee folded flat, a hip far behind the body (U3 F15: elbows the wrong way up to 70
degrees, knees to 180).
*Why:* normal ranges for adults 20-44 (CDC, Soucie et al. 2011 [WE]), in degrees: hip flexion 130-134, extension
17-18; knee flexion 138-142, extension 1-1.6; ankle dorsiflexion 13-14, plantarflexion 55-62; shoulder flexion
169-172; elbow flexion 145-150, extension 1-5; pronation 77-82, supination 85-91. Neck, spine, wrist, hip rotation and
ankle inversion come only from secondary tables [U] (U2 C1); neck norms differ widely (Thoomes-de Graaf 2020).
*Measure:* per frame, each joint's swing and twist in its anatomical frame (never Euler angles), drawn and physics.
Codable only with a per-rig table of each joint's anatomical zero pose and axes (drawn rig and physics body
separately; for creator bodies, generated with the body), which does not exist yet (review).
*Bar:* beyond the CDC mean plus 10 degrees (study plus a judgment margin); a hinge past straight by over 5 degrees
(study: normal extension 0-5); for the [U] joints, U2's secondary values plus 15 degrees (judgment). `FrameRecorder`
uses 8 degrees backwards for its "wrongway" (project); the catalogue's 5 is stricter and governs the scan, the
difference is flagged in section 6 (review).
*Applies:* everything; above all falls, lying, getting up, kneeling.
*Detector:* partial (review): `FrameRecorder.cs` "wrongway", an elbow or knee bent backwards over 8 degrees, and
"overfold", a joint folded past 150 degrees (about `:186-187`, `D:/Dev/WG` only); the new body's joint limits
prevent it on the physics body.
*Eye:* close sheets of elbows, knees and neck in getting up and falls.

**J2. A hinge bent sideways or twisted.**
*Looks:* a knee bending to the side, an elbow twisting; "arms seem like it's broken" (Luis, October 30, L20).
*Why:* knees and elbows are hinges; sideways bend beyond a few degrees reads as broken (twitches file, inference [U]).
*Measure:* per frame, the angle of a hinge's rotation axis away from its anatomical axis, and its twist.
*Bar:* over 5 degrees sideways or 15 degrees of twist (judgment).
*Applies:* everything.
*Detector:* none.
*Eye:* front and side close views of knees and elbows.

**J3. Living at a limit.**
*Looks:* a body that hangs on its joint stops like a doll, or strains.
*Why:* everyday movement uses much less than the full range (walking: about 60 degrees knee, 30 hip; gait texts [WE,
not re-opened]); sitting on a limit reads as a doll (U2 B5, craft [U]).
*Measure:* the share of time each joint spends within 10 degrees of its anatomical or physics limit, per action.
*Bar:* over 20% of an action's time, or over 1 s held at a limit while at ease (judgment).
*Applies:* standing, holding, lying, getting up, kneeling.
*Detector:* none.
*Eye:* held poses: relaxed, or jammed?

**J4. The hip wrung round over a planted foot.**
*Looks:* in a turn the body twists round over a foot that stays put; "weird leg positions while turning" (Luis,
November 13, L24).
*Why:* people use about 31-35 degrees of stance-hip twist in a 90 degree turn (older adults, Steingrebe 2024, turning
file [1S, checked there]); the walk allows 62 degrees (`ProceduralBiped.cs:480`; `MostTwist` is a cap, not a
measurement).
*Measure:* per frame, the yaw of the pelvis relative to the planted foot; the time spent above each level.
*Bar:* over 35 degrees flagged, over 45 a fault (judgment from the study); past `MostTwist` plus 2 degrees a fault
(project, `TurnBench` F1). The two disagree (section 6). Review: the comparison is not like for like. The study's
figure is the hip's own transverse rotation (femur against pelvis); the measure here is pelvis yaw against the
planted foot, which also contains the knee's and shank's rotation. In the kinematic leg, whose knee always points
along the pelvis (`ProceduralBiped.cs`, about :1300), that twist shows up at the ankle and between knee and foot
(J5), not at the hip. The bars are kept as starting values; the study number is a guide, not a matching limit.
*Applies:* turns, standing turns, settling, tool stances.
*Detector:* `TurnBench` F1 (drafted).
*Eye:* top camera on the planted leg through each turn.

**J5. A knee that does not point over its foot.**
*Looks:* the knee aims far off the line of the toes.
*Why:* the leg works as a hinge over the foot; a large offset reads as twisted (project judgment).
*Measure:* per frame, the angle in the ground plane between the knee's bend direction and the foot's forward.
*Bar:* over 35 degrees (project, `TurnBench` F6). Lead recorded in `Docs/Handoff.md`: a planted knee may point up to
62 degrees off its foot (the `MostTwist` cap, J4), so this kind is expected to fire in sharp turns.
*Applies:* walk, turns, crouches, getting up.
*Detector:* `TurnBench` F6.
*Eye:* front view.

**J6. A knee pop or flip near straight.**
*Looks:* the knee snaps straight, or flips to bend the other way.
*Why:* two-bone IK near full reach changes the knee angle very fast for a small change of target; at the singular line
the bend flips (craft practice, consistent across sources [U]).
*Measure:* knee angle speed while within 10 degrees of straight; target distance over thigh plus shin; the sign of the
knee's bend axis against the pelvis's forward.
*Bar:* any sign flip; reach ratio over 0.98 for more than 2 frames; knee speed over the T8 bar while near straight
(judgment).
*Applies:* the kinematic walk, foot placement on slopes, legs reaching in the get-up.
*Detector:* none.
*Eye:* side view of the knee at full stride and on ramps.

**J7. A limb that comes apart or changes length.**
*Looks:* "this weird leg deattached part" (Luis, November 13, L25); a forearm that stretches.
*Why:* viewers notice about a 3% change in a limb's length when attending to it, while over 20% can pass unseen with
attention elsewhere (Harrison, Rensink, van de Panne 2004 [1S]); the fall's joints may pull apart up to their
projection distance of 2 cm and 5 degrees (`PhysicalFall.cs:379`; a lead only, U3 L25).
*Measure:* per frame, the distance between each pair of joined drawn joints against its rest length; per physics
step, the separation of each joint's two anchors.
*Bar:* drawn length off by over 1% (judgment: the bones are rigid, so any change is a bug, well under the 3% seen);
physics anchor separation over 5 mm (judgment).
*Applies:* falls, lying, getting up, hard shoves, anything fast.
*Detector:* `PhysicalFallTests.cs:85` measures "furthest apart" in a fall (bar in the test); nothing per frame.
*Eye:* close views of hips, knees and shoulders while lying and rising.

### 3.6 Family O: coordination and order

**O1. All of one piece (en bloc).**
*Looks:* head, chest and hips start in the same frame and turn at one pace (U3 F03).
*Why:* in quick turns people turn eyes, head, chest, hips, then feet, the head 0.05-0.12 s and 15-28 degrees ahead of
the pelvis; only slow turns (head under about 100 degrees a second) are nearly en bloc (Khobkhun 2021, 2022, turning
file [1S lab, checked there]); walking, the head starts turning about 1.1 m before a corner (Sreenivasa 2008 [1S]).
*Measure:* per turn, onset times and yaw of head, thorax, pelvis, leading foot, trailing foot; the largest head-pelvis
yaw difference.
*Bar:* in a turn over 45 degrees with head yaw speed over 100 degrees a second, a head lead under 30 ms or a
separation under 8 degrees (judgment from the study).
*Applies:* turns, looking round, settling, starting work.
*Detector:* partial (review): `motion_breaks.py` reports when root, hips, chest and head each reach 10% and 90% of a
turn over 60 degrees (`turn_began`), which gives the onset order; head-pelvis separation is not reported.
*Eye:* top camera: does the head lead?

**O2. A turn's parts in the wrong order.**
*Looks:* the body walks off backwards before facing (U3 F03); the feet turn before the head.
*Why:* the order is head, thorax, pelvis, leading foot, trailing foot (Khobkhun 2022 [1S]; Hollands 2004 [1S]).
*Measure:* the onset sequence per turn; travel direction against facing.
*Bar:* any reversal of the order by over 50 ms; travel opposite to the new facing for over 0.3 s (judgment).
*Applies:* turns.
*Detector:* partial: as O1 (`motion_breaks.py`, `turn_began`), for root, hips, chest and head; not the feet (review).
*Eye:* top camera.

**O3. Arms that do not swing with the walk.**
*Looks:* arms stiff at the sides, swinging with the same-side leg, or both together.
*Why:* normal arm swing is opposite the same-side leg; holding the arms still cost 12% more energy, binding them 7%,
and swinging them with the same-side leg 26% (Collins, Adamczyk, Kuo 2009 [1S, read in full, n=10]).
*Measure:* per stride, shoulder flexion against the same-side hip's: relative phase; shoulder swing range.
*Bar:* relative phase outside 180 plus or minus 60 degrees, or swing range under 5 degrees at over 1 m/s with the arm
free (judgment).
*Applies:* the walk with free arms; not while carrying or holding.
*Detector:* none.
*Eye:* side camera, one stride.

**O4. A trunk welded to the hips at pace.**
*Looks:* chest and hips turn as one block in a brisk walk.
*Why:* pelvis and thorax move nearly together at slow speed (about -20 degrees) and increasingly opposite as speed
rises (about -140 degrees at 1.5 m/s), with large differences between people (Lamoth et al. 2002; review 2021 [WE as
direction; large variance]).
*Measure:* per stride, the relative phase of pelvis and thorax yaw, across walking speeds.
*Bar:* no change of relative phase (within 10 degrees) between 0.6 and 1.4 m/s (judgment; a check of direction, not
of value).
*Applies:* the walk.
*Detector:* none.
*Eye:* top camera, slow and brisk walks side by side.

**O5. A head that bobs with the body.**
*Looks:* the head nods and rocks with every step.
*Why:* in walking the head's pitch is held within a few degrees (Pozzo, Berthoz, Lefort 1990 [WE]).
*Measure:* per stride, the range of head pitch and roll in world.
*Bar:* pitch range over 10 degrees in a plain walk (judgment from "a few degrees").
*Applies:* walk, carrying, catch steps.
*Detector:* `MinerTests.TheMinersHeadsStayAboveTheirFeet` (`MinerTests.cs:345`; position, not pitch).
*Eye:* side camera on the head.

**O6. Twinning.**
*Looks:* both arms or both legs doing the same at the same moment; a mirror pose.
*Why:* animators treat it as a mark that kills life (Williams 2001 [WE in craft]); relaxed standing is asymmetric
moment by moment (Prado-Rico and Duarte 2019 [1S]).
*Measure:* per joint pair, the difference between the left angle and the mirrored right angle.
*Bar:* under 2 degrees for all pairs of a limb for over 1 s in an idle or held pose (judgment).
*Applies:* standing at ease, holding, lying, settling, getting up.
*Detector:* none.
*Eye:* front view of the idle.

**O7. A movement built in parts.**
*Looks:* bow, wait, squat, bow, then the hand (U3 L08, F05): "feels more like an animation also in parts" (Luis,
October 8).
*Why:* real actions overlap their parts (craft: follow-through and overlapping action [WE]).
*Measure:* within one action, intervals where whole-body joint speed falls under 10% of the action's peak for over
0.2 s between its sub-movements.
*Bar:* any such pause the task does not explain (judgment).
*Applies:* kneeling, picking up, getting up, tool work, hanging back.
*Detector:* none.
*Eye:* does it flow as one?

**O8. No follow-through.**
*Looks:* every part stops in the same frame.
*Why:* distal parts lag and settle after proximal ones (craft [WE]).
*Measure:* at the end of each action, each segment's stop time (speed under 5% of its peak).
*Bar:* all segments stopping within one physics step of each other (judgment).
*Applies:* swings, catches, settling, reaching.
*Detector:* none.
*Eye:* the end of each action.

**O9. No anticipation.**
*Looks:* a step with no lean first, a swing with no wind-up, a turn with no head lead.
*Why:* the anticipatory shift before a step (Laudani 2021 [1S]); wind-up before a swing (craft [WE]); head lead (O1).
*Measure:* per action, the preparatory motion in the 0.5 s before (centre of pressure shift, backswing, crouch).
*Bar:* for steps see B4; a tool swing with a backswing under 20% of its downswing's angle (judgment).
*Applies:* planned steps, tool work, turns, getting up (weight shifted before each push).
*Detector:* none.
*Eye:* the half second before.

**O10. A strike made by the arm alone.**
*Looks:* a heavy pick swung from the shoulder, trunk and hips still.
*Why:* throws and strikes run proximal to distal, peak speeds in order pelvis, trunk, upper arm, forearm, hand (golf,
throwing [WE in sport]); not every strike does (Preuschl 2016 [1S]); for heavy tools no numbers were found (gap).
*Measure:* per swing, the order and times of peak angular speed of pelvis, trunk, upper arm, forearm, hand.
*Bar:* trunk and pelvis peaks under 20% of the forearm's, for a tool over 2 kg (judgment).
*Applies:* tool work.
*Detector:* none.
*Eye:* side and top cameras through a swing.

**O11. Not looking at what it does.**
*Looks:* the hand reaches for the lantern while the head looks elsewhere (U3 L11).
*Why:* the eyes and head lead reaching and turning (Hollands 2002, turning file [1S]); the lantern fix added the look
(U3 F08).
*Measure:* the angle between the head's forward and the direction to the target during reach and grip.
*Bar:* over 30 degrees for over half of a reach (judgment).
*Applies:* reaching, picking up, hanging back, aiming a tool, turns.
*Detector:* none.
*Eye:* does the head find the target first?

**O12. Changing direction in the air.**
*Looks:* the body yaws round while no foot is down.
*Why:* in games, switching clips while no foot was down made the body seem to change direction in mid-air (van den
Heuvel 2021, turning file [1S, checked there]); a body cannot turn in flight without a push (physics).
*Measure:* body yaw change during frames with no planted foot.
*Bar:* over 5 degrees (judgment).
*Applies:* walk, catch steps, falls.
*Detector:* none.
*Eye:* top camera.

**O13. The walk's spin not cancelled (added in review).**
*Looks:* the body wags or corkscrews about its vertical as it walks; the arms and legs do not balance each other.
*Why:* in human walking the segments' angular momenta largely cancel: whole-body angular momentum stays small, the
segments' contributions cancelling by about 95%, 70% and 80% about the three axes (vertical, side to side, front to
back) (Herr and Popovic 2008 [1S]).
*Measure:* per stride, whole-body angular momentum about the centre of mass, against the sum of the segments' own
magnitudes, per axis (cancellation = 1 - |sum| / sum of |parts|). Needs segment masses and inertias (3.0; in the
kinematic walk from the mass table).
*Bar:* cancellation under 80% about the vertical, 50% side to side, or 60% front to back, averaged over 5 strides
(judgment, strict under the study's figures).
*Applies:* the walk, carrying (with the load as a segment).
*Detector:* none.
*Eye:* top camera: do the arms swing against the legs so the trunk stays quiet?

### 3.7 Family W: weight, cause and reaction

**W1. Wrong gravity in flight: floaty or heavy.**
*Looks:* a fall that drifts down, a dropped tool that sinks slowly, or one that slams down too fast.
*Why:* the feet's "alive" signal needs gravity-shaped vertical acceleration; inverted, it is lost (Troje and Westhoff
2006; Troje and Chang 2023 [WE]); in VR a ball at 1 g was rated natural 83% of the time (median), at half g 67%, at
2 g 27% (La Scaleia et al. 2020 [1S, n=16]); horizontal velocity errors in flight are seen more easily than vertical,
added acceleration more than added deceleration (Reitsma and Pollard 2003 [1S, abstract]).
*Measure:* in every frame of free flight (W definitions in 3.0), the vertical acceleration of the centre of mass of
the flying body or thing, and its horizontal acceleration.
*Bar:* vertical acceleration outside -9.81 plus or minus 5%, or horizontal over 0.5 m/s2, over 3 consecutive steps
(physics for the law; judgment for the tolerance).
*Applies:* falls, jumps, a dropped or thrown tool, things knocked off a hook, the body leaving the ground in a hard
shove.
*Detector:* none.
*Eye:* side camera, falls and drops.

**W2. Turning in flight with nothing to turn it.**
*Looks:* a body or tool spins up or slows its spin in mid-air.
*Why:* in flight, angular momentum about the centre of mass is conserved (physics; used to make animations physical,
Liu and Popovic 2002 [1S]).
*Measure:* in free flight, whole-body angular momentum about its centre of mass, step to step.
*Bar:* change over 10% in 0.1 s (judgment for the tolerance; physics for the law). Corrected in review from 5%: the
fall's bodies carry `angularDamping` 0.6 (`PhysicalFall.cs:335`), which by design loses about 6% of spin per 0.1 s,
so 5% would flag every fall. The damping's share is reported beside the flag as a candidate cause, not hidden.
*Applies:* falls, a thrown or dropped tool, hard shoves.
*Detector:* none.
*Eye:* as W1.

**W3. Energy from nowhere.**
*Looks:* a part flies (the "flying boot"); repeated shoves give "very whacky buggy physics" (Luis, October 13, L17).
*Why:* the engine's own step can add energy to a fast-turning part: a leg gained 270-1,120 J in steps nothing gave it
(U3 F18; project review `Reviews/2026-11-10_FlyingBoot_K1`).
*Measure:* per physics step, each body's and the whole body's change of mechanical energy, minus the joint work
(each drive's real `driveForce` times its joint turn in the step; readable, `OwnBody.cs:1097`) and contact work; the
shove trace's own count (`Docs/Technical/AnyBody/tools/shoves.sh`, in `D:/Dev/WG` only).
*Bar:* any body gaining over 1 J and over 20% more than joint and contact work can account for in one step (judgment,
to calibrate with `shoves.sh` before the first run); the fastest part in repeated shoves within the test's figures
(project).
*Applies:* shoves, falls, the get-up, fast limbs anywhere.
*Detector:* `OwnBodyTests.ShovedAgainAndAgainNothingOfItFlies` (`OwnBodyTests.cs:764`);
`ArticulationFactsTests.AFastLegGainsByTheStepItself` (`:421`) and `WhatAFastLegsGainComesOf` (`:385`) (in
`D:/Dev/WG` only).
*Eye:* sheets of each flagged step.

**W4. Help the body does not give itself.**
*Looks:* an assisted rise, a body lifted by an unseen hand (Luis, October 8: "just an animation", L09).
*Why:* Luis's rule: always up by its own body in the game; help is the "absolute last resort" (October 22, L19).
*Measure:* a log of every force or torque applied by code that is not a joint drive or a contact (root forces,
keepers, helpers, backup animation), with its size and the frames; and the Newton residual (B5).
*Bar:* zero in the game's own path (Luis).
*Applies:* standing, catching, falling, getting up, settling, work.
*Detector:* `PhysicalFallTests.LaidDownAnyWayItGetsUpByItsOwnStrength` (`PhysicalFallTests.cs:158`, "no old way").
*Eye:* does anything rise that nothing pushes?

**W5. Posed or timer-led movement.**
*Looks:* lying "dead" until a timer runs out, then rising with "a magical not physical animation" (Luis, October 13,
L16); every part sliding through the air from lying to a crouch (U3 F09).
*Why:* Luis's rules: the body's own, not posed (October 9); no fake animations (October 8).
*Measure:* during a rise, frames where the centre of mass gains height while the summed contact normal force is under
`M g (1 + a_up / g)` minus 10% (lifted by nothing); the start time of each rise against the body's state (still,
breathing, having tried), across 10 falls.
*Bar:* any lifted-by-nothing frame (physics); rise starts at the same delay (spread under 10% of the mean) across
falls whose bodies came to rest differently (judgment, under Luis's rule).
*Applies:* getting up, kneeling, any pose change.
*Detector:* `PhysicalFallTests.cs:85, :158` (outcome; up within 40 s); `OwnRiseBench.cs`.
*Eye:* does each rise begin from something the body did?

**W6. Lying lifeless when it is not.**
*Looks:* down after a fall, the body lies fully still until it rises (U3 L13: "does not breathe, does not stir").
*Why:* a living body breathes and stirs; six of six judges flagged it (project review October 9).
*Measure:* while lying and not unconscious by design, whole-body joint speed root-mean-square over 2 s, and the
breath's chest motion.
*Bar:* no breath motion, or whole-body joint speed under the standing-at-ease floor (L1) for over 2 s (judgment, under
Luis's rule).
*Applies:* lying after a fall, resting.
*Detector:* none.
*Eye:* a still body: alive?

**W7. A reaction that does not match the push.**
*Looks:* a big shove and a small answer, or a reaction thrown off to one side.
*Why:* over-reaction was more acceptable than under-reaction; angular distortions lowered acceptance; reactions
moving away from the pusher were preferred (Hoyet, McDonnell, O'Sullivan 2012 [1S, abstract]).
*Measure:* in the first 100 ms after a shove, the centre of mass velocity change against impulse over mass, and its
direction against the push's.
*Bar:* direction off by over 30 degrees, or a velocity change under 80% of impulse over mass (judgment; on a physics
body this flags a helper or a wrong mass).
*Applies:* shoves, pulls, being struck, tool recoil.
*Detector:* none.
*Eye:* does the body go where the push sent it?

**W8. A switch at a threshold.**
*Looks:* a small push gives one kind of answer and a slightly larger push a different kind, with a jump between.
*Why:* Luis's rule (October 13): the answer grows smoothly with the push, as a person's does; studies agree that
answers grade with push size (arm raised 25 / 33 / 52% of upper-arm length for small / medium / large pushes, Jeon
2022, turning file [1S, checked there]).
*Measure:* a sweep of push sizes in steps of 10%: for each, step length, step time, arm raise, trunk lean, and whether
a step was taken.
*Bar:* any measure jumping by over 30% of its range between neighbouring push sizes (judgment, under Luis's rule).
*Applies:* shoves, pulls, loads, slopes, tiredness.
*Detector:* `PhysicalFallTests.AHardPullThrowsItDownAndALightOneDoesNot` (`PhysicalFallTests.cs:320`, two points only).
*Eye:* the sweep side by side.

**W9. Limp, or stiff as a statue, when conscious.**
*Looks:* arms trailing like a rag doll while the body is awake and catching itself; or a fall taken stiff as a plank.
*Why:* a conscious body holds its limbs and protects itself; mixing a living look with machine or rag motion is the
road to eeriness (Katsyri et al. 2015 review; Saygin et al. 2012 [WE as direction]).
*Measure:* per limb, drive activity (target changes and the drive's `driveForce`) against its passive motion, by
state.
*Bar:* a limb fully passive for over 0.5 s while the body is in a conscious state (judgment).
*Applies:* catching, falling, getting up, carrying.
*Detector:* none.
*Eye:* does each limb look owned?

**W10. A landing that does not take the weight.**
*Looks:* the body lands from a step, a fall or a drop with no give at the knees and hips, or bounces like rubber.
*Why:* weight is shown by loading and recovery (craft principle "weight" [WE]); the eye reads mass from timing (section
1 of U1; Runeson and Frykholm [WE]).
*Measure:* after each landing, the knee and hip flexion added in the first 0.1 s, and the centre of mass's vertical
velocity: rebound against impact speed.
*Bar:* a centre of mass rebound over 20% of the impact speed, or no added knee flexion after a landing faster than 0.5
m/s (judgment). Review, from U2 B8: on the physics body, a contact force peak beyond what a person's landing gives
(walking heel strike peaks near 1.1-1.3 body weight, gait texts [WE, not re-opened]): over 3 body weights in a
walking or settling step is flagged (judgment).
*Applies:* catch steps, landings, falls, steps down.
*Detector:* none.
*Eye:* side camera at each landing.

**W11. A change of motion nothing caused.**
*Looks:* a part jerks, slows or turns by itself; read as an intention, and a purposeless one.
*Why:* a change of speed and direction with no visible cause is read as the body's own act (Tremoulet and Feldman
2000; Pratt et al. 2010 [WE]).
*Measure:* per physics step, each body's velocity change minus what gravity, its contacts and its joint forces account
for (from the joint impulses where the engine reports them).
*Bar:* an unexplained change over 0.3 m/s in one step (judgment; ties to B5 and W3).
*Applies:* everything with the physics body.
*Detector:* none.
*Eye:* as S2.

**W12. Arms that do nothing when balance goes (added in review).**
*Looks:* shoved hard, the body steps or topples with its arms hanging as before.
*Why:* people raise and spread the arms after a push, more for a bigger push (arm raised 25 / 33 / 52% of upper-arm
length for small / medium / large pushes, Jeon 2022, turning file [1S, checked there]; see W8).
*Measure:* from a shove that takes the XcoM out of the BoS (B1), the hands' rise and spread from the body over the
next 0.5 s, in upper-arm lengths.
*Bar:* under 10% of upper-arm length of raise or spread by 0.5 s after such a push, with the arms free (judgment, well
under the study's smallest 25%).
*Applies:* shoves, trips, slips at work; not while carrying or gripping.
*Detector:* none.
*Eye:* front camera after a hard shove: do the arms come up?

**W13. A fall with no protective reaction (added in review).**
*Looks:* the conscious body falls like a plank or a sack, hands never going out, landing flat on hip or head.
*Why:* in unexpected falls from standing, young adults' movements are not random: the arms bring the wrist to the
ground nearly together with the pelvis (on average about 38 ms apart) and the trunk turns in sideways falls so as not
to land on the side of the hip (Hsiao and Robinovitch 1998 [1S, abstract]); citing papers give about 700 ms from the
start of the fall to impact and active movement from about 300 ms [U, secondary].
*Measure:* per fall of a conscious body, the time from fall start (B1 lost, no step saving it) to first hand-ground
contact against pelvis-ground contact; trunk rotation in sideways falls; head height at the first contact of the
trunk.
*Bar:* no hand reaching the ground within 0.15 s of the pelvis in a forward or sideways fall, or the head hitting the
ground first (judgment from the study). Not for falls designed as unconscious.
*Applies:* falls, trips, hard shoves; ties to W9 (limp or stiff) and to Luis's question 5 (how stylised).
*Detector:* none.
*Eye:* side and front cameras through each fall: do the hands go out?

**W14. A walk that glides, bounces or crouches (added in review).**
*Looks:* the body floats along at one height like on rails, bobs like a ball, or walks with its knees bent low.
*Why:* in walking the centre of mass rises and falls once per step, by a few centimetres at a normal pace; slow
walking roughly halves the vertical travel while side-to-side travel more than doubles (Orendurff et al. 2004 [1S,
read through a citing paper]); the vertical travel grows with speed (several studies [WE as direction]).
*Measure:* per stride on a straight level walk, the vertical range of the centre of mass (the drawn hips in the
kinematic walk), as a share of `L`; the hips' mean height against standing height.
*Bar:* vertical range under 1% of `L` (a glide) or over 8% of `L` (a bounce) at a normal pace; mean hip height
under 92% of standing (a crouched walk) (judgment, around the studies' few centimetres for a 0.9 m leg; to be set on
motion capture before the first run [U]).
*Applies:* the walk at every pace, carrying (a load lowers it a little), any body proportion (scaled by `L`).
*Detector:* partial: `motion_breaks.py` reports the hips' rise and fall over a stride (`rise_fall`); `motion_dips.py`
(this copy only) measures how low the hips ride under standing height (review).
*Eye:* side camera, a few strides, the hips against a level line.

### 3.8 Family L: life and variation

**L1. Too still.**
*Looks:* a statue, a mannequin.
*Why:* quiet stance never stops: centre of pressure moves at about 9.2 mm/s (Posturographic standards [1S]); human sway
makes viewers more empathic to a character (Treal 2021, turning file [1S]); craft's "moving hold" (practice [WE]).
*Measure:* over 10 s at ease, centre of pressure speed from contacts and whole-body joint speed root-mean-square.
*Bar:* centre of pressure speed under 3 mm/s (judgment, a third of the human mean). A floor only: never a reason to
enlarge the idle Luis loves (Luis, October 10 and November 13). Review: PhysX contact noise alone can exceed 3 mm/s,
so a dead-still body could pass on noise. The primary measure is the centre of mass's and the drawn head's sway
speed; the centre of pressure counts only above the noise floor measured on a body held still by a fixed joint.
*Applies:* standing at ease, holding still, waiting, lying.
*Detector:* `OwnBodyTests.StandingAMinuteItIsAliveAndDoesNotWobble` (`OwnBodyTests.cs:299`, "alive").
*Eye:* a minute of the idle: alive?

**L2. Too regular.**
*Looks:* shifts, fidgets or breaths that come like clockwork.
*Why:* in long free standing, shifts came at irregular times: 316 plus or minus 292 s front-back, 199 plus or minus 148
s side to side; drifts 319 plus or minus 173 s and 529 plus or minus 333 s; fidgets more regular, 59 plus or minus 15
s and 49 plus or minus 16 s (Duarte and Zatsiorsky 1999, turning file [1S, n=10, checked there]).
*Measure:* intervals between successive shifts, fidgets and drifts, each kind apart; their spread over their mean.
*Bar:* shifts or drifts with spread over mean under 0.4; fidgets under 0.15 (judgment from the study's 0.5-0.9 and
0.25-0.33). Review: a spread over mean needs at least 10 intervals of each kind, which at the study's rates is about
35 minutes of idle for shifts and up to 90 for drifts; fewer gives no verdict, only a note. A long run is the cost;
raising the idle's rate of shifts to get a verdict sooner is not allowed (memory "Luis loves the idle").
*Applies:* standing at ease, waiting, resting, lying.
*Detector:* none.
*Eye:* ten minutes of the idle, a strip of when things happen.

**L3. Too even.**
*Looks:* weight perfectly shared between both feet; a body perfectly symmetric.
*Why:* relaxed standing has two preferred weight shares and is asymmetric moment by moment (Prado-Rico and Duarte 2019
[1S]); a little gait asymmetry is not seen (5% unseen, 13% seen, Handzic and Reed 2015 [1S]); whether perfect symmetry
itself looks artificial was not found tested [U].
*Measure:* the share of vertical force on each foot over time; left-right differences of step length and swing time.
*Bar:* weight share within 50 plus or minus 2% for over 10 s at ease (judgment); step or swing asymmetry over 10% in a
plain walk (judgment, between the study's 5% and 13%).
*Applies:* standing at ease, the walk.
*Detector:* none.
*Eye:* front view of the idle; side view of the walk.

**L4. The same movement twice.**
*Looks:* the same shift, the same step, the same rise, repeated exactly.
*Why:* viewers caught appearance clones far more easily than motion clones, but motion clones are caught too
(McDonnell et al. 2008 [1S]); three distinct walks made a crowd look varied (Prazak and O'Sullivan 2011 [1S]);
cyclic animation looks unnatural through repetition (Bodenheimer, Shleyfman, Hodgins 1999 [1S]).
*Measure:* for each repeated action, the correlation over all joints between successive instances, time-aligned.
*Bar:* over 0.98 for two instances in a row (judgment).
*Applies:* shifts, fidgets, steps, catches, rises, swings.
*Detector:* none.
*Eye:* instances side by side.

**L5. Life made of noise.**
*Looks:* an idle that trembles; variation that looks like shaking.
*Why:* plain white noise on joints reads as shaking; task-aware variation looked human (Gielniak, Liu, Thomaz 2011
[1S]); 90-95% of sway power lies below 2 Hz (Soames and Atha 1982 [1S]).
*Measure:* the share of joint and centre of mass motion power above 2 Hz at ease; the centre of pressure only after
its contact noise is calibrated (see L1; contact noise is itself above 2 Hz and would raise a false alarm, review).
*Bar:* over 20% above 2 Hz (judgment from the study).
*Applies:* standing at ease, resting hands, lying.
*Detector:* none.
*Eye:* close views of hands at rest.

**L6. Breath wrong.**
*Looks:* a breath too strong (Luis, October 10, L14), too quick at rest, or one that rocks the whole body.
*Why:* adults breathe about 12-20 times a minute at rest (clinical norm [WE]); body motion follows the breath but the
centre of pressure barely does, small leg and trunk movements cancelling it (Hodges et al. 2002 [1S]; the "0.5
degrees" per breath in U2 could not be confirmed [U]); Luis: gentle, and scaled with the situation.
*Measure:* breath rate; the breath's chest motion against the build Luis approved; the coherence between breath and
centre of pressure.
*Bar:* at rest, breath larger than the approved build's (Luis); rate outside 10-20 a minute at rest (study); centre of
pressure coherence with breath over 0.5 (judgment from the study's under 0.2), on the noise-calibrated centre of
pressure (L1, review).
*Applies:* standing at ease, lying, after work, after a shove (larger, then settling).
*Detector:* `OwnBodyTests.ItsBreathHasTwoMoreLooks` (`OwnBodyTests.cs:417`).
*Eye:* the idle against the approved one.

**L7. Tired like fresh.**
*Looks:* a spent miner moving exactly like a fresh one, or tiredness shown only as slower playback.
*Why:* tired people widen steps, change joint angles and vary more stride to stride (2019 and 2025 systematic reviews
[WE as direction; details differ]); weight shifts and fidgets rise with time and discomfort (Rekant 2019; Wiggermann
2013 [1S each]).
*Measure:* across a work-tire-rest cycle: step width, stride variability, sway, frequency of shifts, breath rate.
*Bar:* no measure changing by over 10% between fresh and spent (judgment).
*Applies:* the whole work cycle, all three miners.
*Detector:* none.
*Eye:* fresh and spent side by side.

**L8. A rise or stance that repeats itself.**
*Looks:* every get-up the same sequence of stances; "robotic or repetitive" (Luis, November 3, L21; no record of a
check, U3).
*Why:* as L4; the capacity-driven rise was approved with this watch named (memory "Any-body direction decided").
*Measure:* across 10 rises from different lyings, the sequence of contact events (which limb takes weight when) and
the timing of each.
*Bar:* over 80% of rises with the same sequence and timing spread under 10% of the mean, from lyings that differed
(judgment).
*Applies:* getting up, kneeling, settling.
*Detector:* none.
*Eye:* ten rises side by side.

**L9. Resting limbs locked.**
*Looks:* a free hand that always returns to exactly the same place.
*Why:* Luis asked for "some level of natural variability" in resting hands and limbs (November 9, L22).
*Measure:* the resting pose of each free limb across 10 rests.
*Bar:* spread under 2 degrees at every joint across rests (judgment).
*Applies:* standing at ease, holding, after work.
*Detector:* none.
*Eye:* rests side by side.

**L10. Life switched off outside the idle (added in review).**
*Looks:* the body breathes and stirs at ease, then goes dead the moment it walks, works, carries or waits in a queue;
or life starts and stops with a jump at each change of mode.
*Why:* a living body does not stop breathing or stirring when it changes task; U1 (section 4) names the mismatch
between modes as a mark of an animated, not living, body [U, inference from the perception literature]; Luis's
"gentle, and scaled with the situation" (October 10) asks for life everywhere, scaled, not only at ease.
*Measure:* the breath's chest motion and the small-motion power (L1, L5) in every mode (walk, work, carrying, waiting,
lying), and their change across each mode switch.
*Bar:* breath motion absent for over 5 s in any conscious mode, or the breath's amplitude or phase jumping (over 50%
or over 90 degrees) within one frame of a mode switch (judgment).
*Applies:* every mode; every switch (with S8).
*Detector:* none.
*Eye:* the chest across a switch from the idle to a walk and back.

### 3.9 Family H: held and hung things, and secondary motion

**H1. A thing that floats.**
*Looks:* "the lamp is literally floating by her side" (Luis, October 3, L03); a pickaxe "just sitting behind his back"
(October 1, L02).
*Why:* Luis's rule: anything that reads as an object is held, hangs, swings and is stopped by the body (October 3).
*Measure:* per frame, for each held or hung thing, the force its motion needs (`m (a - g)`) and the contacts or joints
that carry it; its attachment point against the body's colliders.
*Bar:* any frame with motion not carried by a grip, hook or contact; any attachment point inside a body collider
(Luis, physics). Review: for a thing held in two hands, the distance between the two grip points on the thing stays
fixed (a rigid handle); the drawn hands' distance changing by over 5 mm while both grip means a hand has left the
handle (judgment).
*Applies:* carrying, tools, lamps, satchels, mugs, straps, laces.
*Detector:* `MinerTests.WhatAMinerCarriesHangsFromItsHandAndStaysOutOfTheBody` (`MinerTests.cs:157`); `HungThingTests`
(`HungThingTests.cs:62, :199, :254, :327, :397`).
*Eye:* close views of every held and hung thing, moving and at rest.

**H2. A thing through the body or the ground.**
*Looks:* the satchel through the thigh, the lantern into the ground (U3 F13).
*Why:* as H1 and C7.
*Measure:* per frame, penetration of each held or hung thing into body and world colliders; drawn mesh overlap.
*Bar:* over 5 mm for over 2 frames (judgment); clear of the coat however the body is turned, on the ground and not in
it (project, `HungThingTests.cs:254, :327`).
*Applies:* as H1, and lying.
*Detector:* `HungThingTests` as above.
*Eye:* as H1, with the body turned and lying.

**H3. A hanging thing that is not a pendulum.**
*Looks:* it swings too fast, too slow, or not at all; it does not lag when the body moves.
*Why:* a hanging thing swings with a period of about `2 pi sqrt(l / g)` for its pivot-to-centre length `l` (physics);
loose parts lag their parent and settle after (craft [WE]).
*Measure:* the swing period after each disturbance; the lag angle against the parent's acceleration.
*Bar:* period off by over 15% of the pendulum's (judgment for the tolerance); lag zero, or in the wrong direction,
during the parent's accelerations (physics). Review: after a disturbance, with the body still, the swing's
amplitude must fall from one swing to the next; an amplitude that grows with nothing pushing it (a thing pumped by
the engine or by jitter of its pivot) is a fault (physics, as W3).
*Applies:* lamps, satchels, mugs, tools on hooks, straps.
*Detector:* `HungThingTests` (hanging, not period).
*Eye:* after each start, stop and turn.

**H4. Clothing that explodes or is glued.**
*Looks:* coat flaps flying out in a bend (Luis, October 8, L07), or moving exactly with the bone like a shell.
*Why:* a flap turned by two thigh angles ran away when the thigh came level (U3 F06); loose things must lag and
settle (craft [WE]).
*Measure:* each flap's angular speed and its angle against the part it lies on.
*Bar:* flap angular speed over 20 rad/s, or a change over 90 degrees in 0.1 s (judgment); zero lag during the parent's
accelerations (judgment).
*Applies:* kneeling, getting up, falls, turns, walking.
*Detector:* none.
*Eye:* the coat in kneels and rises.

**H5. A hand that does not grip.**
*Looks:* fingers short of the handle, or through it.
*Why:* grasp research measures both contact and penetration and needs both (ContactGen 2023; GraspTTA 2021 [WE as
convention]).
*Measure:* per frame of a grip, the gap and the depth between each finger and the handle.
*Bar:* gap or depth over 3 mm on the fingers that should touch (judgment); handle sizes within 3 to 20 mm grip
(project, `HungThingTests.cs:199`).
*Applies:* every grip.
*Detector:* `MinerTests.AFreeHandClosesRoundAHandleAndOpensAgain` (`MinerTests.cs:255`); `HungThingTests`.
*Eye:* close views of hands on handles.

**H6. A load the body does not show.**
*Looks:* a heavy load carried with an unloaded posture: no lean, the same step.
*Why:* with heavier loads people take more steps a minute, spend longer in double support, bend the knee more and
lean the trunk forward (Harman et al. US Army reports; 10-20% body weight studies [WE as direction; no lean angle per
load found]); the combined centre of mass must stay over the support (physics).
*Measure:* the centre of mass of body plus load against the BoS; trunk lean against the load's offset; cadence and
double support against the unloaded walk.
*Bar:* combined centre of mass outside the BoS while standing (physics); no change of trunk lean (under 1 degree) or of
double support (under 3%) for a load over 10% of body weight (judgment).
*Applies:* carrying, holding a tool still, picking up.
*Detector:* none.
*Eye:* side camera, loaded and unloaded.

**H7. A pose no arm keeps up.**
*Looks:* the lantern held on a straight arm out to the side for as long as it is carried (Luis, October 8, L11).
*Why:* no arm keeps that up (U3 F08); muscle endurance falls steeply with load (physiology [U, not checked here]).
*Measure:* the shoulder moment needed (load and arm weight times their horizontal distance) against the body's
strength, and how long it is held.
*Bar:* over 30% of the shoulder's strength for over 20 s with no change of grip or rest (judgment).
*Applies:* carrying, holding a tool out, reaching.
*Detector:* none.
*Eye:* would a person tire of holding it so?

**H8. A tool swung without weight.**
*Looks:* heavy and light tools swung with the same timing; the head stopping dead at impact; an instant turn at the
top.
*Why:* a heavy head needs a longer path to reach the same speed; after impact some of its momentum returns through the
handle (physics); light hammering shows bell-shaped upswings and a short pause after impact (Petric et al. 2017 [1S,
read in full]); no numbers for heavy tools (gap).
*Measure:* per swing, backswing duration and angle against the tool's inertia; the head's velocity and the arm's give
in the 50 ms after impact; the reversal at the top (S6).
*Bar:* tools differing over two times in inertia swung within 10% of the same timing; no head rebound or arm give
within 50 ms of impact (judgment). Review: at impact both feet planted and braced (no foot in the air, no C1 slide in
the 0.2 s around it) (judgment; see C9).
*Applies:* tool work.
*Detector:* none.
*Eye:* side camera through a swing, light and heavy tools.

**H9. A hand that closes out of time with the grip (added in review).**
*Looks:* the fingers close before the hand reaches the handle, or after the thing is already lifted.
*Why:* errors in the timing of finger motion against the action were noticed by viewers at about 0.1 s and up
(Jörg, Hodgins, O'Sullivan 2010 [1S; whether the figure carries to a watched character's grip is unclear, U]).
*Measure:* per grip, the time between the fingers' closure (H5's contact on the fingers that should touch) and the
first frame the thing's motion is carried by the hand; per release, the reverse.
*Bar:* over 0.1 s either way (study, with the scope caveat).
*Applies:* picking up, hanging back, taking the tool, the lantern.
*Detector:* none; `MinerTests.AFreeHandClosesRoundAHandleAndOpensAgain` (`MinerTests.cs:255`) checks the outcome,
not the timing.
*Eye:* close view of the hand, frame by frame through the grip.

### 3.10 Family Z: scale and body size

**Z1. A tempo that does not fit the body's size.**
*Looks:* a big body stepping at a small body's beat reads small, or wrong; a small one at a big one's beat looks slow
and floaty.
*Why:* observers set a walker's size to go with 1 over stride frequency squared, the pendulum law (Jokisch and Troje
2003 [1S]); adult motion scaled by dynamic-scaling rules reads as a child's (Dong et al. 2017 [1S, details not
checked]); bodies at equal Froude number move alike (Alexander 1976, 1984 [WE]); cadence off by 25% passed and 50% did
not (Handzic and Reed 2015 [1S]).
*Measure:* per miner, dimensionless cadence `f sqrt(L / g)` and dimensionless step `s / L` at each Froude number.
*Bar:* each miner's dimensionless cadence within plus or minus 25% of the human value at the same Froude number
(study for the 25%; physics for the scaling).
*Applies:* walk, turns, steps of every kind; any creator body.
*Detector:* none.
*Eye:* the three miners walking side by side.

**Z2. A walk faster than walking allows.**
*Looks:* a "walk" at a pace that should be a run.
*Why:* people change from walking to running near Froude 0.5 (Kram, Domingo, Ferris 1997 [WE]); walking is impossible
as an inverted pendulum above Froude 1 (physics).
*Measure:* Froude number `v^2 / (g L)` per stride.
*Bar:* a walk above 0.5 flagged; above 1 a fault (study; physics).
*Applies:* walk, hurried walk; any creator body.
*Detector:* none.
*Eye:* the fastest walk of each miner.

**Z3. Speed changed by one knob.**
*Looks:* legs spinning faster with the same steps, or stretching with the same beat.
*Why:* people change speed by lengthening steps and raising cadence together (walk ratio nearly constant above about
1 m/s; Sekiya and Nagasaki 1998; Murakami and Otaka 2017 [WE]); its value was not confirmed [U].
*Measure:* step length over cadence (walk ratio) at several speeds.
*Bar:* walk ratio changing by over 20% between 1.0 and 1.5 m/s equivalent Froude speeds (judgment).
*Applies:* the walk at all paces.
*Detector:* none.
*Eye:* slow and brisk walks side by side.

**Z4. Joints not slowed with size.**
*Looks:* Long as quick in its joints as Small; Small as slow as Long.
*Why:* under dynamic similarity times go with `sqrt(L / g)` and joint speeds with `sqrt(g / L)` (physics); Hill's rule
keeps linear speed equal, so joint speed goes with `1 / L` (Hill 1950 [WE]); real animals lie between [U]; Luis asked
for size-slowed joints (November 3).
*Measure:* each action's duration in body time, per miner.
*Bar:* the same action's duration in body time differing by over 25% between miners (judgment; until question 1 is
answered, Froude scaling).
*Applies:* everything; any creator body.
*Detector:* none.
*Eye:* the three miners doing the same thing.

**Z5. Strength that does not fit the body.**
*Looks:* heavy Round rising as easily as Small; a slight body moving a big load lightly.
*Why:* muscle force grows with cross-section (about mass to the 0.67) while weight grows with mass, so strength per
weight falls with size (Biewener 2005 [WE]); a body whose shape does not match its motion changes what viewers think
it weighed (Kenny et al. 2019 [1S]); mismatched parts are the road to eeriness (Katsyri et al. 2015 [WE as direction]).
*Measure:* per miner and action, the peak share of available joint strength used, and the action's duration in body
time against strength over weight.
*Bar:* a body with strength over weight under 0.7 of another's doing the same action at the same or lower share of
strength (judgment).
*Applies:* getting up, lifting, tool work, catches.
*Detector:* `PhysicalFallTests.AWeakMinerWithAPickaxeTooHeavyGoesDownAndGetsUp` (`PhysicalFallTests.cs:428`).
*Eye:* Round and Small side by side.

**Z6. A human's motion on a body that is not shaped so.**
*Looks:* a creator body with long arms swinging them as a person of ordinary arms does; a big head that does not
weigh.
*Why:* no study of odd proportions was found (gap, U1); the dynamic-similarity rules hold for similar shapes only; the
nearest evidence is the shape-motion mismatch work (Kenny 2019; Yamac 2024 [1S each]).
*Measure:* every per-body quantity in that body's own units (its leg length, arm length, segment masses): arm swing
period against the arm's pendulum period, head motion against its mass.
*Bar:* arm swing period off its pendulum period by over 30% (judgment); others to add as creator bodies come.
*Applies:* any creator body.
*Detector:* none.
*Eye:* odd bodies beside the miners.

### 3.11 Family V: the screen

**V1. The drawn body differs from the physics body.**
*Looks:* a limb drawn where the physics is not: a jump when they meet again; a limb inside the ground that the
physics holds above it.
*Why:* a parent from the interpolated transform and a child from the raw body, or double interpolation, leaves a
one-step offset up to 23 degrees at 20 rad/s (twitches file [U, inference]); the October 30 half-round turns were a
drawing fault, not physics (U3 F14).
*Measure:* per rendered frame, each drawn bone against the pose of its physics body interpolated to the same time.
*Bar:* over 1 cm or 5 degrees (judgment).
*Applies:* everything with the physics body.
*Detector:* none per frame; `TurnsAsSolved` mended one cause.
*Eye:* physics wireframe over the drawn body.

**V2. Drawn motion faster than the physics allows.**
*Looks:* as S3.
*Why:* if the bodies cannot turn faster than their clamp, any drawn bone turning faster between rendered frames is a
render-path fault by construction (twitches file).
*Measure:* per rendered frame, each drawn bone's rotation against the clamp times the frame time.
*Bar:* over the clamp times `dt` plus 2 degrees (judgment). The clamp is confirmed in review: `maxAngularVelocity` 20
rad/s on the own body (`OwnBody.cs:535`) and the fall's bodies (`PhysicalFall.cs:340`), 60 on the physical hands.
*Applies:* everything with the physics body.
*Detector:* none.
*Eye:* as S3.

**V3. Motion that depends on the frame rate.**
*Looks:* faults seen at 120 fps that 30 fps hides (U3 F24), or the reverse.
*Why:* the project's record: 25-30 fps clips hid the hip drop, and the twitches were found at 120 fps (U3 F14, F24).
*Measure:* the same course at frame caps 30, 60, 120 and uncapped, and at a fixed 50 and 100; every kind's count of flags.
*Bar:* any kind whose flag count differs by over 20% between caps, or which flags at one cap and not at another
(judgment).
*Applies:* every bench.
*Detector:* none (suggested in the twitches file).
*Eye:* the same moment at each cap.

**V4. The camera carries the body's jolts.**
*Looks:* the whole screen jumps when the hips do.
*Why:* a follow camera tied to a part passes that part's one-frame jumps to everything.
*Measure:* the camera's position and rotation second difference, as S1.
*Bar:* as S1 for the hips (judgment).
*Applies:* the player's camera in all play.
*Detector:* none.
*Eye:* the player's camera at full rate.

**V5. Skin that stretches or pinches in motion.**
*Looks:* a joint whose mesh collapses or balloons at a bend.
*Why:* the project's model method checks stretched and pinched parts at rest (U3 F23); bends in motion go further.
*Measure:* in poses over 70% of a joint's range, the mesh volume near the joint against rest; edge stretch.
*Bar:* over 20% loss of volume or over 30% edge stretch (judgment).
*Applies:* kneeling, getting up, lying, reaching.
*Detector:* `ModelQualityMethod.md` (at rest).
*Eye:* close views of knees, elbows, hips in deep bends.

**V6. Judder: a 50 Hz body drawn in uneven steps (added in review).**
*Looks:* smooth motion that stutters in a steady beat, the body moving two steps then one, at 60 or 120 fps.
*Why:* the physics runs at 50 steps a second; at 60, 120 or 144 fps a body not interpolated is drawn advancing 0 or 1 steps
per frame in a beating pattern (physics of sampling; PhysicalFall's bodies interpolate, `PhysicalFall.cs:336`;
whether every articulation body and every drawn bone does is not checked [U]).
*Measure:* per rendered frame, the drawn hips' (and each physics-driven bone's) displacement divided by `dt`, against
the same body's motion interpolated; the share of frames with no displacement while the body moves over 0.2 m/s.
*Bar:* any frame with zero displacement, or a displacement off the interpolated one by over 30%, while the body moves
over 0.2 m/s (judgment).
*Applies:* everything with the physics body, at every frame cap (with V3).
*Detector:* none.
*Eye:* the player's camera at 60 and 144 fps on a steady walk.

**V7. The camera inside the body or the ground (added in review).**
*Looks:* the view cuts through a miner's head or coat, or dips under the ground or into the rock.
*Why:* clipping by the camera shows the inside of a body and breaks its solidity, as C7 does for limbs (craft [U]).
*Measure:* per rendered frame, the camera's near-plane rectangle against every body's and the world's colliders.
*Bar:* any frame with the near plane inside a collider (judgment).
*Applies:* the player's camera in all play, above all near walls, lying bodies and crowds.
*Detector:* none.
*Eye:* the player's camera close to walls and miners.

### 3.12 Count

| Family | Kinds | Ids |
|---|---|---|
| C, contact with the ground and the world | 13 | C1-C13 |
| B, balance | 8 | B1-B8 |
| T, timing, speed and effort | 11 | T1-T11 |
| S, smoothness | 10 | S1-S10 |
| J, joints | 7 | J1-J7 |
| O, coordination | 13 | O1-O13 |
| W, weight, cause and reaction | 14 | W1-W14 |
| L, life and variation | 10 | L1-L10 |
| H, held and hung things, secondary motion | 9 | H1-H9 |
| Z, scale and body size | 6 | Z1-Z6 |
| V, the screen | 7 | V1-V7 |
| **All** | **108** | |

Twelve kinds were added in review (C12, C13, T11, S10, O13, W12, W13, W14, L10, H9, V6, V7); their bars are
judgment around the studies named at each, except H9 (study, scope unclear). The paragraph below counts the
original 96. Of the 96 bars, those resting on a study (or a study's number with a judgment margin): C1, J1, T1, T2, T7, Z1, Z2,
L2, L5, L6 (rate), B7 (speed), and the direction (not the number) of B1, B4, T6, O1, O3, O5, W1, W7, W8, H3. On
physics: C8, B6, W1, W2, W4 (none allowed), W5 (lifted by nothing), H1, H6 (combined centre of mass), Z2 (above 1). On
Luis's word: W4, W5, W8, L1 (floor only), L6 (size), L9, H1, B8 (that settling happens). On the project's own earlier
bars: C2, C4, C5, C10, C11, S1, S2, S3, J4 (`MostTwist`), J5, H2, H5. Everything else is judgment and is marked so at
the bar. A judgment bar is a starting point to be set before the first run and kept; it is not evidence.

---

## 4. The judges' checklist

For judge agents shown pictures (contact sheets of every frame of a flagged stretch and of an unflagged stretch, at
least two cameras, the physics wireframe over the drawn body where asked). Each question is answered yes, no or cannot
tell, with the frame numbers. The judges are not told what the scan found.

**Contact (C).** Does each standing foot stay where it was put? Does it turn on the spot when it should not? Is any
foot, hand or knee hovering, sunk, or through something? Does a foot roll heel to toe as it walks? Do the legs pass
through each other? Seen from the front, do the feet land on a line or far apart? Does the swinging foot scuff the
ground or lift like a march?

**Balance (B).** Would a person standing like this fall? Does each step have a reason you can see (a push, a goal, a
shift)? After a catch, does the body settle into a comfortable stance, or stay frozen where it caught itself? Does
weight move onto the standing foot before the other lifts?

**Timing (T).** Is the pace right for the size of this body? Does it answer too soon or too late? Does a movement
speed up and slow down, or run at one speed? Does it slow into a sharp turn? Does anything move faster than a person
could, or slower than the job needs? Does a walk start or stop all at once, in a single step?

**Smoothness (S).** Is there any jump, flick, shake or pop, even for one frame? Does a part snap to a pose? Does
anything turn the long way round? Does a swinging hand, foot or tool tip move on a curve, or along a ruler?

**Joints (J).** Is any joint bent or twisted in a way that would hurt? Does a knee point away from its foot? Does any
limb look broken or detached?

**Coordination (O).** Does the head lead a turn, then the shoulders, then the hips? Do the arms swing against the
legs? Are both arms or legs doing exactly the same thing (twinning)? Is it done in parts with pauses? Do loose parts
follow through after a stop? Does a strike come from the body or only from the arm? Does the body look where it
reaches? Seen from above, does the trunk stay quiet in a walk, or wag and corkscrew?

**Weight and cause (W).** Does it fall as things fall? Does each rise begin from something the body did, or from a
timer? Is anything lifted or turned that nothing pushes? Does the body go where the push sent it, and does a bigger push
give a smoothly bigger answer? While down, is it alive? Does a landing take the weight? When balance goes, do the arms
come up? In a fall, do the hands go out to meet the ground? Seen from the side, does the walk glide on rails, bounce,
or crouch?

**Life (L).** At ease, does it look alive (not frozen, not shaking)? Do its shifts and fidgets come at uneven times?
Does any movement repeat exactly? Is the breath gentle? Does a tired miner move like a tired one? Do ten get-ups look
like ten different attempts? Does the life go out of the body when it starts to walk or work?

**Held things (H).** Is every object held, hung, or resting on something, never floating? Does it swing, lag and
settle like a real thing of its weight? Do fingers meet the handle? Does a heavy load show in the posture? Could a person
keep this pose up? Does a heavy tool swing like a heavy tool? Do the fingers close as the hand arrives, not before
or after? Do both hands stay on a two-handed handle? Does a swinging thing die down, never swing up by itself?

**Size (Z).** Do the three miners each move at the pace of their size? Does Round's strength look like Round's? Does an
odd creator body move like its own shape, not like a person's?

**The screen (V).** Do the drawn body and the physics body agree? Does the same moment look the same at 30, 60 and 120
frames a second? Does the camera shake when the body jolts? Does the view ever cut into a body, a wall or the ground?
(Judder, V6, cannot be judged on still sheets: it needs the clip played at 60 and 144 fps, or the scan's numbers.)

**For everything, last.** Would a viewer say "that movement makes sense" (Luis, October 8)? Name the one frame that
looks least natural, and why.

---

## 5. Luis's own flags

Each of Luis's flags in the project record (U3, table L01-L25) mapped to the kinds that catch it. Quotes are Luis's
own, short.

| Flag | Date | What Luis saw | Kinds |
|---|---|---|---|
| L01 | Oct 1 | "always shuffles a bit its feet" at the spot | B2 |
| L02 | Oct 1 | pickaxe "just sitting behind his back" | H1, H2 |
| L03 | Oct 3 | the lamp floating and clipping her hand; legs not meeting the boots | H1, H2, H5, J7 |
| L04 | Oct 3 | objects should respond to the world, not float | H1, H3 |
| L05 | Oct 8 | a light stutter after each step, "teleported" to the ground | S1, C4, C5 |
| L06 | Oct 8 | legs "teleport" when turning | S1, S3, C11, J4, O1, O2 |
| L07 | Oct 8 | clothes "exploding" when kneeling | H4 |
| L08 | Oct 8 | kneeling "in parts" | O7, O9, T4 |
| L09 | Oct 8 | the recovery "just an animation" | W4, W5 |
| L10 | Oct 8 | stopped recovering, "just still" | T9, L1 |
| L11 | Oct 8 | holding the lantern "a bit uncanny" | H7, O11 |
| L12 | Oct 8 | no fake animations; movement that makes sense | all; the checklist's last question |
| L13 | Oct 9 | "natural and alive rather than robotic" | W6, L1, W5 |
| L14 | Oct 10 | the breath "a bit too strong" | L6 |
| L15 | Oct 10 | still does not get up naturally | W5 |
| L16 | Oct 13 | lying until a timer, then a "magical" rise; convulsing | W5, W6, S4 |
| L17 | Oct 13 | many shoves: "whacky buggy physics" | W3, W11, T8 |
| L18 | Oct 13 | no switch at a threshold, a smooth transition | W8 |
| L19 | Oct 22 | assistance only as "absolute last resort" | W4, B5 |
| L20 | Oct 30 | "very aggressive twitches", arms that look broken; "goofy"; "so long to get up" | S2, S3, J1, J2, T8 (and its clamp check), V1, V2, V3; T1 (question 3) |
| L21 | Nov 3 | is the stance rise "too robotic or repetitive"? | L8, L4 |
| L22 | Nov 9 | "natural variability" in resting limbs | L9, L5 |
| L23 | Nov 13 | no readjusting after recovering; no shifting so as not to feel stiff | B8, L1, L2, L10 |
| L24 | Nov 13 | odd leg positions and rare twitches while turning; "get both to 0" | J4, J5, C11, S2, S3, T7, O1 |
| L25 | Nov 13 | a leg looking detached while getting up | J7, V1 |

Every flag maps to at least one kind with a per-frame measure, except L12, which is the purpose of the whole
catalogue.

---

## 6. Gaps and disagreements

**Disagreements to settle (not settled here).**
1. **The hip's twist in a turn (J4).** The walk allows `MostTwist = 62` degrees (`ProceduralBiped.cs:480`), and
   `TurnBench` F1 fails only past 64. People's hips turned 31-35 degrees in 90-degree turns (Steingrebe et al. 2024,
   turning file [1S]). The catalogue flags over 35 and faults over 45, so both bars stand and the gap is shown, not
   hidden. Lowering `MostTwist` is a design step for Luis, not for this page.
2. **The foot's slide (C1).** `TurnBench` F5 fails a slide over 2 cm a stance. The study's "under 21 mm" is the size of
   slides that were still seen, not a safe size (Prazak, Hoyet, O'Sullivan 2011). The catalogue flags 1 cm and fails
   2 cm; F5's own bar should be 1 cm to agree. Since F5's bar was set before its first run, changing it needs this
   written reason, which does not use any run's numbers.
3. **Slide speed (C1).** The project's 1 mm a frame at 100 fps (0.1 m/s, `motion_breaks.py:26`) is stricter than
   NVIDIA's Kimodo benchmark (0.2 m/s near the ground). The project's stays.
4. **Smooth is not minimum jerk (T4, S5).** Minimum-jerk profiles fit reaching, but a smoother (lower jerk) robot was not
   always liked more (Hirose et al. 2020 [1S]). Low jerk is not a target; only spikes and constant speed are flagged.
5. **More than the push (W7).** Viewers accept a slightly larger reaction (Hoyet 2012); a physics body should not give
   one. This is Luis's question 4.
6. **Size and speed (Z4, T7, T8).** Pendulum scaling and Hill's scaling disagree by a factor of `sqrt(L)`; real animals
   lie between. Luis's question 1.
7. **A hinge past straight (J1), added in review.** `FrameRecorder` calls an elbow or knee "wrongway" past 8 degrees
   backwards; J1 says 5 (normal extension 0-5, CDC). The scan uses 5; `FrameRecorder`'s 8 stays as it is until
   someone decides.
8. **The hip twist is not the study's quantity (J4), added in review.** See J4: pelvis yaw against the planted foot
   contains knee and shank rotation; the study measured the hip alone.

**Gaps (no evidence found, or not checked).**
1. No threshold study found for: a hand or knee hovering (C9), self-penetration (C10), joint flicks in a single frame
   (S2, S3), hip twist seen as wrong (J4), coat or strap motion (H3, H4), heavy tool swings (H8), whether perfect
   symmetry itself looks artificial (L3), odd body proportions (Z6), and drawn-against-physics offsets (V1). Their bars
   are judgment.
2. No study gives a joint speed people do not exceed in a task; U3's "5 rad/s" was struck, and T8's per-task caps are
   judgment to set on people's motion capture before the first run.
3. Numbers not re-verified because the papers would not open as text: the 60 ms of O'Sullivan et al. 2003 (T3 uses
   40 ms, from the safe end), and Vicovaro et al. 2014's percentages (not used as bars).
4. The breath's "0.5 degrees" body motion (Hodges 2002, via U2) could not be confirmed; L6 keeps Luis's approved size
   instead.
5. The "human peak/mean speed about 1.75" (U2) could not be confirmed; T4's 1.3 floor does not depend on it.
6. Kobayashi 2019 gives no number for bodies seen at wrong speeds; T1 rests on Handzic and Reed's cadence figures.
7. The aesthetic guide the judges use is a PDF not in the repository; the judges' checklist above does not replace
   it.
8. (Corrected in review.) Where each detector lives: `FrameRecorder.cs`, `ArticulationFactsTests.cs` and
   `Docs/Technical/AnyBody/tools/shoves.sh` are in `D:/Dev/WG` only (lines now cited from there, read-only);
   `motion_jolts.py`, `motion_dips.py` and `motion_frames.py` ARE in this working copy's `Art/Review` and missing from
   `D:/Dev/WG`; `TurnBench` is in no repository copy (a reviewed draft in a session scratchpad, bars in
   `Docs/Handoff.md:526-530`); `framescan.py` was not looked for in review. Benches run in `D:/Dev/WG` cannot use the
   three scripts until they are carried there, and `TurnBench` must be committed before its F1-F6 can be cited.
9. L21 (Luis's "robotic or repetitive" watch on the stance rise) has no recorded check; L8 is its first measure.
10. Shadows (a foot's shadow not meeting the foot) are left to the eye under C4; no separate kind.
11. Most kinds (about 80 of 108) have no detector in the game yet. The scan is to be written; this page is its
    specification. It also needs a recorder the game does not have (contacts, masses, centre of mass, drive forces,
    mode-change log, section 3.0).
12. All perception studies are of human bodies, mostly seen as point lights or realistic figures; the miners are
    stylised. Luis's question 5.

**Small samples.** Several bars rest on one study with few people: La Scaleia (16), Kobayashi (10), Duarte and
Zatsiorsky (10), Handzic and Reed (small), Steingrebe (one group of healthy older people as reference). Each is marked
[1S].

---

## 7. Check log

The sceptic's check of every load-bearing claim (K01-K33) is in `sceptic_notes.md`, with the source re-opened for each
and the final status table in its section "Second run". In short:

| Outcome | Claims |
|---|---|
| Confirmed | K02, K04, K05, K06, K08-K11, K13-K16, K20, K22-K24, K26-K30 |
| Corrected | K01 (21 mm is a size still seen, not a safe size: bar 1 cm, fail 2 cm); K23 (Troje: cats and pigeons were the stimuli, not the observers); K25 (Duarte and Zatsiorsky: irregular timing holds for shifts and drifts, spread over mean 0.5-0.9, not for fidgets, 0.25-0.33); K14 (Hof's `l` taken as centre of mass height, a judgment; a brief negative margin is recoverable) |
| Marked down | K03 (O'Sullivan's 60 ms not re-verified); K07 (Vicovaro's numbers not re-verified, direction confirmed); K17 (direction only); K19 (Hodges's 0.5 degrees unclear); K21 (arithmetic 1.875 confirmed, "human 1.75" unclear) |
| Demoted | K12 (GMD's 2.5 cm a frame, about 0.5 m/s, is a model benchmark's convention, not a bar) |
| Struck | K31 (U3's "a person rarely above 5 rad/s" as a general bar) |
| Noted | K18 (9.2 mm/s one study; sway spectrum 0.30-0.45 Hz from Soames and Atha); K32 (line numbers: this page cites the main working copy, branch `claude/worker-showcase`, November 13; U3's are from `D:/Dev/WG`); K33 (small samples) |

Detector lines cited in section 3 were read in this working copy on November 13: `OwnBodyTests.cs:93, :141, :299,
:417, :501, :527, :764`; `PhysicalFallTests.cs:85, :158, :320, :428`; `MinerTests.cs:157, :208, :255, :345`;
`HungThingTests.cs:62, :199, :254, :327, :397`; `GroundedBodyTests.cs:142`; `PhysicalFall.cs:379`;
`ProceduralBiped.cs:480`; `motion_breaks.py:24-28`. An independent review re-read all 21 test names by search the same
day: every line is right in this copy; for `D:/Dev/WG` see the offsets in section 3.0.

---

## 8. Sources

Mark after each: what was read (full, abstract, secondary) as the researchers and the sceptic recorded it. Fuller
entries are in `U1_perception.md` section 10 and `U2_craft_measures_biomechanics.md` "Sources".

**Perception of motion and its errors**
- Prazak M., Hoyet L., O'Sullivan C. 2011. Perceptual evaluation of footskate cleanup. SCA 2011.
  https://diglib.eg.org/handle/10.2312/SCA.SCA11.287-294 (abstract)
- Reitsma P., Pollard N. 2003. Perceptual metrics for character animation: sensitivity to errors in ballistic motion.
  ACM TOG 22(3). https://ri.cmu.edu/?p=8709 (abstract)
- Reitsma P., Andrews J., Pollard N. 2008. Effect of character animacy and preparatory motion on perceptual magnitude
  of errors in ballistic motion. CGF 27(2). https://diglib.eg.org/handle/10.2312/CGF.v27i2pp201-210 (abstract)
- O'Sullivan C., Dingliana J., Giang T., Kaiser M. 2003. Evaluating the visual fidelity of physically based
  animations. ACM TOG 22(3). https://gv2.scss.tcd.ie/cosulliv/Pubs/fidelity.pdf (read by U1; not re-opened)
- Vicovaro M., Hoyet L., Burigana L., O'Sullivan C. 2014. Perceptual evaluation of motion editing for realistic
  throwing animations. ACM TAP. https://studios.disneyresearch.com/wp-content/uploads/2019/03/Perceptual-Evaluation-of-Motion-Editing-for-Realistic-Throwing-Animations-Paper.pdf
  (read by U1; not re-opened)
- Hoyet L., McDonnell R., O'Sullivan C. 2012. Push it real: perceiving causality in virtual interactions. ACM TOG
  31(4):90. https://history.siggraph.org/?p=110569 (abstract)
- Hodgins J., Jorg S., O'Sullivan C., Park S. I., Mahler M. 2010. The saliency of anomalies in animated human
  characters. ACM TAP 7(4). (citing papers)
- Jokisch D., Troje N. 2003. Biological motion as a cue for the perception of size. Journal of Vision 3(4).
  https://doi.org/10.1167/3.4.1 (abstract)
- Troje N., Westhoff C. 2006. The inversion effect in biological motion perception: evidence for a "life detector"?
  Current Biology 16(8). (abstract)
- Troje N., Chang D. 2023. Life detection from biological motion. Current Directions in Psychological Science 32(1).
  https://pmc.ncbi.nlm.nih.gov/articles/PMC9975895 (full)
- Viviani P., Stucchi N. 1992. Biological movements look uniform. J. Exp. Psych.: HPP 18. (secondary)
- Handzic I., Reed K. 2015. Perception of gait patterns that deviate from normal and symmetric biped locomotion.
  Frontiers in Psychology 6:199. https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2015.00199/full (full)
- Saygin A. P., Chaminade T., Ishiguro H., Driver J., Frith C. 2012. The thing that should not be. SCAN 7(4).
  https://pmc.ncbi.nlm.nih.gov/articles/PMC3324571/ (abstract)
- Katsyri J., Forger K., Makarainen M., Takala T. 2015. A review of empirical evidence on different uncanny valley
  hypotheses. Frontiers in Psychology 6:390. https://www.frontiersin.org/articles/10.3389/fpsyg.2015.00390/full (abstract)
- McDonnell R., Larkin M., Dobbyn S., Collins S., O'Sullivan C. 2008. Clone attack! Perception of crowd variety. ACM
  TOG 27(3). https://research.gold.ac.uk/id/eprint/15203 (abstract)
- Prazak M., O'Sullivan C. 2011. Perceiving human motion variety. APGV 2011. https://doi.org/10.1145/2077451.2077468
  (citing paper)
- McDonnell R., Newell F., O'Sullivan C. 2007. Smooth movers: perceptually guided human motion simulation. SCA 2007.
  https://diglib.eg.org/handle/10.2312/SCA.SCA07.259-270 (abstract)
- Harrison J., Rensink R., van de Panne M. 2004. Obscuring length changes during animated motion. ACM TOG 23(3).
  https://www.cs.ubc.ca/labs/imager/tr/2004/harrison-ea-2004-obscuring (abstract)
- Kobayashi M. 2019. Perceiving natural speed in natural movies. i-Perception.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC6643185/ (full)
- Chaminade T., Hodgins J., Kawato M. 2007. Anthropomorphism influences perception of computer-animated characters'
  actions. SCAN 2(3). https://pmc.ncbi.nlm.nih.gov/articles/PMC2569803 (full)
- Jorg S., Hodgins J., O'Sullivan C. 2010. The perception of finger motions. APGV 2010. (abstract)
- Thornton I., Wootton Z., Pedmanson P. 2014. Matching biological motion at extreme distances. Journal of Vision
  14(3):13. https://www.um.edu.mt/library/oar/bitstream/123456789/27022/1/Matching_biological_motion_at_extreme_distances_2014.pdf (abstract and introduction)
- Dong et al. 2017 (authors and title not checked). Dynamic scaling of adult motion to child size. MIG 2017.
  https://doi.org/10.1145/3136457.3136460 (search summary)
- Michotte A. 1946/1963. The Perception of Causality; via review https://pmc.ncbi.nlm.nih.gov/articles/PMC3708160 (secondary)
- Hamilton A., Joyce D., Flanagan J. R., Frith C., Wolpert D. 2007. Kinematic cues in perceptual weight judgement and
  their origins in box lifting. Psychological Research 71(1). https://www.antoniahamilton.com/HamiltonJoyceFlanaganFrithWolpert_PsyRes2005.pdf (abstract)
- Runeson S., Frykholm G. 1981. Visual perception of lifted weight. J. Exp. Psych.: HPP 7. (secondary)
- Tremoulet P., Feldman J. 2000. Perception of animacy from the motion of a single object. Perception 29(8).
  https://journals.sagepub.com/doi/10.1068/p3101 (abstract)
- Pratt J., Radulescu P., Guo R. M., Abrams R. 2010. It's alive! Animate motion captures visual attention.
  Psychological Science 21(11). https://pubmed.ncbi.nlm.nih.gov/20974713/ (abstract, via U1)
- Gielniak M., Liu C. K., Thomaz A. 2011. Task-aware variations in robot motion. ICRA 2011.
  https://utw10730.utweb.utexas.edu/static/papers/gielniak11_icra_tav.pdf (summary)
- Bodenheimer B., Shleyfman A., Hodgins J. 1999. The effects of noise on the perception of animated human running
  (title not confirmed). Eurographics CAS 1999. https://eecs.vuse.vanderbilt.edu/People/bobbyb/pubs/variability99.pdf (summary)
- La Scaleia B., Ceccarelli F., Lacquaniti F., Zago M. 2020. Visuomotor interactions and perceptual judgments in
  virtual reality simulating different levels of gravity. Front. Bioeng. Biotechnol. 8:76.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC7039824 (full)
- Wang J., Bodenheimer B. 2004. Computing the duration of motion transitions: an empirical approach. SCA 2004.
  https://eecs.vuse.vanderbilt.edu/People/bobbyb/pubs/sca2004wb.html (abstract)
- Kenny S. et al. 2019. Perceptual effects of inconsistency in human animations. ACM TAP.
  https://is.tuebingen.mpg.de/publications/kenny-tap-2019 (search summary)
- Yamac G., O'Sullivan C., Neff M. 2024. Understanding the impact of visual and kinematic information on the perception
  of physicality errors. ACM TAP. https://www.cs.ucdavis.edu/~neff/papers/PerceptionOfPhysicalityErrors_TAP_2024.pdf
  (abstract, discussion, conclusion)
- Hirose J., Nishikawa A., Horiba Y., Inui S., Pataky T. 2020. Integrated jerk as an indicator of affinity for
  artificial agent kinematics. PeerJ 8:e9843. https://pmc.ncbi.nlm.nih.gov/articles/PMC7500322/ (abstract)
- Treal T., Jackson P. L., Jeuvrey J., Vignais N., Meugnot A. 2021. Natural human postural oscillations enhance the
  empathic response to a facial pain expression in a virtual character. Scientific Reports 11:12493. (abstract, via
  the turning file)
- Atxa Landa et al. 2025. Evaluating idle animation believability: a user perspective. https://arxiv.org/abs/2509.05023 (abstract)

**Animation craft**
- Thomas F., Johnston O. 1981. The Illusion of Life: Disney Animation. Book.
- Lasseter J. 1987. Principles of traditional animation applied to 3D computer animation. SIGGRAPH Computer Graphics
  21(4). https://www.evl.uic.edu/aej/527/papers/lasseter.pdf (abstract and index)
- Williams R. 2001. The Animator's Survival Kit ("Symmetry or Twinning"). Book (contents seen).
- Animation Mentor. Why all animators need to master the moving hold.
  https://animationmentor.com/blog/why-all-animators-need-to-master-the-moving-hold (practice)
- Johansen R. S. 2009. Automated semi-procedural animation for character locomotion (thesis).
  https://runevision.com/thesis/rune_skovbo_johansen_thesis.pdf (via the turning file)
- Knee pops and soft IK, practice threads. https://escapestudiosanimation.blogspot.com/2023/11/how-to-fix-knee-pops-in-walk-cycle.html (practice)

**Measures from motion generation and simulation**
- Kovar L., Schreiner J., Gleicher M. 2002. Footskate cleanup for motion capture editing. SCA 2002.
  https://research.cs.wisc.edu/graphics/Gallery/kovar.vol/Cleanup (abstract)
- Ren L., Patrick A., Efros A., Hodgins J., Rehg J. 2005. A data-driven approach to quantifying natural human motion.
  ACM TOG 24(3). https://www.ri.cmu.edu/publications/a-data-driven-approach-to-quantifying-natural-human-motion (abstract)
- Karunratanakul K. et al. 2023. Guided motion diffusion for controllable human motion synthesis. ICCV 2023.
  https://ar5iv.labs.arxiv.org/html/2305.12577 (metric section)
- NVIDIA. Kimodo benchmark metrics. https://research.nvidia.com/labs/sil/projects/kimodo/docs/benchmark/metrics.html (full)
- Yuan Y. et al. 2023. PhysDiff: physics-guided human motion diffusion model. https://arxiv.org/pdf/2212.02500 (via the
  twitches file)
- Yuan Y., Kitani K. 2020. Residual force control for agile human behavior imitation and extended motion synthesis.
  NeurIPS. https://arxiv.org/abs/2006.07364 (abstract)
- Liu C. K., Popovic Z. 2002. Synthesis of complex dynamic character motion from simple animations. ACM TOG 21(3).
  https://grail.cs.washington.edu/projects/charanim/phylter.html (abstract)
- Tripathi S. et al. 2023. 3D human pose estimation via intuitive physics. CVPR 2023.
  https://openaccess.thecvf.com/content/CVPR2023/papers/Tripathi_3D_Human_Pose_Estimation_via_Intuitive_Physics_CVPR_2023_paper.pdf (abstract)
- Han X. et al. 2023. GroundLink. SIGGRAPH Asia. https://cs-people.bu.edu/xjhan/pubs/GroundLink.pdf (abstract)
- Liu S. et al. 2023. ContactGen. https://arxiv.org/abs/2310.03740 ; Jiang H. et al. 2021. Hand-object contact
  consistency reasoning for human grasps generation. https://arxiv.org/abs/2104.03304 (search summaries)
- Balasubramanian S. et al. 2015. On the analysis of movement smoothness. JNER.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC4674971 (via the twitches file)

**Balance, gait and scaling**
- Hof A. L., Gazendam M., Sinke W. 2005. The condition for dynamic stability. J Biomech 38.
  https://braceworks.ca/wp-content/uploads/2016/05/hof-condition-for-dynamic-stability.pdf (abstract)
- Curtze C., Buurke T., McCrum C. 2024. Notes on the margin of stability. J Biomech.
  https://cris.maastrichtuniversity.nl/en/publications/notes-on-the-margin-of-stability/ (abstract)
- Herr H., Popovic M. 2008. Angular momentum in human walking. J Exp Biol 211.
  https://aspirin.media.mit.edu/biomechatronics/wp-content/uploads/sites/8/2013/07/Herr-2008_Angular-momentum-in-human-walking.pdf (abstract)
- McIlroy W., Maki B. 1993 and later. Compensatory stepping onset.
  https://experts.nau.edu/en/publications/anticipatory-postural-adjustments-and-the-latency-of-compensatory/ (secondary)
- McIlroy W. E., Maki B. E. 1997. Preferred placement of the feet during quiet stance. Clinical Biomechanics 12(1).
  (abstract, via the turning file)
- Alexander R. McN. 1976 and 1984 (dynamic similarity), as quoted in Ruiz and Torices 2013.
  https://www.originalwisdom.com/wp-content/uploads/bsk-pdf-manager/2019/03/Ruiz-and-Torices_2013_Humans-running-at-stadiums-and-beaches-and-the-accuracy-of-speed-estimation-from-fossil-trackways.pdf (secondary)
- Kram R., Domingo A., Ferris D. 1997. Effect of reduced gravity on the preferred walk-run transition speed. J Exp
  Biol. https://www.sfu.ca/locomotionlab/assets/dynsimwalk1997.pdf (abstract)
- Hof A. L. 1996. Scaling gait data to body size. Gait Posture 4; formulas at
  https://clinicalgaitanalysis.com/faq/normalisation.html (secondary)
- Sekiya N., Nagasaki H. 1998 (walk ratio); Murakami R., Otaka Y. 2017. Estimated lower speed boundary at which the
  walk ratio constancy is broken. J Phys Ther Sci. https://pmc.ncbi.nlm.nih.gov/articles/PMC5430280 (abstract)
- Collins S., Adamczyk P., Kuo A. 2009. Dynamic arm swinging in human walking. Proc R Soc B 276.
  https://pmc.ncbi.nlm.nih.gov/articles/PMC2817299 (full)
- Pozzo T., Berthoz A., Lefort L. 1990. Head stabilization during various locomotor tasks in humans. Exp Brain Res
  82. (secondary)
- Axial trunk rotations review, J Hum Kinet 2021. https://pmc.ncbi.nlm.nih.gov/articles/PMC7877281 (abstract)
- Flash T., Hogan N. 1985. The coordination of arm movements. J Neurosci 5. (secondary)
- Steingrebe H., Sell S., Stein T. 2024. Mobility requirements and joint loading during straight walking and 90 degree
  turns in healthy older people and those with hip osteoarthritis. J Clin Med 13(17):5021.
  https://www.ebi.ac.uk/europepmc/webservices/rest/PMC11396374/fullTextXML (full, via the turning file)
- Khobkhun F., Hollands M., Richards J. 2021. The effect of different turn speeds on whole-body coordination in younger
  and older healthy adults. Sensors 21(8):2827. https://knowledge.lancashire.ac.uk/id/eprint/37496/1/sensors-21-02827.pdf (full, via the turning file)
- Khobkhun F., Hollands M., Richards J. 2022. A comparison of turning kinematics at different amplitudes during
  standing turns. Appl Sci 12(11):5474. https://knowledge.lancashire.ac.uk/42227/3/applsci-12-05474-v2.pdf (full, via the turning file)
- Madrid J. et al. 2024. Slowing gait during turning. Front Hum Neurosci 18:1269772. (via the turning file)
- Laudani L. et al. 2021. Age differences in anticipatory and executory mechanisms of gait initiation following
  unexpected balance perturbations. Eur J Appl Physiol 121(2). (via the turning file)
- Jeon W., Wang S., Bhatt T., Westlake K. P. 2022. Perturbation-induced protective arm responses. Brain Sciences
  12(7):953. https://www.ebi.ac.uk/europepmc/webservices/rest/PMC9313371/fullTextXML (full, via the turning file)

**Muscle, size, load, tiredness, tools, standing**
- Hill A. V. 1950. The dimensions of animals and their muscular dynamics.
  https://www.nature.com/nature/journal/v164/n4176/abs/164820b0.html (abstract)
- Biewener A. 2005. Biomechanical consequences of scaling. J Exp Biol 208. (abstract)
- Harman E. et al. US Army load carriage biomechanics. https://apps.dtic.mil/sti/pdfs/ADA377886.pdf (abstract)
- Fatigue and gait systematic reviews: PLoS ONE 2019 https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6936857/ ;
  Bioengineering 2025 https://www.ncbi.nlm.nih.gov/pmc/articles/PMC11939146/ (abstracts)
- Petric T., Simpson C., Ude A., Ijspeert A. 2017. Hammering does not fit Fitts' law. Front Comput Neurosci 11:45.
  https://frontiersin.org/articles/10.3389/fncom.2017.00045/full (full)
- Posturographic standards for optimal control of human standing posture. J Hum Kinet.
  https://jhk.termedia.pl/Posturographic-Standards-for-Optimal-Control-of-Human-Standing-Posture,159452,0,2.html (abstract)
- Winter D. et al. 1998. Stiffness control of balance in quiet standing. J Neurophysiol.
  https://pubmed.ncbi.nlm.nih.gov/9744933/ (abstract)
- Soames R., Atha J. 1982. The spectral characteristics of postural sway behaviour. Eur J Appl Physiol 49. (abstract,
  via search)
- Hodges P. et al. 2002. Coexistence of stability and mobility in postural control: evidence from postural
  compensation for respiration. Exp Brain Res. https://link.springer.com/doi/10.1007/s00221-002-1040-x (abstract)
- NCBI Bookshelf NBK365, respiratory rate. https://ncbi.nlm.nih.gov/books/NBK365 (secondary)
- Duarte M., Zatsiorsky V. M. 1999. Patterns of center of pressure migration during prolonged unconstrained standing.
  Motor Control 3(1). (abstract, via the turning file)
- Prado-Rico J. M., Duarte M. 2019. Asymmetry of body weight distribution during quiet and relaxed standing tasks. Motor
  Control 23(4). (abstract, via the turning file)
- Rekant J. S., Wiltman S. A., Chambers A. J. 2019. A novel method of analysis for prolonged-standing data. IISE Trans
  Occup Ergon Hum Factors 7(2). https://pmc.ncbi.nlm.nih.gov/articles/PMC9976808 (via the turning file)
- Wiggermann N., Keyserling W. M. 2013. Standing comfort and shifting (via a NIOSH report).
  https://stacks.cdc.gov/view/cdc/224976 (secondary)
- Daffin L., Innes S., Stuelcken M. C. 2024. An investigation of the reproducibility of a self-selected natural feet
  position when standing. Musculoskelet Sci Pract 69:102896.
  https://knowledge.lancashire.ac.uk/50420/1/1-s2.0-S2468781223001819-main.pdf (full, via the turning file)
- Thoomes-de Graaf M. et al. 2020. Normative values of cervical range of motion: a systematic review. Musculoskelet Sci
  Pract. https://epublications.regis.edu/facultypubs/98 (abstract)
- Soucie J. et al. 2011. CDC Joint Range of Motion study. https://archive.cdc.gov/www_cdc_gov/ncbddd/jointrom/index.html
  (via the twitches file)

**Added in review** (searched November 13; what was read is marked)
- Donelan J. M., Kram R., Kuo A. D. 2001. Mechanical and metabolic determinants of the preferred step width in human
  walking. Proc R Soc B 268. https://pmc.ncbi.nlm.nih.gov/articles/PMC1088839 (abstract, via search) [1S] (C12)
- Winter D. A. 1992. Foot trajectory in human gait: a precise and multifactorial motor control task. Phys Ther 72.
  (secondary: 1.29 cm cited by later papers) [U] (C13)
- Minimum toe clearance values, about 0.85-3.5 cm by method: e.g. Frontiers in Public Health 2021,
  https://www.frontiersin.org/articles/10.3389/fpubh.2021.612064/pdf; J Neuroeng Rehabil 2015,
  https://jneuroengrehab.biomedcentral.com/track/pdf/10.1186/s12984-015-0052-2 (search summaries only) [U] (C13)
- Gait initiation, about three steps to steady speed: several small studies, e.g. "Gait initiation: the first four
  steps in adults aged 20-25, 65-79 and 80-91 years", https://www.pubmed.gov/24074729/; Darmstadt slope study,
  https://tuprints.ulb.tu-darmstadt.de/23609/1/s41598-021-01694-5.pdf (search summaries only) [U] (T11)
- Hsiao E. T., Robinovitch S. N. 1998. Common protective movements govern unexpected falls from standing height. J
  Biomech 31(1):1-9. https://scilit.com/publications/1422f5a5a063022c36b60e8b287ae335 (abstract) [1S] (W13)
- Orendurff M. S. et al. 2004. The effect of walking speed on center of mass displacement. J Rehabil Res Dev 41(6):
  829-834. Read through a citing paper: https://www.seas.upenn.edu/~posa/DynamicWalking2020/672-997-1-RV.pdf [1S] (W14)
- Herr and Popovic 2008 (O13), Jeon et al. 2022 (W12), Jorg, Hodgins, O'Sullivan 2010 (H9) and Flash and Hogan 1985
  (S10) are listed above.

**The project's own record** (U3, read in this working copy unless marked)
- Luis's words: `Correspondence/` (dates as in section 5) and the memories cited.
- `Docs/Handoff.md` (TurnBench bars F1-F6); `Docs/Research/2026-11-13_TurningAndSettling/research.md` (the turning
  file); the twitches research and the flying-boot review cited by U3.
- Tests and tools at the lines listed in section 7.


---

## Review

An independent review, November 13, by an agent that did not write this page. It read the catalogue,
`sceptic_notes.md`, U1, U2 and U3, and checked the detector claims read-only in this working copy and in
`D:/Dev/WG`. Working notes: `review_notes.md`. U1, U2, U3 and `sceptic_notes.md` were not changed. Nothing here is
Game Director acceptance, and no bar has been run.

**Added (12 kinds).** C12 step width (Donelan 2001); C13 toe clearance, marching and scuffing; T11 full speed in one
step or a dead stop; S10 no arcs (U2 A1); O13 the walk's spin not cancelled (Herr and Popovic 2008); W12 arms that do
nothing when balance goes (Jeon 2022); W13 a fall with no protective reaction (Hsiao and Robinovitch 1998); W14 a
glide, bounce or crouched walk (Orendurff 2004); L10 life switched off outside the idle (U1 section 4); H9 a hand
closing out of time with the grip (Jorg 2010); V6 judder of 50 Hz physics at 60-144 fps; V7 the camera inside a body
or the ground. Why: each is something a player would see as unnatural in walking, falling, tool work or on the screen,
and nothing in the 96 measured it. Count now 108 (3.12). Judges' questions added for each (section 4).

**Folded into existing kinds.** H1: a two-handed grip keeps its hands' distance. H3: a swing must die down, never
pump up. C7: other miners' bodies count as world. W10: contact force peaks beyond a person's landing (U2 B8). H8 and
C9: feet planted and braced at a tool's impact. B2: an exempting event must be outside or scheduled beforehand.

**Corrected: false alarms (bars that would flag ordinary movement).**
1. C1 measured the force-weighted contact centroid, which is the foot's own centre of pressure rolling heel to toe:
   every normal step would fail. Now the slip of the sole's material points, as net drift from stance start (summed
   path would count contact jitter over long idles).
2. S3 and S2 per rendered frame: at 30 fps a normal 10 rad/s swing is 19 degrees a frame. Now measured in the parent
   frame, flagged only above the task's T8 cap times `dt`, impact frame exempt for tool and hands; over 90 degrees
   still always a fault.
3. W2's 5% in 0.1 s: the fall's `angularDamping` 0.6 (`PhysicalFall.cs:335`) loses about 6% by design. Now 10%, the
   damping reported as a candidate cause.
4. B7: shift-free windows only. L1, L5, L6, B7: PhysX contact noise calibrated on a held-still body before the centre
   of pressure is used; centre of mass or drawn sway is primary.
5. T2: a foot lifted passively by a hard shove is not an early catch step; only purposeful lifts count.

**Corrected: misses (bars that would pass what Luis has seen).**
1. S1 and C5: converting by `dt^2` turns 4 mm at 100 fps into 36 mm at 30 fps, so a real teleport passes. Now the
   drawn body is recorded at 100 fps, and lower rates add a spike test (3.0). Sections 1, 2 and 3.0 now say 100 fps
   for the drawn body and 50 for the physics (they said "a fixed 50").
2. T8: the physics bodies are clamped at 20 rad/s (`OwnBody.cs:535`, `PhysicalFall.cs:340`), so the 30 rad/s whip bar
   can never fire on them. Added a "frames at the clamp" check.
3. C1 in `motion_breaks.py` is checked only while the foot is flat; a slide on heel or ball is missed. Noted at C1.
4. L1: contact noise alone could let a dead-still body pass the 3 mm/s floor. Now centre of mass sway first.

**Corrected: other.**
- T7, T8: scaling `sqrt(1 m / L)` now `sqrt(0.9 m / L)`, matching 3.0's adult leg.
- T9, W3, W9: "drive torque estimates" replaced by the real `driveForce`, which the code reads (`OwnBody.cs:1097`).
- Detectors: `D:/Dev/WG` line offsets for `OwnBodyTests` (+141) and the copies' differences added to 3.0; T7, O1,
  O2 marked partial (`motion_breaks.py` `turn_began` and yaw speeds); W14 partial (`rise_fall`, `motion_dips.py`);
  `FrameRecorder.cs` faults mapped (spin to S3, jump to S1, wrongway and overfold to J1, whip to T8);
  `ArticulationFactsTests` lines named (S9 `:1100`, W3 `:385`, `:421`). All 21 main-copy test lines checked: right.
- Gap 8 was wrong twice: the jolts, dips and frames scripts are in this copy (not in `D:/Dev/WG`), and `TurnBench` is
  in no repository copy. Rewritten.
- 3.0: added the kinematic walk's definitions (planted, loaded, BoS without contacts) and the list of missing data.
- J1: needs a per-rig anatomical zero-pose table; the `FrameRecorder` 8 against J1's 5 degrees added to section 6.
- J4: marked not like for like with the study (pelvis yaw on the foot is not hip rotation). Bars kept.
- L2: needs at least 10 intervals per kind (about 35-90 minutes of idle); the idle's rate must not be raised.

**Struck.** None. No kind was found wrong in kind; the faults were in measures and bars.

**Set aside, with reason.** Face and eyes (U1 3.15): the miners have no facial animation to measure (no blink or face
code found); to add when they do. Affect and mood (U1 2.6): not a per-frame quantity; left to judges and Luis. Walking
on a slope as on the flat: no checked study found here; left to C4, W14 and the eye [U].

**Flagged, not changed.**
1. **Not ready to run as a scan yet.** The game records no contacts, centre of mass, masses, drive forces or controller
   mode changes (3.0). About a third of the kinds (all of B, most of W, L1, L5, L6, C1 loaded, O13, T2) need that
   recorder first. Kinds that run on today's trace: C3, C5, C11-C13, S1, T6, T7, T11, O1, O2, O5, W14, Z1-Z3, part of
   C1 and C2.
2. **Most bars are judgment.** Of the 108, about two thirds are our own starting values. They are honestly marked, but
   T8's per-task caps, S5's arc length and W14 should be set on people's motion capture before the first run, as the
   page says.
3. **The detectors live in two copies.** The benches run in `D:/Dev/WG`; three review scripts and `alive.md` exist only
   in this copy; `FrameRecorder` and the tools only there; `TurnBench` nowhere committed. Someone should bring them
   together before the first scan.
4. **Bars set by sources not opened.** B8's 6 s, T11's three steps, C13's toe height, W13's 300 ms are from summaries
   or citing papers [U]. They stay strict and marked.
5. **Luis's questions are left as questions.** Nothing here answers questions 1-5; T7, T8 and Z4 still scale by the
   pendulum rule only until question 1 is answered.
6. **Judges' sheets cannot show judder (V6) or real tempo** (T1, Z1); those need clips played at speed, or numbers.
