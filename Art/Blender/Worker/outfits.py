"""Wonder Gather — garments and accessories (S1d).

Each piece is fitted to a Body (body.py): it reads the skeleton and the trunk's
measurements, so the same piece fits any body. A character is a body plus a
list of pieces. That is the shape of a character creator: modules chosen and
tuned, never one fixed model.

Garments: a top (shirt, smock or coat body, with sleeves), a skirt (a coat's or
smock's lower part), trousers, a collar, lapels, a shirt front, an apron,
patches and buttons.
Accessories, some of them a miner's: a satchel, a lantern, a pickaxe, a mug, a
leather cap with a lamp, a hammer.
"""
import math
import random

from mathutils import Matrix, Vector

import shapes
from shapes import chain, cylinder, displace, ellipsoid, frame, fuse, lathe, panel, simplify, slab, spline, torus, tube


def bend_creases(joints, amplitude, reach, seed=0):
    """Folds where cloth bunches: on the inside of each bent joint (elbow, knee) only, a few soft
    crescents that fade away from the bend. joints: (from, bend, to) triples."""
    from mathutils import noise
    bends = []
    for a, j, c in joints:
        u, v = (a - j).normalized(), (c - j).normalized()
        inside = u + v  # points into the bend
        if inside.length < 0.15:
            continue  # nearly straight: no bunching
        bends.append((j, inside.normalized(), (c - a).normalized(), min(1.0, inside.length)))

    def fold(co, nm):
        m = 0.0
        for j, inside, axis, depth in bends:
            rel = co - j
            dist = rel.length
            if dist >= reach:
                continue
            across = rel - axis * rel.dot(axis)
            facing = across.normalized().dot(inside) if across.length > 1e-5 else 0.0
            if facing <= 0.1:
                continue
            wobble = noise.noise(co * 40 + Vector((seed, 0, 0))) * 1.5
            m += amplitude * depth * (facing ** 1.5) * (1 - dist / reach) ** 1.2 * math.sin(rel.dot(axis) * math.tau / 0.034 + wobble)
        return m
    return fold


def trunk_at(b, z, loose=0.0):
    """The trunk's (half-width, half-depth) at a height, between its measured levels."""
    levels = [("pelvis", b.pelvis.z), ("waist", b.waist.z), ("chest", b.chest.z), ("collar", b.collar.z)]
    if z <= levels[0][1]:
        w, d = b.trunk["pelvis"]
    elif z >= levels[-1][1]:
        w, d = b.trunk["collar"]
    else:
        for (n0, z0), (n1, z1) in zip(levels, levels[1:]):
            if z0 <= z <= z1:
                t = (z - z0) / max(z1 - z0, 1e-5)
                w = b.trunk[n0][0] + (b.trunk[n1][0] - b.trunk[n0][0]) * t
                d = b.trunk[n0][1] + (b.trunk[n1][1] - b.trunk[n0][1]) * t
                break
    return w + loose, d + loose


def axis_at(b, z):
    """The trunk's centre (x, y) at a height."""
    pts = [b.pelvis, b.waist, b.chest, b.collar]
    if z <= pts[0].z:
        return pts[0].x, pts[0].y
    for p0, p1 in zip(pts, pts[1:]):
        if p0.z <= z <= p1.z:
            t = (z - p0.z) / max(p1.z - p0.z, 1e-5)
            q = p0.lerp(p1, t)
            return q.x, q.y
    return pts[-1].x, pts[-1].y


# ---------------------------------------------------------------- garments

def top(b, mat, loose=0.012, sleeve=1.0, cuff=1.25, rolled=False, folds=0.0035, skirted=False, name="Top"):
    """The upper garment: a torso with two sleeves fused into one closed surface, creased at the elbows.
    sleeve: how far down the forearm it reaches (1 is the wrist, more covers part of the hand).
    skirted: a skirt carries on from the waist, so the top stops there."""
    t = b.trunk
    levels = ("waist", "chest", "collar") if skirted else ("pelvis", "waist", "chest", "collar")
    joints = ([b.waist + Vector((0, 0, -0.02))] if skirted else [b.pelvis + Vector((0, 0, -0.03)), b.waist]) + [b.chest, b.collar]
    radii = [(t[k][0] + loose, t[k][1] + loose) for k in levels]
    if skirted:
        # Its lower end tucks inside the skirt, which hangs from just under the top's real surface.
        radii[0] = (radii[0][0] * 0.82, radii[0][1] * 0.82)
    # The neckline: the garment closes around the neck just above the collar bones, so nothing shows between.
    nk = b.hr * b.p.get("neck_r", 0.5)
    joints = joints + [b.collar + Vector((0, 0.01, 0.03))]
    radii = radii + [(nk * 1.2 + loose * 0.3, nk * 1.15 + loose * 0.3)]
    parts = [chain(f"{b.name}_{name}Torso", joints, radii, mat)]
    # A probe: the torso alone, fused and smoothed like the garment, for what hangs from it or lies over it to
    # measure (arms hanging beside the waist must not count as the body). Removed before export.
    fuse(f"{b.name}_{name}Probe", [chain(f"{b.name}_{name}ProbeTorso", joints, radii, mat)], mat, voxel=0.0055, smooth=6)
    r = b.p["arm"]
    out = []
    for i in (0, 1):
        js = b.arm_joints(i)
        rad = [r * 1.2, r * 1.12, r * 1.03, r * 0.97, r * 0.92, r * 0.8]
        el, wr = b.elbows[i], b.wrists[i]
        if rolled:
            js = js[:4] + [el.lerp(wr, 0.1)]
            rad = rad[:4] + [r * 1.0]
            roll = (wr - el).normalized()
            out.append(shapes.torus(f"{b.name}_Roll{i}", el.lerp(wr, 0.12), r * 0.78 + loose * 0.5, r * 0.42, mat,
                                    rotation=frame(roll, Vector((0, 0, 1))) @ Matrix.Rotation(math.pi / 2, 3, 'Y')))
        else:
            end = el.lerp(wr, 0.45 + 0.55 * sleeve) if sleeve <= 1 else wr + (wr - el).normalized() * (sleeve - 1) * b.fore
            js = js[:5] + [end]
            rad[-1] = r * 0.8 * cuff
        parts.append(chain(f"{b.name}_{name}Arm{i}", js, [(x + loose, x + loose) for x in rad], mat))
    obj = fuse(f"{b.name}_{name}", parts, mat, voxel=0.0055, smooth=6)
    if folds:
        displace(obj, bend_creases([(b.shoulders[i], b.elbows[i], b.wrists[i]) for i in (0, 1)], folds, 0.09))
    simplify(obj, 10000)
    return [obj] + out


_TREES = {}


def surface_hit(garment, centre, z, angle, reach=0.6):
    """How far from centre (x, y) the garment's surface is at a height and angle (cast inwards), or None."""
    from mathutils.bvhtree import BVHTree
    import bpy
    tree = _TREES.get(garment.name)
    if tree is None:
        tree = _TREES[garment.name] = BVHTree.FromObject(garment, bpy.context.evaluated_depsgraph_get())
    direction = Vector((math.cos(angle), math.sin(angle), 0))
    hit = tree.ray_cast(Vector((centre[0], centre[1], z)) + direction * reach, -direction, reach)[0]
    return Vector((hit.x - centre[0], hit.y - centre[1], 0)).length if hit is not None else None


def surface_radius(garment, centre, z, angle, guess):
    r = surface_hit(garment, centre, z, angle)
    return (r if r is not None else guess), None


def skirt(b, mat, hem, flare=1.25, loose=0.014, open_front=False, folds=0.07, fold_count=7, ragged=0.0, seed=0, name="Skirt", fit=None, under=()):
    """A coat's or smock's lower part: a flared tube from the waist to the hem (a fraction of the height),
    hanging in soft vertical folds, open at the front for a coat. fit: the top it hangs from; the skirt
    starts just inside that top's real surface (smoothing makes a garment a little smaller than its
    measurements), so the top's hem always hides the skirt's edge."""
    rng = random.Random(seed)
    fit_tree = None
    import bmesh
    import bpy
    top_z = b.waist.z + 0.07
    hem_z = hem * b.H
    rings, around = 9, 48
    bm = bmesh.new()
    verts = []
    for k in range(rings + 1):
        t = k / rings
        z = top_z + (hem_z - top_z) * t
        w, d = trunk_at(b, max(z, b.pelvis.z - 0.02), loose)
        cx, cy = axis_at(b, z)
        if fit is not None:
            # Shrink the upper rings to the top's real size, easing back to the measurements towards the hem.
            if fit_tree is None:
                zf = b.waist.z
                (rw, fit_tree), (rd, _) = surface_radius(fit, axis_at(b, zf), zf, 0.0, w), surface_radius(fit, axis_at(b, zf), zf, -math.pi / 2, d)
                fx, fy = (rw - 0.007) / w, (rd - 0.007) / d
            ease = min(1.0, t * 1.6)
            w *= fx + (1 - fx) * ease
            d *= fy + (1 - fy) * ease
        # Never inside what it covers: all round at the seat, at the sides down the thighs; below, the legs
        # part and the skirt hangs free.
        angles = ((0.0, math.pi, -math.pi / 2, math.pi / 2, -math.pi / 4, -3 * math.pi / 4, math.pi / 4, 3 * math.pi / 4)
                  if z > b.pelvis.z - 0.1 else (0.0, math.pi) if z > b.pelvis.z - 0.28 else ())
        for g in under:
            for angle in angles:
                h = surface_hit(g, (cx, cy), z, angle)
                if h is None:
                    continue
                c, s_ = abs(math.cos(angle)), abs(math.sin(angle))
                need = h + 0.01
                # Grow the ellipse just enough to pass outside this point.
                r = 1 / math.sqrt((c / w) ** 2 + (s_ / d) ** 2) if w > 0 and d > 0 else 0
                if r < need:
                    k = need / r
                    w, d = w * k, d * k
        widen = 1 + (flare - 1) * (t ** 1.5)
        ring = []
        for a in range(around):
            ang = a / around * math.tau
            # Soft vertical folds, crisper in their valleys, growing towards the hem.
            wave = math.sin(ang * fold_count + seed)
            fold = 1 + folds * (t ** 1.1) * math.copysign(abs(wave) ** 0.7, wave) + folds * 0.35 * t * math.sin(ang * (fold_count * 2 + 1) + seed * 2)
            drop = 0.02 * t * math.sin(ang * 2 + seed)
            if k == rings and ragged:
                drop += ragged * (0.5 + 0.5 * math.sin(ang * 11 + seed)) * rng.uniform(0.4, 1.0)
            ring.append(bm.verts.new((cx + math.cos(ang) * w * widen * fold, cy + math.sin(ang) * d * widen * fold * 1.05, z - drop)))
        verts.append(ring)
    gap = around * 3 // 4  # the front (-y)
    for k in range(rings):
        for a in range(around):
            if open_front and k > 0 and abs(a - gap) < 1:
                continue
            c = (a + 1) % around
            bm.faces.new((verts[k][a], verts[k][c], verts[k + 1][c], verts[k + 1][a]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(f"{b.name}_{name}")
    bm.to_mesh(mesh)
    bm.free()
    obj = shapes.link(bpy.data.objects.new(f"{b.name}_{name}", mesh))
    obj.modifiers.new("Thickness", 'SOLIDIFY').thickness = 0.012
    sub = obj.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = sub.render_levels = 1
    shapes.apply_all(obj)
    return [shapes.finish(obj, mat)]


def trousers(b, mat, loose=0.01, boot=0.15, folds=0.004, hidden_above=None):
    """Trousers falling over the boots' tops (the boots' shafts end boot above the ankle), bloused softly
    there, with a few creases behind the knees. hidden_above: a height above which a closed skirt or coat hides
    them; that part is not made at all, so it can never show through."""
    import body as body_module
    leg = b.p["leg"]
    parts = [chain(f"{b.name}_Seat", [b.pelvis + Vector((0, 0, 0.03)), b.pelvis + Vector((0, 0.005, -0.07))],
                   [(b.trunk["pelvis"][0] + loose, b.trunk["pelvis"][1] + loose), (b.trunk["pelvis"][0] * 0.95 + loose, b.trunk["pelvis"][1] + loose)], mat)]
    for i in (0, 1):
        hp, mid, kn, calf, an = b.leg_joints(i)

        def at_height(z):
            tt = (kn.z - z) / max(kn.z - an.z, 1e-4)
            return kn.lerp(an, min(max(tt, 0.0), 1.0))
        top = an.z + boot
        shaft = body_module.shaft_radius(b)
        # The thigh starts a little inside the hip joint, so the trousers stay within the hips' width.
        js = [hp + Vector(((1, -1)[i] * 0.012, 0, 0.03)), mid, kn, at_height(top + 0.07), at_height(top + 0.01), at_height(top - 0.025)]
        rad = [leg * 1.3 + loose, leg * 1.2 + loose, leg * 1.02 + loose, leg * 1.04 + loose, shaft + 0.013, shaft + 0.009]
        parts.append(chain(f"{b.name}_TrouserLeg{i}", js, [(x, x) for x in rad], mat))
    obj = fuse(f"{b.name}_Trousers", parts, mat, voxel=0.0055, smooth=6)
    if folds:
        displace(obj, bend_creases([(b.hips[i], b.knees[i], b.ankles[i]) for i in (0, 1)], folds, 0.08, seed=3))
    if hidden_above is not None:
        # Good practice: geometry no one can see is removed, so it can never poke through what covers it.
        import bmesh
        bm = bmesh.new()
        bm.from_mesh(obj.data)
        bmesh.ops.delete(bm, geom=[f for f in bm.faces if min(v.co.z for v in f.verts) > hidden_above], context='FACES')
        bm.to_mesh(obj.data)
        bm.free()
    simplify(obj, 7000)
    return [obj]


def lay_on(b, points, horizontal=(), down=(), lift=0.004, keep_above=None):
    """Points laid onto garments, as a strap lies on clothes: below the shoulders each is cast towards the
    trunk's axis onto the horizontal garments (the torso and skirt, not the hanging arms); over the shoulders it
    is cast down onto the down garments. Returns the laid points and the surface's outward normals."""
    from mathutils.bvhtree import BVHTree
    import bpy
    def tree(g):
        t = _TREES.get(g.name)
        if t is None:
            t = _TREES[g.name] = BVHTree.FromObject(g, bpy.context.evaluated_depsgraph_get())
        return t
    out, normals = [], []
    for p in points:
        p = Vector(p)
        best = None
        if down and p.z > b.chest.z + 0.02 and (keep_above is None or p.z > keep_above):
            for g in down:
                hit, nm, _, _ = tree(g).ray_cast(p + Vector((0, 0, 0.4)), Vector((0, 0, -1)), 0.8)
                if hit is not None and (best is None or hit.z > best[0].z):
                    best = (hit, nm if nm.z > 0 else -nm)
        if best is None:
            cx, cy = axis_at(b, p.z)
            outward = Vector((p.x - cx, p.y - cy, 0))
            if outward.length < 1e-4:
                outward = Vector((0, -1, 0))
            outward.normalize()
            for g in horizontal:
                hit, nm, _, _ = tree(g).ray_cast(Vector((cx, cy, p.z)) + outward * 0.6, -outward, 0.6)
                if hit is not None:
                    reach = Vector((hit.x - cx, hit.y - cy, 0)).length
                    if best is None or reach > best[2]:
                        best = (hit, nm if nm.dot(outward) > 0 else -nm, reach)
        if best is None:
            out.append(None)
            normals.append(None)
        else:
            out.append(best[0] + best[1] * lift)
            normals.append(best[1].normalized())
    found = [i for i, q in enumerate(out) if q is not None]
    for i in range(len(out)):
        if out[i] is not None:
            continue
        before = max([j for j in found if j < i], default=None)
        after = min([j for j in found if j > i], default=None)
        if before is not None and after is not None:
            f = (i - before) / (after - before)
            out[i] = out[before].lerp(out[after], f)
            normals[i] = normals[before].lerp(normals[after], f).normalized()
        elif before is not None or after is not None:
            j = before if before is not None else after
            out[i], normals[i] = out[j].copy(), normals[j].copy()
        else:
            out[i], normals[i] = Vector(points[i]), Vector((0, 0, 1))
    return out, normals


def neckband(b, mat, garment, loose=0.006):
    """A smock's rolled neckline, hugging the neck where it leaves the garment: each point of the ring
    sits on the garment's own surface (found by casting down onto it)."""
    from mathutils.bvhtree import BVHTree
    import bpy
    nk = b.hr * b.p.get("neck_r", 0.5)
    c = b.collar + Vector((0, 0.008, 0.0))
    surface = BVHTree.FromObject(garment, bpy.context.evaluated_depsgraph_get())
    pts = []
    for k in range(25):
        a = k / 24 * math.tau
        x, y = c.x + math.cos(a) * (nk * 1.1 + loose), c.y + math.sin(a) * (nk * 1.05 + loose)
        hit = surface.ray_cast(Vector((x, y, c.z + 0.3)), Vector((0, 0, -1)), 0.6)[0]
        pts.append(Vector((x, y, (hit.z if hit is not None else c.z) + 0.004)))
    return [tube(f"{b.name}_Neckband", pts, [0.012] * 25, [0.009] * 25, mat, sides=8, cap=False)]


def collar(b, mat, height=0.08, wide=2.2, open_front=True, loose=0.02):
    """A coat's collar, as a tailor makes one: a stand that grows out of the coat's neckline and closes round
    the neck, and a fall that folds down and out over the shoulders. Its cross-section is one strip, so there is
    no gap anywhere between coat, collar and neck."""
    import bmesh
    import bpy
    nk = b.hr * b.p.get("neck_r", 0.5)
    c = b.collar
    w, d = trunk_at(b, c.z - 0.02, loose)
    # The cross-section, from inside the coat up the neck and folding out: (radius x, radius y, height).
    profile = [(w * 0.9, d * 0.9, c.z - 0.025),
               (nk * 1.3, nk * 1.25, c.z + height * 0.35),
               (nk * 1.16, nk * 1.12, c.z + height),
               (nk * (1.16 + (wide - 1.16) * 0.55), nk * (1.12 + (wide - 1.12) * 0.5), c.z + height * 0.8),
               (nk * wide, nk * wide * 0.92, c.z + height * 0.35)]
    around = 40
    bm = bmesh.new()
    rings = []
    for rx, ry, z in profile:
        rings.append([bm.verts.new((c.x + math.cos(a / around * math.tau) * rx, c.y + 0.008 + math.sin(a / around * math.tau) * ry,
                                    z - 0.008 * math.sin(a / around * math.tau * 2) * (rx > nk * 1.5)))
                      for a in range(around)])
    gap = around * 3 // 4  # the front (-y)
    for k in range(len(rings) - 1):
        for a in range(around):
            if open_front and abs(a - gap) < 1.5 and k >= 1:
                continue
            nxt = (a + 1) % around
            bm.faces.new((rings[k][a], rings[k][nxt], rings[k + 1][nxt], rings[k + 1][a]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(f"{b.name}_Collar")
    bm.to_mesh(mesh)
    bm.free()
    obj = shapes.link(bpy.data.objects.new(f"{b.name}_Collar", mesh))
    obj.modifiers.new("Thickness", 'SOLIDIFY').thickness = 0.008
    sub = obj.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = sub.render_levels = 2
    shapes.apply_all(obj)
    return [shapes.finish(obj, mat)]


def lapels(b, mat, loose=0.012):
    """An open coat's lapels, folded back on the chest in a V."""
    out = []
    for i, s in enumerate((-1, 1)):
        top_pt = b.collar + Vector((s * b.width("collar") * 0.55, -b.depth("collar") - loose - 0.004, -0.005))
        low = b.waist + Vector((s * 0.02, -b.depth("waist") - loose - 0.006, 0.04))
        mid = top_pt.lerp(low, 0.45) + Vector((s * b.width("chest") * 0.32, -0.012, 0))
        pts = spline([top_pt, mid, low], 4)
        widths = [0.03 + 0.03 * math.sin(math.pi * min(1, k / (len(pts) - 1) * 1.3)) for k in range(len(pts))]
        widths[-1] = 0.004
        out.append(tube(f"{b.name}_Lapel{i}", pts, widths, [0.006] * len(pts), mat, normals=[Vector((s * 0.4, -1, 0))] * len(pts), sides=8))
    return out


def shirt_front(b, mat, loose=0.012):
    """A light shirt showing in the V of an open coat."""
    x, y = axis_at(b, b.chest.z)
    return [panel(f"{b.name}_ShirtFront", (x, y), b.collar.z - 0.005, b.waist.z + 0.05, lambda v: 0.42 * (1 - v) + 0.05,
                  lambda z: (trunk_at(b, z, loose + 0.003)), mat, thickness=0.003, rows=8, columns=8)]


def apron(b, mat, tie, bib=True, loose=0.03, hem=0.33, flare=1.2, over=()):
    """A leather work apron: a bib on the chest, a skirt to the knee, a neck strap and ties.
    over: the garments beneath; the apron hangs a little outside their real surface at every height."""
    x, y = axis_at(b, b.waist.z)
    top_z = b.chest.z + 0.03 if bib else b.waist.z + 0.03
    hem_z = hem * b.H
    span = lambda v: (0.45 if bib and v < 0.35 else 0.62) if not bib else 0.36 + 0.36 * min(1.0, max(0.0, (v - 0.25) / 0.2))
    def radius(z):
        w, d = trunk_at(b, max(z, b.pelvis.z - 0.02), loose)
        t = min(1.0, max(0.0, (b.waist.z - z) / max(b.waist.z - hem_z, 1e-3)))
        # Leather hangs close: widening only as much as the cloth beneath does (measured below).
        k = 1 + (flare - 1) * t ** 1.5
        w, d = w * k, d * k
        if over:
            centre = axis_at(b, z)
            gap = 0.008 + 0.012 * t
            for angle, is_front in ((-math.pi / 2, True), (-math.pi / 2 + 0.6, True), (-math.pi / 2 - 0.6, True)):
                hits = [h for h in (surface_hit(g, centre, z, angle) for g in over) if h is not None]
                if hits:
                    # The ellipse's front is what the apron lies on; keep it just outside the deepest garment.
                    need = max(hits) + gap
                    if angle == -math.pi / 2:
                        d = max(d, need)
                    else:
                        w = max(w, need * 0.95)
        return w, d
    out = [panel(f"{b.name}_Apron", (x, y), top_z, hem_z, span, radius, mat, thickness=0.007, rows=14, columns=14, folds=0.05, fold_count=3, seed=3)]
    if bib:
        # The neck strap: from the bib's top corners up over the shoulders, lying on the smock, crossing at the back.
        top_garments = [g for g in over if g.name.endswith("Probe")]
        for s in (-1, 1):
            w0, d0 = trunk_at(b, top_z, loose)
            path = [Vector((x + s * w0 * 0.33, y - d0 - 0.01, top_z)),
                    Vector((b.chest.x + s * b.width("chest") * 0.42, b.chest.y - b.depth("chest") - 0.02, (top_z + b.collar.z) * 0.5)),
                    b.collar + Vector((s * b.width("collar") * 0.95, -0.02, 0.04)),
                    b.collar + Vector((s * b.width("collar") * 0.9, 0.04, 0.03)),
                    Vector((b.chest.x - s * b.width("chest") * 0.2, b.chest.y + b.depth("chest"), b.chest.z)),
                    Vector((b.waist.x - s * b.width("waist") * 0.45, b.waist.y + b.depth("waist"), b.waist.z + 0.01))]
            laid, nms = lay_on(b, spline(path, 4), horizontal=top_garments + [g for g in over if not g.name.endswith("Probe")], down=top_garments, lift=0.004)
            out.append(tube(f"{b.name}_ApronStrap{s}", laid, [0.009] * len(laid), [0.0028] * len(laid), mat, normals=nms, sides=6))
    # The ties run round the waist on the garment's real surface, from the apron's sides to a knot at the back.
    w, d = trunk_at(b, b.waist.z, loose)
    for s in (-1, 1):
        pts = []
        for k in range(9):
            angle = -math.pi / 2 + s * (0.75 + k / 8 * (math.pi / 2 + 0.6))
            r = max([h for h in (surface_hit(g, (x, y), b.waist.z, angle) for g in over) if h is not None] or [None]) if over else None
            if r is None:
                r = math.hypot(w * math.cos(angle), d * math.sin(angle))
            pts.append(Vector((x + math.cos(angle) * (r + 0.005), y + math.sin(angle) * (r + 0.005), b.waist.z - 0.004 * k / 8)))
        out.append(tube(f"{b.name}_ApronTie{s}", pts, [0.008] * 9, [0.003] * 9, tie, sides=6))
    return out


def patch(b, mat, at, size, angle=0.0, name="Patch", onto=None):
    """A patch or pocket sewn flat onto a garment: cast from in front onto its surface at (x, z) of at,
    and turned to lie along it. Its centre is kept on b.marks[name], for things tucked into it."""
    rotation = Matrix.Rotation(angle, 3, 'Y')
    at = Vector(at)
    if onto is not None:
        from mathutils.bvhtree import BVHTree
        import bpy
        tree = BVHTree.FromObject(onto, bpy.context.evaluated_depsgraph_get())
        hit, normal, _, _ = tree.ray_cast(Vector((at.x, at.y - 0.5, at.z)), Vector((0, 1, 0)), 1.0)
        if hit is not None:
            if normal.y > 0:
                normal = -normal
            along = Vector((0, 0, 1)).cross(normal).normalized()
            rotation = Matrix.Rotation(angle, 3, normal) @ Matrix((along, -normal, along.cross(-normal))).transposed()
            at = hit + normal * (size[1] + 0.002)
    if not hasattr(b, "marks"):
        b.marks = {}
    b.marks[name] = at
    return [slab(f"{b.name}_{name}", at, size, mat, rotation=rotation, soft=0.6)]


def buttons(b, mat, count=3, loose=0.022, size=0.014, onto=()):
    """Buttons down the front, each sewn onto the cloth beneath it (onto: the garments)."""
    out = []
    for k in range(count):
        z = b.chest.z - 0.02 - k * (b.chest.z - b.waist.z + 0.06) / max(count - 1, 1)
        x, y = axis_at(b, z)
        w, d = trunk_at(b, z, loose)
        at, normal = Vector((x, y - d - 0.004, z)), Vector((0, -1, 0))
        if onto:
            laid, nms = lay_on(b, [at], horizontal=list(onto), lift=0.003)
            at, normal = laid[0], nms[0]
        turn = frame(normal, Vector((0, 0, 1))) @ Matrix.Rotation(math.pi / 2, 3, 'Y')
        out.append(cylinder(f"{b.name}_Button{k}", at, size, 0.007, mat, rotation=turn, vertices=12))
    return out


# ---------------------------------------------------------------- accessories

def satchel(b, strap_mat, bag_mat, buckle_mat, torso, below, loose=0.024):
    """A small leather satchel hanging at the right hip from a strap over the left shoulder. The bag rests
    against the coat, turned to its surface; the strap lies on the coat all the way and ends in the bag's top,
    at two tabs. torso: the torso garments (probe) the strap lies on; below: the skirt the bag rests on."""
    from mathutils.bvhtree import BVHTree
    import bpy
    sh = b.shoulders[1]
    z = b.pelvis.z - 0.03
    cx, cy = axis_at(b, z)
    # Where the bag rests: on the skirt at the side of the right hip, a little to the front.
    angle = math.pi + 0.45  # from +x round to the right side, then towards the front (-y)
    side = Vector((math.cos(angle), math.sin(angle), 0))
    reach = max([h for h in (surface_hit(g, (cx, cy), z, angle) for g in below + torso) if h is not None] or [trunk_at(b, z, loose)[0]])
    half = Vector((0.07, 0.022, 0.06))  # half width, half depth, half height
    rest = Vector((cx, cy, z)) + side * (reach + half.y + 0.004)
    across = Vector((0, 0, 1)).cross(side).normalized()
    turn = frame(across, side)  # local x along the hip, y out from it
    out = [slab(f"{b.name}_Bag", rest, (half.x, half.y, half.z), bag_mat, rotation=turn, soft=0.7)]
    out.append(slab(f"{b.name}_BagFlap", rest + side * (half.y + 0.004) + Vector((0, 0, half.z * 0.35)),
                    (half.x * 1.04, 0.005, half.z * 0.65), bag_mat, rotation=turn, soft=0.5))
    out.append(slab(f"{b.name}_Buckle", rest + side * (half.y + 0.011) + Vector((0, 0, -half.z * 0.1)), (0.011, 0.004, 0.011), buckle_mat, rotation=turn, soft=0.3))
    # The strap: from the bag's top (the end nearer the front) up across the body, over the left shoulder and down
    # the back to the bag's other end; it lies on the coat wherever it touches.
    front_end = rest + across * (-half.x * 0.8) + Vector((0, 0, half.z))
    back_end = rest + across * (half.x * 0.8) + Vector((0, 0, half.z))
    wc, dc = trunk_at(b, b.chest.z, loose)
    path = [front_end + Vector((0, 0, 0.02)),
            Vector((b.waist.x - b.width("waist") * 0.4, b.waist.y - b.depth("waist") - 0.02, b.waist.z)),
            Vector((b.chest.x + wc * 0.35, b.chest.y - dc - 0.02, b.chest.z + 0.01)),
            sh + Vector((-0.01, -0.03, b.p["arm"] * 1.3)),
            sh + Vector((-0.015, 0.04, b.p["arm"] * 1.2)),
            Vector((b.chest.x + wc * 0.2, b.chest.y + dc, b.chest.z - 0.02)),
            Vector((b.waist.x - b.width("waist") * 0.7, b.waist.y + b.depth("waist"), b.waist.z - 0.02)),
            back_end + Vector((0, 0, 0.02))]
    pts = spline(path, 6)
    laid, nms = lay_on(b, pts, horizontal=torso + below, down=[g for g in bpy.data.objects if g.name == f"{b.name}_Top"], lift=0.004)
    # Where it crosses the buttons down the front, it rides over them.
    for k, q in enumerate(laid):
        cx, cy = axis_at(b, q.z)
        if q.y < cy:
            laid[k] = q + nms[k] * 0.008 * max(0.0, 1 - abs(q.x - cx) / 0.05)
    # The ends meet the bag's top exactly.
    laid[0], laid[-1] = front_end + Vector((0, 0, 0.004)), back_end + Vector((0, 0, 0.004))
    out.append(tube(f"{b.name}_Strap", laid, [0.014] * len(laid), [0.003] * len(laid), strap_mat, normals=nms, sides=8))
    for k, end in enumerate((front_end, back_end)):
        out.append(slab(f"{b.name}_StrapTab{k}", end + Vector((0, 0, -0.006)) + side * 0.002, (0.012, 0.006, 0.016), strap_mat, rotation=turn, soft=0.4))
    return out


def bandolier(b, strap_mat, torso, below, start, end, loose=0.02):
    """A strap across the body from one shoulder to the opposite hip (to carry something on the back),
    lying on the clothes."""
    import bpy
    # Over the left shoulder, where the pick's head rides, across the chest to the right hip, and round the back.
    sh = b.shoulders[1]
    wc, dc = trunk_at(b, b.chest.z, loose)
    path = [Vector(start),
            sh + Vector((-0.01, 0.04, b.p["arm"] * 1.2)),
            sh + Vector((-0.01, -0.03, b.p["arm"] * 1.3)),
            Vector((b.chest.x + wc * 0.2, b.chest.y - dc - 0.02, b.chest.z)),
            Vector((b.waist.x - b.width("waist") * 0.55, b.waist.y - b.depth("waist") - 0.02, b.waist.z + 0.03)),
            Vector((b.waist.x - b.width("waist") * 1.0, b.waist.y, b.waist.z + 0.01)),
            Vector((b.waist.x - b.width("waist") * 0.4, b.waist.y + b.depth("waist") + 0.02, b.waist.z + 0.01)),
            Vector(end)]
    laid, nms = lay_on(b, spline(path, 5), horizontal=torso + below, down=[g for g in bpy.data.objects if g.name == f"{b.name}_Top"], lift=0.004)
    return [tube(f"{b.name}_PickStrap", laid, [0.014] * len(laid), [0.003] * len(laid), strap_mat, normals=nms, sides=8)]


def lantern(b, grasp, brass, glass, name="Lantern"):
    """A miner's lamp hanging from a hand: a brass base and cap around warm glass, on a wire bail."""
    top = grasp - Vector((0, 0, 0.012))
    out = [shapes.torus(f"{b.name}_{name}Bail", top - Vector((0, 0, 0.03)), 0.028, 0.0025, brass, rotation=(0, math.pi / 2, 0))]
    body = top - Vector((0, 0, 0.06))
    out.append(lathe(f"{b.name}_{name}Cap", [(0.012, 0.0), (0.036, -0.028), (0.034, -0.034), (0.0, -0.034)], brass, at=body + Vector((0, 0, 0.034))))
    out.append(lathe(f"{b.name}_{name}Glass", [(0.0, 0.0), (0.026, 0.0), (0.029, 0.035), (0.026, 0.07), (0.0, 0.07)], glass, at=body - Vector((0, 0, 0.07))))
    for k in range(4):
        a = k / 4 * math.tau + 0.4
        x, y = math.cos(a) * 0.031, math.sin(a) * 0.031
        out.append(tube(f"{b.name}_{name}Bar{k}", [body + Vector((x, y, 0)), body + Vector((x * 1.08, y * 1.08, -0.035)), body + Vector((x, y, -0.07))],
                        [0.0025] * 3, [0.0025] * 3, brass, sides=6, levels=0))
    out.append(lathe(f"{b.name}_{name}Base", [(0.0, 0.0), (0.034, 0.0), (0.036, 0.012), (0.03, 0.018), (0.0, 0.018)], brass, at=body - Vector((0, 0, 0.088))))
    return out


def pickaxe(b, grasp, along, wood, iron, length=0.86, hold=0.22):
    """A pickaxe whose handle runs through the hand along a direction (towards its head).
    hold: where along the handle the hand is, from its foot (0) to its head (1)."""
    d = Vector(along).normalized()
    low, high = grasp - d * length * hold, grasp + d * length * (1 - hold)
    bow = d.cross(Vector((1, 0, 0))).normalized() * 0.012
    handle = [low, low.lerp(high, 0.35) + bow, low.lerp(high, 0.7) + bow, high]
    out = [tube(f"{b.name}_PickHandle", spline(handle, 4), [0.0145, 0.015, 0.0155, 0.016, 0.0165, 0.017, 0.0175, 0.018, 0.0185, 0.019, 0.019, 0.0185, 0.018],
                [0.0125, 0.013, 0.0135, 0.014, 0.0145, 0.015, 0.0155, 0.016, 0.0165, 0.017, 0.017, 0.0165, 0.016], wood, sides=10)]
    across = d.cross(Vector((1, 0, 0)))
    if across.length < 0.1:
        across = d.cross(Vector((0, 1, 0)))
    across.normalize()
    curve = -d * 0.04
    head = [high + across * 0.25 + curve, high + across * 0.12 + curve * 0.3, high, high - across * 0.1 + curve * 0.25, high - across * 0.2 + curve * 0.8]
    widths = [0.0, 0.016, 0.026, 0.018, 0.004]
    out.append(tube(f"{b.name}_PickHead", spline(head, 3), widths[:1] + [0.008, 0.012, 0.016, 0.02, 0.024, 0.026, 0.025, 0.022, 0.019, 0.015, 0.01, 0.005],
                    [0.0, 0.007, 0.01, 0.013, 0.016, 0.019, 0.021, 0.021, 0.019, 0.017, 0.015, 0.012, 0.009], iron, normals=[d] * 13, sides=8))
    out.append(cylinder(f"{b.name}_PickCollar", high - d * 0.01, 0.024, 0.05, iron, rotation=frame(d, across) @ Matrix.Rotation(math.pi / 2, 3, 'Y')))
    return out


def mug(b, centre, mat, name="Mug"):
    """A tin mug, held warm in the hand."""
    out = [lathe(f"{b.name}_{name}", [(0.0, 0.0), (0.036, 0.0), (0.04, 0.004), (0.041, 0.085), (0.044, 0.09), (0.037, 0.09), (0.035, 0.012), (0.0, 0.012)],
                 mat, at=centre - Vector((0, 0, 0.045)))]
    out.append(shapes.torus(f"{b.name}_{name}Handle", centre + Vector((0.048, 0, 0.005)), 0.022, 0.005, mat, rotation=(math.pi / 2, 0, 0)))
    return out


def lamp_cap(b, leather, brass, glass):
    """A miner's leather cap with a small brass lamp on its front."""
    hm = b.hm
    s = b.hr
    profile = [(0.0, 1.27), (0.45, 1.22), (0.8, 1.05), (1.02, 0.75), (1.1, 0.42), (1.12, 0.32), (1.08, 0.3)]
    pts = [(r * s, (z + 0.0) * s) for r, z in profile]
    centre = b.on_head(0, 0.08, 0.05)
    cap = lathe(f"{b.name}_Cap", [(r * b.hs.x, z) for r, z in pts], leather, at=(0, 0, 0), segments=32)
    cap.data.transform(Matrix.Translation(centre) @ hm.to_4x4() @ Matrix.Diagonal((1, b.hs.y / b.hs.x, b.hs.z, 1)))
    out = [cap]
    brim = [b.on_head(x, -1.16 + 0.1 * abs(x) ** 2, 0.36 - 0.05 * abs(x)) for x in (-0.75, -0.4, 0.0, 0.4, 0.75)]
    out.append(tube(f"{b.name}_CapBrim", spline(brim, 4), [0.02] * 17, [0.004] * 17, leather, normals=[hm @ Vector((0, 0, 1))] * 17, sides=8))
    lamp_at = b.on_head(0, -1.17, 0.78)
    forward = hm @ Vector((0, -1, 0))
    rot = frame(forward, hm @ Vector((0, 0, 1)))
    out.append(cylinder(f"{b.name}_CapLamp", lamp_at, 0.024, 0.032, brass, rotation=rot @ Matrix.Rotation(math.pi / 2, 3, 'Y'), vertices=20))
    out.append(cylinder(f"{b.name}_CapLens", lamp_at + forward * 0.017, 0.018, 0.004, glass, rotation=rot @ Matrix.Rotation(math.pi / 2, 3, 'Y'), vertices=20, bevel=0))
    out.append(slab(f"{b.name}_CapLampMount", lamp_at - forward * 0.02 - hm @ Vector((0, 0, 0.012)), (0.012, 0.01, 0.02), brass, rotation=rot.to_euler(), soft=0.3))
    return out


def hammer(b, at, tilt, wood, iron):
    """A small hammer tucked into a pocket, its head showing."""
    d = Vector(tilt).normalized()
    out = [tube(f"{b.name}_HammerHandle", [at - d * 0.12, at, at + d * 0.12], [0.009, 0.01, 0.011], [0.009, 0.01, 0.011], wood, sides=8)]
    head = at + d * 0.125
    side = d.cross(Vector((0, 1, 0))).normalized()
    out.append(slab(f"{b.name}_HammerHead", head, (0.045, 0.014, 0.016), iron, rotation=frame(side, d).to_euler(), soft=0.4))
    return out
