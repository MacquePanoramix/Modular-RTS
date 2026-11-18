export const meta = {
  name: 'write-check',
  description: 'The guard widened to the tools that write files, for this project only: a candidate check that refuses a write adding the profile folder path to a file of this repository, built with its cases, then one breaker who did not build it',
  phases: [
    { title: 'Build', detail: 'one builder: the candidate in next/ and its suite, each case failed on purpose first' },
    { title: 'Review', model: 'sonnet', detail: 'one breaker: ordinary writes, the edges, speed, failing open' },
  ],
}

const MAIN = "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer"
const HOOKS = MAIN + "/.claude/hooks"
const NEXT = HOOKS + "/next"
const FOLDER = MAIN + "/Docs/Research/2026-11-16_HarnessGuideReview"

const RULES = [
  "Rules:",
  "- Privacy: the path of the user's profile folder and the user's name are never written into any file of the repository, any message or any saved output. Build them at run time from the environment (USERPROFILE); reports write <profile> and <name>. A case may hold the path in memory or pipe it to the check; a case that needs a file holding it puts that file in your temporary folder, outside the repository, and removes it after. Mask every saved output. Before you return, search every file you wrote for the name, built at run time, and say the count.",
  "- Live things stay as they are: .claude/settings.local.json, .claude/settings.json, the live guard (.claude/hooks/guard.sh, guard_profile.py, guard_test.sh), git's hooks (.claude/hooks/git and the repository's .git/hooks). Register no hook and change no setting. Build only new files in " + NEXT + "; import next/guard_profile.py, do not change it. The master installs after the review.",
  "- Do not write into D:/Dev/WG or any other worktree, and do not run Unity. A case may name a path in D:/Dev/WG in the input it gives the check: the check writes nothing.",
  "- Temporary folders only in D:/Dev/wcheck_tmp/<your role>. Make no file names holding a line break. At the end send your folder to the Recycle Bin (PowerShell: Add-Type -AssemblyName Microsoft.VisualBasic, then [Microsoft.VisualBasic.FileIO.FileSystem]::DeleteDirectory(path, 'OnlyErrorDialogs', 'SendToRecycleBin'), in the background); if that is refused, say so and leave it.",
  "- The live guard refuses some commands even when they are only mentioned: staging everything, git checkout -- and git restore, a heredoc holding a doubled backslash, a test run and a commit in one command, --no-verify, core.hooksPath, writing into a .git/hooks folder, and a commit or push carrying the profile path. Write every script and every file holding a backslash with the Write or Edit tool. Do not commit or push: the master does.",
  "- Python is D:/Programs/Python/Python 3.12/python.exe (python3 here is the Store stub); run it with -I.",
  "- What the needles cannot see, by kind, is already listed in " + FOLDER + "/g6_report.md: name it there if it matters, do not catalogue it again.",
  "- Every never, none, all or every in your report quotes its count.",
  "- Usage: before each long stretch, check it with mcp__ccd_session_mgmt__get_usage (load it with ToolSearch \"select:mcp__ccd_session_mgmt__get_usage\"). If the 5-hour window passes 80% or the week passes 95%, stop at the next point where your report is true, and return.",
].join("\n")

const BUILD = [
  "You build a check for this repository (Git for Windows; Claude Code on Windows). Today a guard hook of Claude Code (PreToolUse, matcher Bash|PowerShell, registered in this project's .claude/settings.local.json, so for this project only) refuses a git commit or push that would carry the path of the user's own profile folder (it holds the user's name) into this public repository, and git's own hooks refuse it at every commit and push. The Game Director approved (November 18) widening it, for this project only, to the tools that write files: when the assistant writes or edits a file of this repository, a write that adds the path is refused at once. You did not build the guard.",
  "Read first: " + HOOKS + "/guard.sh (how the guard is run, finds Python, fails open and refuses with exit 2); " + NEXT + "/guard_profile.py, needles() and has() (the forms the path is looked for in: reuse them, write no second list); " + FOLDER + "/g6_report.md (what they see and do not see). The hook gets JSON on stdin with tool_name and tool_input; exit 2 refuses and its stderr goes to the model; exit 0 passes; another exit or a timeout passes with a notice. The input fields are the parameters of your own Write, Edit and NotebookEdit tools (MultiEdit: file_path and edits, each with old_string, new_string, replace_all); say where you read each.",
  "",
  "Build in " + NEXT + ":",
  "1. guard_write.py, and a thin guard_write.sh beside it run the way guard.sh is. For Write, Edit, MultiEdit and NotebookEdit it finds the target file and passes at once unless that file is inside a working tree of this repository (the repository the script's own folder belongs to: the main folder, D:/Dev/WG or another worktree, compared by git's common directory as an absolute path; a new file in a folder that does not exist yet is judged by its nearest existing parent) and is not ignored by git (git check-ignore). Then it judges the file as it would be after the write: for Edit, the current text with the replacement applied as the tool applies it (replace_all as given); for MultiEdit, each edit in order; for Write, the new content; for NotebookEdit, the new source. So a path completed across the edge of an edit is caught. It refuses only what the write adds: the count of matches after is above the count before, so removing the path, or editing elsewhere in a file that already holds it, passes. A refusal: exit 2 and one stderr line naming the tool and the file (masked) and saying to write the path masked as <profile>. Unreadable input, any error, or a git that cannot answer: pass (exit 0) with one stderr line saying the check was skipped. Target paths come with either slash and any case.",
  "2. guard_write_test.sh: its cases. First show the suite catching known-bad checks: a stub that passes everything must fail every refusal case, and one that refuses everything must fail every pass case. Refusals: a Write of a tracked file holding the path in each form needles() looks for (backslash, slash, doubled backslash, %5C, mixed case); a Write of a new file in a new folder of the repository; an Edit whose new_string holds it; an Edit that completes it across the edge of old_string; replace_all; MultiEdit; NotebookEdit; a target in D:/Dev/WG. Passes: clean content; targets outside the repository (the system temporary folder, the assistant's memory folder under the profile's .claude folder, another git repository made in your temporary folder); a gitignored file of the repository (.claude/settings.local.json is one); the name alone inside a longer word (built at run time: a letter on each side of the name); an Edit that removes the path; an unrelated Edit of a file that already holds it; the target path given with the other slash or in another case; malformed JSON; an unknown tool. Timings: a 5 MB Write within 2 seconds, and 20 ordinary small Edits run end to end through guard_write.sh, averaged. The profile path built from USERPROFILE at run time.",
  "3. In your report: the exact entry the master adds to the PreToolUse list of .claude/settings.local.json (matcher Write|Edit|MultiEdit|NotebookEdit, the command run as the guard's is, timeout 10). Say, without building it, whether the guard's G7 rule against writing into a .git/hooks folder (today it sees only shell commands) has the same gap for the tools that write files, and what closing it would take.",
  "Change nothing else. Run your suite, and the guard's suite with GUARD=next to show nothing of it changed, each read to its last line.",
  "",
  RULES,
  "",
  "Report: " + FOLDER + "/write_check_report.md, written early and kept true as you go: what was built, each case and its result against the known-bad checks and then the real one, the timings, the settings entry, G7's gap, and your doubts (for one: an edit that removes the path in one place and adds it in another passes, and git's own check still refuses it at the commit). Save the masked outputs to write_check_output.txt beside it.",
  "Return as your final text, in at most 200 words: what was built, the suites' last lines, the timings, your doubts.",
].join("\n")

const REVIEW = [
  "You check a candidate check for this repository (Git for Windows; Claude Code on Windows): a PreToolUse hook of Claude Code, not yet registered, meant to refuse a Write, Edit, MultiEdit or NotebookEdit that adds the path of the user's own profile folder to a file of this repository, for this project only. You did not build it.",
  "Your file: " + FOLDER + "/review_write_check.md. Your temporary folder: D:/Dev/wcheck_tmp/review.",
  "Shown to you: " + NEXT + "/guard_write.py, guard_write.sh and guard_write_test.sh; " + FOLDER + "/write_check_report.md and write_check_output.txt. Do not edit them. Write your review early to your file and keep it current. Give each finding a file and line and say how sure you are. Say what you were shown and what you did not read. \"No finding\" is a good result.",
  "Your question: fed the JSON Claude Code sends (pipe it in; register nothing), can an ordinary write leave the path in a tracked file of this repository with no refusal: each tool, the edge of an edit, replace_all, a new file in a new folder, a target in D:/Dev/WG, each form needles() looks for? Does it refuse ordinary work: the system temporary folder, the assistant's memory folder, a gitignored file, an edit that removes the path or changes a file that already holds it elsewhere, the name alone inside a word, a target path with the other slash or in another case? Does it fail open on malformed input, a missing target file and a git that cannot be found (a PATH without it)? Time a 5 MB Write and 20 small Edits end to end through guard_write.sh. Is the settings entry in the report right for this Claude Code version, and where did you read that? Run its suite, and the guard's with GUARD=next, yourself and quote their last lines.",
  RULES,
  "Return as your final text, in at most 200 words: what holds, what does not, anything wrongly refused, the timings, the suites' last lines.",
].join("\n")

phase('Build')
const built = await agent(BUILD, { label: 'builder', phase: 'Build', effort: 'high' })
if (!built) {
  log('The builder returned nothing; the review is skipped.')
  return { built: null }
}

phase('Review')
const review = await agent(REVIEW, { label: 'breaker', phase: 'Review', model: 'sonnet', effort: 'medium' })
return { built, review }
