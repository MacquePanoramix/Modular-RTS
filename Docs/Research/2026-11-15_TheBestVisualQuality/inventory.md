# The best visual quality: the inventory

November 15, 2026. The map every other researcher reads first.
Written by a research subagent. Nothing was run in Unity or Blender for this file. It comes from reading code, settings, documents and pictures, plus four web searches.

Tags on claims:
- (read in code) the file or setting was opened and read.
- (measured) a number from a project log such as `Docs/Validation.md`.
- (seen in a picture) looked at a picture in `Docs/Images` or in the research folders.
- (record) written in a project document, not re-checked now.
- (from a source) from a web search. All four searches were "search summary only", no page was read in full.
- (hypothesis) my reasoning. Needs a test before anyone relies on it.

Paths are relative to the project root. The gameplay root is `Assets/_WonderGather`. The Blender scripts sit in `Art/Blender`.

---

## 0. Short version

1. **The look is one custom family.** A painted shader set (`WGPainted`, `WGSky`, `WGGrass`, `WGOutline`) plus three full-screen passes (Kuwahara paint, ink, grain). It does not use normal maps, PBR, reflections, probes or baked light. It is a deliberate choice, and Luis loves its dusk mood. (read in code)
2. **Only one scene shows the final look with miners:** `TheOrdinaryPlace`. The water, clouds and far land live only on the archived branch `claude/essence-exploration`. (read in code, record)
3. **The miners are the largest quality gap.** They are procedural Blender builds: tube limbs, thin cloth, one 2048 atlas, a 1024 painted face, no normal or AO maps. The earlier research (`Docs/Research/2026-11-14_TheModelsRedrawn/`) already ranks the body faults. This file does not repeat it. It adds the texture, shader, light and tool picture around it.
4. **Light is flat on purpose.** Ambient is a three-colour gradient. There are no light probes, no reflection probes, no baked GI. Seven hand-placed lights do the dusk. (read in code)
5. **There is room to spend.** About 4.5 to 5.7 ms GPU at 1080p on an RTX 4060 Laptop, 7.0 ms with 100 miners. Target hardware is not set. (measured)
6. **Claude's tools can reach a high level, but not by hand-sculpting.** Blender by Python is strong for procedural modelling, baking, rigging, weights, cloth baking and audit renders. Shaders and editor scripts in Unity are strong. There is no image generation. Pretty faces and hands are the hard part: they come from code-painted strokes, judged by pictures. See section 3.
7. **Every gain below is an additive option beside the loved look**, as the standing rules ask.

---

## 1. What is in the game today and how each part is made

### 1.1 The renderer and the settings

| Item | Value | Source |
|---|---|---|
| Unity | 6000.6.0f1 | `ProjectSettings/ProjectVersion.txt` (read in code) |
| Pipeline | URP 17.6.0 | `Packages/manifest.json` (read in code) |
| Colour space | Linear | `ProjectSettings` (read in code) |
| Rendering path | Forward+ (`m_RenderingMode` 2) | `Assets/Settings/PC_RPAsset.asset`, `PC_Renderer.asset` (read in code) |
| HDR | On | `PC_RPAsset.asset` (read in code) |
| MSAA | Off in the asset | `PC_RPAsset.asset` (read in code) |
| AA | SMAA, High, set on the camera | camera setup in `Editor/OrdinaryPlaceSetup.cs` (read in code) |
| Renderer features | SSAO | `PC_Renderer.asset` (read in code) |
| Custom post | `LookPostEffects`: Kuwahara paint, ink, grain, injected as render-graph passes | `Scripts/Look/LookPostEffects.cs`, `Art/Shaders/WGLookPost.shader` (read in code) |
| Packages not in use | No VFX Graph, no Shader Graph content, no audio assets | `manifest.json`, asset search (read in code) |

The shader family:
- `Art/Shaders/WGCommon.hlsl` holds the shared lighting and the `_WG_*` globals.
- `WGPainted.shader` is the lit surface for miners, house and props. It has albedo, a trilight style ambient, a toon-banded main light, a rim and shadows.
- `WGOutline.shader` is the inverted-hull outline.
- `WGSky.shader` is the sky dome.
- `WGGrass.shader` is the instanced grass.
- `WGLookPost.shader` is the full-screen paint, ink and grain.
- `TimeOfDay.cs` sets all `_WG_*` globals for the hour.
(all read in code)

What the surface shader does not sample: normal maps, metallic or smoothness maps, occlusion maps, reflection probes, light probes, lightmaps, APV. (read in code)

Quality tiers: `ProjectSettings/QualitySettings.asset` has the stock levels. The game uses one real tier, the PC one. (read in code)

### 1.2 Cameras

- **Game view (RTS style):** `Scripts/Camera/RtsCamera.cs`. Orbit, pan, zoom around a focus point. `CameraCollision.cs` keeps it out of the ground and the walls. (read in code)
- **Cinematic / explore view:** `Scripts/Camera/ExploreFlight.cs`. Free flight for looking around and for capture. (read in code)
- **Capture rigs:** `Editor/MinerCapture.cs` renders portraits, silhouettes and close-ups to `Docs/Images/Miners`. This is the only structured way to look at the models in Unity. (read in code)
- **Camera effects:** no depth of field, no motion blur, no chromatic aberration, no lens flare in the volume. Bloom, tonemapping, colour adjustments and vignette only. (read in code)

### 1.3 Lighting, probes, shadows

- `Scenes/TheOrdinaryPlace.unity` has 7 lights: the sun or moon main light plus point and spot lights for the house and the lanterns. (read in code)
- **Ambient:** `RenderSettings` uses Trilight (sky, equator, ground colours), driven by `TimeOfDay`. No skybox reflection use, no probes. (read in code)
- **Shadows:** URP shadow map for the main light. `ShadowReach.cs` adjusts how far shadows reach with the hour. (read in code)
- **Window glow:** `WindowGlow.cs` makes the lit windows at dusk. It is an emissive trick plus a light, not baked light. (read in code)
- **No baked light of any kind:** no lightmaps, no APV, no reflection probes. (read in code)
- **Light contact:** the miners get soft ground contact from SSAO and the shadow map only. There is no blob or contact shadow decal for feet. (hypothesis from code and pictures)

### 1.4 Post-processing and volumes

- Volume: `Art/OrdinaryPlace/OrdinaryPlaceVolume.asset`. Bloom, Tonemapping (Neutral), Color Adjustments, Vignette. (read in code)
- Custom look values in the scene: Paint 0.8, Ink 0.8, Grain 0.6. (read in code)
- `Settings/DefaultVolumeProfile.asset` is the stock default. (read in code)
- Not used: colour LUT, white balance, depth of field, motion blur, film grain from URP (grain is the custom one), screen-space reflections (URP has none). (read in code)

### 1.5 The miners: mesh, garments, rig, weights, LODs, textures, materials, shaders

Three premade miners: Small, Long, Round. Files:
- Models and textures: `Art/Worker/Miners/Miner_*.fbx`, `Miner_*_Atlas.png` (2048), `Face_*.png` (1024), `Pickaxe_*`. Data: `miners.json`, `tools.json`.
- Prefabs: `Prefabs/Miners/Miner_*.prefab`, LOD switch at 0.18 / 0.035 / 0.004.
- Materials: `Materials/Miners/*.mat`, all on `WGPainted`.
- Blender scripts: `Art/Blender/Worker/*.py`, presets in `workers.py`.
- Unity import and setup: `Editor/MinerSetup.cs`.
(read in code)

How a miner is made:
1. Body parts are skin-modifier chains and metaballs from presets in `workers.py`. (read in code)
2. Garments (coat, trousers, boots, hat) are voxel-remeshed shells at 0.0055 voxel size, then decimated. (read in code)
3. Parts are merged and cleaned. The rig is 19 body bones, 4 skirt-flap bones and finger bones. (read in code)
4. Weights come from distance to capsules around the bones, then smoothed. (read in code)
5. LODs: about 16000 / 5000 / 1600 triangles. (record)
6. One UV island set from `smart_project`, one 2048 atlas. (read in code)
7. Albedo is baked with Cycles using an EMIT shader so the painted colour is the output. No lighting is baked in. (read in code)
8. The face is a 1024 stroke-painted texture projected from the front and re-baked into the atlas. (read in code)
9. Textures import at the default compression level 2 (probably BC1 / DXT1). (hypothesis, from the `.meta` files, not checked in the editor)
10. Blend shapes are not imported into Unity today. (read in code)

Skirt and coat motion: four flap bones, driven physically by the body. (record) This is the only secondary motion on the garments. There is no cloth simulation in Unity.

Pictures: `Docs/Images/Miners/Rigged_Together_Dusk.jpg`, `Miners_Portraits.jpg`, `Miners_Polish_Closeups.jpg`. In them the shapes read well at game distance and as a trio. In close-ups the limbs are soft tubes, the cloth is thin with few folds, and the surface has paint and no depth. (seen in a picture)

### 1.6 Faces

- The face is painted by code as brush strokes into a 1024 texture, one per miner (`Face_*.png`), in `Art/Blender/Worker/` face scripts. Luis likes today's faces and asks for them to be prettier. (read in code, record)
- The face is projected from the front onto the head mesh and baked into the shared 2048 atlas. At 2048 the head gets only a small part of the atlas, so the face is sampled at far less than 1024 on screen in many shots. (hypothesis from layout, to be measured)
- There is no facial rig: no jaw bone, no eye bones, no blend shapes in Unity. Expression today is head motion and the texture. (read in code)
- Head size: Luis allows head size to change. (record, `Docs/Correspondence/2026-11-14_THE_MODELS_PROPOSAL_ANSWERED.md`)

### 1.7 Hair

- Hair is part of the voxel-remeshed shell: a sculpted cap or tuft, painted with strokes in the atlas. No hair cards, no strands, no hair shader. (read in code)
- The outline pass gives it an edge. (read in code)

### 1.8 Props and hung things

- Pickaxes: `Art/Worker/Miners/Pickaxe_*` from Blender. Hung and held by the body per the physical-objects rule. (read in code)
- Tools data: `tools.json`. (read in code)
- Lanterns, house props and signs: made in `OrdinaryPlace/house.py` and `nature.py`. (read in code)
- Props use `WGPainted` with a small flat albedo or vertex colours, no wear maps. (read in code)

### 1.9 The world

- **Ground:** `Scripts/Look/OrdinaryGround.cs` builds a height-field mesh at runtime with vertex colours and no texture. (read in code)
- **Meadow and grass:** `GrassField.cs` plus `WGGrass.shader`, GPU-instanced with `RenderMeshIndirect`. (read in code)
- **House and buildings:** `OrdinaryPlace/house.py` builds the house; `OrdinaryPlaceSetup.cs` places it. (read in code)
- **Vegetation:** trees, bushes, flowers from `nature.py`; painted look from `painting.py`. (read in code)
- **Sky:** `WGSky.shader` with `TimeOfDay`. Sun, moon, stars, horizon glow. (read in code)
- **Water, clouds, far land:** exist only on the archived branch `claude/essence-exploration`. Luis preferred the hand-painted base. Not in this scene. (record)
- **Bench study:** `StyleStudies/style_studies.py` holds the look experiments that led to look E. (read in code)

### 1.10 VFX

- `Fireflies.cs`, `BreathInAir.cs`, `SmokePlume.cs`: small code-driven effects. (read in code)
- No Unity particle systems, no VFX Graph. No dust, no pickaxe sparks, no footstep puffs, no rock chips. (read in code)
- No audio assets. (read in code)

### 1.11 UI

- All UI is IMGUI (`OnGUI`). No UI Toolkit, no uGUI canvas, no custom font, no icons art. (read in code)
- Look dev panels: `LookDevControls.cs`, `LookBenchmark.cs`. (read in code)

---

## 2. Where quality is lost today (with evidence)

Ordered by area. "Cost" is my guess of how much it shows to Luis's eye.

### 2.1 Miners (biggest)

| # | Loss | Evidence | Cost |
|---|---|---|---|
| M1 | **Limbs read as soft tubes.** Skin-modifier chains give round, even cross sections, no muscle or bone landmarks at elbow, knee, wrist, ankle. | (seen in a picture) `Miners_Polish_Closeups.jpg`; ranked in `Docs/Research/2026-11-14_TheModelsRedrawn/` | High |
| M2 | **Cloth is thin and has few folds.** A remeshed shell with no fold detail, no seams, no wear. | (seen in a picture) same pictures | High |
| M3 | **Faces share a small atlas share.** Head area in a 2048 atlas from `smart_project`, not a dedicated face map. | (read in code), (hypothesis) on the on-screen size | Medium to high |
| M4 | **No normal, AO, curvature or detail map.** Everything is a flat painted albedo. Depth comes only from the toon bands and the outline. | (read in code) `WGPainted.shader` | High |
| M5 | **Probable DXT1 compression** on painted strokes and face. Stroke edges and skin gradients can band and block. | (hypothesis) default compression in `.meta`; (from a source) the search says Medium is DXT1 and High is BC7, which has better colour quality at the same size | Medium |
| M6 | **`smart_project` UVs.** Many small islands, seams cut through painted areas. | (read in code) | Medium |
| M7 | **LOD1 and LOD2 never audited.** 5000 and 1600 triangle levels were not looked at in the research set. | (record) the audit covers LOD0 | Unknown |
| M8 | **No face expression.** No jaw, eye or brow control. Blend shapes are not imported. | (read in code) | Medium |
| M9 | **Hair is a cap.** No strand or card shape, no hair motion. | (read in code), (seen in a picture) | Medium |
| M10 | **Body faults** (floating legs, proportion) already ranked. | (record) `Docs/Research/2026-11-14_TheModelsRedrawn/` | See that file |

### 2.2 Light and shading

| # | Loss | Evidence | Cost |
|---|---|---|---|
| L1 | **Flat ambient.** A three-colour gradient lights everything the same. Interiors, under-eaves and tree shade do not get their own colour. | (read in code) | High |
| L2 | **No reflections.** Windows, lantern glass, metal pick heads, wet ground have none. | (read in code) | Medium |
| L3 | **No contact shadow under feet.** Only SSAO and the shadow map. | (hypothesis) | Medium |
| L4 | **No bounce.** The warm house light does not tint the ground and walls around it, except by hand-placed lights. | (read in code) | Medium |
| L5 | **Seven lights only** for a lived-in scene. More lights are cheap in Forward+. | (read in code), (measured) headroom | Low |

### 2.3 Post-processing and camera

| # | Loss | Evidence | Cost |
|---|---|---|---|
| P1 | **No depth of field.** Close-ups and cinematic shots have no focus separation. | (read in code) | Medium for cinematic only |
| P2 | **No colour LUT.** All grading is by three sliders. | (read in code) | Low |
| P3 | **SMAA on a Kuwahara-filtered image.** Fine ink lines may crawl in motion. | (hypothesis) not measured | Unknown |

### 2.4 World

| # | Loss | Evidence | Cost |
|---|---|---|---|
| W1 | **Ground has no texture.** Vertex colours only. Close views show flat colour. | (read in code) | Medium |
| W2 | **Sparse dressing.** Few flowers, stones, fences, moss, wear, clutter next to the house. Reference E is fuller. | (seen in a picture) `References/E_ExpressiveDrawnLife.jpg` against `Essence_House_Times.jpg` | Medium to high |
| W3 | **No water, no clouds, no far land** in the main scene. | (record) archived branch | Luis's choice |
| W4 | **No wear or age on buildings.** No stain, moss, chipped paint, no hand-drawn marks at the edges. | (read in code) | Medium |

### 2.5 VFX and UI

| # | Loss | Evidence | Cost |
|---|---|---|---|
| V1 | **No work-impact VFX.** The pick hits rock with no chips, dust or spark. | (read in code) | Medium |
| V2 | **No footfall dust or grass response.** | (read in code) | Low to medium |
| U1 | **IMGUI-only UI.** Default font, no art, no layout system, no scaling rules. | (read in code) | Medium at the final prototype |

### 2.6 Process

- **Judging is by stills.** The method is in `Docs/ArtDirection/ModelQualityMethod.md`: audit at rest and in motion, capture exhaustively. It is good. It needs Unity runs for in-engine stills, which are blocked while the suite runs. (record)
- **Hardware floor unknown.** Spending the GPU headroom is safe only on the dev laptop until Luis names a target. (measured), (hypothesis)

---

## 3. Capability map: what Claude can do with the tools here

### 3.0 The tools, as they really are

| Tool | What it gives | Limit |
|---|---|---|
| Blender 4.5 by Python, command line | Full `bpy` in background mode. Mesh and bmesh edits, modifiers, UVs, rigging, weights, shape keys, cloth and rigid body bakes, Cycles GPU bake (OPTIX) and EEVEE Next renders, FBX export. Audit renders are known to work. (record) | No viewport. Operators that need a 3D view context may fail; they need a context override. |
| Blender MCP (live session) | Same `bpy`, plus screenshots of the open window and viewport renders. | One live session. Slow for big loops. |
| Unity by C# | Runtime code, editor scripts, batch tests, `-executeMethod` builds, render captures from editor scripts. | **Not allowed now:** a test suite is running in the worktree. |
| Unity shaders as text | HLSL and ShaderLab, fully writable and reviewable. | Result seen only by running Unity. |
| Python image tools | Pillow 12, NumPy 2.4, SciPy 1.17, OpenCV 4.13, Matplotlib. Python 3.12. (measured by an import check) | No scikit-image, trimesh, imageio, torch, onnx or OpenEXR. Installing any is a download: **needs Luis's approval.** |
| GPU | RTX 4060 Laptop, 8 GB. (measured) | Used by Unity and Blender. Do not run heavy jobs beside the suite. |
| Eyes | Claude reads pictures (PNG, JPG) and judges them. | Judgement of beauty is weaker than of faults. Aesthetic judges and Luis decide. |
| Web | Search. Fetch pages (if allowed). | **No downloads** of software, models, textures or assets without approval. |
| Image generation | **None.** | Every texture, face and painting must come from code or from procedural strokes. |

### 3.1 By kind of work

Scale: **Well** (can do to a high standard with a loop of render, look, fix), **Partly** (can do, but quality or control is limited), **Not** (cannot do now).

| Work | Rating | What it means in practice |
|---|---|---|
| **Modelling (hard surface, props, buildings)** | Well | Procedural and bmesh modelling are strong. Bevels, trim, repeated forms. The house and props already come this way. |
| **Modelling (organic bodies)** | Partly | Skin-modifier and metaball bases give clean form but a tube look. Better landmarks need explicit anatomy rules (bulge at deltoid, calf, tendon lines) coded as vertex offsets or sculpt-like displacement. This works but takes many judged loops. |
| **Sculpting by brush** | Partly, unconfirmed | `bpy.ops.sculpt.brush_stroke` is reported to need a viewport context; whether it runs in background mode is unconfirmed. (from a source, search summary only; hypothesis) Safe substitute: mesh displacement from noise and masks, Laplacian and smooth passes, multires with displace, bmesh pulls. These give surface detail, not artistry. |
| **Retopology** | Partly | Voxel and quad remesh, decimate and shrinkwrap work headless. Hand-flowed edge loops around eyes, mouth and joints can be built by a script from a template, not drawn freely. Good deformation loops need a hand-made template once. |
| **UV unwrapping** | Well | Seams, packing, per-part islands and a dedicated face map are scriptable. Texel density can be measured and set. |
| **Texture painting by hand** | Not | No brush painting. |
| **Texture painting by code** | Partly to well | Strokes, noise, gradients, edge wear and cell shading made with NumPy, OpenCV or Blender bakes. Good for the painted look already used. No photoreal skin or fabric. |
| **Baking (albedo, normal, AO, curvature, thickness, ID)** | Well | Cycles OPTIX bake works headless today for albedo. (record) Normal, AO, curvature use the same path; high-poly to low-poly bake needs a cage and tests. (hypothesis for the others) |
| **Texture compression and import settings** | Well | Set by editor script or `.meta`. BC7 gives better colour than DXT1 at the same file size on disk, with the same GPU memory as DXT5. (from a source) |
| **Shaders (Unity URP, HLSL)** | Well | The family is already written by Claude. Normal map, AO, detail, matcap or probe sampling, extra toon controls can be added as a switchable keyword. Needs a render to judge. |
| **Lighting (hand-placed)** | Well | Lights, ranges, colours, shadow settings in scenes via editor script. |
| **Lighting (baked: lightmaps, APV, reflection probes)** | Partly | APV is enabled by a setting and baked in the editor; scripted bake is plausible. (from a source, search summary only) A known issue was reported with the Lit shader in mixed mode. The custom `WGPainted` shader would need its own SH sampling code. (hypothesis) Needs a Unity run to test. |
| **Post-processing** | Well | Volume overrides, custom render-graph passes, LUTs generated by script. Depth of field is stock and one component away. |
| **Cloth** | Partly | Blender cloth modifier bakes headless and could make fold shapes for baking into normal maps. In Unity the existing four-bone flap mechanism stays physical. Unity's own Cloth component is a separate, old system. (hypothesis) A baked fold map plus bones is more fitting than runtime cloth. |
| **Hair** | Partly | Cards, ribbons or clumps can be built by script and given a hair shader. Hair motion on bones is possible. Strand-level hair is out of reach in this pipeline. |
| **Rigging** | Well | Armatures, IK helpers, bone roll, constraints from script. The present 19+4+finger rig exists. |
| **Skin weights** | Well | Capsule-distance weights already work. Heat-map weights, smoothing, mirroring and limit-per-vertex are scriptable. Weight quality is checked by posed renders. |
| **Shape keys / blend shapes** | Well in Blender, Partly in Unity | Authoring is easy. Unity needs the import toggle and a driver script. Face shapes need a pretty base first. (hypothesis) |
| **LODs** | Well | Decimate, remesh and merge per level, with silhouette checks. Needs the missing LOD1/2 audit. |
| **VFX (particles, dust, sparks)** | Well | Unity `ParticleSystem` by code and a simple painted-sprite shader. Sprites can be generated by Python. No VFX Graph is installed, so code-built particle systems are the path. |
| **UI** | Partly | UI Toolkit (UXML and USS) can be written as text. A pleasant look needs a font and icons. Fonts are a download: **needs Luis's approval.** Icons can be drawn in code (SVG or Pillow). |
| **Audio** | Not in scope | Not a visual. No audio assets exist. |
| **Photo reference** | Not | No download without approval. Existing references A to F in `Docs/ArtDirection` are the base. |

### 3.2 What Blender's Python can and cannot do headless

Can (record unless marked):
- Run EEVEE Next renders in background mode for audit pictures.
- Bake with Cycles on the GPU (OPTIX).
- Edit meshes with bmesh and operators, build modifiers, apply them, make UVs, pack, rig, set weights, make shape keys, bake cloth and rigid bodies, export FBX.
- Evaluate geometry nodes (hypothesis: stable in 4.5 background mode, not tested here).
- Run long loops and write pictures to disk for judging.

Cannot, or not reliably:
- Anything that needs a live 3D view region without a context override (some sculpt, paint and snap operators).
- Interactive hand painting or sculpting. (from a source, search summary only)
- Rendering real-time Unity results. Blender's EEVEE look is not the Unity look. Only Unity captures decide the final look.
- Install add-ons from the web without approval.

Blender 4.5 is the long-term support release, so the API is stable for the length of this project. (from a source, search summary only)

### 3.3 What the method needs, and how hard top quality is this way

The method that gets the best result from these tools is a loop: **build by script, render in Blender or Unity, read the picture, judge faults and beauty, fix, render again**, with aesthetic judges and then Luis deciding.

| Target | Hard? | Why |
|---|---|---|
| Clean, readable silhouettes and proportion | Moderate | Scripted, measurable, judged by silhouette sheets. |
| Beautiful, characterful faces | **Hard** | No image generator, no hand painting. Needs many code-painted stroke variants and picture judgement. Luis's taste is the gate. |
| Good deformation at elbows, knees, shoulders | Moderate to hard | Needs good loops and weights, checked in posed renders across all three miners. |
| Believable cloth folds | Hard | Needs baked fold detail and care that it stays painted, not realistic. |
| Surface richness (normal, AO, wear) | Moderate | Bakes are standard. The challenge is to keep the painted look. |
| World dressing, props, plants | Moderate | Procedural and high volume work well. |
| VFX, UI | Moderate | Straight to build, easy to judge, font needs approval. |
| Hair | Hard | Painted cap now. Cards possible. Strands not. |

---

## 4. Ranked quality gains

Ranked by quality raised per effort. Each is an additive option, a switch beside the loved look. Sizes are rough: S under a day of agent work, M a few days, L about a week or more. (hypothesis for all sizes)

| Rank | Gain | Size | Why it ranks here |
|---|---|---|---|
| 1 | **Texture hygiene:** BC7 for the atlases and faces, a dedicated face texture or bigger face share, check the mip chain and sampler | S | Tiny change, fixes banding and softness in the face, which Luis cares about most. |
| 2 | **Baked detail maps for miners:** normal, AO, curvature from a high-poly cloth and body pass, plus an optional shader keyword in `WGPainted` | M | Adds fold and bone depth to every miner at no runtime cost. Keeps the painted look if used gently. |
| 3 | **Work-impact and footfall VFX:** chips, dust, sparks, grass puffs, as code-built particles with painted sprites | S to M | The game is about work. It makes work visible and cheap to build. |
| 4 | **Probe or baked ambient option:** APV or hand-placed light probes plus a few reflection probes, as a switch beside the flat trilight | M | Gives interiors and shade their own colour. Needs a Unity run and a shader change to sample SH. Risk: look E is loved flat. |
| 5 | **World dressing pass:** flowers, stones, moss, fence, clutter, wear, a ground detail texture, in the spirit of reference E | M | Fills the world without touching the loved shader. High volume, procedural. |
| 6 | **Stills-first miner redraw with prettier faces and a head-size trial:** the path already agreed in the Nov 14 answers; judges first, then Luis | L | Biggest gain. Costly and the hardest to get right. |
| 7 | **Cloth fold improvement:** baked fold shapes from a Blender cloth run, plus the existing flap bones | M to L | Pairs with rank 2 and 6. |
| 8 | **UI Toolkit move and a UI look:** layout in UXML and USS, drawn icons; font only with Luis's approval | M | Needed for the final prototype, not for the current look. |
| 9 | **LOD1 and LOD2 audit and fix** | S to M | Protects quality at distance and in crowds. |
| 10 | **Face expression:** import blend shapes, a driver script, a few shapes (blink, brow, mouth) | M | Needs the pretty face first. Must stay physical and non-fake. |
| 11 | **Hair upgrade:** cards or clumps with a hair shader and bone motion | M to L | Only after the redraw decides the head. |
| 12 | **Depth of field and a colour LUT** for the cinematic view only | S | Cheap. Changes the look, so ask Luis first. |
| 13 | **Water, clouds, far land** from the archived branch, as an option | M | Luis chose the hand-painted base, so only on his request. |

---

## 5. What was not done

- Unity and Blender were **not run**. Every item tagged "read in code" is from files, not from a live editor. A suite was running in the worktree.
- The **BC1 / DXT1 guess** for the miner atlases is not confirmed. The `.meta` files were read, but the editor was not asked.
- The **face share of the atlas** was reasoned from the layout, not measured on screen.
- **LOD1 and LOD2** were not looked at.
- Whether **sculpt brush operators run in background mode** is unconfirmed. Only a search summary exists. A one-line test in Blender would settle it.
- **Normal and AO bake paths**, **APV with the custom shader**, and **cloth bake into a fold map** were not tried. All are hypotheses until tested.
- The **web search** was four generic queries, all "search summary only". No page was read in full. No competitor games or other studios' pipelines were studied. That is for the outside-research researchers.
- **Target hardware** was not found. Cost claims are for the dev laptop only.
- **No download** of any software, model, texture, font or HDRI. Anything of that kind is marked "needs Luis's approval".
- No **audio** survey, since it is not visual.
- Nothing was committed. No file outside this folder was written.
