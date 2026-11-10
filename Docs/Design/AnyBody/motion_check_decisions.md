# Check of motion_design.md: the decisions, the research, what Luis can judge

Checker: independent (did not write motion_design.md). November 10, 2026. Read: the design; the correspondence from
October 9 to November 9; the decision log rows of those dates; the memory notes; the proposal; strategy, critique; the
research report and the frame-by-frame study; both judges' rounds and the referee; REVIEW.md and INTEGRATION.md C0-C13;
`PhysicalFall.cs` and `MinerPanel.cs` at `D:/Dev/WG` (`5e9a4a0`). Control theory not reviewed (the lead's part).
No Unity run, no code changed.

Grades: **sure** (read, quoted), **likely**, **question**. Ranked by consequence within each part; the top ten overall
are in §0.

## 0. The ten that matter most

1. **(e) sure. In the build as played, most falls will still end in the old posed getting up.** §3.1: "Fallen onto
   its back or a side it gets up as before (Small's present way if that switch is on, otherwise the old way)". The
   present switch is off by default (`PhysicalFall.cs:773`, `GetsUpByItsOwn = false`), and the panel's push goes "the
   way the view looks". On October 22 Luis wrote after this exact case ("landing on its back or side ... every push
   ended with the old getting up"). So Long and Round fallen on their backs or sides rise by the animation Luis rejected,
   in the build meant to show him the new way. Q1 recommends front first but does not tell him this. Needed: the
   report's first line says how to make a miner fall onto its front. Also a panel line that names how the last getting
   up was done (new way, present way, old way: `ByItsOwnSaid` already holds it), so a posed rise cannot be taken for the
   new one.
2. **(e)(c) sure. The first build cannot show the build choosing the way, which is the point of the direction.** The
   proposal predicted Small "on hands and feet ... as toddlers do" and Round with "both hands pushing on the knee". The
   first build drops both: §3.4 has "the bear and Gowers' hands on the knees". Q5 then recommends keeping the strengths,
   though "from the front they will get up nearly alike". Judges round 2, item 7: "The three miners differ in costume,
   not in carrying weight". The research has heavy trunks staying upright, feet back, on a wide stance. No law in §1.4
   carries that, and no test either (§3.3 has only "Not one route for all three miners"). Luis, November 3: "all the
   visual aspects ... really tie to the capabilities". As written, he can judge only the strength slider.
3. **(d)(c) likely. The speed rule slows a weak body evenly: the anti-reference.** §2.6: pace = ρ·min(ωmax(1 − S_j))/…;
   "as S_j nears 1 the pace falls towards nothing, smoothly". The frame-by-frame study on Tao 2022: "Slowed evenly, it
   reads as slow, not as weak". The research: "A slow rise is not the same motion slowed down" (more turning, less direct
   trunk paths). Law 5 says weakness adds stages "not slowness alone", but nothing tests that a weak rise differs in
   shape. Strategy §3.4's stagger law, 0.1 + 0.9u²/(1 + u²), is gone from the design, and §1.5 makes the sides' lag
   seeded only (0 to 0.05 s·k_H), not a function of u. Wanted: a test that compares a weak rise with the able rise
   time-normalised (paths, stagger, overlap), not only by its time.
4. **(a)(d) likely. The lying still reads as a body dead until a clock.** §2.7: "the body lies at the fall's slack tone
   (`AtRest` 0.05) ... Its tone comes back over the time the fall gives it". That time is `HowLongItLies()`, a sum of
   seconds (`PhysicalFall.cs:99-104`). The lying time came from Luis (October 9: "dependent on ... how strong was the
   force ... it's current stamina"). But October 13 asks for more: "shouldn't look dead and then start moving ... after a
   timer. It should maybe move less while it's recovering". Apart from "the neck first", nothing in §2.7 stirs while the
   tone ramps: no breath on the ground, no small shifts, no hand drawn in. No step of §3.2 looks at the lying. Also
   §1.7: after going down it "lies a little longer at each try (`PhysicalFall.cs:779`)", which is `RestsAfterATry` 0.7 s
   a try: a timed rest, the thing F10's correction ended inside a stage.
5. **(e)(g) sure. Luis sees nothing until step 7.** The steps run from 0 to 7, and only step 7 reaches "then Luis".
   INTEGRATION C4 found this same order fault ("gives the Director nothing to see until step 8") and it was corrected
   then. The memory note on explorations says to show 2-4 frames early. Wanted: a first look for Luis after step 2 or 3
   (one stance held and one change, recorded in the game), with nothing for him to decide.
6. **(e) sure. Known look faults sit on the first build's own routes.** Both routes go through hands and knees, the
   half-kneel and the squat. Judges round 2, item 2, saw Long's coat "out flat on the ground in the deep squat and
   bundles into a dark block in the half-kneel and on hands and knees". §3.4 defers "the coat and the hung things in the
   deep stances". INTEGRATION C2 found Long's model shoulders "9 cm behind", so "its hands float". The fix,
   `AtlasPoser.ReachFrom`, is a test-side drawing, and §2.8 moves only `AtlasHands` into the game. Round's apron "plank"
   (referee, item 8) is not mentioned at all. Luis will see these flaws while judging the motion (proactive polish).
7. **(g) sure. The question the critique left for Luis is missing: the keeper's double budget.** Critique F1: "State
   plainly that capping the keeper at 1× changes standing and the step ... Luis's (Open) decision. Do not quietly keep
   the 2×." The design keeps the keeper at up to 2× (§2.3; risk 2) and hands over "during the push". So the last part of
   every rise has up to twice the strength the plan and the creator's warning used. A weak body may finish a rise its
   atlas says it cannot, and a quick final rise is the "suddenly gets up" of October 13. Ask Luis. Add a test that
   compares the joints' share of strength and the hips' rise speed either side of the hand-over.
8. **(d) likely. Nothing tests posed, robotic or repetitive by number; judges are asked, on unspecified evidence.**
   - §2.2 step 2 blends "the posture ... from this stance's fit towards the next by s": a tween between fitted poses
     wherever the tasks leave a joint free, and the free arms and elbows are what the eye reads (judges: "inert arms").
   - The gathering rocks "at the body's own sway time, each rock a little further": a regular, growing beat.
   - Every stage opens with the same head lead, 0.25 s·k_H ±30% (critique §4).
   - Step 7's judges get no stated material. Round 2: "breath, timing, arcs, tiredness ... CANNOT BE TOLD from stills".
     Needed: judges shown motion at the game's frame rate (clips or dense strips), not only flagged frames.
   - Measures to add: the free joints' path against the straight blend between the fits; rock period and amplitude
     across falls; how much the head-lead and stage times vary across six falls; and a measure of "no two falls
     alike" (§3.3 leaves "alike" undefined).
9. **(f) sure. Absolute metres and seconds remain in gates, bars and the solver.**
   - Gates: the hand-over gate "less than 1 cm outside" the soles (§2.10).
   - Solver: `FeasibleRegion` keeps "absolute tmax 1.5 m and 12 directions ... not scaled by body size" (REVIEW F10c),
     yet §1.4/§1.5/§2.4 gate and spread on "the region the weight may wander".
   - Bars: "2 cm into the ground", "1 mm" in step 1, the palm hidden "about 2 cm".
   - Times: the draw's temperature τ "about 0.4 s" and κ "about 1.5 s" are not scaled. Route costs T grow as √H, so a
     big body's draw is more decided, and it varies less than a small one.
   - Fall constants (`LiesAtLeast` 0.8, `RestsAfterATry` 0.7).
   - Numbers tuned on one body: ρ "from Small's and Long's knees and hips"; eFull 35° from Small's riser; the
     `Balance*` constants of `OwnRiser`, searched on Small (body_map §4.2); `maxAngularVelocity` 20 rad/s on every link
     whatever its size.
10. **(b) sure. ReadPose is to be tested where it is not singular, but it is used where it is.** INTEGRATION C12: "the
    root's Rz·Ry·Rx loses yaw against roll at pitch ±90°, which is lying flat". §1.1 reads every fall's entry by
    `ReadPose`, and step 1 tests "100 random poses", which almost never fall at ±90°. Test it on bodies lying flat on the
    front and back, and with the shoulder and hip at ±90° of abduction. The design does not carry C12's
    `JointFrame.Side` warning (+1 right against the port's +1 left) either.

## 1. (a) Against Luis's decisions, or an open choice quietly decided

| # | Grade | Finding |
|---|---|---|
| a1 | likely | Lying slack and ramped on a computed time, no stirring (item 4 above). |
| a2 | sure | The old way is kept "after 12 tries" (§1.7). Luis allowed this on November 3, item 6. It is consistent, so long as the report counts it in the game as played, not only on the bench. |
| a3 | question | §2.10 changes the hand-over that Luis's liked step lives behind: "`RiseSettles` 0", and the drives move to the keeper's settings "over a few steps, not at once (today `Soften`)". Nothing says this is behind the new switch. If it is not, Small's present way and its stand change unseen (the memory note: change only what is asked). |
| a4 | question | §2.8 moves the drawn palm into the game's drawing. That is a look change. Is it limited to the new way, and shown to Luis beside today's? |
| a5 | question | Law 9 / §2.7: "one on the knee and one on the thigh, or one resting and one hanging". Luis decided "a free hand near a knee or thigh rests on it". A hand that hangs while near a knee is a variation he did not approve. Keep it for when the hand cannot reach, or ask. |
| a6 | sure | "One body for everything" (October 14) stays split: the fall stays on `PhysicalFall`'s other body, and the keeper stays apart (§2.10, "Not in the first build"). The design states this; it is not hidden. Luis should hear it as a deferral, not a decision. |
| a7 | sure | §1.7's "κ about 1.5 s added to that edge" and the failure window "0.5 s·k_H" are tests, not clocks of the motion, as the design says. Acceptable. |
| a8 | sure | Help only while training (October 22) is untouched: there is no hidden push. Good. |

## 2. (b) Earlier findings bearing on layers 2 and 3

**Critique F1-F13**

| F | Status | Where / what is left |
|---|---|---|
| F1 | partly answered | §2.3 one budget for layer 3. The keeper's 2× is kept, and the question to Luis is missing (item 7). |
| F2 | partly | One writer for layer 3 (§2.3, §2.9). The keeper's `Springs` and `Soften` remain, and they meet at the hand-over ramp. |
| F3 | answered | §2.9, left at 20. |
| F4 | answered in words | §1.8, §1.5, step 7. No numerical test (item 8). |
| F5 | answered | Step 0, `StiffAnkleHolds`. |
| F6 | partly | Frames are kept per body (§2.1). The fall's ranges are left wider than layer 0 and widened, then tightened (risk 5). Option (a) asked for "ranges brought to the same table first". |
| F7 | partly | The squat is kept at h ≥ 0.4 (risk 11). There is no single range table, and the §1.2 S values come from fits on b3's ranges (C11: b3 knee 150, layer 0 155). The critique's "Re-fit the atlas on that table before its numbers are quoted again" is not done. |
| F8 | deferred | Risk 10. But step 2 checks the drive force against "the atlas's S" at strength 0.5. Those S are biased upwards by two-sided comfort, so the test checks against a known-biased number. |
| F9 | answered | §2.3, toned damping. |
| F10 | answered inside a stage | §1.6, §1.7, §2.5. Not between tries: `RestsAfterATry` is timed (item 4). |
| F11 | answered | §2.6, one speed rule. ρ is tuned on Small and Long. |
| F12 | answered | §2.1 (targets as made), §2.2 step 4 (loads from the subtree), §2.9 (BodyWatch scoped). |
| F13 | answered | Commitment (§1.3), smooth closure (§1.3), hand-over gate (§2.10), warning that includes the keeper (§1.7), time-slicing (§2.11). |

**REVIEW F1-F11**

| F | Status |
|---|---|
| F2 | answered: the gates use S (§1.6). |
| F3 | answered: §2.11 asks for buffer reuse. |
| F4 | deferred, stated (risk 7). §3.2 counts on the meadow, which is right. |
| F5 | **dropped.** There is no per-side length and no direction-dependent strength, and friction is fixed at 0.6 against the game's 0.7/0.8. "Any body" cannot include one short leg. Limits of the plan's model, never stated in the design. |
| F6 | **dropped.** Line supports are ill-conditioned. Do the routes' stages all have three ground points that are not on one line (the tripod with a single shin, a lone sole on its ball)? |
| F7 | answered: `AnyBody.X` names (§2.1), and the atlas built on its own runtime (risk 8). |
| F8 | answered (§2.11). |
| F9 | not mentioned: are statics built in a body-local frame when the body lies far from the origin? Low. |
| F10 | (a) sensitivities not mentioned. (b) **dropped**: "The fit's cost has no reserve term ... a lower-cost fit can hold worse". The plan refits stages at drawn settings (§1.3, §1.6 step 3) with no check that the refit holds as well as the atlas fit it replaces. (c) **dropped**: item 9. (d) answered (§1.2). |

**INTEGRATION C0-C13** (those that bear on layers 2 and 3)

| C | Status |
|---|---|
| C1 | **open, not carried.** Does a made body inherit its miner's kept factors (up to 40%)? The §1.2 table quotes "Long x1.25" without saying, and the bench runs ten made bodies on it. Q5 touches only the three miners. |
| C2 | **half.** The palm is carried (§2.8). Long's 9 cm shoulder is not (item 6). |
| C3 | answered: a forearm–thigh pair and its touch (§2.1, §2.7). |
| C4 | **repeated** (item 5). |
| C9 | not mentioned. Hung things kicked by fast turns: the roll and the turns while rising will swing the lantern and pick in the game. Measured? |
| C11 | not mentioned. Posture targets from fits beyond layer 0's range are clamped (`JointAim`). Mark which stages of the two routes have them. |
| C12 | ReadPose round trip carried. The singularity and the side sign are not (item 10). |

## 3. (c) Lessons of the references and the judges

| Lesson | Carried? |
|---|---|
| Head leads; stages overlap; placed then loaded; last push over a planted foot; nothing goofy | Laws 1-4 and 11, and §2.4. Only law 3 has a test (step 3); the rest rest on the judges. |
| Weakness adds stages and tries, not slowness alone; "a slow rise is not the same motion slowed down" | In law 5, but against the speed rule (item 3). There is no test. |
| The build chooses the way (the toddler's bear, the heavy trunk upright, Gowers) | Lost for the first build (item 2). |
| The foal: "a very wide base, the legs splayed and trembling" after standing; the proposal: "its effort and its tiredness seen" | **Lost.** Nothing makes effort visible beyond breath and rocks. Judges round 2, item 3: "nothing shows effort even where Small needs 0.56". Once stood, the keeper takes the body, with no widened base or tremble for a weak body. |
| Gathering before a push, never a frozen hold | Law 6 and §2.5, gated. The regular rock is a risk (item 8). |
| Rests tied to the reserve needed next | §1.7, gated. Not between tries (item 4). |
| Low gains with gravity compensation, targets from state, uneven sides and timing (the research's defences against the posed look) | §2.2, §2.3, §1.5. |
| A stable preference near a boundary, so a small change of build does not flip the way | Habits (§1.5), switching cost (§1.3). There is no test that the odds move smoothly over a sweep of the game's own made bodies, only the prototype's sweep sheets (step 4). |
| A body sampler across the creator's range, extremes included; evidence by class of body | Only the ten made bodies of the sheets plus three strengths (§3.1). That is a fixed list, not a sampler (proposal, step 2: "a maker of bodies across the creator's likely range (and beyond it)"). |
| Strategy §3.4's hand-load law and hip height on hands and feet | **Lost.** §1.3 says "the hand's load on the thigh, how high the hips are on hands and feet ... are set by the laws of §1.4", but §1.4 has no such law. |
| Strategy §3.4's stagger | **Lost** (item 3). |
| Judges 2: bear (1), sitting up (3) | Deferred with their routes. Law 8 is written for the head. |
| Judges 2: rising with inert arms (4); arms alike (5) | Laws 4, 7, 9 and 10. Judged only by judges. |
| Judges 2: coat (2), Round's apron (8), Long's shoulder (C2) | Not mended before Luis sees them (item 6). |
| Judges 2: miners alike in carrying weight (7) | Lost (item 2). |
| Judges 1: the stand and the kneels mirror-perfect (7) | §1.5. |
| October 13: a button pressed many times | **Lost.** No test of a miner pushed again while rising, or pushed ten times quickly. The memory note: "Press a button many times before handing it over". |

## 4. (d) What would read as posed, robotic or repetitive, and what would catch it

- Weak bodies slowed evenly (item 3). Catch: a weak rise compared with the able rise time-normalised.
- Free joints tweened between fitted poses (§2.2 step 2). Catch: the free joints' paths against the straight blend; judges shown motion.
- A rock with a regular period. Catch: rock times across falls, and their spread.
- The same head lead at every stage. Catch: the head lead's spread within one fall and across falls.
- Two routes with a fixed stage order, and three miners of near-equal strength (Q5). Every front fall becomes one of two sequences. Catch: the six-falls strip (step 6) and the same strip across the three miners; the odds per miner reported to Luis.
- Variation that narrows as the reserve thins (§1.5). The weak, the bodies most likely on screen for long, vary least in where they move. Catch: the spread of placements and timings at strength 0.5 against strength 1.
- A slack lying, then a ramp (item 4). Catch: a lying-phase recording (joint motion while lying, so it is never zero and never a sudden start).
- The hand-over to the 2× keeper (item 7). Catch: the joints' share of strength and the hips' speed either side of it.
- A drawn palm that "turns only while the hand bears, eased with the load" (§2.8). Catch: wrist angle speed in every frame.

## 5. (e) What the first build leaves Luis unable to judge; the switch and the report

- Back and side falls show the old animation (item 1). The push depends on the camera.
- Builds that do not show in the way chosen (item 2). Made bodies only on the bench: "any body" is not in the game for him. Offer recordings of the made bodies, or a few of them on the panel.
- The end of every rise is the old keeper's (item 7).
- The coat, the apron and Long's floating hands (item 6).
- Two similar toggles: "Gets up by its own body (Small; the others as before)" and "Gets up by its own body: the new way (from its front; any miner)". If both are on, which wins? Name the outcome in the panel. If Q4 is refused and the new switch is off by default, October 10 repeats. The memory note: say in the first line what must be switched on, and what is seen if it is off.
- The report should say what to press for a front fall, what is new and what is old in what he will see, and that the strength slider is where capacity shows. The design's §5 plain words do not say that back falls still use the old way.
- Nothing to see before step 7 (item 5).

## 6. (f) What will not scale from three miners to any body

- The constants of item 9.
- A fixed body plan (15 parts, 34 numbers, a biped). Which attributes the creator has is Open (October 30). Say that "any body" here means any proportions of this biped.
- REVIEW F5: no per-side lengths, no direction-dependent strength, a friction constant.
- C1: kept factors on made bodies.
- The ten sheet bodies as the whole test population.
- §1.2's S table is per body, as information only. That is fine, but it was fitted on b3's ranges (F7, C11).
- The three named passages and the two routes are written once for any body. That is allowed (November 3, item 2), so long as nothing names a body, which holds.

## 7. (g) The five questions

| Q | Right one? Plainly put? |
|---|---|
| Q1 | Right, but it hides the consequence (item 1). Put plainly: "Until the second build, a miner that falls on its back or side gets up the old posed way. You will need to push it onto its front to see the new way." |
| Q2 | Half decided already. November 3: the creator warns but allows; another unit helps later. Ask only what is open: where it waits, how often it tries again, and whether it shows effort while waiting. |
| Q3 | Right. "Within a quarter of each band" is jargon; say "where the hands and feet go varies a little, the two sides differ". |
| Q4 | Right. Add what the other switch does, and how the last getting up is named on the panel. |
| Q5 | Right, but the recommendation keeps the miners alike, which hides the November 3 tenet. The memory note on explorations offers the other way: show both side by side in this build, with a switch "strength by their build". |

**Missing questions**

1. The keeper's double strength at the end of the rise: keep it for now (the step he likes unchanged), or cap it and re-search the keeper, shown to him (F1)?
2. The lying: should a fallen miner stir a little while it recovers (breath, small shifts), and should the time it lies still come from the blow, the daze and the breath as he asked on October 9?
3. Only two routes from the front in the first build, neither the toddler's hands-and-feet that Small was predicted to take: acceptable for a first look?
4. Long's coat and Round's apron in the kneels and the squat: mend them before he sees the build, or judge the motion with them as they are?
5. (Low) Should the panel have a way to push a miner onto its front?
