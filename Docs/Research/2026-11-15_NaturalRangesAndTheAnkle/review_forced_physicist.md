# Physicist's review of the forced ankle checks (Nov 17, 2026)

Shown: forced_checks.md; AnkleForced.cs in full; OwnRiser.cs Aims/AnkleMended/AnkleFolds/AnkleSlows/AnkleDamper; research 4.2 and 7.2; grep of forced_boot2.log, forced_down_c0b.log, forced_down_c1.log. Not read: checks 7 and 8 in depth, the steps.csv files, OwnBody.GoDown, the other logs, the XMLs. No Unity run.

## Findings, most serious first

1. Check 3 (AnkleForced.cs:713, 741). The whirl is a value written, not an event. "65.0 rad/s read back" is the jointVelocity buffer read straight after it was set. The engine had 0.7-9.1 rad/s before the set, and the boot reads NaN the step after. GoDown does not hand the boot's speed on, so nothing whirls through the going down. What differs between C off and C on is a turned boot pose and a whipped shin. Also, the unwhirled right boot is off 0.033 m (Long whirled), 0.0756 m (Long whipped) and 0.104 m (Small whipped) with C off. Plain makes of the left boot are 0.0117 m (Small) and 0.0162 m (Round), already above the 0.01 m bar. So the displacement is not specific to the whirl. One course per miner. C on gives 0.0000 m everywhere, even for plain and unwhirled boots, so the bar is met by construction (the anchor is rewritten to stoodAnkle). It is not evidence about a whirl. Sure, 85%.

2. Check 4 never reaches the shares it claims (:520, :534). The 1.5E and 2E asks are cut by the +-170 clamp and by AnkleAtMost 110. The e/E reached is Small 1.78-1.80, Long 1.27-1.28, Round 0.92 at most. Round never reaches e >= E, so its hard-clamp catch is vacuous (clampDiffLeast = Infinity, :338). The negative shares repeat the positive values. The tanh "expected" is a copy of AnkleSlows (:163 against OwnRiser.cs:535), so 0.0000 deg is re-derivation. Sure, 90%.

3. BAR_A margin and scale (:316). The allowance is 3.7*askTurn+1. At 1800 deg/s the off form steps 140 deg against an allowance of 134.2, so the known-bad is caught by 4%. The bar is also measured as a quaternion angle (FromRV, :310). A flip of the written vector, such as 220 deg as written, reads as 140. A pure vector wrap would read as nearly 0. jumpWrittenMost is recorded but not barred. The slerp_antipode run passes A only because the ask itself jumps 179.8 deg. A leaves a 163 deg target jump and 71.6 rad/s there, as the report says. Sure, 80%.

4. Check 2's twist axis (:601). It is the same expression as AnkleFolds' `t`, so a wrong shin line would not be caught. The boot's rotation axis was not compared with the shin's world axis. The line sits 9 deg off the joint's y axis. No profile mixes twist and swing; "oblique" is two swings. Medium, 65%.

5. Check 6's bar. The report says 1 deg, but the code passes at 2 deg (:383, :390). The results (0.70 deg) pass both. Mapping was tested to about 60 deg, not at 110 or more. Sure, 90%.

6. Check 5 is a record, not a bar. The raw 250/-250 runs have no speed readout, so "not wrapped" is inferred from the final pose. The raw runs chain without a reset. The 190/210 runs end at 50-67 rad/s, close to torque cap / c (about 66 for Small). That fits a sustained push, but the mechanism was not read back. Medium, 60%.

7. The rig does what it claims. Boot at 1.9-2.1 m, 0 touches, 14 colliders ignored. Per-axis speed matches B's per-axis argument; the vector speed can be sqrt(3) larger and is not barred. B passes at 19.5 of 20 (97%), Long 15.9 of 16.18. The off peak of 95-103 rad/s is above the research model's 60-81, with no reason given. The Aims call is stateless apart from ankleAsked (unused while AnkleTurnsAtMost is 0), so calling it twice per step is harmless. Fairly sure.
