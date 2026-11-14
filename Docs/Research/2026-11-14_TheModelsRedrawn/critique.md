# A sceptic's critique of research.md (the miners redrawn, part one synthesis)

November 14, 2026 (night). I read research.md against internal.md, audit.md, external.md, the sheets in
`pictures/sheets/`, Validation.md ("The cut ring as played"), the register rows 542 to 544 in GAME_VISION.md, the
correspondence file with Luis's words, and the Blender scripts `outfits.py`, `shapes.py` and `workers.py`.

How each point is marked: **(checked)** I re-read the source and it differs from research.md; **(picture)** seen in a
sheet; **(hypothesis)** my inference. Severity: high, medium, low. Nothing here changes research.md.

What holds up: the measured numbers I re-checked (waist widths and ratios, trouser cuts against the hip joints, the cut
ring counts, the 172.4 degree fold, the 165.4 degree spin) match audit.md and Validation.md. The waist cause in code
(0.82 tuck, subdivision level 2, voxel remesh with smooth 6) matches `outfits.py` and `shapes.py`. The preset and
physics-leak list matches internal.md. The faults and the "kept" list match the audit.

---

## Points

### 1. High: rung A is called "visual only" but it is a rig and code change
- **Passage:** 5.1 "A visual-only second version"; 5.4 "Physics touched: no"; "Size of the work: moderate"; 1c "a
  second bone or more flaps", "the flaps also follow a thigh that opens sideways".
- **What is wrong:** More flap bones, a second bone per flap, new upper-band weights and a drape that follows a sideways
  thigh change the rig, `MinerBody.Drape` (game code, work branch), `skirtSlack`, the prop stops, `FlapSeamTests` and
  the prefab. That is the "rigging" half of Luis's open question, not "the visual part". The ladder hides this: rung A
  mixes art that touches nothing (a waist, whole trousers) with rig and code work. Also, Luis has already decided the
  raised leg's front flap rests and its point is rounded (register row 544). That is a change to the existing drape
  task, which research.md files under the second version (1c, 7.2); if it is made in shared game code it changes the
  first version too, against 7.3's "first version kept".
- **Fix:** Split A in two. A1: static art only (waist, whole trousers to a seat, the upper band re-weighted with the
  existing four flap bones, no new bones, no code). A2: new bones and drape code, behind a version gate in the game code.
  Say for each change whether it reaches the first version. Put Luis's "how far" question against A1, A2, B.

### 2. High: "physics numbers frozen, equal field by field" cannot hold, and it can build a mismatch
- **Passage:** 5.1 "Every number the physics reads is written exactly as in the first version"; 8.1 "masses, centres,
  moments and radii equal the first's".
- **What is wrong (checked):** weights.py adds the worn mass (internal.md 3.1, 4.2). Whole trousers and a seat add real
  cloth mass, so the masses must change a little, or they are false. That goes against Luis's "true weights". Also
  `bodyFront`, `faceFront` and `headHalf` are measured on the clothed model. If they are frozen while Round's trunk goes
  from a visible 0.133 m wide to near her preset's 0.37 m, the hip capsule and trunk rod stay narrow under a wide drawn
  body (hypothesis on effect). Research.md never says the physics already uses the preset body (a barrel), so the redraw
  brings look and physics closer; the corset made the drawn body thinner than the physical one.
- **Fix:** List which numbers must stay (limb radii, trunk presets, levels, `headHalf`, bone positions) and which should
  follow the new cloth (worn mass, `bodyFront`, `skirtSlack`), with a tolerance. Decide "freeze or re-measure" on purpose
  for `bodyFront` and show what each does to Round. Rewrite the Stage 0 test as "these equal, those change by at most X".

### 3. High: the getting-up checks cannot be compared, and the own get-up's blow-up is not mentioned
- **Passage:** 8.3 "the getting-up bars of November 14 ..., the same runs both versions"; 5.1 "Getting up re-checked:
  checked only".
- **What is wrong (checked):** Validation.md says a course does not run again the same (`cc_det`: the same build and
  push gave 3 tries and 42.5 s against 6 tries and 102.4 s), course 4 of `cc_c1` blew up (NaN, 8e21), one blow-up in
  eight courses, and the test still passed because failing log messages are ignored. Research.md names none of it. With
  eight, four and four courses per run, a before and after comparison of a model change is noise, and a model change
  could not be told from a blow-up.
- **Fix:** Add this to the risks. State that no get-up bar is a gate for A1 or A2; use paired many-course runs only for
  B, after the energy count. Do not claim "checked" for the getting up while the blow-up is open.

### 4. Medium: the recommendation sits oddly with Luis's words, and a missing option is the one he asked for
- **Passage:** 7.1 "Start with rung A ... Go to rung B only with Luis's word"; 1.1 quotes.
- **What is wrong (checked, correspondence lines 13 to 34):** Luis also said "we probably need to actually redesign the
  model to some extent" and "I think we should actually make this big transformation ... smaller scale, not smaller
  quality". Research.md quotes neither. Its A-first path is argued from the physics risk, which is fair, but it is
  presented as following from the evidence, when a bigger first look is equally supported. A missing option: show the
  look of rung B (thicker thighs, belly, seat, a different build) as Blender stills early, beside today's, with no
  physics change, so Luis can decide the look before any retuning is paid for. This also meets "shown early".
- **Fix:** Quote both phrases. Add "stills of A and B looks first, then build" as a path, and say why A-first is or is not
  better. Keep it his call.

### 5. High: shared Blender modules can silently change the loved first version
- **Passage:** 5.1 "made in the shared modules (`outfits.top`, `skirt`, `trousers`, the rigging rules)"; 7.3 first
  version "as it is".
- **What is wrong (hypothesis, strongly suggested by the code layout):** If `outfits.top` and `trousers` change for v2,
  any later re-export of v1 changes too, so the "unchanged" first version is only unchanged until someone re-runs the
  script. The exporter also writes `miners.json` for all miners (internal.md 3.5); a v2 export could overwrite v1's
  entry, which holds the physics numbers. Research.md checks that v1's files are "unchanged" once (8.1) but does not guard
  the script or the json.
- **Fix:** New behaviour behind an explicit flag, default off. A golden check: regenerate v1 and compare it with the
  shipped fbx and json. The v2 export writes only v2 entries. Name the editor builder (`MinerSetup.cs`) in the file list.

### 6. Medium: "the first step of rung C" is not supported
- **Passage:** 5.1 "That is the first step of rung C"; 7.1 "build nothing in A or B that C would have to undo".
- **What is wrong:** Research.md itself says (5.3, section 9) it did not research how a creator body is made while the
  game runs, and that Way B (Blender per body) takes minutes and is not live. If creator bodies cannot use Blender-built
  garments, then fixes made in the Blender modules do not carry into the creator at all. The claim is a hypothesis that
  reads as a benefit. Also, the "nothing C would have to undo" rule cannot be followed without knowing C.
- **Fix:** Mark the claim as a hypothesis. Add the creator-body method as a named gap that must be researched before
  anyone relies on it. Say plainly that A is worth doing for the three even if the creator uses a different route.

### 7. Medium: the waist numbers mix measures and over-reach the sources
- **Passage:** 2.1 "These are corset ratios"; "Every measured or art-historical waist-to-hip value in the sources is 0.5
  or more"; 4.2 "A heavy build sits at 0.85 or above (source S2)"; the 4.3 table.
- **What is wrong (checked):**
  - S1/S2's values are girth ratios from art, not measured bodies. They are compared with front widths.
  - The audit also gives girths: waist 0.35, 0.63, 0.40 against hip 0.92 to 0.99, 1.03, 1.29. That is a directly
    comparable ratio: Small about 0.37, Long about 0.61, Round about 0.31. Research.md never computes it, though it is the
    one comparison the sources allow. It also shows Long near the art range, not "corseted".
  - S3's canon has the ribcage and the waist both 4/3 head wide, so waist to chest is 1.0, not "0.67". The 0.67 is
    waist to shoulders. The table puts 0.67 beside waist to chest.
  - S2's 0.85 and 0.90 are abdominal obesity thresholds. Using them for a "round and heavy" character is a stretch, and
    external.md itself marks it as inference.
  - The canon is a male-hip canon, applied to characters research.md calls "she".
- **Fix:** Compute girth ratios and use those against S1/S2. Say that front-width ratios have no source. Fix the canon
  line (waist = ribcage). Drop or mark the obesity use. Keep "corset" as an impression, not a measured fact.

### 8. Medium: "one root" is not true for Long, nor for the clipping
- **Passage:** section 3, "One root behind the three faults Luis named ... the same place: the trunk is made only of
  garments".
- **What is wrong (checked):** Long's visible waist girth (0.63) equals his preset's (0.618); internal.md 3.3 says his
  own body numbers, 0.33 H, the thinnest of the three, "add to it". Rewriting the garment will not widen Long's waist;
  that is a preset (rung B) number. For the clipping, research.md's own table says the cause is the drape rule that
  ignores a sideways thigh and the upper band's weights, with only medium confidence. Calling it the same root as the
  waist is neater than the evidence.
- **Fix:** Say there are two roots: the garment construction (waist, legs) and the rigid flaps with the drape rule
  (clipping, boards). Say that Long's waist is mostly his preset, and that rung A will leave it.

### 9. Medium: the getting-up numbers are misquoted, and the conclusion is stated as fact
- **Passage:** 2.4 "the own get-up's hip folds to 172.4 degrees ... on 8146 of 19376 getting-up rows"; "So the movement
  Luis sees comes mainly from the body that gets up".
- **What is wrong (checked):** 172.4 degrees is the single deepest fold (course 3, frame 11517). 8146 rows are the rows
  that fail bar 3; the threshold is not given. The text reads as if all those rows reach 172.4. Also these are Small's
  own get-up only; the old way gave 77 and 65 failing rows; Long was not run at all ("Not tested: Long"). The
  claim "mainly from the body" was not judged by eye (research.md says so in 2.4 and 2.11) yet is the conclusion.
  Luis's own words: "I think we didn't touch the getting up part yet", which supports it but is not evidence.
- **Fix:** Write "deepest fold 172.4; 8146 of 19376 rows over the bar (give the bar)". Mark "mainly the body" as
  hypothesis until a picture of the deepest fold has been read (Validation.md lists that as not done). Say Long has no data.

### 10. Medium: "Long is free of it" and the "all fours" claim rest on thin evidence
- **Passage:** 2.2 "Long is free of it: his trousers are whole ... (measured)"; 2.3 the sack "seen often".
- **What is wrong:** Whole trousers is measured; "free of the floating legs" in the game was never tested. The game runs
  were Small and Round only, and Validation.md lists Long under "Not tested". The trousers top is only 4.6 cm above his
  hip joint. In stills (`pictures/sheets/close_poses_Long.png`) I see no gap, but his open front shows a dark void with the
  raised knee, which is a different fault. The sack on all fours (S5) is said to be seen "often" in get-ups; it was
  seen in three frames of one course.
- **Fix:** Soften to "no gap in the posed stills; not run in the game". Say how many frames and courses stand behind "often".

### 11. Medium: prop overlaps are called artefacts, but they appear in many pictures
- **Passage:** 2.9 "most likely artefacts of the static pictures (hypothesis)".
- **What is wrong (picture):** The overlap is not two cases. It recurs: Small's lantern behind the raised thigh
  (`Small_kneeup_close_hips.png`), the lantern under and between the thighs sitting (`Small_sit_close_hips.png`), Long's
  mug inside the front thigh when kneeling (`Long_kneel_close_hips_right.png`) and under the thigh sitting
  (`Long_sit_close_hips.png`), the hammer against the thigh kneeling (`Round_kneel_close_hips_right.png`) and standing
  upright on the ground (`Round_side_close_hips.png`). Luis's rule is that objects hang, swing and are stopped by the body.
  The audit's own method (props aimed straight down, not stopped) explains some of it; nobody checked the game's pendulum.
- **Fix:** Move these from "probably not real" to "unverified, probable; check the stops in the game for all three".

### 12. Medium: faults nobody looked for yet (my reading of the sheets)
- **Passage:** section 2 as a whole; "look in all the models for anything that does not look natural".
- **What is wrong (picture):**
  - Small's trousers are a flat grey with almost no painted texture, unlike her coat and unlike the other two
    (`pictures/sheets/rest_Small.png`, `pictures/sheets/close_poses_Small.png`). TheEssence's frame test bars flat
    untextured areas. The new trousers and seat would be seen far more; they must be painted.
  - Legs are straight columns with no calf, no ankle taper and no thigh-to-knee change (`pictures/sheets/all_rest_silhouettes.png`). The
    boots read as large black-slab soles in profile and kneeling (`pictures/Small_kneeup_close_hips_right.png`,
    `pictures/Small_kneel_close_hips_right.png`). Research.md covers thin thighs and knees only.
  - Necks: Round's head sits almost on her shoulders, Small's collar swallows hers (`pictures/sheets/all_rest_close.png`).
    Luis said on October 3 that heads looked glued.
  - Every rest render is the bind pose, arms 25 degrees out. The silhouette impressions (a ball chest, a bell) are partly
    the arms and shoulders in a pose the game's idle does not use. Nobody rendered the idle.
- **Fix:** Add these to section 2 as new faults with a "to look at" mark, and render the idle's arm pose before judging the chest.

### 13. High: "pretty" was never checked against the reference pictures
- **Passage:** section 9 "The six reference images ... were not opened"; section 4 (from guides in words).
- **What is wrong:** Luis asked for "much more pretty". The target look is in the references (B and E are the two for
  people; his own bust reference is only described) and in his taste notes. Section 4 is text read about pictures nobody
  opened, so the proposal's idea of "pretty" is second-hand. The sources in external.md are about proportions and rigs, and
  say nothing about prettiness beyond Disney principles and a Pixar rebuild.
- **Fix:** Open B and E, the dusk images and the portraits before any look is made, and write a half page of what they
  show for a body, a waist and a coat. State this in the path as a gate for stage 1.

### 14. Medium: options missing from the ladder
- **What is wrong:**
  - A modelled base body (a torso and seat as an under-layer) with garments over it. Research.md mentions "a trunk and a
    seat" once (5.3) but does not weigh it against whole trousers. It would fix the air under the coat on all fours and
    the coat's hollow, and it is the practice S26 reports. Whole trousers to a seat leaves no belly or torso under a
    lifted coat.
  - The skirt mechanism is never chosen. External.md's evidence (S21 to S23) is parent or sync bones plus capsules on the
    legs; research.md says "a second bone or more flaps" and does not say how a flap follows a thigh, what the capsules
    would be, or what it costs. No cost number exists (external.md says so).
  - A weights-only step: re-weight the upper band to the thigh with no mesh change. It might fix the high piercing (S3, R4)
    cheaply, and could be tried first on a copy.
  - The look-first option in point 4.
- **Fix:** Add these as rows or sub-rungs, each with what it fixes and risks.

### 15. Medium: the work's real size is understated
- **Passage:** 5.4 "Size of the work: moderate" for A.
- **What is wrong:** Not counted: repainting the atlas for a new trunk, trouser seat and coat (the painted, drawn look is
  an aesthetic rule); LOD0 to LOD2 for three miners (triangle headroom today is not given); the elbow, forearm and kneecap
  rebuild in stage 2, which rebuilds the arm skin chains the grips and hand bones depend on; the editor builder; the switch
  and its names; the judge brief; new tests; game-side verification for Long and Round, which has never run. AGENTS.md says
  Luis watches usage. No estimate of stages or sessions is given.
- **Fix:** Give a rough number of agent sessions per stage, and say which stage is the first one Luis can see.

### 16. Medium: "never free sliders" borrows a rule the project has not made
- **Passage:** 6.3 "Body: presets plus a few bounded choices, never free sliders for shape".
- **What is wrong:** The source is Baldur's Gate 3 (not a simulated body) and Won and Lee (research). The register has
  "customization of everything, from modules that fit any body" (Direction) and a guardrail that a limited early UI must not
  become a ceiling on customization (GAME_VISION.md:443). The roadmap's S2 plans height, build and strength as body
  categories. "Never" cuts against both. It is marked hypothesis, but the word is strong and the guardrail is not cited.
- **Fix:** Say "bounded sliders or steps, from a verified range"; cite the guardrail; leave the count to Luis.

### 17. Low: the target bands contradict themselves
- **Passage:** 4.3 table.
- **What is wrong (checked):** For Small, 0.75 to 0.85 of chest (0.22 to 0.236 m) gives 0.17 to 0.20 m; 0.7 of hip
  (0.32 to 0.35 m) gives 0.22 to 0.25 m. The two bands do not meet. Long's waist to hip (0.55) is left out. The preset waists
  (Small 0.21, Round 0.37) are the one target with a record behind it, and the research says so in 2.1, then adds guesses.
- **Fix:** Use "each preset's own waist, plus the coat's looseness" as the main target; show the other bands only as stills.

### 18. Low: "nearly every frame" overstates what the cut-ring count shows
- **Passage:** 2.2 "on the ground the cut is seen on nearly every frame".
- **What is wrong (checked):** Validation.md counts rows where the ring is seen "from the game camera's ways" (25 views), and
  says the pictures show hollows and gaps. A row is counted if it is seen from the ways, not from the one camera a player has.
  Standing counts are small for Round (1 of 50) but 14 of 59 for Small.
- **Fix:** Say "seen from at least one of the capture ways", or give the count for the player's camera.

### 19. Low: Stage 0 builds before the proposal is agreed
- **Passage:** 7.2 Stage 0 (a switch, a test, tool changes).
- **What is wrong (checked):** Row 543 says a proposal comes first, "for him to agree to before anything is built". Stage 0
  is mostly infrastructure, but the research reads as a work plan, not a proposal.
- **Fix:** Label section 7 "for the proposal". Put the cheap Blender stills for Luis (waist, trousers) ahead of the switch and UI work.

### 20. Low: the judges' lens set is thin
- **Passage:** 7.2 Stage 0: "a costume maker, the game's art director, a player at the game's distance".
- **What is wrong:** Luis asks for "natural" as well as "pretty". No lens is a figure or anatomy artist. The judges need the
  reference pictures (point 13) in front of them, or they judge from the same text research.md did.
- **Fix:** Add a figure-and-anatomy lens, and the references and today's three as the control row.

### 21. Low: the thin-forearm fault is a look fault, not a broken rule
- **Passage:** 2.5 "This works against Luis's rule that strength comes from muscle".
- **What is wrong:** The strength-from-muscle rule is decided, but the three miners keep their old strengths
  (`KeepsWhatItHad`, internal.md 4.1), so today's thin forearms do not break the game's own rule; they break the Direction's
  "appearance ties to capability" look.
- **Fix:** Say the fault is the look, not the rule, until the strength choice (question 5) is made.

---

## What I checked and found sound
- Every number in the 2.1 and 2.2 tables against audit.md section 2, and the cut ring table against Validation.md.
- The quotes in 1.1 against the correspondence file (accurate; the two I list in point 4 are missing).
- The tags S1 to S44: no claim in research.md goes beyond external.md, except the uses in point 7. Snippet-only sources
  (S6, S10, S37, S42) are marked as such.
- Nothing in research.md promotes an Open item to Locked. The questions in 7.4 stay Luis's.

## What I did not do
- I did not run Unity or Blender, rerender anything, or read every picture (about 15 sheets and stills were opened).
- I did not re-check S1 to S44 against the web; no web search was made.
- I did not read the flap judges' verdicts in full, CharacterPractices.md or the DetachedLeg investigation.
- I did not verify the line numbers research.md gives for `MinerBody.cs`, `OwnBody.cs` or `rigging.py` (work tree); I
  checked `outfits.py`, `shapes.py` and `workers.py` in the main tree only.
