# U2: Unnatural movement - the craft, the measures, the body's norms

Researcher 2 of 3, November 13, 2026. Written for the whole project, not only the walk's turns.

Extends and does not repeat:
- `Docs/Research/research_notes/Getting up for any body/twitches_and_broken_limbs.md` (RL action jitter, CAPS, PD gains,
  PhysX limit behaviour, the CDC joint-range table, PhysDiff penetrate/float/skate, SPARC, render-path detectors,
  frame-rate testing). Where a topic is there, this file only points to it.
- `Docs/Research/2026-11-13_TurningAndSettling/research.md` section 3-4 (turn timings, head lead, catching steps,
  standing weight shifts and fidgets, Prazak foot slide figure, stance widths).

## How to read this

Marks on every claim:
- **[WE]** well established: several studies agree.
- **[1S]** one study (or one lab).
- **[U]** unclear: practice lore, a secondary summary, conflicting sources, or a number I could not re-check.

"Read" says how much of a source I saw: **full** (whole text or the relevant section opened), **abstract** (abstract or
an index page), **secondary** (only another paper's or a search summary's account), **book** (known classic, not
opened online). Nothing was downloaded.

Each item gives: what it is; how to compute it each frame from bones, contacts and the centre of mass (COM); the
number if any; source; mark. "Game threshold" lines are my **inference** for a starting value, to be calibrated on the
game's own footage and the Game Director's eye; they are not findings.

Notation used in the per-frame recipes: `p_j(t)` world position of joint j at frame t, `dt` the frame time,
`c(t)` whole-body COM (mass-weighted sum of the bodies' COMs), `v_c` its velocity, `BoS` the base of support (convex
hull of the contact points of planted feet/hands on the ground, projected to the ground plane), `L` leg length
(hip joint to floor in standing), `g` = 9.81 m/s^2.

---

## Part A. Animation craft: the principles and the named faults

### A1. The principles (what "natural" means to animators)

The twelve principles were named at the Disney studio in the 1930s and written down by Thomas and Johnston (1981,
book). Lasseter (1987) argued that the same principles carry over to 3D computer animation unchanged in meaning, only
the application changes ("timing, anticipation, follow-through ... apply in the same way") [WE: the field's canon;
Lasseter read: abstract and index pages]. For a physics-driven body most of them are not styles to add but signatures
of mass and muscle that a correct simulation should produce by itself; where the sim does not produce them, that is the
fault to look for.

| Principle | What it means in a real body | What to measure per frame | Mark |
|---|---|---|---|
| Timing | Heavier masses and longer limbs take longer; time of an action fits mass and strength | Duration of each action against the body's own scale (B/C below: sqrt(L/g) and strength-to-weight) | [WE] craft; scaling [WE] biomech |
| Spacing / slow in and slow out | Movements start and stop with speed rising and falling smoothly (bell-shaped speed) | Speed profile of the end effector or joint angle: peak/mean speed ratio (minimum jerk gives 1.875; human reaches ~1.75, Flash & Hogan 1985) | [WE] |
| Arcs | Limbs rotate about joints, so hands, feet and head trace curved paths | Curvature of end-effector paths between key events; long straight-line segments in a swing are suspect (except deliberate reaches, which are near straight) | [WE] craft |
| Anticipation | A preparatory move against the action: crouch before a jump, weight shift before a step, wind-up before a swing | Before a step: centre of pressure (COP) shift toward the leg to be unloaded (237-254 ms, 4-5 cm, Laudani 2021 in the turning file); before a swing: backswing angle and time | [WE] |
| Follow-through and overlapping action | Parts do not stop together; distal parts lag and overshoot, loose things settle after | Stop time of each segment after the action ends: proximal first, distal later; lag of held/hanging objects behind the hand | [WE] craft |
| Secondary action | Smaller actions that support the main one (head turns, arm swing, breathing) | Presence and size of arm swing, head stabilisation, breath (Part C) | [WE] craft |
| Weight | Mass shown by how the body loads, compresses, and recovers | Knee flexion at loading, COM drop at contact, time to recover; COM within or moving toward BoS | [WE] craft |
| Squash and stretch | Shape change under force; in a rigid-bone body, only flesh and joint compression | Bone lengths must NOT change (see B5); soft parts may | [WE] |
| Staging, exaggeration, appeal, solid drawing | Readability and character | Not measurable per frame; Game Director judgement | - |

Sources: Thomas & Johnston 1981, *The Illusion of Life* (book). Lasseter 1987, "Principles of traditional animation
applied to 3D computer animation", SIGGRAPH Computer Graphics 21(4):35-44,
https://www.evl.uic.edu/aej/527/papers/lasseter.pdf. Williams 2001, *The Animator's Survival Kit* (book; table of
contents lists "Symmetry or 'Twinning'", p. 324).

### A2. The named faults

Each fault: definition / why it reads wrong / per-frame detector / number / source / mark.

1. **Popping.** A pose or value that jumps between two frames (a blend that is too short, a target switch, an IK flip).
   Detector: per joint, angular change between rendered frames, and its second difference (angular acceleration);
   flag frames where the change exceeds what the joint's speed limit allows (see C9) or where acceleration spikes
   above the joint's running distribution (e.g. > 5x its median absolute deviation). Number: practice treats blends
   under about 0.2 s as pops (Jirik 2026, in the turning file) [U]. The physics-side and render-side checks are in the
   twitches file section 5.
2. **Foot sliding / footskate.** A foot that should be planted moves along the ground. Viewers see slide under about
   21 mm in most conditions; any cleanup was preferred to sliding (Prazak, Hoyet, O'Sullivan 2011, abstract) [1S].
   Kovar, Schreiner, Gleicher 2002 named and fixed it by filtering the planted projections and stretching the leg to
   absorb the rest [1S, abstract]. "Even small amounts of footskate can destroy the realism" (Johansen 2009, in the
   turning file). Detector: B1.
3. **Moonwalking.** The feet cycle but the body's travel does not match the stride (a walk cycle played faster or
   slower than the ground speed), so planted feet slide backward or forward. Detector: per stance phase, planted-foot
   travel relative to the ground (B1) and the ratio of COM travel per step to step length (should be about 1 over a
   cycle). Number: same as footskate. Source: game practice (motion-matching and foot-IK talks: Ubisoft GDC 2016
   motion matching; GDC "Fitting the World: A Biomechanical Approach to Foot IK", session listings) [U: practice].
4. **Knee pops and IK flips.** A two-bone IK near full extension: as the target distance approaches the summed bone
   lengths the knee angle changes very fast for a small target change (the derivative of the law-of-cosines angle
   goes to infinity at full extension), so the knee "snaps" straight; and when the pole/target passes through the
   singular line the knee direction flips to the other side. Practitioners avoid full extension, give the rest pose a
   slight bend and put a pole target in front of the knee; "soft IK" slows the approach to straight but makes the foot
   lag its target (Polycount and Blender community threads; Escape Studios blog) [U: practice, consistent across
   sources]. Detector: knee angle velocity while knee angle < 10 deg from straight; sign change of the knee's bend axis
   relative to the pelvis forward (a flip); distance target-to-hip / (thigh+shin) above 0.98.
5. **Hyperextension.** A hinge (knee, elbow) bent the wrong way, or past straight. Normal extension is about 0-5 deg
   (CDC study: knee extension 1.0-1.6 deg, elbow extension 0.8-4.7 deg mean; healthy elbow extension -6 to +11, in the
   twitches file) [WE]. Detector: signed hinge angle in the joint's anatomical frame; flag below -5 deg (beyond straight).
   Any sideways (varus/valgus) bend of a hinge beyond a few degrees reads as broken (twitches file, inference) [U].
6. **Gimbal flips.** Euler-angle decompositions lose a degree of freedom near 90 deg of the middle angle and jump
   when crossing it; interpolating Euler angles or quaternions without the shortest-path sign check spins a bone the
   long way round. PhysX also has a singularity at 180 deg swing (twitches file) [WE]. Detector: dot(q_prev, q_curr)
   < 0 before any lerp; a bone's rotation change between frames > 90 deg; swing angle > 150 deg in any joint frame.
7. **Linear, robotic interpolation.** Constant-speed motion with abrupt start and stop (no slow in/out); joints that
   all start and stop together. Real point-to-point human movement follows a bell-shaped speed close to minimum jerk
   (Flash & Hogan 1985) [WE]. Detector: per action, the speed profile's shape: peak/mean speed (1.5-2.0 human;
   1.0 for constant speed), time from onset to peak speed (near 50% of the movement for minimum jerk), and the onset
   times of different joints (should be staggered, B/C).
8. **Twinning and symmetry.** Both arms or both legs doing the same thing at the same time, or a mirror-symmetric pose.
   Animators treat it as a mark of inexperience that kills life (Williams 2001, "Symmetry or Twinning"; Animation
   Mentor and Animator Island articles) [WE in craft]. Real standing is asymmetric moment by moment
   (Prado-Rico & Duarte 2019, in the turning file) [1S]. Detector: left-right mirrored joint-angle difference
   |q_L - mirror(q_R)| per joint; flag when it stays below ~2 deg for more than ~1 s in an idle or a held pose; arm
   swing in phase with the same-side leg (anti-normal swing, C4).
9. **Dead stillness.** A hold with no motion at all. Animators use a "moving hold": a long slow ease into a pose or a
   small overshoot and settle, so the character never stops completely (Animation Mentor blog) [WE in craft]. Real
   standing never stops: sway, breathing, shifts (C13). Detector: whole-body RMS joint speed over a 1 s window;
   flag a living character below the floor of human quiet stance (e.g. COM sway speed far below ~2-5 mm/s, inferred
   from COP speed ~9 mm/s, C13), and also flag the opposite (sway far above human).
10. **Mechanical repetition.** Identical cycles: the same idle loop, the same fidget, the same step every time.
    Real shifting intervals have a spread about as large as their mean (Duarte & Zatsiorsky 1999, in the turning
    file) [1S]. Detector: coefficient of variation of event intervals (shifts, fidgets) - human about 0.5-1.0; and
    cross-correlation between successive cycles of the same action (near 1.0 = copy).
11. **Floaty motion.** The body moves as if gravity were weak or the body were light: slow fall, soft landing with
    no compression, drifting without contact forces. In ballistic phases the COM must follow a parabola with g:
    viewers detect errors, more easily in horizontal than vertical velocity, and added accelerations more than added
    decelerations (Reitsma & Pollard 2003) [1S]. Detector: during any frame with no contact, COM vertical acceleration
    should equal -g within a few per cent, horizontal COM velocity constant, and whole-body angular momentum about the
    COM constant (B9). With contact: vertical COM acceleration implied by the motion must come with a ground force the
    feet could supply (B8).
12. **No weight shift.** A step taken without first moving the COM/COP over the stance foot, or a swing foot lifted
    while still loaded. Practice: the foot must reach the ground before the body moves its weight to that side
    (80.lv tip, practice) [U]; science: COP moves toward the swing leg first and then to the stance leg before toe-off
    (Laudani 2021, turning file) [1S]. Detector: at the frame a foot leaves the ground, its share of the vertical
    contact force (from contact impulses) should be near zero, and the COM's ground projection should be over or
    moving toward the other foot.
13. **No anticipation.** A jump without crouch, a swing without wind-up, a turn without the head leading. Detector:
    action-specific (head yaw leads pelvis yaw in turns by 0.05-0.12 s, turning file; tool wind-up, C12).
14. **Limbs through the body or the world.** Self-penetration of non-adjacent parts, or penetration of ground and
    walls. Detector: B6 and B2.
15. **Hands that do not grip.** A held object whose surfaces do not touch the fingers, or fingers that pass into it.
    Hand-object research measures penetration volume (voxelised overlap, 1 mm^3 or 0.1 cm^3 voxels), penetration depth
    (largest distance of a hand vertex inside the object), contact ratio (share of grasps touching, or of hand vertices
    within a small distance, e.g. 0.45 mm), and simulation displacement (drop the object under gravity in the grasp and
    measure how far it moves) (ContactGen, Liu et al. 2023; GraspTTA, Jiang et al. 2021) [WE as convention]. A grasp
    with too little contact or too deep penetration both mislead; papers combine them [WE].
16. **Objects that float.** A held, carried or hanging object that does not hang from the hand, does not swing, or
    moves with no force on the hand. Detector: for each held object, the force needed for its motion
    (m*(a - g)) must pass through the grip contacts, and the hand/arm must show it (load on the arm, the body leaning
    against it, C11). A hanging object's swing period should be about 2*pi*sqrt(l/g) for its pivot-to-COM length l
    (pendulum physics) [WE].
17. **Props and cloth without lag.** Loose parts that move exactly with the bone they hang from. Detector: lag angle
    between a loose part and its parent during accelerations (should be nonzero and in the direction opposite the
    acceleration), and a decaying oscillation after the parent stops (follow-through). Number: none published for
    games [U].

---

## Part B. The measures used by research on generated and simulated motion

These come from motion-generation (diffusion, learned controllers) and pose-estimation papers. They are the most
directly computable checks for the game, because they need only bones, contacts and the COM. The twitches file already
has PhysDiff's three (penetrate, float, skate, 5 mm tolerance), LCP and CAPS action-smoothness, and SPARC; they are not
repeated here.

### B1. Foot skating
- **GMD convention** (Karunratanakul et al. 2023, ICCV, read: full via ar5iv): skating ratio = share of frames in
  which a foot in contact (foot height below about 5 cm) slides more than 2.5 cm between frames [WE: widely reused].
- **NVIDIA Kimodo benchmark** (docs, read: full): `foot_skate_ratio` = share of contact frames (toe below 0.05 m on
  this and the next frame) with toe speed above 0.2 m/s; `foot_skate_from_height` = mean toe speed when toe below
  0.05 m; contact heuristic for checking labels: speed below 0.15 m/s and height below 0.10 m [1S: one benchmark].
- **Perception:** about 21 mm of slide is seen (Prazak 2011) [1S].
- Per frame in the game: for each foot with a contact this frame and last frame, horizontal travel of the contact
  point (or toe and heel joints) and yaw change. Use the physics contact, not a height guess, since the game has
  contacts. Game threshold (inference): accumulated slide per stance phase > 2 cm, or any frame speed > 0.2 m/s while
  loaded with > 20% body weight.

### B2. Ground penetration and floating
- PhysDiff definitions in the twitches file (lowest vertex below/above ground; 5 mm tolerance) [WE as convention].
- Per frame: lowest point of each foot collider/mesh relative to the ground under it; for the planted foot, the gap
  should be within +/-5 mm; a foot "planted" by the logic but more than 5 mm above ground is floating.

### B3. Jitter and jerk
- Jerk = third time derivative of position (or angle). Smoothness metrics: mean squared jerk, log dimensionless jerk,
  spectral arc length (SPARC); Balasubramanian et al. 2015 argue SPARC is the valid one (twitches file) [WE].
- Viewers broadly prefer smoother motion but show a small preference for slightly rougher-than-minimum-jerk motion,
  more in VR (Hirose et al. 2020, "Integrated jerk as an indicator of affinity for artificial agent kinematics",
  PeerJ, https://www.ncbi.nlm.nih.gov/pmc/articles/PMC7500322/, read: abstract) [1S]. Perfectly smooth is not the
  target either.
- McDonnell, Newell, O'Sullivan 2007, "Smooth movers" (SCA): how many updates per second viewers need for a motion
  to look smooth depends on cycle rate, linear speed and complexity; slower movements need fewer
  (https://diglib.eg.org/handle/10.2312/SCA.SCA07.259-270, read: abstract) [1S].
- Per frame: per joint, finite-difference angular velocity, acceleration, jerk over rendered frames (use dt per frame;
  the game's dt varies); report high-percentile jerk and count spikes; SPARC on 2-4 s windows of end-effector speed.
  HumanScore (2026) also flags "kinematic extremes" - velocity spikes in joint angular motion - separately from
  smoothness (read: project page) [1S].

### B4. Bone length
- Viewers notice a limb length change of about 3% when attending to it (2.7% for arm segments in one experiment);
  more than 20% can go unseen when attention is elsewhere (Harrison, Rensink, van de Panne 2004, TOG 23(3):569-573,
  https://www.cs.ubc.ca/labs/imager/tr/2004/harrison-ea-2004-obscuring, read: abstract/page) [1S].
- Generation papers use bone-length error against a standard skeleton or the variance over time (A-MDM; "Shape My
  Moves"; HumanScore "bone length stability") [WE as convention].
- Per frame: distance between each pair of connected drawn joints vs. its rest length; in a rigid-body character it
  must be constant; any change > 1% is a render-path or joint-separation bug (twitches file: joint separation > 2 cm).
  Note Kovar 2002 and Prazak 2011: small leg lengthening is a known, accepted cheat to hide footskate - do not use it
  for the physics body, whose bones are rigid.

### B5. Joint limits
- Generation and pose papers count the share of joint-frame pairs outside a valid range (PhyMotion 2026 uses the
  MuJoCo humanoid ranges; secondary) and penalise hyperextension (HumanScore "joint range") [U for exact ranges].
- Per frame: each joint's swing-twist angles in its anatomical frame against the ranges in C1, and separately against
  the physics limits. Report: frames outside anatomical range, the largest excess, and time spent within 5 deg of a
  limit (sitting on a limit reads as a doll).

### B6. Self-penetration
- Count interpenetration between non-adjacent body parts; for SMPL meshes about 2% of vertices in penetration is a
  normal baseline (mesh noise), above 20% severe (PhyMotion 2026, secondary via search) [U].
- Per physics step: Physics.ComputePenetration between non-adjacent colliders (thigh-thigh, shin-shin, forearm-trunk,
  hand-head), depth and count; and the smallest shin-to-shin distance in swing (turning file item 5).

### B7. Centre of mass against the support
- **Static:** a pose is statically balanced when the COM's ground projection lies inside the BoS; outside it, an
  uncompensated moment acts about the foot edge (Chen et al. 2020 arXiv 2001.00657; IPMAN, Tripathi et al. 2023 CVPR,
  use COP-COM overlap as a stability loss) [WE].
- **Dynamic:** the extrapolated centre of mass XcoM = c + v_c / omega0, omega0 = sqrt(g / l) with l the pendulum
  length (about 1.2 x leg length in Hof's use, or COM height); margin of stability b = smallest distance from XcoM to
  the BoS edge; b < 0 means the body cannot stop without a new support (a step) (Hof, Gazendam, Sinke 2005, J Biomech
  38:1-8, https://braceworks.ca/wp-content/uploads/2016/05/hof-condition-for-dynamic-stability.pdf, read: abstract and
  summaries) [WE]. Caveats on reading MoS as instantaneous stability: Curtze, Buurke, McCrum 2024, "Notes on the margin
  of stability", https://cris.maastrichtuniversity.nl/en/publications/notes-on-the-margin-of-stability/ [1S].
- Per frame: c, v_c from the bodies; BoS from active contacts; b in each direction. Use for unnatural: (a) a body that
  stands still with b < 0 and does not fall (a hidden force holds it), (b) a body that steps when b is large and
  positive with nothing pushing (a step from nowhere), (c) catch steps that land with the XcoM still outside the new BoS.
  In walking, healthy humans keep a small positive sideways margin at heel strike (Hof 2005) [WE].

### B8. Ground reaction force and centre of pressure plausibility
- The net ground force must equal m*(a_c - g) (Newton for the whole body), and the COP must lie inside the BoS (feet
  cannot pull on the ground; force must be compressive and within friction, |F_t| <= mu*F_n) [WE: physics].
- GroundLink (Han et al. 2023, SIGGRAPH Asia) provides force-plate GRF and COP with motion capture to test such
  predictions; kinetics-aware imitation work (arXiv 2603.12408, 2026) reports that motion-only imitation gives
  biomechanically inconsistent GRF/COP patterns and that GRF/COP rewards fix much of it [1S each, abstract].
- In the game PhysX supplies true contact forces, so the test changes: not "is the GRF possible" but "is the force
  the controller asks of the feet human". Per physics step: sum of contact impulses / dt per foot, its vertical share
  over body weight, COP = force-weighted average contact point. Human references: walking vertical GRF has two peaks of
  about 1.1-1.2 body weight with a dip near 0.7-0.8 between (gait texts) [WE, not re-opened]; quiet stance COP speed
  about 9 mm/s (C13). Flag: COP outside BoS by > 1 cm (impossible; means a non-contact force), feet pulling (negative
  normal force), or sideways contact force beyond friction.

### B9. Momentum or energy from nowhere
- **In flight** (no contact), linear momentum changes only by gravity and angular momentum about the COM is conserved;
  Liu & Popović 2002 (SIGGRAPH, "Synthesis of complex dynamic character motion from simple animations") built
  momentum constraints for flight and contact phases to make simple animations physical
  (https://grail.cs.washington.edu/projects/charanim/phylter.html, read: abstract) [WE: physics].
- **In walking**, whole-body angular momentum about the COM is kept small although segments carry large momenta:
  segment cancellation about 95% medio-lateral, 70% anterior-posterior, 80% vertical (Herr & Popovic 2008, J Exp
  Biol 211:467-481, https://aspirin.media.mit.edu/biomechatronics/wp-content/uploads/sites/8/2013/07/Herr-2008_Angular-momentum-in-human-walking.pdf,
  read: abstract) [1S, 10 subjects]. Arm swing is part of that cancellation (C4).
- **Residual or "hand of God" forces:** Yuan & Kitani 2020 (NeurIPS, "Residual force control") add invisible
  external forces so a humanoid can match human motion it otherwise could not; they call them forces that go beyond
  the model's physical limits (https://arxiv.org/abs/2006.07364, read: abstract) [1S]. This is exactly what the
  game must not have in play: any root force, keeper force or helper torque not supplied by a joint or a contact.
- Per physics step: total mechanical energy E = kinetic + potential of all bodies; dE/dt must not exceed the summed
  joint power (torque x relative angular velocity, estimated from drive targets since PhysX does not report drive
  torque) plus contact work. The twitches file and the memory note "the engine's step adds energy" already flag that
  the solver itself can add energy; the measure is the residual: dE/dt - P_joints - P_contacts. In flight: change in
  angular momentum about the COM per step (should be ~0) and COM acceleration (should be -g).

### B10. Learned and statistical "naturalness" scores
- Ren, Patrick, Efros, Hodgins, Rehg 2005 (TOG 24(3), "A data-driven approach to quantifying natural human motion")
  trained hierarchical models (per joint, per limb, whole body) on a 4 h motion database; an ensemble classified
  natural vs unnatural and was compared with a user study; their best model (switching linear dynamic system) was
  reported at about 82-84% correct (secondary summary)
  (https://www.ri.cmu.edu/publications/a-data-driven-approach-to-quantifying-natural-human-motion, read: abstract) [1S].
  Useful idea for the game: the hierarchy - a fault can sit in one joint while the whole body looks fine.
- HumanScore 2026 (Stanford, arXiv 2604.20157; read: project page): six metrics in three tiers (extra limbs, bone
  length stability, joint range, self-collision, kinematic extremes, smoothness) and a reported gap between how
  plausible motion looks and how biomechanically right it is [1S]. Lesson: looking right and being right are
  separate checks; the game needs both.

### B11. Perceptual sensitivity relevant to interactions
- Reactions to a push that come more than 150 ms early or late were accepted less than half the time; over-reaction
  was more acceptable than under-reaction; angular distortions in the reaction lowered acceptance; reactions moving
  away from the pusher's body were preferred (Hoyet, McDonnell, O'Sullivan 2012, TOG 31(4):90, "Push it real",
  https://history.siggraph.org/?p=110569, read: abstract) [1S].
- More humanlike characters raise sensitivity to some ballistic errors (Reitsma, Andrews, Pollard 2008, CGF 27(2),
  https://diglib.eg.org/handle/10.2312/CGF.v27i2pp201-210, read: abstract) [1S]. The miners are stylised, which may
  buy a little tolerance, not much.

---

## Part C. Biomechanics norms a body must keep

### C1. Normal ranges of motion per joint

Checked numbers for 20-44 year olds (CDC, Soucie et al. 2011) are in the twitches file: hip flexion 130-134,
extension 17-18; knee flexion 138-142, extension 1-1.6; ankle dorsiflexion 13-14, plantarflexion 55-62; shoulder
flexion 169-172; elbow flexion 145-150, extension 1-5; pronation 77-82, supination 85-91 (degrees) [WE].

The commonly taught AAOS-style values for the rest (I could not open an authoritative table; flashcards and secondary
tables agree on wrist values and disagree on neck and spine) [U unless marked]:

| Joint | Motion | Typical value (deg) | Mark |
|---|---|---|---|
| Neck | flexion / extension | 45-60 / 45-75 | [U] sources differ |
| Neck | lateral flexion | 45 | [U] |
| Neck | rotation each way | 60-80 | [U] sources differ |
| Thoracolumbar | flexion / extension | 60-80 / 25 | [U] |
| Thoracolumbar | lateral flexion | 25-35 | [U] |
| Thoracic | rotation each way | 30-45 | [U] |
| Shoulder | extension | 50-60 | [U] |
| Shoulder | abduction | 170-180 (includes scapula) | [WE] |
| Shoulder | internal / external rotation (arm at 90 abduction) | 70 / 90 | [U]; coupled to elevation (twitches file) |
| Wrist | flexion / extension | 80 / 70 | [U, two secondary sources agree] |
| Wrist | radial / ulnar deviation | 20 / 30 | [U, two secondary sources agree] |
| Hip | abduction / adduction | 45 / 30 | [U] |
| Hip | internal / external rotation | 35-45 / 45 | [U] |
| Ankle-foot | inversion / eversion | 35 / 15 | [U] |

Normative neck values vary widely between studies and devices; the systematic review by Thoomes-de Graaf et al. 2020
(Musculoskeletal Science and Practice, "Normative values of cervical range of motion for both children and adults")
found mostly low-quality, heterogeneous studies (https://epublications.regis.edu/facultypubs/98, read: abstract) [WE
that values differ]. Ranges shrink with age (CDC 45-69 group slightly smaller) [WE].

Use: these are the passive/active maxima. Everyday movement uses much less: e.g. walking needs about 60 deg knee
flexion in swing and about 30 deg hip flexion (gait texts) [WE, not re-opened]. A body that habitually works near its
range limits reads as strained or broken; the measure is time spent within 10 deg of a limit (B5).

Per frame: swing-twist decomposition of each joint (avoid Euler, A2.6), compared with the table, with a 5-10 deg
margin; hinges also checked for sideways bend and twist (A2.5).

### C2. Gait relations: speed, cadence, step length

- **Preferred walking speed** in healthy adults is about 1.2-1.4 m/s, fairly stable from the 20s to 50s and declining
  after 60; men slightly faster (Bohannon & Williams Andrews 2011, "Normal walking speed: a descriptive meta-analysis",
  Physiotherapy 97:182-189, 41 studies, 23,111 people, https://pubmed.ncbi.nlm.nih.gov/21820535/, read: abstract;
  the per-decade values from a secondary page) [WE]. The turning file has 1.26-1.39 m/s in lab samples.
- **Cadence** about 100-120 steps/min at preferred speed in adults (turning file: 111-113 in turns; older adults
  103-112, Aboutorabi et al. 2016 review) [WE]. Older adults: slower, lower cadence, shorter steps, longer double
  support (Herssens et al. 2018 systematic review, 18 studies, 2,112 adults) [WE].
- **Walk ratio** (step length / cadence) stays nearly constant across a wide range of speeds, so people speed up by
  lengthening steps and raising cadence together, not one alone; it breaks down at slow speeds (below about 62 m/min,
  i.e. ~1.0 m/s) (Sekiya & Nagasaki 1998 Gait Posture; Murakami & Otaka 2017 J Phys Ther Sci,
  https://pmc.ncbi.nlm.nih.gov/articles/PMC5430280, read: abstract) [WE]. Typical adult walk ratio about 0.0055-0.0065
  m/(steps/min) [U: I could not confirm the value].
- Per frame/step: speed = COM travel per second; cadence from foot-strike events; step length between successive
  heel strikes; walk ratio per step. Flag: speed changes made by cadence alone (legs spinning faster with the same
  steps) or by step length alone over a large range.

### C3. Dynamic similarity and the Froude number across body sizes

- Froude number Fr = v^2 / (g L). Bodies of different size moving at equal Fr move in dynamically similar ways
  (equal relative stride length, equal duty factor, equal phase relations) (Alexander 1976, 1984; Alexander & Jayes
  1983) [WE]. Humans change from walk to run near Fr = 0.5 (Kram, Domingo, Ferris 1997; Donelan & Kram 2000:
  Froude is "necessary but not sufficient" in reduced gravity) [WE]. The inverted-pendulum limit for walking is Fr = 1
  (the COM would leave the ground) [WE: physics].
- Alexander's relation: relative stride length (stride / hip height) about 2.3 x Fr^0.3 (Alexander 1976 Nature, as
  quoted by Ruiz & Torices 2013) [WE as an approximation].
- Hof 1996 (Gait Posture 4:222-223, "Scaling gait data to body size"): dimensionless speed v / sqrt(gL), dimensionless
  cadence f / sqrt(g/L), dimensionless step s / L; adult and child data collapse onto one curve this way, with caution
  under about 10 years (Carty & Bennett 2009) (https://clinicalgaitanalysis.com/faq/normalisation.html, read:
  secondary page) [WE].
- **Why this matters most for the game:** Small, Long and Round, and later any creator body, must walk, step and swing
  in dynamically similar ways to people of their size. A small body with a person's absolute cadence looks slow and
  floaty; a long body with a small body's cadence looks frantic. Per step: compute Fr, dimensionless cadence and
  dimensionless step for each miner and check they sit on the human curve (e.g. at Fr 0.2-0.3, relative stride about
  1.4-1.6 x L from Alexander's relation; inference from the formula). A walk above Fr 0.5 that is still a walk (always
  one foot down) is suspect; a walk at Fr near 1 is impossible.
- Time scales as sqrt(L/g) under dynamic similarity: a body with half the leg length should do the same gait event in
  about 0.71 of the time; joint angular speeds scale as sqrt(g/L) [WE: follows from the definition].

### C4. Arm swing with the legs

- Normal arm swing is opposite to the same-side leg, largely passive (pendulum) and cheap; holding the arms raised
  metabolic cost 12%, binding them 7%, swinging them in phase with the same-side leg (anti-normal) 26% over normal at
  1.25 m/s; peak vertical ground reaction moment was lowest with normal swing (3.0 N m vs 5.0 bound, 63% higher)
  (Collins, Adamczyk, Kuo 2009, Proc R Soc B 276:3679-3688, "Dynamic arm swinging in human walking",
  https://pmc.ncbi.nlm.nih.gov/articles/PMC2817299, read: full) [1S, checked; consistent with Herr & Popovic 2008].
- Arm-to-stride frequency changes from about 2:1 at slow speeds (below about 0.8 m/s) to 1:1 at normal speeds
  (Webb et al., Wagenaar & van Emmerik 2000, via an encyclopedia page) [U: secondary].
- Arm swing amplitude grows with speed; the elbow contributes more of the change than the shoulder (2025 study,
  secondary) [U].
- Per frame: shoulder flexion/extension angle in the sagittal plane vs. the opposite hip's: correlation should be
  strongly positive (opposite arm with leg); relative phase near 180 deg to the same-side leg. Flag: arms rigid at the
  sides during walking (looks like "held" or "bound"), arms swinging with the same-side leg, or both arms in phase
  (twinning).

### C5. Head stabilisation in walking

- The head's pitch is held within a few degrees during walking and running despite large vertical head travel; head
  pitch partly counter-rotates against vertical head translation to keep gaze stable (Pozzo, Berthoz, Lefort 1990,
  Exp Brain Res 82:97-106; replications reviewed in later work; a review quotes "<=20 deg" as the general claim)
  [WE for "few degrees" in walking]. With vestibular loss the stabilisation degrades (Pozzo et al. 1991) [1S].
- In turns the head leads (turning file) [WE].
- Per frame: head pitch and roll in world, and the head's angular speed; compare with the trunk's. Flag: the head
  rigidly following the trunk's bob and rock (no stabilisation), or head pitch range > ~10 deg per stride in a plain
  walk (inference from "a few degrees").

### C6. Trunk and pelvis counter-rotation

- Pelvis and thorax rotate about the vertical axis in nearly the same phase at slow walking (relative phase about
  -20 deg) and increasingly opposite as speed rises (about -140 deg at 1.5 m/s) (Lamoth et al. 2002 Gait Posture;
  review in J Hum Kinet 2021; van Dieën et al. 2021 caution that the phase depends on several factors)
  (https://pmc.ncbi.nlm.nih.gov/articles/PMC7877281) [WE].
- Pelvis and thorax contribute under 10% of whole-body transverse angular momentum; legs and arms about 90%
  (Bruijn et al. 2008) [1S].
- Per frame: pelvis and thorax yaw in world; relative phase via Hilbert transform or by the timing of their peaks per
  stride. Flag: trunk welded to the pelvis at normal speeds ("en bloc", also the turning file's Parkinsonian mark),
  or a trunk counter-rotating at very slow speeds.

### C7. Minimum-jerk reaching

- Point-to-point hand movements: nearly straight hand paths with bell-shaped speed; the minimum-jerk profile is
  x(t) = x0 + (x1 - x0)(10 s^3 - 15 s^4 + 6 s^5), s = t/T; peak/mean speed 1.875; human reaching about 1.75 (Flash &
  Hogan 1985, J Neurosci 5:1688-1703; reviews) [WE]. Alternatives (minimum torque change) predict slightly curved
  paths that sometimes fit better for large movements (Todorov & Jordan 1998 and others) [WE that both have support].
- Per action: hand path straightness (path length / chord, near 1.0-1.1), speed peak near mid-movement, peak/mean
  1.6-1.9. Flag: constant-speed reaches (1.0), sharp starts/stops, or many speed peaks (corrective sub-movements
  belong to aiming at small targets, not to every reach).

### C8. Reaction times to a push or a stumble

- Automatic postural (ankle) muscle responses start about 80-140 ms after a support perturbation; compensatory steps
  start as early as 160 ms and on average about 250 ms (force-plate onset) (McIlroy & Maki 1993; review of their work)
  [WE].
- In trips, the earliest muscle responses come below voluntary latency but do not decide the strategy; functional,
  strategy-specific responses start beyond about 110 ms (J Neurophysiol 2015, PMC4760462, read: abstract) [1S];
  vestibular-driven leg responses can appear by about 60 ms [1S].
- Simple voluntary visual reaction is about 180-250 ms (textbook figure) [U: not re-checked here].
- Viewers reject push reactions that are more than 150 ms early or late (Hoyet 2012) [1S].
- Per event: time from the impulse (first contact frame of a shove) to (a) first change in joint drive targets,
  (b) first foot lift. Flag: any purposeful response earlier than about 60-80 ms (a body that knows the push is
  coming - "answers sooner than a person could"), a step lift earlier than ~150 ms, or no response by ~300 ms to a
  push that moves the XcoM out of the BoS. Note: the purely passive response (joints yielding, the body moving under
  the shove) starts at once, as physics, and is not a "reaction".

### C9. Muscle-limited speed and power, and how they slow with size

- Muscle force falls as shortening speed rises (Hill's force-velocity relation); power peaks at about one third of
  maximum shortening speed (Hill 1938) [WE, textbook, not re-opened].
- Force scales with cross-sectional area (about M^0.67 for geometrically similar bodies) while weight scales with M,
  so strength per weight falls as size grows; large animals compensate with straighter limbs and better leverage
  (effective mechanical advantage rising about W^0.25 in mammals) (Biewener 2005, J Exp Biol 208:1665-1676,
  "Biomechanical consequences of scaling", https://cob.silverchair.com/jeb/article-split/208/9/1665/9381/, read:
  abstract and summaries) [WE].
- Smaller animals do each movement faster, their muscles having higher intrinsic speed; among similar animals top
  speed tends to be size-independent (Hill 1950, "The dimensions of animals and their muscular dynamics", as reported
  in Nature 164:820, https://www.nature.com/nature/journal/v164/n4176/abs/164820b0.html, read: abstract) [WE].
- Two scaling rules for joint speed exist: Froude scaling (angular speed ~ L^-0.5) and Hill's same-linear-speed
  scaling (~ L^-1); real animals lie between [U: which fits a creator body best is open]. The project memory "strength
  from muscle, slower with size" (Nov 3) already asks for size-slowed joints; these are the references for the slope.
- Per frame: per joint angular speed against a body-specific cap omega_max(L, muscle); per action, the required
  torque vs the body's available torque. Flag: a small body as slow as a large one or a large body as quick as a small
  one at the same task (judge in dimensionless time t*sqrt(g/L)).

### C10. Tiredness

- After fatiguing exercise healthy adults widen their steps, lower knee joint moments and change joint angles
  (systematic review 2025, 11 studies, https://www.ncbi.nlm.nih.gov/pmc/articles/PMC11939146/) [WE-ish]; older adults
  show longer and wider strides, more stride-to-stride trunk acceleration variability and more ankle co-activation
  (PLoS ONE 2019 systematic review, 11 studies, https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6936857/) [WE-ish:
  directions differ between studies].
- Weight shifts and fidgets in standing rise with time and discomfort (Rekant 2019, Wiggermann 2013, turning file)
  [1S each].
- Per stretch of play: step width, stride variability (coefficient of variation), trunk sway variability, frequency
  of weight shifts, all as functions of the body's work-tire-rest state. Flag: a tired body moving identically to a
  fresh one, or tiredness shown only as slower playback.

### C11. Carrying a load

- With heavier loads people take more steps per minute, spend longer in double support, flex the knee more and lean
  the trunk forward; a backpack shifts the COM back and the trunk leans forward to bring it over the feet (Harman et
  al., US Army report ADA377886, https://apps.dtic.mil/sti/pdfs/ADA377886.pdf; Penn State 20% body-weight study;
  10-20% BW studies; read: abstracts and summaries) [WE]. Exact lean angle per load: not found.
- Physics rule underneath: the combined COM (body + load) must stay over the BoS; a load in front leans the body
  back, a load on one side leans the body away, a load behind leans it forward [WE: physics].
- Per frame: combined COM of body and held load; its ground projection relative to the BoS; trunk lean angle vs the
  load's COM offset. Flag: a body that carries a heavy load with an unloaded posture (no lean, same cadence, same
  double support) - the load floats (A2.16).

### C12. Swinging a heavy tool

- Throwing and striking use a proximal-to-distal sequence: peak angular speeds come in order pelvis, trunk, upper
  arm, forearm, hand, each segment slowing as the next speeds up (the "kinematic sequence"; golf, Neal et al. 2007;
  throwing) [WE in sport]; but not every strike follows it (the taekwondo axe kick does not, Preuschl et al. 2016,
  J Sports Sci Med) [1S].
- Hammering at 1-5 Hz with 0.22-0.40 kg hammers: hammer speed on the way up is bell-shaped and on the way down a
  truncated bell (cut by impact); swing height falls as frequency rises; at 1 Hz people pause after each impact; a
  short latency after impact before the lift was not captured by the authors' model, perhaps arm compliance
  (Petrič, Simpson, Ude, Ijspeert 2017, Front Comput Neurosci 11:45, "Hammering does not fit Fitts' law",
  https://frontiersin.org/articles/10.3389/fncom.2017.00045/full, read: full) [1S, checked]. People pick impact speeds
  that trade task against effort [1S].
- The wrist moves on an oblique path from radial extension to ulnar flexion (dart-thrower's path) in hammering
  (Leventhal et al. 2010, https://pmc.ncbi.nlm.nih.gov/articles/PMC2901240, read: abstract) [1S]. Heavier hammers
  raise distal joint moments most (OpenSim study, abstract) [1S].
- Wind-up, follow-through and recoil for heavy tools (sledgehammer, pick, axe): no biomechanics study found with
  numbers [gap]. Physics says: a heavy head must be accelerated over a longer path (bigger backswing) for the same
  speed; after impact the head's momentum is mostly given to the work, the rest rebounds through the handle into the
  hands; a miss carries the tool through and pulls the body forward (needs a step or a brace) [WE: physics].
- Per swing: order of peak angular speeds (pelvis, trunk, shoulder, elbow, wrist); backswing duration vs downswing;
  feet planted and COM over BoS at impact; after impact, a rebound velocity of the head and a brief arm give.
  Flag: arm-only swings of a heavy tool with a still trunk, the tool stopping dead at impact with no rebound or
  shock in the arms, an instant reverse at the top of the backswing (no deceleration), and heavy and light tools swung
  with the same timing.

### C13. Standing stillness and micro-motion

- Centre of pressure mean speed in quiet stance, young adults eyes open, about 9.2 +/- 1.6 mm/s (100 adults, 19-28 y;
  J Hum Kinet "Posturographic standards", read: abstract) [1S; Prieto et al. 1996 found COP mean speed the most
  sensitive measure to age and vision, WE].
- Sway power lies mostly below 2 Hz (about 90% AP, 95% ML; 29 young men), the COP's dominant frequency below about
  0.4 Hz (Soames & Atha 1982; Frontiers in Neurology 2022) [WE]. Quiet stance behaves like a stiff-controlled inverted
  pendulum (Winter et al. 1998, J Neurophysiol, "Stiffness control of balance in quiet standing",
  https://pubmed.ncbi.nlm.nih.gov/9744933/) [WE, debated in detail].
- The COM sways less than the COP (the COP oscillates about the COM to steer it) [WE: physics, Winter].
- Breathing: adults breathe 12-20 times a minute at rest (clinical norm; one physiology source gives 10-14) with about
  0.5 L tidal volume (NCBI Bookshelf NBK365) [WE]. Each breath moves the trunk and legs about 0.5 deg, and these
  movements cancel so the COP barely follows the breath (coherence below 0.2); deeper breathing gives larger movement
  (Hodges, Gurfinkel, Brumagne, Smith, Cordo 2002, Exp Brain Res, "Coexistence of stability and mobility in postural
  control", https://link.springer.com/doi/10.1007/s00221-002-1040-x, read: abstract) [1S, supported by a 2024
  systematic review].
- Shifts and fidgets: about every 1-5 min for shifts, ~1 min for fidgets, irregular (turning file, Duarte &
  Zatsiorsky 1999) [1S].
- Per frame: COM ground-projection speed and its spectrum; ankle/hip angle oscillation; chest expansion cycle. Flag:
  sway at > 1 Hz dominant (a jittery idle), sway speed several times the human 9 mm/s COP figure (a wobbly idle),
  no sway at all (dead), breathing that visibly moves the COP or the whole body (it should cancel), breathing faster
  than ~20/min at rest without exertion. Note the project memory "breath gentle and scaled" (Oct 10) and "Luis loves
  the idle" (Nov 13): these numbers are a check, not a reason to enlarge what Luis already likes.

---

## Part D. The ten measures most useful for this game (ranked)

Ranked by how often the fault appears in physics-driven bodies, how cheap the per-frame computation is, and how
strongly viewers react. All thresholds are starting values (inference), to be tuned on the game's own footage.

1. **Planted-foot slide and spin** per stance phase (B1): travel > 2 cm or speed > 0.2 m/s while loaded. Viewers see
   ~21 mm [1S]; the convention in research is 2.5 cm/frame at < 5 cm height [WE].
2. **Margin of stability** (XcoM vs BoS, B7): flags stepping from nowhere, standing with b < 0 held by hidden
   forces, and catch steps that do not catch [WE].
3. **Energy and momentum residual** (B9): dE/dt minus joint and contact power; in flight, COM acceleration vs -g and
   angular momentum change. Catches the engine adding energy, helper forces and floatiness in one number [WE physics].
4. **Joint angles against anatomical ranges** in swing-twist (C1, B5): frames outside, time within 10 deg of a limit,
   hinge sideways bend, hyperextension below -5 deg [WE for hinge norms; U for some ball joints].
5. **Per-joint angular speed, acceleration and jerk spikes** per rendered frame, against a size-scaled speed cap
   (B3, C9): pops, twitches, IK flips [WE that jerk is the right quantity; thresholds U].
6. **Dimensionless gait** (C2, C3): Froude number, dimensionless cadence and step, walk ratio per miner; flags small
   bodies too slow, long bodies too frantic, and speed changed by cadence alone [WE].
7. **Response latency to a shove** (C8, B11): time from impulse to first purposeful response (> ~80 ms) and to step
   lift (~160-250 ms); viewers reject > 150 ms timing errors [WE / 1S].
8. **COP inside BoS and contact forces human** (B8): COP outside BoS, pulling feet, friction exceeded; vertical
   force share on the foot about to lift [WE physics].
9. **Arm-leg phase and pelvis-thorax phase in walking** (C4, C6): opposite arm with leg; trunk counter-rotating more
   as speed rises; head pitch held (C5) [WE].
10. **Idle micro-motion statistics** (C13, A2.8-10): COM sway speed and spectrum (< 2 Hz, dominant < 0.4 Hz), left-
    right asymmetry, coefficient of variation of shift intervals (~0.5-1.0), self-penetration count (B6) as a guard
    [WE / 1S].

Close behind: held-object force through the grip and load lean (A2.15-16, C11); bone-length constancy (B4, cheap,
catches render bugs, 3% visible) [1S].

---

## Part E. What I could not find, and what is unclear

- **No perceptual thresholds for twitch size** in degrees per frame or jerk units for game characters (also a gap in
  the twitches file). Only foot slide (21 mm), limb length (3%), push timing (150 ms) and ballistic velocity trends
  have numbers.
- **No biomechanics numbers for heavy-tool swings** (sledgehammer, pick, axe): wind-up angle, timing, follow-through
  and rebound. Only light-hammer studies (0.2-0.4 kg) and sports sequencing.
- **No load-carriage lean angle per % body weight** in a source I could open; only the direction and the gait changes.
- **No authoritative open table for neck, spine, shoulder rotation, wrist, hip rotation and ankle inversion ranges**;
  AAOS values are given from secondary sources and marked [U]. Neck norms differ by device and study.
- **Walk-ratio value** (about 0.0055-0.0065 m per step/min) not confirmed.
- **Arm-swing amplitude in degrees vs speed**, and the 2:1 to 1:1 frequency change, only from secondary sources.
- **Head pitch range in degrees per stride** in walking: only "a few degrees".
- **Simple visual reaction time** not re-checked (textbook 180-250 ms).
- **Which joint-speed scaling rule (Froude L^-0.5 or Hill L^-1) fits creator bodies** is open; it is a design choice
  with physics on both sides.
- **Full texts** of Prazak 2011, Harrison 2004, Reitsma 2003, Ren 2005, Hof 2005, Herr & Popovic 2008, Hoyet 2012
  and Hodges 2002 were not opened; their numbers are from abstracts or index pages.
- PhyMotion's self-penetration baselines (2% / 20%) and HumanScore's exact metric formulas: not in what I could read.
- No source found for industry practice in measuring "floatiness"; the detector here is physics (B9, A2.11).

---

## Sources (56 entries, about 65 works)

Read: full / abstract / secondary / book; see "How to read this".

Animation craft and game practice
1. Thomas F., Johnston O. 1981. *The Illusion of Life: Disney Animation*. Book.
2. Lasseter J. 1987. Principles of traditional animation applied to 3D computer animation. SIGGRAPH Computer Graphics 21(4):35-44. https://www.evl.uic.edu/aej/527/papers/lasseter.pdf (abstract/index)
3. Williams R. 2001. *The Animator's Survival Kit* ("Symmetry or Twinning", p. 324). Book (table of contents seen).
4. Animation Mentor. Why all animators need to master the moving hold. https://animationmentor.com/blog/why-all-animators-need-to-master-the-moving-hold (secondary)
5. Animator Island. Defining the art: twinning. https://www.animatorisland.com/defining-the-art-twinning/ (secondary)
6. Polycount / Blender Artists / Escape Studios threads on knee pops and soft IK. https://polycount.com/discussion/comment/2363642 ; https://escapestudiosanimation.blogspot.com/2023/11/how-to-fix-knee-pops-in-walk-cycle.html (practice)
7. Johansen R. S. 2009. Automated semi-procedural animation for character locomotion (thesis). https://runevision.com/thesis/rune_skovbo_johansen_thesis.pdf (via the turning file)
8. GDC session listings: Fitting the World: A Biomechanical Approach to Foot IK (https://gdcvault.com/play/1023009/); IK Rig: Procedural Pose Animation (https://gdcvault.com/play/1022984/); Rosen D., An Indie Approach to Procedural Animation; Clavet S. 2016 Motion Matching (Ubisoft). (listings only)
9. 80.lv. This simple tip can make your character animations feel more natural. https://80.lv/articles/this-simple-tip-can-make-your-character-animations-feel-more-natural (practice)

Perception of motion errors
10. Pražák M., Hoyet L., O'Sullivan C. 2011. Perceptual evaluation of footskate cleanup. SCA. https://diglib.eg.org/handle/10.2312/SCA.SCA11.287-294 (abstract)
11. Kovar L., Schreiner J., Gleicher M. 2002. Footskate cleanup for motion capture editing. SCA. https://research.cs.wisc.edu/graphics/Gallery/kovar.vol/Cleanup (abstract)
12. Harrison J., Rensink R., van de Panne M. 2004. Obscuring length changes during animated motion. TOG 23(3):569-573. https://www.cs.ubc.ca/labs/imager/tr/2004/harrison-ea-2004-obscuring (abstract/page)
13. Reitsma P., Pollard N. 2003. Perceptual metrics for character animation: sensitivity to errors in ballistic motion. TOG 22(3):537-542. https://ri.cmu.edu/?p=8709 (abstract)
14. Reitsma P., Andrews J., Pollard N. 2008. Effect of character animacy and preparatory motion on perceptual magnitude of errors in ballistic motion. CGF 27(2). https://diglib.eg.org/handle/10.2312/CGF.v27i2pp201-210 (abstract)
15. Ren L., Patrick A., Efros A., Hodgins J., Rehg J. 2005. A data-driven approach to quantifying natural human motion. TOG 24(3). https://www.ri.cmu.edu/publications/a-data-driven-approach-to-quantifying-natural-human-motion (abstract)
16. Hoyet L., McDonnell R., O'Sullivan C. 2012. Push it real: perceiving causality in virtual interactions. TOG 31(4):90. https://history.siggraph.org/?p=110569 (abstract)
17. McDonnell R., Newell F., O'Sullivan C. 2007. Smooth movers: perceptually guided human motion simulation. SCA. https://diglib.eg.org/handle/10.2312/SCA.SCA07.259-270 (abstract)
18. Hirose J. et al. 2020. Integrated jerk as an indicator of affinity for artificial agent kinematics. PeerJ 8:e9843. https://www.ncbi.nlm.nih.gov/pmc/articles/PMC7500322/ (abstract)

Measures from motion generation and simulation
19. Karunratanakul K. et al. 2023. Guided motion diffusion for controllable human motion synthesis. ICCV. https://ar5iv.labs.arxiv.org/html/2305.12577 (full, metric section)
20. NVIDIA. Kimodo benchmark metrics documentation. https://research.nvidia.com/labs/sil/projects/kimodo/docs/benchmark/metrics.html (full)
21. Yuan Y. et al. 2023. PhysDiff (via the twitches file). https://arxiv.org/pdf/2212.02500
22. HumanScore: Benchmarking human motions in generated videos. 2026. arXiv 2604.20157. https://cs.stanford.edu/~xtiange/projects/humanscore/ (project page)
23. PhyMotion: Structured 3D motion reward for physics-grounded human video generation. 2026. arXiv 2605.14269 (secondary; PDF not readable)
24. Motion generation: a survey of generative approaches and benchmarks. 2025. arXiv 2507.05419 (via the twitches file and search summary)
25. Yuan Y., Kitani K. 2020. Residual force control for agile human behavior imitation and extended motion synthesis. NeurIPS. https://arxiv.org/abs/2006.07364 (abstract)
26. Liu C. K., Popović Z. 2002. Synthesis of complex dynamic character motion from simple animations. TOG 21(3):408-416. https://grail.cs.washington.edu/projects/charanim/phylter.html (abstract)
27. Chen et al. 2020. From kinematics to dynamics: estimating center of pressure and base of support from video frames of human motion. arXiv 2001.00657 (abstract)
28. Tripathi S. et al. 2023. 3D human pose estimation via intuitive physics (IPMAN). CVPR. https://openaccess.thecvf.com/content/CVPR2023/papers/Tripathi_3D_Human_Pose_Estimation_via_Intuitive_Physics_CVPR_2023_paper.pdf (abstract)
29. Han X. et al. 2023. GroundLink: a dataset unifying human body movement and ground reaction dynamics. SIGGRAPH Asia. https://cs-people.bu.edu/xjhan/pubs/GroundLink.pdf (abstract)
30. Human motion data alone does not guarantee ... (kinetics-aware gait imitation). 2026. arXiv 2603.12408 (abstract)
31. Liu S. et al. 2023. ContactGen: generative contact modeling for grasp generation. arXiv 2310.03740; Jiang H. et al. 2021. Hand-object contact consistency reasoning for human grasps generation (GraspTTA). arXiv 2104.03304 (search summaries)
32. Balasubramanian S. et al. 2015. On the analysis of movement smoothness. JNER (via the twitches file). https://pmc.ncbi.nlm.nih.gov/articles/PMC4674971

Balance and whole-body mechanics
33. Hof A. L., Gazendam M., Sinke W. 2005. The condition for dynamic stability. J Biomech 38:1-8. https://braceworks.ca/wp-content/uploads/2016/05/hof-condition-for-dynamic-stability.pdf (abstract)
34. Curtze C., Buurke T., McCrum C. 2024. Notes on the margin of stability. J Biomech. https://cris.maastrichtuniversity.nl/en/publications/notes-on-the-margin-of-stability/ (abstract)
35. Herr H., Popovic M. 2008. Angular momentum in human walking. J Exp Biol 211:467-481. https://aspirin.media.mit.edu/biomechatronics/wp-content/uploads/sites/8/2013/07/Herr-2008_Angular-momentum-in-human-walking.pdf (abstract)
36. McIlroy W., Maki B. 1993 (and review of their work). Compensatory stepping onset. https://experts.nau.edu/en/publications/anticipatory-postural-adjustments-and-the-latency-of-compensatory/ (secondary)
37. Trip-recovery responses, J Neurophysiol 2015. https://pmc.ncbi.nlm.nih.gov/articles/PMC4760462 (abstract)

Gait and scaling
38. Alexander R. McN. 1976 (Nature) and 1984; Alexander & Jayes 1983; relation as quoted in Ruiz & Torices 2013. https://www.originalwisdom.com/wp-content/uploads/bsk-pdf-manager/2019/03/Ruiz-and-Torices_2013_Humans-running-at-stadiums-and-beaches-and-the-accuracy-of-speed-estimation-from-fossil-trackways.pdf (secondary)
39. Kram R., Domingo A., Ferris D. 1997. Effect of reduced gravity on the preferred walk-run transition speed. J Exp Biol. https://www.sfu.ca/locomotionlab/assets/dynsimwalk1997.pdf ; Donelan J. M., Kram R. 2000 J Exp Biol 203:2405 (abstract)
40. Hof A. L. 1996. Scaling gait data to body size. Gait Posture 4:222-223; formulas at https://clinicalgaitanalysis.com/faq/normalisation.html (secondary)
41. Bohannon R., Williams Andrews A. 2011. Normal walking speed: a descriptive meta-analysis. Physiotherapy 97:182-189. https://pubmed.ncbi.nlm.nih.gov/21820535/ (abstract)
42. Herssens N. et al. 2018. Do spatiotemporal parameters and gait variability differ across the lifespan of healthy adults? Gait Posture (systematic review) https://repository.uantwerpen.be/link/irua/152210 ; Aboutorabi A. et al. 2016 https://pubmed.ncbi.nlm.nih.gov/26210370/ (abstracts)
43. Sekiya N., Nagasaki H. 1998 (walk ratio); Murakami R., Otaka Y. 2017. Estimated lower speed boundary at which the walk ratio constancy is broken. J Phys Ther Sci. https://pmc.ncbi.nlm.nih.gov/articles/PMC5430280 (abstract)
44. Collins S., Adamczyk P., Kuo A. 2009. Dynamic arm swinging in human walking. Proc R Soc B 276:3679-3688. https://pmc.ncbi.nlm.nih.gov/articles/PMC2817299 (full)
45. Pozzo T., Berthoz A., Lefort L. 1990. Head stabilization during various locomotor tasks in humans I. Exp Brain Res 82:97-106 (secondary/reviews)
46. Lamoth C. et al. 2002 Gait Posture; Bruijn S. et al. 2008 Gait Posture; review of axial trunk rotations, J Hum Kinet 2021. https://pmc.ncbi.nlm.nih.gov/articles/PMC7877281 ; van Dieën J. et al. 2021 (abstracts/secondary)
47. Flash T., Hogan N. 1985. The coordination of arm movements: an experimentally confirmed mathematical model. J Neurosci 5:1688-1703 (secondary/reviews)

Muscle, size, load, tiredness, tools, standing
48. Hill A. V. 1950. The dimensions of animals and their muscular dynamics (Nature 164:820 report). https://www.nature.com/nature/journal/v164/n4176/abs/164820b0.html ; Biewener A. 2005. Biomechanical consequences of scaling. J Exp Biol 208:1665-1676. https://cob.silverchair.com/jeb/article-split/208/9/1665/9381/ (abstracts)
49. Harman E. et al. (US Army) load carriage biomechanics. https://apps.dtic.mil/sti/pdfs/ADA377886.pdf ; Penn State backpack position study https://pure.psu.edu/en/publications/effects-of-backpack-load-position-on-spatiotemporal-parameters-an/ (abstracts)
50. Fatigue and gait: PLoS ONE 2019 systematic review https://www.ncbi.nlm.nih.gov/pmc/articles/PMC6936857/ ; Bioengineering 2025 systematic review https://www.ncbi.nlm.nih.gov/pmc/articles/PMC11939146/ (abstracts)
51. Petrič T., Simpson C., Ude A., Ijspeert A. 2017. Hammering does not fit Fitts' law. Front Comput Neurosci 11:45. https://frontiersin.org/articles/10.3389/fncom.2017.00045/full (full); Leventhal E. et al. 2010 (hammering wrist) https://pmc.ncbi.nlm.nih.gov/articles/PMC2901240 ; Neal R. et al. 2007 golf sequencing; Preuschl E. et al. 2016 J Sports Sci Med https://jssm.org/jssm-15-92.xml-Fulltext (abstracts)
52. Prieto T. et al. 1996. Measures of postural steadiness. IEEE TBME; Winter D. et al. 1998. Stiffness control of balance in quiet standing. J Neurophysiol https://pubmed.ncbi.nlm.nih.gov/9744933/ ; Posturographic standards, J Hum Kinet https://jhk.termedia.pl/Posturographic-Standards-for-Optimal-Control-of-Human-Standing-Posture,159452,0,2.html (abstracts)
53. Hodges P. et al. 2002. Coexistence of stability and mobility in postural control: evidence from postural compensation for respiration. Exp Brain Res. https://link.springer.com/doi/10.1007/s00221-002-1040-x (abstract)
54. NCBI Bookshelf NBK365 (respiratory rate, tidal volume). https://ncbi.nlm.nih.gov/books/NBK365 (secondary)
55. Thoomes-de Graaf M. et al. 2020. Normative values of cervical range of motion for both children and adults: a systematic review. Musculoskelet Sci Pract. https://epublications.regis.edu/facultypubs/98 (abstract)
56. Soucie J. et al. 2011. CDC Joint Range of Motion study (via the twitches file). https://archive.cdc.gov/www_cdc_gov/ncbddd/jointrom/index.html

(56 numbered entries, several holding a pair of closely linked papers; about 65 distinct works. Opened in full or in
the relevant section: 19, 20, 44, 51 (Petrič), plus the abstract pages of most others.)
