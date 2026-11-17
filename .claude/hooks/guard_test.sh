#!/bin/bash
# The guard's cases: pipes made-up PreToolUse payloads through guard.sh, one line per case (pass or FAIL), a count.
# A refusal must exit 2 with exactly one stderr line naming the rule and WG_GUARD_OK; a pass must exit 0, silent.
# Exits 1 if any case fails. Run: bash .claude/hooks/guard_test.sh
# GUARD=<path> runs the same cases against another copy of the guard (used on November 16 to show that the cases added
# after the review fail on the guard as it was before its mend).
G="${GUARD:-$(cd "$(dirname "$0")" && pwd)/guard.sh}"
ok=0; bad=0
enc() { local s=$1; s=${s//\\/\\\\}; s=${s//\"/\\\"}; s=${s//$'\n'/\\n}; s=${s//$'\t'/\\t}; printf '%s' "$s"; }
raw() {  # $1 expect (refuse Gn | pass), $2 label, $3 the payload as is
  local out err code tmp; tmp=$(mktemp)
  err=$(printf '%s' "$3" | bash "$G" 2>&1 1>"$tmp"); code=$?; out=$(cat "$tmp"); rm -f "$tmp"
  local lines; lines=$(printf '%s' "$err" | grep -c '')
  if [ "$1" = pass ]; then
    [ $code -eq 0 ] && [ -z "$err" ] && [ -z "$out" ] && r=pass || r=FAIL
  else
    [ $code -eq 2 ] && [ "$lines" = 1 ] && [ -z "$out" ] && [[ $err == "Guard $1:"* ]] && [[ $err == *"WG_GUARD_OK=$1"* ]] && r=pass || r=FAIL
  fi
  [ $r = pass ] && ok=$((ok + 1)) || bad=$((bad + 1))
  printf '%-4s  expect %-6s got exit %s  %s\n' "$r" "$1" "$code" "$2"
  [ $r = FAIL ] && [ -n "$err" ] && echo "      stderr: $err"
}
t() {  # $1 expect, $2 tool, $3 the command
  local label=${3//$'\n'/ \\n }
  raw "$1" "[$2] $label" "{\"session_id\":\"t\",\"hook_event_name\":\"PreToolUse\",\"tool_name\":\"$2\",\"tool_input\":{\"command\":\"$(enc "$3")\",\"description\":\"a test\"}}"
}

echo "-- G1 everything staged at once"
t G1 Bash 'git add -A'
t G1 Bash 'git add --all'
t G1 Bash 'git add .'
t G1 Bash 'git add -u'
t G1 Bash 'git add -A --dry-run'
t G1 Bash 'cd Docs && git add . && git status'
t G1 Bash 'git commit -a -m "x"'
t G1 Bash 'git commit -am "a fix"'
t G1 PowerShell 'git add -A; git status'
t pass Bash 'git add Docs/Handoff.md'
t pass Bash 'git add ./Docs/Outlook.md "Docs/a b.md"'
t pass Bash 'git commit -F msg.txt'
t pass Bash 'git commit --amend -m "stage a file by path"'
t pass Bash 'git -C D:/Dev/WG status'
t pass Bash 'WG_GUARD_OK=G1 git add -A'
echo "-- G2 uncommitted work discarded"
t G2 Bash 'git reset --hard'
t G2 Bash 'git -C D:/Dev/WG reset --hard origin/main'
t G2 Bash 'git clean -fd'
t G2 Bash 'git checkout -- Assets/_WonderGather/Scenes/TheGroup.unity'
t G2 Bash 'git checkout .'
t G2 Bash 'git restore Docs/Outlook.md'
t G2 Bash 'git restore --staged --worktree Docs/Outlook.md'
t G2 Bash 'git stash'
t G2 Bash 'git stash push -m wip'
t G2 Bash 'git stash save wip'
t G2 Bash 'git stash -u'
t G2 PowerShell 'git status; git reset --hard HEAD~1'
t pass Bash 'git reset HEAD Docs/Outlook.md'
t pass Bash 'git reset --soft HEAD~1'
t pass Bash 'git checkout -b claude/new-branch'
t pass Bash 'git checkout main'
t pass Bash 'git restore --staged Docs/Outlook.md'
t pass Bash 'git stash list'
t pass Bash 'git stash show -p stash@{0}'
t pass Bash 'git stash pop'
t pass Bash 'git stash apply'
t pass PowerShell 'git status --short'
t pass Bash 'WG_GUARD_OK=G2 git -C D:/Dev/WG reset --hard origin/main'
echo "-- G3 a commit chained behind a test"
t G3 Bash 'bash D:/Dev/WG/Docs/Technical/AnyBody/tools/unity_tests.sh OwnBody && git commit -F msg.txt'
t G3 Bash 'LIMIT=600 bash unity_tests.sh X; git commit -m "8 of 8"'
t G3 Bash 'bash tools/shoves.sh trace | tail -5
git -C D:/Dev/WG commit -F m.txt'
t pass Bash 'LIMIT=600 bash D:/Dev/WG/Docs/Technical/AnyBody/tools/unity_tests.sh OwnBody'
t pass Bash 'git commit -F msg.txt'
t pass Bash 'grep -n LIMIT unity_tests.sh && git log -1'
t pass Bash 'WG_GUARD_OK=G3 bash unity_tests.sh X && git commit -F m.txt'
echo "-- G4 a heredoc holding a doubled backslash"
t G4 Bash 'cat > fix.py <<"EOF"
path = "D:\\Dev\\WG"
EOF'
t G4 Bash 'python - <<EOF
print("a\\nb")
EOF'
t G4 Bash 'grep -c x <<< "D:\\Dev\\WG"'
t pass Bash 'grep -P "\\d+" <<< "$x"'
t pass Bash 'cat > fix.py <<"EOF"
path = r"D:\Dev\WG"
EOF'
t pass Bash 'printf "a\\b\n"'
t pass Bash 'cat <<EOF
no backslash at all
EOF'
t pass Bash 'WG_GUARD_OK=G4 cat > f.py <<"EOF"
s = "a\\b"
EOF'
echo "-- added after the review (November 16): forms that slipped through, and honest commands that were refused"
t G1 Bash 'git add ./'
t G1 Bash 'git add "."'
t G1 Bash "git add '*'"
t G1 Bash 'git add :/'
t G1 Bash 'git add -vA'
t G1 Bash 'git add -fu Docs'
t G1 Bash 'bash -c "git add -A"'
t G1 Bash 'git --no-pager add -A'
t G1 Bash 'git -C "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer" add -A'
t G1 PowerShell 'git.exe add --all'
t G1 Bash 'git commit -qam "x"'
t pass Bash 'git commit -m "Run with -ownAsPlayed 1, read to the end"'
t pass Bash 'git commit -m "the -liveCap 240 and -ownRate 50 runs"'
t pass Bash 'git -C "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer" add Docs/Handoff.md .claude/hooks/guard.sh'
t pass Bash 'git add -f -- Docs/a.md'
t pass Bash 'git add .gitignore ./Docs/x.md'
t pass Bash 'git --no-pager log -3 --all --oneline'
t G2 Bash 'git --no-pager reset --hard'
t G2 Bash 'git -C "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer" reset --hard'
t G2 Bash 'git reset --merge'
t G2 Bash 'git checkout -f main'
t G2 Bash 'git switch --discard-changes main'
t G2 Bash 'git switch -f main'
t G2 Bash 'sh -c "git clean -fdx"'
t G2 Bash 'git checkout claude/wip-round -- Assets/_WonderGather/Scripts'
t pass Bash 'git clean -n'
t pass Bash 'git clean -nd'
t pass Bash 'git clean --dry-run -d'
t pass Bash 'git switch claude/wip-round'
t pass Bash 'WG_GUARD_OK=G2 git checkout claude/wip-round -- Assets/_WonderGather/Scripts'
t G3 Bash 'T=Docs/Technical/AnyBody/tools/unity_tests.sh; bash $T ank0 X && git commit -F m.txt'
t G3 Bash '"C:/Program Files/Unity/Editor/Unity.exe" -batchmode -runTests -projectPath D:/Dev/WG; git commit -F m.txt'
t pass Bash '"C:/Program Files/Unity/Editor/Unity.exe" -batchmode -runTests -projectPath D:/Dev/WG'
echo "-- broken or foreign payloads (all must pass)"
raw pass 'empty payload' ''
raw pass 'not JSON' 'hello there'
raw pass 'no command key' '{"tool_name":"Bash","tool_input":{}}'
raw pass 'unterminated string' '{"tool_name":"Bash","tool_input":{"command":"git add -A'
raw pass 'command not a string' '{"tool_name":"Bash","tool_input":{"command":42}}'
raw pass 'Write tool payload' '{"tool_name":"Write","tool_input":{"file_path":"x.md","content":"git add -A"}}'
raw pass 'compact JSON, harmless' '{"tool_input":{"command":"ls -la"}}'
raw G1 'compact JSON, spaces around colon' '{"tool_input":{"command" : "git add -A"}}'
echo "== $ok passed, $bad failed, $((ok + bad)) cases"
[ $bad -eq 0 ] || exit 1
exit 0
