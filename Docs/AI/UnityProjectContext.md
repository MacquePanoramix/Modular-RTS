# Unity project context

Analyzed September 6, 2026. New local repository; the initial implementation is recorded in Git. See Docs/Validation.md for the completed compile, three passing PlayMode tests, Windows build and inspected scene preview.

## Confirmed foundation

- Unity 6000.3.12f1, installed under Program Files/Unity/Hub/Editor.
- Assets, Packages and ProjectSettings form a new project under this repository root.
- Universal 3D template extracted from the installed editor's bundled template archive. GraphicsSettings references its URP asset; QualitySettings preserves template pipeline choices.
- Input System enabled (`activeInputHandler: 1`); runtime owns one programmatic Gameplay action map.
- Package pins: AI Navigation 2.0.0, Input System 1.17.0, URP 17.3.0, Test Framework 1.4.2, uGUI 2.0.0, Visual Studio integration 2.0.22. The bundled template's Input System 1.12.0 failed compilation against the installed editor (removed BuildTarget.ReservedCFE API); 1.17.0 replaces it. Unity resolved the template's older URP request to 17.3.0; the manifest now pins that resolved version explicitly. The resolved lockfile is authoritative after import.
- No callable Unity MCP provider was exposed in this task. Local Unity batch execution is available.
- No existing game repository or project was found in the task or Documents/Codex search. The project is newly created, not an edit of an existing user project.

## Code and ownership

All first-party runtime code is in Assets/_WonderGather/Scripts, namespace WonderGather, assembly WonderGather.Runtime. Unity.InputSystem, Unity.AI.Navigation and Unity.ugui are explicit assembly references.

RtsInput owns actions. SelectionController owns a single current selection and dispatches commands. MoveCommand contains destination intent. UnitMotor validates and applies complete NavMesh paths. RtsCamera owns a smoothed position, angle and zoom. SelectableUnit controls its visual ring. WandererHud shows temporary playtest instructions and order status.

Editor-only WonderGather.Editor contains scene creation, preview and Windows build entry points. Tests live in their own PlayMode test assembly. No networking, save system, economy or animation framework exists yet.

## Startup and assets

WandererSetup.Create creates TheWanderer scene, Wanderer prefab, materials and baked NavMesh. It sets TheWanderer as the only enabled build scene. The scene wires all references explicitly. Layer 6 is Walkable and distinguishes commandable ground from obstacles and units. The camera world mask includes Default and Walkable.

Use private serialized fields, PascalCase types/methods, explicit dependency wiring and small components. New scene creation must not overwrite an existing scene. Preserve Unity-generated metadata and the package lock in Git.

## Validation and limits

Read Docs/Validation.md for final execution results. PlayMode tests cover routing around the obstacle wall, rejection of disconnected/out-of-range/nonfinite destinations, and selection disable lifecycle. Manual checks are in Docs/Playtest.md. Desktop mouse/keyboard and flat terrain are the current scope. Camera feel and art direction require user playtesting.

Source evidence: ProjectSettings/ProjectVersion.txt, GraphicsSettings.asset, QualitySettings.asset, ProjectSettings.asset, Packages/manifest.json, first-party runtime/editor/test sources and recovered conversation text. The referenced PDF dossier was not available as file contents.
