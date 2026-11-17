export const meta = {
  name: 'guard-g6-finish',
  description: 'Finish G6 of the guard hook (no commit or push carrying the profile folder path): wire it in a copy, test it, two reviews, then mend',
  phases: [
    { title: 'Build', detail: 'one builder: the wiring in guard_next.sh, the G6 cases, the old guard shown failing them, timings, the report' },
    { title: 'Review', model: 'sonnet', detail: 'a breaker in temporary repositories, and a code reader' },
    { title: 'Mend', detail: 'only if a review found something that must be fixed' },
  ],
}

const MAIN = 'D:/Programs/Unity/Projects/Wonder Gather/The Wanderer'
const HOOKS = MAIN + '/.claude/hooks'
const FOLDER = MAIN + '/Docs/Research/2026-11-16_HarnessGuideReview'

const RULES = [
  'Rules for this task:',
  '- The profile path is private. The user\'s name is the last part of $USERPROFILE. Never write that name, or the profile folder\'s path in any form, into a file, a test, a report, a saved output or your final text. In scripts, build it at run time from $USERPROFILE; everywhere else show it as <profile> (the path) or <name> (the name). Before you finish, search every file you wrote for the name, with a search built at run time, and mask what it finds.',
  '- Do not edit ' + HOOKS + '/guard.sh. It is the live hook for every command of this session, including a Unity workflow that is running now in D:/Dev/WG. Do not touch D:/Dev/WG, and do not run Unity.',
  '- Commit nothing and push nothing. The main project ' + MAIN + ' holds the user\'s own uncommitted files: never stage, reset, clean, restore or stash anything there. Your git experiments go in temporary repositories (mktemp -d), with a bare temporary repository as their remote.',
  '- The live guard refuses some commands even when their text only mentions them: staging everything (git add -A and the like, git commit -a), discarding work, a test run and a commit in one command, and a heredoc holding a doubled backslash. Put such commands in a script file written with the Write tool and run the file. Inside a temporary repository only, you may instead put WG_GUARD_OK=G1 (or the rule named) in the command.',
  '- Files with backslashes or apostrophes are written with the Write or Edit tool, not through the shell.',
  '- The plan\'s usage is watched. Before long steps, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch "select:mcp__ccd_session_mgmt__get_usage"). If the 5-hour window passes 65% or the week passes 80%, stop at a point where your report is true, and return.',
].join('\n')

const BUILD = [
  'You finish G6, a new rule of a Claude Code PreToolUse hook: a bash script that guards git use on this Windows machine, under Git Bash, for the Bash and PowerShell tools. G6 refuses a git commit or a git push that would carry the path of the user\'s own profile folder (USERPROFILE, which holds the person\'s user name) into a public repository. The user approved this rule. You build and test it; you do not change what the guard refuses beyond it.',
  '',
  'The files, in ' + HOOKS + ':',
  '- guard.sh: the live hook, rules G1 to G4. It is wired from .claude/settings.local.json as bash "$CLAUDE_PROJECT_DIR/.claude/hooks/guard.sh", with the PreToolUse JSON payload on stdin and a timeout of 10 s. Exit 2 with one stderr line refuses. Read it, and do not edit it.',
  '- guard_profile.py: the G6 checker, written but never run. Its docstring says what it reads and what it cannot see. Fix it as you need, and keep the docstring true.',
  '- guard_test.sh: the cases. Run it with bash; GUARD=<path> picks the guard it tests.',
  'And ' + FOLDER + '/guard_hook_report.md: the guard\'s record, for how its cases were built and judged.',
  '',
  'Do, in order:',
  '1. Make guard_next.sh beside guard.sh: guard.sh with G6 wired in before its last exit 0, and a G6 line in its header. When the command matches the guard\'s own git pattern ${G} followed by commit or push, and does not hold WG_GUARD_OK=G6:',
  '   - pipe the raw payload to python -I guard_profile.py, found beside the script. $0 may come with backslashes or slashes, so take its folder with ${0%[/\\\\]*} or the like, and check that this works in both forms;',
  '   - use timeout 8. If it exits 2 with a line, refuse G6 with that line. Anything else passes: the guard fails open.',
  '   - Find which python Git Bash has here. If there is none, say so.',
  '2. Add a "-- G6" section to guard_test.sh.',
  '   - Write a helper that also gives the payload a "cwd".',
  '   - Build the profile path at run time from $USERPROFILE, in these forms: as it is, cygpath -m, cygpath -u, JSON-escaped, upper case, URL-encoded, and C--Users-<name> as a folder name.',
  '   - The temporary repositories lie under the profile folder\'s temp. That is itself a case: a command that cd-s there, or a message file kept there, must pass.',
  '   - Must refuse (the content holds the path), each with its own label:',
  '     - a staged file (backslash form);',
  '     - a -F message file (MSYS form);',
  '     - -m;',
  '     - cd <repo> && git add <new untracked file> && git commit -F <file>;',
  '     - git -C <repo> commit;',
  '     - git commit <a tracked, modified, unstaged file> -m x;',
  '     - git commit -a, with WG_GUARD_OK=G1 in the command: G1 passes it, and G6 must still refuse;',
  '     - a staged file whose name holds the folder-name form;',
  '     - the JSON-escaped form in a staged file;',
  '     - a small binary file with the path inside it;',
  '     - UTF-16LE content;',
  '     - upper case;',
  '     - URL-encoded;',
  '     - --amend --no-edit when HEAD\'s message holds the path;',
  '     - bash -c "git -C <repo> commit ...";',
  '     - git commit -F - fed by a heredoc;',
  '     - the PowerShell tool: Set-Location <repo>; git commit -m x;',
  '     - git push <remote> <branch>, of a commit carrying the path to the bare remote;',
  '     - plain git push with an upstream set;',
  '     - a staged file bigger than WG_G6_MAXMB (set it small): refused as too large to check.',
  '   - Must pass, each with its own label:',
  '     - a clean commit whose -F file lies under the profile folder\'s temp;',
  '     - a staged file holding only the bare name, and one holding b<name>;',
  '     - WG_GUARD_OK=G6 with bad content;',
  '     - a clean push;',
  '     - a push of a clean commit when an older commit, already pushed, holds the path;',
  '     - git rm of a file that held the path, then commit;',
  '     - an edit that takes the path out of a file;',
  '     - a clean commit in repo B while repo A has bad content staged;',
  '     - grep -n "git commit" <file>;',
  '     - echo of the path, with no git;',
  '     - git log and git status;',
  '     - git push --delete;',
  '     - a clean PowerShell commit;',
  '     - a message that says "the profile folder" in words.',
  '   - Add any case you find worth adding.',
  '   - The test output must not show the path: mask it.',
  '3. Run the whole suite against guard_next.sh. Every old case must still pass, and every G6 case must hold. Then run the G6 cases against the old guard (GUARD=' + HOOKS + '/guard.sh), and show that the refusal cases fail there, so they test G6 itself.',
  '4. Time the hook, and quote the times:',
  '   - on a command with neither commit nor push, which should add nothing;',
  '   - on a commit and a push in a temporary repository;',
  '   - with the checker alone on a payload whose cwd is the main project, for git commit -m x and for git push origin claude/worker-showcase. These only read: nothing is committed or pushed, and git runs with GIT_OPTIONAL_LOCKS=0.',
  '5. Write ' + FOLDER + '/g6_report.md, early, and keep it current. It says:',
  '   - what G6 refuses, what it reads and what it cannot see;',
  '   - each case, and the suite\'s last line from both runs;',
  '   - the lazy way to pass these cases (for example, refusing every command that names the profile folder), and which known-good cases catch it;',
  '   - the timings;',
  '   - what was not tested.',
  '   Save the suite\'s output, masked, beside it as g6_test_output.txt.',
  '',
  RULES,
  '',
  'Return as your final text, in at most 200 words:',
  '- the suite\'s last line with guard_next.sh;',
  '- how many G6 refusal cases the old guard let through;',
  '- the timings;',
  '- what you changed in guard_profile.py;',
  '- your doubts.',
].join('\n')

const SHOWN = [
  'Shown to you:',
  '- ' + HOOKS + '/guard_next.sh, the guard with G6 wired in (not live);',
  '- ' + HOOKS + '/guard_profile.py, G6\'s checker;',
  '- ' + HOOKS + '/guard_test.sh, the cases (its "-- G6" section is new);',
  '- ' + FOLDER + '/g6_report.md and g6_test_output.txt, the builder\'s report and run.',
  'G6 must refuse a git commit or push that would carry the path of the user\'s own profile folder (USERPROFILE), and pass everything else. It fails open on its own errors.',
  'Test only by piping payloads to guard_next.sh, or to the checker; the helpers in guard_test.sh show how. Do not make it live, and do not edit guard.sh, guard_next.sh, guard_profile.py or guard_test.sh.',
  'Write your review early to the file named below, and keep it current. For each finding, give the command or the file and line, what happened, and how sure you are. Say what you were shown and what you did not read. "No finding" is a good result.',
  'Must-fix means one of these:',
  '- a way past G6 that an ordinary commit or push in this project could take, by the assistant\'s agents or by the user;',
  '- a refusal of an ordinary innocent command;',
  '- a crash or hang past the hook\'s 10 s;',
  '- the private path in any file.',
  'Should-fix is the rest worth closing.',
].join('\n')

const BREAK = [
  'You try to break G6, a new rule of a Claude Code PreToolUse hook on this Windows machine (Git Bash). You did not build it.',
  'Your file: ' + FOLDER + '/review_g6_breaker.md.',
  SHOWN,
  'Your question: can a commit or a push carry the profile path past G6, or does G6 refuse what it should not, or hang? Work in temporary repositories only. Try at least these:',
  '- other encodings and spellings of the path;',
  '- git forms: aliases, -c options, environment variables such as GIT_DIR and GIT_WORK_TREE, a git launched from a script;',
  '- PowerShell forms, and subshells;',
  '- files staged earlier in the same line;',
  '- renames, symlinks, submodules;',
  '- notes and tags, a push of a tag;',
  '- plumbing: git commit-tree, update-ref, hash-object -w, fast-import;',
  '- a stash, then a commit;',
  '- very large or many files.',
  'For each way past, say whether a hook could close it, or whether it belongs on the checker\'s "cannot see" list.',
  RULES,
].join('\n')

const CODE = [
  'You read G6\'s code line by line. G6 is a new rule of a Claude Code PreToolUse hook on this Windows machine (Git Bash). You did not build it.',
  'Your file: ' + FOLDER + '/review_g6_code.md.',
  SHOWN,
  'Your question: in guard_profile.py and in the G6 wiring of guard_next.sh, which errors would make G6 pass a bad commit, refuse a good one, crash, or run past the hook\'s 10 s? Count only fail-open holes beyond the intended ones. Read in particular:',
  '- the parsing of git\'s outputs (diff --raw -z, cat-file --batch, rev-list --objects);',
  '- paths in Windows and MSYS forms;',
  '- quoting and the tokenizer;',
  '- the regular expressions that find the path (do they match each form they claim, and what else do they match?);',
  '- the read limit and the deadline.',
  'Then check the G6 section of guard_test.sh: would each refusal case still refuse if the feature its label names were taken out? Would each passing case catch a lazy G6 that refuses too much?',
  RULES,
].join('\n')

const ITEM = {
  type: 'object',
  properties: {
    title: { type: 'string' },
    where: { type: 'string' },
    evidence: { type: 'string' },
  },
  required: ['title', 'where', 'evidence'],
}
const REVIEW = {
  type: 'object',
  properties: {
    mustFix: { type: 'array', items: ITEM },
    shouldFix: { type: 'array', items: ITEM },
    summary: { type: 'string' },
  },
  required: ['mustFix', 'shouldFix', 'summary'],
}

phase('Build')
const built = await agent(BUILD, { label: 'builder', phase: 'Build', effort: 'high' })
if (!built) {
  log('The builder returned nothing; the reviews are skipped.')
  return { built: null }
}

phase('Review')
const reviews = await parallel([
  () => agent(BREAK, { label: 'breaker', phase: 'Review', model: 'sonnet', effort: 'medium', schema: REVIEW }),
  () => agent(CODE, { label: 'code reader', phase: 'Review', model: 'sonnet', effort: 'medium', schema: REVIEW }),
])
const breaker = reviews[0]
const code = reviews[1]
const must = [...((breaker && breaker.mustFix) || []), ...((code && code.mustFix) || [])]
const should = [...((breaker && breaker.shouldFix) || []), ...((code && code.shouldFix) || [])]
log('Reviews: ' + must.length + ' must-fix, ' + should.length + ' should-fix.')
if (!must.length) {
  return { built, breaker, code, mended: null }
}

phase('Mend')
const MEND = [
  'You mend G6, a new rule of a Claude Code PreToolUse hook on this Windows machine (Git Bash), by its two reviews. You did not build it.',
  'The files are in ' + HOOKS + ': guard_next.sh (the guard with G6 wired in, not live), guard_profile.py (the checker) and guard_test.sh (the cases). The builder\'s report is ' + FOLDER + '/g6_report.md, with g6_test_output.txt. The reviews are review_g6_breaker.md and review_g6_code.md, in the same folder.',
  'Their must-fix findings, as JSON: ' + JSON.stringify(must),
  'Their should-fix findings, as JSON: ' + JSON.stringify(should),
  'Mend every must-fix finding, each with a case in guard_test.sh that fails before your mend and passes after it; show both runs. Mend the should-fix ones where it is cheap and safe. A finding you think wrong is answered with a run whose output you quote, not with an argument. Keep the checker\'s docstring true, including its "cannot see" list.',
  'Then run the whole suite against guard_next.sh, and the G6 cases against the old guard, again. Update g6_report.md with a section "Mended after the reviews": finding by finding, what was done, and the suite\'s last lines. Save the new output, masked, as g6_test_output.txt.',
  RULES,
  'Return as your final text, in at most 200 words: each finding, mended (how), answered (with the output) or not done; and the suite\'s last line.',
].join('\n')
const mended = await agent(MEND, { label: 'mender', phase: 'Mend', effort: 'high' })
return { built, breaker, code, mended }
