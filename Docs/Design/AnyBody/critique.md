# Adversarial critique of the any-body designs (body map, joint model, stances and strategy)

Status: critic's report, November 3, 2026. Nothing under D:/Dev/WG or the main project was changed. No Unity run.
Code checked at D:/Dev/WG, branch `claude/wip-round`, HEAD `8846684`: the commit all three documents read.
PhysX facts were checked in the saved PhysX 4.1 sources (`scratchpad/anybody/physx41/`); file:line below.

## 0. What was read

- **The three designs, in full:** `body_map.md`, `joint_model.md` and `stances_strategy.md`. None is partial.
- **Code** at the cited lines:
  - `OwnBody.cs`, `OwnKeeper.cs`, `OwnRiser.cs`, `PhysicalFall.cs` and `PhysicalBody.cs`;
  - the prototype `l12/b3.py`;
  - PhysX 4.1 `DyFeatherstoneArticulation.cpp` and `DyFeatherstoneForwardDynamic.cpp`.
- **Luis's documents:** the November 3 decision, the CMU section of the frame-by-frame research, the research report's
  layer-0 sections, and the memory notes on the body's own movement.

**What holds up.** These all checked out:

- The body map's formulas and line numbers.
- The joint model's PhysX reading:
  - the spherical `jointPosition` and its 2π branch (`DyFeatherstoneForwardDynamic.cpp:913-935`);
  - the implicit drive and its target clamp (`DyFeatherstoneArticulation.cpp:2575-2610`);
  - `maxJointVelocity` as a per-DOF clamp.
- Small's gains, reproduced by hand: keeper hip τ/c ≈ 22, knee ≈ 39, ankle ≈ 450.
- The CMU "hip 153" reading. `cmu_getup.py:116` measures it trunk-to-thigh, so "hip + back" is right.

## 1. Ranked findings (most severe first)

### F1. Strength is not where the designs think it is: half the torque bypasses the drive (severe)

**Claim.** The joint model says the drive's torque is `clamp(k·e − c·ω, ±τcap)`, so torque falls linearly with speed,
"enforced inside the engine's own solve with no per-step code". Layers 1-2 then compute r as a share of τcap.

**The code.** The bodies' weight-bearing torques do not go through the drive.

- **Keeper.** It applies Jacobian-transpose leg torques as equal-and-opposite `AddTorque` pairs, each clamped to
  `strength[j]` on its own (`OwnKeeper.cs:1047-1078`, clamp at 1072). The drive has its own `forceLimit = strength[j]`
  as well (`OwnBody.cs:653`, `OwnKeeper.cs:224`).
  - So a keeper leg joint (hip, knee or ankle, the loop at 1067) can apply up to **2 × τcap**. The drive's speed
    damping does not touch the explicit half.
  - The keeper's knee drive is not negligible. On Small, k = 87/1.62 ≈ 54 N m/rad, against tens of degrees of error
    during the keeper's rise.
- **Riser.** It budgets its `AddTorque` against only the drive's static spring estimate, `has − min(has, k·angle)`
  (`OwnRiser.cs:1490-1491`). It ignores damping and the direction of the drive's torque.
- **Ball joints.** `forceLimit` is per axis (`DyFeatherstoneArticulation.cpp:2636`, inside the per-DOF loop). A ball
  joint's drive can therefore give up to √3 × τcap along a diagonal.
  - The prototype's LP instead assumes a near-sphere of radius τcap (13 directions, `b3.py:332-334, 408-416`).
  - So the engine's capacity, the keeper's capacity and the atlas's capacity are three different shapes.

**Evidence that the speeds come from the explicit path.** The joint model's own measurements show it.

- During getting up the hip reaches 20-23 rad/s (99th percentile), and up to 62 after the hand-over. The riser's
  hip drive gives no torque at all above about 8 rad/s (τ/c ≈ 7.5).
- The catching step's hip reaches 24.7 rad/s while its swing drive's τ/c is about 14 (my figure). The carrying
  torques at `OwnKeeper.cs:1050-1051` are explicit.

**Consequences.**

- The "muscle" (torque falling with speed) does not exist for the paths that move the body.
- Every r in the atlas, the creator's warning threshold ("no route below 0.27-0.29") and the tempo law are
  calibrated against a capacity the game body exceeds by up to about 2× while the keeper runs.
- This is a strength rule fixed by accident, the thing the body map warned against.

**Correction.**

- **Layer 0 defines one budget per joint:** `|τ_drive + τ_explicit + τ_ff| ≤ τavail(ω)`, in one capacity shape, either
  a sphere or an anatomical ellipsoid. Enforce it either way:
  - route the controllers' explicit torques through the drive (target = current + τ_des/k, inside the slew); or
  - set `forceLimit` each step to the remainder, read back with `GetDriveForces`/`driveForce`, which the 6000.6 API
    list has.
- **Make the LP use exactly that shape.**
- **Add a `budget` kind to BodyWatch.**
- **State plainly that capping the keeper at 1× changes standing and the step.** Whether to compensate through the
  strength rule is Luis's (Open) decision. Do not quietly keep the 2×.

### F2. The whip fix is overwritten at run time, exactly in the step (severe for stepping/catching)

**The design.** The floor (`c ≥ τcap/ωmax`), the slew and the limits are set at make. The design says the shim covers
the three `Aims` methods.

**The code.** Three other places rewrite stiffness, damping and `forceLimit` at run time:

- `OwnKeeper.Springs` (503-521), on the swinging leg;
- `OwnKeeper.Soften` (220-229);
- `OwnRiser.Drive` (367-376).

**The swing ankle.** While a boot swings, `Springs` sets k = min(61/37°, SwingAnkle 16.27 × Rule²/dt² × 0.0055) ≈ 92
and c = 0.369 × 2√(k·I) ≈ 0.52. That is τ/c ≈ 116 rad/s, matching the step's measured boot maximum of 122. So:

- **As designed**, the fix is absent during the swing, where Luis's liked step lives.
- **If the floor is applied there** (c = 3.05), the swing ankle's lag c/k grows from about 6 ms to about 33 ms in a
  0.12 s swing. The boot may land unflattened.

**The standing ankle.** Its damping rises 22× (0.135 to 3.05). That is omitted from R2, which names only the knee and
elbow (1.3-2.7×). The keeper's windows are narrow: Edge 0.00435 vs 0.008 decided 11 or 12 of 12, and the memory says
the order of a sum once tipped Round.

**Correction.**

- **Write every drive through one layer-0 writer** that applies the floor, the limits and the slew. The keeper and
  riser call it.
- **Decide explicitly** whether the swinging and the loaded ankle get the floor.
- **Add the ankle to R2.**
- **Budget a keeper re-search** (`BodysOwnBench.Search`) on the anatomical body. "Within run-to-run spread" without one
  is unlikely.

### F3. `maxAngularVelocity` is not "nothing" on articulation links (factual error)

**The documents.** The joint model cites the PhysX manual: "links ... do not support damping, velocity clamping". The
body map says the bound "evidently does not hold".

**The source.** PhysX 4.1's reduced-coordinate forward dynamics calls `FeatherstoneArticulation::computeD` every step
(`DyFeatherstoneForwardDynamic.cpp:86`). It applies link linear and angular damping, and above `maxAngularVelocity` a
drag of `I·ω(1 − ωmax/|ω|)/dt`, in world frame, on the link's own inertia
(`DyFeatherstoneArticulation.cpp:3360-3393`). The manual sentence is stale.

**Why it matters.**

- `maxAngularVelocity = 20` on every link (`OwnBody.cs:639`; the toe links are left at the default) is a live soft drag
  today. It acts on world spin, so it also acts on a legitimately tumbling body.
- It barely slows a light link coupled to a heavy chain, which explains 154 rad/s.
- Removing or raising it "because it does nothing" changes every fall.
- The isotropic upper-arm tensor changes the drag that the arm feels.
- The step-0 test `JointSpeedBoundHolds` ("maxAngularVelocity does not") would pass for the wrong reason.

**Correction.**

- Fix the text in both documents.
- In step 0, measure the drag: spin a free link, and a link on a chain, at 2× the setting.
- Hold the setting unchanged through step 3. Whether Unity 6000.6 passes the value through is part of that test.

### F4. Layer 3's "posture prior" invites the posed body Luis rejected (understated acceptance risk)

Layer 3 is to receive "a posture prior: the fitted pose, as offsets from the body's own standing pose". The atlas is
a set of optimised static poses per stage.

**The risk.** If layer 3 tracks those with joint PD targets stage by stage, it is today's riser ("one pose after
another its joints are held towards", `OwnBody.cs:260-261`) with machine-made poses. On October 9 Luis refused a
body posed from weight and strength: "I don't want it to be posed like that". The research report cites Al Borno:
driving to a planned pose "does not work".

**What the stances document guards.** It guards repetition by seeded noise only (logN times, band jitter). Noise is
not naturalness. Neither document says how the prior is used.

**Correction.**

- Write into the layer 1-2 contract that the posture prior is only a weak null-space regulariser.
- Layer 3's primary output is support placement, the centre of mass and the head and trunk direction, recomputed from
  the measured state every step.
- Let most variety come from the state: which hand is already nearest the ground, which knee is already bent, the
  lying side. Seeds come second.
- Make "does it read as posed?" an explicit judge question against the present riser.

### F5. The stiff ankle "for real balance" contradicts the project's own recorded finding (high, untested)

**The design.** The joint model drops the inertia cap for the new controller because "the implicit drive ... is stable
at any stiffness". On Small's ankle, k = 61/35° ≈ 100 N m/rad on I ≈ 0.0055, so k·dt²/I ≈ 7, against the cap's 0.41.

**The project's finding.** `OwnKeeper.cs:37-40` records that "a stiff spring between a heavy body and a light boot does
not hold at the game's step". Implicitness holds for one DOF's own response. Contact rows and neighbouring joints are
coupled through 14/4 PGS iterations.

**The gap.** Step 0 has no test for it.

**Correction.**

- Add `StiffAnkleHolds` to step 0: one- and two-foot stance on the meadow, ankle at τcap/35°, 20 s plus shoves, zero
  BodyWatch chatter, all three miners.
- Until it passes, keep the cap and say what the new controller loses.

### F6. Two bodies, one static switch, and a limp fall that has no path (blocks implementation)

**The design.** It runs "standing, stepping and limp falls" on the new body, keeps getting up on the present body, and
has `Make(down: true)` assert that the switch is off. The code makes that impossible as written.

- **Falls.** `OwnBody.GoDown` puts the articulation away and hands the fall to `PhysicalFall.LetGo`, a different body
  of Rigidbodies and ConfigurableJoints (`OwnBody.cs:1151-1156`). No own-body limp fall exists for step 4 to test.
- **The hand-over keeps the riser's body.** `OnItsFeet` hands the riser's body (made lying, present frames) to the
  keeper (`OwnBody.cs:496-515`). It is only remade at `Remake` after the rise. So the keeper must run on both bodies at
  once.
- **A static is global.** `OwnBody.Anatomical` is a static, and several miners coexist. The body map itself warns:
  "Statics are global ... never vary a body through them".
- **The let-go body's ranges differ.** It hands over poses outside the new table. For example its shoulder allows
  ±95° to the side and 70° of twist (`PhysicalFall.cs:41`), against the new table's −10° of adduction and −80/+70°.
  So "made outside a range" (§3 rule 4) becomes routine, not rare.
- **Wrong layer.** "Until layer 1 exists" (§1.2, §7 step 5) should be layer 3: layers 1-2 never drive a joint.

**Correction.**

- **Keep the frame convention per body**, as a field set at make, with the shim keyed on it.
- **`Make(down)` builds the present body** whatever the switch says.
- **Step 4 must choose.** Either:
  - (a) test limp falls on `PhysicalFall` with its ranges brought to the same table first; or
  - (b) design the own-body fall as a new path: a tone schedule, collision pairs switching mid-fall, GroundTouch
    modes, lie detection, and the remake for the present riser.

  Option (b) is "one body for everything", and it is work.

### F7. The range tables disagree, and the knee still cannot reach the CMU squat (medium)

**The claim.** The joint model says `b3.py` and the let-go body "agree within a few degrees, except their knee and
dorsiflexion".

**The tables.** Against `b3.py:36-41`:

| Joint | Prototype (`b3.py`) | Joint model (soft) | Note |
|---|---|---|---|
| Shoulder abduction | −10/120 | −10/170 | 50° apart |
| Elbow | 145 | 140 | stances fitted beyond the new clamp |
| Neck rotation | ±50 | ±60 | |
| Hip rotation | −35/+40 | −40/+35 | |
| Inversion | ±20 | −15/+25 | |
| Hip flexion | 120 | 125 | |

**The knee.**

- The soft knee is 155 and the hard 162. The CMU deep squat reaches 155-158. The controller can never target it; it
  is reachable only as a transient past the clamp.
- The stances document asks for "160° or more".
- The research notes that the squat works because "thigh presses on calf". Neither design has a thigh-on-own-shin
  contact; `OwnBody.Strike` omits that pair (`OwnBody.cs:480-487`).

**Correction.**

- **One range table** in `BodySpec`, read by both `JointFrames` and the fitter's port.
- **Knee:** soft ≥ 158-160 and hard about 165.
- **Model a load-bearing thigh-calf end range:** a contact pair, or a stop that carries load.
- **Re-fit the atlas** on that table before its numbers are quoted again.

### F8. The fitter's comfort term forbids locking and may manufacture the "heels-up squat" (medium)

**The term.** Comfort penalises the last 12% of both ends of every range (`b3.py:649-656`). With the prototype's
ranges that means:

- knees straighter than about 16°;
- elbows straighter than about 16°;
- dorsiflexion beyond about 20° (the range is −50/30).

**What that does.**

- Weak people bear weight on locked elbows and knees, on all fours, in Gowers and in standing. The fitter cannot use
  that, so arm-supported stances get a higher r. The warning threshold and the ladder are biased toward "too weak".
- A flat-footed deep squat needs more than 30° of dorsiflexion and is penalised above 20°. "Weak Small picks a
  heels-up squat" may be the fitter's artifact, not the body's choice.
- **Layer 0 has the same blind spot.** "At equilibrium the joint never touches a stop" leaves no designed way to rest
  on an extension stop. The chatter check (sign changes within 3° of a hard limit) would flag a legitimately locked
  elbow.

**Correction.**

- Make comfort one-sided (flexion ends).
- Let the LP credit end-range support.
- Give layer 0 a load-bearing extension zone (target clamp at the stop and a small passive spring).
- Re-run the weak-body squats with dorsiflexion at 40° before calling heels-up a property of the body.

### F9. The untoned damping floor turns a limp body into a doll (medium)

**The design.** `c = max(ζ·2√(kI), τcap/ωmax)` with `forceLimit = tone·τcap`. At step 4's 5% tone the damper
saturates above 0.05 ωmax: about 1-1.5 rad/s.

**The effect.** Every joint becomes a dry-friction brake of 5% τcap. That is 4.4 N m at Small's knee, about the shin
and boot's own gravity torque. Limbs would stop where they are instead of swinging: a posed or doll look.

**The precedent.** `PhysicalFall` already tones its damper: `positionDamper = toned strength × Slows 0.12`
(`PhysicalFall.cs:402, 408-409`). That is a constant zero-torque speed of 8.3 rad/s at any tone, and the right form.

**Correction.**

- Use `c ≥ tone·τcap/ωmax`.
- Note that the let-go body's 8.3 rad/s and layer 0's 20-30 rad/s are two speed rules for "one body for everything".

### F10. Rests and gathering are timers, and tiredness runs backwards while getting up (medium)

**The laws.** Rest = 0.6 s · k_H · u^1.5 · (1 + tiredness), and gathering = 0.25 s · k_H · u^1.5. Both are durations
computed in advance. Small at 0.29 strength rests 13 s.

**Why that conflicts.** Luis (October 13) objected to a body that waits "until the timer". The research says rests are
"best tied to the reserve needed for the next stage".

**Tiredness runs backwards.** The own body never calls `PhysicalBody.Worked` (body map §1.5), and `PhysicalBody.FixedUpdate`
rests every group nobody reported (`PhysicalBody.cs:109-116`). So a body pushing at r = 0.9 recovers as if idle. The
"(1 + tiredness)" term reads a state that moves the wrong way, and capacities are frozen at make
(`OwnBody.cs:573-613`).

**Correction.**

- Report work from the own body: group effort = the worst joint's share of its budget.
- Read capacity each step, as the joint model already proposes.
- End rests and gathering when the fatigue state restores the next push's reserve (gated, not timed). Breath and small
  shifts still play during a rest.

### F11. Two unlinked speed models (medium-low)

Layer 0 bounds speed by Ω and β, an Open body attribute. Layer 2 sets the tempo from k_H and a(r) = g(0.9 − r)/r,
which ignores Ω. Two bodies with equal r but different joint speeds therefore get the same planned tempo, and the plan
and the physics can disagree.

**Correction.** Derive the tempo from layer 0's τavail(ω), or put Ω and β into the tempo law. Keep one place for the
speed rule.

### F12. Smaller specification errors that would bite at implementation (low each)

- **The make writes `target = 0`** (`OwnBody.cs:653`). With anatomical frames, 0 is the anatomical zero, not the
  as-made pose. Any joint not yet aimed is pulled toward a straight stand. That covers every joint until the first
  `Aims` or `Still()` (`OwnBody.cs:1002-1012`): a whole physics step if the make lands after that tick's keeper step.
  - Fix: write the targets as `Engine(rested)` at make.
  - `MadeLyingStaysAsMade` would catch it; the design should not leave it to the test.
- **`Rb[i] = Zero[i]⁻¹ · Rb[parent]` gives the forearm the upper arm's offset frame.** It becomes non-principal, and the
  1% off-diagonal assert fires.
  - Fix: apply the rule only across ball joints. Hinge children keep their own principal frames.
- **The gravity feed-forward list is too broad.** "Trunk, head, arms, a leg in the air" is valid only while the
  joint's whole distal subtree touches nothing. The back joint is wrong whenever the hands bear weight, which is most
  of getting up.
- **The stances document's `BodyFrame` uses the present convention** ("made − jointPosition"). Under the joint model a
  knee's bend is −jp. Read joint angles through `JointFrames` only.
- **The `jointPosition` read at `OwnRiser.cs:1160` feeds the learned models' observations.** Switching it to
  `MadeTurn` changes what `SmallRise*.onnx` sees. Since the present riser never runs on the new body, leave the present
  body's read alone (decision 6).
- **BodyWatch "always on" with "tests assert zero"** would fail on the present body at once: the hip reaches 62 rad/s
  after the hand-over, against a whip threshold of 37.5. Scope it to the new body, or record the present body's faults
  without asserting.
- **The step's hip at 24.7 rad/s is "allowed" by Ω_hip = 25** only because it is driven explicitly (F1). Through the
  drive, 24.7 of 25 leaves 1% of the hip's torque.

### F13. Layer 2 gaps that could show as switching or stalls (low-medium)

- **Re-planning at each gate redraws by softmax, with no commitment term.** The route can dither between the squat
  and the bear mid-rise.
  - Fix: add a switching cost, the time to undo the moves already made, and reuse the fall's seed.
- **"The passage is closed beyond 0.06 H" is a hard cut in build space.** The swing sit-up's odds jump to 0. Luis's
  October 13 rule says nothing may switch at a threshold.
  - Fix: a cost that diverges smoothly.
- **The plan ends at "Stand" without saying who raises the body.** Today the keeper does, and reliably only from about
  45° of bow or less with the weight over the soles (memory, October 14). Either layer 2 hands over in such a stance,
  or layer 3 owns the whole rise.
- **The creator's warning uses only the atlas's r.** A body the keeper cannot stand, or that `SetProportions` rejects,
  also cannot rise.
- **The ground-plane re-fit per fall (risk 6) is runtime cost.** If 30 units fall at once, it is 30 × several stances
  × the estimated 20 ms. Time-slice it.

## 2. Contradictions between the documents

| Topic | Body map | Joint model | Stances and strategy |
|---|---|---|---|
| Knee range | hinge stop 150 (`KneeFolds`) | soft 155, hard 162 | fitted at 150; asks "≥ 160" |
| Other ranges | let-go table only | claims agreement with `b3.py` | `b3.py` differs in 6 joints (F7) |
| Capacity shape | magnitude caps per joint | per-axis `forceLimit` with a speed law | 13-direction sphere at τcap; ignores the keeper's 2× (F1) |
| `maxAngularVelocity` | "does not hold" | "not honoured on links" | (none) |
| Time/speed | (none) | Ω, β per joint | k_H and a(r), no Ω (F11) |
| Joint angles | natural = made − jp | flexion = −jp (anatomical) | `BodyFrame` uses made − jp |
| When the present riser goes | (none) | "until layer 1 exists" | layer 3 moves the body; decision 6 says "until the new one stands" |
| Physics centres | low; moving them is Luis's | reads Unity's values, unchanged | the atlas keeps the low centres (risk 7): consistent, but one decision changes all three |

## 3. Missing pieces needed to implement

The corrections above, gathered by layer:

- **Layer 0:**
  - one torque budget per joint (F1);
  - one drive writer (F2);
  - targets written at make (F12);
  - a frame convention kept per body (F6);
  - an own-body limp fall, or `PhysicalFall` given the same ranges (F6);
  - one range table, with thigh-calf support (F7);
  - load bearing at end range (F8);
  - work reported from the own body (F10).
- **Layers 1-2:**
  - a one-sided comfort term (F8);
  - a contract for how layer 3 uses the atlas (F4);
  - conditions for handing over to the keeper, and a warning that includes the keeper (F13);
  - route commitment, smooth closures and a budget for runtime fits (F13).
- **Step-0 tests:**
  - the stiff ankle (F5);
  - the link drag (F3);
  - how long `jointForce` lasts, and its units;
  - the sign and units of `GetDriveForces`, which the budget relies on.

## 4. Understated risks (beyond the findings)

- **Breaking the step.** The shim's float differences fall inside the keeper's narrow windows. Plan a keeper re-search
  and a fresh look by Luis at the step, not "unchanged".
- **Chatter.** In PhysX 4.1 a limited revolute's position is hard-clamped at integration
  (`DyFeatherstoneArticulation.cpp:1202-1208`). A limit tightened inside the pose snaps it in one step, not by erp.
- **Robotic or repetitive.** Each fall repeats the same head lead (0.25 s ± 30%). Stills cannot show repetition, so
  name the judges' questions about motion.
- **Strength rule fixed by accident.** The rule has more in it than the `Rule` options (Need, Thickness, Size) expose:
  - the fixed ratios HipGives 1.5, AnkleGives 0.7 and ToeGives 0.3, which shape the half-kneel-versus-squat result;
  - Ω copied from Small's keeper, with β = ½.

  Present all of it to Luis as the Open strength rule.

## 5. Simpler alternatives

1. **Fix the whips first, on today's frames, behind a switch.**
   - Apply the damping floor through one writer, the slew, nearest-branch targets and a `maxJointVelocity` backstop.
   - Count whips on three miners. It needs no make-time `SetJointPositions` (untried, R1) and no isotropic arm.
   - Do the anatomical frames afterwards, for the hard backstop and the clean coordinates.
   - The joint model's own measurements say most whip events are stored-error releases (163-205 per run); only 1-2
     per run follow a flip.
2. **One capacity shape that both the engine and the LP use.** If per-axis boxes stay, model the box in the LP: it is
   LP-native and cheaper than 13 directions. Otherwise scale the three limits each step so the vector stays in a
   sphere.
3. **One speed rule for both bodies:** PhysicalFall's existing toned `c = tone·τ/ω0`, generalised with Ω and β.
4. **Rests from the existing fatigue model** (`PhysicalBody.Tire`), not new laws: report work and gate on recovery.
5. **Prevent "made outside a range"** by giving the let-go body the same table, rather than repairing it with
   per-step limit widening.
6. **Treat the posture prior as optional.** Build layer 3 first from support targets and the centre of mass. Add the
   prior only where stills show drift (the free-limb pull), not as the stage pose.

## 6. What this critique did not check

- **No Unity run.** Every engine claim above rests on PhysX 4.1 sources. Which PhysX Unity 6000.6 uses is unconfirmed,
  as the joint model says.
- **The keeper's swing figures** (c ≈ 0.52, τ/c ≈ 116) are hand-derived from `OwnKeeper.cs:271, 277, 513-514` and the
  joint model's boot inertia of 0.0055. They are not read in the engine.
- **The "up to 2×" in F1 is a bound.** How often keeper joints actually exceed τcap was not measured. The recordings'
  drive forces were not read.
- **Not reviewed:** `route.py`'s internals, the 765 fits and the sweep. I did not re-run the prototype. The
  heels-up-squat explanation in F8 is a hypothesis to test.
- **Not opened:** the research notes beyond the report's layer-0 sections and the CMU section. Not checked: whether
  Unity's default `maxJointVelocity` is PhysX's 100.
- Nothing here is Game Director acceptance. The strength rule, the speed rule, the range table and the centres are
  Luis's to decide.
