#!/bin/bash
# The guard (November 16, 2026): a PreToolUse hook on the Bash and PowerShell tools that refuses a few commands which
# have cost this project work before. It is a seat belt, not the rule: the rules live in AGENTS.md and Docs/Handoff.md,
# and a command the guard lets through is not thereby right. Why a tool at all: these rules were sentences, and they
# were broken again after they were written (Docs/Research/2026-11-16_HarnessGuideReview/audit_process_failures.md,
# rows 12, 19, 30, 31).
#   G1  everything staged at once (git add -A, -u, ., ./, :/, *; git commit -a): it once carried a settings file into
#       a commit. Stage by path.
#   G2  uncommitted work discarded (git reset --hard or --merge, clean unless a dry run, checkout --/./-f, switch -f,
#       restore without --staged, stash that saves): Luis's own uncommitted files live in the main project.
#   G3  a commit chained behind a test in one command (a commit said 8 of 8 for a run of 7 of 8): read, then commit.
#   G4  a shell heredoc holding a doubled backslash (the shell halved it; work lost three times): use Write or Edit.
#   (G5, a Unity run with no time limit, is not here: unity_tests.sh already sets LIMIT=10800 s when none is given.)
#   G6  a git commit or push that would carry the path of the user's own profile folder (it names the person) into the
#       public repository (approved by Luis, November 17). Unlike G1 to G4 it reads what the commit or push would store,
#       not the command's text: guard_profile.py, beside this file, run by python -I with the payload on stdin, within
#       8 s. Its exit 2 with a line refuses; anything else (no python, an error, a timeout) passes.
# It reads the command's text, so text that only mentions such a command (a search pattern, an echo, a commit message
# given inline) is refused too: put such text in a file. It does not see inside a script that a command runs.
# A deliberate, approved use passes when the command carries the words WG_GUARD_OK=G1 (or G2, G3, G4).
# Wired in .claude/settings.local.json (not in git: this machine only), hooks.PreToolUse, matcher "Bash|PowerShell",
#   command  bash "$CLAUDE_PROJECT_DIR/.claude/hooks/guard.sh"  (timeout 10). Exit 2 with one stderr line refuses.
# Switch it off: remove that PreToolUse entry, or set WG_GUARD=off in the environment (the "env" block of settings).
# It fails open: a payload it cannot read, or any error of its own, lets the call through. Its cases: guard_test.sh.
export LC_ALL=C
[ "$WG_GUARD" = off ] && exit 0
raw=$(timeout 2 cat 2>/dev/null)
# Find tool_input.command (no jq here): the first "command" key, then decode its JSON string.
rest=${raw#*\"command\"}
[ "$rest" = "$raw" ] && exit 0
rest="${rest#"${rest%%[![:space:]]*}"}"; [ "${rest:0:1}" = ":" ] || exit 0; rest=${rest:1}
rest="${rest#"${rest%%[![:space:]]*}"}"; [ "${rest:0:1}" = '"' ] || exit 0; rest=${rest:1}
cmd=""; nl=$'\n'
while :; do
  pre=${rest%%[\"\\]*}
  [ "$pre" = "$rest" ] && exit 0                      # no closing quote: a broken payload passes
  cmd+=$pre; rest=${rest:${#pre}}
  [ "${rest:0:1}" = '"' ] && break
  e=${rest:1:1}; rest=${rest:2}
  case "$e" in n) cmd+=$nl ;; t) cmd+=' ' ;; r) ;; u) cmd+='?' ;; *) cmd+=$e ;; esac
done
[ -z "$cmd" ] && exit 0

refuse() {  # $1 the rule, $2 the reason and what to do instead
  case "$cmd" in *"WG_GUARD_OK=$1"*) return 0 ;; esac
  echo "Guard $1: $2 If this use is deliberate and approved, put WG_GUARD_OK=$1 in the command." >&2
  exit 2
}

# G4: a heredoc (or here-string) whose text holds a doubled backslash.
case "$cmd" in *"<<"*) after=${cmd#*<<}; case "$after" in *'\\'*)
  refuse G4 "a heredoc holding a doubled backslash; the shell halves it. Write the file with the Write or Edit tool." ;;
esac ;; esac

# "git" where a word starts (also right after a quote mark, a backtick or a slash), then any of its own options that
# stand before the subcommand: -C <dir> (quoted or not), -c <name=value>, --no-pager, --git-dir=<dir>, -P.
ARG="(\"[^\"]*\"|'[^']*'|[^[:space:]]+)"
G="(^|[[:space:]\"'\`/])git(\.exe)?([[:space:]]+(-[Cc][[:space:]]+${ARG}|--[a-z-]+(=${ARG})?|-[Pp]))*[[:space:]]+"
QS="^([^\"']*)(\"[^\"]*\"|'[^']*')(.*)\$"

# G3: a test run and a commit in one command.
if [[ $cmd =~ (unity_tests|shoves)\.sh|-runTests|-batchmode ]] && [[ $cmd =~ ${G}commit([[:space:]]|$) ]]; then
  refuse G3 "a commit chained behind a test run. Read the result first, then commit in a separate command."
fi

# G1 and G2, per simple command (split at ; & | ( ) and line ends).
segs=$(printf '%s\n' "$cmd" | tr ';&|()' '\n\n\n\n\n')
while IFS= read -r seg; do
  seg=${seg//$'\r'/}
  args() {  # $1 a git subcommand of this segment: araw is what follows it, a the same with quote marks as spaces
    [[ $seg =~ ${G}$1([[:space:]]|$) ]] || return 1
    araw=" ${seg#*"${BASH_REMATCH[0]}"} "; a=${araw//[\"\']/ }
  }
  if args add && [[ $a =~ [[:space:]](--all|--update|-[A-Za-z]*[Au][A-Za-z]*|\.|\./|:/|\*)[[:space:]] ]]; then
    refuse G1 "this stages everything at once (it has carried a settings file into a commit). Stage by path: git add <file> ..."
  fi
  if args commit; then
    s=$araw; c=""                                     # flags are read outside quoted text (a message may name a flag)
    while [[ $s =~ $QS ]]; do c+="${BASH_REMATCH[1]} "; s=${BASH_REMATCH[3]}; done
    c+=$s
    if [[ $c =~ [[:space:]](--all|-[A-Za-z]{0,3}a[A-Za-z]{0,3})[[:space:]] ]]; then
      refuse G1 "git commit -a stages everything at once. Stage by path (git add <file>), then git commit without -a."
    fi
  fi
  why="uncommitted work would be discarded (Luis's own uncommitted files live in the main project). Inspect with git status/diff; work in a worktree; ask first."
  if args reset && [[ $a =~ [[:space:]](--hard|--merge)[[:space:]] ]]; then refuse G2 "git reset --hard or --merge: $why"; fi
  if args clean && ! [[ $a =~ [[:space:]](--dry-run|-[A-Za-z]*n[A-Za-z]*)[[:space:]] ]]; then
    refuse G2 "git clean: $why (git clean -n only shows what it would remove.)"
  fi
  if args checkout && [[ $a =~ [[:space:]](--|\.|\./|-f|--force)[[:space:]] ]]; then
    refuse G2 "git checkout with --, . or -f: $why To bring files over from another branch (git checkout <branch> -- <path>), first see with git status that those paths are clean."
  fi
  if args switch && [[ $a =~ [[:space:]](-f|--force|--discard-changes)[[:space:]] ]]; then
    refuse G2 "git switch that discards local changes: $why"
  fi
  if args restore && { [[ $a =~ [[:space:]](--worktree|-W)[[:space:]] ]] || ! [[ $a =~ [[:space:]](--staged|-S)[[:space:]] ]]; }; then
    refuse G2 "git restore without --staged: $why"
  fi
  if args stash; then
    read -r first _ <<< "$a"
    case "$first" in list|show|pop|apply|drop|clear|branch|create|store) ;;
      *) refuse G2 "git stash that saves takes the files out of the working copy: $why" ;; esac
  fi
done <<< "$segs"

# G6: a commit or a push that would carry the profile folder's path. Only the checker can tell (it reads the files, the
# message and the objects); the guard asks it only when the command holds git followed by commit or push.
if [[ $cmd =~ ${G}(commit|push)([[:space:]]|$) ]] && [[ $cmd != *"WG_GUARD_OK=G6"* ]]; then
  here=${0%[/\\]*}; [ "$here" = "$0" ] && here=.      # $0 may come with backslashes (from CLAUDE_PROJECT_DIR) or slashes
  py=$(command -v python 2>/dev/null || command -v py 2>/dev/null)
  if [ -n "$py" ] && [ -f "$here/guard_profile.py" ]; then
    why=$(printf '%s' "$raw" | timeout 8 "$py" -I "$here/guard_profile.py" 2>/dev/null); rc=$?
    why=${why//$'\r'/}; why=${why//$'\n'/ }
    [ $rc -eq 2 ] && [ -n "$why" ] && refuse G6 "$why"
  fi
fi
exit 0
