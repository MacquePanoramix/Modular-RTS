# Civilization blueprint playtest

Open `Assets/_WonderGather/Scenes/TheCivilization.unity` and enter Play Mode.
The Little Settlement asset creates a completed starting base, eight workers,
and zero stored supplies. Gather from the green supply node, select a worker,
press B to build a workshop, then select the completed workshop and press T
to train another worker. New workers inherit the same blueprint permissions.
Zoom sensitivity remains 0.005.

## Compare the second setup

Exit Play Mode and open `TheProvisionedCivilization.unity` in the same folder.
This uses the same gameplay and blueprint links, with three starting workers
and 40 stored supplies. Build immediately or gather first. These samples are
configuration experiments, not final factions or balanced alternatives.

## Inspect and edit a civilization

1. Exit Play Mode before editing shared assets.
2. In `Assets/_WonderGather/Data/Civilizations`, select `LittleSettlement` or
   `ProvisionedSettlement` to see the editable starting setup and roster.
3. The Inspector's Starting network section reports dependency warnings and
   broken references. Expand a starting-unit entry to change its count, or
   change Starting Supplies, then play the corresponding scene again.
4. Select `Worker` to inspect Gathers Supplies and Builds. Select `Workshop`
   to inspect its Produces link. Building costs/time/size are in the existing
   BuildingDefinition attached to its prefab; worker cost/time/prefab are in
   WorkerProduction. These data references are reused rather than copied.
5. Duplicate a civilization asset for experiments. Assign it to Civilization
   Session on Player Controls in a copied scene. Duplicating a civilization
   still shares its unit/building assets: duplicate and rewire those too when
   changing permissions independently. Each blueprint needs a unique stable ID
   within its civilization.

Try turning off the worker's Gathers Supplies or removing its Builds link.
The report should explain the lost progression, and workers should obey their
new permissions at runtime. Undo the experiment afterward. Warnings allow
play; missing prefab references and unsupported component configurations
prevent initialization with an explanatory status message.

If a worker has multiple build permissions, the HUD offers building buttons;
B starts its first available option. Each building currently has one produced
unit type. A full visual graph editor and multiple recipes per building are
later work.

## What the report does and does not prove

It traverses construction and production links from the starting base and
starting units until no new blueprint is reached. Seeded cycles are permitted;
cycles without a starting route are reported as unreachable. It also warns
about absent supply gathering and an empty starting economy.

This is structural validation, not an economic simulation. It does not prove
that a complete chain is affordable, that a map has enough finite supplies,
that every spawn has a traversable route, or that the civilization survives
losses. Supplies remain the only implemented resource. Design-value formulas,
the pre-match budget, other resources, research, and competitive legality are
still open. Runtime construction/production costs remain separate from that
future design-value model.

## Feedback to give

Does changing the starting setup feel straightforward? Are dependency warnings
understandable? Does the original gather/build/produce loop still feel right?
Which customization would you most like to express next: a different worker
role, a second building, or another production relationship?

Windows build: `Builds/WindowsCivilization/WonderGather.exe` starts the standard
sample. Use Unity to test the provisioned sample and edit definitions.
