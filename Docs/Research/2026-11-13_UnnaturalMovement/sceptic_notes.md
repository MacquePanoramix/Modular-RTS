# Sceptic's notes (working file, written early)

November 13, 2026. Writer and sceptic for the catalogue. Read U1, U2, U3 in full first.
Plan: re-open the sources behind every number that becomes a bar, and every [well established] claim; keep the
check log here; then write `catalogue.md`.

## Claims to check (load-bearing)

| # | Claim | From | Status |
|---|---|---|---|
| K01 | Foot slide noticed ~21 mm (Prazak, Hoyet, O'Sullivan 2011) | U1 3.1, U2 B1 | pending |
| K02 | Push reaction timing: >150 ms early/late rejected (Hoyet, McDonnell, O'Sullivan 2012) | U2 B11, C8 | pending (U1 could not read) |
| K03 | Collision delay ~60 ms accepted on average (O'Sullivan et al. 2003) | U1 3.6 | pending |
| K04 | Launching-effect window 50-150 ms (Michotte and later) | U1 sec 2 | pending |
| K05 | Gravity 1 g 83%, g/2 67%, 2 g 27% natural (La Scaleia 2020) | U1 3.11 | pending |
| K06 | Playback 0.5x-2x natural for most scenes, narrower for bodies/falling (Kobayashi 2019) | U1 3.12 | pending |
| K07 | Throw +48% / -14% / -26% (Vicovaro 2014) | U1 3.5 | pending |
| K08 | Size ~ 1/f^2 (Jokisch & Troje 2003) | U1 sec 8 | pending |
| K09 | Asymmetry 5% unseen / 13% seen; cadence 25%/50% (Handzic & Reed 2015) | U1 3.10 | pending |
| K10 | Limb length 3% attended / 20% unattended (Harrison 2004) | U1 3.8, U2 B4 | pending |
| K11 | Finger sync 0.1 s (Jorg 2010) | U1 3.9 | pending |
| K12 | GMD skate: >2.5 cm/frame at <5 cm height | U2 B1 | pending |
| K13 | Kimodo skate: toe speed >0.2 m/s at <0.05 m | U2 B1 | pending |
| K14 | XcoM = c + v/sqrt(g/l) (Hof 2005) | U2 B7 | pending |
| K15 | Postural responses 80-140 ms; steps from 160 ms, mean ~250 ms (McIlroy & Maki) | U2 C8 | pending |
| K16 | Arm swing costs: held 12%, bound 7%, anti-normal 26% (Collins 2009) | U2 C4 | pending |
| K17 | Pelvis-thorax phase -20 deg slow, -140 deg at 1.5 m/s (Lamoth 2002) | U2 C6 | pending |
| K18 | Quiet-stance COP speed ~9.2 mm/s; power <2 Hz; dominant <0.4 Hz | U2 C13 | pending |
| K19 | Breathing moves trunk ~0.5 deg; COP barely (Hodges 2002) | U2 C13 | pending |
| K20 | Walk-run near Fr 0.5 (Kram 1997) | U2 C3 | pending |
| K21 | Minimum jerk peak/mean 1.875 | U2 A1, C7 | math check |
| K22 | Two-thirds power law perception (Viviani & Stucchi 1992) | U1 sec 2 | pending |
| K23 | Life detector: inverted feet at chance (Troje & Westhoff 2006) | U1 2.3 | pending |
| K24 | No motion uncanny valley (Thompson 2011) | U1 sec 4 | pending |
| K25 | Shift intervals spread ~ mean (Duarte & Zatsiorsky 1999) | U2 A2.10 | pending |
| K26 | Herr & Popovic angular momentum cancellation 95/70/80 | U2 B9 | pending |
| K27 | Head pitch held within a few degrees (Pozzo 1990) | U2 C5 | pending |
| K28 | CDC ROM values (Soucie 2011) | U2 C1 | pending |
| K29 | McDonnell 2008 clones; Prazak & O'Sullivan 2011 three walks | U1 sec 6 | pending |
| K30 | Tremoulet & Feldman 2000 animacy | U1 sec 2 | pending |

## Check log (filled as I go)

- K01 Prazak 2011: EG page 403. Search summary of the abstract: viewers "can perceive even very low levels of
  footsliding (under 21 mm in most conditions)". So 21 mm is NOT a threshold below which slide is safe; it is an upper
  figure: slides smaller than 21 mm were seen. CORRECTION: the 2 cm bar is at the edge, not safe; keep under 1 cm per
  stance as the bar, 2 cm as a hard fail. Mark: one study, abstract only.
- K02 Hoyet 2012: SIGGRAPH history page abstract read. Timing errors over 150 ms were acceptable less than 50% of the
  time; early and late equally noticeable (U2 did not say this); over-reaction more acceptable than under-reaction;
  angular distortions reduce acceptability; some evidence for reactions expanding away from the pusher; five forces
  distinguished even with one character visible. CONFIRMED (abstract). One study.
- K05 La Scaleia 2020: PMC read. Medians: 83.3% (1 g), 66.7% (g/2), 26.8% (2 g); mixed 58.3% (g ramp, g/2 flight),
  41.7% (g, 2 g). 16 participants. CONFIRMED; note medians, n=16. One study.
- K06 Kobayashi 2019: PMC read. Up to x2 tolerated for most scenes; human movement and falling objects significantly
  narrower; NO numeric range for those given. 10 observers, 22 movies. CONFIRMED but small n; no number for bodies.
- K07 Vicovaro 2014: PDF not readable by the fetch tool (and the tool cached it automatically; not opened). Disney
  abstract page: speed-ups tolerated more than slow-downs; slowed underarm worst; time-warping the thrower did not
  significantly help; release angle helped shortened underarm; ramp reverses the pattern. DIRECTION CONFIRMED;
  the numbers +48 / -14 / -25.8 and JND 8-14% rest on U1's reading only (not re-verified).
- K09 Handzic & Reed 2015: Frontiers read. 5% asymmetry not perceived impaired; 13% perceived; cadence +-25% not
  different from normal, +-50% different; 33 raters analysed (42 submitted); roll-over 29/66/100% all differ; no valley.
  CONFIRMED. One study, side-view silhouette.
- K10 Harrison 2004: UBC page read. ~3% with full attention; >20% can go unnoticed without attention. CONFIRMED
  (abstract). The 2.7% arm figure not seen by me: unclear.
- K11 Jorg 2010: abstract "synchronization errors of as little as 0.1s can be detected" - fingers' timing; what it is
  measured against is not stated in the abstract. CONFIRMED number, scope unclear.
- K12 GMD: secondary (OmniControl) confirms 2.5 cm while foot height < 5 cm. SCEPTIC NOTE: it is per frame of a 20 fps
  dataset (HumanML3D; my knowledge, not re-opened) = about 0.5 m/s; a coarse model-benchmark convention, far laxer
  than perception. Not a bar for the game; use for comparison only.
- K13 Kimodo: NVIDIA docs read. Toe < 0.05 m on this and next frame; toe speed > 0.2 m/s. Contact heuristic 0.10 m and
  0.15 m/s. CONFIRMED. One benchmark.
- K14 Hof 2005: abstract via summaries. XcoM = x + v/omega0, omega0 = sqrt(g/l); b = min distance XcoM to BoS edge.
  CONFIRMED. NUANCE: negative margins can be recovered for a while by counter-rotating trunk/arms (e.g. tiptoe); so
  "b < 0 standing still" is a fault only if it lasts (judgment: > 0.5 s with no step and no counter-rotation).
  U2's "l about 1.2 x leg length": not confirmed; use COM height (judgment). Curtze 2024 cautions.
- K15 McIlroy & Maki: secondary review confirms stepping onset as early as 160 ms (one subject), mean 250 ms
  (force-plate onset); ankle responses ~80-140 ms. CONFIRMED (secondary). Well established direction.
- K16 Collins 2009: PMC read. 1.25 m/s; 10 subjects; bound +7%, held +12%, anti-normal +26%; GRM 3.0 vs 5.0 (bound),
  4.8 held, 8.8 anti-normal. CONFIRMED.
- K17 Lamoth 2002 via 2021 review: PMC read. -20 deg slow; -140 deg at ~1.5 m/s in controls; LARGE between-subject
  variance; gradual; sign conventions differ; some studies null. CONFIRMED as norm but mark the variance: a check of
  direction (more counter-rotation with speed), not a tight bar.
- K18 COP speed: J Hum Kinet page read. 9.2 +- 1.6 mm/s (abstract) eyes open; 100 adults 19-28 y. CONFIRMED, one study
  (COP, not COM; game must compute COP from contacts). The frequency claims (<2 Hz, <0.4 Hz) not in this paper.
- K19 Hodges 2002: abstract via search: body motion coherent with breathing (>0.5), COP coherence low (<0.2); small
  lower-trunk and leg angles counteract. The "0.5 deg" figure NOT confirmed: marked down to unclear.
- K20 Kram 1997: walk-run near Fr ~0.5 confirmed (abstract summaries); at lowest gravities higher. Well established.
- K24 Thompson 2011: abstract: ratings changed monotonically, two walking avatars. CONFIRMED.
- K25 Duarte & Zatsiorsky 1999: abstract gives shifting/fidgeting/drifting patterns in 10 subjects standing > 30 min;
  NO interval statistics in the abstract. The "spread about the mean, CV 0.5-1.0" figure is NOT confirmed: marked
  down to unclear (our judgment for a bar).
- K03 O'Sullivan 2003: abstract (TARA/edeposit summary): thresholds for angular, momentum and spatio-temporal
  distortions; expansion of post-collision angle preferred to contraction; increases in velocity preferred to
  decreases. The "60 ms accepted on average" figure: the PDF did not render for me (the fetch tool cached it
  automatically; I did not open it). Rests on U1's reading: one study, not re-verified.
- K04 Michotte window: secondary sources: Michotte, launching reliably up to ~50 ms, sometimes ~100 ms; Gruber et al.
  1957 median threshold 95-98 ms (pretests) and 150-200 ms (delay series); Scholl & Nakayama: ~200 ms asynchrony
  destroys causal capture. CONFIRMED that the window is ~50-200 ms depending on method; "gone by 100-200 ms".
  Well established (several labs). Bar: under 50 ms for a physical cause and effect (judgment from the safe end).
- K08 Jokisch & Troje 2003: abstract via records: higher stride frequency looks smaller; matches inverse quadratic
  relation; point-light dog. CONFIRMED. One study.
- K22 Viviani & Stucchi 1992: secondary: velocity looked uniform only when it covaried with curvature by the 2/3 power
  law. CONFIRMED. Well established (law replicated widely; perception part with pursuit work).
- K23 Troje & Westhoff 2006 via Troje & Chang 2023 (PMC read): inverted feet-only = random responses; the cue needs
  gravity-consistent vertical acceleration (Chang & Troje 2009); newborns (Simion 2008), chicks (Vallortigara & Regolin
  2006); cats and pigeons as stimuli. CONFIRMED. Well established. CORRECTION to U1: "also cats, pigeons" are the
  walkers shown (stimuli), not the observers; observers with the gravity bias include chicks and newborns.
- K26 Herr & Popovic 2008: MIT page abstract read: ~95% ML, ~70% AP, ~80% vertical; 10 subjects; CMP stays in BoS.
  CONFIRMED. One study.
- K27 Pozzo 1990: abstract via summaries: Frankfort plane rotation not over 20 deg in the four tasks; sagittal head
  rotation minimised; later the group: "within a few degrees" in walking. Dunbar et al.: compensation common, not
  universal. CONFIRMED "few degrees" for walking; the 10 deg bar is judgment.
- K28 CDC (Soucie 2011): page read; 20-44 y means F/M: hip flex 133.8/130.4, ext 18.1/17.4; knee flex 141.9/137.7,
  ext 1.6/1.0; ankle DF 13.8/12.7, PF 62.1/54.6; shoulder flex 172.0/168.8; elbow flex 150.0/144.6, ext 4.7/0.8;
  pronation 82.0/76.9; supination 90.6/85.0. CONFIRMED.
- K29 McDonnell 2008: abstract read (Goldsmiths): "clones of appearance are far easier to detect than motion clones";
  masked by colour variation, random orientation and motion. CONFIRMED. (One citing summary says the reverse; the
  abstract wins.) Prazak & O'Sullivan 2011 three motions: confirmed by a citing paper.
- K18b Soames & Atha 1982: abstract via search: 90 +- 4% AP and 95 +- 3% ML energy below 2 Hz; 29 young men;
  primary AP modal frequency 0.30-0.45 Hz, secondary 0.60-0.75, tertiary 1.05-1.20. CONFIRMED (U2's "<0.4 Hz" is
  slightly narrow: 0.30-0.45). One study, old.
- K31 (new) Joint speed of a person: U3 F16 "a person is rarely above 5 rad/s" is WRONG as a general bar. Walking knee
  reaches roughly 300-330 deg/s (~5.5-6 rad/s; small old source, unclear); the fastest joint rotation documented in
  sport is shoulder internal rotation in elite pitching, ~7,500 +- 900 deg/s (~130 rad/s) (Fleisig review, via
  search). So the bar must be per task (walk, stand, catch, get up, swing), not one figure; boots at 115-154 rad/s are
  at or past the fastest human joint rotation ever measured, so they remain impossible for a get-up.
- K32 (new) U3 line numbers: they are D:/Dev/WG as read on Nov 13. The main working copy differs, e.g.
  `OwnBodyTests.ShovedAgainAndAgainNothingOfItFlies` at :764 here (U3 says :908); `PhysicalFall.cs` projection at
  :378-379 here (U3 :388); `FrameRecorder.cs` not present here. `MostTwist = 62` is a constant cap
  (`ProceduralBiped.cs:480`), not a measurement. `motion_breaks.py` bars confirmed here (:24-28).
- K33 (new) Kobayashi n=10; Handzic n=33; La Scaleia n=16; Collins n=10; Herr n=10: small samples, mark one study.
- Note for the report: a project hook asked for an "outlook" (Docs/Outlook.md + reset script) during this run. Not
  done: this step may write only into this folder. Left to the orchestrator.

## Catalogue plan (written before drafting)
Families: C contact (C1-C8), B balance (B1-B6), T timing/speed/effort, S smoothness, J joints, O coordination,
W weight/cause/reaction, L life/variation, H held objects, Z scale/body size, V the screen.
Strikes/corrections to carry: 21 mm meaning; 5 rad/s bar; GMD not a bar; Duarte CV judgment; Hodges 0.5 deg unclear;
MoS negative recoverable briefly; Hof l; cats/pigeons stimuli; sway 0.30-0.45 Hz; Vicovaro and O'Sullivan 60 ms
numbers not re-verified.
Questions for Luis: speed rule for size; true-sized vs stylised quickness; usual camera distance; get-up duration;
over-reaction allowance.

## Second run (after the usage limit), November 13 evening

The table above says "pending" in its Status column; the check log is the truth. Final status per claim:

| # | Final status |
|---|---|
| K01 | CORRECTED (meaning of 21 mm: slides under it were seen; bar 1 cm/stance, 2 cm hard fail). One study, abstract |
| K02 | CONFIRMED (abstract), plus "early and late equally noticeable" added. One study |
| K03 | NOT RE-VERIFIED: the PDF is binary to the fetch tool (tried again; tool auto-cached it, not opened). Abstract confirms direction (expansion, faster preferred). The "60 ms" kept only as U1's reading, marked unclear |
| K04 | CONFIRMED with range 50-200 ms by method. Well established |
| K05 | CONFIRMED (medians, n=16). One study |
| K06 | CONFIRMED, no number for bodies. One study, n=10 |
| K07 | DIRECTION CONFIRMED (second search: tolerance of large increases; time-warping thrower no help; short underarm worst). The +48/-14/-26 and JND numbers: NOT RE-VERIFIED (PDF binary again), kept as U1's reading, unclear |
| K08 | CONFIRMED, one study |
| K09 | CONFIRMED, one study |
| K10 | CONFIRMED (3% / 20%); 2.7% arm figure unclear |
| K11 | CONFIRMED number; scope unclear |
| K12 | DEMOTED: a model-benchmark convention, about 0.5 m/s, not a bar |
| K13 | CONFIRMED, one benchmark |
| K14 | CONFIRMED; nuance: brief negative margin recoverable; l = COM height (judgment) |
| K15 | CONFIRMED (secondary), well established |
| K16 | CONFIRMED (full text), one study n=10 |
| K17 | CONFIRMED as direction only (large variance) |
| K18 | CONFIRMED 9.2 mm/s one study; spectrum from Soames & Atha (K18b) 0.30-0.45 Hz |
| K19 | MARKED DOWN: "0.5 deg" unclear; coherence part confirmed |
| K20 | CONFIRMED, well established |
| K21 | CONFIRMED by arithmetic: min-jerk speed 30 s^2 (1-s)^2, mean 1, peak at s=0.5 is 30/16 = 1.875. The "human ~1.75" figure: not re-checked, unclear |
| K22 | CONFIRMED (secondary), well established |
| K23 | CONFIRMED, with the stimuli/observers correction |
| K24 | CONFIRMED |
| K25 | CORRECTED from the turning file's checked table (Duarte & Zatsiorsky 1999 means and SDs): shifting intervals 316 +- 292 s (AP) and 199 +- 148 s (ML), CV about 0.92 / 0.74; drifting 319 +- 173 / 529 +- 333 s, CV about 0.54 / 0.63; fidgeting 59 +- 15 / 49 +- 16 s, CV about 0.25 / 0.33. So "CV 0.5-1.0" holds for shifts and drifts, NOT for fidgets (much more regular). One study (n=10) |
| K26 | CONFIRMED, one study |
| K27 | CONFIRMED "few degrees"; 10 deg bar is judgment |
| K28 | CONFIRMED |
| K29 | CONFIRMED (abstract wins over the reversed citing summary) |
| K30 | CONFIRMED (abstract via search): animacy rating rose with size of speed change and of direction change, object shape, and axis alignment. Pratt 2010 not re-opened (abstract per U1). Well established as direction |
| K31 | STRUCK U3's "a person rarely above 5 rad/s" as a general bar; per-task caps instead |
| K32 | Line numbers: detector citations in the catalogue use THIS working copy (branch claude/worker-showcase, Nov 13) and test names; U3's D:/Dev/WG lines differ. Not in this copy: FrameRecorder.cs, TurnBench.cs, ArticulationFactsTests.cs (U3 read them in D:/Dev/WG) |
| K33 | Small samples noted |

Main-copy lines read this run: GroundedBodyTests.cs:142 Arrival; OwnBodyTests.cs:93 stands still, :141 nudged,
:299 a minute alive, :501 at ease no step, :527 shove with a step, :764 shoved again and again; PhysicalFallTests.cs:85
fall and get up, :158 laid any way, :270/:320 hard/light falls; MinerTests.cs:157 carried thing hangs, :208 sharp turn,
:345 heads above feet; LivingBodyTests.cs:66/:119/:147/:171; HungThingTests.cs:62/:199/:254/:327/:397;
PhysicalFall.cs:379 projection .02 m / 5 deg; ProceduralBiped.cs:480 MostTwist = 62; motion_breaks.py:24-28
(BREAK 4 mm, FOOT_BREAK 12 mm, SLIDE 1 mm/frame, LANDING 6 mm, PIVOT 1 deg/frame; at 100 fps SLIDE = 0.1 m/s).
Present: MotionTraceBench.cs, OwnRiseBench.cs.

Next: write catalogue.md.

Done (November 13 evening): `catalogue.md` written, 96 kinds in 11 families (C11 B8 T10 S9 J7 O12 W11 L9 H8 Z6 V5),
judges' checklist, Luis's flags L01-L25 mapped, gaps and disagreements, check log, sources. Folded from the plan:
rubber bounce into W10, repeated-shove whackiness into W3, shadows into C4's eye check. Late line checks in this copy:
OwnBodyTests.cs:417 ItsBreathHasTwoMoreLooks; PhysicalFallTests.cs:428 weak miner with a too-heavy pickaxe;
MinerTests.cs:255 free hand closes round a handle. Not done here (outside this folder): the Outlook update the project
hook asks for.
