# U3. The project's own record of unnatural movement (flags, faults, detectors, philosophies)

Researcher 3 of 3, November 13, 2026. Local files only, no web. Read-only on `D:/Dev/WG`. Sources are named by file (and line where it matters); every claim here is "recorded in the project" (a fact of the repository), unless a line says otherwise. Nothing in this page is Game Director acceptance.

Status of this file: written in parts, early first. Parts 1 to 4 and a checklist follow.

---

## 1. Every unnatural movement the Game Director (Luis) has flagged

Columns: **#**, date, his words (short, verbatim), what it was in the game, cause if found, fixed or not.
Sources are the files in `Docs/Correspondence` (his messages) and the page that records the answer.
"Open" means no record of it being closed, or the record says it stands.

| # | Date | His words | What it was in the game | Cause, if found | Fixed? |
|---|---|---|---|---|---|
| L01 | Oct 1 | "it always shuffles a bit its feet" at the landing spot; "looks a bit awkward" | A walker that had already stopped made one or two extra standing steps 0.4-0.7 s later to reach the exact spot (`Correspondence/2026-10-01_CHECKPOINT_A_FEEDBACK_AND_SHOWCASE_SCOPE.md`) | Two separate standing adjustment steps after the root had stopped (`Validation.md` "Arrival fix", Oct 1) | Fixed: at most one closing step, then 2 s of stillness (`GroundedBodyTests.ArrivalFinishesTheStrideWithoutShuffling`). Not a Luis acceptance |
| L02 | Oct 1 | pickaxe "just sitting behind his back, which doesn't make any sense" | A carried tool resting through the body, not held | A carried object with no physical place | Part of the run of object flags that became the physical-objects line (see L03/L04); exact fix not seen |
| L03 | Oct 3 | "the lamp is literally floating by her side clipping her hand" (also "legs not lining up nicely with her boots", "the bag is also a bit clipping") | Held and hung things floating or clipping; legs not meeting boots (`2026-10-03_SMALL_POLISH_AND_NATURAL_GAITS.md`) | Hidden geometry, straps not attached, clothes not weighted to what they lie on (`ArtDirection/CharacterPractices.md`) | Fixed in the models; a close audit (`MinerCloseCapture`) added; practices written |
| L04 | Oct 3 | "anything that appears like an object like that should appear more as a proper object that responds to environment rather than floating" | Laces floating off the shoe, straps not joined to the bag, the lamp floating when seen close, the back "floating" instead of with gravity (`2026-10-03_PHYSICAL_OBJECTS_AND_A_QUALITY_METHOD.md`) | Objects parented but not held, hung or stopped by the body | Fixed by `MinerBody.Hang` (each thing hangs from its own bone as a pendulum) and later "hung things stopped however the body is turned" (Oct 15 to Oct 21); `ModelQualityMethod.md` method written |
| L05 | Oct 8 | "a very light stutter after each walk... like the character is being teleported a little bit... to the ground" | A small drop of the whole body at every step (`2026-10-08_THE_PLAYTEST_NO_FAKE_ANIMATIONS.md`) | Hips dropped 47-128 mm in ONE frame at every step: they were held down only by legs whose feet were on the ground, so at a landing they were put down at once (`Reviews/2026-10-08_ThePlaytestRound.md` L3). It had been so since the walk was made; clips at 25-30 fps hid it | Fixed: hips come down through the swing; jolts 2.0-2.7 mm (bar 4 mm) in all three miners |
| L06 | Oct 8 | "the legs will teleport a bit sometimes when trying to turn, and the movement is not very human or natural" | A turn in place or after a reversal: legs jumping | A foot in the air jumped 31-40 cm in one frame, 2-3 times per turn (up to 38 m/s): the side it went round the standing boot was recomputed each frame and flipped; body walked off backwards at full pace before facing; head, chest, hips began in one frame and turned at one pace (all of a piece); a leg twisted 140 degrees (Playtest round L4) | Made again twice; the traces pass for Small and Long, Round fails (5.4 mm on 3 frames); sent-back walks fail for Long and Round. Came back as L24 (Nov 13) |
| L07 | Oct 8 | "the clothes when the characters kneel... start exploding and moving in very unnatural ways" | Coat flaps flying out in a bend | A coat's flap hung along the pelvis and was turned by two thigh angles; one angle runs away when the thigh comes level (a gimbal-like fault) | Fixed: a flap hangs by its own weight, stopped by the thigh |
| L08 | Oct 8 | "the kneeling animation feels very off and weird... feels more like an animation also in parts" | Going down for a pickaxe done in sequential parts (bow, wait, squat, bow, then the hand), arms held out like wings, 5.5-6.6 s | Built in parts that ran until done; arms "hung" along the chest | Made again as one movement; 4.2-4.7 s; head over its limit 0-2 frames |
| L09 | Oct 8 | "the animation for them recovering, it was just an animation... not the actual character with weight interacting with the world" | Get-up: every part slid through the air from lying to a crouch in 0.9 s, nothing pushing on the ground | The fall was physical, the get-up was posed ("partly not physics") | Became the long thread L16, L19, L20, L25 |
| L10 | Oct 8 | "after he recovered to 77% spent, he stopped recovering, and now he's just still" | Long stood motionless at the rock for ever | The rest pose asked the hand for a place the handle could not be at; the arm gave 100% through the "rest", so tiredness never fell | Fixed; five-minute runs: none stood for ever |
| L11 | Oct 8 | getting the lantern or mug "looks okay until the part to hold, then it looks a bit uncanny" | Lantern held on a straight arm out to the side, 7 mm short of straight, for as long as it was carried; body did not look at what it reached for | No arm keeps that up; no gaze | Fixed: carried by the bail, forearm raised, looks at the thing |
| L12 | Oct 8 | "I don't want any fake animations... they need to correspond to at least how people... see those movements and feel, Oh, yeah, that movement makes sense" | General: movements that "look built in" and do not answer to the world | The posed/own split (Playtest round, "What is still posed") | Standing programme, see Part 4 |
| L13 | Oct 9 | "I really do want movement to feel natural and alive rather than robotic... it can't be posed" | A body that lies fully still for up to 7 s; stands up from nothing; fully still when standing | Six of six judges: "does not breathe, does not stir, and gets up from nothing" (`Reviews/2026-10-09_TheBodysOwn_Round.md`) | Standing at ease with breath and weight shift built (Oct 10); the lying-still stays open at that date |
| L14 | Oct 10 | "I do feel yeah the breath is a bit too strong in the version that you tried" ... "should be gentle... scale appropriately with situation" | Breath drawn too large | A made-up larger variant (judges could not see the real size) | Kept gentle and scaled (see Part 4 philosophy) |
| L15 | Oct 10 | "when the character falls, it still doesn't get up in a natural manner" | Same as L09 | as L09 | open then |
| L16 | Oct 13 | "the getting up is super weird... convulsing kinda not moving on the ground until the timer to get up reaches and then it suddenly gets up with a magical not physical animation" | Lies "dead" until a timer, then rises with no push | Timer-led, posed rise | Became the whole getting-up programme (Oct 14 onward); in the game as an option, up by its own body for the unswitched path |
| L17 | Oct 13 | "I did try pressing also the shove button many times which did lead to some very whacky buggy physics" | Repeated shoves: parts flung at high speed | "The engine's step adds energy" (`Handoff.md`; `OwnBodyTests.ShovedAgainAndAgainNothingOfItFlies`): Small's fastest part 25.0 m/s with the old turns, 22.7 with the new; Round 17.1 vs 4.2 | Not closed for Small: the test still failed both ways on Nov 13 (later: "16 of 16" with the default on, Handoff, Nov 13 afternoon) |
| L18 | Oct 13 | wants no pre-made animation and no switch "from one kind of response to the other at a certain threshold. It should be a smooth transition" | A design rule (pulled miner's answer grows smoothly) | n/a | A rule, see Part 4 |
| L19 | Oct 22 | "It should be absolute last resort the unnatural assistance" | The backup (assisted) rise animation | n/a | Rule: extra tries over help |
| L20 | Oct 30 | "very aggressive twitches with the limbs... character's arms seem like it's broken... twitch like for one millisecond completely out of frame" (also "it feels goofy" and "so long to get up") | In the getting up, as played at his frame rate | **Found by reading all 134,085 frames at 120 fps** (`Reviews/2026-10-30_GettingUp_EveryFrame.md`): (1) drawn limbs turned half round in one frame, 1,036 times in 18 min, up to 176 degrees, 543 with the limb within 20 degrees of straight (a drawing fault in `MinerBody`, not physics); (2) joints bent as no living joint bends (elbows the wrong way up to 70 degrees, knees folded to 180, hips 89 behind) because the get-up body had no joint limits; (3) whipping parts (boots to 115-154 rad/s; a person rarely above 5); (4) the body blew apart once in four runs; (7) slow, 62 tries for 8 pushes | (1) mended (`TurnsAsSolved`): 1,036 to 40; (2)-(4) not mended in the old body, belong to the new body; stops tried on the old rise left it chattering (Oct 30 late) |
| L21 | Nov 3 | "I would have to check if it looks to robotic or repetitive" | Risk named for the capacity-driven stance rise | n/a | Watch item, no record of a check yet |
| L22 | Nov 9 | "hopefully some level of natural variability... to sell the reality and life like aesthetic" | A rule for resting hands and limbs | n/a | Free limbs rest, default on (Nov 5, Nov 9) |
| L23 | Nov 13 | "doesn't have that moment people... sort of readjust their balance after recovering... Also people generally adjust from time to time their positioning a bit to not feel stiff" | After a shove the body catches itself and stays where the catch left it; shifts are few | Not a bug: the design had no settling to a comfortable stance (Handoff, Nov 13) | Researched (`2026-11-13_TurningAndSettling/`); Luis approved six recommendations Nov 13; not built yet |
| L24 | Nov 13 | "doesn't look very normal while turning around... weird leg positions while turning and even more rare some weird twitches... try to get both to 0" | The kinematic IK walk turning, esp. sharp and right round | One lead, not checked at the time: the walk turns the body at up to 420 degrees/s and holds it where a planted leg is twisted 62 degrees in its hip (`MostTwist`), then lets it go on | Open; research done; turn bench (`TurnBench`) drafted with bars F1-F6 |
| L25 | Nov 13 | "this weird leg deattached part" (picture `Correspondence/2026-11-13_images/leg_while_getting_up.png`) | A leg looking detached while a miner lay trying to get up, with "Gets up by its own body" ticked | Lead, not checked: the fall's body is jointed rigid bodies whose joints can pull apart (projection 2 cm and 5 degrees, `PhysicalFall.cs:388`); or the drawn leg | Open |

(Positive notes he gave, kept so as not to undo them: Oct 6 "bench is promising", Oct 7 "balance tests looking pretty good", Oct 13 stepping after a push "liked", Oct 10 and Nov 13 the breath and "idle... Makes it feel really alive", Nov 13 prefers the solver's turns (14 and 4) and "the little movements it does from time to time".)


---

## 2. Faults found by the project's own tests, numbers and judges

Source pages: `Docs/Reviews/*`, `Docs/Validation.md`, `Docs/ArtDirection/MotionJudging.md`, `ModelQualityMethod.md`, `CharacterPractices.md`. Same columns as Part 1. Some cross the Luis flags above (L-number given); they are listed again because the instrument or the cause differs.

| # | Date | Found by | What it was | Cause | Fixed? |
|---|---|---|---|---|---|
| F01 | Oct 8 | Trace at 100 fps (`motion_breaks.py`) | Hips dropped 47-128 mm in one frame at every landing (L05) | Hips held only by grounded legs | Fixed; jolts 2.0-2.7 mm, bar 4 mm |
| F02 | Oct 8 | Trace, Playtest round | Foot in the air jumped 31-40 cm in one frame in a turn (L06) | Side round the standing boot recomputed each frame and flipped | Made again twice; Round still 5.4 mm on 3 frames then |
| F03 | Oct 8 | Trace | Body walked off backwards before facing; head, chest, hips began in one frame and turned at one pace | Turn not led by the head and carried down the spine | Made again; sent-back walks still failed for Long and Round then |
| F04 | Oct 8 | Trace | A leg twisted 140 degrees in a turn | Same flip as F02 | Made again with F02 |
| F05 | Oct 8 | Judges, Playtest round | Kneel built in sequential parts; arms held out (L08) | Parts ran until done | One movement, 4.2-4.7 s |
| F06 | Oct 8 | Judges | Coat flap ran away when the thigh came level (L07) | One angle of a pair runs away near level | Flap hangs by its weight, stopped by the thigh |
| F07 | Oct 8 | Five-minute runs | Miner stood still for ever at 77 percent spent (L10) | Rest pose asked the hand for an unreachable place | Fixed; none stood for ever |
| F08 | Oct 8 | Judges | Lantern on a straight arm out to the side, no gaze (L11) | No arm keeps that up | Carried by the bail, forearm raised, looks at it |
| F09 | Oct 8 | Judges | Get-up posed: parts slid through the air, 0.9 s (L09) | Fall physical, rise posed | The long getting-up thread (F10-F17) |
| F10 | Oct 9 | Six of six judges | Body lies fully still, does not breathe, gets up from nothing (L13) | Nothing alive in the body at rest | Standing at ease built Oct 10; lying-still open at that date |
| F11 | Oct 10 | Judges, Luis | Breath too strong (L14) | A made-up larger variant | Kept gentle and scaled |
| F12 | Oct 14 | Judges, GettingUp round | A snap onto hands and knees at about 3.8-4.2 s, then a tidy held pose | A pose coming on over a few frames | Worked on in later rounds; Round 3 (Oct 30) found no slide and a 0.4 s ease on arrival |
| F13 | Oct 15 to 21 | Judges, SatchelStop and HungThings rounds | Satchel, lantern, mug, hammer passed through the thigh or ground, or hung wrong when the body was turned or lain on | Hung from a bone but not stopped by the body | `MinerBody.Hang` pendulum plus stops; `HungThingTests` |
| F14 | Oct 30 | Every-frame read, 134,085 frames at 120 fps (L20) | Drawn limbs turned half round in one frame, 1,036 times in 18 min, up to 176 degrees | A drawing fault in `MinerBody` (turn read the long way round), not physics | Mended: 1,036 to 40 (`TurnsAsSolved`) |
| F15 | Oct 30 | Every-frame read | Joints bent as no living joint bends (elbows backwards up to 70 degrees, knees folded to 180) | The get-up body had no joint limits | Not mended in the old body; belongs to the new body |
| F16 | Oct 30 | Every-frame read | Whipping parts: boots 115-154 rad/s, where a person is rarely above 5; the body blew apart once in four runs | Limits absent; engine step energy | Not mended in the old body |
| F17 | Oct 30 | Numbers | Stops tried on the old rise left it chattering | A guard fighting the drive | Dropped; new body took over |
| F18 | Nov 10 | Two fresh reviewers, `Reviews/2026-11-10_FlyingBoot_K1/review.md` | "The flying boot": a leg gains 270-1,120 J in some steps that nothing is counted to give | The engine's own step adds energy to a fast-turning part (first step about 0.7 x (speed x step) squared; reviewers corrected how far that law reaches) | A diagnosis with a rule: no more guards on the keeper; why runaway happens at some speeds is stated as "not yet known" |
| F19 | Nov 12 | `ArticulationFactsTests.AStifflySprungAnkleHoldsALightBoot`, reviewed in `2026-11-12_StiffAnkle_K8` | A light boot held by a stiff spring under the solver's turns | Solver turns 18 or 32 give the drive different shares | Answered under PGS at the design's stiffness; claim limited at 3x and 10x |
| F20 | Nov 13 | `OwnBodyTests.ShovedAgainAndAgainNothingOfItFlies` (L17) | Fastest part 25.0 m/s (Small, old turns) or 22.7 (new); Round 17.1 vs 4.2 | Same energy as F18 | Failed both ways at first; later "16 of 16" with the default on |
| F21 | Nov 13 | Reading `MostTwist` | Planted leg twisted 62 degrees in its hip while the body turns up to 420 degrees/s | The walk holds the body round while a foot is planted | Open; `TurnBench` drafted (F1-F6 bars in `Handoff.md`) |
| F22 | Oct 1 | `GroundedBodyTests.ArrivalFinishesTheStrideWithoutShuffling` | Extra standing steps after arrival (L01) | Two adjustment steps after the root stopped | Fixed |
| F23 | Oct to Nov | Model quality method | Floating (over 3 mm), sinking, meeting, covered, beneath, pinched, stretched parts; hanging things not pendulums | Authoring, not motion | Checks run on each model (`ModelQualityMethod.md`) |
| F24 | Oct 8 on | `MotionJudging.md` lessons | "Smooth is not the same as right"; "the bench itself can be the fault"; clips at 25-30 fps hid what 100 fps showed | Measuring too coarsely or on the wrong bench | Rule: read at the rate the game runs, and test the bench |
| F25 | Nov 13 | Luis's picture | Leg looks detached while getting up (L25) | Lead only: joints of the fall's body can pull apart; or the drawn leg | Open |

The project also records faults of the instruments rather than the body: a build judged with the new switch off (the "say what must be switched on" rule), and sheets every 0.08 s that missed one-frame flicks (F14).

### The five faults that came back most often

1. One-frame jumps and teleports: hips at each step, foot flips in turns, drawn limbs turning half round (L05, L06, F01, F02, F14).
2. Posed or timer-led movement that is not the body acting on the world: get-up, kneel, lying still (L08, L09, L13, L16, F05, F09, F10).
3. Parts flung or whipped by energy the engine step adds: repeated shoves, flying boot, boots at 115-154 rad/s (L17, F16, F18, F20).
4. Joints bent or twisted beyond what a person can: elbow backwards, knee folded, leg twisted 140 or 62 degrees (F04, F15, F21).
5. Held and hung things floating, clipping, or not stopped by the body (L03, L04, F13, F23).

---

## 3. Detectors the project already has

All paths in `D:/Dev/WG`, read-only. Line numbers are as read on Nov 13 and may move. "Bar" is the value in the code or page that states it; "stated in test" means the assertion is in the test and was not copied number by number.

| Detector | Where | What it measures | Bar |
|---|---|---|---|
| Frame-to-frame break (hips, head) | `Art/Review/motion_breaks.py:24` | Change of the per-frame step (second difference) of hips and head | 4 mm |
| Foot in the air break | `motion_breaks.py:25` | Same, for a free foot | 12 mm |
| Planted foot slide | `motion_breaks.py:26` | Movement of a planted foot over ground | 1 mm a frame |
| Landing | `motion_breaks.py:27` | Movement in the frame a foot lands beyond what it was doing | 6 mm |
| Planted foot pivot | `motion_breaks.py:28` | Turn of a planted foot on the ground | 1 degree a frame |
| Trace recorder | `Assets/_WonderGather/Tests/PlayMode/MotionTraceBench.cs:40` | Records walk, turn, turns, pick at 100 frames per second to feed the script above | none in itself |
| Frame recorder (whip) | `PlayMode/FrameRecorder.cs:104, 117, 191` | Per-frame part speed and turning; reports a "whip" | angular velocity over 30 rad/s |
| Turn: boots meet | `PlayMode/MinerTests.cs:208` | Steps taken through a sharp turn and the closest the boots come | more than 3 steps; boots not overlapping beyond 4 mm |
| Heads above feet | `MinerTests.cs:345` | Head stays above the feet in the walk | stated in test |
| Arrival | `PlayMode/GroundedBodyTests.cs:142` | Extra steps after arriving; then still | at most 1 closing step; arrival within 5 cm; stays still |
| Pelvis collapse | `GroundedBodyTests.cs:164, :188` | Corrections of the order do not collapse the pelvis or start a gait | stated in test |
| Idle foot drift | `PlayMode/LivingBodyTests.cs:66` | Support alternation and idle foot drift on flat orders | stated in test |
| Ramp feet | `LivingBodyTests.cs:119, :147, :171` | Feet on the ramp; support and leg reach after correction | stated in test |
| Standing by joints | `PlayMode/OwnBodyTests.cs:234` | Wander, sinking, settling after the switch | wander under 2 mm; sank and settle under 3 cm |
| Nudged | `OwnBodyTests.cs:282` | Keeps its feet when nudged at 0.15 m/s from four directions | does not go down; back within 3 cm |
| A minute of life | `OwnBodyTests.cs:440` | Does not wobble or fall for 60 s | stands throughout |
| Shove with a step | `OwnBodyTests.cs:668`; at ease no step `:642` | Catches itself with a step; takes none at ease | stated in test |
| Shoved again and again | `OwnBodyTests.cs:908` | Fastest part (m/s) and turning in repeated shoves, by how far each part's centre truly moves | nothing flies; figures are in the test |
| Solver turns cost | `OwnBodyTests.cs:1098` | What more turns of the solver cost | information |
| Fall and get up | `PlayMode/PhysicalFallTests.cs:85` | Body sound, deepest into ground, furthest apart, head lowest, weighs what it should, upright again, up within 1.2 m | into ground under 3 cm; head under 0.45 of height; weight and height within 5 percent |
| Laid down any way | `PhysicalFallTests.cs:158` | Gets up by its own strength from any lying; not the old way | up within 40 s; no old way |
| Hard and light falls | `PhysicalFallTests.cs:270, :320` | Harder push lies longer; a quarter-weight pull does not fell; four fifths does | lies 0.5 to 10 s |
| Hung things | `PlayMode/HungThingTests.cs:62, :199` | A thing taken in hand and hung back; handle sizes in range | clear .1 to .5 m; grip 3 to 20 mm |
| Engine facts | `PlayMode/ArticulationFactsTests.cs` (e.g. `:57, :147, :385, :806, :1100, :1624`) | Drive strength holds, joint speed bound, energy a fast leg gains by the step itself, stiff ankle, shortest-turn crossing, solver turns set after the links | each test states its own |
| Judges | `Art/Review/judges/` (`body.md`, `animator.md`, `sceptic.md`, `referee.md`; the eight things in `alive.md`) | Fresh agents read sheets and figures | not numbers; a verdict, not Luis's |
| Tools | `Docs/Technical/AnyBody/tools/` (`README.md`, `unity_tests.sh`, `shoves.sh`, `testresults.py`, `r4_table.py`) | Run and read the suites; the shove trace to count energy; read the stiff-ankle runs | not bars |

Named in the project's pages but not found as files: `framescan.py` (the Oct 30 every-frame read, scratch only), and `motion_jolts.py`, `motion_frames.py`, `motion_dips.py` named in `MotionJudging.md`. A future pass should put the every-frame scan into the repository.

Gaps (not detectors yet): a turn bench with F1-F6 bars (drafted, Nov 13), a settling and weight-shift measure (researched, not built), a foot-turn rate against body-turn rate bar, a frame-cap sweep at several caps (suggested in `twitches_and_broken_limbs.md`).

---

## 4. Design philosophies bearing on natural movement

One line each, with source. These are Luis's rules as recorded; none is a new decision.

1. The body's own, not posed: movement is pushed by the body's own joints, stable, natural and alive (Oct 9; memory note "The body's own, not posed"; `GAME_VISION.md`).
2. No fake or posed animation: every movement is the weighted body acting on the world (Oct 8 message; memory "No fake animations").
3. Physical objects: anything that reads as an object is held, hangs, swings and is stopped by the body; never floating (Oct 3; `CharacterPractices.md`).
4. One body for everything: the same body stands, steps, falls and gets up; a part added for one goes on all (Oct 14).
5. No switch at a threshold: a body's answer grows smoothly with the push or pull, by what that body can do (Oct 13).
6. Strength from muscle, slower with size: true weights, size-slowed joints, all actions for any body (Nov 3).
7. Getting up for any body: any proportion or strength, procedural, natural, no per-body teaching (Oct 30, Nov 3).
8. Never the backup animation: always up by its own body; extra tries over help; help only while training (Oct 22).
9. Natural variability: resting hands and limbs vary a little to sell the life (Nov 9; stance atlas and free limbs).
10. The idle he loves: standing at ease, small movements from time to time; keep it through every change, at turns 14 and 4 (Nov 13).
11. Breath gentle and scaled: life at rest is small and grows with the situation; never enlarged because judges cannot see it (Oct 10).
12. Every frame as played: read the game every rendered frame, from several angles, with numeric scans (Oct 30).
13. Measure all three miners (Small, Long, Round) through a whole work-tire-rest cycle at several strengths; Long fails first (memory note).
14. Test what Luis plays: run flows as played after a default changes; name the switch in the report's first line (memory notes).
15. Search the poses in the sim rather than hand-set them; what is asked gets gamed, so read pictures (memory note).
16. Aesthetic judges check body changes against the game's aesthetic guide (Oct 22).
17. Change only what is asked: a note changes what it names (memory note).
18. Explorations are additive, shown early beside the loved one (memory note).
19. Prototype purpose first: real weight and real strength at work come before polishing looks (memory note).
20. Settling and shifting: people readjust after a shove and shift to avoid stiffness; six recommendations approved Nov 13 (`2026-11-13_TurningAndSettling/research.md`; Correspondence Nov 13).
21. Quality above everything: never trade quality for speed (Nov 9).

---

## Checklist

- Part 1 flags: 25 (L01-L25). Part 2 faults: 25 (F01-F25). Part 3 detectors: 25 rows (several rows hold many tests). Part 4 philosophies: 21.
- Not seen, so not claimed: the aesthetic guide (a PDF from Luis, not in the repository); any record that L21 (robotic or repetitive stance rise) was checked; the cause of L25.
- Marking: everything here is "recorded in the project" (one source each), not outside science.
