# Surfaces, light and rendering: the best way, at the best quality

November 15, 2026. Written by a research subagent for the "best visual quality" deep dive Luis asked for on
November 14 (night). Nothing was run in Unity or Blender. A Unity test suite is running in the worktree.
This file covers texturing, shaders and materials, lighting a character, image quality, skinning quality, LODs, and what
Unity 6000.6 URP offers. Bodies, garments, faces and hair have their own files in this folder
(`modelling.md`, `faces_heads_hair.md`). This file does not repeat them. It points to them where they meet.

## How to read the tags

Claims:
- (read in code) the file or setting was opened and read in the worktree `D:/Dev/WG` (branch `claude/wip-round`).
- (measured) a number from a project log or from the inventory. Not re-measured now.
- (seen in a picture) looked at a picture in `Docs/Images` or `Docs/ArtDirection/References`.
- (record) written in a project document.
- (from a source) from the web. The source tag says how deeply it was read.
- (hypothesis) my reasoning. Needs a test before anyone relies on it.

Sources are tagged: read in full, read in part (a page was fetched and a summariser answered my questions), search
summary only, or weak. No source here was read in full. The list is in section 12.

Size words: S is under a day of agent work, M a few days, L about a week or more. All sizes are hypothesis.

---

## 0. Short version

1. **The look is one custom family, and it is a good one.** Painted lit shader, brush-broken light edge, cool coloured
   shade, warm light pools, a cool rim, fog, Kuwahara paint, ink and grain. It is why the dusk frame works. Everything
   below is meant to sit beside it as a switch, never replace it. (read in code)
2. **The miners are the cleanest surfaces in a painted world.** In the pictures the house, roof and trees carry brush
   texture. The miners are smooth flat colour with a ragged light edge. The two references for people (B, E) show
   stitched patches, worn hems, dry-brush wear and warm shade on skin. Surface richness on the miners is the biggest
   surface gain. (seen in a picture)
3. **Two shader risks are probably real today, and both are cheap to fix.** The brush noise on the miners is computed
   from world position, so it can swim over a walking body and can alias at distance. And the LOD group has no
   cross-fade, and the shader has no cross-fade code, so LODs pop. (read in code; the swim and shimmer are hypothesis
   until seen in motion)
4. **The texture format is probably better than the inventory guessed.** The `.meta` files say the per-platform
   override is off, so the default setting (Compressed HQ, which is BC7 on desktop) applies. The inventory's "probably
   DXT1" is likely wrong. Not confirmed in the editor. (read in code)
5. **The strongest mood gains are small shader terms, not big systems.** A warm rim from the house's lights on
   the miners, a warm bounce from lit ground, a warm skin terminator, and a grounding shadow at the feet. Together they
   make "warm light finds them" true. Each is a switch. (hypothesis, from the pictures and the code)
6. **Baked probes (APV) are the one big lighting system worth a trial, and it is the riskiest.** Luis loves the flat
   dusk. APV should be tried only on the interior of the house, as a switch. (hypothesis)
7. **Image quality has cheap options not yet used:** 64-bit HDR buffer, camera dithering, bloom high-quality filtering,
   MSAA 4x beside SMAA, a depth-aware paint radius, and a supersampled capture mode for judging. (read in code for the
   current settings; the gains are hypothesis)
8. **Claude can do this well with Blender by Python and Unity shaders as text.** The hard part is not code. It is
   beauty: no image generator, no hand painting, so the surfaces come from code-painted strokes, judged by pictures.
   See section 9.

---

## 1. Where the surfaces and the light are today

### 1.1 The shading model (read in code)

`WGPainted.shader` with `WGCommon.hlsl` is the one surface shader for miners, house, props and ground. What it does, in
order:

1. Colour: `_BaseColor * vertex colour * _BaseMap`. Then, if the look is on, a hue and value drift
   (`WG_PaintAlbedo`: a low-frequency noise plus a dab field) is added from the **world position**.
2. Brush field: `WG_Brush` builds elongated "dabs" from two Voronoi layers in a plane tangent to the surface, again from
   **world position**. The brush field breaks the light edge: `edge = n.l + (brush - 0.5) * brushBreak`.
3. Direct light: `smoothstep(-softness, softness, edge)`, so one soft band. Shadow-map light is also broken by the
   brush. Local lights get a gentler falloff (`pow(atten, 0.75)`) and a slight warm boost, so they make "pools".
4. Translucency: `saturate(-n.l) * _Translucency`, a back-light term. Miners use 0.12.
5. Painted highlight: a small Blinn spot, `_Gloss` 0.08 on miners.
6. Ambient: a trilight from `_WG_Sky` and `_WG_Ground`. Multiplied by `s.occlusion`, which is vertex alpha times SSAO.
7. Shade: where direct light is missing, colour is multiplied by a cool tint and saturation is raised.
8. Rim: `pow(1 - n.v, 3) * _WG_Rim * albedo * 1.5`, a constant cool colour. It does not look at the light.
9. Fog: aerial perspective, warm towards the sun.

Look E (the one Luis loves) is `_WG_Paint = (1, 0.5, 3, 0)`: colour variation 1, light-edge break 0.5, brush scale
multiplier 3, painted (not plain Lambert). (read in code: `LookDevControls.cs`)

### 1.2 What the shader does not do (read in code)

- No normal map, no AO map, no curvature, no detail texture. Depth comes only from the light bands, the outline and the
  ink pass.
- No tangent frame at all. The vertex shader passes normal only.
- No light probes, no APV, no reflection use. Ambient is the trilight. `SampleSH` is used only when the look is off.
- No `LOD_FADE_CROSSFADE` keyword. No `MotionVectors` pass. The passes are ForwardLit, ShadowCaster, DepthOnly and
  DepthNormals.
- No derivative filtering of the brush noise.

### 1.3 The miners' materials (read in code)

All on `WGPainted` with `_Drawn = 1`. Typical numbers: `_Brush 0.2 to 0.3`, `_BrushScale 14`, `_Variation 0.1 to 0.12`,
`_Softness 0.38`, `_Translucency 0.12`, `_Gloss 0.08`. With the look's multiplier of 3, the brush scale is **42 dabs
per metre** on a miner. `_VertexColor` is 0, so vertex alpha is 1 and no AO is used.

`_Drawn = 1` writes -1 into the normals alpha. The Kuwahara paint filter then keeps beings at 15 percent paint
strength, so their few marks hold. The ink pass still draws around them. (read in code)

### 1.4 The settings (read in code, `PC_RPAsset.asset`, `PC_Renderer.asset`, volume asset)

| Setting | Value | Note |
|---|---|---|
| Rendering path | Forward+ | Many lights are cheap. |
| HDR | On, **32-bit buffer** | The 32-bit format is the smaller one. The docs say 64-bit avoids banding. (from a source, read in part) |
| Grading mode | **LDR**, LUT 32 | Grading runs after tonemapping. HDR mode grades before tonemapping. (from a source, read in part) |
| Tonemapping | Neutral | |
| Colour adjustments | contrast 6, saturation 8, exposure 0 | |
| Bloom | threshold 1.1, intensity 0.35, scatter 0.5, **high-quality filtering off**, 6 iterations | |
| Vignette | 0.16 (and the grain pass adds another) | |
| AA | SMAA High on the camera. MSAA off (`m_MSAA 1` is Disabled). | |
| Camera dithering | Not set in the setup script | |
| Upscaling | Auto, render scale 1 | |
| Main shadow | 2048, 4 cascades, 50 m, soft shadows High (7x7 tent) | Depth bias 0.1, normal bias 0.5. |
| Extra-light shadows | Supported, atlas 2048 | |
| SSAO | on; intensity 0.4, radius 0.3, medium samples, source depth-normals | |
| Light probes | Legacy system chosen (`m_LightProbeSystem 0`), none placed | APV is available but off. |
| Opaque texture | On, 2x bilinear downsample | A copy every frame. Not clear what uses it. |
| LOD cross-fade | On in the asset, blue-noise dithering | But see 1.2: the shader ignores it. |
| Skin weights | PC quality level: 4 bones. Mobile level: 2. Current level is PC. | |
| Anisotropic filtering | Forced on; textures set to 4 | |
| Texture mip bias | atlas -0.6, face -1.2, swatches -0.3 | Sharpens, but can shimmer. |
| GPU resident drawer | Off | |
| Cost | 4.5 to 5.7 ms GPU at 1080p on the dev laptop, 7.0 ms with 100 miners (measured, inventory) | Room to spend. |

### 1.5 Textures on the miners (read in code, `.meta` files)

- Atlas 2048, face 1024, painted swatches as small JPGs.
- Default platform: `textureCompression 2` (Compressed HQ). The Standalone override entry says 1, but its
  `overridden` flag is 0, so the override is **not** used. On desktop, HQ for RGB is BC7. So the colour is probably BC7
  already. (hypothesis from the flags; not confirmed in the editor)
- sRGB on, mip maps on, bilinear, aniso 4, no streaming.
- Face and atlas have negative mip bias. With SMAA and a 14 times 3 per metre procedural brush on top, distant miners
  may shimmer. (hypothesis)

### 1.6 What the pictures show (seen in a picture)

- `Miners_Dusk.jpg`: the house, roof, trees and path carry brush texture and tonal drift. The three miners are flatter.
  Coats and trousers are single colours with soft shade. The warm lamplight on them is a smooth glow. There is no wear,
  no stitching, no fold line. Faces read at this distance.
- `Miners_Polish_Closeups.jpg`: the light edge on skin and cloth is a fringed, comb-like saw edge. At close range it
  reads as noise more than as brush. The shade side of skin is a dull brownish grey. The boots, laces and apron are
  nicely drawn but their surface is a flat brown. Cloth has no seam.
- Reference B (`B_AFaceInWindowLight.jpg`): flat tonal planes, shade on skin is a warm rosy peach, shade on the dark
  coat is a deeper brown-black, dry-brush wear on the wood, a stitched grey patch on the coat, a worn linen cloth with
  stripes. Contour lines are dark and coloured, not black. Cloth folds are drawn as a few shapes.
- Reference E (`E_ExpressiveDrawnLife.jpg`): torn hems and patches on both coats, moss and lichen on stone and fence,
  a warm lit nose and cheek against a cool field, hair as a dark mass with a few stray strokes. A warm edge light picks
  out the figure on the door side.
- Reference F (`F_CosmicWonder.jpg`): the whole picture is brush texture, including the sky. Distance is simplified,
  not blurred.

---

## 2. Texturing

### 2.1 What the best stylised hand-painted work does, and what Claude can copy

Findings from sources (all weak to partial):
- Hand-painted art for games bakes the look into base colour, with brush marks left visible, often with only a normal map
  beside it. Artists also paint over a baked normal map to make it look painted. (search summary only, weak)
- Wear follows physics: exposed edges and bumps wear first, found with a curvature map. (search summary only, weak)
- A procedural-texturing article for hand-painted characters says the tools give non-destructive, reusable graphs, but it
  names real limits: no good way to make a **flow map** (stroke direction along the form), faces and complex areas still
  need a hand, and the results can feel less personal. (read in part)

What that means here:
- **Flow maps are not a limit for us.** Claude owns the mesh. For a limb, the stroke direction is the limb's axis. For
  a garment panel, it is the panel's grain direction, which the pattern-panel method in `modelling.md` section 7.2 already
  knows. A direction per UV island can be computed by script and used to lay strokes in UV space. (hypothesis)
- **Wear from curvature is doable headless.** Two routes: Cycles bake with a Pointiness or AO pass, or compute convexity
  per vertex with bmesh (mean of normal differences over edges), blur it on the surface, and write it as vertex data or
  into a texture. The bmesh route avoids bake settings and is stable in background mode. (hypothesis)
- **Faces stay painted strokes**, as today. See `faces_heads_hair.md` section 5.

### 2.2 The painted atlas: a layer plan (hypothesis; all of it needs judges and Luis)

Each layer is code-painted into the atlas, in UV space, from masks built on the mesh. Layers are saved apart and merged,
so a variant can swap one layer.

| Layer | What it paints | Why (picture) |
|---|---|---|
| Base planes | The flat colour and the hour-independent tone drift of each garment | Today's atlas |
| Grain strokes | Short dry strokes along the panel grain, light and dark, a few per cm, fading with the garment's fold depth | F and B: brush is everywhere |
| Seams and stitches | A dashed stitch line a few mm inside each panel border, a darker seam crease, a lighter piping on hems | B: stitched patch; the panel method knows the borders |
| Patches and mends | A few patches in a second cloth colour with a stitched border, placed by rules (elbow, knee, hip, never the face) | B and E |
| Wear | Light dry-brush on elbows, knees, cuffs, hem, collar edge, pocket mouth, shoulder strap. Darker grime at the hem and the sole. | E |
| Folds, painted | A few dark and light fold strokes at sleeve elbows, the coat's break at the hip, trouser knees. Placed from the pose sweep, not random. | B, E |
| Frayed hem | Short dark broken strokes at the hem line | E |
| Skin zones | A gentle warm band at cheeks, nose, ears, knuckles, elbows and knees; cooler at the temple and jaw. Masks from vertex groups. | E ruddy skin; `faces_heads_hair.md` 5.2 item 4 |
| Contact grime | Soft darker wash where cloth meets cloth and under the apron strap | Depth without AO |

Rules that keep it "a little stylised, reads natural":
- **Strokes, not noise.** Each mark is a tapered stroke with a hand wobble, as the face painter already does. The face
  painter's stroke code (tapered, Catmull-Rom, wobble) is the template. (read in code, per `faces_heads_hair.md`)
- **Few and placed.** Reference B reads as rich because every mark has a reason. Random speckle would read as a
  filter. (seen in a picture)
- **One light.** Do not paint form shadow into the albedo beyond a faint wash. The game's light already shades it, and
  two shadows look dirty (same warning as in `faces_heads_hair.md` 5.2).
- **Each miner has its own wear.** Small: clean, a mended knee. Long: thin elbows, a patched cuff. Round: a stained apron,
  a scuffed hem. This ties to "individual, worn surfaces" in the Visual Soul. (record)

### 2.3 Texel density and size

- Memory is not the limit. Three miners at 4096 with BC7 and mips come to about 21 MB each, about 64 MB in all. A crowd
  shares the three textures. (hypothesis from the format sizes)
- A rough count: a 2048 atlas, about 60 percent used, over about 2.5 to 3 square metres of clothed surface gives about
  900 texels per metre. A full-body shot at 1080p needs about 640 per metre. So the body is probably fine at game view
  and the head and hands are the weak part. The face share is already flagged in the inventory (M3). (hypothesis;
  measure per part in Blender by script before deciding)
- Proposal: measure texel density per part by script. Then either lift the atlas to 4096 or give the head and hands their
  own map and material. Both are switches that keep today's atlas. This matches `faces_heads_hair.md` 5.2 item 1.
- For the creator ("smaller scale, not smaller quality"): atlases per option are composited into one texture per miner at
  build time or at load, from the same layers. A texture array or a runtime blit is a Unity task for later. (hypothesis)

### 2.4 UVs

- `smart_project` makes many small islands and seams across painted areas (inventory M6). Seams matter more once stitches
  and strokes follow the grain. (hypothesis)
- Better: unwrap by garment panel (the panel method gives panels for free), limbs as long strips, head by its own
  projection. Seams placed where real seams are, and kept out of the face.
- Pad edges by at least 4 texels at 2048 so mips and Kuwahara never bleed colour across a seam.

### 2.5 Textures for the world (short)

The ground has no texture (vertex colours only, inventory W1). A painted ground detail layer would help close views.
A small tileable brush-stroke texture with a world-space triplanar lookup is the cheap fix. It is not a character issue,
so it is left for the world researcher. (hypothesis)

---

## 3. Material and shader models for "a little stylised, reads natural"

Luis's word is "a little stylized as long as it still reads natural visually" (record: the Nov 13 answers in
`GAME_VISION.md`). So: painted shapes, real light logic. No glossy plastic, no photoreal skin.

### 3.1 What stays

The one-band soft terminator, brush-broken edge, cool tinted shade, warm local pools, aerial fog. These are the loved
look. (read in code, record)

### 3.2 Proposed additions, each a keyword or a material switch beside today's

Every item is a `shader_feature` (a keyword) on `WGPainted`, off by default. A new look is then a material variant,
never an overwrite. Each needs a Unity render to judge. (hypothesis)

**A. Anchor and filter the brush on beings (S to M).** Today the dab field uses world position.
- Risk 1, swim: as a miner walks, the dab pattern stays in the world and slides over the body. The edge break is a
  strong term (0.5), so the light edge can "boil" over a moving body. This is the classic "shower-door" effect. Luis
  checks every frame in motion. (read in code; seen in motion: not yet)
- Risk 2, alias: 42 dabs per metre is about one dab per pixel at roughly 20 m. There is no derivative fade. (read in
  code; hypothesis on the size)
- Fix: for beings, take the dab coordinates from the bind-pose position (stored in a spare UV channel or a vertex
  colour at export) or from UV space, so the pattern rides on the body. Fade `_Brush` to 0.5 with `fwidth` of the
  coordinates, so it never aliases. The Kuwahara pass is also screen space and view dependent, which is a separate,
  standard effect.
- Test (cheap, headless in Unity later): render a miner at two world positions 0.5 m apart with the same pose and diff
  the two images in the body's own pixels. A swim shows as a nonzero difference in the terminator.

**B. Detail normal as terminator bend, not as realism (M).** A baked tangent-space normal map from folds and seams.
- Use it only to bend the light edge: `edge += (bump.x*l.x + bump.y*l.y) * k`. Folds then break the terminator into
  fold-shaped lines, as a painter draws them, and do not add specular. This keeps the painted look. Most of the visible
  gain comes from folds on sleeves, the coat's break at the hip, trouser knees. (hypothesis)
- Needs tangents. Unity can compute them on import (`importTangents`). Today's shader has none, so a tangent input and
  a keyword are needed.
- Bake: Cycles bake from a high-poly with folds, or from the cloth-baked fold shape (`modelling.md` 7.2). The manual says
  tangent space is the default and the right choice for animated meshes. A cage or a small ray distance is needed. A
  known report: a custom cage failed in Blender 2.80. It is old; check in 4.5. (search summary only)

**C. AO and wear from vertex data (S).** `s.occlusion = input.color.a` is already read when `_VertexColor` is 1.
- Bake AO (or the convexity from 2.1) to vertex alpha in Blender, turn `_VertexColor` on. No new code. Vertex AO on a
  16k-triangle mesh is fine for this look. `_DirectOcclusion` can also pull it into the direct light. (read in code)
- Caveat: vertex alpha is 8 bit and linear across triangles. For tight creases, a texture channel is better.
- Cycles can bake AO to vertex colours (`bake target = VERTEX_COLORS`). Reports of crashes are from 2.93. A bmesh
  version avoids the question. (search summary only, weak)

**D. Light-aware rim (S).** Today the rim is a constant cool colour that ignores the light and also shows on the shade
side. (read in code)
- Make it from two parts: a cool sky rim as today, and a **warm rim from local lights**: per additional light,
  `rimWarm += light.color * atten * pow(1 - n.v, p) * saturate(dot(n, -v_side))`-style terms, limited to the lit side.
  This puts a warm edge on the side of a miner that faces the house. It is Visual Soul principle 7, "light finds them",
  and it is what reference E shows on the figure by the door. The Team Fortress 2 illustrative-rendering work does a
  similar thing: rim limited to lit sides and fed from the ambient. (from a source, search summary only)
- This is the single cheapest large mood gain on the miners. (hypothesis)

**E. Warm bounce from lit ground (S).** At dusk the path in front of the house is warmly lit. A person standing on it
has warm light on chin, forearms and the underside of the coat. Today the ground ambient is a flat dark colour.
- Add, for each local light: `bounce = saturate(-n.y) * color * atten * 0.25`, tinted by the ground colour. It is
  physically true (lit ground reflects) and cheap. It is the "bounce" gap L4 in the inventory. (hypothesis)

**F. Skin: coloured wrap and a warm terminator (S).** In shade the skin goes grey-brown (seen in `Miners_Polish_Closeups`).
In B and E the shade on skin is warm, saturated.
- Add `_SkinWarm`: in the terminator band, shift the tint towards a warm red-orange instead of the cool shade tint, as
  GPU Gems' coloured wrap does for skin. (from a source, search summary only) Keep the cool tint for the outer shade so
  the world stays cool.
- Thin parts (ears, nose tip, fingers) get a little extra back-light through `_Translucency`, which exists.
- Do not use heavy screen-space skin SSS. It does not fit the look. (hypothesis)

**G. Cloth sheen (S).** A soft fuzz at grazing angles on wool and linen. The Charlie sheen model (Conty and Kulla, used
by glTF) is the standard cheap form. (from a source, search summary only) Here, a simplified version fits: sheen =
`pow(1 - n.v, k) * lightColor * (lit + 0.3)`, tinted by albedo, `_Sheen` 0 to 0.3. It overlaps with the warm rim (D), so
build D first and see if G is still needed. (hypothesis)

**H. Hair band (S).** A soft highlight band across the hair mass, from view and light direction, painted as a flat-edged
shape. This is the stylised form of the anisotropic highlight. (from a source, weak) The band's height comes from the
view-space normal's y. Hair on bones moves, so the band moves. Hair shape and strands are in `faces_heads_hair.md` 6.

**I. Coloured outline (S).** The hull colour is a constant dark brown. B and E draw contours in dark colours that
follow the local colour. Let the outline colour be the albedo darkened and slightly shifted warm. One new property.
(read in code; seen in a picture)

**J. Reflections: not for beings.** Glass, metal pick heads and lantern glass could take a reflection probe sample
(`GlossyEnvironmentReflection` is documented for custom shaders; read in part). Keep it for props, and use a heavily
blurred probe so it reads as a painted sheen. Gloss on cloth and skin stays off. (hypothesis)

### 3.3 Ranking inside the shader

Effort and mood: D, E, F first (small terms, large mood). A next (a likely bug). B and C when the baked detail exists.
G, H, I after. J last.

### 3.4 Shader keyword and variant care (read in code)

`WGPainted` already compiles many variants (`_MAIN_LIGHT_SHADOWS*`, `_ADDITIONAL_LIGHTS*`, `_CLUSTER_LIGHT_LOOP`,
`_LIGHT_COOKIES`, fog, instancing). Adding eight keywords multiplies them. Use `shader_feature_local` (stripped if the
material does not use it) and group the beings' extras under one `_WG_BEING_EXTRAS` keyword that the miners use and
the world does not. (hypothesis)

---

## 4. Lighting a character

### 4.1 Today (read in code, seen in a picture)

Key: the sun or moon. Fill: the flat trilight. Rim: constant cool. Local pools: seven hand-placed lights. Shadows: the
shadow map and SSAO. No probes, no contact shadow, no bounce.

### 4.2 The three-point idea, in this game's terms

| Role | Today | A better form (hypothesis) |
|---|---|---|
| Key | Sun or moon, warm at dusk | Keep. |
| Fill | Flat trilight | Keep as the default. Add the warm bounce (3.2 E) near the house. |
| Rim | Constant cool sky colour | Keep the cool sky rim. Add the light-aware warm rim (3.2 D). |
| Pool | Local lights, `pow 0.75` | Keep. They carry the mood. |
| Grounding | SSAO only | A foot grounding shadow (4.5). |

### 4.3 Probes: legacy light probes, APV, reflection probes

What is documented (read in part): a custom URP shader samples light probes with `SampleSH(normal)`, reflection probes
or the sky with `GlossyEnvironmentReflection`, and APV with `EvaluateAdaptiveProbeVolume(position, normal, view
direction, screen position, rendering layer, out diffuse)` after including `ProbeVolume.hlsl`. The Unity page lists no
keywords. A forum answer says `PROBE_VOLUMES_L1` or `_L2` must be defined before the include. The two disagree.
(search summary only for the forum; treat as unresolved; check the package source in the editor)

What APV offers in URP 17 and later (read in part): lighting-scenario blending, sky occlusion, disk streaming, and L1
or L2 spherical harmonics bands. The asset here has APV off. A forum thread reports APV did not work out of the box
with a custom skeletal shader. (search summary only, weak)

What this game needs from probes:
- Ambient colour that differs inside the house, under eaves and under trees. Today everything gets the same trilight.
  That is gap L1. (read in code)
- A day-night cycle. Baked probes are static in sun direction. APV scenario blending can blend baked scenarios (day,
  dusk, night). So three to four bakes would cover the time of day. (from a source, read in part; the quality of a
  blend between lit and unlit scenarios is a hypothesis)

Recommendation (hypothesis):
1. First, no bake: an **analytic ambient pool** around the house, a coloured term that grows near the windows and the
   open door and fades with distance, added to `WG_Ambient`. It is "bounce" without a bake and works with the time of
   day (it already has `_WG_GlowScale`). S to M.
2. Then, a trial of APV on the house interior only, three scenarios, as a switch beside the flat trilight. L, with
   real risk: Luis loves the flat dusk, and probes can make it look "lit by a renderer". Judge by stills first.
3. Legacy light probe groups are the cheapest probe path (`SampleSH` already exists in the code). But they bake one
   state, which does not fit a moving sun. Not recommended alone.
4. Reflection probes: only for props (3.2 J). One probe near the house. S.

### 4.4 Shadows

- A larger risk than the settings: **shadow bias on small bodies.** Depth bias 0.1 and normal bias 0.5 are the URP
  defaults scaled for typical meshes. On a 1.2 to 1.8 m figure, they can float shadows off the feet (peter-panning) and
  lose the nose, chin and collar shadows. (hypothesis; check on the face and feet in a Unity capture; Unity's own
  contact-shadow notes warn about the same bias trade-off, search summary only)
- Cascades: 4 cascades over 50 m with splits at 12, 29 and 54 percent. At game distance a miner is in cascade 2 or 3.
  Face detail shadows are coarse there. A tighter far split when the camera is close, or a dedicated higher-resolution
  shadow for the one focused miner, are options. (hypothesis)
- Soft shadows High is already the best URP filter (7x7 tent). (read in part)
- The sun and moon are the only shadow casters that matter for the beings. The house lights can cast shadows on the
  ground. `ShadowReach.cs` already tunes reach with the hour. (read in code)

### 4.5 Contact shadows and AO

URP has no true contact-shadow feature. HDRP does. The Screen Space Shadows feature in URP does not add them. (search
summary only) URP's SSAO is the nearest route. A newer SSAO, GTAO, is in the Unity 6.7 alpha and needs a scripting define.
It is not in 6000.6. (read in part) It is worth checking again after an upgrade.

For this look, the foot shadow should be painted, not physical:
- **Analytic grounding.** The game knows where each foot is (it is the articulation body). Pass an array of foot
  positions, heights and radii as a global. The ground shader darkens softly around each foot with a dab-shaped, brush-broken
  edge. Cost is small. It is tied to the real body, so it is not a faked animation, only a rendering aid. S to M.
  (hypothesis)
- Alternative: a URP decal. Decals need a decal renderer feature and per-foot objects. The analytic version is simpler.
- SSAO: radius 0.3 and intensity 0.4. It only touches ambient in this shader (the shader uses the "indirect" factor), so
  the direct light stays clean. That is right for this look. (read in code)
- Hand-placed lights: keep. They are cheap in Forward+.

### 4.6 Light on the world side (short)

The window glow, firefly and smoke code is the other half of "the pocket of warmth". Essence device 4 proposed a painted
glow around lamp light (record). A soft halo sprite or a low bloom threshold only for emissive windows gives that.
It belongs to the world researcher, but it affects how miners are read near the house. (hypothesis)

---

## 5. Image quality

### 5.1 Anti-aliasing options in Unity 6 URP (read in part, plus search summary)

| Option | Needs | Good | Bad here |
|---|---|---|---|
| SMAA (now) | Post pass | Sharp, no motion vectors | Does not fix shader aliasing or thin grass |
| FXAA | Post pass | Cheap | Soft |
| MSAA 2x, 4x, 8x | URP asset setting | Best on triangle edges; the miners' silhouettes and the house's lines; works with SMAA | Does not fix shader or texture aliasing; costs bandwidth; cannot be used with TAA |
| TAA | Motion vectors, history | Fixes shimmer and shader aliasing best | **Ghosting**; cannot be used with MSAA, camera stacking or dynamic resolution. Custom shaders need a MotionVectors pass |
| STP | Compute (SM 5.0); forces TAA; works at render scale 1 or lower | AA plus upscale | Same motion-vector needs as TAA; not with dynamic resolution |

(from a source: the Unity pages, read in part; STP details are search summary only)

What is special here:
- **The three custom full-screen passes.** Kuwahara runs before post-processing, ink also, and grain after. SMAA sits
  between. A TAA would see Kuwahara's output, which is spatially unstable under sub-pixel motion (a sector choice can
  flip). TAA could calm that, or it could smear it. Needs a test. (read in code; hypothesis on the effect)
- **Motion vectors.** `WGPainted`, `WGGrass`, `WGSky` and `WGOutline` have no motion-vector pass. With TAA or STP, miners
  and grass would be treated as static and ghost. Adding the pass to `WGPainted` and `WGOutline` is small. Grass uses
  `RenderMeshIndirect` with wind in the vertex shader. It needs its own previous-frame wind to give correct vectors.
  M. (read in code; hypothesis on the risk)
- **The ink pass** uses depth edges at 1.2 px, which already antialias slightly. Thin ink may crawl in motion
  (inventory P3, not measured). Measure before choosing.

Recommendation (hypothesis):
1. Keep SMAA as the default.
2. Test **MSAA 4x beside SMAA** (S). It is a one-field change. The cost on the dev laptop has room (4.5 to 5.7 ms). It
   cleans the miners' silhouettes and the outline hull, and leaves the painted look alone.
3. Test TAA or STP as a later switch, after motion-vector passes exist (M). STP at render scale 0.77 could also buy GPU
   time, but only if ghosting is acceptable.
4. A **supersampled capture mode** for judging: render at 2x and downsample, so judges and Luis see the best the assets
   can show, without aliasing hiding or inventing detail (S). Real play stays at 1.

### 5.2 HDR, tonemapping and grading

- **HDR buffer precision.** The docs say 32-bit is the default and 64-bit avoids banding at higher bandwidth. (read in
  part) Dusk has long smooth gradients (sky, fog, lamp falloff on plaster). Banding there is a likely flaw. Test 64-bit
  on the dusk and blue-hour captures. One field. S. (hypothesis)
- **Camera dithering.** URP has a per-camera dithering flag that adds 8-bit dither after tonemapping. It is not set
  in the setup script. The grain pass already works as a dither at its strength (`Grain` 0.6 in look E), but other looks
  and the stills do not have it. Turn it on. S. (read in code; hypothesis on need)
- **Grading mode.** LDR now (grade after tonemapping, limited range). HDR mode grades before tonemapping, with a
  log-encoded LUT. (from a source, read in part; forum notes that tonemapping then runs in the LUT pass, search summary
  only) HDR mode would change the look because the contrast 6 and saturation 8 act on different values. It is a switch to
  try, and Luis decides. S. (hypothesis)
- **LUT size.** 32 is the default and cannot be mixed. 64 is more precise. (read in part) Low value.
- **Tonemapper.** Neutral is a good, quiet choice for a painted look. ACES shifts hue and crushes colour. Keep Neutral.
  A custom tonemapping curve could be built into the Kuwahara pass, but it is not needed. (hypothesis)
- **Colour chosen per hour.** TheEssence's device 5: "every hour has its palette", shadows lean toward a shade hue
  and never go grey. (record) This is already partly done by `_WG_Shade` and the sky palette. What is missing is a
  grade that follows the hour. The stock **Split Toning** or **Shadows Midtones Highlights** overrides can be driven
  from `TimeOfDay`, with no LUT. S, a switch. It changes the look, so Luis decides. (hypothesis)

### 5.3 Bloom

- High-quality filtering is off. It is a one-field change that removes blocky or flickery bloom on small bright things
  (lantern flames, fireflies, window highlights). S. (hypothesis)
- Bloom threshold 1.1 means only values above 1.1 bloom. The window glow is emissive, so it blooms. Painted-glow
  alternatives are in 4.6.

### 5.4 Depth of field, for the cinematic view

- URP has Gaussian (cheap, far-field only) and Bokeh (near and far, with bokeh shapes on bright spots). (search
  summary only) Bokeh at dusk gives round "orbs" from lanterns and fireflies, which is in the spirit of reference F.
- For this look the important idea is **painted depth**, not optical blur. In a painting, far things are simplified, not
  blurred (F). The Kuwahara radius is a constant 4 px. Making the radius grow with depth (broader strokes far away) is a
  small shader change and is already TheEssence device 2: "strokes broader with distance". It gives separation in
  every view, not only the cinematic one. S to M. (read in code; hypothesis)
- Grain is after post, so it stays sharp over a blurred image. That is how paper grain should behave. (read in code)
- Recommendation: a **cinematic profile** (a second volume, active only in `ExploreFlight`) with Bokeh at a mild
  setting, high-quality bloom, and a higher paint radius. A switch. S. Luis said every view counts, the cinematic one
  too. (record: Nov 13 answers)

### 5.5 A better paint filter

- The current filter is a four-sector Kuwahara with a fixed radius. The **anisotropic Kuwahara** (Kyprianidis, Kang and
  Döllner, 2009) follows the local image structure with a structure tensor, so strokes lie along edges and forms.
  It is reported to run in real time on a GPU, and a later paper gives a cheaper weighting. (search summary only) This is
  TheEssence device 2 done properly: "the painting follows the forms". Cost: several passes (Sobel, blur, filter).
  M. (hypothesis)
- Flicker risk is real. Test at a still camera and at a slow pan.
- Beings stay at 15 percent paint. They already are "drawn". (read in code)

### 5.6 What not to add

Motion blur, chromatic aberration, lens dirt and film-photo effects fight a painted look. Screen-space reflections do not
exist in URP anyway. (hypothesis)

---

## 6. Skinning, blend shapes, GPU skinning, LODs

### 6.1 Skinning quality

- **Bones per vertex.** The PC quality level uses 4. Unity caps at 4 unless Blend Weights is Unlimited, and more than 4
  needs compute-shader skinning and costs speed. (search summary only) The Blender side should limit each vertex to 4
  and normalise. That is also in `modelling.md` 5.4. Keep 4.
- **Weights.** Quality of weights is mostly a modelling matter. See `modelling.md` 5.4 for the closed-body weight field,
  surface smoothing and garments taking the body's weights. Surface and shader side: the painted terminator exposes
  weight errors, because a candy-wrapper pinch lights differently from the rest. So a pose sweep with the real shader is
  the check, not Blender's viewport. (hypothesis)
- **Blend shapes.** Not imported today (read in code). They are cheap to author in Blender. In Unity they need the
  import toggle (`importBlendShapes`), a driver script, and a decision on cost. For three miners at about 16k triangles, the
  cost is small. The shapes must be driven by the body, not played as clips. For corrective shapes from joint angle see
  `modelling.md` 5.3. For faces, `faces_heads_hair.md` 7.
- **GPU skinning.** Unity does vertex skinning on the GPU by default on desktop. Blend shapes and more than 4
  bones need compute. (search summary only, weak) Not a problem at this scale. Check with 100 miners: 100 times 16k
  is 1.6 M triangles, so LODs matter there (6.2).
- **Normals after skinning.** Skinned normals are re-skinned. If the outline hull reads the same normal, hard-split
  normals can open the hull. See `modelling.md` 5.6.

### 6.2 LODs

- Setup today: three levels at 0.18, 0.035 and 0.004 of screen height, about 16000, 5000 and 1600 triangles. (read in code,
  record)
- **Fade mode is the default (None), so levels pop.** `MinerSetup.cs` makes the group without setting a fade mode. The
  URP asset has cross-fade on (blue-noise dither), but the shader has no `LOD_FADE_CROSSFADE` keyword and no
  `LODFadeCrossFade` call. So even a cross-fade group would not fade. (read in code)
- At 0.18 of the screen, a miner is about 195 pixels tall at 1080p. A pop there is visible. (hypothesis from arithmetic)
- **Fix (S):** add `multi_compile_fragment _ LOD_FADE_CROSSFADE` and the `LODFadeCrossFade` call to `ForwardLit`,
  `ShadowCaster` and `DepthNormals`, set the LOD group to cross-fade with animation, and keep the URP asset's blue-noise
  dither. The documented pattern is: the asset option, the group's fade mode, and a keyword in a custom shader.
  Unity strips the keyword from stock shaders, so a custom shader must declare it itself. (from a source, search summary
  only; Unity's own pages say the same in outline)
- Synergy: the Kuwahara filter smears a dither pattern by up to 4 px, which may hide the dither. Beings keep only 15
  percent paint, so some dither stays visible. Look in the capture. (hypothesis)
- Silhouette match across levels is the modelling side (`modelling.md` section 8). LOD1 and LOD2 were never looked at
  (inventory M7).
- The outline hull width is in pixels, so it keeps the same line across LODs. Good. (read in code)
- Texture and material stay the same across levels, so no texture pop.

---

## 7. What Unity 6000.6 URP offers (and what is in use)

| Feature | Status here | Worth it for this game |
|---|---|---|
| **Render Graph** | In use: `LookPostEffects` records its passes in it (read in code) | Yes. New passes (anisotropic Kuwahara, grounding, a capture-quality blit) go there. The Render Graph Viewer helps debug. |
| **Forward+** | In use | Yes. Many local lights are cheap, so more window and lantern lights are free. |
| **STP** | Off | Maybe. AA plus upscale. Needs motion vectors. See 5.1. |
| **TAA** | Off | Maybe. See 5.1. |
| **Adaptive Probe Volumes** | Off | A trial on the house interior. See 4.3. |
| **Camera history API** | Not used | Useful for temporal stability of Kuwahara or ink. (from a source, read in part) |
| **GPU Resident Drawer** | Off | Only when world dressing grows large. Custom shaders need DOTS instancing support. Not for skinned beings. (hypothesis) |
| **GPU occlusion culling** | Off | Same. |
| **Decals** | Not used | Possible for wear on ground and walls. Needs the decal feature. |
| **Screen Space Lens Flare / data-driven flares** | Supported in the asset, not used | Not for this look. |
| **Colour Lookup, Split Toning, SMH, Lift-Gamma-Gain** | Not used | Yes for hour grading (5.2). |
| **GTAO** | Not in 6000.6 (alpha for 6.7, behind a define) | Watch. Better contact darkening. (from a source, read in part) |
| **8192 shadow maps** | Not used | Only if shadow detail on faces needs it. |
| **Volume framework** | In use (one global volume) | Add a cinematic volume and per-zone volumes (the house interior). |
| **Foveated, VR, mobile tiling** | Not relevant | |

I could not confirm a Deferred+ path or a "what's new" list for 6.2 to 6.6. The "what's new" page I read covers
URP 17 (Unity 6.0) and points to the general "What's new in Unity" pages for later versions. Check those before
relying on any later feature. (read in part)

---

## 8. The look Luis loves, and what protects it

### 8.1 What the references and his words say

- Luis's favourite frame is the lit house at dusk (19:12), seen close and low from the path, tall dark grass in front.
  (record: `TheEssence.md`, October 2) He rejected the vast, bright vistas pass and kept the intimate dusk. So when the task
  says "an intimate lit house at dusk over vast bright vistas", the intimate dusk is the loved look. The vistas, water and
  clouds are on the archived branch and are for his request only. (record)
- What moves him: intimacy, an ordinary moment, warm light in a cool lilac dusk, a soft brushed sky, enclosure by dark
  hills, a tonal palette. (record: memory note on his visual taste)
- What the people references teach about surfaces: shade is colour, not black (B: warm peach on skin, deep brown on the
  coat; E: lilac and blue-green); wear and mends are drawn; contour is coloured; a few strokes carry the form.
  (seen in a picture)
- "Surfaces feel alive. Clothing, wood, plaster, grass and skin should have individual visual character." (record,
  Visual Soul)

### 8.2 Protect list (never change by default)

1. The soft, brush-broken terminator on the world. (The miners' version may be cleaned up, as a switch.)
2. Cool tinted shade with kept saturation.
3. Warm local pools, `pow 0.75`.
4. The flat trilight ambient as the default.
5. Kuwahara, ink and grain as the screen look, with beings kept "drawn".
6. Fog warm toward the sun.
7. The sky.

### 8.3 What would break the loved look

- Glossy PBR on skin, cloth or wood. (J is limited to props, blurred.)
- Strong normal-mapped detail with specular. (B uses the map only to bend the edge.)
- Baked probes that flatten the pool-of-warmth contrast. (4.3: the trial is interior only.)
- Any grade that turns dusk bright. (Look E is "30 percent darker than the daylight references", and that is on purpose;
  record: `TheEssence.md`)
- TAA smear on the paint strokes. (5.1: test first.)
- Dense random surface noise. (2.2: few and placed.)

### 8.4 The frame test, used as the judge for every change

`TheEssence.md` gives five checks: light you can read, air in three layers, one warm accent of life, a calm area for the
eye, no 3D giveaways (faceted silhouettes, plastic highlights, flat untextured areas, shimmering edges). (record) The
miners' flat cloth fails "no flat untextured areas" today. The swim risk in 3.2 A is a "shimmering edge" risk.

### 8.5 The pocket and the vista, as two looks (optional)

The miners should read in both the intimate dusk and, if Luis ever wants it, a bright vista. Surface work done once
(layers, baked detail, shader terms) serves both. Only the light changes. (hypothesis)

---

## 9. What the method needs from Claude's real tools, and how hard it is

### 9.1 The loop

Build by script, render, read the picture, judge faults and beauty, fix, render again. Then aesthetic judge agents. Then
Luis. This is `ModelQualityMethod.md` applied to surfaces. (record)

### 9.2 Tool by tool

| Work | Tool | Reach | Notes |
|---|---|---|---|
| Layer painting for atlases (strokes, stitches, wear, zones) | Python with NumPy, OpenCV, Pillow; mesh masks from Blender | **Well** for placed, stroke-based marks; **partly** for beauty | No brush. The stroke code from the face painter is the base. |
| Flow direction per island, curvature, convexity, AO to vertex | Blender 4.5 by Python, command line | **Well** | bmesh gives these without bake quirks. |
| Normal, AO, curvature bakes | Blender Cycles (OPTIX) headless | **Well** after one working test | Cage and ray-distance care. Tangent space. Not tried yet. |
| Cloth fold shapes | Blender cloth bake, headless | **Partly** | Folds should read as painted. Pairs with `modelling.md` 7. |
| Shader code (new terms, keywords, LOD fade, motion-vector pass) | Unity shaders as text | **Well** | Result seen only in a Unity run. |
| Full-screen passes (depth-aware paint, anisotropic Kuwahara, grounding) | C# render-graph code plus HLSL | **Well** | Pattern already exists in `LookPostEffects.cs`. |
| URP asset and volume changes | Editor scripts | **Well** | Setting values is easy. Judging them needs captures. |
| Probe bake (APV) | Unity editor script and batch bake | **Partly** | Scripted bake is plausible. The custom shader must sample it. Unverified. |
| Judging beauty | Claude reads pictures | **Weaker than for faults** | Aesthetic judges and Luis decide. |
| Photo or painted source material | None | **Not** | No image generator. No downloads without approval. |
| A hand-painted texture from scratch by an artist | None | **Not** | The result is code-painted, so it is more regular than a hand. Stroke variation and placement rules are the countermeasure. |

### 9.3 How hard is top quality this way?

| Target | Hard? | Why |
|---|---|---|
| Shader terms that carry mood (D, E, F, I) | Easy to moderate | Small code. Judged in captures. |
| Clean, anchored paint on a moving body (A) | Moderate | Needs a bind-pose coordinate and a motion check. |
| Stitches, seams, patches, wear (2.2) | Moderate | Placement rules and stroke quality need many judged loops. |
| Painted folds that read natural | **Hard** | They must look drawn and also be right for the pose. |
| Skin that is warm and alive, not dirty | Moderate to hard | Faint colour zones; two shadows look dirty. |
| Probe lighting that keeps the loved look | **Hard** | The danger is the look, not the code. |
| AA that is calm and keeps the paint | Moderate | TAA or STP need motion vectors and tests. |
| A painted filter that follows form without flicker | Moderate to hard | Needs stills and a pan test. |

### 9.4 Constraints for this run

- Unity and Blender were not run now. Every claim about how something looks in the engine is a hypothesis until a Unity
  capture exists. The Blender EEVEE look is not the Unity look. (record)
- A way to look at the texture work without Unity: view the atlas flat, and a Blender toon replica for the light edge. It
  is only a preview.
- A Python mock of `WG_Direct` over a baked normal map could preview the terminator in NumPy. It would never
  replace the Unity capture. (hypothesis; not built)

---

## 10. Ranked quality gains

Ranked by how much quality they raise for the effort. Each is a switch beside today's look, and each goes to aesthetic
judges and then Luis before it becomes the default. (hypothesis for all sizes and ranks)

| Rank | Gain | Size | Why here |
|---|---|---|---|
| 1 | **Light-aware warm rim and warm bounce on beings** (3.2 D and E) | S | Two small shader terms make "warm light finds them" true. Biggest mood per hour of work. |
| 2 | **Anchor and filter the brush on beings** (3.2 A), after a two-position diff test | S to M | Probable swim and alias on every moving miner. Luis checks every frame. |
| 3 | **LOD cross-fade wired** (6.2) | S | The shader ignores it today. Fix is mechanical. |
| 4 | **Painted layers on the atlas: seams, stitches, patches, wear, zones** (2.2) | M | Closes the largest surface gap with the references B and E. |
| 5 | **Skin warm terminator and colour zones** (3.2 F, 2.2) | S | Today's shade on skin is grey-brown. References show warm. |
| 6 | **Vertex AO and wear from bmesh** (3.2 C) | S | No shader change. Depth where cloth meets cloth. |
| 7 | **Image checks: 64-bit HDR, camera dithering, bloom HQ filtering** (5.2, 5.3) | S | Three fields. Likely banding fix at dusk. |
| 8 | **Supersampled capture mode** (5.1) | S | Lets judges and Luis see the best of the assets. |
| 9 | **MSAA 4x beside SMAA, test** (5.1) | S | One field. Cleans silhouettes and the outline hull. |
| 10 | **Foot grounding shadow, analytic** (4.5) | S to M | Contact is the main missing cue. Tied to the real feet. |
| 11 | **Depth-aware paint radius, then an anisotropic Kuwahara** (5.4, 5.5) | S to M | "Strokes broader with distance." Painted depth for all views. |
| 12 | **Texel-density audit, then a 4096 atlas or separate head and hand maps** (2.3) | S to M | Face share is the weak part. Memory is no issue. |
| 13 | **Baked fold normal as terminator bend, plus a tangent input** (3.2 B) | M | The folds the references show. Needs a bake path tested. |
| 14 | **Coloured outline from albedo** (3.2 I) | S | B and E have coloured lines. |
| 15 | **Cinematic profile: Bokeh DoF, HQ bloom, higher paint radius** (5.4) | S | Cheap, view-limited. |
| 16 | **Hour-keyed split toning or SMH** (5.2) | S | A grade that follows the hour. Changes the look: ask Luis. |
| 17 | **Hair band and cloth sheen** (3.2 G, H) | S each | After the rim is judged. |
| 18 | **Shadow bias and cascade check on small bodies** (4.4) | S | A possible peter-pan on feet and face. A capture settles it. |
| 19 | **TAA or STP with motion-vector passes** (5.1) | M | Risk of ghosting and smeared paint. Test as a switch. |
| 20 | **Analytic ambient pool around the house** (4.3) | S to M | "Bounce" with no bake. |
| 21 | **APV trial on the house interior, three scenarios** (4.3) | L | Biggest light system and the riskiest to the loved look. |
| 22 | **Reflection probe for props only** (3.2 J) | S | Small. |

---

## 11. What was not done

- Unity and Blender were **not run**. All claims about the engine are from files, settings and pictures.
- The **texture format** (BC7 or not) was inferred from `.meta` flags, not confirmed in the editor.
- The **swim**, **shimmer** and **LOD pop** were derived from code and arithmetic. No motion capture or two-position diff
  was made.
- **LOD1 and LOD2** were not looked at.
- **Texel density** was estimated by area. It was not measured on the meshes.
- **APV with the custom shader**: the sources disagree on the keywords. Not tested.
- **Cycles bake of normal, AO, curvature** and **vertex AO bake** for these meshes: not tried. Search summaries only.
- **TAA and STP interaction with the three custom passes**: not tried.
- **The `TimeOfDay` palettes** were read only for dusk. The other hours were not checked against the grade ideas.
- **Reference pictures A, C and D** were not re-read for this file. B, E and F were. A, C and D are described in
  `VisualSoul.md`.
- **No page was read in full.** Several Unity pages were fetched through a summariser, the rest are search summaries.
  The Team Fortress 2 paper was reached only as a binary file and not read. Its content here is search summary only.
- **No competitor games** were studied for their pipelines beyond what the searches returned. That belongs to the
  outside-research files.
- **No download** of software, models, textures, fonts or HDRIs. Anything of that kind would need Luis's approval.
  Nothing in this plan needs one: a code-painted texture, a code-built LUT and shader text are all made here.
- Nothing was committed. No file outside this folder was written.

---

## 12. Sources

Unity pages (fetched through a summariser; read in part):
- URP anti-aliasing, Unity manual (current): FXAA, SMAA, TAA, MSAA, their needs and limits.
- URP Asset reference, Unity manual (current): HDR precision, grading mode, LUT size, upscaling filters including STP,
  LOD cross-fade and dithering, SH evaluation, light probe system, soft shadow quality, opaque texture, MSAA.
- Custom shaders in URP, indirect lighting page (current): `SampleSH`, `GlossyEnvironmentReflection`,
  `EvaluateAdaptiveProbeVolume`.
- What's new in URP 17 (Unity 6.0): Render Graph, GPU resident drawer, STP, APV improvements, camera history API.

Forum thread (read in part): URP GTAO preview for Unity 6.7 alpha (define, modes, limits).

Article (read in part): a Game Developer piece on procedural texturing for hand-painted stylised characters (limits:
flow maps, faces, personal feel).

Search summary only (no page read): Unity STP pages; LOD cross-fade pattern for custom URP shaders (forums, a Japanese
shader notes page); skin weights and GPU skinning notes; depth of field Gaussian and Bokeh docs; colour grading order
and banding (forum, issue tracker); APV custom-shader keywords (forums); anisotropic Kuwahara (Kyprianidis and others);
GPU Gems chapter 16 on wrap lighting; Team Fortress 2 illustrative rendering notes (rim and light warping); Charlie sheen
(Conty and Kulla, glTF sheen); Blender baking manual and vertex-colour bake threads; contact shadow notes for URP.

Weak: forum and polycount posts on hand-painted workflows; the skin-weights quality notes; the forum answer on
`PROBE_VOLUMES_L1`.

Project files read: `WGPainted.shader`, `WGCommon.hlsl`, `WGLookPost.shader`, `WGOutline.shader` (top),
`LookPostEffects.cs` (top), `TimeOfDay.cs` (dusk palette and globals), `LookDevControls.cs` (candidates),
`PC_RPAsset.asset`, `PC_Renderer.asset`, `OrdinaryPlaceVolume.asset`, `QualitySettings.asset`, miner `.mat` and `.meta`
files, `MinerSetup.cs` (LOD setup), `OrdinaryPlaceSetup.cs` (camera AA). Documents read: `inventory.md`,
`VisualSoul.md`, `TheEssence.md`, parts of `faces_heads_hair.md` and `modelling.md`, the Nov 14 answers. Pictures read:
`B_AFaceInWindowLight`, `E_ExpressiveDrawnLife`, `F_CosmicWonder`, `Miners_Dusk`, `Miners_Polish_Closeups`.
