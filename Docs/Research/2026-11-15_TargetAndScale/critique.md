# Critique of the target-and-scale research (fresh critic, November 15, 2026)

Critic did not write `research.md`. Read in full, with the brief. Checked: `Docs/Validation.md` (the turns-cost entry, S3b step 1 first part, the crowd tables, the November 13 switch entry), `TimeManager.asset`, `OwnBody.cs` (toe links), the cited web pages (generic queries only), and the local display (read only). Nothing else changed, no Unity run, nothing committed.

Marks: agree, agree in part, refute.

## 1. Tags and sources

**1.1 Project-measured numbers match their records. AGREE.**
Per-body costs (62/111/173/315 etc.), 25 standing bodies 1.3 ms a step at 50 steps a second, 5.2-5.8 ms at 200, the crowd frame times (4.8-5.1, 5.6-6.1, 6.3-6.8, 7.3-8.8 ms), the 13 plus 2 toe links, "asleep in 0 of 500 steps": all found in `Validation.md` or `OwnBody.cs`. Fixed timestep 0.02 confirmed.

**1.2 The tag "measured" is stretched in three places. AGREE IN PART.**
- "25 standing own bodies, 6/1 in effect": the October 9 entry does not say 6/1. It is inferred from the November 13 finding that the root's turns were lost. Also it is the bench's rig (`BodysOwnBench`), not the game's `OwnBody` with keeper, life and riser scripts. Tag it "measured on the bench rig, 6/1 inferred".
- "Round's fastest part moved 17.1 m/s off and 4.2 on": from `ShovedAgainAndAgainNothingOfItFlies`, one run each way, a test that fails both ways and "is not the same run to run" (`Validation.md` ~5133-5136). Small went 25.0 to 22.7, barely changed; only Round is quoted. It is a hint that turns change motion, not a measurement of how much. (The principle itself, that fewer turns change movement, is sound.)
- The crowd table mixes "whole frame" and "GPU" (see 2.4).

**1.3 Cited numbers: several do not match their sources. AGREE IN PART.**
- Match: Steam October 2026 (3060 3.62%, 5070 3.56%, 16 GB 41.42%, 32 GB 37.06%, 8 cores 28.68%, 6 cores 27.29%, VRAM 26.92/26.35/12.91, 1080p 50.23%, 1440p 21.82%, 4K 5.05%, RTX 4060 Laptop 3.40%); CoH3 cap 100 (and population cost not equal to soldiers); Warcraft III/AoE/SC2 caps agree with what I know.
- **Mismatch (refute): euphoria.** The cited wiki page gives "three" for *GTA V*, as a performance reduction, not for *GTA IV*. The table row and the "only precedent found" claim rest on it. Fix to GTA V or drop the row (it is a fan wiki, not an RTS, and the claim is weak).
- **Mismatch: sleep "0.4 s by default".** The cited PhysX articulation and simulation pages do not state a wake counter or 0.4 s (the articulation page says only that all links must meet the criteria; there are no per-link sleep states). Cite the right page or drop the number. The project's own measurement (threshold 0.005, never asleep) makes it unneeded.
- **Mismatch: "cost proportional to degrees of freedom, not links".** The page adds "assuming few contacts need resolving". Standing bodies in a crowd have contacts, so the 1.3-1.6x for a 20-link body is a lower bound, not a mid estimate.
- **"Islands solved independently" is on the page, but "in parallel by the task system" is not** (the Simulation page does not say it). The research does mark the Unity side a hypothesis, which is honest; the table row 2.1 should say the PhysX parallel claim also rests on the threading page only.
- **Notebookcheck: partly unverifiable, one point likely wrong.** I could not read the Time Spy figures on the cited page. Searches show a Time Spy graphics median near 10,300-10,400 (the "about 10,500, range 7,900-11,700" is plausible but the range is unverified). "About 20% above a desktop RTX 3060" is a game-benchmark claim from another outlet, not Time Spy. **"About 25% below a desktop RTX 4060" looks wrong**: the comparison pages I could reach put the two within a few percent in Time Spy graphics. This does not affect the plan (GPU times are measured locally) but should be corrected or removed.
- Not checked: Cyberpunk 30-35 fps, retailer panel listings, the "half to 60%" floor GPU ratio (the research admits unlooked-up; by Time Spy a 2060 is nearer 70% of his GPU, a 3050 or 1660 Super nearer 55-60%).

**1.4 "Covers well over half of Steam's players" (4.1, 6.5). AGREE IN PART.**
Each share is a marginal (cores, VRAM, memory, GPU). Multiplying or intersecting them is not possible from the survey. The sentence is an estimate from independent marginals; label it, or say "roughly half or more, not known jointly".

## 2. Unit-count range and frame budget

**2.1 The range (60-100 a player, up to ~200 awake) follows from comparison with other games, not from the evidence on cost. AGREE IN PART.**
The research says so itself ("the cap comes after one measurement"), which is honest. But the Start-here puts "at most about 200 own bodies awake" as the number, while its own arithmetic gives 20-30 bodies in a 4 ms step on one core and 100-180 only with perfect parallelism. 200 bodies at 130-200 µs is 26-40 ms serial, so it needs 7-10x parallel speed-up to fit 4 ms; the stated 100-180 for 8 P-cores already exceeds a plausible gain (E-cores are slower, memory-bound solver work). Present it as a conditional table: serial 20-30, perfect parallel 100-180, and say plainly the proposal is a design target to be tested, not a result.

**2.2 The worst case is the common case. REFUTE (the framing; the facts are right).**
Bodies in contact join one island and lose the parallelism (the research says so). A fight of 15-40 a side is exactly bodies pressed together. 40 a side touching could be one island of 80 bodies, around 10-16 ms serial, not 4 ms. The cap for the typical fight may be 25-40 touching bodies, far below "fights of 15-40 a side ... 200 awake". "Crowd collision" is Open in the vision; whether units collide at all with each other decides this more than any core count. The ordering in section 6 (asking for the range before this is known) puts the cart first.

**2.3 The overload failure mode is missing. REFUTE (omission).**
`Maximum Allowed Timestep` is 0.333 s. If a fixed step costs more than 20 ms on the floor, Unity runs several steps per frame, each costing more: the game does not just drop frames, it slows or spirals. The floor CPU's behaviour at the cap must be measured for this, and the cap must leave margin below it. Also at 60 fps a step lands in 5 of 6 frames, but at 165 Hz one in 3.3; the p99 line is not stated per refresh rate.

**2.4 The CPU main-thread budget (11 ms) does not reconcile with today's measurements. REFUTE.**
Today's 100-posed-miner crowd is 7.3-8.8 ms a whole frame at 1080p, with no own-body physics. Compare: GPU alone with 0 miners is 3.2-4.1 ms, but the whole frame is 4.8-5.1 ms, so the frame is already 1 ms or more above GPU time: it is probably CPU or render-thread bound at the low end, or driver-bound. If the 100-miner frame is CPU-bound near 8 ms, adding 4 ms of physics gives about 12 ms, over the 11 ms line. If it is GPU-bound, the "GPU 7.0 ms" scaled by 1.98 (11-13 ms at 1600p) is fine, but then "whole frame" cannot also be used as the GPU figure in 3.2/3.3. The research uses both readings. It must separate CPU and GPU in the existing figures before the lines are accepted. This is the biggest gap in the budget.

**2.5 "Room shrinks from about 9 ms to 0-4 ms" compares different budgets. REFUTE (as arithmetic).**
9 ms is 16.7 minus ~7.0 (a full-frame budget). The 0-4 ms is 12.5 minus 11-13 ms. At 1080p against the same 12.5 ms line the room is about 5.5 ms, so the pure resolution effect is 5.5 down to roughly 0-1.5, not 9 down to 0-4. The message (resolution matters a great deal) holds; the numbers as written overstate the resolution effect by mixing in the headroom choice. Also "about twice" assumes every pass scales with pixels; shadows and skinning do not, so 11-13 may be high, and the doubling should be measured, not projected.

**2.6 The panel was read-able now. AGREE IN PART (research left it open; I read it).**
A local read on this machine (read only, sent nowhere) shows the NVIDIA GPU driving a **3440x1440** primary display at about 59 Hz, and the Intel integrated GPU driving a **2560x1600** panel at 60 Hz (probably the laptop's own, with no MUX direct path, so Optimus). Windows reports only the 3440x1440 as an active screen right now (docked or lid closed). So the real native resolution is not necessarily 2560x1600: 3440x1440 is 4.95 Mpx (2.39x 1080p, 1.2x 2560x1600) and 21:9, which widens the frustum (more draw calls and miners in view). The frame-time estimates, the "judge at native resolution" question and the paint radius all need both cases. The research's "to be read" list stands, but the readable part could have been read.

**2.7 The frame lines themselves (60 fps, 12.5 GPU p95, 11 CPU p95, 4 ms physics, 8 ms render) are reasonable as proposals but not derived. AGREE IN PART.**
They are all "estimated". 60 fps / 16.7 ms with 25% headroom is a defensible convention. The 4 ms physics share has no derivation (why not 6 or 3?), and the "floor is 1.5-2 times slower" is asserted. On the floor the same 4 ms becomes 6-8 ms, which then leaves 3-5 ms for everything else against the same 11 ms: not shown to fit. Say the lines are a starting convention to be revised after section 5 readings.

## 3. Physics detail, lower steps and the project's rules

**3.1 The table in 2.2 and its conclusion (cores and sleeping the lifeless are the only levers) are sound. AGREE.**
Fewer turns, lower step rate, simpler bodies and camera-keyed detail do change movement (the project's own bench supports that for turns and rate). The posed-far-bodies and euphoria rows rightly fail "no fake animation". One extra lever the table missed: **reducing contact pairs by layers** is listed and sound; **raising island independence by not colliding unit-to-unit** is what 2.2 above says is decisive, and is a design question, not a technique.

**3.2 Parallelising the keeper's maths in jobs: AGREE IN PART.**
Rated "keeps the rules fully", with the caveat about rounding. Add that it also changes timing of when forces apply (read-then-write order), which is a movement change if not exactly equivalent; the check "equal stay numbers" is the right one, but it is more than a rounding risk. It is also a large change to code that the November 13 notes call fragile (turns lost on the root, "never write an unchanged bone each frame"). Mark as a last resort after measuring.

**3.3 Sleeping "the lifeless" is fine, but the list is incomplete. AGREE IN PART.**
Carried tools and held items are objects that belong to a living body; the research says props may sleep. A tool in a hand must not sleep and wake. State the rule: sleep only what no body holds, touches or hangs from.

**3.4 The lower steps (4.2) claim to leave movement alone, but two rows come close to it. AGREE IN PART.**
- "Hair: LOD1 everywhere; springs only on the selected unit" at Low, and "render-only bones ... selected and nearest few": on a low machine, other units' hair rests at bind values while the same hair swings on his machine. The research argues it is "drawn only". But the project's own rule (physical objects hang and swing, never stiff or floating) makes a stiff-haired crowd on Low a visible loss of the thing being protected, and a selection-keyed detail is a threshold of the kind the vision dislikes. Offer it to Luis as a drawn-quality loss, not a free step.
- **6.6(b) contradicts section 2.2.** 2.2 rejects "physics level of detail by camera distance" because two cameras disagree; 6.6(b) offers coat flaps with fewer links "far away". Garments are physical objects; fewer links far away is exactly a distance-keyed physics detail. The research calls it a drawing change but its own rule says otherwise. Either drop (b) or state clearly it is the one exception and why it cannot affect play.
- 6.6(a) (a joint held still while unused) is a narrow, reasonable idea, correctly kept for Luis. But toes do exist only during getting up in the code, so the "same body, one joint resting" is only a description of what already is; it saves nothing today. The research implies savings that do not exist yet.

**3.5 "Same bodies on every computer ... stand, push and fall exactly as on yours" (6.4). REFUTE (as worded).**
The same bodies and rules yes; "exactly" no. Multithreaded PhysX, different CPUs and different frame pacing (steps per frame) are not bit-identical, and the project already notes its test bench "is not the same run to run". Say "the same kind of body under the same rules", not "exactly".

## 4. The instruments plan

**4.1 Release-build measurement with custom markers and physics counters is not buildable as written. REFUTE.**
- `ProfilerMarker` is a Development-player-only API: in a release build the markers around the keeper, life, riser and hands record nothing. `ProfilerRecorder` in release works only for markers flagged available in non-development players; check with `ProfilerRecorderHandle.GetAvailable` in the release build first.
- The Unity physics profiler module (cited page) has **no "active articulations" counter and no "contact pairs" counter** in its current mode: it has Articulation Bodies (total), Active Dynamic Bodies (includes articulations), and Narrowphase Touches, which counts touches lost or new since last frame, not live pairs. The plan names counters that do not exist as described.
- Working alternative: wrap the scripts' `Advance` calls and a hand-driven `Physics.Simulate` (the project already steps by hand in its benches) in `Stopwatch` timers, written to the CSV in a release build; use a development build only for marker-level profiles and label it, since it perturbs timings. `FrameTimingManager` (CPU main, render, GPU times) does work in release when enabled in Player settings; GPU time arrives a few frames late and is platform-dependent, which the plan should note.

**4.2 The thread-scaling test: AGREE IN PART.**
It is the right first measurement and decides the scale. Weaknesses: the job worker count (`-job-worker-count`) limits threads, it does not choose P-cores over E-cores, so 1/2/4/8/16 on the i9 mixes core types above 8; and it does not shrink per-core speed, so "P-cores capped to 4" is not a floor machine (a 2020 6-core is slower per core). The plan says "a rough floor reading", which is fair, but the cap derived from it should be treated as an upper bound for the floor. Also run the test with bodies both apart and pressed (the plan does) and with units colliding with each other on and off (it does not; see 2.2). And the scene should keep the keeper/life/riser scripts running, not only articulations.

**4.3 Other parts: AGREE.**
Logging conditions (power, plugged in, GPU clock, minute 1 vs 10), two runs per condition, release build with the window in front, editor numbers labelled, CSV plus a `Validation.md` table. `nvidia-smi` samples about once a second, fine for thermals, not frame-level. Add the display (which GPU drives it, whether external) to the logged conditions given 2.6.

## 5. Questions for Luis

**5.1 Plain words: mostly good. AGREE IN PART.**
They avoid jargon in the question bodies (no p95, MSAA, islands). Problems:
- 6.3 asks him to accept a range before 6.4 and 6.5 are decided, although the floor decides the count and fights may collapse to one island (2.2). It should say honestly: "the number could end up anywhere from about 30 to 150 depending on a test; do you accept the aim, knowing it may come down?"
- 6.4 bundles two things: "same bodies everywhere" (a principle that is already the project's rule) and "weak computers decide the unit count" (a real trade-off he has not seen as a number). Split them, and give the likely cost: for example "a unit count of roughly X on the floor against Y on your laptop", once measured, or say it is not known.
- 6.7 "a tenth of a frame" is 1.7 ms at 60 fps; the text elsewhere says 1.5 ms. Make them agree. Also "100-miner scene" and "50 own bodies" are research terms; say what he would see.
- 6.2 offers two options but omits the third (3440x1440 ultrawide, if that is how he plays); see 2.6.

**5.2 Every choice stays his. AGREE IN PART.**
The choices are framed as proposals with recommendations, and 6.6 is "off unless you say yes". But the recommendations are heavy in 6.1, 6.4 and 6.5 (each states a firm recommendation without the cost of the alternative in numbers), and the decision on 6.4 quietly fixes the physics rule and the floor together. Section 3.4 of the research also puts numeric ceilings that "stop the programme" before he has answered 6.7. They are labelled proposals; state that nothing runs until he answers.

## Verdict

**Sound as a research frame and honest about what is not measured; not ready to be accepted as it stands.** The rule analysis (no physics detail change keeps the body's movement; cores and sleeping the lifeless are the levers) is right. Needed before Luis is asked:

1. Separate CPU from GPU in the existing frame figures (2.4); redo the budget lines after.
2. Reframe the unit-count range as conditional on island behaviour and unit-to-unit collision, with the fight case (bodies in contact) as the likely binding case (2.2); add the overload/maximum-timestep failure mode (2.3).
3. Rebuild the instruments plan on stopwatches, `FrameTimingManager` and a development build for markers; remove nonexistent counters (4.1).
4. Fix the source errors: euphoria is GTA V, the 0.4 s sleep figure, the "25% below a desktop 4060" line, the "well over half of Steam" claim (1.3, 1.4).
5. Remove the contradiction between 2.2 and 6.6(b); soften 6.4's "exactly" (3.4, 3.5).
6. Read the real display(s) (2.6) before the resolution question goes to Luis.
