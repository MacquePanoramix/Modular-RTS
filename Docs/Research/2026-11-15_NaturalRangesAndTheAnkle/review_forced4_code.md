# Code review of 4a‴ (6408d91), AnkleForced.cs only

Shown: forced_checks.md top section, the full diff ef12502..HEAD (590 lines), AnkleForced.cs around the changes, forced_wrapbadB2.log by grep. Not read: the earlier reviews, research.md 4.2, the other logs (claims of the report on feed/down runs taken as stated), runtime files (diff stat shows none touched).

1. Floors are far below the profiles (medium, sure). BAR_B, BAR_FORM, BAR_WRAP floor is LeastSteps = 10 (AnkleForced.cs ~l.170), yet the profiles judge 85, 94, 352 steps (forced_wrapbadB2.log:1156, 1244, 1473). A rig that judged only 10 steps would pass. BAR_A's floor is exact; these are not. BAR_AXES_READ needs 5 holds of a long sweep.
2. BAR_DAMP has no floor and a silent skip (low-medium, fairly sure). l.~1088 `if (own.Riser != riser) break;` leaves the config loop with no EventMissed, so a config can go unjudged on a known-good run unflagged.
3. Numbers not defined once (low, sure). AxesFarFirstDeg (l.157) is never used; the sweep's 115 and the 110 vectors are literals (l.932, 936, 811). Literals remain: T.magnitude > 1 (l.521), `intended.magnitude > 1`, strings saying "0.05 deg", "3.75", "past 110".
4. Clean: BAR_WRAP's known-bad uses the same wrapJudged/wrapViolations counter and the same BAR_WRAP Judge line (l.521-540, 746-752); 85 replaced = 85 judged. B's target is written back before the engine steps. NaN is counted in A, B, FORM, TANH, WRAP, AXES, AXES_READ, FOLLOW, DAMP, C_PIVOT. Feed state and wrapBadB are per-run; switches go back in finally and TearDown. BAR_A_SLERP is unfed (acknowledged).
5. No runtime or off-path change.
