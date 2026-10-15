"""How far its hips went (since what bears it was kept) by the time the limb was lifted, per try, and the median.
shiftgone.py <file.out>"""
import re, sys, statistics as st
tries = []; last = 1e9
for l in open(sys.argv[1], encoding="utf8", errors="replace"):
    m = re.match(r"RISE_GETTING \w+ ([\d.]+) s \| (\w+) ([\d.]+) s: (.*)", l)
    if not m: continue
    t = float(m.group(1))
    if t < last: tries.append(None)
    last = t
    if tries[-1] is not None: continue
    s = m.group(4)
    mo = re.search(r"moved off (\d+) times, lifted all the same (\d+)", s)
    g = re.search(r"gone (-?\d+) ahead (-?\d+) right", s)
    if mo and g and (int(mo.group(1)) or int(mo.group(2))): tries[-1] = (int(g.group(1)), int(g.group(2)))
got = [x for x in tries if x]
side = [abs(x[1]) for x in got]; whole = [(x[0] ** 2 + x[1] ** 2) ** .5 for x in got]
print(f"{len(got)} lifts: hips gone sideways median {st.median(side):.0f} mm, in all median {st.median(whole):.0f} mm")
