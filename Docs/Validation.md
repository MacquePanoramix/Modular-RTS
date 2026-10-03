# Validation history

Current milestone: The Living Worker; see the September 26 entry below and
Docs/Playtests/LivingWorkerPlaytest.md. Earlier sections preserve historical evidence.

## Unity 6.6 foundation — September 6, 2026

The development slice was migrated from Unity 6000.3.12f1 to 6000.6.0f1, compiled, passed its automated runtime tests and produced a fresh Windows build. Final milestone acceptance remains pending user playtesting of camera feel and mouse/keyboard interaction.

## Evidence

| Check | Result |
|---|---|
| Unity Editor 6000.6.0f1 migration and compilation | Passed |
| Unity 6.6 package resolution | Passed; manifest and lockfile updated |
| Scene authoring and NavMesh bake | Passed; WANDERER_SETUP_OK |
| PlayMode: navigate around wall and arrive | Passed |
| PlayMode: invalid orders preserve destination | Passed; disconnected island, out-of-range and NaN inputs |
| PlayMode: clear, reselect, disable selection | Passed |
| PlayMode suite | 3 passed, 0 failed, 0 skipped; 5.78 seconds |
| Windows x64 development build | Passed; WANDERER_BUILD_OK |
| Startup scene | TheWanderer only; serialized build settings verified |
| Asset metadata | Every asset file under Assets has a corresponding .meta |
| Rendered scene preview | Previously inspected under 6.3 and retained because headless 6.6 capture cannot render; source scene and materials were unchanged by migration |
| Manual mouse/keyboard interaction and camera limits | Not run; user playtest checklist supplied |
| Standalone executable startup | Passed; 6.6 player remained running for a 10-second D3D12 smoke test without crashing |
| Standalone mouse/keyboard interaction | Not run; startup success is not a manual playtest |

The generated XML test report is intentionally excluded from Git because Unity writes machine-specific absolute paths into it. The durable result is recorded above. Preview: `Images/TheWanderer.png`. Local build: `../Builds/Windows/WonderGather.exe`.

## Resolved setup issues

- Restricted execution initially prevented Unity Package Manager IPC; normal local execution completed.
- The bundled template originally pinned Input System 1.12.0, which failed against a removed BuildTarget API. The Unity 6.3 baseline resolved this with Input System 1.17.0.
- The Unity 6.6 migration resolved and pinned AI Navigation 2.0.14, Input System 1.20.0, URP 17.6.0, Test Framework 1.8.0, uGUI 2.6.0, and Visual Studio integration 2.0.26.
- Unity 6.6 upgraded the URP global settings, player settings, graphics settings, quality settings, and added its current Project Auditor and Physics Core 2D settings assets.
- Removed the template tutorial, sample scene, unused input actions and stale global action reference.
- The first editor preview used unfinished shader compilation. Synchronous shader compilation produced the inspected final preview. Runtime gameplay code was unchanged.

## Remaining limitations

The first Unity 6.6 import detected stale 6.3 library metadata and rebuilt version-specific caches. The subsequent clean test and build runs completed successfully. Generated cache and test-result files remain excluded from Git.

This is a development prototype with placeholder art and a temporary HUD. It has no multiplayer, economy, formations, procedural animation or civilization customization yet. Tests validate the movement/selection boundary; they do not prove every physical input, camera angle, display resolution or visual preference. Complete Docs/Playtests/WandererPlaytest.md with the user before accepting the milestone's feel.

ProjectSettings were created from the bundled template and upgraded to Unity 6.6 serialization. Feature-specific settings are product/company names, text serialization, Walkable layer 6, one enabled build scene and removal of the unused template global input asset. Graphics and quality continue to use the template's URP assets; the new Input System remains enabled.

## Camera tuning — September 7, 2026

Based on the first hands-on playtest, mouse-wheel zoom sensitivity increased from 0.0015 to 0.0020, approximately 33 percent. The exponential zoom response, smoothing, and minimum/maximum distances are unchanged. An isolated Unity 6000.6.0f1 validation copy compiled successfully and passed all three PlayMode tests in 5.78 seconds. Final speed acceptance requires a short user retest.

## Prototype 1.2 — The Group — September 7, 2026

Implemented eight-unit selection and movement in the separate TheGroup scene. The user provisionally accepted the earlier camera feel and zoom sensitivity; further tuning is deferred.

| Check | Result |
|---|---|
| Unity 6000.6.0f1 PlayMode suite | 10 passed, 0 failed; 14.97 seconds |
| Selection | Click/toggle, box/additive selection, deduplication, disabled and behind-camera units, lifecycle cleanup passed |
| Group navigation | All eight agents routed around the wall and reached separate destinations |
| Invalid group orders | Disconnected, edge and non-finite targets preserve existing destinations |
| Formation layout | Counts 1, 2, 5, 8 and 9, rotation and spacing passed |
| Windows x64 development build | Passed; GROUP_BUILD_OK; Unity exit code 0 |
| Scene preview | Rendered with graphics enabled and visually inspected; eight units, terrain and wall visible |
| Existing assets | Original Wanderer scene, prefab and NavMesh preserved |
| Physical mouse/keyboard playtest | Pending user feedback; see GroupPlaytest.md |

Validation used an isolated project copy. A long staging path initially prevented package import; a shorter path resolved it. Testing exposed crowding at arrival with 1.8-unit spacing. The minimum spacing now leaves room for an arriving agent between settled neighbours (2.4 units for these agents), and the complete suite passed afterward.

TheGroup is the default build scene. TheWanderer remains enabled in Editor build settings for regression tests; the separate Group Windows build includes TheGroup only. Preview: Images/TheGroup.png. Local executable: ../Builds/WindowsGroup/WonderGather.exe. Generated builds, logs and XML test reports remain excluded from Git. The rendered preview validates scene appearance, not runtime HUD interaction. Movement uses destination slots and independent navigation; automatic formation reshaping and locked formations during travel are deferred.

## The Gatherer and reachable movement — September 7, 2026

The user accepted The Group and chose 0.005 zoom. The Gatherer is the first implemented step toward The Little Settlement, using one provisional supply resource and one depot.

| Check | Result |
|---|---|
| Unity 6000.6.0f1 complete PlayMode suite | 14 passed, 0 failed; 91,8194654 seconds |
| Reachable movement | Island, edge, wall and far-off targets resolve to complete routes and separate destinations; eight units arrive at fallback slots |
| Economy | Eight workers exhaust all 120 supplies through repeated trips; remaining + carried + stored stays exactly 120; carrying stays within 0–5 |
| Interruption and lifecycle | Move orders cancel work and retain cargo; manual delivery works; disabled workers stop; unavailable resource returns carried cargo |
| Regressions | Existing group routing, selection, layout and strict navigation tests pass |
| Windows x64 development build | Passed; GATHERER_BUILD_OK |
| Scene preview | Rendered with graphics and visually inspected |
| Physical input and gameplay feel | Pending user playtest in GathererPlaytest.md |

The initial 14-test suite passed. An additional disable-position assertion then exposed residual movement after ResetPath; explicitly stopping the agent and clearing velocity fixed it. The final suite above includes that assertion. Existing Unity 6.6 obsolete object-discovery warnings remain in older editor/test code; new code uses current discovery APIs. Generated reports and logs remain local and excluded from Git.

New scene: Assets/_WonderGather/Scenes/TheGatherer.unity. Local build: Builds/WindowsGatherer/WonderGather.exe. Preview: Docs/Images/Gatherer/TheGatherer.png. Earlier scenes remain available; the standalone build starts only TheGatherer. No packages, render settings or existing prefab GUIDs were changed. User recovery files and unrelated local settings remain untouched.

Scope limits: fixed interaction offsets, one placeholder resource and depot, no persistence or construction/production. Nearby formation search is bounded and may still reject an order when no suitable space is found. Gathering requires a complete route to its interaction spot. This is a playable prototype, not a final economy or resource balance.

## HUD clipping correction — September 8, 2026

The user accepted the gathering loop and reported that the final status line was clipped. The economy HUD still used a fixed 215-pixel height. The panel now takes its height from its laid-out contents, wraps labels, and limits its width to the Game view. Pointer exclusion uses the actual visible panel bounds.

Unity 6000.6.0f1 compiled the change. A temporary PlayMode capture completed in a separate rendered Editor session, and the 804 × 400 image was visually inspected: all rows and the final gathering status fit inside the panel. Evidence: Images/GathererHud.png. The batch screenshot attempt could not produce an image; the rendered Editor check succeeded. Temporary probe code was removed. The Windows x64 development build succeeded (GATHERER_BUILD_OK); the existing gameplay suite was not repeated for this presentation-only change.

## Construction — September 8, 2026

The Little Settlement now supports preview placement, stored-supply payment and one-worker construction. Workshop values (20 supplies, eight seconds, three-unit footprint) are provisional asset data. Previous prototype scenes and 0.005 zoom are preserved.

| Check | Result |
|---|---|
| Unity 6000.6.0f1 PlayMode suite | 18 gameplay tests plus one temporary visual probe passed; 126.58 seconds |
| Invalid placement | Insufficient funds, wall, island, edge, non-finite coordinates and unit overlap reject without payment |
| Preview cancellation | No supplies spent |
| Construction | Worker travels and finishes; duplicate footprint rejected; payment occurs once |
| Interruption | Move pauses progress; resume finishes without extra cost; gathering replaces building; disable releases site |
| Navigation | Completed workshop footprint is absent from the walkable NavMesh |
| Existing systems | All 14 movement, selection and gathering regression tests passed |
| Visual check | Separate rendered Editor capture inspected; workshop, progress and HUD visible; temporary probe removed afterward |
| Windows x64 development build | Passed; SETTLEMENT_BUILD_OK |

The first run passed 17/18; the failing invalid-placement test had incorrectly treated the clear world origin as a wall. The test was corrected to the authored wall at x=4. The final full run above passed. A subsequent capture verified minor HUD wording changes on visible ground. Physical B-key and mouse placement feel await user playtesting; tests exercise the same placement and order APIs. Existing obsolete discovery warnings in older Editor/test code remain unchanged.

Scene: Assets/_WonderGather/Scenes/TheSettlement.unity. Local executable: Builds/WindowsSettlement/WonderGather.exe. Visual evidence: Images/Construction.png. See ConstructionPlaytest.md. No packages or existing scene/prefab GUIDs were changed. Placement assumes the current flat terrain; unit production, multiple builders per site, demolition/refunds and persistence remain deferred.

## Worker production — September 8, 2026

The user accepted construction. The Little Settlement now includes one worker production option per completed workshop, with provisional 10-supply cost, six-second duration and three-entry queue capacity.

| Check | Result |
|---|---|
| Unity 6000.6.0f1 full PlayMode suite | 22 gameplay tests passed, 0 failed; 207.89 seconds |
| Final selection-feedback follow-up | Four production tests passed again |
| Queue rules | Unfinished buildings, insufficient funds and full queues reject without payment; cancellation refunds and preserves earlier progress |
| Produced worker | Spawns safely, joins box selection, moves, gathers and completes another building; destruction unregisters it |
| Blocked exit | Ready worker waits; opening space spawns it without another charge |
| Multiple workshops and lifecycle | Independent queues; disabled producer pauses; destruction refunds pending orders while depot exists |
| Rendered PlayMode capture | Separate visual probe passed; production controls and spawned worker inspected in Images/Production.png |
| Windows x64 development build | Passed; PRODUCTION_BUILD_OK |

Temporary capture code was removed before the player build. Tests and logs remain local. The source scene starts with zero stored supplies; test funding was confined to tests. Physical T-key and button interaction still needs user playtesting. No packages, base prefab GUIDs or earlier scene files were changed. Existing obsolete discovery warnings in older Editor/test code remain.

Scene: Assets/_WonderGather/Scenes/TheProduction.unity. Local build: Builds/WindowsProduction/WonderGather.exe. See ProductionPlaytest.md. Limits: one worker type, no rally point/population cap/persistence, bounded nearby spawn search and simple expanding gathering offsets. Production data is assumed fixed during a match; values remain open for future tuning.


## Civilization blueprints — September 9, 2026

Unity 6000.6.0f1 imported the new scripts and authored both sample scenes,
blueprint assets and starting-base prefab (CIVILIZATION_SETUP_OK). The first
launch was blocked by license activation; the subsequent editor run succeeded.

- Full PlayMode suite: 27 passed, 0 failed;
  280,8304446 seconds. Local evidence: TestResults/civilization-full.xml.
- Five civilization tests cover data-driven starting setups, the complete
  gathering/construction/production chain, produced blueprint inheritance,
  construction and gathering permissions, seeded/unseeded cycles, missing
  references, duplicate IDs and nonblocking economy warnings.
- The first targeted run passed 4/5. Its gather-to-20 test reached 15 delivered
  supplies at a 55-second deadline. The deadline was increased to 90 seconds
  for four real trips; assertions and production behavior were retained, with
  diagnostic worker state added on failure. The full suite above passed.
- Separate rendered PlayMode visual probe passed; screenshot in
  Images/Civilization.png. Temporary probe source and metadata removed before
  the player build.
- Windows x64 development build passed (CIVILIZATION_BUILD_OK), output
  Builds/WindowsCivilization/WonderGather.exe.

The report proves structural reachability only. It does not prove whole-chain
affordability, finite-map resource sufficiency, resilience, or competitive
legality. Supplies remain the only resource; worker prefab contracts and one
recipe per building are current limits. Physical controls, ease of editing and
design feel await user playtesting. Existing obsolete API warnings in older
test/editor sources remain. No packages or older authored scenes were changed.


## First player-facing faction creator — September 9, 2026

- Unity 6000.6.0f1 created/imported TheFactionCreator and FactionPlaytest using
  editor authoring APIs (CREATOR_SETUP_OK).
- Full PlayMode suite: 31 passed, 0 failed;
  298,41571 seconds. Local result: TestResults/creator-full.xml.
- Four creator tests cover cloned draft edits, runtime gather/build/production
  permissions, unchanged source assets, repeated playtest/return cleanup,
  updated counts/resources on relaunch, blank-name rejection, permissive
  challenge warnings and map bounds. Previous gameplay regressions also pass.
- Separate rendered probe exercised normal and warning UI states, launch and
  return. Captures inspected: Images/FactionCreator.png,
  Images/FactionCreatorWarnings.png, Images/FactionCreatorPlaytest.png.
- The first visual inspection exposed scaled arrow transforms and a clipped
  starting-panel note. Both were corrected; the rendered probe passed again
  and the final captures were inspected. Gameplay behavior was unchanged.
- Temporary capture source/meta removed before the Windows x64 development
  build. Build passed (CREATOR_BUILD_OK); output:
  Builds/WindowsFactionCreator/WonderGather.exe.

Runtime tests invoke the same draft and scene-transition methods as the UI;
physical mouse/keyboard interaction and final layout acceptance remain user
playtests. Current UI uses a scaled 1280×720 reference and scrolling details/
warnings. Session-only drafts have no save/load. The sample has one worker,
one workshop and a fixed starting depot. Design costs and final aesthetic
remain open. No packages or earlier scene assets were modified. Existing
obsolete Unity API warnings in older test/editor code remain.


## Faction saving and library — September 9, 2026

- Unity 6000.6.0f1 full PlayMode regression run: 39 passed, 0 failed;
  296,8404493 seconds. Result: TestResults/library-full.xml.
- After adding active-rename conflict detection and a short-lived exclusive
  save lock, all nine final persistence tests passed. Result:
  TestResults/library-final.xml. The final project contains 40 tests; the
  entire 40-test suite was not rerun because these final changes were isolated
  to persistence and covered by that focused run.
- Covered round trips, complete creator choices, independent copies,
  renaming/deleting, unsaved replacement guards, backup rotation, external
  changes, cooperative writer conflicts, corrupt/unknown/missing fields,
  unavailable blueprint IDs, invalid names, failed writes preserving old
  files and dirty drafts, and loading/playtesting in a fresh creator session.
- The initial targeted run passed 5/8; invalid-data exceptions were missing
  from the storage error filter. The filter was corrected, then the full and
  final runs above passed. No error cases or assertions were disabled.
- Rendered probe passed and four captures inspected: FactionSaving,
  FactionLibrary, FactionUnsavedPrompt and FactionQuitPrompt in Docs/Images.
  The probe also verified that unsaved changes reject a close request and
  that Cancel clears the quit prompt. Temporary probe removed before build.
- Windows x64 development build passed (CREATOR_BUILD_OK), at
  Builds/WindowsFactionCreator/WonderGather.exe.

All automated persistence and visual tests used uniquely named temporary
directories and cleaned only those directories. Actual player saves were not
used. The format is local version-1 faction design data, not a running-match
save. Unsupported formats are refused rather than migrated. Atomic replacement,
one previous backup and recoverable deletion were exercised on Windows; cloud
sync and other operating systems were not validated. Editor Stop Play Mode
bypasses player quit prompts. Physical UI interactions and flow acceptance
remain user playtests. No package or authored scene changes were required.


## Multiple unit blueprints — September 9, 2026

- Baseline: e370cf0, prior regression/persistence and Windows build passed.
  Primary user changes to TheGroup, PackageManagerSettings, URPProjectSettings
  and the Recovery assets were preserved. Validation used the isolated WGGroup
  copy with Unity 6000.6.0f1, unchanged packages and existing creator scenes.
- First targeted run: 18/19 passed. A boundary test exposed AddUnit returning
  early at the roster limit before rejecting a foreign blueprint reference.
  Ownership checking now precedes that limit. No assertion was removed.
- Final full PlayMode suite: 46 passed, 0 failed, 317,184863
  seconds. Result: Docs/TestResults/units-full.xml (local ignored artifact).
- Six new permanent tests cover independent clones, map/roster limits, invalid
  names and foreign references, removal of linked units, complete round trips
  after removing the original blueprint, dirty tracking, strict nested format
  checks, legacy migration without read-time writes and with save-time backup,
  and mixed starting units followed by workshop construction and production
  of the selected blueprint with the correct permissions.
- Existing malformed-save tests now target version 2 and per-unit start fields;
  separate new tests preserve version-1 compatibility and unknown-field refusal.
- Rendered visual probe passed. Inspected UnitBlueprints,
  WorkshopBlueprintChoice, BlueprintRemovalPrompt and UnitBlueprintsMany in
  Docs/Images, including the eight-blueprint list and long names. This checks
  rendered states; manual mouse interaction/feel remains user acceptance work.
- The first long-name capture exposed horizontal overflow in the detail panel.
  After the full suite, presentation-only width/compact-label corrections
  were applied and the rendered probe rerun successfully before building.
- Removed the temporary probe before the Windows x64 development build.
  Build method WonderGather.Editor.FactionCreatorSetup.BuildWindows reported
  CREATOR_BUILD_OK; output Builds/WindowsFactionCreator/WonderGather.exe.

Save tests used owned temporary directories, never actual player saves. No
scene, prefab, package or project settings changes were required. The schema
is version 2 with strict read support for version 1; an older game build cannot
read new version-2 files. Prototype scope remains one trained unit type per
workshop, shared worker visuals/recipe, eight blueprints and eight starting
units. This is not validation of final graph design, balance, localization,
controller support, other platforms or multiple recipes per production queue.


## Building blueprints and mixed production — September 10, 2026

- Baseline: ec8f64f; prior 46-test suite and Windows build passed. The primary
  user's modified TheGroup scene, PackageManagerSettings, URPProjectSettings
  and Recovery assets were preserved. Unity 6000.6.0f1 and the isolated WGGroup
  copy were used, with unchanged packages and authored scenes/prefabs.
- Initial existing creator/persistence compatibility run: 15 passed, 0 failed.
- New tests initially failed compilation because three collection assertions
  selected a string-only NUnit overload. The assertions were corrected to
  membership checks without weakening their expected behavior.
- Focused building-network run: 7 passed, 0 failed, including complete graph
  round trips, incoming/outgoing link cleanup, copies, limits/invalid data,
  fixed-point reachability across multiple options and buildings, version-2
  migration without read-time writes, and backup on explicit upgrade.
- Final full PlayMode suite: 53 passed, 0 failed, 345,4892977
  seconds. Local result: Docs/TestResults/buildings-full.xml.
- Runtime tests construct a named building and train two different blueprints
  in order. A separate mixed-order test varies test-only recipes and checks
  original payments/durations, blocked-exit retention, cancellation and
  destruction refunds. The earlier movement/gathering/construction/production
  and version-1 migration tests also passed in the full suite.
- Rendered probe passed. Six captures in Docs/Images show the building roster,
  construction links, removal confirmation, eight-type/long-name case, and
  mixed-production HUD at top/bottom scroll positions. Visual state and layout
  were inspected; physical interaction and design acceptance remain playtests.
- After the full run, explicit roster/option counts were added to the dirty
  snapshot and empty-training HUD wording clarified. Final rendered run:
  16 focused network/persistence tests plus one visual probe passed (17/17),
  recorded in Docs/TestResults/buildings-final.xml. The first captures exposed
  a narrow horizontal overflow and clipped footer in the creator. Width/text
  adjustments were followed by another successful visual-only probe, including
  long training labels and HUD pointer exclusion, in buildings-visual.xml.
- Temporary visual probe removed before Windows x64 development build.
  FactionCreatorSetup.BuildWindows reported CREATOR_BUILD_OK; output:
  Builds/WindowsFactionCreator/WonderGather.exe.

Save tests use owned temporary directories, never player saves. Version 3
writes the supported network; versions 1/2 migrate only in memory until an
explicit save or rename. Earlier game builds cannot read version-3 saves.
No package, scene or prefab changes were needed. New BuildingBlueprint fields
preserve the existing serialized first-production reference and default extra
options to empty; custom labels are explicit, leaving authored recipes intact.
No final aesthetic, balance rules, controller support, localization, other
platforms, multiplayer or custom starting-base production was validated.


## Prototype unit performance — September 20, 2026

- Baseline: c6745fb; previous 53-test regression suite and Windows build passed.
  User changes to TheGroup, PackageManagerSettings, URPProjectSettings and
  Recovery assets were preserved. Validation ran in the isolated WGGroup copy
  using Unity 6000.6.0f1, without package, authored scene or prefab changes.
- Focused first run: four passed, one failed because the construction test
  omitted ChooseBuilding before TryPlace. Corrected the test setup; no runtime
  fix or weakened assertion. Final focused run: five passed, zero failed.
- September 10 full PlayMode regression: 58 passed, zero failed, 374,639018
  seconds. Evidence: Docs/TestResults/stats-full.xml (local, ignored).
- New coverage checks independent duplicate values, defaults, dirty state,
  save/load and reset, invalid bounds and nested fields, version-3 read-only
  migration with backup on explicit upgrade, and starting/produced inheritance.
  Runtime checks verify 6.4 navigation speed at 200%, non-compounding repeated
  application, two-supply capacity, 0.25-second gathering intervals, delivery
  conservation, and about four seconds of construction work at 200%.
- Existing movement, gathering, construction, production, creator, graph,
  library and version-1/version-2 migration regressions passed in the full run.
- One rendered visual probe passed. Four inspected captures in Docs/Images
  show top/middle/bottom creator scroll positions and the selected-unit HUD;
  pointer exclusion was asserted. Temporary probe removed before full tests
  and build. Physical UI clicking remains the user's playtest.
- September 20 Windows x64 development build via FactionCreatorSetup.BuildWindows succeeded
  (CREATOR_BUILD_OK). Local output: Builds/WindowsFactionCreator/WonderGather.exe.
  The standalone executable was built but not separately launched in this pass.

Save tests use owned temporary directories, not player saves. Schema version 4
adds explicit performance values. Versions 1–3 load with original defaults;
explicit save/rename upgrades with the existing backup. Older executables
cannot read version-4 files. New authored blueprint fields default to the
previous behavior; runtime tests exercised existing serialized assets.

No final body/animation model, pricing/balance formula, aesthetic, multiplayer,
other platform, controller input or localization was implemented or validated.
Rates are temporary outcome controls, not balanced tradeoffs.


## The Living Body — September 22, 2026

- Baseline: 0ebe23b, matching GitHub main at the start of this milestone.
  Prior 58-test suite and Windows creator build passed. Work used the isolated
  WGGroup copy and Unity 6000.6.0f1. Existing user edits to TheGroup, package/URP
  settings and Recovery assets were preserved; no packages changed.
- Unity authored a new scene, shared rig prefab, seven materials, ramp mesh
  and NavMesh (LIVING_BODY_SETUP_OK). The build scene is appended to existing
  entries. Old scenes/prefabs and faction saves remain unchanged.
- Initial compile exposed a missing raycast direction argument, corrected
  before runtime checks. Focused six tests passed (43,6833359s).
- Added a constant-leg-length assertion and inspected six rendered captures.
  The stronger ramp test exposed a 0.688m leg versus the intended 0.68m; a
  minimum pelvis-height clamp was overriding reach. The clamp was removed.
  Visual inspection also showed excessive crouch and feet landing behind the
  moving root. Rest posture and predictive landing placement were corrected;
  arm counter-swing is tied to actual steps rather than an independent phase.
- Final three LivingBodyTests plus the rendered probe passed, 4/4 in
  49,73184s. Tests check alternating support,
  fixed world-space planted feet, constant leg lengths, grounded normals on
  both ramp directions at both paces, arrival, stops/idle drift, redirection,
  and re-enable/reset without changing the navigation root.
- The last visual pass corrected horizontal rings cutting into the slope.
  Rings now follow support normals. One final rendered probe passed, 1/1;
  its six captures were inspected (Docs/Images/LivingBody*.png). Poses, steps,
  standing on the slope and the route HUD were reviewed. These are scripted
  runtime captures, not a claim of final aesthetic acceptance or manual input.
- Temporary visual probe removed before full tests/build. Full PlayMode suite:
  61 passed, zero failed, 401,9977968s. Local evidence:
  Docs/TestResults/living-full.xml. Earlier economy, creator, save migration,
  production, group selection and movement regressions passed.
- Windows x64 development build through LivingBodySetup.BuildWindows passed
  (LIVING_BODY_BUILD_OK), output Builds/WindowsLivingBody/WonderGather.exe.
  The executable was built; standalone interactive playtesting was not run.

This is a navigation-driven, kinematic presentation experiment. Physical
balance, ragdolls, falls, combat, work animations, arbitrary creature anatomy,
stairs/moving terrain, multiplayer and RTS-scale performance are unproven.
The scene's opt-in terrain-aware camera preserves default behavior elsewhere.
The user's judgment of weight, pace, expression and aesthetic fit remains the
next acceptance step. Faction schema remains version 4.


## The Living Worker — September 26, 2026

- Baseline b9d2902 matched GitHub main. The prior 61-test suite and Living
  Body Windows build passed. Work used the isolated WGGroup project and
  Unity 6000.6.0f1. Existing primary edits to TheGroup, package/URP settings
  and Recovery assets were preserved. No packages, build scene settings,
  faction file schema, blueprint IDs or economic recipe values changed.
- Unity compilation and targeted authoring passed (LIVING_WORKER_SETUP_OK).
  Created the worker prefab variant, production recipe and two materials.
  Updated the civilization worker's recipe reference, base delivery
  surfaces/positions and faction playtest resource/camera integration. Unity
  also serialized the worker's existing default performance values explicitly;
  their behavior is unchanged.
- Five focused LivingWorkerTests passed in 105.45 seconds.
  These cover the actual starting and produced worker paths; complete
  gather/carry/delivery; constant leg lengths and ground support; real hand
  contact; partial-cargo interruption/disable/return; eight workers sharing
  two test stations; depletion and reservation release; and movement/rate/
  capacity extremes. Test storage is isolated from player faction files.
- Fresh read-only runtime review found no actionable defects. Focused and
  visual evidence supplement that static review; it is not runtime proof.
- The first rendered probe passed. Inspection found that some new visible
  work surfaces lacked click colliders and the delivery camera was occluded.
  Unity authored colliders on the rack, supplies, marker and shelves
  (LIVING_WORKER_INTERACTION_OK). The integration test now also checks that
  supply boxes and shelves resolve to resource/depot command targets.
- The first full regression run passed 65/66; the earlier Living Body slope
  test detected stretched legs. Its focused rerun passed. Inspection found
  the unchanged solver could lower the pelvis but not recover horizontal
  overreach. A controlled 1.5 m navigation correction reproduced the same
  defect before the fix (.740 m segment versus its authored .68 m). The
  exact timing trigger in the first slope run was not captured.
- The visual pelvis now stays within both legs' reachable volume while
  navigation and planted feet retain their authority. Mutually unreachable
  contacts use the existing presentation-reset policy for discontinuities.
  Added a deterministic navigation-correction regression; all nine Living
  Body/Worker tests passed in 144.86 seconds.
  No assertions were weakened or tests excluded. Initial failed reports
  were retained separately from the final full-suite report.
- Final rendered probe passed 1/1 in 17.70 seconds.
  Seven final captures were inspected: overview/HUD, gathering, carrying,
  holding after interruption, delivery, empty-handed, and fast movement.
  The temporary probe was removed before the full suite and build. Captures
  are in Docs/Images/LivingWorker*.png. These are scripted Editor runtime
  captures, not a manual mouse/keyboard or final aesthetic acceptance claim.
- Full PlayMode suite: 67 passed, zero failed, 500.35 seconds.
  This includes the final click-surface assertions and earlier camera,
  movement, economy, construction, production, creator, graph, persistence,
  migration, performance and Living Body checks. Local reports are under
  Docs/TestResults/worker-*.xml (ignored generated evidence).
- Windows x64 development build via LivingWorkerSetup.BuildWindows passed
  (LIVING_WORKER_BUILD_OK): Builds/WindowsLivingWorker/WonderGather.exe.
  Standalone interactive playtesting was not performed.

Known limits: primitive provisional bodies, one supplies fiction, and a small
authored work course. Existing saved factions resolve to the worker template
with the new body; their version-4 data and settings keep their meaning.
Construction retains its earlier gameplay with no new work gesture. No
active ragdolls, physical balance, load penalties, personality, combat,
arbitrary anatomy, network model or RTS-scale performance budget is claimed.
The user's judgment of weight, contact, carrying support and readability is
the next milestone acceptance step.


## Source review and design clarification — September 29, 2026

- Baseline 5f61351 was confirmed against GitHub main. Luis accepted the Living
  Worker as an early prototype on September 28, with deeper showcase direction.
- Reviewed design/context/history and the relevant gameplay, body, blueprint,
  creator, persistence, test and configuration sources. A separate read-only
  review checked the equipment/contact integration and save/dirty-state boundaries.
- Confirmed timer-based resource extraction, presentation-only tool-free work
  poses, fixed body dimensions and absence of equipment/strength/load modeling.
  These are prototype limitations, not newly reproduced runtime failures.
- Recorded the one-worker showcase reference, an Equipped Worker recommendation,
  explicit open mechanics and compatibility/acceptance requirements. Archived
  the completed Living Worker plan and updated current-state/acceptance pointers.
- Documentation changes only. No Unity tests, Editor scene operations, build
  or profiler run occurred for this review. The latest executed runtime evidence
  remains September 26: 67 passing PlayMode tests and the Windows build above.
- Existing human edits in TheGroup, package/URP settings and Recovery assets
  were preserved. No source, asset, package, save data or serialized setting
  changes are part of this documentation update.


## The Equipped Worker — September 29, 2026

- Baseline 1dce817, after Luis approved the documented scope. Implementation
  and Unity validation used the isolated WGGroup project, Unity 6000.6.0f1 / URP.
  Human edits in TheGroup, package/URP settings and Recovery assets were preserved.
  No package, blueprint ID, recipe price, navigation authority or final art change.
- Unity compile/authoring passed (EQUIPPED_WORKER_SETUP_OK). Authored pickaxe
  prefab/definition/materials, matching mineral render/collision mesh, eight
  stations and the separate EquipmentPlaytest map. Preserved the original map,
  prefab variant GUID and session/resource references; added the map to build scenes.
- Initial persistence run: 6/6 passed, 14.12 seconds. Independent save review
  found a tool-ID/save boundary mismatch; definitions now share the printable
  1–64-character constraint, with explicit boundary/serialized-invalid tests.
- Initial focused run: 13/15 passed, two failed on the unchanged 0.015 m grip
  assertion during travel. HandPosition cached old world coordinates while the
  parented tool and rendered limbs already followed the navigation root. Store
  root-local hand coordinates and report through TransformPoint. Both real
  starter/produced loops then passed in 34.89 seconds. Assertions were preserved.
- Independent runtime review found a mid-strike origin-overlap gap in sphere
  casts and a scale mismatch possibility. Every unspent strike segment now
  checks origin overlap. Fixed-scale tool definitions and worker/model scale
  guards prevent displayed and queried paths from diverging. Added regression
  tests for a blocker entering the swing and initial/mid-work scale rejection.
  The fresh follow-up source review reported no remaining actionable findings.
- Final focused equipment/persistence suite: **18 passed, zero failed**,
  116.30 seconds. Covers valid hit/deduplication, real complete starter/produced
  loops, grip/support, absent tool, moved surface, nearer blocker and mid-swing
  overlap, unreachable grip, cancel/disable/re-enable, shared depletion,
  coarse stationary action frames and supported rate extremes; plus choice,
  dirty/duplicate/copy/template isolation, strict catalog/fields, unknown-ID
  safety, v4 None/defaults/read-without-rewrite/backup upgrade and failed writes.
  Earlier v1–3 migration tests remain in the full suite.
- Rendered probe: **1/1 passed**, 16.58 seconds. Seven captures inspected:
  creator, overview/HUD, preparation, contact, recovery, carrying and delivery.
  Refined the stow pose to clear the torso and route around the side. Captures
  are Docs/Images/EquippedWorker*.png. Batch mode did not produce screenshots;
  the successful probe used the rendered Editor workflow. The temporary probe
  and metadata were removed before final tests/build; source retained locally
  in the conversation work directory. These are scripted views, not a manual
  mouse/keyboard or aesthetic acceptance claim.
- Full PlayMode suite: **85 passed, zero failed**, 626.51 seconds, including all
  earlier movement, economy, construction, production, graph/creator, save,
  performance and Living Body/Worker regressions. No tests disabled or weakened.
  After that run, the equipment grip check gained a minimum arm-reach bound
  matching the existing IK solver. The unreachable-grip test now also places
  a grip at the actual shoulder to exercise that boundary. All 18 focused tests
  passed again on this final candidate, followed by a fresh Windows build.
  Independent source review confirmed the fixture targets the minimum bound
  and the authored pickaxe grips remain comfortably within the accepted range.
  Initial failure and final reports are retained under ignored local
  Docs/TestResults/equipment-*.xml; execution logs are Logs/equipment-*.log.
- Windows x64 development build via EquippedWorkerSetup.BuildWindows passed
  (EQUIPPED_WORKER_BUILD_OK). Output: Builds/WindowsEquippedWorker/WonderGather.exe;
  scenes: TheFactionCreator, FactionPlaytest, EquipmentPlaytest. Artifacts remain
  local; source/assets/docs are versioned. Standalone interactive testing was
  not performed. Existing obsolete-API/editor/native diagnostics remain in logs;
  this work does not claim warning-free third-party packages or measured performance.

Ready for Luis's prototype playtest with explicit limits: primitive fixed-scale
body/tool, fixed one-supply hit yield, bounded two-handed action, feet-side cargo
representation and short back-stow transfer. No strength/mass/burden, container
loading, loose material physics, detailed mounting gesture, arbitrary body editor,
final art, networking, public-showcase readiness or RTS-scale performance claimed.

## Equipped Worker review and source analysis — September 30, 2026

- Luis playtested The Equipped Worker in the Editor. Today's Editor log shows
  Play Mode sessions in TheFactionCreator and EquipmentPlaytest with no runtime
  exceptions; it contains only existing obsolete-API compiler warnings and
  offline package-registry errors.
- Experiential result: accepted as a foundation, not as motion quality. See
  EquippedWorkerPlaytest.md.
- Documentation-only change. No Unity tests, builds or rendered probes were run.
  The gait, cadence, Froude and clipping figures in NextMilestonePlan.md were
  computed from source constants, not measured in a running scene.
- Human workspace state was preserved: TheGroup.unity, PackageManagerSettings,
  URPProjectSettings and Assets/_Recovery.

## Strength and Burden, checkpoint A: the grounded body — September 30, 2026

- **Environment.** Validation ran in an isolated git worktree (branch
  `claude/grounded-body` from 6131598), seeded with the previous isolated
  project's Library. The project open in the Editor was not used for runs.
  Validated files were copied into it afterwards, and byte comparison confirmed
  they matched. Human edits in TheGroup, package/URP settings and `_Recovery`
  were preserved.
- **Baseline.** On the untouched code the full PlayMode suite passed
  **85/85** in 628.7 s.
- **Authoring.** `GroundedBodySetup.Apply` passed (GROUNDED_BODY_SETUP_OK):
  - LivingBodyBiped feet became 0.13 × 0.09 × 0.26 m, with new toe segments;
  - the LivingWorker variant changed from 3.2 m/s at 10 m/s² to 1.8 m/s at
    4 m/s² (decision D2);
  - no other prefab, scene or package changed.
- **First focused runs.**
  - Gait and Living Body tests passed 7/7.
  - Worker, equipment, performance and gait tests passed 24/25. The 25%
    Movement extreme exceeded its 65 s budget: its first delivery came at
    65.7 s at 0.45 m/s. Its time budget became 120 s, with elapsed time
    reported and all other assertions unchanged. At 200% the jog delivered in
    14.2 s.
- **Assertions changed for approved decisions:**
  - expected agent speeds changed from 3.2/6.4 m/s to 1.8/3.6 m/s;
  - worker support checks now allow only a brief flight phase (under 0.2 s)
    outside a walk. Walking still requires a planted foot every frame,
    unchanged in LivingBodyTests and enforced again in the new tests.
- **Measured in the new tests (TheLivingBody):**
  - walk: 2.08 steps/s, stride 1.85 m, foot pitch −12° to 30°, never airborne;
  - jog at 3.6 m/s: 3.13 steps/s, stride 2.29 m, longest flight 0.077 s.
- **Rendered review.** A temporary probe (not committed) rendered walking,
  jogging, standing, settling, walking with the pickaxe and carrying cargo home.
  It exposed two defects the tests had not:
  1. Living Body walkers' free arms hung with flared elbows. Free arms now use
     a pendulum target with elbows pointing back.
  2. A body that turned before departing never entered its gait: standing
     adjustment steps chained while the root moved, and all 14,156 steady
     frames were "Standing". Adjustment steps now start only below the gait
     start speed. The new regression test failed before the fix and records
     0 standing frames after it.
- **Final runs.**
  - Gait and Living Body tests: **8/8**.
  - Full PlayMode suite: **89/89** in 808.2 s (85 existing plus 4 new
    GroundedBodyTests). No test was disabled.
- **Windows build.** The x64 development build via
  `GroundedBodySetup.BuildWindows` passed (GROUNDED_BODY_BUILD_OK). Output:
  `Builds/WindowsGroundedBody/WonderGather.exe`, containing TheFactionCreator,
  FactionPlaytest and EquipmentPlaytest.
- **Not tested:**
  - interactive play in the standalone build or in the open Editor;
  - gait quality on stairs, steep or rough terrain, or among many simultaneous
    walkers;
  - performance at RTS scale.
  Rendered frames were reviewed as stills, not video. Luis's judgment of feel,
  pace and gait character is pending.

## Arrival fix and a shared-position deadlock — October 1, 2026

All validation ran in the isolated worktree; Luis's open project was not used
for runs.

### Arrival

- **Diagnosis.** A temporary frame trace (not committed) showed both a Living
  Body walker and a faction worker making two separate standing adjustment
  steps, 0.4–0.7 s after the root had already stopped.
- **After the fix.** One closing step follows immediately, and then the body
  stands still.
- **Regression test.** `GroundedBodyTests.ArrivalFinishesTheStrideWithoutShuffling`
  allows at most one step after the root comes to rest and requires 2 s of
  stillness. It records exactly 1 step.
- **Results.** The gait and Living Body tests passed 9/9.

### Erratum for checkpoint A

The first full run including the arrival fix failed one test:
`LivingWorkerTests.SharedLimitedPositionsQueueAndDrainAResourceWithoutLosingSupplies`
timed out at 90 s with 16 of 17 supplies delivered. A ten-attempt diagnostic
probe (not committed) measured the stall rate:

| Code under test | Stalls |
|---|---|
| Original 6131598 | 0 of 10 |
| Checkpoint A, at both its original and its new pace | 1–4 of 10 per run |

So the checkpoint A full run reported as 89/89 on September 30 was a real
pass, but it hid an intermittent failure of about 30%. That failure was caused
by the slower pace's timing, not by the arrival fix.

### Cause and fix

- **Cause.** A worker that ran out of work went idle while standing on its
  station position. Reservations considered that position free, but agents
  need 0.76 m clearance, so the next claimant could never reach it, or was
  boxed in by idle bodies near the station.
- **`Gatherer.StandAside`.** A worker left without work steps 1.2 m clear of
  the station it was using, if it is within 3 m of it.
- **`ResourceWorkplace` claims.** Claims skip a position that another worker's
  body physically occupies, using a non-allocating overlap query on unit
  colliders.
- **After the fix.** The probe completed 20 of 20 attempts in 22–28 s.

### New regression tests

- `LivingWorkerTests.AnIdleBodyOnAReleasedPositionIsNotClaimedUnderneathIt`
  (11.9 s; by construction it fails without the occupancy check). Its first
  run failed because the test did not wait for a physics step after warping
  the idle body; it now waits for two fixed updates before claiming.
- `LivingWorkerTests.WorkersLeftWithoutWorkStepClearOfTheStation`.

The Living Worker suite passed with both tests, apart from that one test-side
failure, which was fixed and then passed on its own.

### Two further intermittent failures

The next full run (92 tests) failed two more tests intermittently. Each passed
3 of 3 times in isolation afterwards.

**`CivilizationTests.BlueprintWorkerGathersBuildsAndProducesItsOwnBlueprint`
(time budget).** One worker must gather 20 supplies within 90 s. At the time
limit it had 15 stored and 5 in hand, on its last trip home. The whole test
took 73 s at the original 3.2 m/s and 92–110 s at the approved 1.8 m/s, so the
budget became 150 s. No assertion changed.

**`LivingBodyTests.NavigationCorrectionKeepsSupportAndLegReach` (a body defect,
older than checkpoint A).**

- **Symptom.** A leg segment measured 0.445 m instead of 0.68 m, 0.2 s after a
  1.5 m warp.
- **Diagnosis.** Temporary instrumentation (not committed) caught 10 occurrences
  in 40 repeated corrections. The pelvis collapsed to about 0.17 m, ankle
  height, so the legs lay horizontal and the knee had no bend direction.
- **Cause.** Each frame, the reach-projected hip height was written back as the
  next frame's target height. While both feet were out of horizontal reach,
  every projection landed lower. At test frame rates (about 5000 fps) the
  pelvis reached the ground within milliseconds, before its time-based
  recovery could act.
- **Fix.** The target height stays separate from the projected result.
- **After the fix.** The instrumented repeat run caught 0 occurrences, and the
  gait and Living Body tests passed 14/14.
- **Regression test.** `GroundedBodyTests.NavigationCorrectionsNeverCollapseThePelvis`
  applies 8 corrections in both directions on both walkers and checks every
  frame that the pelvis stays more than 0.8 m above the root and the legs keep
  their length. With the old write-back temporarily restored it failed (pelvis
  0.778 m); with the fix it passes.

**A second pelvis drop, found by the new regression test.** The next full run
failed the new test with the write-back already removed: the pelvis was
0.24 m above the root.

- **Diagnosis.** Instrumentation (not committed) caught 30 frames in 6 × 8
  corrections. After a 1.5 m correction, one foot had stepped home while the
  other swung in from 1.34 m away, just inside the 1.33 m leg reach. The height
  rule counted that swinging foot as support and lowered the pelvis to
  ankle height to reach it. Just beyond reach, a separate rule kept the body
  standing, so the old logic was also discontinuous there.
- **Fix.**
  - Only planted feet constrain the pelvis height and the reach projection.
  - A swinging foot that is out of reach is drawn within the leg's reach of
    the hip, with its rendered foot and toe moved accordingly.
  - When support passes to a foot that allows a higher pelvis, the solved
    height rises smoothly. This is an output filter, never fed back into the
    target; lowering stays immediate.
- **Results.**
  - The instrumented repeat run caught 0 drops in 48 corrections, and the
    gait and Living Body tests passed 16/16.
  - Gait measurements were unchanged: walk 2.08 steps/s with a 1.85 m stride,
    jog 3.13 steps/s with flight under 0.08 s, 1 step after arrival, and 0
    standing frames while travelling.
  - Re-rendered walk, jog and carry frames were reviewed. Their quality is
    unchanged, and the playtest images were refreshed from them.

### Final evidence for this step

- **Full PlayMode suite: 93/93 in two consecutive runs** (839.8 s and
  834.9 s). These are the 89 earlier tests plus four new ones: arrival
  without shuffling, the pelvis never collapsing, an idle body blocking a
  claim, and stepping clear of a station.
- **Windows build.** `GroundedBodySetup.BuildWindows` passed
  (GROUNDED_BODY_BUILD_OK). Output: `Builds/WindowsGroundedBody/WonderGather.exe`.
- **Not tested:** interactive play in the build or the open Editor; crowds of
  more than eight workers; terrain beyond flat ground and the gentle ramp.


## S1a: two camera systems — October 1, 2026

Isolated worktree `D:\Dev\WG`, Unity 6000.6.0f1 batch mode. Luis's Editor and
his uncommitted files were not touched.

### What changed

- **`RtsCamera` keeps its component and serialized fields.** Every map's
  preferences survive, for example the 0.005 zoom sensitivity. It gains two
  modes:
  - **Strategy:**
    - faster pan, scaled by height;
    - edge scrolling (full screen only);
    - middle-mouse grab pan;
    - zoom toward the cursor with an exact ground anchor;
    - Alt + middle-mouse rotation.
  - **Explore:** the new `ExploreFlight`.
- **`CameraCollision`.**
  - A sphere sweep with sliding against static scenery.
  - A residual-overlap guard.
  - Ground and ceiling limits.
  - Soft pushes from living bodies, with personal space instead of blocking.
- **`RtsInput`.**
  - New actions: `V` toggle, pointer delta, middle mouse, Alt and Ctrl.
  - In Explore, a right click orders on release only when the press moved
    less than 6 px, so a right-drag can look.
  - Alt + left click orbits instead of selecting.
  - Cursor capture with a return to the starting point.
- **`CameraIntent`.** Devices are read only in `RtsInput`. Tests drive the
  camera with intents.

### Tests: `CameraTests` (9 new)

| Test | Measured |
|---|---|
| Toggling into Explore does not move the view | Position change < 1 cm; near plane 0.02 m |
| Explore approaches the ground but never passes through | Lowest eye height 0.0620 m (radius 0.06) |
| Explore slides along scenery without entering it | Stopped 0.0635 m from a 16 m wall face; slid 3.28 m along it; no stone entered at full speed |
| Explore can look straight up | View direction y > 0.99 |
| Returning to Strategy re-centres on the ground in view | Target within 0.1 m of the viewed point, heading kept, first frame moved < 0.5 m, blend finished within 1 s |
| Follow keeps a walking worker framed | Worst off-centre 3.5° while the worker walked 15 m; distance 4.2 m |
| Workers nudge the camera aside | 0.05 m from the body's surface pushed out to 0.280 m |
| Strategy zoom keeps the ground under the cursor | 0.00 px error (640×480 batch screen) |
| Middle drag moves the ground with the cursor | 0.00 px error for a 75 px drag |

**Full PlayMode suite: 102/102 in two consecutive runs** (845.8 s and
862.6 s). These are the 93 earlier tests plus the 9 new ones.

**Windows build.** `GroundedBodySetup.BuildWindows` passed (the faction
creator, FactionPlaytest and EquipmentPlaytest, now with both camera modes).
It was copied to `Builds/WindowsTwoCameras/WonderGather.exe`.

**Not tested by automation:**

- the feel of speeds and smoothing;
- real-mouse right-click/right-drag discrimination and cursor capture;
- edge scrolling;
- the platform's mouse-wheel scale. The player log records the first wheel
  delta.

## S1b/S1c: the Ordinary Place look test — October 1, 2026

Authored and validated in the isolated worktree. Sources were staged outside
the main project and copied in only after they compiled and their tests
passed.

### Authoring

- `OrdinaryPlaceSetup.Create`, a guarded one-time setup that builds:
  - the painted materials;
  - the Blender house and nature, imported with their materials remapped;
  - the ground definition (regenerated at load, not stored as an asset);
  - house and nature colliders, plus the doorway blocker;
  - warm lights at the Blender anchors;
  - the sun and moon, time of day, grass, smoke, look candidates, the
    benchmark and the post volume;
  - the baked NavMesh and both camera modes;
  - the dressed worker prefab.

  It ran without compile or shader errors and planted 481,213 grass blades.
- **Blender sources.** `Art/Blender/OrdinaryPlace/house.py` and `nature.py`
  produce `House.fbx` and `Nature.fbx`. Previews of both were reviewed during
  modelling.

### Iterations judged from captures

`OrdinaryPlaceCapture.Capture` renders 7 views × 4 times. The path and worker
views are also rendered in every candidate. Problems found and fixed across
four passes:

- The ink shader used `line` (a reserved HLSL word) and did not compile, so it
  was renamed.
- Clouds were invisible because the noise scale was too large for the visible
  sky.
- The ground between the blades read pale. Flat soil takes full sun while
  the blades don't, so the soil is now shaded under dense grass.
- The windows overexposed to white; they now glow amber.
- **Night values.**
  - The hills were brighter than the sky, so the fog is now darker than the
    horizon.
  - The night sky was washed purple by wide bloom from the windows. The bloom
    is now tighter (intensity 0.35, scatter 0.5).
- Distant land turned into a pale wall. Fog now falls off with squared
  distance and is capped.
- **Brushwork** was hard to see. It is now dab-based, in tone and in the
  light's edge.
- **Colour semantics.** `Material.SetColor` treats colours as sRGB in this
  linear project, so the night sky came out about 3× too dark and was tipped
  purple by bloom. The night sky palette is now authored in sRGB.
- The windows saturated to white; their emission is lower.
- The ground meshes would have been about 18 MB of text assets per
  regeneration. They are now rebuilt from the height field when the scene
  loads (`DontSave`), and the scene itself is 309 KB.

### Final evidence

- **`OrdinaryPlaceTests` (5 new)** check that:
  - the scene is complete (481,213 blades, house lights, smoke);
  - the worker walks to the doorstep;
  - Explore cannot enter the house through the door or a wall;
  - night and day switch the house lights;
  - every candidate runs.

  They passed together with `CameraTests` (14/14).
- **Full PlayMode suite: 107/107 in two consecutive runs on the final state**
  (865.8 s and 878.6 s), plus an earlier 858.4 s run. These are the 102
  earlier tests plus 5 new ones.
- **Captures.** The full matched set (141 images) is in the worktree's results
  folder. The comparison sheets are in `Docs/Images/OrdinaryPlace/`.
- **Release build.** `OrdinaryPlaceSetup.BuildWindows` passed
  (ORDINARY_PLACE_BUILD_OK). Output: `Builds/WindowsOrdinaryPlace/WonderGather.exe`.
  The build enables Unity's frame timing stats in Player Settings.
- **Benchmark.** `-wgbenchmark` at 1920×1080 on an RTX 4060 Laptop GPU and an
  i9-14900HX. GPU frame time ranged 3.2–4.1 ms (baseline) to 4.6–5.4 ms (paint
  filter + ink). The full table is in
  [OrdinaryPlaceLookTest.md](Playtests/OrdinaryPlaceLookTest.md).
- **Not tested:**
  - other GPUs and vendors;
  - long sessions;
  - interactive play in the build or the open Editor.

  The captures are edit-mode stills, so the worker stands in its stored pose.

## Moon fix and docs reorganization — October 1, 2026

- **Defect, reported by Luis.** The crescent moon stayed in the sky during
  the day.
- **Fix.**
  - The moon now rides opposite the sun, turned 25°.
  - The sky shader multiplies it by `_MoonVisibility`, which `TimeOfDay` sets
    to 0 once the sun is about 6° above the horizon and to 1 at night.
- **Test.** `NightLightsTheHouseAndDayRestsIt` now asserts that the moon is
  invisible at 06:30, 09:00, 12:00 and 16:00 and visible at 23:00. The
  OrdinaryPlaceTests passed 5/5, and a recapture of the night sky was
  reviewed.
- **Docs reorganization**, at Luis's request for a very organized structure.
  - **Moves.** 81 files moved with `git mv`, so their history is kept:
    - playtest guides to `Docs/Playtests/`;
    - art direction to `Docs/ArtDirection/`;
    - design notes to `Docs/Design/`;
    - technical docs to `Docs/Technical/`;
    - images into one folder per milestone.
  - **Links.** Every relative link and path mention was rewritten by script.
    A link check over all Markdown files found no broken links, after fixing
    three that already pointed nowhere in an archived plan.
  - **Agent instructions.** `AGENTS.md` now points to
    `Docs/Technical/UnityProjectContext.md`.
  - **Capture paths.** The Wanderer, Group and Gatherer preview scripts now
    write into their image folders. They compile.
  - **Index.** `Docs/README.md` describes the layout.

## S1c, second pass: hand-painted surfaces — October 2, 2026

Authored in the isolated worktree. Blender 4.4 runs the painting; Unity
6000.6.0f1 runs everything else.

- **Painting.** `painting.py` baked 14 material textures (512–2048 px JPEGs,
  about 6 MB in total) through `house.py --paint` and `nature.py --paint`.
  The house takes about 6 minutes on GPU (OptiX), and nature about 4.
- **Iterations judged from renders:**
  - **Display transform.** The first bakes were saved through the AgX
    display transform, so colours washed out. They are now saved with
    Standard.
  - **Plaster.** It was too timid and too grey, so the daubs, warm/cool swings
    and worn patches are stronger.
  - **Roof.** The candy-coloured patchwork was toned down to board-by-board
    value.
  - **Daub edges.** The hard polygonal daub edges were softened (smooth F1
    with warped coordinates).
  - **Black leaf blotches.** They came from mipmaps sampling the black space
    between many small UV islands. The texture background is now filled with
    the material's colour, and round shapes unwrap at 75°.
  - **Wood.** Timber read spotty under lamplight. The wood daubs are weaker,
    and wood materials use `_Brush` 0.3.
- **Asset churn removed.** `TimeOfDay` now drives the sky and window glow
  through shader globals; no material asset changes with the hour.
  - **Colour conversion.** `Shader.SetGlobalColor` does not convert from sRGB,
    so the sky colours are sent as `.linear`.
  - **Pixel check.** Recaptured night, dusk and day skies match the earlier
    captures pixel for pixel.
- **Tests.** `OrdinaryPlaceTests` now checks the moon and glow globals (moon
  hidden by day and shown at night; windows fully lit at night, almost unlit
  by day). Together with `CameraTests` they passed 14/14.
- **Full PlayMode suite: 107/107 in two consecutive runs** (863.1 s and
  861.5 s).
- **Release build.** `OrdinaryPlaceSetup.BuildWindows` passed. Output:
  `Builds/WindowsOrdinaryPlace/WonderGather.exe`.
- **Benchmark.** At 1920×1080 on the RTX 4060 Laptop, GPU frame time is:

  | Candidate | GPU ms |
  |---|---|
  | A | 3.1–3.9 |
  | B | 3.3–4.3 |
  | C | 4.4–5.3 |
  | D | 3.4–4.4 |
  | E | 4.5–5.5 |

  The slowest 5% of frames are at most 6.3 ms of CPU. The painted textures
  add no measurable cost.
- **Not tested:**
  - interactive play in the new build or the open Editor;
  - GPUs other than this one;
  - painted textures on grass, ground and far land (those are not painted
    yet).

## S1e — the essence beyond the surface, first iteration (October 2) — not adopted

**Outcome.** Luis preferred the hand-painted pass ("the before"). The code
was archived on the branch `claude/essence-exploration` (commit 66f9ea2) and
is not on `claude/worker-showcase`. Record:
[TheEssencePlaytest.md](Playtests/TheEssencePlaytest.md),
[TheEssence.md](ArtDirection/TheEssence.md).

- **Environment.** The isolated worktree, never Luis's open project. Nothing
  from this iteration was copied into the main project's Assets.
- **Iterating on the far world.** These problems were found and fixed in
  captures:
  - **Mountains.** They first stood as a pale wall close around the meadow.
    They were rebuilt as layers: low ridges, far ranges and one peak.
  - **The lake.** It was invisible from ground level because of two things:
    - the eye cannot see water below a level meadow past its edge;
    - an old 3 m ring of hills at the meadow's edge sat above eye level.

    It was solved with a bluff, a valley lake 26 m below and no rim towards
    the water. A height probe confirmed the profile.
  - **The top-down map.** It showed no water at all. The cause was depth
    precision 7 km from the camera, not the water. Far water now ignores
    scene depth.
  - **Shores.** A flat lowland at water level read as a mudflat. The valley
    floor now stays above the water.
  - **The cloud shader.** It had a vector-constructor compile error, fixed.
  - **The painting pass.** It first cost about 5 ms. Sampling wide strokes
    every other pixel brought it to 0.5–1.4 ms over look E.
- **The meadow's ground and navigation are unchanged.** `BeyondTests` samples
  200 points: heights stay within ±3 m, and the baked navigation still lies on
  the ground.
- **Full PlayMode suite: 112/112** (107 earlier plus 5 new `BeyondTests`) in
  three runs:
  - 913.2 s, before the last small change (the O key);
  - 894.7 s and 888.9 s, on the archived code (two consecutive runs).
- **Release build.** It passed (`-buildOut WindowsTheEssence`).
- **Benchmark.** GPU ms at 1920×1080 on the RTX 4060 Laptop:

  | View | E | F |
  |---|---|---|
  | Overview | 7.4–7.5 | 7.7–7.8 |
  | Path | 6.5–6.8 | 7.5–7.8 |
  | Grass | 5.8–6.6 | 6.7–7.0 |
  | The lake below the meadow | 4.6–4.7 | 5.9–6.2 |
  | Strategy | 7.2–7.5 | 7.7–7.9 |

  The beyond added about 1.5–2 ms to E.
- **Colour against Luis's references.** The daylight captures moved:
  - **Green:** from about 100° to 83–87°, brightness 0.52–0.54;
  - **Sky and water:** to 194–195° and saturation 0.43–0.50;
  - **Median luminance:** to 0.58–0.65.

  Luis's verdict was that the before is better: the measurements matched the
  references' surface, not his taste.
- **Not tested:** interactive play of this build by hand, other GPUs, and
  scenes with many units.

## S1c, third pass — dusk details (October 2)

After Luis's answers (merge the hand-painted pass, bring back the fireflies,
deepen the dusk in small steps), these details were built over the
hand-painted look, each switchable:
- fireflies (key 7, on);
- the hearth's flicker (key 8, off until judged);
- window glow (key 9, off until judged).

[OrdinaryPlaceLookTest.md](Playtests/OrdinaryPlaceLookTest.md#third-pass-dusk-details-october-2)
has the details.

- **Environment.** The isolated worktree was reset to the merged base
  (fcaa654). The validated files were then copied into the main project.
- **Judged on Luis's own frame.** `-captureSet dusk` recreates his favourite
  frame: look E at 19:12, low on the path, 1600×670.
  - The first fireflies near the camera swelled into large blots, so near
    ones now fade and shrink.
  - Lowering them into the grass then hid them behind the blades, so their
    height was restored.
- **Tests.** `DuskDetailsTests` (4 new) check that:
  - fireflies appear only after sunset and are on by default;
  - the hearth moves the lights only when on, and never puts them out;
  - the window glow has four halos (three windows and the door) and is off by
    default;
  - each detail switches on its own.
- **Full PlayMode suite (111 tests):** three runs.
  - Run 1 passed 111/111 (875.5 s).
  - Run 2 failed 1 test (898.2 s):
    `EquippedWorkerTests.ProducedWorkerReceivesTheBlueprintPickaxeAndMinesThroughTheSameAction`
    ("The worker must retain a supporting foot outside a brief jogging flight
    phase").
  - Run 3 passed 111/111 (881.1 s).
- **Investigating that failure.** That test checks the S0 gait in the
  equipment scene, where none of the dusk code runs. In isolated runs:

  | Code | Isolated runs |
  |---|---|
  | The base (fcaa654) | 18 of 18 passed |
  | With the dusk details | 14 of 15 passed (the failure was in the first run; the last 10 all passed) |

  It is a rare, existing intermittent failure in the gait support, roughly 1
  in 15–30 runs. It is not caused by this pass. It is flagged for its own fix.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Benchmark.** Look E, GPU ms at 1920×1080 on the RTX 4060 Laptop, with
  fireflies on, the hearth and glow off:

  | | Day | Night |
  |---|---|---|
  | Look E | 4.9–5.6 | 5.7–5.8 |

  The night views carry the fireflies, about 0.3–0.5 ms more than the
  hand-painted pass's 4.5–5.5. The window glow is four quads and was not
  benchmarked separately.
- **Not tested:**
  - **The hearth's flicker.** It moves in time; it was checked by test, not
    watched by eye.
  - **Play by hand.** Interactive play in the build was not tried.
  - **Hardware.** Other GPUs were not tried.

## S1c, third pass — after Luis's test (October 2)

Luis tested the dusk details and asked for:
- all three on by default;
- fewer fireflies, with some sign of them even when very zoomed out;
- the lamplight close up matching the far look Luis liked.

[OrdinaryPlaceLookTest.md](Playtests/OrdinaryPlaceLookTest.md#after-luiss-test-october-2)
has the details.

- **Environment.** The isolated worktree was reset to 8b79de9. The validated
  files were then copied into the main project.
- **Finding the cause of the lamplight.** The scene was captured near, at
  middle distance and far at 23:00. Each was captured three ways: as it is,
  with bloom off, and with the grass at full density.
  - Bloom was not the cause.
  - Far away the eye sees mostly grass tips, which local light lit at
    `0.5 + 1.6·h` (twice the lower blade).
  - Close up, the darker blade bodies, and the soil under them (occluded by
    the grass's vertex colour, like the sun), hid the warm pool.
  - After the fix, the near capture shows the warm pool the far one does. The
    far capture is unchanged.
- **Fireflies far out.** Captured at three zooms at night: a sparse scatter
  of dots stays visible when zoomed out.
- **Tests.**
  - `DuskDetailsTests` now checks that the hearth and window glow are on by
    default.
  - **Full PlayMode suite (111 tests):** two runs, both passed 111/111
    (902.4 s and 877.5 s).
  - The gait test that failed once in the previous entry passed in both runs.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Benchmark.** GPU ms at 1920×1080 on the RTX 4060 Laptop, now with all
  three details on:

  | | Day | Night |
  |---|---|---|
  | Look E | 4.5–5.1 | 5.3–5.7 |

  This is slightly lower than the previous entry (4.9–5.6 by day, 5.7–5.8 at
  night, with fireflies on and the hearth and glow off). The fewer fireflies
  more than pay for the hearth and the glow.
- **Not tested:**
  - **Play by hand.** Interactive play in the build was not tried.
  - **Luis's frame and screenshot views.** Judged by capture only, not by
    Luis's eye.
  - **Hardware.** Other GPUs were not tried.

## S1d, worker concepts — first frames (October 2)

Luis asked to move to the worker's redesign "with heart and soul", from the
Visual Soul and its principles for people. Three concepts were made for Luis to
choose a direction
([WorkerConcepts.md](ArtDirection/WorkerConcepts.md)).

- **Environment.** The isolated worktree was reset to 38a51d2. The validated
  files were then copied into the main project.
- **Judged in the engine, not only in Blender.** Each pass was rendered in
  the Ordinary Place (look E) by `WorkerConceptCapture`, and looked at before
  the next. Fixes along the way:
  - **First frames.** They read as stiff clay dolls: tube arms, blob hands,
    tiny dot faces, symmetrical stances. Postures, structured heads, painted
    faces, shaped hands and hair locks were added.
  - **Orientation.** The figures came in facing away; a pivot turns them.
  - **Hair.** It covered the eyes; the hairlines were raised. Locks first
    stood up like spikes, then read as a hat; they now flow over the skull,
    and Long's cut is swept to one side.
  - **Faces vanished in wide frames.** The paint filter wiped out the marks,
    so the `_Drawn` switch now keeps beings clear of it.
  - **A dark block on Round's chest.** The skin modifier's branch at the
    collar turned faces inside-out. A voxel remesh of that mesh then dropped
    the torso entirely, which confirmed the cause. The torso and arms are now
    separate chains fused into one closed surface.
  - **Faces too white by day.** The skin is now a warmer, deeper tone.
- **Tests.** Full PlayMode suite (111 tests), with `_Drawn`, the paint filter's
  normals read and the outline in place: passed 111/111 (882.9 s).
- **The capture after its Editor safety steps.** It was re-run in batch and
  succeeded. The frames match the earlier ones, apart from the moving
  fireflies and grass.
- **Not tested:**
  - **Motion.** The concepts are posed, not rigged.
  - **Cost.** The outline's cost was not benchmarked; it is an extra draw of
    each being's mesh.
  - **The Editor menu path.** It was run only in batch mode; its scene-safety
    steps (asking to save, `DontSaveInEditor`, restoring the scene) were not
    exercised by hand.
  - **Luis's eye.** These are proposals; nothing is accepted.

## S1d, the miners — second pass (October 2)

After Luis's ranking (Small, Long, Round) and the request for much higher model
quality with miner's hints, the three were rebuilt from modules
([TheMiners.md](ArtDirection/TheMiners.md)).

- **Environment.** The isolated worktree was reset to 3066921. The validated
  files were then copied into the main project.
- **Judged pass by pass,** in Blender previews (full figures, faces and
  hands) and in the engine (the line-up at three hours, Strategy height,
  portraits, four-view turnarounds). Fixes along the way:
  - **Boots.** They came out as balloons (their width was a radius); now about
    half as wide.
  - **Long's pickaxe.**
    - **Over the shoulder,** it crossed Long's face. A hand can now be told the
      direction of the handle it grips.
    - **Now a walking stick.** The pickaxe is leant on like one, its length
      reaching the ground from the hand.
  - **A ledge at the waist** of coats and smocks. A skirted top now stops at
    the waist, and the skirt widens gently.
  - **Floating parts in the Blender three-quarter view** were a preview
    artifact: copies turned about their own origins. Every part's origin is
    now the being's origin, and the preview turns each copy about the
    character's.
  - **Painted textures about twice too light.** The emission bake saves sRGB,
    so bases now go in as linear colours, with the gap fill in sRGB.
  - **Blotches in the painting.** The house's brush scale is too broad for a
    person, so a `scale` key was added, and dust kept low.
  - **Blotches from the bake.** The three were baked overlapping on one spot,
    so each one's cavities were shaded by the others' bodies. They are now
    baked apart.
  - **The engine's own brush marks** at the world's 3 per metre blotched the
    cloth; beings now use 14.
  - **Dark blotches from a few metres away** came from the smock's outline
    showing through the apron lying just over it. Found by switching the
    outline, ink, paint and shadows off in turn. The hull is now pushed 3.5 cm
    away from the eye.
  - **The back of Long's head** showed bare when nodding. The hair caps were
    thickened outwards and lifted, and the back hairline lowered to the nape.
  - **Soot on faces** read as bruises from afar. Only a small smudge on
    Small's cheek, and a touch on Round's, remain.
- **Compilation and capture.** Every engine pass ran
  `MinerCapture.Capture` in batch, and all succeeded.
- **Not tested:**
  - **The PlayMode suite** was not re-run. No runtime code changed: only an
    Editor capture, the outline shader (used by the miners alone), the
    painting script and assets. The last full run, 111/111, was on 3066921.
  - **The Editor menu path** was not exercised by hand.
  - **Motion.** The miners are posed, not rigged.
  - **Cost.** About 95,000–105,000 faces per miner, plus an outline draw;
    this was not benchmarked. They are hero models, not yet RTS units.
  - **Luis's eye.** Nothing here is accepted until Luis has seen it.

## S1d, the miners — polish (October 3)

After Luis's notes (necks, feet, Round's hands) and the request for a careful
look at every distance
([TheMiners.md](ArtDirection/TheMiners.md#polish-after-luiss-word-october-3)).

- **Environment.** The isolated worktree was reset to 2bd6613. The Blender
  sources are edited in the main project; the exported assets are validated
  in the worktree and then copied in.
- **How it was judged:**
  - **Blender close-ups** of each neck from the front and side, each hand on
    its own, and the boots.
  - **Four-side full figures and three-angle portraits** of each miner.
  - **The engine captures** (line-ups at three hours, Strategy height,
    portraits, turnarounds), compared before and after.
- **Fixes found along the way** (beyond Luis's three notes):
  - **Round's rim.** Round's smock skirt stood out in a hard rim at the waist.
    The cause: voxel remesh and smoothing make a fused garment 1–2 cm smaller
    than its measurements. The skirt now measures the top's real surface by
    ray casts.
  - **The apron** showed the fuller skirt through it. It now hangs outside
    the measured top and skirt at every height.
  - **Long's hip bumps** were the trouser thighs (wider than the hips) showing
    through the coat.
    - **A first fix** (clearing all round) turned the coat into a bell. The
      clearance is now all round at the seat and at the sides down the
      thighs.
    - **A sign error** pushed the thighs outward on the first try.
  - **Round's hands.**
    - **On the first try,** they hung beside the hips: their place used the
      measurements, not the real surface.
    - **Now** they rest on the measured waist.
  - **Round's curls.** Stray curl springs poked out beside the face like
    horns; they now spring only from the back.
  - **Faces at a distance** faded as the face texture's mipmaps averaged the
    thin strokes into the skin. A mipmap bias of −1.2 on faces fixes it.
  - **Small's lantern light** burnt the hand above it white.
- **A layout mistake in 2bd6613, fixed here.** When the first concepts were
  removed, the empty `Art/Worker` folder went with them. The miners' assets
  were then copied in as `Art/Worker/*` instead of `Art/Worker/Miners/*`, so
  `MinerCapture` would not have found them in the main project. They are now
  in `Art/Worker/Miners/`, and the stray copies are removed. The asset IDs
  never collided.
- **Compilation and capture.** `MinerCapture.Capture` ran in batch and
  succeeded.
- **Not tested:**
  - **The PlayMode suite.** No runtime code changed (Blender sources, an
    Editor capture and assets only).
  - **Motion.** The miners are still posed.
  - **Luis's eye.**

