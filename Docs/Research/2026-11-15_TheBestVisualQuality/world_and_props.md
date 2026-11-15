# The world and the props: the best way, at the best quality

November 15, 2026. Written by a research subagent for the "best visual quality" deep dive.
Nothing was run in Unity or Blender. This file comes from reading code, documents and pictures, and from web pages and search summaries.
Read `inventory.md` first. This file does not repeat it. It adds the world side: what is really in the code, what the pictures show, and the best way forward for each part.

## Tags

Claims:
- (read in code) the file was opened and read, in the worktree `D:/Dev/WG` (branch `claude/wip-round`). The world scripts in `Art/Blender/OrdinaryPlace` are the same in the main project; only `faces.py` and `weights.py` differ there (a diff, by file names only).
- (measured) a number from a project log. I measured nothing new.
- (seen in a picture) looked at a picture in `Docs/Images` or `Docs/ArtDirection/References`.
- (record) written in a project document.
- (from a source) taken from a web page or a search summary.
- (hypothesis) my reasoning. It needs a test.

Sources, tagged in section 14: read in full / read in part / search summary only / weak.

Size words: S is under a day of agent work, M is a few days, L is a week or more. All are hypotheses.

---

## 0. Short version

1. **The world already has a strong base.** It is one coherent painted family. The grass (about 48 blades a square metre, wind, parting around bodies, back-glow), the sky (painted clouds, stars, moon, galaxy), the far hills (to 1500 m), the house, and the physical props are all more finished than the inventory says. (read in code, seen in a picture)
2. **Three corrections to `inventory.md`:**
   - The sky shader already draws painted clouds. The ground already rises into far hills and ranges. What lives only on the archived branch is the monumental 3D cumulus, the lake and the planar reflections. (read in code, seen in a picture: `Rigged_Together_Dusk.jpg`)
   - The house and the trees are not flat albedo. Each material is baked in Blender as a painted texture with cavities, edge wear, moss and stains (`painting.py`). The flat look at close range comes from the small number of forms, not from missing paint. (read in code)
   - The props a miner carries (lantern, mug, satchel, pickaxe, hammer) are carefully modelled, with a bail, guard bars, a flap, a buckle, a strap, and a spot light on the lantern glass. They are the best-made props in the game. (read in code, seen in a picture)
3. **The biggest world gaps, in order of how much they show:**
   - **Dressing is sparse and repeats.** Three tree models, two bushes, four rocks, no flowers, no fallen things, no clutter beyond the pots and woodpile at the house. Reference A has far more life. (read in code, seen in a picture)
   - **Trees and bushes read as faceted blobs.** The frame test in `TheEssence.md` already says "no faceted silhouettes". (seen in a picture)
   - **Every building but the house is a cube.** `Workshop.prefab` uses the built-in cube mesh. The RTS will need many buildings. (read in code)
   - **No work VFX.** No chips, dust, sparks or footfall. The game is about work. (read in code)
   - **The UI is stock IMGUI.** It does not look like part of the game at all. (seen in a picture, read in code)
   - **The far land is one flat grey-blue mass** with no layers, no tree dots and no air between ridges. (seen in a picture)
4. **What can be made by script:** almost all of it. Buildings, trees, bushes, rocks, flowers, fences, clutter, terrain heights, UI sprites, particle sprites. Blender by Python and Unity by C# and HLSL do this well. What cannot be reached by script is **hand-painted feeling at the level of a painter**. That comes from code-painted strokes, judged by pictures, and Luis's eye. See section 11.
5. **What needs Luis's approval (a download):** fonts, any package added to `manifest.json` (Cinemachine, VFX Graph, even though they are Unity's own), any store asset (SpeedTree, water packs), any texture or photo library. None is needed to reach a high level. See section 10.
6. **Every gain below is a switchable option beside the look Luis loves.** His favourite frame is the lit house at dusk from low on the path, in look E. (record: `GAME_VISION.md`, 2026-10-02) Nothing here should change that frame by default.

---

## 1. Props and hung things

### 1.1 What is in the game now

- **Lantern (Small):** a wooden grip on a wire bail, a vented brass cap, warm glass behind four guard bars, a brass base. Built by `lathe` and `tube` in `outfits.py`. The glass bone carries a spot light "a warm pool on the ground and the legs, never lighting the face from below". (read in code: `outfits.lantern`, `MinerSetup.cs` near line 760)
- **Mug (Long):** a lathe-turned tin cup with a strap handle, hanging from the fingers on its side. (read in code: `outfits.mug`)
- **Satchel (Small):** a soft box, fuller at the bottom, a flap with tongue and buckle, brass rings, a sewn strap over the shoulder. It has its own bone `Satchel` and swings from the rings with damping and a limit. (read in code: `outfits.satchel`, `MinerBody.cs`)
- **Pickaxe:** 714 triangles, a 512 atlas, sized by the arm that holds it; weight, centre and inertia are measured on a fine copy and written to `tools.json`. (read in code: `tools.py`, `tools.json`)
- **Hammer, sling, cap lamp:** also in `outfits.py`. (read in code)
- **Hanging and swinging:** `HungThing.cs`, `ThingsInHand.cs`, `MinerBody.cs`. The thing hangs by a handle, swings, is stopped by the body, and can be taken in a hand. This already meets the "physical objects" rule. (read in code)
- **House props:** the iron lantern at the door (a box frame, four wire posts, a glass box), clay pots with lumpy plants, a firewood stack, a bench, jars, a hanging rail. (read in code: `house.py` `build_props`)
- **In the picture:** the lantern reads well from every angle; its glass is the strongest warm accent in a close shot. (seen in a picture: `Method1_Sheet_Small_Lantern.jpg`) The house props read as small and low-poly: the plants in the pots are faceted blobs, the stepping stones are flat pebbles. (seen in a picture: `Painted_door_night_E.jpg`)

### 1.2 What would raise quality most

| # | Gain | Why | Size |
|---|---|---|---|
| P1 | **Wear and edge bake on every prop atlas**, in the same painted manner as the house (cavity, lit worn edge, handled spots on grips, tarnish low in the brass). The house already has this through `painting.py`; the miner props do not use it. | Metal and leather get a life beyond the frame (Visual Soul item 6). Cheap. Same Cycles bake path. | S |
| P2 | **Atlas size by on-screen size.** The pickaxe is 512 for a tool about half a metre long; the Explore camera has a 2 cm near clip. Measure texel density at the nearest view and raise to 1024 where it blurs. BC7 on these (see the surfaces file). | Close-ups of hands on tools are the most-seen close-ups. | S |
| P3 | **Lantern glass as a lit object, not a yellow box.** A soft inner glow shape (a small flame or wick form, a gradient from bright centre to amber edge) on the glass, with the flicker the house lantern already has (`TimeOfDay.Flicker`). Keep it small and warm. | Reference A and D carry the whole mood on lit glass. | S |
| P4 | **Painted metal read.** Use the `_Gloss` painted highlight already in `WGPainted` on brass and iron. It is a switch on the material. No reflection probes needed. | Metal now reads as painted flat orange. | S |
| P5 | **A richer set of everyday things**, built the same careful way and hung or placed by the rules: a kettle, a cup, a bucket, a rope coil, a spade, a lunch bundle, a bedroll, a water flask, a washing line at the house. Reference E has a kettle and a cup; A has pots and a bench with pots. | The "lived-in" cue. Props must hang, rest on something, or lie on the ground, never float. Anything a miner carries gets a bone and a swing like the satchel. | M |
| P6 | **Pot plants and herbs as painted leaf clusters**, not lumpy blobs (shares the leaf-cluster method in section 4). | The house's front is where the eye rests. | S after section 4 |

### 1.3 By script, or by third party

- **By script (all of it):** every prop above is lathe, tube, bevelled-box and bmesh work, as `outfits.py` and `house.py` already do. Wear and stains are a Cycles EMIT bake as in `painting.py`. (read in code)
- **Third party:** none needed. A prop library from a store would clash with the hand-built, measured props and with the physical rules (a bought prop has no handle bar, no measured grip). **Not recommended.**
- **Rule that binds:** a prop that a body carries gets its measured grip and mass written to data, as `tools.json` does. A prop that stands in the world gets a collider that matches its look. (record: physical-objects rule)

---

## 2. Buildings and the house

### 2.1 What is in the game now

- **The house** is one script (`house.py`, 549 lines): a leaning plaster shell with bowed walls, a timber frame, a hand-laid shingle roof, a stone chimney, deep window reveals, a doorway into a small lit room with a curtain, door, bench, fence, pots, firewood lean-to. It uses a `Part` class (boxes, cylinders, blobs with a per-vertex wobble). Materials are painted per material and baked into one texture each (`painting.py`): plaster, timber, roof, stone, clay, cloth, iron and so on, at 1024 to 2048. (read in code)
- **Empties** (`Light_*`, `Smoke_*`, `Blocker_*`) tell Unity where lights, smoke and the door blocker go. Seven lights make the dusk. (read in code)
- **Other buildings:** `Workshop.prefab` uses Unity's built-in cube mesh (file id 10202). `CivilizationBase.prefab` holds 16 cubes. (read in code) Their materials (`Workshop.mat`, `Supplydepot.mat`) are flat. These are scaffolding for mechanics, not art.
- **In the pictures:**
  - From low on the path at dusk the house is lovely and is Luis's favourite frame. (seen in a picture: `Dusk_Steps_Favourite.jpg`)
  - Close up, the roof shingles are very even rows, the plaster is a soft painted patchwork, the corners are crisp, the door is a plain board, the doorstep stones are flat discs. Reference A has a sagging, patched, tattered roof edge, uneven plaster with dark stains, a deep porch, a woodpile, a fence, plants on every ledge, and a person inside. (seen in a picture: `Painted_door_night_E.jpg` against `A_TheLitHouse.jpg`)

### 2.2 What would raise quality most

| # | Gain | Why | Size |
|---|---|---|---|
| B1 | **A house wear and irregularity pass**, as a switchable variant `House_Worn` beside the loved house. Roof: shingles vary in size, colour and tilt, a sag along the ridge, a ragged eave, a few missing or replaced boards, moss on the north side. Walls: plaster breaks away at the base and corners to show stone and lath; stains run down under sills and the roof edge. Door: planks of different widths, a latch, hinges. Doorstep: real stones with grass in the joints. | Closes most of the gap to reference A without changing the shape. It is the "worn, individual surfaces" principle (Visual Soul item 6). | M |
| B2 | **Contact with the ground.** A skirt of weeds, small stones, trampled earth and a darker damp base where the walls meet the meadow; today the walls and the grass just meet. | A house that grows out of its ground reads real. Cheap and high in effect. | S |
| B3 | **Baked cavity and curvature as a second vertex colour or a small texture**, so under-eaves, window reveals and timber joints get a cool dark. | The baked textures already have cavities; this adds them where the textures are tiled or shared. | S |
| B4 | **A parametric building kit** from the same `Part` class: a function takes size, roof pitch, window count, lean, wear and a seed, and returns a house, a workshop, a depot, a barn, a shed, a well. Each gets its own empties. | The RTS needs many buildings with one family look. One house is a hand-made hero; a kit lets the rest match it. | L |
| B5 | **Construction stages.** The same kit builds a scaffold, a half-walled stage and the finished form, so construction (already a mechanic) reads as work, not as a cube growing. | Fits "work made visible". Must be driven by real build progress, not a fake animation. | M after B4 |
| B6 | **A lit interior worth looking into**: the room seen through the door has a table, shelf, pots on a rail. Add a figure's trace (a cup of steam, a chair pushed back) rather than an animated person. | Reference A's person at the doorway is the "inhabited" cue. A real person is a miner; no fake one is needed. | S to M |

### 2.3 By script, or by third party

- **By script:** all of B1 to B6. Walls, roofs and timber are procedural; the painted bake is already scripted. Blender geometry nodes could do shingle placement and fence runs, but `bmesh` as used in `house.py` already works and is easier to review. Geometry nodes are a choice for the kit if rules grow complex (see section 11.2). (read in code, hypothesis)
- **Third party:** none needed. A building pack from a store would not match the painted bake, the leaning walls or the empties convention. **Not recommended.** If Luis ever wants a modular medieval kit, it would "need Luis's approval", and it would need re-painting to match.

---

## 3. Terrain and the meadow

### 3.1 What is in the game now

- **One height field, shared by everything.** `OrdinaryGround.cs` builds the mesh, the walkable collider, the grass density and the path from the same functions: Perlin layers for the meadow, hills that rise beyond 63 m, ridges, and ranges out to 1500 m. A flat patch under the house; a path dug 7 cm. The mesh is rebuilt at load. (read in code)
- **Resolution:** the meadow mesh has 0.5 m spacing (about 240 by 240 cells); beyond it the cells grow geometrically. Colour is per vertex: meadow, dirt on the path, far tint, a darker tone under dense grass. There is no ground texture. The painted brush noise comes from the shader in world space (`brushScale` 1.5). (read in code)
- **Meadow and grass:** about 690 000 blades over the 120 m square plus seed-head plumes, drawn by `RenderMeshIndirect`, with bodies parting the grass and wind gusts. (read in code, computed from the defaults 48 per m² and `innerHalf` 60)
- **In the pictures:**
  - The path looks soft and painterly but its edges stair-step into blocky patches, from the colour resolution and the paint filter. (seen in a picture: `Rigged_Together_Dusk.jpg`)
  - The far hills are one flat grey-blue mass with blotchy paint, with no tree dots, no field patches, no layers of air. (seen in a picture: same, and `Dusk_Steps_Favourite.jpg`)
  - Grass blades from the Explore camera are dark, even "green swords", with sharp ends and little variety of shape. (seen in a picture: `Painted_grass_golden_E.jpg`, `Rigged_Together_Dusk.jpg`)
  - Reference A's grass is long, with amber seed plumes catching the lamp, and flowing in one direction. The game's plumes exist but are few (0.35 a m²). (seen in a picture, read in code)

### 3.2 What would raise quality most

| # | Gain | Why | Size |
|---|---|---|---|
| T1 | **A ground detail layer in the ground shader**, switchable: world-space painted strokes at two scales, a trodden-earth look on the path (small stones, ruts, bare patches with grass tufts on the edge), colour mottling by a world-space mask. No new texture asset is needed; it is noise and strokes in HLSL, as `WG_PaintAlbedo` already does for the grass. A baked colour texture for the meadow, 4 to 8 pixels per metre, made by Python from the same height field, is the alternative. | Close views and the path edge. The stair-stepped path is the most visible fault in the ground. | M |
| T2 | **Path as a mesh ribbon or a decal-like overlay** with its own painted texture (stones, packed earth) and a soft grassy edge, laid along the same polyline `OrdinaryGround` uses. | Path edges stop depending on vertex spacing. | M |
| T3 | **A far-land pass:** ridges layered with aerial perspective, a few tree and field patches in the mid distance, a river or lake only if Luis asks. The heights can come from a Python heightmap with erosion (ridge and valley structure that Perlin alone lacks), written once as a small data file and read by `OrdinaryGround`. Distant ridges get a tint step per layer. | The far land is where "immense and organic" (Visual Soul item 8) is felt. Present form is the flattest thing in the frame. | M |
| T4 | **Grass variety:** two or three blade shapes (broad, thin, curled), small clump cards, a rare flower, more seed plumes near the path and the house, colour shifting from root to tip by patch. The shader already has dry patches and translucency. | The grass is the near field of every wide shot. | M |
| T5 | **Cloud shadows** as a slow scrolling mask on the ground and grass. | Makes the sky reach the ground (`TheEssence.md` device 6). A shader global; one noise lookup in `WGCommon.hlsl`. | S |
| T6 | **Footprints and trodden grass** where bodies walk, as a decaying mask: the movers array already parts the blades; a small height-of-bend texture would let the parting remain a while and spring back. | Makes the grass a physical thing. Must be driven by real contact. | M |
| T7 | **Ground LOD and the Strategy camera:** check grass density, plume count and detail layer at the strategic height. The frame test also applies there. | At 42 m the grass is a texture. | S |

### 3.3 Unity Terrain, or the custom mesh

- Unity Terrain has painted layers, trees, grass detail and holes. A search summary says the URP decal projector does **not** work on terrain details, and the screen-space decal path needs the depth-normals pass (from a source: the URP decal reference). For this project's code-built height field and shader family, I see no gain from moving to Unity Terrain, and it would leave the shared height function (grass, path, flat patch, colliders) in two places. (hypothesis)
- **Recommendation:** keep the custom mesh. Add the layers in section 3.2 to it.

### 3.4 By script, or by third party

- **By script:** all of T1 to T7. The heightmap with erosion is NumPy and SciPy, both present. Particle erosion is a short loop; a search found a small Python generator and a maintained library with particle erosion (from a source, search summary only). I would write it directly in NumPy so as not to add a dependency; installing anything is a download and "needs Luis's approval".
- **Third party:** none needed. Terrain tools, tile libraries and photo scans would clash with the painted look.

---

## 4. Vegetation

### 4.1 What is in the game now

- **Trees:** `nature.py` builds a curving trunk and branches from a skin-modified skeleton, and leaf clumps as lumpy icospheres (subdivision 3, squashed to 78%). Three tree models only (`Tree_Large`, `Tree_Small_A`, `Tree_Small_B`), placed 3 near the house and 16 on a far line, with random yaw and scale. (read in code: `nature.py`, `OrdinaryPlaceSetup.PlaceNature`)
- **Bushes:** two models, 6 placed. **Rocks:** four models, 10 placed. (read in code)
- **No flowers, no reeds, no ferns, no mushrooms, no fallen branches, no moss on stones, no hedge.** Flowers are in `TheEssence.md` as a proposal for iteration 3; the code has none. (read in code, record)
- **Painted textures:** Bark, Leaves, LeavesLight, Rock are baked by `painting.py` with a leaf-dab motif. (read in code)
- **Wind:** the `Sway` function in `WGPainted.shader` moves a whole object by height above its own origin (`lift * lift`). There is no per-branch or per-leaf phase. (read in code)
- **In the pictures:** the trees are chunky, faceted lobes with sharp light breaks between facets; they read "low poly with a filter on top". The far tree line reads well as a rhythm of small shapes. The large tree behind the house is a good silhouette in the favourite frame. (seen in a picture: `Painted_Compare_A.jpg`, `Dusk_Steps_Favourite.jpg`, `Painted_grass_golden_E.jpg`)
- **What the guides ask:** `TheEssence.md` device 2 and the frame test already say "painted clumps with leaf-dab silhouettes instead of faceted blobs" and "no faceted silhouettes". (record)

### 4.2 The best method at prototype scale

What the practitioner pages agree on (from a source; several search summaries, one blog read in full):

1. **Build the crown from clusters of leaf cards or clumps, and give them smooth, outward normals** so each cluster shades like one brushed mass. The usual way: a closed smooth proxy shape (a sphere or a squashed ellipsoid) around each cluster, and the proxy's normals copied to the cards. Then import the normals; do not let Unity recalculate them. (from a source: the foliage-normals guides; the project already imports normals, `importNormals = Import`, read in code)
2. **Paint the cluster texture as leaf dabs with a cut-out edge**, so the silhouette is leafy, not a ball. The 80.lv breakdowns use cards with hand-painted atlases for clusters, and one meadow scene hand-paints each plant. (from a source: read in part)
3. **Wind by vertex colour**: one channel for how much a part moves (branch tips more), one for a per-leaf or per-clump phase, none on the trunk, bend from height. The conventions differ between engines; the idea is the same. Keep the amplitude small. (from a source: the foliage shader pages, read in part)

What this means here:

- Leaf **cards** carry a hard cost in this game: the paint filter and the ink pass read depth and normals. Alpha-cut cards need their own depth and depth-normals passes, as the grass shader has. `WGGrass.shader` is the model. (read in code)
- A cheaper path that fits the present shader: keep **solid clumps**, but change what they are. Each clump is a flattened, noise-displaced sphere with **leaf-dab relief on the geometry** (a ring of bumps, a ragged lower edge, a few cards or small leaf shapes sticking out at the rim), and a **spherical proxy normal** so the shading is one soft mass. The silhouette then reads as leaves and the shading stays painted. (hypothesis)
- **Many more variants**, from rules, not by hand: an oak, a birch-like thin tree, a pine, a willow-like drooping tree, a dead tree, a bush with berries, a hedge piece, a reed clump, a fern, a flower set. `nature.py` already has the skeleton grower; a seed gives a new tree. (read in code, hypothesis)

### 4.3 What would raise quality most

| # | Gain | Size |
|---|---|---|
| V1 | **Leaf-clump redraw with proxy normals** (above), as a `Nature_B` set beside `Nature.fbx`, switched in the scene setup. The loved faceted set stays. | M |
| V2 | **Flowers and small plants** as accents: painted cards or tiny 3D forms (a daisy, a bellflower, a poppy near the door), placed by rules near the path, the house and the stones, with clumping. The Python painter that makes the face and atlas strokes can paint the petals. | S to M |
| V3 | **More species and more variants per species** (6 to 10 trees, 4 bushes, 8 rocks, reeds, ferns, fallen logs, stumps). One seed per variant, judged on a contact sheet. | M |
| V4 | **Per-clump flutter and branch sway** from vertex data written at build time, in `WGPainted` as a keyword. Bend must follow the same `WG_WindAt` as the grass, so wind is one physical thing. | S to M |
| V5 | **Moss, lichen and soil at the base**: a darker, mossy skirt on rocks and trunks, grass tufts at the foot (the stones sit on the ground, not in it). | S |
| V6 | **Scatter rules, not scatter noise**: clusters around trees (fallen leaves, mushrooms), flowers along the path and wall bases, reeds at low ground, rocks in groups with one big and small ones, a dead tree as a landmark. Every rule is a function of the ground (slope, path distance, shade). | M |

### 4.4 Geometry nodes, or `bmesh`

- Blender 4.5 can build a geometry-nodes group from Python and attach it with a `NODES` modifier. The pattern: a node group, `DistributePointsOnFaces` (random or Poisson), `InstanceOnPoints`, `RealizeInstances`. (from a source: a tutorial summary and the API pages, search summary only) Whether it runs cleanly in background mode on this machine is **unconfirmed**; a one-line test settles it. (hypothesis)
- For this project, `bmesh` code as in `nature.py` is already enough and is easier to review; geometry nodes pay off for scatter in Blender (a ground cover, a hedge, a thatch) when the rules grow. In Unity, scatter by C# with the shared height function (as `PlaceNature` does) is the right place, since placement must agree with the ground, the path and the grass.
- **Recommendation:** model in `bmesh`, scatter in C#, and keep geometry nodes as an optional tool for dense ground cover, after a one-line test.

### 4.5 By script, or by third party

- **By script:** all of V1 to V6.
- **Third party:** SpeedTree and store tree packs would give richer branching and built-in wind, but they are paid, need Luis's approval, and would not carry the project's painted look or its look-pass. **Not needed; not recommended** unless Luis later wants realistic species. If wanted, it "needs Luis's approval".

---

## 5. Sky

### 5.1 What is in the game now

- **`WGSky.shader`:** a painted gradient with a few broad strokes, a dusk band that gathers on the sun's side, a sun disc and halo, stars, a galactic band, a crescent moon with halo, and clouds on a curved layer, domain-warped and lit in bands from the sun or the moon. `TimeOfDay.cs` drives it all from keyframes (daylight, golden hour, lilac dusk, navy night). (read in code)
- **In the pictures:** the dusk sky has lilac brush-stroke clouds with blue shadows and reads as painted. (seen in a picture: `Dusk_Steps_Favourite.jpg`, `Rigged_Together_Dusk.jpg`) The archived look F had tall cumulus towers; it was "not adopted". (record) Luis liked the version without it better.

### 5.2 What would raise quality most

The sky is already the strongest world part. Keep it. Cheap, switchable gains:

| # | Gain | Why | Size |
|---|---|---|---|
| S1 | **Cloud shadows on the land** (see T5). | The sky reaches the ground. | S |
| S2 | **Clouds seen from the Strategy camera.** At 42 m high and a downward tilt the sky barely shows; check that the sky's colour at the horizon still gives the fog its colour. | Frame test, wide views. | S |
| S3 | **A few distinct cloud forms as an option** (the archived 3D cumulus), only as a switch and only on Luis's request. | He chose not to adopt look F; it stays an option. | M |
| S4 | **Painted sun shafts at low sun** as a cheap radial streak mask in the post pass, not a volumetric raymarch. Only if the frame test asks for it. | Light leads the emotion. | S to M |

- **Cards, a dome, or raymarched clouds:** the real-time sources say a raymarched volume replaces cards at the cost of GPU time (Guerrilla measured about 2 ms on a console) while cards or a dome cost little but light poorly between clouds. (from a source: Guerrilla's talk summary, search summary only; the dome approach is what this project uses) For a painted look that is lit "in bands", the dome shader is the right match. **Keep it.** (hypothesis)

### 5.3 By script, or by third party

- All by script (shader text, `TimeOfDay`). **No third party.** A sky pack would replace a look Luis loves.

---

## 6. Water

### 6.1 What is in the game now

- **None in the main scene.** Water, monumental clouds and far mountains were built in look F on the branch `claude/essence-exploration` and not adopted. (record)

### 6.2 The best method, if Luis asks for water

- **A stylised water shader, written by Claude** in the existing family: depth-based colour (shallow to deep, optionally banded to a few steps), edge foam from the depth difference, a slow painted ripple (two panned normal-free noises), a soft sky reflection from the sky shader itself (the sky is analytical, so the reflection costs one function call, not a planar render), and refraction by a small offset of the opaque texture. Waves are vertex Gerstner if the water is large. (from a source: Ameye's stylised water notes, read in full; hypothesis for the sky-reflection shortcut)
- **URP needs the depth texture and the opaque texture switched on** for depth colour, foam and refraction. These cost a little and touch the whole renderer, so a switch in the volume or the renderer asset, set only when water is in the scene. (from a source: read in full; read in code that SSAO already needs depth normals)
- **Version care:** water examples written for Unity 2022 have broken in Unity 6 render-graph projects (from a source: a forum summary; search summary only). The project's own custom passes are already render-graph, so Claude must write and test the shader, not paste one. (hypothesis)
- **Where it would earn its place:** a small stream or pond near the house on the Explore camera's walk; a still lake in the far land (`TheEssence.md` device 6 "water doubles the sky"); reflected lantern light is the best single use. (record, hypothesis)

### 6.3 Size, script, third party

- **Size:** M for a pond or stream, M to L for a lake with a shore and cloud reflection.
- **By script:** yes, all of it.
- **Third party:** water packs exist (Fab and Gumroad listings; one says it needs Unity 6 with render graph) (from a source: search summary only). They are not needed and would not match the painted family. **Needs Luis's approval and is not recommended.**
- **Rule:** the water is an option beside the loved dusk, and only on his request. It is not in any default scene.

---

## 7. VFX (dust, particles, work)

### 7.1 What is in the game now

- `Fireflies` (GPU roaming dots with blink and distance rules), `BreathInAir` (puffs of cold breath, one per real breath, drawn with no mesh), `SmokePlume` (chimney puffs), `WindowGlow`. All are drawn by procedural shaders with `RenderPrimitives`, no Unity `ParticleSystem`. Each ties to a real cause: breath comes from a body's breath, fireflies roam round a fixed home. (read in code)
- **Missing:** pickaxe chips and dust, footfall dust and grass flecks, a fall's puff, mined-rock debris, sparks on iron, a thrown-up leaf in a gust, a lantern's moths, rain. (read in code: none exist)

### 7.2 The best method

- **Code-built `ParticleSystem` (Shuriken)** for short, physical events, one-shot bursts spawned by a real contact event. The built-in system handles a few thousand particles; a GPU system is needed only for larger crowds. (from a source: Unity VFX guides, search summary and one page read in part) **VFX Graph is not installed.** Adding it means a package change in `manifest.json`: a download that **needs Luis's approval**, and it is not needed at this scale.
- **Painted sprites by Python:** a soft dust puff with a brushed edge, a chip, a spark, a leaf, a petal, a drop. Pillow and NumPy can paint these as small flipbook sheets, with the same stroke character as the smoke shader ("a soft blob with a brushed, irregular edge"). Use the Texture Sheet Animation module for a painted flipbook. (from a source: Unity VFX guides, search summary only; read in code for the smoke's look)
- **Layer each effect from a few simple parts**, with randomised size, lifetime and speed; pick the variant by surface (rock, earth, wood, grass) from tags or materials, with two or three variants each. (from a source: shooting-VFX and particle guides, search summary only)
- **Depth cue for soft particles:** one writer reports giving particles a height map to vary the soft-particle fade so they look less flat. (from a source: 80.lv, read in full; a short page with one idea) Useful for dust against a wall.
- **Physical rule:** every effect is caused by the body acting on the world. A chip flies from the real contact point of the pick head (the measured `head` and `tip` in `tools.json`), with a velocity from the real swing speed. A footfall puff comes from the real impact of the foot, scaled by the body's weight and the ground. No timed, fake effect. (record: no-fake-animations rule)
- **Light:** dust in a lantern pool should catch warm light. Particles can be drawn with the painted shader family, so they take the same lights, fog and paint. (hypothesis)

### 7.3 What would raise quality most

| # | Gain | Size |
|---|---|---|
| X1 | **Pick-strike chips and dust**, driven by the contact event; surface variants (rock, wood). | S to M |
| X2 | **Footfall puffs and grass flecks**, scaled by weight, speed and ground (dry path dust, wet grass none). Rare; do not litter. | S |
| X3 | **Motes in lamplight** and a soft glow around the lamp (`TheEssence.md` device 4 "next"). | S |
| X4 | **Leaf and petal drift in gusts**, from the same wind field as the grass. | S |
| X5 | **Fall and get-up dust**, from the body's real contact. The get-up work is delicate; this must not change the body, only read it. | S |
| X6 | **Rain, mist and morning fog**, only if Luis wants weather. | M |

### 7.4 By script, or by third party

- **By script:** all. **Third party:** particle packs (stylised FX bundles) exist on the Unity store (from a source: search summary only); they bring their own look and textures. **Not recommended; would need Luis's approval.**

---

## 8. The cameras (game and cinematic)

### 8.1 What is in the game now

- **Strategy (RTS) camera:** `RtsCamera.cs`. Pan, orbit, zoom to the cursor, smoothing; zoom 5 to 42 m; edge scroll; `CameraCollision.cs`. (read in code)
- **Explore (cinematic) camera:** `ExploreFlight.cs`. Soft, eased free flight; a 6 cm body, 2 cm near clip; speed shrinks near surfaces; glides to a close three-quarter view of a followed body. (read in code)
- **Effects on the camera:** bloom (0.35, threshold 1.1, scatter 0.5), neutral tonemapping, colour adjustments, vignette 0.16, SMAA High. **No depth of field, no motion blur, no lens flare, no chromatic aberration.** (read in code: `OrdinaryPlaceSetup.cs`)
- **Capture:** `MinerCapture.cs` renders portraits and close-ups; `LookBenchmark.cs` measures. (read in code)
- **Rule:** every view the game can show counts, the cinematic one too ("it will look always beautiful from all angles"). (record: `GAME_VISION.md`, 2026-11-13)

### 8.2 The best method

- **Depth of field only for the Explore camera, as a switch, off in the Strategy view.** URP has two modes: a fast Gaussian one, and a bokeh one with real camera values (focal length, aperture, focus distance). The bokeh mode follows the physical camera. (from a source: Unity URP docs, search summary only) A shallow focus pulls the eye to a face, a lantern, a hand: the "drawn lives, painted world" idea. (hypothesis)
- **Focus on the thing looked at.** The Explore camera already knows its followed body; set the focus distance to the distance to that body or to the nearest surface along the view. This is a script on the volume; no package needed. The Cinemachine auto-focus route exists but needs the package, **which needs Luis's approval**; a docs page contradicts itself on whether it works in URP (from a source: Cinemachine 3.0 and 3.1 manual pages, search summary only). I would not rely on it.
- **Interaction with the paint filter:** the Kuwahara pass blurs fine detail already. A DoF blur under it may over-soften. This needs a captured test, with the frame test as judge. (hypothesis, "not measured")
- **Motion:** the Explore camera's eased motion is already "soft and cozy". Do not add camera shake or a fake bob. Any handheld feel must come from a physical rig (a body, a spring) not from noise. (record: no-fake-animations rule, adapted; hypothesis)
- **Colour LUT:** a small look-up table generated by script (Python writes the 32-cube strip) could grade each hour as the "every hour has its palette" device wants. (record, hypothesis) It changes the look, so ask Luis first.
- **A capture rig for judges and for Luis:** fixed named shots (the favourite frame, the doorway, the path, the grove, a miner portrait, the strategic overview, a wide at dusk) at fixed hours, one command, one contact sheet. `MinerCapture.cs` and the earlier `Docs/Images/OrdinaryPlace` set show the habit; making it one stable rig would let every later change be judged on the same frames. It needs Unity, which is blocked while the suite runs. (hypothesis)

### 8.3 What would raise quality most

| # | Gain | Size |
|---|---|---|
| C1 | **A named shot set and one capture command** (above). Every other gain is judged on it. | S to M |
| C2 | **Explore-only depth of field with a script-driven focus**, off by default, with a switch on the look panel. | S |
| C3 | **Strategy camera at height:** check the frame test at 42 m (grass LOD, fog colour at the horizon, ink and paint scale). | S |
| C4 | **An hour-graded LUT** (after Luis agrees). | S |

### 8.4 By script, or by third party

- **By script:** all of it. **Third party:** Cinemachine is first-party Unity but still a package download: **needs Luis's approval**; not needed for these gains.

---

## 9. The UI's look

### 9.1 What is in the game now

- **All IMGUI** (`OnGUI`): `RtsCamera`, `WandererHud`, `FactionCreatorView`, `LookDevControls`, `MinerPanel`, `MinerChoice`, `InteractionClick`, `LivingBodyDemo`. The default skin (`GUI.skin.box`, `button`, `label`) with a few sizes. No UI Toolkit, no uGUI canvas, no font asset, no icons. (read in code)
- **In the pictures:** the faction creator is a dark grey panel set with default buttons, thin default type, and a small arrow diagram. It is a clean scaffold. It does not look like the game at all. (seen in a picture: `FactionCreator.png`)
- **The design record:** "a high-quality creation interface" and "the final HUD design remain future work"; "interface language" is **Open**. A limited early UI "must not become a ceiling". (record: `GAME_VISION.md`) So this research may suggest a direction but must not lock it.

### 9.2 The best method

- **Move to UI Toolkit** (UXML for structure, USS for style), which Unity builds for runtime UI, with themes by swapping variables. (from a source: Unity docs and a community design-system page, search summary only.) Unity's own roadmap thread (read in part) says world-space UI arrived in 6.2 and was extended in 6.3, Shader Graph for UI is in 6.3 for URP, USS filters were added, and none of the runtime features is labelled stable. So **keep the migration small and the logic separate from the look**. (from a source: Unity forum roadmap, read in part)
- **A pleasant, game-true look, drawn in code:**
  - Panels as **painted paper or cloth** in the game's own palette, nine-sliced sprites generated by Python with a brushed edge and a faint stain, not flat grey boxes.
  - **Icons drawn as simple ink and wash shapes**, made as SVG or Pillow drawings, in the same hand as the miners' ink outline.
  - **A warm light logic in the UI**: the same amber for what is active and cool blue for the rest, as the world uses.
  - **Diegetic and world-space where it fits**: a name and a tool glyph over a selected miner; a warm ring on the ground; progress as a thing on the building (a plank count on its scaffold), not a bar. World-space UI exists in Unity 6.2 and later. (from a source: Unity docs, search summary only; hypothesis for the uses)
- **Type is the biggest lever and is a download.** The default font is plain. A hand-lettered, readable face (an open licence face) would carry much of the personality. Choosing and adding a font **needs Luis's approval**. Until then use the built-in font with letter-spacing, weight and colour done well. Hand-drawn lettering for titles can also be painted by Python as images (a title, a banner), which needs no font.
- **Readability first.** The game is an RTS; the UI must stay legible over a bright day and a navy night. Test on both. (hypothesis)

### 9.3 What would raise quality most

| # | Gain | Size |
|---|---|---|
| U1 | **A UI look kit** (panel, button, tab, tooltip, card sprites by Python; one USS theme with colour variables). | M |
| U2 | **Move the miner panel and the creator to UI Toolkit** in that theme, with the logic untouched. | M to L |
| U3 | **A world-space marker set**: selection ring, destination mark, tool glyph, name tag, in the painted style. | S to M |
| U4 | **A font** (needs approval). | S, after approval |
| U5 | **Scaling rules** for 1080p, 1440p and 4K, and for a laptop screen. | S |

### 9.4 By script, or by third party

- **By script:** U1, U2, U3, U5. Python draws the sprites; C# and USS text build the rest. **Third party:** a UI kit from a store would not match; fonts and icon sets are downloads. **Needs Luis's approval**; not needed except the font.

---

## 10. By script, or needing Luis's approval

| Area | Fully by script | Would need a download (Luis's approval) |
|---|---|---|
| Props and hung things | All: lathe, tube, bevel, bake, wear, measured data | None wanted |
| Buildings, house | All: parametric kit, wear, stages | A modular building pack: not recommended |
| Terrain, meadow | All: height field, erosion, detail shader, path ribbon | Terrain tools, scans: not wanted |
| Vegetation | All: tree grower, clumps, proxy normals, scatter | SpeedTree or store trees: not wanted |
| Sky | All: shader, hour keyframes, cloud shadow | None |
| Water | All: depth, foam, sky reflection | A water pack: not wanted |
| VFX | All: code-built particles, Python flipbooks | VFX Graph package; a stylised FX pack |
| Cameras | All: DoF script, LUT, capture rig | Cinemachine package |
| UI | All: sprites, USS, world markers | A font; icon sets; a UI kit |
| Python tools | Pillow, NumPy, SciPy, OpenCV present | scikit-image, trimesh, imageio, torch, noise libraries |
| Photo or HDRI reference | Existing references A to F | Any downloaded photo, texture or HDRI |

Everything marked "not wanted" is not wanted because the game's look is one hand-made, painted family, and a bought asset would have to be repainted to fit.

---

## 11. What the method needs from Claude's real tools, and how hard it is

### 11.1 The tools

| Tool | What it does for the world | Limit |
|---|---|---|
| Blender 4.5 by Python, command line | Builds props, houses, trees, rocks, flowers; paints and bakes the per-material textures (Cycles on the GPU, EMIT); makes UVs; exports FBX; renders audit pictures. | No viewport. Some operators need a view context. Not Unity's look. |
| Blender MCP (live) | The same, plus screenshots. | One live session; slow for loops. |
| Unity by C# | Scene setup by editor script, the ground, scatter, particles, cameras, capture rigs, batch tests. | **Blocked now by the running suite.** All "in-engine" claims here are therefore not yet tested. |
| Unity shaders as text | Painted ground, wind, foliage, water, cloud shadow, UI shaders. | Seen only by running Unity. |
| Python image tools | Painted sprites (puffs, chips, petals), UI panels, icons, LUTs, heightmaps with erosion, contact sheets, picture checks. | No scikit-image, trimesh, imageio, torch. Installing is a download. |
| Eyes | Claude reads pictures and finds faults well. | Judging **beauty** is weaker than judging faults. Aesthetic judge agents, then Luis, decide. |
| Web | Search and read pages. | No downloads. |
| Image generation | None. | Every texture, sprite and painting comes from code. |

### 11.2 How hard is top quality this way, by part

| Part | How hard | Why |
|---|---|---|
| Props, hung things | **Moderate** | Measured, built from curves, baked. Already shows good quality. |
| House wear and irregularity | **Moderate** | Rules and noise with a painted bake; many judged loops to keep it painted, not noisy. |
| Building kit | **Moderate to hard** | Rule design; one family look across types; building roofs and walls that never intersect badly. |
| Terrain heights, far land | **Moderate** | Erosion is a short loop; the aerial layers are shader work. |
| Ground painted detail | **Moderate to hard** | Close views of painted ground show any mechanical repeat; strokes need variety. |
| Trees and bushes | **Moderate to hard** | Leafy silhouettes with painted shading is a craft; code-built trees tend toward "procedural". The judging loop is long. |
| Flowers, small plants | **Moderate** | Small, high in count, forgiving. |
| Sky | **Done; easy to keep** | Existing. |
| Water | **Moderate** | A well known shader family; testing in Unity 6 render graph is the risk. |
| VFX | **Moderate** | Easy to build; the painted sprite look and the physical timing need judged loops. |
| Cameras | **Easy** | A volume and a script. |
| UI | **Moderate to hard** | Layout is easy; a distinctive, **beautiful** look is hard without hand drawing or a font. |
| "Hand-painted" feeling overall | **Hard** | It is the one thing code does least well. It comes from stroke variety, restraint and Luis's eye, not from more detail. |

### 11.3 The method, in one loop

Build by script, render in Blender or in Unity, read the picture against the references and the frame test (light, air with three layers, one warm accent, a calm area, no faceted silhouettes), fix, render again; judges; Luis. The same loop as `ModelQualityMethod.md`, with contact sheets of variants (ten trees on one sheet) so choice is by eye.

---

## 12. Ranked quality gains for the world and the props

Ranked by how much each raises quality for the effort. Each is an additive switch beside the loved look. Sizes are hypotheses.

| Rank | Gain | Size | Why it ranks here |
|---|---|---|---|
| 1 | **A dressing pass:** flowers, more rock and bush variants, ground cover, wear on stones, a few things at the house (kettle, bucket, washing line), by usage rules | M | Fills the biggest gap with the least risk; reference A and E are full of life; the shader family is unchanged. |
| 2 | **Work and footfall VFX**, painted sprites, contact-driven | S to M | The game is about work. Makes work visible. Cheap. |
| 3 | **House wear and ground contact** (B1, B2) as a switchable `House_Worn` | M | Closes the gap to reference A without changing the loved shape. |
| 4 | **Leaf-clump trees and bushes with proxy normals, more species** (V1, V3) | M | Fixes "faceted blobs" named in the frame test. Risk: Luis likes the present trees in his favourite frame; keep it a switch. |
| 5 | **Ground detail layer and path ribbon** (T1, T2) | M | Fixes the stair-stepped path and the flat close ground. |
| 6 | **Prop wear bake, atlas size, lit lantern glass, painted metal** (P1 to P4) | S | Small and quick; lifts the closest-seen objects. |
| 7 | **A named shot set and one capture command** (C1) | S to M | Makes every other gain judgeable. Needs Unity. |
| 8 | **Cloud shadows and a far-land pass** (T5, T3) | S then M | Makes the world immense and ties sky to land. |
| 9 | **Grass variety** (T4) | M | Improves the near field in all wide shots. |
| 10 | **Explore-only depth of field with script focus** (C2) | S | Cheap. Changes the look; ask Luis first. |
| 11 | **A building kit and construction stages** (B4, B5) | L | Needed for the RTS; the largest effort here. |
| 12 | **A UI look kit, UI Toolkit move, world-space markers** (U1 to U3) | M to L | Needed by the final prototype; not for today's look. A font needs approval. |
| 13 | **Water** (pond, stream, lake) | M | Only on Luis's request. |

---

## 13. What was not done

- Unity and Blender were **not run**. A suite was running. Nothing here was rendered or measured on screen.
- **Not opened in a live editor:** the import settings of the world textures (compression guess is from the surfaces file), and the real colour resolution of the ground at the path edge (the stair-step was seen in pictures only).
- **The grass blade count (about 690 000)** is computed from defaults in the code, not read from a log.
- **Whether geometry nodes run in background mode** on this machine was not tested. The search found the API pattern only.
- **Depth of field with the paint filter** was not tested. It may over-soften.
- **LOD and cost** of any new world part (trees with more detail, flowers, particles) were not measured. The headroom number (4.5 to 5.7 ms) comes from `inventory.md`.
- **Target hardware** is not set; cost claims are for the dev laptop only.
- **Web research was thin by design:** about fourteen generic searches and six pages read. Pages read in full: the stylised water note, the nature-shader blog, a short particle article, a meadow breakdown. Most claims about Unity 6.6 are search summaries; check against the installed version before building.
- **No competitor games or studios** were studied beyond generic breakdowns. That is for the outside-research researchers.
- **No audio, no weather system, no day-cycle gameplay.**
- **No download** of any software, model, texture, font or package. No file was written outside this folder; nothing was committed.
- **Aesthetic judges and Luis were not consulted.** Every proposal is a hypothesis for them. Nothing here promotes an Open question to Locked: the UI language, the world setting, and the water and far land are Open, and are only proposed as options.

---

## 14. Sources

Read in full (short pages):
- Ameye, stylised water shader notes (depth colour, foam, refraction, Gerstner waves, depth and opaque textures; written for Unity 2022.2).
- Unity blog, nature shaders with Shader Graph (water, sand; vertex-colour masks, edge foam, probe against planar reflection).
- 80.lv, adding depth to particles in Unity (a height map for the soft-particle fade).
- 80.lv, a UE4 stylised meadow (hand-painted plants, large-scale colour variation, dynamic light).

Read in part:
- Unity forum roadmap, UI Toolkit status, November 2025 (world space in 6.2 and 6.3, Shader Graph for UI, filters; nothing labelled stable).
- Foliage-normals guides and add-on pages (proxy-shape normals, import, not calculate), via search results only; treat the Breath of the Wild link as unverified.

Search summary only:
- Unity URP decal renderer feature (DBuffer, screen space; not on terrain details).
- Unity URP depth of field and physical camera; Cinemachine auto focus.
- Particle system guides (Shuriken against VFX Graph; surface-based impacts; layered effects).
- Blender geometry nodes from Python (node group, distribute points on faces, instance on points).
- Foliage wind vertex-colour conventions (S&box, Torque, Godot pages).
- Hydraulic erosion in Python (a small generator; a maintained library with particle erosion).
- Real-time cloud methods (Guerrilla's raymarched clouds, billboards).
- Environment storytelling and clutter guides (usage logic, restraint).
- 80.lv painterly environment breakdowns (card clusters, hand-painted atlases, a Ghibli-inspired market scene).

Weak: a Unity forum report that a depth and foam water shader broke after a Unity 6 move; blog and listing pages for store water and FX packs; one student dissertation on clouds. Used only as warnings, not as facts.

Project files read for this file (all read in code unless noted): `Docs/Research/2026-11-15_TheBestVisualQuality/inventory.md`, `Art/Blender/OrdinaryPlace/house.py`, `nature.py`, `painting.py`; `Art/Blender/Worker/outfits.py`, `tools.py`; `Scripts/Look/OrdinaryGround.cs`, `GrassField.cs`, `Fireflies.cs`, `BreathInAir.cs`, `SmokePlume.cs`, `TimeOfDay.cs`; `Art/Shaders/WGGrass.shader`, `WGSky.shader`, `WGPainted.shader`; `Scripts/Camera/RtsCamera.cs`, `ExploreFlight.cs`; `Editor/OrdinaryPlaceSetup.cs`, `MinerSetup.cs`; `Prefabs/Workshop.prefab`; `Docs/GAME_VISION.md`, `Docs/ArtDirection/VisualSoul.md`, `TheEssence.md`; pictures in `Docs/Images/OrdinaryPlace`, `Miners`, `TheEssence`, `FactionCreator`, and reference A.
