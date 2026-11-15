# Critique 2 of the revised ankle mend (fresh sonnet critic, November 15, 2026)

Read only. Code read in `D:/Dev/WG` (`claude/wip-round` `3182e08`), no Unity run, nothing changed but this file. The speed
numbers were redone in a scratchpad script (`chk_speed.py`, `python -I`: the implicit step, force limit, tanh error, smooth
damper floor). Web queries were generic.

## 1. The speed bound: agree in part

**The numbers reproduce.** Small (I 0.0065, k 66.6, c today 0.92, tau 60.86, E 52.3 deg, dt 0.02):
- today, jumps 29/88/140/180 deg: peaks 13.1 / 39.7 / 63.1 / 81.2 rad/s; with the bound (c 1.75): 9.0 / 17.1 / **19.2** /
  **19.6**, overshoot 0.00 deg. Same as the research's table.
- misplaced I 0.25 (k 99.6, c 6.99, E 35 deg): bound 3.4 / 6.2 / 6.7 / 6.7 (today 3.8 / 11.3 / 17.1 / 21.4), overshoot
  0.5-0.7 deg (better than today's 0.6-3.4).
- misplaced I 0.67 (c 11.44): 2.2 / 4.1 / 4.4 / 4.5 (today 2.5 / 7.4 / 11.1 / 13.6), overshoot 0.7-1.0 deg.
- Small already at 40 rad/s toward 140 deg: 21.5 next step, falling. Already at 20 with the target behind: it brakes to
  -15.8 then -19 (no sticking), so the drive does brake and reverse.
So the bound is below 30 for every case worked, and the drive settles at the target (zeta 1.33 for Small, 0.70 for I 0.25).
The first critique's refutation (point 5) is truly answered.

**The proof is right as written** (convex combination: omega' = a*omega + (1-a)*k*e'/(k*dt + c); k*E = tau; the floor makes
Omega*(k*dt + c) >= tau; the force limit only shortens omega' - omega). It does not depend on I. Same-both-ways force
limit: respected, since `forceLimit` is untouched (only sub-option B' changes it, and it is Open and off).

**Is it different from `AnkleTurnsAtMost`? Agree in kind, not in cost.** That limit slewed the *target* against its own last
value (state `ankleAsked`, `OwnRiser.cs:421`), so a planted boot's target lagged without bound. This one measures the
error from where the joint *is*, so the lag is capped at E. But two things the text understates:
1. "Only an error beyond about E is shortened" is not true. `E*tanh(e/E)` shortens at every error: torque is
   `k*E*tanh(e/E)`, so a planted boot loses 2% of its spring torque at 0.25E (13 deg), 7.6% at 0.5E (26 deg), 12.5% at
   0.67E (35 deg, the design's FullAt), 24% at E (52 deg), and reaches 96% of tau only at 2E (105 deg). Today the full tau
   comes at E. The "nothing is changed" sentence should read "softened smoothly from about 10 deg".
2. The damper floor (0.92 to 1.75, plus 90%) is velocity-proportional for a planted boot too, the very case that failed
   (flat boot turning against a turning shin). The research counts it (5 N m at 3 rad/s against 2.8) and puts footA/footB/
   stay on watch: good, but that watch is the real risk of step 1, not "invisible-ish".

**Gaps that do not break the proof but break its use:**
- *Slack share.* `Drive(j, share)` (`OwnRiser.cs:367-376`) writes `spring = springOf*share` and `forceLimit = hasOf*max(share,.3)`.
  Below share 0.3 the force limit stops following the spring, so E = tau/k must be taken from what `Drive` actually writes
  (and c_floor recomputed there, not in `Springs`), also when `Tone` or `StrongerWhileLearning` change `has`. The research
  names neither.
- *Per axis, not per vector.* The recursion bounds each axis by Omega. The vector norm stays <= Omega only if the axes share an
  inertia; with unequal ones a transient can reach about sqrt(2)*Omega, and sqrt(3)*Omega = 34.6 is the envelope, above K1's 30.
  State the test as per-axis joint speed, or take Omega about 15.
- *What "boots above 30 rad/s" measures.* The proof bounds the **joint's relative speed by its own drive**. A link's absolute
  angular velocity adds the shin's turn, so step 1's "none above 30 by the drive's own doing" must be read from the joint
  (`GetJointVelocities`), not the link, or it can fail with the bound holding.
- *Target range.* PhysX (per the research, 5.6 API) wants the spherical target in [-pi, pi]; B writes `reading + e'`, and §2.2
  says the reading is unwrapped. After a multi-turn whirl (60 rad/s for 0.3 s is about 3 turns) `reading + e'` is far outside
  that range, and if the engine clamps it the drive pulls back through the turns, which is the whip B should remove. The
  read-back test (§2.2) must include a target above pi with the reading beyond it.
- *Long and Round are not worked.* The memory says Long fails first; the table is Small and its misplaced boots only. The
  bound is size-free, but c_floor vs c_today for Long's I and tau is not shown.

## 2. The limit's continuity: agree in part

The piece-by-piece check holds; I tried the hard cases:
- A half turn about n with n.t = 0.707 (w = 0): the label tau jumps +178.6 to -178.6 across w = 0, but the twist as a rotation is
  the same (R(180,t) = R(-180,t)) and gtwist = 1-(1-w_t^2)^4 is 6e-4 there, so the twist target is identity on both sides.
  The swing is continuous (s = 90 deg, fold 0.996). Continuous.
- Near s = 180: fold is about 2*delta^2 (delta in rad from 180), so the target is about 2*delta^2*a (1.1 deg at 5.7 deg), while the
  axis turns at rate v/delta: Lipschitz, product about 2*delta*a*v. Continuous.
- The w >= 0 sign fix is hit only by quantities that are even in q (fold, gtwist) or by swing = q*twist^-1, which does not change
  when both flip. Good. (Implementation: `twist = normalize(w, (v.t)t)` has norm cos(s/2); guard the s -> 180 case against a
  NaN, as `Aims` already does for `size`.)

What is still not continuous, beyond the three the research lists:
- **`Slerp(aimFrom, toStood*Turn(j), share)` (`OwnRiser.cs:845-848`)** feeds the limit. At a stage change, if the two poses are
  about 180 deg apart (and the asks sat past 110 deg for 32 of 32 steps in c0 and c3), a small move of the destination flips
  Slerp's shortest path, so `own` itself jumps by up to twice the reach; the limit is continuous in `own`, not in time. This is
  a fourth ask-jump source next to `LookRotation`; the forced check "ask turned through 180 deg" should include it.
- **The shortest turn from the reading.** B and §2.2 use `reading + shortest(T - r)`, but the engine's drive is linear in its
  coordinates (it goes through zero). With the natural ellipse (|T|, |r| below about 70 deg) |T - r| < 180 and the two agree.
  With A = 1 at an isotropic reach of 110 deg (a about 126), a boot held near -100 whose target crosses to +100 has
  |T - r| = 200 > 180: the shortest turn goes the other way round, the same topological cut moved into the reading. Either use
  the linear difference (never wrap), or say A = 1 is safe only while the reading is inside the reach.
- `riser.Aim(j)` is `aimNow[j]` (`OwnRiser.cs:287`), the ask **before** `AnkleCan`; the limited ask is not stored. So the open
  question "does `Aim` return the limited ask" has an answer: no, and `riseFrom` is identical with A on or off. The keeper
  blends from the unlimited ask.

## 3. The order and the expected results: agree in part

- Order B, A = 1, A+B, then C is right: no protection is removed first. C last, never alone: agree.
- Step 4's rule "if worse, the bound is not holding" is too narrow. C changes the keeper's ankle gains after a whirling fall
  (§7.1 says so) and the falling body's hand-over; neither is under B (`OwnRiser` only). A worse result at step 4 can come from
  the keeper phase losing the large damper, with B holding. Add "check the keeper-phase blow-ups separately".
- Step 1 "target jumps over 60 deg: as many as at 0": under B the written target is `r + e'`, so a flip makes a jump of about
  2*E*tanh(.) = 100 deg (Small) or 70 deg (heavy boot): still over 60, so the count holds. But `wayAsks[j]` is set **before**
  `drivenNow` and would be set before B too (`OwnRiser.cs:405`), so a recorder that reads `wayAsks` sees the unbounded ask and
  the step-1 check is empty. The test must log the target actually written.
- Step 2's "A = 1 is visible where asks lie far past 110": also visible inside 110. S(rho) = rho/(1+rho^4)^.25 gives 0.66 at
  rho 0.7 and 0.84 at 1.0 (a asked 88 deg of a 110 reach comes out about 81; 110 comes out 92, times fold 0.96 about 88).
  "Invisible-ish" does not hold for A either; the text should say A = 1 changes every ask above half the reach.
- Expected "Small about 9 of 24" is cc_es24's finding as given (not rechecked here).

## 4. Axes, sources, thresholds: agree in part

**Axes.** The Unity conventions check out: `Euler(90,0,0)*forward` is down, so +x is plantarflexion; `Euler(0,0,90)*up` is -x,
so for a right boot (+x lateral) +z tips the top medially, sole lateral, eversion; inversion -z (mirrored for the left).
§2.2 now says clearly what is known (single axis, unwrapped) and not (combined turns), the order of operations, and a four-part
read-back. Missing: **units**. `Aims` writes `SetDriveTarget` in degrees (`angle*axis`), `jointPosition` is in radians and
stiffness is per radian in this code; E and the tanh work in one unit and the write in the other. State it in the read-back.

**Sources.** Verified here by a generic search: Soucie/CDC 20-44 y ankle values (F dorsi 13.8, plantar 62.1 CI 60.6-63.6;
M 12.7, 54.6 CI 53.2-56.0; CIs of the mean, passive, sitting): correct. Ball & Johnson 1996 (Clin Biomech 11(3):165-9) exists,
applies a set torque about an anterior-posterior axis, females have more range with the gap closing with age, mostly less
eversion in women: matches the text; the 53.2 and 44.3 totals did not appear in what I could read, so I cannot confirm them
(the reviser says they come from the abstract). Still stated as fact: the Katz lengthening figure 1.5-1.8 F0 (§4.2, §4.3;
§7.4 admits not re-fetched), Hill's a/F0 ranges, the Soucie 2-8 y and 45-69 y ankle values and hip/knee numbers (earlier
critic verified only the 20-44 hip/knee), "Froude" source details (Alexander & Jayes 1983 is about quadruped gaits, the source
line is loose). Mean minus one SD with SD 7-8 deg is marked estimate: fine. Eversion is marked derived, mixing a torque-applied
total (Ball & Johnson) with a subtalar-only inversion (Milgrom): a rough subtraction, flagged.

**Thresholds.** The revision's smoothing holds: `c_floor = max(0,x)` inside a 4th-power mean is C3 at 0; `S(rho)/rho` is a
function of rho^4 (analytic at 0); `tanh(x)/x` is analytic; folds and the a(x) blend are smooth. Left: the w >= 0 sign switch
(harmless, shown), the `Mathf.Max(share,.3f)` already in the code (it breaks E, §1), and `rho0`-free. No new kink found.

## 5. The idle and the learnt models: agree

- The riser exists only for a body made low and down (`OwnBody.cs:924`); `OnItsFeet` sets `riser = null` and calls
  `keeper.Soften()`, which writes stiffness, damper (`Floored`) and force limit on all three axes from values read at
  `OwnKeeper.cs:509`, before the riser is made (`:892` vs `:924`). The riser writes no other joint property (no
  `jointFriction`, `maxJointVelocity`, or damping in `OwnRiser.cs`). Sound.
- Two limits of the check: the tick-by-tick identity test on a body made standing is a guard against leaks only (A and B cannot
  run there, C acts at a going down), so it cannot show the post-get-up idle is unchanged; the hand-over read-back compares
  `Soften`'s output with itself. Neither is wrong; the post-get-up idle is a Luis look, as §7.1 step 4 says.
- Model files: right. `SmallRisePace.onnx` (pacer, `"RiseModels/" + called + "RisePace"`), no `SmallRisePaced`, so the joints'
  agent loads `SmallRiseFull.onnx` as Full, not Paced; `SmallRise`, `SmallRiseFullP`, `SmallRisePFull`, `SmallRisePaceP` are
  never named by the loader. Tracked: only `SmallRiseFull` and `SmallRisePace`. `Called` is the choice name, so Long and Round
  load nothing. The early return for a non-Paced agent outside `LearnsAt` (`RiseAgent.cs:171`) and the clearing only in the
  Paced path (`:250`) confirm the stale `Driven`. `DrivesBoot` 20 deg: right.

## 6. The test commands: agree in part

- Exist: `-liveWho`, `-ownAsPlayed` (read in `CaptureTools.cs:18`), `-liveOwn`, `-liveSeam`, `-liveOther`, `-ownRate`,
  `-ownPushes`, `-ownPushWays`, `-liveCap`, `-liveEnergy` (`CoverRingLiveTests.cs:91-190`). Namespace `WonderGather.Tests`, class
  `CoverRingLiveTests`, the test `TheCutRingAsPlayed` is `[Explicit]` and named by class: the filter works. `LIMIT` 15000 s is
  above the test's 14,400 s timeout. Names Small/Long/Round are in the scene. The fixed step is 0.02 s (`fixed 0,0199...`).
- Do not exist, as the research says: `-ownUnheard`, `-riseSet`, `-ownBodySet`, `-liveStrength` in the live test.
- Two more details for the added code: `-riseSet` in `OwnRiseBench.cs:395-402` sets a **public static field of `OwnRiser`** with
  `float` for non-bool types, so `AnkleLimit` and `AnkleSpeed` must be declared `float` (an `int` fails in `SetValue`);
  and `BootsFromOwnShin` is an `OwnBody` switch, so the loop needs `typeof(OwnBody)`. `RiseAgent.Unheard` must be set by the
  live test itself.

## Verdict
Sound to propose to Luis as a switch, off by default, built only after his word: the bound is now real (reproduced: Small
19.2 / 19.6 rad/s at 140 / 180 deg against 63 / 81, no overshoot, brakes at the target), and the limit is continuous on SO(3)
in the swing, the twist and the combined case. Before build, fix in the text: E and the floor from the share-scaled gains
(§1); the target outside [-pi, pi] and the wrapped shortest turn (§1, §2); the Slerp antipode as an ask-jump source (§2);
log the written target, not `wayAsks` (§3); step 4 cannot blame B alone (§3); "A = 1 changes every ask past half the reach"
and "B softens a planted boot from about 10 deg" (§1, §3). None changes the design.
