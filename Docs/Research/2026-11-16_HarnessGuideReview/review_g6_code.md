# G6 code review (the checker and its wiring)

Reviewer: a fresh agent that did not build G6. Status: final for what was run; 15 of 32 mutation runs were read, the rest not run (usage), said below.
The profile path is shown as `<profile>`, the user's name as `<name>`.

## What I was shown and what I did not read

Read in full: `.claude/hooks/guard_next.sh`, `.claude/hooks/guard_profile.py`, the whole of the "-- G6" section of `guard_test.sh` plus its helpers, and the tail of `g6_test_output.txt`.
Not read: `g6_report.md` (I judged the code, not the claims), `guard.sh` (live, not touched), the settings that wire the hook, Unity files.
How I tested: payloads piped to `guard_next.sh` and to `guard_profile.py` (with `WG_G6_TRACE=1`), in temporary repositories under the temp folder with bare temporary remotes; nothing was staged, reset, cleaned or committed in the main project (the checker was run against it read-only, for timing). Python here is a Windows Python 3.12 (`os.name` is `nt`), so the MSYS-python branch of `native()` is not exercised anywhere and I did not test it.
Mutation runs: copies of the checker with one feature taken out, run through `ONLY=G6 GUARD=<copy>/guard_next.sh bash guard_test.sh` (copies live in the scratchpad; the repo files were not edited).

## Must-fix

### 1. The push check does not look at path names, so folders `Users/<name>/...` go out
- `guard_profile.py` `check_push`, the loop `for oid, (kind, data) in cat(...)`: only `has(data)` is tested. For a tree, `data` holds one name per entry, never `Users` and `<name>` together; `where[oid]` (the path `rev-list --objects` prints, `Users/<name>` for the subtree) is never tested. The checker's own header says a push reads "the names in its folders"; that holds for a flat name such as `C--Users-<name>.txt`, not for nested folders.
- Reproduced: temporary repo, commit made outside the guard that adds `Users/<name>/sub/x.txt` (clean content), then payload `git push origin main` through `guard_next.sh`: exit 0, no output. The same files as `<name>_dir/x.txt` with the path in the content: refused, as it should be.
- The commit side does catch it (the diff's full names), so this is a hole only for commits made without the guard (a script, Unity tooling, a commit from before G6, another tool) and for what is already committed and unpushed now. Push is the backstop, so it matters.
- Fix: also run `has(where[oid])` for every object, and for every blob/tree test the full `where` path (it is the full path). Sure: high (reproduced).

### 2. The wiring does not call the checker for forms of `git commit` and `git push` that are ordinary shell
`guard_next.sh`, the pattern `G`: `git` must follow the start, a space or tab, a quote, a backtick or a `/`, and only `-C`, `-c`, `--word`, `--word=value`, `-P` may stand before the subcommand. Through `guard_next.sh`, with a bad file staged (all exit 0, nothing refused):
- `cd <repo>;git commit -m x` and `true&&git commit -m x` (no space before `git`);
- `echo $(git commit -m x)` and `(git commit -m x)`;
- `git --git-dir .git commit -m x` and `git --work-tree <repo> commit -m x` (a value after a space);
- `& 'C:\...\git.exe' commit -m x` in PowerShell (a backslash or a closing quote around `git.exe`); the same path with forward slashes IS caught.
Python itself would parse most of these (its `git_call` knows `--git-dir X`), so the hole is only the gate. G1 to G4 share the same pattern, so they have the same hole; that is no excuse for G6, which is the one rule that guards a privacy leak.
- Fix: the character before `git` may also be `;`, `&`, `|`, `(`, `{`, `\`, `!`; options may be `--word value` for `--git-dir`, `--work-tree`, `--namespace`, `--exec-path`; or simply drop the gate for G6 and let the checker (0.3 s) decide on any command holding `commit` or `push` as a word. Sure: high (reproduced).

### 3. A PowerShell here-string with an apostrophe before the path loses the path
`tokens()` reads `@'` as a single-quote string. A here-string `@' ... '@` that holds an apostrophe ends at it, and the rest of the lines become other "commands".
- Reproduced (PowerShell tool payload): `git commit -m @'` newline `it's a note` newline `see <profile>` newline `'@` -> exit 0. The same without the apostrophe -> refused.
- Commit messages in this project say "Luis's" and the like; a here-string is how a PowerShell agent writes a multi-line message. This is an ordinary commit. Same family: `@"..."@` with a `"` inside.
- Fix: in PowerShell mode, recognise `@'`...`'@` and `@"`..."`@` (the closing marker on its own line) as one word before the quote rules. Sure: high (reproduced for `@'`; `@"` not run).

## Should-fix

### 4. A hit is lost when the checker raises something other than the two expected errors
`main()`: `except Exception: return 0` discards `hits` collected earlier. Reproduced: a bad staged file, command `git commit -m x` newline `git -C "a<NUL>b" commit -m y` (JSON `\u0000`): the first commit alone -> exit 2; with the second command -> `error: ValueError('embedded null character')`, exit 0. The trigger is contrived, but any bug in later parsing (lone surrogates in a path make `open()` raise `UnicodeEncodeError`, not `OSError`) opens the same door. Fix: on a generic exception, if `hits` is not empty, report them (exit 2). Sure: high.

### 5. `<<` inside quoted text swallows the lines after it
`heredocs()`: `HEREDOC` matches `<<WORD` anywhere in a line, also in a quoted message or an arithmetic `<<`, then drops every following line until a line equal to WORD (none: to the end). Reproduced: `echo 'a <<EOF b'` newline `git commit -m x`, bad file staged -> exit 0. Needs a multi-line command with the text before the commit; rare, cheap to close (only start a heredoc when the `<<` is outside quotes, i.e. after tokenising). Sure: high.

### 6. Git variables set by the command are not honoured
`GIT_INDEX_FILE=<other index> git commit -m x` (a pattern used for temporary indexes): the checker reads the real index, finds nothing, exit 0 while the other index holds the path. Reproduced for `GIT_INDEX_FILE`. `GIT_DIR`, `GIT_WORK_TREE` are the same (the checker's `var` map keeps them but `git()` never passes them). Fix: pass the command's `GIT_*` assignments in the child environment, or refuse when they are present. Sure: high for INDEX_FILE.

### 7. `inner()` only looks at the first word
`timeout 60 bash -c "git commit -m x"` -> exit 0 with a bad file staged (the gate matches the quote before `git`, the checker sees a wrapper it does not unpack). Same for `env`, `nohup`, `sudo`, `xargs`... Fix: look for a shell word anywhere in the segment, as `git_call` does for `git`. Sure: high.

### 8. `$'...'` (ANSI-C quoting) is read as plain text
`git commit -m $'see C:\x5cUsers\x5c<name>'` -> exit 0: the shell would store the real path, the checker reads `\x5c`. Contrived for an honest author; listed because the tokenizer claims to know quoting. Sure: high.

### 9. `--not --remotes` trusts every remote-tracking ref, not the one pushed to
Reproduced: a commit with the path was pushed to a second remote (`other`), fetched; `git push origin main` -> exit 0, though origin does not have it. If a private or local second remote exists, a push to the public one passes. Fix: when the remote is named, exclude only `refs/remotes/<remote>/*`. Sure: high; likelihood in this project: low (one remote).

### 10. The refusal can show the user's name
Reproduced: a pushed file at `<name>_dir/x.txt` (the content holds the path): the one-line refusal repeats `<name>_dir/x.txt`; only the full path pattern (`TEXT`) is masked. The suite checks that its refusals carry no name, but none of its file names carry it alone. The refusal goes into the agent's transcript, from where it reaches documents. Fix: mask the name alone in `say()` and `trace()`. Sure: high.

### 11. The patterns for the path
- Do match (tested, exit 2): `C:\x`, `C:/x`, `/c/x` (MSYS), `C:\\x` (JSON-escaped), `%5C`/`%2F` forms, upper case, `C--Users-<name>` folder form, UTF-16LE.
- Do not match: `Users/./<name>`, the JSON escape `\u005c`, UTF-16BE, gzip/zip content (nothing can). These are not ordinary.
- What else they match: no boundary on either side. `Users/<name>s` and `end-users-<name>` both refuse a commit. Innocent text will rarely look like that, so a one-line note is enough, but the `-` separator means any "users-<name>" in a sentence refuses.
- An 8.3 alias does not apply while the name is under nine characters (I did not test a long name).

### 12. Time and size
- Measured here: regex speed 100 to 200 MB/s (a 100 MB file with no hit: 0.5 to 1.2 s). Through the guard: an 80 MB file 1.8 s, a 200 MB random binary 3.3 s, 200 MB of text with the path at the end 2.8 s, refused correctly. The checker on the main project: 0.27 to 0.44 s for a commit and a push. The guard's 8 s `timeout` plus the guard's own 0.2 s stays under the hook's 10 s on every case I ran.
- `has()` is not under the deadline: the six seconds are checked in `spend()` and `git()` only. A read that ends at 5.9 s followed by a 2 s scan reaches the guard's `timeout 8`, which kills the checker: that is a pass (exit 124), not the refusal the header promises ("more than six seconds is refused"). Not reproduced (the largest read here took 3.3 s in all). Fix: check the deadline between objects in `check_push`/`check_commit`, or shorten SECONDS to 4.
- The first push to an empty remote has no remote-tracking refs, so `rev-list --objects` lists every object of the history; this Unity project would be refused as "too large" (256 MB). Not tested here (no such repository); say so in the refusal or add a note, so an innocent push is not read as a leak.

### 13. By design, listed so nobody counts it as found
`WG_GUARD_OK=G6` anywhere in the command text (a commit message that mentions it too) passes. Scripts, aliases, `$(...)` other than `cat` in `-m`, `git cherry-pick/merge/rebase/am` (commits made by other commands are only met at the push) are the stated holes.

## What I found nothing wrong with (tested)
- `diff --raw -z` parsing with `--no-renames`: modified, added, deleted, a rename done with `git mv` (reported as delete + add), and submodule lines are read correctly; a staged file holding the path is found in each form; an edit that takes the path out passes.
- `cat-file --batch` parsing (header, size, trailing newline), a missing object, a binary with the path inside.
- `rev-list --objects` for `git push`, `-u`, `-fu`, `--force-with-lease=ref:sha`, `--set-upstream`, `-o`, `--no-verify`, `--porcelain`, a URL remote, `--repo=`, `HEAD`, `main:feature`, `--tags`, `--follow-tags`, an annotated tag whose message holds the path, a commit object with the path in its author: all refused when they carry it; `--delete` and a clean push pass.
- Messages: `-m`, `--message=`, a second `-m`, `--trailer`, `-F -` fed by `echo` or `printf`, `-F file`, `$(cat <<'EOF' ... EOF)`, the `'\''` idiom, `-C <worktree>`, `cd` forms, `git add <path> && git commit`, `-i`, `-o`, `--only`, `add -f` of an ignored file, `add` of an ignored file without `-f` (passes, correctly).
- Over-refusal of ordinary commands: none found (`echo git commit`, `grep "git commit" file`, clean commits and pushes pass; clean commit in repo B while repo A, the cwd, holds the path passes).
- The main project today: two tracked files in HEAD hold the path (`House.fbx`, `Nature.fbx`, the exports Luis chose to redo); any commit that stages them is refused, which is the intent.

## The cases in guard_test.sh (G6 section)
Method: 32 copies of the checker, each with one feature taken out (or made lazy), each run against the G6 section (`ONLY=G6 GUARD=<copy>/guard_next.sh bash guard_test.sh`, 60 cases). I stopped the runner after 15 copies because the plan's 5-hour window reached 63 %: the 15 below were read to the end, the other 17 were not run. The baseline copy (no change) was among the unread ones; the suite's own output in `g6_test_output.txt` is the baseline I rely on.

Read, with the cases that caught the change (a case that catches it is the only one unless said):
- UTF-16LE form removed: caught by "UTF-16LE content" (1 case).
- URL-encoded form removed: caught by "URL-encoded" (1).
- `-` as separator removed, and file names not checked, and the name shown in the refusal: all three are caught by the SAME single case, "a staged file whose name holds the folder-name form". One case carries three features: if it were weakened, three things would go unseen.
- Push: commit objects ignored: caught by "plain git push with an upstream set (the path in a message)" (1). 
- `-F` message files ignored: 1 case. `-m` ignored: 5 cases. Heredocs of the command ignored: 2. Unquoted heredoc not expanded: 1. `bash -c` unpacking off: 1. `cd` off: 3 refusals and 2 passes, as it should be.
- NOT caught by any case: (a) the push check of trees ignored (`kind == 'tree'`): the case that holds a path in a folder name for a push does not exist, which is finding 1; (b) the six-second deadline removed (`spend()`); (c) the timeout of git calls removed. (b) and (c) have no case at all: nothing in the suite makes a read or a git call slow. A case would be a 0.01 MB cap WITH a slow stand-in for git (a `git` script earlier on PATH that sleeps), run with `SECONDS` lowered through an environment variable; today SECONDS is fixed in the file.
- Not run (reading only): the lazy variants. By reading: a lazy G6 that refuses any text holding the word "Users" is NOT caught, since no passing case holds "Users" in a message or a file (the passing cases hold the bare name, `b<name>`, "the profile folder", `~`, `%USERPROFILE%`, `$USERPROFILE` as words). A lazy G6 that looks for the bare name is caught by the two bare-name passes. A lazy G6 that takes every object of the history instead of `--not --remotes` is caught by "a push of a clean commit; an older commit, already pushed, holds the path". A lazy G6 that always reads the cwd's repository instead of `git -C` is caught by the two repo-B passes. A lazy G6 that refuses `git push --delete` is caught by its pass case.
- The `tcc` helper (a pass counts only if the trace says "checked") is a good guard against the commonest lazy pass (the checker erroring out and failing open); it is not used on the `tc pass` cases (the grep, the echo, the `--delete`, `WG_GUARD_OK=G6`), where the checker may not run at all; for the last one that is the point.
- The suite's known-bad case for its own refusals ("a refusal that shows the path", "a pass that is refused", "a wrong reason") is a sound idea and I did not find it failing; I did not mutate it.

Cases that do not exist and would have caught the must-fix findings: `;git commit`, `&&git commit`, `git --git-dir X commit`, a PowerShell here-string with an apostrophe, a pushed folder `Users/<name>/...`, `timeout 60 bash -c "git commit"`, `GIT_INDEX_FILE=`.

## Housekeeping
Everything I wrote lives in the scratchpad (scripts, mutated copies of the checker, logs) except this file. This file was searched at the end for the user's name with a pattern built at run time from USERPROFILE.
