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

**What the bench settled** (October 6; [below](#9-what-is-built)):

- **How the arms are built: forces at the hands, the arms following.** The
  object is a real body in the engine's physics. Each hand pushes and turns
  it, and can give only what its arm's joints can at that moment, in that
  pose. The arms are then drawn to where the object is. This was built and
  it answered the bench's questions.
- **The other way was not built:** the engine's own jointed physical body
  for the arms. The plan said both would be tried and compared, and only
  one was. It stays worth trying if the arms' own movement comes to look
  stiff, because that way gives each part of the arm its own swing.
- **Steady at every frame rate tried:** the same lift and the same blow
  with the game running free, and at 25, 50 and 100 frames a second.
- **What one body costs:** about 26 millionths of a second for each step of
  the physics (fifty a second), for the hands' own work, measured in the
  editor. A hundred miners at work would add about 2 thousandths of a
  second a frame. The engine's own share for the objects is not in that
  figure.
- **How it reads at the miners' size:** see the clips. That judgement is
  Luis's.

**The riskiest part is rung 4** (the fall, and getting up believably). It
is built after the swing works, and shown as clips before it is trusted.

## 9. What is built

### Step 1: everything weighed (October 6)

Each part of each miner, and each pickaxe, has a weight, a place where
that weight is, and a resistance to turning. All of it is measured on the
models themselves (`Art/Blender/Worker/weights.py`):

- **the body under its clothes,** as the solids its own measures describe,
  at the density of a living body;
- **what it wears,** by the area of each piece of cloth, leather or sheet
  metal;
- **solid wood and iron** (a pickaxe, a hammer) by their volume.

| | Small | Long | Round |
|---|---|---|---|
| Height | 1.42 m | 1.86 m | 1.56 m |
| **Weight** | **40.7 kg** | **58.5 kg** | **87.6 kg** |
| of which worn and carried | 1.8 kg | 4.6 kg | 2.8 kg |
| The head | 10.0 kg | 6.5 kg | 9.8 kg |
| One arm (upper arm, forearm, hand) | 1.9 kg | 3.2 kg | 3.7 kg |
| Its weight's centre, above the ground | 0.81 m | 1.03 m | 0.87 m |
| **Its pickaxe** | **1.50 kg**, 517 mm | **3.21 kg**, 718 mm | **2.39 kg**, 561 mm |
| The pickaxe against the body's weight | 3.7% | 5.5% | 2.7% |
| The most its shoulder gives, at strength 1 | 24 N·m | 31 N·m | 61 N·m |

The full reports: `Art/Review/Miners/weights_<Name>.txt` and
`tool_<Name>.txt`.

**What the figures say:**

- **The heads are heavy.** Small's is a quarter of its weight. That is the
  shape of the characters, weighed honestly. Whether it reads well when the
  body has to balance is for step 6 to show.
- **Round is twice Small's weight,** and by far the strongest in the arms:
  strength is built from the limbs' own thickness.
- **Long's pickaxe is heavy for Long.** A pickaxe is sized by the arm that
  swings it, and its weight grows much faster than its length. Long's arms
  are long and thin. This is expected to show when Long has both hands free
  (step 3). It has not been tried yet.
- **A real pickaxe** of the common kind is 0.9 m long with a head of 2.3
  to 3.2 kg: with its handle, 4 to 5.5% of the weight of a grown person of
  75 kg. The miners' are 2.7 to 5.5% of theirs.

### Step 2: the bench (October 6)

Round stands in the Ordinary Place with its pickaxe over a block. The
pickaxe is a real body. Round bows to it, raises it over the right
shoulder as high as it will go, and brings it down on the block. This is
tried at three strengths, with the pickaxe at half, once and twice its own
weight.

![Round at three strengths](../Images/PhysicalBody/Bench_Round_Strengths.gif)

*Its own pickaxe (2.39 kg) at half strength, ordinary strength and double
strength. All nine: [Bench_Round.gif](../Images/PhysicalBody/Bench_Round.gif);
as stills: [Bench_Round_Strips.jpg](../Images/PhysicalBody/Bench_Round_Strips.jpg).*

**Nothing in these clips is timed by hand.** The plan says only where the
pickaxe is meant to go. How fast it rises, how hard the arms work, how
fast it lands: these come out of the weights and of what the arms can
give.

| Strength | Pickaxe at half weight (1.19 kg) | Its own (2.39 kg) | Twice its weight (4.78 kg) |
|---|---|---|---|
| **0.5** | raised in 0.60 s, arms at 55%; lands at 6.2 m/s | 0.62 s, 83%; 4.1 m/s | **1.36 s**, 97%, and not as high; **2.2 m/s** |
| **1** (ordinary) | 0.60 s, 27%; 8.4 m/s | **0.60 s, 42%; 6.5 m/s** | 0.60 s, 73%; 4.5 m/s |
| **2** | 0.60 s, 14%; 9.6 m/s | 0.60 s, 21%; 8.3 m/s | 0.60 s, 36%; 6.5 m/s |

*"Arms at 55%": the hardest-worked joint used 55% of what it has, on
average, while raising it. "Lands at": the head's speed as it strikes.*

**How to read it:**

- **The weak one with the heavy pickaxe** takes more than twice as long to
  raise it, with everything it has, does not get it as high, and lands a
  feeble blow. No rule says so: the arms run out.
- **Twice the strength with twice the weight** is the same swing (0.60 s,
  about 40%, 6.5 m/s) with twice the energy behind it. That is what physics
  says it should be, and it is a check that the sums are right.
- **An ordinary blow** with Round's own pickaxe lands with about 50 joules:
  what the same pickaxe would have if dropped from 2.1 m. The arms and the
  back add to what gravity gives.

**What the bench found, and what was done about each:**

| ID | Seen | Why | Done |
|---|---|---|---|
| B1 | Told to go straight to the end of the blow with all it had, the pickaxe swung the wrong way round: its head fell back behind the hands and came through underneath | Each arm gave what it could of its own share, and the two shares no longer added up to the turn that was meant | Both arms give the same share of what is asked, so together they always push the way that was meant. The plan leads the pickaxe along its path, a fixed way ahead of where it is |
| B2 | With the trunk upright, two hands can hold the handle only while it leans less than about 45 degrees forward | The upper hand's arm is too short to follow the handle further | The body bows from the hips to its work. The bow is by intention only: the back's own strength is not yet counted |
| B3 | The blow landed at the same speed whatever the strength | The arms finished early and the back's steady pace brought the head down | The back goes first; the arms follow when it is well on its way |
| B4 | After the blow the pickaxe vaulted over its own head | The plan went on pushing into the block | After a blow the pickaxe is meant to stay where the blow left its head |
| B5 | The same lift took 72% of the arms at one frame rate and 37% at another | The body is posed once a frame and the physics steps on its own clock: read as last posed, the body moved in stairs | The physics reads the body as it stands at each step's own moment. The same at every rate tried since |
| B6 | After the blow the pickaxe went on turning in the hands, another 45 degrees | A pickaxe's weight is nearly all at its head, so a blow there stops the head and hardly slows the turn. What stops the turn in a real swing is the hands and forearms on the handle, and they had no weight | The arms' own weight rides on the handle where each hand holds. A muscle forced back resists with more than it can push with |
| B7 | A hand came off the handle at full stretch | The link that keeps the handle within an arm's length was measured from the shoulder, not from where the wrist must be | Measured from the wrist. Left: up to 3 cm, for a step or two, in the fastest part of the blow |
| B8 | In the pictures the pickaxe was a frame ahead of the hands | The pictures were taken before the body was posed | Pictures and measures are taken after the body is posed |

### Step 3: both hands free, things that hang (October 6)

Both hands close on all three miners. Small's lantern hangs from a hook on
the coat in front of the left thigh; Long's mug hangs in a loop of thong at
the left hip. The pictures, the checks and what was found are in
[TheMiners.md](../ArtDirection/TheMiners.md#both-hands-free-the-lantern-and-the-mug-at-the-hip-october-6).
They swing as they did, from a bone of their own; they are not yet objects
a hand can take (step 8).

### Step 4: the swing (October 6)

After Luis's look at the bench ("very promising … the version we want is a
full swing that will involve the body adapting to the strength
capabilities and rock").

![The three miners with their own pickaxes](../Images/PhysicalBody/Swing_ThreeMiners.gif)

*Each miner with the pickaxe made for it, at ordinary strength.*

![Round aimed at three heights](../Images/PhysicalBody/Swing_Round_Aimed.gif)

*Round aiming at a low block, one at the hips' height and one at the
chest's.*

**What the body now does by itself:**

- **The upper hand goes up the handle for the lift, by as much as the tool
  feels heavy.** Holding the pickaxe at rest tells the body how much of its
  arms that takes. If it is little, the hands stay where they are. If it is
  much, the upper hand slides up towards the head, where the weight is,
  before the lift. In the blow it slides down to meet the lower hand, and
  the swing lengthens as it comes over. A sliding hand holds loosely: it
  steadies the handle and does not pull along it.
- **The back has its own strength.** It holds up the upper body, further
  out the further it bows, and whatever the hands are holding up, at the
  end of the arms. An ordinary back holds up about two and a half times
  its own upper body bent level. A back that is overloaded straightens
  slowly, or not at all.
- **The hips go back as the body bows,** by as much as keeps its weight,
  the pickaxe's with it, over its feet. This is the first rung of the
  ladder of balance.
- **Aimed at a point, the body takes the stance that reaches it.** It finds
  how far to bow, how far to bend its knees and how the pickaxe must lean
  for the head to rest just over that point. It prefers to stand tall. The
  legs straighten under the lift and bend again into the blow.

**The three miners, each with its own pickaxe, at ordinary strength:**

| | Small | Long | Round |
|---|---|---|---|
| The pickaxe | 1.50 kg | 3.21 kg | 2.39 kg |
| The upper hand goes up the handle | 26% of the way to the head | **all the way** | 18% |
| Raised | 0.79 m in 0.60 s | 1.03 m in 0.60 s | 0.88 m in 0.60 s |
| The arms, while raising it | 42% of what they have | **65%** | 36% |
| The back, while raising it | 28% | 32% | 30% |
| The head lands at | 6.3 m/s | 6.1 m/s | 6.9 m/s |

**Round, by its own adapting:**

| | The upper hand | |
|---|---|---|
| At half strength | 88% of the way to the head | raised with the arms at 66%, where they needed 83% with the hands left in place |
| At double strength | stays where it is | |

| Aimed at | The knees bend | The pick's head lands |
|---|---|---|
| A low block (a fifth of its height) | 23 cm | 11 mm from the height aimed at, at 6.9 m/s |
| A block at half its height | not at all | 13 mm from it, at 6.9 m/s |

**What the step found:**

| ID | Seen | Why | Done |
|---|---|---|---|
| S1 | Long needs its upper hand right at the head, and two thirds of its arms, to raise its own pickaxe | The pickaxe is sized by the arm that swings it, and its weight grows much faster than its length; Long's arms are long and thin | Nothing: it is true of this body and this tool. At half strength Long cannot manage it. It is what the creator's strength and a lighter pickaxe will be for |
| S2 | A blow was reported 10 cm inside the block | The pick's head moves fast by the tool's turning, and the engine found it inside the block only after the step | The tool is watched for ahead of each step, so the head is stopped at the surface. Where a blow landed is read from the head itself |
| S3 | The same lift felt lighter after the back and the hips were added | The hips going back bring the hands nearer the body | Nothing: it is what going back is for |
| S4 | Aimed at a block as high as the chest, the pickaxe ends lying on its side on the block | A blow on something that high should come in level, at its face, not down on its top | Left for step 9, where a rock's own shape says where and how to strike |

**What is not there yet:**

- **The legs have no strength of their own.** They bend and straighten at
  a set pace, whatever they carry. Step 6.
- **It is not in the game's mining.** Since later on October 6 the look
  with `K` shows this swing on a block
  ([below](#in-the-place-to-try-october-6)); a boulder is step 9.
- **The pickaxe does not yet stop at the miner's own body,** nor at what
  hangs at the hip.
- **Tiredness** came with step 5 (below).
- **The path over the shoulder is still the first swing's,** on the right
  side only.

### Step 5: tiredness (October 6)

Luis: "some level of stamina for the muscles and tiredness", short-term at
least.

![Fresh, tired, and resting](../Images/PhysicalBody/Tired_Round.gif)

*Round's first swing; its 26th, with its arms and back 40% spent; and the
rest it then takes.*

- **Each group of muscles tires by itself:** each arm, the back (the legs
  are counted too, and do not work yet). A share of the group is spent for
  now, and what it can give is what is not spent.
- **Hard work spends it, the harder the faster.** All-out work spends a
  third of a group in about ten seconds.
- **Light work does not tire.** Below about a fifth of what a muscle has,
  it can go on all day. This is what makes a restful way of holding a tool
  restful.
- **Rest brings it back,** the sooner the less the muscles are doing: most
  of a spent third in about half a minute.
- **Nothing else was added to make a tired miner look tired.** It is a
  weaker miner for a while, so by the same rules as before it lifts with
  more of what it has left, takes the tool nearer its head, and lands a
  weaker blow.

**Round, its own pickaxe, swinging on:**

| | The first swing | The 26th, before its rest | The 27th, after it |
|---|---|---|---|
| Spent | 1% | 40% | 15% |
| The head lands at | 6.7 m/s | 5.5 m/s | 5.9 m/s |
| The upper hand, up the handle | 19% of the way | 83% | |
| The arms, raising it | 37% of what they have | 55% of what is left | |

**The rest.** When its arms or its back are 40% spent, it stops after a
blow. It stands up straight, which is what rests a back. Its lower hand
lets go, and it carries the pickaxe in its upper hand alone, near the head
where the weight is, level at its side with the arm hanging: the way a
tool is carried. That costs the arm less than a fifth of what it has, so
the arm rests too. When it is down to 15% spent (under half a minute), the lower hand reaches for the handle and takes hold again, and
it goes back to work.

**What the step found:**

| ID | Seen | Why | Done |
|---|---|---|---|
| T1 | Resting bowed over the block with the pick lying on it, the arms rested and the back did not | Holding a bow is work for a back, whatever the hands do | To rest, the body stands up |
| T2 | Straightening while the hands stayed on the tool dragged the tool off the block | The arms are not long enough to hold a tool on the block from upright | The tool comes with the body: it is carried |
| T3 | Holding the tool level across the thighs with two hands, the upper arm never rested | One hand is always beyond the weight, so it carries more than all of it, with the arm reaching forward round the belly | One hand at the balance point, at the side, the arm hanging |
| T4 | Even a light hold kept a muscle a little tired for ever | The first rule tired a muscle at any effort | Light work does not tire |

**What came with it, for later steps:**

- **A hand lets go and takes hold again** (with a reach that takes a
  moment). The small actions of step 8 are made of this.
- **The tool carried in one hand at its balance point** is the first of
  the ways of holding and walking with a tool (step 7).

**Open:** how fast tiredness comes and goes (the paces here are a first
setting); whether there is a longer tiredness; what the player is shown.

### In the place, to try (October 6)

The bench could only be seen in clips. So that Luis can watch the work and
try it, this swing is now what the `K` key shows in the Ordinary Place, in
place of the old swing.

![The three miners at the block, in the place](../Images/PhysicalBody/Look_InThePlace.gif)

*Each miner where the place puts it, with its own pickaxe, at ordinary
strength.*

| Key | What it does |
|---|---|
| `K` | The chosen miner bows, a block stands before it, and it takes up its pickaxe and works on the block. `K` again (or walking it away, or choosing another miner) puts the block and the pickaxe away |
| `,` and `.` (the two keys right of `M`) | Weaker and stronger, a quarter at a time, from 0.3 to 3 times ordinary. It takes effect at once |
| `-` and `=` (the two keys right of `0`) | A lighter and a heavier pickaxe, from 0.4 to 3 times its own weight. The miner takes it up afresh |

A line at the foot of the screen says the strength, the pickaxe's weight,
how spent the miner is and how fast the last blow landed.

- **It is an early part of step 11,** brought forward so that the work can
  be watched while the rest is built. Keys stand in for the panel. The
  plan said the look with `K` would be retired; for now the key is kept
  and what it shows is replaced.
- **It is the bench, not mining.** Nothing is mined, the block is put
  where the miner stands, and the pickaxe appears in the hands. Any
  boulder by a click is step 9; taking up and laying down the pickaxe is
  step 8.
- **The old swing is no longer shown in the Ordinary Place.** Its code is
  still there (the equipment scene and its tests use it) until step 9
  replaces the game's mining.

**At the ends of the keys** (all three miners, the look begun as the key
begins it; nothing breaks at any of them):

| Strength | Pickaxe | What is seen |
|---|---|---|
| 3 | 0.4 of its weight | Fast, easy swings. The blows land at about 10 m/s (6 to 7 at ordinary strength with its own pickaxe), the hands stay at the end of the handle, the arms give 8 to 16% of what they have |
| 3 | 3 times its weight | Much like an ordinary miner with its own pickaxe: 5.7 to 6.7 m/s; Small's and Long's upper hand a quarter of the way up the handle |
| 0.3 | 0.4 of its weight | A weak body. The upper hand is right at the head, the back gives all it has and hardly straightens, and the blows land at 4 to 5.6 m/s |
| 0.3 | 3 times its weight | **It cannot swing it, and what it then does is not designed yet.** Round holds the head on the block and cannot raise it. Small's and Long's pickaxe slips off the block and hangs head down from their hands in front of their legs, where nothing stops it passing through them. What a body does with a tool too heavy for it (drag it, or leave it) is step 7 |

**What is not there yet** (of the bench, step 2):

- **The swing's plan was rough,** the back as strong as it liked, and only
  Round had been tried. Steps 3 and 4 answered these (above).
- **Balance is measured only as far as the hips going back.** Step 6.
- **The pickaxe does not yet stop at the miner's own body.**
- **It is not in the game's mining:** it can be tried on a block with `K`
  ([above](#in-the-place-to-try-october-6)).
- **Strength's figures are a first setting,** from an ordinary grown
  person's arm. They have not been set against measured swings.
- **Luis's look (October 6):** "very promising so far … On the hands
  side it's already looking quite good"; the full swing, with the body
  adapting to its strength and to the rock, is what is wanted next
  ([message](../Correspondence/2026-10-06_THE_BENCH_IS_PROMISING.md)).

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
