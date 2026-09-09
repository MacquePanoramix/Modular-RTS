# First faction creator playtest

Open `Assets/_WonderGather/Scenes/TheFactionCreator.unity` in Unity and enter
Play Mode, or run `Builds/WindowsFactionCreator/WonderGather.exe`.

## Create and try a faction

1. Name your faction in Starting setup. Adjust workers and stored supplies.
2. Select the Worker card in the central graph. Toggle supply gathering and
   permission to construct a workshop in the right panel.
3. Select Workshop. Toggle its ability to train workers. The graph updates to
   show the construction and production links that remain.
4. Read the messages below the graph. Missing growth routes warn, but allow
   intentional challenge setups. A blank name must be fixed before launch.
5. Press Playtest faction. Use the existing controls: click/drag selection,
   right-click gathering/movement, B construction, T production, F focus.
6. Use Return to faction creator in the upper right. Edit your draft and try
   again. Each map starts fresh; your draft choices remain intact.

Use Save or Save as copy to keep a faction between sessions. Faction library
opens, renames and deletes saved designs. Unsaved edits remain session-only;
save before stopping Unity Play Mode. See FactionLibraryPlaytest.md for the
save-state prompts, recovery files and current format limits. Editing a draft
does not modify the example ScriptableObject assets.

## What to look for

- Can you tell what each arrow means and which panel changes its relationship?
- Do starting setup, blueprint details and dependency messages feel easy to find?
- Do your choices behave as expected in the map, including disabled gathering,
  disabled construction and disabled training?
- Can you return, change a choice and test again without losing your draft?
- Are text, controls and warnings readable in your normal window size?

Try three workers and 40 supplies for quick construction tests. Try removing
the construction link to see the warning and inability to place workshops.
Try disabling production: a completed workshop should not train workers.

## Scope of this first pass

This is a functional layout prototype for review with the game director, not
the final aesthetic. It uses the existing immediate-mode UI foundation with
a scaled 1280×720 layout. Desktop mouse and keyboard are the target; inspect
readability at smaller sizes before deciding on final responsive behavior.

The available content is one starting depot, one worker type and one workshop.
The starting map limits are 0–8 workers and 0–120 stored supplies. They are
prototype bounds, not a design budget or balancing formula. Costs remain
visible but fixed. Blueprint cards have fixed positions; graph dragging,
additional types, new recipes, bodies, equipment, technologies, visual
customization are future work. Saved faction files are now supported.

Warnings check structural reachability and basic supply availability. They do
not prove whole-chain affordability, map resource sufficiency or competitive
legality. We will agree on the next customization and visual direction together.
