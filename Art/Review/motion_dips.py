"""How far a traced body's hips ride under their standing height, over the feet it stands on: motion_dips.py <folder> [names...]

Measured over the feet themselves (the body's own place rides over the ground by more in some places than in others),
and only while the knees are not bent on purpose. Made by WonderGather.Tests.MotionTraceBench."""
import csv, sys
from pathlib import Path
folder = Path(sys.argv[1])
names = sys.argv[2:] or [p.stem for p in sorted(folder.glob("*.csv"))]
for name in names:
    rows = list(csv.DictReader(open(folder / f"{name}.csv")))
    if "atL_y" not in rows[0]: print(name, "has no true foot places"); continue
    t0 = next(float(r["t"]) for r in rows if r["order"] != "0")
    def over(r):
        feet = [float(r["at" + s + "_y"]) for s, p in (("L", "plantedL"), ("R", "plantedR")) if r[p] == "1"]
        return float(r["hips_y"]) - (sum(feet) / len(feet)) if feet else None
    stand = over(rows[0])
    data = [(float(r["t"]) - t0, over(r), float(r["speed"]), float(r["sink"])) for r in rows if over(r) is not None and float(r["sink"]) < .01]
    low = min(data, key=lambda d: d[1])
    first = [d for d in data if 0 < d[0] < 2.6]
    lowFirst = min(first, key=lambda d: d[1]) if first else low
    print(f"{name:12s} standing {stand:.3f} m over its feet; lowest in the first 2.6 s {(stand-lowFirst[1])*1000:4.0f} mm under that (at {lowFirst[0]:.2f} s, pace {lowFirst[2]:.2f}); lowest of all {(stand-low[1])*1000:4.0f} mm (at {low[0]:.2f} s, pace {low[2]:.2f})")
