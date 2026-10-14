"""How many tries held the last pose, up off the ground: heldsum.py <bench output>"""
import sys, re
n = up = held = 0; lines = []; furthest = []
for line in open(sys.argv[1], encoding="utf8", errors="replace"):
    m = re.match(r"RISE_HELD (\w+) from (\S+): at pose (\d+) of (\d+), kept .*?, ([\d.]+) s; (\S+) mm within at the end, (\w+)", line)
    if not m: continue
    n += 1
    last = m.group(3) == m.group(4)
    if last and m.group(7) == "up": up += 1
    if last and float(m.group(5)) >= 1: held += 1
    f = re.search(r"furthest foot ([\d.-]+) legs", line)
    furthest.append(float(f.group(1)) if f else 0)
    lines.append(f"{m.group(2)}:{m.group(3)}/{m.group(4)},{float(m.group(5)):.1f}s,{m.group(7)}" + (f",{float(f.group(1)):.2f}" if f else ""))
wild = sum(1 for l in open(sys.argv[1], encoding="utf8", errors="replace") if l.startswith("RISE_WILD"))
mean = sum(furthest) / len(furthest) if furthest else 0
print(f"{n} tries: up at the end {up}, held a second or more {held}, wild {wild}, furthest foot {mean:.2f} legs on average | " + " ".join(lines))
