# The judging of movement

How a movement of a body in this project is checked before Luis sees it.
Set up on October 8, 2026, after Luis's playtest of S3
([the message](../Correspondence/2026-10-08_THE_PLAYTEST_NO_FAKE_ANIMATIONS.md)):
"search ways that we can really fully optimize the way to check every
animation, make everything natural, and debug everything. Like one thing
I saw online is like a loop gauntlet or something like that with a bunch
of agents to keep judging each other. I don't know if that makes sense,
but anyways, you can search what makes sense for our project and
implement it."

What was searched, and what was found, is in
[the research](../Research/2026-10-08_RealBodiesAndHowToJudgeThem.md#11-ways-of-checking).
This page is what was made of it. It is a way of working, **Implemented,
not Locked**: Luis may change any part of it.

## Why it was needed

Until this round a movement was checked by whether it *worked*: the
miner picked the pickaxe up, stood again, did not fall, and the figures
were sane. Every one of Luis's ten notes passes that check. A walk in
which the hips dropped five to thirteen centimetres in a single frame at
every step passed it for weeks, because nothing asked how the hips
moved from one frame to the next, and because I looked at my own clips
as I meant them.

So three things are checked now that were not, in this order.

## 1. The numbers: breaks

**What it is.** A movement is traced: where the body's place, its hips,
chest and head and each foot are at every frame, as they are shown
(after the body is posed), with each foot's own state. Then the trace is
read for *breaks*: places where a part changes its motion between two
frames by more than a body can.

**Why first.** A break is a fact. It needs no opinion, it cannot be
argued with, and it is found in seconds. The stutter Luis saw in the walk
and the legs that "teleport" in a turn were both breaks, and both were
found the first time the trace was read.

**The frame rate.** Traces are taken at **100 frames a second**, about
what Luis plays at (92 to 106 in the screenshots), with the physics at
its own 50 steps. A movement that is smooth at 50 can stutter at 100.

| What is measured | The limit (a first setting) | What it catches |
|---|---|---|
| Hips and head: how much the change of place from one frame to the next itself changes | 4 mm | A jolt: being set down, pushed, snapped |
| A planted foot, lying flat: how far it moves over the ground in a frame | 1 mm | Sliding |
| A planted foot: how far it turns on the ground in a frame | 1 degree | Spinning on the spot |
| A landing foot: how much more it moves in the frame it lands than in the one before, or how far aside | 6 mm | Being put somewhere it was not going |
| A foot leaving the ground, and in the air: the same break as for the hips | 12 mm | Popping up; jumping in mid-air |
| Turning: when the head, chest, hips and body have each gone a tenth and nine tenths of the way round | read, not limited yet | A body that turns all of a piece |
| The rise and fall of the hips over the ground in a step | read, not limited yet | A walk with no weight in it, or too much |

A break of 4 mm between frames a hundredth of a second apart is an
acceleration of four times gravity: nothing does that to a body but a
blow.

**Where it lives.**

- `Assets/_WonderGather/Tests/PlayMode/MotionTraceBench.cs` takes the
  traces: each miner walking seven metres, turning round from standing,
  and being sent back the way it came in mid-stride. It writes one CSV a
  movement, and pictures of every frame if asked.
- `Art/Review/motion_breaks.py` reads them, prints what it finds for
  each, says PASS or FAIL, and can draw the heights of hips and feet as
  lines over time.

```bash
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MotionTraceBench -traceOut Captures/Trace
```

```bash
python Art/Review/motion_breaks.py Captures/Trace/*.csv --plot Captures/Trace/heights.png
```

**What it does not do.** It finds what is *wrong*; it cannot say a
movement is *right*. A body that glided with no breaks at all would pass,
and read as a ghost. And it reads only what it is given: the trace has
the hips, head and feet, not yet the hands, the tool, or the clothes.

## 2. The judges: people who did not make it

**What it is.** The movement is drawn as a sheet of frames, in order,
at a pace fine enough to see what happens (and as a strip of its
closest moments). The sheet goes to judges that are separate agents:
each starts with nothing but the sheet, what the body was told to do,
the figures from the trace, and what a real body does (from the
research page). None of them made the movement, and none has seen an
earlier round of it.

**Why separate, and why fresh.** What I made I see as I meant it. And a
judge that has already said its piece defends it the next time. The
accounts of such loops that were read agree on both.

**Three judges, each with its own question.** Judges given the same
part to play add little; given different parts they catch different
things.

| Judge | Its question | It is given |
|---|---|---|
| **The body** | Where is the weight, and what is holding it up, at each frame? Could this body be frozen here and stay? What pushes on what to make the next frame happen? | The figures for real bodies (how long, how many steps, which part first) |
| **The animator** | Does anything start or stop at once, move in a straight line, or move all together? Is the big movement prepared? Does what hangs follow and settle? | The table of what the eye sees and what it usually is |
| **The sceptic** | Where is the fake? Find the one frame a person would point at: a part set rather than moved, a thing through another, clothes doing what cloth does not, a body answering nothing it touches | Luis's own words for the faults found so far |

Each returns: **what it saw** (with the frame), **why a person would not
believe it**, and **how sure it is**; and a verdict of *reads as real*,
*reads as real with a named flaw*, or *does not*.

**A referee.** The findings are then checked against the frames and the
trace, and each is kept or struck: a judge can see a fault that is not
there (a foot that looks to slide because the camera turned). Only what
stands is acted on. Where two judges disagree it is said so, not
averaged away.

**The loop.** What stands is put right; the movement is traced and drawn
again; **new** judges, who have not seen it before, look at it. At most
three rounds: the accounts read agree that most of what such a loop
gives comes early, and that going on makes the judges agree with each
other rather than with the world. What is still open after three rounds
is written down and shown to Luis as open.

**Where it lives.** The judges' briefs are in `Art/Review/judges/`
(`body.md`, `animator.md`, `sceptic.md`, `referee.md`). The sheets are
made by the benches' pictures and `Art/Review/sequence_clip.py` and
`select_frames.py`.

**What it costs.** Each judge is a separate agent that starts cold: it
is the dearest way there is to look at a picture. So judges are called
for a movement that has passed the numbers and that I believe is right,
not for one I already know is wrong; and for the movements a player sees
most first.

## 3. Luis

Luis plays it. Nothing above stands for that: the numbers and the
judges exist so that what reaches Luis has had its plain faults found,
and so that Luis's time goes on the things only Luis can say: whether it
*feels* like the people of this world.

What the numbers and the judges said is given to Luis with the clip, the
doubts first.

## What is judged, and where each stands

Kept up to date in [the round's page](../Reviews/2026-10-08_ThePlaytestRound.md).

| Movement | Numbers | Judges | Luis |
|---|---|---|---|
| Walking | **Passes** (all three miners) | Called once; cut off before they answered | Not yet |
| Turning round on the spot | Fails on two to four frames | Called once; cut off before they answered | Not yet |
| Sent back the way it came, walking | Fails (a foot jumps once for Long and for Round) | Not yet | Not yet |
| Going down for a pickaxe, and standing up with it | Not traced yet (the trace has no hands or tool) | Not yet | Not yet |
| The blow, and the rest from it | Efforts of each muscle group measured over five minutes | Not yet | Not yet |
| Falling, and getting up | Not traced yet | Not yet | Not yet |
| Taking the lantern or the mug, and holding it | Not traced yet | Not yet | Not yet |
| Clothes in the deepest bend | Not measured yet | Not yet | Not yet |

## What was learnt setting it up

- **Six judges at once is more than the account bears.** The first round
  (three for the walk, three for the turn, each a separate agent on the
  largest model, each reading four to eight sheets) ran into the
  account's usage limit while they were starting, and all six were cut
  off with nothing said. Judges are to be called fewer at a time, on a
  smaller model (which is also a different pair of eyes from the one
  that made the movement), and only after the work in hand is committed.

- **The trace found in a minute what weeks of clips had not.** Read
  frame by frame at 100 a second, the hips' height in a walk was a
  sawtooth. It had been a sawtooth since the walk was made.
- **A measure has to be of the right thing.** The first measure of a
  planted foot's sliding failed every walk: it measured the foot's own
  place, which moves when the heel rises before the foot leaves the
  ground. A foot is held to the ground only while it lies flat.
- **The slope matters.** The miners begin on a slope. Going down it the
  faults were three times the size they are on the flat. A bench on flat
  ground would have shown a third of the truth.
