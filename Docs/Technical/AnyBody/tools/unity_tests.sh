#!/usr/bin/env bash
# unity_tests.sh <name> [filter] [extra Unity args...]
#
# Runs this worktree's PlayMode tests in the editor's batch mode and prints each test's result.
#   name    names the run: the results go to $OUT/<name>_r.xml, the editor's log to $OUT/<name>.log
#   filter  a -testFilter value (classes or tests, ';' between them). "" or "all" runs the whole suite with no
#           filter, so tests marked [Explicit] ("run only when asked") are skipped, as they should be. A capture
#           or bench marked [Explicit] runs only when its own class is named here.
#   extra   passed to Unity as they are (the tests read them: -ownAnatomical 1, -atlasOut <folder>, ...)
#
# OUT defaults to <worktree>/TestRuns (ignored by git). UNITY may name the editor's executable.
#
# Rules paid for (see the assistant's memory "unity-validation-runs"):
#   - nothing heavy beside the whole suite, and no code or test file edited while a run is going;
#   - read the result before saying it, and never chain a commit behind a run;
#   - a run stopped half-way can fail the next launch once ("Scripts have compiler errors"): launch again.
set -u
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$(cd "$HERE/../../../.." && (pwd -W 2>/dev/null || pwd))"
UNITY="${UNITY:-C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe}"
OUT="${OUT:-$PROJECT/TestRuns}"
name="${1:?a name for the run}"; shift
filter="${1:-}"; [ $# -gt 0 ] && shift
mkdir -p "$OUT"
rm -f "$PROJECT"/Assets/InitTestScene* "$OUT/${name}_r.xml"
if [ -z "$filter" ] || [ "$filter" = "all" ]; then
  "$UNITY" -batchmode -projectPath "$PROJECT" -runTests -testPlatform PlayMode -testResults "$OUT/${name}_r.xml" -logFile "$OUT/${name}.log" "$@" > /dev/null 2>&1
else
  "$UNITY" -batchmode -projectPath "$PROJECT" -runTests -testPlatform PlayMode -testFilter "$filter" -testResults "$OUT/${name}_r.xml" -logFile "$OUT/${name}.log" "$@" > /dev/null 2>&1
fi
rm -f "$PROJECT"/Assets/Resources.meta "$PROJECT"/Assets/InitTestScene*
grep -h "error CS" "$OUT/${name}.log" | sort -u | head -10
if [ -f "$OUT/${name}_r.xml" ]; then
  python -I "$HERE/testresults.py" "$OUT/${name}_r.xml"
else
  echo "NO RESULTS: $OUT/${name}_r.xml was not written (see $OUT/${name}.log; a compile error, or the launch after a stopped run)"
fi
