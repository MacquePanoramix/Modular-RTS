# Physicist's second review of the forced ankle checks (Nov 17, 2026)

Shown: forced_checks.md (mended), the three first reviews, AnkleForced.cs 1028-1075 and greps, xml results and FORCED_TEST_VERDICT / FORCED_WHIRL / FORCED_GOING_DOWN lines of forced_barfails, forced_good, forced_bad2, down_c0e, down_c1c. Not read: the rest of AnkleForced.cs, OwnBody/OwnRiser source, research.md 4.2 (the report's quotes of it taken as given), steps.csv, down_c0c/c1/c1b/c0d logs, checks 7 and 8. No Unity run.

## Findings, most serious first
1. Check 3 (AnkleForced.cs:1059, 1066). Now a real event: the drive is let go, the speed set once, the engine's read the step after with no set between (down_c1c log:1185: 65 set, 56.1 read; 1774 Long: 65 set read 43.2, 95 read 7.7, -95 read 77.3), the going down in the step it reads 50-80. Mended against first finding 1. But the going-down destroys the boot (Part(Foot) null the step after), so the whirl is never carried on; what is forced is the let-go pivot choice at a whirling boot. BAR_C (0.025 m) does not separate C off from on on every course: down_c0e Long and Round whirled 0.0141/0.0148 pass; the machinery honestly Fails the run. The whip's knee is set at 25 but read 4.5-6.5 at the going down: mostly decayed. Open, sure 85%.
2. BAR_AXES_READ past 110 deg compares the joint's reading to the boot's turn: a reading of the engine's consistency, not a check of the target mapping. Assumed, not read: that the engine holds the boot still below 110. Fairly sure.
3. Check 5 stays a reading; the 51 rad/s against a 60.9 N m limit with torque 3.8-4.2 N m is unexplained (engine's drive/limit behaviour assumed). Open.
4. BAR_A_SLERP cannot catch off (0.995 vs 0.904): a reading only, as stated.
5. Verdict machinery works: barfails Failed, bad2 Passed with named bars failing, c0e Failed for an uncaught known-bad. Sure.

## First physicist review
1 whirl: mended in the event, bar open. 2 check 4 shares: mended (e/E 2.000). 3 BAR_A margin/vectors: mended (written vectors, 1.626 margin). 4 twist axis: mended (own bones, mixed profiles). 5 1 deg axes: mended. 6 check 5: answered as reading, mechanism open. 7: unchanged, fine.
