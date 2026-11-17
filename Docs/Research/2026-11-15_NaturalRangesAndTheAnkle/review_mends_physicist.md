# Physicist review of mend 4: the order in which what the model learnt is added to the way's ask

Reviewer: physicist, did not write the mends. Question asked, and only it: mend 4 (AnkleLimit 1 composes the learnt turn d as
`own * FromRotationVector(d)` (b), where the model was taught `FromRotationVector(RotationVector(own) + d)` (a); (c) is
`FromRotationVector(d) * own`).

## Verdict

- The code is (b), as described, and AnkleFolds receives it. wayAsks records the way's ask alone, as Aims does. No finding there.
- (b) and (c) are continuous through 180 degrees; (a), the taught sum, is not (a jump of up to 44 deg of rotation in the box).
- (b) is NOT the taught sum: away from own = 0 it departs from (a) by a median of 0.8 deg (own 0-15) rising to 17.7 deg
  (own 165-180), worst 38 deg. It equals (a) to first order in d only at own = 0. (c) is no nearer: it is exactly as far.
- The numbers do not say which order the model "wants". What would decide it is listed at the end.

## What I ran

Script: `C:/Users/luish/AppData/Local/Temp/claude/e2b675c90a3db94d/86f16be5-fb94-46d4-a96c-f3f3e90cd18a/scratchpad/mend4.py`
(plus `mend4b.py` beside it); output in `mend4.out`. Run with `python -I`. numpy only. Quaternions (x,y,z,w), Hamilton product, which
is Unity's `q1 * q2` (q2 applied first, in q1's frame). `RotationVector` is Unity's ToAngleAxis ([0,360]) folded into (-180,180]
times the axis; `FromRotationVector` is `AngleAxis(|v|, v/|v|)`; both in degrees. "Angle between two turns" = 2 acos |q1.q2|
(sign-blind). Own's axis uniform on the sphere, own's angle uniform on 0..180, d uniform in the box plus or minus 20 per axis
(median |d| 19.7 deg, largest 34.2), N = 400000. I did not test the script on a known-bad case beyond the checks below: (1) at
own = 0 all three agree exactly (a, b, c are the same turn: measured 0); (2) the first-order result (section 3) matches the
analytic Jacobian prediction `(theta/2) * |axis x d|` to 4 digits at small theta; (3) (a) at 180 shows the jump that the
algebra predicts (below). The lazy way to meet "continuous" is d = 0, where all three are trivially continuous; the rows below
use d up to 20 per axis.

## 1. Continuity as own's angle passes 180 degrees

Own = AngleAxis(180 -/+ 0.001 deg, axis); the two samples are 0.002 deg apart as rotations. 8 values of d
(zero, plus or minus 20 on each axis, (20,20,20), (-20,20,-20), (5,-10,15), (10,0,0)), 7 named axes and 200 random axes.

| order | largest jump found (rotation) |
|---|---|
| (a) FromRotationVector(RotationVector(own) + d) | 44.07 deg |
| (b) own * FromRotationVector(d) | 0.0020 deg (= the 0.002 deg own itself moved) |
| (c) FromRotationVector(d) * own | 0.0020 deg |

(a) rows: for own about x, y or z, d = 20 about one other axis gives 25.46 deg; d = (20,20,20) gives 35.89 deg; d about the
own axis gives 0. With d perpendicular to the axis, the jump is 1.273 x |d| (|d| = 1, 5, 10, 20, 28.28 gives 1.27, 6.37, 12.73,
25.46, 35.99 deg). Cause: RotationVector(own) goes from +(180-e) to -(180-e) about the axis, a vector change of 360 deg, and
adding d to a vector that changed by 360 deg is not the same rotation on the two sides unless d is along the axis. So the
sum the model was taught is discontinuous at half a turn of own, by about 1.27 x |d_perp|.

Note for reading the log: the written vector `RotationVector(result)` of (b) and (c) still jumps by 360 deg-vector at 180
(measured 359.998), as it must for any quaternion read back as a rotation vector; the rotation is continuous. Folding that
away near half a turn is what AnkleFolds does (see section 4); I did not test AnkleFolds numerically.

## 2. Distance from the taught sum (a), by own's angle (degrees between the turns; d random in the box)

| own angle | (b)-(a) median | p95 | worst | (c)-(a) median | p95 | worst |
|---|---|---|---|---|---|---|
| 0-15 | 0.84 | 2.41 | 4.20 | 0.84 | 2.41 | 4.20 |
| 15-30 | 2.86 | 5.33 | 8.02 | 2.86 | 5.33 | 8.02 |
| 30-45 | 4.85 | 8.35 | 12.30 | 4.85 | 8.35 | 12.30 |
| 45-60 | 6.79 | 11.38 | 16.20 | 6.79 | 11.38 | 16.20 |
| 60-75 | 8.61 | 14.31 | 19.66 | 8.61 | 14.31 | 19.66 |
| 75-90 | 10.34 | 17.11 | 23.57 | 10.34 | 17.11 | 23.57 |
| 90-105 | 12.01 | 19.70 | 26.61 | 12.01 | 19.70 | 26.61 |
| 105-120 | 13.43 | 22.10 | 30.12 | 13.43 | 22.10 | 30.12 |
| 120-135 | 14.68 | 24.21 | 32.91 | 14.68 | 24.21 | 32.91 |
| 135-150 | 15.91 | 26.22 | 34.56 | 15.91 | 26.22 | 34.56 |
| 150-165 | 16.92 | 27.81 | 37.19 | 16.92 | 27.81 | 37.19 |
| 165-180 | 17.73 | 29.13 | 38.00 | 17.73 | 29.13 | 38.00 |

Whole range: median 9.18 deg, worst 38.00 deg, for both. As a ratio to |d| the median grows from 0.05 (own 0-15) to 1.00 (own
165-180): at half a turn the taught sum and the composed turn differ about as much as d itself. (b) and (c) are also far from
each other (median 30 deg, worst 65 deg at 165-180), so the order between them is not a small matter.

## 3. Which is nearer (a); first order in d

- Away from 180, (b) is not nearer (a) than (c), and (c) is not nearer than (b): on every one of 400000 samples the two
  distances are equal (max |difference| 0.000 deg; script `mend4b.py`). "Fraction where (b) is nearer" printed in
  `mend4.out` (0.02-0.28) counts only strict inequality of numbers that agree to rounding and means nothing; ignore it.
  Reason, with the Jacobian: (a) = own * exp(Jr(w) d) = exp(Jl(w) d) * own (w = own's rotation vector, Jl = Jr transposed),
  so the angle from (a) to (b) is |(I - Jr) d| and from (a) to (c) is |(I - Jl) d|; the two vectors have equal length because
  [w]d is orthogonal to [w]^2 d. The two orders sit symmetrically about the taught sum.
- First order: neither equals (a) to first order in d, except at own = 0. Scaling d by 1, 0.1, 0.01, 0.001 at own = 10..170 deg
  scales the difference by exactly 10 each time (ratio 10.00 between 0.01 and 0.001): linear in d, not quadratic. Leading
  order for small own angle theta (radians): difference = (theta/2) * |axis x d|, checked at theta = 2, 5, 10 deg to
  4 digits (e.g. theta 10: measured 0.0023379, predicted 0.0023399). Spectral size of (I - Jr): 0.26 at 30 deg, 0.73 at 90,
  1.08 at 150, 1.18 at 180. (b) applies d unchanged (the identity); the taught sum applies Jr d; (c) applies R^T d.
- Two orders cannot both be nearer: the taught sum is, to first order, "the turn d seen through Jr(w)", which lies half way (to
  leading order in theta) between "d in own's child frame" (b) and "d in the parent frame" (c).

## 4. The code (git show 262f4ca:Assets/_WonderGather/Scripts/Units/OwnRiser.cs)

- OwnRiser.cs:463: `by = RotationVector(AnkleFolds(leg, own * FromRotationVector(drivenNow[j])));` This is (b): `own` on the
  left, FromRotationVector(drivenNow[j]) on the right, Unity's order = d applied first, in own's frame. As described.
- AnkleFolds receives that product as its `ask` (OwnRiser.cs:507, `ask` is then `q = Inverse(stood) * ask`, w >= 0 enforced).
  Yes. `own` was normalised in Aims (before the call at :412) and FromRotationVector returns a unit quaternion, so the product
  is unit. As far as I read, no further normalisation is needed.
- `drivenNow[j]` is a Vector3 of three numbers from `Driven[3j..3j+2]`, eased (:953); the code comment (:1294) calls them
  degrees about each axis. I assume the same units for the box (plus or minus 20); I did not check the trainer.
- wayAsks: at :462 `wayAsks[j] = RotationVector(own)`, the way's ask alone, before the learnt turn and before the limit.
  Aims sets `wayAsks[j] = by` before adding drivenNow (:418), also the way's ask alone. Same quantity in both branches, so the
  observation (:1321, `wayAsks[j][d] * Deg2Rad` clamped to plus or minus pi) is unchanged in meaning. One difference to
  remember: with the limit on, the observation shows own's ask even where the target actually written (after the limit) is
  quite different, so the model does not see the limit. As in the AnkleAtMost branch (:474-476), where wayAsks is the
  post-clamp ask: the two branches of AnkleMended record wayAsks at different stages (limits: unclamped; no limit: post-AnkleCan).
  Not new in mend 4; a note.
- The comment at :458-461 states the order is open ("Whether this order is the one the model was taught by is open"). The
  numbers above agree: it is not the same, by up to 38 deg, and it is not first-order equal.

## Findings

1. note: (b) is a different turn from the taught sum, median 9.2 deg and worst 38 deg across the box; 0.8 deg median only
   for own within 15 deg of 0. A heard model run through AnkleLimit 1 gets targets it was not trained on whenever the ankle's
   ask is more than about 30 deg from how it was made (median > 5 deg there). The note in build.md and the code comment
   say "open"; the size of "open" is above.
2. note: (b) and (c) are exactly equally far from (a) (not an advantage for either); choosing between them is a choice
   between "d in the boot's frame" and "d in the shin's frame", and (a) is neither.
3. note: (a) is itself discontinuous at 180 (up to 44 deg in the box); it is the sum the model was taught, so any
   model that learnt it learnt a target that flips, and (b) removes the flip rather than reproducing it.
4. note: wayAsks has different meaning in the two AnkleMended branches (unclamped vs after AnkleCan), see above. Not a
   defect of mend 4.

No must-fix, no should-fix on mend 4 itself.

## What would decide the order

The numbers say only how far the orders are from each other and from the taught sum, not which the model wants. Decisive
evidence would be: (i) the trainer's own code for how it wrote the target (if it wrote `RotationVector(own) + d`, (a) is the
definition of what d means, and the question is whether to reproduce its flip or its unfolded sum: build (a) with a fold only
for the written target); (ii) the heard model's d as a function of own's angle: if its d in training samples never occurred with
own more than about 15 deg from 0 (the ankle's usual range, below the 0.84 median band), the order is below noise and
(b) is harmless; if it did, the 30-180 deg bands matter; (iii) a run of the same heard get-up with (a), (b) and (c) as three
switchable orders on the same courses and counting, as the project's rule says, courses not runs; (iv) the distribution of
own's angle in the rise log (ANKLE_ lines in D:/Dev/WG/TestRuns/*.log) against the table in section 2.

## Not checked

- AnkleFolds itself (its continuity, its fold, its twist/swing split) numerically. I read it, did not model it.
- Whether PhysX applies a ball joint's X/Y/Z drive targets as a rotation vector, as a swing-twist angle triple or as per-axis
  independent angles: the "taught" order is a claim about the trainer's code and the joint's convention, neither seen.
- The trainer's (D:/Dev/mlagents-env) code, the learnt model's weights, the logs in D:/Dev/WG/TestRuns/, mends 1, 2, 3, 5.
- Whether |ask + d| above 180 deg occurs (the sum in (a) is not folded again by the code before FromRotationVector).
- Nothing was run in Unity. No file under D:/Dev/WG was touched.
