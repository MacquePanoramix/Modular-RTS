export const meta = {
  name: 'ankle-step4a3',
  description: 'Step 4a3 of the ankle build: a C-on run that passes by itself, what check 3 shows, B\'s own wrap path failed on purpose, no bar passing on NaN or nothing; then a physicist, a sceptic and a code reader',
  phases: [
    { title: 'Mend', detail: 'one builder in D:/Dev/WG: the six open items of the third build\'s reviews, each run and read' },
    { title: 'Review', model: 'sonnet', detail: 'physicist, sceptic and code reader, each with its own question' },
  ],
}

const MAIN = "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer"
const FOLDER = MAIN + "/Docs/Research/2026-11-15_NaturalRangesAndTheAnkle"

const RULES = [
  "Running Unity: one Unity at a time, with nothing heavy beside it. Never edit code or tests while a run is going. Read each run to its end before the next.",
  "  cd D:/Dev/WG && LIMIT=<seconds> bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> \"<testFilter>\" [args]",
  "It writes D:/Dev/WG/TestRuns/<name>.log and <name>_r.xml. Give every run a new name; never reuse the name of an earlier build's run. A run stopped half-way can fail the next launch once with \"Scripts have compiler errors\"; if so, launch again. Read logs through grep, not whole. If a run hangs past its LIMIT, stop only this worktree's Unity (the one whose command line names D:/Dev/WG).",
  "",
  "Code and git rules:",
  "- Gameplay stays under Assets/_WonderGather. No UnityEditor in runtime code. Never delete or recreate a .meta file, so GUIDs are kept. Do not change any FBX import settings. OwnKeeper gets no new guards.",
  "- Commit in D:/Dev/WG by path only. Never stage everything, and never commit with -a.",
  "- Leave these out of every commit: ProjectSettings/EditorBuildSettings.asset, ProjectSettings/ProjectSettings.asset, ProjectSettings/PackageManagerSettings.asset, the untracked Assets/ML-Agents and Assets/ML-Agents.meta, and Assets/_WonderGather/Resources/RiseModels/ SmallRise.onnx, SmallRiseFullP.onnx, SmallRisePFull.onnx and SmallRisePaceP.onnx with their metas.",
  "- A guard hook refuses some git commands, even when a command's text only mentions them: staging everything, destructive resets and cleans, git checkout -- and git restore (copy a file back with cp instead), skipping git's hooks (--no-verify), a test run and a commit in one command, a heredoc with a doubled backslash, and a commit or push that carries the path of the user's profile folder. Git's own hooks (pre-commit, commit-msg and pre-push, in the shared .git) refuse that path too, in D:/Dev/WG as well; if one refuses, take the path out and commit again. Never write that path or the user's name into any file; where a script needs it, build it at run time.",
  "- So write commit messages and scripts with the Write tool, and commit with git commit -F <file>. End each message with the line: Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
  "- Push with git push origin claude/wip-round.",
  "",
  "The plan's usage is watched. Before each Unity run, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch \"select:mcp__ccd_session_mgmt__get_usage\"). If the 5-hour window passes 80%, or the week passes 95%, stop at the next point where the report is true, commit what compiles, and return.",
].join("\n")

const MEND = [
  "You mend and run checks for the ankle switches of a Unity 6000.6.0f1 character (URP, PhysX ArticulationBody, 50 physics steps a second). You build and run checks; you do not decide design.",
  "Work only in the validation worktree D:/Dev/WG (branch claude/wip-round, head ef12502; the checks last changed in a251444). The docs live in the main project, " + MAIN + "/Docs: read them there, write only your report file there, touch nothing else in that project, and commit nothing there.",
  "",
  "Read first:",
  "- in " + FOLDER + ": the report forced_checks.md, its newest section (\"Step 4a″\"), and the three reviews of that build, review_forced3_physicist.md, review_forced3_sceptic.md and review_forced3_code.md (earlier sections and reviews only where these point to them);",
  "- " + MAIN + "/Docs/Validation.md, the section \"The forced checks, third build\": what stands and what does not;",
  "- the checks, D:/Dev/WG/Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs;",
  "- research.md in that folder, lines 536-541 (what each check must force) and section 4.2, where a check needs them.",
  "",
  "The switches: A is OwnRiser.AnkleLimit, B is OwnRiser.AnkleSpeed (with AnkleSpeedHeight), C is OwnBody.BootsFromOwnShin. All are off by default and stay so; change nothing on the off path.",
  "",
  "The six items:",
  "1. Check 3, a C-on run that passes by itself. No C-on run passed: down_c1d and down_c1e each judged only 2 whirled courses on Long against their own rule of 3 (down_c1d.log:3243). In one run per setting, on each of Small, Long and Round, judge at least 3 distinct whirled courses with C on, and run the C-off known-bad on the same courses. A course differs in the whirl's speed or axis or the moment of the going down; repeats of one course count once. Add courses; do not lower the 0.01 m judging threshold or any bar after seeing the data. Also find why Long's going down about the third axis at 75 rad/s was not made (down_c1e.log:2800, the boot still touching after 75 steps), and mend the rig if the cause is the rig, or say what it is.",
  "2. What check 3 shows. The physicist (review_forced3_physicist.md, finding 1, 85% sure) says BAR_C_PIVOT reads which pivot the slack was given (a blend of the made boot's pose in code, with no joint turned by the engine), so it shows that C's branch ran, not that C acted on a whirling boot: the whirling boot is destroyed at the make, and plain makes separate the same way. Read, from the running body, per course, whether the whirl changes what C acts on: the made boot's pose against the shin at the make, and the slack's turn, on plain and on whirled makes, as a reading. Then write in the report what check 3 shows and what it does not, in those terms, claiming no more than what is read.",
  "3. B's own wrap path, failed on purpose. BAR_WRAP's known-bad (beyond_raw) judges the rig's own count of raw writes outside half a turn, a different branch from the count that judges B's runs (wrapViolations; review_forced3_code.md, finding 2). Give BAR_WRAP a known-bad that reaches the very counter and verdict line that judge B, in its own run only (for example, B's written target replaced, after B and before that count reads it, by one outside half a turn). The known-good B, AB and edge runs still pass.",
  "4. Nothing passes on NaN or on nothing. No bar may pass on a NaN or on an empty set (review_forced3_code.md, finding 1): BAR_FORM counts a NaN error as no failure; BAR_WRAP has no floor on the steps it judges; BAR_FOLLOW is skipped with no missed-event line when its error is NaN; one NaN hold is lost from BAR_AXES_READ's maximum. Look for the same in every other bar, BAR_C_PIVOT included. A NaN is a failure and too few judged is an event missed. Show each guard failing once on purpose: a rig setting that feeds a NaN or an empty set into each guarded measure, in its own run, which the test must fail for that reason; the known-good runs still pass.",
  "5. Numbers and words. Define each bar's number once, with its unit in its name or beside it (among them 3 and 5 and 0.05 degrees, 110 and 115 degrees, 1 rad/s, 0.01 m, 0.25). The FORCED_SET line names the bars judged now (it still names the old \"BAR_C 0.025 m\"). BAR_AXES_READ's 3 degrees was set after seeing sags of 1.2-1.9 degrees, and the known-good's worst is 2.25: ground it in something apart from the data it judges (for example the sag that the drive's stiffness and the boot's weight give, read back from the running joint), or say plainly that it was set from the data and give the margin.",
  "6. Run again, on this step's final code, after every edit: forced_barfails, forced_good, forced_bad (with the new known-bads) and forced_formbad, and check 3's runs, C off and C on; read each to its end. The report names the log file of every line number it quotes.",
  "",
  "Follow AGENTS.md \"Claims and checks\", which you have: quote each switch as read back from the running body; show that the event was forced or counted; check that each run ended by itself, not at its LIMIT; write beside each bar the lazy way to meet it; say what the rig set up, pinned or left out. A result over several runs is not a test that passed: say which single run passes.",
  "",
  "If a check finds a fault in A, B or C, mend it on that switch's own path only, and say so in the report's first lines with the commit. A mend to B means Small's arm B (ank1_small, run on e8bac96) must be run again: do not run it; say so.",
  "",
  RULES,
  "",
  "The report: update forced_checks.md early and as each item is run, in a new section at its top for this step (\"Step 4a‴\"), keeping what the earlier builds found and marking what changed and why. For each bar: what it forces, its known-bad and known-good runs by name, the lazy way to meet it, the values read back, quoted with their log file and line, whether each run ended by itself, and the rung reached. End with what was not tested.",
  "",
  "Return as your final text, in at most 250 words: the commit(s); for each of the six items, whether it was done (and how), answered (with the log file and line) or not done; any fault found in A, B or C, and whether arm B must be run again.",
].join("\n")

const SHOWN = [
  "Shown to you:",
  "- the report, " + FOLDER + "/forced_checks.md, its newest section first;",
  "- the reviews of the step before, review_forced3_physicist.md, review_forced3_sceptic.md and review_forced3_code.md in that folder;",
  "- the code in D:/Dev/WG: git -C D:/Dev/WG log --oneline ef12502..HEAD and the diff over the same range. The checks are in Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs; the switches are in Assets/_WonderGather/Scripts/Units/OwnRiser.cs and OwnBody.cs;",
  "- the run logs and result files in D:/Dev/WG/TestRuns named in the report (read them through grep, not whole);",
  "- research.md in that folder, section 4.2 and lines 536-541, for what each check was meant to force.",
  "Do not run Unity, do not edit any code or test file, and commit nothing. Write your review early to the file named below, and keep it current. Give each finding a file and line, and say how sure you are. Say what you were shown and what you did not read. \"No finding\" is a good result.",
].join("\n")

const PHYS = [
  "You review, as a physicist, checks built and run for the ankle switches of a Unity PhysX ArticulationBody character (50 steps a second). You did not build them.",
  "Your file: " + FOLDER + "/review_forced4_physicist.md.",
  SHOWN,
  "Your question: for check 3, do the courses that a single C-on run now judges differ physically, or are some repeats of one course? Does the new reading of what the whirl changes read it from the running body, and does the report's statement of what check 3 shows claim only what that reading shows? Do the new known-bads (B's own wrap path, and each NaN or empty case) fail for the reason their bar or guard claims, and not by some other effect of the rig? Is BAR_AXES_READ's allowance now grounded in something apart from the data it judges? Where is the engine's behaviour still assumed rather than read back? Do not reproduce engine source.",
  "Return as your final text, in at most 200 words: your findings, most serious first.",
].join("\n")

const SCEP = [
  "You review, as a sceptic, checks built and run for the ankle switches of a Unity character. You did not build them.",
  "Your file: " + FOLDER + "/review_forced4_sceptic.md.",
  SHOWN,
  "Your question: is each claim in the report what its logs say?",
  "- For each setting, is there one run that passes by itself? A result that holds only across several runs is not a passing test.",
  "- Does each bar decide its test: a failing bar marks its run Failed in the result file, and a known-bad run passes only because it asserts that its bar failed? The result files' asserts count is no evidence either way: this Unity writes asserts=\"0\" even for a test that failed on an Assert.That.",
  "- Did each run end by itself, and did each run start after the last change of the code it tests (compare each run's start time with the file's last write and the commits)?",
  "- Is each switch quoted as read back from the running body?",
  "- Does every number in the report match the log file and line it names? Quote both.",
  "- Is the lazy way to meet each bar written, and avoided?",
  "Then take each finding of review_forced3_physicist.md, review_forced3_sceptic.md and review_forced3_code.md, and say whether the new logs and code show it mended, answered or still open.",
  "Return as your final text, in at most 200 words: your findings, most serious first.",
].join("\n")

const CODE = [
  "You review, as a code reader, a change to the test code for the ankle switches of a Unity character. You did not write it.",
  "Your file: " + FOLDER + "/review_forced4_code.md.",
  SHOWN,
  "Your question, from the code: can any bar still pass on a NaN, on an empty set, or on a value judged against itself? Does BAR_WRAP's new known-bad go through the very counter and verdict line that judge B's runs? Does each known-bad setting reach only its own run, and is every switch and setting the rig changes put back in a finally? Is each number a bar uses defined once, with its unit? Does any change touch a runtime file, or the off path of A, B or C?",
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
