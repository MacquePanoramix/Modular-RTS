# Implementation plan — S3: weight and strength at the rock

**Proposed:** October 6, 2026, in the review Luis asked for
([Reviews/2026-10-06_SamePageReview.md](Reviews/2026-10-06_SamePageReview.md)).
**Approved by Luis:** October 6, 2026: "I like the order that you proposed.
So yeah, let's go with it", with more to fit in
([correspondence](Correspondence/2026-10-06_STABLE_BUT_ABLE_TO_FALL_AND_THE_INTERACTION_CLICK.md)).
**Baseline:** 246c11b on `claude/worker-showcase`. Unity 6000.6.0f1 / URP;
Blender 4.4/4.5.
**The design it builds:** [Design/ThePhysicalBody.md](Design/ThePhysicalBody.md).
**The plan before it** (S1: the Ordinary Place, two cameras, the miners) is
archived in
[Plans/S1_OrdinaryPlaceCamerasAndMiners.md](Plans/S1_OrdinaryPlaceCamerasAndMiners.md).

## Which of the prototype's questions it answers

The first of them: does a body with real strength, moving a tool with real
weight against a real rock, look true and not goofy? Carrying, equipment
and the creator are built on the answer.

## The order of stages (approved October 6)

| Order | Stage | What it gives |
|---|---|---|
| **Now** | **S3, weight and strength at the rock** (this plan) | A miner really lifting a pickaxe that has weight, with strength and tiredness that can be watched; a body that is very stable and can still fall; the interaction click; mining any boulder |
| Then | **S4, carrying and equipment** | No pickaxe, a pickaxe, a strap for the back, a backpack, a dragged sack, a cart; stones brought home; loads that change the walk; each of these done by the body, physically |
| Then | **S2, the creator** | The screen: the three appearances, strength with its gentle change of body, equipment, and what this worker will be able to do |
| Last | **S5, ready for players** | As in [ShowcaseRoadmap.md](ShowcaseRoadmap.md) |

## Luis's choices (October 6)

| ID | Question | Luis's answer |
|---|---|---|
| **A** | How far does the physics go? | **A mixture.** Very stable, never naturally wobbly, but able to trip or fall "in this, like, ragdoll-ish way if it's an extreme situation that calls for it". This replaces D1 |
| **B** | The lantern and the mug | **Hanging somewhere that makes physical sense,** from the clothes. Hands empty |
| **C** | The order of stages | **As proposed.** Where the new things fit is left to Claude |

**Added by Luis in the same message:**

- **Stamina:** tiredness of the muscles, short-term at least, already in
  this prototype.
- **The interaction click:** a key and a click on something show its
  options beside it, with a cancel.
- **Every action is physical:** equipping, unequipping, strapping, putting
  into a bag are all seen done by the body, with real weight.
- **Built to be built upon:** running and sword fights come later, on the
  same body.

## The approach

Everything a miner holds, carries, drags or pulls is a real body in the
physics, with its real weight and balance. The miner's arms, shoulders and
back move it only by pushing and pulling, and each working joint has a most
it can give, set by strength and lowered by tiredness. The legs keep the
planted walk while the body's weight is within its feet. The body leans,
braces and steps as its real weights require, and falls only when no step
can catch it. A swing or any other action is planned as intentions, and
what happens comes out of the weights and the limits.

Why this is both stable and able to fall, and how each part works:
[Design/ThePhysicalBody.md](Design/ThePhysicalBody.md).

## Steps

Where Luis's new things fit is marked **(new)**.

| Step | What | What Luis gets to see | State |
|---|---|---|---|
| 1 | **Weigh everything.** Each body part and each pickaxe gets its weight, its balance point and how hard it is to turn, measured from its own model and what it is made of | A table: what each miner and each pickaxe weighs | **Built, October 6** ([the table](Design/ThePhysicalBody.md#step-1-everything-weighed-october-6)). Small 40.7 kg, Long 58.5 kg, Round 87.6 kg; their pickaxes 1.50, 3.21 and 2.39 kg |
| 2 | **The bench.** One miner standing on plain ground, with a pickaxe that is a real body, and a strength that can be changed. A grid of strengths and pickaxe weights, each trying to lift the pickaxe to the shoulder and bring it down on a block. The two ways of building the arms are tried and compared | **Clips, before anything is built on it.** Also what one body costs, and whether it is steady at every frame rate | **Built, October 6. Luis: "very promising so far … we can just proceed with your plan as is"** ([message](Correspondence/2026-10-06_THE_BENCH_IS_PROMISING.md); [clips and figures](Design/ThePhysicalBody.md#step-2-the-bench-october-6)). One way of building the arms was built, not two; it is steady at every frame rate tried; one body's hands cost about 26 millionths of a second a step |
| 3 | **Free hands, and things that hang (B).** Both hands free and closing on all three miners. The lantern and the mug hang from the clothes by something real, as real bodies | The remade miners beside the present ones, before anything is replaced | |
| 4 | **The swing,** in its six parts ([review](Reviews/2026-10-06_SamePageReview.md#the-swing-in-six-parts)): set, lift, top, drive, strike, recover. The upper hand slides along the handle. The legs and the back work. The jolt of the strike goes into the arms | The three miners swinging, at several strengths | |
| 5 | **Stamina (new).** Short-term tiredness for the arms, the back and the legs. A tired miner lifts lower, drives slower, rests the head on the ground and waits | One miner worked until it tires, then resting | |
| 6 | **Balance: lean, brace and step (A).** The body's real weights decide how it stands: leaning against a load, bracing before an effort, stepping to catch itself | A miner with a pickaxe too heavy for it; a miner pulled | |
| 7 | **Holding and walking with the tool.** How it is held comes from strength: in one hand at its balance point; in two hands; the head on the ground and dragged; or not at all. Dragging slows the walk | The same miner at several strengths, walking | |
| 8 | **The interaction click and the small actions (new).** Space and a click open a thing's options beside it, with a cancel. The first actions, each done by the body: take the lantern or the mug in hand and hang it back; lay the pickaxe down and pick it up; rest | Each action, close up | |
| 9 | **Any boulder.** A click on any boulder of the Ordinary Place sends the miner to mine it. A place to stand is found from the rock's own shape and the ground. A spot to strike is found on its surface within this body's reach. The swing adapts to the spot. What is struck off falls as real stones and lies where it falls (bringing them home is S4) | Mining several different boulders | |
| 10 | **The fall, and getting up (A).** When no step can catch the body, or its legs cannot bear it, the whole body follows the physics, protects itself, lies, and gets up. The riskiest step | Clips of extreme cases, before it is trusted | |
| 11 | **In the Ordinary Place.** A plain panel: the strength slider, and a light, a middling and a heavy pickaxe. The look with `K` is retired | The build | |
| 12 | **Evidence** (below), the playtest guide, the documents | | |

**Why this order.**

- **Steps 1 and 2 first:** everything else stands on them, and they answer
  what is not yet known (steadiness, cost, how it reads at the miners'
  size).
- **Stamina (5) straight after the swing (4):** it is the same rule as
  strength, and it is seen best in repeated swings.
- **Lean, brace and step (6) before the fall (10):** they are what makes
  the body "very stable". The fall is the last rung of the same ladder, and
  the hardest to make believable, so it comes when the rest is steady.
- **The interaction click (8) before any boulder (9):** laying the pickaxe
  down and picking it up are how a miner comes to hold one at all, now that
  it no longer appears from nowhere.
- **Strapping, bags, the sack and the cart** are S4's, built from the same
  small actions as step 8.

## Evidence

- **Tests:**
  - a heavier pickaxe is lifted lower and arrives slower, on the same body;
  - a stronger body lifts the same pickaxe higher and it arrives faster;
  - a body too weak for its pickaxe does not strike, and nothing is mined;
  - a tired body is weaker, and recovers with rest;
  - the head's speed and energy at each strike are measured and shown;
  - the pickaxe never passes through its bearer, at any strength;
  - the hands stay on the handle, and let go only when the plan says so or
    their hold is overcome;
  - a body within its balance does not step or fall; a body pulled hard
    enough steps; pulled harder, it falls, and gets up;
  - nothing appears in a hand or vanishes from one: every object is at
    every moment held, hanging, or lying;
  - only a real touch of the head on the rock yields anything (the rule of
    the strike, unchanged);
  - any boulder of the place can be mined, by each of the three bodies;
  - the old test body and its maps work as before.
- **The model quality method:** the audit in motion, run across a range of
  strengths, since the motion is no longer the same every time.
- **Cost:** one miner at work, and a crowd, measured in a release build
  against today's figures.
- **Pictures and clips** at every step, read before any number is believed.
- **A build** and a playtest guide.

## Contracts to keep

- **The rule of the strike.**
- **The walk:** planted feet, arrival without shuffling, each body's own
  gait, while the body is within its balance. Loads and the ladder of
  balance may change it; nothing else does.
- **The test body and its maps** keep working as they do.
- **The looks Luis has liked** are shown beside the new before anything is
  replaced.
- **Shown early.** Each step's clips go to Luis before the next is built on
  it, the bench and the fall above all.
- **The project's rule on physics bodies.** AGENTS.md says not to introduce
  active ragdolls "without a corresponding milestone request". Luis's
  messages of October 6 are that request: bodies answer to real physics, and
  can fall "ragdoll-ish" in extreme situations.
- **Protected files.** Luis's uncommitted files (TheGroup.unity, two
  ProjectSettings files, `_Recovery`) are never edited, committed or
  discarded.
- **Third-party material** needs Luis's approval. Everything here is
  planned to be made from scratch.

## Left open, not decided here

- **The interaction key.** Space is proposed
  ([why](Design/ThePhysicalBody.md#the-key-the-space-bar-recommended)); it
  is one setting, for Luis to try.
- **The list of interaction options.** Luis's examples and a proposed list
  are in [the design](Design/ThePhysicalBody.md#options-for-the-prototype-a-proposal-luis-asked-what-else-makes-sense).
- **Stamina's pace,** whether there is a longer-term tiredness, and what
  the player is shown of it.
- **Strength's scale:** what the numbers on the slider are, and how the
  body's appearance changes with them (the creator, S2).
- **Whether a rock visibly wears away** as it is mined.
- **Whether how well it is struck changes how much is mined** (D6). S3
  measures every strike; the amount stays fixed until Luis decides.
- **How Long stands in the game** (P1), and the walk's high step in the
  sharpest turn.
- **How heavy the miners are.** They are small beings. Their weights come
  from their models; whether that reads well is for the bench to show.
