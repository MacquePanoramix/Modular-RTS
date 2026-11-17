Switches each check needs on: 1 A and B (and all off, as its known-bad); 2 A; 3 C (C off as its known-bad); 4 B; 5 B (the engine's answer read with every switch off too); 6 none (today's path); 7 whichever is on (all three on here); 8 none (AnkleLimit 2, a wrong value).

# Step 4a: the forced checks (November 17, 2026)

Builder: Opus 5.5, in the worktree D:/Dev/WG (branch claude/wip-round, from e8bac96). Rule: AGENTS.md "Claims and checks".
No fault was found in A, B or C; nothing on their paths was mended, so arm B (ank1_small) needs no new run for this step.
The code: `Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs` (checks 1-6 and 8, new), `AnkleMend.cs` (AnkleWatch's `Exercised()`), `CoverRingLiveTests.cs` (prints it). Commit: 6b84d78 on claude/wip-round (pushed); all the runs below were made on that code (the check-3 whirl change, its last edit, was in place before `forced_down_c0b`).

**State: every run below was read to its end.** Logs in `D:/Dev/WG/TestRuns/<run>.log`.

| Check | Rung reached |
|---|---|
| 1 boot through 180° | tested on a rig, read to the end; bars met (A, B, AB), off fails as it must |
| 2 pure twist | tested on a rig, read to the end; A's bar met |
| 3 whirling going down (C) | tested on a rig, read to the end; C on meets the bar, off fails as it must |
| 4 B near its edge | tested on a rig, read to the end; bar met |
| 5 target outside [-pi, pi] | tested on a rig, read to the end; engine behaviour recorded (a risk, not a B fault) |
| 6 joint-axis mapping | tested on a rig, read to the end; bar met |
| 7 the live test's exercised lines | tested as played (2 courses each, three miners), read to the end; bar met |
| 8 once-said messages in two tests | tested (two tests, one run), read to the end; bar met |

## The rig (checks 1, 2, 4, 5, 6: run `forced_boot2`; check 3: `forced_down_*`)

- Every miner is given Small's ways up (`Resources/RiseWays/Small`): a rig, not the game.
- Each miner is pushed, made down, and its riser's clock held (`got` kept at 0, StuckAfter 0) so the ways do not move on.
- Hips made immovable and lifted 1.5 m (TeleportRoot): the left boot hangs free. Its colliders are set to ignore every other collider of the body (14 others: the unit's, every OwnBody part's, every PhysicalFall part's); touches counted per step: 0 on all three miners.
- The left boot's ask is re-written through the riser's private `Aims` after everything else in the step (ForcedStep, execution order 30003; AnkleWatch reads at 30002).
- Run as `-ownAsPlayed 1 -ownRate 50`, on the flat test floor (not the meadow; the boot touches nothing anyway).
- Left out: the meadow, the miners' own ways, the hips moving, the right boot (not forced), the toes' own forcing.

`forced_boot2`: all three miners, 120 profiles, 0 lost, every profile ended by itself at its own step count (FORCED_DONE "3 of 3 miners ... 0 lost"), exit 0. Switches read back per config in FORCED_CONFIG lines (`AnkleMendArguments.ReadBack()`, from the running OwnRiser). Steps per profile in `D:/Dev/WG/TestRuns/Forced/boot2`.

On every step of every profile, the target written on the boot's drive was read back and compared with what the switch's form says it should be (A: AnkleFolds of the ask; B: the tanh-slowed form from the same reading and E): within 0.0000° everywhere. Lazy way caught by this: a constant or stale target.

## Check 1: the boot turned through 180° (A and B)

Forced: swing asks at 0/60/179/181° instantly; swing ramps at 90, 360 and 1800°/s through the crossing; an oblique ramp at 360°/s; the Slerp to the antipode (`slerp_antipode`).

Bars, with their lazy ways:
- BAR_B: the fastest per-axis speed of the boot over every step at most B's bound Ω = AnkleSpeed·sqrt(H/tall). Lazy way: a profile that never asks fast; caught by off reaching 95-103 rad/s on the same profiles.
- BAR_A: no step where the form before B jumps more than 3.7·(the ask's turn that step) + 1°. Lazy way: a form that never crosses; caught by off's 140° step at the crossing.
- Follow: the boot within 5° of the written target at step 34. Lazy way: a target that is met by not moving.

Results (`forced_boot2`, read to the end):
- **Off (the known-bad):** fastest Small 95.0-103.0, Long 93.9-101.8, Round 94.0-102.9 rad/s; the brief expected about 60, the rig shows more (the free boot, hips pinned). The target steps 140° at the crossing: BAR_A FAIL, as it must. Follow 0.4-0.6°.
- **A alone:** BAR_A PASS on every swing, oblique and twist profile; the form steps at most 58.7° (67.4° at 1800°/s, inside its bar). A does not bound speed: 25-43 rad/s.
- **A and the Slerp:** the ask itself jumps 179.8° in one step and A's target 163.3°. A cannot remove a jump that is in the ask; its bar is met there only trivially (the bar scales with the ask's turn). Stated, not a fault.
- **B:** Small at most 19.5 [bound 20.00], Long 15.9 [16.18], Round 17.3 [18.64]: PASS, the Slerp bounded too. B's damping read back from the drive: Small 0.7791 → 1.9291, Long 1.2965 → 6.8560, Round 1.0187 → 6.8043, all "held as written".
- **A and B:** both bars PASS; Small at most 18.9, Long 15.1, Round 15.6.

## Check 2: a pure twist through 180° (A)

Forced: `twist_instant` and `twist_360dps`. Bar: BAR_A as above (known-bad: off, FAIL at the crossing). Result: A PASS on all three. Follow against the ask is 5.4-6.3° short: A's designed twist limit, not a fault; follow is therefore judged against the written target (within 5°, PASS).

## Check 4: B near its edge

Forced: AnkleSpeed set so that Ω equals the drive's own edge, forceLimit/(c + k·dt): Small 31.93, Long 44.52, Round 61.89 rad/s.
Bars: the damping held must be the smooth maximum (c⁴ + floor⁴)^¼; at the edge floor = c, so it must read 2^¼·c. Lazy way: a plain maximum (reads c); caught: it would differ by 19%. The `tanh` profiles (shares ±0.25…2 of E): the written target must be reading + E·tanh(|e|/E)·ê. Lazy way: a hard clamp at E; caught: it would be at least 3.289° away.
Results: damping held Small 0.9265, Long 1.5418, Round 1.2115 = 2^¼·c_today each; speeds Small 28.4, Long 39.0, Round 49.3, each under its bound; the tanh form exact (0.0000°). PASS.

## Check 5: a target outside [-pi, pi]

Forced (`beyond`, configs off, B, AB; 460 steps): a whirl at 30 rad/s with the drive zeroed (steps 10-50); the drive restored and asked (170 0 0); raw targets ±250, then 150, 190 and 210, 65 steps each. Read back from the running joint only (no engine source quoted).
- After a whirl the joint's reading itself passes 180°: up to 292.6° (Small), 312-314° (Long), 329.6° (Round), with 1-2 wraps.
- From such a reading B (and AB) writes targets outside ±180 for 1-3 steps, up to 230.2°; the joint then comes back to the asked pose (B to about 110-111.8°, its 110 clamp; AB near stood).
- Raw ±250: the joint ends at unrelated poses. Raw 150: settles with B (146-150°); off is still at about 31-33 rad/s at its end. Raw 190 and 210 from an inside reading: the joint does not settle (off 50-67 rad/s at the end, B 11-35) and ends at unrelated poses.

Reading: the engine does not wrap a target beyond ±180; a drive asked there is not held. B never writes beyond ±180 from an inside reading: its target lies on the segment between the reading and the pre-B form, both inside the 180° ball. It does so only after the reading is already outside (a whirl), and then it brings the joint back. Recorded as a risk of the engine, not a fault of B.

## Check 6: the joint-axis mapping

Forced (`axes`, off only): 9 targets × 35 steps. Bar: the boot within 1° of the written target read as a turn, and the joint's reading equal to the boot's turn. Known-bad hypotheses that must fail: Euler angles (fail by 34.89/21.19/22.71°), X and Y swapped (fail by at least 7.49/40.87/3.28°). Result: within 0.70/0.62/0.66° (Small/Long/Round): PASS.

## Check 3: a going down while the boot whirls (C)

Forced (`TheWhirlingGoingDown`, all three miners, Small's ways given to each, `-ownAsPlayed 1 -ownRate 50`, the flat test floor): each miner is pushed down, then made to go down (OwnBody's private GoDown) three times while getting up: plainly; with its left boot whirled (its joint's speed set to 65 rad/s on its first axis on each of six steps, the last the step of the going down); whipped (the same, its knee at 25 rad/s and its hip at 15). At each make after, each boot's ankle anchor against the one the standing body was made with.

Bar (BAR_C): the whirled boot's ankle within 0.01 m of the standing one at the make. Lazy way: a going down that never whirls; answered by reading the boot's speed back from the joint at the going down (65.0 rad/s on its axis in every whirled and whipped case) and by C off failing. Known-bad: C off. Known-good: C on.

- `forced_down_c0b` (C off; `OwnBody.BootsFromOwnShin False` read back; 12 makes read, 0 missed; ended by itself; 1/1 passed, exit 0): plain makes 0.0016-0.0191 m (Small 0.0117 and Round 0.0162 already above 1 cm); whirled: Small 0.0268, Long 0.0412, Round 0.0619 m, BAR_C FAIL; whipped: Small 0.1450, Long 0.0495, Round 0.0603 m, BAR_C FAIL. Caught.
- `forced_down_c1` (C on; `OwnBody.BootsFromOwnShin True` read back; 12 makes read, 0 missed; ended by itself; 1/1 passed, exit 0): every make, both boots, 0.0000 m from the standing ankle: BAR_C PASS on all six whirled and whipped makes. The refit itself moved the boot 0.0000 m in most makes (the let-go body's own side of C had kept it at its shin's end), 0.0001 m on Small whipped and 0.0242 m on Long whipped: both halves of C were exercised.
- An earlier run, `forced_down_c0` (C off), set the whirl only on the five steps before the going down: the drive had damped it to 0.8-9.4 rad/s by then. Its numbers are like c0b's, but it did not force the whirl at the going down, and so is not counted.

What the forcing is and is not: the 65 rad/s is read back after the set, in the step of the going down; read before the set, the engine had already damped the previous step's 65 to 0.9-10.3 rad/s (25.4-55.7 on whipped Long). GoDown hands the let-go body the speeds of the parts above the boots only (OwnBody.cs, the loop over `i < Foot`); the boot's own speed is not handed on (the step after reads NaN: the boot is put away). What C meets is the boot's turned pose and the whipped shin's speed. The bar is near to C's construction (C places the boot from its shin's end); what it shows is that the path runs on the forced event and that off does not.

## Check 7: the live test's exercised lines

Built: `AnkleWatch.Exercised()` (AnkleMend.cs), printed by CoverRingLiveTests before its asserts and appended to `ankle.txt`; a LIVE_NOT_EXERCISED line is also a warning (so each shows twice in the log). Per switch that is on:
- AnkleLimit: boot-steps getting up whose written target is unlike today's form of the ask and unlike that form slowed as B slows it.
- AnkleSpeed: boot-steps getting up whose damping is unlike today's.
- BootsFromOwnShin: boots slackened about the let-go shin's end at a going down by itself (with the downs, the "own ankle" and "neither" counts, and the downs by a push, which do not count).
A switch that is off prints LIVE_SWITCH_OFF.

Bar: EXERCISED only where the switch's own path changed what was written; NOT EXERCISED where it did not. Lazy way: a line that says EXERCISED whenever the switch is on; answered by Long and Round below. Runs: all three switches on (`-riseSet AnkleLimit=1,AnkleSpeed=20 -ownBodySet BootsFromOwnShin=1`), the live arguments as played with `-ownPushes 2` (Long also `-liveRing 0`); each 1/1 passed, exit 0, ended by itself after its 2 courses; the switches read back in LIVE_SWITCHES: "OwnRiser.AnkleLimit 1, AnkleSpeed 20, AnkleSpeedHeight 0.605714 ... OwnBody.BootsFromOwnShin True".
- `live_ex_small` (the known-good): LIVE_DONE 2 courses, up by its own body 0, the old way 1, not up 1, 28 own tries. EXERCISED AnkleLimit 24929 of 29624 boot-steps (2870 as today's form itself); EXERCISED AnkleSpeed 29624 of 29624 (all as B's smooth maximum); EXERCISED BootsFromOwnShin: 27 downs by itself, 54 boots slackened about the let-go shin's end, 0 about their own ankle, 0 neither; 3 let go by a push.
- `live_ex_round` (the known-bad for C, as the brief asks): the old way 2 of 2, 0 own tries; NOT EXERCISED for all three: 0 of 0 boot-steps; 0 downs by itself (2 by a push). PASS: it says NOT EXERCISED for C on Round.
- `live_ex_long`: the old way 2 of 2, 0 own tries; NOT EXERCISED for all three, as Round.

Two courses each: this check is about the lines, not about how well any miner gets up (Small's "not up 1" is not judged here). LIVE_SWITCH_OFF was not run.

## Check 8: a once-said message heard in two tests of one run

Forced: two ordered tests each expect OwnRiser's once-said error for AnkleLimit 2 on an uninitialised OwnRiser (AnkleMended). Bar: both hear it. Known-bad: `-onceSaidKeep 1` (the flag not forgotten between tests).
- `forced_once`: 2/2 passed, exit 0; ONCE_SAID heard in both.
- `forced_once_bad`: the second test FAILED ("Expected log did not appear"), exit 2: caught.
Lazy way: a test that passes because nothing expects the message; caught by the bad run failing.

## Not tested

- The meadow's uneven ground: the rig hangs the boot free on the flat floor; no check here is a claim about the game as played.
- Each miner's own ways up: Small's were given to all three.
- The hips moving, the right boot, the toes, contact while forced.
- AnkleSpeed values other than 20 and the edge; any `Unheard` false path (every run read back `RiseAgent.Unheard True`).
- Check 3: the boot's own whirl is not handed to the let-go body by GoDown, so a whirling boot's speed reaching the let-go body was not forced (it cannot be, on today's path).
- Check 7: LIVE_SWITCH_OFF lines; one switch on at a time; more than 2 courses; Long and Round never get up by their own body as played, so A, B and C are exercised live on Small only.
- Each forced event is one course per miner (repeats would be the same course).
- Nothing seen by the Game Director; nothing accepted.
