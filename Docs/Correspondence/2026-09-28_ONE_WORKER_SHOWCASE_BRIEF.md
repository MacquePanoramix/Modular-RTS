# The one-worker showcase — Luis's original brief

**Date given:** 2026-09-28, after the Living Worker playtest.
**Recorded verbatim:** 2026-09-30, supplied again by Luis so the source wording is kept.
**Status:** source material, not a specification. [GAME_VISION.md](../GAME_VISION.md)
remains the design authority, and [WorkerShowcaseVision.md](../Design/WorkerShowcaseVision.md)
is its structured interpretation. Words such as "maybe" and "perhaps" in this brief
mark ideas to explore. Keep them as possibilities, not locked rules.

---

> Okay, that prototype is good. So now I want you to look over the repository and start thinking about what should be the next implementation. However, I would also like to highlight that although this current prototype implementation is good, it is good to remember that it is just a prototype to be improved upon, and which is not the final vision, of course. In fact, I don't know if it is on the documents. If it's not, you can add to the documents. But I would like to describe for you what I envision as one of the more high-level, advanced, hyper-polished prototypes. And what I mean with that is a prototype that is more on scale, a prototype than in levels of completeness, since it's one of the prototypes that I want to be able to let people try out to test the game. So the idea is of just one worker in a landscape scene with one, like, metal or boulder thing. And the idea is that the worker will be a worker that has a pickaxe equipped from it, and this is also supposed to be stuff that you can customize and create in the modular RTS unit creation part. And yeah, my idea was always that the unit creation will be complex enough that it almost basically resembles like a video game character creator, like Dark Souls, for example, something like that, with all our mechanics, and so on, so on, so on for the buildings and all the modular RTS blueprint part. Anyways, my idea is that its movement is really fully procedural, but without looking goofy, but you can feel more of a realness in its movement. And the idea is maybe, yeah, the worker really walks to the mineral, and then you need to physically hit the mineral with the pickaxe. And maybe depending on the way it hits the mineral, it can get more or less of the mineral, maybe. And that we will need to see. And the idea is to really implement all of the character stuff to showcase in this worker one also. For example, an important condition: the character's strength will determine perhaps how it holds the pickaxe. For example, if it has a certain level of strength, it can hold the pickaxe with one hand while walking. If it has another level, it needs to sort of, like, drag the pickaxe on the ground, which will make it move in a different way, impacts its movement. If it has even lower strength, maybe needs another method that you need to, like, get some extra equipment to let it carry the pickaxe. Maybe you need to get it a cart, like a wooden cart that it pulls around with its tools. And it also needs to bring the stones, and that will enter the strength also, and maybe extra tools and materials, like a little cart or a bag, and how many it can carry. And I think you're getting the idea now of how in-depth I want the character customization and the modularity part of custom units and all that to be, and all to really interact in this, like, procedural movement and procedural stuff kind of way where you really need to physically carry the stuff around and all that. Is it clear from this? So you can decide what you think should be the next implementation part and add this as a thing that is good reference also.

---

## Emphases worth protecting

- **Hyper-polished but small.** "Prototype" refers to scale, not to depth or finish.
  Outside players should be able to try it.
- **"Fully procedural, but without looking goofy … more of a realness."** Motion
  quality is central to the showcase. It is not decoration added at the end.
- **Physical consequence.** The pickaxe must really hit the mineral. How it hits
  *may* change yield ("that we will need to see").
- **Strength shapes handling.** One-handed carry, dragging (which changes movement)
  and needing extra equipment such as a cart are examples of the intended depth.
  The exact thresholds are not specified.
- **Material has burden too.** Stones must be carried. Strength, bags and carts
  decide how many.
- **Everything comes from the creator.** The worker and its equipment are made with
  the same modular unit creation system as every other unit. That system aims for
  character-creator depth.
