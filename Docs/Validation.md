# Validation — September 6, 2026

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

This is a development prototype with placeholder art and a temporary HUD. It has no multiplayer, economy, formations, procedural animation or civilization customization yet. Tests validate the movement/selection boundary; they do not prove every physical input, camera angle, display resolution or visual preference. Complete Docs/Playtest.md with the user before accepting the milestone's feel.

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
