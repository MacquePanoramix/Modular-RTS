# The performance target and the scale the foundations serve (research, November 15, 2026; revised after its critique)

Research only: no Unity or Blender was run, no file but this one changed, nothing committed. Every number is tagged
**(cited)** from a source listed at the end, **(measured)** in this project's own records (`Docs/Validation.md`,
the deep dive's inventory) or read on this machine, or **(estimated)** by reasoning here. Nothing below is Locked;
every number is a proposal for Luis (the Game Director) to accept or change. **Nothing in it runs until he answers.**
A fresh critic's points (`critique.md`) are answered in section 7.

## Start here

1. **The CPU physics decides the scale.** A standing own body never sleeps: about 60-70 µs a body a step at 6/1,
   107-123 µs at 14/4 (editor, measured). Whether Unity spreads bodies over the i9's cores is **not measured**.
2. **The unit count is a design aim to test, not a result:** about **20 to 150** own bodies in one 4 ms step, by
   how well cores are used, whether bodies press into one island, and whether units collide (table, 2.1).
3. **The fight, bodies in contact, is the likely binding case:** 15-40 a side pressed together may run on one core.
4. **Today's frame figures are whole-frame intervals,** except the ordinary-place GPU times. CPU main thread and
   render thread were never measured apart (3.2). The budget lines are a starting convention (3.3).
5. **His screens (read):** a 3440x1440 ultrawide at 60 Hz on the NVIDIA GPU, the only screen active; the laptop's
   2560x1600 panel at 60 Hz on the Intel GPU (Optimus). Both have 2-2.4 times 1080p's pixels.
6. **The floor's fit is not shown** (3.3): it fits only if today's non-physics main thread is about 3 ms or less.
7. **Physics is never a setting** (2.2); a physics step over 20 ms slows the whole game (2.3).
8. Section 5: instruments that work in a release build. Section 6: questions in three groups; group A comes first.

---

## 1. The final scale

### 1.1 What the vision says

- **Locked:** a slow-paced, fully 3D fantasy RTS with the rhythm of *Age of Empires III*. Its units are far more
  physically expressive than usual. It rewards close observation: "body language, movement, construction, small fights".
  The camera runs from high enough to read armies down to close enough to care about one unit (`GAME_VISION.md`, the
  game in one paragraph, pillars 2 and 4).
- **Locked:** matches are long, and "small engagements should be readable and worth watching".
- **Direction:** "The playable army may need to be smaller than in mass-battle RTS games so procedural bodies,
  behavior, and individual readability remain practical", while maps keep "grandeur and the feeling of a large world"
  (match structure and pacing).
- **Direction:** "the highest useful degree of procedural motion that remains ... performant at RTS scale" (pillar 4).
- **Open:** "What is the practical unit count and target match duration?" (open-decision register). Also open:
  maximum unit and building counts, map size, simulation tick rate, the performance budget, and **crowd collision**.
- **Luis, November 15:** "a good RTS amount of units, not an insane amount, since it's more of a high focus on the units
  RTS, but still a good amount without having any performance issues."
- **Locked technical principle:** intention, order, interpretation and action are kept apart, "eventual networking"
  included. So whatever keeps the physics affordable must not depend on one player's camera (section 2).

### 1.2 What unit-focused and classic RTS games hold

| Game | Cap | What one unit is | Typical fight (estimated from the caps) |
|---|---|---|---|
| *Age of Empires III* (the vision's pace) | 200 population, shared by workers and army (cited) | One soldier or villager; some units cost more than one | About 100 workers and 50-100 soldiers at the cap; fights of 20-60 a side |
| *Age of Empires IV* | 200 population (cited, community) | As AoE III | As AoE III |
| *StarCraft II* | 200 supply (cited, community); most units cost 2 to 6 supply | One unit | 30-80 units a side at the cap |
| *Warcraft III* (hero- and unit-focused) | 100 food; above 50 and 80 food, income falls to 70% and then 40% (cited) | A unit costs 2 to 6 food | Usually 15-30 units a side; the upkeep pushes armies toward the small end |
| *Company of Heroes 3* (unit-focused, physical cover) | 100 population (cited) | A squad of 3 to 6 soldiers costing several population; vehicles cost more | About 15-20 squads, so 60-100 soldiers a side at most (cited inference) |
| *GTA V*'s *euphoria* (physical characters, not an RTS) | Reportedly at most three euphoria-driven characters at a time, cut for performance (cited, a fan wiki only; a fan mod raises it to ten) | A full active ragdoll | A weak hint that fully physical bodies are costly; not an RTS and not a measured limit |

No published unit cap or cost figure was found for *Totally Accurate Battle Simulator*. Its bodies are simple ragdolls
and the game is a sandbox, not an economy RTS. None of these games runs bodies like the own body; the caps say what
"a good RTS amount" means to players, not what this game can carry.

### 1.3 Proposed aim, and what decides it

- **Aim (estimated, to be tested):** 60 to 100 population per player, workers included. In a 1v1, up to about 200
  own bodies on the map, of which a fight of 15 to 40 a side; 40 to 80 bodies in the frame at Strategy height.
  - That is "a good RTS amount": Warcraft III to Company of Heroes, about half of AoE III. The vision's Direction
    toward a smaller army is kept.
  - **The aim comes from the other games, not from the evidence on cost.** The evidence (2.1) gives between about 20
    and 150 bodies in one 4 ms step on his laptop. The aim holds only at the top of that spread. It may come down.
- **What decides it.** The cost of one own body for one physics step, times the bodies awake, divided by how many
  cores the engine and the scripts really use (2.1). Whether units collide with each other decides how often bodies
  merge into one island (2.1, 2.2). The GPU does not decide it, but it decides the screen's cost (3.3).
- The body counts in the cost, not only the bodies seen. A body standing by its own joints is never put to sleep
  (measured: "never put to sleep standing still, either way", `CURRENT_STATE.md`, November 13).
- **Today's body:** 13 jointed parts, and 15 while getting up (two toe links) (read in code, `OwnBody.cs`). The brief
  expects about 20 or more in the final body. PhysX says an articulation's cost grows with its degrees of freedom
  rather than its link count, "assuming few contacts need resolving" (cited). Standing bodies have contacts, so a
  20-link body costs **at least** about 1.3 to 1.6 times today's (estimated, a lower bound).
- **Buildings and props do not add to this count** while they sleep: what no body holds, touches or hangs from
  (section 2.2).

---

## 2. The physics at that scale

### 2.1 What an own body costs, and the count it gives

| Measurement | Value | Tag | Where |
|---|---|---|---|
| 20 copies of each miner lying awake (ground contacts), the engine stepped by hand, editor: µs a body a step at 6/1, 14/4, 28/4, 60/4 | Small 62, 111, 173, 315; Long 70, 123, 184, 353; Round 62, 107, 167, 302 | measured | `Validation.md`, "What the true turns cost" |
| 25 standing bodies on the bench rig (`BodysOwnBench`, not the game's `OwnBody` with its keeper, life and riser), 50 steps a second, editor | 1.3 ms a step (about 52 µs a body) | measured on the bench rig; 6/1 inferred from the November 13 finding that the root's turns were lost | `Validation.md`, S3b step 1, first part |
| The same at 100 and 200 steps a second | 2.6-2.8 ms and 5.2-5.8 ms per fiftieth of a second | measured, as above | same |
| One posed body's hands (a script) | 26 to 41 µs a step | measured | `Validation.md` (several builds) |
| The keeper's, the life's and the riser's scripts per body per step | **not measured** | — | — |
| Whether Unity's PhysX solves separate articulations in parallel on the job threads | **not measured.** PhysX's Simulation page says islands are solved independently (cited); that they run in parallel rests on PhysX's Threading page only (cited); Unity's page for 3D was not found | cited (PhysX) / hypothesis (Unity) | — |
| Player build against editor | not measured for the own body | — | — |

**Per body (estimated).** At 14/4, with its own scripts, about 130-200 µs a step in a player build: the engine's
107-123 µs, plus perhaps 20-80 µs of scripts by analogy with the hands. At 6/1 (as played today), about 80-150 µs.
A 20-link body costs at least 1.3-1.6 times more. The scripts run on the main thread, one body after another, unless
they are moved to jobs; so even a perfectly parallel engine leaves the scripts' part serial.

**Bodies in one 4 ms physics step on his laptop (estimated; today's 13-link body):**

| Case | How the cores are used | 14/4 | 6/1 |
|---|---|---|---|
| Serial | Everything on one core: no parallel islands, or every body pressed into one island | 20-30 | 27-50 |
| Partial | The engine about 3 times faster over several cores; scripts serial | 33-70 | 40-100 |
| Near-perfect engine | The engine about 8 times faster (the 8 P-cores); scripts serial | 42-120 | 45-145 |
| Everything parallel | Engine and scripts both about 8 times faster (scripts moved to jobs, a last resort: 2.2) | about 160-250 | higher |

- **Bodies apart or pressed.** Islands form from contacts and joints (cited). Bodies apart, each touching only the
  static ground, stay separate islands and can use the parallel rows. Bodies pressed together, if units collide with
  each other, merge into one island and fall towards the serial row. Their contacts with each other add cost the
  table does not count.
- **Units colliding with each other, or not.** If units' bodies do not collide with each other (Open: crowd
  collision), a pressed crowd stays in separate islands. If they do, the fight is one island.
- **The fight is the likely binding case.** A fight of 15-40 a side, bodies in contact, is 30-80 bodies in perhaps one
  island: about 4-16 ms a step serial at 14/4 (estimated). Only the small end fits 4 ms. Workers at work are mostly
  apart and are the cheap case.
- **The honest spread.** About 20 to 150 own bodies in 4 ms on his laptop, today's body; about 30 to 150 at 6/1, as
  played. The final 20-link body divides these by at least 1.3-1.6. The "everything parallel" row (7-10 times speed-up)
  is unlikely on a hybrid CPU with memory-bound solver work, and is not counted in the spread.
- **Solver turns.** At 6 and 1 the count is about 1.7 times 14 and 4's for the engine's part (measured ratio
  1.72-1.77). The turns are Luis's open switch (November 13). This research does not choose them; the count must be
  measured at whichever he keeps.
- **Steps a second.** The physics runs 50 steps a second (`TimeManager`, 0.02 s; read in settings). A step's whole
  cost lands in one frame, so the budget must hold a frame with a full step, not the average (p99 lines, 2.3). The
  drawn body is already blended between steps (`OwnBody` writes the segments "between physics steps", read in code).

### 2.2 How games keep many physical bodies affordable, against the project's rules

The rules are: one body for everything; the body's own movement; no fake animation; no switch at a threshold; never
change how a body moves, only how it is drawn.

| Technique | What it is | Keeps the rules? | Where it breaks one |
|---|---|---|---|
| **Spread bodies over cores** (the engine's islands in parallel) | The engine's island split (cited: islands, Simulation page; parallel, Threading page only) | **Yes, fully.** The same body, the same maths | Nothing, if the engine does it. Measured first (5.5) |
| **The keeper's per-body maths in parallel jobs** | The project's own scripts moved to jobs | **Only if proven equal.** A job reads every body's state, then writes; today each body reads and writes in turn. That changes the order in which forces are applied, a movement change, not only rounding | A **last resort, after measuring**: a large change to code the November 13 notes call fragile (turns lost on the root; "never write an unchanged bone each frame"). Checked by equal stay numbers and paired courses |
| **Sleep** (PhysX: every link under the energy threshold for a time; cited) | Unmoving bodies are skipped until touched | **Yes, only for what no body holds, touches or hangs from:** a loose rock, a tool lying alone, a felled tree, a carcass | A living body breathes and is kept up by torques, so it never meets the threshold (measured), and must not. A tool in a hand or at a belt, a lantern hanging, never sleeps |
| **Fewer solver turns far away** | Cheaper, less exact stepping | **No.** Fewer turns change movement (the principle is sound; one hint: Round's fastest part reached 17.1 m/s at 6/1 and 4.2 at 14/4, measured once each way in a test that is not the same run to run; Small's barely changed) | The same push could fell a far body and not a near one |
| **Lower step rate far away** | Bodies stepped at 25 a second | **No.** The miners stand far less surely at lower rates (measured: at 50 without the spring rule, 14 of 25 Small) | Same as above, worse |
| **Simpler body far away** (fewer links; Carlson and Hodgins 1997 and Redon et al. 2005 swap or simplify bodies; cited) | A level of detail of the dynamics | **No, as published.** A second body is not "one body for everything" | One narrow idea is left for Luis (B4): a joint the current action does not use held still. It saves nothing today (below) |
| **Posed or animated bodies off screen or far away** (*GTA V*'s *euphoria* cap, cited weakly) | Only near bodies are physical | **No.** A fake animation wherever it can be seen, and a body changes kind at a threshold | Everywhere visible; off screen it breaks fairness |
| **Physics level of detail by camera distance** (any of the above, keyed to the camera) | Common in single-player games | **No, for this game.** A fight would turn out differently by where one player looks; in multiplayer two cameras disagree | Any change of simulation by camera, garments included |
| **Level of detail by situation** (a body far from any action may run cheaper; the same for every player) | Fair across cameras | **Only if the motion does not change.** Every cheaper stepping above changes it | Allowed only with a technique that leaves the motion identical |
| **Fewer contact pairs** (layers: the boots and hands against the ground and tools only) | Cuts collision work | **Yes,** if bodies still touch where real bodies would | If units passed through each other where they should shove |
| **Units not colliding with each other** | Keeps every body its own island, so the cores stay usable in a fight | **A design question, not a technique.** Crowd collision is Open in the vision | A press of bodies that does not press; a shove that passes through (B2) |
| **GPU physics** | Not exposed for Unity's 3D articulations (hypothesis; no source found) | — | — |

**Conclusion (estimated):** the rules leave cores and sleeping what nothing holds. Whether units collide with each
other decides how much the cores can do in a fight, and that is Luis's question (B2). No distance-keyed or
camera-keyed physics detail is proposed anywhere in this research, garments and hanging things included.

**The joint held still (B4).** The game already gives the toes only while getting up (read in code). So "a joint
resting while unused" describes what already is, and saves nothing today. A saving would come only from holding still
a joint that exists while standing (the fingers when empty, for instance), which the body does not have as links yet.

### 2.3 When a step costs too much: the overload

- **The setting:** `Maximum Allowed Timestep` is 0.333 s (read in `TimeManager.asset`), with a fixed step of 0.02 s.
- **Below 20 ms a step,** frames slow but the game keeps time. A frame interval F carries F/20 ms steps on average, so
  F = rest / (1 − c / 20 ms), where c is one step's cost and rest the rest of the frame (estimated, from how a fixed
  step works). With a rest of 8 ms: c = 10 ms gives 16 ms (60 fps); c = 12 gives 20 ms; c = 15 gives 32 ms; c = 18
  gives 80 ms.
- **At 20 ms a step or more,** each frame needs more steps than it has time for. Unity runs up to about 16 steps in one
  frame (0.333 / 0.02), then lets game time fall behind real time: the game runs in slow motion at a few frames a
  second (for c = 25 ms: about 0.4 s a frame and 0.8 of real speed; estimated).
- **The margin:** the worst fight on the floor must keep its step well under 20 ms. Proposal (estimated): the cap is
  set so that the floor's step in the worst measured fight is at most about 10 ms, half the step's period, so a fight
  larger than measured still keeps time at a lower frame rate rather than slowing the game. A lower maximum timestep
  (for instance 0.1 s) would turn a freeze into slow motion sooner; it is a later choice, not proposed here.
- **The p99 line by refresh rate.** The frames that carry a step are the long ones. p99 is always such a frame.

| Refresh | Frame period | Frames carrying a step | p99 line (1.2 periods; estimated) |
|---|---|---|---|
| 60 Hz (both his screens today) | 16.7 ms | 5 in 6 | 20 ms |
| 100 Hz (the highest the NVIDIA GPU reports for the ultrawide; not confirmed) | 10 ms | 1 in 2 | 12 ms |
| 120 Hz | 8.3 ms | 5 in 12 | 10 ms |
| 144 Hz | 6.9 ms | 1 in 2.9 | 8.3 ms |
| 165 Hz | 6.1 ms | 1 in 3.3 | 7.3 ms |

Above 100 Hz a frame with a 4 ms step and a few milliseconds of everything else no longer fits its period, so the
frames alternate long and short. A higher rate is smooth only when a whole step-bearing frame fits it.

---

## 3. The frame budget on his computer

### 3.1 The machine

- **CPU:** Intel Core i9-14900HX, 24 cores (8 performance, 16 efficient), 32 threads (read locally November 15).
- **GPU:** NVIDIA GeForce RTX 4060 Laptop GPU, 8 GB. **Memory:** 32 GB.
- **Performance class (cited):**
  - Time Spy graphics: median about 10,300 (range about 7,500 to 11,500 over laptops, by power limit) (Notebookcheck).
  - At a full power limit, within a few percent of a desktop RTX 4060 in Time Spy graphics (cited, a secondary
    comparison site); a low power limit costs up to about a third (cited, LaptopMedia's comparison of 45-140 W
    variants). His laptop's power limit is still to be read.
  - About 30-35 fps in *Cyberpunk 2077* at 1440p on High with ray tracing and DLSS Quality (cited, not rechecked).
  - The RTX 4060 Laptop alone is 3.4% of Steam's October 2026 sample (cited, secondary).
- **His screens (read locally, read only, November 15; sent nowhere):**

| Screen | Driven by | Mode now | Modes it lists | Active |
|---|---|---|---|---|
| ASUS 34-inch ultrawide (about 79 x 34 cm), on HDMI | NVIDIA RTX 4060 Laptop, directly | 3440x1440 at 59.97 Hz | 3440x1440 at 59.97 Hz, lower modes at 60-75 Hz; the NVIDIA adapter reports a highest rate of 100 Hz | Yes, the only screen in use (primary) |
| The laptop's own 16-inch panel (about 34 x 22 cm) | Intel UHD Graphics (so the NVIDIA GPU's frames pass through it: Optimus) | 2560x1600 at 60 Hz | Only 2560x1600 at 60 Hz in the list read; a higher rate may sit in a part of the list not read | Not in use now (lid closed or docked) |

- **Pixels against 1080p (2.07 Mpx):** the ultrawide 4.95 Mpx, **2.39 times**; the panel 4.10 Mpx, **1.98 times**.
  The ultrawide is also 21:9: with the vertical view kept (Unity's default; the camera sets no width rule, read in
  `RtsCamera.cs`), it shows about a third more width, so more miners, grass and shadows in view (estimated).
- **Optimus on the panel.** The NVIDIA GPU renders and the Intel GPU shows the frame, so each frame is copied across.
  Sources disagree on the cost: about 0-2% at 60 fps by user reports, up to 10-20% by some guides, larger at high frame
  rates and when the CPU binds (cited, none rigorous). At 60 fps it is likely small; it adds some delay. The ultrawide
  has no such cost. `FrameTimingManager`'s GPU time does not include the copy; the frame interval does.
- **Still to be read on his machine** (locally, sent nowhere): whether the laptop has a MUX switch or "discrete only"
  mode; whether the ultrawide runs above 60 Hz (on DisplayPort or at another setting); Windows' display scale; the
  power mode and plug; the GPU's power limit and clock after ten minutes (`nvidia-smi`); the P-cores' clock under load.

### 3.2 What today's frames cost, and what each figure measured

All release builds at 1920x1080 on his laptop, vsync off (`vSyncCount: 0`, read in `QualitySettings.asset`), frame
timing stats on in the Player settings (read). What each benchmark records was read in its code:

| Figure | What it measured | Value | Tag |
|---|---|---|---|
| Ordinary place (`-wgbenchmark`, `LookBenchmark`) | **GPU only:** `FrameTimingManager`'s GPU frame time | 3.2-4.1 ms (candidate A) to 4.6-5.4 ms (E, the kept look); E with its three details 4.5-5.1 by day, 5.3-5.7 at night | measured |
| Same, "slowest 5% of frames (CPU ms)" | **Whole frame:** the interval between frames (`unscaledDeltaTime`), not the CPU | at most 5.0-6.4 ms | measured; mislabelled CPU in the record |
| One miner at work (`-wgwork`, `MinerWorkBenchmark`) | **Whole frame** (same measure) | 4.5-4.8 ms at work, 4.6-4.7 standing | measured |
| Walking posed crowd (`-wgcrowd`, `MinerCrowdBenchmark`), Strategy view, 0 / 25 / 50 / 100 miners | **Whole frame.** The code also logged a GPU mean in its CSV, but no record reports it | Machine idle (October 3-8): 4.8-5.2 / 5.5-5.7 / 5.9-6.1 / 6.9-7.3 ms. Machine busy (October 6): 5.5 / 6.3 / 6.8-7.0 / 8.0-8.2 ms, p95 up to 10.0 | measured |
| Deep dive's "7.0 ms GPU with 100 miners" | Matches the crowd's **whole-frame** 7.0 of October 3, not a GPU reading | 7.0 ms | measured; mislabelled GPU in the deep dive |
| CPU main thread, render thread | **Never measured apart** in any record | — | — |

**What the records can and cannot tell.**

- With vsync off, a frame's interval is about the slowest of the main thread, the render thread and the GPU
  (estimated). So each whole-frame figure is an **upper bound** for each of the three.
- The empty crowd scene's frame (4.8-5.2 ms) sits close to the kept look's GPU time by day (4.5-5.1 ms), if the crowd
  scene draws that look (probable, not confirmed). So the empty frame is near GPU-bound, or CPU and GPU are near equal.
- What 100 posed miners add (about 2.1-2.8 ms of frame) **cannot be assigned** to CPU or GPU from the records.
- The crowd's 100 miners are **posed** walkers, not own bodies (read: `MinerCrowdBenchmark`). There is **no CPU or
  physics figure for a crowd of own bodies.**

### 3.3 Proposed lines: a starting convention, revised after the measurements

These lines are **not derived from measurement**. They are a convention to start from, revised once section 5's
readings exist (estimated throughout).

- **Frame rate:** a steady **60 fps** on the screen he plays, at its native resolution, every best switch on, after
  ten warm minutes. Both his screens run at 60 today. Above 60 is a bonus where a whole step-bearing frame fits (2.3).
- **Frame lines at 60 fps (16.7 ms):** p95 at most 16.7 ms and p99 at most 20 ms, frames carrying a step included.
- **GPU:** p95 at most **12.5 ms** on any named shot with the crowd: about 25% kept for a hot laptop and Windows.
- **CPU main thread and render thread:** each p95 at most **11 ms**, physics included on the main thread. (The first
  draft gave the render thread 8 ms with no reason; the frame waits on whichever is slowest, so each gets one line.)
- **Physics step plus the bodies' scripts:** at most **4 ms** on his laptop at the cap. Why 4: the floor's cores are
  about 1.5-2 times slower (estimated, not looked up per model) and fewer, so 4 ms here is 6-8 ms there, or more if
  the parallel rows of 2.1 rely on many cores. That keeps the floor's step near half of 20 ms (2.3). It is the
  conservative end; 5-6 ms would fit if the rest of the floor's frame is small.

**Does the floor fit? (estimated)**

| Today's non-physics main thread on his laptop (unknown; at most the crowd frame, 6.9-8.2 ms) | On the floor (x1.5-2) | Plus the floor's physics (6-8 ms) | Against 16.7 ms (60 fps) |
|---|---|---|---|
| 3 ms | 4.5-6 ms | 10.5-14 ms | Fits |
| 5 ms | 7.5-10 ms | 13.5-18 ms | Borderline |
| 7 ms (the frame is CPU-bound) | 10.5-14 ms | 16.5-22 ms | Does not fit |

So the floor fits at 60 fps only if today's non-physics main thread is about 3 ms or less. The records cannot tell
(3.2). The same reckoning on his laptop: 3-7 ms plus 4 ms is 7-11 ms, at or under the 11 ms line. If the crowd frame
is CPU-bound near 7-8 ms, 4 ms of physics on top crosses the 11 ms line on his laptop too.

**What the screens mean for the visual programme (estimated).** The screen passes (Kuwahara, ink, grain, bloom,
grading, MSAA resolve) scale with pixels; shadows, skinning and geometry do not. If 60-90% of today's GPU time scales
with pixels:

| GPU at 1080p (measured) | Panel 2560x1600 (x1.59-1.88) | Ultrawide 3440x1440 (x1.83-2.25, plus a wider view) |
|---|---|---|
| Kept look, no crowd: 4.5-5.7 ms | 7.2-10.7 ms | 8.2-12.8 ms |
| 100 posed miners: at most 6.9-8.2 ms (an upper bound: whole frame) | at most 11-15 ms | at most 13-18.5 ms |

- **Room against the same 12.5 ms GPU line, no crowd:** 6.8-8.0 ms at 1080p; about 2-5 ms on the panel; about 0-4 ms
  on the ultrawide. **With 100 posed miners** (upper bounds): at least 4.3-5.6 ms at 1080p; possibly none on the
  panel; likely none on the ultrawide. The doubling is a projection; G0b measures it.
- This must be measured before G7 (MSAA and 64-bit HDR) or G35 (anisotropic Kuwahara) is judged.
- **The paint must be judged at native resolution.** A Kuwahara radius set in pixels paints finer strokes at 1440 or
  1600 rows than at 1080. The look Luis loves may be a 1080p look. The radius should be set relative to the screen's
  height (G20 can include it). On the ultrawide the extra width shows more world, not larger strokes (question A1).

### 3.4 The lines that would stop the visual programme to show him

**Proposal (estimated), for Luis (question A3); it does not run until he answers.** Measured on his laptop in a
release build at native resolution on the screen he plays, every kept switch on. The programme stops and shows him if:

1. **The crowd scene's GPU p95 is above 12.5 ms.** The scene: G0b's 100 posed miners plus 25 own bodies standing.
2. **The CPU main thread's or render thread's p95 is above 11 ms, or the physics step above 4 ms,** with 50 own
   bodies standing in view.
3. **Any single switch adds more than 1.5 ms of GPU** at native resolution (a little under a tenth of a 60 fps
   frame). It is reported with its picture before it is kept.
4. **The deep dive's working bar:** the running sum more than 30% over today's GPU or frame time in the all-on
   100-miner scene.

When the own body can walk and work in a crowd, line 2 moves to the cap Luis accepts after the crowd measurement.

### 3.5 May LOD0 grow denser, and where

- **Yes, near the camera only (proposal, question C2).** Today LOD0 is about 16,000 triangles a miner (measured; LOD1
  about 5,000, LOD2 about 1,600).
- **Triangles are probably cheap here.** Skinning 100 x 16k is small for this GPU (surf research, hypothesis to be
  measured). So a denser LOD0, perhaps 20,000-30,000 (estimated), may go where it shows: the hair locks (G3); the hands'
  and face's silhouettes; the cinematic Explore close-ups.
- **The conditions:** LOD0 only within the Explore and portrait distances (Strategy height shows LOD1 or LOD2: a miner
  there is about 30 px tall at 1080p, about 40 px at 1440 rows and 45 px at 1600; estimated). LOD changes cross-fade
  (G5). The crowd GPU line holds. Every new bone is render-only and never added to the physics.

---

## 4. The floor and the steps below

### 4.1 The floor machine

Steam survey, October 2026 (cited, secondary summaries; opt-in sample):

- **Most common GPU:** RTX 3060 (3.6-3.8%), tied with RTX 5070.
- **VRAM:** 8 GB 26.9%, 16 GB 26.4%, 12 GB 12.9%.
- **CPU cores:** 8 cores 28.7%, 6 cores 27.3%.
- **System memory:** 16 GB 41.4%, 32 GB 37.1%.
- **Primary display:** 1080p 50.2%, 1440p 21.8%, 4K 5.1%.

**Proposed floor (estimated):**

- a 6-core, 12-thread desktop CPU of about 2020 (Ryzen 5 3600 or Core i5-10400 class);
- a 6-8 GB GPU of the RTX 2060 / RTX 3050 / GTX 1660 Super class: by Time Spy, an RTX 2060 is near 70% of his GPU and
  a 3050 or 1660 Super near 55-60% (estimated from the critic's reading; not looked up per model here);
- 16 GB of memory;
- 1080p at 60 fps on the Low step.

The share of players at or above this floor is **roughly half or more, not known jointly**: the survey gives each
part's share on its own (cores, VRAM, memory), and they cannot be combined (estimated).

**The floor CPU sets the unit cap.** It runs every body in full (section 2). The cap is measured on his laptop and
treated as an upper bound for the floor (5.5); 3.3 shows the fit is not yet shown. Better: a real floor-class
machine, if one is ever at hand.

### 4.2 The steps

| Step | Best (his laptop, native) | High | Medium | Low (the floor, 1080p) |
|---|---|---|---|---|
| **Shadows** | Main light 4 cascades, high resolution, soft; the house's lights cast shadows | 4 cascades, medium | 2 cascades, shorter distance | 2 cascades, low resolution; local lights without shadows except the hearth |
| **Anti-aliasing** | MSAA 4x if G7 earns it, else SMAA high | SMAA | SMAA low | FXAA or SMAA low |
| **Colour precision** | 64-bit HDR if G7 earns it | 32-bit HDR | 32-bit | 32-bit |
| **Paint (Kuwahara)** | Anisotropic (G35), depth-aware radius (G20) | Basic Kuwahara, depth-aware | Basic, fewer samples | Basic, half-resolution pass, radius kept relative to screen height so the strokes read the same |
| **Ink and grain** | Full | Full | Full | Full (cheap; they are the look) |
| **Grading** | Hour-keyed (G34) | Same | Same | Same (a lookup costs little; dusk must stay dusk on every machine) |
| **Depth of field** (Explore only) | Bokeh, script focus (G33) | Gaussian | Gaussian, half resolution | Off |
| **LOD distances** | Bias 1.0, cross-fade | 0.85 | 0.7 | 0.5 |
| **Hair** | LOD0 locks and their springs near the camera (G3, G31) | Same, springs to a shorter distance | LOD1 locks near, springs on every visible miner | **Luis's choice (C1):** springs on every visible miner, or stiller hair beyond a distance |
| **Render-only bones** (correctives, helpers) | Every visible miner | Within mid distance | Near only | **Luis's choice (C1)** |
| **World density** (grass, plumes, fireflies, particles) | Full | 80% | 60% | 40% |
| **Upscaling** (Unity's STP or similar, painted after) | Off (native) | Off | Optional | Optional, with the paint drawn after the upscale |
| **Physics, every body** | Full | Full | Full | Full |
| **Garment chains, flaps and hanging things** (physical objects on the body) | Full | Full | Full | Full |

**Does the rule hold at every step?**

- **Yes for the body.** No step changes the own body's links, solver turns, step rate, scripts or contacts.
- **Garments and hanging things are physics, so they are never a step.** Every step runs them in full.
- **Hair springs and render-only bones are drawn and feed nothing back to the physics,** so thinning them changes how
  the body is drawn, not how it moves. But hair that rests at its bind values on Low while it swings on his machine is
  a **visible loss of drawn quality**, against the rule that hanging things hang and swing. A detail keyed to the
  selected unit is a threshold of the kind the vision dislikes. Neither is a free step: both are offered to Luis (C1),
  and "selected unit" is no longer proposed.

---

## 5. The instruments (a plan G0b, the visual turn's tier 0, can build)

Extend G0b's capture command; do not add a second tool. Every reading below works in the build being measured.

1. **Timings in a release build.**
   - **Stopwatch timers** (`System.Diagnostics.Stopwatch`) around each body's `OwnBody.Advance` (it runs the riser and
     the keeper; read in code) and around the physics step. For the step, the benchmark scene steps the physics by
     hand (`SimulationMode.Script`, then `Physics.Simulate(0.02)` after every body's `Advance`), as the project's
     benches already do; the order (bodies first, then the step) is the game's. Written per step to the CSV.
   - **`FrameTimingManager`** (enabled in the Player settings: `enableFrameTimingStats: 1`, read): the main thread's
     frame time, the render thread's, the present wait and the GPU's (cited). The GPU time arrives a few frames late
     and depends on the platform and driver (cited); the readings are matched to their frame, not to the latest one.
   - **The frame interval** (`unscaledDeltaTime`), labelled "whole frame", never "CPU".
   - **`ProfilerRecorder`** only for built-in markers flagged available in non-development players: at start-up the
     benchmark lists them with `ProfilerRecorderHandle.GetAvailable` and checks `MarkerFlags.AvailabilityNonDevelopment`
     (cited), and logs which physics markers exist. Custom `ProfilerMarker`s record nothing in a release build (cited)
     and are not used there.
   - Log mean, p95, p99 and max per shot, and the physics steps in each frame.
2. **Counts.** The Unity physics profiler module has no "awake articulations" or "contact pairs" counter (cited). It
   has Articulation Bodies (total), Active Dynamic Bodies (Rigidbodies and ArticulationBodies not asleep, so links
   count each), and Narrowphase Touches (touches new or lost since the last frame, not live pairs). Those are read
   only in a development build. In the release build the scene counts its own: bodies awake by each root's
   `IsSleeping`, and the bodies in contact with another unit, each step.
3. **A development build for marker-level profiles** (the engine's island and solver markers, the counters above),
   labelled "development build" on every figure, since it perturbs the timings. Never mixed with release figures.
4. **Conditions logged with every run:**
   - **the display: which GPU drives it, internal or external, its resolution and refresh;** vsync; quality step;
     every switch;
   - posed and own body counts, triangles per LOD, solver turns;
   - plugged in or not, the power mode;
   - GPU clock, power and temperature from `nvidia-smi` beside the run (local; about once a second, for heat, not
     for frames);
   - minute 1 against minute 10.
5. **Rules of a run** (from the project's own lessons, `Validation.md`): a release build, the window in front, the
   machine otherwise idle; two runs per condition (runs of one build differ by about 0.5 ms); editor numbers labelled
   as editor.
6. **The crowd check (G0b), widened:**
   - **posed walkers:** 0, 25, 50 and 100 (as today);
   - **own bodies standing,** with the keeper, life and riser scripts running (the game's `OwnBody`, not the bench
     rig): 0, 10, 25, 50, 100 and 200, **apart** and **pressed together**;
   - **units colliding with each other on and off,** pressed;
   - each at 6/1 and at 14/4;
   - when the body can work and walk, the same counts at work and in motion;
   - the named shots at native resolution on each screen he uses, and at 1080p;
   - the all-on scene of 3.4.
7. **The thread-scaling test** (decides the count): the 50- and 100-body scenes, apart and pressed, collision between
   units on and off, scripts running, with the job workers set to 1, 2, 4, 8 and 16 (`-job-worker-count`, or
   `job-worker-count=N` in the player's `boot.config`; cited).
   - **What it does:** it sets the most worker threads the job system may use (cited).
   - **What it does not do:** it does not choose performance cores over efficient ones (Windows places the threads),
     so above 4-8 workers the readings mix core types; it does not slow a core, so few workers here is not a floor
     machine. Whether Unity's PhysX uses these workers at all is the hypothesis being tested: if the step does not
     change with the count, that is the answer, not a fault.
   - **Optional, by Luis's own hand:** starting the build restricted to some cores (Windows' `start /affinity`) to
     separate P-cores from E-cores. It changes nothing on the system.
   - **Reading it:** if the step does not fall with workers, the count sits in the serial row of 2.1 until the engine
     or the scripts are parallelised. If it falls about linearly while apart, the parallel rows hold for bodies apart.
     The pressed and colliding runs say what a fight costs.
8. **The floor estimate:** the same scenes with 4 workers, as a rough floor reading; the cap from it is an **upper
   bound** for the floor (slower cores are not simulated).
9. **The overload:** the 200-body pressed scene, colliding, run until the step crosses 20 ms if it does, to see the
   slowing of 2.3 and set the margin.
10. **What is written:** a CSV per run, plus one table per change in `Validation.md`: the cost of each switch at native
   resolution and the running sum against 3.4's lines.

**Not tested by this research:** all of it. No Unity run was made; every estimate above waits for these readings.

---

## 6. Questions for Luis, in plain words

Nothing in this research runs until you answer. Each question says what you would see, the choices, what each costs,
and a recommendation.

**Dropped, because the project's rules already answer it:** whether bodies are the same on every computer. Your rule
"one body for everything" (October 14), "no fake animations" and "no switch at a threshold" (October 13) already say
yes: a weaker computer draws less finely but never moves a body differently. Also dropped: thinner coat flaps far
away. Coats and hanging things are physical objects on the body (your rule "anything that reads as an object is held,
hangs, swings"), so changing them with distance would change physics by where a player looks.

### A. Needed before the visual work's first measuring step (tier 0) starts

**A1. Which screen do you play and judge on?** Today you play on the 34-inch ultrawide (3440x1440). The laptop's own
screen is 2560x1600. Every picture and number so far was made at 1920x1080, a smaller picture.
- *What you would see:* on either of your screens the painted strokes come out finer than in the pictures you know,
  unless the stroke size is tied to the screen's height. On the ultrawide you also see about a third more world to
  the sides.
- *Choices:* (1) judge on the ultrawide; (2) judge on the laptop screen; (3) judge both.
- *Cost:* the ultrawide draws 2.4 times the pixels of 1080p, the laptop screen 2 times. Drawing may cost about
  1.6-2.3 times today's (estimated). Judging both doubles the pictures to check.
- *And the strokes:* keep them the size you know from 1080p on any screen, or take a fresh look at full sharpness.
- **Recommendation:** judge on the ultrawide, since you play there, with the strokes kept the size you know; check the
  laptop screen once at the end of each look.

**A2. How smooth?** Both your screens show 60 pictures a second today.
- *What you would see:* at a steady 60, bodies' movements never stutter. Above 60 is smoother still, but only if the
  ultrawide can go higher (its graphics card says up to 100; not confirmed).
- *Choices:* (1) a steady 60 at the best looks; (2) aim higher and give up some looks.
- *Cost:* at 60, a picture has 16.7 thousandths of a second. At 100 it has 10, and the moment the bodies move (every
  fiftieth of a second) may not fit: pictures would come unevenly, which feels worse than a steady 60.
- **Recommendation:** a steady 60 at the best looks; more only where it fits without giving up a look.

**A3. When should the picture work stop and show you?** The visual work would stop and show you, with pictures, if:
the busy crowd scene no longer runs a steady 60 on your screen; 50 standing bodies take more than their share of the
computer; or any one new look costs more than 1.5 thousandths of a second (a little under a tenth of one picture's
time).
- *Choices:* these lines; stricter lines (it stops more often to ask); looser lines (it asks less, and the slowdown
  shows later).
- *Cost:* not known until the test. On your screens the room may already be small (perhaps 0-5 thousandths of a
  second with no crowd; estimated), so the work may stop early.
- **Recommendation:** these lines, revised after the first measurement.

### B. Needed only after the crowd measurement

**B1. How many units?** The aim is like *Warcraft III* or *Company of Heroes*: 60 to 100 per player, workers
included, fights of 15 to 40 a side. Every one is a real body standing by its own joints, and that is what costs.
- *Honestly:* the real number could end up anywhere from about 20 to 150 bodies awake at once, depending on one test
  (how many of the computer's cores carry the bodies together) and on B2. It may come down.
- *Choices:* (1) accept the aim, knowing it may come down; (2) a smaller aim (30-60) from the start; (3) wait for the
  test before choosing.
- **Recommendation:** accept the aim as an aim, decide the number after the test, with pictures of 50, 100 and 200
  standing together if you want them.

**B2. Do units bump into each other?** (This is still open in the vision.)
- *What you would see:* (1) bodies press, shove and lean on each other in a crowd or a fight; (2) they pass through
  each other except in a blow or a grab; (3) something between, for instance only enemies collide.
- *Cost:* bodies that touch each other are worked out together, on one part of the computer. A fight of bodies that
  touch may hold about 15-30 bodies in its share; bodies that do not touch may hold several times more (estimated).
  Not known until the test.
- **Recommendation:** decide after the test shows both. If touching fights hold too few, (3) keeps the shove where
  it matters.

**B3. The weakest computer, and what it costs.** The proposal is a mid-range gaming PC from about 2020 (six cores, a
6-8 GB graphics card like an RTX 2060 or 3050), at 1080p on Low, steady 60: roughly half or more of Steam's players.
- *What you would see:* nothing on your machine. On the weakest computer, the same bodies, drawn less finely.
- *The trade:* because bodies are never simplified, the weakest computer sets the unit count for everyone. A lower
  floor means fewer units; a higher floor means fewer players can play. The count on the floor against yours is not
  known until the test; on today's reckoning the floor may carry about half to two thirds of what your laptop does
  (estimated).
- *If a computer cannot keep up in a huge fight,* the game would slow down rather than skip (2.3). The cap is set so
  the weakest computer never reaches that in the largest fight measured.
- **Recommendation:** this floor, decided with the count after the test.

**B4. A joint held still while unused (off unless you say yes).** A body might hold still a joint it is not using
and that cannot be seen, for instance the fingers when the hand is empty.
- *What you would see:* nothing, if it is right; a hand that stays still where it might have twitched, if it is not.
- *Cost:* it saves nothing today (the body has no finger joints in its physics; the toes already appear only for
  getting up). It matters only if a later body adds joints and the count falls short.
- **Recommendation:** leave it unless the count falls short, and then judge it yourself in motion.

### C. Needed later

**C1. Stiller hair on the weakest computer?** On Low, hair could swing only near the camera and rest still beyond.
- *What you would see on the weakest computer:* a crowd whose hair hangs still while it swings on yours.
- *Choices:* (1) hair swings on every visible miner on every computer, and the cost is found elsewhere; (2) stiller
  hair beyond a distance on Low.
- *Cost:* small for (2), but it is a visible loss of the thing your rules protect. Not known until the test for (1).
- **Recommendation:** (1) unless the floor test shows it does not fit.

**C2. More detail up close.** Close up in the Explore view, the miners may get finer models: smoother hair, hands and
faces. At the strategy height they stay as today.
- *Cost:* probably small (not known until the test), with no visible jump when the detail changes.
- **Recommendation:** yes, close up only.

---

## 7. After the critique

`critique.md` beside this file, point by point.

- **1.1** Agreed; nothing to change. The figures stand.
- **1.2** Answered in 2.1 and 2.2: the 25-body figure is now "measured on the bench rig, 6/1 inferred"; Round's 17.1
  is a single-run hint, Small's near-unchanged result named; the crowd table is split by what it measured (3.2).
- **1.3** Answered: euphoria corrected to *GTA V*, a performance cut, fan wiki, weak (1.2, 2.2); the 0.4 s figure
  dropped (it sits in PhysX's API headers, not on the cited pages, and the project's own measure makes it unneeded);
  "with few contacts" added, 1.3-1.6x now a lower bound (1.3, 2.1); the parallel claim rests on the Threading page
  (2.1, 2.2); Notebookcheck: "25% below a desktop 4060" removed, now "within a few percent at full power"; the
  "20% above a 3060" line dropped as unverified (3.1). Cyberpunk kept, marked not rechecked.
- **1.4** Answered: "roughly half or more, not known jointly" (4.1, B3).
- **2.1** Answered: the count is a conditional table and a design aim to be tested; spread about 20-150, 30-150 at
  6/1 (2.1, 1.3, B1). The critic's 30-150 checked: true for today's body at 6/1; the serial 14/4 case goes to 20.
  The scripts' serial share was added: even a perfect engine leaves them on one thread.
- **2.2** Answered: the fight named the binding case; units colliding or not is now a row and question B2, asked
  after the measurement, not before (2.1, 2.2, B2).
- **2.3** Answered in 2.3: the maximum timestep read (0.333 s), the slowing explained, a 10 ms floor margin
  proposed, the p99 line per refresh rate.
- **2.4** Answered in 3.2, with one correction to the critic: the crowd's 0-miner frame should be compared with the
  kept look's GPU time (4.5-5.1 by day), not candidate A's 3.2-4.1; the gap is then 0-0.6 ms. The core of the point
  stands and is taken: every crowd figure is a whole-frame interval, the deep dive's "7.0 GPU" is a frame figure, and
  CPU and render thread were never measured apart. The lines are redone as a convention, and on his laptop 4 ms of
  physics may cross 11 ms if the crowd frame is CPU-bound (3.3).
- **2.5** Answered: the room is now compared against the same 12.5 ms line at every resolution, and only the
  pixel-bound share is scaled (3.3).
- **2.6** Answered: the screens re-read locally, read only (3.1); both are budgeted (3.3); Optimus's cost given; A1
  now offers the ultrawide.
- **2.7** Answered: the lines are a starting convention; the 4 ms is derived from the floor and the overload margin,
  and the floor's fit is shown as a table: not shown to fit (3.3).
- **3.1** Agreed. Unit-to-unit collision added as a design row (2.2).
- **3.2** Answered: the keeper's maths in jobs is now a last resort after measuring, changing force order (2.2).
- **3.3** Answered: sleep only what no body holds, touches or hangs from (1.3, 2.2).
- **3.4** Answered: stiller hair and fewer helper bones on Low are a drawn-quality loss offered as C1; "selected
  unit" removed (4.2). 6.6(b) dropped as distance-keyed physics (6, dropped). 6.6(a) now says it saves nothing today
  (2.2, B4).
- **3.5** Answered: the "exactly" question is dropped as already answered by rules; nowhere does the text claim
  identical results across computers. Unity's PhysX is not bit-identical across machines or step pacing; the claim
  is the same kind of body under the same rules.
- **4.1** Answered in 5.1-5.3: stopwatches, `FrameTimingManager` with its lag, `ProfilerRecorder` only after
  `GetAvailable`, a labelled development build; the nonexistent counters removed and the real ones named.
- **4.2** Answered in 5.7-5.8: what the worker count does and does not do; apart and pressed; collision on and off;
  the scripts running; the floor reading an upper bound.
- **4.3** Answered: the display and its GPU are logged (5.4).
- **5.1** Answered: 6.3's honest spread (B1); 6.4 split, its first half dropped by rule, its second half B3; "a tenth
  of a frame" now 1.5 ms, "a little under a tenth" everywhere (3.4, A3); research terms replaced; the ultrawide
  offered (A1).
- **5.2** Answered: every question gives the alternative's cost or "not known until the test"; nothing runs until he
  answers (head, 3.4, 6).

---

### After the second critique (`critique2.md`): sound to propose after five small corrections

These corrections are made after the weekly reset, before tier 0 builds anything. Group A's questions went to Luis
with the plain-word fixes (item 5) already taken in.
1. The 100-miner whole-frame bound (8.2 ms) is below the 8.5-8.8 ms recorded in `Validation.md`. Widen it, and carry
   it into section 3.3's tables.
2. "Roughly half or more" of Steam's players is unsupported. Write "between about a fifth and two thirds, not known
   jointly", and give B3 its one number.
3. `-job-worker-count` for the player and `boot.config` are not on the cited page. Cite them, or mark them unverified.
   The Notebookcheck Time Spy range (7,500-11,500) needs a source that can be checked.
4. A hand-stepped `Physics.Simulate` does not reproduce frames with several steps, or the overload test. Use the
   game's own `FixedUpdate` for those. Section 3.4's line 2 is a report, not a stop, until the thread test exists.
5. Plain words:
   - split A1's stroke choice from the screen;
   - give A2's option 2 and A3's "share" a number, or "not known";
   - B2's 15-30 disagrees with section 2.1's 20-30;
   - state 10 ms as the overload ceiling and 6-8 ms as the aim.

## Sources

**Project records**

- `Docs/Correspondence/2026-11-15_THE_TWELVE_QUESTIONS_ANSWERED.md` (question 7, the machine).
- `Docs/GAME_VISION.md` (the game in one paragraph, pillars, match and pacing, movement and combat, the open register,
  the November 15 rows).
- `Docs/Research/2026-11-15_TheBestVisualQuality/research.md` (2.3, 3.1, 3.4, 3.6, tier 0 and 4, 6.2, question 7);
  `inventory.md` 1.0.
- `Docs/Validation.md`: the ordinary-place benchmark; the crowd benchmarks of October 3, 4, 6 and 8; S3b step 0 and
  step 1; "What the true turns cost".
- `Docs/Playtests/OrdinaryPlaceLookTest.md` (runtime cost).
- `Docs/CURRENT_STATE.md`, November 13.
- `Docs/Technical/UnityProjectContext.md`: `MinerCrowdBenchmark`, `OwnBody`, `BodysOwnBench`.
- Code read: `MinerCrowdBenchmark.cs`, `MinerWorkBenchmark.cs`, `LookBenchmark.cs`, `OwnBody.cs`, `RtsCamera.cs`.
- Settings read: `ProjectSettings/TimeManager.asset`, `DynamicsManager.asset`, `ProjectSettings.asset`
  (`enableFrameTimingStats`), `QualitySettings.asset` (`vSyncCount`).
- This machine's displays, read locally November 15 (Windows' video controller and monitor lists; read only).

**Unit caps**

- Age of Empires forums, "Population limit 200?": https://forums.ageofempires.com/t/population-limit-200/28596
- AoE forum poll on population (AoE III and AoE IV at 200; StarCraft II at 200):
  https://forums.ageofempires.com/t/poll-population-limit/137760
- Battle.net, Warcraft III upkeep: https://classic.battle.net/war3/basics/upkeep.shtml
- Company of Heroes 3 population cap: https://ggrecon.com/guides/company-of-heroes-3-population-cap
- Company of Heroes 3 squad sizes: https://videogames.si.com/guides/company-of-heroes-3-best-infantry
- GTA wiki, euphoria (GTA V's three characters): https://gta.wiki/w/Euphoria
- Euphoria Ragdoll Overhaul mod (three to ten): https://de.gta5-mods.com/misc/euphoria-ragdoll-overhaul-ero

**Physics**

- NVIDIA PhysX 5.4 articulations (cost by degrees of freedom with few contacts; sleep):
  https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/docs/Articulations.html
- NVIDIA PhysX simulation (islands): https://nvidia-omniverse.github.io/PhysX/physx/5.4.0/docs/Simulation.html
- NVIDIA PhysX threading: https://nvidia-omniverse.github.io/PhysX/physx/5.1.1/docs/Threading.html
- Unity ArticulationBody reference: https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/articulationbody
- Unity physics profiler module (its counters): https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/profiler-physics
- Carlson and Hodgins, "Simulation levels of detail for real-time animation", Graphics Interface 1997:
  https://graphicsinterface.org/?p=2676
- Redon, Galoppo and Lin, "Adaptive dynamics of articulated bodies", SIGGRAPH 2005:
  https://history.siggraph.org/?p=113935

**Hardware and screens**

- Notebookcheck, RTX 4060 Laptop GPU: https://www.notebookcheck.net/NVIDIA-GeForce-RTX-4060-Laptop-GPU-Benchmarks-and-Specs.675692.0.html
- Notebookcheck, RTX 4060 against RTX 4060 Laptop:
  https://www.notebookcheck.net/NVIDIA-GeForce-RTX-4060-vs-NVIDIA-GeForce-RTX-4060-Laptop-GPU-vs-NVIDIA-GeForce-RTX-4060-Ti-8G_11593_11455_11592.247598.0.html
- Nanoreview, RTX 4060 Laptop against RTX 4060 (Time Spy): https://nanoreview.net/ru/gpu-compare/geforce-rtx-4060-mobile-vs-geforce-rtx-4060
- LaptopMedia, RTX 4060 laptop variants by power limit:
  https://laptopmedia.com/analysis/comparing-all-rtx-4060-variants-45w-vs-80w-vs-105w-vs-120w-vs-140w-power-limits-dont-matter-anymore/
- Optimus and MUX switches: https://laptopwiki.eu/laptopwiki/guides/general/optimus ,
  https://www.laptopmag.com/features/the-mux-switch-is-overhyped-heres-why ,
  https://www.howtogeek.com/780933/what-is-a-mux-switch-in-a-gaming-laptop/
- Steam survey, October 2026 (secondary summaries):
  https://www.guru3d.com/story/steam-october-survey-puts-the-rtx-3060-back-at-the-a-sampling-caveat/
  and https://gamingpcguru.com/steam-hardware-survey-october-2026-explained/

**Instruments**

- Unity FrameTiming: https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/frametiming
- Unity blog, Frame Timing Manager: https://unity.com/blog/engine-platform/detecting-performance-bottlenecks-with-unity-frame-timing-manager
- Unity, `MarkerFlags.AvailabilityNonDevelopment` (and `ProfilerMarker` being development-only):
  https://docs.unity3d.com/ScriptReference/Unity.Profiling.LowLevel.MarkerFlags.AvailabilityNonDevelopment.html
- Unity forum, release-build profiler recorders: https://discussions.unity.com/t/allow-filtering-release-build-working-profilerrecorderdescriptions/860199
- Unity command-line arguments (`-job-worker-count`): https://docs.unity.com/en-us/engine/6000.6/manual/unity-editor/command-line-arguments/editor
  and the player's `boot.config`: https://docs.unity3d.com/2023.3/Documentation/Manual/EditorCommandLineArguments.html

**Not verified**

- Whether Unity's 3D PhysX spreads separate articulations across job threads.
- The floor GPUs' and CPUs' benchmark positions, per model.
- Any player-build figure for the own body; any CPU-thread figure for any scene.
- The laptop panel's highest refresh rate and the ultrawide's above 60 Hz; MUX switch.
- That the crowd scene draws the kept look (E).
