"""The bench's frames as a moving picture and as strips: a grid of strengths (rows) by pickaxe weights (columns).
   bench_clip.py <frames folder> <out folder> <miner> [panel size]"""
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

src, out, who = Path(sys.argv[1]), Path(sys.argv[2]), sys.argv[3].lower()
size = int(sys.argv[4]) if len(sys.argv) > 4 else 180
# Only these (strength, weight) pairs, as one row, when given: e.g. 0.5:1,1:1,2:1
only = [tuple(float(v) for v in pair.split(":")) for pair in sys.argv[5].split(",")] if len(sys.argv) > 5 else None
out.mkdir(parents=True, exist_ok=True)
tags = sorted({p.name.rsplit("_", 1)[0] for p in src.glob(f"{who}_s*_w*_000.ppm")})
strengths = sorted({t.split("_")[1] for t in tags}, key=lambda s: float(s[1:]))
weights = sorted({t.split("_")[2] for t in tags}, key=lambda s: float(s[1:]))
count = min(100, min(len(list(src.glob(f"{t}_*.ppm"))) for t in tags))
try:
    font = ImageFont.truetype("arial.ttf", 13)
except OSError:
    font = ImageFont.load_default()
crop = (70, 50, 330, 310)


def panel(tag, i):
    return Image.open(src / f"{tag}_{i:03d}.ppm").convert("RGB").crop(crop).resize((size, size), Image.LANCZOS)


def cells():
    if only:
        return [(0, c, next(s for s in strengths if float(s[1:]) == a), next(w for w in weights if float(w[1:]) == b)) for c, (a, b) in enumerate(only)]
    return [(r, c, s, w) for r, s in enumerate(strengths) for c, w in enumerate(weights)]


def grid(i):
    placed = cells()
    sheet = Image.new("RGB", ((max(c for _, c, _, _ in placed) + 1) * size, (max(r for r, _, _, _ in placed) + 1) * size), "white")
    d = ImageDraw.Draw(sheet)
    for r, c, s, w in placed:
        if True:
            sheet.paste(panel(f"{who}_{s}_{w}", i), (c * size, r * size))
            label = f"strength {float(s[1:]):g}, pickaxe x{float(w[1:]):g}"
            d.rectangle((c * size, r * size, c * size + size, r * size + 17), fill=(255, 255, 255))
            d.text((c * size + 5, r * size + 2), label, fill=(20, 20, 20), font=font)
    return sheet


frames = [grid(i) for i in range(count)]
sample = Image.new("RGB", (frames[0].width, frames[0].height * 3))
for k, i in enumerate((0, count // 3, 2 * count // 3)):
    sample.paste(frames[i], (0, k * frames[0].height))
palette = sample.quantize(colors=64, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
quantized = [f.quantize(palette=palette, dither=Image.Dither.NONE) for f in frames]
path = out / (f"Bench_{who.capitalize()}_Row.gif" if only else f"Bench_{who.capitalize()}.gif")
quantized[0].save(path, save_all=True, append_images=quantized[1:], duration=40, loop=0, optimize=True)
print(path.name, path.stat().st_size // 1024, "KB", count, "frames", frames[0].size)

# A strip of each: every fourth frame.
for tag in ([] if only else tags):
    picks = list(range(0, count, 4))[:30]
    strip = Image.new("RGB", (len(picks) * 150, 150), "white")
    for k, i in enumerate(picks):
        strip.paste(panel(tag, i).resize((150, 150), Image.LANCZOS), (k * 150, 0))
    strip.save(out / f"strip_{tag}.jpg", quality=85)
