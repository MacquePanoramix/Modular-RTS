# Ways for the getting up (OwnRiseBench), as of October 14, 2026 (evening)

Data for `OwnRiseBench` (`-riseWays <file>`), kept so the work can go on from here. Not used by the game.

- `ways_hold_small.txt`: Small, from its front onto hands and knees, then held there (the balance: what bears it kept
  where it is, its hips held). Found by the bench's search with `-riseGoal hold` at six places; held at 7 of 9 places.
- `ways_a1.txt`: the same, then its hips raised and one foot brought forward (its weight off that leg first). Being
  studied when work stopped.
- `ways_squat_small.txt`: both feet forward to a squat with hands down. Searched, but the search's measure was
  fooled (lying on its back counted as held); with the measure mended it held at 0 of 9 places. Not good.
- `ways_bal0.txt`, `ways_q5.txt`, `ways_kneel0.txt`: hand-set ways tried on the way.
- `keep_front_*.txt`: each miner's way found before the arms were made as strong as the body needs (one place only).
- `failwhen.py`: reads a bench log with `-riseTick 1` and tells when each try first lost its balance.

Bench arguments used: `-riseMiner Small -riseStarts front,front@1,...,front@8 -riseFrom flat -riseFor 12
-riseBalPushes 0 -riseTick 1`; searches add `-riseGoal hold -riseSearch feet -riseFirst N -riseRounds 40 -riseMany 24`.

## October 15, 2026

- `ways_laid_small.txt`: Small, from its front onto hands and knees with its shins on the
  ground and its boots laid on their tops (`kneel=0.50 laid`), then one foot forward, its
  weight off that knee first. Searched from its hands on (40 rounds, chance starts and
  pushes, `-riseHoldHurry 0.4`). Use with `-riseSet BalanceHoldLearns=1.734,
  BalanceMargin=0.021,KneeFaces=0.759,BalanceShiftsAt=0.114,BalanceShiftsAtMost=0.072,
  BalanceUnloads=0.362,LetsGo=0.250,FoldedUnder=0.163,BalanceShiftsNearest=1,
  BalanceShiftsDeep=0.040`. At `starts36.txt` with `-riseFrames 1`: up at the end 31 of 36
  (`-riseSeed 21`), 34 (`-riseSeed 9`); 28 without frames. Its weight off the knee first in 35.
- `ways_folded_small.txt`: the way before it (its legs folded tight on hands and knees,
  its boots in the air), searched before the leg's reach was made sound.
- `shiftsum.py`: per try, whether its weight was moved off the leg before it was lifted.
  `liftkick.py`: per try, how far the lifted leg's thigh was from what it was asked just
  after the lift. `stance.py`: how it stands on hands and knees (heights, knees bent, what
  bears it). `holdlook.py`: a short line per tick (what bears it, hips asked and gone).
- `shiftgone.py`: how far its hips went before the limb was lifted.
- `ways_toddler_small.txt` (October 20): Small, the toddler way to standing: onto hands and
  knees, one foot forward, the other, back into a squat with its hands let go, handed to its
  standing balance (soles at least 0.75 flat, its weight within 0.25 of them). Use with
  `-riseSet BalanceHoldLearns=1.395,BalanceMargin=0.021,KneeFaces=0.759,BalanceShiftsAt=0.132,
  BalanceShiftsAtMost=0.065,BalanceUnloads=0.487,LetsGo=0.379,FoldedUnder=0.163,
  BalanceShiftsNearest=1,BalanceShiftsDeep=0.040`. At `starts36.txt` with `-riseFrames 1`:
  stands 9 of 36 (`-riseSeed 21`), 11 (`-riseSeed 9`); most of the rest fall in the squat.
- `ways_bear_small.txt` (October 20): Small, the bear way to standing: onto hands and knees,
  one foot forward, the other, hips up on hands and feet, then its hands let go as its trunk
  rolls up over its feet, handed to its standing balance. Searched from the second foot on
  (30 rounds, with the game's frames). Same `-riseSet` as `ways_toddler_small.txt`. At
  `starts36.txt` with `-riseFrames 1`: **stands 17 of 36 (`-riseSeed 21`) and 17 (`-riseSeed 9`)**;
  on its feet at about 10.4 s, standing at about 13.
- `ways_bear_small.txt` (October 21, replacing October 20's): searched again with the boot
  turning as a foot does (40 rounds, from the first foot on, wider). Use with `-riseSet
  BalanceHoldLearns=1.862,BalanceMargin=0.021,KneeFaces=0.759,BalanceShiftsAt=0.122,
  BalanceShiftsAtMost=0.040,BalanceUnloads=0.429,LetsGo=0.357,FoldedUnder=0.163,
  BalanceShiftsNearest=1,BalanceShiftsDeep=0.040`. At `starts36.txt` with `-riseFrames 1`:
  stands 12 of 36 (`-riseSeed 21`), 8 (`-riseSeed 9`); standing at about 13.8 s; no body went
  wild (October 20's, with the whipping boot: 17 and 17).
- `ways_roll_small.txt` (October 22): Small, a roll from its back or side onto its front
  (`way roll -1 0.4 turns`: its uppermost knee up, then over and across as that foot pushes, its
  uppermost arm across, its trunk twisting that side forward; then lying on its front, still),
  followed by the bear way of October 21. The roll searched with `-riseSearch roll -riseGoal turn
  -riseSame 0` (both sides free, the twist free) from 12 starts on its back and sides, 30 rounds.
  With the defaults of October 22 (the riser's numbers as searched on October 21), at `backs36.txt`
  and `sides36.txt`, `-riseGoal turn`: **onto its front and still at 29 of 36 from its back, and 13
  of the 17 side starts that did not already lie on their front**; about 2.6 s. Then to standing,
  at `starts36.txt` (front) and `backs36.txt`, `-riseFrames 1 -riseFor 30`: 1 to 8 of 36 and 0 to 2
  of 36 (each count with the same code differs by that much from run to run); most give up rolling
  up from the bottom-up stance.
- `backs36.txt`, `sides36.txt`: 36 starts laid on its back (18 places along the path, then the same
  turned a quarter), and 36 on its sides.
- `-riseGoal turn` (the bench): a try ends as soon as the body goes on by another way than it
  began with, and counts how soon. `-riseSpinTrace 40`: the steps before and after a boot first
  turns faster than that, with each ankle's joint speed and how far its asked turn moved;
  `spinlook.py <log>` shows them. `sumrise.sh <run>`: what a run came to (stands, turned, wild,
  boot whips over 30 and over 100 rad/s, how the tries ended).

## The third path: a learned correction (October 22; Luis: "I approve we try the third path")

What learns the moment from hands and feet to the squat: `Assets/_WonderGather/Learning/RiseAgent.cs` (its own
assembly, `WonderGather.Learning`, with Unity's ML-Agents 4.0.3), on the riser's hook (`OwnRiser.Learned`, six numbers:
the trunk's and head's bow and the twist, how high the hips are held, where the weight is meant to be; `OwnRiser.Sense`,
29 numbers it is told). It acts only in the stages `pike` and `rollup`, and only through the body's own joints.

- **The trainer** (outside the project): `D:/Dev/mlagents-env`, Python 3.10.12 made with `uv`, `mlagents` 1.1.0,
  PyTorch 2.2.2 (CUDA 12.1), `setuptools<70` (the trainer still imports `pkg_resources`).
- **Teaching** (two processes; the trainer first, it listens on port 5004):
  `D:/Dev/mlagents-env/Scripts/mlagents-learn train_smallrise.yaml --run-id=<name> --results-dir=D:/Dev/mlagents-results`,
  then Unity on the work branch's copy, as the bench: `-runTests -testPlatform PlayMode -testFilter
  WonderGather.Tests.RiseTraining -trainWays <ways file> -trainHours 6` (also `-trainTries`, `-trainEvery 5` steps of the
  physics between decisions, `-trainFrames 1`, `-trainFor 25`, `-trainTurn 20 -trainMove 0.3`, `-trainSeed`). A try that
  stands is worth 1, one handed to its standing balance that then falls 0.3, any other 0; each change in what it says
  costs a little (`RiseAgent.Jerk`). The model comes out as `<results>/<name>/SmallRise.onnx`.
- **Without the trainer** (October 22, dry run): the agent says nothing and the body does as searched: 23 of 100 tries
  stood, 31 came to the hand-over, the agent's stages were reached in 86; 100 tries in 36 s.
- **In builds, many at once** (October 23): build the work branch with `Unity.exe -batchmode -quit -projectPath D:/Dev/WG
  -executeMethod WonderGather.Editor.OrdinaryPlaceSetup.BuildWindows -buildOut <name>` (it rewrites EditorBuildSettings
  and ProjectSettings: put them back before committing), then from `D:/Dev`: `mlagents-env/Scripts/mlagents-learn <yaml>
  --run-id=<name> --results-dir=D:/Dev/mlagents-results --env=D:/Dev/WG/Builds/<build>/WonderGather.exe --num-envs=12
  --no-graphics --time-scale=1 --capture-frame-rate=50 --env-args -riseTrain -trainFalls 1 -trainWays <ways file>`. The
  time scale must stay 1: at 20, what runs by the frame (its breath) ran twenty times as fast as the physics.
  `-trainFalls 1`: its starts pushed over as in the game, 72 of them. `--initialize-from=<run>` begins from a run's
  learning. A model is tried on the bench with `-riseLearned <name>` (from `Resources/RiseModels`), and in the game a
  miner with `Resources/RiseModels/<its name>Rise` uses it.

## Option 1: every joint set by what learnt (October 26; Luis: "option 1")

The nudges helped from neat front starts and not from the falls the game gives (7 of 72 with or without them). Now, with
`-trainFull 1`, at the second foot's step, the pike and the roll-up the agent also sets each joint's target itself
(`OwnRiser.Driven`): every joint but the hips' and the head's, so many degrees more about each of its axes than the way
asks (at most 25 for the trunk, 35 for an upper arm or forearm, 30 for a thigh or shin, 20 for a boot; `RiseAgent.Drives*`),
eased in over 0.08 s. It is told each joint's turn and speed and what the way asks of it, and how its hips go and turn
(`OwnRiser.SenseJoints`): 144 numbers in, 33 out. Besides standing (1) it is paid as its hips come higher once its hands
have let go (`RiseAgent.Rises`). Its behaviour is `SmallRiseFull`; a model for it is called `<name>RiseFull` (the bench
and the game take one so called as setting every joint).

**Help only while it learns** (Luis, October 22), by the trainer's lessons (`train_smallrise_full.yaml`, the environment
parameter `help`, from 1 to nothing; `-trainHelp` without a trainer): its joints `0.3 x help` stronger
(`OwnRiser.StrongerWhileLearning`; `-trainStrongerBy`), and its weight drawn towards over its boots by up to `0.15 x help`
of its weight on its hips and trunk (`-trainPullsBy`), in the stages it learns at until it is handed to its standing
balance. A lift of the hips (`-trainLiftsBy`) is left at nothing: rolling up asks the boots to bear 55% of its weight. In
the game it has none of these. Ways: `ways_roll_small_flat.txt` (boots let down flat in the pike and the roll-up).

**Measuring what was learnt on the teaching's own falls** (October 27): a build with the model in
`Resources/RiseModels/`, started without a trainer, `-riseTrain -trainFull 1 -trainFalls 1 -trainTries 400 -trainModel
SmallRiseFull` (its own best answer, as in the game; `-trainDrawn 1`: drawn as when taught), against the same without
`-trainModel` (the agent says nothing: the body as searched), with the same `-trainSeed`. A few hundred tries a copy in
about three minutes; counts of 72 on the bench move by several from run to run.

**Two faults found in the teaching, October 26-27:** (1) between tries the body could be made its own as its posed body
stood, and *how it stands* noted from however the last try laid it (`OwnBody.Make` now begins its count of standing at
ease afresh); copies so struck stood none from then on. (2) With an agent attached, even saying nothing, the hips were
held at the height they were planted at in every stage that does not hold them itself (the second foot's step and the
roll-up): 26 of 400 falls stood so, 37 and 38 without it. Now the agent only moves a height the stage holds. Runs before
`full2` were taught with (2).

**Tries begun where the last left it** (`-trainRetries 0.5`, at most `-trainRetriesAtMost 4` in a row; October 27): after
a try that went down, so often its fall has it (as in the game, `OwnBody.FallsBack`), it lies still, and the next try
begins there. In the game most tries are such: pushed over fifteen times (`GettingUpByItsOwnTests` with `-ownUnheard 1
-ownPushes 20 -ownPushWays 0,180,90,270 -ownAsPlayed 1`), Small alone tried 159 times and stood twice. In the teaching
such tries stood 1 of 158 alone and about 3% with the first run's model, against about 9% and 21% from fresh falls.
Without the hold the help no longer added anything (the body alone 15 and 20 of 400 with all of it, 20 to 23 without),
so from `full3` it is taught without help or lessons: `train_smallrise_full_retries.yaml`.

**Falls learnt by heart** (October 27): the teaching's fresh falls were 72 made once at its start, drawn again and again;
taught on them, `full3` at 2,000,000 stood from them 31 to 33% and from where a failed try left it 12 to 14%, and nothing
in how those lay told why (the place, `-trainRetryLater 2`; what a try keeps, `-trainRetryLater 1`; each joint's turn,
told by `-trainTell 2` as `pose` and `turns`). On falls made anew for each try (`-trainFreshFalls 1`: one of the eighteen
places, facing and pushed any way, 0.7 to 1.4 times as hard, 0.7 to 1.3 times as long, up to 20 cm lower or 25 higher)
it stands 20% (retries 14%), the body alone 5.6% (3.7%). Measure and teach with `-trainFreshFalls 1`.
`-trainPictures <folder>` (with graphics, not `-nographics`) pictures the first `-trainPictured` tries begun on the back,
fresh and retries alike.

## The learner paces its way (October 27; Luis: "Yes I think that approach sounds good")

`-trainPaced 1` (model `<name>RisePaced`, behaviour `SmallRisePaced`, `train_smallrise_paced.yaml`): besides every joint
at the hard moment, it says in every pose of its way how long the pose is kept at least (`OwnRiser.Pace`, from a quarter
to twice the searched time); how fast each pose is come to, and what a pose asks before it is through, stay as searched.
Each second of getting up costs `-trainTimeCost` (0.02). Said nothing, it keeps the searched times. In the game a
`RisePaced` model is taken before a `RiseFull` one; the bench runs one with `-riseLearned <name>RisePaced`. Run:

    mlagents-learn train_smallrise_paced.yaml --run-id=paced2 --env=Builds/RiseTrain14/WonderGather.exe --num-envs=12
      --no-graphics --time-scale=1 --capture-frame-rate=50 --env-args -riseTrain -trainPaced 1 -trainFalls 1
      -trainFreshFalls 1 -trainRetries 0.7 -trainRetriesAtMost 8 -trainNearer 1 -trainWays ways_roll_small_flat.txt

**As two learners** (October 27, afternoon): one network for both (`paced2`) stood no more often than the body alone at
500,000 decisions; its 33 joint outputs were learnt from nothing in every pose, where they do nothing. Now the pace is an
agent of its own (`-trainPace 1`, behaviour `SmallRisePace`, model `<name>RisePace`, on an object "Pace" under the miner)
beside the one that sets the joints (`SmallRiseFull`, begun from `full3` at 2,249,994 by `init_path`); both are given
whether it stood, the pacer also 0.02 a second. `train_smallrise_pace.yaml` begins a run; **to resume one use
`train_smallrise_pace_resume.yaml`** (the trainer begins from `init_path` whenever it is given, resuming or not, which
would set the joints back to `full3`). Saved every 50,000 decisions. Without a trainer (`-trainModel SmallRiseFull`,
`-trainPaceModel <name>` or a silent pacer): with a silent pacer, 20.5% of falls made anew stood, as the joints alone.

    mlagents-learn train_smallrise_pace.yaml --run-id=pace1 --env=Builds/RiseTrain15/WonderGather.exe --num-envs=12
      --no-graphics --time-scale=1 --capture-frame-rate=50 --env-args -riseTrain -trainFull 1 -trainPace 1 -trainFalls 1
      -trainFreshFalls 1 -trainRetries 0.7 -trainRetriesAtMost 8 -trainNearer 1 -trainWays ways_roll_small_flat.txt

**The pace said once as each pose begins** (October 28): asked every tenth of a second and eased in as the joints are,
no one answer of the pacer moved a pose's end, and in `pace1`'s 250,000 decisions the poses shortened by a tenth of a
second each. Now the pacer says once, as each pose begins, how long that pose is kept (`RiseAgent.PacedStage`; its
answer taken as it is, not eased in); each second of getting up since it last said costs it 0.02, charged as it next
says and at the end. In the game it says as each pose begins too. `train_smallrise_pace2.yaml` begins `pace2` from
`pace1` (the joints at 99,996; the pacer new: pace1's, asked once a pose, spread the times and stood 9%), learning the pace from 2,048 at a
time (about eleven a try); resume with `train_smallrise_pace2_resume.yaml`.

**The pacer's seconds only where it stood; the toes reckoned** (October 28): charged on every try, the pacer's seconds
made failing early cheaper than standing late (and in the game a failed try costs another): `pace2`'s pacer at 50,000
stood 11% of fresh falls (19% as searched). Now a try that stands is worth 1 less 0.02 a second it took, a try that
fails nothing. And the standing balance now reckons what the ground bears through the toes where the body has them (the
body that got up; the standing body has none): of tries handed to it, 71% stood, not 59% (21.3% of all tries, not
15.3%). `-trainTell 3` tells what the standing balance says as it rises (`TRAIN_RISE`). `pace4`
(`train_smallrise_pace4.yaml`, `Builds/RiseTrain18`): the joints from `pace2` at 249,951, the pacer new.

**Holds cost the pacer** (October 28; Luis: "Penalise holds"): 0.1 a second the body getting up is slow (no part faster
than 12 cm/s) past 0.25 s at a stretch, only in a try that stood (`-trainHoldCost`, `-trainHoldGrace`); slow movement is
not a hold. The teaching tells the held seconds of every try (`held` in `TRAIN_STAGES`, `-trainTell 2`). With `pace4`'s
joints, falls made anew: unpaced 0.4 s held a standing try (median), `pace4`'s pacer (seconds alone charged) 1.5 s.
`pace5` (`train_smallrise_pace5.yaml`, `Builds/RiseTrain20`): the joints from `pace4` at 199,961, the pacer new, saved
every 20,000; resume with `train_smallrise_pace5_resume.yaml`.

**Where it held, and holds charged as held** (October 29): the teaching now tells in which pose each held second was
(`holds by pose` in `TRAIN_STAGES`). With `pace5`'s joints at 649,997 on falls made anew, about two thirds of the holds
were in `hands`: lying on its front, its arms moved 10 to 20 degrees over 1.4 s, the pose kept 1.58 s. The ways' `hands`
is now half as long and come to in half the time (0.70 s, 0.79 s; in `ways_roll_small_flat.txt` and the game's
`Resources/RiseWays/Small.txt`): held 0.3 s a standing try (median), not 0.7, standing as before (21.2% of fresh
falls, 16.6% of retries, in 28 copies of 200 tries; 21.4% and 16.3% in 9; runs of three copies differ by up to 8 points). Switches to try such times without a new ways file:
`-risePoses name=times,...` (each pose kept so many times as long), `-risePoseOvers` (come to in so many times as
long), and `-riseGoesOnStill <s>` (a pose whose ends are so goes on once the body has been slow in it so long;
`-riseGoesOnAfterMoving 1`: only once it has moved in it). Gone on after 0.25 s slow from a pose's start, 5% stood;
only after moving, 19% (holds 0.52 s). `under` three quarters as long as well halved the holds again (0.14 s), 20.1% and
14.5% standing: not taken. `pace5`'s pacer, charged at the end of a try that stood, at 140,000 still held more than the
searched times (0.9 to 1.0 s) and its answers were nearly as random as at first; `-trainHoldsAsHeld 1` charges each held
second as it is held, in every try. `pace6` (`train_smallrise_pace6.yaml`, `Builds/RiseTrain22`, the halved `hands`):
the joints from `pace5` at 799,994, the pacer from `pace5` at 159,992, learning three times as fast; resume with
`train_smallrise_pace6_resume.yaml`.
A pose not through while the body has been slow in it 3 s now gives the try up (`OwnRiser.StuckAfter`,
`-riseStuckAfter`; 0 as before): a foot that never came to bear held it on hands and knees up to 17.8 s. Given up after 2
or 3 s, the longest held of tries never handed over was 5.5 to 6.5 s, standing not lower (24.0%, 6 copies). `pace7`
(`train_smallrise_pace7.yaml`, `Builds/RiseTrain23`): as `pace6`, with it, begun from `pace6`'s saves.

**The pacer only shortens** (October 29): taught free to keep a pose up to twice as long, every pacer kept `prop` and
`push` longer (`prop` 2.0 to 2.2 s for 1.6), standing no more often and holding more (`pace7` at 40,000: held 0.45 s a
standing try, as searched 0.29). Kept to shortening (`RiseAgent.PacesLongest 1`, `-risePacesLongest`; 2 as before), the
same pacer held 0.18 s and the getting up took about 0.8 s less (under, stay, footB and pike shorter), 20.4% of all
tries standing (20.6% as searched; 6 copies of 200 each way). `pace8` (`train_smallrise_pace8.yaml`,
`Builds/RiseTrain24`): as `pace7`, shortening only, begun from `pace7`'s saves (the joints at 199,900, the pacer at
39,997).

**The game's tries are deep retries** (October 29, before dawn): the game's test (`GettingUpByItsOwnTests`, about 4
minutes a run of 20 pushes now) tells why a rise after getting up did not stand and where (the standing balance's
reasons, the hips' place, the walked ground, the ground's slope). Its hand-overs stood about half the time against the
teaching's 68 to 71%; by try within a push, 57% in tries 1 to 5 and 22% from the sixth on. The teaching given the same
(`-trainRetries 1 -trainRetriesAtMost 12`) does the same: from the sixth try in a row 7% reach the hand-over (33% in the
first two) and 34% of those stand (60%). The measure (`-trainRetries 0.5`) and the teaching (0.7, at most 8) seldom go
that deep. Not the cause: the slope, the house, how it lay, how high it rises to. The teaching and the bench note how
it stands once at their start, 0.696 m (its weight over its soles); the game 0.75 to 0.76: `OwnBody.StoodHeightIs`
(`-riseStoodHeight` in the teaching, `-ownStoodHeight` in the game's test; 0: as noted) sets the height the rise after
getting up aims for, to measure it.
