# Brief: an independent review of the stiff-ankle answer (November 12, 2026)

For one reviewer who did not write what is reviewed. Sized to finish inside a small usage window: **create
`review.md` beside this brief first, with a heading, and add each finding to it as soon as it is reached**, claim by
claim, so that a cut-off loses nothing. Read-only otherwise: no Unity run, no other edit, no commit. The verdict is
advice to whoever builds, not the Game Director's.

## What is reviewed

- `Docs/Design/AnyBody/motion_design.md`, §6.2: R4 (from "**Answered (November 12)"), R1 (from "One thing is added
  (November 12"), K8 (all of "What a drive gives a part that the ground holds"), and the last sentences of K4(a) and K5
  (dated November 12); the notes of November 12 in §2.3; row 0 of §6.3.
- `Docs/Validation.md`, its last section ("the stiff ankle: it holds under the game's solver").
- `Docs/Handoff.md`, item 3 of "In hand" (the making-up, and the stroke).

## What it rests on

- **The test:** `D:/Dev/WG/Assets/_WonderGather/Tests/PlayMode/ArticulationFactsTests.cs` (the work branch
  `claude/wip-round` at `582c022`): `Stood`, `AsMade`, `Shoed` and `AStifflySprungAnkleHoldsALightBoot` (roughly lines
  540 to 900).
- **The runs:** `D:/Dev/WG/TestRuns/facts_r4_pgs3.log` (PGS, the reshaped bar, passed) and `facts_r4_tgs.log` (TGS,
  the first bar). As a table, from `D:/Dev/WG`:
  `python -I Docs/Technical/AnyBody/tools/r4_table.py TestRuns/<log> flat|rest|all [text the row must hold]`.
- **The engine's source:** PhysX 4.1 (public: `NVIDIAGameWorks/PhysX`, branch `4.1`),
  `physx/source/lowleveldynamics/src/DyFeatherstoneArticulation.cpp`: `setupInternalConstraintsRecursive` (about
  lines 2556 to 2650) and `solveInternalConstraintRecursive` (about 4130 to 4180). Whoever starts the review gives the
  path of a local copy. Do not paste its text into the review: cite functions and lines, in your own notation. Unity's
  build of the engine may differ from it; the runs are the proof.

## The numbers

dt = 0.02 s. Turns of the solver: 14 of place and 4 of speed (n = 18), the game's; also 28 + 4, 14 + 18 and 60 + 4.
eFull = 35 degrees = 0.6109 rad. A drive: k = S / eFull, c = S / ωmax (K5), a = dt (dt k + c), and for a light end part
x = I / (I + a).

| | Small | Long | Round |
|---|---|---|---|
| ankle's strength S (N m) | 61 | 141 | 154 |
| ankle's top speed ωmax (rad/s) | 19.9 | 16.1 | 18.5 |
| k (N m/rad) | 100 | 231 | 252 |
| c (N m s/rad) | 3.07 | 8.76 | 8.32 |
| a | 0.1014 | 0.2676 | 0.2672 |
| boot's I about the ankle, toe / sideways (kg m²) | 0.00695 / 0.0054 | 0.01153 / 0.00764 | 0.0091 / 0.0065 |
| asked in the flat cases, toe / sideways (N m) | 22.5 / 13.3 | 38.5 / 19.1 | 53.3 / 28.6 |
| what the flat sole can bear, toe / sideways (N m) | 33.7 / 20.0 | 57.7 / 28.7 | 79.9 / 43.0 |
| body / boot (kg) | 40.7 / 1.29 | 58.5 / 1.83 | 87.6 / 1.55 |

## The claims, in order (most weight first)

1. **The held part's law under PGS:** a part the ground holds gets `1 - (1 - x)^n` of what its spring says, n the turns
   of place and of speed together. Recompute the shares (toe 0.70 / 0.53 / 0.45, sideways 0.61 / 0.40 / 0.35 at 18
   turns) and the law's torques at 14 + 4, 28 + 4 and 60 + 4, and set them against the readings in K8 and in the
   validation table. Is "both kinds of turns count" in the source, or only in the numbers (14 + 18 reads as 28 + 4)?
2. **The free part** gets the implicit spring and damper exactly (`k e - (dt k + c) w_end`): is that what the update
   comes to after its first turn?
3. **A part let go** turns at most `(I w0 + dt k e) / (I + dt (dt k + c))`. Check the rows in the air and bent 22
   degrees, at 1, 3 and 10 times the spring. Is "none was over it" true in the logs? Is the reason given for 4.1
   against 7.2 rad/s (the edge still bears weight and drags) sound?
4. **R1's making-up** (not built):
   (a) is "what the spring and damper said" right on a ball joint with three driven axes (each axis on its own; the
   coupling through the part's turning weight)?
   (b) can the share's estimate and the target written from it form an unstable loop (a half-held part; a hold that
   comes and goes from one step to the next)?
   (c) the bound `eFull + dt ωmax` on the target's offset: does it keep a freed part under ωmax also when the target is
   kept at that offset every step, with a larger c, under gravity, and with the parent part moving?
   (d) **the stroke.** Fable 5.2 reckoned that at the bound the most a drive can work on a part in one step is
   `S eFull b (1 + b)^2 / 4`, `b = dt ωmax / eFull` (0.89, 0.61 and 0.78 of the stroke `S eFull / 2`; past it only
   above about 21 rad/s). Opus 5.5, checking it against the implicit law of claim 2 (`τ = k e0 - (dt k + c) w_end`, the
   joint's turn `dt w_end`), gets `S eFull b (1 + b) / 4` (0.54, 0.40 and 0.49 of the stroke; past it only where
   b > 1, a top speed over eFull / dt, about 30.5 rad/s): the first form leaves out the `dt k` part of the damping and is
   then a looser bound. Which holds, and under which assumptions (a part already turning at the step's start; a part
   partly held; the work as the test counts it, the told torque times the joint's turn)? Say what R1 should state.
   (e) "believed at once when it says the joint is free and slowly when it says it is held": is that a switch at a
   threshold in disguise (Luis, October 13: nothing switches at a threshold; a body's answer grows smoothly with the push
   or pull)? If it is, propose a smooth form.
5. **The clamp:** the engine clamps a drive's target to the joint's limits. Is that in the source as stated, and does
   §2.3's consequence follow (a straight joint that bears stays on its stop only where the load presses it there or
   where its strength is over 0.61 of the weight times the lever)?
6. **The sideways sum and the tip:** sideways the drive reads the law's share plus what it gives against the boot's own
   tip with nothing asked (within 5% at all four counts of turns); a flat boot sits 1.2 to 4.0 degrees off flat, less
   with more turns of place. Is there a better explanation of either? Does either threaten step 2 (a stance held by its
   own joints and nudged)?
7. **TGS's law** (lowest priority): a free spring acts (place + speed) / place times as strong as set; a held part gets
   that over `1 + h (h k + c) / I`, h the substep.
8. **Against the standing decisions** (the register of `Docs/GAME_VISION.md`; the design's section 6) and R4's own
   clause (the solver, its step and the true weights are Luis's to decide if R4 fails): is it right that nothing is put
   to the Game Director?
9. **What is missing:** a claim not shown, a case not run, a number that does not follow.

## What to write

`review.md` beside this brief: for each claim "holds", "wrong" or "not shown", with a two- or three-line recomputation,
or the line of the log or of the source that decides it. Then at most ten lines: what must change in the design text
before step 0 goes on.
