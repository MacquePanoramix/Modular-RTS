# G6: the profile folder's path kept out of commits and pushes (November 17, 2026)

The path of the user's profile folder is shown here as `<profile>`, and the name in it as `<name>`.
Rung reached: written and tested on a rig (made-up payloads piped to the guard, temporary repositories). `guard_next.sh`
is **not wired**: the live hook is still `guard.sh`, unchanged, so G6 has not run on a real command of a session.
The masked output of the runs below: `g6_test_output.txt`, beside this page.

## The files

- `.claude/hooks/guard_next.sh`: `guard.sh` with G6 added before its last `exit 0`, and a G6 line in its header.
  When the command matches the guard's own `${G}` followed by `commit` or `push`, and does not hold `WG_GUARD_OK=G6`,
  it pipes the raw payload to `python -I guard_profile.py` (found beside itself by `${0%[/\\]*}`, `.` when `$0` has
  no folder part) under `timeout 8`. Exit 2 with a line refuses as G6 with that line; anything else passes.
  Python here: `python` is `D:/Programs/Python/Python 3.12/python.exe` (3.12.10). If `python` is not found, it tries
  `py`; `python3` is left out on purpose (on this machine it is the Microsoft Store stub).
  `$0` was checked in four forms: slashes (the suite), backslashes only, mixed as `$CLAUDE_PROJECT_DIR` gives it
  (`D:\...\The Wanderer/.claude/hooks/guard_next.sh`), and bare `guard_next.sh` / `./guard_next.sh` from its folder.
- `.claude/hooks/guard_profile.py`, the checker. Changed by this step:
  1. a `$` the shell leaves as written (in single quotes, or escaped with `\` or a PowerShell backtick) is no longer
     filled in from the environment: `git commit -m 'write $USERPROFILE for it'` was refused (shown failing first);
  2. an unquoted heredoc, and a here-string not in single quotes, has its `$NAME` filled in from the environment, as
     the shell would: `git commit -F - <<EOF` with `$USERPROFILE` in it passed (shown failing first);
  3. `WG_G6_TRACE=1` makes it write on stderr what it read (`commit checked: ...`, `push checked: ...`) or what
     failed (the path masked); the guard throws stderr away. The cases use it to show that a pass was read.
  The docstring was brought in line with these, and with the guard's trigger.
- `.claude/hooks/guard_test.sh`: a `-- G6` section, `ONLY=G6` to run it alone, a check on every case that the
  guard's own output does not hold the name, an optional `WANT=` phrase a refusal must hold, and `mask` for any
  stderr it prints.

## What G6 refuses, reads and cannot see

It refuses a git commit or push that would store the path of the profile folder: its last two parts (Users and the
name) joined by any run of `/ \ -` (or `%5C`, `%2F`), in any case, in UTF-8 or UTF-16LE. The bare name is not looked
for. It also refuses, as too large to check, more than `WG_G6_MAXMB` (256 MB unless set) to read, or more than 6 s.

It reads: for a commit, what is staged, what the same command stages (`git add`, `commit -a`, a pathspec), the files'
names, the message (`-m`, `-F` file or stdin, `$(cat <file>)` in `-m`, the old message for `--amend`, `-C`, `-c`),
`--author`, `--trailer`, and the command's heredocs and here-strings; for a push, every object not yet on a remote
(`git rev-list --objects <what is pushed> --not --remotes`). It does not read a message file's own path or a `cd`
into the profile folder: those are not stored.

It cannot see: a commit or push run by a script, a git alias, or a variable the command does not set; a command
substitution other than `$(cat <file>)` in `-m`; PowerShell here-strings as heredocs; other encodings (UTF-16BE,
base64, a path split over lines); the 8.3 short form of the profile folder (`C:\Users\<NAM~1>`); what a git hook
or a commit template adds after it has looked.

## The cases and the suite's last lines

Must refuse (each refused as G6, one stderr line, no name in it; `WANT` checked the reason where named):
a staged file (backslash form); a `-F` message file (MSYS form, the file also named in MSYS form, WANT "the commit
message"); `-m` (WANT same); `cd <repo> && git add <new file> && git commit -F <file>` (WANT "staged by this same
command"); `git -C <repo> commit`; `git commit <tracked, modified, unstaged file> -m x`; `WG_GUARD_OK=G1 git commit -a`
(G1 lets it by, G6 does not); a staged file whose name holds `C--Users-<name>` (WANT "a file name"); the JSON-escaped
form; a small binary file; UTF-16LE content; upper case; URL-encoded; `--amend --no-edit` with HEAD's message
holding the path (WANT "taken from HEAD"); `bash -c "git -C <repo> commit ..."`; `git commit -F -` fed by a heredoc
(WANT "heredoc"); PowerShell `Set-Location <repo>; git commit -m x`; `git push origin main` (WANT "not yet pushed");
plain `git push` with an upstream, the path in a commit message (WANT "commit"); a staged file bigger than
`WG_G6_MAXMB=0.01` (WANT "too large").
Added: the guard run by a backslash path, and by a mixed path; `-m "$HOME/notes"`; `-m "kept in $USERPROFILE"`;
PowerShell `-m "kept in $env:USERPROFILE"`; `-m "$(cat <file>)"`; an unquoted heredoc expanding `$USERPROFILE`;
`git push origin main:feature`; `cd <repo>/sub && git commit -m x`.

Must pass (each a pass marked `tcc` also had the checker, run alone with `WG_G6_TRACE=1`, say it "checked" the
commit or push, with no error):
a clean commit that `cd`-s into a repository under the profile folder's temp, with its `-F` file there (tcc); the
same with the `-F` file named in the `/tmp` form (tcc); a staged file holding only the bare name (tcc), and one
holding `b<name>` (tcc); `WG_GUARD_OK=G6` with bad content; a clean push (tcc); a push of a clean commit when an
older commit, already pushed, holds the path (tcc); `git rm` of a file that held the path, then commit (tcc), and
`git rm <file> && git commit` in one command (tcc); an edit that takes the path out (tcc); a clean commit in repo B
(`git -C`, and `cd <B> &&`) while repo A, the payload's cwd, has bad content staged (tcc); `grep -n "git commit"
<file holding the path>`; `echo` of the path; `git log` and `git status`; `git push --delete` with a bad commit not
yet pushed; a clean PowerShell commit (tcc); a message saying "the profile folder" (tcc); one saying `~` and
`%USERPROFILE%` (tcc); `-m '$USERPROFILE'` in single quotes (tcc); a quoted heredoc holding `$USERPROFILE` (tcc);
PowerShell `-m '$env:USERPROFILE'` in single quotes (tcc).
The guard fails open (bad content staged in each): no python on PATH; no checker beside the guard; the checker
exits 2 with no line; the checker is not Python; the checker hangs for 30 s (the case ended by itself after 9 s).

The suite's own checks, each shown a known-bad case (a fake guard, or a cwd that is no repository), all caught:
a refusal that shows the path; a refusal for another reason than `WANT`; a pass that is refused; a pass whose commit
the checker could not read.

Run 1, the whole suite against `guard_next.sh` (read to the end):
`== 154 passed, 0 failed, 154 cases` (the 94 old cases all pass; G6: 29 refusal cases, 0 not refused; 118 s).
Run 2, the G6 section alone against `guard.sh` (`ONLY=G6`, read to the end):
`== 31 passed, 29 failed, 60 cases`: all 29 refusal cases are let through by the guard without G6; the 31 that pass
are the pass cases, the fail-open cases and the suite's own checks, which do not depend on G6.
The old cases ran with the scratch folder (no repository) as the current folder: their payloads carry no `cwd`.

## The lazy way to pass

- Refuse any commit or push whose command or cwd names the profile folder: caught by the clean commits that `cd`
  into, or name a `-F` file under, the profile folder's temp (every temporary repository lies there), the clean
  PowerShell commit, and repo B by `cd`.
- Look for the bare name: caught by "only the bare name" and `b<name>`.
- Refuse every commit or push, or any command with `git commit` in its text: caught by every must-pass case, among
  them `grep -n "git commit"` and the clean push.
- Refuse when the path is anywhere in the repository's history or working copy: caught by the already-pushed older
  commit, `git rm` then commit, the edit that takes it out, and repo B beside a bad repo A.
- Match `$USERPROFILE` in the command's text: caught by the single-quoted `$USERPROFILE` and `$env:USERPROFILE`
  cases, the quoted heredoc, and `~` / `%USERPROFILE%`.
- Pass everything, or pass by crashing (the guard fails open): caught by the 29 refusal cases (run 2 shows they
  fail on a guard without G6), and for the passes by the `tcc` trace check, itself shown a known-bad case.

## Timings

Median of 15 runs each, whole hook (`bash guard*.sh` with the payload on stdin), this laptop, nothing heavy beside it
except the Unity workflow in D:/Dev/WG that the brief said was running (not checked):

| command | guard.sh | guard_next.sh |
|---|---|---|
| `ls -la` (neither commit nor push) | 210 ms | 218 ms |
| `git status --short` | 207 ms | 195 ms |
| `git commit -m x`, one staged file, temporary repository | 212 ms | 542 ms |
| `git push origin main`, one commit not pushed, temporary repository | 205 ms | 413 ms |

So a command without commit or push gains nothing measurable (the differences are within the spread, 175-324 ms);
a commit gains about 330 ms and a push about 210 ms.
The checker alone, payload cwd the main project (it only reads; `GIT_OPTIONAL_LOCKS=0`; nothing staged there, and the
branch had no object not yet on a remote): `git commit -m x` 241 ms; `git push origin claude/worker-showcase` 201 ms;
`python -I -c pass` alone 87 ms. Once more, `WG_GUARD_OK=G1 git commit -a -m x` there (reads the four modified tracked
files, the scene among them): 297-384 ms, three runs, nothing found. `git status` of the main project after these
reads showed only the files changed by this step and by others; nothing was staged or committed by these runs.

## Not tested

- `guard_next.sh` as the live hook: it is not wired in `.claude/settings.local.json`, so neither Claude Code's own
  payload nor its 10 s limit has been seen with G6. Wiring it is the next step and is Luis's or the main session's.
- Push forms `--all`, `--mirror`, `--tags`, `--follow-tags`, `tag <name>`; commit `-C`/`-c <commit>`, `--author`,
  `--trailer`, `--pathspec-from-file`, `--fixup`, `--squash`, `--git-dir`/`--work-tree`, worktrees, submodules.
- The checker's own 6 s limit and the 256 MB default (only `WG_G6_MAXMB=0.01` and the guard's `timeout 8`, with a
  fake checker, were run). Whether the hung python process is killed by `timeout` or left to finish on its own.
- A large real commit or push; the `py` launcher; a name with spaces, dots or non-ASCII letters; HOME or USERPROFILE
  unset; Linux or macOS; PowerShell here-strings; a commit made through a script or an alias (known blind).
