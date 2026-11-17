#!/bin/bash
# Installs git's own side of G6 (pre-commit, commit-msg, pre-push and profile_check.py, all from this folder) into the
# hooks folder of one repository's common git folder: "$(git rev-parse --git-common-dir)/hooks/". There they act on
# that repository and its worktrees, and on no other repository.
#   bash .claude/hooks/git/install.sh [the repository's folder, default the current one]
# Each file is written with LF line endings and a marker line. A file already there without the marker is not
# overwritten: then nothing at all is changed, and it says so (exit 1). Safe to run twice: it says, file by file,
# what it did (installed, updated, unchanged). uninstall.sh removes only the marked files.
# It refuses (exit 1, nothing changed) when core.hooksPath is set, since git would then run hooks from that folder
# and not from this one.
MARK='# wg-profile-check: installed by .claude/hooks/git/install.sh; uninstall.sh removes it'
FILES="pre-commit commit-msg pre-push profile_check.py"
src=$(cd "$(dirname "$0")" && pwd) || exit 1
repo=${1:-.}
say() { echo "install: $*"; }
common=$(git -C "$repo" rev-parse --path-format=absolute --git-common-dir 2>/dev/null) || {
  say "not a git repository: $repo (nothing changed)"; exit 1; }
hp=$(git -C "$repo" config --get core.hooksPath 2>/dev/null)
if [ -n "$hp" ]; then
  say "core.hooksPath is set ($hp): git would run hooks from there, not from $common/hooks. Nothing changed."
  exit 1
fi
dest=$common/hooks
for f in $FILES; do
  [ -f "$src/$f" ] || { say "$f is missing beside install.sh (nothing changed)"; exit 1; }
  if [ -e "$dest/$f" ] && ! grep -qxF "$MARK" "$dest/$f" 2>/dev/null; then
    say "$dest/$f is already there and was not installed by this script: refusing to overwrite it. Nothing changed."
    say "Look at it, and move it away yourself if it should go."
    exit 1
  fi
done
mkdir -p "$dest" || { say "could not make $dest (nothing changed)"; exit 1; }
tmp=$(mktemp "$dest/.wg-install.XXXXXX") || { say "could not write into $dest (nothing changed)"; exit 1; }
trap 'rm -f "$tmp"' EXIT
for f in $FILES; do
  # The marker goes after a #! line, else first; carriage returns are dropped (LF line endings).
  tr -d '\r' < "$src/$f" | awk -v m="$MARK" 'NR == 1 && /^#!/ { print; print m; next } NR == 1 { print m } { print }' > "$tmp" ||
    { say "could not prepare $f"; exit 1; }
  if [ -f "$dest/$f" ] && cmp -s "$tmp" "$dest/$f"; then
    say "$f unchanged"
    continue
  fi
  was=installed; [ -f "$dest/$f" ] && was=updated
  cp "$tmp" "$dest/$f" && chmod +x "$dest/$f" 2>/dev/null
  say "$f $was"
done
say "done: $dest"
exit 0
