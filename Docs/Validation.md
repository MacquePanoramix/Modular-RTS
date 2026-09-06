# Validation — September 6, 2026

The development slice compiled, passed its automated runtime tests and produced a Windows build. Final milestone acceptance remains pending user playtesting of camera feel and mouse/keyboard interaction.

## Evidence

| Check | Result |
|---|---|
| Unity Editor 6000.3.12f1 compilation | Passed |
| Scene authoring and NavMesh bake | Passed; WANDERER_SETUP_OK |
| PlayMode: navigate around wall and arrive | Passed |
| PlayMode: invalid orders preserve destination | Passed; disconnected island, out-of-range and NaN inputs |
| PlayMode: clear, reselect, disable selection | Passed |
| PlayMode suite | 3 passed, 0 failed, 0 skipped; 6.21 seconds |
| Windows x64 development build | Passed; WANDERER_BUILD_OK |
| Startup scene | TheWanderer only; serialized build settings verified |
| Asset metadata | Every asset file under Assets has a corresponding .meta |
| Rendered scene preview | Inspected: green ground, gold capsule, stone wall, shadows; no pink missing shaders |
| Manual mouse/keyboard interaction and camera limits | Not run; user playtest checklist supplied |
| Standalone executable interaction | Not run; build success is not a manual playtest |

The generated XML test report is intentionally excluded from Git because Unity writes machine-specific absolute paths into it. The durable result is recorded above. Preview: `Images/TheWanderer.png`. Local build: `../Builds/Windows/WonderGather.exe`.

## Resolved setup issues

- Restricted execution initially prevented Unity Package Manager IPC; normal local execution completed.
- The bundled template pinned Input System 1.12.0, which failed against a removed BuildTarget API. Updated to 1.17.0; subsequent compilation and tests passed.
- Unity resolved URP to 17.3.0; the manifest now explicitly pins that version.
- Removed the template tutorial, sample scene, unused input actions and stale global action reference.
- The first editor preview used unfinished shader compilation. Synchronous shader compilation produced the inspected final preview. Runtime gameplay code was unchanged.

## Remaining limitations

The long generated task path caused an API updater warning for an unused Collections package test DLL. It did not prevent compilation, the three first-party tests or the Windows build. If relocating the source archive, use a short local project path.

This is a development prototype with placeholder art and a temporary HUD. It has no multiplayer, economy, formations, procedural animation or civilization customization yet. Tests validate the movement/selection boundary; they do not prove every physical input, camera angle, display resolution or visual preference. Complete Docs/Playtest.md with the user before accepting the milestone's feel.

ProjectSettings were created from the bundled template. Unity upgraded their serialized versions on import. Feature-specific settings are product/company names, text serialization, Walkable layer 6, one enabled build scene and removal of the unused template global input asset. Graphics and quality use the template's URP assets; new Input System was already enabled in the template.
