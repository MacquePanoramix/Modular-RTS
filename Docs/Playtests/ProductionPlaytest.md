# The Little Settlement — worker production

Open `Assets/_WonderGather/Scenes/TheProduction.unity` in Unity 6000.6.0f1. The local Windows build is `Builds/WindowsProduction/WonderGather.exe`.

1. Gather and deliver at least 30 supplies. Select a worker, press B, and place a 20-supply workshop on green ground.
2. Let construction finish. **Left-click the workshop**. Its selection outline and production controls should appear. F focuses the selected workshop; selecting a unit returns to unit commands.
3. Click **Train worker** or press **T**. A worker costs 10 supplies and takes six seconds. Costs are charged when queued. Up to three workers, including the active worker, can be queued at one workshop.
4. Queue more workers, then use **Cancel last / refund**. The last queued worker is canceled for a full refund. Canceling the active worker when it is the only entry also clears its progress.
5. Select a new worker by clicking or boxing it. Try moving it, gathering, delivering and constructing another workshop.
6. Try two workshops: their queues should be independent. A blocked exit keeps the ready worker in its queue until nearby space opens; it does not charge again or spawn on top of units.

Please report whether building selection and the queue are clear, whether workers emerge in sensible places, and whether new workers feel fully integrated with existing controls.

The production definition is `Assets/_WonderGather/Data/WorkerProduction.asset`. Cost and time are provisional. No rally point, multiple unit types, population cap, save system or multiplayer is included. Disabling a producer pauses it; destroying it refunds pending orders when its depot still exists. The original scenes remain available. New workers receive distinct gathering positions in expanding rings; large crowds and many resource sites will need a later assignment system.
