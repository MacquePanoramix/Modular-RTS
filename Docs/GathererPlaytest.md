# The Gatherer — Little Settlement, first step

Open `Assets/_WonderGather/Scenes/TheGatherer.unity` in Unity 6000.6.0f1 and press Play. The separate Windows prototype is `Builds/WindowsGatherer/WonderGather.exe`.

Eight workers start between a green supply node and a blue depot. Supplies are a placeholder resource, not a decision about the final resource list. Camera zoom sensitivity is 0.005, as chosen in playtesting.

1. Select one worker and right-click the green sphere. Watch it gather up to five supplies, walk to the blue depot, deliver them, and repeat. The stored total increases only at delivery.
2. Box-select all eight and order them to gather. Watch the carried, remaining and stored totals, worker spacing, and repeated trips. The node starts with 120 supplies and eventually runs out.
3. Give a ground move order while a worker carries supplies. Gathering stops and cargo stays with the worker. Right-click the blue depot to deliver it, or the green node to resume gathering.
4. In this scene or TheGroup, right-click the wall, isolated island, and map edge. Units should move toward reachable alternatives, with separate spots for the group. The marker shows the resolved group center.
5. Check Shift-click, drag selection, Escape and F still feel right. Reload the scene to reset the economy.

Please report how clear the gathering loop is, whether the rate and travel distance feel pleasant, and any crowding or stalled workers. Physical mouse input and overall feel still need your playtest.

The prototype uses fixed per-worker interaction offsets around one node and one depot. It has no animation, persistence, construction, production, automatic search for a new resource, or final resource balance. Missing or unavailable targets stop work; existing cargo is retained. Movement searches the baked navigation triangles for a reachable alternative, then searches nearby spots for the other units; this is a practical small-group fallback, not a guarantee of an optimal formation on every map. Non-finite orders and groups that cannot fit retain their previous destinations. Gathering requires a complete route to its interaction spot and never collects remotely from an unreachable node.
