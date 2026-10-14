"""Kinds of failure in a bench output: failkinds.py <bench output> [stages to skip]"""
import sys, re, collections, subprocess
skip = sys.argv[2] if len(sys.argv) > 2 else "hands,under,prop"
out = subprocess.run([sys.executable, __file__.replace("failkinds.py", "failwhen.py"), sys.argv[1], skip], capture_output=True, text=True).stdout
c = collections.Counter()
for l in out.splitlines():
    if "kept" in l: c["kept"] += 1; continue
    m = re.search(r"(\w+) ([\d.]+) s \((moving off/lifting|after) leg (-?\d) hand (-?\d).*moved off (\d+)", l)
    if not m: c["?"] += 1; continue
    st, got, ph, leg, hand, moved = m.groups()
    if st in ("prop", "stay"): k = "before the step (" + st + ")"
    elif ph == "after" and moved != "0": k = st + ": after landing"
    elif moved == "0": k = st + ": moving weight off"
    else: k = st + ": limb swinging"
    c[k] += 1
print(sys.argv[1], dict(c))
