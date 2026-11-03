# Getting up for any body - layers 1 and 2: reading the body and choosing its way up

Status: a design to implement (November 3, 2026); nothing here is Locked. Luis chose the direction on November 3
(`Docs/Correspondence/2026-11-03_THE_DIRECTION_FOR_ANY_BODY.md`) and allowed stances "written once as goals"
**provisionally**, to be watched for anything robotic or repetitive. Evidence: a Python prototype
(`scratchpad/anybody/l12/`), 765 stance fits on 17 bodies, with routes scored on them. No Unity run was made.

## 0. What this covers

- **Layer 1, reading the body,** runs once per body design and is cached. It fits a short library of stances to the
  body and records, for each, whether the body can hold it, the share of its strength that takes, the balance margin,
  and whether a support can be let go. The result is the body's **stance atlas**.
- **Layer 2, choosing the way,** runs at each fall. It picks a route through the atlas and sets continuous style
  settings, which follow capacity and vary by seed. The result is a **rise plan** for layer 3.
- **Not covered:** layer 0 (joint stops and speed bounds, which come first), layer 3 (moving) and layer 4 (tuning).
  Layers 1 and 2 never drive a joint. They only say where the supports go, and when.

## 1. The stance library and its transitions

### 1.1 A stance is a goal, not a pose

Each stance is a record written once, for no body in particular:

- **Supports:** the parts that bear weight (sole, ball and toes, shin, hand, forearm, seat, calf, a hand on the thigh).
  One side is written, and the other mirrors it.
- **Placement bands**, in the body's own units:
  - hip height H;
  - the arm lA (upper arm + forearm + arm radius);
  - the leg lL (thigh + shin + ankle height) and the thigh lT;
  - the foot;
  - the half-widths of the hips and shoulders, wH and wS.

  An example band: "the wrist 0 to 0.15 lA ahead of the shoulder".
- **Posture bands:** the trunk's angle, the head's look, the elbows and the knees, with left matching right.
- **A weight rule:** the centre of mass stays inside the base by a margin. A **ready** variant also keeps it inside the
  base that is left once a named support lets go, so the weight moves before a hand or foot lifts.
- **Free limbs** (new; see §2.2): a weak pull toward the relaxed pose. The arms reach forward only when the weight must
  go forward.

The rules every stance shares (clearance, effort, no pulling, comfort) are the fit's residuals (§2.1).

### 1.2 The library

These are the prototype's families, at their sampled parameters.

| Stance | Supports | Main bands | Role |
|---|---|---|---|
| Sitting up, trunk at 15/35/60° | seat, calves (hands behind, or arms forward) | hands 0.05-0.55 lA behind the pelvis | from the back |
| Long sit | seat, calves, hands | trunk 55-85° | a rest |
| On the forearms | forearms, pelvis, knees | elbows under the shoulders | from the front, weak |
| Pushed up | hands, knees | hands under the shoulders; neck ≥ 0.55 lA up | from the front |
| Hands and knees | hands, shins (toes laid or tucked) | wrists 0-0.15 lA ahead of the shoulders; knees under the hips; trunk level ±15° | the hub; a rest |
| Tripod | hands, one shin | one knee ≥ 0.04 H up | a foot coming forward |
| Hands and feet at D | hands, soles or toes | hands D·lL ahead of the balls (0.8, 0.5, 0.3; ready at 0.25) | the bear |
| Squat at h | soles or toes, hands optional | pelvis at h·H (0.35-0.8) | the symmetrical rise |
| Tall kneel | shins | trunk 70-90° | Fury |
| Half-kneel: no hands, hand on the thigh, or hands on the ground | front sole, back shin | front ankle 0.25-0.55 lL ahead of the pelvis | the asymmetrical rise |
| Rising from the half-kneel at h | front sole, back toes | h 0.55, 0.7, 0.85 | the last push over a planted foot |
| Stoop, hands on the knees | soles, hands on thighs | knees 30° or 60° | Gowers; after the bear |
| Stand | soles | feet 0.8-1.3 wH out | the goal |

**To add before the port:**

- side-lying on a forearm (the weak adult);
- side-sit on one hand (Fury's turn);
- the tuck sit, with the feet drawn in (the CMU seat-off);
- kneeling back on the heels (a rest);
- the plank (the armour);
- the roll;
- a squat on hands and feet at h ≈ 0.25.

### 1.3 Transitions are generated, not written

Two stances are joined when their supports differ by one change: one support, or a left-right pair, is placed, loaded,
unloaded or lifted. Sitting up and the roll are the only named passages. Each edge keeps the order seen in the
references:

1. the head leads, by about 0.25 s;
2. the weight moves over the supports that stay;
3. the freed support travels;
4. it is placed before it is loaded;
5. the weight follows it;
6. the push comes last, over a planted foot, and a weak body gathers before it.

**Timing** comes from CMU subject 140, whose hip height is assumed to be 0.9 m. Times scale by k_H = √(H/0.9), the
Froude rule the game's walking already uses: Small 0.82, Long 1.01, Round 0.88.

- **From face down (2.95-3.41 s):** hands and knees by about 1.0 s; feet under 0.5 s later; knees off at 2.0-2.4 s;
  hands off 0.4-0.6 s later; up 0.4-0.55 s after that.
- **From the back or the side (4.6-4.7 s):** about 1 s to sit up; seat off at 2.75-3.0 s; the squat on hands and feet
  held about 1 s, the longest hold; hands off at 4.0-4.1 s.
- **Joint ranges used:** knees 131-158° (155-158° in the deep squat), hips to about 153°, elbows 90-130°.
- **Stages overlap.** Fury turns while still sitting up. Only the weak gather, and only before a push.

## 2. Layer 1: the per-body solver

### 2.1 What the prototype does (`b3.py`, `lib3.py`, `run3.py`)

**The body** is a 3D quasi-static model of the game's own body:

- 15 links, with OwnBody's grouping of the weighed masses;
- centres where the colliders put them, so the physics head sits low, as in the game;
- PhysicalBody's capacities and OwnBody's contact shapes;
- friction 0.6 (the game uses 0.7/0.8);
- joint ranges as written: knee −2 to 150°, elbow −2 to 145°, hip flexion −18 to 120°, ankle −50 to 30°.

**Fitting** is bounded least squares over 31 pose variables, from at most two starts. The residuals are:

- supports on the ground, the stance's bands, and everything else clear of it by 1% of H;
- the weight inside the base, and inside the remaining base for a ready stance;
- effort, shared the easiest way by a small force split, so a weaker body's fit leans on what it has;
- no pulling on the ground, and comfort: joints out of the last 12% of their ranges.

**Reading the fit.** A linear program gives r, the smallest s for which contact forces inside friction pyramids
balance the weight with every joint within s × its capacity. Hinges are checked about their axis, ball joints in 13
directions. **r is the share of its strength the worst joint must use.** The fit also reports:

- the worst joint and the hands' share of the weight;
- the base margin;
- the feasible-region depth: how far the weight may wander before no force split holds it at r ≤ 1;
- for ready stances, the margin and r once that support is gone.

### 2.2 What it found

| r (worst joint), Strength 1 | Small | Long | Round | worst |
|---|---|---|---|---|
| Stand | 0.06 | 0.05 | 0.07 | ankle |
| Pushed up / hands and knees | 0.20 / 0.21 | 0.09 / 0.20 | 0.13 / 0.26 | shoulder |
| Hands and feet, D 0.8 / 0.3 | 0.11 / 0.07 | 0.08 / 0.11 | 0.10 / 0.08 | ankle |
| Bear, hands let go at D 0.25: r, margin left | 0.26, +14 mm | 0.26, +12 mm | 0.23, +17 mm | |
| Squat, h 0.35 | 0.19 | 0.21 | 0.21 | hip |
| Squat with hands down: hands' share; then let go | 68%; 0.23, +75 mm | 24%; 0.19, +63 mm | 17%; 0.27, +84 mm | neck, shoulder |
| Rising from the half-kneel, h 0.55: no hands / hand on thigh | 0.33 / 0.30 | 0.28 / 0.28 | 0.32 / 0.30 | front hip |
| Stoop, hands on the knees | 0.12 | 0.16 | 0.20 | |
| Sitting up 15°, arms forward | cannot hold (14 mm short) | 0.14 | cannot hold (18 mm short) | |
| Sitting up 25°, propped on the hands | 0.63 | 0.15 | 0.42 | shoulder |

1. **At Strength 1 the three miners agree to within a few hundredths.** Every main stance needs a third of a body's
   strength or less. Each capacity is 1.6 × its own need (body map §1.5), so only geometry tells them apart:
   - **Sitting up** is Small's hardest task: 0.63 propped on its hands. At 15° with the arms forward it cannot hold at
     all, so it must swing through. Long's thighs and shins are 26% of its weight (against 18-20% for the others), and
     they hold its trunk there at 0.14.
   - **Leaning on the hands:** Small puts 68% of its weight on them in the squat on hands and feet.
   - **Letting go:** all three can let go from the bear with only 12-17 mm to spare.
2. **The fit adapts to the body.**
   - At Strength 0.3, Small's hands and knees needs 0.52, not the 0.70 that 0.21/0.3 would give: its arms stand nearer
     to vertical.
   - "Small, arms by thickness" (shoulders ×0.57) keeps 0.21. Its knees come 7 cm further under it, and the hand
     forces tilt toward the shoulders.
3. **By strength, the half-kneel is the harder rise, not the easier.** One leg's push needs 0.28-0.33 at the front hip,
   against 0.19-0.21 for the squat. A hand on the thigh carries only 7-8% of the weight. The half-kneel can win only on
   balance, joint range, and moving one support at a time.
4. **Geometry closes ways.**
   - Arms ×0.65 cannot reach back to the feet, so the bear is closed.
   - Ankles stiff at 5° lose the flat squat with the hands down (and at h 0.5) but keep a heels-up one.
   - Knees stiff at 100° lose the deep squat.
   - A trunk that bends only 30° cannot put its hands down in a squat.
5. **41 of 765 fits failed** (cost > 2):
   - 17 are "hands and knees, ready", which asks for a contradiction and must be rewritten;
   - 12 are the crouch-kneel ready, mostly on Long's bodies, with a part 0.21-0.24 m under the ground;
   - the rest are mostly the closed ways of item 4, where the fitter cannot tell "impossible" from "not found".

   The port needs several starts, and continuation from a neighbouring body.
6. **Free limbs drift** (Long's free elbows at 107-119°, free toes curled 40-70°). Without the relaxed-pose pull,
   stills will show limbs no living body holds.
7. **The knee range is too tight.** At 150° the squat at h 0.35 bends the knees only 131-136°; CMU's squat on hands
   and feet reaches 155-158°. Layer 0's knee stop needs 160° or more, and a sample is needed at h ≈ 0.25.

### 2.3 Cost, and the port to C#

Python takes 49 s per fit on average (37,310 CPU-seconds), with numeric derivatives. The port would use:

- **exact Jacobians** (the kinematics is a chain of rotations; finite differences cost about 32 evaluations a step);
- **warm starts** from the start pose scaled to the body, and in the creator from the previous body's atlas, which
  also keeps fits smooth from body to body;
- **a capped Levenberg-Marquardt solver**, with extra starts only on failure;
- **a dense bounded simplex** for the linear program (about 41 unknowns, 6 equalities, 220 inequalities);
- **feasible regions only where a let-go starts** (up to 145 linear programs each), or read from the dual.

**Estimated, not measured:** under 20 ms per stance and 3 s per body design (about 120 samples), on a worker thread at
unit creation, cached by spec hash and library version. Tiredness needs only the linear programs re-run.

## 3. Layer 2: choosing the way at each fall

### 3.1 In and out

- **In:** the atlas; how the body lies (facing, side, limbs, the ground's slope under each support); capacity now; the
  unit's seed and fall count; this fall's failed tries.
- **Out:** a rise plan. Each stage has a stance goal, the order of support changes, a readiness gate (the weight over
  what stays, by a margin), and its tempo, gathering, overlap, hand load and leading side. A policy for a failed push
  comes with it.
- **Re-planning:** the plan is a forecast. Layer 3 reports at each gate, and layer 2 re-plans from the state reached.

### 3.2 Few route shapes, continuous settings

There are two route shapes:

- **From the front:** pushed up, then hands and knees; the feet come under, the hands come off, and the body rises.
- **From the back:** the body sits up (swinging, or propped on its hands), puts the hands down, lifts the seat off,
  squats and rises. Or it rolls to the front (the weakest go by the side and a forearm) and takes a front route.

Continuous settings join the named ways into families, so that a body between two ways rises between them:

- **Stagger:** how late the second foot comes. At 0 both feet come together (CMU). At 1 the other knee stays down until
  the push, which is the half-kneel. In between lies the asymmetrical squat, which 90.6% of older adults used (Klima
  2016).
- **Release distance:** where the hands leave. For the able, half a leg ahead. Walked back to a quarter of a leg, it is
  the bear. Onto the knees, it is Gowers.
- **Hip height on hands and feet:** from the CMU's low squat to the toddler's high bear.
- **Hand load:** none, one hand on the thigh, or both hands on the ground.
- **Swing:** none, or the arms reaching while the trunk curls (Fury), or a rock forward before the last push. Heavier
  people swing faster (Bollinger 2019).

### 3.3 The cost and the choice

`C(ρ, θ) = T + ∫u² dt + Σ_let-go exp(−m/0.03) + w_R·range + swing + κ·failures`

The cost is in seconds; the weights are a first guess, not yet tuned.

- **T** is the time from the tempo law (§3.4), so the able prefer the short way.
- **u = r/(1 − r)** is the strain: 0 unloaded, 1 at half strength, and unbounded as r nears 1. It is integrated over
  time, so how densely a route is sampled does not matter.
- **m** is the margin left at each let-go, in hip heights.
- **range** counts entry into the last 12% of a joint's range, which is how stiff bodies lose the deep squat.
- **swing** is the shortfall a swing must carry. The passage is closed beyond 0.06 H.
- **κ·failures** (κ ≈ 1.5 s): each failed push makes the route dearer for that fall. After one to three failures the
  body turns to a way with more supports, with no counter or threshold.

θ is optimised per route in a few Gauss-Newton steps. The route is then **drawn, not picked**: P ∝ exp(−C/τ), with
τ ≈ 0.4 s and the draw seeded.

- Close costs give a mix. Only 25% of young adults rose symmetrically (VanSant 1988).
- As the build changes, the odds change smoothly. No single value of a build switches every fall at once.

### 3.4 The style laws

These are first forms, and each constant is a layer-4 knob. In the table, u = r/(1 − r) and k_H = √(H/0.9).

| Setting | Law | r 0.2 | r 0.8 | Anchor |
|---|---|---|---|---|
| Tempo | a = smooth min(4.7 m/s², g(0.9 − r)/r); stage time = CMU time · k_H · √(a_able/a) | CMU pace | ×2.1 | young adults' peak (Schwickert) |
| Gathering before a push | 0.25 s · k_H · u^1.5 of small shifts and a breath, never a frozen hold | 0.03 s | 2.0 s | the foal |
| Overlap with the next stage | 0.5/(1 + u²) of a stage | 0.47 | 0.03 | Fury, the armour |
| Stagger | 0.1 + 0.9 u²/(1 + u²) | 0.15 | 0.95 | CMU; the weak adult |
| Hand load | the least load that brings the legs' r to 0.5, ramped smoothly | 0 | up to half the weight | each hand bears 16-30% of the weight in chair rises |
| Hands kept | let go when the margin left ≥ 0.03 H · (1 + u) | early | walked back, or onto the knees | toddler; Gowers |
| Rest | 0.6 s · k_H · u^1.5 · (1 + tiredness), in the steadiest stance before the hardest push | 0.1 s | 4.8 s | the weak adult |
| Head leads | 0.25 s · k_H, ±30% | | | Fury |
| Leading side | P(left) = σ((C_right − C_left)/τ + habit + noise), which includes the lying side | | | a weak side means a lopsided rise |
| Tries | no fixed count: κ per failure, with a rest that grows each time | | | horses take 4.5 tries |

**Checks:**

- An able body keeps the CMU's pace. Small takes 2.9 s from the front; a toddler takes 2.5-3 s.
- Older adults use 80% of the knee to rise from a chair (Hortobágyi), and rise from the floor at a peak of 3.5 m/s²
  (Schwickert). Taken together, that is about 0.59 just to hold. The tempo law slows that by 30%, close to their
  measured 27% lower peak speed. So a weak body's longer rise comes mostly from more stages, gathering, less overlap,
  rests and tries, as the references show.

### 3.5 Seeded variation, against "robotic or repetitive"

- **One seed per fall,** hash(unit id, fall count), so any fall can be replayed.
- **Per unit (habits):** a preferred side (N(0, 0.5) on the logit), tempo × N(1, 0.08), a usual place in each band,
  a head carriage.
- **Per fall:** stage times × logN(0, 0.1); placements inside the bands (σ = a quarter of the band); left and right up
  to 0.05 s apart; which hand goes first; the route draw.
- **Variation never touches capacity.** A strong unit never rises like a weak one, yet no two falls are the same.

## 4. Predictions from the prototype, checked against the human ladder

`route.py` scored the routes of §3.2 on each body's atlas with the first weights (`l12/res_routes_final.json`). Odds
are the draw's, grouped by kind of way. Times include gathering and one rest, but not tries (layer 3).

| Body | From the front | From the back | r max | Time | People |
|---|---|---|---|---|---|
| Small | squat 55%, half-kneel 19%, bear 14%, heels-up squat 12% | rolls to the front 86% | 0.25 | 2.9 s | the toddler rolls over, then squats or bears, in 2.5-3 s ✓ |
| Long | squat 59%, half-kneel 19%, heels-up 13% | sits up, kneels, one knee (Fury) 57%; rolls 32% | 0.21 | 3.5 s | Fury, a tall man, sits up and kneels ✓ |
| Round | squat 49%, half-kneel 20%, bear 16%, heels-up 16% | rolls 85% | 0.27 | 3.2 s | the proposal's slow, resting Round ✗ |
| Small / Long / Round at 0.5 | heels-up 72% / bear 35%, heels-up 33%, squat 31% / bear 73% | rolls 74-100% | 0.41-0.50 | 3.5-4.4 s | more supports, slower ✓ |
| Small / Round at 0.3 | heels-up 100% / bear 93% | rolls 100% | 0.68 / 0.78 | 6.5 / 8.1 s | older adults take 6-13 s ✓; frail elders go on all fours ✓ (Round) |
| Toddler-like (head ×1.3, legs ×0.85) | squat 62%, heels-up 23%, half-kneel 10% | rolls 100% | 0.22 | 2.6 s | toddlers roll over, then squat or bear ✓ |
| Giant (Long ×1.6, strength ∝ L³) | bear 47%, squat 27%, heels-up 20% | rolls 62%, Fury 30% | 0.31 | 4.8 s | a larger body is nearer its limit and keeps more supports ✓ |
| Long, arms ×0.65 | half-kneel 65%, squat 30%; no bear | Fury 82% | 0.31 | 3.9 s | short arms lose the hands-and-feet ways ✓ |
| Long, stiff ankles | half-kneel 59%, heels-up 41%; no flat squat | Fury 78% | 0.24 | 3.5 s | no heels-down squat (Kasuyama) ✓ |
| Round, stiff knees | bear 63%, half-kneel 37% | rolls 88% | 0.26 | 3.2 s | less knee range, hands and feet (Stutzenberger) ✓ |
| Heavy trunk (×1.5, bends ≤ 30°, 0.7) | bear 50%, heels-up 47% | rolls 100% | 0.35 | 3.4 s | a heavy trunk stays upright (Sibella) ✗ |
| Long, right leg 0.4 | squat 47%, bear 26%; half-kneel 18% with the left foot forward, 0% with the right | Fury 44%, always left foot forward | 0.29 | 3.6 s | lopsided, toward the able side ✓ |
| Long at 2 | squat 49%, half-kneel 35% | Fury 68% | 0.15 | 3.2 s | quick ✓, but no quicker than the CMU pace |

**Strength swept** (`sweep.py`). This uses Small's and Round's fitted stances, with poses held and r ∝ 1/S.

- **The odds move smoothly.** Small's bear goes 8% at 2, 13% at 1, 32% at 0.65, 67% at 0.5 and 94% at 0.42. Its
  half-kneel falls from 29% to nothing.
- **The time grows without a step:** 2.6, 2.9, 3.9, 6.2 (at 0.36), 9.3 (at 0.32) and 18.6 s (at 0.29). The last
  includes a 13 s rest. Over the same range, overlap falls from 0.49 to 0.01 and stagger rises from 0.12 to 0.99.
- **No route opens for Small at 0.27, or for Round at 0.29.** Such a body cannot get up alone, and the creator warns of
  it (Luis, November 3). Re-fitted, though, Small at 0.3 still rises (r 0.68), by the heels-up squat. So the warning
  must come from the fitted atlas.

**What matches the ladder** is marked ✓ above: more supports, longer stages and less overlap as strength falls; ways
closed by head weight, range, arm length and size, as in people; and a lopsided rise when one leg is weak.

**What does not match yet:**

- **The miners barely differ at Strength 1:** the strength rule again (§6).
- **The half-kneel**, the commonest adult way, takes about 20% at Strength 1 and vanishes as strength falls (§2.2,
  item 3), whereas people move toward it as they weaken.
- **The weak, flexible Small squats heels-up**, a stance that is hard to balance. Its choice between that squat and
  the bear swings on one untuned weight: Round at 0.3 bears 93%, but only 37% when the let-go penalty grows with
  strain.
- **The CMU's own way from the back** (sit up, then squat) takes at most 21%. Its reference time is the longest, and
  each route's time comes from a different person.
- **The heavy trunk does not stay upright,** and very strong bodies are capped at the CMU pace.

These are layer 4's targets: tune the weights over many made bodies until the ladder's shape holds, then show Luis the
sheets.

## 5. C# modules and how they feed layer 3 and the pictures

The runtime code would live in `Assets/_WonderGather/Scripts/Units/Rising/`, in namespace `WonderGather`, with no
`UnityEditor`. It is plain math with no physics, so it can be tested without a scene.

| Module | Does |
|---|---|
| `BodyFrame` | the body, from a made `OwnBody` or a `BodySpec` (body map §3.4): masses and centres as PhysX has them, anchors, contact shapes, capacities now, layer 0's ranges and the body's units. Angles are natural ones (elbow and knee: the bend as made minus `jointPosition`) |
| `StanceGoal` | one family as versioned text in `Resources`, like `RiseWays`: supports, bands, weight rule, ready supports, free limbs, mirroring, start pose and samples |
| `StanceFitter` | the fit: a capped Levenberg-Marquardt solver with exact Jacobians and warm starts (§2.1) |
| `ReserveLP` | a dense bounded simplex giving r, the worst joint, the forces, the hands' share and the let-go check |
| `StanceAtlas` | a per-body cache: each sample's pose, r, margins, let-go result, and where each support sits relative to the pelvis. It gives the creator a **rise reserve** (1 − the best route's r) and a warning when no route opens |
| `WayChooser` | reads how the body lies, scores the routes (§3.3), applies the laws (§3.4), draws (§3.5), and re-plans at each gate |
| `RisePlan` | the stages: goals placed where the body lies, the order of support changes, gate margins, tempo (a speed, not a clock), gathering, overlap, hand load, side and head lead, and the failure policy |
| `SeededDraw` | hashes the unit id and fall count, so a fall replays exactly |

**Layer 3 receives three things:**

- **Goals in task space:** where each support goes, where the weight should be, and how the trunk and head are turned.
- **A posture prior:** the fitted pose, as offsets from the body's own standing pose (`OwnRiser`'s `toStood`).
- **A gate per stage:** it moves on when the gate is met, never on a clock.

The present riser stays until the new one stands (Luis, November 3).

**Pictures** (test and bench code):

- **Atlas sheets:** each stance from four sides with the made body's primitives (`BodysOwnBench.Shape`), the supports
  marked and r and the margin printed.
- **Route strips:** the drawn route's stances, with times and every route's odds.
- **Sweep sheets:** one variable swept (strength, head mass, leg length, ankle range), with the odds and every setting
  plotted. These show that nothing switches, and they catch fits that jump.

**The evidence comes in order:**

1. The C# port must match the prototype's r and margins within 0.02, against a golden file from `res_miners2.json`.
2. The sheets go to the aesthetic judges, then to Luis.
3. Only then does layer 3 start.

## 6. Risks

1. **Quasi-static only.** r ignores acceleration. The swinging sit-up, the push off the hands from the bear and the
   rock forward are modelled only as a shortfall, and the 0.06 H limit is a guess.
2. **The strength rule hides the build** (§2.2). How strength scales is Open, and Luis's to decide; layers 1 and 2 only
   read it. The giant at L³ and "arms by thickness" show what other rules would do.
3. **Untuned weights.** The half-kneel and the weak heels-up squat do not yet follow the ladder (§4). Layer 4 tunes.
4. **Fits that jump.** A fit that fails, or flips between local minima for neighbouring bodies, would switch the way at
   a tiny change of build, which Luis ruled out (October 13). The defences are continuation, several starts, the
   relaxed-pose pull, and the sweep sheets as a test.
5. **Robotic or repetitive** (Luis's watch item). The defence is seeded variation, plus layer 3's targets re-worked
   from the actual state. Stills cannot show repetition, so several falls of one unit must be watched in a row.
6. **Level ground only.** The game's ground is not level. At plan time the fitter must take the ground plane under each
   support, warm-started from the atlas pose.
7. **The model is not the game.** Its friction is 0.6, its parts never touch each other (a thigh pressing on a calf),
   and it keeps the low physics centres. If layer 0 changes the stops or the centres, the atlas must be read again.
8. **Thin ground.**
   - Stances and passages are missing (§1.2), and a way people rarely use appears only once it is written.
   - The timing rests on one CMU adult whose hip height is assumed, a Froude rule of thumb, and route times taken from
     different people.
   - The port's cost is estimated, not measured.
