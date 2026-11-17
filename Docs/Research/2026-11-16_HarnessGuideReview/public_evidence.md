# Public evidence for four questions (checked 2026-11-16)

Scope: independent of the friend's guide. Every source below was opened, unless marked "not opened". Evidence strength: controlled study, vendor advice, or opinion. "Implication" means for one director, one master agent, short-lived sub-agents.

## Q1. Instruction files loaded every session

### 1.1 Evaluating AGENTS.md: Are Repository-Level Context Files Helpful for Coding Agents?
- Publisher: arXiv 2602.11988 (ICLR 2026 listing). Gloaguen, Muendler-Sasahara, Mueller, Raychev, Vechev. First posted 12 Feb 2026 (page shows later revisions).
- URL: https://arxiv.org/abs/2602.11988
- Finding: Context files did not generally raise task success, and raised cost by over 20 percent. This held for machine-written and human-written files. Agents obey the instructions, but repository overviews did not help. Files are useful mainly for non-standard practices. The authors say to test any change before keeping it.
- Search summaries (not the abstract page) add: success moved by about -0.5 to -2 percent on average; human-written files helped slightly (about 4 percent on their new benchmark). Tested on Python, 138 issues, four models. I did not open the full text for these numbers.
- Strength: controlled study, one language, preprint/conference.
- Implication: A long router file costs money every session and may not help. Keep only what the agent cannot discover alone (project-specific rules, forbidden actions). Do not put architecture tours in the always-loaded file. Test a change to the file as a change.

### 1.2 How Claude remembers your project (Claude Code memory docs)
- Publisher: Anthropic. Undated live docs.
- URL: https://code.claude.com/docs/en/memory
- Finding: Target under 200 lines per CLAUDE.md. Longer files use more context and reduce adherence. Specific, concise instructions are followed more reliably. Imports organise a file but do not cut its context cost. Path-scoped rules load only when matching files are touched. The text is context, not enforcement: a hook is the way to block an action. Auto memory loads only its first 200 lines or 25 KB.
- Strength: vendor advice, no data shown.
- Implication: Our MEMORY.md index (about 45 long lines) is near the point where later entries risk not loading. Anything that must never happen belongs in a hook, not a sentence.

### 1.3 Effective context engineering for AI agents
- Publisher: Anthropic Engineering. 29 Sep 2025.
- URL: https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents
- Finding: Aim for "the right altitude": not brittle if-else rules, not vague. Start minimal on the best model, then add instructions only to fix failures you saw. Minimal does not mean short.
- Strength: vendor advice.
- Implication: Matches the project's own rule (add process only after a real failure). Supports pruning rules that have never prevented a failure.

## Q2. AI judges and reviewers

### 2.1 Play Favorites: A Statistical Method to Measure Self-Bias in LLM-as-a-Judge
- Publisher: arXiv 2508.06709, Spiliopoulou et al. 8 Aug 2025.
- URL: https://arxiv.org/abs/2508.06709
- Finding: Nine judges, over 5,000 prompt-answer pairs with expert human labels. Some judges (GPT-4o and Claude 3.5 Sonnet named) score their own output higher, and favour other models of their own family. Abstract gives no effect size.
- Strength: controlled study, older models.
- Implication: A Claude judge grading Claude work leans kind. Size unknown for current models.

### 2.2 Preference Leakage: A Contamination Problem in LLM-as-a-judge
- Publisher: arXiv 2502.01534, Li et al. Feb 2025, v3 Mar 2026, ICLR 2026.
- URL: https://arxiv.org/abs/2502.01534 (abstract opened; full PDF unreadable)
- Finding: A judge favours outputs from a model that is the same, descended from it, or in its family. The authors say this is harder to detect than earlier judge biases.
- Strength: controlled study, aimed at training data, so only partly our case.
- Implication: Same-family judging of same-family work has a known tilt that is hard to see by eye.

### 2.3 Nine Judges, Two Effective Votes: Correlated Errors Undermine LLM Evaluation Panels
- Publisher: arXiv 2605.29800, G. Kohli, single author. 28 May 2026. Not peer reviewed.
- URL: https://arxiv.org/abs/2605.29800 (abstract opened; PDF unreadable)
- Finding: Nine frontier models from seven families, three language-inference sets plus one preference set. They made the same mistakes on the same items, so the panel held about two independent votes. The best single judge matched or beat the whole panel. Smarter vote counting closed at most 11 percent of the gap.
- Strength: one controlled preprint, narrow tasks (language inference), mixed families (so same-model panels are likely worse, but not measured here).
- Implication: More same-model judges add little. A second judge helps only if it differs in family, information or method (a code check, a measurement, a human look). Unproven for visual or aesthetic judging.

### 2.4 Healthcare scoping review (only as a pointer)
- Publisher: arXiv 2604.25933. URL: https://arxiv.org/pdf/2604.25933. Not opened; seen only in search summary. It reportedly assumes a 0.6 error correlation for same-family judges as a modelling assumption, not a measurement. Do not rely on it.

### 2.5 How to check a judge
- Anthropic, Demystifying evals for AI agents (9 Jan 2026), URL in Q4: calibrate model graders against human experts; give the judge an "Unknown" option; one judge per dimension; each task needs a reference solution that passes, so the grader is shown able to pass. A 0 percent pass rate over many trials usually means a broken task.
- Anthropic, multi-agent research system (13 Jun 2025), URL in Q3: a single rubric call giving scores and pass/fail matched humans best; human testers still caught things automation missed (for example a bias toward weak sources).
- Strength: vendor advice with examples.
- Implication: Before trusting a judge, give it a known-good and a known-bad case and see it separate them. Keep Luis's eye as the final calibration.

## Q3. Long work across many context windows

### 3.1 Effective harnesses for long-running agents
- Publisher: Anthropic Engineering. 26 Nov 2025.
- URL: https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents
- Finding: Each session starts blank. What worked: a progress file plus git history; a feature list (JSON, so the model edits it less) where the agent may change only a "passes" field; one feature per session; commit with clear messages; a script that starts the app; start each session by reading these and running a basic test. Common failures: declaring victory too early, marking features done after unit or curl checks without testing end to end. Testing like a user (browser automation) improved results. Authors call multi-window consistency an open problem.
- Strength: vendor advice from one build, no controlled comparison.
- Implication: Supports Docs/Handoff.md and a "claimed done" list. The strongest idea is separating "built" from "verified by running it". Our Unity runs and Luis's playtests play that role.

### 3.2 Effective context engineering (same page as 1.3)
- Finding: Compaction summarises the window and restarts; Claude Code keeps decisions, open bugs and details and drops stale tool output. Tune the compaction prompt for recall first, then trim. Clearing old tool results is a light, safe form. Notes written outside the window (a NOTES.md) suit work with clear milestones. Compaction suits long back-and-forth.
- Strength: vendor advice.
- Implication: Files on disk plus compaction (already our practice) fits. Check that a compaction summary keeps the open questions, not only the results.

### 3.3 How we built our multi-agent research system
- Publisher: Anthropic Engineering. 13 Jun 2025.
- URL: https://www.anthropic.com/engineering/multi-agent-research-system
- Finding: The lead agent saves its plan to external memory, since context can be cut past 200k tokens. Fresh sub-agents with clean context get careful handoffs. A sub-agent brief needs an objective, output format, tool guidance and boundaries (vague briefs failed). Resume from a checkpoint, do not restart. Multi-agent runs cost far more tokens (a secondary summary says about 15 times chat; I did not confirm the figure in the post). Start evals with about 20 cases.
- Strength: vendor advice with an internal result (lead plus sub-agents beat a single agent by 90 percent on their research eval; internal, not independent).
- Implication: Tight briefs to cheap sub-agents fit the Pro-plan budget. Cost grows with agent count.

## Q4. Evaluating agents

### 4.1 Demystifying evals for AI agents
- Publisher: Anthropic Engineering. 9 Jan 2026.
- URL: https://www.anthropic.com/engineering/demystifying-evals-for-ai-agents
- Finding: Grade the outcome (the real final state), not what the agent says. Example: the chat says "booked", the database holds no booking. Grade what was produced, not the exact path. Read many transcripts to confirm the graders work. A grader bug made one model score 42 percent when it deserved 95 percent. Prefer code checks; use model graders where needed and humans to validate. Test both cases where a behaviour should and should not occur, or you optimise one way.
- Strength: vendor advice with examples.
- Implication: A sub-agent's report is a claim; the build, test log or screenshot is the outcome. A check that has never failed has not been shown to work: run it once on a known-bad input.

## Short list

What the evidence supports
- Short always-loaded instruction files with only non-obvious, project-specific rules (one controlled study plus vendor advice). Overviews and long detail cost money without gain.
- Same-model or same-family judges lean toward their own side, and a panel of similar judges adds little beyond one good one (two preprints, partly narrow).
- Written handoff state (progress file, git, feature list) plus clean-context sub-agents with tight briefs (vendor practice).
- Check the outcome in the real world, not the agent's account; show each checker can fail and can pass.

What it does not support
- That longer or more detailed instruction files improve results. No study found says so.
- That extra same-model judges are worth their cost on correctness questions.
- Any claim that these results hold for Unity, 3D or aesthetic judging, long multi-month projects, or current models. Evidence is mostly Python code tasks, language inference, and 2025 models.

What is unknown
- The best length for a file loaded alongside many linked docs; the 200-line figure is vendor advice, not measured.
- The size of self-preference for current Claude models grading its own sub-agents.
- Whether mixing judge types (code check, measurement, picture review, human) beats same-model panels on game-feel work. No public test found.
- How much a compaction summary loses over many cycles.

Not opened: arXiv 2604.25933; full PDFs of 2.2 and 2.3 (abstracts only). Search summaries other than those listed were not used as evidence.
