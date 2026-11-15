# The ankle's mend and the own body's natural joint ranges (research, November 15, 2026)

Research only. Nothing in `D:/Dev/WG` was changed and no Unity was run. Code read on `claude/wip-round` `3182e08`.
Recorded runs read: `D:/Dev/WG/TestRuns/CoverLive/cc_es24` (course `.bin` files and `energy_detail_*.txt`), with
scratchpad scripts (`bootoff*.py`, `fisher_n.py`, run with `python -I`). Nothing here is a test result of a mend: no
mend exists yet.

## Start here (revised after the critique; what changed: §8)
1. **Boot off its shin (§4.1):** in c0 and c8 `GoDown` took the ankle point from the physics boot while the let-go
   body is built from the lagging drawn pose; the boot hung 0.55-0.71 m off its shin. Mend C: geometric.
2. **The flip is topological (§1):** a clamp is continuous at 180° only if the target folds back to neutral. Proposed:
   swing-twist, lopsided ellipse, smooth saturation, a fold on the swing **and one on the twist** (the first version
   cut at a pure 180° twist); continuous on all of SO(3). Three cuts outside the limit remain (§1.4).
3. **The speed bound (§4.2), corrected:** a damper floor alone does **not** bound a saturated drive (Small reaches 35
   rad/s on a 140° flip, 45 on 180°). What bounds every jump: the error written within τcap/k of the joint's present
   reading, the damper at least τcap/Ω − k·dt. Modelled: Small at most 19.6 rad/s, no overshoot, full strength held.
4. **The asks (§5):** set in world terms, they leave the ankle's reach; form them in joint space.
5. **`drivenNow` (§6):** as played, Small loads `SmallRisePace.onnx` and `SmallRiseFull.onnx` (the untracked files
   are never loaded); its addition comes after the clamp and is not cleared. `-ownUnheard` is not read by the live test.
6. **Order (§7.2):** B, then A = 1, then both, then C (C removes the damper that made three flips harmless).
7. **The idle (§7.1):** the riser never exists while standing; `keeper.Soften()` restores every drive at the
   hand-over. Proof: tick-by-tick identity with every switch on.
8. Not tested: everything. To read back at build: the engine's coordinates for combined turns, the axis signs.

## 4. The damper (first, because it was found first)

### 4.1 What `lighter` measures, and why it changed a hundredfold: found
`OwnBody.Lighter(i)` (`OwnBody.cs:565`) is the turning weight about the joint of the lighter side: each part as a point
at its transform plus a rod, `m·(|centre − joint|² + L²/12)`. For a boot that is the boot alone, about its ankle point,
`segmentsMade[Foot].TransformPoint(anchors[Foot])`. `anchors[Foot]` is the shin's far end, written in the boot's frame,
at the moment the body is made (`OwnBody.cs:748`). So `lighter[foot]` is fixed by one distance: how far the boot was
made from the end of its shin.

Read from the run (the implied inertia `I = (c / 1.4)² / k` from each detail file's ankle gains):
- In 22 of the 24 courses every try had I = 0.0053 to 0.0079 kg m².
- In c0 from its third try on (right boot 0.65 to 0.67) and in c8 from its second (right boot 0.24 to 0.27), and then in
  every later try of that course. The left boot changed too (c0: 0.0058 to 0.034; c8: 0.0055 to 0.0024).

Read from the recorder (`course_*.bin`, the solved segments the body is made from): in a healthy make the boot's centre
sits 0.19 m along its shin's axis from the shin's centre (the shin's half-length plus a little). At c0's third make the
right boot sat at (−0.54, −0.33, −0.30) m in the shin's frame: 0.71 m away and off the axis. At c8's second make,
(−0.18, 0.36, −0.38): 0.55 m. Every later make of those courses kept such an offset (c0: 7 makes, c8: 13). It never
happened in the other 22 courses.

How it starts, read frame by frame (c0 f1712-1729, c8 f838-851):
1. The right boot whirls on the own body: 45 to 84 rad/s in the last five steps before it goes down.
2. The drawn ("solved") shin lags the physics shin by 0.29 to 0.39 m in those frames (the drawn pose is interpolated
   between steps; at that speed, a step is a long way).
3. At `OwnBody.GoDown` (`OwnBody.cs:1316-1342`), `ankleAt` is taken from the **physics** boot, but the let-go body is
   built from the **solved** pose (`PhysicalFall.cs:515`). `PhysicalFall.Slacken` (`:902-911`) then turns the boot about
   `about = shin.InverseTransformPoint(ankleAt)`, a point 0.3 m and more away from the let-go shin's real end, by
   `slackBy` (up to a half turn).
4. Over the next 0.3 s (`BootsSlackIn`) the boot slides away from the shin: 0.15 m to 0.50 m in ten frames (c0
   f1720-1729), 0.18 to 0.29 in seven (c8).
5. Every later make copies the let-go body's boot offset (`footAt`), and every later going down copies it back. The boot
   stays detached for the rest of the course, until the posed body takes the miner back.

What it does: the own body is then made with a boot hung 0.5 to 0.9 m off its ankle on a lever nothing draws. The
model draws the foot where the skinned shin ends (`MinerBody.cs:577` sets only the foot's turn), so the collider that
meets the ground is half a metre or more from the drawn foot. Its inertia is 50 to 120 times the boot's, the spring
reaches the strength cap (99.6 N m/rad) and the damper rises to 6.9 to 11.4 N m s/rad: the "stronger damper" seen in
three of the seven harmless flips was this fault. c0 and c8 both ended in blow-ups later (c0 f6191, rollup, try 6; c8
f29723, footA, try 14).

This is a fault of its own, not the ankle's range. Its mend is geometric, not a guard (§7, switch C):
- take `ankleAt` and `about` in the **let-go body's own frame**: the solved shin's far end
  (`shin.TransformPoint(0, shinScale.y, 0)` of the body the let-go copy is built from), not the physics boot; and
- at each make, place the boot at its stood offset from the shin's end when the copied offset is not the boot's own
  (a refit of the geometry the body is made from, not a clamp on a motion).
Both keep the whirl's real consequence (the boot is where the limb put it) without carrying a misplaced point forward.

### 4.2 The gains against inertia, the 20 ms step and the force limit
The riser's ankle (`OwnRiser.cs:353-376`), Small, Strength 1, fresh:
- strength cap `has` = 60.86 N m (the drive's `forceLimit`);
- spring k = min(has / (35°·π/180), 10·Rule²/dt²·I) = min(99.6, 10 240·I) with Rule ≈ 0.64, dt = 0.02 s;
- damper c = 0.7·2·√(k·I).

With a healthy boot (I ≈ 0.0065 kg m²): k = 66.6 N m/rad (the inertia cap binds), ω_n = √(k/I) = 101 rad/s, so
**ω_n·dt ≈ 2.0** (exactly √10·Rule). An explicit integrator would be at its edge; the engine's drive is implicit
(PhysX: "the drives are implicit", stable for "very large gains"), so it is stable, but it no longer behaves like a
resolved spring: one step moves the boot most of the way to its target. c = 0.92 N m s/rad.

What a flip does then (revised after the critique, §8 point 5). The engine's drive is implicit: per axis, in one step,
the joint's new speed is (the joint model's own form, `joint_model.md` §5 item 2)

    ω' = (I/dt·ω + k·e) / (I/dt + k·dt + c),   then the step's torque I·(ω' − ω)/dt is held to ±τcap

where e is the error at the step's start. On a free boot from rest, the share of a jump J that becomes speed in one step
is f(r) = r² / (1 + r² + 2ζr), r = ω_n·dt. At r = 2.0 (today's feet) f = 0.51: **a 140° flip becomes 63 rad/s in one
step** (2.44 · 0.51 / 0.02), and a 180° one 81 rad/s. The step's torque is then 20.5 N m, under the cap: it is not the
cap that lets it through but the implicit spring closing half the error in one step. The joint model measured the
riser's τ/c at about 78 and `maxJointVelocity` does **not** bound a joint moved by its own drive (39.7 rad/s under a
bound of 5), so no engine clamp catches it. The recorded whirls (45-84 rad/s) sit in that band.

**The first proposal was wrong.** It said a damper floor `c ≥ τcap/Ω` alone bounds the boot at Ω whatever the target
does. That holds only while the error is at most e_full = τcap/k (52° for Small): beyond it, the spring pushes harder
than the damper can answer. Modelled (scratchpad `ankle_speed.py`, the implicit step above, free boot, no gravity), Small
with c = 3.04 reaches **34.6 rad/s on a 140° jump and 44.5 on 180°** (today 63 and 81). The critic's continuous-time
figure (34 rad/s) agrees.

**The bound that holds for every jump: the error held within e_full, and the floor counting the implicit spring.**

    E  = τcap / k                                      (the error at which the drive gives all it has)
    e' = E · tanh(|e| / E) · ê                         (the error written: smooth, never more than E; ê its direction)
    c  = (c_today⁴ + c_floor⁴)^¼,  c_floor = max(0, τcap/Ω − k·dt)      (a smooth maximum, never under either)
    target = the joint's present reading + e'           (along the shortest turn, §2.2)

Proof, for the drive's own doing (no outside torque): if |ω| ≤ Ω, then |ω'| ≤ (I/dt·Ω + k·E)/(I/dt + k·dt + c) and,
since k·E = τcap ≤ Ω·(k·dt + c), that is at most Ω. By induction the boot never goes faster than Ω from rest, for any
jump, any size, and any speed of the target. A boot already pushed faster by the world loses speed every step. The force
limit only lowers |ω' − ω|, so it cannot break the bound. The torque-speed line this gives is
τ = τcap·(1 − ω/Ω) at a large error: the linear chord of Hill's curve, exact and implicit, which is what the joint
model's rule meant and its `clamp(k·e − c·ω)` form missed (its error clamp to eFull was the missing half).

Worked numbers (the same model; Ω = 20 rad/s):

| Boot | k | c today → bound | E | peak, jump 29° / 88° / 140° / 180° | overshoot | held, torque at 30° / 60° / 140° error |
|---|---|---|---|---|---|---|
| Small, I 0.0065 | 66.6 | 0.92 → **1.75** | 52.3° | 8.9 / 17.1 / **19.2** / **19.6** rad/s (today 12.9 / 39.5 / 63.0 / 81.2) | 0.0° | 31.5 / 49.7 / 60.3 N m |
| misplaced, I 0.25 | 99.6 | 6.99 → 6.99 | 35.0° | 3.4 / 6.2 / 6.7 / 6.7 (today 3.8 / 11.3 / 17.1 / 21.4) | 0.7° | 42.3 / 57.0 / 60.8 |
| misplaced, I 0.67 | 99.6 | 11.44 → 11.44 | 35.0° | 2.2 / 4.1 / 4.4 / 4.5 (today 2.5 / 7.4 / 11.1 / 13.6) | 1.0° | same |

Small's boot already moving at 40 rad/s toward a 140° jump: 21.5 at most, falling. Near the target the joint is a
spring k with damping 1.75: ζ ≈ 1.3, time constant about 26 ms. Held still (a planted boot), the drive still gives all it
has once the error passes about twice E. For Long and Round, Ω scales by (H_Small/H)^β (β Open) and the same formulas
apply with their own k, I and τcap.

**How this differs from `AnkleTurnsAtMost`, which was harmful** (`OwnRiser.cs:418-419`: a boot kept flat on the ground is
asked to turn against its shin as fast as its shin turns; the rate-limited ask lagged, and 0 of 36 stood). That limit
slowed the **target** against its own last value, so a planted boot whose right target moves with the shin was left
behind, and the drive pulled it off flat. The bound here never moves the target away from the wish while the boot is
near it: the error is measured from where the joint **is**, so a planted boot that keeps up with its moving target has
a small error and nothing is changed. Only an error beyond about E is shortened, and then the drive still pushes with
nearly all its strength. What bounds the speed is the drive's own torque-speed line, not the target's pace. One cost
is real: the higher damper (1.75 against 0.92) resists a planted boot turning with its shin: about 5 N m at 3 rad/s
against 2.8 today. It must be measured on the stages where boots are planted (footA, footB, stay).

**Other forms weighed:**
- **The damper floor alone** (the first proposal): fails above e_full, as above. Kept only as the c_floor half.
- **A velocity-limited PD** (stiffness 0, damping c_v = τcap/Ω, target speed v_ref = Ω·tanh(e/e_v) written each
  step). It bounds Small at 19.8 rad/s with no overshoot and brakes at the target by design. But v_ref is explicit (one
  step late), and on a heavy boot c_v cannot track: I/c_v = 0.22 s at 0.67 kg m², and the model overshoots 23-36° (87°
  when already moving). The implicit slew above has the same bound and keeps the heavy boot as today (0.7-1.0°). Ranked
  second.
- **A spring capped by ω_n·dt.** The feet's rule (k up to 10·Rule²/dt²·lighter) sets r = √10·Rule ≈ 2.0, and the
  one-step share f(r) is the whip's direct multiplier: r = 2.0 → 0.51 (63 rad/s on 140°), r = 1 → 0.29 (36), the
  keeper's own r = 0.64 → 0.18 (22). **So the feet's rule is part of the cause.** But lowering it to bound speed would
  cut a planted boot's stiffness tenfold (6.7 N m/rad; 4 N m at 35°): it would trade the whip for a boot that cannot
  hold. And for a boot on the ground the inertia that resists is not the boot's but the body's. The cap is kept as it
  is; the bound above makes its whip harmless.
- **A Hill-shaped strength that falls with speed**, set through `forceLimit` each step from the joint's speed: rejected
  in that form. The engine's force limit is the same both ways, so lowering it at speed also takes away the brake,
  exactly when a whirling boot must be stopped (a muscle lengthening gives 1.5-1.8 F0, Katz 1939). The implicit chord
  above is already Hill's shortening side, linear. The lengthening side can be had by setting `forceLimit` to 1.5·τcap:
  a concentric push is still held to τcap(1 − ω/Ω) by k·E = τcap, while braking may reach 1.5·τcap. That is a change of
  strength, an Open choice: a sub-option, off. Hill's curvature (slope −5 at rest) would need c set from the speed each
  step, explicit: later, if at all.

**The fold's own speed (§1.4).** The target may move faster than the ask (up to 4.3 times in the first fold, 2.4 with
the revised fold at a 110° reach, 1.2 at a 55° reach). Under the bound above that no longer reaches the boot: the
induction holds for any sequence of targets. A fast target is then followed at Ω at most, and the boot visibly travels
there, which is the body's own answer, not a whip.

The inertia that enters √(k·I) is still `Lighter()`. With the bound above, the speed no longer depends on I being right
(only ζ near the target does), which is the robust choice: the speed bound is a property of the drive, not of an
estimate.

### 4.3 Hill's force-velocity relation as a natural damper
Hill (1938): `(F + a)(v + b) = (F0 + a)·b`, with a/F0 ≈ 0.15-0.3 (about 0.25 classically) and b = (a/F0)·vmax.
Normalised (f = F/F0, u = v/vmax, a/F0 = 0.25):

    f(u) = (1 − u) / (1 + 4u)

- At u = 0 the slope is −5: near rest Hill acts like a damper c₀ = 5·τcap/ωmax, five times the linear chord.
- At u = 0.25 Hill gives 0.375 of the strength, the linear rule 0.75: the hyperbola is the stronger brake.
- Lengthening (eccentric) force rises to about 1.5-1.8 F0 (Katz 1939): a muscle brakes better than it pushes.

For the controller: the bound of §4.2 (error within τcap/k, damping counting k·dt) is the implicit, lag-free chord of
Hill's curve. A closer Hill through `forceLimit` per step is rejected (§4.2: the limit is the same both ways and would
take away the brake); its curvature would need the damper set from the speed, explicit and one step late, so it comes
only after the linear chord is measured, if at all. "Slower with size":
under isometric scaling, fibre length and moment arm both grow with L, so a joint's free speed would stay equal; muscle
fibres get slightly slower with body size across animals, and gravity-driven movement follows dynamic similarity
(Froude), angular speeds ∝ L^−½. The joint model's β = ½ is that rule; β is Open.

## 1. A limit that cannot flip

Notation: `q` = the ask relative to stood (`fromStood` in `AnkleCan`), as angle θ about unit axis n; A = 110° today.

### 1.0 Why every clamp flips, whether hard or smooth
The turns form SO(3), a ball of radius 180° whose opposite surface points are the same turn (θ about n = 360° − θ about
−n). Today's clamp maps θ ∈ (−180°, 180°] to clamp(θ, ±A) about n. A turn of 180° + ε about n is read as 180° − ε about
−n, so the target jumps from +A·n to −A·n: 2A = 220° about n, **that is 140° the short way, in one step.** This is not a defect of
`ToAngleAxis`; it is topology: the nearest point of the ball |θ| ≤ A is not unique at θ = 180°, and any map that equals
the identity inside the ball and **stays at the boundary outside it** must break somewhere on the 180° shell. A smooth
saturation such as `θ' = A·tanh(θ/A)` has the same break (102° about n against 102° about −n, at A = 110°).
The only continuous retractions are ones whose value at θ = 180° does not depend on n, i.e. **the identity (neutral)**:
the target must fold back toward neutral as the ask nears the far side. (On a circle: clamp to [−A, A] breaks at ±180°;
the fold f(θ) = A·(180° − |θ|)/(180° − A)·sign θ for |θ| > A is continuous everywhere.)

### 1.1 Option A: choose the side by continuity with the last ask
Keep the sign of the quaternion with `dot(q, q_last) ≥ 0` and read θ unwrapped (0-360° and beyond), then clamp.
- At 180°: no jump while the ask moves continuously; +A about n is held as the ask goes on to 250°.
- But it is path-dependent: an ask that goes once round comes back to stood while the target stays at +A, until a
  rewinding rule (a threshold: Luis's "no switch at a threshold" is broken) puts it back. Asks that jump (c19) still
  jump. The clamp keeps a kink at |θ| = A (the target stops dead).
- Cost: one dot product and one stored quaternion per ankle. Cheapest; fails the principle.

### 1.2 Option B: swing-twist with an elliptical swing cone and a twist range
Choose the twist axis t (the shin's long axis in the boot's stood frame). For q = (w, v):
`twist = normalize(w, (v·t)·t)`, `swing = q·twist⁻¹`; twist angle τ = 2·atan2(v·t, w); swing as a rotation vector σ in
the plane ⊥ t, components (σ_pf about the boot's right axis: dorsi/plantar; σ_ie about its forward axis:
inversion/eversion). Lopsided ellipse, one semi-axis per quadrant:
`ρ = √((σ_pf / a_pf)² + (σ_ie / a_ie)²)`, a_pf = plantar or dorsal reach by the sign of σ_pf, a_ie = inversion or
eversion reach; clamp σ radially to ρ ≤ 1 and τ to [τ_lo, τ_hi].
- Anatomical: each direction has its own reach (plantar 55° is not dorsal 25°), as people have.
- At 180°: the decomposition's own cut is at a swing of 180° (the swing axis is undefined, the twist too). It flips there
  exactly as today unless the asks keep away from it or the fold of §1.0 is added. Radial clamping keeps a kink at
  ρ = 1.
- Cost: about 30 floating-point operations, one atan2, one sqrt per ankle per step. Negligible.

### 1.3 Option C: a smooth saturation toward the range
On the normalised radius: `σ' = σ·S(ρ)/ρ`, with S smooth, S(ρ) ≈ ρ near zero, S → 1 at the edge. (The first version
used a soft knee, identity up to ρ0 ≈ 0.7 and tanh above: C¹, but with a point where the yielding starts. Revised to
`S(ρ) = ρ/(1 + ρ⁴)^¼`, which has no such point: §1.4.) Grows smoothly: the farther toward
the end of its range, the less extra the joint yields, as tissue stiffens toward a person's end of range.
- At 180°: on its own, still the break of §1.0.
- Cost: one tanh. Negligible.

### 1.4 Recommendation: B with C, folding back to neutral at the far side (revised after the critique)
The first version folded on the swing only, and claimed continuity on all of SO(3). That was false (critique point 6):
a pure twist near 180° about the shin has swing 0 (fold 1), and its twist angle τ = 2·atan2(v·t, w) jumps from +180° to
−180° as q crosses w = 0 (or as q is read as −q), so softclamp(τ) jumped from τ_hi to τ_lo, about 30°. The revision
gives the twist its own fold and makes every piece smooth, with no point where anything starts:

    q = (w, v) with w ≥ 0 (the sign fixed; q and −q are the same turn)
    twist = normalize(w, (v·t)·t) = (w_t, …),  swing = q·twist⁻¹ = (w_s, …)
    s = swing angle, σ = swing as a rotation vector ⊥ t,  τ = twist angle in (−180°, 180°]
    ρ = √((σ_pf / a_pf(σ_pf))² + (σ_ie / a_ie(σ_ie))²)        the elliptical radius, lopsided (§1.2)
    a(x) = ½(a₊ + a₋) + ½(a₊ − a₋)·tanh(x / δ),  δ ≈ 5°        each side's reach, blended smoothly across zero
    S(ρ) = ρ / (1 + ρ⁴)^¼                                      the saturation: ≈ ρ near 0, → 1 far out, no knee
    fold(s)  = 1 − (1 − w_s²)^m,   m ≈ 8                       (1 − w_s² = sin²(s/2)): 1 inside the range, 0 at s = 180°
    gtwist(τ) = 1 − (1 − w_t²)^m_t, m_t ≈ 4                    the twist's own fold: 0 at |τ| = 180°
    swing' = exp(fold·S(ρ)·σ/ρ),   twist' = exp(fold·gtwist·Tw·tanh(τ/Tw)·t)    (Tw: the twist's reach, ±15°)
    ask'   = stood · swing' · twist'

Values (scratchpad `ankle_speed.py`): fold at s = 50 / 90 / 110 / 140 / 160° is 1.00 / 0.996 / 0.96 / 0.63 / 0.22; S at
ρ = 0.5 / 0.7 / 1.0 / 1.5 / 3.0 is 0.49 / 0.66 / 0.84 / 0.96 / 1.00. Because S reaches 1 only far out, the reach a is
set where the soft limit should be met (S(ρ) = 1 at ρ → ∞; at ρ = 1 the target is 84% of a), so a is taken about
1.15 times the soft value of §2.1, to be read from the stage histograms (§2.3) and judged.

**Continuity, checked piece by piece:**
- fold and gtwist depend on w_s² and w_t², which do not change when q is read as −q: no cut from the sign.
- At |τ| = 180° (w = 0 with no swing, or any turn whose twist part is a half turn) gtwist = 0, so twist' = identity on
  both sides of the jump in τ: **continuous**, which mends the critique's case.
- At s = 180° the swing axis and the twist are undefined; fold = 0 makes both swing' and twist' the identity whatever
  they are: continuous. Near it the target is about m·cos²(s/2)·a in size, so even the swinging axis moves it only a
  little: it is Lipschitz there too.
- At s = 0 the direction σ/ρ is undefined, but S(ρ)/ρ → 1, so swing' → exp(σ): smooth.
- The lopsided reach was only continuous (its slope changed across σ_pf = 0 or σ_ie = 0 when saturated); the tanh
  blend a(x) makes it smooth.
- The swing-twist split itself is continuous as turns everywhere except s = 180°, where the fold covers it.
- **Not continuous, and outside this limit:** (1) `Aims` takes the angle of `own = toStood · ask'` relative to as made
  and folds it into (−180°, 180°] (`OwnRiser.cs:398-399`): if |toStood| + the reach comes near 180°, that cut returns.
  It is read back at build (|toStood[Foot]| per make; it is near zero after a healthy fall, since `BootsLieAt` is 0) and
  removed by writing the target from the joint's present reading (§4.2, §2.2). (2) `drivenNow` is added after the limit
  (§6). (3) An ask that itself jumps (c19, `LookRotation` near the vertical, §5) gives a target jump of up to twice the
  reach: continuous in the ask, not in time. §4.2's bound makes the boot follow it at Ω at most.

**Speed of the target:** the largest |∂fold/∂s| is 1.23 a radian (at s = 151°), so in the fold region the target
moves at most about 1.23·a times the ask's swing speed: 2.4 at a 110° reach, 1.2 at a 55° reach (the first fold's cos²
ramp gave 4.3 at 110°). §4.2's bound holds whatever the target does.

- Meaning, true to the body: an ask the joint can do is followed; an ask beyond its reach is followed as far as the
  joint's own range allows; an ask that is the foot turned backwards (no side preferred) relaxes toward neutral rather
  than forcing one side.
- **Not invisible at today's reach.** The recorded asks sat beyond 110° for whole stretches (32 of 32 steps in c0 and
  c3, §2.3). With the fold, an ask at 140° gets 63% of the reach, at 160° 22%: those boots go toward neutral where today
  they were held at 110°. So A = 1 changes the boot where today's asks are far beyond reach, not only where it flipped.
- A ranks last (fails the principle), C alone does not mend the flip, B alone keeps the cut: **B + C + both folds** is
  the fit. The joint model's design (swing-twist clamp in the controller, engine limits as a backstop) is the same family.

## 2. The natural ranges for the own body

### 2.1 Legs (degrees from the anatomical position; mean and spread between people)
| Joint, motion | Range in healthy adults | Spread and sources | Joint model §2.1, soft [hard] |
|---|---|---|---|
| Hip flexion | 130-134 passive (F 133.8, M 130.4; 20-44 y) | Soucie 2011, 674 people 2-69 y; falls with age; women more | 125 [135] |
| Hip extension | 17-18 passive (F 18.1, M 17.4) | Soucie 2011 | 15 [25] |
| Hip abduction / adduction | about 40-45 / 20-30 | clinical norms (Roaas & Andersson 1982, 210 hips, men 30-40 y; table not re-read here) | 45 / 20 |
| Hip rotation int / ext | about 35-45 / 40-45 | same; population differences in ext. rotation (Ahlberg 1988) | 40 / 35 |
| Knee flexion | 138-142 passive (F 141.9, M 137.7); kneeling uses 141 ± 12 | Soucie 2011; kneeling ADL study (Asian ADLs) | 155 [162] |
| Knee hyperextension | 0-5 | | 0 [−4] |
| Ankle dorsiflexion | passive: F 13.8, M 12.7 at 20-44 y (24.8 / 22.8 at 2-8 y; 11.6 / 11.9 at 45-69 y); weight-bearing squat 30.5 ± 4.5 | Soucie 2011 **verified** (CDC table, means with 95% CI of the mean); Comfort 2015 **not verified** | 30 [38] |
| Ankle plantarflexion | passive: F 62.1 (CI 60.6-63.6), M 54.6 (53.2-56.0) at 20-44 y; 56.5 / 49.4 at 45-69 y; 58 ± 14 range of flexion sitting cross-legged; 63 ± 14.6 kneeling plantarflexed | Soucie 2011 **verified** (CDC table); Hemmerich 2006 **verified from its abstract** (58 ± 14, Indian subjects); the kneeling figure: abstract found, authors **not confirmed** | 50 [58] |
| Inversion | 32 (22 with the ankle dorsiflexed) | Milgrom 1985, 272 men 18-20 y, **verified by the critic**; **subtalar motion only**, not the whole foot's inversion: as the boot's semi-axis it is an approximation | 25 [32] |
| Eversion | **derived, not measured:** Ball & Johnson give the total rearfoot range under a set torque, 53.2 at 20 y falling to 44.3 at 60 y (100 people, women more, the gap closing with age mostly by less eversion in women); total minus Milgrom's inversion gives about 15-20 | Ball & Johnson 1996, Clin Biomech 11:165-169, **totals verified from the abstract**; the split is this page's | 15 [22] |
| Foot rotation about the shin | about ±15 | joint model | ±15 [±22] |
| Toes (MTP) up / down | about 70-90 / 20-40; kneeling on the toes needs about 90 up | clinical norms (not re-read here) | 80 up / 20 down; the toe hinge is −80/+20 |

Spread (the commonly cited figures, **not re-verified**; Soucie's table gives confidence intervals, not SDs): one
standard deviation is about 5-10° for most motions and up to 15° for plantarflexion in loaded activities (Hemmerich's
± 14); women are 3-8° more mobile than men (Soucie's plantarflexion: 7.5° at 20-44 y, verified); range falls 10-20% from youth to 60; children are more mobile. Inversion and eversion depend on the
ankle's flexion (Milgrom: 32 → 22 dorsiflexed), so their reaches are a coupled cone, not a box: another reason for an
ellipse in swing space.

Others, briefly: shoulder flexion about 165-170, elbow about 145-150, wrist about 70-80 each way, spine flexion
moderate. The model's `ElbowFolds 145` and `KneeFolds 150` sit inside them.

**The rule for a soft and a hard limit** (stated after the critique; a proposal, Open, Luis's):
- **Soft (the controller's range, what the body's own drive reaches): the population mean minus one standard
  deviation** of the passive range, sexes pooled, young adults. Why: the norms are **passive** (a therapist turns the
  joint to its end); a body's own muscles reach its **active** range, which is smaller; and mean − 1 SD is what about
  five people in six reach, so a default body is never asked past what most people can do by themselves. A creator's
  flexibility (Open) moves it by about ±1 SD.
- **Hard (the engine's backstop): the mean plus one to two SD, plus the joint model's margin**, met only under outside
  load: body weight on a kneeling boot, the ground, a fall. That is where loaded ranges such as kneeling (63 ± 15) live.
- The Soucie table gives 95% confidence intervals of the mean, not SDs; the SDs need the paper's full tables (not read
  here). With an SD of about 7-8° (an estimate, **unverified**), plantarflexion gives soft about 50 and hard about 66:
  **the joint model's soft 50 fits the rule**, and the first version's "correction" to a soft 58-60 is withdrawn. Its
  hard 58 is low for kneeling: a hard 65-70 would let a loaded kneel reach what people's do. Dorsiflexion: unloaded
  12-14 is the active range; the soft 30 is a loaded (squat) value, so by the rule it belongs in the hard limit, and the
  soft range should be nearer 15-20 with the squat reached under load. Not changed here: for Luis or a judge.

### 2.2 Mapping onto the own body's frames
The riser's ankle ask is relative to `toStood[Foot+leg]`, the standing pose's ankle turn, which is near anatomical
neutral (foot flat, shin near upright). In the boot's stood frame:
- dorsi/plantar: about the boot's right axis. In Unity (left-handed), `Quaternion.Euler(90, 0, 0)·forward = down`,
  so **+x is plantarflexion** (toes down).
- inversion/eversion: about the boot's forward axis. `Euler(0, 0, 90)·up` points to −x, so for a **right** boot +z
  tips the top medially (eversion) and **inversion is −z**; for a left boot it mirrors: **inversion is +z**.
- twist: about the shin's axis (≈ the boot's up).
These signs come from Unity's conventions, not from a read-back in the game: read them back at build on both boots
(a known pose, the reported angles) before any range is trusted. If `toStood` differs from neutral by more than a few
degrees on any body, the table must be shifted by that difference (the joint model's anatomical frames do that).

**How the range reaches the engine's three coordinates (added after the critique, point 8).** Three frames are in
play: the **ellipse's** (the boot's stood frame: swing about its right and forward axes, twist about the shin), the
**ask's** (`own`, the turn from as made, `toStood · fromStood`), and the **engine's** (a spherical joint's three
coordinates, twist about the anchor's X and swings about its Y and Z, read against the as-made zero). Today `Aims`
writes the rotation vector of `own` (angle × axis, degrees, `OwnRiser.cs:398-406`) straight into X, Y and Z. That is the
same turn as the engine's equilibrium only if the engine's coordinates **are** the rotation vector of the relative
turn in the anchor's frame. What is known: single-axis turns read as the angle about that axis, unwrapped (1.580 rad at
a quarter turn, 6.12 rad at 350°, no jump: `ArticulationFactsTests`, November 4); the joint frames' directions were
checked on both sides (`JointFrames`, 7 of 7). What is **not** known: PhysX's guide says only that with two or more free
axes the motion is integrated "by decomposed quaternions" with drift corrected by extra constraints. For a combined
turn, a 110° diagonal rotation vector (77.8°, 77.8°, 0) is the engine's triple only under the rotation-vector map; under
a twist-then-swing or Euler-like split it is another turn, and the ellipse's edge lands somewhere else. Box limits on
the engine's coordinates, if ever set (§3), are likewise not the ellipse: they are a box in those coordinates, rotated
from the ellipse's axes by `toStood` and the anchor.

The order of operations the controller must use: limit `fromStood` in the ellipse's frame (§1.4); compose
`own' = toStood · ask'`; conjugate into the anchor's frame (`anchorRotation`, read at build); convert to the engine's
coordinates by the map the read-back finds; write it **on the branch nearest the joint's present reading** (the reading
is unwrapped, so a target written in (−180°, 180°] against a reading past 180° would push the long way round), as the
present reading plus the shortest turn to it, slewed by §4.2.

**The read-back, at build, in an `ArticulationFactsTests`-style test** (one shin held fixed, one boot, gravity off,
Small's masses, both sides):
1. For a set of turns R (each axis at ±30, ±60, ±110°; each pair of axes on the diagonal at 30, 60, 110°; a 20° twist
   with a 50° swing; the ellipse's edge points of §2.1), write the candidate triple with `SetJointPositions`, step once,
   read the boot's turn from the transforms, and compare with R. Pass: within 0.5° for every R.
2. From zero, drive to each candidate triple with high stiffness and damping; at rest read `GetJointPositions` and the
   transforms' turn. Both must agree with step 1.
3. Turn the boot slowly along a great circle through 180° and back; record the reading against the rotation vector of
   the transforms' turn, to see the unwrapping and any drift.
4. Read `anchorRotation` and `parentAnchorRotation` of both ankles of all three miners, and |toStood[Foot]| at every
   make of a course.
If step 1 fails for combined turns, the map the readings show (its inverse) replaces the rotation vector in step 4 of
the order above, and the ellipse is trusted only after that.

### 2.3 What the stages and the stance atlas need, and where they conflict
- **The kneel with laid boots** (`under` 0.90, `prop` 0.5, `stay`, `footA`; `OwnRiser.cs:591-632`): each boot laid on its
  top, toes back, `LookRotation(lies, up)·AngleAxis(180, right)·stoodBoot`, in **world** terms. With the shin flat on
  the floor that is about 90° of plantarflexion; with the shin raised (prop 0.5) it is 90° plus the shin's tilt, 110-130°.
  People give 55-63 (± 15). A real foot lies "on its top" at 60-70° because its top slopes from the ankle to the toes;
  the boot is a box, so it needs 90. **Conflict.** Natural answer: the kneel asks plantarflexion toward the range's
  plantar end and lets the boot rest on toes and instep as the shin decides (a person's kneel), or the boot's collider
  gets the foot's sloped top (a body change: aesthetic judges first).
- **footA, the lead foot planted from the kneel**: weight-bearing dorsiflexion of about 30-40 with knee flexion 120-140.
  Within the loaded range (30.5 ± 4.5), at its end for stiff people. Soft 30 [38] fits; a body with less range has to
  lean or step wider: the stance atlas should ask it from the range, not assume it.
- **pike and rollup (flat = 120)**: a flat boot under a shin tilted far forward or back asks dorsiflexion or
  plantarflexion beyond any range. The recorded asks sat beyond 110° from stood for 32 of 32 steps in c0 and c3: not
  natural kneeling but asks formed without reach (§5).
- **What to measure at build:** per stage, a histogram of the ask's swing and twist in the stood frame (the recorder
  has `wayAsks`); every stage whose 95th percentile lies outside the soft range is a conflict for the stage, not for the
  range.

### 2.4 How they hold for any body
Joint ranges are angles, not lengths: they do not scale with height or limb proportion. They vary with age, sex and
flexibility (the spread above), and soft-tissue contact limits deep flexion: a thick thigh and calf (Round) stop the
knee and hip earlier than the bones would. So: one table for every body, times a creator's flexibility (about ±1
standard deviation, Open, Luis's), with deep knee and hip flexion reduced by the limb's thickness only if contact between
parent and child links is not simulated. A body that cannot reach a stage's pose within its range must find another way
(the capacity-driven rise), never be posed past its range.

## 3. Stops on the joint (documented behaviour only)
From Unity's Articulation Body manual and PhysX 5's articulation guide and API (the engine's source is not used):
- A spherical joint has three rotations (twist about the parent anchor's X, swings about its Y and Z), each Free,
  Limited or Locked. Limits are per axis, a lower and an upper value, editable only when Limited. One cannot lock all
  three; locking both swings to make a hinge is discouraged.
- The limits are a **box on the engine's joint coordinates**, not a cone: at the corners the combined swing exceeds each
  axis's limit (by √2 for equal limits). The manual does not say how the coordinates are defined; PhysX says that with
  two or more free axes the motion is integrated by "decomposed quaternions" and that axis drift is corrected by extra
  constraints. The spherical drive's target "must be in range [−π, π]" (PhysX 5.6 API). The joint model §1.1 records
  that the joint coordinates have a branch cut near ±π.
- The drive is "an implicit 1D spring" per axis: `stiffness·(target − position) + damping·(targetVelocity − velocity)`,
  limited by `forceLimit`. PhysX: implicit drives handle large gains, but this "does not necessarily hold true for very
  stiff drives in conjunction with other hard constraints", position limits among them; stiff drives can make the
  solver fail to converge; at large angular velocities "velocity clamps or damping may be required".
- `maxJointVelocity` clamps a joint's speed by a torque, but the joint model measured that it does not bound a joint
  moved by its own drive.

So the two should sit like this:
1. The **controller's soft range** (§1.4) is the joint's range in use: its targets never leave it, so the drive never
   presses a stop by itself.
2. The **engine's limits** are a backstop outside the hard range, with a margin (the joint model: the anatomical
   extreme plus 8-10°), and far from ±180°. They are met only under outside load (a fall, the ground).
3. **Energy at a stop:** a drive pressing into a hard limit is two stiff constraints against each other, which the
   documentation names as the case where the implicit drive's stability does not hold; and a link that reaches a stop
   fast is stopped by the solver's position correction, which can return speed. Both are bounded by §4.2's revised
   bound (error within τcap/k with the damping counting k·dt: the drive's own speed at most Ω = 20 rad/s, under K1's
   30; a damper floor alone does not do it) and by targets that stay inside the range.
4. Engine stops are active while standing and stepping too, so they could touch the idle Luis loves. They belong in a
   later, separate step, after the soft range is measured, and only if a fall shows boots passing the hard range.

## 5. The asks beyond reach
Why the stages ask more than any ankle gives:
- The boot's aim is set in **world** terms: flat along `lies` (the stepping boot, `OwnRiser.cs:482-510`, aimed once at
  the step), laid on its top (the kneel), or turned by way of toes down past 120° (`BootTurn`, `:514-521`).
- `AnkleFrom` (`:531-583`) turns it into a joint ask with `Inverse(shin·rested[Foot])·boot`, using the **asked** shin
  from the leg's solution. Nothing ties the world aim to what the ankle can do on that shin, so the ask lands wherever
  the shin and the aim disagree: 110-180° from stood for whole stretches.
- When the boot is far from its aim (c19: 0.7-1.0 m above it and 121-179° off; c0 f1575-1579: 0.68-0.72 m above and
  167-171° off), small motions of the shin or of `lies` swing the ask across the 180° shell, and `LookRotation(lies, up)`
  is ill-conditioned when `lies` nears the vertical: the ask itself jumps (c19), with no clamp involved.

How to form the ask within reach, for any body:
1. **Ask in joint space, against the shin it will act on.** Keep the world aim as the wish, convert it as now, then
   pass it through the soft range of §1.4. The ankle then turns as far as it can toward the wish, as a person's does.
2. **The kneel's laid boots as a joint ask**: "toward the plantar end of my range, no inversion", so the boot settles on
   toes and instep as the shin lies. The boot's world pose is then the body's own answer, not an aim.
3. **Weigh the aim by nearness**: far from the ground a foot is not yet aimed at its landing. ask = slerp(neutral,
   wish, w(d)), with w smooth in the boot's height d over its aim, e.g. `w = 1/(1 + (d/d0)²)`, d0 a fraction of the
   leg's length (no threshold; any body by its own lengths).
4. **A ground frame that cannot degenerate**: build `lies` projected on the ground plane, and blend smoothly to the
   boot's present heading as the projection shrinks, instead of `LookRotation` of a nearly vertical vector.
Points 2 and 3 change visible poses (the kneel's boots, a stepping foot in the air): aesthetic judges first, then Luis.

## 6. `drivenNow`
- **Where it comes from as played:** `RiseAgent.Heard()` loads models when the Academy steps on its own and
  `RiseAgent.Unheard` is false. `Resources/RiseModels` holds the committed `SmallRiseFull.onnx` and `SmallRisePace.onnx`
  (commit 1ba2e50, October 30) and untracked variants. So, as played, **Small's** get-up adds the learnt policy's turns
  (up to `DrivesBoot` 20° per axis) to the boots in `LearnsAt` stages (footB, pike, rollup); Long and Round have no
  model and add nothing.
- **Which files load, by the load order (`RiseAgent.cs:127-159`), revised after the critique (point 3).** The pacer
  loads `RiseModels/<name>RisePace`: for Small **`SmallRisePace.onnx`** (committed), on its own "Pace" object, pace
  only (it sets `Pace`, never `Driven`). The joints' agent tries `<name>RisePaced`, then `<name>RiseFull`, then
  `<name>Rise`: there is no `SmallRisePaced` (the untracked files are `SmallRise`, `SmallRiseFullP`, `SmallRisePFull`,
  `SmallRisePaceP`, none of which matches `SmallRisePaced` in any case), so it loads **`SmallRiseFull.onnx`**
  (committed) as Full, not Paced. The untracked `SmallRise.onnx` would load only if `SmallRiseFull` were missing. So
  the untracked files do not change what is heard in the test build, which runs the worktree's working copy; they are
  in `Resources`, so they would be packed into a built player, unused. Because the agent is Full and not Paced, its
  stale `Driven` is cleared neither by the pacer (pace-only agents do not touch it) nor on leaving its stages
  (`RiseAgent.cs:250` is the Paced path): the critique's reading holds.
- **`-ownUnheard 1` is not read by the live test.** It is read only in `GettingUpByItsOwnTests` (line 51). A
  `CoverRingLiveTests` run with it is the same as without it: the test needs that argument added (one line) before the
  "unheard" arm of §7.3 means anything.
- **Two defects:** (a) the addition comes **after** `AnkleCan` (`OwnRiser.cs:403`): it is outside the limit; (b) a Full
  non-paced agent returns early outside its stages without clearing `riser.Driven`; only a new riser clears it (it eases
  over `DrivenEases` 0.08 s, `:801`). The recorded 14-23° in c0, c4, c6, c12 and c15 fit that.
- **Should it act?** Luis judged the learnt get-up "goofy" (October 30) and turned to the procedural rise for any body
  (GAME_VISION, line 538). A trained addition that only Small has goes against "one body for everything" and "any body".
  Proposal for Luis: off as played (`RiseAgent.Unheard` true by default; `-ownUnheard 1` exists in `GettingUpByItsOwnTests` only, not in the live test), or, if kept for
  training, added **before** the soft range and cleared on leaving its stages. Switching it off changes what Luis has
  played on Small: his to decide. The mend's tests should run with it both on and off on Small, because it is part of
  "as played" today.

## 7. The proposal and the test plan

### 7.1 The switches (all off by default; built only after Luis's word; revised after the critique)
| Switch | When on | Visible? |
|---|---|---|
| A. `OwnRiser.AnkleLimit` = 0 today; 1 = the fold-safe limit at today's 110° (isotropic §1.4, both folds); 2 = the natural ankle (§1.4 with §2.1's ellipse and twist) | the ankle ask; `drivenNow` added before the limit | 1: **yes where today's asks lie far beyond 110°** (they fold toward neutral, §1.4), and where the flip was; 2: yes |
| B. `OwnRiser.AnkleSpeed` = 0 today; Ω in rad/s, proposed 20 for Small, × (H_Small/H)^β (β Open, ½ proposed) | the riser's ankle: the error written within E = τcap/k of the joint's present reading (smoothly, tanh), the damper a smooth maximum of today's and τcap/Ω − k·dt (§4.2) | boots a little slower and more damped in the get-up |
| C. `OwnBody.BootsFromOwnShin` false | §4.1's geometric mend: `ankleAt`/`about` from the let-go body's own shin end; boot refit at make | falls only, when a boot whirled; and the standing after such a fall (the keeper's ankle gains were made from the misplaced boot) |
| (B′, a sub-option of B, Open) `OwnRiser.AnkleBrakes` 1 | `forceLimit` 1.5·τcap on the ankle: braking up to 1.5, pushing still τcap(1 − ω/Ω) | a change of strength: Luis's |

What they change: only the riser's ankle targets and gains (A, B) and the hand-over geometry of a falling boot (C).

**What must not change: the idle standing Luis loves ("really alive"), the step and standing.** Read in the code:
- A and B live in `OwnRiser`, which exists only while getting up (`OwnBody.cs:924`, made only for a body made down).
  A body made standing has no riser, so A and B cannot run during the idle.
- At the hand-over, `OwnBody.OnItsFeet` (`OwnBody.cs:624-634`) sets `riser = null` and calls **`keeper.Soften()`**
  (`OwnKeeper.cs:220-230`), which writes stiffness, damping (`Floored(stoodDamper)`) and force limit back on all three
  axes of every joint, from the values the keeper read off the drives when the body was made (`OwnKeeper.cs:509`;
  the keeper is made at `OwnBody.cs:892`, before the riser at `:924`, so it never reads the riser's gains). After that
  nothing of the riser writes (its `drivenNow` goes with it). So B's gains cannot linger past the hand-over step.
- Two things do carry over: `riseFrom[j] = riser.Aim(j)` (the riser's last pose aim, from which the keeper's rise
  blends) and, with C, the keeper's own ankle gains after a fall that misplaced the boot. Whether `Aim` returns the
  limited ask with A on is read at build; C's change to the keeper's gains is intended (they become a healthy boot's).

**The tick-by-tick proof, with every switch on:**
1. **The idle:** `OwnBodyTests.StandingAMinuteItIsAliveAndDoesNotWobble` and `AtEaseItTakesNoStep`, each miner, the
   solver's turns at 14 and 4, on the meadow and on the level floor (`-ownFloor 1`), every switch off and then all on,
   with each physics step's link positions and turns written out (the keeper's own `-ownTick 1` lines, as in the
   shove test, extended to every link). **Pass: identical to the last bit, every step, for a minute.** With A and B not
   running and C acting only at a going down, any difference names a leak.
2. **The step:** `ShovedItCatchesItselfWithAStep` and `NudgedItKeepsItsFeet`, the same way: identical.
3. **The hand-over:** in a get-up course, at the first keeper step after `OnItsFeet`, read back every joint's
   stiffness, damping and force limit and compare with the keeper's made values: equal with the switches off and on.
   Write `riseFrom` both ways: equal, or the difference named.
4. Then Luis's own look, side by side with the build he loves, switch on (the idle is a look he loves; a test that
   passes does not say he accepts it).

### 7.2 Order (revised after the critique, point 10)
C restores a normal `lighter`, and with it removes the large damper that made three flips harmless. So C comes **after**
a speed bound that holds (B, as revised) and after the limit (A = 1), never first. Each step is a separate arm, from the
same build, on the same courses, read before the next:

| Step | Switches | Expect, per step (the mechanism) | Expect, per course |
|---|---|---|---|
| 0 | none (baselines) | as `cc_es24` on Small; Long and Round unknown | Small about 9 of 24, if the courses are those of `cc_es24` |
| 1 | B | boots above 30 rad/s by the drive's own doing: none (above 30 only under contact); target jumps over 60°: as many as at 0 (the flips stay, harmless); the energy detail's drive injections fall | fewer blow-ups if the drive's whips caused them. If speeds are bounded and blow-ups do not fall, the energy comes from contacts or toes: a finding, not a failure of B. Watch footA/footB/stay for failed tries (the planted boot is damped more) |
| 2 | A = 1 | target jumps over 60° only where the ask itself jumped (c19-like), none from the clamp; boots fold toward neutral where asks lie past about 125° | unchanged or fewer; visible (to judges) |
| 3 | A = 1 + B | both of the above | the best of 1 and 2, or better |
| 4 | A = 1 + B + C | `lighter[foot]` 0.005-0.008 kg m² at every make of every course; the former misplaced-boot flips under Ω (≤ 20 rad/s) | not worse than 3. **If worse, the bound is not holding**: read the per-step speeds, do not build on C |
| 5 | A = 2 and §5's asks | as 4, and every stage's asks within the soft range (§2.3 histograms) | visible pose changes: aesthetic judges first, then Luis |
| 6 | engine backstops (§3, item 4) | only if a fall shows boots past the hard range | |

C alone is not an arm to ship. If C's own effect must be told, it is run as a diagnostic with a predicted worse result
(the critique's point 10), never as a step. "Measure each change alone" holds for A and B (steps 1 and 2); C is measured
as the difference between steps 3 and 4.

**Forced checks, before counting courses** (a rare event is made to happen, not waited for):
- a free boot on a held shin whose ask is turned through 180° at several speeds: boot speed today about 60 rad/s, with
  B at most 20 (§4.2's model says 19.6); with A, no target jump at the crossing;
- a pure twist through 180° about the shin: with A, no jump in the target (§1.4);
- a going down while the boot whirls at 50-80 rad/s: with C, the boot's offset from its shin at the next make within a
  centimetre of a healthy one.

### 7.3 The test plan
- **The live test picks the miner by `-liveWho`** (`CoverRingLiveTests.cs:91-127`): it matches `MinerChoice` names,
  which in `TheOrdinaryPlace` are `Small`, `Long`, `Round` (scene, line 10018-10020). Long is supported. (Round was run
  that way before, `cc_c3`.)
- **Arguments that do not exist yet in the live test and must be added at build** (test code only; nothing in the game):
  `-ownUnheard 1` (§6); `-riseSet Name=value,…` for the riser's switches (as `OwnRiseBench.cs:395-402` does);
  `-ownBodySet BootsFromOwnShin=1` for C; `-liveStrength <s>` through `MinerWorkPreview.SetStrength`, for "several
  strengths". Until they exist, only the baselines below can run.
- The exact command lines (from `D:/Dev/WG`, nothing else heavy running; the runner's `LIMIT` above the test's own 4 h
  timeout):

      cd D:/Dev/WG
      T=Docs/Technical/AnyBody/tools/unity_tests.sh; F=WonderGather.Tests.CoverRingLiveTests; O=D:/Dev/WG/TestRuns/CoverLive
      A="-ownAsPlayed 1 -ownRate 50 -liveOwn 1 -liveSeam 1 -liveOther 0 -ownPushes 24 -ownPushWays 0,180,90,270 -liveCap 240 -liveEnergy 2"
      LIMIT=15000 bash $T ank0_small "$F" $A -liveWho Small -liveOut $O/ank0_small
      LIMIT=15000 bash $T ank0_long  "$F" $A -liveWho Long  -liveOut $O/ank0_long
      LIMIT=15000 bash $T ank0_round "$F" $A -liveWho Round -liveOut $O/ank0_round
      # after the arguments are added:
      LIMIT=15000 bash $T ank0_small_unheard "$F" $A -liveWho Small -ownUnheard 1 -liveOut $O/ank0_small_unheard
      LIMIT=15000 bash $T ank1_small "$F" $A -liveWho Small -riseSet AnkleSpeed=20 -liveOut $O/ank1_small
      LIMIT=15000 bash $T ank2_small "$F" $A -liveWho Small -riseSet AnkleLimit=1 -liveOut $O/ank2_small
      LIMIT=15000 bash $T ank3_small "$F" $A -liveWho Small -riseSet AnkleLimit=1,AnkleSpeed=20 -liveOut $O/ank3_small
      LIMIT=15000 bash $T ank4_small "$F" $A -liveWho Small -riseSet AnkleLimit=1,AnkleSpeed=20 -ownBodySet BootsFromOwnShin=1 -liveOut $O/ank4_small
      # the same for Long and Round (-liveWho Long / Round; Ω scaled by their height inside the switch), and each
      # arm again at two more strengths (-liveStrength 0.7 and 1.3, the values to be chosen from the panel's range)

  One run at a time; read each result before the next and before saying it. These lines are the plan, not run here.
- Small twice: `drivenNow` as played (models heard) and unheard.
- Per step, the mechanism (far more sensitive than course outcomes): ankle target jumps over 60° in a step; boots
  above 30 rad/s, split by whether a contact touched the boot that step; the energy detail's injections; `lighter[foot]`
  per make; |toStood[Foot]| per make (§1.4, §2.2).
- Per course, the outcome (blow-ups; count courses, not tries). Exact two-sided Fisher power at α 0.05 (`fisher_n.py`),
  baseline 37.5%:

  | Courses per arm | 12 | 16 | 20 | 24 | 30 | 36 | 40 | 48 |
  |---|---|---|---|---|---|---|---|---|
  | to 0% | 0.49 | 0.78 | 0.92 | 0.98 | 0.99 | 1.00 | 1.00 | 1.00 |
  | to 5% | 0.31 | 0.50 | 0.66 | 0.77 | 0.85 | 0.92 | 0.95 | 0.98 |
  | to 10% | 0.19 | 0.31 | 0.43 | 0.53 | 0.63 | 0.73 | 0.79 | 0.86 |
  | to 20% | 0.07 | 0.10 | 0.15 | 0.19 | 0.23 | 0.29 | 0.32 | 0.39 |

  9/24 against 6/24 gives p = 0.53 (chance, as found). **24 courses per arm and miner** tell a full mend (to 0-5%) from
  chance; 40-48 are needed for a drop to 10%; a drop to 20% cannot be told at any practical count, so then the per-step
  mechanism has to speak. 0 blow-ups in 24 courses bounds the true rate under 11.7% (95%); in 48, under 6.1%.
  Runs at a fixed frame rate repeat their courses: arms on the same 24 pushes are paired by push, and an exact paired
  test (McNemar's) has more power than Fisher's; Fisher's table above is the conservative case. Courses that are the
  same course met again are counted once.
- Not decidable by tests: whether the get-up looks natural. Every visible change: aesthetic judges, then Luis.

### 7.4 What was not tested
Everything proposed: no code exists. The speed bound is a model of the engine's implicit drive on a free boot (no
gravity, no contact, the shin held), from the joint model's form; the engine's solver with its iterations may differ,
which the forced check of §7.2 measures. Not read back: the axis signs (§2.2), how the engine's three coordinates
represent a combined turn (§2.2), |toStood[Foot]| per make, whether `riser.Aim` returns the limited ask. Read in the
code, not run: who restores the drives at the hand-over (`keeper.Soften()`). Not verified: Comfort 2015's figure, the
kneeling study's authors, the SDs behind the soft-limit rule, the Roaas & Andersson table, hip rotation and toe norms
(clinical norms, marked so). Hill and dynamic-similarity numbers are from the literature as commonly cited, not
re-fetched.

## 8. The revision after the critique
By a fresh reviser (opus) who did not write the research, from `critique.md`'s 12 points. Code re-read on
`claude/wip-round` `3182e08`; no Unity run; no file changed but this one. Model numbers from scratchpad
`ankle_speed.py` and `ankle_speed2.py` (`python -I`).
1. **Code readings of spring, damper, clamp** (agreed): nothing to change.
2. **Clamp flip arithmetic** (agreed): nothing to change.
3. **drivenNow and which model loads:** §6 now names the files by the load order: the pacer `SmallRisePace.onnx`,
   the joints' agent `SmallRiseFull.onnx` (Full, not Paced; no `SmallRisePaced` exists); the untracked files are never
   loaded. Found besides: `-ownUnheard` is read only by `GettingUpByItsOwnTests`, not by the live test (§6, §7.3).
4. **Boot off its shin** (agreed; run evidence not rechecked by the critic): unchanged. Added: C also changes the
   keeper's ankle gains after such a fall, since the keeper reads them from the made drives (§7.1).
5. **The damper floor does not bound a saturated drive** (refuted, accepted): §4.2 rewritten. The implicit step's
   share f(r) and a model of four forms; the bound that holds for every jump (error within E = τcap/k of the present
   reading, smooth tanh; damping a smooth maximum with τcap/Ω − k·dt), its proof by induction, worked numbers for Small
   and the misplaced boots; how it differs from `AnkleTurnsAtMost` (error from where the joint is, not a lagged target);
   the velocity-limited PD (second: explicit, and cannot hold a heavy boot); the ω_n·dt cap (the feet's rule is part of
   the cause, but lowering it costs a planted boot its strength); Hill through `forceLimit` (rejected: the limit is the
   same both ways and would take the brake; a 1.5·τcap brake as an Open sub-option). §3 item 3, §4.3 and §7.1 B follow.
6. **The fold at a pure twist** (refuted, accepted): §1.4 rewritten with q's sign fixed (w ≥ 0), a twist fold
   `1 − (1 − w_t²)^m_t`, a swing fold `1 − (1 − w_s²)^m`, and a piece-by-piece continuity check. Cuts that remain,
   outside the limit: `Aims`'s own angle fold of `toStood · ask'`, `drivenNow` after the limit, asks that jump.
7. **The fold's own speed** (agreed in part): the revised fold's slope is 2.4 times the ask's speed at a 110° reach,
   1.2 at 55° (was 4.3); the bound of §4.2 holds for any target sequence, so it no longer reaches the boot (§4.2 end).
   Added: A = 1 is visible wherever today's asks lie far past 110° (§1.4, §7.1).
8. **The engine's axes** (agreed in part): §2.2 now says what is known (single-axis readings, unwrapped) and not
   (combined turns), the order of operations from the ellipse to the engine's triple, the branch nearest the present
   reading, and a four-part read-back test at build.
9. **The sources:** Soucie's plantar- and dorsiflexion verified from the CDC table (F 62.1, M 54.6 at 20-44 y; CIs of
   the mean, not SDs); Ball & Johnson's totals verified from the abstract (53.2 → 44.3), the eversion split marked
   derived; Milgrom marked subtalar only; Hemmerich's 58 ± 14 verified from its abstract; Comfort and the kneeling
   study's authors marked unverified. The soft-limit rule stated (mean − 1 SD of the passive range; hard: mean + 1-2 SD
   plus margin) with its reason; under it the joint model's soft plantarflexion 50 stands and the first version's
   "correction" is withdrawn; dorsiflexion's soft 30 is a loaded value (§2.1).
10. **C removes a protection; the order:** §7.2 rewritten: B, A = 1, both, then C; C never first and not an arm to
    ship alone; expected results per step; forced checks before course counts.
11. **Thresholds, idle and hand-over:** every max() is a smooth maximum (p = 4), the soft knee is replaced by
    `ρ/(1 + ρ⁴)^¼`, the folds have no starting angle, the lopsided reach is blended by tanh. The hand-over read in
    the code: `OnItsFeet` → `keeper.Soften()` restores stiffness, damping and force limit on every axis from the made
    values; the riser exists only while getting up. The tick-by-tick proof is in §7.1.
12. **Power table** (agreed): kept. Added: courses repeat at a fixed frame rate, so arms are paired by push (McNemar).

**Still unsure:** the engine's coordinates for combined turns; whether the engine's solver follows the implicit model
closely enough for the bound (the forced check measures it); the SDs behind the soft-limit rule; whether the higher
damper on planted boots costs the get-up tries (step 1 measures it); whether `riser.Aim` returns the limited ask.

### 8.1 After the second critique (`critique2.md`, a fresh sonnet critic)

Its verdict: **sound to propose to Luis as switches, off by default, built only after his word.** It reproduced the
bound's numbers: Small at most 19.2 and 19.6 rad/s on the 140° and 180° jumps, against 63 and 81 today, with no
overshoot, and the drive still brakes at the target. It found the limit continuous in the swing, the twist and the
combined case. None of its points changes the design. They are carried to the build, and the text is mended then:
- **E and the damper floor** are taken from the gains `Drive(j, share)` actually writes: below a share of 0.3 the force
  limit stops following the spring. Recompute them there, also when `Tone` or `StrongerWhileLearning` change `has`.
- **Per axis, not per vector:** with unequal axis inertias a transient may reach about √2·Ω. The test reads per-axis
  joint speed from `GetJointVelocities`, not a link's absolute speed, or Ω is taken at about 15.
- **Targets outside [−π, π]:** after a whirl, `reading + e'` can leave the engine's range. The read-back must include
  such a target. Use the linear difference, never a wrapped shortest turn, or else A = 1 holds only while the reading
  is inside the reach.
- **The units:** `SetDriveTarget` takes degrees, while `jointPosition` and the stiffness are per radian. The read-back
  states both.
- **A fourth source of ask jumps:** the `Slerp` from `aimFrom` (`OwnRiser.cs:845-848`) flips its shortest path when two
  poses are about 180° apart at a stage change. The forced check "ask turned through 180°" includes it.
- **The log:** log the target actually written, not `wayAsks` (which is set before `drivenNow` and before B).
- **Step 4:** C also changes the keeper's ankle gains after a whirling fall. A worse result there may come from the
  keeper's phase with B holding, so the keeper-phase blow-ups are counted separately.
- **Visibility, said plainly:** A = 1 changes every ask above about half the reach, not only the far ones. B softens a
  planted boot's spring smoothly from about 10° of error (2% at 13°, 24% at 52°). So step 1's watch on footA, footB and
  stay is its real risk.
- **`riser.Aim(j)` returns the ask before `AnkleCan`**, so `riseFrom` is the same with A on or off.
- **The switches' types:** `AnkleLimit` and `AnkleSpeed` are declared `float` (the bench's `-riseSet` writes floats),
  `BootsFromOwnShin` is set on `OwnBody`, and the live test sets `RiseAgent.Unheard` itself.
- **Long and Round** are worked numerically at build, before their courses run.
- **Still stated without a source in hand:** the eccentric 1.5-1.8 F0 (Katz), Hill's a/F0 ranges, Soucie's 2-8 and
  45-69 year values, and the Froude line. The second critic could not confirm Ball & Johnson's 53.2 and 44.3 totals,
  which the reviser read from the abstract. Each is checked again before a number from it is built in.

## Sources
- Soucie J.M. et al. (2011), Range of motion measurements: reference values and a database for comparison studies,
  Haemophilia 17:500-507 (CDC normal joint range of motion study, 674 people, 2-69 y).
  The 20-44 y ankle values were read from the CDC's table (https://archive.cdc.gov/www_cdc_gov/ncbddd/jointrom/index.html)
  in the revision: verified.
- Comfort P. et al. (2015), single-leg squat kinematics (dorsiflexion 30.5 ± 4.5°): **not verified**.
- Hemmerich A. et al. (2006), Hip, knee, and ankle kinematics of high range of motion activities of daily living,
  J Orthop Res 24(4):770-781: the cross-legged 58 ± 14 verified from the abstract.
- Kneeling and Asian ADLs (kneeling knee 140.6 ± 11.5°, ankle 63 ± 14.6° plantarflexed): abstract found; authors not
  confirmed.
- Milgrom C. et al. (1985), inversion and eversion in 272 young men (subtalar motion; verified by the critic).
- Ball P., Johnson G.R. (1996), Clinical Biomechanics 11:165-169: total rearfoot range under a set torque, 100 people,
  53.2° at 20 y to 44.3° at 60 y (abstract, verified); no separate eversion figure.
- Roaas A., Andersson G.B.J. (1982), Normal range of motion of the hip, knee and ankle joints in male subjects, 30-40
  years of age, Acta Orthop Scand 53(2):205-208. https://actaorthop.org/actao/article/view/28654
- Ahlberg A. et al. (1988), geographical variations in normal joint motion.
- Hill A.V. (1938), The heat of shortening and the dynamic constants of muscle, Proc R Soc B 126:136-195.
- Katz B. (1939), the force of lengthening muscle.
- Alexander R.McN., Jayes A.S. (1983), dynamic similarity in legged locomotion (Froude).
- Unity Manual, Articulation Body:
  https://docs.unity.com/en-us/engine/current/manual/physics-section/physics-overview/articulations-section/class-articulation-body
- NVIDIA PhysX 5.4.1, Articulations: https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/docs/Articulations.html ;
  PhysX 5.6.0 API, PxArticulationJointReducedCoordinate (spherical drive target in [−π, π]).
- The project: `Docs/Validation.md` (the ankle split, November 15); `Docs/Design/AnyBody/joint_model.md`;
  `Docs/Design/AnyBody/body_map.md` §1.5.
