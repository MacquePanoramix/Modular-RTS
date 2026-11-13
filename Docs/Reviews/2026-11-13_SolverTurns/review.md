# The review of the solver's turns switch, and of step 0 read again (November 13, 2026)

Two independent reviewers who wrote none of what they read (opus, medium; one after the other, the workflow
`wf_27b671e8-8eb`), on the brief beside this file: **A** on the finding and the fix (`A.md`), **B** on step 0's facts
at the true turns and the design (`B.md`). Their verdicts are advice to whoever builds, not Luis's.

## Verdicts

- **A: sound with changes.** The finding holds on two readings (the root's read-back and courses the same to the last
  bit); the fix is correct and complete for every path that makes or remakes the own body (`Begin`, `BeginLow`,
  `BeginDown`, `Remake` all go through `Make`, which sets the turns once the toes are joined; `Advance` keeps them each
  step); the project's rules hold. Three things to do before it is relied on: whether the own body ever sleeps (3),
  that the switch off is the game as played on the game's own body and not only on the facts' leg (7), and that the
  bench says its turns (9).
- **B: sound with changes.** The record's reading of step 0 at the true turns is right as far as it goes; the
  proposed unfloored share for R1's making-up and K5's summed-only bar must not run as proposed (2, 4, 6).

## What was done with each finding

| | Weight | Done |
|---|---|---|
| A1, A2: the switch's run and the finding hold | note | nothing needed |
| A3: OwnBody sets no sleep threshold; a miner standing still could be put to sleep | should | `OwnBodyTests.EachMinersOwnBodyStandingStillIsNotPutToSleep` (explicit), bar set before its run; result in `Docs/Validation.md` |
| A4: the root's own settings confirmed only by getters | note | not done: they are the link's own, not the articulation's; left as a note |
| A5: the bench's post rig sets `immovable` before its link | note | not done; left as a note |
| A6: the fix is complete for every path | note | nothing needed |
| A7: "off is the game as played" shown on the facts' leg only | should | **open.** `OwnRiseBench` now folds the bits of every part's place and turn each step into `RISE_BITS`; run on the game's own bodies at `2ed1c7c` and with the switch's code, switched off, every course differed, but `2ed1c7c` run twice differed from itself too: the bench is not the same run to run, so the comparison shows nothing either way (`Docs/Validation.md`). Still shown on the facts' leg only. Next: both makings in one run from one kept start, the new one run twice first to show the run repeats itself |
| A8: the fallen body (`PhysicalFall`) runs at 14 and 4 whatever the switch | note | said in the report and the handoff; the switch is the own body's alone |
| A9: the bench follows the switch only in part | should | `BodysOwnBench`: its report reads the turns back from each body's root; `-ownIterations`, when given, is now kept by its articulated body whatever the switch (until now set before its links and lost); its body of joints unchanged; the one-ankle rig stays at 14 and 4, set after its link and so kept all along, with a comment that it is a study of one ankle's law, not the game's body |
| A10: the project's rules hold | note | nothing needed |
| B1: the share is to be read against what the spring and damper *said*, as R1 reads it: 0.64 to 0.71 of K8's law at 14 and 4, not 0.74 to 0.89 | should | for the next step's records (R1's making-up) |
| B2: K8's law holds in form, but its R (1/I of the boot) is the held-parent case; the standing leg's parent turns | should | a bar before any rerun: x from the joint's response in the whole articulation at its pose, within 5% in all nine cases; next step |
| B3: K4(a)'s early miss is the ankle's give, its end the sole | note | nothing needed |
| B4: an unfloored share read from `driveForce` ratchets where the give has a ceiling (Round's ankle held at 21.6 N m four steps running, cause unknown) | **blocks the proposed change** | the change to R1 does not run until what caps Round's ankle is found and bounds (a) to (e) of B.md are set before its run |
| B5: bars for that run | should | taken into the next step's plan |
| B6: K5's per-step clause trips on the count's lag; the summed bar alone could hide a one-step injection | should | the count from the joint's own change of place, a prefix clause and a per-step clause with a tolerance, set before the run; next step |
| B7: the design says 18 turns as if the game ran them; with the switch off it runs 7 | should | `motion_design.md` K8: said in a bracket where "18 in the game" stands, with the law's shares at 7; `LawShare` (facts test) to take n from the root before its next run |
| B8: the knee's share is not near 1 (0.77, 1.14, 1.35) | should | next step: the knee's and hip's give against "said" before R1's making-up goes to them |
| B9: what the true turns change in the design | should | none of it into the design as settled before B2's and B4's runs |

## Not checked by either

The engine's source; any run of their own (both read logs only); the panel's new row as drawn; TGS; anything as
played (the flows with `-ownAsPlayed 1`); the tolerances B proposes are reasoned, not tuned on data.
