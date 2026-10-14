#!/bin/bash
# The outlook habit (Luis, October 14, 2026: "every now and then to have a general outlook on the project and then
# continue focusing on the current task, since we never want to lose the forest for the trees").
#   outlook.sh start  - at the start of a session, after it is resumed, and after its context is compacted:
#                       reminds to read Docs/Outlook.md and take an outlook if one is due.
#   outlook.sh tick   - after every tool call: counts them, and every EVERY calls reminds that an outlook is due.
#   outlook.sh reset  - after an outlook is written: the count starts again.
# Nothing here blocks anything; it only puts a line in front of the model.
EVERY=250
dir="$(cd "$(dirname "$0")" && pwd)"
count_file="$dir/outlook-count"
# (The hook's JSON on stdin is not needed; it is drained, but never waited on for more than a moment: run by hand,
# with nothing on stdin, a plain read would wait for ever.)
[ "$1" = reset ] || timeout 2 cat > /dev/null 2>&1
case "$1" in
  start)
    msg="Forest for the trees (Luis, October 14): read Docs/Outlook.md first. If its last outlook is older than its cadence says, take a general outlook of the whole project now (the vision, the milestone, every open thread, what is drifting or forgotten), write it there, then go on with the task in hand."
    printf '{"hookSpecificOutput":{"hookEventName":"SessionStart","additionalContext":"%s"}}\n' "$msg"
    ;;
  tick)
    n=0
    [ -f "$count_file" ] && n=$(cat "$count_file" 2>/dev/null)
    case "$n" in ''|*[!0-9]*) n=0 ;; esac
    n=$((n + 1))
    echo "$n" > "$count_file"
    if [ $((n % EVERY)) -eq 0 ]; then
      msg="Forest for the trees: $n tool calls since the last outlook. A general outlook of the whole project is due (how, and the log: Docs/Outlook.md). Finish the step in hand, take the outlook, write it down, run .claude/hooks/outlook.sh reset, then go on."
      printf '{"hookSpecificOutput":{"hookEventName":"PostToolUse","additionalContext":"%s"}}\n' "$msg"
    fi
    ;;
  reset)
    echo 0 > "$count_file"
    ;;
esac
exit 0
