# Wonder Gather

A first playable foundation for a slow, fully 3D RTS about civilizations designed by their players.

> **Project signpost:** start with [Current State](Docs/CURRENT_STATE.md) for what is happening now.
> [Game Vision](Docs/GAME_VISION.md) remains the design source of truth;
> [Design Rationale](Docs/DESIGN_RATIONALE.md) preserves the small set of "whys"
> that would be dangerous to lose; [Project Culture](Docs/PROJECT_CULTURE.md)
> explains lightweight collaboration roles and context routing.

Current milestone: **The Living Worker**, integrating procedural workers
with the faction creator's gathering, carrying and delivery loop. Scope
approved September 26; ready for Game Director playtesting. Open
`Assets/_WonderGather/Scenes/TheFactionCreator.unity` and launch its playtest.
See [Living Worker playtest](Docs/LivingWorkerPlaytest.md) and
[validation evidence](Docs/Validation.md).

Local standalone build: `Builds/WindowsLivingWorker/WonderGather.exe`.
The full 67-test PlayMode suite, rendered probe and Windows build passed.
Earlier local builds retain their previously built behavior.

The earlier movement comparison remains in `TheLivingBody.unity`. See
[Living Body playtest](Docs/LivingBodyPlaytest.md). Creator performance controls
remain available; see [Unit performance playtest](Docs/UnitPerformancePlaytest.md).

> Previous milestone: **Faction saving and library**. Open
> `Assets/_WonderGather/Scenes/TheFactionCreator.unity`, or run
> `Builds/WindowsFactionCreator/WonderGather.exe`.
> See [Faction library playtest](Docs/FactionLibraryPlaytest.md).

> Previous milestone: **Player-facing faction creator**. Open
> `Assets/_WonderGather/Scenes/TheFactionCreator.unity`, or run the local
> `Builds/WindowsFactionCreator/WonderGather.exe` build.
> See [Faction creator playtest](Docs/FactionCreatorPlaytest.md).

> Previous milestone: **Civilization blueprints**. Open
> `Assets/_WonderGather/Scenes/TheCivilization.unity`.
> See [Civilization playtest](Docs/CivilizationPlaytest.md) for editable assets,
> the provisioned sample, and validation limits.

## Open and play

1. In Unity Hub, add this folder as an existing project.
2. Open it using Unity **6000.6.0f1** (installed locally).
3. Open `Assets/_WonderGather/Scenes/TheFactionCreator.unity` and press Play.
4. Edit or load a faction with workers allowed to gather, then launch its playtest.
5. Select a worker and right-click the supply station marked by the green sphere.
6. Focus with F and zoom close to watch its feet, collecting gesture and carried
   bundle, then follow its delivery to the blue base.

The creator retains unit/building blueprints, production links and performance
controls. Save before stopping Unity Play Mode. The Living Worker guide covers
interrupted orders, shared work positions, production and save compatibility.

See [Building network playtest](Docs/BuildingNetworkPlaytest.md) for a
concrete construction and mixed-production test. Earlier scenes remain available for comparison.

For this integrated playtest without Unity, run
`Builds/WindowsLivingWorker/WonderGather.exe` with its data folders.
GitHub contains source; generated caches and Windows builds remain local.
Use a short local folder path when cloning. The committed assets contain
the integration; no setup menu command is needed to play.

| Control | Action |
|---|---|
| WASD or arrow keys | Pan |
| Q / E | Rotate |
| Mouse wheel | Smooth zoom |
| Left click | Select; empty ground clears selection |
| Shift + left click | Toggle a unit in the selection |
| Left drag / Shift + left drag | Box-select / add boxed units |
| Right click | Move on terrain; gather/deliver in economy scenes |
| B (economy scenes) | Place a workshop; left-click confirms valid ground |
| Left-click workshop / T (economy scenes) | Select workshop / queue a worker |
| Escape | Cancel placement / deselect |
| F | Center camera on selected group |

## First slice

- Smoothed, bounded RTS camera with a closer viewing angle when zoomed in.
- Owned Input System action map with balanced enable/disable/disposal.
- Selection ring, destination marker and order feedback.
- Move commands separated from mouse input and navigation execution.
- Movement searches for a reachable alternative when the requested destination is blocked or disconnected; non-finite orders preserve the current route.
- Workers collect finite supplies, carry at most five, deposit them, and repeat. Movement interrupts work while preserving cargo.
- Reusable Wanderer prefab; The Group scene contains eight units with click, Shift-click, and box selection.
- Group movement validates every destination before issuing the order; agents use local avoidance and separate arrival slots.
- Automated PlayMode coverage for obstacle routing, invalid destinations and selection lifecycle.

The Gatherer inherits the preferred 0.005 zoom. Camera tuning is on **RTS Camera → Rts Camera**. Unit movement tuning is on the **Wanderer prefab → Nav Mesh Agent**. Placeholder geometry and colors establish readable testing conditions, not a final art direction.

## Technical structure

`RtsInput → SelectionController → CommandDispatcher → GroupMoveCommand → UnitMotor → NavMeshAgent`

`RtsCamera` consumes input and selected-unit position independently. `SelectableUnit` owns selection presentation. `WandererSetup` is editor-only scene authoring. Runtime and test assemblies have separate boundaries. There is no static game state or scene-wide lookup in production update loops.

The project began with the Universal 3D template bundled with the installed editor. URP settings originate from that template. Unity resolves packages in `Packages/packages-lock.json`; commit this lock and all asset metadata. Do not commit Library, Temp, Logs or local IDE files.

Start with `Docs/GAME_VISION.md`, the living source of truth for locked decisions, current direction, possible ideas, and open questions. `Docs/Playtest.md` contains acceptance checks, and `Docs/Validation.md` records execution evidence and limitations.
