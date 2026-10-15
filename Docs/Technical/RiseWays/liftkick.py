"""Per try: the largest error of the lifted leg's thigh (along its length / in all) in the first 0.6 s after the lift,
and the try's end. liftkick.py <file.out>"""
import re, sys
tries = []; last = 1e9
for l in open(sys.argv[1], encoding="utf8", errors="replace"):
    m = re.match(r"RISE_GETTING \w+ ([\d.]+) s \| (\w+) ([\d.]+) s: (.*)", l)
    if m:
        t = float(m.group(1))
        if t < last: tries.append({"rows": [], "end": ""})
        last = t; tries[-1]["rows"].append((t, m.group(2), m.group(4))); continue
    m = re.match(r"RISE_HELD \w+ from (\S+): .*? (\w+); furthest foot ([\d.-]+)", l)
    if m:
        for tr in tries:
            if not tr["end"]: tr["end"] = f"{m.group(1)} {m.group(2)} {m.group(3)}"; break
kicks = 0
for k, tr in enumerate(tries):
    lift = None; worst = (0, 0); leg = None
    for t, st, s in tr["rows"]:
        mo = re.search(r"moved off (\d+) times, lifted all the same (\d+)", s)
        if mo and lift is None and (int(mo.group(1)) or int(mo.group(2))):
            lift = t; ol = re.search(r"leg (-?\d+) hand", s); leg = int(ol.group(1)) if ol else None
        if lift is not None and t <= lift + .6 and leg is not None and leg >= 0:
            th = re.search(r"thighs off \(along/all\): (\d+)/(\d+) (\d+)/(\d+)", s)
            if th:
                a, b = int(th.group(1 + 2 * leg)), int(th.group(2 + 2 * leg))
                if b > worst[1]: worst = (a, b)
    if worst[1] > 90: kicks += 1
    print(f"try {k}: lift {lift}, leg {leg}, thigh off {worst[0]}/{worst[1]}, end {tr['end']}")
print("lifts with a thigh more than 90 degrees off:", kicks)
