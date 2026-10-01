> Historical milestone plan, archived October 1, 2026. Checkpoint A (the grounded
> body) was implemented, and Luis judged it much better. His arrival-shuffle note
> is fixed. On October 1 he clarified the showcase's scope: a single worker with a
> character creator, real hauling options and Blender-quality models. Checkpoints
> B and C carry forward as stages S3 and S4 of [ShowcaseRoadmap.md](../ShowcaseRoadmap.md),
> with D5 revised: no back-stow, and leaving the pickaxe behind is not the
> default. D1–D4 still hold. The current proposal is in
> [NextMilestonePlan.md](../NextMilestonePlan.md).

# Proposed implementation plan — Strength and Burden

**Proposed:** September 30, 2026.
**Baseline:** 6131598 (The Equipped Worker), Unity 6000.6.0f1 / URP.
**Status:** Approved by Luis on September 30, 2026, with decisions D1–D5
chosen as recommended (see [Decisions](#decisions-for-luis)). D6–D9 proceed
on their recommendations until Luis says otherwise. **Checkpoint A is implemented
and technically validated. It awaits Luis's playtest**
([GroundedBodyPlaytest.md](../GroundedBodyPlaytest.md)); B and C follow his review. The completed previous plan is archived in
[Plans/EquippedWorker.md](EquippedWorker.md).

## Where this starts

Luis reviewed The Equipped Worker on September 30. He accepted it as the bare
starting point for the [one-worker showcase](../WorkerShowcaseVision.md), with
three problems:

- the strike "looks just like an animation";
- the pickaxe clips the worker's body;
- the feet still look goofy.

He also restated the direction: movement should be procedurally animated,
grounded, and without goofiness. His original brief is preserved verbatim in
[Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md](../Correspondence/2026-09-28_ONE_WORKER_SHOWCASE_BRIEF.md).

Strength and burden show up almost entirely through motion: how a worker walks
under a load, holds or drags a tool, lifts it and swings it. Adding handling
modes on top of the current motion would repeat the three problems Luis named
in every new mode. This plan therefore first makes the body's motion follow
from weight and effort. Strength then chooses among handling modes using that
same model.

## The question

Can a worker's strength, its tool's weight and its load visibly and credibly
determine how it walks, holds, drags and swings, with motion generated from
those physical relationships rather than from fixed curves?

## Why the current motion looks the way it does

These observations come from reading the source. They are not new runtime
measurements. Where a figure is computed from source constants, it says so.

| Observation | Evidence under Assets/_WonderGather | Consequence |
|---|---|---|
| The worker moves faster than its legs can walk | The Living Worker agent speed is 3.2 m/s (Editor/LivingWorkerSetup.cs). ProceduralBiped poses the pelvis about 1.43 m high, with 1.33 m leg reach. Computed Froude number v²/(g·L) ≈ 0.73; walking bipeds normally switch to running near 0.5. | The body is asked to walk at running speed. The comparison bodies Luis judged "okay" on September 24 moved at 1.8 and 2.5 m/s. |
| Steps are reactive, short and very frequent | A step begins when a foot is 0.16 m from its home position. Step lead is capped at 0.55 m. Step duration for a worker is 0.576 / speed, about 0.18 s at 3.2 m/s. Only one foot swings at a time, with no flight phase (ProceduralBiped.BeginStep / CurrentStepDuration). | Computed estimate: roughly five steps per second, about twice a human running cadence. This reads as shuffling. |
| Feet and hips do not transfer weight | Feet are rigid 0.44 m boxes on a sine arc, kept parallel to the ground. There is no heel strike, roll or toe-off. The pelvis bobs 2.5 cm, with no side sway or shoulder counter-rotation (ProceduralBiped.Pose). | Walking lacks the loading and unloading that makes a body look grounded. |
| The strike is a stored curve | EquippedTool.SwingAngle interpolates 20° → −52° → 88° with smoothstep. PoseAt rotates the tool about a hand point fixed 24 cm in front of the pelvis. The body adds only a 4° pitch. | Every swing is identical, the hands never rise, and weight plays no part. It is a keyframe animation written as code. |
| The tool passes through the body | Computed from the authored grips/head: at the −52° windup, the pickaxe head lands at about hips + (0, 0.64, −0.19). The worker's head is posed at hips + (0, 0.64, 0). At rest, the shaft butt sits inside the pelvis/thigh volume. No body volume constrains the tool path. | This is the clipping Luis saw. |
| Navigation owns speed; the body only presents it | UnitMotor.SetMovementRate multiplies the prefab's agent speed by the creator's Movement %. Nothing about body, tool or cargo can change speed. | Burden cannot change movement without a deliberate authority change, limited to speed limits. |
| Nothing has mass | ToolDefinition has grips, head and radius, but no mass. Cargo is an integer count. UnitPerformance has no strength. | Every capability in this plan needs new data. |

The contact contract from The Equipped Worker is sound, and this plan keeps it:

- one gameplay-owned attempt;
- the displayed trajectory is the one that gets swept;
- at most one extraction per attempt;
- misses and interruptions give nothing.

## Recommended approach: physics-informed, not physics-driven

Navigation keeps the root path, and the body stays kinematic and procedural.
What changes is where its motion comes from. Hand-authored curves are replaced
by motion computed from a small physical model:

- a mass for each body segment, giving a center of mass;
- support from planted feet;
- the tool's mass and inertia;
- strength as a limit on force and torque;
- ground contact for a dragged tool.

Heavier tools and weaker bodies then produce different motion by themselves:
lower windups, slower drives, deeper stances and heavier gaits.

**Why not an active ragdoll**, meaning physics-driven joints? That is the most
direct route to the TABS-like wobble Luis wants to avoid. It is hard to keep
responsive to RTS orders and costly at scale, and AGENTS.md defers active
ragdolls unless a milestone requests them. A physics-informed body can still
show emergent weight while staying controllable, readable and testable. If
Luis wants to try physics-driven joints, that should be a separate labelled
experiment, not something hidden inside this milestone (decision D1).

No new packages are needed. The existing custom solver in ProceduralBiped is
extended rather than replaced with a third-party rig.

## Scope: three checkpoints

Each checkpoint ends with a build and a playtest by Luis. The next checkpoint
starts only after his review, so a wrong direction is caught early.

### Checkpoint A — A grounded body

Goal: remove the goofiness from walking, and give the body what it needs to
show effort.

1. **Gait from the body.** Stride length and cadence come from leg length and
   speed, on a shared gait cycle with double-support periods. Above the
   walk–run threshold the body jogs, with a short flight phase, instead of
   shuffling. How fast the worker moves by default is decision D2.
2. **Feet that roll.** Each foot gets a heel and a toe, or a toe pivot, so it
   strikes, rolls and pushes off. Ankles follow the terrain.
3. **Weight transfer.** The pelvis rises, falls and shifts over the stance foot.
   The torso and shoulders counter-rotate. Arms swing from the shoulder with
   natural lag.
4. **A spine.** A pelvis–chest–neck chain that can bend and twist. The swing
   and the carrying poses both need it.
5. **Mass and balance.** Authored segment masses give a center of mass.
   Posture leans to keep the combined center of mass of body, tool and cargo
   over the feet. Without a load this effect is subtle.
6. **Body volume.** Capsules for the head, torso, pelvis and limbs keep tools
   and cargo outside the body.

Luis judges walking at measured and brisk pace, close up and from RTS height,
in the Living Body comparison scene and the faction playtest. Orders,
navigation and planted-foot behavior must not regress.

### Checkpoint B — Effort-driven tool use

Goal: the strike is produced by a body moving a mass, not replayed from a
stored curve.

1. **A planned swing.** It has lift, windup, drive, impact and recovery phases,
   planned as targets for the tool head and hands. Arm reach and body volume
   constrain it. The windup rises over the shoulder instead of pivoting
   through the head.
2. **Simple dynamics for the drive.** Tool inertia, gravity and a drive torque
   limited by strength set the head's speed. A heavier tool or weaker body
   lifts less high and swings more slowly.
3. **The whole body takes part.** Weight shifts to the lead foot, knees flex,
   and the torso bends and twists. At impact the tool stops at the actual
   obstruction and rebounds according to its speed.
4. **Variation from state, not noise.** Stance, the previous recovery and how
   high the tool was lifted change each swing. Nothing is randomized for its
   own sake.
5. **The contact contract stays.** The swept path is the displayed path, and
   there is one extraction per attempt at most.
6. **Impact is measured.** Head speed and energy at each accepted strike are
   recorded and shown in the worker HUD. Yield stays fixed at one unit per
   valid strike (decision D6).

Luis judges whether the strike looks like effort rather than an animation, and
whether the tool stays out of the body. All Equipped Worker contact tests must
keep passing.

### Checkpoint C — Strength decides handling

Goal: Luis's examples emerge from comparing capability with demands:

- one-handed carry;
- dragging, which changes movement;
- being unable to manage without help.

1. **Strength and tool weight become data.** Each unit blueprint gets a
   Strength value in the creator (D3). Tool definitions gain mass and a
   center of mass. Three authored pickaxes (light, standard and heavy) make
   the differences easy to compare (D4).
2. **A capability resolver.** It compares strength with each demand and
   decides how the tool is transported: in one hand, in two hands or on the
   shoulder, dragged, or not movable without an aid. Separately, it decides
   whether the worker can swing it fully, only reduced, or not at all (D7).
3. **Movement consequences.** Carried and dragged loads change top speed,
   stride, lean and turning. A dragged pickaxe's head touches and slides along
   the ground, adding resistance and an asymmetric gait.
4. **Cargo has weight.** Each carried unit of material has mass that affects
   gait. How the tool and cargo share the hands is decision D5. The capacity
   count stays a separate limit for now (D8).
5. **Readable inability.** Impossible orders are refused or stopped with a
   precise reason, for example "Heavy pick is too heavy to lift — needs a
   transport aid". The creator's unit panel shows the resolved handling before
   the playtest, not only after.
6. **Side-by-side comparison.** An equipment test layout lets three workers
   with different strength take the same order at once.

Luis judges whether each worker's limits are visible and understandable
without reading numbers, and whether dragging and heavy carrying look
credible rather than goofy.

### Deliberately outside this milestone

These wait for Luis's review of this milestone:

- carts and bags (the next proposed milestone, Materials and Transport Aids);
- loose ore physics;
- fatigue and injury;
- an impact-based yield rule;
- personality;
- an editor for body proportions or anatomy;
- final art and sound;
- networking.

## Authority and contracts

- **Root and path.** Navigation keeps the root and path. New: the body's
  capability model supplies the top speed, acceleration and turn rate that
  UnitMotor applies. It has one owner and is recomputed when load or handling
  mode changes. How Movement % relates to that pace is decision D2.
- **Action phases.** Gameplay keeps owning action phases, through
  EquippedTool's attempt identity. The body poses in one explicit order:
  support → balance → spine → tool → arms. This is the same principle as now,
  with more stages.
- **Handling mode.** One component owns it, derived from the blueprint,
  equipment and cargo. Presentation and HUD read it but never decide it.
- **Resources.** Gatherer and ResourceNode remain the authority for accounting.
  Measured impact energy is reported; it does not grant material.
- **Existing values keep their meaning.** Strength does not silently change
  them: capacity stays a count, and Gathering % still schedules attempts.

## Compatibility and saves

- **Save version 6.** Faction saves add Strength per unit and may add new tool
  IDs. Versions 1–5 open with a default strength chosen so the standard
  pickaxe keeps today's two-handed, usable behavior. Rules unchanged from
  earlier milestones:
  - reading never rewrites a file;
  - an explicit save upgrades it, keeping a backup;
  - unknown data is refused, never replaced.
  The existing `pickaxe` ID keeps its meaning as the standard weight.
- **Shared body.** Checkpoint A changes the shared Living Worker body, so the
  faction playtest and TheLivingBody scenes change too. If D2 slows the default
  pace, economy timing in the supplies map also changes. That is a gameplay
  change to confirm, not a side effect.
- **Independent review.** Version 6 changes what a saved faction means, so its
  migration gets an independent review before it is trusted, following
  PROJECT_CULTURE.md.

## Required evidence

- **Checkpoint A:**
  - automated checks that stance feet stay planted;
  - stride and cadence stay within body-derived bounds across the supported
    speed range, and walk/jog selection happens at the expected threshold;
  - no pelvis overreach;
  - the existing 85-test PlayMode suite still passes;
  - reviewed rendered frames and a short captured sequence.
- **Checkpoint B:**
  - the tool never enters body capsules at sampled points across every
    swing phase;
  - all Equipped Worker contact and deduplication tests pass;
  - peak head speed decreases as tool mass increases or strength decreases;
  - impact values are reported.
- **Checkpoint C:**
  - resolver tables for strength × tool × cargo;
  - dragging produces ground contact and reduced speed;
  - inability is explained and never grants work;
  - starting and produced units agree;
  - version-6 migration tests: defaults for versions 1–5, unknown data
    refused, reading without rewriting, and backup on upgrade.
- **Every checkpoint:** a Windows build. Tests cannot settle feel, so each
  checkpoint returns to Luis.

## Decisions for Luis

**Chosen September 30:**

- **D1:** physics-informed kinematic body.
- **D2:** natural walk, with a jog at higher Movement %.
- **D3/D4:** one Strength value with named bands, and authored light,
  standard and heavy pickaxes.
- **D5:** carry the tool in the free hand when strong enough; otherwise lean
  it at the worksite and collect it on return.

D6–D9 are not yet answered and proceed on their recommendations.

**Scope refinement made at the start of Checkpoint A:**

- Load-dependent balance moves to Checkpoint C, because nothing has mass
  before then.
- Body collision volumes move to Checkpoint B, where the tool path needs them.

Checkpoint A covers the gait, feet, weight transfer, spine, head and default
pace.

Recommendations are marked below.

- **D1 — Approach.** Physics-informed kinematic body *(recommended)*, or a
  separate active-ragdoll experiment first.
- **D2 — Pace.** For the current proportions, a natural walk is roughly
  1.8–1.9 m/s (computed); the worker moves at 3.2 m/s today.
  - (a) Slow the default worker to a natural walk, with higher Movement %
    becoming a jog *(recommended: it fixes the shuffling at its root and fits
    "slowness is attention")*.
  - (b) Keep 3.2 m/s and animate it as a jog everywhere.
- **D3 — Strength in the creator.** One Strength value with named bands
  *(recommended for now)*, separate arm/grip/leg strengths, or strength
  derived later from a body-build editor.
- **D4 — Tool weight.** Authored light, standard and heavy pickaxes whose
  shape matches their weight *(recommended)*, or a free weight slider.
- **D5 — Hands while hauling stones.**
  - Keep the back strap.
  - Carry the tool in the free hand when strong enough, otherwise lean it at
    the worksite and collect it on return *(recommended: the tool stays in
    the world)*.
  - Something else.
- **D6 — Yield.** Measure impact energy only *(recommended)*, or start
  experimenting with energy-based yield now.
- **D7 — Moving a tool vs using it.** Recommended: a worker who can only drag
  a tool may still make reduced, gravity-assisted strikes above a threshold.
  Below the threshold it can bring the tool but not use it, which is the
  future cart case.
- **D8 — Capacity.** Keep the count limit for now *(recommended)*, or let
  strength and stone mass decide how many stones fit. That is natural, but
  it overlaps with the bags and carts milestone.
- **D9 — Body look while judging motion.** Keep primitive segments, adding
  heel/toe and spine parts *(recommended)*, or switch now to a neutral skinned
  mannequin.

## After this milestone

The recommended next step is **Materials and Transport Aids**: stones with
mass and bulk, a bag and a small wooden cart with real loading, hauling and
unloading, including a cart for a worker who cannot carry its tool. After
that comes polishing the one-worker showcase for outside testers. The order
stays adjustable through Luis's feedback. Three Temperaments remains part of
the wider direction.
