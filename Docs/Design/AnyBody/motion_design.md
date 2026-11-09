# Getting up for any body - layers 2 and 3: the way up, and what moves the body along it

Status: a design for Luis to agree before anything is built (November 9, 2026). Nothing here is Locked. Nothing was built
or run for it: the numbers are from the cited files, and every cost is an estimate until it is measured.

Code is cited on the work branch (`D:/Dev/WG`, `claude/wip-round` at `53f388d`). Paths are under
`Assets/_WonderGather/Scripts/Units/` unless they begin with `Tests/` (`Assets/_WonderGather/Tests/PlayMode/`) or `Docs/`;
the solver's files (`Stances.cs`, `StanceFitter.cs`, `Statics.cs`, `Targets.cs`) are in `Rising/AnyBody/Body/`.
It builds on layer 0 ([joint_model.md](joint_model.md)), layer 1 and the first form of layer 2
([stances_strategy.md](stances_strategy.md) §3), their critique ([critique.md](critique.md), F1-F13), the port's review
(`D:/Dev/WG/Docs/Technical/AnyBody/port/REVIEW.md`, F1-F11) and the judges' two rounds on the stances.

## 0. What binds it, and its shape

**Luis's decisions it keeps** (records in `Docs/Correspondence/`):

| When | Decision | What it asks of layers 2 and 3 |
|---|---|---|
| Oct 9 | the body's own, never posed | joints push on the world; a fitted pose is never played, nor tracked as the stage's pose |
| Oct 13 | nothing switches at a threshold; never dead until a timer | sizes and odds grow smoothly with what the body can do; stages begin on the body's state, never on a clock |
| Oct 14 | one body for everything | the same articulated body lies, gets up and stands; what is added for one goes to all in the end |
| Oct 22 | never the backup animation | more tries, rests and other ways; no posed rescue |
| Oct 30 | any body, at once; procedural; natural; every frame read | nothing per body; every constant in the body's own units; evidence at every rendered frame |
| Nov 3 | the direction; stances allowed provisionally (watch for robotic, repetitive); non-risers warned; time follows capacity; strength from muscle; slower with size | routes from the atlas; variation; the creator's warning from the atlas; tempo from strength and size |
| Nov 5 | free limbs rest | resting is the default for anything that bears nothing |
| Nov 9 | a free hand near a knee or thigh rests on it, sides not mirrored, with natural variability | rest targets on the legs; seeded asymmetry wherever a body would vary |

**The shape.**

- **Layer 1** (built): each body's stance atlas, every stance fitted to this body with its reserve S (the share of its
  strength its worst joint must use; above 1 it cannot hold it), its margins and its reserve once a support lets go.
- **Layer 2** (§1): at each fall it reads how the body lies, draws a way through the atlas by cost, sets the style from
  capacity and a seed, and makes the plan again at every gate. It never drives a joint.
- **Layer 3** (§2): each physics step (50 a second) it moves the body towards the plan's next stance by its own joints,
  within each joint's strength and speed. A support changes only when a gate on loads and margins opens.
- **The standing balance** (`OwnKeeper`) takes the body once it stands on both feet, as today.

**What exists, and what the new parts take from it.**

| What | Where | Kept, changed or replaced |
|---|---|---|
| The fall | `PhysicalFall.cs`: its own ranges `:40-42`, how long it lies `:99-104`, the tries `:778-814` | kept; it hands its pose to the own body as now (`ByItsOwn`, `:788-814`), but as soon as it is still (§1.1) |
| The own body made lying | `OwnBody.Make` `:637-899`: anatomical frames only for a body made standing (`:742`); drives `:769-782` (`forceLimit` = strength, no stops); `maxAngularVelocity` 20 `:751`; the pairs that strike `:587-595`; leaning touches `:806-807`; toes `:809-835` | made with layer 0's frames behind the new switch (§2.1) |
| Layer 0 | `JointFrames.cs`: ranges `:27-41`, soft clamp `:111-126`, engine box `:158-172`, `JointAim.Target` `:193-198` (clamped, not slewed); `OwnBody.Stops` `:414-425`, `WriteMade` `:431-447`, `SpeedOf` `:215`; `Tests/ArticulationFactsTests.cs`, 9 of 9 | used; the slew, the toned damping floor and the limits' tightening are added (§2.3, §2.9) |
| The present riser | `OwnRiser.cs`: rows of poses per way (`:30-108`; `Resources/RiseWays/Small.txt`), gates on pose times (`:808-834`), balance on hands, knees and feet (`Balance` `:1379-1497`, `Shares` `:1336-1373`), the weight off a limb before it lifts (`ShiftedOff` `:1201-1223`), what bears it held where it is (`Plant`, `HoldPlanted` `:1226-1332`) | **replaced** behind the new switch; its balance pieces are what layer 3 generalises (§2.2): they carry Small's present way to its hand-over, where 72% of those handed over stood (October 29) |
| The standing balance and its rise | `OwnKeeper.cs`: the legs' torques for the push wanted at the soles (`:1047-1086`, by `AddTorque`, each clamped alone), the rise paced by the legs' effort (`:645-689`), `Soften` `:220-229` | kept as it is (Luis likes the step); it takes the body at the hand-over (§2.10) |
| The hand-over and after | `OwnBody.OnItsFeet` `:603-622`, `Remake` `:626-633`, `GoDown` `:1268-1303` | kept; the gate before the hand-over changes (§2.10) |
| Tiredness | `PhysicalBody.cs:53-116` (`Worked` `:71`); the own body never reports its work (body_map §1.5) | the own body reports its effort (§1.6) |
| The solver | `Rising/AnyBody`: `StanceLibrary` (`Body/Stances.cs:528-579`), `FitOptions` (`Body/StanceFitter.cs:9-44`; free limbs resting `:33`), `Statics.Build`, `ReleaseKinds`, `Report` (`Body/Statics.cs:22-150`), `ReserveSolver`, `FeasibleRegion` (`Reserve/`) | layer 2's numbers. Fits in the editor take 37 to 503 ms, median 120 (117 fits, `D:/Dev/WG/Docs/Technical/AnyBody/results/atlas_2026-11-09_made.csv`) |
| The bridge and the drawing | `Tests/BodyFrame.cs` (a miner as the solver's body; made bodies `:24-48`); `Tests/StanceAtlasDrawnCapture.cs` (the frame map, `AtlasPoser` `:287-397`; palms laid on the ground, `AtlasHands` `:241-269`) | moved into the game (§2.1, §2.8); `ReadPose` (INTEGRATION §2.4) written |
| Touches | `GroundTouch.cs:23-34`: the upward impulse and where; getting up, ground only (`:26`) | used; a touch for a hand on a thigh added (§2.7) |
| Evidence | `Tests/FrameRecorder.cs` (every rendered frame), `Tests/GettingUpByItsOwnTests.cs` (the game), `Tests/OwnRiseBench.cs` (the bench) | used; the recorder's version 2 (joint_model §6) |

## 1. Layer 2: the way up

### 1.1 How it reads how it lies

The fall ends as it does now: the let-go body has been still for half a second (`PhysicalFall.cs:58, 964`). What
changes, behind the new switch, is that the own body is made then, not after the lying time: the lying becomes the time
its tone takes to come back (§2.7). As it is made, layer 2 reads, in the body's own units:

1. **Its pose**, as the solver's 34 numbers: `ReadPose`, the frame map's inverse (INTEGRATION §2.4; to be written, with a
   round trip on 100 random poses to 1e-6, check C12).
2. **Which way it faces**: the trunk's front against down, from -1 (on its back) to 1 (on its front)
   (`OwnRiser.Measure`, `:334`), how far it is onto a side, and which side is uppermost (`OwnRiser.Choose`, `:455`).
3. **What bears it now**: each part's touch (forearms, thighs, shins, boots, toes: `OwnBody.cs:804-835`), and the ground
   under each part that may become a support, by a ray as `OwnRiser.GroundAt` (`:326`), so the slope under it is known.
4. **What it has now**: each joint's capacity times its muscle group's fresh share (`PhysicalBody.Fresh`, `:67`), and how
   the fall left it: shaken, dazed, out of breath (`PhysicalFall.cs:95-97`).
5. **Limbs left outside a living range**: the let-go body allows more than layer 0 (its shoulder 95 degrees aside and 70
   of twist, `PhysicalFall.cs:41`; critique F6), so the first moves bring them in.

From these comes the **entry**: which passage leaves the ground. On its front it pushes up; on its back it sits up or
rolls; on a side it rolls. The facing gives each passage a weight, not a line: a body half on its side scores both
ways, and the draw decides (§1.3).

### 1.2 The routes: a graph of this body's atlas, not a list per body

Built once per body design, on a worker thread, and kept under the body's numbers and the library's version
(REVIEW F10d):

- **Nodes** are the atlas's stances at their sampled values (strategy §1.2). Each carries S, the base margin, how far
  the weight may wander (`FeasibleRegion`), and, for each support it may let go (`Statics.ReleaseKinds`, `:81-100`),
  S and the margin without it.
- **Edges** are generated (strategy §1.3): two nodes join when their supports differ by one change (a support, or a
  pair, placed or lifted). Each edge also gets its S along the way (at its start; with the moving support gone, at the
  start and at the end, `Statics.Build` with `keep`, `:22-31`; and at its end), whether the moving limb reaches its
  place without passing through the body, and each joint's travel (for the speed rule, §2.6).
- **Three named passages leave the ground**: pushing up from the front (the hands come under the shoulders, then the
  chest lifts; onto the forearms first for the weak), sitting up (the head first, the trunk curling after it: Fury),
  and the roll onto the front (the head and the near shoulder leading, the far knee swinging over: the armour). They
  are the only transitions written by hand.

A body's ways are the paths its own atlas holds. A stance it cannot hold is closed to it; so is a let-go whose reserve
is beyond its strength. Nothing names a body.

**The first build's two routes from the front** (§3):

- **Through one knee** (Fury, the armour, the physiotherapists): pushed up; hands and knees; one knee lifted (the
  tripod, its hips higher so the leg has room to come forward: a lesson of October 14); the foot forward, hands still
  down (the crouch-kneel); a hand to the front thigh; rising over the front foot (h 0.55, 0.7, 0.85); the back foot
  alongside; standing.
- **Through the squat** (CMU 140_01, 140_02): pushed up; hands and knees; toes tucked; the knees off, the hands down
  (the squat with hands, its hips at 0.4 of their standing height or higher at first: §4.1 item 11); the hands off;
  standing.

The bear (hands and feet) waits until its stance is mended: drawn, it reads as a pike with the head buried (judges
round 2, item 1). Every stance of the two routes is a family of the library (`Stances.cs:77-522`), and most are already
fitted in its batch, with the left foot forward (`:558-579`); the right is the same family, mirrored. The reserve they
ask of the bodies on the sheets (`atlas_2026-11-09_made.csv`; S, resting; * a poor fit, cost 0.5 or more):

| Body | Hands and knees | Half-kneel | Rising from it (h 0.7, no hands) | Squat (h 0.35, flat) |
|---|---|---|---|---|
| Small | 0.19 | 0.07 | 0.27 | 0.16 |
| Long | 0.19 | 0.04 | 0.22 | 0.18 |
| Round | 0.24 | 0.11 | 0.25 | 0.18 |
| Long x1.25 | 0.19 | 0.09 | 0.42* | 0.31* |
| Small at strength 0.5 | 0.34 | 0.13 | 0.56 | 0.29 |
| Round at strength 0.3 | 0.64 | 0.30 | 0.88* | 0.45* |

Rising over one foot is the hardest of these on every body (one leg's hip does it; a hand on the thigh carries only 7
to 8% of the weight: strategy §2.2 item 3), and the bigger and the weaker need much more of their strength for it.

### 1.3 The cost and the draw

The form of strategy §3.3, with u = S/(1 - S) (0 unloaded, 1 at half strength, without bound as S nears 1), k_H =
√(H/0.9) and H the standing hip height:

`C = T + ∫u² dt + Σ_let-go exp(-m/0.03H) + w_R·range + shortfall + switching + κ·failures`

Three changes, from the critique:

- **T comes from layer 3's own speed rule** (§2.6), not a separate tempo law: one rule for how fast a body can move
  (F11).
- **Nothing closes at a line.** A passage that needs momentum (the swinging sit-up) costs (shortfall/0.06H)^4, which
  grows smoothly with the build, instead of closing at 0.06H (F13).
- **Switching costs the moves already made.** Re-planned mid-way, a route that must undo moves pays for them (F13), so
  a body does not dither between two ways.

The route is **drawn, not picked**: P ∝ exp(-C/τ), τ about 0.4 s, from the fall's seed. Close costs give a mix (only a
quarter of young adults rise symmetrically: VanSant 1988), and as the build changes the odds move smoothly; no build
switches every fall at once. **Which side leads** is drawn the same way, from the two sides' costs (the side it lies on
and a weaker leg among them), the unit's habit and the seed (strategy §3.4): a body with one weak leg rises lopsided,
towards the able side.

**The settings inside a route** (where in each band a hand or foot goes, the hand's load on the thigh, how high the
hips are on hands and feet) are set by the laws of §1.4, then varied (§1.5). The chosen stages are fitted again at
those settings, from the atlas's fit (`FitOptions.Start`, `StanceFitter.cs:21`), on the worker thread while the body
lies (§2.11).

### 1.4 The style laws

From the references read frame by frame (`Docs/Research/2026-11-02_GettingUpFrameByFrame.md`), the research and the
judges:

| # | Law | From | In the plan | In layer 3 |
|---|---|---|---|---|
| 1 | The head leads | Fury, 0.25 s before anything else; the armour's roll | each stage's head goal begins 0.25 s·k_H ahead (±30%, seeded) | the neck's tone comes back first; the head looks where the next support goes |
| 2 | Stages overlap | Fury turns while still sitting up; the armour's foot comes forward as it reaches hands and knees | the next stage's trunk, head and free-limb goals blend in before its gate, by 0.5/(1 + u²) of a stage | supports never overlap: one change at a time |
| 3 | A hand goes down before the weight goes onto it | Fury at 0:24.5; every reference | an edge places, then loads | placed, loading, bearing; unloading, lifted (§2.4) |
| 4 | The last push is over a planted foot, leaning forward | Fury, the armour, the physiotherapists; judges round 2, item 4 | the rise's weight goal is over the front sole (or the middle of both), the trunk's lean the free variable that brings it there | both legs straighten together; the trunk comes up after the legs (with the trunk after 0.4 of the rise, 79% of hand-overs stood, against 54% all at once: `OwnRiser.cs:1180-1184`) |
| 5 | The able take 3 to 4 s; weakness adds stages and tries, not slowness alone | CMU 2.95 to 4.70 s; the weak adult; the foal | the strain and let-go costs make weak bodies keep more of themselves on the ground | the speed rule (§2.6) |
| 6 | The weak gather before a push, not after reaching a stance | the foal | before each push a gathering that grows with its u (nothing to see for the able) | the weight rocks towards the push; the push begins at a forward turn once the reserve is there (§2.5), never after a set time (F10) |
| 7 | No inert arms | judges round 2, items 3 to 5 | every arm has a role in every stage: it bears, reaches, rests on a leg, counterweighs, or pushes on a thigh | §2.7 |
| 8 | The head lifted, never limp | judges round 2, item 3 (sitting up) | the head's goal from where it looks | the neck always holds the head against its own weight (§2.7) |
| 9 | A free hand near a knee or thigh rests on it, the sides not mirrored | Luis, November 9 | a rest target for each free hand, drawn per side | §2.7 |
| 10 | Free limbs rest | Luis, November 5 | the fits' resting pull (`FitOptions.RelaxFreeLimbs`, on) | low tone, damped: they swing with the body and settle |
| 11 | Nothing goofy: the limbs that bear are firm, and every try is a sensible one | the frame-by-frame study, item 6 | every route is one the atlas holds | bearing limbs at working tone; free limbs damped, never flailing |

### 1.5 Natural variation, seeded

Luis, November 9: "some level of natural variability that is permitted to sell the reality and life like aesthetic of
it". Variation comes from two places, in this order (critique F4):

1. **The state.** Which hand is nearest the ground, which knee is already bent, the side it lies on, how it fell. Every
   fall begins differently, and every goal is placed from where the body really is.
2. **Seeds.** A unit's seed gives its habits for life; a fall's seed, hash(unit, fall, try), gives the rest, so any fall
   can be replayed exactly.

| | What varies | How much (at variation 1) |
|---|---|---|
| Per unit (habits) | the side it likes to lead with; its ease (ρ, §2.6); how it carries its head; which hand usually rests where; its usual place in each band | the side's odds N(0, 0.5) on the logit; ρ × N(1, 0.08); the neck ±4 degrees; places a quarter of each band |
| Per fall | the route; each placement inside its band, left and right drawn apart; the second side's goals lagging the first by 0 to 0.05 s·k_H; which hand moves first; the pace × logN(0, 0.1); where each resting hand goes on the leg | as strategy §3.5 |

- **Never mirrored.** The stances' symmetry terms (`Sym`, for example `Stances.cs:89, 110, 152, 178`) stay in the atlas's
  canonical fits. At plan time each side is drawn on its own, and the refit holds the drawn difference instead.
- **Within what a body would do.** Every draw stays inside its band and inside the region the weight may wander, and the
  spread narrows as the reserve thins (the spread times the region's depth now over an able body's). So a weak body
  varies less in where it puts its hands and more in its time and its tries, as the references do.
- **Never in capacity** (strategy §3.5): a strong unit never rises like a weak one.
- **Its size is Luis's to set** (Q3): one number, 1 by default, on a slider.
- **From the unit, not its name.** Today the body's life is seeded by its name (`OwnBody.cs:850`); a made unit needs an
  id (body_map §3.6).

Stills cannot show repetition (strategy §6 item 5): six falls of one unit are put side by side (§3.2, step 6).

### 1.6 Re-planning at each gate

At each gate (a support placed, loaded, unloaded or lifted; a stance reached) layer 3 reports what bears the body and
how much, where its weight and its capture point are within that, the most any joint gave for its strength since the
last gate, the muscle groups' fresh shares, and whether the stage failed. Layer 2 then:

1. **Reads the reserve now**, on the real pose: `ReadPose`, `Statics.Build` with the supports that bear, and
   `ReserveSolver.Solve` (S is unique: REVIEW F2; 0.3 to 1.4 ms in .NET, more in the editor's Mono).
2. **Scores the rest of the way, and the others, from here**, the switching cost included, and draws again from the
   same seed. A body doing better than planned goes on; one that is tiring turns, smoothly, towards more supports.
3. **Fits the next one or two stages again** from the state reached, on the worker, ready before their gates. If a fit
   is late, layer 3 uses the atlas's: the body never waits for a computation.

**Effort is reported**, which it is not today (body_map §1.5; critique F10): each muscle group's effort is the most any
of its joints gives for what it has, told through `PhysicalBody.Worked` (`:71`). Work then tires the body, rests restore
it, and a second try is weaker than the first, as in the references.

### 1.7 When it cannot

- **A stage that cannot be finished is caught early**: its gate's measure stops coming nearer while the joints it needs
  are at their limit, for 0.5 s·k_H. This is a test of failure, not a clock for the movement. (Today a try is given up
  once the body has been slow in a pose for 3 s, `OwnRiser.cs:752`, and a limb is lifted "all the same" after 2.5 s,
  `:1216-1218`: both go.)
- **It goes back to the last steady stance**: the moving limb back where it was, the weight back over it. It **rests
  there** until its fresh shares have come back far enough for that edge (a gate on recovery, with breath and small
  shifts, never a frozen hold), and the plan is made again with κ about 1.5 s added to that edge for this fall. After
  one to three failures it takes a way with more supports, or the other leg leads, with no counter.
- **If it goes down** (its hips low again, or tumbling: `OwnRiser.cs:833-834`, `OwnKeeper.cs:607-613`), the fall takes it
  as now (`OwnBody.GoDown`, `:1268-1303`); it lies a little longer at each try (`PhysicalFall.cs:779`) and tries again
  from where it lies.
- **Never a played animation.** In the first build the old way stays only where it is today, after 12 tries
  (`PhysicalFall.cs:778`; Luis, November 3, item 6), counted and reported; the aim is that it never comes.
- **A body that cannot get up at all** (no route under S 1): it goes as far as its atlas holds, rests, tries again.
  What it does in the end is Luis's (Q2).
- **The creator's warning** comes from the atlas (the rise reserve, 1 minus the best route's highest S: strategy §5)
  and the standing balance's own limits (F13). Printed by the bench now; shown in the creator later (S2).

### 1.8 What layer 3 is given

A plan of stages. Each stage gives:

- **Supports**: which bear, which one changes, and its place in the frame of the supports that stay (and the way the
  body lies along the ground); the path it travels, its lift in the limb's own length.
- **The weight's place**: the centre of mass as the fit has it relative to its supports, and the margin it must keep,
  m = 0.03 H·(1 + u), before a support lifts.
- **Trunk and head**: their turns relative to the supports' frame; where the head looks, and its lead.
- **A posture**: the fitted pose as joint turns (through the frame map, `AtlasPoser`, `Tests/StanceAtlasDrawnCapture.cs:329-335`,
  and `JointAim`), held weakly. It decides only what the tasks leave free (which way an elbow points, how a free arm
  hangs); it is never the stage's pose to be reached (critique F4).
- **The style**: ease, overlap, head lead, gathering, each arm's role, each resting hand's place.
- **Its gates**, as numbers in the body's own units; never a time.

## 2. Layer 3: the controller

It is given the plan of §1.8, and nothing else asks its joints anything while it has the body.

### 2.1 The body it drives

- **Made lying with layer 0's frames.** Today `Make` gives them only to a body made standing (`OwnBody.cs:742`). Behind
  the new switch the lying body gets them too, the frames kept per body (a field set at make, not the static: F6), so
  the present riser keeps the body it was made on.
- **Its pose written as its joints' positions before the first step** (`WriteMade`, `:431-447`; tested: no snap), its
  targets as made (`Stops`, `:419-423`).
- **Its limits**: the engine's box a little beyond each range, widened to take in how the fall left it
  (`JointFrame.Box`, `JointFrames.cs:158-172`). New: tightened back towards the table as the joint comes in, at most 30
  degrees a second and never inside the pose (joint_model §3 rule 4).
- **A forearm strikes a thigh**, for the resting hand (today it strikes only the trunk and the head: `OwnBody.cs:587-595`;
  INTEGRATION C3), with a touch that reads it (`GroundTouch` ignores moving parts when getting up: `:26`).
- **The bridge in the game**: `BodyFrame` (in the tests today, `Tests/BodyFrame.cs:9`) moved into the runtime with
  `ReadPose`. Game files name the solver's types as `AnyBody.X`, never with a `using` (INTEGRATION §1.4).

### 2.2 Each physics step

The present riser's balance pieces, generalised. They work, but their numbers were searched on Small and many are in
metres (body_map §4.2, §4.4); here every number is in the body's own units (its hip height, leg, arm and foot) and its
own time, √(H/g).

1. **Read** each part's place and speed, what each touch bore and where, the weight, where it is going and its capture
   point ξ = c + ċ/ω₀ (ω₀ = √(g/h)), and the outline of what bears it (as `OwnRiser.Outline`, `:970-990`).
2. **Each joint's goal at this point of the stage** (its progress s, §2.6): for what stays planted, the joints that
   keep it where it is from where the hips are going (as `Plant` and `HoldPlanted`, `:1226-1332`, with the riser's
   reach pieces `ArmFrom`, `LegFrom` and `AnkleFrom` and their folded-leg mend, `:527-666`); for the moving support,
   its place on its path; the trunk's and head's turns; the resting limbs' places; and the posture, blended from this
   stance's fit towards the next by s (the head and the free limbs ahead of it by the overlap), only where nothing
   else asks.
3. **Where the ground must push**: the hips' goal moves the weight to its place without overshoot (a spring and damper
   on the body's own time, with an integral for what the pose holds back, as `HoldPlanted` and `Balance` do,
   `:1268-1332, 1452-1474`); the push is kept inside the outline by its margin and shared among what bears it by the
   plan's loads and the even split (`Shares`, `:1336-1373`), within friction.
4. **What each joint must carry**: the turning of the weight of everything beyond it (from each part's own mass and
   centre as the engine has them, `ArticulationBody.mass` and `worldCenterOfMass`) less the turning of the ground's
   pushes on the parts beyond it. That is the sum the reserve LP solves (`StaticsProblem`'s torque map), so what the plan
   trusts is what the body is asked for. Not the engine's `GetJointGravityForces`: it was tested on a body hung from a
   fixed root, and on a body whose root floats, as the getting-up body's hips do (`OwnBody.cs:753`), PhysX 4.1's
   source lets the whole body fall freely and asks its joints for nothing (`DyFeatherstoneInverseDynamic.cpp:415-480,
   208-209`, in joint_model's saved sources). A fact for step 0 to test.
5. **The goal and the load together, within what it has**, through one drive writer (§2.3). Nothing through
   `AddTorque`.
6. **Gates** (§2.4), with a report to layer 2 at each.
7. **Watch** (§2.9), and the effort reported (§1.6).

### 2.3 One budget per joint: strength and speed through the drive

The engine's facts (`Tests/ArticulationFactsTests.cs`, 9 of 9): a drive's `forceLimit` is a torque in N m; the drive is
solved implicitly, stable at any stiffness; `maxJointVelocity` does not bound a drive's own motion; target speeds are in
degrees a second.

Today strength is spent twice. The keeper's leg torques go through `AddTorque` beside the drive (`OwnKeeper.cs:1075-1086`),
up to twice a joint's strength; the riser's through `AddTorque` within what its spring leaves (`OwnRiser.cs:1490-1491`);
and a ball joint's three limits allow √3 times its strength on a diagonal (critique F1). The new controller has **one
budget**, all of it through the drive:

- each step a joint's target is its goal (§2.2, step 2) plus τ/k for the load it carries (step 4): the drive then
  gives k·(goal − now) + τ − c·speed, the spring and the weight it carries in one torque, within one limit. The target
  is kept inside the soft range, never more than eFull from the joint, and never moving faster than its top speed.
  (That slew is what joint_model §4 asks; `JointAim.Target`, `JointFrames.cs:193-198`, does not do it yet.) The one
  place a target may go past the soft range: a straight elbow or knee that bears (the weak on straight arms, critique
  F8) rests on its stop with a little push into it; whether the engine holds that without chatter is tested at step 2.
- k = tone·τcap/eFull, eFull 35 degrees (the riser's `FullAt`, `OwnRiser.cs:115`), so a full-strength target is never
  more than eFull ahead;
- damping c = max(ζ·2√(k·I), tone·τcap/ωmax): the torque falls as the joint turns faster, to nothing at its top speed.
  That is the muscle's law, inside the engine's own solve. It is toned (critique F9), so a slack limb swings instead of
  freezing like a doll's;
- each axis's `forceLimit` is what the joint has now at this speed, shared over the axes in τ's direction, so the whole
  stays within one sphere, the LP's own shape (F1);
- ωmax from `OwnBody.SpeedOf` (`:215`: slower with size, as √(0.6/H)); τcap = strength times the fresh share, read each
  step, not only at make.

The weak and the big are slower by the same law that makes them weaker, and nothing outside the joints moves the body.
The keeper keeps its own way until it is folded into this one (§2.10; §4.1 item 2).

### 2.4 Supports: placed, loaded, unloaded, lifted

Each support (a hand, a forearm, a knee or a shin, a sole, a ball of the foot and its toes, the seat, a hand on a thigh)
is free, travelling, placed, loading, bearing or unloading. It moves on only through a gate:

| From, to | Gate (no clock; numbers in the body's units) |
|---|---|
| bearing, to unloading | the plan's next change is this support, and the head's lead has begun |
| unloading, to lifted | it bears 0.15 or less of what it bore (`BalanceLifts`, `OwnRiser.cs:156`); the weight and its capture point are inside what stays by m = 0.03 H·(1 + u); and its reserve without this support, corrected by the effort measured, is under 1 |
| travelling, to placed | its touch bears at least 0.02 of the weight (`BalanceBorne`, `:152`); over the last part of its path it comes down slowly until it does |
| placed, to bearing | its share of the push grows as the weight comes over it, at the stage's pace (§2.6) |
| one stance, to the next | every support of the next stance bears, the weight is inside the region it may wander, the trunk inside its band |

**The weight goes before a lift.** The weight's place moves into what stays, to the nearest place deep enough (as
`Balance` does, `:1399-1425`); the support's share falls as it goes; and the integral (`BalanceHoldLearns`, `:174`) makes
it really arrive. Two lessons paid for (October 15) stay: without the integral the held weight sits short; and "what
stays" never counts the support that is leaving, whatever it still bears. If the gate never opens while the joints are at
their limit, that is a failure (§1.7), not a lift "all the same".

### 2.5 Moving the weight, gathering, the last push

- **The hands leave for good only when the capture point is over the feet** by the margin: on a course that ends
  standing (the robots' rule, Zambella 2023; strategy §3.4, "hands kept"). Weak bodies keep their hands down longer, or
  put them on their knees.
- **Gathering** grows with the strain of the push ahead: nothing to see for the able, more for the weak. Before the
  push the weight rocks towards it and back, at the body's own sway time, each rock a little further. The push begins
  at a forward turn of a rock once the joints it needs have their fresh shares back far enough for it, so how many rocks
  a body makes follows from the body. The rock gives the push the momentum the still plan lacks (strategy §6 item 1),
  and the breath deepens with the effort (`OwnBody.cs:1052-1064`).
- **The last push**: the trunk leans until the weight is over the front sole (from the half-kneel) or between the soles
  (from the squat); the arms go forward as far as the weight's shortfall asks; a hand pushes on the front thigh where
  the plan gives it a load; both legs straighten together, and the trunk comes up after them (§1.4, law 4).

### 2.6 Tempo from capacity: one speed rule

A stage's progress s goes on at the pace its joints allow:

`pace = ρ · min over the joints that move of (ωmax·(1 - S_j)) / (that joint's turn per unit of s)`

S_j is the share of its strength the joint needs now, read from its drive's force (as the keeper's rise reads its legs':
`OwnKeeper.cs:648-664`). ρ, a body's ease (how much of what it could do it uses when not hurried), is the one setting
layer 4 tunes: about 0.15 (an estimate, from Small's and Long's knees and hips in the last push) brings an able body to
the CMU pace (the last push 0.4 to 0.55 s; 2.95 to 3.41 s from lying on the front; times k_H). Then:

- **the weak are slower**: as S_j nears 1 the pace falls towards nothing, smoothly, with no line;
- **the big are slower**: ωmax goes as √(0.6/H), so their times grow as √H, as the gait's already do (Froude);
- **tiredness slows**: S_j is S over the fresh share;
- **the plan's time T** (§1.3) comes from the same rule: one speed rule (F11).

The gates still decide when a support changes: s cannot pass a gate that has not opened.

### 2.7 Free limbs, resting hands, breath, the head, and lying

- **Every arm has a role in every stage** (§1.4, law 7): it bears; it reaches towards where it will be put (moving with
  the head's lead); it rests; it counterweighs; or it pushes on a thigh.
- **Resting** (Luis, November 5): the fit's resting pose (hanging a little out, the elbow a little bent, untwisted), at
  low tone with the toned damping, so it swings with the body and settles, as hung things do.
- **Resting hands** (Luis, November 9): a free hand within reach of a knee or a thigh (squatting, kneeling, rising) goes
  to it. Its place on the leg is drawn per fall inside the band the solver already has for a hand on a thigh (the wrist
  0.6 to 0.97 of the way from the hip to the knee: `Body/Targets.cs:178-203`; `Stances.cs:262-271, 347`), the two sides
  apart: one on the knee and one on the thigh, or one resting and one hanging. The arm's joints are asked for what puts
  the hand there (as `ArmFrom`, `OwnRiser.cs:644`). It bears nothing unless the plan gives it a load; then it is a
  support (§2.4). The forearm must strike the thigh for this (§2.1), or the hand would sink into the leg.
- **Breath**: on the trunk as today (`OwnRiser.cs:852`, from `OwnBody.cs:1071`) and on the shoulders, deeper with
  effort; gentle when little has been done (Luis, October 10).
- **The head**: its goal is where it looks (the ground where the next support goes, then ahead as it rises), leading
  each stage. The neck always carries the head's own weight (§2.2, step 4) at working tone, so it never hangs. Lying,
  it rests on the ground, and it is the first thing to lift.
- **Lying**: made where it fell, the body lies at the fall's slack tone (`PhysicalFall`'s `AtRest` 0.05, `:47`). Its tone
  comes back over the time the fall gives it (shaken, dazed, out of breath: `:99-104`), the neck first. The first stage
  begins when its tone is enough for its first edge. So a dazed or a weak body lies longer, and no body lies dead and then
  moves (Luis, October 13).

### 2.8 The hand on the ground

The physics body has no hand: the forearm's capsule runs on by an arm's radius, and its round end is what touches
(`OwnBody.cs:706-708`; `OwnRiser.HandEnd`, `:636`). The first build keeps it so and draws the palm on the ground: the
sheets' drawn wrist (`AtlasHands`, `Tests/StanceAtlasDrawnCapture.cs:241-269`; hands within about 2 cm of the ground)
moves into the game's drawing after `MinerBody` (execution order above 50). It turns only while the hand bears, eased in
and out with the load so it never snaps; a free hand hangs as it is drawn today. A hand of its own (a small part on a
two-way wrist) is a later step, if the capsule's rolling or sliding shows (§4.1 item 6).

### 2.9 Nothing whips, nothing breaks

| Seen before | Its guard here |
|---|---|
| no stops: elbows bent backwards, knees past 180 degrees (October 30) | anatomical ranges: a soft clamp and a hard backstop (§2.1) |
| stops added to the old riser: chattering, up after 1 to 3 pushes of 8 (November 2) | targets never rest at a stop (inside the soft range); limits tightened gently |
| boots at 115 to 154 rad/s; once a body blew apart (October 30) | the toned damping floor and the slew (the drive's own free speed at most ωmax: joint_model §5); a boot unloaded before it lifts, so no error is stored in it; anatomical zeros, so no flips |
| a stiff spring on the light boot does not hold at the game's step (`OwnKeeper.cs:37-40`) | tested first (`StiffAnkleHolds`, F5); the ankles' and toes' inertia cap kept until it passes |
| torques beside the drive (F1); drives written in four places (F2) | one budget, one writer |
| drawn limbs flicking for a frame (October 30) | drawn from the parts' own turns (`MinerBody.TurnsAsSolved`); every rendered frame recorded and scanned |
| `maxAngularVelocity` 20 on every link (`OwnBody.cs:751`) is a live drag (F3) | left as it is in this step |

`BodyWatch` (joint_model §6) runs on the new body at every step: range, whip, flip, target, torque, chatter, apart. The
tests assert none; the present body is recorded, not asserted (F12). The recorder's version 2 adds each joint's
anatomical turn, its target and its drive's force.

### 2.10 The hand-over to the standing balance

Both first-build routes end with both soles bearing: from the half-kneel the back foot comes alongside as the body
rises, the route's last support change, made by layer 3 (the keeper is a balance on two flat soles, and a back foot on
its toes is not that). The gate is the keeper's own envelope, so that it can take the body: both soles bear 0.85 of the
weight (`OwnKeeper.RiseOnFeet`, `:193`), the push it wants lies within the soles (less than 1 cm outside, `:653`), and
the hips are within the keeper's lean (`DownLeaning` 35 degrees, `:26`). Then `OnItsFeet` as now (`OwnBody.cs:603-622`),
with two changes. The hand-over comes during the push, not after a still crouch (the judges of October saw the crouch
held before the rise), and the keeper's rise goes on from there at once, paced by its legs (`RiseSettles` 0,
`OwnKeeper.cs:193`; `:645-689`). And the drives go from this controller's settings to the keeper's over a few steps, not
at once (today `Soften`, `OwnBody.cs:613`). Risen, `Remake` (`:626-633`) makes it the standing body Luis knows, its
catching step on as in the game (`MinerWorkPreview.cs:640`).

Later (one body for everything, October 14): the keeper's two-sole balance is the law of §2.2 with two supports. The two
become one controller, and the keeper's settings are searched again on it. Not in the first build.

### 2.11 What it costs in the frame

| Where | What | Estimate, to be measured |
|---|---|---|
| main thread, each physics step, each body getting up | §2.2: 15 parts, at most 16 touch points, a 3-by-3 solve, sums over the joints | tens of microseconds, about what the present riser costs; no garbage |
| the engine | a 15-part articulation with its touches | as the present getting up |
| worker thread, once per body design | the first build's 20 or so stances (both sides) and their edges | about 2.5 s in the editor (fits at a median 120 ms, at most 503), kept under the body's numbers |
| worker, each fall | the route's stages fitted again at their settings; the LPs along its edges | 0.5 to 1 s, while it lies (it lies 0.8 s at least, `PhysicalFall.cs:74`) |
| worker, each gate | one LP on the real pose (1 to 5 ms), one or two fits | ready before the next gate; if not, the atlas's fit |

Before any per-gate call the solver's calls reuse their buffers (REVIEW F3: `Statics.Build` makes 16 to 20 kB, a fit
about 640 kB). One `ReserveSolver` per thread; `Margin.RoundHull` never touched while fits run (REVIEW F8). Measured with
1, 8, 16 and 32 bodies down at once.

## 3. The first build

### 3.1 What Luis plays

A new switch on the panel beside the present one: **"Gets up by its own body: the new way (from its front; any
miner)"**. On, any miner that falls onto its front gets up by layers 2 and 3. Fallen onto its back or a side it gets up
as before (Small's present way if that switch is on, otherwise the old way) until the second build. The present switch
and its way stay as they are (Luis, November 3, item 6). The panel's strength (0.3 to 3) shows what capacity does: lowered, a miner
keeps more of itself on the ground, gathers, rests and takes longer.

The two routes from the front (§1.2), drawn by cost, either leg leading. Small, Long and Round in the game; on the bench
also the ten made bodies of the sheets (`BodyFrame.MadeBodies`, `Tests/BodyFrame.cs:24-48`), made as game bodies by the
body maker of body_map §3.4 (primitives, no model), and the three at strengths 0.5, 1 and 2. What it costs in the frame
is §2.11, measured at step 6.

### 3.2 The steps, each with its test

Every count in courses, at six or more places on the meadow (the bench's floor is level; the game's is not), both sides
leading.

| # | Step | Test |
|---|---|---|
| 0 | layer 0 on the lying body (§2.1), the one drive writer (§2.3), `BodyWatch`, the recorder's version 2 | made lying from real falls: no snap; lying limp 20 s with no fault, 30 courses × 3 miners; `StiffAnkleHolds` (F5); what `GetJointGravityForces` gives on a body whose root floats (§2.2, step 4) |
| 1 | `BodyFrame` in the game, `ReadPose`, a fit as joint turns | the round trip, 100 random poses to 1e-6; a fit written as joint positions puts every part within 1 mm of the solver's |
| 2 | holding one stance (§2.2) | hands and knees, the crouch-kneel, the squat with hands, the half-kneel: each held 20 s, 3 miners at strengths 0.5, 1 and 2, no fault; the most any joint gives within 0.1 of the atlas's S (the one budget checked against the plan) |
| 3 | one change at a time (§2.4) | each edge of the two routes alone, from the stance before it held: the gates open, nothing lifts while it bears, no fault |
| 4 | the plan (§1): the graph, the draw, the laws, the variation, re-planning, failures, resting hands | before any motion: the routes' odds for the 13 bodies, and sweeps of strength and size that change smoothly (strategy §5's sweep sheets), against the human ladder (strategy §4) |
| 5 | the whole getting up on the bench, from real falls; the body maker (body_map §3.4) for the made bodies | the maker's Small, Long and Round against the prefabs' first (body_map §3.4); then by kind of body: up at the first try, tries, time from the first stir; no fault |
| 6 | the game as played | pushes onto the front, every rendered frame at 60, 120 and 144 frames a second, scanned; pictures from four sides as faults happen; six falls of one unit side by side |
| 7 | judges, then Luis | three fresh judges and a referee (three rounds at most), asked also "does it read as posed?" and "does it repeat?"; then a build, its switch named in the report's first line |

### 3.3 The bars before Luis sees it

First bars; layer 4 tunes towards them.

- No frame with a joint more than 2 degrees outside its range, a part turning faster than 1.5 ωmax against its parent, a
  drawn bone more than 5 degrees off its part, or a part 2 cm into the ground; no `BodyWatch` fault.
- Small, Long and Round at strength 1: up at the first try in 80% of courses or more (Jones's bar, the proposal's
  target), in about 3 to 4 s from the first stir.
- Weak bodies: more supports and stages, rests, longer; up within three tries in most courses, or as far up as their
  atlas holds.
- Not one route for all three miners, and no two falls of one unit alike (the judges and the side-by-side strips).
- Of those handed over, 90% stand (72% today).

### 3.4 Not in the first build

From the back and the sides (the roll, sitting up); the bear and Gowers' hands on the knees; slopes beyond what the
feedback absorbs (the fits stay level: REVIEW F4); a hand of its own; one controller for standing and rising; the
creator's warning in the creator; one unit helping another; the coat and the hung things in the deep stances (judges
round 2, item 2).

## 4. Risks and open questions

### 4.1 Risks

1. **The plan is still, the body is not** (strategy §6 item 1). The push off the hands and the last push need momentum.
   Defences: capture-point gates, gathering rocks, the speed rule. A learned correction stays layer 5, only if needed.
2. **Two budgets meet at the hand-over.** Layer 3 spends each joint's strength once; the keeper still spends up to twice
   (F1). The hand-over ramps between them; measured at step 5. Folding the keeper in changes the step Luis likes, so it
   is its own later step, shown to him.
3. **The posed look** (critique F4). The posture is weak and the goals follow the real state; the judges are asked
   whether it reads as posed, against the present riser.
4. **The ankles and boots** (F2, F5). Step 0 tests the stiffness before anything relies on it; the boots are unloaded
   before they lift.
5. **The fall leaves limbs outside the range** (F6). Widened, then tightened gently; counted at step 0. If often, the
   fall's own ranges come to the same table: a change to the fall Luis has seen, so shown to him.
6. **The hand is a capsule end.** It may roll or slide, and the drawn palm hides up to about 2 cm; a hand of its own
   later, if it shows.
7. **The ground is not level**, the fits are (REVIEW F4). Placements by rays and the feedback carry small slopes; fits
   with the ground's plane under each support are a later step.
8. **Fits that jump** between neighbouring bodies, or between runtimes (strategy §6 item 4; REVIEW F7). The atlas is built
   on the runtime that uses it; the sweep sheets catch jumps.
9. **The costs are untuned.** The prototype's routes do not yet follow the human ladder everywhere (the half-kneel
   vanishes as strength falls; the heavy trunk does not stay upright: strategy §4). The first build reports the odds by
   kind of body; layer 4 tunes them.
10. **Weak arms cannot lock** (F8): the fits' comfort term keeps elbows and knees from straight, which raises S for arm-
    and leg-bearing stances. Made one-sided before weak bodies' routes are trusted.
11. **The deep squat sits at the knee's stop** (F7: soft 155, hard 162 degrees, the CMU squat 155 to 158; no thigh on
    calf). The squat route keeps h at 0.4 or more until the thigh can bear on the calf.
12. **Many bodies down at once** (F13; REVIEW F3). Fits on the worker, spread over frames; buffers reused; measured with
    32.
13. **Runs repeat courses.** Starts are varied (places, sides, seeds), and courses, not runs, are counted.
14. **The cost of the evidence.** A round of judges is about 400k tokens (November 4 and 5). The first build is eight
    steps, each finishable and reported on its own, so a limit met half-way loses nothing.

### 4.2 Not checked here

- No Unity run, no code: every cost in §2.11 is an estimate, and so is the ease ρ (§2.6), worked out by hand for Small's
  and Long's last push.
- The routes' odds quoted are the Python prototype's (strategy §4); the C# graph and draw do not exist yet.
- `GetJointGravityForces` on a floating root is read from the saved PhysX 4.1 source, not run (Unity 6000.6's PhysX
  version is not confirmed: joint_model R5).
- Whether the implicit drive gives goal-plus-load (§2.3) closely enough in the quick moments (it adds a damping of k
  times the step) is not worked out beyond that.
- The bars of §3.3 are first guesses.
- This page has had no independent check yet. As INTEGRATION.md had, a fresh agent should read it against the code at
  its lines before step 0 begins.

### 4.3 Open questions for Luis

Each with a recommendation; none is decided by building.

- **Q1. Which falls first?** The first build covers falls onto the front (the commonest and the simplest); onto the back
  and sides it gets up as now until the second build (the roll, then sitting up). *Recommended: front first.* The other
  choice, all falls in one build, would bring the first build to you later.
- **Q2. A body that cannot get up by itself** (the creator would warn): after a few tries, should it stay where it got
  to (sitting, or on hands and knees) and rest there, trying again now and then, until another unit can help (later)?
  *Recommended: yes, and never the old animation for it.* For bodies that can get up, the old way after 12 tries stays
  only as today's last guard, as you decided on November 3, counted, aiming at never.
- **Q3. How much natural variation?** *Recommended:* on by default at the size the references show (timing about a
  tenth, places within a quarter of each band, the sides never mirrored), with a slider on the panel for you to set by
  eye (0 none, 1 as the references, 2 more).
- **Q4. May the build open with the new way switched on**, the present way one click away? *Recommended: yes*, the
  switch named in the report's first line.
- **Q5. The three miners' strengths.** Today each keeps exactly what it had before strength from muscle, so from the
  front they will get up nearly alike, differing by their shape. Following their build instead (Long weaker for its
  height, Round stronger) would show more, but also changes their work and standing. *Recommended:* keep them as they
  are for the first build (only the getting up changes), and show you the difference after, side by side.

## 5. For Luis, in plain words

The plan for how a fallen miner gets itself up, for you to agree before I build it.

**The way up.** A fallen body reads how it lies and what it can do in each stance: how much of its strength each needs,
whether it can let a hand or knee go there, how tired it is. From that it picks a way up, through one knee or a squat,
easier ways more often, never one fixed way per body. Weaker or bigger bodies keep more of themselves on the ground,
rock before a hard push, rest, and take longer. How it fell, and a seed per miner and fall, give natural differences:
which leg leads, where the hands go, which hand rests on which knee; the sides never mirror. It chooses again at every
stage.

**The moving.** Each joint pushes with no more than its muscle has, less the faster it turns, so nothing whips or
breaks. A hand goes down before weight goes onto it; weight leaves a hand or knee before it lifts; the head leads; the
arms always have a job; the last push leans forward over a planted foot. Then your standing balance takes over.

**First build:** from lying on the front, behind a new switch beside today's, on Small, Long, Round and test bodies;
every frame checked, then judges, then you.

**Five questions** close the design: front first; a body that truly cannot rise; how much variation; opening switched
on; the miners' strengths.
