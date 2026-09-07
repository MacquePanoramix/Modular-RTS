# Wonder Gather — The Group

A first playable foundation for a slow, fully 3D RTS about civilizations designed by their players.

## Open and play

1. In Unity Hub, add this folder as an existing project.
2. Open it using Unity **6000.6.0f1** (installed locally).
3. Open `Assets/_WonderGather/Scenes/TheGroup.unity` and press Play.
4. Drag a box around the gold capsules, then right-click across the wall. The group should route around it and arrive at separate destinations.

See `Docs/GroupPlaytest.md` for Shift-click, box selection, group movement, and the current playtest. The earlier `TheWanderer` scene remains available for comparison. The current standalone build is `Builds/WindowsGroup/WonderGather.exe`.

For a quick playtest without opening Unity, run `Builds/WindowsGroup/WonderGather.exe`. Keep the executable together with its data folders. GitHub contains the source; generated caches and Windows builds remain local. Use a short local folder path when cloning.

If the scene has not yet been generated, use **Wonder Gather → Create Wanderer Scene**. This command creates the scene, unit prefab, materials, baked navigation and build scene entry. It will not overwrite an existing Wanderer scene.

| Control | Action |
|---|---|
| WASD or arrow keys | Pan |
| Q / E | Rotate |
| Mouse wheel | Smooth zoom |
| Left click | Select; empty ground clears selection |
| Shift + left click | Toggle a unit in the selection |
| Left drag / Shift + left drag | Box-select / add boxed units |
| Right click | Order selected group to spaced destinations |
| Escape | Deselect |
| F | Center camera on selected group |

## First slice

- Smoothed, bounded RTS camera with a closer viewing angle when zoomed in.
- Owned Input System action map with balanced enable/disable/disposal.
- Selection ring, destination marker and order feedback.
- Move commands separated from mouse input and navigation execution.
- NavMesh movement that accepts only complete routes. Invalid orders preserve the current route.
- Reusable Wanderer prefab; The Group scene contains eight units with click, Shift-click, and box selection.
- Group movement validates every destination before issuing the order; agents use local avoidance and separate arrival slots.
- Automated PlayMode coverage for obstacle routing, invalid destinations and selection lifecycle.

Camera tuning is on **RTS Camera → Rts Camera**. Unit movement tuning is on the **Wanderer prefab → Nav Mesh Agent**. Placeholder geometry and colors establish readable testing conditions, not a final art direction.

## Technical structure

`RtsInput → SelectionController → CommandDispatcher → GroupMoveCommand → UnitMotor → NavMeshAgent`

`RtsCamera` consumes input and selected-unit position independently. `SelectableUnit` owns selection presentation. `WandererSetup` is editor-only scene authoring. Runtime and test assemblies have separate boundaries. There is no static game state or scene-wide lookup in production update loops.

The project began with the Universal 3D template bundled with the installed editor. URP settings originate from that template. Unity resolves packages in `Packages/packages-lock.json`; commit this lock and all asset metadata. Do not commit Library, Temp, Logs or local IDE files.

Start with `Docs/GAME_VISION.md`, the living source of truth for locked decisions, current direction, possible ideas, and open questions. `Docs/Playtest.md` contains acceptance checks, and `Docs/Validation.md` records execution evidence and limitations.
