# Code review: git's own G6 hooks, install/uninstall, G7 and the M1 mend

Reviewer's file. Shown: `.claude/hooks/git/*` (profile_check.py, pre-commit, commit-msg, pre-push, install.sh,
uninstall.sh, test_git_hooks.sh), `.claude/hooks/next/guard.sh` and `guard_profile.py` (read as a diff against the live
ones), the G7 section of `next/guard_test.sh`, `g6_git_report.md`. Not read line by line: the whole of
`guard_profile.py` (only the M1 diff), `g6_git_test_output.txt`. Not run: the builder's suites, GitHub Desktop, Unity.
All experiments ran in temporary repositories with a bare remote each, hooks installed by install.sh
(git 2.51.1.windows.1, python on PATH is found by the hooks: the trace line appeared). Name searched for in every
file I read or wrote: not found (search built at run time).

## Must-fix

1. **An ordinary cleanup commit is refused when git's message file carries the diff or the status comments.**
   commit-msg reads the whole `COMMIT_EDITMSG`, and git calls the hook before it strips comments and the `-v` diff.
   Runs (temporary repo, `GIT_EDITOR=true`, hooks installed, each leak committed with `--no-verify` first):
   - `git rm l.txt` (file held the path), then `git commit -v`: pre-commit 0 hits, commit-msg refuses ("the commit message").
   - same with `git config commit.verbose true` and plain `git commit`: refused.
   - edit the leaking file to remove the path, `git add`, `git commit -v`: refused (the diff's `-` line).
   - `git rm C--Users-<name>.txt` (a file whose NAME is the path), plain `git commit` with the editor: refused (the
     status comment `deleted: ...` holds the name).
   So the very commit that "takes it out" is refused whenever it goes through an editor template; `-m`/`-F` pass (the
   builder's delete case uses `-m`). Sure: 4 of 4 runs. Fix: in commit-msg read only what git will keep: drop lines
   starting with the comment character (core.commentChar/commentString, default `#`) and everything from the scissors
   line `# ------------------------ >8 ------------------------` on; or read the message with
   `git stripspace --strip-comments` after cutting at the scissors line. Add the four cases above to the suite
   (pass cases, with the trace line) and a refusal case "the path typed into the editor's text still refuses".

## Should-fix

2. **pre-push can miss a path that lives only in names (a hole in the backstop).** `rev-list --objects` prints each
   object once, under the first name it meets. A tree or blob that is the same object as an earlier-sorted or
   newer one loses its second name, and the needle spans two tree levels (`Users` / `<name>`) so the tree objects read
   raw do not hold it either. Runs: (a) `A/f.txt` and `Users/<name>/f.txt` with identical content, committed with
   `--no-verify`, `git push origin main`: exit 0, "3 names, 5 objects, 0 hits". (b) commit 1 adds `Users/<name>/f.txt`,
   commit 2 deletes it and adds `zzz/f.txt` with the same content, pushed together: exit 0, 0 hits. Content holding
   the path is caught either way; this is the name-only leak, for commits that no pre-commit saw (merge, cherry-pick,
   plumbing, another clone). Sure of the runs; the need for identical content makes it narrow. Fix: for the commits
   being pushed (`rev-list <tips> --not <nots>`) read every changed path per commit, e.g. `git log --raw -z
   --no-renames -m --first-parent`-free `git diff-tree -r -z --root -m` per commit, and run `has()` on each full path.
   Add the two runs above as refusal cases. The suite's "folder name only" and "rename" cases pass only because the
   name is the first one printed.
3. **The check keys on the environment of the process that runs it.** `USERPROFILE=C:/Users/zzzz git commit -m x`
   with a staged file holding the real path: exit 0, "0 hits" (run). G7 does not look at USERPROFILE/HOME. Beyond the
   intended `--no-verify`, but as cheap for an agent to type. Fix: besides USERPROFILE/HOME also take the path from
   `git config`-free sources not set by the caller (`SystemDrive` + `\Users\` + the account name from
   `getpass.getuser()` or `os.path.expanduser('~')` computed with the env stripped, and `ctypes` SHGetFolderPath),
   and have G7 refuse commands that set USERPROFILE/HOME for a git command. Sure of the run.
4. **G7 text patterns miss ordinary-looking variants** (each run through `next/guard.sh`, exit 0 = passed):
   - quoted flag: `git commit -m x "--no-verify"`, `git commit -m x '-n'`, `git push "--no-verify"` (flags are read
     outside quotes, and the quotes are removed by the shell);
   - quoted or backticked command: `"git" commit -n -m x`; a PowerShell continuation `git commit \`` + newline + `-n -m x`;
   - alias: `git -c alias.c='commit --no-verify' c -m x` (G6, G1 and G7 all miss it);
   - hooks folder reached in two steps or by another spelling: `cd .git/hooks && rm pre-commit`, `cd .git && rm
     hooks/pre-commit`, `rm .git/h*/pre-commit`, `rm .git/./hooks/pre-commit`, `rm ".git"/hooks/pre-commit`;
   - `>|` redirection: `echo x >| .git/hooks/pre-commit` (the segment split at `|` cuts it);
   - verbs not in WRITERS: `dd of=`, `find .git/hooks -delete`, `rsync`, `robocopy`, `perl -pi`, `python -c` ;
   - `git config core.hooksPath get` sets the value "get" and passes (the `get` word test).
   All verified as passes except that I did not run them as real commands. The guard calls itself a seat belt and
   these are deliberate evasions, so should-fix. Cheap closes: strip all quote marks from the flag text before the -n
   test (accepting a message like `"-n"` costs only a false refusal), treat any command containing `.git` and `hooks`
   within one word-run and a write verb as refused, and add `>|`, `dd`, `find`, `-delete`, `rsync`, `robocopy`,
   `perl`, `python` to the verbs.
5. **G7 false refusals by mention.** The verb list holds plain words (`install`, `move`, `copy`, `touch`, `del`,
   `ren`, `rm`). `cp .git/hooks/pre-commit /tmp/x` (a read copy) is refused (run). A commit message heredoc whose body
   line says "install .git/hooks ..." is refused the same way, since body lines are separate segments (reasoned from the
   code, not run). Documented limitation; list it in the G7 header. Also `git commit -m "x" -- -n.txt` is refused (run).
6. **commit-msg and the verbose template also affect the "only the user decides" text**: nothing wrong, but the
   refusal tells the person to use `--no-verify`, which for Q1 is the only way out until item 1 is fixed.
7. **M1 mend (`worktree_copies`), read not run.** It trusts that "not in `git diff --name-only`" means the working-tree
   file equals the staged blob. That fails for attributes that change the bytes other than `filter`:
   `working-tree-encoding` (a UTF-16BE or other encoding in the working tree hides a UTF-8 staged leak from the
   working-tree read) and `ident`. The repo's `.gitattributes` has `* text=auto` and per-extension eol only, so today
   it is moot. Also `o not in out` reads only the first path per oid, which is right. Suggest adding
   `working-tree-encoding` and `ident` to the check-attr list. Not tested by me: the 3000-file runs.
8. **Hooks pass silently when no Python is on git's PATH** (GitHub Desktop untried, as the report says): the warning
   goes to stderr only. Not a code error; if the master installs, run one commit from GitHub Desktop and read its log.

## Checked, no finding

- pre-commit: `diff --cached --raw -z --no-renames` parse (meta, path pairs; deletes, gitlinks, first commit via the
  empty tree, `commit -a`/`commit <path>` temporary index) ran clean; a path with a newline and a space committed and
  pushed with a trace line and no crash; a gitlink next to a file committed.
- pre-push: ref lines, delete pushes (builder), zero oids, missing remote objects (`missing` lines skipped), annotated
  and blob tags (a clean annotated tag and a tag of a blob pushed with trace lines), a merge commit and a cherry-pick of
  a hook-less leak commit were both refused at the push (content leak). cat-file feeding thread: no deadlock seen.
- Needles: forms listed in the docstring match (builder's cases); `--no-v`/`--no-ve`/`--no-ver` are ambiguous in git
  and git itself refuses them (run), so the guard's `--no-veri` floor is right.
- install.sh/uninstall.sh: a copy of all six files with CRLF line endings installed LF hooks (0 CR in
  the installed pre-commit) and uninstalled cleanly; a foreign hook and `core.hooksPath` refusals are in the suite.
  `.gitattributes` is `* text=auto` with autocrlf=true here, so checkouts will have CRLF scripts; they still ran.
  Not tried: a dangling-symlink hook file, a read-only hooks folder, an install that fails halfway (it could leave
  pre-commit installed without profile_check.py: the hook then passes with the "not beside" warning).
- Private path: no file I read or wrote holds the name or the path (run-time search).

## The tests

- test_git_hooks.sh: each refusal case checks exit, message, place, skip text, unchanged state, and the NOHOOKS run
  makes all 39 fail, so each case needs the hooks. Cases that would still refuse with the feature removed: none found
  among the content cases; the name-only push cases (item 2) pass for the wrong reason (first-name order). Missing pass
  cases that a lazy check would survive: the editor/`-v` cleanup commits (item 1), a name containing the path as a
  prefix of another word (over-match: `Users/<name>x` is read as the path), and a push to a second remote with
  an old leak in its history.
- guard_test.sh G7: the quoted `-m "the -n flag"`, `-mn`, `--amend --no-edit`, `ls`/`cat`/`grep` cases do catch the
  lazy "any -n or any .git/hooks" check. No case for the bypasses in item 4; each one would pass today.

## Cleanup

My temporary repositories were in `D:/Dev/g6git_tmp/code` (moved to the Recycle Bin at the end; see the final report
for whether that worked).
