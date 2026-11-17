# A sceptic's review of the harness-guide changes (November 16, 2026, evening; written by an agent that did not make them)

**Shown and read.** Read in full: `review.md`, `audit_process_failures.md`, `handoff_entry_changes.md`,
`guard_hook_report.md`, `guard.sh`, `guard_test.sh`; `git diff` of AGENTS.md, PROJECT_CULTURE.md, Outlook.md, Handoff.md;
the first 60 lines of `cold_entry_before.md`; the first half of `public_evidence.md`; the memory files
agent-reviews-always, unity-validation-runs, read-results-before-claiming, patch-scripts-with-write-tool,
never-the-backup-animation, test-what-luis-plays. I also saw `cold_entry_after.md` (it exists, written 22:05; I was not
pointed to it and did not judge it). **Not read:** the friend's guide, `notes_main_guide.md`, `readers/`, the rest of the
48 memory files, most of Validation, CURRENT_STATE. **Run:** `guard_test.sh` (61 passed, 0 failed, exit 0) and ~90 made-up
payloads piped only into `guard.sh` (script: `scratchpad/probe.sh`). Nothing was edited, committed or run in Unity.
Rung of this review: read and probed, not tested as played.

## Part 1. Each row of "Adopted"

| # | Evidence row fits? | Duplicate (drift risk)? | Clash with Luis? | Verdict |
|---|---|---|---|---|
| 1 rung | Rows 8, 19, 23, 25, 30 fit (false "all pass", not run, hung). Row 1 fits weakly. | Same idea twice in this change: AGENTS line and PROJECT_CULTURE saying "Say which rung you stand on". | None. | **Mend**: keep the AGENTS line; drop the saying. Its pointer "the cases: review.md" is wrong: the cases are in `audit_process_failures.md`. |
| 2 rig claim | Rows 8, 10, 14, 20, 24, 25, 28 fit; seven of the seven are real. | Memory bench-floor-is-level, test-what-luis-plays, measure-all-three-miners, plus the saying "The workshop floor is level". | None; agrees with them. | **Mend**: a sub-agent that loads only AGENTS.md cannot know `Small, Long and Round` (three miners), "the meadow", or `-ownAsPlayed` (a test flag). Say "the three miners" and what the flag does, or scope the line to miner physics. Drop the saying. |
| 3 pass exercised | Rows 8, 9, 14, 17, 20, 24, 28 fit. Row 22 does not (it is a filter that ran slow benches, not a false pass). Row 1 is a flaky test hidden, a loose fit. | memory runs-repeat-courses. | None. | **Keep, mend wording**: "the run ended by itself" and "courses" are project shorthand; one clause each. Lazy way: write "switch read back: yes" without reading it back; nothing makes the line show the read-back value. Ask for the value quoted. |
| 4 fail on purpose | Rows 13, 16, 24, 25, 27, 28 fit. | Memory search-the-poses, every-frame-as-played; the saying "A bell that has never rung". | **Cost clash**: it says a *judge* is first shown a known-bad and a known-good case. Luis's every-step rule means three judges a step on a small Pro window (memory usage-budget-aware: one agent at a time, tight brief). Applied literally it doubles judge runs. | **Mend**: say it is for a check that decides a claim, and that a judge is calibrated once per kind, not per step. "Write the lazy way beside each bar" is met by writing "none"; it is a ritual unless a reviewer reads it. Drop the saying. |
| 5 read, then write | Row 19 fits exactly. Row 27 does not (it is an unchecked instrument, not writing early). | Three copies now: memory read-results-before-claiming, Handoff "Rules" (old line **and** the new pointer bullet, same section, lines 1051 and 1053-1054), AGENTS. | None. | **Keep** in AGENTS; delete the older Handoff clause or the new pointer so Handoff does not hold two. |
| 6 review has its own question | Rows 16, 25 fit. | The new AGENTS paragraph repeats memory agent-reviews-always in shorter form; the two will drift (memory says judges are told figures, the new line says "not what was hoped"). Compatible today. | No clash. Residual: AGENTS still says "ordinary reversible experiments stay lightweight" right above "every step is also reviewed"; PROJECT_CULTURE lines 110-112, 337 and 373 still say gait tuning / ordinary iteration needs no independent review. Only §1 got the reconciling sentence. | **Keep, mend**: add the same pointer at lines 337 and 373, or one clause at AGENTS line 42. |
| 7 Oct 9 word in AGENTS | **The audit does not show what the row claims**: AGENTS.md never said "usually unnecessary for gait tuning" (`git show HEAD:AGENTS.md` has no "gait"); that text was in PROJECT_CULTURE only. The drift is real but sits in PROJECT_CULTURE. | Memory agent-reviews-always; Handoff Rules. | It is Luis's word. | **Keep** the line; correct the row's description in review.md. |
| 8 Start here / one route | Row 18 and the cold-entry test fit. | See Part 2: the do-not-touch list now exists three times. | None. | **Keep, mend** (Part 2). |
| 9 one list | Audit §E fits (no single list). | The list copies lines owned by "Rules that have cost work before", "Where things are", Validation, Outlook. The page says "the owner wins" but the list is rewritten by hand, so a copy per fact. | None. | **Keep**; its check at each outlook is the control. Add the date of the last full check. |
| 10 new rule names its wound | Rows 9, 12, 15, 17, 26 fit. | None. | None. | **Keep**. Note: the AGENTS block itself does not name what it replaces or what drops it (that lives in review.md §7). The first user of the rule breaks it. |
| 11 guard | Rows 12, 19, 31 fit as the wounds. | Duplicates nothing. | Fits "protect human workspace state". | **Mend** (Part 3). |
| 12 outlook count | Rows 3, 4, 15 fit; 32 is a one-round miss, not a repeat. | None. | None. | **Keep**; cheap. Lazy: "0, none". |
| 13 brief carries rules | Audit §D fits. | Memory usage-budget-aware, fable-window. | None. | **Keep**; one line. |
| 14 five sayings | The audit lists "memory aids only": no failure row. Three of five are copies of rows 1, 2, 4. | Three copies above. | None. | **Drop** "workshop floor", "bell", "rung". Keep "newest signpost"; "parent" has no recorded failure behind it (fails the change's own test 1): drop or keep by Luis's taste. The §10 chronicle of the neighbour project is a diary entry in a culture page: harmless, optional. |

**AGENTS.md size and plainness:** 44 to 55 lines, about 5 KB: still short. The new lines are dense (long sentences with
lists inside) and use "Luis" where the page elsewhere says "the Game Director". By its own cited study (public_evidence 1.1) a
file loaded every session pays cost for rules an agent could guess; "say what you tested", "read then write" are of that kind.
That argues for keeping the five claim lines to the least (rows 2, 3, 5), not for the long form. The drop test in review.md §7
("no report uses these words for a month") is met by ritual use alone.

## Part 2. Handoff: dropped or changed

Diff read line by line. **Nothing was deleted.** The old "Start here (Nov 12)" body is kept word for word under "History".
Changes of meaning or loss in effect:
1. **The binding list ("What binds every step": quality above speed, never a played animation, nothing that switches at a
   threshold, one body for everything, ...) now sits under a heading that says it is "done or overtaken".** The new Start here
   does not repeat it. A cold reader who skips History loses the standing principles. Mend: one line in Start here.
2. History says "Its Begin there is done or overtaken" without a check (handoff_entry_changes "Unsure" says so). K5 and K4(a)
   are still red per the new holds list. "Overtaken" is probably right (Luis chose 14/4 on Nov 13), "done" is not shown.
3. `Waiting on Luis` (lines 178-179) still says "The three switches are built off and tested"; the new Start here says the build
   has not begun. Verified: `AnkleLimit`, `AnkleSpeed`, `BootsFromOwnShin` appear in no .cs file of `D:/Dev/WG`. Two pages
   contradict within one file. Left alone by the builder; should be mended now.
4. The change itself (AGENTS, Handoff, Outlook, PROJECT_CULTURE, guard, review folder) is **uncommitted and named on no page**
   as pending; the "Never committed" list names only Luis's files. A fresh agent cannot tell whose these are (the cold-entry test
   after reported the same).
5. "The newest build Luis has": the Oct 30 GitHub-release mention is dropped; the correction to Nov13 is right.
6. The shove test is listed "not determined". Validation 5176-5183 shows `ShovedAgainAndAgainNothingOfItFlies` passing in the
   16 of 16 and in the 240-test run whose only two failures are K5 and K4(a). Understated, not wrong.
7. The hold "`PhysicalFall.TriesByItsOwn = 4` and the old way after 12 tries" reads as a prohibition; the memory it cites says
   those *stay* until the new way stands (any-body-direction-decided item 6; never-the-backup: the cap "must go once the roll
   exists"). Reword as "left in place until ...".

**Ten facts of Start here, checked:** (1) work in `D:/Dev/WG`, branch `claude/wip-round`: confirmed by `git branch`, and
HEAD equals origin (3182e08). (2) Main project on `claude/worker-showcase`: confirmed (98b152d, equals origin). (3) "Sure."
approval, Nov 15 afternoon: GAME_VISION row 552 confirmed. (4) research.md §7.1, 7.2, 7.3, 8.1 exist at 482, 517, 543, 647.
(5) Switch names and types `AnkleLimit`, `AnkleSpeed`, `BootsFromOwnShin`: research 485-487 confirmed. (6) Order B, A=1, both,
then C, never C alone, 24 courses per arm: research 517-534 and 584 confirmed. (7) `RiseAgent.Unheard` is `public static bool
Unheard;` (false) at `RiseAgent.cs:109`: confirmed, so "true by default" is still to do. (8) Week reset Nov 16 16:00 UTC,
17:00 for Luis, UTC+1: the machine zone is UTC+1 without summer time; consistent; the block says "reset on" without saying it
has passed (it had, at 21:05 UTC). (9) "Target research section 7, After the second critique" is at line 592: confirmed.
(10) Outlook "the 34th, Drift watched" is at line 81: confirmed.
**The list whose error would hurt most: the do-not-touch lists.** Main project: `git status` shows exactly the five named
(TheGroup.unity, PackageManagerSettings.asset, URPProjectSettings.asset, `Assets/_Recovery`, `.meta`): confirmed. `D:/Dev/WG`: 13
status lines = EditorBuildSettings, ProjectSettings, PackageManagerSettings, `Assets/ML-Agents*` (2), four .onnx with metas:
all named, none missing. The same list also lives in "Where things are" (two files only) and CURRENT_STATE: three copies.
Hold-list red tests: K5/K4(a), 15 failing as played, 6 and 9 of 24 blow-ups, night light 0.887/0.896: all match Validation.

## Part 3. The guard

`guard.sh` is plain and fails open; `guard_test.sh` ran 61/61 with exit 0. Its known-bad cases are real refusals, known-good
real passes; the compact-JSON case shows a refusal on a payload form the author did not expect.
**Not refused, though in the stated scope** (each probed, exit 0): `git add ./`, `git add "."`, `git add '*'`, `git add :/`,
`git add -vA`, `-Av`, `-fA` (G1 handles `commit -am` but not clustered flags on `add`); `bash -c "git add -A"` and `sh -c '...'`
(the match needs a space before `git`); `git --no-pager reset --hard`; `git checkout -f`, `git switch -f`,
`git switch --discard-changes`, `git reset --merge`; G3: `bash $T ank0 && git commit -F m.txt`, which is how research.md §7.3
writes every run, a direct Unity call, `dotnet test`. The G3 case that cost work (8 of 8 for 7 of 8) was a message written
before the result existed; a separate command does not stop that. One command to name: `git add ./`.
**Refused, though harmless and everyday here:** (a) `git commit -m "Run with -ownAsPlayed 1, read to the end"`: refused G1,
because `-[A-Za-z]*a` also matches a flag word inside the quoted message. This project's flags (`-ownAsPlayed`, `-trainFreshFalls`)
are routinely named in commit messages. (b) `git checkout claude/wip-round -- Assets/_WonderGather/Scripts`: the project's own
documented way to bring work into the main project (memory unity-validation-runs) is refused G2 (override needed each time).
(c) `git clean -n`, `git clean --dry-run`, `git stash push --dry-run`: dry runs refused. (d) a commit message or file name
that merely mentions `unity_tests.sh` or `git add -A` (G3, G1). The author documented (d); (a) and (b) are not documented.
**Other:** the hook lives in `.claude/settings.local.json`, which `.gitignore` excludes (line 29): it is on this machine only,
and the worktree and sub-agents may not load it (the report says so). `review.md` still says "treat it as not on" while the
hook is wired and was shown refusing; one of the two should change. The Handoff and AGENTS do not mention that the guard exists,
so an agent that meets "Guard G1" must read the message to learn it. Mend: tighten the `-a` match to the flag words outside quotes
(or accept only `-a` and clusters that start the argument list), allow `checkout <ref> -- <path>` with a clean-path check or name
it in the message, add `./`, `:/`, `*`, clustered `add` flags, and leave the rest, since it is a seat belt.

## Part 4. Claimed more strongly than the evidence allows

1. "Rules that are a tool or a switch held; sentences came back about half the time." The audit has no tally of tool rules
   against sentence rules. Its own row 14 is a tool rule (`-ownAsPlayed`) that "came back"; rows with "no recorded return"
   are as common among sentences. Keyword search misses repeats in either group (review §10 admits the method). Say
   "an impression".
2. Row 7: AGENTS.md "said review is usually unnecessary for gait tuning". It did not (PROJECT_CULTURE did).
3. "Each rule answers a recorded failure" is true for 1 to 5 and 12, and false for sayings 14 (audit: "memory aids only").
4. The guard stops "a commit chained behind a test": only for two named scripts, only in one spelling (Part 3).
5. §1: "this project's own record shows the need for each" of four things; the cold-entry test is one agent, one run
   (confidence "about 70%" is the agent's own word).
6. §8 is stale and understates: the guard and handoff reports exist, the cold-entry test after was written. §8 and the
   status line still say not yet checked.
7. §7's drop conditions are not measurable by anyone but the author ("a month with no report using these words", "more than twice
   a week" requires someone to count refusals nobody logs).
8. The study cited for keeping the file short (+20% cost, no general gain, coding tasks, Python, 2025 models) is used for and
   against the same change: for the length limit, but the change adds 13 lines of general-purpose rules.

**No finding** on: the 33-row audit's counts (they match its own table); the claim that nothing of the friend's guide was copied
(not checked, I did not open it); AGENTS.md remaining short.
