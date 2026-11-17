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
- Content git stores compressed or that a tool encodes another way (zip, an FBX's compressed blocks, UTF-16BE,
  base64): only plain UTF-8 and UTF-16LE text is matched.
- `core.hooksPath` set in a config file written with an editor, a `GIT_CONFIG_GLOBAL` file, or `.git/config` edited by
  the Write or Edit tools: G7 reads command texts only. The same for the Write/Edit tools writing into `.git/hooks`.
- `bash .claude/hooks/git/uninstall.sh` (and install.sh) are not refused by G7: uninstalling is a plain command.
- What is already on the remote: a leak pushed before stays there; the check only stops new ones (see "History").

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
