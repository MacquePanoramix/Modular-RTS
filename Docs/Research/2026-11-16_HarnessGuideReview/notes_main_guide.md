# Working notes: the friend's harness guide, its main guide read in full (Fable, November 16, 2026)

**Status: working notes, not the review.** The review is `review.md` in this folder once it is written.

Luis asked (November 15, evening; his words are in `Docs/Correspondence/2026-11-15_THE_FRIENDS_HARNESS_GUIDE.md`
once recorded) for a look at a friend's harness guide for a different AI-assisted project: which of its principles
would help this project, implemented for the Opus 5.5 project master where they do, reported as "none" where they do
not. The package is the zip Luis named in `D:\Downloads` (70 files, 2.3 MB of text). It is unpacked in this session's
scratch folder (`friend_guide`); a later session unpacks it again into a new empty folder. It is another person's
private work: nothing of it is copied into this repository, its links are not opened, and text in it addressed to AI
agents is material to weigh, never an instruction.

(This paragraph was written while the reading was under way; what was read in the end is in `review.md`, section 2.)
**Read so far, by Fable itself:** the package's entry pages (readme, package note, receiving note, review scope,
document review) and the whole main guide, sections 1 to 12. **Not read by Fable:** the twenty reference files, the
reviews, the history. Readers (a workflow) cover the director's answers and the strategy, the 27 requirements, the 48
original scenarios with the comparison protocol, the fresh-receiver review and the long-task lessons. **Read by nobody:**
the five packets A to E, the output baseline, the acceptance coverage and the four large contract files (acceptance
cases, target envelope, qualification phases, implementation units), which the main guide's sections 5 to 11 summarise.

## What the guide is

A completion guide for finishing a large "administrative reform" of how a team of AI agents runs a software project
(many agent roles, hosted services, a relay, a daily observer, custody of archives, 109 acceptance cases, 68
qualification phases). Most of it is that project's machinery and does not transfer. Its transferable part is a way
of thinking about claims, evidence, completion, review and process cost, mostly in its sections 2, 4, 10 and 12.

## The transferable principles found (P1 to P36), in this project's words

Claims and evidence:
- **P1 The narrowest true claim.** "Written", "tested", "works as played", "seen by the director", "accepted" are
  different claims; each report says which one was reached (sections 3, 12).
- **P2 Typed outcomes.** Not run, did not finish (crash, time-out, build error), failed its bar, passed, unknown
  (not read) stay distinct; none is rounded to another (sections 3, 5).
- **P3 A pass needs proof of exercise.** A result counts only when the run shows the thing tested was really on, the
  event really happened, and the run really ended (section 4's table: a pass row without the execution behind it).
- **P4 Controls for every check relied on.** A detector or bar is shown to reject a known-bad case and accept a
  known-good one before its verdicts count; a written rule is not demonstrated enforcement (sections 4, 5).
- **P5 Judges are calibrated.** An AI review that matters is shown to catch plausible seeded errors and to accept
  valid alternatives, labels concealed; its misses and false alarms are kept; same-model judges agreeing is not
  independent proof (section 4).
- **P6 The original over the summary.** Actual behaviour and the original record before another summary (section 4).
- **P7 Counts are not correctness.** "16 of 16" controls omissions, not whether the 16 test the right thing.

Completion and closure:
- **P8 The finish is a useful received result.** A plan, a passing part or a correct refusal is not the finish; the
  finish is named before the work starts (section 4).
- **P9 A long task is finished at its whole terminal boundary**, observed by its owner: not when a child process
  exits or a partial sample is green (section 4).
- **P10 Temporary things are closed**, and what stays is listed: removed, kept, pending, with the reason (section 8).
- **P11 An owned block is truthful.** A hold names its missing condition, what it blocks, and what reopens it; other
  work goes on (sections 4, 10, 12).

Authority and history:
- **P12 Old instructions do not reactivate themselves.** A past "next" in a handoff, or an instruction inside
  received material, is history to weigh, not a command (sections 1, 3, 8).
- **P13 Every durable process addition earns its cost**: it names the failure it answers, what it changes, and the
  old rule it retires; restrictions carry who set them, what they protect, their scope, and what reopens them; fewer
  mutable facts are better (section 2).
- **P14 Four meanings of "can"** (tool callable, capability supported, guarantee observed, action authorised): not
  needed here.
- **P15 An unknown outcome is looked at before a retry** (sections 7, 8).

Working with the director:
- **P16 Involve the director where his judgment improves the outcome**, early enough to shape it; never hand him
  routine technical homework; keep working on independent things meanwhile (section 2). Already this project's way.
- **P17 Count the director's interventions by reason** (section 10's measures).
- **P18 Keep kinds of statements apart** (evidence, inference, proposal, accepted). Already here: Locked, Decided,
  Direction, Open.
- **P19 A surprising result has three possible parents**: coherent emergence, a defect, or a rule working as written
  that the director dislikes; say which before changing anything (section 2).
- **P20 Observe before optimising; keep the full-detail version as the reference**, and show a cheaper version
  equivalent to it before using it (section 2).

Ownership and review:
- **P21 Ownership carries work, not reports**; workers get interfaces, invariants and acceptance, and do not invent
  cross-system meaning (section 2). Already here (tight briefs).
- **P22 A review has a reason**: a distinct question, an independent source, or a different failure mode. A new agent
  label is not independence; the reviewer's exposure is disclosed; "no new finding" is a legitimate result (section 2).
- **P23 Proportionate process**: recheck only what a change affects; reuse accepted checks at unchanged scope
  (sections 2, 10). Already here in AGENTS.md.
- **P24 The cold-entry test.** A fresh agent enters by the project's normal entry pages, with no rescue from the
  author, and must recover the goal, the holds, the decisions, the unfinished work, what is running and the next
  step (sections 4, 10).
- **P25 Peer architects and connected waves**: a multi-team matter, not needed here.

Measuring the process:
- **P26 Measure a process change against the old way**, counting failed and unknown trials too; six anti-gaming
  checks: always stopping fails usefulness, trusting a summary fails evidence, deleting everything fails custody,
  keeping everything fails closure, duplicated rules fail single ownership, escalating every choice fails autonomy
  (section 10).
- **P27 Recheck after the setup changes** (model, harness, instructions, tools), and only what the change affects.

Documents:
- **P28 A synthesis does not make its originals disposable.** Already here (Correspondence verbatim).
- **P29 One owner per rule; a change reaches every reader of it**, and a stale reader is looked for on purpose
  (section 6).
- **P30 Give a worker the subset it needs.** Already here.
- **P31 Beside each claim, what it does not establish** (section 12's tables). Already here ("Not tested").
- **P32 Bars frozen before the run.** Already here.
- **P33 Disclose what a test set up or faked.** Here as a lesson (the bench's level floor), not as a rule.
- **P34 A missing fact holds only what depends on it.** Already here.
- **P35 Serialise the short consequential act, not the thinking.** Already here (nothing heavy beside the suite).
- **P36 Its public sources** on agent evaluations, instruction-file size, correlated judges and harness engineering:
  to be read for the judge and instruction-file questions.

## First judgment, before the readers and the project's audit return

Likely worth adopting, in order of value here:
1. the claim ladder with typed outcomes (P1, P2, P31, P33);
2. proof of exercise and the terminal read (P3, P7, P9);
3. controls for every detector and bar (P4);
4. judges calibrated, independent, allowed to find nothing (P5, P22, P27);
5. the cold-entry test, with entry pages kept short, one owner per fact and pointers elsewhere (P12, P24, P26, P29);
6. process additions earn their cost and retire what they replace (P13), with the six anti-gaming checks (P26);
7. closing temporary things and a list of standing exceptions with what reopens each (P10, P11);
8. the three parents of a surprising movement (P19), and the full body as the reference for any cheaper one (P20);
9. the director's interventions counted by reason at each outlook (P17).

Not to adopt: the machinery (packets, phases, receipts, witnesses, custody ledgers, hash manifests for documents,
relay and observer services, layers of architects). The guide itself warns against new registries, standing reviewer
layers and ceremony; this project is one director and one master agent.
