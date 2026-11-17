# Cold entry, before (November 16, about 17:15 local by the machine clock)

A fresh master session's entry by the ordinary route. Nothing changed, committed or started. Line numbers are as read.
Abbreviations: CS = Docs/CURRENT_STATE.md, HO = Docs/Handoff.md, GV = Docs/GAME_VISION.md, OL = Docs/Outlook.md,
VA = Docs/Validation.md, AM = Docs/Research/2026-11-15_NaturalRangesAndTheAnkle/research.md.

## 1. The goal of the current milestone

S3b, "the body's own": the miners' movement pushed by their own joints, stable and alive (Docs/NextMilestonePlan.md:1),
now widened by Luis to the final game's getting up: any body the creator can make gets up by its own joints, never a
played animation (HO:74-75; GV rows of Oct 30 to Nov 9). In practice, at present, the work alternates the get-up
(the ankle mend, so the get-up stops blowing up) with a visual programme (HO:99-115; GV:548, question 12).

## 2. The exact next action

The weekly reset is said to be November 16 "about 17:00 his time" (HO:103) / 16:00 UTC (HO:902, HO:948). The clock
reads 17:15 WEST, so it has probably just passed. Then the ankle mend's build in D:/Dev/WG (HO:869-876; GV:552):
1. Read AM section 8.1 (AM:647-677) and work Long and Round numerically (HO:870).
2. In D:/Dev/WG (branch claude/wip-round, clean of my edits): set `RiseAgent.Unheard` true by default; teach the live
   test `-ownUnheard`, `-riseSet`, `-ownBodySet`, `-liveStrength` (AM:547-550; HO:871-872).
3. Build the three switches, all off: A `OwnRiser.AnkleLimit`, B `OwnRiser.AnkleSpeed`, C `OwnBody.BootsFromOwnShin`
   (AM:482-488; HO:856-860). Builder opus, fresh reviewer (HO:971-972).
4. Forced checks first (AM:536-541), then the baselines and arms in order, one run at a time, each read before the next:
   `LIMIT=15000 bash Docs/Technical/AnyBody/tools/unity_tests.sh ank0_small WonderGather.Tests.CoverRingLiveTests <A> -liveWho Small ...`
   then ank0_long, ank0_round, ank0_small_unheard, ank1 (B), ank2 (A=1), ank3 (A+B), ank4 (A+B+C), per miner, 24
   courses per arm (AM:551-569, AM:517-534); the idle's tick-by-tick proof (AM:504-515).
5. Because a default changed: the flows as played (`-ownAsPlayed 1`) (HO:876; memory test-what-luis-plays); then a
   build for Luis with a short list of what to switch on, do, watch and tell (HO:124-125).

Finished when: the switches exist off, forced checks pass, every arm's 24 courses are read and recorded in VA, the idle
is bit-identical, the as-played suite fails nothing new, judges have seen any visible change, and the report/build is
sent to Luis. Confidence: moderate (about 70%). Unsure because: (a) whether the reset really passed and how much
allowance is left (only Luis/the UI can say); (b) HO:107-108 also puts the target research's "five small corrections"
"after the weekly reset, before tier 0" and it is not said whether they come before the ankle build; (c) HO:10-30
"Start here" points elsewhere (step 0's rerun), see 8.

## 3. Waiting on Luis / Luis waiting on me

- On Luis: nothing right now (HO:117). Next questions come with the work: group B after the crowd measurement, the
  turning proposal, `AnkleBrakes`, the natural ankle (HO:117-118; GV:552 "AnkleBrakes ... Open, his").
- Luis waits on: the ankle mend's build and the next build to play (HO:124-125); the first motion clips after step 2
  (HO:138); step 0's stills of the models (HO:900-901). The Nov13b build is off his list (HO:120-121).

## 4. What must not be touched; standing holds

- Luis's uncommitted files in the main project: TheGroup.unity, PackageManagerSettings.asset, URPProjectSettings.asset,
  Assets/_Recovery*; never committed, reset or discarded (HO:67-69; CS:1534-1535; AGENTS.md). Set by Luis/the agreement.
  No lift stated.
- In the worktree, EditorBuildSettings.asset and ProjectSettings.asset out of commits; stage by path (HO:69-70).
- No merge to main without Luis's word (HO:34, HO:132). Lifts on his word.
- Unity only in batch mode in D:/Dev/WG (HO:62); nothing heavy beside the suite; no code edits during a run (HO:969).
- Do not spend more on guarding the keeper; mended by folding into the new controller (HO:253; set by the K1 finding,
  Nov 10). Lifted by the controller (design §2.10).
- R1's unfloored share must not run until what caps Round's ankle is found (CS:112-113; independent review B, Nov 13).
- C never alone; order B, A=1, both, then C (AM:532; HO:861).
- The three switches off by default; turning any on, the natural ankle and joint-space asks go to aesthetic judges,
  then Luis (GV:552). `AnkleBrakes` changes strength: Luis's (GV:552).
- Small's learnt addition off as played, kept for training (GV:552; memory learned-getting-up).
- The idle Luis loves must not change (AM:492; memory luis-loves-the-idle).
- Weighed centres (`OwnBody.Weighed`) off, "wait" (HO:79); no lift stated.
- The visual programme must not build before the target exists (OL:78); the five corrections before tier 0 (HO:107-108).
- Usage: one or two agents at a time, tight brief (HO:35-36); "stop heavy work until the weekly reset" (OL:81-82),
  lifted by the reset.
- Open, not to be promoted: unit count, head proportions, how far the models' redraw goes (GV:543, 546, 549, 553).

## 5. Luis's decisions of the last week that bind the next step

- Nov 15 afternoon: build the ankle mend ("Sure."), learnt addition off as played ("option a") (GV:552).
- Nov 15 afternoon: target questions A answered as recommended; every common screen must work (GV:553).
- Nov 15 morning: visual work and getting up take turns; everything explored as switches off (GV:548); five delegated
  calls approved (GV:551).
- Nov 14: flaps on their seams on by default (GV:542); models redrawn, depth Open (GV:543); order energy count first,
  then pictures (GV:546).
- Nov 13: solver turns 14/4, on (GV:540); six turning recommendations approved; research first, check rigorously
  (GV:554).

## 6. Unfinished, in flight, left behind; actual state

- Actual (read-only): main project on claude/worker-showcase at 98b152d, in sync with origin; Luis's files modified as
  listed; untracked `Docs/Research/2026-11-16_HarnessGuideReview/` and
  `Docs/Correspondence/2026-11-15_THE_FRIENDS_HARNESS_GUIDE.md` (not in the session-start snapshot; presumably the side
  task's; not opened). D:/Dev/WG on claude/wip-round at 3182e08 (Nov 15 10:28), in sync with origin; modified
  EditorBuildSettings, PackageManagerSettings, ProjectSettings; untracked Assets/ML-Agents* and four
  Resources/RiseModels/SmallRise*.onnx (+meta). No Unity.exe running; four python.exe are the Blender MCP server.
- Differences: HO:69-70 lists only two worktree files to keep out; PackageManagerSettings.asset and the untracked
  ML-Agents/onnx files are not mentioned (are the onnx models meant to be committed? the learnt addition reads them).
  HO:65 says the newest build Luis has is Oct 30; VA:5195 and GV:540 say he played the Nov 13 build. OL:68-69 says
  waiting on Luis for the build word and drivenNow; both answered (commit cab183a, HO:117).
- Unfinished: the ankle build (not started, matches pages); the target research's five corrections (HO:107-108);
  tier 0 instruments, step 0 stills; the clipping detector, flap rest and hem, turn bench (OL:75); R1's making-up not
  built (HO:198); steps 1-8 of the motion design (HO:958-960); smaller items (HO:962-963).

## 7. Known failing / red

- `ArticulationFactsTests` K5 and K4(a) fail their bars since Nov 12 (VA:5181-5183; VA:5072-5079).
- As played, 15 fail both ways, 13 from the Oct 10 list (VA:5184-5191; TheBodysOwn.md).
- `CoverRingLiveTests` own get-up blows up: 6/24 (VA:5411...), 9/24 (VA:5497...).
- The shove test: HO:230 "still fails as known"; VA:5179 says `OwnBodyTests` 16 of 16 including it. Contradiction.

## 8. The entry itself

Tally (lines read, why):
- AGENTS.md, ~45, preloaded: entry rule. MEMORY.md, ~45, preloaded: index. No memory file opened.
- CS, 1-120 and 1530-1551 plus a grep (~145 of 1551): told to start there.
- HO, 1-300 and 740-974 plus greps (~535 of 974): CS:3 and HO "to resume".
- Docs/NextMilestonePlan.md, 1-30: "next concrete milestone" in CS:1543.
- GV, grep of Nov 10-16 rows, three rows in full (~25 of 1041): the decisions.
- OL, 1-85 (of 972): AGENTS.md session-cost line and OL:15 "at the start of a session".
- VA, headings grep and 5173-5198 (~40 of 5609): known red.
- AM, headings and 482-591 (~115 of 702): the next action's content. TargetAndScale research: headings only.
- Command output: git status/log both repos, process list, tools folder listing.
Not read: DESIGN_RATIONALE, PROJECT_CULTURE, UnityProjectContext, motion_design.md (HO:17 tells to read §6-7 first).

Problems found:
- AGENTS.md calls CS "the short current-state router"; it is 1551 lines; its top bullet alone is 56 lines (CS:5-60).
- CS:5 heads its top entry "November 15, morning" but it holds afternoon items (CS:27-44). Entries are not in date
  order: Nov 13 afternoon (CS:88) sits above Nov 13 night (CS:103), below Nov 14 night (CS:62).
- CS:86 "The models' deep research is running." stale (done, CS:45).
- CS:1532-1533 "merges into main after Luis playtests the stage (including the arrival fix)": an old instruction.
- CS:1543 routes to NextMilestonePlan, whose "Where it stands" (NextMilestonePlan.md:21-30) is October 10.
- HO:10 "Start here (November 12)" and HO:21-30 "Begin there: the rigs ... step 0's misses rerun": an old instruction
  reading as current. The real next step is buried at HO:100-103 and HO:869-876 (inside "Next steps" item 2, nested
  five levels deep, under a sub-bullet about Nov 14 answers).
- HO:898 "Waiting on his answers." stale (answered, HO:86).
- HO:65 vs VA:5195 / GV:540: which build Luis has.
- HO:230 vs VA:5179: whether the shove test fails.
- OL:68-69 (latest outlook) lists waits since answered.
- Reset time given three ways: "about 17:00 his time" (HO:103), "17:00 local" (HO:236), "16:00 UTC" (HO:902, 948);
  consistent only if his zone is UTC+1, which no page states.
- GV:554 (Nov 13) placed after GV:553 (Nov 15): the register is not in date order, so "latest" needs dates read row by row.
- HO:43 "max thinking, ultracode" vs HO:238 and memory usage-budget-aware "medium effort".
- Hunting needed: the next action (HO greps), the build's commands (AM 7.3), the known-red list (VA greps).

Could not determine: whether the reset happened and the remaining allowance; whether the five target corrections or
the ankle build go first; whether the worktree's onnx/ML-Agents/PackageManagerSettings changes are protected or
leftovers; whether motion_design §6-7 must be read before the ankle build; whether the shove test passes today; what
the untracked Correspondence file is.

A short page would have needed: one dated "Next action" block at the very top (the ankle build, its first three steps,
its done-condition, and its file:line), the reset time in one zone, "waiting on Luis: nothing", the full do-not-touch
list for both repos including untracked files, the current known-red list with one source, and a note that HO's
"Start here" and the older CS entries are history.
