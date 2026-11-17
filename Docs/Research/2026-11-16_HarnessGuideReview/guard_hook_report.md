# The guard hook (November 16, 2026)

Answers `audit_process_failures.md` rows 12, 19, 30, 31 (rules that were sentences and were broken again).
**This report describes the guard as first built** (78 lines, 61 cases). What a reviewer then found, and the mend, are
in the last section; `guard_test_output.txt` beside this report is now the mended guard's run (94 cases).
Files: `.claude/hooks/guard.sh`, `.claude/hooks/guard_test.sh`, and the verbatim test output beside this
report, `guard_test_output.txt` (the first run is condensed below). Wired in `.claude/settings.local.json`
as `hooks.PreToolUse`, matcher `Bash|PowerShell`, `bash "$CLAUDE_PROJECT_DIR/.claude/hooks/guard.sh"`, timeout 10;
`autoCompactWindow` and the two outlook hooks are unchanged; the file parses as JSON (checked with node).
`.claude/settings.json` was not touched. There is no jq here, so the script decodes the JSON string by hand.

## The rules as built
Each refusal is exit 2 with one stderr line: `Guard Gn: <reason>. <what to do instead> If this use is deliberate and
approved, put WG_GUARD_OK=Gn in the command.` The override word passes that one rule only.
- **G1** everything staged at once: `git add` with `-A`, `--all`, `.`, `-u`, `--update`; `git commit` with `-a`,
  `--all` or a short-flag group holding `a` (`-am`). Say: stage by path. (`--amend`, `-F file`, `-m` pass.)
- **G2** uncommitted work discarded: `git reset --hard`; any `git clean`; `git checkout --` or `git checkout .`;
  `git restore` without `--staged`/`-S` (or with `--worktree`/`-W`); `git stash` bare, `push`, `save` or a flag
  (`-u`). Passes: stash `list show pop apply drop clear branch create store`, `reset --soft`, `checkout -b`.
- **G3** a commit chained behind a test: the whole command holds `unity_tests.sh` or `shoves.sh` and `git commit`.
  Say: read the result first, commit in a separate command.
- **G4** a heredoc or here-string (`<<`, `<<<`) whose text after the marker holds `\\`. Say: Write or Edit tool.
- **G5 left out**: `D:/Dev/WG/Docs/Technical/AnyBody/tools/unity_tests.sh` line 28 sets `LIMIT="${LIMIT:-10800}"`
  (three hours) and stops the run with a STOPPED line past it, so a run without LIMIT= already has a time limit.
G1 and G2 are checked per simple command (split at `; & | ( )` and line ends); `git -C <path>` / `-c` are allowed
before the subcommand. It fails open: empty, non-JSON, unterminated or non-string payloads, and any error, pass.

## guard_test.sh output (condensed; every line said `pass`; verbatim in guard_test_output.txt)
```
-- G1  refused (exit 2): git add -A | git add --all | git add . | git add -u | git add -A --dry-run |
        cd Docs && git add . && git status | git commit -a -m "x" | git commit -am "a fix" | [PowerShell] git add -A; git status
   passed (exit 0): git add Docs/Handoff.md | git add ./Docs/Outlook.md "Docs/a b.md" | git commit -F msg.txt |
        git commit --amend -m "stage a file by path" | git -C D:/Dev/WG status | WG_GUARD_OK=G1 git add -A
-- G2  refused: git reset --hard | git -C D:/Dev/WG reset --hard origin/main | git clean -fd | git checkout -- Assets/.../TheGroup.unity |
        git checkout . | git restore Docs/Outlook.md | git restore --staged --worktree Docs/Outlook.md | git stash |
        git stash push -m wip | git stash save wip | git stash -u | [PowerShell] git status; git reset --hard HEAD~1
   passed: git reset HEAD Docs/Outlook.md | git reset --soft HEAD~1 | git checkout -b claude/new-branch | git checkout main |
        git restore --staged Docs/Outlook.md | git stash list | git stash show -p stash@{0} | git stash pop | git stash apply |
        [PowerShell] git status --short | WG_GUARD_OK=G2 git -C D:/Dev/WG reset --hard origin/main
-- G3  refused: bash .../unity_tests.sh OwnBody && git commit -F msg.txt | LIMIT=600 bash unity_tests.sh X; git commit -m "8 of 8" |
        bash tools/shoves.sh trace | tail -5 <newline> git -C D:/Dev/WG commit -F m.txt
   passed: LIMIT=600 bash .../unity_tests.sh OwnBody | git commit -F msg.txt | grep -n LIMIT unity_tests.sh && git log -1 |
        WG_GUARD_OK=G3 bash unity_tests.sh X && git commit -F m.txt
-- G4  refused: cat > fix.py <<"EOF" path = "D:\\Dev\\WG" EOF | python - <<EOF print("a\\nb") EOF | grep -c x <<< "D:\\Dev\\WG"
   passed: grep -P "\\d+" <<< "$x" (the doubled backslash is before the marker) | cat > fix.py <<"EOF" path = r"D:\Dev\WG" EOF |
        printf "a\\b\n" (no heredoc) | cat <<EOF no backslash at all EOF | WG_GUARD_OK=G4 cat > f.py <<"EOF" s = "a\\b" EOF
-- broken or foreign payloads, passed: empty | not JSON | no command key | unterminated string | command 42 |
        Write tool payload (file_path/content) | compact JSON ls -la;  refused G1: "command" : "git add -A" (spaces round colon)
== 61 passed, 0 failed, 61 cases          (exit 0)
```
A first run gave 59/60: one G4 case of mine was wrong (its doubled backslash sat before `<<<`, outside the text); the
case was corrected and kept as a near miss, and a true here-string case added. The guard was not changed for it.

## Live proof (this session)
The settings reloaded in the running session without a restart. After the edit:
- Bash `git add -A --dry-run` was refused: `PreToolUse:Bash hook error: [command hook]: Guard G1: this stages everything at once ...`
- PowerShell `git add -A --dry-run` was refused the same way (`PreToolUse:PowerShell hook error ... Guard G1`).
- Bash `git stash list` passed (exit 0). Nothing was staged by any of these.

## What the guard does not cover
- Commands inside scripts or files it runs (`bash fix.sh` holding `git add -A`), aliases, and `git` called from
  Python, node or Unity; only the command text given to the Bash and PowerShell tools is read.
- Other tools: Write, Edit, NotebookEdit, MCP tools, sub-agents' own tools only if their harness loads these settings.
- Text that only mentions a command (an `echo "git add -A"`, a heredoc body or commit message quoting one) is
  refused like the real thing; the override word or a Write-tool file is the way past. `git -C "path with spaces"`
  before the subcommand was not recognised at first (mended: the last section). G4 cannot tell a heredoc that really
  halves (`<<EOF`) from one that would not; it refuses both. Settings in another worktree (D:/Dev/WG) are separate.
- It is a seat belt, not the rule: passing it does not make a command right.
## How to switch it off
Remove the `PreToolUse` entry from `.claude/settings.local.json`, or set `WG_GUARD=off` in the environment
(for example the `env` block of the settings); for one deliberate call, put `WG_GUARD_OK=Gn` in the command.

## After the review (November 16, late evening): what was found, and the mend
A reviewer that did not build the guard (`review_of_the_changes.md`, part 3) found forms that passed though they were
in scope, and honest commands that were refused. 33 cases were added to `guard_test.sh` from its findings. Run against
the guard as first built, 23 of them failed (`guard_test_before_mend.txt`: 71 of 94 passed), so the cases can fail.
Run against the mended guard, all pass (`guard_test_output.txt`: 94 of 94, exit 0). The guard is now 101 lines.
- **Now refused too:** `git add` with `./`, `:/`, `*`, a quoted `.`, or a flag group holding `A` or `u` (`-vA`); a
  git call inside `bash -c "..."` or right after a quote mark; `git.exe`; git's own options before the subcommand
  (`--no-pager`, `-C "a path with spaces"`); `git reset --merge`; `git checkout -f`; `git switch -f` and
  `--discard-changes`; and, for G3, a direct Unity run (`-batchmode`, `-runTests`) chained with a commit.
- **No longer refused:** a commit message that names a flag (`-m "Run with -ownAsPlayed 1 ..."`; flags are now read
  outside quoted text); `git clean -n` and `git clean --dry-run`.
- **Still refused, on purpose:** `git checkout <branch> -- <path>`, this project's way to bring work over from the
  work branch, because the same form discards uncommitted changes to those paths. See with `git status` that the
  paths are clean, then put `WG_GUARD_OK=G2` in the command.
- **Still not covered:** a test runner named through a variable or not known by name (`dotnet test`); a message
  written before its result was read, when the commit is a separate command; and everything in "What the guard does
  not cover" above, except its `-C` line. `git stash push --dry-run` is still refused though harmless.
- **Live, after the mend (the same session, 22:34):** Bash `git -C "<the main project, a path with spaces>" add -A
  --dry-run` was refused (`PreToolUse:Bash hook error: [command hook]: Guard G1: this stages everything at once ...`);
  as first built the guard let that form pass. `git -C "<the same path>" clean -n` ran (exit 0; it only listed what
  it would remove); as first built the guard refused it. Nothing was staged or removed by either.
