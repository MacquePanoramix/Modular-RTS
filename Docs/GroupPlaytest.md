# Prototype 1.2 — The Group

Open `Assets/_WonderGather/Scenes/TheGroup.unity` in Unity 6000.6.0f1, press Play, and click the Game view. Eight Wanderers start on the meadow. The earlier `TheWanderer` scene remains available.

| Control | Result |
|---|---|
| Left click | Replace selection with one unit; empty ground clears it |
| Shift + left click | Add or remove that unit |
| Left drag | Select unit centers inside the box, in either drag direction |
| Shift + left drag | Add boxed units without removing existing selection |
| Right click ground | Move selection into separate destination slots |
| Escape | Clear selection and cancel a drag |
| F | Focus the center of the selected group |
| WASD / arrows, Q/E, wheel | Pan, rotate, and zoom |

Try these checks:

1. Click one unit, Shift-click two more, then Shift-click one again. Check rings and selected count.
2. Drag a box around part of the group in both directions. Shift-drag a second box, including some already selected units.
3. Select all eight and order them across the wall. They should bend around obstacles and settle into separate spots.
4. Give a second order while moving. Check that everyone responds and the new destination marker appears.
5. Order the group near the map edge or onto the isolated island. If the entire formation cannot fit or reach its spots, the order is rejected and all previous destinations are retained.
6. Focus the selection with F. Clear with Escape. Start a drag, switch applications, and return; the drag should cancel.

Report selection clarity, spacing, crowding near the wall, arrival jitter, and whether the group feels too rigid or loose. Zoom sensitivity remains at the provisionally accepted 0.002 value; a slightly faster value can be revisited later.

Current movement uses a loose destination grid oriented toward travel. Units use NavMesh avoidance while moving; there is no locked formation during travel, formation chooser, drag-to-set facing, command queue, combat, or economy yet. A rejected order suggests choosing more open ground; automatic formation reshaping near obstacles is deferred.

The separate local Windows build is `Builds/WindowsGroup/WonderGather.exe`. It must remain beside its data folders. Builds and generated logs are excluded from Git.
