# Designed (non-learned) controllers for getting up, and robotics whole-body control for getting up

Scope: hand-designed feedback controllers for physically simulated characters and humanoid robots rising from the ground; what makes them robust or fragile; what transfers to a 13-part, ~40 kg PhysX ArticulationBody miner (big heavy head, short limbs) that must rise only by its own joint torques. The miner currently gets 1–8 of 36 tries up from prone, failing mostly when it rolls from hands-and-feet (bear stance) onto its feet, or when the standing controller first raises it.

Reading notes: "full text" means I read the paper's text (PDF converted to text). "Abstract only" or "secondary" is marked where it applies. Everything under "Inferences" is my reasoning, not a source claim.

---

## 1. Composable controllers (Faloutsos, van de Panne, Terzopoulos, SIGGRAPH 2001): how the rising controllers were built, their pre- and post-conditions, and how robust they were

### Takeaway
The rising controllers were ordinary pose-control finite state machines with timed transitions. Their PD "muscles" had joint-strength limits. The contribution was the wrapper around them: each controller declares a pre-condition (where in state space it can start), a post-condition (where it promises to leave the body) and an "expected performance" monitor that declares failure early so a different controller can take over. No success rate is reported for the rising controllers themselves. The only numbers are the accuracy of the learned pre-condition classifiers (85–100% for kneel and rise controllers).

### Cited Findings (full text read)
- Framework: a supervisor polls a pool of black-box controllers. Each returns a score: 0 for unsuited, 1–10 for "can do something sensible", 10–20 for "can guarantee reaching the desired state". The highest bidder becomes active — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Each controller must define pre-conditions, post-conditions and expected performance. Pre-conditions are built from: the initial-state region R(qi); environment terms (contact points, ground normal, friction); a balance term Cb, which tests the centre-of-mass (COM) projection and COM velocity against the support polygon S; and the target region — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Expected performance: "Only the individual controllers can detect whether they are operating normally or whether failure is imminent… The sooner a controller can detect failure the sooner another more appropriate controller can take over." — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- No explicit finite state machine links the controllers. Transitions happen implicitly as the state passes through the controllers' "regions-of-competency". For example, prone-to-standing hands over to the fall controller if the figure is pushed hard while rising — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Inside a controller: "Pose control is based on cyclic or acyclic finite state machines with time transitions between the states. Each state of the controller can be static or depend on feedback parameters." Balance controllers use continuous feedback instead — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Model: 37 DOF in 3D (6 of them for the root), anthropometric adult male. It has "natural limits both on the motion of the joints and the strength of their muscles". Control uses rotational spring-dampers, and joint limits use exponential springs. Simulation uses SD/FAST with RK4 integration and penalty contacts. The 3D model ran 5–9× slower than real time on a 733 MHz PIII — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Demonstrated chain: balance → fall → default → roll-over → prone-to-standing → balance. A second push produces similar but not identical behaviour because the initial conditions differ — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Rising from supine: "a surprisingly difficult motion to simulate. It involves rapid changes of the contact points and significant shifting of the figure's weight. In addition, the frictional properties of the ground model can influence the motion."
  - Pre-condition: lying back-flat within a tolerance.
  - Post-condition: "on its feet with its center of mass within the support polygon". A separate controller then brings it from crouching to standing.
  - Source: [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Rising from prone: the pre-conditions "can be fairly relaxed". The controller "assumes that it has the time to change the state of the character to one from which it knows how to rise". It needs the figure not to be lying on its arms and the ground to be fairly flat. Its post-conditions are chosen to satisfy the balance controller's pre-conditions — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Roll-over: the pre-condition is supine with no COM movement. The post-condition is any extended, fairly straight prone pose (no crossed limbs) — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Kip (dynamic rise): first moves to a suitable start pose, then a mostly ballistic phase with control focused at kick-off and landing, then "continuous control to bring the stuntman to an erect position from which the balance controller can take over" — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Balance controller pre-condition (Appendix A):
  - COM acceleration < 0.1 m/s² and COM velocity < 0.3 m/s.
  - COM projection inside S.
  - Posture error over thigh, knee and waist < 0.1 rad.
  - Post-condition: acceleration < 0.01 m/s² and velocity < 0.05 m/s.
  - It stiffens the ankles for small perturbations and actively actuates them for larger ones (inverted-pendulum ankle strategy).
  - Source: [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Learned pre-condition accuracy (SVM vs nearest neighbour). These measure how well the classifier predicts success; they are not controller success rates:

  | Controller | SVM accuracy |
  |---|---|
  | LyingOnBellyToKneel | 94.46% |
  | LyingOnBackToKneel | 100% |
  | KneelToStand | 85.06% |
  | BendToStand | 99.77% |
  | StepToStand | 87.29% |
  | StandInPlace | 87.67% |

  Training states came from 0.3 s of random external pushes on the pelvis plus random joint-target offsets. That external force was used only to generate start states, not to control the character — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Later critiques:
  - Jones: the controllers "are based on pose tracking and timed state transitions, making it difficult to transfer controllers to new characters or environments" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
  - Kenwright also criticises the fixed postures and timed transitions — [Kenwright 2023 (ar5iv)](https://ar5iv.labs.arxiv.org/html/2304.05056)

### Inferences
- Body fully responsible? Yes. Control is joint spring-dampers with strength limits, and no assist force acts during control.
- The architecture fits the miner's problem better than the rising controllers themselves do. The useful pattern is that each stage has:
  - an explicit pre-condition checked before it starts (COM over the support polygon, COM speed and acceleration small, posture close);
  - a post-condition that is deliberately made equal to the next stage's pre-condition;
  - a running failure monitor that hands control to a recovery or retry stage.
- The miner's "standing controller first raises it" failure looks like a pre-/post-condition mismatch: the hand-off state is outside the standing controller's region of competence. Faloutsos' supine riser explicitly stops at "on its feet, COM inside the support polygon, crouching". A separate controller then does crouch-to-stand. In other words, a deep-crouch balance state is its own stage with its own pre-condition.
- Robustness inside each controller still came from timed pose FSMs. Expect the same fragility seen in the miner wherever a phase relies on timing rather than feedback.

### Gaps
- The 2001 paper says the full controller details are in Faloutsos' PhD thesis ([York page PDF](https://www.eecs.yorku.ca/~pfal/Petros_Faloutsos/Publications_files/phdthesis.pdf)). I did not read it, so the exact key poses, timings and feedback terms of the prone and supine risers are unknown here.
- No success rate over repeated rises was found for the 3D rising controllers.

---

## 2. Jones 2011 UBC MSc thesis: rising controllers from pose tracking, root-orientation correction and virtual forces

### Takeaway
Jones built sit-to-stand (3D biped), quadruped get-ups (3D) and prone-to-stand (2D biped only) from three shared components:
- PD pose tracking with analytic two-link IK;
- "root orientation correction", where a world-frame torso PD torque is realised through the stance hips;
- Jacobian-transpose virtual forces that keep the "active COM" over the centre of support.

Robustness came from:
- a support graph whose phase pauses until stance limbs are in contact and swing limbs carry no weight;
- weight ramped off a limb (1 → 0 over 0.5 s) before it lifts;
- a stochastic optimisation that scores each controller by its success over 16 perturbed runs.

All actuation is internal joint torque. The weakest controller (quadruped lying-on-side to stand) was the one that ran largely open-loop.

### Cited Findings (full text read: [Jones 2011 PDF](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf); record at [UBC Open Collections](https://open.library.ubc.ca/collections/24/items/1.0052110))

**Pose tracking**
- τ = kp·E − kd·Ė, with errors evaluated as quaternions. Targets can be given in world or heading frame and converted to parent-relative.
- Arms and legs use analytic two-link IK. The leg's swivel plane is set relative to the pelvis, and the arm keeps the forearm as vertical as possible with the elbow away from the torso.

**Root orientation correction**
- A world-frame PD on the pelvis gives τ_root. The stance torque is τ_stance = τ_root − Σ τ_pose over the swing limbs, and it replaces the stance-hip PD torques (−τ_stance/n_stance each).
- In prone-to-stand, the error torque is clamped and subtracted from the stance-hip torques instead of replacing them: τ_hip ← τ_hip − τ_clamped/n_stance.
- Jones: "Ideally, this corrective torque will induce a tangential contact force from the ground". It "works well for small perturbations, but interferes with pose tracking at the hips joints and can cause feet to slip".
- These are internal hip torques. No external torque acts on the root.

**Virtual forces**
- τ = Jᵀ F. The base link is the torso; the application point is a hand or foot.
- Uses:
  - gravity compensation (virtual force −m·g at link COMs, so PD gains can stay low);
  - COM regulation in the coronal and sagittal planes;
  - leg-frame height.

**3D biped sit-to-stand** (17 links, ODE at 2 kHz, friction 0.8)
- An FSM of static target poses with timed transitions: Lean 0.9 s, Lift 0.5 s, then Stand.
- Root correction acts during Lift and Stand.
- A virtual-force PD drives the "active COM" — the COM of all bodies above the supporting limbs — to the centre of support: F = kp(x_com − x_des) − kd·v_com, per plane. The target is the pelvis centre when seated and the feet midpoint when standing.
- "Without this control, the character may fall sideways out of the chair, and cannot maintain balance while standing."
- Works unmodified for chairs of 45–65 cm. Below that range "the character does not generate enough momentum to stand, and falls backward."

**Quadruped** (gait-graph framework of Coros et al.)
- Naive keyframe-like gait graphs failed for sit-to-stand because "the center of mass is too far behind the rear feet". It was fixed by a shoulder dip plus a horizontal virtual force ramping 0 → 100 N over 0.2 s, then back to 0 over 0.6 s, "to generate forward momentum".
- "When the balance feedback fails … this naive approach is insufficient… These situations require an extra manual balance specification on top of the keyframe-like specification."

**Quadruped lying-on-side to stand**
- "Extremely difficult": "almost all of the feedback loops … expect the feet to be well planted below the character".
- It relies on explicit pose tracking and hand-specified virtual forces through the planted feet: front feet up to 350 N forward and 100 N up, rear feet 150 N forward and 50 N up. It hands over to the trot controller once the shoulders are above 25 cm.
- Jones calls it "the least robust, … often generating unnatural motions when transitioning to the trotting controller… the motion is largely open loop, so disturbances are not corrected quickly."
- The sitting and lying controllers, by contrast, were "extremely robust to disturbances", for example on or near an 8 cm block.

**2D prone-to-stand**
- JBox2D; 9 links, about 1.1 m and 25 kg, with most of the mass in the torso.
- The support graph gives limb modes (stance, swing, idle, and kneel with the knee as end-effector) plus hip and shoulder height trajectories over phase.
- "Our controllers can pause phase advancement until it is safe to proceed… safe means that stance limbs are in contact with the ground, and swing limbs are not supporting weight."
- Gains are kp = 35 and kd = 8 N·m/rad for all joints.
- Stance end-effector targets are chosen so the hip or shoulder sits at its desired height. The swing landing target is recomputed halfway through the swing.

**2D prone-to-stand: balance feedback**
- Gravity-compensation virtual forces act on the swing limbs. A height virtual force acts through the planted limbs.
- A torso corrective virtual force drives the torso centre to a weighted mean of the planted lower-limb centres.
- "Limb weights are linearly interpolated from 1 to 0 over the 0.5 s before their expected lift time… Additionally, the desired vertical position of the lifting limb's end effector is increased from 0 cm to 4 cm over the 0.5 s before lifting."

**2D prone-to-stand: hand-designed sequence**
1. Hands planted one at a time, 10 cm ahead of the shoulders (1 s each).
2. Legs move to kneeling stance; hips and shoulders raised to 35 cm and 25 cm.
3. Arms re-placed directly under the shoulders, one at a time (1 s each).
4. Right leg planted 30 cm in front of the hip, now standing on the foot; then the left leg.
5. Hips and shoulders raised to standing height while the arms go idle.

Total: 13.5 s.

**Optimisation**
- Greedy stochastic search over 67 parameters (timings, placements, 7 height knots each, 10 time-warp points), 16 evaluations per iteration.
- Duration went from 13.5 s to 1.2 s. "The fastest motions generated are quite unnatural and include violent collisions with the ground."
- Robustness score = fraction of 16 perturbed runs that succeed. The search continues until one parameter vector reaches ≥ 80%.
- Continuation extended the controller to slopes from −12° to +17°, and to a torso 1.45× larger with 2.10× the mass. The extremes needed thousands of evaluations.

**Conclusions and limits**
- "The main challenge … is controlling the center of mass position and orientation"; "planning support points is one of the most important aspects of any controller".
- "Especially for dynamic motions, a balance strategy must be anticipatory, planning for future contact configurations and considering character momentum."
- Limits: the prone controller is 2D only (3D "would likely require … an additional lateral balance feedback strategy"); seeds are hand-made; the optimiser cannot change the number or type of support changes.

### Inferences
- Body fully responsible? Yes, everywhere. The virtual forces are Jᵀ-mapped joint torques, and root correction is redistributed hip torque. Note that the quadruped "forces" of 350 N only exist if the feet are planted and do not slip, because they are internal torques that react against the ground.
- Closest analogue to the miner's gap: the step from "arms re-placed under the shoulders" to "leg planted 30 cm in front of the hip" while the torso corrective force is still pulled toward the remaining planted limbs. In the miner's terms:
  - The hands should be walked back under the shoulders, near the COM, before the feet take over.
  - The balance target should be a weighted mean of supports, ramped continuously as a limb is unloaded — not switched.
- The miner already has "weight shifted off a limb before it is lifted". Two parts of Jones' version may be missing from the miner:
  - the phase gate ("pause until swing limbs are not supporting weight"), which is state-triggered rather than timed;
  - the small pre-lift of the end-effector target (+4 cm), which proves the limb is unloaded before it actually leaves.
- Jones' robustness scoring (success over 16 perturbed runs) is directly reusable as the acceptance metric for the miner's searched poses. It is cheaper than the current 36-try evaluation and matches the project's "search the poses" practice.
- Effort in Unity C#: moderate. Unity's ArticulationBody exposes `GetDenseJacobian`, `GetJointGravityForces` and a `jointForce` field (reduced coordinates, N or N·m) — [Unity docs](https://docs.unity3d.com/ScriptReference/ArticulationBody.html), [jointForce](https://docs.unity3d.com/ScriptReference/ArticulationBody-jointForce.html). Cost per frame for 13 links is negligible.

### Gaps
- No 3D prone-to-stand biped result exists in the thesis.
- Root-correction gains, virtual-force gains and the clamp τ_max are not given numerically for the prone controller.
- No success rate is reported for unperturbed hand-designed seeds — only the 80% threshold on perturbed runs.

---

## 3. Kenwright 2023 (arXiv 2304.05056), "Real-Time Character Rise Motions": planning a rise from support polygons and hand placement

### Takeaway
A low-dimensional planner treats the body as a point mass with massless telescopic limbs. It keeps the COM inside the triangle formed by two feet and one support hand, and re-plants the free hand closer under the COM, repeating until the COM is over the feet. Only then are both hands released, and the legs extend vertically. It is static and slow by design, reports no success rates, and the paper's own model descriptions have inconsistencies. The core rule — walk the hands back until the COM is over the feet, then release — maps directly onto the miner's bear-stance failure.

### Cited Findings (read via ar5iv HTML and the arXiv abstract)
- The abstract claims a low-dimensional controller that takes the COM and hand and foot locations as inputs, uses an approximate model "similar to the inverted-pendulum stepping model", runs in real time, and uses "no key-framed data and no expensive offline optimization or search" — [arXiv abstract](https://arxiv.org/abs/2304.05056)
- Model in the body text: "a particle-mass with weightless telescopic arms and legs". The stability criterion is ZMP = projected COM, for slow static motion. The support region is the triangle of two feet plus one support hand — [Kenwright 2023 (ar5iv)](https://ar5iv.labs.arxiv.org/html/2304.05056)
- Phases (all from [Kenwright 2023 (ar5iv)](https://ar5iv.labs.arxiv.org/html/2304.05056)):
  - **Phase 0**: roll to front or side; place four limb targets (no crossed limbs).
  - **Phase 1**: release one hand and optimise the simple model. "If the CoM is not over the feet, the free hand moves to the side of the body at the CoM location, becomes the new support hand", then repeat.
  - **Phase 2**: release both hands only when the COM is over the feet; extend vertically keeping the COM over the feet.
- Tracking: analytic two-link IK as in Coros et al. 2010, with an elbow and knee swivel-plane parameter. In 3D the hip midpoint is used as the COM. PD joint torques τ = kp(θd − θ) − kd·θ̇ with hand-tuned gains. A secondary IK layer adds style (sway, looking around) — [Kenwright 2023 (ar5iv)](https://ar5iv.labs.arxiv.org/html/2304.05056)
- Feasibility rule: the upper body must be longer than the lower body but "less than twice the length of the lower body" — [Kenwright 2023 (ar5iv)](https://ar5iv.labs.arxiv.org/html/2304.05056)
- Simulation at 100 fps; controller plus simulation under 3 ms (i7-2600). Physics engine not named. It engages after linear and angular velocities fall below an unstated threshold — [Kenwright 2023 (ar5iv)](https://ar5iv.labs.arxiv.org/html/2304.05056)
- Results are qualitative only ("robust against initial posture poses", a "variety of starting poses"). No success rates, no perturbation tests.
- Stated limits:
  - point mass with massless limbs;
  - front or back only;
  - static motions; the paper "did not address highly dynamic get-up motions where the character could gain momentum by swinging their body";
  - flat ground.
  - Source: [Kenwright 2023 (ar5iv)](https://ar5iv.labs.arxiv.org/html/2304.05056)
- Internal inconsistencies:
  - "36 DOF" is claimed while the listed joints total 18.
  - The abstract's inverted-pendulum framing is not developed in the body.
  - Root or external forces are neither described nor ruled out.
  - Source: [Kenwright 2023 (ar5iv)](https://ar5iv.labs.arxiv.org/html/2304.05056)

### Inferences
- Body fully responsible? It appears so (PD joint torques), but the paper does not say whether the 6-DOF root is helped. Treat it as unverified.
- The transferable idea is the iterative "hand walk-back" with a COM-over-support test before each release. Two miner-specific problems:
  - Kenwright uses the hip midpoint as the COM. With the miner's heavy head the true COM is well forward of the hips, so the whole-body COM (Σ mᵢxᵢ/Σ mᵢ over the 13 links) must be used.
  - With short arms, the hands cannot reach far back under the COM. Kenwright's own feasibility rule (upper body between 1× and 2× the lower body) hints that proportions decide whether a purely static rise exists at all. If no static hand placement puts the COM over the feet, a short dynamic push is needed (see section 7).
- Run-time cost: tiny. Effort: moderate (support polygon, point-in-polygon test, re-plant loop). Naturalness: slow, deliberate, "elderly" rise.

### Gaps
- No quantitative evaluation, so robustness claims cannot be checked.
- No treatment of what happens when the COM cannot be brought over the feet statically, which is likely the miner's situation.

---

## 4. Balance feedback laws applied to multi-contact getting up (SIMBICON, virtual model control / Jacobian transpose, Coros 2010) — state- vs time-triggered transitions, and what closes the hands-to-feet gap

### Takeaway
The feedback laws behind the UBC locomotion controllers transfer to multi-contact rising, as Jones showed in 2D. The pieces are:
- world-frame torso control realised through stance-hip torques;
- Jᵀ gravity compensation so PD gains can stay low;
- a Jᵀ virtual force on the COM, chained from the stance ankle(s), toward a target over the support;
- state-triggered transitions (on contact, or after a fraction of a weight shift).

Coros et al. trigger the first step after the COM has moved 40% of the way to the new stance foot — the walking analogue of "hand off only when the load has moved". All of these are internal torques.

### Cited Findings
**SIMBICON** (full text read: [Yin, Loken, van de Panne 2007](https://www.cs.ubc.ca/~van/papers/2007-siggraph-simbicon.pdf))
- "Transitions between states happen after an elapsed time … or after foot contact". "If a foot contact has already occurred before entering a state having an outbound foot-contact transition, then the controller simply spends no time in that state."
- Torso and swing hip are tracked in the world frame through "virtual PD" torques. "In order to make the resulting torques be physically realizable without the use of external torques, the stance-hip torque is left as a free variable": τ_stance = −τ_torso − τ_swing.
- Balance feedback on the swing-hip target: θd = θd0 + cd·d + cv·v, where d is the horizontal distance from the stance ankle to the COM and v is the COM velocity. Applied in both the sagittal and coronal planes in 3D.

**Generalized Biped Walking Control** (full text read: [Coros, Beaudoin, van de Panne 2010](http://www.cs.ubc.ca/~van/papers/2010-TOG-gbwc/paper.pdf); [project page](https://www.cs.ubc.ca/~van/papers/2010-TOG-gbwc))
- Four parts: PD tracking, inverted-pendulum foot placement, Jᵀ gravity compensation, and Jᵀ velocity tuning.
- Gravity compensation: "a virtual force Fi = −mi·g at the center of mass of every link … τi = JiᵀFi… computed for all joints that lie in between each link i and the root link, i.e., the pelvis", for all links except the stance leg. It "allows for acceptable accuracy tracking … using low-gain PD-tracking… The low gains, in turn, help allow for natural, highly compliant motion."
- Velocity tuning: F_V = kV(Vd − V) in the sagittal plane, and a PD on the lateral COM position. τ = J_Vᵀ F_V, with J_V the COM Jacobian "for a chain of links that is considered rooted at the stance ankle" (stance foot to head).
  - "We zero the ankle component of the torque if we detect that the stance foot is not well planted, as measured by the toe leaving the ground or the foot exceeding a rotational velocity threshold."
  - "During double stance each stance leg is treated independently from its foot to the torso and the results are simply summed."
- Standing and the first step: a PD tracks a static COM target at the midpoint of the feet. "To begin walking, the COM-target is simply switched to lie in the middle of the desired future stance foot. Once the COM has moved 40% of the way, walking is triggered."
- Robustness: omnidirectional 600 N pushes for 0.1 s during a 0.6 m/s walk, on a 70.4 kg character. "Best suited for dynamically-balancing but slower motions"; highly dynamic motions are out of scope.

**Virtual model control / Jacobian transpose**
- GBWC traces Jᵀ control of the COM to Sunada et al. 1994 and Pratt et al.'s Virtual Model Control (IJRR 2001) — [Coros et al. 2010](http://www.cs.ubc.ca/~van/papers/2010-TOG-gbwc/paper.pdf)
- Jones: "Virtual forces are a technique for generating coordinated joint torques along a chain of links that mimic the effect of an external force… A virtual force acts from a base link, which is assumed to be stable, onto a link of application" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)

**Applied to rising**
- Jones used exactly these components (active-COM virtual force, root correction via stance hips, gravity compensation of swing limbs, phase pause until safe) for his sit-to-stand and prone-to-stand controllers — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
- Lin & Huang dynamics-filtered rising motions using "virtual actuator control … an 'external' force … converted into internal joint torques … we control the center of mass of the body by converting a control force into joint torques of the joints of limbs that have contact with the environment. This is helpful for maintaining balance while rising up."
  - However, their simulated part stops at sitting or squatting. They then "connect to a closest squatting-to-standing trajectory in our MOCAP database", which is kinematic.
  - Source: [Lin & Huang 2012](https://people.cs.nycu.edu.tw/~wclin/rising/rising_tvcj2012.pdf)

**State-triggered vs time-triggered transitions**
- Timed transitions: Faloutsos' pose FSMs ([2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)); Jones' sit-to-stand ([2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)); Stückler's robot routines ([2006](https://www.ais.uni-bonn.de/behnke/papers/IAS9.pdf)).
- State-triggered transitions:
  - SIMBICON (foot contact);
  - GBWC (COM 40% of the way);
  - Jones' support graph (pause until stance limbs are in contact and swing limbs are unloaded);
  - B-Human's NAO stand-up ("conditional waiting", a time-limited wait at the end of keyframes so the torso can tilt over before the next keyframe — [B-Human, arXiv 2510.02129 (2025)](https://arxiv.org/html/2510.02129v1));
  - Zambella et al. (hands released when a DCM terminal condition holds — section 5).

### Inferences
- Body fully responsible? Yes, for all of these. SIMBICON is explicit that the stance-hip "free variable" exists precisely to avoid external torques. Jᵀ forces are joint torques that react through planted contacts.
- For the miner, the hands-to-feet gap is a support transfer with a moving target. The GBWC and Jones laws suggest:
  1. A COM target that is a weighted blend of the support centres. The hands' weights ramp to 0 while the feet's weights ramp to 1.
  2. Jᵀ virtual forces from each planted foot (the GBWC chain, foot → torso) — and, while the hands still bear load, from each hand (hand → torso). These push the COM toward the target. Zero the ankle component when the foot is not flat.
  3. A state gate: the hands do not lift until the COM (or better, the capture point, section 7) has gone, say, most of the way to the foot polygon and the measured hand load is near zero.

  This replaces "timed minimum durations" with state conditions.
- GBWC's reasoning about gravity compensation is directly relevant to "looks posed vs alive". Gravity compensation lets PD gains stay low, so joints yield naturally. Unity provides `ArticulationBody.GetJointGravityForces` ([docs](https://docs.unity3d.com/ScriptReference/ArticulationBody.html)), but it compensates the whole articulation as a floating chain. The stance-referenced Jᵀ version in GBWC is different: it is computed per chain from a support. Which version is right for multi-contact needs testing.
- Risk: Jᵀ virtual forces assume a stable base. During rolling or slipping contacts they produce wrong torques. Jones saw foot slip from root correction, and GBWC zeroes ankle torque when the foot is not planted. With PhysX articulations and stiff PD drives, extra feedforward torques can also fight the drives. Keep the drives soft where feedforward is used.

### Gaps
- SIMBICON's quantitative robustness numbers were not extracted.
- No source applies SIMBICON-style or GBWC-style feedback to a 3D bear-stance-to-squat transition specifically. Jones' prone controller is 2D and goes knees → feet, not hands-and-feet → feet.
- How `GetJointGravityForces` (floating-base, whole articulation) relates to stance-rooted Jᵀ compensation was not tested.

---

## 5. Robotics: HRP-2 getting up, fall recovery, Atlas, NAO and kid-size robots, multi-contact whole-body control, centroidal momentum, ZMP and contact-wrench criteria — which could run in light form inside a game?

### Takeaway
Real robots that reliably get up mostly use designed sequences with static phases that bring the COM right next to the feet, and only a short dynamic phase:
- HRP-2P uses a contact-state graph with a short ZMP-controlled kneel-to-crouch transition.
- Kid-size and NAO robots use keyframes, made robust by sensor checks, COM balancing, waits and retries.

Model-based multi-contact controllers (DLR TORO, IIT) compute a desired COM wrench, distribute it over contacts within friction cones, and map it to joints with Jᵀ. They time the hand release with a momentum criterion (3D-DCM). This contact-force-distribution core plus a DCM release test is light enough for a game. Full MPC (Tassa/Todorov) and full centroidal-momentum QPs are heavy.

### Cited Findings

**HRP-2 / HRP-2P (AIST)**
- Kanehiro et al., ICRA 2003: the first human-size humanoid that could lie down and get up, face up and face down. The software splits the motion into a sequence of robot–floor contact states with a controller for each transition. Abstract only — [bib/abstract](https://unit.aist.go.jp/jrl-22022/assets/bib/kanehiro_icra_2003.bib)
- Secondary description (Stückler et al.): HRP-2P is 154 cm, 58 kg, 30 DOF. "Kanehiro et al. proposed … a motion controller that supports static motion and ZMP-controlled dynamic motion… When the robot stands up from a prone position, the dynamic motion controller is applied to perform a short transition between kneeing and crouching. Dynamic motion is needed because the knees and soles of the robot can not have contact with the floor simultaneously." — [Stückler et al. 2006](https://www.ais.uni-bonn.de/behnke/papers/IAS9.pdf)
- Kanehiro et al., ICRA 2007 ("Getting up motion planning using Mahalanobis distance"): generalises to arbitrary lying states. It finds the most similar predefined falling state by Mahalanobis distance, plans a collision-free motion to it, then follows the state-transition graph — [bib/abstract](https://unit.aist.go.jp/jrl-22022/assets/bib/kanehiro_icra_2007.bib)
- In the planner, nodes are sets of contacting body parts, arcs add, remove or move a contact, and the search is A* — [KAKENHI 16300064 summary](https://kaken.nii.ac.jp/grant/KAKENHI-PROJECT-16300064)
- Journal version: Hirukawa et al., IJRR 2005, credits a waist joint and a new motion controller — [bib](https://unit.aist.go.jp/jrl-22022/assets/bib/hirukawa_ijrr_2005.bib)
- Jones notes these robot motions are "extremely slow, and require static stability throughout most of the motion" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)

**Kid-size: Stückler, Schwenk, Behnke 2006 (NimbRo "Jupp")** (full text read: [IAS-9 paper](https://www.ais.uni-bonn.de/behnke/papers/IAS9.pdf))
- Robot: 60 cm, 2.3 kg, 19 servos. Fall detection: tilted more than 45° for more than 1 s.
- Motions are sinusoidal joint-target trajectories, i.e. open-loop position control. They were designed in an ODE simulation and transferred to the robot.
- Principle: "Statically stable motion sequences that utilize support by the arms, the knees, or the backside of the robot bring the COM projection close to the foot polygon. Dynamic motion is needed to come back on two feet."
- Prone routine:
  1. Lift the trunk; forearms under the shoulders.
  2. Bring the COM projection "as close as possible to the leading edges of the feet" by folding spine, hips and knees, into a "bridge-like position, leaning on the leading edges of the feet and the strained forearms".
  3. Straighten the arms to tip over the leading edges of the feet. This is the dynamic phase: the COM leaves the support polygon briefly.
  4. Straighten up.
- Main parameters are the elbow amplitude e and the length of the dynamic phase. "If they are too small, the COM projection will not leave the support polygon. Too high values cause the backward motion to overshoot … which causes the robot to fall over the leading edges of the feet again."
- Results: "100 percent in 100 tests". Parameters could vary widely (h = 0.21 ± 0.025, e = 0.16 ± 0.06) "explained by the shortness of the dynamic phases, which is achieved by moving the COM projection as close as possible to the foot polygon in the preliminary static phases."
- The routines were robust to initial posture, hard or slippery floors, and even one relaxed arm. They failed at low battery, when the arm servos browned out during the dynamic phase. They were used at RoboCup 2005.
- The same paper says Sony QRIO, CycloidII, KHR-1 and VisiON stand up statically. CycloidII manages this thanks to "powerful Dynamixel actuators and disproportional long arms".

**NAO, B-Human 2025 ("Stand Up, NAO!")** (read via arXiv HTML: [arXiv 2510.02129](https://arxiv.org/html/2510.02129v1))
- The stand-ups are keyframes, augmented with:
  - a COM PD balancer (expected vs measured COM, with expected values recorded at keyframe ends);
  - arm and torso checks — a stuck arm triggers a freeing motion or a step back; a torso outside the allowed range triggers a low-stiffness fall and a retry from the start;
  - "conditional waiting", so the torso can tilt over before the next keyframe;
  - small ankle-roll oscillations to un-stick the soles;
  - joint-error compensation.
- "The main reason for failed stand-up attempts are large errors in the executed joint positions". A stuck hip-yaw-pitch error above 20° exceeds what balancing can handle. If the torso tilt-over is delayed, "the next keyframe starting too early causes the robot to fall back."
- Competition success rates:

  | Team / event | Success |
  |---|---|
  | B-Human, 2016 (before artificial turf) | 89.6% |
  | B-Human, RoboCup 2018 (turf) | 76.5% |
  | B-Human, 2019 | 97.5% |
  | B-Human, 2022 | 98.1% |
  | B-Human, 2024 | 81.7% (worn robots) |
  | B-Human, 2025 | 97.7% |
  | Other teams | roughly 55–94% |

**Other RoboCup teams** (search snippets only)
- NUbots' team report splits the get-up into scripted segments with sensor checkpoints and restarts on failure — [NUbots TDP](https://hsl.robocup.org/wp-content/uploads/2026/03/NUbots-tdp-69ab720ed1e73.pdf)
- A Hamburg thesis notes that static animations cannot adapt to turf or contact — [Stelter BSc](https://tams.informatik.uni-hamburg.de/paper/2020/BSc_Sebastian_Stelter.pdf)

**Kuniyoshi et al. (adult-size robot, dynamic roll-and-rise)** (abstracts only)
- The robot rose from flat-lying to crouching within 2 s by controlling only sparse "knacks" (critical points in phase space) rather than the whole trajectory — [Kuniyoshi et al. 2005](https://unpaywall.org/10.1007%2F11008941_41)
- Overall trajectory difference did not separate successes from failures, but a localised measure, "energy input timings", did — [JRSJ 2005 abstract](https://doi.org/10.7210%2Fjrsj.23.706)
- The motion succeeded in simulation but did not transfer to the real robot because ground contact is hard to simulate — [Stückler et al. 2006](https://www.ais.uni-bonn.de/behnke/papers/IAS9.pdf); [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)

**Multi-contact passivity-based whole-body control (DLR)**
- Ott, Roa & Hirzinger 2011: compute the wrench needed to restore COM position and trunk orientation, then distribute it "as forces at predefined contact points via a constrained optimization, which aims at achieving the desired wrench while minimizing the Euclidean norm of the contact forces", with friction restrictions. Roa & Ott 2013 add joint-torque limits. Abstract or secondary only — [DLR elib](https://elib.dlr.de/73492)
- Henze, Roa & Ott, IJRR 2016: balance on a subset of end-effectors while others interact. The controller regulates the COM, hip orientation and end-effector poses by applying a wrench at each end-effector. Tested on TORO. Abstract only — [DLR elib](https://elib.dlr.de/105384); [summary](https://metatoc.com/papers/80639-passivity-based-whole-body-balancing-for-torque-controlled-humanoid-robots-in-multi-contact-scenarios)
- Hyon, Hale & Cheng, T-RO 2007: gravity-compensating, passive full-body balancing that sets a desired ground reaction force and transforms it "into full-body joint torques". Hyon & Cheng 2006 "optimally solves the contact force closure and transforms it directly into joint torques in real-time without any joint trajectory planning". Abstracts or secondary only — [TUM mediatum](https://mediatum.ub.tum.de/doc/1545378/document.pdf); [ATR page](https://bicr.atr.jp/icorp/topic3e.html)

**Zambella et al., RA-L 2023: agile standing up with 3D-DCM, TORO** (full text read: [TU Wien PDF](https://repositum.tuwien.at/bitstream/20.500.12708/189191/1/Zambella-2023-IEEE%20Robotics%20and%20Automation%20Letters-vor.pdf))
- Two phases:
  - **Push**: the passivity-based whole-body controller "maximizes the push forces applied to the robot's center of mass".
  - **Rising**: forces are reduced to zero and the CoM is stabilised upright.
- Centroidal angular momentum is optimised online to set the waist and arm motion after the hands detach. It is regulated to zero during the push, then actively generated during the rise.
- Switch condition: "we need to check if the current DCM is already on a trajectory that allows the robot to recover the upward position in a certain time T. If this condition is verified and the push phase is not stopped, the humanoid will fall on its back". The hands detach at the switch.
- DCM ξ = x_c + b·ẋ_c with b = √(z̄/g).
- Robot: TORO, 27 DOF, 79.2 kg, 1.74 m. Tested in OpenHRP simulation and on hardware.
- Chair scenario: push starts at 0.1 s, switch at 0.57 s, stable at 1.5 s. Elbow torques saturated, which limited the push.
- On hardware, "the maximum force with which each arm can push (i.e., 50 N) narrowed the range of the possible TORO's initial configurations".
- The hands were on supports at about 0.84–1.0 m height (chair or wall), not on the floor.
- Earlier work by Y. Lee, Tsagarakis & J. Lee (IROS 2019) used an energy-based push whose "switching time … which coincides with the hands detachment time, is determined empirically. This makes this approach heuristic" — as described in [Zambella et al. 2023](https://repositum.tuwien.at/bitstream/20.500.12708/189191/1/Zambella-2023-IEEE%20Robotics%20and%20Automation%20Letters-vor.pdf)

**Multi-contact stability criteria**
- Bretl & Lall 2008: for fixed contacts with friction, the set of statically feasible COM positions is a vertical prism with a convex cross-section, the "support region". On uneven terrain, "keeping the CoM above the convex hull of the feet can be arbitrarily bad". Secondary summary — [Bretl & Lall PDF](https://lall.stanford.edu/papers/bretl_eqmcut_ieee_tro_projection_2008_08_01_01/pubdata/entry.pdf)
- Caron, Pham & Nakamura 2015: the contact wrench cone / gravito-inertial wrench cone, built from contact friction cones, as a general criterion for non-coplanar contacts. Secondary summary — [RSS 2015](https://roboticsproceedings.org/rss11/p28.html)

**Momentum control in animation** (abstract only)
- Macchietto, Zordan & Shelton 2009 control linear and angular momentum by relating them to the COM and the centre of pressure (COP). An optimisation reconciles the momentum goals with tracking to produce joint accelerations, and hybrid inverse/forward dynamics turns those into torques. The method produces whole-body strategies such as "wind-milling arms" and works for non-human morphologies — [UCR page](https://www.cs.ucr.edu/~cshelton/papers/MacZorShe09.html)

**MPC getting up** (relevant parts read: [Tassa, Erez, Todorov 2012](https://02465material.pages.compute.dtu.dk/02465public/_assets/tassa2012.pdf))
- iLQG model-predictive control got a 22-DoF, 1.6 m, 55 kg humanoid up "from an arbitrary pose on the ground", running "7× slower than real time" on a 2012 PC. Controls are motor torques.
- The state cost includes:
  - the horizontal COM distance to the mean of the feet;
  - the horizontal torso distance to the COM;
  - the vertical torso distance to a point 1.3 m above the feet.
- Finite-differenced derivatives need about 4,000 dynamics evaluations per control step (20 DOF, 50-step horizon), which required a custom fast engine (MuJoCo).

**Boston Dynamics Atlas**
- Atlas' controller is MPC-based: "an inaccurate model will lead to incorrect predictions and actions, which for Atlas usually means falling over" — [BD blog](https://bostondynamics.com/blog/picking-up-momentum/)
- I found no source describing an Atlas getting-up controller.

**Learned contrast (out of scope, for calibration only)**
- HumanUP (RL) reports 78.3% get-up success on real human-size humanoids — [He et al. 2025](https://arxiv.org/html/2502.12152v2)

### Inferences — what is light enough for a game (C#, per frame, one character)
- **Contact-force distribution with Jᵀ** (Ott 2011, Hyon 2007, Henze 2016 core): light. Feasible per frame. Fully the body's own. Probably the strongest structural upgrade for the bear-stance and squat phases, because it directly decides how much each hand versus each foot pushes and lets the hand share be driven to zero on purpose. Steps:
  1. Desired COM wrench = PD on COM position and velocity + gravity + a torso-orientation PD.
  2. Solve for contact forces at the planted feet, hands and knees: least-squares or a tiny QP, with friction-cone clamping and a per-contact upper bound from joint strength. Add a regulariser that pushes the hand forces toward zero as the hand-off proceeds.
  3. τ = Σ J_cᵀ f_c from `GetDenseJacobian`.
  4. Add gravity compensation; keep low-gain PD for posture.

  Cost: 13 links, about 4–8 contacts, a 6 × (3·contacts) system — microseconds to tens of microseconds (inference, not measured).
- **3D-DCM / capture-point release test** (Zambella, Mesesan 2017 via Zambella): a few lines of math. The miner's limiting factor will be short arms with limited strength, as TORO was limited by 50 N per arm.
- **ZMP / COM-in-polygon checks** (Kanehiro, Stückler, Faloutsos): trivial. A COM-over-support-polygon test is valid on flat ground. Bretl & Lall warn that it can be "arbitrarily bad" on uneven terrain or with non-coplanar contacts such as hands on a slope.
- **Contact-state graph** (Kanehiro): a design pattern — nodes are contact sets, edges are single contact changes. It suits authoring the miner's stage list and choosing a recovery stage on failure. A* planning over it is unnecessary for one character with a handful of states.
- **Keyframes plus checks plus retries** (B-Human, NUbots): essentially what the miner has now. The robots reach 90%+ only with:
  - waits until the torso has tilted over (state-triggered);
  - torso-orientation checks with abort and retry;
  - COM balancing on top of the keyframes;
  - compensation for joint tracking error.
- **Centroidal momentum QP with inverse dynamics** (Macchietto): possible with `GetJointForcesForAcceleration` ([Unity docs](https://docs.unity3d.com/ScriptReference/ArticulationBody.html)), but heavier and much more effort. It is the route to natural arm "wind-milling" balance.
- **MPC** (Tassa, Atlas): not feasible in PhysX per frame — it needs thousands of forward simulations per step. Its cost function is a good definition of "up": COM over the feet, torso over the COM, torso high.

### Gaps
- Kanehiro 2003, 2007 and Hirukawa 2005: full texts not read. Controller gains, ZMP feedback details and success rates are unknown here.
- No source found for HRP-4 or HRP-2Kai get-up controllers, Atlas get-up, or Darwin-OP get-up specifics.
- Henze 2016, Ott 2011, Hyon 2007 and Macchietto 2009: only abstracts or secondary summaries were read.
- Mesesan 2017 (multi-contact DCM transitions) and Lee/Tsagarakis/Lee 2019: known only through Zambella's description.
- Zambella's experiments start with hands on a chair or wall at about 1 m, not on the floor. No floor-to-stand result was found for a torque-controlled adult humanoid using these methods.

---

## 6. Reported success rates, robustness to varied start states, and naturalness

### Takeaway
Few designed controllers report success over repeated varied starts.
- The best numbers are on robots with keyframes: Stückler 100/100 in the lab, B-Human up to 97–98% in competition, both after heavy engineering of static phases, checks and retries.
- In animation, the only quantified robustness is Jones' 2D prone controller: at least 80% of 16 perturbed runs per slope or size step after optimisation.
- Faloutsos reports only classifier accuracy; Kenwright, Tassa and Zambella report none.
- Naturalness is rarely measured. Fast optimised rises look "unnatural … violent collisions", and largely open-loop transitions look unnatural.

### Cited Findings
**Numbers**

| Source | What was measured | Result |
|---|---|---|
| [Stückler et al. 2006](https://www.ais.uni-bonn.de/behnke/papers/IAS9.pdf) | Kid-size robot, lab | 100% in 100 tests |
| [B-Human, arXiv 2510.02129 (2025)](https://arxiv.org/html/2510.02129v1) | B-Human NAO stand-ups, competition | 76.5–98.1% depending on year and surface |
| [B-Human, arXiv 2510.02129 (2025)](https://arxiv.org/html/2510.02129v1) | Other NAO teams | about 50–94% |
| [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf) | 2D prone, perturbed runs | ≥ 80% of 16 per slope or size step; slopes −12° to +17°; torso 1.45× size |
| [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf) | 3D sit-to-stand | works for 45–65 cm chairs |
| [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf) | Pre-condition classifier accuracy (not success) | 85–100% for kneel and stand controllers |
| [Lin & Huang 2012](https://people.cs.nycu.edu.tw/~wclin/rising/rising_tvcj2012.pdf) | Planning from 60 random lying postures (20 supine, 20 prone, 20 lateral) | all planned successfully; squat-to-stand was kinematic MOCAP |
| [He et al. 2025](https://arxiv.org/html/2502.12152v2) | RL on hardware (contrast) | 78.3% get-up |

**Varied start states**
- Faloutsos' prone controller first drives the body to a known pose ("assumes it has the time to change the state of the character to one from which it knows how to rise") — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
- Jones' lateral quadruped controller works "for most falls on flat ground because before starting to rise, the character is driven to a standard pose" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
- Tassa's MPC handles arbitrary poses by re-optimising from the current state — [Tassa et al. 2012](https://02465material.pages.compute.dtu.dk/02465public/_assets/tassa2012.pdf)

**Naturalness**
- Jones: the fastest optimised rises are "quite unnatural and include violent collisions"; the quadruped side-lying rise is "often generating unnatural motions" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
- Lin & Huang user study (1–10 scale): their rises 6.29 ± 2.27 vs MOCAP 6.92 ± 2.22. Dynamics-filtered results were preferred over unfiltered in 73.61% of pairs — [Lin & Huang 2012](https://people.cs.nycu.edu.tw/~wclin/rising/rising_tvcj2012.pdf)
- GBWC attributes "natural, highly compliant motion" to low PD gains made possible by gravity compensation — [Coros et al. 2010](http://www.cs.ubc.ca/~van/papers/2010-TOG-gbwc/paper.pdf)

### Inferences — per-method assessment for the miner (judgement, built from the cited findings)

| Method | Only the body's joint torques? | Expected robustness | Naturalness | Run-time cost per character | Unity C# effort | Main risks |
|---|---|---|---|---|---|---|
| Faloutsos composable pre/post/failure-monitor framework | Yes | Wrapper adds robustness through early failure detection and re-selection; inner timed FSMs stay fragile | Same as inner controllers | Negligible | Low–medium | Hand-tuned condition boxes; inner timed transitions |
| Jones support graph + active-COM Jᵀ force + root correction via stance hips + phase gating + load ramps | Yes | Shown ≥ 80% under perturbation in 2D after optimisation; lateral 3D untested | Slow versions plausible; fast ones violent | Negligible | Medium | Root correction can slip feet; Jᵀ needs planted base; 3D lateral balance missing |
| Kenwright hand walk-back over a support triangle | Apparently (not stated) | Unquantified; static only | Slow, deliberate | <3 ms total in paper | Medium | Fails if the short arms cannot bring the COM over the feet; hip-midpoint COM is wrong for a big head |
| SIMBICON / GBWC laws (world-frame torso via stance hip, Jᵀ gravity compensation, Jᵀ COM velocity force, state triggers) | Yes (explicitly avoids external torque) | Robust walking (GBWC: 600 N pushes); needs adapting to hands | Compliant, natural with low gains | Negligible | Low–medium | Foot-not-flat torque errors (GBWC zeroes the ankle term); interplay with PhysX drive stiffness |
| Contact-force distribution (Ott/Hyon/Henze), light form | Yes (τ = Σ Jᵀ f) | High on torque-controlled robots (abstract-level evidence) | Compliant | Tiny least-squares/QP per frame | Medium–high | Needs reliable contact detection, friction cone, strength limits; drive interplay |
| 3D-DCM / capture-point release (Zambella) | Yes | Principled release timing; validated sim + TORO, no rates | Agile | Trivial formula | Low–medium | Weak arms limit the push (TORO: 50 N per arm); COM height varies during rise |
| Momentum control (Macchietto) | Yes (inverse dynamics torques) | Good push recovery (abstract) | Natural whole-body, arm wind-milling | Moderate | High | Only abstract read; needs inverse dynamics plus optimisation |
| MPC (Tassa) | Yes | Handles arbitrary start poses | Varies | Prohibitive in PhysX | Very high | Needs thousands of rollouts per step |
| Robot keyframes + checks + retries (Stückler, B-Human) | Yes | 90–100% only when static phases bring the COM to the feet and checks/retries exist | Robotic | Negligible | Low | Sensitive to joint tracking error and surface |
| Lin & Huang planning + dynamics filter | Partly — the squat-to-stand is kinematic MOCAP | n/a | 6.29/10 | Seconds offline | High | Violates the director's rule for the final phase |

### Gaps
- No designed 3D character get-up controller from prone with a reported success rate over many varied starts was found.
- No naturalness study exists for physics-based get-ups apart from Lin & Huang.

---

## 7. Specific mechanisms for the exact failure: losing balance when the hands leave the ground in a deep squat or bear stance, and when the standing controller first raises the body

### Takeaway
The literature converges on five mechanisms, used in combination.
1. **Shrink the gap statically before release.** Bring the COM projection as close as possible to the feet — fold the legs under, walk the hands back under the shoulders or COM — so any unsupported dynamic part is short. Stückler credits his 100/100 and wide parameter tolerance to this.
2. **Transfer load continuously and confirm it.** Ramp a limb's balance weight to zero and pre-lift its target slightly; advance the phase only when the limb is unloaded (Jones). Trigger on how far the COM has moved, not on time (GBWC).
3. **If a dynamic phase is unavoidable, push with the hands, then release on a momentum test.** Release when the DCM or capture point is on a course that ends over the feet (Zambella). Without such a test the release time is "heuristic", and a push that is too weak or too strong falls back or overshoots (Stückler).
4. **After release, regulate the COM over the feet.** Use ankle and hip strategies at the current crouch height before raising it: GBWC velocity tuning from the stance ankles, Jones' active-COM force, Faloutsos' inverted-pendulum ankle balance, and Macchietto's COP plus angular momentum.
5. **Check and retry.** Monitor torso orientation and COM against expected values; on failure, back off to the last stable contact state and retry (Faloutsos expected performance, B-Human, NUbots).

### Cited Findings
- **Static gap-shrinking.** "Statically stable motion sequences … bring the COM projection close to the foot polygon… Dynamic motion is needed to come back on two feet." Wide parameter tolerance is "explained by the shortness of the dynamic phases, which is achieved by moving the COM projection as close as possible to the foot polygon in the preliminary static phases" — [Stückler et al. 2006](https://www.ais.uni-bonn.de/behnke/papers/IAS9.pdf)
- **Hand walk-back before release.** "If the CoM is not over the feet, the free hand moves to the side of the body at the CoM location, becomes the new support hand"; both hands are released only when the COM is over the feet — [Kenwright 2023](https://ar5iv.labs.arxiv.org/html/2304.05056)
- **Re-place the arms under the shoulders before transferring to the feet.** "The arms are lifted one at a time, and placed directly under the current shoulder position… Next, the character's right leg is lifted and planted 30 cm in front of the hip" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
- **Load ramps and a lift gate.**
  - Weights go from 1 to 0 over the 0.5 s before lift; the lifting end-effector target rises 0 → 4 cm over 0.5 s before lift.
  - The phase pauses until "stance limbs are in contact with the ground, and swing limbs are not supporting weight".
  - Source: [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
- **Trigger on COM progress.** "Once the COM has moved 40% of the way, walking is triggered" — [Coros et al. 2010](http://www.cs.ubc.ca/~van/papers/2010-TOG-gbwc/paper.pdf)
- **Momentum-gated hand release.**
  - Release when "the current DCM is already on a trajectory that allows the robot to recover the upward position in a certain time T"; pushing past that point makes the robot "fall on its back".
  - During the push, the controller "maximizes the push forces applied to the robot's center of mass"; afterwards it reduces them to zero.
  - Angular momentum is regulated to zero during the push and shaped during the rise.
  - Source: [Zambella et al. 2023](https://repositum.tuwien.at/bitstream/20.500.12708/189191/1/Zambella-2023-IEEE%20Robotics%20and%20Automation%20Letters-vor.pdf)
- **Push magnitude window (open-loop).** "If they are too small, the COM projection will not leave the support polygon. Too high values cause the backward motion to overshoot … which causes the robot to fall over the leading edges of the feet again." — [Stückler et al. 2006](https://www.ais.uni-bonn.de/behnke/papers/IAS9.pdf)
- **Forward momentum from the upper body plus a pushing force when the COM is behind the feet.** "Dipping the shoulders and applying an extra virtual force provide enough forward momentum to return the dog to standing" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
- **Short ZMP-controlled dynamic transition** between kneeling and crouching on HRP-2P, because knees and soles cannot both be on the floor — [Stückler et al. 2006 describing Kanehiro 2003](https://www.ais.uni-bonn.de/behnke/papers/IAS9.pdf)
- **Wait for the torso to tilt over before continuing**, otherwise the robot falls back. Check torso orientation; on failure, fall softly and retry — [B-Human, arXiv 2510.02129 (2025)](https://arxiv.org/html/2510.02129v1)
- **Balance after hand release, at crouch height.**
  - Faloutsos' supine riser ends "on its feet with its center of mass within the support polygon" in a crouch, and another controller raises it. The balance controller's pre-condition requires COM velocity < 0.3 m/s, acceleration < 0.1 m/s² and the COM inside S — [Faloutsos et al. 2001](https://www.cs.ubc.ca/~van/papers/2001-siggraph-composable.pdf)
  - Jones' active-COM virtual force targets the centre of support: "without this control … cannot maintain balance while standing" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
  - GBWC's Jᵀ COM velocity force is rooted at each stance ankle, with the ankle term zeroed if the foot is not planted — [Coros et al. 2010](http://www.cs.ubc.ca/~van/papers/2010-TOG-gbwc/paper.pdf)
- **Angular-momentum (arm) strategies.** Momentum control yields "wind-milling arms" balance recoveries — [Macchietto et al. 2009](https://www.cs.ucr.edu/~cshelton/papers/MacZorShe09.html)
- **Anticipation.** "Especially for dynamic motions, a balance strategy must be anticipatory, planning for future contact configurations and considering character momentum" — [Jones 2011](https://users.cs.utah.edu/~benjones/ubc_2011_fall_jones_benjamin.pdf)
- **Sparse critical points.** Success in a dynamic roll-and-rise was predicted by "energy input timings" at sparse critical points, not by overall trajectory similarity — [Kuniyoshi et al., JRSJ 2005 abstract](https://doi.org/10.7210%2Fjrsj.23.706)

### Inferences (how these map onto the miner; none of this has been tested)
- **Why 1–8 of 36 and chaotic.** The failing transition (bear stance → feet) is executed as a timed key-pose change with a COM balance over the contacts. If the COM is still well ahead of the feet when the hands lift — likely with a heavy head and short arms — the hand release makes the body fall forward. Whether it recovers depends on small differences in momentum at release. That explains the try-to-try chaos.
  - Stückler's and Kanehiro's designs both make that unsupported interval very short and start it from a consistent state.
  - Zambella's design times it from the momentum state rather than the clock.
- **A concrete gate for hand release** (cheap, fully the body's own):
  - Compute the whole-body COM x and its velocity v (13 links).
  - Compute the capture point / DCM ξ = x_h + v_h·√(z/g), horizontal components, z = current COM height.
  - Lift the hands only when:
    - ξ lies inside the foot support polygon (toes included) with a margin;
    - the measured or estimated hand load is near zero;
    - |v| is small.
  - Otherwise either keep transferring load statically (hips back and down, hands walked back), or perform a short, bounded hand push and re-test ξ every frame, releasing the moment it enters the polygon.
- **Contact-force distribution makes "weight shifted off the hands" a controlled quantity.** It replaces the side-effect of pose changes: the hand force target is ramped to zero, and the feet pick it up through Jᵀ. If the feet cannot take it (the friction or strength bound is hit), the solver signals that the state is not yet releasable. That signal is the pre-condition check.
- **A "crouch-balance" stage before the standing controller raises the body.** Hand over only when the stand controller's pre-conditions hold (Faloutsos-style: COM over the feet, low COM velocity and acceleration, posture near its start pose). Raise the height slowly while a COM PD over the feet (GBWC-style Jᵀ from each stance ankle) holds the horizontal COM. Raise faster only after it is stable. This should be "the same body", one controller whose height target moves, rather than a different controller appearing.
- **The heavy head is a large lever for angular momentum.** Lifting it early during the hands-to-feet transfer moves the COM backward toward the feet — a natural, human-like strategy. Lifting it too fast creates a backward pitch moment. Treat head and trunk pitch as a COM-shaping input inside the COM controller rather than a fixed key pose. This follows GBWC's world-frame torso control, Jones' root correction and Macchietto's angular momentum.
- **Retries over help.** The B-Human and Faloutsos patterns are: detect failure early (torso orientation or COM leaving the expected tube), back off to the last stable contact state (bear stance or kneel), and retry. This fits the director's "extra tries over help" and "never the backup animation" rules without any external force.

### Gaps
- No source studies the bear-stance (hands-and-feet) → squat transfer specifically for a character with short arms and a heavy head. Kenwright's proportion rule hints that proportions decide whether a static solution exists, but nothing quantifies it for such bodies.
- Whether a capture-point test calibrated for standing holds at deep-squat heights with multi-contact was not found in any source. Zambella's 3D-DCM uses an average COM height, and validation started from about 1 m hand supports, not the floor.
- How Jᵀ feedforward torques interact with PhysX ArticulationBody PD drives (stiffness, damping, force limits) was not found in any source. It needs testing in the project.
