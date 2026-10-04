# Model quality method — how every model is brought to its best

**Started:** October 3, 2026. **First round:** October 3 and 4, on the three
miners.

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
- [TheMiners.md](TheMiners.md#the-methods-first-round-october-3-and-4): the
  method's first round, with its findings log.

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
   - A lantern has a handle that a hand closes round.
5. **It reads at every distance.**
   - From the Strategy camera, the silhouette and colours say who it is.
   - From the closest Explore view, every junction holds up.
6. **The soul stays.** Polish never makes a being generic. The stylization
   and appeal Luis loved stay ([VisualSoul.md](VisualSoul.md)).

## 2. Four questions for every object

Ask these of every object, and of every place where two things meet:

| Question | How to answer it | What it catches |
|---|---|---|
| **What holds it?** | Follow the chain of support from the object to the body or the ground. Every link must touch | Laces off the boot; a lantern below the fingers; a pickaxe behind a back with nothing under it |
| **How is it attached?** | Sewn, riveted, buckled, tied, looped, gripped: the attachment is modelled where it would show | A strap that meets no ring; a flap standing off its bag; apron straps that end above the bib |
| **What does gravity do to it?** | Where does it sag, hang, drape, or pull taut? | A bag standing stiff like a board; a strap in kinks; an apron hovering off a belly |
| **How does it move?** | Rigid with a bone, bending with the body, swinging, colliding | A lantern trailing behind as if in a wind; a knee through a coat; a boot through the other boot in a turn |

## 3. The method, in seven passes

Each pass feeds the next. Every finding goes into the round's findings log
(section 8). Passes 3 to 6 repeat until a full round finds nothing.

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
The audit's rules (pass 3) are the same list, written in code.

### Pass 2 — Construction

For each object:
1. **Describe how it would be made and worn in reality:** its pieces, seams
   and attachments. Use reference where needed.
2. **Check that the model shows it.** Stylization may simplify the
   construction. It must not remove the parts that explain how the object
   holds together.

For example, a satchel has a body, a flap sewn along the top's back edge, a
closure, and rings held by tabs. The strap's ends pass through the rings and
are stitched back on themselves.

### Pass 3 — Geometry audit at rest (automatic, in Blender)

Every build runs the audit (`audit.py`) on the separate parts, as they will
be exported, before they are joined:

| Check | What it measures | Fails when |
|---|---|---|
| **Floating** | Every connected piece of every part against all the other parts and pieces | A piece touches nothing (further than 3 mm from everything, and no surface crossing). A piece sealed inside a solid is hidden, not floating |
| **Lies on** | Every point of a part that lies on another: laces on their boot, ties on the smock, a patch on the cloth | Any point is further than its tolerance |
| **Meets** | Parts that must touch somewhere: a handle and the hand, a strap and each ring, a loop and the handle it holds, a bag and the hip | Their nearest points are further apart than the tolerance |
| **Sinks** | No point of a part inside another, solid part: a hand in a coat, a lantern in a leg, a strap in a collar. "Inside" is confirmed by counting the surfaces a ray crosses | Any point is deeper than its tolerance. The deepest point is reported |
| **Covered** | What is well inside another part at rest stays inside: a boot's shaft in the trouser leg | A covered point comes out |
| **Beneath** | What a skirt covers never comes out through it, seen along rays from the trunk's axis: trousers under a coat | A point is outside the skirt |
| **Technical** | At most four bones per vertex; solids not inside out; soles on the ground | Any rule is broken |

Each failure prints the parts, the worst value, where it is, and renders a
close view of the place. Failures are fixed before anything is captured.

**Read each failure before fixing it.** A check that reads wrong is itself a
finding: correct the check, and say so in the round's log.

### Pass 4 — Pose sweep (automatic)

This is the same audit, in motion.

1. **The game records its own movement** (`MinerPoseRecord`). Every miner
   stands, starts walking, walks straight, turns sharply and stops. For
   every frame (30 a second, 235 frames) it writes, for each bone, the
   matrix that skinning applies.
2. **Blender replays the recording.** It poses each part exactly as the game
   skins it, runs the pass 3 checks on every second frame, and reports the
   worst value for standing, walking, turning and stopping separately.

Problems like these show here before anyone looks:
- a knee coming through a coat;
- a carried thing swinging into the body;
- a handle turning in the closed fingers;
- one boot passing through the other in a turn.

**Not built yet:** extreme poses beyond the game's own movement (a deep knee
bend, arms raised), as riggers use to test a rig's whole range. They join the
sweep when the miners mine and haul.

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
6. **Against the before.** Read the orbit and the walk beside the same sheets
   of the build Luis last saw. A change that crept in over several rounds
   shows at once there, and nowhere else.

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
4. **Measure before explaining.** When something moves wrongly, read it from
   the recorded frames (which way, how much, in which phase) before choosing
   a cause.
5. **Keep what Luis liked.** A fix that would change a movement or a shape
   Luis has praised is made beside it, not through it.

### Pass 7 — Record and learn

- **Close the findings log** (section 8).
- **Send before and after images** to Luis.
- **Update the misses ledger** (section 9). Whenever Luis finds something the
  method missed:
  - record why it was missed;
  - add a check, a view or a question so that this kind of flaw is caught
    from then on.

  The method grows with every miss.

## 4. The capture matrix

`MinerCloseCapture` renders about 240 frames per miner:

| Set | What | Views |
|---|---|---|
| **Distances** | The Strategy camera's height (far and near), the Explore default, close, and macro | Front three-quarters, at dusk and at midday (10 frames) |
| **Orbit** | The whole figure, close | 8 directions × 3 heights: ground level, eye level, and from above at 45° (24) |
| **Zones** | Each junction zone in macro, 36 to 70 cm away ([appendix A](#appendix-a--the-miners-junction-map)) | 7 to 10 directions each, including from below and above. Zones at one side of the body are seen from that side (about 95) |
| **Walk** | 8 phases of one stride, walking steadily on the path | Both sides, front, back, each boot at ground level, the coat's hem, the carried thing from two sides, the bag (up to 88) |
| **Start, turn, stop** | Six moments of each | Side and three-quarters (36) |
| **Fresh eyes** | Silhouette (black on white) and clay (no paint) | 8 directions each, plus clay close-ups of boots, neck, the carried thing and the bag. The sheets add mirrored and grey copies of the orbit |

`Art/Review/sheets.py` assembles the frames into contact sheets: one per
zone, one per walk view across the eight phases, and so on. Every sheet is
read, and kept with the round's findings log.

The meadow's grass is put away for the close sets, because it stands between
a low camera and the boots. It stays for the distance set.

## 5. The tools

All commands run from the repository's root. `<Unity>` is the Unity 6000.6
editor, and `<project>` the Unity project.

| Step | Command | Output |
|---|---|---|
| **Audit at rest** | `blender -b --factory-startup --python Art/Blender/Worker/workers.py -- --out <folder> --audit [--only Small]` | `audit_<Name>.txt`, `.json`, and `snap_*.png` of each failure, in `<folder>` |
| **Record the game's movement** | `<Unity> -batchmode -projectPath <project> -runTests -testPlatform PlayMode -testFilter WonderGather.Tests.MinerPoseRecord -poseOut <poses>` | `Miner_<Name>_poses.json` |
| **Audit in motion** | The audit command, plus `--poses <poses>` | The same report, with the worst value per phase |
| **Build for the game** | `blender … workers.py -- --out Assets/_WonderGather/Art/Worker/Miners --rigged [--poses <poses>] [--report <folder>]` | The models, atlases and `miners.json`. The audit runs as part of every build. Its reports go to `Art/Review/Miners` (or `<folder>`), never among the game's assets |
| **Set up in the game** | `<Unity> -batchmode -projectPath <project> -executeMethod WonderGather.Editor.MinerSetup.CreateAll -quit` | The miners' prefabs and the scene, from the new models |
| **Capture** | `<Unity> … -testFilter WonderGather.Tests.MinerCloseCapture -captureOut <frames> [-captureMiner Small] [-captureSets zones,walk]` | PPM frames |
| **Sheets** | `python Art/Review/sheets.py <frames> <sheets>` | JPG frames and contact sheets |

A recording belongs to the build it was recorded from. After the models or
the walk change, record again before auditing in motion.

The last round's reports are kept in `Art/Review/Miners` (`audit_<Name>.txt`).

## 6. What to look for (the defect catalogue)

| Kind | Examples |
|---|---|
| **Floating** | Laces off the boot; a carried thing below the fingers; a strap ending in the air; a button off the cloth |
| **Clipping** | A hand through the coat; a bag into the hip; a lantern through a leg; a knee through a skirt |
| **Gaps** | Skin or void between collar and neck, sleeve and wrist, or boot and trousers |
| **Out of line** | A leg beside its boot; a strap off the shoulder; a sleeve twisted |
| **Missing construction** | A strap with no ring; a flap with no hinge; laces with no eyelets |
| **Against gravity** | A bag standing stiff; a strap slack or kinked under weight; an apron like a board |
| **Wrong motion** | Something rigid that should swing; a thing trailing as if in a wind; something bending like rubber; popping; jitter |
| **Deformation** | Elbows and knees collapsing; wrists twisting; cloth stretching at the shoulders |
| **Surface** | Blurry or stretched paint; seams; a face fading; values too close together |
| **Readability** | Tangents; clutter; a silhouette that no longer says who it is |
| **Light** | Lamps lighting the wrong places; a flame bleached white; broken outlines; flicker |
| **Distance** | Details popping between levels of detail; attachments lost far away |

## 7. Severity and done

**Severity:**
- **A — breaks reality.** Floating, clipping, gaps, things out of line, an
  object that ignores gravity, visible from Explore's closest. Never shown
  to Luis.
- **B — reads wrong.** Construction that does not make sense, wrong motion,
  deformation, poor readability.
- **C — refinement.** Surface, proportion, finer construction.

**Done means all of these:**
- The automatic audit passes at rest and on every recorded frame, or each
  remaining failure is written in the findings log with its reason.
- Every junction has been seen in macro from at least six directions,
  standing and walking, with nothing open at severity A or B.
- The orbit, distance and fresh-eyes sets have been read.
- Luis's notes are answered with before and after images.
- The findings log and the misses ledger are written and pushed.

## 8. The findings log

Every round writes one log, in its section of the model's page (for the
miners, [TheMiners.md](TheMiners.md)):

| ID | Model | Zone | Found by | Severity | Finding | Cause | Fix | Checked |
|---|---|---|---|---|---|---|---|---|

## 9. The misses ledger

Things Luis found that the method missed, why they were missed, and what
catches them now.

| Date | What Luis saw | Why it was missed | What catches it now |
|---|---|---|---|
| Oct 3 | Small's lantern floating by the hip | Nobody asked "what holds it?". The rest pose moved it to "the belt", with nothing modelled there | The four questions; the floating and meets checks |
| Oct 3 | Small's satchel strap not meeting the bag; the bag clipping | The bag was placed by measurements and the strap by guesses | Laying on surfaces; "strap meets each ring"; "bag sinks into the coat" |
| Oct 3 | Small's neck still odd | The neck was only seen from the front | The neck zone, from eight directions |
| Oct 3 | Small's legs not lining up with the boots | The feet were seen from 90 cm, in one walking frame | The boot zone at ground level; the walk's eight phases |
| Oct 3, evening | The laces' crosses floating | The boot's shaft was slimmed, but the laces kept the old measurements, and nothing checked them again. From the front they looked right; from the side they hovered | "Laces lie on the boot"; laces laid on the boot's real surface; the boot zone from every side |
| Oct 3, evening | The leg still beside the boot | The shaft was modelled standing straight up, while the shin leans forward from the ankle even at rest. It only showed from the side, at ground level | The shaft follows the shin; the boot zone from every side at ground level; "shaft covered by the trouser leg" on every frame |
| Oct 3, evening | The strap still not joined to the bag | Tabs were added, but the strap was never made to pass through them | The construction pass; "strap meets each ring" |
| Oct 3, evening | The lantern floating up close | The grip was a guessed point, never measured; the closed fist left 2 to 4 mm for a handle; the hand was never seen closer than a metre | "Handle meets hand"; the hand wrapped round the handle and cut to fit it; the hand zone in macro |
| Oct 3, evening | The bag not hanging with gravity | The bag was rigid on the pelvis; nobody asked what gravity does to it | Question 3; the bag hangs from its rings on a bone of its own and swings |

### What the method's own rounds taught it

| Round | What went wrong | The rule since |
|---|---|---|
| First (October 3 and 4) | The audit called hands "inside" their own cuffs, grazing surfaces "crossing", and hidden bubbles "floating" | Read each failure before fixing it (pass 3) |
| First | Two guesses at why the lantern leaned were wrong; the recorded frames showed it leaning backward, standing still | Measure before explaining (pass 6) |
| First | A fix for knees through a long coat made the coat a bell, a little more each round | Read against the before (pass 5) |
| First | A test read the bones after the miner had walked on, and measured the walk | A check reads one moment: the body reports what it last posed |
| First | A change to Small's skirt would have calmed a movement Luis had praised | Keep what Luis liked (pass 6) |
| First | The audit's reports were written among the game's assets | Tools write to `Art/Review`, never into `Assets` |

## 10. Research behind the method

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
| **Neck** | Neck rising from behind the jaw; collar or neckband round the neck (no gap, no void, no skin through the cloth, from every side) | Closed collar | Open collar, lapels, shirt front | Neckband; the apron's strap round the back of the neck |
| **Shoulders and back** | Straps lying taut on the cloth over the shoulder; things slung on the back held by loops | Satchel strap | The pick's strap; two loops; the pickaxe hanging by its head | — |
| **Chest and waist** | Buttons sewn on; patches flat on the cloth; apron bib and ties | Three buttons; strap over the buttons | Patch on the chest; strap over the lapels | Bib; ties knotted at the back |
| **Arms and hands** | Sleeve or cuff round the wrist (no gap); fingers; hands clear of the coat | Cuffs | Cuffs | Rolled sleeves; bare forearms |
| **Carried things** | The hand closes round the handle (touching all round); the object hangs below with gravity and swings; the body stops it | Lantern in the left hand | Mug in the left hand | — |
| **Bag** | Strap ends through rings on the bag (touching); flap folded over the top (touching); bag resting against the hip, hanging with gravity, never inside the coat | Satchel at the right hip | — | — |
| **Hips and hem** | Skirt over the trousers (covered, in every stride); apron on the smock, drawn in by its ties; things hung at the hip | Coat skirt | Long coat skirt; patch | Smock skirt; apron; pocket; hammer in its loop |
| **Legs and boots** | Trousers falling over the boot's shaft (covered, all round, every frame); laces on the boot through eyelets (touching); soles on the ground; one boot never in the other | Boots, laces | Tall boots, laces | Boots, laces |
