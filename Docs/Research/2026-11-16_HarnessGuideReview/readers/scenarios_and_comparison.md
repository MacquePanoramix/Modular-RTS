# Reader note: the 48 situations, the comparison protocol, the two short files (November 16, 2026)

Source: files 24 to 27 of the package's reference folder. All 48 situations, the whole protocol, the short appendix and the six reconciliation cases were read. None of these tests has ever been run in that project: they are designs, not results.
Counts of "can it happen here" are for one director, one master agent, short-lived sub-agents, a Unity game.

## 1. The 48 situations, by kind of failure

1. **Fresh start finds the true present** (5). A new agent gets only the normal entry pages, or a rival alias, or a retired carrier exists.
   Good: it finds current authority, holds and evidence without digging through chat, and old "next" lines do not steer it. Also covers a project winding down its temporary process and still working.
   Here: YES. A new session after compaction reads CURRENT_STATE and must not follow a stale Handoff line.
2. **Evidence that is missing, doubled, malformed or the wrong thing** (5). Stage summaries with gaps, archives with bad entries, a coverage list with one item dropped, a stale pass swapped in, a file whose hash matches but whose content is an error page.
   Good: only complete, exact, matching, successful evidence counts; "same bytes" never proves "the right source".
   Here: YES. A test summary that says passed while one test never ran; a copied asset that is the wrong file.
3. **The checker itself must be shown to work** (3). Real known-bad changes fed in; a cleanup that left the workspace dirty; a deliberately broken parser.
   Good: the faults are caught; an unfinished campaign cannot pass; the check does not trust the thing it checks.
   Here: YES. A detector for a bad movement that never fired on a known-bad clip.
4. **Evidence goes stale when things move** (3). The base moved, the definition of the check changed, a report-only edit versus a real change.
   Good: harmless movement keeps earlier evidence; material change voids exactly the claims it touches; a mismatch is named as such.
   Here: YES. A test result from before a tuning change quoted after it.
5. **Interrupted or doubled actions with unknown outcome** (5). Stop before send, after accept, before the receipt; cancel mid-flight; a half-published step.
   Good: one intent kept; look at what really happened; never replay blindly; never invent a "no"; unrelated work goes on.
   Here: SOMETIMES. A Unity run killed mid-way; a commit or build whose result was not read.
6. **Authority and ownership** (4). Scope or ceiling raised, parent-only power claimed, a grant revoked, an ancestor narrowed while a leaf stays; two workers claim overlapping work; a late old return tries to overwrite a successor.
   Good: invalid grants fail before any effect; a timeout alone never frees a claim; the full chain is checked now.
   Here: SOMETIMES. Two sub-agents editing one scene; a late sub-agent report after the plan changed. Grant chains: NO.
7. **Retiring old things safely** (3). Delete a branch or resource with a live dependent, a dirty leftover, a half-failed delete, an obsolete group that points at itself.
   Good: unsafe retirement stops; eligible leftovers go; unfinished cleanup has an owner and a retry condition.
   Here: YES. Old worktrees, branches, recovery folders, scratch builds.
8. **Rule changes leave old readers behind** (4). An interface changed but an old caller, prompt or hold line still looks current; a summary of a rule drops its exception; a repeated annoyance with a sibling in another route.
   Good: readers are found; cause fixed everywhere; workaround retired; compressed rules keep every qualifier or flag the loss.
   Here: YES. A rule edited in AGENTS.md but not in memory notes or docs; a fix on one miner and not the others.
9. **Research and exploration handling** (4). Relevant idea in new words versus an irrelevant one; a decision answer versus open-ended curiosity; disagreeing sources with shared roots; a big conceptual challenge versus a small reversible change.
   Good: keep sources, context and doubt; shared roots count once; no quota; full ceremony only when the change is large.
   Here: YES. Research sub-agents quoting the same source; a design idea silently becoming "decided".
10. **Holds and gaps stay honest** (3). Readiness becomes true while holds remain; an incomplete list of cases; a legacy list needing every item to have an owner.
    Good: ready does not lift a hold; a gap blocks its own claim and escalates only if it is a real choice.
    Here: YES. "Prototype ready" versus Luis's acceptance; Open items kept Open.
11. **Away-time, relay and observer claims** (4). Phone-started task, courier, unattended monitoring, offline worker, a replaced session.
    Good: only demonstrated coverage is advertised; unavailable and missed are different; a missing capability stops the dependent work instead of moving it elsewhere.
    Here: NO for most (no relay). SOMETIMES: scheduled or remote runs while Luis is away.
12. **Platform mechanics** (2). Launcher script runs on one OS but not another; edits that silently change file mode or type.
    Good: the supported entry works on each platform; unsupported edits are refused, not rewritten.
    Here: SOMETIMES. Windows paths, line endings and backslashes in scripts; Unity meta files.
13. **Switching to a new way, fallback, compaction** (3). Before/after comparison on matched tasks; going back to old code must not revive expired permissions; shrinking old records must keep the live ones.
    Good: justify each new cost by mechanism; fallback keeps current holds; compaction keeps active state.
    Here: YES. Revert a commit; compact the conversation or the handoff file.

Total 5+5+3+3+5+4+3+4+4+3+4+2+3 = 48.

## 2. The comparison protocol

**Set-up.** First measure the old way on its real record: successful ordinary paths as well as failures. Where the old way had no guarantee, write that down; do not count it as free safety. Then freeze the tasks, the faults to inject, the claims and the scoring. Run each contender on the same work. Four contenders: repair the old way, the big new design, a slimmer new design, and a design with more writers. Twelve "journeys" stand for typical life: a small fix, an exploration, a big task with review, a hard choice, a stop mid-action, a lost helper, two parallel lines, retiring old things, no courier, cold recovery, a fix after the process is retired, a yearly repeat.
**Measured per trial.** Useful result; director interventions by reason; manual handoffs; rules edited; stale copies repaired; active time; queue time; recovery time; leftover mess; evidence for each claim; fault caught; false blocks; limits. Failed, held and unknown trials are kept.
**Anti-gaming.** Six lazy strategies must each fail a named test: always stop (fails usefulness), accept any summary (fails evidence), delete everything (fails custody), keep everything (fails closure), copy a rule everywhere (fails one-owner), ask about everything (fails autonomy). Also: scoring changes after the freeze need a written successor and a rerun; a cold reader gets only the normal entry pages; reviewers state what they saw; the worker's own confidence is no evidence; a cheaper rival gets a fair, real design, and its limits are written down.
**Choosing.** Hard conditions first (safety checks pass; useful work still happens; no new routine burden on the director). Reliability beats speed. At equal quality, fewer changing facts and easier restore win. Reversal triggers are written in advance. A failed test does not switch the design on its own: it blocks the capability and brings prepared options to the director.
**Cheap for a tiny project.** The six lazy-strategy checks as a thought test for any new rule. Freeze the claim and the bar before looking. Record intervention reasons. Keep failed and unknown runs. A cold-entry read. Write the reversal triggers when adding a rule. A short list of four or five typical journeys.
**Too heavy.** Four contenders, twelve journeys, sixteen record fields, three formal gates, qualification phases, host and credential fencing, provider read-back, shared journals.

The short appendix adds: a bigger specification or more model checks does not make something reliable in the real setting; the primary failures are unauthorised or doubled actions, misattributed evidence, lost recovery, missed restrictions and hidden manual rescue; any of these voids a reliability claim whatever the speed; reviewers challenge from three directions (ownership and freshness, execution and recovery, migration and preservation). Reports from such reviews are paper challenge, not live proof.

## 3. Transferable ideas not in P1 to P36

- **S1 Name the lazy wrong answer for each test.** Each reconciliation case lists a "negative": the shortcut that would pass it falsely (all green because names and hashes agree; a restored old version reviving expired permission). Where: file 27, every case; echoed by file 24's six controls. Guards against: a test that passes for the wrong reason. (Related to P4 but per test, not per detector.)
- **S2 Unrun designs are no evidence.** Every case carries "not run"; a design's own rehearsal and the author's help are not credited as acceptance. Where: file 25 header, file 27 case on later corrections. Guards against: a written test list read as a passed test; author-assisted handover counted as cold.
- **S3 Compress a rule without losing its exceptions.** When a rule is shortened, each qualifier, exception, stop condition and open lock is kept, deliberately replaced under authority, or flagged as a loss; untouched history stays untouched. Where: file 25, the rule-compression situation. Guards against: CURRENT_STATE or memory summaries quietly dropping a "but not when".
- **S4 Equal bytes do not prove the right source.** A matching hash on an error page or a stand-in. Also: "original lost, rebuilt from elsewhere" is a different statement from "original". Where: file 25, ingest situation. Guards against: a copied asset or document that is valid but wrong.
- **S5 Fix the cause, then look at the sibling.** Repair reaches every affected consumer including the same fault in a second route, earns a regression check, and retires the workaround. Where: file 25, recurring-friction situation. Guards against: the same complaint coming back from the twin case (other body, other miner).
- **S6 Late or old returns cannot overwrite the present.** A duplicate or old-generation report may be reused as evidence on purpose but never closes or replaces current ownership. Where: file 25, transfer situation. Guards against: a sub-agent finishing after a decision changed.
- **S7 Going back does not go back in permission.** Restoring older code or a fallback keeps current holds, revocations and decisions. Where: file 25 (fallback) and file 27 (rollback). Guards against: a git revert resurrecting a design choice Luis has since closed.
- **S8 Fair rival, with its limits written.** The cheaper alternative gets a real implementable design and its deliberate limits, never a straw man; decide invariant by invariant, and "unknown" is neither pass nor fail. Where: file 24, bounded rival and gate text. Guards against: a comparison decided by who described it better.
- **S9 Reversal triggers written at adoption, and a failed test blocks rather than switches.** Where: file 24, reversal contract and the appendix. Guards against: drift in design after a failure, or sunk-cost holding on.
- **S10 Justify overhead per mechanism.** Each new cost is argued by its own failure, not hidden in an aggregate score; a capability that is explicitly required may justify cost before an incident. Where: file 25, comparison situation. Guards against: process that "feels safer" and stays.
- **S11 Time alone never proves failure or frees a claim.** Where: file 25 (claims, lost acknowledgement), file 24 (lost executor). Guards against: killing or re-running something that is only slow. (Close to P15; the claim-freeing half is new.)
- **S12 Out-of-band changes lie outside a guarantee.** A protocol guarantee covers only the supported route; say so, and do not stretch it to hand edits. Where: file 25, retirement-seal situation. Guards against: claiming safety for things done outside the checked path (hand edits of scenes beside the authoring tools).
- **S13 An optional service left out is "not applicable", never "passed".** Where: file 25, observer situation. Guards against: counting no monitoring as healthy monitoring. (Close to P2.)
- **S14 Curiosity-led exploration is allowed, kept apart from decisions.** No quota, no automatic implementation; duplicate corroboration is not independent. Where: file 25, exploration situation. Guards against: forcing every research task to justify itself by use, or letting it turn into a build.

## 4. Not read or not parsed

- Everything was read; the large JSON files were pretty-printed to a private copy and read from it. In the 48 situations I read the stimulus and expected-behaviour text only, not the "successor mapping" fields (which point to other packages' cases I did not open).
- Nothing failed to parse.
- Their details of provider, host and credential fencing were skimmed, since they do not transfer.
