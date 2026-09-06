# Wonder Gather — The Wanderer

A first playable foundation for a slow, fully 3D RTS about civilizations designed by their players.

## Open and play

1. In Unity Hub, add this folder as an existing project.
2. Open it using Unity **6000.6.0f1** (installed locally).
3. Open `Assets/_WonderGather/Scenes/TheWanderer.unity` and press Play.
4. Click the gold capsule, then right-click the meadow. The unit should route around the stone wall.

For a quick playtest without opening Unity, run `Builds/Windows/WonderGather.exe` or extract the accompanying Windows archive and run its executable. Keep the executable together with its data folders. The source archive excludes generated caches and the build. If extracting it elsewhere, prefer a short local folder path.

If the scene has not yet been generated, use **Wonder Gather → Create Wanderer Scene**. This command creates the scene, unit prefab, materials, baked navigation and build scene entry. It will not overwrite an existing Wanderer scene.

| Control | Action |
|---|---|
| WASD or arrow keys | Pan |
| Q / E | Rotate |
| Mouse wheel | Smooth zoom |
| Left click | Select; empty ground clears selection |
| Right click | Order selected unit to move |
| Escape | Deselect |
| F | Center camera on selected unit |

## First slice

- Smoothed, bounded RTS camera with a closer viewing angle when zoomed in.
- Owned Input System action map with balanced enable/disable/disposal.
- Selection ring, destination marker and order feedback.
- Move commands separated from mouse input and navigation execution.
- NavMesh movement that accepts only complete routes. Invalid orders preserve the current route.
- Reusable Wanderer prefab; selection currently holds one unit at a time.
- Automated PlayMode coverage for obstacle routing, invalid destinations and selection lifecycle.

Camera tuning is on **RTS Camera → Rts Camera**. Unit movement tuning is on the **Wanderer prefab → Nav Mesh Agent**. Placeholder geometry and colors establish readable testing conditions, not a final art direction.

## Technical structure

`RtsInput → SelectionController → CommandDispatcher → MoveCommand → UnitMotor → NavMeshAgent`

`RtsCamera` consumes input and selected-unit position independently. `SelectableUnit` owns selection presentation. `WandererSetup` is editor-only scene authoring. Runtime and test assemblies have separate boundaries. There is no static game state or scene-wide lookup in production update loops.

The project began with the Universal 3D template bundled with the installed editor. URP settings originate from that template. Unity resolves packages in `Packages/packages-lock.json`; commit this lock and all asset metadata. Do not commit Library, Temp, Logs or local IDE files.

Start with `Docs/GAME_VISION.md`, the living source of truth for locked decisions, current direction, possible ideas, and open questions. `Docs/Playtest.md` contains acceptance checks, and `Docs/Validation.md` records execution evidence and limitations.
