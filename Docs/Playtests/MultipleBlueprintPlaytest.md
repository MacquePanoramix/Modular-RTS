# Multiple unit blueprints — playtest

Open TheFactionCreator in Unity, or run Builds/WindowsFactionCreator/WonderGather.exe.

1. Open a saved faction or start a new one. Older version-1 factions still open.
2. Select the Worker blueprint. Rename it Gatherer, leave Gather supplies on,
   turn Construct workshop off, and set At start to 1.
3. Choose Add worker blueprint. Name it Builder, turn gathering off, leave
   construction on, and give it one starting unit.
4. Select Workshop and choose Builder as its trained blueprint. Review the
   starting and construction/training relationships in the center list.
5. Playtest. Select an individual unit to see its blueprint name in the HUD.
   The Gatherer can gather but cannot build; the Builder can build but cannot
   gather. Build a workshop with the Builder, then train another Builder.
6. Return to the creator. Duplicate a blueprint and change its name or
   capabilities. The original should keep its choices. A duplicate starts
   with zero units and does not replace the workshop's training link.
7. Remove a blueprint. Read the confirmation, try Cancel, then confirm.
   Its starting units disappear; if it was trained by the workshop, that
   training link clears. Other blueprints keep their settings.
8. Save, restart Play Mode, and reopen the faction. Check names, counts,
   capabilities, and the workshop choice. Save as copy should preserve both
   faction designs independently.

Save before stopping Unity Play Mode. The standalone player prompts on a normal
close with unsaved changes; stopping the Editor bypasses that prompt.

## Prototype scope

- One to eight unit blueprints; up to eight starting units shared across them.
- One editable worker template with gather/build permissions. The Add button
  starts with both permissions on. Duplicate copies permissions with a new ID.
- One workshop with one selected trained blueprint, or no training.
- All units use the same placeholder body, movement and production recipe
  (10 supplies / six seconds). These are provisional, not design-budget rules.
- Blueprint names can repeat, with a clarity warning. Empty or control-character
  names prevent playtesting and saving. The last blueprint cannot be removed,
  but its starting count may be zero for a deliberate empty-start design.
- The center list scrolls; details scroll when needed. Long names are shortened
  only on the compact workshop card; the editor and save keep the full name.

This is a functional relationship editor, not the final freely arranged graph
HUD or visual customization system. Multiple training choices per workshop,
editable building rosters, appearance, equipment, stats and pricing remain
future design decisions.

## Save compatibility

Version 2 stores every unit's stable ID, name, starting count and gather/build
permissions, plus the workshop's trained ID. Version-1 saves are read into a
single Worker blueprint without changing the file. Explicit Save or Rename
writes version 2 and retains the previous file as .bak. Save as copy leaves
the original file unchanged. Older game builds cannot open version-2 files.

Unknown fields, duplicate IDs, broken training links and unsupported versions
are rejected without replacing the open draft. Normal storage, atomic updates,
conflict detection and recoverable deletion are unchanged. See
FactionLibraryPlaytest.md for the save location and recovery instructions.

Please focus feedback on whether creating distinct blueprints feels clear,
whether you can follow their relationships, and whether one training choice
per workshop is sufficient for the next prototype.
