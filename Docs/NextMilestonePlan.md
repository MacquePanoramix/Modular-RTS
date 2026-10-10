# Next milestone — S3b: the body's own (movement pushed by the joints, stable, and alive)

**Proposed and approved:** October 9, 2026. Luis answered its four
questions the same day
([the answers](Correspondence/2026-10-09_THE_BODYS_OWN_ANSWERS.md)):
the body moved by its own joints replaces "no active ragdoll"; it comes
before S4 ("we will probably end up doing a big rework I imagine for
it"); the measures of "stable" "looked pretty good"; breathing is wanted,
in the game's own look, and how it is shown is open.

**The name "S3b" is mine:** it goes on from S3's question (a body with
real strength moving real weight, true and not goofy) to the body's own
movement.

**The plan before it** (S3: weight and strength at the rock; all twelve
steps built, waiting for Luis's play) is archived in
[Plans/S3_WeightAndStrengthAtTheRock.md](Plans/S3_WeightAndStrengthAtTheRock.md).

**The design as it is built:** [Design/TheBodysOwn.md](Design/TheBodysOwn.md).

**Where it stands:** step 1 (standing, with life in it) is **accepted
by Luis** (October 10, night:
[the message](Correspondence/2026-10-10_STEP_ONE_ACCEPTED.md)), and its
switch is on from the start. Step 3 (a step) is in hand, and is not in
the game. Step 0 is done (October 9): figures, and a bench;
nothing in the game is changed by it
([what it found](Design/TheBodysOwn.md#step-0-the-ground-for-it-october-9)).
**It changed the plan:** see "What step 0 changed", below. Nothing else
here is built except where it says "exists".

## The order of stages (October 9)

| Order | Stage | |
|---|---|---|
| Built, waiting for Luis's play | **S3, weight and strength at the rock,** and the two rounds after it | [The plan](Plans/S3_WeightAndStrengthAtTheRock.md); [the playtest round](Reviews/2026-10-08_ThePlaytestRound.md); [the round of October 9](Reviews/2026-10-09_TheBodysOwn_Round.md) |
| **Now** | **S3b, the body's own** (this plan) | Standing, stepping, walking, reaching and working by the body's own joints, stable and alive |
| Then | **S4, carrying and equipment** | Built this way from its first step ([the proposal](Plans/S4_CarryingAndEquipment_Proposal.md); five of its questions are open) |
| Then | **S2, the creator** | |
| Last | **S5, ready for players** | [ShowcaseRoadmap.md](ShowcaseRoadmap.md) |

**Why.** Luis, October 9
([the message](Correspondence/2026-10-09_THE_BODYS_OWN_NOT_POSED.md)):

> "I want stability in the body but with it still being the body's own.
> I don't want it to be posed like that."
>
> "I really do want movement to feel natural and alive rather than
> robotic. That's why it can't be posed and we must do media studies and
> research of what kind of movements feel natural for certain actions."

That answers the first question of
[the proposal for S4](Plans/S4_CarryingAndEquipment_Proposal.md) (posed, or the
body's own?), against what I had recommended. This page says how it is
built, in what order, how "stable" is held to, and what Luis sees at
each step. What was read for it is in
[the research of October 9](Research/2026-10-09_AliveAndTheBodysOwn.md).

## The two words

- **Posed:** each frame, the game works out from the ground, the body's
  build, its weights and its strengths where every part should be, and
  puts it there. Nothing pushes it. The walk, the turn, the standing,
  the reach for a pickaxe and the standing up are made so today.
- **The body's own:** the body is weighted parts joined at joints. Each
  joint pushes with no more than the strength it has. Where the body
  goes is what comes of those pushes, its weight and the ground. The
  fall, the lying, the turning over and the drawing of the knees under
  are made so today.

## What is which today

| Movement | Today |
|---|---|
| A thing hanging on the body (lantern, mug, bag, a pickaxe in the hand) | Its own |
| The pickaxe in the swing | Its own (the hands push it with what the arms can give) |
| The fall; lying; turning over; drawing the knees under | **The body's own** |
| From its knees onto its feet (half a second) | Posed |
| Standing up | Posed |
| Standing; keeping its balance when pulled | Posed (the balance is worked out by real weights, and the body is then put where it says) |
| Walking; turning; stopping | Posed |
| Going down for a thing; picking it up; laying it down | Posed |
| The body in the swing (back, knees, arms) | Posed to the tool |

## What this changes that was decided before

**Both confirmed by Luis on October 9.**

- **D1, September 30: "a physics-informed body (no active ragdoll)"** is
  replaced. A body moved by its own joints is what that line ruled out.
  `AGENTS.md` says not to introduce such a body "without a corresponding
  milestone request": this plan, with Luis's answers, is that request.
- **The order of stages** (approved October 6: S4, then S2, then S5).
  This comes before S4: S4 cannot be built the body's own on a body that
  walks and stands posed.

## How it is made stable

"Very stable, never naturally wobbly", yet able to fall "ragdoll-ish" in
"an extreme situation" (Luis, October 6). Both stand.

1. **Nothing pushes the body from nowhere.** Many games keep such a body
   up with an unseen hand at its hips. That is a posed body wearing a
   simulated one. Here the only forces on the body are its weight, the
   ground, what it holds, and what its own joints give; a test holds
   that to zero.
2. **It keeps its feet as a body does, by degrees:** at the ankles for a
   small disturbance; at the hips and back for a larger; by a step when
   those are not enough; and it falls when a step is not enough. The
   balance that exists already works out all of this from the real
   weights. What changes is that the joints are then pushed towards it,
   not the parts placed.
3. **Where the foot comes down keeps a walk up** (the oldest and best
   tried way: see the research, part 3). It is corrected at every
   moment by how far and how fast the body's weight is from the
   standing foot.
4. **Each joint is held towards a pose with the strength that joint
   has,** as in the fall and the getting up; **the poses and their
   numbers are found by a search on Small, Long and Round together,**
   not set by hand (the tool exists: `GetUpSearch`; five hand-set tries
   at the getting up got no body up, and the search did).
5. **"Stable" is said in numbers, measured first on today's posed body,
   and the body's own must do no worse** (Luis, October 9: "they looked
   pretty good for me"):

   | Measure | What is asked |
   |---|---|
   | Standing at ease, a minute | It breathes and shifts its weight, and between shifts its head wanders no more than a centimetre or two (so read since October 10: a weight that goes over a leg takes the head 3 to 4 cm with it, and that is meant). No tremble (nothing moving back and forth faster than about twice a second) |
   | Pulled a little | It leans against the pull and does not step: at the same pulls as today (Round: 100 N forwards, its hips 6 to 8 cm back) |
   | Pulled hard | It steps and stands: at the same pulls as today (Round: one step at 200 N, two at 350 N; Small: three steps at 110 N) |
   | An extreme pull, or legs that give | It falls: at the same pulls as today (about three fifths of its weight) |
   | Walking the place, its tool in hand, at half, ordinary and double strength, ten minutes each miner | It does not fall |
   | At its work at each of the ten boulders | It does not fall, except as today (legs that give under work far too heavy) |

6. **The posed body stays beside it, to switch to, until Luis accepts
   each movement.** The panel gets a choice, movement by movement. Luis
   liked the walk as it is (October 1); nothing Luis has is taken away
   by a step of this plan.

## How it is made alive

The eight things a living body does
([the research, part 2](Research/2026-10-09_AliveAndTheBodysOwn.md#for-ours-eight-things-a-living-body-does-and-where-the-miners-are)).
Each step below says which of them it is to bring. Before a movement is
built, what real bodies do in it and how films and games make it read
are looked up and written down, as Luis asked (a first pass, action by
action, is [part 4 of the research](Research/2026-10-09_AliveAndTheBodysOwn.md#4-action-by-action)). Then, as now: the three
miners, traced; read in pictures; three judges who did not make it;
Luis.

## Steps

Each is shown to Luis before the next is built on it.

**The order was changed by Luis on October 10:** step 3 (a step) is built
before step 2 (from its knees onto its feet), because leaning as today's
body does, a real shift of weight, and standing up to a push all wait on
the step ([Luis, October 10](Correspondence/2026-10-10_STEP_ONE_THREE_ANSWERS.md)). The numbers are
kept as they were, so that what is written elsewhere still points right.

| | Step | It is to bring | What Luis sees |
|---|---|---|---|
| 0 (done) | **The ground for it.** Ankles: the boot becomes a part of its own (today the shin and the boot are one, and a body cannot balance at an ankle it does not have). The test that nothing pushes from nowhere. Today's posed body measured for the table above. What a body's own costs in the frame, one miner and a crowd | | Figures |
| 1 (built October 10; **accepted by Luis** that night) | **Standing, with life in it.** It stands by its own ankles, hips and back. It breathes (faster when it has worked); its weight goes from one leg to the other now and then; its head and eyes move. Pulled a little, it leans | Never quite still (1); balance by degrees (7); shows its effort (8) | A miner standing that is alive and does not wobble. A switch on the panel: this, or as it was |
| 2 | **From its knees onto its feet.** The half second that is posed in the getting up. Then the whole of it, from the fall to standing, is the body's own | Not all on one clock (2) | A pushed miner getting up with nothing taking over |
| 3 | **A step.** Pulled hard, it catches itself with a step of its own. Before a first step it shifts its weight and leans; the first step is shorter | Gets ready (3); balance by degrees (7) | A miner pulled, stepping; a miner setting off |
| 4 | **Walking, turning, stopping.** On the path, the meadow and its slopes. No two steps quite the same. The free arm swings | Arcs and one hump of speed (5); does not repeat itself (6) | The walk, beside the walk as it was |
| 5 | **Down to the ground and up.** Picking a thing up, laying it down | Not all on one clock (2); gets ready (3) | The pick-up, beside the pick-up as it was |
| 6 | **The work.** The body in the swing is its own, as the tool already is | Shows its effort (8) | A miner mining |
| 7 | **Then S4,** each of its movements built this way from the start | | The proposal for S4, with its first question answered |
| At any point | **The fall, again.** It is the body's own already, and it is the furthest of all from a real body: a young person who falls gets a hand to the ground nine times in ten and keeps the head off it eight in nine ([the research, part 4](Research/2026-10-09_AliveAndTheBodysOwn.md#4-action-by-action)); a miner gets no hand down and strikes its head almost every time. Arms that reach for the ground, and a head kept off it | Gets ready (3); shows its effort (8) | A pushed miner catching itself on its hands |

### What step 0 changed (October 9)

- **Standing is a keeper to be designed, not springs to be set.** The
  engine's joint springs, set stiffly towards a standing pose, do not
  hold a heavy body over a light boot. What stood still (heads within
  0.1 to 0.35 mm, twenty-five of twenty-five for twenty seconds) was a
  body whose legs give torques worked out each step from the push it
  wants from the ground, its joints held softly towards their pose.
- **How often the physics steps.** In step 0 all three stood at 200
  steps a second, Long and Round at 100, none reliably at the game's 50.
  **Step 1's first part (the same day): with each joint's spring set
  by rule, all three stand at the game's own 50, but less steadily than
  at 100 or 200** (set going at 0.2 m/s, 3, 20 and 21 of 24 stay
  standing at 50; 19, 24 and 19 at 100; all at 200)
  ([figures](Design/TheBodysOwn.md#step-1-first-part-how-often-the-physics-steps-october-9)).
  So the body can be begun at the game's own step, and nothing already
  built need be touched to begin; the rate is a trade to be chosen when
  the body is in the game, and the keeper comes first.
- **It does not lean yet.** Round took a pull of 30 N; Small and Long
  fell at it. Today's body leans at 50 to 150 N.
- **Step 1 is larger** than "it stands by its own ankles, hips and
  back" sounded. Its order: (a) how often the physics steps, counted
  over many starts on all three miners (looked into for standing: it
  stands at fifty, sturdier at a hundred and two hundred; chosen later,
  in the game); (b) the body's own body in the
  game, as an articulation of thirteen parts, beside the posed one,
  switched on the panel; (c) the keeper: standing still, leaning against a pull as
  today's body does, a joint giving no more than it has, the ground
  known by what touches it; (d) life in it: weight shifting, breath, the
  head and eyes.
- **A crowd of bodies of their own cannot be afforded** (one costs a
  tenth to a fifth of a millisecond of each fiftieth of a second; a
  hundred, more than the frame). A far-off crowd keeps the posed body.
  That was open in this plan and stays open for the full game.

Lying is part of step 1: a body that lies should breathe and stir. A
stir before it gets up was tried on October 9 and taken out again (see
[the round of October 9](Reviews/2026-10-09_TheBodysOwn_Round.md)). Since
October 10 a lying body's chest is drawn breathing; it does not stir.

### What step 1 found (October 10)

[The design as built](Design/TheBodysOwn.md#step-1-the-rest-of-it-the-body-in-the-game-and-the-life-in-it-october-9-and-10);
[what its judges said](Reviews/2026-10-10_StandingWithLife_Judges.md).

- **Built:** the body's own body in the game beside the posed one,
  switched on the panel; a keeper whose settings a search found; life
  in it (breath, its weight from leg to leg, a slow drift, a head that
  looks about). It stands as still as was asked, and gives its body
  back, or to the fall, when anything else is asked of it.
- **Not reached: "leaning against a pull as today's body does".** On
  flat feet, without a step, it holds 20 to 30 N (Round to 65 N);
  today's posed body leans against 50 to 150 N. The posed body is not
  bound by what feet can do. **It waits on step 3.**
- **Alive, by its judges: only in its head.** Its breath and its shift
  of weight are there in the figures and cannot be seen in the
  pictures. A fuller shift took a nudged miner over: it too waits on
  step 3. How breath is shown is put to Luis (B6).
- **The measure "its head wanders no more than a centimetre or two"**
  (the table above) is met between shifts of weight (millimetres) and
  not across them (its chest 2 to 3 cm, its head 3 to 4 cm, over a
  minute): a weight that goes over a leg takes the body with it. Put to
  Luis (B5).
- **The order of steps 2 and 3** is put to Luis (B7).

**The two more looks of breath (B6) are built** ([what they are](Design/TheBodysOwn.md#two-more-looks-of-breath-october-10)),
and are on the panel beside the three. Which look: Luis's, on the build.

### Step 3, as it is to be built (written October 10, before it is begun)

Read for it: [the research, part 7](Research/2026-10-09_AliveAndTheBodysOwn.md#7-a-step-catching-itself-read-for-step-3).

1. **One keeper, for the game and for the search.** Today the bench has
   one keeper and the game a copy of it that has since been mended
   three times. The keeper is taken out into one piece of code that
   both use, changing nothing of how it stands; the seven tests of the
   body's own are the proof.
2. **The step, in the bench:** when where its weight is going cannot be
   kept within its soles, a leg is chosen, unloaded, lifted, swung to
   where its weight is going and a little beyond, and stood on; then
   the other boot is brought alongside. Its settings found by a search
   on Small, Long and Round together, which also looks at the boots.
3. **Held to today's posed body** (the table above): it leans without
   stepping to about a sixth of its weight, steps at about a quarter,
   falls from a third to three quarters. Pulls, nudges, and many in a
   row.
4. **In the game,** on the panel beside the posed body, with a way to
   pull it; the miner's place going with its feet.
5. **Then what waited on it:** a fuller shift of weight; its chest
   following a look; and judges.

**Where it stands (October 10).** On the work branch `claude/wip-round`
only; nothing of step 3 is in the game Luis has.

- **1 is done.** The keeper is one piece of code (`OwnKeeper`), used by
  the body in the game; the seven tests give the figures they gave.
  (Written with three numbers added in another order, Round on one leg
  went down on a fourth nudge: its hold there is that thin.)
- **2 is begun.** The keeper can step (off unless asked), with settings
  set by hand, not searched; and the bench has a trial for it
  (`BodysOwnBench.Steps`: the game's keeper on bodies of the bench,
  pulled and set going eight ways each).

  | | Small | Long | Round |
  |---|---|---|---|
  | Set going at 0.3 m/s: keep their feet, of 8 (without the step) | 8 (5) | 8 (7) | 8 (7) |
  | ...at 0.5 m/s | 3 (2) | 1 (2) | 6 (3) |
  | ...at 0.8 m/s | 0 | 0 | 0 |
  | Pulled with 0.08 of its weight for 2.5 s | 5 (4) | 3 (2) | 5 (5) |
  | ...with a sixth of its weight, and more | 0 | 0 | 0 |

  Where it keeps its feet it takes none to two steps (the catch, and
  the other boot brought alongside), and ends with its boots flat and
  standing as they stood.
- **Later the same day: three searches of the step's settings**
  (`BodysOwnBench.StepSearch`: twenty-odd settings, the three miners
  together, pulled and set going four ways each; about 560 tries a
  search), and four changes to how it steps on the way (its swinging
  boot carried by what its hip and knee give for a pull on the boot;
  the hip it stands on taking up what the swinging hip gives; a stiffer
  ankle in the air; the blow of landing not taken for a push). With
  what the third search found:

  | | Small | Long | Round |
  |---|---|---|---|
  | Set going at 0.3 m/s: keep their feet, of 8 (without the step) | 8 (5) | 8 (7) | 8 (7) |
  | ...at 0.5 m/s | 4 (2) | 8 (2) | 7 (3) |
  | ...at 0.7 m/s | 1 | 3 | 2 |
  | Pulled with 0.08 of its weight for 2.5 s | 6 (4) | 5 (2) | 5 (5) |
  | ...with an eighth of its weight, and more | 0 | 0 | 0 or 1 |

  In none to four steps (one of them took nine), the other boot
  brought back alongside. **So a shove is caught far better than
  before; a pull that is held is not caught at all.**
- **A pull that is held is not understood yet.** Told step by step, a
  Small pulled forwards with an eighth of its weight does not step for
  four tenths of a second; its boot lands barely ahead of its hips and
  tipped; its hips pitch further forward with each step, and it goes
  down. Two things were tried for it and changed nothing: counting
  what it feels in when and where it steps (the search turned that
  down to nought), and answering its body's tipping with where the
  ground pushes it (at a first hand-set size). What I think is so, and
  have not shown: on a boot as short as these, a pull of an eighth of
  its weight at the chest cannot be stood through without a step; and
  the step it takes is too late and too short for a force that goes on
  pushing while it steps. **Next:** trace one pulled body through its
  first step with nothing else changed; a search asked for weaker
  pulls first; and only then the game.
- **One more thing tried by hand, later on October 10, and no
  better:** stepping sooner (as soon as what it feels cannot be met
  within its soles) with a slower, gentler swing of the leg. A pull
  of an eighth of its weight was still caught by one body in eight at
  the most, and a shove less often than with the searched settings.
  So the cause is not yet found by reasoning about it; it is to be
  found in the trace.

**All three were answered the same day:** Luis approved what was
suggested for each ([Luis, October 10](Correspondence/2026-10-10_STEP_ONE_THREE_ANSWERS.md)).

## What it will cost, said plainly

- **Time.** By my estimate, weeks for steps 0 to 3 and more for the walk.
  (After step 0: step 1 alone is weeks. Standing still is had in a
  bench; standing against a pull, in the game, on all three miners, with
  everything built before still working, is not.)
  The walk is the hard one: walks made this way are known to look stiff
  until their numbers are searched for it.
- **It can fail in ways nobody planned.** A body pushed by forces will,
  now and then, do what was not meant: miss a step, sit down. That is
  what makes it real, and it is why the measures above are run for
  minutes on all three miners at several strengths before Luis sees a
  step. (Long fails first.)
- **The frame.** Eleven to thirteen weighted parts a miner. For one
  worker it should not show; for a crowd it will. What a far-off crowd
  does is a question for the full game, kept open.
- **Step 2 was tried and failed once.** Five searches for a way from the
  knees onto the feet found none that all three bodies do. They were
  made without ankles. With ankles it may be found; it may not.

## Questions for Luis

All four were answered on October 9
([the message](Correspondence/2026-10-09_THE_BODYS_OWN_ANSWERS.md)).

| | Question | Luis |
|---|---|---|
| B1 | Is D1 ("no active ragdoll") replaced? | "Yes." |
| B2 | Does this come before S4? | "Yeah I also think so, we will probably end up doing a big rework I imagine for it." |
| B3 | Is the table of "stable" the right measure? | "Yeah they looked pretty good for me." (Luis's eye on the build is still the measure that counts) |
| B4 | May breathing be drawn on the body (the chest rising), with its rate taken from how hard the body has worked? | "If you think it will look nice/apparent, then sure. If not then we can think of other ways to represent it. But yeah breathing is good to have while following the visual aesthetic of the game for it too." |

**So for breathing** (step 1): it is wanted. It is tried drawn on the
chest first; if it cannot be seen at the distances the game is played
at, or does not sit in the game's look, other ways are tried (the
shoulders and the back moved by their own joints; the coat; breath seen
in cold air at dusk). Each is shown to Luis as a look beside the others,
and none is taken as settled.

**Three more, from step 1 (October 10). Answered by Luis the same day:**
"I approve of all your suggestions for the questions you asked me."
([Luis, October 10](Correspondence/2026-10-10_STEP_ONE_THREE_ANSWERS.md)). So each is as its
right-hand column says.

| | Question | What was suggested, and Luis approved |
|---|---|---|
| B5 | The table of "stable" says a standing miner's "head wanders no more than a centimetre or two". Is that to hold while its weight goes from leg to leg, or only between? | Only between. A weight that really goes over a leg takes the head 2 to 4 cm with it, slowly, and that is not a wobble. As it is now: millimetres between shifts, 3 to 4 cm across them |
| B6 | Breath drawn on the chest cannot be seen, even at four times life (six judges of six). How is it to be shown? | The panel has three looks to begin from ("drawn stronger", "as in life", "not drawn"). Others, not built: the shoulders and collar rising, drawn boldly; a tired body heaving; breath seen in the cold air at dusk. I would try the shoulders and the dusk air next, beside these |
| B7 | Shall "a step" (step 3) come before "from its knees onto its feet" (step 2)? | Yes. Leaning as today's body does, a real shift of weight, and standing up to a push all wait on the step; step 2 waits on nothing |

**After Luis looked at step 1 (October 10). Answered the same evening**
([the message](Correspondence/2026-10-10_THE_SWITCH_TICKED.md)): B8, "I think it wasn't, sorry"; B9, "to some
degree... the breath should be gentle... scale appropriately with
situation"; B10, "all three options looked good" and the size of that
morning "a bit too strong"; B11, "Sure can be thanks!"

**Asked after that, and answered by Luis that night**
([the message](Correspondence/2026-10-10_STEP_ONE_ACCEPTED.md)):

| | Question | What I thought | Luis |
|---|---|---|---|
| B12 | Is step 1 (standing by its own joints, with life in it) accepted? And may the switch be on from the start, so that a miner at ease stands so without being asked? | Yes to both: Luis likes it, and the switch being off is what hid it. The body as it was stays one click away | **"Indeed yes to both."** Done: the switch is on from the start |
| B13 | Are the looks of breath to be drawn together (chest, shoulders, and the air from dusk to dawn), as they now are? | Yes | **"Yes that's perfect I think."** |
| B14 | A miner's blows at a boulder sometimes land thirty centimetres off (one try in thirty; five in fifteen while breath was drawn on its shoulders). Is that to be found now, before the step goes on, or after? | Now: it is a miss Luis could see in the game, and it is what keeps breath off a miner at work | **"Yes I think so, good suggestion."** Found: [three faults, two mended](Design/TheBodysOwn.md#the-miss-at-a-low-boulder-october-10) |

**One thing for Luis to say, when it matters to Luis** (nothing waits
on it):

| | Question | What I think |
|---|---|---|
| B15 | A miner resting from its blows does not heave: breath is drawn on the chest and shoulders only of a miner standing by its own joints with empty hands, or down ([why](Design/TheBodysOwn.md#the-miss-at-a-low-boulder-october-10)). Is that to wait for walking and the work to become the body's own (steps 4 and 6), or to be done on the body as it was first (about a day)? | Wait. It comes of itself with steps 4 and 6, and a day on the body as it was is a day not spent on them |

**The questions as they were put that morning:**

| | Question | What I think, for Luis to overrule |
|---|---|---|
| B8 | Was "Stands by its own joints" ticked when the standing miner was looked at? (Without it a standing miner is the body as it was, which does not breathe at all; a fallen one breathes either way.) | It decides what "I couldn't really notice the difference" was said of |
| B9 | Is every miner to breathe, always: walking, working, and standing as it did before? | Yes, drawn (its chest and shoulders: that costs nothing and pushes nothing), with its rate and depth from how hard it has worked. Today only a miner standing by its own joints breathes, and a fallen one |
| B10 | What size, and which look? | Set by eye on the panel (the slider, and the Breath button), and told to me. It begins on the strong side |
| B11 | Of the outside review: are the three stale passages it names to be put right (the first lines of the validation page; an old "not yet built" list in the current-state page; the test counts in the README)? I looked: all three are stale | Yes. Nothing else of the review is taken up without Luis saying which |

The five other questions of the proposal for S4 (Q2 to Q6) are still
open.

## What it does not do

- No recorded movement, no downloads, no learning from recordings.
- No running, no fighting: later, on the same body.
- Nothing is taken out of the game until Luis has accepted what
  replaces it, and nothing is merged to `main` without Luis's word.
