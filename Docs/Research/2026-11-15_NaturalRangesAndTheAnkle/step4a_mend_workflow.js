export const meta = {
  name: 'ankle-step4a-mend',
  description: 'Mend the ankle forced checks by their three reviews, run them again, then a physicist and a sceptic check each finding against the new logs',
  phases: [
    { title: 'Mend', detail: 'one builder in D:/Dev/WG: every finding mended or answered by a run' },
    { title: 'Review', model: 'sonnet', detail: 'physicist and sceptic, each finding against the new logs' },
  ],
}

const MAIN = 'D:/Programs/Unity/Projects/Wonder Gather/The Wanderer'
const FOLDER = MAIN + '/Docs/Research/2026-11-15_NaturalRangesAndTheAnkle'

const RULES = [
  'Running Unity: one Unity at a time, with nothing heavy beside it. Never edit code or tests while a run is going. Read each run to its end before the next.',
  '  cd D:/Dev/WG && LIMIT=<seconds> bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> "<testFilter>" [args]',
  'It writes D:/Dev/WG/TestRuns/<name>.log and <name>_r.xml. A run stopped half-way can fail the next launch once with "Scripts have compiler errors"; if so, launch again. Read logs through grep, not whole. If a run hangs past its LIMIT, stop only this worktree\'s Unity (the one whose command line names D:/Dev/WG).',
  '',
  'Code and git rules:',
  '- Gameplay stays under Assets/_WonderGather. No UnityEditor in runtime code. Never delete or recreate a .meta file, so GUIDs are kept. Do not change the Miner FBX import settings. OwnKeeper gets no new guards.',
  '- Commit in D:/Dev/WG by path only. Never stage everything, and never commit with -a.',
  '- Leave these out of every commit: ProjectSettings/EditorBuildSettings.asset, ProjectSettings/ProjectSettings.asset, ProjectSettings/PackageManagerSettings.asset, the untracked Assets/ML-Agents and Assets/ML-Agents.meta, and Assets/_WonderGather/Resources/RiseModels/ SmallRise.onnx, SmallRiseFullP.onnx, SmallRisePFull.onnx and SmallRisePaceP.onnx with their metas.',
  '- A guard hook refuses some git commands (staging everything, destructive resets and cleans), even when a command\'s text only mentions them. It also refuses a test run and a commit in one command, and a heredoc with a doubled backslash.',
  '- So write commit messages and scripts with the Write tool, and commit with git commit -F <file>. End each message with the line: Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>',
  '- Push with git push origin claude/wip-round.',
  '',
  'The plan\'s usage is watched. Before each Unity run, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch "select:mcp__ccd_session_mgmt__get_usage"). If the 5-hour window passes 80%, stop at the next point where the report is true, commit what compiles, and return.',
].join('\n')

const MEND = [
  'You mend checks that an earlier builder made for the ankle switches of a Unity 6000.6.0f1 character (URP, PhysX ArticulationBody, 50 physics steps a second). You build and run checks; you do not decide design.',
  'Work only in the validation worktree D:/Dev/WG (branch claude/wip-round, head 6b84d78). The docs live in the main project, ' + MAIN + '/Docs: read them there, write only your report file there, touch nothing else in that project, and commit nothing there.',
  '',
  'Read first:',
  '- in ' + FOLDER + ': the report forced_checks.md, and the three reviews of it: review_forced_physicist.md, review_forced_sceptic.md and review_forced_code.md;',
  '- the checks, D:/Dev/WG/Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs;',
  '- research.md in that folder, lines 536-541 (what each check must force) and section 4.2 (B\'s bound and its formula).',
  '',
  'The switches: A is OwnRiser.AnkleLimit, B is OwnRiser.AnkleSpeed (with AnkleSpeedHeight), C is OwnBody.BootsFromOwnShin. All are off by default and stay so; change nothing on the off path.',
  '',
  'Mend every finding of the three reviews, or answer it with evidence from a run. A finding you think wrong is answered by a log line you quote, not by an argument. Among them:',
  '1. Each bar decides its test: a bar that fails fails the test. A known-bad run is the same test with an argument saying the bar must fail, and the test then asserts that it failed. The result files must show asserts above 0.',
  '2. Check 3 makes a real whirl. The engine\'s own reading of the boot\'s speed is 50-80 rad/s through at least one physics step before the going down, read back from the running joint, not right after a set. Find out why the boot reads NaN the step after the going down, and say why. Set the bar so that C off passes on plain makes and fails on whirled ones before trusting it for C on. If C on is zero by construction, say so, and find what does show that C acted on a whirled boot.',
  '3. Check 4 reaches 1.5-2 E on all three miners, and its expected value is computed apart from the code: written anew in the test from research.md\'s formula, not by calling the code\'s own function.',
  '4. Check 2 takes its twist axis from the shin\'s own bones, not from AnkleFolds\' expression, and adds a profile mixing twist and swing.',
  '5. Check 6 uses the bar its report states (1 degree), or states its reason for another, and turns through 180 degrees.',
  '6. Check 1 measures the written target as a vector, so a flip is caught; its A bar\'s margin is stated; on slerp_antipode, A\'s jump is judged against the ask\'s own jump.',
  '7. Check 5 gets a bar, or is called a reading and has its mechanism read back.',
  '8. The switches the rig sets are restored in a finally.',
  '9. The report quotes its numbers from the log lines, with their line numbers.',
  '',
  'Run every mended check again, known-bad and known-good, on all three miners (Small, Long and Round) wherever it applies. Check 7 runs as played and is not run again unless its code changes. Follow AGENTS.md "Claims and checks", which you have: quote each switch as read back from the running body; show that the event was forced or counted; check that each run ended by itself; write beside each bar the lazy way to meet it; say what the rig set up, pinned or left out.',
  '',
  'If a mended check finds a fault in A, B or C, mend it on that switch\'s own path only, and say so in the report\'s first lines with the commit. A mend to B means Small\'s arm B (ank1_small, run on e8bac96) must be run again.',
  '',
  RULES,
  '',
  'The report: update forced_checks.md, early and as each mend is run. Keep what the first build found, and mark what changed and why, finding by finding. For each check: what it forces, its bar, the lazy way to meet it, the known-bad and known-good runs by name, the read-back values quoted with log line numbers, whether each run ended by itself, and the rung reached. End with what was not tested.',
  '',
  'Return as your final text, in at most 250 words: the commit(s); for each finding, whether it was mended (and how), answered (with the log line) or not done; any fault found in A, B or C, and whether arm B must be run again.',
].join('\n')

const SHOWN = [
  'Shown to you:',
  '- the report, ' + FOLDER + '/forced_checks.md, as mended;',
  '- the first reviews of it, review_forced_physicist.md, review_forced_sceptic.md and review_forced_code.md in that folder;',
  '- the code in D:/Dev/WG: git -C D:/Dev/WG log --oneline 6b84d78~1..HEAD and the diff over the same range. The checks are in Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs; the switches are in Assets/_WonderGather/Scripts/Units/OwnRiser.cs and OwnBody.cs;',
  '- the run logs and result files in D:/Dev/WG/TestRuns named in the report (read them through grep, not whole);',
  '- research.md in that folder, section 4.2 and lines 536-541, for what each check was meant to force.',
  'Do not run Unity, do not edit any code or test file, and commit nothing. Write your review early to the file named below, and keep it current. Give each finding a file and line, and say how sure you are. Say what you were shown and what you did not read. "No finding" is a good result.',
].join('\n')

const PHYS = [
  'You review, as a physicist, checks built for the ankle switches of a Unity PhysX ArticulationBody character (50 steps a second). You did not build them or mend them.',
  'Your file: ' + FOLDER + '/review_forced2_physicist.md.',
  SHOWN,
  'Your question: for each check, does the rig now physically make happen the event the report says it forces, read back from the running joint through physics steps? Is each bar physically right, and does it fail on its known-bad case for the reason it claims? Where the engine\'s behaviour is assumed rather than read back, say where. Do not reproduce engine source.',
  'Then take each finding of the first physicist\'s review, and say whether the new logs show it mended, answered or still open.',
  'Return as your final text, in at most 200 words: your findings, most serious first.',
].join('\n')

const SCEP = [
  'You review, as a sceptic, checks built and run for the ankle switches of a Unity character. You did not build them or mend them.',
  'Your file: ' + FOLDER + '/review_forced2_sceptic.md.',
  SHOWN,
  'Your question: is each claim in the report what its logs say?',
  '- Does each bar now decide its test (asserts above 0; a failing bar fails the test; a known-bad run asserts that its bar failed)?',
  '- Did each run end by itself?',
  '- Is each switch quoted as read back from the running body?',
  '- Does every number in the report match its log line? Quote both.',
  '- Is the lazy way to meet each bar written, and avoided?',
  'Then take each finding of the first sceptic\'s and the code reviewer\'s reviews, and say whether the new logs and code show it mended, answered or still open.',
  'Return as your final text, in at most 200 words: your findings, most serious first.',
].join('\n')

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
])
return { mended, physicist: reviews[0], sceptic: reviews[1] }
