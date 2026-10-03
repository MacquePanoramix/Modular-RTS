# Character practices — how Wonder Gather's beings are built and checked

**Started:** October 3, 2026, after Luis asked for "a good blender research on
good practices" and for proactive polish, so that he does not have to point
out every strange little detail
([correspondence](../Correspondence/2026-10-03_SMALL_POLISH_AND_NATURAL_GAITS.md)).
**Applies to:** every being built from the modules in `Art/Blender/Worker/`
(today the three miners), and every future unit.

This page records the practices we follow and where each comes from. How
every model is checked and improved, before Luis sees it, is the
[model quality method](ModelQualityMethod.md).

## 1. Build practices (Blender)

| Practice | Why | How it is done here |
|---|---|---|
| **Remove geometry no one can see** under clothes | Hidden body or garment parts poke through what covers them when the body moves. This is the common game-art practice ([Polycount](https://polycount.com/discussion/comment/2698752), [CG Cookie](https://cgcookie.com/community/7103-how-do-you-rig-clothes-your-process)) | Trousers under a closed coat or smock are cut away above the knee (`outfits.trousers(hidden_above=…)`). Skin is only made where it shows: head, neck, hands, bare forearms |
| **Layers lying on each other take the same weights** | Clothing whose weights differ from what it lies on moves apart from it and cuts through it. The common fix is to transfer the body's weights to the clothes ([Blender Artists](https://blenderartists.org/t/problem-with-rigging-belts-and-straps-on-a-character/694725), [Cyberpunk modding wiki](https://wiki.redmodding.org/cyberpunk-2077-modding/for-mod-creators-theory/3d-modelling/weight-painting-for-gonks)) | `rigging.candidates`: the collar, lapels, buttons, neckband, straps and apron ties take the same bones by the same rule as the top beneath them. Skirts and aprons follow the torso above the waist exactly as the top does, and blend to the thighs below |
| **Straps and belts lie on the clothes** | A strap placed by guesswork floats or sinks as the cloth beneath changes | `outfits.lay_on` casts every point of a strap onto the real (smoothed) surface of the garments beneath. Points that find nothing are placed between neighbours that did, so a strap never floats |
| **Rigid things are rigid** | A carried thing weighted to several bones bends like rubber | A satchel, its buckle and the strap's tabs are rigid on the pelvis. A pickaxe on the back is rigid on the chest |
| **Carried things are carried** | A thing hung in mid-air by a hip looks floating as soon as the body moves | Small's lantern is held by its bail in the left hand, on a bone of its own under the hand, and swings like a pendulum in the game (`MinerBody.Swing`). Long's mug is held in the left hand |
| **The neck grows from behind the jaw** | Necks that start under the chin look glued on. The neck's muscles start behind the jaw, under the ear, and the neck widens towards the collar bones ([Polycount](https://polycount.com/discussion/comment/1527461), [Anime Outline](https://www.animeoutline.com/how-to-draw-anime-neck-shoulders/)) | `body.head`: the neck is part of the head's form, rising from behind the jaw, slender at the top and widening into the collar. A soft hollow under the chin keeps the jaw readable |
| **A collar closes round the neck** | A gap between coat and neck shows skin or the void behind | The garment's neckline closes round the neck above the collar bones. The collar is one strip from inside the coat up the neck and folding out, so nothing shows between coat, collar and neck |
| **Leg and boot read as one line** | A thin leg in a wide boot shaft looks like a stick in a bucket | The boot's shaft hugs the leg (`body.shaft_radius`). The trousers fall over the shaft's top |
| **Hang close, clear just enough** | An apron that stands off the body looks like a board | The apron is measured from the clothes beneath and hangs just clear of them (8 mm at the waist, 2 cm at the hem) |
| **One rest pose, hands free or holding** | Skinning works best in a neutral pose with arms apart; the game needs hands free for work, or visibly holding what they carry | `workers.rest_preset`: feet under the hips, arms 25° out, head straight. A hand that carries something curls round it |

## 2. Walks natural to each body

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

## 3. The close audit (before Luis sees anything)

**Superseded on October 3, evening,** by the
[model quality method](ModelQualityMethod.md): seven passes, automatic
geometry checks at rest and in the game's own movement, and an exhaustive
capture matrix. The first close audit is kept below as it was run.

Every character passes this in the engine, rigged, in the game's own look,
not only in Blender. Blender previews and static portraits hid flaws that
only appear rigged and moving: props on the wrong bone, layers moving apart,
hands through things.

1. **Run the capture:**

   ```
   Unity -batchmode -projectPath <project> -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MinerCloseCapture -captureOut <folder> [-captureMiner Small]
   ```

   It renders each miner at dusk, as close as the Explore camera goes:
   - standing from the front, back, left and right;
   - the face;
   - the neck from behind and from the front;
   - the hands;
   - the feet from the front and from behind;
   - mid-stride from the side, front and back, and the feet.

2. **Look for:**
   - floating parts;
   - clipping (anything through anything);
   - gaps (skin or void between layers);
   - legs and boots out of line;
   - straps that do not meet what they carry;
   - things carried by nothing;
   - wrong bend at knees and elbows;
   - hands through clothes;
   - faces fading;
   - a silhouette that no longer reads.
3. **Fix it in the modules,** so every character that uses the piece is
   fixed. Re-export, and capture again.
4. **Report** what was found and fixed beyond Luis's notes, with before and
   after.

## 4. What we learned the hard way

- **Small's lantern floated beside the hip** (Luis, October 3). The rest pose
  moved it to "the belt", but no belt or hook was modelled, and it was skinned
  to the pelvis. **Practice:** a thing is either carried by a hand, worn on
  something modelled, or not there.
- **Small's body walked away from her.** After choosing a miner, the body was
  still standing where that miner was last shown. **Practice:** a body that
  is moved stands up afresh where it is (`ProceduralBiped.ResetPose`).
- **The satchel's strap missed the bag.** The bag was placed by measurements,
  and the strap's path by guesses. **Practice:** the strap is laid on the
  cloth and ends exactly at the bag's top, at two tabs.
- **A hole at the back of the neck.** The garment ended in a wide opening and
  the collar sat above it. **Practice:** the neckline closes round the neck,
  and the collar grows out of it.
