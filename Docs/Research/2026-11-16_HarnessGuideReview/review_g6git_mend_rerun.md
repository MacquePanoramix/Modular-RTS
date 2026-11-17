# Rerun of the git-side hole cases against the mended hooks (November 18, 2026)

Reviewer: an agent that did not build or mend them. Own cases in my own temporary repositories under
D:/Dev/g6git_tmp/rerun (hooks installed there by install.sh with the repository's folder as argument; a bare local
remote each; git 2.51.1 command line). Profile path written <profile>, user name <name>; both built at run time,
outputs masked. Nothing installed, committed or pushed in the real repository; the live guard untouched.

Read: the mend section of g6_git_report.md, review_g6git_breaker.md, review_g6git_code.md, install.sh, the three
hook wrappers, the head of test_git_hooks.sh and the case names of its mend cases. Not read: profile_check.py in
full, guard.sh and guard_profile.py (the guard suite was only run), g6_git_mend_output.txt.
Rung reached: run as stated and read to the end. Not tested: GitHub Desktop, an https remote, a real symlink or
submodule, LFS, the failing system call for the profile folder, a sha256 repository, Linux/macOS, the real history's
size. My case script is cases.sh in my folder (before it goes to the Recycle Bin); output cases_out2.txt.

## Closed (each run: leak made with --no-verify first, then the ordinary command with hooks on)

- Replace ref, dirty blob and dirty commit message: both refused at push (control without replace also refused). Sure.
- commit-msg cleanup commits, through the editor template (`-e -m`, GIT_EDITOR=true): git rm of the leak with `-v`;
  `commit.verbose=true`; the path edited out with `-v`; git rm of a file named with the path. All four pass
  (rc 0, trace "left out as git will, a scissors cut and 10 comment lines"). The path typed by the editor script
  above the template: refused; on a non-comment line via -m: refused. Sure.
- Name-only pushes, the code reader's run 1 (A/f.txt and Users/<name>/f.txt, same content) and run 2 (added under the
  path, moved away, pushed together), plus a variant: all refused. Sure.
- USERPROFILE set to another folder; also HOME; both empty; `env -u` of USERPROFILE, HOME and others: all refused. Sure.
- Spellings (S-3), refused: plain, forward slash, upper case, UTF-16LE, `Users\.\<name>`, `Users/./<name>`,
  `Users_<name>`, space, newline, tab, CRLF, UTF-16BE, UTF-32LE, mixed separators, doubled separators, `//`,
  `\\?\` prefix, upper-case word and name, `Users-<name>`. Controls pass: `C:\Users\Public`, `%USERPROFILE%`,
  `~/Documents`, `Users/<name>x`, `Users\<name>2`.
- 3000 new files: commit with the leak in the last file refused in 0.94 s; clean commit 1.4 s (1.5 s in the report;
  breaker measured 49.7 s); with no hooks 0.5 s. The push takes 55.6 s with hooks and 55.4 s with no hooks
  (git's own cold reads): the hooks add nothing there, but the push is not fast. Sure.

## Not closed (breaker's own "cannot see" list; confirmed still passing, not new)

Passed with the leak inside, rc 0: `%255C` double encoding, `\u005c` escape, `&#92;` entity, base64, gzip,
fullwidth backslash, `Users\x\..\<name>`, no-break space between `Users\` and the name. The report's docstring should
say the dot-dot, no-break space and fullwidth forms too; I did not check whether it does. Sure of the runs; each in a
repository of its own. These need no fix if the seat-belt claim is stated.

## False refusals / slowdowns in ordinary work

No false refusal in: commit through an editor, `commit -v` of an ordinary change, amend, lightweight and annotated
tag, push, push --tags, true merge, rebase, pull (ff), pull (merge), pull --rebase, cherry-pick, revert.
Time per command with hooks: 0.13 s (tags), 0.25 s (cherry-pick, revert), 0.47-0.57 s (merge, rebase), 0.73-1.08 s
for commits and pushes. Past a second: `commit -v` through an editor, 1.08 s once (editor 0.95 s). The suite's
own timing (hooks 700-1020 ms, without 210-290 ms per commit) agrees: the hooks add about half a second to every
commit. Finding, minor, certain: ordinary commits are not "under a second" every time; the cost is Python startup
twice (pre-commit, commit-msg) plus git calls. A harness slip of mine (GIT_EDITOR=true on a bare template gives
"empty commit message", git's own abort) is not a hook refusal; mended in my script before the second run.

## Suites (quoted last lines; run by me from the real folder, nothing installed there)

- git suite (G6GIT_DIR in my folder): `== hooks installed: 103 passed, 0 failed, 103 cases; 63 refusal cases, 0 of
  them not refused, 0 went through (committed or pushed)`, exit 0.
- guard suite, GUARD set to next/guard.sh: `== G7: 62 refusal cases, 0 of them not refused`, `== 263 passed, 0 failed,
  263 cases`, exit 0.

## Finding count

No hole found among the cases named in the question; those reopened: none. Findings: the minor timing above; and the
unclosed encodings (known). Sure about all runs.
