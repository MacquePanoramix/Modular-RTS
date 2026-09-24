# Next milestone proposal — The Living Worker

**Reviewed:** September 24, 2026

**Status:** Proposed; awaiting the Game Director's choice.

**Baseline:** GitHub d03e045; gameplay remains d9b5f4d.

## Where we are

The prototype has RTS camera and group orders, gathering/delivery,
construction/production, civilization blueprints and reachability checks,
editable unit/building networks, a local faction library with versioned saves,
and temporary unit-performance controls. The separately tested Living Body
adds procedural stepping and terrain adaptation. Luis accepted its current
feel as adequate for the prototype on September 24.

The latest recorded validation is 61 passing PlayMode tests and a successful
Windows development build on September 22. This planning review did not run
fresh tests, a build or a performance profile. The newer GitHub continuity
commit changes documentation only.

## Where we are going

Players author a civilization's connected possibilities and watch it become
alive. Bodies, equipment and eventually personality should give mechanical
choices visible meaning. The creator needs a strong HUD that explains those
choices, dependencies and consequences. Current numerical controls, resource,
body proportions and network bounds remain provisional.

Still unproven are the connection between physical actions and the economy,
personality/autonomy, physical combat, meaningful body/equipment tradeoffs,
design-budget pricing, a complete match and practical simulation scale.

## Why this next

The clearest current gap is that faction workers and articulated walkers are
separate implementations. Connect them with one purposeful routine:

**player-created worker → approach supplies → gather → carry → deposit → repeat.**

This tests the design pillars of enjoyable observation, embodied actions and
player authorship together. It also supplies a real activity for later
personality experiments. Three Temperaments remains a useful follow-up;
this proposal inserts an integration step before it, subject to Luis's choice.

## Bounded implementation plan

1. Author a worker variant using the provisional biped and existing worker
   components. Use the faction spawn/production paths for both starting and
   newly trained units, retaining blueprint identity and permissions.
2. Establish reachable work positions beside the resource and depot. Face
   the interaction and perform one restrained reach-and-collect gesture.
3. Show a temporary carried bundle and a carrying pose derived from actual
   cargo. Make delivery and return to an empty-handed pose readable. Final
   resource fiction, equipment and load penalties are not decided here.
4. Keep selection and orders responsive through starts, turns, interruptions,
   depletion and repeated deliveries. Show concise activity/cargo feedback
   in the existing HUD, useful at strategic distance as well as close up.
5. Validate a few workers sharing the loop, including production and the
   existing creator performance settings. Return the result to Luis before
   adding construction gestures or further behavioral depth.

## Technical boundaries exposed by the review

- `CivilizationValidator` expects Gatherer, Builder and ProducedWorker on
  faction worker prefabs; the current movement-only biped lacks them.
- `CivilizationSession` and `UnitProducer` use work offsets 3.4m or more from
  node/depot centers. Arm motion alone cannot make that contact believable;
  the slice needs explicit reachable interaction positions and modest
  coordination of occupied positions.
- The creator allows movement from 25 to 200 percent of the worker's 3.2
  base speed (0.8–6.4). The biped was demonstrated at 1.8 and 2.5 only.
  Adapt and validate stepping for the supported settings instead of silently
  changing their meaning or imposing unapproved limits.
- `Gatherer.State` and `Carried` already own activity and cargo. Add only
  the small read-only action context needed by the body. Gameplay owns
  resource accounting; presentation must not duplicate it or invent cargo.
- Keep navigation responsible for root movement. Integrate work/carry poses
  with the biped's limb ownership so two components do not fight over arms.
- Existing build permissions and construction behavior must keep working;
  procedural construction gestures are outside this first work loop.

## Acceptance checks

- Starting and produced faction workers complete repeated deliveries, with
  resource totals and visible cargo agreeing through interruption/depletion.
- Feet remain grounded, work occurs at believable reach, and arm/cargo
  transitions do not snap or continue after the underlying action ends.
- Existing speed, gathering-rate and carrying-capacity settings still work;
  old factions, permissions and ordinary RTS controls retain their behavior.
- Luis can read the action at strategic distance and enjoys observing it
  closely. Weight, restraint and tone remain his creative judgment.

Automated integration/regression checks should cover gameplay accounting and
state transitions; rendered/runtime review should cover body interaction.
A few-worker playtest does not establish an RTS-scale performance budget.

## What follows, provisionally

Revisit Three Temperaments to test readable interpretation of orders, then
a small physical encounter. Feed established capabilities back into faction
blueprints and the creator HUD. Use those demonstrated consequences to guide
body/equipment customization and the two-cost model. Reassess the order after
each playtest; full morphology, final art, active ragdolls and multiplayer
remain separate decisions.

Before implementing this proposal, agree that the neutral collecting gesture
and visible carried bundle are useful temporary tests of the intended feel.
No species, culture, permanent resource list or final movement style is chosen.
