# U1: How people perceive that a movement is unnatural (researcher 1 of 3)

November 13, 2026. Part of the research Luis asked for: every kind of movement that can read as unnatural, written
for the walk's turns now and for the rest of the project. This part covers **perception**: what viewers notice, how
small an error they can see, and what makes them see it.

This extends two earlier pages and does not repeat them:
- `Docs/Research/2026-10-08_RealBodiesAndHowToJudgeThem.md`, section 10 (animation principles: easing, arcs, overlap,
  follow-through, anticipation; Piwek et al. 2014, where better motion never made a figure eerier).
- `Docs/Research/2026-11-13_TurningAndSettling/research.md`, section 4 (foot slide about 2 cm; the hip wrung over a
  planted foot; body faster than feet; en-bloc turning; a foot cutting through the other leg; changing direction in
  mid-air; no braking; twitches; clockwork idles).

**Marks.** **[well established]** = several studies agree. **[one study]** = a single study, or one lab only.
**[unclear]** = conflicting, unchecked, or my inference. **Read** = full text or its results section opened;
**abstract** = only the abstract or a citing summary. Sources are numbered S01..S48 and listed in section 10.

What this page is not: the game has not been measured against any of it. Every number below comes from lab studies
with their own conditions (mostly a screen at 60-100 cm, often point lights or a plain avatar). They are starting
points for thresholds, not acceptance. Luis's eye decides.

---

## 1. For Luis, first

1. **Bodies and falling things are the two kinds of motion people judge sharply.** For most film scenes a speed off by
   up to 2x still looks natural; for human movement and falling objects the tolerated range is clearly narrower
   (Kobayashi 2019, S20). Our game is exactly those two things.
2. **Gravity is built into how we see a body.** The feet's swing carries the "this is alive" signal, but only when
   their vertical acceleration matches gravity; flipped, the signal is gone, even for newborns and for animals
   (Troje and Westhoff 2006, Troje and Chang 2023; S09, S10). A body or foot that floats, or accelerates up when it
   should fall, is read wrong before anything else.
3. **Size is read from timing.** A walker's stride frequency tells its size: people set a point-light dog's size so that
   size goes with 1 over frequency squared (Jokisch and Troje 2003, S08). A big body moving at a small body's tempo
   reads small, or wrong. Scaled adult motion is read as a child's (Dong et al. 2017, S27).
4. **Effort and weight are read from the movement, not the look.** Lift weight is judged from the kinematics alone
   (Runeson and Frykholm 1981, S31), mostly from how long the lift takes (Hamilton et al. 2005, S30). When effort shown
   and object shown disagree, naturalness drops (Yamac, O'Sullivan, Neff 2024, S40); a body whose shape does not match
   its motion changes what viewers think the object weighed (Kenny et al. 2019, S39).
5. **People forgive a little, and forgive in one direction.** A delay of about 60 ms at a collision was accepted on
   average (O'Sullivan et al. 2003, S05); a thrown ball may go 48% faster than the arm made it but only 14-26% slower
   (Vicovaro et al. 2014, S06). Faster-than-expected passes; weaker-than-expected looks wrong.
6. **A mismatch is worse than a uniformly simple thing.** The uncanny response is best explained by parts that disagree
   (a human look moving like a machine; a heavy body moving lightly), not by "almost human" as such (Kätsyri et al.
   2015, S29; Saygin et al. 2012, S14). For motion alone, ratings fall steadily as motion gets worse; no dip
   (Thompson et al. 2011, S12; Handzic and Reed 2015, S13).
7. **The more human the character looks, the less "biological" it is granted.** Viewers are as good at telling mocap
   from keyframes on any full-body character, but are stricter with more human-looking ones (Chaminade et al. 2007,
   S21); errors in ballistic motion look bigger on a more human character (Reitsma et al. 2008, S04). Our miners are
   stylised, which buys a little tolerance, not much.
8. **Attention decides.** The same 20% limb-length change goes unseen when attention is elsewhere and 3% is seen when
   it is on the limb (Harrison et al. 2004, S18). Errors in the periphery and among similar moving things are missed
   (O'Sullivan and Dingliana 2001, in S05). The player watches the miner they control, its feet when it steps, its
   hands when it works.
9. **Repetition is seen less in motion than in looks, but it is seen.** Duplicate appearance is caught sooner than
   duplicate motion; a motion clone is the same clone on any body (McDonnell et al. 2008, S15); three distinct walks
   made a crowd look as varied as a fully varied one (Prazak and O'Sullivan 2011, S19). Perfectly repeated cycles look
   unnatural; plain white noise looks like shaking; variation must come from the task and the body (S35, S36).

---

## 2. Biological motion: what tells the eye "alive", "heavy", "trying"

| # | Finding | Number / threshold | Conditions | Source | Mark |
|---|---|---|---|---|---|
| 2.1 | A handful of moving dots on the joints is enough to see a walking person, an action, often a sex and an identity | friends identified from 6 at about 38% (chance 17%); figures differ between summaries | point lights on joints, side view | Johansson 1973 (S44); Cutting and Kozlowski 1977 (S43) | [well established]; the often-quoted "recognised in 200 ms" was not verified here: [unclear] |
| 2.2 | The visual system sums walker information over a long window and gains steeply with each visible joint | summation up to about 3 s, about 8x longer than for plain motion | walker in dynamic noise | Neri, Morrone, Burr 1998 (S23) | [well established] |
| 2.3 | The feet alone carry facing direction; the cue works only with gravity-consistent vertical acceleration (falling down, not up); upside down it is lost | inverted feet-only displays at chance | scrambled walkers, 100 masking dots; also cats, pigeons; newborns show the bias | Troje and Westhoff 2006 (S09); Troje and Chang 2023 (S10, read) | [well established] |
| 2.4 | Kinematics specify dynamics: the weight of a lifted box is seen from how the lifter moves | judged weight tracks true weight; lift velocity the main cue (Shim and Carlton 1997) | point-light lifts | Runeson and Frykholm 1981 (S31) | [well established] |
| 2.5 | Viewers judge weight mostly from the lift's duration, although in real lifts the grasp's duration predicts weight best | - | edited box-lift clips | Hamilton et al. 2005 (S30) | [one study] |
| 2.6 | Affect is read from kinematics: speed goes with arousal; pleasantness with the phase relations between segments | - | point-light arm movements | Pollick et al. 2001 (S42), abstract | [one study]; velocity-arousal detail [unclear], not opened |
| 2.7 | Effort is read mainly from the motion; weight mainly from the object's visible size. Mismatched, naturalness drops. Small mismatches pass; viewers do not tell 0% from 25% effort lifts but do tell higher efforts apart (somewhere in 50-100% depending on lifter, or above about 30 lb) | as stated | VR, dumbbell lifts, avatars of varied size, muscle flex shown or not | Yamac, O'Sullivan, Neff 2024 (S40, read) | [one study] |
| 2.8 | Shape-motion inconsistency (motion of a heavy man on a slim body and the reverse) shifts perceived object weight and throw distance; viewers cannot say which source they used | - | retargeted push, lift, throw | Kenny et al. 2019 (S39), 2017; abstract | [one study] |

**Speed along a curve: the two-thirds power law.** In human movement, angular speed along a path goes with curvature
to the power 2/3 (equivalently, linear speed falls where the path bends). Perception is tuned to it: a dot moving on
an ellipse looks uniform in speed only when it actually obeys the law (and so is highly non-uniform); truly
constant speed on a curve looks like it speeds up and slows down (Viviani and Stucchi 1992, S11). **[well
established]** (repeated with drawing and pursuit tasks; brain response stronger for law-following motion, Dayan et
al. 2007, not opened.) For us: a hand, foot or tool tip that travels a curved path at even speed, or keeps speed
through a sharp bend, reads as driven.

**Animacy: a movement nothing caused.** One rigid dot looks alive when it changes speed and direction at once, more so
the bigger the change and when its long axis points where it goes (Tremoulet and Feldman 2000, S33). The same change
read as a bounce off an obstacle looks less alive (Tremoulet and Feldman 2006, in S33). Animate (self-propelled,
unpredictable) motion also captures attention faster than motion that follows from a collision (Pratt et al. 2010,
S34). **[well established]** The flip side for a physics body: when nothing visible pushes, a change of velocity is
read as **the body's own decision**. A twitch, a limb that flies, a hip that jumps all get read as intention, and an
intention with no purpose reads as strange. And when the cause is visible, the effect must arrive in time:
Michotte's launching impression holds for contact delays up to about 50 ms (sometimes 100 ms); later studies put the
median threshold at 95-200 ms, shifting with what was seen before (S28). **[well established]** that the window is
around 50-150 ms; the exact figure depends on method.

**Smoothness is not maximal smoothness.** People prefer smooth motion over jerky, but in one study a little more jerk
than the minimum-jerk path was slightly preferred, more so in VR (Hirose et al. 2020, S41). **[one study]** Adding
plain noise to joints reads as shaking (Gielniak et al. 2011, S35). **[one study]**

---

## 3. Measured detection thresholds for animation errors

| # | Error | Threshold | Conditions | Source | Mark |
|---|---|---|---|---|---|
| 3.1 | Foot sliding (footskate) on a planted foot | noticed below about 21 mm in most conditions; any cleanup preferred to none; simply lengthening limbs preferred to IK plus smoothing | walking characters, screen | Prazak, Hoyet, O'Sullivan 2011 (S01), abstract | [one study]; agrees with practice (S02) |
| 3.2 | Foot slipping with camera motion | camera motion raises the threshold (masks the slip) | parameterised slip clips | "Perceptual thresholds for foot slipping in animated characters" (S02), abstract; authors not confirmed | [unclear] |
| 3.3 | Ballistic jumps: wrong velocity in flight | horizontal velocity errors easier to see than vertical; added acceleration easier than added deceleration | human jump clips, errors added | Reitsma and Pollard 2003 (S03), abstract | [one study] |
| 3.4 | Same, by character | errors look larger on more human-like characters; preparatory motion matters | human vs simpler characters | Reitsma, Andrews, Pollard 2008 (S04), abstract | [one study] |
| 3.5 | Thrown object speed vs thrower's motion | +48% release speed accepted half the time; only -25.8% (overarm), -14% (underarm); just-noticeable difference about 8-14%; retiming the windup (time warping) to match did NOT help; with a mechanical ramp instead of a person the asymmetry reverses | mocap thrower, ball | Vicovaro, Hoyet, Burigana, O'Sullivan 2014 (S06, read) | [one study]; direction agrees with S05 |
| 3.6 | Collision timing and contact | delay at contact of about 60 ms accepted on average; gaps between colliding objects accepted when the view hides them, far less when visible; widened bounce angles and faster outgoing speed preferred to narrowed and slower | spheres colliding, 3D | O'Sullivan, Dingliana, Giang, Kaiser 2003 (S05, read) | [one study]; delay part [well established] with S28 |
| 3.7 | Collision anomalies off the fixation point or among similar distractors | detected less | spheres | O'Sullivan and Dingliana 2001, via S05 | [one study] |
| 3.8 | Limb length change during motion | about 3% (2.7% arm segments) seen with full attention; over 20% can go unseen with attention elsewhere | animated arm/limb | Harrison, Rensink, van de Panne 2004 (S18), abstract | [one study] |
| 3.9 | Finger motion out of sync with the body | 0.1 s detectable; depends strongly on motion type | conversation-like clips | Jörg, Hodgins, O'Sullivan 2010 (S22), abstract | [one study] |
| 3.10 | Gait asymmetry and tempo | cadence +-25% not noticed, +-50% noticed; step-length or swing-time asymmetry 5% not noticed, 13% noticed; roll-over (foot shape) asymmetry 29% read as only slightly impaired | side-view silhouette walker, 33 raters | Handzic and Reed 2015 (S13, read) | [one study] |
| 3.11 | Wrong gravity on a falling, then flying ball | natural rated 83% at 1 g, 67% at g/2, 27% at 2 g; mixed gravity (1 g on a ramp, other in air) 42-58% | VR CAVE, head tracked, interception | La Scaleia, Ceccarelli, Lacquaniti, Zago 2020 (S37, read) | [one study]; "a 1 g internal model" [well established] across the Zago/Lacquaniti work, though disputed by some |
| 3.12 | Playback speed of whole scenes | most scenes look natural from about 0.5x to 2x; scenes of human bodies or falling objects much narrower; a soccer match at 1.1x not noticed | natural movies | Kobayashi 2019 (S20, read); de'Sperati and Baud-Bovy 2017 (via S20) | [one study each, agreeing] |
| 3.13 | Update rate (pose frames) needed to look smooth | depends on cycle rate, speed, motion complexity, group size; not on character, clothes, scene | human characters | McDonnell, Newell, O'Sullivan 2007 (S17), abstract; numbers not obtained | [one study] |
| 3.14 | Blend length between clips | per-motion computed blend lengths preferred to a fixed one | linear blends | Wang and Bodenheimer 2004 (S38), abstract | [one study] |
| 3.15 | Face vs body anomalies | facial anomalies stand out even when large body anomalies are present; large body changes went largely unnoticed in paired comparison | full virtual actors | Hodgins, Jörg, O'Sullivan, Park, Mahler 2010 (S07), via citing papers | [one study] |
| 3.16 | Interpenetration | in VR, people are far less sensitive to the hand being visually stopped short of where it really is than to seeing the hand pass into an object | VR hands, own body | Burns et al. (S46); Prachyabrued and Borst 2016 (S47); abstracts | [one study each]; for a third-person character: [unclear], nothing found |

Physical plausibility is judged by heuristics, not by physics: people accept wrong-but-plausible collisions, and
accept "more" (faster, wider) more than "less" (S05, S06). **[well established]** across these and the older
naive-physics literature cited in them. Thresholds were measured one at a time; the authors warn they may not add up
independently when several errors combine (S05). **[unclear]**

---

## 4. The uncanny valley, for motion rather than looks

- **No valley for motion alone.** When walking motion is degraded step by step (joint disarticulation, jerk, phase),
  humanness, familiarity and eeriness ratings change monotonically, with no dip (Thompson, Trafton, McKnight 2011,
  S12). The same for gait abnormality (Handzic and Reed 2015, S13) and for improved walking on still-imperfect
  figures (Piwek et al. 2014, already in the October page). **[well established]** (three studies).
- **Mismatch is the road to eeriness.** A 2015 review found no consistent support for the plain "almost human" valley
  nor for the idea that movement deepens it, but good support for perceptual mismatch: parts of one figure at
  different levels of realism (Kätsyri et al. 2015, S29). In the brain, the action-perception system responds most to
  the android (human look, machine motion), read as a prediction error (Saygin et al. 2012, S14). **[well
  established]** as a direction; brain reading [one study].
- **Realistic looks raise the bar.** The more human a character looks, the less likely its motion is called
  biological (Chaminade et al. 2007, S21), and the larger its ballistic errors look (S04). **[one study each, agreeing]**
- **What it means here (inference, [unclear]).** For the miners the danger is not "too realistic"; it is parts that
  disagree: a body that is heavy in its look but light in its timing; legs stepping by physics while the hips follow
  a script; a breathing idle that is alive next to a turn that is mechanical. Each half may be fine alone.

---

## 5. How sensitivity changes with how the body is seen

| Factor | What changes | Source | Mark |
|---|---|---|---|
| Distance / size on screen | Actions still matched above chance at an apparent 1000 m (figure 0.18 degrees tall); moving figures beat still ones; the typical lab figure is 6.7 degrees (a person at 15 m) | Thornton, Wootton, Pedmanson 2014 (S26, read abstract) | [one study] |
| Distance for small errors | No study found that measures footskate or joint error thresholds against distance. Inference: errors measured in cm on the body shrink in degrees with distance, while timing errors (delay, tempo, frequency) do not shrink | - | [unclear] |
| Camera motion | Raises the foot-slip threshold | S02 | [unclear] |
| Viewpoint | Walking direction and leg motion hard to judge from the front (lower legs hidden); profile and half-profile good. A depth-ambiguous walker is seen facing the viewer most of the time (about 70-80%) | Kuhlmann, de Lussanet, Lappe 2009 (S25) | [well established] |
| Viewpoint for contact | A gap at contact is accepted when the view hides it, much less when visible | S05 | [one study] |
| Attention | 3% vs over 20% for limb length depending on attention | S18 | [one study] |
| Periphery and crowds | Anomalies off fixation and among similar distractors detected less | S05 | [one study] |
| How much of the body | Sensitivity rises steeply with the number of visible joints (S23). Viewers look mostly at the torso when judging motion, though they say they use hands, elbows and feet (Eye-tracktive 2015, S45); head and upper torso draw most looks when spotting clones (S16) | S23, S45, S16 | [one study each] |
| Face vs body | The face wins attention and error detection over the body | S07 | [one study] |
| Exposure time | Information builds up over about 3 s (S23): a brief flick may pass once and be caught when seen again | S23 | [well established]; the "again" part is inference |
| Prior exposure | Watching fast or slow motion shifts what speed looks natural (Mather et al. 2017, via S20) | S20 | [one study] |

---

## 6. Repetition, symmetry and regularity

- **Appearance clones are caught before motion clones**; colour variation, random orientation and motion hide clones;
  a motion clone is seen as the same whichever body plays it; closeness helps detection (McDonnell et al. 2008, S15).
  **[one study]**
- **Three distinct walking clips** made a replicated crowd indistinguishable from a fully varied one (Prazak and
  O'Sullivan 2011, S19). **[one study]**
- **Cyclic animation looks unnatural through its repetition**; a biomechanically shaped noise function made running
  look more natural (Bodenheimer, Shleyfman, Hodgins 1999, S36, abstract). Plain white noise on joints reads as
  shaking; task-aware variance in the torque domain was as human-like as the best method tested (Gielniak, Liu, Thomaz
  2011, S35). Human-like variability in timing makes robots read as more human (Wykowska and colleagues, S48, press
  summary). **[well established]** as a direction; no study found that measures the loop length at which repetition
  is noticed. **[unclear]**
- **Idle motion.** Viewers could not tell acted idles from genuine ones (Atxa Landa et al. 2025, S32). **[one study]**
  (Agrees with the October idle work: irregular intervals, never symmetric.)
- **Symmetry.** Gait asymmetry under about 5-13% of step length or swing time is not seen (S13); so a little
  asymmetry is free, and perfect symmetry is not needed for "normal". Whether *perfect* symmetry itself reads as
  artificial was not found tested. **[unclear]**

---

## 7. What viewers notice first

No single study ranks error types by order of noticing. Pieced together (each item [one study] unless marked):
1. **The face**, if there is one (S07).
2. **Things that move by themselves**: animate-looking changes of velocity grab attention first (S34), and a change
   with no visible cause is read as intention (S33). [well established]
3. **The feet against the ground**: the foot's gravity-shaped swing is the life signal (S10), and footskate shows at
   about 2 cm (S01). [well established for the life signal]
4. **Timing against gravity**: falling, jumping and thrown things (S03, S20, S37).
5. **Effort against result**: a weak windup with a strong throw passes; a strong windup with a weak throw does not
   (S06); effort vs object size (S40).
6. **Repetition**, later and with more exposure (S15, S19, S23).

Where viewers look is not the same as what they use: gaze stays on the torso while they report judging by hands and
feet (S45). Eye tracking alone will not show what made a motion look wrong.

---

## 8. Creatures, odd proportions and dynamic similarity

- **Size is read from tempo.** People make a walking dog look natural by setting size proportional to 1 over stride
  frequency squared, the pendulum law (Jokisch and Troje 2003, S08). **[one study]**, physics agrees.
- **Dynamic scaling works on the eye.** Adult motion scaled in time and length by dynamic scaling rules is more often
  read as a child's (Dong et al. 2017, S27, abstract). The scaling rules come from robotics and graphics (Raibert and
  Hodgins 1991; Hodgins and Pollard 1997 "Adapting simulated behaviors for new characters"; not opened).
  **[one study]**
- **Physics background (not a perception study; [well established] in biomechanics).** For geometrically similar
  bodies under the same gravity, matching Froude number means times scale with the square root of length: a body
  twice as tall steps about 1.41 times slower. A long-legged miner at a short miner's cadence breaks this.
- **Shape-motion mismatch** shifts perceived weight and effort (S39, S40); in walking such mismatches are detectable,
  in other actions much less (Russell et al., cited in the 2024 "Shape shifters" paper; not opened). **[unclear]**
- **What was not found:** a study of non-human or strongly non-human proportions (huge heads, short legs, long arms)
  and whether viewers expect them to move by their own proportions. The Froude rule applies to similar shapes; for
  odd shapes the closest evidence is the shape-motion mismatch work. **[unclear]**

---

## 9. What this gives for measuring the game

Each is a quantity a scan or bench can log, with a starting threshold from the studies above. All need Luis's eye.

| Quantity | Starting threshold | From |
|---|---|---|
| Planted-foot travel per stance | under 2 cm (under 1 cm to be safe) | S01 |
| Cause-to-effect delay (shove to first reaction; foot strike to body response; tool hit to object response) | under 50 ms safe; 60 ms accepted on average; over about 100-150 ms causality breaks | S05, S28 |
| Vertical acceleration of a body part or held object in free flight | should be about -9.81 m/s2; departures toward 2 g look worst, half g less bad | S10, S37 |
| Ballistic velocity errors (in flight, after a jump or a fall) | horizontal errors and added acceleration show first | S03 |
| Result vs effort (throw, push, lift) | result may exceed the effort shown by tens of percent; less than shown by about 14-26% looks wrong | S06, S40 |
| Speed along curved paths of hands, feet, tool tips | speed should fall where the path bends (power law exponent near 1/3 for speed vs radius) | S11 |
| Change of velocity with no visible cause | count them; each reads as an intention | S33, S34 |
| Step tempo vs body size | step time scaled with sqrt(leg length) across Small, Long, Round | S08, physics |
| Left-right asymmetry of step length and swing time | under about 5% unseen, 13% seen | S13 |
| Limb length or bone stretch (IK) | under about 3% | S18 |
| Limb or hand timing against the body | under 0.1 s offset | S22 |
| Repetition of idles, shifts, steps | no two in a row identical; variation from the task, not white noise | S15, S35, S36 |
| Camera | judge in the game's own view: distance, angle and camera motion change what shows (frontal views hide leg errors; profile shows them) | S02, S25, S26 |

---

## 10. Sources

S01. Prazák M., Hoyet L., O'Sullivan C. 2011. Perceptual evaluation of footskate cleanup. SCA 2011. https://diglib.eg.org/handle/10.2312/SCA.SCA11.287-294 (abstract)
S02. (authors not confirmed) Perceptual thresholds for foot slipping in animated characters. https://core.ac.uk/works/153922345 (search summary; page refused)
S03. Reitsma P., Pollard N. 2003. Perceptual metrics for character animation: sensitivity to errors in ballistic motion. ACM TOG 22(3) 537-542. https://ri.cmu.edu/?p=8709 (abstract)
S04. Reitsma P., Andrews J., Pollard N. 2008. Effect of character animacy and preparatory motion on perceptual magnitude of errors in ballistic motion. Computer Graphics Forum 27(2) 201-210. https://diglib.eg.org/handle/10.2312/CGF.v27i2pp201-210 (abstract)
S05. O'Sullivan C., Dingliana J., Giang T., Kaiser M. 2003. Evaluating the visual fidelity of physically based animations. ACM TOG 22(3) (SIGGRAPH 2003). https://gv2.scss.tcd.ie/cosulliv/Pubs/fidelity.pdf (read)
S06. Vicovaro M., Hoyet L., Burigana L., O'Sullivan C. 2014. Perceptual evaluation of motion editing for realistic throwing animations. ACM TAP. https://studios.disneyresearch.com/wp-content/uploads/2019/03/Perceptual-Evaluation-of-Motion-Editing-for-Realistic-Throwing-Animations-Paper.pdf (read)
S07. Hodgins J., Jörg S., O'Sullivan C., Park S.I., Mahler M. 2010. The saliency of anomalies in animated human characters. ACM TAP 7(4). (citing papers)
S08. Jokisch D., Troje N. 2003. Biological motion as a cue for the perception of size. Journal of Vision 3(4) 252-264. doi 10.1167/3.4.1; demo https://www.biomotionlab.ca/size-and-speed-demo/ (abstract)
S09. Troje N., Westhoff C. 2006. The inversion effect in biological motion perception: evidence for a "life detector"? Current Biology 16(8) 821-824. (abstract)
S10. Troje N., Chang D. 2023. Life detection from biological motion. Current Directions in Psychological Science 32(1) 26-32. https://pmc.ncbi.nlm.nih.gov/articles/PMC9975895 (read)
S11. Viviani P., Stucchi N. 1992. Biological movements look uniform: evidence of motor-perceptual interactions. J. Exp. Psych.: HPP 18 603-623. (secondary summaries)
S12. Thompson J., Trafton J.G., McKnight P. 2011. The perception of humanness from the movements of synthetic agents. Perception 40(6) 695-704. doi 10.1068/p6900 (abstract)
S13. Handzic I., Reed K. 2015. Perception of gait patterns that deviate from normal and symmetric biped locomotion. Frontiers in Psychology 6:199. https://www.frontiersin.org/journals/psychology/articles/10.3389/fpsyg.2015.00199/full (read)
S14. Saygin A.P., Chaminade T., Ishiguro H., Driver J., Frith C. 2012. The thing that should not be: predictive coding and the uncanny valley in perceiving human and humanoid robot actions. SCAN 7(4) 413-422. https://pmc.ncbi.nlm.nih.gov/articles/PMC3324571/ (abstract)
S15. McDonnell R., Larkin M., Dobbyn S., Collins S., O'Sullivan C. 2008. Clone attack! Perception of crowd variety. ACM TOG 27(3). https://research.gold.ac.uk/id/eprint/15203 (abstract)
S16. McDonnell R. et al. 2009. Eye-catching crowds: saliency based selective variation. ACM TOG 28(3). https://www.tara.tcd.ie/items/9ffa2d7e-2a7a-4da5-8082-071b60ca0471 (summary)
S17. McDonnell R., Newell F., O'Sullivan C. 2007. Smooth movers: perceptually guided human motion simulation. SCA 2007 259-269. https://diglib.eg.org/handle/10.2312/SCA.SCA07.259-270 (abstract)
S18. Harrison J., Rensink R., van de Panne M. 2004. Obscuring length changes during animated motion. ACM TOG 23(3) 569-573. https://www.cs.ubc.ca/labs/imager/tr/2004/harrison-ea-2004-obscuring (abstract)
S19. Prazák M., O'Sullivan C. 2011. Perceiving human motion variety. APGV 2011. doi 10.1145/2077451.2077468 (citing paper)
S20. Kobayashi M. 2019. Perceiving natural speed in natural movies. i-Perception. https://pmc.ncbi.nlm.nih.gov/articles/PMC6643185/ (read)
S21. Chaminade T., Hodgins J., Kawato M. 2007. Anthropomorphism influences perception of computer-animated characters' actions. SCAN 2(3) 206-216. https://pmc.ncbi.nlm.nih.gov/articles/PMC2569803 (read)
S22. Jörg S., Hodgins J., O'Sullivan C. 2010. The perception of finger motions. APGV 2010. https://sciweavers.org/node/182194 (abstract)
S23. Neri P., Morrone M.C., Burr D. 1998. Seeing biological motion. Nature 395 894-896. (abstract, citing papers)
S24. Lopez C., Bachofner C., Mercier M., Blanke O. 2009. Gravity and observer's body orientation influence the visual perception of human body postures. Journal of Vision. https://infoscience.epfl.ch/record/154848 (abstract)
S25. Kuhlmann S., de Lussanet M., Lappe M. 2009. Perception of limited-lifetime biological motion from different viewpoints. Journal of Vision 9(10):11. (abstract)
S26. Thornton I., Wootton Z., Pedmanson P. 2014. Matching biological motion at extreme distances. Journal of Vision 14(3):13. https://www.um.edu.mt/library/oar/bitstream/123456789/27022/1/Matching_biological_motion_at_extreme_distances_2014.pdf (read: abstract and intro)
S27. Dong et al. 2017 (first name and title not checked). (Dynamic scaling of adult motion to child size, perception study.) MIG 2017. doi 10.1145/3136457.3136460 (search summary; PDF failed)
S28. Michotte A. 1946/1963. The Perception of Causality; Gruber, Fink, Damm 1957; via review https://pmc.ncbi.nlm.nih.gov/articles/PMC3708160 and search summaries (secondary)
S29. Kätsyri J., Förger K., Mäkäräinen M., Takala T. 2015. A review of empirical evidence on different uncanny valley hypotheses. Frontiers in Psychology 6:390. https://www.frontiersin.org/articles/10.3389/fpsyg.2015.00390/full (abstract)
S30. Hamilton A., Joyce D., Flanagan J.R., Frith C., Wolpert D. 2005/2007. Kinematic cues in perceptual weight judgement and their origins in box lifting. Psychological Research 71(1) 13-21. https://www.antoniahamilton.com/HamiltonJoyceFlanaganFrithWolpert_PsyRes2005.pdf (abstract)
S31. Runeson S., Frykholm G. 1981. Visual perception of lifted weight. J. Exp. Psych.: HPP 7 733-740. (secondary)
S32. Atxa Landa et al. 2025. Evaluating idle animation believability: a user perspective. arXiv 2509.05023. https://arxiv.org/abs/2509.05023 (abstract)
S33. Tremoulet P., Feldman J. 2000. Perception of animacy from the motion of a single object. Perception 29(8) 943-951. https://journals.sagepub.com/doi/10.1068/p3101 (abstract); Tremoulet and Feldman 2006, motion context and animacy (summary)
S34. Pratt J., Radulescu P., Guo R.M., Abrams R. 2010. It's alive! Animate motion captures visual attention. Psychological Science 21(11) 1724-1730. https://pubmed.ncbi.nlm.nih.gov/20974713/ (abstract)
S35. Gielniak M., Liu C.K., Thomaz A. 2011. Task-aware variations in robot motion. ICRA 2011. https://utw10730.utweb.utexas.edu/static/papers/gielniak11_icra_tav.pdf (summary)
S36. Bodenheimer B., Shleyfman A., Hodgins J. 1999. The effects of noise on the perception of animated human running (title from memory, PDF not opened). Eurographics CAS 1999. https://eecs.vuse.vanderbilt.edu/People/bobbyb/pubs/variability99.pdf (summary)
S37. La Scaleia B., Ceccarelli F., Lacquaniti F., Zago M. 2020. Visuomotor interactions and perceptual judgments in virtual reality simulating different levels of gravity. Frontiers in Bioengineering and Biotechnology 8:76. https://pmc.ncbi.nlm.nih.gov/articles/PMC7039824 (read)
S38. Wang J., Bodenheimer B. 2004. Computing the duration of motion transitions: an empirical approach. SCA 2004. https://eecs.vuse.vanderbilt.edu/People/bobbyb/pubs/sca2004wb.html (abstract)
S39. Kenny S. et al. 2019 (co-authors not checked). Perceptual effects of inconsistency in human animations. ACM TAP. https://is.tuebingen.mpg.de/publications/kenny-tap-2019 (search summary)
S40. Yamac G., O'Sullivan C., Neff M. 2024. Understanding the impact of visual and kinematic information on the perception of physicality errors. ACM TAP. https://www.cs.ucdavis.edu/~neff/papers/PerceptionOfPhysicalityErrors_TAP_2024.pdf (read: abstract, discussion, conclusion)
S41. Hirose J., Nishikawa A., Horiba Y., Inui S., Pataky T. 2020. Integrated jerk as an indicator of affinity for artificial agent kinematics. PeerJ 8:e9843. https://pmc.ncbi.nlm.nih.gov/articles/PMC7500322/ (abstract)
S42. Pollick F., Paterson H., Bruderlin A., Sanford A. 2001. Perceiving affect from arm movement. Cognition 82(2) B51-B61. https://eprints.gla.ac.uk/25001 (abstract)
S43. Cutting J., Kozlowski L. 1977. Recognizing friends by their walk: gait perception without familiarity cues. Bull. Psychonomic Soc. 9 353-356. (secondary)
S44. Johansson G. 1973. Visual perception of biological motion and a model for its analysis. Perception & Psychophysics 14 201-211. (secondary)
S45. (Disney Research / TCD) 2015. Eye-tracktive: measuring attention to body parts when judging human motions. Eurographics short paper. https://diglib.eg.org/handle/10.2312/egsh.20151009.037-040 (abstract)
S46. Burns E. et al. (UNC). Visual interpenetration vs visual-proprioceptive conflict in VR. https://techreports.cs.unc.edu/papers/07-002.pdf (search summary)
S47. Prachyabrued M., Borst C. 2016. Design and evaluation of visual interpenetration cues in virtual grasping. IEEE TVCG. https://vrlab.cmix.louisiana.edu/wp-content/uploads/2025/04/Prachyabrued_Borst_GraspCues_TVCG.pdf (search summary)
S48. Wykowska A. and colleagues 2022. Science Robotics study of human-like behavioural variability in a nonverbal Turing test (exact title not checked). https://www.nature.com/articles/d43978-022-00100-6 (press summary)

---

## 11. What could not be found or checked

- No study measuring error thresholds (footskate, joint error, contact gaps) against viewing distance or character
  size on screen; only the extreme-distance action matching (S26).
- No study of how long a loop or idle must run before repetition is noticed; nor of whether perfect symmetry itself
  reads as artificial.
- No perception study of turning errors specifically (en-bloc, over-fast yaw); those remain from motor-control work
  (the turning page).
- No perception study of balance in a moving body (centre of mass outside the support while upright); only static
  posture stability judgments (S24).
- No study of odd, non-human proportions and the motion viewers expect from them; only same-shape scaling (S08, S27)
  and shape-motion mismatch on humans (S39, S40).
- No study ranking which kinds of error are noticed first; section 7 is pieced together.
- Interpenetration thresholds were found only for one's own hands in VR, not for watching a third-person character.
- Numbers not obtained: McDonnell 2007 update-rate thresholds (S17); Reitsma and Pollard 2003 exact percentages (S03);
  Prazak 2011 per-condition values (S01); Harrison 2004 full table (S18); Hoyet, McDonnell, O'Sullivan 2012 "Push it
  real: perceiving causality in virtual interactions" (ACM TOG 31(4)), which is directly relevant to shoves, was found
  only as a citation, no findings read.
- Several full texts refused (403) or were paywalled; those claims rest on abstracts and are marked so.
