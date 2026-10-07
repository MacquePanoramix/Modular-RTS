"""Parts of one capture, one after another, as one moving picture: each part with its label while it plays.
   sequence_clip.py <out.gif> <panel size> <left,top,right,bottom of the frame to keep> <frames folder>/<tag>=<label> ...
   A part may be followed by :first-last to keep only those frames."""
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

out, size = Path(sys.argv[1]), int(sys.argv[2])
crop = tuple(int(x) for x in sys.argv[3].split(","))
try:
    font = ImageFont.truetype("arial.ttf", 14)
except OSError:
    font = ImageFont.load_default()
frames = []
for part in sys.argv[4:]:
    stem, label = part.split("=", 1)
    span = None
    if ":" in label and label.rsplit(":", 1)[1].replace("-", "").isdigit():
        label, span = label.rsplit(":", 1)
        span = tuple(int(x) for x in span.split("-"))
    stem = Path(stem)
    files = sorted(stem.parent.glob(stem.name + "_[0-9][0-9][0-9].ppm"))
    if span:
        files = [f for f in files if span[0] <= int(f.stem[-3:]) <= span[1]]
    for f in files:
        im = Image.open(f).convert("RGB").crop(crop).resize((size, size), Image.LANCZOS)
        d = ImageDraw.Draw(im)
        d.rectangle((0, 0, size, 18), fill=(255, 255, 255))
        d.text((5, 2), label, fill=(20, 20, 20), font=font)
        frames.append(im)
count = len(frames)
sample = Image.new("RGB", (size, size * 4))
for k, i in enumerate((0, count // 4, count // 2, 3 * count // 4)):
    sample.paste(frames[i], (0, k * size))
palette = sample.quantize(colors=96, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
quantized = [f.quantize(palette=palette, dither=Image.Dither.NONE) for f in frames]
quantized[0].save(out, save_all=True, append_images=quantized[1:], duration=40, loop=0, optimize=True)
print(out.name, count, "frames", out.stat().st_size // 1024, "KB")
