# Every frame of the getting up, as played — October 30, 2026

**Why:** Luis played the build of October 30 (`pace10`, the switch "Gets up by its own body" on) and saw what three
rounds of judges had not: "very aggressive twitches with the limbs", arms bent so they "seem like it's broken", and a limb
that would "twitch like for one millisecond completely out of frame". He asked for "much more visual checks for all
the what's happening in the game of every moment ... from all angles" ([the message](../Correspondence/2026-10-30_THE_GETTING_UP_FOR_THE_WHOLE_GAME.md)).
The judges had seen the bench, every 0.08 s, from one side: a fault that lasts a frame cannot be in such pictures.

## How it was read

- **The game as played**, work branch, `pace10`'s models (as in his build): the game's own test
  (`GettingUpByItsOwnTests`, `-ownAsPlayed 1`), 8 pushes from four sides, **at 120 frames a second** (a screen faster
  than the physics, as his is: 2.4 frames to each step of the physics, where the tests had one).
- **Every rendered frame written** (`Tests/PlayMode/FrameRecorder.cs`, `-ownFrames`): the model's bones as drawn, the
  segments they are drawn from, and the physics (each part, how fast it turns, its joint), after the bones are turned
  and before the frame is drawn. 134,085 frames, about 18 minutes of play. Read by a scan of every frame
  (`framescan.py`, the session's scratchpad): drawn limbs turning faster than the physics under them, hands and feet
  jumping and coming back, elbows and knees bent the way they do not bend or folded past their stop, shoulders and hips
  where a person's cannot go, parts turning faster than a person's do.
- **Pictures as faults happen**, from four sides (two at its height, one above, one close), the frame the fault is
  seen in and the nine after, the meadow's grass left out of these pictures only (it hid a body lying in it). Runs do
  not repeat frame for frame (a second run differed from its second frame), so a fault found afterwards cannot be
  pictured again: the recorder looks for faults as it goes (`-ownShotFaults 1`).

## What was found

| | Fault | How often, how large | Cause | Now |
|---|---|---|---|---|
| 1 | **Drawn limbs turned half round in a frame** (an arm or leg spinning about its own length: the "twitch" and the "broken" look of the sleeve, the hand) | 1,036 times in 18 minutes, up to 176 degrees in one frame; 543 of them with the limb within 20 degrees of straight | Drawing, not physics: `MinerBody` worked out each limb's turn about its length from where its elbow or knee sits off the limb's line. A limb straight (an arm pushing on the ground) or folded double has no such side, and the turn jumped | **Mended** (work branch, `MinerBody.TurnsAsSolved`): each bone of a physical body's limb is drawn turned as its own part is turned (the side its joint sticks out is part of how each part is made, at any bend). Over a run of the same length: **1,036 spins to 40**, spread thin and most likely real quick turns. Drawn at ease standing by its own joints, the same frames drawn the old way and the new differ by 0.1 to 0.5 degrees (never more than 1.5): the look Luis accepted is unchanged |
| 2 | **Joints bent as no living joint bends** | Elbows the wrong way in 1 to 3% of the getting up (most by 8 to 17 degrees, once 70); knees folded past 150 and up to 180; hips 89 degrees behind; shoulders turned half round | The body that gets up (`OwnBody`, articulations) has **no joint limits at all**: every joint turns freely, held only by its motors, which give way under the body's weight | Not mended: the first piece of the new body ([the plan](#what-follows)) |
| 3 | **Whipping parts** | Boots spinning at up to 115 to 154 rad/s; forearms past 12 rad/s for about 3,000 frames, a few steps at a time. A person getting up rarely turns a limb faster than about 5 | Joint motors far stronger and quicker than a person's on light parts (the boots), with targets that jump from pose to pose | Not mended: the new body's joints |
| 4 | **The body blew apart once** (in another run): its right boot spun up to 154 rad/s, then its shin, then all of it, flung 10^17 metres, then nothing | Once in about four runs of 8 pushes | The same as 3: a light part with a strong motor and nothing to stop it, pumping | Not mended: the new body's joints |
| 5 | **A whole-body flick** of half a metre for one frame | Once | Not the game: the test itself takes the body back standing after 300 s without getting up | — |
| 6 | **Hands and feet jumping** 4 to 12 cm for 1 to 3 frames | About 45 | Mostly the whips (3) seen from outside | With 3 |
| 7 | **Slow**: tries that fail, one after another | 62 tries for 8 pushes; most pushes needed several, several took 2 to 5 minutes | How the getting up is made (the ways, the learnt corrections) and 2 and 3 | The new way of getting up |
| 8 | **The first try rolls past its front**: lying face down it pushes with an arm, rolls on over onto its back, legs up and arms waving, and fails | Seen in the pictures of the first push | The roll's poses and their timing | The new way of getting up |

## The pictures

Each row a frame, 1/120 s apart; its four sides left to right: two at its height, from above, close.

- [Drawn arm turning half round](../Images/GettingUp_2026-10-30/every_frame/drawn_arm_turning_half_round.jpg)
  (lying on its side, the right arm straight on the ground: the sleeve turns 148 degrees from one frame to the next)
- [Knee folded past its stop](../Images/GettingUp_2026-10-30/every_frame/knee_folded_past_its_stop.jpg) (171 degrees)
- [Knee bent the wrong way, rising](../Images/GettingUp_2026-10-30/every_frame/knee_bent_the_wrong_way_rising.jpg)
  (48 degrees, falling back from the rise)
- [Elbow bent the wrong way](../Images/GettingUp_2026-10-30/every_frame/elbow_bent_the_wrong_way.jpg) (17 degrees,
  hands and feet on the ground)
- [Boot whipping](../Images/GettingUp_2026-10-30/every_frame/boot_whipping.jpg) (105 rad/s)
- [The first try, every 0.1 s](../Images/GettingUp_2026-10-30/every_frame/first_try_rolls_past_its_front.jpg) (rolling
  past its front onto its back)

The pictures are small and these faults are clearest in the numbers; seen in motion they are what Luis described.

## What follows

Fault 1 is mended on the work branch for every physical body (standing by its own joints, let go, getting up). Faults 2
to 4 belong to the body itself, and 7 and 8 to how it gets up: both are what Luis has asked to be made again for the
whole game, for any body (the research and the plan, [Research/reports](../Research/reports/)). Whatever that plan is, its
body has a living body's joint stops, motors no stronger or quicker than its build allows, and is read every frame as
played before it is shown.

**Not tested:** Luis's own screen rate (120 a second was used); the build itself (the editor's test, as played); Long and
Round (only Small gets up by its own body); the posed body's drawing (walking, working) with the drawing change, beyond
the frames of these runs; the whole PlayMode suite with it (to be run before it is committed).
