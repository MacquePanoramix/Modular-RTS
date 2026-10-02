# Dusk details, and the worker's redesign — October 2, 2026

**From:** Luis (Game Director), after playing the build with the dusk details
(fireflies, the hearth, window glow).
**Attachment:** six screenshots from the Strategy camera at night (23:00).
- **Closer in:** fireflies thick over the meadow near the worker.
- **Zoomed out:** the house's warm light spreads strongly over the path and the
  grass around the door.

## Messages (verbatim)

> Okay I tested it! Honestly I loved it. I would leave all features on by default, maybe only adjustment is to reduce at least a little bit the frequency of fireflies. Like look, specially when more near the ground the amount went from gentle wonder inducing to a bit overwhelming. Also one slight thing that I noticed which probably should be corrected. Just I noticed when we are far away the lights suddenly look very strong and even spread a bit to the terrain nearby. But when we are more zoomed inn that extra brightness disappears. Now I really liked how it looked from far away, but it doesn't make a lot of sense that extra brightness seemingly disappear upon moving closer, so I think we should try to fix that also, primairily by adjusting how it looks when not super zoomed out since I liked the zoomed out version. I tried to take some screenshots and send you as reference.
>
> Also after these two, yes let's finally move to the player remodel/redesign. Do you still remember from the previous documents the design and aesthetic principles that led us to this, especially the ones that were related to people? Also do you still have access to that PDF aesthetic guide document? Please use them to implement this remodelling/redesign with heart and soul.

Mid-way through the fixes:

> Oh also, I noticed the fireflies also disappeared when too zoomed out. Together with the plan to make them more scarce, even if their light gets less obvious from far away, I would still like there to be an indication of them even when very zoomed out (you can implement in a way and with styalization that makes the scene usually still very manageable fps wise). Anyways sorry for interrupting your work. You're doing a good job, just thought of this remark to add mid-way.

On the principles for people:

> Oh the scarf was just part of examples, that shouldn't need to be one of the key details, especially since in the final rts we will have all kinds of units. I just wanted to make sure you understand this is supposed to be the base principles that we will apply as art style to all beings and things in this worlds we portrey.

## How it was recorded

- **The dusk details.** All three are on by default: fireflies, the hearth
  and window glow.
- **Fewer fireflies.** They now number 600 instead of 1,100, and blink a
  little more sparsely.
- **Fireflies from far away.** They now live in a patch of meadow around
  where the camera looks, which widens as the camera pulls back. Each keeps at
  least a tiny painted dot, so from high above they read as a sparse scatter of
  lights instead of vanishing.
- **Lamplight close up.** It now matches the zoomed-out look he liked.
  - **The cause.** From far away the eye sees mostly grass tips, which local
    light lit about twice as strongly as the lower blade. Close up, the darker
    blade bodies, and soil shaded under the grass, hid the warm pool.
  - **The fix.** Lamplight reaches further down the blades and onto the soil;
    the tips are unchanged. Sun and moon shading under the grass stays as it
    was.
- **The worker's redesign (S1d) starts now,** from the Visual Soul handoff
  ([VisualSoul.md](../ArtDirection/VisualSoul.md)) and the principles for
  people.
- **The scarf is not a key detail.** It was only part of the examples. The
  Visual Soul's principles are the base art style for **every being and thing**
  in the world, across all the kinds of units the RTS will have. They are
  recorded as such in [VisualSoul.md](../ArtDirection/VisualSoul.md#the-language-for-every-being-and-thing).
