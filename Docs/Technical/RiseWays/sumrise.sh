#!/bin/bash
# What a bench run came to: sumrise.sh <name of its .out>
O="$1.out"
echo "$1: stands $(grep -cE '^RISE Small from [^:]*: stands;' $O) of $(grep -cE '^RISE Small from' $O); turned $(grep -c 'by another way' $O); went wild $(grep -c '^RISE_WILD' $O); $(grep -E '^RISE Small from' $O | grep -oE 'boots turned at most [0-9.]+' | awk '{if ($5>30) b++; if ($5>100) w++; if ($5>m) m=$5} END {print "boots over 30 rad/s:", b+0, "over 100:", w+0, "fastest", m}')"
grep -E '^RISE Small from' $O | grep -oE ': (gave up|went down|stands|not on its feet[^;]*);' | sed -E 's/at [0-9.]+ s//' | sort | uniq -c | sort -rn | head -5
