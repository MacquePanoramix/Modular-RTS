"""Pictures of one try around a pose: stagesheet.py <out.jpg> <folder> <bench output> <try index> <tag> <stage> [before s] [after s] [every s] [frame s]"""
import sys, re
from PIL import Image, ImageDraw
out, folder, log, which, tag, stage = sys.argv[1:7]
before = float(sys.argv[7]) if len(sys.argv) > 7 else .4
after = float(sys.argv[8]) if len(sys.argv) > 8 else 2.4
every = float(sys.argv[9]) if len(sys.argv) > 9 else .2
per = float(sys.argv[10]) if len(sys.argv) > 10 else .1
tries, last = [], 1e9
for line in open(log, encoding="utf8", errors="replace"):
    m = re.match(r"RISE_GETTING \w+ ([\d.]+) s \| (\w+) ([\d.]+) s", line)
    if not m: continue
    t = float(m.group(1))
    if t < last: tries.append([])
    last = t
    tries[-1].append((t, m.group(2), float(m.group(3))))
rows = tries[int(which)]
start = next(t - got for t, st, got in rows if st == stage)
times = [start - before + k * every for k in range(int((before + after) / every) + 1)]
ims = []
for t in times:
    k = int(round(t / per))
    try: im = Image.open(f"{folder}/{tag}_{k:03d}.ppm")
    except FileNotFoundError: continue
    w, h = im.size
    im = im.crop((int(w * .15), int(h * .3), int(w * .85), int(h * .75)))
    d = ImageDraw.Draw(im); d.rectangle((0, 0, 70, 14), fill=(0, 0, 0)); d.text((3, 2), f"{t - start:+.1f} s", fill=(255, 255, 255))
    ims.append(im)
cols = min(8, len(ims)); rws = (len(ims) + cols - 1) // cols
cw, ch = ims[0].size
sheet = Image.new("RGB", (cw * cols, ch * rws))
for i, im in enumerate(ims): sheet.paste(im, ((i % cols) * cw, (i // cols) * ch))
sheet.save(out); print(out, len(ims), sheet.size, f"{stage} began at {start:.2f} s")
