"""Wonder Gather — contact sheets for the sweep of extreme poses (Docs/ArtDirection/ModelQualityMethod.md, pass 4).

Reads the pictures and the list written by the sweep (Art/Blender/Worker/sweep.py: sweep_<Name>_poses.txt and
sweep_<Name>_<pose>_<view>.png) and lays them out: one tile per pose, its two views side by side, labelled with the
pose and what it tests (a third picture, when the sweep made one, is a close look at the joint tested). Several
sheets per being, so each tile stays large enough to read.

Usage: python sweep_sheets.py <sweep folder> <sheet folder> [tiles per sheet, default 6]
"""
import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

try:
    FONT = ImageFont.truetype("segoeui.ttf", 17)
except OSError:  # not on Windows: any font will do for labels
    FONT = ImageFont.load_default()

VIEW = (300, 400)


def main(src, dst, per):
    src, dst = Path(src), Path(dst)
    dst.mkdir(parents=True, exist_ok=True)
    for listing in sorted(src.glob("sweep_*_poses.txt")):
        name = listing.stem[len("sweep_"):-len("_poses")]
        rows = [line.rstrip("\n").split("\t") for line in listing.read_text(encoding="utf-8").splitlines() if line.strip()]
        for start in range(0, len(rows), per):
            chunk = rows[start:start + per]
            shown = max(len(r) - 2 for r in chunk)
            cols = 3 if shown <= 2 else 2
            lines = (len(chunk) + cols - 1) // cols
            tile_w, tile_h = VIEW[0] * shown + 4 * (shown - 1), VIEW[1] + 26
            sheet = Image.new("RGB", (cols * tile_w + (cols - 1) * 8, lines * tile_h + (lines - 1) * 8), (18, 22, 30))
            draw = ImageDraw.Draw(sheet)
            for k, (label, tests, *pictures) in enumerate(chunk):
                x, y = (k % cols) * (tile_w + 8), (k // cols) * (tile_h + 8)
                draw.text((x + 6, y + 2), f"{label}  ({tests})", fill=(240, 228, 205), font=FONT)
                for v, picture in enumerate(pictures[:shown]):
                    im = Image.open(src / picture).convert("RGB").resize(VIEW, Image.LANCZOS)
                    sheet.paste(im, (x + v * (VIEW[0] + 4), y + 26))
            out = dst / f"sweep_{name.lower()}_{start // per:02d}.jpg"
            sheet.save(out, quality=88)
            print(out.name, sheet.size, [r[0] for r in chunk])


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], int(sys.argv[3]) if len(sys.argv) > 3 else 6)
