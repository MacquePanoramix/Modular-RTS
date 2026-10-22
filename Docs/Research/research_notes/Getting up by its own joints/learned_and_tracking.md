# Learned and tracking controllers for getting up by the body's own joints

Scope: deep reinforcement learning (RL) and physics-based motion tracking for making a simulated character or humanoid robot get up from the ground, and how such a controller could be trained and run in Unity 6 for a 13-part ArticulationBody miner (about 40 kg, heavy head, short limbs, PD drives capped by realistic strength, three proportions: Small, Long, Round). The game director's rule: only the body's own joint torques against the ground at run-time; no root/residual forces, no kinematic or blended animation, no teleport.

Reading depth: full text was read (via arXiv HTML / ar5iv) for Tao et al. 2022, HumanUP, HoST, ASE, PHC, DeepMimic, AMP, the OP3 soccer paper, FRASA, HiFAR, the Disney bipedal character paper, and MuJoCo Playground (first 100k characters of the long ones). Only abstracts or search snippets were readable for Frezzato et al. 2022, StableMimic (2026), Residual Force Control, Won and Lee 2019, AdaptNet, XHugWBC, the sim-to-sim papers, Marathon Environments and the Unity community posts; these are marked.

## 1. "Learning to Get Up" (Tao, Wilson, Gou, van de Panne, SIGGRAPH 2022)

### Takeaway
A three-stage RL method with no motion capture: discover get-ups with a strong body, anneal torque limits down to a weak body (usually below 60% of default), then train a policy that tracks the weak policy's own motion at up to 5x slower speed. About 24 h per stage on a 32-core desktop with an RTX 2070; the paper reports no success rate, and its naturalness claims are subjective. Its 38.3 kg character is close to the miner's mass.

### Cited Findings
- No motion capture: "without recourse to motion capture data"; stages are (1) a strong character model to discover solution modes, (2) adaptation to progressively weaker versions, (3) reproduction of the weaker get-ups at much slower speeds — [arXiv abstract](https://arxiv.org/abs/2205.00307)
- Character: 19 body parts, 21 DoF, 38.3 kg, about 1.5 m (the paper's "1.5 mm" is a typo); no fingers or toes; capsules/cylinders; MuJoCo at 800 Hz, policy at 40 Hz — [full paper, ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Actuation: PD with kp = torque limit 𝒯 and kd = kp/10; torque limits derived from documented human values scaled by body-part weight. In the slow stage the policy outputs a residual added to the reference orientation, i.e. PD target offsets — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Strong-to-weak curriculum: limits = β^i × 𝒯 with β = 0.95 per stage, each episode samples the multiplier from N(β^i, 0.04); advance when test reward > 60; final limits were usually below 60% of default on average. Training directly with fixed limits of 60%, 50% or 40% "failed to discover any get-up mode" — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Naturalness: the authors subjectively found 40–60% strength looked more natural than above 70%; high-strength or curriculum-free policies produce more dynamic, less natural motion. There is no user study and no comparison against human get-up data — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Slow stage: retiming coefficient κ sampled uniformly from [0.2, 0.8] (up to 5x slower), reference = the weak policy's own fast trajectory, linearly retimed; after the motion the policy balances in a manually designed standing pose for 100 timesteps; uses random state initialization from reference states with early termination when COM height deviates by 0.5 m. Repeating a reference state lets the character pause in statically stable poses — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Initial states: rag-doll drop from 1.5 m with randomized pose and random actions N(0, 0.1) for 80 control steps; ten fixed random initial states for evaluation — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Training cost: SAC, two-layer 1024-unit networks; each of the weak and slow policies took roughly 24 hours on a 32-core CPU desktop with an RTX 2070; total samples not reported — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Results are qualitative (learning curves over 10 runs, t-SNE, video): most policies converge on a common final stand-up strategy; some use different strategies for prone and supine; strategies often have significant static stability so they can be paused; tested with a leg and an arm in a cast — [abstract](https://arxiv.org/abs/2205.00307); [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Stated limitations: a separate simulation of the weak policy is needed to make the slow reference (distillation into one policy is future work); no control over which strategy emerges; missing toes and fingers "matter" for get-up — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- Independent re-implementation on a Unitree G1 (by the HumanUP authors, Isaac Gym): Tao et al. reached 92.62% (supine) and 98.99% (prone) success but with action jitter 5.39 / 11.73 and energy 650 / 1015, versus HumanUP's 0.56 / 0.39 jitter and 92 / 70 energy; described as "unstable, jumping motions" — [HumanUP](https://arxiv.org/html/2502.12152)

### Inferences
- The strong-to-weak schedule maps directly onto the miner: train first with drive force limits well above "realistic joint strength", then anneal to the realistic caps; the deployed policy would run only at the realistic caps, so it satisfies the director's rule at run-time. Whether training-time "extra strength" is acceptable as scaffolding is a design question for the director, not something the paper settles.
- The finding that fixed 40–60% strength never discovers a get-up is a strong warning for the miner, whose caps are already "realistic": RL started at final strength may fail the same way.
- The slow stage (tracking the policy's own physically valid motion, slowed) is the paper's answer to "learned motion looks unnatural"; but the HumanUP re-implementation shows that without strong smoothness terms the result can still be jittery when moved to another body and simulator.

### Gaps
- No success rate, robustness-to-start-state number, or sample count is reported in the paper itself; only the HumanUP re-implementation gives numbers, on a different body.
- Behaviour with a very heavy head and short limbs (miner proportions) is untested anywhere I found.

## 2. Character-animation controllers: DeepMimic, AMP, ASE, PHC (Perpetual Humanoid Control), and others

### Takeaway
Animation-side methods get up either by imitating get-up clips (DeepMimic, AMP) or by learning recovery from fallen starts under a motion-style prior (ASE, PHC). They are robust (ASE recovered from all 500 test falls; PHC 95.0% within 5 s, 98.8% within 10 s) but cost 50–300 million samples per skill on CPU (DeepMimic/AMP) or about 10 billion samples on a GPU simulator over 7–10 days (ASE/PHC). All drive PD targets only; PHC explicitly uses no external forces, while the older UHC needed residual forces.

### Cited Findings
- DeepMimic (Peng et al. 2018): Getup Face-Down 3.28 s clip, 49 M samples, normalized return 0.885; Getup Face-Up 4.02 s, 66 M samples, 0.838 — [DeepMimic, ar5iv](https://ar5iv.labs.arxiv.org/html/1804.02717)
- DeepMimic humanoid: 13 links, 34 DoF, 45 kg, 1.62 m; actions are PD target orientations with stable PD; Bullet at 1.2 kHz, policy at 30 Hz; learning "often requir[es] several days per skill" — [DeepMimic](https://ar5iv.labs.arxiv.org/html/1804.02717)
- DeepMimic: reference state initialization (RSI) plus early termination (ET) are critical for dynamic skills (e.g. backflip 0.791 with both vs 0.379 with RSI only); a composite policy "activates the appropriate getup policy without requiring any manual scripting" after a fall — [DeepMimic](https://ar5iv.labs.arxiv.org/html/1804.02717)
- DeepMimic learned a T-Rex and a dragon from artist-authored keyframes because no mocap existed (T-Rex walk 140 M samples, NR 0.979), while warning that "keyframed motions may not be physically correct at all" — [DeepMimic](https://ar5iv.labs.arxiv.org/html/1804.02717)
- AMP (Peng et al. 2021): added get-up clips "for getting up from the ground in various configurations" so the character can "recover from a fall and continue to perform a given task"; "The policy also discovers novel recovery behaviors that are not present in the dataset"; early termination disabled for getting up and rolling. Each policy 100–300 M samples, about 30–140 hours on 16 CPU cores, Bullet 1.2 kHz, 30 Hz policy, PD targets — [AMP, ar5iv](https://ar5iv.labs.arxiv.org/html/2104.02180)
- ASE (Peng et al. 2022): 10% of episodes start in a random fallen state made by dropping the character from random heights/orientations; the dataset (187 clips, about 30 min) contains no get-up clips; no separate recovery reward. Under 500 trials of 500–1000 N pushes for 0.5 s, the character "successfully recover[s] from all falls, with a mean recovery time of 0.31s and a maximum recovery time of 4.1s" — [ASE, ar5iv](https://ar5iv.labs.arxiv.org/html/2205.01906)
- ASE cost: 37-DoF humanoid, PD targets; Isaac Gym, 4096 environments on one V100, 120 Hz sim, 30 Hz control; "over 10 billion samples ... requiring about 10 days on a single GPU" — [ASE](https://ar5iv.labs.arxiv.org/html/2205.01906)
- PHC (Luo et al. 2023): a fail-state recovery primitive trained with reward 0.5 point-goal + 0.5 AMP style + 0.1 energy from random drops; the humanoid "should get up from the ground, approach the reference motion in a natural way, and resume motion imitation." Fallen-start recovery 95.0% within 5 s and 98.8% within 10 s (1000 trials); a keypoint variant 92.5% / 94.6% — [PHC, ar5iv](https://ar5iv.labs.arxiv.org/html/2305.06456)
- PHC: "We do not use any external forces"; UHC with residual force control tracked AMASS at 97.0% / 96.4% (train/test) but UHC without it only 84.5% / 62.6%; PHC 98.9% / 96.4% without forces. Cost: about 10 billion samples, "around 7 days" on one A100, 1536 parallel humanoids, Isaac Gym — [PHC](https://ar5iv.labs.arxiv.org/html/2305.06456)
- Residual Force Control (Yuan and Kitani, NeurIPS 2020) adds external residual forces to the policy's action space to follow motions the humanoid cannot physically reproduce (abstract/search summary only) — [arXiv 2006.07364](https://arxiv.org/abs/2006.07364)
- Frezzato, Tangri, Andrews (CGF 2022, "Synthesizing Get-Up Motions for Physics-based Characters"): DRL policies start lying down and follow input style curves through a latent pose embedding learned from a motion-capture database; follows user-drawn curves; works on rough and inclined terrain (abstract/search summary only) — [Eurographics DL](https://diglib.eg.org/items/d9302d75-fe63-4a27-a476-d52b543d54a3/full)
- StableMimic (Wu et al., arXiv preprint, 3 Aug 2026): perturbed resets around human get-up references (prone, supine, intermediate) teach a tracker to return to trackable states; "recovers in 100/100" push-to-fall trials; references used only in training, none at deployment; G1 robot shown qualitatively (abstract only, not peer-reviewed) — [arXiv 2608.02385](https://arxiv.org/abs/2608.02385)

### Inferences
- ASE and PHC show that a policy can learn to get up from arbitrary falls with no get-up data at all if (a) a share of episodes starts fallen and (b) a style prior keeps the motion human-like; but that prior was learned from real human mocap, which the project lacks. A hand-keyed miner animation set could stand in as AMP data, with DeepMimic's caveat that keyframes may be physically infeasible.
- PHC's numbers are the clearest evidence that tracking-plus-recovery works without root forces, and the UHC ablation shows how much tracking leans on residual forces when they are allowed (84.5% to 97.0%), so residual-force methods should be ruled out for this project.
- The GPU-scale budgets (10 B samples) are not reproducible in Unity on a CPU; the CPU-era budgets (DeepMimic 49–66 M samples for one get-up clip; AMP 100–300 M over 30–140 h on 16 cores) are the relevant order of magnitude for a Unity-trained policy.
- DeepMimic's 13-link, 45 kg humanoid is structurally close to the miner (13 parts, 40 kg).

### Gaps
- No paper here gives get-up success rates for DeepMimic's or AMP's get-up skills from varied fallen states.
- ASE's recovery evaluation pushes a standing character; mean 0.31 s recovery suggests many trials were stumbles, not full get-ups from lying; the paper does not separate them.
- Frezzato et al. full text (success rates, cost) not read.

## 3. Humanoid robots, 2024–2026: HumanUP, HoST, HiFAR, FRASA, OP3 soccer, MuJoCo Playground

### Takeaway
Robot get-up policies now reach 92–100% success in simulation and 78–100% on hardware, using staged curricula (discover then refine), randomized fallen poses, and heavy smoothness regularization (action-rate and torque penalties, L2C2, action bounds, action filters). Without those smoothness tools motion becomes violent or oscillatory, and real success drops sharply (HoST 20/20 vs 11/20 without L2C2). Small, reduced-DoF problems train in minutes; full-body ones need billions of GPU-sim steps.

### Cited Findings
- HumanUP (G1): Stage I discovery with task + soft-symmetry rewards, simplified collision mesh, canonical lying pose, weak regularization, motion under 1 s; Stage II tracks an 8x slowed version of that trajectory with the full URDF, about 20K supine and 20K prone randomized poses (10K each for training), varied terrain, dynamics randomization and strong regularization — [HumanUP](https://arxiv.org/html/2502.12152)
- HumanUP rewards: Stage I height, height change, uprightness, feet on ground, feet contact-force change, soft symmetry; Stage II DoF-position tracking (weight 8) with penalties on torque limits (−5), DoF limits (−5), termination (−50), energy, DoF velocity and torque — [HumanUP](https://arxiv.org/html/2502.12152)
- HumanUP compute: Isaac Gym, 1000 Hz physics, 50 Hz PD control, 4096 envs on one RTX 4090 or L40S; Stage I about 5 B simulation steps; wall-clock not reported — [HumanUP](https://arxiv.org/html/2502.12152)
- HumanUP sim success (held-out poses): supine 95.34%, prone 92.10%, roll prone-to-supine 94.40%. Ablations: without Stage II 24.82% / 27.59%; without posture randomization 65.39% / 30.25%; single-stage training failed; without full collision mesh 93.95% in sim but 0/5 on the real robot — [HumanUP](https://arxiv.org/html/2502.12152)
- HumanUP real G1 on six terrains: supine get-up 78.3% vs 41.7% for the manufacturer controller; rolling 98.3%; stands in about 6 s vs about 11 s. Failure modes: foot placement on slopes, slipping on snow; "unnatural hand-raising for balance" — [HumanUP](https://arxiv.org/html/2502.12152)
- HoST (G1, 35 kg, 1.32 m, 23 DoF): multi-critic PPO (task, style, regularization, post-task); three phases by base height; Isaac Gym 4096 envs, 50 Hz policy; training compute not reported — [HoST](https://arxiv.org/html/2502.08378)
- HoST training-only assistance: an upward force on the base starting at 200 N, reduced by 20 N each time the robot reaches the target head height, floored at 0 N, applied only when the trunk is near-vertical; without it success was 0 on ground, wall and slope. Action bound β annealed from 1 to 0.25 to limit torque and speed — [HoST](https://arxiv.org/html/2502.08378)
- HoST sim success: ground 99.5%, platform 99.8%, wall 94.2%, slope 98.5%; single critic 0% everywhere; no style reward: wall 0%, slope 21.4%; no action bound: smoothness 9.5–16.4 vs 2.9–5.3 ("violent") — [HoST](https://arxiv.org/html/2502.08378)
- HoST real robot: 20/20 (5/5 on each terrain); without L2C2 smoothness regularization 11/20 (slope 0/5), oscillating motions. Payload 3/3 up to 10 kg, 2/3 at 12 kg; sim torque dropout 3/3 up to 15%, 0/3 at 20% — [HoST](https://arxiv.org/html/2502.08378)
- HoST: training supine and prone together "degraded performance" (interference between rollouts); prone needed extra hip-deviation constraints and thigh-orientation rewards; side-lying worked without tuning — [HoST](https://arxiv.org/html/2502.08378)
- HiFAR (Booster T1, 118 cm, 23 DoF; IROS 2025): Stage 1 sagittal-plane joints only, lenient constraints; Stage 2 adds hip roll, 3D falls, realistic torque/velocity limits, more randomization; network grown with zero-initialized layers. Real robot 20/20 supine and 20/20 prone, 2.7–2.9 s average; at 75% torque limit supine success drops to 60% in sim — [HiFAR](https://arxiv.org/html/2502.20061v1)
- FRASA (Sigmaban kid-size, about 70 cm, 7.5 kg): only 5 DoF controlled (sagittal, mirrored); CrossQ in MuJoCo, 16 parallel envs, trained in 13–37 minutes (235k–575k steps) on an i9-12900 + RTX 3060. Real stand-up time prone 2.135 s vs 3.154 s for the keyframe controller, supine 2.678 s vs 5.06 s; early transfers showed tremors, fixed by lower motor gains and higher inference rate; symmetry limits lateral behaviours — [FRASA](https://arxiv.org/html/2410.08655v3)
- OP3 soccer (Haarnoja et al.): three key poses taken from the robot's pre-programmed get-up became goal poses; the policy learns to reach any interpolated target pose, new target every ~1.5 s; conditioning on the final standing pose makes it get up. Real get-up 0.93 s vs 2.52 s scripted; actions exponentially filtered (0.8 old / 0.2 new); without randomization and perturbations zero-shot transfer failed — [OP3 soccer](https://arxiv.org/html/2304.13653)
- MuJoCo Playground: Go1 fall-recovery policy trained in MuJoCo/MJX transferred to the real robot; Go1Getup runs about 96,000 PPO steps per second on an A100; policies trained 200 M steps flat + 100 M rough; "most tasks" train in minutes on one GPU. No humanoid get-up task is listed — [MuJoCo Playground](https://arxiv.org/html/2502.08844v1)

### Inferences
- The common recipe across HumanUP, HoST, HiFAR and Tao et al.: relax the problem to discover a get-up (strength, assist force, simple collisions, fewer DoF), then harden it (realistic limits, full collisions, randomized starts, smoothness), and keep the hardened policy as the only thing that runs.
- For a stylised game body, the robot smoothness toolkit (action-rate and acceleration penalties, L2C2, action bounds, an action low-pass filter, energy/torque penalties) is directly reusable and is what separates "robust" from "violent" in these papers.
- FRASA and HiFAR suggest a cheap first experiment for the miner: a sagittal, left-right-mirrored action space trained to stand from supine and prone, then extended to 3D.
- HoST's supine/prone interference and HumanUP's separate prone-to-supine roll policy both suggest splitting "roll to the back" and "rise from the back" for the miner, which is the strategy Small already lands into.

### Gaps
- No robot paper reports wall-clock training time for full-body get-up (HumanUP, HoST, HiFAR all omit it).
- No hardware success rates are reported for side-lying or seated starts.

## 4. Tracking a get-up reference with joint torques only (no root forces)

### Takeaway
Tracking a single get-up reference is brittle when the start differs: HumanUP's tracker drops from 92–95% to 30–65% without randomized start poses. It becomes robust when training starts from many perturbed or randomized fallen states, and when the reference is the policy's own physically valid motion slowed down (Tao et al., HumanUP) rather than an arbitrary keyframe. Hand-keyed references work (DeepMimic T-Rex/dragon, Disney's robot character, OP3 key poses) but can be infeasible, and policies "cheat" around them.

### Cited Findings
- Without residual forces, earlier trackers lost much robustness (UHC 62.6% vs 96.4% on test motions); PHC closed that gap with no external forces at 96.4% — [PHC](https://ar5iv.labs.arxiv.org/html/2305.06456)
- HumanUP Stage II is a tracking policy (DoF-position tracking weight 8) of a single slowed motion, started from 10K randomized lying poses; 92–95% success on held-out poses; without posture randomization 30–65% — [HumanUP](https://arxiv.org/html/2502.12152)
- Tao et al.'s slow stage tracks the weak policy's own trajectory and uses reference-state initialization and early termination at 0.5 m COM deviation; it pauses in statically stable states but "loses balance in more dynamic states" — [Tao et al., ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307)
- OP3: goal-conditioned tracking of interpolations between three hand-picked key poses, reward = scaled joint-position error × torso gravity-direction error; produced a real-robot get-up faster than the scripted original — [OP3 soccer](https://arxiv.org/html/2304.13653)
- Disney bipedal character: artist-authored Maya animations (episodic), perpetual standing and procedurally composed walking tracked by PPO policies outputting PD setpoints at 50 Hz; imitation + regularization (torques, joint acceleration, action rate and acceleration) + survival; for the jump the policy "tends to cheat by keeping the toes on the ground", fixed by a larger contact reward; about 10 h of robot runtime without a fall. No get-up is described — [Disney, arXiv 2501.05204](https://arxiv.org/html/2501.05204v1)
- Disney training cost as stated: "Each policy is trained for 100 000 iterations, which corresponds to a computation time of around 22 days on a Nvidia RTX 4090", while "1500 iterations or 30 min are enough to preview the nominal behavior" — [Disney](https://arxiv.org/html/2501.05204v1) (note: the two figures are inconsistent; 1500 iterations per 30 min implies about 33 h for 100k iterations)
- DeepMimic: tracking requires RSI and ET to learn hard motions; keyframed motions "may not be physically correct at all" — [DeepMimic](https://ar5iv.labs.arxiv.org/html/1804.02717)
- StableMimic: perturbed resets around get-up references, a separate recovery expert blended by a proprioceptive gate; 100/100 push-to-fall recoveries (abstract only) — [arXiv 2608.02385](https://arxiv.org/abs/2608.02385)

### Inferences
- The project's existing searched key poses could be reused in two ways that keep the body responsible: as OP3-style goal poses for a goal-conditioned policy, or as a reference for a HumanUP-style Stage II tracker trained from many randomized falls. The policy, not the poses, supplies the closed-loop correction that the current open-loop approach lacks.
- A tracker that sees only one start state will reproduce the current "chaotic from try to try" problem; randomized fallen starts (thousands of poses, from real falls of each miner) are the main robustness lever in the evidence.
- Physically infeasible keys (for the miner's heavy head and short arms) are best avoided by letting RL discover the motion first and tracking that, as Tao et al. and HumanUP do.

### Gaps
- No source measures how far a start state can deviate from a single get-up reference before a tracking policy fails; only the with/without randomization ablation exists.
- No published get-up tracker for a non-human (stylised) proportion was found.

## 5. Training and running in Unity (ML-Agents, Inference Engine/Sentis, external simulators)

### Takeaway
Unity ML-Agents 4.x can train PPO or SAC policies directly on ArticulationBody characters (it ships ArticulationBody sensors) and runs the exported ONNX network through Unity's Inference Engine (formerly Sentis), where CPU/Burst is recommended for small models. Training inside Unity avoids any sim-to-sim gap but is CPU-bound with no published throughput for ragdoll get-up; training in MuJoCo or Isaac is much faster per sample but needs the PhysX drive and contact model reproduced and a validation/fine-tune pass in Unity.

### Cited Findings
- ML-Agents 4.0 adds Inference Engine support and moves documentation to the Unity package docs (forum announcement, search snippet) — [Unity Discussions](https://discussions.unity.com/t/ml-agents-4-0-0-is-now-available/1681770); the registry lists 4.1.0 depending on "com.unity.ai.inference" 2.6.1 (search snippet) — [package registry](https://packages.unity.com/com.unity.ml-agents); Unity 6.4 docs list the 4.0 line — [Unity manual](https://docs.unity3d.com/Manual/com.unity.ml-agents.html)
- ML-Agents' Sensors namespace includes ArticulationBodyPoseExtractor, ArticulationBodyJointExtractor, ArticulationBodySensorComponent and PhysicsBodySensor ("observations for groups of Rigidbodies or ArticulationBodies") — [ML-Agents 4.0 API](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/api/Unity.MLAgents.Sensors.html)
- Walker example: 26-DoF humanoid, 10 agents per scene sharing one behaviour, 243 observations, 39 continuous actions (target rotations and strength per joint), multiplicative reward, benchmark mean reward 2500; ArticulationBody is not mentioned — [ML-Agents examples](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Learning-Environment-Examples.html)
- Walker PPO config: batch 2048, buffer 20480, lr 3e-4 linear, 3 epochs, 3 layers × 256, normalization on, gamma 0.995, time horizon 1000, max 30 million steps — [Walker.yaml](https://raw.githubusercontent.com/Unity-Technologies/ml-agents/develop/config/ppo/Walker.yaml)
- Parallelism: `--num-envs=<n>` launches concurrent Unity instances, "constrained by the resources on the machine"; multiply buffer_size by num-envs; time_scale 20 by default; PPO and SAC trainers available — [Training ML-Agents](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Training-ML-Agents.html)
- Inference: models are `.onnx`; "CPU inference (Burst) will be faster than GPU inference" for most ML-Agents models; IL2CPP faster than Mono; externally trained models are not supported through ML-Agents, use the inference package directly — [ML-Agents Inference Engine page](https://docs.unity3d.com/Packages/com.unity.ml-agents@4.0/manual/Inference-Engine.html)
- A forum user reported 4.0 training about 6x slower than 3.0.0-exp.1 on a Mac, and a CUDA/GPU tensor mixing problem with threaded training (search snippet) — [Unity Discussions](https://discussions.unity.com/t/ml-agents-4-0-0-is-now-available/1681770)
- Marathon Environments (Booth and Booth 2019): open-source continuous-control benchmarks (humanoid, walker, hopper, ant) in Unity's native PhysX, trained with ML-Agents, inspired by DReCon and DeepMimic; community-maintained, setup referenced mlagents 0.14.1 (search snippet; PDF not readable) — [GitHub](https://github.com/Unity-Technologies/marathon-envs); [arXiv](https://arxiv.org/abs/1902.09097)
- Community evidence is thin: one developer could not get a ConfigurableJoint ragdoll past learning to stand (search snippet) — [ngmeijer wiki](https://github.com/ngmeijer/Advanced-Tools/wiki/22%E2%80%9004%E2%80%902024); a hobbyist Walker-based agent sometimes rolled over around 80,000 steps (search snippet) — [emsbay devlog](https://emsbay.itch.io/crawler525/devlog/949649/documentation)
- Unity articulations use a reduced-coordinate Featherstone solver (search snippet) — [Unity 6 ArticulationBody](https://docs.unity.cn/6000.0/Documentation/ScriptReference/ArticulationBody.html); Unity moved to PhysX 4.1 (TGS solver) in 2020.1 (search snippet) — [Unity blog](https://unity.com/blog/industry/use-articulation-bodies-to-easily-prototype-industrial-designs-with-realistic-motion-and)
- Sim-to-sim practice: Humanoid-Gym validates Isaac Gym policies in MuJoCo to confirm robustness across engines — [Humanoid-Gym](https://arxiv.org/abs/2404.05695); under unseen contact dynamics, plain domain randomization collapsed within 6 s in one study (snippet) — [arXiv 2504.06585](https://arxiv.org/pdf/2504.06585); training across several simulators reduced sim-to-sim tracking error (snippet) — [PolySim](https://arxiv.org/pdf/2510.01708v1)
- Desktop-scale reference points: Tao et al. about 24 h per stage on a 32-core CPU + RTX 2070 (MuJoCo, SAC) — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307); FRASA 13–37 min on an i9 + RTX 3060 for a 5-DoF problem — [FRASA](https://arxiv.org/html/2410.08655v3); AMP 30–140 h on 16 CPU cores — [AMP](https://ar5iv.labs.arxiv.org/html/2104.02180); HumanUP 5 B steps on one RTX 4090 in Isaac Gym — [HumanUP](https://arxiv.org/html/2502.12152)

### Inferences
- Route A, train in Unity: build a headless training scene with many miners (Walker uses 10 per scene) on the same ArticulationBody prefab, drives and contact settings as the game, observations via the ArticulationBody sensors or a custom sensor, actions = PD drive targets (optionally with a per-joint strength scale, as Walker does, capped at realistic limits). No sim-to-sim gap is the main advantage. The budget is likely tens to hundreds of millions of steps per policy (DeepMimic/AMP scale); whether that is hours or weeks depends on Unity's steps per second for a 13-part articulation, which must be measured first.
- Route B, train elsewhere and run in Unity: MuJoCo/MJX or Isaac Lab give far higher throughput, but Unity's drive model (stiffness, damping, force limit), joint limits, toe links, friction and solver must be replicated, and the policy validated and probably fine-tuned in Unity. It also means loading an external ONNX model through the Inference Engine directly, since ML-Agents only supports its own trainers' models.
- Run-time cost (an estimate, not sourced): a 3×256 MLP with roughly 100–250 inputs and about 30–40 outputs is on the order of 0.15–0.2 million multiply-adds per decision; at 30–50 decisions per second per miner this is small next to the physics step on Burst CPU. It must be profiled in the game.
- A sagittal, mirrored first stage (FRASA/HiFAR) would make the first Unity experiment cheap enough to measure feasibility in hours rather than days.

### Gaps
- No published steps-per-second or wall-clock figure for training a ragdoll or ArticulationBody humanoid in Unity ML-Agents was found.
- No measured per-inference CPU cost for a small MLP in Inference Engine/Sentis was found.
- Which PhysX version Unity 6 ships, and whether its ArticulationBody behaves like Isaac Lab's PhysX 5 articulations, was not confirmed.
- No documented case of a learned get-up trained in Unity was found.

## 6. Risks: unnatural motion, reward hacking, body changes, per-miner training, determinism

### Takeaway
The main risks are jittery or violent motion (mitigated by smoothness terms, action bounds, slowing, weaker bodies), reward hacking (seen even in polished systems), retraining when the body changes (a pre-trained policy degrades on a modified lower body; shape-conditioned policies exist), and the gap between a deterministic policy and chaotic contact physics. None of the evidence settles whether the result would look natural to the director.

### Cited Findings
- Jitter/violence: Tao et al.'s method re-implemented on G1 had action jitter 5.39–11.73 vs 0.39–0.56 for HumanUP — [HumanUP](https://arxiv.org/html/2502.12152); HoST without action bound was "violent", without L2C2 real success fell to 11/20 with oscillation — [HoST](https://arxiv.org/html/2502.08378); FRASA saw tremors on transfer — [FRASA](https://arxiv.org/html/2410.08655v3)
- Unnatural learned details: HumanUP's "unnatural hand-raising for balance" — [HumanUP](https://arxiv.org/html/2502.12152); high-strength policies are "more dynamic, less natural" — [Tao et al.](https://ar5iv.labs.arxiv.org/html/2205.00307); OP3's learned get-up is about 2.7x faster than the scripted one — [OP3 soccer](https://arxiv.org/html/2304.13653)
- Reward hacking: Disney's jump policy kept the toes on the ground — [Disney](https://arxiv.org/html/2501.05204v1); HoST without the style reward failed on wall (0%) and slope (21.4%) — [HoST](https://arxiv.org/html/2502.08378)
- Sim gaps: HumanUP's simplified-collision policy scored 93.95% in sim but 0/5 on hardware — [HumanUP](https://arxiv.org/html/2502.12152)
- Body changes: Won and Lee (TOG 2019) train one controller parameterized by body-shape features (e.g. leg and arm length) with adaptive shape sampling, because controllers otherwise "need to be relearned from scratch" (search snippet) — [MorphCon](https://mrl.snu.ac.kr/publications/ProjectMorphCon/MorphCon.html); AdaptNet reports that without adaptation a pre-trained policy performs poorly on modified bodies, especially the lower body (search snippet) — [AdaptNet](https://arxiv.org/pdf/2310.00239); XHugWBC drives seven real humanoids with one policy via morphological randomization (search snippet) — [arXiv 2602.05791](https://arxiv.org/html/2602.05791)
- Strength sensitivity: HiFAR supine success fell to 60% at 75% torque limits — [HiFAR](https://arxiv.org/html/2502.20061v1); HoST failed at 20% torque dropout — [HoST](https://arxiv.org/html/2502.08378)
- Determinism: Unity's Enhanced Determinism makes simulation consistent "provided that the game inserts the actors in a deterministic order" and "sacrifices some performance"; TGS gives better convergence for high mass ratios — [Unity Physics settings](https://docs.unity3d.com/6000.0/Documentation/Manual/class-PhysicsManager.html)

### Inferences
- Plan for one policy per miner, fine-tuned from a shared one, or one policy with body-shape inputs trained on all three; any later change to masses, drive strength, toes or collision shapes should be treated as needing at least a fine-tune and a re-measure.
- A learned policy is deterministic for identical inputs, but small state differences still diverge through contact; its value is closed-loop correction across that divergence, which is what the randomized-start success rates measure.
- Naturalness is the director's call; the evidence only shows which levers make motion slower and smoother (weak body, slowed tracking, smoothness rewards, style priors from reference motion).

### Gaps
- No study measured naturalness against human get-up data or with viewers for these get-up methods.
- Whether ML-Agents inference is bit-identical across machines was not checked.

## 7. Per-method assessment against the project's rule

### Takeaway
Every RL method here can be deployed as "policy outputs PD targets, body does the rest", which meets the rule at run-time. Residual-force tracking (RFC/UHC) does not. Several methods use scaffolds only during training (extra strength, an assist force, simplified collisions), and whether that is acceptable is a design question. For this project the best-evidenced path is a two-stage discover-then-track policy with randomized fallen starts and smoothness terms, trained per miner.

### Cited Findings
- Run-time actions are PD targets in Tao et al. — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307); DeepMimic — [ar5iv](https://ar5iv.labs.arxiv.org/html/1804.02717); AMP — [ar5iv](https://ar5iv.labs.arxiv.org/html/2104.02180); ASE — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.01906); PHC (no external forces) — [ar5iv](https://ar5iv.labs.arxiv.org/html/2305.06456); HumanUP and HoST (50 Hz PD targets on real robots) — [HumanUP](https://arxiv.org/html/2502.12152), [HoST](https://arxiv.org/html/2502.08378)
- Training-only scaffolds: Tao strong body — [ar5iv](https://ar5iv.labs.arxiv.org/html/2205.00307); HoST assist force annealed to 0 N — [HoST](https://arxiv.org/html/2502.08378); HumanUP simplified collision mesh in Stage I — [HumanUP](https://arxiv.org/html/2502.12152)
- External forces in the controller itself: RFC — [arXiv 2006.07364](https://arxiv.org/abs/2006.07364)

### Inferences
Assessment per method for the miner (the costs are estimates from the cited budgets, not measurements):

| Method | Body fully responsible at run-time? | Robustness to varied falls (evidence) | Naturalness (evidence) | Run-time cost per miner | Effort in a Unity C# project | Main risks |
|---|---|---|---|---|---|---|
| Tao et al. 2022 (strong→weak→slow, no mocap) | Yes; extra strength only in training | Not reported by authors; 92.6–99% in a G1 re-implementation | Subjectively more natural at 40–60% strength; jittery in re-implementation | One small MLP, 40 Hz | High: three stages, a reference-generation run, SAC or PPO; about 24 h per stage on a CPU desktop in MuJoCo | Strategy cannot be chosen; jitter without smoothness terms |
| HumanUP-style two-stage (discover, then track 8x slowed from randomized poses) | Yes | 92–95% sim, 78% real supine | Smooth by metrics; occasional odd arm raising | One MLP, 50 Hz | High; designed for GPU scale (5 B steps) | Budget may be too large for Unity on CPU unless reduced |
| HoST-style multi-critic with assist force curriculum | Yes at run-time; external force in training | 94–99.8% sim, 20/20 real | Smooth only with L2C2 and action bounds | One MLP | High; multi-critic PPO is not in ML-Agents out of the box | Assist force is exactly what the director forbids at run-time; acceptability as a training aid is a design question |
| OP3-style goal-conditioned key poses | Yes | Robust on real robot | Faster than scripted; may look hurried | One MLP | Medium; reuses the project's searched key poses | Speed and style differ from intent |
| DeepMimic/AMP with hand-keyed miner get-up clips | Yes | Not reported for get-up | Follows the clip's style | One MLP | Medium–high; needs animated clips, 50–300 M samples | Infeasible keys; style tied to clips |
| ASE / PHC style prior + fallen starts | Yes (PHC: no forces) | ASE all 500 recovered; PHC 95% within 5 s | Human-like via mocap prior | One or two MLPs | Very high; 10 B samples on GPU; needs a motion dataset | No mocap of a miner exists |
| Residual force tracking (RFC/UHC) | No | — | — | — | — | Breaks the rule |
| FRASA/HiFAR reduced-DoF first stage | Yes | 100% real for supine/prone (HiFAR) | Symmetric, limited variety | One MLP | Low for the first stage (minutes–hours) | Must still be extended to 3D |

### Gaps
- No method has been demonstrated on a body with the miner's proportions, in Unity PhysX, under a "no assist even in training" constraint; all cost estimates for this project remain to be measured.
