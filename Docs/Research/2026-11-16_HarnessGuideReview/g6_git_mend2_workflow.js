export const meta = {
  name: 'g6-git-hooks-mend2',
  description: 'Second, small mend of git hooks for the profile-path privacy check: the code reader finding on commit-msg and four low items, then one reviewer who did not make it',
  phases: [
    { title: 'Mend', detail: 'one mender: items 1-6' },
    { title: 'Review', model: 'sonnet', detail: 'one reviewer: the diff, the cases rerun, ordinary work' },
  ],
}

const MAIN = "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer"
const GITH = MAIN + "/.claude/hooks/git"
const NEXT = MAIN + "/.claude/hooks/next"
const FOLDER = MAIN + "/Docs/Research/2026-11-16_HarnessGuideReview"
const BASE = (args && args.base) || "HEAD"

const RULES = [
  "Rules:",
  "- Privacy: the path of the user's profile folder and the user's name are never written into any file, message or saved output. Scripts build them at run time from the environment; reports write <profile> and <name>. Mask every saved output. Before you return, search every file you wrote for the name, built at run time, and say the count.",
  "- The first mend's hooks have been installed in the real repository's .git/hooks since November 18 at 00:42 (they act on its worktrees too), and the live guard has been the first mend's candidate since 00:43. Leave both as they are: the master reinstalls after the review. Never write into the real repository's .git/hooks, never set core.hooksPath, never commit or push there.",
  "- G7 is live: the guard refuses a command that skips or changes git's hooks (--no-verify, core.hooksPath, writing into a .git/hooks folder) and a commit, push or merge with GIT_DIR, HOME, USERPROFILE or GIT_CONFIG* set in the same command. Your temporary repositories get their hooks from install.sh (pass the folder) and from the suites. A probe hook in your own temporary repository is approved; put it there with the Write tool. If a command of yours is refused, say so in your report.",
  "- install.sh and uninstall.sh act on the repository given as their argument, and on the current folder's repository when none is given; your working folder is the real repository. So always pass your temporary repository's folder, and never run either with no argument.",
  "- Do not edit the live guard: .claude/hooks/guard.sh, guard_profile.py and guard_test.sh stay as they are. The candidates are in " + NEXT + ".",
  "- Do not touch D:/Dev/WG or any other worktree, and do not run Unity.",
  "- Temporary repositories go only in D:/Dev/g6git_tmp/<your role> (the suite takes G6GIT_DIR). Make no file names holding a line break. At the end, send your own folder to the Recycle Bin (PowerShell: Add-Type -AssemblyName Microsoft.VisualBasic, then [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteDirectory(path, 'OnlyErrorDialogs', 'SendToRecycleBin'); it can take minutes, so run it in the background); if that is refused, say so and leave it.",
  "- The live guard refuses some commands even when they are only mentioned: staging everything, git checkout -- and git restore, a heredoc holding a doubled backslash, a test run and a commit in one command, and a commit or push carrying the profile path. Write every script and every file holding a backslash with the Write or Edit tool.",
  "- Python is D:/Programs/Python/Python 3.12/python.exe (python3 here is the Store stub); run it with -I.",
  "- Usage: before each long run, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch \"select:mcp__ccd_session_mgmt__get_usage\"). If the 5-hour window passes 80% or the week passes 95%, stop at the next point where your report is true, and return.",
].join("\n")

const MEND = [
  "You make a second, small mend of git hooks for a privacy check in this repository (Git for Windows). They refuse a commit or a push that would carry the path of the user's own profile folder (it holds the user's name) into this public repository. You did not build them or make the first mend.",
  "The files: " + GITH + " (profile_check.py, pre-commit, commit-msg, pre-push, install.sh, uninstall.sh, test_git_hooks.sh) and the guard's candidate copies in " + NEXT + " (guard.sh, guard_profile.py, guard_test.sh). As mended and reviewed, they are commit " + BASE + ".",
  "Read first, in " + FOLDER + ": review_g6git_mend_code.md (the code reader's six findings, whole); g6_git_report.md, its last section \"Mend (November 17-18)\"; review_g6git_mend_rerun.md.",
  "",
  "Make exactly these changes, each with its cases in the suites, shown failing (or, for a pass case, wrongly refused) on " + BASE + "'s files and right after; change nothing else:",
  "1. The code reader's finding 1, confirmed by its run: commit-msg guesses from the message text whether git will drop comment lines and cut at a scissors line, but with -m or -F git keeps them (cleanup whitespace). Git tells the hook instead: githooks(5) says every commit hook is run with GIT_EDITOR=: in its environment when the command will not bring up an editor. First check that on this machine's git with a probe hook in a temporary repository that records GIT_EDITOR for commit -m, commit -F, commit -F -, commit -e -m (with GIT_EDITOR=true), commit through the editor, merge --no-edit, merge through the editor, and commit --amend --no-edit; quote what it recorded. If it holds: when GIT_EDITOR is ':', commit-msg reads every line of the message (no cut, no comment line dropped); only when an editor was brought up do today's rules apply; the trace says which. Cases: the reader's probe (a -F file holding git's two template lines and a # line holding the path), the same given with -m, and a scissors line not git's own given with -F: all refused; the first mend's four cleanup commits through the editor still pass. If the probe shows git does not do this, stop item 1, say what it showed, and do the rest.",
  "2. Finding 2: needles() runs at import (profile_check.py:156), outside the fail-open handling, so an error there exits 1 and the hooks refuse with no 'skipped' line. Make an error there fail open like the checker's other errors, with the same visible line. Test it with a harness that imports the module and makes the function raise (no switch or variable in the hooks themselves: a switch would be a way past the check).",
  "3. Finding 3: when the system's profile folder cannot be read (profile_check.py:131-135), say so on stderr in one line on every run, not only in the trace. Test it with the same harness.",
  "4. Finding 4: also look for the short (8.3) form of each profile folder (GetShortPathNameW through ctypes) when it differs from the long form. Test it on a temporary folder with a long name, if this volume makes short names (fsutil 8dot3name query); if it does not, say so and test the function on what it returns.",
  "5. Finding 5: in next/guard_profile.py (lines 442-443) the working-tree shortcut trusts git diff --name-only, which does not see paths marked assume-unchanged or skip-worktree (git ls-files -v). Those paths go to cat-file. Case: over 32 staged files, a staged leak in a path marked assume-unchanged, and one marked skip-worktree, each with a clean working copy: refused.",
  "6. The no-hooks suite's count flake (report, 'Suites'): the case '--amend --no-edit keeps a message that holds the path' made the same commit object within one second. Make that amend always make a new object (for example a later committer date), so that with NOHOOKS=1 all 62 refusal cases go through.",
  "",
  "The suites, each read to its last line: the git suite with the hooks and with NOHOOKS=1; the guard suite with GUARD=next.",
  "",
  RULES,
  "",
  "Report: a section \"Second mend (November 18)\" at the end of " + FOLDER + "/g6_git_report.md, item by item: what changed, its cases, before and after, and the probe's record for item 1. Also add one line to its doubts for the code reader's finding 6 (github.com/users/<name> addresses, and item c's variable rule reading the whole command): both are false refusals, kept. Save the masked outputs to g6_git_mend2_output.txt beside it. Write the report early and keep it true as you go.",
  "Return as your final text, in at most 200 words: each item done or not (and why), the suites' last lines, your doubts.",
].join("\n")

const REVIEW = [
  "You check a second, small mend of git hooks for a privacy check in this repository (Git for Windows): they refuse a commit or a push that would carry the path of the user's own profile folder into this public repository. You did not build or mend them.",
  "Your file: " + FOLDER + "/review_g6git_mend2.md. Your temporary folder: D:/Dev/g6git_tmp/review2.",
  "Shown to you: the files in " + GITH + " and " + NEXT + "; `git -C \"" + MAIN + "\" diff " + BASE + " -- .claude/hooks` shows the second mend (not committed); in " + FOLDER + ": g6_git_report.md (its last section is the second mend), g6_git_mend2_output.txt, review_g6git_mend_code.md (the findings it answers). Do not edit any of these files. Write your review early to your file and keep it current. Give each finding a file and line and say how sure you are. Say what you were shown and what you did not read. \"No finding\" is a good result.",
  "Your question: in your own temporary repositories, with the hooks installed there by install.sh (pass the folder), is each of the six items closed? Rerun the code reader's probe of its finding 1 yourself. Then: does any change open a way for the path to be stored or pushed that was closed before, or refuse ordinary work? Try a commit through the editor, commit -m, commit -F, commit -F - (as GitHub Desktop commits), an amend, a merge, a pull and a push. Run the git suite (with the hooks and with NOHOOKS=1) and the guard suite (GUARD=next) yourself and quote their last lines.",
  RULES,
  "Return as your final text, in at most 200 words: what is closed, what is not, anything opened or wrongly refused, the suites' last lines.",
].join("\n")

phase('Mend')
const mended = await agent(MEND, { label: 'mender', phase: 'Mend', effort: 'high' })
if (!mended) {
  log('The mender returned nothing; the review is skipped.')
  return { mended: null }
}

phase('Review')
const review = await agent(REVIEW, { label: 'reviewer', phase: 'Review', model: 'sonnet', effort: 'medium' })
return { mended, review }
