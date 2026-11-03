# Layer 0: a body that cannot look broken (the joint model)

Design for `OwnBody` on the work branch (`D:/Dev/WG`, `claude/wip-round`, HEAD `8846684`). Nothing there or in the main
project was changed and Unity was not run. **Measured** means this run's reading of the October 30 recordings
(`scratchpad/frames/f120a`, `f120h`, `f120i`: 310,348 own-body frames at 120 fps, physics at 50 Hz) with the saved
scripts, whose outputs are in `scratchpad/anybody/out/`. **Read** means the PhysX 4.1 articulation sources
(`scratchpad/anybody/physx41/`). API names were checked against the installed 6000.6.0f1 `UnityEngine.PhysicsModule.xml`.
Which PhysX Unity 6000.6 uses is not confirmed; 4.1 matches every measured fact, and step 0 (§7) tests each fact first.

> **Step 0 done (November 3, work branch, `Tests/PlayMode/ArticulationFactsTests.cs`, 7 of 7 pass).** A drive's force
> limit is a torque in N m (a tenth too weak, an arm sinks towards the angle where its weight meets the full torque, not
> to hanging). `GetJointGravityForces` put back with `SetJointForces` holds a hung body exactly (the control falls 73 to
> 91 degrees). A body made lying with joint zeros at its standing relation keeps its pose when its joint positions are
> written before the first step (the engine reads every joint as zero until then): no snap. **Corrections to this page:**
> `maxJointVelocity` bounds a joint moved by a force (5.00 rad/s held) but **not one moved by its own drive** (39.7 rad/s
> under a bound of 5): it is no backstop against a drive's whip, so the drive's own damping and slewed targets (§4, §5)
> are the fix, not the bound. `maxAngularVelocity` on a link changed nothing measurable (39.7 rad/s under 2). The October
> 30 boots' bound (`OwnBody.BootTurnsAtMost`, through `maxJointVelocity`) therefore most likely did nothing; the stops
> made that run fail. Drive target speeds are in degrees a second.

## 0. Summary

1. **Anatomical frames.** Each joint's engine zero is the body's anatomical position: limbs straight down, soles flat,
   trunk and head upright. The shoulders are the exception: their zero is mid-range (§2.3). The frames are set with
   `matchAnchors = false`. The as-made pose is then written as joint positions before the first physics step. The
   October 14 attempt left that write out, and the body snapped to standing.
2. **Body frames aligned at each joint's zero** (explicit `inertiaTensorRotation`). PhysX then reports at most π for every
   ball joint, and the coordinate's branch cut lies π from zero, outside every range. Today it lies wherever Unity's
   principal axes happen to put it. The 2π jumps measured on boots, followed by 74-168 rad/s, come from this.
3. **Lopsided ranges per axis** (§2). Hinges are exact. A ball joint's engine limits are only a backstop. The anatomical
   range lives in the controller's target clamp, which uses a swing-twist decomposition.
4. **The boots whip** because the ankle's damping is scaled by the boot's tiny inertia and its strength by the body's
   weight: on Small its full 61 N m lasts up to about 450 rad/s (keeper) or 78 (riser). The fix is damping of at least
   strength ÷ ω_max and a target never more than the full-strength error ahead, which gives a torque that falls linearly
   with speed, enforced implicitly by the engine (§4, §5).
5. **Strength rule and speed rule** are exposed as data and left Open for Luis.
6. **Per-step invariants** run always (`BodyWatch`, §6), and the recorder gains a version 2.
7. Everything sits behind `OwnBody.Anatomical`, which is off by default. The present riser never runs on the new body
   (Luis's point 6; the October 30 stops). The keeper runs on it through a conversion shim, which must reproduce today's
   standing and step before any controller relies on it (§7).

## 1. Anatomical joint frames

### 1.1 What the engine does today (measured and read)

- **Ball joints (3 DOF).** `jointPosition` is the rotation vector of the turn from as made, in the child's frame:
  `jp = Log(rested⁻¹·P⁻¹C)`.
  - The implied rest stays within 0.06-0.08° (99.9th percentile) for the trunk, head, arms and thighs, except in the run
    that blew apart and one arm at 0.7°.
  - The source matches: `computeSphericalJointPositions` takes `jointRotation = n·conj(relativeQuat)`, where `n` is the
    relation between the two links' **body frames** (centre of mass and principal axes), normalised to w ≥ 0. The joint
    rotation itself is not normalised, so |jp| reaches 2π: the largest seen was 267-332°.
  - jp therefore jumps by 2π where the body-frame relation passes 180°, which can be at any joint angle.
  - Measured: 15 such jumps in three runs. Nine fell in the episode that blew apart (f120h, about frame 131,100). Six were
    on boots, at link relations of 84-179°, followed within 10 frames by 74-168 rad/s (five of the six above 130).
- **Limits** apply to each DOF separately, so a ball joint's limits form a box in rotation-vector space, not a cone. The
  pose integration uses exponential coordinates only when all three DOF are free; with two, it uses sequential turns.
  Keep all three free.
- **Hinges** have an integrated angle with no branch. Natural bend = made − jp, in degrees (measured October 30).
- **Distance from the as-made pose.** Because bodies are made lying, |jp| exceeds 90° in 41-49% of frames for the arms,
  thighs and boots. The present drive targets are written on the short branch (`angle > 180 → −360`), while the reading
  may sit on the long one. Either branch flip makes the drive push the long way round, as in the October 22 boot at
  200 rad/s.

### 1.2 The frames (code-level)

New runtime file `Units/JointFrames.cs`:

```csharp
public sealed class JointFrame {
  public Quaternion Zero;    // child-in-parent link relation at the engine zero
  public Quaternion Anchor;  // child anchorRotation: X = flexion axis, Y = along the bone, Z = abduction axis
  public bool Hinge; public int Side;  // +1 right, -1 left: mirrors the signs of Y and Z in the range table
  public Vector3 Engine(Quaternion rel) =>                              // radians, as the engine reports
      Log(Quaternion.Inverse(Anchor) * Quaternion.Inverse(Zero) * rel * Anchor);
  public Quaternion Relation(Vector3 jp) => Zero * Anchor * Exp(jp) * Quaternion.Inverse(Anchor);
}
```

**Zero for any body.** No data or standing pass is needed: it follows from `OwnBody`'s own link conventions (y along the
bone away from the joint, z where the middle joint sticks out).

| Joint | Zero relation |
|---|---|
| hips→trunk, trunk→head | identity |
| trunk→upper arm | 180° about x (y down, z back: the elbow bends back) |
| elbow, knee | identity (straight) |
| hips→thigh | 180° about z (y down, z forward) |
| shin→boot | 180° about z (sole square to the shin) |
| shoulder | the hang × O, where O = 60° of elevation in the plane 35° in front of the side (§2.3) |

The body's own plain stand (`stoodLocal`) is just a pose in these coordinates; for Small it is a knee of about 34°.
`PhysicalFall`'s joints already rest "standing straight", which is the same convention, so one range table can later
serve both bodies.

**`Make` with `Anatomical` on.** This replaces `OwnBody.cs:645-646` and `:658-670`:

```csharp
link.matchAnchors = false;
link.anchorPosition = anchors[i];  link.anchorRotation = F[i].Anchor;
link.parentAnchorPosition = parent.InverseTransformPoint(part.TransformPoint(anchors[i]));  // same point, any pose
link.parentAnchorRotation = F[i].Zero * F[i].Anchor;
// after all links exist, before the first physics step (the October 14 attempt stopped short of this):
root.GetDofStartIndices(start); root.GetJointPositions(q);
for each joint i, dof d:  q[start[i] + d] = F[i].Engine(rested[i])[d];   // as made
root.SetJointPositions(q); root.SetJointVelocities(zeros);
// assert: each link within 1 mm and 0.2° of where it was made (MadeLyingStaysAsMade, §7)
```

- **Anchor** is built as `LookRotation(Z, Y)` from the anatomical axes at zero, so it is always a proper rotation. Side
  mirroring lives in the range signs, never in the frame.
- **Hinges keep** `Anchor = identity` (X is the bend axis). Flexion = −jp, with limits `[−Folds, +PastStraight]`. This is
  `HingesStop`'s form with made = 0.

**Body frames.** Set `automaticCenterOfMass = false` and `automaticInertiaTensor = false`.

- First read the values Unity computed, so nothing changes by default. Moving the centres to the weighing (the head is
  8 cm low, body map §1.7) stays a separate decision for Luis.
- Then set `Rb[root] = I` and `Rb[i] = Zero[i]⁻¹ · Rb[parent]`, and `inertiaTensorRotation = Rb[i]`, with
  `inertiaTensor` taken as the diagonal of the local tensor in that frame.
- The two links' body frames now coincide at each joint's zero, so `relativeQuat = I`. That gives |jp| ≤ π, with the
  cut at a turn of π from zero.
- Check principal axes: thigh, shin and arms (180° about z or x) and the boot (identity) stay principal for their shapes.
- The upper arm, with its offset zero, is not. Give it an isotropic tensor (its along-bone moment raised to the cross
  moment, about 9× on Small). Twist about a thin bone is invisible, and the shoulder rotators still dominate.
- At make, assert that off-diagonal terms are under 1%.

**Converting existing targets.** Today `Aims(j, own)` (in `OwnBody.cs:971`, `OwnKeeper.cs:491` and `OwnRiser.cs:391`)
takes a turn from as made. Under the switch, all three call one shim:

```csharp
Vector3 jp = F[j].Engine(rested[j] * own);   // the same desired relation, in engine coordinates
jp = Range[j].ClampSoft(jp);                  // §3
jp = SlewFrom(current[j], jp, eFull[j]);      // §4: never more than the full-strength error ahead
jp = NearestBranch(current[j], jp);           // only matters if |jp| nears π; kept as a guard
SetDriveTargets(j, jp * Mathf.Rad2Deg);
```

`OwnRiser.cs:1160` reads `jointPosition` directly. It and any other direct reads switch to
`MadeTurn(j) = rested⁻¹·P⁻¹C`, taken from the transforms, which does not depend on the frames.

**Why only behind a switch.** In the October 30 test, stops added to the present riser cut its success from 5-7 of 8
pushes to 1-3 of 8, and its limbs chattered: its poses lie outside any range. Decision 6 keeps it until the new one
stands. So with `Anatomical` on:

- Standing, stepping and limp falls run on the new body.
- Getting up runs on the present body until layer 1 exists.
- `Make(down: true)` with the present riser asserts the switch is off.

## 2. Ranges: twelve joints, per axis

### 2.1 Table

Values are in degrees and give the soft limit (the controller's clamp) with the hard limit (engine backstop) in brackets.
Signs: + is flexion, abduction or inward rotation. The left side mirrors Y and Z.

| Joint | X: flexion + / extension − | Z: abduction + / adduction − | Y: along the bone, in + / out − |
|---|---|---|---|
| Back (hips→trunk) | −20 / +75 [−28 / +85] | ±25 [±32] lateral | ±30 [±38] |
| Neck (trunk→head) | −40 / +45 [−50 / +55] | ±30 [±38] | ±60 [±70] |
| Shoulder ×2 (engine box: §2.3) | −50 / +170 | −10 / +170 (more adduction when flexed) | −80 / +70 [±90] |
| Elbow ×2 (hinge) | 0 / +140 [−3 / +146] | — | — |
| Hip ×2 | −15 / +125 [−25 / +135] | −20 / +45 [−28 / +52] | −40 / +35 [±48] |
| Knee ×2 (hinge) | 0 / +155 [−4 / +162] | — | — |
| Ankle ×2 | dorsiflexion +30 / plantarflexion −50 [+38 / −58] | eversion −15 / inversion +25 [−22 / +32] | ±15 [±22] |
| Toe ×2 (down only, exists) | up 80 / down 20, as now | — | — |

### 2.2 Where the numbers come from

- **CMU, one adult, six rises:** knees 131-158 (the deep squat 155-158), elbows 90-130, hips 139-153. The hip figure is
  root-to-thigh and includes pelvic tilt, which our back carries: hip 125 plus back 75 ≥ 153. The hard knee of 162 makes
  the squat reachable, which today's 150 (`KneeFolds`) does not.
- **Today's let-go body** (`PhysicalFall.cs:40-42`) and the layers 1-2 model (`l12/b3.py`) agree within a few degrees,
  except their knee (145-150, too tight) and their dorsiflexion (30, no margin for a squat on the feet).
- **Ranges are per-body data** in `BodySpec`, this table the default, which covers stiff bodies and children. Whether
  players choose ranges is Open.

### 2.3 Which axes carry the big ranges, and singular points

- **Big ranges** (over 90°): shoulder elevation (220° in all), hip flexion, knee, elbow, back flexion.
- **Shoulder.** Elevation from −50 to +170 cannot fit inside |θ| < π around the hang once twist is added. With the zero
  at O the farthest point is about 115° of swing, at most about 140° with twist: at least 40° from the cut.
- **Hip.** With the zero at standing, the worst corner (flexion 125, abduction 45, rotation 35) is about 138°. If layers
  1-2 need more flexion, move the hip zero to 45° of flexion too, with an isotropic thigh.
- **Swing-twist** about Y is singular only at 180° of swing; every swing stays at or under 140° from its zero.
- **Clamp shape and engine box.** At make, the clamp is computed from the clinical table: sample its boundary (72
  directions × 3 twists), convert to engine coordinates, and fit a lopsided ellipse with one semi-axis per quadrant,
  plus a twist interval. The engine box is that boundary's bounding box plus 10°.
- **Exponential coordinates are not anatomical angles at large turns.** A box limit is exact only along one axis, and the
  solver predicts a limit from `jp + v·dt` with v = ω·axis, which is about 40% off across the axis at a 120° turn. Engine
  limits far from zero would be loose and bouncy, the second reason they are only a backstop.

## 3. No chatter at the stops

What the solver does (PhysX 4.1, `DyFeatherstoneArticulation.cpp`, limit and drive solve):

- A limit is **hard and speculative**: if the next step would cross it, it removes the closing velocity exactly. Once
  past it, it corrects with `erp`.
- There is no restitution and no softness.
- The engine clamps a drive's target into a limited axis's range itself ("no point in having 2 parts fight").
- Chatter comes from a drive that rests at or beyond a stop, and from a pose that starts outside one. Both were present
  in the October 30 test.

Rules:

1. **Targets are clamped inside the soft range**, which sits 5-10° inside the hard limit. At equilibrium the joint never
   touches a stop. Only transients reach it: impacts, falls and pushes.
2. **Engine limits are a backstop at the anatomical extreme plus a margin** (§2.1). For hinges they are exact. For ball
   joints they form the box above. Set them with `twistLock`/`swingYLock`/`swingZLock = LimitedMotion` and with
   `x/y/zDrive.lowerLimit`/`upperLimit`, in degrees.
3. **A soft approach comes from the drive, not from added per-step torques.** Near a soft edge the clamped target makes
   the drive's spring pull inward, and its speed damping (§4) slows the limb before the hard stop. Explicit ligament-like
   end-stiffness would have to stay under about I/dt² to be stable (14 N m/rad for the boot), which is useless.
4. **Made outside a range** (a fall can do that): widen that hard limit to include the pose, as `HingesStop` did with
   `min(made − folds, −1)`, and tighten it back toward the table as the joint comes in, by at most 30°/s and never inside
   the current pose. A limit that jumps inside the pose is corrected in one step by `erp`, a visible yank.
5. **The chatter check** is in §6.

## 4. Strength and speed

**Capacity.** `τcap[j] = Strength × Rule[j](body) × fresh(j)`.

`Rule` is data in `PhysicalBody`: `Need` (today's: 1.6 × the body's own need in a reference posture, so every build has
the same reserve), `Thickness` ((armRadius / 0.047)³ × a reference) or `Size` (cross-section × lever). `Need` stays the
default; the choice is Open (proposal, question 1) and the body maker passes it through. Capacity is read each step, not
only at make, so tiredness can count later (today a body never tires on its own joints).

**Speed.** Each joint has a zero-torque speed, scaled `ωmax[j] = Ω[j] × (H_Small / H)^β`.

- H is the standing hip height. β = ½ (dynamic similarity, like the gait's Froude speed) is the default. β = 0 gives the
  same speed for every size.
- `SpeedRule` is data, and Open.
- Ω is chosen so that today's standing keeper is unchanged where it works. Its effective τ/c is shown for comparison,
  with Small's lighter-side inertias computed as `Lighter()` does.

| Small, rad/s | Back | Neck | Shoulder | Elbow | Hip | Knee | Ankle | Toe |
|---|---|---|---|---|---|---|---|---|
| Ω (zero-torque speed) | 20 | 20 | 30 | 30 | 25 | 30 | **20** | 20 |
| today, keeper's τ/c | ≈19 | ≈18 | ≈30 | ≈82 | ≈23 | ≈40 | **≈450** | — |
| today, riser's τ/c | — | — | — | ≈25 | ≈8 | ≈14 | **≈78** | — |
| measured need: standing and stepping, max | 4.3 | 7.0 | 11.5 | 3.6 | 24.7 | 9.1 | 121.7 (whips) | — |
| backstop `maxJointVelocity` = 3 Ω | 60 | 60 | 90 | 90 | 75 | 90 | 60 | 60 |

- **Every backstop is above every measured speed that is not a whip.** Measured during getting up, 99th percentile: hip
  20-23, shoulder 17-18. After the hand-over: hip up to 62, shoulder up to 43.
- **The catching step is not clipped.** Its hip reaches 20.7 (99.9th percentile) and 24.7 at most. The global 15 rad/s
  cap of October 13 broke it.
- **`maxJointVelocity` is honoured.** It is a per-DOF clamp in forward dynamics and in the limit solve, defaulting to 100.
- **`maxAngularVelocity` is not honoured on links.** PhysX 4.1: "links ... do not support damping, velocity clamping".
  That is why boots reached 154 rad/s under a bound of 20.

**The drive as a muscle.** All of this is set through `ArticulationDrive` (`Force` type). The engine solves it
implicitly, with `x = 1/(1 + dt(dt·k + c)·response)`, so it is stable at any stiffness.

```
k = tone × τcap / eFull                              eFull: 35° new controller; the keeper keeps its 92.7°
c = max(ζ · 2√(k·I), τcap / ωmax)                     I: Lighter() now; optionally M_dd from the engine
forceLimit = tone × τcap
target: |target − current| ≤ eFull (slewed along the turn between them, §1.2)
```

- With the error at most eFull, the drive's torque is `clamp(k·e − c·ω, ±τcap)`. That is at most τcap(1 − ω/ωmax) when
  shortening: **torque falls linearly with joint speed**, and the free speed is at most ωmax.
- The engine enforces this with no per-step torque code and no one-step lag.
- Lengthening torque is capped at τcap. A Hill hyperbola or eccentric gain can follow later, by setting `forceLimit` per
  step from the joint's speed.
- **Inertia** enters only through the √(k·I) term. It keeps heavy chains (back, hip) from ringing, at the controller's ζ
  (0.38 keeper, 0.7 riser).
- **The inertia cap on k** (`Rule²/dt² · I`, kept from the explicit-integration view) is not needed for stability by the
  implicit drive. Keep it for the keeper by default, so its feel is unchanged. The new controller drops it, which gives
  the ankle real stiffness for balance.
- **Effective inertia** comes from the engine:
  `M_dd ≈ GetJointForcesForAcceleration(e_d)[d] − GetJointForcesForAcceleration(0)[d]`. Verify this in step 0, then use
  it in place of `Lighter()` when stances change.

**Gravity compensation.** The verified names in 6000.6 are:

- `GetJointGravityForces(List<float>)` and `GetJointCoriolisCentrifugalForces(List<float>)`
- `GetDofStartIndices(List<int>)` for indexing
- `SetJointForces(List<float>)` or `jointForce` to apply

Set the joint forces every `FixedUpdate`; assume they last one step. The hips are the root, so the forces assume the
hips carry everything: right for the trunk, head, arms and a leg in the air, wrong for a leg on the ground. Feed them
forward only on joints whose load runs to the hips, and leave weight-bearing limbs to layers 1-2, which share the weight
between supports. Keep the budget: `|τff| ≤ τavail(ω)` and `forceLimit = τavail(ω) − |τff|`. The same call also gives
**how near its limit each joint works** (|τg| / τcap), the signal the capacity-driven layers need.

## 5. Why boots whip, and the fix

**Measured.** Each run had 163-205 boot events above 30 rad/s, with peaks of median 38-46 and maximum 105-168 rad/s
(thousands in the run that blew apart). Each lasted 25-42 ms, one or two physics steps. Most came while legs were moved
(`rollup`, `footA`, `footB`, `pike`), and the step had them too (99.9th percentile 71, maximum 122). A coordinate flip
came just before only 1-2 per run.

**Cause, with numbers for Small.**

1. **Strength sized for the body, damping sized for the boot.** The boot's inertia about the ankle is about 0.0055 kg m²
   against an ankle that holds 61 N m: 11,000 rad/s², or 220 rad/s within one step. Damping set by `ζ·2√(k·I)` comes to
   0.135 (keeper) or 0.78 (riser) N m s/rad, so the full 61 N m lasts up to about 450 or 78 rad/s.
2. **The implicit drive closes stored error in about one step.** From rest an unloaded boot reaches
   `v' = k·e / (I/dt + k·dt + c)`, about 26 rad/s per radian of error with the riser's springs. Error builds while the
   ground holds the boot and comes out within 1-2 steps when it lifts, at 30-50 rad/s, as measured.
3. **Branch flips.** The engine's flips (§1.1) and the target's flip at 180° (`OwnRiser.Aims`) make the drive push the
   long way round: most of the cases above 130 rad/s.
4. **No real bound.** `maxAngularVelocity = 20` does nothing on links.

**Fix, all inside layer 0.**

- **(a) Damping** `c ≥ τcap/ωmax`, with Ω = 20 at Small. The ankle's c goes from 0.135/0.78 to 3.05.
- **(b) Slewed targets.** The error is at most eFull, so `v' ≤ k·eFull/(k·dt + c) ≈ 8-12 rad/s` even with the inertia cap
  removed.
- **(c) Anatomical zero and aligned body frames.** The ankle's |jp| stays under 60°, so no flips.
- **(d) Backstop.** `maxJointVelocity = 60`.

The boots keep their real mass. Nothing is slowed outside the drive. A boot slapped flat by the ground still turns as
fast as the contact makes it.

## 6. Per-step invariants (always on)

Add `Units/BodyWatch.cs`: runtime, cheap, with no `UnityEditor`. It reads each physics step through `FixedUpdate` (the
step before) and counts faults by kind and stage. It keeps the first 20 of each kind with step and stage, and logs them
in development builds. Tests assert zero.

`FrameRecorder` moves to version 2. It adds each joint's anatomical coordinates, its target, and its drive force (from
`GetDriveForces`), so offline scans see the same faults.

| Kind | Fault when |
|---|---|
| `range` | a joint is outside its hard range by more than 2°, or a hinge bends the wrong way by more than 5° (the October 30 "broken" arms) |
| `whip` | a joint's speed relative to its parent exceeds 1.5 ωmax (relative, not world: a falling body spins legitimately) |
| `flip` | the per-step change in jp exceeds the backstop × dt + 0.05 rad, or a ball joint's |jp| exceeds π − 20° |
| `target` | a written target is outside the soft range, or more than eFull + 1° from the joint |
| `torque` | a drive force exceeds 1.01 × its `forceLimit` (this also checks the units, §7) |
| `chatter` | a joint's speed changes sign on 4 consecutive steps at more than 0.5 rad/s within 3° of a hard limit, or the body's relative speeds don't settle while it stands supported and unasked |
| `apart` | anything is not finite, a link is farther from the root than 1.5 × the body's height, an anchor separates by more than 1 cm, or a part is more than 3 cm under the ground |

## 7. Implementation order, tests, risks

Every step has PlayMode tests that run in the worktree (`D:/Dev/WG`). The suite runs alone. Count courses, not runs, and
report what was not tested.

**0. Engine facts** (test scene only; no gameplay change):

- `ForceLimitIsATorque`: a 1-DOF arm held level holds at 1.1 × m·g·r and falls at 0.9 ×. The source scales limits by dt
  only under a flag, and if they were impulses every strength would be 50 times larger.
- `MadeLyingStaysAsMade`: §1.2's write keeps every link within 1 mm and 0.2° through steps 1, 5 and 50, with gravity off
  and on.
- `JointSpeedBoundHolds`: `maxJointVelocity` holds and `maxAngularVelocity` does not.
- `GravityForcesHoldAHungBody`: with an immovable root and zero stiffness, the feed-forward holds the pose to within
  1°/s. This checks indexing, sign and units.
- `NoBranchJumpInRange`: sweep each joint across its range, corners included, and expect a continuous jp.

**1. JointFrames** (EditMode): anatomical to engine and back at 1e-4 rad; the swing-twist soft clamp keeps the direction
and never leaves the range; the box and ellipse fit from the table; the left and right mirror.

**2. Make behind the switch.** Explicit mass properties equal Unity's within 1e-4 apart from rotations; the upper arms
are isotropic; backstop limits and speeds are set; the shim is used in the three `Aims` methods.

**3. One body stands and steps.** On Small, Long and Round, at Strength 0.5, 1 and 2, compare the anatomical body with
the present one: standing 20 s, the four-way shove, catching steps, and `BodyWatch` at zero. Pass means within
run-to-run spread, counted in courses. Luis then sees the step: he likes it and has not accepted it, so it must not
change.

**4. Limp falls on the new body** (drives at 5% tone, then it lies): zero `range`, `chatter`, `whip` and `apart` faults,
across 30 courses × 3 miners plus sampler extremes.

**5. Hand-over to layer 1.** `BodyWatch` stays on for the rest of the project.

**Risks**

- **R1.** The joint-position write at make may be ignored or arrive late, and the body snaps. Fallback: keep frames as
  made. Hinge stops are exact relative to made; ball joints get the soft clamp only, with no engine box.
- **R2.** The speed damping may change the keeper's feel through the knee and elbow, where c rises 1.3-2.7×. `max()`
  keeps today's damping wherever it is larger. Measure in step 3, and if needed raise Ω for the knee and elbow toward
  today's 40 and 82.
- **R3.** The isotropic upper arm, or explicit frames, may change dynamics. It is measured in step 3. The alternative is
  a zero at the hang with elevation capped at 150°.
- **R4.** The box and ellipse are approximations at combined extremes, so a pose can still look wrong. The every-frame
  reading and the judges decide, and the table is data.
- **R5.** The engine may not be PhysX 4.1. Every fact above has a step-0 test.
- **R6.** Feed-forward on loaded legs would be wrong (§4).
- **R7.** Ranges and speeds were set from one adult and three miners. Children-like and giant bodies need the sampler
  (body map §3.4) before Ω, β or ranges are trusted.

## 8. Not done, not tested

- No Unity run, and no code written.
- Small's inertias and gains are hand-derived from the code's formulas, not read in the engine.
- The ranges are proposals; nothing has looked at them yet.
- The boots' readings disagree with their parts' relation in about 1,200 single steps per boot, against at most 25 for
  any other joint. The cause was not established; sampling at whip speeds is likely.
- `bodyframes.py`, which looks for the principal axes Unity chose, was not rerun.
- Nothing here is Game Director acceptance. The strength rule, the speed rule (β) and whether centres move to the
  weighing are Luis's to decide.
