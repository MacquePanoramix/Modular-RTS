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
