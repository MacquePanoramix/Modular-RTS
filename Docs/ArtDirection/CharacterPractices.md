# Character practices — how Wonder Gather's beings are built

**Started:** October 3, 2026, after Luis asked for "a good blender research on
good practices" and for proactive polish, so that Luis does not have to point
out every strange little detail
([correspondence](../Correspondence/2026-10-03_SMALL_POLISH_AND_NATURAL_GAITS.md)).

**Extended:** October 4, after Luis restated that anything that reads as an
object must behave as a proper object
([correspondence](../Correspondence/2026-10-03_PHYSICAL_OBJECTS_AND_A_QUALITY_METHOD.md)).

**Applies to:** every being built from the modules in `Art/Blender/Worker/`
(today the three miners), and every future unit.

This page records the practices we follow and where each comes from. How
every model is checked and improved, before Luis sees it, is the
[model quality method](ModelQualityMethod.md).

## 1. Build practices (Blender)

### Body and clothes

| Practice | Why | How it is done here |
|---|---|---|
| **Remove geometry no one can see** under clothes | Hidden body or garment parts poke through what covers them when the body moves. This is the common game-art practice ([Polycount](https://polycount.com/discussion/comment/2698752), [CG Cookie](https://cgcookie.com/community/7103-how-do-you-rig-clothes-your-process)) | Trousers under a closed coat or smock are cut away above the knee (`outfits.trousers(hidden_above=…)`). Skin is only made where it shows: head, neck, hands, bare forearms. A head is one piece: sealed bubbles inside it are removed (`shapes.keep_largest`) |
| **What lies on something takes its weights** | Clothing whose weights differ from what it lies on moves apart from it and cuts through it. The common fix is to transfer the weights ([Blender Artists](https://blenderartists.org/t/problem-with-rigging-belts-and-straps-on-a-character/694725), [Cyberpunk modding wiki](https://wiki.redmodding.org/cyberpunk-2077-modding/for-mod-creators-theory/3d-modelling/weight-painting-for-gonks)) | `rigging.LIKE` and `weights_like`: laces and eyelets take their boot's weights, point by point, from the nearest place on its surface. So do straps, buttons, patches, the apron's strap and ties, from the clothes they lie on. A rigid thing hung on cloth (a hammer in its loop) takes the weights of the one point it hangs from, so it keeps its shape |
| **The neck grows from behind the jaw** | Necks that start under the chin look glued on. The neck's muscles start behind the jaw, under the ear ([Polycount](https://polycount.com/discussion/comment/1527461), [Anime Outline](https://www.animeoutline.com/how-to-draw-anime-neck-shoulders/)) | `body.head`: the neck is part of the head's form, rising from behind the jaw. At the neckline it is no wider than the garment's opening, so skin never shows through a neckband |
| **A collar closes round the neck** | A gap between coat and neck shows skin or the void behind | The garment's neckline closes round the neck above the collar bones. The collar is one strip from inside the coat up the neck and folding out |
| **A neckband covers the join of skin and cloth** | Where a neck comes out of cloth at a shallow angle, the two surfaces nearly lie on each other and show as slivers | `outfits.neckband`: one seamless band on the garment's neckline. Where the join of skin and cloth is not under it (the back of a round neck), the band moves to the join |
| **The boot's shaft follows the shin** | The shin leans forward from the ankle even standing. A shaft that stands straight up leaves the leg beside the boot | `body.boot`: the shaft runs from the ankle towards the knee and hugs the leg (`body.shaft_radius`). The trousers fall over its top |
| **A skirt hangs in flaps** | A coat that follows the thighs only partly lets the knee through; one that follows them wholly is dragged after a leg that has left it | The skirt has four bones at the hips: front and back of each leg (`rigging.joints`). The game turns a flap with the thigh that moves into it, and lets it hang when the thigh moves away (`MinerBody.Drape`). From a hand's width above the hem down, each flap is wholly its own |
| **Cloth that is not touched hangs still** | A long coat hangs well clear of the legs behind. Moved with every degree of the thigh, its back flies out like a bell | `outfits.skirt_slack` measures how far a thigh swings before the leg reaches the cloth, to the front and to the back. For a long open coat the game's flaps wait that long (`miners.json`, `skirtSlack`). A short closed skirt moves with the legs from the first: that is the movement Luis liked on Small, kept as it is |
| **A long coat lies over the knees** | The knees stand a little forward even at rest. A slim coat cuts through them; a coat grown all round to clear them becomes a bell | `outfits.skirt`: below the thighs, the cloth goes out only where a leg is, and only as far as that leg asks. Elsewhere it keeps its measured shape |
| **An apron hangs from its strap and is drawn in by its ties** | Leather hanging from the belly's fullest point stands off a narrower waist, and ties cannot reach it | `outfits.apron`: down to the ties it lies on the clothes, point by point; below them it hangs from the waist, never tucking back under. The strap goes round the back of the neck; the ties run to a knot at the back |
| **A patch follows the cloth** | A flat slab on curved cloth stands off it at its edges | `outfits.patch`: every point of the patch is laid on the cloth. Patches are as thin as cloth |
| **One rest pose** | Skinning works best in a neutral pose with arms apart | `workers.rest_preset`: feet under the hips, arms 25° out, head straight. A hand that carries something is closed round its handle |

### Things held, worn and carried

| Practice | Why | How it is done here |
|---|---|---|
| **A hand closes round a handle** | A fist closed on nothing leaves no room for a handle; a handle placed at a guessed point floats beside the fingers. A grip follows grip logic: the fingers wrap, the thumb locks ([PoseMyArt](https://posemy.art/blog/drawing-hand-holding-object-guide/)) | `body.hand(bar=…)`: each finger is wrapped round a bar of the handle's radius, joint by joint, along the tangents to it (`body.wrap`). The thumb closes the hook. The hand is then cut to fit the bar exactly, so the handle touches the fingers all round and never passes through them |
| **A free hand closes on what it is given** | A tool is not always in the hand, and tools differ in thickness. A hand fused shut round one handle cannot let go or take another | `hands.py`: three bones for each finger and the thumb. The build closes the skinned hand round handles of six thicknesses (each finger until the skin of its outer bones touches; the thumb until it meets the handle or a finger) and writes the closings as a table; the game blends between them. A hand that only ever carries one thing is still modelled closed on it and cut to fit |
| **A part that bends gets points to bend at** | A hand given 600 triangles by its size folds when fifteen joints bend it | `rigging.HAND_TRIANGLES`: a closing hand keeps 1,600 triangles at the nearest level, outside the shared budget. The farthest level has no skin on finger bones |
| **A carried thing hangs from its handle** | What holds it? The hand. What does gravity do? It hangs straight down from the grip | Small's lantern: a wooden grip in the fist, on a wire bail hinged at two ears on the cap. Long's mug: carried by its handle, hanging on its side below the hand, as an empty mug does. Each has a bone of its own at the grip |
| **A strap bearing a load is pulled taut** | A strap carrying weight runs in straight lines and smooth curves | `outfits.taut` (Long's sling, Round's apron strap): the strap is laid on the clothes' real surface, then drawn towards a straight line and laid again, over and over. It is pinned where it bears (the shoulder, the back of the neck): left free, the shortest path between two rings slips down round the waist. Over its last stretch it leaves the cloth and runs straight to its ring. Where the cloth curves under it (a collar's rim), it rides flat over the highest point |
| **A light bag's strap is worn soft** | Luis preferred Small's strap as it first was (October 5): out on the shoulder, a broad band lying easy on the coat, below the collar. Pulled taut, it sat nearer the neck and climbed over the collar | `outfits.worn` (Small's satchel): every point is laid on the coat along the course it is given, not drawn tight; the collar falls over it where they meet. Its ends still leave the cloth and run straight to their rings (`outfits.release`) |
| **A bag is made as a saddler makes one** | A strap's fastening explains how the bag hangs ([Cut Out + Keep](https://www.cutoutandkeep.net/projects/messenger-bag-7.html)) | `outfits.satchel`: a soft body, fuller at the bottom where its load settles; a flap sewn along the top's back edge; a tongue and buckle; a tab and a brass ring at each end of the top. The strap's ends pass through the rings and are sewn back on themselves. The bag leans on the coat's real surface and has a bone of its own at its rings |
| **A thing on the back hangs in loops** | A pickaxe held behind a back by nothing floats | `outfits.sling`: a strap worn across the body; two leather loops sewn to its back; the handle passes through the loops, and the pick hangs by its head, which rests on the upper loop |
| **A tool at the hip hangs in a loop** | A hammer "in a pocket" that is a flat patch is held by nothing | `outfits.hammer`: a leather loop sewn to the apron holds the handle a little off the cloth; the handle hangs down through it, resting on the apron; the head lies across the loop and rests on it. In the game it swings out from the body and back, not sideways (its head across the loop is a hinge), and the place it hangs from moves with the cloth the loop is sewn to |
| **Laces are threaded** | Laces placed by measurements float as soon as the boot changes | `body.lacing`: pairs of eyelets up the instep and the shaft; the lace crosses between them, every point laid on the leather; a bow at the top, its ends hanging down the front |
| **Thin things keep their shape** | Making a model lighter crushes thin wires, laces and rings | `shapes.exact`: laces, eyelets, rings, wire and turned things (a mug, a lamp) are not simplified. The finest (laces, eyelets) are left out from the middle level of detail on |

## 2. In the game: objects that answer to gravity and motion

| Practice | Why | How it is done here |
|---|---|---|
| **A hanging thing is a pendulum** | Follow-through and overlap give weight ([CG Wire](https://blog.cg-wire.com/follow-through-overlapping-action)). Accessories should react to the body's movement ([Procedural Animation in Games](https://www.abratabia.com/game-animation/procedural-animation.php)) | `MinerBody.Hang`: the lantern, the mug, the satchel and the hammer each hang from their own bone, pulled by gravity |
| **Its weight is kept as a small offset and a speed** | Kept as two places in the world, the weight loses gravity at very high frame rates: far from the world's middle, a place cannot be told apart from one a few thousandths of a millimetre away, and that is more than gravity moves it in one such frame | The pendulum's state is where the weight is under the place it hangs from, and how fast it moves. Both are small numbers anywhere in the world |
| **Its swing fades against what holds it, not against the world** | A swing damped against the world trails behind a walking body as if in a wind. Carried steadily along, a thing hangs straight | Only the swing relative to the place it hangs from fades. The movement shared with that place is kept |
| **The body stops it, and it rests there** | A point that enters the body is pushed out to its surface ([Procedural Animation in Games](https://www.abratabia.com/game-animation/procedural-animation.php)). A push kept as swing makes the thing bounce off the coat | Each thing keeps its distance from the pelvis along a direction measured on the model (`miners.json`, `stopNormal`, `stopDistance`). A bag rests on its hip at exactly that distance; the flap of the skirt beneath pushes it out as the leg moves (`pusher`). What the body stops loses its speed towards the body |
| **The wrist gives, and the give is only shown** | A handle turning in a closed fist cuts through the fingers. But the give moves the handle: fed back to the pendulum as if the arm had moved, it locks the load tilted | The hand turns at the wrist so that the handle stays square to the way its load hangs. Past what a wrist can give (32°), the load swings no further that way. The pendulum itself hangs from where the arm carries the handle, before the give |
| **The carrying arm hangs clear** | A lantern beside a coat needs room, through the whole stride | Each arm has its own carriage (`ProceduralBiped.Proportions.armCarry`, `armSwingSide`). A carrying arm swings less, and hangs far enough out that its load hangs straight even where the stride brings the hand nearest the hips (about a tenth of the arm's drop, measured on the recorded walks). The arm on the bag's side hangs clear of the bag |
| **A boot goes round the other** | In a sharp turn the feet's paths cross, and the two boots point different ways | `ProceduralBiped`: boots are measured as boots, from heel to toe with their width. A swinging boot's path bows out round the standing one, on the nearer side, and a boot never lands on the other |

## 3. Walks natural to each body

Luis: "Their bodies should walk in a way that feels natural to their
appearance." Animation practice reads a character's weight and personality in
the walk's timing, bounce, sway and arm swing
([Adobe](https://www.adobe.com/in/creativecloud/roc/blog/video/animation-walk-cycle.html),
[Animation Mentor](https://www.animationmentor.com/?p=98617)). Biomechanics
gives the pace: a comfortable walk sits near a Froude number of about 0.25,
whatever the size ([NCBI](https://www.ncbi.nlm.nih.gov/pmc/articles/PMC12592974/)).
So each miner walks at the speed its own legs give at its walk's Froude number:

| Miner | Froude | Pace | Bounce | Sway | Arm swing | The walk |
|---|---|---|---|---|---|---|
| **Small** | 0.27 | 1.27 m/s | 1.35× | 0.9× | 1.15× | Brisk, quick and bouncy, arms swinging freely |
| **Long** | 0.21 | 1.38 m/s | 0.75× | 0.8× | 1.25× | Long, smooth, unhurried strides, little bounce, long loose arms |
| **Round** | 0.24 | 1.28 m/s | 1.1× | 1.7× | 0.75× | A rolling side-to-side walk, arms swinging less round a round body |

- **Steps.** Small takes the quickest steps and Long the longest; Long
  covers the most ground for the least effort.
- **Where the values live.** They are in `workers.py` (`gait`), travel in
  `miners.json`, and reach the body as `ProceduralBiped.Proportions.bounce`,
  `sway` and `armSwing`. All three are 1 for the original test body.

## 4. What we learned the hard way

- **Small's lantern floated beside the hip** (Luis, October 3). The rest pose
  moved it to "the belt", but no belt or hook was modelled. **Practice:** a
  thing is either carried by a hand, worn on something modelled, or not
  there.
- **Small's body walked away from Small.** After choosing a miner, the body
  was still standing where that miner was last shown. **Practice:** a body
  that is moved stands up afresh where it is (`ProceduralBiped.ResetPose`).
- **The satchel's strap missed the bag.** The bag was placed by measurements,
  and the strap by guesses. **Practice:** the strap ends on its rings.
- **A hole at the back of the neck.** The garment ended in a wide opening and
  the collar sat above it. **Practice:** the neckline closes round the neck,
  and the collar grows out of it.
- **The laces floated after the shaft was slimmed** (Luis, October 3,
  evening). They kept the old measurements. **Practice:** what lies on a
  surface is laid on that surface, and checked again when the surface
  changes.
- **The leg stood beside the boot.** The shaft was modelled upright; the
  shin leans. From the front it looked right. **Practice:** build along the
  bone, and look from the side, at ground level.
- **The lantern floated below the fist.** The grip was a guessed point, and a
  closed fist had 2 to 4 mm of room. **Practice:** wrap the hand round the
  handle, and measure the contact.
- **The taut strap slipped to the waist.** Drawn tight with nothing to bear
  on, it took the shortest path between its rings. **Practice:** a strap is
  pinned where it bears.
- **The taut strap was not what Luis wanted on Small's shoulder** (October 5).
  It was more "correct" and less liked. **Practice:** change only what was
  asked; a rule of ours never outranks a look Luis already has.
- **The lantern trailed behind the walk as if in a wind.** Its swing was
  damped against the world. **Practice:** damp a swing against what holds
  the thing.
- **The "floating ears" were bubbles.** The audit flagged a piece 5 mm from
  the head: a 1 cm bubble sealed inside it, left by the ear's hollow.
  **Practice:** read what a check found before fixing it; here the fix was
  to remove the bubble, not to move the ear.
- **The lantern leaned backward, standing still.** Two guesses at the cause
  were wrong. The recorded frames showed it: the wrist's give had been fed
  back to the pendulum. **Practice:** measure in the game before explaining;
  keep what is simulated apart from what is only shown.
- **The long coat became a bell.** Clearing the knees by growing the whole
  ring asked for a depth without end. It grew over several rounds, and showed
  at once beside the first capture. **Practice:** change the cloth only where
  the leg is; compare each round with the "before" sheets.
- **The hammer slid sideways along its stop.** The stop was a flat place on a
  round body, so the lowest point was off to the side, and the hammer's top
  tipped into the smock. **Practice:** give a thing the freedom its fastening
  gives it (a hinge), and no more.
- **A test measured the walk, not the lantern.** It read the bones after the
  miner had walked on. **Practice:** a test reads one moment; the body reports
  what it last posed.
