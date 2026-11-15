# Critique of research.md: sceptic and completeness check

November 15, 2026. A critic's pass over `research.md`, checked against `inventory.md`, `modelling.md`,
`faces_heads_hair.md`, `surfaces_light_render.md` and `world_and_props.md`. Nothing was run. research.md was not
changed. Line numbers (L) point into research.md.

Severity: **high** (changes a decision or breaks a standing rule), **medium** (a wrong or missing claim that matters),
**low** (wording, small gap).

Overall: the file is honest about its limits and the adjudications in 2.3 are mostly right (checked against the part-one
sources). Its weak spots are (a) a missing visual part (hands, feet), (b) a protect list that is too short for the
favourite frame, (c) switches that need Luis's ruling but are scheduled as if they did not, (d) no cost model beyond one
GPU number, (e) a version/switch scheme whose combinations are never counted, and (f) a few claims stated stronger than
their evidence.

---

## A. Completeness: parts of the visuals and dropped gains

**1. [high] Hands, feet, boots, ears and necks are not a visual part anywhere in research.md.**
Passage: the 2.1 table (L107 to L129) has no row for them; "hands" does not appear in the file.
Wrong: inventory 0.6 says "pretty faces and hands are the hard part". Modelling keeps a 1,600-triangle hand budget with
finger bones and a `whole_hand` LOD2. Hands and boots are what the eye meets in every work close-up (pick, mug, satchel
grips) and in the "physical objects are held" rule. The only mention of necks is "necks sink into collars" in the heads
row, with no gain attached. Surf rank 12 ("a 4096 atlas or separate head and hand maps") was dropped; G8 covers only the
face.
Fix: add rows for hands, feet and boots, ears and neck to 2.1 with a verdict and evidence; give them a gain (or say
"not looked at"). Put hands and boots on the step-0 close-up sheets so they are judged beside faces.

**2. [high] The protect list for the favourite frame is incomplete.**
Passage L602 to L604: "G18 and G19 touch it; they are variants, and the favourite frame is in every capture of them."
Wrong: the frame is "the lit house at dusk from low on the path, tall dark grass in front, the large tree behind".
G23 (grass variety, tier 2) changes the tall dark grass in front. G17 (ground detail layer and path ribbon) changes the
path and ground in front. G21 (far land, cloud shadows) changes what stands behind the house. G15 (flowers, things at
the house) and G13 (VFX) place new objects in it. G20 (depth-aware paint radius) changes the whole screen, this frame
included, and sits in tier 2 while the risk table (L624) lists only "APV, grading, TAA, depth of field".
Fix: list every gain that changes any pixel of the favourite frame; require the frame as a control capture for each,
off by default, and move G20 and G23 into the risk table.

**3. [medium] The Strategy camera is mostly unweighted in the ranking, and its checks were dropped.**
Passage: 2.2 ranks "by how much each harms pretty and natural in the views Luis counts" (L133).
Wrong: at the Strategy camera a miner is about 30 px and a head about 10 px (picture, faces 2.5), and no face mark can
be seen. The ranking never says which gains are visible at that scale. Painted atlas layers (G16, M), stitches, flyaways
and hair springs are invisible there. World C3 and T7 ("check the frame test at 42 m", grass LOD, fog colour, ink and
paint scale) and S2 were dropped from the programme. The Strategy view is the RTS's main view.
Fix: add a column "visible at the Strategy camera: yes / no / silhouette only" to the gain tables; restore C3, T7 and S2
into G0b or G0c.

**4. [medium] Other world gains that part one ranked were dropped without a note.**
Passage: section 4 (L404 to L489) against world sections 2 to 9.
Dropped: B3 (baked cavity on the house), B6 (a lit interior worth looking into), T6 (footprints and trodden grass,
which reads real bodies and fits the "no fake" rule well), V5 (moss and soil at the base), P6 (pot plants), X6 (weather),
U5 (scaling rules for 1080p/1440p/4K), C4. Cloth sheen (surf 3.2 G) is named in 3.5 but has no G number.
Fix: either list them in a "kept for later" line with the reason, or add them; say once that omissions are deliberate.

**5. [medium] The modelling file's own first-ranked gain has no place in the programme.**
Passage: G11 gives landmarks only as a still (L444). Mod 13 ranks "anatomy landmarks in the form" first (S to M, "ends the
tubes; no new topology") and says steps 1 and 4 need no new topology.
Wrong: there is no in-game gain for landmarks on today's topology, and no baseline range-of-motion sweep of today's
miners as an instrument (mod step 4, "easy"). The sweep appears only inside G26 and the spike bar (L549), which wants
"measured on both".
Fix: add a Tier 0 item "range-of-motion sweep of today's three miners" (it is the control for the spike and for G26) and
a Tier 1 or 2 item "landmarks in the form, as a version", or say why not.

**6. [medium] Nothing says what the step-0 stills lack, and the depth choice is confounded.**
Passage: 5.1 (L495 to L522).
Wrong: the third-ranked gap, flat surfaces, and the fourth, light on the miners, are absent from the stills by
construction: G16 waits for Luis's choice, shader gains are "judged on Unity captures, not on step 0's sheets" (L522).
Luis picks the body depth from clean, flat, unlit bodies, the weakest-looking things in the game. A fuller body may look
worse or better once cloth has wear and the rim is warm. The file flags that Blender is not Unity's look, but not that
the missing surfaces confound the choice.
Fix: add one extra sheet: the chosen candidates with a mock of painted layers (a few placed patches, hem wear) or one
Unity capture with G1 on, so the depth choice is not made on the flattest rendering.

---

## B. Unsupported claims, hypotheses stated as fact, misquoted numbers

**7. [medium] "About 52 percent of the painted face's resolution" is a linear ratio of one miner, called a cause.**
Passage L113 and L157: "the face gets about 52 percent of the painted face's resolution"; "the real cause of the soft
face is the atlas share, about 52 percent".
Wrong: faces 2.4 item 3 derives it from eye distance: 303 px in `Face_Small.png`, about 159 px in the atlas (159/303 =
0.52). That is a linear scale; by area it is about 27 percent. It is "measured, rough", from Small only, from eye
positions in the code and a picture. Calling it "the real cause" is a hypothesis; the BC7 question is also unchecked, and
inventory 1.5 says the source face is 1024. Long and Round were not measured.
Fix: write "about half the linear resolution (about a quarter by area), Small only, rough"; mark cause as hypothesis; add
Long and Round to G0c.

**8. [medium] The heads-tall numbers are shown without their weakness and hide an inconsistency.**
Passage L114: "about 4.5, 5.8 and 4.1 heads tall with hair".
Wrong: faces 3.1 gives these "read from silhouettes by eye, rough". Its second column (skull only) is 4.9, 6.5 and 5.5:
Round's two figures differ by a third, which does not match the other two (0.4 and 0.7 apart). Nothing in research.md
says a check is needed, and the head sheet H (G12) uses these as the baseline for "-8, 0, +8, +15 percent".
Fix: give both columns, mark "by eye", and add a measured head-height-from-mesh item to G0c before H is drawn.

**9. [medium] "Hair is decimated tube locks, not a shell" corrects too far.**
Passage L46, L158.
Wrong: faces 6.1 says the locks go over a skull and "a cap closes the gaps"; curls are metaball clumps. So hair is locks
plus a cap plus metaball curls, and the inventory's "cap" (M9) was partly right. The fix "exempt from the global
decimation" (G3) raises the triangle count of every miner; no budget is given, and LOD levels are fixed decimations.
Fix: say "locks plus cap plus metaball curls"; give G3 a triangle budget and a LOD rule before it is called S.

**10. [medium] The ranking claim "ten small gains with large effect" is not supported for all ten.**
Passage L53 to L55 and the Tier 1 heading (L425).
Wrong: "large effect" comes from part one's hypotheses and ranks. Surf puts G7 (image precision) at rank 7, vertex AO at 6.
World puts the prop pass at rank 6. G4 is conditional on a swim that has never been seen. G9 bakes AO in the rest pose;
it will not follow a bend. Sizes S are all hypotheses.
Fix: call Tier 1 "cheap gains whose effect is mostly hypothesis", mark G4 as conditional in the heading, and note G9's
rest-pose limit.

**11. [medium] "Compressed HQ, likely BC7" is stated as the corrected inventory in the short version.**
Passage L44 to L45: "the miner textures are already 'Compressed HQ' (likely BC7), not DXT1".
Wrong: surf 1.5 says the Standalone override entry is set to 1 with `overridden` 0, so the default is used. That is a
reading of a `.meta` file, not of the editor, and the file says so in 2.3 and 9. The short version drops the hedge for
the first half of the sentence.
Fix: "set to Compressed HQ in the import file; the editor was not asked".

**12. [low] The LOD triangle figures are partly mislabelled.**
Passage L112: "16,000 / 5,000 / 1,600 triangles".
Wrong: mod 2 says the 1,600 is the hand budget at LOD0, and mod 8 lists "16,000, 5,000 and 1,600" as the LOD triangles.
The two readings are both in mod. Research.md picks one with no note.
Fix: confirm in G0c ("triangles per part per LOD") and mark it.

**13. [low] "Sessions throughout" hides a scale conversion that was not shown.**
Passage L30 to L32.
Wrong: part one used "S under a day, M a few days, L a week or more" (faces, inv) and "S a session or less" (mod). The
sizes in section 4 look copied across, not converted. One session is a five-hour usage window; "a few days" may be
three to six of them. The totals in 5.2 depend on it.
Fix: state the mapping used (for example S = 1, M = 3, L = 7 sessions) or show the totals as a range from both scales.

---

## C. Standing rules and Open questions

**14. [high] Gains that need Luis's ruling are scheduled before the ruling, and their rows say "none".**
Passage: G1 (L429), G7 (L435), G20 (L453), G6 (L434): "More than judges and Luis: none". Q6 in section 8 (L652 to L653)
asks whether Claude may explore "the warm rim and bounce, the depth-aware paint radius, hour-keyed grading, depth of
field, MSAA".
Wrong: the same research asks permission for these in section 8 and schedules G1, G7 (with MSAA) in Phase D without it.
Either the standing rule (explorations are additive and switchable) is enough, in which case Q6 is unnecessary, or it
is not, in which case Phase D waits.
Fix: decide which, in the text. If a ruling is needed, move the screen-wide items out of Phase D and mark the "More than"
column "ruling".

**15. [medium] Twist and helper bones are treated as ruling-free; correctives are not.**
Passage G25 (L463) "none", G26 (L464) "ruling (light)"; Q5 (L650) lumps both.
Wrong: mod 5.2 and 5.3 describe the same mechanism: a script sets new bones or shape weights from joint angles each frame.
If one needs the "no fake animations" ruling, so does the other. The table contradicts Q5.
Fix: give G25 the same ruling entry; keep one consistent answer.

**16. [medium] The closed body under the clothes appears as "the best method" while it is Open.**
Passage L50 to L51 (short version 4) and L199 to L202.
Wrong: the file says it "replaces the cut-away practice only with Luis's word", and MR Q3 and Q7 are Open, but the
method list of the short version presents it as part of the best method with no marker. A reader taking the short
version alone would treat it as decided. Also vents and pose-driven shapes are Open but shaped into section 3.1's method.
Fix: add "(Open: MR Q3, Q7)" to that line of the short version, and mark vents and pose-driven shapes the same way.

**17. [medium] "Landmarks at today's mean radii leave the physics radii alone" contradicts "one body for everything".**
Passage L175 to L176 against L516 to L517.
Wrong: 3.1 says the form comes "from the same presets the physics reads, with anatomy landmarks added" and claims this
keeps the drawn and physical body as one. 5.1 then says the landmarks sheet "leaves the physics radii alone" (hypothesis).
Landmarks (deltoid, calf, seat) bulge beyond the capsules; the fuller body (thicker thighs, belly) changes mass and
reach. If the drawn body stops matching the colliders, the memory rule "one body for everything" and the get-up for any
body are at risk. The file never says how big a mismatch is allowed.
Fix: state a tolerance (drawn surface to nearest capsule), say where the landmarks live (a render-only layer vs the preset),
and put it in the spike and in the physics-number table (L579).

**18. [medium] Open and ruling-dependent items are bundled into dependencies without a stop.**
Passage Phase F (L537): "A1 built on the template if the spike's junctions pass and his depth goes past static art".
Wrong: it is a good rule, but G28, G27 and A2 inherit "Open: vents", "ruling" in their own rows only; Phase F lists "A2
(G28) behind a version gate" with no word that vents or the cut-away must be answered first, and the any-body get-up
check is mentioned for G40 only.
Fix: Phase F should list which Open answers each step needs, and G25 to G29 should name "the get-up on all three miners"
as a gate, not only G40.

**19. [low] "Downloads: none is needed" sits beside two possible approvals.**
Passage Q7 (L654) and L61 to L67.
Wrong: "none is needed" is right for the programme as written, but the text also calls a font "the biggest lever" and a CC0
mesh "would help". The summary should say "none needed; two optional, both Luis's".
Fix: reword. Also say in 3.2 that libraries in use (Pillow, NumPy, SciPy, OpenCV; inventory 3.0) cover the code-painting.

---

## D. Risks played down

**20. [high] Cost: one GPU number stands in for the whole budget.**
Passage L165 to L166, L580: "4.5 to 5.7 ms GPU at 1080p on the dev laptop, 7.0 ms with 100 miners" and "room to spend".
Wrong: the figure is GPU only, from one laptop, with no CPU number. The programme adds per-frame C# for helper bones,
correctives, hair springs, garment chains with leg capsules and the foot-shadow arrays. All of it multiplies by miner
count, and the RTS wants many miners; the articulation-body physics is already the CPU cost. On the GPU side: 64-bit HDR
doubles the colour buffer bandwidth, MSAA 4x on it multiplies that, plus more samples under the Kuwahara pass; extra
local lights in the beings' shader; hair exemption raises triangles; blend shapes add vertex work at LOD0 for every miner.
The measure-each-switch rule (L580) is good, but there is no cap and no running-sum rule that stops the programme.
Fix: add a CPU measurement to G0b, a per-crowd test (100 miners with everything on), and a written ceiling that is
Luis's call; make "stop if over the ceiling" part of the order.

**21. [high] The "version" switch multiplies and its combinations are not counted.**
Passage L413: "version = a second miner set (`Miner_<Name>_v2` files with new GUIDs)". G2, G3, G8, G25, G26, G27, G28,
G29 and G31 all say "version"; Phase F also adds "the chosen faces as a face variant" (L537).
Wrong: nine different gains each as their own version, plus a face variant, plus keywords, over three miners, is a test
matrix nobody has counted. Which versions combine (face v2 + body A1 v2 + hair v2) is not stated. Each re-export carries
the "shared Blender modules" risk (L617), and the golden check is only for the first version. Test cost for the physics
table (L579) grows with each.
Fix: define a small ladder (for example v2 = faces and hair only; v3 = body A1; v4 = garments) and state that gains
within a rung ship as one export; add the matrix size to the work estimate.

**22. [medium] The "idle at 14 and 4" is protected only against regression, not against new idle motion.**
Passage L609, L579, L267 to L270.
Wrong: hair springs (G31), helper bones and correctives all animate at rest. Luis loves the idle and the breath is "small
and grows with the situation" (memory). The detectors (L591) say "no motion at rest beyond the breath" for hair but there
is no breath-scaled bound, and no check that corrective weights at rest are exactly zero.
Fix: add an "idle unchanged" detector per gain that adds a moving part: hair, helpers, correctives, garment chains.

**23. [medium] Skeleton changes versus the articulation builder.**
Passage L195 to L196, L626: "render-only bones (helpers, hair) never in the articulation".
Wrong: no one read the code that builds the articulation from the FBX bones. If it walks all child bones, new bones are
swept in; the hint is a hypothesis. The file's own rule "set after, read back" (memory: articulation turns after links)
points to where this fails.
Fix: add to G0c: "does the articulation builder ignore bones it does not name?" with a code read.

**24. [medium] The scope of the work is stated, but not its real size.**
Passage L540 to L542: "about 9 to 15 sessions until his next two sheets"; "G. The rest ... many; not counted" (L538).
Wrong: the programme has 44 entries (G0a to G41), with at least six L-size items (G24 whole, G28, G37 APV, G38, G39, G40)
and many M. Phase F alone is 4 to 6 + 4 to 6 + 5 to 8 sessions. Judge work is not costed at all: blind pairwise judges, a
referee, three miners, many sheets (body three ways, landmarks, F0, F1, H, game-camera crops, three hours of light).
At about seven sessions a week the total is plausibly months, against Luis also wanting the getting-up work (the scope
risk at L632 is named but not sized).
Fix: add a total (range) for the whole programme, a count of judge agents per sheet, and a "cut line": the tier at which
Luis may stop and still have a coherent result.

**25. [medium] Protected workspace: no mention of how the programme avoids touching human work.**
Passage 5.2 and section 7.
Wrong: AGENTS.md says to inspect and protect uncommitted human state; the working tree has modified scene and project
files. G0b, G13, G15, G17, G18 and G19 edit scenes, scene setup, a shared shader (`WGPainted`) and the ground shader.
Research.md says "the worktree" once (L409) but gives no rule for where code and scene edits happen.
Fix: add a rule: edit in the validation worktree or a clean clone; use Unity authoring APIs for scenes; keep GUIDs; a
`shader_feature_local` change to `WGPainted` is reviewed as shared code (it touches every material).

**26. [low] Judges' limits are noted, but their calibration is not.**
Passage L576 and L630.
Wrong: "Judges spot faults better than beauty, and may prefer a generic face" is acknowledged, yet the guard is "pairwise,
references beside, Luis decides". For faces Luis already likes, a judge that prefers a generic face would bias F1 against
the loved one.
Fix: add a calibration step: include today's face as a known-good control in every F1 pairing, and report a judge's
result only if it picks today's face over a degraded copy.

**27. [low] Hair springs and flutter fight the paint filter and the outline in motion; not listed.**
Passage L267 to L270, L626.
Wrong: moving thin strands and the 8-sided tubes pass through the outline hull and Kuwahara pass; the file raises ink
crawl as a hypothesis (L317) but the hair risk list has only physical coupling.
Fix: add "outline crawl on moving hair" to the risk table and the detectors.

---

## E. Order of work and structure

**28. [medium] The order does not fully follow from the ranking.**
Passage 2.2 against 5.2.
Wrong: the largest gap (bodies and cloth) comes only as stills; faces, hair and shader gains come first in Unity. That is
defensible (cheap, additive), but the file never says that this puts small gains before the biggest, nor how long the body
work then waits (Luis's pick, then the spike results, then A1 at 4 to 6 sessions, A2 at 4 to 6). Meanwhile the third and
fourth gaps (surface, warm light) are in two different phases (G16 after Luis's choice, G1 in D).
Fix: add a line to 5.2 that states the trade and the date at which a first improved body could be in the game, and
consider moving the spike (G24, riskiest) ahead of the world's first pass in Phase E, since the world gains are lowest in
the gap ranking.

**29. [medium] The reconcile with the models research rests on a partly read document.**
Passage L674 to L676: "the models research only in its Start here, sections 1, 5 and 7 to 9".
Wrong: sections 2 to 4 hold the measured faults, the audit and the external sources, and the central choice (step 0 with
the current pipeline, not the template) assumes they do not contradict it. Not a fault of honesty (it is stated), but a
gap that matters to the biggest decision.
Fix: before the revision is final, have someone read MR 2 to 4 and `critique.md` of the models research for any
contradiction with 5.1.

**30. [low] Count and labels.**
"41 gains in five tiers" (L53): the tables hold G0a to G0c plus G1 to G41: 44 entries; G41 itself bundles four options and
G37 bundles three. Tier 0 is called a tier but is instruments.
Fix: "44 entries (3 instruments, 41 gains)", and split the bundled rows.

**31. [low] Section 2.3 closes with a claim of no contradictions that is not quite true.**
Passage L165: "No file contradicts another on the shading model, the cameras, the capture tools or the headroom".
Wrong: the inventory says the face texture is 1024 painted and baked into 2048, where faces measures eye distance 303 px;
world line 221 says "far mountains" where line 108 says hills to 1500 m. These are small. More important, the file lists
the contradictions it found and does not say how complete the comparison was.
Fix: soften to "none found in these four areas".

**32. [low] Evidence marks.**
The marks (measured, code, picture, source, record, hypothesis) are used well, but several entries carry two or three
at once ("code; source; hypothesis", L197, L267) with no way to tell which half is which. A few "(source, search summary)"
items drive method choices (Guilty Gear Xrd normals, glTF sheen, TF2 rim) with weak authority; part one tagged them
"search summary only".
Fix: for each method in section 3 name the weakest link in one clause (for example "the method rests on a search summary").

---

## What this critique did not do

- It did not open any picture, texture, mesh, code file or Unity/Blender. All checks are text against text.
- It did not read the models research (`2026-11-14_TheModelsRedrawn`) at all, so points 17, 28 and 29 are about what
  research.md says it read.
- It did not re-check every number in the five files; it sampled: 52 percent, heads-tall, BC7, LOD triangles, hair, the
  world's clouds and hills, the dropped world gain IDs, the ranked lists of surf, faces, world and mod, the tool
  budgets (4.5 to 5.7 ms, 7.0 ms), and the standing rules.
- It did not run a web search.
- It did not rank the points against each other beyond severity.
