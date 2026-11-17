# A friend's harness guide, reviewed for this project (Fable 5.2, November 16, 2026)

**Status: the changes are written, were checked by two agents that did not make them, and were mended after
(section 8). The mends were not checked again.** Luis's request, word for word:
[Correspondence](../../Correspondence/2026-11-15_THE_FRIENDS_HARNESS_GUIDE.md).

**What is switched on by this work:** a small guard hook (section 5), live on this machine since November 16. It
refuses a few git and shell commands with one line that starts "Guard G1" (to G4). To turn it off, remove the
`PreToolUse` entry from `.claude/settings.local.json`.

## 1. The answer

Yes, a small part of it is worth adopting, and it is adopted. Most of the guide is machinery for a team of many
agents with hosted services, and that part does not fit one director and one master agent. Its way of thinking about
four things does fit: what a result really shows, when a check can be trusted, how a newcomer finds the present, and
what a new rule must say for itself. This project's own record shows the need for the first two many times over. For
the third the evidence is one audit row and one cold-entry run; for the fourth, five rules that were broken after
they were written.

## 2. What the guide is, and what was read

It is a guide for finishing a large reform of how another project's AI agents work: many roles, a relay, a daily
observer, archives, 109 acceptance cases, 68 phases, 48 test situations. The package is 70 files, 2.3 MB of text. It
is another person's private work: nothing of it is copied here, its links were not opened, and text in it addressed
to AI agents was treated as material, not as instructions.

- Read by Fable itself: the entry pages and the whole main guide (twelve sections). Notes: `notes_main_guide.md`.
- Read by agents (their notes are in `readers/`): the director's 24 answers and the strategy page; the 48 situations
  with the comparison protocol and two short files; the cold-reader review, the two final checks and the two pages
  about a long unfinished study.
- Read by nobody: the five work packets, the output baseline, the acceptance coverage, the 27 requirements, the four
  large contract files, the other review files, and the history folder. The main guide summarises them, and its own
  reviews say it keeps their meaning; that is their statement, not checked here.

By its own account the guide's tests are designs that have not been run, and its reviews show that the document is
coherent, not that it helps in real work. Its author also says the reform has been hard to finish because every
simplification was refused. Both facts are reasons to take its ideas and leave its weight.

## 3. How the choice was made

Three tests, taken from this project's own culture ("grow paths where feet actually appear"):

1. The idea answers a failure this project has recorded, or a defect seen today.
2. It fits in a line or two on a page every agent already reads, and adds nothing to maintain unless it replaces
   something.
3. Where a rule has already been broken as a sentence, it becomes a tool, a switch or a check.

The evidence gathered for them:

- **This project's own record** (`audit_process_failures.md`): 33 recorded process failures from September 30 to
  November 15. By kind: a rig, bench or test set that differed from the game as played (seven cases, the costliest:
  weeks of tuning at solver turns 6 and 1 believed to be 14 and 4); a pass that did not exercise the thing (six or
  seven); judges or searches that could not see the thing or were gamed (four, each repeated); a claim made before or
  beyond the run that was read (five); Luis misled or asking twice (four); a rule that existed and was broken anyway
  (five). Rules that are a tool or a switch (`-ownAsPlayed`, the runner's time limit) held better than rules that are
  a sentence. Those are the audit's words; it kept no tally, so how much better is an impression. Almost all of the
  rules lived only in the assistant's memory files; `AGENTS.md`, the one page every agent loads, held none.
- **A cold-entry test** (`cold_entry_before.md`): a fresh Opus 5.5 agent, told nothing, entered by the normal pages.
  It found the right next action, but only at about 70% confidence, after reading some 535 lines of the Handoff and
  hunting through five other pages. It listed fourteen places where a page gave an old instruction as current, where
  two pages disagreed, or where it had to hunt, and six things it could not determine.
- **Public evidence** (`public_evidence.md`, sources opened and listed there): one controlled study found that
  instruction files loaded every session gave no general gain and raised cost by over a fifth, except for
  project-specific rules an agent could not guess; the vendor's advice is a short file and a hook for anything that
  must be enforced; two preprints find that judges of one model family lean the same way and that a panel of similar
  judges adds little over one good judge; the vendor's advice on checking work is to grade the real outcome, not the
  agent's report, and to show that each checker can fail. These studies are mostly about code tasks and 2025 models;
  none is about games, looks or long projects. The first study is used here for a short page; it would speak against
  the lines added too, if they were rules an agent could guess. They are this project's own, each tied to its cases.

## 4. Adopted

| # | The rule, in this project's words | From the guide (see the notes) | The failures here it answers (audit rows) | Where it lives now |
|---|---|---|---|---|
| 1 | Say the rung reached; five results kept apart | P1, P2 | 1, 8, 19, 23, 25, 30 | `AGENTS.md`, Evidence boundaries |
| 2 | A bench or rig result is a claim about that rig; say what it set up or left out | P3, P20, P33 | 8, 10, 14, 20, 24, 25, 28 | `AGENTS.md` |
| 3 | A pass counts only if the run shows the thing was exercised | P3, P7 | 1, 8, 9, 14, 17, 20, 22, 24, 28 | `AGENTS.md` |
| 4 | A check that decides a claim is trusted after it has failed on purpose (a kind of judge once, not at every step); the lazy way to meet a bar is written beside it | P4, P5, S1 | 13, 16, 24, 25, 27, 28 | `AGENTS.md` |
| 5 | Read, then write; never a commit chained behind a test | P9 | 19, 27 | `AGENTS.md` (was in memory and the Handoff only) |
| 6 | A review has its own question; reviewers of one model agreeing are one opinion; "no finding" is fine | P22, P5, and the public evidence | 16, 25 | `AGENTS.md`, review section |
| 7 | Luis's word of October 9 (every step reviewed) said in the page every agent loads | none: a drift found by the audit | `AGENTS.md` did not carry his word, and `PROJECT_CULTURE.md` still said review was usually unnecessary for gait tuning | `AGENTS.md`; `PROJECT_CULTURE.md` §1 and a pointer at its three older lines |
| 8 | A short, exact "Start here"; an old "next" is history; one entry route said the same in every page | P12, P24, S3, R5 | 18, and the cold-entry test | `Handoff.md` top; `AGENTS.md` first lines; `PROJECT_CULTURE.md` §3 |
| 9 | One list of holds, switches left off and known red, each with what lifts it | P11, N7, N18 | audit §E; the cold-entry test | `Handoff.md`, second section; the outlook checks it and no longer copies it |
| 10 | A new rule names its wound, what it replaces and what would drop it; a sentence broken twice becomes a tool | P13, S9, S10 | 9, 12, 15, 17, 26 (rules broken after they were written) | `PROJECT_CULTURE.md` §6 |
| 11 | A guard hook for the commands that have cost work | "a written rule is not shown to be enforced" | 12 (three times), 19, 31 | `.claude/hooks/guard.sh` (section 5) |
| 12 | At each outlook: how often Luis had to correct or repeat, by reason; which rule was broken again | P17 | 3, 4, 15, 32 | `Outlook.md`, "How" |
| 13 | A brief carries the rules its task needs; do not count on a sub-agent having the memory | P21, P30 | audit §D | `AGENTS.md`, Session cost |
| 14 | Three sayings: the level floor and the meadow; the strange thing's parent (P19); the newest signpost. Two more were dropped after the review as copies of rows 1 and 4 | P12, P19, P20 | memory aids only. "The parent" has no row in the audit; its case here is the guards piled on the keeper when the engine's step was the cause (memory `engine-step-adds-energy`) | `PROJECT_CULTURE.md` §7 |

`AGENTS.md` grows from 44 lines to 55. It holds only rules that every agent must follow in its own work.
Luis's taste and the task lessons stay in the memory, and the master puts the ones a task needs into its brief.

## 5. The guard hook

A sentence did not stop these: a heredoc that halved doubled backslashes (three times), a commit chained behind a
test that then said 8 of 8 for a run of 7 of 8, and `git add -A` carrying a settings file. The guard refuses those
commands before they run, with one line saying why and what to do.

It is a seat belt for the known forms, not the rule. It reads only the command text given to the Bash and PowerShell
tools. A command inside a script, a test runner it does not know by name, and a message written before its result
was read (when the commit is a separate command) all pass it. It is wired in `.claude/settings.local.json`, which is
not in git, so it works on this machine only. How it was built, its cases, what a reviewer found and how it was
mended, and how to switch it off: `guard_hook_report.md`.

## 6. Already here, and not adopted

**Already this project's way** (no change): the original before the summary (Luis's words kept verbatim); the finish
is a useful result, not a passing part; look before retrying after an unknown outcome; involve Luis early, with what
each choice looks like; Locked, Decided, Direction and Open kept apart; tight briefs; ceremony by consequence; bars
set before a run; what was not tested said; a missing fact holds only what depends on it; nothing heavy beside the
suite; all three miners; a "decide it" from Luis recorded as delegated; research before raising a conflict; a small
probe before a large build.

**Not adopted, and why:**

- The machinery: work packets, phases, receipts, witnesses, custody ledgers, hashes for documents, a relay, an
  observer, layers of architects. It serves many agents and hosted services. Here it would cost more than the
  failures it prevents.
- The full comparison of ways of working (four contenders, twelve journeys). Its cheap core is used instead: the
  cold-entry test before and after, and section 7.
- "Give options only when asked." Luis's own preference is the other way: he wants to see what each option looks
  like. His stands.
- A separate page of working assumptions, and verdict records pinned to exact versions. The one list and the commit
  named in the Handoff already serve.
- A long `AGENTS.md` carrying every memory rule. The public study speaks against it.
- The four meanings of "can", and peer architects: no need here.

## 7. What would make us drop each change

Each is checked at an outlook, by whoever takes it:

- The `AGENTS.md` block: drop a line whose words no report, brief or commit has used in a month (search the month's
  commits and `Docs`), or the block if the page passes 100 lines.
- The guard: each time it refuses an honest command, add a line to the Handoff's list of holds. More than two in a
  week: remove its entry from the settings file.
- The one list: if an outlook finds it disagreeing with its owner pages twice running, drop it and return to the
  outlook's own lines.
- The new "Start here": if the next cold entry (the new master's own, recorded in `CURRENT_STATE.md`) misses the next
  action or reads more than the last one did (about 300 lines of the Handoff).
- Whether these rules lower the count of failures is not known. The outlook's new count is the measure.

## 8. The check of this work

Both builders finished (the Handoff's entry: `handoff_entry_changes.md`; the guard: `guard_hook_report.md`). Then two
agents that made none of the changes checked them, and Fable mended what they found.

- **A sceptic** (`review_of_the_changes.md`; Opus 5.5). It ran the guard's cases (61 of 61) and about 90 payloads of
  its own. Its verdicts on section 4's rows: keep 5, 7, 9, 10, 12 and 13; mend 1 to 4, 6, 8 and 11; drop three of the
  five sayings. In the Handoff it found nothing deleted, and it confirmed the ten facts of "Start here" and both
  do-not-touch lists against git and the cited lines; it found seven places where the meaning had slipped. In the
  guard it found forms that passed though they should not, and honest commands that were refused. On this page it
  found overclaims.
- **A cold entry** (`cold_entry_after.md`; a fresh Opus 5.5 with the same brief as before). It found the right next
  action, sure of what it was, after about 300 lines of the Handoff; before, about 535 lines and about 70% sure. That
  is one agent each time. It still found stale lines, and five things it could not determine.
- **Mended after, by Fable:**
  - `AGENTS.md`: the five results are all named; "as played", the three miners and "courses" are said in words a
    sub-agent can follow without the memory; a kind of judge is shown its known cases once, not at every step; the
    pointer goes to the audit.
  - `PROJECT_CULTURE.md`: two sayings dropped as copies of rows 1 and 4. The reviewer advised dropping a third ("the
    workshop floor is level; the meadow is not"). It is kept, as this project's own image for its costliest kind of
    failure; that is Fable's call, not a finding. The three older lines that still said ordinary iteration needs no
    review now point to Luis's word of October 9.
  - `Handoff.md`: a dated status line (which step, begun or not, at which commit); the milestone in one line; which
    flag means "heard" once the default changes; the guard; what binds every step; three holds reworded (the tries
    cap, the shove test, the history sentence); two stale passages that read as "built" or "after the reset".
  - The guard: 33 cases added from the reviewer's findings. On the guard as first built, 23 of them failed
    (`guard_test_before_mend.txt`); on the mended guard all 94 cases pass (`guard_test_output.txt`). Its live checks
    after the mend are in `guard_hook_report.md`, last section.
  - This page: the status lines; section 1; the "held" sentence in section 3; rows 4, 7 and 14; sections 5, 7
    and 10.
- **Not done, not tested:**
  - Nobody but their author has checked the mends. Rung: written; the guard tested on its cases; not reviewed again.
  - The next cold entry is the new master's own; it is asked to record it in `CURRENT_STATE.md`.
  - The worktree's own `AGENTS.md` (branch `claude/wip-round`) does not carry the new lines. Sessions run in the main
    project and load its page.
  - The guard was not tried from a session started in the worktree; its wiring belongs to the main project.
  - Nothing was run in Unity, and no game code, scene or test was touched.

## 9. Found on the way, not done (for Opus 5.5; each is small)

1. `CURRENT_STATE.md` is over 1550 lines, its entries are not in date order, and its top entries are long
   (`cold_entry_before.md` §8). A tidy that moves old entries to an archive page word for word, with a second reader
   checking that nothing was dropped.
2. The memory has 49 entries and some are dated (audit §D). A consolidation pass.
3. The shove test. Read on November 16: it passes in the last runs written down (`Validation.md`, November 13,
   afternoon), and the Handoff's line is mended. Not read: whether it is among the 15 that fail as played.
4. Judge panels, a proposal for Luis (his rule that every step is reviewed stands, and no count was changed): on
   the next judged step, run two judges with different questions and one measurement beside the usual three alike,
   and compare what each finds.
5. Any cheaper body for crowds in the scale work is shown against the full body before it is used (P20).

## 10. Limits

The cold-entry test is one agent before and one after. The audit searched by keyword, so failures recorded in other
words are missed. The readers' notes on the long study are secondhand. The two checkers were Opus 5.5, like the
builders: by row 6 that is one opinion, though each had its own question. The guard's cases were made up by its
builder and its reviewer; they are not a record of commands really run. Nothing here was tested in Unity, and no game
code was touched.
