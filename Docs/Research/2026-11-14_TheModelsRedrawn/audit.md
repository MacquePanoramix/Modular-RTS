# The models redrawn: a visual audit of the three miners

November 14, 2026. This audit is part of the deep research before the proposal
(Docs/Correspondence/2026-11-14_THE_FLAPS_ON_AND_THE_MODELS_REDRAWN.md).

It was read-only:
- nothing in Assets/, Art/ or ProjectSettings was changed;
- nothing was committed;
- Unity was not started.

The models audited are the exported miners in Assets/_WonderGather/Art/Worker/Miners/Miner_{Small,Long,Round}.fbx
(dated October 6, and identical by md5 to the worktree's copies).

Status: done. These are findings only. Nothing here is a decision; the Game Director decides what changes.

Severity scale:
- **B**: breaks the look;
- **V**: clearly visible;
- **m**: minor.

"Where" names the pose, the view, and whether it shows at the game camera or only close.

## 1. How the pictures were made

- **Blender 4.5.2.** It ran in the background with factory settings. The FBX was imported read-only.
  - The atlas `Miner_<Name>_Atlas.png` had to be loaded by hand, because the FBX does not carry it.
  - The camera was orthographic.
  - The renderer was EEVEE with the Standard view transform.
  - All three miners had the same key, fill and rim lights and the same grey ground.
  - Only LOD0 was rendered.
- **Rest views.** The views were front, three-quarter front, side, three-quarter back and back. The angle is measured as az, the camera's turn around the miner: az 0 is the front and az 90 is the miner's left side.
  - A "game camera" view (high, at about 47 degrees, and small) was added.
  - Close views of the head, waist and hands were added.
- **Silhouettes.** Flat black silhouettes, front and side, were made by thresholding the renders.
  - Glow and eye whites leave white holes: Small's eyes and lantern, Round's lamp. This is an artefact; ignore it.
- **Poses.** Every pose was set only by turning bones. The poses were:
  - a walking stride;
  - a knee raised to about 90 degrees;
  - sitting on the ground;
  - kneeling on one knee;
  - lying on the back;
  - lying on the side.
- **The game's flaps, emulated.** The game moves the skirt flaps at runtime (MinerBody.Drape). The audit stands in for that statically:
  - each flap hangs by gravity;
  - each flap is stopped by the thigh's front or back plane, less the slack;
  - each flap is laid on the ground when it would go under it;
  - hanging props are aimed straight down.

  So the cloth in the posed pictures is close to the game's, but it is not the game's own drape.
- **Pose views.** Each pose was rendered three-quarter, side and close at the hips (elevation 22). Stride, knee raised, sit and kneel also have a close view from the miner's right (`_close_hips_right`, az -45).
- **The game's own frames.** These came from the courses run as played, with the flaps on, at
  D:/Dev/WG/TestRuns/CoverLive/cc_c1/pictures. They are all Small.
  - Eight to nine moments, each from four angles: side47, headright32, feetleft32 and above.
- **The in-game images in the repository:**
  - Docs/Images/Miners/Miners_Portraits.jpg;
  - Docs/Images/Miners/Miners_Dusk.jpg;
  - Docs/Images/Miners/Miners_DayStrategy.jpg.
- **Measurements.** These are explained in section 2.
- **Files.**
  - Every picture is in `pictures/`, named `<Miner>_<pose>_<view>.png`, at most 720 px; the game frames are at most 480 px.
  - The contact sheets are in `pictures/sheets/`.
  - The scripts and raw numbers are in `audit_tools/`:
    - render_miner.py, measure.py, silwidth.py and sheet.py;
    - the files `<Name>_sections.txt` and `silwidths.txt`.

  A known slip: measure.py prints the shoulder joint as hip + shoulder.y. The right value is hip + waistRise +
  shoulder.y, so its "chest" and "narrowest" summary lines use the wrong window. The per-height rows are right, and the
  numbers below were read from those rows by hand.

Contact sheets to start from:
- `pictures/sheets/all_rest_silhouettes.png`: the three silhouettes, front and side, at the same scale;
- `pictures/sheets/all_rest_close.png`;
- `pictures/sheets/rest_{Small,Long,Round}.png`, `poses_{...}.png` and `close_poses_{...}.png`;
- `pictures/sheets/Small_game_sheet_{a,b,c}.png`.

## 2. Measurements on the meshes

There are two kinds of measurement. Both are "over the clothing": they include the coat, smock and apron.

- **Visible width** is read from the flat front silhouette, in the image's centre column.
- **Sections** come from cutting LOD0's edges with level planes. They give width (x), depth (y) and the convex-hull girth. The trunk is every vertex whose strongest bone is the pelvis, spine, chest or a skirt flap.

The joint heights come from miners.json:
- the shoulder joint is at hipHeight + waistRise + shoulder.y: Small 1.015, Long 1.432, Round 1.209;
- the waist level is at hip + waistRise: Small 0.809, Long 1.144, Round 0.920.

| | Small (H 1.42) | Long (H 1.86) | Round (H 1.56) |
|---|---|---|---|
| Visible front width at the waist | **0.125 m** (z 0.80–0.82) | **0.183 m** (z 1.10) | **0.133 m** (z 0.92) |
| Waist width / H | 0.088 | 0.098 | 0.085 |
| Waist section, width × depth | 0.113 × 0.116 (z 0.812) | 0.189 × 0.206 (z 1.149) | 0.112 × 0.141 (z 0.923) |
| Waist girth (hull) | 0.35 m = 0.25 H | 0.63 m = 0.34 H | 0.40 m = 0.26 H |
| Chest, visible front width just under the arms | 0.22–0.236 | 0.262 | 0.323–0.326 |
| Chest section / girth | ~0.27 × 0.19 / ~0.75 | 0.261 × 0.255 / ~0.82 | 0.325 × 0.267 / ~0.93 |
| Hips, visible front width | 0.32–0.35 (z 0.58–0.62) | 0.30–0.35 (z 0.86–0.94) | 0.43–0.44 (z 0.60–0.66) |
| Hip girth | ~0.92–0.99 | ~1.03 | ~1.29 |
| Waist / chest, front width | **0.53** | 0.70 | **0.41** |
| Waist / hip, front width | **0.37** | 0.55 | **0.30** |
| Preset body waist (workers.py, full width) | 0.21 | — | 0.37 |
| Thigh tube, front width | ~0.10 (0.07 H) | ~0.119 (0.064 H) | ~0.143 (0.092 H) |
| Trousers' top / skirt hem / hip joint (z) | 0.489 / 0.414 / 0.619 | 0.993 / — / 0.947 | 0.641 / 0.516 / 0.713 |

What a natural body gives, roughly:
- **Front waist breadth:** about 0.15–0.18 H.
- **Waist breadth against chest breadth:** about 0.8–0.95.
- **Waist breadth against hip breadth:** about 0.7–0.9.
- **Waist girth:** about 0.42–0.50 H. Below about 0.40 H reads as very slim, and that is for a bare body; clothing only adds to it.
- **Mid-thigh breadth:** about 0.085–0.095 H.

These are approximate figures from general anthropometric knowledge. They were not checked against a source in this audit.

Read against these:

- **The waist.**
  - All three waists are about half of natural width.
  - Small and Round's waist girth is about half of the slimmest natural one.
  - Small's ratios (0.53 and 0.37) and Round's (0.41 and 0.30) are corset ratios, not body ratios.
  - Small's waist is about 60 percent of her own preset (0.125 against 0.21). Round's is about 36 percent of hers (0.133 against 0.37).
  - Luis's word, "too extreme how thin", is borne out by the numbers. Round is the worst, against what her preset says she is.
- **Long** is the least pinched. His 0.70 waist/chest ratio is still below natural, and the long coat hides the rest.
- **The trousers.**
  - Small's stop 13 cm below her hip joint. Round's stop 7 cm below hers.
  - The skirt hem is lower than the trousers' top: by 7.5 cm on Small, 12.5 cm on Round. At rest the skirt covers the gap.
  - So nothing joins the legs to the body above the trousers' top. Once a hem lifts, through a raised knee, sitting, lying or a flap swinging, the gap shows. That is the "floating legs".
  - Long's trousers reach above his hip joint and are whole.
- **The thighs** are thin:
  - Small at 0.07 H;
  - Long at 0.064 H, the thinnest;
  - Round at 0.092 H, which is natural for a person but thin against her very wide trunk and hips.

The likely cause of the pinch is a hypothesis, read from outfits.top and shapes.chain. It was not tested by changing the code.

1. A skirted top is built as a Skin-modifier chain with Subsurf level 2, then voxel fuse and smoothing.
2. Its root end sits at the waist. It starts at waist − 0.02, with the radii × 0.82.
3. A Skin chain's end is capped and rounded like the end of an egg. Subsurf pulls a short chain's middle into a ball. So the chest becomes a sphere and the waist becomes the egg's narrow end.
4. The skirt is fitted to the top's surface, less about 7 mm, so it follows the pinch down.

This explains a narrowing far beyond the 0.82 tuck alone, which would give about 0.17–0.21 m on Small, not 0.125. A test would be to rebuild one top with an extra ring at the waist at full radius, or with the tuck removed, and measure again.

## 3. Small

Small is 1.42 m, with a blue coat, a satchel, a lantern and big eyes. Read with `pictures/sheets/rest_Small.png`, `poses_Small.png` and `close_poses_Small.png`.

| # | Finding | Sev. | Where | Picture |
|---|---|---|---|---|
| S1 | Hourglass trunk: a ball chest, a deep notch at the waist (0.125 m against a 0.33 m skirt), then a bell skirt. She reads as a doll or a child in a party dress, not a small sturdy miner. | B | rest, every front and back view; visible at the game camera and even from above in the game | `pictures/Small_rest_front.png`, `pictures/Small_rest_silhouette_front.png`, `pictures/Small_rest_close_waist.png`, `pictures/Small_game_c07_f029202_knee_above.png` |
| S2 | Floating legs: the trousers start 13 cm below the hip joint. When the coat lifts, two grey tubes start in the air under the hem, with a visible gap. | B | lying on the back, sitting, knee raised; on all fours in the game; close and at the game camera | `pictures/Small_back_close_hips.png`, `pictures/Small_game_c04_f016246_knee_side47.png`, `pictures/Small_game_c00_f001150_knee_headright32.png`, `pictures/Small_game_c06_f028220_knee_above.png` |
| S3 | A raised thigh pierces the coat high, near the hip. The skirt's upper band is weighted to the pelvis and stays stiff. From the right, the leg comes out through the side seam, as Luis saw. | B | knee raised, stride (less); close, and visible at the game camera on a high step | `pictures/Small_kneeup_close_hips.png`, `pictures/Small_kneeup_close_hips_right.png`, `pictures/Small_stride_close_hips_right.png` |
| S4 | Sitting: both thighs come out through the coat's sides. The coat hangs to the ground as a closed tube or cone, not over the lap. | V | sit, every view | `pictures/Small_sit_threequarter.png`, `pictures/Small_sit_close_hips.png`, `pictures/Small_sit_close_hips_right.png` |
| S5 | On all fours the coat balloons into a big sack or hump over the hips, larger than the body under it. | V | game: knee and hip moments; at the game camera | `pictures/Small_game_c02_f007676_knee_side47.png`, `pictures/Small_game_c03_f008739_knee_side47.png`, `pictures/Small_game_c00_f000957_hip_side47.png` |
| S6 | Lying down, the coat's corner stands up as a stiff pointed tip. | V | lying in the game; my lying-on-the-back render | `pictures/Small_game_c04_f016246_knee_feetleft32.png`, `pictures/Small_back_threequarter.png` |
| S7 | Legs are thin sticks under a short, wide skirt (thigh 0.07 H). The side profile is flat and thin: little depth to the chest, belly or seat. | V | rest side, silhouettes; game camera | `pictures/Small_rest_side.png`, `pictures/Small_rest_silhouette_side.png` |
| S8 | Arms are noodle tubes of the same thickness from shoulder to wrist, with no elbow point. The cuffs puff out larger than the forearm. | V (close) / m (game) | rest close, stride | `pictures/Small_rest_close_hands.png`, `pictures/Small_rest_threequarter.png` |
| S9 | The hair's sawtooth hem reads as a spiky blob from behind and above. | m | back, three-quarter back, game "above" | `pictures/Small_rest_back.png`, `pictures/Small_game_c03_f008739_knee_above.png` |
| S10 | Knees bend like a bent pipe, with no kneecap and no change in volume. | m | knee raised, kneel, sit; close | `pictures/Small_kneel_close_hips.png` |
| S11 | Her face washes out pale in daylight (only in the in-game day picture). | m | game camera, day | Docs/Images/Miners/Miners_DayStrategy.jpg |

Good on Small:
- the big eyes and the face;
- the blue coat's colour;
- the satchel and the lantern, which hang by gravity;
- the hand at the collar in the portrait;
- the laced boots.

## 4. Long

Long is 1.86 m, with a brown long coat, a mug and a long nose. Read with `pictures/sheets/rest_Long.png`, `poses_Long.png` and `close_poses_Long.png`.

| # | Finding | Sev. | Where | Picture |
|---|---|---|---|---|
| L1 | An hourglass under the long A-line coat. The waist (0.183 m, waist/chest 0.70) is still pinched against natural, but the coat's length softens it. | V | rest front and back; game camera | `pictures/Long_rest_front.png`, `pictures/Long_rest_silhouette_front.png`, `pictures/Long_rest_close_waist.png` |
| L2 | A raised knee pushes the front flap out as a stiff flat board or tent, with a dark void inside. The cloth does not fold or drape over the thigh. | B (close) / V (game) | knee raised, stride | `pictures/Long_kneeup_threequarter.png`, `pictures/Long_kneeup_close_hips.png`, `pictures/Long_kneeup_close_hips_right.png` |
| L3 | Kneeling: the back panel sticks out as a fin or sail, and the front flap as a spike. | B | kneel, every view | `pictures/Long_kneel_side.png`, `pictures/Long_kneel_threequarter.png`, `pictures/Long_kneel_close_hips.png` |
| L4 | Sitting: the coat panels splay out as stiff flat sheets, not cloth pooling on the ground. | V | sit | `pictures/Long_sit_threequarter.png`, `pictures/Long_sit_close_hips.png` |
| L5 | The thinnest thighs of the three (0.064 H). Long and thin legs under a heavy coat read as stilts. | V | rest side and front, stride | `pictures/Long_rest_side.png`, `pictures/Long_stride_side.png` |
| L6 | The head juts forward, and the nose is a sharp spike from the side. Charming from the front; harsh in profile. | m | rest side, silhouette side | `pictures/Long_rest_silhouette_side.png`, `pictures/Long_rest_close_head.png` |
| L7 | Noodle arms with no elbow point, the same as on Small. | m | rest close, stride | `pictures/Long_rest_close_hands.png` |
| L8 | Lying on the back or side, the coat's panels lie as flat stiff planes. | V (close) | back, side | `pictures/Long_back_threequarter.png`, `pictures/Long_side_threequarter.png` |

Good on Long:
- **the trousers are whole and reach above the hip joint**, so no leg floats; this is the model for Small and Round;
- the long coat's colour and line;
- the mug held in the hand;
- the long nose from the front;
- the height that reads at once.

## 5. Round

Round is 1.56 m, with a green smock, an apron and a hammer. Her preset idea is "round and heavy". Read with `pictures/sheets/rest_Round.png`, `poses_Round.png` and `close_poses_Round.png`.

| # | Finding | Sev. | Where | Picture |
|---|---|---|---|---|
| R1 | The most extreme hourglass of the three: chest 0.325, waist 0.133, hips 0.44 (waist/chest 0.41, waist/hip 0.30). She reads as a corseted bust over a bell, not round and heavy. This fails her one clear idea. | B | rest, every front, back and silhouette view; visible at the game camera | `pictures/Round_rest_front.png`, `pictures/Round_rest_silhouette_front.png`, `pictures/Round_rest_close_waist.png`, `pictures/sheets/all_rest_silhouettes.png` |
| R2 | The apron bib clings into the waist pinch, where a hanging apron would bridge it straight from the chest. This deepens R1. | V | rest front, three-quarter; close | `pictures/Round_rest_threequarter.png`, `pictures/Round_rest_close_waist.png` |
| R3 | Floating legs: the trousers stop 7 cm below the hip joint, and the hem lies 12.5 cm below the trousers' top. The gap shows when the smock lifts. | B | knee raised, sit, lying | `pictures/Round_kneeup_close_hips.png`, `pictures/Round_sit_close_hips.png`, `pictures/Round_back_close_hips.png` |
| R4 | A raised or sitting thigh passes through the smock's surface. The apron stays a flat board or lies as a flat tongue. | B (close) / V (game) | knee raised, sit | `pictures/Round_kneeup_threequarter.png`, `pictures/Round_kneeup_close_hips_right.png`, `pictures/Round_sit_threequarter.png` |
| R5 | Kneeling: the smock and apron jut out horizontally as a stiff shelf over the front thigh. | B | kneel | `pictures/Round_kneel_threequarter.png`, `pictures/Round_kneel_side.png`, `pictures/Round_kneel_close_hips.png` |
| R6 | Lying on the back: the apron and smock pass through each other (mottled patches). Lying on the side: the apron hangs to the ground as a stiff plate. Partly my static drape, but the game's per-flap drape would do much the same. | V | back, side | `pictures/Round_back_threequarter.png`, `pictures/Round_side_threequarter.png` |
| R7 | Forearms and wrists are thin against her heavy body and the hammer. This goes against strength from muscle: the strong one has the arms of a slim one. | V | rest close and three-quarter; game camera | `pictures/Round_rest_close_hands.png`, `pictures/Round_rest_threequarter.png` |
| R8 | Thighs (0.092 H) are natural in themselves but thin under 0.44 m hips. The legs read as pegs under a bell. | V | rest front and side | `pictures/Round_rest_side.png`, `pictures/Round_rest_silhouette_side.png` |
| R9 | The side profile is flatter than "round" suggests: little belly. Her depth sits in the chest and the skirt. | V | rest side, silhouette side | `pictures/Round_rest_silhouette_side.png` |
| R10 | Knees bend like a pipe, and the elbows have no point (as on the others). | m | kneel, sit; close | `pictures/Round_kneel_close_hips_right.png` |

Good on Round:
- the closed-eye smile;
- the hands-on-hips gesture in the portrait;
- the green smock with the apron as a colour idea;
- the hammer;
- the lamp.

## 6. Shared to all three

1. **The trunk pinch at the waist** (S1, L1, R1). It is one construction for all three, the skirted top's Skin+Subsurf chain (hypothesis in section 2), so it would be one fix. Severity B on Small and Round, V on Long.
2. **Garments that are rigid flap boards.** Each skirt is four flaps (front and back, left and right), each a rigid bone-weighted plate. A raised thigh either goes through a flap or pushes it out as a board. They never fold, bunch or drape over the leg. Seen in S3, S4, L2–L4, L8 and R4–R6. This is the largest kind of fault in motion. It is also behind "the leg sometimes clips through the skirt".
3. **The skirt's upper band is weighted to the pelvis.** Between the waist and the flap roots, the cloth cannot follow the thigh at all. That is where the high piercing happens (S3, R4).
4. **Trousers that end below the hip** on Small and Round (S2, R3). Long shows the cure: trousers whole to above the hip joint and continued up under the coat, as Luis asked.
5. **Thin limbs against the trunk.**
   - Thighs at 0.064–0.092 H.
   - Arms are constant-thickness tubes with no elbow point, no wrist taper and no forearm swell.
   - Knees have no kneecap.

   Strength from muscle is not visible in any limb.
6. **Flat side profiles.** Chests, bellies and seats have little depth, so the side silhouettes are thin columns with a skirt (`pictures/sheets/all_rest_silhouettes.png`).
7. **Front silhouettes say the same shape three times.** A ball chest, a notch and a bell. Size and colour tell them apart, not the body. A redraw could give each its own trunk: Small compact and stocky, Long long and narrow and straight, Round barrel and belly.

Shoulders and hips at rest deform acceptably. No candy-wrapper twisting was seen in these poses. The hip joints are hidden by the skirt in most poses, so their deformation is mostly unread (see section 9).

## 7. The game's own pictures

All the frames are Small, with the flaps on, in the courses as played (cc_c1). Read with `pictures/sheets/Small_game_sheet_{a,b,c}.png`.

- **Get-up on all fours** (c02_f007676, c03_f008739, c00_f000957): the coat balloons into a sack over the hips (S5). It is the strongest in-game fault after the waist.
- **Leg seen through the coat** (c00_f001150 headright32): a grey trouser-tube end sticks up out of the coat (S2).
- **Lying** (c04_f016246): grey trouser stubs come out at the coat's corner, and a stiff coat tip stands up (S2, S6).
- **From above** (c06_f028220): a trouser leg shows beside the coat. (c07_f029202): the hourglass waist reads even from straight above (S1).
- **The cut-ring moment** (c03_f008096, ring_game+ring_all+hip): Small is standing. At this picture size the coat
  reads well: it hangs, the legs join under the hem, and no ring is readable. It is a good reference for what the coat
  should look like in motion. The ring itself was studied by the earlier cut-ring reading, not here.
- **Strategy height** (Docs/Images/Miners/Miners_DayStrategy.jpg): the three read apart by colour and height, but they are tiny. At that height the waist pinch and the thin legs are still what sets the silhouette.
- **Portraits and dusk** (Docs/Images/Miners/Miners_Portraits.jpg, Miners_Dusk.jpg): the faces and gestures carry great charm. At dusk the lit faces and lamps fit Luis's taste: an intimate warm light against a darker world.

## 8. What is good and must be kept

- **The faces, which Luis loved:**
  - Small's big eyes;
  - Round's closed-eye smile;
  - Long's long nose, seen from the front.

  A redraw should keep their head meshes and textures.
- **Each one's colour identity:**
  - Small: the blue coat;
  - Long: the brown long coat;
  - Round: the green smock with the cream apron.

  These read apart at every distance.
- **The readable height differences** (1.42, 1.86, 1.56).
- **Long's whole trousers.** This is the pattern for the other two.
- **The props, which hang and are held:**
  - the lantern and the satchel;
  - the mug in the hand;
  - the hammer.
- **The laced boots** and the fuller palms of the hands.
- **The hair shapes** as ideas: each miner's is distinct. Small's hem needs softening, not replacing.
- **The stylisation** (a little cartoon, reads natural) and **the tonal palette**.
- **The gestures in the portraits**: Round's hands on hips, Small's hand at the collar, Long's mug.

## 9. What this audit did not do

- **The poses are static, hand-set bone turns.** The cloth uses my emulation of the flaps' drape, not the game's dynamics. Swinging, speed and contact were not seen. Only the game frames (Small only) show the real drape.
- **No Long or Round game frames** were read. The courses used were cc_c1 only, which is Small. Long's and Round's in-game faults in motion are inferred from the Blender poses.
- **The getting-up leg movement**, which Luis called unnatural, was not judged. It is motion, and these are stills. This audit is about the models.
- **Shoulders and elbows** were not given dedicated close captures in strong poses: arms raised, swinging a hammer, or reaching. Their deformation is judged only from rest and the leg poses.
- **No web search** was made. The natural proportions in section 2 are approximate, from general knowledge, and should be checked against an anthropometric source before they are used as targets.
- **The cause of the waist pinch** (the Skin+Subsurf chain's capped end) is a hypothesis. It was not tested by rebuilding a top.
- **LOD1 and LOD2** were not audited; only LOD0.
- **The cut ring** was not studied. Its one game frame shows no ring at 480 px.
- **The silhouettes have white holes** where glow and eye whites are. Widths were measured on the centre column, so the holes do not affect the waist numbers.
- **A housekeeping note.** Early in the work some helper files were first written to the scratch folder's root, which other sessions also use. A file named sheet.py there may have replaced another session's file of the same name. My files were then moved to their own subfolder.
- **Severities are my judgement.** They are not the Game Director's. Passing measurements here does not mean anything is accepted.
