# S1d — The miners in the game: choose one, walk the meadow

**Built:** October 3, 2026, on branch `claude/worker-showcase`.
**Asked by Luis:** after the polish, "start the rigging process and making
them light enough to run well on the rts", and offer all three as choices in
this prototype ([correspondence](../Correspondence/2026-10-03_THE_MINERS_FEEDBACK.md)).
**The models:** [TheMiners.md](../ArtDirection/TheMiners.md).
**Build:** `Builds/WindowsOrdinaryPlace/WonderGather.exe`. It opens in the
Ordinary Place with the choice of miner.

![Long, Small and Round walking to the door at dusk](../Images/Miners/Rigged_Together_Dusk.jpg)

## What to try

1. **Choose your miner.** The choice opens when the place loads:
   - **Small:** young and curious, a lantern at the hip (in the hand
     until October 6).
   - **Long:** tall and unhurried, a pickaxe on the back.
   - **Round:** sturdy and laughing, a lamp on the cap.

   Pick one. `M` opens the choice again at any time. The new miner takes the
   last one's place, and the selection if the last one was selected. The
   last choice is remembered for next time.
2. **Walk.** Select the miner and right-click the ground, as before. The
   procedural body now walks the modelled body:
   - **Feet** plant and roll heel to toe.
   - **Hips** sway.
   - **Arms** swing against the legs.
   - **The head** looks along the path.
   - **Clothes:** coats, smocks and the apron follow the thighs; boots follow
     the feet.
3. **Look closely and from afar.**
   - **Up close.** `V` switches to Explore for a close look.
   - **Levels of detail.** The models change between three levels as the
     camera rises, so the Strategy camera's height is cheap.
4. **Dusk and night** (keys `4` and `6`): Small's lantern and Round's cap lamp
   glow, and light whoever carries them.

![Walking, frame by frame](../Images/Miners/Rigged_Walking.jpg)

![At dusk, walking towards the camera](../Images/Miners/Rigged_Fronts_Dusk.jpg)

![From the Strategy camera's height](../Images/Miners/Rigged_Together_Strategy.jpg)

## After Luis's play (October 3, evening)

- **Small.** The lantern is carried in the left hand and swings. The satchel
  sits on the right hip, properly hung from its strap. Legs line up with the
  boots, and the neck rises cleanly into a closed collar.
- **All three.** These were polished on a close audit; see
  [TheMiners.md](../ArtDirection/TheMiners.md#polish-after-luiss-play-october-3-evening)
  and the practices in
  [CharacterPractices.md](../ArtDirection/CharacterPractices.md).
- **Natural walks** replace the one shared pace:

  | Miner | Pace | The walk |
  |---|---|---|
  | Small | 1.27 m/s | brisk, quick, bouncy |
  | Long | 1.38 m/s | long, smooth, unhurried strides |
  | Round | 1.28 m/s | rolling, side to side |

## After the method's first round (October 4)

Luis's five notes on Small, the principle that every object is a proper
object, and the [model quality method](../ArtDirection/ModelQualityMethod.md)
Luis asked for. The round's log, with before and after images, is in
[TheMiners.md](../ArtDirection/TheMiners.md#the-methods-first-round-october-3-and-4).

**Where to look, on Small** (`V` for Explore, then close in):

1. **The boots, from the side, at ground level.** The leg goes down into the
   boot. The laces lie on the leather, through eyelets, with a bow.
2. **The lantern, from behind and from below.** The hand is closed round a
   wooden handle; the lantern hangs from it and swings as Small walks, starts
   and stops. It never goes into the coat.
3. **The satchel.** Each end of the strap passes through a ring on the bag.
   The bag hangs from the strap, rests against the hip, and moves with the
   stride.
4. **The strap,** over the shoulder and across the buttons. Since October 5
   it lies as it did before the round (Luis preferred that): out on the
   shoulder, soft on the coat, below the collar.

**On Long and Round:**

- **Long's pickaxe** hangs in a sling on the back: a strap, two leather
  loops, the pick resting by its head.
- **Long's mug** hangs from the fingers by its handle, and swings.
- **Long's coat** hangs slim; its front goes with the knee, its back hangs
  until a leg reaches it.
- **Round's apron** hangs from a strap round the neck, is drawn in by ties
  knotted at the back, and carries a hammer in a leather loop.

**What to judge:**

- **Do the things read as objects now?** Held, hanging, swinging, stopped by
  the body.
- **Is the movement Luis liked still there** on Small: the coat's skirt, the
  lantern, the bag?
- **Long's coat in the walk:** calm enough, or too calm?
- **Sharp turns.** The boots now step round each other. For a frame, in the
  sharpest turn, a knee shows under the lifted hem: the walk's high step,
  left for Luis's word.

## A first look at the work (October 6)

> **This is how it was until October 8.** `K` put a plain block before
> the chosen miner and made its pickaxe in its hands; `,` `.` made it
> weaker and stronger, `-` `=` its pickaxe lighter and heavier
> ([design](../Design/ThePhysicalBody.md#in-the-place-to-try-october-6);
> [clip](../Images/PhysicalBody/Look_InThePlace.gif)). Those keys do
> nothing any more: the next section is how the work is tried now.

## The physical work, with the panel (October 7)

> **Changed on October 7 and 8 (S3, step 11).** The keys are gone: `K` (the
> block and a pickaxe made in the hands), `,` `.` (strength) and `-` `=`
> (the pickaxe's weight). A plain panel takes their place
> ([design](../Design/ThePhysicalBody.md#step-11-the-panel-in-the-ordinary-place-october-7)).
> Nothing appears in a miner's hands any more: the panel puts a pickaxe
> on the ground beside it, and it picks it up itself. What `K` showed
> before October 6 was the first swing
> ([clip](../Images/Miners/Work_Clip_Side.gif);
> [review](../Reviews/2026-10-06_SamePageReview.md#what-the-swing-is-today)).

![The panel, with Round chosen](../Images/PhysicalBody/Panel_InThePlace.png)

![Round picks up the pickaxe put beside it, goes to a boulder and strikes it](../Images/PhysicalBody/Panel_Round_PicksUpAndMines.gif)

**The panel** (lower left, while the choice of miner is closed):

| On it | What it does |
|---|---|
| **Strength** | A slider from 0.30 to 3.00 times what the miner's build gives it, and a button back to 1.00. It takes effect at once, whatever the miner is doing |
| **Light, Its own, Heavy** | Puts a pickaxe of the miner's own kind on the ground beside it: 0.6 of its own pickaxe's weight, its own, or 1.8 times it. Each button says the kilograms |
| **Take away** | Takes away the pickaxes that lie on the ground, in no hand |
| **A line of words** | What the miner is doing: how it holds its pickaxe and what that asks of its hand, how spent it is, how fast and with how much energy its last blow landed, the stones off its boulder; and, for a few seconds, why it did not do what it was told |

**To try** (run `Builds/WindowsOrdinaryPlace/WonderGather.exe`):
1. Choose a miner in the choice that opens (`M` opens it again). The
   panel is at the lower left.
2. **A pickaxe.** Press **Its own**. A pickaxe lies on the ground beside
   the miner. Nothing is in its hands.
3. **Pick it up.** Hold the space bar: the pickaxe, the miner, what hangs
   on it and the boulders show their names (the pickaxe with its
   kilograms). Click the pickaxe and choose **Pick it up**. The miner
   places itself by it, sets its feet, goes down, takes it, and stands up
   with it in one hand.
4. **A boulder.** Hold the space bar, click any boulder in the grass, and
   choose **Mine**. The miner goes to it, takes the last steps to the
   rock's foot, and strikes it. Pieces break off and lie. (With no
   pickaxe in its hands it first goes to the nearest one on the ground
   and picks it up. If none lies anywhere, it does nothing, and the panel
   says why.)
   - **The two lowest boulders** are small domes in the grass, a fifth of
     a metre high. A miner whose knees would be bent too deep there (Small,
     Long) bends to its work, stands up, and takes the blow another way,
     its knees bent less. If it finds no such way at a rock, it stands up
     with its pickaxe, and the panel says why.
5. Go close with the Explore camera (`V`), and round it.
6. **Stronger and weaker.** Move the slider while it works. Stronger: the
   lift is quicker, the blow lands harder, more pieces come off. Weaker:
   the upper hand goes up the handle towards the head, the blows land
   softer, it rests sooner.
7. **Lighter and heavier.** Hold Space, click the pickaxe in its hands,
   **Lay it down**. Press **Heavy** (or **Light**), and have it pick that
   one up and mine with it.
8. Leave it working. After a while it tires and gets its breath: it
   stands up with the pickaxe in one hand at its side (Long puts the head
   down on the rock or the ground), and goes on.
9. **Rest, and back to work.** Hold Space and click the miner itself at
   its boulder: **Rest**. It stands at ease by its rock. Click it again:
   **Back to work**.
10. Send it somewhere while it works (a right click on the ground). It
    takes its pickaxe with it.
11. **Too weak to get down.** With nothing in its hands, bring the slider
    down to about 0.60, and tell it to pick a pickaxe up. It bows, bends
    its knees a little, does not reach, and stands up again; the panel
    says why. At 1.00 it picks the same pickaxe up.
12. **Weak at its work.** Let it pick the pickaxe up at 1.00 and send it
    to a tall boulder; once it stands with the pickaxe, bring the slider
    down to 0.70, then 0.50. Its blows land slower and softer, and it
    rests sooner. (Long at 0.50 does not work at all: it stands up from
    the rock, and the panel says why.) Do not bring the slider down
    while it is down in its squat for a pickaxe: weakened there, its
    legs give way.
13. **The lantern and the mug.** Choose Small or Long, with nothing in the
    hands. Hold the space bar, click the lantern (or the mug) and choose
    **Take in hand**. Walk the miner about. Click it again: **Hang it
    back**. With it in the hand, tell the miner to pick a pickaxe up: it
    hangs it back first.
14. **A fall.** Choose Small. Let it pick up the **Heavy** pickaxe at
    1.00 and send it to a tall boulder; on its way, bring the slider down
    to 0.50, and leave it. After half a minute or so, as it rests with
    its pickaxe's head down, its legs give way: it lets the pickaxe go,
    falls, lies, and gets up. (Long does the same at 0.70, after about
    fifty seconds. Round does not fall so.)

**What to look at:**
- **Weight.** Does the pickaxe look as if it weighs something, lying,
  picked up, going up and coming down?
- **Strength.** Is the difference along the slider enough to read, and
  does it read as strength?
- **Each body.** The three do not swing alike. Long's pickaxe is heavy for
  Long: its upper hand is right up at the head.
- **Picking up.** Does the body place itself and go down to the ground as
  a body would? It takes about four seconds: too deliberate?
- **Too weak to get down.** Is "it does not reach, and stands up again"
  right for a weak body? Or should it kneel, put a hand on its knee, or
  fall?
- **Tiring and resting.** Does the rest read as a rest?
- **Its feet.** Does it look steady, and never wobbly?
- **Walking with it.** Does the pickaxe look carried, with a weight of its
  own?
- **The fall.** Does it fall like a body, and not like a doll? Does the
  getting up read? Is it rare enough? Should it cost the miner something?
- **At a boulder.** Does it stand where a miner would, and strike where
  one would? Do the pieces look and fall like stone? Should the boulder
  get smaller, and run out?
- **The panel.** Is it in the right place, and plain enough? Are three
  pickaxes of the miner's own kind right, or should they be the three
  miners' own pickaxes (1.5, 2.4 and 3.2 kg)?
- **The space bar.** Is it the right key for the click?

**What is not there:**
- **It is not the game's mining yet.** Pieces break off a boulder, but the
  boulder stays whole and nothing is brought home (S4).
- **A weak miner does not pick a pickaxe up** (below about nine tenths of
  its strength): it has no other way down to the ground than the squat.
- **A way to work at rock lower than a body can strike standing** (on
  one knee, say): at a low boulder a miner stands as it can, or leaves
  the rock.
- **Known:** Long at twice its strength with the heavy pickaxe was
  thrown off its feet by its own swing, once; a weak miner up from a fall
  may stand bent double until the slider is raised (the panel says so).
- **Setting the lantern or the mug down** on the ground, and a pickaxe in
  one hand with the lantern in the other.
- **Something that pushes a miner over.** Nothing in the build pulls or
  pushes a miner: a fall comes only from work far too heavy for it.
- **The pickaxe does not stop at the miner's own body,** nor at the
  lantern or the mug at the hip.
- **The block of rock that `K` put before the miner** is gone from the
  build (the tests and the benches still use it).

## After Luis's play of S3: what to look at now (October 8)

Luis's ten notes, and what was found and done for each, are in
[the playtest round's page](../Reviews/2026-10-08_ThePlaytestRound.md).
The build in `Builds/WindowsOrdinaryPlace` has all ten. To see
them:

1. **The fireflies** (key `4` for dusk, `6` for night). Pull the camera
   back and push it in: each stays where it is in the meadow; from
   further there are fewer, and fainter. *Is this few enough? Is it too
   few from far?*
2. **The cabin's light.** Pull the camera all the way back, with the
   house in the middle of the screen and at its edge: the light should
   not change at any point. **Key `0`** switches the lamplight between
   *spreading* (the look from far that Luis liked, now at every
   distance; the place starts with it) and *shaded* (the door's frame,
   the bench and whoever stands there shade it). *Which is it to be?*
3. **The walk.** Send a miner down the slope by the path and watch its
   head and hips against the far hills. *Is the stutter gone?*
4. **Turning round.** Send a standing miner straight behind it. Its head
   looks round first; it turns in three steps; then it walks. **Send a
   walking miner back the way it came:** it stops, turns the same way,
   and walks (about two seconds). *Does it read as a body turning? Is
   two seconds too long to answer an order?*
5. **Long's rest.** Have Long mine with its own pickaxe for a few
   minutes: it rests with the pickaxe at its side and goes back to work.
   With the heavy pickaxe it strikes twice, puts it down, and the panel
   says why.

6. **Going down for the pickaxe.** Lay a miner's pickaxe down (`Space`,
   click it, "Lay it down") and have it picked up again. It looks at the
   pickaxe, goes down in one movement with its hand going out, takes
   hold, and stands up. *Does it read as a body picking a thing up?
   Watch the coat as it goes down and comes up: does anything stand out
   or fly?*
7. **The start of a walk.** A miner now takes about a second to reach
   its whole pace. *Does it feel slow to answer?*
8. **The pickaxe in one hand.** Have a miner rest, or walk with its
   pickaxe: it hangs from the hand, the handle down behind (it was held
   level). *Is this the carry Luis wants?*
9. **The lantern** (`Space`, click it, "Take in hand"). The miner looks
   at it, then takes it off its hook, and carries it before its hip, the
   forearm raised; walking, the hand goes a little with the walk. *Is
   the hold still uncanny?*

10. **Getting up after a fall.** On the panel (lower left), press
    **Push it over**: the chosen miner is shoved the way the view looks
    and falls away from the eye. Turn the view to push it from another
    side. It lies (longer the harder it came down; the panel says how
    long, and whether it is winded or dazed), draws its knees up and rolls onto its front,
    brings its hips up over its knees with its head down, folds onto its
    knees, and gets up. Try all three miners. *Does it read as a body
    getting itself up? Watch the moment it comes off its knees (half a
    second): that is the part that is not its own strength yet. Is
    eight seconds on the ground too long? Since October 9: is the time
    it lies right for how it fell? (It lies quite still: that is known,
    and open.) The panel's push is always the same for the miner's
    weight. By the reckoning, and not tried in the build: with its
    strength set lower it lies longer; tired from its work, a little
    longer.* (The way to a fall in step
    14 may no longer bring one about: since October 8 a pickaxe too
    heavy slips from the hands before the legs give way.)

## What changed for the game

- **Carried things are carried.**
  - Small carries the lantern in the left hand (until October 3 evening, it
    hung at the hip with nothing holding it).
  - Long carries the mug in the left hand, with the pickaxe slung on the back
    on a strap across the chest.
  - Round's hands are free; the apron's hammer stays in its pocket.
- **Each walks at its own pace** (see above). Until October 3 evening, all
  three walked at 1.3 m/s.
- **Arms** hang close to the body and slightly bent, just clearing each
  one's clothes. Round's are a little wider, around a round body.

## What it costs

A release build at 1920×1080 on the RTX 4060 Laptop, by day, with every
miner walking between random places in the meadow:

| Miners walking | Strategy view, frame ms (p95) | Close view, frame ms (p95) |
|---|---|---|
| 0 | 4.8 (5.2) | 4.7 (5.1) |
| 25 | 5.5 (5.8) | 5.2 (5.4) |
| 50 | 5.9 (6.7) | 5.5 (6.0) |
| 100 | 6.9 (7.8) | 6.4 (7.1) |

A hundred miners add about 2.1 ms: about 0.02 ms each. (Measured again on October 4, with the hanging things and the skirts' flaps.)

**With all of S3 on each miner (measured October 8,** the machine
otherwise idle, full screen):

| Miners walking | Strategy view, frame ms (p95) | Close view, frame ms (p95) |
|---|---|---|
| 0 | 4.84 (5.35) | 4.92 (5.27) |
| 25 | 5.62 (5.95) | 5.33 (5.64) |
| 50 | 6.08 (6.88) | 5.81 (6.54) |
| 100 | 7.31 (8.70) | 7.00 (8.33) |

A hundred miners add about 2.5 ms: about 0.025 ms each.

**One miner at its physical work** (the same build, `-wgwork`): 4.51 to
4.83 ms a frame at its work at a boulder, 4.57 to 4.72 ms standing. Its
hands cost 31 to 33 millionths of a second for each step of the physics
([the tables](../Design/ThePhysicalBody.md#step-12-evidence-october-8)).

**With the finger bones (measured October 6).** The computer was in use and
the game's window in the background, so every figure is higher than the
table above and the two are not comparable. The builds without and with the
finger bones were measured one after the other, twice each (means, the 95th
percentile in brackets):

| Miners walking | Without finger bones: strategy view | close view | With finger bones: strategy view | close view |
|---|---|---|---|---|
| 0 | 5.46 (6.64) | 5.22 (6.04) | 5.46 (6.79) | 5.19 (5.95) |
| 25 | 6.30 (8.81) | 5.85 (8.07) | 6.32 (8.98) | 5.83 (7.93) |
| 50 | 6.76 (9.34) | 6.64 (9.04) | 7.00 (9.51) | 6.61 (8.98) |
| 100 | 7.97 (10.00) | 7.70 (9.40) | 8.16 (10.00) | 7.72 (9.51) |

A hundred miners add about 2.5 ms without the finger bones and about 2.6 ms
with them: no more than two runs of the same build differ by.

- **Levels of detail:** about 18,300 (Small, Long) or 19,600 (Round), then
  5,000 and 1,600 triangles per miner. A hand that closes keeps 1,600
  triangles at the nearest level.
- **Materials:** one painted material and one texture (2048²) each, plus
  the outline on the nearer two levels.
- **Bones:** 40 for Small, 39 for Long, 54 for Round: the body's 19, one for
  each hanging thing, four for the skirt's flaps, and fifteen for each hand
  that closes.

The benchmarks are in the build: run it with `-wgcrowd` (the walking
crowd) or `-wgwork` (one miner at its work). They write
`miner-crowd-benchmark.csv` and `miner-work-benchmark.csv` beside the
player log.

## What to judge

- **Do they feel alive walking,** or still stiff? The body was tuned on the
  2.2 m test figure; its gait now scales to each miner, but it has not been
  re-tuned by eye for them.
- **The choice.** Does the picker feel right as a first step towards the
  creator?
- **Up close in motion.** Look at shoulders, hips and knees: skinning stretches
  the cloth at the joints.
- **The walks.** Do they feel natural to each body? (Luis asked for that on
  October 3.)

## Standing by its own joints (October 10)

**New on the panel** (the Ordinary Place; nothing is different until
the switch is on):

| On the panel | What it does |
|---|---|
| **Stands by its own joints (at ease, hands empty)** | A switch. On: the chosen miner, when it stands at ease with nothing in its hands, stands by what its own hips, knees and ankles give. Off: as it was |
| **Nudge it** | Sets its whole body going the way the view looks, at 0.15 m/s. (Only while it stands by its own joints) |
| **Breath: drawn stronger / in its shoulders / seen in the air (dusk to dawn) / as in life / not drawn** | How its breath is drawn. Five looks to choose between; press to go to the next. **For the look in the air, make it dusk or night first (keys `4` and `6`):** by day nothing is seen |
| **Tire it** | Makes the chosen miner tired at once, as after hard work, to see it breathe so. It rests as it does after work |
| **Breath size** (a slider, since October 10) | How large its breath is drawn, from nought to three (one is the size it had when Luis could not see it). It begins at 1.6, on the strong side on purpose: **turn it until it looks right, and tell me the number** |

**Since Luis looked (October 10):** the breath begins with the look
"in its shoulders" and at a larger size. **The switch must be on** for
a standing miner to breathe at all: without it, it stands as it did,
and only a fallen miner breathes.

**What to try.** Choose a miner, switch it on, and watch it for a
minute from near. Nudge it from several sides. Push it over ("Push it
over"): it should fall as before, get up, and stand by its own joints
again. Send it somewhere: it should walk as before and stand so again
where it stops. Tire it and watch its breath. Then switch it off and
look at the same miner standing as it did.

**What to judge.**

- Is it **stable**? (It should not wobble, sink, or slide. Nudged, it
  should rock a little and come back.)
- Is it **alive**? Its judges said: only in its head. Its breath and
  its weight going from leg to leg are there and can hardly be seen.
- **Its breath:** which of the five looks, if any, or which together
  (they are separate for now so that each can be judged alone); and
  whether the look in its shoulders is a breath or a shrug. Tire it
  and look again.
- **Its head:** does it look about too often, or too far? It has no
  eyes that move, so its head does all of it.

**What it does not do, so that it is not taken for a fault.**

- **It cannot step.** Nudged again and again before it has settled, or
  harder than the button does, it will go over (into the fall, and get
  up). It does not lean against a pull as the posed body does.
- **Only the chosen miner** stands so, and only with empty hands. With
  a pickaxe in its hand, or at its work, it is the posed body.
- Long's chin held up, the arms hanging away from the body, Round's
  shut eyes: these are how the models are made, and show more on a
  still body seen from near.

## Not yet

- **Mining as part of the game.** A miner picks a pickaxe up, goes to a
  boulder and strikes pieces off it
  ([above](#the-physical-work-with-the-panel-october-7)). The boulder
  stays whole, and nothing is hauled home: that is S4
  ([the plan](../Plans/S3_WeightAndStrengthAtTheRock.md#steps)). The equipment scene still
  uses the 2.2 m test body and the first swing.
- **Faces.** One expression each. The hands that carry something keep one
  closed shape.
- **Not watched by eye.** Turning on the spot and jogging were not checked
  this way.
- **Other hardware.** Only the RTX 4060 Laptop was tried.
- **Extreme poses.** The models were checked on the game's own movement
  (standing, walking, a sharp turn, stopping), not yet on a deep knee bend or
  raised arms.

See [Validation.md](../Validation.md) for the tests and evidence.
