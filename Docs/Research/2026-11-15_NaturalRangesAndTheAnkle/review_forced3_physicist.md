# Physicist's third review of the forced ankle checks (Nov 18, 2026)

Shown/read: forced_checks.md top sections; review_forced2_physicist.md (head); the diff b76f311..a251444 of AnkleForced.cs (all 363 lines); OwnBody.cs ~1355-1365; PhysicalFall.cs 898-945 (Slacken, Advance); greps of FORCED_SLACK in down_c0f.log, down_c1d.log. Not read: the other logs in full (verdict/bar lines taken from the report), OwnRiser source, research.md 4.2/536-541, the sceptic's review. No Unity run.

## Findings, most serious first
1. Check 3 (AnkleForced.cs ~1096-1115, 1190-1195; PhysicalFall.cs 902-940). BAR_C_PIVOT separates C off from on, but it reads a kinematic, not a physical, effect. The slack is a Lerp/Slerp of the made boot's localPosition/localRotation about a pivot chosen in code (Advance, per step); no joint or PhysX step turns it. A point fixed in the boot's frame staying put under a rotation about itself (dS or dA exactly 0.0000 in every FORCED_SLACK) is geometry, so it says which pivot Slacken was given: that C's branch ran. It does not say C acted on a whirled boot: the made boot is new, the whirling one destroyed, and the whirl enters only via the pre-slack pose and slack turn. Plain makes would separate equally (the plain make, dA 0.019 vs 0.000, did). Sure 85%.
2. The whirl is assumed to matter, not read: nothing reads that the slack's turn depends on the whirl. Fair.
3. Known-bads: FOLLOW_BAD (drive let go, boot hangs 50 deg) and AXES_READ_BAD (swapped X/Y, 161-169 deg) fail for the claimed reason. FORM_BAD and WRAP_BAD feed the judge a wrong input (a flipped switch; raw writes judged as B's), so they test the judge, not B's own code. Fairly sure.
4. AXES_READ now compares the boot's transform turn to the written target: real, but assumes the engine settles still (<1 rad/s) and the 1.2-2.25 deg sag is gravity; not read back.
5. Rule of 3 whirled courses fails inside every C-on run (Long): no single passing C-on test. Sure.
