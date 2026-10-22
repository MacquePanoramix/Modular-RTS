"""Where the weight was when the hands let go, and when it was handed to its standing balance, by how each try ended:
release.py <bench .out> [more .out]"""
import re, sys
from collections import defaultdict

rows = []
for path in sys.argv[1:]:
    for line in open(path, encoding="utf8", errors="replace"):
        if not re.match(r"RISE \w+ from ", line):
            continue
        ended = re.match(r"RISE \w+ from [^:]*: ([^;]*);", line).group(1)
        ended = "stands" if ended == "stands" else "went down" if ended == "went down" else "gave up" if ended == "gave up" else "time out"
        row = {"ended": ended}
        for tag in ("RELEASED", "HANDED"):
            m = re.search(tag + r" ([^;]*?) at ([\d.]+) s of (\w*): weight (-?[\d.]+|-?Infinity) mm within, heading (-?[\d.]+|-?Infinity) mm within; moving ([\d.]+) m/s \((-?[\d.]+) up\); hands bore ([\d.]+), boots ([\d.]+)", line)
            if m:
                row[tag] = dict(how=m.group(1), weight=float(m.group(4)), heading=float(m.group(5)), moving=float(m.group(6)), up=float(m.group(7)), hands=float(m.group(8)), boots=float(m.group(9)))
        rows.append(row)

def show(tag):
    print(f"--- {tag}")
    groups = defaultdict(list)
    for r in rows:
        if tag in r: groups[r["ended"]].append(r[tag])
    for ended, items in sorted(groups.items()):
        n = len(items)
        def mean(k): return sum(i[k] for i in items) / n
        over = sum(1 for i in items if "over" in i["how"]) if tag == "RELEASED" else None
        inside_w = sum(1 for i in items if i["weight"] > 0)
        inside_h = sum(1 for i in items if i["heading"] > 0)
        print(f"{ended:10s} {n:3d} tries | weight within {mean('weight'):6.0f} mm (inside in {inside_w}), heading within {mean('heading'):6.0f} mm (inside in {inside_h}); "
              f"moving {mean('moving'):.2f} m/s; hands bore {mean('hands'):.2f}, boots {mean('boots'):.2f}" + (f"; weight over the rest first in {over}" if over is not None else ""))
    for ended, items in sorted(groups.items()):
        print(f"  {ended}: weight/heading mm " + ", ".join(f"{i['weight']:.0f}/{i['heading']:.0f}" for i in items))

print(f"{len(rows)} tries; ended: " + ", ".join(f"{k} {sum(1 for r in rows if r['ended'] == k)}" for k in ("stands", "went down", "gave up", "time out")))
print(f"hands let go in {sum(1 for r in rows if 'RELEASED' in r)}; handed over in {sum(1 for r in rows if 'HANDED' in r)}")
show("RELEASED")
show("HANDED")
