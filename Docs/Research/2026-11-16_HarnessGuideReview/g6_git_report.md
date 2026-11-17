# G6 in git's own hooks, G7, and the M1 mend (November 17, 2026, evening)

Status: built and tested on temporary repositories (the builder's step). Rung: written and tested on a rig; not
installed in the real repository, not tried from GitHub Desktop, nothing committed or pushed. The saved outputs of
every run are in `g6_git_test_output.txt` beside this page (masked: the profile folder's path is `<profile>`, the user
name `<name>`). The Game Director decides whether to install them.

## What was made

| File | What it is |
|---|---|
| `.claude/hooks/git/profile_check.py` | The check. Self-contained (the needles are built as in `guard_profile.py`). Modes `pre-commit`, `commit-msg <file>`, `pre-push <remote> <url>`. Exit 1 on a hit, 0 otherwise. |
| `.claude/hooks/git/pre-commit`, `commit-msg`, `pre-push` | `#!/bin/sh` hooks: find a Python on PATH (skipping the `WindowsApps` Store stub), run `python -I profile_check.py <mode>`. |
| `.claude/hooks/git/install.sh`, `uninstall.sh` | Put the four files into `$(git rev-parse --git-common-dir)/hooks/` of one repository, and take them out. |
| `.claude/hooks/git/test_git_hooks.sh` | The suite, on temporary repositories only (`D:/Dev/g6git_tmp/builder/suite_*`). `NOHOOKS=1` runs the refusal cases with no hooks. |
| `.claude/hooks/next/guard.sh` | Candidate live guard: the live one plus G7. |
| `.claude/hooks/next/guard_profile.py` | Candidate: the live one plus the M1 mend. |
| `.claude/hooks/next/guard_test.sh` | Candidate suite: the live one plus the M1 cases in G6 and a `-- G7` section; `ONLY=G7` runs G7 alone. A third file to copy over the live one together with the other two. |

The live `.claude/hooks/guard.sh`, `guard_profile.py` and `guard_test.sh` are untouched.

## What each hook reads

The needle is the same as G6's: the profile folder's parent and the name joined by any run of `\`, `/`, `-`, `%5C`,
`%2F` (so `C:\Users\<name>`, `C:/Users/<name>`, `/c/Users/<name>`, `C:\\Users\\<name>` in JSON, `C--Users-<name>`,
URL-encoded), any case, in UTF-8 and in UTF-16LE. The bare name alone never refuses.

- **pre-commit**: the names (paths) and the contents of every staged file against HEAD (or the empty tree for a
  first commit): `git diff --cached --raw -z --no-renames`, then the new blobs through one `git cat-file --batch`,
  16 MB at a time with a 64 KB overlap. It reads the index git hands it, so `commit -a` and `commit <path>` (a
  temporary index, `GIT_INDEX_FILE`) are covered. Deleted files and submodule links are not read.
- **commit-msg**: the whole message file, after the editor or `-m`/`-F`/`--amend` wrote it.
- **pre-push**: for each ref line git gives it (deletes skipped), every object the push would send that the remote
  does not have yet: `git rev-list --objects` from the new tips, not the remote's old oids and not that remote's
  tracking refs (`refs/remotes/<remote>/*`; for a push by URL there are none, so more is read). The objects' paths,
  and every blob, tree, commit and tag object itself: so commit messages, authors, tag messages, notes, and a name
  held only by a tree (a rename) are all read. A commit made by plumbing (no commit hook ran) is caught here.
- The message on a hit names the place (the file, "commit message", or the object and its path), shows the path as
  `<profile>`, says to write `~` or `%USERPROFILE%` instead, gives `git commit --no-verify` / `git push --no-verify`,
  and says "Only the user decides to skip it."
- `WG_PROFILE_TRACE=1` prints one line on a pass: what was read (names, objects, bytes) and "0 hits".
- Its own failures pass with "profile check skipped: <why>" (no Python, the script missing, no USERPROFILE, any
  exception). A failure never blocks a commit.

## What it cannot see

- `--no-verify` / `-n` (git runs no commit hooks; G7 refuses the agents typing it, nothing stops the user).
- Programs that run no hooks or other hooks: libgit2 tools, some IDE integrations, the GitHub website and its API
  (an edit made on github.com never passes here). GitHub Desktop drives its own bundled git, which should run the
  hooks; **not tried here**.
- A commit made by plumbing (`commit-tree`, `update-ref`) until it is pushed.
- `git merge`: the content brought by a merge is not read at merge time (no `pre-merge-commit` hook is installed; the
  merge's message is read by commit-msg). It is read at the push.
- Content git stores compressed or that a tool encodes another way (zip, an FBX's compressed blocks, base64, UTF-7):
  only plain UTF-8, UTF-16LE, UTF-16BE and UTF-32LE text is matched (the last two since the mend).
- `core.hooksPath` set in a config file written with an editor, a `GIT_CONFIG_GLOBAL` file, or `.git/config` edited by
  the Write or Edit tools: G7 reads command texts only. The same for the Write/Edit tools writing into `.git/hooks`.
- `bash .claude/hooks/git/uninstall.sh` (and install.sh) are not refused by G7: uninstalling is a plain command.
- What is already on the remote: a leak pushed before stays there; the check only stops new ones (see "History").
- Added by the mend (November 17-18), one line each:
  - Variables and substitutions in a command (`$X`, `$(...)`, an alias of the shell): the guard reads the text as
    typed, before the shell expands it.
  - Scripts and interpreters that run git (`bash x.sh`, `python -c`, a Makefile): the guard sees only the outer command.
  - Programs that run no hooks (libgit2 tools, IDE integrations, the GitHub website and API): nothing here runs.
  - The Write and Edit tools: the guard watches only the Bash and PowerShell tools, so a hook or a config written
    with them goes past it.
  - Hand-made or fetched tracking refs (`refs/remotes/<remote>/...` set by `update-ref` or a fetch from elsewhere):
    the push trusts them as already on the remote (review item M-5, not mended).
  - A `--cleanup` on the command line: commit-msg cannot see it and reads the message as the file shows; a message git
    would then store differently is read again, as stored, at the push.
  - Compressed or otherwise encoded content (see above).
  - A clone elsewhere: the hooks live in one repository's `.git/hooks`; another clone, or a worktree of another
    clone, has none until install.sh is run for it.
  - Git's editor template in another language (git's messages translated): the template is then not recognised, and
    every comment line is read (stricter, never looser).

## Install and uninstall

    bash .claude/hooks/git/install.sh [repository folder]    # default: the current folder
    bash .claude/hooks/git/uninstall.sh [repository folder]

install.sh writes the four files into the common git folder's `hooks/` (so every worktree of that repository is
covered, and no other repository), with LF line endings and the marker line
`# wg-profile-check: installed by .claude/hooks/git/install.sh; uninstall.sh removes it`. It says per file
installed, updated or unchanged; run twice, it changes nothing. It refuses, changing nothing, when a hook file is
already there without the marker, or when `core.hooksPath` is set. uninstall.sh removes only marked files. Installing
into the real repository is the Game Director's call; this step installed only into temporary repositories.

## The git suite: cases (`test_git_hooks.sh`)

All cases run on fresh temporary repositories with a bare `origin`, `GIT_CONFIG_GLOBAL` an empty file. Each refusal
case checks: git exited non-zero, the refusal message is there, the place is named, how to skip is said, the output
does not show the name, and the repository and its remote are unchanged (the lazy way "print the message, exit 0"
fails here). Each pass case checks exit 0, the trace line was printed (the lazy way "pass without reading" fails here)
and that the commit or push happened.

- **The suite's own judges** (5), each shown a known-bad hook first: one that refuses a clean commit; one that passes
  without reading (no trace); a refusal that shows the name; one that says it refuses but lets the commit through; a
  refusal for another reason.
- **Must refuse, commits** (21): a staged file holding the path in backslash, `cygpath -m`, MSYS, JSON-escaped, upper
  case, URL-encoded and UTF-16LE forms; a binary file; a file named `C--Users-<name>.txt`; a folder `Users/<name>/`;
  `-m`; `-F <file>`; the editor; `--amend --no-edit` keeping such a message; `--amend` with a staged leak; a merge
  commit's message; `git -C <dir>` from elsewhere; a linked worktree; `commit -a`; `commit <path>`; a Store stub first
  on PATH.
- **Must refuse, pushes** (17): a plain push; a new branch; an annotated tag message on an already-pushed commit;
  notes; a wildcard refspec with the leak on a branch not checked out; `--all`; `--mirror`; `remote.origin.push` in the
  config; `push.default=matching`; a push by URL; a force push; a plumbing commit; the path in a folder name only; a
  rename whose content is already on the remote (only the tree holds the name); the path in a commit message only; in
  the author's name; a leak held by a second remote but not by origin.
- **Must refuse**: a 200 MB staged blob with the path at its very end (the chunking).
- **Must pass** (15): a clean commit and push; the bare name alone in a file and the message, committed and pushed;
  `C:\Users\Public`; the words "the profile folder"; `~` and `%USERPROFILE%` as text, committed and pushed; deleting a
  file that held the path; a clean push on top of a leak already on the remote (the lazy way "refuse anything in the
  history" fails here); a delete push of a branch that holds the path; a clean merge; a clean commit in a worktree;
  3000 small files, the commit and the push.
- **Fails open, saying so** (2): no Python on PATH; USERPROFILE and HOME empty.
- **install.sh / uninstall.sh** (5): twice gives all unchanged; LF endings and the marker; refuses beside a foreign
  pre-commit and changes nothing; refuses with core.hooksPath set; uninstall removes the marked files and leaves a
  foreign one.

Last lines, read to the end:

    == hooks installed: 66 passed, 0 failed, 66 cases; 39 refusal cases, 0 of them not refused, 0 went through (committed or pushed)
    == no hooks: 0 passed, 39 failed, 39 cases; 39 refusal cases, 39 of them not refused, 38 went through (committed or pushed)

With no hooks all 39 refusal cases fail. 38 of them went through (the repository or remote changed); the 39th,
`--amend --no-edit`, exited 0 but rewrote the commit within the same second, so its oid came out identical and the
state check saw no change. It is still counted failed (no refusal, exit 0).

## G7 (candidate `next/guard.sh`)

G7 refuses, in the agents' Bash and PowerShell commands (quoted text is not matched as a flag or a path):
- `git commit --no-verify` / `-n` (also inside short-flag clusters such as `-qn`, and abbreviated `--no-veri`), `git
  push --no-verify`;
- setting or unsetting `core.hooksPath`: `git -c`, `--config-env`, `git config` (any scope, `set`, `--unset`; reads
  with `--get` pass), `GIT_CONFIG_COUNT/KEY/VALUE`, `GIT_CONFIG_PARAMETERS`, in Bash and PowerShell;
- removing, moving, copying over, redirecting into, `tee`, `sed -i`, `chmod`, `touch` and the PowerShell cmdlets
  (`Remove-Item`, `Move-Item`, `Copy-Item`, `Set-Content`, `Out-File`, `New-Item` and their aliases) on `.git/hooks`.
- `WG_GUARD_OK=G7` lets one command through, as for the other rules.

Passes kept: `git push -n` / `--dry-run`, `git add -n`, `-m "the -n flag"`, `-mn`, `--amend --no-edit`, reading
`.git/hooks` (`ls`, `cat`, `grep "rm "`, `grep ">"`), `git config --get core.hooksPath`, `install.sh`. The lazy way
"refuse any command naming `-n` or `.git/hooks`" fails the pass cases; "match the text anywhere" fails the quoted
cases.

    GUARD=next, all:  == G6: 30 refusal cases, 0 of them not refused / == G7: 35 refusal cases, 0 of them not refused / == 211 passed, 0 failed, 211 cases
    GUARD=next ONLY=G7:  == G7: 35 refusal cases, 0 of them not refused / == 54 passed, 0 failed, 54 cases
    live guard ONLY=G7:  == G7: 35 refusal cases, 35 of them not refused / == 19 passed, 35 failed, 54 cases

All 154 old cases pass on the candidate (211 = 154 + 3 M1 + 54 G7). Against the live guard all 35 G7 refusals fail.

## M1: the slow commit of many new files

The breaker reported that the live G6 commit check took about 3 ms a blob through `git cat-file --batch` and blamed
the format. Measured here: the **first** open of a freshly written loose object costs about 16 ms on this machine,
whatever reads it and in whatever format (most likely on-access scanning of the new file); a second read is fast.
The breaker's faster alternatives were measured warm. Working-tree files are fast to read.

Mend in `next/guard_profile.py`: with more than 32 staged blobs (`MANY`), a staged file that is the same as its
working-tree copy (not in `git diff --name-only`), a plain file (mode 100644 or 100755) and with no `filter` attribute
is read from the working tree instead of from the object store; the rest still go through cat-file. The trace says
how many were read from the working tree. Cases added in G6: 3000 staged files with the path in the last (must
refuse, and must not fail open by its time limit), 3000 clean files (pass), the same as a commit within the command.

    live guard ONLY=G6:  == G6: 30 refusal cases, 1 of them not refused / == 61 passed, 2 failed, 63 cases
    candidate:           all pass (above)

The live guard fails open at its time limit (about 8.9 s) on the 3000-file leak; the candidate refuses it in about
1.1 s.

## Timings (this machine, temporary repositories unless said)

- `python -I` start: 73 to 91 ms.
- A typical commit (one small file), five each: with the hooks 701 to 834 ms, without 212 to 248 ms (so about 0.5 s
  added, two Python starts and git calls).
- A 200 MB staged blob with the path at its end: refused in 2.1 s.
- 3000 new small files: the commit 32.5 s (an earlier run 53.5 s) with the hooks; the push 1.4 s. The git pre-commit
  hook reads through cat-file as asked, so it pays the cold-object cost of M1. The same working-tree reading would
  cut it; not done here, since the step asked for cat-file.
- The next push of the working branches, read only (`profile_check.py pre-push` fed the ref line git would give,
  nothing pushed): both `claude/worker-showcase` and `claude/wip-round` are level with origin, 0 objects, 122 to
  175 ms. As if the last 10 commits were new: 469 ms and 577 ms, 0 hits. As if the remote were empty (the whole
  history): 16.9 s and 12.0 s, and refused.

## History: where the whole-history hits lie

The whole-history reading found the path in `Assets/_WonderGather/Art/OrdinaryPlace/House.fbx` and `Nature.fbx`
and in `Docs/Research/2026-11-15_NaturalRangesAndTheAnkle/build.md` and `review_mends_physicist.md`, in older
commits. Read only: the tips of `claude/worker-showcase` and `claude/wip-round` and the working tree are clean for all
four (for the FBX files, the patch of commit 16b5636 took the path out). **`origin/main`'s House.fbx and Nature.fbx still hold the path**,
and the older commits holding the four files are on GitHub already. The hooks do not touch what is already pushed:
the next push of either branch passes; only a push to an empty remote (or a mirror to a new one) would be refused.
Taking the path out of `main` is a merge for the Game Director; taking it out of the history would need a history
rewrite, his call alone.

## Not tested

GitHub Desktop (it bundles its own git; whether it finds a Python on its PATH is untested: if not, the hooks pass
saying "skipped"); the `py` launcher as the only Python; a name with non-ASCII letters; Linux or macOS; PowerShell
commands actually run through the guard (the G7 PowerShell cases feed the guard the command text, as the other
rules' cases do); Git LFS pointers and the content behind them; a real push to GitHub; the real repository with the
hooks installed.

## Doubts

1. A commit of thousands of new files takes 30 to 50 s with the git pre-commit hook (M1's cold reads); the working-tree
   reading from the M1 mend would fix it the same way.
2. GitHub Desktop untried: the hooks may run without a Python found and pass, saying so in its log.
3. G7 reads command texts: a Write or Edit to `.git/hooks` or `.git/config`, or `uninstall.sh`, goes past it.
4. `origin/main` and older pushed commits already hold the path; the hooks stop only new leaks.

## Mend (November 17-18)

The mend of the two reviews, made by a separate mender, not installed and not committed (the master does both after
reading it). Rung reached: written and tested on rigs (temporary repositories under `D:/Dev/g6git_tmp/mender`, the
guard fed made-up payloads); not run in the real repository, not seen by the Game Director. Each item's cases are in
`test_git_hooks.sh` (marked "mend N") or `next/guard_test.sh` (under "the mend"); "before" is the same new suite run on
29faa56's files (a copy of them beside it), "after" on the mended files. The outputs, masked, are in
`g6_git_mend_output.txt` beside this report.

### Part 1: git's own hooks

1. **Replace refs.** Every git call of `profile_check.py` sets `GIT_NO_REPLACE_OBJECTS=1` and passes
   `--no-replace-objects`, so the bytes read are the bytes pushed. Cases: a dirty blob, and a dirty commit (its message)
   made with `commit-tree`, each hidden behind a replace ref, pushed. Before: both went through (pushed). After: both refused.
2. **The message as git stores it.** commit-msg cuts at a scissors line only when it is git's own (the line followed
   by git's two explanation lines with the same comment prefix), and drops comment lines only when the file holds git's
   editor template saying lines starting with that prefix "will be ignored", or when `commit.cleanup` is `strip`
   (`core.commentString`, then `core.commentChar`, then `#`; `auto` is read in full). Otherwise every line is read. The
   trace says what was left out ("message: left out as git will, a scissors cut and 10 comment lines"). Pass cases:
   git rm of the leak with `commit -v`; the same with `commit.verbose=true`; the path edited out, `commit -v`; git rm of
   a file whose name holds the path, the editor's template (status comments name it). Refusal cases: the path on a
   non-comment line of an editor message; the path on a `#` line given with `-m`, and with `-F`; a scissors line that
   is not git's own, given with `-F`. Before: the four refusal cases were already refused (29faa56 read every line); the four pass cases failed,
   refused. After: all pass or refuse as they should.
3. **Every full path of each pushed commit.** pre-push gathers the pushed commits from its object listing and reads
   every path each adds or changes through one `git diff-tree --stdin -r -z --root -m --no-renames` process (deletions
   skipped). Cases: the code reader's run 1 (the same content at `A/f.txt` and at `Users/<name>/f.txt`) and run 2 (a
   file added under the path, then moved away), both refused. Before: both went through. After: refused. Time: a push of
   1000 new commits (made with fast-import): 996 ms after (761 ms before, without the diff-tree reading).
4. **Many staged files read from the working tree.** Above 32 staged blobs, a staged file whose working copy is its
   blob is read there: a copy counts only when its git hash (sha1 or sha256, as the repository says), as it is or with
   CRLF made LF, equals the staged oid; paths with a `filter`, `working-tree-encoding` or `ident` attribute, links and
   files over the chunk size go to cat-file untried; the rest stream through cat-file as before. Above 300 files the
   hook first prints "profile check: reading N files" to stderr. Text, eol and `core.autocrlf` are not excluded by
   attribute (this repository has `* text=auto`, so excluding them would have left the mend useless here): the hash
   check makes them exact. Cases (refusals): 3000 files with the leak in the last; 40 files whose staged blob holds the
   path while the working copy does not; a clean filter that puts the path into the staged blob only; 40 CRLF files
   with `text=auto`, the leak in the last. Checks: the 3000-file commit said "reading 3000 files" and "3000 read from
   the working tree"; 40 CRLF files (and `.gitattributes`) read from the working tree, 41. The same three attributes
   are added to the `check-attr` call of the M1 mend in `next/guard_profile.py`. Time, 3000 new small files, the
   commit: before 49.6 s (the first report measured 32.5 s and 53.5 s), after 1.5 s (1.3 s in an earlier run). The
   push of those 3000 files is now the slow one (51.6 s after, 1.3 s before; 55.1 s in an earlier run): the cold-object cost moves to git's own push, which pays it with no hooks at all (a probe, hooks
   absent: the commit 355 ms, `git push --no-verify` 52.2 s). Before, the hook's cat-file paid it at the commit and the
   push found the objects warm (1.4 s in the first report).
5. **Spellings.** The separator between the parent folder and the name may also be an underscore, whitespace (space,
   tab, line break, form feed) or a dot segment (`Users\.\<name>`, `Users/./<name>`); the text is matched in UTF-8,
   UTF-16LE, UTF-16BE and UTF-32LE; and a letter or digit right after the name means another user (a right boundary;
   `<name>.txt` still counts). Refusal cases: a dot segment with `\` and with `/`, an underscore, a space, a line
   break, UTF-16BE, UTF-32LE. Pass cases: `Users/<name>x` and `Users\<name>2`; `~/Documents` and `%USERPROFILE%`;
   `C:\Users\Public` (the older case). Before: six of the seven refusal cases went through; UTF-16BE was
   already refused by accident (UTF-16BE text is UTF-16LE shifted by one byte, so the old UTF-16LE form matched it
   after the first letter); the boundary pass case failed (another user's folder refused).
6. **Needles from the environment and the system.** The checker looks for the environment's profile folder
   (USERPROFILE, else HOME) and the one Windows itself names (`SHGetKnownFolderPath(FOLDERID_Profile)` through
   ctypes, which does not read USERPROFILE; `pwd` elsewhere). When they differ both are looked for, and the trace says
   so; when the system call fails the trace says "the system profile folder was not read (why), the environment's
   alone is used". Cases: USERPROFILE set to another folder on the commit, and USERPROFILE and HOME both empty, each
   with a staged leak: refused. Before: both went through. The failing system call is **not tested** (forcing it would
   need a switch, and a switch would be a way past the check).
7. **The Python found at install.** install.sh writes the full path of the Python it found (not the Store stub, in
   `cygpath -m` form; it refuses to write a path holding a quote, `$`, a backtick or a backslash) into the hooks'
   `wg_python=` line; the hooks use it when it is there and fall back to the PATH search (still skipping
   `WindowsApps`) when it is gone. Cases: no Python on PATH, the installed hook still checks (refused); the installed
   Python gone, the hook searches PATH (refused); the installer check reads the path back from all three hooks.
   Before: no Python on PATH went through (the old hooks failed open); "the installed Python gone" was refused,
   since the old hooks always searched PATH (that case cannot fail before); the installer check failed.
8. **A foreign hook.** install.sh still refuses beside a hook it did not write, and changes nothing, but now says per
   hook how to merge by hand: move it to `<hook>.local`, run install.sh again, and put the given line (call the
   foreign hook, then ours; for pre-push with git's ref lines fed again) under the hook's
   "# (a hook of your own, merged by hand ..." line; a later reinstall writes its own copy back, so the line must be
   put in again. No chaining is done by the script. uninstall.sh names a `<hook>.local` it finds. Case: a foreign
   pre-commit and pre-push; the message checked; the pre-push merged by hand as told; the leak push refused, and the
   foreign pre-push shown to have run first with git's ref lines. Before: the message check and the ran-first check failed (the merged push was still
   refused, by our own pre-push alone).
9. **M-5 (hand-made or fetched tracking refs)**: not mended, one line under "What it cannot see".

### Part 2: the guard candidate (`next/guard.sh`, `next/guard_test.sh`)

- a. Refused (G7): `git merge --no-verify`; `git send-pack` and `git http-push`; `git lfs install` and
  `git lfs update` with `--force` or `-f`. Pass: `git merge --no-ff`, `git lfs install --local`, `git lfs update`.
- b. For commit, push and merge the guard now splits the segment into shell words with the shell's quote removal
  (quotes dropped, a backslash outside single quotes taken as escaping the next character) and compares each word:
  `"--no-verify"`, `'-n'` as its own word after commit, and `--no-ver""ify` are refused; `-m "the -n flag"` and
  `-m "say -n here" -m "and --no-verify in words"` pass.
- c. Refused: a git commit, push or merge that sets `GIT_DIR`, `--git-dir`, `GIT_CONFIG*`, `USERPROFILE` or `HOME` on
  its own command (Bash `X=... git`, PowerShell `$env:X = ...` in the same command); setting an alias, `include.path`
  or `includeIf.*` with `git config` or `-c`; `git config -e` / `--edit`. Pass: `git config --get alias.x`,
  `--list --show-origin`, `--get-regexp include`, `HOMEDIR=x git commit`, `echo $HOME; git commit`, `git -C <dir>`.
- d. No longer refused, plain reads: `git config core.hooksPath` with no value; `grep`, `cat`, `diff`, `ls` and `head`
  reading a file in `.git/hooks`, whatever their pattern; `cp` from a `.git/hooks` file to a place outside it;
  `ls .git/hooks | tee <a file outside>`. Still refused: `cat x > .git/hooks/y`, `cp -t .git/hooks x`,
  `cp .git/hooks/pre-push .git/hooks/pre-push.off`, and the older write cases.
- e. G6's call to the checker also fires when git follows `;`, `&&`, `||`, `|`, `(` or `$(` with no space before it
  (cases with a dirty staged repository: `echo hi;git commit`, `true&&git commit`, `(git commit)`,
  `echo $(git commit)`, `false||git commit`, `echo x|git commit`, each refused by G6).

Before and after, the new guard cases:
- Against 29faa56's candidate guard (`ONLY=G7`, then `ONLY=G6`): every one of the 24 new G7 refusal cases of a to c
  was not refused; the three new d write cases were refused (as they were before); of the d pass cases,
  `git config core.hooksPath`, `grep install .git/hooks/pre-commit` and `cp .git/hooks/pre-push /tmp/backup` were
  refused, the others already passed. `== G7: 62 refusal cases, 24 of them not refused`; `== 73 passed, 27 failed,
  100 cases`. G6: the six e cases not refused, `== G6: 36 refusal cases, 6 of them not refused`.
- Against the live guard (`.claude/hooks/guard.sh`, which has G6 but no G7): `== G7: 62 refusal cases, 62 of them not
  refused`; G6: the six e cases not refused and the 3000-file case (M1, not in the live guard),
  `== G6: 36 refusal cases, 7 of them not refused`.
- Against the mended candidate, the whole suite: `== G6: 36 refusal cases, 0 of them not refused`,
  `== G7: 62 refusal cases, 0 of them not refused`, `== 263 passed, 0 failed, 263 cases`, exit 0.

### Doubts and findings of the mend

1. Whitespace as a separator can refuse prose that writes the parent folder's word and then the name ("users
   <name>"); a false refusal, told plainly, not a leak.
2. A message given with `-F` that holds git's template text and a `#` line holding the path is passed by commit-msg
   (it drops the comment lines as git's editor would) and caught only at the push, if git kept the line.
3. `core.commentChar=auto` is read in full: stricter than git, which may then strip a line commit-msg refused.
4. The right boundary knows ASCII letters and digits only; a name followed by a non-ASCII letter is still matched.
5. The failing system call (item 6) is untested; so is a sha256 repository for item 4's hash check.
6. The merge line of item 8 is lost at each reinstall (said in the message itself).
7. Item c's variable rule reads the whole command: `HOME=/x ls; git commit` and a message that writes `HOME=x` in
   words are refused too (stricter).
8. Item d's `ls .git/hooks | tee hooks_list.txt` already passed on 29faa56's candidate: that case does not fail before.
9. The first run of the mended suite took 15.0 s on the 200 MB blob (2.2 s before): the four encodings and separator
   runs made the pattern about five times slower. A quick first test was added (the name itself, in each encoding,
   in the bytes lowered as `re.I` folds them; the full pattern runs only where one is found): 1.8 s after. On 64 MB of
   plain letters, `has()` took 0.32 s at 29faa56, 1.51 s before the quick test, 0.14 s with it. The needle check of
   80 spellings (20 forms in 4 encodings, refused and passed as they should) gave 0 wrong after the change.
10. The 3000-file push takes about a minute (git's own cold reads, item 4); the hooks no longer add to it much, but
   nothing here makes it fast.

### Suites, their last lines

Each read to its last line; the full outputs, masked, in `g6_git_mend_output.txt`.

- The git suite, hooks installed, the mended files (the last run, after the quick test of finding 9):
  `== hooks installed: 103 passed, 0 failed, 103 cases; 63 refusal cases, 0 of them not refused, 0 went through
  (committed or pushed)`, exit 0. A typical commit: with the hooks 810 939 789 751 748 ms, without 228 215 275 204
  235 ms. An earlier run of the same suite was spoiled by the mender (the suite file was edited while bash was still
  reading it: a syntax error at its end) and is not counted; one before it showed one real failure, the suite's own
  name check catching git's summary line of the status-template case (`delete mode ... <name>.txt`), mended with
  `commit -q`.
- The git suite, `NOHOOKS=1`: `== no hooks: 0 passed, 62 failed, 62 cases; 62 refusal cases, 62 of them not refused,
  61 went through (committed or pushed)`, exit 1. Every refusal case fails without the hooks, as it must. The exit 1
  is the suite's own count rule (went through = refusal cases): the one not counted is the older case
  "--amend --no-edit keeps a message that holds the path", where git exited 0 but, made within the same second, the
  amended commit was the same object as the one before, so the repository did not change. A flake of the count,
  not of the hooks; not mended (outside this step).
- The new git suite on 29faa56's files: `== hooks installed: 80 passed, 23 failed, 103 cases; 63 refusal cases, 13
  of them not refused, 13 went through (committed or pushed)`, exit 1 (the failures are the mend's cases, as listed
  item by item above).
- The guard suite on the mended candidate: `== 263 passed, 0 failed, 263 cases`, exit 0 (G6 36 refusal cases, 0 not
  refused; G7 62, 0 not refused).
- The guard's new cases on 29faa56's candidate and on the live guard: as listed under Part 2.

Not tested by the mend: the real repository with the hooks installed; a real push to GitHub; the system call of item
6 failing; a sha256 repository; a name with non-ASCII letters; Linux or macOS; GitHub Desktop; PowerShell commands
run for real through the guard (the cases feed it the command text).
