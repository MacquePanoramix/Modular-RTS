"""How it stands on hands and knees near the end of a stage (default stay), over all tries: head and hips height,
knees bent, what bears it. stance.py <file.out> [stage] [at s]"""
import re, sys, statistics as st
f = sys.argv[1]; stage = sys.argv[2] if len(sys.argv) > 2 else "stay"; at = float(sys.argv[3]) if len(sys.argv) > 3 else .6
rows = []
for l in open(f, encoding="utf8", errors="replace"):
    m = re.match(r"RISE_GETTING \w+ ([\d.]+) s \| (\w+) ([\d.]+) s: (.*)", l)
    if not m or m.group(2) != stage or abs(float(m.group(3)) - at) > .101: continue
    s = m.group(4)
    hd = re.search(r"head ([0-9.]+), hips ([0-9.]+)", s); kn = re.search(r"knees (\d+) (\d+) \(off", s)
    b = re.search(r"\(hands ([0-9.]+) ([0-9.]+) knees ([0-9.]+) ([0-9.]+) boots ([0-9.]+) ([0-9.]+)\)", s); el = re.search(r"elbows (\d+) (\d+)", s)
    if hd and kn and b:
        rows.append([float(hd.group(1)), float(hd.group(2)), (int(kn.group(1)) + int(kn.group(2))) / 2, float(b.group(1)) + float(b.group(2)), float(b.group(3)) + float(b.group(4)), float(b.group(5)) + float(b.group(6)), (int(el.group(1)) + int(el.group(2))) / 2 if el else -1])
if rows:
    cols = list(zip(*rows))
    names = ["head", "hips", "knees bent", "hands bear", "knees bear", "boots bear", "elbows bent"]
    print(f"{len(rows)} tries at {stage} {at} s: " + ", ".join(f"{n} {st.median(c):.2f}" for n, c in zip(names, cols)))
