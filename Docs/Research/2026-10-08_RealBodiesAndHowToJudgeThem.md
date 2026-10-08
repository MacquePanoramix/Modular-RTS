# What real bodies do, and how to judge ours — research of October 8, 2026

**Why this exists.** Luis, after playing S3
([the message](../Correspondence/2026-10-08_THE_PLAYTEST_NO_FAKE_ANIMATIONS.md)):
"research online about all of these things and do a lot of research and
make it all very documented and organized. And search ways that we can
really fully optimize the way to check every animation, make everything
natural, and debug everything."

**What it is for.** Each of Luis's ten notes is about something that does
not read as real. To put one right, two things are needed first: what a
real body (or a real meadow, or a real lamp) does, in figures where there
are any; and a way to tell whether ours does it. This page gathers both.
It is a reference, not a decision: nothing here is Locked by being written
down.

**How to read it.** One section to each subject. In each: *what was
found* (with where it comes from), then *what we hold ours to* (the
measures and the questions a judge asks), then *what is not known*.
The sources are of uneven weight, and each section says which kind it
leans on: papers, teaching material, or forum practice.

**How far to trust a figure.** The pages were found by search and read
through its summaries; where a page was opened and read, the figure is
from the page. A figure marked † comes from the search's summary of a
page that would not open (the publisher refused the request): it is
likely right, and it has not been seen in its own words.

| Section | Luis's note | In one line |
|---|---|---|
| [1. Fireflies](#1-fireflies) | L1 | Each flies low over its own ground and flashes for under a second every few seconds |
| [2. Shadows far from the eye](#2-shadows-far-from-the-eye) | L2 | The pipeline stops drawing shadows at a set distance from the camera |
| [3. Walking](#3-walking-the-rise-and-fall-of-the-body) | L3 | The body rises and falls about 4 to 5 cm a step, smoothly; the knee gives at each landing |
| [4. Turning round on the spot](#4-turning-round-on-the-spot) | L4 | Two to three steps, about a second and a half, the head first |
| [5. Clothes on a bending body](#5-clothes-on-a-bending-body) | L5 | Hanging cloth breaks when the body moves into it faster than it can answer |
| [6. Going down for something](#6-going-down-for-something-on-the-ground) | L6 | People choose a half squat; a whole lift takes four to six seconds |
| [7. Getting up from the ground](#7-getting-up-from-the-ground) | L7 | Young adults take about 3.7 s, by way of a side sit or all fours |
| [8. Resting from work](#8-resting-from-work) | L8 | A muscle holding more than about 15% of what it has does not rest |
| [9. Reaching, taking and holding](#9-reaching-taking-and-holding) | L9 | The hand speeds up and slows down, and opens widest two thirds of the way |
| [10. What makes a movement read as real](#10-what-makes-a-movement-read-as-real) | L10 | Nothing moves at one speed, in a straight line, or all at once |
| [11. Ways of checking](#11-ways-of-checking) | the method | Numbers first, then judges who did not make the thing, then Luis |

---

## 1. Fireflies

**What was found** (species accounts and field studies):

- **The flash.** A male of the common North American *Photinus pyralis*
  glows for about three quarters of a second and does so again every five
  to six seconds; *P. marginellus* flashes for under half a second every
  three seconds; other species give double flashes half a second to two
  seconds apart, or a single flash every ten seconds or more
  ([Silent Sparks, a guide to flash patterns](https://silentsparks.com/2018/06/02/a-guide-to-lightningbug-linguistics/);
  [US National Park Service](https://home.nps.gov/grsm/learn/nature/firefly-flash-patterns.htm)).
  So a firefly is dark most of the time: lit for about a seventh of it.
- **The height.** Searching males fly about 1 to 1.5 m above the ground,
  often in a straight line; later in the night some species fly 2 to 4 m
  up. Females sit low on the plants (a mean of 27 cm in one study) and
  answer from there. In grassland most are below 1.5 m
  ([Cambridge Entomological Club, *Psyche*](https://groups.csail.mit.edu/mac/projects/psyche/98/98-293.html);
  [Journal of Threatened Taxa, 2025](https://threatenedtaxa.org/index.php/JoTT/article/view/9656);
  [swarms of *P. carolinus*, 2021](https://www.biorxiv.org/content/10.1101/2021.01.26.428319.full.pdf)).
- **They are where they are.** A swarm filmed in three dimensions sits in
  a thin layer about a metre over the ground and follows the shape of
  the ground (the same 2021 study).

**What we hold ours to:**

- Each firefly belongs to a place in the world, low over *its own*
  ground. The camera has no say in where it is.
- Most are dark at any moment.
- What distance changes is what is *seen*: a small light gives the eye
  less the further away it is, so from far the faint ones drop out of
  sight first and the rest are fainter. (This is Luis's own description
  of what is wanted.)

**What is not known.** No figure was found for how many fireflies there
are to a square metre of meadow; counts in the field vary by orders of
magnitude with the place and the year. How many there should be is a
matter of taste, and Luis has said twice that there were too many.

## 2. Shadows far from the eye

**What was found** (Unity's manual, and the pipeline's own source):

- The Universal Render Pipeline draws shadows only out to its *Max
  Distance* from the camera, and spreads its shadow map over that
  distance: the further it reaches, the coarser the shadows near by
  ([Unity manual: shadow resolution](https://docs.unity3d.com/Manual/urp/shadow-resolution-urp.html);
  [shadow optimisation](https://docs.unity3d.com/Manual/shadows-optimization.html)).
  Cascades give the near ground more of the map than the far.
- The pipeline fades shadows out over the last part of that distance,
  for the sun *and* for lamps: the source (`MainLightShadowCasterPass`,
  `AdditionalLightsShadowCasterPass`, `ShadowUtils`) fades both by the
  same distance and the same border.
- Light passing through walls is a known family of faults with several
  causes: too much shadow bias, too coarse a map, too many lights
  ([Unity manual: troubleshooting shadows](https://docs.unity3d.com/Manual/urp/shadows-troubleshooting-urp.html)).

**What it meant here.** Ours was none of the usual causes: the shadows
simply ended. With the distance at 50 m, the house's lamps stopped being
shadowed by its walls between 46 and 51 m from the camera
([the playtest round, L2](../Reviews/2026-10-08_ThePlaytestRound.md)).

**What we hold ours to.** Nothing that is lit changes because the camera
moved. The measure: the brightness of the same patch of ground, at each
step of the zoom with time standing still, never rises from one step to
the next.

## 3. Walking: the rise and fall of the body

**What was found** (teaching material for gait analysis; standard values
that the texts agree on, but not checked here against a primary source):

- The body's centre rises and falls once a step, in a smooth wave of
  about 4 to 5 cm in all. It is highest in the middle of the time on one
  leg (the body passing over the foot) and lowest when both feet are
  down ([American Academy of Physical Medicine and Rehabilitation: biomechanics of normal gait](https://now.aapmr.org/biomechanics-normal-gait/);
  [Newcastle FRCS revision notes on normal gait](https://lists.ncl.ac.uk/wws/d_read/orthnorth/Basic%20Science%202013/Normal%20gait%20-%20FRCS%20revision.pdf)).
- The knee is almost straight when the heel lands (0 to 5 degrees),
  bends to 15 to 20 degrees as the leg takes the weight, and
  straightens again by the middle of the stance. That give at the knee is
  one of the things that keep the rise and fall smooth; it bends to about
  40 degrees before the foot leaves and 60 in the swing
  ([Wheeless' Textbook of Orthopaedics: the knee in locomotion](https://www.wheelessonline.com/joints/knee/role-of-knee-joint-in-locomotion/)).

**What goes wrong in made walks** (tutorials and forum practice):

- A foot that is held to the ground and then let go, or a pelvis
  corrected afresh every frame, makes a break at the moment a
  constraint switches. The cure is to spread the change over the frames
  round it ([Kovar, Schreiner and Gleicher: footskate cleanup](https://research.cs.wisc.edu/graphics/Gallery/kovar.vol/Cleanup);
  [Vulkan tutorial: foot placement](https://docs.vulkan.org/tutorial/latest/Advanced_glTF/Procedural_Animation_IK/04_foot_placement.html)).
- A leg close to straight needs a large change at the knee for a small
  move of the ankle: the knee pops (the same Kovar paper).
- Physics that steps fifty times a second, shown at a hundred frames a
  second, stands still on one frame and jumps on the next unless what is
  shown is interpolated between steps
  ([Unity: Rigidbody interpolation](https://docs.unity3d.com/ScriptReference/Rigidbody-interpolation.html);
  [gamedev.net: fixing the timestep](https://gamedev.net/blogs/entry/2265460-fixing-your-timestep-and-evaluating-godot/)).

**What we hold ours to:**

- The height of the hips over a step is one smooth wave: no frame in
  which it changes by more than its neighbours do. The measure is taken
  **at the frame rate Luis plays at** (about 100 a second), not only at
  the physics' 50.
- A planted foot does not move over the ground; a foot that lands is
  not moved after it has landed.
- Nothing that is shown stands still on one frame and jumps on the next.

## 4. Turning round on the spot

**What was found** (gait and neurology papers; most were written about
older or ill people, with healthy adults as the comparison):

- **How long, how many steps.** From standing, healthy adults turn half
  round in a median of 2 steps and 1.5 s when free to choose the way,
  and 3 steps and 1.6 s when told which way †
  ([Oxford NDORMS: the standing start 180 degree turn test](https://www.ndorms.ox.ac.uk/publications/487235)).
- **The order.** The head turns first, before the shoulders, the pelvis
  and either foot; younger people turn head, chest and pelvis one after
  the other, older people more as one block †
  ([University of Southampton: turning half round, in people with Parkinson's disease and without](https://eprints.soton.ac.uk/18058);
  [Frontiers in Neurology, 2017](https://www.frontiersin.org/articles/10.3389/fneur.2017.00194/pdf)).
- **Two ways.** In a *step turn* both feet go on stepping and each hip
  serves as the axis for part of the turn; in a *spin turn* the body
  turns on the ball of one foot. For half a turn the step turn is the
  easier: its base is broader, and it keeps the rhythm of walking
  (Hase and Stein, 1999, as reported by
  [Golyski and others, 2017](https://sites.gatech.edu/p-golyski/wp-content/uploads/sites/755/2018/01/Golyski-2017-A-computational-algorithm-for-classifying-step-and-spin-turns-using-pelvic-center-of-mass-trajectory-and-foot-position.pdf)).
  A spin turn puts the body's weight outside its feet and asks more of
  the joints.

**What we hold ours to:**

- Half a turn takes about a second and a half, in two or three steps.
- The head looks round first; the chest follows; the feet step round
  under it. Not the whole body as one block.
- No foot turns on the ground while it bears weight further than a
  foot can (a little, on its ball); and no foot is anywhere it did not
  step to.

## 5. Clothes on a bending body

**What was found** (tool documentation and forum practice; no paper
describes this fault itself):

- Hanging cloth made of chains of bones on springs is pushed out of the
  body by shapes round the limbs. The usual layout is a capsule from
  each hip to each knee, sized from the body
  ([a skirt tool's description](https://booth.pm/en/items/8930315)).
- The known cures for cloth that flies apart: a limit on how far each
  joint may bend; damping that is not near zero; more steps of the
  solver; a reset of the cloth's state after a sudden move; and, for a
  squat, shapes driven by the legs' own angle instead of collision
  ([Resonite: dynamic bone chains](https://wiki.resonite.com/Dynamic_bone_chain_parameters);
  [Godot: jiggle bones](https://docs.godotengine.org/en/4.7/classes/class_skeletonmodification2djiggle.html);
  [Automatic Dynamic Bone](https://github.com/OneYoungMean/Automatic-DynamicBone/wiki/Automatic-Dynamic-Bone-Tutorial)).

**The likely causes, reasoned and to be checked on ours:** the thighs
come up into the cloth faster than it can be pushed out, so the
correction overshoots; the shapes at hip and knee push the same piece
two ways; the pose changes by a large step in one frame.

**What we hold ours to:** no piece of clothing passes through the body
or stands away from it by more than its own cut allows, at any frame of
the deepest bend; and no piece moves faster than the limb it hangs from.

## 6. Going down for something on the ground

**What was found** (ergonomics and biomechanics papers, small samples):

- **Which way people choose.** Left free, people use something between
  a squat (knees bent, back upright) and a stoop (knees straight, back
  bent). In one study twelve of thirteen preferred the *half squat*, and
  lifted more with less effort that way than in a full squat
  ([Curtin University, in *Work*](https://content.iospress.com/articles/work/wor00279)).
- **No one way is right.** For the load on the low back, which is best
  depends on the thing and where it lies; with a wide box from near the
  floor the squat was the *worst* of four
  ([Kingma and others, *Physical Therapy* 2006](https://research.vu.nl/ws/files/43522568/Kingma_PhysTher_2006.pdf)).
- **Light things, one hand.** The *golfer's lift*: one foot stays down,
  the other leg goes back and up as the body tips forward over the hip,
  so that the leg's weight answers the trunk's; it suits light things
  and sore knees, and gives up steadiness for reach
  ([Workplace NL](https://workplacenl.ca/resource/safety-share-golfers-lift)).
  *One knee down* keeps the back upright and is preferred for lifts to
  the side ([NIOSH](https://stacks.cdc.gov/view/cdc/227278)).
- **How long.** A whole lift of a 15 kg box, down and up: 4.6 s as
  people chose to do it, 4.9 s as a squat, 5.9 s as a stoop
  ([von Arx and others, 2021](https://www.frontiersin.org/journals/bioengineering-and-biotechnology/articles/10.3389/fbioe.2021.769117/pdf)).
- **What bears it.** In a squat the hips and ankles give most of what
  holds the body up; in a stoop the knee's part is larger
  ([Hwang and others, 2009](https://pmc.ncbi.nlm.nih.gov/articles/PMC2651112)).

**What we hold ours to:**

- The body chooses its way down by what it is and what the thing is,
  and may choose differently: a light thing with one hand and a leg
  going back; a heavy thing between the feet with both.
- Hips, knees, back and the reaching arm all move through the same
  stretch of time, none finishing while another has not begun; the
  weight stays over the feet all the way.
- About two seconds down and two up for something heavy; quicker for
  something light.

**What is not known.** No timing was found for the one-handed or the
kneeling lifts.

## 7. Getting up from the ground

**What was found** (physical therapy studies; character animation
research):

- **How long.** Adults of 20 to 50 take a median of 3.7 s to rise from
  the floor; over 60, 5.7 s. Leg power goes closely with the time †
  (Schwickert and others, 2016:
  [getting up from the floor, younger and older adults](https://infoscience.epfl.ch/record/219346)).
- **Which way.** Three ways are seen among healthy adults over fifty:
  from a *side sit* through half kneeling (half of them); pushing up
  from *all fours* (a third); and *sitting up* and rolling forward over
  the feet (the rest)
  ([Bohannon and Lusardi, 2004](https://digitalcommons.sacredheart.edu/pthms_fac/43)).
  Young adults most often rise through an uneven squat, one hand
  leaving the floor before the other, the trunk turning a little
  (VanSant's studies, as reported by a
  [Pacific University thesis](https://commons.pacificu.edu/works/publication-dissertation/8evd5-4x010)).
  Stronger, more supple people use the more direct ways.
- **Made by forces alone.** A character can be made to get up with no
  recorded motion at all, by learning: first with a body much stronger
  than it should be, then made weaker step by step, then slowed. The
  authors found that the weaker bodies (40 to 60% of the strength they
  began with) moved the most naturally, and that the strong ones were
  "overly dynamic". Their slow get-ups can be stopped at many points and
  hold still there
  ([Tao and others, *Learning to Get Up*, SIGGRAPH 2022](https://arxiv.org/abs/2205.00307)).
  Another approach uses a simple model, like the upside-down pendulum
  used for stepping, to plan where the body's centre, hands and feet go,
  and has the body follow it by forces, with no keyframes
  ([*Real-Time Character Rise Motions*](https://arxiv.org/abs/2304.05056)).
  Older work joins several hand-made controllers, each of which knows
  from which lying positions it can start
  ([Faloutsos, van de Panne and Terzopoulos, SIGGRAPH 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)).
- **What games usually do** is what Luis saw: the body falls as a rag
  doll, then is blended into a recorded get-up. The forums are about
  hiding the join
  ([Unreal forum](https://forums.unrealengine.com/t/smooth-transition-between-ragdoll-and-animation/372228)).

**What we hold ours to:**

- At every moment of getting up the body is held up by what touches
  the ground and by nothing else: its weight is over its hands, knees
  and feet, in turn. If it were frozen at any moment before the last
  push, it would stay where it is.
- It gets up by stages a person would name: rolls to its side or
  front; gets its hands under its shoulders; pushes up to all fours or
  a side sit; brings a foot under it; rises over that foot.
- About three to four seconds; longer for a weak or tired body, which
  may also fail and try again.
- It is not stronger getting up than it is doing anything else.

## 8. Resting from work

**What was found** (ergonomics; the original curve is Rohmert's of 1960,
read here through later sources):

- How long a muscle can hold a force falls steeply with the share of
  its greatest force that it is giving. The working rule drawn from it:
  a force held for long should be no more than about 15% of the
  greatest ([Ergoweb: static load](https://ergoweb.com/static-load/)).
- It is a rule for design, not a line below which a hold lasts for
  ever: in measurements, a grip at 15% lasted six to eight minutes, and
  a hold at the shoulder ended even at 5%
  ([Heinzl and others, 2025](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC11707015/);
  Garg and others, as cited there). Different joints tire at different
  rates ([Frey Law and Avin, 2010](https://iro.uiowa.edu/esploro/outputs/journalArticle/Endurance-time-is-joint-specific-A-modelling/9984046821002771)).

**What we hold ours to:**

- A rest is a way of standing in which no muscle gives more than it
  can go on giving (ours: 18% of what it has now). A body "resting"
  with an arm or its knees at work is not resting, and must find
  another way to stand, or put the thing down.
- Resting ends: by the body being fresh enough, or by the rest having
  given all it can.

## 9. Reaching, taking and holding

**What was found** (motor control papers):

- **The hand's speed** on its way to a thing rises and falls in one
  smooth hump, on a nearly straight path; its greatest speed comes
  before the middle (about 40% of the way through in one set of healthy
  people, over about one second)
  ([Jeannerod's finding, in Hu and others, 2005](https://bicr.atr.jp/~kawato/Ppdf/YaopingEBR05r.pdf);
  [table of healthy controls](https://pmc.ncbi.nlm.nih.gov/articles/PMC8891374/table/T2)).
- **The hand opens on the way.** It opens gradually to wider than the
  thing, is widest at 60 to 80% of the way (about two thirds), and
  closes onto the thing as it arrives. The fingers take the thing's
  shape before they touch it
  ([Hill and others, 2011](https://eprints.soton.ac.uk/272553/1/HillISB2011.pdf)).
- **Carrying past something** in the way makes two humps of speed and a
  longer slowing ([Valevicius and others, 2018](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6013217/)).

**What we hold ours to:**

- The hand slows into the thing; it does not arrive at speed and stop.
- The hand is open before it gets there and closes on arrival; the
  thing does not move until the hand has closed.
- Once held, the thing hangs or rides as its weight makes it: a
  lantern by its handle hangs straight down from the hand and swings
  when the hand moves; a mug is kept level by the wrist. The arm that
  holds it carries it where an arm carries such a weight without
  effort: close to the body, the elbow down.

**What is not known.** Nothing was found on how people carry a lantern
or a mug in particular; the last point is reasoning from the weight,
and from section 8.

## 10. What makes a movement read as real

**What was found** (animation teaching; the principles go back to Thomas
and Johnston's *The Illusion of Life* and Lasseter's 1987 SIGGRAPH paper,
which were not read here):

- **Nothing moves at one speed.** A thing that starts, moves and stops
  at a constant rate looks like a machine. Real things gather speed and
  lose it ([Adobe: easing](https://www.adobe.com/uk/creativecloud/animation/discover/easing.html)).
- **Nothing moves in a straight line.** Limbs swing about joints: their
  ends travel on arcs
  ([University of Washington course notes](https://courses.cs.washington.edu/courses/cse458/21au/content/exercises/animation_principles.html)).
- **Nothing moves all at once.** In an arm coming down the upper arm
  leads, the forearm follows, the hand follows that; each part arrives
  after the one it hangs from. But not strictly one after another
  either: reaching to catch a ball, the eyes, arms and hands act
  together (the same notes).
- **Nothing stops dead.** When the body stops, what hangs from it goes
  on and settles: hair, clothes, a tool. How far it lags tells its
  weight.
- **A big movement is prepared.** A body crouches before it jumps and
  draws back before it strikes.
- **Weight is told by time.** Heavy things start and stop slowly.
- **Better motion is never worse.** In one study, making a character
  move better made it more acceptable, most of all for the character
  people had liked least; the fear that good motion on an imperfect
  figure makes it eerier was not borne out
  ([Piwek, McKay and Pollick, *Cognition* 2014](https://eprints.gla.ac.uk/105081)).

**What this means for a body moved by forces.** Ours gets most of these
for nothing when the forces are honest: a thing with mass cannot start
at full speed, and what hangs will follow through. Where a movement of
ours reads as "an animation", one of these has been broken, and that
tells where the fault is:

| What the eye sees | What it usually is, in a body like ours |
|---|---|
| A part arrives at speed and stops | A place was *set*, not reached: no mass in between |
| Everything starts and ends together | One clock drives every joint: a blend from pose to pose |
| A thing changes place between two frames | A rule switched (a foot let go, a state changed) and nothing spread the change |
| The body stays up where it should fall | Something other than its feet and hands is holding it |
| No answer to what it touches | The movement was planned without the thing, and runs the same without it |

## 11. Ways of checking

Luis asked for a search of ways "to check every animation, make
everything natural, and debug everything", and named one seen online: "a
loop gauntlet or something like that with a bunch of agents to keep
judging each other."

### What was found

**Numbers that can be computed from a movement** (research on generated
motion, where they are used to score thousands of clips with no one
watching):

| Measure | What it catches | Source |
|---|---|---|
| How far a foot slides while it should be planted; the share of frames in which it does | Skating | [Survey of motion generation, 2025](https://arxiv.org/pdf/2507.05419) |
| How deep, and how often, a part is inside the ground or another part | Passing through | the same |
| How far the lowest part is above the ground when it should be on it | Floating | the same |
| The change of acceleration from frame to frame (jerk): its peak, and the share of frames above a limit | Pops, jitter | the same |
| Where the body's centre is against where its feet are, frame by frame | A body held up by nothing | [animationsight](https://pypi.org/project/animationsight/) |
| The gravity implied by a body in the air | Floating falls | the same |

The thresholds differ from study to study and have to be set for each
body.

**Judges that are language models** (research on judging by models, and
practice with coding agents):

- The plain form: one makes, another judges against stated criteria and
  says what to change, and the maker tries again until the judge
  passes it. It works where the criteria are clear and where being told
  what is wrong makes the next try better
  ([Anthropic's cookbook: evaluator and optimizer](https://platform.claude.com/cookbook/patterns-agents-evaluator-optimizer)).
- **The "gauntlet loop"** that Luis saw is this at a larger size: a lead
  splits the work, builders make the parts, and *separate* critics
  compare what was really made against a stated bar and a reference;
  what fails goes round again. It is said to work best with a good first
  version and a clear bar, and to go astray with a vague one
  ([AI Fire](https://www.aifire.co/p/claude-gauntlet-loop-one-claude-prompt-game-changer);
  [we0.ai](https://we0.ai/articles/claude-opus-5-s-gauntlet-loop)).
- **Where it fails.** A judge that is the same model as the maker, told
  only to judge, shares its blind spots and passes what both cannot
  see; vague criteria give noisy verdicts; a judge who has already
  judged a round defends what it said
  ([one account of running such a loop](https://dev.to/nunc/i-made-claude-code-and-codex-argue-about-my-code-until-they-agreed-1pkd);
  [survey of agents as judges, 2025](https://arxiv.org/pdf/2508.02994)).
- **What helps.** Judges with *different* roles do better than several
  with the same one
  ([ChatEval, ICLR 2024](https://proceedings.iclr.cc/paper_files/paper/2024/hash/25cc3adf8c85f7c70989cb8a97a691a7-Abstract-Conference.html)).
  A judge with a fresh view each round. A checker that is not a model
  at all (a test, a measure) wherever one can be had. A second judge
  whose work is to knock down the first one's findings, so that only
  what stands is acted on
  ([Adversarial Review, ICML 2026](https://icml.cc/virtual/2026/82747)).
  A few rounds give most of what there is to gain; debate that goes on
  can make a shared bias worse.
- **Judging pictures of motion.** Models that see are used to judge
  video: given a grid of frames from a few seconds, with a list of
  faults to look for ("teleporting limbs", limbs through things), and
  asked for a reason and a score
  ([Generative Action Tell-Tales, 2025](https://arxiv.org/pdf/2512.01803)).
  A coarse look by a model followed by measures of the pose agreed with
  people 58% of the time in one study
  ([HuM-Eval, 2026](https://www.alphaxiv.org/abs/2604.25361)): useful,
  and far from the last word.
- **What games do.** Animation testing in studios is mostly by eye
  against a list of named faults (skating, pops, speed that does not
  match the feet), in a room built to show every movement
  ([a testing guide](https://mocaponline.com/blogs/mocap-news/game-animation-qa-testing-guide)).

### What makes sense for this project

Three things stand out from the above, and each answers a way this
project has gone wrong:

1. **Numbers before eyes.** The stutter in the walk and the legs that
   "teleport" in a turn are *discontinuities*: they can be measured
   exactly, at the frame rate Luis plays at, without anyone's opinion.
   Until now movements were measured for whether they *worked* (it
   picked the thing up; it did not fall), which passes every one of
   Luis's ten notes.
2. **Judges who did not make it, with a real body beside them.** What
   I made I see as I meant it. A judge that starts cold, is given the
   frames and the figures from this page, and is asked "what here would
   a person say is not how a body does that?" has no such loyalty.
3. **Luis is the last judge.** No number and no model's verdict
   stands for Luis's acceptance; they exist so that what reaches Luis
   has had its plain faults found first.

How it is set up for this project (the bench, the measures, the judges
and their questions, and what was learnt running it) is written in
[the judging of movement](../ArtDirection/MotionJudging.md).
