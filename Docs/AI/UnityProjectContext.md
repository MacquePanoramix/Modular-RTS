# Unity project context

Updated September 9, 2026 for civilization blueprints. See Docs/Validation.md for execution evidence and Docs/FactionCreatorPlaytest.md for the current playtest.

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
