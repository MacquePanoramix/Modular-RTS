# Independent review of catalogue.md: working notes

Reviewer: independent agent (did not write the catalogue). Started 2026-11-13.
These notes are kept as the review goes so the work survives an interruption.
The final list of changes goes into catalogue.md, section "Review".

## Progress
- [x] read catalogue.md
- [x] read sceptic_notes.md
- [x] read U3 (Luis's faults), U1, U2 for completeness
- [x] spot-check file:line claims in D:/Dev/WG (read-only)
- [x] apply corrections, write Review section

## Findings (as found)

Read: catalogue.md (all), sceptic_notes.md (all), U3 (all).

### Detector spot-checks (read-only)
- Main-copy test lines cited in the catalogue: ALL 21 test names checked by grep; main lines correct.
  In D:/Dev/WG (where the benches run, branch claude/wip-round, 957a21b) OwnBodyTests lines are +141
  (:93->:234, :141->:282, :236->:377, :263->:404, :299->:440, :417->:558, :501->:642, :527->:668, :764->:908);
  GroundedBody, PhysicalFallTests, MinerTests, LivingBodyTests same lines in both.
- HungThingTests :254, :327, :397 exist only in the main copy; D:/Dev/WG has only :62 and :199.
- motion_breaks.py:24-28 bars correct in both copies. BUT: SLIDE is checked only while the foot is FLAT
  (|pitch| < 1.5 deg, motion_breaks.py:90-ish "flat"): a foot sliding while heel or toe is up is not checked -> C1 miss
  in pivots on the ball. "planted" is the walk's own flag (plantedL/R), not a physics contact.
- motion_breaks.py already reports each of root/hips/chest/head's fastest yaw speed and the times each has turned
  10% and 90% of a turn over 60 deg ("turn_began"): partial detectors for T7, O1, O2 (catalogue says "none").
- motion_jolts.py, motion_dips.py, motion_frames.py ARE in the main copy's Art/Review (gap 8 says not in this copy:
  wrong). They are missing from D:/Dev/WG (U3 right for WG). motion_jolts covers hands too (S1 for hands).
- TurnBench is in NO repository copy: a reviewed draft in a session scratchpad (Handoff.md:525-553). Catalogue
  gap 8 says "U3 read them in D:/Dev/WG": wrong for TurnBench. Its F1-F6 are bars written in Handoff.md:526-530.
- FrameRecorder.cs (D:/Dev/WG only, 239 lines): records drawn bones (15), solved segments (13), physics parts (13:
  pose, angular velocity in WORLD, joint positions). Faults (:147-191): "spin" drawn limb >30 deg in a frame (:162),
  "jump" drawn hand/foot >6 cm in a frame (:168), "wrongway" elbow/knee bent backwards >8 deg (:~186), "overfold"
  >150 deg (:~187), "whip" part >30 rad/s world (:191). These cover parts of S1, S3, J1, J2 not mapped.
  No contacts, no COM, no forces recorded.
- ArticulationFactsTests.cs (WG only): :421 AFastLegGainsByTheStepItself (energy), :385 WhatAFastLegsGainComesOf,
  :1100 ADriveWrittenByTheShortestTurnCrossesAHalfTurn, :147 JointSpeedBoundHolds, :1624 solver turns.
- shoves.sh (WG tools only; main copy has no Docs/Technical/AnyBody/tools folder).
- PhysicalFall.cs:378-379 projection .02 m / 5 deg (main) - catalogue right; U3 :388 is WG.
- ProceduralBiped.cs:480 MostTwist=62 both copies.
- MotionTraceBench trace columns (main :92): positions of root/hips/head/feet/knees/hands/shoulders/tool, yaws of
  root/hips/chest/head/feet, pitches of feet/head/chest, planted flags, swing phase, etc. NO per-bone rotations
  (except listed yaws/pitches), NO contacts, NO centre of mass, NO forces, NO elbows. -> many measures need a new
  recorder; for the kinematic walk, COM needs assumed segment masses.

### Codability (data the game has)
- Kinematic walk: no contacts or forces exist. C1 "force-weighted centroid", "loaded" (20% weight), B4 share, B5,
  B6, W-family force measures cannot be computed there. Need a kinematic definition (planted flag + sole height).
- COM for kinematic walk: drawn bones have no masses -> needs a segment mass table (anthropometric) per body; say so.
- Contacts with impulses on ArticulationBody: needs Physics.ContactEvent subscription (not recorded today).
- Joint torques: ArticulationBody.jointForce / GetJointForces readable after a step; drive torque "estimates" ok.
- Physics poses interpolated to render time (V1): ArticulationBody has no interpolation in Unity 6? -> check claim.

### False alarms and misses (to apply)
- C1: force-weighted contact centroid = foot's own COP, travels heel->toe every normal step -> false alarm. Use
  material-point slip velocity (v + w x r) at contact, net drift from stance start (not summed path).
- S1/C5: dt^2 conversion of 4 mm makes 16 mm at 50 fps, 36 at 30 -> teleport passes. Drawn body at 100 fps
  (bench rate) + spike test at lower rates (2nd diff > 3x both neighbours and > 2 mm).
- S3/S2 per rendered frame: 10 rad/s tool swing = 19 deg/frame at 30 fps -> false alarm; parent frame, T8 cap x dt,
  exempt impact frame. Keep >90 deg always.
- B7: windows with weight shifts inflate COP speed -> use shift-free windows.
- L1/B7/L5/L6: PhysX COP noisy/quantised; calibrate noise on a held-still body; COM/drawn sway primary.
- T2: passive foot lift under hard shove -> count purposeful lifts only; controller must log mode changes.
- T7/T8: sqrt(1 m/L) inconsistent with 0.9 m leg -> sqrt(0.9/L).
- W2: angularDamping 0.6 (PhysicalFall.cs:335) loses ~6%/0.1 s -> 5% bar flags every fall; 10%, report damping.
- T8: maxAngularVelocity 20 (OwnBody.cs:535, PhysicalFall.cs:340) -> 30 rad/s whip never fires on physics parts;
  add "sat at clamp" frames.
- J4: pelvis yaw vs planted foot is not hip transverse rotation (Steingrebe); mark not like-for-like.
- B2: code-labelled event exempts a step -> events must be external or scheduled; judges see each.
- L2: needs >=10 intervals per kind (~50+ min idle); never raise the idle's rate.
- J1: needs per-rig zero-pose table; FrameRecorder 8 deg vs J1 5 deg.

### New kinds (verified by search where marked)
- C12 step width: Donelan, Kram, Kuo 2001, preferred 0.13 L, foot width 0.11 L (one study, abstract checked).
- C13 toe clearance: [U] not verified here (Winter 1992 ~1-2 cm, memory).
- S10 no arcs (U2 A1). T11 start/stop in one step [U]. O13 angular momentum cancel (Herr & Popovic 2008).
- W12 idle arms when balance goes (Jeon 2022). W13 fall without protective reaction: Hsiao & Robinovitch 1998
  (one study; active movement ~300 ms after onset per citing papers, ~700 ms to impact; wrist and pelvis ~38 ms).
- W14 glide/bounce/crouch: Orendurff 2004 (one study via citing paper: slow walk vertical ~half of normal).
- L10 life off outside the idle. H9 hand out of time with grip (Jörg 2010). V6 judder. V7 camera in body/ground.
- Set aside: face (no face animation), affect (not per-frame), slope (U, kept as flag).
- Done: catalogue edited, Review section appended (12 added, 0 struck).
