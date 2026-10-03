# Model quality method — how every model is brought to its best

**Started:** October 3, 2026.

**Why:** Luis asked for "a proper strategy in a document" to keep and refer
to, so that the models are as polished as possible
([correspondence](../Correspondence/2026-10-03_PHYSICAL_OBJECTS_AND_A_QUALITY_METHOD.md)).
The strategy should:
- look at every little object and place from all angles;
- search each angle exhaustively for anything weird or worth polishing;
- research how to reach the highest polish;
- develop a method for raising model quality from now on.

**Applies to:** every model in the game. Beings come first (today the three
miners); buildings, tools and props follow.

**Companion pages:**
- [CharacterPractices.md](CharacterPractices.md): how pieces are built.
- This page: how a model is checked and improved until nothing is left to
  find.
- [VisualSoul.md](VisualSoul.md): what polish must never take away.

Luis judges taste, feel and soul. The method exists so that Luis never has
to hunt for defects.

## 1. The standard

A model is ready when all of these hold, standing and moving, at every
distance the game allows:

1. **Nothing floats.** Every object is held, worn, sewn, buckled, tied, hung,
   or standing on something, and the viewer can see how.
2. **Nothing passes through anything,** standing or in any phase of any
   movement.
3. **Objects obey gravity and motion.**
   - Weight hangs and sags.
   - A strap carrying weight pulls straight.
   - Loose things swing, lag behind, settle, and bump against the body
     instead of passing through it.

   Luis: the physicality and reality of all movement is one of the game's
   main focuses.
4. **Everything is made the way a maker would make it.**
   - A bag has a flap and rings for its strap.
   - A lace runs through eyelets.
   - A lantern has a bail that a hand closes round.
5. **It reads at every distance.**
   - From the Strategy camera, the silhouette and colours say who it is.
   - From the closest Explore view, every junction holds up.
6. **The soul stays.** Polish never makes a being generic. The stylization
   and appeal Luis loved stay ([VisualSoul.md](VisualSoul.md)).

## 2. Four questions for every object

Ask these of every object, and of every place where two things meet:

| Question | How to answer it | What it catches |
|---|---|---|
| **What holds it?** | Follow the chain of support from the object to the body or the ground. Every link must touch | Laces off the boot; a lantern below the fingers; a strap ending in the air |
| **How is it attached?** | Sewn, riveted, buckled, tied, looped, gripped: the attachment is modelled where it would show | A strap that meets no ring; a flap standing off its bag; a collar sitting on a coat |
| **What does gravity do to it?** | Where does it sag, hang, drape, or pull taut? | A bag standing stiff like a board; a slack strap carrying weight |
| **How does it move?** | Rigid with a bone, bending with the body, swinging, colliding | A bag sinking into the hip mid-stride; a boot shaft bending away from the leg |

## 3. The method, in seven passes

Each pass feeds the next. Every finding goes into the round's findings log
(section 7). Passes 3 to 6 repeat until a full round finds nothing.

### Pass 1 — Inventory

1. **List every object and every junction.** A junction is any place where
   two things meet:
   - skin and cloth (neck and collar, wrist and cuff);
   - cloth and cloth (layers, hems);
   - cloth and hardware (buttons, buckles, rings);
   - strap and object;
   - hand and object;
   - object and ground.
2. **Write each junction's expected state:**
   - **touching**, and how close;
   - **covered**: which one is inside, and by how much;
   - **clear**: how much space;
   - **moving**: rigid, bending, or swinging.

The miners' junction map is in [appendix A](#appendix-a--the-miners-junction-map).
The audit's relations (pass 3) are the same list, written in code.

### Pass 2 — Construction

For each object:
1. **Describe how it would be made and worn in reality:** its pieces, seams
   and attachments. Use reference where needed.
2. **Check that the model shows it.** Stylization may simplify the
   construction. It must not remove the parts that explain how the object
   holds together.

For example, a satchel has a body, a flap folded over from the back, a
closure, and rings sewn to its sides. The strap's ends loop through the rings
and are stitched back on themselves.

### Pass 3 — Geometry audit at rest (automatic, in Blender)

After every build, the audit measures the separate parts before they are
joined:

| Check | What it measures | Fails when |
|---|---|---|
| **Floating** | Every connected piece of every part against all the other parts | A piece is more than 3 mm from everything else. This is the catch-all for anything left in the air |
| **Contacts** (`touch`) | Places that must touch: laces on the boot, strap ends in their rings, a bag resting on the coat, a handle inside a closed hand | The largest gap exceeds its tolerance |
| **Layers** (`outside`) | Every point of an outer part near an inner part must lie on the inner part's outer side | Any point is deeper than its tolerance. The deepest point is reported |
| **Technical** | Triangle budgets; inside-out faces; carried things rigid on one bone; at most four bones per vertex | Any rule is broken |

Each failure prints the parts, the worst value and where it is. Failures are
fixed before anything is captured.

### Pass 4 — Pose sweep (automatic)

This is the same audit, in motion.

1. **The game records its own movement.** Every miner walks from standing,
   along a straight path and round a turn, and stops. Every bone is recorded
   30 times a second.
2. **Blender replays the recording.** It poses the parts with each recorded
   frame, runs the pass 3 checks on every frame, and lists the worst frames.
3. **Extreme poses** are added too (range-of-motion testing, as riggers do):
   - the longest stride;
   - arms swung fully;
   - a deep knee bend;
   - the head turned.

Problems like these show here before anyone looks:
- clothes bending away from what they cover;
- legs leaving their boots;
- a swinging lantern passing through a leg.

### Pass 5 — The exhaustive look (in the engine)

The model is captured in the game itself: rigged, in the game's own light,
with every angle in the capture matrix (section 4).

**Read every image, one by one, in this order:**
1. **Silhouette,** also blacked out. Does it read? Look for tangents (edges
   touching awkwardly), anything sticking out, and anything lost.
2. **Every junction in view,** from the junction map, top to bottom. Ask the
   four questions of each.
3. **Surfaces:**
   - paint sharpness and stretching;
   - seams;
   - colour and value;
   - faces.
4. **Light:**
   - lamps lighting what they should;
   - outline artefacts;
   - flicker where two surfaces fight.
5. **Fresh eyes.** Read the same images again mirrored, in grey values, and
   as flat clay. Mirroring shows errors that familiarity hides.

**Rules:**
- Write every finding down at once, with the image it came from, however
  small.
- The search is exhaustive. An image is done only when every junction in it
  has been looked at.

### Pass 6 — Fix at the root

1. **Fix in the module,** never one view by hand: body.py, outfits.py,
   rigging.py, or the runtime. Then every model that uses the piece is fixed.
2. **When a piece changes, check again everything placed against it.** For
   example, laces lie on the boot and straps lie on the coat. Place such
   things by laying them on the real surface, never by measurements that can
   go stale.
3. **Re-run passes 3 to 5.** Capture the same views again, and keep before
   and after pairs.

### Pass 7 — Record and learn

- **Close the findings log** (section 7).
- **Send before and after images** to Luis.
- **Update the misses ledger** (section 8). Whenever Luis finds something the
  method missed:
  - record why it was missed;
  - add a check, a view or a question so that this kind of flaw is caught
    from then on.

  The method grows with every miss.

## 4. The capture matrix

| Set | What | Views |
|---|---|---|
| **Distances** | The Strategy camera's height, the Explore default, Explore's closest, and macro (30 to 50 cm) | Front three-quarters, at dusk and at midday |
| **Orbit** | The whole figure, at Explore's closest | 8 directions × 3 heights: ground level, eye level, and from above at 45° |
| **Zones** | Each junction zone, in macro ([appendix A](#appendix-a--the-miners-junction-map)) | At least 6 directions, including from below and from behind |
| **Walk** | 8 phases of one stride, walking steadily | Side, front and back, plus macro on the feet, hands and carried things |
| **Start, stop, turn** | Follow-through as the miner starts, stops and turns | Side and three-quarters |
| **Fresh eyes** | Silhouette (black on white), clay (no paint), mirrored, grey | From the orbit set |

Images are assembled into contact sheets per set. Every sheet is kept with
the round's findings log.

## 5. What to look for (the defect catalogue)

| Kind | Examples |
|---|---|
| **Floating** | Laces off the boot; a carried thing below the fingers; a strap ending in the air; a button off the cloth |
| **Clipping** | A hand through the coat; a bag into the hip; a lantern through a leg; hair through a cap |
| **Gaps** | Skin or void between collar and neck, sleeve and wrist, or boot and trousers |
| **Out of line** | A leg beside its boot; a strap off the shoulder; a sleeve twisted |
| **Missing construction** | A strap with no ring; a flap with no hinge; laces with no eyelets |
| **Against gravity** | A bag standing stiff; a strap slack under weight; an apron like a board |
| **Wrong motion** | Something rigid that should swing; something bending like rubber; popping; jitter |
| **Deformation** | Elbows and knees collapsing; wrists twisting; cloth stretching at the shoulders |
| **Surface** | Blurry or stretched paint; seams; a face fading; values too close together |
| **Readability** | Tangents; clutter; a silhouette that no longer says who it is |
| **Light** | Lamps lighting the wrong places; broken outlines; flicker |
| **Distance** | Details popping between levels of detail; attachments lost far away |

## 6. Severity and done

**Severity:**
- **A — breaks reality.** Floating, clipping, gaps, things out of line, an
  object that ignores gravity, visible from Explore's closest. Never shown
  to Luis.
- **B — reads wrong.** Construction that does not make sense, wrong motion,
  deformation, poor readability.
- **C — refinement.** Surface, proportion, finer construction.

**Done means all of these:**
- The automatic audit passes at rest and on every recorded frame.
- Every junction has been seen in macro from at least six directions,
  standing and walking, with nothing open at severity A or B.
- The orbit, distance and fresh-eyes sets have been read.
- Luis's notes are answered with before and after images.
- The findings log and the misses ledger are written and pushed.

## 7. The findings log

Every round writes one log, in its section of the model's page (for the
miners, [TheMiners.md](TheMiners.md)):

| ID | Model | Zone | Seen in | Severity | Finding | Cause | Fix | Checked |
|---|---|---|---|---|---|---|---|---|

## 8. The misses ledger

Things Luis found that the method missed, why they were missed, and what
catches them now.

| Date | What Luis saw | Why it was missed | What catches it now |
|---|---|---|---|
| Oct 3 | Small's lantern floating by the hip | Nobody asked "what holds it?". The rest pose moved it to "the belt", with nothing modelled there | The four questions; the floating check |
| Oct 3 | Small's satchel strap not meeting the bag; the bag clipping | The bag was placed by measurements and the strap by guesses | Laying on surfaces; the contact check from strap end to ring |
| Oct 3 | Small's neck still odd | The neck was only seen from the front | The neck zone, from six directions |
| Oct 3 | Small's legs not lining up with the boots | The feet were seen from 90 cm, in one walking frame | The boot zone at ground level; the walk phases; the pose sweep |
| Oct 3, evening | The laces' crosses floating | The boot shaft was slimmed, but the laces kept the old measurements, and nothing checked them again | Laces laid on the boot's surface; the floating check; re-checking everything placed against a changed piece |
| Oct 3, evening | The leg still beside the boot | The boots were never seen at ground level while walking | The pose sweep (trousers outside the shaft on every frame); boot views from ground level during the walk |
| Oct 3, evening | The strap still not joined to the bag | Tabs were added, but the strap was never made to pass through them | The construction pass; the contact check |
| Oct 3, evening | The lantern floating up close | The grip was a guessed point that was never measured, or seen closer than a metre | The contact "handle inside a closed hand"; the hand zone in macro |
| Oct 3, evening | The bag not hanging with gravity | The bag was rigid on the pelvis; nobody asked what gravity does to it | Question 3; the bag hangs and swings from its strap |

## 9. Research behind the method

- **Silhouette.** Fill the character with black and check that it still
  reads ([Hitem3D](https://blog.hitem3d.ai/blog/Game-Character-Design-Principles-Process-and-Tips-for-2026)).
  Turnarounds under back light compare mass and negative space without paint
  ([Artfolio](https://www.artfolio.com/article/character-turnaround-sheets-impress-recruiters-with-silhouette-clarity)).
- **Turntables and wireframes** reveal angles that one portrait hides
  ([Polycount](https://polycount.com/discussion/comment/1138177)).
- **Range-of-motion and extreme-pose tests.**
  - Riggers sweep shoulders, elbows, wrists, hips, knees and the neck through
    their range.
  - They look for collapsing volume, twisting and stretching.
  - Errors hide until a limb is rotated through its full range
    ([Pixune](https://pixune.com/blog/rigging-and-skinning-pipeline-for-characters/),
    [Seeles](https://www.seeles.ai/features/tools/dcc-rigging-skinning-weights-checklist)).
- **Automatic geometry checks.**
  - Blender's BVH trees answer nearest-point, overlap and ray queries
    ([Blender API](https://docs.blender.org/api/4.3/mathutils.bvhtree.html)).
  - The 3D-Print Toolbox checks intersections, non-manifold edges and
    distorted faces
    ([Blender manual](https://docs.blender.org/manual/de/4.1/addons/mesh/3d_print_toolbox.html)).
  - Studio pipelines run pass/fail validators on budgets, UVs and naming
    before export
    ([Polycount](https://polycount.com/discussion/238781/blendermcp-pro-ai-pipeline-tool-for-blender-asset-validation-batch-export-game-engine-prep)).
- **Visual bugs QA looks for:** clipping, z-fighting (flicker), seams and
  popping ([Globant](https://stayrelevant.globant.com/en/technology/gaming/varieties-game-bugs/)).
- **Secondary motion.**
  - Accessories should react to the body's movement: a backpack bounces with
    each step, a ponytail swings as the body turns.
  - Points that enter the body are pushed out to its nearest surface
    ([Procedural Animation in Games](https://www.abratabia.com/game-animation/procedural-animation.php)).
  - Follow-through and overlap give weight and gravity, and apply to props
    too ([CG Wire](https://blog.cg-wire.com/follow-through-overlapping-action)).
- **Grips.** A hand holding something follows grip logic: the fingers wrap,
  the palm makes contact, and the thumb locks
  ([PoseMyArt](https://posemy.art/blog/drawing-hand-holding-object-guide/)).
- **Construction.** A messenger bag's strap fastens to D-rings, held in
  fabric loops stitched to the side panels
  ([Cut Out + Keep](https://www.cutoutandkeep.net/projects/messenger-bag-7.html)).
- **Fresh eyes.** Painters flip the canvas, squint and look in grey values to
  see past familiarity
  ([Draw Paint Academy](https://drawpaintacademy.com/painting-tests/)).
  Tangents, edges touching awkwardly, flatten depth and confuse shapes
  ([3D Artist](https://3dartist.substack.com/p/the-art-of-tangents-mastering-intentional)).
- **Iteration.** At Pixar, shots go to dailies each day and may be sent back
  three or four times. Notes add to the work ("plussing") rather than knock
  it down ([Frame.io](https://blog.frame.io/2018/06/18/making-incredibles-2/)).

## Appendix A — The miners' junction map

Each zone is captured in macro from at least six directions, standing and
walking. The expected state follows each junction.

| Zone | Junctions | Small | Long | Round |
|---|---|---|---|---|
| **Head** | Hair on the scalp, covering the nape (touching); cap on the head (touching, no hair through it); ears; the painted face | Bob | Swept hair | Curls under the lamp cap; the lamp's mount on the cap |
| **Neck** | Neck rising from behind the jaw; collar or neckband round the neck (no gap, no void, from every side) | Closed collar | Open collar, lapels, shirt front | Neckband |
| **Shoulders and back** | Straps lying on the cloth over the shoulder; things slung on the back | Satchel strap | Bandolier; the pickaxe on the back (held by the strap) | Apron straps crossing at the back |
| **Chest and waist** | Buttons sewn on; patches flat on the cloth; apron bib and ties | Three buttons; strap over the buttons | Patch on the chest; bandolier over the lapels | Bib; ties knotted at the back |
| **Arms and hands** | Sleeve or cuff round the wrist (no gap); fingers; hands clear of the coat | Cuffs | Cuffs | Rolled sleeves; bare forearms |
| **Carried things** | The hand closes round the handle (touching all round); the object hangs below with gravity and swings | Lantern in the left hand | Mug in the left hand | — |
| **Bag** | Strap ends through rings on the bag (touching); flap folded over the top (touching); bag resting against the hip, hanging with gravity, never inside the coat | Satchel at the right hip | — | — |
| **Hips and hem** | Skirt over the trousers (covered); hem clear of the legs; apron over the smock (clear 8–20 mm); pocket with hammer | Coat skirt | Open coat skirt; patch | Smock skirt; apron; hammer in the pocket |
| **Legs and boots** | Trousers falling over the boot's shaft (covered, all round, every frame); laces on the boot through eyelets (touching); soles on the ground | Boots, laces | Tall boots, laces | Boots, laces |
