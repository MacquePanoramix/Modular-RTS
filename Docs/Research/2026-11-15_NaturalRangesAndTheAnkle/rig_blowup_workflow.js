export const meta = {
  name: 'ankle-rig-blowup',
  description: 'Find what sets the forced rig\'s boot whirling at its start with every switch off, from the running body step by step; mend the rig only; then a physicist reviews',
  phases: [
    { title: 'Find', detail: 'one finder in D:/Dev/WG: the cause of the start blow-up, read and forced; a mend in AnkleForced.cs only' },
    { title: 'Review', model: 'sonnet', detail: 'a physicist who did not make it, with its own question' },
  ],
}

const MAIN = "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer"
const FOLDER = MAIN + "/Docs/Research/2026-11-15_NaturalRangesAndTheAnkle"

const RULES = [
  "Running Unity: one Unity at a time, with nothing heavy beside it. Never edit code or tests while a run is going. Read each run to its end before the next.",
  "  cd D:/Dev/WG && LIMIT=<seconds> bash Docs/Technical/AnyBody/tools/unity_tests.sh <name> \"<testFilter>\" [args]",
  "It writes D:/Dev/WG/TestRuns/<name>.log and <name>_r.xml. Give every run a new name; never reuse the name of an earlier run. A run stopped half-way can fail the next launch once with \"Scripts have compiler errors\"; if so, launch again. Read logs through grep, not whole. If a run hangs past its LIMIT, stop only this worktree's Unity (the one whose command line names D:/Dev/WG).",
  "",
  "Code and git rules:",
  "- Gameplay stays under Assets/_WonderGather. No UnityEditor in runtime code. Never delete or recreate a .meta file, so GUIDs are kept. Do not change any FBX import settings. OwnKeeper gets no new guards.",
  "- Commit in D:/Dev/WG by path only. Never stage everything, and never commit with -a.",
  "- Leave these out of every commit: ProjectSettings/EditorBuildSettings.asset, ProjectSettings/ProjectSettings.asset, ProjectSettings/PackageManagerSettings.asset, the untracked Assets/ML-Agents and Assets/ML-Agents.meta, and Assets/_WonderGather/Resources/RiseModels/ SmallRise.onnx, SmallRiseFullP.onnx, SmallRisePFull.onnx and SmallRisePaceP.onnx with their metas.",
  "- A guard hook refuses some git commands, even when a command's text only mentions them: staging everything, destructive resets and cleans, git checkout -- and git restore (copy a file back with cp instead), skipping git's hooks (--no-verify), a test run and a commit in one command, a heredoc with a doubled backslash, and a commit or push that carries the path of the user's profile folder. Git's own hooks (pre-commit, commit-msg and pre-push, in the shared .git) refuse that path too, in D:/Dev/WG as well; if one refuses, take the path out and commit again. Never write that path or the user's name into any file; where a script needs it, build it at run time.",
  "- So write commit messages and scripts with the Write tool, and commit with git commit -F <file>. End each message with the line: Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>",
  "- Push with git push origin claude/wip-round.",
].join("\n")

const FIND = [
  "You find the cause of a fault in a test rig for a Unity 6000.6.0f1 character (URP, PhysX ArticulationBody, 50 physics steps a second). You find causes and show them by runs; you do not decide design.",
  "Work only in the validation worktree D:/Dev/WG (branch claude/wip-round, head 6408d91). The docs live in the main project, " + MAIN + "/Docs: read them there, write only your report file there, touch nothing else in that project, and commit nothing there.",
  "",
  "Read first:",
  "- in " + FOLDER + ": forced_checks.md, its newest section (\"Step 4a‴\"), above all its paragraph \"New finding, unmended\"; and, of the three reviews of that build, what they say of the blow-up: review_forced4_sceptic.md (findings 1 and 2) and review_forced4_physicist.md (finding 5);",
  "- " + MAIN + "/Docs/Validation.md, the section \"The forced checks, fourth build\";",
  "- the rig, D:/Dev/WG/Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs: how it takes the body (the lift, the root made immovable, the riser's clock held, every miner given Small's ways up), the first writes to the left boot and its shin, and the FORCED_PINNED and FORCED_BLOW lines.",
  "",
  "The fault, from the logs (D:/Dev/WG/TestRuns):",
  "- With every switch off (forced_bad5: -forcedConfigs off; its full command line is at the top of the log, lines 20-60), the left boot passes 100 rad/s within the first 10 steps on all 15 of Small's cases, touching nothing. forced_bad5.log:1097: \"first past 100 rad/s at step 1: speeds (17.4 -14.9 246.8) rad/s, the joint at (-2.982 -1.546 -1.432) rad, the target written (-0.1, -0.1, 0.0) deg, touching False\"; :1197 (step 6), :1311 (step 2). So at the first judged step the boot's joint is already about 3 rad from its target: something before it moves the joint far from where a hanging boot would be.",
  "- Long and Round blew too, less: 9 FORCED_BLOW lines in forced_bad5 between 101.8 and 149.8 rad/s, and 2 in forced_formbad3 (the sceptic). forced_good5 has none. forced_good4 has no FORCED_BLOW line, but its FORCED_READ lines show Small A's swing cases at 1600 to 13412 rad/s. Up to 9344.5 rad/s in forced_formbad3.",
  "- The start differs from run to run because the body lives its own idle before the rig takes it. forced_bad5.log:988: \"FORCED_PINNED Small: the hips at 0.669 m before, 2.169 m after the lift (immovable True); the left boot at 1.860 m, its toe at 1.870 m\" (Small's boot at 1.959, 2.031, 2.055 and 2.115 m in other runs).",
  "",
  "What is known of this engine, from earlier work in this project (Docs/Design/AnyBody/motion_design.md section 6.2, K1, in the main project; ArticulationFactsTests.AFastLegGainsByTheStepItself in D:/Dev/WG; reviewed in Docs/Reviews/2026-11-10_FlyingBoot_K1):",
  "- PhysX articulations at 50 steps a second add energy to a chain that turns fast and that nothing damps: a bare leg left alone at 30 rad/s holds a thousand times its energy after a second, its joints at the engine's bound of 100 rad/s. One part on one hinge keeps its energy exactly.",
  "- The engine's own telling of a fast part's speed (linearVelocity, GetPointVelocity) is too great, by sqrt(1 + (turning x step)^2): reckon speed and energy from how far each part moved or turned in a step (TrulyGoes in ArticulationFactsTests does so).",
  "- A part turning faster than its maxAngularVelocity is dragged back a quarter of the excess a step: not a cap.",
  "- What starts it is a light part set spinning within one step: a torque put straight on a part (AddTorque), above all an ankle's on a boot (about 0.003 kg m^2) that is not flat on the ground. So when anything whirls, count the energy first; do not add guards.",
  "",
  "Your question: what, from the moment the rig takes the body to the first judged steps, sets the left boot whirling with every switch off? Candidates to test, none to assume: how the root is lifted and made immovable while the body moves; a write of a joint position, a joint velocity, a drive target or a torque on the boot or its shin at the take-over (by the rig, OwnBody, OwnRiser, OwnKeeper, or the ways up it is given); the boot's joint read past a limit or wrapped; the drive's stiffness against the boot's small inertia at 50 steps a second; anything the idle leaves turning.",
  "",
  "The method:",
  "1. Read the take-over code and every path that writes the left boot or its shin in the steps around it (search AnkleForced.cs and Assets/_WonderGather/Scripts/Units for AddTorque, AddForce, jointPosition, jointVelocity, xDrive, yDrive, zDrive, driveTarget, TeleportRoot, immovable, maxJointVelocity, maxAngularVelocity).",
  "2. Read from the running body, on each of Small, Long and Round, every step from a few before the lift to step 10: the left boot's and its shin's joint positions, joint velocities and drive targets, the drives' stiffness, damping and force limit as read back; the boot's turn in each step, reckoned from its pose; the leg's energy reckoned from the poses, against what its drives give; and what wrote the boot or the shin that step, by which path. Put this reading in the rig behind an argument of its own, off by default.",
  "3. Show the cause by changing one thing only: a start that must blow and a start that must not, which differ by the named cause alone, each on Small, Long and Round. The blow-up comes and goes with the start, so a start that must blow is one the rig sets on purpose, not one waited for. Repeats of one identical course are one course; count courses.",
  "4. If the cause is in the rig, and its mend is in AnkleForced.cs alone, mend it there. Then run forced_good and forced_bad once each on the final code (their command lines are at the tops of forced_good5.log and forced_bad5.log; give the runs new names) and read each to its end: FORCED_BLOW must be 0 on every miner, quoted as a count per miner, and every known-bad must still fail for its own reason.",
  "   If the cause is in runtime code (OwnBody, OwnRiser, OwnKeeper, or any script outside Tests), change no runtime code: say where, with the steps read, and whether the same kick could act in the game as played, where the hips are not pinned. That goes to the master.",
  "",
  "Bounds: at most 8 Unity runs in all. Change nothing on the off path of the ankle switches (A is OwnRiser.AnkleLimit, B is OwnRiser.AnkleSpeed with AnkleSpeedHeight, C is OwnBody.BootsFromOwnShin; all off by default and they stay so).",
  "",
  "Follow AGENTS.md \"Claims and checks\", which you have: quote each switch and setting as read back from the running body; show that the event was forced or counted; check that each run ended by itself, not at its LIMIT; write beside each bar the lazy way to meet it; say what the rig set up, pinned or left out. Every \"never\", \"none\" or \"all\" you write quotes the count it rests on, per miner, with its log file and line: the last report's \"Long and Round never blew\" was false. A result over several runs is not a test that passed: say which single run passes.",
  "",
  RULES,
  "",
  "The plan's usage is watched. Before each Unity run, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch \"select:mcp__ccd_session_mgmt__get_usage\"). If the 5-hour window passes 80%, or the week passes 93.5%, stop at the next point where the report is true, commit what compiles, and return.",
  "",
  "The report: write it early and keep it current, as a new section at the top of " + FOLDER + "/forced_checks.md, \"The rig's start blow-up (November 18)\", keeping every earlier section as it is. Say what was read, the cause (or each candidate ruled out, and by which run), each run by name with its log lines, whether each ended by itself, the rung reached, and what was not tested. Name the log file of every line number you quote.",
  "",
  "Return as your final text, in at most 250 words: the commit(s); the cause, found or not (with its log file and line); whether the rig was mended and whether forced_good and forced_bad each passed in one run; anything found in runtime code.",
].join("\n")

const PHYS = [
  "You review, as a physicist, a search for the cause of a fault in a test rig for a Unity PhysX ArticulationBody character (50 steps a second). You did not make it.",
  "Your file: " + FOLDER + "/review_blowup_physicist.md.",
  "Shown to you:",
  "- the report, " + FOLDER + "/forced_checks.md, its newest section (\"The rig's start blow-up\"), and the section below it (\"Step 4a‴\") for the fault as first found;",
  "- the code in D:/Dev/WG: git -C D:/Dev/WG log --oneline 6408d91..HEAD and the diff over the same range (the rig is Assets/_WonderGather/Tests/PlayMode/AnkleForced.cs);",
  "- the run logs and result files in D:/Dev/WG/TestRuns that the report names (read them through grep, not whole);",
  "- what this project knew of the engine before: " + MAIN + "/Docs/Design/AnyBody/motion_design.md, section 6.2 (K1): a fast chain that nothing damps gains energy at 50 steps a second; the engine's told speed of a fast part is too great, so speed and energy are reckoned from the poses; a light boot set spinning in one step starts it.",
  "Do not run Unity, do not edit any code or test file, and commit nothing. Write your review early to your file, and keep it current. Give each finding a file and line, and say how sure you are. Say what you were shown and what you did not read. \"No finding\" is a good result.",
  "",
  "Your question:",
  "- Is the named cause read from the running body, step by step, or inferred? Does the energy count, reckoned from poses and not from the engine's told speeds, show where the energy enters, and when?",
  "- Do the start that must blow and the start that must not differ by the one thing named, and does each do what it must on Small, Long and Round, as distinct courses?",
  "- If the rig was mended: does forced_good pass and forced_bad pass in one run each? Count the FORCED_BLOW lines per miner yourself, with grep, and check that every known-bad still fails for its own reason. Is the mend in the rig only, with nothing on the off path of A, B or C, and does it hide the fault rather than remove its cause (a cap, a skip of the first steps, a retry until a start does not blow)?",
  "- Did each run end by itself, and start after the last write of the code it tests?",
  "- Does the report claim anything about the game as played that its runs do not show? Where is the engine's behaviour still assumed rather than read back? Do not reproduce engine source.",
  "",
  "The plan's usage is watched: if mcp__ccd_session_mgmt__get_usage shows the 5-hour window past 80% or the week past 95%, write what you have and return.",
  "Return as your final text, in at most 200 words: your findings, most serious first.",
].join("\n")

phase('Find')
const found = await agent(FIND, { label: 'finder', phase: 'Find', effort: 'high' })
if (!found) {
  log('The finder returned nothing; the review is skipped.')
  return { found: null }
}

phase('Review')
const physicist = await agent(PHYS, { label: 'physicist', phase: 'Review', model: 'sonnet', effort: 'medium' })
return { found, physicist }
