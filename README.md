# Wonder Gather

A first playable foundation for a slow, fully 3D RTS about civilizations designed by their players.

> **Project signpost:** start with [Current State](Docs/CURRENT_STATE.md) for what is happening now.
> [Game Vision](Docs/GAME_VISION.md) remains the design source of truth;
> [Design Rationale](Docs/DESIGN_RATIONALE.md) preserves the small set of "whys"
> that would be dangerous to lose; [Project Culture](Docs/PROJECT_CULTURE.md)
> explains lightweight collaboration roles and context routing.

**Current milestone:** Strength and Burden ([plan](Docs/NextMilestonePlan.md)).
**Checkpoint A, the grounded body,** is ready for Luis's playtest. It adds:

- gait scaled to the body and a jog;
- heel/toe feet and weight transfer;
- a natural 1.8 m/s default walk.

See [the grounded body playtest](Docs/GroundedBodyPlaytest.md). The pickaxe
swing is unchanged until Checkpoint B.

Latest milestone: **The Equipped Worker**, connecting a blueprint-selected
pickaxe to reachable procedural strikes and real resource extraction. Open
`Assets/_WonderGather/Scenes/TheFactionCreator.unity`, choose **Pickaxe** under
the worker's **Equipped tool**, then select **Playtest equipment →**.
See [Equipped Worker playtest](Docs/EquippedWorkerPlaytest.md),
[equipment architecture](Docs/EquipmentArchitecture.md) and
[validation evidence](Docs/Validation.md). Luis accepted it on September 30 as
the bare starting point; its motion quality is the next thing to improve.

The full 85-test PlayMode suite, 18 focused equipment/persistence tests,
reviewed rendered probe and Windows build passed. Local standalone:
`Builds/WindowsEquippedWorker/WonderGather.exe`. The ordinary **Playtest faction →**
map retains the Living Worker's supply collection. Older faction files open
with None; explicit saves upgrade to version 5 with the existing backup flow.

The longer-term reference is a [deep, polished one-worker showcase](Docs/WorkerShowcaseVision.md).
Its limited scene scale does not limit the intended depth of character and
civilization creation. Tool shape, poses, fixed hit yield and back stow are
provisional. Strength-dependent handling and material transport aids follow
this contact proof and Luis's feedback.

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
4. Edit or load a faction, enable gathering, choose Pickaxe and launch **Playtest equipment →**.
5. Select a worker and right-click the grey mineral boulder.
6. Focus with F and zoom close to watch preparation, contact and recovery,
   then follow cargo transport and delivery to the blue base.

The creator retains unit/building blueprints, production links and performance
controls. Save before stopping Unity Play Mode. The Equipped Worker guide covers
interrupted strikes, shared work positions, production and save compatibility.

See [Building network playtest](Docs/BuildingNetworkPlaytest.md) for a
concrete construction and mixed-production test. Earlier scenes remain available for comparison.

For this integrated playtest without Unity, run
`Builds/WindowsEquippedWorker/WonderGather.exe` with its data folders.
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
