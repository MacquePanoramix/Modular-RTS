"""Contact sheet: python -I sheet.py <out.png> <cols> <cell_h> <img1> <img2> ... ; labels each cell with its file name."""
import sys, os
from PIL import Image, ImageDraw
sys.stdout.reconfigure(encoding='utf-8')
out, cols, ch = sys.argv[1], int(sys.argv[2]), int(sys.argv[3])
files = sys.argv[4:]
ims = [Image.open(f).convert('RGB') for f in files]
cells = []
for im in ims:
    w = int(im.width * ch / im.height)
    cells.append(im.resize((w, ch), Image.LANCZOS))
cw = max(c.width for c in cells)
rows = (len(cells) + cols - 1) // cols
sheet = Image.new('RGB', (cw * cols, (ch + 18) * rows), (255, 255, 255))
d = ImageDraw.Draw(sheet)
for i, (c, f) in enumerate(zip(cells, files)):
    x, y = (i % cols) * cw, (i // cols) * (ch + 18)
    sheet.paste(c, (x + (cw - c.width) // 2, y + 18))
    d.text((x + 4, y + 3), os.path.splitext(os.path.basename(f))[0], fill=(0, 0, 0))
if sheet.width > 1280 or sheet.height > 1280:
    s = 1280 / max(sheet.width, sheet.height)
    sheet = sheet.resize((int(sheet.width * s), int(sheet.height * s)), Image.LANCZOS)
sheet.save(out)
print(out, sheet.size)
