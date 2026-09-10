# Unity project context

Updated September 10, 2026 for building networks. See Docs/Validation.md for execution evidence and Docs/BuildingNetworkPlaytest.md for the current playtest.

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

Editor-only WonderGather.Editor contains scene creation, preview and Windows build entry points. Tests live in their own PlayMode test assembly. Gatherer owns a small work state machine and cargo; ResourceNode owns remaining supplies and ResourceDepot owns stored supplies. GatherCommand and ReturnSuppliesCommand enter through CommandDispatcher. No networking, save system or animation framework exists yet.

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
