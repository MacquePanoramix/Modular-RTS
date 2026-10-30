# Learned controllers that generalize across body morphology and strength (a basis for player-built bodies)

*Compiled 30 Oct 2026 for Wonder Gather (Unity 6 / PhysX, rigid parts joined by ConfigurableJoints with PD drives capped by strength). Each result carries its year. "[abstract/snippet only]" means I saw the claim only in an abstract or a search-result summary, not in the full paper. "(my arithmetic)" means I derived the number from cited figures. Earlier project research (designed feedback controllers, SAMCON, Al Borno 2017, Faloutsos 2001, Stückler 2006, DReCon, PuppetMaster) is not repeated here.*

## 1. Which works train one policy over randomized morphologies, and how far does it generalize?

### Takeaway
One learned policy can control bodies it never saw. Almost all of the evidence is for **locomotion on flat or mildly rough ground**:
- **Quadrupeds generalize well.** GenLoco (2022) transferred zero-shot to 10 unseen simulated robots and 3 real ones, and held up to about 2× its largest training size.
- **Humanoids are the hardest class in every study.** URMA (2024) showed no meaningful zero-shot transfer to humanoids. In the 2025 scaling-law study, the H1 humanoid did worse than the Go2 quadruped. XHugWBC (2026) reached about 85 % of a specialist's return on 12 unseen simulated humanoids.
- **Ranges are modest.** Each body parameter typically varies by ±20–50 %. Nothing published covers child-to-giant humanoid proportions with one policy.
- **Getting up is almost uncovered.** Only one small 2025 study addresses it (section 4).

### Cited Findings
**How far each result reaches (summary table)**

| Work (year) | Variation trained | Result on unseen bodies | Source |
|---|---|---|---|
| GenLoco (2022) | size α ∈ [0.8, 1.2] applied to base/leg ranges that each span about 3×; link masses 0.5–1.5×; motor strength 0.8–1.2×; PD gains scaled with mass | 10 unseen simulated quadrupeds and 3 real ones; pacing does not degrade until about 2× the largest training size | [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309) |
| ManyQuadrupeds (ICRA 2024) | 2–200 kg, 18–100 cm standing height, 12 or 16 DoF | real Go1 and A1; carries a 15 kg load | [arXiv 2310.10486](https://arxiv.org/abs/2310.10486) |
| URMA (2024) | 16 robots with 8–24 joints, plus broad dynamics randomization | A1 transfers well; humanoids do not transfer zero-shot | [URMA](https://arxiv.org/html/2409.06366) |
| Embodiment scaling laws (2025) | 1,012 generated robots varying link scale, size, knee count and joint limits | real Go2 good; real H1 weaker | [arXiv 2505.05753](https://arxiv.org/html/2505.05753) |
| LocoFormer (CoRL 2025) | about 100k generated robots | 0.96 vs a per-robot expert's 0.99 (normalized, sim); real G1, H1, Go2 | [LocoFormer](https://arxiv.org/html/2509.23745v1) |
| XHugWBC (ICML 2026) | G1 template: stretch 0.8–1.5 (some axes 0.5–1.5), density 0.8–1.5, torque limits 0.7–1.0, 12–32 DoF | about 85 % of specialist return on 12 unseen humanoids; 7 real robots | [XHugWBC](https://arxiv.org/html/2602.05791) |
| Get-up across morphologies (Dec 2025) | 7 small humanoids, 0.48–0.81 m and 2.8–7.8 kg | held-out success 17–42 % on 6 of 7 robots, 72 % on the seventh | [arXiv 2512.12230](https://arxiv.org/html/2512.12230v1) |

**GenLoco (Feng et al., 2022; quadrupeds with a fixed 12-DoF template)**
- **Body randomization, drawn anew each episode.** — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309)
  - Base length α×[0.134, 0.400] m, width α×[0.097, 0.291] m, height α×[0.057, 0.171] m.
  - Calf length α×[0.11, 0.33] m; thigh length 0.75–1.25× the calf.
  - Base density 400–1200 kg/m³; link masses 0.5–1.5× nominal.
  - PD gains scaled by (robot mass / 12.458 kg) × [0.7, 1.3].
- **Dynamics randomization.** Motor strength 0.8–1.2×, link inertia 0.5–1.5×, friction 0.5–1.25, latency 0–40 ms. — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309)
- **Policy.** — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309)
  - Feed-forward MLP [1024, 512] with no morphology input.
  - Inputs: a 15-step history of IMU orientation, joint positions and past actions, plus a gait phase.
  - Outputs: low-pass-filtered joint targets, tracked by PD at 30 Hz.
  - Trained with PPO as motion imitation of retargeted dog mocap.
  - Cost: 800 M samples, about 2 weeks on 16 CPU workers in PyBullet.
- **Test robots.** — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309)
  - Simulation, none seen in training: A1, Aliengo, Go1, Mini Cheetah, Sirius, Laikago, Spot, SpotMicro, ANYmal-B and ANYmal-C. ANYmal's inverted knees fall outside the template and still transferred.
  - Real: A1, Mini Cheetah, and Sirius (zero-shot, days after its hardware was finished).
- **Real pacing returns.** — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309)
  - Mini Cheetah: GenLoco 0.743 ± 0.092; a Mini-Cheetah-specific policy failed 6/6.
  - A1: GenLoco 0.773 vs 0.696 for an A1-specific policy.
- **Out of distribution.** Pacing does not degrade until size exceeds about 2× the largest training size. The authors say performance "drops sharply" for larger robots. The policy cannot handle a different DoF count. — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309)

**ManyQuadrupeds (Shafiee, Bellegarda, Ijspeert; ICRA 2024)** [abstract only]
- The policy drives a CPG rhythm generator. Only a per-robot "Pattern Formation" layer, which scales stride height and length, differs between robots. — [arXiv 2310.10486](https://arxiv.org/abs/2310.10486)
- It covers 12- and 16-joint robots, three morphologies, masses of 2–200 kg and heights of 18–100 cm. It was tested on real Go1 and A1 and stayed robust with a 15 kg load (about 125 % of the A1's mass). — [arXiv 2310.10486](https://arxiv.org/abs/2310.10486)

**URMA, "One Policy to Run Them All" (Bohlinger et al., 2024; revised Oct 2025)**
- **Training robots.** 16 in total: 9 quadrupeds, 5 humanoids (Talos, OP3, Nao, G1, H1), Cassie and one hexapod, with 8–24 joints each. — [URMA](https://arxiv.org/html/2409.06366)
- **Randomization, resampled about twice per episode.** — [URMA](https://arxiv.org/html/2409.06366)
  - Trunk mass, inertia and CoM; foot size.
  - Torque and velocity limits, damping, rotor inertia, stiffness, friction, control ranges.
  - PD gains, action scale, motor strength, joint offsets, delays.
  - Gravity and contact stiffness.
- **Training cost.** — [URMA](https://arxiv.org/html/2409.06366)
  - 100 M steps per robot (1.6 B total) in CPU MuJoCo at 50 Hz, using JAX (RL-X).
  - 20 CPU cores plus one RTX 3090 or A5000; about 2.5 days per run.
  - CPU simulation was the bottleneck. The network has about 430k parameters.
- **Results.** — [URMA](https://arxiv.org/html/2409.06366)
  - The unseen A1 transfers well (figures only).
  - Real deployment worked on the A1, the unseen MAB Honey Badger, and the Silver Badger.
  - H1 and Talos showed "no meaningful zero-shot transfer". Fine-tuning from URMA still beat training from scratch within 20 M steps.
  - One reward set for all robots was only slightly worse than per-robot tuned coefficients.

**Towards Embodiment Scaling Laws in Robot Locomotion (2025)**
- **GenBot-1K.** 1,012 generated robots: 348 humanoids, 332 quadrupeds and 332 hexapods. — [arXiv 2505.05753](https://arxiv.org/html/2505.05753)
  - Varied: link scaling, overall size, knee count and joint limits.
  - Held fixed: mass distribution, joint damping and actuation type.
- **Method.** One PPO expert per robot in Isaac Lab (4,096 environments), then behaviour-cloning distillation into one URMA policy. The student uses multi-head attention and a descriptor built from the robot's URDF. — [arXiv 2505.05753](https://arxiv.org/html/2505.05753)
- **Cost.** — [arXiv 2505.05753](https://arxiv.org/html/2505.05753)
  - Experts: about 5 days on 160 RTX 4090/3090 GPUs, more than 2 trillion simulation steps.
  - Distillation: about 2 B samples, about 1 week on one H100.
- **Results.** — [arXiv 2505.05753](https://arxiv.org/html/2505.05753)
  - More training robots gave up to about 2× the cumulative reward.
  - Quadrupeds and hexapods saturate at about 100 robots; humanoids keep improving.
  - Giving more data to 5 % of the robots saturates at about 4× the base data. Diversity mattered more than data volume.
  - Real Go2 and H1 worked zero-shot, with H1 worse.
  - With its knee limit cut by 20 %, Go2 walked with a stable limp.

**LocoFormer (Liu, Pathak, Agarwal, Skild AI; CoRL 2025)**
- **Training.** About 100k generated robots: quadrupeds, bipeds and wheeled versions of each. It randomizes kinematics, mass and CoM, plus "large variations in inertia, control gains and joint limits". — [LocoFormer](https://arxiv.org/html/2509.23745v1); [PMLR](https://proceedings.mlr.press/v305/liu25a.html)
- **Architecture.** — [LocoFormer](https://arxiv.org/html/2509.23745v1)
  - No explicit morphology input.
  - Transformer-XL with 6 layers and segment length 128.
  - Memory up to 896 steps (about 18 s at 50 Hz); the deployed KV cache holds 255 steps.
  - The context spans episode boundaries, so the policy learns from earlier falls.
- **Simulation, 10 unseen robots × 1,000 environments, normalized displacement.** — [LocoFormer](https://arxiv.org/html/2509.23745v1)

  | Variant | Score |
  |---|---|
  | LocoFormer, zero-shot | 0.96 |
  | LocoFormer, after 5 s of adaptation | 0.98 |
  | Per-robot expert | 0.99 |
  | Explicit privileged-kinematics "Conditioning" baseline | 0.78 |
  | GRU | 0.37 |

- **Real robots, zero-shot.** G1, H1, Go2 and Go2-W. A locked knee produced a three-legged gait within 2–3 s. With the lower legs cut off, it walked after 7–8 s. — [LocoFormer](https://arxiv.org/html/2509.23745v1)
- **Cost and scope.** Total compute is not stated; one 6-layer ablation ran 4K iterations in 28 h. The authors call it "highly resource-intensive". It does not stand up from the ground. — [LocoFormer](https://arxiv.org/html/2509.23745v1)

**XHugWBC (ICML 2026), cross-humanoid whole-body control**
- **Randomization of a G1 template (plus 3 head joints).** — [XHugWBC](https://arxiv.org/html/2602.05791)
  - Mass-density scale about [0.8, 1.5]; stretch mostly [0.8, 1.2]–[0.8, 1.5], and [0.5, 1.5] on some axes; shear ±0.1.
  - Torque limits [0.7, 1.0]; velocity and range-of-motion limits about [0.8, 1.0].
  - Waist, arm and head joints randomly fixed, giving 12–32 active DoF.
  - PD gains and torque limits scaled linearly with total mass.
  - Inertia reparameterized so the bodies stay physically consistent.
- **Simulation results on 12 held-out humanoids.** — [XHugWBC](https://arxiv.org/html/2602.05791)
  - The generalist reaches about 85 % of specialist return; both survive 100 %.
  - Velocity-tracking error is 0.084 / 0.116 m/s, against 0.060 / 0.079 for specialists.
  - Fine-tuning from the generalist ends about 10 % above the specialist.
- **Real robots, zero-shot.** 7 robots (Booster T1, Fourier N1, G1 23- and 29-DoF, Agibot X2, Dobot Atom, H1-2), 5 trials each, 100 % task success. The real-vs-sim velocity gap ranged from 27 % to 108 % depending on the robot. — [XHugWBC](https://arxiv.org/html/2602.05791)
- **Not covered.** Training compute and the simulator are not given in the readable text. There is no fall recovery. — [XHugWBC](https://arxiv.org/html/2602.05791)

**Other 2026 cross-humanoid work** [snippet only]
- **URMAv2 (TU Darmstadt, GRC 2026).** One policy trained on 50 legged robots transfers zero-shot to unseen real humanoids and quadrupeds. Humanoid zero-shot is clearly lower than quadruped. — [GRC 2026 PDF](https://www.ias.informatik.tu-darmstadt.de/uploads/Team/NicoBohlinger/grc_2026.pdf)
- **Shared latent space.** A latent space shared by humans and several humanoids. New robots join through lightweight embedding training. — [arXiv 2601.15419](https://arxiv.org/pdf/2601.15419v1)
- **SLMP (Mar 2026).** A latent motion prior whose abstract claims generalization across humanoid morphologies, with no numbers. — [arXiv 2603.01294](https://arxiv.org/abs/2603.01294)

**Earlier morphology-agnostic RL (mostly simple MuJoCo or UNIMAL bodies)**
- **NerveNet.** A graph network over the body graph that matches MLPs on standard MuJoCo tasks. It was the only model that walked zero-shot on new centipede variants: policies trained on CentipedeSix walked on all of them. — [NerveNet](http://www.cs.toronto.edu/~tingwuwang/nervenet.html)
- **SMP (ICML 2020).** One identical module per actuator, with message passing between modules. It generalizes to unseen planar hoppers, quadrupeds and bipeds. — [arXiv 2007.04976](https://arxiv.org/abs/2007.04976)
- **Amorpheus (ICLR 2021).** The morphology graph does not help graph networks. A transformer that ignores morphology outperforms them. — [arXiv 2010.01856](https://arxiv.org/abs/2010.01856)
- **AnyMorph (ICML 2022).** Learns a morphology representation from the RL objective and improves the zero-shot state of the art on the standard agent-agnostic benchmark. [abstract only] — [arXiv 2206.12279](https://arxiv.org/abs/2206.12279)
- **MetaMorph (2022).** — [MetaMorph](https://ar5iv.labs.arxiv.org/html/2203.11931)
  - 100 training and 100 test UNIMAL robots, about 15–20 DoF each.
  - Transformer with 5 layers, embedding 128, feed-forward 1024; baselines were matched at about 3.3 M parameters.
  - 1e8 timesteps, about 1.5 GPU-days on an RTX 2080, against about 30 GPU-days for Amorpheus.
  - About 5× more sample-efficient than training 100 separate MLPs (5e6 steps each).
  - Performance drops zero-shot on new morphologies. A graph-network baseline saturates 2–3× lower. Without morphology input it fails.
- **Body Transformer (2024).** Masked attention along the sensor/actuator body graph; it beats an MLP and a plain transformer. The abstract shows no cross-morphology test. — [arXiv 2408.06316](https://arxiv.org/abs/2408.06316)
- **CrossLoco (2023).** Translates human motion to a legged robot through cycle-consistency RL. It is retargeting, not one policy across many bodies. — [arXiv 2309.17046](https://arxiv.org/abs/2309.17046)

### Inferences
- **Wonder Gather's likely regime.** If player bodies share one skeleton (same joints) and vary in proportions, masses and strength, that is the GenLoco/XHugWBC regime: a fixed template with continuous parameters. That is much easier than the regime where topology varies (UNIMAL, GenBot-1K, LocoFormer). If players can add or remove limbs, it becomes the per-joint-token regime (URMA, MetaMorph, XHugWBC's 32-slot canonical joint vector). The attribute list is undecided, so both stay open.
- **Coverage.** Published single-policy coverage is roughly ±20–50 % per link, with degradation starting around 2× size for an easy gait (GenLoco). Extreme player builds (child-like, giant, very heavy) would lie outside every humanoid result found. ManyQuadrupeds' 100× mass range came from hand-structuring the action space (CPG plus a per-robot scaling layer), not from a plain policy.
- **Humanoids need more diversity.** Every multi-embodiment study found humanoids the weakest class, and humanoid performance had not saturated even with hundreds of generated humanoids (scaling-law study). A bipedal get-up across player bodies is at the hard end of this literature.

### Gaps
- No study controls continuously varied *humanoid proportions* with one policy for whole-body, multi-contact motion, apart from the small get-up study in section 4.
- The following are not stated in the text I could read: ManyQuadrupeds' simulator and step count, and whether Go1/A1 were held out; the total compute for LocoFormer and XHugWBC.
- NerveNet, AnyMorph and Body Transformer details come from abstracts or project pages only. NerveNet's venue was not confirmed (OpenReview blocked the fetch).

## 2. How do body parameters enter: explicit conditioning, implicit adaptation from history, or graph/transformer over body parts?

### Takeaway
All three approaches work. The evidence on which is best is mixed:
- **Implicit, long-context adaptation beat explicit kinematic conditioning** in LocoFormer (0.96 vs 0.78).
- **URMA did not need mass or size descriptors** but collapsed without correct joint descriptions.
- **MetaMorph failed without morphology input** when topology varied widely.

In a game the body is known exactly, so explicit parameters are free. The pattern that recurs in recent work combines explicit parameters (or a privileged critic), a short proprioceptive history, and, if topology varies, per-joint tokens. A hypernetwork can turn a body description into a small per-body network with no training.

### Cited Findings
- **Explicit, exact parameters.** — [SIGGRAPH archive](https://history.siggraph.org/?p=213182); [GaitNet](https://ar5iv.labs.arxiv.org/html/2201.12044); [URMA](https://arxiv.org/html/2409.06366); [MetaMorph](https://ar5iv.labs.arxiv.org/html/2203.11931)
  - Won & Lee 2019: body-shape parameters drive a single "parametric controller".
  - Generative GaitNet 2022: per-muscle weakness and contracture values and joint force capacity are part of the state.
  - URMA: each joint gets a description vector (rotation axis, relative position, torque and velocity limits, control range), encoded by MLPs and pooled by attention with a learnable temperature. A universal decoder pairs each joint description with an action latent.
  - MetaMorph: one token per limb carrying shape, density, position and orientation relative to the parent, and joint type, range, axis and gear.
- **Explicit, but estimated from history (RMA, 2021).** — [RMA](https://ar5iv.labs.arxiv.org/html/2107.04034)
  - A 17-D environment vector (payload mass and position 3, motor strength 12, friction 1, terrain height 1) is encoded to an 8-D extrinsics vector z.
  - At deployment, z is estimated from 50 steps (0.5 s) of states and actions by a 1-D CNN running at 10 Hz. The base policy runs at 100 Hz.
- **Implicit, morphology-blind.** — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309); [LocoFormer](https://arxiv.org/html/2509.23745v1); [arXiv 2512.12230](https://arxiv.org/html/2512.12230v1)
  - GenLoco: a 15-step history into an MLP.
  - LocoFormer: a long context across episodes.
  - The Dec 2025 get-up study: no morphology identifier at all.
- **Head-to-head evidence.** — [LocoFormer](https://arxiv.org/html/2509.23745v1); [URMA](https://arxiv.org/html/2409.06366); [MetaMorph](https://ar5iv.labs.arxiv.org/html/2203.11931)
  - LocoFormer's privileged-kinematics "Conditioning" baseline scored 0.78 against 0.96 for the morphology-blind long-context model. A GRU scored 0.37.
  - URMA: removing mass and dimensions from the descriptors changed nothing. Shuffling joint descriptions made the A1 fall within 1–2 steps and the H1 almost immediately.
  - MetaMorph: removing morphology input failed to learn diverse robots. Reversing the limb order cost about 75 %; randomizing same-depth order in training fixed it.
- **Graph structure helps, but not dramatically.** — [XHugWBC](https://arxiv.org/html/2602.05791); [Amorpheus](https://arxiv.org/abs/2010.01856)
  - XHugWBC: graph models (GCN or Transformer) beat an MLP. The Transformer masks its first layer by the joint graph and attends freely after that. The critic gets morphology as privileged information; the actor gets a 5-step history and a state estimator.
  - Amorpheus: an unstructured transformer beats graph networks.
- **Hypernetwork distillation (HyperDistill, ICML 2024).** A morphology-conditioned hypernetwork generates a separate MLP for each robot, trained by behaviour cloning from a transformer teacher. On UNIMAL it matched the teacher on training and unseen robots while being 6–14× smaller and needing 67–160× less compute. [figures from the abstract] — [PMLR](https://proceedings.mlr.press/v235/xiong24c.html); [arXiv](https://arxiv.org/pdf/2402.06570)

### Inferences
- **Exact parameters, but not by themselves.** The game knows every built body exactly, so explicit conditioning needs no estimator, unlike robotics. URMA and LocoFormer show a network can ignore or underuse descriptors, though. The robust pattern seen in RMA and XHugWBC is: body parameters (and strength) as input, a short proprioceptive history, and a privileged critic in training.
- **Hypernetwork fits "no per-body training".** HyperDistill's design matches that rule: when the player finishes building, one forward pass generates a small per-body MLP, which is cheap at runtime (section 7). It needs a strong teacher policy first. This is an inference; no hypernetwork result exists for humanoid getting up.
- **When per-joint tokens become necessary.** For a fixed skeleton, a plain MLP over [body-parameter vector + proprioception + history] matches GenLoco, Won & Lee and GaitNet. Per-joint tokens (URMA, XHugWBC) only become necessary if limb count or joint layout varies.

### Gaps
- No controlled comparison of explicit vs implicit conditioning exists for humanoid getting up or multi-contact motion.
- HyperDistill's numbers were not checked against the full paper's tables.

## 3. Character-animation side: shape-conditioned physics trackers

### Takeaway
Body shape enters motion trackers in two ways:
1. The simulated body's geometry and mass are built from SMPL β.
2. β (10-D) is given to the policy as input.

UHC and PHC both have shape-variable models in their code. Their papers, PULSE and MaskedMimic all **evaluate only the mean SMPL shape**. No quantitative results on extreme shapes (child-like, very heavy, long-limbed) were found. The training shapes are real adult subjects from AMASS. PHC's fail-state recovery gets up 95–99 % of the time and is described as "human-like", but the authors admit gait artifacts and jolts. Won & Lee 2019 is the one character paper built around body-shape variation, and its numeric ranges could not be verified.

### Cited Findings
- **Won & Lee, "Learning body shape variation in physics-based characters" (TOG 38(6), SIGGRAPH Asia 2019).** — [SIGGRAPH archive](https://history.siggraph.org/?p=213182); [project page](https://mrl.snu.ac.kr/publications/ProjectMorphCon/MorphCon.html)
  - One parametric controller for characters of different heights, weights and proportions.
  - New characters can be controlled immediately, and the shape can change during simulation.
  - Training samples body shapes adaptively, guided by a "marginal value function" of how controllable each shape is.
  - Demonstrated on bipeds, quadrupeds and underwater animals.
- **UHC paper (2021).** — [UHC paper](https://ar5iv.labs.arxiv.org/html/2106.05969)
  - Trained on the SMPL mean shape only; shape information from AMASS was discarded.
  - Each bone is the convex hull of its SMPL vertices, with mass from its volume.
  - The action adds a 6-D residual force on the root (residual force control, RFC).
  - 97.0 % success on AMASS (10,956 of 11,299 sequences), in MuJoCo.
  - Policy: 8 MCP primitives of (512, 256). Training took about 5 days (the text also cites about 1 day on an RTX 2080 Ti with 35 threads).
- **UHC repository.** It supports SMPL, SMPL-H and SMPL-X humanoids "of all genders and body shapes" and ships an "Implicit RFC + Shape variation" checkpoint. It "relies heavily" on residual forces. — [UHC repo](https://github.com/ZhengyiLuo/UniversalHumanoidControl)
- **PHC (2023).** — [PHC](https://ar5iv.labs.arxiv.org/html/2305.06456)
  - SMPL 10-D β; trained with shapes sampled from AMASS, where β encodes limb lengths. Average mass was set to about 70 kg.
  - Evaluated only on the mean shape; shape and gender variation are shown qualitatively.
  - No residual forces.
  - Isaac Gym; one A100 for about 1 week; about 10 B samples; 1,536 parallel humanoids.
  - AMASS success 98.9 % on train and 96.4 % on test.
- **PHC repository.** — [PHC repo](https://github.com/ZhengyiLuo/PHC)
  - Shape-variable models: `phc_shape_mcp_iccv` and `phc_shape_pnn_iccv` ("Shape + rotation + keypoint").
  - PHC+ can get up and walk back.
  - H1 and G1 robot support since Oct 2024 (trained per robot); an Isaac Lab evaluation script since Aug 2025.
- **PULSE (2023–24).** — [PULSE](https://arxiv.org/html/2310.04582)
  - Mean shape only; the authors say they do not consider shape variation.
  - PHC+: about 1e10 samples, 100 % train and 99.2 % test, using a 6-layer [2048, 1536, 1024, 1024, 512, 512] MLP.
  - PULSE distils this in about 1e9 samples into a 32-D latent space.
  - Recovery is "largely retained", but random generation can leave the humanoid stuck on the ground.
- **MaskedMimic (2024).** — [MaskedMimic](https://arxiv.org/html/2409.14393)
  - The text I read describes no body-shape variation.
  - Isaac Gym, 16,384 environments on 4 A100s, about 2 weeks; about 30 B steps (full-constraint controller) plus 10 B (partial).
  - 99.2 % full-body tracking.
  - Authors note "unnatural jitter" in some generated motions.
  - The portion I read describes no fallen-state recovery.
- **SimPoE (CVPR 2021).** "Meta-control" adjusts the character's dynamics parameters to its current state. Body-shape handling was not confirmed from the abstract. — [SimPoE](https://export.arxiv.org/abs/2104.00683)
- **ShapeVerse (Eurographics 2024 poster)** [snippet only]. Physics-based characters with varied SMPL β shapes under a single trained policy. The full text was blocked (HTTP 403). — [Eurographics DL](https://diglib.eg.org/handle/10.2312/egp20241042)
- **HUMOS (ECCV 2024)** [snippet only]. A *kinematic* generative motion model conditioned on body shape, with physics and stability terms. It is not a physics controller. — [HUMOS](https://humos.is.tue.mpg.de)

### Inferences
- **Shape range is human-only.** Shape-conditioned trackers learn the shape range of real adult mocap subjects. Bodies far outside it (dwarf-like, ogre-like, very long-armed) would be out of distribution. Their style is anchored to the mocap performer's body, which may work against the director's wish that proportions visibly shape how each body gets up.
- **UHC's residual force breaks the project's rule.** It is an outside force at runtime, so UHC's approach is incompatible with "no outside forces in the game". PHC, PULSE and MaskedMimic use joint PD only.
- **Mocap as a training prior is an open design question.** Using human get-up mocap only as a training-time style prior (an AMP-style discriminator, as PHC's recovery primitive does) would still leave runtime motion produced purely by joint torques. Whether that counts as an "animation clip" is a call for the Game Director, not something this research settles.

### Gaps
- No per-shape success or naturalness numbers exist for PHC's or UHC's shape models. It is unknown whether PHC's shape models include fail-state recovery.
- Won & Lee 2019: the numeric body ranges, motions and whether getting up was included could not be read. The PDF was not parseable here and the project page reset the connection.
- SimPoE's handling of body shape was not verified.

## 4. Getting up and fall recovery across bodies (direct evidence)

### Takeaway
The only direct result found is a Dec 2025 single-author study. One morphology-blind policy was trained over 7 small humanoids (0.48–0.81 m, 2.8–7.8 kg):
- **In distribution:** often above 80 % success.
- **Held out:** 17–42 % for 6 of the 7 robots. Top-heavy and long-armed robots did worst.
- **Coverage:** continuous, overlapping coverage of size, mass and limb ratio mattered most.

Single-body get-up work supplies the remaining recipe pieces:
- **HoST (2025):** 99.5 % in simulation and 20/20 on the real G1, made smooth by L2C2 regularization and an action-scaling curriculum, with a vertical pull force used only during training.
- **Tao et al. (2022):** natural-looking get-ups through a strong-to-weak strength curriculum.

None of these combine cross-body generalization with naturalness.

### Cited Findings
- **"Learning to Get Up Across Morphologies" (Spraggett, U. Toronto, arXiv Dec 2025).** — [arXiv 2512.12230](https://arxiv.org/html/2512.12230v1)
  - **Robots.** 7 humanoids from 0.48 m / 2.82 kg (Bez1) to 0.81 m / 6.68 kg (NUGUS), with 18–20 DoF.
  - **Training.** CrossQ (SAC family), MLP [512, 512, 256], about 600k steps per policy in about 1.5 h with 16 parallel environments in MuJoCo, on a Ryzen 3900X with an RTX 3090.
  - **Actions and inputs.** The policy commands changes to shoulder, elbow, hip, knee and ankle pitch only, applied symmetrically. Inputs are joint states, trunk orientation, angular velocity and head height, with no morphology input.
  - **Start states.** Face-up, face-down and side, with torso and joints displaced up to ±90°.
  - **Domain randomization.** Mass and CoM ±10 %, friction ±15 %, motor gains ±10 %.
- **Results of that study.** — [arXiv 2512.12230](https://arxiv.org/html/2512.12230v1)
  - **Held out (leave-one-out):** Wolfgang 72 ± 21 %, the other six 17–42 %. The abstract's "86 ± 7 %" is Wolfgang trained alongside four morphologies, not a leave-one-out result.
  - **In distribution:** 21 of 42 cross entries exceed 80 %.
  - **Shared vs specialist:** NUGUS reaches 81 ± 6 % with shared training, though a specialist alone could not learn to get up at all (+61 %). Wolfgang lost 20 % and Bez2 lost 18 % under shared training.
  - **Failure pattern:** top-heavy and long-armed robots fail most. A set of only extreme robots reached at most 16 %. Gains plateau at 4–5 training morphologies. Adding robots sometimes hurt others, and excluding the tallest helped the small ones.
  - **Not reported:** naturalness, smoothness or jitter assessment, and real-robot results.
- **HoST (RSS 2025; Unitree G1, a single robot).** — [HoST](https://arxiv.org/html/2502.08378v2)
  - **Setup.** Isaac Gym with 4,096 environments, PPO, an actor of [512, 256, 128] and multiple critics; policy at 50 Hz, PD at 200 Hz.
  - **Domain randomization.** Trunk mass −2 to +5 kg, link mass 0.8–1.2×, motor strength 0.9–1.1×, P/D gains 0.85–1.15×, delay 0–100 ms.
  - **Smoothing machinery:**
    - An action rescaler β falls from 1 by 0.02 each time the robot reaches the target height, with a floor of 0.25. This implicitly limits torque and speed.
    - L2C2 smoothness regularization.
    - Penalties on joint acceleration, action rate, the action's second difference, torque, power and velocity.
  - **Training-only assist.** A vertical pull on the base, applied only when the trunk is near vertical, starts at 200 N and drops by 20 N per success down to 0 N.
  - **Simulation success.** Ground 99.5 %, platform 99.8 %, wall 94.2 %, slope 98.5 %.
  - **Real robot.** 20/20 successes. Without L2C2 it managed 11/20, with oscillations; the smoothness metric was 4.01 vs 6.54.
  - **Robustness.** Stood with payloads up to 12 kg (2/3 at 12 kg). Torque dropout was handled at 5–15 % (3/3 each) and failed at 20 % (0/3).
  - **Limitations.** Training prone and supine starts together caused interference.
  - **Other robots.** The method was re-applied to H1 and H1-2 "with minor hyperparameter tuning", which implies separate training rather than one shared policy.
- **PHC fail-state recovery (2023).** — [PHC](https://ar5iv.labs.arxiv.org/html/2305.06456)
  - **Training.** A dedicated primitive trained on a hand-picked walking/running AMASS subset. Reward: 0.5 × point-goal + 0.5 × AMP style + 0.1 × energy. Fallen starts are created by dropping the humanoid at episode start.
  - **Test.** A drop plus 150 steps of random torques, 1,000 trials.

    | Recovery from fallen state | Within 5 s | Within 10 s |
    |---|---|---|
    | Rotation-based / keypoint-based | 95.0 % / 92.5 % | 98.8 % / 94.6 % |

  - **Quality.** Described as human-like, but with "asymmetric walking gait", speed artifacts, and a sudden jolt when switching back to imitation.
- **Tao, Wilson, Gou, van de Panne, "Learning to Get Up" (SIGGRAPH 2022; one character).** — [Tao et al.](https://ar5iv.labs.arxiv.org/html/2205.00307)
  - **Character.** About 1.5 m tall (the paper's text has a unit typo), 38.3 kg, 21 DoF. MuJoCo physics at 800 Hz, policy at 40 Hz.
  - **Start states.** Rag-doll falls from 1.5 m followed by random low-amplitude actions, so any fallen pose.
  - **Three stages:**
    1. Discover a get-up with a strong character.
    2. Lower the torque limits step by step (strong to weak).
    3. Imitate retimed trajectories up to 5× slower.
  - **Constrained bodies.** Variants with a leg and an arm in casts (left elbow and right knee locked) and with a missing arm also got up. No success rates are reported.
  - **Cost.** About 24 h per policy on a 32-core CPU with an RTX 2070; SAC with 2 × 1024 networks.
- **MuJoCo Playground "Go1 Getup" (2025; one quadruped).** — [MuJoCo Playground](https://arxiv.org/html/2502.08844)
  - Trained with a 400 W power termination cutoff, then fine-tuned with a joint-velocity cost.
  - Runs at 96,173 PPO steps/s on one A100.
  - Fall recovery transferred to the real Go1 "robustly".
- **LocoFormer (2025).** It "learns from falls" across trials (a biped fails trial 1 and walks by trial 3 on hardware), but it does not stand up from the ground. — [LocoFormer](https://arxiv.org/html/2509.23745v1)

### Inferences
- **Why the current miner is hard.** The cross-morphology get-up study's failure pattern (top-heavy and long-armed bodies worst; extreme-only sets fail) matches Wonder Gather's current body, a small head-heavy miner, which is exactly the hard case. Its tiny symmetric 5-joint action space is much simpler than a full player-built body, so its success rates are, if anything, optimistic for this project.
- **How the current numbers compare.** HoST reaches 99.5 % on one robot with randomized mass and strength, against roughly 1/3 for the current per-body PPO. Most of the gap plausibly comes from:
  - sample count (4,096 parallel environments);
  - the training-only assist curriculum;
  - explicit smoothness machinery (L2C2, the β rescaler, rate and acceleration penalties).

  The last two map directly onto the current "aggressive limb twitches" and onto the project rule that help is allowed only while training.
- **Continuous coverage.** "Continuous, overlapping coverage of size, mass and limb ratio" (Dec 2025 study) and Won & Lee's adaptive shape sampling suggest that, if attempted, a cross-body get-up controller should sample the body space densely and adaptively rather than train on a few hand-made extremes.

### Gaps
- No work found trains one get-up controller over continuously varied *adult-scale* humanoid proportions **and** strengths, or evaluates whether the motion looks natural or body-specific.
- The Dec 2025 study is a single-author arXiv preprint with no hardware results yet. Treat it as preliminary.
- HumanUP (arXiv 2502.12152, real-humanoid getting up) appeared in results but was not read here. It is single-body and likely covered by the other research threads.

## 5. Strength generalization: torque limits, weakness, fatigue, injury, and whether strength visibly changes style

### Takeaway
Robotics randomizes strength **narrowly** (±10–20 %, torque limits 0.7–1.0) so the policy behaves the same regardless of strength: robustness, not expression. Where strength or weakness is a **wide, known input** and the limits actually bind, the motion style visibly changes:
- **Generative GaitNet (2022):** per-muscle weakness produces recognizable pathological gaits.
- **Tao et al. (2022):** strong bodies produce "excessively dynamic" get-ups, while bodies at 40–60 % of default strength look subjectively more natural.
- **Fatigue study (2023):** fatigued agents find recovery strategies.

The Tao curriculum runs **strong to weak**, not weak to strong as the task brief assumed. Very weak bodies trained from scratch often never found a get-up at all.

### Cited Findings
- **RMA (2021).** — [RMA](https://ar5iv.labs.arxiv.org/html/2107.04034)
  - Per-motor strength (12 values) is part of the privileged environment vector. Training range [0.90, 1.10], test range [0.88, 1.22].
  - Payload 0–6 kg in training. The real A1 (about 12 kg) carried up to 12 kg, where the policy without adaptation fails to move above 8 kg.
  - Simulation success: RMA 73.5 %, expert 76.2 %, domain-randomized robust policy 62.4 %, no adaptation 52.1 %.
  - About 1.2 B steps in about 24 h on a one-GPU desktop (RaiSim), plus 80 M steps in 3 h for the adaptation module.
- **Mass-scaled strength in randomization.** — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309); [XHugWBC](https://arxiv.org/html/2602.05791)
  - GenLoco: motor strength 0.8–1.2×, PD gains scaled by the ratio of robot mass to nominal mass.
  - XHugWBC: torque limits scaled [0.7, 1.0], with PD gains and torque limits scaled linearly with total mass "as a heuristic".
- **HoST.** Motor strength 0.9–1.1× and ±5 % torque noise. Torque dropout up to 15 % was handled; 20 % failed. — [HoST](https://arxiv.org/html/2502.08378v2)
- **Tao et al. 2022: the strength curriculum.** — [Tao et al.](https://ar5iv.labs.arxiv.org/html/2205.00307)
  - **Direction is strong to weak.** Torque limits start at default human-based values (hips 120 Nm, knees 80 Nm, ankles and shoulders 20 Nm) and are multiplied by 0.95 at each curriculum step once test reward exceeds 60.
  - **Where it ends.** Final limits average below 60 % of default. Strategies ending at 40–60 % were "subjectively more natural" than those above 70 %.
  - **Weak from scratch fails.** Fixed limits of 40–60 % from the start often prevented finding any solution.
  - **Strong without the curriculum** produced "excessively dynamic" motion.
  - **Penalties alone were not enough.** Energy or joint-velocity penalties without the curriculum had minimal or unstable effect.
  - **Style was not mapped.** The paper does not systematically show style across final strength levels.
- **Generative GaitNet (SIGGRAPH 2022).** — [GaitNet](https://ar5iv.labs.arxiv.org/html/2201.12044); [project page](https://mrl.snu.ac.kr/research/ProjectGaitNet/GaitNet.html)
  - **Model.** 23 bones and 304 Hill-type muscles.
  - **Conditions given to one network.**
    - Per-muscle weakness factors in [0, 1] and contracture factors.
    - 10 body-scale factors: head and body size about ±15 %, limb length about ±10 %.
    - Stride and cadence ±25 %.
  - **Gaits produced, all from one network:**
    - foot drop from dorsiflexor weakness;
    - Trendelenburg gait (pelvic drop with trunk lean) from one-sided gluteal weakness, and waddling when both sides are weak;
    - crouch gait and toe walking from contractures;
    - a longer stance phase on the longer leg when leg lengths differ.
  - **Training.** About 340 M rollouts over 4–5 days on 4 Xeon 6242 CPUs plus an RTX 3090, using DART physics at 480 Hz. Gait simulation runs in real time after training.
  - **Evaluation and scope.** Qualitative only, walking only.
- **Fatigue (Cheema et al., SIGGRAPH Asia 2023).** — [arXiv 2310.08583](https://arxiv.org/abs/2310.08583)
  - A Three-Compartment Controller fatigue model sets time- and state-dependent joint-torque limits.
  - Agents "adapt to different fatigue and rest rates interactively" and "discover realistic recovery strategies" without fatigued mocap.
  - Whether a single policy takes the fatigue parameters as input was not clear from the abstract.
- **MyoSuite (2022).** Muscle fatigue, sarcopenia (muscle weakness) and tendon transfer are built in as non-stationary physiological conditions. — [MyoSuite](https://proceedings.mlr.press/v168/caggiano22a.html)
- **Injury-like changes in locomotion.** — [LocoFormer](https://arxiv.org/html/2509.23745v1); [arXiv 2505.05753](https://arxiv.org/html/2505.05753)
  - LocoFormer: a locked knee led to a three-legged gait in 2–3 s.
  - Scaling-law study: a 20 % knee-limit cut led to a stable limp.
- **Power caps to calm motion.** Playground's handstand and footstand were fine-tuned with joint-acceleration and energy costs while the power budget was cut from 400 W to 200 W. — [MuJoCo Playground](https://arxiv.org/html/2502.08844)

### Inferences
- **Invariance vs expression.** Robotics-style ±10 % strength randomization teaches *invariance*: one strategy that works for all. For strength to *visibly shape* the get-up, it plausibly needs three things:
  - it varies widely in training;
  - it is a known input (the game has it exactly);
  - the torque caps actually bind, so a weak body physically cannot use the strong body's strategy.

  GaitNet and Tao are the only evidence that this produces readable style differences, and both are qualitative.
- **Strength-to-weight ratio.** The game's rule (strength ∝ limb thickness × one strength number) parallels GenLoco's and XHugWBC's mass-scaled gains and torque limits. Those works scale strength with mass to keep the strength-to-weight ratio in a trainable band. The ratio, more than absolute strength, probably decides which get-up strategies are feasible.
- **Train weak bodies from strong.** Weak player bodies will likely need training that starts strong and anneals strength down: Tao's curriculum, HoST's β rescaler, Playground's power budget. In a cross-body controller this becomes a curriculum along the strength axis. It also tends to remove the aggressive, abrupt motion, the current twitch problem.

### Gaps
- No study found shows one policy, conditioned on continuous strength, producing visibly different and natural *get-up* strategies across strengths.
- "Learning with muscle weakness" as a separate named work was not identified. GaitNet is the closest strong match.

## 6. Training cost, simulators, and sim-to-sim transfer into Unity PhysX

### Takeaway
The morphology-generalizing results above used 1e8–1e10 samples, and one used 2e12. They ran on GPU simulators or many-core CPU MuJoCo:
- **Current gap.** Your stated ~1 M steps per night is about 30 steps/s (my arithmetic). That is about 3–4 orders of magnitude below those budgets.
- **GPU training is fast.** Humanoid locomotion trains in under 30 minutes on 2× RTX 4090 in MuJoCo Playground.
- **Windows-laptop GPU options are narrow:**
  - Isaac Sim's current minimum is an RTX 4080 with 16 GB.
  - JAX (MJX) GPU support on Windows exists only through WSL2, and only experimentally.
  - MuJoCo Warp runs natively on Windows but needs NVIDIA CUDA 12.4+.
- **Sim-to-sim into Unity PhysX is undocumented.** No quantitative study was found. Known risks are solver iterations, the actuator model and contact.

### Cited Findings
**Training-cost table (as reported; "n/s" = not stated in the text read)**

| Work | Simulator | Samples | Hardware | Wall-clock |
|---|---|---|---|---|
| GenLoco 2022 | PyBullet | 800 M | 16 CPU workers | about 2 weeks |
| RMA 2021 | RaiSim | about 1.2 B + 80 M | one-GPU desktop | about 24 h + 3 h |
| MetaMorph 2022 | MuJoCo | 1e8 | RTX 2080 | about 1.5 GPU-days |
| URMA 2024 | CPU MuJoCo + JAX | 1.6 B | 20 cores + RTX 3090/A5000 | about 2.5 days |
| Embodiment scaling laws 2025 | Isaac Lab | more than 2e12 (experts) + 2e9 (distillation) | 160 × RTX 4090/3090, then 1 × H100 | about 5 days + about 1 week |
| Tao 2022 | MuJoCo | n/s (stages capped at 3e5–8e5 steps) | 32-core CPU + RTX 2070 | about 24 h per policy |
| Generative GaitNet 2022 | DART | about 340 M | 4 × Xeon 6242 + RTX 3090 | 4–5 days |
| UHC 2021 | MuJoCo | n/s | RTX 2080 Ti + 35 threads | about 1–5 days (text inconsistent) |
| PHC 2023 | Isaac Gym | about 10 B | 1 × A100 | about 1 week |
| MaskedMimic 2024 | Isaac Gym | about 30 B + 10 B | 4 × A100 | about 2 weeks |
| Get-up across morphologies 2025 | MuJoCo | about 600k per policy (CrossQ) | Ryzen 3900X + RTX 3090 | about 1.5 h |

Sources: [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309); [RMA](https://ar5iv.labs.arxiv.org/html/2107.04034); [MetaMorph](https://ar5iv.labs.arxiv.org/html/2203.11931); [URMA](https://arxiv.org/html/2409.06366); [arXiv 2505.05753](https://arxiv.org/html/2505.05753); [Tao et al.](https://ar5iv.labs.arxiv.org/html/2205.00307); [GaitNet](https://ar5iv.labs.arxiv.org/html/2201.12044); [UHC](https://ar5iv.labs.arxiv.org/html/2106.05969); [PHC](https://ar5iv.labs.arxiv.org/html/2305.06456); [MaskedMimic](https://arxiv.org/html/2409.14393); [arXiv 2512.12230](https://arxiv.org/html/2512.12230v1)

**GPU simulators and platform limits**
- **MuJoCo Playground / MJX (2025).** — [MuJoCo Playground](https://arxiv.org/html/2502.08844)
  - Wall-clock on 2 × RTX 4090: Go1 joystick under 5 min, Berkeley Humanoid under 15 min, G1 and T1 under 30 min each. The joystick schedule was 100 M + 50 M + 100 M steps.
  - Throughput on one A100 (PPO steps/s):

    | Task | Steps/s |
    |---|---|
    | Go1 joystick, flat | 417,451 |
    | Go1 getup | 96,173 |
    | G1 joystick | 106,093 |
    | Berkeley Humanoid, rough | 30,393 |

  - MJX contact cost scales with possible rather than active contacts, and JIT compilation takes 1–3 min.
- **JAX on Windows.** NVIDIA GPU on native Windows is "no"; through WSL2 it is "experimental". CUDA pip installs "do not work with Windows". AMD GPU is experimental through WSL2 only. — [JAX installation](https://docs.jax.dev/en/latest/installation.html)
- **MuJoCo Warp.** Listed for Windows or Linux on x86-64 with CUDA 12.4 or later. It still lacks some MuJoCo features, such as the PGS and noslip solvers and plugins. [snippet-level] — [mujoco-warp on PyPI](https://pypi.org/project/mujoco-warp/3.5.0/); [GitHub](https://github.com/google-deepmind/mujoco_warp)
- **Isaac Lab Newton backend (built on MuJoCo Warp).** Part of the Isaac Lab 3.0 Beta develop branch, with a limited set of examples (classic control and flat-terrain locomotion) and breaking changes expected. Newton was validated against PhysX by transferring policies, and a Newton-trained policy was deployed on a G1. [snippet-level] — [Isaac Lab docs](https://isaac-sim.github.io/IsaacLab/main/source/experimental-features/newton-physics-integration/index.html)
- **Isaac Sim requirements (docs updated 14 Oct 2026).** — [Isaac Sim requirements](https://docs.isaacsim.omniverse.nvidia.com/latest/installation/requirements.html)
  - Windows 11 or Ubuntu 22.04/24.04; Windows 10 is not supported.
  - Minimum GPU GeForce RTX 4080 with 16 GB VRAM; 32 GB RAM; only NVIDIA drivers are listed.
- **Genesis.**
  - The developers' 43 M FPS figure is self-reported, from a scene with one plane and a Franka arm on one RTX 4090. They also claim 10–80× the speed of Isaac Gym and MJX. — [DataCamp summary of developer claims](https://www.datacamp.com/blog/genesis-physics-engine)
  - A ManiSkill author (a conflict of interest) built a pick-cube benchmark and found Genesis 3–10× *slower* than ManiSkill for physics-only simulation. He called the original benchmark misleading. — [Stone Tao](https://stoneztao.substack.com/p/the-new-hyped-genesis-simulator-is)
- **Off-policy alternatives for slow simulators.**
  - CrossQ needed only about 600k steps per get-up policy in the Dec 2025 study. — [arXiv 2512.12230](https://arxiv.org/html/2512.12230v1)
  - FlashSAC (2026) claims to beat PPO across 60+ tasks in 10 simulators and to cut humanoid sim-to-real training "from hours to minutes". [abstract only] — [arXiv 2604.04539](https://arxiv.org/abs/2604.04539)

**Sim-to-sim transfer**
- **Cross-engine validation is standard practice.** — [Humanoid-Gym](https://arxiv.org/abs/2404.05695); [MuJoCo Playground](https://arxiv.org/html/2502.08844)
  - Humanoid-Gym (2024) trains in Isaac Gym, validates policies in MuJoCo, and transfers zero-shot to the real XBot-S and XBot-L.
  - MuJoCo Playground uses ros2-control interfaces shared with Gazebo for sim-to-sim checks.
- **Unity ArticulationBody.** Uses a Featherstone solver in reduced coordinates, so joints cannot stretch. That is the same family as MuJoCo and Isaac articulations, unlike maximal-coordinate joints. — [Unity ArticulationBody docs](https://docs-alpha.unity3d.com/ScriptReference/ArticulationBody.html)
- **MuJoCo Unity plug-in.** — [MuJoCo docs, Unity plug-in](https://mujoco.readthedocs.io/en/stable/unity.html)
  - Runs MuJoCo physics inside the Unity Editor and runtime, with MJCF import.
  - Limitations: no trigger colliders; changes made at runtime apply only when the scene is recreated; the page does not confirm standalone-build support.
- **Modular Agents (Unity).** Its MuJoCo humanoid environments work. The PhysX version was "still being translated", hampered by limited access to the PhysX runtime, and a PhysX-to-MuJoCo converter is work in progress. [snippet-level] — [modular-agents](https://github.com/Balint-H/modular-agents)
- **Solver iterations (anecdote, possibly dated).** Unity's solver ran 4–6 iterations against about 20 in MuJoCo. Matching the iteration counts reproduced the instabilities. — [Azimuth substack](https://azimuth.substack.com/p/machine-learning-environments-in)
- **Actuator details matter.** XHugWBC's real-robot velocity gap varied from 27 % to 108 % by platform, attributed partly to gear ratios and joint clearances. — [XHugWBC](https://arxiv.org/html/2602.05791)

### Inferences
- **Scale of the gap (my arithmetic).** 1 M steps over an 8–10 h night is about 28–35 steps/s across 12 Unity copies. At that rate, 1 B steps (URMA-scale) takes about a year. One A100 at about 100k steps/s (Playground's G1 joystick) would take about 3 h. Even URMA's CPU MuJoCo (1.6 B in 2.5 days, about 7,400 steps/s on 20 cores) is about 200× the current Unity rate. The models and step definitions differ, so treat this as an order of magnitude.
- **Cheapest first experiment.** The current Unity pipeline probably has large headroom (headless runs, more agents per instance, decision interval, physics substeps). That is worth measuring before any engine change. This is an inference; I measured nothing.
- **A one-policy-for-all-bodies controller needs more samples than per-body training.** URMA used 100 M per robot, and the scaling-law study shows diversity matters most. That points to (a) a GPU simulator, (b) a much faster Unity pipeline, or (c) sample-efficient off-policy methods (CrossQ, FlashSAC), possibly combined.
- **Laptop hardware (inference).**
  - If the GPU is NVIDIA, MuJoCo Warp (native Windows) or MJX through WSL2 (experimental) are the realistic GPU paths. Isaac Sim's stated minimum (RTX 4080, 16 GB) is above most laptop GPUs.
  - If it is not NVIDIA, only CPU simulation (MuJoCo, Unity) or rented cloud GPUs remain. Cloud cost was not researched.
- **Sim-to-sim is the largest unresearched risk.** Training in MuJoCo or Isaac and running in Unity PhysX with ConfigurableJoint drives is a cross-engine transfer with no published numbers. The likely mismatches:
  - how the PD drive is implemented (Unity's drive with a force cap versus explicit PD torque);
  - solver iterations and timestep;
  - contact and friction models.

  Mitigations seen in the literature: randomize PD gains, torque caps, delays, friction and mass (as URMA and HoST do); output joint targets through PD rather than raw torques; validate in the target engine before trusting a policy. Two further options each mean a major engine-level change, and both are director decisions:
  - Unity ArticulationBody, which is closer to MuJoCo and Isaac;
  - the MuJoCo Unity plug-in, which would remove the gap but replace PhysX for the characters.

### Gaps
- No throughput figures were found for Unity ML-Agents with articulated ragdolls. The only Unity figure is the one in the task brief.
- No quantitative sim-to-sim study exists between Unity PhysX (ConfigurableJoint or ArticulationBody) and MuJoCo or Isaac.
- The PhysX SDK version inside Unity 6 was not verified.
- Cloud GPU pricing was not researched.
- Performance on consumer laptop GPUs for MJX, MuJoCo Warp or Isaac Lab was not found; the published numbers are for A100 and desktop RTX 4090.

## 7. Runtime: network sizes and CPU inference cost (Unity Inference Engine / ML-Agents)

### Takeaway
Deployed locomotion and get-up policies are small MLPs:
- **Typical size:** 0.1–1 M parameters, run at 30–100 Hz on embedded robot computers.
- **Unity guidance:** run models of this kind on the CPU with Burst.
- **Larger models cost more:** mocap-scale trackers (PHC+: 6 layers up to 2,048 wide) and long-context transformers (LocoFormer's 255-step KV cache) are one to two orders of magnitude more expensive per decision.
- **Hypernetwork distillation** can cut compute 67–160× while keeping the performance of a transformer teacher.

No published per-agent timings for the Unity Inference Engine were found.

### Cited Findings
- **ML-Agents 4.0.3 inference docs.** — [ML-Agents docs](https://docs.unity3d.com/Packages/com.unity.ml-agents%404.0/manual/Inference-Engine.html)
  - Inference runs on Sentis (package `com.unity.ai.inference`), through compute shaders or the CPU via Burst.
  - "For most of the models generated with the ML-Agents Toolkit, CPU inference (Burst) will be faster than GPU inference." GPU is for ResNet visual encoders or many agents with visual observations.
  - IL2CPP is generally faster than Mono.
  - Only models from Unity's own trainers are supported. Externally trained models should use Sentis directly (ONNX).
- **Arm blog (mobile).** ML-Agents ran faster on CPU than GPU, because the model is too small to use the GPU well and CPU-to-GPU transfer dominates. — [Arm community blog](https://community.arm.com/arm-community-blogs/b/ai-blog/posts/p3-multi-agent-reinforcement-learning)
- **Deployed policy sizes:**
  - RMA: a 3-layer MLP with 128 units at 100 Hz plus a 1-D CNN at 10 Hz, on the robot's onboard compute. — [RMA](https://ar5iv.labs.arxiv.org/html/2107.04034)
  - HoST: actor [512, 256, 128] at 50 Hz on a Jetson Orin NX. — [HoST](https://arxiv.org/html/2502.08378v2)
  - GenLoco: [1024, 512] at 30 Hz. — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309)
  - URMA: about 430k parameters at 50 Hz. — [URMA](https://arxiv.org/html/2409.06366)
  - Get-up across morphologies: [512, 512, 256] with a 50 ms control step. — [arXiv 2512.12230](https://arxiv.org/html/2512.12230v1)
  - Tao: 2 × 1024 at 40 Hz. — [Tao et al.](https://ar5iv.labs.arxiv.org/html/2205.00307)
  - GaitNet: policy 3 × 512 plus muscle networks 3 × 256, real time after training. — [GaitNet](https://ar5iv.labs.arxiv.org/html/2201.12044)
- **Larger models:**
  - PHC: [1024, 512] MLP primitives; the final 4-primitive model is 28.8 MB; about 50 FPS with simulation and policy and no rendering. — [PHC](https://ar5iv.labs.arxiv.org/html/2305.06456)
  - PHC+: [2048, 1536, 1024, 1024, 512, 512]. — [PULSE](https://arxiv.org/html/2310.04582)
  - MetaMorph: transformer with about 3.3 M parameters (baselines matched). — [MetaMorph](https://ar5iv.labs.arxiv.org/html/2203.11931)
  - LocoFormer: 6-layer Transformer-XL with a 255-step KV cache at deployment. — [LocoFormer](https://arxiv.org/html/2509.23745v1)
- **HyperDistill.** 6–14× smaller and 67–160× less compute than a universal transformer teacher, at equal performance on UNIMAL. [abstract-level] — [PMLR](https://proceedings.mlr.press/v235/xiong24c.html)

### Inferences
- **Cost per decision (my estimates, not measured; dense layers only, about 2 FLOPs per weight):**

  | Policy shape | Approx. parameters | Approx. cost per decision |
  |---|---|---|
  | HoST-size, [512, 256, 128] with a few hundred inputs | about 0.2–0.4 M | about 0.5–1 MFLOP |
  | GenLoco-size, [1024, 512] with about 400 inputs | about 1 M | about 2 MFLOP |
  | PHC+-size, with about 1,000 inputs (assumed) | about 8–9 M | about 17 MFLOP |

  For 100 units at 30 Hz, that is about 1.5–3 GFLOP/s for HoST-size and about 50 GFLOP/s for PHC+-size. The first is trivial on one CPU core with Burst. The second is a real share of an RTS frame budget.
- **Choose small students.** For RTS-scale unit counts, the controller should be a small student: a distilled MLP or a HyperDistill-style generated per-body MLP. Large trackers and long-context transformers do not fit that budget.
- **Physics may dominate.** With small policies, per-unit physics (many ConfigurableJoints at a high substep rate) likely costs more than inference. That was not measured.
- **Rebuilding on Sentis directly.** A policy trained outside ML-Agents (MJX, Isaac Lab, MuJoCo Warp) has to run through Sentis / Inference Engine with an ONNX export, because ML-Agents' own inference only accepts models from Unity's trainers. Observation building, normalization and action scaling then have to be reimplemented exactly in C#.

### Gaps
- No published per-agent latency or throughput was found for Unity Inference Engine / Sentis with small MLPs on desktop CPUs, nor any scaling data for hundreds of agents.
- The Sentis → Inference Engine renaming and the matching ML-Agents version were not verified beyond the package name shown in the ML-Agents 4.0.3 docs.

## 8. Honest limits: out-of-distribution degradation and whether any result shows natural-looking motion

### Takeaway
Performance degrades as bodies move away from the training distribution:
- **Sharply beyond about 2× size** (GenLoco).
- **About 15 % below specialists** inside a well-covered humanoid family (XHugWBC).
- **To 17–42 % success** for most held-out bodies in the only cross-body get-up study.

Humanoid, top-heavy and long-armed bodies are worst, and adding bodies can hurt others (negative transfer). Naturalness is almost never measured in the cross-body literature; success, velocity tracking and smoothness metrics are what get reported. The natural-looking results come from mocap-driven trackers (PHC, MaskedMimic, PULSE: mean shape only, still with artifacts) or from single-body curricula (Tao, HoST). **No published result shows one controller producing natural, body-specific get-ups across a continuous space of player-designed proportions and strengths.**

### Cited Findings
- **Robotics multi-embodiment limits:**
  - URMA: zero-shot to embodiments "far outside the training distribution remains problematic"; no humanoid zero-shot; no humanoid real-world test. — [URMA](https://arxiv.org/html/2409.06366)
  - Scaling-law study: flat terrain only; mass distribution, damping and actuation held fixed; transfer to H1 worse than to Go2. — [arXiv 2505.05753](https://arxiv.org/html/2505.05753)
  - GenLoco: sharp drop for larger robots; fixed DoF. — [GenLoco](https://ar5iv.labs.arxiv.org/html/2209.05309)
  - MetaMorph: zero-shot drop on new morphologies, largest when joint ranges change; about 75 % drop from limb-order reversal unless order is randomized in training. — [MetaMorph](https://ar5iv.labs.arxiv.org/html/2203.11931)
  - XHugWBC: about 85 % of specialist return; needs "embodiment-specific parameter ranges" for physically consistent randomization; sim-to-real gaps vary widely by platform. — [XHugWBC](https://arxiv.org/html/2602.05791)
  - LocoFormer: highly resource-intensive; hand-crafted procedural task space; adaptation can involve failed early trials. — [LocoFormer](https://arxiv.org/html/2509.23745v1)
- **Getting up across bodies.** Held-out success is weak for most robots, especially top-heavy or long-armed ones. Extreme-only training sets reached at most 16 %. Adding robots sometimes reduced others' success. No naturalness assessment. — [arXiv 2512.12230](https://arxiv.org/html/2512.12230v1)
- **What has been said or measured about naturalness:**
  - PHC recovery: "human-like", but with an asymmetric gait, speed artifacts and a jolt at the switch back to imitation; no metric. — [PHC](https://ar5iv.labs.arxiv.org/html/2305.06456)
  - MaskedMimic: "unnatural jitter" in some generated motions. — [MaskedMimic](https://arxiv.org/html/2409.14393)
  - PULSE: random generation can get stuck lying down; naturalness only in videos. — [PULSE](https://arxiv.org/html/2310.04582)
  - HoST: quantifies smoothness. On the real robot, L2C2 made the difference between 20/20 and 11/20 successes (smoothness 4.01 vs 6.54). Prone and supine starts interfere when trained together. — [HoST](https://arxiv.org/html/2502.08378v2)
  - Tao: naturalness is "subjective", gained from the strength curriculum and slow imitation; paused poses become less stable near the end of the get-up. — [Tao et al.](https://ar5iv.labs.arxiv.org/html/2205.00307)
  - GaitNet: comparisons to human data and videos only; walking only. — [GaitNet](https://ar5iv.labs.arxiv.org/html/2201.12044)

### Inferences
- **What a single cross-body get-up controller would need, assembled from the above (inference, not a plan):**
  - a body and strength space that is densely and adaptively sampled (Won & Lee; Dec 2025 study);
  - explicit body and strength inputs plus a short history, with a privileged critic (RMA, XHugWBC);
  - a strong-to-weak strength/power curriculum (Tao, HoST, Playground);
  - explicit smoothness regularization (L2C2, rate and acceleration penalties, low-pass filtered targets as in GenLoco);
  - possibly a training-only assist force removed by a curriculum (HoST);
  - training on the order of 1e8–1e10 steps, which effectively means a GPU simulator or a far faster Unity pipeline;
  - distillation into a small per-unit student for runtime (URMA, HyperDistill);
  - validation inside Unity PhysX.
- **What to expect.** Even with all of that, the literature predicts lower success at the edges of the build space and possible negative transfer. The look of the motion ("natural, grounded, shaped by proportions and strength") would have to be judged by the Game Director, because no paper has a metric for it.
- **Design questions for the director, not resolved here:**
  - whether the build space should be bounded to the trained envelope;
  - whether a body visibly "learning its new shape" over its first tries (LocoFormer-style in-context adaptation) is acceptable;
  - whether human get-up mocap may be used as a training-only style prior.

### Gaps
- No user study or perceptual metric of naturalness was found for any morphology-generalizing controller.
- No quantitative out-of-distribution curve exists for humanoid *proportion* scaling beyond XHugWBC's ranges (mostly up to 1.5×) and the 7-robot get-up study.
- Behaviour on extreme shapes (child-like, very heavy, long-limbed) is unreported for every shape-conditioned character tracker found.
