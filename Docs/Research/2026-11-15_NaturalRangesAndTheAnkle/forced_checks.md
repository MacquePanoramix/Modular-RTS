Switches each check needs on: 1 A and B (all off as its known-bad); 2 A; 3 C (C off as its known-bad); 4 B; 5 B (with every switch off too); 6 none (today's path); 7 whichever is on (all three on here); 8 none (AnkleLimit 2, a wrong value). A = OwnRiser.AnkleLimit, B = OwnRiser.AnkleSpeed (with AnkleSpeedHeight), C = OwnBody.BootsFromOwnShin; all off by default, and they stay so.

No fault was found in A, B or C. Nothing on their paths, and nothing on the off path, was changed: arm B (ank1_small) need not be run again.
The mend of the checks (the second build, after the four reviews `review_forced_*.md`) is commit 7c6edf0 on claude/wip-round (pushed). Only `Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs` changed.
Check 3's bar does **not** separate C off from C on on every course (below): that finding is open, not mended.

# Step 4a: the forced checks (November 17, 2026)

Builder: Opus 5.5, in the worktree D:/Dev/WG (branch claude/wip-round). Rule: AGENTS.md "Claims and checks". Logs: `D:/Dev/WG/TestRuns/<run>.log` and `<run>_r.xml`. **Every run named below was read to its end.**

## The second build: each review finding, mended or answered

How a bar decides now: each bar prints `FORCED_BAR <bar> <miner> <config> <profile>: PASS|FAIL`; the test ends with `FORCED_TEST_VERDICT` and **asserts that its list of problems is empty**. A problem is a bar that fails when not named, a bar named by `-forcedMustFail BAR[@profile],...` that passes on any of its cases on any miner, or a forced event that was not made. Unity's `asserts="0"` in the xml is how Unity counts and is no evidence either way; the evidence is the test's result below.

| # | Finding | Now |
|---|---|---|
| 1 | A bar must decide its test | **Mended.** `forced_barfails` (off, one profile, Small, nothing named): the test **Failed** (`_r.xml` result Failed, failed 1) on its bars, log:1554 "3 problems: BAR_DAMP Small off config FAIL ... BAR_A Small off swing_instant FAIL". `forced_bad2` (off, `-forcedMustFail BAR_A,BAR_B,BAR_TANH,BAR_DAMP,BAR_AXES@axes_bad`) Passed because each named bar failed on every case on every miner (FORCED_KNOWN_BAD log:9449-9841; verdict log:10121 "153 bars judged ... 0 problems"). `down_c0e` Failed because its known-bad was **not** caught (check 3 below): the machinery fails a run on a known-bad that passes. |
| 2 | Check 3: a real whirl; why NaN; a bar that separates | **Partly mended, the bar not met.** The whirl is now the engine's own: the boot's drive let go, its speed set, read back from the engine the step after with no set between, and the body made to go down in the step the engine reads 50-80 rad/s (else set again, at most 30 times). Made on all 12 whirled and whipped makes of `down_c0e` and `down_c1c`. The NaN: the step after the going down `OwnBody.Part(Foot)` is null and the old boot destroyed (GoDown puts the made body away): read in FORCED_GOING_DOWN, e.g. down_c1b "OwnBody.Part(Foot) is null True, the boot it was destroyed True". BAR_C (0.025 m) does not separate on every course: see check 3. |
| 3 | Check 4 to reach 1.5-2 E on all three; its expected value anew | **Mended.** The `tanh` profile runs with AnkleAtMost 0 (set by the rig) so no clamp cuts the ask; e/E reached 2.000 on all three (forced_good log:2771, 6378, 9985; edge 4506, 8113, 11720). The expected form and the damper are written anew in doubles from research §4.2 (ResearchTarget, ResearchDamper), not copied from OwnRiser. |
| 4 | Check 2's twist axis from the shin's own bones; a mixed profile | **Mended.** The shin's line from knee to ankle in the boot, against AnkleFolds' line: forced_barfails log:1204 "(0.038 -0.988 -0.150) ... (0.038 -0.988 -0.150): 0.00 deg apart". The boot's own turn against its shin read each step (FORCED_TWIST_AXIS): `twist_360dps` 1.5/1.3/2.1° at most from the shin's line (forced_good log:1631, 5238, 8845). New profiles `mixed_instant`, `mixed_360dps` (twist plus swing): A PASS, largest share 0.701/0.692/0.691 (log:1732, 5339, 8946); off FAIL (forced_bad2 log:1880, 1954). |
| 5 | Check 6: a 1° bar, through 180° | **Mended.** BAR_AXES at 1° on 13 holds up to 110° (each axis at 110, and (-60 60 60)): 0.68/0.59/0.65° (forced_bad2 log:2116, 4876, 7636). Past 110° the engine does not hold the boot still (`axes_far`, below), so there the bar is BAR_AXES_READ, the joint's reading against the boot's turn, on 114 holds through 178°: 0.00° (log:3587, 6347, ...). |
| 6 | Check 1: the target as a vector; A's margin; slerp against the ask's jump | **Mended.** Jumps measured as written vectors. Bar: 3.75 × the ask's turn + 1° (research's fold slope, 3.74, review_physicist.md). Margin: A's worst 0.701 of the allowance (log:1732); off's least 1.626 (swing_1800dps, forced_bad2 log:1542; 221.1° as written against an ask of 36.0°). `slerp_antipode`: BAR_A_SLERP judges the form's step against the ask's own step as written + 1°: A 0.904 (forced_good log:1819). **Off passes it too** (0.995, forced_bad2 log:2042, 4802): it cannot catch off, and is a reading. |
| 7 | Check 5: a bar, or a reading with its mechanism | **Answered: a reading.** Read back per raw segment: drive torque every 8 steps, steps at the torque limit, wraps, the drive's terminal speeds. Small B raw 250: 51.4 rad/s at the end, above limit/c (31.5), the drive's torque 3.8-4.2 N m, 0 of 64 steps at its 60.9 N m limit, 0 wraps (forced_good log:2801). The drive does not sustain that speed, and the reading stays at 78-85° while 51 rad/s is read. **The cause was not found.** |
| 8 | Switches restored in a finally | **Mended.** Load keeps every switch; SwitchesBack() runs in TearDown, in each test's finally and in a finally per miner. |
| 9 | Numbers quoted from log lines | **Mended** in this second build (the first build's text, kept below, had misquoted ranges; sceptic 4). |

Sceptic 3 answered: live_ex_small's course 0 ran to its 240 s cap ("not up after 240.0 s", live_ex_small.log:2239; the cap at log:920). The test ended by itself; that course did not. Sceptic 6: AnkleSpeed's "29624 of 29624" is so by construction whenever B is on; Long and Round's 0 of 0 are what answer the lazy way. Sceptic 7: check 8's xml shows failed=1 on forced_once_bad.

### Runs of the second build

- `forced_barfails`: off, swing_instant, Small. Failed, as it must (above).
- `forced_good`: configs A, B, AB, edge; all three miners. Passed: FORCED_DONE log:11777 "3 of 3 miners ... 108 profiles run to their end, 0 lost"; verdict log:12448 "420 bars judged ... 0 problems". Switches read back from the running OwnRiser: log:1037 "AnkleLimit 1, AnkleSpeed 0 ...", 1863 "AnkleLimit 0, AnkleSpeed 20 ...", 3016 (AB), 4245 "AnkleSpeed 31.93069" (edge), and the same for Long (4644-7852) and Round (8251-11459). Code changed after this run only in the off-only axes profiles and in check 3: the A, B, AB and edge paths are as run.
- `forced_bad2`: off, `-forcedAxesBad 1`, the five named bars. Passed: log:9422 "45 profiles ... 0 lost"; log:10121 "0 problems"; off read back log:1223, 3983, 6743.
- `down_c0e` (C off, known-bad) and `down_c1c` (C on, known-good): on the committed code. Earlier courses of check 3: down_c0c, down_c1, down_c1b, down_c0d (below).

Each profile ended at its own step count (FORCED_DONE "0 lost"); each check-3 run read 12 makes, 0 missed, apart from down_c0d.

## Check 1: the boot turned through 180° (A and B)

Forced: swing asks at 0/60/179/181° instantly; swing ramps at 90, 360 and 1800°/s; an oblique ramp; the Slerp to the antipode; now also `mixed`.
Bars and their lazy ways:
- BAR_B: the fastest per-axis speed over every step ≤ Ω = AnkleSpeed·sqrt(H/tall). Lazy: a profile that never asks fast; off reaches it, forced_barfails log:1315 "95.1 rad/s" with the implicit drive's prediction -97.6 beside it (the answer to the physicist's 95-103 against 60-81: the engine does what the implicit step predicts for the free boot). B: Small 18.9 (19.5 as a vector) of 20.00, Long 15.5 of 16.18, Round 16.6 of 18.64 on swing_1800dps (forced_good log:2182, 5789, 9396).
- BAR_A: no step where the form before B jumps more than 3.75 × the ask's turn + 1°, measured as written vectors. Lazy: a form that never crosses; off fails it (above).
- BAR_FOLLOW: the boot within 5° of the written target at the profile's end. Lazy: a target met by not moving.
- BAR_FORM: the written target within 0.05° of the switch's form, every step. Lazy: a stale target.
Known-bad: forced_bad2 (off). Known-good: forced_good. Rung: tested on a rig, read to the end.

## Check 2: a pure twist through 180° (A)

As check 1, with the twist and mixed profiles and FORCED_TWIST_AXIS (above). Follow is judged against the written target: A's designed twist limit leaves the ask short. Rung: tested on a rig, read to the end.

## Check 3: a going down while the boot whirls (C)

Forced (`TheWhirlingGoingDown`, all three miners, Small's ways given to each, `-ownAsPlayed 1 -ownRate 50`, the flat test floor): each miner pushed down, then made to go down while getting up: plainly; boot whirled; whipped (the knee at 25 and the hip at 15 rad/s on the first set only). The whirl waits until the boot and its toe touched nothing for 2 steps. Read back: FORCED_WHIRL with the sets, the engine's reads, the joint's type, locks, limits and maxJointVelocity (SphericalJoint, FreeMotion ×3, maxJointVelocity 100, friction 0, damping 0).
Bar: BAR_C, the let-go body's ankle of the whirled boot, before any refit, within 0.025 m of where the standing body had it; BAR_C_PLAIN the same on the plain make. Lazy way: a going down that never whirls; answered by the engine's own read at the going down. With C on the refit then sets every ankle to 0 m by construction; what shows C acted is the refit moving it by the let-go offset, in FORCED_MAKE.

Courses (each run is one course per miner and make; the whirl's sets adapt, so courses differ):

| Run | C | plain (S/L/R m) | whirled | whipped | test |
|---|---|---|---|---|---|
| down_c0c | off | 0.0177/0.0030/0.0004 PASS | 0.0360/0.0806/0.0763 FAIL | 0.0608/0.6490/0.1119 FAIL | Passed (known-bad caught) |
| down_c0e | off | 0.0208/0.0049/0.0022 PASS (log:1285, 1874, 2523) | 0.0414 FAIL / **0.0141, 0.0148 PASS** (log:1432, 2033, 2670) | 0.0735/0.0799/0.0551 FAIL (log:1591, 2240, 2817) | **Failed**: known-bad not caught on Long and Round whirled (log:3064) |
| down_c1 | on | 0.0003/0.0000/0.0000 | 0.0000/0.0236/0.0013 | 0.0158/0.0000/**0.0398 FAIL** (log:2795) | Failed; also Long whirled not made in 10 sets (log:1967) |
| down_c1b | on | PASS | 0.0001/0.0032/0.0000 | 0.0000/0.0006/0.0001 | Passed (log:2999) |
| down_c1c | on | 0.0001/0.0001/0.0000 (log:1105, 1682, 2271) | 0.0000/0.0002/0.0109 (log:1252, 1841, 2466) | 0.0019/0.0002/0.0039 (log:1399, 1988, 2649) | Passed (log:2868) |

C read back: FORCED_SET "OwnBody.BootsFromOwnShin = True" (down_c1c log:904); off "nothing set" with FORCED_RIG "BootsFromOwnShin False" (down_c0e log:1084 and the RIG line after). Engine reads at the going down, down_c0e: 62.7, 65.0, 70.4, 68.0, 57.2, 58.6 rad/s (log:1365, 1524, 1966, 2173, 2603, 2750); down_c1c: 56.1, 57.2, 77.3, 64.8, 51.4, 77.0 (log:1185, 1332, 1774, 1921, 2399, 2582). Every refit with C on left 0.0000 m after (FORCED_MAKE lines).

Reading: with C off the whipped make fails the bar on every course run (6 of 6 in c0c and c0e); the whirled-only make fails on 4 of 6, and C off's plain makes reach 0.0258 m (down_c0d Long, log:1729). With C on the let-go offset is mostly under 1 cm, once 0.0398 m (down_c1 Round whipped, its reads 44.3, 121.0, 72.5, log:2728). So no 0.025 m bar separates off from on on every course of the whirled make. Not changed to pass. The difference C makes is in the let-go body's pivot (the shin's end) and in the refit; which part of the offset is the whirl's and which is the going down's own is not shown.

Rig mends in this build, each from a run read: sets above the joint's maxJointVelocity of 100 were read back at 7-41 (down_c0b), so the sets are capped at 95 and flipped when the engine kept less than half; ten sets missed the band once (down_c1 log:1967), so 30 are allowed; down_c0d set the whip every step and blew up (Long whipped read up to 1e12 rad/s, log:2571; then "Round did not come to stand by its own joints"), so the whip is set once, and a read above twice maxJointVelocity goes down at once.
Rung: tested on a rig, read to the end; **its bar failed its known-bad on one course**.

## Check 4: B near its edge

Forced: AnkleSpeed so that Ω equals forceLimit/(c + k·dt): Small 31.93, Long 44.52, Round 61.89 rad/s (read back log:4245, 7852 "AnkleSpeed 55.04117", 11459 "66.42154"; Ω after the height).
Bars:
- BAR_DAMP: the damping held equals research's smooth maximum (c⁴ + floor⁴)^¼ within 1e-4 relative. Lazy: the plain maximum. B: Small 1.9291 (plain 0.7% apart), Long 6.8560 (0.03%), Round 6.8043 (0.012%); edge 0.9265/1.5418/1.2115 (plain 15.9% apart) (forced_good log:1890, 5497, 9104, 4272, 7879, 11486).
- BAR_TANH: on 13 asks of 0.25-2 E, the written target within 0.05° of reading + E·tanh(|e|/E)·ê; e reaches E within 0.03. Lazy: a hard clamp at E (at least 2.226/3.104/4.315° away), or no slowing (at least 0.314/0.438/0.610° away). Result 0.0000°, e/E to 2.000 (log:2771, 6378, 9985).
Known-bad: off fails BAR_TANH and BAR_DAMP (forced_bad2 log:9617-9757). Rung: tested on a rig, read to the end.

## Check 5: a target outside [-pi, pi]: a reading

As in the first build, with the mechanism read back (finding 7). BAR_WRAP (B never writes beyond ±180 from an inside reading): 0 violations; the raw run, the known-bad, counts 235/161/126 (forced_good log:3002, 6609, 10216). Each raw segment is now followed by 45 steps asked as stood, which bring the joint back to its start at 0.00 rad/s (log:2814). Targets of 150° settle; 190 and 210 do not; 250 spins on at 51 rad/s with B. Rung: tested on a rig, read to the end; no bar on the engine.

## Check 6: the joint-axis mapping

BAR_AXES 1° (finding 5). Known-bad: `-forcedAxesBad 1` writes each target wrongly after the riser: FAIL at about 160° (forced_bad2 log:2175, 4935, 7695). Lazy way beside it: X and Y swapped is at least 41.28° away (log:2116); Euler angles up to 41.68° away.
`axes_far`, a reading: of 57 holds past 110°, every miner settled every target up to 130°; the smallest still turning at its hold's end is 135° (FORCED_AXES_FAR_SAID log:3602, 6362, 9122). Today's written targets reach about 110° from stood plus |toStood| (Small 22.3°, forced_barfails log:1204), near that edge.
Rung: tested on a rig, read to the end.

## Checks 7 and 8

Code unchanged; not run again. As in the first build below.

Commit: 7c6edf0 on claude/wip-round (pushed); down_c0e and down_c1c ran on it.

## Not tested (second build)

- The meadow, each miner's own ways, the hips moving, the right boot forced, the toes forced, contact while forced: no rig result here is a claim about the game as played.
- Check 3: more than one course per run of each make. A bar that separates the whirl from the going down's own offset was not found.
- Check 5's cause.
- forced_good was not run again after the off-only axes edit and the check-3 edits; the paths it runs are as run.
- Nothing seen by the Game Director; nothing accepted.

---

# The first build (kept as it was; superseded where the table above says)

What changed and why, in short: the bars only logged and the test passed with a bar failing (finding 1, sceptic 2); check 3's 65 rad/s was the value read straight after the set, not the engine's (finding 2); check 4's shares were cut by the 170 clamp and AnkleAtMost (finding 3); check 6's bar was 2° in the code (finding 5); check 1 measured jumps as turns, so a written flip read as 140° (finding 6); the ranges below were misquoted in places (sceptic 4).

| Check | Rung reached (first build) |
|---|---|
| 1 boot through 180° | tested on a rig, read to the end; bars met (A, B, AB), off fails as it must |
| 2 pure twist | tested on a rig, read to the end; A's bar met |
| 3 whirling going down (C) | tested on a rig, read to the end; C on meets the bar, off fails as it must |
| 4 B near its edge | tested on a rig, read to the end; bar met |
| 5 target outside [-pi, pi] | tested on a rig, read to the end; engine behaviour recorded (a risk, not a B fault) |
| 6 joint-axis mapping | tested on a rig, read to the end; bar met |
| 7 the live test's exercised lines | tested as played (2 courses each, three miners), read to the end; bar met |
| 8 once-said messages in two tests | tested (two tests, one run), read to the end; bar met |

The code: `AnkleForced.cs` (checks 1-6 and 8), `AnkleMend.cs` (AnkleWatch's `Exercised()`), `CoverRingLiveTests.cs`. Commit 6b84d78.

## The rig (checks 1, 2, 4, 5, 6: run `forced_boot2`; check 3: `forced_down_*`)

- Every miner is given Small's ways up (`Resources/RiseWays/Small`): a rig, not the game.
- Each miner is pushed, made down, and its riser's clock held (`got` kept at 0, StuckAfter 0) so the ways do not move on.
- Hips made immovable and lifted 1.5 m (TeleportRoot): the left boot hangs free. Its colliders ignore every other collider of the body (14 others); touches counted per step: 0 on all three miners.
- The left boot's ask is re-written through the riser's private `Aims` after everything else in the step (ForcedStep, execution order 30003; AnkleWatch reads at 30002).
- Run as `-ownAsPlayed 1 -ownRate 50`, on the flat test floor.
- Left out: the meadow, the miners' own ways, the hips moving, the right boot, the toes' own forcing.

`forced_boot2`: all three miners, 120 profiles, 0 lost, every profile ended by itself; switches read back per config in FORCED_CONFIG lines.

## Check 1 (first build)

- Off: fastest Small 95.0-103.0, Long 93.9-101.8, Round 94.0-102.9 rad/s (misquoted: the log has lower values on some profiles, sceptic 4); the target steps 140° at the crossing (as a turn; 221° as written): BAR_A FAIL.
- A alone: BAR_A PASS; the form steps at most 58.7° (67.4° at 1800°/s). A does not bound speed (23.2-72.7 rad/s; slerp 71.6-72.7).
- A and the Slerp: the ask jumps 179.8° in one step and A's target 163.3°; A cannot remove a jump in the ask.
- B: Small 19.5 [20.00], Long 15.9 [16.18], Round 17.3 [18.64]: PASS. Damping Small 0.7791 → 1.9291, Long 1.2965 → 6.8560, Round 1.0187 → 6.8043.
- A and B: both PASS.

## Check 2 (first build)

`twist_instant`, `twist_360dps`: A PASS; follow against the ask 5.4-6.2° short (A's designed twist limit).

## Check 4 (first build)

Damping held 2^¼·c_today at the edge; speeds under their bounds; the tanh form 0.0000° (a re-derivation of AnkleSlows, physicist 2).

## Check 5 (first build)

A whirl at 30 rad/s, then raw targets ±250, 150, 190, 210. After a whirl the joint's reading passes 180° (up to 292.6/312-314/329.6°); B writes beyond ±180 only from such a reading, then brings the joint back. Raw 190 and 210 do not settle. Recorded as a risk of the engine, not a fault of B.

## Check 6 (first build)

9 targets × 35 steps: within 0.70/0.62/0.66° (the code's bar was 2°, not the 1° written).

## Check 3 (first build)

The 65 rad/s was read straight after the set; before the set the engine had damped it to 0.9-10.3 rad/s. C off (`forced_down_c0b`) failed a 0.01 m bar on plain makes already (0.0117, 0.0162); C on (`forced_down_c1`) read 0.0000 m everywhere, by the refit's construction.

## Check 7: the live test's exercised lines (unchanged)

`AnkleWatch.Exercised()` per switch that is on: AnkleLimit (boot-steps getting up whose written target is unlike today's form and unlike it slowed as B slows it), AnkleSpeed (whose damping is unlike today's), BootsFromOwnShin (boots slackened about the let-go shin's end at a going down by itself). Runs with all three on, `-ownPushes 2`:
- `live_ex_small`: EXERCISED AnkleLimit 24929 of 29624 boot-steps; AnkleSpeed 29624 of 29624; BootsFromOwnShin 27 downs by itself, 54 boots slackened about the let-go shin's end.
- `live_ex_round`, `live_ex_long`: the old way 2 of 2, 0 own tries; NOT EXERCISED for all three.
LIVE_SWITCH_OFF was not run.

## Check 8: a once-said message in two tests (unchanged)

`forced_once`: 2/2 passed; `forced_once_bad` (`-onceSaidKeep 1`): the second test FAILED ("Expected log did not appear"): caught.

## Not tested (first build)

- The meadow; each miner's own ways; the hips moving, the right boot, the toes, contact while forced.
- AnkleSpeed values other than 20 and the edge; any `Unheard` false path (every run read back `RiseAgent.Unheard True`).
- Check 7: LIVE_SWITCH_OFF; one switch at a time; more than 2 courses; Long and Round never get up by their own body as played, so A, B and C are exercised live on Small only.
- Nothing seen by the Game Director; nothing accepted.
