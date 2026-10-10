# Wonder Gather — Independent Project Review & Agent Handoff

**Date:** 10 October 2026  
**Prepared by:** OpenAI GPT-6, as an outside reviewer  
**For:** Luis (Game Director), Claude, and future Wonder Gather collaborators  
**Repository:** https://github.com/MacquePanoramix/Modular-RTS  
**Snapshot reviewed:** `main` at [`b82e5196ba42adb2cbabcc291c8c723a75a11863`](https://github.com/MacquePanoramix/Modular-RTS/commit/b82e5196ba42adb2cbabcc291c8c723a75a11863)  
**Suggested repository location:** `Docs/Reviews/2026-10-10_Independent_Outside_Review.md`  
**Status:** **Independent advisory assessment — NOT a design decision, approved milestone, task order, or source of authority.**

> **A note to Claude and future agents:** Luis asked for an independent evaluation so a perspective from outside the implementation loop can be preserved. Store this as a dated review, with its provenance and qualifications intact. Do **not** silently promote its suggestions into `Locked` decisions, change the milestone order, or treat the reviewer's preferences as Luis's preferences. Where this review conflicts with more recent direct instructions from Luis, those instructions govern. Before acting on a recommendation, check `AGENTS.md`, `Docs/CURRENT_STATE.md`, `Docs/GAME_VISION.md`, `Docs/DESIGN_RATIONALE.md`, the current plan, and any newer correspondence.

## 1. Executive judgment

Wonder Gather is an ambitious, coherent, and technically substantive *early prototype*, not merely an idea or a demo scene. Its distinctive premise is that players **design an entire civilization as a network of possible units, structures, resources, and progression paths**, then command that creation in a slow-paced 3D RTS while observing individual beings that have convincing physical presence.

The strongest version of the game is **not** “Age of Empires with a faction editor,” nor “a beautiful physics simulation of miners.” It is the relationship between the two: **the player imagines a way of living, constructs it from meaningful parts, and sees the consequences of those parts come alive in the world.**

The central development risk is **two exceptionally demanding projects inside one project**: (1) a freely composed but strategically intelligible and balanceable civilization system, and (2) expressive, stable, sufficiently inexpensive physics-driven bodies whose behavior is worth watching. Either can absorb enormous resources without validating the other. The remedy is *not* to abandon the physical-body ambition, which Luis has explicitly reaffirmed. It is to make sure successive experiments demonstrate **player-visible choices, consequences, readability, and enjoyment**—not only increasingly sophisticated simulation.

**One-sentence north star from this review:** *The game succeeds when players want to try another creation because they have seen their previous creation behave in a way that was intelligible, surprising, and emotionally engaging.*

## 2. Evidence scope and confidence

This review was based on direct inspection of the public GitHub repository, including its design, rationale, current-state documents, roadmap, historical validation, and representative C# systems; repository tree and development history; and the recorded internal reviews. I examined, among others, `CivilizationValidator`, `CivilizationDefinition`, `UnitBlueprint`, `BuildingBlueprint`, `CivilizationSession`, `FactionDraft`, `FactionRecord`, `FactionStore`, `FactionCreatorView`, `IUnitCommand`, `OwnBody`, `ProceduralBiped`, `PhysicalBalance`, `PhysicalFall`, `PhysicalBody`, `MinerBody`, `Boulder`, and `RockWork`.

The snapshot contained approximately **1,113 tracked blob files**, **147 `.cs` files**, and **12 Unity scene files**. These numbers describe repository contents, not development quality. The repository was created on **6 September 2026** and is moving quickly.

**Important limitations:** I did **not** open Unity, compile the project, rerun its tests, launch the Windows build, inspect every binary asset in-game, or independently playtest camera feel, animation, atmosphere, or moment-to-moment fun. Test counts and performance figures below are **reported in the repository's validation records**, not measurements I reproduced. Code findings are stronger evidence than promotional claims, but reading code does not prove runtime behavior. The observations are time-bounded to the commit above; newer changes may supersede them.

Use the labels below when interpreting claims:

- **Observed in source:** directly supported by inspected code or repository structure.
- **Reported by project:** stated in project validation/playtest documents, not independently rerun.
- **Reviewer judgment:** interpretation, concern, or recommendation—not an established project fact.

## 3. What is genuinely distinctive

### A. A civilization is a *way of becoming*, not merely a roster

The defining design idea is a graph of possibility rooted in the starting setup: starting base → starting units/resources → build permissions → buildings → production links → downstream capabilities. Players author **how** something can come into existence, not just **what** it looks like once purchased.

The separation of **pre-match design value** from **in-match cost** is promising. For example, direct access to a powerful unit can be expensive in design value, whereas access through a longer, fragile production chain might be cheaper before the match but harder to execute during it. This creates structural tradeoffs: immediate power versus reachability, speed versus fragility, redundancy versus efficiency.

A particularly fertile source of strategy could be **discoverable structural weaknesses**. An opposing player may learn that a seemingly formidable civilization depends on one fragile producer, rare resource, or specialist builder. The strategy arises from understanding another player's authored system.

**Caution:** The value of a component is context-dependent. Synergies, bottlenecks, timing, redundancy, resource scarcity, and counterplay make universal pricing difficult. Do not assume that summing component costs will yield fair civilizations. The exact design-budget formula remains open and should be developed from experiments involving *whole* civilizations, not only individual units.

### B. Observation is part of play

Wonder Gather's two-camera philosophy—**Strategy** for fluent RTS command, **Explore** for intimate observation—is more fundamental than presentation polish. The close view should reveal real differences in bodies, load, stance, work, hesitation, encounters, and interactions.

The design principle that ordinary moments should be worth pausing for is unusually coherent with slower pacing. Ideally the world will produce moments worth watching **without scripting those moments solely for the camera**: a miner shifts a burden, a supply route passes a warmly lit house, a hesitant unit responds to danger, or a fragile production network finally produces something extraordinary.

**Caution:** The two cameras must not split the game into “playing” versus “watching.” The ideal interface lets curiosity and strategic control coexist. Explore optional follow/focus modes and readable alerts later, but only after essential controls and player experience warrant them.

### C. Fictional identity has mechanical consequences

The planned creator seeks relationships between body proportions, strength, equipment, personality, abilities, and visual character. That can be far more meaningful than a set of raw percentage sliders. A lightly built worker and a powerful worker should not merely display different numbers; their choices should affect what they can physically accomplish and how their work looks.

The test of successful customization is not “how many parameters exist?” It is “can the player understand and anticipate what the creation will do differently?”

### D. Physical emergence without slapstick

The physical-body ambition is coherent: bodily actions should follow believable capacities and forces, not be canned performances indifferent to mass, grip, momentum, and terrain. The game explicitly rejects comedy physics as its main aesthetic. This is a demanding, valid artistic/technical ambition rather than an accidental scope expansion.

However, **physically caused** does not automatically mean **visually readable, delightful, or strategically useful**. Design perceptual expression alongside physical correctness; keep the causal behavior genuine while using body shape, material movement, timing, camera, lighting, and sound to help players see its meaning.

## 4. What the project has actually built

| Domain | State at reviewed snapshot | Evidence boundary |
|---|---|---|
| RTS foundations | Selection, grouped movement, navigational fallback, camera systems, basic contextual orders | Implemented prototype systems, not a finished RTS |
| Economy and settlement | Finite resource collection, delivery, construction, unit production | Playable simplified loops; not full economic simulation |
| Civilization authoring | Data-backed unit/building blueprints, build/produce links, faction editing, reachability warnings | Genuine foundation; no fully general designer/balance system |
| Saving | Versioned faction JSON, stable identifiers, token checks, backups/migrations | Thoughtful early persistence, still prototype-bound |
| Character models and visual world | Small/Long/Round miners, Blender-authored assets, rigs/LODs, painterly Ordinary Place, dusk lighting, two cameras | Project-reported/rendered evidence; live visual feel not independently assessed |
| Physical mining | Tool mass, grip, strength, fatigue, contact-based work, boulder strikes and loose stone, balance, falling/getting up | Extensive implementation and benching; work remains |
| Articulated “body's own” | Switchable physics-driven standing with breathing/look/weight-shift experiments | S3b step 1 only; **not** yet general physics-driven walking/work |
| Worker character creator | Intended high-depth character creation | Not yet built to intended showcase quality |
| Physical hauling options | Hands, sack/backpack/cart interactions as meaningful alternatives | S4 proposal, not complete |
| Combat/morale/multiplayer | Long-term design intentions | Not yet proven in a playable representative loop |

A specific architectural strength is that the creator's chosen equipment is applied through blueprint/runtime identity rather than a totally separate, showcase-only character type. Preserve this integration. It is how a one-worker prototype can teach the final civilization system.

### Latest documented physical-body state

In `Docs/CURRENT_STATE.md`, **S3b step 1** is built, with a panel switch allowing a miner to stand using forces produced at its own articulated joints. It gives control back to the posed locomotion/work system when sent to move or work, and falls if sufficiently disturbed. The project reports stability under its particular tested conditions, but its review of six visual judgments found that breathing and weight shifts were **not convincingly visible**, and the head seemed to move without the rest of the body joining it. Step 3 (stepping) was moved ahead of step 2 (knees-to-feet), at Luis's direction.

This is meaningful progress **and** evidence that “a measurable simulation” and “a living-looking character” are different acceptance criteria. Do not describe the entire movement system as already fully physical.

## 5. Engineering assessment

### Real strengths worth preserving

1. **Reasonable separation of responsibilities.** Input, commands, movement, blueprints, runtime state, editor setup, and tests have dedicated areas. `IUnitCommand` and dispatch are a useful order boundary. They do not by themselves make networking, replay, AI authority, or determinism solved.
2. **Civilization graph validation.** `CivilizationValidator.Validate` computes a fixed-point set of reachable units/buildings, allowing cycles but not self-seeding ones. It reports unreachable content and economy risks separately. Its own comment correctly cautions that warnings are **not** a solvability or balance proof.
3. **Persistence caution.** `FactionRecord` version 5 and `FactionStore` distinguish stable IDs from Unity object references, provide migration behavior, detect competing file changes, and replace saves with backups. This is thoughtful work on a system where silent corruption would be costly.
4. **A serious evidence habit.** `Docs/Validation.md` records test outcomes, regressions, repaired failures, performance bench setups, and explicit “not tested” limitations. This is especially valuable in AI-assisted development.
5. **Reproducible art and measured rendering.** Script-authored Blender assets, capture/review techniques, performance budgets, and camera-distance checks show an effort to connect aesthetics to runtime constraints rather than treating the two as separate disciplines.
6. **Responsive direction-setting.** The documentation regularly captures Luis's actual feedback and marks what was accepted, rejected, merely proposed, or explicitly superseded.

### Increasing technical risks

**Subsystem coupling and large files.** The reviewed `ProceduralBiped.cs` (~1,417 lines), `PhysicalFall.cs` (~969), `OwnBody.cs` (~892), `MinerBody.cs` (~782), and `PhysicalBalance.cs` (~663) are examples of growing, interconnected systems. Long source files are not automatically defects, and physics experiments naturally accumulate complexity. Nevertheless, the latest validation records a concrete cross-system regression: a breathing-related chest-size update affected pickaxe contact during a mining test. This is a warning to clarify contracts between action intent, physics control, visual rigging, object attachment/contact, and resource extraction.

**Recommendation:** Do not order a sweeping rewrite solely for smaller files. Extract stable concepts around recurring faults, build focused contract tests, and verify that physical invariants survive presentation changes. A candidate boundary is: desired action / body state → force/constraint solving → presentation rig → object contact → gameplay consequence → diagnostics.

**Slow regression feedback.** The October 10 validation reports a complete suite of **191 tests: 170 passed, 0 failed, 21 opt-in/explicitly skipped**, taking approximately **24 minutes**. This is valuable for integration, but too slow to be the sole feedback loop. Prefer a fast smoke selection, subsystem-specific regressions, and a slower full pre-merge/milestone run. The target minutes are engineering proposals, not existing benchmarks.

**Automation gap.** At the reviewed snapshot there was no committed `.github/workflows/` CI configuration in the repository tree. Add lightweight automated checks when operationally practical; don't adopt heavyweight infrastructure just for appearances. Relevant Unity builds and visual captures may still require local/editor-specific validation.

**Documentation drift.** Despite excellent records, some “current” paragraphs are stale relative to later sections and code: `Validation.md` opens by calling the Living Worker current; an older “Not yet built” subsection of `CURRENT_STATE.md` contradicts its newer S3/S3b history; the README foregrounds older validation counts; and some branch notes predate the October 10 merge into `main`. Keep one compact, trustworthy **as-of snapshot** and clearly mark historical passages. Don't erase history; make its status obvious.

**Repository hygiene and asset provenance.** The public snapshot has no root `LICENSE` file. That is not an immediate gameplay blocker, but licensing for code, source art, generated assets, and outside visual references should be clarified deliberately before broader outside contributions or distribution. Do not choose a license or assert redistribution rights on Luis's behalf.

**Physics at scale.** Older release benchmarks were encouraging: the October 8 record describes approximately **7.31 ms/frame for 100 walking miners** in Strategy view at 1080p on an RTX 4060 Laptop. That measurement **does not validate 100 fully articulated miners all mining, hauling, interacting, or fighting**. The newer joints/active-body work has narrower performance coverage. Profile representative combined workloads before making promises about army scale.

**Network and competitive architecture.** Long-term multiplayer needs decisions about authoritative simulation, persistence/versioning, and synchronization. Physics-driven bodies may be costly or nondeterministic across machines. A likely candidate is server-authoritative gameplay outcomes with careful client presentation, but this is only a future architecture hypothesis, not a decision or reason to implement networking now.

## 6. The five largest project-level risks

1. **Two deep research problems in parallel.** Full civilization authorship/balancing and performant physics-driven characters could each dominate the schedule. Protect a visible connection between their milestones.
2. **Technical sophistication outrunning the fun.** Joint measurements, realistic forces, and green tests are meaningful only insofar as they eventually communicate player-understandable behavior and worthwhile choices.
3. **Open Workshop balance.** Modular synergies, hidden bottlenecks, structural fragility, nonlinear costs, degenerate loops, and counter-strategies cannot be solved reliably by a simple additive cost formula. Study whole factions and match outcomes.
4. **Readability and scale.** Fine physical details visible in close-up may disappear in Strategy view; dense, physical battles may become confusing or expensive. Define evidence at *both* relevant camera distances.
5. **Showcase isolation.** A beautiful, technically fascinating miner risks becoming a separate simulation with no path back to faction authoring and strategic play. Keep the showcase's data and consequences compatible with civilization blueprints.

A sixth, more artistic tension deserves deliberate later work: Wonder Gather's tender, poetic, cozy environment and its multiplayer warfare need a consistent emotional vocabulary. This is **not** a call to remove conflict. It is a question about how battle, danger, loss, spectacle, and gentleness coexist in the same world.

## 7. Recommended validation path — advisory, not a replacement roadmap

These suggestions broadly respect the current project order **S3b → S4 → S2 → S5**, followed by a return to the larger RTS vision. Luis has already approved S3b before S4 and moved stepping earlier; this review does not undo that.

### S3b — A body acting through its own joints

Continue the physically driven standing/stepping/walking/reaching/work research. Alongside solver metrics, ask:

- Does the movement look **alive** rather than robotic or posed in real-time video?
- Can a player see meaningful differences in strength, burden, fatigue, and recovery without a debug panel?
- Does the body remain stable under relevant slopes, turns, interruptions, pushes, and carrying conditions?
- Does an order still feel responsive and legible at both camera distances?
- What does the whole system cost for *several* bodies in a representative environment?

Be careful not to optimize one standing benchmark until it becomes an unrepresentative goal. The genuine objective is a believable *whole action*.

### S4 — One complete mining-and-hauling journey

Prove the linked consequences of **tool → movement/work → real extraction → real load → transport → delivery**. Compare at least two distinct body/equipment configurations doing the **same task**. A strong worker might carry tool and stone directly; a smaller worker might need a pack, sack, or cart. The specifics should follow Luis's approved S4 choices, not be silently inferred from this illustration.

Watch for failures that teach rather than simply stall: can the game explain *why* the worker cannot lift, carry, reach, or move something, and can the player redesign to overcome that limitation?

### S2 — A creator that explains consequences

The creator should support experimentation, not just numeric editing. The strongest candidate experience is to change a physically meaningful attribute or equipment choice and immediately preview a representative real task. Prefer comprehensible visual feedback, honest capability limits, and reversible exploration. The current `FactionCreatorView` is a useful temporary Unity IMGUI scaffold—not evidence that the final creator UX is solved.

### S5 — External users observing without coaching

Give a small number of fresh humans the build, and observe: what do they understand without explanation, when do they feel lost, and what makes them spontaneously want to try another configuration? Pair human playtests with objective performance and regression checks. Agent reviewers and static frame sheets are useful but do not substitute for humans actually playing the game.

### Return to the broader RTS premise

After the showcase demonstrates a genuinely meaningful customization-to-consequence loop, test **two tiny authored civilizations** with distinct production paths. Ask whether a player can:

1. understand each civilization's starting possibility space;
2. predict some benefits and weaknesses from its graph;
3. discover emergent consequences during a small match;
4. make a better or simply more interesting second creation.

Do not wait for a finished, enormous technology tree to test whether “authoring a way of becoming” is intrinsically fun.

## 8. A proposed diagnostic playtest (not yet requested for implementation)

**Prompt:** “Create a worker for this mine, then see what happens.”

1. The player chooses a body, one meaningful capability, and equipment.
2. The same worker travels to a mineral-bearing boulder and tries to work.
3. Its strength, tool, stance, fatigue, and eventual cargo cause visible, comprehensible differences.
4. A short journey home tests whether those choices remain coherent beyond the mining animation.
5. The player can immediately revise and retry.

**Strongest qualitative success signal:** Without being prompted, the player wants to create another worker to answer an “I wonder what happens if…” question.

**Failure signals:** The player cannot infer why an action succeeded/failed; creation feels like cosmetic decoration or abstract sliders; the simulation produces impressive motion but no meaningful variation in play; performance or camera friction prevents observation; or a worker quietly breaks its order without an intelligible cause.

This is a **test of the design hypothesis**, not a demand for a new feature or a locked definition of fun.

## 9. Things I would explicitly *not* recommend

- **Do not discard Luis's physically driven-body direction** merely because it is difficult. It was explicitly reaffirmed and the current milestone exists for it.
- **Do not mistake more physics for more authenticity.** The payoff requires convincing, legible, internally coherent action, not maximal simulation detail at any cost.
- **Do not optimize exclusively for a high strategic-camera unit count** while ignoring the close observation pillar, or exclusively for one beautiful close-up while ignoring RTS scale.
- **Do not force every unusual civilization into a conventional balance template.** Keep creative warnings distinct from competitive hard legality rules.
- **Do not interpret an implemented prototype value as a permanent design decision.** Especially avoid silently locking body parameters, economic formulas, cargo rules, or creator controls.
- **Do not hurry multiplayer, ECS/DOTS, or elaborate autonomous-agent infrastructure** before concrete measured needs justify them.
- **Do not regard independently prompted AI reviewers as equivalent to independent human playtesting.** They can catch mistakes, especially with clear traces and evidence, but may share blind spots or judge still images differently from play.
- **Do not make refactoring, documentation ceremony, or speculative future-proofing the new center of development.** Keep work proportionate to observed risks.

## 10. Notes specifically for Claude and future implementation agents

**Honor the project's existing authority chain.** Luis remains Game Director; `GAME_VISION.md` and newer direct correspondence encode creative intent. `CURRENT_STATE.md` and the active plan encode stage state. This review is a dated, external *opinion*. If an item here seems useful, propose it; don't quietly turn it into implementation work.

**For every milestone, report separate kinds of evidence:**

- **What was built** (paths/behavior and what remains provisional).
- **What ran** (specific tests, builds, captures, benchmarks, dates, hardware where relevant).
- **What was observed by humans** (and by whom).
- **What is still untested or failed intermittently.**
- **Which design question was actually answered.**

**Continue preserving causes and meaning.** Good next-step reporting can connect an implementation detail to why it matters: e.g., “the body can recover from a nudge, *and the recovery reads as a believable shift at playable camera distance*,” rather than just “the joint tests pass.”

**Use independent criticism responsibly.** Fresh agent reviewers can inspect plans, numerical traces, unexpected regressions, and visuals. For material changes to save semantics, stable blueprint identity, civilization-graph rules, or eventual network authority, consider an independent technical reviewer. For aesthetics and play feel, Luis and external human playtests still matter.

**Keep the showcase attached to the game.** New equipment and body behavior should attach to reusable blueprints and command/action contracts when reasonable, not become an isolated cinematic rig that cannot exist in a player-authored civilization.

**Preserve document status.** If this file is stored in `Docs/Reviews/`, link it from an index only as a *historical outside review*. Do not copy its recommendations into the roadmap unless Luis approves. Do not silently modify existing game files while archiving it.

## 11. Relevant inspected source material

**Vision and authority**

- [`AGENTS.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/AGENTS.md)
- [`Docs/GAME_VISION.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/GAME_VISION.md)
- [`Docs/DESIGN_RATIONALE.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/DESIGN_RATIONALE.md)
- [`Docs/ArtDirection/VisualSoul.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/ArtDirection/VisualSoul.md)
- [`Docs/Design/WorkerShowcaseVision.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/Design/WorkerShowcaseVision.md)

**State, implementation history, and validation**

- [`Docs/CURRENT_STATE.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/CURRENT_STATE.md)
- [`Docs/NextMilestonePlan.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/NextMilestonePlan.md)
- [`Docs/Validation.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/Validation.md)
- [`Docs/Reviews/2026-10-10_StandingWithLife_Judges.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/Reviews/2026-10-10_StandingWithLife_Judges.md)
- [`Docs/ArtDirection/MotionJudging.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/ArtDirection/MotionJudging.md)
- [`Docs/Technical/EquipmentArchitecture.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/Technical/EquipmentArchitecture.md)
- [`Docs/Plans/S4_CarryingAndEquipment_Proposal.md`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Docs/Plans/S4_CarryingAndEquipment_Proposal.md)

**Representative code**

- [`CivilizationValidator.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Civilizations/CivilizationValidator.cs)
- [`FactionStore.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Creator/FactionStore.cs)
- [`FactionRecord.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Creator/FactionRecord.cs)
- [`IUnitCommand.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Commands/IUnitCommand.cs)
- [`OwnBody.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Units/OwnBody.cs)
- [`ProceduralBiped.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Units/ProceduralBiped.cs)
- [`PhysicalBalance.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Units/PhysicalBalance.cs)
- [`PhysicalFall.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Units/PhysicalFall.cs)
- [`Boulder.cs`](https://github.com/MacquePanoramix/Modular-RTS/blob/main/Assets/_WonderGather/Scripts/Units/Boulder.cs)

---

## Closing perspective

The most impressive aspect of Wonder Gather is not its quantity of source code, complexity of physics, or meticulous documentation on their own. It is the consistent desire for **a player-designed possibility to acquire a believable life**. That is a potentially powerful identity, but it still needs direct proof through accessible, enjoyable play.

If the game succeeds, players may not remember the solver that held a miner upright or the formula that priced a production chain. They may remember *creating* a strange little civilization, watching it discover what it could and could not do, and feeling affection for the world that resulted.

**Preserve the connection: authorship → coherent simulation → legible consequence → curiosity → new authorship.**

*End of independent review. Advisory only; no implementation changes or approvals implied.*
