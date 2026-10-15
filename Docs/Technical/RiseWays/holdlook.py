"""A short line per tick of a bench log (-riseTick 1) in the given stages: what bears it, the weight from the middle,
hips asked and gone, head height, slips. holdlook.py <file.out> <stage,stage> [every]"""
import re, sys
path, stages = sys.argv[1], sys.argv[2].split(",")
every = int(sys.argv[3]) if len(sys.argv) > 3 else 1
k = 0; last = 1e9; tr = -1
for line in open(path, encoding="utf8", errors="replace"):
    if line.startswith(("RISE_HELD", "RISE_FELL", "RISE_WILD")):
        print("  ->", line.strip()[:160]); continue
    m = re.match(r"RISE_GETTING \w+ ([\d.]+) s \| (\w+) ([\d.]+) s: (.*)", line)
    if not m: continue
    t = float(m.group(1))
    if t < last: tr += 1; print(f"== try {tr}")
    last = t
    if m.group(2) not in stages: continue
    k += 1
    if k % every: continue
    s = m.group(4)
    def g(rx, n=1):
        x = re.search(rx, s)
        return " ".join(x.group(i) for i in range(1, n + 1)) if x else "?"
    print(f"{t:5.2f} {m.group(2)} {float(m.group(3)):.2f} | h {g(r'hands ([0-9.]+) ([0-9.]+) knees', 2)} k {g(r'knees ([0-9.]+) ([0-9.]+) boots', 2)}"
          f" | in {g(r'its weight (\S+) mm within')} mid {g(r'weight from their middle (-?\d+) ahead (-?\d+) right', 2)}"
          f" | asked {g(r'hips asked (-?\d+) ahead (-?\d+) right', 2)} gone {g(r'gone (-?\d+) ahead (-?\d+) right', 2)}"
          f" | head {g(r'head ([0-9.]+), hips')} hips {g(r'hips ([0-9.]+) high')} | sh off {g(r'shoulders off (\d+) (\d+)', 2)} hipoff {g(r'hips off (\d+) (\d+)', 2)} | slid {g(r'slid: hands (\S+) (\S+),', 2)}")
