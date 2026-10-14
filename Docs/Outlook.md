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
