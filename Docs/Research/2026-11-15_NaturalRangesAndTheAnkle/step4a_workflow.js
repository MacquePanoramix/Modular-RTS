export const meta = {
  name: 'ankle-step4a-forced-checks',
  description: 'Build and run the ankle forced checks and the live test exercised check; then a physicist, a sceptic and a code reviewer who did not build them',
  phases: [
    { title: 'Build', detail: 'one builder in D:/Dev/WG: the forced checks, the exercised line, the once-said check' },
    { title: 'Review', model: 'sonnet', detail: 'physicist and sceptic, then a code reviewer' },
  ],
}

const MAIN = 'D:/Programs/Unity/Projects/Wonder Gather/The Wanderer'
const FOLDER = MAIN + '/Docs/Research/2026-11-15_NaturalRangesAndTheAnkle'

const BUILD = [
  'You are the builder for step 4a of the ankle build in a Unity 6000.6.0f1 project (URP, PhysX ArticulationBody, 50 physics steps a second). You build and run checks; you do not decide design.',
  'Work only in the validation worktree D:/Dev/WG (branch claude/wip-round, head e8bac96; its only local changes are the files listed under the git rules below). The docs live in the main project, ' + MAIN + '/Docs: read them there, write only your report file there, touch nothing else in that project, and commit nothing there.',
  '',
  'First write your report file, early, and keep it current as each check finishes: ' + FOLDER + '/forced_checks.md. If you are cut off, it must say what was done, run and read.',
  '',
  'Read first (only the parts named):',
  '- research.md in that folder: section 7.1 (lines 482-516, the switches A, B and C), the forced checks at lines 536-541, section 4.2 (B\'s speed bound; the model says 19.6 rad/s) and line 663.',
  '- build.md in that folder: lines 356-368 (the rig read-backs, and what is still not read back).',
  '- review_physicist.md lines 135-151, and critique2.md around line 69 (the Slerp from aimFrom, a fourth source of jumps in the ask).',
  '- The code in D:/Dev/WG/Assets/_WonderGather: Scripts/Units/OwnRiser.cs (AnkleLimit, AnkleSpeed, AnkleSpeedHeight, AnkleAtMost, AnkleTurnsAtMost, and where the boot drives are written); Scripts/Units/OwnBody.cs (BootsFromOwnShin, GoDown); Tests/PlayMode/AnkleMend.cs (AnkleWatch, AnkleMendArguments, and the AnkleMendTests rig run with -ankleWays Small); Tests/PlayMode/CoverRingLiveTests.cs.',
  '',
  'The switches: A is OwnRiser.AnkleLimit (0 off, 1 on); B is OwnRiser.AnkleSpeed (0 off, 20 in the arms) with AnkleSpeedHeight 0.605714; C is OwnBody.BootsFromOwnShin. All are off by default and stay so. With all of them off the game must behave exactly as today: change nothing on the off path.',
  '',
  'Build, run and read these checks. Each makes its rare event happen: it is forced, not waited for.',
  '1. A free boot on a held shin whose ask is turned through 180 degrees at several speeds, including the crossing of plus or minus 180 degrees and the Slerp from aimFrom. Measure the boot\'s speed with every switch off (expected about 60 rad/s) and with B on (at most 20; the model says 19.6). With A on there must be no jump in the target at the crossing.',
  '2. A pure twist through 180 degrees about the shin. With A on there must be no jump in the target.',
  '3. A going down while the boot whirls at 50-80 rad/s. With C on, the boot\'s offset from its shin at the next make must be within a centimetre of a healthy one\'s.',
  '4. B near its edge: a forced case where the smooth maximum in B\'s damping (as the code writes it) works near E. Read back what the drive holds.',
  '5. A target outside [-pi, pi] after a whirl: what the engine does with it, read back from the running joint. Do not reproduce or quote engine source.',
  '6. The joint-axis mapping under turns about several axes: read back that the axis the code believes it drives is the one that turns.',
  '7. The live test (CoverRingLiveTests) says at its end, for each switch that is on, whether that switch\'s event happened, in a line that cannot be missed (for example LIVE_EXERCISED and LIVE_NOT_EXERCISED, with the counts behind them). For A that is targets written unlike today\'s form; for B, damping unlike today\'s form; for C, boots slackened at a going down by itself. It must say NOT EXERCISED for C on Round, whose falls on these courses are all the panel\'s push (it never goes down by itself there). A short run is enough (-ownPushes 2 with the standard arguments below).',
  '8. The once-said messages are forgotten between tests (AnkleMendArguments.ForgetSaid, commit e8bac96). Write a check that expects the same once-said message in two tests of one run.',
  '',
  'For every check, follow AGENTS.md "Claims and checks", which you have. Before trusting a check, show it a known-bad case it must catch and a known-good one it must pass. Beside each bar, write the lazy way to meet it. Quote each switch as read back from the running body. Show that the event was forced or counted, and that each run ended by itself rather than at its LIMIT. A rig result is a claim about that rig: say what the rig set up, pinned or left out. Run the rig checks on all three miners (Small, Long and Round) wherever the check applies.',
  '',
  'If a check finds a fault in A, B or C, mend it on that switch\'s own path only, and say so in the report\'s first lines with the commit. Small\'s arm B (ank1_small) was run on e8bac96, so a mend to B means it must be run again. OwnKeeper gets no new guards.',
  '',
  'Running Unity: one Unity at a time, with nothing heavy beside it. Never edit code or tests while a run is going. Read each run to its end before the next.',
  '  cd D:/Dev/WG && LIMIT=<seconds> bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> "<testFilter>" [args]',
  'It writes D:/Dev/WG/TestRuns/<name>.log and <name>_r.xml. A run stopped half-way can fail the next launch once with "Scripts have compiler errors"; if so, launch again. Read logs through grep, not whole. If a run hangs past its LIMIT, stop only this worktree\'s Unity (the one whose command line names D:/Dev/WG).',
  'The standard live arguments: -ownAsPlayed 1 -ownRate 50 -liveOwn 1 -liveSeam 1 -liveOther 0 -ownPushes 24 -ownPushWays 0,180,90,270 -liveCap 240 -liveEnergy 2 -liveWho <Small|Long|Round> -liveOut D:/Dev/WG/TestRuns/CoverLive/<name>. Long also takes -liveRing 0. The filter is WonderGather.Tests.CoverRingLiveTests. Switches are set with -riseSet AnkleLimit=1,AnkleSpeed=20 and with -ownBodySet BootsFromOwnShin=1.',
  '',
  'Code and git rules:',
  '- Gameplay stays under Assets/_WonderGather. No UnityEditor in runtime code. Never delete or recreate a .meta file, so GUIDs are kept. Do not change the Miner FBX import settings.',
  '- Commit in D:/Dev/WG by path only. Never stage everything, and never commit with -a.',
  '- Leave these out of every commit: ProjectSettings/EditorBuildSettings.asset, ProjectSettings/ProjectSettings.asset, ProjectSettings/PackageManagerSettings.asset, the untracked Assets/ML-Agents and Assets/ML-Agents.meta, and Assets/_WonderGather/Resources/RiseModels/ SmallRise.onnx, SmallRiseFullP.onnx, SmallRisePFull.onnx and SmallRisePaceP.onnx with their metas.',
  '- A guard hook refuses some git commands (staging everything, destructive resets and cleans), even when a command\'s text only mentions them. It also refuses a test run and a commit in one command, and a heredoc with a doubled backslash.',
  '- So write commit messages and scripts with the Write tool, and commit with git commit -F <file>. End each message with the line: Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>',
  '- Push with git push origin claude/wip-round.',
  '',
  'The plan\'s usage is watched. Before each Unity run, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch "select:mcp__ccd_session_mgmt__get_usage"). If the 5-hour window passes 80%, stop at the next point where the report is true, commit what compiles, and return.',
  '',
  'The report, forced_checks.md. Its first line says which switch each check needs on. Then, for each check: what it forces, its bar, the lazy way to meet it, the known-bad and known-good runs by name, the read-back values quoted from the logs, whether each run ended by itself, and the result at the rung it reached (written, compiled, tested on a rig, failed its bar, not run). End with what was not tested. Write numbers only from logs you have read to the end.',
  '',
  'Return as your final text, in at most 250 words: the commit(s); which checks passed their bars; which failed or were not run; any fault found in A, B or C, and whether arm B must be run again.',
].join('\n')

const SHOWN = [
  'Shown to you:',
  '- the builder\'s report, ' + FOLDER + '/forced_checks.md;',
  '- the code in D:/Dev/WG: git -C D:/Dev/WG log --oneline e8bac96~1..HEAD and the diff over the same range. The switches are in Assets/_WonderGather/Scripts/Units/OwnRiser.cs and OwnBody.cs; the checks are in Assets/_WonderGather/Tests/PlayMode;',
  '- the run logs in D:/Dev/WG/TestRuns named in the report (read them through grep, not whole);',
  '- research.md in that folder, section 4.2 and lines 536-541, for what each check was meant to force.',
  'Do not run Unity, do not edit any code or test file, and commit nothing. Write your review early to the file named below, and keep it current. Give each finding a file and line, and say how sure you are. Say what you were shown and what you did not read. "No finding" is a good result.',
].join('\n')

const PHYS = [
  'You review checks that another agent built for the ankle switches of a Unity PhysX ArticulationBody character (50 steps a second). You did not build them, and you review as a physicist.',
  'Your file: ' + FOLDER + '/review_forced_physicist.md.',
  SHOWN,
  'Your question: for each check, does the rig physically make happen the event the report says it forces? That is: a boot really free; an ask really crossing plus or minus 180 degrees; a pure twist really about the shin; a whirl really at 50-80 rad/s at the going down; B really near its edge; a target really outside [-pi, pi]; each axis really the one turned. And is each bar physically right? Where the engine\'s behaviour is assumed rather than read back from the running joint, say where. Do not reproduce engine source.',
  'Return as your final text, in at most 200 words: your findings, most serious first.',
].join('\n')

const SCEP = [
  'You review, as a sceptic, checks that another agent built and ran for the ankle switches of a Unity character. You did not build them.',
  'Your file: ' + FOLDER + '/review_forced_sceptic.md.',
  SHOWN,
  'Your question: is each claim in the report what its logs say? For each check, ask:',
  '- Was it shown a known-bad case it must catch and a known-good one it must pass?',
  '- Did each run end by itself, and not at its LIMIT (look for STOPPED, and for the test\'s own end in the log)?',
  '- Is each switch quoted as read back from the running body?',
  '- Does every number in the report match the log line it comes from? Quote both.',
  '- Is the lazy way to meet each bar written down, and does the check avoid it?',
  'Also say whether any rig result is said of the game.',
  'Return as your final text, in at most 200 words: your findings, most serious first.',
].join('\n')

const CODE = [
  'You review code for correctness: test code, and switches in a Unity character\'s runtime code. Another agent wrote it; you did not.',
  'Your file: ' + FOLDER + '/review_forced_code.md.',
  SHOWN,
  'Your range starts at e8bac96~1. It includes e8bac96\'s own two test changes: CoverMeter.FindRing\'s needed path with CoverRingLiveTests\' -liveRing 0, and AnkleMendArguments.ForgetSaid.',
  'Your question:',
  '- Are there correctness bugs?',
  '- Is the game with every switch off changed in any way, by any line on the off path?',
  '- Is there UnityEditor in runtime code (under Assets/_WonderGather/Scripts)?',
  '- Was a .meta file deleted or recreated?',
  '- Is static state left between tests (the once-said flags, or switches not restored after each test)?',
  '- Can any check pass without its event happening?',
  'Return as your final text, in at most 200 words: your findings, most serious first.',
].join('\n')

phase('Build')
const built = await agent(BUILD, { label: 'builder', phase: 'Build', effort: 'high' })
if (!built) {
  log('The builder returned nothing; the reviews are skipped.')
  return { built: null, reviews: [] }
}

phase('Review')
const firstTwo = await parallel([
  () => agent(PHYS, { label: 'physicist', phase: 'Review', model: 'sonnet', effort: 'medium' }),
  () => agent(SCEP, { label: 'sceptic', phase: 'Review', model: 'sonnet', effort: 'medium' }),
])
const code = await agent(CODE, { label: 'code reviewer', phase: 'Review', model: 'sonnet', effort: 'medium' })
return { built, physicist: firstTwo[0], sceptic: firstTwo[1], code }
