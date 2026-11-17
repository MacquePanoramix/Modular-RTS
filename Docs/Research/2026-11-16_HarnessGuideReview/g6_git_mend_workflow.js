export const meta = {
  name: 'g6-git-hooks-mend',
  description: 'Mend git hooks for the profile-path privacy check by their two reviews (the git side, and a short fixed list for the guard candidate), rerun every case, then a rerunner and a code reader who did not mend them',
  phases: [
    { title: 'Mend', detail: 'one mender: the git side items 1-9, the guard candidate items a-e, both suites' },
    { title: 'Review', model: 'sonnet', detail: 'a rerunner (the reviews own cases, the suites) and a code reader (the diff)' },
  ],
}

const MAIN = "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer"
const GITH = MAIN + "/.claude/hooks/git"
const NEXT = MAIN + "/.claude/hooks/next"
const FOLDER = MAIN + "/Docs/Research/2026-11-16_HarnessGuideReview"
const BASE = "29faa56"

const RULES = [
  "Rules:",
  "- Privacy: the path of the user's profile folder and the user's name are never written into any file, message or saved output. Scripts build them at run time from the environment; reports write <profile> and <name>. Mask every saved output. Before you return, search every file you wrote for the name, built at run time, and say the count.",
  "- Nothing is installed in the real repository: never write into its .git/hooks, never set core.hooksPath, never commit or push there. The master installs and commits after reading your work.",
  "- install.sh and uninstall.sh act on the repository given as their argument, and on the current folder's repository when none is given; your working folder is the real repository. So always pass your temporary repository's folder, and never run either with no argument.",
  "- Do not edit the live guard: .claude/hooks/guard.sh, guard_profile.py and guard_test.sh stay as they are. The candidates are in " + NEXT + ".",
  "- Do not touch D:/Dev/WG or any other worktree, and do not run Unity.",
  "- Temporary repositories go only in D:/Dev/g6git_tmp/<your role> (the suite takes G6GIT_DIR). Leave D:/Dev/g6git_tmp/breaker alone: it belongs to an earlier reviewer. Make no file names holding a newline. At the end, send your own folder to the Recycle Bin (PowerShell: [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteDirectory(path, 'OnlyErrorDialogs', 'SendToRecycleBin') after Add-Type -AssemblyName Microsoft.VisualBasic); if that is refused, say so and leave it.",
  "- The live guard refuses some commands even when they are only mentioned: staging everything, git checkout -- and git restore, a heredoc holding a doubled backslash, a test run and a commit in one command, and a commit or push carrying the profile path. Write every script and every file holding a backslash with the Write or Edit tool.",
  "- Python is D:/Programs/Python/Python 3.12/python.exe (python3 here is the Store stub); run it with -I.",
  "- Usage: before each long run, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch \"select:mcp__ccd_session_mgmt__get_usage\"). If the 5-hour window passes 80% or the week passes 95%, stop at the next point where your report is true, and return.",
].join("\n")

const MEND = [
  "You mend git hooks for a privacy check in this repository (Git for Windows), by two reviews of them. You did not build them. The check refuses a commit or a push that would carry the path of the user's own profile folder (it holds the user's name) into this public repository. The user approved the hooks for this repository only.",
  "The files: " + GITH + " (profile_check.py, pre-commit, commit-msg, pre-push, install.sh, uninstall.sh, test_git_hooks.sh), and candidate copies of the assistant's guard in " + NEXT + " (guard.sh, guard_profile.py, guard_test.sh). As built and reviewed, they are commit " + BASE + ".",
  "Read first, in " + FOLDER + ": g6_git_report.md; review_g6git_code.md, whole; review_g6git_breaker.md, its sections M-1, M-5, S-1, S-2, S-3 and S-6 (the git side). The guard's items are given below in full; you need not read the rest of the breaker's review.",
  "",
  "Part 1, the git side. Each item gets a case in test_git_hooks.sh, shown failing on " + BASE + "'s files and passing after the mend (a pass case the other way round):",
  "1. Replace objects off on every git call of profile_check.py (GIT_NO_REPLACE_OBJECTS=1 in its environment, or --no-replace-objects), so what is read is what git sends. Cases: a push of a dirty blob hidden behind a replace ref to a clean one, and of a dirty commit message hidden the same way: both refused.",
  "2. commit-msg reads the message as git will store it. Git cleans the message after the hook has run, and what it removes depends on how the commit was made: with -v (or commit.verbose) it cuts everything from its own scissors line on; when the editor ran, the default cleanup (strip) also drops comment lines (core.commentChar, default #); with -m or -F and no editor, the default (whitespace) keeps them, so a # line given with -m or -F is stored; commit.cleanup in the config changes the default. The hook cannot see the command line. So: cut at a scissors line only when it is git's own (the line followed by git's two comment lines saying the line above must not be changed and what is below is ignored); drop comment lines only when git will (the file holds the template git writes when it opens the editor, whose own text says lines starting with the comment character will be ignored, or the config's commit.cleanup is strip); otherwise read every line. What it cannot tell (a --cleanup on the command line, a template in another language) goes on the cannot-see list: pre-push reads every stored commit message, so such a message is still refused at the push. Pass cases, the code reader's four: a cleanup commit (git rm of the leaking file; the path edited out of a file) made with commit -v, with commit.verbose=true, with the editor's template (GIT_EDITOR=true), and the git rm of a file whose name holds the path. Refusal cases: the path on a non-comment line of a message made with the editor; the path on a # line of a message given with -m, and with -F.",
  "3. pre-push checks every changed full path of each pushed commit (for example one git diff-tree --stdin -r -z --root -m --no-renames process fed the commits rev-list gives), so a path held only by a full name is caught even when its object was printed under another name. Cases: the code reader's run 1 and run 2, both refused. State the time for many commits.",
  "4. The cold first read (S-2): with many staged files, a staged file is read from the working tree when it equals its working-tree copy and no attribute changes the bytes between them (filter, working-tree-encoding, ident, and text or eol conversion: ask git for each path's attributes), as next/guard_profile.py does; with more than a few hundred staged files, print one line to stderr first (profile check: reading N files). Timing for 3000 new files, before and after. Add working-tree-encoding and ident to the attribute check of the M1 mend in next/guard_profile.py too.",
  "5. Spellings (S-3): the separator class also takes dot segments, whitespace and line breaks between the parent folder and the name, and the underscore; add UTF-16BE and UTF-32LE forms. Give the name a right boundary: a letter or digit right after it means another user (Users/<name>x is not this profile). A must-refuse case for each spelling; pass cases for the boundary and for %USERPROFILE%, C:\\Users\\Public and ~/Documents.",
  "6. Not only the caller's USERPROFILE or HOME (code reader): build the needles from the environment's value together with the profile folder the system reports for the user running the hook (on Windows its documented API, for example the known folder for the profile through ctypes), so a changed USERPROFILE does not remove the real needle. Case: USERPROFILE set to another folder on the commit, with a staged leak: refused. If the system call fails, keep the environment's value and say so in the trace.",
  "7. install.sh writes the full path of the Python it found into the hooks it installs; the hooks fall back to the PATH search (still skipping the Store stub) if that path is gone. Case: the installed hook runs with no Python on PATH and still checks.",
  "8. install.sh's refusal beside a foreign hook says, per hook, how to merge by hand (call the foreign hook, then ours). No chaining.",
  "9. M-5 (a hand-made or fetched tracking ref) is not mended: it goes on the cannot-see list, in one line.",
  "",
  "Part 2, the guard's candidate (next/guard.sh and next/guard_test.sh). The guard is a seat belt: it stops the assistant's own ordinary commands that would skip or remove git's hooks; git's hooks and the user's review are the real protection. Make exactly these changes, each with its cases; add no others, and do not search for further ways past the guard. What it cannot see goes on the report's cannot-see list, one line per kind.",
  "a. Refuse git merge --no-verify; git send-pack and git http-push; git lfs install and git lfs update with --force or -f.",
  "b. A flag in quotes is still a flag: compare each shell word after its quotes are removed, so \"--no-verify\", '-n' as its own word after commit, and --no-ver\"\"ify are refused, while -m \"the -n flag\" still passes (the message is one word).",
  "c. Refuse a git commit, push or merge that sets GIT_DIR, --git-dir, any variable whose name starts with GIT_CONFIG, USERPROFILE or HOME on its own command; refuse setting an alias, include.path or includeIf with git config or -c (the assistant needs none of them), and git config -e or --edit.",
  "d. Stop refusing plain reads: git config core.hooksPath with no value; grep, cat, diff, ls and head reading a file in .git/hooks even when their pattern is a word such as install, copy, move or touch; cp with a .git/hooks file as the source and a place outside .git/hooks as the destination; ls .git/hooks | tee <a file outside .git/hooks>.",
  "e. G6's call to the checker also fires when git follows ;, &&, ||, |, ( or $( with no space before it.",
  "",
  "The suites, each read to its last line: the git suite with the hooks and with NOHOOKS=1; the guard suite with GUARD=next (every old case must still pass), and its new refusal cases shown failing against the live guard.",
  "",
  RULES,
  "",
  "Report: a section \"Mend (November 17-18)\" at the end of " + FOLDER + "/g6_git_report.md, item by item: what changed, its cases, before and after, timings. Update \"What it cannot see\" with one line each: variables and substitutions in a command; scripts and interpreters that run git; programs that run no hooks; the Write and Edit tools (the guard watches the Bash and PowerShell tools only); hand-made or fetched tracking refs; a --cleanup on the command line (caught at the push); compressed or otherwise encoded content; a clone elsewhere. Then the last lines of each suite. Save the masked outputs to g6_git_mend_output.txt beside it. Write the report early and keep it true as you go.",
  "Return as your final text, in at most 250 words: each item done or not (and why), the suites' last lines, the timings, your doubts.",
].join("\n")

const SHOWN = [
  "Shown to you: the files in " + GITH + " and " + NEXT + "; their state as built and reviewed is commit " + BASE + " (git -C \"" + MAIN + "\" diff " + BASE + " -- .claude/hooks shows the mend, which is not committed); in " + FOLDER + ": g6_git_report.md (its last section is the mend), g6_git_mend_output.txt, review_g6git_breaker.md and review_g6git_code.md (the reviews that asked for the mend).",
  "Do not edit any of these files. Write your review early to your file, and keep it current. Give each finding a file and line, and say how sure you are. Say what you were shown and what you did not read. \"No finding\" is a good result.",
].join("\n")

const RERUN = [
  "You check, by running them, a mend of git hooks for a privacy check in this repository (Git for Windows): they refuse a commit or a push that would carry the path of the user's own profile folder into this public repository. You did not build or mend them.",
  "Your file: " + FOLDER + "/review_g6git_mend_rerun.md. Your temporary folder: D:/Dev/g6git_tmp/rerun.",
  SHOWN,
  "Your question: in your own temporary repositories, with the hooks installed there by install.sh, run again the cases by which the two reviews found the holes on the git side: the replace-ref push (blob and commit message), the commit-msg cleanup commits (-v, commit.verbose, the editor's template, the git rm of a file named with the path), the name-only pushes (the code reader's runs 1 and 2), USERPROFILE set to another folder, the spellings of review_g6git_breaker.md S-3, and the time of a commit of 3000 new files. Is each closed? Then the user's ordinary work: a commit with the editor, commit -v of an ordinary change, a merge, a rebase, a pull, an amend, a tag: is any of it refused or slowed past a second? Run the git suite and the guard suite (GUARD=next) yourself and quote their last lines.",
  RULES,
  "Return as your final text, in at most 200 words: what is closed, what is not, any false refusal, the suites' last lines.",
].join("\n")

const READER = [
  "You read, line by line, a mend of git hooks for a privacy check in this repository, and of a candidate guard rule beside them. You did not build or mend them.",
  "Your file: " + FOLDER + "/review_g6git_mend_code.md. Your temporary folder, if you run anything: D:/Dev/g6git_tmp/reader.",
  SHOWN,
  "Your questions, from the diff: can a pass case pass for the wrong reason? Can dropping comment lines or the scissors part in commit-msg let through a message git will store with the path? Does the working-tree read skip every file whose bytes can differ from the staged blob? Is the right boundary of the name right (the path followed by a separator, a quote, a line end or the end of the file must still be caught)? Does the system's profile folder code fail safely? Is each guard change limited to the commands it lists, and does any of them refuse an ordinary command?",
  RULES,
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
  () => agent(RERUN, { label: 'rerunner', phase: 'Review', model: 'sonnet', effort: 'medium' }),
  () => agent(READER, { label: 'code reader', phase: 'Review', model: 'sonnet', effort: 'medium' }),
])
return { mended, rerun: reviews[0], reader: reviews[1] }
