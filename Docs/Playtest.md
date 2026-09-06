# The Wanderer playtest

Run with the Game view focused. Record observations about feel separately from reproducible defects.

1. Enter Play Mode. Confirm meadow, stone wall, gold unit and instructions appear without Console errors.
2. Pan in all directions, rotate both ways, zoom to each limit. Confirm smooth movement, sensible limits and no view below the ground.
3. Click the unit. Its ring appears. Click empty ground and press Escape in separate trials; both clear selection.
4. Select again and issue a move on the opposite side of the wall. Confirm visible destination feedback, a route around the wall and stopping near the destination.
5. Issue a new valid move while traveling. The unit should adopt the new route immediately.
6. Click a stone, outside the map, or the isolated island to the east. The status should reject the destination and the previous valid route should continue.
7. Press F while selected. Confirm the camera centers smoothly on the unit. Repeat without selection; no error should occur.
8. Disable the unit while selected, then enable it. It should not retain a stale selection ring.
9. Stop and re-enter Play Mode. The scene should restart cleanly. Test with normal domain reload settings first.
10. Report camera speed, zoom range, turning pace, and whether selection/order feedback is clear.

The baseline is mouse and keyboard on a flat test map. Controller support, drag selection, formations, uneven-terrain camera collision and a full production HUD are not implemented in this milestone.
