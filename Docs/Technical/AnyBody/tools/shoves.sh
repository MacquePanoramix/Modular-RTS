#!/usr/bin/env bash
# shoves.sh <name> [extra Unity args...]
#
# The shove test that is "not yet met" (OwnBodyTests.ShovedAgainAndAgainNothingOfItFlies: three miners, shoved three
# ways) on its own, and what it found: each course's line, how often a leg was held back as unbraced, and how often a
# joint was given less for its speed.
# Keeper settings to compare go in the tests' own argument, for example:
#   bash shoves.sh base
#   bash shoves.sh braced -ownStepSet Braced=0.03
#   bash shoves.sh byspeed -ownStepSet BySpeed=1
# With -ownShoveTrace 1 the steps before a part flies are kept too (each leg part, the boots, and what the body holds
# and was given, in joules): the lines after "OWN_SHOVE_TRACE" in TestRuns/<name>.trace.txt, one step a line.
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$(cd "$HERE/../../../.." && (pwd -W 2>/dev/null || pwd))"
OUT="${OUT:-$PROJECT/TestRuns}"
name="${1:?a name for the run}"; shift
bash "$HERE/unity_tests.sh" "$name" "WonderGather.Tests.OwnBodyTests.ShovedAgainAndAgainNothingOfItFlies" "$@" | cut -c1-200
grep -h -o "OWN_SHOVES[[:print:]]*" "$OUT/$name.log" | sed -E 's/; its highest part.*//' | cut -c1-230
grep -h -o "OWN_UNBRACED[[:print:]]*" "$OUT/$name.log"
grep -h -o "OWN_SLOWED[[:print:]]*" "$OUT/$name.log"
grep -h -o -E "OWN_SHOVE_(TRACE|PARTS)[[:print:]]*" "$OUT/$name.log" | sed 's# // #\n#g' > "$OUT/$name.trace.txt"
