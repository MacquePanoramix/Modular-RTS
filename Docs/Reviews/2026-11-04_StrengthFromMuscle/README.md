# Review: strength from muscle as the default (November 4, 2026)

Three independent reviewers, each alone with its own question, on a copied folder ([the brief](brief.md): the change,
its evidence, its claims); none had made the change. Work branch `claude/wip-round`, change `23754f6`.

| Reviewer | Question | Review |
|---|---|---|
| The physicist | Is the arithmetic and the physics right? | [physicist.md](physicist.md) |
| The sceptic | Where is the claim "nothing the Director has played changes" wrong? | [sceptic.md](sceptic.md) |
| The builder | What will bite later? | [builder.md](builder.md) |

## What they found, and what was done (`f7312d1`)

| Finding | By | Done |
|---|---|---|
| One share for both arm joints leaves each 0.3 to 0.8% off; the two failing swing tests follow Round's shoulder (+0.71%), not the arms in general; mend the cause rather than ease the tests | all three | Each joint of a body made before the rule keeps exactly what it had (`shoulderKept`, `elbowKept`, `kneeKept`); the two tests' thresholds as they were; the weak Round's grip 61% and the feeble lift 0.74 s again |
| One field means two things: the creator's strength stat and a calibration of bodies drawn before the rule (Long's 1.40 would contradict its look once muscle is shown) | builder, sceptic | The calibration is its own (the kept factors); the creator's share of muscle per region is 1 for the three miners |
| "The knees exactly as before" is by construction: the calibration reads the saved proportions, the rule before the live ones; nothing compared a woken miner | physicist, sceptic, builder | A test reads each miner woken in the game under both rules: shoulder, elbow, knee, back, wrist and hold the same to a hundredth of a newton metre |
| The static rule switched without try/finally | builder | Switched under try/finally, in PhysicalBody and MinerSetup |
| `Configure` keeps cached arm needs | builder | Cleared in `Configure` |
| The wrist and the hand's hold ignore the arm's share | physicist, builder | They take it under strength from muscle (1 for the three miners, so unchanged) |
| Other bodies than the three would silently go uncalibrated | sceptic, builder | Checked: only the three miner prefabs carry a `PhysicalBody` (its script's GUID in every prefab, scene and asset); `MinerSetup` calls the keep when it makes one |
| Silent fallbacks and clamps | sceptic, builder | An absurd keep (outside 0.1 to 10) is refused with an error; the missing-stance fallback (0.4 m) is still silent |
| `StaysPut: 0` in the prefab diffs | sceptic, builder | `ProceduralBiped.StaysPut`, a plain field (false) added on October 8: its default, written when the prefabs were first saved since |
| No whole suite on the final change | sceptic | The whole PlayMode suite on `f7312d1`: 220 tests, 192 passed, 0 failed, 28 run only when asked |
| Not done | | The 28 tests run only when asked (the benches); strengths other than 1 in the game (every capacity is linear in strength under both rules, so the keep holds at any strength: the physicist); a work-tire-rest cycle on the three; a build |

Their verdicts are not Luis's. Whether the three miners keep what they had, or their builds speak (Long weaker for its
height, Round stronger), is his to choose.
