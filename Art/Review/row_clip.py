"""A row of bench clips side by side as one moving picture.
   row_clip.py <out.gif> <panel size> <frames, or first-last> <frames folder>/<tag>=<label> ..."""
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

# The frames are a count from the first, or first-last.
out, size = Path(sys.argv[1]), int(sys.argv[2])
first, last = (int(x) for x in sys.argv[3].split("-")) if "-" in sys.argv[3] else (0, int(sys.argv[3]) - 1)
count = last - first + 1
cells = [(Path(a.split("=")[0]), a.split("=")[1]) for a in sys.argv[4:]]
try:
    font = ImageFont.truetype("arial.ttf", 14)
except OSError:
    font = ImageFont.load_default()
crop = (60, 40, 330, 310)
frames = []
for i in range(count):
    row = Image.new("RGB", (len(cells) * size, size), "white")
    d = ImageDraw.Draw(row)
    for k, (stem, label) in enumerate(cells):
        im = Image.open(stem.parent / f"{stem.name}_{first + i:03d}.ppm").convert("RGB").crop(crop).resize((size, size), Image.LANCZOS)
        row.paste(im, (k * size, 0))
        d.rectangle((k * size, 0, k * size + size, 18), fill=(255, 255, 255))
        d.text((k * size + 5, 2), label, fill=(20, 20, 20), font=font)
    frames.append(row)
sample = Image.new("RGB", (frames[0].width, frames[0].height * 3))
for k, i in enumerate((0, count // 3, 2 * count // 3)):
    sample.paste(frames[i], (0, k * frames[0].height))
palette = sample.quantize(colors=64, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
quantized = [f.quantize(palette=palette, dither=Image.Dither.NONE) for f in frames]
quantized[0].save(out, save_all=True, append_images=quantized[1:], duration=40, loop=0, optimize=True)
print(out.name, out.stat().st_size // 1024, "KB")
