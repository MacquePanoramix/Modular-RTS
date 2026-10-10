"""A stretch of a trace frame by frame: motion_frames.py <csv> <from s> <to s>

How far the body's place, its hips (each way), its head and each hand move in each frame (mm); which feet are down and
how far through its swing each is; the bow, the knees, what the hands hold, the gait. Times from the first order."""
import csv, math, sys
rows = list(csv.DictReader(open(sys.argv[1])))
a, b = float(sys.argv[2]), float(sys.argv[3])
t0 = next((float(r["t"]) for r in rows if r["order"] != "0"), float(rows[0]["t"]))
prev = None
print("    t   root   hips(dx,dy,dz)        head   handL  handR  pl sw            bow   sink  holds gait")
for r in rows:
    t = float(r["t"]) - t0
    if prev is not None and a <= t <= b:
        d = lambda p: math.dist(*[(float(x[p + "_x"]), float(x[p + "_y"]), float(x[p + "_z"])) for x in (r, prev)]) * 1000
        h = [(float(r["hips_" + k]) - float(prev["hips_" + k])) * 1000 for k in "xyz"]
        print(f"{t:6.2f} {d('root'):5.1f}  ({h[0]:6.1f},{h[1]:6.1f},{h[2]:6.1f})  {d('head'):5.1f}  {d('handL'):5.1f}  {d('handR'):5.1f}  {r['plantedL']}{r['plantedR']} {float(r['swingL']):5.2f} {float(r['swingR']):5.2f}  {float(r['bow']):5.1f} {float(r['sink']):6.3f}  {r['holdsL']}{r['holdsR']}  {r['gait']}")
    prev = r
