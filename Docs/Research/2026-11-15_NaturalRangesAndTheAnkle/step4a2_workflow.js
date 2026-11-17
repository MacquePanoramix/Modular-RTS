export const meta = {
  name: 'ankle-step4a2',
  description: 'Step 4a2 of the ankle build: the forced checks run again on their final code, a known-bad for each bar that has none, check 3 decided or called a reading; then a physicist, a sceptic and a code reader',
  phases: [
    { title: 'Mend', detail: 'one builder in D:/Dev/WG: the four open items of the Handoff, each run and read' },
    { title: 'Review', model: 'sonnet', detail: 'physicist, sceptic and code reader, each with its own question' },
  ],
}

const MAIN = "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer"
const FOLDER = MAIN + "/Docs/Research/2026-11-15_NaturalRangesAndTheAnkle"

const RULES = [
  "Running Unity: one Unity at a time, with nothing heavy beside it. Never edit code or tests while a run is going. Read each run to its end before the next.",
  "  cd D:/Dev/WG && LIMIT=<seconds> bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> \"<testFilter>\" [args]",
  "It writes D:/Dev/WG/TestRuns/<name>.log and <name>_r.xml. A run stopped half-way can fail the next launch once with \"Scripts have compiler errors\"; if so, launch again. Read logs through grep, not whole. If a run hangs past its LIMIT, stop only this worktree's Unity (the one whose command line names D:/Dev/WG).",
  "",
  "Code and git rules:",
  "- Gameplay stays under Assets/_WonderGather. No UnityEditor in runtime code. Never delete or recreate a .meta file, so GUIDs are kept. Do not change any FBX import settings. OwnKeeper gets no new guards.",
  "- Commit in D:/Dev/WG by path only. Never stage everything, and never commit with -a.",
  "- Leave these out of every commit: ProjectSettings/EditorBuildSettings.asset, ProjectSettings/ProjectSettings.asset, ProjectSettings/PackageManagerSettings.asset, the untracked Assets/ML-Agents and Assets/ML-Agents.meta, and Assets/_WonderGather/Resources/RiseModels/ SmallRise.onnx, SmallRiseFullP.onnx, SmallRisePFull.onnx and SmallRisePaceP.onnx with their metas.",
  "- A guard hook refuses some git commands, even when a command's text only mentions them: staging everything, destructive resets and cleans, git checkout -- and git restore (copy a file back with cp instead), skipping git's hooks (--no-verify), a test run and a commit in one command, a heredoc with a doubled backslash, and a commit or push that carries the path of the user's profile folder. Since November 18, git's own hooks (pre-commit, commit-msg and pre-push, in the shared .git) refuse that too, in D:/Dev/WG as well; if one refuses, take the path out and commit again. Never write that path or the user's name into any file; where a script needs it, build it at run time.",
  "- So write commit messages and scripts with the Write tool, and commit with git commit -F <file>. End each message with the line: Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
  "- Push with git push origin claude/wip-round.",
  "",
  "The plan's usage is watched. Before each Unity run, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch \"select:mcp__ccd_session_mgmt__get_usage\"). If the 5-hour window passes 80%, or the week passes 95%, stop at the next point where the report is true, commit what compiles, and return.",
].join("\n")

const MEND = [
  "You mend and run checks for the ankle switches of a Unity 6000.6.0f1 character (URP, PhysX ArticulationBody, 50 physics steps a second). You build and run checks; you do not decide design.",
  "Work only in the validation worktree D:/Dev/WG (branch claude/wip-round, head b76f311; the checks last changed in 7c6edf0). The docs live in the main project, " + MAIN + "/Docs: read them there, write only your report file there, touch nothing else in that project, and commit nothing there.",
  "",
  "Read first:",
  "- in " + FOLDER + ": the report forced_checks.md, as the last mend left it, and the two reviews of that mend, review_forced2_physicist.md and review_forced2_sceptic.md (the earlier reviews only where these point to them);",
  "- the checks, D:/Dev/WG/Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs;",
  "- research.md in that folder, lines 536-541 (what each check must force) and section 4.2, where a check needs them;",
  "- " + MAIN + "/Docs/Handoff.md, the paragraph that begins \"Next: 4a\" (the four items below come from it).",
  "",
  "The switches: A is OwnRiser.AnkleLimit, B is OwnRiser.AnkleSpeed (with AnkleSpeedHeight), C is OwnBody.BootsFromOwnShin. All are off by default and stay so; change nothing on the off path.",
  "",
  "The four items:",
  "1. forced_good, forced_barfails and forced_bad2 ran on code older than the last change of 7c6edf0. Run all three again on this step's final code, after every edit below, and read each to its end.",
  "2. Give a known-bad to each bar that has none: BAR_AXES_READ (it reads 0.00 degrees on every hold: a quantity measured against itself, so it cannot fail), BAR_A_SLERP, BAR_FOLLOW, BAR_FORM, BAR_WRAP and BAR_C_PLAIN. A known-bad is a run the bar must fail, for the reason the bar claims to catch; the test asserts that it failed, and the known-good run passes. A bar measured against itself is rewritten to measure against something apart from it. Write beside each bar the lazy way to meet it. A bar that cannot be given a known-bad is called a reading, with the reason.",
  "3. Check 3 (C). The whirl is now the engine's own (50-80 rad/s read the step after the set), but the going down destroys the boot, so the whirl is not carried on through it. BAR_C (0.025 m) does not separate C off from C on: in down_c0e, with C off, Long whirled at 0.0141 m and Round at 0.0148 m passed where they must fail; once, with C on, Round whipped at 0.0398 m. Find what shows that C acted on a whirled boot: a measure that fails with C off and passes with C on, on each of Small, Long and Round, on at least three different courses per miner and setting (a course differs in the whirl's speed or axis or the moment of the going down; repeats of one course count once). Read the engine's behaviour back from the running joint; do not assume it. If no measure separates after a fair search, call check 3 a reading, say what was tried and what each attempt read, and leave BAR_C out of every verdict.",
  "4. The report names the log file of every line number it quotes (forced_good.log:2771, not log:2771).",
  "",
  "Follow AGENTS.md \"Claims and checks\", which you have: quote each switch as read back from the running body; show that the event was forced or counted; check that each run ended by itself, not at its LIMIT; write beside each bar the lazy way to meet it; say what the rig set up, pinned or left out.",
  "",
  "If a check finds a fault in A, B or C, mend it on that switch's own path only, and say so in the report's first lines with the commit. A mend to B means Small's arm B (ank1_small, run on e8bac96) must be run again: do not run it; say so.",
  "",
  RULES,
  "",
  "The report: update forced_checks.md early and as each item is run, in a new section at its top for this step, keeping what the earlier builds found and marking what changed and why. For each bar: what it forces, its known-bad and known-good runs by name, the lazy way to meet it, the values read back, quoted with their log file and line, whether each run ended by itself, and the rung reached. End with what was not tested.",
  "",
  "Return as your final text, in at most 250 words: the commit(s); for each of the four items, whether it was done (and how), answered (with the log file and line) or not done; any fault found in A, B or C, and whether arm B must be run again.",
].join("\n")

const SHOWN = [
  "Shown to you:",
  "- the report, " + FOLDER + "/forced_checks.md, its newest section first;",
  "- the reviews of the step before, review_forced2_physicist.md and review_forced2_sceptic.md in that folder;",
  "- the code in D:/Dev/WG: git -C D:/Dev/WG log --oneline b76f311..HEAD and the diff over the same range. The checks are in Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs; the switches are in Assets/_WonderGather/Scripts/Units/OwnRiser.cs and OwnBody.cs;",
  "- the run logs and result files in D:/Dev/WG/TestRuns named in the report (read them through grep, not whole);",
  "- research.md in that folder, section 4.2 and lines 536-541, for what each check was meant to force.",
  "Do not run Unity, do not edit any code or test file, and commit nothing. Write your review early to the file named below, and keep it current. Give each finding a file and line, and say how sure you are. Say what you were shown and what you did not read. \"No finding\" is a good result.",
].join("\n")

const PHYS = [
  "You review, as a physicist, checks built and run for the ankle switches of a Unity PhysX ArticulationBody character (50 steps a second). You did not build them.",
  "Your file: " + FOLDER + "/review_forced3_physicist.md.",
  SHOWN,
  "Your question: does each new known-bad fail for the physical reason its bar claims to catch, and not by some other effect of the rig? For check 3, does what the report now shows (a separating measure, or a reading) physically say whether C acted on a whirled boot, read back from the running joint through physics steps? Where is the engine's behaviour assumed rather than read back? Do not reproduce engine source.",
  "Return as your final text, in at most 200 words: your findings, most serious first.",
].join("\n")

const SCEP = [
  "You review, as a sceptic, checks built and run for the ankle switches of a Unity character. You did not build them.",
  "Your file: " + FOLDER + "/review_forced3_sceptic.md.",
  SHOWN,
  "Your question: is each claim in the report what its logs say?",
  "- Does each bar decide its test: a failing bar marks its run Failed in the result file, and a known-bad run passes only because it asserts that its bar failed? The result files' asserts count is no evidence either way: this Unity writes asserts=\"0\" even for a test that failed on an Assert.That.",
  "- Did each run end by itself?",
  "- Is each switch quoted as read back from the running body?",
  "- Does every number in the report match the log file and line it names? Quote both.",
  "- Is the lazy way to meet each bar written, and avoided?",
  "- Were forced_good, forced_barfails and forced_bad2 run on the step's final code (compare each run's start time with the commits)?",
  "Then take each finding of review_forced2_sceptic.md, and say whether the new logs show it mended, answered or still open.",
  "Return as your final text, in at most 200 words: your findings, most serious first.",
].join("\n")

const CODE = [
  "You review, as a code reader, a change to the test code for the ankle switches of a Unity character. You did not write it.",
  "Your file: " + FOLDER + "/review_forced3_code.md.",
  SHOWN,
  "Your question, from the code: can any known-good run pass for a reason other than the thing working (a bar skipped when a profile is lost, a NaN compared as false, an empty set judged as all good, a bar judged against the value it reads)? Does each known-bad setting reach only its own run, and is every switch and setting the rig changes put back in a finally? Does any change touch a runtime file, or the off path of A, B or C? Is each number a bar uses defined once, with its unit?",
  "Return as your final text, in at most 200 words: your findings, most serious first.",
].join("\n")

phase('Mend')
const mended = await agent(MEND, { label: 'mender', phase: 'Mend', effort: 'high' })
if (!mended) {
  log('The mender returned nothing; the reviews are skipped.')
  return { mended: null }
}

phase('Review')
const reviews = await parallel([
  () => agent(PHYS, { label: 'physicist', phase: 'Review', model: 'sonnet', effort: 'medium' }),
  () => agent(SCEP, { label: 'sceptic', phase: 'Review', model: 'sonnet', effort: 'medium' }),
  () => agent(CODE, { label: 'code reader', phase: 'Review', model: 'sonnet', effort: 'medium' }),
])
return { mended, physicist: reviews[0], sceptic: reviews[1], code: reviews[2] }
