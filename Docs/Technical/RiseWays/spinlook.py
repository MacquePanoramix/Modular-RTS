"""Around each boot whip a bench told (-riseSpinTrace): spinlook.py <log> [events] [before] [after]"""
import sys, re
log = sys.argv[1]; many = int(sys.argv[2]) if len(sys.argv) > 2 else 3
before = int(sys.argv[3]) if len(sys.argv) > 3 else 6; after = int(sys.argv[4]) if len(sys.argv) > 4 else 6
events, cur, mode = [], None, None
for line in open(log, encoding="utf8", errors="replace"):
    if line.startswith("RISE_SPIN "):
        if mode != "spin": cur = {"spin": [], "after": []}; events.append(cur); mode = "spin"
        cur["spin"].append(line)
    elif line.startswith("RISE_SPIN_AFTER "):
        mode = "after"; cur["after"].append(line)
def short(l):
    m = re.search(r"Small try (\d+) ([\d.]+) s", l); t = m.group(1) + ":" + m.group(2)
    b = re.search(r"boots turning (.*) rad/s \(their own axes\)", l).group(1)
    b = re.sub(r" \(-?[\d.]+, -?[\d.]+, -?[\d.]+\) ankle", " ankle", b)
    st = re.search(r"\| ([a-zA-Z]+ [\d.]+ s): on (\d+) \((hands [^)]*)\)", l)
    return f"{t} | {b} | {st.group(1) if st else ''} {st.group(3) if st else ''}"
for e in events[:many]:
    print("----")
    for l in e["spin"][-before:]: print(short(l))
    for l in e["after"][:after]: print("  " + short(l))
