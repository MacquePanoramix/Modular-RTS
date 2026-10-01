# The Little Settlement — construction playtest

Open `Assets/_WonderGather/Scenes/TheSettlement.unity` in Unity 6000.6.0f1 and press Play. The Windows prototype is `Builds/WindowsSettlement/WonderGather.exe`.

1. Select workers and gather from the green supply node until at least 20 supplies have been delivered to the blue depot.
2. Select a worker and press **B**. Move over open ground: green means valid, red means blocked. Left-click green to place a workshop. Escape, B again, or right-click cancels the preview without paying.
3. The workshop costs 20 supplies immediately. Its assigned worker walks to it and builds for eight seconds. The structure rises as progress increases. Only the first selected eligible worker is assigned.
4. Give that worker a move or gather order partway through. Progress pauses, supplies remain spent, and any carried supplies stay with the worker. Select a worker and right-click the unfinished structure to resume for no extra cost.
5. Try placing on units, resources, walls, another workshop, map edges and the isolated island. Invalid placements must not spend supplies. Try B without a selected worker or enough stored supplies.
6. Move units around the placed structure. Its footprint blocks navigation while unfinished and after completion. Check camera controls, selection and the gathering loop still feel right.

Please report placement clarity, construction speed, interruption/resumption and whether progress is easy to see. The workshop is a placeholder building; its 20-supply cost, eight-second duration and three-unit footprint are provisional settings in `Assets/_WonderGather/Data/Workshop.asset`.

This step adds construction only. Workshops do not produce units yet. One worker builds each site at a time; there is no demolition/refund, cooperative speed-up, rotation, construction queue or save system. Placement supports the current flat prototype terrain and requires an unobstructed footprint plus a reachable work position. Canceling the preview is free; pausing a placed site does not refund its cost.
