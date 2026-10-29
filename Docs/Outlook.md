# Outlook — the forest, now and then

Luis, October 14, 2026: "I do want you to get a habit of every now and then
to have a general outlook on the project and then continue focusing on the
current task, since we never want to lose the forest for the trees."
([the message](Correspondence/2026-10-14_THE_FOREST_AND_THE_TREES.md))

This page is that habit: how and when an outlook is taken, and a log of them,
newest first. An outlook is short. It looks at the whole project, says where
the current task sits in it and what is drifting or forgotten, and then the
work goes back to the task in hand.

## When

- **At the start of a session, when it is resumed, and after its context is
  compacted.** A hook says so by itself (below): read this page; if the last
  outlook is older than the cadence, take one.
- **About every 250 tool calls** (a few hours of work). The same hook counts
  them and says when one is due.
- **Before choosing what comes next** once a piece of work is finished, and
  **before a report to Luis** that closes a stretch of work.
- **Cadence:** not more often than that, and never more than a working day
  apart. Luis: "not necessarily extremely frequent but stable."

## How

1. Read, quickly: [CURRENT_STATE.md](CURRENT_STATE.md) (top), the stage table
   and steps of [NextMilestonePlan.md](NextMilestonePlan.md), the pillars and
   the open-decision register of [GAME_VISION.md](GAME_VISION.md), the last few
   messages in [Correspondence](Correspondence/), and the entry before this one.
2. Answer, in a few lines each:
   - **Where are we** in the order of stages, and in the current stage's steps?
   - **What is Luis waiting on**, and what is waiting on Luis (builds not yet
     played, questions not yet answered)?
   - **What is built but not in a build**, and **what is in a build but not
     tested as played**?
   - **What has been put off** and must not be forgotten (faults, "not yet",
     "waits for step N")?
   - **Does the task in hand still serve the stage** (and the prototype's
     purpose: real weight and real strength at work, a body that is its own)?
     Has it grown past what it was asked to be?
   - **What would Luis want to know**, or to play, now?
3. Write the entry here (date, a few lines each), change the plan if it must
   be changed, and tell Luis anything that is his to decide.
4. Reset the counter: `bash .claude/hooks/outlook.sh reset`. Then go back to
   the task.

## The setup that keeps it

- **The hook:** `.claude/hooks/outlook.sh` (in the repository), started from
  `.claude/settings.local.json` (on this machine only):
  `SessionStart` (matcher `startup|resume|compact`) runs `outlook.sh start`;
  `PostToolUse` (matcher `*`) runs `outlook.sh tick`, which counts tool calls
  in `.claude/hooks/outlook-count` and says when an outlook is due (every 250).
  To set it up again elsewhere, copy those two entries into the settings file.
- **The memory:** `forest-for-the-trees` in the assistant's memory says the
  same, so it is known even where the hook is not.

## Log

### October 29, 2026 — the fifteenth (dawn, 250 steps after the fourteenth)

- **Where we are.** The morning's report is written, committed and pushed (the night measured, the game's tries found
  to be deep retries, judges round 2). Since: the deep retries' cause, by how each try began: **the toes**. The let-go
  body has no toes and kept them as bent as a failed try left them; the next try was made from them, 45 degrees and
  more in 1,282 of 4,800 tries, which stood 3.7% (toes as they stand, 19%). The boots had the same fault, mended
  October 27; the toes, added October 14, were never given it. Mended behind `OwnBody.ToesSlacken` (on); measuring.
- **Waiting on Luis:** the morning report's two questions (what next; the old way after 12 tries); S3, the builds of
  October 13 and 14, R1, the analytics define, `risebuild3.log` as before.
- **Drifting or forgotten:** the stance the teaching and bench note (0.696 m) is not the game's (0.75); the flaky
  night-light test (a separate task offered); the models in `Resources/RiseModels` not committed.
- **Does the task serve the stage?** Yes: this is the game's own failure, found by measuring the game as played, and
  it is the kind of fault (a frozen part) that no amount of teaching would have mended. If it holds, it changes the
  morning's numbers; the addendum must say so plainly, and that it came after the report.

### October 29, 2026 — the fourteenth (before dawn, 250 steps after the thirteenth)

- **Where we are.** S3b step 2, option 1 with the learner pacing its way. `pace8` (holds
  charged as held, the pacer only shortening) at 60,000 paces and its joints at 250,000: in
  the teaching 22 to 23% of all tries stand, paced or not (the joints' best so far); the
  pacer makes the getting up about 0.6 s shorter and holds a little less (0.24 s a standing
  try against 0.30). In the game as played, the first runs with a pacer: up by its own body
  after 12 and 15 of 20 pushes, the old way after 3 and 2, not up in 300 s after 5 and 3;
  of tries, 7 and 13% up. The hand-over: 39% stood in one run, 68% in the next (the
  teaching's 68%): the game varies run to run more than was thought; pooled runs under way.
- **Waiting on Luis:** S3 (since October 8); the builds of October 13 and 14; R1 (the arms);
  the analytics define; the stray `risebuild3.log`. He has not seen option 1 move.
- **Built, not in a build:** all of the getting up (work branch, switch off).
- **Drifting or forgotten:** **the old way still ends 2 to 3 pushes in 20** in the game's
  test (after 12 tries; the code's own note says no measure reaches that, and they now do):
  Luis's aim is never to need it. The suite since the night's changes; Long and Round (no
  getting up); the work branch drifting from the game branch; the game's tries succeeding
  less often than the teaching's (most are retries of retries).
- **Does the task serve the stage?** Yes, as directed. But the joints have stood at about a
  fifth of tries since `pace4`, the pacer's gain is time, not standing, and the stage has
  been on the getting up for over two weeks. The report this morning should show Luis how
  it moves (pictures he can watch), say plainly how often the old way is still needed, and
  put the next choice to him with the prototype's purpose in view.

### October 29, 2026 — the thirteenth (past midnight, the night's teaching under way)

- **Where we are.** S3b step 2, option 1 with the learner pacing its way, holds charged (Luis,
  October 28: "Penalise holds"). At 140,000 decisions the pacer still held more than the
  searched times (0.9 to 1.0 s a standing try against 0.5 to 0.8), its answers barely less
  random than at first. Measured by pose tonight: about two thirds of the holds were in
  `hands`, lying on its front while its arms moved 10 to 20 degrees for 1.6 s. Halving that
  pose and its arm movement removes them (about 0.8 s to 0.3 s), standing unchanged; the
  pacer is now charged for each held second as it happens.
- **Waiting on Luis:** S3 (since October 8); the builds of October 13 and 14; R1 (the arms);
  the analytics define; the stray `risebuild3.log`. He has not seen option 1 move: round 2
  of the judges first, then a build with the switch named in the report's first line.
- **Built, not in a build:** all of the getting up (work branch, switch off).
- **Drifting or forgotten:** the game's test as played with tonight's models; the suite as
  played (never run); Long and Round (no getting up); the work branch drifting from the
  game branch; the old way after 12 tries (the absolute last resort).
- **Does the task serve the stage?** Yes, as Luis directed it (October 13: natural, no dead
  holds; October 28: penalise holds). But the getting up has held the stage for two weeks;
  the prototype's purpose is real weight and strength at work. Once holds and standing are
  where the game needs them, the next choice should be put to Luis with that in view.

### October 28, 2026 — the twelfth (a new day)

- **Where we are.** S3b step 2, option 1 with the learner pacing its way (Luis, October 27:
  "Yes I think that approach sounds good"). Found yesterday: the teaching's fresh falls were
  72 made once and learnt by heart; on falls made anew `full3` stands 20%, which is what the
  game sees. Now two learners, taught on falls made anew: the joints (from `full3`) and the
  pace of every pose (`pace1`, 250,000 decisions: poses about 0.1 s shorter each, standing
  not lower; the holds the judges named not yet touched).
- **Waiting on Luis:** confirmation that "yes" meant the learner pacing (taken so, said in the
  report). Still open from before: S3 (since October 8); the builds of October 13 and 14; R1
  (the arms); the analytics define; the stray `risebuild3.log`.
- **Built, not in a build:** all of the getting up (work branch, switch off); Luis has not seen
  option 1 move. **Not tested as played since its last changes:** the try count's mend in the
  game's test (`32f1e2c`).
- **Drifting or forgotten:** the suite on the work branch, not run since October 14 while
  OwnBody, OwnKeeper, OwnRiser and PhysicalFall changed: run it now, on a quiet machine,
  before the teaching resumes. Long and Round (no getting up). The work branch drifting from
  the game branch. The old way after 12 tries in the game (Luis: the absolute last resort).
- **Does the task serve the stage?** Yes, but its pace is set by hours of teaching; keep the
  machine teaching when nothing else needs it, and measure in the game as soon as the poses
  have clearly shortened.

### October 27, 2026 — the eleventh (250 steps of work since the tenth)

- **Where we are.** S3b step 2, option 1. `full3` (taught on falls and on tries begun
  where a failed one left it) at 2,000,000 decisions, in the teaching without a trainer:
  fresh falls stand 31%, those retries 12% (at 1,000,000: 30% and 10%; the body alone
  6 to 7% and 2 to 3%). The teaching's reward has stayed level since about 1,000,000,
  so more of the same teaching gives little more. In the game, at 1,000,000: up by its
  own body after 15 of 23 pushes (the body alone 7 of 20); 2,000,000 is being tried
  there now. Judged once (round 1 of 3): reads as real, with a flaw. The flaw is mostly
  the searched ways' fixed stage times, which more teaching does not touch.
- **Waiting on Luis:** the question in the last report (let the learner choose when a
  stage it controls moves on, a new network taught from the start, or keep the stage
  times). Still open from before: S3 (since October 8); the builds of October 13 and
  14; R1 (the arms); the inference package's analytics define; the stray
  `risebuild3.log`.
- **Built, not in a build:** all of the getting up (work branch, behind the switch,
  off). Luis has not seen option 1 move. Before he does: round 2 of the judges, and a
  build he can play with the switch named in the report's first line.
- **Drifting or forgotten:** the suite on the work branch (not since October 14; it
  needs the teaching stopped, nothing heavy beside it); Long and Round (no getting up);
  the work branch drifting from the game branch; the old way still waits in the game
  after 12 tries in one fall, and the game's test did reach that (16 tries in 300 s,
  pushed sideways), so its comment "no measure has reached" is out of date. Luis allows
  it only as the absolute last resort; whether it showed there is to be read. Also the
  bench against the teaching for the body alone, the arm the model throws, the coat's
  fin, the let-go body lifting a limb while lying.
- **Does the task serve the stage?** Yes: the measure is the game, and it is being
  measured there. But the teaching has levelled off; the next real step is either
  Luis's pacing answer or work on where tries are lost (sideways falls, retries),
  not more hours of the same run.

### October 27, 2026 — the tenth (250 steps of work since the ninth)

- **Where we are.** S3b step 2, the getting up by its own body, option 1 (Luis, October
  26: what learns sets every joint at the hard moment). Measured on falls as in the game
  without a trainer, the first run's model stands about twice as often as the body
  alone (94 and 74 of 400 against 37 and 38). Two faults in how it was taught were
  found and mended on the way (the body made its own between tries; a silent learner
  holding the hips down in the roll-up, which had also made every "body alone" in the
  teaching since October 23 look worse than it is). `full2` is taught without them.
- **Waiting on Luis:** nothing new asked of him. Still open from before: S3 (since
  October 8); the builds of October 13 and 14; R1 (the arms); the inference package's
  analytics define in the project settings; the stray `risebuild3.log`.
- **Built, not in a build:** all of the getting up (work branch, behind the switch,
  off). The game has not been tried with an option 1 model; that, pictures and the
  aesthetic judges come before anything goes to Luis.
- **Drifting or forgotten:** `CURRENT_STATE.md` still says October 22 (brought up to
  date with this outlook); why the bench's body alone stands more often than the
  teaching's on falls made the same way (a quarter against a tenth); bodies flying
  apart now and then; the suite on the work branch (not since October 14); Long and
  Round; the work branch drifting from the game branch.
- **Does the task serve the stage?** Yes, if it ends in the game: the measure is
  Small getting up by its own body after real pushes. The teaching's numbers only
  point the way; the next proof must be the game.

### October 26, 2026 — the ninth (after three days away)

- **Where we are.** As the eighth, October 23: the third path (a learned correction
  for Small's getting up). From neat front starts it doubled standing on the bench;
  from falls as in the game (where Small alone stands 4% of tries) the fourth run
  reached 15 to 19% and then sagged, stopped at 650,000 decisions when the laptop
  went off. In the game, up at two pushes of six (October 23, the front-taught model).
- **Luis, October 26:** "Please proceed." Nothing else has changed.
- **Next in hand:** find where tries from falls are lost (a bench run of falls
  crashed in the editor while the teaching ran beside it, October 23: to be run
  alone), and give the learning what it needs there.
- **Still drifting:** bodies flying apart (about 1 in 70 to 150 tries); the suite on
  the work branch (not since October 14); Long and Round; the analytics define and
  the stray log for Luis.

### October 23, 2026 — the eighth (250 steps of work since the last)

- **Where we are.** S3b step 2, the getting up by its own body, now on the third
  path (a learned correction; Luis approved it and the downloads, October 22).
  The learning works end to end (twelve copies of a build teach together; a model
  runs on the bench and in the game). Taught from neat front starts it doubled
  standing on the bench (35 of 72; without it 13 to 26) and reads as the body's
  own in pictures; but real falls are far harder (without it, 4% stand from
  game-like falls), and in the game it got up at two pushes of six. Now taught
  from falls as in the game (15 to 19% so far).
- **Done since the seventh:** the side sessions' work merged into the game branch
  (the whole suite passes); the hands let go only over the boots and the rise is
  legs first; the trainer's time scale that ran the breath 20 times as fast,
  found and mended.
- **Waiting on Luis:** S3 (since October 8); the builds of October 13 and 14; R1
  (the arms); whether the inference package's analytics define
  (`SENTIS_ANALYTICS_ENABLED`) may stay in the project settings; a stray build log
  at the shell's root (`risebuild3.log`) for him to delete.
- **Drifting or forgotten:** bodies flying apart (1 in 70 to 150 tries, not looked
  into; in the game an error); the whole suite not run on the work branch since
  October 14; Long and Round have no getting up of their own; the roll's judges
  (round 2 of 3) and the getting up's holds and pace; the work branch and the game
  branch drift further apart (the getting up will need merging by hand).
- **Does the task serve the stage?** Yes: real weight and strength at work need a
  body that gets itself up from wherever it fell. But the measure that counts is
  now the game's falls, not the bench's front starts.

### October 22, 2026 (evening) — a short one, before a choice of approach

- Luis asked, before any approach is chosen, for research of every option that
  keeps the body wholly responsible; it is done ([the report](Research/reports/Getting%20up%20by%20its%20own%20joints.md)).
  Nothing is decided until he has read it. The side branches (satchel, hung things)
  are finished and unmerged; merging them is his. The suite is still not run since
  October 14. Nothing else has moved since the sixth, this morning.

### October 22, 2026 — the sixth (after the context was compacted)

- **Where we are.** S3b step 2, the getting up by its own body. Since the fifth:
  the bear way (pike, then roll up over the feet) stands Small up at 12 and 8 of
  36 chance starts with the real boot turn; no body flew apart in 216 tries; the
  judges' round 3 (the last) is done. In the game behind a switch (off), Small
  tries by its own body when it lies on its front, again if it goes down, four
  times, then the old way. Pushed over in the game it **lands on its back or
  side**, where it knows no way, so every push ended the old way.
- **Luis, October 22:** never the backup animation; extra tries rather than any
  help from outside the body, which is the absolute last resort. So the next
  stretch is: **a way up from the back and the sides** (rolling onto its front
  first), then **trying again without a cap**, with more effort through its own
  joints from the third try; then the judges' holds and pace (13 s; G6 asks
  three to six), lying not dead (G1), Long and Round.
- **Waiting on Luis:** S3 (since October 8); the builds of October 13 and 14;
  R1; merging the satchel branch (unchanged since October 15, two tasks unrun).
- **Drifting:** the whole suite not run since October 14 (eight days); the
  current state's header still says October 15; the getting up is Small's
  alone, and the stage's three miners must all have it.
- **Does the task serve the stage?** Yes: real weight and strength at work need
  a body that gets itself up by its own limbs wherever it fell; the roll is the
  piece that makes the game's falls reach the way Small already has.

### October 20, 2026 — the fifth (after five days away)

- **Where we are.** S3b step 2. Since the fourth: breath after effort (Luis's
  "Sure") made one breath for one body; the bench's frames keep the physics'
  time; for Small the half-kneel failed in every form, and the toddler way (both
  feet forward, a squat, up) **stood Small up by its own body at 7 and 4 of 36
  chance starts**. A search with the game's frames finished on October 15 (8 to
  10 of its 18 starts stand); not yet counted at unseen starts. Reported to Luis
  on October 15; his only word since: "Try again" (October 20, as the session
  came back).
- **Nothing moved elsewhere:** the satchel's branch is as it was on October 15
  (its two further tasks not run); `main` and the game's branch unchanged but for
  my docs.
- **Waiting on Luis:** S3 (since October 8); the builds of October 13 and 14;
  R1; whether Small keeps the toddler way (told October 15, his to overrule).
- **Put off, not forgotten:** too slow (about 13 s to standing; G6 asks three to
  six); lying not dead (G1); the judges' round 3 (the last for this movement);
  Long and Round on the new getting up (the half-kneel meant for Long); straight
  arms; the suite not run since October 14; merging the satchel branch.
- **Does the task serve the stage?** Yes. The getting up now exists end to end
  for Small, rarely; the next stretch is making it hold more often and quicker,
  then the judges, then Long and Round, before anything goes into a build.
- **What Luis would want next:** the whole get-up holding at most places, in
  pictures, judged; then in a build behind a switch.

### October 15, 2026 (evening) — the fourth (the third was the satchel session's, on its own branch)

- **Where we are.** S3b step 2, the getting up. Done since the second: on the
  bench, Small comes onto hands and knees as a child does (shins down, boots on
  their tops) and brings one foot forward with its weight off that knee first:
  31 and 34 of 36 chance starts with the game's frames. The judges (round 2 of
  3): "reads as real, with a flaw". Reported to Luis; he answered (October 15,
  evening): "Sure" (breath shows more after a fall and effort, then eases),
  "you can proceed". **Next in hand:** rising over the front foot, which needs
  designing anew (the poses of October 14 bow it onto its face).
- **Two lines of work now.** The satchel is mended in part on its own branch
  (`claude/satchel-stop`, pushed): the body stops what hangs however it is
  turned. Its code touches `MinerBody.cs` and tests only; its docs touch the
  same pages as mine (this log, the current state, validation), so bringing it
  into the game's branch needs those merged by hand. Merging is Luis's call; it
  offered two more tasks of its own (the satchel lying flat; the lantern, mug,
  hammer and the ground).
- **Waiting on Luis:** S3 (since October 8); the builds of October 13 and 14
  (no word on the arms, tiredness and breaths yet); question R1.
- **Put off, not forgotten:** a weight shift a person can see before a lift;
  straight arms; lying not dead (G1); from the back and sides; Long and Round on
  the new getting up; toes on the standing body; bodies flying apart on the
  bench (none seen tonight, cause never found); the suite not run since October
  14; the twelve known failures as played.
- **Does the task serve the stage?** Yes, and it is moving: three faults found
  and mended in a night, the first half robust. Watch the time it takes (G6):
  6.8 s to one foot forward is too slow, and the rise is still to come.
- **What Luis would want next:** the getting up whole, to standing, even
  imperfect, in pictures; then in a build, behind a switch, when it is safe.

### October 21, 2026 — from a side session (hung things), closing it

- **Where we are.** S3b step 2, the getting up, moved on the work branch
  (`77712a1`: with the boot turning as a foot does, 6 and 10 of 36 stand). The
  satchel's second branch (`claude/satchel-lies`, `b738126`) is committed with
  its suite and docs. This branch (`claude/hung-things-stop`) closes: what hangs
  lies on the ground and is held there, goes out along it when lain on (but
  Round's hammer), and is kept off the thigh along its whole line; suite 207,
  178 passed, 0 failed; standing and walking unchanged.
- **Waiting on Luis:** S3, the builds of October 13 and 14, question R1; and
  three branches of small mends to what hangs (satchel stop, satchel lies, hung
  things), none merged or seen.
- **Put off, not forgotten:** the three branches touch the same functions of
  `MinerBody` and share two field names with different meanings: merging them
  is a piece of work, to be read as meaning. Round's hammer lain on; the
  lantern's lean at rest; the stop's one-frame flick; the skirt's flaps guess
  the ground from the feet (so a lantern's hook can sit under the ground).
- **Does the task serve the stage?** It grew: the ground's hold and the jumps
  were found only by counting after the judges. Enough: what is left is
  either older than this branch or needs the body's trunk in the stop, which
  is a choice for Luis, not a mend.
- **What Luis would want to know:** pictures before and after (one folder);
  what still clips, plainly; that merging the three branches is the next
  step before any of it is in a build.

### October 20, 2026 — from a side session (hung things)

- **Where we are.** Nothing has been committed on any branch since October
  15 evening: the getting up (work branch `dd30219`, Small up at 7 and 4 of
  36) and its showcase (`6d6e786`) are where they were. This session was
  asked (by its starting session, on Luis's behalf) to mend what it found:
  the lantern, the mug, the hammer and the satchel go into the ground, the
  lantern and the mug into the coat when lain on, the hammer through a thigh.
- **Waiting on Luis:** S3 (since October 8), the builds of October 13 and
  14, question R1; and now the satchel's stop and this finding, neither
  merged nor seen.
- **At risk of being lost:** the sibling session on the satchel lying on the
  body stopped with its work uncommitted in its worktree (`elated-fermat`:
  `MinerBody`, `ClothesDepth`, `HungThingBench`, and an outlook entry of
  October 15 evening). It is not this session's to commit; Luis or that
  session should.
- **Does the task serve the stage?** Yes, by Luis's rule that what hangs is
  stopped by the body; but the last outlook's warning holds: no cloth
  simulation built case by case. Two stops (the ground; the thigh against
  the whole thing), standing and walking unchanged, what still clips said.
- **What Luis would want to know:** pictures before and after; that walking
  and standing are measured unchanged; that three branches of small mends
  (satchel stop, satchel lies, hung things) wait on one look from him.

### October 20, 2026 — from the satchel's second side session, resumed

- **Nothing moved since October 15** on any branch, and no message from Luis
  since his of October 15 (evening), which started this task himself ("I also
  clicked the button suggesting to fix the satchel"). The game's branch is at
  Small standing up the toddler way (7 and 4 of 36), on the bench.
- **This task:** half done (below). Keep it to the satchel; the lantern, the
  mug, the hammer and the ground are the sibling session's, found and not
  mended, waiting for a go-ahead.
- **Waiting on Luis**, as on October 15; and now two satchel branches
  (`claude/satchel-stop`, this one on it) and the sibling's, none merged.

### October 15, 2026 (evening) — from the satchel's second side session

- **Where we are.** S3b step 2, the getting up, goes on in its own session on
  the work branch (now `dd30219`: Small stands up at 7 and 4 of 36 chance
  starts, the "toddler way"). This side session finishes the satchel through
  the coat (`claude/satchel-lies`, from `claude/satchel-stop`): the round 1
  judges' two findings (the coat across the bag's top on hands and knees; the
  bag lost behind the thigh as a foot comes forward). A sibling session looks
  at the lantern, the mug and the hammer (`claude/hung-things-stop`).
- **Waiting on Luis:** S3 (since October 8), the builds of October 13 and 14,
  question R1. No message from Luis since October 14 is in this branch.
- **Built but not in a build:** the satchel's stop (not merged), this mend,
  the getting up's balance (bench).
- **Put off, not forgotten:** the bag pressed into the ground when the body
  lies on it; the satchel's strap ends turning into the hips; the coat's
  skirt is four rigid boards, which every hung thing must be stopped around.
- **Does the task serve the stage?** Yes, but it must stay small: one bag, on
  the movement being built, by Luis's rule. The risk is building a cloth
  simulation around rigid flaps, case by case. Keep to what the bag rests on
  and where its rings ride; say plainly what still clips, and stop.
- **What Luis would want to know:** before and after pictures from three
  sides; that walking and standing are unchanged; what is still wrong.

### October 15, 2026 (afternoon) — the third, from a side session

- **Where we are.** S3b step 2, the getting up, goes on in its own session on
  the work branch. This session was given one fault from that round's judges
  (finding 4): the satchel passing through the coat when the body is bowed
  low. It is worked on its own branch, from `claude/worker-showcase`, and
  touches only how a hung thing is stopped (`MinerBody`), not the getting up.
- **Waiting on Luis** (as far as this branch shows): S3 (since October 8),
  the builds of October 13 and 14, question R1. This session has seen no
  message from Luis since October 14.
- **Built but not in a build:** this fix, when it is done; and the getting
  up's balance (bench, the other session).
- **Put off, not forgotten** (found on the way, not asked for): Small's
  lantern, Long's mug and Round's hammer also read inside the coat when the
  body is bowed or lying, before as after; what hangs goes into the ground
  when the body lies on it; the satchel's strap ends turn into the hips when
  it swings out (it is turned rigidly about its rings). Each to be told to
  Luis and proposed, not done unasked.
- **Does the task serve the stage?** Yes: the rule that what hangs is
  stopped by the body is Luis's, and the judges saw it fail on the very
  movement being built. It must stay the size it was asked: the satchel,
  however the body is turned, with standing and walking as they were.
- **What Luis would want to know:** before and after pictures of the
  bowed miner; that walking and standing are measured unchanged; what still
  clips, plainly.

### October 15, 2026 (after midnight) — the second

- **Where we are.** Still S3b step 2, the getting up, on the bench only. The
  stretch in hand is one foot forward from hands and knees. Tonight: the
  weight never went off the knee before it lifted. My own change of the night
  before caused it (a knee that still bore its load counted as one of "the
  others"). Mended; and the hold now learns the steady gap its pose leaves.
  Searched again and counted at 36 starts it had not seen, with the game's
  frames: the weight goes off before the knee lifts at 19 (6 before); the
  miner is still up at the end at 14 (8 before). The kick of the leg behind
  it, which the judges saw, was then found to be the leg's reach itself:
  the way the knee bends turns over once the foot goes ahead of the hip.
  Being tested.
- **Not to hide:** on October 14 (afternoon) one foot forward held at **25
  of 36**, with an older way and before the mends that followed the judges
  (the kick, breath, the wander, the knees drawn at a boot's height). The
  way searched after them holds at 8 to 19. The current-state page still
  says 25; it must say what it is now.
- **Waiting on Luis:** S3 (since October 8), the three builds of October 13,
  `WindowsOrdinaryPlace_Oct14` (arms, tiredness, breaths), question R1. No
  message from Luis since "Okay sounds good" (October 14, late).
- **Built but not in a build:** only the getting up's balance (bench).
- **Put off, not forgotten:** bodies flying apart on the bench (cause not
  found); toes on the standing body; lying, not dead (G1, what Luis asked
  first); from the back and the sides; Long and Round on the balance; the
  step's getting ready for Long; the twelve known failures as played; S4's
  open questions Q2 to Q6. Steps 4 (walking), 5, 6 and "the fall, again"
  not begun.
- **Does the task serve the stage?** Yes, but it has been two days on the
  bench with nothing Luis can see, and nothing yet stands. G6 asks three to
  six seconds to stand; the way so far takes eight to twelve to reach one
  knee. Keep the next stretch short: mend the kick, count it, send the
  judges (round 2, with pictures), then tell Luis plainly where it is, with
  pictures, before building the rise over the front foot.
- **What Luis would want to know:** the fault I made and mended, the kick's
  cause, the honest numbers (25 then, 8 to 19 now), and that nothing stands
  yet.

### October 14, 2026 (late) — the first

- **Where we are.** Stage S3b, the body's own. Step 1 (standing, alive) is
  accepted. Step 3 (a step) is in the game and liked, not accepted; its
  "getting ready" is built but off (wrong for Long). **Step 2 (the getting up)
  is the task in hand**, on the work branch and the bench only: Small and Long
  come onto hands and knees and bring a foot forward with their own balance
  (about three tries in four at chance starts); none stands up from there yet.
  Steps 4 (walking), 5 (down and up), 6 (the work) and "the fall, again" are
  not begun. Then S4 (carrying), S2 (the creator), S5 (players).
- **Waiting on Luis:** S3 (weight and strength at the rock) built, waiting for
  his play since October 8; the three builds of October 13.
- **Built but not in a build** (work branch, since October 13): arms as strong
  as the body needs (Luis's decision), the work measured again (harder, truer
  swings, rests about half as often), tiredness taking precision, breaths
  between blows, toes on the getting-up body, the balance. **Luis has played
  none of it.** The arms change how every miner works; that is his to feel.
  The whole suite has not been run since precision and breaths went in.
- **Put off, not to be forgotten:** bodies on the bench sometimes fly apart
  (cause not found; nothing goes into the game before it is); toes are on the
  getting-up body only (Luis: one body for everything, so the standing body
  gets them too and is tuned again); the "lying, not dead" part of the getting
  up (what Luis asked first) is not begun; from the back and the sides; breath
  on the chest at work waits for steps 4 and 6; the step's "getting ready"
  for Long; one house-light test failing, unexplained; twelve known failures
  as played.
- **Does the task serve the stage?** Yes: the getting up is step 2, and Luis
  called the old one "super weird". But it has been long and bench-only. Two
  risks: the bench drifting from the game (found today: the game's frames
  change results), and Luis seeing nothing for days.
- **So, beside the getting up:** run the whole suite on the work branch and,
  if it is clean, **make a build with what is done and off by default** (the
  arms, the tiredness, the breaths; the getting up stays off), so Luis can play
  the work as it now is. Keep the judges on the getting up (Luis asked), and
  research rising from one knee before building it.
- **Done the same night:** the suite clean (as played, the known failures
  only); `Builds/WindowsOrdinaryPlace_Oct14` made; the judges looked at the
  getting up (round 1); rising from one knee researched.
