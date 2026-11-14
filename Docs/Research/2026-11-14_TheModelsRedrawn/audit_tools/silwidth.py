"""Visible trunk width per height from the flat black front/side silhouettes (orthographic, 2.15 m over 1200 px,
centre row at z 0.98 m). The run of black pixels through the image's centre column is the trunk; arms that touch
the trunk merge into it (flagged when the run jumps)."""
import sys, json
from PIL import Image
sys.stdout.reconfigure(encoding='utf-8')
d = sys.argv[1]
mpp = 2.15 / 1200
info = {}
for name, H, hip, sh in (('Small', 1.42, 0.619, 0.825), ('Long', 1.86, 0.947, 1.235), ('Round', 1.56, 0.713, 1.002)):
    res = {}
    for view in ('front', 'side'):
        im = Image.open(f'{d}/{name}_rest_silhouette_{view}.png').convert('L')
        W, Hh = im.size
        px = im.load()
        cx = W // 2
        rows = []
        for r in range(Hh):
            z = 0.98 + (Hh / 2 - r) * mpp
            if not (0.25 * H <= z <= 0.85 * H):
                continue
            if px[cx, r] > 128:
                rows.append((z, 0.0)); continue
            a = cx
            while a > 0 and px[a - 1, r] <= 128:
                a -= 1
            b = cx
            while b < W - 1 and px[b + 1, r] <= 128:
                b += 1
            rows.append((z, (b - a + 1) * mpp))
        res[view] = rows
    out = [f'== {name} (H {H}, hip joint {hip}, shoulder joint {sh})']
    for z0 in [round(x * 0.02, 2) for x in range(int(0.25 * H / 0.02), int(0.85 * H / 0.02) + 1)]:
        f = min(res['front'], key=lambda t: abs(t[0] - z0))
        s = min(res['side'], key=lambda t: abs(t[0] - z0))
        out.append(f'{z0:.2f} ({z0 / H:.2f}H) front width {f[1]:.3f}  side depth {s[1]:.3f}')
    print('\n'.join(out))
