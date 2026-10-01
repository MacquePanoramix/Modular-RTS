> Historical milestone plan, archived September 30, 2026. Luis reviewed The
> Equipped Worker on September 30 and accepted it as the bare starting point
> for the one-worker showcase; see [the playtest record](../Playtests/EquippedWorkerPlaytest.md).
> The current proposal is in [NextMilestonePlan.md](../NextMilestonePlan.md).

# Approved implementation plan — The Equipped Worker

**Reviewed:** September 29, 2026.
**Review baseline:** 5f61351, matching GitHub main during the recommendation review;
Unity 6000.6.0f1. Implementation begins from the documentation commit 1dce817.
**Status:** Approved scope implemented and technically validated on September 29.
The detailed scope below remains its contract. See [Validation.md](../Validation.md)
and [EquippedWorkerPlaytest.md](../Playtests/EquippedWorkerPlaytest.md). Luis's playtest acceptance
of the result is pending.

Luis accepted The Living Worker as a useful early prototype and described a
later, highly polished one-worker showcase. The reference is
[WorkerShowcaseVision.md](../Design/WorkerShowcaseVision.md); the prior completed plan
is preserved in [Plans/LivingWorker.md](LivingWorker.md).

## The next question

Can a worker made through the faction blueprint system equip a real tool,
hold and use it convincingly, and extract material only when that tool
actually makes a valid strike?

The approved slice is **The Equipped Worker**: one provisional biped,
one pickaxe definition, one mineral/boulder target, and the existing
approach → work → carry → deliver loop. Its purpose is to establish the
connection between equipment, embodied action and a gameplay consequence.
It is a step toward the polished public showcase, not that complete showcase.

## What the pre-implementation review established

The recommendation was based on a focused source/configuration/history
review, not a new runtime or performance audit. Its baseline evidence was
the September 26 result: 67 passing PlayMode tests, reviewed rendered captures
and a successful Windows build. The observations below describe that earlier
implementation; they are not a current test report for the new equipment code.
Current candidate ownership is described in [EquipmentArchitecture.md](../Technical/EquipmentArchitecture.md),
and the review path is in [EquippedWorkerPlaytest.md](../Playtests/EquippedWorkerPlaytest.md).
Actual equipment validation results belong in [Validation.md](../Validation.md).

| Confirmed observation | Evidence under Assets/_WonderGather | Implication for the next slice |
|---|---|---|
| Work, cargo, delivery, reservations and production are integrated | Scripts/Units/Gatherer.cs; ResourceWorkplace.cs; Tests/PlayMode/LivingWorkerTests.cs | Extend these contracts rather than making a separate showcase economy |
| Gathering grants one supply after a timer; HasWorkContact means a reserved workplace | Scripts/Units/Gatherer.cs:24–28,157–164 | Actual tool contact must become an explicit success condition for mining |
| The body poses in LateUpdate and reaches toward a point; arm reach is clamped | Scripts/Units/ProceduralBiped.cs:93–140,244–277 | A desired contact point or a rendered gesture cannot prove a reachable strike |
| Starting and produced units receive UnitIdentity from their blueprints | Scripts/Civilizations/UnitIdentity.cs; CivilizationSession.cs; Scripts/Units/UnitProducer.cs | Apply equipment through the same shared path |
| Blueprints and saves have no equipment choice; v4 uses exact field validation | Scripts/Civilizations/UnitBlueprint.cs; Scripts/Creator/FactionRecord.cs | Persisting equipment needs an explicit schema extension with safe legacy defaults |
| Dirty tracking is separate from serialization | Scripts/Creator/FactionWorkspace.cs:16–25 | Tool edits must participate in unsaved-change detection as well as save/load |
| Performance values are temporary outcomes, not physical attributes | Scripts/Civilizations/UnitPerformance.cs | Do not reinterpret capacity as kilograms or movement rate as strength |

These are confirmed limits relative to the new target, not defects in the
accepted earlier prototype. Contact authority and save compatibility are the
highest-priority implementation risks because a visually plausible result
could conceal false extraction or lost player choices. Their boundaries can
be extended locally; this review does not justify a broad engine rewrite.

## Bounded scope

1. **A tool belongs to a blueprint.** Add a stable, data-backed pickaxe
   definition and a small None/Pickaxe choice in the existing unit editor.
   Give the tool a coherent transform, grip points and a striking head.
   Tool mass and handling properties may be authored as provisional data;
   this slice does not claim a working strength or burden simulation.
2. **Use the existing creation paths.** The choice survives duplicate,
   save/open, return from playtest and production. Starting and trained
   workers receive the same selected equipment. A small test environment
   can isolate the mining action, but its worker uses those blueprint paths.
3. **Approach and prepare.** Reuse navigation, selection and work reservations.
   Author one compatible surface and working region beside a mineral-bearing
   boulder. Confirm both navigation access and tool/body reach, then face and
   settle into a supported stance.
4. **Perform one restrained procedural strike.** A preparation, striking
   phase and recovery describe a single attempt. Hands hold authored grip
   points on the tool, with body participation and planted support. Do not
   stretch limbs or detach the tool to manufacture contact.
5. **Make contact matter.** Use the solved tool-head movement against the
   intended target surface to validate a strike. The first test gives a fixed
   provisional amount per accepted hit. A miss or cancelled attempt gives
   none. Impact angle/force-dependent yield remains an experiment for later.
6. **Complete the work loop.** Show where the pickaxe goes while the worker
   carries cargo or delivers it; choose one modest temporary carry/stow
   arrangement for review. Do not put a tool and a bundle in the same hands
   or make the tool disappear without an explicit transition. Physical loose
   chunks and a full loading system are later work, clearly marked as such.
7. **Explain the result.** The existing HUD distinguishes ready/working,
   interrupted, missing tool and unreachable work. A valid gather permission
   alone does not supply a missing tool. Keep ordinary RTS commands and make
   this action readable from close and strategic views.

## Authority and contact contract

Keep order intent, navigation, the work action, resource accounting and body
presentation distinguishable. Navigation continues to own the root; this
slice does not introduce active ragdolls or body-driven navigation.

A small gameplay-owned strike cycle owns attempt identity, phase and target.
Its tool pose and contact query must use the **same reachable, solved tool
trajectory** shown to the player. Define update order explicitly so the query
does not rely on stale LateUpdate transforms or a desired point that the
arms cannot reach. Do not add a second component that independently moves
the same arms or grants resources from arbitrary visual collision callbacks.

A hit is accepted only when the worker, target and equipped tool are valid,
the worker still owns its work position, the attempt is in its striking
phase, and the intended tool head contacts the intended surface. Test the
path between solved samples so a fast head cannot skip through the target.
Accept at most one extraction per attempt even if contact spans frames or
multiple target colliders. A new command, disable, target loss or release of
the station invalidates any outstanding attempt. If actual reach fails,
reposition or explain inability; never silently award the timer's yield.

Gatherer/ResourceNode remain the resource-accounting authority. Clip accepted
extraction to remaining stock and cargo space, preserve partial cargo and
conservation, and handle depletion by another worker safely. The body follows
the resulting action and cargo state. This is constrained physical contact;
it does not establish a full force-based simulation or multiplayer determinism.

## Compatibility boundaries

- Existing gathering scenes retain their established collection behavior.
  Mining is an explicit capability/target path, not a silent global conversion
  of all supplies into ore. The final resource list and cost formulas stay open.
- Existing rate/capacity controls keep their documented meanings. A mining
  rate can schedule attempts; it cannot bypass required physical contact.
  Reach and grip must remain valid across supported test rates.
- Add equipment to blueprint copying, runtime application, record capture,
  encoding/decoding, restoration and workspace dirty tracking together.
  Use stable definition IDs, not scene references or shared mutable instances.
- Introduce a versioned equipment field deliberately. Versions 1–4 open with
  explicit legacy defaults that preserve their current behavior. Reading must
  not rewrite files; explicit save upgrades through the existing atomic write,
  conflict detection and backup flow. Do not silently replace unknown tool IDs
  with a different creative choice. Leave unsupported files and drafts safe.
- Keep blueprint IDs, construction/production links, permissions and economic
  recipe values intact. Review migration and identity changes independently.

## Required evidence

- A valid hit extracts once; a miss, wrong collider, blocked/unreachable
  surface, absent tool or interrupted attempt cannot produce invisible ore.
- Repeated contact, coarse time steps, re-enable, depletion and shared targets
  cannot duplicate extraction or leak an old attempt into a new command.
- Hands remain on usable grips; tool/head contact corresponds to the visible
  geometry; limbs retain reach, feet remain supported and orders stay responsive.
- Starting and produced units agree on equipment. Editing marks the draft
  dirty; duplicate/copy/template ownership, save/open and playtest return are
  tested. Legacy files, unknown IDs, malformed data and failed writes are safe.
- Cargo, stock and storage stay conserved through the loop. Earlier gathering,
  movement, construction, production and faction-library regressions pass.
- Review rendered motion and a Windows build, then return the slice to Luis
  for judgment of grip, weight, restraint, contact and carrying transitions.

## What follows this proof

**Strength and burden** is the recommended follow-up: compare the same tool
on differently capable workers and make handling/movement consequences real.
Then extend material loads and bags/carts before polishing the small scene
for outside testers. The exact rules and sequence stay adjustable through
Luis's feedback; [WorkerShowcaseVision.md](../Design/WorkerShowcaseVision.md) preserves
the examples and open questions.

Three Temperaments remains part of the wider direction, but is not the next
recommended task under this clarification. Personality, combat, arbitrary
anatomy, full character/building editors, final art, free rigid-body ore,
advanced hauling, balance formulas and networking are outside this first
equipment slice. Deferring them here does not reduce the promised depth of
the eventual showcase or the final game.
