# The best visual quality: the programme

November 15, 2026. Part two of the deep dive Luis asked for on November 14 (night). This is the one programme for the
best visual quality that the next work rests on. It is a synthesis, revised once after a sceptic and completeness
critic (`critique.md` in this folder; section 10 says what was done with each point). It rests on the five files of
part one in this folder and on the models research:

- `inventory.md` (here called **inv**): every visual part today and Claude's real tools;
- `modelling.md` (**mod**): bodies, topology, joints, garments, LODs, the creator's base;
- `faces_heads_hair.md` (**faces**);
- `surfaces_light_render.md` (**surf**);
- `world_and_props.md` (**world**);
- `Docs/Research/2026-11-14_TheModelsRedrawn/research.md` (**MR**): for this revision its "Start here" and sections 1,
  2, 3, 4, 5 and 7 to 9 were read.

Nothing was run in Unity or Blender for this file. Nothing was committed. No web search was made. Every claim from
part one keeps the evidence its file gave it. Section numbers after a file name point into that file (for example
"surf 3.2 D").

**Nothing here is a decision.** Luis decides. Every Open question stays Open: proportions, heads, costumes, vents, the
cut-away practice, the closed body under the clothes, the creator's options, the UI language, water and the far land.
Every gain below is an additive, switchable option beside the look he loves, off by default.

**How each claim is marked:**
- **(measured)**: a number taken from a mesh, a texture or a game run (by part one or by a project log);
- **(code)**: read in the code or in a settings file;
- **(picture)**: seen in a picture in the project;
- **(source)**: from an outside source, as tagged in the part-one file named beside it; "search summary" means part one
  saw only a search engine's summary, the weakest kind;
- **(record)**: from a project document or Luis's words;
- **(hypothesis)**: an inference, not checked.
Where one claim carries two marks, the first mark is for the fact and the last for the inference drawn from it.

**Sizes** (all hypotheses). This file counts in **sessions**: one agent working inside one five-hour usage window.
**S** = one session or less; **S to M** = one to three; **M** = two to four; **L** = five to eight. Part one used two
scales. Modelling's "S a session or less" maps directly. The inventory's and faces' "S under a day, M a few days, L a
week or more" were mapped by reading a working day as one to two sessions; so their M (a few days) is read as two to
four sessions here, and their L as five to eight or more. The totals in 5.3 are ranges for that reason.

---

## Start here

**The biggest gaps** (ranked in 2.2):
1. The miners' bodies and cloth: pinched waists, floating legs, cloth as boards, tube limbs (MR). Step 0's subject.
2. How the loved faces and the hair are carried: blotchy light on the cheeks, the face given about half the painted
   face's linear resolution (Small only, rough), faceted hair. Every face gains without one mark changed.
3. The miners' flat surfaces: clean cloth in a brushed world, with no seams, wear or mends.
4. Light that never finds the miners warmly, and nothing grounding their feet.
5. Likely motion faults on every moving miner (the brush may swim on the body; LODs pop). Read in code, never watched.
6. Then the world's sparse dressing and faceted trees, no work VFX, a stock UI. Hands, boots and necks were missing
   from part two's first draft; they are now a part of their own (2.1, G42).

**The first steps** (section 5):
1. Now, while the energy count's long run holds Unity: light work only. The judges' brief; step 0's Blender code behind
   flags that are off; the tools made repeatable.
2. The energy count is read first, as Luis approved.
3. Step 0: Blender stills of each miner three ways (today's, look only, fuller body), in the idle's arm pose, with
   separate sheets for landmarks, faces (F0 carrier, F1 prettier drawings), heads (H), and a close-up row of hands,
   boots and necks; one extra "dressed" row so the depth is not chosen on the flattest rendering; blind aesthetic
   judges, then Luis.
4. Beside it, in other sessions: the Unity shot set and the cheap facts (texture format, swim test, LOD pictures, CPU
   and crowd cost), then a handful of small shader switches on today's miners.
5. While Luis looks: the template spike (the riskiest work) first, the world's first pass after it.

**The questions only Luis can answer** are in section 8: twelve, among them the order; how much to look at in step 0;
three rulings on "no fake animations" (a body-driven blink, pose-driven garment shapes, joint-angle correctives and
helper bones); whether any screen-wide switch is unwanted; a performance ceiling and the target hardware; whether the
drawn body may carry anatomy the colliders do not; and where the programme may stop.

**The size, honestly:** about 80 to 155 sessions through tier 3, judges included; at about seven sessions a week,
roughly three to five months. There are three cut lines where it can stop and still be coherent (5.3). A first improved
miner (the "carrier" version on today's bodies) could reach the game after about 10 to 17 sessions.

---

## 1. What Luis asked, and what this answers

### 1.1 His words (November 14, night; record: `Docs/Correspondence/2026-11-14_THE_MODELS_PROPOSAL_ANSWERED.md`)

- The order: "It's fine to continue with your order." (the energy count first, then step 0's pictures)
- The faces: "I actually like the current faces", and he would like Claude "to explore if there's ways to make them
  even more 'pretty' as according to our aesthetic".
- The heads: "They can try a change honestly."
- The deep dive: "a super deep dive research of what is the best way of doing the models and all the visual parts
  that you are capable of to have best quality possible".
- The bar: "maximize quality as much as we can on all aspects of the game with no efforts saved".
- The habit: "start thinking more proactively on ways to improve quality whenever possible".

And from November 14, evening (record: MR 1.1): the legs must "seem connected to the body"; the waist is "too
extreme"; "make all the models much more pretty in general"; "smaller scale, not smaller quality".

### 1.2 What this file answers

1. Where quality stands in every visual part, and the gaps ranked; where part one disagrees, which reading is right
   (section 2).
2. The best method for each part within Claude's real tools, with each method's weakest link (section 3).
3. One ranked programme across all parts, each gain with its size, dependencies, switch, visibility at the Strategy
   camera, and need for Luis (section 4).
4. The order of work, reconciled with the models research's step 0, with the total size and the cut lines (section 5).
5. How each gain is checked (section 6), the risks (section 7), Luis's questions (section 8), what was not done
   (section 9), and the critic's points (section 10).

It does not choose a look. It does not start a build. Looks are made, judged by aesthetic judges, then shown to Luis.

---

## 2. Where quality stands, part by part

### 2.1 Part by part

Verdict words: **strong** (keep, small gains), **medium** (good base, clear gains), **weak** (the gap shows to the eye),
**not looked at** (no picture read for it).

| Part | Today | Verdict | The main gap | Evidence |
|---|---|---|---|---|
| Miner bodies | Skin-modifier tubes and metaballs, voxel-remeshed garments, collapse-decimated to a 16,000-triangle LOD0; capsule-distance weights | **Weak** | Pinched waist, floating legs, cloth as four rigid plates, tube limbs, pipe knees, flat profiles | code (mod 2); measured and picture (MR 2) |
| Topology and joints | No edge loops survive the decimate; four-bone linear skinning; no helpers, no correctives; blend shapes off at import | **Weak** | Knees bend like a hose; elbows are arcs; toon bands may zigzag on random triangles | code (mod 2, 5.1); picture (mod 2); the band zigzag is a hypothesis (mod 4) |
| Garments | Shells fused by a voxel remesh; four single-bone skirt flaps, physical | **Weak** | Thigh through the coat, tents, fins, the sack on all fours | measured and picture (MR 2.3) |
| Hands | Metaball clusters kept as one piece; a 1,600-triangle budget inside LOD0 with finger bones; `whole_hand` merges the fingers at LOD2; the grips carry props | **Not looked at closely; kept** | "Pretty faces and hands are the hard part" (inv 0.6). Round's wrists are thin against her hammer (MR 2.5). The models research keeps the hands and grips exactly where they are (MR 7.2). Their texel share is unmeasured | code (mod 2); picture (MR 2.5); record (inv 0.6, MR 7.2) |
| Boots and feet | Voxel-remeshed garment shells, laced; a box sole collider in the physics | **Medium; on the keep list** | Large black slab soles in profile and kneeling, read by the models critic, "to be looked at again" (MR 2.6) | code (inv, the garments item of the miners' list; `OwnBody.cs` sole box); picture, critic's read (MR 2.6) |
| Ears and necks | Ears from the head's metaballs; necks join under collars | **Weak (necks), not seen (ears)** | Round's head sits almost on her shoulders; Small's collar swallows her neck; Luis said on October 3 heads looked glued. Ears are hidden under hair or too small | picture, critic's read; record (MR 2.8); picture (faces 2.4 item 14) |
| LODs | About 16,000, 5,000 and 1,600 triangles in all by decimation; no fade, so levels likely pop | **Unknown** | LOD1 and LOD2 never looked at; the pop is likely | code (surf 6.2; mod 8); record (inv M7) |
| Faces | Few confident code-painted strokes per miner; front projection; baked into the 2048 atlas | **Medium, loved** | Blotchy light from the metaball surface; the face keeps about half of the painted face's linear resolution (about a quarter by area), Small only, rough; a stray eye fragment on a second island; skin, hair and cloth share one material | measured, rough, and picture (faces 2.4) |
| Heads | Metaball skull. Heads tall with hair: about 4.5, 5.8 and 4.1 (Small, Long, Round), read from silhouettes by eye. Skull only: about 4.9, 6.5 and 5.5, from the preset numbers. Round's two figures differ most because her curl buns add height (faces 3.1) | **Medium** | At the Strategy camera a head is about 10 px and only its size and hair read | picture, by eye; code, presets (faces 3.1, 2.5) |
| Hair | Tube locks with 8 sides over the skull, a cap that closes the gaps, and metaball curl clumps; decimated with the body; skinned to the head, no motion | **Weak up close** | Flat facets (a "3D giveaway"), a saw-edge fringe, no flyaways, no sheen | code and picture (faces 6.1, 2.4) |
| Miner surfaces | Flat painted albedo; no wear, seams, stitches or folds; no normal or AO data; Small's trousers almost bare | **Weak** | Clean flat cloth in a brushed world; references B and E show patches, mends, worn hems | picture (surf 1.6; MR 2.8) |
| Shading model | `WGPainted`: one soft band broken by a world-space brush field, cool tinted shade, warm pools, constant cool rim, trilight ambient | **Strong (world), medium (miners)** | On miners: the rim ignores the lights, skin shade goes grey-brown, the brush may swim on a moving body and alias at distance | code (surf 1.1, 3.2 A); the swim is a hypothesis |
| Light | Seven hand-placed lights, trilight ambient, shadow map, SSAO; no probes, no bake, no bounce, no foot contact | **Strong mood, thin on the miners** | "Warm light finds them" is not true on the miners; no grounding at the feet; Small's face washes pale in daylight | code (inv 1.3, surf 4.1); record (MR 2.8) |
| Image | Forward+, HDR 32-bit, SMAA, Neutral tonemap, LDR grading, bloom without its high-quality filter, Kuwahara, ink and grain passes | **Strong look, cheap precision gains left** | Possible banding at dusk; LOD pop; untested crawl of thin ink | code (surf 1.4, 5) |
| Sky | Painted gradient, bands, sun, moon, stars, galaxy, painted clouds lit in bands | **Strong** | Little; keep. Untested from the Strategy camera (world S2) | code and picture (world 5.1) |
| Ground and meadow | One height field to 1500 m; vertex colour, shader brush; about 690,000 grass blades with wind and parting | **Strong near-field grass, medium ground** | Path edges stair-step; ground flat up close; blades even "green swords"; no trace where bodies walk | code (world 3.1); picture; blade count computed (world 13) |
| Far land | Hills and ranges in the height field | **Weak** | One flat grey-blue mass, no layers of air, no tree or field patches | picture (world 3.1) |
| Vegetation | Three tree models, two bushes, four rocks, no flowers; whole-object sway | **Weak** | Faceted lobes; repetition; the frame test bars "faceted silhouettes" | code and picture (world 4.1); record (TheEssence frame test) |
| House | One careful script; painted baked textures with cavities, edge wear, moss | **Strong, Luis's favourite frame** | Close up: even shingles, crisp corners, plain door, flat doorstep stones | code and picture (world 2.1) |
| Other buildings | Built-in cubes | **Placeholder** | The RTS will need many buildings in one family | code (world 2.1) |
| Props | Carried props carefully built and physical (lantern, mug, satchel, pickaxe, hammer); house props low-poly | **Strong (carried), medium (house)** | No wear bake on carried props; pickaxe at 512; metal reads flat orange | code and picture (world 1.1) |
| VFX | Fireflies, breath, smoke, window glow, each tied to a real cause | **Medium, sparse** | No chips, dust, sparks or footfall: work leaves no trace | code (world 7.1) |
| Cameras | Strategy and Explore; bloom, tonemap, vignette; no depth of field | **Medium** | No named shot set to judge on; no focus in the cinematic view; the frame test never checked at 42 m (world C3, T7) | code (world 8.1) |
| UI | All IMGUI, default skin and font | **Placeholder** | Does not look like the game at all | picture and code (world 9.1) |

### 2.2 The biggest gaps, ranked

Ranked by how much each harms "pretty and natural" in the views Luis counts (all views, the cinematic ones too;
record: memory, November 13). The Strategy camera is the RTS's main view; where a gap shows there is said.

1. **The miners' bodies and cloth** (MR's ranked faults 1 to 7). The largest gap. Already researched; it is step 0's
   subject. At the Strategy camera it shows as silhouette (the waist, the legs, the board skirts). (measured, picture:
   MR 2)
2. **The faces and hair as they are carried.** The drawing is loved; the carrier loses it: blotchy cheek light, a soft
   face, faceted hair. Cheap to fix, and every face gains without one mark changed. Close views only; at the Strategy
   camera only the head's size and hair shape read. (measured, picture: faces 2.4, 2.5)
3. **The miners' flat surfaces.** The frame test bars "flat untextured areas"; the miners fail it while the house
   passes. Close and conversation views. (picture: surf 1.6, 8.4)
4. **Light that does not find the miners.** A constant cool rim, grey-brown skin shade, no warm bounce from the lit
   path, no foot contact. The dusk mood stops at the miners' edges. The grounding shows in every view. (code, picture:
   surf 1.1, 1.6, 3.2 D to F)
5. **Likely motion faults: brush swim and alias, LOD pop.** Read in code, never watched. Luis reads every frame, so
   if real they are severe; if not, they cost nothing. They rank here because the check is cheap. Alias and LOD pop
   are worst at the Strategy camera's distances. (code; hypothesis on the effect: surf 3.2 A, 6.2)
6. **Hands, boots and necks.** What the eye meets in every work close-up. Hands and grips are kept as they are by the
   models research; boots and necks were flagged "to be looked at again". Close views only. (record: inv 0.6; picture,
   critic's read: MR 2.6, 2.8)
7. **The world's dressing:** three trees, two bushes, four rocks, no flowers; faceted crowns; a flat far land; a
   stair-stepped path. Seen in every view, the Strategy view most. (code, picture: world 3, 4)
8. **No work VFX.** The game is about work, and work leaves no chip, dust or puff. (code: world 7.1)
9. **The UI.** Stock IMGUI. It matters at the final prototype, not for today's look. (picture: world 9.1)
10. **Buildings other than the house are cubes.** It matters when the RTS grows. (code: world 2.1)

### 2.3 Where the five files disagree, and which is right

| Question | One reading | The other | Which is right, and why | Consequence |
|---|---|---|---|---|
| **The miners' texture format** | inv 2.1 M5: "probably DXT1", ranked first as "texture hygiene" | faces 2.1 and surf 1.5: `MinerSetup.cs` sets Compressed HQ; the Standalone override entry has `overridden` 0, so the default applies, which is usually BC7 on desktop | **faces and surf, on the reading of the code and the import file.** The editor was not asked. (code; hypothesis on BC7) | Inv's first gain shrinks to a check in the editor (G0c). The face's softness most likely comes from its small share of the atlas (hypothesis; the share is measured roughly, Small only: faces 2.4 item 3) |
| **What the hair is** | inv 1.7: a voxel-remeshed cap | faces 6.1: tube locks with 8 sides over the skull, a cap closing the gaps, metaball curl clumps, all decimated with the whole miner | **faces, with the inventory partly right** about the cap. Faces read `hair.py` and `rigging.py`. (code) | The fix keeps the mesh-lock method: more sides and smooth normals, and the locks given their own triangle budget at LOD0 (G3), not cards |
| **How sparse the world is** | inv 1.9 and 2.4: "no water, no clouds, no far land" in the main scene; props on flat albedo, no wear | world 0: the sky draws painted clouds; the ground rises to hills and ranges to 1500 m; the house and trees carry baked painted textures with cavity, wear and moss; carried props are the best-made things in the game | **world**, on every point but one. It read `WGSky.shader`, `OrdinaryGround.cs`, `painting.py` and saw the clouds in the dusk pictures. What lives only on the archived branch is the 3D cumulus, the lake and planar reflections. The inventory is right that the **carried** props have no wear bake (world P1 agrees). (code, picture) | The world is **sparse in its dressing** (counts read in code: 3 tree models, 2 bushes, 4 rocks, no flowers) and **flat in its far land** (picture), **not in its big layers.** Gains go to dressing, crowns and far land |
| **Baked or probe ambient** | inv rank 4: a probe or APV option, size M | surf 4.3: an analytic ambient pool first; APV only on the house interior, size L, the riskiest gain | **surf.** The sun moves with `TimeOfDay`, so one bake does not fit; APV needs three or four scenario bakes; the custom shader needs its own sampling and the sources disagree on its keywords; and the flat dusk is loved. (code; source: surf 4.3; record) | APV drops to tier 4; the analytic pool comes first |
| **Which face gain first** | inv: texture hygiene | faces 11: smooth face normals | **faces**, on the evidence: blotchy bands are seen in the portraits; the format question is likely moot. Both are small and go together in F0 | G2 then G8, both in F0 |
| **Cloth fold detail** | inv 3.1: a baked fold map plus bones rather than runtime cloth | mod 7.4: chains in the game (A2), perhaps a cage cloth later; surf 3.2 B: a fold normal used only to bend the light edge | **Compatible, not a conflict.** Silhouette folds are geometry and hang on chains; inner folds are paint or a normal that bends the terminator (mod 7.2; surf 3.2 B) | G28 and G30 |
| **The paint filter on beings** | inv: beings are filtered like the world | faces 2.1, surf 1.3: `_Drawn = 1` keeps beings at 15 percent paint | **faces and surf** (code) | Face strokes survive the screen pass; their sharpness depends on the texture, not on the filter |
| **The face's source size** | inv 1.5: the face is painted at 1024 and baked into the 2048 atlas | faces 2.4 item 3: eye distance 303 px in `Face_Small.png` | **Not settled.** The two may describe different files or stages; neither file reconciles them. (record) | G0c reads the painted face's real size and the face's texel share on all three miners |

Contradictions were looked for in four areas (the shading model, the cameras, the capture tools and the headroom) and
none was found there; small ones elsewhere (the face source size above; world's "far mountains" against hills to
1500 m) are noted where they matter. The comparison was not exhaustive. The headroom figure is GPU only: 4.5 to 5.7 ms
at 1080p on the dev laptop, 7.0 ms with 100 miners (measured, inv 1.0). No CPU figure exists (section 6.2).

---

## 3. The best method for each part

Each method ends with its **weakest link**: the claim it rests on that has the least evidence.

### 3.1 The models: body, topology, joints, garments, LODs, the creator's base

**The method: form by code, topology by template** (mod 3.1 E, 3.2). The body's form stays procedural, made from the
same presets the physics reads. Anatomy landmarks are added to the **drawn surface only** (deltoid, elbow point,
forearm swell, wrist taper, kneecap, calf, ankle, ribcage and pelvis as two masses, the seat), at each limb's same mean
radius, so the preset numbers the physics reads do not change (record: MR 7.2, stage 2, "at the same mean radius";
hypothesis that the colliders then still fit). A fixed quad template with designed loops at every joint is wrapped onto
that form (landmark warp, then projection with Laplacian smoothing). Every body then shares one topology. (hypothesis,
built from code and sources in mod)

**How far the drawn body may stray from the physical one.** "One body for everything" asks that the drawn body and the
physical body stay one. Today's drawn miners already stand outside their capsules (coats, skirts, hair). The proposed
bar: on every segment, the largest distance from the drawn surface to the nearest collider must not grow beyond
today's for that segment, measured on all three miners (hypothesis; a relative bar, so no invented number). Thicker
thighs, a belly or a seat (the "fuller body") are stills only; in the game they reach the physics and are rung B, only
with Luis's word (record: MR 5.3, 7.2). Whether the drawn body may carry landmarks the colliders do not is his call
(question 9).

**Why it is best here:**
- It keeps "one body for everything": the drawn body and the physical body come from one set of numbers. (record)
- It puts the quality into what Claude does well: rules, maths, measurement, many render-and-read loops. It avoids
  what Claude cannot do: sculpt by eye. Brush sculpting in background mode is unconfirmed. (record; source: inv 3.1)
- One topology pays many times: weights authored once, correctives, garment fits, LODs that keep their loops, UVs that
  stay put, and a creator built on blend shapes. (hypothesis: mod 3.2, 9)
- The rejected options: a ready parametric body (MPFB, Anny) fights the drawn look and is a download; SMPL is
  research-only; paid cloth tools and Burst or DOTS packages are excluded or need approval. (source: mod 3.1, 11)

**Topology** (mod 4): three or more loops across elbows and knees, a loop around the elbow point and kneecap, loops
circling the deltoid and each thigh, poles off hinge lines, density by how much a zone bends. Even quads with smooth
normals should also make the toon band follow the form instead of zigzagging. (source, low authority; hypothesis)

**Joints** (mod 5): four tools together. Loops placed for the joint; weights on the closed body with an authored blend
zone per joint and surface smoothing; twist and helper bones set each frame from the physical bones' angles, after the
physics writes (new bones only, so the rule "never write an unchanged bone each frame" holds); corrective blend shapes
made by rule (volume restore per ring, the point pushed out, the crease pulled in), driven by joint angle. Helper bones
and correctives are the same mechanism (a script sets something from joint angles each frame), so both wait on the
same ruling (question 5). Linear blend, four bones a vertex, stays; dual quaternions bulge and are a download. (code;
source; hypothesis)

**The body under the clothes (Open: MR questions 3 and 7)** (mod 6): one closed body, painted as its underlayer. Faces
are removed only where a coverage test over the swept poses and 25 directions proves them never seen. It would end the
floating legs and the sack at the root, and let `bodyFront` be measured on the body. It replaces the cut-away practice
only with Luis's word; until then it is one candidate beside "whole trousers to a seat". (record: MR 7.4 questions 3
and 7; hypothesis)

**Garments** (mod 7): **fitted** parts (sleeves, trousers, the coat's top) are shells grown from the body's own faces,
so they bend where the body bends and take its weights. **Loose** parts (coat skirts, tails, aprons) are flat pattern
panels sewn and draped by Blender's cloth, then hung in the game on chains of bones with leg capsules (rung A2). Big
folds are geometry; small folds are paint or a normal that bends the light edge. **Vents are a costume choice (Open).
Pose-driven garment shapes wait on a ruling (question 4).** Unity's own Cloth and Magica Cloth 2 are not recommended.
(source: mod 7; hypothesis)

**Hands, boots and necks** (mod 11; MR 7.2): hands keep their 1,600-triangle budget, finger bones and grips exactly
where they are, since props hang from them and the models research keeps them. Gains are on the surface (skin zones at
the knuckles, a texel share fit for close-ups) and in being judged beside the faces. Boot soles and necks are judged in
close-ups first; any change to them is a change to the drawn body and goes to judges, then Luis. (record; hypothesis)

**LODs** (mod 8, surf 6.2): the same template with fewer rings; garment panels on coarser grids; correctives only at
LOD0 and perhaps LOD1; the LOD group kept (Unity's automatic Mesh LOD ignores skin weights and blend shapes); cross-fade
wired in the shader. (source; code; hypothesis)

**The creator's base** (mod 9): corner bodies (the three miners plus a few extremes) as blend shapes of one neutral
body on the shared topology; bones moved, never scaled; bind poses recomputed once and baked into the copy; the
physics numbers derived from the same weights and checked against Blender within a set tolerance; weights kept inside
the range where standing, stepping, falling and getting up were verified. Which options it offers is Open. (source;
hypothesis)

**What the tools reach:** procedural form, rigging, weights, shape keys, cloth bakes, renders: **well** (record: inv
3). The template's shoulder, armpit, hip and groin patches and the beauty of the form: **partly**, by many judged loops;
the riskiest part of the whole plan (mod 11 step 2). A CC0 base mesh (Blender Studio's Human Base Meshes) as a
reference for junction loops would help; it is an optional download and needs Luis's approval. (source: mod 3.3)

**Weakest link:** that a template can be wrapped onto three very different forms with clean junction patches. No wrap
has been tried; the spike (G24) exists to test it.

### 3.2 Faces

**The method: keep the drawn face; fix its carrier first; explore prettier drawings second, as stills beside today's**
(faces 0, 4.2, 8.2).

- **Carrier (F0):** normals transferred from a smooth proxy (an ellipsoid for the face plus the nose's own form), as
  Guilty Gear Xrd is reported to edit face normals so light falls in clean zones; the outline reads smooth normals from
  a spare channel; a dedicated face UV island or texture at a set texel density, painted at 2048 to 4096 and filtered
  down; a first-hit projection so the eye is painted only on the outer face; seams kept out of the eyes and mouth; a
  skin material slot with its own softness, translucency and warmth. (source: faces S1 to S4, search summary or read
  in part; hypothesis for our heads)
- **Drawing (F1):** each miner moved toward its own pole: Small toward reference B (a dark upper-lid mass carrying the
  lash flick, a larger iris share, a softer wider mouth with a lip tint, a smaller nose blended in); Long and Round
  toward reference E (Long's eyes a little larger and still sleepy, warmer middle band; Round's crescents with a lash
  flick, cheeks lifted as apples by the form, laugh lines). One change per variant where possible. Today's face is the
  control in every pairing (6.2). (picture: faces 1.2; hypothesis: faces 5.3)
- **Parts as layers** (Wind Waker's practice): skin, blush, soot, eyes, lids, brows, mouth painted apart and saved
  together, for variants, a possible blink and the creator's face presets. (source, weak: faces 4.1)
- Not now: geometric eyes (a different art direction); an SDF face shadow map only if smooth normals are not clean
  enough. (hypothesis: faces 4.2, 11)

**What the tools reach:** the carrier is **moderate** and standard (Blender by Python, an FBX with custom normals, a
Unity import). Beautiful drawings are **hard**: no image generator, no hand painting; beauty comes from many judged
loops of code-painted strokes, and Luis's taste is the gate. (record: inv 3.3; faces 10)

**Weakest link:** the smooth-proxy method rests on community write-ups of one game, seen as search summaries (mod M20;
faces S1). Whether it keeps our noses is untried; the F0 sheet settles it.

### 3.3 Heads

**The method: stills first, at skull scales of minus 8, 0, plus 8 and plus 15 percent, plus one "hair volume only"
row, each with the Strategy-camera crop at its true pixel size** (faces 3.4). At game distance the head's size and the
hair's shape **are** the face (picture: faces 2.5). Bigger heads read younger and cuter and read better from above;
they also fight "real weight and real strength at work" and Long's long, thin idea (source, weak; record; hypothesis:
faces 3.2). The baseline is measured from the mesh before sheet H is drawn (G0c), since today's heads-tall figures are
by eye or from presets. A skull change reaches `headHalf`, the head sphere, the head's mass and the centre of mass, so
in the game it goes with the physics work, by Luis's word; a hair-volume change does not reach the physics.
Proportions stay Open. (record: MR 5.3; faces 3.3)

**Weakest link:** that a head change "pays" at 10 px. Only the 1x crops can show it.

### 3.4 Hair

**The method: mesh locks as the body of the hair, without facets, lit as one mass; a few thin solid ribbons as
flyaways; no alpha cards** (faces 6.2, 6.3). Steps: give the locks their own triangle budget at LOD0 instead of the
global decimation, and raise the lock sides; transfer normals from a smoothed hull of the hair; vary lock widths and
split some ends; an uneven, broken fringe; two to five flyaways per head; strand UVs so a stroke texture runs with the
flow; a soft painted or toon band on the crown from a stored tangent (surf 3.2 H); its own material values; Round's
curls as spiral clumps. Alpha cards fight the outline, sorting and the paint filter. (code; source, weak; hypothesis)

**The triangle budget** (hypothesis, to be set from G0c's per-part counts): LOD0 stays at 16,000 in all by taking the
hair's extra triangles from parts whose decimation shows no loss in pictures; if that is not enough, LOD0 grows only
within the budget Luis sets (question 7). LOD1 and LOD2 decimate the hair with the rest, as today.

**Motion:** a short chain of bones per lock group, on springs driven by the head's real motion and gravity, stopped by
the skull sphere and neck and shoulder capsules. Light, lagging and settling; never a clip; still at rest except as the
breath moves the head. It obeys the physical-objects rule. (record: the rule; hypothesis: faces 6.4)

**What the tools reach:** facets, flyaways and strand UVs: **moderate**. Springs: **moderate to hard** (stable and
gentle on all three miners). Strands: **not** in this pipeline. (faces 10)

**Weakest link:** moving thin locks under the outline hull and the paint filter may crawl (hypothesis); the detector in
6.3 must show it does not.

### 3.5 Surfaces and shaders

**The method: code-painted, placed strokes in UV space from masks built on the mesh, saved as layers; small
light-aware shader terms behind `shader_feature_local` keywords, off by default** (surf 2.2, 3.2, 3.4).

- **Layers on a miner's atlas:** base planes; grain strokes along each panel's grain; seams and stitches a few
  millimetres inside each panel border; patches and mends by rule (elbow, knee, hip, never the face); dry-brush wear on
  elbows, knees, cuffs and hems; a few painted folds placed from the pose sweep; frayed hems; skin zones (cheeks, nose,
  ears, knuckles); contact grime. **Strokes, not noise; few and placed; one light** (no painted form shadow beyond a
  faint wash, since the game's light already shades). Each miner has its own wear. The face painter's tapered,
  wobbling stroke code is the template. (picture: references B, E, F; code; hypothesis)
- **The libraries already installed cover it:** Pillow, NumPy, SciPy and OpenCV, with Blender's bmesh (measured by an
  import check, inv 3.0). No new download.
- **Flow and wear are reachable headless:** the stroke direction is the limb's axis or the panel's grain, known from
  the mesh; convexity and curvature from bmesh avoid bake quirks. (hypothesis: surf 2.1)
- **UVs:** by garment panel, limbs as strips, the head by its own projection; seams where real seams are; 4 texels of
  padding at 2048; a 4096 atlas, or separate head and hand maps, if the texel audit shows the face and hands short
  (surf rank 12). (hypothesis: surf 2.3, 2.4)
- **Shader terms, in order of mood per effort** (surf 3.3): a warm rim from local lights on the lit side (D) and a warm
  bounce from lit ground (E); a warm skin terminator (F); the brush anchored to the body and faded by `fwidth` (A, if
  the swim is real); vertex AO from data already read (C); a fold normal that only bends the light edge (B); coloured
  outline from albedo (I); hair band (H) and cloth sheen (G, now G45) after D is judged; a blurred reflection probe for
  props only (J). Group the beings' extras under one keyword to limit variants. (code; source, search summary;
  hypothesis)
- **What stays** (the protect list, surf 8.2): the brush-broken terminator on the world, cool tinted shade, warm pools
  (`pow 0.75`), the flat trilight as default, Kuwahara, ink and grain with beings kept drawn, fog warm toward the sun,
  the sky.

**What the tools reach:** shader text and code painting: **well** for code, **partly** for beauty. Painted folds that
read natural: **hard**. Every shader result is seen only in a Unity run. (surf 9)

**Weakest link:** the order of the shader terms is part one's judgement of mood per effort (hypothesis); the rim's
form rests on one game's practice seen as a search summary (TF2 rim, surf 3.2 D).

### 3.6 Light and image

**The method: keep the hand-lit dusk; add analytic terms tied to real lights before any baked system** (surf 4).

- Key, pools and the trilight stay. Add the warm rim and bounce (3.5). Add an **analytic grounding shadow**: the ground
  shader darkens softly around each foot from the articulation body's real foot positions, with a brush-broken edge. It
  is a rendering aid tied to the real body, not an animation. (hypothesis: surf 4.5)
- An **analytic ambient pool** around the house's windows and door, scaled by `_WG_GlowScale`, before any APV. APV
  only as a trial on the house interior, three scenarios, as a switch. Legacy probes do not fit a moving sun.
  (hypothesis: surf 4.3)
- **Shadows:** check bias on small bodies (floating foot shadows, lost nose and chin shadows) in a capture before
  changing anything. (hypothesis: surf 4.4)
- **Image precision:** try a 64-bit HDR buffer, camera dithering and bloom high-quality filtering (one field each);
  test MSAA 4x beside SMAA; keep Neutral tonemapping; HDR grading mode and hour-keyed split toning only as switches
  for Luis. 64-bit HDR doubles the colour buffer's bandwidth and MSAA multiplies it, so both are measured (6.2). TAA or
  STP only after motion-vector passes exist, and only if they do not smear the paint. Motion blur, chromatic
  aberration and lens dirt fight the painted look. (code; source, read in part; hypothesis: surf 5)
- **Painted depth:** a Kuwahara radius that grows with depth ("strokes broader with distance", TheEssence device 2)
  serves every view and changes every pixel, the favourite frame included; an anisotropic Kuwahara later. A cinematic
  profile (a second volume, only in `ExploreFlight`) with mild Bokeh depth of field and script-driven focus on the
  followed body. (record; source; hypothesis: surf 5.4, 5.5; world 8.2)
- **A supersampled capture mode** for judging only, so judges and Luis see what the assets can show. (hypothesis: surf
  5.1)

**What the tools reach:** settings and passes: **well**. Probe lighting that keeps the loved look: **hard**; the danger
is the look, not the code. (surf 9.3)

**Weakest link:** that 64-bit HDR and dithering cure a dusk banding nobody has yet measured (hypothesis); G0b's dusk
wide shows whether there is any.

### 3.7 The world and props

**The method: build everything by script in the existing family; place by rules tied to the shared height field;
each new set a switchable variant beside the loved one** (world 0, 10, 11).

- **Dressing by usage rules, not scatter noise:** flowers along the path and wall bases, rocks in groups (one big,
  some small), fallen leaves and mushrooms round trees, reeds at low ground, a dead tree as a landmark, things at the
  house (kettle, bucket, rope coil, washing line). Placed objects rest on something; carried ones get a bone, a measured
  grip and a swing. (record: physical-objects rule; hypothesis: world 4.3 V6, 1.2 P5)
- **Trees and bushes:** solid clumps with leaf-dab relief and a ragged edge, given a spherical proxy normal so each
  crown shades as one brushed mass, as a `Nature_B` set; more species by seed; per-clump flutter from vertex data,
  driven by the same wind as the grass. Alpha cards would need their own depth passes. (source: foliage-normal guides;
  hypothesis: world 4.2)
- **Ground:** a painted detail layer in the ground shader (strokes at two scales, a trodden path) and a path ribbon
  laid on the same polyline; keep the custom mesh, not Unity Terrain. Footprints and trodden grass as a decaying mask
  from real foot contact, so the grass remembers bodies (world T6). (hypothesis: world 3.2, 3.3)
- **Far land:** layered ridges with a tint step per layer, tree and field patches, heights from a NumPy erosion pass
  written once to data. Cloud shadows as a slow mask. (hypothesis: world 3.2 T3, T5)
- **The house:** a `House_Worn` variant (varied shingles, a sag, ragged eaves, broken plaster at the base, stains, a
  real doorstep) and a ground skirt where the walls meet the meadow. A building kit from the same `Part` class later,
  with construction stages driven by real build progress. (hypothesis: world 2.2)
- **Props:** the house's wear bake applied to carried props; atlas size by on-screen size; lantern glass with a small
  inner glow and the existing flicker; the painted highlight on brass and iron. (hypothesis: world 1.2)
- **Sky:** keep. Water and the 3D cumulus only on Luis's request. (record)
- **The Strategy camera:** the frame test at 42 m (grass density and LOD, plume count, the detail layer, fog colour at
  the horizon, ink and paint scale) is checked before any world gain is judged (world C3, T7, S2; now in G0b).

**Kept for later, on purpose** (each named so it is not lost; world's own sizes): B3 baked cavity on tiled house parts
(S; after `House_Worn`, which may carry it); B6 a lit interior worth looking into (S to M; after the analytic pool,
since it is lit by it); V5 moss and soil at the base of rocks and trunks (S; with G19's `Nature_B`); P6 pot plants as
leaf clusters (S; after G19's leaf-cluster method); X6 weather (M; only if Luis wants weather: Open); U5 UI scaling
rules (S; with the UI kit, G38). C4, an hour-graded LUT, is already G34.

**What the tools reach:** all by script; **moderate** for most; **hard** for the "hand-painted feeling" overall, which
comes from stroke variety, restraint and Luis's eye. No store asset is wanted; each would need repainting and Luis's
approval. (world 10, 11.2)

**Weakest link:** that rules place things as naturally as a hand would (hypothesis); regularity is the risk, and only
pictures show it.

### 3.8 VFX

**The method: code-built `ParticleSystem` bursts spawned by real contact events, with painted flipbook sprites made by
Python, drawn in the painted shader family** (world 7.2). A chip leaves the pick head's real contact point (`head` and
`tip` in `tools.json`) at the real swing speed; a footfall puff scales with the body's weight and the ground; a fall
leaves dust at the real contact; motes drift in lamplight; leaves lift in gusts from the grass's wind. Two or three
variants per surface. Rare, never littering. VFX Graph is not installed, is a package change needing approval, and is
not needed at this scale. (code; source, search summary; record: no-fake-animations rule)

**What the tools reach:** **moderate**: easy to build; the painted sprite look and the physical timing need judged
loops.

**Weakest link:** that hand-coded flipbooks read as painted rather than as generic particles (hypothesis).

### 3.9 The UI

**The method: UI Toolkit (UXML and USS), logic kept apart from look, migrated in small steps; a painted UI kit drawn by
Python (nine-sliced paper or cloth panels in the game's palette, ink-and-wash icons); world-space markers where they
fit (a warm ring, a tool glyph, progress shown on the building)** (world 9.2). Runtime UI Toolkit features are not
labelled stable, so keep the move small. (source, read in part) Type matters most, and a font is an optional download
needing Luis's approval; until then the built-in font, well spaced, and titles painted as images. The UI language is
**Open** (record: GAME_VISION); this is a direction to show, not to lock.

**What the tools reach:** layout **well**; a distinctive, beautiful look **moderate to hard** without a font or hand
drawing. (world 11.2)

**Weakest link:** the claim that type is the biggest lever is part one's judgement (hypothesis).

### 3.10 What Claude's tools reach, in one table

From inv 3 and the part-one files. "Well" means a high standard with a render, look and fix loop.

| Work | Reach | The limit |
|---|---|---|
| Procedural modelling, props, buildings | Well | None beyond judging |
| Organic form | Partly | Landmarks by rule, not sculpting; many judged loops |
| Retopology by template | Partly | The junction patches must be designed once |
| UVs, texel density, packing | Well | |
| Painting by code (strokes, wear, zones) | Partly to well | No hand; regularity is the risk |
| Baking (albedo now; normal, AO, curvature) | Well after one test | Cage and ray distance untried (hypothesis) |
| Rigging, weights, shape keys | Well | Unity blend-shape normals need a test (source: mod M21) |
| Cloth drape in Blender | Partly | Sewing is touchy at real scale (source: mod M11) |
| Shaders, passes, settings | Well | Seen only in a Unity run |
| Probe bakes | Partly | Custom-shader sampling unverified |
| Particles, sprites | Well | |
| UI | Partly | A font needs approval |
| Judging beauty | Weaker than judging faults | Aesthetic judges with a calibration control, then Luis |
| Image generation, hand painting, photo reference | Not | Everything comes from code |

---

## 4. One programme of gains, ranked

Ranked by quality gained for the effort, within five tiers. **The ranks and sizes are part one's hypotheses**; no gain
has been built or watched. Every look gain goes to the aesthetic judges, then to Luis, before it becomes a default;
that is not repeated in each row.

**The last column** names only what is **more** than that: a **ruling** on a rule; an **approval** for a download; an
**Open** design choice; or **standing rule** for a screen-wide switch. On screen-wide switches this file takes a side:
the standing rule (explorations are additive, switchable beside the loved look, off by default; record: memory,
"explorations are additive") is enough to build and show them, because nothing Luis sees by default changes. So they
are not held back. Question 6 asks only whether he wants any of them **not** explored; a "no" from him removes it.

**The "Strat." column:** what of the gain can be seen at the Strategy camera, where a miner is about 30 px and a head
about 10 px (picture: faces 2.5): **yes**, **shape** (silhouette or colour only), **no**. All hypotheses, settled by
G0b's 1x crops.

**Switch kinds:** **keyword** = a `shader_feature_local` on `WGPainted` (or the ground or post shader), off by
default, today's values unchanged; **rung** = a miner version on the ladder in 4.6 (new files, new GUIDs, chosen on
the miner panel; MR 5.1); **variant** = a second asset or scene-setup set beside the first (`Nature_B`, `House_Worn`);
**volume** = a second volume profile; **asset copy** = a second URP asset chosen on the look panel; **component** = an
on or off flag; **stills** = Blender sheets only, nothing in the game.

"Unity" in the dependency column means the gain needs a Unity run, in the validation worktree (7.3), which waits while
the long test run holds it (record: Handoff). **FF** in a row means the gain changes pixels of Luis's favourite frame
(7.1).

### Tier 0: the instruments (not gains; every later gain is judged on them)

| # | Instrument | Size | Depends on | Switch | More than judges and Luis |
|---|---|---|---|---|---|
| G0a | **Blender stills kit:** the audit tools repeatable on any version (`render_miner.py`, `measure.py` with its shoulder slip fixed, `silwidth.py`, `sheet.py`); renders in the idle's arm pose (MR 2.9); a wireframe pass; one-bone weight renders; a Blender toon replica of `WG_Direct` so band shapes show; Strategy-camera crops at true pixel size; drawn-surface-to-collider distances per segment (3.1) | S to M | Blender, after the long run | none (tools) | none |
| G0b | **Unity named shot set and one capture command:** the favourite frame, the doorway, the path, a grove, a portrait, the Strategy overview, a dusk wide, at fixed hours; the frame test at 42 m (world C3, T7) and the sky's horizon colour from above (world S2); a supersampled judging mode; motion strips of a walking and working miner; GPU **and CPU** frame time (main thread and physics) per shot; a crowd scene of 100 miners with every switch on | S to M | Unity | none (editor tool) | none |
| G0c | **Cheap facts:** the texture format in the editor; the painted face's real size and the face's and hands' texel share on all three miners; split normals on the miners; triangles per part per LOD (hair included); head height from the mesh (for sheet H); LOD1 and LOD2 pictures; the brush swim test (one pose, two positions 0.5 m apart, diffed); shadow bias at the feet and face; whether sculpt operators and geometry nodes run headless; blend-shape normals on import; **whether any code that builds or follows the physical body would take up new bones** (see 7.2) | S in all | Blender and Unity; a code read | none | none |
| G0d | **Range-of-motion sweep of today's three miners:** pose sheets of every joint to its game extremes (the get-up's hip to 172 degrees included), with ring areas at the knee and elbow measured (mod 11 step 4, "easy"). It is the control for the template spike (G24) and for correctives (G26) | S | G0a | none (instrument) | none |

### Tier 1: cheap gains whose effect is mostly hypothesis (each S, mostly on today's miners, each behind a switch)

G4 is conditional: it is built only if G0c's swim test shows a swim or alias.

| # | Gain | Size | Depends on | Switch | Strat. | More than judges and Luis |
|---|---|---|---|---|---|---|
| G1 | **Warm light-aware rim and warm bounce on beings** (surf 3.2 D, E): "warm light finds them" | S | Unity; G0b | keyword | shape (colour) | none |
| G2 | **Smooth face normals** from a proxy, the nose kept; the outline reads smooth normals from a spare channel (faces 4.3; mod 5.6) | S | G0c (split normals); Blender, then Unity | rung v2 | no | none |
| G3 | **Hair without facets:** its own triangle budget at LOD0, more sides, normals from a smoothed hull (faces 6.3 items 1, 2; budget rule in 3.4) | S to M | Blender; G0c's per-part counts | rung v2 | no | none, unless LOD0 must grow (question 7) |
| G4 | **The brush anchored to the body and filtered** (bind-pose coordinates in a spare UV; `fwidth` fade) (surf 3.2 A). **Conditional** | S to M | G0c swim test shows a swim or alias; Unity | keyword | yes | none |
| G5 | **LOD cross-fade wired** (keyword and `LODFadeCrossFade` in three passes; the group's fade mode) (surf 6.2) | S | Unity | keyword plus the group's fade mode | yes | none |
| G6 | **Warm skin terminator and gentle skin colour zones** (surf 3.2 F; faces 5.2 item 4) | S | Unity; better after G8's skin slot | keyword plus a face variant | no | none |
| G7 | **Image precision:** 64-bit HDR, camera dithering, bloom high-quality filtering; MSAA 4x beside SMAA as a test (surf 5). FF | S | Unity; G0b | asset copy | yes | standing rule (question 6) |
| G8 | **Dedicated face map, skin and hair material slots, the stray eye fragment mended,** strokes painted finer and filtered down (faces 5.2) | S to M | Blender and `MinerSetup`; G0c texel share | rung v2 | no | none |
| G9 | **Vertex AO and convexity from bmesh** into vertex alpha (data the shader already reads) (surf 3.2 C). Baked in the rest pose: it does not follow a bend, so it is kept faint | S | Blender | rung v2 | no | none |
| G10 | **The prop pass:** wear bake on carried props, atlas size by on-screen size, lit lantern glass, painted metal highlight (world 1.2 P1 to P4) | S | Blender; G0c texel density | prop variant | no | none |

### Tier 2: medium gains

| # | Gain | Size | Depends on | Switch | Strat. | More than judges and Luis |
|---|---|---|---|---|---|---|
| G11 | **Step 0 body sheets** (today's, look only, fuller body; MR 7.2), plus a separate **landmarks sheet** at each limb's same mean radius (mod 11 step 1), plus one **"dressed" row** (5.1) | M | G0a; the energy count first (record) | stills | shape | **Open:** Luis picks the look and the depth |
| G12 | **Face sheets F0 (G2, G3, G8 in stills) and F1 (prettier drawings), and the head sheet H** (faces 8.2, 3.4) | M | G0a; G0c head height; G2, G3, G8 in Blender | stills, later a face variant | shape (H only) | **Open:** the faces' look; head proportions |
| G42 | **Hands, boots and necks:** a close-up row on the step-0 sheets (grips with pick, mug, satchel; boot soles in profile and kneeling; necks and collars); then, if the texel audit asks, separate head and hand maps or a 4096 atlas (surf rank 12); skin zones at the knuckles; any change to boot soles or necks only after judges and Luis | S to M | G0a; G0c texel share | stills, then rung v2 or v3 | no | none for the maps; the drawn body's changes go to Luis |
| G13 | **Work and footfall VFX**, contact-driven, painted sprites (world 7.3 X1 to X5). FF | S to M | Unity | component | yes | none (it must be caused by real contact) |
| G14 | **Analytic foot grounding shadow** (surf 4.5) | S to M | Unity | keyword on the ground shader | yes | none |
| G15 | **The dressing pass:** flowers, rock and bush variants, ground cover, things at the house, by usage rules (world 12 rank 1). FF | M | Blender and C# | variant | yes | none |
| G16 | **Painted atlas layers on the new version:** seams, stitches, patches, wear, painted folds, contact grime (surf 2.2) | M | Luis's step-0 choice; the new version's UVs (panel UVs if G24 and G28 are used) | rung v3 | no | none |
| G17 | **Ground detail layer and path ribbon** (world 3.2 T1, T2). FF | M | Unity | keyword plus variant | yes | none |
| G44 | **Footprints and trodden grass** as a decaying mask from real foot contact; the parting stays a while and springs back (world T6). FF | M | Unity; G17 | keyword | shape | none (driven by real contact) |
| G18 | **`House_Worn` and the ground contact skirt** (world 2.2 B1, B2). FF | M | Blender | variant | shape | none |
| G19 | **Leaf-clump trees and bushes with proxy normals, more species** (`Nature_B`) (world 4.3 V1, V3, V4). FF | M | Blender; the wind in `WGPainted` | variant | yes | none |
| G20 | **Depth-aware paint radius** ("strokes broader with distance") (surf 5.4). Changes the whole screen. FF | S to M | Unity | look panel | yes | standing rule (question 6) |
| G21 | **Cloud shadows (S), then the far-land pass (M)** (world 3.2 T5, T3). FF | S then M | Unity | keyword plus variant | yes | none |
| G22 | **Coloured outline from albedo** (surf 3.2 I) | S | Unity | material property, today's value kept | shape | none |
| G23 | **Grass variety** (world 3.2 T4). Changes the tall dark grass of the favourite frame. FF | M | Unity | keyword | yes | none |

### Tier 3: the foundations (large; they unlock the depth Luis may choose)

**A gate on every row from G24 to G31 and G43:** before a rung reaches the game, the get-up on all three miners as
played, and the idle at 14 and 4 unchanged (6.2), not only for the creator (G40).

| # | Gain | Size | Depends on | Switch | Strat. | More than judges and Luis |
|---|---|---|---|---|---|---|
| G24 | **The template topology,** spike first (one arm, one leg, the hip and shoulder patches on Small, with wireframe and range-of-motion sheets against G0d), then all three miners (mod 3, 4) | spike M, whole L | G0a, G0d | rung v3 (if the spike passes) | no | **approval** only if a CC0 base mesh is wanted as a reference |
| G43 | **Landmarks in the form, in the game** (elbow point, forearm swell, wrist taper, kneecap, calf) at each limb's same mean radius, on today's topology or the template; the models research puts them in rung A2 (MR 7.2) | M | the landmarks sheet judged and chosen; G0a's collider distances | rung v3 or v4 | no | **ruling:** may the drawn body carry anatomy the colliders do not (question 9) |
| G25 | **Weights on the closed template; twist and helper bones** set from the physical bones (mod 5.2, 5.4) | S to M | G24; G0c bone check | rung v4 | no | **ruling** (question 5): the same mechanism as correctives |
| G26 | **Correctives by rule,** driven by joint angle, measured against G0d; blend-shape import switched on (mod 5.3) | M | G24, G25; G0c blend-shape normals | rung v4 | no | **ruling** (question 5) |
| G27 | **The closed body under the clothes** with the coverage test (mod 6) | M | G24; Luis's MR questions 3 and 7 | rung v4 | shape | **Open:** trousers or a body under the coat; the cut-away practice |
| G28 | **Fitted garments as shells; loose garments as draped panels on chains with leg capsules** (mod 7; rung A2) | L | G24; A2 game code behind a version gate | rung v4 | shape | **Open:** vents; **ruling:** pose-driven garment shapes if used (question 4) |
| G29 | **LODs from ring counts and grids,** audited at each switch distance (mod 8) | S to M | G24 | rung v3 | yes | none |
| G30 | **A baked fold normal that only bends the light edge,** with a tangent input (surf 3.2 B) | M | G28 or a fold source; a bake test | keyword | no | none |
| G31 | **Hair's finishing:** flyaways and lock variety (M), strand UVs and a crown band (M), springs on a few lock groups (M) (faces 6.3, 6.4; surf 3.2 H) | M each | G3; Luis's head choice | rung v4 plus keyword | no | none (springs obey the physical-objects rule) |
| G45 | **Cloth sheen** (surf 3.2 G), after G1 is judged | S | G1 | keyword | no | none |
| G32 | **A blink from the body** (head turns, effort, tiredness), as painted lid layers (faces 7.3) | M | G8 layers; Luis's face choice | component | no | **ruling:** does it fit "no fake animations"? (question 3) |

### Tier 4: later, larger, or only on request

| # | Gain | Size | Depends on | Switch | Strat. | More than judges and Luis |
|---|---|---|---|---|---|---|
| G33 | **A cinematic profile:** Explore-only Bokeh depth of field with script focus, high-quality bloom, a wider paint radius (surf 5.4; world 8.2) | S | Unity | volume | no | standing rule (question 6) |
| G34 | **Hour-keyed grading** (split toning or shadows-midtones-highlights from `TimeOfDay`, or a script-built LUT; world C4) (surf 5.2). FF | S | Unity | volume | yes | standing rule (question 6); dusk must not brighten |
| G35 | **Anisotropic Kuwahara** (surf 5.5). FF | M | G20 judged | look panel | yes | standing rule (question 6) |
| G36 | **TAA or STP** with motion-vector passes in the custom shaders (surf 5.1). FF | M | Unity | asset copy | yes | none; high risk of smear |
| G37a | **Analytic ambient pool around the house** (surf 4.3). FF | S to M | Unity | keyword | shape | none |
| G37b | **APV trial on the house interior, three scenarios** (surf 4.3) | L | G37a judged | asset copy | no | none; the riskiest to the loved dusk |
| G37c | **A blurred reflection probe for props only** (surf 3.2 J) | S | Unity | keyword | no | none |
| G38 | **A painted UI kit, UI Toolkit migration, world-space markers, scaling rules** (world 9.3, U5) | M to L | none | component | yes | **Open:** the UI language; **approval:** a font, optional |
| G39 | **A building kit and construction stages** (world 2.2 B4, B5) | L | the RTS's needs | variant | yes | none |
| G40 | **The creator's foundation** (mod 9) | L | G24 to G29; the any-body get-up verified over a range | separate system | shape | **Open:** the creator's options |
| G41a | **Water, the 3D cumulus, a lake** | M to L | Luis's request | variant | yes | **Open** (water and the far land) |
| G41b | **An SDF face shadow map,** only if G2 is not clean | M | G2 judged | rung v2 | no | none |
| G41c | **Geometric eyes** (not recommended: a different art direction) | M | Luis's request | rung | no | his request |

**The count:** 4 instruments (G0a to G0d) and 45 gains (G1 to G45), in 49 rows once G37 and G41 are split into a, b
and c.

**Not in the programme:** a ready parametric body as the game mesh; dual quaternion skinning first; Unity's Cloth;
Magica Cloth 2; Unity's automatic Mesh LOD for skinned miners; store trees, buildings, water, FX or UI packs; VFX
Graph and Cinemachine (packages needing approval, not needed); motion blur, chromatic aberration, lens dirt; glossy
PBR on skin or cloth. Reasons in section 3 and in the part-one files. (source; hypothesis) Kept for later on purpose:
the world items listed at the end of 3.7.

### 4.6 The version ladder: how the miner versions combine

Nine gains change a miner's files. Each as its own version, over three miners, would be a test matrix nobody could
check. So they ship as **rungs**. Each rung contains the one below it. Gains inside a rung ship as one export.

| Rung | What it carries | Built on | Reaches the game when |
|---|---|---|---|
| **v1** | Today's miners, untouched; the default until Luis says otherwise | | now |
| **v2, "the carrier"** | G2 smooth face normals, G3 hair without facets, G8 face map and slots, G9 vertex AO, G42's maps if needed; today's bodies. The chosen F1 drawings are **face variants inside v2** (a texture choice, not a new mesh) | v1's bodies | after F0 is judged and Luis has seen it |
| **v3, "the body"** | Luis's chosen look and depth as static art (MR A1), G16 painted layers, G29 LODs, the template if the spike passed (G24) | v2 | after his step-0 choice |
| **v4, "the garments"** | MR A2: G28 garments on chains, G25 helpers, G26 correctives, G27 the closed body if chosen, G43 landmarks if not in v3, G31 hair finishing and springs | v3 | after his rulings and Open answers (5.2, phase F) |
| Rung B, the creator | Proportions that reach the physics; the creator's base | | only with his word (MR 5.3, 5.4) |

**The test matrix:** 3 miners by 4 rungs = 12 model sets. On each new rung: the golden check that the rungs below are
unchanged; the physics-number table; the get-up as played on all three; the idle at 14 and 4. Shader keywords are
independent of rungs: each is tested on and off against v1 and against the newest rung, and all on once per capture
set; not every combination. (hypothesis: a proposal, not built)

---

## 5. The order of work

### 5.1 What step 0's stills are built with

**The current pipeline, with targeted changes behind flags that are off by default.** Not the template method.

Why (hypothesis, from the evidence named):
- **The stills exist so that Luis chooses the look and the depth from pictures** (record: MR 7.1). Their content (a
  waist ring at the coat's looseness, whole trousers or a simple body under the coat, the apron from the bib, thicker
  thighs, a belly and a seat) is garment construction and preset numbers that today's code can make (record: MR 5.1).
- **At rest the template changes topology, not form.** The form comes from the same presets either way (mod 3.2). So
  a still of today's pipeline is a fair picture of the look Luis is choosing. The template's gain shows in bending, in
  band shape and in LODs, which a rest still does not decide.
- **The template is the riskiest part of the whole plan** (mod 11 step 2). Putting it before step 0 would hold Luis's
  pictures behind the hardest, least certain work.
- **The guards already exist on paper:** every new behaviour behind a flag, off by default; a golden check that
  regenerates the first version and compares it with the shipped FBX and JSON; the second version's export writing
  only its own entries (record: MR 5.1).
- **The models research agrees** (record, read for this revision: MR 2 to 4, 7.2): stage 0 is Blender stills in the
  idle's arm pose (the rest pose's arms at 25 degrees may make the chest read as a ball, MR 2.9); the getting-up leg
  folds belong to the body that gets up, not to the model (MR 2.4); limbs show strength "only where the physics agrees"
  (MR 4.4 item 7). Nothing there contradicts building step 0 on today's pipeline.

**What else joins step 0, each on its own sheet so one variable is judged at a time** (record: memory, "sweep the
variable Luis names"):
- the **landmarks sheet**: anatomy landmarks at each limb's same mean radius, on today's topology (mod 11 step 1); it
  needs no new topology; the collider distances (G0a) are shown beside it, so "leaves the physics alone" is measured,
  not assumed;
- the **face sheets F0 and F1** and the **head sheet H** (faces 8.2): F0 carries today's exact marks better (G2, G3,
  G8 in Blender); F1 shows prettier drawings on top of F0, with today's drawing on F0 as the control; H shows the four
  skull scales and the hair-volume row, from the head height measured on the mesh;
- a **close-up row of hands, boots and necks** (G42), beside the faces;
- **one "dressed" row:** the look-only and fuller-body candidates with a light mock of the surface layers (a few placed
  patches, hem wear, seams; G16's method in miniature) and the toon replica's warm rim. It is labelled a mock. It
  exists because the bare stills are the flattest, least-lit rendering of the miners, and a fuller body may look better
  or worse once its cloth is worn and its edge is warm. The bare rows stay the comparison; the dressed row is a second
  look, not a mixed variable.

**A caveat to state on every sheet:** Blender's render is not Unity's look (record: inv 3.2). The toon replica of
`WG_Direct` (G0a) brings the band shape closer, but the paint filter, ink and outline are only Unity's. Shader gains
(G1, G4 to G7) are therefore judged on Unity captures. Phase D's capture sheet may reach Luis before he picks the depth;
if it does, he sees today's miners with the warm rim beside the stills.

### 5.2 The sequence

The usage rule is one agent at a time (record: memory, "usage budget aware"). "Beside" below means in alternating
sessions, never two heavy jobs at once, and never heavy GPU work beside a Unity test run (record: memory, "Unity
validation runs").

**The trade, said plainly:** the largest gap (bodies and cloth) reaches the game later than the small gains. Faces,
hair and shader switches come first because they are cheap, additive and on today's miners; the body waits for Luis's
pick, then the spike, then A1 (4 to 6 sessions) and A2 (4 to 6) (record: MR 5.5). A first improved miner in the game
is the v2 carrier, after about 10 to 17 sessions from now; Luis's chosen body (v3) after about 18 to 28 sessions plus
his looking time; at about seven sessions a week, roughly three to five weeks (hypothesis).

| Phase | When | What | Sessions (hypothesis) |
|---|---|---|---|
| **A. Light work only** | Now, while the 24-course energy run (`cc_en24`) holds Unity and the GPU | This revision; the judges' brief for a model at rest and in poses (none exists; record: MR 7.2), with the calibration control (6.2); step 0's Blender code written behind flags (waist ring, whole trousers or a body under the coat, the fuller-body numbers, landmarks; F0: proxy normals, the hair budget, the face island; the head-scale preset; the dressed-row mock); the golden check; `measure.py`'s slip fixed; the code read for new bones (G0c) | 1 to 2 |
| **B. The energy count read** | When the run ends | It stays first, as Luis approved (record): the `LIVE_BLOWUP` events and traces read and written into Validation | (outside this programme) |
| **C. Step 0's renders and judges** | After B | G0a, G0d; G11 (body sheets, landmarks, the dressed row); G12 (F0, F1, H); G42's close-up row; blind aesthetic judges with today's three as the control row; then the sheets to Luis, each report naming its switch first | 5 to 8 |
| **D. The Unity side** | Beside C, in other sessions, once Unity is free | G0b (with the 42 m frame test, CPU, the crowd scene), G0c; then G1, G5, G7, the swim test and G4 if needed, G6, on today's miners as switches; then v2 (G2, G3, G8, G9) exported; one capture sheet to Luis, separate from step 0 | 3 to 5, plus 2 to 4 for v2 |
| **E. While Luis looks** | After C's sheets are sent | **The template spike first** (G24, one arm, one leg, the hip and shoulder; measured against G0d), because it is the riskiest and serves the biggest gap; then the world's first pass (G10 props, G13 VFX, G15 dressing), lowest in the gap ranking | 5 to 8 |
| **F. After Luis picks** | His choice of look and depth | Each step names the answers it needs before it starts: **v3** (A1) needs his look and depth (MR Q1, Q2, Q3); built on the template if the spike passed, or on today's pipeline with targeted changes if not; with G16 and G29. **The chosen faces** as face variants in v2 need his F1 pick. **v4** (A2) needs vents (question 10), the cut-away practice (MR Q7) if the closed body is used, the correctives and helpers ruling (question 5), and the pose-shapes ruling (question 4) if those are used; G28 behind a version gate. **G43** needs question 9. **Rung B** only with his word, after the blow-up is understood (record: MR 7.2). Every rung passes the gate in tier 3 before it reaches the game | v3 about 4 to 6, v4 about 4 to 6 (record: MR 5.5); the template's rest 5 to 8 |
| **G. The rest of tiers 2 to 4** | In rank order, between the above | G14, G17 to G23, G44, G45, then tier 4 as Luis wants | counted in 5.3 |

**Until Luis's next two sheets** (step 0, and the Unity switches): about **9 to 15 sessions** (phases A, C, D without
v2). By the Handoff's rate of 13 to 14 points of the week per five-hour window (record), about seven sessions fit in a
week. These counts are rough guesses, given because Luis watches usage, not as a promise.

### 5.3 The whole size, the judges' cost, and where it can stop

Using the mapping at the top (S = 1, S to M = 1 to 3, M = 2 to 4, L = 5 to 8 sessions), summed per tier, overlaps with
the models research's rungs counted once (hypothesis throughout):

| Part | Sessions |
|---|---|
| Tier 0, instruments | 4 to 8 |
| Tier 1 | 10 to 16 |
| Tier 2 | 26 to 54 |
| Tier 3 | 31 to 59 |
| Judges for tiers 0 to 3 (about 30 to 40 judged sheets, 0.3 to 0.5 of a session each) | 10 to 20 |
| **Through tier 3** | **about 80 to 155** (roughly 3 to 5 months at seven a week) |
| Tier 4, if all of it is wanted | 28 to 55 more |

**Judges per sheet:** four lenses (a costume maker, a figure and anatomy artist, the game's art director, a player at
the game's distance; record: MR 7.2) plus a referee: five judge agents, blind and pairwise, the order shuffled; one
agent at a time, by the usage rule. A face sheet swaps the costume maker for a face-and-portrait lens (hypothesis).

**Cut lines** (where Luis may stop and still have a coherent result; hypothesis):
1. **After step 0, tier 1 and v2** (about 20 to 30 sessions): today's bodies, carried well; warm light on them; his
   choice of look made and recorded. Nothing half-built.
2. **After v3** (about 35 to 50): his chosen body as static art, painted, with LODs. The models research's A1.
3. **After tier 3** (about 80 to 155): the garments on chains, correctives, hair finishing; the base the creator needs.
Tier 4 is on request. The getting-up work Luis also wants competes for the same sessions; which comes first is his
call (MR question 10).

### 5.4 What the template spike decides

The spike's bar, set before it runs: on Small's arm and leg and the hip and shoulder patches, at rest, 45, 90 and 135
degrees and the get-up's real extremes (hip to 172 degrees; measured, MR 2.4):
- no collapse or pinch that the judges rate as breaking the look;
- the ring-area loss at the knee and elbow smaller than today's mesh at the same angle (measured on both, against
  G0d);
- the toon band in the replica following the form, where today's zigzags (picture, side by side);
- the drawn-surface-to-collider distance per segment no larger than today's (3.1);
- the wrap stable on all three miners' forms (Long fails first; record: memory).

If it passes, v3 may be built on the template. If not, v3 is built on today's pipeline with targeted changes, and the
template goes on as research. Either way, step 0's chosen look is the target.

---

## 6. How each gain is checked

### 6.1 The rules for every check

- **Pictures before numbers.** A result is reported only after its run and pictures were read; no commit is chained
  behind a test (record: memory, "read results before claiming").
- **All three miners,** Small, Long and Round, through a whole work-tire-rest cycle where motion is involved, at
  several strengths; Long fails first (record: memory).
- **Today's look as the control** in every sheet and capture, same light, same frame.
- **The favourite frame as a control capture** for every gain marked FF (7.1).
- **One variable per sheet** where possible.
- **Every report to Luis names the switch to turn on in its first line** (record: memory, "say what must be switched
  on").
- **Passing judges and tests is not acceptance.** Only Luis accepts a look (record: AGENTS.md).

### 6.2 By kind of gain

| Check | For | How | Bar or what is read |
|---|---|---|---|
| **Stills and judges** | Every look gain; step 0; faces; heads; world sets | Blender sheets (G0a): turntables, poses, close-ups, grey values, mirrored and blurred copies, the references B and E (and A for the world) at the same scale. Blind aesthetic judges, pairwise, the order shuffled per sheet, each told the miner's one idea and the "keep" list; a referee; then Luis (record: faces 9; MR 7.2). **Calibration:** today's face (or look) is a known-good control in every pairing, and a judge's result counts only if, in the same round, it picks today's face over a degraded copy of it (blurred, one mark moved) | No top-severity fault on any miner; the judges' reasons, not only their pick. Judges spot faults better than beauty, and may prefer a generic face (record: inv 3.0; faces 9.2) |
| **Game-camera captures at true size** | Heads, hair, faces, LODs, the trio's reading, dressing, VFX | The Strategy camera at its real height and 1080p: a miner about 30 px and a head about 10 px (picture: faces 2.5); 1x crops plus 2x and 4x zooms of the same pixels; conversation distance; the cinematic view; the named shot set (G0b) at three hours of light | The three miners still read apart by silhouette and colour; the head change pays or does not at 1x; the "Strat." column of section 4 confirmed or corrected |
| **Every frame as played** | Anything seen in motion | Captures of standing, the idle, walking, working, falling and getting up, as played (`-ownAsPlayed 1`), from the four ways (side47, headright32, feetleft32, above) and the player's own camera; a detector per fault (record: memory, "every frame as played") | See the detectors below |
| **Physics and rig regressions** | Anything that changes a miner's FBX, rig or weights | The physics-number table (the stay numbers equal, the follow numbers reported; MR 5); the golden check on the rungs below; `FlapSeamTests` and `CoverRingLiveTests` as played; the hand checks; prop stops re-measured; the get-up as played on all three; the idle at 14 and 4 against v1; render-only bones (helpers, hair) never in the articulation and never written unchanged each frame | Every stay number equal; the idle unchanged; no new fault the model brings. Rung B adds the whole PlayMode suite and paired runs of many courses (MR 8.3) |
| **Performance** | Every switch and every rung | `LookBenchmark` on the named shots, switch on and off, at 1080p on the dev laptop; **GPU and CPU** frame time (main thread, physics, and the per-frame scripts this programme adds: helper bones, correctives, hair springs, garment chains, the foot-shadow arrays); the 100-miner crowd with everything on; triangle counts per LOD; shader variant counts | Each switch's cost and the **running sum** written down after every change. **A ceiling is Luis's call** with the target hardware (question 7). Until he sets one, a working bar (hypothesis, a proposal): stop and ask him before any switch that takes the all-on 100-miner scene more than 30 percent over today's GPU or CPU frame time |

### 6.3 Detectors for motion faults (each with its test)

| Fault | Detector |
|---|---|
| Brush swim on a moving body | One pose rendered at two world positions 0.5 m apart, diffed in the body's own pixels (surf 3.2 A); then a walking strip where the terminator should move with the body |
| Shimmer and alias | A still camera on a still miner: per-pixel luminance variance over frames, at 5, 20 and 40 m |
| LOD pop | A slow dolly through each switch distance: a spike in the frame-to-frame difference on the miner's pixels |
| Outline opening | Gaps in the hull at seams and on the face after G2, read in close captures |
| Outline crawl on moving hair | Frame-to-frame difference of the ink and outline pixels on the hair during a head turn and a walk, against a still head; and against v1's hair |
| Cloth clipping | Points of a leg inside the skirt and of the skirt inside a leg, counted per frame (MR 8.2) |
| Hair springs | Lock angle and speed bounds; settling time after a stop |
| **The idle unchanged** (for every gain that adds a moving part: hair springs, helper bones, correctives, garment chains) | At rest, at 14 and 4, every added part's motion stays inside what the breath alone moves (its amplitude measured on v1 the same way); corrective weights at rest exactly zero; helper bones at rest equal to their bind values. The breath stays small and grows with the situation (record: memory, "breath gentle and scaled"); no added part may enlarge it |
| VFX | Every effect matched to a logged contact event, and no effect without one |
| Footprints | Every print matched to a logged foot contact; fades on its timer from contact, never placed |
| Blink (if ruled in) | Every blink matched to a logged trigger (head turn, effort, landing); no fixed period in the intervals |
| Correctives and helpers | Ring areas against rest through the sweep (G0d); no pop when a weight starts |

---

## 7. Risks, and what could break the loved look

### 7.1 What is loved and must be protected (record)

- **The favourite frame:** the lit house at dusk from low on the path, tall dark grass in front, the large tree behind
  the house (record: TheEssence, October 2; world 0). **Every gain that changes any of its pixels** is marked FF in
  section 4: G7 image precision, G13 VFX, G15 dressing, G17 the ground and path, G44 footprints, G18 the house, G19 the
  trees, G20 the paint radius, G21 cloud shadows and far land, G23 the grass, and in tier 4 G34, G35, G36 and G37a
  (and G33 if the frame is taken in the Explore view). Each is off by default, and the favourite frame is a control
  capture in every report on it, today's beside the switch.
- **Look E's screen look:** the brush-broken terminator, cool coloured shade, warm pools, the flat trilight, Kuwahara,
  ink and grain with beings kept drawn, warm fog, the sky (surf 8.2). Dusk is "30 percent darker than the daylight
  references" on purpose (record: TheEssence).
- **Today's faces,** their marks and each miner's idea (record: the November 14 answers).
- **The hands, grips, props and laced boots,** kept exactly by the models research (record: MR 2.10, 7.2).
- **The idle standing and its little movements,** at 14 and 4, guarded against both regression and new motion (6.3)
  (record: memory, November 13).
- **Luis's intimate dusk over the bright vista pass** he rejected (record: memory, his visual taste).

### 7.2 What could break it, and the guard

| Risk | How it would break the look | Guard |
|---|---|---|
| A switch left on by default | The loved look changes silently | Every gain off by default; today's values are the default values; the switch named in the report's first line |
| Shared Blender modules | A re-export of the first version changes it (record: MR 5.1) | The golden check on every rung below the new one; flags off by default; each rung's export writes only its own entries |
| Too many versions | Untested combinations; each re-export a risk | The rung ladder (4.6): four rungs, gains inside a rung ship as one export |
| Smooth face normals (G2) | The nose flattens; the outline moves on the face | The nose's own proxy; the outline reads a separate smooth channel; F0 sheets with today's face beside |
| Prettier drawings (F1) | The miner becomes generic and loses its idea | Judges told each idea and the keep list; one change per variant; the calibration control; Luis picks |
| A head change | Miners read as children; the centre of mass rises | Stills only; hair-volume row; the physics work only with Luis's word |
| The warm rim and bounce (G1) | Miners glow; the pool contrast flattens | Small amounts; judged at three hours of light against today |
| Brush anchoring (G4) | The miners' paint character changes | Only if the swim test shows a swim; side-by-side motion strips |
| Painted layers (G16) | Random noise that reads as a filter; "two shadows look dirty" | Few placed strokes; no painted form shadow beyond a faint wash (surf 2.2) |
| Screen-wide changes: APV, grading, TAA, depth of field, **the depth-aware paint radius (G20)**, **the grass variety (G23)** | Dusk lit "by a renderer", brightened, smeared or over-softened; the tall dark grass of the favourite frame lost | Off by default; the favourite frame as a control; still and pan tests; the frame test as judge; APV interior-only |
| Trees, the house, the ground and the far land redrawn | The favourite frame's silhouettes change | Variants; the favourite frame captured first |
| The drawn body strays from the physical one | "One body for everything" is lost; the get-up for any body at risk | The collider-distance bar (3.1); landmarks at the same mean radius; the fuller body only as stills until rung B |
| Garment physics (A2, springs) | The cloth or hair pushes the physical body | Render-only bones; never in the articulation; the get-up checked on all three |
| **New bones swept into the physics** | Helper or hair bones become physical links | Code read for this revision: `OwnBody` makes its links as its own objects from `ProceduralBiped`'s named segments (pelvis, torso, head, the limbs, feet, toes), not by walking the FBX's bones (code: `OwnBody.cs` near lines 470 to 530; `ProceduralBiped.cs:425`). So unnamed bones are not made links on that path. **Not read:** the code that makes the drawn bones follow the links, and the old way's body. G0c finishes the read; the rule "set after, read back" applies (record: memory) |
| Helper bones | A rewrite of an unchanged bone takes over the body (record: memory) | New bones only, written after the physics |
| Moving hair under the outline and paint filter | Ink crawl, shimmering locks | The crawl detector (6.3); few lock groups; judged in motion strips |
| Too many keywords | Variant explosion, slower builds | `shader_feature_local` and one beings' group keyword (surf 3.4) |
| Cost: CPU and GPU | Per-frame scripts multiply by miner count; 64-bit HDR and MSAA multiply bandwidth; hair and blend shapes add vertex work | CPU and GPU measured per switch and as a running sum, with a crowd test (6.2); the ceiling is Luis's; stop and ask over it |
| Unknown hardware | A gain that is cheap on the dev laptop is not on the target | Each switch's cost measured and reported; Luis sets the floor |
| Judges and beauty | Faults are caught, beauty is missed; a generic face preferred | Pairwise judging; references beside; the calibration control (6.2); Luis decides |
| Code-painted regularity | Strokes look mechanical up close | Stroke variety and placement rules; close captures |
| Scope | The programme crowds out the getting-up work Luis also wants | The size and cut lines (5.3); rank order; finishable steps; his priority call (MR 7.4 question 10) |

### 7.3 Where the work happens (protecting human work)

- The main working copy has uncommitted changes to `TheGroup.unity` and two project settings files (record: git
  status, November 15). No gain is built there.
- Unity work happens in the validation worktree (`D:/Dev/WG`) or a clean clone pinned to a known commit (record:
  AGENTS.md; memory, "Unity validation runs"); nothing is reset, cleaned or overwritten in the main copy without Luis's
  word.
- Scenes and prefabs are changed through Unity's authoring APIs; asset GUIDs are kept; new miner rungs get new GUIDs
  and leave v1's files untouched.
- A `shader_feature_local` added to `WGPainted` or the ground shader touches every material that uses it, so it is
  reviewed as shared code by a reviewer who did not write it, with today's look captured before and after with the
  keyword off (record: AGENTS.md, consequence-proportional review).
- Blender work writes only its own rung's files; the golden check guards the rest.

---

## 8. The questions only Luis can answer

Every item below stays Open until he answers. The models research's twelve questions (MR 7.4) still stand; the depth,
the waist, trousers or a body under the coat, the cut-away practice, the strength rule, held objects, Long's stance and
which version is the default are not repeated here.

1. **The order.** Miners first (step 0, with the small Unity switches beside it); the template spike while he looks,
   then the world's first pass; the UI and buildings later. Does that match his priority?
2. **Step 0's sheets.** The three body ways, plus separate landmarks, faces (F0, F1), head (H), a close-up row of hands,
   boots and necks, and one "dressed" mock row. Is that the right amount to look at, or should some wait?
3. **A blink from the body** (triggered by head turns, effort and landing, scaled by tiredness, never on a timer):
   does it fit "no fake animations"?
4. **Pose-driven garment shapes** (Blender's cloth answer at set poses, blended in the game by the hip and knee angles,
   beside the swinging chains): do they count as physical?
5. **Joint-angle correctives and twist or helper bones** (the skin's answer to the bone, the same each time): may they
   be judged as the body's own? One answer covers both.
6. **Screen-wide switches.** Under the standing rule these are built off by default as switches to compare: 64-bit
   HDR and MSAA, the depth-aware paint radius, hour-keyed grading, depth of field in the Explore view, an anisotropic
   Kuwahara. Is any of them one he does **not** want explored?
7. **Target hardware, budgets and a ceiling.** Which machine is the floor? What ceiling on GPU and CPU frame time
   (with how many miners) should stop the programme until he looks? May LOD0 grow denser (for the hair, or for the
   cinematic view)?
8. **Downloads.** None is needed. Two are optional, both his to approve: a CC0 base mesh as a topology reference only,
   and a font for the UI. Does he want either?
9. **The drawn body against the physical one.** May the drawn body carry anatomy landmarks (an elbow point, a calf, a
   kneecap) at the same mean radius, which the colliders do not have, within today's own distance from them?
10. **Coat vents** (slits at the hem so the legs pass between panels): a costume choice. Wanted, not wanted, or shown
    in stills?
11. **Water, the 3D cumulus, the far land with a lake, weather, the UI language:** still only on his request. Does he
    want any of them explored now, as options?
12. **Where the programme may stop.** Of the three cut lines (5.3), which one should the work aim for before the
    getting-up work takes the sessions again, or should they alternate?

---

## 9. What this did not do

- **No Unity or Blender run.** No still, capture, bake, wrap, cloth sim or shader was made. Every gain is a hypothesis
  until its pictures are read.
- **No new measurement.** Every number is from part one or a project log. The face's atlas share is rough, from Small
  only, and linear. Heads-tall figures are by eye or from presets.
- **Not confirmed in the editor:** the texture format; whether the brush swims; whether LODs pop; split normals;
  shadow bias on small bodies; texel density per part; triangles per part.
- **LOD1 and LOD2 were not looked at,** here or in part one. Hands, boots and necks were not looked at in pictures for
  this file; their state comes from the code and the models research's critic.
- **No web search.** Outside claims come from the part-one files, many of them search summaries.
- **The models research:** for this revision its sections 2 to 4 and 7.2 to 7.4 were read and found consistent with
  5.1. Its `audit.md`, `internal.md`, `external.md` were not re-read; its `critique.md` was only searched for its points
  on stage 0.
- **One code read:** `OwnBody.cs` (the links) and one line of `ProceduralBiped.cs`. The code that makes the drawn bones
  follow the links was not read.
- **No reference picture was opened** for this file. Readings of references A, B, E and F are part one's and the models
  research's.
- **The revision has not been re-read by a fresh critic.** The critic's points were weighed by the author of the file.
- **Session counts and totals are rough guesses,** from a mapping of part one's sizes, not from a plan. The 30 percent
  working ceiling is a proposal, not a measured need.
- **No design question was decided.** Proportions, heads, costumes, vents, the cut-away practice, the closed body, the
  creator's options, the UI language, water and the far land all stay Open. No register row was written.
- **Nothing was committed.** Only this file was written.

---

## 10. The critic's points and what was done

One line per point of `critique.md`. "Agreed" means the change is in the file; "partly" says what was not taken and why.

1. Hands, boots, ears and necks missing (high): **agreed.** Rows in 2.1, gap 6 in 2.2, a method in 3.1, G42, a close-up
   row on step 0; the models research's "keep hands and grips exactly" kept.
2. The favourite frame's protect list too short (high): **agreed.** Every gain touching it marked FF; listed in 7.1;
   the frame a control capture (6.1); G20 and G23 in the risk table.
3. The Strategy camera unweighted (medium): **agreed.** A "Strat." column in every gain table; C3, T7 and S2 restored
   into G0b; 2.2 says where each gap shows.
4. World gains dropped silently (medium): **agreed, mostly.** T6 added as G44 and cloth sheen as G45; B3, B6, V5, P6, X6
   and U5 listed "kept for later" with reasons (3.7). **Not agreed for C4:** it was already in G34; now named there.
5. The modelling file's first gain has no place (medium): **agreed.** G0d (the range-of-motion sweep) as the control;
   G43 (landmarks in the game), with the models research's placement in A2.
6. Step 0 judged on the flattest rendering (medium): **agreed.** One labelled "dressed" mock row (5.1), kept apart from
   the bare comparison so one variable stays per row; Phase D's captures may reach Luis before his pick.
7. "52 percent" called a cause (medium): **agreed.** Now "about half the linear resolution, about a quarter by area,
   Small only, rough"; the cause is a hypothesis; Long and Round added to G0c.
8. Heads-tall numbers without their weakness (medium): **partly.** Both columns and their sources are given and a mesh
   measurement is in G0c. **Not agreed** that there is an unexplained inconsistency: the skull-only column comes from
   the preset numbers, not by eye, and faces 3.1 says Round's curl buns add height, which explains her larger gap.
9. Hair "not a shell" overcorrected; no budget (medium): **agreed.** "Locks plus a cap plus metaball curls"; G3 given a
   budget rule and a LOD rule (3.4) and resized S to M.
10. "Small gains with large effect" unsupported (medium): **agreed.** Tier 1 renamed; G4 marked conditional; G9's
    rest-pose limit noted.
11. The BC7 hedge dropped in the short version (medium): **agreed.** Now "the code and the import file say Compressed
    HQ; the editor was not asked".
12. LOD triangles mislabelled (low): **partly.** Not mislabelled: the LOD totals (16,000, 5,000, 1,600) appear in mod 8,
    surf 6.2 and inv 1.1, and mod 2's 1,600 is the hands' budget inside LOD0, a coincidence of numbers. Labels made
    explicit; "triangles per part per LOD" added to G0c.
13. Sizes not converted (low): **agreed.** The mapping is stated at the top; the totals in 5.3 are ranges.
14. Gains scheduled before the ruling they ask for (high): **agreed that the text contradicted itself.** Decided: the
    standing rule (additive, switchable, off by default) suffices to build and show them; the column says "standing
    rule"; question 6 now asks only which he does not want.
15. Helper bones and correctives treated differently (medium): **agreed.** G25 and G26 carry the same ruling;
    question 5 covers both.
16. The closed body shown as decided (medium): **agreed.** Marked Open (MR Q3, Q7) in 3.1, the Start here and G27;
    vents and pose-driven shapes marked too.
17. Landmarks against "one body for everything" (medium): **agreed.** Landmarks live in the drawn surface only, at the
    same mean radius; a relative collider-distance bar (3.1, G0a, the spike); question 9 asks Luis.
18. Open answers bundled into Phase F (medium): **agreed.** Phase F names the answers each step needs; a get-up and
    idle gate on all of tier 3's miner rows.
19. "No download needed" beside two possible ones (low): **agreed.** "None needed; two optional, both Luis's"; the
    installed libraries named in 3.5.
20. Cost is one GPU number (high): **agreed.** CPU and a 100-miner all-on crowd in G0b and 6.2; a running sum; the
    ceiling is Luis's (question 7), with a proposed working bar until he sets it.
21. Versions not counted (high): **agreed.** The rung ladder (4.6): v1 to v4, gains inside a rung as one export, a
    12-set test matrix, keywords tested independently.
22. The idle protected only against regression (medium): **agreed.** An "idle unchanged" detector for every moving part
    (6.3), corrective weights exactly zero at rest.
23. Skeleton changes against the articulation builder (medium): **agreed, and partly answered.** `OwnBody` was read: it
    builds its links from named segments, not from the FBX's bones; the bone-follow code is left for G0c.
24. The real size not stated (medium): **agreed.** A total, the judges' count and cost, and three cut lines (5.3);
    question 12.
25. Protected workspace not addressed (medium): **agreed.** Section 7.3.
26. Judges not calibrated (low): **agreed.** Today's face as control and a degraded-copy check in every round (6.2).
27. Hair against the outline and paint filter (low): **agreed.** In the risk table and the detectors.
28. The order does not follow the ranking (medium): **agreed.** The trade and the dates are stated (5.2); the spike moved
    ahead of the world's first pass in phase E.
29. The reconcile rests on a partly read document (medium): **agreed and done in part.** MR 2 to 4 and 7.2 to 7.4 were
    read; no contradiction with 5.1; three findings used (the idle arm pose, the hands kept, strength only where the
    physics agrees). MR's `critique.md` only searched.
30. Count and labels (low): **agreed.** Tier 0 called instruments; G37 and G41 split; the count stated in section 4.
31. "No contradictions" too strong (low): **agreed.** Softened to "none found in these four areas"; the face-size
    disagreement added to 2.3.
32. Evidence marks mixed (low): **agreed.** A reading rule for double marks at the top; a "weakest link" line ends each
    method in section 3.
