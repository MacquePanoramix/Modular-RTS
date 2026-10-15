"""Per try of a bench log with -riseTick 1: in a stage, whether its weight was moved off the leg before lifting it
(moved off) or the leg was lifted all the same, when, the least 'to go' while shifting, and how the try ended.
shiftsum.py <file.out> [stage]"""
import re, sys
path = sys.argv[1]; stage = sys.argv[2] if len(sys.argv) > 2 else "footA"
tries, ends = [], []
last = 1e9
for line in open(path, encoding="utf8", errors="replace"):
    if line.startswith("RISE_HELD") or line.startswith("RISE_FELL") or line.startswith("RISE_WILD"):
        ends.append(line.strip()[:150]); continue
    m = re.match(r"RISE_GETTING \w+ ([\d.]+) s \| (\w+) ([\d.]+) s: (.*)", line)
    if not m: continue
    t = float(m.group(1))
    if t < last: tries.append([])
    last = t
    tries[-1].append((t, m.group(2), float(m.group(3)), m.group(4)))
moved_all = lifted_all = 0
for k, tr in enumerate(tries):
    rows = [r for r in tr if r[1] == stage]
    if not rows:
        print(f"try {k}: never at {stage}"); continue
    first_moved = first_lifted = None; least = 1e9; leg = None
    for t, st, got, s in rows:
        mo = re.search(r"moved off (\d+) times, lifted all the same (\d+)", s)
        tg = re.search(r"to go (-?\d+) ahead (-?\d+) right", s)
        lg = re.search(r"moving off leg (-?\d+)|leg (-?\d+) hand", s)
        off = re.search(r"leg (-?\d+) hand", s)
        if off and int(off.group(1)) >= 0: leg = int(off.group(1))
        if mo:
            if int(mo.group(1)) > 0 and first_moved is None: first_moved = got
            if int(mo.group(2)) > 0 and first_lifted is None: first_lifted = got
        if tg and off and int(off.group(1)) >= 0 and first_moved is None and first_lifted is None:
            least = min(least, (int(tg.group(1)) ** 2 + int(tg.group(2)) ** 2) ** .5)
    if first_moved is not None: moved_all += 1
    elif first_lifted is not None: lifted_all += 1
    how = f"moved off at {first_moved:.1f} s" if first_moved is not None else (f"lifted all the same at {first_lifted:.1f} s" if first_lifted is not None else "neither")
    print(f"try {k}: leg {leg}, {how}, least to go {least:.0f} mm")
print(f"moved off {moved_all}, lifted all the same {lifted_all}, of {len(tries)}")
for e in ends: print(" ", e)
