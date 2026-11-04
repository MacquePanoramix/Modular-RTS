# Interface: body/fitter (src/Body) <-> reserve LP (src/Reserve)

Shared by the two port agents. Each edits its own section and appends to the log; changes to the shared types are
proposed here first. Status: **proposal v1 by the body agent (Nov 3)**; the LP agent please confirm or amend below.

## Layout (proposed)

```
port/
  AnyBody.sln                      (both projects + tests; whoever creates it first, the other adds to it)
  src/Core/AnyBody.Core.csproj     netstandard2.1, C# 9: Vec3, Mat3, Quat, BodyLinks, SupportKind,
                                   ContactPoint, JointLoad, StaticsProblem   <- THE INTERFACE (exists, builds)
  src/Body/AnyBody.Body.csproj     body model, kinematics, stances, fitter, effort split (refs Core)   [body agent]
  src/Reserve/AnyBody.Reserve.csproj  the reserve LP, feasible region (refs Core only)               [LP agent]
  tests/Body.Tests/                console runner (no NuGet cache offline, so no xUnit)              [body agent]
  tests/Reserve.Tests/             (LP agent's choice)
  reference/                       Python exports (golden data)
```

Namespace `WonderGather.AnyBody` everywhere. No UnityEngine, no System.Numerics in public APIs. Core has no
dependencies, so neither agent's half-built code can break the other's build. Reserve does **not** need to reference
Body: everything the LP needs is in `StaticsProblem` (Core). Body's tests may reference Reserve to print r.

## The types (src/Core/StaticsProblem.cs) - mirrors b3.py exactly

- `StaticsProblem`: `Mass` (kg), `Com` (world), `Contacts` (ContactPoint[], in b3.py `all_contacts` order),
  `Joints` (JointLoad[], order: back, neck, then L then R: shoulder, elbow, hip, knee, ankle, toe = b3.py `K['joints']`),
  `Weight` = M g, `Gravity` = 9.81, `BallDirections` = b3.py `DIRS` (13 unit vectors).
- `ContactPoint`: `Point`, `Normal` (unit; +Z on the ground), `Mu` (0.6 ground, 0.5 hand on thigh), `Link`
  (BodyLinks index the force acts on), `Other` (-1 = ground; else the link taking the reaction: a self-contact, which
  is NOT in the whole-body equilibrium), `Kind` (SupportKind), `Support` (index into the stance's support list),
  `OnGround`.
- `JointLoad`: `Name` ("back", "neck", "shoulderL", "elbowL", "hipL", "kneeL", "ankleL", "toeL", then R),
  `Origin`, `IsHinge` + `Axis` (elbow: upper arm's y; knee: thigh's y; toe: foot's y; others are ball joints),
  `Capacity` (N m, strength applied), `SubtreeMask`, `SubMass`, `SubMoment` (sum m c), `ContactSign[i]` in {-1,0,+1}.
- Helpers that reproduce b3.py's matrices:
  - `Equilibrium(shift, A[6,3nc], b[6])` = `equilibrium(K, C, shift)`;
  - `TorqueMap(j, shift, B[3,3nc])` returns tau0 and fills B, so tau_j = tau0 - B f = `torque_maps(K, C, shift)`;
  - `JointLoad.GravityTorque(shift)`;
  - `PyramidEdges(n, mu, edges[4])` = `generators(n, mu)` (same tangent construction, so the LP matches);
  - `Subset(keep)` = the same pose with only some contacts (b3/run3's "if a support let go now": `release_r`).
- Physics, as in b3.py: tau_j = G [S'.y, -S'.x, 0] - sum_i sign_ji (p_i - o_j) x f_i, S' = SubMoment - SubMass (o - shift);
  ground equilibrium sum f = (0,0,Mg), sum p x f = g (S.y, -S.x, 0), S = M (Com + shift).

## What the Body side provides (body agent)

```csharp
// src/Body: build a body, fit a stance, hand the result to the LP
BodyModel body = BodyFactory.Make("Small" /*, strength, scale, capMul, ... as b3.make_body */);
Stance st = StanceLibrary.Squat(body, 0.35);              // the lib3.py families
FitResult fit = StanceFitter.Fit(body, st /*, warm start */);   // fit.Q (34 VARS, radians/metres), fit.Cost
StaticsProblem p = Statics.Build(body, fit.Q, st.Supports);    // the LP input (any subset of supports too)
double margin = Statics.BaseMargin(p);                         // poly_margin of the ground points around the COM
```

## What the Reserve side provides (LP agent)

**Confirmed by the LP agent (Nov 3):** the layout, the namespace, and `StaticsProblem` as the only input. Reserve
refs Core only and does not change Core's existing types. Tests: `tests/Reserve.Tests/`, a console runner too.

```csharp
// src/Reserve (AnyBody.Reserve.csproj, netstandard2.1, C# 9), namespace WonderGather.AnyBody
public enum LpStatus { Optimal, Infeasible, Unbounded, IterationLimit, Numerical }

public sealed class ReserveResult
{
    public LpStatus Status;       // Optimal, or Infeasible = no force split holds the pose (S = R = +inf)
    public bool Held;             // Status == Optimal
    public double S;              // the LP optimum = b3.reserve_lp(K, C, shift) = res.x[-1]   (unique)
    public double R;              // run3's r = max PerJoint (what res_*.json call r)          (NOT unique, see below)
    public int Worst;             // index into p.Joints of max PerJoint (-1 when not held);  WorstName
    public Vec3[] Forces;         // per contact, world N, acting on Contacts[i].Link
    public double[] PerJoint;     // b3 reserve_lp(want=True) 'per': ball |tau|/cap, hinge |axis.tau|/cap
    public double[] PerJointLp;   // the LP's own measure (ball: max over the 13 directions); its max = S
    public double HandsShare;     // run3 hands_share: sum f.z of ground contacts on a forearm / W
    public double OnThighs;       // run3 on_thighs: sum |f.n| of self-contacts / W
    public int Pivots;
}

public sealed class ReserveSolver           // one per thread; reuses its buffers
{
    public ReserveResult Solve(StaticsProblem p);               // = b3.reserve_lp(K, C, want=True)
    public ReserveResult Solve(StaticsProblem p, Vec3 shift);   // whole body's mass shifted horizontally
    public double Reserve(StaticsProblem p, Vec3 shift);        // S only; warm-started when only shift changed
}
public static class ReserveLP { public static ReserveResult Solve(StaticsProblem p, Vec3 shift = default); }

public static class FeasibleRegion          // = b3.feasible_region(K, C, w, ndir, tmax): 11 bisections/direction
{
    public static FeasibleRegionResult Compute(StaticsProblem p, double w = 1.0, int ndir = 12, double tmax = 1.5);
    // .S0, .Depth (-1 when S0 > w), .Points (boundary, xy), .LpCount
}

public static class SupportPolygon          // = b3.hull + b3.poly_margin
{
    public static double Margin(StaticsProblem p);   // ground points' hull, signed distance of Com.xy, + inside
    public static double Margin(double[] xy, int n, double cx, double cy);
}
```

**Read this before using R, Worst or HandsShare.** The prototype's secondary cost (1e-4/W per unit force) is a
constant when every normal is +Z (sum of the pyramid weights = W sqrt(1 + mu^2)), so the LP's force split is not
unique: only `S` is. `R` (max of |tau|/cap, ball joints by the full norm, not the 13 directions), `Worst` and
`HandsShare` depend on which optimal vertex the solver returns. HiGHS's own two methods differ on the saved fits
(Small's tripod: hands' share 0.53 by dual simplex, 0.60 by interior point; worst shoulderL vs shoulderR). Gates and
route costs should use `S`; `R` is kept for comparison with the saved files.

**Base margin:** `SupportPolygon` is in src/Reserve for reading a fit. The fitter needs poly_margin in its residuals,
so if the body agent wants it, I will move `SupportPolygon.cs` to Core unchanged (same namespace, no API change):
say so in the log, or move it yourself.

**Golden data for the LP:** `reference/export_reserve.py` -> `reference/reserve_cases.json`: every saved fit of
res_miners.json and res_miners2.json as StaticsProblem fields (mass, com, contacts with p/n/mu/link/other/kind/support,
joints with origin/hinge/axis/cap/mask/subMass/subMoment/sign), the saved values, and the prototype's values now
(s, per, r, worst, forces, hands' share, feasible-region depth and points, base margin, let-go). The body agent can
use it as a golden file for `Statics.Build` too (same field names as the C# types).

## Log

- Nov 3, body agent: created Core (builds offline with `dotnet build`), this file, and the layout above.
- Nov 3, LP agent: confirmed the layout and StaticsProblem; filled in the Reserve API above (ReserveSolver,
  ReserveResult with S unique and R/Worst/HandsShare vertex-dependent, FeasibleRegion, SupportPolygon); started
  reference/export_reserve.py. Will not touch src/Core or src/Body; the .sln: I add my two projects if it exists.
- Nov 3, body agent: thanks, agreed. (1) Keep `SupportPolygon` in Reserve: the fitter needs a *differentiable*
  poly_margin (exact Jacobians by forward-mode AD), so Body has its own and will cross-check against yours in tests.
  (2) Heads-up: `reference/check_saved.py` shows the prototype as it stands reproduces **res_miners2.json, res_others.json
  and res_extra.json exactly** (765 of 765 costs to 1e-12 at the saved poses), but **not res_miners.json** (101 of 126
  costs differ, e.g. Round "on its forearms" saved 5.07, now 1285): res_miners.json was written by an older b3/lib3.
  Its poses are still valid inputs for the LP, but its saved r/fr values may come from older code; golden = miners2.
  (3) Body exports: `reference/export_body.py` -> body_ref_bodies.json (the 17 bodies as make_body builds them),
  body_ref_cases.json (765 fits: x0, bounds, residual vectors at the saved pose and at x0), body_ref_fk.json.
- Nov 4, LP agent: the Reserve side is verified against the prototype (README.md, "Reserve side"): S to 5e-11 on all
  252 saved miner fits, held/not held 252/252, feasible-region depth and points exact (243/243), base margin and
  let-go exact. Additions to the API above, no changes to it: `ReserveSolver.Prepare(p)` then `Reserve(shift)` /
  `Solve(shift)` warm-start from the last basis (what FeasibleRegion does); `FeasibleRegion.Compute(solver, p, w, ndir,
  tmax, bisections)` re-uses a solver; `ReserveResult.Objective`, `.Residual` (the answer checked in the original
  units), `.WarmStarted`; `ReserveSolver.CostJitter` (1e-4: each pyramid edge's tiny force cost a little different,
  so the force split is picked deterministically; S moves by about 1e-8 at most; 0 gives b3.py's costs exactly);
  `ReserveSolver.Evaluate(p, forces, shift, result)` reads any force split as b3/run3 do. Still vertex-dependent, as
  said above: R, Worst, HandsShare (HiGHS's interior point differs from its dual simplex by up to 0.025 in r).
  Later the same morning, an option (off by default, b3.py's 13 directions stay the default): `ReserveSolver.Directions`
  (the ball joints' checked directions, null = `StaticsProblem.BallDirections`) and
  `ReserveSolver.HalfSphereDirections(n)`; with 200, S becomes the full-norm reserve to within 0.0036 (unique, round
  strength). `Evaluate(p, forces, shift, result, ballDirections)` measures PerJointLp in a given set.
- Nov 4, body agent: the Body side is verified (README.md, "Body side" and "Fits against the prototype"). No change to
  Core's types. Additions on the Body side only: (1) each link's **centre** (`BodyModel.LinkCentre[link]`, in the
  link's own frame from its origin joint) and **inertia** (`LinkInertia[link]`) are data beside the masses, filled from
  the collider shapes by `BodyFactory.ShapeMassProperties` (b3.fk's centres exactly) and settable from the game's
  weighed models by `SetLinkMassProperties(link, mass, centre, inertia)`; the kinematics read the centres from there,
  so `Statics.Build` hands the LP the measured centres once the game sets them. (2) `Statics.Build` is checked against
  your `reserve_cases.json` at all 252 poses and 102 let-go subsets: within 1.4e-14, 0 structural mismatches.
  (3) `FitOptions` defaults now 400 iterations per start and 2 extra starts (was 150 and 1, run3's rule): on 450 test
  fits, worse than the prototype 40 -> 21, about 1.5-2.5x the time (still ~0.05-0.2 s per fit against ~49 s).
