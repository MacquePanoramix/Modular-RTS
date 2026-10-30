# The getting up, for the whole game — October 30, 2026

**From:** Luis (Game Director). Two messages on October 30: the first after the build of that morning was made
(`D:/Dev/WG/Builds/GettingUp_2026-10-30`, `pace10`'s joints and pacer, the switch "Gets up by its own body" off by
default); the second after he had played it.

## The first message (verbatim)

> Okay, so how can I try it in the game? Also are you able to upload it to github then? Also can you check what happened with the separate session then?

Answered the same morning: how to run the build and where the switch is (press M, choose Small, tick "Gets up by its
own body", "Push it over"); both branches pushed, the learnt models committed on the work branch, the build itself not
in git (a GitHub release offered, on his word); the separate session's night-light fix finished on both branches.

## The second message, after playing it (verbatim)

The message was spoken and written down by his device; it is kept as it came.

> Okay, so I tested it and in some aspects it was good, but I noticed, yeah, it sometimes would take so long to get up, so that's not perfect yet. But also, when trying to get up, it would make a lot of weird movements, weird unnatural movements, like very aggressive twitches with the limbs. And sometimes especially more, the more demny stuff. Sometimes it would bend them in like ways that would make like a character's arms seem like it's broken or something like that, or even worse sometimes. It would twitch like for one millisecond completely out of frame, like it's really, really clipping and bugging out the limb. So I think those need to be corrected. I don't know how, maybe we need to add better limitations to how it can move. Also, however though, I am noticing that this task is taking much longer than anticipated. I mean, just taking a very long time in general, and I think it will still take a very long time. So at first I was wanting to limit its scale to the high-fi prototype, but now I think it might be better for us to just implement the full game version of it, since it's already taking so long, this version, and maybe otherwise we'll have to redo it later. Because for the full one, remember that the full game is like a modular civilization builder thing, where the units are also all customizable. So the player will usually fully customize the unit with, like, its own body and attributes. Like, it will have different body proportions and different strength and, I don't know, any other attribute that might be relevant to getting up. So the final get-up should adapt to any body proportion or strength. And like it needs to be the person this general algorithm that work for any, since not algorithm but just general method. I don't know how to call it since the person will customize the unit and then it shouldn't train a new one that takes a long time in the game after that to let it get up. It should work for any kind basically. Be super adaptable. So I think what we should do is you should do a super deep research to see how we can come up with that and do much more visual checks for all the what's happening in the game of every moment. So you can really see if it's natural all the movements and from all angles. And really do much more deep research, no angles possible. So, and it needs to feel natural because right now even when it gets up many times, it doesn't feel natural. It feels goofy, which is the thing that we were wanting to avoid in the movement. But I do still want it to be very procedural because I want the body proportions and the strength of it to have a real natural impact in how it gets up in an adaptive way. Also try to really do an in-depth study analysis of like references, not just study like the theory. Try to really see the visuals of all the real life cases frame by frame and all the animation and character ones from video games and see what fits our aesthetic that we're going for and our style, okay? Anyways. Yes, let's do it this longer route but after it, it will be perfect even for the final version, even if it doesn't apply so much to the high-fidelity prototype.

## What it says

| | Luis | What follows |
|---|---|---|
| As played | "in some aspects it was good"; "it sometimes would take so long to get up" | Liked in part; **not accepted**. Slow is a fault |
| Twitches | "very aggressive twitches with the limbs", "especially more" with "the more demny stuff" (heard so; most likely "dynamic") | A fault to find and end, in the game as played |
| Broken limbs | bends "that would make like a character's arms seem like it's broken", "or even worse sometimes" | A fault: no bend a living arm or leg could not make |
| One-frame glitches | "twitch like for one millisecond completely out of frame", "clipping and bugging out the limb" | A fault, and one the judges' pictures (every 0.08 s, on the bench) could not have shown: the game is to be read frame by frame as played |
| How | "maybe we need to add better limitations to how it can move" | His thought, to be weighed with what the faults turn out to be |
| Scope | "implement the full game version of it" rather than the high-fidelity prototype's; "it will be perfect even for the final version, even if it doesn't apply so much to the high-fidelity prototype" | **Direction from Luis:** the getting up is now made for the whole game, not only for Small |
| Any body | "the final get-up should adapt to any body proportion or strength"; "it shouldn't train a new one" after a player makes a unit; "It should work for any kind basically. Be super adaptable" | **Direction from Luis:** one way of getting up that works at once for every body the creator can make, its proportions, its strength and whatever else bears on it. Which attributes the creator has stays Open (the vision's register) |
| Procedural | "very procedural", "the body proportions and the strength of it to have a real natural impact in how it gets up in an adaptive way" | **Direction from Luis:** how a body gets up comes from that body; a different build gets up differently, and visibly so |
| Natural | "it doesn't feel natural. It feels goofy, which is the thing that we were wanting to avoid" | The vision's guardrail, restated: no comedy in the physics. The judges' "reads as real, with a flaw" was not his eye |
| Research | "a super deep research"; "no angles possible"; references "not just ... the theory": real life "frame by frame", and "animation and character ones from video games", for "what fits our aesthetic" | Asked: research that builds on October 14 and October 22, with the references themselves looked at frame by frame |
| Checking | "much more visual checks for all the what's happening in the game of every moment ... from all angles" | Asked: the game read at every frame, from all sides, as played |
| Time | "this task is taking much longer than anticipated"; "let's do it this longer route" | **Decided by Luis:** the longer route, done once for the final game |

Not changed by this message: never the backup animation in the game (October 22), the old way after 12 tries kept for
now (October 29), a body that is its own and not posed (October 9), no switch at a threshold (October 13), breath gentle
and scaled (October 10). The order of stages after the getting up is not said; it is asked again when this is done.
