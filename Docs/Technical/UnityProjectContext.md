# Unity project context

Updated September 30, 2026 after Luis's Equipped Worker review (source analysis only).
Current playtest: Docs/Playtests/EquippedWorkerPlaytest.md; evidence: Docs/Validation.md.
The creator offers supplies and equipment maps; faction saves are version 5.
Sections describe successive extensions; later sections supersede earlier limits.

## Confirmed foundation

- Unity 6000.6.0f1, installed under Program Files/Unity/Hub/Editor. The project was migrated from its initial Unity 6000.3.12f1 baseline while still at the first prototype stage.
- Assets, Packages and ProjectSettings form a new project under this repository root.
- Universal 3D template foundation migrated to Unity 6.6. GraphicsSettings references its URP asset; QualitySettings preserves the template pipeline choices through Unity's serialized-settings upgrade.
- Input System enabled (`activeInputHandler: 1`); runtime owns one programmatic Gameplay action map.
- Package pins after the Unity 6.6 migration: AI Navigation 2.0.14, Input System 1.20.0, URP 17.6.0, Test Framework 1.8.0, uGUI 2.6.0, and Visual Studio integration 2.0.26. Unity 6.6 also added its required Physics Core 2D, TetGen, and Timeline Foundation modules. The resolved lockfile is authoritative after import. The original 6.3 setup required an Input System update from the template's incompatible 1.12.0 package; that historical issue remains resolved.
- No callable Unity MCP provider was exposed in this task. Local Unity batch execution is available.
- The repository was created for this project in the initial setup task.

## Code and ownership

All first-party runtime code is in Assets/_WonderGather/Scripts, namespace WonderGather, assembly WonderGather.Runtime. Unity.InputSystem, Unity.AI.Navigation and Unity.ugui are explicit assembly references.

RtsInput owns actions, including press/release/hold and Shift state. SelectionController owns the selection list and drag state, with an explicitly serialized roster for box selection. GroupMoveCommand plans spaced destinations and validates all paths before applying any. UnitMotor separates path planning from execution. MoveCommand still supports individual orders. RtsCamera focuses the selection center; SelectableUnit owns its ring. WandererHud draws the drag box, selected count, and order feedback.

Editor-only WonderGather.Editor contains scene creation, preview and Windows build entry points. Tests live in their own PlayMode test assembly. Gatherer owns a small work state machine and cargo; ResourceNode owns remaining supplies and ResourceDepot owns stored supplies. GatherCommand and ReturnSuppliesCommand enter through CommandDispatcher. Networking is not implemented. Later extensions below document faction persistence and procedural movement presentation.

## Startup and assets

WandererSetup.Create creates the original TheWanderer scene, Wanderer prefab, materials and baked NavMesh. GroupSetup.Create copies that scene into TheGroup, places eight prefab instances, wires the selection roster, and configures high-quality NavMesh avoidance with varied priorities. It refuses to overwrite an existing Group scene. TheGroup is the first enabled build scene; TheWanderer remains enabled for regression tests. GroupSetup.BuildWindows builds only TheGroup into Builds/WindowsGroup. Layer 6 is Walkable and distinguishes commandable ground from obstacles and units. The camera world mask includes Default and Walkable.

For future runtime spawning, extend roster ownership explicitly; current box selection covers the scene's configured friendly units. Formations are destination grids, not rigid travel constraints. ReachableDestination searches triangle candidates by distance and validates complete routes, excluding disconnected islands. GroupMoveCommand adjusts overlapping projected slots with a bounded nearby search; orders that cannot fit still preserve previous destinations. Strict UnitMotor.TryPlanMove remains available for interactions that must actually reach their target.

Use private serialized fields, PascalCase types/methods, explicit dependency wiring and small components. New scene creation must not overwrite an existing scene. Preserve Unity-generated metadata and the package lock in Git.

## Validation and limits

Read Docs/Validation.md for final execution results. PlayMode tests cover routing around the obstacle wall, rejection of disconnected/out-of-range/nonfinite destinations, and selection disable lifecycle. Manual checks are in Docs/Playtests/WandererPlaytest.md. Desktop mouse/keyboard and flat terrain are the current scope. Camera feel and art direction require user playtesting.

Source evidence: ProjectSettings/ProjectVersion.txt, GraphicsSettings.asset, QualitySettings.asset, ProjectSettings.asset, Packages/manifest.json, first-party runtime/editor/test sources and recovered conversation text. The referenced PDF dossier was not available as file contents.

GathererSetup.Create copies TheGroup into TheGatherer, adds one finite supply node, one depot and eight worker components, and preserves the 0.005 zoom preference. It refuses to overwrite the scene. TheGatherer is first in build settings; previous scenes remain for regressions. GathererSetup.BuildWindows builds only TheGatherer into Builds/WindowsGatherer. Interaction offsets are serialized per worker and no runtime scene search is used. New materials are placeholders.


## Construction extension

SettlementSetup.Create copies TheGatherer into TheSettlement without overwriting earlier scenes, adds Builder components, and wires ConstructionController on Player Controls. It authors Workshop.asset, a Workshop prefab with a carving NavMeshObstacle, and preview materials. TheSettlement is first in build settings; previous test scenes remain. SettlementSetup.BuildWindows builds only TheSettlement into Builds/WindowsSettlement.

BuildingDefinition owns provisional cost, duration and footprint. ConstructionController owns preview/placement and validates before ResourceDepot.TrySpend. BuildCommand enters through CommandDispatcher. Builder owns the active site assignment and travel/work state; BuildingSite owns persistent session progress and one-worker reservation. Successful move/gather/delivery orders release the builder's site. B enters placement through the existing RtsInput action map; SelectionController routes placement before normal selection input and right-click sites to resumption. No new packages or runtime scene searches were added. Current placement assumes flat terrain.


## Production extension

ProductionSetup.Create creates TheProduction from TheSettlement, with a ProductionWorkshop prefab variant and ProducedWorker prefab variant; older scenes and base prefabs stay unchanged. WorkerProductionDefinition owns the prefab, cost and duration. Each UnitProducer owns its queue, timer, exit search and stored-supply payments/refunds. ConstructionController supplies the depot, roster and reachable work position when placing a production workshop. ProductionCommand enters through CommandDispatcher.

SelectionController now distinguishes a selected building from selected units and owns explicit RegisterUnit/UnregisterUnit and worker-slot allocation. ProducedWorker registers on initialization and unregisters on destruction. New workers get the depot and distinct work offsets at spawn. Exit search checks physical space, matching NavMesh agent settings and connectivity to the workshop approach. No runtime scene search is used. T and HUD buttons queue workers; cancellation removes the last order. TheProduction is the first build scene; ProductionSetup.BuildWindows builds it into Builds/WindowsProduction.


## Civilization foundation

CivilizationDefinition owns a starting base, starting supply count, counted
starting units and explicit unit/building rosters. UnitBlueprint references
WorkerProductionDefinition, optional supply gathering and construction links.
BuildingBlueprint references a BuildingSite prefab (whose BuildingDefinition
is authoritative) and one optional produced UnitBlueprint. Stable IDs must be
unique across both rosters. Blueprint data remains immutable during runtime.

CivilizationSession starts TheCivilization and TheProvisionedCivilization from
these assets after structural validation and initial spawn-space checks. It
wires the starting depot, selection roster, HUD, construction and workers
explicitly. UnitIdentity carries unit data; Gatherer and Builder enforce its
permissions. ConstructionController offers the union of selected units' build
links and chooses a permitted builder. UnitProducer applies the produced
blueprint before registering the new worker. Earlier scenes without identities
retain their established behavior.

CivilizationValidator computes structural reachability by fixed point and
issues supply/bootstrap warnings. Broken references/component contracts/IDs
are errors; unreachable and resource-limited designs are warnings. It does not
solve cumulative affordability, finite resources, survivability, or map paths.
CivilizationInspector exposes the report and textual links beneath normal
asset editing. No additional packages, graph-editor framework, runtime scene
search, save format, research system or design-cost formula were added.

CivilizationSetup.Create authors two new scenes and sample assets through
Unity; it refuses to overwrite existing scenes. BuildWindows builds both
scenes into Builds/WindowsCivilization with the standard sample first. Existing
scenes stay enabled for regressions. No changes to older authored scenes or
the 0.005 zoom preference are required by this milestone.


## Player-facing creator

FactionDraft owns cloned CivilizationDefinition, UnitBlueprint and
BuildingBlueprint instances with remapped links; prefab and cost recipe
references remain read-only. It constrains the small demo's editable values
and refreshes CivilizationValidator after edits. It destroys only owned copies.

FactionCreator owns the draft and additive map load/unload state. The creator
scene remains loaded while FactionPlaytest is added. It locates the explicitly
authored FactionPlaytestBridge only within that loaded scene, makes that scene
active before spawning and calls CivilizationSession.InitializeFrom. The new
map disables automatic session initialization; older scenes retain it. Returning
unloads the entire map before exposing the draft again. No static singleton or
cross-session persistence is introduced.

FactionCreatorView draws the fixed-card graph, starting setup and contextual
details using the existing IMGUI approach, scaled to a 1280×720 reference.
Details and warnings have scroll areas. RtsInput accepts an explicit pointer
blocker so the in-game return button does not issue world orders beneath it.
Keyboard movement is absent in the creator because the map is unloaded.

FactionCreatorSetup authors TheFactionCreator and FactionPlaytest without
overwriting earlier scenes; BuildWindows builds those two scenes into
Builds/WindowsFactionCreator. Earlier scenes stay in build settings for tests.
No package changes or modifications to existing scene assets are required.
The creator's sample contract is intentionally one worker and one workshop.
Do not imply arbitrary content editing, save/load or a final visual design.


## Faction persistence

FactionWorkspace now owns the runtime draft, clean-state snapshot, current file
ID and optimistic version token. FactionCreator delegates draft access to it;
the prior creator/playtest API remains intact. Explicit storage injection lets
tests use unique temporary directories without touching player saves.

FactionRecord version 1 is a strict flat DTO for the existing settlement-1
template. Stable blueprint IDs and all current editable fields are written
with Utf8JsonWriter and read with JsonDocument (Unity 6.6's bundled BCL
System.Text.Json; no added package). Unknown, duplicate, missing or unsupported
fields/versions and invalid bounds are refused. No Unity object serialization
or reflection-based JSON serialization is used.

FactionStore uses Application.persistentDataPath/Factions in normal play.
Generated GUID filenames prevent display names becoming paths. Small files
are written on explicit user actions: a flushed temporary file replaces the
destination atomically, retaining one .bak. A short-lived exclusive lock
serializes cooperative writers; a content hash rejects stale updates.
Deletion moves the saved file into Deleted. Reads validate before the workspace
replaces/disposes its current draft; failed operations retain the current data.

FactionLibraryPanel owns transient library, naming and confirmation views.
The existing scaled creator view supplies toolbar and save-state feedback.
Standalone close requests use Application.wantsToQuit with save/discard/cancel;
the map's input is disabled while a quit prompt is active. Editor Stop Play
Mode does not participate in that player lifecycle. Prior scenes and packages
are unchanged; the existing creator build command includes the new features.


## Multiple unit blueprint extension

FactionDraft now owns a bounded unit list and per-unit starting counts. Existing
Worker/StartingWorkers/SetStartingSetup methods remain compatibility accessors
for the first unit; TotalStartingUnits describes the faction-wide count.
SetFactionSetup changes name/supplies without touching the mixed starting roster.
AddUnit uses the original read-only worker recipe; duplicates copy permissions,
receive new stable IDs and zero starting count. RemoveUnit releases its owned
copy, removes starting entries and clears the matching workshop link. The
creator UI asks for confirmation before removal. No authored assets change.

The current fixed-card view is extended with a scrollable blueprint list,
per-unit details and a workshop target picker. Each building still produces
one unit type through the existing runtime UnitProducer. The existing session
already spawns mixed starting entries and applies UnitIdentity. The playtest
HUD now names a single selected unit's blueprint. No scene/prefab/package
changes are required; TheFactionCreator remains the entry scene.

FactionRecord writes strict version-2 records with a unit array and trained ID.
The worker field identifies the supported recipe template, independently of
user-defined roster IDs (including after removal of the original worker).
Version 1 is strictly checked and migrated in memory to one named Worker.
Reads never rewrite; an explicit save/rename upgrades with the existing backup.
Unknown nested fields/versions, duplicate IDs and dangling links are rejected.
Workspace dirty tracking includes every unit and relationship using a length-
prefixed snapshot that can represent temporarily invalid names during editing.

The final prototype bounds are 1–8 unit blueprints and 0–8 total starting units.
All use the existing worker prefab and recipe, with optional gather/build.
Multiple production options per building and building roster editing remain
future work. See MultipleBlueprintPlaytest.md and Validation.md for evidence.


## Building roster and mixed production extension

FactionDraft owns up to eight workshop-derived BuildingBlueprint copies plus
the fixed starting base. Building names live on the blueprint, leaving shared
BuildingDefinition recipes immutable. Runtime buildings expose DisplayName
through BuildingSite; placement and selection HUDs use that name.
AddBuilding duplicates outgoing training options only. SetBuildPermission and
SetTraining validate roster ownership. Removal cleans all affected links before
destroying owned copies. Original Worker/Workshop convenience APIs remain for
earlier one-type tests; template IDs are tracked separately from editable IDs.

BuildingBlueprint retains its serialized produces reference as the first option
and adds an additionalProduction array, with safe empty defaults for existing
assets. ConfigureOptions supports multiple types; legacy Configure remains.
CivilizationValidator and the Inspector follow every production option and
reject missing/duplicate/out-of-roster options. Existing scenes require no edits.

UnitProducer snapshots each enqueued blueprint, prefab, paid amount and duration.
Enqueue's optional blueprint argument must belong to the producer options; null
uses the first option or legacy recipe. Cancel/destroy refunds actual queued
payments. SelectionController and ProductionCommand carry the requested type.
The HUD shows separate buttons, queue names, and bounded scrolling, retaining
world-pointer exclusion through its measured visible rectangle.

FactionRecord writes strict version 3 with unit builds-ID arrays and named
building records with ordered trains-ID arrays. Version 1 maps to one worker
and workshop; version 2 maps to multiple workers and one workshop. Both migrate
only in memory until explicit save/rename, retaining the old file as backup.
Nested unknown fields and duplicate/dangling links are refused. Workspace dirty
tracking includes complete rosters, names and links. No package updates needed.

Current playtest: Docs/Playtests/BuildingNetworkPlaytest.md. Windows build entry remains
FactionCreatorSetup.BuildWindows and the existing creator/map scenes.


## Prototype unit performance extension

UnitPerformance is a serializable value owned by UnitBlueprint, with validated
integer rate percentages and carrying capacity. Field initialization preserves
existing authored blueprint defaults. FactionDraft clones and duplicates these
values without changing prefab/recipe assets. SetPerformance validates roster
ownership; CivilizationValidator checks authored values before session spawn.

UnitIdentity.Configure applies performance to existing UnitMotor, Gatherer and
Builder components for both starting and produced units. UnitMotor captures
the prefab speed once; Gatherer captures its original interval once. Repeated
application therefore does not compound multipliers. Builder multiplies work
time passed to BuildingSite. Permissions and commands retain their ownership.
Older scenes without identities retain their serialized settings.

FactionRecord writes strict version 4 with a nested performance object for each
unit. Versions 1–3 default to the original worker values without rewriting;
explicit saves retain backups. All four values participate in dirty tracking.
The creator detail panel scrolls to the controls and reset button; the selected
unit HUD shows the blueprint settings. The blueprint is immutable during a
playtest; changing live cargo capacity mid-order is not an exposed player flow.

These are temporary outcome controls. No body system, pricing, new packages,
scene/prefab changes or final customization choices are part of this slice.
Current guide: Docs/Playtests/UnitPerformancePlaytest.md.


## Living Body procedural locomotion experiment

LivingBodySetup.Create authors TheLivingBody, one reusable LivingBodyBiped
prefab, provisional materials, ramp mesh and baked navigation via Unity APIs.
It refuses existing scene/assets and appends its build entry without changing
earlier entries. BuildWindows builds only this scene into Builds/WindowsLivingBody.
Existing creator assets, production recipes and faction schema 4 are unchanged.

Two instances share geometry/solver with measured and brisk navigation speed
(1.8/2.5), acceleration 3, and different step reach/lift/duration. These are
comparison settings, not a species, finalized proportions or player body editor.
The course has flat ground, an 11-degree ramp (2m rise over10m) and a plateau.

ProceduralBiped poses only visual transforms in LateUpdate. UnitMotor and
NavMeshAgent remain authoritative for movement/turning. Foot support state is
world-space, one foot swings at a time, and ground sampling uses Walkable layer 6
at initialization/new steps. A two-segment leg solve, body height adaptation,
acceleration lean and arm swing visualize actual root displacement. Re-enable
and large root relocations reinitialize nearby support without moving the root.
This is kinematic presentation, not simulated balance forces or active ragdolls.

LivingBodyDemo owns explicit units/route targets and an input-blocking scrollable
HUD. Route buttons use existing movement commands; direct selection/box orders
remain available. The scene opts into RtsCamera.terrainAware for ground-relative
focus height and camera clearance. The new serialized option defaults off in
earlier scenes, preserving their camera behavior and authored 0.005 zoom.

The supported test is a simple biped on static flat/gentle terrain. Stairs,
jumps, moving platforms, arbitrary body topology, physical work/combat and
RTS-scale animation performance have not been established. Movement feel and
aesthetic acceptance remain the user's review.

Final presentation refinement: swing endpoints predict where the root will be
at landing, the rest stance is upright, pelvis lowering obeys leg reach, and
arm counter-swing follows the actual foot position. The explicitly referenced
selection ring aligns with support normals to remain visible on the ramp.
The two-unit checks establish this prototype only, not arbitrary body support.


## Living Worker integration

September 26, 2026: approved scope, implemented and technically validated.
Luis provisionally accepted the early prototype on September 28. This section
supersedes the earlier separation between faction workers and procedural bodies. Current guide: Docs/Playtests/LivingWorkerPlaytest.md;
execution evidence and remaining limits belong in Docs/Validation.md.

LivingWorkerSetup.Create uses Unity authoring APIs to make LivingWorker.prefab
from the existing biped, add Gatherer/Builder/ProducedWorker, and attach a
provisional bundle through ProceduralBiped.ConfigureWork. A new
LivingWorkerProduction recipe copies the old recipe's economic settings and
becomes the existing Worker blueprint's production reference. Stable blueprint
IDs, editable rosters, permissions, performance meanings and save schema 4
remain unchanged. The baseline NavMesh speed stays 3.2, preserving the
creator's 0.8–6.4 range. Earlier non-civilization production keeps its recipe.

The faction map's ResourceNode and CivilizationBase prefab each receive a
ResourceWorkplace with eight authored stand/contact pairs. Stand/contact
transforms are relative to their owner. The component reserves a reachable
stand for one Gatherer, rejects paths that resolve away from that stand,
and releases occupancy when the worker leaves, cancels or disables. The map's
supply station gets a carving obstacle and visual rack; its original command
collider and ResourceNode identity remain. The base has provisional delivery
shelves. FactionPlaytest enables terrain-aware camera framing and retains
the user's authored 0.005 zoom preference.

Gatherer remains authoritative for task state, timing and cargo. It claims
work positions during travel, waits/retries when none can be reached or are
free, faces the contact through UnitMotor.Face, and exposes HasWorkContact,
WorkContact and ActionProgress to presentation. Contextual delivery adds a
0.4-second Depositing state before committing cargo to the depot. Cancellation
releases the reservation and retains carried supplies. Nodes/depots without
ResourceWorkplace keep their existing offset-based interaction and immediate
delivery, supporting the earlier prototypes.

ProceduralBiped owns all visual limb posing. An explicitly configured
Gatherer supplies read-only activity and cargo; there is no second resource
counter or animation-driven transfer. Its right arm reaches toward the work
contact during gathering/delivery; carried cargo chooses the carrying pose
and visible bundle, with smoothed arm targets in body space. Arm reach is
constrained by a two-segment solve. The configured worker uses earlier,
shorter steps and a duration derived from actual displacement to accommodate
the existing movement range without changing navigation speed. The separate
Living Body walkers retain their original step-timing path. Both paths
constrain the visual pelvis to the intersection of the two leg-reach volumes,
covering horizontal overreach after navigation corrections as well as slopes.
Feasible corrections retain planted contacts and never move the root;
mutually unreachable contacts reset presentation like a teleport.

WandererHud adds the selected worker's activity label and cargo/capacity.
Starting and produced units continue through the existing CivilizationSession
and UnitProducer paths. Builder behavior is retained, but construction has
no new gesture. There is no new input map, package, save migration, body editor,
physics authority or morale system. Resource visuals and load representation
are provisional; further body/equipment work still requires Game Director
playtesting rather than inheriting acceptance from this early slice.

The entry remains TheFactionCreator.unity, which opens FactionPlaytest through
the existing playtest flow. LivingWorkerSetup.BuildWindows builds those two
scenes to Builds/WindowsLivingWorker/WonderGather.exe. Five focused tests and
the full 67-test PlayMode suite passed; a rendered probe and Windows build also
passed. See Validation.md for the evidence and limits. Visible rack/box/marker
and shelf surfaces have command-raycast colliders, matching what can be clicked.
The original resource collider is retained for compatibility.


## September 29 equipment/contact review

The next recommendation is The Equipped Worker; see Docs/NextMilestonePlan.md
for the bounded scope and source evidence. Docs/Design/WorkerShowcaseVision.md records
Luis's deeper target. No new tool, physics, strength or save system was implemented
by this review, and Unity tests/builds were not rerun.

Critical distinction: Gatherer.HasWorkContact means ownership of an authored
work position, not a collision. Gathering still grants stock through a timer;
ProceduralBiped illustrates it in LateUpdate. Mining needs a gameplay-owned
strike identity and valid contact along the same solved tool path that is
shown, with one accepted extraction per attempt and explicit cancellation.

Equipment persistence must cover UnitBlueprint, UnitIdentity, FactionDraft
copy/restore, FactionRecord capture/encode/decode, and FactionWorkspace dirty
tracking. Version-4 capacity/rates must not become physical strength or mass
implicitly. Retain read-without-rewrite, safe unknown-data handling and the
FactionStore explicit-save backup/conflict contract when extending the schema.


## Equipped Worker integration — September 29

See Docs/Technical/EquipmentArchitecture.md for the current ownership/contact/save contracts.
ToolDefinition owns stable printable IDs (1–64 characters), prefab, rigid grips
and head point/radius. Blueprint/catalog choices flow through draft editing,
duplication, dirty tracking, v5 records and UnitIdentity. Unknown definitions
fail restoration safely; legacy1–4 defaults to None and only explicit save
upgrades with the existing conflict/backup flow.

EquippedTool owns a monotonic attempt ID and procedural phase. ProceduralBiped
solves support/torso, explicitly invokes the tool solve, then follows its grips.
The same bounded reachable trajectory is swept against physical colliders.
Every unspent segment checks origin overlap and the nearest obstruction; only
the intended mineral surface can extract. Gatherer revalidates the accepted
attempt and transfers the actual ResourceNode.Take result into cargo. Scale
changes, invalid reach, disabled components and replacement commands invalidate
work without deleting cargo. HandPosition reports rendered coordinates through
the moving root, matching parented tool/limbs between pose updates.

EquipmentPlaytest is copied/authored separately from the legacy supplies map.
The worker prefab variant receives the equipment component; the template choice
remains None. Mineral visuals/collision share one mesh and eight work ports.
Mining cargo is displayed beside the feet until transport; the tool interpolates
around the side to the back and leaves hands for the existing carry/delivery pose.
This is temporary handling, with no mass/strength/loose ore/cart simulation.

Eighteen focused and 85 full PlayMode tests, reviewed rendered frames and
EquippedWorkerSetup.BuildWindows passed. Output:
Builds/WindowsEquippedWorker/WonderGather.exe. The build contains TheFactionCreator,
FactionPlaytest and EquipmentPlaytest. Packages, old maps and user edits were
preserved. No Unity MCP provider was callable; validation used local Unity.

## September 30 motion review

Luis accepted The Equipped Worker as a foundation. He did not accept its motion:
the strike looks canned, the tool clips the body and the feet look goofy.
Source-derived causes, none measured at runtime:

- **Gait speed.** LivingWorker's agent runs at 3.2 m/s with the pelvis posed
  about 1.43 m high. The computed Froude number of about 0.73 is above the
  walk–run threshold, yet the body only walks.
- **Steps.** Worker steps trigger at 0.16 m of foot error, lead at most 0.55 m
  and last about 0.576/speed s, roughly five steps per second.
- **Feet and hips.** Feet are rigid boxes with no roll, and the pelvis bobs 2.5 cm.
- **Swing.** EquippedTool.SwingAngle is a fixed 20° → −52° → 88° curve about a
  hand point at hips + (0, −0.03, 0.24). The body adds only a 4° pitch.
- **Clipping.** At the −52° windup the pickaxe head reaches about
  hips + (0, 0.64, −0.19), coinciding with the posed head at hips + (0, 0.64, 0).
  No body volumes constrain tool paths.
- **Speed authority.** UnitMotor multiplies the prefab speed by Movement %.
  Nothing body- or load-dependent can set speed yet.

Docs/NextMilestonePlan.md proposes Strength and Burden: a physics-informed
kinematic body, not an active ragdoll, in three checkpoints. Its decisions
D1–D9 are open. Keep the EquippedTool contact contract (the swept path is the
displayed path, one extraction per attempt) and the explicit solve order when
extending the rig. The previous plan is archived at Docs/Plans/EquippedWorker.md.

## Strength and Burden, checkpoint A — grounded body

Implemented September 30. The playtest guide is Docs/Playtests/GroundedBodyPlaytest.md;
evidence is in Validation.md.

**Gait.** ProceduralBiped's reactive, threshold-triggered stepping is replaced
by a phase-based gait.

- The Froude number v²/(g·1.43) selects Walking or Jogging: jog above 0.55,
  back to walk below 0.45.
- Stride follows Alexander's relation, shortened: 2.1·h·Fr^0.3, capped at
  2·stepReach·h. Cadence is speed/stride, clamped to 0.75–1.8 strides/s.
- Duty factor is 0.64–0.56 for a walk and 0.38 for a jog.
- Feet alternate on one cycle. Swing feet retarget to land where the hips will
  pass over them at mid-stance.
- In a walk a foot can lift only while the other is planted. Only a jog is
  Airborne; FlightTime reports the continuous flight.
- Standing adjustment steps (stepDuration) settle the feet after stopping or
  turning. They start only below the gait start speed, so a moving body hands
  over to its gait. A regression test covers the earlier settle-chasing bug.
- Displacement faster than max(8, 2.5·agent speed) is treated as a
  correction, not locomotion.

**Feet and rig.**

- A planted foot keeps a fixed world-space support frame, so the
  FootPosition/FootNormal contracts are unchanged. The rendered foot pitches
  about the heel (toe up) or the ball (heel up); FootPitch reports it.
- Optional toe segments stay flat while the heel rises.
- GroundedBodySetup.Apply authored LivingBodyBiped feet at 0.13 × 0.09 × 0.26
  plus toes, and set the LivingWorker variant to 1.8 m/s speed and 4 m/s²
  acceleration. It refuses to author twice.
- SetTuning keeps its signature. stepReach now bounds step length as a fraction
  of hip height, footLift sets walking swing clearance, and stepDuration sets
  standing adjustment steps.

**Body frames and shoulders.**

- Pose separates a steady body frame (posture, used by tools and cargo) from
  the gait-oscillating hip frame and the counter-rotating chest.
- EquippedTool.SolveFrame now receives the actual shoulder positions from the
  chest, so its reach checks match the arm IK.
- The arm IK takes a bend hint: elbows point back for free arms and out/down
  while the hands carry or grip.

**Build.** GroundedBodySetup.BuildWindows builds the creator and both maps to
Builds/WindowsGroundedBody.

## Arrival without shuffling — October 1

Luis reported foot re-shuffling on arrival. ProceduralBiped changes:

- **Stopping.** A gait whose root stops enters `stopping`. Its swings finish on
  their own clock beneath the hips, and at most one closing gait step follows
  at once. It becomes Standing when both feet are within 0.15 m of home and
  turned less than 40°.
- **Resuming.** Moving again during `stopping` resumes the cycle from the
  swinging foot (`Resume`).
- **Anticipation.** Phased swing targets read the agent path (read-only). When
  the path has less than 1.5 m remaining, landings are clamped so they never
  pass the path end's stance position.
- **Standing tolerance.** Settle steps only for a home error over 0.3 m, a turn
  over 40°, or a stance narrower than 0.14 m (crossed feet).
- **Test.** GroundedBodyTests.ArrivalFinishesTheStrideWithoutShuffling.

## Station occupancy — October 1

Workers that run out of work no longer idle on station positions, which had
intermittently deadlocked shared stations at the natural pace.

- **`Gatherer.StandAside`.** Applies when delivery finds the resource
  exhausted, or the resource empties while the worker holds no cargo. If the
  worker is within 3 m of that station, it moves 1.2 m radially clear;
  otherwise it stops where it is.
- **`ResourceWorkplace.TryClaim`.** Now also skips a position whose ground
  point (plus 0.5 m, radius 0.3) overlaps another active Gatherer's collider.
  Reservations remain the authority for assignment; this only prevents
  claiming a place a body cannot reach.
- **Physics timing.** Physics queries see moved bodies after the next physics
  step, so tests that warp an agent must wait for fixed updates before
  claiming.

**Pelvis support (October 1).**

- **Target height.** `pelvisY` is the smoothed target height: lowered at once
  by the reach of planted legs, raised gradually.
- **Planted feet only.** ReachableHips projects that target onto the reach
  spheres of planted feet only; with both feet swinging it is unconstrained.
  The projected height is never written back, because doing so made the
  pelvis ratchet to the ground when both feet were beyond horizontal reach.
- **Output filter.** A rise in the solved height (`hipLift`) is smoothed as an
  output filter; lowering is immediate.
- **Swinging feet.** A swinging foot beyond the leg's reach is drawn within
  reach of the hip (its rendered foot and toe follow) rather than pulling the
  body down to it.


## Two camera modes (S1a) — October 1

- **`RtsCamera` stays the single camera rig.** It keeps its component, GUID
  and serialized preferences in every map, and gains `CameraMode.Strategy`
  and `CameraMode.Explore`. `V` toggles, and `Step(CameraIntent, dt)` is the
  testable entry point.
- **Input.** `RtsInput` remains the only owner of devices. `ReadCameraIntent()`
  fills the intent. In Explore:
  - `MovePressed` fires on right-button release when the press moved less
    than 6 px;
  - Alt + left click orbits instead of selecting;
  - `CaptureCursor` locks the cursor while looking and returns it to where
    the look began.
- **`ExploreFlight`.**
  - Eased fly and look.
  - Speed scales with clearance (`baseSpeed × (0.5 + clearance)`).
  - Orbit about the viewed point or the followed body, slide, and wheel
    glide; focus with F follows a body.
- **`CameraCollision`.** It sweeps a 6 cm sphere and slides against
  everything except living bodies (`SelectableUnit`/`NavMeshAgent`
  hierarchies), which push the camera softly via personal space. It also
  provides walkable-ground and residual-overlap guards. Explore stays inside
  the bounds of the layer-6 (Walkable) colliders.

## The Ordinary Place look test (S1b/S1c) — October 1

- **Scene.** `TheOrdinaryPlace`, authored once by `OrdinaryPlaceSetup.Create`
  (guarded). Stills come from `OrdinaryPlaceCapture.Capture`; the release
  player is built by `OrdinaryPlaceSetup.BuildWindows`.
- **Art sources.** `Art/Blender/OrdinaryPlace/house.py` and `nature.py`
  export FBX into `Assets/_WonderGather/Art/OrdinaryPlace`. Empties named
  `Light_*`, `Smoke_*` and `Blocker_*` anchor the lights, smoke and doorway
  blocker.
- **Shaders.** They live in `Assets/_WonderGather/Art/Shaders`:
  - `Wonder Gather/Painted`: surfaces with paint dabs, a brush-broken light
    edge, cool coloured shade, warm local-light pools, rim, aerial fog and
    optional wind sway;
  - `Wonder Gather/Grass`: instanced blades from a structured buffer, with
    wind and parting around bodies;
  - `Wonder Gather/Sky`;
  - `Wonder Gather/Smoke`;
  - `Hidden/Wonder Gather/Look Post`: paint filter, ink and grain.

  `WGCommon.hlsl` holds the shared noise, dabs, lighting and fog. Scene-wide
  values are `_WG_*` globals set by `TimeOfDay`. When `_WG_Sky.a` is 0, the
  surfaces fall back to URP ambient and fog.
- **Runtime components.** They live in `Scripts/Look`:
  - `OrdinaryGround`: a deterministic height field whose meshes are
    regenerated on enable as `DontSave` objects, so there are no large mesh
    assets;
  - `GrassField`: chunked `RenderMeshIndirect` in `beginCameraRendering`,
    with distance thinning by drawing a shuffled prefix;
  - `TimeOfDay`: one palette. Sky, light and glow colours are sRGB via
    `SetColor`; the globals are linear;
  - `SmokePlume`;
  - `LookPostEffects`: render-graph passes injected from
    `beginCameraRendering`, so the shared URP renderer asset is untouched;
  - `LookDevControls`: candidates A–E, time presets and the cost panel;
  - `LookBenchmark`: `-wgbenchmark`.
- **Assemblies.** `WonderGather.Runtime` now references URP Core and
  Universal runtime, for the injected passes.

## Hand-painted surfaces (S1c, second pass) — October 2

- **Baking.** `Art/Blender/OrdinaryPlace/painting.py` unwraps every face into
  its material's texture (`unwrap_by_material`). It builds a painter's node
  graph per material from `RECIPES` and bakes it with Cycles EMIT into one
  JPEG per material. Each image is pre-filled with the material's base colour
  so mipmaps never bleed black.
- **Running it.** `house.py --paint <folder>` and
  `nature.py --paint <folder>` run the pass before exporting the FBX. The
  textures live in `Assets/_WonderGather/Art/OrdinaryPlace/Textures`.
- **Applying.** `OrdinaryPlaceSetup.ApplyPaintedTextures` assigns each
  texture to its existing material in place (GUIDs unchanged). It lowers
  `_Variation` to 0.25 (wood also gets `_Brush` 0.3), sets the glow
  material's fixed emission, and starts the scene on look E.
- **Time-of-day globals.** `TimeOfDay` drives the sky and window glow
  through globals only (`_WG_Sky*`, `_WG_Sun*`, `_WG_Moon*`, `_WG_Cloud*`,
  `_WG_Stars`, `_WG_Galaxy`, `_WG_GlowScale`), so no material asset changes as
  time passes.
  - Colours are sent as `.linear`, because `Shader.SetGlobalColor` does not
    convert from sRGB the way `Material.SetColor` does.
  - The glow strength is `max(.08, houseLights)^2.2`, which reproduces scaling
    the colour before its sRGB conversion.

## The beyond and look F (S1e) — October 2, archived

This code lives only on the branch `claude/essence-exploration`. Luis preferred
the hand-painted pass, so none of it is on `claude/worker-showcase`. It is
described here so it can be revived piece by piece.

- **The far land.** `OrdinaryGround` keeps the meadow grid as it was.
  - It appends far rings from the grid's own edge. They round into circles and
    widen geometrically to 9 km (`AppendFarLand`).
  - The height field adds a lake basin (`ShoreDistance`, a union of
    ellipses), ranges (ridged noise) and one peak.
  - Nothing reaches inside the meadow's square, so the collider and the baked
    navigation are unchanged; `BeyondTests` checks this.
  - The water level is −26 m, below the meadow's bluff.
- **Clouds.** `Art/Blender/OrdinaryPlace/clouds.py` builds cumulus from
  metaball domes (towers, heaps and banks) into `Clouds.fbx`.
  - Vertex colours carry occlusion (R) and height through the cloud (G).
  - `CloudBank` places about 30 clouds deterministically (DontSave) and drifts
    them along the wind. A cloud re-forms on the far side after leaving the sky
    disc.
  - `WGCloud.shader` paints them: banded light from `_MainLightPosition`,
    shaded folds, a silver lining, a silhouette blended to the sky, and the air.
- **Water.** `WaterSurface` draws one plane and renders a planar reflection
  before each game or scene camera with
  `RenderPipeline.SubmitRenderRequest` (`SingleCameraRequest`).
  - The mirror camera sits below the water, upside down, so its rotation stays
    proper and no culling inversion is needed.
  - An oblique near plane clips everything under the water.
  - The reflection camera has `CameraType.Reflection`, so grass, smoke and the
    look passes skip it.
  - `WGWater.shader` flips the image back (`1 - v`). It also adds the depth
    tint, painted ripple streaks, wind-ruffled patches, sun glitter and a
    shoreline.
  - Far water (beyond about 700 m) ignores scene depth, whose precision is too
    coarse there.
- **The air and cloud shadows.** These live in `WGCommon.hlsl`.
  - `WG_SkyGradient` is shared by the sky, clouds, water and air.
  - `WG_AirColor` samples the sky a little above the horizon, so far shapes
    read blue.
  - `WG_AirAmount` integrates an exponential height density: `_WG_Air` holds
    the distance, maximum, haze height and base.
  - `WG_CloudShadow` projects a drifting noise field along the main light to
    900 m; the painted and grass shaders multiply it into the main light's
    shadow.
  - The `_WG_Fog*` globals other than `_WG_FogShape` (clear distance) are
    retired.
- **Look F.** `LookPostEffects.PaintingPass`:
  - structure tensor (pass 3) → soften across and down (4, 5) → anisotropic
    Kuwahara with polynomial weights (6);
  - the radius grows with depth and towards the frame's edges;
  - wide strokes sample every other pixel at half-pixel offsets, for a quarter
    of the cost.

  The grain pass also applies the hour's palette grade (`_WG_GradeShadow`,
  `_WG_GradeLight`, `_WG_GradeLift`, from `TimeOfDay`).
- **Ink only on characters.** Painted surfaces now write a kind into the
  normals texture's alpha: 0 drawn (`_Drawn`, the worker's materials), 0.5 the
  painted world, 1 grass.
  - `_WG_PaintShape.w` is the highest kind that takes ink: 0.7 for E (as
    before), 0.2 for F.
  - Shaders without a DepthNormals pass leave alpha 0, which only matters
    within the ink's 45 m reach.
- **Motes.** `WonderMotes` draws about 2,400 GPU quads in a box that follows
  the camera (`WGMotes.shader`): seeds lit when backlit by the sun, and
  fireflies at dusk and night.
- **Setup.** `OrdinaryPlaceSetup.AddTheBeyond` applies all of this to the
  existing scene in place (GUIDs kept) and can run again.
  `BuildWindows -buildOut <folder>` builds to `Builds/<folder>`.
- **Capture.**
  - `-captureSet essence` renders the vista, house, lake, sky, strategy, path,
    overview and grass views in E and F.
  - `-captureOnly`, `-captureHours` and `-captureMap` narrow a run.
  - The top-down map is only for checking the layout; its depth precision is
    too coarse for the water.

## Dusk details (S1c, third pass) — October 2

- **`Fireflies`.** 600 GPU quads (`WGFireflies.shader`), on a patch of meadow
  around where each camera looks.
  - **The patch** is centred where the view ray meets the ground (at most
    200 m out). Its radius is 22 m close up and widens with the camera's
    distance, up to 75 m. `_WG_FireflyVolume.w` carries it to the shader,
    which fades the patch's rim.
  - **Presence** follows the sun's height: none by day, all after sunset.
  - **Near the camera** they fade and shrink.
  - **Far away** each keeps a minimum size of about 1.6 pixels, made a little
    brighter while it is held there, so they read as a sparse scatter instead
    of vanishing.
- **The hearth.** `TimeOfDay.Hearth` flickers every house light with one
  shared fire noise (the lantern with its own candle noise).
  - It also scales `_WG_GlowScale` by part of that flicker, so the windows'
    emission breathes with it.
  - It is on by default.
- **`WindowGlow`.** It draws a soft additive halo just outside the door and
  window lights (`WGWindowGlow.shader`).
  - The halos are pushed out along each light's horizontal direction.
  - Each halo is faded by scene depth where it meets geometry, and as the
    camera comes close.
  - It follows `_WG_GlowScale`, and is on by default.
- **Switches.** `LookDevControls.SetDusk` and keys 7, 8 and 9 switch the
  three. `OrdinaryPlaceSetup.AddDuskDetails` adds them to the scene in place
  and can run again.
- **Captures.** `OrdinaryPlaceCapture -captureSet dusk` recreates Luis's
  favourite frame (look E, 19:12, low on the path, 1600×670) and the doorway
  at night, with each detail off and on.
- **Lamplight in the grass.** Local lights (house, lantern) are applied
  separately from the main light.
  - **`WGPainted`.** Local lights apply only 35% of the vertex-colour occlusion
    that the sun and moon get, so lamplight reaches the soil under the grass.
  - **`WGGrass`.** Local light on a blade scales with `1.05 + 1.05·h` (it was
    `0.5 + 1.6·h`), so the blade bodies take nearly as much warm light as the
    tips.
  - Together they keep the warm pool the same near and far.

## The miners (S1d) — October 2

The first concepts (`worker_concepts.py`, `WorkerConceptCapture`,
`Art/Worker/Concepts`) were replaced by the miners; they remain in the history
at 3066921.

- **Source:** `Art/Blender/Worker/`, one module per kind of part. Every module
  fits any `Body`, so a character is a preset of modules.
  - **`shapes.py`.** Shared helpers:
    - materials;
    - flesh on joint chains (the skin modifier);
    - metaball clusters, negative balls carving;
    - `fuse` (join, voxel remesh, smooth, simplify);
    - `tube` (an elliptical cross-section along a path, closing to a point);
    - `lathe`, `panel` (cloth wrapped around the trunk), `spline`;
    - `two_bone` (a limb reaching a target, bending towards a pole).

    Every part's origin is the being's origin, as for rigging.
  - **`body.py`.** `Body` builds a posed skeleton from proportions: weight leg,
    hip shift and tilt, shoulder tilt, stoop, head turn, nod and tilt. Hands
    and feet reach targets by two-bone solving. It also builds:
    - **The head:** metaballs with the face projected in the head's frame.
    - **The neck and bare forearms.**
    - **Hands:** palm metaballs, plus finger and thumb tubes curled by a grip,
      fused. A hand can be told the direction of the handle it holds.
    - **Boots:** shaft and foot fused, a sole tube, a heel slab and laces.
  - **`faces.py`.** The painted faces (laughing, sleepy, curious) and soot.
  - **`hair.py`.** Locks are combed in skull space (a unit sphere) with
    gravity, bend and twist. Each is a flat tapered tube, joined over a cap
    thickened outwards above a hairline.
    - **Styles:** `bob`, `swept`, `curls` (with `top` to stop under a cap).
  - **`outfits.py`.** Garments and accessories fitted to the trunk's measured
    levels (`trunk_at`, `axis_at`):
    - **Tops.** Torso and sleeve chains fused, creased at the elbows. A
      `skirted` top stops at the waist.
    - **Skirts.** Rings with vertical folds and ragged hems.
    - **Trousers.** Tucked and bunched.
    - **Collar, lapels, shirt front, apron (bib, straps, ties).**
    - **Buttons and patches.**
    - **Accessories:** a satchel, lantern, pickaxe (`hold` places the hand
      along it), mug, lamp cap and hammer.
  - **`workers.py`.** The three presets (Small, Long, Round), the material
    table, painting, export and manifest.
    - **Writes:** `Assets/_WonderGather/Art/Worker/Miners/`: `Workers.fbx`,
      `Face_<Character>.png`, one painted JPEG per painted material, and
      `workers.json`.
- **Painting.** It uses `../OrdinaryPlace/painting.py`, with new optional
  recipe keys (the house's defaults are unchanged):
  - `scale` multiplies the broad, middle and drip noise, for small objects;
  - `stain_top` is the height dust reaches;
  - `ao` and `bevel` are the cavity and edge radii;
  - `fill` is the colour between islands.

  Two rules for the bake:
  - **Linear bases.** Character bases go in as linear colours, because the
    emission bake saves sRGB; otherwise the texture comes out about twice as
    light.
  - **Apart.** The three miners are moved apart while baking. Built on one
    spot, each one's cavities were shaded by the others' bodies.
- **Fitting to real surfaces (October 3).** A fused garment comes out 1–2 cm
  smaller than its measurements after the voxel remesh and smoothing. So
  pieces laid over others measure them by ray casts (`outfits.surface_hit`):
  - **Skirts** hang from just inside the top's measured surface, and clear the
    trousers at the seat and down the sides of the thighs.
  - **The apron and its ties** lie outside the measured top and skirt.
  - **Patches and pockets** are cast onto their garment and turned to its
    surface (`b.marks` keeps a pocket's place, for the hammer).
  - **The neckband** rides on the smock's surface.
  - **Round's hands** rest at the measured waist (`workers.on_hip`).
  - **Build order.** Trousers are built before the skirts that cover them.
- **Polished body parts (October 3):**
  - **Neck.** The head's metaballs include the neck, a column from behind the
    jaw into the collar that widens into the shoulders. `neck_drop` lowers the
    head for a short neck.
  - **Boots.** One shoe-last form and a shaft, fused, flattened underneath;
    the sole follows the outline; crossed laces.
  - **Hands.** `THUMBS` paths per grip.
  - **Creases.** `bend_creases` puts folds only inside bent joints.
  - **Curls.** A metaball cloud.
- **Faces at a distance.** `MinerCapture` imports faces with a mipmap bias of
  −1.2 (other painted textures −0.3), so their thin strokes do not average
  into the skin when small on screen.
- **`MinerCapture`** (Editor).
  - **Materials.** It reads `workers.json` and makes `Materials/Miners/*.mat`
    on `WGPainted`:
    - painted textures and faces on a white base;
    - variation 0.1–0.12;
    - `_BrushScale` 14 (a person is smaller than a house; at the world's 3
      per metre, the shader's own marks blotched the cloth);
    - glow as emission;
    - `_Drawn` on everything.
  - **Outline.** It adds the outline, except on glass.
  - **Lamps.** It puts a small warm point light (no shadows) at each lamp's
    glass.
  - **Shots.** It stands the miners on the path and renders:
    - the line-up at dusk, by day and at night;
    - the Strategy height;
    - portraits at three hours;
    - a four-view turnaround of each, alone.
  - **In the Editor** it asks before leaving a modified scene, and keeps the
    miners out of the saved scene (`DontSaveInEditor`). It restores the worker,
    hour and look afterwards, and leaves the miners standing to look at.
- **`WGOutline.shader`.** A drawn contour as an inverted hull (back faces
  pushed out along the normals).
  - **Pushed back.** The hull is also pushed `_Behind` (3.5 cm) away from the
    eye. Without it, a smock's line showed through an apron lying a few
    millimetres over it, as dark blotches seen from a few metres away.
  - **Width.** About 1.6 pixels at 1080p near or far, capped at 1.2 cm.
  - **Wobble.** It varies with object-space noise, so it does not swim as a
    being moves.
  - **Pass.** It renders in URP's `SRPDefaultUnlit` pass (depth priming is off
    in this project's renderer).
- **`_Drawn` on `WGPainted`.** A toggle, off by default.
  - **When on.** The DepthNormals pass writes −1 into the normals' alpha:
    0 is the painted world, 1 is grass.
  - **Paint filter.** It reads that alpha and applies only 15% of its strength
    to drawn beings, so painted faces keep their marks.
  - **Ink.** It still draws on beings, since `1 - saturate(-1)` is 1.
  - **`LookPostEffects`.** The paint pass now declares the normals texture
    when it is valid.

## The rigged miners (S1d) — October 3

- **Export.** `workers.py --rigged` rebuilds each miner in a rest pose
  (`rest_preset`: neutral stance, arms 25° out, head straight, held things
  moved to the belt or back). `rigging.py` then:
  - **Skeleton.** It builds the armature: 19 deform bones under `Root`, named
    `.L`/`.R` for the being's own sides.
  - **Skinning.** It skins by kind of part (`candidates`), then by distance to
    each bone's capsule (3 influences). Skirts, aprons and pockets are blended
    pelvis to thighs by height.
  - **Simplification.** It simplifies to 16,000 triangles and joins into one
    mesh. A `wg_detail` face attribute marks small details.
  - **Atlas.** It unwraps once (Smart UV), triples the face's islands, and
    bakes painting, face and colours into `Miner_<Name>_Atlas.png` (2048²).
  - **Materials.** It reduces them to `Miner_<Name>` plus `Glow`. The glow
    faces are sorted first: Unity orders submeshes by first use, and the
    outline (an extra material) draws the last submesh, which must be the
    body.
  - **Levels of detail.** It makes LOD1 (5,000 triangles) and LOD2 (1,600,
    details dropped).
  - **Export.** One FBX per miner, armature plus three meshes.
  - **Writes:** `miners.json`, the body's dimensions in the game's terms:
    - hip height and width, leg segment;
    - ankle, heel, ball and toe;
    - waist and head rise, the shoulder offset;
    - arm segments;
    - the game's arm hang (`hang`: close to the body, the elbow just clear of
      the trunk).

    `--rigged --dims` rewrites only this file.
- **`ProceduralBiped.Proportions`.** The body's measurements as data:
  - **Defaults.** Exactly the old constants: the 2.2 m test body. With
    `customProportions` false, every derived value equals the original
    literal.
  - **Derived from the proportions:** leg reach, the leg limit, the narrow
    stance (two thirds of the hip width), and the stance tolerances (scaled).
  - **Scaled by `scale`:** carried loads and pumping arms.
  - **API.** `SetProportions` validates and applies them;
    `BodyProportions` reports them.
- **`MinerBody`** (runtime, execution order 50, after the biped). The rig
  adapter.
  - **Rest.** It captures the rest pose at `Awake`: each bone's rotation
    relative to the root, and limb aims.
  - **Every `LateUpdate`:**
    - **Pelvis.** It is placed at the solved hips and turned by the hip frame.
    - **Spine and neck.** They take half the turn between hips and chest, and
      between chest and head.
    - **Chest, head and feet.** Each turns by its solved frame's turn from
      rest; toes follow the solved toes.
    - **Limbs.** Each aims at its solved joints, rolled so knees bend forward
      and elbows back (a fallback steadies nearly straight limbs). Hands
      follow forearms.
  - **Segment ends.** These are read from the biped's segments (position
    ± up × half length).
- **`MinerSetup`** (Editor).
  - **`CreatePrefabs`.** It imports each FBX (generic, no avatar, objects not
    optimised) with materials remapped:
    - `Miner <Name>.mat`: the atlas on `WGPainted`, `_BrushScale` 14,
      `_Drawn`;
    - `Lamp glow.mat`.
  - **Each unit:**
    - a capsule, agent (1.3 m/s, radius 0.3), motor, selection and ring;
    - invisible solution segments under `Body solution`;
    - the biped with the model's proportions and tuning;
    - the model, turned to face forward and checked for height;
    - the LOD group (screen heights 0.18, 0.035, 0.004);
    - the outline on LOD0 and LOD1;
    - a lamp's point light on the bone carrying the glass;
    - `MinerBody`.

    Prefabs go in `Prefabs/Miners/`.
  - **`AddToOrdinaryPlace`.** It replaces the scene's worker with the three,
    under `The miners`, with `MinerChoice` and `MinerCrowdBenchmark`. It can
    run again.
- **`MinerChoice`** (runtime).
  - **One active at a time.** `Choose` swaps the miner in place (warping its
    agent) and hands over the selection.
  - **Opening.** IMGUI picker at the bottom, open on load and with `M`.
  - **Memory.** `PlayerPrefs` remembers the choice (wrapped in try/catch).
- **`MinerCrowdBenchmark`** (`-wgcrowd`). It runs crowds of 0, 25, 50 and 100
  wandering miners, in the Strategy and close views, and writes
  `miner-crowd-benchmark.csv`.
- **`MinerWalkCapture`** (PlayMode, `[Explicit]`). It renders frames of each
  miner walking, by day and at dusk, and the three together.
  - **Format.** PPM files, because the image conversion module is not in this
    project.
  - **Running it:** `-testFilter WonderGather.Tests.MinerWalkCapture -captureOut <folder>`.

## The miners' polish and natural walks — October 3, evening

- **Blender sources.**
  - **`body.head`.** The neck rises from behind the jaw (`on(0, 0.3, -0.62)`)
    and widens into the collar, and a negative ball hollows under the chin.
    `body.shaft_radius` is the boot shaft (leg × 1.12).
  - **`outfits.top`.** It closes the neckline round the neck.
  - **`outfits.collar`.** One strip from inside the coat (stand, then fall);
    `open_front=False` buttons it up.
  - **`outfits.trousers`.** They fall over the boots' shafts;
    `hidden_above` deletes faces no one can see under a closed skirt.
  - **`outfits.lay_on`.** It lays points on garments: cast towards the
    trunk's axis below the chest, or down over the shoulders. Points that miss
    are placed between neighbours that hit.
  - **Laid with it:**
    - the satchel's strap (to the bag's top, with tabs);
    - Long's new `bandolier`;
    - the apron's straps;
    - the buttons.
  - **The satchel** rests on the measured skirt at the right hip.
  - **The apron** hangs 8 mm to 2 cm clear of the measured clothes.
- **Rest pose.** `workers.REST` declares, per miner:
  - **`hold`:** a hand that carries something curls round it with the grip
    `hold`;
  - **`swap`:** a carried thing changes how it is carried;
  - **`add`:** extra pieces, such as Long's bandolier.

  `b.props` records carried things that get bones of their own: `Lantern` and
  `Mug`, under the hand's bone. The skeleton is 20 bones for Small and Long.
- **Skinning (`rigging.candidates`).**
  - **Rigid.** `rigid:<bone>` makes a part rigid: the lantern, the mug, the
    satchel and tabs on the pelvis, the slung pickaxe on the chest.
  - **Same weights.** Pieces on the torso take the same candidate bones as
    the top.
  - **Skirts and aprons** blend from the torso's own weights above the waist
    to the pelvis and thighs below.
- **`ProceduralBiped.Proportions`.** It gains `bounce`, `sway` and
  `armSwing`, multipliers of the pelvis' rise and fall, its sway and roll, and
  the arm swing. They are 1 by default; zero, from older data, reads as 1. The
  selection ring's height scales with `scale`.
- **`MinerBody`.** It takes an optional swinging prop (the lantern's bone) and
  swings it as a damped verlet pendulum under the hand, at most 55° from
  hanging.
- **`MinerSetup`.**
  - **Pace.** Each agent's speed is √(Froude · g · hip height), the walk's
    Froude number coming from `miners.json`: Small 1.27, Long 1.38, Round
    1.28 m/s. The walk's character goes to the proportions.
  - **Lamps.** A carried lantern is a downward spot (150°, 2.2 m); a cap lamp
    is a forward-down spot (75°, 5 m).
- **`MinerChoice`.** Choosing a miner calls `ProceduralBiped.ResetPose`, so
  its body stands where it now is. Before this, the feet stayed where that
  miner was last shown, and the body walked away from the unit.
- **`MinerCloseCapture`** (PlayMode, `[Explicit]`). It produces the frames of
  the model quality method's capture matrix (see
  [ModelQualityMethod.md](../ArtDirection/ModelQualityMethod.md#4-the-capture-matrix)).


## Physical objects and the model quality method — October 3 and 4

Luis asked that anything that reads as an object behave as one, and for a
method to raise model quality
([ModelQualityMethod.md](../ArtDirection/ModelQualityMethod.md);
the round's log is in
[TheMiners.md](../ArtDirection/TheMiners.md#the-methods-first-round-october-3-and-4)).

- **Blender sources** (`Art/Blender/Worker/`).
  - **`audit.py`** (new). The automatic audit of the parts as exported,
    before they are joined: floating, lies on, meets, sinks, covered, beneath
    and technical checks, at rest and on the game's recorded frames. The rules
    are tables at its top (`LIES`, `MEETS`, `APART`, `COVERED`, `BENEATH`).
    It writes `audit_<Name>.txt` and `.json`, and renders each failure.
  - **`shapes.py`.** `Surface` (cast, nearest, lay on real surfaces);
    `exact` (thin parts that are not simplified); `keep_largest`; `sheet`;
    `ring`.
  - **`body.py`.** `boot` builds the shaft along the shin; `lacing` threads
    laces through eyelets on the boot's surface; `wrap` and `hand(bar=…)`
    close a hand round a handle and cut it to fit.
  - **`outfits.py`.**
    - `taut`: a strap laid on cloth and pulled taut, pinned where it bears.
    - `satchel`, `sling`, `lantern`, `mug`, `apron`, `patch`, `hammer`: each
      built as it is made and fastened.
    - `skirt`: records its hem; lies over the knees point by point;
      `skirt_slack` measures how far a thigh swings before it reaches the
      cloth.
  - **`rigging.py`.** Four flap bones per skirt (`SkirtFront.L` and so on, at
    the hips); `LIKE` and `weights_like` give what lies on something its
    weights; carried things get bones (`Lantern`, `Mug`, `Satchel`,
    `Hammer`). Bones: Small 25, Long 24, Round 24.
  - **`workers.py`.** `carriage` writes, per miner, into `miners.json`:
    - `armCarry`, `armSwingSide`: how far out each arm hangs, and how much
      of its swing it keeps;
    - `hanging[]`: each hanging thing's bone, hand, length, damping, limit,
      aim, stop (normal and distance), handle, pusher (the skirt flap under
      it), and whether it `rides` cloth or is `hinged`;
    - `skirtSlack`: how far a thigh swings, front and back, before the flaps
      move.

    New flags: `--audit`, `--poses <folder>`, `--report <folder>`.
- **`MinerBody`** (runtime).
  - **`Hanging`.** A pendulum per hanging thing. Its state is the weight's
    offset under the place it hangs from, and its speed. Its own swing fades;
    the movement it shares with that place does not. The body stops it at a
    plane in the pelvis' space (pushed out by the flap under it), and it
    rests there. With a hand, the wrist gives up to 32° so the handle stays
    square to the load; the give is shown, not fed back. `hinged` things swing
    only square to their handle's direction. `riders` place the bone on the
    cloth it is sewn to.
  - **`Flap`.** Skirt flaps turn with the thigh that moves into them, after
    the measured slack, and fall back when it leaves.
  - **For tests:** `HangingWay`, `HangingIntoBody`, `HangingAskew` report what
    was last posed, as one moment.
- **`ProceduralBiped`.**
  - `Proportions.armCarry` and `armSwingSide`, per arm.
  - **Boots keep clear of each other.** `BootGap` measures two boots as
    lines from heel to toe with a width. A swinging boot's path bows round
    the standing one (`StepClear` finds the least step aside), and a boot
    never lands on the other. `BootClearance` reports the room left.

  The walk itself (timing, lift, stride) is unchanged.
- **`MinerSetup`.** Reads the new fields, finds the cloth a riding thing is
  sewn to (`Riders`: the nearest skin that is not the thing's own), and
  lowers the lamp glass's glow.
- **Tests and tools** (`Tests/PlayMode`).
  - `MinerTests`: two new tests. What a miner carries hangs from its hand,
    stays out of the body, and hangs straight when carried steadily; in a
    sharp turn the boots never overlap.
  - `MinerPoseRecord` (`[Explicit]`): records each miner's bones through
    standing, walking, a sharp turn and a stop, for the audit in motion.
  - `MinerCloseCapture` (`[Explicit]`): the capture matrix. `CaptureTools`
    holds what the two share.
  - `Art/Review/sheets.py`: contact sheets from the frames.

## The sweep of extreme poses — October 5

The first step of "the miners at work"
([NextMilestonePlan.md](../Plans/S1_OrdinaryPlaceCamerasAndMiners.md#s1d-last-part--the-miners-at-work-plan-waiting-for-luiss-choices);
findings in [TheMiners.md](../ArtDirection/TheMiners.md#the-sweep-of-extreme-poses-october-5)).
No game code changed.

- **`Art/Blender/Worker/sweep.py`** (new).
  - **Poses:** a table of 27 (`poses()`), each a set of turns on bones, in
    the rest pose's own axes. A limb that hangs goes forward by a negative
    turn about x; the trunk and the head bow forward by a positive one.
  - **The solve** (`solve`): forward kinematics down the rig, then linear
    blend skinning on the exported weights, as the game does. Skirt flaps
    follow the thighs with the measured slack (as `MinerBody.Drape`). A thing
    in a hand follows the hand and hangs plumb within the wrist's give (32°,
    as `MinerBody`); a thing hung on the body stays plumb.
  - **Checks:** the audit's own on every pose, plus `pinched` (a joint's
    cover against the rest pose, for the points both bones share) and
    `stretched` (a cloth's longest edge against the rest pose).
  - **Output:** `sweep_<Name>.txt`, `sweep_<Name>_poses.txt`, and three
    pictures of each pose (two sides and the joint tested).
- **`workers.py`:** `--sweep` runs it alone, without baking or exporting.
- **`audit.py`:** the `beneath` check for legs under a skirt counts a point
  only if the way to it from the hip, down the leg's line, crosses the cloth
  (`leg_lines`, `crosses`). A leg showing under a lifted hem is not through
  the cloth.
- **`Art/Review/sweep_sheets.py`** (new): contact sheets of the sweep.

## Hands that close — October 5

The second step of "the miners at work"
([TheMiners.md](../ArtDirection/TheMiners.md#hands-that-close-october-5)).

- **Blender** (`Art/Blender/Worker/`).
  - **`hands.py`** (new).
    - `joints`: fifteen bones for every hand that closes (`Index1.R` …
      `Little3.R`, `Thumb1.R` … `Thumb3.R`), each from its joint to the
      next, on its hand.
    - `weights`: the hand's skin shared with the digits it lies on. The
      joints' zones never overlap, so a point is on at most two bones of a
      finger and four in all (the engine keeps four).
    - `table`: for each handle radius (`RADII`, 6 to 22 mm), where every
      joint goes. `close` solves one hand on one handle by halving, on the
      skinned mesh: each finger until its outer bones' skin touches
      (`finger_goals`, `curled`), then the thumb (`thumb_goal`). The handle
      is lowered towards the palm while the fingers can still close.
    - `report` (`hands_<Name>.txt`, pictures with `--hands`) and `export`
      (the `grips` entry of `miners.json`: bones, joints at rest, open and
      closed for each radius, the handle's axis).
  - **`body.py`:** `hand` records each digit's joints (`b.hands`); a hand
    that closes keeps only its own surface (`shapes.keep_largest`). `wrap`
    takes one standoff a bone.
  - **`rigging.py`:** `HAND_TRIANGLES`; `whole_hand` (the farthest level has
    no skin on finger bones); `skin` calls `hands.weights`.
  - **`workers.py`:** `--hands`; every build checks the hands and exports
    `grips`.
- **Unity.**
  - **`MinerBody`.**
    - `Grip`: a hand's finger joints, and each one's turn (in its own frame)
      to the open hand and to the hand closed on each handle.
    - `Hold(hand, radius)`, `Release(hand)`, `Held(hand)`, `CanHold(hand)`;
      `HandleIn(hand, radius, …)` gives where the handle lies in the hand
      now.
    - `Close`: from rest to open to closed, in 0.3 s. A hand at rest or
      fully closed is not touched again.
  - **`MinerCrowdBenchmark`:** keeps running when the game's window is not
    in front (it hung for as long as the computer was in use).
  - **`MinerSetup.Grips`:** turns the model's joint places into each bone's
    turn (the smallest turn that aims it, its parents' first: the same rule
    as `hands.turns`). The model's space is fitted to the hand's own bones
    from three knuckles, because the prefab squares the model by head and
    feet (Long's sits 11° off its own axes). It logs `MINER_GRIP_FIT`.
  - **Tests:** `MinerTests.AFreeHandClosesRoundAHandleAndOpensAgain`. The
    bone guard in `EachMinerIsLightEnoughForTheRts` is now 58 (the body's
    bones, and fifteen for each closing hand).
- **Not yet used by the game:** nothing calls `Hold` until a tool is in the
  hand (the next steps).

## A pickaxe for each body, and a tool solve that reads the body — October 6

Steps 3 and 4 of "the miners at work"
([TheMiners.md](../ArtDirection/TheMiners.md#a-pickaxe-for-each-and-the-first-swings-october-6)).

- **Blender** (`Art/Blender/Worker/`).
  - **`tools.py`** (new). `pickaxe(name)` builds one miner's pickaxe with
    `outfits.pick` (round-handled, stouter by the hand, a quarter of the
    faces); `measured` reads the grips, the handle's radius at each and the
    striking head from the model; `build` bakes a 512 atlas, exports
    `Pickaxe_<Name>.fbx` (two levels of detail) and writes `tools.json`;
    `handle_radii` gives the hands their own rows.
  - **`workers.shape`:** `bodyFront`, `faceFront`, `headHalf` in
    `miners.json`. `--dims` now merges into an entry (the hands' closing is
    only found by a full build). `miners.json` is written under a lock.
  - **`audit.py`:** `run(…, tool=…)`. On frames that carry `"t"` the tool's
    parts (`WorkPickHandle`, `WorkPickHead`, `WorkPickCollar`) are posed by
    the pseudo-bone `Tool`; `WORK_APART` and `WORK_MEETS` apply; the sweep's
    `pinched` and `stretched` run on those frames.
  - **`hands.tables(…, own)`:** rows for the body's own handles.
- **Unity.**
  - **`ToolDefinition`:** `gripRadii`, `GripRadius(hand)`.
  - **`ProceduralBiped`.**
    - `IHandHolds` (interface): `HandFree`, `WristFor`, `HoldHandle`.
    - For tools: `ArmReach`, `ToolScale`, `ToolReachMin/Max`, `ToolHand`,
      `ShoulderFromHips`. Without measurements of its own (the first body)
      each returns the exact number it replaced.
    - `Proportions.bodyFront`, `faceFront`, `headHalf`.
    - A hand that is not free takes no load's place.
  - **`EquippedTool`.**
    - `Uses(hand)`, `WristPosition(hand, shoulder)`, `HandleDirection`,
      `Model`. Reach is judged at the wrist, for each hand that holds; the
      lower grip is the right hand's.
    - `PoseAt`: for a body with hands of its own, the swing over the
      shoulder (the hands' path is a curve from ready to raised, and out as
      it strikes; the tool leans 14° outwards when raised). Otherwise the
      first body's swing, unchanged.
    - `SolveFrame` tells the body's hands what they hold (`HoldHandle`).
  - **`MinerBody`** implements `IHandHolds`: `HoldTurn` (the hand bone's
    turn to lie on a handle), `WristFor` (less the model's shoulder
    offset), `HoldHandle` (closes or lets go; the hand turns onto the handle
    as it opens). `Pickaxe`: the tool made for this body.
  - **`MinerSetup.Pickaxe`:** imports the tool, checks it came in the right
    way up (and turns it to strike forwards), paints and outlines it, gives
    it two levels of detail, and writes `Data/Miners/Pickaxe_<Name>.asset`.
  - **`MinerSetup`:** `-minerStance built` stands each model on its own
    vertical (choice P1). The default stands it with its head above its
    hips, as since October 3.
  - **`MinerWorkPreview`** (new): `K` in the Ordinary Place. `RockFace`
    (also used by tests and captures) and `FaceDistance`.
  - **Tests:** `MinerToolTests` (three). `MinerPoseRecord` records the
    mining; `MinerCloseCapture` has the `work` set and, only when asked
    for (`-captureSets clip`), the `clip` set: every frame of four seconds
    of the mining at 25 frames a second, which `Art/Review/clip.py` puts
    together as a moving picture (added October 6 for
    [the review](../Reviews/2026-10-06_SamePageReview.md)).
- **Not built:** a place to mine, body-sized places to stand, where a tool
  is kept, hauling (step 5 and the choices M1 to M4).

## The physical body, steps 1 and 2: weights, and a real tool in the hands — October 6

Stage S3 ([plan](../NextMilestonePlan.md); [design](../Design/ThePhysicalBody.md)).
Nothing here is in the game's mining yet: it is a bench, run as a test.

- **Blender** (`Art/Blender/Worker`):
  - **`weights.py`** (new). `body(b, meshes, bones)` weighs one being:
    the body under its clothes as solids from its own measures (the
    trunk's widths and depths, the limbs' thicknesses, the head), at
    1000 kg/m3; what it wears by area (cloth 0.55, hair 0.3, leather 1.8,
    sheet metal 6.8, glass 5.0 kg/m2) or, for a pick's or a hammer's wood
    and iron, by volume (700 and 7850 kg/m3). Each piece goes to the part
    of the body whose bone its skin follows. `tool(parts)` weighs a tool:
    mass, centre, principal moments and their turn, in the tool's space.
    A pick's collar is a ring, not the solid drum it is modelled as.
  - **`workers.py --weigh`** and **`tools.py --weigh`** add the weights
    to `miners.json` (`weights`: per bone a mass and its centre; four
    joints; the limbs' thickness) and `tools.json` (`mass`, `centre`,
    `inertia`, `inertiaTurn`) without rebuilding. A full build weighs
    too. The pickaxe is weighed on a fine copy (the game's is faceted).
- **Unity:**
  - **`PhysicalBody`** (new, on each miner). Each part's mass at its
    place in its own bone; the centre of mass from the pose; the arms'
    capacities. A joint's capacity is an ordinary grown arm's (shoulder
    70, elbow 60, wrist 12 N·m; hold 400 N) scaled by the cube of the
    limb's thickness against 47 mm (the hold by its square), times
    `Strength` (1: ordinary for the build). `MinerSetup.Weigh` fills it,
    finding how the model sits in its prefab from four joints (the same
    angles the hands' fit found).
  - **`ToolDefinition`**: `Mass`, `Centre`, `Inertia`, `InertiaTurn`,
    `Foot`, `Top`, `Point` (`ConfigureWeight`).
  - **`PhysicalHands`** (new; added at need). `Take(tool, place, turn,
    weight)` makes the tool a `Rigidbody` with its own mass properties and
    a solid (a capsule for the handle, one for the head), and holds it in
    the free hands. `Want(place, turn, all)` says where it is meant to
    be. Each physics step:
    1. the push and the turn that would bring it there without overshoot
       (a rate of 14 a second, 16 for the turn; 30 when `all`), following
       the wanted place's own motion, and holding the weight up;
    2. shared between the hands: two hands turn it by pushing opposite
       ways across the handle; a twist about the handle is the wrists';
    3. limited: a push at the hand asks a turning force of the shoulder
       and of the elbow (a hinge), beside what the arm's own weight asks;
       neither gives more than its capacity; a muscle gives less the
       faster the hand already moves that way (nothing at 14 arm's
       lengths a second) and 1.5 times as much when forced back. **Both
       arms give the same share** of what is asked, the lesser one's, so
       the two together push the way that was meant;
    4. applied as forces on the body.

    The hand, half the forearm and an eighth of the upper arm ride on the
    handle as mass at each held place (one mass, centre and inertia for
    tool and arms, by Jacobi's turns). A `ConfigurableJoint` from each
    shoulder (a kinematic body, moved to the shoulder plus the palm's
    offset) keeps the wrist within an arm's length.
  - **`IArmGuide`** (in `ProceduralBiped.cs`). The body is told how it
    stands, then asked for each guided hand's wrist: the arm follows the
    object, it does not place it. `PhysicalHands` implements it, and
    turns each hand onto the handle through `IHandHolds`.
  - **`ProceduralBiped`**: `GuideArms`; `Bow(degrees)` (the body bows
    from the hips at 150 degrees a second); `StandsAt(time, ...)`: the
    hips, the posture, the shoulders and the elbows as they will be at a
    moment a little after the body was last posed, going on as it was
    going. The physics reads the body through it (B5 in the design's
    findings).
  - **`HeldThing`** (new, on the held object): what it last struck, and
    blows given with its striking part.
  - **`PhysicalSwing`** (new): the bench's rough plan of a swing, as
    intentions. It leads the tool along the first swing's path over the
    shoulder, a set way ahead of where the tool is (22 degrees for the
    lift, at a set pace; 50 for the blow, with no pace set), with the
    body bowing to its work. Step 4 replaces it.
- **Tests** (`Tests/PlayMode`):
  - **`PhysicalBodyTests`** (three): every miner and pickaxe weighed;
    strength and weight decide the swing; the swing is the same at any
    frame rate.
  - **`PhysicalBench`** (`[Explicit]`): the grid, its figures (`BENCH`
    lines), a step-by-step trace (`-benchTrace`), every other frame of
    each (`-benchFrames`), at a set frame rate (`-benchRate`).
    `Art/Review/bench_clip.py` puts the frames together.
  - **`AfterEverything`** (new): calls back when a frame's bodies have
    all been posed. A picture or a measure of something the physics
    moved must be taken then (B8); a batch run cannot wait for the end of
    a frame.
- **Not built:** see [the design](../Design/ThePhysicalBody.md#step-2-the-bench-october-6).

## The physical body, step 3: both hands free — October 6

- **Blender:**
  - **`outfits.hip_hang`** (new): a leather tab sewn to the coat, with a
    brass hook standing off it or a loop of thong; where a handle rests in
    it; and how the thing hangs from there (straight down, or resting on
    the cloth below). It chooses the tab's height: the highest place where
    the thing leans out from plumb by no more than a set angle.
    **`outfits.hang_by`** turns parts built hanging plumb to hang that way.
  - **`workers.py`**: `REST` hangs Small's lantern and Long's mug this way
    (`hung=...`), and no hand is modelled shut. `REST_CARRYING` and
    `--carrying` build them as before. The lantern and the mug keep their
    bones (`Lantern`, `Mug`), now children of the pelvis, with a stop on
    the coat and `rides` (the hook goes with the cloth). `carriage` hangs
    the arm on that side clear of the thing; `shape` leaves hung things
    out of the body's front.
  - **`rigging.py`**: `_HipTab`, `_HipHook`, `_HipLoop` take their weights
    from the skirt or the top, and are dropped at the farthest level.
  - **`audit.py`**: a lantern's grip rests in a hand or in its hook, a
    mug's handle in a hand or its loop; hook and loop meet the tab, the tab
    lies on the coat; what hangs at the hip may press 6 mm into the skirt
    (as the bag does).
- **Unity:** nothing new. Both of every miner's hands are free
  (`MinerBody.HandFree`), so the old swing holds with two hands on all
  three. `MinerTests` expects six closing hands. Small's line in the
  choice reads "a lantern at the hip".

## The physical body, step 4: the swing — October 6

- **`PhysicalHands`:**
  - **A hand slides** (`Slide(hand, along)`, `Sliding`, `GripAlong`,
    `LowestGrip`, `HighestGrip`): its place on the handle moves at 1.6 m/s,
    no nearer the other hand than a fist and a half. The link from its
    shoulder and the mass that rides with it move with it (the held
    thing's mass, centre and inertia are worked out again). While it
    slides it holds loosely: what it would have pulled along the handle is
    the other hand's to give. `RadiusAt(along)`: the handle's thickness
    there, for the fingers.
  - **`GripPlace(hand)`**: where a hand holds, as the physics has it.
  - **Collisions are speculative** (`ContinuousSpeculative`): a pick's head
    moves fast by turning, which sweeping does not follow (S2).
- **`PhysicalBack`** (new). Each physics step: the turning force the upper
  body's weight asks of the back about the hips; what the hands' pushes ask
  (the load); the muscle's answer, which would bring the bow to where it is
  meant to be without overshoot (9 a second), limited by
  `PhysicalBody.BackCapacity` (less the faster it already bends, nothing at
  320 degrees a second; 1.5 times as much when forced back); the bow is
  integrated and given to the body (`ProceduralBiped.Bow(angle, pace)`).
  Then the hips: `Ahead` is how far the weight of body and tool stands
  ahead of the middle of the feet, and the hips go back by half of that
  each step (`SetBack`).
- **`PhysicalBody`:** the parts above the hips (`UpperBody`: their mass,
  centre and inertia about the hips, from the pose); `BackCapacity`: 2.4
  times what the upper body asks bent level, times `Strength`.
- **`ProceduralBiped`:** `Bow(degrees, pace)`; `SetBack` (the hips go back
  behind the feet, at 0.5 m/s); `Sink` (the hips sink and the knees bend,
  at 0.6 m/s); `FootMiddle`; `StandingHipHeight`.
- **`PhysicalSwing`:**
  - **The upper hand:** at rest it reads how much of the arms holding the
    tool takes, and slides the upper hand towards the head by that (none
    below 22%, all the way at 50%); in the blow it slides back.
  - **`Aim(point)`:** searches the bow, the knees' bend and the tool's lean
    for the pick's head to be at the point, standing where it stands
    (preferring to stand tall). The rest pose, the blow's bow and where the
    blow means to end follow from it. The legs straighten under the lift.
  - **`Result`:** also `choked`, `backEffort`, `landed`.
- **Tests:** `PhysicalBodyTests` has five: weighed; strength and weight
  decide the swing; each miner swings its own pickaxe; the body adapts to
  its strength and to where it strikes; the same at any frame rate.
  `PhysicalBench` takes `-benchMarks` (heights to aim at).
  `Art/Review/row_clip.py` puts clips side by side.

## The physical body, step 5: tiredness — October 6

- **`PhysicalBody`:**
  - **`Muscles`** (`LeftArm`, `RightArm`, `Back`, `Legs`), each with a
    share that is spent. `Worked(muscles, effort, dt)` is said once a
    physics step by whatever works a group; groups nothing worked rest (in
    `FixedUpdate`). The rule: spent rises by 0.04 a second times the effort
    above 0.18 (rescaled to 0 to 1) times what is not spent; it falls by
    0.012 a second times itself, 3.5 times as fast at no effort (by the
    square of what is not being given). At most 0.85.
  - **`ShoulderOf(side)`, `ElbowOf`, `WristOf`, `BackNow`:** what the
    joints give now (their capacity times what is not spent). `Refresh()`
    clears tiredness.
- **`PhysicalHands`:** uses each arm's own capacities and tells each arm
  what it gave. `Want(..., bears)`: the share of the thing's weight the
  hands mean to carry. `Release(hand)`: one hand lets go (its link and
  riding mass go). `Grasp(hand, along)`: a free hand reaches (0.35 s; the
  wrist goes from where it was to where it will hold, the fingers close as
  it arrives) and then holds and pushes. `Reaching(hand)`.
- **`PhysicalBack`:** uses `BackNow` and tells the back what it gave.
- **`PhysicalSwing`:** `Phase.Rest`, entered after a blow when `Spent` (the
  most spent of the arms and the back) is at `RestsAt` (0.4): the back
  straightens (with a slow sway, as breathing), the upper hand goes to the
  head's end of the handle, the lower hand lets go, and the tool goes to
  `AtSide` (level at the left side, the arm hanging, out past what hangs
  there). At `GoesOnAt` (0.15) it recovers: the tool returns to rest over
  its mark and the lower hand takes hold again. `Result.spent`; `rests`.
- **Tests:** `PhysicalBodyTests` has seven: with the five before, hard
  work tires a muscle and rest brings it back; a tired miner weakens,
  rests and goes on (32 swings, frame for step). `PhysicalBench` takes
  `-benchSwings`, `-benchFrom` and `-benchView`.

## The physical body, in the place: `K` shows the swing with real weight — October 6

- **`MinerWorkPreview`** (rewritten). `K` in the Ordinary Place:
  - **`Toggle()`** begins or ends the look on the chosen miner. Beginning
    adds `PhysicalHands`, `PhysicalBack` and `PhysicalSwing` to it; the
    miner bows (0.7 s); a block (a plain cube with a collider, named
    "Block (a look at the work)") is put under where the pick's head then
    rests; `PhysicalHands.Take` gives it its pickaxe.
  - **`End()`** drops and destroys the pickaxe, the three components and
    the block, straightens the body (`Bow(0)`, `Sink(0)`, `SetBack(0)`),
    and puts strength back to 1 with tiredness cleared. It also ends when
    another miner is chosen or the miner has moved 0.3 m.
  - **`SetStrength(value)`** (0.3 to 3; keys `,` and `.`, a factor of
    1.25) sets `PhysicalBody.Strength` at once.
    **`SetWeight(value)`** (0.4 to 3; keys `-` and `=`) ends the look and
    begins it again on the next frame, since what the last look added to
    the miner is destroyed only at the end of the frame.
  - **`Showing`, `Swinging`, `Swing`, `Strength`, `Weight`.** `OnGUI`
    writes the line at the foot of the screen.
  - **`RockFace` and `FaceDistance`** stay, as static helpers for the
    first swing's tests and captures (`CaptureTools.RockFace`).
- **The first swing** (`Gatherer`, `EquippedTool`, `MineableResource`) is
  no longer reached from the Ordinary Place. The equipment scene, the
  recording for the audit in motion (`MinerPoseRecord`), the `work` and
  `clip` capture sets and `EachMinerSwingsItsOwnPickaxeAndTheHeadStrikesTheRock`
  still use it.
- **Tests:** `MinerToolTests.TheLookAtTheWorkBeginsAndEndsCleanly`
  (rewritten for this look). `PhysicalLookCapture` (explicit): pictures of
  the look as the key begins it, for each miner, from its side and from
  the game's camera; `-lookOut`, `-lookFrames`, `-lookStrength`,
  `-lookWeight`.

## The physical body, step 6: balance — October 7

- **`PhysicalBalance`** (new; `[DefaultExecutionOrder(520)]`, after the
  hands and the back). At each step of the physics:
  - **The body's weight:** `PhysicalBody.CentreOfMass()`, less what rides
    on a held thing with the hands (`PhysicalHands.Rides`).
  - **Loads:** what each arm gives the held thing, given back to the body
    (`PhysicalHands.Gives`: its push and its link's force), and whatever
    `Push(force, at)` was told for this step.
  - **Where the feet must press** for the body not to be turned over
    (moments about the ground, with what the feet bear), and from how fast
    the weight is going, **the weight's point** (weight + speed / the
    body's own falling rate, sqrt(g / its height)).
  - **What it stands on:** the planted boots' lines (`ProceduralBiped.Sole`),
    their shape from above, reaching a boot's half-width round it.
  - **Holding:** the feet press at the weight's point plus `Firm` (2)
    times how far it is from where it should rest, kept inside what it
    stands on. The weight then falls away from where they press. The
    hips' lean is whatever puts the weight where that has it.
  - **At ease:** it rests anywhere within 12% of a boot's length of where
    this body carries its weight (`PhysicalBody.StandsOn`, measured once
    when it has stood still with nothing asked of it). It leans no more
    than it needs, and under a load that lasts keeps its own weight over
    its feet too.
  - **Inclining:** against a load that lasts (smoothed over 0.12 s), by
    60% of the angle the load asks, 12 degrees at most.
  - **Stepping:** when the point has been within 40% of a boot's
    half-width of the edge, and not coming back, for 0.06 s. Past a
    foot's own side, that foot; between the feet, the one behind. It lands
    where the point will be after the step (1.1 falling times), on its own
    side of it, no further than 0.75 hip heights from the other foot. The
    stance becomes where the feet are and the root goes to its middle
    (`ProceduralBiped.Shift`). The feet come together when no load has
    lasted for 1.2 s.
  - **A foot due to step to its place** waits (`ProceduralBiped.MayLift`)
    until the weight is over the other foot, 0.8 s at most.
  - **The legs:** each knee's share of what the feet bear times how far it
    stands out beyond a straight leg's knee, against
    `PhysicalBody.KneeNow`. Told to tiredness (`Muscles.Legs`); sets how
    fast the hips rise (`SetRise`); over all it has, the body sinks
    (`GaveWay`).
  - **`Brace(wider, stagger)` and `Ease()`:** a planner's stance.
    `Margin`, `LeastMargin`, `MostLean`, `Steps`, `Lean`, `Inclined`,
    `LegEffort`, `Weight`, `WeightPoint`, `Presses`, `Outline`, `Mark()`.
    `Acts = false` only measures.
- **`ProceduralBiped`:**
  - **`SetLean(metres, pace)`** (hips to the right and ahead),
    **`SetTilt(degrees, pace)`**, **`Crouch(metres)`**, **`SetRise(share)`**.
  - **`SetStance(wider, stagger)`** (the feet step to it, one at a time),
    `StanceTaken`, `StanceMiddle`, `FootHome`.
  - **`StepTo(foot, point, duration, out lands)`:** a step of the body's
    own choosing; it stays where it lands.
  - **`Shift(delta)`:** the root moves without starting a gait, and the
    hips stay where they are.
  - **`BowIs(degrees)` and `LeanIs(metres)`:** what moves the body on the
    physics' clock says where it has it now. `StandsAt` then gives the
    body as last drawn, moved by what they have done since, in place of
    carrying the last movement forward. This is what makes the tool's
    physics the same at any frame rate.
  - `Sole`, `KneeOut`, `KneeOutStraight`, `LiftDue`, `Guided`, `PosedAt`,
    `LeanPosed`, `FacingNow`.
- **`PhysicalBody`:** `KneeCapacity`, `KneeNow` (200 N m for an ordinary
  leg of radius 0.06, by the cube of the leg's own); `StandsOn`.
- **`PhysicalHands`:** `Gives(hand)`, `Rides(hand)`. A link's force is
  reported as it acts on the held thing (measured with a weight hanging
  from such a link).
- **`PhysicalBack`:** leaves the hips to the balance when there is one;
  tells the body where the bow is (`BowIs`).
- **`PhysicalSwing`:** braces for the work (`StanceWider` 0.6 of the hips'
  width each side, `StanceStagger` 0.1 of their height) and eases for a
  rest. The tool's heaviness is the arms' effort over twelve steps with
  the hands where they hold at rest (thresholds 0.28 to 0.62). The arms
  follow the back by the back's own bow. Resting, it straightens as the
  tool comes up; after a rest the upper hand goes back to its own place.
  `Result.upright`: the head's speed as the tool comes down through
  upright, read between steps.
- **`MinerWorkPreview`:** the look with `K` gives the miner its balance.
- **Tests:** `PhysicalBalanceTests` (four). `PhysicalBalanceBench`
  (explicit): pulls a standing miner (`-balanceMiner`, `-balancePulls`,
  `-balanceWay`, `-balanceFor`, `-balanceAfter`, `-balanceFrames`,
  `-balanceView`). `PhysicalBench` takes `-benchBalance on|measure`. Both
  write a `BALANCE` line for every pictured frame (the feet, the weight's
  point, where the feet press), which `Art/Review/balance_clip.py` draws
  from above under the pictures. `BalancePull` gives a pull to a balance
  at every step.

## The physical body, step 7: holding and walking with the tool — October 7

- **`PhysicalCarry`** (new): how a body holds a tool it is not working
  with. Enabled in place of `PhysicalSwing` (one of the two plans at a
  time).
  - **`Asks`:** the tool's weight over `PhysicalBody.HoldOf(0)`.
  - **`Way.OneHand`** (while `Asks` is at most 0.5): the upper hand goes to
    the head's end of the handle, the lower lets go, and the tool is held
    at `AtSide` (the same place as the swing's rest, now a static helper
    here). The back stands up as the tool comes.
  - **`Way.Dragged`** (above 0.5; back to one hand under 0.3): the right
    hand goes to the lowest grip and holds the tool by that end
    (`PhysicalHands.WantEnd`), 0.8 of the tool's length above the ground
    walking (0.97 standing), under its shoulder. `stoop` grows while the
    arm is short of that (to 52 degrees), then the knees bend (to 0.12 of
    the hips' height). `Pull`: what the hand gives the tool along the
    way it walks, smoothed. `Pace` = 1 - pull / (0.3 x its weight x its
    strength x how fresh its legs are), given to `UnitMotor.SetMovementRate`.
  - **`Way.Left`:** under 0.12 of its pace for 2 s while it means to walk,
    or the tool slipped from its hand: `Leave()` drops it; `Lies` is the
    tool on the ground.
- **`PhysicalHands`:**
  - **A hand's hold has a most** (`PhysicalBody.HoldOf`): a push is scaled
    to it like the other joints, and it is part of the arm's effort
    (`Hold(hand)`).
  - **`WantEnd(hand, place)`:** one hand holds the thing by its grip and
    means that point to be at a place; no turning; the other hand lets
    go. The hand holds up its end's share of the weight (by where the
    head and the centre are, taken as resting on the ground).
    `Trailing`.
  - **Slipping:** a hand that gives more than 1.5 times its hold (its
    push and its link together) for 0.3 s lets go; from its last hand the
    thing falls (`Slipped`).
  - The arms' links are placed for where the body will be at the end of
    the step.
- **`ProceduralBiped.StandsAt`:** when the back or the balance says where
  it has the body, the body also goes on with its measured walking speed
  (`VelocityNow`).
- **`PhysicalBalance`, walking:** the hips go against a load that lasts
  by half of what it shifts the weight, and the body inclines against it
  as it does standing. It does not step while the miner walks.
- **`PhysicalSwing.TakeUp()`:** from however the tool is held, the hands
  go back to their places and the swing begins at its recovery. A swing
  is recorded only when one was made.
- **`MinerWorkPreview`:** `Carrying`, `Carry`. Sent somewhere while at
  work, the block goes, the swing is disabled and the carry enabled. `K`
  while carrying: `WorkHere()` (a block where it stands, `TakeUp`).
- **Tests:** `PhysicalCarryTests` (four); the look's test walks each
  miner with its pickaxe and puts it to work again. `PhysicalCarryBench`
  (explicit): `-carryMiner`, `-carryStrengths`, `-carryWeights`,
  `-carryWalk`, `-carryFrames`, `-carryView`.

## The physical body, step 8 (first half): the interaction click; laying down and picking up — October 7

- **`InteractionClick`** (new; added beside `MinerWorkPreview` at run
  time until the place is set up with it, step 11). Space held, or tapped
  for the next click (`Armed`); a left click picks the thing nearest the
  pointer within 48 pixels (`Pick`; a tool is preferred to the miner that
  holds it) and opens its options (`OpenOn`); `Choose(label)` gives the
  order; `Close()`. While armed or open it is the `RtsInput` interface
  blocker (`Blocks`). The things are the `HeldThing`s in the world and
  the chosen miner; the options are asked of `MinerWorkPreview`
  (`OptionsFor`, `OptionsForMiner`).
- **`PhysicalCarry`:**
  - **`LayDown()`:** from the one-hand carry, the hand takes the tool to
    the ground under its shoulder, flat (the head's points level, the
    handle the way the body faces), while `BendTo` takes the body down;
    it lets go when the grip is within 3 cm of there and the tool is
    still (`Lies`, `Way.Left`). A dragged tool is only let go.
  - **`Fetch(lying)`:** `PhysicalHands.Adopt`; the left hand under the
    head, or the right at the handle's end if the tool asks more than
    half the hand's hold. It walks to where that place will be under its
    bent shoulder (ahead by 0.8 of the shoulder's height over the hips),
    with the agent's stopping distance at 3 cm for the walk; stays;
    faces the way it came; `BendTo`; `Grasp` when within reach; then
    `Way.OneHand` or `Way.Dragged`. If the place is more than 9 cm from
    under its shoulder it steps nearer (three times at most). After 6 s
    without reaching, it leaves it.
  - **`BendTo(shoulder, place)`:** the bow to 42 degrees, then the hips
    down to 0.6 of their height, then the bow to 78; the arm stretches to
    0.96 of its length.
  - Heights are from the ground under the feet (`Ground`).
- **`PhysicalHands`:** `Adopt(lying)` (a tool in the world becomes the
  held thing with no hand on it; `Grasp` then reaches for it);
  `PlaceAlong(along)`. `Drop` takes the arms' riding mass off the tool.
- **`HeldThing`:** `Tool`, `Weight`, `Holder`.
- **`ProceduralBiped`:** `MostBowed` 80, `DeepestSink` 0.6.
- **`PhysicalBody.KneeCapacity`:** 1.6 x half the body's weight x the
  thigh's length x strength (it was by the leg's thickness).
- **`MinerWorkPreview`:** `Lying` (the pickaxe in the world), `LayDown()`,
  `PickUp(thing)`, `Rest()`, `OptionsFor`, `OptionsForMiner`, `Equip`.
  When the miner's hands are empty and it has stood up, the look ends and
  the pickaxe stays. `K` puts a lying pickaxe away before making one.
- **Tests:** `InteractionTests` (three). `PhysicalActionCapture`
  (explicit): `-actionOut`, `-actionMiner`, `-actionView`.

## The physical body, step 8 (second half): the lantern and the mug taken in hand — October 7

- **`MinerBody.Hanging`** has what a hand needs to take a thing that
  hangs by a handle: `bar` (the handle's direction in the thing's own
  bone), `grip` (its radius), `deep` and `wide` (how far the thing reaches
  below its handle, and to each side), `clear` (how far from the pelvis
  the body's side stops it when it is carried there). A thing with no
  `grip` is not taken (the satchel, Round's hammer).
- **Measured from the model, in the editor** (`MinerSetup.MeasureThings`):
  the bar is cut across at the bone's place, and the skin's crossings give
  its radius; the thing's own skin gives `deep` and `wide`; the body's
  skin (without the arms and what hangs) gives the clothes' reach to the
  side over the heights the thing hangs at from a relaxed arm's hand, and
  `clear` is that plus `wide` plus 12 mm. Creating the miners measures it;
  **Wonder Gather > Measure What Hangs On The Miners** measures it into
  the prefabs as they are, changing nothing else in them (that is how the
  three prefabs got it).
- **`MinerBody`, at run time:**
  - `Takes(k)`; `Carry(k, hand)` (a hand has it; -1: back on its hook);
    `InHand(k)`, `Taken(k)`, `Carries(hand)`; `Hook(k)`, `HookBar(k)`,
    `HookHome(k)`, `HookOut(k)`, `HangsFrom(k)`, `HangingBar(k)`.
  - In a hand the thing's bone is put at the handle's place in the closed
    fingers (`HandleIn`), passing from the hook in 0.12 s. Its pendulum is
    the same one (`Hang`), under the new place; the wrist gives with it
    (the path a thing modelled in a hand always had); it turns about the
    way it hangs until its bar lies in the fingers.
  - The stop: on its hook, as before. In a hand, the body is where both
    its front (the hook's stop) and its side (`clear`) would stop the
    thing, and it is pushed out the nearer way (`Held`).
  - `HandFree(hand)` is false for a hand with a thing in it.
- **`ThingsInHand`** (new; made for a miner when first asked, `Of(unit)`;
  null for a body with nothing to take). `Take(thing)`, `HangBack()`,
  `CanTake(thing)`; `Now` (Hung, Reaching, Lifting, Carried, Returning,
  Lowering, LettingGo), `Has`, `Busy`, `Hand`, `HangsOut`. It is a second
  guide of the arm (`IArmGuide`): at the hook the wrist is where
  `MinerBody.WristFor` puts it for the handle, corrected each frame by
  half of what the hand still lacks (for 0.1 s before it closes or lets
  go); carried, the wrist is the body's own hanging one.
- **`HungThing`** (new): the mark on the thing's bone that the interaction
  click points at.
- **`ProceduralBiped`:** `GuideAlso(guide)` (a second guide, for a hand
  the first does not guide); `FreeWrist(hand)` (where the wrist would be
  unguided); `CarryAtSide(hand, metres, keep)` (the arm hangs that much
  further out and keeps that share of its swing); `SavedProportions`.
- **`InteractionClick`** shows the chosen miner's hung things and opens
  their options; **`MinerWorkPreview.OptionsForHung`** gives them, and
  `K` or "Pick it up" with a thing in the hand has it hung back first.
- **Tests:** `HungThingTests` (two). `HungThingCapture` (explicit):
  `-hungOut`, `-hungMiner`, `-hungView`, `-hungAim`, `-hungSize`,
  `-hungEvery`, `-hungHour`.

## The physical body, step 9: any boulder, by a click — October 7

- **`Boulder`** (new). The place's boulders are marked when first asked
  for (`Boulder.All()`: every solid rock under an object called
  "Boulder"; in one order, from the west), until the place is set up with
  them (step 11). `Strike(point, outward, energy)` takes a blow: `Blows`,
  `Struck` (where each landed), `Taken` (energy since the last piece).
  When it has taken `Breaks` (90 J) a piece comes off: a `LooseStone`
  (the rock's own mesh at 9 to 14 cm, a box to lie on, 2,600 kg a cubic
  metre at half the box), thrown a little out and up; it does not touch
  the rock it came from until it is clear of it. `Stones`.
- **`RockWork.Find(boulder, swing, body, tall, from)`** gives a `Plan`:
  `spot`, `outward`, `lands` (where the head first meets the rock's
  outline on its way down), `stand`, `approach` (the place on the walked
  ground it steps from), `facing`, `height`, `away`, `aimed`. The rock is
  felt with rays from above along 12 directions (the side the body comes
  from first); spots every 9 cm inwards from its foot; six distances for
  each, aimed roughly, the best aimed in full. `RockWork.Last` says why
  the side it came from gave what it gave.
- **`PhysicalSwing`:** `AimFrom(feet, facing, point, leansUpTo, roughly)`
  gives how a blow would be taken from a place (`Aimed`: bow, lean, sink,
  miss, cost) without taking it; `Aim(point, leansUpTo)` takes it.
  `RestsUpTo` 46 (the block's limit, unchanged), `RockWork.LeansUpTo` 78,
  `ThroughAtMost` 104. `HeadAt(...)`: the head's path. `Current`: the
  swing being made. **The rest:** `HoldAsks` (what holding the tool at
  the side would ask of the shoulder now); above `RestsOnlyBelow` (0.27)
  the rest is taken with the head on the ground (`RestsOnGround`): the
  lower hand goes to the lowest grip, `PhysicalHands.WantEnd` brings the
  handle's end to hang at the side `StoodUp` (0.97) of the tool's length
  up, and the knees give what the arm lacks.
- **`UnitMotor`: off the walked ground.** `StepOff(place)` (within
  `OffAtMost`, 0.9 m, of where it leaves the walked ground): the agent is
  switched off and the unit walks there itself at `OffPace` (0.6) of its
  pace, riding as high over the ground as it did. `IsOff`, `StandsOff`.
  A move order while it is off plans from where it left the walked
  ground, brings it back there first, switches the agent on, and goes.
  `IsMoving` is true through all of it. (Why: the place's navmesh is
  baked for the default agent, radius 0.5 m, from physics colliders.)
- **`MinerWorkPreview`:** `Mine(boulder)`, `OptionsForBoulder`, `Mining`,
  `MiningPlan`, `AtRock`, `LeftRock` (why it last gave a boulder up).
  What must come first is queued (`After`): a thing hung back, the
  pickaxe picked up. Each frame at a rock (`AtTheRock`): it comes to its
  place, turns, aims and takes the pickaxe up; each swing that struck
  gives the rock its energy once; before a swing, if it has moved, it
  aims again, and goes back to its place if the spot is more than 5 cm
  out of reach.
- **`InteractionClick`** lists the boulders; a boulder is picked by the
  pointer being on it (a ray), when nothing small is near the pointer.
- **`PhysicalHands.EffortOf(hand)`:** what an arm's effort is made of
  (shoulder, elbow, wrist, hold).
- **`LooseStone.Knocked(outward, energy)`:** a blow landed on a piece
  where it lay; `Takes` (0.3) of its energy sends it off, mostly to one
  side, at `Fastest` (3 m/s) at most. `MinerWorkPreview.StruckLast` says
  what the last blow at a rock landed on.
- **Tests:** `BoulderTests` (four). `PhysicalRockBench` (explicit):
  `-rockOut`, `-rockMiner`, `-rockMine`, `-rockBlows`, `-rockFor`,
  `-rockView`, `-rockSize`, `-rockStrength`.

## The physical body, step 10: the fall, and getting up — October 7

- **`PhysicalFall`** (new; order 40: after `ProceduralBiped` poses, before
  `MinerBody` turns the bones). States: Up, Falling, Lying, Gathering,
  Rising.
  - **`LetGo()`:** what it holds is dropped (`PhysicalHands.Drop`,
    `ThingsInHand` off); the balance stops acting; eleven `Rigidbody`
    parts are made from the posed body's own segments
    (`MinerBody.Solved`) with the masses of `PhysicalBody`'s parts
    (hips, trunk, head, upper arms, forearms with hands, thighs, shins
    with boots), each moving as its segment was. Capsules, a sphere for
    the head, a box for each boot.
  - **Joints** (`ConfigurableJoint`): each is made with its part put as
    the body stands straight, so the joint's own rest is standing; then
    the part is put back. Limits per joint (waist, neck, shoulder, elbow,
    hip, knee). The engine measures a joint's bending the other way round
    from the part's own turn: the X limits are given negated.
  - **The hold** (`Holds`, every step): a slerp drive on each joint
    towards a pose, its spring giving all the joint's strength at 35
    degrees from the pose, its damper 12% of that for each radian a
    second, never more than the strength. The strengths are
    `PhysicalBody`'s (knee, back, shoulder, elbow: what is left of
    them), a hip 1.5 knees, a neck 3 times the head's own turning weight.
    `Tone`: 1 falling and gathering, 0.05 lying.
  - **Lying:** hips, trunk and head slower than 0.2 m/s for 0.5 s.
    Falling again only if one of those moves faster than 1.5 m/s.
  - **Getting up:** after 1.2 s lying it gathers (tone 1) for 0.8 to
    2.2 s; `GiveBack` puts the unit under its hips, facing as a body
    would come up, finds the crouch (`Crouch`: the posed body is posed at
    depths, and its knees' standing out read, as `PhysicalBalance` reads
    it), destroys the parts, and poses the body crouched; for 0.9 s each
    segment is between where the physics left it and the posed crouch;
    `StandUp` then lets it rise. `RoseFrom`, `LayDownAgain`, `Falls`,
    `GotUp`, `GetsUp`.
  - While let go the unit's own collider is off, it is off the walked
    ground (`UnitMotor.CarriedOff`), and its place follows its hips.
  - `Push(force, at)`: a force on the nearest part. `TakeBack()`: stood
    up at once (when the component is switched off).
- **`ProceduralBiped.LetGo`:** the body is not posed while it is set.
  **`MinerBody.Solved`:** the segments. **`PhysicalBack.Is(degrees)`:**
  the back is bowed so, now.
- **`PhysicalBalance`:** `Falls(why)` lets the body go if it has a
  `PhysicalFall`; called when two steps in a row needed more than a
  step's reach (`MostShort`), when one needed 2.5 times it
  (`MostNeeded`), when the weight's point has been outside the feet
  2.2 s (`LongestOutside`), or when the knees have given way all they can
  and are still overloaded 0.3 s. `Fell` (why). `Afresh()`: begun again.
  A push on a body that is down goes to `PhysicalFall.Push`.
- **`PhysicalCarry`** does not stand a body up while it is down.
- **`MinerWorkPreview`** gives a miner a `PhysicalFall` with its physical
  work (`Fall`). When it is down: the pickaxe it had is `Lying`, the
  block and the boulder work are left, and the look ends when it is up
  with empty hands.
- **Tests:** `PhysicalFallTests` (four). `PhysicalFallBench` (explicit):
  `-fallOut`, `-fallMiner`, `-fallShoves`, `-fallWays`, `-fallFor`,
  `-fallView`, `-fallSize`, `-fallLook strength,weight`.
  `PhysicalBalanceBench` takes `-balanceFall 1`.

## The physical body, step 11: the panel in the Ordinary Place — October 7

- **`MinerPanel`** (new; IMGUI, beside `MinerChoice` and
  `MinerWorkPreview`, which adds it where it is, as it adds
  `InteractionClick`; the scene file is unchanged). Lower left, 400 by
  176, shown while a miner is chosen and the choice is closed.
  - The strength slider (`Weakest` 0.3 to `Strongest` 3, in steps of
    0.05) and a button back to 1: `MinerWorkPreview.SetStrength`.
  - Three buttons, `Weights` 0.6, 1 and 1.8 of the miner's own pickaxe
    (`Names`: Light, Its own, Heavy), each with its kilograms
    (`PickaxeWeighs`): `MinerWorkPreview.LayPickaxe(share)`.
  - "Take away": `ClearLaid()`.
  - The line of words: `MinerWorkPreview.Status()`.
  - `Over(screen)`: whether a point of the screen is on it.
    `InteractionClick` asks it, for its own click and for the input's
    blocker (`RtsInput.SetInterfaceBlocker`).
- **`MinerWorkPreview`:**
  - **The keys are gone** (`K`, `,` `.`, `-` `=`), and its own line at the
    foot of the screen (`OnGUI`).
  - **`Toggle()`, `SetWeight()`:** the bench's block and a pickaxe made in
    the hands, kept for the tests and the captures only.
  - **`LayPickaxe(share)`:** `PhysicalHands.Make` at a place beside the
    chosen miner: 0.6 m off, then 0.9 m, to its right first and then
    round it (`LaidRound`), where a ray finds the ground (layer 6), the
    walked ground is within 0.25 m (first pass), and a box the size of
    the lying pickaxe touches nothing but the ground. Flat, its head away
    from the miner.
  - **`Mine(boulder)`** with no pickaxe in the hands: `NearestLying`,
    `PickUp`, and the order goes on when the miner holds it and stands up
    (`StandsUp`: bowed less than 5 degrees, knees less than 2 cm). None
    lying: `Say(...)`, and nothing is made. (`Given` is removed.)
  - **`OptionsForMiner`:** "Rest" at work; "Back to work" resting at its
    boulder. ("Work here", the block, is no longer offered.)
  - **Strength** is the chosen miner's at all times (`Strengthen`, when
    the choice changes; a miner no longer chosen is at 1 again).
  - **`Status()`**, made once a frame; `Say(what)` for 5 s.
- **`PhysicalHands.Make(definition, position, rotation, weight)`**
  (static): a tool as a body in the world, in no hand, with its own
  weight. `Take` makes one so and takes it.
- **`InteractionClick.Named`:** a tool says its kilograms.
- **Knees** (`PhysicalBalance`):
  - `KneesAsked(carried)`: what the knees are asked, read from the posed
    body: the further-out knee of the feet that are down, holding half of
    what the body weighs and carries, as a share of `PhysicalBody.KneeNow`
    (the measure `PhysicalFall` gets up by).
  - `Raises` 0.5: a bend the body chooses goes no deeper once each knee is
    asked half of what it has. `KneesMayBend(carried)`;
    `KneesBendTo(wanted, carried, dt)` (no deeper than it is; coming up at
    0.3 m/s while asked 0.02 more).
  - `WorkRaises` 0.33: bent to its work at a rock and ready to swing, a
    knee asked more than this is not worked with (`MinerWorkPreview`).
  - `KeepsFeet` (set by `PhysicalCarry`): no step is taken to catch the
    body while it is bent to the ground. `GaveNow` (how far the knees
    have given way), `OnLeft` (the share of the weight on the left leg).
- **`MinerWorkPreview`, at a rock:** once for each order to mine, the
  first time the swing stands ready there for 0.3 s (`kneesJudged`), it
  reads `KneesAsked`; over `WorkRaises`, it leaves the rock
  (`LeftRock`: "its knees would be bent too deep for the work"), stands
  at ease with its pickaxe, and says so.
- **`RockWork.Low`** is 0.14 m (it was 0.08): no spot nearer the ground
  is struck.
- **`PhysicalCarry`**, in all it does: its feet come together
  (`PhysicalBalance.Ease`) only once it stands up (knees bent less than
  0.05 of its hips' height, bowed less than 15 degrees); and it sets
  `KeepsFeet` from when its knees are bent more than a quarter of its
  hips' height until it has stood up again or is sent somewhere.
- **`PhysicalCarry`**, going for a tool (`Take`):
  - Arrived, it turns and bows (`FirstBow` 42) with its knees straight.
    Bowed and turned (within 3 degrees), if the grip is more than 9 cm
    from under its shoulder it steps nearer. `feetSet` when both feet
    have been down, none due to be put in its place, for 0.12 s (or after
    2.5 s). Only then do the knees bend (`BendTo(..., kneesToo)`), and it
    turns no more.
  - `BendTo`: the knees bend while `KneesAsked` is under `Raises`; then
    the back bows on, and the knees come up if the bowing asks more of
    them.
  - It gives the tool up: bent all it can and 1 cm short for 1.2 s
    (`GivesUpShort`); its knees stopped under 0.7 of the deepest bend and
    what lacks is more than the bowing left could give, for 0.3 s
    (`beyond`); its knees giving way under it by 0.12 of its hips' height
    (`GivesUnder`); or 6 s. `NotTaken` says which; `MinerWorkPreview`
    says it when the look ends.
  - Dragging: the knees by `KneesBendTo`.
- **`PhysicalSwing`**, resting with the head on the ground: the knees by
  `KneesBendTo`.
- **`PhysicalFall.Follow`:** nothing, if its parts are gone (the place
  being put away while a miner is down).
- **Tests:** `MinerPanelTests` (five). `MinerPanelPicture` (explicit; the
  screen itself, so it is run with a window, without `-batchmode`).
  `MinerPanelBench` (explicit):
  `-panelOut`, `-panelMiner`, `-panelStrengths`, `-panelWeights`,
  `-panelPickaxeOf`, `-panelBoulder`, `-panelFor`, `-panelBlows`,
  `-panelSize`, `-panelView`, `-panelOrder pick`, `-panelPickAt`,
  `-panelSend seconds`, `-panelTrace n`. `PhysicalRockBench` takes `-rockWeight` and
  `-rockPickaxeOf`, and puts the pickaxe on the ground as the panel does.
  `BoulderTests`, `InteractionTests` and `PhysicalFallTests` changed with
  the step (below, in Validation).

## The physical body, step 12: evidence — October 8

- **`MinerWorkBenchmark`** (new; added where the miners' physical work is,
  as the panel is). Started with `-wgwork`: each miner in turn is
  measured standing (6 s), has its own pickaxe put on the ground, is told
  to mine the nearest boulder whose top is 0.5 m or more over it, and is
  measured at its work (12 s) from the same close view; `PhysicalHands`
  times its own step meanwhile. It writes `miner-work-benchmark.csv`
  beside the player log, and quits. (`-wgcrowd`, the walking crowd, is
  `MinerCrowdBenchmark`'s, as before.)
- **`PhysicalBalance`:** how long the weight has been outside the feet
  (`outside`, which lets the body go after 2.2 s) is counted only while
  it stands.
- **`PhysicalSwing.KneesAtMost`:** no way of taking a blow that bends the
  knees further is considered (`AimFrom`, and so `RockWork.Find`).
  **`MinerWorkPreview`:** where the knees are asked too much at a rock
  (`WorkRaises`), it sets `KneesAtMost` to 0.65 of what the plan bent
  them, finds a plan again, and goes to it; up to three times for an
  order; then it gives the rock up.
- **Tests:** `EvidenceTests` (one: a pickaxe and a lantern followed frame
  by frame). `BoulderTests.ABodyDoesNotWorkWhereItsKneesWouldBeBentTooDeep`
  now takes either end: a way to stand that the knees bear, or the rock
  given up. `MinerPanelBench` takes `-panelFrames` (the frames a second
  it runs at) and `-panelBoulder -1` (from where the place puts the
  miner, the nearest boulder of ordinary height).

## The playtest round, first part: the look at every distance; how a body walks, turns and rests — October 8

What Luis's ten notes changed so far
([the round's page](../Reviews/2026-10-08_ThePlaytestRound.md); the rules
themselves are in
[the design](../Design/ThePhysicalBody.md#after-luiss-play-the-walk-the-turn-and-the-rest-october-8)).

- **`Fireflies` (Scripts/Look) and `WGFireflies.shader`.**
  - Each firefly has a home (`Homes`: xyz on the ground, w the number it
    is told apart by), found once from `OrdinaryGround.GrassDensity` and
    `Height`, one to `meadowEach` square metres. They go to the shader
    in a `GraphicsBuffer` (`_WG_FireflyHomes`).
  - The shader roams each round its home and decides what is seen from
    the eye's distance (`_WG_FireflySight`, `_WG_FireflyOut`).
    `ShareSeen(distance)` and `Brightness(distance)` give the same in
    code, for tests.
  - The scene still carries the old fields (`count`, `extent`,
    `farExtent`, `floor`); Unity ignores them.
- **`ShadowReach` (Scripts/Look),** added by `TimeOfDay` when it plays.
  - Before each camera draws, it sets the pipeline's shadow distance
    beyond all that a lit, shadow-casting lamp lights (`Needed(eye,
    border)`), scales the cascade splits so the nearer cascades keep
    their metres, and puts everything back after the camera has drawn.
    It looks for the place's lamps once a second.
  - `TimeOfDay.LampShadows` (key `0` in `LookDevControls`): the spot
    lamps of the house are shaded or not (their `shadowStrength` 1 or
    0). Point lamps (the fire inside) keep their shadows.
- **`ProceduralBiped`.**
  - **The hips' height** is one line: `Reachable(...)` for any two
    ankles; `LandingAnkle(foot)`; `LandsIn(foot)` (to the frame the foot
    lands in); from `ComesDownFrom` of a swing the hips come down along
    `Comes(from, pace, to, lasts, u)` to where both legs will reach;
    they rise by `SmoothDamp` with a pace that is kept (`hipPace`).
  - **A foot in the air:** where it is going (`foot.to`) follows its
    target by `SmoothDamp` (`AimsIn`); the way round the standing boot
    is looked for along the rest of its path (`LooksAlong`) and reached
    at a pace (`foot.round`), with a hard clear after it; the lift
    gathers pace over `LeavesOver` of the swing.
  - **Turning:** `MayFace(from, to)` (no further than `MostTwist` round
    from a planted foot); `MeansToFace(degrees)` and `LandsFacing(foot)`
    (a stepping foot lands turned on by up to `OpensBy`); in `Settle`
    the foot on the side of the turn steps first, quicker
    (`TurningStep`); a landing boot is turned less, or set a little
    aside (`StepsAside`), before it is ever laid across the other.
  - **The head** looks along `agent.desiredVelocity` (`LooksRound`,
    `LooksIn`); the chest turns `ChestLooks` of that.
- **`UnitMotor`.** The agent no longer turns the body
  (`updateRotation = false`). `Steer` turns it towards
  `agent.desiredVelocity` by `SmoothDampAngle`, through `Turned(...)`
  (which tells the body where it means to face and takes what its feet
  allow), and holds its going back while it is turned from its way
  (`agent.velocity` clamped; `agent.speed` is left alone). `Face` and
  the way off the walked ground turn through `Turned` as well.
- **`PhysicalBody.Effort(muscles)`:** what each group gave at the last
  step.
- **`PhysicalSwing`:** one rest (the tool at the side); `NoRest`;
  `HoldAsks` counts the hold as well as the shoulder. **`PhysicalCarry`:**
  `Aside(body)`, and the arm hangs at 0.995 of its length; a tool it can
  only hold by its end, told to be laid down, is let go.
  **`MinerWorkPreview`:** ends the work when the tool cannot be rested
  with or has slipped from the hands, and says why.
- **Benches** (explicit tests): `DuskZoomCapture` (`-zoomOut`, `-zoomDark
  1`, `-zoomShaded 1`, `-zoomFlies name=value`); `MotionTraceBench`
  (`-traceOut`, `-traceWhat walk,turn,turns`, `-traceSize`,
  `-traceEvery`, `-traceView`); `MinerPanelBench -panelMuscles n`.
  **`Art/Review/motion_breaks.py`** reads the traces;
  **`Art/Review/judges/`** holds the judges' briefs.

## The playtest round, second part: going down for a tool; a walker sent back; drawn between the physics' steps — October 8

What changed in the code
([the round's page](../Reviews/2026-10-08_ThePlaytestRound.md); the rules,
with the settings, are in
[the design](../Design/ThePhysicalBody.md#after-luiss-play-second-part-going-down-for-a-tool-a-walker-sent-back-and-what-is-drawn-between-the-physics-steps-october-8)).

- **`PhysicalCarry`.** `BendTo(hips, posture, shoulder, place, dt,
  further)` is one movement on one number (`down`), aimed by a model of
  the body (`ShoulderAt`); `Take` no longer bows to look; the reach is
  led (`lacksFrom`); `KeepsItsFeet()`; `Stands()`; the lay-down follows
  the body (`laid`), opens the hand (`OpensIn`) and stands up eased
  (`rising`). The stand place is 0.6 of the trunk's height ahead (was
  0.8), and the miner walks the way it will face.
- **`PhysicalHands`.** `GraspLed`, `Lead`, `Withdraw`, `Open`; a reach is
  drawn between the physics' steps (`reachWas`, `reachStepped`).
- **`ProceduralBiped`.**
  - `Regard(point)` / `RegardNothing()`; `StaysPut`; `Stopping`;
    `StepSize`; `PaceItsLegsAllow(way)`; `BowBy(degrees, seconds)`;
    `SetLeanAt(metres, pace)` (the balance's own pace) beside `SetLean`
    (now eased).
  - `Sink` is eased (`SinksIn`, `SinkFalls`); a bow told only where to
    go is eased (`BowsIn`).
  - A free arm: `hangs` (the level frame under the chest), `KeptOut`,
    and for a body with no worker `handHang` (eased from its shoulder).
    `freeWrist` is where it would hang, before any easing.
  - The swing: `GoesRound` and `NotTouching` replace the bow
    (`foot.round`, `foot.bow` are left unused); `Retarget` keeps the
    landing place clear by both.
  - A stopping body that turns lifts its inside foot first; a body that
    `StaysPut` stands as its last stride left it.
- **`PhysicalBalance`.** While `KeepsFeet` the hips are placed
  (`HipsComeIn`, `HipsGoAtMost`), not let fall and caught; `Slowing`
  eases the lean to the end of its way; walking, the lean eases from the
  pace it had (`leanGoes`).
- **`PhysicalBack`.** `BowBy` in place of a pace; a tool only gone for
  is not counted as held.
- **`UnitMotor`.** `Steer`: no turn while it has pace the other way or
  is `Stopping`; `mayGo` (the pace along its way comes down at `Slows`);
  the legs' pace; `ComeToRest()`; `MovingHow`.
- **`MinerBody`.** `Drape` is by weight (`flapHangs`, `flapDown`,
  `flapInside`, `thighFront`, `Stopped`, `ThighAlong`, `ThighOut`);
  `Swing` and `Out` (the two angles) are gone.
- **`MinerWorkPreview`.** Why a rock was given up is cleared when a rock
  is taken up; "sent somewhere" says how the miner was moving.
- **Tests.** `MotionTraceBench`: `-traceWhat pick`; columns for the bow,
  hands, shoulders, tool, head and chest pitch, lean (drawn and the
  balance's), where each foot means to land, its true place and where it
  left from; a reversal ends with a stop as a walk stops.
  `BoulderTests.AMinerMinesABoulderByAClick` waits for the miner to be
  back at its work before it offers a rest, and for its next blow not
  counting a rest it takes of its own accord.
  `MinerTests.InASharpTurnTheSwingingFootGoesRoundTheStandingOne` says
  when the boots were closest.
- **`Art/Review`.** `motion_jolts.py`, `motion_frames.py`,
  `motion_dips.py`.
- **After the judges of the pick-up (the third part).**
  `PhysicalCarry.AtSide` pitches the tool by `HandleHangs` or as the
  ground allows (`HandleClears`); `BackLeads`; `ProceduralBiped.
  HangsAtSide(hand, on)` (set by `PhysicalCarry`, `PhysicalSwing` at
  rest, and `ThingsInHand` carrying); `MinerBody.Drape` takes the
  thigh's line in the pelvis' own fore-and-aft plane (the outer-side
  stop is gone); `ThingsInHand.Carries` (`ForearmRaised`, `UpperHangs`,
  plus `HangsOut`), and it calls `Regard`. `HungThingTests` asks for the
  new hold.
- **Getting up after a fall (the fourth part, October 9).**
  `PhysicalFall`: `Way`, `Stage`, `Asks`/`Asked`, `Measure(Asks)`,
  `Ways` (default `Usual()`), `Rouse()`, `Advance(dt)` (the step that
  `FixedUpdate` takes; a search that steps the physics itself calls it),
  `LieStill()`, `Fell()`, `Gives`/`Through` (for the search),
  `GaveUpGetting`, `GotUpTheOldWay`; `GiveBack` sets the posed stance
  from where the let-go feet are planted (`plantAt`).
  `Tests/PlayMode/GetUpSearch.cs` (explicit; `-searchOut`,
  `-searchWays`, `-searchWay`, `-searchFor front|kneel|crouch|squat|
  seat|feet|rise`, `-searchMiner Small,Long,Round`, `-searchStarts`,
  `-searchRounds`, `-searchMany`, `-searchKeep`, `-searchSpread`,
  `-searchSame`, `-searchFrom`, `-searchHurry`, `-searchKicks`): sets
  `Physics.simulationMode` to `Script` while it runs and puts it back.
  `PhysicalFallBench`: `-fallLays`, `-fallScripts`, `-fallEvery`,
  `-fallFollow`; `Lay(...)` and `Read(...)` are shared with the search
  and the tests. `PhysicalFallTests.LaidDownAnyWayItGetsUpByItsOwnStrength`.
  `MinerWorkPreview.PushOver()`/`PushOver(way)`/`CanPushOver`, the
  button on `MinerPanel`, and
  `PhysicalFallTests.PushedOverFromThePanelItFallsAndGetsItselfUp`.
