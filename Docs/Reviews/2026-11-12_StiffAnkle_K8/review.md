# Review: the stiff-ankle answer (K8, R4, R1)

November 12, 2026. Reviewer: an Opus 5.5 subagent at medium effort, who wrote nothing reviewed here. Advice, not the
Game Director's verdict.

Source cited: PhysX 4.1 `DyFeatherstoneArticulation.cpp` (local copy) and `DySolverControl.cpp` beside it. Log:
`D:/Dev/WG/TestRuns/facts_r4_pgs3.log` through `r4_table.py`.

## 1. The held part's law under PGS: holds (at the design's stiffness)

- Source: `setupInternalConstraintsRecursive` (copy ~2600-2611, 2632-2635) sets a = dt(dt k + c), x = 1/(1 + a R),
  bias k x dt, velocity factor -x a, and for PGS keeps (1 - x) of the last impulse; `solveInternalConstraintRecursive`
  (~4153, 4170-4174) applies it every turn. Both kinds of turns count **in the source**, not only in the numbers:
  `DySolverControl.cpp` calls `solveInternalConstraints` once per place turn (`velocityIteration` false, ~237/266) and
  once per speed turn (true, ~244/293/312), and the drive's update is not gated by that flag (only the limit rows are,
  ~4189). So n = place + speed, and 14 + 18 = 28 + 4 = 32 is expected, as read.
- Recomputed shares at n = 18: toe 0.697 / 0.532 / 0.453, sideways 0.608 / 0.398 / 0.351. Law torques toe: 18 turns
  15.7 / 20.5 / 24.1; 32 turns 19.8 / 28.5 / 35.0; 64 turns 22.2 / 35.9 / 47.0. All as K8 (Round 24.0 vs my 24.1:
  rounding). Readings 16.1 / 21.7 / 26.1 are 2.5 / 5.9 / 8.3-8.8% over; never under. Holds.
- Limit of the claim: at 3x and 10x stiffness the toe readings are far over the law (Small 12.5 vs 11.1, 9.3 vs 5.3;
  Long 17.4 vs 14.0, 13.8 vs 6.6; Round 21.1 vs 15.5, 19.2 vs 6.9: up to 2.8 times). K8 must say "at the design's
  stiffness"; the excess tracks the tip (claim 6), so the share's floor on stiff/light joints is not the law alone.

## 2. The free part: holds

Per turn F' = (1 - x) F + x (dt k e - a w), w = w0 + R F before the turn's own delta. From F = 0 the first turn gives
F1 = x (dt k e - a w0), which is already the fixed point of F = dt k e - a (w0 + R F); then w_end = w0 + R F1 and
dt k e - a w_end = F1, i.e. tau = k e - (dt k + c) w_end. Later turns leave it unchanged, provided no other row (contact,
limit, another axis of the same joint) moves w afterwards; with such coupling it holds only once the turns converge.

## 3. A part let go: holds as a one-step law; "no faster after" is wrong as a reason

- w1 = w0 + F1/I = (I w0 + dt k e)/(I + a): algebra holds. Air, full strength: Small dt S/(I + a) = 1.22/0.1083 = 11.26
  (read 11.3; diagonal 11.4, a different I); Long 10.1, Round 11.2: at the law, within ~1%, not under it. Bent 22 deg
  (0.384 rad): 7.1 / 12.2 / 16.4 for Small at 1x / 3x / 10x, read 4.1 / 11.9 / 16.4. "None was over it" holds to the
  rounding of the table (10x is at the law, not under it).
- The reason for 4.1 vs 7.2 is sound: the edge bears and drags (a partly held part, claim 1), while e shrinks as the
  joint turns; at 10x, a >> I, so w goes in one step to its fixed point k e/(dt k + c) whatever the first steps held,
  which is why the stiff rows meet the law.
- But the bound is the **fixed point**, not the first step: w_{n+1} is a weighted mean of w_n and k e_n/(dt k + c), so
  with the offset kept (as R1's writer can, the target written from where the joint is) the speed climbs to
  k e/(dt k + c) > dt k e/(I + a). R1's "no faster after" holds only because e shrinks in these runs. See 4(c).

## 4. R1's making-up (not built)

**(a) Ball joint, three axes: holds per axis, with conditions not stated.** The engine makes one row per driven axis
(`setupInternalConstraintsRecursive`, each with its own unit response along that axis) and solves them in turn; the
coupling through the part's turning weight enters only through each row's velocity change on the others. So "told over
said" per axis is right once the turns converge (claim 2's fixed point holds row by row). But (i) under coupling a free
axis's ratio can leave [law, 1] for a step; the clamp to [law, 1] covers it. (ii) The axis's "real turning" read as the
change of the joint's position reading over dt is not the axis's angular speed on a ball joint (the rotation-vector
reading's rate differs from the angular velocity by terms of order angle times speed): small per step, an
approximation to name. (iii) The offset bound eFull + dt wmax must be on the **norm** of the three-axis offset, not per
axis, or a diagonal target gets up to sqrt(3) times the offset and 3 times the work of 4(d).

**(b) An unstable loop: not shown; the linear part is safe, three non-linear paths are not.** In the linear law the
share does not depend on e, so the estimate is not moved by the target it writes (Handoff's "does not feed back on
itself" holds there). Three paths do feed back. (1) The force clamp: once the made-up k e exceeds S, given/said falls,
the estimate falls, the target rises, the clamp bites harder: a ratchet to the floor. "Said" must be the clamped value
(min(|said|, S)), or clamped steps must not teach. (2) A joint on its limit: the limit row takes the drive's push and
reads as "held"; and near a stop the engine clamps the made-up target (claim 5), so the offset written is not what
arrives. (3) A hold that comes and goes: more torque tips the sole off its hold, the reading says free, the estimate
jumps to 1, the torque drops, the hold returns: a possible rocking cycle at an edge, bounded by (c) but not excluded.
Step 2 should run an edge case with the making-up on and read the estimate step by step.

**(c) The offset bound keeps a freed part under wmax: holds, by the fixed point, with two additions.** With
dt k + c = (S/wmax)(1 + b) and k E = S(1 + b) at E = eFull + dt wmax, the fixed point k E/(dt k + c) = wmax exactly; the
speed approaches it from below as a weighted mean (claim 3), also when the offset is kept every step. A larger c lowers
it. Gravity or any other torque T on the part adds T/(dt k + c) (a boot: ~0.6 N m over ~5 N m s, 0.1 rad/s). The bound
is on the joint's relative speed: a turning parent adds its own turning to the part's speed in the world, so R3's "a
part turns faster than its bound" must be read on the joint. The force clamp only lowers it.

**(d) The stroke: Opus 5.5's form holds; Fable 5.2's is true but looser.** Assumptions: the implicit law of claim 2 holds
at the step's end (a free part, or any part whose turns settled), the work counted as told torque times dt w_end,
|e0| <= E = eFull (1 + b). Then W(w) = dt w (k e0 - (dt k + c) w), greatest at w = k e0 / (2 (dt k + c)):
W* = dt (k e0)^2 / (4 (dt k + c)) = S eFull b (1 + b) / 4, whatever I and the starting speed (they only choose w_end).
Over the stroke S eFull / 2: b (1 + b) / 2 = 0.538 / 0.402 / 0.486 (b = 0.651, 0.527, 0.606); over 1 only for b > 1,
wmax > eFull/dt = 30.5 rad/s. At that w the torque is S (1 + b)/2 <= S for b <= 1, so the clamp does not bite.
Fable's b(1 + b)^2/4 (0.888 / 0.615 / 0.781) is the same with c alone in the denominator (dt k dropped).
Not covered: a partly held part whose hold releases during the step's turns (the impulse lags above the law's value at
w_end); there only W <= S dt w_end <= S eFull b holds. R1 should state: "In one step a drive works at most
S eFull b(1 + b)/4 on a part whose turns settle (0.54, 0.40, 0.49 of the stroke at the three ankles; over it only where
wmax > eFull/dt). With the target rewritten each step no stroke bounds the sum; what bounds it is the speed bound of
(c), 1/2 I wmax^2 on the part." And replace "and no faster after" by the fixed point.

**(e) "At once free, slowly held": not a switch in the answer, but write it so.** s <- max(r, s + a (r - s)) is
continuous in the reading: no jump of torque at a threshold, so Luis's rule (October 13) is kept in the answer. The
"learns nothing while the spring says next to nothing" is a threshold if built as a cut. Smooth form: weight
w = said^2/(said^2 + T0^2), T0 a small share of S; s <- s + w (a_hold + (a_free - a_hold) sigma((r - s)/d)) (r - s),
sigma a smooth step, a_free near 1.

## 5. The clamp: holds; §2.3's consequence follows under a stated geometry

`setupInternalConstraintsRecursive` (copy ~2575-2577) clamps the target to [low, high] for a limited axis only, once a
step before the rows are made. At the stop the target is the stop, e = 0, the spring gives nothing, and the limit row
only pushes out of the stop. For a straight limb whose load's lever grows as L sin(theta) off straight, the spring
S theta/eFull out-pulls W L theta iff S > eFull W L = 0.61 W L (eFull in radians). So the 0.61 holds with L the
distance along the limb to the load and small angles (larger angles favour the spring); with tone, tone x S. Say so;
and note R1's made-up target is clamped too.

## 6. The sideways sum and the tip: holds as read; a simpler reading of the sum; the tip is the ground's

The "nothing asked" torque is the sideways law applied to the tip angle: share_side x k x tip = Small 1.7 / 3.7 / 6.1
(1x / 3x / 10x) against read 2.1 / 4.2 / 6.6; Long 2.8 / 5.6 / 8.7 against 3.5 / 6.6 / 9.9; Round 5.9 / 11.0 / 16.3
against 6.1 / 11.2 / 16.4. So the sum is one linear law on one error (asked offset plus tip), not two contributions.
The tip does not shrink with a 10x stiffer ankle (1.62 -> 1.85, 3.83 -> 3.93) but does with turns of place (1.62 ->
0.24): the contact rows' unsettled answer under PGS (a 1.3 to 1.8 kg sole under 41 to 88 kg; the place turns carry the
penetration bias), not a sag against the spring. Readable by the four corners' depths; not run. Threat to step 2:
moderate. The writer reads the tip as error and, with the making-up, puts 1/share more torque into the ground (with
nothing asked the sole's push already sits 3 to 13 mm aside); Round sits 3.8 to 4.7 degrees against the design's
5-degree "flat" (the test's bar is 6: the two disagree). Hands on wrists are the same kind of light end part.

## 7. TGS's law: holds (matched; derivation not re-traced)

Free: 18/14 = 1.286; probe 7.63/5.92 = 1.289. Held, h = dt/14: h(h k + c)/I = 0.661 / 1.127 / 1.363, shares
0.774 / 0.605 / 0.544, torques 17.4 / 23.3 / 29.0 as K8, read 17.3 / 23.1 / 28.8. Source consistent: under TGS the
impulse is not decayed (`driveImpulseMultiplier` 1, copy ~2635) and the rows are solved in every substep
(`DyTGSDynamics.cpp` ~2619/2630).

## 8. Against the standing decisions: holds; nothing to decide for Luis, two things to tell plainly

R4's clause asks Luis only if the ankle holds under neither solver; it held under PGS, so solver, step, turns and
weights stay. The making-up goes through the drive (R1, "the body's own"), and written as in 4(e) it keeps "no switch
at a threshold". To tell, not ask: the test's bar was reshaped after the first run (flat 2 -> 6 degrees, rest judged by
position), and more turns of place are a priced lever (1.5x cost at 28 + 4) that would bring Round's tip from ~4 to
~1.5 degrees.

## 9. What is missing

- K8's "at most 8.8% over the law" holds at the design's stiffness only; at 3x / 10x the toe readings are 1.1 to 2.8
  times the law (claim 1): unexplained, and the making-up's floor rests on the law.
- R1's "no faster after" is wrong as a reason (claim 3); the fixed point is the reason.
- No held case on a diagonal (toe and side at once); the offset bound is not said to be on the norm.
- "Flat" is 5 degrees in the design and 6 in the test.
- The other joints' shares "about 1" are rough; wrists and hands are not named.
- Several parts held at once, and a half-held part: not run (stated as step 2's).
- Gravity and the parent's turning in the speed bound; the clamp and the limit row in the share's reading.

## What must change in the design text before step 0 goes on

1. K8.2: "at the design's stiffness"; add the 3x / 10x toe excess as not known. Say the source shows both kinds of turns
   count (`DySolverControl.cpp`), moving it out of "Not known".
2. K8.4 / R1: the speed bound is the fixed point k e/(dt k + c) (= wmax at the offset bound), plus T/(dt k + c) for
   gravity; on the joint's relative speed. Drop "and no faster after".
3. R1: the offset bound on the norm of the three-axis offset.
4. R1: the per-step work S eFull b(1 + b)/4 with its assumptions (settled turns; not a releasing hold); the stroke does
   not bound the sum when the target is rewritten.
5. R1: "said" clamped to S (or clamped steps do not teach); a limited axis near its stop does not teach.
6. R1: the learning weight and the asymmetric rate written as smooth functions (4e).
7. §2.3: the 0.61 condition with its geometry (lever L sin theta, eFull in radians, times tone).
8. K8.2 / R4: the sideways sum as one law on (asked + tip); the tip as the ground's, independent of the ankle's stiffness.
9. One "flat" number (5 or 6 degrees) in the design and the test.
