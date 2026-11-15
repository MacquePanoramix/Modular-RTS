# Critique of research.md (fresh sonnet critic, November 15, 2026)

Read only. Code read in `D:/Dev/WG` (`claude/wip-round`), no Unity run. Web checks were generic queries.

1. **Code readings of the spring, damper and clamp: agree.** `OwnRiser.cs:359` (k = min(has/(35 deg), 10*Rule^2/dt^2*lighter)),
   `:371` (c = 0.7*2*sqrt(k*lighter)), `:132` (AnkleAtMost 110, AnkleTurnsAtMost 0), `OwnKeeper.cs:41` (Rule .64038). The
   arithmetic (k 66.6, omega_n*dt about 2.0, c 0.92, k capped at 99.6 when I is 0.65, c 11.3) checks out.
2. **Clamp flip arithmetic (sec 1.0): agree.** `OwnRiser.cs:416-417`: far is shifted by -360 above 180, and Sign(far) picks
   the side, so +110 to -110 about one axis, 140 deg the short way. tanh figure (102 deg) correct.
3. **drivenNow (sec 6): agree.** Added after `AnkleCan` (`:397` then `:403`). `RiseAgent.cs:171` returns early for a
   non-Paced agent outside `LearnsAt` (`OwnRiser.cs:1126`), and only the Paced path clears `Driven` (`RiseAgent.cs:250`),
   so a stale Driven persists. `Heard` (`:120-124`) does load models when automatic stepping is on. Note the load order
   (`:155-159`): RisePaced, then RiseFull, then Rise; untracked `SmallRise.onnx` etc. in the working copy could change which
   model "as played" hears. The research should name which file loads in the test build.
4. **Sec 4.1 fault (boot taken off its shin at GoDown): agree on the code.** `OwnBody.cs:1322` takes `ankleAt` from the
   physics boot; `PhysicalFall.cs:515` builds `footAt` from the solved `s.feet`; `:907-909` rotates `footAt` about
   `about` made from that mismatched point. `OwnBody.cs:748` fixes `anchors[Foot]` at make time, so `Lighter` depends only
   on the made boot offset. The run evidence (0.55-0.71 m) was not rechecked here.
5. **Refute: "the drive alone can never turn the boot faster than 20 rad/s" (sec 4.2, Start-here 3).** The torque is
   clamp(k*e - c*w, +-tau_cap) (the research's own joint_model.md line 282). The damper does not brake while the sum
   exceeds the cap. For the 140 deg flip, e = 2.44 rad, k*e = 163 N m, so the torque stays at 60.86 until
   w = (163-60.86)/c. With c = 3.0 that is 34 rad/s, and the unsaturated settling speed k*e/c is 54 rad/s. The tau/c bound
   holds only when e <= e_full (= tau_cap/k, about 52 deg here, not 110). Switch B alone therefore does not bound the
   whips that the flips cause; it bounds them only with A (smaller jumps) or a target-rate limit. To bound w by Omega for
   a jump J the floor must be c >= k*J/Omega (about 8 for J = 2.44), which is overdamped far past "own". The proposal and
   the test order ("B alone first, invisible-ish") should be revised; expect B alone to fail to remove the whips.
   Also, `AnkleTurnsAtMost` exists for this and was found harmful (comment `OwnRiser.cs:418-419`); the research does not
   say why a different rate bound would not be.
6. **Refute: "continuous on all of SO(3)" for the fold (sec 1.4).** The fold depends on swing only. A pure twist of about
   180 deg about the shin axis has swing 0 (fold = 1) and twist tau = 2*atan2(v.t, w), which jumps from +180 to -180 with
   q to -q (or across w = 0). softclamp(tau) then jumps from tau_hi to tau_lo (about 30 deg for +-15). A much smaller cut
   than 140 deg, and rare, but the claim of no cut is false; the twist needs its own fold as tau nears 180 (or the sign
   fixed with w >= 0 and the twist folded as |tau| nears 180).
7. **Agree in part: B + C + fold is the best fit to "no switch at a threshold".** Option A (side by continuity) fails
   the principle as argued. The fold itself is smooth (cos^2 ramp), though it is non-monotone: an ask of 150 deg gives a
   smaller target than 120 deg, and the target can move up to 4.3x the ask's speed in the ramp (research's own bound), so
   the fold can itself make a fast target change if the ask moves fast near 140-180 deg. That interacts with point 5.
8. **Agree in part: map to the engine's drive axes (sec 2.2, 3).** The controller clamps a rotation vector (`Aims`,
   `:398-407`) while the engine's spherical drive reads three joint angles (a decomposition, not the rotation vector, for
   large combined turns), and the ellipse is in boot axes while the drive axes are the joint anchor's. The research marks
   the axis signs "to read back" but does not mark that a 110 deg diagonal rotation vector is not the same as a 110 deg
   joint-angle triple. Read this back at build before any elliptical range is trusted.
9. **Natural ranges: agree in part.** Soucie 2011 hip and knee flexion verified (hip 133.8 F / 130.4 M, knee 141.9 / 137.7, 20-44 y,
   CDC tables). Milgrom 1985 verified (272 males 18-20 y, inversion 32 falling to 22 dorsiflexed, eversion little
   affected). Not verified here: the ankle plantarflexion 62.1/54.6, Hemmerich, Comfort, Ball & Johnson numbers (not
   found in my searches). Caveat: Milgrom measured subtalar motion only, so it is not the whole foot's inversion and
   using it as the boot's swing cone semi-axis is an approximation; "eversion 15-20" is derived from Ball & Johnson totals, not
   a measured eversion, and should be flagged as such. Mean-minus-SD for a soft limit is unstated.
10. **Interaction (a new risk): fault C may remove a protection.** Three flips were harmless with the huge damper
    (research sec 4.1). If C restores a normal `lighter`, those flips lose that damper. So C alone could raise blow-ups;
    run it only after A and a correct speed bound (point 5), or measure the three-miner effect expecting a worse result.
11. **K1, no switch at a threshold, one body, any body, idle: agree in part.** Stops and dampers in the controller, not
    guards: yes (A, B live in `OwnRiser`; no `OwnKeeper` change). But a floor `max(today, tau/omega_max)` is a max()
    switch of sorts (a kink), and the fold start s_f = 140 and soft knee rho0 = 0.7 are thresholds, though smooth. One
    body: yes if the table is the same for all and scaled only by the beta = 0.5 exponent (Open). Idle standing: the research
    flags the hand-over gains as unread. `OwnRiser.Springs/Drive` (`:353-376`) set drives on the shared parts, and
    nothing read shows who resets them for the keeper; this must be proven tick-by-tick as the plan says.
12. **Sec 7.3 power table: not rechecked numerically.** Plausible for Fisher exact at 37.5% baseline; the 24-per-arm
    claim is the usual figure. Fine.

## Verdict
Sound to build behind a switch, off by default, with two corrections: (a) the damper floor does not by itself bound the
drive speed when the ask error exceeds e_full, so B must not be expected to work alone (point 5); (b) the fold is not
continuous at a 180 deg pure twist (point 6). C needs to come after A/B (point 10).
