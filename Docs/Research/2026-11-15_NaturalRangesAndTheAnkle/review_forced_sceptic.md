# Sceptic review of forced_checks.md (Step 4a), November 17, 2026

Shown: forced_checks.md; logs forced_boot2, forced_down_c0b, forced_down_c1, forced_once, forced_once_bad, live_ex_small/round/long (+ _r.xml) by grep. Not read: the C# diff and test code (only AnkleForced.cs:765-791), research.md, per-step files in TestRuns/Forced, the energy files.

Overall: the numbers mostly match their log lines; no run stopped at a limit except one course (below). Findings, most serious first.

1. Check 3 does not isolate the whirl (forced_checks.md:78-83; forced_down_c0b.log:1256,1336,1416). C off already fails the 0.01 m bar on plain makes with no whirl (Small plain 0.0117, Round 0.0162). The report itself says GoDown does not hand on the boot's own speed (NaN the step after, c0b.log:1296). C on gives 0.0000 on every make, also plain (c1.log:1075). So BAR_C passes and fails with or without the whirl; "the path runs on the forced event" is not shown. Report admits "near C's construction". Confidence high.
2. Test pass is not the bar. All _r.xml show asserts="0" (forced_boot2, c0b, live_ex_*). "1/1 passed, exit 0" for c0b says nothing, since BAR FAIL lines do not fail it. Only forced_once_bad fails by assertion.
3. live_ex_small course 0 ended at the cap: "not up after 240.0 s" (log:2239), cap 240 s (log:920). Report line 94-ish "ended by itself after its 2 courses" holds for the test, not for that course. Medium.
4. Misquoted ranges. Off "Small 95.0-103.0" (report:45): log has 91.7 (swing_1800), 83.9 (oblique), 78.3 (slerp); Long low 70.5. A alone "25-43 rad/s": log has 23.2 (oblique) and 71.6 / 72.7 / 72.5 (slerp_antipode, forced_boot2.log:1777); the 71.6 is hidden. "5.4-6.3 short": max in log 6.2. Minor, high confidence.
5. Slerp known-bad is not caught: off gives A PASS on slerp_antipode (log:1443), so BAR_A is vacuous there (report says so). Axes known-bad hypotheses are computed alternatives, not run cases.
6. Check 7: AnkleSpeed "29624 of 29624" is true whenever B is on; only Long/Round (0 steps) test the lazy way. Round/Long NOT EXERCISED match their lines (2574-2709, 2116-2251).
7. Check 8 matches (once_bad xml failed=1, passed=1); exit 2 is not in the log. Known-bad is a test flag, not the old code.

Rig vs game: the report says rig, Small's ways given to all, flat floor, "not the game"; I found no rig result stated of the game. Live check 7 is as played, Small only.
