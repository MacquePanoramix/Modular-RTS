# Getting up, for any body — the proposal (October 30, 2026)

**For Luis to decide.** He asked (October 30, [the message](../Correspondence/2026-10-30_THE_GETTING_UP_FOR_THE_WHOLE_GAME.md)) for
the final game's getting up: for any body the creator can make, with no teaching after a player makes it; "very
procedural", its proportions and strength having "a real natural impact in how it gets up"; natural, never goofy; and
first a deep research, its references looked at frame by frame. This page is what the research and the game's own
frames say should be built, and what is his to choose. The research:
[Research/reports/Getting up for any body.md](../Research/reports/Getting%20up%20for%20any%20body.md) (six threads, 153
sources). The game read every frame: [Reviews/2026-10-30_GettingUp_EveryFrame.md](../Reviews/2026-10-30_GettingUp_EveryFrame.md).

Nothing here is Locked. Which attributes a unit has, how strength is scaled, and the ways of getting up a body may use
stay Open (the vision's register).

## What it must do

- **Any body, at once.** Whatever the creator makes (short or tall, long-limbed or round, light or heavy, weak or
  strong) gets up as soon as it is made, with nothing taught for it.
- **Its build shows.** A weak heavy body goes by more stages, leans on its hands, rests; a strong light one goes
  straight up; a long-legged one does not squat; a toddler-like one with a heavy head goes up as toddlers do.
- **The body's own.** Its own joints, as strong as its build, move it; nothing lifts it; no animation is played
  (October 9, 13, 22).
- **Natural, grounded, never goofy.** No twitch, no limb a living body could not hold, nothing that flicks; quiet and
  weighty; its effort and its tiredness seen.
- **It gets up.** Not after minutes of failures: a failure now and then, as a person has, and then another way.

## What was found

1. **What Luis saw is mostly the body and its drawing, not the way of getting up.** The drawn limbs flipped half round
   in a frame (mended today); the body that gets up has no joint stops at all (elbows backwards, knees past 180
   degrees); its boots whip at up to 154 rad/s and once blew it apart. No way of getting up can look natural on such a
   body. ([The frames](../Reviews/2026-10-30_GettingUp_EveryFrame.md))
2. **One learned controller for every body is not ready.** It works for walking within about ±20 to 50% of a body's
   size; for getting up, the one study across bodies stood 17 to 42% of the time on unseen bodies, and top-heavy,
   long-armed bodies (Small's kind) did worst. It needs 100 million to 10 billion tries of practice; the laptop does
   about a million a night. No learned getting up has been shown to look natural to viewers; the ones that look human
   borrow one recorded person's movement, which pulls every body towards that person.
3. **Planning ahead live costs too much and looks twitchy:** about a thousand seconds of simulated body per real second
   for each fallen unit, and its own authors call it "stiff" and "noisy".
4. **A controller that works everything out from the body's own numbers has done what is needed, in parts:** walking
   for bodies of any proportions with nothing retuned (2010); arbitrary player-made creatures moved in a shipped game
   (Spore, 2008); a planned getting up carried out in physics that viewers rated 6.29 out of 10 against 6.92 for real
   recorded movement (2012). None has yet got any body up from the floor: this would be the first.
5. **People choose how they get up by what they can do**: how much of their strength a stance needs, how far their
   joints bend, where their weight sits. The less they have for the task, the more of their body stays on the floor,
   the more stages, the longer (about 4 s for an able adult, up to about 15 s). Toddlers, with a quarter of their
   weight in the head, go up on hands and feet, or by a squat. Heavy bodies keep upright and work the knee. Stiff
   ankles rule out the deep squat. That is exactly what makes a build show.

## The proposal: a body that works out its own way up

Built in layers. The first is needed whatever is chosen.

| | What | For any body because |
|---|---|---|
| **0. A body that cannot look broken** | Every joint stops where a living joint stops (elbows and knees never backwards, shoulders and hips within their ranges); its strength set by its build, weaker as a joint moves faster; no part turning faster than such a part can; the light parts (boots) calm; every frame checked as it is played, at several screen rates | Ranges, strength and speeds are read from the build |
| **1. Reading the body** (when a unit is made; milliseconds) | Its masses and where its weight sits, its limb lengths, joint ranges and strength; the stances people pass through (lying, on its side, on its front, on its forearms, sitting, hands and knees, hands and feet, kneeling, on one knee, squatting, bent over, standing), each placed for this body ("a hand under the shoulder", "a foot a third of a leg ahead"): which it can hold, and how much of its strength each needs | Stances are written for no body in particular, then fitted to each |
| **2. Choosing its way, at each fall** | From how it lies, the way up through stances it can hold with the most to spare. How much it has to spare sets how it goes: time at each stage, how many hands and knees stay down, rests, which side leads, how often it must try again. Changing smoothly with the body, never switching at a threshold (Luis, October 13) | The same numbers that make it work for any body make each body its own |
| **3. Moving, by its own joints** | Soft joints that hold the body's weight and push through the limbs on the ground; the next stage begun only when the body is ready (its weight over what will carry it), its weight moved before a hand or foot lifts; the hands let go only when the body is already on its way over its feet; a failure caught early, back to the last steady stance, and another way | Everything is worked out again every step from where the body actually is |
| **4. Tuned once**, while the game is made | The controller's general settings, tried over hundreds of made-up bodies across the creator's range; success, time to stand and every frame's faults, by kind of body | Never per body |
| **5. Later, only if needed** | One small learned correction for the hardest moment, taught once over the made-up bodies | Only if 0 to 4 fall short and the laptop's speed allows it |

**How the three miners might get up** (what it predicts, to be checked): **Small** (a quarter of its weight in its
head, short weak arms) on hands and feet, its hands walking back, a squat, standing, as toddlers do; from its back it
first rolls onto its front. **Long** (long legs) rolls to its side, onto hands and knees, onto one knee, a hand on that
knee, standing; no deep squat. **Round** (heavy body) more slowly: hands and knees, a rest, one knee, both hands pushing
on the knee, a rock forward, standing. A strong, light body made in the creator would sit up and stand through a squat
in a couple of seconds; a weak, heavy one would keep its hands down, rest, and sometimes need a second try.

**What it gives up.** Ways a person rarely uses (a kip-up, a backward roll) appear only if a stance for them is written.
The stances are written once by hand, from the references, for no body in particular; everything else is worked out.
**The risk:** nobody has published such a controller getting bodies up from the floor across builds, and the project's
own riser, a designed one, stood about a third of its tries. What is new is the body with stops, stances fitted to each
body, and stages begun by the body's readiness rather than a clock.

## The order of work

1. **The body** (layer 0) for Small, Long and Round, at 60, 120 and 144 frames a second: no joint past its stop, no part
   faster than its bound, nothing broken, nothing flicking, in every frame; standing as accepted.
2. **Made-up bodies**: a maker of bodies across the creator's likely range (and beyond it), and the evidence changed from
   "36 tries of Small" to success, time and faults across those bodies, by kind.
3. **Reading the body and choosing its way** (layers 1 and 2), checked before anything moves: the stances each body
   chooses, shown as still pictures from four sides, against what people of such builds do.
4. **Moving** (layer 3), one way first (front, hands and knees, one knee, standing), then all; every frame read; judges.
5. **Tuning** (layer 4) to at least 80% standing at the first try across the creator's range; then Luis plays.

## What Luis is asked

1. **This direction**, a body working out its own way up from the stances people use, built from layer 0 up? Or
   another: a learned controller for any body (research frontier; a GPU trainer and much compute; not shown to work for
   getting up); or live planning (too costly; twitchy).
2. **The stances**: a short list of the stances people pass through, written once as goals for any body (where hands,
   knees and feet go, relative to that body), not as movements. Does that count as "pre-made"? (October 13: nothing
   "pre-made".) The movement between them is the body's own.
3. **Bodies that cannot get up alone.** Real people too weak for their weight cannot (a quarter to a third of frail
   elders). May a creator's body be such that it cannot get up unaided, and if so, what then (another unit helps; the
   creator warns; the creator does not allow it)?
4. **How long getting up may take**, for an able body and for a weak one (people: about 4 s to 15 s).
5. **References**: the reference videos frame by frame need the Claude app's window open (its browser draws only then).
   And the CMU motion capture (six get-ups of one adult, free to use in a sold game) as reference for timing and
   stances, never played: may it be downloaded?
6. **Until the new way stands reliably**, Small's present way (and the old way after 12 tries) stays as it is.
