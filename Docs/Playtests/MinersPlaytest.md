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
   - **Small:** young and curious, a lantern in hand.
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
4. **The strap,** over the shoulder and across the buttons: pulled straight
   by the bag's weight.

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

- **Levels of detail:** about 17,400, 5,000 and 1,600 triangles per miner.
- **Materials:** one painted material and one texture (2048²) each, plus
  the outline on the nearer two levels.
- **Bones:** 25 for Small, 24 for Long and Round: the body's 19, one for each
  hanging thing, and four for the skirt's flaps.

The benchmark is in the build: run it with `-wgcrowd`. It writes
`miner-crowd-benchmark.csv` beside the player log.

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

## Not yet

- **Mining.** The miners walk in the Ordinary Place. Mining and hauling with a
  pickaxe (the equipment scene) still use the 2.2 m test body; the tool's
  grips are sized for it.
- **Fixed hands and faces.** Hands keep one relaxed shape, and faces one
  expression.
- **Not watched by eye.** Turning on the spot and jogging were not checked
  this way.
- **Other hardware.** Only the RTX 4060 Laptop was tried.
- **Extreme poses.** The models were checked on the game's own movement
  (standing, walking, a sharp turn, stopping), not yet on a deep knee bend or
  raised arms.

See [Validation.md](../Validation.md) for the tests and evidence.
