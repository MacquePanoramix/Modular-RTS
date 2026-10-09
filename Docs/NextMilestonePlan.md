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

**Where it stands:** step 0 is begun. Nothing else here is built except
where it says "exists".

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
   | Standing at ease, a minute | It breathes and shifts its weight, and its head wanders no more than a centimetre or two. No tremble (nothing moving back and forth faster than about twice a second) |
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

| | Step | It is to bring | What Luis sees |
|---|---|---|---|
| 0 | **The ground for it.** Ankles: the boot becomes a part of its own (today the shin and the boot are one, and a body cannot balance at an ankle it does not have). The test that nothing pushes from nowhere. Today's posed body measured for the table above. What a body's own costs in the frame, one miner and a crowd | | Figures |
| 1 | **Standing, with life in it.** It stands by its own ankles, hips and back. It breathes (faster when it has worked); its weight goes from one leg to the other now and then; its head and eyes move. Pulled a little, it leans | Never quite still (1); balance by degrees (7); shows its effort (8) | A miner standing that is alive and does not wobble. A switch on the panel: this, or as it was |
| 2 | **From its knees onto its feet.** The half second that is posed in the getting up. Then the whole of it, from the fall to standing, is the body's own | Not all on one clock (2) | A pushed miner getting up with nothing taking over |
| 3 | **A step.** Pulled hard, it catches itself with a step of its own. Before a first step it shifts its weight and leans; the first step is shorter | Gets ready (3); balance by degrees (7) | A miner pulled, stepping; a miner setting off |
| 4 | **Walking, turning, stopping.** On the path, the meadow and its slopes. No two steps quite the same. The free arm swings | Arcs and one hump of speed (5); does not repeat itself (6) | The walk, beside the walk as it was |
| 5 | **Down to the ground and up.** Picking a thing up, laying it down | Not all on one clock (2); gets ready (3) | The pick-up, beside the pick-up as it was |
| 6 | **The work.** The body in the swing is its own, as the tool already is | Shows its effort (8) | A miner mining |
| 7 | **Then S4,** each of its movements built this way from the start | | The proposal for S4, with its first question answered |
| At any point | **The fall, again.** It is the body's own already, and it is the furthest of all from a real body: a young person who falls gets a hand to the ground nine times in ten and keeps the head off it eight in nine ([the research, part 4](Research/2026-10-09_AliveAndTheBodysOwn.md#4-action-by-action)); a miner gets no hand down and strikes its head almost every time. Arms that reach for the ground, and a head kept off it | Gets ready (3); shows its effort (8) | A pushed miner catching itself on its hands |

Lying is part of step 1: a body that lies should breathe and stir. A
stir before it gets up was tried on October 9 and taken out again (see
[the round of October 9](Reviews/2026-10-09_TheBodysOwn_Round.md)).

## What it will cost, said plainly

- **Time.** By my estimate, weeks for steps 0 to 3 and more for the walk.
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

The five other questions of the proposal for S4 (Q2 to Q6) are still
open.

## What it does not do

- No recorded movement, no downloads, no learning from recordings.
- No running, no fighting: later, on the same body.
- Nothing is taken out of the game until Luis has accepted what
  replaces it, and nothing is merged to `main` without Luis's word.
