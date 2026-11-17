#!/bin/bash
# The cases of git's own side of G6 (pre-commit, commit-msg, pre-push and profile_check.py, beside this file).
# Each case runs real git commands in a new temporary repository under $G6GIT_DIR (default D:/Dev/g6git_tmp/builder),
# with a bare temporary repository as its remote and the hooks installed by install.sh. Nothing here touches another
# repository. The profile folder's path is built from USERPROFILE as the cases run, and no output shows the name.
#   bash .claude/hooks/git/test_git_hooks.sh            all cases, hooks installed (exit 1 if any case fails)
#   NOHOOKS=1 bash .claude/hooks/git/test_git_hooks.sh  the refusal cases with no hooks installed: each must come out
#                                                       FAIL there, which shows that the cases test the hooks
# A refusal counts when: git exits non-zero; the output holds the check's own message ("profile check: this ...
# would carry"), the place the case is about, and how to skip (--no-verify, only the user decides); the repository
# and its remote are as they were; and the name is not in the output.
# A pass counts when: git exits 0; the output holds the trace line ("0 hits") of each hook that must have run, and no
# "skipped" warning; the commit or the push really happened (the repository or its remote changed); no name shown.
# Both judges are first shown known-bad hooks they must catch (section "the suite's own judges").
HERE=$(cd "$(dirname "$0")" && pwd)
BASE=${G6GIT_DIR:-D:/Dev/g6git_tmp/builder}
NOHOOKS=${NOHOOKS:-0}
NAME=${USERPROFILE##*[\\/]}
if [ -z "$NAME" ] || ! command -v cygpath >/dev/null 2>&1 || ! command -v iconv >/dev/null 2>&1; then
  echo "SKIP  no USERPROFILE, cygpath or iconv here"; exit 1
fi
NAME_RE=$(printf '%s' "$NAME" | sed 's/[][\/.*^$]/\\&/g')
mask() { sed "s/$NAME_RE/<name>/Ig"; }
say() { printf '%s\n' "$*" | mask; }

T="$BASE/suite_$(date +%Y%m%d_%H%M%S)_$NOHOOKS"
mkdir -p "$T" || { echo "FAIL  could not make $T"; exit 1; }
case "$(cygpath -m "$T")/" in "$(cygpath -m "$USERPROFILE")"/*) echo "FAIL  $T lies under the profile folder"; exit 1 ;; esac
: > "$T/global.gitconfig"
export GIT_CONFIG_GLOBAL="$T/global.gitconfig"   # the user's own global config (aliases, hooksPath, signing) left out
export GIT_AUTHOR_NAME=t GIT_AUTHOR_EMAIL=t@example.invalid GIT_COMMITTER_NAME=t GIT_COMMITTER_EMAIL=t@example.invalid
export GIT_CONFIG_COUNT=3 GIT_CONFIG_KEY_0=core.autocrlf GIT_CONFIG_VALUE_0=false GIT_CONFIG_KEY_1=init.defaultBranch \
  GIT_CONFIG_VALUE_1=main GIT_CONFIG_KEY_2=advice.detachedHead GIT_CONFIG_VALUE_2=false
export WG_PROFILE_TRACE=1 GIT_EDITOR=true
unset GIT_DIR GIT_WORK_TREE GIT_INDEX_FILE

PW=$(cygpath -w "$USERPROFILE")                  # as it is: backslashes
PM=$(cygpath -m "$USERPROFILE")                  # forward slashes (cygpath -m)
PU=$(cygpath -u "$USERPROFILE")                  # the MSYS form, /c/...
PJ=${PW//\\/\\\\}                                # JSON-escaped
PUP=${PW^^}                                      # upper case
PURL=${PW//:/%3A}; PURL=${PURL//\\/%5C}          # URL-encoded
PF=${PM//:/-}; PF=${PF//\//-}                    # as a file name, C--Users-<name>
up=${PM%/*}; PARENT=${up##*/}                    # Users
DRIVE=${PW%%\\*}                                 # C:

ok=0; bad=0; nref=0; nrefmiss=0; npass=0; nwent=0
n=0
repo() {  # a new repository R with one commit, pushed to its bare remote R.git (upstream set); hooks unless NOHOOKS
  n=$((n + 1)); R="$T/r$n"
  {
    git init -q "$R" && git init -q --bare "$R.git" &&
      { [ "$NOHOOKS" = 1 ] || bash "$HERE/install.sh" "$R"; } &&
      printf 'base\n' > "$R/base.txt" && git -C "$R" add base.txt && git -C "$R" commit -q --no-verify -m base &&
      git -C "$R" remote add origin "$R.git" && git -C "$R" push -q --no-verify -u origin main
  } >/dev/null 2>&1 || echo "      (could not set up repository r$n)"
}
put() { mkdir -p "$(dirname "$R/$1")"; printf '%s\n' "$2" > "$R/$1"; }   # $1 a file of R, $2 its text
stage() { git -C "$R" add -- "$@" >/dev/null 2>&1; }
store() { git -C "$R" commit -q --no-verify -m "$1" >/dev/null 2>&1; }   # a commit made here, past the hooks
state() {  # what a refused command must leave as it was, and an allowed one must change
  git -C "$R" rev-parse -q --verify HEAD 2>/dev/null
  git -C "$R" for-each-ref 'refs/heads' 'refs/tags' 'refs/notes' 2>/dev/null
  git -C "$R.git" for-each-ref 2>/dev/null
}
run() {  # $1 the command, run by eval in R: sets out, rc, ms
  local s e; s=$(date +%s%N)
  out=$(cd "$R" && eval "$1" 2>&1); rc=$?
  e=$(date +%s%N); ms=$(( (e - s) / 1000000 ))
}
show() { [ -n "$out" ] && printf '%s\n' "$out" | head -8 | mask | sed 's/^/      | /'; }
named() { [[ ${out,,} == *"${NAME,,}"* ]]; }

refuse() {  # $1 label, $2 a phrase the refusal must name (the place), $3 the command
  local before after why=""; before=$(state)
  run "$3"; after=$(state)
  [ $rc -ne 0 ] || why+=" git exited 0;"
  [[ $out == *"profile check: this "*"would carry"* ]] || why+=" no refusal message;"
  [ -z "$2" ] || [[ $out == *"$2"* ]] || why+=" the place ($2) not named;"
  [[ $out == *"--no-verify"*"Only the user decides"* ]] || why+=" how to skip not said;"
  [ "$before" = "$after" ] || why+=" the repository or its remote changed;"
  named && why+=" the output shows the name;"
  nref=$((nref + 1))
  [ $rc -eq 0 ] && [ "$before" != "$after" ] && nwent=$((nwent + 1))     # the commit or the push went through
  if [ -z "$why" ]; then r=pass; ok=$((ok + 1)); else r=FAIL; bad=$((bad + 1)); nrefmiss=$((nrefmiss + 1)); fi
  say "$(printf '%-4s  refuse  %6d ms  %s' "$r" "$ms" "$1")"
  if [ $r = FAIL ]; then say "      why:$why"; [ "$NOHOOKS" = 1 ] || show; fi
}
allow() {  # $1 label, $2 the hooks whose trace must show (pre-commit, commit-msg, pre-push), $3 the command
  local before after why="" h; before=$(state)
  run "$3"; after=$(state)
  [ $rc -eq 0 ] || why+=" git exited $rc;"
  for h in $2; do [[ $out == *"profile check ($h): read "*"; 0 hits"* ]] || why+=" no trace of $h;"; done
  [[ $out == *"profile check skipped"* ]] && why+=" a skipped warning;"
  [[ $out == *"profile check: this"* ]] && why+=" refused;"
  [ "$before" != "$after" ] || why+=" nothing was committed or pushed;"
  named && why+=" the output shows the name;"
  npass=$((npass + 1))
  if [ -z "$why" ]; then r=pass; ok=$((ok + 1)); else r=FAIL; bad=$((bad + 1)); fi
  say "$(printf '%-4s  pass    %6d ms  %s' "$r" "$ms" "$1")"
  if [ $r = FAIL ]; then say "      why:$why"; show; fi
}
fails_open() {  # $1 label, $2 the warning it must give, $3 the command: passes unchecked, saying so
  local before after why=""; before=$(state)
  run "$3"; after=$(state)
  [ $rc -eq 0 ] || why+=" git exited $rc;"
  [[ $out == *"profile check skipped: $2"* ]] || why+=" no warning ($2);"
  [ "$before" != "$after" ] || why+=" nothing was committed or pushed;"
  named && why+=" the output shows the name;"
  if [ -z "$why" ]; then r=pass; ok=$((ok + 1)); else r=FAIL; bad=$((bad + 1)); fi
  say "$(printf '%-4s  open    %6d ms  %s' "$r" "$ms" "$1")"
  if [ $r = FAIL ]; then say "      why:$why"; show; fi
}
check() {  # $1 label, then a test command: pass when it succeeds
  if eval "$2" >/dev/null 2>&1; then r=pass; ok=$((ok + 1)); else r=FAIL; bad=$((bad + 1)); fi
  say "$(printf '%-4s  check            %s' "$r" "$1")"
}
caught() {  # $1 label, then a case that must come out FAIL: one of this suite's own judges catches a known-bad hook
  local label=$1 o=$ok b=$bad a=$nref m=$nrefmiss p=$npass w=$nwent; shift
  "$@" >/dev/null 2>&1
  ok=$o; bad=$b; nref=$a; nrefmiss=$m; npass=$p; nwent=$w
  if [ $r = FAIL ]; then ok=$((ok + 1)); say "pass  the suite catches $label"
  else bad=$((bad + 1)); say "FAIL  the suite does not catch $label"; fi
}
fake() {  # $1 a hook of R replaced by a known-bad one: $2 its body
  printf '#!/bin/sh\n%s\n' "$2" > "$R/.git/hooks/$1"
}

say "git: $(git --version); python: $(command -v python | mask); folder: $T"
[ "$NOHOOKS" = 1 ] && say "NO HOOKS INSTALLED: every refusal case below must come out FAIL"

if [ "$NOHOOKS" != 1 ]; then
echo "-- the suite's own judges, each shown a known-bad hook first"
repo; put c.txt "clean"; stage c.txt
fake pre-commit 'echo "profile check: this commit would carry <profile>: c.txt. git commit --no-verify. Only the user decides to skip it." >&2; exit 1'
caught "a hook that refuses a clean commit" allow "clean commit" "pre-commit commit-msg" 'git commit -m clean'
repo; put c.txt "clean"; stage c.txt; fake pre-commit 'exit 0'; fake commit-msg 'exit 0'
caught "a hook that passes without reading (no trace)" allow "clean commit" "pre-commit commit-msg" 'git commit -m clean'
repo; put a.txt "$PM"; stage a.txt
fake pre-commit "echo \"profile check: this commit would carry $PM: a.txt. git commit --no-verify. Only the user decides to skip it.\" >&2; exit 1"
caught "a refusal that shows the name" refuse "leak" "a.txt" 'git commit -m x'
repo; put a.txt "$PM"; stage a.txt
fake pre-commit 'echo "profile check: this commit would carry <profile>: a.txt. git commit --no-verify. Only the user decides to skip it." >&2; exit 0'
caught "a hook that says it refuses but lets the commit through" refuse "leak" "a.txt" 'git commit -m x'
repo; put a.txt "$PM"; stage a.txt; fake pre-commit 'echo "something else went wrong" >&2; exit 1'
caught "a refusal for another reason" refuse "leak" "a.txt" 'git commit -m x'
fi

echo "-- must refuse: commits"
repo; put a.txt "log written to $PW\\AppData\\Local\\x.log"; stage a.txt
refuse "a staged file holds the path: backslashes" "staged file a.txt" 'git commit -m "add a"'
repo; put a.txt "see $PM/AppData"; stage a.txt
refuse "a staged file holds the path: cygpath -m form" "staged file a.txt" 'git commit -m "add a"'
repo; put a.txt "see $PU/AppData"; stage a.txt
refuse "a staged file holds the path: MSYS form" "staged file a.txt" 'git commit -m "add a"'
repo; put j.json "{\"p\": \"$PJ\"}"; stage j.json
refuse "a staged file holds the path: JSON-escaped" "staged file j.json" 'git commit -m "add j"'
repo; put a.txt "$PUP"; stage a.txt
refuse "a staged file holds the path: upper case" "staged file a.txt" 'git commit -m "add a"'
repo; put a.txt "file:///$PURL/x"; stage a.txt
refuse "a staged file holds the path: URL-encoded" "staged file a.txt" 'git commit -m "add a"'
repo; printf 'at %s\r\n' "$PW" | iconv -f utf-8 -t utf-16le > "$R/u.txt"; stage u.txt
refuse "a staged file holds the path: UTF-16LE" "staged file u.txt" 'git commit -m "add u"'
repo; printf '\000\001\002%s\000\377\376' "$PM" > "$R/b.dat"; stage b.dat
refuse "a binary file holds the path" "staged file b.dat" 'git commit -m "add b"'
repo; put "$PF.txt" "clean"; stage "$PF.txt"
refuse "a staged file's name: C--Users-<name>.txt" "name of the staged file" 'git commit -m "add f"'
repo; put "$PARENT/$NAME/notes.txt" "clean"; stage "$PARENT/$NAME/notes.txt"
refuse "a staged file's folder name: Users/<name>/notes.txt" "name of the staged file" 'git commit -m "add notes"'
repo; put c.txt "clean"; stage c.txt
refuse "-m holds the path" "commit message" 'git commit -m "notes kept in $PM/x"'
repo; put c.txt "clean"; stage c.txt; printf 'notes kept in %s\n' "$PW" > "$T/msg_bad.txt"
refuse "-F <file> holds the path" "commit message" 'git commit -F "$T/msg_bad.txt"'
repo; put c.txt "clean"; stage c.txt; printf '#!/bin/sh\nprintf "written in %%s\\n" "%s" > "$1"\n' "$PM" > "$T/ed_bad.sh"
refuse "the editor writes the path into the message" "commit message" 'GIT_EDITOR="sh $T/ed_bad.sh" git commit'
repo; put c.txt "clean"; stage c.txt; store "kept in $PM"
refuse "--amend --no-edit keeps a message that holds the path" "commit message" 'git commit --amend --no-edit'
repo; put a.txt "$PM"; stage a.txt
refuse "--amend with a staged file that holds the path" "staged file a.txt" 'git commit --amend -m "base, amended"'
repo; git -C "$R" checkout -q -b side; put s.txt "side"; stage s.txt; store "side"; git -C "$R" checkout -q main
put m.txt "main"; stage m.txt; store "main"
refuse "a merge commit's message holds the path" "commit message" 'git merge --no-ff -m "merged at $PM" side'
repo; put a.txt "$PM"; stage a.txt
refuse "a commit from git -C <dir>, run from another folder" "staged file a.txt" 'cd "$T" && git -C "$R" commit -m "add a"'
repo; git -C "$R" worktree add -q -b wt "$R-wt" >/dev/null 2>&1; R0=$R; R="$R-wt"; put a.txt "$PM"; stage a.txt
refuse "a commit in a linked worktree (the hooks live in the common git folder)" "staged file a.txt" 'git commit -m "add a"'
R=$R0
repo; put base.txt "$PM"
refuse "git commit -a of a tracked file (a temporary index)" "staged file base.txt" 'WG_GUARD_OK=G1 git commit -a -m x'
repo; put base.txt "$PM"
refuse "git commit <path> of a tracked file (a temporary index)" "staged file base.txt" 'git commit -m x base.txt'
repo; put a.txt "$PM"; stage a.txt; mkdir -p "$T/fakeapps/WindowsApps"
printf '#!/bin/sh\necho "the Store stub ran" >&2\nexit 9009\n' > "$T/fakeapps/WindowsApps/python"; chmod +x "$T/fakeapps/WindowsApps/python"
refuse "a Store stub (WindowsApps) first on PATH is skipped" "staged file a.txt" 'PATH="$(cygpath -u "$T")/fakeapps/WindowsApps:$PATH" git commit -m "add a"'

echo "-- must refuse: pushes"
repo; put a.txt "$PM"; stage a.txt; store "add a"
refuse "git push origin main, a commit with the path in a file" "the file a.txt" 'git push origin main'
repo; git -C "$R" checkout -q -b feat; put a.txt "$PM"; stage a.txt; store "add a"
refuse "a new branch (remote oid all zeros)" "the file a.txt" 'git push origin feat'
repo; git -C "$R" tag -a v1 -m "tagged at $PM"
refuse "an annotated tag whose message holds the path (on a commit already pushed)" "tag object" 'git push origin v1'
repo; git -C "$R" notes add -m "see $PM" HEAD
refuse "notes" "the file" 'git push origin refs/notes/commits'
repo; git -C "$R" checkout -q -b other; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" checkout -q main
refuse "a wildcard refspec, the leak on a branch not checked out" "the file a.txt" "git push origin 'refs/heads/*:refs/heads/*'"
repo; git -C "$R" checkout -q -b other; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" checkout -q main
refuse "--all" "the file a.txt" 'git push --all origin'
repo; git -C "$R" checkout -q -b other; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" checkout -q main
refuse "--mirror" "the file a.txt" 'git push --mirror origin'
repo; git -C "$R" checkout -q -b other; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" checkout -q main
git -C "$R" config remote.origin.push 'refs/heads/*:refs/heads/*'
refuse "remote.origin.push in the config, then git push origin" "the file a.txt" 'git push origin'
repo; git -C "$R" branch other; git -C "$R" push -q --no-verify origin other >/dev/null 2>&1
git -C "$R" checkout -q other; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" checkout -q main
git -C "$R" config push.default matching
refuse "push.default=matching, the leak on the other branch" "the file a.txt" 'git push'
repo; put a.txt "$PM"; stage a.txt; store "add a"
refuse "a push by URL (no remote-tracking refs for it)" "the file a.txt" 'git push "$R.git" main'
repo; put a.txt "$PM"; stage a.txt; git -C "$R" commit -q --no-verify --amend -m "base, rewritten" >/dev/null 2>&1
refuse "a force push" "the file a.txt" 'git push -f origin main'
repo
b=$(printf 'kept in %s\n' "$PM" | git -C "$R" hash-object -w --stdin)
tr=$(printf '100644 blob %s\tbase.txt\n100644 blob %s\tleak.txt\n' "$(git -C "$R" rev-parse HEAD:base.txt)" "$b" | git -C "$R" mktree)
c=$(git -C "$R" commit-tree "$tr" -p HEAD -m "made by plumbing"); git -C "$R" update-ref refs/heads/main "$c"
refuse "a commit made by plumbing (commit-tree, update-ref: no commit hook ran)" "the file leak.txt" 'git push origin main'
repo; put "$PARENT/$NAME/x.txt" "clean"; stage "$PARENT/$NAME/x.txt"; store "add x"
refuse "the path in a folder name only (Users/<name>/x.txt, clean content)" "file or folder name" 'git push origin main'
repo; git -C "$R" mv base.txt "$PF.txt"; store "rename"
refuse "a rename to C--Users-<name>.txt, its content already on the remote (only the tree holds the name)" "a name in the folder" 'git push origin main'
repo; put c.txt "clean"; stage c.txt; store "see $PM"
refuse "the path in a commit message only" "commit" 'git push origin main'
repo; put c.txt "clean"; stage c.txt; GIT_AUTHOR_NAME="$PW" store "clean"
refuse "the path in the author's name" "commit" 'git push origin main'
repo; git init -q --bare "$R-bak.git"; git -C "$R" remote add bak "$R-bak.git"
put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" push -q --no-verify bak main >/dev/null 2>&1
refuse "a leak held by a second remote (bak) but not by origin, pushed to origin" "the file a.txt" 'git push origin main'

echo "-- must refuse: a 200 MB staged blob with the path at its end"
repo; { head -c 209715200 /dev/zero | tr '\0' a; printf '\n%s\n' "$PW"; } > "$R/big.txt"; stage big.txt
say "      (the blob: $(git -C "$R" cat-file -s :big.txt 2>/dev/null) bytes)"
refuse "a 200 MB staged blob, the path at its end" "staged file big.txt" 'git commit -m "add big"'

if [ "$NOHOOKS" != 1 ]; then
echo "-- must pass"
repo; put c.txt "clean"; stage c.txt
allow "a clean commit" "pre-commit commit-msg" 'git commit -m "add c"'
allow "a clean push" "pre-push" 'git push origin main'
repo; put n.txt "$NAME"; stage n.txt
allow "the bare name alone, in a file and in the message" "pre-commit commit-msg" 'git commit -q -m "by $NAME"'
allow "the same pushed" "pre-push" 'git push'
repo; put p.txt "shared files are in $DRIVE\\$PARENT\\Public"; stage p.txt
allow "$DRIVE\\$PARENT\\Public in a file" "pre-commit commit-msg" 'git commit -m "add p"'
repo; put c.txt "clean"; stage c.txt
allow "the words \"the profile folder\" in the message" "pre-commit commit-msg" 'git commit -m "keep the profile folder out of it"'
repo; put t.txt 'write ~ or %USERPROFILE% (or $USERPROFILE) for it'; stage t.txt
allow "~ and %USERPROFILE% as text, in a file and in the message" "pre-commit commit-msg" 'git commit -m "write ~ or %USERPROFILE% for it"'
allow "the same pushed" "pre-push" 'git push'
repo; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" push -q --no-verify >/dev/null 2>&1; git -C "$R" rm -q a.txt >/dev/null 2>&1
allow "deleting a file that held the path" "pre-commit commit-msg" 'git commit -m "remove a"'
repo; put a.txt "$PM"; stage a.txt; store "add a"; git -C "$R" push -q --no-verify >/dev/null 2>&1
put c.txt "clean"; stage c.txt; store "clean"
allow "a clean push on top of a leak already on the remote" "pre-push" 'git push origin main'
repo; git -C "$R" checkout -q -b feat; put a.txt "$PM"; stage a.txt; store "add a"
git -C "$R" push -q --no-verify origin feat >/dev/null 2>&1; git -C "$R" checkout -q main
allow "a delete push of a branch that holds the path" "pre-push" 'git push origin --delete feat'
repo; git -C "$R" checkout -q -b side; put s.txt "side"; stage s.txt; store "side"; git -C "$R" checkout -q main
allow "a clean merge commit" "commit-msg" 'git merge --no-ff -m "merge side" side'
repo; git -C "$R" worktree add -q -b wt "$R-wt" >/dev/null 2>&1; R0=$R; R="$R-wt"; put c.txt "clean"; stage c.txt
allow "a clean commit in a linked worktree" "pre-commit commit-msg" 'git commit -m "add c"'
R=$R0
repo; mkdir -p "$R/many"; for i in $(seq 1 3000); do printf 'small file %d\n' "$i" > "$R/many/f$i.txt"; done; stage many
allow "3000 small files: the commit" "pre-commit commit-msg" 'git commit -m "add 3000"'
allow "3000 small files: the push" "pre-push" 'git push origin main'

echo "-- must pass unchecked, saying so (the hooks fail open)"
repo; put a.txt "$PM"; stage a.txt
fails_open "no Python on PATH (the path staged)" "no Python found" 'PATH=/usr/bin:/bin:/mingw64/bin git commit -m "add a"'
repo; put a.txt "$PM"; stage a.txt
fails_open "USERPROFILE and HOME empty (the path staged)" "USERPROFILE and HOME name no profile folder" 'USERPROFILE= HOME= git commit -m "add a"'

echo "-- install.sh and uninstall.sh"
repo; o=$(bash "$HERE/install.sh" "$R" 2>&1)
check "install.sh a second time: all four unchanged" '[ "$(printf "%s\n" "$o" | grep -c " unchanged$")" = 4 ]'
check "the installed files: LF line endings and the marker line" \
  '! grep -l $'"'"'\r'"'"' "$R/.git/hooks/pre-commit" "$R/.git/hooks/commit-msg" "$R/.git/hooks/pre-push" "$R/.git/hooks/profile_check.py" &&
   [ "$(grep -l "^# wg-profile-check: installed by" "$R/.git/hooks/pre-commit" "$R/.git/hooks/commit-msg" "$R/.git/hooks/pre-push" "$R/.git/hooks/profile_check.py" | wc -l)" = 4 ]'
n=$((n + 1)); R="$T/r$n"; git init -q "$R"; printf '#!/bin/sh\necho mine\n' > "$R/.git/hooks/pre-commit"
o=$(bash "$HERE/install.sh" "$R" 2>&1); orc=$?
check "install.sh beside a foreign pre-commit: refuses, says so, changes nothing" \
  '[ $orc -ne 0 ] && [[ $o == *"refusing to overwrite"* ]] && [ "$(cat "$R/.git/hooks/pre-commit")" = "#!/bin/sh
echo mine" ] && [ ! -e "$R/.git/hooks/commit-msg" ] && [ ! -e "$R/.git/hooks/pre-push" ] && [ ! -e "$R/.git/hooks/profile_check.py" ]'
n=$((n + 1)); R="$T/r$n"; git init -q "$R"; git -C "$R" config core.hooksPath elsewhere
o=$(bash "$HERE/install.sh" "$R" 2>&1); orc=$?
check "install.sh with core.hooksPath set: refuses, changes nothing" '[ $orc -ne 0 ] && [[ $o == *"core.hooksPath"* ]] && [ ! -e "$R/.git/hooks/pre-commit" ]'
repo; rm -f "$R/.git/hooks/pre-push"; printf '#!/bin/sh\nexit 0\n' > "$R/.git/hooks/pre-push"
o=$(bash "$HERE/uninstall.sh" "$R" 2>&1)
check "uninstall.sh: removes the three marked files, leaves a foreign pre-push" \
  '[ ! -e "$R/.git/hooks/pre-commit" ] && [ ! -e "$R/.git/hooks/commit-msg" ] && [ ! -e "$R/.git/hooks/profile_check.py" ] && [ -e "$R/.git/hooks/pre-push" ] && [[ $o == *"pre-push left"* ]]'

echo "-- timing: a typical commit (one small file), five each, with and without the hooks"
repo; tw=""; for i in 1 2 3 4 5; do put "t$i.txt" "typical $i"; stage "t$i.txt"; run "git commit -q -m t$i"; tw+="$ms "; done
R0=$R; NOHOOKS=1 repo; to=""; for i in 1 2 3 4 5; do put "t$i.txt" "typical $i"; stage "t$i.txt"; run "git commit -q -m t$i"; to+="$ms "; done
say "      with the hooks: $tw ms; without: $to ms"
fi

echo "== $( [ "$NOHOOKS" = 1 ] && echo "no hooks" || echo "hooks installed" ): $ok passed, $bad failed, $((ok + bad)) cases; $nref refusal cases, $nrefmiss of them not refused, $nwent went through (committed or pushed)"
[ "$NOHOOKS" = 1 ] && { [ $nwent -eq $nref ] && exit 0 || exit 1; }
[ $bad -eq 0 ] || exit 1
exit 0
