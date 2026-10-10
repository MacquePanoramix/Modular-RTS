"""The breaks in a movement: where a part of a body changes place between two frames by more than a body can.

    python motion_breaks.py <trace.csv> [more.csv ...] [--list 6] [--plot out.png]

Reads the traces written by MotionTraceBench (one line a frame, taken after the body is posed) and reports, for each:

  hips      the largest change of height in one frame, and the largest *break* (the change of that change: what the
            eye sees as a jolt). A break of more than BREAK millimetres between frames a hundredth of a second apart
            is an acceleration of more than four times gravity: no body does that without being struck.
  feet      how far a planted foot moves over the ground; how far a foot jumps in the frame it lands; any frame in
            which a foot moves faster than a foot can, or turns on the ground while it bears weight.
  head      the same breaks, for the head.
  turning   how fast the body, hips, chest and head turn, and which begins first.
  steps     how far the hips rise and fall in a step (a person's rise and fall about 4 to 5 cm, in one smooth wave).

It prints PASS or FAIL against the limits below. They are a first setting: a judge's eye and Luis's come after.
"""
import csv
import sys
from pathlib import Path

import numpy as np

BREAK = 4.0          # mm: the most the change from frame to frame may itself change, for hips and head
FOOT_BREAK = 12.0    # mm: the same for a foot in the air (it is light and quick, and lands)
SLIDE = 1.0          # mm a frame: the most a planted foot may move over the ground
LANDING = 6.0        # mm: the most a foot may move in the frame it lands, beyond what it was already moving
PIVOT = 1.0          # degrees a frame: the most a planted foot may turn on the ground


def column(rows, key):
    return np.array([float(r[key]) for r in rows])


def unwrap(deg):
    return np.degrees(np.unwrap(np.radians(deg)))


def analyse(path, listed=6):
    rows = list(csv.DictReader(open(path)))
    if len(rows) < 10:
        return None
    t = column(rows, "t")
    dt = float(np.median(np.diff(t)))
    out = {"name": Path(path).stem, "frames": len(rows), "rate": round(1 / dt), "fails": []}
    planted = [column(rows, "plantedL") > .5, column(rows, "plantedR") > .5]
    steps = column(rows, "steps")
    out["steps"] = int(steps[-1] - steps[0])

    def breaks(prefix):
        p = np.stack([column(rows, prefix + "_x"), column(rows, prefix + "_y"), column(rows, prefix + "_z")], 1) * 1000
        d = np.diff(p, axis=0)                      # mm a frame
        dd = np.diff(d, axis=0)                     # the change of that
        return p, d, dd

    # What happened at a frame: which foot landed or left the ground.
    def event(i):
        said = []
        for f, side in enumerate("LR"):
            for j in range(max(1, i - 1), min(len(rows), i + 2)):
                if planted[f][j] and not planted[f][j - 1]: said.append(side + " landed")
                if not planted[f][j] and planted[f][j - 1]: said.append(side + " left")
        return ", ".join(dict.fromkeys(said)) or "-"

    worst = []
    for part, limit in (("hips", BREAK), ("head", BREAK)):
        p, d, dd = breaks(part)
        up = dd[:, 1]
        along = np.linalg.norm(dd[:, [0, 2]], axis=1)
        out[part + "_drop"] = float(np.abs(d[:, 1]).max())
        out[part + "_break_up"] = float(np.abs(up).max())
        out[part + "_break_along"] = float(along.max())
        bad = np.where((np.abs(up) > limit) | (along > limit))[0]
        out[part + "_breaks"] = int(len(bad))
        if len(bad):
            out["fails"].append(f"{part}: {len(bad)} breaks of more than {limit:.0f} mm (the worst {max(np.abs(up).max(), along.max()):.1f} mm)")
        for i in np.argsort(-np.maximum(np.abs(up), along))[:listed]:
            worst.append((max(abs(up[i]), along[i]), f"{part} at {t[i + 1]:.2f} s: its change of place changed by {up[i]:+.1f} mm up, {along[i]:.1f} mm along ({event(i + 1)})"))

    for f, side in enumerate(("footL", "footR")):
        p, d, dd = breaks(side)
        over = np.linalg.norm(d[:, [0, 2]], axis=1)
        both = planted[f][1:] & planted[f][:-1]
        # A planted foot rolls: its heel comes down after it lands and rises before it leaves, and the foot's own
        # place moves with that. It is held to the ground only while it lies flat.
        key = "pitch" + side[-1]
        flat = both & (np.abs(column(rows, key))[1:] < 1.5) & (np.abs(column(rows, key))[:-1] < 1.5) if key in rows[0] else both
        slide = over[flat].max() if flat.any() else 0
        out[side + "_slide"] = float(slide)
        if slide > SLIDE:
            out["fails"].append(f"{side}: planted, it moved {slide:.1f} mm over the ground in one frame")
        lands = np.where(planted[f][1:] & ~planted[f][:-1])[0]          # d[i] is the move into frame i+1, the landing frame
        # A foot coming down is slowing: in the frame it lands it moves less than in the one before. If it moves
        # more, or another way, it was put somewhere it was not going.
        jump = 0.0
        for i in lands:
            if i < 1: continue
            before = d[i - 1]
            along = before / max(1e-6, float(np.linalg.norm(before)))
            more = max(0.0, float(np.dot(d[i], along) - np.linalg.norm(before)))
            aside = float(np.linalg.norm(d[i] - along * np.dot(d[i], along)))
            jump = max(jump, more + aside)
        out[side + "_landing"] = jump
        if jump > LANDING:
            out["fails"].append(f"{side}: in the frame it landed it jumped {jump:.1f} mm from the way it was going")
        air = ~planted[f][2:] & ~planted[f][1:-1] & ~planted[f][:-2]
        flight = float(np.linalg.norm(dd, axis=1)[air].max()) if air.any() else 0
        # Leaving the ground: the change in the first frame off it.
        leaves = np.where(~planted[f][2:] & planted[f][1:-1])[0]
        out[side + "_leaving"] = float(np.linalg.norm(dd, axis=1)[leaves].max()) if len(leaves) else 0
        if out[side + "_leaving"] > FOOT_BREAK:
            out["fails"].append(f"{side}: leaving the ground its change of place changed by {out[side + '_leaving']:.1f} mm in one frame")
        out[side + "_air"] = flight
        if flight > FOOT_BREAK:
            out["fails"].append(f"{side}: in the air its change of place changed by {flight:.1f} mm in one frame")
        yaw = unwrap(column(rows, side + "_yaw"))
        turn = np.abs(np.diff(yaw))
        pivot = float(turn[both].max()) if both.any() else 0
        out[side + "_pivot"] = pivot
        out[side + "_pivot_total"] = float(turn[both].sum()) if both.any() else 0
        if pivot > PIVOT:
            out["fails"].append(f"{side}: planted, it turned {pivot:.1f} degrees on the ground in one frame")
        speed = np.linalg.norm(d, axis=1) / 1000 / dt
        out[side + "_fastest"] = float(speed.max())

    # The rise and fall of the hips over a step, over the ground it stands on.
    hips = column(rows, "hips_y") - column(rows, "root_y")
    moving = column(rows, "speed") > .3
    if moving.sum() > 50:
        h = hips[moving] * 1000
        out["rise_fall"] = float(np.percentile(h, 97) - np.percentile(h, 3))

    # Turning: how fast each part turns, and when each has turned a tenth of the whole way.
    turns = {}
    for part in ("root", "hips", "chest", "head"):
        yaw = unwrap(column(rows, part + "_yaw"))
        turns[part] = yaw
        out[part + "_turn_fastest"] = float(np.abs(np.diff(yaw)).max() / dt)
    whole = turns["root"][-1] - turns["root"][0]
    out["turned"] = float(whole)
    if abs(whole) > 60:
        began = {}
        for part, yaw in turns.items():
            gone = (yaw - yaw[0]) / whole
            first = np.argmax(gone > .1)
            last = np.argmax(gone > .9)
            began[part] = (t[first] - t[0], t[last] - t[0])
        out["turn_began"] = began
    out["worst"] = [w for _, w in sorted(worst, key=lambda x: -x[0])[:listed]]
    return out


def report(o):
    print(f"== {o['name']}: {o['frames']} frames at {o['rate']} a second, {o['steps']} steps")
    print(f"   hips: at most {o['hips_drop']:.1f} mm up or down in a frame; breaks: {o['hips_break_up']:.1f} mm up, {o['hips_break_along']:.1f} mm along ({o['hips_breaks']} over the limit)")
    print(f"   head: breaks: {o['head_break_up']:.1f} mm up, {o['head_break_along']:.1f} mm along ({o['head_breaks']} over the limit)")
    for side in ("footL", "footR"):
        print(f"   {side}: planted it moves at most {o[side + '_slide']:.2f} mm a frame and turns {o[side + '_pivot']:.2f} degrees a frame ({o[side + '_pivot_total']:.0f} in all); "
              f"landing jump {o[side + '_landing']:.1f} mm; leaving {o[side + '_leaving']:.1f} mm; in the air {o[side + '_air']:.1f} mm; fastest {o[side + '_fastest']:.1f} m/s")
    if "rise_fall" in o:
        print(f"   the hips rise and fall {o['rise_fall']:.0f} mm over the ground while it walks")
    if "turn_began" in o:
        b = o["turn_began"]
        print(f"   turned {o['turned']:.0f} degrees; a tenth of the way, and nine tenths, after: " + ", ".join(f"{k} {v[0]:.2f} s and {v[1]:.2f} s" for k, v in b.items()))
        print(f"   fastest turning: " + ", ".join(f"{k} {o[k + '_turn_fastest']:.0f} deg/s" for k in ("root", "hips", "chest", "head")))
    for w in o["worst"]:
        print("     " + w)
    print("   " + ("PASS" if not o["fails"] else "FAIL: " + "; ".join(o["fails"])))


def plot(path, traces):
    from PIL import Image, ImageDraw
    W, H, pad = 1500, 260, 34
    im = Image.new("RGB", (W, H * len(traces)), "white")
    d = ImageDraw.Draw(im)
    for n, src in enumerate(traces):
        rows = list(csv.DictReader(open(src)))
        t = column(rows, "t")
        t = t - t[0]
        lines = [("hips over the ground", (column(rows, "hips_y") - column(rows, "root_y")) * 1000, (200, 40, 40)),
                 ("left foot", (column(rows, "footL_y") - column(rows, "root_y")) * 1000, (40, 110, 200)),
                 ("right foot", (column(rows, "footR_y") - column(rows, "root_y")) * 1000, (40, 160, 90))]
        top = n * H
        d.text((pad, top + 4), f"{Path(src).stem}: height over the ground it stands on (mm), frame by frame", fill=(0, 0, 0))
        for k, (name, y, colour) in enumerate(lines):
            y = y - np.median(y)
            lo, hi = -160, 160
            pts = [(pad + (W - 2 * pad) * t[i] / t[-1], top + H - pad - (H - 2 * pad) * (np.clip(y[i], lo, hi) - lo) / (hi - lo)) for i in range(len(t))]
            d.line(pts, fill=colour, width=1)
            d.text((pad + 420 + 150 * k, top + 4), name, fill=colour)
        d.line([(pad, top + H - pad), (W - pad, top + H - pad)], fill=(150, 150, 150))
        for s in range(int(t[-1]) + 1):
            x = pad + (W - 2 * pad) * s / t[-1]
            d.line([(x, top + H - pad), (x, top + H - pad + 4)], fill=(120, 120, 120))
            d.text((x - 3, top + H - pad + 6), str(s), fill=(90, 90, 90))
    im.save(path)
    print(path)


if __name__ == "__main__":
    args = sys.argv[1:]
    listed, picture = 6, None
    if "--list" in args:
        i = args.index("--list"); listed = int(args[i + 1]); del args[i:i + 2]
    if "--plot" in args:
        i = args.index("--plot"); picture = args[i + 1]; del args[i:i + 2]
    failed = 0
    for a in args:
        o = analyse(a, listed)
        if o is None: continue
        report(o)
        failed += bool(o["fails"])
    if picture: plot(picture, args)
    print(f"{len(args) - failed} of {len(args)} pass")
