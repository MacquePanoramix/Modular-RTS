# The body map: how a miner's body is defined and built today

Read-only mapping of the work branch worktree `D:/Dev/WG` (branch `claude/wip-round`, HEAD `8846684`). Paths are relative
to `D:/Dev/WG/` unless written in full; `Units/` means `Assets/_WonderGather/Scripts/Units/`. Nothing was modified and no
Unity run was made. Numbers marked *derived* were computed from `Assets/_WonderGather/Art/Worker/Miners/miners.json` with
the code's own formulas (scripts: `scratchpad/anybody/work/derive.py`, `centres.py`, `example.py`). The strength numbers
agree with the tables measured in the game (`Docs/Design/TheBodysOwn.md` in the main project, lines 93-99 and 1637-1641:
knee 87/202/220, hip 130/303/331, back 257/347/462, ankle 61/141/154, shoulder 42/76/85, elbow 36/65/74 N m).

---

## 0. The short version

1. **A physical body is defined by three inputs plus code constants.** The model mesh is not one of them.
   - `ProceduralBiped.Proportions`, 23 numbers saved in the prefab (`Units/ProceduralBiped.cs:40-72`), give every segment
     length both physical bodies use.
   - The boot and toe boxes are the `localScale` of the prefab's foot and toe segments: fixed at 0.10 x 0.06 m and
     0.10 x 0.05 m, with only their lengths per miner (`Editor/MinerSetup.cs:365-366`).
   - `PhysicalBody` holds 19 bone-bound masses with their centres, `armRadius`, `legRadius` and one `Strength`
     (`Units/PhysicalBody.cs:37-45`).
   - From those three, `OwnBody.Make` (`Units/OwnBody.cs:530-768`) and `PhysicalFall.LetGo`
     (`Units/PhysicalFall.cs:449-551`) build every mass, collider, anchor, drive and strength deterministically.
2. **Strength is no longer "limb thickness times one number" for the joints that matter for getting up.**
   - Legs: the knee holds 1.6 x (half the body weight x thigh length) (`PhysicalBody.cs:202-210`). The hip is 1.5 x the
     knee and the ankle 0.7 x the knee (`OwnBody.cs:51`).
   - Back: 2.4 x what the upper body asks when bent level (`PhysicalBody.cs:155`).
   - Arms: since October 14, `max(thickness, 1.6 x need)` (`PhysicalBody.cs:167-198`). Need wins for all three miners.
   - Neck: 3 x head weight x neck lever (`OwnBody.cs:580`).
   - The thickness rule `(armRadius/0.047)^3 x 70 or 60 N m` survives only as the arms' floor, the wrist and the grip.
   - **Consequence:** every body's capacity is exactly 1.6 x its need in the reference postures, at `Strength` 1 (checked
     for the three miners and for a 1.4x-leg, 0.7x-arm, 2x-head, 1.5x-trunk variant). Proportions change a body's reserve
     only through geometry (stances away from the reference posture), never through size or mass. How strength scales is
     **Open** (the proposal says so), so a body maker must expose the rule and must not fix it.
3. **The three miners differ** in everything the Blender presets set: size (1.42/1.86/1.56 m), mass (40.7/58.5/87.6 kg),
   head share (24.5/11.1/11.2%), leg share, limb radii and trunk shape. One `legSegment` serves as both thigh and shin
   (thigh = shin is built into the Blender body as well). They share one sole width, one ankle height (0.085 m) and every
   code constant.
4. **Physics centres sit lower than the measured ones.** Each link's centre of mass and inertia come from its collider
   shapes. Unity computes them automatically; nothing sets them, and the measured inertias in `miners.json` are never
   read.
   - The physics head is centred at the Head bone's *base*, about 8 cm below the measured head centre, in all three
     miners.
   - The neck lever is 0.175 m against a measured 0.258 m (Small). The hips are 3.5-6.4 cm low and the trunk 2-7 cm low.
   - This matters most for head-heavy bodies.
5. **Recommended body sampler: a "made body" built from a spec through the game's own classes, with no model.**
   - It sets the same components the prefab has: `ProceduralBiped`, `PhysicalBody`, `MinerBody` with an empty 19-bone rig
     at the spec's joints, and a `NavMeshAgent`.
   - Both physical bodies then come from exactly the code the game runs, and no existing file has to change.
   - Bone-scaling a miner model does not work cheaply. `MinerBody` only turns bones and keeps the modelled lengths. Unity
     shears non-uniform bone scale under rotation. Clothes, hanging things and grips are measured on the unscaled model.
     `MinerSetup` rejects models off their weighed size.
   - Blender presets per body are faithful but take minutes each. Use them for a few showcase bodies only.
6. **What assumes Small:**
   - `Resources/RiseWays/Small.txt`, the only ways file, is looked up by miner name, so Long and Round get up "the old
     way".
   - `Resources/RiseModels/SmallRise*.onnx` and the behaviour names `SmallRise*` in `Learning/RiseAgent.cs`.
   - The teaching and the game's getting-up test choose "Small" by name.
   - Most of `OwnRiser`'s balance settings were searched on Small.
   - Settings found on all three miners are still general only across three. Many contain absolute metres and m/s that
     do not scale with the body (listed in §4.4).

---

## 1. Where Small, Long and Round differ

### 1.1 Where a miner is defined: one chain of sources

| Stage | File : lines | What it holds |
|---|---|---|
| Preset (the creator-like parameters) | `Art/Blender/Worker/workers.py:106-196` (Small 109-133, Long 136-164, Round 167-195) | height; head radius `head`, `head_shape`; levels `hip`, `waist`, `chest` (shares of height), `collar`; `shoulder_w`, `hip_w`, `shoulder_h`, `neck_drop`; `upper_arm`, `forearm`, `hand`, `foot` (shares of height); trunk ellipses (half-width, half-depth at pelvis, waist, chest, collar); limb radii `arm`, `leg`; `neck_r`; pose, gait, outfit |
| Skeleton | `Art/Blender/Worker/body.py:24-73` | joints from the preset. Thigh = shin = 0.51 x (hip height − ankle) (`body.py:69-70`). Ankle at 0.085 m for every body (`body.py:59`) |
| Bones | `Art/Blender/Worker/rigging.py:71-110` | bone heads and tails. The Head bone starts at `on_head(0, .12, -.62)`, about 0.62 x head radius below the skull's centre (`rigging.py:75, 81`) |
| Game measures | `rigging.py:585-615` (`dimensions`), `618-626` (`hang`) | hipHeight, hipWidth, leg = (thigh+shin)/2, ankleHeight, heel/ball/toe = foot x 0.30/0.35/0.23, waistRise, shoulder (out, up, forward from the waist), headRise (to the Head bone's **base**), upperArm, forearm, armOut/armDrop/armForward |
| Weights | `Art/Blender/Worker/weights.py:129-171` (flesh at 1000 kg/m³: `:26`), `213-256` (worn and carried), `259-279` (per bone) | for each of 19 bones: mass, flesh-only mass, centre, `along`, three moments of inertia. armRadius and legRadius are the preset's `arm` and `leg` |
| Manifest | `Assets/_WonderGather/Art/Worker/Miners/miners.json` | all of the above per miner, plus gait character, hanging things, grips, skirt slack, `bodyFront`/`faceFront`/`headHalf` |
| Prefab | `Editor/MinerSetup.cs:290-513` | the unit (§2.2) |

### 1.2 The presets (source parameters, `workers.py`)

| | Small | Long | Round |
|---|---|---|---|
| height (m) | 1.42 | 1.86 | 1.56 |
| head radius (m); shape (x,y,z) | 0.13; (1.04, 1, 1) | 0.112; (0.94, 1, 1.14) | 0.128; (1.08, 1, 0.98) |
| hip / waist / chest level (share of height) | 0.45 / 0.57 / 0.68 | 0.52 / 0.615 / 0.735 | 0.47 / 0.59 / 0.72 |
| shoulder_w, hip_w (m) | 0.145, 0.068 | 0.165, 0.082 | 0.19, 0.095 |
| upper_arm, forearm, hand, foot (share of height) | 0.165, 0.14, 0.105, 0.17 | 0.175, 0.15, 0.10, 0.155 | 0.16, 0.14, 0.115, 0.17 |
| arm radius, leg radius (m) | 0.033, 0.041 | 0.036, 0.046 | 0.045, 0.058 |
| trunk half-width x half-depth: pelvis / waist / chest (m) | .115x.085 / .105x.078 / .12x.085 | .125x.09 / .115x.08 / .14x.095 | .17x.13 / .185x.15 / .175x.13 |
| other | neck_r 0.4 | neck_r 0.48, stoop 0.7 (head carried forward) | neck_r 0.6, shoulder_h 0.055, neck_drop 0.03 |

### 1.3 The game's measures (`miners.json` to `ProceduralBiped.Proportions`, as saved in the prefabs)

| | Small | Long | Round | Set at |
|---|---|---|---|---|
| model hip height (m) | 0.619 | 0.947 | 0.713 | `rigging.py:598` |
| biped hipHeight = ankle + 0.956 x 2 x leg | 0.6057 | 0.9257 | 0.6976 | `MinerSetup.cs:330, 377` (knees kept slightly bent) |
| legSegment (thigh **and** shin) | 0.2723 | 0.4397 | 0.3204 | `MinerSetup.cs:378` |
| hipWidth (half the hips) | 0.068 | 0.082 | 0.095 | |
| ankleHeight | 0.085 | 0.085 | 0.085 | |
| heel / ball / toe length | .0724/.0845/.0555 | .0865/.1009/.0663 | .0796/.0928/.0610 | |
| waistRise | 0.1904 | 0.1967 | 0.2072 | |
| shoulder (x out, y up, z fwd from waist) | (.145,.2059,−.010) | (.165,.2883,.032) | (.190,.2886,−.035) | |
| torsoRise = shoulder.y / 2 | 0.1030 | 0.1441 | 0.1443 | `MinerSetup.cs:380` |
| headRise (waist to Head bone base) | 0.3805 | 0.4901 | 0.3876 | |
| upperArm, forearm | .2343, .1988 | .3255, .2790 | .2496, .2184 | |
| armHang (out, down, fwd) | (.093,.401,.024) | (.101,.565,.030) | (.164,.414,.048) | |
| scale = model hip height / 1.43 | 0.4329 | 0.6624 | 0.4987 | `MinerSetup.cs:321` |
| bodyFront / faceFront / headHalf | .1452/.1525/.163 | .1706/.3065/.1329 | .1963/.1588/.1976 | |
| gait froude, bounce, sway, armSwing | .27, 1.35, .9, 1.15 | .21, .75, .8, 1.25 | .24, 1.1, 1.7, .75 | |
| leg share = 2·leg / height | 0.384 | 0.473 | 0.411 | *derived* |
| arm share = (upper+fore) / height | 0.305 | 0.325 | 0.300 | *derived* |

`bodyFront` and `headHalf` are named as "shape for a tool" (`ProceduralBiped.cs:62-65`), but both are load-bearing for
physics. `bodyFront` sets the hip capsule's radius and `headHalf` sets the head sphere's radius (§1.6).

### 1.4 Weights: per link as `OwnBody` groups them

The bone-to-link mapping is in `OwnBody.cs:567-613`. `Weighs()` (`OwnBody.cs:410-420`) sums the bones whose names start
with the given prefixes, with a 0.1 kg floor. `PhysicalFall` groups them the same way except that the shin also carries
the boot (`PhysicalFall.cs:511`).

| kg (*derived*) | Small | Long | Round |
|---|---|---|---|
| whole (flesh + worn and carried) | 40.7 (38.9 + 1.8) | 58.5 (53.9 + 4.6) | 87.6 (84.9 + 2.8) |
| Hips = Pelvis | 7.88 | 9.68 | 23.63 |
| Trunk = Spine + Chest + Neck | 8.96 | 17.04 | 26.20 |
| Head | 9.96 (**24.5%**) | 6.48 (11.1%) | 9.82 (11.2%) |
| Upper arm | 1.12 | 1.82 | 2.17 |
| Forearm = Forearm + Hand | 0.80 | 1.38 | 1.53 |
| Thigh | 2.38 | 4.87 | 5.60 |
| Shin | 1.35 | 2.74 | 3.14 |
| Boot standing = Foot + Toe (`OwnBody.cs:609`) | 1.29 | 1.83 | 1.55 |
| Getting up: boot = Foot, toe link = Toe (`OwnBody.cs:608-609, 691`) | 1.08 + 0.21 | 1.52 + 0.30 | 1.30 + 0.26 |
| upper body (everything above the hips; `MinerSetup.cs:505-506`) | 22.76 | 29.91 | 43.43 |

### 1.5 Strengths: the formulas and the numbers

All capacities are multiplied by `PhysicalBody.Strength`, which is clamped to 0.1-4 (`PhysicalBody.cs:45, 121`). The
panel offers 0.3-3 (`Units/MinerPanel.cs:19`) and applies it at `Units/MinerWorkPreview.cs:771-779, 816`. They are taken
"now", that is times the fresh share of the muscle group (`PhysicalBody.cs:53-85, 156, 211-217`), **at the moment the
body is made**: `OwnBody.cs:549-550, 573, 580, 593, 596, 604, 610, 613`. The body standing or getting up by its own
joints never reports work (`Worked` is only called from `PhysicalBalance.cs:565` and `PhysicalBack.cs:87`), so it does
not tire while doing so.

| Joint (link) | Formula | File : line |
|---|---|---|
| Knee (shin) | `KneeHolds 1.6 x mass x g x 0.5 x legSegment x Strength` | `PhysicalBody.cs:35, 202-211` |
| Hip (thigh) | knee x `HipGives 1.5` | `OwnBody.cs:51, 604`; `PhysicalFall.cs:55, 409` |
| Ankle (boot) | knee x `AnkleGives 0.7` | `OwnBody.cs:51, 613` |
| Toe (getting up only) | ankle x `ToeGives 0.3`, hinge −80/+20° | `OwnBody.cs:179-180, 700-702` |
| Back (trunk) | `BackHolds 2.4 x upperAsks x g x Strength`. `upperAsks` = Σ over upper parts of mass x distance(centre, hips), taken once at `Configure` from the bones | `PhysicalBody.cs:32, 134-135, 155-156` |
| Neck (head) | `NeckHolds 3 x head mass x HeadTimes x g x neckLever x Strength`, with neckLever = max(0.03, headRise − 2 x torsoRise) | `OwnBody.cs:51, 578-580`; `PhysicalFall.cs:410, 484` |
| Shoulder (upper arm) | `max(70 x Build, 1.6 x ½ x upperKg x g x upperArmLength) x Strength`, with Build = (armRadius / 0.047)³. The arm lengths are read from the **rig's bone positions** | `PhysicalBody.cs:29, 166, 174-197` |
| Elbow (forearm) | the same with `60 x Build` and forearm length | the same |
| Wrist, hand's hold | `12 x Build`, `400 x (armRadius/0.047)² N` (no getting-up role) | `PhysicalBody.cs:199-200` |
| Bench knobs | `OwnBody.ArmsTimes`, `OwnBody.HeadTimes`: **own body only**, not `PhysicalFall`, not `PhysicalBody` | `OwnBody.cs:312, 315, 578, 593, 596` |

| N m at Strength 1, fresh (*derived*) | Small | Long | Round |
|---|---|---|---|
| knee / hip / ankle / toe | 87 / 130 / 61 / 18 | 202 / 303 / 141 / 42 | 220 / 331 / 154 / 46 |
| back | 257 | 347 | 462 |
| neck (neckLever) | 51 (0.175 m) | 38 (0.202 m) | 29 (0.099 m) |
| shoulder: thickness → need → **has** | 24 → 42 → **42** | 31 → 76 → **76** | 61 → 85 → **85** |
| elbow: thickness → need → **has** | 21 → 36 → **36** | 27 → 65 → **65** | 53 → 74 → **74** |

**Consequence.** For every body, knee capacity / (½ weight x thigh) = 1.60 and shoulder capacity / (½ upper body x upper
arm) = 1.60 at `Strength` 1. The `example.py` runs confirm this for all three miners and for each miner with legs 1.4x,
arms 0.7x, head mass 2x and trunk mass 1.5x.

A heavier head, a heavier trunk or longer legs make the neck, back, arms and legs stronger by the same factor, so no body
is "weaker for its build" except through `Strength`. Neither is a body's size: the research's square-cube reserve
(`Docs/Research/research_notes/Getting up for any body/body_build_and_floor_rise.md:421`) is cancelled by construction.
Whether strength should keep doing this is the Open question "how strength is scaled"
(`Docs/Design/GettingUpForAnyBody.md:11-12`).

> **Decided by Luis, November 3** ([the message](../../Correspondence/2026-11-03_STRENGTH_SIZE_AND_EVERYTHING_ON_GITHUB.md)):
> strength from muscle, not from need; joints slower with size; the weights where the models have them. **How it is to
> be done:** the muscle rule as data beside the present one (`PhysicalBody`), each group's capacity the creator's number
> times a reference times (limb radius / reference radius)^3, the references set so that the three miners' mean capacity
> stays near today's (shoulder about 122 N m at a 0.047 m arm, knee about 137 N m at a 0.046 m leg, by those means; the
> neck kept as now until its thickness is modelled); then Small, Long and Round measured standing, stepping and at the
> rock (the pace Luis liked, R1, may change: thin-armed miners would rest more) before it becomes the default. The
> measured centres the same way. Expected at Strength 1 (shoulder / knee, N m): Small 42 / 97, Long 54 / 137, Round 106 /
> 275 (today 42 / 87, 76 / 202, 85 / 220): Long weaker for its height, Round stronger, Small's arms as now.

### 1.6 Contact shapes (built identically for any body; `OwnBody.cs:567-623`, `PhysicalFall.cs:471-531`)

| Link | Shape and rule | Small | Long | Round |
|---|---|---|---|---|
| Hips | capsule across (direction x), radius `girth = clamp(bodyFront, 1.5 x legRadius, 0.3 x hipHeight)`, height `2 x (hipWidth + girth)` (`OwnBody.cs:568-571`) | r .145, h .426 | r .171, h .505 | r .196, h .583 |
| Trunk | capsule along, radius `breadth = max(shoulder.x − armRadius, 0.5 x shoulder.x)`, half-length `torsoRise + breadth/2`, centred at waist + torsoRise (`OwnBody.cs:574-576`) | r .112, h .318 | r .129, h .417 | r .145, h .434 |
| Head | sphere, radius `max(0.06, headHalf)`, centred at the Head bone's base height (`OwnBody.cs:581-583`) | r .163 | r .133 | r .198 |
| Upper arm | capsule, radius armRadius, length = segment (`OwnBody.cs:594`) | r .033, h .234 | r .036, h .326 | r .045, h .250 |
| Forearm + hand | capsule, radius armRadius, half-length = segment half + armRadius; the hand is the rounded end (`OwnBody.cs:597`) | h .265 | h .351 | h .308 |
| Thigh | capsule, radius 1.25 x legRadius (`OwnBody.cs:605`) | r .051, h .272 | r .058, h .440 | r .073, h .320 |
| Shin | capsule, radius legRadius (`OwnBody.cs:611`) | r .041 | r .046 | r .058 |
| Boot (sole) | box = foot segment `localScale` = **0.10 x 0.06** x (heel + ball) (`MinerSetup.cs:365`, `OwnBody.cs:614-618`) | x .157 | x .187 | x .172 |
| Toe (getting up only) | box 0.10 x 0.05 x toeLength on a hinge at the ball (`MinerSetup.cs:366`, `OwnBody.cs:680-706`) | x .056 | x .066 | x .061 |

- Every part uses friction 0.7 dynamic and 0.8 static, with bounciness 0 (`OwnBody.cs:548`; `PhysicalFall.cs:466`).
- Self-collision is off while standing. When down, only the "Strike" pairs collide (shins against trunk, head, the other
  shin and the arms; thighs against the head; forearms against trunk and head) (`OwnBody.cs:478-488, 709-713`).
- `GroundTouch` is on the boots always, and on the forearms, thighs, shins and toes when down (`OwnBody.cs:675-706`).

### 1.7 Centres of mass and inertia

- **No body code sets `centerOfMass` or `inertiaTensor`.** A grep over `Scripts`, `Learning` and `Tests` finds only
  `PhysicalHands.cs:177-179, 360-362` (tools) and the bench's `BootTurns` scaling (`Tests/PlayMode/BodysOwnBench.cs:368-373`).
  Unity computes each link's centre and inertia from its collider shapes at the given mass, with uniform density per
  link.
- The `miners.json` per-part `inertia` (and `body`, `along`) are measured but never read. `MinerSetup.WeighedPart` declares
  only `bone`, `mass` and `at` (`MinerSetup.cs:68-75`).
- `PhysicalBody.Part.centre` (the measured centres) feeds only the posed body: `CentreOfMass`, `StandsOn`,
  `PhysicalBalance`, `upperAsks` and arm needs. It never reaches the articulation or the let-go body.

Heights above the hip joints, standing (*derived*):

| | Hips: physics / measured | Trunk: physics / measured | Head: physics / measured | Neck lever: physics / measured from the shoulder |
|---|---|---|---|---|
| Small | 0 / 0.054 | 0.293 / 0.346 | 0.571 / 0.654 (**−8.3 cm**) | 0.175 / 0.258 |
| Long | 0 / 0.035 | 0.341 / 0.412 | 0.687 / 0.768 (−8.1 cm; measured head 15 cm **ahead**) | 0.202 / 0.283 |
| Round | 0 / 0.064 | 0.352 / 0.371 | 0.595 / 0.677 (−8.2 cm) | 0.099 / 0.181 |

- The head is lower because `headRise` is measured to the Head bone's base (`rigging.py:608`, `75`) and the head
  segment is placed there (`ProceduralBiped.cs:1242`).
- Long's forward head (the stoop) is dropped. The prefab stands the model with its head straight over its hips by default
  (`MinerSetup.cs:398-402`), and the biped places the head on the chest's axis (`ProceduralBiped.cs:1242`).
- The neck joint sits at shoulder height, `anchors[Head] = (0, −neckLever, 0)` (`OwnBody.cs:584`). The real collar is
  5-6 cm higher.
- The effect on behaviour is not measured. For a head-heavy body, the physics head turns about a lever about 30% shorter
  than the model's, which is about half the turning inertia about the neck.

### 1.8 Differences that do not reach the physical bodies

- Gait character (`bounce`, `sway`, `armSwing`, `armCarry`, `armSwingSide`) and walking speed
  `sqrt(froude x g x standing hip height)` (`MinerSetup.cs:327-339`).
- Selection ring and the unit's own capsule (radius `clamp(0.15 x height, 0.2, 0.3)`: `MinerSetup.cs:322-326`). OwnBody
  ignores it (`OwnBody.cs:713`) and PhysicalFall disables it while lying (`PhysicalFall.cs:542-543`).
- Hanging things, skirt flaps, grips, the pickaxe and the lamp (`MinerSetup.cs:428-475, 521-602, 608-662, 693-741`).
  Their weights are already inside the part masses (weights.py "worn"). Their swinging is drawn only (`MinerBody`).
- Face, hair, outfit, atlas, materials, LODs.

---

## 2. The construction chain

```
workers.py preset ─► body.py skeleton ─► rigging.py bones + dimensions/hang ─► weights.py ─► miners.json
                                                                                    │
          Editor/MinerSetup.Create (prefab, editor time) ◄──────────────────────────┘
            ├─ "Body solution": 13 invisible segments (+2 toes)        ◄─ feet/toe box sizes fixed here
            ├─ ProceduralBiped.SetProportions(P) + SetTuning            ◄─ all segment lengths
            ├─ the FBX model (rig) + MinerBody.Configure(bones, solution, hanging, flaps, grips)
            └─ Weigh → PhysicalBody.Configure(parts on bones, arms, upper, hips, armRadius, legRadius)
Runtime, every frame:
  ProceduralBiped.LateUpdate poses the segments (position, rotation, localScale.y = half length)
  MinerBody.LateUpdate turns the model's bones to the segments (drawing only)
On demand:
  OwnBody.Make(low, plain, down)  reads segments + P + PhysicalBody → 13 ArticulationBody links (+2 toes)
  PhysicalFall.LetGo()            reads segments + P + PhysicalBody → 11 Rigidbodies + ConfigurableJoints
  Each physical body then writes the segments back each frame, and MinerBody draws from them (TurnsAsSolved)
```

### 2.1 Editor: `MinerSetup.Create` (`Editor/MinerSetup.cs:290-481`)

| Lines | What is built | Numbers read |
|---|---|---|
| 304-319 | imports `Miner_<name>.fbx` (Generic, no avatar) | model |
| 321 | `scale = e.hipHeight / 1.43` | model hip height |
| 322-343 | root; unit capsule; NavMeshAgent (speed from froude: 330-331); UnitMotor; SelectableUnit and ring | height, froude, ankle, leg |
| 345-369 | **"Body solution"**: Pelvis, Torso, Head, thighs, shins, upper arms, forearms (scale 1, overwritten each frame); feet `(0.10, 0.06, heel+ball)`; toes `(0.10, 0.05, toe)` | heel, ball, toe lengths |
| 370-390 | `ProceduralBiped.Configure`, `ConfigureToes`, `ConfigureRing`, `SetProportions` (hipHeight = ankle + .956·2·leg; torsoRise = shoulder.y/2), `SetTuning(.65, clamp(.16·scale, .05, .3), .34)` | all Proportions |
| 393-407 | model placed and turned to face forward, its "up" from Pelvis to Head (402: `-minerStance built` keeps the built stoop); height checked within 15% | bone positions |
| 409-428 | three LODs, outline, lamp | |
| 430-474 | `MinerBody.Configure(Bones, Solution, hanging, flaps, skirtSlack, grips)`; tool | bone names (fixed list 462-465) |
| 475 | `MeasureThings`: what a hand needs to take hanging things, measured on the skin | mesh |
| 476, 486-513 | **`Weigh`**: frame from four joints (hips, collar, shoulders); asserts the model is at its weighed size within 2% (496-497); centres into bone space (498-502); upper body = all but Pelvis, Thigh, Shin, Foot and Toe (505-506); `PhysicalBody.Configure` (507-509) | `weights.parts[].bone/mass/at`, `frame`, `armRadius`, `legRadius` |

### 2.2 The posed body: `ProceduralBiped` (`Units/ProceduralBiped.cs`)

- `SetProportions` validates hipHeight > 0.2, hipWidth > 0.01, legSegment > 0.1, arms > 0.05, scale > 0.1 and
  armHang.y > 0.05, then applies (`114-134`). Derived values: `LegReach = 2·leg·1.33/1.36`, `LegLimit = 2·leg − 0.001`,
  `NarrowStance = 2/3·hipWidth`, tolerances x scale (`132-133`), `StepSize = sqrt(hipHeight/0.7)` (`524`).
- `Segment(part, from, to, thickness)` sets position = middle, rotation from up to the limb, and
  `localScale = (thickness, length/2, thickness)` (`1070-1075`). `localScale.y` is the half-length every physical body
  reads (`OwnBody.cs:396-400`, `PhysicalFall.cs:311-315`).
- Pelvis at the hips (the hip joints' middle). Torso at waist + `torsoRise` along the chest. Head at waist + `headRise`
  along the chest (`1238-1255`). Shoulders at waist + chest·(±shoulder.x, shoulder.y, shoulder.z) (`1264`).
- Legs: hip = hips ± hipWidth; knee by two equal segments, `kneeOut = sqrt(L² − d²/4)` (`1299-1303`). Feet and toes are
  placed from the support frames (`1292-1298`). Arms by two-bone IK with `upperArm` and `forearm` (`1401-1415`).
- `LetGo` true: not posed; the physical body writes the segments (`570, 574`).

### 2.3 The rig adapter: `MinerBody` (`Units/MinerBody.cs`)

- `Bones` (pelvis, spine, chest, neck, head, upperArms, forearms, hands, thighs, shins, feet, toes) and `Solution` (the
  segments) (`15-26`). `Ready` needs all of them except toes (`347-350`).
- `CaptureRest` stores each bone's rest turn relative to the root, the limb aims (directions only), and the pelvis
  offset from the middle of the thighs (`353-454`).
- `LateUpdate` places the pelvis bone and only **turns** every other bone. Each bone "keeps its modelled length" (header
  `5-10`, code `510-577`). The limb's pole comes from the physical parts' own frames when `TurnsAsSolved` is called that
  frame (`190-195, 477-500`). That is the October 30 mend.
- It does not affect physics. Its bones are read by `PhysicalBody` (§2.4) and by `MinerWorkPreview.cs:818` (`tall`).

### 2.4 Weights and strength: `PhysicalBody` (`Units/PhysicalBody.cs`)

- `Configure(parts, armParts[6], upperParts, hips, armRadius, legRadius)` (`124-136`) sums the mass and takes
  `upperAsks` once, from the bones as they stand.
- The capacities and their reads are in §1.5. `ArmNeeds` reads shoulder, elbow and hand bone positions the first time it
  is asked, then caches the result (`177-191`). `KneeCapacity` reads `ProceduralBiped.BodyProportions.legSegment`
  (`207`).
- Tiredness has four groups (left arm, right arm, back, legs) (`57-85`). `Refresh()` resets them (`85`).

### 2.5 The body that stands, steps and gets up: `OwnBody.Make` (`Units/OwnBody.cs:530-768`)

Indices: Hips 0, Trunk 1, Head 2, UpperArm 3/4, Forearm 5/6, Thigh 7/8, Shin 9/10, Foot 11/12. Parents:
`From = {-1, Hips, Trunk, Trunk, Trunk, UpperArm, UpperArm+1, Hips, Hips, Thigh, Thigh+1, Shin, Shin+1}` (`OwnBody.cs:47`).

| Link | Made at (from the segment) | Mass | Joint and anchor (in the link) | Strength (forceLimit) |
|---|---|---|---|---|
| Hips (root) | pelvis position and turn (`567`) | Pelvis | root: immovable false, solver 14/4 (`641`) | 0 |
| Trunk | torso (`573`) | Spine+Chest+Neck | spherical; `(0, −torsoRise, 0)` = waist (`576`) | `BackNow` |
| Head | head (`580`) | Head x HeadTimes | spherical; `(0, −neckLever, 0)` = shoulder height (`584`) | neck (§1.5) |
| Upper arm | segment middle, turned so its forward is the elbow's bend (`590-593`, `Along` 402-408, `Bends` 389-394) | UpperArm.X | spherical; `(0, −half, 0)` = shoulder (`595`) | `ShoulderOf(i) x ArmsTimes` |
| Forearm | the same (`596`) | Forearm.X + Hand.X | **revolute**; `(0, −half, 0)` = elbow (`598`) | `ElbowOf(i) x ArmsTimes` |
| Thigh | the same, knee forward (`601-604`) | Thigh.X | spherical; hip (`606`) | knee x 1.5 |
| Shin | (`610`) | Shin.X | **revolute**; knee (`612`) | knee |
| Boot | foot segment (`613`) | Foot.X (+Toe.X standing) | spherical; ankle = the shin's far end in boot space (`617`) | knee x 0.7 |
| Toe (down only) | toe segment (`684-686`) | Toe.X | revolute at the ball, limited −80/+20° (`695-702`) | ankle x 0.3 |

- **Rested relations:** `rested[i] = parent⁻¹ · child` as made (`628-631`), and `madeTurned` (`718`). Drive targets are
  turns from that pose: `Aims` and `Asks` (`965-981`), where target 0 means "as made".
  - Made from the plain stand (`plain`), the standing pose is kept: `stoodLocal`, `stoodHips`, `stoodBoot`, toe pose
    (`744-759`); height and over-the-boots after the first keep (`957`).
  - Made low or lying, `toStood = rested⁻¹ · stoodLocal` (`762`) is what "how it stands" means to the riser and keeper.
    `OwnRiser` aims `toStood[j] · Turn(j)` (`OwnRiser.cs:848`), so a way's degrees are offsets from **this body's own
    standing pose**.
- **Drives** (`632-674`):
  - all axes `FreeMotion` (`647-648`); no joint limits unless `HingesStop` (`195-196, 658-671`, off);
  - `spring = min(strength / (OwnKeeper.FullAt 92.743° in rad), Rule² / dt² x Lighter(i))`, with Rule = 0.64038;
  - `damper = 0.38328 x 2 x sqrt(spring x Lighter)`; `forceLimit = strength` (`649-655`; settings `OwnKeeper.cs:41`);
  - `Lighter(i)` is the turning weight of the lighter side, from part positions and lengths at make, so it depends on
    the pose (`437-449`).
- The riser re-springs: `min(has / 35°, RuleTimes 10 x Rule² / dt² x lighter)`, damped 0.7, toned and slackened
  (`OwnRiser.cs:115-127, 353-376`).
- Every link has zero damping and friction, continuous collision, `maxAngularVelocity 20` and `maxDepenetrationVelocity
  1.5` (`636-640`).
  - The every-frame review measured boots at 115-154 rad/s, so this per-link bound evidently does not hold on
    articulation links. Not checked further here.
  - Joint-speed bounds exist only as `maxJointVelocity`, through `OwnKeeper.TurnsAtMost` (0) and `BootTurnsAtMost`
    (with HingesStop) (`657, 671`).
- **Hand-off:** `OwnKeeper` is given parts, masses, strength, hinges, anchors, sole points and sizes, and Lighter (`731`).
  It measures thigh and shin length from the anchors (`OwnKeeper.cs:445-470`). `OwnRiser` is given the same plus
  `toStood`, `rested`, `stoodBoot`, standing hip height, ways and touches (`763`).

### 2.6 The let-go body: `PhysicalFall.LetGo` (`Units/PhysicalFall.cs:449-551`)

- There are 11 Rigidbodies: the boot is a child BoxCollider of the shin, so the shin is a compound with mass
  Shin+Foot+Toe (`511-531`).
- Settings: linear damping 0.05, angular damping 0.6, solver 14/4, `maxAngularVelocity 20` (`338-354`).
- The colliders are the same rules as §1.6 (`471-531`).
- Joints are ConfigurableJoints whose **rest is the body standing straight**. The part is put in its standing turn while
  the joint is made, then put back (`365-393`).
- These are the game's **only joint ranges**: Waist (−25, 70, 30, 25), Neck (−40, 45, 50, 30), Shoulder (−170, 50, 70,
  95), Elbow (−145, 3, 5, 3), Hip (−20, 120, 35, 45), Knee (−145, 3, 5, 3) (bend one way, bend the other, twist, side;
  `40-42`). They are the same for every body.
- Holds: slerp drives with `spring = strength / 35°`, damper `strength x 0.12`, max force = strength. The strengths are
  back, neck, shoulder, elbow, hip (knee x 1.5) and knee, each times tone (`397-446`). How long it lies scales with
  `1/sqrt(Strength)` (`99-104`).
- Its own getting-up ways ("the old way"): `curl` and `knees`, found by `GetUpSearch` on the three miners together
  (`138-141, 187-201`).

### 2.7 Who reads which number

| Number | ProceduralBiped | MinerBody | PhysicalBody | OwnBody | PhysicalFall |
|---|---|---|---|---|---|
| hipHeight, legSegment | pose | | knee (legSegment) | girth clamp; riser `tall` | girth clamp |
| hipWidth | pose | | | hips capsule | hips capsule |
| waistRise, torsoRise, headRise | pose | | | trunk anchor and rod; neck lever | the same |
| shoulder (x, y, z) | pose | | | trunk radius | trunk radius |
| upperArm, forearm | arm IK | | (via rig bones) | via segments | via segments |
| heel, ball, toe | foot placement | | | via foot and toe boxes | via the boot box |
| bodyFront, headHalf | tools | | | hips radius, head radius | the same |
| armRadius, legRadius | | | arm thickness | limb radii, girth floor | the same |
| part masses (19, by bone name) | | | mass, back, arm and knee needs | link masses | Rigidbody masses |
| part centres (in bone space) | | | posed centre of mass, back, arm needs | not used | not used |
| Strength, tiredness | | | all capacities | strengths at make | holds |
| foot and toe box scale (prefab) | foot posing | | | boot and toe boxes | boot box |
| rig bone positions | | drawing | arm needs, `upperAsks`, posed centre of mass | | |

---

## 3. Making a body of other proportions

### 3.1 What actually defines a physical body

Only these define one: the `Proportions`, the two box sizes, `PhysicalBody` (masses and centres on named bones, radii,
`Strength` and tiredness), and the code constants (§1.5, §1.6, §2.5, §2.6). The model's mesh never reaches the physics.
The rig's bones reach it only through `PhysicalBody`'s arm needs, `upperAsks` and the posed centre of mass. So a body with
other proportions needs those inputs consistent with each other, and nothing else.

### 3.2 The example body mapped: legs 1.4x, arms 0.7x, head mass 2x, trunk mass 1.5x, strength 0.5-2x

| Change | Where it goes | What must be re-derived with it | What the code then does by itself |
|---|---|---|---|
| Legs 1.4x | `legSegment x 1.4` (thigh and shin both, since one number serves both) | `hipHeight = ankle + 0.956 x 2 x leg` (not x 1.4: the ankle stays at 0.085). Thigh and shin masses (x 1.4 at the same radius, or re-weighed). The rig's Thigh, Shin and Foot bones. `scale` = new hip height / 1.43. Body height, agent height and speed | knee, hip and ankle strength rise with mass x leg (still 1.6 x need). OwnKeeper and OwnRiser read the new lengths from the anchors |
| Arms 0.7x | `upperArm`, `forearm` x 0.7 | `armHang` (port `rigging.hang`, `rigging.py:618-626`); arm masses and centres; rig arm bones (they feed `ArmNeeds`) | shoulder and elbow needs fall with the arm length (thickness may then win) |
| Head mass 2x | `PhysicalBody` part "Head" mass x 2 (**not** `OwnBody.HeadTimes`, which only reaches the own body) | decide whether the head grows: `headHalf x 2^(1/3)` at the same density, or a denser head of the same size | neck x 2; back and arm needs rise. Head share: Small 24.5% → 35%, Long 11.1% → 16.7%, Round 11.2% → 17.0% (with the other changes) |
| Trunk mass 1.5x | Spine, Chest, Neck masses x 1.5 (and Pelvis if "trunk" includes the hips) | decide whether the trunk thickens (`shoulder.x`, `bodyFront`) or gets denser | back, arm and knee needs rise |
| Strength 0.5-2x | `PhysicalBody.Strength` (0.1-4) **before** `Make` | `PhysicalBody.Refresh()` for a fresh comparison | every capacity x Strength. Springs stay capped by `Rule x Lighter`, so above about 1.5-2x only the force limit grows for light links. Lying time x 1/sqrt(Strength) |

*Derived* at Strength 1, Small changed this way: 57.0 kg, hip height 0.814 m, knee 170, hip 256, ankle 119, back 438,
neck 102, shoulder 46, elbow 39 N m. For Long: 77.6 kg, knee 375, back 531. For Round: 115.3 kg, knee 406, back 712.
In every case the knee and shoulder hold exactly 1.6 x their reference need (§1.5).

Expected limits: the short-armed, long-legged variant will not reach the ground with its hands where Small's ways expect
(the research predicts exactly that, `body_build_and_floor_rise.md:247`). That is a property of the body and the
present ways, not of the maker.

### 3.3 Four ways to make such a body, and what breaks

| Way | Rig adapter | Colliders | Weights | Clothes, things, tools | Cost and verdict |
|---|---|---|---|---|---|
| **A. Scale a miner model's bones** | `MinerBody` only turns bones and keeps the modelled lengths. Length-only bone scale is non-uniform, and Unity's transform hierarchy shears it as soon as a joint bends (there is no segment-scale compensation). Compensating the child with 1/s is exact only while the child is unturned. Uniform scale works under rotation but thickens the limb too. Moving joints without scale stretches only the joint region of the skin | unaffected (they come from Proportions and radii), so the drawn mesh and the colliders disagree unless both are changed | measured on the unscaled model. `Weigh` rejects a model more than 2% off its weighed size (`MinerSetup.cs:496-497`). The import rejects >15% height (`405-407`) | skirt flaps (slack in degrees), hanging things (lengths, stops and clearances measured on the skin: `MinerSetup.cs:160-266`), grips (fingers must sit within 1 mm of the model's joints: `MinerSetup.cs:628-630`), pickaxe per miner (`tools.json`), lamp. All tied to the unscaled model | not cheap, not faithful. Pictures only |
| **B. A Blender preset per body** (`workers.py` → `miners.json` → `MinerSetup.Create`) | correct | correct | measured | fitted ("every module fits any body", `body.py:8-11`) | **faithful** but minutes per body in Blender, an import and a prefab each. Thigh = shin and ankle 0.085 are fixed in `body.py`. For a handful of showcase bodies that judges and Luis look at |
| **C. New Proportions on an existing prefab, at runtime** (`SetProportions`, `PhysicalBody.Configure`) | the drawn limbs keep the model's lengths: feet float or sink | correct | **inconsistent**: `ArmNeeds` reads the model's arm bones (`PhysicalBody.cs:185-186`), `upperAsks` the model's centres, and the posed centre of mass the model's bones | the old model's | cheap but subtly wrong. Avoid |
| **D. A made body: primitives only, through the game's own classes** | an empty 19-bone rig placed at the spec's joints. `MinerBody` turns it harmlessly | correct (the game's own rules) | from the spec; consistent with the rig | none. Their masses are already in the part masses | **cheapest and faithful.** No existing file changes |

### 3.4 Recommendation: the made body (D), anchored on the three miners

**New code** (runtime-safe, no `UnityEditor`; a sketch, nothing written):

1. **`BodySpec`** (plain data, one line when logged, so a failed body can be rebuilt):
   - `Proportions` (the 23 numbers);
   - `soleWidth`, `soleHeight`, `toeHeight`, defaulting to 0.10, 0.06 and 0.05 as `MinerSetup.cs:365-366`;
   - 19 parts: bone name, mass, centre as (along, offset);
   - `armRadius`, `legRadius`, `strength`, and the strength rule (present, thickness or explicit; §5.1);
   - optional joint ranges (unused by the present code; for layer 0);
   - name and seed.
   - `BodySpec.FromMiner(GameObject)` reads a prefab: `ProceduralBiped.SavedProportions` (`:97`), foot and toe
     `localScale`, `PhysicalBody.PartAt/PartCount/ArmRadius/LegRadius/Strength`.
   - `BodySpec.FromManifest(entry)` mirrors `MinerSetup.cs:321, 374-389`.
2. **`BodyMaker.Make(BodySpec, position, facing)`**, a runtime factory that mirrors `MinerSetup.Create` minus the model,
   LODs, materials, tool, hanging things and grips:
   - root, NavMeshAgent, unit capsule and UnitMotor (optional);
   - "Body solution" segments with the spec's box sizes;
   - `ProceduralBiped.Configure`, `ConfigureToes`, `SetProportions` and `SetTuning` with MinerSetup's rules;
   - a **rig** of empty Transforms named exactly `Pelvis, Spine, Chest, Neck, Head, UpperArm.L/R, Forearm.L/R, Hand.L/R,
     Thigh.L/R, Shin.L/R, Foot.L/R, Toe.L/R`, parented as the model's are (`rigging.py:76-97`), at the spec's standing
     joints, each bone's local Y along the bone so that centre = (0, along x length, 0);
   - `MinerBody.Configure(bones, solution)` with no hanging, flaps or grips;
   - `PhysicalBody.Configure` with the arm indices in the order of `MinerSetup.cs:508` and the upper set of
     `MinerSetup.cs:505-506`; `Strength`;
   - optionally, renderer-only primitives on the segments for pictures (as `BodysOwnBench.Shape`, `BodysOwnBench.cs:224-257`).
   - All the APIs it calls are already public: `ProceduralBiped.Configure/ConfigureToes/SetProportions/SetTuning` (`:424-426,
     114, 439`), `MinerBody.Configure` (`:203`), `PhysicalBody.Configure` (`:124`).
   - It belongs in the runtime assembly, so that the trainer build (Learning) and later the creator can use it too. The
     sampler and benches stay in Tests.
3. **`BodySampler`** (Tests): spec generators.
   - (i) **The three miners as specs.**
   - (ii) **One-variable multipliers** with re-derivation as in §3.2, for "sweep the variable" studies.
   - (iii) **Creator-like parameters**: a C# port of `body.Body` (skeleton, `body.py:24-73`), `rigging.joints`
     (`:71-110`), `rigging.dimensions/hang` (`:585-626`) and `weights.flesh` (`:129-171`, about 60 lines). This makes the
     masses follow the lengths and radii.
     - **The port can be verified part by part:** `miners.json` stores every part's flesh-only `body` mass, so the port
       must reproduce Small, Long and Round's `body` values. "Worn" mass is then added as a share or left out.
   - (iv) A seeded Latin-hypercube draw over ranges (§3.5).
4. **A made-body bench** (or `-riseMade <specs>` on `OwnRiseBench`). It spawns made bodies at the meadow path spots, as
   `OwnRiseBench.cs:218-262` does with the miners, because a bench floor is level and the game is not (memory: "the
   bench's floor is level"). Then:
   - each body first stands by its own joints from the plain stand (`own.Wanted = true` until `Stands`, then false: this
     sets `stoodKnown` and `CanRise`, `OwnBody.cs:257, 744-759, 876-894`);
   - it is let go and laid down (`PhysicalFall.LetGo`, `PhysicalFallBench.Lay`, `OwnRiseBench.Now/Put`);
   - it gets up with `OwnBody.RiseWays` set by the bench (the getting-up controller under test);
   - the bench reports success, time and every-frame faults by body class.

**The faithfulness check, first:** a made-from-spec Small, Long and Round against the prefab ones, in the same scene.

- Equal `PhysicalBody` capacities, to float error.
- Equal `OwnKeeper.Describe()` (`OwnKeeper.cs:371-379`): masses, soles, thigh and shin, weight height, strengths and
  springs. `lighter` will be near-equal because it depends on the pose.
- Standing 20 s, the four-way shove, and the fall and rise with Small's ways, within run-to-run spread. Count courses,
  not runs (memory: "runs repeat courses").
- Only then are made bodies evidence.

**Optional changes to existing files, each its own step, each measured:**

| Change | Files : lines | Why | Effect on the three miners |
|---|---|---|---|
| Separate thigh and shin lengths | `ProceduralBiped.cs:43, 132, 335, 1301-1303`; `PhysicalBody.cs:207` | to vary the thigh:shin ratio (the research: femur length and squat lean). OwnRiser and OwnKeeper already handle unequal lengths (`OwnRiser.cs:486, 534-565`; `OwnKeeper.cs:464-465`) | none if thigh = shin |
| Per-group strength multipliers in `PhysicalBody` (arms, legs, back, neck), replacing `OwnBody.ArmsTimes/HeadTimes` | `PhysicalBody.cs:155-217`; `OwnBody.cs:312-315, 578, 593, 596` | "weak arms" bodies, the same in both physical bodies | none at 1 |
| Sole width and height from data | `MinerSetup.cs:365-366` | real soles per body | the miners' soles change (re-measure standing) |
| Centre and inertia from the spec (`automaticCenterOfMass/InertiaTensor = false`) | `OwnBody.cs:632-674`; `PhysicalFall.cs:338-354` | the low head and trunk (§1.7) | changes every miner. That is a layer-0 decision for Luis, not the sampler's |

### 3.5 What the sampler should vary (test ranges; the creator's range is Luis's and Open)

- **Size:** height 1.0-2.2 m, or hip height share (Small 0.43, Long 0.51, Round 0.46 of height).
- **Leg share:** 2·leg/height 0.33-0.52 (miners 0.38-0.47).
- **Arm share:** 0.25-0.38 (miners 0.30-0.33).
- **Head share:** 6-30%. Adult 5.8%, toddler 21-25.5% (`body_build_and_floor_rise.md:185`); Small 24.5%.
- **Trunk and hip girth, limb radii:** 0.7-1.5x. Masses follow when generated by the flesh port, or are set
  independently by the multiplier path.
- **Strength:** 0.3-3 (the panel's range).
- **Tiredness at make:** 0-0.85.
- **Later:** joint range (stiff bodies) and asymmetry (one weak leg).
- **Rules:**
  - Report every result by class (toddler-like, long-legged, heavy, weak-light ...), never only as a total.
  - Always include the three miners as fixed anchors.
  - Include extremes outside the creator's range; some builds honestly cannot rise, which is Open (proposal, question 3).

### 3.6 Pitfalls a made body must avoid

- **Names drive lookups.**
  - The ways file is found by the GameObject's name when no MinerChoice entry names the unit
    (`OwnBody.cs:275-277`). A made body called "Small" would load `Small.txt`.
  - Learnt models are found by `Called` (`RiseAgent.cs:128-160`).
  - The breath seed comes from the name (`OwnBody.cs:721`).
  - Use names like `Made 017`. Use the same name to repeat a body exactly.
- **Statics are global.** `OwnBody.RiseWays/HeadTimes/ArmsTimes/HingesStop`, every `OwnKeeper`/`OwnRiser` setting and
  `PhysicalFall.GetsUpByItsOwn` apply to every body at once. Never vary a body through them.
- **Ground.** It must be on layer 6 (`ProceduralBiped.cs:86`, `OwnBody.cs:229`, `PhysicalFall.cs:231`). A NavMesh is
  needed for `OwnBody.GroundFor` (`:814-821`), or set `OwnBody.AnyGround`. `ProceduralBiped` requires a NavMeshAgent
  (`:33`).
- **Order.** The rig must be in its standing rest pose when `MinerBody.Configure` runs (`CaptureRest`) and when
  `PhysicalBody.Configure` runs (`upperAsks`). `ArmNeeds` is cached at first use. Set `Strength` and call `Refresh()`
  before each `Make`.
- **Validation limits.** `SetProportions` needs hipHeight > 0.2 and scale > 0.1 (`ProceduralBiped.cs:116`). `SetTuning`
  needs lift in 0.05-0.3 (`:441`).
- **Stand first.** A body cannot be made lying before it has stood by its own joints from the plain stand
  (`OwnBody.cs:257, 460-474`). That requires the posed body at ease, both feet planted and a plain stance
  (`OwnBody.cs:344-354`).

---

## 4. Where the present code assumes Small (or a size)

### 4.1 Small-only data, names and tests

| What | Where | Effect |
|---|---|---|
| The only ways file | `Assets/_WonderGather/Resources/RiseWays/Small.txt` (27 lines). It is loaded by miner name (`OwnBody.cs:263-284`) | Long and Round know no way. `PhysicalFall.ByItsOwn` returns "no way up for lying so" and they use the old way (`PhysicalFall.cs:788-814`, `797`) |
| Its contents | `roll` (plant, push, front) and `feet` (hands, under, prop, stay, footA, footB, pike, rollup). Poses in degrees, searched on Small. Times in seconds. Steps in shares of a leg, but `stepLifts` in metres. Kneel in shin lengths, holds in hip-height shares, `handStep` in metres | degrees and seconds are not body-relative. The ways are offsets from each body's own standing pose (`OwnRiser.cs:848`) |
| Learnt models | `Resources/RiseModels/SmallRiseFull.onnx`, `SmallRisePace.onnx` (committed); `SmallRise`, `SmallRiseFullP`, `SmallRisePFull`, `SmallRisePaceP` (untracked). Loaded by `Called` + suffix (`Learning/RiseAgent.cs:128-160`) | a learned path for Small only (to be superseded) |
| Behaviour names | `"SmallRise"`, `"SmallRiseFull"` (`RiseAgent.cs:17`), `"SmallRisePaced"` (`:26`), `"SmallRisePace"` (`:45`) | the trainer is tied to Small |
| Learned correction ranges | `RiseAgent.By` (`:66`): degrees, hip-height shares and **metres** (0.06, 0.15, 0.10). `Drives*` in degrees (`:52`) | sized on Small |
| The teaching | `Learning/RiseSchool.cs:183-184` picks "Small" by name | |
| The game's getting-up test | `Tests/PlayMode/GettingUpByItsOwnTests.cs:71-80` (Small only; it asserts `Small.txt`) | evidence is "Small only" (the every-frame review says so) |
| Searches | `GetUpSearch.cs:102` defaults to Small. `OwnRiseBench.cs:155` defaults to all three, but the ways exist only for Small | |
| Comments citing Small's values | `OwnRiser.cs:119-122` (Small's elbow 11 N m/rad cap against 36 N m); `OwnBody.cs:251-253` (`StoodHeightIs` 0.696/0.75 m); `OwnKeeper.cs:84` (`SwingsLeg` 0.544 m, off) | |

### 4.2 Settings found on Small's getting up (the only body with ways)

| Settings | Where | Notes |
|---|---|---|
| `RuleTimes = 10`, `RuleTimesFeet = 10` | `OwnRiser.cs:116-127` | chosen from Small's elbow and boot |
| `FullAt 35`, `Damped 0.7`, `Balances 150`, `BalancesAtMost 70` | `OwnRiser.cs:115` | |
| Balance on hands, knees and feet: `BalanceQuick 3.36`, `BalanceMargin 0.021 m`, `BalanceSlides`, `BalanceLeans 0.6`, `BalanceLeansAtMost 0.12 m`, `BalanceFeels`, `BalanceBorne`, `BalanceUnloads 0.429`, `BalanceLifts`, `BalanceShiftsWithin`, `BalanceShiftsAt 0.122 m/s`, `BalanceShiftsAtMost 0.04 m`, `BalanceDamps`, `BalancePushes`, `BalanceHoldLearns`, `FoldedUnder`, `BalanceShiftsDeep 0.04 m`, `KneeFaces 0.759` | `OwnRiser.cs:136-180` | "Numbers searched (Small)" (`Docs/Design/TheBodysOwn.md:1797-1799`, main project) |
| `LetsGo 0.357`, `AnkleAtMost 110`, `WandersBy 0.012 m`, `UpFrom/DownUnder/DownFor`, `ReadyAfter`, `StuckAfter 3`, `RiseTrunkAfter 0.4`, `ReleaseWaitsAtMost 4`, release walk | `OwnRiser.cs:132, 298, 380, 736-752, 1184-1195` | measured on Small's tries |
| The keeper's rise: `RisesAt 0.3`, `RiseEases 0.8`, `RiseBears 1`, `RiseLeads 0.03 m`, `RisenFor`, `RisenShort 0.07 m`, `GivesUpAfter 4`, `RiseOnFeet 0.85`, `RiseSettles 0`, `RiseCurve 1`, `StallsAfter 3` | `OwnKeeper.cs:186-215` | tuned on Small's hand-overs (October 27-29) |
| `OwnBody.ArmsSettleIn/Share`, `RiseJoints`, `ToesSlacken`, `BootsLieAt`, `RollsTheOtherWay` | `OwnBody.cs:144-148, 186, 202, 207` | from Small's teaching and bench |

### 4.3 Settings found on the three miners together (general only across three bodies)

- The keeper's standing settings: `Quick, Feels, Leans, LeansBy, Comfort, KeepsBehind, Tracks, FullAt 92.743, Rule
  0.64038, Damped 0.38328, Rights, Edge, Rises, RisesDamped, Tilts`, found by `BodysOwnBench.Search` on all three
  (`OwnKeeper.cs:28-49, 70`).
- The step: set by hand first, then searched on bodies of all three miners (`OwnKeeper.cs:72-167`; the search and its
  ranges are at `BodysOwnBench.cs:1508-1530`). Some settings were then turned off by hand because of one miner, for
  example Long (`OwnKeeper.cs:318-323`).
- The let-go body's ways, found by `GetUpSearch` on all three (`PhysicalFall.cs:138-141, 187-201`).
- Life at rest, set by hand and by the judges on all three (`OwnBody.cs:65-105`).

### 4.4 Absolute sizes that do not scale with the body (a 2 m body sees them half as large as Small does)

| Constant | Value | Where |
|---|---|---|
| ankle height | 0.085 m, every body | `body.py:59` |
| sole and toe box width and height | 0.10 x 0.06 m and 0.10 x 0.05 m | `MinerSetup.cs:365-366` |
| `OwnKeeper.Edge, Comfort, KeepsBehind` | 0.01, 0.033, 0.021 m | `OwnKeeper.cs:36, 49` |
| step: `StepsBeyond, Clears, StepsFurther` | 0.035, 0.084, 0.022 m | `OwnKeeper.cs:82, 93` |
| `SinksAtMost` (a share), `SinksDown` 0.5 m/s, `RiseLeads` 0.03 m, `RisenShort` 0.07 m | | `OwnKeeper.cs:157-158, 186` |
| riser: `BalanceMargin` 0.021, `BalanceShiftsDeep` 0.04, `BalanceShiftsAtMost` 0.04, `BalanceLeansAtMost` 0.12 m; `BalanceShiftsAt` 0.122 m/s; `StillUnder` 0.2 m/s; `GoesOnBelow` 0.12 m/s; `WandersBy` 0.012 m | | `OwnRiser.cs:137, 149, 162, 180, 298, 745` |
| ways: `stepLifts` and `handStep`/`handLift` in metres | | `Small.txt`; `OwnRiser.cs:53, 71` |
| life: `Sways` 0.004 m; `OwnBody.AtEase` 0.02 m/s; `SunkAtMost` 0.02 m; `PlainWithin` 0.02 m | | `OwnBody.cs:55, 59, 78` |
| colliders: head ≥ 0.06 m; neck lever ≥ 0.03 m | | `OwnBody.cs:579, 582` |
| step-reach and tool references from the 2.2 m test body (`ToolScale = ArmReach/0.87`, `scale = hip/1.43`) | | `ProceduralBiped.cs:98-111`; `MinerSetup.cs:321` |
| speed bounds: `maxAngularVelocity 20`, `maxDepenetrationVelocity 1.5` | | `OwnBody.cs:639`; `PhysicalFall.cs:349-350` |

### 4.5 Fixed shapes and human references (body-independent by design; may need to become build data)

- Strength references and margins: `ArmReference 0.047`, `70/60/12/400`, `BackHolds 2.4`, `KneeHolds 1.6`,
  `ArmHolds 1.6`, `NeckHolds 3`, `HipGives 1.5`, `AnkleGives 0.7`, `ToeGives 0.3` (`PhysicalBody.cs:29-35, 174`;
  `OwnBody.cs:51, 180`).
- Joint ranges: let-go only, the same for all (`PhysicalFall.cs:40-42`). The own body's hinge stops, when on, are elbow
  145°, knee 150°, 3° past straight (`OwnBody.cs:196`).
- Collider rules: hips girth, trunk breadth, thigh 1.25 x leg radius, forearm + arm radius (`OwnBody.cs:568-611`).
- Friction 0.7/0.8 (`OwnBody.cs:548`).

---

## 5. Found on the way: what bears on the new body (for decision, not decided here)

1. **The strength rule.** It keeps reserve constant across builds (§1.5). If proportions and strength are to "visibly
   shape" the getting up, Luis must say how strength scales: per build need (now), per thickness (the old arm rule,
   still a switch: `PhysicalBody.ArmsAsBodyNeeds`, `:175`), or an independent attribute. The body maker should carry the
   rule as data. (Open in `GettingUpForAnyBody.md:11-12`.)
2. **Low physics centres.** The head, trunk and hips sit 2-8 cm below their measured centres, and the neck lever is
   about 30% short (§1.7). Long's forward head is dropped. Layer 0 ("a body that cannot look broken") might take its
   link centres and inertias from the weighing (`automaticCenterOfMass/InertiaTensor` off). It would change all three
   miners, so it is a decision, not a fix.
3. **Two rests.** `PhysicalFall`'s joints rest at "standing straight" and have anatomical ranges. `OwnBody`'s rest at
   "as made" (often lying) and have none (§2.5-2.6).
   - Anatomical limits on the own body must be expressed relative to the made pose, as `HingesStop` does with
     `made − folds … made + PastStraight` (`OwnBody.cs:663-668`). Remember the measured fact: natural bend = bend as made
     − jointPosition.
   - The alternative is a standing rest. The October 14 attempt snapped the body to that rest (`OwnBody.cs:243-247`).
     Setting `jointPosition` after making might avoid that; this is untried.
4. **`HeadTimes` and `ArmsTimes`** change only the own body, so the let-go body and `PhysicalBody` disagree when they are
   used (§1.5). Do not use them for made bodies.
5. **`maxAngularVelocity 20`** is set on every link (`OwnBody.cs:639`), yet boots were measured at 154 rad/s. Find out
   which bound PhysX actually honours on articulation links before relying on one.

---

## 6. Not done, not tested

- No Unity run. All numbers are the code's formulas applied to `miners.json`. The strength figures were cross-checked
  against measured tables, but the centre and lever figures (§1.7) and the worked example (§3.2) were not checked in
  the engine.
- The `PhysicalBalance`, `PhysicalHands`, `PhysicalSwing`, `PhysicalCarry` and `PhysicalBack` internals were not mapped
  (the posed body's work). Neither were the step's internals in `OwnKeeper` beyond the settings.
- The claim that Unity non-uniform bone scale shears under rotation is general Unity transform behaviour, not tested on
  these rigs.
- That a made body reproduces a prefab miner is a prediction, to be checked as described in §3.4 before any made body
  counts as evidence.
- The main project's copy of these files (branch `claude/worker-showcase`) was not compared with the worktree. Everything
  here is the worktree at `8846684`.
