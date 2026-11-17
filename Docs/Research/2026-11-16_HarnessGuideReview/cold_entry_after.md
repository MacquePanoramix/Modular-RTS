# Cold entry after the mend (November 16, 2026, about 22:00 local)

A fresh Opus 5.5 session, entered by AGENTS.md -> Handoff "Start here" -> CURRENT_STATE top -> what those named.
Nothing changed, nothing committed, no Unity started. Paths below are relative to `Docs/` unless they say otherwise.

## 1. The milestone's goal
S3b, "the body's own": standing, stepping, walking, reaching and working moved by the body's own joints, stable and
alive, never posed (`NextMilestonePlan.md:1`, `:36`). Its present step, widened by Luis, is getting up for any body the
creator can make, by its own joints, never a played animation, built in layers 0-3 (`Handoff.md:152-160`).

## 2. The next action
The ankle mend's build in `D:/Dev/WG`, branch `claude/wip-round` (`Handoff.md:12-27`; approved, `GAME_VISION.md:552`):
1. Read `Research/2026-11-15_NaturalRangesAndTheAnkle/research.md` §8.1 (`:647-676`); work Long and Round numerically
   (Ω scaled by height, E and the damper floor from the gains `Drive(j, share)` writes).
2. In WG: `RiseAgent.Unheard` true by default (now `public static bool Unheard;` = false, `Assets/_WonderGather/Learning/RiseAgent.cs:109`);
   teach `CoverRingLiveTests` `-ownUnheard`, `-riseSet`, `-ownBodySet`, `-liveStrength` (`research.md:547-550`).
   Small's baseline both ways once.
3. Build switches A `OwnRiser.AnkleLimit`, B `OwnRiser.AnkleSpeed` (floats), C `OwnBody.BootsFromOwnShin`, all off
   (`research.md:482-488`, `:671`); an opus builder, a fresh reviewer (`Handoff.md:17-18`).
4. Forced checks (`research.md:536-541`), then baselines and arms B, A=1, both, then C (`:517-534`), 24 courses per
   arm per miner on Small, Long, Round, one run at a time, by `research.md:554-567`, e.g.
   `LIMIT=15000 bash Docs/Technical/AnyBody/tools/unity_tests.sh ank0_small WonderGather.Tests.CoverRingLiveTests -ownAsPlayed 1 -ownRate 50 -liveOwn 1 -liveSeam 1 -liveOther 0 -ownPushes 24 -ownPushWays 0,180,90,270 -liveCap 240 -liveEnergy 2 -liveWho Small -liveOut D:/Dev/WG/TestRuns/CoverLive/ank0_small`;
   the idle's tick-by-tick proof (`research.md:504-515`).
5. The as-played suite (`-ownAsPlayed 1`), then a build for Luis with what to switch on, do, watch and tell.
Finished when (`Handoff.md:23-25`): switches exist and are off; forced checks pass; every arm read and written in
`Validation.md`; the idle identical tick by tick; as-played fails nothing new; judges saw any visible change; build and
list sent to Luis.
Before step 1, by `Outlook.md:15-17`, an outlook is due (the last, the 34th, is November 15 afternoon, `Outlook.md:64`);
and the usage is checked first (memory `usage-budget-aware`).
**Confidence:** high on *what* (three pages agree: `Handoff.md:12-27`, `:947-954`, `CURRENT_STATE.md:33-35`). Unsure:
(a) the `-ownUnheard 1` lines of `research.md:561` were written when "heard" was the default; once step 2 flips the
default, which flag means "heard" for Small's other baseline is not said (`research.md:672` says only that the live test
sets `Unheard` itself); and `GettingUpByItsOwnTests.cs:30` (`Restore()` sets `Unheard = false`) and `:51` (sets it from
the argument) would override a default of true; no page mentions those tests; (b) whether this week's allowance and
Luis's clock allow a long run tonight; (c) the uncommitted page edits (section 6).

## 3. Waiting
- **On Luis:** nothing now (`Handoff.md:33`, `:195-196`); his next questions come with the work: group B after the
  crowd measurement; the turning proposal, `AnkleBrakes`, the natural ankle (`Handoff.md:33-34`).
- **He waits on:** the ankle build and the next build to play; step 0's stills; the first motion clips after step 2
  (`Handoff.md:34-35`). His newest build: `WindowsOrdinaryPlace_Nov13`; Nov13b is off his list (`Handoff.md:143`, `:198`).

## 4. Do not touch; standing holds (one list, `Handoff.md:51-84`)
- Main project, never committed/reset/discarded: `TheGroup.unity`, `PackageManagerSettings.asset`,
  `URPProjectSettings.asset`, `Assets/_Recovery*` (Luis's own; `Handoff.md:40-41`, `:145-147`). WG, left out of commits:
  `EditorBuildSettings.asset`, `ProjectSettings.asset`, `PackageManagerSettings.asset`, `Assets/ML-Agents*`, four
  `SmallRise*.onnx` (`Handoff.md:42-45`). Stage by path. Set by Luis/project; no lift stated.
- No merge to main: Luis's word lifts it (`Handoff.md:53`, `:210`).
- Unity batch mode in WG only, nothing heavy beside a suite, no code edit during a run: not lifted (`:55-56`).
- No more guards on the keeper (`Braced`, `BySpeed` off): the engine step adds energy; folded into the new controller (`:57-58`).
- R1's unfloored share does not run until review B's bound and Round's ankle cap are found (`:59`).
- A, B, C off, C never alone; `AnkleBrakes` Open, Luis's; any default-on goes judges, then Luis (`:60-61`).
- Small's learnt addition: off as played by Luis, still on in code until step 2 (`:62-63`).
- The idle Luis loves does not change; not lifted (`:64`).
- `OwnBody.Anatomical`, `OwnBody.Weighed` off; no lift stated (`:65-66`).
- The five delegated calls: switches, off; each look to judges then Luis (`:67-68`).
- No visual build before the target exists; five corrections before tier 0 (`:69-70`).
- `PhysicalFall.TriesByItsOwn = 4` and the old way after 12 tries: never the backup (`:71-72`).
- Open, not promoted: unit count, head proportions, how far the redraw goes (`:73-74`).

## 5. Luis's decisions of the last week that bind the next step
- Nov 15 afternoon: the mend may be built, switches off and tested; Small's learnt addition off as played, kept for
  training; any default-on, the natural ankle and joint-space asks go to judges then him; `AnkleBrakes` Open (`GAME_VISION.md:552`).
- Nov 15: the visual work and the getting up take turns, in Claude's order (`GAME_VISION.md:548`; `Handoff.md:177-193`).
- Nov 15 afternoon: looks judged on his ultrawide, every common screen must work, steady 60 (`GAME_VISION.md:553`).
- Nov 15 afternoon: Nov13b skipped (`Handoff.md:198-203`). Nov 15: the five calls approved as switches (`GAME_VISION.md:551`).
- Nov 13 night: solver turns 14/4 on; the idle "really alive" must be kept (`GAME_VISION.md:540`).
- Standing: quality above speed; every step reviewed by other agents; never the backup animation (memory index).

## 6. Unfinished, in flight, left behind (checked read-only, 22:02 local)
- Main project `claude/worker-showcase`, in step with origin, last commit `98b152d` (Nov 15 15:04). Uncommitted:
  Luis's four protected items, **and** `AGENTS.md` (+13), `Docs/Handoff.md` (+92), `Docs/Outlook.md` (+5),
  `Docs/PROJECT_CULTURE.md` (+39), untracked `.claude/hooks/guard.sh`, `guard_test.sh`,
  `Correspondence/2026-11-15_THE_FRIENDS_HARNESS_GUIDE.md`, `Research/2026-11-16_HarnessGuideReview/`. No page names
  these; Handoff's "Last updated November 16" (`Handoff.md:3`) lives only in this uncommitted copy. Treated as protected.
- WG `claude/wip-round`, in step with origin, last commit `3182e08` (Nov 15 10:28). Only the known left-out files dirty.
  `AnkleLimit`, `AnkleSpeed`, `BootsFromOwnShin` do not exist in the code: the build has not begun, although the pages
  set it "after the weekly reset" (Nov 16 17:00 his time; `Handoff.md:36`, `:180`), five hours ago.
- Processes: no Unity. Running and named by no page: JupyterLab (since 20:25) with a kernel (21:56); Blender MCP servers.
- Pages vs state: `Handoff.md:178-179` and `CURRENT_STATE.md:34-35` say "the three switches are built off and tested",
  which reads as done; it is what Luis approved, not done. `Outlook.md:71-72` still lists as waiting on Luis three
  things since answered.

## 7. Known red, and where
All in `Handoff.md:75-83`: `ArticulationFactsTests` K5, K4(a) since Nov 12 (`Validation.md`, "The solver's turns on by
default"); 15 fail as played both ways (same section; `Outlook.md` near 267); the night light test flaky (0.887/0.896
for 0.9, `Validation.md` near 3305, 3496); the own get-up blows up in `CoverRingLiveTests`, 6 and 9 of 24 (`Validation.md`
near 5450, 5533), the ankle build's target; the shove test either "fails as known" or passes 16/16: undetermined.

## 8. The entry itself
**Tally** (files, lines read, why):
| File | Lines read | Why |
|---|---|---|
| `AGENTS.md` | ~70 (pre-loaded) | entry route |
| memory `MEMORY.md` | ~45 (pre-loaded) | Luis's rules |
| `Handoff.md` (~1100 lines) | 1-221, 900-979, two greps (~300) | Start here, holds, the thread named at :25 |
| `CURRENT_STATE.md` (~1470) | 1-60 + a grep of entry heads | top entry |
| `Research/2026-11-15_NaturalRangesAndTheAnkle/research.md` | headings + 480-677 (~200) | §7, §8.1, named at :14-19 |
| `GAME_VISION.md` | grep of rows, heads of 539-556, all of 552 | decisions of the week |
| `Outlook.md` | 1-90 | "the 34th, Drift watched", named at Handoff:70 |
| `NextMilestonePlan.md` | 1-57 + headings | the milestone's goal (no entry page states it) |
| memory `usage-budget-aware.md` | 30 | allowance rule before launching agents |
| `Docs/Playtests/` listing | names | AGENTS' "current milestone playtest document" |
| git, process, `git grep` in WG | output | section 6 |

**Old instructions that read as current:**
- `Handoff.md:178-181` and `CURRENT_STATE.md:34-35`: "built off and tested" reads as done.
- `Handoff.md:180`, `:947`, `CURRENT_STATE.md:35`: "starts after the weekly reset": the reset has passed, nothing says
  whether it started.
- `Outlook.md:84-85`: "stop heavy work until the weekly reset": past, but no newer outlook replaces it.
- `NextMilestonePlan.md:21-29` "Where it stands" is from October 10 (step 3 "in hand"); the layered get-up of
  `Handoff.md:150-160` is not in it. `Outlook.md:27-28` still sends the outlook-taker to that plan's "stage table and steps".
- `research.md:561` `-ownUnheard 1` predates the default flip of `Handoff.md:15`.
**Two pages disagreeing / which is latest:**
- `CURRENT_STATE.md:3` "Updated November 15" and `:5` "Now (November 15, morning)" over afternoon content, against
  `Handoff.md:3` "November 16" (uncommitted).
- The log is "newest first" (AGENTS) but `CURRENT_STATE.md:88` (Nov 13 afternoon) stands above `:103` (Nov 13 night),
  and `GAME_VISION.md:554-555` (Nov 13, Nov 11) stand below the Nov 15 rows.
- `Handoff.md:82-83`: the shove test, two statements, the page itself says not determined.
**Hunts:** "Next steps, item 2" (`Handoff.md:25`) is one item spanning about `:320-1042`; the thread was found only by
grepping its quoted title (`:947`). "Waiting on Luis (the Nov13b paragraph)" and "near 112"/"near 5450" are
locators, not anchors. The milestone's goal is stated on no entry page; AGENTS' "current milestone playtest document"
names none, and `Docs/Playtests/` has no S3b file I could identify. GAME_VISION rows are 500-3400 characters on one line
(a plain read omits them).
**Too long to read as told:** CURRENT_STATE (1470 lines) and Handoff (~1100) were read top-and-grep only, as AGENTS allows.
**Could not determine:** whether the build began in a session that left nothing on disk; who owns the uncommitted
`AGENTS.md`/`Handoff.md`/`PROJECT_CULTURE.md` edits and the hooks (Luis, or the side task); who started JupyterLab; this
week's remaining allowance; the flag that means "heard" after the flip; which playtest document is current; the shove
test's state.
**What a short page needed to say:** "As of <date time>: next step N of 5, not begun / begun at <commit>"; the milestone
in one line; the flag semantics after step 2; "uncommitted edits X, Y are <owner>'s, leave them"; "processes you may
find running: ..."; "outlook due: yes/no"; and one stated date per page that the next session can compare.
