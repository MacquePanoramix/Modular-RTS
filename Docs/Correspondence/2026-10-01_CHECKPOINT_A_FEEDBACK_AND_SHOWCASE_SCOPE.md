# Checkpoint A feedback and the showcase's scope

**Date:** 2026-10-01, after Luis playtested Strength and Burden checkpoint A.
**Status:** source material from the Game Director. [GAME_VISION.md](../GAME_VISION.md)
records the resulting decisions, and [ShowcaseRoadmap.md](../ShowcaseRoadmap.md)
turns them into a staged plan.

---

> Okay, here's my feedback so far I'm thinking. I understood that you're basically focused on the walk right now so far, right? So yes, the walk is much better now, I must say. The only thing, I think, is when the character is going to the landing spot, it always shuffles a bit its feet, I think to, like, try to be on the exact location that we directed. But it looks a bit awkward, so I think after it lands near the landing spot, it can just stay still there. It doesn't need to reshuffle its feet to the exact spot, because I think that will look more natural. I'm still thinking. Yeah, I'm still thinking how we should handle movement speed. I think for now just this constant one, like we don't have the character customization had movement speed in it. But yeah, there's a lot of stuff. I don't know how much work accounting you need to do still, but there's a lot of stuff needs to be changed. Because, like I said, this is to be the very low-scale hi-fi prototype. So what I mean with that is the scale will be very low, because for the final one I want just one worker, or maybe more, but at first the idea is just one worker and a, like, character customization or character creation screen for that worker. So it will always be a worker, so it should always have the basic worker stuff there. But instead of the civilization customization, I want just the focus on the character customization, as if it's like a video game character customization, which will be the final one to actually customize the characters. And, yeah, I mean, as you see right now, when the worker is carrying the box, like the pickaxe, it's just like sitting behind his back, which doesn't make any sense. So my first thought already is the worker needs to either leave the pickaxe, but that's a bit strange, so I feel what actually needs to be is the worker needs to be strong enough to carry, like, the box with one hand and have the pickaxe with another, or have a backpack, or have a sack, or have a cart. It needs to have one of these options to be able to come back also with the stuff. And, yeah, I would also like, if possible for you to, you can use the Blender also that I have on the computer if needed for those. But I want the models for, since it's a hi-fi prototype, I want the models for these to be much better quality already. I don't know if I'm forgetting about anything now, but yeah, I want you to upload the changes to GitHub and always be very organized about documentation at each step, so it is very clear what we are doing. And I don't know if there's anything more that we need to account for. But I hope you can understand the general picture that I'm trying to get at, and yeah, consider all the previous stuff that I was saying. So this is supposed to really be the high fidelity, super high quality but small-scale prototype to show the player, okay, this is how a single worker would be like from the character customization screen. In the actual game there's no such thing as a worker, like the actual unit type. It's just you customize units to end up as workers, since you need to collect resources somehow. But this is just for a worker and just to test exactly the capabilities of this section, of a very small section of the game. That's why it needs to be very high quality, because I'll actually let people playtest this to see how they feel about it.

---

## What this settles

- **Walking.** Checkpoint A is "much better". The one defect named is the
  re-shuffling on arrival. The body should stop near the destination and stay
  there.
- **Movement speed.** It stays constant for now and is not a character-creation
  option. Luis is still thinking about movement speed in general.
- **Showcase scope.**
  - The hi-fi prototype is one worker (maybe more later) plus a character
    customization screen for that worker.
  - It replaces the civilization/faction creator in this prototype.
  - The creator should feel like a video-game character creator. It is meant
    to become the quality bar for the final unit customization.
  - The character is always a worker and always has basic worker abilities.
- **Hauling.** The back-stowed pickaxe "doesn't make any sense." The worker
  must get the material home in one of these ways:
  - carry the load in one hand and the pickaxe in the other, if strong enough;
  - a backpack;
  - a sack;
  - a cart.
  Leaving the pickaxe behind "is a bit strange". This revises decision D5.
- **Models.** They should already be of much better quality, and Blender on
  Luis's computer may be used.
- **Process.** Changes go to GitHub, with organized documentation at every
  step.
- **Audience.** Outside players will playtest it, which is why quality matters.
- **Framing.** In the full game, "worker" is not a unit type: players customize
  units that become workers. The showcase isolates this one slice to test it.
