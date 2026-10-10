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
   stood reliably. How softly the joints are sprung mattered as much as
   the rate (with stiffer springs Round fell at 200 and stood only at
   500). **Looked into the same day, in step 1's first part: with each
   joint's spring set by rule, all three stand at the game's own 50,
   but less steadily than at 100 or 200**
   ([below](#step-1-first-part-how-often-the-physics-steps-october-9)).
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

**Not decided in step 0: how often the physics steps** (taken further
in
[step 1's first part](#step-1-first-part-how-often-the-physics-steps-october-9)).
200 a second stood all three; 100 stood two. The first writing of this page took that for a
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

## Step 1, first part: how often the physics steps (October 9)

**What step 0 left open:** how many steps of physics a second a body
kept up by its own joints needs. All three miners had stood at 200,
two at 100, none reliably at the game's 50. The reviewers of step 0
said the cheapest thing had not been tried: set each joint's spring by
the rule of the engine's makers, and its damper by the turning weight
it works against.

**This part, too, was written twice.** The first writing said "the
game's own fifty steps a second will do for standing". A reviewer
showed that its count was of one start made twenty-five times, and
that the stance at fifty is a poorer one. Different starts were then
counted, at three rates. What is below is what they show.

### In short

- **With each joint's spring set by rule, all three miners stand at the
  game's own fifty steps a second:** a minute on the path, and
  twenty-five of twenty-five on a level floor.
- **But less steadily than at a hundred or two hundred.** Set going at
  a fifth of a metre a second (a light shove), 3 of 24 of Small, 20 of
  Long and 21 of Round were still standing as they stood at fifty steps
  a second; 19, 24 and 19 at a hundred; all 24 of each at two hundred.
  And at fifty the body sags before it stands (Small's and Round's
  heads go 47 mm; 11 and 8 mm at a hundred; 4 and 6 at two hundred).
- **So the rate buys steadiness, and is a trade:** a body costs about
  0.05 ms for each fiftieth of a second at 50 steps a second, 0.11 at
  100, 0.22 at 200 (in the Editor); and above 50 everything else built
  on the physics is stepped finer too. **Not settled.** The keeper is a
  first one; a better one may move the whole table.

### What was done

In the bench, each joint's spring is now no stiffer than the rule
allows, and its damper is set by weight and not by a time:

- **The rule:** a spring has a rate of its own, the root of its
  stiffness over the turning weight of the *lighter* side of its joint.
  That rate times the step is kept at one or under.
- **The damper:** what just stops that lighter side swinging on that
  spring.
- **The lighter side** of each joint is reckoned roughly from the
  body's plan: all that hangs beyond the joint, or all the rest,
  whichever is less.

**What the rule changed.** Of the springs, only the ankles'; of the
dampers, every joint's (the waist's went from about 10 to 39). No run
changed the one without the other.

| At 50 steps a second | Small | Long | Round |
|---|---|---|---|
| The lighter side of an ankle (a boot) | 0.007 kg m² | 0.011 | 0.009 |
| An ankle's spring by its strength alone (soft, 75°) | 46 N m a radian | 108 | 118 |
| **An ankle's spring by the rule** | **16** | **27** | **22** |
| A knee's; a hip's; the waist's (unchanged) | 66; 100; 196 | 154; 231; 265 | 168; 253; 353 |

With springs so soft the ankles hold nothing up: both together are a
sixteenth to a tenth of what the body's leaning asks. What stands the
body is the torques worked out for its legs.

### Standing, one start

Kept by torques worked out for its legs, as in step 0; an
articulation; one on the path, and twenty-five of the same on a level
floor.

| Steps a second | The rule | Small | Long | Round |
|---|---|---|---|---|
| **50** | **rate times step at one** | **stands a minute; 25 of 25 after a minute** | **stands; 25 of 25** | **stands; 25 of 25** |
| 50 | at a half (softer) | stands twenty seconds; but 10 of 25 | stands, wandering 3.8 mm; 25 of 25 | stands; 25 of 25 |
| 100 | at one | stands; 25 of 25 after twenty seconds | stands; 25 of 25 | stands; 25 of 25 |
| 50, no rule (step 0's soft springs) | | falls; 14 of 25 | falls; 0 of 25 | |

(The twenty-five are one start, twenty-five times: they differ only by
where on the floor they stand. They show that a start is not on a
knife's edge, and no more. "Stands" here is only that the head is
above seven tenths of its height.)

**How it stands** (on the path, rule at one):

| Steps a second | Small | Long | Round |
|---|---|---|---|
| 50: its head goes, before it is still | 47 mm | 5 mm | 47 mm |
| 100 | 11 mm | 5 mm | 8 mm |
| 200 | 4 mm | 5 mm | 6 mm |
| Then, at 50, its head keeps within | 0.11 mm | 0.28 mm | 0.12 mm |

At fifty steps a second Small and Round sag into another stance before
they are still: most of their weight on one leg (a knee giving 27 and
67 N m, the other 3 and 14), and the push they want reaching 16 and
23 mm outside their soles. At a hundred and two hundred they do not.
It is a poorer stance, not the same one.

### Standing, twenty-four different starts

Twenty-four of each miner on a level floor, each set going as it begins
at the same speed, each a different way round the compass; counted
after twenty seconds as **standing as it stood** (its head no lower
than nineteen twentieths of where it was, and within a tenth of its
height of there).

| Set going at | 50 steps a second: Small, Long, Round | 100 | 200 |
|---|---|---|---|
| 0.05 m/s | 24, 24, 24 | | |
| 0.1 m/s | 11 (22 still upright), 24, 24 | 24, 24, 24 | |
| 0.2 m/s | **3, 20, 21** | **19, 24, 19** | **24, 24, 24** |
| 0.3 m/s | 1, 4, 6 | 15, 18, 17 | 19, 21, 19 |

(0.2 m/s is what a push of 80 to 175 N gives these bodies in a tenth
of a second. By reckoning, a body that only brings its weight back
over its feet, and takes no step, can stop itself from about 0.2 to
0.35 m/s, by which way it is going.)

**Pulled at the chest** for two and a half seconds, at 50 steps a
second: Round takes 30 N (its head going 17 mm, and back to about a
millimetre, though it was not still within three seconds); at 50 N it
falls. Small and Long fall at 30 N. As in step 0: it does not lean.

### What follows

- **The body can be had at the game's own step,** which the first
  writing of step 0 said it could not. What is already built on the
  physics need not be touched to begin step 1.
- **How often the physics steps is to be chosen when the body is in the
  game,** with its tool and what hangs on it, on the place's own
  ground, against the measures Luis accepted (lean, step and fall at
  today's pulls): not on a bench's level floor. The table above says
  what a faster step buys with this first keeper.
- **The keeper is the next thing, not the step.** It does not lean; it
  sags into a poorer stance at the slowest step; it holds the place
  its weight was, and not where its weight is going.

**Run:** `BodysOwnBench.Bench` with `-ownRule 1 -ownDamped 1 -ownFullAt
75 -ownKinds articulation+torques`, `-ownStep 0.02`, `0.01`, `0.005`;
`-ownCrowd 25 -ownCrowdFor 60` (and `20`); `-ownCrowd 24 -ownJolt
0.05`, `0.1`, `0.2`, `0.3`.

**Not run:** a slope; the path under the twenty-four; starts that
differ in how they lean or what they weigh; the rule at other values
than a half and one; the ankle's spring changed without the dampers;
the engine's other solver with proof.

### What the reviewer said

One reviewer (a separate agent, the sceptic) was given the first
writing of this part, the bench and its results.

| What was said | Checked | Done |
|---|---|---|
| Every figure matches the results | | |
| "25 of 25" is not a count of different starts: the twenty-five differ only by where they stand; and the level floor is easier than the path (before the rule, one fell on the path while 14 of 25 stood on the floor) | Stands | Twenty-four different starts counted, at three rates, with a stricter measure of standing |
| "Stands" is only the head above seven tenths of its height: a body sunk, leaning or on one leg passes | Stands | The stricter measure; and the stance is described |
| "Only the ankles" is true of the springs; the dampers changed at every joint. "It was the ankle" is not shown alone | Stands | Said so. Not run apart |
| The 47 mm is not the path's slope: at 100 steps a second it is 11 and 8 mm, at 200, 4 and 6. It is a poorer stance of the slowest step, with the push wanted outside the soles | Stands (the results were there; I had read them wrongly) | Said so, with the table |
| The lighter side is reckoned crudely (parts as points at their middles); a joint that turns three ways has one spring for all, and its twisting may break the rule | Not checked | Kept as a caution; for the body in the game |
| What holds it up is the worked-out torques: the ankles' springs are a sixteenth to a tenth of what its leaning asks | Stands | Said so |
| A joint could give twice its strength (as in step 0) | Stands | For the keeper in the game |
| Round "comes back to about a millimetre" leaves out that it was not still within three seconds | Stands | Added |
| The trial most likely to break it: truly different starts, a slope, a shove, the rule swept, 50 against 100, judged by lean and hips and sole margin | In part done (different starts, 50 against 100 and 200) | The rest listed as not run |

## Step 1, the rest of it: the body in the game, and the life in it (October 9 and 10)

Luis, October 9, after the first part: "Shall we proceed then with the
next part then?" The next part was: the body's own body in the game
beside the posed one, with a switch on the panel; a keeper that leans
as today's body does; then life in it.

### In short

- **It is in the game, beside the posed body, and off unless it is
  switched on.** The miners' panel has a switch, "Stands by its own
  joints". With it on, the chosen miner, at ease and with empty hands,
  stands by what its own hips, knees and ankles give. Nothing of what
  Luis has is changed with the switch off.
- **It stands as still as was asked.** With no life in it: all three
  miners, twenty-five seconds on the path, their heads within 0.00 to
  0.07 mm once settled (they settle 7 to 12 mm from where the posed
  body had them). Nudged four ways (set going at 0.15 m/s), their heads
  go 11 to 28 mm and are back within 6 mm five seconds after.
- **There is life in it,** and none of it is a pose put on the body: it
  breathes (15 breaths a minute at rest, 30 to 36 tired); its weight
  goes from one leg to the other every nine to twenty-six seconds;
  where it holds its weight drifts by a few millimetres between; its
  head looks somewhere else every four to eleven seconds, at another
  miner if one is near. Each is something its joints are asked for, and
  the body answers as its weights let it.
- **It gives its body back when anything else is asked of it,** and
  takes it again: sent somewhere it walks as it did (posed) and stands
  by its own joints again three to four seconds after it was sent;
  pushed harder than it can stand, it goes over into the fall that was
  built, gets up, and stands so again.
- **Three things Luis should know before looking:**
  1. **It cannot step, so it cannot yet do what today's posed body does
     when it is pulled.** In the bench it held pulls of 20 to 30 N
     (Round: to 65 N); today's posed body leans against 50 to 150 N and
     steps beyond. A body on flat feet that may not step cannot be made
     to: the posed body is not bound by what feet can do. Stepping is
     step 3 of the plan.
  2. **For the same reason its shift of weight is small** (about two
     thirds of its weight on a leg; its hips 2 to 3 cm over; its hips
     rolled 3 degrees). A fuller one (the hip out) was built, and a
     miner standing so went over on a fourth nudge in a row. A body on
     one leg saves itself from a push by stepping.
  3. **Its breath can hardly be seen.** Drawn at the size it has in
     life it cannot be seen at all; at two and a half times that, three
     judges of three could not see it; it is now drawn at four times.
     How breath is shown is Luis's to choose (answer B4): the panel has
     the three looks, and other ways are open (below).
- **The plan's measure "its head wanders no more than a centimetre or
  two" is not met as written,** and I think it should be read anew: over
  a minute a miner's chest keeps within 2 to 3 cm of where it began,
  and its head within 3 to 4 cm across; nearly all of that is its
  weight going over a leg, and its head turning (a head that turns
  moves its own middle). Between shifts it keeps within millimetres.
  Put to Luis below.

### What is which

| | |
|---|---|
| **The body's own** | The standing: thirteen parts on joints, held up by torques at its hips, knees and ankles, each no more than the joint has. Its breath's movement, its weight going over, its head turning: asked of its joints as a turn from the pose, answered by the body |
| **Drawn, not physical** | The chest drawn fuller with breath (the chest bone made larger; what is on it moved, not made larger) |
| **Still posed** | Everything else a miner does: walking, turning, picking up, work. The pose it stands in when it begins (its stance, its bent knees, its arms a little away from it) is the posed body's, taken as it is |
| **Handed over** | To the fall that was built, when it is down. Back to the posed body, over a quarter of a second, when it is sent somewhere, put to work, or switched off |

### The keeper

Each step of the physics (`OwnBody.Keep`; first written and searched in
`BodysOwnBench`):

1. **Where the ground really bore it** in the last step is read from
   what touched its boots (`GroundTouch`).
2. **How high its weight is** is kept by a little more or less push
   than its weight.
3. **Where its weight is going** is its place and its speed over the
   rate at which a standing body falls away from where the ground
   pushes it (the root of gravity over its height). The push it wants
   is beyond where its weight is going, by how far that is from where
   it means to hold it; and further for a push from outside, which it
   feels by how its weight really went.
4. **Where the push can act** is on its soles and nowhere else: each
   boot bears its share along its own middle line, and the point is
   kept inside the sole.
5. **Each leg's hip, knee and ankle give** what makes its boot push the
   ground so. Its hips keep its trunk upright. A hinge gives only about
   its own line. Nothing gives more than it has; every torque is put on
   the two parts a joint joins, equal and opposite.
6. **It leans against what it feels,** for what it cannot take standing
   as it is, keeping sole behind its weight.
7. **Every joint is held softly towards its pose** by the joint's own
   spring, set by the rule of step 1's first part.

Its fifteen settings were **found by a search on Small, Long and Round
together** at the game's fifty steps of the physics a second
(`BodysOwnBench.Search`), not set by hand: twenty-five bodies of each
(one left alone, sixteen set going at 0.12 and 0.2 m/s eight ways,
eight pulled with a twentieth and an eleventh of their weight and let
go), scored by how many still stand as they stood and how still. **The
search gamed what it was asked once:** a body that fell in the first
second "stood as it stood" where it lay. What it is asked now also
needs the head to be where it was made.

| Setting | Found | |
|---|---|---|
| Quick | 6.33 a second | how quick it brings its weight back |
| Feels | 0.040 s | over how long it comes to feel a push |
| Leans, by, beyond, keeping behind | 2.90 a second, 0.94, 33 mm, 21 mm | how it leans against one |
| Tracks | 6.87 a second | how fast it learns the ground's push falls short of the one it meant |
| A joint's spring: full at, rule, damped | 92.7 degrees, 0.64, 0.38 | (step 1's first part) |
| Rights, damped | 5.40 for each kilogram, 0.05 | its hips righting its trunk |
| Edge | 4 mm | how near a sole's edge the push may act |
| Rises, damped | 224, 14.3 | keeping its height |

In the bench, with these: all three stand a minute on the path at
fifty steps a second (within 0.2 to 0.3 mm once settled); twenty-four
of twenty-four of each keep their feet set going at 0.2 m/s (they were
3, 20 and 21 of 24 with the first keeper); pulls held: Small and Long
20 and 30 N (down at 40), Round 20 to 65 N (down at 80).

### Three faults found by tracing, and mended

| What was seen | What it was | What was done |
|---|---|---|
| Small went down the moment it stood by its own joints again after a walk | It began while the posed body was on its closing step, one boot in the air (the posed body going 42 mm/s) | It begins only when the posed body has had both boots down, at rest, for four tenths of a second |
| Tired at once after several nudges, Small went over sideways | Where it stands was a place on the ground. Its boots had slid under the nudges, and its whole weight was left on one of them | Where it stands is kept by its boots: between them, as far across and ahead as when it began |
| On one leg and nudged, both boots tipped twenty degrees and left the ground, and it went over backwards | What the keeper had learnt of the ground's push falling short (up to 5 cm) was added to where it meant the push to act *after* that had been kept inside the sole: it asked for a push beyond the sole's edge, and a push asked for where there is no sole only tips the boot. The search never saw it (it does not look at boots) | The sum is kept inside the sole. After it, all three miners held every nudge on one leg |

### The life in it

What real bodies do standing, and how it is drawn, was read first:
[the research, part 6](../Research/2026-10-09_AliveAndTheBodysOwn.md#6-standing-at-ease-breath-weight-head-read-for-step-1).

| | What its joints are asked | Sizes now (set by hand and by what the judges said, not searched) |
|---|---|---|
| **Breath** (`Breath`, a clock of its own) | Its back to straighten as its chest fills, its arms to go out. And its chest is drawn fuller | 15 a minute at rest, to 42 wholly out of breath, and to two and a half times as deep; at the default look ("drawn stronger", four times life) the back 1 degree, the arms 2.5, the chest 9 parts in a hundred at rest. No two breaths quite as long or as deep; no two bodies in step |
| **Weight from leg to leg** | The keeper holds its weight nearer one boot. Its hips to roll, a quarter of a second behind its weight; its trunk to stay upright with its shoulders a little the other way; the other knee to ease, later still | Every 9 to 26 s, over about 1.6 s in one hump of speed; 0.28 of the way to a boot (about two thirds of its weight on that leg); three times in ten only a little further on or off the leg it is on. Pushed, it stands square |
| **Never quite still** | Where it holds its weight drifts, two slow turns along it and two across | 4 mm |
| **Its head** | To turn to where it looks, and to keep to it whatever the body under it does; between looks to wander a little | Every 4 to 11 s, to 40 degrees aside, in 0.25 to 0.5 s; at another miner nearer than 15 m about one time in three; wanders 1.5 degrees |
| **Lying after a fall** | (Drawn only) its chest drawn fuller with each breath, faster and deeper the more shaken and out of breath | The fall itself is as it was |

The miners' **eyes are not modelled apart from their heads** and they
do not blink, so the head says where a miner looks; a real head would
turn less often and less far, with the eyes doing most of it.

**Tried and taken out** (each made a miner go over under a light
nudge, or did nothing):

- Breath lifting the whole body by its legs (8 mm). Long, on one leg
  and nudged, went 11 to 13 cm and fell once in three runs; 4 to 6 cm
  without it.
- A fuller shift of weight (0.4 to 0.5 of the way to a boot, with
  larger rolls). Went over on a third or fourth nudge in a row.
- Its trunk asked to keep upright in the world, step by step. It threw
  its hips about when nudged. It is asked against its hips, taking up
  the roll they are seen to have, slowly.
- Its chest following a far look. Not the cause of anything; one thing
  less asked of the trunk.
- **Still in, and never seen to act:** it goes over a leg only as far as
  that leg's joints bear with ease (half of what they have). In every
  test they were asked for 17 to 26 hundredths. It is meant for a weak
  or a tired body, and was not tried on one.

### Figures (the final state, Small / Long / Round)

`OwnBodyTests`, six tests, on the path of the ordinary place, at the
game's fifty steps a second.

| | Small | Long | Round |
|---|---|---|---|
| **No life in it.** Settling, its head goes from where the posed body had it | 6.5 mm | 10.3 mm | 12.2 mm |
| ...then, for twenty seconds, keeps within | 0.00 mm | 0.01 mm | 0.07 mm |
| ...set going at 0.15 m/s four ways, its head goes at most | 13 to 24 mm | 23 to 28 mm | 11 to 24 mm |
| ...and is back within, five seconds after | 0.3 to 1.8 mm | 1.6 to 5.7 mm | 0.4 to 2.7 mm |
| **With life in it, a minute.** Breaths | 15 | 15 | 15 |
| Its back straighter with its chest full than empty | 1.2 degrees | 1.1 | 1.0 |
| Its weight went to the other leg | 3 times | 2 | 3 |
| On its right leg / its left: its hips across | +21 / -17 mm | +21 / -29 | +21 / -28 |
| ...its hips roll | 3.1 / -2.8 degrees | 2.5 / -3.2 | 2.0 / -3.1 |
| ...the knee of the leg it stands on / the other | 27 / 34 degrees | 26 / 31 | 27 / 34 |
| Its chest goes at most (across / along) | 21 / 19 mm | 27 / 12 | 28 / 7 |
| Its head goes at most (across / along) | 41 / 43 mm | 42 / 37 | 34 / 19 |
| Its head turns back across it (a tremble would be more than twice a second) | 0.8 a second | 0.7 | 0.6 |
| Asked to look 40 degrees to its right, its head turns | 40 degrees | 40 | 41 |
| **On one leg wholly, set going at 0.15 m/s four ways,** its head goes at most | 15 to 20 mm | 36 to 61 mm | 37 to 83 mm |
| Tired at once, it breathes | 31 a minute | 36 | 30 |
| **Set going backwards at 1.2 m/s,** it is down, and the fall has it, after | 0.5 s | 0.6 s | 0.5 s |
| ...it is up after / stands by its own joints again after | 9.0 / 10.1 s | 10.8 / 12.2 s | 9.6 / 10.8 s |
| **Sent 2.5 m,** it gives its body back, walks, and stands so again after | 3.6 s | 3.1 s | 3.4 s |
| It went down in any of it but where it was meant to | no | no | no |

The whole suite: 170 pass, none fail, 21 are run only when asked (the
benches and recordings). The build is made from this state.

### What it costs

Not measured again in the game. In the bench a standing body of its own
cost about 0.05 ms of each fiftieth of a second at fifty steps a second
(in the Editor). One miner at a time stands so (the chosen one); a
crowd keeps the posed body.

### Not done, and not tested

- **Stepping** (the plan's step 3), and with it leaning as today's body
  does, a fuller shift of weight, and any pull beyond a few tens of
  newtons.
- **A slope.** Every test is on the path, which is nearly level.
- **Hands with something in them.** It stands so only with empty hands.
- **More than one miner standing so at once.** The panel keeps the
  chosen one so.
- **Many nudges in a row, as Luis may give them.** Four in a row on one
  leg were held by all three; a body nudged again and again, each time
  before it has settled, was not tried, and its boots slide a little
  under each (Small's came 3 cm nearer each other over four).
- **Long is the least sure:** in an earlier state it went 13 cm under
  one nudge and stood. It is the tallest on the narrowest stance.
- **What a far crowd does;** what it costs in the built game.
- **Whether the engine's joint springs twist the rule** for a joint that
  turns three ways (a caution of step 1's first part): not looked into.
- **How often the physics steps** is still the game's fifty; a hundred
  would be sturdier (step 1's first part), and is not chosen.
- **A weak or tired body's limit on shifting** (above): never seen to act.

### What the judges said

Two rounds, three judges each, on sheets of pictures of the three
miners ([all of it](../Reviews/2026-10-10_StandingWithLife_Judges.md)).

- **Stable: three of three, both rounds.** Boots flat and planted in
  every picture; nothing snaps.
- **Alive only in its head: three of three, both rounds.** "A still body
  with a roving head." Its breath could not be seen at two and a half
  times life nor at four; its weight going over a leg could not be
  seen; its head turns alone, nothing leading it and nothing following.
- **What was done between the rounds:** breath drawn larger; the head
  turning less often and never quite fixed; and three things tried and
  taken out again because a nudged miner went over (above).
- **What is left open,** for Luis: how breath is to be shown; that a
  real shift of weight waits on the step; its chest following a look;
  and how the models are made to stand (Long's chin up, the arms in an
  A, Round's shut eyes), which a still body looked at closely shows.

Pictures: [Long, its weight asked onto its right leg and then its left](../Images/StandingWithLife_2026-10-10/long_shift.jpg);
[Small, breathing, from the side](../Images/StandingWithLife_2026-10-10/small_breath.jpg);
[Small, asked to look to its right](../Images/StandingWithLife_2026-10-10/small_look.jpg);
[Round, nudged](../Images/StandingWithLife_2026-10-10/round_nudged.jpg);
[the three, their chests empty and full, tired](../Images/StandingWithLife_2026-10-10/chest_empty_full.jpg).
