#!/bin/bash
# The guard's cases: pipes made-up PreToolUse payloads through guard.sh, one line per case (pass or FAIL), a count.
# A refusal must exit 2 with exactly one stderr line naming the rule and WG_GUARD_OK; a pass must exit 0, silent.
# Exits 1 if any case fails. Run: bash .claude/hooks/guard_test.sh
# GUARD=<path> runs the same cases against another copy of the guard (used on November 16 to show that the cases added
# after the review fail on the guard as it was before its mend).
# ONLY=G6 runs the G6 section alone (used on November 17 to show that its refusal cases fail on the guard without G6);
# ONLY=G7 the G7 section alone (the same evening: its refusal cases fail on the guard without G7).
G="${GUARD:-$(cd "$(dirname "$0")" && pwd)/guard.sh}"
ok=0; bad=0; g6miss=0; g6refuse=0; g7miss=0; g7refuse=0
enc() { local s=$1; s=${s//\\/\\\\}; s=${s//\"/\\\"}; s=${s//$'\n'/\\n}; s=${s//$'\t'/\\t}; printf '%s' "$s"; }
# The name in the profile folder's path (the last part of USERPROFILE) is private: no output of this file shows it.
NAME=${USERPROFILE##*[\\/]}
mask() {
  if [ -z "$NAME" ]; then printf '%s' "$1"; return; fi
  printf '%s' "$1" | sed "s/$(printf '%s' "$NAME" | sed 's/[][\/.*^$]/\\&/g')/<name>/Ig"
}
raw() {  # $1 expect (refuse Gn | pass), $2 label, $3 the payload as is; WANT, if set, a phrase the refusal must hold
  local out err code tmp; tmp=$(mktemp)
  err=$(printf '%s' "$3" | bash "$G" 2>&1 1>"$tmp"); code=$?; out=$(cat "$tmp"); rm -f "$tmp"
  local lines; lines=$(printf '%s' "$err" | grep -c '')
  if [ "$1" = pass ]; then
    [ $code -eq 0 ] && [ -z "$err" ] && [ -z "$out" ] && r=pass || r=FAIL
  else
    [ $code -eq 2 ] && [ "$lines" = 1 ] && [ -z "$out" ] && [[ $err == "Guard $1:"* ]] && [[ $err == *"WG_GUARD_OK=$1"* ]] && r=pass || r=FAIL
    [ -n "$WANT" ] && [[ $err != *"$WANT"* ]] && r=FAIL
  fi
  # A refusal must not show the path it refuses: the name must not be in what the guard writes.
  local both="$err$out"
  if [ -n "$NAME" ] && [[ ${both,,} == *"${NAME,,}"* ]]; then r=FAIL; err="(the guard's own output shows the name) $err"; fi
  [ $r = pass ] && ok=$((ok + 1)) || bad=$((bad + 1))
  if [ "$1" = G6 ]; then g6refuse=$((g6refuse + 1)); [ $r = FAIL ] && g6miss=$((g6miss + 1)); fi
  if [ "$1" = G7 ]; then g7refuse=$((g7refuse + 1)); [ $r = FAIL ] && g7miss=$((g7miss + 1)); fi
  printf '%-4s  expect %-6s got exit %s  %s\n' "$r" "$1" "$code" "$2"
  [ $r = FAIL ] && [ -n "$err" ] && echo "      stderr: $(mask "$err")"
}
t() {  # $1 expect, $2 tool, $3 the command
  local label=${3//$'\n'/ \\n }
  raw "$1" "[$2] $label" "{\"session_id\":\"t\",\"hook_event_name\":\"PreToolUse\",\"tool_name\":\"$2\",\"tool_input\":{\"command\":\"$(enc "$3")\",\"description\":\"a test\"}}"
}

if [ "${ONLY:-all}" = all ]; then
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
fi

if [ "${ONLY:-all}" = all ] || [ "$ONLY" = G6 ]; then
echo "-- G6 the profile folder's path carried by a commit or a push (November 17)"
# Every repository here is a new one in a temporary folder, with a bare temporary one as its remote. The profile
# folder's path is built here from USERPROFILE, in the forms below, and is never written in this file.
PY=$(command -v python 2>/dev/null || command -v py 2>/dev/null)
PYF="${G%[/\\]*}/guard_profile.py"
if [ -z "$NAME" ] || ! command -v cygpath >/dev/null 2>&1 || ! command -v iconv >/dev/null 2>&1; then
  echo "SKIP  the G6 cases: no USERPROFILE, cygpath or iconv here"; bad=$((bad + 1))
else
export GIT_AUTHOR_NAME=t GIT_AUTHOR_EMAIL=t@example.invalid GIT_COMMITTER_NAME=t GIT_COMMITTER_EMAIL=t@example.invalid
export GIT_CONFIG_COUNT=2 GIT_CONFIG_KEY_0=core.autocrlf GIT_CONFIG_VALUE_0=false GIT_CONFIG_KEY_1=init.defaultBranch GIT_CONFIG_VALUE_1=main
PW=$(cygpath -w "$USERPROFILE")                  # as it is: backslashes
PM=$(cygpath -m "$USERPROFILE")                  # forward slashes
PU=$(cygpath -u "$USERPROFILE")                  # the MSYS form, /c/...
PJ=${PW//\\/\\\\}                                # JSON-escaped
PUP=${PW^^}                                      # upper case
PURL=${PW//:/%3A}; PURL=${PURL//\\/%5C}          # URL-encoded
PF=${PM//:/-}; PF=${PF//\//-}                    # as a folder name, C--Users-<name>
T=$(mktemp -d); trap 'rm -rf "$T"' EXIT
TM=$(cygpath -m "$T"); TW=$(cygpath -w "$T")
case "$TM/" in "$PM"/*) echo "      (the temporary repositories lie under the profile folder, as the cases need)" ;;
  *) echo "FAIL  the temporary folder is not under the profile folder: the cases below do not test that"; bad=$((bad + 1)) ;; esac
n=0
repo() {  # a new repository R (and RM, RW: its other forms) with one commit, pushed to its bare remote, upstream set
  n=$((n + 1)); R="$T/r$n"
  { git init -q "$R" && git init -q --bare "$R.git" && printf 'base\n' > "$R/base.txt" && git -C "$R" add base.txt &&
    git -C "$R" commit -q -m base && git -C "$R" remote add origin "$R.git" && git -C "$R" push -q -u origin main; } >/dev/null 2>&1 ||
    echo "      (could not set up repository r$n)"
  RM=$(cygpath -m "$R"); RW=$(cygpath -w "$R")
}
put() { printf '%s\n' "$2" > "$R/$1"; }                       # $1 a file of R, $2 its text
stage() { git -C "$R" add -- "$@" >/dev/null 2>&1; }
store() { git -C "$R" commit -q -m "$1" >/dev/null 2>&1; }  # a commit made here, not through the guard
pay() {  # $1 tool, $2 the payload's cwd, $3 the command
  printf '%s' "{\"session_id\":\"t\",\"hook_event_name\":\"PreToolUse\",\"cwd\":\"$(enc "$2")\",\"tool_name\":\"$1\",\"tool_input\":{\"command\":\"$(enc "$3")\",\"description\":\"a test\"}}"
}
tc() {  # $1 expect, $2 tool, $3 the payload's cwd (Windows form), $4 label, $5 the command
  raw "$1" "[$2] $4" "$(pay "$2" "$3" "$5")"
}
tcc() {  # a pass whose commit or push the checker must have read: run alone with WG_G6_TRACE=1, it says "checked"
  local tr; tr=$(pay "$1" "$2" "$4" | WG_G6_TRACE=1 timeout 8 "$PY" -I "$PYF" 2>&1 >/dev/null)
  tc pass "$1" "$2" "$3" "$4"
  if [ $r = pass ] && [[ $tr != *checked* || $tr == *error* ]]; then
    ok=$((ok - 1)); bad=$((bad + 1)); r=FAIL
    echo "FAIL  the checker did not show that it read this commit or push: $(mask "$tr")"
  fi
}
caught() {  # $1 label, then a case that must come out FAIL: shows that one of this suite's own checks catches it
  local label=$1 o=$ok b=$bad gr=$g6refuse gm=$g6miss; shift
  "$@" >/dev/null 2>&1
  ok=$o; bad=$b; g6refuse=$gr; g6miss=$gm
  if [ $r = FAIL ]; then ok=$((ok + 1)); echo "pass  the suite catches $label"
  else bad=$((bad + 1)); echo "FAIL  the suite does not catch $label"; fi
}

echo "   the suite's own checks, each shown a known-bad case first"
printf 'echo "Guard G6: at %s. WG_GUARD_OK=G6" >&2; exit 2\n' "$PM" > "$T/leaks.sh"
G0=$G; G="$T/leaks.sh"
caught "a refusal that shows the path" tc G6 Bash "$TW" "leak" 'git commit -m x'
printf 'echo "Guard G6: a reason. WG_GUARD_OK=G6" >&2; exit 2\n' > "$T/refuses.sh"; G="$T/refuses.sh"
WANT="too large" caught "a refusal for another reason than the one the case is about" tc G6 Bash "$TW" "reason" 'git commit -m x'
caught "a pass that is refused" tc pass Bash "$TW" "refused" 'git commit -m x'
G=$G0
caught "a pass whose commit the checker could not read (cwd not a repository)" tcc Bash "$TW" "no repo" 'git commit -m x'

echo "   must refuse"
repo; put a.txt "log written to $PW\\AppData\\Local\\x.log"; stage a.txt
tc G6 Bash "$RW" "a staged file holds the path (backslash form)" 'git commit -m "add a"'
repo; put c.txt "clean"; stage c.txt; printf 'notes kept in %s/x\n' "$PU" > "$T/msg_bad.txt"
WANT="the commit message" tc G6 Bash "$RW" "a -F message file holds the path (MSYS form; the file named in MSYS form)" "git commit -F \"$PU${TM#"$PM"}/msg_bad.txt\""
repo; put c.txt "clean"; stage c.txt
WANT="the commit message" tc G6 Bash "$RW" "-m holds the path" "git commit -m \"see $PM/notes\""
repo; put new.txt "$PM/AppData/x"; printf 'clean\n' > "$T/msg_clean.txt"
WANT="staged by this same command" tc G6 Bash "$TW" "cd <repo> && git add <new file> && git commit -F <file>" "cd \"$RM\" && git add new.txt && git commit -F \"$TM/msg_clean.txt\""
repo; put a.txt "$PM"; stage a.txt
tc G6 Bash "$TW" "git -C <repo> commit" "git -C \"$RM\" commit -m x"
repo; put base.txt "$PW"
tc G6 Bash "$RW" "git commit <tracked, modified, unstaged file> -m x" 'git commit base.txt -m x'
repo; put base.txt "$PM"
tc G6 Bash "$RW" "WG_GUARD_OK=G1 git commit -a: G1 lets it by, G6 does not" 'WG_GUARD_OK=G1 git commit -a -m x'
repo; put "$PF.txt" "clean"; stage "$PF.txt"
WANT="a file name" tc G6 Bash "$RW" "a staged file whose name holds the folder-name form" 'git commit -m x'
repo; put j.json "{\"p\": \"$PJ\"}"; stage j.json
tc G6 Bash "$RW" "the JSON-escaped form in a staged file" 'git commit -m x'
repo; printf '\000\001\002%s\000\377\376' "$PM" > "$R/b.dat"; stage b.dat
tc G6 Bash "$RW" "a small binary file with the path inside" 'git commit -m x'
repo; printf 'at %s\r\n' "$PW" | iconv -f utf-8 -t utf-16le > "$R/u.txt"; stage u.txt
tc G6 Bash "$RW" "UTF-16LE content" 'git commit -m x'
repo; put up.txt "$PUP"; stage up.txt
tc G6 Bash "$RW" "upper case" 'git commit -m x'
repo; put url.txt "file:///$PURL/x"; stage url.txt
tc G6 Bash "$RW" "URL-encoded" 'git commit -m x'
repo; put c.txt "clean"; stage c.txt; store "written in $PM"
WANT="taken from HEAD" tc G6 Bash "$RW" "--amend --no-edit, HEAD's message holds the path" 'git commit --amend --no-edit'
repo; put a.txt "$PM"; stage a.txt
tc G6 Bash "$TW" "bash -c \"git -C <repo> commit ...\"" "bash -c \"git -C '$RM' commit -m x\""
repo; put c.txt "clean"; stage c.txt
WANT="heredoc" tc G6 Bash "$RW" "git commit -F - fed by a heredoc" "git commit -F - <<'EOF'
written in $PM
EOF"
repo; put a.txt "$PM"; stage a.txt
tc G6 PowerShell "$TW" "Set-Location <repo>; git commit -m x" "Set-Location '$RM'; git commit -m x"
repo; put a.txt "$PM"; stage a.txt; store "add a"
WANT="not yet pushed" tc G6 Bash "$RW" "git push <remote> <branch>, a commit carrying the path" 'git push origin main'
repo; put c.txt "clean"; stage c.txt; store "see $PM"
WANT="commit" tc G6 Bash "$RW" "plain git push with an upstream set (the path in a message)" 'git push'
repo; head -c 60000 /dev/zero | tr '\0' 'a' > "$R/big.txt"; stage big.txt
export WG_G6_MAXMB=0.01
WANT="too large" tc G6 Bash "$RW" "a staged file bigger than WG_G6_MAXMB=0.01: too large to check" 'git commit -m x'
unset WG_G6_MAXMB
echo "   must refuse (added: other forms)"
repo; put a.txt "$PM"; stage a.txt; G0=$G
G=$(cygpath -w "$G0")
tc G6 Bash "$RW" "the guard run by a path with backslashes only" 'git commit -m x'
G="$(cygpath -w "$(dirname "$G0")")/$(basename "$G0")"
tc G6 Bash "$RW" "the guard run by a mixed path, as from CLAUDE_PROJECT_DIR" 'git commit -m x'
G=$G0
repo; put c.txt "clean"; stage c.txt
tc G6 Bash "$RW" "-m \"\$HOME/notes\": the shell would put the path in" 'git commit -m "$HOME/notes"'
tc G6 Bash "$RW" "-m \"\$USERPROFILE\" in double quotes" 'git commit -m "kept in $USERPROFILE"'
tc G6 PowerShell "$RW" "PowerShell -m \"\$env:USERPROFILE\"" 'git commit -m "kept in $env:USERPROFILE"'
printf 'from %s\n' "$PM" > "$T/msg_cat.txt"
tc G6 Bash "$RW" "-m \"\$(cat <file>)\", the file holding the path" "git commit -m \"\$(cat '$TM/msg_cat.txt')\""
tc G6 Bash "$RW" "an unquoted heredoc that expands \$USERPROFILE" 'git commit -F - <<EOF
kept in $USERPROFILE
EOF'
repo; put a.txt "$PM"; stage a.txt; store "add a"
tc G6 Bash "$RW" "git push origin main:feature (a refspec)" 'git push origin main:feature'
repo; mkdir -p "$R/sub"; put sub/a.txt "$PM"; stage sub/a.txt
tc G6 Bash "$TW" "cd <repo>/sub && git commit -m x" "cd \"$RM/sub\" && git commit -m x"

echo "   M1 (November 17, evening): 3000 staged files, read within the time limit"
many() {  # 3000 small staged files in R; $1, if given, the text of the last one
  mkdir -p "$R/many"; for i in $(seq 1 3000); do printf 'small file %d\n' "$i" > "$R/many/f$i.txt"; done
  [ -n "$1" ] && printf '%s\n' "$1" > "$R/many/f3000.txt"; stage many
}
repo; many "kept in $PM"; t0=$(date +%s%N)
WANT="would carry" tc G6 Bash "$RW" "3000 staged files, the path in the last one: refused for it, not as too large" 'git commit -m x'
echo "      (that case took $(( ($(date +%s%N) - t0) / 1000000 )) ms)"
repo; many; t0=$(date +%s%N)
tc pass Bash "$RW" "3000 clean staged files: passes, not refused as too large" 'git commit -m x'
echo "      (that case took $(( ($(date +%s%N) - t0) / 1000000 )) ms)"
tcc Bash "$RW" "the same, the checker shown to have read it" 'git commit -m x'

echo "   must pass"
repo; put c.txt "clean"; stage c.txt; printf 'clean\n' > "$T/msg_ok.txt"
tcc Bash "$TW" "a clean commit; cd into, and -F file under, the profile folder's temp" "cd \"$RM\" && git commit -F \"$TM/msg_ok.txt\""
tcc Bash "$RW" "a clean commit; -F file named in the /tmp form" "git commit -F \"$T/msg_ok.txt\""
repo; put n.txt "$NAME"; stage n.txt
tcc Bash "$RW" "a staged file holding only the bare name" 'git commit -m x'
repo; put n.txt "b$NAME"; stage n.txt
tcc Bash "$RW" "a staged file holding b<name>" 'git commit -m x'
repo; put a.txt "$PM"; stage a.txt
tc pass Bash "$RW" "WG_GUARD_OK=G6 with bad content" 'WG_GUARD_OK=G6 git commit -m x'
repo; put c.txt "clean"; stage c.txt; store "clean"
tcc Bash "$RW" "a clean push" 'git push origin main'
repo; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" push -q >/dev/null 2>&1; put c.txt "clean"; stage c.txt; store "clean"
tcc Bash "$RW" "a push of a clean commit; an older commit, already pushed, holds the path" 'git push'
repo; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" rm -q a.txt >/dev/null 2>&1
tcc Bash "$RW" "git rm of a file that held the path, then commit" 'git commit -m "remove a"'
repo; put a.txt "$PM"; stage a.txt; store "add a"
tcc Bash "$RW" "git rm <that file> && git commit, in one command" 'git rm a.txt && git commit -m "remove a"'
repo; put a.txt "$PM"; stage a.txt; store "add a"; put a.txt "clean"; stage a.txt
tcc Bash "$RW" "an edit that takes the path out of a file" 'git commit -m "take it out"'
repo; A=$R; AW=$RW; AM=$RM; put a.txt "$PM"; stage a.txt
repo; put c.txt "clean"; stage c.txt
tcc Bash "$AW" "a clean commit in repo B (git -C) while repo A, the cwd, has bad content staged" "git -C \"$RM\" commit -m x"
tcc Bash "$AW" "the same with cd <B> &&" "cd \"$RM\" && git commit -m x"
tc pass Bash "$AW" "grep -n \"git commit\" <file holding the path>" "grep -n \"git commit\" \"$AM/a.txt\""
tc pass Bash "$AW" "echo of the path, no git" "echo \"$PM\""
tc pass Bash "$AW" "git log and git status in repo A" 'git log --oneline -3; git status'
repo; put a.txt "$PM"; stage a.txt; store "add a"
tc pass Bash "$RW" "git push --delete, with a bad commit not yet pushed" 'git push --delete origin feature'
repo; put c.txt "clean"; stage c.txt
tcc PowerShell "$TW" "a clean PowerShell commit" "Set-Location '$RM'; git commit -m x"
tcc Bash "$RW" "a message that says \"the profile folder\" in words" 'git commit -m "keep the profile folder out of it"'
tcc Bash "$RW" "a message that says ~ and %USERPROFILE%" 'git commit -m "write ~ or %USERPROFILE% for it"'
tcc Bash "$RW" "-m '\$USERPROFILE' in single quotes: the shell leaves it as words" "git commit -m 'write \$USERPROFILE for it'"
tcc Bash "$RW" "a quoted heredoc holding \$USERPROFILE as words" "git commit -F - <<'EOF'
write \$USERPROFILE for it
EOF"
tcc PowerShell "$RW" "PowerShell -m '\$env:USERPROFILE' in single quotes" "git commit -m 'write \$env:USERPROFILE for it'"
echo "   must pass: the guard fails open (bad content staged in each)"
repo; put a.txt "$PM"; stage a.txt
PATH=/usr/bin:/bin tc pass Bash "$RW" "no python on PATH" 'git commit -m x'
mkdir -p "$T/fo"; cp "$G" "$T/fo/guard.sh"; G0=$G; G="$T/fo/guard.sh"
tc pass Bash "$RW" "no guard_profile.py beside the guard" 'git commit -m x'
printf 'import sys\nsys.exit(2)\n' > "$T/fo/guard_profile.py"
tc pass Bash "$RW" "the checker exits 2 with no line" 'git commit -m x'
printf 'this is not python\n' > "$T/fo/guard_profile.py"
tc pass Bash "$RW" "the checker fails (not Python)" 'git commit -m x'
printf 'import time\ntime.sleep(30)\n' > "$T/fo/guard_profile.py"
t0=$(date +%s)
tc pass Bash "$RW" "the checker hangs (30 s): the guard's timeout 8 ends it" 'git commit -m x'
echo "      (that case took $(( $(date +%s) - t0 )) s)"
G=$G0
echo "== G6: $g6refuse refusal cases, $g6miss of them not refused"
fi
fi

if [ "${ONLY:-all}" = all ] || [ "$ONLY" = G7 ]; then
echo "-- G7 git's own hooks skipped, moved or changed (November 17, evening)"
t G7 Bash 'git commit --no-verify -m x'
t G7 Bash 'git commit -n -m x'
t G7 Bash 'git commit -nm "x"'
t G7 Bash 'git commit -qn -F msg.txt'
t G7 Bash 'git commit --no-veri -m x'
t G7 Bash 'git -C D:/Dev/WG commit --amend --no-verify --no-edit'
t G7 PowerShell 'git commit -m "a fix" --no-verify'
t G7 Bash 'git push --no-verify'
t G7 Bash 'git push --no-verify origin claude/wip-round'
t G7 PowerShell 'git push origin main --no-verify'
t G7 Bash 'git -c core.hooksPath=/dev/null commit -m x'
t G7 Bash 'git -c CORE.HOOKSPATH=x push'
t G7 Bash 'git --config-env=core.hooksPath=H commit -m x'
t G7 Bash 'git config core.hooksPath D:/elsewhere'
t G7 Bash 'git config --global core.hooksPath ~/hooks'
t G7 Bash 'git config --unset core.hooksPath'
t G7 Bash 'git config set core.hooksPath x'
t G7 Bash 'GIT_CONFIG_COUNT=1 GIT_CONFIG_KEY_0=core.hooksPath GIT_CONFIG_VALUE_0=/x git commit -m x'
t G7 Bash 'export GIT_CONFIG_PARAMETERS="'"'"'core.hooksPath=x'"'"'"; git push'
t G7 PowerShell '$env:GIT_CONFIG_KEY_0 = "core.hooksPath"; $env:GIT_CONFIG_COUNT = 1; git push'
t G7 Bash 'rm .git/hooks/pre-push'
t G7 Bash 'rm -f "D:/Programs/Unity/Projects/Wonder Gather/The Wanderer/.git/hooks/pre-commit"'
t G7 Bash 'mv .git/hooks/pre-commit /tmp/x'
t G7 Bash 'cp x.sh .git/hooks/pre-commit'
t G7 Bash 'echo "exit 0" > .git/hooks/pre-commit'
t G7 Bash 'printf x >> .GIT/Hooks/commit-msg'
t G7 Bash 'echo x | tee .git/hooks/pre-push'
t G7 Bash "sed -i 's/exit 1/exit 0/' .git/hooks/profile_check.py"
t G7 Bash 'chmod -x .git/hooks/pre-push'
t G7 PowerShell 'Remove-Item .git\hooks\pre-commit'
t G7 PowerShell 'Move-Item .git\hooks\pre-push .git\hooks\pre-push.off'
t G7 PowerShell 'Copy-Item x.sh .git\hooks\pre-commit'
t G7 PowerShell 'Set-Content -Path .git\hooks\pre-commit -Value "exit 0"'
t G7 PowerShell '"exit 0" | Out-File .git\hooks\pre-commit'
t G7 PowerShell 'New-Item -ItemType File .git\hooks\pre-commit -Force'
t pass Bash 'git push -n'
t pass Bash 'git push --dry-run origin main'
t pass Bash 'git add -n Docs/x.md'
t pass Bash 'git commit -m "never use --no-verify here"'
t pass Bash 'git commit -m "the -n flag"'
t pass Bash 'git commit -mn'
t pass Bash 'git commit --amend --no-edit'
t pass Bash 'git log --oneline -3'
t pass Bash 'ls .git/hooks'
t pass Bash 'ls -la .git/hooks/ 2>/dev/null'
t pass Bash 'cat .git/hooks/pre-commit'
t pass Bash 'grep -n "rm " .git/hooks/pre-commit'
t pass Bash 'grep -c ">" .git/hooks/pre-push'
t pass Bash 'git config --get core.hooksPath'
t pass Bash 'echo "core.hooksPath stays unset"'
t pass Bash 'bash .claude/hooks/git/install.sh'
t pass PowerShell 'Get-ChildItem .git\hooks'
t pass PowerShell 'Get-Content .git\hooks\pre-commit'
t pass Bash 'WG_GUARD_OK=G7 git commit --no-verify -m x'
echo "== G7: $g7refuse refusal cases, $g7miss of them not refused"
fi
echo "== $ok passed, $bad failed, $((ok + bad)) cases"
[ $bad -eq 0 ] || exit 1
exit 0
