"""The stance atlas's sheets, from the tiles and figures StanceAtlasCapture writes (Docs/Technical/AnyBody/INTEGRATION.md 3.6).

python atlas_sheets.py <capture folder> [<sheets folder>]

- converts the tiles from PPM to PNG (the PPM are then removed);
- one sheet per stance: for each body a row as fitted and a row with its free limbs relaxed, its four sides (front,
  right, back, left: the side that faces the camera), a label of plain words per row;
- one page per body: every stance from its left side, as fitted beside relaxed, to see stiffness and repetition.
The figures are printed here, not in Unity, so the tiles stay clean and the layout can change without rendering again.
"""
import csv
import os
import sys

from PIL import Image, ImageDraw, ImageFont

VIEWS = ['front', 'right', 'back', 'left']
LOOKS = [('fitted', 'as fitted'), ('relaxed', 'free limbs relaxed')]
TILE = 384
LABEL = 50
HEADER = 70
PAD = 8
INK = (40, 38, 36)
FAINT = (110, 106, 100)
PAPER = (246, 243, 237)
WARN = (170, 60, 40)


def font(size, bold=False):
    names = ['segoeuib.ttf' if bold else 'segoeui.ttf', 'arialbd.ttf' if bold else 'arial.ttf', 'DejaVuSans.ttf']
    for n in names:
        for d in [os.path.join(os.environ.get('WINDIR', 'C:/Windows'), 'Fonts'), '']:
            try:
                return ImageFont.truetype(os.path.join(d, n) if d else n, size)
            except OSError:
                pass
    return ImageFont.load_default()


def convert(tiles):
    for name in sorted(os.listdir(tiles)):
        if name.endswith('.ppm'):
            p = os.path.join(tiles, name)
            Image.open(p).save(p[:-4] + '.png', optimize=True)
            os.remove(p)


def figures(folder):
    rows = {}
    with open(os.path.join(folder, 'figures.csv'), newline='', encoding='utf-8') as f:
        for r in csv.DictReader(f):
            rows[(r['body'], r['stance'], r.get('look', 'fitted'))] = r
    return rows


def label(r):
    """One row's words: the fit, the reserve S (the share of its strength the worst joint must use), the base margin."""
    if r is None:
        return 'no fit', True
    cost = float(r['cost'])
    held = r['held'] == '1'
    worst = ' '.join(w[:-1] + (' left' if w.endswith('L') else ' right') if w[-1] in 'LR' else w for w in r['worst'].split())
    parts = [f"fit {cost:.2f}" + ('  poor fit' if cost >= .5 else '') + ('  failed fit' if cost >= 2 else '')]
    parts.append(f"reserve S {float(r['S']):.2f} ({worst})" if held else 'cannot hold')
    if r['base_mm']:
        parts.append(f"base {float(r['base_mm']):+.0f} mm")
    flagged = not held or cost >= .5
    for key, words in (('contact_err_mm', 'off the ground'), ('penetration_mm', 'into the ground')):
        if r[key] and float(r[key]) > 2:
            parts.append(f"{float(r[key]):.0f} mm {words}")
            flagged = True
    return '  ·  '.join(parts), flagged


def tile(folder, body, stance, look, view, size):
    p = os.path.join(folder, 'tiles', f'{body}_{stance}_{look}_{view}.png')
    return Image.open(p).resize((size, size), Image.LANCZOS) if os.path.exists(p) else None


def stance_sheet(folder, sheets, stance, name, bodies, rows):
    width = PAD + len(VIEWS) * (TILE + PAD)
    height = HEADER + len(bodies) * len(LOOKS) * (LABEL + TILE + PAD) + PAD
    sheet = Image.new('RGB', (width, height), PAPER)
    d = ImageDraw.Draw(sheet)
    d.text((PAD, 10), name, font=font(26, True), fill=INK)
    d.text((PAD, 42), 'front · right · back · left (the side that faces the camera).  Tinted: what bears weight.  '
           'The dark dot on the head: its front.  The thin line: the centre of mass, plumb.', font=font(15), fill=FAINT)
    y = HEADER
    for body in bodies:
        for look, words in LOOKS:
            text, flagged = label(rows.get((body, stance, look)))
            d.text((PAD, y + 8), f'{body}, {words}', font=font(19, True), fill=INK)
            d.text((PAD, y + 30), text, font=font(15), fill=WARN if flagged else INK)
            y += LABEL
            for i, view in enumerate(VIEWS):
                t = tile(folder, body, stance, look, view, TILE)
                if t:
                    sheet.paste(t, (PAD + i * (TILE + PAD), y))
            y += TILE + PAD
    sheet.save(os.path.join(sheets, f'stance_{stance}.png'), optimize=True)


def body_page(folder, sheets, body, stances, names, rows, view='left', size=240, pairs=3):
    columns = 2 * pairs
    rows_n = (len(stances) + pairs - 1) // pairs
    gap = 24
    width = PAD + pairs * (2 * size + PAD + gap)
    height = HEADER + rows_n * (size + 44 + PAD)
    page = Image.new('RGB', (width, height), PAPER)
    d = ImageDraw.Draw(page)
    d.text((PAD, 10), f'{body}: every stance from its {view} side, as fitted (left) and with its free limbs relaxed (right)',
           font=font(22, True), fill=INK)
    for k, stance in enumerate(stances):
        x = PAD + (k % pairs) * (2 * size + PAD + gap)
        y = HEADER + (k // pairs) * (size + 44 + PAD)
        d.text((x, y + 2), names.get(stance, stance)[:60], font=font(13, True), fill=INK)
        for j, (look, _) in enumerate(LOOKS):
            r = rows.get((body, stance, look))
            if r:
                d.text((x + j * (size + PAD), y + 20), (f"S {float(r['S']):.2f}" if r['held'] == '1' else 'cannot hold') +
                       f" · fit {float(r['cost']):.2f}", font=font(12), fill=FAINT)
            t = tile(folder, body, stance, look, view, size)
            if t:
                page.paste(t, (x + j * (size + PAD), y + 40))
    page.save(os.path.join(sheets, f'body_{body}.png'), optimize=True)


def main():
    folder = sys.argv[1]
    sheets = sys.argv[2] if len(sys.argv) > 2 else os.path.join(folder, 'sheets')
    os.makedirs(sheets, exist_ok=True)
    convert(os.path.join(folder, 'tiles'))
    rows = figures(folder)
    bodies, stances, names = [], [], {}
    for (body, stance, look), r in rows.items():
        if body not in bodies:
            bodies.append(body)
        if stance not in stances:
            stances.append(stance)
        names[stance] = r['name']
    for stance in stances:
        stance_sheet(folder, sheets, stance, names[stance], bodies, rows)
    for body in bodies:
        body_page(folder, sheets, body, stances, names, rows)
    print(f'{len(stances)} stance sheets, {len(bodies)} body pages in {sheets}')


if __name__ == '__main__':
    main()
