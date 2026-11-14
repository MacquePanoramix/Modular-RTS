# External research: the three miners redrawn

Date: November 14, 2026. Scope: outside sources only. The project's own sources are in `internal.md`, the audit in `audit.md`.

How to read this file:
- Every claim carries a source tag like [S12]. The source list at the end gives title, URL and how I read it.
- Reading status per source: **read** = I fetched the page and read what the fetch returned; **read (PDF text)** = a PDF I extracted to text and grepped; **snippet** = I only saw a search-result summary, not the page, so treat it as a lead; **weak** = low-quality or unverified, shown only so nobody re-finds and trusts it.
- Sources are summarised in my own words. "Inference" marks what I conclude for this project; it is never a sourced fact.
- Search queries were generic topic queries only.

Short version for the reader in a hurry:
1. Natural-looking stylisation keeps the structure of the body (ribcage and pelvis as two masses, the waist as the gap between them) and exaggerates by purpose. Every measured anatomical or art-historical waist-to-hip value I found sits at 0.5 or above; the audit's 0.30 to 0.37 waist-to-hip breadth ratios for Small and Round are well outside it. The strongest fix is in the garment, not the body: real tailoring gives the coat a waist, but not a corseted one.
2. Shipped games do not "solve" legs through skirts; they combine (a) tight or separate legwear under loose cloth, (b) leg-following bones plus leg capsules, (c) hiding or simplifying what is under the garment, and (d) accepting some clipping. No source gives a runtime cost number I trust; the honest answer is "profile".
3. Creators that stay pretty either lock proportions inside presets or keep sliders few and bounded. The one creator that changed a physically simulated body (a research system, not a game) needed a controller trained across the range, and failed outside it. Spore, the shipped example, was kinematic, not physical.
4. For a simulated body, mass and strength must be re-derived when proportions change (cross-section against volume), and the physics colliders must follow the drawn body. The physics is stable with mass ratios that rigid joints cannot take, so the design risk is the controller, not the solver.

---

## 1. Pretty and still natural: proportions and shape language

### 1.1 What natural proportions are (the baseline)

- Head-count canons: an average adult is about 7.5 heads tall; the classical and Renaissance ideal is 8; heroic figures are drawn at about 8.5, with the extra height in chest and legs [S1]. Legs from floor to crotch are 3.5 to 4 heads; the body's midpoint falls at the hip joints (greater trochanters) [S1].
- Hip-to-shoulder (circumference basis): women about 1 : 1.03, men about 1 : 1.18 [S1]. So an adult female's hips and shoulders are nearly equal in breadth.
- Waist-to-hip ratio (WHR) by circumference: values across 2,500 years of European art run from 0.67 to 0.80 (Venus de Milo 0.76); Paleolithic figurines are about 1 or higher [S1]. Another survey: Greek and Egyptian sculpture averages about 0.7, Indian artwork about 0.6, African artwork about 0.5 [S2]. Health thresholds put abdominal obesity above 0.85 for women and above 0.90 for men [S2].
- Caveat on those numbers: WHR is a circumference ratio and includes the depth of the body, so it is not directly the same as the front-view width ratio the audit measured. Inference: a front-view width ratio of 0.30 to 0.37 (waist to hip) would still be far below the 0.5 that even the most waist-emphasising art averages.
- A classical canon in head units: one shoulder is 1 head wide (so about 2 heads across the shoulders), the ribcage and the waist are both 4/3 head wide, male hips are 1.5 heads, the pelvis is 1 head tall [S3]. From this canon, waist-to-shoulder is about 0.67 and waist-to-male-hip about 0.89. The author warns the outlines are oversimplified and the numbers are for proportion, not for rulers; other historical canons differ [S3].
- Female against male: the shoulder-to-hip relationship is the main difference between them; the waist lies between and is only a little defined in male figures [S4][S5]. For a female figure hips are often only slightly wider than the shoulders; for a male figure the waist is only slightly narrower than the hips [S4]. Differences are mainly horizontal, and the female waist reads narrow largely by contrast with the hip line rather than by being small in itself [S5].
- Drawing instruction treats the torso as two masses, ribcage and pelvis, meeting at the thin part of the waist, with the masses overlapping in the construction [S6, snippet]. The ribcage and pelvis are about equal in breadth on a male and the pelvis is the wider on a female [S6, snippet]. Inference: the waist is the gap between two masses, so a waist narrower than both the masses by a large factor would need a body that has lost the ribcage's lower edge, which is why it reads as a corset.

### 1.2 How acclaimed stylisation behaves

- Disney's principles: exaggeration works when the action or form stays true to reality and is pushed further, with restraint and balance among exaggerated elements so the viewer is not confused [S7]. "Solid drawing" asks for forms with mass, balance and shadow, built on anatomy knowledge; and it warns against "twins", figures whose two sides mirror and so look lifeless [S7]. Appeal is described as charisma: the character must be believable and engaging; simple, symmetrical, baby-like faces read as likeable and complicated faces weaken appeal [S7].
- Pixar (The Incredibles rebuild): the team studied muscles and structures beneath the skin and used anatomy to decide where geometry detail goes; the simplified base mesh kept the major shapes, mass and design lines that carry the character's identity; detail was spent where audiences look (face, hands) and reduced where natural creases hide imperfections (elbow, under the chin) [S9]. Pixar's own "Articulating Appeal" paper, by search summary only, states that body contours were kept simple and direct so stylised designs hold up in extreme action [S10, snippet, not read directly].
- Uncanny-valley findings relevant to stylisation: the dip lies near realism; clearly non-human or neotenous cartoon designs sit on the safe side; but stylisation is not a guarantee, and an appearance that is more human than its movement gives a negative impression, as does the reverse (mechanical thing moving human-like) [S8]. Inference: our bodies move by real physics, so the drawn body must not promise more realism than the motion (soft cloth, anatomy) delivers, nor less. The three miners' hard-edged flap boards are the "mechanical" side of that mismatch.
- Shape language: sources agree silhouette and shape give the first read, and exaggeration needs a purpose [S11, snippet, generic character-design articles; low authority]. One generic source claims a wide-shoulder, narrow-waist silhouette reads as heroism and physical power [S11, snippet, weak]. Treat as folklore: it is the least useful rule for a game about ordinary workers, and Luis's stated taste is for the ordinary warm moment.
- Games that rely on silhouette: Blizzard's Overwatch team pushed each character's traits further so any hero is identifiable at a glance, and used one character as the feel test for the whole style [S12, snippet]. Rare's Sea of Thieves chose style over realism with a deliberately slightly wonky, lived-in painted look [S13, snippet]. The Breath of the Wild art director described the style as an evolution of a toon style chosen so it was easy to "lie" in the design [S14, snippet]. Inference: the common thread is that stylisation is decided per character from a few strong shapes and then kept consistent, not derived from a thin-waist formula.
- Real tailoring as a source of the waist (important for a coat): a frock coat is built with a horizontal seam at the waist and side panels that pull the cloth in, so it is narrower at the waist than the chest and reads as an hourglass; the skirt below it flares out [S15]. Coats cut without a waist seam hang looser [S15]. Inference: a tailored waist belongs to the coat's cut. The shipped miners narrow the top by a tuck (internal.md: 0.82 of width) and then fit the skirt to the narrowed top; the tailoring precedent says the skirt should flare from the waist, which restores the hip mass.

### 1.3 What this gives the redraw

(Inference, to be tested against the audit's pictures, not decided.)
- Treat waist-to-hip and waist-to-chest width ratios as audit numbers with a natural-range target, not as a taste knob. Natural-range anchors from sources: waist about 0.89 of male hip width in the canon [S3]; female WHR 0.67 to 0.80 in European art [S1] and about 0.5 at the extreme of stylised art [S2].
- A heavy character's waist should be near or wider than the hips by the health thresholds [S2] (WHR above 0.85 for women, above about 0.9 for men). That agrees with Luis's wish that Round read "round and heavy".
- Keep the structure (ribcage, narrow gap, pelvis) and let the garment decide how much of the gap shows [S6][S15].

---

## 2. Garments over legs in motion

### 2.1 Why legs poke through, and the classic answers

- Legs clipping through a skirt are a long-standing, hard problem, especially with varied or motion-captured animation. The traditional workaround is tight-fitting legwear that does not need to move freely [S16 Lander, 2000]. Bone-based deformation of loose clothing is effective but not necessarily realistic [S16].
- Real-time cloth with legs uses collision spheres or capsules approximating the limbs, resolving a penetration along the collider's surface normal so cloth slides off [S16][S17]. Limits: only cloth vertices are tested, so thin or stretched cloth between vertices can pass through; self collision is costly or skipped [S16][S17].
- Unity's built-in cloth accepts only capsule colliders and pairs of spheres (a cone between two spheres is described as suited to limbs); the restriction is for performance; it works only on a skinned mesh; cloth reacts to the colliders but does not push them back [S17]. Self collision and inter-collision are described as taking a significant share of simulation time; the advice is to keep collision particles and distances small [S17].
- Unreal's clothing tool ties cloth to the character's physics asset for collision, and limits each point by a painted maximum distance from its animated position, with a backstop to hold points off the body [S18]. A forum claim says backstop is much cheaper than collision capsules [S19, snippet, unverified user claim]. Epic's page gives no cost numbers [S18].

### 2.2 Bone-driven skirts that follow the legs

- Spring-bone chains: each chain is a list of joints where each simulates the head swinging about its parent; colliders are spheres or capsules grouped and assigned per chain; each step the tail moves by inertia, stiffness and gravity, collision pushes it out, and the bone length is restored [S20, VRM spring-bone spec]. The spec does not discuss skirts.
- Unreal KawaiiPhysics skirt tutorial: use several chains around the skirt, tie neighbouring chains together with constraints so they do not drift apart, and cover each leg with several capsules (thigh capsule plus knee-to-calf capsule); a "sync bones" feature copies a thigh bone's motion onto skirt bones so the skirt rises and follows leg motion; lower stiffness and collision radius toward the hem for a softer hem; pleated skirts want higher stiffness and a small angle limit, flared ones lower stiffness and a larger limit; to cut cost, reduce constraint iterations and the number of collisions [S21].
- VRChat PhysBone guidance: clipping comes from missing, too small or unassigned leg colliders; capsules suit legs; test crouching and jumping, not only walking; flipping comes from too much movement and from weights concentrated on one bone, so weights must be spread; angle limits of about 30 to 60 degrees help [S22].
- VRM 1.0 "inside colliders": the chain is kept inside a collider volume so the skirt hangs outside the legs while swaying more freely with fewer chains. A worked setup used 8 chains of 5 bones (front, back and both sides, half parented to each upper leg), capsules on both upper and lower legs, four inside capsules; weights near chain boundaries hand-painted at about 0.35 to 0.4; remaining faults: the hem cannot flip downward so the inside of the skirt shows when sitting, weights from four directions meeting can spike outward, left-right asymmetry appeared, and the author calls it acceptable for walking and running, not perfect [S23].
- Forum practice for coats: parent flap bones to the thigh (or constrain them to it) so the flap follows the leg exactly with no simulation; or separate lagging bones for secondary motion; the trade-off is exact tracking against lag [S24, snippet; Blender Artists thread]. A cape/hair thread says corrective shapes in games are usually driven by preset animation data and are often too costly [S24, snippet].
- Overlap and drag: loose parts lag, then settle; these apply to clothing; the torso is the core and soft parts move more independently than bony ones [S7]. Inference: the animation principle supports lag in the hem and none at the waist.

### 2.3 Hide, replace or cover what is under the garment

- Hide the body under clothing: the Space Station 3D art guide's remedy is to hide body parts beneath clothing and end clothing at fixed cutting lines (wrist, below shoulder, just above knee, below ankle) so clipping is never visible; it also models the inside of sleeve ends so a severed-looking limb still reads natural [S25].
- A developer thread on whether games hide the player model under clothing: clipping is hard to avoid even in large games (a seated character's legs pass through a skirt in a shipped open-world game); fixes are weight matching per layer (tedious across poses), modular body parts hidden by flags (works only where the hidden part cannot be seen through the garment), material slots swapped to invisible, or per-garment skinned meshes over a base body in underwear [S26].
- Modding and artist forums: make the skirt slightly larger than the legs so the two do not run close; avoid skirts tight over legs because skirt and legs deform differently; a plane or panel covering the groin area is more reliable than cutting away the body, since seeing nothing above the knee breaks immersion [S27, snippet].
- Inference tied to the project: the roadmap's principle that every module fits any body argues against cut-away garments, and the shipped Long miner (whole trousers) is exactly the "legwear under loose cloth" answer that Lander's article and the forums name as the safest [S16][S27].

### 2.4 Costs at runtime

- I found no trustworthy benchmark for skirt chains, capsule counts or cloth meshes. What the sources do say:
  - Unity cloth: self collision and inter-collision are the expensive parts; the page says they can take a significant share [S17].
  - KawaiiPhysics: cost is cut with fewer constraint iterations and fewer collisions [S21].
  - Unreal: a forum user claims backstop is far cheaper than capsules [S19, snippet, unverified]; another thread calls CPU budget a matter of substeps, backstop and anim-drive [S19, snippet].
  - Magica Cloth (Unity asset): a delayed-execution mode and trimming per-garment colliders to the body parts they touch are suggested; a single user's profiler numbers are anecdotal [S28, snippet].
  - Unity skinning: the weights per vertex are limited by project quality settings (one, two, four, or auto); more than four costs more [S29, snippet].
- Inference: bone-chain skirts with capsules driven by the existing leg bodies cost less than a cloth mesh, because there are no per-vertex collision tests. The project already has physical bodies for the legs; reusing their poses to drive flaps (Sync-bones style) needs no new simulation. This must be measured; no source gives the number.

---

## 3. Character creators with few but meaningful options

- Curated parts beat a long list: the Mii team cut many facial parts to avoid overwhelming users while keeping variety, and avoided strict realism because a caricature that captures character is satisfying [S30]. Other Nintendo interviews (search summaries) say adjustable sizes of a few parts gave more variation than adding parts, and that Miyamoto said a large number of options is not needed [S31, snippet].
- Presets over sliders for the hard-to-make-pretty parts: Baldur's Gate 3's creator uses presets for facial features and four body types not tied to gender, with no direct height or body-type sliders; the team's reasons were that millimetre changes can make or break a face, that slider-heavy creators produce blank, similar faces, and that every character in a very large cast had to look distinct [S32][S33]. The cost: players and critics find it limiting, and the developers accepted that [S32].
- A generic design guide lists the failure modes as too many options, ugly results and choices that do not feel meaningful [S34, snippet, vendor-style]. A community wiki says some foundational proportions are set by the preset and cannot be fully overridden by sliders, which keeps combinations valid [S34, snippet, weak].
- Modules that must fit every body: Spore's runtime solved the animation side by recording motion in a morphology-independent form and solving poses with inverse kinematics on whatever body the player built; about 90 percent of tested animation and creature pairs passed in testing, failures were sorted into bugs, art-generalisation faults and missing features, and the system had no collision between a creature's own parts [S35 PDF text]. It was not physically simulated: secondary motion was a separate damped "jiggle" pass that does not feed back [S35 PDF text]. Inference: Spore shows that testing a large random sample of creator output against every action finds failures early; it does not show that a physical body survives arbitrary proportions.
- A creator that does change a physically simulated body: Won and Lee trained one controller over body-shape parameters (global scale 0.5 to 1.5, per-segment scale 0.7 to 1.2, so segments span about 0.35 to 1.8 times the base). They scaled joint torques according to total mass and adjusted the reward by the size ratio, citing Hodgins and Pollard's geometric scaling of simulation parameters [S36 PDF text][S37, snippet]. Failures: some shapes could not be walked stably by the parametric controller although a controller trained for that shape alone could; shapes outside the training range eventually fail; so the designer must define the allowed shape range and train over it [S36 PDF text].
- Inference for "not too many, but some": presets for the three miners plus a small number of bounded choices (for example a build/weight choice, a height choice, a garment choice) keep every combination within a tested range. Because strength and getting up are body-driven in this project, each choice must map to a range in which the physics has been verified, as Won and Lee had to.

---

## 4. What a stylised simulated body needs from its model and rig

### 4.1 Strength, mass and size

- Muscle strength follows cross-sectional area (square of size) and mass follows volume (cube); so relative strength falls as bodies scale up, and larger bodies need disproportionately thicker structure [S38 Square-cube law]. This is the physical reason for Luis's "strength from muscle, slower with size" rule; the source supports the direction, not a particular exponent for a game.
- Hodgins and Pollard's classic approach: first rescale control parameters by the differences in size, mass and moment of inertia between the original and the new character, then fine-tune a subset by search [S37, snippet].
- Inference: if the redraw changes a limb radius or a head or body segment size, the mass, inertia and joint torque limits derived from it change and have to be recomputed by the same rule, or the miner gets stronger or weaker by accident.

### 4.2 Physics bodies against the drawn body

- Unity's articulation anchors default to the local zero of each body and are not a good default when colliders are attached, since the joint may try to push the bodies apart; the manual offers a "snap anchor to closest contact" helper [S39]. Drive force is a spring with stiffness, damping and a force limit; locking both swing axes of a spherical joint to mimic a hinge is slower and behaves differently than a revolute joint [S39].
- PhysX articulations have zero joint error by design and handle larger mass ratios between jointed bodies than ordinary rigid-body joints do; they can lose stability at high angular velocity, where velocity clamps or damping are suggested; acceleration-type drives make behaviour less dependent on mass and inertia [S40]. Inference: heavier or lighter limbs from a redraw are less a solver risk than a tuning risk for the controllers.
- General ragdoll guidance (lower authority): collision shapes are usually kept a little smaller than the visible mesh to avoid interpenetration; overlapping shapes on adjacent bodies can cause force spikes at start; a narrow hip collider under a wide mesh makes the character jitter against the floor [S41, snippet; the last point anecdotal]. Inference: the audit's finding that bodyFront is measured on the clothed model means that any garment change to the hip area moves the hip collider; the colliders should be measured on the body that is meant to touch, not on the garment.

### 4.3 Skinning and joint placement for good deformation

- Linear blend skinning suffers joint collapse (candy-wrapper), volume loss and self-intersection at large rotations; dual quaternion blending removes the collapse at small extra cost but has its own faults (bulging when bending, a separate pass for scale and shear, flipping), and Disney's production note says it is not a drop-in replacement [S42, snippet]. Whether Unity 6000.6 offers dual quaternion skinning I did not verify; do not assume it.
- Wikipedia says linear blend skinning does not give realistic muscle and skin motion and that fixes attach special controllers to bones; bone placement need not follow anatomy [S43].
- Topology advice from forums and tool tips (lower authority): at least three loops of faces around each joint, loops closer on the pinching side of an elbow or knee than on the stretching side, poles kept off the hinge line, extra loops for the shoulder and clavicle [S44, snippet]; Pixar's rebuild put loops where the anatomy puts them rather than where they fit [S9].
- Per-vertex bone count is limited by the project quality setting (one, two, four or auto) and above four costs more [S29, snippet]; weights concentrated on one bone make skirts fold sharply, so spread skirt weights across neighbours [S22].
- Inference: the audit's finding of "thin tubes with no elbows or kneecaps" matches the guidance that joints need loops and some volume to bend well; adding knee and elbow volume changes the visual radius but should not change the physics radius if the colliders are separate.

### 4.4 When changing proportions needs the physics retuned

(Mostly inference, anchored in 4.1 to 4.3.)
- Visual-only change (surface, cloth, waist shaping, trousers under the coat) that does not change any collider, mass or joint anchor: no retune, but verify cloth mass is not counted and that the hip collider is not measured from the new garment.
- Changes to a segment's length or thickness, head size or limb radius: recompute mass, inertia and torque limits with the rule in 4.1, and re-verify balance and getting up, because Won and Lee found some shapes cannot be controlled even by a controller trained over a range [S36 PDF text].
- Changes to a joint position: move the anchors with it; the hip and knee joints define stride and rise, so they are the most sensitive.

---

## 5. What this means for the three miners (hypotheses, not decisions)

1. Waist. Target ranges from sources: the canon waist is about 0.67 of shoulder width and about 0.89 of male hip width [S3]; a female WHR of 0.67 to 0.80 is the European art range [S1]; a heavy build is near or above 0.85 [S2]. The audit's visible waist-to-hip breadth of 0.37 (Small) and 0.30 (Round) is out of range; Small and Long at about half of shoulder width are outside the canon's 0.67. Fix the garment cut first: a waist seam with a flare below, as in real coats [S15].
2. Floating legs. The proven pattern is legwear under loose cloth (Long's whole trousers) [S16][S27]; extend the trousers up under the coat for Small and Round. Cut-away is the documented alternative but conflicts with the project's every-module-fits-any-body rule [S25][S26].
3. Skirt clipping. Replace rigid flap boards with flaps whose bones follow the thigh (parent or sync bones) and add a lag on the hem only, keep capsules around the thigh and shin with a margin, and keep the garment slightly larger than the legs [S21][S22][S27]. Spread the weights [S22]. Inside-collider style containment is an option with known residual faults [S23].
4. Cost. Nothing in the sources lets me state a cost; the bone-chain route is expected to be the cheapest, and the Unity cloth route has the documented self collision cost [S17]. Measure.
5. Creator. A few bounded choices on top of the three presets, each mapped to a verified range [S30][S32][S36]. Do not use free sliders for body shape if getting up must work for every combination.
6. Physics. A visual-only redraw leaves the physics alone; anything touching radii or masses is recomputed by cross-section and volume [S38][S37].

---

## 6. What I did not do

- I did not read the Pixar "Articulating Appeal" paper itself: its URL redirected to the Pixar libraries index; the points attributed to it come from a search summary [S10]. The Autodesk interview on The Incredibles was read [S9].
- I did not read the full text of the Hodgins and Pollard paper, the dual quaternion paper, or the Won and Lee paper's figures. I read the Won and Lee and Hecker et al. PDFs as extracted text (grep for scaling, limits and failure). The exact torque and gain scaling formulas from Hodgins and Pollard are not in this file.
- I found no anthropometric table (for example the Army survey means for waist, hip and shoulder breadth) that I could read; the PDF fetches failed. The natural-ratio numbers rest on Wikipedia, a drawing canon and fashion-drawing guides, so they are convention-level, not measurement-level. Front-view width ratios for a real adult body are therefore not given; only circumference ratios and canon widths.
- I found no source on how acclaimed stylised games pick a waist ratio numerically. The section on stylisation rests on principles, one rebuild interview, and several search-summary-only sources (Overwatch, Sea of Thieves, Breath of the Wild); none of those summaries was read as a page.
- I found no authoritative runtime cost figures for skirts, cloth or capsules (searched for them). Every cost statement here is qualitative or a flagged anecdote.
- I did not verify whether Unity 6000.6 offers dual quaternion skinning, or the current default skin weight setting in this project.
- I did not look at fully simulated cloth with a physics body's contact forces (cloth pushing back on the body); Unity's cloth does not push back [S17], and I did not search for shipped games whose cloth does.
- I did not search for creators for physically simulated bodies in shipped games (other than Spore and the research system). Gang Beasts-style or ragdoll-comedy games with body options were not examined.
- I did not search the web for anything from the project, a person or Luis's references; no personal or project detail went into any query or URL.
- No file outside this folder and the scratch folder was written. Scratch holds two extracted PDF texts I used for the grep; nothing in the repository names the scratch path.

---

## Sources

(Tag. Title. URL. Reading status.)

- S1. Wikipedia, "Body proportions". https://en.wikipedia.org/wiki/Body_proportions. read.
- S2. Wikipedia, "Waist-hip ratio". https://en.wikipedia.org/wiki/Waist%E2%80%93hip_ratio. read.
- S3. Life Drawing Academy, "Human Body Proportions - Canon 1". https://lifedrawing.academy/drawing-lessons/human-body-proportions. read (partial page; rest is members-only).
- S4. Fashion Drawing For Dummies, "How to Draw Waists and Hips on Fashion Figures". https://dummies.com/article/academics-the-arts/art-architecture/drawing/fashion-drawing/how-to-draw-waists-and-hips-on-fashion-figures-171547. read.
- S5. Envato Tuts+, "Human Anatomy Fundamentals: Advanced Body Proportions". https://design.tutsplus.com/articles/human-anatomy-fundamentals-advanced-body-proportions--vector-19869. read.
- S6. Proko, "How to Draw Structure in the Body - Robo Bean", and "The Bean". https://www.proko.com/robo-bean/ and https://www.proko.com/the-bean/. snippet. Also Life Drawing Academy, "How to draw a torso", https://lifedrawing.academy/life-drawing-academy-news/how-to-draw-a-torso, snippet.
- S7. Wikipedia, "Twelve basic principles of animation". https://en.wikipedia.org/wiki/Twelve_basic_principles_of_animation. read.
- S8. Wikipedia, "Uncanny valley". https://en.wikipedia.org/wiki/Uncanny_valley. read (first half of the page only).
- S9. Autodesk, "Pixar evolves its legendary characters from The Incredibles with Autodesk Maya". https://blogs.autodesk.com/media-and-entertainment/pixar-incredibles-autodesk-maya. read.
- S10. Pixar, "Articulating Appeal" (paper). https://graphics.pixar.com/library/ArticulatingAppeal/paper.pdf. snippet (fetch redirected to the libraries index, not read).
- S11. Generic character-design articles on shape language and exaggeration, for example Dream Farm Studios, "Shape language in character design", https://dreamfarmstudios.com/shape-language-in-character-design/. snippet; weak.
- S12. PC Gamer, "How Torbjorn became the art inspiration for all of Overwatch". https://www.pcgamer.com/how-torbjrn-became-the-art-inspiration-for-all-of-overwatch. snippet.
- S13. 80.lv, "GDC18: Visual Adventures on Sea of Thieves". https://80.lv/articles/gdc18-visual-adventures-on-sea-of-thieves. snippet.
- S14. Thumbsticks, "GDC17: Designing Zelda: Breath of the Wild". https://www.thumbsticks.com/gdc17-designing-zelda-breath-of-the-wild/. snippet.
- S15. Wikipedia, "Frock coat". https://en.wikipedia.org/wiki/Frock_coat. read (first 100,000 characters).
- S16. Jeff Lander, "Devil in the Blue Faceted Dress: Real Time Cloth Animation", Game Developer, 2000. https://gamedeveloper.com/programming/devil-in-the-blue-faceted-dress-real-time-cloth-animation. read.
- S17. Unity Manual, "Cloth". https://docs.unity.com/en-us/engine/current/manual/physics-section/physics-overview/class-cloth. read.
- S18. Epic Games, "Clothing Tool in Unreal Engine". https://dev.epicgames.com/documentation/unreal-engine/clothing-tool-in-unreal-engine. read.
- S19. Unreal Engine forums, "How to setup low budget Chaos Cloth sim?". https://forums.unrealengine.com/t/how-to-setup-low-budget-chaos-cloth-sim/2701370. snippet; user claims, unverified.
- S20. VRM Consortium, VRMC_springBone-1.0 specification. https://github.com/vrm-c/vrm-specification/blob/master/specification/VRMC_springBone-1.0/README.md. read.
- S21. KawaiiPhysics wiki, "Tutorial Skirt (en)". https://github.com/pafuhana1213/KawaiiPhysics/wiki/Tutorial-Skirt-en. read (page says it may be outdated).
- S22. VRNavi, "Fix VRChat Skirt Clipping and Flipping with PhysBones". https://vrnavi.jp/en/skirt-trouble/. read.
- S23. note.com (Melvina Sakura), "Countermeasures for skirt clipping using VRM1.0 features". https://note.com/melvina/n/n38124ee31437?hl=en. read.
- S24. Polycount, "How to rig and animate cloth/hair in games??", https://polycount.com/discussion/88047/how-to-rig-and-animate-cloth-hair-in-games; Blender Artists, "rigging charactor with a long coat question", https://blenderartists.org/t/rigging-charactor-with-a-long-coat-question/510291. snippet.
- S25. Space Station 3D art guide, "Body Clipping". https://ss3d.gitbook.io/art-guide/3d/rigging/rig-formatting/body-clipping. read.
- S26. Unreal Engine forums, "Do video games hide the player model underneath the clothing?". https://forums.unrealengine.com/t/do-video-games-hide-the-player-model-underneath-the-clothing/33335. read.
- S27. Modding and forum threads: Unreal forums, "How to deal with upskirts", https://forums.unrealengine.com/t/how-to-deal-with-upskirts/87338; Sims 4 Studio, "Skirt legs clipping problem solved", https://sims4studio.com/thread/34735/skirt-legs-clipping-problem-solved. snippet; low authority.
- S28. Unity Discussions, "[RELEASED] Magica Cloth". https://discussions.unity.com/t/released-magica-cloth/773425?page=36. snippet; anecdotal.
- S29. Unity Scripting API, "QualitySettings.skinWeights". https://docs.unity3d.com/ScriptReference/QualitySettings-skinWeights.html. snippet.
- S30. Iwata Asks, Wii Channels (Mii creation). https://iwataasks.nintendo.com/interviews/wii/wii_channels/0/3. read.
- S31. Iwata Asks, "Mario 25th" interviews and Miyamoto on the 3DS Mii Maker. https://iwataasks.nintendo.com/interviews/wii/mario25th/0/8 and https://vooks.net/Miyamoto-discusses-improvements-in-the-3DSs-Mii-Maker. snippet.
- S32. Game Developer, "Building an inclusive character creator for the fantasy world of Baldur's Gate 3". https://gamedeveloper.com/art/building-an-inclusive-character-creator-for-the-fantasy-world-of-baldur-s-gate-3. read.
- S33. 80.lv, "Larian explained why there are no face sliders in Baldur's Gate 3". https://80.lv/articles/larian-explained-why-there-are-no-face-sliders-in-baldur-s-gate-3. read.
- S34. Bugnet, "How to design a character creator". https://bugnet.io/blog/how-to-design-a-character-creator; and a community wiki on inZOI creation, https://thegameswiki.com/inzoi/wiki/create-a-zoi. snippet; vendor-style and weak.
- S35. Hecker, Raabe, Enslow, DeWeese, Maynard, van Prooijen, "Real-time Motion Retargeting to Highly Varied User-Created Morphologies", ACM Transactions on Graphics 27(3), 2008. Paper page https://www.chrishecker.com/Real-time_Motion_Retargeting_to_Highly_Varied_User-Created_Morphologies (down when fetched); text read from the Chalmers-hosted copy https://www.cse.chalmers.se/edu/year/2011/course/TDA361/Advanced%20Computer%20Graphics/027-hecker%20copy.pdf. read (PDF text).
- S36. Won and Lee, "Learning Body Shape Variation in Physics-based Characters", ACM Transactions on Graphics 38(6), 2019. https://mrl.snu.ac.kr/publications/ProjectMorphCon/MorphCon.pdf. read (PDF text).
- S37. Hodgins and Pollard, "Adapting Simulated Behaviors For New Characters", SIGGRAPH 1997. https://ri.cmu.edu/?p=14446. snippet.
- S38. Wikipedia, "Square-cube law". https://en.wikipedia.org/wiki/Square%E2%80%93cube_law. read.
- S39. Unity Manual, "Articulation Body". https://docs.unity.com/en-us/engine/current/manual/physics-section/physics-overview/articulations-section/class-articulation-body. read.
- S40. NVIDIA PhysX 5.3.1 documentation, "Articulations". https://nvidia-omniverse.github.io/PhysX/physx/5.3.1/docs/Articulations.html. read.
- S41. Unity ragdoll documentation and guides: https://docs.unity3d.com/Manual/wizard-RagdollWizard.html and https://learn.unity.com/tutorial/creating-ragdolls-2019; a vendor page https://superhivemarket.com/products/ragdollforge/docs. snippet; the hip-jitter point is anecdotal.
- S42. Kavan, Collins, Zara, O'Sullivan, "Skinning with Dual Quaternions", I3D 2007. https://users.cs.utah.edu/~ladislav/dq/; and the Disney production note https://media.disneyanimation.com/uploads/production/publication_asset/98/asset/dualQ.pdf. snippet.
- S43. Wikipedia, "Skeletal animation". https://en.wikipedia.org/wiki/Skeletal_animation. read.
- S44. CG Cookie, "Searching for topology and modeling advice", https://blog.cgcookie.com/?p=12434 (fetch blocked), and Novedge, "ZBrush Tip: Deformation-Ready Joint Topology with ZRemesher", https://novedge.com/blogs/design-news/zbrush-tip-deformation-ready-joint-topology-with-zremesher. snippet; low authority.
