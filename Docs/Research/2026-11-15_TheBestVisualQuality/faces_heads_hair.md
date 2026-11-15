# Faces, heads and hair: the best way, at the best quality

November 15, 2026. Part of the deep dive Luis asked for on November 14 (night): "a super deep dive research of what
is the best way of doing the models and all the visual parts". On the faces he said he likes today's and wants ways
to make them even prettier, by the aesthetic; head sizes "can try a change".

Written by a research subagent. **Nothing was run in Unity or Blender.** The evidence is code read in the worktree
`D:/Dev/WG` (branch `claude/wip-round`), pictures already in the project, two small Python measurements on the
exported textures, and web research.

Tags on claims:
- (read in code) a file or setting was opened and read.
- (measured) a number taken now from a file with Python.
- (seen in a picture) looked at a picture in `Docs/Images` or the research folders.
- (record) written in a project document, not re-checked now.
- (from a source) from the web; each source is tagged in section 12.
- (hypothesis) my reasoning; it needs a test.

**Nothing here is a decision.** Head proportions, the cast and the faces' final look stay Open. Today's three faces
are kept as the reference look; everything below is an additive, switchable option shown beside them.

---

## 0. Short version

1. **Today's faces already follow the aesthetic.** Few drawn marks, wide-set eyes, a strong nose idea per miner, warm
   blush. That is the "feeling from very few marks" the guides ask for, and it matches references B and E. Keep it.
   (read in code, seen in a picture)
2. **Most of what keeps them from looking prettier is not the drawing. It is how the drawing is carried.**
   - The light on the face follows a lumpy metaball surface, so cheeks and jaw get blotchy bands. (seen in a picture,
     read in code)
   - The face gets about half of the painted face's resolution in the atlas. One eye fragment also sits on a second
     UV island. (measured, seen in a picture)
   - Skin, hair and cloth share one material in the game, so skin cannot have its own warmth and hair cannot have its
     own sheen. (read in code)
   - Hair is decimated tube locks, so it shows flat facets: a "3D giveaway" the frame test bars. (read in code, seen in
     a picture)
3. **Fix the carrier first, then explore the drawing.** Smooth face normals, a dedicated face texture with a skin
   material, and cleaner hair are small to medium jobs. They raise every face without touching a mark Luis loved.
4. **Prettier drawings come second, as stills beside today's.** The ideas, all from references B and E: a darker
   upper-lid mass, a larger dark iris share, softer noses blended into the face, a hint of lip colour, warm colour
   zones, and cheeks lifted by the form as well as by blush.
5. **Head size is a trade.** Bigger heads read younger and cuter, and they read better from the Strategy camera, where
   a head is about 10 px tall. Too big, and the miners stop reading as strong adult workers, which is the prototype's
   purpose. Today's heads are about 4.5 (Small), 5.8 (Long) and 4.1 (Round) heads tall with hair (rough, from the
   silhouettes). The proposal is stills at minus 8, 0, plus 8 and plus 15 percent. Proportions stay Open until Luis
   picks. A head change in the game goes with the physics work.
6. **Small living face movement can be the body's own.** Real blinks come with head turns and big gaze shifts, rise
   with tiredness, and squeeze at an effort. Hair can swing on springs driven by the head's real motion. Whether a
   blink fits "no fake animations" is a question for Luis.
7. **Judging faces needs set lenses:** a portrait, a conversation distance, the Strategy camera at its real pixel size,
   three hours of light, grey values, mirrored copies, and today's face beside every variant. Then aesthetic judge
   agents, then Luis.

---

## 1. What makes a stylised but natural face pretty here

### 1.1 What the game's own guides ask

- **Feeling from very few marks.** A face that "expresses much with almost nothing" (record: `VisualSoul.md`,
  principle 3).
- **Drawn, not only modelled.** Lively, slightly imperfect contours and confident flat shapes (record: principle 2).
- **Value design.** Confident dark shapes, such as hair, against lighter ones (record: principle 4).
- **Light finds them.** Warm light picks out a cheek (record: principle 7; reference B: "warm light finds the cheek").
- **Individual faces.** Different noses, shoulders and gestures make personality (record: `VisualSoul.md:129-130`).
- **No 3D giveaways:** no faceted silhouettes, no plastic highlights, no flat untextured areas (record:
  `TheEssence.md`, the frame test).
- **The register.** "A little stylized as long as it still reads natural" (record: Luis, November 13, said of motion).
- **Polish never makes a being generic.** The stylisation and appeal Luis loved stay (record:
  `ModelQualityMethod.md:53-54`).

### 1.2 What the two people references show (seen in a picture)

**Reference B** (`References/B_AFaceInWindowLight.jpg`, a young woman at a window):
- **Eyes:** large, dark irises fill most of the eye, with little white. The upper lid is one thick dark mass that
  carries the lashes. The lower lid is a faint line or nothing. One small catch light sits in each eye.
- **Eye line:** a little below the middle of the head. The eyes are about one eye width apart.
- **Brows:** short, soft, partly hidden by the fringe.
- **Nose:** almost only a tiny shadow and a lit tip. No bulb.
- **Mouth:** small, with a soft colour on the lips and a darker line between them.
- **Cheeks:** round, warm blush patches, placed low and out on the cheek.
- **Light:** flat, graphic light and shadow shapes on the face. A warm key on one cheek and a cool, still-coloured
  shadow side. Painted texture stays visible in both.
- **Hair:** one confident dark mass, a blunt but uneven fringe, a few loose strands and flyaways that break the
  outline, and inner strand lines that follow the flow. A lighter sheen on the crown.

**Reference E** (`References/E_ExpressiveDrawnLife.jpg`, two figures at a cottage door):
- **Caricature, not prettiness:** big bulb noses (red on the woman), small dark eyes, closed-eye contentment, a sly
  side smile on the man.
- **Faces carry the charm with very few marks.** Ruddy cheeks and nose, a few wrinkles.
- **Hair:** messy, with strands sticking out. It is never a helmet.
- **Head ratio, rough:** the round woman is about 4.3 heads tall and the long man about 5.3 heads (my rough read of
  the picture; hypothesis-grade numbers).

So "pretty" in this game has two poles: B's quiet, youthful prettiness and E's warm, characterful caricature. Small
leans to B. Round and Long lean to E. A prettier pass should move each miner toward its own pole, not all toward B.
(hypothesis)

### 1.3 What outside sources add

- **Shape language.** Rounder outlines read younger and friendlier. Sharper, more angular features read older.
  (from a source: practitioner tutorials, search summary only)
- **Baby schema.** A large round head, a high forehead, big low-set eyes and full cheeks raise ratings of cuteness.
  This is tested on real infant photos, kept within natural ranges (Glocker et al. 2009). The effect fades for faces
  older than small children. (from a source: search summary only) For adult miners, this is a dial to turn a little,
  not a goal.
- **Characters for younger audiences** were drawn by animators with larger head height, larger eye height and rounder
  eyes (Disney Research, "Designing Animated Characters for Children of Different Ages"). The same study found that
  artists' trends did not match what children preferred. (from a source: search summary only; the PDF could not be read)
- **Eye line.** A lower eye line, with a bigger forehead, reads younger. One tutorial also ties it to reading as
  cleverer. So one change shifts several readings. (from a source: tutorials, search summary only)
- **Skin colour zones.** A painter's observation: on a light-skinned face the forehead leans golden, the middle band
  (cheeks, nose, ears) reads redder, and the jaw leans cooler or greener on men. The painter himself warns it is a
  starting point, not a rule. (from a source: Gurney's blog, through mirrors, search summary only)

### 1.4 Where they meet: a working definition (hypothesis, to be shown, not decided)

A face is pretty here when:
1. **Few marks, each confident.** One upper-lid mass, one iris shape, one catch light, one brow stroke, a nose by its
   form plus one shadow, a mouth by one line and a touch of colour. No outlines around every feature.
2. **Soft, rounded planes.** Cheeks and jaw are smooth and continuous. Light falls on them in two or three clean
   shapes, not in blotches.
3. **Warmth.** Blush and a warmer middle band, so the skin is never one flat tone. "Light finds the cheek."
4. **Each miner's one idea, pushed:** Small curious and bright-eyed, Long kind and sleepy with the long nose, Round
   laughing with the bulb nose.
5. **A little asymmetry.** A crooked smile (Long's), one smudge of soot (Small's, Round's), one brow a touch higher, a
   lock out of place. Real faces and painted ones are never mirror-perfect. (from a source: general practice;
   hypothesis for our faces)
6. **Natural anatomy under the style:** a jaw that turns into the neck behind the ear, a skull behind the face, eyes
   that sit in the skull's front plane. (record: `CharacterPractices.md`, the neck rule)

---

## 2. Today's three faces: what works, what could be prettier

### 2.1 How a face is made today (read in code)

- `Art/Blender/Worker/faces.py` paints each face as brush strokes into a 1024 x 1024 texture with NumPy: tapered
  strokes with a small hand wobble, filled ellipses and soft "glow" washes for blush and soot. Three styles:
  `curious` (Small), `sleepy` (Long), `laughing` (Round).
- `body.head` builds the head from metaballs (skull, jaw, brow, cheeks, chin, ears, the nose from the preset, the neck)
  and keeps one piece. The face texture is projected from the front: every point with `y < 0.2` in head space gets a
  front-projection UV. (read in code: `body.py:251-298`)
- The rig bakes the face into the miner's shared 2048 atlas (Cycles EMIT). In Unity the whole miner is one material,
  `Miner <Name>.mat` on `WGPainted`. (read in code: `rigging.py`, `Editor/MinerSetup.cs:772-799`)
- **Import:** `CompressedHQ`, aniso 4, mip bias -0.6 so the thin strokes hold at a distance. (read in code:
  `MinerSetup.cs:777-783`) The inventory's guess of BC1 is therefore wrong for the atlases. `CompressedHQ` is usually
  BC7 on desktop (hypothesis; check in the editor).
- **The paint filter skips the face.** Drawn beings write -1 into the normals' alpha. The Kuwahara pass then applies
  only 15 percent of its paint to them, so the marks hold. (read in code: `WGLookPost.shader`, the "drawn" step;
  `MinerSetup.cs:796`)
- **Light on the face:** `WG_Direct` uses the mesh normal with a smoothstep edge, softness 0.38 on the miner material,
  broken by brush noise. (read in code: `WGCommon.hlsl`, `WGPainted.shader`)
- **One material for all.** In the game, skin, hair and cloth share softness 0.38, gloss 0.08 and translucency 0.12.
  The separate `Skin_*.mat` (softness 0.45, translucency 0.25) and `Hair.mat` (gloss 0.25) belong to the portrait
  models, not to the rigged miner. (read in code: `Materials/Miners/*.mat`, `MinerSetup.cs`)
- The worktree's `faces.py` differs from the main project's only in line endings. `hair.py`, `body.py` and
  `workers.py` are the same. (measured: `diff --strip-trailing-cr`)

### 2.2 The face numbers today (read in code: `faces.py`, `workers.py`; positions in head radii)

| | Small (`curious`) | Long (`sleepy`) | Round (`laughing`) |
|---|---|---|---|
| Head radius, shape | 0.13 m, (1.04, 1, 1) | 0.112 m, (0.94, 1, 1.14) | 0.128 m, (1.08, 1, 0.98) |
| Eye centre height | -0.10 (below the head's centre) | +0.02 | -0.02 |
| Eye width | about 0.31 | about 0.10 | crescent, about 0.22 |
| Gap between the eyes | about 0.43 (about 1.4 eye widths: wide-set) | about 0.50 (5 eye widths: very wide for the eye size) | about 0.50 |
| Iris | dark, tall, set to the outer side (a glance) | small dark dot under a heavy lid | closed |
| Catch light | one white dot | none | none |
| Brow | thin arc | long, sloping down | short arcs |
| Nose (form) | one ball, 0.085 | three balls, a long spike | two balls, a bulb |
| Mouth | small parted ellipse | crooked smile line | open wide smile |
| Blush | three small strokes on a soft wash | faint | big rosy washes |

### 2.3 What each face does well (seen in a picture: `Docs/Research/2026-11-14_TheModelsRedrawn/pictures/*_rest_close_head.png`, `Docs/Images/Miners/Miners_Portraits.jpg`)

- **Small:** big, bright, wide-set eyes with a catch light and a lash flick. A curious glance to the side. Blush
  strokes. It is the closest to reference B. Luis's favourite by rank. (record: `TheMiners.md`)
- **Long:** a kind, sleepy, slightly melancholy face. A crooked smile. The long nose is charming from the front. It
  is pure reference E.
- **Round:** the closed-eye laugh, the bulb nose and the rosy cheeks. Warm and lively. Pure reference E.
- **All three:** few marks, strong ideas, wide-set eyes. Every face is different. They hold up at dusk in the
  portraits.

### 2.4 What could be prettier (seen in a picture unless marked)

**Carried by the surface, not by the drawing:**
1. **Blotchy light on cheeks and jaw.** The metaball skin has bumps where the cheek, jaw and chin balls meet. The light
   follows each bump. Small shows a lump at the jaw in three-quarter view. Long's face has patchy bands in the day
   portrait. Reference B has two or three clean light shapes. (seen in a picture; cause read in code: `body.head`
   items, mesh normals in `WG_Direct`)
2. **Noses read as balls stuck on.** Small's nose is one sphere with a sharp crease where it meets the face. Round's
   bulb is right for E, but its join is sharp too. Long's spike is sharp in profile. (record: research L6, "a question,
   not a fault") In B the nose is nearly only a shadow and a lit tip.
3. **Soft face texture.** The face island in the atlas carries about 52 percent of the face texture's resolution:
   Small's eyes are 303 px apart in `Face_Small.png` and about 159 px apart in the atlas. So the face is
   effectively a ~530 px painting. (measured, rough, from eye positions in the code and the picture) At a full-screen
   portrait the strokes soften. At a cinematic close-up they blur.
4. **A stray eye fragment.** Part of an eye is painted on a second UV island in Small's atlas. Either a seam crosses
   the eye, or the front projection paints the eye onto a second surface behind the face (under the fringe or the
   ear's hollow). (seen in a picture of the atlas; the cause is a hypothesis) A seam through an eye can show as a line.
5. **Skin is one flat tone with blush on it.** No warmer middle band, no cooler jaw, no warmth at the ears. In the day
   light Small's face washes pale. (record: research S11)
6. **One material for everything.** Skin cannot get its own soft edge, its own translucency (the warm glow at the edge
   of the shadow) or its own rim. Hair cannot get its own sheen. (read in code)

**The drawing itself (ideas only; the drawings are loved):**
7. **Small's mouth** is a tiny filled ellipse. In the game portraits it reads as an "o", surprised or whistling,
   more than curious. (seen in a picture) A slightly wider, softer mouth with a lip tint would read as curious and
   pretty, as in B. (hypothesis)
8. **Long's eyes** are very small for the gap between them. The face can read blank at conversation distance. A
   slightly larger lid mass with a lash line, still sleepy, would give him more warmth. (hypothesis)
9. **No lid mass on Small.** Her upper lid is a thin line. B's charm is a thick dark upper lid that carries the
   lashes. (seen in a picture; hypothesis that it would read prettier)
10. **Cheeks are lifted only by paint.** Round's laugh would lift the cheeks into "apples" in a real face. Today the
    form does not lift. (hypothesis)

**Hair (section 6 has more):**
11. **Facets.** The locks are 8-sided tubes, then decimated with everything else into the 16,000-triangle budget. Flat
    facets show in every close render. Small's fringe is a saw edge of even triangles. Long's locks look like dark
    crystal shards. Round's curls look like brown buns. (seen in a picture; read in code: `hair.lock_mesh`,
    `rigging.LOD_BUDGETS`)
12. **No flyaways, no inner strand lines, no sheen.** Both references have them. (seen in a picture)

**Heads:**
13. **Necks:** Round's head sits almost on her shoulders. Small's collar swallows her neck. (record: research 2.8) Not
    redone here.
14. **Ears** are not seen in the close renders. They are hidden under hair or too small. B shows an ear, and it
    helps the head read as a head. (seen in a picture; hypothesis that a visible ear helps)

### 2.5 At game distance (seen in a picture: `Docs/Images/Miners/Rigged_Together_Strategy.jpg`)

From the Strategy camera's height a miner is about 30 px tall and a head about 10 px. No face mark can be seen. What
reads is: the hair's dark shape, a dot of skin, the head's size against the body, and the coat colour. So:
- **At the Strategy camera, head size and hair shape are the face.**
- At conversation distance (the portraits), eyes and mouth read. The blotchy light and the soft texture show there.
- In close and cinematic shots, everything shows: facets, seams, texture softness.
Luis counts every view, the cinematic ones too. (record: memory, November 13)

---

## 3. Head-to-body ratio: appeal and age

### 3.1 Today (rough)

| | Height | Heads tall, with hair (from the silhouettes) | Heads tall, skull only (from the preset numbers) |
|---|---|---|---|
| Small | 1.42 m | about 4.5 | about 4.9 |
| Long | 1.86 m | about 5.8 | about 6.5 |
| Round | 1.56 m | about 4.1 (the curl buns add width and height) | about 5.5 |

Method: the first column is read from `pictures/sheets/all_rest_silhouettes.png` by eye, so it is rough (seen in a
picture). The second is the head's vertical extent from `body.head`, taken as about 2.25 head radii times the
vertical head shape (hypothesis-grade arithmetic from the code, not a measurement of the mesh).

For scale: an average adult is about 7.5 heads tall (record: models research, source S1). Reference E's figures are
about 4.3 and 5.3 (rough, seen in a picture). The miners already sit in reference E's range.

### 3.2 What changing it does (from a source, and hypothesis)

- **Bigger heads** read younger, cuter and more approachable, and they read better at small sizes and from above,
  where the face cannot be seen. (from a source: weak game-art pages and a game-news piece; the principle is common
  practice)
- **They also shift the reading of strength and age.** Bigger heads on short bodies read as children. The prototype's
  purpose is "real weight and real strength at work". (record: memory, prototype purpose first) Small is already the
  closest to a child's proportions.
- **Each miner's one idea decides the direction.** A bigger head helps "small and quick" (Small). It fights "long and
  thin" (Long), whose idea lives in height and a small head. Round's "round and heavy" may gain from a rounder,
  slightly bigger head over a barrel body, as in reference E. (hypothesis)
- **The hair counts.** Round's curls already make her head read big. A head change can be done in the skull, in the
  hair volume, or both. Hair volume changes nothing in the physics. (hypothesis)

### 3.3 The physics side (record)

The head's size reaches `headHalf`, the head sphere, the head's mass and the centre of mass. A bigger head raises the
centre of mass, which makes standing and getting up harder. (record: models research 5.3; `modelling.md` 3.4) The
getting-up must still work for any body, by the body's own movement. So:
- **In stills, a head change is free.**
- **In the game, it goes with the physics work**, as Luis already said. A change carried only by hair volume avoids
  that, if Luis prefers it. (hypothesis)

### 3.4 The proposal (for stills only; hypothesis)

- For each miner: skull scale at minus 8, 0 (today), plus 8 and plus 15 percent, with the face marks scaled along.
- Plus one "hair volume only" row at plus 8 percent apparent size.
- Each one shown as: a front and three-quarter full figure, the portrait, and **the Strategy camera crop at its true
  pixel size**. The far crop is where a head change pays or does not.
- Today's head is always column A. Luis picks per miner, or keeps today's.

---

## 4. How the best stylised films and games build faces

### 4.1 Two families

| | Drawn face (texture) | Built face (geometry) |
|---|---|---|
| Where | Wind Waker, many anime-style games, our miners | Pixar and Disney films, most realistic and many stylised 3D games |
| Eyes | Painted, or swapped textures, or decals on the face | Separate eyeballs: sclera, iris, cornea lens, catch light |
| Expression | Swap textures per part (eyes, brows, mouth) | Bones and blend shapes on a face with loops around eyes and mouth |
| Strength | Graphic, few marks, reads small, cheap | Depth, parallax, real light in the eye |
| Risk | Swims or stretches on a turning head if the projection is wrong | Uncanny or "plastic" if pushed realistic; much work |

- **Wind Waker's Link:** the face is split into texture layers. Eyes, brows and mouth are separate parts, swapped and
  combined to make many expressions. One count: 7 eyes, 6 brows, 9 mouths, 1 pupil. (from a source: articles, search
  summary only; the count is unattributed)
- **Separate eye and brow pieces raised slightly above the face** are a common game practice for drawn faces. (from a
  source: search summary only, weak)
- **Pixar, Monsters University:** eyes with a corneal bulge and a refraction shader to the iris; cornea and iris
  normals turned towards the camera so the highlights stay put while the eye moves. (from a source: SIGGRAPH 2013
  talk, search summary only; the PDF could not be fetched)
- **Stylised eye kits:** three meshes (cornea, sclera with iris, highlight), with iris colour and pupil size in the
  shader. (from a source: search summary only)
- **Guilty Gear Xrd:** a 2D look from 3D. Hand-edited vertex normals so the face lights in clean zones; a per-vertex
  threshold (in vertex colour) that biases areas to shadow; inner lines from sharp, axis-aligned texture strips; an
  outline from a duplicate mesh with smoothed normals, its width set by vertex colour. (from a source: GDC 2015 talk
  summary and a community article, read in part)
- **Face shadow maps (SDF):** anime-style games draw the face's light and shadow at several light angles, turn each
  into a distance field, merge them into one threshold map, and compare it with the light's angle around the head.
  This gives clean shadow shapes that normals alone cannot. One tool's guide uses nine angles across 0 to 180
  degrees. (from a source: a tool's documentation, read in part; a tech-art lecture, search summary only)
- **Arcane (Fortiche):** much of the light and shadow painted into the textures by hand; some features (a scar) exist
  only in paint. (from a source: articles and forum claims, search summary only; weak)

### 4.2 What fits this game (hypothesis)

- **Keep the drawn face.** It is the aesthetic ("drawn, not only modelled"; "few marks"), it reads small, and Luis
  loves it. A built face with eyeballs is a different art direction.
- **Borrow the built face's tricks where they do not break the drawing:**
  - **Clean light zones on the face** (Guilty Gear): normals transferred from a smooth proxy. This is the largest and
    cheapest gain. See 4.3.
  - **Parts as layers** (Wind Waker): eyes, brows, mouth, blush, soot as separate layers in the face painter. This
    makes blinks, a few expressions and the creator's face options possible.
  - **A catch light that holds** (Pixar): the painted catch light is fixed in the texture, which is right for a
    front-facing face. A view-stable catch light in the shader is an option for cinematic close-ups only.
- **Geometric eyes** (an eye socket and a lens cap over the painted eye) are listed as a far option for the cinematic
  view, not recommended now. They would add depth up close and nothing at game distance, and they move away from
  "drawn".

### 4.3 Face normals: the key method

- **Today:** normals are the metaball isosurface's own, after decimation. (read in code)
- **The method:** in Blender, a smooth proxy (an ellipsoid fitted to the face, plus the nose's own smooth form) gives
  its normals to the face skin through a Data Transfer modifier with a falloff, so the nose still shades as a nose.
  The FBX is exported with custom normals. Unity already imports normals. (read in code: `MinerSetup.cs`,
  `importNormals = Import`, per `modelling.md`; the Blender steps are a hypothesis, not tried)
- **Also:** a vertex-colour threshold bias, as Guilty Gear does, could keep the eye sockets and under the nose a
  touch darker. That needs a small `WGPainted` change behind a keyword. (hypothesis)
- **The outline:** `WGOutline` pushes the mesh along normals. Custom normals on the face would change the outline
  there. The outline should read the smooth normals from a spare channel. (hypothesis; `modelling.md` 5.6 notes the
  same)
- **A switch:** export both normal sets, or keep the old FBX beside the new one, so Luis can flip between them.

---

## 5. Face texture painting

### 5.1 What the painter does well (read in code)

- Tapered strokes with a hand wobble, along Catmull-Rom curves: a drawn line, not a vector line.
- Soft washes with a broken edge for blush and soot.
- One function per face style, parameters in the preset. The creator can grow from it.

### 5.2 What would raise it (hypothesis unless marked)

1. **A dedicated face map.** Give the head skin its own UV island at a set texel density, or its own 1024 or 2048
   texture and material slot. Today the face gets about 530 px of effective resolution (measured, rough). A separate
   material also lets skin have its own values (softness, translucency, a warmer rim) and its own blink layer. Cost:
   one more draw per miner, small against the measured headroom. (record: inventory 1.0 on headroom)
2. **Mend the stray eye fragment.** Restrict the front projection to the outermost face surface (cast a ray from the
   front and paint only the first hit), and keep UV seams out of the eye and mouth areas.
3. **Strokes at a finer resolution, then down-sampled.** Paint at 2048 or 4096 and filter down, so the edges are
   clean but not hard. Check that mip bias -0.6 does not shimmer at a distance once the face is sharper.
4. **Colour zones in the skin wash.** A warmer middle band (cheeks, nose, ears), a slightly golden forehead and a
   slightly cooler jaw, each very gentle, plus warmth at the nose tip and ears. (from a source: Gurney, as a
   starting observation) For Round and Long the reference E ruddiness is right. For Small, less.
5. **Painted form shadow, very light.** A soft cool tint under the brow ridge, under the nose and under the lower lip,
   as reference B and Arcane do. It must stay faint, because the game's light also shades the face. Two shadows on
   top of each other look dirty. (from a source: Arcane, weak; hypothesis for us)
6. **Layers, not one image.** Paint skin, blush, soot, eyes, brows, mouth and lids as separate layers, saved
   together and also apart. This supports blinks, the creator, and side-by-side variants.
7. **A face that turns well.** Front projection stretches the marks on the sides of the face. Test the three-quarter
   and profile views: if the outer eye corners stretch, paint them in the head's own UV space instead (a
   cylindrical or a proper unwrap), or move the eyes a little inward. (hypothesis; the three-quarter picture of Small
   looks fine)
8. **Texture quality checks.** Confirm the platform format in the editor (BC7 or not). Look for banding in the blush
   gradients. (hypothesis)

### 5.3 Prettier drawings to explore (stills only, beside today's)

All are hypotheses for the judges and Luis. Each keeps the miner's own idea.

| | Keep | Explore |
|---|---|---|
| **Small** | Big wide-set eyes, the glance, the lash flick, blush strokes, the soot | A thick dark upper-lid mass with the lash flick growing out of it (B); a larger dark iris share; a second, tiny catch light; a slightly lower eye line; a wider, softer mouth with a lip tint; the nose smaller and blended into the face, with a lit tip and a shadow |
| **Long** | The sleepy lids, the crooked smile, the long nose from the front, the melancholy | Eyes a little larger with a lash line on the lid; a soft catch light; brows a touch fuller; a warmer, ruddier middle band (E); a softer nose tip in profile (only if Luis wants it; research L6) |
| **Round** | The closed-eye laugh, the bulb nose, the rosy cheeks, the open smile | Crescents with a small lash flick at the outer end; cheeks lifted into "apples" by the form; laugh lines at the eye corners; a little shine on the nose tip (E) |

---

## 6. Hair: methods and stylised shading

### 6.1 Today (read in code: `Art/Blender/Worker/hair.py`)

- Hair is **locks**: paths combed over a skull sphere with gravity, bend, twist and a lift, each made into a flat,
  tapered tube with 8 sides that ends in a point. A cap closes the gaps. Curls are metaball clumps joined into one
  cloud. The styles are a bob, a swept cut and curls.
- The inventory calls hair a voxel-remeshed shell. The code shows tube locks and a metaball cloud. The inventory is
  corrected here. (read in code)
- The locks then go through the whole miner's decimation to 16,000 triangles. (read in code: `rigging.py`)
- They are skinned to the head and do not move on their own. (read in code: `rigging.py` line 181, hair follows the
  head)
- In the game they use the shared atlas material: gloss 0.08. (read in code)

### 6.2 The methods (from a source unless marked)

| Method | What it is | For our look |
|---|---|---|
| **Mesh locks** (sculpted or built clumps) | Hair as solid shapes | Fits "confident dark shapes". Most stylised games with a drawn look use it. Needs painted or shader-made highlights. (from a source: forum practice, search summary only) |
| **Cards** | Flat or curved strips with a strand texture and alpha | Good for wisps and flyaways. Alpha cards fight the outline pass, sorting and the paint filter. Flat cards look poor unless curved around the head. (from a source: search summary only; our pass issues are a hypothesis) |
| **Curves / strands** | Real strands | Out of this pipeline in real time, and not our look. (record: inventory 3.1) |
| **Curves as the authoring tool** | Guide curves turned into ribbons or locks by geometry nodes | Our `comb` already does this in Python. Geometry nodes would add nothing essential. (from a source; hypothesis) |

**Recommendation (hypothesis):** keep mesh locks as the body of the hair. Add a few thin, solid tapered ribbons as
flyaways and stray strands. No alpha.

### 6.3 What would make the hair prettier (hypothesis)

1. **No facets.** Exempt the hair from the heavy decimation, or decimate it on its own with smooth normals. Raise
   the lock tube's sides or subdivide before export. The facets are the strongest 3D giveaway on the miners up close.
2. **Light on the hair as one mass.** Transfer normals from a smoothed hair shell (the hair's outer hull, blurred) so
   the hair lights as one mass with a few lock edges, as painted hair does. This is the same trick as the face.
3. **Lock shapes with variety:** wider and narrower locks, some ends split in two, a few overlapping, the fringe
   uneven (reference B's fringe is blunt but broken). Small's saw edge becomes a soft, broken edge.
4. **Flyaways:** two to five very thin ribbons per head that leave the silhouette. They break the helmet outline,
   which both references show.
5. **Painted inner strands and a sheen:** UVs along each lock, so a stroke texture runs with the flow. A soft lighter
   band on the crown, painted or from a toon "anisotropic" highlight that uses the lock's direction (a tangent stored
   in a spare channel). Stylised hair highlights are often painted bands or zigzags, not a physical highlight. (from a
   source: search summary only) Value design stays: the hair stays a confident dark shape.
6. **Its own material values:** a little more gloss than the cloth, a cooler rim, so the hair separates from the
   coat at dusk. Needs a hair material slot or a mask in the atlas. (read in code: one material today)
7. **Round's curls:** clumps with spiral forms and lighter tops, not round buns. (seen in a picture)

### 6.4 Hair that moves, physically

- Only where it can swing: Small's bob ends, Long's front locks, a few of Round's springing curls.
- A short chain of bones per lock group, moved by springs driven by the head's real motion and gravity, stopped by a
  sphere for the skull and capsules for the neck and shoulders. This is the "physical objects" rule: the hair hangs,
  swings and is stopped by the body. (record: the rule; the method is the spring-bone practice in the models research,
  source S24 there)
- Strength: small. Hair is light; it should lag and settle, not wave. Measured on all three miners, at rest, walking,
  at work and falling. (record: memory, measure all three miners)
- It must never be a looping clip.

---

## 7. Small living face movement, done by the body

### 7.1 What exists (record)

- Breath is shown on the chest and shoulders, and in the dusk air, gently and scaled to the situation. Luis decided
  the look on October 10. (record: `GAME_VISION.md` lines 520-533)
- There is no blink and no expression change. Faces are fixed. (record: `TheMiners.md`, "Fixed expressions"; "Faces in
  motion" is under Next)

### 7.2 What real blinks do (from a source)

- Adults blink about 10 to 20 times a minute at rest. (from a source: a children's blink study, search summary only)
- **Blinks go with gaze shifts and head turns.** In one study, about 87 percent of large eye movements came with a
  blink. In another, the lid muscle fired with every head turn larger than about 17 degrees. (from a source: two
  studies, search summary only)
- Blink rate shifts with attention and tiredness. Concentration lowers it. Double blinks happen now and then. (from a
  source: a game-dev guide, weak)
- Lids and blinks are among the strongest cues that an eye is alive. (from a source: a small student study, weak)

### 7.3 A blink that is the body's own (hypothesis; for Luis to rule on)

The rule is "every movement is the weighted body acting on the world". A blink is a muscle of the body, not a push on
the world. Read strictly, it is outside the rule. Read by its spirit ("nothing pre-made, nothing on a timer, the
body's own"), a blink can be made the body's own:
- **Triggered by what the body does:** the head turns by more than about 15 to 20 degrees, the gaze target jumps, the
  pick strikes (a squeeze at the effort), the body lands from a fall.
- **Scaled by the body's state:** more and slower blinks when tired; fewer while concentrating at the rock; the breath
  clock may set the rest rate.
- **Lids as a drawn layer:** the face painter makes two or three lid states (open, half, closed), and the shader blends
  them by a per-miner value. No blend shapes are needed. Round's laugh is already closed, so her blink is a squeeze.
- **No fixed timer.** Any randomness is in the body's state, not a loop.

**The question for Luis:** does a blink, driven this way, fit "no fake animations"? Until he answers, it stays an
option.

### 7.4 Other small movement (hypothesis)

- **Eye glances** (a painted iris that moves) would need an iris layer that slides inside the eye shape. It would
  follow the head's look target. Small's glance is part of her face; moving it changes the loved face. So it is a
  later option, only with Luis's word.
- **Mouth with heavy breath:** after hard work the mouth could part a little with the breath clock's size. A drawn
  layer, scaled to the situation. (record: breath is gentle and scaled)
- **Nothing on the face at rest except breath and the rare blink.** Luis loves the idle; keep it. (record: memory)

---

## 8. Exploring prettier faces and a head-size change, additively

### 8.1 The rules that apply (record)

- Explorations are additive: a switchable option beside the loved one, never an overwrite.
- Change only what is asked. The faces are liked; the ask is "ways to make them even prettier".
- Any fix to the drawn body goes to aesthetic judges, then Luis.
- Head proportions stay Open until Luis picks.

### 8.2 The order (hypothesis)

1. **Stage F0, the carrier, in stills.** Today's exact marks with: smooth face normals; a dedicated face map; the stray
   eye fixed; skin with its own values; hair without facets, lit as a mass. Shown as A (today) and B (carried better).
   The drawing does not change, so it is "change only what is asked" at its most careful.
2. **Stage F1, prettier drawings, in stills.** Per miner, two or three variants from table 5.3, each on top of F0,
   with today's drawing on F0 as the control. One change per variant where possible, so Luis can say which change he
   likes. (record: memory, sweep the variable Luis names)
3. **Stage H, head size, in stills.** Section 3.4. It can run beside F1.
4. **Judges, then Luis.** Section 9.
5. **Only then the game.** The chosen face goes in as a version switch (a material and texture swap). A head change
   waits for the physics work.

### 8.3 How a variant is kept switchable (hypothesis)

- In Blender: `faces.STYLES` grows new style functions beside the old ones, for example `curious_lidded`, never edits of
  `curious`. Presets in `workers.py` pick the style. The same goes for a new head option (`head_scale` in the preset).
- In Unity: each variant is its own face texture (and FBX if the normals or head change) and its own material. The
  switch is on the miner or on the look panel. Today's assets keep their GUIDs. (record: the project's GUID rule)
- For the creator later: the face painter's layers become the creator's few options: two or three eye sets, brows,
  mouths, noses and skin tones, each made and judged by hand, not sliders. (record: models research, source S33,
  Baldur's Gate 3 uses presets for faces)

---

## 9. How faces are judged

### 9.1 The lenses (hypothesis; built on `ModelQualityMethod.md`)

| Lens | Why |
|---|---|
| **Portrait**, three-quarter, eye level, at day, dusk (lamp light) and night | Where Luis looks at faces |
| **Conversation distance** (the explore view at a few metres) | Where eyes and mouth must still read |
| **Strategy camera crop at its true pixel size**, plus 2x and 4x zooms of the same pixels | Where only head size, hair and skin read |
| **Profile and three-quarter back** | Noses, necks, hair from behind |
| **A head-turn strip** (8 directions) | Projection stretching, seams, the face swimming |
| **Grey values, mirrored copy, a blurred "squint" copy** | Painters' fresh-eye checks (record: the method) |
| **Today's face beside each variant, same light, same frame** | The loved face is the control |
| **Reference B and E crops at the same scale** | The aesthetic's own pictures |

Blender stills are fine for drawings and form. Only Unity captures decide the final look: the light, the paint filter
and the outline differ from EEVEE. (record: inventory 3.2) Unity captures wait until the test suite is finished.

### 9.2 The judges (record and hypothesis)

- **Aesthetic judge agents** read the sheets against the guides (VisualSoul, TheEssence, TheMiners, the references).
  They compare pairs ("which is prettier, by the aesthetic, and why"), rather than score alone. Pairwise judgement is
  steadier than scores. (hypothesis)
- **Pitfalls:** judges and Claude spot faults better than beauty (record: inventory 3.0). A judge may prefer the
  generic, "prettier" face that loses the miner's idea. So every judge is told each miner's idea and the "keep" list.
- **Luis decides.** Passing judges is not acceptance.

---

## 10. What it needs from Claude's tools, and how hard

| Work | Tool | What it needs | How hard to reach top quality |
|---|---|---|---|
| Smooth face and hair normals | Blender by Python, command line | A proxy mesh per head, a Data Transfer modifier, FBX export with custom normals; then a Unity import and capture | **Moderate.** Standard, scriptable. The risk is the outline and the nose; needs pictures |
| Dedicated face map, skin and hair material slots | Blender (UVs, bake), Unity editor script (`MinerSetup`) | A face UV island or a second texture; a ray-cast first-hit projection; material values per slot | **Easy to moderate** |
| Prettier drawings | The NumPy face painter in Blender | New style functions; many variants; sheets | **Hard.** No image generator and no hand painting. Beauty comes from many judged loops of code-drawn strokes. Luis's taste is the gate |
| Colour zones, faint form shadow | NumPy painter | Masks from the head's own geometry (for example its curvature or ambient occlusion baked once) | **Moderate.** Easy to overdo; judge at three hours of light |
| Hair without facets, flyaways, strand UVs | Blender (`hair.py`, `rigging.py`) | Exempt hair from the global decimation, raise sides, add ribbons, UVs along locks | **Moderate** |
| Hair sheen or toon highlight | Unity shader text (`WGPainted` keyword) | A tangent per vertex in a spare channel; a band function; a capture to judge | **Moderate.** Only seen in Unity |
| Hair springs | Unity C# (runtime), rig bones in Blender | Bones per lock group; springs driven by the head; colliders | **Moderate to hard.** Must be physical, stable and gentle on all three miners |
| Blink from the body | Painter (lid layers), shader blend, C# trigger from head turns and effort | Two or three lid states per face; a per-miner value | **Moderate**, once Luis rules on it |
| Head size stills | Blender command line | A preset scale; renders including a Strategy-camera match | **Easy** in stills. In the game it is the physics work: **hard** |
| SDF face shadow map | Blender renders of a face proxy at 9 light angles, Python to make distance fields and merge, a shader branch | Only if smooth normals are not enough | **Moderate to hard** |
| Geometric eyes | Blender modelling, shader | Sockets, lens caps, catch light | **Hard**, and a different art direction |

The Blender MCP connection helps for quick looks at one head. The command line is better for batches of sheets.
Python image tools (Pillow, NumPy, OpenCV) make the sheets, the grey and mirrored copies and the pixel-true crops.
(record: inventory 3.0) Nothing here needs a download.

---

## 11. Ranked quality gains

Ranked by quality raised per effort. Sizes are rough: S under a day of agent work, M a few days, L a week or more.
(hypothesis for all sizes) Every gain is an option beside today's look.

| Rank | Gain | Size | Why here |
|---|---|---|---|
| 1 | **Smooth face normals** from a proxy (the nose kept), with the outline reading smooth normals | S | Ends the blotchy light on every face without changing a mark. The light on the cheek is in the guides |
| 2 | **Dedicated face map plus skin and hair material slots**; mend the stray eye fragment | S to M | About double the face's sharpness; skin gets its own warmth; hair its own sheen |
| 3 | **Hair without facets, lit as one mass**, with an uneven fringe and a few flyaways | M | Removes the strongest 3D giveaway up close; both references show it |
| 4 | **Face sheets, stage F0** (today's marks, better carried) for judges and Luis | S | Cheap proof of gains 1 to 3 before anything is changed in the game |
| 5 | **Prettier drawings, stage F1**: lid mass, iris share, lip tint, softer nose join, colour zones, per table 5.3 | M | The direct answer to Luis's ask; the hardest to get right |
| 6 | **Head-size stills** at -8, 0, +8, +15 percent, plus hair-volume only, with Strategy-camera crops | S | Cheap in stills; answers an Open question with pictures |
| 7 | **Hair sheen and inner strands** (strand UVs, a painted or toon band) | M | Separates hair from coat at dusk; value design kept |
| 8 | **A blink from the body** (head turns, effort, tiredness), as lid layers | M | Strong cue of life; needs Luis's ruling on the rule |
| 9 | **Hair springs** on a few lock groups, physical and gentle | M | Secondary motion that obeys the physical-objects rule |
| 10 | **Layered face parts for the creator** (a few eye, brow, mouth, nose and skin options) | M | Needed for the final prototype's creator; after Luis picks the faces |
| 11 | **SDF face shadow map** | M to L | Only if gain 1 is not clean enough |
| 12 | **A head-size change in the game** | L | Rides on the physics work, by Luis's word |
| 13 | **Geometric eyes** for the cinematic view | L | Depth up close only; leaves the drawn look |

---

## 12. Sources

Tags: read in full, read in part, search summary only, weak. Only short phrases quoted; the rest is my summary.

- **S1.** Arc System Works, Junya Motomura, "GuiltyGearXrd's Art Style: The X Factor Between 2D and 3D", GDC 2015.
  https://www.ggxrd.com/Motomura_Junya_GuiltyGearXrd.pdf and https://gdcvault.com/play/1022031/GuiltyGearXrd-s-Art-Style-The
  — search summary only (the PDF fetch failed).
- **S2.** Sketchfab blog, "Art Spotlight: Here, at Last" (a Guilty Gear style model breakdown).
  https://sketchfab.com/blogs/community/art-spotlight-here-at-last — read in part.
- **S3.** Polycount threads on the Guilty Gear threshold in vertex colour, e.g.
  https://polycount.com/discussion/comment/2187341 — search summary only, weak.
- **S4.** Anime Shading Plus documentation, "Face shadow map creation and baking workflow".
  https://erichu33.github.io/ASPDocs/en/articles/face-shadow-map-creation-and-baking-workflow.html — read in part.
- **S5.** A tech-art blog on face SDF shadows,
  https://xiaoxiaodengfiea.blogspot.com/2026/06/tech-art-summer-initial-overview-of-vfx.html — search summary only.
- **S6.** SIGGRAPH 2013 talk on Monsters University eyes (Pixar), via a disc mirror (404 on fetch). — search summary
  only.
- **S7.** Stylised eye kits and forum threads (three-mesh eyes; Blender Artists on cartoon eye rigs),
  https://blenderartists.org/t/how-would-you-make-rig-this-characters-cartoony-eyes/1501464 — search summary only, weak.
- **S8.** Wind Waker / Link face layers: https://medium.com/@gordonnl/links-expressions-eb7beae2c62c and
  https://ponderworthy.com/the-wind-waker-link-faces-why-this-20-year-old-expression-engine-is-still-unbeatable-sv5
  — search summary only, weak (the asset counts are unattributed).
- **S9.** Glocker et al. 2009, "Baby Schema in Infant Faces Induces Cuteness Perception and Motivation for Caretaking
  in Adults", Ethology. https://pmc.ncbi.nlm.nih.gov/articles/PMC3105163 — search summary only.
- **S10.** Disney Research, "Designing Animated Characters for Children of Different Ages" (2016).
  https://studios.disneyresearch.com/2016/06/21/designing-animated-characters-for-children-of-different-ages/ —
  search summary only (the PDF did not parse).
- **S11.** Tutorials on eye line and age, e.g. https://design.tutsplus.com/articles/quick-tip-brainstorming-head-design-a-focus-on-the-eye-line--vector-5436
  — search summary only, weak.
- **S12.** James Gurney, "Color zones of the face" (2008) and "Relative color temperature on skin tones" (2017),
  https://gurneyjourney.blogspot.com/2017/07/relative-color-temperature-on-skin-tones.html — search summary only.
- **S13.** Blink and gaze studies: driving study on blinks with large saccades (https://core.ac.uk/download/41774567.pdf);
  blinks with head turns (https://sites.pitt.edu/~neg8/rajimages/pdf/EBR-11-0590R1.pdf); EOG study
  (https://www.uni-muenster.de/imperia/md/content/psyifp/ae_lappe/freie_dokumente/kirchner_jnp2022.pdf); children's
  blink rates (Canadian Journal of Neurological Sciences) — all search summary only.
- **S14.** Game-dev guides on gaze and blinking: https://www.abratabia.com/ai-talking-characters/facial-animation.php,
  https://docs.convai.com/api-docs/plugins-and-integrations/convai-unity-sdk/utilities/gaze-and-attention/profiles-and-tuning,
  a student report https://dreuarchive.cra.org/2014/Moore/uploads/3/0/4/2/30422230/final_report.pdf — search
  summary only, weak.
- **S15.** Hair methods: https://sidney-eliot.github.io/3d-artists-handbook/mesh/hair, Polycount and Blender Artists
  threads on stylised hair shaders, a stylised cartoon hair renderer paper (typeset.io) — search summary only, weak.
- **S16.** Blender geometry-nodes hair: https://studio.blender.org/blog/procedural-hair-nodes,
  https://80.lv/articles/free-hair-cards-from-curves-setup-for-blender — search summary only.
- **S17.** Arcane texturing: https://80.lv/articles/a-closer-look-at-texturing-in-arcane (read in full; it is a picture
  showcase with almost no text), https://redsharknews.com/why-netflixs-arcane-looks-so-good-how-fortiche-ramped-up-the-animation-pipeline
  — search summary only, weak.
- **S18.** Game-art pages on why game characters have bigger heads (several weak content-farm pages and a game-news
  piece) — weak.

Project sources read: `inventory.md`, `modelling.md` (parts), the models research `research.md` (parts), the
correspondence of November 14, `VisualSoul.md`, `TheMiners.md` (parts), `CharacterPractices.md` (parts),
`GAME_VISION.md` (breath lines), the references B and E, the close-head and silhouette pictures, the portraits and
the Strategy picture, the Small atlas.

---

## 13. What was not done

- **No Unity or Blender run.** No new still was rendered. Every gain is a hypothesis until it is rendered and judged.
- **Numbers are rough.** The heads-tall values come from a silhouette sheet by eye and from preset arithmetic. The
  face's share of the atlas comes from one miner (Small) and from eye positions. Long's and Round's atlases were not
  measured.
- **The stray eye fragment's cause** was not found. Only Small's atlas was looked at.
- **The texture format** (BC7 or not for `CompressedHQ`) was not checked in the editor.
- **LOD1 and LOD2 heads and hair** were not looked at.
- **The face in motion** (a head turn in the game, the projection's stretching) was not watched.
- **Key sources could not be read in full:** the Guilty Gear PDF, the Pixar eye talk and the Disney Research paper
  failed to fetch or parse. Their claims are from search summaries.
- **No study of specific stylised games' faces** beyond the sources above (for example no developer talk on a recent
  drawn-face 3D game was found).
- **No ruling on blinks.** Whether a body-driven blink fits "no fake animations" is Luis's call.
- **No download** of any tool, model, texture or add-on. None is needed for anything here.
- Nothing was committed. Only this file was written.
