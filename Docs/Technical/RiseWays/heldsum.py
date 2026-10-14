"""How many tries held the last pose, up off the ground: heldsum.py <bench output>"""
import sys, re
n = up = held = 0; lines = []
for line in open(sys.argv[1], encoding="utf8", errors="replace"):
    m = re.match(r"RISE_HELD (\w+) from (\S+): at pose (\d+) of (\d+), kept .*?, ([\d.]+) s; (\S+) mm within at the end, (\w+)", line)
    if not m: continue
    n += 1
    last = m.group(3) == m.group(4)
    if last and m.group(7) == "up": up += 1
    if last and float(m.group(5)) >= 1: held += 1
    lines.append(f"{m.group(2)}:{m.group(3)}/{m.group(4)},{float(m.group(5)):.1f}s,{m.group(7)}")
wild = sum(1 for l in open(sys.argv[1], encoding="utf8", errors="replace") if l.startswith("RISE_WILD"))
print(f"{n} tries: up at the end {up}, held a second or more {held}, wild {wild} | " + " ".join(lines))
