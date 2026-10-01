# Equipment and mining architecture

**Status:** Implemented and technically validated September 29, 2026. This
document describes the current prototype contracts; Luis's acceptance is pending. See [Validation.md](../Validation.md) for actual results
and [EquippedWorkerPlaytest.md](../Playtests/EquippedWorkerPlaytest.md) for the review path.

## Scope and ownership

The Equipped Worker connects an authored blueprint choice to an embodied
work action. Navigation still owns the worker root. The existing economy
still owns stock, worker cargo and storage. Equipment adds a constrained
tool path and a contact requirement for explicitly mineable resources.

| Component | Responsibility |
|---|---|
| `ToolDefinition` | Stable ID, label, visual prefab, two local grip points, local striking-head point and contact radius |
| `UnitBlueprint` / `CivilizationDefinition` | Selected tool or None, and the available definition catalog |
| `FactionDraft` / `FactionWorkspace` | Editable choices, copied blueprints, template isolation and unsaved-change detection |
| `UnitIdentity` | Apply the blueprint's tool and performance to starting and produced units through one path |
| `Gatherer` | Contextual orders, reservations, travel/work/delivery states and accepted extraction into cargo |
| `EquippedTool` | Tool instance, attempt identity, strike phase, reachable pose, physical contact queries and stow pose |
| `ProceduralBiped` | Ground support, body posture and limb pose; explicitly invoke the tool solve before consuming its grips |
| `MineableResource` / `ResourceWorkplace` | Required tool and exact target surface; separate reserved standing/contact points |
| `ResourceNode` / `ResourceDepot` | Finite remaining stock and deposited supplies |

Definitions are shared authored data. Each worker owns its instantiated
tool model and action state. `UnitIdentity.Configure` applies equipment
through `EquippedTool.SetDefinition`; selecting None clears the equipped
model. Both `CivilizationSession` and `UnitProducer` use `UnitIdentity.Apply`.
There is no special showcase-only worker creation path.

## Creator and persistence

The unit editor presents **Equipped tool** choices from the faction's
catalog, including explicit **None**. `FactionDraft.SetTool` requires a
definition from that catalog. Duplicating a unit copies its tool selection;
editing the duplicate leaves the source blueprint and template intact.
The workspace dirty-state fingerprint includes the stable tool ID.

The creator has two entry points. `Playtest()` keeps the existing
`FactionPlaytest` scene. `PlaytestEquipment()` loads `EquipmentPlaytest`.
Both initialize the loaded scene's bridge from the current draft, and
returning unloads the running world while retaining the editable draft.

Faction records now encode **version 5**. Each unit has a required `tool`
string: the empty string means None, and `"pickaxe"` identifies the current
tool. Saves contain definition IDs, not Unity object or scene references.
Exact field validation remains version-specific.

Versions 1–4 decode with None. Version 4 preserves its performance values;
the earlier existing performance defaults still apply to versions 1–3.
Reading does not rewrite a file. An explicit save uses the existing
temporary-write/atomic-replace flow, checks the previously read file token,
and keeps the replaced file as `.bak`. A copy gets its own faction ID.
Unknown tool IDs cannot silently become None or a different tool: draft
restoration fails before exposing a replacement draft, preserving the open
workspace and the original file. Equipment does not change blueprint IDs,
building links, production recipes or resource costs.

## From order to contact

`Gatherer.Gather` checks permission, the active target/depot and the required
tool before committing a mining order. It uses the existing workplace
reservation and navigation path. A reserved position is a place to attempt
work; reservation by itself is not evidence of a successful strike.

After arrival, the worker faces the work contact. A valid mine order and
supported body allow `EquippedTool` to settle and begin an attempt. The
attempt has four readable phases:

| Phase | Meaning |
|---|---|
| Rest | Held tool or settling between attempts; no extraction |
| Preparation | Move toward the backswing; no extraction |
| Striking | Sweep the solved head toward the surface; at most one accepted extraction |
| Recovery | Recover from contact, obstruction or a miss; no further extraction |

Gathering rate schedules attempts, with a minimum cycle duration of 0.8 seconds
for the current readable pose. Each solved frame consumes at most 0.5 seconds
of action time and subdivides progress by at most 0.02 of a cycle. A very long
stall therefore slows the action instead of replaying many hits at once.
Rate never overrides contact validity. The strike
path is procedural and deliberately bounded; it is not a force-based
simulation, active ragdoll or a claim of multiplayer determinism.

### Explicit shared solve

The order within `ProceduralBiped.Pose` is intentional:

1. Solve ground support, reachable pelvis height and torso posture.
2. Invoke `EquippedTool.SolveFrame` with that current body pose and support.
3. Solve the tool trajectory and validate both authored grip positions
   against the shoulders' arm reach before testing extraction.
4. Use the resulting grip positions as the arm IK targets while holding
   the tool.

This is one shared call sequence. A separate LateUpdate component or a
visual collision callback does not move the arms or grant resources from
stale transforms. The same reachable tool trajectory drives the rendered
model and contact query. Independent hand clamps cannot stretch the arms
to justify an otherwise unreachable hit. The current rig accepts shoulder-to-grip
distances from 0.025 to 0.865 metres, inside its IK solver's clamp limits.
Failure of tool reach interrupts
mining with feedback; loss of a supported stance invalidates the current
attempt so the worker must settle again.

The current rig and tool require unit root/world scale. Tool definitions reject
scaled prefab roots, and mining rejects or interrupts scaled workers or tool
instances. This keeps displayed grips/head and the query in the same world
coordinates. Arbitrary body dimensions need a later rig-aware solve.

### Contact and accounting

During the striking interval, the solver subdivides its progress and sweeps
a sphere representing the authored head between solved samples. This
tests the path rather than only its endpoint. An overlap check on every unspent
strike segment prevents an obstructed origin from counting as a new strike,
including an obstacle entering the swing mid-attempt.

The nonallocating physics queries use fixed buffers, ignore triggers and
the worker's own colliders, and select the nearest physical obstruction.
A full query buffer fails closed. Only the intended `MineableResource.Surface`
can succeed; a nearer unrelated collider blocks extraction. The visible
tool stops its forward motion at the detected obstruction and recovers.
Missing the target leaves stock and cargo unchanged.

The action marks an attempt spent after its first obstruction and records
the accepted attempt ID. `Gatherer.AcceptStrike` rechecks the source, target,
order state, reservation, facing, tool and unconsumed attempt identity.
Only then does it request up to **one** unit from `ResourceNode.Take`, clipped
to remaining cargo space and stock, and add the actual returned amount to
`Carried`. The resource counter is never duplicated in the tool or body.
Repeated contact in one attempt cannot award another unit.

A changed command, changed tool, disabled worker/tool/body, lost target or
lost work position invalidates the old attempt. Already collected cargo
remains accounted to the worker. Full cargo or depletion leads to the
existing delivery path; the last work cycle can finish recovery before
departure. Several workers share the finite `ResourceNode`, so each
successful transfer must use its current remaining amount.

## Visible cargo and provisional handling

The existing bundle reflects `Gatherer.Carried`. While mining, its visual
position is beside the worker's feet, leaving both hands for the pickaxe.
This is a temporary representation of that worker's cargo, not a second
resource inventory or independently collectible world stockpile.

When the worker leaves work with cargo, the pickaxe transitions to a back
stow and the hands use the carrying pose. Delivery clears cargo through the
existing depot transaction. The tool also stows for activities that require
the previous hand pose, including legacy collection and construction.
The current transition interpolates around the worker's side to the back.
It does not yet
model a strap, a hand-driven mounting/pickup sequence, loading containers,
loose stone physics, tool mass or changes to movement from burden.

## Authored assets and compatibility

`EquippedWorkerSetup.Create` is a guarded one-time editor authoring step.
It creates the provisional pickaxe prefab and definition, adds equipment to
the existing Living Worker prefab, adds the definition catalog to the
creator example, and copies the old faction map into a new equipment map.
The existing template worker keeps None until the player chooses a tool.

The copied resource retains its `ResourceNode` identity and session/HUD
references. Its new faceted mineral mesh is also its collision mesh.
Eight ports surround it, with a carving navigation obstacle reserving the
boulder footprint. Existing scene entries remain in Build Settings; the
new map is added. The Windows build entry point includes the creator and
both playtest maps.

`MineableResource` is an explicit capability marker. A legacy `ResourceNode`
without it retains the established timer-based gathering path, even when
the worker has a pickaxe. This does not silently redefine every resource
as ore or make old factions require equipment. Generic supplies, prices,
capacity counts and raw rate controls remain provisional.

## Evidence and design boundaries

Eighteen focused equipment/persistence tests, 85 full PlayMode tests, seven
reviewed rendered captures and a Windows x64 development build passed.
[Validation.md](../Validation.md) records exact results, initial failures and
limits. The rendered frames cover the creator, overview, preparation, contact,
recovery, carrying and delivery; they do not establish final aesthetic acceptance.

Luis's [showcase direction](../Design/WorkerShowcaseVision.md) remains the authority
for the deeper target. Strength-dependent carrying and use, dragging,
bags/carts, material loads, detailed character and building creation,
force/angle-sensitive yield and final art remain open or later work.
This slice's fixed yield and two-handed/back-stowed poses do not settle
those rules or replace Luis's aesthetic judgment.
