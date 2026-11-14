# The miners redrawn: the synthesis of part one

November 14, 2026 (night). This is the one synthesis of part one of the research Luis asked for before any proposal
(Docs/Correspondence/2026-11-14_THE_FLAPS_ON_AND_THE_MODELS_REDRAWN.md). It was written, then checked by a separate
critic (`critique.md`), then revised. Section 10 lists every critic point and what was done.

It rests on three files in this folder:

- `internal.md`: what the project's guides, vision, decision register and code say;
- `audit.md`: the three LOD0 miners rendered in Blender at rest and in six poses, and measured (pictures in
  `pictures/`, sheets in `pictures/sheets/`);
- `external.md`: 44 outside sources, tagged S1 to S44.

It also uses the game's own run of November 14, late evening (Docs/Validation.md, "The cut ring as played"), the flap
judges' verdicts (Docs/Reviews/2026-11-14_Flaps_Judges/verdicts.md), and two of the art references
(Docs/ArtDirection/References, B and E), opened for this revision.

Nothing here is a decision. The Game Director decides. Every Open question stays Open. This is research for a
proposal, not a work plan: row 2026-11-14 of the register says the proposal comes "for him to agree to before
anything is built".

**How each claim is marked:**
- **(measured)**: a number taken from the meshes or from a game run;
- **(code)**: read in the code;
- **(picture)**: seen in a picture, named by its path in this folder (or in Docs/ArtDirection/References);
- **(source Sn)**: from an outside source in `external.md`;
- **(record)**: read in a project document or in Luis's words;
- **(hypothesis)**: an inference, not checked.

---

## Start here: the short version

**The faults, ranked** (section 2 has the evidence):

1. **The pinched waist.** Small and Round are corseted under their coats; Round, meant "round and heavy", is the worst.
   The garment code throws away the waist the body numbers already give them.
2. **The floating legs.** Small's and Round's trousers stop below the hip, with nothing above. On the ground the cut is
   seen from at least one capture view on nearly every frame.
3. **Cloth that behaves as boards.** Each skirt is four rigid plates. A raised thigh goes straight through the coat;
   panels stand as tents and fins; on all fours Small's coat balloons into a sack.
4. **The getting-up leg movement.** Measured folds past a person's range in Small's own get-up. Most likely the
   physical body, not the model (a hypothesis until a picture of the deepest fold is read).
5. **Thin limbs.** Stick thighs on all three; slim forearms on Round, the strong one.
6. **Joints and legs without structure.** Tube arms, pipe knees, legs with no calf or ankle taper.
7. **Flat side profiles.** Little chest, belly or seat from the side.
8. **Smaller things:** Small's flat grey trousers, necks that sink into collars, Small's hair hem, Long's profile,
   Small's pale face in daylight, Long's 11 degree stance; and the props overlapping legs in poses (unverified).

**Two roots, not one** (section 3):
- **The garment construction:** a trunk made only of smoothed garment tubes, tucked at the waist and cut where it was
  thought hidden. It makes the waist and the floating legs.
- **The rigid flaps and the drape rule:** one bone per flap, no contact with the legs, a rule that ignores a sideways
  thigh. It makes the clipping, the boards and probably the sack.
- Long's thin waist is mostly his own body numbers, not the garment.

**What the art references show** (section 4.1, picture): in reference E the round woman has no waist at all; her coat
and apron hang straight from the chest over a barrel body. The long man's coat hangs straight too, and his legs show
whole beneath it. Coats are loose, folded, patched and worn. Nothing in the references, the guides or the sources asks
for a pinched waist.

**The recommended path** (section 7):
1. **Stills first, before anything is built.** Blender stills of two looks beside today's: a "look only" version
   (natural waist from each body's own numbers, whole trousers, the apron from the bib) and a "fuller body" version
   (thicker thighs, a belly, a seat). Aesthetic judges first, then Luis. He chooses how far from pictures.
2. **A1, static art:** the waist, the whole trousers (or a simple body under the coat), painted; the coat's upper band
   re-weighted to the thighs on today's four flap bones. No game code changes.
3. **A2, the skirt mechanism:** flaps that follow the thigh and bend, with leg capsules, in the game code behind a
   version switch.
4. **B, proportions that reach the physics:** only with Luis's word, and only after the own get-up's blow-up is
   understood.
5. **C, the creator's body:** with the S2 creator, after the way a creator body is made has been researched.

Separate from all this: the raised leg's flap rest and the rounded point, already decided, go on with the flaps-on
drape for today's models.

---

## 1. What Luis asked, and what this answers

### 1.1 His words (November 14, evening)

- The legs: "even with this fix, there's a lot of issues from just the model ... being floating legs". He wants "the
  legs actually seem connected to the body".
- How big: "I think we probably need to actually redesign the model to some extent", and "I think we should actually
  make this big transformation".
- The waist: "I think it's too extreme how thin the waist is on the characters. It doesn't look natural and doesn't fit
  the aesthetic of being something pretty to look at."
- All of it: "make all the models much more pretty in general", "continuing with all the aesthetic guides that we
  have", and "look in all the models for anything that doesn't look natural".
- How far: "I don't know if actually to the actual model and rigging and all that or part, or just the visual part".
  He left this open.
- The prototype: "these 3 model suggestions", and a creator from the prototype's options: "It shouldn't be too many, but
  should be some."
- Two faults he sees: "the leg will just clip through the skirt ... literally just clipping it"; and "I still see some
  unnatural leg movements" while getting up, adding "I think we didn't touch the getting up part yet".
- The bar: "smaller scale, not smaller quality".
- Then: "a really deep dive research like we were doing for the other subjects".

He asks for a big transformation and leaves open how deep it goes. The path below keeps both: it shows the bigger look
early, as stills, and lets him choose the depth from pictures.

### 1.2 What this file answers

1. What is wrong now, every fault found, ranked (section 2).
2. Where each fault is made, with how sure we are (section 3).
3. What the references, the guides and the sources ask of a pretty, natural miner (section 4).
4. How far the change could go: the rungs, with what each fixes, leaves and risks (section 5).
5. What the creator could offer, and what the physics can bear (section 6).
6. A path for the proposal, and the questions only Luis can answer (section 7).
7. How each stage would be checked (section 8).

It does not choose a look. The looks are to be made, judged by aesthetic judges, and then shown to Luis.

---

## 2. What is wrong now

Ranked by how much each fault harms "pretty and natural". Severity follows the audit's scale: **B** breaks the look,
**V** clearly visible, **m** minor. The audit's fault numbers (S1 to S11, L1 to L8, R1 to R10) are kept so that each
fault can be found there with its pictures. (The audit's S-numbers are faults on Small; the sources are also S-numbered,
but always written "source Sn" here.)

### 2.1 The pinched waist: an hourglass under every coat (B on Small and Round, V on Long)

- **What:** each trunk is a ball chest, a deep notch at the waist, then a bell skirt (S1, L1, R1). At the waist the
  visible front width is 0.125 m on Small, 0.183 m on Long and 0.133 m on Round (measured).
- **Against the miners' own bodies.** This is the comparison with the firmest footing. The presets, which the physics
  already uses, give natural waists: waist width against chest width is 0.88 on Small, 0.82 on Long and 1.06 on Round
  (measured from the preset table, internal.md 3.2). The drawn waist is far below them:
  - Small's visible waist is 60 percent of her body's (0.125 against 0.21 m);
  - Round's is 36 percent (0.133 against 0.37 m) (measured).
  - Round's preset is a barrel, wider at the waist than at the chest. Her smock turns her into the most corseted of the
    three. Her one clear idea, "round and heavy", is lost (record: VisualSoul.md:180-182).
  - So the physical body is already wider than the drawn one. Fixing the drawing brings the look and the physics
    closer, not apart.
- **Against the sources, by girth** (the one comparison the sources allow). Waist girth against hip girth, both from the
  audit's hull sections: Small about 0.35 to 0.38, Long about 0.61, Round about 0.31 (measured; computed here from
  audit.md section 2). European art over 2,500 years runs 0.67 to 0.80 for women; the most waist-marking art averages
  about 0.5 (source S1, S2). Small and Round are below even the most extreme art. Long is just under the art range.
  - A caution: the audit's hip hull takes in the skirt's flare, so the hip girth is the garment's, not the body's. That
    lowers the ratio somewhat. Art figures are drawn silhouettes too, so the comparison is rough, not exact.
  - Front-width ratios (waist against chest: Small 0.53, Long 0.70, Round 0.41; waist against hips: Small 0.37, Round
    0.30; measured) have no direct source. They show the shape, not a measured gap from nature.
  - The drawing canon makes the ribcage and the waist the same width, and the waist about 0.67 of the shoulders
    (source S3). It is a male canon, so it guides the build, not a woman's waist.
- **Round's apron** clings into the notch instead of hanging straight from the chest, and deepens it (R2).
- **Where it shows:** every front and back view, at the game camera, and even from straight above in the game
  (picture: `pictures/Small_rest_front.png`, `pictures/Round_rest_silhouette_front.png`,
  `pictures/Small_game_c07_f029202_knee_above.png`, `pictures/sheets/all_rest_silhouettes.png`).
- **A second harm, seen in the silhouettes:** the three say the same shape three times, ball, notch and bell. Size and
  colour tell them apart, not the body (picture: `pictures/sheets/all_rest_silhouettes.png`). On Small and Round the
  ball chest also reads like a bust over a corset (picture: `pictures/sheets/rest_Round.png`). That is an impression,
  not a measurement. Part of it may be the bind pose, with the arms 25 degrees out, which the idle does not use (see
  2.9).

This is the fault Luis named. The numbers bear him out on Small and Round. Long's waist is thin too, but for a
different reason (3).

### 2.2 The floating legs: trousers cut off under the coat (B on Small and Round)

- **What:** Small's trousers stop 13 cm below her hip joint, Round's 7 cm below hers. The skirt hem hangs only 7.5 cm
  (Small) and 12.5 cm (Round) below the trousers' top (measured). Above the cut there is nothing: no seat, no hips, no
  body (code: `outfits.trousers` deletes every face above `hidden_above`, the seat included).
- **When it shows:** whenever the hem lifts: a raised knee, sitting, kneeling, lying, a flap swinging. The viewer sees
  two grey tubes that start in the air, with an open, dark tube end and the ground between legs and coat (S2, R3)
  (picture: `pictures/Small_back_close_hips.png`, `pictures/Round_sit_close_hips.png`,
  `pictures/Small_game_c00_f001150_knee_headright32.png`, `pictures/Small_game_c04_f016246_knee_side47.png`).
- **How often, in the game as played** (measured, Validation.md, "The cut ring as played", flaps on). A row counts when
  the cut ring is seen from **at least one** of the capture's camera ways (25 views). How often the player's one camera
  sees it was not counted.

  | Run | Lying | Getting up | Standing |
  |---|---|---|---|
  | Small, its own get-up (`cc_c1`) | 7052 of 7324 | 18533 of 21445 | 14 of 59 |
  | Small, the old way (`cc_c2`) | 886 of 898 | 1064 of 1112 | 9 of 50 |
  | Round, the old way (`cc_c3`) | 1039 of 1195 | 2198 of 2319 | 1 of 50 |

  So on the ground the cut is visible from some view on nearly every frame, for Small and for Round. The flaps on their
  seams barely change it. The fix is the art, not the drape.
- **Long:** his trousers are whole and reach 4.6 cm above his hip joint (measured). No gap shows in his posed stills
  (picture: `pictures/sheets/close_poses_Long.png`). He was not run in the game, so "free of it in play" is not
  tested. His open coat front shows a dark void over a raised knee, which is a different fault (L2, in 2.3). He is the
  pattern for the other two's trousers.

### 2.3 Cloth that behaves as boards: the leg through the skirt, fins, tents and a sack (B in motion)

Each skirt is four rigid plates on four bones (front and back of each leg). None can fold, bunch or drape over a leg
(code; audit section 6). This is the largest kind of fault in motion, and it holds Luis's "literally just clipping".

- **The leg straight through the coat.** A raised thigh pierces Small's coat high, near the hip, and from the right it
  comes out through the side seam (S3) (picture: `pictures/Small_kneeup_close_hips.png`,
  `pictures/Small_kneeup_close_hips_right.png`, `pictures/Small_stride_close_hips_right.png`). Sitting, both thighs
  come out through the coat's sides (S4) (picture: `pictures/Small_sit_close_hips.png`). Round's thigh passes through
  the smock the same way (R4) (picture: `pictures/Round_kneeup_close_hips_right.png`).
- **Boards and tents.** Long's raised knee pushes the front panel out as a stiff flat tent with a dark void inside
  (L2) (picture: `pictures/Long_kneeup_close_hips.png`). Kneeling, his back panel stands out as a fin and the front as
  a spike (L3) (picture: `pictures/Long_kneel_close_hips.png`). Sitting and lying, his panels lie as flat stiff sheets
  (L4, L8) (picture: `pictures/Long_sit_close_hips.png`, `pictures/Long_back_threequarter.png`). Round's smock and
  apron stand out as a shelf when she kneels (R5), and pass through each other lying (R6) (picture:
  `pictures/Round_kneel_side.png`, `pictures/Round_back_close_hips.png`).
- **The sack on all fours.** In the game's get-up, on hands and knees, Small's coat balloons into a hump over her
  hips, bigger than the body under it (S5). It was seen in three game frames, from three different courses (picture:
  `pictures/Small_game_c02_f007676_knee_side47.png`, `pictures/Small_game_c03_f008739_knee_side47.png`,
  `pictures/Small_game_c00_f000957_hip_side47.png`). The audit ranks it the strongest fault in the game after the
  waist. Every own get-up passes through hands and knees (record: GAME_VISION.md, 2026-11-11), so it is likely common
  (hypothesis: how many frames show it was not counted).
- **The stiff tip.** Lying, the coat's corner stands up as a stiff point (S6) (picture:
  `pictures/Small_game_c04_f016246_knee_feetleft32.png`). The flap judges saw a pointed front hem to the ground with
  the switch on, and a flap standing against gravity, as a fin or a hood, over a leg raised past square (record:
  verdicts.md). Luis has decided the raised leg's front flap gets a rest and the point is rounded, on the flaps-on
  version (record: GAME_VISION.md, 2026-11-14). That is its own task, on today's models (see 7.2).
- **Cloth under the ground.** In Small's own get-up the drape lies more than 20 mm under the ground on about a third
  of the rows (measured, Validation.md, bar 7). Not yet seen in a picture.

### 2.4 The getting-up leg movement (motion; Luis's fault; not judged in pictures here)

Luis sees "unnatural leg movements" while getting up. Part one did not judge this by eye. What the game's run of
November 14 measured, for **Small only** (Validation.md, bars 3, 4 and 6):

- In Small's own get-up (`cc_c1`, the blown-up course left out), 8146 of 19376 getting-up rows fold the hip past a
  person's range (bar 3; its exact limit is not restated in that entry). The deepest fold is 172.4 degrees (course 3,
  frame 11517) (measured). The old way fails far less: 77 rows for Small, 65 for Round.
- The knee is bent past 150 degrees on 1574 leg-frames of the own get-up (measured).
- The drawn hip follows the physical body's own links to within about a degree at the median, so these are real folds
  of the physical body, not a drawing fault (measured). The own body's hip is a free joint with no limits (record:
  Validation.md).
- There are fast spins and jumps of limbs; the largest spin was 165.4 degrees in one frame, the left shin (measured).
- **Long has no getting-up data at all.**
- **One course blew up.** Course 4 of `cc_c1` spun up to 8e21 and went to NaN; the test still passed, because the
  courses ignore failing log messages. And **a course does not run again the same**: the same build and push stood
  after 3 tries in one run and 6 in another (`cc_det`) (measured, Validation.md). So getting-up numbers from a few
  courses are noisy, and a model change could not be told apart from that noise.

So the movement Luis sees most likely comes from the body that gets up, not from the model (hypothesis: the numbers
point there, and Luis's "we didn't touch the getting up part yet" agrees, but no picture of the deepest fold has been
read). The model adds to it: the cut ring and the cloth boards are seen on the same frames (2.2, 2.3). The redraw can
remove what the model adds. It cannot fix the folds; they belong to the getting-up work (joint limits on the body that
gets up are built behind `OwnBody.Anatomical`, off; record: Handoff.md).

### 2.5 Thin limbs that show no strength (V)

- Thighs: Small 0.07 H, Long 0.064 H (the thinnest), Round 0.092 H (measured). Long's legs read as stilts under a heavy
  coat (L5); Small's as sticks under a wide skirt (S7); Round's as pegs under 0.44 m hips (R8) (picture:
  `pictures/Long_rest_side.png`, `pictures/Round_rest_front.png`).
- Round's forearms and wrists are thin against her heavy body and her hammer (R7) (picture:
  `pictures/Round_rest_close_hands.png`). The strong one has a slim one's arms.
- This is a fault of the look, against the Direction that appearance ties to capability (record: GAME_VISION.md,
  2026-11-03). It does not break the game's strength rule today: the three miners keep their old strengths, not the
  muscle rule (code: `PhysicalBody.KeepsWhatItHad`). Whether it becomes a rule matter depends on question 5.

### 2.6 Joints and legs without structure (V close, m at the game camera)

- Arms are tubes of nearly the same thickness from shoulder to wrist, with no elbow point, no forearm swell and no wrist
  taper; Small's cuffs puff out wider than the forearm (S8, L7, R10) (picture: `pictures/Small_rest_close_hands.png`,
  `pictures/Long_rest_close_hands.png`).
- Knees bend like a pipe, with no kneecap and no change of volume (S10, R10) (picture:
  `pictures/Small_kneel_close_hips.png`).
- Legs are straight columns, with no calf, no ankle taper and no change from thigh to knee (picture, read by the
  critic: `pictures/sheets/all_rest_silhouettes.png`). To be looked at again.
- The boots read as large black slab soles in profile and kneeling (picture, read by the critic:
  `pictures/Small_kneeup_close_hips_right.png`, `pictures/Small_kneel_close_hips_right.png`). The laced boots are on the
  keep list, so this is a question of the sole's shape only. To be looked at again.

### 2.7 Flat side profiles (V)

Chests, bellies and seats have little depth. From the side the three are thin columns with a skirt; Round has little
belly for a "round" body (S7, R9) (picture: `pictures/Small_rest_silhouette_side.png`,
`pictures/Round_rest_silhouette_side.png`).

### 2.8 Minor faults

- **Small's trousers** are a flat grey with almost no painted texture, unlike her coat and the other two (picture, read
  by the critic: `pictures/sheets/rest_Small.png`, `pictures/sheets/close_poses_Small.png`). The frame test bars flat
  untextured areas (record: TheEssence.md). Whole trousers and a seat would be seen far more, so they must be painted.
- **Necks:** Round's head sits almost on her shoulders, and Small's collar swallows her neck (picture, read by the
  critic: `pictures/sheets/all_rest_close.png`). Luis said on October 3 that heads looked glued (record, via the
  critic). To be looked at again.
- Small's hair hem reads as a spiky blob from behind and above (S9) (picture: `pictures/Small_rest_back.png`).
- Long's head juts forward and his nose is a sharp spike in profile; charming from the front (L6) (picture:
  `pictures/Long_rest_silhouette_side.png`). This touches a face Luis loved; it is a question, not a fault to fix.
- Small's face washes out pale in daylight (S11) (record: Docs/Images/Miners/Miners_DayStrategy.jpg). A light and
  shading matter more than a model one.
- Long stands about 11 degrees off his build in the game (record: TheMiners.md, question P1, still open).

### 2.9 Unverified: the props against the legs, and the bind pose

- **Props overlapping legs** recur across the posed pictures, not in one or two: Small's lantern behind the raised
  thigh and between the thighs sitting; Long's mug inside the front thigh kneeling and under the thigh sitting; Round's
  hammer against the thigh kneeling and upright into the ground (picture: `pictures/Small_kneeup_close_hips.png`,
  `pictures/Small_sit_close_hips.png`, `pictures/Long_kneel_close_hips_right.png`, `pictures/Long_sit_close_hips.png`,
  `pictures/Round_kneel_close_hips_right.png`, `pictures/Round_side_close_hips.png`). The audit aimed hanging props
  straight down and did not let the body stop them, which explains some of this (record: audit method). In the game
  each prop is a pendulum the body stops (record: CharacterPractices.md, section 2). Nobody checked the game's frames.
  **Status: unverified, probable in part.** Check the stops in the game for all three.
- **The bind pose.** Every rest render has the arms 25 degrees out, a pose the game's idle does not use. The ball chest
  and the bell may read partly from the arms and shoulders (hypothesis, from the critic). Render the idle's arm pose
  before judging the chest.
- `pictures/Small_game_c07_f029202_knee_feetleft32.png` is an empty frame: the capture camera was inside something. A
  fault of the capture, not of the model (picture).

### 2.10 What is good and must be kept

From the audit (section 8) and the record: the faces Luis loved (Small's big eyes, Round's closed-eye smile, Long's
nose from the front); each one's colour (blue coat, brown long coat, green smock with the apron); the heights (1.42,
1.86, 1.56 m); Long's whole trousers; the props that hang and are held (lantern, satchel, mug, hammer, lamp); the laced
boots and the fuller palms; the hair ideas; the portrait gestures (Round's hands on hips, Small's hand at the collar,
Long's mug); the stylisation and the tonal palette. Also the idle standing Luis loves (record: GAME_VISION.md,
2026-11-13 night), which any change that reaches the physics must keep.

### 2.11 What part one could not judge

- **The getting-up leg movement by eye.** It was measured (2.4), not looked at in motion.
- **Long and Round in the game's own frames.** All game pictures read are Small's. Round's cut ring was counted in the
  game; Long was not run.
- **The real drape in motion.** The posed pictures use the audit's static stand-in for `MinerBody.Drape`, not the
  game's own drape: no swing, speed or contact.
- **LOD1 and LOD2.** Only LOD0 was rendered.
- **Shoulders and elbows in strong arm poses** (arms raised, a hammer swung, a reach), and the idle's arm pose.
- **The hip's skinning at deep folds.** The skirt hides the hips in most poses; with the legs joined under the coat,
  the hip's bend will start to show (see 5.1).
- **Faces and light at every hour in the game,** and the strategy camera's read of any change.
- **The faults the critic added** (2.6 legs and boots, 2.8 trousers and necks) were read in the pictures by the critic,
  not re-checked for this revision.

---

## 3. The causes

Each fault traced to where it is made. Confidence: **high** (read in code and matched by measurement or picture),
**medium** (read in code, or seen, but the mechanism not tested), **low** (a hypothesis).

| Fault | Where it is made | Evidence | Confidence |
|---|---|---|---|
| The pinched waist on Small and Round (2.1) | The garment, not the body numbers. `outfits.top` with a skirt builds the torso as a skin chain of only four rings, waist, chest, collar and neck (code: outfits.py:84-118). The waist ring is the chain's end, and is shrunk to 0.82 (code: :92-94). The chain is then smoothed twice: subdivision at level 2 (code: shapes.py:103-117) and a voxel remesh with six rounds of smoothing (code: shapes.py:246-259, `smooth=6`). The skirt is fitted to the top's real surface less about 7 mm (code: outfits.py:147 on), so it follows the notch down | The preset waists are 0.21 m (Small) and 0.37 m (Round); the tuck alone would give about 0.17 to 0.21 m on Small, but 0.125 m is measured. The extra loss is where the end of a smoothed chain rounds off, and where the smoothing shrinks the narrow part most | High that it is the garment's construction; medium on how much each step takes (no top was rebuilt to test it) |
| Long's thin waist (2.1) | Mostly his preset: his body's waist girth is 0.618 m, 0.33 H, the thinnest of the three; his visible waist girth is 0.63 m, about the same. The garment takes away only the coat's looseness (measured; internal.md 3.2, 3.3) | Girths match | High. A garment fix alone will not widen Long much; that is a proportion choice (rung B) |
| The ball chest and three like silhouettes (2.1) | The same chain: one ring at the chest, wider than the waist and the collar, smoothed into a ball (code). Possibly helped by the bind pose's arms (2.9) | Seen in every front silhouette (picture: `pictures/sheets/all_rest_silhouettes.png`) | Medium |
| Round's apron in the notch (2.1) | `outfits.apron` lays the bib on the clothes point by point down to the ties (record: CharacterPractices.md, the apron row), so it follows the notch | Picture: `pictures/Round_rest_close_waist.png` | High |
| The floating legs (2.2) | A practice: "Trousers under a closed coat or smock are cut away above the knee" (record: CharacterPractices.md:25). Small `hidden_above` = 0.486 m, Round 0.637 m (code: workers.py:125, :185); faces above it deleted, seat included (code: outfits.py:262-296) | The ring seen from some view on nearly every lying and getting-up frame (measured) | High |
| The leg through the coat, high (2.3) | The skirt's upper band is weighted to the pelvis above the hip joint and only blends into its four flaps lower down (code: the `skirt` rule in rigging.py); a thigh raised high meets cloth that cannot move | Picture: `pictures/Small_kneeup_close_hips.png`; matches the code | Medium-high |
| The leg out through the side seam (2.3) | `MinerBody.Drape` turns a flap by how far the thigh comes forward or back under it, "not by how far it goes out to the side", on purpose, so that a long coat's front does not stand out like a board (code: MinerBody.cs:685-687, work branch; line not re-checked). A knee opened sideways leaves the cloth. Seam vertices are split between the front and back flaps | Picture: `pictures/Small_kneeup_close_hips_right.png`, `pictures/Small_sit_close_hips.png` | Medium (read in code and seen in the stand-in drape, not in the game's own) |
| Boards, tents, fins, the shelf (2.3) | One bone per flap, so each flap is a rigid plate; no fold or bend down its length, and no contact between cloth and leg; the drape is a rule, not cloth (code: four flap bones in rigging.py; Drape) | Pictures in 2.3; sources S21 to S23: shipped bone skirts use several bones per chain and capsules on the legs | High that rigid plates cause the board look; medium for each pose |
| The sack on all fours (2.3) | Probably the upper band, weighted to the pelvis, keeping its outward flare in the pelvis's frame when the pelvis tips forward, with the flaps hanging below it, and no body under the coat to fill it | Picture: `pictures/Small_game_c02_f007676_knee_side47.png` | Low (hypothesis; not traced bone by bone) |
| The pointed tip, the hooding flap (2.3) | The flap is a plate with a ragged hem point at the front; nothing lets it rest on the thigh past square (record: verdicts.md, "Drape has no belly to stop the front flap") | Judges and pictures | Medium |
| Cloth under the ground (2.3) | Drape's ground is taken as the lowest foot less the ankle's height (code: MinerBody.cs:638, from the DetachedLeg investigation); lying with a foot raised, that guess is wrong | Measured (bar 7) | Medium |
| The getting-up leg movement (2.4) | The physical body that gets up has no joint limits at the hip and knee (record: Validation.md bar 3, CURRENT_STATE.md) | Measured: deepest fold 172.4 degrees, knees past 150; Small only | High that the folds are real and physical; that they are what Luis sees is a hypothesis |
| Thin thighs and forearms (2.5) | The preset numbers: limb radii arm 0.033 / 0.036 / 0.045 m and leg 0.041 / 0.046 / 0.058 m (code: workers.py presets, via internal.md 3.2) | Measured thigh widths match | High |
| Tube arms, pipe knees, column legs (2.6) | The limbs are skin chains with nearly constant radii (sleeve radii 1.2 to 0.8 of the arm radius, code: outfits.py:107) and no shaped elbow, kneecap or calf; only creases are displaced at the elbows (code: `bend_creases`) | Pictures in 2.6 | High for arms and knees; the legs' columns not re-checked |
| Flat side profiles (2.7) | The trunk depths in the presets, then the smoothing shrinks them; there is no body under the coat to give a belly or seat | Measured sections (audit section 2) | Medium |
| Small's hair hem, Long's profile (2.8) | `hair.py` and the head and face meshes | Pictures | Medium |
| Long's 11 degree stance (2.8) | Open since October (record: TheMiners.md:379-405) | Record | Not traced here |

**Two roots behind the three faults Luis named.**
- **The garment construction** makes the waist and the floating legs. The trunk is made only of garments, built as
  separate smoothed tubes, tucked at the waist and cut where it was thought hidden. There is no body under the coat
  (code: "There is no torso skin", internal.md 3.1). The garment code then throws away what the presets already say:
  Round's preset is a barrel; her smock makes her a corset (measured).
- **The rigid flaps and the drape rule** make the clipping and the boards: one bone per flap, weights at the hip that
  stay with the pelvis, a rule that ignores a sideways thigh, and no contact with the legs.
- The two meet in one place: with no body under the coat, the cloth has nothing to lie on.
- **Long is the exception:** his waist is his own thin preset.

So most of the look's fix is to make the clothes follow the bodies the presets already describe and give the legs
somewhere to join. The clipping needs a separate fix in the rig and the drape.

---

## 4. What the aesthetic vision asks of a pretty, natural miner

### 4.1 What the two references for people show (picture, opened for this revision)

`Docs/ArtDirection/References/E_ExpressiveDrawnLife.jpg` (two figures at a cottage door at dusk):
- **The round woman has no waist.** Her green coat hangs straight from the shoulders over a barrel body, wider at the
  belly than the chest. Her apron hangs flat from the chest to below the knees, over the belly, not into it. This is
  almost exactly "Round" as she should read: green coat, apron, round and heavy (picture).
- **The long man has no pinched waist either.** His long coat hangs straight and open; the body under it is a slim,
  straight column. His legs show below the coat as whole, dark, joined legs down to the boots (picture).
- **Garments are loose and lived in:** big soft folds at the elbows and the hem, ragged and patched hems, a scarf in a
  warm accent, rolled sleeves. The cloth hangs by gravity and its own weight, never stiff (picture).
- **Faces carry the charm:** big noses, a closed-eye smile, few marks (picture). The miners' loved faces already match.

`Docs/ArtDirection/References/B_AFaceInWindowLight.jpg` (a young woman at a window):
- An oversized dark coat, much bigger than the body, with deep folds, a stitched patch, turned-back cuffs showing an
  under-layer, and a soft scarf. The body reads under the cloth by its bulk and the folds, not by a narrowed waist
  (picture).
- The prettiness is in the face, the hair, the light and the cloth's softness, not in body shape (picture).

What this gives the redraw (hypothesis drawn from two pictures): "pretty" here means soft, loose, folded, worn cloth
hanging from the shoulders and chest over honest bodies, with the charm in faces, hands, hair and light. A waisted
coat appears nowhere. Four other references (A, C, D, F) are about places and light and were not opened. Luis's taste
notes (auto-memory: an intimate lit house at dusk over bright vistas) agree with these two.

### 4.2 What the game's guides say

- **One clear idea per body, honestly exaggerated:** "round and heavy, long and thin, small and quick" (record:
  VisualSoul.md:180-182). The idea is in the whole body, not in a waist.
- **Individual bodies:** "Different body shapes, noses, shoulders and relaxed gestures create personality" (record:
  VisualSoul.md:129-130).
- **Small, simple shapes against a big world,** readable from the Strategy camera by silhouette and colour (record:
  VisualSoul.md:167-168; ModelQualityMethod.md:51).
- **Drawn, not only modelled:** lively, slightly imperfect contours and confident flat shapes; confident dark shapes,
  such as hair and coats, against lighter ones (record: VisualSoul.md:183-188). People are drawn and the world is
  painted (record: TheEssence.md, device 1).
- **No 3D giveaways:** no faceted silhouettes, plastic highlights or flat untextured areas (record: TheEssence.md, the
  frame test).
- **Feeling from very few marks** in the face (record: VisualSoul.md:185-186). Luis loved every face (record:
  TheMiners.md).
- **Posture and small gestures** carry personality (record: VisualSoul.md:189-190).
- **Worn surfaces** that hint at a life: wear, patches, personal objects (record: VisualSoul.md:191-193).
- **Made as a maker makes it; nothing floats; nothing passes through; objects obey gravity** (record:
  ModelQualityMethod.md:34-49). Its defect catalogue names "a knee through a skirt" as clipping and "skin or void
  between ... boot and trousers" as a gap. Its junction map asks for "Skirt over the trousers (covered, in every
  stride)" (record: ModelQualityMethod.md, Appendix A).
- **The soul stays through polish:** "Polish never makes a being generic. The stylization and appeal Luis loved stay"
  (record: ModelQualityMethod.md:53-54).
- **Appearance carries the mechanics:** appearance "should not be an interchangeable skin laid over a spreadsheet"
  (record: DESIGN_RATIONALE.md:94); "all the visual aspects ... really tie to the capabilities" (record:
  GAME_VISION.md, 2026-11-03).
- **The register:** "a little stylized as long as it still reads natural visually" (record: Luis, November 13). He said
  it of movement. It may fairly be read for bodies too, but he did not say it of bodies (hypothesis that it carries
  over).

Nothing in the guides asks for a thin waist (record: internal.md 1.2). Final proportions, the cast and the costumes
are Open (record: GAME_VISION.md:286).

### 4.3 What the outside sources add

- **Keep the structure, exaggerate by purpose.** Good stylisation keeps the body's two masses, the ribcage and the
  pelvis, with the waist as the gap between them (source S6, snippet only). Exaggeration works when it stays true to
  the real form and is pushed with restraint and balance (source S7).
- **Natural ranges.** The drawing canon makes the ribcage and the waist the same width, the waist about 0.67 of the
  shoulders and about 0.89 of a man's hips (source S3; a male canon; the author warns it is for proportion, not
  rulers). European art runs a waist-to-hip girth ratio of 0.67 to 0.80 for women; the most waist-marking art averages
  about 0.5 (source S1, S2). These are girths from art, not measured bodies. Source S2's 0.85 and 0.90 are health
  thresholds, not a guide for a heavy character, so they are not used here.
- **Women and men differ mainly across the hips and shoulders.** A woman's waist reads narrow mostly by contrast with
  the hips, not by being small (source S4, S5).
- **Simple contours hold up in action** (source S10, snippet only). Pixar kept the major shapes and design lines,
  spent detail where people look (the face and hands), and placed joint loops where anatomy puts them (source S9).
- **Body and motion must promise the same realism.** A body that looks more real than it moves, or the reverse, reads
  wrong (source S8). Our bodies move by real physics, so hard-edged cloth boards over a physical body are that
  mismatch (inference in external.md).
- **A coat's waist belongs to its cut.** A frock coat is narrower at the waist by a waist seam and side panels, and its
  skirt flares out from that seam (source S15). Our miners narrow the top by a tuck and then fit the skirt to the
  narrowed top. Real tailoring flares from the waist, which gives back the hips. The references (4.1) go further: their
  coats and smocks hang straight.
- **Legs under loose cloth.** The safest answer in games is whole legwear under loose cloth (source S16, S27). Cutting
  away what is hidden only works where it can never be seen (source S25, S26). Another common answer is a simple base
  body under the garments (source S26). Skirts that follow legs use several bones per chain, neighbours tied together,
  "sync bones" that copy the thigh's motion onto the skirt, capsules on thigh and shin, softer toward the hem, and
  weights spread over neighbouring bones (source S21, S22, S23). Even then some faults stay, such as the inside showing
  when sitting (source S23). No source gives a cost in work time.
- **Joints need structure to bend well:** some loops and volume at elbow and knee; plain linear skinning loses volume
  and collapses at large turns (source S42, S43, S44; low authority except S43).

### 4.4 Where they meet

1. **The stylisation lives in the one clear idea per body and in the faces, not in a pinched waist.** Small is small
   and quick (a big head, short limbs). Long is long and thin (height, a long coat, a long nose). Round is round and
   heavy (a barrel trunk, as in reference E). A natural waist takes none of these away; it gives Round hers back.
2. **"Honest" exaggeration means true structure, pushed.** The ribcage, the waist and the pelvis stay; their sizes
   can be pushed for each body.
3. **The waist is a fact of the garment.** A coat may have a gentle waist from its cut, or hang straight; a smock hangs
   from the chest. The garment hangs over the body by its own looseness.
4. **The legs join a body.** Whole trousers to a seat under every coat, as on Long, or a simple body under the coat.
5. **Cloth answers to the body and to gravity.** It lies over a raised thigh, falls away from it, rests on what is
   under it, and never stands as a board. This is the physical-objects rule applied to cloth.
6. **Cloth looks worn and soft:** folds, patches, painted texture; no flat grey areas.
7. **Limbs show their strength,** but only where the physics agrees (one body; appearance carries the mechanics).
8. **Three different silhouettes** from the Strategy camera: compact, long and straight, barrel.
9. **The soul stays:** faces, colours, heights, props and gestures kept.

**The main target for the waist** (hypothesis, to be shown as pictures): **each miner's own preset waist, plus the
coat's looseness.** That is the one target with a record behind it, and the presets are already natural (waist about
0.82 to 1.06 of the chest, 2.1). Other bands (a little more waist, a little less) are offered only as stills to choose
from, not as numbers. The earlier draft's band table was dropped: its waist-to-chest and waist-to-hip bands did not
agree with each other (section 10, point 17).

---

## 5. How far to go: the ladder of options

Luis's question frames three depths: the visual model, the rig, and the body's proportions. The ladder below splits
the first rung so that each depth is clear.

**A fact that decides much of this** (record: Docs/Design/AnyBody/body_map.md, via internal.md 4): "the model's mesh
never reaches the physics". The physical body is built from the proportions (23 numbers), the masses per bone and the
limb radii. The drawn mesh is only turned by the bones. There are three small leaks (code: `workers.shape`,
`weights.py`, `outfits.skirt_slack`):
- `bodyFront`, `faceFront` and `headHalf` are measured on the clothed model. They reach the hip capsule, the tool's
  clearance and the head sphere.
- Worn mass (the cloth) is added to the weights.
- `skirtSlack` is measured on the skirt and used by the drape.

**Which physics numbers stay, which follow the cloth** (for A1 and A2):

| Number | Rule | Why |
|---|---|---|
| Trunk presets (widths, depths, levels), limb radii, bone positions, heights | **Stay exactly** | They are the body; a change is rung B |
| `headHalf`, `faceFront` | **Stay exactly** | The head and face are not changed |
| Worn mass | **Follows the new cloth** (re-measured, the change reported) | Luis's "true weights": whole trousers and a seat are real cloth. Expected small (hypothesis; not computed) |
| `skirtSlack` | **Follows the new skirt** | The drape reads it |
| Prop stops (`stopNormal`, `stopDistance`, the bag's push) | **Re-measured** | They are measured on the model |
| `bodyFront` | **A choice, made on purpose** | Measured on the clothed model; it reaches the hip capsule (code: OwnBody.cs:696 clamps it) and the tool's clearance. Frozen: the physics stays exactly as today. Re-measured: the physics follows a looser coat (likely small on Small and Long; larger on Round, whose coat would widen most; hypothesis). Both should be computed and shown before choosing |

The Stage 0 test is then: "these numbers equal the first version's; those change, by amounts that are reported".

### 5.1 Rung A1: static art only

**What it is.** A second set of the three miners (for example `Miner_<Name>_v2.fbx`, with its own atlas, json entry
and prefab), built from the same presets with the garment code changed. **No new bones, no game code change.**

**What it fixes:**
- **The waist.** A top built with its own waist ring at the coat's looseness (no 0.82 tuck at the end of a chain); a
  coat hanging straight or with a gentle waist from its cut; the skirt flaring from there instead of fitted into a
  notch. Round's smock hangs from her chest over her barrel; her apron hangs from the bib straight down. On Long, only
  the looseness comes back; his waist stays his preset's.
- **The floating legs**, one of two ways (to be weighed with stills):
  - **Whole trousers to a seat**, as on Long. Cheaper. It leaves nothing under a lifted coat at the belly or the back.
  - **A simple body under the coat** (a trunk and a seat as an under-layer, painted as a shirt and trousers), with the
    garments over it (source S26). More work. It also fills the coat on all fours, which may cure the sack, and gives
    the cloth something to lie on.
- **Part of the clipping, by weights only.** Re-weight the coat's upper band near each leg to the thigh, on today's
  four flap bones. It might fix the high piercing (S3, R4) cheaply. It can be tried first on a copy.
- **The cloth painted:** the new trousers or under-layer painted as the coats are, with wear.

**What it leaves:** the side-seam clipping, the boards, tents and fins (they need A2); thin limbs; flat profiles where
they come from the trunk depths; the head proportions (Open); Long's waist; the getting-up folds; Long's stance.

**What it risks:**
- **The seat brings a new place to look at: the hip's bend.** Joined legs show the hip's skinning at deep folds, and
  the get-up folds the hip to 172 degrees (measured). Linear skinning collapses at large turns (source S42). The seat
  must be swept through the get-up's real angles.
- **Whole trousers under a closed coat can poke through the coat.** That is why the cut-away practice was adopted
  (record: TheMiners.md:735-736). The trousers and the coat's upper band must change together (hypothesis, strongly
  suggested by the record and by `pictures/Small_kneeup_close_hips.png`).
- **The shared Blender modules can change the first version.** If `outfits.top`, `trousers` or the rigging rules change,
  any later re-export of the first version changes too. The exporter also writes `miners.json` for every miner
  (internal.md 3.5), so a second version's export could overwrite the first's entry. Guards: every new behaviour behind
  an explicit flag, off by default; a golden check that regenerates the first version and compares it with the shipped
  fbx and json; the second version's export writes only its own entries. The editor builder (`MinerSetup.cs`) is in
  the file list too.
- **GUIDs and the switch.** New files get new GUIDs; the first version's files are untouched. A version switch is
  needed: MinerChoice and MinerPanel choose by name today, and the ways file (`Resources/RiseWays/Small.txt`), the ONNX
  models and the tests are keyed by the name "Small" (record: internal.md 4.4). The switch must carry the name through.
- **LODs.** Each version needs LOD0 to LOD2 within about 16,000, 5,000 and 1,600 triangles. A seat or an under-layer
  costs triangles. Today's headroom was not measured; LOD1 and LOD2 were never audited.
- **The painted atlas** is redone for the new trunk and trousers. The painted look is an aesthetic rule, so this is real
  work, not a detail.

### 5.2 Rung A2: the skirt mechanism (rig and drape code)

**What it is.** A1, plus a rig that lets the cloth follow the legs, and the drape code that drives it. This is the
"rigging" part of Luis's question.

**What it adds:**
- **The mechanism, chosen from the sources** (source S21, S22, S23): flap bones that follow the thigh ("sync" or parent
  bones), a second bone down each flap so it can bend (or more flaps), weights spread at the seams, a softer hem, and
  capsules on thigh and shin that keep the cloth outside the leg. Which of these, and how many bones, is not chosen
  yet; no source gives a cost.
- **The flaps following a thigh that opens sideways** (today's rule ignores it on purpose, so a long coat's front does
  not stand out as a board; the new rule must keep that).
- **The coat on all fours hanging** instead of ballooning, if A1's under-layer does not already cure it.

**What it fixes:** the side-seam clipping, the boards, tents, fins and the shelf; most of the remaining clipping.

**What it risks:**
- It changes the rig, the prefab, `MinerBody.Drape` (game code, on the work branch; record: internal.md 6.7),
  `skirtSlack`, the prop stops and `FlapSeamTests`.
- **It must sit behind a version gate in the game code,** so the first version's drape stays as it is.
- More bones per vertex cost more (source S29, snippet).

### 5.3 Rung B: proportions that reach the physics

**What it is.** A1 and A2, plus changes to numbers the physics reads: thicker thighs on all three, thicker forearms on
Round, deeper chests, bellies and seats where the trunk is too flat, a fuller waist on Long if wanted, perhaps a
different head share on Small, perhaps Long's stance.

**What it fixes beyond A:** the stick legs and the slim strong arms, so strength shows in the body (record:
GAME_VISION.md, 2026-11-03); the flat profiles; a truer "round and heavy" for Round; Long's waist.

**What it leaves:** the getting-up folds (still the body's limits); anything the creator needs that a retune of three
bodies cannot give.

**What it risks:**
- **Strength.** Today the three miners keep the strengths they had before the muscle rule (code:
  `PhysicalBody.KeepsWhatItHad`, called by MinerSetup.cs:512 and :534). If the redrawn miners take the muscle rule,
  thicker limbs make them stronger and a heavier trunk makes them relatively weaker (source S38: strength by
  cross-section, weight by volume). Either way it changes how they play, and Luis has to choose it.
- **Masses and moments** change with every radius and trunk number (code: weights.py, flesh at 1000 kg/m3). Hodgins and
  Pollard first rescale the controls by size, mass and inertia, then tune (source S37, snippet only).
- **Colliders.** The thigh rod is the leg radius times 1.25, the shin rod the leg radius, the arms the arm radius, the
  head sphere `headHalf` (code: OwnBody.cs:696-736). A head share change on Small moves her centre of mass and her
  rise.
- **The getting up is tuned on today's bodies.** Small's ways to get up (`Small.txt`), OwnRiser's settings and the ONNX
  models were searched on Small; the stance atlas was fitted per body (record: internal.md 4.1). Changing Small's
  proportions means checking or searching all of these again. Some shapes cannot be walked even by a controller trained
  across a range (source S36).
- **The getting-up numbers cannot judge it yet.** A course does not run again the same, and the own get-up blew up once
  in eight courses with the test still passing (2.4). A before-and-after of a proportion change would be lost in that
  noise. Rung B's getting-up checks need the energy count and the blow-up understood first, then paired runs of many
  courses.
- **The tests and the idle.** The PlayMode suite, the whole work-tire-rest cycle on all three (Long fails first),
  several strengths, and the idle Luis loves at 14 and 4 must all be read again (record: auto-memory, Handoff.md).
- **One body for everything.** A change of proportion goes into the same body that stands, steps, falls and gets up
  (record: Luis, October 14). There is no separate drawn proportion.

**How it serves the creator:** it proves, on three bodies, that a change of proportion passes through the whole chain
(Blender, json, prefab, physics, getting up). Every creator choice will do the same.

### 5.4 Rung C: one parametric body with uncut garments, as the creator's foundation

**What it is.** One body whose measurements are parameters within bounds, with garments that fit any body in the
bounds and are never cut. The three miners are three points in that space.

**What it fixes beyond B:** the creator's foundation. A creator cannot know which skirt will hide what, so cut-away
cannot be used safely (record: internal.md 5). Every module fits any body (record: TheMiners.md:836-863).

**What it leaves:** the getting up across the creator's range is not a model matter. It needs the any-body getting up
tuned over many bodies (record: GAME_VISION.md, 2026-11-03).

**What it risks:**
- **How a creator body is made while the game runs is not known.** Scaling an existing model's bones breaks, because
  Unity shears non-uniform bone scale (record: body_map.md, Way A). Building in Blender per body takes minutes (Way B),
  so it is not a live creator. A made body from primitives (Way D) is cheap but is not the drawn miner. The usual
  answers in games (shape blends on one shared mesh, bones moved rather than scaled) were not researched. **This is a
  named gap that must be researched before anyone relies on it.**
- **A range to verify.** Every choice must map to a range where standing, stepping, falling and getting up have been
  verified (source S36). Spore tested a large random sample of creatures against every action and about 90 percent
  passed, but Spore was not physical (source S35).
- **Scope.** This is the S2 creator's work. It should not be rushed into the redraw.

**Does A lead into C?** Only if creator bodies are built from the same Blender garment modules. If the creator uses
shape blends or a different route, A's fixes in the Blender modules may not carry over (hypothesis either way, until
the gap above is researched). A is worth doing for the three miners in its own right, whatever route the creator takes.

### 5.5 The rungs side by side

| | A1: static art | A2: plus the skirt mechanism | B: plus proportions | C: one parametric body |
|---|---|---|---|---|
| The waist (Luis) | fixed on Small and Round; Long gets his looseness back | the same | fixed on all three | fixed |
| The floating legs (Luis) | fixed | fixed | fixed | fixed for every body |
| The leg through the skirt (Luis) | the high piercing perhaps (weights) | mostly fixed | mostly fixed | mostly fixed |
| Boards, tents, fins | no | mostly fixed | mostly fixed | mostly fixed |
| The sack on all fours | perhaps, with a body under the coat | likely fixed | the same | the same |
| The getting-up leg folds (Luis) | the model's part removed; the folds stay | the same | the same | the same |
| Thin limbs; strength shown | no | no | fixed | fixed |
| Flat profiles | partly, by the cloth or under-layer | the same | fixed | fixed |
| Rig changed | weights only | yes | yes | yes |
| Game code changed | no | yes, behind a version gate | yes | yes |
| Physics touched | worn mass, `skirtSlack`, prop stops; `bodyFront` by choice | the same | yes | yes |
| The first version and its GUIDs | kept, with guards on the shared scripts | kept; first drape untouched | kept | kept |
| Rough size (hypothesis, agent sessions) | about 4 to 6 | about 4 to 6 more | large, about 8 or more, after the blow-up work | the S2 creator |
| Luis's choices needed first | the look, from stills | none beyond A1 | the strength rule; the proportions (Open) | the creator's options (Open) |

The session counts are rough guesses from the lists above (modelling, painting, LODs, the switch, tests, judges,
game-side checks for all three). They are given because Luis watches usage, not as a promise. Every stage also adds a
judges' round and game-side checks for Long and Round, which have never been run.

---

## 6. The creator

### 6.1 What is decided and what is open

- Decided or directed (record: GAME_VISION.md): a worker creator replaces the faction creator in this prototype; speed
  is not a creator option; strength from muscle, slower with size; the creator warns of a body too weak to get up
  but allows it; customization of everything from modules that fit any body; a limited early interface must not become
  a ceiling on customization (record: GAME_VISION.md:443); the three miners as ready-made suggestions; "not too many,
  but some" options (Luis, November 14).
- Planned (record: ShowcaseRoadmap.md:179-188, S2): a scene with a 3D preview; body (height, build, strength), face and
  hair, clothing and colours, tool, hauling gear, name; optional presets and randomize; a panel saying what the
  choices do to work; versioned saves.
- Open: how many options, of what kind, and the final proportions and costumes.
- Nothing of the creator is built. `Scripts/Creator` holds only the faction files (code, via internal.md 5).

### 6.2 What the sources say about few, meaningful options

- Curated parts beat a long list. The Mii team cut parts to keep from overwhelming people, and chose caricature over
  realism (source S30). A few parts with adjustable size gave more variety than more parts (source S31, snippet).
- Presets for what is hard to make pretty. Baldur's Gate 3 uses face presets and four body types, with no sliders for
  height or body shape, so that every face stays good and distinct; the cost was that some players found it limiting
  (source S32, S33). Baldur's Gate 3 is not a simulated body.
- The failure modes are too many options, ugly results, and choices that do not matter (source S34, weak).
- A creator that changes a simulated body needed its controller trained across the allowed range, and failed outside
  it (source S36).

### 6.3 What this suggests (hypothesis, for Luis to choose)

- **Body: the three miners as starting types, with bounded sliders or steps,** each inside a range where the physics
  has been verified: height, build (slim to heavy) and strength, as the roadmap plans. Bounded, not "never sliders":
  the bounds come from what the getting up can bear, and the guardrail says early limits must not become a ceiling.
- **Face: the loved faces as presets,** each with a skin tone. Faces are where "millimetre changes can make or break" a
  look (source S33). Whether faces get finer choices later is Open.
- **Hair:** the three styles and a colour.
- **Clothes:** the three garments (coat, long coat, smock), each in a few colours, each fitting any body in the range.
- **What is carried:** lantern, mug or hammer; held objects were allowed "for now" (record: October 6), which Luis may
  want to revisit.
- **Name.**

That is six or seven categories, each with a few choices: one reading of "some, not too many". The count is Luis's.

### 6.4 What "any body" means for the creator, and what the physics can bear

- **Any body gets up by its own capacity,** procedurally, with no teaching per body (record: GAME_VISION.md,
  2026-10-30 and 2026-11-03). The creator's range is bounded by what that getting up has been verified on, not by what
  looks good alone.
- **Today that range is narrow.** Only Small has her own ways to get up; Long and Round use the old way (record:
  internal.md 4.1; Validation.md, `cc_c3`). How far the any-body layers 2 and 3 are built was not re-read here. And the
  own get-up has an open blow-up (2.4). A creator body today would be a body no getting up was verified on.
- **A body too weak to rise is allowed, with a warning** (record: GAME_VISION.md, 2026-11-03). So the bounds are about
  the body behaving physically right (no blow-ups, no broken poses), not about every body succeeding.
- **Strength from muscle** ties the build choice and the strength choice: a heavy body on thin limbs is weaker for its
  weight (record: GAME_VISION.md, 2026-11-03; source S38).
- **The garments must fit every body in the range without cuts** (rung C). The redraw's garment fixes should be made
  that way from the start, so long as that does not slow the three miners' fix.
- **Testing:** as Spore did, a large sample of creator results through every action (source S35); as Won and Lee did,
  a defined range (source S36).

---

## 7. A recommended path, for the proposal

### 7.1 The shape of it

**Show the looks first, as stills; build only what Luis chooses.** Then A1, A2, and B only with his word. Leave C to
the S2 creator.

Why this order:
- Luis asked for "a big transformation" and to see things early. Stills of a fuller look (rung B's look) cost little
  and touch nothing; they let him see the bigger change before anyone pays for retuning the physics.
- The register says the proposal comes before anything is built. Stills are the proposal's pictures, not the build.
- A1 fixes two of his three named faults (the waist, the floating legs) and the start of the third without any game
  code. A2 fixes the clipping and needs game code behind a gate.
- B changes how the miners play, and its getting-up checks cannot be read until the blow-up and the run-to-run noise
  are handled (2.4). It is cheaper to decide once the look is chosen.
- C needs the creator-body method researched and the any-body getting up verified over a range.

The earlier draft said "start with rung A". The critic was right that this was argued from the physics risk and read
as if the evidence required it. The evidence equally supports showing the bigger look first. The stills step answers
both.

### 7.2 The stages

Every stage is additive: a second version behind a switch, the first version kept and chosen by default until Luis
says otherwise. Every look goes to the aesthetic judges first, then to Luis. Every report to Luis names the switch to
turn on in its first line (record: auto-memory, "Say what must be switched on").

**Gate before any look is made: the references.** The judges and the modeller work with the art references in front
of them: B and E (4.1), the dusk images and the miners' portraits, and today's three as the control row.

**Stage 0: stills for the proposal (nothing in the game changes).**
- In a copy, with new behaviour behind a flag that is off by default: Blender stills of each miner in three versions
  side by side: today's; the "look only" version (the preset waist plus looseness, whole trousers or a body under the
  coat, the apron from the bib); and the "fuller body" version (thicker thighs, a belly, a seat, Round's forearms).
  Rendered at rest in the idle's arm pose, in the audit's poses, and from the game camera.
- The judges' brief for a model at rest and in poses (none exists; record: internal.md 1.6), based on
  `Art/Review/judges/aesthetic_flaps.md`, ModelQualityMethod.md and VisualSoul.md. Lenses: a costume maker, a figure
  and anatomy artist (for "natural"), the game's art director, and a player at the game's distance; a referee; blind,
  with the versions' order changing from sheet to sheet.
- The audit's tools made repeatable on any version (render_miner.py, measure.py, silwidth.py, sheet.py), with
  measure.py's known slip on the shoulder height fixed (audit.md section 1).
- Then Luis chooses from the pictures: the look, and how deep to go.

**Stage 1 (A1): static art, once a look is chosen.**
- The waist, the legs (whole trousers or a body under the coat), Round's apron, the new cloth painted.
- The coat's upper band re-weighted to the thighs on today's four bones.
- The version switch: new files beside the first (new names, new GUIDs, first files untouched), a choice on the miner
  panel, the names carried through for the ways file, the ONNX models and the tests.
- The guards: the golden check on the first version; the second version's export writing only its own entries; the
  physics-number test from section 5's table.
- Kept unchanged: the faces, the colours and painting style, the heights and every proportion the physics reads, the
  props and their stops (measured again, not redesigned), the boots, the hair, the gestures, the hands and grips.

**Stage 2 (A2): the skirt mechanism.**
- The flap bones that follow the thigh and bend, capsules on the legs, spread weights, the sideways thigh, all behind a
  version gate in the game code.
- Then joints and small things: elbow points, forearm swell, wrist taper, kneecaps, calves, at the same mean radius;
  Small's hair hem. The arm skin chains carry the grips and hand bones, so the hands and wrists must stay exactly where
  they are.
- LOD1 and LOD2 for both versions.

**Stage 3 (B): proportions that reach the physics, only with Luis's word,** after the own get-up's energy count and
blow-up are understood. Thicker limbs, deeper trunks, whatever he chooses on heads and Long's stance, his choice on the
strength rule, and the getting up checked or searched again.

**Stage 4 (C): the creator's foundation, with S2,** after the creator-body method is researched.

**Separate, already decided, not part of the redraw:** the raised leg's front flap resting and the hem's point
rounded, on the flaps-on drape (now the default) for today's models. Luis decided the change; which version changes was
left to judgment and recorded as the switch-on version only (record: GAME_VISION.md, 2026-11-14). It goes on in its own
task and its own judges' round. The second version inherits it.

### 7.3 What is kept unchanged throughout

- The first version, its files and GUIDs, chosen by default until Luis says otherwise; guarded against re-exports.
- The faces, the colour identities, the heights, the props hanging and held, the laced boots, the hair ideas, the
  portrait gestures (record: audit.md section 8).
- The idle standing Luis loves, at 14 and 4 (record: GAME_VISION.md, 2026-11-13 night).
- The flaps-on drape as the first version's default, with its already-decided rest and rounded point.
- Every Open item in the register stays Open; nothing in this path promotes a proportion, a costume or the creator's
  shape to Locked.

### 7.4 Questions only Luis can answer

1. **How far.** After seeing the stills of the "look only" and the "fuller body" versions, which depth does he want:
   static art only, the skirt mechanism too, or the proportions too? (Open; his.)
2. **The waist.** Each miner's own body waist plus the coat's looseness is the suggested start. Does he want it nearer
   natural, or a little stylised? Shown as stills side by side, so he picks from pictures.
3. **Trousers or a body under the coat.** Whole trousers to a seat (cheaper), or a simple painted body under the coat
   (fills the coat, more work)? Also shown as stills.
4. **The faces.** Kept exactly, as "change only what is asked" says, or part of "much more pretty"? Long's profile (the
   head jutting, the nose a spike from the side) is the one face fault the audit found.
5. **Strength in the body.** Should the limbs show strength (thicker thighs, Round's forearms)? If so, should the
   redrawn miners take the muscle rule instead of keeping their old strengths? Both change how they play.
6. **Head proportions.** Asked on October 2 ("more stylized (bigger heads) or more grounded?") and never answered. Keep
   today's heads, or change them? A change reaches the physics. (Open.)
7. **The cut-away practice.** May CharacterPractices.md be amended so that nothing is cut away under a garment that can
   open? Amending a guide needs his word.
8. **Held objects**, allowed "for now" on October 6: do they stay with the redraw?
9. **Long's stance**, 11 degrees off his build in the game (P1, open since October): part of the redraw or not?
10. **Which comes first:** the redraw, or the getting-up work behind the leg movement he sees? (His priority call.)
11. **The creator.** Does the shape in section 6.3 (the three as starting types with bounded choices; face presets;
    three garments; colours; what is carried) match "some, not too many"? (Open.)
12. **Which version is the default** once the second is made: chosen only by him, after playing both.

---

## 8. How each stage would be checked

### 8.1 Stage 0 (stills)

- The first version's files are untouched (the same content and GUIDs); the flag is off by default.
- The stills from the same views and poses for all three versions, in the idle's arm pose as well as the bind pose.
- Measured at rest on each version: visible waist width; waist girth against hip girth and against the preset waist;
  thigh width; the trousers' top against the hip joint.
- The judges' verdicts and the referee's, then Luis.

### 8.2 Stages 1 and 2 (A1, A2)

- **The physics numbers test:** the "stay" numbers in section 5's table equal the first version's; the "follow" numbers
  are reported with their change; `bodyFront` as Luis or the proposal chose.
- **The golden check:** the first version regenerated equals the shipped fbx and json.
- **The switch** flips between versions in the game; the names reach the ways file, the ONNX models and the tests.
- **At rest, in Blender:** the audit's renders of both versions, side by side, from the same five views, the game
  camera and the close views (head, waist, hands), with the silhouettes front and side.
- **In poses, in Blender:** the six poses of the audit, from three-quarter, side, close at the hips and close from the
  right. Added: the hip folded to the get-up's real extremes (to 172 degrees), arms raised, a hammer swung, a reach.
  Counted on every pose: points of a leg or the trousers outside the skirt where they should be inside it, and points
  of the skirt inside a leg.
- **The game's own frames, for all three miners** (not only Small): falls and get-ups as played (`-ownAsPlayed 1`),
  from the four ways (side47, headright32, feetleft32, above), including the hands-and-knees stage.
- **The cut ring as played** (`CoverRingLiveTests.TheCutRingAsPlayed`). With whole trousers there is no ring, so the
  bar changes to "a trouser or leg end seen with nothing above it". Expected: none, for all three, lying, getting up and
  standing. Also counted for the player's own camera, not only for any of the capture ways.
- **The drape:** `FlapSeamTests` as played (standing and walking hang within a small angle of before), the wrong-face
  flap count (bar 2), and the drape under the ground (bar 7).
- **The props:** the lantern, mug, satchel and hammer hang, swing and are stopped by the body in the game frames, with
  their stops measured again.
- **The grips:** the hand checks (`--hands`) pass; no finger in a handle.
- **LODs:** each level at the distance it is used, the triangle counts within budget, and the Strategy camera's view of
  all three together (they still read apart by silhouette and colour).
- **The getting-up bars are not a gate for A1 or A2.** The physics is not changed beyond the cloth's mass, and the
  bars are too noisy run to run to judge a model change (2.4). The getting up is watched only for new faults the model
  brings (a seat clipping at deep folds).
- **The aesthetic judges, blind,** on sheets with the two versions in rows whose order changes, today's three as the
  control row, the references beside them; then the referee; then Luis in the build, told which switch to turn on.

### 8.3 Stage 3 (B)

All of 8.2, plus, after the own get-up's energy count and blow-up are understood:
- the PlayMode suite, normal and as played;
- `OwnBodyTests`; the three miners through a whole work-tire-rest cycle, Long first; several strengths;
- the idle at 14 and 4, compared with the first version;
- standing, stepping, falling and getting up on all three, counted in courses, paired runs of many courses for each
  version, each change measured alone;
- the getting-up bars of November 14 (hip, knee, spin, jump, blow-ups) on those paired runs, with failing log messages
  no longer ignored;
- the masses and the centre of mass read back from the game against weights.py.

### 8.4 Stage 4 (C)

A sample of bodies across the creator's range, through every action, with the failures sorted (as Spore did, source
S35), and the aesthetic judges on a spread of the results.

### 8.5 What no check can give

Passing measurements and tests do not mean the look is accepted. Only Luis accepts a look (record: AGENTS.md;
DESIGN_RATIONALE.md:269).

---

## 9. What this research did not do

- **No model was changed, built or exported.** Blender and Unity were not run for this file. Nothing was committed.
- **No look was made or judged.** The aesthetic judges have not seen anything from this research. Luis has seen
  nothing new.
- **The getting-up leg movement was not judged by eye,** and no picture of the deepest fold was read. Its numbers are
  from the game's run of November 14 (Validation.md), Small only.
- **Long and Round were not seen in the game's own frames.** Long was not run in the game at all.
- **The cloth in the posed pictures is the audit's static stand-in for the drape,** not the game's own.
- **LOD1 and LOD2 were not audited.** Shoulders and elbows in strong arm poses, and the idle's arm pose, were not
  rendered.
- **The waist's cause is read in code and matched by measurement, but not tested.** No top was rebuilt.
- **The sack on all fours is a hypothesis** in its cause, and how often it shows was not counted.
- **The props' overlaps** were not checked in the game's frames.
- **The faults the critic added** (legs as columns, slab soles, flat trousers, necks) were read by the critic in the
  pictures, not re-checked in this revision.
- **Two of the six art references (B and E) were opened;** A, C, D and F (places and light) were not. The miners'
  portraits and dusk images were not re-opened.
- **The natural proportion ranges** are from drawing canons and art, not from an anthropometric table.
- **How a creator body is made while the game runs** was not researched. A named gap, before rung C.
- **The state of the any-body getting up** (how far layers 2 and 3 are built) was not re-read.
- **The session counts in 5.5 are guesses,** not estimates from a plan.
- **No web search was made.** Every outside claim comes from `external.md`.
- **The code line numbers on the work branch** (MinerBody.cs, OwnBody.cs, rigging.py) were not re-checked.

---

## 10. The critic's points and what was done

One line per point of `critique.md`.

1. **Rung A is a rig and code change** (high): agreed; A split into A1 (static art, weights only) and A2 (bones and
   drape code behind a version gate); the flap rest moved out as its own decided task on today's flaps-on drape, which
   the register already allows, so it does not break "first version kept" for the model.
2. **Physics numbers cannot all be frozen** (high): agreed; a table in section 5 says which numbers stay, which follow
   the cloth (worn mass for "true weights", `skirtSlack`, prop stops) and that `bodyFront` is a choice to show both
   ways; added that the physics already uses the wider preset body, so the redraw brings look and physics closer.
3. **Getting-up checks cannot be compared; the blow-up unmentioned** (high): agreed; the blow-up and the run-to-run
   noise added to 2.4 and 5.3; the getting-up bars are no gate for A1 or A2; B's checks wait for the energy count.
4. **The recommendation sits oddly with Luis's words** (medium): agreed; both quotes added; the path now starts with
   stills of the "look only" and "fuller body" versions so he chooses the depth from pictures.
5. **Shared Blender modules can change the first version** (high): agreed; flags off by default, a golden check, the
   second export writing only its own entries, and `MinerSetup.cs` named.
6. **"The first step of rung C" is unsupported** (medium): agreed; marked as a hypothesis, the creator-body method
   named as a gap, and A justified for the three miners in its own right.
7. **The waist numbers mix measures** (medium): agreed; girth ratios computed (Small 0.35-0.38, Long 0.61, Round
   0.31) and used against S1 and S2, with the caution that the hip hull includes the skirt's flare; front-width ratios
   marked as unsourced; the canon corrected (waist equals ribcage; 0.67 is of the shoulders; a male canon); the health
   thresholds dropped; "corset" kept as an impression.
8. **"One root" is not true for Long nor for the clipping** (medium): agreed; now two roots, and Long's waist is his
   preset, which A leaves.
9. **The getting-up numbers misquoted** (medium): agreed; the deepest fold and the failing rows are now separate, Small
   only, Long without data, and "mainly the body" marked as a hypothesis until the deepest fold is seen.
10. **"Long is free of it" and the sack "often"** (medium): agreed on Long (no gap in stills, not run in the game).
    Disagreed in part on the sack: the audit shows it in three frames from three different courses (c00, c02, c03),
    not one; but how often it shows was not counted, so "often" became "likely common, not counted".
11. **Prop overlaps called artefacts** (medium): agreed; moved to "unverified, probable in part", all six pictures
    named, and a game check for all three asked.
12. **Faults nobody looked for** (medium): added (legs as columns, slab soles, flat grey trousers, necks, the bind
    pose) marked as read by the critic and to be looked at again; the idle's arm pose added to the stills.
13. **"Pretty" never checked against the references** (high): agreed; references B and E opened and described in 4.1
    (no waisted coat anywhere; Round's very look in E); the references made a gate before any look is made.
14. **Options missing from the ladder** (medium): agreed; a body under the coat weighed against whole trousers, a
    weights-only step in A1, the skirt mechanism named from the sources in A2 (with no cost known), and the look-first
    stills as Stage 0.
15. **The work's size understated** (medium): agreed; painting, LODs, the switch, guards, judges and Long's and Round's
    game checks listed, and rough session counts given, marked as guesses.
16. **"Never free sliders" borrows a rule** (medium): agreed; now "bounded sliders or steps from a verified range",
    with the guardrail on early limits cited and the count left to Luis.
17. **The target bands contradict themselves** (low): agreed; the table dropped; the main target is each preset's
    waist plus the coat's looseness, other bands only as stills.
18. **"Nearly every frame" overstates** (low): agreed; now "seen from at least one of the 25 capture views", the
    player's camera count noted as missing and added to the checks.
19. **Stage 0 builds before the proposal** (low): agreed; section 7 labelled "for the proposal"; Stage 0 is now stills
    in a copy, and the switch and tests moved to Stage 1.
20. **The judges' lens set is thin** (low): agreed; a figure and anatomy lens added, with the references and today's
    three as the control row.
21. **The forearm fault is a look fault, not a broken rule** (low): agreed; 2.5 now says so, until question 5 is
    answered.
