"""Take a run of frames out of a capture and number them from nothing, so that runs from different captures can play
side by side.   select_frames.py <frames folder>/<tag> <first> <last> <to folder>/<tag> [every]"""
import shutil
import sys
from pathlib import Path

src, first, last, dst = Path(sys.argv[1]), int(sys.argv[2]), int(sys.argv[3]), Path(sys.argv[4])
every = int(sys.argv[5]) if len(sys.argv) > 5 else 1
dst.parent.mkdir(parents=True, exist_ok=True)
count = 0
for i in range(first, last + 1, every):
    f = src.parent / f"{src.name}_{i:03d}.ppm"
    if not f.exists():
        f = src.parent / f"{src.name}_{i}.ppm"
    if not f.exists():
        break
    shutil.copyfile(f, dst.parent / f"{dst.name}_{count:03d}.ppm")
    count += 1
print(dst, count, "frames")
