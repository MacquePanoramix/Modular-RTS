# Adversarial review of the AnyBody port (layer 1: stance fitter + reserve LP)

Reviewer: an independent agent that wrote none of the port. November 4, 2026. Scope (as asked): build and run every
test; numerical agreement with the prototype (re-run what is doubtful); kinematics and derivatives against finite
differences; the LP on degenerate and infeasible cases; Unity compatibility (C# 9, .NET Standard 2.1); allocations in
hot paths; thread-safety; whether the interface serves layers 2 and 3 and the game's later data. Small bugs are fixed
directly (each below); larger problems are ranked.

Machine state: a Unity PlayMode suite (`-runTests`, D:/Dev/WG) ran the whole time, so every build used `-m:1`, one
test process ran at a time, Python ran with `OMP_NUM_THREADS=1`; timings are under that load. No Unity process was
touched, nothing under Assets/ProjectSettings, nothing committed. The review's own harness (C# console + Python) lives
in the session scratchpad (`review/cs`, `review/py`), not in the port; what it showed is written here.

**Status: review complete** (this file is the result; the summary returned to the lead is at the end).

## 1. The port's own tests, re-run

| run | result |
|---|---|
| `dotnet build -c Release -m:1 --no-incremental` (Core, Body, Reserve, both runners) | 0 warnings, 0 errors |
| Reserve.Tests, full, before the fixes | **5072 passed, 0 failed**, 23 s |
| Body.Tests, full (`all`), before the fixes | **21541 passed, 0 failed**, 43 s; fit verdicts 293 match / 53 better / 83 other optimum / 21 worse, as README |
| both again after the fixes below | 5072 / 0 and 21541 / 0; every printed number identical apart from timings (diffed) |

The agents' reports reproduce exactly.

## 2. Independent checks (new data the port's tests never used)

| check | how | result |
|---|---|---|
| kinematics at random poses | 84 poses on 7 bodies, every angle uniform over 1.3x its range (past the stops), root anywhere (z -0.5..1.5, yaw/pitch/roll +-180); b3.fk / all_contacts (15 support kinds, both hand-on-thigh variants) / effort_forces / poly_margin in Python vs C# | points 6.7e-16 m, rotations 4.4e-16, link centres 6.7e-16 m, COM 5.6e-16 m, lowest 6.7e-16 m, contacts 8.9e-16, effort forces 9.6e-12 W, effort residuals 8.6e-13, base margin 3.3e-16 m |
| residuals at random points of the box | 432 points: 3 bodies (Small, Round, Giant) x the 45 batch stances + 6 never fitted by run3 (onekneel L/R, halfkneel R both, halfkneel L toes, halfkneel_up thigh, squat hands), uniform inside the bounds and near x0; b3.solve_stance's own resid closure vs StanceProblem.Evaluate | 0 length mismatches; every family within 1e-8 relative **except `onekneel`** (1.3e-5, see F6) |
| AD Jacobian vs finite differences | the same 432 points (not near saved fits), central differences at 1e-5 and 1e-7 plus one-sided at 1e-6 to tell kinks from defects, relative error per entry | 1.3 M entries, 424 k non-zero: median 9e-10; hull rounding OFF: 3 entries off where all differences agree, all in `onekneel` (F6); with rounding ON (production) 16 more in the base-margin rows, all explained by the 1e-9 m rounding (they vanish with it off). No defect in kinematics, contacts, targets, effort split or margins |
| LP edge cases vs HiGHS | 203 problems: 7 real C# fits (incl. self-contacts, forearms, one knee) x 29 variants: shifts, translations to 1e5 m and in z, mu 0 / 0.05 / 3, capacities x1e-4 / x1e4 / spread 1..1e4, one capacity 1e-6 or 0, every contact twice, a single contact, only self-contacts, no contacts, normals tilted 15/50/89 deg and pointing down, mass x1e-3 / x1e3, no joints, hinge axes zero. b3.reserve_lp itself solves each (the StaticsProblem turned back into a K and C its torque_maps/equilibrium read) | before the fixes: **zero capacity wrong** (F1); after: 196/203 agree to 1e-6, the 7 others are "no joints" (b3 raises; C# gives S = 0, sensible) and one marginal line support (F6). Translations: S within 7e-8 relative at 1000 m (F9) |
| the chain on 4 bodies the prototype never saw | Tiny, Lanky, Stocky, Frail x 45 stances fitted by C#, + 68 let-go subsets = 248 problems: C# S / feasible region vs b3.reserve_lp / b3.feasible_region(ndir=12) on the same problem | held or not 248/248; S within 1.3e-10 (220 held); feasible-region depth identical, 175/175 (0 m) |
| threads | 24 fits + LPs + feasible regions on 8 threads at once vs one thread | 24/24 bitwise identical |
| determinism | the same fit twice | bitwise identical |
| allocations | GC bytes per call after warm-up (below) | see F3, F5 |

## 3. Small fixes made (all verified; both suites unchanged afterwards)

1. **`src/Body/Effort.cs`: the Jacobian broke when the AD tape grew during the effort split** (latent). The gradient
   section cached `tape.Buf` in a local, then called `tape.Alloc()` (and D operations), which can replace the
   buffer; later writes went to the stale array. Shown by starting a StanceProblem with a small tape: 4 of 42
   evaluations threw IndexOutOfRange/ArgumentException (others could be silently wrong). Masked only because the
   default tape (8192 rows) is 4.5x the largest use (1802 rows); a stance with ~5x more contacts would have hit it.
   Fix: re-read `tape.Buf` after every Alloc; `AddRow` reads the source row from `d.T.Buf`. Now 42/42 bitwise equal
   to an unconstrained tape.
2. **`src/Body/AD.cs`: Tape starts at 2048 rows, not 8192** (2 MB -> 0.5 MB per StanceProblem, i.e. per
   `StanceFitter.Fit(b, st)`); safe now that growth works (fix 1). Max use measured: 1802 rows (17 bodies x 48 stances).
3. **`src/Body/Kinematics.cs`: no garbage per evaluation.** `Forward` built 14 `params int[]` masks and 12 joint-name
   strings per call: 1000 B per residual evaluation (~150 kB per fit, every LM trial). Now constants: 0 B per
   `Evaluate`, with or without the Jacobian.
4. **`src/Body/Margin.cs`: NaN input threw IndexOutOfRange** (`kmin` stayed -1). Now NaN: `Evaluate` returns NaN
   residuals and `Fit` from a NaN start ends with status "nan", cost +inf, as the LM intends.
5. **`src/Reserve/ReserveSolver.cs`: a joint of zero capacity broke the LP.** Rows were scaled by
   1/max(cap, 1e-9 W), so cap = 0 gave entries near 1e9: on 7 real fits, 6 of 7 "a hip's capacity 0" and 3 of 7
   "worst joint's capacity 0" came back Numerical, and one (Long, on its forearms) **Infeasible**, where HiGHS holds
   the pose (S 0.06-0.30). The floor is now `CapacityFloor` = 1e-3 W x 1 m (0.1-2.5 N m for these bodies; every
   real capacity is above it, so nothing else changes): 14/14 agree with HiGHS to 2e-16. With it, `Evaluate` treats
   a zero-capacity joint's torque within 1e-9 W m as zero, so R/Worst/Residual are no longer +inf at a held pose.
6. **`src/Reserve/ReserveSolver.cs`: invalid input gave a confident wrong answer.** A NaN centre of mass came back
   Optimal, held, S 0.043 (the pose's true S 0.062); a NaN contact point Optimal with S = NaN; a NaN capacity
   Optimal (the joint ignored). `Prepare` now throws ArgumentException for non-finite data, negative capacity or
   friction, a zero normal, or a ContactSign of the wrong length (as it already did for a non-positive mass), and
   `Reserve/Solve(shift)` for a non-finite shift. Also: `Prepare` no longer allocates (0 B, was 120 B).

## 4. Findings, ranked (not fixed: each is a decision or more than a small change)

**F1 (fixed above, listed for the record).** Zero-capacity joints: see fix 5. Worth a test in Reserve.Tests.

**F2 - High (for the acceptance gate, not the code). The design's first evidence step gates on r, which is not
unique.** stances_strategy.md section 5: "match the prototype's r and margins within 0.02". r (full-norm max share
at the force split found) depends on which optimal vertex a solver returns: res_miners2 passes with max |dr| 0.0198
(a margin of 0.0002), res_miners.json misses 3 of 121 (0.023-0.026); with CostJitter 0 or Bland at once the port
gets 237/243, and HiGHS's own interior point vs its dual simplex misses 4 of 122. So "within 0.02" passes or fails by
vertex luck. S (b3's LP optimum) is unique and matches to 5e-11 (and to 1e-10 on new bodies here). Recommend the gate
be restated on S (and margins, which are exact), with r reported, or on the unique full-norm option (`Directions`).
The Game Director's / lead's call; nothing to change in the code.

**F3 - Medium (layers 2-3). Allocation in the non-LP hot paths.** After fixes 2-3: `Evaluate` 0 B, `Reserve(shift)`
0 B, `Prepare` 0 B, but `Solve(shift)` 0.5-0.65 kB (result + 3 arrays), `Statics.Build` 16-20 kB per call (Kin, lists,
StaticsProblem, 14 JointLoads, sign arrays), `StanceFitter.Fit(b, st)` ~640 kB (a new StanceProblem with its tape,
Lm buffers). Fine for the atlas (once per body), not for a layer-3 controller that rebuilds the statics every frame:
add a `Statics.Build(..., StaticsProblem reuse)` / `FromKin` into a reused object, and a `Solve(shift, ReserveResult
reuse)`, before layer 3. Unity's Boehm GC (non-generational) makes per-frame garbage costlier than .NET's.

**F4 - Medium (layer 2, design section 6.6). The fitter assumes level ground at z = 0.** Contacts are pulled to
z = 0 (`300 p.z`), clearance is against z = 0, every ground normal is +Z (`Supports.Add`). The LP side already takes
any contact height and normal (checked: normals tilted 15/50/89 deg and translations in z agree with HiGHS), so the
gap is only in `Supports`/`StanceProblem`: a ground plane (height, normal) per support is needed before plans on the
game's ground (design: "the fitter must take the ground plane under each support").

**F5 - Medium (the game's later data).** What the interface can and cannot carry:
- can: each link's measured mass, centre and inertia (`SetLinkMassProperties`, verified: the statics follow);
  strength per joint and side (`Cap*`), so strength per region maps in; capacity "now" (tiredness) by editing
  `JointLoad.Capacity` and `Prepare` again (S scales as 1/k for a uniform factor, verified to 1e-8).
- cannot: (a) **direction-dependent strength**: a ball joint has one capacity for all 13 directions, so "hip extensors
  stronger than abductors" cannot be said, although each direction is its own LP row and would take its own cap
  cheaply; (b) **asymmetric lengths**: one ThighLength/ShinLength/arm length for both sides (masses and capacities are
  per side, lengths are not), so a body with one short leg cannot be built; (c) friction is a constant (0.6, hand on
  thigh 0.5; the game uses 0.7/0.8); (d) ranges are per joint kind, not per side. (a) and (b) touch BodyModel,
  Kinematics and Statics: a layer-0/BodyFrame decision (design section 5), not a review fix.

**F6 - Low. Line supports.** When every ground point lies on one line (a single shin: `onekneel`; also a seat alone,
a ball or heel alone), the equilibrium has rank 5, the effort KKT is singular and the port regularises it (-1e-9 max|K|
on the multipliers) where b3 uses lstsq: residuals differ by up to 1.3e-5 relative, and the regularised system
(condition ~1e9) is noisy enough that finite differences disagree with AD there (the 3 entries in section 2). The LP
verdict on such a support is tolerance-dependent too (zero-width feasible set): Small "one knee" with mass x1e-3 is
not held in C#, held by HiGHS (s 1e-4). None of the 45 batch stances is affected; any stance meant to be held should
have at least three non-collinear ground points, or the contact patch should be modelled.

**F7 - Low. Unity integration notes.**
- C# 9 / netstandard2.1: clean. No records, init setters, file-scoped namespaces, global usings, interfaces or default
  interface members, unsafe/Span/stackalloc, System.Numerics or UnityEngine; the libraries compile against the
  netstandard2.1 reference assemblies with LangVersion 9 and 0 warnings, so no API outside it is used
  (`HashCode`, `Array.Empty`, `FormattableString`, value tuples are all in netstandard2.1). Not compiled by Unity
  itself (a Unity job was running; not touched).
- `WonderGather.AnyBody.Pose` collides with `UnityEngine.Pose`: any script with both usings gets CS0104 on `Pose`.
  Rename (e.g. `PoseVars`) or use an alias when moving in.
- Frames: the port is right-handed, x forward, y left, z up; Unity is left-handed, y up. The BodyFrame adapter
  (design section 5) must convert positions and rotations (an improper map) and the 34 Euler-chain angles to the
  ArticulationBody drives; nothing for that exists yet (expected).
- Bitwise equality between .NET and Mono/IL2CPP is not promised (Math.Sin/Cos/Exp in the last bit), and the LM picks
  basins chaotically, so an atlas cache keyed by spec hash should be built on the runtime that uses it.
- Speed is measured on .NET 10 RyuJIT only (fits 30-125 ms, LP 0.3 ms); Mono in the editor is typically several times
  slower. The atlas (45-120 fits per body) belongs on a worker thread, as the design says.

**F8 - Low. Shared mutable statics.** `Margin.RoundHull` is a public static bool read in the hot path (the tests flip
it); flipping it while fits run on other threads changes their margins by up to 1e-9 m. Also mutable through
`static readonly`: `BodyFactory.Small/Long/Round` (public fields), `PrototypeBodies`, `StaticsProblem.BallDirections`,
`StanceFitter.Noise1`. Suggest a per-StanceProblem option for the rounding and read-only views for the tables.
Otherwise thread-safe as documented: one StanceProblem / ReserveSolver per thread; BodyModel, Stance and
StaticsProblem are only read (24/24 parallel fits bitwise equal).

**F9 - Low. Far from the origin.** Translating a problem to (1000, 500) m costs precision (S within 7.4e-8 relative,
against 1e-14 near the origin); verdicts held. Build StaticsProblems in a body-local frame (Statics.Build does: the
root's x, y are 0) and shift COM queries, rather than passing world coordinates from the game.

**F10 - Low. Missing for layer 2, cheap to add later.** (a) LP sensitivities: dS/dshift (and dS/dpose) come free from
the dual's solution but are not exposed; layer 2's Gauss-Newton over continuous settings would otherwise
finite-difference warm-started LPs (fine at 40 us each). (b) The fit's cost has no reserve term (README "where it
stopped", 4): a lower-cost fit can hold worse (Long "hands and knees, ready": cost 2.37 not held vs the prototype's
4.74 held). (c) `FeasibleRegion` keeps b3's absolute tmax 1.5 m and 12 directions; fine for these bodies (the
Giant's boundary is at most 0.26 m) but not scaled by body size. (d) No body/stance hash or library version for the
atlas cache. (e) The LM evaluates the full AD Jacobian for every trial step, rejected ones too (a values-only trial
first would save ~20-30 % of fit time); not needed for correctness.

**F11 - Low, documentation.** The AD.cs header claims an evaluation "allocates nothing on the GC heap once the tape
has grown"; that was not true before fix 3 (it is now). README's "Into Unity ... it allocates only when a problem
grows, plus the result's arrays" holds for the LP (verified 0 B).

## 5. What was not tested

- Nothing ran inside Unity (Mono/IL2CPP compile, speed, determinism there); see F7.
- No idle-machine timings (a Unity suite ran throughout).
- The fitter's basins against the prototype were not re-judged beyond re-running the agents' 450-fit comparison;
  whether the 21 worse fits matter is a design question (README "Fits against the prototype").
- The full-norm option (`Directions = HalfSphereDirections(200)`) was re-run only through the port's own test.
- `LinProg` (the scipy-form front end) only through the port's 321 random LPs.
