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
#   G7  git's own hooks skipped, moved or changed (November 17): git's pre-commit, commit-msg and pre-push hold the
#       same profile check (.claude/hooks/git/), and only Luis decides to skip them. Refused: git commit --no-verify
#       or a short option cluster holding n (-n, -nm, -an); git push --no-verify (git push -n, a dry run, passes);
#       core.hooksPath in a git command (-c, --config-env, git config other than a read, or the GIT_CONFIG_*
#       variables); a command that writes into, moves or deletes files in .git/hooks (rm, mv, cp, ln, tee, a
#       redirection, sed -i, chmod, touch, Remove-Item, Move-Item, Copy-Item, Set-Content, Add-Content, Out-File,
#       New-Item, Rename-Item and their aliases). Reading passes (ls, cat, grep). Installing goes through
#       .claude/hooks/git/install.sh, whose inside the guard does not see.
#       Mended November 17-18 (g6_git_report.md, "Mend"): flags are compared word by word after the shell's quote
#       removal ("--no-verify", '-n', --no-ver""ify are flags; -m "the -n flag" is one word, a message); also refused:
#       git merge --no-verify; git send-pack and git http-push (they push without running pre-push); git lfs install
#       or update with --force or -f (they overwrite pre-push); a git commit, push or merge in a command that sets
#       GIT_DIR, a GIT_CONFIG* variable, USERPROFILE or HOME, or gives git --git-dir; an alias, include.path or
#       includeIf set with git config or -c, and git config -e/--edit. Passes: git config core.hooksPath with no
#       value (a read); grep, cat, diff, ls and head reading a file in .git/hooks whatever their pattern; cp from
#       .git/hooks to a place outside it.
#       It is a seat belt for the assistant's own ordinary commands: a variable or a $(...) can build any word, and a
#       script, an interpreter, or the Write and Edit tools are not read at all. git's hooks and Luis's review are
#       the protection.
# It reads the command's text, so text that only mentions such a command (a search pattern, an echo, a commit message
# given inline) is refused too: put such text in a file. It does not see inside a script that a command runs.
# A deliberate, approved use passes when the command carries the words WG_GUARD_OK=G1 (or G2, G3, G4, G6, G7).
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
GOPTS="([[:space:]]+(-[Cc][[:space:]]+${ARG}|--git-dir[[:space:]]+${ARG}|--[a-z-]+(=${ARG})?|-[Pp]))*[[:space:]]+"
G="(^|[[:space:]\"'\`/])git(\.exe)?${GOPTS}"
G6LEAD="(^|[[:space:]\"'\`/;&|(])git(\.exe)?${GOPTS}"      # G6's call: also right after ; & | ( and $( (no space)
QS="^([^\"']*)(\"[^\"]*\"|'[^']*')(.*)\$"
# G7: the variables that move git's folder, its config or the profile check's own needle.
SETS="(^|[[:space:];&|(\`])(git_dir|git_config[a-z0-9_]*|userprofile|home)="
PSSETS="\\\$env:(git_dir|git_config[a-z0-9_]*|userprofile|home)[[:space:]]*="
words() {  # $1 a text: W, its shell words after quote removal (' and ", and \ outside single quotes); an open quote
  W=(); local s=$1 i=0 n=${#1} c w="" in=0 q=""   # runs to the end
  while [ $i -lt $n ]; do
    c=${s:i:1}
    if [ -n "$q" ]; then
      if [ "$c" = "$q" ]; then q=""
      elif [ "$q" = '"' ] && [ "$c" = '\' ] && [[ ${s:i+1:1} == [\"\\\$\`] ]]; then i=$((i + 1)); w+=${s:i:1}
      else w+=$c; fi
    else
      case "$c" in
        "'"|'"') q=$c; in=1 ;;
        '\') i=$((i + 1)); w+=${s:i:1}; in=1 ;;
        [[:space:]]) [ $in = 1 ] && W+=("$w"); w=""; in=0 ;;
        *) w+=$c; in=1 ;;
      esac
    fi
    i=$((i + 1))
  done
  [ $in = 1 ] && W+=("$w")
}
noverify() {  # $1 also -n and short clusters holding n (git commit): whether a word of W skips git's hooks
  local w
  for w in "${W[@]}"; do
    [[ $w =~ ^--no-veri(f|fy)?$ ]] && return 0
    [ "$1" = n ] && [[ $w =~ ^-[^-[:space:]mFCctSu]*n[^[:space:]]*$ ]] && return 0
  done
  return 1
}
config_writes() {  # W, the words after git config: 0 when it writes or edits; key, the key it names (lower case)
  local i=0 w pos=() wr=0 rd=0
  key=""; edits=0
  while [ $i -lt ${#W[@]} ]; do
    w=${W[i]}
    case "$w" in
      [0-9]'>'*|[0-9]'<'*|'>'*|'<'*) [[ $w =~ ^[0-9]*[\<\>]+\&?$ ]] && i=$((i + 1)) ;;     # a redirection
      -e|--edit) edits=1 ;;
      --get|--get-all|--get-regexp|--get-urlmatch|--get-color|--get-colorbool|-l|--list) rd=1 ;;
      --add|--replace-all|--unset|--unset-all|--rename-section|--remove-section) wr=1 ;;
      -f|--file|--blob|--type|--default|--comment|--value) i=$((i + 1)) ;;
      -*) ;;
      *) pos+=("${w,,}") ;;
    esac
    i=$((i + 1))
  done
  case "${pos[0]}" in
    get|list) return 1 ;;
    edit) edits=1; return 0 ;;
    set|unset|rename-section|remove-section) key=${pos[1]}; return 0 ;;
  esac
  key=${pos[0]}
  [ $edits = 1 ] || [ $wr = 1 ] && return 0
  [ $rd = 1 ] && return 1
  [ ${#pos[@]} -ge 2 ]
}
# G7: the hooks folder (any case, either slash), a redirection into it, and the commands that write, move or delete.
HOOKS='\.git[/\\]+hooks'
HOOKSREDIR='>[>|]?[[:space:]]*[^[:space:]]*\.git[/\\]+hooks'
WRITERS='(^|[[:space:]/])(rm|rmdir|unlink|mv|cp|ln|install|tee|chmod|touch|truncate|del|erase|rd|move|copy|ren|rename|remove-item|ri|move-item|mi|copy-item|cpi|set-content|sc|add-content|ac|out-file|new-item|ni|rename-item|rni|clear-content|clc)(\.exe)?[[:space:]]'

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
    # G7: -n or --no-verify (or its unique prefixes) skips pre-commit and commit-msg. In a short cluster the n counts
    # until a letter that takes a value (m F C c t S u): -mn is the message "n". Each word after quote removal.
    words "$araw"
    if noverify n; then
      refuse G7 "git commit --no-verify (or -n) skips git's own hooks, which hold the profile check; only Luis decides to skip them."
    fi
  fi
  if args push; then                                  # G7: git push --no-verify skips pre-push (-n is a dry run)
    words "$araw"
    if noverify; then
      refuse G7 "git push --no-verify skips git's own pre-push hook, which holds the profile check; only Luis decides to skip it."
    fi
  fi
  if args merge; then                                 # G7: git merge --no-verify skips commit-msg (-n is --no-stat)
    words "$araw"
    if noverify; then
      refuse G7 "git merge --no-verify skips git's own commit-msg hook, which holds the profile check; only Luis decides to skip it."
    fi
  fi
  if args send-pack || args http-push; then           # G7: they push and run no pre-push hook
    refuse G7 "git send-pack and git http-push push without git's pre-push hook, which holds the profile check; push with git push."
  fi
  if args lfs; then                                   # G7: git lfs install/update --force overwrites git's pre-push hook
    words "$araw"
    if [[ ${W[0]} == install || ${W[0]} == update ]] && [[ " ${W[*]} " == *" --force "* || " ${W[*]} " == *" -f "* ]]; then
      refuse G7 "git lfs ${W[0]} --force overwrites git's pre-push hook, which holds the profile check; only Luis decides that."
    fi
  fi
  # G7: core.hooksPath set or unset for git (it moves git's hooks, and the profile check, away from .git/hooks); an
  # alias or an include (another config file) set, or the config edited in an editor: none is needed here.
  low=${seg,,}
  if [[ $low =~ (^|[[:space:]\"\'\`/])git(\.exe)?[[:space:]] ]]; then
    if [[ $low == *hookspath* ]] && [[ $low =~ (^|[[:space:]])(-c|--config-env)[[:space:]=]*[\"\']?core\.hookspath ]]; then
      refuse G7 "core.hooksPath given to git moves its hooks away from .git/hooks, and the profile check with them; only Luis decides that."
    fi
    if [[ $low =~ (^|[[:space:]])(-c|--config-env)[[:space:]=]*[\"\']?(alias\.|include\.path|includeif\.) ]]; then
      refuse G7 "an alias or an include given to git with -c can run a commit or push past its hooks, or bring in another config; not needed here."
    fi
    if args config; then
      words "$araw"
      if config_writes; then
        if [ $edits = 1 ]; then
          refuse G7 "git config -e opens the config in an editor (it can set core.hooksPath unseen); set one key by name."
        elif [ "$key" = core.hookspath ]; then
          refuse G7 "git config of core.hooksPath moves git's hooks away from .git/hooks, and the profile check with them; only Luis decides that."
        elif [[ $key =~ ^(alias|include|includeif)(\.|$) ]]; then
          refuse G7 "git config of an alias or an include can run a commit or push past its hooks, or bring in another config; not needed here."
        fi
      fi
    fi
  fi
  # G7: a file in .git/hooks written, moved or deleted (where git's own profile check lives). The path may be quoted;
  # the verb is looked for outside quoted text (grep "rm " .git/hooks/x only reads).
  if [[ $low =~ $HOOKS ]]; then
    s=$low; u=" "                                     # quoted text kept as one word: blanks to _, no > or |
    while [[ $s =~ $QS ]]; do
      q=${BASH_REMATCH[2]}; q=${q:1:${#q}-2}; q=${q//[[:space:]]/_}; q=${q//[>|]/}
      u+="${BASH_REMATCH[1]}$q"; s=${BASH_REMATCH[3]}
    done
    u+="$s "
    # Reads pass: grep, cat, diff, ls or head (whatever their pattern), and cp from .git/hooks to a place outside it.
    reads=0
    [[ $u =~ ^[[:space:]]*(grep|cat|diff|ls|head)(\.exe)?[[:space:]] ]] && reads=1
    if [[ $u =~ ^[[:space:]]*cp(\.exe)?[[:space:]] ]]; then
      words "$seg"; dest=""; t=0
      for w in "${W[@]:1}"; do
        case "$w" in -t|--target-directory*|-[a-z]*t) t=1 ;; -*) ;; *) dest=$w ;; esac
      done
      [ $t = 0 ] && [ -n "$dest" ] && ! [[ ${dest,,} =~ $HOOKS ]] && reads=1
    fi
    if [[ $u =~ $HOOKSREDIR ]] || { [ $reads = 0 ] && [[ $u =~ $WRITERS ]]; } || { [[ $u =~ (^|[[:space:]/])sed(\.exe)?[[:space:]] ]] && [[ $u =~ [[:space:]](-[a-z]*i|--in-place)([[:space:]=]|$) ]]; }; then
      refuse G7 "this writes into, moves or deletes files in .git/hooks, where git's own profile check lives; only Luis decides that (install with .claude/hooks/git/install.sh)."
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

# G7: core.hooksPath through git's own environment (GIT_CONFIG_KEY_<n>, GIT_CONFIG_PARAMETERS), set anywhere in the
# command (a prefix, export, or PowerShell's $env:).
if [[ ${cmd,,} == *git_config* ]] && [[ ${cmd,,} == *hookspath* ]]; then
  refuse G7 "core.hooksPath given through GIT_CONFIG_* moves git's hooks away from .git/hooks, and the profile check with them; only Luis decides that."
fi
# G7: a commit, push or merge in a command that sets GIT_DIR, a GIT_CONFIG* variable, USERPROFILE or HOME (bash, cmd's
# set, PowerShell's $env:), or gives git --git-dir: another repository's hooks, another config, another needle.
if [[ $cmd =~ ${G6LEAD}(commit|push|merge)([[:space:]]|$) ]]; then
  if [[ ${BASH_REMATCH[0]} == *--git-dir* ]] || [[ ${cmd,,} =~ $SETS ]] || [[ ${cmd,,} =~ $PSSETS ]]; then
    refuse G7 "a git commit, push or merge with GIT_DIR, --git-dir, GIT_CONFIG*, USERPROFILE or HOME set in the same command runs past this repository's hooks or changes what they look for; run it plainly."
  fi
fi

# G6: a commit or a push that would carry the profile folder's path. Only the checker can tell (it reads the files, the
# message and the objects); the guard asks it only when the command holds git followed by commit or push (also right
# after ; & | ( or $( with no space).
if [[ $cmd =~ ${G6LEAD}(commit|push)([[:space:]]|$) ]] && [[ $cmd != *"WG_GUARD_OK=G6"* ]]; then
  here=${0%[/\\]*}; [ "$here" = "$0" ] && here=.      # $0 may come with backslashes (from CLAUDE_PROJECT_DIR) or slashes
  py=$(command -v python 2>/dev/null || command -v py 2>/dev/null)
  if [ -n "$py" ] && [ -f "$here/guard_profile.py" ]; then
    why=$(printf '%s' "$raw" | timeout 8 "$py" -I "$here/guard_profile.py" 2>/dev/null); rc=$?
    why=${why//$'\r'/}; why=${why//$'\n'/ }
    [ $rc -eq 2 ] && [ -n "$why" ] && refuse G6 "$why"
  fi
fi
exit 0
