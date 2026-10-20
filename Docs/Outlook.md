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
