# The physical body

**Written:** October 6, 2026, by Claude, from Luis's two messages of that
day
([real weight and real strength](../Correspondence/2026-10-06_REAL_WEIGHT_REAL_STRENGTH_AND_THE_CREATOR.md);
[stable but able to fall, and the interaction click](../Correspondence/2026-10-06_STABLE_BUT_ABLE_TO_FALL_AND_THE_INTERACTION_CLICK.md))
and the research Luis asked for.

**Status:** the design that stage S3 builds
([NextMilestonePlan.md](../NextMilestonePlan.md)). What Luis said is marked
as Luis's. The rest is proposal: it is tried on a bench and shown to Luis
before anything is built on it. [GAME_VISION.md](../GAME_VISION.md) remains
the design authority.

## 1. What Luis asked for

- Bodies "react against real physics, with real strength in their limbs,
  and against objects with real weight".
- "I want characters to be very stable, but for them to still technically
  have these possibilities of tripping in this, like, ragdoll-ish way if
  it's an extreme situation that calls for it."
- "Some level of stamina for the muscles and tiredness."
- "I want all the actions to feel physical and real … It needs to be all
  the actions."
- "I want to later build upon what we do for this high-fi prototype":
  running and sword fights come later, on the same body.

## 2. Can "very stable" and "able to fall" be combined?

**Yes.** It is how people stand, and how the games that do this well are
built.

### How a person keeps from falling

People do not wobble, and they can still fall. Studies of balance describe
a ladder of answers, each used only when the one before is not enough:

| Rung | What the body does | When |
|---|---|---|
| **1. Sway at the ankles** | Presses the feet into the ground to shift where its weight is carried | Small pushes; all the time, unseen |
| **2. Bend at the hips** | Moves the trunk against the push | Faster or larger pushes |
| **3. Step** | Puts a foot where the weight is going, so the feet are under it again | Pushes the first two cannot hold |
| **Fall** | | Only when no step can catch it |

**What decides the rung can be worked out.** Take where the body's weight
is and how fast it is moving, and you get the point on the ground where the
feet must be for the body to stop upright (in the studies: the
"extrapolated centre of mass", or "capture point"). While that point is
inside the feet, the body is safe. When it leaves them, a step is needed.
When no reachable step can get under it, the body falls. So falling can be
a matter of numbers, never of chance.

### How games combine them

- **A simulated body that tries to keep its feet** (the technology in *GTA
  IV* and its successors): bodies take steps to stay up and fall when they
  cannot.
- **A posed body that can be let go** (a widely used Unity tool works this
  way): the body follows its animation while it can; a strong enough blow
  lets it go into physics; then it gets up.

The wobbly games are the ones that skip the ladder: they leave balance to
physics alone from the first moment.

### The design: stable until the physics says otherwise

The miner's body always knows its real weights and how they are moving. It
climbs the ladder only as far as the numbers make it.

| Rung | What is seen | What decides it |
|---|---|---|
| **0. At ease** | The planted walk and stance Luis has liked | The weight is well inside the feet |
| **1. Lean** | The body carries its weight against the load: back against a heavy pick held in front, forward into a pull | Where the weight of body and load together falls |
| **2. Brace** | Feet wider, knees bent, the body lower | The weight nearing the edge of the feet; or a known effort coming (a lift, a swing) |
| **3. Step** | One or more quick steps to get the feet under the weight again | The weight's point leaving the feet |
| **4. Fall** | The body is let go into physics, "ragdoll-ish", with its remaining strength trying to protect it. It lies, gathers itself, and gets up | No reachable step can catch it; or the legs cannot bear it |

**Why it will not be wobbly.** In ordinary work a healthy miner with a
pickaxe it can manage stays on rungs 0 to 2. Rung 3 needs something
unusual; rung 4 something extreme.

**What would be extreme enough,** from Luis's examples:

- a very strong force pulling or pushing the body;
- a swing or a thrust put in far too hard for the body's own weight;
- legs that give out from tiredness under a load;
- a foot caught while running (running itself comes later).

**What makes one body steadier than another** is real: heavier, lower,
wider-set, stronger in the legs, less tired.

## 3. Strength and stamina

- **Strength** is one number for now (Luis: "it can be a general
  strength"). It sets the most each working joint can give and the most a
  hand can hold. Later it may be by limb.
- **A body's weight** comes from its own model.
- **Stamina** (short-term) is how much of that strength is there right now.
  - Hard effort uses it up; rest brings it back.
  - It is kept for each group of muscles that works (arms, back, legs), so
    an arm can be spent while the legs are fresh.
  - The model is a simple one from the study of work and tiredness: muscle
    is rested, working or tired, and moves between the three at rates that
    differ by joint.
- **What tiredness looks like comes out of the same rules as strength.** A
  tired miner is a weaker miner for a while: it lifts lower, drives
  slower, takes shorter chops, rests the pick's head on the ground and
  waits. If it is made to go on, its hold can slip or its legs can give.
- **Open:** how fast tiredness comes and goes; whether there is a
  longer-term tiredness; what the player is shown of it.

## 4. Every object is a real body

- **Weight and balance** from its own model: the pickaxe, a stone, the
  lantern, the mug, later the sack and the cart.
- **A hand holds it** as hard as the hand can. A hold can slide along a
  handle (the sliding hand of a real swing) and can be overcome.
- **It hangs** from what really holds it: a hook on a belt, a ring on a
  bag, a strap. (Luis: "hanging somewhere, like that makes physical sense,
  from the clothes".)
- **It lies** where it is put or dropped, and stays there.
- **It is never placed by a rule alone.** If it moves, something moved it.

## 5. Every action is physical

Luis: "if it's going to strap the pickaxe, you need to find a believable,
simple way that looks like he's strapping the pickaxe on its back".

Every action is made from the same few movements, done by the same arms
with the same strength on the same real objects:

| Movement | What it is |
|---|---|
| **Reach** | A hand goes to a place: a handle, a hook, the ground |
| **Take hold** | The fingers close round what is there |
| **Lift and carry** | The object, now held, is moved. Its weight is felt |
| **Set or hang** | The object is brought to where it will rest: the ground, a hook, a bag's mouth, a strap |
| **Let go** | The fingers open. The object stays by what holds it now |

So "take the lantern in hand" is: reach to the belt, take hold, lift it off
its hook, carry it. "Lay the pickaxe down" is: lower the head to the
ground, lower the handle, let go. Mining is the same movements with a
swing between them. Nothing appears in a hand and nothing vanishes from
one.

## 6. The interaction click

**What Luis asked for.** The ordinary right-click keeps doing the ordinary
RTS thing. A special click on something that can be interacted with shows
its options beside it, in a small box, with a cancel. It is for "actions
… that I don't know how you would click on them on a usual RTS". The key
"would be the most important" after moving the map.

### The key: the space bar (recommended)

| Key | What RTS players expect of it | For this |
|---|---|---|
| **Shift** | Queues orders, in nearly every RTS | Must stay free |
| **Ctrl** | Groups of units; select all of a kind | Must stay free |
| **Alt** | Small extras; and Windows itself answers to it (Alt+Tab) | Risky |
| **Tab** | Cycles through a selection | Awkward to hold while moving the map |
| **Space** | Jumps the view to the last event or to the selection. This game gives that to `F` | **Free, the largest key, under the thumb while the fingers stay on the map keys** |

- **How it works.** Hold Space: everything that can be interacted with
  shows itself softly. Click one of them: its options open beside it.
  Tapping Space instead of holding it does the same for the next click.
  Escape, or a click elsewhere, closes it. One of the options is always
  **Cancel**.
- **The first option is what the plain right-click would do,** so the box
  also teaches the ordinary orders.
- **It is one setting,** so Luis can try another key.

Two games reach such a box without a key: in one, holding the right button
down opens it; in another, a plain right-click does. Neither fits here: the
plain right-click is the RTS order, and holding the right button looks
around in the Explore camera.

### Options for the prototype (a proposal: Luis asked what else makes sense)

| Clicked | Options | Stage |
|---|---|---|
| **The lantern or the mug,** hanging | Take in hand · Cancel | S3 |
| **The lantern or the mug,** in the hand | Hang it back · Set it down · Cancel | S3 |
| **The pickaxe,** in the hands | Lay it down · Cancel | S3 |
| **A pickaxe on the ground** | Pick it up · Cancel | S3 |
| **A boulder** | Mine · Cancel | S3 |
| **The miner itself** | Rest · Cancel | S3 |
| **The pickaxe,** with a strap worn | Sling it on the back · Take it in hand · Cancel | S4 |
| **A loose stone** | Pick it up · Put it in the bag · Cancel | S4 |
| **The sack, the cart** | Take hold · Let go · Cancel | S4 |

Each option is an order like any other. It goes the way every order goes
(what the player meant, the order given, how the unit takes it, what it
does), and what it does is one of the physical actions above.

## 7. Built to be built upon

- **Running and fighting** use the same body: the same weights, the same
  limits, the same ladder. A sword is another object with weight. A thrust
  put in too hard carries the body's weight past its feet, which is rung 3
  or 4.
- **Many units.** A body is worked out fully only while it needs to be:
  working, carrying, pushed, or near the edge of its balance. A miner
  walking with empty hands far from the camera stays as light as it is
  today. What a fully worked body costs is measured on the bench.

## 8. How it is built

**Two layers.**

1. **The body's intention** (what exists today, extended): the planted
   walk, where the hands mean to go, the plan of a swing or an action.
2. **The body's physics** (new): every part's weight; every working
   joint's limit; real objects in the hands; the measure of balance.

While the body is on rungs 0 to 3, the legs follow the intention and the
physics tells them what they are bearing. The arms, the back and
everything held follow the physics, pulled toward the intention as hard as
strength allows. On rung 4 the whole body follows the physics.

**To be settled on the bench, by pictures and numbers:**

- whether the arms are best built from the engine's jointed physical body,
  or from forces worked out at the hands with the arms following;
- whether it is steady at every frame rate;
- what one body costs;
- how it reads at the miners' size.

**The riskiest part is rung 4** (the fall, and getting up believably). It
is built after the swing works, and shown as clips before it is trusted.

## Sources

- The ladder of balance (ankle, hip, step):
  <https://www.frontiersin.org/articles/10.3389/fbioe.2021.670498/pdf>;
  <https://link.springer.com/doi/10.1007/s10439-009-9717-y>
- The point the feet must be under, and the margin of stability:
  <https://www.mdpi.com/2076-3417/13/19/10574/htm>
- A simulated body that keeps its feet (*GTA IV*):
  <https://en.wikipedia.org/wiki/NaturalMotion>
- A posed body that can be let go and gets up (PuppetMaster, a Unity tool;
  not used here):
  <https://assetstore.unity.com/packages/tools/physics/puppetmaster-48977/reviews>
- Tiredness as rested, working and tired muscle:
  <https://pmc.ncbi.nlm.nih.gov/articles/PMC3397684>
- What RTS players expect of each key:
  <https://www.liquipedia.net/starcraft2/Hotkey>;
  <https://news.blizzard.com/en-us/article/4552955/game-guide-special-control>
- A box of options with a cancel (*RuneScape*):
  <https://runescape.wiki/w/Choose_Option>
- Holding the right button for a box of options (*Kenshi*):
  <https://kenshi.fandom.com/wiki/Controls>
- The swing, strength and the ways of building a physical body:
  [Reviews/2026-10-06_SamePageReview.md](../Reviews/2026-10-06_SamePageReview.md)
