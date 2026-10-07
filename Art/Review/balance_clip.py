"""Bench clips of a body keeping its balance, side by side, each with a drawing from above of what it stands on and
where its weight is (from the bench's BALANCE lines).
   balance_clip.py <out.gif> <panel size> <first> <last> <side|front> <frames folder>/<tag>=<label> ...
side: the picture is from the body's right side (ahead is to the right); front: from before it (its right is to the
left). The drawing is turned the same way."""
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

out, size, first, last, view = Path(sys.argv[1]), int(sys.argv[2]), int(sys.argv[3]), int(sys.argv[4]), sys.argv[5]
cells = [(Path(a.split("=")[0]), a.split("=")[1]) for a in sys.argv[6:]]
try:
    font = ImageFont.truetype("arial.ttf", 13)
    small = ImageFont.truetype("arial.ttf", 11)
except OSError:
    font = small = ImageFont.load_default()
crop = (30, 8, 330, 308)
below = int(size * .5)
scale = size / 1.5          # pixels to a metre in the drawing


def trace(stem):
    rows = {}
    log = stem.parent / "log.txt"
    for line in log.read_text(encoding="utf-8", errors="ignore").splitlines():
        if not line.startswith("BALANCE " + stem.name + " "):
            continue
        p = line.split()
        shot = int(p[2].rstrip(":"))
        v = [float(x.replace(",", ".")) for x in p[3:24]]
        # The last word is the pull in newtons (the balance's bench) or the swing's phase (the swing's bench).
        try:
            v.append(float(p[24].replace(",", ".")))
        except ValueError:
            v.append(0)
        rows[shot] = dict(feet=[(v[0] > .5, (v[1], v[2]), (v[3], v[4]), v[5]), (v[6] > .5, (v[7], v[8]), (v[9], v[10]), v[11])],
                          weight=(v[12], v[13]), point=(v[14], v[15]), press=(v[16], v[17]), hips=(v[18], v[19]), margin=v[20], force=v[21])
    return rows


traces = [trace(stem) for stem, _ in cells]


def place(p, origin, middle):
    # (to the body's right, ahead) in metres -> pixels in the drawing, which keeps the feet in its middle.
    x, z = p[0] - middle[0], p[1] - middle[1]
    if view == "side":
        return origin[0] + z * scale, origin[1] + x * scale
    return origin[0] - x * scale, origin[1] - z * scale


frames = []
for i in range(first, last + 1):
    row = Image.new("RGB", (len(cells) * size, size + below), (250, 248, 242))
    d = ImageDraw.Draw(row)
    for k, (stem, label) in enumerate(cells):
        f = stem.parent / f"{stem.name}_{i:03d}.ppm"
        if f.exists():
            row.paste(Image.open(f).convert("RGB").crop(crop).resize((size, size), Image.LANCZOS), (k * size, 0))
        d.rectangle((k * size, 0, k * size + size, 18), fill=(255, 255, 255))
        d.text((k * size + 5, 2), label, fill=(20, 20, 20), font=font)
        t = traces[k].get(i)
        if t is None:
            continue
        origin = (k * size + size * .5, size + below * .56)
        ends = [e for _, heel, toe, _ in t["feet"] for e in (heel, toe)]
        middle = (sum(e[0] for e in ends) / 4, sum(e[1] for e in ends) / 4)
        for planted, heel, toe, half in t["feet"]:
            a, b = place(heel, origin, middle), place(toe, origin, middle)
            colour = (120, 110, 100) if planted else (205, 200, 190)
            w = max(2, int(half * 2 * scale))
            d.line((a, b), fill=colour, width=w)
            for c in (a, b):
                d.ellipse((c[0] - w / 2, c[1] - w / 2, c[0] + w / 2, c[1] + w / 2), fill=colour)
        c = place(t["press"], origin, middle)
        d.ellipse((c[0] - 5, c[1] - 5, c[0] + 5, c[1] + 5), outline=(40, 90, 170), width=2)
        c = place(t["point"], origin, middle)
        c = (min(max(c[0], k * size + 6), k * size + size - 6), min(max(c[1], size + 32), size + below - 6))
        inside = t["margin"] >= 0
        d.ellipse((c[0] - 4, c[1] - 4, c[0] + 4, c[1] + 4), fill=(30, 130, 60) if inside else (200, 40, 30))
        d.text((k * size + 5, size + 3), "from above: its feet, where they press (ring),", fill=(90, 90, 90), font=small)
        d.text((k * size + 5, size + 15), "and its weight's point (dot; red: outside them)", fill=(90, 90, 90), font=small)
        if t["force"] > 0:
            d.text((k * size + 5, size + below - 16), f"pulled with {t['force']:.0f} N", fill=(120, 80, 20), font=small)
    frames.append(row)
sample = Image.new("RGB", (frames[0].width, frames[0].height * 3))
for k, i in enumerate((0, len(frames) // 3, 2 * len(frames) // 3)):
    sample.paste(frames[i], (0, k * frames[0].height))
palette = sample.quantize(colors=96, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
quantized = [f.quantize(palette=palette, dither=Image.Dither.NONE) for f in frames]
quantized[0].save(out, save_all=True, append_images=quantized[1:], duration=40, loop=0, optimize=True)
frames[len(frames) // 3].save(out.with_suffix(".check.jpg"), quality=88)
print(out.name, out.stat().st_size // 1024, "KB")
