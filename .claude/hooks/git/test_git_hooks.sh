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
# The cases named "mend N" were added on November 17-18 for the mend after the two reviews (g6_git_report.md, "Mend");
# each was shown failing on the files of commit 29faa56 (the suite run beside copies of them) and passing on these.
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

echo "-- must refuse: the mend (November 17-18)"
# 1. replace refs: what is read must be what git sends.
repo; put a.txt "$PM"; stage a.txt; store "add a"
cb=$(printf 'clean\n' | git -C "$R" hash-object -w --stdin); git -C "$R" replace -f "$(git -C "$R" rev-parse HEAD:a.txt)" "$cb"
refuse "mend 1: a dirty blob hidden behind a replace ref (git replace)" "the file a.txt" 'git push origin main'
repo; put c.txt "clean"; stage c.txt; store "kept in $PM"
cc=$(git -C "$R" commit-tree "HEAD^{tree}" -p HEAD^ -m "clean"); git -C "$R" replace -f "$(git -C "$R" rev-parse HEAD)" "$cc"
refuse "mend 1: a dirty commit message hidden behind a replace ref" "commit" 'git push origin main'
# 2. the message as git stores it.
printf '#!/bin/sh\n{ printf "notes kept in %%s\\n" "%s"; cat "$1"; } > "$1.new" && mv "$1.new" "$1"\n' "$PM" > "$T/ed_add.sh"
repo; put c.txt "clean"; stage c.txt
refuse "mend 2: the path on a non-comment line, typed above the editor's template" "commit message" 'GIT_EDITOR="sh $T/ed_add.sh" git commit'
repo; put c.txt "clean"; stage c.txt
refuse "mend 2: the path on a # line given with -m (git keeps it)" "commit message" 'git commit -m "x" -m "# kept in $PM"'
repo; put c.txt "clean"; stage c.txt; printf 'x\n\n# kept in %s\n' "$PW" > "$T/msg_hash.txt"
refuse "mend 2: the path on a # line given with -F (git keeps it)" "commit message" 'git commit -F "$T/msg_hash.txt"'
repo; put c.txt "clean"; stage c.txt; printf 'x\n# ------------------------ >8 ------------------------\nkept in %s\n' "$PM" > "$T/msg_cut.txt"
refuse "mend 2: the path below a scissors line that is not git's own (-F, no -v)" "commit message" 'git commit -F "$T/msg_cut.txt"'
# 3. every changed full path of each pushed commit.
repo; put A/f.txt "same"; put "$PARENT/$NAME/f.txt" "same"; stage A/f.txt "$PARENT/$NAME/f.txt"; store "two names, one blob"
refuse "mend 3: A/f.txt and Users/<name>/f.txt with the same content (the code reader's run 1)" "file or folder name" 'git push origin main'
repo; put "$PARENT/$NAME/f.txt" "same"; stage "$PARENT/$NAME/f.txt"; store "add"
git -C "$R" rm -q -r "$PARENT" >/dev/null 2>&1; put zzz/f.txt "same"; stage zzz/f.txt; store "move"
refuse "mend 3: Users/<name>/f.txt added, then moved to zzz/f.txt, pushed together (run 2)" "file or folder name" 'git push origin main'
# 4. many staged files: the working-tree copies.
repo; mkdir -p "$R/many"; for i in $(seq 1 2999); do printf 'small file %d\n' "$i" > "$R/many/f$i.txt"; done
put many/f3000.txt "at $PM"; stage many
refuse "mend 4: 3000 new files, the path in the last" "staged file many/f3000.txt" 'git commit -m "add 3000"'
repo; mkdir -p "$R/many"; for i in $(seq 1 40); do printf 'small file %d\n' "$i" > "$R/many/f$i.txt"; done
put many/f40.txt "at $PM"; stage many; put many/f40.txt "small file 40"
refuse "mend 4: 40 staged files, the staged one holds the path, its working copy not" "staged file many/f40.txt" 'git commit -m "add 40"'
repo; mkdir -p "$R/many"; for i in $(seq 1 40); do printf 'small file %d\n' "$i" > "$R/many/f$i.txt"; done
printf '*.txt filter=leak\n' > "$R/.gitattributes"; git -C "$R" config filter.leak.clean "sed 's|XXX|$PM|'"
put many/f40.txt "at XXX"; stage .gitattributes many
refuse "mend 4: 40 staged files, a clean filter puts the path in the staged blob only" "staged file many/f40.txt" 'git commit -m "add 40"'
repo; mkdir -p "$R/many"; for i in $(seq 1 40); do printf 'small file %d\r\n' "$i" > "$R/many/f$i.txt"; done
printf 'at %s\r\n' "$PW" > "$R/many/f40.txt"; printf '* text=auto\n' > "$R/.gitattributes"; stage .gitattributes many
refuse "mend 4: 40 CRLF files with text=auto, the path in the last" "staged file many/f40.txt" 'git commit -m "add 40"'
# 5. other spellings.
repo; put a.txt "at $DRIVE\\$PARENT\\.\\$NAME\\x"; stage a.txt
refuse "mend 5: a dot segment, Users\\.\\<name>" "staged file a.txt" 'git commit -m "add a"'
repo; put a.txt "at $DRIVE/$PARENT/./$NAME/x"; stage a.txt
refuse "mend 5: a dot segment, Users/./<name>" "staged file a.txt" 'git commit -m "add a"'
repo; put a.txt "at $DRIVE/${PARENT}_$NAME/x"; stage a.txt
refuse "mend 5: an underscore, Users_<name>" "staged file a.txt" 'git commit -m "add a"'
repo; put a.txt "the $PARENT $NAME folder"; stage a.txt
refuse "mend 5: a space, Users <name>" "staged file a.txt" 'git commit -m "add a"'
repo; put a.txt "at $DRIVE\\$PARENT\\"$'\n'"$NAME\\x"; stage a.txt
refuse "mend 5: a line break between Users\\ and the name" "staged file a.txt" 'git commit -m "add a"'
repo; printf 'at %s\n' "$PW" | iconv -f utf-8 -t utf-16be > "$R/be.txt"; stage be.txt
refuse "mend 5: UTF-16BE" "staged file be.txt" 'git commit -m "add be"'
repo; printf 'at %s\n' "$PW" | iconv -f utf-8 -t utf-32le > "$R/w.txt"; stage w.txt
refuse "mend 5: UTF-32LE" "staged file w.txt" 'git commit -m "add w"'
# 6. not only the caller's USERPROFILE.
repo; put a.txt "$PM"; stage a.txt
refuse "mend 6: USERPROFILE set to another folder on the commit, a staged leak" "staged file a.txt" "USERPROFILE='$DRIVE\\$PARENT\\zzzz' git commit -m 'add a'"
repo; put a.txt "$PM"; stage a.txt
refuse "mend 6: USERPROFILE and HOME empty, a staged leak (the system's profile folder)" "staged file a.txt" 'USERPROFILE= HOME= git commit -m "add a"'
# 7. the Python written into the hooks at install.
repo; put a.txt "$PM"; stage a.txt
refuse "mend 7: no Python on PATH, the installed hook still checks" "staged file a.txt" 'PATH=/usr/bin:/bin:/mingw64/bin git commit -m "add a"'
repo; put a.txt "$PM"; stage a.txt
for h in pre-commit commit-msg pre-push; do [ -f "$R/.git/hooks/$h" ] && sed -i "s|^wg_python=.*|wg_python='$T/gone/python.exe'|" "$R/.git/hooks/$h"; done
refuse "mend 7: the installed Python gone, the hook searches PATH" "staged file a.txt" 'git commit -m "add a"'

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
check "mend 4: the 3000-file commit said first how many files it reads, and read them from the working tree" \
  '[[ $out == *"profile check: reading 3000 files"* ]] && [[ $out == *"3000 read from the working tree"* ]]'
allow "3000 small files: the push" "pre-push" 'git push origin main'

echo "-- must pass: the mend (November 17-18)"
# 2. a cleanup commit made through the editor's template (GIT_EDITOR=true keeps git's template as it is).
repo; put l.txt "$PM"; stage l.txt; store "leak"; git -C "$R" rm -q l.txt >/dev/null 2>&1
allow "mend 2: git rm of the leaking file, commit -v (the diff below git's scissors line)" "pre-commit commit-msg" 'git commit -v -e -m "remove l"'
repo; put l.txt "$PM"; stage l.txt; store "leak"; git -C "$R" rm -q l.txt >/dev/null 2>&1; git -C "$R" config commit.verbose true
allow "mend 2: the same with commit.verbose=true" "pre-commit commit-msg" 'git commit -e -m "remove l"'
repo; put l.txt "$PM"; stage l.txt; store "leak"; put l.txt "clean now"; stage l.txt
allow "mend 2: the path edited out of a file, commit -v" "pre-commit commit-msg" 'git commit -v -e -m "take the path out"'
repo; put "$PF.txt" "clean"; stage "$PF.txt"; store "leak"; git -C "$R" rm -q "$PF.txt" >/dev/null 2>&1
allow "mend 2: git rm of a file whose name holds the path, the editor's template (status comments)" "pre-commit commit-msg" 'git commit -q -e -m "remove f"'  # -q: git's own summary line names the file
# 3. a cleanup push: the deletion of a name already on the remote.
repo; put "$PF.txt" "clean"; stage "$PF.txt"; store "leak"; git -C "$R" push -q --no-verify >/dev/null 2>&1
git -C "$R" rm -q "$PF.txt" >/dev/null 2>&1; store "remove f"
allow "mend 3: a push whose commit deletes a file named with the path, already on the remote" "pre-push" 'git push origin main'
# 4. many CRLF files read from the working tree (the hash with CRLF made LF is the staged blob).
repo; mkdir -p "$R/many"; for i in $(seq 1 40); do printf 'small file %d\r\n' "$i" > "$R/many/f$i.txt"; done
printf '* text=auto\n' > "$R/.gitattributes"; stage .gitattributes many
allow "mend 4: 40 CRLF files with text=auto" "pre-commit commit-msg" 'git commit -m "add 40"'
check "mend 4: those 40 (and .gitattributes) were read from the working tree" '[[ $out == *"41 read from the working tree"* ]]'
# 5. the name's right boundary, and the innocent forms.
repo; put a.txt "at $DRIVE/$PARENT/${NAME}x/notes and $DRIVE\\$PARENT\\${NAME}2"; stage a.txt
allow "mend 5: Users/<name>x and Users\\<name>2 (other users)" "pre-commit commit-msg" 'git commit -m "add a"'
repo; put a.txt 'see ~/Documents and %USERPROFILE%\Documents'; stage a.txt
allow "mend 5: ~/Documents and %USERPROFILE%" "pre-commit commit-msg" 'git commit -m "see ~/Documents"'
# 3. the time for many commits: 1000 new commits pushed.
repo; fi_=$T/fi_$n.txt; : > "$fi_"
for i in $(seq 1 1000); do
  printf 'commit refs/heads/many\ncommitter t <t@example.invalid> %d +0000\ndata %d\nc%d\n' $((1700000000 + i)) $(( ${#i} + 2 )) "$i" >> "$fi_"
  [ "$i" = 1 ] && printf 'from refs/heads/main^0\n' >> "$fi_"
  printf 'M 100644 inline d%d/f%d.txt\ndata %d\nv%d\n\n' $((i % 37)) $((i % 50)) $(( ${#i} + 2 )) "$i" >> "$fi_"
done
git -C "$R" fast-import --quiet < "$fi_" >/dev/null 2>&1
say "      (branch many: $(git -C "$R" rev-list --count main..many 2>/dev/null) new commits)"
allow "mend 3: a push of 1000 new commits" "pre-push" 'git push origin many'

echo "-- must pass unchecked, saying so (the hooks fail open)"
repo; put a.txt "$PM"; stage a.txt
for h in pre-commit commit-msg pre-push; do [ -f "$R/.git/hooks/$h" ] && sed -i "s|^wg_python=.*|wg_python='$T/gone/python.exe'|" "$R/.git/hooks/$h"; done
fails_open "no Python on PATH and the installed one gone (the path staged)" "no Python found" 'PATH=/usr/bin:/bin:/mingw64/bin git commit -m "add a"'

echo "-- install.sh and uninstall.sh"
repo; o=$(bash "$HERE/install.sh" "$R" 2>&1)
check "install.sh a second time: all four unchanged" '[ "$(printf "%s\n" "$o" | grep -c " unchanged$")" = 4 ]'
check "the installed files: LF line endings and the marker line" \
  '! grep -l $'"'"'\r'"'"' "$R/.git/hooks/pre-commit" "$R/.git/hooks/commit-msg" "$R/.git/hooks/pre-push" "$R/.git/hooks/profile_check.py" &&
   [ "$(grep -l "^# wg-profile-check: installed by" "$R/.git/hooks/pre-commit" "$R/.git/hooks/commit-msg" "$R/.git/hooks/pre-push" "$R/.git/hooks/profile_check.py" | wc -l)" = 4 ]'
PYW=$(sed -n "s/^wg_python='\(.*\)'\$/\1/p" "$R/.git/hooks/pre-commit" 2>/dev/null)
check "mend 7: install.sh wrote the full path of a Python (not the Store stub) into the three hooks" \
  '[ -n "$PYW" ] && [ -x "$PYW" ] && [[ $PYW != *[Ww]indows[Aa]pps* ]] && "$PYW" -I -c "import sys" &&
   [ "$(grep -lxF "wg_python='"'"'$PYW'"'"'" "$R/.git/hooks/pre-commit" "$R/.git/hooks/commit-msg" "$R/.git/hooks/pre-push" | wc -l)" = 3 ]'
n=$((n + 1)); R="$T/r$n"; git init -q "$R"; printf '#!/bin/sh\necho mine\n' > "$R/.git/hooks/pre-commit"
o=$(bash "$HERE/install.sh" "$R" 2>&1); orc=$?
check "install.sh beside a foreign pre-commit: refuses, says so, changes nothing" \
  '[ $orc -ne 0 ] && [[ $o == *"refusing to overwrite"* ]] && [ "$(cat "$R/.git/hooks/pre-commit")" = "#!/bin/sh
echo mine" ] && [ ! -e "$R/.git/hooks/commit-msg" ] && [ ! -e "$R/.git/hooks/pre-push" ] && [ ! -e "$R/.git/hooks/profile_check.py" ]'
# 8. beside a foreign pre-commit and pre-push: refuses, changes nothing, says per hook how to merge by hand; the
# merge done as it says (for pre-push, the hardest: git's ref lines on stdin), both run.
NOHOOKS=1 repo
printf '#!/bin/sh\necho mine\n' > "$R/.git/hooks/pre-commit"
printf '#!/bin/sh\ncat > "$(dirname "$0")/../foreign_saw.txt"\nexit 0\n' > "$R/.git/hooks/pre-push"; chmod +x "$R/.git/hooks/pre-push"
o=$(bash "$HERE/install.sh" "$R" 2>&1); orc=$?
LINE_PP=$(printf '%s\n' "$o" | sed -n 's/^install:      *\(printf .*pre-push\.local.*\)$/\1/p' | head -1)
check "mend 8: install.sh beside a foreign pre-commit and pre-push: refuses, changes nothing, says per hook how to merge" \
  '[ $orc -ne 0 ] && [[ $o == *"pre-commit is already there"*"mv "*"pre-commit.local"*"\"\$here/pre-commit.local\" \"\$@\" || exit \$?"* ]] &&
   [[ $o == *"pre-push is already there"*"mv "*"pre-push.local"* ]] && [ -n "$LINE_PP" ] && [[ $o == *"Nothing changed."* ]] &&
   [ ! -e "$R/.git/hooks/commit-msg" ] && [ ! -e "$R/.git/hooks/profile_check.py" ] && [ "$(cat "$R/.git/hooks/pre-commit")" = "#!/bin/sh
echo mine" ]'
rm -f "$R/.git/hooks/pre-commit"; mv "$R/.git/hooks/pre-push" "$R/.git/hooks/pre-push.local"
bash "$HERE/install.sh" "$R" >/dev/null 2>&1
printf '%s\n' "$LINE_PP" > "$T/line_pp.txt"
sed -i "/^# (a hook of your own, merged by hand/r $T/line_pp.txt" "$R/.git/hooks/pre-push"
put a.txt "$PM"; stage a.txt; store "add a"
refuse "mend 8: the pre-push merged by hand as install.sh says: the check refuses" "the file a.txt" 'git push origin main'
check "mend 8: and the foreign pre-push ran first and was given git's ref lines" '[[ "$(cat "$R/.git/foreign_saw.txt" 2>/dev/null)" == "refs/heads/main "* ]]'
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
