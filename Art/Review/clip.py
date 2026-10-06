"""Put the clip capture's frames together: the three miners side by side, as a moving picture and as a strip of moments.
   make_clip.py <frames folder> <out folder>"""
import sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

src, out = Path(sys.argv[1]), Path(sys.argv[2])
out.mkdir(parents=True, exist_ok=True)
NAMES = ("small", "long", "round")
FRAMES = 25
try:
    font = ImageFont.truetype("arial.ttf", 15)
except OSError:
    font = ImageFont.load_default()


def frame(view, i, size=280):
    row = Image.new("RGB", (size * 3, size), "white")
    for k, name in enumerate(NAMES):
        im = Image.open(src / f"{name}_clip_{view}_{i:03d}.ppm").convert("RGB").resize((size, size), Image.LANCZOS)
        row.paste(im, (k * size, 0))
    d = ImageDraw.Draw(row)
    for k, name in enumerate(NAMES):
        d.text((k * size + 8, 6), name.capitalize(), fill=(30, 30, 30), font=font)
    d.text((size * 3 - 78, size - 22), f"{i / 25:.2f} s", fill=(30, 30, 30), font=font)
    return row


for view in ("side", "front"):
    rows = [frame(view, i) for i in range(FRAMES)]
    # One palette for the whole clip, so colours do not flicker from frame to frame.
    sample = Image.new("RGB", (rows[0].width, rows[0].height * 4))
    for k, i in enumerate((0, 8, 14, 20)):
        sample.paste(rows[i], (0, k * rows[0].height))
    palette = sample.quantize(colors=96, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
    frames = [r.quantize(palette=palette, dither=Image.Dither.NONE) for r in rows]
    frames[0].save(out / f"Work_Clip_{view.capitalize()}.gif", save_all=True, append_images=frames[1:], duration=40, loop=0, optimize=True)
    print(view, (out / f"Work_Clip_{view.capitalize()}.gif").stat().st_size // 1024, "KB")

# One swing as a strip: every second frame of the first 0.8 s, from the side.
picks = list(range(0, 22, 2))
strip = Image.new("RGB", (len(picks) * 200, 3 * 200 + 24), "white")
d = ImageDraw.Draw(strip)
for c, i in enumerate(picks):
    for r, name in enumerate(NAMES):
        im = Image.open(src / f"{name}_clip_side_{i:03d}.ppm").convert("RGB").resize((200, 200), Image.LANCZOS)
        strip.paste(im, (c * 200, 24 + r * 200))
    d.text((c * 200 + 70, 4), f"{i / 25:.2f} s", fill=(30, 30, 30), font=font)
strip.save(out / "Work_Clip_Strip.jpg", quality=88)
print("strip", strip.size)
