"""When each try first lost its balance in the getting up: failwhen.py <bench output> [stages to skip, by commas]"""
import sys, re
tries = {}
skip = set((sys.argv[2] if len(sys.argv) > 2 else 'hands,under').split(','))
order = []
for line in open(sys.argv[1], encoding="utf8", errors="replace"):
    m = re.match(r"RISE_GETTING (\w+) ([\d.]+) s \| (\w+) ([\d.]+) s: on (\d+) \(hands ([\d.]+) ([\d.]+) knees ([\d.]+) ([\d.]+) boots ([\d.]+) ([\d.]+)\), its weight (\S+) mm within them; moving off leg (-?\d) hand (-?\d) \(giving it ([\d.]+)\); moved off (\d+) times, lifted all the same (\d+)", line)
    if not m: continue
    who, t = m.group(1), float(m.group(2))
    key = who
    if key not in tries or t < tries[key][-1][0]:
        n = sum(1 for k in order if k.startswith(who + "#"))
        key2 = f"{who}#{n}"; order.append(key2); tries[who] = []; tries[key2] = tries[who]
    tries[who].append((t, m.group(3), float(m.group(4)), int(m.group(5)), float(m.group(12)) if m.group(12) != "NaN" else float("nan"), int(m.group(13)), int(m.group(14)), int(m.group(16)), int(m.group(17)), sum(float(m.group(k)) for k in range(6, 12))))
for key in order:
    rows = tries[key]
    lost = None
    for i, r in enumerate(rows):
        t, stage, got, on, depth, leg, hand, moved, failed, total = r
        if stage in skip: continue
        if on <= 1 or (depth == depth and depth < -60):
            lost = r; break
    if lost is None:
        print(f"{key}: kept its balance; last {rows[-1][1]} on {rows[-1][3]}, {rows[-1][4]:.0f} mm within, moved off {rows[-1][7]}, all the same {rows[-1][8]}")
    else:
        t, stage, got, on, depth, leg, hand, moved, failed, total = lost
        phase = "moving off/lifting" if leg >= 0 or hand >= 0 else "after"
        print(f"{key}: lost it at {t:.2f} s, {stage} {got:.2f} s ({phase} leg {leg} hand {hand}), on {on}, {depth:.0f} mm within, moved off {moved}, all the same {failed}")
