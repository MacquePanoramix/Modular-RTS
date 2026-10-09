# The body's own — the design as it is built (S3b)

**The plan:** [NextMilestonePlan.md](../NextMilestonePlan.md) (approved
by Luis on October 9, 2026).
**What it is for:** every movement of a worker pushed by its own joints,
stable, natural and alive (Luis, October 9:
[the direction](../Correspondence/2026-10-09_THE_BODYS_OWN_NOT_POSED.md);
[the four answers](../Correspondence/2026-10-09_THE_BODYS_OWN_ANSWERS.md)).
**What was read for it:**
[the research of October 9](../Research/2026-10-09_AliveAndTheBodysOwn.md).

This page is written step by step, as each is built. The body before it
(weighed, with strength and tiredness, posed; let go only to fall) is in
[The physical body](ThePhysicalBody.md).

## Step 0: the ground for it (October 9)

**What the plan asked:** ankles; the test that nothing pushes the body
from nowhere; today's body measured for the table of "stable"; what a
body's own costs in the frame. **What Luis sees:** figures. Nothing in
the game is changed by this step: it is a bench
(`Tests/PlayMode/BodysOwnBench.cs`) and this page.

**This page was written twice.** The first writing said the physics
must step four to ten times as fast as the game's. Three reviewers who
did not make the bench showed that the figures did not carry that
([what they said](#what-the-reviewers-said)); two of their trials were
run the same day, and what is below is what the figures carry now.

### In short

1. **A body can stand by its own joints, and stand as still as Luis
   asked.** All three miners stood twenty seconds on the path; after
   settling a few millimetres in the first seconds, their heads stayed
   within 0.1 to 0.35 mm. On a level floor, twenty-five of twenty-five
   of each still stood after twenty seconds.
2. **That was at 200 steps of physics a second; the game takes 50.** At
   100, Long and Round stood as still and Small did not. At 50 none
   stood reliably. **How few will do is not settled:** how softly the
   joints are sprung mattered as much as the rate (with stiffer springs
   Round fell at 200 and stood only at 500).
3. **What stood** was a body whose legs give torques **worked out** each
   step from the push it wants from the ground, with every joint also
   held softly towards its pose by the engine's spring. The engine's
   springs alone, set stiffly towards a standing pose, let every miner
   fall within three seconds at 50 steps a second, and at 500 held it
   up swaying 5 to 15 mm. (Worked-out torques with no springs at all
   was not run.)
4. **It does not yet keep its feet when pulled.** Round took 30 N and
   came back to within a tenth of a millimetre; Small and Long fell at
   30 N. Today's posed body leans at 50 to 150 N. Leaning against a
   pull is step 1.
5. **What it is to be made of** (my decision, reviewed, for Luis to
   know): one jointed body solved as one (an *articulation*), of
   thirteen parts with ankles; nothing slowing its parts but its
   joints; its legs' torques worked out.
6. **What it costs:** one worker's body, about a fifth of a millisecond
   for each fiftieth of a second at 200 steps a second, a tenth at 100
   (in the Editor; twenty-five standing bodies took 5.2 to 5.8 ms and
   2.6 to 2.8 ms). The game's frame is about 4.5 ms now. A crowd of a
   hundred cannot have it: a far-off crowd keeps the posed body.

### Today's body, measured (the bar)

The posed body, standing on the path, pulled forwards at the chest for
two and a half seconds (`PhysicalBalanceBench`). The body's own must do
no worse.

| Pull | Small (41 kg) | Long (58 kg) | Round (88 kg) |
|---|---|---|---|
| 50 N | leans (hips 74 mm) | leans (72 mm) | leans (33 mm) |
| 100 N | **one step** (9 cm) | leans (97 mm) | leans (79 mm) |
| 150 N | six steps (0.89 m) | six steps (0.70 m) | leans (92 mm) |
| 200 N | five steps (1.39 m) | six steps, then **falls** at 2.5 s | **one step** (9 cm) |
| 300 N | **falls** at 0.7 s | falls at 1.2 s | six steps (0.83 m) |
| 400 N | falls | falls | **falls** at 2.1 s |
| 550 N | falls | falls | falls at 0.8 s |

As a share of its weight: it leans without stepping up to about a
sixth; takes a step at about a quarter; and falls from about a third
(Long) to three quarters (Small).

### The body in thirteen parts

The let-go body has eleven parts; its boots are fixed to its shins. A
body cannot balance at an ankle it does not have. The bench reads the
let-go body as the game makes it and takes each boot off its shin as a
part of its own, on an ankle at the shin's lower end.

| | Small | Long | Round |
|---|---|---|---|
| The whole | 40.7 kg | 58.5 kg | 87.6 kg |
| A boot; what is left of a shin | 1.29; 1.35 kg | 1.83; 2.74 kg | 1.55; 3.14 kg |
| An ankle gives (new: seven tenths of a knee) | 61 N m | 141 N m | 154 N m |
| A knee; a hip; the back | 87; 130; 257 | 202; 303; 347 | 220; 331; 462 |
| Its weight over its boots, standing | 6 mm ahead; boots from -52 to 105 mm | 18 mm; -66 to 122 | 6 mm; -60 to 114 |

**An ankle's strength is a first setting,** from how a person's ankle
compares with the knee (about 0.6 to 0.75 of it, pushing the toes down;
from what I know, not read again).

### The whole body, standing

Each miner made in thirteen parts at the pose it stands in, and left to
stand twenty seconds on the path; and twenty-five of it on a level
floor. One run each on the path; the twenty-five are the better count.

**Held by the engine's springs alone** (stiff: all of a joint's strength
when it is 5 degrees from its pose):

| Steps a second | Small | Long | Round |
|---|---|---|---|
| 50, jointed rigid bodies | falls, 1.2 s | falls, 1.9 s | falls, 1.3 s |
| 50, articulation | falls, 1.6 s | falls, 2.6 s | falls, 1.6 s |
| 500, articulation | stands; head wanders 8 mm, 1.3 times a second | stands; 5 mm | stands; 14.5 mm |

**Kept by torques worked out for its legs,** with the engine's springs
holding each joint softly towards its pose (articulation):

| Steps a second | The springs | Small | Long | Round |
|---|---|---|---|---|
| 50 | stiffer (20°) | falls, 1.8 s | falls, 3.4 s | falls, 2.5 s |
| 50 | soft (75°) | falls, 0.9 s; of 25 on the floor, **14 stand** | falls, 1.5 s; **0 of 25** | (the run broke off) |
| 100 | stiffer (20°) | falls, 2.7 s | falls, 3.0 s | falls, 2.4 s |
| 100 | soft (75°) | falls, 2.5 s; 14 of 25 | **stands, 0.11 mm; 25 of 25** | **stands, 0.13 mm; 25 of 25** |
| **200** | **soft (75°)** | **stands, 0.35 mm; 25 of 25** | **stands, 0.23 mm; 25 of 25** | **stands, 0.08 mm; 25 of 25** |
| 200 | stiffer (20°) | stands, 0.26 mm | stands, 0.56 mm | falls, 2.9 s |
| 500 | stiffer (20°) | | | stands, 1.13 mm |

- **"Stands, 0.35 mm":** the furthest its head went from its middle
  place between the fifth second and the twentieth. In the first
  seconds it settles: 4 to 10 mm with the soft springs, 19 to 32 mm
  with the stiffer.
- **"25 of 25":** of twenty-five on a level floor, how many still stood
  after twenty seconds.
- **"The springs":** how far from its pose a joint is when its spring
  gives all the joint's strength. The damper was a twentieth of a
  second for the soft rows and a tenth for the stiffer. With no damper
  at all, all three fell at 100 steps a second; with the springs softer
  still (120°), Long and Round stood but wandered 3 and 13 mm.

**"Torques worked out"** is a first keeper, written in the bench:

- What the ground should push the body with is reckoned each step: its
  weight; a little more or less to keep its height; and sideways to
  bring its weight back over where it began.
- Where on its soles that push must act, so that it passes through the
  body's weight; no further than the soles reach.
- Each leg's hip, knee and ankle give what makes its boot push the
  ground so (the turning of that push about each joint).
- Its hips keep its trunk upright.
- Every torque is put on the two parts a joint joins, equal and
  opposite.

Standing, this gave: an ankle 12 to 25 N m, a knee 14 to 39, a hip 8 to
18 (of 61 to 154, 87 to 220 and 130 to 331 they have). **That is the
worked-out share only:** what the engine's spring adds at the same joint
is not counted in it, and the two are each held to the joint's strength
separately, so a joint could give up to twice what it has. That is a
fault of the bench's keeper, to be mended in step 1.

**Pulled at the chest for two and a half seconds** (200 steps a second,
soft springs): Round took 30 N (3% of its weight), its head going 47 mm
with the pull and coming back to within 0.1 mm. Small (8%) and Long
(5%) fell at 30 N. With the stiffer springs all that were tried fell at
50 N.

- **Part of that is not a fault.** A body that only brings its weight
  back over where it began, on flat feet, can resist about as much as
  its weight times the distance to its toes over its height: by the
  reviewers' reckoning, some 50 N for Small and 60 N for Long, pulled
  at the height of its weight, and less at the chest. Today's posed
  body takes more because it leans back first.
- **Part of it is.** Round should take about 110 N so; it took 30, and
  with the stiffer springs fell at 50. And Small and Long fell below
  their reckoning. Why is not found: the bench said nothing about a
  body while it was pulled.

### What the engine's joint springs do

**One joint alone.** An arm of one kilogram held out level from a post,
on a spring. **An articulation's spring is what it is set to:** it sags
13.5°, 2.83° and 0.28° where the reckoning says 14.05°, 2.81° and
0.28°, and the engine reports what the joint gives truly (4.90 N m for
the 4.905 the arm asks). The same test on jointed rigid bodies sagged
three to eleven times as much; but that joint was not set up as the
let-go body's joints are, so it says little about them.

**One ankle under a heavy body.** One part of 86 kg, its weight 0.75 m
over an ankle, on one boot lying on a level floor; the ankle's spring
five times what the leaning asks (stiffer than any of the miners' in
the tables above); begun leaning one degree.

| Steps a second | Boot of 1.55 kg | Boot of 20 kg |
|---|---|---|
| 50 | **falls** (in all four ways it was made) | stands (3 of 4) |
| 100 | stands in 2 of 4 | |
| 200, 250 | stands in 3 of 4 | |
| 500 | stands in 4 of 4 | stands in 4 of 4 |

("Four ways": the boot hung from the heavy part or the heavy part from
the boot; the ankle damped or not. They are four makings, not four
tries of one. The way that is like a miner, the boot hanging from the
body, **fell when it was damped** at 100, 200 and 250 steps a second
and stood undamped; at 500, damped, it stood but leaned 1.7°.)

**What that shows, and what it does not.** A stiff spring between a
heavy part and a light one that lies on the ground does not hold at
large steps, and heavy damping makes it worse. The makers of the
engine's physics say the same: a joint's spring has a rate of its own,
the root of its stiffness over the inertia of the *lighter* side, and
that rate times the step should not be much more than one; when it is,
lower the stiffness or give the light part more turning weight, before
stepping faster
([NVIDIA's guide to stable articulations](https://docs.omniverse.nvidia.com/kit/docs/omni_physics/107.3/dev_guide/guides/articulation_stability_guide.html)).
The bench did not vary the spring in this test, so it shows the
direction and not the law. By a reviewer's reckoning the ankles of the
"stiffer" rows above have that rate times the step at 0.8 (Small), 1.0
(Long) and 1.1 (Round) at 200 steps a second: the order in which they
stood and fell.

Those who walk bodies by joint torques have stepped fast: one to two
thousand steps a second by tradition, 225 at the lowest with care
([Carensac and others](https://hal.archives-ouvertes.fr/hal-01884827));
a spring worked out from the state a step ahead allows larger steps
([Tan, Liu and Turk](https://www.cc.gatech.edu/~turk/my_papers/stable_pd.pdf)),
and bodies trained with it are stepped 120 times a second. (Read as
search summaries, not as whole papers.)

**Not confirmed:** more solver rounds (14 or 80) made no difference to
an articulation in one trial, and the engine's other solver, set in
the project's settings, made none that could be seen; but nothing in
the run says which solver was in force, and its makers recommend it for
exactly this.

### Nothing pushes it from nowhere

The body is dropped through the air for a second and a half, swinging
its arms and legs by its joints.

| | Its weight left the course its weight alone gives it by | Its turning about itself changed by |
|---|---|---|
| Articulation, nothing slowing the parts | 0.004 m/s at 500 steps a second; 0.03 to 0.08 at 50 | 0.19 of 3.5 kg m²/s |
| Articulation, kept by its own torques | 0.02 m/s | 0.17 of 3.1 |
| Jointed rigid bodies, nothing slowing the parts | 0.0000 m/s | 0.07 of 4.5 |
| Jointed rigid bodies, kept by the same torques | 0.0000 m/s | **0.4 to 0.9 of 2.3 to 3.3** |
| **Either, its parts slowed as the let-go body's are today** | **0.54 to 0.62 m/s** | (the test cannot tell: see below) |

- **The let-go body's parts are slowed by the engine** (a setting on
  each part, as air would slow them, but much more): 0.6 m/s in a
  second and a half. That is a push from nowhere, by the plan's rule.
  The body's own has none of it; what slows a joint is in the joint.
- **An articulation is not exact,** and less so at larger steps: up to
  0.08 m/s (of the 14.7 it fell) at the game's step, 0.004 at 500 a
  second.
- **What this test can and cannot see.** A torque put on two parts,
  equal and opposite, cannot change how the body's weight falls
  whatever its size: so for the keeper's torques the first column says
  nothing, and only the second does. And the limbs were swung as
  mirrors of each other, so what slowing does to the body's turning
  cancelled. The jointed rigid bodies with the keeper's torques did
  not keep their turning (a quarter of it changed): another reason not
  to make the body of them.
- **The test** is in the bench. It becomes a test of the game's body
  when the body's own is in the game (step 1), with limbs swung
  unevenly, and its bar set for the step in use.

### What it costs

For each fiftieth of a second (one step of the game), over the place
alone, in the Editor. Not measured in the built player.

| | One | 25, standing |
|---|---|---|
| Articulation and its own torques, 100 steps a second | | 2.6 to 2.8 ms (0.11 each) |
| **Articulation and its own torques, 200** | 0.42 to 0.45 ms | **5.2 to 5.8 ms (0.22 each)** |
| Articulation and its own torques, 50 | 0.12 ms | |
| Articulation, the engine's springs, 500 | 0.9 ms | |
| Jointed rigid bodies, 50 (fallen and lying) | 0.05 to 0.07 ms | 0.2 to 0.45 ms |

(One body alone costs more than a twenty-fifth of twenty-five. The
bench's keeper makes new lists every step, which a built one would
not.) Everything else the physics steps (what hangs on the body, the
tool, the stones) is stepped as often.

### What follows for the plan

**Decided here (mine; reviewed; for Luis to know):**

- **The body's own is an articulation** of thirteen parts with ankles.
  Its springs are exact, what its joints give can be read truly, it
  kept its turning in the air where jointed rigid bodies did not, and
  the way of keeping it up (below) is the known one for such a body.
- **Its legs' torques are worked out** from the push it wants from the
  ground (the reviewers name the method: Pratt's, and Coros's, on
  SIMBICON; it is the one the research of October 9 pointed to). The
  engine's springs hold the pose, softly.
- **Nothing slows its parts** but its joints.

**Not decided: how often the physics steps.** 200 a second stood all
three; 100 stood two. The first writing of this page took that for a
need. It is a setting among others, and the cheapest ones were not
tried: an ankle's spring set by rule (its rate times the step a half
or less), its damper by the boot's turning weight and not by a time, a
boot of the weight of a miner's boot, the engine's other solver
confirmed in force. Step 1 begins there.

**What step 1 has to do that the plan did not say:**

- **Settle the step,** counted over many starts on all three miners,
  with a trace of the first one that fails; and keep it one number, so
  that if only the body needs it, the body can be stepped apart from
  the rest of the place.
- **Whatever the step becomes, what is already built is touched.** The
  fall, the getting up, what hangs on the body and the pickaxe in the
  hands are stepped by the same physics, and their springs behave
  differently at a finer step. The getting up's poses were found at
  fifty steps a second.
- **A keeper that leans, then steps.** The reviewers' route: say during
  a pull where the push is wanted and where it can act, how the boots
  tip and the trunk leans (the bench said nothing); reckon balance from
  where the body's weight *is going*, not only where it is; let the
  place it holds its weight drift against a steady pull (that is the
  lean); use the hips; and step when the weight is going where the
  feet cannot reach.
- **Know the ground by what touches it,** not by the shapes of the
  boots and "up": the bench's keeper would not do on a slope or with
  one foot off the ground.
- **A joint gives no more than it has,** springs and worked-out torques
  together; and when a leg cannot give what is asked, the whole leg
  gives less together (not each joint cut off by itself, which bends
  the push), and a body that cannot stand goes over into the fall that
  is already built, on the same body.
- **Stillness is not life.** A quarter of a millimetre is a statue.
  Life comes in step 1 by what the body is asked to do (its weight
  drifting from leg to leg, breath in its pose), not by letting it
  wobble.
- **What it holds and carries** is best made part of the body from the
  start: changing an articulation's parts while it runs resets it.

**Kept open:** a crowd (the posed body, far off); fewer parts for a
start (nine: the reviewers' cheaper way, giving up the arms' and the
head's own movement until later).

### What the reviewers said

Three separate agents who did not make the bench (the physicist, the
sceptic, the builder) were given the first writing of this page, the
bench's code and its results. Each claim was checked against the
results before it was acted on.

| What was said | By | Checked | Done |
|---|---|---|---|
| The family is right: an articulation, its legs' torques from the push wanted of the ground. The signs and the pairing of the torques are right | The builder; the physicist | | Kept |
| **"The lever it pulls is wrong":** stiffness and damping were never varied where the torques were on; the ankle's spring over the boot's inertia orders every result; soften it, damp it by the boot's inertia, give the boot a miner's weight; guess: 100 steps a second | The builder; the sceptic (with the reckoning) | **Stands.** Run the same day: with soft springs Long and Round stand at 100, and all three at 200 with 25 of 25; Round, which fell at 200, stands at 100 | The page rewritten; the step is not decided |
| "None at 50 or 100" rests on single runs; on the level floor 16 to 18 of 25 stood six seconds at 50 | The sceptic | Stands | Twenty-five counted for twenty seconds at 50, 100 and 200 |
| "What stood was torques, not springs": the springs were on at every joint; no run has torques without them | The sceptic; the physicist | Stands | Said so. Not run |
| "A joint gives no more than it has" is false: the spring and the torque are each held to the joint's strength, so twice it can be given; the figure printed leaves the spring out | The physicist; the sceptic | Stands | Said so; for step 1 |
| "Heads within a quarter of a millimetre": only from the fifth second, and only sideways; before it they moved 19 and 32 mm; and the speeds printed (1 and 2.4 mm/s, never turning back) cannot go with that | The sceptic; the physicist | Stands. The engine's reading of a resting part's speed was wrong | The settling is said; speeds are now taken from where the head is, step to step |
| "Pulled with 50 N each fell" is, for Small and Long, about what flat feet can resist; it says nothing of the step. Round's fall is real and unexplained | All three | Stands | Said so |
| The one-ankle test's spring is 7 to 18 times a miner's; its "N of 4" are four makings; one damped case at 500 leans 1.7°, not "within 0.1°"; its figures were taken 800 m up, where the engine's numbers are coarse | The sceptic; the physicist | Stands | Corrected. Not run again near the ground |
| The one-joint test of jointed rigid bodies uses a joint set up differently from the body's; its "four to eight" is three to eleven | The physicist | Stands | Said so |
| In the air: the weight's course cannot show a torque at all; mirrored limbs hide what slowing does to turning; Long is over the bar proposed (0.076 m/s); the rigid bodies with the keeper's torques lose a quarter of their turning, and the page left it out | The physicist | Stands | The table and its reading rewritten |
| The pull test reported bodies that had already fallen as standing (the first run) | The physicist; the sceptic | Stands; mended before the figures used here | Those lines are not used |
| Cost: lying crowds may be asleep; the crowd at 500 had none standing; one figure of 1.4 ms was left out | The physicist | Stands | Cost is now of twenty-five standing bodies |
| The body is set 2 mm over the ground and dropped; the torque is from the state before the step | The sceptic | True; neither thought the cause | Nothing |
| The engine's other solver was not shown to be in force; start the engine afresh and prove it | The sceptic; the builder | Stands | Not done; first in step 1 |
| What will bite later: feet and what touches the ground; slopes; a leg that cannot give enough; the swinging leg; what is held as part of the body; the engine does not smooth an articulation between steps; things moved once a frame beside a body stepped four times | The builder | Not checked: they are of steps to come | Written into "what follows" |
| A route to leaning and stepping: balance from where the weight is going; a held place that drifts against a steady pull; the hips; a step when the feet cannot reach | The builder (from memory, with names) | Not checked | Step 1's plan |
| Cheaper: nine parts first; or the body in a place of physics of its own | The builder | Not checked | Kept open |

**What the review did:** it took out the step's largest claim before it
reached the plan.

### What was run, and what was not

- `BodysOwnBench` (`Bench`, `OneJoint`, `OneAnkle`; run only when
  asked): the figures above. One run for each cell on the path;
  twenty-five on the floor where it says so. The bench's body is drawn
  as plain rods and boxes; no miner's model is on it yet.
- `PhysicalBalanceBench`, the three miners, seven pulls each: the bar.
- **Not run:** the whole suite and the build (no code of the game was
  changed, and the bench is run only when asked); the built player; a
  slope; a body carrying anything; the engine's other solver with proof
  that it was in force; torques with no springs; the one-ankle and
  one-joint tests near the ground.
- **Judges:** there is no movement to judge in this step; it was
  reviewed (above).
