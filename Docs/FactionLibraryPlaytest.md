# Faction saving and library playtest

Open TheFactionCreator in Unity or run Builds/WindowsFactionCreator/WonderGather.exe.

1. Name a faction, change starting values or blueprint permissions, and check
   that the footer says Unsaved changes.
2. Save. The footer should say All changes saved.
3. Save as copy, give it a different name, and save. Both designs should appear
   in Faction library with separate identities, even if their names match.
4. Open an entry. Its starting setup and gather/build/train relationships should
   return exactly. Playtest it, then return and confirm it is still saved.
5. Make unsaved edits and open another entry or choose New faction. Try Cancel,
   then Save and continue or Discard edits and continue. A failed save must not
   continue with the discard action.
6. Rename a library entry. Renaming the open entry updates the draft name too;
   save pending draft changes first. Delete asks for confirmation and leaves
   the open draft intact. Its next Save creates a new file.
7. Close and reopen the standalone game, or stop/restart Play Mode, then open
   Faction library. Saved factions should remain available.

The standalone Exit button and normal application-close request prompt when
there are unsaved changes. Unity's Stop Play Mode bypasses that player flow:
save before stopping Play Mode. The Editor Exit button explains this instead
of closing Unity. Force termination or a power failure cannot present a prompt.

## Storage and recovery

Saves are local, under Application.persistentDataPath/Factions. With the current
Windows project settings this is normally:

`%USERPROFILE%/AppData/LocalLow/Wonder Gather/Wonder Gather - The Wanderer/Factions`

Filenames use generated IDs, not faction names. Each `ID.faction.json` is a
versioned UTF-8 JSON document. Updating a save retains the previous version as
`ID.faction.json.bak`. Deleted entries move to the Deleted subfolder.

To recover manually, close the game first and copy a backup or a Deleted file
back to the main Factions folder, using the internal JSON id followed by
`.faction.json` as the filename. Keep a copy of any existing file before
replacing it. The recovery files are never auto-loaded.

Corrupt files, unsupported versions, unknown fields and unavailable blueprint
IDs are shown with an explanation. They are not silently converted or
overwritten. Open and Rename remain unavailable for unreadable entries;
Delete still requires explicit confirmation. A file changed outside the
creator requires reopening it or saving your draft as a new copy.

## Current limits

Format version 1 records the first creator's name, starting base/worker/workshop
IDs, worker/supply counts, and gather/build/train links. It is a faction design,
not a running match save. Costs and prefabs come from the game's current
supported blueprint collection. Unsupported future versions are refused;
there is no older released save format to migrate yet.

No cloud sync, sharing, import UI, autosave, save-game progress, faction budgets
or extra blueprint types are included. Those remain separate decisions.

Focus feedback on save-state clarity, library navigation and whether the
confirmation choices make it clear what will be kept or replaced.
