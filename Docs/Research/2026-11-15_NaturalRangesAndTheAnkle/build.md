# The ankle mend's switches: the build (November 17, 2026)

Built from `research.md` (§1.4, §2.2, §4.1, §4.2, §6, §7, §8, §8.1) in `D:/Dev/WG`, on branch `claude/wip-round` at
head `3182e08`. Nothing is committed or staged.

**Rung reached:** written, compiled, and read back on a rig. On that rig each switch was set from the command line
and read back from the running body, on one short get-up of each of the three miners, with the switches on and
again with all of them off. **No course was measured.** The 24-course arms, the forced checks of §7.2 and the idle
proof were not run. Nothing here has been seen or accepted by the Game Director.

**Every switch is off by default.** To turn them on, give the tests these arguments:
`-riseSet AnkleLimit=1,AnkleSpeed=20 -ownBodySet BootsFromOwnShin=1`.

**The heard run is now `-ownUnheard 0`.** `RiseAgent.Unheard` is true by default, so a test with no `-ownUnheard`
runs unheard, as the game does.

## 1. The three miners worked numerically

**Every figure in this section is the model's, not the body's** (a free boot on a held shin, with no gravity and no
contact; its inputs were read back from the running body). The physicist recomputed them on its own and agrees to
the digits shown; that is two readings of one model, not a measure of the game.

**The script.** `ankle_build_miners.py` is my own copy, in the session scratchpad:
`C:/Users/luish/AppData/Local/Temp/claude/e2b675c90a3db94d/86f16be5-fb94-46d4-a96c-f3f3e90cd18a/scratchpad/ankle_build_miners.py`.
`ankle_speed2.py` was left untouched.

**The model.**
- One free boot, its shin held still, with no gravity and no contact.
- The engine's drive is modelled as implicit, axis by axis: w' = (I/dt·w + k·e) / (I/dt + k·dt + c).
- Each axis's torque is held within its own force limit.
- With B on:
  - the error is written as E·tanh(|e|/E)·ê, taken over all three axes as one vector;
  - c_B = (c⁴ + max(0, tau/Omega − k·dt)⁴)^¼;
  - Omega = 20·(H_Small/H)^½.

**The inputs.** The script was run with the values read back from the running body (run `ank_rb_on3`, `ANKLE_MAKE`
lines), passed in as `readback.json` beside the script. These inputs agree with my geometric estimates to the
digits shown. For example, Small's lighter is 0.005496 as read back; my estimate was 0.00550, and the research's
0.0065 was too high.

| Input | Where it comes from (D:/Dev/WG) | Only a running body gives it? |
|---|---|---|
| dt = 0.02 | `ProjectSettings/TimeManager.asset:6`; `lastStep = step` at `OwnRiser.cs:887` | no |
| k = min(has/FullAt, RuleTimesFeet·Rule²/dt²·lighter) | `OwnRiser.cs:373` (Springs); FullAt 35 and Damped .7 at `:115`; RuleTimesFeet 10 at `:127`; Rule .64038 at `OwnKeeper.cs:41` | yes: lighter and has are needed |
| c today = Damped·2·√(k·lighter) | `OwnRiser.cs:385`; written to the boot's three drive axes at `:387-390` | yes |
| tau (has) = knee·AnkleGives·Tone·StrongerWhileLearning | `OwnBody.cs:753` (`knee * AnkleGives`, AnkleGives .7 at `:51`); knee from `PhysicalBody.cs:156` (KneeHolds 1.6 at `:35`) through `OwnBody.cs:686`; Tone at `OwnRiser.cs:372` | yes: Tone and strength are read at the make |
| lighter | `OwnBody.Lighter`, `OwnBody.cs:573-585` | yes |
| H (the riser's `tall`) | `body.StandingHipHeight`, passed at `OwnBody.cs:944` | read back |
| the boot's inertia about each joint axis | the engine's tensor, from the box at `OwnBody.cs:763` and the boot's mass at `:749`, plus its mass at its middle's distance from the axis | yes: the engine computes it |
| AnkleSpeedHeight 0.605714 and the B damper | `OwnRiser.cs:146` and `:545-551` | no |

**Read back, per make, from both boots (identical to 4 digits):**

| | Small | Long | Round |
|---|---|---|---|
| H (tall), m | 0.605714 | 0.925748 | 0.697570 |
| tau (limit), N·m | 60.861 | 141.218 | 154.256 |
| lighter, kg·m² | 0.005496 | 0.009146 | 0.007187 |
| inertia about the joint's X, Y, Z, kg·m² | 0.005819, 0.003140, 0.004470 | 0.009603, 0.005807, 0.006335 | 0.007575, 0.004346, 0.005389 |
| boot's middle from its ankle, m | 0.0553 | 0.0555 | 0.0554 |
| k (the inertia cap binds), N·m/rad | 56.347 | 93.767 | 73.678 |
| c written, all off | 0.7791 | 1.2965 | 1.0187 |
| c written, B on (AnkleSpeed 20) | 1.9291 | 6.8560 | 6.8043 |

**Worked from the read-back:**

| | Small | Long | Round |
|---|---|---|---|
| E = tau/k | 1.080 rad (61.9°) | 1.506 rad (86.3°) | 2.094 rad (120.0°) |
| Omega | 20.00 rad/s | 16.18 rad/s | 18.64 rad/s |
| floor tau/Omega − k·dt | 3.043 − 1.127 = 1.916 | 8.729 − 1.875 = 6.854 | 8.277 − 1.474 = 6.803 |
| c with B (times today's) | 1.929 (2.48×) | 6.856 (5.29×) | 6.804 (6.68×) |
| ζ near the target, today / with B | 0.70 / 1.73 | 0.70 / 3.70 | 0.70 / 4.68 |
| c/k, today / with B | 14 ms / 34 ms | 14 ms / 73 ms | 14 ms / 92 ms |
| peak boot speed, 140° jump, today (per axis) | 62.7-66.7 rad/s | 62.7-66.2 rad/s | 62.7-66.4 rad/s |
| peak boot speed, 180° jump, today (per axis) | 80.6-85.8 rad/s | 80.7-85.1 rad/s | 80.6-85.4 rad/s |
| peak boot speed, 140° jump, with B | 19.0 rad/s | 14.4-14.5 rad/s | 14.7-15.0 rad/s |
| peak boot speed, 180° jump, with B | 19.5-19.6 rad/s | 15.4 rad/s | 16.3-16.4 rad/s |
| already turning at 40 rad/s, 140° jump, with B | 21.3 rad/s | 16.3 rad/s | 16.4 rad/s |

The written c on the running body equals the model's c in both the on and the off case. This checks the gains
only, not the motion.

**The per-axis √2·Omega transient did not appear in this model.** For a jump about the X+Z diagonal, with B on:
- each axis peaks near Omega/√2: 13.4 for Small, 10.2 for Long, 10.5 for Round;
- the speed taken as one vector peaks at Omega.

No axis went above Omega, because B limits the error as one vector over the three axes. On the running body this
is unmeasured. The model leaves out contact, the shin's own motion, and the coupling between the axes in a joint
that turns about all three.

**A risk to look at before the arms.** With B on, Long's and Round's damping is 5.3 and 6.7 times today's
(ζ 3.7 and 4.7). A boot that is planted and asked to move will be slow to follow: c/k is 73-92 ms against 14 ms
today. The research's §4.2 floor does this. Whether that costs footing on the planted stages is for the arms to
show.

**What only a running body can give, and has not given yet:**
- the boot's actual peak speeds through a whole get-up;
- the target jumps on stages where the ask nears half a turn;
- the speeds while touching the ground and while touching nothing.

None of these events was forced in this run (§4 below).

## 2. Step 2: RiseAgent.Unheard is true by default; the tests' new arguments

**RiseAgent.Unheard.** It is now `public static bool Unheard = true;` at `Assets/_WonderGather/Learning/RiseAgent.cs:112`,
with a comment at `:108-111`. Every place that reads or sets it (from a grep over all of Assets):

| File:line | What it does |
|---|---|
| `RiseAgent.cs:126` | Reads it: `if (Unheard \|\| riser == null) return;`. This is the only runtime read. |
| `GettingUpByItsOwnTests.cs:24` | Keeps the value it found (`unheardWas`). |
| `GettingUpByItsOwnTests.cs:33` | Restore puts that value back (it used to set false). |
| `GettingUpByItsOwnTests.cs:55-56` | Sets it only when `-ownUnheard` is given. Without it the test now runs unheard, as the game does. |
| `AnkleMend.cs` (`AnkleMendArguments`) | Save, Apply and Restore for the other tests. |

No runtime code sets it.

**The heard run is now `-ownUnheard 0`.** Before this change:
- a test with no `-ownUnheard` ran heard;
- `CoverRingLiveTests` never read the argument at all, so its runs were heard whenever a model was given.

Now both run unheard unless `-ownUnheard 0` is given.

**`CoverRingLiveTests`, new arguments.** They are listed in its header comment and handled by the shared
`AnkleMendArguments` in the new file `Assets/_WonderGather/Tests/PlayMode/AnkleMend.cs`:

| Argument | What it does |
|---|---|
| `-ownUnheard <0\|1>` | Sets RiseAgent.Unheard, only when given. |
| `-riseSet Name=value,...` | Sets any public static number or switch of OwnRiser, by reflection, as `OwnRiseBench.cs:395-402` does: a bool from a value over .5, an int, or a float. A name that is missing or read-only fails the test. |
| `-ownBodySet Name=value,...` | The same, for OwnBody. |
| `-liveStrength <s>` | Calls `MinerWorkPreview.SetStrength` after the miner is chosen and before its own body is made. The old strength is put back in TearDown. |

**Restoring.** `AnkleMendArguments.Save()` runs first in Load. `Restore()` runs in TearDown: it puts every field
it set back in reverse order, then RiseAgent.Unheard. The new test also restores `OwnBody.RiseWays`,
`PhysicalFall.GetsUpByItsOwn` and `Time.captureFramerate`.

**What is logged.** `CoverRingLiveTests` logs:
- `LIVE_SET`: what the arguments set;
- `LIVE_SWITCHES`: the switches as the running body reads them, with the strength.

It also runs the ankle watch (`AnkleWatch`) on every course, and at the end writes `ankle.txt` to its output folder.

## 3. Step 3: the switches (all off by default)

**When every switch is off,** the only new statements that run are two condition tests:
- in `Drive`: `j >= Foot && AnkleSpeed > 0`;
- in `Aims`: `j >= Foot && (AnkleLimit != 0 || AnkleSpeed > 0)`.

There are also two writes, `stoodAnkle[i] = anchors[Foot+i]` at OwnBody's plain make. They have no behavioural
effect and allocate nothing.

**The new fields.**
- OwnRiser: three static floats and one static bool.
- OwnBody: one static bool and two arrays of two `Vector3`, allocated once per OwnBody.

Nothing new is allocated per step. With every switch off, the drive targets and damping written read back as
today's form on all three miners (§4).

### A. `OwnRiser.AnkleLimit` (`OwnRiser.cs:146`, code at `:445-525`)

| Value | What it does |
|---|---|
| 0 | As before. |
| 1 | The fold-safe limit described below. |
| any other | `Debug.LogError` once, then it acts as 0. |

With 1, `drivenNow` (what learning adds) is added **before** the limit. Then:
1. The ask from how the boot stands, q with w ≥ 0, is split into a twist about the shin's long line and a swing.
2. The swing is saturated toward the reach, ρ/(1+ρ⁴)^¼.
3. The twist is saturated as reach·tanh(τ/reach).
4. Both are folded back toward how the boot stands: fold = 1 − (1 − w_swing²)⁸ and g_twist = 1 − (1 − w_twist²)⁴.

The reach is AnkleAtMost (110°), or 110 if AnkleAtMost is 0. At half a turn the target is how the boot stands,
whichever way the ask is read. Nothing in it switches at a threshold.

**Choices made in the build, to check against the research:**
- **The twist reach equals the swing reach, so the limit is isotropic.** This is my reading of "isotropic 110°". A
  real ankle twists far less than it swings. This is an open choice and is not Locked.
- **wayAsks with A on is the way's ask before drivenNow,** as `Aims` keeps it.
- **The shin's line in the boot's frame** is Inverse(rested[Foot]·toStood)·up.

### B. `OwnRiser.AnkleSpeed` (`:146`; `AnkleSlows` at `:530-541`, `AnkleDamper` at `:545-551`)

| Value | What it does |
|---|---|
| 0 | As before. |
| above 0 | Omega for Small, in rad/s, slower by (AnkleSpeedHeight/tall)^½ for a taller body. |

With B on:
- **The target** is written within E = forceLimit/stiffness of the joint's present reading, as
  at + off·E·tanh(|off|/E)/|off|. The difference off = target·Deg2Rad − jointPosition is taken as it is, never
  wrapped round. Units: `SetDriveTarget` takes degrees; `jointPosition` is in radians; stiffness is per radian.
- **The damper** is (c⁴ + max(0, has/speed − spring·lastStep)⁴)^¼. It is written in `Drive`, that is, when the
  springs are set and when a limb slackens.

**Two cautions for B:**
- Set B before a get-up starts. A change made in the middle of a get-up reaches the damper only at the next `Drive`
  call.
- `AnkleSlows` reads `jointPosition[0..2]` as the joint's X, Y, Z. The anchor rotation is the identity, so these are
  the boot's own axes. **That mapping is unverified when the boot is turned about several axes at once.**

### C. `OwnBody.BootsFromOwnShin` (`OwnBody.cs:250`; make at `:725`, `:754-761`, `:770`, `:856`; stood value at `:927`; GoDown at `:1361-1363`)

**At the make (with the body made down and its stood pose known):**
- each boot is placed where its ankle was from the shin's end in the body made standing (`stoodAnkle`), with its
  turn kept;
- its toe is moved with it.

**At GoDown:** each boot is slackened afresh about the end of the let-go body's own shin. Calling `Slacken` a
second time replaces the first call's point.

**What else C touches (the sceptic's review):** it is more than a falling boot's hand-over. At the make it refits
the made-down body's boot, its `toeAt` and its toe's collider. At GoDown the second `Slacken` acts on every fall
once the stood pose is known, Long's and Round's too: they fall by GoDown, then get up the old way. So C stays off
by default until a course that meets the 0.4 m case of the research has been run with it.

### D. AnkleBrakes

Not built, as asked.

### Logging (test code only)

**`AnkleWatch`** (in `AnkleMend.cs`, execution order 30002) writes these lines:

| Line | What it says |
|---|---|
| per step | The targets actually written, from the drives. |
| `ANKLE_JUMP` | A jump over 60° in one step, both as written and as a turn. |
| `ANKLE_FAST` | A boot's per-axis joint speed over 30 rad/s, split by whether the boot or its toe touched anything (`BootTouch` on each), and by riser or keeper. |
| `ANKLE_MAKE` | Per make: lighter[foot], \|toStood[Foot]\|, tall, springOf, hasOf, the written k, c and limit, the boot's mass, tensor and anchor, and its inertia about each joint axis. |
| `ANKLE_DOWN` | Per GoDown: which point each boot was slackened about. |
| `ANKLE_DONE` | The summary, with the read-back counts. |

A push from the panel lets the body go without `GoDown`, so it is counted apart and asks no slackening. `-liveEnergy 2`
already writes the energy split and the ankles' contacts; the watch does not repeat them.

## 4. Compile and read-back

**The test.** `AnkleMendTests.TheSwitchesReadBack` (`[Explicit]`, in `AnkleMend.cs`):
1. Loads TheOrdinaryPlace and calls `CaptureTools.AsItWas`, then sets the switches from the arguments, at 50 frames
   a second, with `PhysicalFall.GetsUpByItsOwn` true.
2. For each miner (`-liveWho`, or Small, Long and Round):
   - makes its own body, waits until it stands at ease, and watches it 0.3 s standing;
   - pushes it over forward, waits for the riser (at most 40 s), and watches 4 s of the get-up;
   - takes it back.

**Read back:**
- the switches at the start, per miner (`ANKLE_RUNNING`), and at the end;
- **B's damper:** the written damping against today's form and against B's form;
- **A or B:** the written target against today's form of the same ask (Aims and AnkleCan replicated, with drivenNow
  read from the riser);
- **C at the make:** the boot's ankle against where it was in the body made standing;
- **C at GoDown:** the slackening point.

**Compiled:** yes. Every run below compiled with no `error CS`.

| Run | Command (in `D:/Dev/WG`; each with `LIMIT=1800 bash Docs/Technical/AnyBody/tools/unity_tests.sh`) | Outcome |
|---|---|---|
| `ank_readback` | `"WonderGather.Tests.AnkleMendTests" -riseSet AnkleLimit=1,AnkleSpeed=20 -ownBodySet BootsFromOwnShin=1 -ownUnheard 1 -liveStrength 1` | **Failed** (the test's own assertion): only Small was made down to get up by its own body. Long and Round know no way up in the game: only `Resources/RiseWays/Small.txt` exists, so they get up the old way. Small read back as in on3. |
| `ank_rb_on3` | the same, plus `-ankleWays Small` (a rig: every miner given Small's ways through `OwnBody.RiseWays`) | **Passed**, 3 of 3 made down; ended by itself (4.8 s of test time). |
| `ank_rb_off3` | `"WonderGather.Tests.AnkleMendTests" -ankleWays Small` (every switch at its default) | **Passed**, 3 of 3 made down; ended by itself. |

Three earlier runs (`ank_readback_on`, `ank_rb_on`, `ank_rb_on2`) were runs of the same test before its C checks
were finished. They gave the same gains and damping matches. They are superseded and are not counted.

**Read back from the running body:**

| Check | All off (`off3`) | All on (`on3`) |
|---|---|---|
| Switches as read | AnkleLimit 0, AnkleSpeed 0, BootsFromOwnShin False, Unheard True | AnkleLimit 1, AnkleSpeed 20, AnkleSpeedHeight 0.605714, BootsFromOwnShin True, Unheard True; strength 1 (the panel's 1) |
| Damping written as today's form | Small 404/404, Long 266/266, Round 404/404 | 0/404, 0/266, 0/402 |
| Damping written as B's smooth maximum | (B off: the same as today's) | 404/404, 266/266, 402/402 |
| Target written as today's form of the ask | 404/404, 266/266, 404/404 (most apart 0.000°) | 132/404, 99/266, 166/402 (most apart 4.20°, 0.17°, 0.81°) |
| Target's distance from the reading, in shares of E (most) | 0.327, 0.195, 0.240 | 0.461, 0.132, 0.269 |
| Boot's ankle at the make, from where it was standing | ≤ 0.000001 m | ≤ 0.000000 m |
| GoDown slackening (Long only; once in each run) | about its own body's ankle, 2 of 2 boots (shin's end 0.76 mm and 7.0 mm off; the two points 11.6 mm and 15.2 mm apart) | about the let-go shin's end, 2 of 2 boots (own ankle 0.55 mm and 0.78 mm off; the two points 1.6 mm and 1.4 mm apart) |
| Target jumps over 60°, boot speeds over 30 rad/s | none (most jump 2.2-29.8°; fastest 14.1 rad/s touching) | none (most jump 2.2-11.3°; fastest 8.7 rad/s touching) |

**What these show.** Each check was run once on a known-good case and once on a known-bad one:
- **B's damper is read back exactly:** today's form matches only when B is off, and B's form matches when B is on.
- **The target replica:** with everything off it equals what the riser wrote on every boot-step. With A and B on it
  differs on 60-67% of them. The difference does not separate A from B, because both act together in this run.
  **A was not read back alone.**
- **E:** with B on, the written target stayed within 0.46 E of the joint's reading. Off, it was within 0.33 E: no
  ask on these short get-ups came near E, so **the tanh was not tested at its edge.**
- **C at the make: no effect on these makes.** The body made down already has its boot's ankle at the shin's end,
  within 1 µm, whether C is on or off. This check cannot tell on from off on these courses; the 0.4 m case in the
  research was not met here.
- **C at GoDown: read back on Long, off and on.** It happened once in each run. On these courses the two points were
  1-15 mm apart.

**The courses.** The courses are not counted. They are one push forward per miner, three courses per run, at
strength 1. They ran in TheOrdinaryPlace at its defaults, but without `-ownAsPlayed 1`. **Long and Round ran on
Small's ways (the rig).** `-liveStrength 1` was read back as 1. The on run and the off run are one course each per
miner: they show the switches acting, not an effect of the switches.

## Not done, not tested

**Runs not done:**
- The 24-course arms, the forced checks of §7.2 and the idle proof, as asked.
- Any run with `-ownAsPlayed 1`.
- Any run on the meadow's uneven ground on purpose.
- Any strength other than 1.

**Code that compiled but was not run:**
- `CoverRingLiveTests` with its new arguments and its watch.
- `GettingUpByItsOwnTests` after its Unheard change.

**Not read back:**
- A alone, without B.
- AnkleLimit at a value other than 0 or 1 (the error path).
- B's tanh near E.
- The joint-axis mapping when the boot is turned about several axes at once.
- Any target jump over 60° or boot speed over 30 rad/s. None happened, so the `ANKLE_JUMP` and `ANKLE_FAST` lines
  are untried on a real case.
- The `BootTouch` split by contact, beyond "touching" being seen on every run.

**Not in the game, and not settled:**
- **Long and Round know no way up in the game,** so A and B act on their own bodies only on a rig that lends them
  Small's ways. In the game they reach Small alone.
- **Open:** the isotropic twist reach, and whether B's 5-7 times damping on Long and Round costs footing.
- Bit-identical behaviour with every switch off was not proven by a course-by-course comparison. It rests on the code
  paths and on the off run's read-back matching today's form on every boot-step.

## After the reviews (November 17, 2026, by Opus 5.5, the session's master; not by the builder)

The three reviews (`review_physicist.md`, `review_sceptic.md`, `review_code.md`) found nothing that must be fixed.
Their should-fix points, and what was done with each:

**Mended in the code:**
- **An unusable AnkleSpeed acts as off and says so** (code reviewer, 1). `OwnRiser.AnkleSpeedHere()` gives B's
  speed for this body. A speed of 1e-3 rad/s or less, one that is not a number or endless, or a height of none, logs
  one error and acts as off. Before, a zero or tiny `AnkleSpeedHeight` gave an endless damper. The drive, `Aims` and
  `AnkleMended` now ask it, not `AnkleSpeed > 0`; with AnkleSpeed 0 it returns at once. A negative AnkleSpeed was
  silently off before; now it says so.
- **`-ownUnheard` takes only 0 or 1** (code reviewer, 2), in `AnkleMend.cs` and `GettingUpByItsOwnTests.cs`. Anything
  else fails the test, since an argument left without its value takes the next argument's name.
- **AnkleTurnsAtMost says it is not applied with AnkleLimit 1** (code reviewer, 3): one warning.
- **With A on, what learns adds is put on as a turn** (physicist, 2): `own * FromRotationVector(drivenNow)`, after
  the way's ask, in place of adding it to the ask's rotation vector, which jumped where the ask crosses half a turn.
  Only a heard run has anything to add; as played nothing. **Open:** the model was taught with the two added as
  targets, so whether this order is the one it would want is not known.
- The numbers in these messages are printed with a point, whatever the computer's language (the first bad-value run
  printed `0,605714`).

**Written in this note** (the sceptic, 1 and 2): §1's figures are the model's; what else C touches (§3, C); the on and
off runs are single courses (§4).

**Read back after the mends** (`AnkleMendTests`, `-ankleWays Small`, the same rig as §4; every run ended by itself):

| Run | Arguments | Result |
|---|---|---|
| `ank_fix_on3` | every switch on, `-ownUnheard 1 -liveStrength 1` | **Passed**, 3 of 3 made down. Read from the running body: AnkleLimit 1, AnkleSpeed 20, AnkleSpeedHeight 0.605714, BootsFromOwnShin True, Unheard True. Small: damping in B's form on 404 of 404 boot-steps; no target jump over 60° (most 11.3°); no boot over 30 rad/s (fastest 8.7 touching); written within 0.462 E. |
| `ank_fix_bad` (must catch) | `AnkleLimit=1,AnkleSpeed=0.000001,AnkleTurnsAtMost=5`, Small | Caught: one error ("cannot be used, so it acts as 0 (off)") and one warning (AnkleTurnsAtMost not applied); damping in today's form on 404 of 404 boot-steps, B's on 0. |
| `ank_fix_unheard2` (must fail) | `-ownUnheard 2`, Small | **Failed**, as it must: "-ownUnheard takes 0 (the learnt model heard) or 1 (unheard), not 2." |
| `ank_fix_Aalone` | `-riseSet AnkleLimit=1 -ownUnheard 1`, all three | **Passed**, 3 of 3 made down; read back AnkleLimit 1, AnkleSpeed 0. The target written as today's on 178 of 404 boot-steps (Small), 212 of 404 (Long), 192 of 402 (Round): A changes about half of them. No target jump over 60° (most 11.4°, 15.1°, 11.0°); fastest boot touching 9.5, 12.0, 13.8 rad/s. With B off, today's damper and the smooth maximum coincide (404 of 404 each), so that column tells nothing here. |
| `ank_fix_limit2` (must catch) | `-riseSet AnkleLimit=2,AnkleSpeed=-3`, Small | Caught: each logged one error, its numbers with a point; both acted as off: the target written as today's on 404 of 404 boot-steps (most apart 0.000°), damping as today's on 404 of 404. The test itself passes: it is a read-back, and these errors are its message. |

So the sceptic's "A alone never read back" and the code reviewer's error path are now read back on this rig. Still
not read back: B's tanh near E, the engine's handling of a target outside [−π, π] (the physicist, 1), and the
joint-axis mapping under turns about several axes. These go into step 4's forced checks with the rest of §7.2.

**Long and Round: what their courses can show.** This is the order of the work, set by Opus 5.5. It is not a design
decision, and nothing in it is Locked.
- In the game they know no way up: only `Resources/RiseWays/Small.txt` exists. Standing by their own joints, they
  fall by `GoDown` (`OwnBody.cs:1139`, `:1144`) and get up the old way (`PhysicalFall.ByItsOwn`: "no way up for lying
  so"). So **A and B never act on them as played; C does, at their falls.**
- Their own get-up belongs to the new controller (`motion_design.md`: the first build takes every fall of every
  miner), not to ways written for each body: Luis's word of October 30 rules out teaching each body its own. So no
  ways are made for them here.
- **Small** takes every arm of §7.2 as played, at three strengths. **Long and Round as played** take arm 0 and arm 4
  (all on, where only C acts): their falls and the old way, counted for the first time (§7.2's table has "Long and
  Round unknown"). **On the rig that lends them Small's ways**, arms 0 and 3 at strength 1, and only if Small's arms
  show the mend helps. That is a claim about the rig: whether the bound holds on their longer and heavier boots.
  `CoverRingLiveTests` must first learn `-ankleWays`.
- **Small's heard baseline repeats `cc_es24`**: the same command line with `-ownUnheard 0`. If its 24 courses come
  out as `cc_es24`'s did, course by course, then the off path is the old build's, shown on courses and not only on
  the code paths. If they differ, the off path or the run is not what it was believed to be, and that is read before
  any arm.

The course counts differ a little from §4's on3 run (for Long, 268 boot-steps against 266). Different numbers on one
course say nothing either way; the composition of what learns adds changed only by rounding when nothing is added.

**The mends reviewed** (`review_mends_code.md`, `review_mends_physicist.md`; two agents who made none of it; read
only, while a run went on). Nothing that must be fixed.
- **The code reviewer:** the five mends do what they say, and with every switch at its default no line they changed
  alters what the game computes. That is argued from the code, not shown on courses. Two should-fix points:
  1. the new order of what learns adds was never run with anything to add, since every short run was unheard. To do:
     one heard Small run with A on (`-ownUnheard 0 -riseSet AnkleLimit=1`), counting target jumps over 60°;
  2. the three once-only flags are static and never reset, so in one editor session without a domain reload a second
     test with a bad value would print nothing. To mend after the runs: reset them in `AnkleMendArguments.Restore`.

  It also wrote that `ank0_small_heard.log` "shows `RiseAgent.Unheard True` and `AnkleLimit 1`". Checked against the
  log, that is not so: its read-back line says `RiseAgent.Unheard False; OwnRiser.AnkleLimit 0`, the learnt model's
  academy is made from `RiseAgent.Heard` (`OwnBody.cs:1130`), and the log holds no "Unheard True" at all. It noted
  rightly that `ank_fix_bad` printed `0,605714` because it ran before the mend for the point; only `ank_fix_limit2`
  shows that mend.
- **The physicist** (a numpy script, 400 000 samples): the code is `own * FromRotationVector(d)` and reaches
  `AnkleFolds`. It is continuous through half a turn, where the taught sum jumps by up to 44°. It is not the taught
  sum: the two are a median 9.2° apart and at most 38° over the box of what learns adds (0.8° near no turn, 17.7° near
  half a turn), and they agree to first order only at no turn. The other order, `FromRotationVector(d) * own`, is
  exactly as far from the taught sum on every sample, so the numbers do not choose an order. What would decide it:
  the trainer's code, or the orders as switchable options in heard runs. It stays open, and as played nothing is
  added. Also noted: with A on, `wayAsks` (what the model is shown) holds the way's ask before the limit. That is not
  new, and it matters when training.
