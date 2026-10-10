# Validation history

This page is a history, oldest first: each section is the evidence of its
day, kept as it was written. **The latest is at the end.** What is current
is in [CURRENT_STATE.md](CURRENT_STATE.md); the milestone in hand (October
2026) is S3b, the body's own ([the plan](NextMilestonePlan.md)).

(Until October 10 this page opened by calling the Living Worker of
September 26 the current milestone. An outside review pointed out that it
was stale; Luis said to put it right.)

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
uncommitted files were not touched.

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
  references' surface, not Luis's taste.
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
- **Judged on Luis's own frame.** `-captureSet dusk` recreates Luis's favourite
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

## S1d, the miners — rigged, light, and the choice (October 3)

Luis asked to start rigging, make the miners light enough for the RTS, and
offer all three in this prototype
([MinersPlaytest.md](Playtests/MinersPlaytest.md)).

- **Environment.** The isolated worktree was reset to 9930e35. The validated
  files were then copied into the main project.
- **The export, checked in Blender before the engine.** Each rigged FBX was
  re-imported, posed mid-stride bone by bone and rendered at each level of
  detail:
  - coats and the apron followed the thighs;
  - boots followed the feet;
  - arms swung.

  Fixed along the way:
  - **One white material.** Clearing a mesh's material slots reset every
    face's material index, so all faces got the lamp glass. The faces are now
    read first and assigned after.
  - **Small's lantern rode on the head.** Its own cap matched the rule for
    caps; things carried at the belt are now checked first.
  - **The outline drew the lamp glass instead of the body.** Unity orders a
    mesh's parts by first use, and an extra material draws the last part. The
    glass's faces are now sorted first.
  - **The rest pose's wide arms** (kept for clean skinning) made a scarecrow
    walk in the game. The game's hang is now computed per miner: close to the
    body, the elbow just clear of the trunk.
- **Tests** (`MinerTests`, 5 new, all passed on their first run):
  - three miners are offered and one stands;
  - choosing another puts it where the last stood, with the selection;
  - each is within budget: three levels of at most 20,000, 6,000 and 2,000
    triangles, 3 materials at most, 24 bones at most;
  - each walks to the door, its modelled ankles staying within 9 cm of the
    body's planted feet, step by step;
  - each stands upright.
- **Full PlayMode suite (117 tests, the capture skipped as explicit):** two
  runs, both 116/116 passed (927.9 s and 926.2 s). The proportions' defaults
  reproduce the original constants exactly; the earlier tests did not
  change.
- **Watched by eye.** `MinerWalkCapture` renders each miner walking, by day
  and at dusk, from the side, the front and the Strategy height, and the
  three together.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Crowd benchmark** (`-wgcrowd`). GPU and frame time at 1920×1080 on the
  RTX 4060 Laptop:

  | Miners walking | Strategy view, frame ms (p95) | Close view, frame ms (p95) |
  |---|---|---|
  | 0 | 4.9 (5.9) | 4.7 (5.1) |
  | 25 | 5.7 (7.1) | 5.2 (5.4) |
  | 50 | 6.0 (8.4) | 5.6 (5.9) |
  | 100 | 7.0 (9.2) | 6.2 (7.3) |

- **Not tested:**
  - **Mining and hauling with the miners.** The equipment scene still uses
    the 2.2 m test body.
  - **Turning on the spot and jogging,** by eye.
  - **The Editor menus** (`Create The Miners`, `Add The Miners To The
    Ordinary Place`). They ran only in batch.
  - **The picker by hand.** It was driven by tests only.
  - **Other hardware.** Other GPUs were not tried.
  - **Luis's eye.**

## S1d, the miners — polish after Luis's play, natural walks (October 3, evening)

Luis's notes on Small, the request for proactive polish on all three and
natural walks
([correspondence](Correspondence/2026-10-03_SMALL_POLISH_AND_NATURAL_GAITS.md);
[TheMiners.md](ArtDirection/TheMiners.md#polish-after-luiss-play-october-3-evening);
the practices in [CharacterPractices.md](ArtDirection/CharacterPractices.md)).

- **Environment.** The isolated worktree was at c733d4f. The validated files
  were then copied into the main project.
- **Research first.** Good practice for game characters was researched and
  recorded, with sources, in CharacterPractices.md:
  - remove hidden geometry;
  - give clothes the weights of what they lie on;
  - attach straps and carried things;
  - neck anatomy;
  - walk character;
  - walking speed by Froude number.
- **The close audit, before fixing anything.** A new capture
  (`MinerCloseCapture`) renders each rigged miner in the game at dusk, as
  close as the Explore camera goes:
  - from every side, standing;
  - the face, the neck from behind and from the front, hands, feet;
  - mid-stride.

  Findings beyond Luis's notes:
  - **Small's body walked away from the unit.** After choosing a miner, its
    feet were still where that miner was last shown. This is a real bug, also
    in the build Luis played.
  - **A hole at the back of every neck.**
  - **Long's chest strap floated** where the coat opens.
  - **Round's apron stood off the smock.**
  - **Buttons stood off the cloth.**
  - **Long's boot shafts were wide** around thin legs.
- **Fixed, and checked by audit again** (Blender previews of the rest poses,
  then the engine, three rounds):
  - Small's lantern, satchel and strap, legs and boots, neck and collar;
  - the swap's body (`ResetPose`);
  - necklines and collars on all three;
  - Long's strap, pickaxe and mug;
  - Round's apron and straps;
  - buttons;
  - trousers hidden under skirts;
  - layered weights;
  - ring sizes;
  - lamps lighting the ground.

  Before and after: `Images/Miners/Polish3_BeforeAfter.jpg`.
- **Natural walks:**
  - Small: 1.27 m/s, brisk and bouncy;
  - Long: 1.38 m/s, long smooth strides;
  - Round: 1.28 m/s, rolling.

  Each pace comes from the body's own legs at its walk's Froude number.
- **Tests.**
  - `MinerTests` now also checks that a newly chosen miner's feet are under
    it.
  - **Full PlayMode suite (118 tests, the two review captures skipped as
    explicit):** two runs, both 116/116 passed (914.0 s and 909.7 s).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Crowd benchmark** (`-wgcrowd`, 1920×1080, RTX 4060 Laptop). The cost
  is unchanged by the polish:

  | Miners walking | Strategy view, frame ms (p95) | Close view, frame ms (p95) |
  |---|---|---|
  | 0 | 5.2 (5.3) | 5.1 (5.5) |
  | 25 | 5.6 (5.9) | 5.5 (7.0) |
  | 50 | 5.9 (6.7) | 5.9 (6.9) |
  | 100 | 6.9 (8.5) | 6.4 (7.9) |
- **Not tested:**
  - **The lantern's swing** was judged in frames, not watched moving by eye.
  - **Jogging and turning on the spot,** by eye.
  - **Mining with the miners.** It still uses the test body.
  - **Other hardware.** Other GPUs were not tried.
  - **Luis's eye.** Luis's notes on Long and Round are still to come.

## S1d, the miners — the model quality method's first round (October 3 and 4)

Luis's five notes on Small, the principle that every object is a proper
object, and the request for a method
([correspondence](Correspondence/2026-10-03_PHYSICAL_OBJECTS_AND_A_QUALITY_METHOD.md);
[ModelQualityMethod.md](ArtDirection/ModelQualityMethod.md);
the round's log in
[TheMiners.md](ArtDirection/TheMiners.md#the-methods-first-round-october-3-and-4)).

- **Environment.** The isolated worktree was at e5372c4. The validated files
  were then copied into the main project. Luis's own uncommitted files were
  not touched.
- **The method first.** It was written and pushed (d1dbe00) before any fix,
  as Luis asked, with its research and sources.
- **Tools built for it:**
  - `audit.py`: the automatic audit, at rest and on recorded frames;
  - `MinerPoseRecord`: the game records its own movement (235 frames a
    miner: standing, starting, walking, a sharp turn, stopping);
  - `MinerCloseCapture`: the capture matrix (about 240 frames a miner);
  - `Art/Review/sheets.py`: 92 contact sheets a round.
- **Rounds.** Eighteen rounds of the whole pipeline: build the three models,
  set them up in the game, record their movement and run the miner tests,
  audit in motion, capture, make the sheets.
- **The audit.**
  - **On the build Luis played:** 45 failing checks on Small, 41 on Long,
    42 on Round. Some were the audit's own mistakes, read and corrected
    (TheMiners.md lists them).
  - **At the end, at rest:** 0 of 386, 0 of 268, 0 of 202.
  - **At the end, on the recorded movement:** 4 of 856 remain, each with its
    reason:

    | Model | Check | Worst | Frames | Reason it is left |
    |---|---|---|---|---|
    | Small | Trousers under the skirt | 29.2 mm | 1 (the sharpest turn) | The knee comes out under the lifted hem, not through the cloth; the walk's high step |
    | Small | Boot into its trouser leg | 8.6 mm | 1 (the same step) | The same high step |
    | Long | Free hand into the coat | 7.3 mm | 2 (the sharpest turn) | The knee lifts the coat into a hand swinging forward |
    | Round | Hammer's handle into the apron | 3.9 mm (limit 3.5) | 6 of 118 (walking) | The leg lifts the apron against the handle |

    The reports are in `Art/Review/Miners`.
- **Findings.** Luis's five on Small, and eighteen more found by the method
  on all three. Each has its cause and fix in TheMiners.md. Two of the
  eighteen were made by this round's own fixes (Long's coat became a bell;
  Round's bib became a narrow strip) and were caught by reading the sheets
  against those of the build Luis played.
- **The exhaustive look.** The last round's sheets were read: every junction
  zone, the carried things, the orbit and the main walk views at full size;
  the remaining walk, motion, distance and fresh-eyes sheets at half size.
  Before and after pairs: `Images/Miners/Method1_BeforeAfter_Small.jpg` and
  `Method1_BeforeAfter_LongRound.jpg`.
- **Tests.**
  - **Miner tests:** 8 of 8 passed, in each of the last seven rounds. Two are
    new:
    - what a miner carries hangs from its hand, stays out of the body, keeps
      its handle in the closed fingers, and hangs straight when carried
      steadily;
    - in a sharp turn the boots never overlap.
  - **Full PlayMode suite (121 tests, 3 skipped as explicit):** two
    runs on the final state, both 118/118 passed
    (935.9 s and 932.1 s).
- **Failures on the way, and what they were:**
  - **One earlier suite run failed one test**
    (`LivingWorkerTests.SharedLimitedPositionsQueueAndDrainAResourceWithoutLosingSupplies`:
    15 of 17 supplies delivered in its 90 seconds). Blender was rendering
    previews on the same machine during that run. Alone, the seven living
    worker tests passed, and so did both final runs, made with nothing else
    running. The test is sensitive to the machine's load; the code it tests
    was not changed.
  - **The test of carried things failed in turn for four reasons,** each a
    real finding: the lantern trailing; the wrist's give fed back to the
    pendulum (it leaned 16° backward standing still); gravity lost at about
    3,800 frames a second, the rate these tests run at; and the test itself
    reading the bones after the miner had walked on.
  - **The turn test failed** while boots were kept apart as points; it
    passes with boots measured from heel to toe.
  - **A setup step failed once with "compiler errors"** after a validation
    run had been stopped part-way: the editor's compiler helper did not
    start. It was not a code error; the next launch compiled and ran.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Crowd benchmark** (`-wgcrowd`, 1920×1080, RTX 4060 Laptop), with the
  hanging things and the skirts' flaps:

  | Miners walking | Strategy view, frame ms (p95) | Close view, frame ms (p95) |
  |---|---|---|
  | 0 | 4.8 (5.2) | 4.7 (5.1) |
  | 25 | 5.5 (5.8) | 5.2 (5.4) |
  | 50 | 5.9 (6.7) | 5.5 (6.0) |
  | 100 | 6.9 (7.8) | 6.4 (7.1) |

  A hundred miners add about 2.1 ms.
- **Not tested:**
  - **Extreme poses** (a deep knee bend, raised arms): the sweep is not built
    yet. The models were checked on the game's own movement only.
  - **Moving, by eye.** Everything in motion was judged on frames and
    measurements, not watched.
  - **Jogging,** and turning on the spot.
  - **Ordinary frame rates in the tests.** The tests run at thousands of
    frames a second and the recordings at 30; 60 to 144 was not measured
    directly.
  - **Mining with the miners.** It still uses the test body.
  - **Other hardware.**
  - **Luis's eye.** Passing checks are not acceptance of the look or the
    feel.

## S1d, the miners — after Luis's look: the shoulder strap, and Round's neckband (October 5)

Luis liked the method's first round except the strap on Small's shoulder,
and said to go on with the plan
([correspondence](Correspondence/2026-10-05_THE_SHOULDER_STRAP.md);
[TheMiners.md](ArtDirection/TheMiners.md#after-luiss-look-october-5)).

- **Environment.** The isolated worktree was at dab5ed0. The validated files
  were then copied into the main project. Luis's own uncommitted files were
  not touched.
- **The strap.**
  - **First try:** kept taut, but moved out on the shoulder and under the
    collar. Read beside the build Luis preferred, it still was not that
    strap: it had a pinch at the shoulder and dropped straight down the back.
    The audit also found it 3.1 mm inside the collar (limit 3.0).
  - **Second try, kept:** the strap's course and breadth exactly as in the
    build Luis preferred (laid soft on the coat, not drawn tight), with its
    ends on the bag's rings. Read beside that build in five views
    (`Images/Miners/Method2_Strap.jpg`): the same strap.
  - **A small failure on the way:** the wider strap left its folded ends
    1.6 mm off it in the sharpest turn (limit 1.5). The ends were set 0.6 mm
    closer.
- **Round's neckband.** The flaw left open after the first round was read
  closely. It was not a seam, as first thought: making the band seamless
  changed nothing. A probe of which surface is uppermost round the neck
  showed the band sitting below the join of skin and cloth at the back, where
  the smock falls away steeply. The band now sits on that join at the back
  and the sides; the front is unchanged
  (`Images/Miners/Method2_Neckband.jpg`). A first version also moved the band
  at the front, by up to 43 mm; that was taken back, since nobody asked for
  it.
- **The audit** (five rounds of the whole pipeline):
  - **At rest:** 0 of 386 (Small), 0 of 268 (Long), 0 of 203 (Round, one new
    check: a neckband sits on its garment's neckline all the way round).
  - **On the recorded movement:** the same four checks as after the first
    round remain, with the same values and reasons. The strap passes all of
    its checks: it meets both rings, and does not sink into the coat or the
    collar.
- **Tests.**
  - **Miner tests:** 8 of 8 passed in every round.
  - **Full PlayMode suite (121 tests, 3 skipped as explicit):** one run on
    the strap's final state, 118/118 passed (947.3 s). It was not run again
    after the neckband: that change is to Round's model only, and no code
    changed.
- **Release build.** It passed, on the final state
  (`Builds/WindowsOrdinaryPlace`).
- **Not tested:**
  - **The crowd benchmark** was not run again: no code changed, and the
    models' weights are as they were.
  - **Moving, by eye.** The strap and the neckband were judged on frames.
  - **Luis's eye.** The strap is put back from the pictures of the build Luis
    preferred; Luis has not yet seen it in the game.

## S1d, the miners at work — step 1: the sweep of extreme poses (October 5)

The first step of the plan Luis said to go on with
([NextMilestonePlan.md](Plans/S1_OrdinaryPlaceCamerasAndMiners.md#s1d-last-part--the-miners-at-work-plan-waiting-for-luiss-choices)).
Findings are in
[TheMiners.md](ArtDirection/TheMiners.md#the-sweep-of-extreme-poses-october-5).

- **What was run.** The sweep (`workers.py --sweep`) on the three miners as
  committed in e7a2a67: 27 poses each, 81 pictures a miner, read on 15
  contact sheets. No model and no game code changed.
- **The sweep's first run was wrong in three ways,** found on its own
  pictures and reports before anything else was read: the trunk and head
  bent backward for "forward"; held things stayed behind a raised arm; a leg
  under a lifted hem counted as through the cloth. All three were corrected
  and the sweep run again.
- **Results of the second run** (failing checks of all):

  | Miner | Failing | Of |
  |---|---|---|
  | Small | 7 | 206 |
  | Long | 9 | 86 |
  | Round | 14 | 73 |

  Every failure is in a pose no movement of the game reaches yet: arms
  overhead, a knee lifted to 90°, knees bent 90°, a deep bend, a bow of 40°,
  and one side bend with the arm not carried as the game carries it. In the
  ranges the game reaches (measured on its recorded movement: shoulders 35°
  from rest, elbows 28°, a thigh 52°, a knee about 50°, the spine 5°) nothing
  fails. Today's swing keeps both hands within 32 cm of a point in front of
  the hips (the grips are 32 cm apart and the tool turns about the lower
  one), so it does not raise the arms to the shoulder. Nothing was changed on
  the models.
- **The audit's corrected check, on the recorded movement** (the recording of
  e7a2a67): the same four items remain. One reads differently: the knee in
  Small's sharpest turn is 11.3 mm past the hem, not 29.2 mm, now that a leg
  under a lifted hem is not counted as through the cloth. Reports:
  `Art/Review/Miners/audit_<Name>.txt` and `sweep_<Name>.txt`.
- **Not tested:**
  - **No Unity run.** Nothing in the game changed; the miner tests and the
    suite were last run on the strap's state (above).
  - **The sweep's poses are not the game's.** They are single joints turned
    in the rest pose's axes, not solved movement. The mining itself will be
    recorded and audited when it is built.
  - **Luis's eye.**

## S1d, the miners at work — step 2: hands that close (October 5)

[TheMiners.md](ArtDirection/TheMiners.md#hands-that-close-october-5).

- **Environment.** The isolated worktree at 1bb78a6 with this step's changes
  (committed as a1b1ed0 before the last runs).
  The validated files were then copied into the main project. Luis's own
  uncommitted files were not touched.
- **The closing, checked on the skin** (`workers.py --hands`, and in every
  build): 144 checks (four hands, six handles, five digits and the palm),
  none failing. The farthest digit is 0.06 mm off what it closes on; the
  deepest is 0.3 mm inside a handle (limit 0.5).
- **On the way there** (each read on the pictures before the numbers were
  believed):
  - a dark fold on the back of one hand: 622 triangles; fixed by a budget
    of 1,600 (first 1,800, which left Round 18 triangles under the test's
    limit);
  - fingers bent backwards, and one left pointing, in solves that reported
    "touching": a curl axis taken from each bone, and a finger's root taken
    as its grip. Both corrected;
  - the thumb touching nothing on some handles: corrected.
- **The audit of the three miners** (four rounds of the whole pipeline):
  - **At rest:** 0 of 386 (Small), 0 of 267 (Long), 0 of 203 (Round).
  - **On the recorded movement:** the same four checks as before remain.
    One value moved: Long's free hand into the coat in the sharpest turn
    reads 8.4 mm, not 7.3 mm (the hand has more points now; the same two
    frames).
- **Tests.**
  - **Miner tests:** 9 of 9 passed, one of them new (the closing hands).
    That test first failed on Long by 4.5 mm: the game's turns were made in
    the prefab's axes, and Long's model sits 11° off them. The closing is
    now fitted to the hand's own bones.
  - **Full PlayMode suite (122 tests, 3 skipped as explicit), twice:**
    119 of 119 passed (973.4 s); 119 of 119 passed (974.4 s).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Crowd benchmark** (1920×1080, RTX 4060 Laptop, by day; frame ms, the
  95th percentile in brackets). Measured on October 6 with the computer in
  use and the game's window in the background, so every figure is higher
  than October 4's (no miners: 5.5 ms now, 4.8 ms then) and the two days
  are not comparable. So the build without the finger bones (1bb78a6) and
  the build with them were each built and measured twice, one after the
  other; each figure is the mean of its two runs:

  | Miners walking | Without finger bones: strategy view | close view | With finger bones: strategy view | close view |
  |---|---|---|---|---|
  | 0 | 5.46 (6.64) | 5.22 (6.04) | 5.46 (6.79) | 5.19 (5.95) |
  | 25 | 6.30 (8.81) | 5.85 (8.07) | 6.32 (8.98) | 5.83 (7.93) |
  | 50 | 6.76 (9.34) | 6.64 (9.04) | 7.00 (9.51) | 6.61 (8.98) |
  | 100 | 7.97 (10.00) | 7.70 (9.40) | 8.16 (10.00) | 7.72 (9.51) |

  A hundred miners add 2.39 to 2.60 ms without the finger bones and 2.51 to
  2.79 ms with them (four readings each): about 0.1 ms more, which is no
  more than two runs of the same build differ by.
- **The benchmark first hung** for seventeen minutes: the game pauses when
  its window is not in front, and the computer was in use. The benchmark
  now keeps running in the background (`Application.runInBackground`, set
  only for `-wgcrowd`). That one line was added after the suite's two runs;
  the build and the benchmark were made with it.
- **The validation was cut off once** by a shutdown, in the second suite
  run. The first run's result stands; the second run, the build and the
  benchmark were run again the next morning
  ([correspondence](Correspondence/2026-10-05_SHUTDOWN_AND_RESUME.md)).
- **The look at rest.** The free hands' captures in the game were read
  beside the last round's: the same shape and pose
  (`Images/Miners/Hands_AtRest_Game.jpg`).
- **Not tested:**
  - **Closing in play.** Nothing in the game closes a hand yet; it was
    judged in Blender's pictures and by the test.
  - **A real handle.** The handles are ideal round bars. The pickaxe comes
    in the next step.
  - **Other levels of detail closed.** The closing was measured on the
    nearest level's skin only.
  - **Luis's eye.**

## S1d, the miners at work — steps 3 and 4: a pickaxe for each, and the tool's solve (October 6)

[TheMiners.md](ArtDirection/TheMiners.md#a-pickaxe-for-each-and-the-first-swings-october-6).

- **Environment.** The isolated worktree at 3ee044b with these steps'
  changes. The validated files were then copied into the main project.
  Luis's own uncommitted files were not touched.
- **The tools** (`tools.py`): three pickaxes, none failing its checks (a
  handle a hand can close on, and round). 517, 561 and 718 mm long; 714
  triangles (214 far away).
- **The hands' closing:** 0 failing of 48 (Small), 42 (Long) and 96 (Round)
  checks, with the rows for each one's own handle.
- **The audit** (`Art/Review/Miners/audit_<Name>.txt`):
  - **At rest:** 0 of 386 (Small), 0 of 267 (Long), 0 of 203 (Round).
  - **On the recorded movement** (291 frames each, 56 of them mining with
    the pickaxe in the hands): the same four checks as before remain from
    the walk. In the mining, two small ones: Small's bag strap end 3.9 mm
    into the right sleeve (6 frames of 28), Round's apron strap 2.4 mm into
    the smock (limit 2.0; 2 frames), and the cloth of a sleeve pulled just
    past its limit (below). The tool passes through nothing; the
    fingers enter the handle by at most 0.3 mm; every holding hand touches
    it on every frame. At the elbows the sleeves thin by up to 29%, at Round's left shoulder (the arm that reaches across) by 30%, and at the wrists by up to 22%, against a limit of 45%; the cloth is pulled to 2.0 times its length at that shoulder and 1.9 on Small's right sleeve, against a limit of 1.9.
- **How it got there** (a capture, then three recordings, each audited and
  read; failing checks for Small, Long and Round, the walk's four among
  them):
  1. the first body's swing, captured: the pickaxe through chest and head
     (seen on the first picture; no test caught it);
  2. over the shoulder, recorded: 13, 7 and 9 (the pick's head 43 mm into
     Round's face, handles in coats, a lantern in a coat);
  3. with the body's shape measured and a carrying hand left alone: 4, 1
     and 2;
  4. with rows for each one's own handle: 3, 1 and 2. With the sleeves and
     the skin at the joints measured too (the last reports): 4, 1 and 3.
- **Tests.**
  - **Miner tests:** 9 of 9. **Tool tests** (new): 3 of 3. In the swing
    test, with a hand closed on it the handle was 0.0 mm and 0.0° from
    where the hand holds it, over 2,246 to 4,524 frames a miner.
  - **Full PlayMode suite (125 tests, 3 skipped as explicit), three runs:**
    1. 122 of 122 passed (1,009 s).
    2. 119 of 122: three failures, all of one kind (a foot without support
       on the first body, twice; Round's ankle 0.44 m from its planted foot
       in the walk to the door). The computer was in use and stalling: the
       run took 63 s longer over the same tests, and the benchmark that
       followed has a row with a mean of 114 ms beside a 95th percentile of
       6 ms. A stalled frame moves a body further than a foot can follow.
    3. 122 of 122 passed (run after the three had passed alone, twice).

    The first body's equipment tests are among them and unchanged.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Crowd benchmark** (1920×1080, by day, the computer in use and the
  window in the background as on the hands' run; one run): two runs, both disturbed (several rows have a mean above their 95th percentile, which a stalled frame does). The undisturbed rows are in line with the hands' run: a hundred miners in the close view, 7.2 and 7.3 ms (7.7 then); fifty, 6.2 and 6.2 (6.6 then). No
  miner in it holds a tool.
- **Not tested:**
  - **Watched in play.** The swing was judged on captured frames, the
    audit and the tests. `K` shows it; nobody has watched it yet.
  - **Arriving at a rock.** In every test and capture the miner starts at
    its place (a small body stops too far from it; step 5).
  - **Hauling, delivering, putting the tool away.** Not built for the
    miners.
  - **Many miners mining at once,** and its cost.
  - **Other hardware.**
  - **Luis's eye.**

## A clip of the mining as it is, for the review (October 6)

Luis could not find how to watch the mining and asked for a review of the
whole project. A moving picture of the present swing was made for it.

- **Added:** a `clip` set in `MinerCloseCapture` (only when asked for:
  `-captureSets clip`). After the first strike it renders every frame of
  four seconds of each miner's mining from the side and from before it, at
  25 frames a second. `Art/Review/clip.py` puts one swing of the three
  together as a looping picture and as a strip.
- **Run:** in the isolated copy, once: 1 of 1 passed; 600 frames.
- **Measured on those frames:** the swing's progress is the same number on
  every frame for Small, Long and Round. One swing takes 20 frames (0.8 s),
  then 5 frames of pause, whatever the body and whatever the pickaxe (517,
  718 and 561 mm). The lift takes about a third of a second.
- **Seen in the pictures:** only the arm and the tool move. The legs, the
  hips and the back stay as they stand.
- **Not run:** the other tests and the build. Nothing in the game changed;
  the capture set is new and off unless asked for.

Pictures: `Docs/Images/Miners/Work_Clip_Side.gif`, `Work_Clip_Front.gif`,
`Work_Clip_Strip.jpg`. The finding is used in
[Reviews/2026-10-06_SamePageReview.md](Reviews/2026-10-06_SamePageReview.md#what-the-swing-is-today).

## S3, steps 1 and 2 — everything weighed, and the bench (October 6)

Stage S3, approved by Luis that day ([plan](Plans/S3_WeightAndStrengthAtTheRock.md);
[design and findings](Design/ThePhysicalBody.md#9-what-is-built)).

- **Built:** the weights of every miner and pickaxe, measured on the
  models; a pickaxe as a real body in a miner's hands, moved only as hard
  as the arms' joints can; a bench that tries it at several strengths and
  weights.
- **The weights** (`workers.py --weigh`, `tools.py --weigh`): Small 40.7 kg,
  Long 58.5 kg, Round 87.6 kg; pickaxes 1.50, 3.21 and 2.39 kg. Reports in
  `Art/Review/Miners/weights_<Name>.txt` and `tool_<Name>.txt`.
  - **Checked by hand:** Small's trunk, head and arm were worked out
    beforehand from its measures (about 13 kg, 10 kg and 1.9 kg); the build
    gives 14.8, 10.0 and 1.9. The pickaxes' moments agree with a head on a
    handle.
  - **Fixed on the way:** a pick's collar was weighed as a solid drum of
    iron (677 g on Long's); it is a ring round the handle (296 g).
- **In the game** (`MinerSetup.CreateAll`, in the isolated copy): each
  prefab has its weights; how each model sits in its prefab came out as the
  hands' fit had found it (Long 11.25, Round 2.44, Small 0.56 degrees).
- **The bench** (`PhysicalBench`, Round, frame for step at 50 a second;
  two swings each, alike in every case):

  | Strength | Pickaxe 1.19 kg | 2.39 kg (its own) | 4.78 kg |
  |---|---|---|---|
  | 0.5 | raised in 0.60 s at 55%; 6.2 m/s, 23 J | 0.62 s, 83%; 4.1 m/s, 21 J | 1.36 s, 97%, 0.71 m; 2.2 m/s, 11 J |
  | 1 | 0.60 s, 27%; 8.4 m/s, 42 J | 0.60 s, 42%; 6.5 m/s, 51 J | 0.60 s, 73%; 4.5 m/s, 48 J |
  | 2 | 0.60 s, 14%; 9.6 m/s, 55 J | 0.60 s, 21%; 8.3 m/s, 83 J | 0.60 s, 36%; 6.5 m/s, 101 J |

  All but the weakest with the heaviest raise the head 0.84 to 0.88 m.
  - **The hands** stay within 0 to 19 mm of where they hold (a step or two
    in the fastest part of the blow); 29 mm at most in the tests.
  - **The pickaxe** leans at most 36 degrees aside in the hands (14 are
    meant), but for the weakest with the heaviest, which lets it fall back
    at the top (86).
- **Frame rate.** The ordinary swing, running free and at 25 and 50 frames
  a second: raised at 42% of the arms; the blow at 6.6, 6.6 and 6.5 m/s.
  (Before the fix B5 the same lift took 72% at 25 frames a second and 37%
  at 50.) At 100 frames a second it was run only before the last two
  changes, and agreed then.
- **Cost.** One body's hands: 26 millionths of a second a physics step, on
  average over about 350 steps, in the editor (44 in the first run of a
  session). The engine's own share for the object is not in it.
- **Tests.**
  - **`PhysicalBodyTests`** (new, three): 3 of 3. Its figures: ordinary,
    raised 0.88 m in 0.60 s at 42%, struck at 6.6 m/s; heavy, 75% and
    4.2 m/s; strong, 21% and 8.3 m/s; feeble, 0.72 m in 1.42 s at 96%,
    1.4 m/s.
  - **Miner tests and tool tests:** all passed with them (14 in that run);
    the old swing still holds its handle to 0.0 mm.
  - **Full PlayMode suite, one run:** 129 tests; 125 passed, none failed,
    4 skipped as explicit (1,061 s).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`). Nothing in
  it looks different: the bench is a test.
- **What failed on the way, and why:** the eight findings B1 to B8 in
  [the design](Design/ThePhysicalBody.md#step-2-the-bench-october-6). Two
  were found only in pictures (the pickaxe swinging the wrong way round;
  the pickaxe a frame ahead of the hands, which was the pictures' own
  fault) and two only in a step-by-step trace (the back and the arms out
  of time; the plan pushing into the block).
- **Not tested:**
  - **A second run of the suite,** and the crowd benchmark.
  - **Small and Long,** and any one-handed hold: only Round has two free
    hands.
  - **The cost in a release build,** and with many at work at once.
  - **The other way of building the arms** (the engine's jointed body):
    not built.
  - **The game's own mining:** it still uses the old swing.
  - **Other hardware.**
  - **Luis's eye.**

## S3, step 3 — both hands free, the lantern and the mug at the hip (October 6)

- **Built:** Small's and Long's left hands as closing hands; the lantern on
  a hook and the mug in a loop at the hip
  ([TheMiners.md](ArtDirection/TheMiners.md#both-hands-free-the-lantern-and-the-mug-at-the-hip-october-6)).
- **Two full cycles** (build, setup, record, test, audit, capture, sheets).
  - **At rest:** Small 0 failing of 396 checks, Long 0 of 277, Round 0 of
    203. The hands' closing: 0 of 96, 0 of 84, 0 of 96.
  - **In the game's movement:** Small 9 of 465, Long 1 of 333, Round 3 of
    257. Of Small's nine: five are the moment of the sharpest turn that
    waits for Luis's word (one of them new: the knee meets the lantern),
    three are the old swing (being replaced), and one is new: the free left
    hand brushes the coat for two frames of a turn, as Long's right does.
  - **First cycle:** Small 13, Long 4. The tab stood off the coat, and the
    lantern and the mug pressed about 5 mm into the skirt while walking.
    The tab now lies on the cloth's slope. The 5 mm is allowed, as the
    bag's 6 mm is: a thing resting on a coat presses into it a little.
- **Tests:** miner tests, tool tests and the physical tests, 14 of 14
  (one expected count changed: six hands close, not four).
- **Read in pictures:** the orbit, the lantern's and the mug's zones, the
  walk's eight phases, before and now side by side.
- **Not tested:**
  - **The full suite and the build** with these models: they run at the
    end of step 4.
  - **The physical bench with Small and Long:** next, in step 4.
  - **Luis's eye** on where the things hang.

## S3, step 4 — the swing (October 6)

Built on the bench: the upper hand that slides, the back's own strength,
the hips going back, and a swing aimed at a point
([design](Design/ThePhysicalBody.md#step-4-the-swing-october-6)).

- **The physical tests** (`PhysicalBodyTests`, five): 5 of 5. Their
  figures:
  - **Each miner, its own pickaxe, ordinary strength:** Small raises it
    0.79 m in 0.60 s with the arms at 42% and the back at 28%, the upper
    hand 26% of the way to the head, and lands the head at 6.3 m/s; Long
    1.03 m, 65%, 32%, the hand all the way up, 6.1 m/s; Round 0.88 m, 36%,
    30%, the hand 18% of the way, 6.9 m/s. Hands at most 20 mm off the
    handle; the tool at most 28 degrees aside (14 are meant).
  - **Strength and weight** (Round): ordinary, arms 36%, 6.9 m/s; the
    pickaxe twice as heavy, 52%, 5.2 m/s; twice the strength, 19%, 8.4 m/s;
    half the strength with twice the weight, 0.78 m in 0.78 s at 88%,
    3.7 m/s.
  - **Adapting:** at half strength Round's upper hand goes 88% of the way
    to the head, at double strength not at all. Aimed at a low block the
    knees bend 23 cm and the head lands 11 mm from the height aimed at;
    aimed at half its height they do not bend, and it lands 13 mm from it.
  - **Frame rate:** running free, arms 36% and 6.85 m/s; at 25 frames a
    second, 37% and 6.78 m/s.
- **The bench**, all three miners at strengths 0.5, 1 and 2: at half
  strength Small and Round still manage (arms 73% and 66%); Long takes
  1.34 s to raise its pickaxe with the arms at 97%, lands it at 2.7 m/s
  and loses its line (the tool 140 degrees aside).
- **Miner tests and tool tests:** passed in the suite.
- **Full PlayMode suite, one run:** 131 tests; 127 passed, none failed,
  4 skipped as explicit (1,129 s). These are step 3's models too.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`): the miners
  in it have both hands free, the lantern and the mug at the hip. The
  swing in it is still the old one (`K`).
- **Read in pictures:** each miner's swing frame by frame; Round at two
  strengths; Round aimed at three heights.
- **What failed on the way:**
  - A name the hands' code used twice (it did not compile).
  - **The place of a blow, read from the engine, was 10 cm inside the
    block** in one case and 14 cm above it in another way of detecting
    collisions (S2). It is read from the pick's head itself now.
  - **Two tests' expectations** were written for a body that could not
    move its hand: the feeble swing is quicker now that the hand goes up
    the handle (0.78 s, where 1.42 s was expected).
- **Not tested:**
  - **A second run of the suite,** and the crowd benchmark.
  - **The cost** of the back and of sliding (the hands' own was 26
    millionths of a second a step in step 2).
  - **Small and Long aimed** at a point, and any miner aimed at a face
    (S4).
  - **The game's own mining,** which still uses the old swing.
  - **Luis's eye** on the three miners' swings.

## S3, step 5 — tiredness (October 6)

Built on the bench
([design](Design/ThePhysicalBody.md#step-5-tiredness-october-6)).

- **The physical tests** (`PhysicalBodyTests`, seven): 7 of 7. The two new
  ones:
  - **A muscle by itself:** ten seconds of all-out work spend between a
    fifth and under a half of an arm (about a third); a spent arm gives
    that much less; the other arm is not spent; half a minute of rest
    brings back more than half of it; two and a half minutes at 15% of
    what it has do not tire it; work at 50% tires it, less than all-out
    work does.
  - **A miner that goes on** (Round, its own pickaxe, 32 swings, frame for
    step): its first blow lands at 6.7 m/s with the upper hand 19% of the
    way to the head and the arms at 37%; its 26th, 40% spent, at 5.5 m/s,
    the hand 83% of the way, the arms at 55% of what is left; it rests
    once; its next blow, 15% spent, lands at 5.9 m/s. Every swing strikes.
- **The five from before** pass with tiredness in: their two swings spend
  little (5% after the first).
- **Full PlayMode suite, one run:** 133 tests; 129 passed, none failed,
  4 skipped as explicit.
- **Release build.** It passed.
- **Read in pictures:** the last swings before a rest, the rest and the
  return, from the side and from the front.
- **What failed on the way** (T1 to T4 in the design): three ways of
  resting that did not rest. Bowed over the block, the back stayed spent
  and the run never went on (it timed out, twice). Straightening with the
  hands still on the tool pulled the tool off the block. Holding the tool
  across the thighs kept the upper arm working. Also: an edit of mine
  dropped the swing's recovery step, and the miner stood still for 400
  seconds; the count of swings showed it.
- **Not tested:**
  - **Small and Long tiring,** and a rest with something hanging on the
    side the tool is carried on (Long's mug is there; the hand is put out
    past it by the arm's own allowance).
  - **The tool against the body** while carried at the side: nothing stops
    it there yet.
  - **A second run of the suite,** the crowd benchmark, the cost.
  - **Luis's eye.**

## S3 — the swing with real weight in the Ordinary Place, behind `K` (October 6)

An early part of step 11
([how to try it](Design/ThePhysicalBody.md#in-the-place-to-try-october-6)).

- **The look's own test**
  (`MinerToolTests.TheLookAtTheWorkBeginsAndEndsCleanly`, rewritten):
  passed. For each miner:
  - the look begins, it takes up its pickaxe, and its first swing strikes
    the block;
  - strength set to 2 is the body's strength at once;
  - a pickaxe at 1.5 times its weight is taken up afresh, and weighs that;
  - ending the look leaves no hands', back's or swing's component on the
    miner, no pickaxe and no block; its strength is 1 again; its hands are
    open and it is upright within 3 s; and it walks on.
- **Full PlayMode suite, one run:** 134 tests; 129 passed, none failed,
  5 skipped as explicit (1,149 s).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Read in pictures** (`PhysicalLookCapture`, new): each miner where the
  place puts it, the look begun as the key begins it, from its side (90
  frames) and from the game's camera.
  - **Ordinary strength, own pickaxe:** Small's first blow lands at
    6.2 m/s and Round's at 6.7 m/s. Long's blow is in the pictures; its
    swing was not over when the frames ended, so it has no figure.
  - **The four ends of the keys' ranges** (200 frames each): nothing
    breaks. At strength 3 with the pickaxe at 0.4 of its weight the blows
    land at 9.6 to 11.1 m/s. At strength 3 and 3 times the weight, 5.7 to
    6.7 m/s. At strength 0.3 and 0.4 of the weight, 4.0 to 5.6 m/s with
    the upper hand at the head. At strength 0.3 and 3 times the weight no
    miner can swing: Round holds the head on the block; Small's and Long's
    pickaxe slips off it and hangs head down in front of their legs.
- **What failed on the way:** changing the pickaxe's weight did not begin
  the look again. The look ended and began within one frame, and what the
  last look had added to the miner is destroyed only at the end of a
  frame, so the new one found it still there and did not begin. It now
  begins on the next frame. The look's test caught it.
- **Not tested:**
  - **The keys in the built game.** The look was driven through the calls
    the keys make, in play mode in the editor. Nobody has pressed the keys
    in the build, and the line at the foot of the screen has not been seen
    in a picture.
  - **On a slope,** or with something where the block would stand: the
    miners were where the place puts them, on level ground.
  - **Changing strength or weight many times in a row,** or while the
    miner rests.
  - **A second run of the suite,** the crowd benchmark, the cost.
  - **Luis's eye.**

## S3, step 6 — balance (October 7)

Built on the bench and in the look with `K`
([design](Design/ThePhysicalBody.md#step-6-balance-october-7)).

- **The balance's own tests** (`PhysicalBalanceTests`, four): 4 of 4.
  - **At ease it stands as it did:** with its balance, each miner's hips
    move 0.0 mm in over three seconds, it takes no step, and its feet do
    not move. Its weight's point is 109 (Small), 113 (Long) and 119 mm
    (Round) inside its feet.
  - **A light pull is leaned against and a hard one stepped for** (Round,
    at the chest, two and a half seconds each):
    - 100 N forwards: no step; hips 61 mm back against the pull; the
      weight's point never outside its feet; standing as before within
      10 mm some seconds after.
    - 200 N forwards: one step, 0.09 m the way it is pulled; feet 0.26 m
      apart while it lasts (0.19 at ease); together again after.
    - 180 N to its right: one step, 0.17 m; feet 0.52 m apart; together
      again after.
  - **A heavier body is steadier:** 110 N forwards takes Small (40.7 kg)
    three steps and 0.39 m; Round (87.6 kg) holds it with its feet where
    they are.
  - **The legs:** Round's knees give 3% of what they have standing, 44%
    with the hips 0.21 m lower, 91% at half strength; it rises from that
    in 0.34 s, and in 0.78 s at half strength; its legs were 8% spent.
- **The physical tests** (`PhysicalBodyTests`, seven): 7 of 7, with the
  balance in every one.
  - **Each with its own pickaxe:** Small, upper hand 40% of the way to
    the head, arms 42%, back 28%, 5.9 m/s; Long 100%, 69%, 31%, 5.6 m/s;
    Round 16%, 38%, 30%, 7.0 m/s.
  - **A miner that goes on** (Round, 32 swings): it rests once, after its
    19th swing (39% spent, 5.5 m/s, hand 85% of the way up); then 6.0 m/s
    at 15% spent. Its weight's point is never nearer the edge of its feet
    than 65 mm, and it takes no step.
  - **Frame rate:** running free, the tool comes down through upright at
    5.81 m/s and strikes at 6.99 m/s; at 25 frames a second, 5.80 and
    6.44 m/s. The second blows: 5.76 and 5.75 m/s through upright. The
    speed as it strikes is read at the last step before the blow and
    moves with when the contact is found (K3 in the design); the test now
    holds the speed on the way down to 0.25 m/s and the speed at the blow
    to 0.8 m/s.
- **The tools' and the miners' tests:** the look with `K` begins, strikes,
  takes a heavier pickaxe and ends leaving no balance on the miner.
- **Full PlayMode suite:** 139 tests; 133 passed, none failed, 6 skipped
  as explicit (1,212 s). This was the second run: see what failed on the
  way.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`). After the
  suite one more change was made, to the look with `K` alone (it ended
  itself when a miner stepped to keep its feet; it now ends only when the
  miner is sent somewhere). The tools' tests (3 of 3) and the build were
  run again after it; the suite was not.
- **The look with `K`, pictured again** (`PhysicalLookCapture`): each miner
  where the place puts it, with its balance. Small's first blow lands at
  5.8 m/s, Long's at 5.6 m/s, Round's at 7.0 m/s. At the four ends of the
  keys' ranges nothing breaks
  ([the table](Design/ThePhysicalBody.md#in-the-place-to-try-october-6)).
  With its own pickaxe on the bench, Small's and Round's weight's point
  stays 89 mm inside their feet. Long's goes 16 mm outside for an
  instant, and it does not step.
- **On the bench** (`PhysicalBalanceBench`, `PhysicalBench`):
  - Round pulled forwards with 100, 200 and 350 N: no step; one step;
    two steps and 0.56 m. Small with 60 N: no step, hips 80 mm; with
    110 N: three steps. Long with 110 N: one step.
  - Round with a pickaxe three times its weight (7.17 kg), ordinary
    strength: upper hand at the head, arms 71 to 77%, blows at 4.1 and
    4.2 m/s, three steps in two swings, its weight's point 57 mm outside
    its feet at worst. At 0.6 of its strength: raised in 1.24 s at 93%,
    a blow at 2.6 m/s; the second swing got 19% of the way up and did
    not strike, the tool turned 109 degrees aside; no step.
  - 21 swings with a rest: hands at most 7 mm off the handle.
- **One fact measured:** a weight of 2 kg hanging from a link like the
  arms' reports 19.62 N upwards: the engine gives a link's force as it
  acts on the hanging thing. (A probe test, not kept.)
- **Read in pictures:** each pull from the side or the front; the three
  miners' swings from the side, with what each stands on drawn from above;
  the look with `K` for each miner; the too-heavy pickaxe.
- **What failed on the way** (K1 to K9 in the design). Also:
  - The first full run of the suite failed two walking tests ("ankle
    strayed 0.273 m", "retain at least one supporting foot"). I was
    encoding clips on the same machine while it ran. Both classes passed
    alone (15 of 15), and the suite was run again with nothing beside it.
  - With the link's force taken with the wrong sign, a heavy pickaxe sent
    the miner off in ten steps; the probe settled the sign.
- **Not tested:**
  - **The keys in the built game,** as before.
  - **A slope.** Every pull and swing was on the level path.
  - **Small and Long** pulled sideways or backwards, aimed, or tiring.
  - **Pulls from behind** in the tests (seen once on the bench: it steps
    back and stands set against it).
  - **A push or pull while the miner swings.**
  - **Balance while walking:** it lets go when the miner walks.
  - **What it costs,** a second run of the suite on this code, the crowd
    benchmark.
  - **Luis's eye.**

## S3, step 7 — holding and walking with the tool (October 7)

Built on the bench and in the look with `K`
([design](Design/ThePhysicalBody.md#step-7-holding-and-walking-with-the-tool-october-7)).

- **The carry's own tests** (`PhysicalCarryTests`, four): 4 of 4.
  - **Each miner walks with its own pickaxe in one hand** (3.5 m): held
    in the left hand alone; at its own pace; a hand never more than 1 mm
    off the handle; the hand gives 15% (Small), 24% (Long) and 14%
    (Round) of its hold on average; the tool's lowest part never lower
    than 0.44, 0.66 and 0.59 m over the ground.
  - **A tool too heavy for its hand is dragged** (Round at 0.35 of its
    strength, its pickaxe three times as heavy): dragged from the start,
    by the right hand alone; the tool's lowest part 0.00 m over the
    ground on average as it walks; it pulls with 30 N at most and keeps
    66% of its pace at its slowest; it gets there, later.
  - **A tired arm drags what it carried** (Long, its pickaxe 2.9 times as
    heavy, 6 m): one hand at first, dragged by the end.
  - **What it cannot move it leaves** (Long at 0.12 of its strength, its
    pickaxe three times as heavy): dragged, then left; the pickaxe lies
    on the ground; the miner walks on to where it was sent.
- **The look's test** (`MinerToolTests.TheLookAtTheWorkBeginsAndEndsCleanly`):
  for each miner, after its first blow it is sent 2 m off: the block
  goes, it arrives with its pickaxe in one hand, `K` there sets a new
  block and it strikes it; the look then ends leaving nothing on the
  miner (no carry either).
- **The physical and balance tests:** 7 of 7 and 4 of 4. Swings are now
  compared by the speed on the way down (K3, C6): the tired miner comes
  down through upright at 5.81 m/s fresh, 4.82 m/s before its rest and
  5.57 m/s after it.
- **Full PlayMode suite:** 144 tests; 137 passed, none failed, 7 skipped
  as explicit (1,283 s). This was the third run: see what failed on the
  way.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **On the bench** (`PhysicalCarryBench`, 4 m along the path):
  - **Their own pickaxes:** in one hand; 1.16 (Small), 1.26 (Long) and
    1.18 m/s (Round) over the 3.84 m, setting off and stopping included
    (their paces are 1.27, 1.38 and 1.28 m/s); the hand gives 12%, 21%
    and 11% of its hold on average; hands at most 1 mm off the handle;
    the hips lean 24 to 25 mm at most.
  - **Round at 0.35 of its strength, 7.17 kg:** dragged; 0.82 m/s; at its
    slowest 47% of its pace, pulling 47 N; the hand gives 68% of its hold
    and the arm 86% of what it has, on average; the tool's lowest part
    19 mm over the ground and the hand 0.35 m up, on average; its right
    arm 20% spent after the 4 m.
  - **Small at 0.3, 4.51 kg:** dragged; 0.68 m/s; slowest 22% of its
    pace, pulling 28 N; the tool's lowest part 19 mm over the ground.
- **Read in pictures:** each miner walking with its own pickaxe, from its
  left; Round, Small and Long dragging, from low at the side.
- **What failed on the way** (C1 to C6 in the design). Also:
  - The first drag bent the knees and the miner squatted along; the tool
    held by its end with the arm hanging did not reach the ground and
    swung like a pendulum (its lowest part 0.11 m up on average).
  - **A walking test that was not this step's.** The laptop was shut
    down during the first full run. In the second,
    `MinerTests.EachMinerWalksToTheDoorOnItsOwnBones` failed ("Round's
    modelled ankle strayed 0.469 m"), and again alone (0.429 m, then
    Small 0.158 m). On step 6's code, which had passed that morning, the
    same class passed once and failed once (Small, 0.455 m): the laptop
    was in use, and the test compared the bones with the solved feet
    before the frame's pose, when the root has already walked on. A
    stalled frame then reads as feet that stray: 0.43 m is Round's pace
    times Unity's longest frame (0.333 s). The test now reads after the
    body is posed; the class passed three runs in three (8 of 8). Its
    limit (0.09 m) is unchanged.
- **Not tested:**
  - **The keys and move orders in the built game,** as before: the look
    was driven through the same calls in the editor.
  - **A slope,** a turn while dragging, a long walk (more than 6 m).
  - **Going back from dragging to carrying** when strength returns.
  - **The moment a heavy tool is taken to drag:** the hand is 25 to 46 mm
    off the handle for an instant (measured, left: C4).
  - **What it costs,** the crowd benchmark.
  - **Luis's eye.**

## S3, step 8, first half — the interaction click; the pickaxe laid down and picked up (October 7)

Built in the look with `K`
([design](Design/ThePhysicalBody.md#step-8-first-half-the-interaction-click-and-the-pickaxe-laid-down-and-picked-up-october-7)).

- **The interaction tests** (`InteractionTests`, three): 3 of 3.
  - **The pickaxe is laid down and picked up again by the body** (each
    miner): in its hands it offers "Lay it down" and not "Pick it up".
    Laid down in 2.5 s (Small), 3.0 s (Long) and 2.5 s (Round): the look
    ends; the pickaxe lies on the ground (its lowest part no higher than
    3 cm over it), still, 0.26 to 0.38 m from the miner, and weighs its
    own weight; the miner is upright with nothing left on it. The miner
    walks 2.5 m off and the pickaxe does not move. Lying, it offers "Pick
    it up": taken up 4.5, 4.6 and 4.9 s after the order, in the left
    hand; then carried in one hand, 0.46 to 0.71 m over the ground, the
    miner upright, the hand on the handle. `K` then puts it to work, and
    it strikes.
  - **A miner at work rests when told and goes back to it:** standing
    idle it offers nothing; at work, "Rest"; then it stands upright with
    its pickaxe and no block; then "Work here", and it swings again.
  - **The box:** armed, or open, it stops the ordinary clicks; the thing
    nearest the pointer is picked, nothing 500 pixels away; an order
    that is not offered is refused; closed, the ordinary clicks pass.
- **The carry, balance, physical and tool tests:** 4, 4, 7 and 3: all
  pass. With a knee now as strong as its body needs, Round's knees give
  36% of what they have with the hips 0.21 m lower (it was 44%), and 74%
  at half strength; it rises in 0.34 s, and in 0.46 s at half strength.
- **Full PlayMode suite, one run:** 148 tests; 140 passed, none failed,
  8 skipped as explicit (1,320 s).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Read in pictures** (`PhysicalActionCapture`): each miner laying its
  pickaxe down and picking it up, from its left.
- **What failed on the way** (A1 to A6 in the design). Also: the first
  test looked for the miner's hands in the frame the look ended, before
  they were removed.
- **Not tested:**
  - **The space bar and the mouse themselves:** the orders were given as
    the box gives them. Nobody has pressed the key in the build.
  - **A pickaxe too heavy to carry,** laid down or picked up (it is let
    go, and taken by the end of the handle, by the code; not run).
  - **A weak miner:** whether it can stand up from the squat.
  - **A slope steeper than the path,** and a pickaxe lying against
    something.
  - **What it costs,** the crowd benchmark.
  - **Luis's eye.**

## S3, step 8, second half — the lantern and the mug taken in hand and hung back (October 7)

Built for the chosen miner, with the interaction click
([design](Design/ThePhysicalBody.md#step-8-second-half-the-lantern-and-the-mug-taken-in-hand-and-hung-back-october-7)).

- **The hung things' tests** (`HungThingTests`, two): 2 of 2.
  - **What hangs by a handle is taken in hand and hung back** (Small's
    lantern, Long's mug; Round has nothing of the kind, and the click
    shows none on it):
    - **Measured from the models:** the lantern's handle 7.0 mm round,
      the thing 175 mm deep and 44 mm to each side, stopped by the body's
      side 258 mm from the pelvis; the mug's 5.6 mm, 128 mm, 50 mm and
      233 mm.
    - **On its hook** it shows itself to the click and offers "Take in
      hand", not "Hang it back".
    - **Taken:** at the side 1.80 s after the order, both. The fingers
      closed 0.0 mm (lantern) and 0.1 mm (mug) from where the handle
      hung; the arm was never stretched straight to reach it. Carried,
      standing: the handle 0.0 mm from its place in the closed fingers,
      its bar 1.6 and 1.2 degrees from lying along them; 297 and 244 mm
      from its hook; hanging 0.0 and 0.3 degrees from straight down; 37
      and 102 mm clear of the body. The arm hangs 50 mm (Small) and 0 mm
      (Long) further out than free, at the height and the place it hung
      before. The hand is not free for a tool.
    - **In the hand** it offers "Hang it back", not "Take in hand".
    - **Walking 2.5 m with it:** the handle never more than 0.0 mm from
      the fingers; it swung to 32.4 and 34.4 degrees; nearest the body,
      3.8 and 82.6 mm clear; standing again it hangs 0.0 degrees from
      straight down.
    - **Hung back:** on its hook 2.08 s after the order, both; 0.0 mm
      from its hook; the hand open, free for a tool, and 0 mm from where
      it hung before; the thing offers "Take in hand" again.
  - **A miner hangs its thing back before it takes up its pickaxe**
    (Small): with the lantern in its hand, `K` does not begin the look;
    the lantern is hung back, and 3.4 s after `K` the pickaxe is in its
    hands and the look shows the work. The look never showed while the
    lantern was in the hand. With the pickaxe in its hands the lantern
    offers nothing; after `K` again, it offers "Take in hand".
- **Full PlayMode suite, one run:** 151 tests; 142 passed, none failed,
  9 skipped as explicit (1,375 s). The figures above are that run's. The
  interaction, carry, balance, physical-body, tool and walking tests are
  in it, and pass: the arm's solver and the hung things' swing were both
  touched.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **The prefabs.** The three miners' prefabs differ from before by the
  new measures only (five lines for each hung thing).
- **Read in pictures** (`HungThingCapture`): Small with its lantern and
  Long with its mug, from the front left: taking, walking, hanging back.
- **What failed on the way** (H1 to H5 in the design). Also: the first
  test read how far the arm was from straight before the arm had been
  asked for anything.
- **Not tested:**
  - **The space bar and the mouse themselves:** the orders were given as
    the box gives them.
  - **Taking it while walking,** and hanging it back while walking (the
    code allows both; only standing was run).
  - **A turn, a slope, a run** with the thing in the hand.
  - **The right hand** (both things hang on the left).
  - **Any frame rate but 50 a second** for the hand's settling on the
    handle.
  - **What it costs,** the crowd benchmark.
  - **Luis's eye.**

## S3, step 9 — any boulder, by a click (October 7)

Built for the chosen miner, with the interaction click
([design](Design/ThePhysicalBody.md#step-9-any-boulder-by-a-click-october-7)).

- **The boulder tests** (`BoulderTests`, four): 4 of 4.
  - **Every miner finds its place at every boulder** (3 miners, the
    place's 10 boulders: 30 of 30, each on the side the miner comes
    from). For each: the swing would miss its spot by less than 3 cm
    (1, 2 and 1 mm at most for Small, Long and Round); the spot is rock
    between 0.08 m over the ground and half the miner's height (0.14 to
    0.57 m, 0.14 to 0.62 m, 0.14 to 0.53 m); the place is more than
    0.3 m from the spot, off the walked ground by less than 0.9 m (0.53,
    0.55 and 0.42 m at most), and the way to it begins on the walked
    ground; the miner would face its spot; the pick's head is to land
    within 0.2 m of the spot. Holding its pickaxe at its side would ask
    5%, 18% and 4% of a fresh shoulder.
  - **A miner mines a boulder by a click** (Small at a small low rock,
    Long at a middling one, Round at a large one; each put a few steps
    from its boulder first):
    - The boulder shows itself to the click and offers "Mine". The miner
      sets out at once with its pickaxe in its hand.
    - **At its place:** 2.7, 2.7 and 4.5 s after the order; 0 mm from the
      place planned; facing its spot within 0.0, 0.3 and 0.0 degrees; off
      the walked ground (0.43, 0.45 and 0.71 m).
    - **Striking, until a piece breaks off:** Small 5 blows in 14.4 s,
      Long 4 in 12.3 s, Round 4 in 11.3 s; landing at most 50, 120 and
      65 mm from where the plan put them. At least three recorded swings
      struck.
    - **Pieces:** Small 1 (1.49 kg), Long 2 (0.74 and 0.66 kg), Round 2
      (0.61 and 0.54 kg): each between 0.2 and 4 kg, lying still, within
      2.5 m of the spot, on the ground or on the rock.
    - **Sent somewhere else:** it comes back to the walked ground, goes
      where it was sent (within 0.6 m), with its pickaxe; it is no longer
      mining, and its agent is on the walked ground again.
  - **A loose piece that is struck is knocked aside:** a blow of less
    than a piece's cost breaks nothing off; the next does, and what it
    gave beyond the cost counts towards the next piece. The piece (0.61
    kg) came to lie; struck with 20 J where it lay, it went 1.02 m and
    lay again.
  - **To mine, it hangs its lantern back and picks its pickaxe up**
    (Small, with its pickaxe laid on the ground and its lantern in its
    hand): told to mine, it hung the lantern back, picked the same
    pickaxe up, went to the boulder and struck it 9.5 s after the
    order. It never had its pickaxe with the lantern still in its hand.
- **The bench** (`PhysicalRockBench`), by its figures and by eye:
  - **Long's rest.** Before: after four blows it rested holding its
    pickaxe at its side; its shoulder gave 59% of what it had left, and
    its spent share rose from 42% to 71% in a minute, with no end. With
    the head put down: 40% to 15% spent in about half a minute, then it
    took the pickaxe up and went on (8 blows and 3 pieces in 90 s at one
    boulder).
  - **Left at a boulder for 200 s:** Round struck 16 times, rested
    holding its pickaxe at its side (40% spent to 15% in about half a
    minute), and went on: 36 blows, 16 pieces. Small struck 8 or 9
    times between rests of the same kind: 29 blows, 6 pieces, three
    rests. Before pieces were knocked aside, Small's blows stopped
    counting after its second piece (10 in 220 s).
  - **Blows' energy:** Small 16 to 26 J, Long 39 to 62 J, Round 39 to
    53 J. A piece costs 90 J.
  - **Where blows land:** Small at one boulder, six blows within 1 cm of
    each other, 5.5 cm to the right of the plan and 7 cm short of it.
- **Full PlayMode suite, three runs** (each alone, about 21 minutes):
  1. **155 tests; 144 passed, 1 failed, 10 skipped as explicit.** The
     failure: `EquippedWorkerTests.ProducedWorkerReceivesTheBlueprintPickaxeAndMinesThroughTheSameAction`,
     "the worker must retain a supporting foot" (the older equipment
     scene, which this step does not touch but for the unit's motor). Its
     class then passed 3 times of 3 on its own (12 of 12 each).
  2. **155 tests; 145 passed, none failed, 10 skipped.** The same code.
  3. **156 tests; 146 passed, none failed, 10 skipped** (1,274 s). The
     finished code but for one last change: with pieces knocked aside,
     and the test of that.
  - **After the third run** the loose stone was given a file of its own,
    and a piece is solid to its rock again after two seconds at most.
    For that, the boulder tests were run again (4 of 4) and the build;
    not the whole suite.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Read in pictures:** each miner at a boulder, going there, striking;
  Long resting with the head of its pickaxe on the rock.
- **What failed on the way** (R1 to R10 in the design). Also: a test
  asked for a piece after four blows (Small's take five); a test took
  the lantern in the frame the look ended (the hands were still being
  removed); the first search stopped at once on a number too small to
  subtract from.
- **Not tested:**
  - **The space bar and the mouse themselves,** and the pointer on a
    boulder (the ray): the orders were given as the box gives them.
  - **A long walk to a boulder** in a test (the bench walked 12 to 37 m).
  - **A weak miner, a heavy pickaxe, a dragged pickaxe** at a boulder.
  - **Two miners at one boulder;** another unit in the place planned.
  - **The rest with the head down at the block with `K`** (Long): only
    at a boulder.
  - **A move order to a group** with a unit off the walked ground.
  - **A boulder struck hundreds of times** (it never runs out), and many
    pieces lying about a miner's feet.
  - **What it costs,** the crowd benchmark.
  - **Luis's eye.**

## S3, step 10 — the fall, and getting up (October 7)

Built to be shown as clips before it is trusted
([design](Design/ThePhysicalBody.md#step-10-the-fall-and-getting-up-october-7)).

- **The fall tests** (`PhysicalFallTests`, four): 4 of 4.
  - **A miner let go falls, lies and gets up** (each miner, shoved for
    0.3 s with three newtons for each of its kilograms, forwards and then
    backwards: six falls). For each: its let-go body weighs what the body
    weighs (within 5%); no part's place is ever not a number; it comes to
    lie; no part goes more than 3 cm into the ground (0 mm in all six);
    its parts stay within its own height of its hips; its head comes
    down below 45% of its height (to 0.09 to 0.20 m over the ground).
    Down for 3.3 to 4.7 s (falling, lying, gathering itself). Up and
    standing 1.4 to 1.7 s after that: its head at its standing height
    (within 5%), 0.00 to 0.19 m from where it lay (less than 0.4 m). It
    rose from a crouch 0.30 (Small), 0.46 (Long) and 0.35 m (Round) deep.
    One fall and one getting up are counted each time. Sent 2 m off, it
    comes back to the walked ground and goes there.
  - **A hard pull throws it down and a light one does not** (each miner,
    pulled at the chest for 1.85 s):
    - **A quarter of its weight** (100, 143 and 215 N): none fell; each
      took 1 to 3 steps, none landing short, and stood.
    - **Four fifths of its weight** (319, 459 and 688 N): each fell 0.64
      to 0.70 s after the pull began ("its steps are not catching it",
      "no step reaches where its weight is going"), and got up again.
  - **A miner that falls at its work lets its pickaxe go** (Round, let
    go by the test while it swung): the pickaxe left its hands, lay
    still in the world, and the look knew where; the block was gone; the
    look ended by itself when Round was up; "Pick it up" then had Round
    take the same pickaxe up, 5.0 s after the order.
  - **A weak miner with a pickaxe too heavy goes down and gets up**
    (Small at half its strength, a pickaxe three times its weight, in
    the look): its legs gave way after 21.7 s of work ("its legs cannot
    bear it"); it was up again 5.3 s later, from a crouch 0.14 m deep; it
    fell once; the look ended, and the pickaxe lay in the world.
- **The bench** (`PhysicalFallBench`, `PhysicalBalanceBench` with a body
  that can fall), by its figures and by eye:
  - **Nine falls** (three miners; forwards, to the side, backwards; 250 N
    for 0.3 s): all lie 1.2 to 2.6 s after the shove and stay lying;
    with getting up, all stand again 4.6 to 6.7 s after the shove, within
    9 cm of where they lay.
  - **Limp against holding itself:** the fastest part 4.4 to 6.2 m/s
    limp, 2.8 to 4.8 m/s holding; the head's coming down 3.0 to 5.5 m/s
    limp, 1.3 to 3.9 m/s holding.
  - **Pulls from 100 to 700 N:** Small steps at 100 N, stumbles at 150,
    falls from 250; Long steps to 150, stumbles at 250, falls from 350;
    Round steps to 250, falls at 350 and 500 as the rope lets go, and at
    700 while it pulls.
  - **In the look, weak with a heavy pickaxe, for 75 s:** Small and
    Round fell once (at 21.7 and 23.9 s) and got up (7 and 6 s later).
    Long, in different runs, fell once, fell twice, or did not fall at
    all in 75 s.
- **Full PlayMode suite, one run:** 161 tests; 150 passed, none failed,
  11 skipped as explicit (1,287 s). After it, only a bench's camera was
  changed (it follows a fallen body, for the clip).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Read in pictures:** each miner falling three ways, limp and then
  holding itself; lying; getting up from its back and from its front;
  Round pulled off its feet; Small's legs giving way at its work.
- **What failed on the way** (F1 to F8 in the design).
- **Not tested:**
  - **Anything Luis has seen in the build:** only clips.
  - **A fall on a slope, against or onto a rock, from a height, or onto
    a lying pickaxe or another miner.**
  - **A fall while walking,** while carrying or dragging, while picking
    a pickaxe up or laying it down, or with the lantern in the hand (the
    code puts it back on its hook; not run).
  - **A fall at a boulder** (the code leaves the boulder; not run).
  - **A very weak miner** (below half strength): whether it ever gets up.
  - **Long's fall at its work,** which differs from run to run.
  - **Many falls one after another,** and a fall as it is getting up.
  - **Frame rates other than 50 a second.**
  - **What it costs,** the crowd benchmark.
  - **Luis's eye.**

## S3, step 11 — the panel in the Ordinary Place (October 7 and 8)

The keys' place taken by a plain panel; nothing made in a hand any more.
Trying every order along the whole slider found defects that were there
before it, and most of this step's work went into them
([design](Design/ThePhysicalBody.md#step-11-the-panel-in-the-ordinary-place-october-7)).

- **The panel's tests** (`MinerPanelTests`, five): 5 of 5.
  - **The panel is with the miners, and a click on it is not a click in
    the world:** it is not shown under the choice of miner; with a miner
    chosen it is at the screen's lower left; a point on it is refused to
    the ordinary clicks (what the input asks before a click), and a
    point away from it is not.
  - **A pickaxe is put on the ground and the miner picks it up itself**
    (each miner, each of the three pickaxes: nine): it lies in no hand
    from the first moment, 0.60 m from the miner, within 3 cm of the
    ground, at the weight the panel says; the miner has nothing; the
    click names it with its kilograms and offers "Pick it up"; the miner
    stands with it 4.4 to 5.1 s after the order (in one hand: 4 to 25% of
    its hold); no second pickaxe is made; laid down again, it lies; "Take
    away" takes it away.
  - **A weak miner does not get down to its pickaxe, and stands up
    again** (each miner at 0.6 of its strength): it bent its knees 0.15,
    0.20 and 0.17 m (the deepest bend is 0.36, 0.56 and 0.42 m), each
    asked 51 to 52% of what it has holding half the body; it stood up
    3.0 to 3.1 s after the order; the pickaxe lay where it lay; it did
    not fall; the panel said why. At its own strength it then picked the
    same pickaxe up.
  - **The panel's strength is the chosen miner's, with or without a
    pickaxe:** with empty hands it takes effect at once, and is held to
    0.3 and 3. Round with the heavy pickaxe (4.30 kg) in one hand: 12% of
    its hold at its own strength, 23% at half, 6% at twice; it did not
    fall. Another miner chosen: the new one has the panel's strength, and
    the one that left is as it was built.
  - **With no pickaxe anywhere, a miner told to mine is given none:** it
    does nothing, none is made, and the panel says why.
- **Tests changed with the step:**
  - **`BoulderTests`** (five now): the pickaxe is put on the ground as
    the panel puts it, and "Mine" has the miner pick it up first (5.0 to
    5.1 s), go, and strike. Told to rest at its boulder and to go back,
    each struck it again 2.6 to 3.1 s later. Small's boulder is now one
    of ordinary height (it was the lowest of the place).
    **New:** a body does not work where its knees would be bent too
    deep. Long at the lowest boulder (its top 0.18 m over the ground):
    it came to its place, bent to its work with each knee asked 41%,
    struck nothing, left the rock 8.9 s after the order, stood up with
    its pickaxe, did not fall, and the panel said why.
  - **`InteractionTests`:** a resting miner with no boulder is offered
    nothing (it was "Work here", a block). Picked up, the pickaxe is in
    the hand 5.5 to 5.9 s after the order, and the miner stands with it
    1.3 s later (a knee asked 57 to 96% at most as it rose).
  - **`PhysicalFallTests`:** the weak miner whose legs give way at the
    block is Round (it was Small, which no longer falls there in a minute
    and a half). Its legs gave way after 23 to 27 s. Up again, it either
    stands straight, or stands bent with a back that does not raise it;
    then the panel says so, and at its own strength it straightens in
    0.8 s.
- **The bench** (`MinerPanelBench`), by its figures:
  - **Picking up, along the slider** (three miners, three pickaxes, eight
    strengths from 0.3 to 3: 72 tries): no fall. From 0.9 up, all picked
    up in 3.6 to 4.0 s; Small also at 0.8 (not the heavy one); below
    that, none. The most a knee was asked at ordinary strength: 79%
    (Small), 81 to 102% (Long; most lifting the heavy one), 55 to 66%
    (Round). Before the step it was 95 to 96% for all three.
  - **At work at a boulder of ordinary height, strengths 1, 2 and 3**
    (27 tries of six blows): 26 worked (the table in the design). Long at
    twice its strength with the heavy pickaxe was thrown off its feet by
    its own swing after one blow.
  - **A weak miner at its work** (picked up at its own strength, made
    weaker once it stood with the pickaxe; 0.5 and 0.7; 18 tries of 70 s
    or eight blows): 13 worked, landing slower and softer (the table in
    the design). Long at half its strength left the rock before its
    first blow, each time (its knees). Two fell, their legs giving way
    while they rested with the head down: Small at 0.5 with the heavy
    pickaxe (after 34 s) and Long at 0.7 with the heavy one (after 50 s).
    Long at 0.7 with the lighter two worked for 70 s with a knee asked
    144 to 146% at its rests.
  - **Weakened at the bottom of its squat** (the same bench, the
    strength lowered to 0.5 at the moment it took hold: nine tries): all
    nine fell 5.5 to 6.4 s after the order ("its legs cannot bear it").
    At 0.7 one of nine did (Long, with the heavy pickaxe).
  - **The two lowest boulders, ordinary strength** (twelve tries): no
    fall. Small and Long left the rock before their first blow, each
    time; Round worked at both (ten blows; at one of them a knee asked
    101 to 102% in the blows).
  - **Sent away from a low spot at four moments of the swing** (three
    miners: twelve): all arrived on the walked ground. Before: Small fell
    at two of the four.
  - **Long at ordinary strength, five minutes at a boulder:** 35 blows,
    15 pieces, 7 rests with its head down, no fall; its legs between 72
    and 88% fresh throughout (resting so tires them a little, and the
    work between rests them).
- **Full PlayMode suite.** Five runs, each alone.
  - **The first three** (before the changes N6 to N10): 153, 154 and
    154 of 155 passed. Every failure was Small at the place's lowest
    boulder in `BoulderTests` (it fell, or was still off the walked
    ground after a fall), a different one each run.
  - **The fourth** (with N6 to N8, and Small's boulder tests moved to a
    boulder of ordinary height): 167 tests; 155 passed, none failed, 12
    skipped as explicit (1,319 s).
  - **The fifth** (with N9 and N10, and the new test of it): 169 tests;
    156 passed, none failed, 13 skipped as explicit (1,329 s).
  - **After it:** the numbers on the panel and in a pickaxe's name were
    made to follow the machine's decimal sign, and the picture test's
    and the bench's cameras moved. `MinerPanelTests` and
    `InteractionTests` were run again: 8 of 8. The suite was not.
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Read in pictures:** the panel on the screen itself, with the three
  pickaxes on the ground and the names the space bar shows; Round
  picking its pickaxe up, going to a boulder and striking it; Round at
  six tenths of its strength not getting down to it; Small at the lowest
  boulder before the changes (going round, and falling).
- **What failed on the way** (N1 to N12 in the design).
- **Not tested:**
  - **The mouse on the panel.** Its buttons and slider were called, not
    pressed. Nobody has played the build.
  - **Another miner's pickaxe** (picked up after the miner is changed).
  - **A weak miner's whole work for longer than 70 s.**
  - **Picking up on a slope, beside a rock, or with something in the
    way.**
  - **Laying down along the slider** (only at ordinary strength).
  - **Low rock other than the place's two domes.**
  - **Round at the lowest boulder but one for longer than a minute**
    (its knees are asked all they have).
  - **Frame rates other than 50 a second.**
  - **What it costs,** the crowd benchmark (step 12).
  - **Luis's eye.**

## S3, step 12 — evidence (October 8)

What the plan's list of evidence asked for, gathered and set against it
([the table](Design/ThePhysicalBody.md#step-12-evidence-october-8)). The
build was run for the first time (by its own measures, not by a person),
and that found one more defect.

- **The evidence test** (`EvidenceTests`, one): 1 of 1. Small with its
  lantern and one pickaxe put on the ground: the lantern taken in hand;
  told to mine, the lantern hung back, the pickaxe picked up, carried to
  a boulder and worked with; a rest; the pickaxe laid down. Through 1,120
  frames (22 s) there was the one pickaxe: in the miner's hands for 814
  frames, in none for 306; it never moved more than 7 cm in a frame. The
  lantern was in the hand for 129 frames and never more than 0.37 m from
  the miner's hips. The last blow's speed and energy were measured, and
  were in the panel's line.
- **The build's own measures** (the release build, 1920 by 1080, full
  screen, the machine otherwise idle):
  - **One miner at its work** (`-wgwork`, the final code): 4.51 to
    4.83 ms a frame at its work, 4.57 to 4.72 ms standing (the tables in
    the design). The hands' step: 31 to 33 millionths of a second.
  - **The walking crowd** (`-wgcrowd`): 4.84, 5.62, 6.08 and 7.31 ms with
    0, 25, 50 and 100 miners in the strategy view (4.8, 5.5, 5.9 and 6.9
    on October 4).
  - **What the first run found:** Small "did not come to its work: it
    fell". On the bench, from where the place puts it to the nearest
    boulder of ordinary height (17 m): it fell five seconds into the
    walk, at 50 frames a second and at 200 ("its weight has been outside
    its feet too long"); Long and Round did not. With that counted only
    standing: all three, with each of their three pickaxes (nine), walked
    there and worked, and the build's measure ran through.
- **Every miner at every boulder** (`MinerPanelBench`, ordinary strength,
  its own pickaxe, from a few steps off: thirty tries).
  - **Before the change E2:** 22 mined; 8 gave the rock up for their
    knees (Small at three boulders, Long at four, Round at one); none
    fell.
  - **After it:** all thirty struck five blows (in 22 to 33 s) and broke
    one or two pieces off; none fell; none gave its rock up. The most a
    knee was asked at its work: 89% or less at 26 of them; 91% at one of
    Round's; 103 to 124% at three of Long's.
- **`BoulderTests`** (five): 5 of 5. Long at the lowest boulder: its
  knees asked 39% as it first stood to it; it found a way to stand that
  asks them 23%, and struck the rock three times within 18.2 s.
- **The captures read for the model's quality** (each miner at its work
  at 0.7 and at 3 times its strength; four of the six read, forty frames
  each): nothing breaks; Long's resting pickaxe lies in the skirt of its
  coat (the design).
- **Full PlayMode suite.** Two runs, each alone.
  - **With E1** (the long walk): 170 tests; 157 passed, none failed, 13
    skipped as explicit (1,342 s).
  - **With E2 as well** (the final code): 170 tests; 157 passed, none
    failed, 13 skipped as explicit (1,327 s).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`).
- **Not tested:**
  - **Anything Luis has seen or played.**
  - **The pickaxe through its bearer:** not built, and not measured.
  - **A crowd at physical work.**
  - **Frame rates under 50 a second** for anything but the bench's swing
    (step 2).
  - **Every boulder from more than one side.**
  - **The close audit of the model quality method** (the captures were
    read at the miners' size in play).
  - **Other hardware.**

## The playtest round, first part — the fireflies, the cabin's light, the walk, the turn, the rest (October 8)

Luis played S3 and sent ten notes
([the round's page](Reviews/2026-10-08_ThePlaytestRound.md) has what was
found and done for each, with the tables and pictures). This is what was
run.

- **The dusk at every step of the zoom** (`DuskZoomCapture`, time
  standing still; forty steps from 5 to 80 m; each step also without the
  fireflies):
  - **The light round the door, before:** it fell by 0.2 to 1% a step
    to 45 m, then **rose 7.3% and 2.8%** over the next two steps (45 to
    52 m), the bright ground from 23% to 33%.
  - **After, spreading** (the place's own setting): it never rises by
    more than 0.3% a step, from 5 to 80 m. **After, shaded** (key `0`):
    the same.
  - **Fireflies lit on the screen, before:** 9 from 5 m, 137 from 28 m,
    126 from 42 m. **After:** 0 to 25; 10 from 42 m; 6 from 80 m.
- **`DuskDetailsTests`** (eight, five of them new): 8 of 8.
- **The walk, the turn, and being sent back while walking**
  (`MotionTraceBench`, each miner, 100 frames a second; read by
  `Art/Review/motion_breaks.py`):
  - **Before:** none of the nine passed. The hips jolted 83 to 128 mm
    in one frame at every step of a walk; a foot jumped 289 to 403 mm in
    the air in a turn.
  - **After:** the three walks pass every limit (hips 2.0 to 2.7 mm).
    The three turns from standing fail on two to four frames each (hips
    5.4 to 11.6 mm up or down); the three reversals fail (a foot 48 mm
    once for Long and 122 mm once for Round; the hips 12 to 14 mm along).
- **Five minutes of work and rest, each miner with each pickaxe**
  (`MinerPanelBench -panelMuscles`, ordinary strength, from where the
  place puts the miner):
  - **Before:** four of nine did not work for five minutes (Long rested
    for ever with the light pickaxe, stuck at 77% spent with its own and
    with the heavy one, or fell; Small fell with the heavy one).
  - **After:** none fell and none stood for ever. Eight worked
    throughout (11 to 87 blows); Long with the heavy pickaxe struck
    twice, put it down, and the panel said why.
- **Full PlayMode suite,** alone.
  - **The first whole run after the walk and the turn were changed:**
    176 tests; 143 passed, **18 failed**, 15 skipped as explicit
    (1,614 s). Their causes, each put right:
    - Legs stretched 1 to 5 cm past their length (the hips were brought
      sideways to a landing and not held within reach afterwards).
    - The pelvis fell to the ground after a navigation correction (it
      went on down at the pace it had been put down at).
    - A stopped worker never came to face its work (the turn's pace was
      taken away each frame it was not travelling): eight tests of the
      older worker.
    - Boots overlapped by 23 to 43 mm in a sharp turn (the way round the
      standing boot was now reached at a pace, and came late).
    - A jog was not reached, and a body "travelling at its pace" counted
      as standing (the turn held the agent's own pace down; it holds its
      going back instead, and leaves the pace alone).
    - Three tests whose premises the new rest changed, rewritten and
      said so in them: a tired arm drags what it carried (the pickaxe is
      3.4 times as heavy now, not 2.9: a hanging arm tires more slowly);
      a weak miner with a pickaxe too heavy goes down and gets up (it
      may now give the pickaxe up before its legs give way: the test
      takes either, and asks that the panel said why).
  - **The final code:** 176 tests; **161 passed, none failed**, 15
    skipped as explicit (1,373 s).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`), and its
  own measures were run in it:
  - **One miner:** standing 4.63 to 4.73 ms a frame; at its work 4.62 to
    4.67 ms (hands 31 to 32 microseconds a step); four blows each in
    twelve seconds.
  - **A walking crowd:** 4.95, 5.73, 6.24 and 7.82 ms for none, 25, 50
    and 100 miners (before the round, the same day: 4.84, 5.62, 6.08,
    7.31).
  - Nothing was thrown in its log.
- **The judges:** six were called for the walk and the turn (three
  each); all six were cut off by the account's usage limit before they
  answered. Nothing came back.
- **Not tested:**
  - **Anything Luis has seen or played** of this.
  - **The turn and the walk in the build,** at Luis's frame rate and on
    Luis's screen (the traces are the editor's, at 100 frames a second).
  - **What the longer shadow distance costs** at night with the camera
    far from the house.
  - **The clothes, the kneeling, the getting up, the lantern** (not
    begun).
  - **Frame rates under 50 a second.** Other hardware.

## The playtest round, second part — going down for the pickaxe, the clothes, a walker sent back (October 8)

What was found and done is in
[the round's page](Reviews/2026-10-08_ThePlaytestRound.md). This is what
was run.

- **The walk, the turn, and being sent back while walking**
  (`MotionTraceBench`, each miner, 100 frames a second; read by
  `Art/Review/motion_breaks.py`):
  - **After the first part:** the three walks passed; the three turns
    failed on two to four frames each; the three reversals failed (a
    foot 15 to 122 mm; the hips 12 to 14 mm).
  - **Now:** the three walks pass. A turn from standing passes for Small,
    passes for Long, fails for Round (hips 5.4 mm on 3 frames). Sent
    back, it passes for Small, fails for Long (a foot 17 mm once), fails for
    Round (hips 9.4 mm on 4 frames).
  - **The hips' sinking on the first step out of a turn**
    (`Art/Review/motion_dips.py`): 89 mm, 336 mm and 99 mm under standing
    before; 64 mm, 98 mm and 65 mm now.
- **Taking a pickaxe up and laying it down** (`MotionTraceBench
  -traceWhat pick`, new):
  - **Before** (the code of the first part, by the same trace): from the
    order to standing with it 6.6 s, 6.9 s and 6.0 s; the head over
    the limit on 411, 461 and 450 frames.
  - **Now:** 4.2 s, 4.6 s and 4.7 s; the head over the limit on
    0, 2 and 0 frames; the hips on 0, 0 and
    0.
- **Full PlayMode suite,** alone.
  - **Runs on the way** (each alone, each looked into):
    - 160 of 161: Long, told to go back to its work, did not strike
      again within the forty seconds the test waited (it rested of its
      own accord), and the failure gave a reason left over from another
      miner. The test waits for the blow, not counting such a rest, and
      the reason is cleared with each new rock.
    - 159 of 161: a hand carrying the lantern crept to its place for two
      seconds (the easing of a free hand was applied to a hand that is
      led); and Long was offered no rest while it stepped back to its
      place between two blows (the test now waits for it to be at its
      work).
    - 160 of 161: the boots overlapped by 85 mm in a sharp turn (the new
      way round the standing boot let a foot land across it).
  - **The final code:** 176 tests; **161 passed, none failed**, 15 skipped as explicit (1,393 s).
- **Release build.** It passed (`Builds/WindowsOrdinaryPlace`), and its own measures were run in it:
  - **One miner** (two runs): standing 4.73 to 5.21 ms a frame; at its work 4.79 to 5.12 ms (hands 35 to 41 microseconds a step); three or four blows each in twelve seconds. Before this part: 4.63 to 4.73 and 4.62 to 4.67. The two runs differ from each other by as much as they differ from before.
  - **A walking crowd** (one run): 5.05, 6.14, 6.78 and 8.36 ms for none, 25, 50 and 100 miners (before this part: 4.95, 5.73, 6.24, 7.82). A hundred walking miners cost about half a millisecond more: each now works out what pace its legs allow, and how its feet clear each other, at every frame.
  - Nothing was thrown in its log.
- **The judges:** none called in this part.
- **Not tested:**
  - **Anything Luis has seen or played** of this.
  - **The pick-up, the turn and the walk in the build at Luis's frame
    rate** (the traces are the editor's, at 100 frames a second).
  - **A pickaxe too heavy to carry** (taken by its end and dragged), one
    on a slope, one against a rock: in pictures.
  - **The clothes of all three in every bend,** beyond the pick-up
    pictures in the round's page.
  - **Frame rates under 50 a second.** Other hardware.

## The playtest round, third part — after the judges of the pick-up; the lantern in the hand (October 8)

- **Three judges of the pick-up** (separate agents on the smaller
  model; sheets of Small and Long from the side and from the front; the
  trace's figures). All three: "reads as real, with a flaw". Of eight
  things they said, four stood and were put right, two were struck
  against the trace, one was not a fault, one is Luis's to decide
  ([the table](Reviews/2026-10-08_ThePlaytestRound.md#what-the-judges-of-the-pick-up-said-first-round)).
- **The traces of the pick-up** (`MotionTraceBench -traceWhat pick`),
  with the changes: all three miners pass every limit.
- **`HungThingCapture`** from behind and from the front, before and
  after: read in pictures.
- **Full PlayMode suite,** alone: 176 tests; **160 passed, 1 failed**,
  15 skipped as explicit (1,405 s). The one:
  `HungThingTests.WhatHangsByAHandleIsTakenInHandAndHungBack`, "In the
  hand, standing, it should hang down": 14.2 degrees, for 12 allowed.
  Carried before the hip, the lantern leaned on the coat. The hand now
  holds it as much further out as lets it hang straight.
- **After that one change:** `HungThingTests`, `BoulderTests`,
  `MinerTests`, `InteractionClickTests`, `PhysicalFallTests` and
  `EvidenceTests`: 20 of 20. **The whole suite was not run again.**
- **Release build:** made from the final code. **Its own measures were
  not run again.**
- **Not tested:** anything Luis has seen or played of this; a second
  round of judges; the lantern and the mug by any judge; the mug in
  pictures (only Small's lantern was pictured).

## The playtest round, fourth part — getting up after a fall by the body's own strength (October 9)

- **`PhysicalFallTests`:** 6 of 6 (two of them new).
  `PushedOverFromThePanelItFallsAndGetsItselfUp` (new): the three
  miners, pushed over backwards by the panel's button: up 7.6 to 8.2 s
  after the push, their own way.
  `LaidDownAnyWayItGetsUpByItsOwnStrength` (new): the three miners, each
  laid on its back, front, left and right: 12 of 12 up their own way;
  the old way no times; eleven at the first try, up 5.0 to 7.8 s after
  being laid; Round on its front at its second try, 13.4 s (in an
  earlier run of the same test, at its first).
  `AMinerLetGoFallsLiesAndGetsUp`: 6 of 6; it now allows 1.2 m from
  where it lay (0.25 to 1.16 m seen), where it allowed 0.4.
- **The bench** (`PhysicalFallBench -fallLays back,front`), the three
  miners: 6 of 6 up; read in pictures
  ([the round's page](Reviews/2026-10-08_ThePlaytestRound.md#l7-getting-up-after-a-fall)).
- **The search** (`GetUpSearch`): the turning over, three miners and
  twelve ways of lying: 35 of 36 flat on their fronts in one go. Five
  searches for a way from the knees onto the feet found none.
- **Three judges of the get-up** (separate agents on the smaller model;
  timed sheets of Small and Round from their backs and Long from its
  front; the figures). Two: "reads as real, with a flaw". Of ten things
  said, six stood ([the table](Reviews/2026-10-08_ThePlaytestRound.md#what-the-judges-of-the-get-up-said-first-round)).
  One change was kept (a way ends when the body is there); one was
  tried and not kept (shorter waits: two bodies then failed their first
  try).
- **The whole PlayMode suite,** alone, on the code as it is: 179 tests; **163 passed, none failed**, 16 are run only when asked (1,410 s).
- **The release build** was made from that code, and its own measures run: one miner standing 4.4 to 4.6 ms a frame and at its work 4.4 ms; the crowd 4.8, 5.6, 6.3 and 7.9 ms a frame for none, 25, 50 and 100 miners; no exceptions in either log. **The getting up itself was not run in the built player** (nothing falls in those measures).
- **Three judges of the lantern** (the same way; sheets from behind of
  Small taking it, walking with it and hanging it back). Two: "reads as
  real, with a flaw". Three things stood and were put right (the hand
  that carries goes with the walk; it looks first; the lantern is
  brought out sooner)
  ([the table](Reviews/2026-10-08_ThePlaytestRound.md#what-the-judges-of-the-lantern-said-first-round)).
  `HungThingTests`, with a new check that the carrying hand goes with
  the walk: 2 of 2 (84 mm for Small, 129 mm for Long).
- **After that last change:** the whole PlayMode suite, alone: 179 tests; **162 passed, 1 failed**, 16 are run only when asked (1,398 s). The one: `BoulderTests.AMinerMinesABoulderByAClick`, Long's blows landing 31 cm from where the plan put them, for 20 allowed. Run alone three times afterwards it passed each time (15 to 16 cm). Nothing of the swing at the rock was changed in this part, and in the run before this one (the same code but for the lantern's hand) the suite passed, 163 of 163. So Long's blows are near that limit and not steady: **not looked into**.
- **The release build** was made from the final code and its measures run again: one miner standing 4.4 to 4.7 ms a frame and at its work 4.4 ms; the crowd 5.0, 5.8, 6.3 and 8.0 ms a frame for none, 25, 50 and 100 miners; no exceptions in either log. **Neither the getting up nor the lantern was run in the built player.**
- **Three judges of the walk** (the same way; Small and Long on the
  path from the side, a picture every 0.08 s). "Reads as real, with a
  flaw"; four things stood, two were struck against the trace; nothing
  was changed after it
  ([the table](Reviews/2026-10-08_ThePlaytestRound.md#what-the-judges-of-the-walk-said-first-round-october-9)).
- **Not tested:** Luis's eye; a second round of any judges; the stutter
  after a step by any eye but the numbers'; the hook and the grip from
  the side; slopes, steps, walls,
  boulders or bodies in the way of a get-up; a weak or spent body
  getting up; a fall with a tool in both hands beyond what the existing
  test of a fall at work asks.

## The round of October 9 — how long a fallen miner lies (October 9)

On Luis's word ([message](Correspondence/2026-10-09_THE_BODYS_OWN_NOT_POSED.md));
the whole of it in [the round's page](Reviews/2026-10-09_TheBodysOwn_Round.md).

- **`PhysicalFallTests`:** 7 of 7. New:
  `AHarderFallAndASpentBodyLieLonger` (the three miners; lightly, hard,
  and lightly with legs and back spent): it lies 1.8 to 3.1 s after the
  light push, 3.9 to 7.3 s after the hard one, 3.8 to 5.2 s spent. The
  older fall tests now wait up to 24 and 30 s (a body may lie ten).
- **The bench** (`PhysicalFallBench -fallShoves 100,350,600 -fallWays
  0,90,180,270`), the three miners: 36 of 36 up their own way, 35 at the
  first try; it lies 0.8 to 6.7 s. Before the judges' change: 24 of 24
  at the first try (350 N, eight sides) and 11 of 12 (100 and 600 N).
- **A stir before it gets up: tried, and taken out.** With three
  quarters of its strength, the hard-pushed Small failed its first try;
  with under a half, 22 of 24 at the first try and hardly to be seen.
- **Three judges of the lying, twice** (separate agents on the smaller
  model; six sheets; also asked the eight things of `alive.md`). All six
  verdicts: "reads as real, with a flaw". The times read about right.
  After the first round one change was made (a falling body lets go
  when the ground has it: its legs stood in the air after it had
  landed). After both it stands that the body lies frozen and gets up
  from nothing; that is open
  ([the tables](Reviews/2026-10-09_TheBodysOwn_Round.md#what-the-judges-of-the-lying-said)).
- **The whole PlayMode suite,** alone, on the code as it is: 180 tests; **164 passed, none failed**, 16 are run only when asked (1,434 s).
- **The release build** was made from that code and its measures run: one miner standing 4.5 to 4.8 ms a frame and at its work 4.5 to 4.6 ms; the crowd 4.9, 5.6 to 5.9, 6.5 to 6.8 and 8.5 to 8.8 ms a frame for none, 25, 50 and 100 miners; no exceptions in either log. Copied to the main project and opened from there once (a measuring run): no error.
- **Not tested:** Luis's eye; a fall, the lying or the panel's words in
  the built player; a body weak, or tired by real work, lying; a fall
  with a tool in the hands, on a slope, or against something; a third
  round of judges. Long's blows at a boulder (one failure in five runs
  on October 9) passed here and are still not looked into.

## S3b step 0 — the ground for the body's own (October 9)

The whole of it, with its tables: [Design/TheBodysOwn.md](Design/TheBodysOwn.md#step-0-the-ground-for-it-october-9).
No code of the game was changed; the bench is run only when asked.

- **`PhysicalBalanceBench`,** the three miners, seven pulls each (50 to
  550 N at the chest for 2.5 s): today's posed body leans without
  stepping up to about a sixth of its weight, steps at about a quarter,
  and falls from about a third (Long) to three quarters (Small). The
  bar for the body's own.
- **`BodysOwnBench.OneJoint`** (new): one arm on one spring. An
  articulation sags as reckoned (2.83 degrees for 2.81).
- **`BodysOwnBench.OneAnkle`** (new): 86 kg on one stiffly sprung ankle
  and a boot of 1.55 kg, leaning a degree: falls at 50 steps a second
  in all four ways it was made; stands in all four at 500; in two or
  three between.
- **`BodysOwnBench.Bench`** (new), each miner in thirteen parts,
  standing twenty seconds on the path, and twenty-five on a level
  floor. Held by the engine's springs alone (stiff): all fall within
  3 s at 50 steps a second and stand swaying 5 to 15 mm at 500. Kept by
  torques worked out for its legs, its joints sprung softly: at 200
  steps a second all three stand (heads within 0.08 to 0.35 mm; 25 of
  25 after twenty seconds); at 100, Long and Round (0.11 and 0.13 mm;
  25 of 25) and not Small (14 of 25); at 50, 14 of 25 of Small and none
  of Long. Pulled with 30 N for 2.5 s at 200: Round stands and comes
  back to 0.1 mm; Small and Long fall.
- **In the air** (a second and a half, swinging its limbs): an
  articulation's weight leaves its course by 0.004 m/s at 500 steps a
  second and 0.03 to 0.08 at 50; with its parts slowed as the let-go
  body's are today, by 0.54 to 0.62 m/s.
- **Cost,** in the Editor, for each fiftieth of a second: twenty-five
  standing articulations kept by their own torques, 5.2 to 5.8 ms at
  200 steps a second and 2.6 to 2.8 ms at 100.
- **Three reviewers** who did not make it (separate agents: the
  physicist, the sceptic, the builder) read the first write-up, the
  bench and the results. They showed that the first write-up's largest
  claim (the physics must step four to ten times as fast) was not
  carried by its figures; two of their trials were run and the write-up
  rewritten
  ([what they said](Design/TheBodysOwn.md#what-the-reviewers-said)).
- **Not run:** the whole suite and the build (nothing of the game
  changed); the built player; a slope; a body carrying anything;
  torques with no springs; the plain tests near the ground. **Not
  confirmed:** that the engine's other solver was in force when it was
  tried.

## S3b step 1, first part — how often the physics steps (October 9)

[Design/TheBodysOwn.md](Design/TheBodysOwn.md#step-1-first-part-how-often-the-physics-steps-october-9).
No code of the game was changed; the bench is run only when asked.

- **`BodysOwnBench.Bench`,** each joint's spring set by rule (its own
  rate times the step at one, by the turning weight of the lighter side
  of its joint; damped to just stop that side swinging): at **50 steps
  a second** all three miners stand a minute on the path (heads within
  0.11 to 0.28 mm once still) and 25 of 25 of each still stand after a
  minute on a level floor (one start, twenty-five times). Without the
  rule, at 50: 14 of 25 of Small, none of Long.
- **Twenty-four different starts** (each set going a different way at
  the same speed; counted as standing as it stood after twenty
  seconds), Small, Long, Round: at 0.1 m/s, 11, 24, 24 at 50 steps a
  second and 24, 24, 24 at 100; at 0.2 m/s, 3, 20, 21 at 50; 19, 24, 19
  at 100; 24, 24, 24 at 200; at 0.3 m/s, 1, 4, 6; 15, 18, 17; 19, 21,
  19.
- **Before it is still** its head goes 47, 5 and 47 mm at 50 steps a
  second; 11, 5 and 8 at 100; 4, 5 and 6 at 200.
- **Cost,** in the Editor, for each fiftieth of a second: twenty-five
  standing bodies, 1.3 ms at 50 steps a second (2.6 to 2.8 at 100, 5.2
  to 5.8 at 200).
- **Pulled** with 30 N for 2.5 s at 50: Round stands; Small and Long
  fall. At 50 N Round falls.
- **One reviewer** (a separate agent, the sceptic) showed that the
  first write-up's "fifty will do" rested on one start counted
  twenty-five times, and that the stance at fifty is a poorer one; the
  different starts were then counted and the write-up rewritten
  ([what was said](Design/TheBodysOwn.md#what-the-reviewer-said)).
- **Not run:** a slope; starts that differ in lean or weight; the rule
  at other values than a half and one; the ankle's spring changed
  without the dampers; the built player; the engine's other solver with
  proof.

## S3b step 1, the rest — the body in the game, and the life in it (October 9 and 10)

[Design/TheBodysOwn.md](Design/TheBodysOwn.md#step-1-the-rest-of-it-the-body-in-the-game-and-the-life-in-it-october-9-and-10).
The body's own body is off unless the panel's switch is on.

- **`OwnBodyTests`, six tests, each on Small, Long and Round, at the
  game's fifty steps a second: all pass.** Standing still with no life
  in it (heads within 0.00, 0.01 and 0.07 mm for twenty seconds);
  nudged four ways at 0.15 m/s (heads go 11 to 28 mm, back within
  6 mm); set going at 1.2 m/s (down in 0.5 to 0.6 s, into the fall; up
  after 9 to 11 s; standing by its own joints again a second later);
  pushed over from the panel; sent 2.5 m (it gives its body back,
  walks, and stands so again 3.1 to 3.6 s after); and a minute with
  life in it (15 breaths; its weight to the other leg two or three
  times; its chest within 21 to 28 mm; then on one leg and nudged four
  ways, heads go 15 to 83 mm; tired at once, 30 to 36 breaths a
  minute). [The table](Design/TheBodysOwn.md#figures-the-final-state-small--long--round).
- **The whole PlayMode suite, on the final state:** 191 tests; **170 passed, none failed**, 21 are run only when asked (the benches and recordings); about 24 minutes. It was run three times on October 10: before the judges' first round (170, none failed); after the second round's changes (169, and one failed: the boulder test below); and on the final state (170, none failed).
- **A fault this step made, found by the suite and mended.** Drawing
  breath wrote the chest's size every frame for every miner, breathing
  or not. `BoulderTests.AMinerMinesABoulderByAClick` (Long's blows
  landing where the plan put them; Long's pickaxe hangs on its chest)
  passed three times of three on the code before this step and failed
  two times of four after it (30 cm off). With the size written only
  when breath changes it: four of four. **The same test failed once on
  October 9,** before any of this, so it can fail without it; that was
  not looked into.
- **States that failed on the way** (each on one leg and nudged at
  0.15 m/s): a fuller shift of weight (Small down on a fourth nudge);
  breath lifting the body by its legs (Long down once in three runs);
  the trunk kept upright in the world step by step (Small down). And
  before the keeper was mended (it asked for a push beyond a sole's
  edge): Round down at the settings that are in now.
- **Judges:** two rounds of three, on sheets from `OwnBodyRecord`
  ([what they said](Reviews/2026-10-10_StandingWithLife_Judges.md)).
- **The build** was made from the final state and copied to `Builds/`.
  It was started once in its own measure of a miner's work: it ran and
  ended with no error in its log. Its figures were not compared with
  earlier ones, and its measure of the crowd was not run (nothing they
  measure was changed). The switch itself was not pressed in the built
  game by me: it is tested in the Editor's play mode only.
- **Not tested:** a slope; something in its hands; more than one miner
  standing so at once; nudges given faster than it settles; a weak or
  tired body's limit on shifting (never seen to act); what it costs in
  the built game; whether Luis finds it stable, or alive.

## S3b step 1 — two more looks of breath (October 10)

[Design/TheBodysOwn.md](Design/TheBodysOwn.md#two-more-looks-of-breath-october-10).

- **`OwnBodyTests`, now seven, each on Small, Long and Round: all
  pass,** on the final state. The new one
  (`ItsBreathHasTwoMoreLooks`): with the look in its shoulders they are
  drawn 26, 33 and 31 mm higher full than empty (4, 5 and 7 mm
  without); with the look in the air, no puff in nine seconds by day
  and six after dusk; it stands through all of it.
- **The whole PlayMode suite:** 192 tests; **171 passed, none failed**, 21 are run only when asked. That run was on the two looks as they were first built. What was changed after their judges (how a puff is drawn and moves, and how high the shoulders go: drawing only) was followed by the seven tests of the body's own, which pass, and not by the whole suite again.
- **A run that hung, and why:** the new test first waited for the end
  of each frame, and Unity run without a window never comes to it. The
  worktree's Unity was stopped (only that one) and the test waits a
  frame instead.
- **Its chest following a far look, put back:** Long, on one leg and
  nudged, went 146 mm and down. Taken out again; with it out, 42 to
  80 mm.
- **Judges:** one round of three on the two looks
  ([what they said](Reviews/2026-10-10_StandingWithLife_Judges.md#the-two-more-looks-of-breath)).
  What was changed after them was looked at by me in new pictures and
  not judged again.
- **The build** was made from the final state and copied to `Builds/`.
  The two looks were not looked at in the built game by me: that the
  look of breath in the air is found there (its material is loaded by
  name) is tested in the Editor's play mode only.
- **Not tested:** the looks from where the game is usually played;
  the look in the air against a bright background (it is pale); many
  miners breathing at once (one stands so at a time).

## S3b step 1 — after Luis looked: breath drawn larger, and a size for it (October 10)

[Design/TheBodysOwn.md](Design/TheBodysOwn.md#after-luis-looked-at-it-october-10).

- **`OwnBodyTests`, seven, each on Small, Long and Round: all pass.**
  With the look in its shoulders at the first size they are drawn 42,
  53 and 49 mm higher full than empty. On one leg and nudged, heads go
  15 to 83 mm, as before (what is drawn changes nothing of that).
- **The whole PlayMode suite, on this state:** 194 tests; **171 passed, none failed**, 23 are run only when asked (the benches, the searches and the recordings); about 25 minutes.
- **With this the game has the keeper as one piece of code**
  (`OwnKeeper`, used by the game and by the bench), with the step in it
  off. The seven tests give the figures they gave before it was taken
  out of `OwnBody`.
- **Looked at by me:** each miner's chest empty and full at rest, from
  the side (hardly to be told apart, even at this size), and its
  shoulders, from in front (plainly different).
- **The build** was made from this state and copied to `Builds/`. Not
  opened by me.
- **Not tested:** whether Luis can see it. That is what the slider is
  for.

## S3b — every miner breathes on one clock (October 10, evening)

[Design/TheBodysOwn.md](Design/TheBodysOwn.md#every-miner-breathes-october-10-evening).

- **`OwnBodyTests`, seven, each on Small, Long and Round: all pass.**
  At size one, with the look in its shoulders, they are drawn 29, 33
  and 32 mm higher full than empty at rest; six puffs in nine seconds
  after dusk and none by day; tired at once, 30 to 33 breaths a minute.
  On one leg and nudged, heads go 13 to 116 mm (Long's the 116), none
  down.
- **The whole PlayMode suite:** 194 tests; **171 passed, none failed**, 23 are run only when asked; about 25 minutes.
- **The mining test, many times over,** because drawing breath touches
  what a swing is planned from (`BoulderTests.AMinerMinesABoulderByAClick`:
  blows landing within 20 cm of where they were meant):

  | What was drawn on a miner standing or walking as it did | Failed |
  |---|---|
  | Its chest and shoulders, always | 1 of 4 |
  | Its chest and shoulders, while its hands were empty | 4 of 11 |
  | Nothing (as built now: only its breath in the air) | 1 of 14 |
  | The state before any of this, the same day | 0 of 16 |
  | (October 9, before breath existed) | once |

  So drawing it on the body as it was makes the miss far more
  frequent, and it is not drawn there. Whether the state as built is
  any worse than before cannot be told from one in fourteen against
  none in sixteen. **The miss itself (thirty centimetres, Long or
  Round) is a fault older than this, and is not found.**
- **The build** was made from this state and copied to `Builds/`. Not
  opened by me.
- **Not tested:** many miners breathing at once at dusk (each draws up
  to eight puffs); a miner out of breath from real work, seen at dusk.
