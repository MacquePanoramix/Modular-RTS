export const meta = {
  name: 'g6-git-hooks',
  description: 'Git hooks for this repository only (pre-commit, commit-msg, pre-push) refusing a commit or push that carries the profile folder path; the guard refuses agents skipping them; built and tested in temporary repositories, two reviews, then a mend',
  phases: [
    { title: 'Build', detail: 'one builder: the hooks and their installer, G7 and M1 in candidate copies of the guard, the cases, timings, the report' },
    { title: 'Review', model: 'sonnet', detail: 'a breaker in temporary repositories, and a code reader' },
    { title: 'Mend', detail: 'only if a review found something that must be fixed' },
  ],
}

const MAIN = 'D:/Programs/Unity/Projects/Wonder Gather/The Wanderer'
const HOOKS = MAIN + '/.claude/hooks'
const GITH = HOOKS + '/git'
const NEXT = HOOKS + '/next'
const FOLDER = MAIN + '/Docs/Research/2026-11-16_HarnessGuideReview'

const RULES = [
  'Rules for this task:',
  '- The profile path is private. The user\'s name is the last part of $USERPROFILE. Never write that name, or the profile folder\'s path in any form, into a file, a test, a report, a saved output or your final text. In scripts, build it at run time from $USERPROFILE; everywhere else show it as <profile> (the path) or <name> (the name). Before you finish, search every file you wrote for the name, with a search built at run time, and mask what it finds.',
  '- Never install anything into the real repository\'s hooks folder (' + MAIN + '/.git/hooks, which D:/Dev/WG and the other worktrees share), and never set core.hooksPath in its config: the master installs after the reviews. Install only into temporary repositories.',
  '- Do not edit ' + HOOKS + '/guard.sh or ' + HOOKS + '/guard_profile.py: they are the live hook for every command of this session. Candidate versions go in ' + NEXT + '/ under the same file names.',
  '- Commit nothing and push nothing in the real repository. The main project ' + MAIN + ' holds the user\'s own uncommitted files: never stage, reset, clean, restore or stash anything there. Only read-only git calls there, with GIT_OPTIONAL_LOCKS=0. Do not touch D:/Dev/WG, and do not run Unity.',
  '- Your git experiments go in temporary repositories under your own folder (named below), each with a bare temporary repository as its remote, never under the profile folder. When you finish, move your folder to the Recycle Bin, after checking it holds only what you made. If that is refused, leave it and name it in your report.',
  '- The live guard refuses some commands even when their text only mentions them: staging everything (git add -A and the like, git commit -a), discarding work, a test run and a commit in one command, a heredoc holding a doubled backslash, and a commit or push carrying the profile path. Put such commands in a script file written with the Write tool and run the file. Inside a temporary repository only, you may instead put WG_GUARD_OK=G1 (or the rule named) in the command.',
  '- Files with backslashes or apostrophes are written with the Write or Edit tool, not through the shell.',
  '- Python: D:/Programs/Python/Python 3.12/python.exe. python3 on PATH is the Windows Store stub, and does not run.',
  '- The plan\'s usage is watched. Before long steps, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch "select:mcp__ccd_session_mgmt__get_usage"). If the 5-hour window passes 80% or the week passes 85%, stop at a point where your report is true, and return.',
].join('\n')

const BUILD = [
  'You build git\'s own side of a privacy check on this Windows machine (Git for Windows; the user also commits from other programs, such as GitHub Desktop). The check refuses a commit or a push that would carry the path of the user\'s own profile folder (USERPROFILE, which holds the person\'s user name) into this public repository. The user approved it for this repository only.',
  'Git keeps hooks in the repository\'s common git folder (git rev-parse --git-common-dir), so hooks placed there act on this repository and its worktrees, and on no other repository.',
  '',
  'What exists, in ' + HOOKS + ' (read them first):',
  '- guard.sh: a Claude Code PreToolUse hook for the assistant\'s own Bash and PowerShell commands. Rules G1 to G4, and G6, which runs guard_profile.py on a git commit or push. Exit 2 with one stderr line refuses; WG_GUARD_OK=Gn in a command passes rule n.',
  '- guard_profile.py: G6\'s checker. Its docstring says what it reads and what it cannot see, and how the path\'s forms are built (the "needles").',
  '- guard_test.sh: 154 cases. GUARD=<path> picks the guard it tests; ONLY=G6 runs one section.',
  'And in ' + FOLDER + ': g6_report.md (how G6 was built), review_g6_breaker.md and review_g6_code.md (what its reviewers found). A hook run before the command cannot see what the same command line does first, and misses some forms of git; git\'s own hooks see the commit and the push as they really are. That is why these hooks are wanted.',
  '',
  'Build, in ' + GITH + ':',
  '1. profile_check.py, self-contained (it is copied into the hooks folder and must not import from the working tree). The needles are the same forms as guard_profile.py\'s: the parent of the profile folder and the name, any of their separators, any case, in UTF-8 and in UTF-16LE. Modes:',
  '   - pre-commit: every staged path name, and the whole content of every added or modified staged blob, against HEAD (against the empty tree on a first commit). Stream the contents through one git cat-file --batch process. No size limit: git\'s hooks have no time limit, and a big asset must still be read.',
  '   - commit-msg <file>: the message file.',
  '   - pre-push <remote> <url>, with git\'s ref lines on stdin. For each line that is not a delete, the objects the push would send: git rev-list --objects <local oid> --not --remotes=<that remote>, and also not <remote oid> when that object exists here. When the remote is a URL with no remote-tracking refs, exclude only the remote oid; with none, read the whole history of that ref. In those objects, read: every blob\'s content; every path name that rev-list prints (this covers file and folder names); and every commit and tag object raw (messages and names).',
  '   - A hit exits 1 with a short message: which file, name or message, the path shown as <profile>, and how to skip the check for one command (git commit --no-verify, git push --no-verify), adding that only the user decides to skip. Any error of its own passes with one warning line on stderr ("profile check skipped: <why>"). WG_PROFILE_TRACE=1 in the environment prints one line on success: what was read (counts and bytes) and 0 hits.',
  '2. pre-commit, commit-msg and pre-push: small #!/bin/sh scripts that find Python as guard.sh does, skipping the Store stub, and run python -I on profile_check.py beside them, passing their arguments and stdin. With no Python, they pass with the warning line.',
  '3. install.sh: copies the three hooks and profile_check.py into "$(git rev-parse --git-common-dir)/hooks/", with LF line endings and a marker line. It refuses to overwrite a hook without the marker, says so, and changes nothing then. It is safe to run twice and says what it did. Also uninstall.sh, which removes only the marked files.',
  '4. test_git_hooks.sh: the cases, in temporary repositories under D:/Dev/g6git_tmp/builder, with the hooks installed into each by install.sh. The profile path is built from $USERPROFILE at run time, in the forms guard_test.sh builds. Mask it in all output.',
  '   Must refuse, each with its own label:',
  '   - commits: the path in a staged file (each form: backslashes, cygpath -m, MSYS, JSON-escaped, upper case, URL-encoded, UTF-16LE); in a staged file\'s name or folder name (C--Users-<name> and Users/<name>/...); in a binary file; in -m, in -F and in an editor message; in --amend; in a merge commit\'s message; in a commit from git -C <dir>;',
  '   - pushes: a branch; a new branch (remote oid all zeros); an annotated tag; notes; a wildcard refspec (\'refs/heads/*:refs/heads/*\'); --all; --mirror; remote.<r>.push in the config; push.default=matching; a push by URL; a force push; a commit made by plumbing (commit-tree and update-ref, which run no commit hook); a leak in a path name only; a leak held by a second remote but not by the one pushed to;',
  '   - a 200 MB staged blob with the path at its end.',
  '   Must pass, each with its own label: clean commits and pushes; the bare name alone; C:\\Users\\Public; the words "the profile folder"; ~ and %USERPROFILE% as text; deleting a file that held the path; a clean push on top of a leak already on the remote; a delete push; 3000 small files.',
  '   Then run the refusal cases with no hooks installed, and show they fail there, so they test the hooks.',
  '5. G7, in ' + NEXT + '/guard.sh (a copy of guard.sh with G7 added and named in its header). It refuses, unless WG_GUARD_OK=G7 is in the command: git commit with --no-verify or a short option cluster holding n; git push --no-verify; core.hooksPath in a git command (-c, git config, or the GIT_CONFIG_* variables); and a command that writes into, moves or deletes files in .git/hooks or .git\\hooks (rm, mv, cp, redirection, tee, sed -i, chmod, Remove-Item, Move-Item, Copy-Item, Set-Content, Out-File, New-Item). Read-only uses pass, such as ls, cat and grep. Add a "-- G7" section to guard_test.sh. Must pass: git push -n (a dry run); git add -n; a message that says --no-verify inside its quotes; git log; ls .git/hooks.',
  '6. M1, in ' + NEXT + '/guard_profile.py (a copy of guard_profile.py): the cat-file --batch-check format with %(objectname) %(objecttype) %(objectsize) was slow (500 files 5.6 s; 1500 refused as too large; 3000 passed unchecked after the hook\'s 8 s). The breaker found %(objectsize) alone, or --batch, at 0.1 to 0.2 s. Mend it, with a case that fails before and passes after.',
  '   ' + NEXT + '/guard.sh must find guard_profile.py beside itself, so the two candidates are tested together. The master later copies both over the live ones.',
  '7. Run the whole guard suite with GUARD=' + NEXT + '/guard.sh: all 154 old cases must still pass, as well as the new ones. Run the G7 cases against the live guard too, and show that they fail there.',
  '8. Time, and quote: a typical commit, a 200 MB blob, 3000 files, and the next push of each of this repository\'s two working branches (claude/worker-showcase and claude/wip-round), read only. Run profile_check.py pre-push on ref lines built from git rev-parse, with nothing pushed.',
  '9. Write ' + FOLDER + '/g6_git_report.md early, and keep it current. It says:',
  '   - what each hook reads, what it cannot see (for example a push from a program that runs no hooks, the GitHub website and API, and the Write and Edit tools writing into .git/hooks), and how to install and uninstall;',
  '   - each case, and the last line of every suite run;',
  '   - the lazy way to pass the cases, and which known-good case catches it;',
  '   - the timings;',
  '   - what was not tested.',
  '   Save the outputs, masked, beside it as g6_git_test_output.txt.',
  '',
  RULES,
  '- Your folder for temporary repositories: D:/Dev/g6git_tmp/builder.',
  '',
  'Return as your final text, in at most 200 words: the suites\' last lines; how many refusal cases fail with no hooks installed; how many G7 cases fail against the live guard; the timings; your doubts.',
].join('\n')

const SHOWN = [
  'Shown to you:',
  '- ' + GITH + '/: profile_check.py, pre-commit, commit-msg, pre-push, install.sh, uninstall.sh and test_git_hooks.sh: git hooks for this repository that refuse a commit or push carrying the path of the user\'s own profile folder (USERPROFILE). They are not installed in the real repository;',
  '- ' + NEXT + '/guard.sh and guard_profile.py: candidates for the live Claude Code guard (G7 refuses the assistant\'s commands that skip or change those hooks; M1 is a speed mend);',
  '- ' + HOOKS + '/guard_test.sh, with its "-- G7" section;',
  '- ' + FOLDER + '/g6_git_report.md and g6_git_test_output.txt, the builder\'s report and runs.',
  'The hooks pass on their own errors, with a warning line.',
  'Test only in temporary repositories, with the hooks installed there by install.sh, and by piping payloads to ' + NEXT + '/guard.sh. Do not edit the files you review.',
  'Write your review early to the file named below, and keep it current. For each finding, give the command or the file and line, what happened, and how sure you are. Say what you were shown and what you did not read. "No finding" is a good result.',
  'Must-fix means one of these:',
  '- a way for an ordinary commit or push in this repository, by the user from any git program or by the assistant\'s agents, to carry the path past the hooks without --no-verify;',
  '- a refusal of an ordinary innocent commit or push;',
  '- a crash, or a hang;',
  '- an installer that harms an existing hook or the repository;',
  '- the private path in any file.',
  'Should-fix is the rest worth closing.',
].join('\n')

const BREAK = [
  'You try to break git hooks for a privacy check on this Windows machine. You did not build them.',
  'Your file: ' + FOLDER + '/review_g6git_breaker.md.',
  SHOWN,
  'Your question: can a commit or a push carry the profile path past the hooks? Does a hook refuse what it should not, or hang? Can the assistant\'s commands skip the hooks past G7? Try at least these:',
  '- pushes: refspec forms, tags, notes, remotes by name and by URL, several remotes, new branches, force pushes, pushes from a worktree;',
  '- commits: rebase, cherry-pick, revert, merge with a conflict, stash then push of refs/stash, commit-tree, fast-import;',
  '- content: other encodings and spellings of the path, compressed or binary files, file and folder names, symlinks, submodules;',
  '- odd names: spaces and newlines in path names;',
  '- size: very large files, many files;',
  '- the installer: run it twice, run it with a foreign hook already in place, run it from a worktree;',
  '- G7: other ways for the assistant\'s command to skip or replace the hooks, and innocent commands that G7 refuses.',
  'For each way past, say whether it could be closed, or whether it belongs on the "cannot see" list.',
  RULES,
  '- Your folder for temporary repositories: D:/Dev/g6git_tmp/breaker.',
].join('\n')

const CODE = [
  'You read, line by line, the code of git hooks for a privacy check on this Windows machine, and the guard rule G7 beside them. You did not build them.',
  'Your file: ' + FOLDER + '/review_g6git_code.md.',
  SHOWN,
  'Your question: which errors would make a hook pass a bad commit or push, refuse a good one, crash or hang, or make the installer harm the repository? Count only the holes beyond the intended ones. Read in particular:',
  '- the parsing of git\'s outputs (diff-index or diff --cached, cat-file --batch, rev-list --objects with names), with odd names;',
  '- the pre-push ref lines, zero oids, and objects missing here;',
  '- the needles and the regular expressions: do they match each form they claim, and what else do they match?',
  '- the Python search, the shell scripts and their line endings, and the exit codes;',
  '- install.sh and uninstall.sh;',
  '- G7\'s patterns in ' + NEXT + '/guard.sh, and the M1 mend.',
  'Then check test_git_hooks.sh and guard_test.sh\'s G7 section. Would each refusal case still refuse if the feature its label names were taken out? Would each passing case catch a lazy check that refuses too much?',
  RULES,
  '- Your folder for temporary repositories, if you need one: D:/Dev/g6git_tmp/code.',
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
  'You mend git hooks for a privacy check on this Windows machine, and the guard rule G7 beside them, by their two reviews. You did not build them.',
  'The files: ' + GITH + '/ (the hooks, install.sh, uninstall.sh, test_git_hooks.sh); ' + NEXT + '/guard.sh and guard_profile.py (the guard candidates); ' + HOOKS + '/guard_test.sh. The builder\'s report is ' + FOLDER + '/g6_git_report.md, with g6_git_test_output.txt. The reviews are review_g6git_breaker.md and review_g6git_code.md, in the same folder.',
  'Their must-fix findings, as JSON: ' + JSON.stringify(must),
  'Their should-fix findings, as JSON: ' + JSON.stringify(should),
  'Mend every must-fix finding, each with a case that fails before your mend and passes after it; show both runs. Mend the should-fix ones where it is cheap and safe. A finding you think wrong is answered with a run whose output you quote, not with an argument. Keep the "cannot see" list true.',
  'Then run test_git_hooks.sh, and the whole guard suite with GUARD=' + NEXT + '/guard.sh, again. Update g6_git_report.md with a section "Mended after the reviews": finding by finding, what was done, and the suites\' last lines. Save the new output, masked, as g6_git_test_output.txt.',
  RULES,
  '- Your folder for temporary repositories: D:/Dev/g6git_tmp/mender.',
  'Return as your final text, in at most 200 words: each finding, mended (how), answered (with the output) or not done; and the suites\' last lines.',
].join('\n')
const mended = await agent(MEND, { label: 'mender', phase: 'Mend', effort: 'high' })
return { built, breaker, code, mended }
