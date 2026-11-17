#!/bin/bash
# Installs git's own side of G6 (pre-commit, commit-msg, pre-push and profile_check.py, all from this folder) into the
# hooks folder of one repository's common git folder: "$(git rev-parse --git-common-dir)/hooks/". There they act on
# that repository and its worktrees, and on no other repository.
#   bash .claude/hooks/git/install.sh [the repository's folder, default the current one]
# Each file is written with LF line endings and a marker line. Into each hook it writes the full path of the Python
# it finds now on PATH (never the Microsoft Store stub in a WindowsApps folder), so the hook still checks when git runs
# it with no Python on PATH (GitHub Desktop, perhaps); if that Python is gone later, the hook searches PATH again.
# A file already there without the marker is not overwritten: then nothing at all is changed, and it says so, hook by
# hook, with how to merge the two by hand (exit 1). It does not chain hooks itself. Safe to run twice: it says, file by
# file, what it did (installed, updated, unchanged). uninstall.sh removes only the marked files.
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
foreign=0
for f in $FILES; do
  [ -f "$src/$f" ] || { say "$f is missing beside install.sh (nothing changed)"; exit 1; }
  if [ -e "$dest/$f" ] && ! grep -qxF "$MARK" "$dest/$f" 2>/dev/null; then
    foreign=1
    say "$dest/$f is already there and was not installed by this script: refusing to overwrite it."
    case $f in
      profile_check.py)
        say "  Move it away yourself if it should go: this script needs that name." ;;
      *)
        say "  To keep both by hand (yours first, then the profile check):"
        say "  1. mv \"$dest/$f\" \"$dest/$f.local\""
        say "  2. run this script again"
        say "  3. in the new $dest/$f, put this line right below its line \"# (a hook of your own, merged by hand, ...\":"
        if [ "$f" = pre-push ]; then
          say "     printf '%s\\n' \"\$wg_in\" | \"\$here/$f.local\" \"\$@\" || exit \$?"
        else
          say "     \"\$here/$f.local\" \"\$@\" || exit \$?"
        fi
        say "  Running this script again later writes its own copy back (\"updated\"): then put that line in again." ;;
    esac
  fi
done
if [ $foreign = 1 ]; then say "Nothing changed."; exit 1; fi
# The Python the hooks will use: python, else python3, else py, in PATH order, never from a WindowsApps folder.
pyf=
IFS0=$IFS; IFS=:
for c in python python3 py; do
  for d in $PATH; do
    case "$d" in ''|*[Ww][Ii][Nn][Dd][Oo][Ww][Ss][Aa][Pp][Pp][Ss]*) continue ;; esac
    for f in "$d/$c.exe" "$d/$c"; do [ -f "$f" ] && [ -x "$f" ] && { pyf=$f; break 3; }; done
  done
done
IFS=$IFS0
[ -n "$pyf" ] && command -v cygpath >/dev/null 2>&1 && pyf=$(cygpath -m "$pyf")
case "$pyf" in *"'"*|*'"'*|*'$'*|*'`'*|*'\'*) say "the Python found ($pyf) has a quote, \$, \` or \\ in its path: not written into the hooks"; pyf= ;; esac
if [ -n "$pyf" ]; then say "python: $pyf (written into the hooks)"
else say "no Python found on PATH: the hooks will search PATH each time they run"; fi
mkdir -p "$dest" || { say "could not make $dest (nothing changed)"; exit 1; }
tmp=$(mktemp "$dest/.wg-install.XXXXXX") || { say "could not write into $dest (nothing changed)"; exit 1; }
trap 'rm -f "$tmp"' EXIT
for f in $FILES; do
  # The marker goes after a #! line, else first; the line "wg_python=" gets the Python's path; carriage returns are
  # dropped (LF line endings).
  tr -d '\r' < "$src/$f" | WG_PY="$pyf" awk -v m="$MARK" '
    NR == 1 && /^#!/ { print; print m; next }
    NR == 1 { print m }
    /^wg_python=$/ && ENVIRON["WG_PY"] != "" { print "wg_python='\''" ENVIRON["WG_PY"] "'\''"; next }
    { print }' > "$tmp" ||
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
