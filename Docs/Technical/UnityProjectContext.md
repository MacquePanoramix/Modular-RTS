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
([NextMilestonePlan.md](../NextMilestonePlan.md#s1d-last-part--the-miners-at-work-plan-waiting-for-luiss-choices);
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
