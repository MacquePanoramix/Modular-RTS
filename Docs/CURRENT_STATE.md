# Wonder Gather — Current State

**Updated:** October 27, 2026.

- **Now (October 27, afternoon): option 1, and the learner paces its way** (Luis, October 27: "Yes I think that
  approach sounds good", to letting what learns decide when each pose goes on). On the work branch only, behind the
  switch (off), not in a build. The teaching's fresh falls had been 72 made once and learnt by heart: on falls made anew
  the 2,000,000 model stands 20% (not 31%), and that is what the game sees (the first try of a push stood 4 of 20). Now
  taught on falls made anew, as two learners: the joints at the hard moment (from `full3`) and the pace of every pose
  (new), `pace1`. In the game, as played, with `full3`: up by its own body after 13 of 20 pushes (the body alone 7 of 20).
  Judged once (round 1 of 3): reads as real, with a flaw (the ways' fixed times; the standing balance's slow start).
  [Validation](Validation.md). **Next:** teach `pace1` on, measure it on falls made anew and in the game, pictures,
  round 2 of the judges (with the smoothstep rise beside the old), then Luis. How it is run:
  `Docs/Technical/RiseWays/README.md` on the work branch.

- **Where it was left for the night (October 22, late; Luis turned the laptop off).**
  The getting up (work branch `claude/wip-round`, behind the switch, off): the hands
  let go only when the weight is over the boots, and the standing balance raises it
  legs first (79% of those handed over stand); about a third of tries reach the
  hand-over. **The third path, a learned correction (Luis approved it and the
  downloads), is begun, not trained:** ML-Agents 4.0.3 on the work branch, the
  trainer in `D:/Dev/mlagents-env` (outside the project), the agent and its
  teaching written (`Assets/_WonderGather/Learning`), a training build made
  (`Builds/RiseTrain` on the work branch's copy). Proved: it connects and learns in
  the editor, but alone it is too slow (each decision waits ~36 ms on the trainer).
  **Next:** see the build train with many copies at once
  (`Docs/Technical/RiseWays/README.md` on the work branch), then a night's training,
  then the bench and the game with what it learnt.
- **Luis, October 22: never the backup animation; extra tries rather than help
  from outside the body** ([the message](Correspondence/2026-10-22_NEVER_THE_BACKUP_ANIMATION.md)).
  On the work branch, behind the panel's switch "Gets up by its own body"
  (**off**; not in a build): Small now rolls from its back or side onto its
  front (on the bench 29 of 36 from its back; the judges, round 1: "reads as
  real, with a flaw"), its boots no longer whip round in most tries, it gives a
  try up as soon as it has gone down, and it tries again without the cap of
  four. **But it does not get up yet:** rolling up from hands and feet onto its
  feet fails nearly every time (1 to 8 of 36 stand from its front, 0 to 2 from
  its back), and in the game 11 tries in 300 s brought it up none. Next: bring
  that hand-over to the crouch the standing body already rises from.
  [The design page](Design/TheBodysOwn.md#never-the-backup-animation-luis-october-22-work-branch-in-progress).

- **The side sessions' work is merged into the game branch (October 22; Luis:
  "can you merge the work that these other agents had done here?").** The
  satchel stopped by the body, the satchel lying on the body, and the lantern,
  the mug, the hammer and the ground: three branches, the two last changing the
  same code in `MinerBody`, merged by hand by what each part means
  ([how, and what was measured](Validation.md#the-side-sessions-work-merged--october-22)).
  On the merge: the whole suite **207 tests, 178 passed, 0 failed** (29 run
  only when asked); the satchel through its clothes in falls in 2, 5, 3 and 16
  looks (front, back, left, right; each branch alone 2, 8, 3, 44); nothing on a
  hook more than 4 mm under the ground (the satchel too, now); standing and
  walking as before. Not in a build; not looked at by Luis. The getting up
  (work branch) is not part of it. The three entries below are each branch's own
  account, as written then.
- **The satchel through the coat (the judges of the getting up, finding 4),
  partly mended on its own branch `claude/satchel-stop`, not merged, not in a
  build** ([what was found and how it was tested](Validation.md#hung-things-stopped-however-the-body-is-turned--october-15);
  [before and after](Images/SatchelStop_2026-10-15/);
  [its judges](Reviews/2026-10-15_SatchelStop_Judges.md)). Whatever way the
  body is turned, what hangs on a hook is now stopped by the hips, by the
  cloth it rests on and by the thigh on its side; standing and walking it
  hangs and swings as before (2.6 mm at most). Three judges: "reads as real,
  with a flaw". **Still open:** the coat still cuts the bag's top as it
  pushes onto hands and knees (the bag is held out by its weight, but turned
  rigidly about its rings); the bag is lost behind the thigh as a foot comes
  forward; a fall onto the bag; Small's lantern, Long's mug and Round's
  hammer, which read inside the coat when bowed, as before. For Luis to look
  at.
- **The lantern, the mug, the hammer and the ground (October 15 to 21), on
  their own branch `claude/hung-things-stop` (from `claude/satchel-stop`), not
  merged, not in a build**
  ([what was found, built and tested](Validation.md#the-other-hung-things-bowed-lying-and-getting-up--october-15-night);
  [pictures](Images/HungThings_2026-10-15/)). Found by pictures from four
  sides: all four hung things went into the ground (the lantern to 176 mm),
  the lantern and the mug hid inside the coat when lain on, Round's hammer went
  through its thigh on hands and knees; the lantern bowed was a misreading of
  the measure, not a fault. Mended: what hangs lies on the ground, held there
  (at most 12 mm under it in the test), out along it when lain on, and the
  thigh is kept off a thing's whole line; standing and walking unchanged
  (0.00 mm); the whole suite passes but for one test on its own edge, which
  fails on the base commit too. **Still open:** Round lain on its right hides
  its hammer in its torso; in a deep squat its handle sinks into the thigh for
  about 0.1 s; Long's mug is cut by the coat as it falls, the lantern is held
  out at rest, and a one-frame flick of what hangs, all as on October 15.
  Judges (three rounds): "reads as real, with a flaw"
  ([their page](Reviews/2026-10-21_HungThings_Judges.md)). Touches
  the same code as the satchel's second branch (`claude/satchel-lies`): to be
  merged with care. For Luis to look at.

- **The satchel lies on the body (October 20), on its own branch
  `claude/satchel-lies` (on `claude/satchel-stop`), not merged, not pushed, not
  in a build** ([what was found and how it was tested](Validation.md#the-satchel-lies-on-the-body--october-20);
  [first stop and now, three views](Images/SatchelLies_2026-10-20/);
  [its judges, round 2](Reviews/2026-10-20_SatchelLies_Judges.md)). Round 1's
  two flaws are gone: on hands and knees the bag no longer has the coat across
  its top, and as a foot comes forward it is no longer lost behind the thigh.
  Its rings ride out on what comes up under them (the coat's skirt bulged 15 to
  40 mm over them; the drawn-up thigh came within a centimetre), it turns to lay
  its back on what holds it, and the thigh, shin and boot keep it off. In the
  judges' try a tenth of it is under the clothes in 5 frames of 158 (the first
  stop: 101); at work 53 of 529 (288). Standing and walking as before, frame by
  frame; the lantern, mug and hammer untouched. The whole suite: 206, 177
  passed, 0 failed. Three new judges (on the version before the lying flip was
  mended): "reads as real, with a flaw". **Still
  open:** as the hips leave the ground (2.2 to 2.4 s) the bag sits under the
  skirt's hem and comes out in one step (the first stop too); the boot's top
  grazes it for about 0.4 s as the foot swings forward; the strap's end may look
  cut by the coat (not checked); lying, it goes into the ground, as before. The
  "hung things" session changes the same code (`MinerBody`): merging both is by
  hand. For Luis to look at.
- **A build for Luis to play, October 14 (late): `Builds/WindowsOrdinaryPlace_Oct14`**
  (beside the three of October 13). In it: arms as strong as the body needs
  (Luis's decision), tiredness taking precision, breaths between blows. The
  getting up in it is the old one; the new one stays on the bench. Nothing to
  switch on. [What to try](Playtests/MinersPlaytest.md#the-work-with-arms-as-strong-as-the-body-needs-october-14-for-luis-to-play).
  The whole suite before it: 201 tests, 175 passed, 0 failed, 26 run only
  when asked; as played, the 12 failures known since October 10 and the
  house's night-light test, which comes and goes. The built game was not
  opened by me.
- **The outlook habit (Luis, October 14):** now and then a general look at
  the whole project, then back to the task: [Outlook.md](Outlook.md).
- **Luis, October 14: "You can make what feels right"** (the pace of
  work), with deep research on mining in media first, and **tiredness
  to take strength and precision**: [the research](Research/2026-10-14_MiningAndTiredness.md)
  (what the work is held to, W1 to W8) and
  [the message](Correspondence/2026-10-14_MINING_TIREDNESS_AND_BALANCE.md).
  Built on the work branch: **tired, a miner's blows spread more** (Round:
  10 mm fresh, 26 mm tired) and come down weaker (it did already);
  **breaths between blows**, more often as it tires. Not in a build, not
  looked at in pictures.
- **Balance for the getting up (Luis: "I like your idea"; October 14,
  night; work branch only, in progress, saved when Luis turned the laptop
  off).** What bears the miner (hands, knees, boots) stays where it is and
  its joints carry its body over it; its weight goes over the others
  before a limb is lifted. Small comes onto hands and knees and holds
  there; on October 14 (afternoon) one foot brought forward from there
  held at 25 of 36 starts laid by chance (with the game's frames), **but
  the way searched after the judges' mends holds at 8 to 19 of 36**.
  **October 15, after midnight:** its weight now really goes off the knee
  before it lifts (19 of 36, from 6; a fault of mine mended); the kick of
  the leg behind it, which the judges saw, was found in how a leg is
  reached for and mended; its shins now lie on the ground on hands and
  knees (they were folded up, its boots in the air). Searched again so,
  after the judges (round 2: "reads as real, with a flaw", three of three):
  **one foot forward and still up at the end, 31 and 34 of 36** chance
  starts with the game's frames, its weight off the knee first in 35.
  **Onto its feet (October 15, evening): Small stands up by its own body
  at 7 and 4 of 36 chance starts**, the toddler way (both feet forward,
  back into a squat, up); about 13 s, too slow; most of the rest fall as
  they let go of their hands in the squat. **October 20: the bear way (hips
  up, rolling up over the feet) stands Small up at 17 and 17 of 36**; the
  judges (round 3, the last): "reads as real, with a flaw" (holds where
  nothing changes; 13 s, too slow). Bodies that sometimes fly apart: a boot
  that whipped half round in every step (130 radians a second) was found and
  mended; with the boot turning as a foot does, 6 and 10 of 36 stand (the
  whip had helped it); searched again: 12 and 8 of 36. In the game behind a
  switch (off): "Gets up by its own body" (tries again if it goes down); but
  pushed over in the game Small lands on its back or side, and its only way
  is from its front, so it rarely starts. Next: a way from the back and
  sides (rolling onto its front).
  See [The body's own](Design/TheBodysOwn.md), "Its weight off a knee,
  and a leg that kicked". Not in a build.
- **The getting up is being built (October 14; on the work branch
  and the bench only; not in the game, not in a build; Luis has seen
  none of it).** For the first time a miner goes from lying to
  standing with nothing posed anywhere: laid on its front, it draws
  its knees under, comes onto its feet in a tuck, and its own legs
  raise it. Each of the three does so **at the one place on the path
  where its way was searched** (Small 7 tries of 8; Round and Long 2
  of 2; 8 to 12 s, too long) **and at none of six other places (0
  of 18):** the ground lies differently at each, and a row of poses
  does not answer to it. It is done on the body that stands (with
  ankles), made as the miner lies, so one body goes from lying to
  standing; the poses are searched, each miner its own.
  With two pieces that answer to the body (a foot put on the ground
  by its leg's joints; sitting back until its boots bear it), Small
  now comes to the same place at five places of six: its feet flat
  under it, its weight still on its head and arms. Getting its weight
  from there onto its feet is what is left of that.
  **Luis decided (October 14): a miner's arms are to be as strong as
  its own body needs, as its legs are,** "and adapt the rest to it
  as necessary" ([the message](Correspondence/2026-10-14_ARMS_AS_ITS_BODY_NEEDS.md)).
  **Before that is changed, Luis asked for deep reference research**
  on how people get up and how it is animated, in games and every
  other medium: [done](Research/2026-10-14_HowPeopleGetUp.md). It
  ends in twelve things our getting up is held to (G1 to G12).
  **Done on the work branch since:** the arms (Small's shoulder 24 ->
  42 N m, Long's 31 -> 76, Round's 61 -> 85); the work measured again
  (they swing harder and truer and rest about half as often: Round's
  first rest at swing 48, not 19; a question to Luis whether to keep
  that); a cap found that held arms and ankles to a fifth of what they
  have while getting up (loosened: Small now pushes itself up onto its
  hands and knees, its head up); toes tried on the body that gets up
  (Luis: whatever is kept goes on the standing body too, and it is
  tuned again). **Still missing: balance on hands, knees and feet**
  (built next). Not in the game.
  **Not done:** a way that holds wherever it lies; from its back and
  sides; the lying itself (what Luis asked for first: not dead until
  a clock runs out); a shorter middle; the game
  ([what is built](Design/TheBodysOwn.md#step-2-the-getting-up-the-bodys-own-begun-october-14-not-in-the-game)).
  The three builds are as they were on October 13.

- **Luis tried the step (October 13, night): "It is generally looking
  well."** "I did like so far how the stepping mechanism after being
  pushed around is like currently generally." Liked; not said
  accepted ([the message](Correspondence/2026-10-13_THE_STEP_TRIED.md)).
  - **"The getting up is super weird"**: it lies as dead until a
    timer, convulses, and rises by "a magical not physical animation
    at all". It is to get up "naturally with their own limbs and
    strength", stirring less while it recovers, then slowly. **That
    is next** (the plan's step 2, widened to the whole of it). The
    step did not change the getting up; it is S3's.
  - **The shove pressed many times gave "very whacky buggy
    physics": found that night; mended in part.** A
    miner driven faster than it could catch went on stepping, and a
    boot went at up to 41 m/s. Now a catch it cannot make is not
    tried, nor a fourth in a row: pressed fast it goes down after one
    step, cleanly; **in 3 or 4 of 9 ways of pressing a boot still goes
    at 9 to 20 m/s** (it was 10 to 41 in 8 of 9). **It is in a third
    build, `Builds/WindowsOrdinaryPlace_Oct13c`, beside the two,** which
    are as they were. Shoved once, or pulled, it steps as they do
    (the three stood every shove of the button's strength from four
    sides, one run); it differs when it is shoved again and again.
    The built game was not opened by me.
  - **The last whole run** (that state): 199 tests, 174 passed, 1
    failed, 24 run only when asked. The one is a test of the house's
    night light (its windows at 0.887 for 0.9), which nothing of this
    touches; it passes alone, twice. Not explained. As the game is
    played: 163 passed, 12 failed, the twelve known since October 10.
- **Luis, October 13, on how a body answers a pull** (direction):
  pulled gently a miner steps along and stays on its feet, pulled hard
  it may fall, for now; and "I don't want anything to be a pre-made
  animation, and it should not switch from one kind of response to the
  other at a certain threshold. It should be a smooth transition that
  follows how a person is expected to really react to pulls... It
  should also be based on their capabilities of course."
  ([the messages](Correspondence/2026-10-13_HOW_A_PULLED_MINER_ANSWERS.md)).
  Held against that, the step jumps in two places: its smallest step
  is about 22 cm however little it was needed, and the three step on
  one clock. **Next: getting ready before a first step, and a shorter
  first step** (the part of step 3 not yet built). **Begun that
  evening, and switched off:** on the bench a pulled body shifts its
  weight for 0.6 s and takes a step of 13 cm for 22; in the game, on
  the meadow, standing on one leg against the pull, its one ankle is
  asked for what two shared, its boot tips and it falls. The getting
  ready and the step have to overlap
  ([the plan](NextMilestonePlan.md#step-3-as-it-is-to-be-built-written-october-10-before-it-is-begun),
  "Begun the same evening"). On the work branch only. **A second try,
  with the two overlapping, does it for Small and Round** (pulled
  gently in the game, Small's first step is 8 cm for 22 and it moves
  14 cm in all for 30) **and fells Long,** which runs out of time
  getting ready. **That night it was made to stand for all three and
  switched on, and then off again:** Long, alone, stood 3 runs of 4
  in five to seven steps with it, and 4 of 4 in two steps without.
  It is right for Small and Round and wrong for Long, and stays off
  (on the work branch) until it is right for all three; where to
  take it up is in the plan. **Both builds are as they were.**
- **Two builds, the afternoon of October 13.**
  `Builds/WindowsOrdinaryPlace` is the morning's, left as it was for
  Luis to judge. `Builds/WindowsOrdinaryPlace_Oct13b` steps exactly as
  it does and differs only after a fall: a miner that has got up now
  steps its feet together before it stands by its own joints, and
  shoved again it mostly catches itself (Long and Round 4 runs of 4,
  Small 4 of 6: where it falls the meadow slopes more). Three things
  tried for what the judges pointed at (a swing as long as the leg
  asks, a slower second step, a boot just put down not lifted again)
  cost falls or steps and are switched off
  ([the plan](NextMilestonePlan.md#step-3-as-it-is-to-be-built-written-october-10-before-it-is-begun),
  "October 13, afternoon").
- **Step 3 (a step) is in the game since October 13. Luis tried it
  that night: liked, not said accepted** (the entry at the top). A miner
  standing by its own joints, shoved or pulled harder than it can stand
  through, puts a boot down where its weight is going and brings the
  other after it. On the panel: **"Catches itself with a step"** (on
  from the start), **"Shove it"**, **"Pull it"**
  ([what to try](Playtests/MinersPlaytest.md#a-step-october-13-for-luis-to-judge)).
  - **In the game, on the meadow, with life in the miners:** shoved at
    the button's 0.5 m/s from four sides, all three stood every time
    (12 of 12, in each of five runs); at 0.7 m/s, 10 of 12. Under the
    button's pull (0.08 of its weight for 2.5 s) Small and Long stood
    9 runs of 9 and Round 7. An eighth of its weight fells them.
  - **The bench had said 24 of 24 and the game gave Small one or two of
    four.** Five things were found by setting the two side by side
    step by step
    ([the plan](NextMilestonePlan.md#step-3-as-it-is-to-be-built-written-october-10-before-it-is-begun),
    "October 13"). The two that matter most: **the step that brought
    the other boot alongside felled it, not the catch;** and **a
    figure found by a search on the bench's level floor (how near a
    sole's edge its push may act: 4 mm) tips a boot over on the
    meadow, which slopes two degrees.** It is 10 mm now, and that is a
    narrow window.
  - **Three judges looked at it once** (pictures of each miner shoved
    from three sides). Two: "reads as real, with a flaw". The third:
    a person would point at the first step, which is a snap (a boot
    in the air in one picture, down in the next). All three: once it
    has caught itself it freezes, and its second step starts out of
    stillness. **None of that is mended:** it is how the step is
    made, and Luis has not seen it yet
    ([each finding, checked](Design/TheBodysOwn.md#what-the-judges-said-october-13-one-round)).
  - **Not mended in the morning's build** (mended in the afternoon's)**:**
    after a fall a miner stands by its own joints as its getting up
    left it, feet apart and staggered, and shoved again from there it
    mostly falls. Getting ready before a step, and setting off, are
    not built.
  - **A false line of mine is put right:** the work branch's commit of
    the morning said the standing body's tests passed 8 of 8; it was 7
    of 8 (written before the result was read). They pass 11 of 11 in
    the last two whole runs.
  - The question open since October 12 (what a pulled miner is to be
    held to) was answered by Luis that afternoon: the entry above.
- **Step 1 of the body's own is accepted by Luis (October 10, night):**
  "Indeed yes to both": the miner standing by its own joints, with life
  in it; and its switch on from the start. A miner at ease with empty
  hands now stands so without being asked; the body as it was is one
  click away. The looks of breath drawn together: "Yes that's perfect I
  think." ([the message](Correspondence/2026-10-10_STEP_ONE_ACCEPTED.md)).
  **It is the first thing of S3b that is accepted. The step (step 3) is
  not.**
- **The pickaxe miss is found** (Luis: "Yes I think so, good
  suggestion"). Three faults came out of it
  ([the design page](Design/TheBodysOwn.md#the-miss-at-a-low-boulder-october-10);
  three reviewers, whose findings changed it):
  - **Mended:** a tall miner at a low boulder, taken off its place by
    its own steps, sometimes swung from where it stood, and the blow
    landed 30 cm off. It now swings only from its place, and keeps its
    feet as it takes its pickaxe up.
  - **Mended:** a blow that met its rock and a bush at once was given
    to the bush, and the rock took nothing (Small, at one boulder: 3
    blows of 8, and every blow when it had stood by its own joints
    first). **Found only because a reviewer asked for the test as the
    game is played;** it had been run only with the switch off.
  - **Not mended:** breath drawn on a working miner's chest and
    shoulders moves where its blows land (Round's, to 25 cm off). So a
    miner at work, or resting from it, still shows its breath only in
    the air at dusk. **Luis, October 11: "Sure we can wait first for
    steps 4 and 6"**
    ([the message](Correspondence/2026-10-11_BREATH_AT_WORK_WAITS.md)):
    it waits, and walking and the work are each to bring it.
  - **Not mended either:** Long at a low boulder is still taken a
    hand's length back by about one blow in three, and walks back to
    its place each time. That is the swing itself, which becomes the
    body's own in step 6.
  - **Tests can now be run as the game is played** (`-ownAsPlayed 1`).
    The whole suite so run, before the panel's line was mended: 155 pass and 17 fail (172 pass with the switch off). Two of the 17 were a fault of the game as played (the panel's line hid that a pickaxe lies by the miner), mended, and pass as played since; thirteen are tests that take the body by hand, which the game does not do to a body standing by its own joints; two measure a hand against how the body as it was stands. Sorted by reading each test, not by running each again ([the design page](Design/TheBodysOwn.md#the-whole-suite-as-the-game-is-played)).
- **Step 3, October 12: saved in the middle of the work** (Luis: "nice
  progress. Please continue now then with it"; then the machine had to
  go off). A search with three new levers (the swinging leg's hip
  sprung, the trunk leaning back, sinking into the step) stood the
  first two bodies ever through a pull of a sixth of its weight (2 of
  24; an eighth: 18 of 24), and a pull that comes on over a second is
  stood far oftener than one that comes at once. A second round of
  reviewers overturned my conclusion that a sixth could not be done.
  (Taken up and written out on October 13: the entry above, and
  [the plan](NextMilestonePlan.md#step-3-as-it-is-to-be-built-written-october-10-before-it-is-begun),
  "October 12: three more reviewers".)
- **Step 3 (a step), October 11: one pulled body traced, three
  reviewers, and two searches set like against like.** A step begun on
  what it feels, as far as that has lasted, and a boot landed only
  when it bears: with these a held pull of 0.08 of its weight is stood
  by all 24 of 24 (15 before) and one of 0.12 by 11 (1 before). What
  shoves gained that day was the search's, not the new rules'. **A pull
  of a sixth of its weight or more is still never stood through, and
  why is not known:** three things are to be measured first
  ([the plan](NextMilestonePlan.md#step-3-as-it-is-to-be-built-written-october-10-before-it-is-begun),
  "Where it stands"). (As it stood on October 11: the step was then on
  the work branch only.)
- **Luis saw step 1 with its switch on, on the evening of October 10:**
  "I am loving the results." (That morning the switch had not been
  ticked: what follows in this entry was said of the body as it was.)
  Every miner now breathes on one clock, gently at rest and more the
  more it has spent; the size made up that morning was "a bit too
  strong" and is back to one
  ([the message](Correspondence/2026-10-10_THE_SWITCH_TICKED.md);
  [what is built](Design/TheBodysOwn.md#every-miner-breathes-october-10-evening)).
  (Step 1 was accepted that night: above.)
- **Luis looked at step 1 on October 10, and could hardly tell it from
  the body as it was:** "I couldn't really notice the difference. I
  could only really notice the breathing when the character fell."
  That was not intended. The standing breath is now drawn as large as
  the fallen one, and the panel can set its size
  ([the message](Correspondence/2026-10-10_STEP_ONE_LOOKED_AT_AND_AN_OUTSIDE_REVIEW.md)).
  (The switch had not been ticked: the entries above.)
- **`main`:** Luis merged the work branch into `main` on October 10
  (pull request 3). What came after is on `claude/worker-showcase`.
- **An outside review** of the whole project is kept, unchanged, in
  [Reviews](Reviews/2026-10-10_Independent_Outside_Review.md): advice,
  not direction. Nothing was changed because of it.

- **Now: S3b, the body's own. Approved by Luis on October 9; step 0 is
  done, step 1 is built and accepted, and Luis said on October 10 to go
  on. The looks of breath (its chest, its shoulders rising, its breath
  seen in the dusk air) are drawn together, by Luis's word; each can
  still be seen alone on the panel. Step 3, a step, is in the game
  since October 13 for Luis to judge (not accepted): shoved, a miner
  catches itself with a step and brings the other boot after it; pulled
  gently it steps and stands; pulled with an eighth of its weight or
  more it still goes down** ([the plan](NextMilestonePlan.md);
  [the design as it is built](Design/TheBodysOwn.md);
  [Luis's answers](Correspondence/2026-10-09_THE_BODYS_OWN_ANSWERS.md)).
  - **Step 1 is built (October 10), accepted, and on from the start**
    ([the design](Design/TheBodysOwn.md#step-1-the-rest-of-it-the-body-in-the-game-and-the-life-in-it-october-9-and-10);
    [its judges](Reviews/2026-10-10_StandingWithLife_Judges.md);
    [how to look at it](Playtests/MinersPlaytest.md#standing-by-its-own-joints-october-10)).
    - **The panel has a switch, "Stands by its own joints".** The chosen
      miner, at ease with empty hands, then stands by what its own
      hips, knees and ankles give. Sent somewhere or put to work it
      gives its body back to the posed one; pushed harder than it can
      stand it goes over into the fall that was built, gets up, and
      stands so again.
    - **Stable:** with no life in it, heads within 0.00 to 0.07 mm;
      nudged four ways (0.15 m/s), heads go 11 to 28 mm and come back.
      With life in it, on one leg and nudged: 15 to 83 mm, nobody down.
    - **There is life in it** (breath, its weight from leg to leg, a
      slow drift, a head that looks about), each asked of its joints
      and none a pose. **Six judges of six: stable, and alive only in
      its head.** Its breath and its shift of weight cannot be seen.
    - **It cannot step.** So it does not lean as today's body does
      (20 to 30 N held; today's posed body, 50 to 150 N), and its shift
      of weight is small: a fuller one took a nudged miner over. Both
      wait on the plan's step 3.
    - **Three questions, answered by Luis on October 10**
      ([the message](Correspondence/2026-10-10_STEP_ONE_THREE_ANSWERS.md)):
      the measure "a centimetre or two" holds between shifts of
      weight; for breath, the shoulders rising and breath seen in the
      dusk air are tried next, beside the three looks; the step
      (step 3) is built before the knees onto the feet (step 2).
      (Said then: nothing of whether the build was opened. Step 1 was
      accepted later that day.)
    - **A fault of the keeper found by tracing:** it asked the ground to
      push from beyond the edge of a sole, which only tips the boot.
  - **Step 0 found** ([figures](Design/TheBodysOwn.md#step-0-the-ground-for-it-october-9)):
    - **A body can stand by its own joints as still as Luis asked:**
      all three miners, twenty seconds, their heads within 0.1 to
      0.35 mm once settled; twenty-five of twenty-five of each on a
      level floor.
    - **Even at the game's own fifty steps of physics a second, but
      less steadily** (step 1's first part, the same day; step 0 had it
      only at 200). With each joint's spring set by a rule of the
      engine's makers, all three stand a minute at 50. Given a light
      shove (0.2 m/s), 3, 20 and 21 of 24 stay standing at 50; 19, 24
      and 19 at 100; all at 200. The rate is a trade, chosen later
      ([figures](Design/TheBodysOwn.md#step-1-first-part-how-often-the-physics-steps-october-9)).
    - **Not by the engine's joint springs alone:** by torques worked
      out for its legs each step, with soft springs holding the pose.
    - **It does not lean yet:** Round took a pull of 30 N; Small and
      Long fell at it. Today's posed body leans at 50 to 150 N.
    - **Cost:** 0.05 ms of each fiftieth of a second for a standing
      body at 50 steps a second, 0.11 at 100, 0.22 at 200 (in the
      Editor).
    - **Nothing in the game is changed:** it is a bench and figures.
  - **Reviewers changed its conclusion twice.** The first writing of
    step 0 said the physics must step four to ten times as fast; three
    reviewers showed the figures did not carry that. The next writing
    said fifty would do; a fourth showed that was too much the other
    way. What stands is between
    ([step 0's](Design/TheBodysOwn.md#what-the-reviewers-said);
    [step 1's first part](Design/TheBodysOwn.md#what-the-reviewer-said)).
  - **What it changes:** standing is a keeper to be designed, not
    springs to be set, and step 1 is larger than the plan said. The
    body can be begun at the game's own step; how often the physics
    steps is chosen when it is in the game.
  - **Decided by Luis:** the body moved by its own joints replaces "no
    active ragdoll"; it comes before S4 ("we will probably end up doing a
    big rework I imagine for it").
  - **Accepted by Luis as the measures:** the plan's table of "stable"
    ("they looked pretty good for me").
  - **Breathing:** wanted, "following the visual aesthetic of the game";
    drawn on the chest if that reads well, another way if not. Open.
- **Before it: Luis's direction of October 9, and what was built on it**
  ([the message](Correspondence/2026-10-09_THE_BODYS_OWN_NOT_POSED.md);
  [the round's page](Reviews/2026-10-09_TheBodysOwn_Round.md)).
  - **What Luis said:**
    - Movement is to be **the body's own, not posed, and stable:** "I
      want stability in the body but with it still being the body's own.
      I don't want it to be posed like that."
    - **Natural and alive, not robotic,** and studied before it is
      built: "we must do media studies and research of what kind of
      movements feel natural for certain actions."
    - **How long a fallen miner lies** is to be researched, and to
      depend on the force that pushed it down, its stamina, and anything
      else relevant.
    - **Judges for every further step, always.**
    - Luis **could not open a test** of the playtest round. Nothing of
      that round or of this one has been played
      ([how to open the build](Reviews/2026-10-09_TheBodysOwn_Round.md#how-to-open-the-build)).
  - **Built: how long a fallen miner lies.** It lay 1.2 s whatever had
    happened. It lies now by how hard it came down, how hard its head
    struck, how spent its legs and back are and how strong it is: 0.8 to
    1.9 s after a light push, 1.9 to 6.7 s after a hard one; the panel
    says how long and why. The settings are mine, set between the one to
    three seconds of games and the ten of old people who fall and get
    up; they are for Luis to change.
  - **Judged twice** (three judges each time, who did not make it): the
    times read "about right"; what stood is in
    [the round's page](Reviews/2026-10-09_TheBodysOwn_Round.md#what-the-judges-of-the-lying-said).
    Put right: its legs stood in the air for most of a second after it
    had landed on its back. **Open: a fallen miner lies quite still.**
  - **Written: the research**
    ([Alive, and the body's own](Research/2026-10-09_AliveAndTheBodysOwn.md)):
    how long a fallen body lies; **eight things a living body does**
    that a robot does not, and where the miners are in each (two not at
    all: they stand and lie perfectly still, and every step is the same
    step); how a body is walked by its own joints and kept up.
  - **Proposed that day, and approved by Luis the same day:**
    [the body's own](NextMilestonePlan.md) (above).
  - Luis merged it into `main` on October 10 (pull request 3); nothing in it is Locked.
- **Before it: the playtest round; all ten notes looked into, and waiting
  for Luis to play.** Luis played the build of S3 and sent ten
  notes ([the message](Correspondence/2026-10-08_THE_PLAYTEST_NO_FAKE_ANIMATIONS.md));
  they come before S4 ("this polishing run and the fixing run" first).
  Each is kept track of in
  [the round's page](Reviews/2026-10-08_ThePlaytestRound.md).
  - **The rule Luis gave:** "I don't want any fake animations." Every
    movement is the body, with its weight, acting on the world, and must
    read as real to someone watching ("fantasy media realistic"). Recorded
    as a Direction, in Luis's words.
  - **Done, and for Luis to see:** the fireflies (each has a home in the
    meadow; fewer, and fainter from far); the cabin's light (it no longer
    jumps as the camera pulls back; one choice of look is Luis's, key 0);
    the stutter after each step of the walk (the hips fell 5 to 13 cm in
    one frame at every footfall; now 2 to 3 mm); Long's rest that never
    ended.
  - **Made again, and for Luis to see** (the second part, October 8):
    - **Going down for the pickaxe** is one movement now: the back, the
      knees and the hand through the same stretch of time, the body
      looking at what it goes for; it stands up with knees and back
      together. It was built in parts and went in parts.
    - **The clothes:** a coat's flap hangs by its own weight and lies on
      the thigh; free arms hang by their own weight (bowed, they stood
      out like wings).
    - **Turning round, and being sent back while walking:** a walker
      sent back stops, turns as from standing, and walks; a foot goes
      round the standing boot by where it is; the body goes no faster
      than its legs carry it. By the numbers, six of the nine traced
      walks and turns pass every limit (none did before the round).
    - **Found by the trace, not by a note:** the upper body shook fifty
      times a second whenever the back moved (drawn in jumps between the
      physics' steps). Gone.
    - **The lantern, once it is held** (the third part): it was
      carried on a straight arm held out to the side; it is carried now
      on a hanging upper arm with the forearm raised, and the body looks
      at what it reaches for.
    - **After the judges of the pick-up:** a pickaxe carried in one
      hand hangs from the hand (it was held level and still); the elbow
      of the carrying arm is back; the back waits for the knees in
      standing up; a coat's front hangs between open knees.
    - **Getting up after a fall** (the fourth part, October 9): the
      let-go body turns itself over onto its front and draws its knees
      under it, by its own joints' strength; only then does the posed
      body take over, in half a second, and it stands. (It curled where
      it lay, and the posed body took over from there.) All three
      miners, from their backs, fronts and sides: 12 of 12. The poses
      were found by a search in the simulation, not set by hand. The
      panel has a button to push the chosen miner over, to see it.
  - **Not the body's own yet:** the half second from its knees onto its
    feet; and the standing up, which is posed as the walk is
    ([which is which](Reviews/2026-10-08_ThePlaytestRound.md#what-is-still-posed-and-what-is-the-bodys-own)).
  - **Judges:** one round each of the turn (after the first part), the
    pick-up (after the second) and the getting up (the fourth). All:
    "reads as real, with a flaw". What stood of the turn and the pick-up
    was put right; of the getting up, most stays open (it sits folded a
    second and a half before it rises; three bodies move on one clock;
    Long rests on its head). And one of the lantern (the end of the
    fourth part): the hand that carried it was held still while the
    body walked; it goes with the walk now, and the body looks before
    it reaches. And one of the walk, last: nothing breaks in it; what
    stood is open (nothing gets ready before the first step, which is as
    long as the rest; a standing body is perfectly still; the coats'
    panels are stiff). No second round of any yet.
  - **Found in the last run, and not looked into:** Long's blows at a
    boulder landed 31 cm from where they were aimed, once in five runs
    of that test (it allows 20; the other four were 15 to 16). Nothing
    of the swing was changed in the round's fourth part. Long's swing
    is near that limit, and not steady.
  - **For Luis to decide:** Long has two pickaxes (the one modelled on
    its back, and its own in its hands).
  - **How movements are checked now**
    ([the judging of movement](ArtDirection/MotionJudging.md)): traced
    frame by frame at the rate Luis plays at and read for breaks; then
    judges who did not make them; then Luis. What real bodies do, with
    sources, is in
    [the research](Research/2026-10-08_RealBodiesAndHowToJudgeThem.md).
  - Luis merged it into `main` on October 10 (pull request 3); nothing in it is Locked.
- **Before it: S3, weight and strength at the rock.** Approved by Luis on
  October 6 ([the plan](Plans/S3_WeightAndStrengthAtTheRock.md);
  [the design](Design/ThePhysicalBody.md)).
  - **What it is for:** a body with real strength moving a tool with real
    weight against a real rock, true and not goofy. Luis: "that's the whole
    point of it."
  - **How it came about:** Luis restated what the prototype is for and
    asked for a review of the whole project
    ([message](Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md);
    [review](Reviews/2026-10-06_SamePageReview.md)).
    The look is on Luis's page. The work on what the prototype is for had
    not begun, and the mining built that week was a drawn path with no
    weight in it ([a clip of it](Images/Miners/Work_Clip_Side.gif)).
  - **Luis's answers** ([message](Correspondence/2026-10-06_STABLE_BUT_ABLE_TO_FALL_AND_THE_INTERACTION_CLICK.md)):
    - **The order:** S3, then carrying and equipment (S4), then the creator
      (S2), then readiness for players (S5).
    - **The physics:** a mixture. Very stable, never wobbly, yet able to
      trip or fall "ragdoll-ish" in an extreme situation.
    - **The lantern and the mug** hang from the clothes; hands are empty.
    - **Added:** stamina (short-term tiredness); the interaction click (a
      key and a click show a thing's options, with a cancel); every action
      done physically by the body; all of it built to be built upon.
  - **Steps:** twelve, in [the plan](Plans/S3_WeightAndStrengthAtTheRock.md#steps).
    - **Step 1, everything weighed: built** (October 6). Small weighs
      40.7 kg, Long 58.5 kg, Round 87.6 kg; their pickaxes 1.50, 3.21 and
      2.39 kg ([the table](Design/ThePhysicalBody.md#step-1-everything-weighed-october-6)).
    - **Step 2, the bench: built; Luis finds it "very promising so far"**
      and says to go on with the plan as it is
      ([message](Correspondence/2026-10-06_THE_BENCH_IS_PROMISING.md)).
      (October 6.) Round raises a pickaxe that has real weight and brings
      it down on a block, at three strengths and three weights. How fast
      it rises and how hard it lands come out of the weights and of what
      the arms can give; a weak miner with a heavy pickaxe visibly
      struggles ([clips and figures](Design/ThePhysicalBody.md#step-2-the-bench-october-6);
      [the clip](Images/PhysicalBody/Bench_Round_Strengths.gif);
      [all nine](Images/PhysicalBody/Bench_Round.gif)).
    - **Step 3, both hands free: built** (October 6). Small's lantern
      hangs from a hook on the coat and Long's mug in a loop of thong at
      the hip; both hands of all three miners close on a handle
      ([pictures](ArtDirection/TheMiners.md#both-hands-free-the-lantern-and-the-mug-at-the-hip-october-6)).
    - **Step 4, the swing: built** (October 6). All three miners swing
      their own pickaxes with both hands. The body adapts by itself: the
      upper hand goes up the handle by as much as the tool feels heavy;
      the back has its own strength; the hips go back to keep the weight
      over the feet; aimed at a point, it bows and bends its knees to
      reach it
      ([clips and figures](Design/ThePhysicalBody.md#step-4-the-swing-october-6)).
      Long's pickaxe is heavy for Long.
    - **Step 5, tiredness: built** (October 6). Each arm and the back tire
      with hard work and come back with rest. After about 26 swings Round
      is 40% spent: its blows are weaker and its hand is up near the
      pick's head. It then stands up and rests, the pickaxe carried in one
      hand at its side, and goes back to work in under half a minute
      ([clip and figures](Design/ThePhysicalBody.md#step-5-tiredness-october-6)).
    - **It could be tried in the build with keys** (October 6 to 8; since
      step 11 a panel takes their place, below): in the Ordinary Place
      `K` showed this swing on a block, in place of the old swing; `,`
      and `.` made the miner weaker and stronger, `-` and `=` made its
      pickaxe lighter and heavier
      ([how to try it](Playtests/MinersPlaytest.md#a-first-look-at-the-work-october-6);
      [clip](Images/PhysicalBody/Look_InThePlace.gif)). It is still the
      bench, not mining: nothing is mined, and a boulder by a click is
      step 9.
    - **Step 6, balance: built; Luis finds its clips "looking pretty
      good"**
      ([message](Correspondence/2026-10-07_THE_BALANCE_LOOKS_PRETTY_GOOD.md)).
      (October 7.) The body keeps its own
      balance by its real weights. At ease nothing moves. Pulled a
      little, it leans against the pull; pulled hard, it steps and stands
      set against it; a heavier miner takes more pulling. The miners set
      their feet apart for the work. The knees have a strength of their
      own. Very stable: with its own pickaxe no miner steps
      ([clips and figures](Design/ThePhysicalBody.md#step-6-balance-october-7);
      [the clip](Images/PhysicalBody/Balance_Round_Pulled.gif)). The fall
      is step 10.
    - **Step 7, holding and walking with the tool: built** (October 7).
      Each miner walks with its own pickaxe in one hand at its side. One
      too heavy for its hand's hold is dragged by the end of its handle,
      the body stooping and walking slower; one it cannot move, it
      leaves. A tired arm drags what it carried. A miner sent somewhere
      takes its pickaxe along
      ([clips and figures](Design/ThePhysicalBody.md#step-7-holding-and-walking-with-the-tool-october-7)).
    - **Step 8, first half: built** (October 7). The interaction click:
      the space bar and a click open a thing's options beside it. The
      pickaxe is laid down on the ground and picked up again by the
      miner's body, and a miner rests when told
      ([clips and figures](Design/ThePhysicalBody.md#step-8-first-half-the-interaction-click-and-the-pickaxe-laid-down-and-picked-up-october-7)).
      Space is a proposal, not yet Luis's choice.
    - **Step 8, second half: built** (October 7). The lantern and the mug
      are taken in hand and hung back by the miner's body: the hand
      reaches to the handle, lifts it off its hook, and carries it at the
      side with the arm hanging, as it was carried before October 6
      ([clips and figures](Design/ThePhysicalBody.md#step-8-second-half-the-lantern-and-the-mug-taken-in-hand-and-hung-back-october-7)).
      Setting them down on the ground is not built: each would have to be
      made an object of its own. A pickaxe in one hand with the lantern in
      the other is not built either.
    - **Step 9: built** (October 7). A click on any boulder of the place
      offers "Mine". The miner finds where to stand and where to strike
      from the rock's own shape, goes there (the last steps off the
      walked ground, which stops half a metre short of every rock),
      strikes it, and pieces break off and lie
      ([clips and figures](Design/ThePhysicalBody.md#step-9-any-boulder-by-a-click-october-7)).
      A tool too heavy to rest holding (Long's pickaxe) is now put down
      to rest, its head on the ground: Long used to stop after four
      blows. The boulder does not get smaller or run out, and the stones
      cannot be picked up yet (S4).
    - **Step 10: built, not yet trusted** (October 7). A miner that cannot
      keep its feet falls: its whole body follows the physics, holding
      itself with what strength it has; it lies, gathers itself, and gets
      up ([clips and figures](Design/ThePhysicalBody.md#step-10-the-fall-and-getting-up-october-7)).
      It takes something extreme: a pull of about three fifths of its
      weight, or legs that give way under work far too heavy. At its
      work, a miner that falls lets its pickaxe go. Luis has seen clips
      only.
    - **Step 11: built; Luis has not seen it** (October 7 and 8). The
      keys (`K`, `,` `.`, `-` `=`) are gone. A plain panel at the lower
      left sets the chosen miner's strength and puts a light, its own or
      a heavy pickaxe on the ground beside it; the miner picks it up
      itself, by the interaction click, and nothing is made in its hands
      ([pictures and figures](Design/ThePhysicalBody.md#step-11-the-panel-in-the-ordinary-place-october-7)).
      - **What trying the whole slider found, and changed:** the squat
        for a pickaxe asked one knee 93 to 96% of what it has even at
        ordinary strength, and below it a miner fell or not by tenths of
        a second. Now a body bends its knees no deeper than one could
        hold alone; it places itself and sets its feet first; bent to
        the ground it keeps its feet where they are; and bent to its
        work at a rock, its knees are read before the first blow.
      - **What that leaves:** below about nine tenths of its strength a
        miner does not get down to a pickaxe (it stands up again, and
        the panel says why); the weakest bodies at their work are not
        designed.
    - **Step 12: the evidence is gathered; Luis has not played the build**
      (October 8;
      [the plan's list, item by item](Design/ThePhysicalBody.md#step-12-evidence-october-8)).
      - **Shown:** ten of the twelve things the tests were to show; one
        in part; one not (nothing stops the pickaxe at its bearer's own
        body).
      - **Cost, in the release build:** one miner at its physical work
        costs nothing that shows in the frame (4.5 to 4.8 ms working, 4.6
        to 4.7 ms standing); a hundred walking miners add about 2.5 ms
        (2.1 on October 4).
      - **Found by running the build, and put right:** Small fell five
        seconds into any long walk with its pickaxe (the fall's rule was
        counted while walking). Tried at every boulder, eight of thirty
        miners and boulders gave their rock up for their knees: a miner
        now looks for a way to stand that bends them less, and each
        miner mines each of the ten boulders.
    - **S3's twelve steps are built.** What it needs now is Luis's play of
      the build ([the guide](Playtests/MinersPlaytest.md#the-physical-work-with-the-panel-october-7)).
      Luis merged S3 into `main` on October 10 (pull request 3); nothing in it is Locked.
    - **Next, by the approved order, after the playtest round:** S4
      (carrying and equipment), then S2, then S5. **S4 is proposed, not
      begun** ([the proposal](Plans/S4_CarryingAndEquipment_Proposal.md)):
      it says which of the prototype's questions it answers, its ten
      steps, and six things it needs Luis to say first. The first of
      them decides how all of it is built: whether lifting a stone,
      shouldering a pack and pulling a cart are to be posed (as the walk
      is) or the body's own (as the fall is).
- **The base look:** the hand-painted pass (S1c, second pass) in look E,
  merged into `main` on October 2 (7f7fcc0). Luis's favourite frame is from
  it: the lit house at dusk, seen from low on the path.
- **Latest implementation:** the dusk details over that look (third pass).
  Luis tested them and "loved it", so all three are on by default: fireflies,
  the hearth's flicker and a window glow. Keys 7, 8 and 9 switch each one.
  After Luis's notes:
  - **Fewer fireflies**, with a small painted dot still marking them when the
    camera is far out.
  - **The lamplight close up** now matches the zoomed-out look Luis liked.

  See [OrdinaryPlaceLookTest.md](Playtests/OrdinaryPlaceLookTest.md#after-luiss-test-october-2);
  build `Builds/WindowsOrdinaryPlace`.
- **Now:** S1d, the worker's redesign, from the Visual Soul. Its principles
  are the base art style for every being and thing in the world, not one
  character's costume
  ([VisualSoul.md](ArtDirection/VisualSoul.md#the-language-for-every-being-and-thing)).
  - **Luis's verdict on that round (October 5):** "I really liked all the
    changes you did, except the shoulder strap."
    - **The strap** on Small's shoulder is put back as it was: out on the
      shoulder, soft on the coat, below the collar. Its ends stay on the
      bag's rings
      ([TheMiners.md](ArtDirection/TheMiners.md#after-luiss-look-october-5)).
    - **The method learned:** change only what a note or a failed check
      names.
    - **Next, on Luis's word to go on with the plan: the miners at work**
      ([NextMilestonePlan.md](Plans/S1_OrdinaryPlaceCamerasAndMiners.md#s1d-last-part--the-miners-at-work-plan-waiting-for-luiss-choices)).
      - **Step 1, the sweep of extreme poses, is built.** Nothing fails in
        the poses the game uses. What fails beyond them (arms overhead, a
        deep bend in a coat, a deep bow) is logged with the movement that
        would need it
        ([TheMiners.md](ArtDirection/TheMiners.md#the-sweep-of-extreme-poses-october-5)).
      - **Step 2, hands that close, is built.** The free hands (Small's
        and Long's right, both of Round's) close round a handle of any
        thickness from 12 to 44 mm and open again, measured on the skin
        ([TheMiners.md](ArtDirection/TheMiners.md#hands-that-close-october-5)).
        Nothing shows in play until a tool is in the hand.
      - **A question for Luis came out of it:** in the game Long leans back
        11° compared with the model as built
        ([pictures](ArtDirection/TheMiners.md#for-luis-how-long-stands-in-the-game)).
        Not changed.
      - **Steps 3 and 4 are built (October 6): the miners mine.** Each has
        a pickaxe made for its arms and hands; the tool's solve reads the
        body (reach, a front to keep clear of, which hands are free); the
        swing goes over the shoulder; the mining is recorded and audited
        with the pickaxe in the hands
        ([TheMiners.md](ArtDirection/TheMiners.md#a-pickaxe-for-each-and-the-first-swings-october-6)).
        That swing had no weight; since later on October 6, `K` in the
        Ordinary Place shows the swing with real weight instead
        ([MinersPlaytest.md](Playtests/MinersPlaytest.md#a-first-look-at-the-work-october-6)).
      - **Step 5, the miners at the rock, is overtaken** by Luis's message
        of October 6: M1 is answered (any boulder, by a click), M2 to M4
        become choices of equipment, and the swing is to have real weight
        before it is taken to a rock.
  - **A method for model quality, and its first round (October 3 and 4).**
    Luis asked for a strategy, to refer to from now on, for making every
    model as polished as possible. It is
    [ModelQualityMethod.md](ArtDirection/ModelQualityMethod.md):
    - the standard (nothing floats, nothing clips, objects obey gravity);
    - four questions for every object;
    - seven passes, including automatic checks at rest and in the game's own
      movement;
    - a capture matrix of every angle and distance;
    - a ledger of every miss.

    **Its first round is done and waits for Luis's play**
    ([TheMiners.md](ArtDirection/TheMiners.md#the-methods-first-round-october-3-and-4);
    [MinersPlaytest.md](Playtests/MinersPlaytest.md#after-the-methods-first-round-october-4)):
    - **Luis's five notes on Small** are answered: the leg goes into the
      boot; laces lie on the leather; the strap's ends pass through rings on
      the bag; the hand closes round the lantern's handle; the bag hangs from
      its strap and rests on the hip.
    - **Every object is a proper object** on all three: held, hanging,
      swinging, stopped by the body. Long's pickaxe hangs in a sling; Round's
      hammer hangs in a loop on an apron that is tied on.
    - **Found by the method beyond the notes:** eighteen more, among them
      knees through the coats mid-stride, boots through each other in sharp
      turns, and a lantern that leaned even standing still.
    - **Left open, each with its reason:** four small audit checks (three of
      them single moments of the sharpest turn). The sweep of extreme poses
      was built on October 5.
  - **Polished after Luis's play (October 3, evening).**
    - **Small:** the lantern is carried in the hand and swings; the satchel
      hangs from its strap at the hip; legs line up with the boots; the neck
      rises into a closed collar.
    - **All three:** polished on a close audit in the game.
    - **Walks:** each body walks naturally at its own pace.
    - **Practices:** the character practices and the audit are in
      [CharacterPractices.md](ArtDirection/CharacterPractices.md).
  - **Rigged and in the game (October 3).**
    - **Walking.** Small, Long and Round walk the Ordinary Place on the
      procedural body, whose proportions now come from each model.
    - **The choice.** The player chooses one when the place loads (`M` to
      change).
    - **Light.** Three levels of detail and one texture each; 100 miners
      walking add about 2 ms a frame.

    See [MinersPlaytest.md](Playtests/MinersPlaytest.md).
  - **The miners: "almost perfect… already adorable"** (Luis, October 3).
    Polished on Luis's notes: the necks grow out of the clothes, the boots
    are single boots, Round's hands rest on the hips. Smaller flaws found on
    a careful look at every distance were also fixed
    ([TheMiners.md](ArtDirection/TheMiners.md#polish-after-luiss-word-october-3)).
    All three will be offered as choices in this prototype. They are Small,
    Long and Round, remade at much higher quality with miner's hints:
    - **The models.** Real hands, hair in locks, sculpted boots, fitted and
      painted clothes.
    - **Miner's hints.** Small carries a lantern, Long leans on a pickaxe,
      Round wears a lamp cap.
    - **Built from modules** (body, face, hair, garments, accessories) that
      fit any body, towards the character creator.
  - **First concepts:** superseded
    ([WorkerConcepts.md](ArtDirection/WorkerConcepts.md)). Luis ranked them
    Small, Long, Round and loved every face.
  - **Beings stay drawn.** A material switch keeps them clear of the paint
    filter. It is off for everything else, so the world's look is unchanged.
- **Latest experiment, not adopted:** S1e, the essence beyond the surface,
  first iteration ([TheEssencePlaytest.md](Playtests/TheEssencePlaytest.md)).
  After seeing its captures, Luis preferred the hand-painted pass. Its code is
  kept on the branch `claude/essence-exploration`. It contained:
  - **The world beyond the meadow:**
    - a bluff over a still lake that mirrors monumental clouds and mountains;
    - a water horizon, layered ranges and a snow peak;
    - aerial perspective, cloud shadows and a luminous daylight palette.
  - **Look F:**
    - a painting pass whose strokes follow the forms;
    - ink on characters only;
    - the hour's palette;
    - seeds and fireflies in the air.
  - **The research.** [TheEssence.md](ArtDirection/TheEssence.md) holds the
    study of Luis's references and the proposed language unique to Wonder
    Gather. It also reads what Luis's favourite frame teaches about Luis's taste:
    intimacy, the lit house at dusk, a soft brushed sky, enclosure and a tonal
    palette, rather than vastness and brightness.
- **Earlier steps:**
  - **S0 (the grounded body), S1a (two cameras) and S1b/S1c (the Ordinary
    Place, first pass)** are in `main`.
  - **The S1c second pass (hand-painted surfaces)** is on the branch. Luis
    found it "already quite beautiful", but not yet the emotion Luis seeks.
- **Overall plan:** [ShowcaseRoadmap.md](ShowcaseRoadmap.md).

## What the project is building now

A small-scale, high-fidelity **Worker Showcase** for outside playtesters:

- one worker, made in a video-game style character creator;
- a small landscape whose boulders can each be mined, by a click (October
  6), and a place to bring the stones;
- physical mining with real weight and real strength (October 6: "that's
  the whole point of it"), and real hauling;
- models made to high quality in Blender.

The civilization/faction systems remain in the repository for the full game.
The showcase flow does not use them.

## Latest Game Director direction (October 9)

Recorded verbatim in
[Correspondence/2026-10-09_THE_BODYS_OWN_NOT_POSED.md](Correspondence/2026-10-09_THE_BODYS_OWN_NOT_POSED.md):

- **The body's own, and stable.** "I want stability in the body but with
  it still being the body's own. I don't want it to be posed like that."
- **Natural and alive.** "I really do want movement to feel natural and
  alive rather than robotic. That's why it can't be posed and we must do
  media studies and research of what kind of movements feel natural for
  certain actions."
- **How long a fallen miner lies:** to be researched, and to depend on
  "how strong was the force that pushed it down, it's current stamina and
  anything else relevant".
- **The judges:** "good job with the agent reviews, please continue with
  that for further steps always".
- **Not played:** "the rest I couldn't open a test to see how your
  progress was so far, but I liked the comments you were making."

## Game Director direction (October 8)

Recorded verbatim in
[Correspondence/2026-10-08_THE_PLAYTEST_NO_FAKE_ANIMATIONS.md](Correspondence/2026-10-08_THE_PLAYTEST_NO_FAKE_ANIMATIONS.md):
ten notes on the build of S3, and the rule "I don't want any fake
animations".

## Game Director direction (October 6, second message)

Recorded verbatim in
[Correspondence/2026-10-06_STABLE_BUT_ABLE_TO_FALL_AND_THE_INTERACTION_CLICK.md](Correspondence/2026-10-06_STABLE_BUT_ABLE_TO_FALL_AND_THE_INTERACTION_CLICK.md):

- **A mixture of stable and able to fall.** "I want characters to be very
  stable, but for them to still technically have these possibilities of
  tripping in this, like, ragdoll-ish way if it's an extreme situation that
  calls for it."
- **Stamina:** "some level of stamina for the muscles and tiredness",
  short-term at least, already in this prototype.
- **Things hang where it makes physical sense,** from the clothes.
- **The interaction click.** The ordinary right-click stays the RTS order.
  A key and a click on something show its options beside it, "MMO style",
  with a cancel. It is for actions an ordinary RTS has no click for. The
  key is to be researched; Space is proposed.
- **Every action is physical:** "It's not just the important actions …
  It needs to be all the actions."
- **Built to be built upon:** running and sword fights come later, on the
  same body.
- **The order of stages is approved.**

## Game Director direction (October 6, first message)

Recorded verbatim in
[Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md](Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md),
with what was said plainly kept apart from what was said as a present
thought:

- **Any rock.** No special rock: the player clicks one of the map's big
  boulders and the miner mines it. Small in scale, final in kind.
- **Real weight and real strength, in this prototype.** The character
  really lifts the tool; bodies "react against real physics, with real
  strength in their limbs, and against objects with real weight". Like
  ragdolls, without being goofy.
- **The creator is part of the prototype;** the three miners are its
  appearances.
- **Equipment is a choice with consequences:** no pickaxe, a pickaxe, a
  strap for the back (asks strength of the back), a backpack, a dragged
  sack, a wheeled cart. The rocks are brought back.
- **Present thoughts, not decisions:** nothing held in the hands ("I'm not
  sure yet"); strength as one general number, maybe a slider; strength
  showing gently in the body.
- **The worry:** "I'm worried you lost a bit sight of what I actually want
  with this prototype."
- **Asked for:** a review of everything, research into how a pickaxe is
  really swung, and how to proceed. Written as
  [Reviews/2026-10-06_SamePageReview.md](Reviews/2026-10-06_SamePageReview.md).

## Game Director direction (October 3, late evening)

Recorded verbatim in
[Correspondence/2026-10-03_PHYSICAL_OBJECTS_AND_A_QUALITY_METHOD.md](Correspondence/2026-10-03_PHYSICAL_OBJECTS_AND_A_QUALITY_METHOD.md):

- **Small, still to fix:**
  - the leg still not lined up with the boot;
  - the laces' crosses floating off the boot;
  - the satchel's strap not joined to the bag;
  - the lantern floating up close;
  - the bag should hang with gravity.
- **The principle.** Physicality and the reality of all movement are among
  the game's main focuses. Anything that reads as an object behaves as a
  proper object that responds to its environment, never floating.
- **Liked:** how the coat's skirt, the lantern and the bag move with the
  walk.
- **First, a method.** A strategy document, to refer to from now on:
  - look at every object and place from all angles, exhaustively;
  - research how to reach the highest polish;
  - develop a method to raise model quality.

  Written as [ModelQualityMethod.md](ArtDirection/ModelQualityMethod.md).

## Game Director direction (October 3, evening)

Recorded verbatim in
[Correspondence/2026-10-03_SMALL_POLISH_AND_NATURAL_GAITS.md](Correspondence/2026-10-03_SMALL_POLISH_AND_NATURAL_GAITS.md):

- **Walks.** Each body walks in a way natural to its appearance; one shared
  speed is not needed.
- **Small, to fix:**
  - the lantern floating through the hand;
  - legs not lining up with the boots;
  - the neck;
  - the satchel clipping and not hanging from its strap.
- **Proactive polish** on all three, before Luis has to point out every
  detail. Luis will give notes on Long and Round later.
- **Research** good Blender practice.
- **Documentation.** Everything documented and pushed to GitHub is a top
  priority.

## Game Director direction (October 3)

Recorded verbatim in
[Correspondence/2026-10-03_THE_MINERS_FEEDBACK.md](Correspondence/2026-10-03_THE_MINERS_FEEDBACK.md):

- **The miners.** "Almost perfect now! Honestly they are already adorable!"
  Luis loves their designs.
- **To polish:**
  - necks that looked glued on up close;
  - the separated feet;
  - Round's hands;
  - anything else found on a careful look at every distance.
- **Then:**
  - rigging;
  - making them light enough to run well in the RTS.
- **All three in this prototype.** Luis decided to offer Small, Long and Round
  as choices.

## Game Director direction (October 2, late evening)

Recorded verbatim in
[Correspondence/2026-10-02_WORKER_CONCEPTS_FEEDBACK.md](Correspondence/2026-10-02_WORKER_CONCEPTS_FEEDBACK.md):

- **Ranking of the concepts:** Small, then Long, then Round. Luis loved the
  face of each one, and the idea of every character: distinct, unique, not
  generic, with soul.
- **Quality.** All three can be much prettier and more pleasing to look at,
  with much higher model quality. Keep the stylization.
- **They are all the miner.** Give each some hints, even small ones.
- **Maybe a choice of characters** in the final prototype. A possibility, not
  a decision.
- **Modular, all the way down.** The final game lets players customize
  everything, units included, down to appearance, like detailed character
  creators.

## Game Director direction (October 2, evening)

Recorded verbatim in
[Correspondence/2026-10-02_DUSK_DETAILS_AND_THE_WORKER.md](Correspondence/2026-10-02_DUSK_DETAILS_AND_THE_WORKER.md):

- **The dusk details.** "Honestly I loved it." Leave all of them on by
  default.
- **Fireflies.**
  - Fewer of them. Near the ground they went "from gentle wonder inducing to a
    bit overwhelming".
  - Even when very zoomed out, there should still be some sign of them,
    stylized and cheap to render.
- **The lamplight.** Far away it looked strong and spread onto the terrain;
  closer in that brightness disappeared. Luis liked the far look, so the closer
  view should be adjusted to match it.
- **Then the worker.** "Let's finally move to the player remodel/redesign",
  using the Visual Soul and the principles for people, "with heart and soul".
- **For every being and thing.** The scarf was only one of the examples. The
  Visual Soul's principles are the base art style for all beings and things in
  the world, across every kind of unit.

## Game Director direction (October 2, afternoon)

Recorded verbatim in
[Correspondence/2026-10-02_THE_ESSENCE_BEYOND_THE_SURFACE.md](Correspondence/2026-10-02_THE_ESSENCE_BEYOND_THE_SURFACE.md):

- **The hand-painted pass.** It is "already quite beautiful" and matches much
  of Luis's taste, but is not perfect yet. Luis would rather explore further
  than stop.
- **The aim.** "The breath-taking out of this world emotion": the stylistic and
  artistic essence, not the surface; "the image from my mind's eye straight out
  of a dream". He sent seven Ghibli film references, which are described but
  not committed.
- **Every frame a painting.** The rendering style, not only the environment,
  should make any frame beautiful and driven by light, while the game still
  runs well as a modular RTS.
- **Unique.** The Visual Soul PDF is the artistic soul. The result must go
  above and beyond it, unique to Wonder Gather: "My soul game and style."
- **Time.** A long checkpoint with research is welcome.
- **The verdict.** After seeing the first iteration, Luis said "honestly I think
  I like the before better", with Luis's favourite frame from the hand-painted
  pass. The hand-painted pass stays the base. S1e's first iteration is
  archived, not adopted.
- **Luis's answers.**
  - **Merge:** the hand-painted pass, into `main`. Done.
  - **Fireflies:** back in this scene.
  - **Next:** deepen the dusk mood in small steps, then move to the worker
    model.

## Earlier Game Director feedback (October 1, night)

Recorded verbatim in
[Correspondence/2026-10-01_ORDINARY_PLACE_FEEDBACK.md](Correspondence/2026-10-01_ORDINARY_PLACE_FEEDBACK.md):

- **The Ordinary Place.** It is going "really well" towards the Visual Soul,
  but is not there yet.
- **Rendering.**
  - Candidate E (painted light + paint filter + ink) is Luis's favourite, and is
    now the working base look.
  - Hand-painted textures are the next exploration; Claude recommends them.
- **Cameras.** Accepted ("I really liked the camera").
- **Merge.** S0, S1a and the S1b/S1c first pass are merged into `main`.
- **Organization.** Everything, including docs, goes to GitHub in a very
  organized structure. The docs were reorganized into topic folders (see
  [Docs/README.md](README.md)).
- **Moon.** It was visible in daylight. Fixed: it rises opposite the sun and
  shows only at night.

## Earlier Game Director direction (October 1, evening)

Recorded verbatim in
[Correspondence/2026-10-01_VISUAL_SOUL_AND_TWO_CAMERAS.md](Correspondence/2026-10-01_VISUAL_SOUL_AND_TWO_CAMERAS.md):

- **Art direction.** The Visual Soul handoff ([VisualSoul.md](ArtDirection/VisualSoul.md)):
  - "Even an ordinary moment should feel worth pausing for";
  - lighting leads the emotion, surfaces feel alive, characters have a soul,
    and the wonder is playful and personal;
  - images A–F are approved, with no ranking;
  - proportions, architecture, setting and rendering technique remain open,
    to be decided after an in-engine test.
- **Cameras.** Two toggled systems:
  - a fast "best RTS possible" Strategy camera;
  - a free Explore camera, editor-like, cozy and soft, that can go very near
    characters, floors and buildings without passing through, and look at the
    sky.

## Earlier Game Director direction (October 1)

Recorded verbatim in
[Correspondence/2026-10-01_CHECKPOINT_A_FEEDBACK_AND_SHOWCASE_SCOPE.md](Correspondence/2026-10-01_CHECKPOINT_A_FEEDBACK_AND_SHOWCASE_SCOPE.md):

- **Walk.** It is much better. Arrival must not re-shuffle the feet to the
  exact spot. This is fixed: the last steps land where the body stops, at most
  one closing step follows, and the body then stands still.
- **Speed.** Movement speed is constant for now and is not a creator option.
- **Scope.**
  - One worker (maybe more later), with a character creator in place of the
    civilization creator.
  - Always a worker.
  - It sets the quality bar for the final unit customization.
- **Hauling.** Load in one hand and pickaxe in the other if strong enough, or
  a backpack, sack or cart. No back-stowed pickaxe, and leaving the pickaxe
  behind is not the default. This revises D5.
- **Models.** Much better quality, using Blender (4.4 and 4.5 are installed).
- **Process.** Upload to GitHub, with organized documentation at every step.

Still valid from September 30:

- **D1:** a physics-informed body (no active ragdoll).
- **D2:** a natural constant pace.
- **D3/D4:** strength bands and light/standard/heavy pickaxes.

## What exists and what it proves

- **Foundation.** Unity 6000.6.0f1 / URP. RTS controls, economy, construction,
  production, the faction creator and version-5 faction saves are still
  present for the full game.
- **Equipment.** Blueprint pickaxe equipment. Mining requires a valid head
  strike against the mineral, and material is conserved through delivery.
- **The grounded body.**
  - A phase-based gait scaled to hip height: about 2 steps/s and a 1.85 m
    stride at 1.8 m/s.
  - A jog above about 2.8 m/s.
  - Heel/toe feet, weight transfer, counter-rotation, pendulum arms and a
    level head.
  - Arrival finishes the stride; a settled stance tolerates small drift.
- **Shared stations.** Workers that run out of work step clear of their
  station, and claims skip positions a body still occupies. This fixed an
  intermittent deadlock that the slower pace had exposed (see the erratum in
  [Validation.md](Validation.md)).
- **Evidence.** The full PlayMode suite passes, and Windows builds succeed.
  See [Validation.md](Validation.md).
- **Not yet built** (put right on October 10: this list was written in
  September and had gone stale, as an outside review pointed out):
  - the character creator at the quality the showcase asks (S2);
  - carrying and hauling gear (S4);
  - walking, reaching and working by the body's own joints (S3b, in hand:
    only standing is built, and a step is begun);
  - sound, and final art beyond the Ordinary Place.
- **Built since that list was written:** the miners' own models (Small,
  Long and Round); the swing by effort, with tool weight, grip, strength
  and tiredness (S3); falling and getting up.

## Recommended next action

**Now (October 9):**

1. **Luis opens the build** and plays what the playtest round and this
   one changed
   ([how to open it](Reviews/2026-10-09_TheBodysOwn_Round.md#how-to-open-the-build);
   [what to look at](Playtests/MinersPlaytest.md#after-luiss-play-of-s3-what-to-look-at-now-october-8)).
   If it does not open, what happens when it is tried is the first
   thing to know.
2. **Step 1 of [the body's own](NextMilestonePlan.md#what-step-0-changed-october-9):**
   the body's own body in the game beside the posed one; standing still and leaning against a pull, all three
   miners; then life in it.

The items below are still open from before (October 6 and earlier).

1. **Luis checks the dusk fixes** in the build:
   - fewer fireflies, still marked when far out;
   - the lamplight close up.
2. **Luis plays the polished miners** in the build
   ([MinersPlaytest.md](Playtests/MinersPlaytest.md)):
   - Small's fixes;
   - the natural walks;
   - notes on Long and Round.

   Then mining and hauling move onto the miners (the pickaxe sized to each
   body).
3. **Further explorations are additive and switchable**, and shown early as
   frames before being built out.

Luis's other choices on October 1:

- **Model first** (O4).
- **Strength** comes from the body plus training (O2). Overtaken on
  October 6 by Luis's present thought: one general number, maybe a slider.
- **Stage merges.** Each stage merges into `main` after Luis's playtest.

## Housekeeping

- **Branch.** Work is pushed to the `claude/worker-showcase` branch. It merges
  into `main` after Luis playtests the stage (including the arrival fix).
- **Luis's uncommitted local changes** are never included in commits:
  TheGroup.unity, two ProjectSettings files and `Assets/_Recovery/`.
- **`codex/agent-bootstrap-draft`** is an unmerged, stale automation proposal.
- **`docs/wonder-gather-continuity-2026-09-23`** is already merged as d03e045
  and can be deleted.

## Context router

- Showcase plan and open decisions: Docs/ShowcaseRoadmap.md
- Next concrete milestone: Docs/NextMilestonePlan.md
- Design authority: Docs/GAME_VISION.md
- Showcase reference: Docs/Design/WorkerShowcaseVision.md and Docs/Correspondence/
- Latest playtest guide: Docs/Playtests/MinersPlaytest.md
- Reviews of the whole project: Docs/Reviews/
- Technical contracts: Docs/Technical/EquipmentArchitecture.md and Docs/Technical/UnityProjectContext.md
- Design meaning and collaboration: Docs/DESIGN_RATIONALE.md and Docs/PROJECT_CULTURE.md
- Evidence: Docs/Validation.md
- Completed plans: Docs/Plans/
