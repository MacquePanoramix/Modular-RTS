# Unity project context

Analyzed September 6, 2026. New local repository; the initial implementation is recorded in Git. See Docs/Validation.md for the completed compile, three passing PlayMode tests, Windows build and inspected scene preview.

## Confirmed foundation

- Unity 6000.6.0f1, installed under Program Files/Unity/Hub/Editor. The project was migrated from its initial Unity 6000.3.12f1 baseline while still at the first prototype stage.
- Assets, Packages and ProjectSettings form a new project under this repository root.
- Universal 3D template foundation migrated to Unity 6.6. GraphicsSettings references its URP asset; QualitySettings preserves the template pipeline choices through Unity's serialized-settings upgrade.
- Input System enabled (`activeInputHandler: 1`); runtime owns one programmatic Gameplay action map.
- Package pins after the Unity 6.6 migration: AI Navigation 2.0.14, Input System 1.20.0, URP 17.6.0, Test Framework 1.8.0, uGUI 2.6.0, and Visual Studio integration 2.0.26. Unity 6.6 also added its required Physics Core 2D, TetGen, and Timeline Foundation modules. The resolved lockfile is authoritative after import. The original 6.3 setup required an Input System update from the template's incompatible 1.12.0 package; that historical issue remains resolved.
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
