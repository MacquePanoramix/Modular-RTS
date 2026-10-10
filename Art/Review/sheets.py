"""Wonder Gather — contact sheets for the model quality method (Docs/ArtDirection/ModelQualityMethod.md, pass 5).

Turns the frames of MinerCloseCapture (<miner>_<set>_<view>.ppm) into JPG contact sheets, one per miner and
group, labelled, for reading view by view:

- orbit: one sheet per height (low, eye, high);
- distances: one sheet per light;
- zone: one sheet per zone (head, neck, back, chest, hem, hands, boots, carried things, bag);
- walk: one sheet per view across the eight phases (the same view, phase by phase);
- motion: one sheet per movement (start, turn, stop);
- work: one sheet per view across the swing's six moments (ready, raising, raised, striking, struck, recovering);
- fresh: silhouettes, clay, and the eye-level orbit mirrored and in grey values.

Usage: python sheets.py <capture folder> <sheet folder> [--keep-ppm]
"""
import re
import sys
from collections import defaultdict
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont, ImageOps

TILE = 460
try:
    FONT = ImageFont.truetype("segoeui.ttf", 17)
except OSError:  # not on Windows: any font will do for labels
    FONT = ImageFont.load_default()


def sheet(images, labels, cols, out):
    rows = (len(images) + cols - 1) // cols
    w = TILE
    h = int(TILE * images[0].height / images[0].width)
    canvas = Image.new("RGB", (cols * w + (cols - 1) * 6, rows * h + (rows - 1) * 6), (18, 22, 30))
    for i, (im, label) in enumerate(zip(images, labels)):
        tile = im.resize((w, h), Image.LANCZOS)
        d = ImageDraw.Draw(tile)
        d.rectangle([0, 0, d.textlength(label, font=FONT) + 12, 24], fill=(18, 22, 30))
        d.text((6, 1), label, fill=(240, 228, 205), font=FONT)
        canvas.paste(tile, ((i % cols) * (w + 6), (i // cols) * (h + 6)))
    canvas.save(out, quality=90)


def main(src, dst, keep):
    src, dst = Path(src), Path(dst)
    dst.mkdir(parents=True, exist_ok=True)
    frames = {}
    for p in sorted(src.glob("*.ppm")):
        im = Image.open(p).convert("RGB")
        frames[p.stem] = im
        im.save(dst / f"{p.stem}.jpg", quality=92)
        if not keep:
            p.unlink()
    groups = defaultdict(list)
    for stem in frames:
        miner, kind, view = stem.split("_", 2)
        if kind == "orbit":
            key = f"{miner}_orbit_{view.split('_')[0]}"
        elif kind == "distances":
            key = f"{miner}_distances_{view.split('_')[0]}"
        elif kind == "zone":
            key = f"{miner}_zone_{view.split('_')[0]}"
        elif kind == "walk":
            key = f"{miner}_walk_{view.split('_', 1)[1]}"
        elif kind == "motion":
            key = f"{miner}_motion_{view.split('_')[0]}"
        elif kind == "work":
            key = f"{miner}_work_{view.split('_')[-1]}"
        else:
            key = f"{miner}_fresh_{view.split('_')[0]}"
        groups[key].append(stem)
    for key, stems in sorted(groups.items()):
        images = [frames[s] for s in stems]
        labels = [s.split("_", 2)[2] for s in stems]
        cols = 4 if len(images) > 6 else min(len(images), 3)
        if "_distances_" in key:
            cols = 2
        sheet(images, labels, cols, dst / f"sheet_{key}.jpg")
    # Fresh eyes: the eye-level orbit mirrored and in grey values.
    for miner in sorted({s.split("_")[0] for s in frames}):
        stems = sorted(s for s in frames if s.startswith(f"{miner}_orbit_eye"))
        if stems:
            sheet([ImageOps.mirror(frames[s]) for s in stems], [f"mirrored {s.split('_', 2)[2]}" for s in stems], 4, dst / f"sheet_{miner}_fresh_mirrored.jpg")
            sheet([ImageOps.grayscale(frames[s]).convert("RGB") for s in stems], [f"grey {s.split('_', 2)[2]}" for s in stems], 4, dst / f"sheet_{miner}_fresh_grey.jpg")
    print(f"{len(frames)} frames, {len(groups)} sheets in {dst}")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2], "--keep-ppm" in sys.argv)
