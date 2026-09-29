# Unity project context

Updated September 29, 2026 after Equipped Worker implementation/validation.
Current playtest: Docs/EquippedWorkerPlaytest.md; evidence: Docs/Validation.md.
The creator offers supplies and equipment maps; faction saves are version 5.
Sections describe successive extensions; later sections supersede earlier limits.

Automation preparation, September 28, 2026: the offline Python controller in
`.automation/` is separate from the Unity assemblies and does not launch the
Editor. Its local tests and limitations are in `Docs/AutomationBootstrap.md`.
Repository configuration at `5f613512` confirms the Unity/package versions
below; Unity runtime, Editor connectivity and builds were not revalidated for
this infrastructure draft. The current gameplay milestone is unchanged.

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

Read Docs/Validation.md for final execution results. PlayMode tests cover routing around the obstacle wall, rejection of disconnected/out-of-range/nonfinite destinations, and selection disable lifecycle. Manual checks are in Docs/Playtest.md. Desktop mouse/keyboard and flat terrain are the current scope. Camera feel and art direction require user playtesting.

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

Current playtest: Docs/BuildingNetworkPlaytest.md. Windows build entry remains
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
Current guide: Docs/UnitPerformancePlaytest.md.


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
supersedes the earlier separation between faction workers and procedural bodies. Current guide: Docs/LivingWorkerPlaytest.md;
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
for the bounded scope and source evidence. Docs/WorkerShowcaseVision.md records
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

See Docs/EquipmentArchitecture.md for the current ownership/contact/save contracts.
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
