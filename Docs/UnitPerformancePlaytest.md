# Prototype unit performance playtest

Open `Assets/_WonderGather/Scenes/TheFactionCreator.unity` and press Play,
or use the local `Builds/WindowsFactionCreator/WonderGather.exe`.

1. Select a unit in the Units tab. Scroll its right-hand details to
   **Prototype performance**. Set movement to 150%, carry to 10, gathering to
   200%, and construction to 200%. Give it one starting unit, gathering
   permission and permission to construct a workshop. Start with 40 supplies.
2. Duplicate it, name the copy, and reset the copy's performance. Give it one
   starting unit. The original keeps its values. Assign both training links
   to a workshop so you can compare trained units too.
3. Save, open the faction again, and check both sets of values. Launch the
   playtest and select each unit to see its settings in the HUD.
4. Send units to separate distant points. The 150% worker should move faster.
   Send them to gather. The faster gatherer collects twice as quickly while
   actively working and carries ten per trip; travel still affects throughput.
5. Have the 200% builder construct a workshop. Its work phase takes about four
   seconds instead of eight; travel time is additional. Train both types and
   confirm that new workers inherit their respective settings.
6. Return to the creator, change one value, and check the unsaved indicator.
   Save before stopping Editor Play Mode. Reset performance restores the
   original 100% / five supplies / 100% / 100% worker settings.

Controls and bounds are temporary. Rate changes do not grant permissions.
Costs and training durations remain unchanged, so these are experimental
performance differences rather than balanced choices. Older saved factions
receive the original defaults and are only upgraded when explicitly saved or
renamed. Final body, equipment, procedural behavior and pricing remain open.
