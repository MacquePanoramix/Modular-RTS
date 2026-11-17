#!/bin/bash
# Removes git's own side of G6 from one repository: only the files in "$(git rev-parse --git-common-dir)/hooks/"
# that carry install.sh's marker line. A file of the same name without the marker is left, and named.
#   bash .claude/hooks/git/uninstall.sh [the repository's folder, default the current one]
MARK='# wg-profile-check: installed by .claude/hooks/git/install.sh; uninstall.sh removes it'
FILES="pre-commit commit-msg pre-push profile_check.py"
repo=${1:-.}
say() { echo "uninstall: $*"; }
common=$(git -C "$repo" rev-parse --path-format=absolute --git-common-dir 2>/dev/null) || {
  say "not a git repository: $repo (nothing changed)"; exit 1; }
dest=$common/hooks
for f in $FILES; do
  if [ ! -e "$dest/$f" ]; then
    say "$f not there"
  elif grep -qxF "$MARK" "$dest/$f" 2>/dev/null; then
    rm -f "$dest/$f" && say "$f removed"
  else
    say "$f left: it was not installed by install.sh"
  fi
  [ -e "$dest/$f.local" ] && say "$f.local is there (a hook of yours, merged by hand): move it back to $f yourself if it should run"
done
exit 0
