# Building blueprints and production networks — playtest

Open TheFactionCreator in Unity, or run Builds/WindowsFactionCreator/WonderGather.exe.

1. Start with a worker and enough supplies to test construction. The starting
   depot remains fixed. Existing saved factions still open.
2. On Units, duplicate the worker and name the copy Gatherer. Set its starting
   count as desired, leave gathering on, and turn off its construction links.
3. On Buildings, add a workshop blueprint and call it Training Hall. Enable
   both the worker and Gatherer under Can train.
4. Return to Units, select your worker, and enable Training Hall under Can
   construct. The building's detail panel should list that worker as a builder.
5. Playtest. Select the worker, choose Build Training Hall in the HUD, and
   place it on open ground. Wait for construction to finish.
6. Select the completed hall. Queue a Gatherer and then a worker with their
   training buttons. Confirm the queue names/order and the resulting units'
   capabilities. T queues the first listed option. Cancel last refunds that
   order while preserving the current order's progress.
7. Return to the creator and duplicate the hall. Its training choices are
   copied, but no unit gains construction permission automatically. Add a
   construction link explicitly, then change one hall's training choices and
   confirm the other retains its own choices.
8. Try removing a building. Cancel first, then confirm. Its incoming
   construction links and outgoing training list are removed. Removing a
   unit similarly removes it from all training lists and the starting setup.
9. Save, restart Play Mode, and reopen the faction. Check building names,
   unit counts, and every construction/training link. Playtest it again.

The center roster, detail panel and in-game HUD scroll when needed. Hovering
the HUD blocks world clicks and wheel zoom. Select the Starting base button
to inspect the fixed depot. Save before stopping Editor Play Mode.

## Current scope

- One to eight editable unit blueprints and one to eight editable building
  blueprints, plus the fixed starting depot; at most eight starting units.
- Buildings use the existing workshop prefab, footprint and construction
  recipe: 20 supplies and eight seconds. Units use the existing worker
  recipe: 10 supplies and six seconds. All are provisional values.
- Any editable building can train zero or more roster unit types. Units can
  construct zero or more editable buildings. A building with no incoming
  construction route is allowed with a reachability warning.
- Duplicates receive new stable IDs. Building copies preserve training
  choices; unit copies preserve construction choices and start with zero units.
- The last unit/building blueprint cannot be removed. Links can all be off.
  The starting depot is not an editable production building in this slice.
- The queue has three entries. Each entry remembers its requested blueprint,
  prefab, paid cost and duration. Cancelling or destroying a producer refunds
  each remaining entry's original payment. Blocked exits keep the head order.

Names may repeat with a clarity warning, but printable names are required to
save/playtest. The art, costs, final graph layout, building categories, combat,
design budget and detailed customization tradeoffs remain open design work.

## Saved factions

Version 3 stores named building records, ordered training links and per-unit
construction links. Version-1 and version-2 factions load into the current
model without changing the file. Explicit Save or Rename writes version 3
and keeps the previous file as .bak. Save as copy preserves the original.
Older game builds cannot open version-3 saves.

Unknown fields, missing or duplicate IDs, duplicate links and links outside
the faction roster are rejected without replacing the draft. Local paths,
atomic writes and recoverable deletion are documented in FactionLibraryPlaytest.md.

Please focus feedback on whether the two tabs make the network easy to follow,
whether training/constructing links are easy to find, and whether queue choices
remain clear in the playtest. The next proposed discussion is a small set of
meaningful customization tradeoffs to inform the future design-budget system.
