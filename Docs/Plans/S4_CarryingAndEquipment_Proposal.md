# Proposal — S4: carrying and equipment

**Proposed:** October 9, 2026, at the end of the playtest round
([the round's page](../Reviews/2026-10-08_ThePlaytestRound.md)).
**Not approved.** Luis said, on October 8, to "proceed with all the plans
I asked for" once the polishing was done; S4 is the first of them
([the order, approved October 6](../NextMilestonePlan.md#the-order-of-stages-approved-october-6)).
This page says how I would build it and what I need Luis to say first.
Nothing here is built.

**It becomes `NextMilestonePlan.md`** when Luis has answered the
questions below; S3's plan is then archived beside S1's.

**October 9: Luis has answered the first question (Q1), against my
recommendation:** "I want stability in the body but with it still being
the body's own. I don't want it to be posed like that."
([the message](../Correspondence/2026-10-09_THE_BODYS_OWN_NOT_POSED.md)).
So S4's movements are to be the body's own. What that needs first, and
whether it comes before S4, is in
[the proposal for the body's own](TheBodysOwn_Proposal.md). Q2 to Q6 are
still open. The table below is kept as it was written.

## Which of the prototype's questions it answers

The second of them: **can what a worker needs (its tool, and what it
mines) be carried by its own body, with real weight, in ways that its
strength and its gear decide; and can someone watching tell which, and
why?** S3 answered the first for one tool at one rock. S4 asks it of a
whole trip: to the rock, the work, and home with the stone.

It does not answer: who the worker is (S2, the creator), or whether the
whole is ready for other people (S5).

## What Luis has said that it builds

| Luis | When |
|---|---|
| The stones must be brought back, by strength or by gear: a load in one hand and the pickaxe in the other, a backpack, a sack, a cart | Sep 28, Oct 1, Oct 6 |
| Equipment is a choice with consequences: no pickaxe (cannot mine), a pickaxe, a strap for the back (asks strength of the back), a backpack, a dragged sack, a wheeled cart. "All of that needs logistics and real strength and weight against it" | Oct 6 |
| Every action is physical: "equipping, unequipping, strapping, putting into a bag" are seen done by the body, with real weight | Oct 6 |
| No fake animations: every movement is the body, with its weight, acting on the world, and reads as real ("fantasy media realistic") | Oct 8 |
| A worker that cannot do a thing shows that it cannot, and why, rather than failing silently (the roadmap's "readable inability") | Oct 1, the roadmap |

## What is already there to build on (S3 and the round)

- A body that is **weighed**, part by part, with **strength at each
  joint** and **tiredness** (`PhysicalBody`).
- **Hands that hold a thing by force** (`PhysicalHands`): what an arm can
  do depends on how the load is held; a thing too heavy for the hold
  slips.
- **A pickaxe carried** in one hand, or left where it lies if it cannot
  be moved; picked up from the ground and laid down in one movement
  (`PhysicalCarry`).
- **A back that carries the upper body and what the hands hold**
  (`PhysicalBack`), and **a balance** that leans against a load and steps
  to catch it (`PhysicalBalance`).
- **Boulders that break into pieces** with weight; **things that hang**
  on the body by a handle and are taken in hand (the lantern, the mug).
- **The interaction click** and **the panel**.
- **A fall, and a getting up** that is the body's own as far as its
  knees.
- **A search for the poses of a let-go body** (`GetUpSearch`), and **a
  way of judging a movement** (traces, judges, Luis).

## What I need Luis to say first

These decide how S4 is built, not only how it is tuned. I have a
recommendation for each; none is settled.

| | Question | Why it cannot wait | My recommendation |
|---|---|---|---|
| **Q1** | **Posed, or the body's own?** Lifting a stone, shouldering a pack, leaning into a cart: are they to be *posed* (worked out each frame from the ground, the body's build, its weights and strengths, nothing recorded: as the walk and the pick-up are) or *the body's own* (parts pushed by their joints' strength: as the fall and the turning over are)? | By my estimate it is the difference between weeks and months, and between movements that always succeed and movements that can fail in ways nobody planned. The getting up shows both: the body's own part is honest and takes eight seconds; the posed part is quick and is the part the judges called pushed by nothing | **Posed, with the load real.** The stone, the pack, the sack and the cart are bodies in the physics, pushed by the hands with no more than the joints can give (as the pickaxe is). The body is posed to them. Where a posed movement would do what the body's strength could not, it does not happen, and the game says why. The body's own movement is kept for when the body loses control of itself (a fall under a load) |
| **Q2** | **Long's pickaxe on its back.** Is that the "strap for the back"? | S4's first equipment is the strap | **Yes:** the pickaxe modelled on Long's back becomes the one it works with, taken off and put back by its hands; Small and Round get a strap only if chosen |
| **Q3** | **Where is home?** Where do the stones go? | The trip needs an end | **A heap beside the cabin's door,** on the ground, that grows with what is brought (V3 of the roadmap: "possible") |
| **Q4** | **What is a load?** A boulder's pieces as they fall (each with its weight, picked up one by one), or stone as a quantity? | It decides what the hands, the pack and the cart hold | **The pieces themselves.** A hand takes one; a pack, a sack and a cart take as many as fit and as the body can move (O3 of the roadmap) |
| **Q5** | **The order within S4.** | Each is built on the one before | Hands; the heap; the pack; the sack; the cart; the strap last (it is the only one that is not about the stone) |
| **Q6** | **How gear is chosen before the creator exists.** | S2 comes after | **On the panel:** a row of what the chosen miner has (nothing, a pack, a sack, a cart, a strap), put on the ground beside it as the pickaxes are, and put on by the miner itself |

## Steps (proposed)

Each is shown to Luis before the next is built on it, as in S3.

| | Step | What Luis sees |
|---|---|---|
| 1 | **A stone in the hand.** A boulder's piece is picked up from the ground (one hand; two for a large one; left where it lies if it cannot be moved, and the panel says why) and carried; set down again | A miner carrying a stone, and one that cannot |
| 2 | **Two hands, two things.** A stone in one hand and the pickaxe in the other, if the body is strong enough for both; if not, it says which it cannot | Luis's first way of bringing stone home |
| 3 | **Home.** A heap by the cabin: "Take it home" on a stone; the miner walks there and sets it down. What has been brought is counted by weight | The whole trip, once, by hand |
| 4 | **The walk under a load.** The lean against it, the shorter and slower step, the turn; what a load too heavy does to the legs (they tire; then they give) | The same walk, visibly laden |
| 5 | **A pack.** Lying on the ground: stones are put into it; it is lifted onto the back if the body can lift it, and worn; taken off and emptied at the heap. Its weight is on the back and the legs | Hands free, back laden |
| 6 | **A sack, dragged.** Filled on the ground; dragged by one hand, the body leaning away from it; slower on grass than on the path | A weak body bringing more than it could carry |
| 7 | **A cart.** Two handles, wheels; the tool and the stones in it; pulled; a slope matters | The most stone, at the cost of the path |
| 8 | **The strap.** The pickaxe hung on the back and taken from it by the hands | The tool put away and taken up, done by the body |
| 9 | **"Mine this and bring it home."** One order: the miner does the whole trip with what it has, and says what it cannot do | The loop, for each of the three miners |
| 10 | **What it costs.** The frame's time with one miner and with a crowd; the tests; the playtest's steps | Figures |

**Models** (Blender, made here, no downloads): the pack, the sack, the
cart, the strap; the heap.

## How it is checked

As the round has it ([the judging of movement](../ArtDirection/MotionJudging.md)):
each new movement is traced frame by frame on **all three miners**,
through a whole trip and at several strengths; read in pictures before
any number is believed; given to three judges who did not make it;
then shown to Luis. Passing these is not Luis's acceptance.

## What it does not do

- No second worker helping, no trade, no storehouse: one worker, one
  rock, one heap.
- No running, no fighting.
- No creator (S2): gear is chosen on the panel.
- Nothing is merged to `main` without Luis's word.
