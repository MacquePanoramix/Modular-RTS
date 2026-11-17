# Code review of the ankle mend build (reviewer: code, did not build it; November 17, 2026)

Question asked: is the code right as code? Nothing was compiled or run by me (no Unity, as told). I read the whole diff of
`D:/Dev/WG` (RiseAgent.cs, OwnBody.cs, OwnRiser.cs, CoverRingLiveTests.cs, GettingUpByItsOwnTests.cs) and the whole new
file `Tests/PlayMode/AnkleMend.cs` (500 lines), and the surrounding code they call (Aims, AnkleCan, Springs, Drive, Slacken,
Make, GoDown, PhysicalFall.Slacken, CaptureTools.Argument, MinerWorkPreview.SetStrength). I did not touch the files Luis keeps
out (EditorBuildSettings, ProjectSettings, PackageManagerSettings, ML-Agents, the .onnx files); they show in `status` and are
not part of this step.

## Verdict

No finding that stops the step. I found no compile error and no warning by reading (the `Summary` format string has 35
placeholders and 35 arguments in the right order; no name clashes in `ReadBack`'s sibling scopes). Off, the runtime adds two
comparisons and allocates nothing. On, the per-step path allocates nothing (structs, `Math.Tanh`, no LINQ, no closures; the one
`Debug.LogError` is once). A few hardening points and nits below; none changes a result unless the input is bad.

## Argument parsing (AnkleMendArguments)

- Invariant culture: `float.Parse(bits[1], Inv)` and `Convert.ToString(..., Inv)` are right. `-liveStrength` the same.
- Bad input fails the test loudly, which is right: no `=` or more than one (`Assert bits.Length == 2`), unknown or non-public
  name (`GetField` with `Public | Static`; case-sensitive), const or readonly, a non-number (`FormatException`). A value of
  `NaN` or `Infinity` parses and is set (see should-fix 1). An empty value (`AnkleLimit=`) is a raw `FormatException`, not the
  friendly message; harmless.
- **`-ownUnheard` is lenient (should-fix 2).** `unheard == "1"`: any other word (`true`, `2`, `0 `, a following switch because
  `CaptureTools.Argument` returns the next token even if it is another flag) silently means "heard". For the arm that decides
  heard-versus-unheard this should be `0` or `1` and nothing else, or the run should say what it read. `LIVE_SWITCHES` /
  `ANKLE_SWITCHES` do print `RiseAgent.Unheard` as read back, which is what saves it; the strict parse would catch it earlier.
  Same line in `GettingUpByItsOwnTests` (`:56`).
- A `-riseSet` / `-ownBodySet` field of another type than bool, int, float (enum, delegate) hits `SetValue` with a float and
  throws `ArgumentException`: loud, fine. Bool is `value > .5` as in OwnRiseBench.
- `-riseSet` only reaches `CoverRingLiveTests` and `AnkleMendTests`; `GettingUpByItsOwnTests` reads `-ownUnheard` only. Say so
  in the arms' brief (the header comment of CoverRingLiveTests does, the other file's does not).

## Statics put back (restore on failure)

- `Save()` in `Load` first; `Restore()` in `TearDown`; both new fixtures. `was` is a stack replayed in reverse, so a name set
  twice returns to the first value. `Restore` runs after a failed `Apply` (the `was.Add` precedes the `SetValue`, so a
  half-applied set is still undone). `RiseAgent.Unheard` is saved in `Save` and restored. Good.
- `GettingUpByItsOwnTests`: `unheardWas` captured in `Load`, put back in `Restore`; this replaces the old "set false". It also
  now puts back a value some other fixture left, which is right.
- `AnkleMendTests` restores `OwnBody.RiseWays` (default is an empty array, not null, so the `waysWas != null` guard is
  harmless), `GetsUpByItsOwn`, `captureFramerate`. `CoverRingLiveTests` puts the strength back only if it set it.
- Not restored (nits, no effect on the next fixture): `OwnRiser.ankleLimitSaid` (a private static, so the "said once" error is
  once per domain, not per test; with domain reload off a later run would show no error line); `LogAssert.ignoreFailingMessages`
  set true in `AnkleMendTests` and never put back (the same pattern exists in the other fixtures; but it means the
  `Debug.LogError` for `AnkleLimit` not 0 or 1 cannot fail a run, it only appears in the log; grep the log for it).
- `Save()` while `saved` is true (a fixture whose TearDown did not run) clears `was` and loses the first values. Cannot happen
  with NUnit's TearDown guarantee; mention only.
- In `CoverRingLiveTests.TearDown` the statics are restored before the body is destroyed (the body still exists for the rest of
  that method). It runs between frames and `Drive`/`Aims` do not run there; fine, but putting `Restore` last would be tidier.

## Order of operations inside the step

- `OwnBody.FixedUpdate` (execution order 41) calls `riser.Step`; `Step` sets `lastStep`, runs `Springs` (only if Tone or
  StrongerWhileLearning changed, which includes the first call), eases `drivenNow`, then the stages and `Aims`. So the damper
  (`Drive` → `AnkleDamper`) uses the `lastStep` and `tall` of this step, and `AnkleSlows` reads the joint reading left by the
  previous physics step, the same one for every axis. Consistent.
- `AnkleMended` order, `limits`: what learns adds first (`asked + drivenNow[j]`), then the fold, then (if on) the slow. With
  only B: clamp (`AnkleCan`), then add, then slow. `wayAsks[j]` is the ask before `drivenNow` in both, as `Aims` keeps it.
  Matches the comments.
- `AnkleFolds` arithmetic read through: `swing.w` is `n >= 0` by construction (q.w >= 0 after the flip, twist w >= 0), so the
  `Abs` and `Clamp01` do not change a result; `Eighth` is x^8, `gTwist` is (1-w^2)^4, `fold` the swing's (1-w^2)^8; the
  `Rad2Deg` / `FromRotationVector` degrees/radians are consistent. Smooth forms only, no threshold (the `n > 1e-6`, `vLong > 1e-9`
  and `angle > 1e-6` tests only avoid a division by zero and pick the limit value; the `Max(0, ...)` clamps are guards against
  rounding). Whether the geometry is right (the shin's line, the twist/swing order) is for the physicist, not me.
- **`AnkleLimit=1` silently ignores `AnkleTurnsAtMost`** (should-fix 3, low). With A off, `AnkleCan` applies the turn-rate limit
  (`RotateTowards`); `AnkleFolds` does not (it only sets `ankleAsked`). It is 0 by default and the watch prints "unsure:
  AnkleTurnsAtMost is on" when it is set, so no run is misread, but `AnkleLimit` 1 with `AnkleTurnsAtMost` above 0 is a
  quiet combination: either log it once like the bad-value error, or say it in the comment.
- `OwnKeeper.cs` is untouched; no guard or clamp was added there. Good.
- `OwnBody.GoDown`: the first `fall.Slacken(i, ankleAt[i], ...)` is made and then, with C on, made again with the shin's end.
  `PhysicalFall.Slacken` rewrites footAtWas/To, toeAtWas/To and `slackened[i] = 0` from scratch, so the second call fully
  replaces the first (read at `PhysicalFall.cs:902`); `SlackenToes` after it then reads the right `footAtTo`. Correct, but
  wasteful and easy to misread; an `if (...) else` would say it once (nit). `OwnBody.Shin` and `PhysicalFall.Shin` are both 9.
  `miner.Solved.shins[i]` is used without the null/length check the test makes (`AnkleMend.cs:340`); it is already used a few
  lines up in `Make` and the old code read `miner.Solved`, so I take it as safe.
- `OwnBody.Make` with C: the boot is moved after `Made` and before it gets its `ArticulationBody` (those are added later in the
  jointed loop), so moving the transform is safe; `anchors[Foot+i]` is computed after the move, so it equals `stoodAnkle[i]` by
  construction; `toeAt` is computed twice (first unshifted, then overwritten under `refits`), a harmless double compute (nit:
  make it an if/else). The toe `SetPositionAndRotation` then position override: right. `stoodAnkle` is written only in the
  plain make, while `stoodKnown` is set later (`:1149`): `refits` needs both, and the array is always set by the time
  `stoodKnown` is, so there is no read of an unset value.
- **Test-watch ordering:** the watch runs at order 30002 in `FixedUpdate`, after `OwnBody` (41) and `CoverPair` (30001). So its
  `xDrive.target` is what the riser wrote this very step, and `jointVelocity` / `jointPosition` are the previous sim step's.
  Right for "target actually written". `BootTouch`'s `OnCollision*` run after the sim and before the next `FixedUpdate`, and are
  cleared each step: right.

## Logging: the target written, and per-axis speeds

- The jump and the replica use `boot.xDrive/yDrive/zDrive.target` read back from the drives, not `wayAsks`. Right.
  `ANKLE_FAST` uses the per-axis `jointVelocity[d]` for `d < dofCount`. Right. Units noted (degrees for targets, radians for
  reading and speed) and `OffOverEMost` converts correctly.
- Weak spot in the "as today's form" replica (`ReadBack`): it recomputes from `riser.Aim(j)` (= `aimNow[j]`) every step, but
  `Aims` for a boot is only called when the stage turns the boot (`turnsBoot`, `OwnRiser.cs:708`), so on other steps the
  replica compares a stale aim against a target that was not rewritten. With all switches off the two are equal by
  construction, which is what the off run showed; with the switches on the "132/404" kind of count therefore also contains
  steps in which nothing was written. Read the A and B counts as "different on at least that many steps", not as a rate of
  change. (The builder already says A alone was not separated from B.)
- `DampTodaySame` / `DampBSame` compare with `Time.fixedDeltaTime` where the riser uses its `lastStep`; the same 0.02 here
  (captureFramerate 50, TimeManager 0.02), so they agree; a different fixed step would show as a mismatch, not a false match.

## File writing and slowing a run

- `ankle.txt` is one `File.WriteAllText` at the end of the course, from at most 60 lines (`LinesAtMost`) plus the summary.
  Written before the blow-up assertions, so a failing course still leaves it. `Debug.Log` is capped by the same 60. Nothing is
  written per step. It cannot slow a run measurably and cannot break one unless the folder is not writable (it is the folder
  the test already writes to).
- Per-step work in the watch: two legs; `jointVelocity`, drives, a few `Mathf` calls; `Said` is called only on an event, but
  it builds its string with `string.Format` before it checks the cap, so a run in which a boot is above 30 rad/s on every step
  keeps allocating after line 60 (test-only; nit: check `Lines.Count` first).
- The watch adds a `BootTouch` (a `MonoBehaviour` with `OnCollisionEnter/Stay`) to each boot and toe link. The header comment
  says "reads only; writes nothing to the body". That is true of the state, but contact callbacks are turned on for those links,
  which can cost a little time and, in principle, change which pairs the engine reports (not the solution). Say "adds
  collision listeners" where the build note says "reads only". Runs with and without the watch were not compared (not
  checked by me).
- Leg 1's boot is not null-checked in `FixedUpdate` (`Own.Part(Foot+1)` is used at `boot.jointVelocity`); only leg 0 is
  checked. The two parts are made together, so low risk.

## Should-fix (small)

1. `AnkleDamper`: `AnkleSpeed` or `AnkleSpeedHeight` of 0, tiny, NaN or negative via `-riseSet` gives `speed` 0 / tiny / NaN →
   `has / speed` infinite or NaN → an infinite or NaN damper written to a drive (physics blows up or silently zeros). Normal
   values are fine (`tall >= .1` by the ctor). Cheap hardening: treat `speed <= 1e-3` as off, or `Mathf.Max`.
2. Strict `-ownUnheard` (above).
3. `AnkleLimit=1` with `AnkleTurnsAtMost > 0` (above).

## Not checked

- Compiling, warnings from the compiler (read only), any run. The geometry and the physics of A, B, C (physicist's question).
- Whether the OwnBody make with C at the 0.4 m case behaves (the build says it was not met).
- Other tests that may have relied on `RiseAgent.Unheard` being false: a grep over Assets shows only RiseAgent.cs and the two
  fixtures read or set it, but I did not read the training code in `Learning/` or the ML-Agents trainer path for any
  dependence on the heard hook with Academy automatic stepping on.
- Whether `-ownAsPlayed 1` is read anywhere in the new code (it is not; the build says no run used it).
