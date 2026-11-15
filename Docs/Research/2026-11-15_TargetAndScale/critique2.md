# Second critique of the target-and-scale research (fresh critic, November 15, 2026)

Critic wrote neither `research.md` nor `critique.md`. Read both in full, with the revision brief. Checked: `TimeManager.asset` (0.02 fixed, 0.33333334 maximum), `ProjectSettings.asset` (`enableFrameTimingStats: 1`), `QualitySettings.asset` (`vSyncCount: 0`), `MinerCrowdBenchmark.cs` and `LookBenchmark.cs` (the interval is `Time.unscaledDeltaTime`, GPU is `FrameTimingManager` `gpuFrameTime`), `OwnBody.cs` (`Advance` called from `FixedUpdate` when the mode is FixedUpdate; toe links; 14/4 on the root), the crowd entries of `Validation.md`, and three web pages (generic queries only). No Unity run, nothing else changed, nothing committed.

Marks: yes, in part, no.

## 1. Point by point

| Point | Answered? | Where | Note |
|---|---|---|---|
| 1.1 | Yes (agreed, nothing to change) | 7 | Fine. |
| 1.2 | Yes | 2.1 (bench rig, "6/1 inferred"), 2.2 (Round's 17.1 as a single-run hint, Small's near-unchanged named), 3.2 (crowd split by what it measured) | All three stretched tags are fixed. |
| 1.3 euphoria | Yes | 1.2, 2.2, sources | Now GTA V, a cut for performance, fan wiki, "weak". |
| 1.3 0.4 s sleep | Yes | 2.2 (sleep row) | Dropped. Section 7 says it "sits in PhysX's API headers": that is a claim from memory with no source; harmless, but it is untagged. |
| 1.3 "proportional to degrees of freedom" | Yes | 1.3, 2.1 | "assuming few contacts"; 1.3-1.6x now a lower bound. |
| 1.3 islands in parallel | Yes | 2.1 table, 2.2 | Parallel claim rests on the Threading page only; Unity side a hypothesis. |
| 1.3 Notebookcheck | In part | 3.1 | "25% below a desktop 4060" is gone and replaced by "within a few percent at full power" (cited to a secondary comparison site, which is what the critic saw). But the Time Spy range moved from 7,900-11,700 to **7,500-11,500** with no stated source. I fetched the cited page: the part I could read gives no Time Spy range or median at all (only a composite rating and other 3DMark tests), so the range stays unverified and is now a different unverified number. Drop it or tag it "not rechecked". |
| 1.3 floor GPU ratio | Yes | 4.1 | Now "near 70%" and "55-60%", honestly attributed to the first critic, not looked up. Still an estimate resting on another estimate; labelled. |
| 1.4 "well over half" | In part | 4.1, B3 | Now "roughly half or more, not known jointly" as the brief told the reviser. But it is still a floor on an unknown intersection. From the marginals alone the joint share could be anywhere from about a fifth (the loosest possible combination of 6+ cores, 8 GB+ VRAM, 16 GB+ memory) to about two thirds. "Half or more" is not a lower bound the survey supports. Say "somewhere between a fifth and two thirds; not known jointly". B3 repeats the phrase to Luis as if it were reassurance. |
| 2.1 | Yes | Start here 2, 2.1 table, 1.3 | The conditional table is there (serial, partial, near-perfect, everything parallel; apart or pressed; colliding or not). The spread 20-150 is called an aim to test. Arithmetic checked: 4 ms / 130-200 µs = 20-30 serial; 4 ms / 52 µs is 77 on the bench figure, fine. The "everything parallel" row is excluded from the spread, which is right. |
| 2.2 | Yes | 2.1 (fight), 2.2 row, B2 | The fight is named the binding case; 30-80 bodies in one island at 14/4 is 4-16 ms (checks: 30 x 130 = 3.9, 80 x 200 = 16). B2 asks about collision after the test. |
| 2.3 | Yes | 2.3 | Max timestep read. The formula F = rest / (1 - c / 20 ms) checks (c = 10, 12, 15, 18 with rest 8 gives 16, 20, 32, 80). The spiral case (about 16 steps a frame, 0.82 of real speed at c = 25) follows from the setting. p99 per refresh rate checks (5 in 6, 1 in 2, 5 in 12, 1 in 2.9, 1 in 3.3). One inconsistency: the floor margin is "at most about 10 ms" in 2.3 but the floor physics line in 3.3 is 6-8 ms; at c = 10 and rest 8 the frame is exactly 16 ms, so 10 ms leaves no room at 60 fps. State that 10 ms is the overload ceiling, 6-8 ms the aim. |
| 2.4 | In part | 3.2 | The CPU/GPU split is now honest: every crowd figure is a whole-frame interval; the deep dive's "7.0 GPU" is relabelled; CPU main and render thread "never measured apart". The reviser's correction to the critic (compare the empty crowd frame with the kept look E, 4.5-5.1, not candidate A) is fair and flagged as "probable, not confirmed". **Remaining error:** 3.2 says "Machine idle (October 3-8): 6.9-7.3 ms at 100 miners; busy 8.0-8.2". `Validation.md` also records 100-miner frames of 7.82, 8.36 (October 8), 7.9, 8.0 (October 9) and **8.5-8.8** (release build, October 9). The first critic quoted 7.3-8.8. The "at most 6.9-8.2 ms" bound in 3.3 is therefore below the records, and the resolution table's upper entries (11-15, 13-18.5) are too low by up to 0.6 ms times the scale. Widen to 6.9-8.8 or say why the later builds are excluded. |
| 2.5 | Yes | 3.3 | Same 12.5 ms line at every resolution; only the pixel-bound share (60-90%) scaled. Factor arithmetic checks (1.59-1.88, 1.83-2.25; room 6.8-8.0, 2-5, 0-4). |
| 2.6 | Yes | 3.1, 3.3, A1 | Screens re-read; both budgeted; Optimus given as a range (0-2% to 10-20%, "none rigorous"). The "still to be read" list is honest. |
| 2.7 | Yes | 3.3 | Called a starting convention; floor-fit table added (fits at about 3 ms non-physics main thread, borderline at 5, fails at 7). Arithmetic checks. The 4 ms is justified as the floor's overload margin, which is a derivation from the 20 ms limit, though the 1.5-2 slowness stays "estimated". |
| 3.1 | Yes | 2.2 | Unit-to-unit collision row added. |
| 3.2 | Yes | 2.2 | Last resort after measuring; names the force-order change. |
| 3.3 | Yes | 1.3, 2.2 | Sleep only what no body holds, touches or hangs from. |
| 3.4 | Yes | 4.2, B4, C1, sec. 6 head | Stiller hair is offered as a drawn-quality loss (C1); "selected unit" removed; coat flaps dropped with the reason; the toe/B4 point corrected ("saves nothing today"). |
| 3.5 | Yes | 6 head, 7 | "Exactly" is gone; the claim "same kind of body under the same rules" appears in 7 and in the dropped-question note. |
| 4.1 | In part | 5.1-5.3 | Stopwatches, `FrameTimingManager` with its lag, `ProfilerRecorder` after `GetAvailable`, development build labelled, nonexistent counters removed. See 3 below for two instrument weaknesses. |
| 4.2 | In part | 5.7-5.8 | What the worker count does and does not do is stated, floor reading is an upper bound, apart/pressed, collision on/off, scripts running. But the boot.config route is not sourced (see 3). |
| 4.3 | Yes | 5.4 | Display and its GPU logged. |
| 5.1 | Yes | A1-C2 | 6.3's spread (B1), 6.4 split (first half dropped by rule, second half B3), "a tenth of a frame" agrees at 1.5 ms "a little under a tenth", research terms replaced, ultrawide offered. |
| 5.2 | In part | 6 | Costs are given or "not known until the test", and the head says nothing runs until he answers. Recommendations still lean (see 4 below). |

## 2. The six verdict fixes

1. **CPU/GPU separated (2.4):** in part. Separated as far as the records allow, and the records' limit is stated. The 100-miner upper bound is too low (above). The budget lines are now a "starting convention" with the floor arithmetic shown. Good.
2. **Unit count conditional (2.1, 2.2, 2.3):** yes. Table, fight as binding case, "design aim, not a result", 20-150 spread, max timestep, p99 per refresh. Small point: B2's "15-30 bodies in its share" does not match 2.1's 20-30 serial at 14/4 and 27-50 at 6/1.
3. **Instruments (4.1-4.3):** in part. Builds mostly on things that exist; see section 3.
4. **Sources (1.2-1.4):** in part. Euphoria, 0.4 s, 25% line, DOF-with-few-contacts and islands are fixed. The Steam share sentence is still not supported (above). The Notebookcheck range is a new unverified number.
5. **Contradictions and overclaims (3.2-3.5):** yes, all five and 6.6(a).
6. **Displays (2.6):** yes. Both screens read and budgeted, Optimus costed, unread items listed.

## 3. New errors the revision brought in

1. **`-job-worker-count` in the player is not established.** The cited Unity page lists it under "Configuration arguments" with no scope; the page itself says Player arguments sit on a separate Player page, and **says nothing of `boot.config`**. The cited second URL (the 2023.3 editor page) redirects to the same editor page. Section 5.7 ("or `job-worker-count=N` in the player's `boot.config`; cited") is therefore not supported by its citation. Fix: cite the Player command-line page, or mark "to be tested in the build" and add a fallback (`Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount` set at start, which is a documented property named on the same page).
2. **The by-hand stepping instrument does not reproduce multi-step frames.** 5.1 steps the physics once per rendered frame with `Physics.Simulate(0.02)`. The game uses `SimulationMode.FixedUpdate` (`OwnBody.cs` line 773 runs `Advance` from `FixedUpdate`). At 60 Hz a frame has 0.83 steps, at 165 Hz 0.3, and an overloaded frame several. A once-per-frame hand step measures a step's cost but not the pacing that 2.3's p99 table and overload test (5.9) depend on. Fix: for the pacing and overload runs keep the game's own FixedUpdate and wrap `Advance` with Stopwatch there, reading the step's cost from the frame's main-thread time (or from a development build's marker); use the hand-stepped scene only for per-step cost.
3. **Release-build `FrameTimingManager` render-thread and main-thread figures are Windows- and API-dependent;** 5.1 notes the GPU lag but not that a GPU reading can be unavailable (zero) on some drivers, as the benchmark code itself guards (`gpuFrameTime > 0`). Minor: log how many frames had no GPU reading.
4. **Ranges narrowed in the budget sentences that gate the stop lines.** 3.4 line 1 uses 12.5 ms; with the 100-miner whole-frame bound at 8.8 rather than 8.2 the panel and ultrawide rows read "none" more clearly. This sharpens, rather than contradicts, the revision's message, but the tables should carry the right number.
5. **3.4 line 2** ("physics step above 4 ms with 50 own bodies in view") will trip at once in the serial case (50 x 130-200 µs = 6.5-10 ms). The text says line 2 moves to the cap later, but as written the first measurement stops the programme. Say that line 2 is a reading to report, not a stop, until the thread-scaling test is in.
6. **The 3.1 performance lines are partly unverified** (range moved; Cyberpunk kept as "not rechecked"). Tagged honestly; remove the range or tag it.
7. **Nothing found that would change how a body moves.** The proposal keeps every physics detail the same at every step; the keeper-in-jobs and joint-held-still ideas are quarantined (last resort; off unless yes). The one near-miss is C1's stiller hair, correctly offered as a visible loss. The optional `start /affinity` is a read-only-effect launch option, fine.
8. **No source misstates what it says** that I could find, other than the items above. Generic queries only; no personal detail sent.

## 4. The questions for Luis

- **Plain words, what he would see, option costs, his choice:** mostly yes. Every question has choices and a recommendation, and the head says nothing runs until he answers. Group A has three questions (at most four allowed). The dropped question (same bodies everywhere) names the rules that answer it; the coat flaps are dropped with the rule.
- **A1** is good: it names both screens and gives pixel multiples. It bundles a second choice (stroke size kept as at 1080p or a fresh look at full sharpness) inside the "And the strokes" bullet; make it A1b so he can answer separately.
- **A2** says what he would see at 60, but for option (2) says only "uneven", with no figure for how much looks he would give up. Say "not known until the test" there.
- **A3** says "50 standing bodies take more than their share of the computer": "their share" has no number. Give it the 4 ms line in plain words ("more than a quarter of a picture's time") or say it is not known.
- **B1 and B2** state their honest spread. B2's "15-30" disagrees with 2.1 (20-30; 27-50). B2's option (3), "only enemies collide", is introduced for the first time there and, as a choice, belongs to him with its cost ("not known until the test"), which is what the text says; fine.
- **B3** repeats the Steam "half or more" as reassurance (above) and gives the alternative's cost only qualitatively (lower floor = fewer units, higher floor = fewer players). Give the one number it has: the floor carries about half to two thirds of what the laptop does (estimated), and say that raising the floor to, say, an 8-core desktop would raise that, cost unknown. The recommendation ("this floor") is firm; add "or you may choose the floor yourself".
- **Order:** group A is the right set before tier 0; B waits for the crowd test; C is later. B4 is the weakest question (it saves nothing today) and could be left out of the questions and kept as a note.
- Technical words left in plain-word text: "cores", "awake", "island" appears only in research sections. Acceptable, as each is explained once.

Every choice is still his: no option is chosen for him, nothing is Locked, Open questions stay open.

## 5. Verdict

**Sound to propose, after five small corrections.** The revision answers all six fixes, most points fully, and brought in no change to how a body moves. Still to change:

1. Widen the 100-miner whole-frame bound to the recorded 8.8 ms (or state why later builds are excluded) and carry it to the 3.3 tables.
2. Replace "roughly half or more" with "between about a fifth and two thirds, not known jointly" (4.1, B3), and give B3 its one numeric cost.
3. Source or soften `-job-worker-count` for the player and `boot.config`, and drop the unverified Time Spy range.
4. Make the pacing and overload tests use the game's own `FixedUpdate` stepping, not a once-per-frame hand step (5.1, 5.9); make 3.4 line 2 a report, not a stop, until the thread test exists.
5. Small plain-word fixes: split A1's stroke choice, give A2's option 2 and A3's "share" a number or "not known", reconcile B2's 15-30 with 2.1, state 10 ms as the overload ceiling and 6-8 ms as the aim.
