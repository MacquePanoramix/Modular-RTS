# The models redrawn: what the project itself says (internal research)

November 14, 2026. Part of the deep research Luis asked for before any proposal
(Docs/Correspondence/2026-11-14_THE_FLAPS_ON_AND_THE_MODELS_REDRAWN.md). Read-only research:
nothing in Assets/, Art/ or ProjectSettings was changed; nothing was built, run or committed.

Status: complete for the project's own sources (November 14, 2026). Measurements on the meshes and pictures are still to do.

## 1. The aesthetic guides: what they ask of a character

Files read: Docs/ArtDirection/VisualSoul.md, TheEssence.md, CharacterPractices.md, TheMiners.md, WorkerConcepts.md,
StyleStudies.md, ModelQualityMethod.md, MotionJudging.md. Docs/ArtDirection/References holds six images (not opened
here): A_TheLitHouse.jpg, B_AFaceInWindowLight.jpg, C_TheBreathingGrove.jpg, D_TactileDarkness.jpg,
E_ExpressiveDrawnLife.jpg, F_CosmicWonder.jpg. B and E are the two the guides cite for people; Luis's own reference
(a bust of a girl in a pink scarf) is described but not committed (VisualSoul.md:162-166).

### 1.1 Status of the guides

- The Visual Soul is the **Direction**; "Final proportions, architecture, world setting and rendering technique stay
  **Open**" (VisualSoul.md:9-10, restated :64-65). Proportions have never been decided.
- TheEssence.md: "Nothing here is Locked" (:5); its rule that matters here is additive exploration: "Nothing Luis loves
  is replaced by default" (:46-47).
- StyleStudies.md: "Not chosen" (:4); a record only. Nothing there binds the miners.
- WorkerConcepts.md: superseded by TheMiners.md (:4).

### 1.2 What they ask of a being's body and silhouette

- **One clear idea per silhouette, honestly exaggerated.** "a distinctive, readable silhouette built on one clear idea
  (round and heavy, long and thin, small and quick), with honest exaggeration (E)" (VisualSoul.md:180-182).
- **Individual bodies.** "Different body shapes, noses, shoulders and relaxed gestures create personality"
  (VisualSoul.md:129-130).
- **Small simple shapes against a big world.** "Small, simple character shapes contrast with an oversized organic
  world" (VisualSoul.md:167-168; principle 8 at :197).
- **Readable from the Strategy camera.** "From the Strategy camera, the silhouette and colours say who it is"
  (ModelQualityMethod.md:51); the concepts "read apart even as three dots" (WorkerConcepts.md:39).
- **The soul stays through polish.** "Polish never makes a being generic. The stylization and appeal Luis loved stay"
  (ModelQualityMethod.md:53-54). Luis, October 2: "I like how styalized they are though"
  (Correspondence/2026-10-02_WORKER_CONCEPTS_FEEDBACK.md:9).
- **Proportions were asked about once and never answered.** "The heads are about a fifth to a sixth of the height. Do
  you want more stylized (bigger heads) or more grounded?" (WorkerConcepts.md:104-105). No answer is recorded anywhere I
  found; the miners kept that range.
- **No 3D giveaways.** "no faceted silhouettes, plastic highlights, flat untextured areas" (TheEssence.md:302-303).
- **Drawn contours, flat confident shapes.** "Lively, slightly imperfect contours, and confident flat shapes"
  (VisualSoul.md:183-184); people are drawn, the world is painted (TheEssence.md:285, device 1, built as look F).

Nothing in the guides asks for a thin waist. The waist appears only as construction notes (a smock's "hard rim ... at
the waist", TheMiners.md:670-674; coats that "bell out below the waist", TheMiners.md:97, 143). The "honest
exaggeration" of VisualSoul.md:182 is the only line that could be read as licence for it, and Luis has now said the
waist goes too far (Correspondence/2026-11-14_THE_FLAPS_ON_AND_THE_MODELS_REDRAWN.md:17-19).

### 1.3 Garments

- **Value design:** "Confident dark shapes, such as hair, coats, hides or shells, against lighter ones"
  (VisualSoul.md:187-188); "strong dark hair and coat shapes" (VisualSoul.md:91, :210).
- **Worn surfaces that hint at a life:** "wear, patches, personal objects" (VisualSoul.md:191-193).
- **Costume is open:** "the final cast and costumes are still open" (VisualSoul.md:165-166); the scarf "was only an
  example" (VisualSoul.md:210).
- **Made as a maker makes it; nothing floats; nothing passes through** (ModelQualityMethod.md:34-49). The defect
  catalogue names "a knee through a skirt" as clipping and "Skin or void between ... boot and trousers" as a gap
  (ModelQualityMethod.md:285-286).
- **The one practice behind the floating legs:** "Trousers under a closed coat or smock are cut away above the knee
  (`outfits.trousers(hidden_above=…)`). Skin is only made where it shows" (CharacterPractices.md:25). It was adopted on
  October 3 so trousers "can never poke through" (TheMiners.md:735-736). It is a game-art practice for a cloth that
  never opens; the miners' skirts now open (flaps, lying, raised legs), which is where it fails (section 3.4).
- **Skirt practices:** "A skirt hangs in flaps" on four hip bones, turned by `MinerBody.Drape`
  (CharacterPractices.md:31); "A long coat lies over the knees" without becoming a bell (:33); "Cloth that is not
  touched hangs still" (:32). The junction map requires "Skirt over the trousers (covered, in every stride)"
  (ModelQualityMethod.md:425).
- **Layers move together:** "Layers lying on one another now take the same weights from the same field"
  (TheMiners.md:737-738; CharacterPractices.md:26).

### 1.4 Face

- "Feeling from very few marks. A face ... that expresses much with almost nothing" (VisualSoul.md:185-186); "Simple
  drawn faces carry feeling" (:130).
- Luis "loved every face" (WorkerConcepts.md:5; TheMiners.md:34); the miners kept "The same faces Luis loved,
  unchanged" (TheMiners.md:804). A redraw that changes the faces would touch something explicitly loved.
- Faces are painted into a texture projected from the front (WorkerConcepts.md:88-90); fixed expressions are a known
  limit (TheMiners.md:884).

### 1.5 Hands and posture

- "Posture and small gestures. These carry personality: how a being stands, leans and rests its hands (B, E)"
  (VisualSoul.md:189-190); "a relaxed hand give the figure personality" (VisualSoul.md:92).
- Hands: "A fuller palm and chunkier fingers" (TheMiners.md:668); a hand closes round a handle by grip logic
  (CharacterPractices.md:42-43); "The hand's proportions (a short thumb set near the wrist)" limit the grip
  (TheMiners.md:415).
- Necks grow from behind the jaw, not glued on (CharacterPractices.md:27; Luis, October 3, "a bit too obvious that their
  heads looks glued", Correspondence/2026-10-03_THE_MINERS_FEEDBACK.md:8).
- Stance: "A being standing differently in the game from how it was built" is a defect (ModelQualityMethod.md:292);
  Long stands 11 degrees off its build in the game (TheMiners.md:379-405, question P1).

### 1.6 How a new version is judged

- The model quality method's seven passes and capture matrix (ModelQualityMethod.md:67-254) and its junction map
  (:412-427) apply to any new model.
- Movement is judged by three fresh judges (body, animator, sceptic), none of whom made it (MotionJudging.md:94-122).
  The "aesthetic judges" Luis named are not a written role in MotionJudging.md; they are referred to in
  Docs/Design/AnyBody/stances_strategy.md:355 and the 2026-11-14 decision rows. **Gap:** there is no written brief for
  an aesthetic judge of a model at rest (only for motion). Docs/Reviews/2026-11-14_Flaps_Judges/verdicts.md is the
  nearest precedent (not read here).

## 2. The vision, the decision register and the rationale

### 2.1 Pillars that bind a redraw

- "Aesthetic and mechanical decisions are connected" (**Locked**, GAME_VISION.md:35).
- "The game should reward close observation ... body language" (**Locked**, GAME_VISION.md:39).
- "Movement and combat should be dynamic and procedural, but grounded" (**Locked**, GAME_VISION.md:53).
- The tone is "grounded, poetic, and wonder-filled" (**Locked**, GAME_VISION.md:275).
- **Open** within the Visual Soul: "final proportions ... the final cast and costumes" (GAME_VISION.md:286). The open
  register still asks "Which rendering technique, proportions ... realize the Visual Soul direction" (:417) and "What
  is the scale of strength, and how does a body's appearance change with it?" (:428).
- Guardrail: "a limited early UI must not become a ceiling on character or building customization" (:443).

### 2.2 Decision register rows on the miners, bodies, models, garments, creator, proportions and strength

| Date | Row | Status | GAME_VISION.md line |
|---|---|---|---|
| 2026-10-01 | Models of much higher quality, made with Blender | Direction | 182, 472 |
| 2026-10-01 | One worker with a video-game style character creator, replacing the faction creator in this prototype | Direction | 166-169, 469 |
| 2026-10-01 | Speed is not a creator option | Direction, for now | 172, 470 |
| 2026-10-01 | Strength source: body build sets base strength; training adjusts within limits | Chosen for the showcase (since refined, 2026-11-03) | 475 |
| 2026-10-01 | Visual Soul; proportions open | Direction | 477 |
| 2026-10-01 | The worker model (S1) before the creator (S2) | Chosen | 474 |
| 2026-10-02 | Concepts judged: Small, Long, Round; faces loved; prettier; stylization kept | Luis's ranking | 496 |
| 2026-10-02 | Customization of everything, from modules that fit any body | Direction | 498 |
| 2026-10-03 | The miners "Almost perfect now! ... already adorable!" | Approved in direction | 500 |
| 2026-10-03 | A choice of three miners | Chosen for this prototype | 501 |
| 2026-10-03 | Rigging: body proportions come from each model; a rig adapter | Built, awaiting playtest | 502 |
| 2026-10-14 | Arms as strong as the body needs | Decided, later replaced by 2026-11-03 | 536 |
| 2026-10-30 | Getting up for any body proportion or strength, no per-body teaching | Direction | 538 |
| 2026-11-03 | Capacity-driven procedural getting up, tuned over many bodies, never per body | Decided | 550 |
| 2026-11-03 | Strength from muscle (limb thickness and build), slower with size, weights where the models have them; "all the visual aspects ... really tie to the capabilities" | Decided | 551 |
| 2026-11-03 | Strength per region; muscle shown beautifully; the fat-and-muscle split is the creator's | Direction | 552 |
| 2026-11-03 | Every action for any body | Direction | 553 |
| 2026-11-03 | The creator warns of a body too weak to rise but allows it | Decided | 554 |
| 2026-11-14 | Flaps on their seams on by default | Decided | 542 |
| 2026-11-14 | The models redrawn, prettier and natural; how far **Open**; second version; aesthetic judges; three presets plus a creator with "some" options; research first, then a proposal | Direction (scope Open) | 543 |
| 2026-11-14 | A raised leg's front flap rests; the hem's point rounded; switch-on version only | Decided (which version: Claude's call) | 544 |

"One body for everything" (October 14) is in the auto-memory and the getting-up documents; I did not find it as its
own row in the register (grep for "one body" in GAME_VISION.md found nothing). **Gap:** it binds this work (a part
added for one body goes on all) but is not a register row.

### 2.3 Drift warnings in DESIGN_RATIONALE.md

- **Appearance carries mechanics:** "Appearance should not be an interchangeable skin laid over a spreadsheet"
  (DESIGN_RATIONALE.md:94). A redrawn body whose look changes (a fuller waist, thicker limbs) but whose physics does not
  would drift from this, since strength is read from limb thickness (row 551).
- **Physicality serves presence:** the aim is that "this particular creature occupies space, carries weight"
  (DESIGN_RATIONALE.md:75).
- **A prototype is not a law:** a failure shape is "one successful biped decides the anatomy architecture forever"
  (DESIGN_RATIONALE.md:235); and "Calling a system a prototype must not become a reason to keep it permanently
  superficial" (:223-225). Both argue the rig and proportions may change if the look needs it.
- **Evidence and taste differ:** "A test can prove a foot remained planted. It cannot prove the walk feels beautiful"
  (DESIGN_RATIONALE.md:269).

### 2.4 Luis's words on the miners' look, over time

- October 2 (Correspondence/2026-10-02_WORKER_CONCEPTS_FEEDBACK.md:9): all three "can be much more pretty and
  aesthetically pleasing"; "I like how styalized they are"; "super distinct and unique and not generic and with soul";
  a creator "down to appareance".
- October 2 (Correspondence/2026-10-02_DUSK_DETAILS_AND_THE_WORKER.md:14): the redesign from the guide's principles
  for people, "with heart and soul".
- October 3 (Correspondence/2026-10-03_THE_MINERS_FEEDBACK.md:8): "almost perfect ... already adorable"; necks looked
  glued; the separated feet strange; Round's hands strange; look at every distance.
- October 6 (Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md:50, 76): the creator is part of the
  hi-fi prototype and the miners can be its appearances; held objects allowed "for now, so they look more cute and
  quirky"; the worry that the aesthetic had taken the place of real weight and strength.
- November 3: strength from muscle; muscles shown "in a beautiful and visually pleasing way"
  (Correspondence/2026-11-03_STRENGTH_SIZE_AND_EVERYTHING_ON_GITHUB.md:54).
- November 13 (Correspondence/2026-11-13_ANSWERS_THE_FIVE_QUESTIONS.md:31-34): movement "a little stylized as long as
  it still" reads natural; the same may fairly be read for bodies, but he said it of movement.
- November 13 (Correspondence/2026-11-13_WHAT_TO_JUDGE_AND_THE_TURNS.md:8-21): the "weird leg deattached part" while
  getting up; investigated in Docs/Research/2026-11-13_DetachedLeg/investigation.md (section 3.4 below).
- November 14 (the message that starts this work): the waist "too extreme how thin", legs "floating", "smaller scale,
  not smaller quality".

## 3. How each miner is built (Art/Blender/Worker)

The Blender scripts are the same in the main project and in the work tree (D:/Dev/WG, branch claude/wip-round): workers.py,
outfits.py, rigging.py and body.py compared equal on November 14. Game-side line numbers below are from the work tree,
where the newer game code lives.

### 3.1 The pipeline in one paragraph

A preset in `Art/Blender/Worker/workers.py` (Small :111-133, Long :138-164, Round :169-195) gives shares of height for the
levels (hip, waist, chest, collar), half-widths and half-depths for the trunk at pelvis, waist, chest and collar, limb
lengths and radii, a head radius and size, and an outfit list. `body.py` (:21-73) turns that into a posed skeleton of points.
**There is no torso skin.** The trunk you see is the garments: `outfits.top`, `outfits.skirt`, `outfits.trousers`
(outfits.py:84, :147, :262). Arms, hands, neck and head are skinned flesh pieces; boots and props are separate meshes.
`rigging.py` builds 19 body bones plus 4 skirt flap bones (front and back, left and right), the prop bones and finger
bones, skins every mesh by named rules (:163-264), and exports (:569-626). `weights.py` weighs the flesh at 1000 kg/m3
from the trunk ellipses, limb radii and head (:26, :129 `flesh`), adds what is worn and carried (:213 `worn`), and writes
masses, centres and moments per bone. The export lands in `Assets/_WonderGather/Art/Worker/Miners/Miner_<Name>.fbx` with
`Miner_<Name>_Atlas.png` and `miners.json`; `Assets/_WonderGather/Editor/MinerSetup.cs` turns miners.json into the prefabs
`Miner_<Name>.prefab` and the game's `ProceduralBiped.Proportions` (:380, `torsoRise = shoulder[1] * 0.5`).

### 3.2 The three in numbers (derived from the preset values; the visible widths are estimates, not measured on the mesh)

| | Small | Long | Round |
|---|---|---|---|
| Height | 1.42 m | 1.86 m | 1.56 m |
| Heads tall | 5.46 | 7.28 | 6.22 |
| Hip joint height | 0.619 m (0.436 H) | 0.947 m (0.509 H) | 0.713 m (0.457 H) |
| Thigh = shin | 0.272 m | 0.44 m | 0.32 m |
| Body width/depth at the pelvis | 0.23 / 0.17 | 0.25 / 0.18 | 0.34 / 0.26 |
| Body width/depth at the waist | 0.21 / 0.156 | 0.23 / 0.16 | 0.37 / 0.30 |
| Body width/depth at the chest | 0.24 / 0.17 | 0.28 / 0.19 | 0.35 / 0.26 |
| Waist girth (body) | 0.578 m (0.41 H) | 0.618 m (0.33 H) | 1.055 m (0.68 H) |
| Outer shoulders (sleeve) | about 0.417 m | about 0.448 m | about 0.524 m |
| Top looseness / skirt looseness | 0.024 / 0.026 | 0.016 / 0.018 | 0.018 / 0.020 |
| Skirt hem height, flare | 0.44 m (0.31 H), x1.38 | 0.48 m (0.26 H), x1.30, open front | 0.58 m (0.37 H), x1.15 |
| Trousers | cut above 0.486 m | whole | cut above 0.637 m |
| Arm (upper + fore) + hand | 0.433 + 0.149 m | 0.604 + 0.186 m | 0.468 + 0.179 m |
| Limb radii arm / leg | 0.033 / 0.041 | 0.036 / 0.046 | 0.045 / 0.058 |
| Mass | 40.7 kg | 58.5 kg | 87.6 kg |

Small's head is about 9.96 kg, 24.5 percent of its mass (a child's share, not an adult's: adults are nearer 7-8 percent).
The thigh is always the shin's length (body.py) and the ankle stands 0.085 m up on every body.

### 3.3 How the waist is made, and why it reads "too extreme how thin"

- With a skirt (`skirted=True`, all three), the top stops at the waist: its lowest ring sits 0.02 m below the waist and
  is shrunk to 0.82 of its width and depth so that it tucks inside the skirt (outfits.py:89-94).
- The skirt starts 0.07 m above the waist; its upper rings are fitted to the top's actual surface less about 7 mm, then
  ease out by min(1, 1.6 t) and flare by t^1.5 towards the hem (outfits.py:147-236).
- So the narrowest visible ring is about the bare body waist, with the coat's looseness taken away by the tuck: about
  0.22 m wide on Small. Against the sleeves at the shoulder (the sleeve is the arm's radius x1.2 plus looseness outside
  the shoulder joint) the visible shoulder-to-waist ratio is about 1.9 on Small and 2.0 on Long, against about 1.6 on
  Round, whose waist is wider than its chest (a barrel) and so is not thin.
- Long adds a thin trunk depth (0.16 m at the waist) seen from the side, and the most heads (7.28), so its waist reads
  thinnest of all in proportion to its height.
- A real coat or smock belted at the waist still hangs over the body by its own looseness; here the looseness is
  removed exactly where it would show. **The cause is mostly the garment construction (the 0.82 tuck and the skirt
  fitted to it), not the body numbers**, though Long's body numbers add to it. This is an inference from the code; the
  ring widths have not been measured on the exported meshes.

### 3.4 Garments, layering, and why the legs read detached

- Layering, inside to out: trousers, then the top tucked at the waist, then the skirt over both from the waist to the
  hem. Long wears an open-front coat over whole trousers.
- `outfits.trousers` (outfits.py:262-296) deletes every face whose lowest vertex lies above `hidden_above`, the seat
  piece included (:285-290). Small: hidden_above = 0.30 x 1.42 + 0.06 = 0.486 m (workers.py:125), 0.133 m below the hip
  joint, about half the thigh gone, only 4.6 cm above the hem. Round: 0.37 x 1.56 + 0.06 = 0.637 m (workers.py:185),
  7.6 cm below the hip joint, about a quarter of the thigh gone.
- **Nothing is under the skirt**: no torso skin, no seat, no hips. When a thigh swings forward or out past the skirt
  (sitting, lying, getting up, a leg raised), the viewer sees an open tube end with the dark inside of the tube, and
  air above it. This is what Luis saw as the legs "floating" and the "weird leg deattached part".
  Docs/Research/2026-11-13_DetachedLeg/investigation.md confirmed it: candidate 1 at :189-194; the posed cover grid
  at :307-316, where the cut ring was seen in all 36 poses. The flap fix behind `MinerBody.FlapsOnTheirSeams` (on by
  default since November 14; MinerBody.cs:109) reduced it, but "the root is the art cut" (:315).
- The practice that made the cut (CharacterPractices.md:25, cut-away for hidden parts) assumed a skirt that never
  opens. The miners' skirts now open: the flaps swing with the thighs.
- Skin weights: the trousers are weighted to Pelvis, Spine and the leg bones. The skirt (the "skirt" rule) is Pelvis
  above the hip joint, blends into its four flaps by smoothstep, and is wholly the flaps' from a hand's width above the
  hem; above the pelvis it blends into Pelvis, Spine and Chest. The flap bones run from the hip joint to the hem height.
- **The leg through the skirt (Luis's fault), a hypothesis, not checked:** vertices at the side seams are split between
  the front and the back flap, and `MinerBody.Drape` turns a flap by how far the thigh comes forward or back under it,
  "not by how far it goes out to the side" (MinerBody.cs:685-687, by design so that a long coat's front does not stand
  out as a board). A knee opened sideways, or a leg bent at the side seam, should pass through the cloth there. Other
  possible causes: a flap that cannot follow both a bent thigh and a straight one; the skirt's stiff upper band. The
  aesthetic judges' verdicts on the flaps (Docs/Reviews/2026-11-14_Flaps_Judges/verdicts.md) were not read here.

### 3.5 The rig, the weights and the export

- Rest pose: feet under the hips, arms 25 degrees out. LODs at 16000, 5000 and 1600 triangles; one 2048 atlas per
  miner. FBX axes: -Z forward, Y up.
- miners.json carries, per miner: height, hipHeight, hipWidth, leg, ankleHeight, heel/ball/toe, waistRise, shoulder,
  headRise, upperArm, forearm, arm out/drop/forward, gait, armCarry, hanging, skirtSlack, bodyFront, faceFront,
  headHalf, grips, and the weights (mass; per bone its mass, centre along the bone and moments; armRadius; legRadius).
- `workers.shape` (workers.py:352-378) measures `bodyFront`, `faceFront` and `headHalf` **on the clothed model at
  rest**: whatever stands furthest ahead of the hips between the hips and the shoulders (clothes, apron pocket, hammer,
  strap; the lantern and mug on their hooks excluded), and the widest point of the head with hair and cap. A garment
  change therefore changes numbers the game uses (section 4).
- `skirt_slack` (outfits.py:237-260) measures how far each flap may swing before the cloth runs out (Long [0, 12.32]
  degrees); it changes with any skirt change.

## 4. What the physics and the getting up take from the rig and the model

Source for the map: Docs/Design/AnyBody/body_map.md (read-only map of the work tree), its table of who reads which
number (:397-415). Its rule: **"the model's mesh never reaches the physics"**. A physical body is made from
`ProceduralBiped.Proportions` (23 numbers), box sizes, and `PhysicalBody` (masses per bone, armRadius, legRadius,
Strength). The rig's bones reach it only through the arm needs, the upper-body asks and the posed centre of mass.

### 4.1 Where the model's numbers go

- Colliders, the same in `OwnBody.Make` and `PhysicalFall.LetGo`:
  - the hips capsule radius is clamp(bodyFront, leg x 1.5, 0.3 x hipHeight) (OwnBody.cs:696, PhysicalFall.cs:472);
  - the trunk rod's breadth is max(shoulder.x - arm, 0.5 shoulder.x);
  - the head sphere is max(0.06, headHalf) (OwnBody.cs:713);
  - the thigh rod is leg x 1.25 and the shin rod leg; arms use the arm radius (OwnBody.cs:736);
  - the boot boxes are a fixed 0.10 x 0.06 on every body.
- Masses and moments: from weights.py, flesh plus what is worn.
- Strength: the work tree's muscle rule reads the limb radii and masses (knee 1.6 x need, hip 1.5 x knee, ankle 0.7 x,
  back 2.4 x, arms max(thickness, 1.6 x need), neck 3 x). The three miners keep the strengths they had before the rule:
  `PhysicalBody.KeepsWhatItHad()` (PhysicalBody.cs:144) is called by MinerSetup.cs:512 and :534 whenever a miner is made.
- Gait speed scales with leg length (Froude); OwnRiser's asks are given in shares of hip height and leg length.
- The ways to get up: `Assets/_WonderGather/Resources/RiseWays/Small.txt` is the only ways file and is found by the
  miner's name, so Long and Round get up the old way. The ONNX models, the tests and OwnRiser's settings were searched
  on Small. The stance atlas (Layer 1 solver, the BodyFrame bridge) was fitted per body.
- Known offsets: the physics' centres sit lower than measured (the head is centred at the bone's base, about 8 cm low);
  Long stands 11 degrees off (P1).

### 4.2 What a redraw can change without touching the physics

These change no number the physics reads, as long as the trunk and limb numbers, levels and bones stay as they are:

- garment looseness at the waist, removing or easing the top's 0.82 tuck, and the skirt's fit and flare;
- continuing Small's and Round's trousers up to a seat under the skirt (removing or raising `hidden_above`), or a
  modelled body or under-layer under the coat;
- the face, hair, hands' shape, textures, the atlas and the LODs;
- the side-seam weighting of the skirt, and more flaps.

Three caveats:

- weights.py's worn mass changes a little with more cloth (a full trouser seat), and so do the centre of mass and the
  moments;
- `bodyFront` is measured on the clothed model, so a looser coat at the belly moves the hips capsule's radius (within
  its clamp) and the tool's clearance;
- `skirtSlack` is recomputed, and the cover test, the flap judges and the hand grips should be rerun.

### 4.3 What changes the physics, and what must then be retuned

- Trunk widths and depths: mass, the back's need, bodyFront.
- Limb radii: the strength floor and the muscle rule, the colliders.
- Heights and levels: the Proportions, the gait speed, the shares OwnRiser asks in.
- Head size: headHalf (the skull collider) and the head's share of mass. A smaller head on Small moves its centre of
  mass and its rise.
- Bone positions: the arm needs.

Then re-check or retune: KeepsWhatItHad (do the redrawn miners keep the old strengths or take the muscle rule?);
Small.txt's ways to get up; OwnRiser and OwnKeeper settings; the stance atlas fits; the PlayMode suite; the idle Luis
loves (at 14 and 4); and the three miners through a whole work-tire-rest cycle, Long first.

### 4.4 Asset GUIDs and a second version beside the first

Luis wants the new version switchable beside the first. Replacing `Miner_<Name>.fbx` in place keeps the GUIDs and
overwrites the loved version. A second set (for example `Miner_<Name>_v2.fbx` with its own atlas, json entry and prefab)
keeps both and needs a switch: MinerChoice/MinerPanel today choose among the three by name, and the ways file is found
by name. Ways to build the bodies (body_map.md):

- Way A, scaling an existing model's bones, breaks: Unity shears non-uniform bone scale.
- Way B, a Blender preset per body, is faithful and takes minutes per body.
- Way C, new Proportions on an existing prefab, is inconsistent.
- Way D, a made body from primitives, is cheapest and faithful.

A redraw of the three is Way B.

## 5. The creator

What exists:

- No character-creator code. `Scripts/Creator` holds only the Faction files.
- MinerChoice and MinerPanel choose among the three miners and flip switches.
- The made-body sampler (Way D) can make a body of any proportion for tests.

What the project says the creator will be:

- ShowcaseRoadmap.md:14 and :179-188, S2, the worker creator:
  - a UI Toolkit scene with a 3D preview;
  - categories: body (height, build, strength), face and hair, clothing and colours, tool, hauling gear, name;
  - presets and a randomize button, both optional;
  - a consequences panel, saying what the choices do to work;
  - versioned character saves.
  - S2 also states: "the three appearances, strength as one number that shows gently in the body".
- ShowcaseRoadmap.md:259, O5 lists the categories. O6: MakeHuman/MPFB only after asking before any download.
- WorkerShowcaseVision.md:54 names Dark Souls as the example of depth.
- TheMiners.md:836-863: modules as a creator; "every module fits any body"; the choices of today.
- Register rows on the creator (GAME_VISION.md:498, :552, :554): the creator warns of bodies that cannot get up; any
  body proportion and strength gets up by its own procedural capacity, with no per-body teaching.
- Correspondence 2026-10-06:50: the miners can be the creator's appearances.
- Luis, November 14: three presets plus "not too many, but some" options.

What a redraw implies for the creator:

- If the three presets are to be points in one creator space, the redraw should make them from one parametric body:
  the same garments fitting any trunk, and trousers that are never cut, because a creator cannot know which skirt
  will hide what.
- The cut-away practice is a per-model shortcut that a creator cannot use safely.

## 6. Contradictions, gaps and open questions

1. **"Honest exaggeration" against "too extreme".** VisualSoul.md:180-182 asks for honest exaggeration of the body;
   Luis now finds the waist too thin. The guide does not ask for a thin waist anywhere, so the waist is a construction
   artefact (3.3), not a principle. How far to take back the stylisation is still Luis's call.
2. **The cut-away practice against skirts that open.** CharacterPractices.md:25 says cut away what is hidden; the
   flaps now open the skirt. The practice should be amended for garments that move; amending a guide needs Luis's
   word.
3. **Luis loved every face, and wants them prettier.** A redraw risks the faces he loved ("change only what is
   asked"). The faces are not among the faults he named. Open: are the faces part of "prettier", or kept?
4. **Proportions are Open** (GAME_VISION.md:286), and the head-proportion question in WorkerConcepts.md:104-105 was
   never answered. Small's head is 24.5 percent of its mass. A smaller head is prettier by adult norms, but changes the
   physics (4.3).
5. **Strength and build.** The miners keep the old strength rule (KeepsWhatItHad). Should the redrawn bodies show
   their build and take the muscle rule? November 3: muscles shown "in a beautiful and visually pleasing way".
6. **How far the change goes is Open** (Luis, November 14): visual model only, or rig and body too. Section 4.2 lists
   what can be fixed visual-only: the waist, the floating legs, and probably the skirt clipping.
7. **The two code trees differ.** The main project's code lacks FlapsOnTheirSeams and the muscle rule that the work
   tree has. A redraw exported from the shared Blender scripts lands in both; the game code that drives it is the work
   tree's.
8. **No brief exists for an aesthetic judge of a model at rest** (1.6). Luis asks that judges see the new version
   first; one should be written from ModelQualityMethod.md and VisualSoul.md before the judges run.
9. **"One body for everything" is not a decision register row** (2.2), although Luis said it on October 14 and it
   governs the redraw (toes, a body under the coat, the same on all three).
10. **Getting up still shows unnatural leg movements** (Luis, November 14). Part of it may be the cut ring and the flaps
    (drawn), and part the riser (moved). The (c)(c) course results were not read here, so this file cannot say which.
11. **Held objects were allowed "for now"** (October 6). A redraw is a moment to ask whether they stay.
12. **Long's 11-degree stance offset** (P1) and the low physics centres are open faults that a rebuild should not hide
    or worsen.
13. **The second version needs a switch.** Nothing in MinerChoice selects a version today, and the ways file, the
    tests and the ONNX models are keyed by the name "Small".

## What this file did not do

- No image was opened: no model, render or reference picture was looked at. The visual claims are read from the code
  and the documents.
- Blender and Unity were not run. The visible waist widths and shoulder-to-waist ratios are estimates from the preset
  values and the garment code, not measurements of the exported meshes.
- The leg-through-skirt cause is a hypothesis from the code (3.4); it was not tested.
- No web search was made. This file is the project's own sources only.
- The (c)(c) course results, the flap judges' verdicts (Docs/Reviews/2026-11-14_Flaps_Judges/verdicts.md) and the
  ONNX/rise data were not read.
- Game-side line numbers are from the work tree (claude/wip-round). The main project's files may differ.
- Every row of GAME_VISION.md's register was not reread: only the rows on bodies, models, garments, the creator,
  proportions and strength.
