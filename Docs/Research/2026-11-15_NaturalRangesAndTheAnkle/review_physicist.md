# The ankle mend's build: the physicist's review (November 17, 2026)

A reviewer who did not build the step. I was asked one question: does the code do what research §1.4, §4.2 and §8.1
specify, and will it hold in the engine? I did not run Unity and I changed no project file.

**Rung reached by this review:** I read the code and worked a model of it in Python. The model is of the engine's
implicit drive on a free boot. It is not the engine.

## What I was shown and what I did
- **The change.** I read `git -C D:/Dev/WG diff` and `status --short`:
  - the runtime changes: `OwnRiser.cs`, `OwnBody.cs`, `RiseAgent.cs`;
  - the test changes: `CoverRingLiveTests.cs`, `GettingUpByItsOwnTests.cs`;
  - the new file `AnkleMend.cs`, but only searched for its read-back points, not read line by line.
- **The documents.** I read `build.md` and research §1, §2.2, §4.1, §4.2, §6, §7 and §8/§8.1, with the lines of
  `critique2.md` that §8.1 cites.
- **The code around the change.** I read the code the change sits in:
  - in `OwnRiser.cs`: `Springs`, `Drive`, `Slacken`, `Aims`, `AnkleCan`, `Step` (lines 884-984) and `Begin`;
  - in `OwnBody.cs`: `Lighter`, the make (lines 722-845, 918-948) and `GoDown`;
  - in `PhysicalFall.cs`: `Slacken`;
  - in the project settings: the solver type (PGS, `m_SolverType: 0`) and the fixed step (0.02 s).
- **The one run I read.** From `TestRuns/ank_rb_on3.log` I read only the `ANKLE_MAKE` / `ANKLE_RUNNING` lines, for
  |toStood[Foot]| and the switches.
- **My scripts,** in the session scratchpad, all run with `python -I`:
  - `review_physicist_ankle.py`: the miners' gains, the jumps, a port of `AnkleFolds`, the drivenNow cut and the
    unwinding;
  - `review_physicist_rand.py`: a random scan of the fold's continuity;
  - an inline search for the fold's largest target.

  `review_physicist_max.py` is a stub left over in the scratchpad and does nothing.

## Checked, and holds (in the code, and in my model)
- **Units.**
  - `AnkleSlows` turns the target (degrees) into radians before it subtracts `jointPosition` (radians).
  - `most = forceLimit/stiffness` is in radians, since the stiffness is per radian.
  - It writes degrees back.
  - The floor `has/speed` is in N·m·s/rad.
  - The read-back's "share of E" of at most 0.46 is consistent: a mix-up of degrees and radians would be off by 57 times.
- **E and the floor come from the gains actually written.**
  - `Drive` works out the damper from the spring and force limit it writes, share-scaled, with has floored at
    0.3 of the share.
  - `AnkleSlows` reads E off the live `xDrive`.
  - A change of `Tone` or `StrongerWhileLearning` goes through `Springs` → `Drive` (`Step`, line 923).
  - In my model, at a share of 0.357 and of 0.2, the 180° jump stays under Ω: Small 18.9 and 18.2, Long 15.3 and 13.7,
    Round 16.2 and 13.5. (Below a share of 0.3, E grows and the floor falls, as it should.)
- **The bound holds with the smooth maximum.**
  - c_B ≥ floor, so k·E = has ≤ Ω·(k·dt + c_B).
  - `max(0, x)` sits inside a fourth power, so the joint at x = 0 is smooth to the third derivative. Nothing switches.
- **The tanh and smooth-maximum forms are as specified.**
  - `x⁸` (Eighth) is m = 8.
  - `(1 − w_t²)⁴` is m_t = 4. The variable `sinTwist` holds sin², so `sinTwist⁴` is right despite its name.
  - S(ρ)·σ/ρ has magnitude s/(1+ρ⁴)^¼, as the code has it.
  - The twist is fold·gtwist·reach·tanh(τ/reach).
- **The isotropic twist reach (Tw = 110°)** is what §7.1 specifies for A = 1. The ±15° of §1.4 is for A = 2.
- **The folds are continuous.** I ported `AnkleFolds` and checked it three ways:
  - paths through ±180° in swing, in twist, combined, and on a wobbling combined path: the largest target step was
    0.09° for an ask step of 0.036°, the same whatever stood and rested were;
  - 200,000 random asks between 150° and 210° (a 0.01° nudge each): the largest ratio was 3.74, with no jump;
  - the sign of q (w ≥ 0), the pure half twist (gtwist = 0) and the half swing (fold = 0) all reproduce.
- **`drivenNow` comes before the limit,** as §6 and §7.1 ask. But see finding 2 for how it is added.
- **Omega scales by height.** speed = AnkleSpeed·(0.605714/tall)^½ gives Small 20.00, Long 16.18 and Round 18.64,
  which is β = ½.
- **Long's and Round's numbers in build.md part 1, recomputed** from the code's formulas and the read-back inputs:
  - k: 93.766 and 73.682 (read back 93.767 and 73.678);
  - c off: 1.2965 and 1.0188;
  - Omega: 16.178 and 18.637;
  - E: 86.3° and 120.0°;
  - floor: 6.854 and 6.803;
  - c_B: 6.8560 and 6.8042, which is 5.29 and 6.68 times today's;
  - ζ: 3.70 and 4.68;
  - c/k: 73 and 92 ms;
  - peaks, today: 140° 62.7-66.2 and 62.7-66.4, 180° 80.7-85.1 and 80.6-85.4;
  - peaks, with B: 140° 14.4-14.5 and 14.7-15.0, 180° 15.4 and 16.3-16.4;
  - already turning at 40 rad/s, with B: 16.3 and 16.4 at the most;
  - diagonal, with B: 10.21 and 10.51 per axis, 14.42 and 14.77 as a vector.

  All agree with build.md. Small agrees too.
- **The √2·Ω transient.**
  - Spinning at Ω about one axis and then asked 140° about another, with B, the vector peaked at no more than 0.95,
    0.90 and 0.80 of Ω.
  - The reason: the share kept from the step before, I/dt ÷ (I/dt + k·dt + c_B), is only 0.087, 0.052 and 0.044, so a
    transient dies within one step.
  - For a planted boot, the mode with a large inertia cannot be sped up by the drive.
  - Not checked: coupling through the off-diagonal mass matrix of the whole articulation.
- **The read-back test reads per-axis joint speed** (`boot.jointVelocity`), as §8.1 asks.
- **C.**
  - At the make, `anchors[Foot]` is computed after the refit, so `Lighter` and the anchor agree: anchor = stoodAnkle
    exactly.
  - At GoDown, the second `Slacken` overwrites `footAtTo`. `footAtWas` is the same on both calls.
- **The idle path is unchanged.** The off-path adds only the two tests named in build.md, and runtime code has no
  UnityEditor reference.

## Findings
1. **(should-fix before the arms) The read-back §8.1 asks for, of a target outside [−π, π], was not done, and on it
   hangs whether B holds after a whirl.**
   - `ArticulationFactsTests` showed that a 3-DOF spherical joint spun about one axis reads unwrapped (6.12 rad at 350°).
   - So after a whirl within one make, `at` can be several turns, and B writes `at + off`, far outside [−π, π].
   - If the engine accepts that target, the linear difference against a target in (−180°, 180°] unwinds the boot at Ω.
     My model: 3 turns took 1.10 s (Small), 1.48 s (Long) and 1.42 s (Round) at 19.9, 16.2 and 18.6 rad/s. That is
     bounded but visible.
   - If the engine clamps or wraps the target, |e| is no longer ≤ E, and the speed bound fails on that unwind.
   - Research §2.2 (the branch nearest the reading) and §8.1 (the linear difference, never wrapped) disagree here. The
     code follows §8.1.
   - The mend: run §2.2's read-back steps 1-3 and §8.1's case, with the reading past π and a target beyond it, before
     any course is counted.
2. **(should-fix before a heard arm) With A on, `drivenNow` is added to the rotation vector of the unlimited ask
   (`asked + drivenNow[j]`). That makes a new cut where |own| crosses 180°.**
   - The old path added it to an ask already clamped within 110°, so it had no such cut.
   - My model, stood = identity: drivenNow of 20° on one axis makes a 25.5° jump in the ask, which the fold all but
     removes (0.16°).
   - At its cap of (20, 20, 20), the ask jumps 35.9° and the target 37.2°.
   - It is absent as played: `Unheard` is true, so drivenNow is 0. It is present in §7.3's heard Small arm.
   - Composing as turns, `own · FromRotationVector(drivenNow)`, is continuous (0.02° in my model). Which order matches
     how the model was taught is open.
3. **(note) The remaining cut (1) of §1.4 is not removed: `RotationVector(can)` is still folded into (−180°, 180°]
   from as made.**
   - Under A = 1 the largest target from stood is 107.1° (swing 107°, twist −96°), so the cut needs |toStood[Foot]|
     ≳ 73°.
   - The run read back 21.98° (Small), 1.66° (Long) and 3.33° (Round), one make each.
   - If it is met with B on, the target jumps by up to 2E (Small 124°, Long 173°, Round 240°) and the speed stays ≤ Ω.
   - Keep reading |toStood| at every make in the arms.
4. **(note) A boot whose `Aims` is skipped keeps its last written target.** This happens to a leg whose hip turn is NaN
   and is not planted (`Step`, line 974).
   - With B that target is the slewed one, not the ask, so the boot stops short of an ask that was far from the
     reading when the writes stopped. For Small, at 30° of error it stops 2.1° short.
   - On those steps the bound's premise, a target rewritten from the present reading, does not hold. It acts as an
     ordinary spring toward a fixed point.
5. **(note) Long's and Round's damping, 5.3 and 6.7 times today's, comes from τcap/Ω, not from the tanh.**
   - Their E is 86° and 120°, so the tanh hardly acts. At 30° of error Round loses about 0.6°.
   - An alternative bound, if footing suffers: hold the error within E′ = Ω(k·dt + c_today)/k instead of raising c.
     That gives Small 38.8°, Long 31.4° and Round 36.1°.
   - It keeps ζ at 0.7 and the same Ω bound, but caps the push at a large error to 38.1, 51.3 and 46.5 N·m, out of
     60.9, 141.2 and 154.3.
   - That is a change of strength. It is Open, and Luis's to decide, not a defect of the build.
6. **(note) The fold's slope.**
   - Combined with the twist fold, the target moved up to 3.7 times the ask's speed in my random scan. §1.4 states 2.4,
     for the swing alone.
   - This is harmless under B. With A alone, it multiplies how fast the target moves.
7. **(note) `AnkleSpeedHeight` is fixed at Small's present height (0.605714).** If Small is redrawn, its Ω is no longer
   AnkleSpeed.
8. **(note) C's refit at the make** could place a boot that was misplaced by 0.4-0.7 m into the ground. Depenetration
   is capped at 1.5 m/s. No such make was met, so this is untested. It belongs to §7.2's forced check 3.

## Not checked
- Nothing was run in the engine:
  - the implicit-drive model against PhysX PGS with 14 and 4 iterations;
  - coupling to the shin, the body and contacts;
  - how the engine represents combined turns, and the order of the axes in `jointPosition` for a 3-DOF ankle;
  - whether the engine accepts a spherical drive target outside [−π, π].
- The Slerp from `aimFrom` (`OwnRiser.cs:975`) as a fourth source of ask jumps. It is not changed by this build, as the
  spec allows, and its forced check was not run by anyone.
- §7.2's forced checks, the arms and the idle proof.
- `AnkleMend.cs`, line by line: its replicas of the target and the damper. I read only its use of `jointVelocity` and
  the field reads.
- Whether `wayAsks`, which is set before the limit when A is on, changes what any trainer observes.
