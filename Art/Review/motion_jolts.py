"""When each part of a traced body jolts, in runs of frames: motion_jolts.py <csv> [limit mm=4] [parts: hips head handL handR ...]

A jolt: how much a part's change of place from one frame to the next itself changes (mm). Times are counted from
the first order of the trace. Made by WonderGather.Tests.MotionTraceBench; see Docs/ArtDirection/MotionJudging.md."""
import csv, math, sys
rows = list(csv.DictReader(open(sys.argv[1])))
limit = float(sys.argv[2]) if len(sys.argv) > 2 else 4
parts = sys.argv[3:] or ["hips", "head", "handL", "handR"]
t0 = next((float(r["t"]) for r in rows if r["order"] != "0"), float(rows[0]["t"]))
P = lambda r, a: (float(r[a + "_x"]), float(r[a + "_y"]), float(r[a + "_z"]))
for part in parts:
    worst = []
    for i in range(2, len(rows)):
        a, b, c = P(rows[i - 2], part), P(rows[i - 1], part), P(rows[i], part)
        j = math.dist([c[k] - b[k] for k in range(3)], [b[k] - a[k] for k in range(3)]) * 1000
        worst.append((j, float(rows[i]["t"]) - t0, rows[i]["order"], rows[i]["holdsL"] + rows[i]["holdsR"], rows[i]["bow"], rows[i]["sink"]))
    over = sorted((w for w in worst if w[0] > limit), key=lambda w: w[1])
    print(f"{part}: {len(over)} frames over {limit:g} mm; the largest {max(w[0] for w in worst):.1f} mm")
    runs = []
    for w in over:
        if runs and w[1] - runs[-1][-1][1] < .06: runs[-1].append(w)
        else: runs.append([w])
    for run in runs:
        m = max(run)
        print(f"   {run[0][1]:6.2f} to {run[-1][1]:6.2f} s  {len(run):3d} frames, worst {m[0]:5.1f} mm at {m[1]:.2f} s (order {m[2]}, holds {m[3]}, bow {float(m[4]):.0f}, sink {float(m[5]):.3f})")
