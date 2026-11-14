"""Cross-sections of a miner's clothed trunk at rest, by cutting the LOD0 mesh's edges with level planes.
Run: blender -b --factory-startup --python measure.py -- <Name> <outfile>
Read-only import; writes only the text file."""
import bpy, sys, json, math
sys.stdout.reconfigure(encoding='utf-8')
argv = sys.argv[sys.argv.index('--') + 1:]
NAME, OUTF = argv[0], argv[1]
PROJ = 'D:/Programs/Unity/Projects/Wonder Gather/The Wanderer/Assets/_WonderGather/Art/Worker/Miners'
INFO = {m['name']: m for m in json.load(open(f'{PROJ}/miners.json', encoding='utf-8'))['miners']}[NAME]
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=f'{PROJ}/Miner_{NAME}.fbx')
body = bpy.data.objects[f'Miner_{NAME}_LOD0']
rig = bpy.data.objects[f'Miner_{NAME}_Rig']
me = body.data
names = [g.name for g in body.vertex_groups]
dom = []
for v in me.vertices:
    best, w = '', -1
    for g in v.groups:
        if g.weight > w:
            best, w = names[g.group], g.weight
    dom.append(best)
TRUNK = {'Root', 'Pelvis', 'Spine', 'Chest', 'SkirtFront.L', 'SkirtFront.R', 'SkirtBack.L', 'SkirtBack.R'}
LEG = {'Thigh.L', 'Thigh.R', 'Shin.L', 'Shin.R'}
co = [v.co.copy() for v in me.vertices]
H = INFO['height']


def hull(pts):
    pts = sorted(set(pts))
    if len(pts) < 3:
        return pts
    def cross(o, a, b):
        return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0])
    lo, up = [], []
    for p in pts:
        while len(lo) >= 2 and cross(lo[-2], lo[-1], p) <= 0:
            lo.pop()
        lo.append(p)
    for p in reversed(pts):
        while len(up) >= 2 and cross(up[-2], up[-1], p) <= 0:
            up.pop()
        up.append(p)
    return lo[:-1] + up[:-1]


def section(z, cls):
    pts = []
    for e in me.edges:
        a, b = e.vertices
        if dom[a] not in cls or dom[b] not in cls:
            continue
        za, zb = co[a].z, co[b].z
        if (za - z) * (zb - z) < 0:
            t = (z - za) / (zb - za)
            p = co[a].lerp(co[b], t)
            pts.append((round(p.x, 5), round(p.y, 5)))
    return pts


lines = [f'# {NAME}: clothed trunk cross-sections at rest (LOD0 edges cut by level planes; trunk = strongest bone pelvis, spine, chest or a skirt flap)',
         f'height {H}; hip joint {INFO["hipHeight"]:.3f}; shoulder joint {INFO["hipHeight"] + INFO["shoulder"][1]:.3f}; waistRise {INFO["waistRise"]:.3f}',
         'z  z/H  width(x)  depth(y)  hullGirth   | legs: each leg tube width at this level (if any)']
z = 0.22 * H
rows = []
while z < 0.86 * H:
    pts = section(z, TRUNK)
    lp = section(z, LEG)
    legs = ''
    if lp:
        left = [p for p in lp if p[0] > 0]
        if left:
            legs = f'legL {max(p[0] for p in left) - min(p[0] for p in left):.3f}'
    if len(pts) >= 3:
        xs = [p[0] for p in pts]; ys = [p[1] for p in pts]
        h = hull(pts)
        g = sum(math.dist(h[i], h[(i + 1) % len(h)]) for i in range(len(h)))
        rows.append((z, max(xs) - min(xs), max(ys) - min(ys), g))
        lines.append(f'{z:.3f} {z / H:.3f} {max(xs) - min(xs):.3f} {max(ys) - min(ys):.3f} {g:.3f}  {legs}')
    else:
        lines.append(f'{z:.3f} {z / H:.3f} -  -  -  {legs}')
    z += 0.01
# outer shoulder (sleeves included): every vertex 0-4 cm below the shoulder joint
shz = INFO['hipHeight'] + INFO['shoulder'][1]
band = [c for c in co if shz - 0.04 <= c.z <= shz + 0.01]
lines.append(f'outer shoulder width (all vertices, 0-4 cm below the shoulder joint): {max(c.x for c in band) - min(c.x for c in band):.3f}')
# skirt hem and the trousers' top: lowest skirt-flap vertex, highest thigh-weighted vertex not under the trunk class
sk = [c.z for c, d in zip(co, dom) if d.startswith('Skirt')]
th = [c.z for c, d in zip(co, dom) if d.startswith('Thigh')]
lines.append(f'skirt (flap-weighted) lowest {min(sk):.3f} ; thigh-weighted vertices highest {max(th):.3f}')
# narrowest between hips and chest, widest at the chest and the hips
hipz, chz = INFO['hipHeight'], shz
mid = [r for r in rows if hipz + 0.02 <= r[0] <= chz - 0.08]
if mid:
    w = min(mid, key=lambda r: r[1])
    lines.append(f'narrowest width between hips and chest: {w[1]:.3f} at z {w[0]:.3f} ({w[0] / H:.3f} H), depth there {w[2]:.3f}, girth {w[3]:.3f}')
    g = min(mid, key=lambda r: r[3])
    lines.append(f'least girth between hips and chest: {g[3]:.3f} at z {g[0]:.3f}, width {g[1]:.3f}, depth {g[2]:.3f}')
ch = [r for r in rows if chz - 0.12 <= r[0] <= chz - 0.02]
if ch:
    c = max(ch, key=lambda r: r[1])
    lines.append(f'chest (widest 2-12 cm below the shoulder joint): width {c[1]:.3f} at z {c[0]:.3f}, depth {c[2]:.3f}, girth {c[3]:.3f}')
hp = [r for r in rows if hipz - 0.04 <= r[0] <= hipz + 0.06]
if hp:
    h_ = max(hp, key=lambda r: r[1])
    lines.append(f'hips (widest 4 cm below to 6 cm above the hip joint): width {h_[1]:.3f} at z {h_[0]:.3f}, depth {h_[2]:.3f}, girth {h_[3]:.3f}')
open(OUTF, 'w', encoding='utf-8').write('\n'.join(lines))
print('\n'.join(lines[-8:]))
