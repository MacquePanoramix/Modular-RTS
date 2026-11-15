# The best visual quality: modelling the characters (body and garments)

November 15, 2026. Written by a research subagent for the deep dive Luis asked for on November 14
(`Docs/Correspondence/2026-11-14_THE_MODELS_PROPOSAL_ANSWERED.md`). Nothing was run in Unity or Blender. Nothing was
committed. It builds on `inventory.md` (this folder) and on the models research
(`Docs/Research/2026-11-14_TheModelsRedrawn/`), and does not repeat their fault lists.

Nothing here is a decision. Luis decides. Every Open question stays Open (proportions, heads, costumes, the creator's
options, the cut-away practice). Every new look is an additive option beside the loved one.

**Tags.** Claims: (read in code), (measured) from a project log or a mesh, (seen in a picture), (record) from a
project document, (from a source Mn) with the source list in section 14, (hypothesis) my reasoning, untested.
Sources: read in full, read in part, search summary only, weak.

---

## 0. Short version

1. **Today's miners lose quality at three steps of the build, not in the idea.** Parts are made as tubes and blobs,
   fused by a voxel remesh, smoothed, then cut down to a triangle budget by a collapse decimate. The last step throws
   away every edge loop. So the joints have no rings to bend on, the toon bands fall on random triangles, and weights
   come from distance to capsules only. (read in code: `shapes.fuse`, `shapes.chain`, `rigging.simplify_to`,
   `rigging.capsule_weights`)
2. **The best method within Claude's tools is "form by code, topology by template".** The body's form stays procedural
   and comes from the presets the physics already uses, with anatomy landmarks added. A fixed quad topology with loops
   at every joint is wrapped onto that form. Every body then has the same vertices. That one fact gives clean bends,
   weights authored once, LODs that keep their loops, and a creator built on blend shapes. (hypothesis, built from
   sources M1, M14, M16 and the code)
3. **Garments come in two kinds.** Fitted parts (sleeves, trousers, bodices) are shells grown from the body's own
   topology. Loose parts (skirts, coat tails, aprons) are flat pattern panels, sewn and draped over the body by
   Blender's cloth, with vents where a tailor would put them for the legs. (hypothesis; M11, M18)
4. **A closed body sits under every garment.** It gives the legs a seat and a trunk, fills the coat on all fours, and
   gives the cloth something to lie on. It also lets `bodyFront` be measured on the body, not the coat. (record:
   models research 5.1; hypothesis for the rest)
5. **Joints bend well by four tools together:** loops placed for the joint, weights with a set blend zone, twist and
   helper bones driven from the physical bones, and corrective blend shapes driven by joint angle. Unity skins with
   four bones a vertex and linear blending on the PC tier. (read in code: `QualitySettings.asset` PC `skinWeights: 4`;
   from a source M7, M9, M12, M13)
6. **The creator should be one topology with blend shapes, bones moved (not scaled), bind poses recomputed, and the
   physics numbers derived from the same weights.** Unity's automatic Mesh LOD is not a fit for skinned miners.
   (from a source M6, M10; hypothesis for the recipe)
7. **What is hard for Claude:** the template's junction topology (shoulder, hip, groin), the beauty of the form, and
   cloth sims that behave. Each needs many render, look and fix loops and the aesthetic judges. Hand sculpting is not
   available; landmark-driven forms replace it.

The recommended order is in section 11. The ranked gains are in section 13.

---

## 1. What this file adds to the models research

The models research ranked the faults (waist, floating legs, cloth as boards, thin limbs, joints without structure,
flat profiles) and the ladder A1, A2, B, C. This file asks a different question: **how should a character be built
to reach the highest quality Claude can reach**, whatever rung Luis chooses. So it covers:

- the build method itself (section 3);
- topology for deformation (4);
- joints, correctives, weights and Unity's skinning limits (5);
- the body under the clothes (6);
- garments, including legs in motion beyond the flap mechanism already researched (7);
- LODs (8);
- one base body for any proportion, as the creator's foundation, and what the physics needs (9);
- what to keep (10), the method (11), the checks (12), and the ranked gains (13).

It does not cover faces and their painting, textures, shaders or light, except where the mesh decides them. Those
are other researchers' files.

---

## 2. Today's pipeline, as read

The scripts in the worktree (`D:/Dev/WG/Art/Blender/Worker`) and the main project's copy were compared. All twelve
files have the same content. `faces.py` and `weights.py` differ only in line endings (CRLF against LF). (measured:
`diff --strip-trailing-cr` reports no change)

| Step | What it does | Where | What it costs in quality |
|---|---|---|---|
| Skeleton first | A posed skeleton from the presets (height, trunk widths and depths per level, limb radii, joints by two-bone solving) | `body.Body` (read in code) | Nothing; this is the strength of the pipeline |
| Limbs and trunk pieces | Skin-modifier chains on joints with radii, then subdivision level 2, then a noise roughen | `shapes.chain` (read in code) | Radii nearly constant along a limb: tubes (record: models research 3) |
| Head, hands, small forms | Metaball clusters, meshed and kept as one piece | `shapes.blobs`, `body.head` (read in code) | Smooth, soft forms; triangle-rich isosurface topology with no loops around eyes or mouth (read in code) |
| Garments | Pieces joined, voxel remesh at 0.0055 m, smoothing (factor 0.5, up to 6 rounds), then decimate to a face budget | `shapes.fuse` (read in code; voxel size from inventory) | Smoothing shrinks thin parts (the pinched waist, record: models research 3); remesh gives uniform, direction-less topology |
| Joining and budget | Each part skinned, then collapse-decimated to share the 16,000-triangle LOD0 budget; hands keep 1,600 | `rigging.prepare`, `simplify_to` (read in code) | **Collapse decimation produces irregular triangles with no edge loops.** Any loop the skin modifier made is lost |
| Weights | Per part, by distance to bone capsules (inverse distance to the 5th power, best 3 bones, then at most 4) | `rigging.capsule_weights`, `limit_influences` (read in code) | No authored blend zone per joint; the zone width depends on the capsule radii, not on the anatomy |
| Layered things | Laces, straps, buttons, patches, the apron take their weights from what they lie on, point by point | `rigging.LIKE`, `weights_like` (read in code) | A good rule; keep it |
| Skirt | Above the pelvis it follows the trunk; below, four flap bones blend in, a flap wholly its own a hand above the hem | `rigging.skin`, the `skirt` branch (read in code) | Rigid plates (record: models research 2.3) |
| LODs | LOD1 and LOD2 are collapse-decimated copies of LOD0; LOD2 drops small parts and merges finger bones into the hand | `rigging.lod_copy`, `whole_hand` (read in code) | Loops lost again; LOD1 and LOD2 never audited (record) |
| UVs and texture | One unwrap, one 2048 atlas baked with Cycles (EMIT), the face projected from the front | `unwrap_atlas`, `bake_atlas`, `body.head` (read in code) | Outside this file's scope |
| Export | One FBX: armature plus three LOD meshes; `mesh_smooth_type='OFF'`; deform bones only; no animation | `rigging.export` (read in code) | Normals exported as they are; no smoothing groups; no blend shapes |
| Unity import | `importBlendShapes = false`, `importNormals = Import`, generic rig, no avatar | `Editor/MinerSetup.cs` lines 306-316 (read in code) | No corrective or face shapes can reach the game today |
| Unity skinning | PC tier `skinWeights: 4`; Mobile tier 2 | `ProjectSettings/QualitySettings.asset` (read in code) | Linear blend skinning, four bones a vertex |

Pictures confirm the result in poses: knees bend like a hose, elbows are smooth arcs with no point, the sitting
trousers fold at the knee with no kneecap or volume change (seen in a picture:
`Docs/Research/2026-11-14_TheModelsRedrawn/pictures/sheets/close_poses_Small.png`).

---

## 3. The methods for making the body

### 3.1 The candidates

| Method | What it is | Quality ceiling | Fits the game's rules? | What it needs from Claude's tools | How hard to reach top quality this way |
|---|---|---|---|---|---|
| **A. Script-built, as now** (chains, metaballs, voxel remesh, decimate) | Forms grown from the skeleton and presets | Medium. Clean, soft forms; tubes; no loops | Yes: every module fits any body (record: `body.py` header) | Blender Python, headless. Works today (record) | Easy to keep; hard to lift further, because the last two steps erase the structure that deformation and shading need |
| **B. A sculpted base body** | An artist sculpts and retopologises a body by hand | High | Only as a template: a hand-made body does not follow the presets by itself | Brush sculpting needs a 3D view; it is unconfirmed in background mode (inventory 3.1). Claude cannot sculpt with artistry | Not reachable by hand. Reachable only as "a template wrapped onto a coded form" (method E) |
| **C. A ready parametric body** (MakeHuman/MPFB, Anny, SMPL) | A tested human mesh with morph targets for age, weight, muscle, height, and a rig | High for realistic bodies | Partly. Realistic proportions and anatomy; the miners are stylised and "drawn" (record: VisualSoul.md). Exaggeration by preset would fight the realistic morphs | A download: **needs Luis's approval.** MPFB is a Blender add-on (GPL code, CC0 assets, M3). Anny is Apache 2.0 and built on MakeHuman assets (M5). SMPL is research-only unless a paid licence is bought (M4): **not suitable** | Medium. The topology and rig are good; making it look like our miners is the hard part. Licence checks per asset (CC-BY items exist beside CC0 ones, M3) |
| **D. Geometry nodes** | Node trees that build or change meshes; evaluated from Python | Same as what the nodes build | Yes | Blender 4.5 headless; the evaluated mesh is read by `evaluated_get(...).to_mesh()` (M15). Stability in background mode untested here (inventory 3.2) | A tool, not a method. Best uses: Curve to Mesh for limbs with a set number of rings; seams, stitches, hems and patch borders; scattering wear. It does not solve junction topology |
| **E. Form by code, topology by template** (recommended) | A procedural form (the presets plus anatomy landmarks) and a fixed quad template with designed loops, wrapped onto the form | High | Yes: the form still comes from the presets; every body shares one topology | Blender Python plus NumPy and SciPy (sparse solvers for a Laplacian-regularised wrap). A template, either written by Claude once or taken from a CC0 base mesh (M1, **needs Luis's approval**) | Hard but reachable. The wrap is standard maths. The template's junctions and the beauty of the form need many judged loops |

### 3.2 Why E, in more detail

- **The form stays the game's.** The presets already define the body the physics uses (record: models research 5,
  "the model's mesh never reaches the physics"). The form generator keeps reading them. So "one body for everything"
  holds: the drawn body and the physical body come from the same numbers.
- **Anatomy without sculpting.** The tubes come from near-constant radii (record: models research 3). The form can
  carry named landmarks as extra implicit shapes, pushed by each body's own idea: the deltoid, the elbow point, the
  forearm's swell and the wrist's taper, the kneecap, the calf, the ankle, the seat, the belly, the ribcage and the
  pelvis as two masses with the waist between them (source S6 and S7 in the models research). Each landmark is a
  small blob or a radius profile along the bone, not a brush stroke. Claude can write and judge these. (hypothesis)
- **The topology is designed once.** A quad template with the loop counts of section 4. It is wrapped onto any form
  by: placing landmarks on both (joint centres, section extremes); an RBF warp from those landmarks; then repeated
  projection onto the form with Laplacian smoothing, so the loops stay even. This is how a scan is wrapped by a
  template in film and games tools (hypothesis from general practice; no source read on the specific tools).
- **Same vertices for every body.** Any two bodies become blend shapes of each other. Weights, UVs, correctives and
  garment fits authored once carry over. This is the foundation section 9 needs.
- **What is lost:** the voxel remesh's freedom to fuse anything. Parts that are not body (boots, laces, buttons,
  props, hair) keep today's methods. Only the deforming skin and the garments move to templates.

### 3.3 The template: written by Claude, or a CC0 base mesh

| | Written by Claude | Blender Studio's Human Base Meshes |
|---|---|---|
| What | A generator that emits rings along each limb and the trunk, and hand-designed patches at the shoulder, armpit, hip and groin | A free bundle of closed base meshes, realistic and stylised, made for sculpting and multiresolution (M1, M2) |
| Licence | Ours | CC0 per the release report (M1, search summary only); the developer page does not state it (M2, read in part). **Check before use** |
| Download | None | **Needs Luis's approval** |
| Quality of junctions | Unknown until made; the hardest part of the whole method | Made by artists; likely good (hypothesis) |
| Fit to the miners | Designed for them (hands, boots and head joins match today's parts) | Needs adapting: hands, feet and head joins differ; stylised proportions differ from ours |

Suggestion (hypothesis): write the generator, and if Luis approves, use the base mesh as a reference to compare
junction loops against, not as the game mesh. That keeps the miners wholly our own.

### 3.4 The head

- Today the head is a metaball cluster with the face projected from the front (read in code: `body.head`). The face
  is paint, so the head's topology matters for three things: the silhouette (and the outline), the light bands, and
  any future expression.
- A head template with loops around the eyes and mouth costs little if expression may come later (inventory M8).
  Without expression, an even quad head with a clean jaw and neck line is enough. (hypothesis)
- **Head size.** Luis allows a change to be tried (record: the November 14 answers). On the mesh side it is one
  parameter of the form. On the physics side it changes `headHalf`, the head sphere, the head's mass and the centre of
  mass (record: models research 5.3). So a head change in stills is free; in the game it goes with the physics work,
  by his word.

---

## 4. Topology and edge flow for deformation

The rules below are common practice; the sources are of low to medium authority (M14; S44 in the models research).
They are starting points to test on our bodies, not laws.

| Zone | Rule | Why |
|---|---|---|
| Elbow, knee | At least three loops of faces across the joint; two or three tight rings on the bending side, one or two wider rings on the stretching side | The bend has rings to fold on; the outside keeps its curve (M14) |
| Elbow point, kneecap | One loop that outlines the point, so a corrective shape can push it out when bent | The bone shows when bent; the reference guide asks that the elbow keep jutting out when the arm bends (M14, Reallusion guide, read as summary) |
| Shoulder | Loops that circle the deltoid; an extra stack for the collarbone's lift; the armpit as its own patch | The shoulder has the widest range and the worst collapse (M14) |
| Hip and groin | Loops that circle each thigh at the top, a patch at the groin, loops across the seat | The get-up folds the hip to 172 degrees (measured, models research 2.4). With joined legs the hip will show (record: models research 5.1) |
| Wrist, ankle | Two or three rings; forearm twist spread over the forearm's length | The forearm turns; spreading the twist avoids the candy-wrapper (M13) |
| Poles (points where 3 or 5+ edges meet) | Never on a hinge line; put them on flat, slow parts (the side of the chest, the back of the thigh) | Poles pinch and shade badly when bent (M14) |
| Fingers | One loop per phalanx, denser at the knuckles | Already handled by the 1,600-triangle hand budget (read in code); keep |
| Even spacing elsewhere | Quads near square; density by how much a zone bends, not by its size | ModelQualityMethod.md already says it: "Triangles are budgeted by what a part must do" (record) |

**For the painted look, topology also shapes the light.** The toon-banded main light in `WGPainted` puts a hard edge
where the shading crosses a threshold (record: inventory 1.1). On an irregular triangulation that edge zigzags. On even
quads with smooth normals it follows the form (hypothesis; not yet seen side by side). Section 5.6 has the normals.

**Triangle budget.** Even quads cost more triangles than a collapse decimate for the same silhouette. A first guess:
a body skin of about 6,000 to 9,000 triangles at LOD0, plus garments, hands and props (hypothesis). The measured
headroom (7.0 ms with 100 miners, inventory 1.0) suggests the LOD0 budget could rise for close views, but the target
hardware is not set. That is Luis's call.

---

## 5. Joints that bend well

### 5.1 What Unity gives

- **Linear blend skinning, four bones a vertex** on the PC tier (read in code). The setting can be One, Two, Four or
  Unlimited, and more than four costs more (M7, M8). The importer can also cut bones per vertex in the mesh data
  (M7, read in part).
- **No dual quaternion skinning built in** (hypothesis, from M9 and M13: open-source replacements exist, which
  implies it is missing). KosRud's DQ skinning for Unity is an MIT-style compute-shader replacement that keeps blend
  shapes (M9, search summary only). Using it is a download: **needs Luis's approval.** DQ removes the twist collapse
  but bulges at bends (S42 in the models research), so it is not a free win.
- **Blend shapes** work on skinned meshes; they are off at import today (read in code). Imported blend-shape normals
  have known pitfalls (artefacts when normals are imported and blend-shape normals calculated; a "legacy" option and
  fixes in 6000.5 are reported, M21, search summary only). A toon shader shows normal errors clearly, so this needs a
  test in Unity before any shape is relied on. (hypothesis)

### 5.2 Twist and helper bones

- **Twist bones** split the forearm's turn (and the upper arm's and thigh's) over two or three bones, each taking a
  share. The collapse is spread out, not removed (M13).
- **Helper bones** sit at a joint and turn part of the way (often half) with it, holding volume at the elbow, knee and
  hip (M13).
- **How it fits our game:** the physics drives the 19 main bones. Helpers are not driven by physics; a small script
  sets them from the main bones' angles, after the physics writes them and before rendering. They are deformation of
  the skin, like a muscle bulging, not motion. (hypothesis) A known pitfall applies: "never write an unchanged bone
  each frame" (record: auto-memory). Helpers are new bones, so they do not fight the existing writes.
- **Cost:** a few bones per limb, more skinning work, all on the GPU. Small next to the measured headroom (hypothesis).

### 5.3 Corrective blend shapes driven by joint angle

- A corrective is a shape that turns on as a joint bends, fixing what linear skinning gets wrong: the kneecap and the
  elbow point pushed out, the crease pulled in, volume put back (M12).
- Shoulders and hips move in more than one direction. Their correctives are weighted by distance to a few set poses,
  often with radial basis functions (M12, search summary only).
- **How Claude can make them without sculpting** (hypothesis, to test):
  1. Pose the rig in Blender (elbow at 45, 90, 135 degrees; knee likewise; hip flexed and opened; shoulder raised).
  2. Make a goal shape by rule: Blender's Corrective Smooth modifier on the posed mesh, or a ring-by-ring volume
     restore (each loop's cross-section area put back to its rest area), plus the landmark push (the point out on the
     outside, the crease in on the inside).
  3. Store the difference, in rest space, as a shape key.
  4. In Unity, a script reads each joint's angle and sets the shape's weight.
- **Fits the rules:** the weight follows the body's own joint angle every frame. Nothing is played. It is the skin's
  answer to the bone, the same each time. (hypothesis; judges and Luis decide whether it reads as real)
- **Cost:** each active shape adds GPU work; only joints that are bent need non-zero weights (hypothesis on Unity's
  handling of zero weights).

### 5.4 Weight quality

- Today: per part, inverse distance to capsules (read in code). It works, but the blend zone at a joint is set by the
  capsule radii, and parts weighted apart can disagree where they meet.
- Better (hypothesis):
  - Weight the **closed body** once, on the template, with an authored blend zone per joint (width as a share of the
    limb's length, centred on the joint, wider at the hip and shoulder, narrow at the knee).
  - Smooth on the surface (geodesic), not in space, so a hand's weights never leak into a thigh it rests on.
  - Normalise; limit to four; prune tiny weights.
  - **Garments take the body's weights** by nearest point, as straps already do (read in code: `weights_like`),
    then the skirt rules on top. One field for all layers keeps the layers moving together (record: CharacterPractices
    "What lies on something takes its weights").
  - With one template for every body, these weights are authored and judged once.
- Blender's automatic "heat" weights need a closed, clean mesh; today's separate parts would not suit them, the
  closed template would (hypothesis).

### 5.5 The skeleton itself

- Keep the 19 body bones, the fingers, the prop bones (read in code). They are what the physics drives.
- Add: twist bones (forearm, upper arm, thigh), helpers (elbow, knee, hip back, shoulder), and the skirt chains of
  rung A2 (several bones per flap; S21 to S23 in the models research).
- Not added: a separate visual skeleton with different joint places. The drawn joints must sit where the physical
  joints are (record: `rigging.dimensions`, "its joints land where the model's bones are").

### 5.6 Normals (a mesh matter that the toon look depends on)

- **Light bands.** Toon games edit vertex normals so bands fall in pleasing shapes, especially on faces, and use
  per-vertex values to shift the band threshold (M20, community summaries only; the details disagree). With a clean
  template, Claude can transfer normals from a simpler smooth proxy (an ellipsoid for the face, a smoothed body for
  the trunk) by script. (hypothesis)
- **Outline.** `WGOutline` pushes back faces out along the mesh normal (read in code). Where normals split (hard
  edges, seams), the hull opens. A smoothed normal stored in a spare vertex channel, read by the outline shader,
  is the usual fix (hypothesis from general practice; not checked whether today's miners have split normals).

---

## 6. The body under the clothes

### 6.1 Why

- The floating legs, the sack on all fours and the thigh through the coat share a cause: there is no body under the
  coat (record: models research 3).
- The references show loose cloth over honest bodies (record: models research 4.1).
- Shipped practice: whole legwear under loose cloth is the safest answer; a base body under per-garment meshes is the
  other common one (S16, S26, S27 in the models research).

### 6.2 What it is

- **One closed body skin** from the template: trunk, seat, hips, legs, arms, neck, joined to today's head, hands and
  boots.
- **Painted as its underlayer**: shirt and trousers where garments open, skin where skin shows (neck, forearms when
  rolled, hands). (hypothesis)
- **What may be removed under a garment:** only faces that no swept pose and no camera ever sees. This is computed,
  not guessed:
  1. Sweep the poses the game reaches (the audit's poses, the get-up's real extremes, walking, kneeling, all fours).
  2. Render or ray-cast from many directions (the 25 capture ways already used, record: Validation.md).
  3. A body face is hidden only if it is covered in every pose from every direction, with a safety margin.
  Under a coat skirt that is nothing below the hip. Under a fitted sleeve it is most of the upper arm. (hypothesis)
- **The cut-away practice** ("Trousers under a closed coat ... cut away above the knee", record:
  CharacterPractices.md:25) is replaced by this test only with Luis's word (Open question 7 of the models research).

### 6.3 What the physics gains

- `bodyFront` and the prop stops can be measured on the body, not the coat (record: models research 5, the
  `bodyFront` row). That brings the hip capsule closer to the body that touches (S41 in the models research).
- Worn mass can be computed per garment exactly, as now (read in code: `weights.py`).

---

## 7. Garments

### 7.1 Two kinds of garment

| Kind | Examples | How it is made | Why |
|---|---|---|---|
| **Fitted** | Sleeves, trousers, the coat's upper body, a bodice, socks | A shell grown from the body's own faces: copied, pushed out by the looseness, thickened at the rim; given its own folds | It has the body's loops, so it bends where the body bends; its weights match exactly; it fits any body on the template (hypothesis) |
| **Loose** | A coat's skirt and tails, a smock below the chest, an apron, a scarf | Flat pattern panels, sewn at their seams, draped over the body in the rest pose by Blender's cloth, then frozen | Cloth hangs by its weight: real folds, real hems, no boards (hypothesis; M11) |

### 7.2 Pattern panels and the drape, by script

- Blender's cloth has sewing springs since version 2.70: loose edges between panels pull the seams together (M11,
  search summary only).
- Reported limits: sewing can be unstable at real scale, and it is hard to predict how a flat change alters the fit
  (M11). So the script should place the panels already close to the body (wrapped round it, not flat in the air),
  sew with a low force, and run many short, checked steps. (hypothesis)
- **The draped mesh is already clean topology.** A panel made as a grid of quads stays a grid of quads when it is
  draped. No retopology is needed; the LODs come from coarser grids (section 8). (hypothesis)
- **Big folds in the mesh, small folds in paint or a normal map.** Folds that touch the silhouette must be geometry;
  folds inside the shape can be painted, which is the look the guides ask for (record: VisualSoul.md, "drawn, not
  only modelled"). A normal-map bake of the small folds is another researcher's subject (inventory rank 2).
- Paid tools that do this in a viewport (Simply Cloth, Marvelous Designer) exist; both would be downloads, **need Luis's
  approval**, and are made for hand use, not scripts (M11). Not recommended.
- **The look goes to the judges.** A real drape is not automatically pretty. The references' cloth is loose and
  soft but simplified (record: models research 4.1).

### 7.3 Garments made as a maker makes them: vents

- A tailor puts a vent (a slit at the hem) in a coat to let the legs move; riding coats have a back slit so the coat
  can fall either side of the saddle (M18, search summary only).
- The guides already ask that things be "made as a maker makes it" (record: ModelQualityMethod.md).
- So the long coat's skirt could open at the back and sides, with the flap edges overlapping at rest. A raised or
  opened thigh then passes between two panels instead of through one. Each panel hangs on its own chain. Whether the
  miners' coats should have vents is a costume choice: **Open, for Luis.** (hypothesis that it helps the clipping)

### 7.4 Garments over legs in motion, beyond the models research

The models research chose flap bones that follow the thigh, more bones per flap, spread weights and leg capsules
(rung A2, S21 to S23). Added here:

| Option | What it is | Physical? | Tools and cost | Verdict (hypothesis) |
|---|---|---|---|---|
| **A. Simulated chains** (A2) | Each panel on a chain of bones, each bone a pendulum with gravity, stopped by thigh and shin capsules | Yes: gravity, inertia, contact | C# in the game, like today's `MinerBody.Drape` and `Hang` (record) | The base. Keep |
| **B. A cage cloth** | A coarse hidden cage of particles (a ring of 12 to 16 points per row, 3 to 4 rows) simulated with distance constraints and leg capsules; the visible skirt skinned to the cage | Yes | A small cloth solver in C#. The game's rule against DOTS (AGENTS.md) argues against Burst-based packages; plain C# on 100 miners is untested | Better folds and overlap than chains; real work to make stable; a step after A |
| **C. Pose-driven garment correctives** | Blender cloth runs over the body in set poses (thigh raised, sitting, kneeling, all fours); the frozen results become shapes; the game blends them by the hip and knee angles | It is the cloth's real answer at that pose, read back; but nothing swings | Blender cloth headless (record: inventory 3.2) plus blend shapes in Unity | Fixes the sack and the tents where chains cannot. **Whether this counts as physical is for the judges and Luis** (rule: no fake animations). Pair it with A for the swing |
| **D. Unity's Cloth component** | Built-in vertex cloth on a skinned mesh, with sphere and capsule colliders | Yes | Old system; users report it is poor for detailed clothes (M19, anecdotal) | Not recommended |
| **E. Magica Cloth 2** | A paid Unity package for bone and mesh cloth | Yes | Built on DOTS and Burst (M19), which the project excludes without a milestone; a paid download, **needs Luis's approval** | Not recommended |

Rules for all of them (record and hypothesis):
- The garment is a little larger than the legs; tight skirts over legs fail (S27 in the models research).
- Weights are spread at seams; a flap never on one bone (S22).
- Test crouching, kneeling, sitting, lying and all fours, not only walking (S22); for us, the get-up's real angles.
- The getting up must still work for any body: the cloth is visual and must never push the physical body.

---

## 8. LODs

- **Today:** collapse-decimated copies of LOD0; LOD2 drops details and merges finger bones; 16,000, 5,000 and 1,600
  triangles (read in code). LOD1 and LOD2 never audited (record).
- **Unity's automatic Mesh LOD** (Unity 6.2 on) does not suit the miners: it ignores skin weights and blend shapes
  when it simplifies, can remove points a bend needs, and still deforms LOD0 whatever level it draws; it is not
  recommended beside a LOD Group (M6, read in part). Keep the LOD Group with authored levels.
- **With a template:** each LOD is the same template with fewer rings (the generator emits, say, 3 rings at a knee at
  LOD0, 2 at LOD1, 1 at LOD2). Loops survive, UVs stay in the same place so the one atlas serves all levels, weights
  come from the same field, and the outline's smooth normals are made per level. (hypothesis)
- **Garment panels:** coarser grids of the same pattern, draped by the same run or decimated by grid. (hypothesis)
- **Correctives and helpers** only at LOD0 and perhaps LOD1; far away they cost and do not show. (hypothesis)
- **A closer level for the cinematic view?** Every in-game view counts, the cinematic one too (record: auto-memory).
  A denser LOD0 used only below a set distance is possible with the measured headroom; it is Luis's call with the
  target hardware. (hypothesis)

---

## 9. One base body for any proportion (the creator's foundation)

### 9.1 What the physics needs, and from where

- The physical body is built from the proportions (23 numbers), the masses per bone and the limb radii; the mesh
  never reaches it, except `bodyFront`, `faceFront`, `headHalf`, worn mass and `skirtSlack` (record: body_map.md and
  models research 5).
- So a creator body needs, from one source, both the drawn body and those numbers. The body map compared four ways:
  scale a miner's bones (shears: rejected), a Blender build per body (faithful, minutes per body), new proportions on
  an old prefab (inconsistent: avoid), and a made body of primitives (cheap, faithful, but not the drawn miner)
  (record: `Docs/Design/AnyBody/body_map.md` 3.3).

### 9.2 The way a shared topology opens

With one template, a fifth way exists (hypothesis, to be tested in Unity):

1. **Shapes.** In Blender, build the template onto a set of corner bodies: the three miners, plus a few extremes along
   the creator's axes (taller, heavier, stronger, as Luis chooses). Store each as a blend shape of one neutral body.
2. **Bones moved, never scaled.** For each shape, store how far each joint moves (the joint places are a linear
   function of the shape weights, as body models compute joints from the mesh; M5 and M3 describe such models). The
   creator sets the bones' rest places from the weights.
3. **Bind poses recomputed.** A mesh's bind poses are the inverse of each bone's rest matrix and can be set from
   script (M10). After moving the bones, the creator writes new bind poses on its own copy of the mesh, so the
   reshaped skin sits on the moved skeleton. A forum answer says re-binding is not offered out of the box and
   suggests baking (M10). For a creator, baking once is fine: apply the shapes into the copy's vertices, set the new
   bind poses, and drop the shape weights. Nothing changes per frame.
4. **Physics numbers from the same weights.** The 23 proportions and the radii are read from the moved joints and
   the measured sections; the masses are computed on the reshaped body by the same rules as `weights.py`, ported to
   C#. A test checks the three miners against Blender's numbers within a set tolerance (hypothesis: 1 to 2 percent).
5. **Garments follow.** Each garment carries the same shapes, made in Blender by fitting it to each corner body
   (fitted parts by their body faces; loose panels by re-draping). Games either copy body morphs onto garments and
   drive them together, or warp the garment by the difference between two bodies (M16). With a shared topology the
   first is exact for fitted parts. Blender's Surface Deform modifier is one way to carry a garment along; reports
   disagree on whether it follows shape keys (M17), so test it before use.
6. **Bounds.** The weights stay inside a range where standing, stepping, falling and getting up have been verified
   (record: models research 6.4; S36 in the models research). Outside the corners, blend shapes extrapolate badly.

What it gives: any body inside the corners, built in milliseconds, with the drawn body and the physics from one
source. What it costs: the corner bodies are many Blender builds; every garment needs its shapes; a texture painted
for one body stretches on another (within bounds, probably fine; hypothesis).

### 9.3 Bodies that change shape during play

- **At the creator** (once, at rest): the recipe above.
- **In play** (if the game ever wants a body that grows stronger, ages or tires visibly; nothing like this is asked):
  - Visual-only changes (breath, a flushed or slumped look) are blend shapes driven by the body's state. The breath
    must stay gentle and scaled (record: auto-memory, "Breath gentle and scaled").
  - Changes that reach the physics (mass, joint places) must reconfigure the articulation. Changing link masses and
    inertia is supported on articulation links; moving joint anchors under a moving body risks a jolt. Safer: change
    only at rest, then rebuild the links (hypothesis; the known pitfall that turns set on a root before its links are
    lost applies, record: auto-memory).

### 9.4 Two ways for the creator's mesh, compared

| | Blend shapes on one topology (recommended) | Port the generator to C# and build at runtime |
|---|---|---|
| Quality | All Blender tools (drapes, correctives, judged corners) | Only what the C# code can do; no cloth drape at runtime |
| Range | Inside the corners | Any numbers |
| Two codebases? | No | Yes: Blender and C# must agree forever |
| Cost | Many Blender builds; blend shapes in Unity | A big port |

Which options the creator offers, and how far they reach, is Open: "some, not too many", "smaller scale, not smaller
quality" (record: Luis, November 14). This section only says what foundation would carry them.

---

## 10. What to keep from today's pipeline

The pipeline's spine is good. The new method replaces only the deforming skin's topology, the weights and the loose
cloth. Keep (read in code unless marked):

- **The presets as the single source of truth**, and the `Body` class that solves the posed skeleton from them.
  The physics and the drawing stay one body.
- **The any-body modules**: every function works on any body (record: `body.py` header).
- **`Surface`** (BVH cast, nearest, lay) and **`lay_on`**, **`taut`**: the right tools for things that lie on others.
  They also serve the garment fit and the clipping checks.
- **`weights_like`** and the `LIKE` list: what lies on something takes its weights. Extend it to every garment.
- **Rigid props on their own bones**, held and hanging (record: auto-memory, "Physical objects").
- **Hands**: their 1,600-triangle budget and finger bones; `whole_hand` at LOD2.
- **Boots, laces, buttons, straps, patches**: small parts made as today, laid on the new surfaces.
- **The atlas bake (Cycles EMIT) and the painting recipes; the face stroke painter.** The template would make UVs
  steadier, but the painting is other researchers' subject.
- **The export conventions** (axes, deform bones only, no leaf bones) and the **dimensions manifest** that tells the
  game where the joints are.
- **`audit.py`, `sweep.py`, the models research's audit tools** (`render_miner.py`, `measure.py`, `silwidth.py`,
  `sheet.py`), the golden check, and the switch between versions. The new body is built beside the old one and
  chosen by a switch (rule: explorations are additive).
- **The metaball head for now.** Luis likes today's faces (record). A head template is an option, not a replacement.

Drop or replace (hypothesis):
- collapse decimation of the deforming skin (replaced by ring counts);
- voxel remesh plus smoothing for garments that deform (replaced by shells and panels); keep it for rigid lumps;
- capsule weights as the final weights (keep them as a first guess and a fallback);
- the four single-bone flaps (replaced by chains, as the models research proposed).

---

## 11. The recommended method, with reasons

Each step is additive: the new body sits beside today's and is switched on to compare. Each step ends in pictures,
judges, then Luis. Order follows the models research's path (stills first), with the building method under it.

| Step | What | Why first | Claude's tools | How hard for top quality |
|---|---|---|---|---|
| **1. The form** | Add anatomy landmarks to the form generator: radius profiles along each bone (deltoid, forearm swell, wrist taper, thigh, kneecap, calf, ankle), the ribcage and pelvis masses, the seat. Still today's topology | Shape is the first thing seen; it answers the models research's thin limbs and tubes; stills are cheap | Blender Python, renders, the sheets | Medium. Beauty needs many judged loops; no sculpting |
| **2. The template** | A generator for one quad skin with the loops of section 4, and a Laplacian-regularised wrap onto the form | Every later gain needs it | Python, NumPy, SciPy sparse solve; wireframe renders | **Hard.** The shoulder, armpit, hip and groin patches are the riskiest part of the whole plan. A CC0 base mesh as a reference only would help (**needs Luis's approval**) |
| **3. Weights and helpers** | Weights on the closed template with set blend zones; twist and helper bones; garments take body weights | Clean bends at once; cheap after step 2 | Python in Blender; in Unity, a small C# script after the physics writes | Medium |
| **4. The range-of-motion sweep** | Pose sheets of every joint to its game extremes, on all three miners | Shows where linear skinning fails before any fix is made | The existing audit tools, extended | Easy |
| **5. Correctives** | Shapes made by rule (volume restore, point out, crease in) at the poses step 4 finds; driven by joint angle | The last 20 percent of a good bend: points, creases, volume | Blender Python; Unity blend-shape import switched on, normals tested | Medium; the Unity normal pitfalls need a test (M21) |
| **6. The body under the clothes** | The closed body everywhere; faces hidden only where the coverage test proves them covered | Removes floating legs and the sack at the root | Ray casts in Blender over the swept poses | Medium |
| **7. Fitted garments as shells** | Sleeves, trousers, the coat's top from the body's faces | Bend with the body; fit any body | Python | Medium |
| **8. Loose garments as panels** | Patterns sewn and draped by Blender's cloth; vents where a maker would put them (**Open, for Luis**); chains in the game | Real hang and folds; legs between panels | Blender cloth headless (record: inventory 3.2); C# chains | **Hard.** Cloth sims are touchy; the drape must also be pretty |
| **9. LODs** | Fewer rings and coarser grids; the audit at each switch distance | Far bodies must read as the same miner | Python; the sheets | Easy to medium |
| **10. Normals** | Face and trunk normals from smooth proxies; smooth outline normals in a spare channel | Cleaner light bands and outline | Blender Python; one shader line in `WGOutline` | Easy to medium; must be seen in Unity's own render |
| **11. The creator's foundation** | Corner bodies as shapes; joint regressor; bind poses; physics numbers from the same source | Only after the three miners are good | Blender Python; Unity C#; tests | Hard but well defined |

Reasons, in short:
- **It keeps the game's one body.** The form still comes from the numbers the physics uses (rule: one body for
  everything).
- **It moves the quality into the steps Claude does well**: rules, maths, measurement and many careful render-and-read
  loops. It avoids what Claude cannot do: brushing a sculpt by eye.
- **One topology pays many times**: weights, correctives, garments, LODs and the creator all reuse it.
- **Every step can be stopped and still leave a gain.** Steps 1 and 4 need no new topology at all.
- **Nothing is fake.** Helpers and correctives follow the body's own joints each frame; cloth hangs by gravity in
  the game. Step 8's pose-driven garment shapes (section 7.4 C) are flagged for the judges and Luis.

Not recommended (with reasons above): a realistic parametric body as the game mesh (it fights the drawn look; licences
vary; SMPL is not licensed for this); paid cloth tools; Burst or DOTS cloth packages; Unity's automatic Mesh LOD for
skinned miners; dual quaternion skinning as the first fix (it bulges; a download).

---

## 12. How each step is checked

Rule: read the pictures before the numbers (record: auto-memory, "Read results before claiming"; "Every frame as
played"). Each step's check runs on all three miners, Small, Long and Round, because Long fails first (record:
auto-memory).

| Check | What it shows | Tool | Exists? |
|---|---|---|---|
| **Turntables** at rest, 8 to 16 angles, in the game's look and in plain grey | Form, silhouette, profiles | `render_miner.py` (read in code) | Yes |
| **Wireframe sheets** | Loops at joints, poles off hinge lines, even quads | A wireframe pass in the render script (hypothesis: a few lines) | Partly |
| **Weight-gradient renders**, one bone at a time | Blend zones where they should be; no leaks | A vertex-colour render (hypothesis) | No |
| **Range-of-motion sheets**: each joint at rest, 45, 90, 135 degrees and its game extreme (hip to 172 degrees, record) | Collapse, pinching, the elbow and knee points | `sheet.py` with posed armatures (read in code) | Partly |
| **Ring-area numbers**: each loop's cross-section area at a pose against rest | Volume loss as a number (a pipe knee loses area) | Python on the template's rings (hypothesis) | No; needs the template |
| **Clipping count**: body and garment points inside each other at each pose | Thigh through the coat; sleeve through arm | `Surface` and BVH queries (read in code) | Partly |
| **Coverage test**: each hidden body face never seen from 25 directions over the swept poses | That a removed face is never missed | Ray casts in Blender (hypothesis) | No |
| **Silhouette width and profiles** | Thin limbs, flat profiles | `silwidth.py`, `measure.py` (read in code) | Yes |
| **LOD silhouette overlap** at each switch distance (intersection over union of the outlines) | LOD1 and LOD2 read as the same miner | Renders and a Python image compare (hypothesis) | No |
| **Physics numbers** derived for the three miners against Blender's | The creator's foundation is faithful | A Unity edit-mode test (hypothesis) | No |
| **In the game**: captures of standing, the idle, stepping, falling and getting up, every frame and several angles | What Luis will see, including toon bands and outline on the new mesh | `MinerCapture` and the frame scans (record) | Yes; **only when Unity is free** |
| **Judges, then Luis** | Aesthetic and natural reading | Aesthetic judge agents against the guides (record: auto-memory) | Yes |

Stopping rule for each step (from ModelQualityMethod.md's done criteria, record): no defect of the top severity
left in any picture on any miner, and the misses ledger updated with what the step found.

---

## 13. Ranked quality gains

Ranked by quality raised for the effort. Sizes: S (a session or less), M (a few sessions), L (many sessions).
All are hypotheses until the pictures are read.

1. **Anatomy landmarks in the form** (deltoid, elbow, forearm, kneecap, calf, ankle, ribcage and pelvis): S to M. Ends
   the tubes; no new topology.
2. **A fixed quad template with joint loops, wrapped onto the form**: M to L. The foundation for clean bends,
   bands and every later step.
3. **Weights authored once on the closed body; garments take them**: S to M after step 2.
4. **Twist and helper bones driven from the physical bones**: S to M. Holds volume at elbows, knees and hips.
5. **The closed body under the clothes, with a coverage test for hidden faces**: M. Ends floating legs and the sack.
6. **Corrective shapes driven by joint angle**, made by rule: M. Points, creases and volume when bent.
7. **Smooth outline normals and proxy normals for the bands**: S. Cleaner outline and light on the same mesh.
8. **LODs from ring counts and grids, with an audit**: S to M. Far miners keep their shape.
9. **Loose garments as sewn, draped panels with vents (vents Open, for Luis)**: L. Real hanging cloth; legs pass between panels.
10. **The creator's foundation (shapes on one topology, regressed joints, new bind poses, derived physics)**: L.

---

## 14. Sources

Sources in the models research (S1 to S44) are cited as such and not repeated. New sources for this file:

| Id | Source | Used for | Read |
|---|---|---|---|
| M1 | Release reports on Blender Studio's free Human Base Meshes bundle (CG Channel; 80.lv) | Content and CC0 licence | Search summary only |
| M2 | Blender developer documentation page on the Human Base Meshes | What the bundle holds; licence not stated there | Read in part |
| M3 | MPFB 2 (MakeHuman plugin for Blender) project pages and licence notes | GPL code, CC0 base assets, some community assets CC-BY; glTF and FBX export | Search summary only |
| M4 | SMPL and SMPL-X licence pages, and Meshcapade commercial licence notes | Research-only licence; paid commercial terms | Search summary only |
| M5 | "Anny" human body model paper (NAVER LABS Europe, arXiv 2511.03589) | Apache 2.0 claim; built on MakeHuman; calibrated proportions. **The repository's licence was not checked** | Search summary only |
| M6 | Unity 6000.6 manual, Mesh LOD pages | Import-time generation; ignores skin weights and blend shapes; skinned meshes deform LOD0; not with LOD Group | Read in part |
| M7 | Unity 6000.6 scripting reference, `ModelImporter.maxBonesPerVertex` | Range, effect on mesh data, runtime limits elsewhere | Read in part |
| M8 | Unity documentation on `QualitySettings.skinWeights` and `SkinnedMeshRenderer.quality` | One, Two, Four, Unlimited; cost above four | Search summary only |
| M9 | KosRud, DQ skinning for Unity (open-source repository); TriceHelix BurstSkinning | A dual quaternion replacement keeping blend shapes; licence described as MIT-style | Search summary only |
| M10 | Unity forum threads on re-binding a skinned mesh at runtime | Setting `Mesh.bindposes`; baking as the workaround | Search summary only; weak |
| M11 | Blender manual and forum threads on cloth sewing springs; Simply Cloth and Marvelous Designer pages | Sewing since 2.70; instability at scale; paid tools | Search summary only |
| M12 | Pose space deformation literature and patent summaries | Correctives driven by joint rotation, radial basis functions | Search summary only |
| M13 | Game-rigging articles on twist and helper joints, candy-wrapper collapse, DQ bulging | Twist and helper bones | Search summary only |
| M14 | Topology guides for deformation (forum posts; a character-creator vendor's topology guide) | Loops per joint, deltoid and clavicle, poles, the elbow point | Search summary only; low authority |
| M15 | A third-party guide to evaluating geometry nodes headless in Blender 4.x | `evaluated_get(...).to_mesh()`, `to_mesh_clear()`, `temp_override`, timers in background mode | Search summary only |
| M16 | Epic's documentation on MetaHuman parametric clothing; a Diffeomorphic issue on garment morphs | Warping a garment from one body to another; garment morphs driven by body morphs | Search summary only |
| M17 | Blender forum threads on Surface Deform with shape keys | Binding needs well-matched meshes; whether it follows shape keys is disputed | Search summary only; weak |
| M18 | Encyclopaedia entry on vents in tailoring | Slits at the hem for movement; riding coats' back slit | Search summary only |
| M19 | Magica Cloth 2 store and documentation pages; Unity forum threads on the built-in Cloth | Built on Jobs, Burst and DOTS; built-in Cloth weak for detailed clothes | Search summary only; forum weak |
| M20 | Community write-ups on Guilty Gear Xrd's art pipeline | Edited normals, vertex-colour thresholds, inverted hull | Search summary only; weak |
| M21 | Unity issue tracker entries on blend-shape normals | Artefacts with imported normals and calculated blend-shape normals; a fix in 6000.5 | Search summary only |

Project sources read: `inventory.md` (this folder); `Docs/Research/2026-11-14_TheModelsRedrawn/research.md` (read in
full) and its pictures and audit tools; `Docs/Correspondence/2026-11-14_THE_MODELS_PROPOSAL_ANSWERED.md`;
`Docs/Design/AnyBody/body_map.md` (section 3.3, read in part); `Docs/ArtDirection/ModelQualityMethod.md` (read in
part); the worker scripts `rigging.py`, `shapes.py`, `body.py`, `outfits.py` (read in part); `Editor/MinerSetup.cs`
(import settings); `WGOutline.shader`; `ProjectSettings/QualitySettings.asset`.

---

## 15. What was not done

- Nothing was run in Unity or Blender. No wrap, cloth sim, corrective, weight or LOD test was made. Every gain in
  section 13 is a hypothesis until its pictures are read.
- Nothing was downloaded. The CC0 base meshes, MPFB, Anny, DQ skinning and any cloth tool all **need Luis's
  approval** before use.
- Most outside sources were read as search summaries only. The Anny repository's licence and the Blender base meshes'
  licence on their own page were not checked.
- Untested: whether Surface Deform follows the target's shape keys; whether sculpt brushes run in background mode;
  whether today's miners have split normals that open the outline; whether Unity drops cost for zero-weight blend
  shapes.
- No triangle budget was set for the target hardware; that is Luis's call.
- Faces, textures, shaders, light and hair were left to the other researchers' files.
- No design question was decided: proportions, heads, costumes, vents, the cut-away practice and the creator's
  options all stay Open.
