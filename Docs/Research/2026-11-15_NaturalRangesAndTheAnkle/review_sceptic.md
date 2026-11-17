# Sceptic's review of the ankle mend's build (November 17, 2026)

Reviewer: did not build it; read only (no project file edited, no Unity run, nothing staged or committed).
Question: is anything claimed beyond what was run, and can anything change the game while every switch is off?

## Verdict

No must-fix. The off path is, by reading, today's code in today's order, and the three default switches are off.
RiseAgent.Unheard = true is the change Luis approved on November 15 (correspondence file, line 42; research §6). The
read-back on both runs is a real read-back from the running body and both ended by themselves. Two of build.md's
statements go a little past the run (below), and the premise that C touches only a falling boot's hand-over is not
quite right.

## What I was shown

- `git diff` and `git status --short` in D:/Dev/WG (branch claude/wip-round, head 3182e08): RiseAgent.cs, OwnBody.cs,
  OwnRiser.cs, CoverRingLiveTests.cs, GettingUpByItsOwnTests.cs; new AnkleMend.cs (+ .meta), read in full.
- build.md in full; the logs and result xml of `ank_rb_off3` and `ank_rb_on3` in D:/Dev/WG/TestRuns (the ANKLE_SET,
  ANKLE_SWITCHES, ANKLE_RUNNING, ANKLE_DONE, ANKLE_DOWN, ANKLE_MAKE, READBACK_DONE lines, and "error CS" counts).
- OwnKeeper.Soften, OwnKeeper's Begin (stoodSpring and stoodDamper), OwnRiser.Aims/Drive/Slacken/Springs, PhysicalFall.Slacken,
  OwnBody.GoDown/OnItsFeet, the correspondence file and research.md on Unheard.

## With every switch off: does anything change?

By reading, no.
- OwnRiser.Aims: the only new text is `j >= Foot && (AnkleLimit != 0 || AnkleSpeed > 0) && AnkleMended(...)`; with both
  at their defaults it short-circuits false and the original lines follow, same order, same float operations.
- OwnRiser.Drive: `if (j >= Foot && AnkleSpeed > 0) damper = ...` is skipped; the damper line before it is unchanged.
- OwnBody: `refits` is false (`BootsFromOwnShin` false), so the make is as before; `toeAt` is computed first as before and
  only overwritten when `refits`; GoDown's second Slacken and the toe line are behind `BootsFromOwnShin`.
- New state with all off: `stoodAnkle`/`bootShift` (two Vector3 arrays each, allocated once per OwnBody), one static
  bool `ankleLimitSaid`, three static floats. One extra write at the plain make (`stoodAnkle[i] = anchors[Foot+i]`).
  No per-step allocation, no side effect. Defaults: AnkleLimit 0, AnkleSpeed 0, BootsFromOwnShin false: confirmed in the
  off3 log ("AnkleLimit 0, AnkleSpeed 0 ... BootsFromOwnShin False").
- Rung: read only for "the same operations in the same order". The off run (off3) shows the written target equal to a
  replica of today's form on 404/404, 266/266, 404/404 boot-steps (most apart 0.000 degrees) and damping equal to today's
  form on every step. That replica lives in the test and is a copy of the old AnkleCan/Aims text, not a run of git HEAD;
  so it is a check against my reading of the old code, not a bit-for-bit comparison with the old build. build.md says
  as much in its last line. I agree.

## RiseAgent.Unheard

- `public static bool Unheard = true;` is the whole runtime change; the only runtime read is RiseAgent.cs:126
  (`if (Unheard || riser == null) return;`). This is the approved "option a" (off as played, kept for training). The
  training and bench step the Academy themselves, so `Heard` returns before use for them; the game, which steps it
  automatically, is the one place this default changes anything. That is the intended effect, not a side effect.
- Setters: only GettingUpByItsOwnTests (keeps `unheardWas` in Load, puts it back in Restore; sets only when
  `-ownUnheard` is given) and AnkleMendArguments (Save in Load, Restore in TearDown, used by CoverRingLiveTests and
  AnkleMendTests). A grep over Assets and Docs/Technical finds no other setter. Put back correctly in all three.
- Consequence to remember: a GettingUpByItsOwn or CoverRingLive run with no `-ownUnheard` was heard before and is
  unheard now. Old results quoted from such runs are for the other arm. build.md says so.
- Not run: the new GettingUpByItsOwnTests Load/Restore and CoverRingLiveTests' new arguments and watch (compiled
  only). build.md says so.

## Can A or B run while standing, or linger after OnItsFeet?

- A and B live only in OwnRiser instance methods (Aims, Drive, Springs, Slacken), and the riser exists only while
  getting up (`riser = null` in OnItsFeet). The static switches cannot act on a standing body.
- Lingering gains: OnItsFeet nulls the riser and calls `keeper.Soften()`, which rewrites every joint's stiffness,
  damping and forceLimit from `stoodSpring/stoodDamper/strength` (captured in OwnKeeper's constructor from the drives as
  made, line 509) for x, y, z. B's raised damper is therefore overwritten at hand-over, and A's targets are replaced by
  the keeper's own. By reading this holds, provided the keeper captured the drives before the riser's first Springs
  (the riser writes its gains on its first Step, after OwnBody makes the keeper; I did not trace every call order).
  No run reads the ankle's damper after OnItsFeet: the watch is for 4 s of getting up, and the three makes' gains were
  read at the make. So "no linger" is read, not run.
- Slacken inside the riser (a lifted leg) calls Drive with a share; B's damper is recomputed there from the reduced
  `has` and `spring`, consistent with its stated form.

## Does C touch only the hand-over geometry of a falling boot?

No, two places, not one:
1. GoDown: a second `fall.Slacken` about the let-go shin's end (idempotent restart: Slacken re-reads footAt, sets
   Was/To and `slackened = 0`; `SlackenToes` still runs after it). This is the hand-over geometry; it applies to every
   fall of a body whose stood pose is known, including a plain standing body pushed over, not only after a get-up.
2. The make of a body made down with its stood pose known (`refits = BootsFromOwnShin && down && stoodKnown`): the boot,
   its toe position (`toeAt`) and the toe collider's position are moved to the shin's end as it stood. That changes
   the get-up body's own geometry before the riser and keeper ever see it, not only a falling boot. The run shows it
   moved the ankle by at most 0.000001 m on these makes (the 0.4 m case in the research was not met), so as run it was
   a no-op; as built it is a wider change than the question's premise. Also the boot's mass/inertia is computed after the
   move, so a real shift would feed through to `lighter` and the gains; not exercised.

## UnityEditor, OwnKeeper, files to leave alone

- No `UnityEditor` in OwnRiser.cs, OwnBody.cs, RiseAgent.cs (grep count 0). AnkleMend.cs is under Tests and has none.
- OwnKeeper.cs: no diff. No guard or clamp added.
- Miner FBX import settings: no change in status.
- D:/Dev/WG status shows ProjectSettings/EditorBuildSettings.asset, PackageManagerSettings.asset and ProjectSettings.asset
  modified, Assets/ML-Agents untracked, and the four SmallRise*.onnx untracked. None is in the builder's diff by name and
  the EditorBuildSettings diff (an app-ui config object) and PackageManagerSettings diff look like what a Unity open
  rewrites; I cannot tell who made them. The master must not commit them with this step.
- Everything gameplay stays under Assets/_WonderGather; the .meta for AnkleMend.cs is present.

## The read-back: is it from the running body, and did it end by itself?

Yes to both, for the runs named (ank_rb_on3, ank_rb_off3).
- Switches are printed by `AnkleMendArguments.ReadBack()` from the static fields at the start, once per miner in
  ANKLE_RUNNING, and at the end; off3 reads AnkleLimit 0 / AnkleSpeed 0 / BootsFromOwnShin False / Unheard True; on3
  reads 1 / 20 / True / True. The gains are read from the drives (`boot.xDrive`), not from the fields.
- Ended by itself: runner.txt "total 1 passed 1 failed 0", xml result Passed, duration about 4.8 s of the test's
  clock against a Timeout of 1800000 ms; "ANKLE_READBACK_DONE 3 of 3 made down" printed by the test's last line. Zero
  "error CS" in both logs.
- Known-bad and known-good: the damper check (today's form 404/404 off, 0/404 on; B's form the reverse) and the target
  replica (equal off, 132 of 404 on) each discriminate. The C-at-make check does not (the same ~0 either way), as
  build.md says.

## Which of build.md's claims rest on a run, which on reading only

Run (the two logs, read to the end):
- Defaults and switches as read back; Unheard True in both.
- Damping written equals today's form off and B's smooth maximum on, on all three miners (every boot-step).
- Off: target equals today's form on every boot-step.
- On: target differs (up to 4.2 degrees); written target within 0.46 E of the reading.
- Boot ankle at make unchanged by C within 1 micrometre (2 compared per run).
- C at GoDown on Long: slackened about the own ankle off, about the let-go shin's end on (2 of 2 boots each), once each.
- The gains table (lighter, tau, k, c) matches the ANKLE_MAKE lines I checked (c 0.7791 off / 1.9291 on for Small,
  1.2965 / 6.8560 Long, 1.0187 / 6.8043 Round; k and limit equal).
- The earlier failed run (Long and Round know no way up in the game; only Resources/RiseWays/Small.txt exists) is
  reported as a fail, and the rig `-ankleWays Small` is named as a rig.

Reading or a script model only (not on the body):
- Peak boot speeds, zeta, c/k, the "no per-axis sqrt(2) Omega transient", the 19.0 rad/s numbers: the builder's own
  Python model fed with read-back inputs; I did not re-run it. The running body never turned a boot past 30 rad/s on
  either run (fastest 14.1 off, 8.7 on), so ANKLE_FAST/JUMP are untried on a real case.
- Bit-identical off behaviour; no linger after OnItsFeet; the tanh at its edge (max 0.46 E); A alone; AnkleLimit
  other than 0 or 1 (error path); the axis mapping of jointPosition under multi-axis turns.
- Comparisons between the on and off runs for jumps and speeds are between two different courses (one push each,
  random fall); e.g. |toStood| for Small was 22.56 off and 21.98 on, Long 0.57 / 1.66, Round 1.86 / 3.33. The
  difference cannot be attributed to a switch. build.md does not do so, but it puts "most jump 2.2-29.8 / 2.2-11.3" side
  by side; read that as two courses.

## Findings that are claims past the run (should-fix wording, no code change needed)

1. build.md's section 3 describes C as only the hand-over and make; make it plain that C also moves the made-down body's
   boot, toe and toe collider, and that the run could not tell (shift under 1 micrometre).
2. "Read back" statements about A: the on run has A and B together; build.md already says A alone was not read back.
   Keep that sentence next to any word that says A is checked.
3. With A on, `AnkleMended` does not apply `AnkleTurnsAtMost` (it calls AnkleFolds, not AnkleCan). Default 0, so no
   effect today, but it is a different behaviour from the switch's name; note it in the code comment or build.md.
4. Courses: three single pushes, strength 1, rig ways, not `-ownAsPlayed`, not the meadow. Nothing here is a claim
   about the game as played; build.md states this.

## Not checked

- Did not run Unity, the Python model, or any test; did not re-derive the folds math (fold, gtwist, swing saturation) or
  check them for singularities at half a turn beyond reading them.
- Did not trace keeper construction order against the riser's first Springs.
- Did not verify the physical effect of AnkleSpeed on planted boots (the 5 to 7 times damping risk is the builder's).
- Did not check who modified the ProjectSettings assets.
