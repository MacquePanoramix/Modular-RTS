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
