# The detached-looking leg while getting up by its own body: investigation

Status: read-only investigation of D:/Dev/WG at 957a21b (plus OwnBody.TurnsAsMeant defaulting to true). Nothing was run;
every claim below is from reading code, art scripts, docs and the picture. Line numbers are D:/Dev/WG unless stated.

## The picture
Docs/Correspondence/2026-11-13_images/leg_while_getting_up.png (646x580); crops in this folder (crop_legs.png, crop_hip.png).
Small (dark bob) lies on its back seen from above, arms out. The picture-left leg is a long grey trouser tube, boot at the
top-left; its UPPER end is an open, cut edge with a dark inner band, and ground is visible between that end and the blue
coat: the leg looks severed at the hip, not broken at the knee. A big blue skirt panel stands out on the right; the
right leg is raised, knee bent. Luis (Docs/Correspondence/2026-11-13_WHAT_TO_JUDGE_AND_THE_TURNS.md:8-21): "weird leg
deattached part", with "Gets up by its own body" on.

Key fact for reading it: the drawn model is skinned and driven by bone ROTATIONS only (MinerBody.cs:510-581; only the
pelvis bone gets a position, :521). A gap between physics parts cannot open a gap in the mesh; it can only aim a drawn
bone wrongly. So a mesh that looks cut must be a mesh that IS cut, shown where it should be covered.

## Candidates (most likely first)

### 1. The cut trousers exposed: thigh leaves the skirt (art + drawing; Small and Round only) - MOST LIKELY
What happens:
- Art/Blender/Worker/outfits.py:262-292 `trousers(..., hidden_above)` deletes trouser faces above a height.
  workers.py:125-126 Small: hidden_above = 0.30*1.42+0.06 = 0.486 m; skirt hem 0.31 (scaled ~0.44 m); closed coat. There is
  no leg skin under the coat (Docs/ArtDirection/CharacterPractices.md:25 records the practice). Round: workers.py:185-186,
  hidden_above = 0.37*1.56+0.06, smock hem 0.37. Long: workers.py:154-155, trousers NOT cut, open-front coat.
- The skirt is four flaps on bones at the hips (rigging.py:101-107); toward the hem vertices are wholly the flaps', above
  they blend into the pelvis (rigging.py:218-237). The skirt never follows the thighs.
- MinerBody.Drape (MinerBody.cs:633-671) turns the flaps from a "ground" taken as min(feet.y) - ankleUp (:638). Lying on
  the back with a foot raised (the picture's right leg) that ground is wrong, so flaps are laid level or slightly up
  (:661-668). The comment at :647-651 says a knee opening outward deliberately goes out from under the cloth.
- Anything that swings the drawn thigh far out of the skirt's cover (wide abduction, extension behind, a big twist), or
  lifts a flap off the thigh, shows the open ring at 0.486 m with nothing above it: a floating tube end, ground between it
  and the coat.
What drives the thigh out while getting up by its own body: candidate 2 (no hip limits; 89 degrees behind measured Oct 30),
plus the first-try roll onto the back "legs up and arms waving" (Validation.md:4668-4690,
Docs/Reviews/2026-10-30_GettingUp_EveryFrame.md).
When: Small (only miner on its own body; MinerPanel.cs:89) or Round on the old way; lying on the back or side, any try,
more at low strength (legs sprawl); not the fall pose alone unless the fall's hip limits (PhysicalFall.cs:40, side 45)
plus a lifted flap expose it.
Look vs picture: exact - open cut edge with a dark inner band (the inside of the tube), floating above ground, the leg
otherwise whole; the standing-out blue panel is a flap laid wrongly.
Likelihood: high (~60%).
Per-frame measure: (a) drawn hip angles of each thigh bone against the pelvis (flexion, abduction, twist); (b) "covered"
test: rays from the trunk axis through points of the cut ring (the drawn trouser vertices nearest 0.486 m in bind pose,
skinned) - each ray must hit skirt mesh before reaching the ring (ModelQualityMethod's "Beneath"); report the fraction of
ring points uncovered; (c) each flap's angle and Drape's ground estimate against the real terrain height under the pelvis.
Ruled out if no frame has an uncovered ring point.

### 2. No joint limits on the own body made lying (OwnBody.cs) - LIKELY (the driver of 1, and a look of its own)
What happens: OwnBody.cs:195 HingesStop = false; :203 Anatomical false; :763 anatomical = Anatomical && !down; :789-790
twist and swings FreeMotion; stops only with HingesStop (:807-819) or anatomical (:821). ByItsOwn -> BeginDown makes the
body down, so the body that gets up has NO limits: hips free in every axis, knees past straight or folded beyond 180,
ankle/boot free (BootTurnsAtMost=20 at :196 only through the drawn turn, not a stop).
When: any try, worst when the body is pushed by its own drives against the ground (first try, rolling over).
Look: a thigh splayed past human range (exposing 1), a knee bent backwards, a boot turned round on the shin. A knee bent
backward under a trouser tube could read as "broken off" but would not show an open cut edge.
Likelihood: high as a cause of 1; medium-low as the picture on its own.
Measure: each own link's jointPosition (FrameRecorder already writes it) against anatomical ranges: hip flexion -20..120,
abduction 45, twist 35 (the fall's own Hip vector, PhysicalFall.cs:40); knee -145..3; boot spin rate.

### 3. The posed body and balance acting during a try (PhysicalFall.cs:806-808 undoing OwnBody.cs:881-883)
What happens: OwnBody.Make sets balanced = balance.Acts; balance.Acts = false; body.LetGo = true (OwnBody.cs:881-883).
PhysicalFall.ByItsOwn (PhysicalFall.cs:788-814) then sets body.LetGo = false, own.enabled = true, balance.Acts = balanced,
Now = Up. So during a try the posed biped is posing again (ProceduralBiped.cs:570-574 gates on LetGo) and PhysicalBalance
reads the drawn, lying body's centre of mass (PhysicalBody.cs:207-212) against posed feet. PhysicalBalance.Falls
(:213-219) calls fall.LetGo() whenever fall.Now == Up, with no check of OwnBody.Stands; triggers at :460 (weight outside
feet > FallsAfter 2.2 s), :569 collapse, :649-650 hopeless/stumble. That would build a second, fall body from the drawn
segments while the own body still holds them, and OwnBody's GiveBack (:943-954 / :1374-1389) sets body.LetGo = false while
the fall holds it.
When: a try longer than ~2.2 s with the centre outside the posed feet - most tries lying on the back.
Look: a jolt; two bodies' colliders overlapping and kicking; drawn segments written by two writers in one frame (posed
LateUpdate vs OwnBody.LateUpdate vs PhysicalFall.LateUpdate), one leg taken from one writer and one from another. Could
put one leg in a far pose for a frame or more; still cannot open the mesh.
Likelihood: medium (the state contradiction is certain from code; whether it shows depends on order of LateUpdates).
Measure: log every fall.LetGo() call with own.Stands, body.LetGo, balance.Acts; count frames where both a fall body and
an own body exist; count overlapping colliders between them; per frame, which component wrote each segment last.

### 4. Joint gaps baked in at each hand-over and ratcheted across tries
What happens: PhysicalFall.Join (:367-393) uses autoConfigureConnectedAnchor = true (:377); OwnBody.Make with down uses
matchAnchors = true (:788). Both bake whatever gap the segments have at creation into the joint's rest. Hand-overs:
posed -> fall (LetGo :449-551, from miner.Solved; Ends() uses localScale.y as half length; Along() biases a straight leg
0.02 m forward) -> own (BeginDown) -> fall (GoDown, OwnBody.cs:1308-1343) -> ... up to TriesByItsOwn = 12 (:778).
Each pass may add a small gap or twist that the next keeps.
Look: since drawing is rotation-only, the gap shows as the shin aimed wrongly: MinerBody.cs:547-554 aims the shin bone at
the shin's ankle minus the THIGH's knee end, so a knee gap g gives an error of atan(g / shin length). It would bend the
leg oddly at the knee, not cut it at the hip.
Likelihood: low-medium for the picture; real and worth measuring.
Measure: at each creation and every physics step, world distance between each joint's two anchors; on the drawn
segments, thigh knee end to shin start; growth across tries.

### 5. Fall-body stretch and limit violations
What happens: projection PositionAndRotation at 0.02 m / 5 degrees (PhysicalFall.cs:388-389); angular projection inert
because no angular axis is locked; limits are not projected; knee swing cone 5/3 degrees (:40-42) is jitter-prone; at
high hip flexion twist and side limits swap meaning. enablePreprocessing = false.
When: hard landings, legs under the body, low strength.
Look: a knee or hip briefly past its limit, a jittering shin; drawn as a wrongly aimed limb.
Likelihood: low for the picture.
Measure: anchor gap per step (bar 0.02 m); joint angles against limits.

### 6. Blends at hand-over
What happens: the old way's Rising (PhysicalFall.cs:1039-1066) and OwnBody GiveBack (OwnBody.cs:1374-1389, GivesBackIn
0.25 s) lerp segment positions independently and use FromToRotation for limbs, dropping twist.
When: after 12 failed tries or an unknown lying (old way), and at every give back.
Look: a transient odd leg for a fraction of a second, mostly while rising, not lying flat.
Likelihood: low.
Measure: per-frame twist of each limb bone across the blend; limb length of drawn segments.

### 7. Boot slackening and drawn boot rotation
PhysicalFall.cs:902-943 (slackening at :928-943); OwnBody GoDown hands slackened boots on. A boot turned round on the shin.
Likelihood: low. Measure: boot angle against shin, boot spin rate.

### 8. A known blow-up
Boots measured at 115-154 rad/s and the body once blew apart (Validation.md:4668-4690). A blown-apart leg would show
segments far apart; drawn rotation-only it would be a wildly aimed limb. Likelihood: low. Measure: max link speed and
angular speed per frame.

## Ranking
1. Cut trousers exposed (thigh out of skirt / flap laid wrong) - the only candidate that draws an open cut edge.
2. No limits on the down-made own body - drives 1.
3. Posed body + balance acting during a try - a second writer / second body.
4. Gaps baked in at hand-overs, ratcheting over 12 tries.
5. Fall-body stretch / limits not projected / tiny knee cone.
6. Hand-over blends.
7. Boot slackening.
8. Blow-up.

## Bench plan that would catch it

Build on:
- D:/Dev/WG/Assets/_WonderGather/Tests/PlayMode/GettingUpByItsOwnTests.cs:35-182 (SmallPushedOverGetsUpByItsOwnBody) with
  its arguments -ownRate, -ownPushWays, -ownPushBack, -ownHingesStop, -ownFrames, -ownShots/-ownShotFrames/-ownShotFaults,
  -ownGoOn.
- D:/Dev/WG/Assets/_WonderGather/Tests/PlayMode/FrameRecorder.cs (records drawn bones, segments and parts every rendered
  frame; faults: limb spin > 30 deg/frame, hand/foot jump > 6 cm, own knee/elbow wrong way > 8 deg or folded past 150 deg;
  pictures from four sides).
- OwnRiseBench.cs (all miners, starts front/back/left/right, physics stepped by hand) for the start set.
What they lack: only Small; pushes only (no stumble); no low strengths; no skirt-cover test; no flap/ground check; no hip
range check; no fall-body anchor gaps or limit checks; no log of fall.LetGo while own.Stands or of body.LetGo /
balance.Acts; no baked-gap record per hand-over; cameras not chosen at the worst frame.

Runs (course = one fall followed by the whole getting up, to standing or to 12 tries):
- Miners: Small, Long, Round (Long as the control: its trousers are not cut).
- Falls: shove from front, back, left, right; stumble (walk into a step / trip) front and side; each 8 courses.
- Strengths: the game's (PhysicalBody.Strength as played) and 0.75x, 0.5x.
- Frame rate: -ownRate 50 fixed and as played (-ownAsPlayed 1, the game's own frame rate; also 120 to vary
  Time.time - Time.fixedTime interpolation).
- Both "Gets up by its own body" on (Small) and off.

Bars (set before any run; a frame failing any is a fault):
- Cut ring covered: 0 uncovered ring points in any frame (Small, Round).
- Drawn hip: flexion -20..120, abduction <= 45, twist <= 35 degrees (+5 tolerance).
- Knee: -145..+3 (+5 tolerance); never wrong way > 8, never past 150 (existing).
- Flap: Drape's ground within 0.05 m of terrain under the pelvis; flap never rising above level when lying.
- Joint anchor gap: fall body <= 0.02 m every step; own body <= 0.005 m; baked gap at creation <= 0.005 m and not growing
  across tries.
- fall.LetGo() while own.Stands: 0 calls; frames with two bodies: 0; body.LetGo must be true and balance.Acts false
  throughout every own try.
- Limb spin <= 30 deg/frame, hand/foot jump <= 6 cm (existing); link angular speed <= 40 rad/s; boot against shin <= 20 deg.
- Ground: no part below terrain by more than 0.02 m.

Pictures: for each fault, the worst frame and 5 frames either side, from two cameras: from above looking down (as in
Luis's picture) and from the side at ground height across the hips; plus a strip of the worst course.

## Check (the sceptic; read-only, nothing run; D:/Dev/WG working copy, whose OwnBody.cs lines after 220 are 2 more than at 957a21b)

(Written as it went; the summary is at the end of this section.)

### The picture, read again
- Brightened and enlarged crops: check_bright_body.png, check_crop_legs.png, check_crop_hip_nearest.png (this folder).
- The head shows only hair (no face): it lies with its crown toward the camera, or face down; "on its back" is not
  certain from the picture. Nothing below depends on which.
- The selection ring is an ellipse about 140 x 105 px, so the camera looks down at about 49 degrees. A raised (vertical)
  limb would project UP the screen; both grey tubes project mostly SIDEWAYS (left tube about (-120,-35) px from the coat,
  right about (+80,-40)). So the legs are spread wide to opposite sides, the left one about 70-90 degrees out from the
  body's line (head at the bottom, hips above it). That is past a living hip's 45 and past the fall body's side limit.
- Each tube is about 108 px, against the head's 120 px; the trousers from the boot top (about 0.2 m) to the cut at
  0.486 m are about 0.29 m long, the head with its bob about 0.3 m. So the visible tube IS the whole trouser leg below
  the cut: the open end in the picture is at the right place to be the cut ring.
- The open end runs within about 13 degrees of square to the tube's axis (a ring cut across the thigh, not the slanting
  line where a tube meets the ground). The band between it and the coat is darker than the ground nearby (lower left):
  more likely the inside of the open tube than ground. Either way: an open tube end, uncovered.

### Claims, one by one
Key fact (rotation-only drawing): CONFIRMED. MinerBody.cs:510-581 is LateUpdate; only the pelvis bone gets a position
(:521); thighs/shins are aimed by Turn() at the solved segments' ends (:547-554, Ends at :456-462). OwnBody.LateUpdate
(OwnBody.cs:1348-1373) writes every segment and the toes, then calls miner.TurnsAsSolved(), so each limb's twist is
also drawn from its physics part (Poles, MinerBody.cs:480-487). A physics gap cannot open the mesh.

Candidate 1 (cut trousers exposed): CONFIRMED in substance; the picture fits it best (see above).
- workers.py:125 Small hidden_above = 0.30*1.42+0.06 = 0.486 m: confirmed. Skirt hem 0.31 (about 0.44 m), so in the
  bind pose the skirt overlaps the cut by only about 4.6 cm. Small's hip is 0.45*1.42 = 0.64 m: the cut is about
  0.15 m down the thigh, about half way to the knee. Round :185-186 confirmed; Long :154-155 uncut, open front: confirmed.
- outfits.py:262-292: confirmed; faces wholly above hidden_above are deleted, the seat piece included, so the leg is an
  open tube with nothing above it (CharacterPractices.md:25 confirmed: "cut away above the knee").
- rigging.py:101-107 four flaps on bones at the hips; :218-237 flaps wholly their own from 0.09 m above the hem: confirmed.
- MinerBody.cs:638 ground = min(feet) - ankleUp; :661-668 laid level, up to asin(0.2) = 11.5 degrees above level when
  the estimate is above the flap: confirmed. :647-651 a thigh going out to the side leaves the cloth: confirmed (along is
  the thigh projected off the pelvis's across line, :653).
- MISSED, a code defect inside the living range: MinerBody.cs:655-656 picks the flap's stop face as cross(across, along)
  turned to agree with the pelvis's forward. Worked through: for hip flexion t that face is (0, -sin t, -cos t) before
  the test, so for t < 90 it is flipped to the thigh's true front, but for t > 90 it is NOT flipped and is the thigh's
  UNDERSIDE (at 111 degrees, (0,-0.93,0.36) where the front is (0,0.93,-0.36)). Past 90 degrees of hip flexion the front
  flap is held on the back of the thigh and the back flap on its front; at 90 the face jumps sides in one frame. Small's
  own ways ask 87 to 111 degrees (Resources/RiseWays/Small.txt: under, prop, stay; OwnRiser.cs:439 clamps asked hips to
  -20..130). So the ring can be uncovered with NO joint past its range. Consistent with (not proof): the Oct 30 sheet
  Images/GettingUp_2026-10-30/every_frame/first_try_rolls_past_its_front.jpg, frames #1068-#1092, fourth camera, shows a
  flap standing up like a board beside folded hips (crop: check_oct30_rows.png). The sign is independent of which way
  `across` points (worked both ways).
- "No leg skin under the coat": confirmed by the outfit list (no body mesh in Small's outfit; skin only where it shows).
- Likelihood: I agree it is first. "~60%" is not earned by anything measured; the picture is the only evidence.

Candidate 2 (no limits on the body made lying): CONFIRMED, one correction.
- OwnBody.cs:195 HingesStop = false; :203 Anatomical unset (false); :763 anatomical = Anatomical && !down; :788
  matchAnchors; :789-790 FreeMotion; :807-819 stops only on hinges (elbows, knees) and only with HingesStop; :821 Stops
  only anatomical. The hips are spherical and NEVER limited on this body, HingesStop or not.
- Correction: BootTurnsAtMost (:196) is not "through the drawn turn": it is link.maxJointVelocity on the boots (:819),
  applied only with HingesStop, so off as played.
- "89 degrees behind" is in Reviews/2026-10-30_GettingUp_EveryFrame.md:30: confirmed. No side (abduction) angle was
  measured there.
- Refutation attempt: 2 is NOT necessary for 1. Small's asked poses already put a hip 43 degrees out (plant, leading hip
  "54 43 110 10"; OwnRiser.cs:440 mirrors "out" per side, so positive is outward on both) and past 90 degrees of
  flexion (the flap flip above). What the missing limits add is the picture's degree of spread: the left leg is about
  70-90 degrees out, past what any way asks and past the fall body's 45 (PhysicalFall.cs:42). So 2 fits the picture's
  pose; the flap flip is an independent route to the same look, and both need measuring.

Candidate 3 (posed body and balance back on during a try): CONFIRMED as code; whether it fires is open.
- PhysicalFall.cs:801 BeginDown -> OwnBody.Make runs at once (OwnBody.cs:596-601, 658) and sets balance.Acts = false,
  body.LetGo = true (:881-883); then PhysicalFall.cs:806-808 sets body.LetGo = false, own.enabled = true, balance.Acts =
  the fall's own `balanced` (taken at PhysicalFall.cs:459 before the fall: true for a miner whose balance acted). The
  miner worked in the panel has a PhysicalBalance (MinerWorkPreview.cs:821; Acts defaults true, PhysicalBalance.cs:112).
- PhysicalBalance.Falls (:213-219) checks only fall.Now == Up, which ByItsOwn sets (:810). Triggers :460 (2.2 s outside),
  :569 (collapse after 0.3 s), :649-650: confirmed. Its weight is read from the drawn bones (PhysicalBody.cs:207-212).
  fall.LetGo (PhysicalFall.cs:449) has no OwnBody check: confirmed; it would build a second body from the segments, its
  parts not ignored by the own body's parts.
- Correction: drawing is NOT split between writers. ProceduralBiped has no execution order (0) and runs first;
  PhysicalFall 40, OwnBody 41 (writes every segment and the toes), MinerBody 50. While the own body stands it is what is
  drawn. So 3 can act only through physics (a second body kicking the first), never "one leg from one writer".
- Against it showing: a second body would leave marks in every recorded try; but nobody looked for one.

Candidate 4: mechanism CONFIRMED with a correction. PhysicalFall.Join (:367-393) first puts the part in its STANDING rest
against its parent (:372-373), then autoConfigureConnectedAnchor (:377): only a position gap is baked, never a twist
(the rotation rest is `rests`, not the pose). OwnBody's matchAnchors (:788) bakes the made pose as each joint's zero,
rotation too, but that is intended (drives aim from `rested`). Drawn effect: aim error atan(g / shin length), as said.

Candidate 5: limits (:40-42) and projection (:388-389) confirmed as written. Whether angular projection is inert with no
locked angular axis, and whether this engine version (6000.6.0f1) projects at all, NOT checked; irrelevant to the picture.

Candidates 6-8: OwnBody.cs:1374-1389 uses FromToRotation for limbs (twist dropped): confirmed. PhysicalFall.cs:1039-1066
and :902-943 not reopened. Validation.md:4668-4690 (115-154 rad/s, blew apart once): confirmed.

### A cause missed (a cheap check, probably negative)
The unit's own solid: Miner_Small.prefab has a SOLID CapsuleCollider (not a trigger; radius 0.213, height 1.42, centre
0.71) and no Rigidbody. The fall disables it (PhysicalFall.cs:542-543). OwnBody.Make tells physics to ignore it against
every own part (OwnBody.cs:866) while it is still DISABLED; only then does PhysicalFall.cs:807 enable it. During a try
OwnBody.LateUpdate moves the unit, and so this capsule, to the ground under the hips every frame (:1354-1358; StaysPut
false, :973). Unity issue reports and forum posts (not the official docs) say the ignore state can be lost, or not
applied, for colliders disabled or inactive when it is set. If it is lost here, a 0.43 m wide post stands in the hips
every frame and shoves both thighs outward: legs splayed to opposite sides, as in the picture. Against: the Oct runs stood
5 to 7 of 8 pushes, hard to believe with a post in the hips. One call settles it: Physics.GetIgnoreCollision(capsule,
part) for every part right after ByItsOwn, plus a count of contacts between them over a try. Bar: 0.

### Changes to the bench
1. The hip-range bar cannot be the measure of 1: the flap flip uncovers the ring inside the range, and Small's own ways
   ask up to 130 degrees (OwnRiser.cs:439), past the bar's 120+5, so the bar would flag designed poses. Keep it as a
   factor logged per frame ("beyond what was asked", from OwnRiser's asked turn), not as the test of the picture.
2. The cover test decides 1. Find the ring as the trousers mesh's BOUNDARY edges (edges with one face), not "vertices
   nearest 0.486 m". Each frame, bake the skinned trousers, skirt and coat (SkinnedMeshRenderer.BakeMesh) and cast from
   each ring vertex toward the game camera and straight up; uncovered = the ray leaves without hitting coat, skirt or
   body. "Rays from the trunk axis through the ring" is not what the eye sees from above at 49 degrees.
3. Add the flap face test: for each flap, whether its stop face (MinerBody.cs:655-656) is the thigh's true front (or
   back), with the hip flexion. Expected wrong whenever flexion > 90.
4. The Drape-ground bar (0.05 m) fails by design on every lying frame with a foot raised (Small, Long and Round). Log it
   as a factor beside the cover result; it is not a fault bar.
5. Long is no control for getting up by its own body: only Small has ways (Resources/RiseWays holds only Small.txt), so
   Long and Round always get up the old way, switch on or off. The useful controls: Round the old way (cut trousers, fall
   body with limits; does the fall alone uncover the ring?), and Small with the switch off.
6. OwnRiseBench steps physics by script (OwnRiseBench.cs:277-278) and lays bodies itself: it bypasses ByItsOwn's restore
   of balance and LetGo and the unit's capsule, so it cannot see 3 or the capsule. Take only its start poses; run the
   courses through the game flow (GettingUpByItsOwnTests' PushOver path, FixedUpdate mode).
7. Link speed: FrameRecorder already faults "whip" at 30 rad/s (FrameRecorder.cs:191); the plan's 40 is looser. Keep 30.
8. For 3, log PhysicalBalance.Fell and every fall.Now change with own.Stands; balance.Acts being true during a try is
   already certain from the code and needs no bar, only whether Falls fires.
9. Baked gap: state its bar in drawn degrees (atan(g / shin length)) as well as metres; that is what can show.
10. Pictures from the game camera (about 49 degrees down, as the ring's ellipse gives) at several yaws, besides straight
    above and the ground-height side view.
11. Bars set before a run: yes as written, with 1, 4 and 7 changed here before anything runs.
12. Cheapest decisive step, before any course: pose Small's drawn model with no physics, lying on its back, over a grid
    of hip flexion (-20..130) x out (0..90) x twist, feet down and one foot up, Drape running, and run the cover test
    and a picture from the game camera. It maps where the ring shows, and tells abduction (2) from the flap flip.

### Summary of the check
Held: 1 (the art cut; the picture's open tube is the whole trouser leg below the cut, the cut square to it), 2 (no hip
limits at all on the body made lying), 3 as code (balance acts and the posed body poses during every try), 4's mechanism.
Fell or corrected: 3 cannot draw one leg from another writer (OwnBody draws last); 4 bakes no twist in the fall body;
BootTurnsAtMost is a joint speed cap, off as played; 2 is not needed for 1. Missed: the flap face flips past 90 degrees
of hip flexion (MinerBody.cs:655-656), uncovering the ring inside the living range; and the unit's capsule, ignored
while disabled (OwnBody.cs:866 before PhysicalFall.cs:807), worth one call. Order to test: (a) the posed-model cover
grid (no physics), (b) GetIgnoreCollision on the capsule, (c) game-flow courses on Small with cover, flap face, hip
angles, balance.Fell and second-body log, Round old way and Small switch off as controls, (d) gaps, projection, blends.

## Checked since

- **(b), the capsule: ruled out** (November 13, afternoon; Validation, "The unit's capsule against its own body"). Small
  pushed over three times through the game's flow, 25 tries: the capsule inside the body on 14,557 of 14,562 frames,
  every pair ignored on every frame, 0 contacts from either side; the controls saw contacts (a ball on the capsule, 23;
  the parts with anything, 121,010).
