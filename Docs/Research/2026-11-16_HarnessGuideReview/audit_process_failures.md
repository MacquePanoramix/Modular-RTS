# Audit: this project's recorded process failures (November 16, 2026)

**Status: done, read-only.** Written for the harness guide review. A process failure here is a moment where the way of
working (not the physics or the code) cost work or misled someone. Memory files are named by file name (in
the assistant's memory directory, which is outside the repository). "Came back" means the
same kind of failure is recorded again after the rule was written.

## A. The failures

| # | Date | Source | What happened | Cost | Rule written after | Where it lives now | Came back? |
|---|---|---|---|---|---|---|---|
| 1 | Sep 30 | Validation.md:640 | Checkpoint A was reported 89/89; it hid a test that stalled about 3 runs in 10. | A false "all pass"; an erratum. | Run again before trusting one pass (erratum only). | Validation only | Yes: night light test fails now and then, left "not explained" (Validation.md:3496) |
| 2 | Oct 2 | explorations-additive | A big look replaced the loved one by default; Luis saw nothing until it was mostly built. | Most of a day not adopted; Luis felt guilty. | New looks are switchable options; show 2-4 frames in the first hour. | memory only | No |
| 3 | Oct 2-8 | luis-said-which-look | Far-off lights: a cause was guessed, grass tuned, "fixed" reported. Luis had to report it again. | A week; Luis asked twice. | Sweep the variable Luis names before naming a cause. | memory only | Partly: see 17 (guards before counting) |
| 4 | Oct 3 | proactive-polish | Luis had to point out too many small flaws; asked twice for proactive polish. | Luis's time. | Seven-pass model method. | memory + ModelQualityMethod.md | Yes: Oct 15 satchel looked mended from one side only (proactive-polish) |
| 5 | Oct 4, 7 | unity-validation-runs | A Blender render, then GIF encoding, beside the suite made tests fail falsely. | Wasted runs; false failures. | Nothing heavy beside the suite. | memory + Handoff rules | No recorded |
| 6 | Oct 5 | change-only-what-is-asked | A note on the strap's end; the whole strap was restyled by Claude's own rule. Luis preferred the old. | A redo. | Change only what the note names. | memory + ModelQualityMethod pass 6 | No recorded |
| 7 | Oct 5 | unity-validation-runs | A laptop shutdown killed a 40-minute final validation. | A lost run. | Commit and push before long runs. | memory only | Yes: long workflows cut off 3 times Nov 4 by limits (usage-budget-aware) |
| 8 | Oct 6 | prototype-purpose-first | Mining reported done; it was a stored curve with no weight. All tests and audits passed, as none asked about weight. | Six days off the purpose; Luis worried. | Say what a stage answers in Luis's terms; no polish of stand-ins. | memory only | Partly: Oct 30 judges passed what Luis rejected (20) |
| 9 | Oct 7 | measure-all-three-miners | Tiredness measured on Round only; Long never resumed work, in the build since step 5. Pick-up run at strength 1 only. | A defect shipped for steps. | All three miners, whole cycle, several strengths. | memory only | Yes: get-up searched on Small only; Small's way fails others (search-the-poses, getting-up-on-the-standing-body) |
| 10 | Oct 8 | unity-validation-runs | 150 passing tests; the built player found Small falling 5 s into any long walk. No test walked far. | A defect two steps old. | Run the built player before calling a stage done. | memory only | Yes: rows 14, 16, 20 (rig not the game) |
| 11 | Oct 8 | no-fake-animations | Clips judged by "did it complete"; Luis saw stutters, teleports, a posed get-up. Weeks of 25 fps clips hid one-frame jumps. | Weeks; Luis's playtest. | Trace every frame, numbers first. | memory + MotionJudging.md | Yes: row 20 |
| 12 | Oct 8-9 | patch-scripts-with-write-tool | Bash heredoc halved backslashes; patches failed. Already noted once before. | Four failed patches. | Scripts with backslashes go through Write/Edit; assert each old text once. | memory + Handoff rules | Yes: "bit again twice on November 10" (Handoff.md:968) |
| 13 | Oct 9, 14 | search-the-poses; getting-up-on-the-standing-body | Pose searches gamed what they were asked: lay on its back, feet planted; head ahead; a ball; sat down. | Several searches. | Ask for the thing itself; say what must not be; read pictures. | memory only | Yes, three times on Oct 14 |
| 14 | Oct 10 | test-what-luis-plays | Default flipped; ~160 tests pinned to the old body, so they stopped exercising what Luis plays. Build already copied when a sceptic saw mining never ran as played; it failed 3 of 3. | Near hand-over of a broken flow. | `-ownAsPlayed 1` run before any build after a default changes. | memory only (flag in Handoff) | Yes: "one test failing again as played since Oct 14 (which, not read)" (Outlook.md:264) |
| 15 | Oct 10 | say-what-must-be-switched-on | Luis judged with the switch off; "no difference" read as a verdict; a round spent enlarging the breath. | A wasted round; a wrong size. | Switch in the report's first line; a list of what to switch on, do, watch, tell. | memory + Handoff rules | Yes: Luis asked twice what to judge (Nov 13 night, Nov 15) |
| 16 | Oct 10 | agent-reviews-always; breath-gentle-and-scaled | Six judges of six could not see the breath at sheet size; the trace measured it. | A round; a pull to enlarge it. | Check the thing shows at sheet size; overlays and scales. | memory only | Yes: row 20 |
| 17 | Oct 10 | runs-repeat-courses | "5 runs of 23 before, 0 of 17 after": 23 runs were about six repeating courses. Reviewers took it apart. | A false mend claim. | Count courses, force the event, measure each change alone. | memory (Handoff mentions courses) | Yes: Oct 11 a day's gain given to four rules, a search's own (same file); Oct 29 runs of 3 copies differ by 8 points (learned-getting-up) |
| 18 | Oct 10 | Validation.md:10; CURRENT_STATE.md:1483 | Validation's opening and CURRENT_STATE's "not yet built" list were stale since September; an outside review saw it. | Misleading entry pages. | Put right (no standing rule). | nowhere | Yes: CURRENT_STATE still said Oct 22 weeks later (Outlook.md:649); see D for Handoff now |
| 19 | Oct 13 | read-results-before-claiming | Commit said 8 of 8; the run was 7 of 8. The commit had been chained behind the test. | A false line in a commit and three documents. | Read the totals, then write, in a separate command. | memory + Handoff rules | Kind returns: K1 said more than the logs, corrected twice (Nov 10, engine-step-adds-energy); K4(a) reading withdrawn (Nov 13, Handoff.md:27) |
| 20 | Oct 13 | bench-floor-is-level | Step stood 24 of 24 on the bench, 1-2 of 4 in the game. Bench floor level, plain stand; its closing step never exercised. | A day. | Run bench results on the meadow; side-by-side tick traces. | memory only | Yes: each rise way stood only where searched, 0 of 18 elsewhere (getting-up-on-the-standing-body) |
| 21 | Oct 13 | getting-up-natural | One shove tested; Luis pressed it many times; a boot flew at 41 m/s. | A visible fault in Luis's hands. | Test twice and ten times fast. | memory only | No recorded |
| 22 | Oct 15 | unity-validation-runs | `-testFilter WonderGather` also ran the run-only-when-asked benches; over an hour, still in a bench. | An hour+. | Run the whole suite with no filter. | memory only | No recorded |
| 23 | Oct 14-29 | Outlook.md:595, 621 | The work branch suite not run from Oct 14 while four core classes changed; flagged in several outlooks. | Regressions unread for two weeks. | None beyond the outlook flag. | Outlook log only | Unclear |
| 24 | Oct 26-27 | learned-getting-up | The training school's "body alone" control had an attached silent agent holding the hips; every baseline before Oct 27 had it. The 72 fresh falls were reused, learnt by heart. A settle of 0 skipped the weight check. | Wrong baselines; inflated scores. | Measure with `-trainFreshFalls 1`; check whether starts repeat. | memory only | Yes: bench body alone 25% vs school 10%, gap unexplained (same file) |
| 25 | Oct 30 | every-frame-as-played | Three judge rounds said "reads as real, with a flaw"; Luis saw twitches, broken arms, flicking limbs at once. Bench, 0.08 s, one side. | A build Luis found goofy; scope widened. | Every rendered frame, as played, several cameras, a detector per fault. | memory + catalogue (Research/2026-11-13) | Not yet tested: new motion not shown since |
| 26 | Nov 4, 11-12 | usage-budget-aware | Long workflows hit the 5-hour limit three times; three Opus reviewers used a whole window in five minutes, twice, returning nothing. | Lost summaries; Luis's "continue". | One agent at a time, tight brief, file written early. | memory + Handoff ("Start here") | Yes: Nov 13 two workflows side by side took 44% in 25 min (same file) |
| 27 | Nov 10 | engine-step-adds-energy | Two days of guards on the keeper before the energy was counted; the engine's told speeds inflated the first count. | Two days; a write-up corrected twice. | Count energy first; check the instrument on a one-part case. | memory only | Yes: row 28 (instrument unchecked) |
| 28 | Nov 13 | articulation-turns-after-links; Handoff.md:24 | Solver turns set before links were silently lost. All own-body work since October, and step 0's rigs, ran at 6/1, believed 14/4. | Weeks of tuning at the wrong setting; a withdrawn reading. | Set root settings after the last link; read back before trusting a result. | memory (+ Handoff state) | Unknown |
| 29 | Nov 10 | compact-when-it-costs | The compact setting did not take effect in the session that wrote it; the session reached 575k. | Usage. | Check `get_usage` early; past ~250k tell Luis. | memory + AGENTS.md | No recorded |
| 30 | Nov 14 | unity-validation-runs | A batch test needing a window waited 16.5 h; its timeout did not fire; Luis noticed before Claude did. | 16.5 h; Luis's attention. | Wall-clock LIMIT on every run; check logs past expected length. | memory only | No recorded |
| 31 | various | unity-validation-runs; CURRENT_STATE.md:175, 223 | Leftovers: `InitTestScene*.unity` files, a stale lockfile, `D:/Dev/WG-night`, `risebuild3.log`, `git add -A` carrying a settings file, a stale codex branch. | Failed launches; weeks on the waiting list. | Delete after stopped runs; stage by path. | memory + Handoff ("Stage by path") | Yes: stray log and worktree listed for weeks before deletion |
| 32 | Nov 15 | plain-words-questions | 5 of 12 questions not understood: technical shorthand. | A round of questions. | Plain words, what he would see, a picture. | memory only | No: the re-asked five landed at once |
| 33 | Nov 15 | Handoff.md:96 | A planned pre-review of the five calls was not run; its questions deferred to later judges. | Not yet any. | (recorded honestly) | Handoff | Open |

## B. Tally by kind (most frequent and costly first)

1. **Rig, bench or test set differs from the game as played** (8, 10, 14, 20, 24, 25, 28): seven, the costliest (days to
   weeks each; row 28 touched everything own-body).
2. **A pass that did not exercise the thing** (1, 8, 9, 14, 20 closing step, 22, 24 settle check): six or seven.
3. **Judges or searches that could not see, or were gamed** (11, 13, 16, 25): four, with three or more repeats each.
4. **Claim before the original was read, or beyond it** (3, 17, 19, 27, 28 reading): five.
5. **Luis misled or asked twice** (3, 4, 15, 32): four; each costs his time directly.
6. **A rule that existed and was broken anyway** (12 again, 15 again, 26 again, 9 again, 17 again): five repeats.
7. **Stale entry pages trusted** (18, and D below): two recorded, more live now.
8. **Leftovers and run hygiene** (5, 7, 22, 30, 31): five, cheap each, but 30 cost 16.5 hours.

Pattern: most rules were written right after the failure, in a memory file; about half of the kinds came back. Rules
that are a tool or a switch (`-ownAsPlayed`, `LIMIT`, `-liveEnergy`) held better than rules that are a sentence.

## C. P1 to P36 against this project

- P1 narrowest true claim: answers 8, 19, 25; partly practised ("liked, not accepted": getting-up-natural; Windows of PROJECT_CULTURE).
- P2 typed outcomes: answers 1, 23, 30 (a hang read as running; "not explained" left as is).
- P3 proof of exercise: answers 8, 14, 20, 22, 24; partly practised (`-rockAsItWas` shows the check fails before, runs-repeat-courses).
- P4 controls for every check: answers 24 (settle 0), 27 and 28 (instrument unchecked), Validation.md:1589 (sweep wrong three ways).
- P5 judges calibrated: answers 16, 25; practised partly (blind A/B sheets, stance-atlas-and-free-limbs; findings checked against trace).
- P6 original over summary: answers 3, 27; practised (Correspondence verbatim; judges given figures not opinions).
- P7 counts are not correctness: answers 8, 10, 17 ("150 passing tests").
- P8 finish is a useful received result: answers 8; practised (prototype-purpose-first).
- P9 whole terminal boundary: answers 1, 30, 7.
- P10 temporary things closed: answers 31.
- P11 truthful owned block: answers E (no single list); partly practised (Handoff "Waiting on Luis").
- P12 old instructions do not reactivate: answers D (Handoff "Start here" of Nov 12 still says "Begin there").
- P13 additions earn their cost, retire the old: answers D (rules never retired; memory grows, 47 entries).
- P14 four meanings of "can": no recorded need here.
- P15 unknown outcome looked at before retry: practised (usage-budget-aware: "read files and journals before re-running"; unity-validation-runs: rerun failed tests alone).
- P16 involve the director early: practised (explorations-additive: frames in the first hour); answers 2.
- P17 count director interventions by reason: answers 3, 4, 15 (asked twice, three times); not practised.
- P18 kinds of statements apart: practised (Locked/Decided/Open; plain-words-questions "not Locked").
- P19 three parents of a surprise: answers 15 ("no difference" = missed switch), 3.
- P20 full-detail reference first: answers 20, 25 (bench as cheap stand-in for the game).
- P21 ownership carries work: practised (tight briefs, usage-budget-aware).
- P22 a review has a reason: practised (physicist/sceptic/builder, agent-reviews-always); answers 25 (same-kind judges agreeing).
- P23 proportionate process: practised (AGENTS.md, PROJECT_CULTURE §5); but see D (drift with "every step reviewed").
- P24 cold-entry test: answers 18 and D (entry pages stale or very long); never run here.
- P25 peer architects: no recorded need here.
- P26 measure a process change against the old way: answers 17 (search confound) by analogy; no process change measured.
- P27 recheck after setup changes: answers 28 (setting lost silently); model switch Fable to Opus handled by Handoff.
- P28 synthesis keeps originals: practised (Correspondence verbatim).
- P29 one owner per rule: answers D (rules in memory, Handoff and AGENTS with drift).
- P30 subset to the worker: practised (copied review folders, agent-reviews-always).
- P31 what a claim does not establish: practised ("Not tested" in Validation.md, e.g. :3205, :3358).
- P32 bars frozen before the run: practised (every-frame-as-played; Handoff "bar set before its run").
- P33 disclose what a test faked: answers 20, 24, 14 (level floor, silent agent, pinned old body); lesson only, no rule.
- P34 missing fact holds only what depends on it: practised (Handoff "The work goes on meanwhile").
- P35 serialise the short consequential act: practised (nothing heavy beside the suite; mechanical steps alone).
- P36 public sources: no recorded need beyond row 25 and the judge questions.

## D. Where the rules live

**Only in memory files** (a sub-agent does not load memory; only AGENTS.md is loaded by every agent): test as played
before a build (`-ownAsPlayed`); count courses and force the event; bench results run on the meadow; all three miners,
several strengths, whole cycle; press a button many times; set articulation root settings after links and read back;
count energy first and check the instrument on one part; search gaming (ask for the thing itself); judges' protocol
(three, sonnet, not told what changed, check findings against trace, sheet size); every rendered frame as played; stop
only the worktree's Unity; no `-testFilter`; wall-clock LIMIT; change only what is asked; sweep the named variable;
explorations additive; plain-words questions. Handoff "Rules that have cost work before" (Handoff.md:965) holds six.
AGENTS.md holds none of these.

**Stated twice and drifted:**
- AGENTS.md calls `CURRENT_STATE.md` "the short current-state router"; it is 108 KB, and its own first line sends the
  reader to Handoff.md. Handoff.md says read AGENTS, CURRENT_STATE's top entry, then Handoff. PROJECT_CULTURE §3 omits
  Handoff entirely.
- Review: AGENTS.md and PROJECT_CULTURE §1, §5 say fresh review only for silent consequential changes, "usually
  unnecessary for gait tuning". Memory agent-reviews-always and Handoff.md:971 say every step is reviewed. These
  disagree; Luis's Oct 9 word stands behind the second.
- Handoff.md "Start here" is headed November 12 and says "Begin there" with step 0's rigs (Handoff.md:29), while
  "Waiting on Luis" orders the ankle build first. An old "next" sits in the entry section.
- Handoff.md:65 says the newest build Luis has is October 30 and "nothing since changes what he plays"; he has played
  `WindowsOrdinaryPlace_Nov13` (luis-loves-the-idle). Appears stale.
- Model words: memory fable-window-and-handoff says builders on 'opus', judges on 'sonnet'; usage-budget-aware says
  medium effort, one agent; Handoff says "max thinking with ultracode". Not a contradiction, but three phrasings.

**Look obsolete or need a date check:** learned-getting-up (long training log; learnt addition now off in the game,
Nov 15); bodys-own-keeper-pitfalls ("it cannot step yet", Oct 10; stepping was built later); getting-up-on-the-standing-body
"State Oct 14 night" lines; PROJECT_CULTURE §8 "current expedition" (equipment and faction meaning, not the getting up);
CURRENT_STATE "Housekeeping" (branch merges "after Luis playtests the stage (including the arrival fix)", September).

## E. Known-red things

Known: twelve tests failing as played "since October 10" (Validation.md:3308, CURRENT_STATE.md:419, 531), never
named in the places read; one night light test flaky, "not explained" (Validation.md:3496); one test "failing again as
played since Oct 14 (which, not read)" (Outlook.md:264). Switches built and left off: `OwnBody.Anatomical`,
`OwnBody.Weighed` (breaks standing), `MinerBody.FlapsOnTheirSeams` (later on), the three ankle switches, five delegated
calls "each an option behind a switch, off" (Handoff.md:79, 93, 101, 651). Waiting on a condition: the ankle build
(after the weekly reset), target research's five corrections (after the reset), group B questions (after the crowd
measurement), R1 (until what caps Round's ankle is found, Outlook.md:279), `PhysicalFall.TriesByItsOwn` cap (until the
roll exists, never-the-backup-animation), the old way after 12 tries (until the new way stands, any-body-direction-decided).

**Is there one place?** No. Handoff "Waiting on Luis" covers Luis's items; Outlook's "Open, not to be forgotten" lines
carry the rest forward as "as the thirty-second, plus"; known red tests sit only inside run reports. None lists, for
each item, what reopens it.

## F. What I did not read

Handoff.md beyond the three sections (lines 142-964: "In hand", "Next steps"); Outlook.md, CURRENT_STATE.md and
Validation.md beyond Grep hits and short context; GAME_VISION.md, DESIGN_RATIONALE.md, NextMilestonePlan.md (one Grep
hit only); Correspondence; Reviews; the worktree's tools README; ModelQualityMethod.md, MotionJudging.md; the review
folder other than notes_main_guide.md. Git history was not searched. Searches were by keyword, so failures recorded in
other words may be missed.
