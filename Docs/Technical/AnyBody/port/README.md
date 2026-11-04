# AnyBody port: the stance solver in C#

The capacity-driven getting up for any body (Game Director, November 3, 2026), layer 1: for each stance people pass
through, fit a pose for *that* body, and its reserve r by a linear program over contact forces in friction pyramids.
This is the Python prototype (`../proto/b3.py`, `lib3.py`, `run3.py`) ported to engine-independent C# 9 /
.NET Standard 2.1, so it can move into Unity unchanged (no records, init-only setters, file-scoped namespaces, global
usings, default interface members; no UnityEngine, no System.Numerics in public APIs). Namespace `WonderGather.AnyBody`.

Two halves, each by its own agent, meeting at `src/Core/StaticsProblem.cs` (see `INTERFACE.md`):

| folder | what | owner |
|---|---|---|
| `src/Core` | Vec3, Mat3, Quat, BodyLinks, ContactPoint, JointLoad, StaticsProblem (the interface) | body agent |
| `src/Body` | the body model and factory, kinematics, poses, the stances (lib3 families), their residuals, the LM fitter, effort split, statics, margins; forward-mode AD (`AD.cs`) for exact Jacobians | body agent |
| `src/Reserve` | the reserve LP (dense simplex), feasible region, support polygon | LP agent |
| `tests/Body.Tests` | console runner for Core + Body (no offline NuGet, so no xUnit) | body agent |
| `tests/Reserve.Tests` | console runner for Reserve | LP agent |
| `reference/` | golden data exported from the prototype by `export_*.py` (only `body_ref_bodies.json` is committed; the large files are made again) | both |
| `results/` | the fit comparison tables written by the tests (`fit_all.txt`, `fit_all.csv`) | body agent |

## Build and test

```
# the golden data (once; Python 3.12 + numpy + scipy), from port/reference:
python export_body.py          # body_ref_bodies.json, body_ref_cases.json, body_ref_fk.json
python export_reserve.py       # reserve_cases.json        (LP side)
python export_lp_random.py     # lp_random.json            (LP side)

# Body side (from port/tests/Body.Tests):
dotnet build -c Release -m:1
dotnet run -c Release --no-build -- bodies mass fk handover resid jac warm   # exact checks, ~3-7 s
dotnet run -c Release --no-build -- fit --csv=../../results/fit_all.csv   # 450 fits against the saved ones, ~1-1.5 min
dotnet run -c Release --no-build -- fit "--bodies=Small|Long"     # a subset; --families=squat|stand ...
dotnet run -c Release --no-build -- fit --maxit=150 --restarts=1  # the fitter's settings (also --restartcost= --xtol= --ftol=)
dotnet run -c Release --no-build -- fit --atlas-extra             # a miner's fit as the first extra start of its variants (--atlas: as the start)
dotnet run -c Release --no-build -- anybody                       # 4 bodies the prototype never saw x 45 stances, ~30 s
dotnet run -c Release --no-build -- reserve                       # the whole chain on the 126 miner stances: fit -> Statics.Build -> S, ~20 s
dotnet run -c Release --no-build -- time                          # one residual / Jacobian evaluation per stance
dotnet run -c Release --no-build                                  # all of the above but time and the atlas runs, ~2 min
```
`handover` and `reserve` need `reference/reserve_cases.json` (export_reserve.py) and read the poses from
`../proto/res_miners*.json`; Body.Tests references src/Reserve only for `reserve`.
The exit code is the number of failed checks. While a Unity test suite runs on the machine use `-m:1` and one test
process at a time.

## Body side: state (November 4, 09:50)

What exists, builds and passes: the whole runner (`dotnet run -c Release --no-build`, everything but `time`):
**21541 checks, 0 failed, 114 s**; the exact part (`bodies mass fk handover resid jac warm`): 21402 checks, 3-7 s.
No compiler warnings.

- **bodies**: `BodyFactory` builds the prototype's 17 bodies (`run3.BODIES`) exactly as `b3.make_body` does: worst
  relative difference 4e-16 over every length, mass, capacity and range. Strength is data per joint
  (`CapBack`, `CapNeck`, `CapShoulder[L,R]` ... `CapToe[L,R]`, N m with the strength applied).
- **mass** (new Nov 4): each link's mass (`MassHips` ... `MassToes[L,R]`, `MassOf(link)`), **centre** (`LinkCentre[link]`,
  in the link's own frame from its origin joint) and **inertia** (`LinkInertia[link]`, about the centre, link frame)
  are data on `BodyModel`, so the game can set the measured ones from its weighed models
  (`SetLinkMassProperties(link, mass, centre, inertia)` also updates `Mass`). The factory fills them from the collider
  shapes (`BodyFactory.ShapeMassProperties`: capsules, the head's sphere, the foot's and toes' boxes), which gives
  b3.fk's centres exactly; b3 has no inertia, so the inertias are the shapes' (the statics do not use them). The
  kinematics read the centres from the data. Tested: shape formulas, every body's inertias physical, a measured trunk
  centre and mass move the body's centre and the back's subtree moment exactly as the statics say.
- **fk**: `Kinematics.Forward` at the 126 saved miner poses: points, rotations, link centres, lowest points, contacts,
  effort forces, effort residuals and base margin all within 2e-12 of `b3.fk` / `all_contacts` / `effort_forces` /
  `poly_margin`.
- **handover** (new Nov 4): `Statics.Build` (the LP's input) at the 252 saved miner poses of res_miners.json and
  res_miners2.json against the StaticsProblem the reserve side's golden file holds (`reserve_cases.json`): mass,
  centre, every contact (point, normal, mu, link, other, kind, support), every joint (name, origin, hinge, axis,
  capacity, subtree mask, subtree mass and moment, contact signs) and the 102 let-go subsets: all within 1.4e-14,
  0 structural mismatches. So a fit hands the reserve LP exactly what the prototype would.
- **resid**: for all 765 saved fits (res_miners*, res_others, res_extra) the start vector and bounds are identical and
  the residual vectors at the saved pose and at x0 agree to 1.4e-11 relative (cost 3e-12). So every stance family of
  lib3.py and every residual of `b3.solve_stance` is ported.
- **jac**: the AD Jacobian against central differences on 153 fits x 2 poses (925350 entries): median error 0,
  99.9th percentile 9e-8, no entry above 1e-4.
- **warm**: started at the prototype's own optimum (126 miner fits), the C# LM never ends higher; it lowers 44 of 126
  (the prototype's TRF with finite differences stopped short), 9-13 ms per fit.
- **anybody** (new Nov 4): 4 bodies the prototype never saw (Tiny: Small x0.6 S0.8, 8.8 kg; Lanky: Long legs x1.2 arms
  x1.15 trunk x0.9; Stocky: Round x1.2 legs x0.8 trunk mass x1.3 S1.3, 159 kg; Frail: Long S0.45 knees x0.6) x the 45
  stances: every fit finite, no contact off the ground and nothing through it by more than 0.5 mm, cost medians
  0.08-0.39 (the 10 prototype bodies': 0.12-0.62); 107-199 ms per fit under load. No saved fits to compare with.
  (As in b3.make_body, `Height` scales with `Scale` only, not with the length multipliers.)

The fitter (`StanceFitter`, `StanceProblem`): bounded Levenberg-Marquardt on b3.solve_stance's residuals with exact
forward-mode AD Jacobians (`AD.cs`), Nielsen's damping update, More's column scaling, an active set at the bounds,
Cholesky on the normal equations; stops on ftol/xtol 1e-7 (scipy's), gtol 1e-8, 400 iterations per start; extra
starts only while the cost is >= 0.05 (run3's rule), the first with run3's own noise (numpy RandomState(1)), later
ones with a self-contained SplitMix64 noise (no System.Random, so Unity's runtime gives the same starts; bitwise
equality across runtimes is still not promised, since Math.Sin/Cos may differ in the last bit). `FitOptions.Start`
takes a warm start (a neighbouring body's or stance's fit), `ExtraStarts` given poses.

Fits from the stances' start poses against the saved fits: see "Fits against the prototype" below.

## Fits against the prototype

`fit`: 10 bodies (Small, Long, Round, and 7 invented ones from `run3.BODIES`: Small S0.5, Toddler-like, Giant,
Long arms x0.65, Round stiff knees, Heavy trunk, Long right leg 0.4) x the prototype's 45 stances each = **450 fits**,
each from the stance's own start pose (as the prototype did), compared with the prototype's saved fit of the same
body and stance (`reference/body_ref_cases.json`, from res_miners2 / res_others / res_extra). Per fit: cost, pose
difference (max and rms over the angles, and dz), contact error, penetration, base margin, let-go margin, ms.
Tables: `results/fit_all.txt`, `results/fit_all.csv` (default settings), `results/fit_maxit400*.{txt,csv}`.

Verdict per fit: *match* = cost within max(0.02, 10%) of the saved one and contacts within 2 mm (or the saved error
+ 1 mm), no new penetration over 2 mm, base margin within 10 mm; *better* / *worse* = cost lower / higher beyond that
tolerance; *other optimum* = the same cost but a different base margin or contact error.

| fitter settings | match | better | other optimum | worse | mean ms (median) |
|---|---|---|---|---|---|
| LM, 150 iterations per start, 1 extra start (run3's rule) | 264 | 50 | 96 | 40 | 74 (57) |
| LM, 400 iterations, 1 extra start | 279 | 50 | 83 | 38 | 43 (24) |
| LM, 400 iterations, 2 extra starts (the third with System.Random noise) | 291 | 56 | 83 | 20 | 119 (78); 186 (129) |
| **the default now: as above, the third start's noise SplitMix64 (runtime-independent)** | **293** | **53** | **83** | **21** | **125 (83)** |

Timings: Release, one thread, with a Unity PlayMode suite running on the machine all the while; identical runs vary
up to 2x (the same settings above: 119 and 186 ms mean). Small alone: 51-62 ms mean per fit (31-41 median, max 172).
One residual evaluation is 18-120 us and one with the Jacobian 70-520 us (m = 83-109 residuals, n = 31 free; `time`).
The prototype took 38 to 62 s per fit (mean 49 s): the C# is about 250 to 1000 times faster. **Not measured on an
idle machine.**

With the default (`results/fit_all.*`): mean cost over each body's 45 stances lower than the prototype's for all 10
bodies (e.g. Long 0.28 vs 0.78, Long arms x0.65 0.77 vs 1.24, Giant 0.781 vs 0.791); median costs within 0.02 of the
prototype's for every body (Small 0.156 vs 0.158, Giant 0.616 vs 0.611); total cost (each capped at 10) 233 vs 321;
307 of 450 fits at or below the saved cost. Contact error over 2 mm in 3 fits (max 18 mm; the saved fits: 10, max
49 mm); penetration over 2 mm in 3 fits (max 18 mm; saved: 10, max 238 mm). Base margin within 10 mm of the saved one
in 319 of 450 (median difference 4.3 mm). 15 of 450 end on the iteration cap.

**Causes of the differences** (read from the runs):
1. *Another basin.* All 21 worse fits, started at the prototype's own optimum instead, stay at or below its cost
   (mean 0.5030 vs 0.5037): the residuals are the same problem (as `resid` shows to 1e-11); from the start pose the
   LM path (exact Jacobians, Nielsen damping, active-set bounds) reaches another local minimum than scipy's TRF
   (finite differences, `x_scale='jac'`, reflective bounds). Likewise the 53 better fits are basins the TRF missed,
   and the `warm` test shows the TRF often stopped short (44 of 126 lowered from its own optimum). The worst: Round
   stiff knees, squat h=0.60 toes 1.33 vs 0.83; Heavy trunk, half-kneel hands on the ground 2.48 vs 2.14-2.18.
2. *Flat directions.* Even matching fits differ by a median 42 deg in their largest angle (rms 12 deg): neck
   rotation (the largest difference in 89 of the matches), toe angles, shoulder rotation and the elbows of free arms
   barely change the cost in many stances, so equal-cost poses differ there. Pose difference is not a quality
   measure here; cost, contacts, penetration and margins are. (The *other optimum* fits: the same cost, the base
   margin more than 10 mm away.)
3. *Iteration cap.* At 150 iterations more fits ended on the cap; at 400, 15 of 450 still do (slow zig-zag near
   the kinks of the hull/min terms).

**The atlas idea, tried** (stances_strategy.md 2.3: warm starts from a neighbouring body; `fit --atlas`,
`fit --atlas-extra`; tables `results/fit_atlas*.{txt,csv}`): the 7 bodies made from a miner use that miner's fit of
the same stance (fitted first, in the same run).

| start | match | better | other optimum | worse | variant bodies' mean cost (7) | mean ms |
|---|---|---|---|---|---|---|
| the stance's start pose, then 2 noisy starts (default) | 293 | 53 | 83 | 21 | Giant 0.781, LgArm 0.773, RdKnee 0.915, HvTrunk 0.949 | 125 |
| the miner's fit *instead* of the start pose (`--atlas`) | 280 | 58 | 89 | 23 | Giant 0.937 (worse: 1.6x the size), LgArm 0.707, HvTrunk 0.883 | 119 |
| the miner's fit as the *first extra* start (`--atlas-extra`) | 281 | 66 | 86 | **17** | **Giant 0.635, LgArm 0.668, RdKnee 0.881, HvTrunk 0.886** | 208 |

So a neighbour's fit is a good *extra* start (every variant body's mean cost at or below the default's), not a
replacement for the stance's own start (a much larger body starts badly from a small one's pose). Not made the
default: the atlas (which neighbour, scaled how) belongs to the `StanceAtlas` module of section 5; the API for it is
`FitOptions.ExtraStarts`.

**The whole chain** (`reserve`, Body.Tests references src/Reserve for it): the 126 res_miners2 stances (Small, Long,
Round x 42) fitted from x0 by the C# fitter -> `Statics.Build` -> `ReserveSolver` (the LP agent's) -> S, against the
prototype's s (`reserve_cases.json` py.s, the unique LP optimum; r is vertex-dependent, see the Reserve side):
- at the *saved* poses the C# path gives the prototype's s in 126 of 126 (within 1e-6);
- at the *C# fits*: held or not the same in 123 of 126; where both are held (121), |S - s| median 0.007, 90th
  percentile 0.033, max 0.66; within 0.02 in 92, within 0.05 in 113; mean S 0.175 vs 0.180.
- The three held/not-held differences: Small and Round "sitting up, trunk 15 deg, arms reaching" are held at the
  C# fits (cost 0.09 / 0.07) where the prototype's fits were poor and not held (cost 1.45 / 1.73); Long "hands and
  knees, ready to let go of a hand" is not held at the C# fit (cost 2.37) but held at the prototype's (cost 4.74):
  the fit's cost does not contain the reserve, so a lower cost need not hold better.
- The largest S differences are where one side's fit is poor: Round "hands and knees, ready to let go of a hand"
  (S 0.93 vs 0.27; costs 4.7 vs 7.5, both poor), Long "half-kneel, hands on the ground, ready" (S 0.28 vs 0.77 and
  0.28 vs 0.54: the prototype's fits there have costs 390 and 467, failed), Round's half-kneels with hands on the
  ground (0.21 vs 0.30, lower costs), Round "squat h=0.40 toes" (0.26 vs 0.19, a worse basin: cost 0.19 vs 0.16).
- Run time: 126 fits + 252 LPs in ~21 s under load.

**Against the design's first evidence step** (stances_strategy.md section 5: "the C# port must match the prototype's
r and margins within 0.02, against a golden file from res_miners2.json"): *at the prototype's poses* it does (S
126/126 to 1e-6 here; r within 0.02 in 122/122 and margins exact on the Reserve side). *Fit for fit from the start
poses* it does not everywhere, because the fits land in other basins (both ways): S within 0.02 in 92 of 121 held
stances (0.05: 113); base margin within 20 mm in 116 of the miners' 135 fits (median 4.3 mm), let-go margin within
20 mm in 40 of 51. Which reading the step means is the lead's / Game Director's call. Speed against section 2.3's
estimate ("under 20 ms per stance", not measured then): from the start pose 43-125 ms mean under load (three starts:
the default; one extra start: 43 ms); warm-started (from the prototype's pose, as the atlas would from a neighbouring
body) 9-13 ms.

## Reserve side

State: November 4, 09:40 (LP agent): done and verified; nothing half-made. Run from `port/tests/Reserve.Tests`:

```
dotnet build -c Release -m:1
dotnet run -c Release --no-build                  # everything, 15-70 s (with a Unity suite running beside it)
dotnet run -c Release --no-build -- --quick       # no primal cross-check, no feasible regions, short timings, ~5 s
dotnet run -c Release --no-build -- --strict-r    # r within 0.02 of the prototype as a plain gate (see below)
dotnet run -c Release --no-build -- --case "Round | on its forearms"   # one fit in detail (LP_TRACE=1: every pivot)
```

What is in `src/Reserve` (namespace `WonderGather.AnyBody`, refs Core only, netstandard2.1, C# 9):

- `DenseSimplex`: bounded-variable primal simplex on a dense tableau (Dantzig pricing, Harris two-pass ratio test,
  Bland's rule after 12 degenerate pivots in a row, small pivots avoided, phase 1 with artificials or a start from a
  slack basis, a final LU refactorisation that checks primal and dual feasibility and goes on, or restarts once with
  Bland, if the check fails; an unbounded ray is certified on a fresh factorisation, by the fresh reduced cost and
  one solve for the entering column, before it is reported; `ResolveNewCosts` re-uses the basis when only the costs
  change). `LpStatus`: Optimal / Infeasible / Unbounded / IterationLimit / Numerical.
- `LinProg`: scipy's `linprog` form (Aeq, Aub, bounds) on top of it.
- `ReserveSolver` / `ReserveResult` / `ReserveLP`: b3.py `reserve_lp(want=True)`, solved through its **dual** (rows =
  4 per contact + 1, so 9 to 41 rows instead of about 220; the slack basis is feasible at once, so no phase 1; a shift
  of the body's mass changes only the dual's costs, so `Prepare(p)` + `Reserve(shift)` warm-starts). The forces are
  read from the dual's multipliers. `S` = the LP optimum (b3's s), `R` = run3's r (max per-joint share, ball joints by
  the full norm), `Worst`, `PerJoint`, `PerJointLp`, `HandsShare`, `OnThighs`, `Residual` (check in the original
  units), `Pivots`, `WarmStarted`. `CostJitter` (1e-4) makes each pyramid edge's tiny force cost a little different,
  so one force split is picked deterministically; it moves S by about 1e-8 at most.
- `FeasibleRegion`: b3.py `feasible_region` (12 directions, 11 bisections, tmax 1.5 m), warm-started per direction.
- `SupportPolygon`: b3.py `hull` + `poly_margin` (the base margin).

Against the prototype (`reference/reserve_cases.json`: the 252 saved fits of the three miners, res_miners.json and
res_miners2.json; full run, 09:42: **5072 checks passed, 0 failed**; with `--strict-r` 3 fail, the r question below):

| what | result |
|---|---|
| dense simplex, hand-made LPs (textbook, bounds, free variables, redundant rows, 2 infeasible, 2 unbounded, Beale's cycling example, an all-degenerate cycle, Klee-Minty 8, the pivot limit, new costs on a kept tableau 20 times) | all as expected |
| dense simplex, 321 random LPs against scipy/HiGHS (`lp_random.json`: general, degenerate, reserve-shaped, degenerate reserve-shaped with repeated columns, 3 sorts of infeasible, unbounded, Beale; 243 optimal, 37 infeasible, 41 unbounded by scipy) | status as scipy in 321/321; objective within 6.5e-14 relative; worst infeasibility 7e-14 |
| held or not held | the same as the prototype in 252/252 (243 held, 9 not) |
| warm starts (one Prepare, 12 random shifts of up to 0.5 m per fit) against cold solves | the same verdict in 2916/2916 (1074 held), S to 8e-11 |
| changed problems against the primal path (b3.py's own form, phase 1 + 2): the mass shifted up to 0.3 m as a new StaticsProblem, one support dropped | the same verdict in 486/486 (256 held, 230 not), S to 1.3e-15 |
| every capacity scaled by k in [0.3, 3] (243 fits) | S(k) = S / k to 1.1e-8 relative (the jitter's size) |
| S (the LP optimum) against b3's s | max abs difference 5.1e-11; the same under 8 other solver settings (no jitter, Bland at once or never, pivot tolerances 1e-11 / 1e-7, ...); a primal path written as b3.py gives the same S (4e-11) |
| base margin; let-go reserve and margin (102 fits) | exact (0 m; 9e-16) |
| feasible-region depth (243 held fits) and its 12 boundary points | **exact**: 243/243, max difference 0 m |
| r (run3's max per-joint share) within 0.02 | res_miners2.json (saved values): 122/122 (max 0.0198); res_miners.json (the prototype's code now): 118/121 (max 0.026). The 3 beyond (Long squat h=0.50 hands down 0.311/0.285, Round squat h=0.35 hands down 0.299/0.276, Round half-kneel left foot forward 0.318/0.341) are other optimal splits of the same LP (proven per fit, as below): the default run passes them as such, `--strict-r` fails them |
| worst joint the same | 95/122 (+10 the same joint on the other side); 91/121 (+16) |
| hands' share within 0.02 | 92/122; 94/121; where they differ, both values lie inside the range over the optimal force splits (73/73) |
| option, not the prototype's measure: `Directions = HalfSphereDirections(200)` (ball joints in 200 directions), the full-norm reserve R_true = the least worst \|tau\|/cap over all force splits, unique | certified bracket [S_fine, R_fine] of width at most 0.0036 on the 243 held fits; the 13 directions' S lies below R_true by mean 0.003 (max 0.041); the prototype's vertex r lies above it by mean 0.008 (max 0.036), so S_fine is within 0.02 of the prototype's r in only 227/243; 58 pivots, about 17-25 ms per LP |

**Why r, the worst joint and the hands' share cannot all match:** only S is unique. The secondary force cost of b3.py
is a constant when every normal is +Z, so the optimal force splits form a face, and r (full norm at the split
found), the worst joint and the hands' share depend on which vertex a solver returns. The proof that the port is
the same LP: the prototype's own forces, read by the C# code, give the prototype's r exactly (243/243, 1e-16) and are
optimal in the C# LP (243/243, 4e-10). HiGHS's own two methods differ as much: interior point against dual simplex,
r beyond 0.02 in 4 of the 122 golden fits (max 0.025), the worst joint different in 52 of 243, hands' share beyond
0.02 in 50 of 243. Every other setting of the C# solver gets 237 to 240 of 243 within 0.02 (max 0.025 to 0.033),
HiGHS's interior point 239. The hands' share is the loosest: over the optimal splits it can range from 0 to 0.55
(Round, squat h=0.35 hands down). The feasible-region points being identical also means every one of its ~35000
bisection LPs (s <= 1 or not) got HiGHS's verdict. Gates and route costs should use S (b3.py's measure, the
default); a unique full-norm r exists as the option above, off by default: which measure the game uses is a design
choice, not made here.

Timings (Release, one thread, with a Unity test suite running beside it, so they vary about 3x between runs; the
prototype's numbers were taken by the export, same machine, Nov 3): one reserve LP from scratch **median 0.3-1.4 ms**
(42 pivots, max 97), the prototype 5.4 ms; one feasible region (145 LPs, warm-started, 2.1 pivots and 43-180 us per
LP) **median 6-26 ms** (max 20-88), the prototype 697 ms. Certifying an unbounded dual by one solve instead of
rebuilding the tableau made the feasible region about 5x faster (87 ms before, at the same load). The dense simplex in
`LinProg` form (phase 1 + 2, up to 226 rows) on the random LPs: 10 ms per LP; the reserve LP goes through the dual.

Interface: as `INTERFACE.md`, plus `ReserveSolver.Prepare` / `Reserve(shift)` / `Solve(shift)` (warm start),
`FeasibleRegion.Compute(solver, ...)`, `ReserveSolver.Evaluate`, `CostJitter`, `Directions` +
`HalfSphereDirections(n)`, and `ReserveResult.Objective / Residual / WarmStarted` (noted in its log). Masses, centres of mass and capacities come only through
`StaticsProblem` (`Mass`, `Com`, `JointLoad.SubMass / SubMoment / Capacity`): the Reserve side keeps no body data of
its own, so measured centres from the weighed models and strength from muscle per region reach it unchanged.

Reserve side, next: (1) a design question for the lead / Game Director: keep S (13 directions, as the prototype) or
use the full-norm reserve (`Directions`, round strength, unique; about 20x slower per LP: fewer directions, or a
cutting-plane loop adding only the violated directions, would bring that down). (2) The halves meet: the body agent
checks `Statics.Build` against `reserve_cases.json` (252 poses, 102 let-go subsets, 1.4e-14), so what is verified
here holds for the C# fits' poses too; the whole chain on the C# fits is in "The whole chain" above (S differences
there come from the fits, not the LP); the feasible region along that chain is not run yet. (3) In Unity: one
`ReserveSolver` per thread; it allocates only when a problem grows, plus the result's arrays.

## Where it stopped / what next

**Body side, November 4, ~09:45 (body agent).** Done and verified, nothing half-made: body model and factory (with
each link's mass, centre and inertia as data, strength per joint as data), kinematics, the stances and their
residuals, the AD Jacobians, the LM fitter, the hand-over to the LP, and the whole chain on the miners. Files of
this run: `src/Body/BodyModel.cs` (LinkCentre, LinkInertia, MassOf, SetLinkMassProperties, ShapeMassProperties,
Capsule, Box), `src/Body/Kinematics.cs` (centres from the data), `src/Body/StanceFitter.cs` (defaults 400 / 2,
SplitMix64 noise), `tests/Body.Tests/{MassDataTests, HandoverTests, AnyBodyTests, ReserveAtFitsTests}.cs`, options
in `FitTests.cs`, `results/`. Not done / next:
1. **Timings on an idle machine** (every number here was taken with a Unity PlayMode suite running).
2. **The worse basins** (21 of 450): a neighbouring body's fit as an extra start brings them to 17 and lowers every
   variant body's mean cost (tried, above, not the default); still to try: the neighbouring *stance's* fit (which may
   also make one body's fits more alike from stance to stance), scaling a neighbour's pose (z) to the body, and the
   15 fits on the iteration cap.
3. **Flat directions**: if the game wants one pose per stance (not just one cost), a small regulariser toward the
   start pose on neck rotation, toes, shoulder rotation and free elbows would pin them; that changes the problem,
   so it is the Game Director's call, not made here.
4. **The let-go stances**: the fit's cost does not contain the reserve (Long "hands and knees, ready to let go of a
   hand": lower cost, not held). The design (stances_strategy.md section 5) is where a reserve-aware fit would be
   decided.
5. Into Unity: the three src projects are netstandard2.1 / C# 9 with no UnityEngine; copy src/Core, src/Body,
   src/Reserve under `Assets/_WonderGather` as one or three asmdefs (not done here: the task kept away from Assets).
   The game would then set each link's measured mass and centre with `SetLinkMassProperties` from its weighed models.
6. Golden data: `reference/*.json` other than body_ref_bodies.json are not committed; `export_body.py`,
   `export_reserve.py` and `export_lp_random.py` make them again (Python 3.12 + numpy + scipy).
