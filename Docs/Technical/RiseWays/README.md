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
