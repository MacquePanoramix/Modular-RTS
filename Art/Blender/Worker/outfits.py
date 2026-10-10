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
    b.skirt_hem = hem_z  # it hangs in four flaps, each on a bone at its hip (rigging.joints)
    b.skirt_open = open_front
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
        knees = z <= b.pelvis.z - 0.28  # lower still (a long coat), it lies over the knees: see below
        angles = ((0.0, math.pi, -math.pi / 2, math.pi / 2, -math.pi / 4, -3 * math.pi / 4, math.pi / 4, 3 * math.pi / 4)
                  if z > b.pelvis.z - 0.1 else (0.0, math.pi) if not knees else ())
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
                    k_ = need / r
                    w, d = w * k_, d * k_
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
            xo, yo = math.cos(ang) * w * widen * fold, math.sin(ang) * d * widen * fold * 1.05
            if knees:
                # A long coat lies over the knees, which stand a little forward even at rest: the cloth goes out
                # only where a leg is, and only as far as the leg asks (its own thickness, and a little room). Its
                # shape elsewhere stays as measured. (Growing the whole ring to clear the knees made it a bell.)
                reach = math.hypot(xo, yo)
                for g in under:
                    h = surface_hit(g, (cx, cy), z - drop, math.atan2(yo, xo))
                    if h is not None and reach > 1e-6 and reach < h + 0.024:
                        xo, yo = xo * (h + 0.024) / reach, yo * (h + 0.024) / reach
                        reach = h + 0.024
            ring.append(bm.verts.new((cx + xo, cy + yo, z - drop)))
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
    b.skirt_slack = skirt_slack(b, obj, hem_z)
    return [shapes.finish(obj, mat)]


def skirt_slack(b, garment, hem_z):
    """How far a thigh can swing forward, and back, before the leg reaches a skirt's cloth, in degrees: the room
    between the leg and the cloth as modelled, seen from the hip the thigh turns about. The game pushes the
    skirt's flaps only beyond this (MinerBody), so cloth the leg does not touch hangs still."""
    surface = shapes.Surface(garment)
    leg = b.p["leg"] * 1.25 + 0.012  # a trouser leg, a little generous
    slack = [90.0, 90.0]
    for i in (0, 1):
        hip, knee, ankle = b.hips[i], b.knees[i], b.ankles[i]
        z = hip.z - 0.1
        while z > hem_z + 0.01:
            # The leg's axis at this height: the thigh, or below the knee the shin.
            at = hip.lerp(knee, (hip.z - z) / max(hip.z - knee.z, 1e-4)) if z >= knee.z else knee.lerp(ankle, (knee.z - z) / max(knee.z - ankle.z, 1e-4))
            for k, way in enumerate((-1, 1)):  # to the front (-y), to the back
                if k == 0 and z < knee.z:
                    continue  # a shin hangs back from a thigh that swings forward
                hit, _ = surface.cast(Vector((at.x, at.y, z)), Vector((0, way, 0)), 0.6)
                if hit is None:
                    continue
                room = abs(hit.y - at.y) - leg
                slack[k] = min(slack[k], math.degrees(math.atan2(max(room, 0.0), hip.z - z)))
            z -= 0.02
    return [round(s if s < 90 else 0.0, 2) for s in slack]


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


def trunk_point(b, p):
    """The nearest point to p on the trunk's axis (from under the hips up to the collar)."""
    axis = [b.pelvis + Vector((0, 0, -0.3)), b.pelvis, b.waist, b.chest, b.collar]
    best = None
    for p0, p1 in zip(axis, axis[1:]):
        d = p1 - p0
        u = max(0.0, min(1.0, (Vector(p) - p0).dot(d) / max(d.length_squared, 1e-9)))
        q = p0 + d * u
        if best is None or (Vector(p) - q).length < (Vector(p) - best).length:
            best = q
    return best


def taut(b, points, torso, clothes, lift=0.004, rounds=260, per=5, keep=(), free=0.09, width=0.0):
    """A strap pulled tight over the clothes, as its load pulls it. The points are laid on the garments' real
    surface (cast from outside towards the trunk's axis, so the path turns smoothly over a shoulder), then each is
    drawn towards the middle of its neighbours and laid again, over and over: the path slides to the short, straight
    line a taut strap takes, with no kinks. The two ends stay where they are (a strap's ends are on its rings),
    and so do the points in keep (a strap hangs from the shoulder it bears on: left free, the shortest path between
    two rings at the hip would slip down round the waist). Over its last stretch (free) before each end the strap
    leaves the cloth and runs straight to its ring, as a strap under load does.
    torso: the torso alone (the probe) and what hangs below it, for the chest, waist and hips, where the arms hang
    beside the body; clothes: the real garments (shoulders, collar), for above the chest.
    Returns the laid points and the surface's normals."""
    under, over = shapes.Surface(*torso), shapes.Surface(*clothes)

    def lay(p):
        c = trunk_point(b, p)
        d = Vector(p) - c
        if d.length < 1e-4:
            d = Vector((0, -1, 0))
        d.normalize()
        for surface in ((over, under) if p.z > b.chest.z + 0.02 else (under, over)):
            hit, nm = surface.cast(c + d * 0.6, -d, 0.6)
            if hit is not None:
                return hit + nm * lift, nm
        return Vector(p), d

    pts = [Vector(q) for q in points]
    laid = [pts[0]] + [lay(q)[0] for q in pts[1:-1]] + [pts[-1]]
    for _ in range(rounds):
        for k in range(1, len(laid) - 1):
            if k in keep:
                continue
            laid[k] = lay(laid[k].lerp((laid[k - 1] + laid[k + 1]) * 0.5, 0.5))[0]
    fine = spline(laid, per)
    out, normals = [fine[0]], [None]
    for q in fine[1:-1]:
        hit, nm = lay(q)
        out.append(hit)
        normals.append(nm)
    out.append(fine[-1])
    normals.append(None)
    normals[0], normals[-1] = normals[1], normals[-2]
    if width:
        # A strap is flat across its width: where the cloth curves under it (over a collar's rim), it rides on the
        # highest of what lies under its edges and middle, and bridges the rest; it never cuts into the cloth.
        raised = []
        for k in range(1, len(out) - 1):
            along = (out[k + 1] - out[k - 1]).normalized()
            across = along.cross(normals[k]).normalized()
            rise = max((lay(out[k] + across * s_)[0] - out[k]).dot(normals[k]) for s_ in (-width, -width * 0.5, width * 0.5, width))
            raised.append(out[k] + normals[k] * max(0.0, rise))
        out[1:-1] = raised
    release(b, out, free)
    return out, normals


def release(b, out, free):
    """The free stretch at each end of a strap: from where it leaves the cloth, straight to its ring, as a strap
    under load runs. out: the strap's points, changed in place."""
    for order in (range(len(out)), range(len(out) - 1, -1, -1)):
        order = list(order)
        run, leave = 0.0, None
        for a_, c_ in zip(order, order[1:]):
            run += (out[c_] - out[a_]).length
            if run >= free:
                leave = c_
                break
        if leave is None:
            continue
        end_, far, span = out[order[0]], out[leave], run
        run = 0.0
        for a_, c_ in zip(order, order[1:]):
            if c_ == leave:
                break
            run += (out[c_] - out[a_]).length
            u = run / span
            straight = end_.lerp(far, u)
            # Never inside the cloth: the straight line, or the cloth where it rises above the line.
            if (straight - trunk_point(b, straight)).length >= (out[c_] - trunk_point(b, out[c_])).length - 0.001:
                out[c_] = straight


def worn(b, points, torso, coat, lift=0.004, free=0.09):
    """A strap worn soft on the body, along the course it is given: every point laid on the coat (from above over
    the shoulder, towards the trunk's axis below it), not drawn tight. It keeps the broad, easy line of a leather
    strap resting on a shoulder and across a back. Luis preferred this on Small's shoulder (October 5) to the
    strap pulled taut, which sat nearer the neck and climbed over the collar. Its ends are on their rings: over
    its last stretch before each it leaves the cloth and runs straight to the ring (release).
    Returns the laid points and the surface's normals."""
    laid, normals = lay_on(b, spline(points, 6), horizontal=list(torso), down=list(coat), lift=lift)
    laid[0], laid[-1] = Vector(points[0]), Vector(points[-1])
    release(b, laid, free)
    return laid, normals


def neckband(b, mat, garment, loose=0.006):
    """A smock's rolled neckline, hugging the neck where it leaves the garment. Each point of the ring sits on
    the garment's own surface (found by casting down onto it), at the band's measure round the neck. Where the
    place the neck really comes out of the cloth is not under the band there (at the back of a round neck the
    cloth falls away steeply, and the band would land low), the band moves to that place: the join of skin and
    cloth is what a neckband is for, and left bare it shows as slivers of skin and cloth."""
    from mathutils.bvhtree import BVHTree
    import bpy
    nk = b.hr * b.p.get("neck_r", 0.5)
    c = b.collar + Vector((0, 0.008, 0.0))
    depsgraph = bpy.context.evaluated_depsgraph_get()
    surface = BVHTree.FromObject(garment, depsgraph)
    skin = bpy.data.objects.get(f"{b.name}_Skin")
    neck = BVHTree.FromObject(skin, depsgraph) if skin is not None else None
    count = 24
    measured, joins, lean = [], [], []
    for k in range(count):
        a = k / count * math.tau
        ux, uy = math.cos(a), math.sin(a)
        rx, ry = nk * 1.1 + loose, nk * 1.05 + loose

        def on_cloth(grow):
            x_, y_ = c.x + ux * (rx + grow), c.y + uy * (ry + grow)
            found = surface.ray_cast(Vector((x_, y_, c.z + 0.3)), Vector((0, 0, -1)), 0.6)[0]
            return x_, y_, (found if found is not None and found.z > c.z - 0.06 else None)
        grow = 0.0
        x, y, hit = on_cloth(grow)
        while hit is None and grow < 0.045:
            grow += 0.002
            x, y, hit = on_cloth(grow)
        measured.append(Vector((x, y, (hit.z if hit is not None else c.z) + 0.004)))
        # The join: going down the neck, the first height at which the cloth, not the skin, is outermost.
        join, d = None, Vector((ux, uy, 0))
        z = c.z + 0.05
        while neck is not None and z > c.z - 0.05:
            origin = Vector((c.x, c.y, z)) + d * 0.3
            on_skin, on_cloth_ = neck.ray_cast(origin, -d, 0.3)[0], surface.ray_cast(origin, -d, 0.3)[0]
            if on_cloth_ is not None and (on_skin is None or (origin - on_cloth_).length <= (origin - on_skin).length):
                join = on_cloth_ + d * 0.006 - Vector((0, 0, 0.002))
                break
            z -= 0.002
        joins.append(join)
        lean.append((d + Vector((0, 0, 1))).normalized())
    # How far the band must go to the join: nothing where the join is under it already, and nothing at the front,
    # where the chin hides the join and the neckline keeps the look it had.
    move = []
    for k, (at, join) in enumerate(zip(measured, joins)):
        away = (join - at).length if join is not None else 0.0
        back = min(1.0, max(0.0, (math.sin(k / count * math.tau) + 0.3) / 0.5))
        move.append(min(1.0, max(0.0, (away - 0.006) / 0.008)) * back * back * (3 - 2 * back))
    for _ in range(2):  # ease it in round the ring, so the band keeps a smooth line
        move = [max(move[k], (move[k - 1] + move[k] * 2 + move[(k + 1) % count]) * 0.25) for k in range(count)]
    pts = [at.lerp(join, w) if join is not None else at for at, join, w in zip(measured, joins, move)]
    for _ in range(2):
        pts = [(pts[k - 1] + pts[k] * 2 + pts[(k + 1) % count]) * 0.25 if 0 < move[k] else pts[k] for k in range(count)]
    print(f"NECKBAND {b.name}: moved to the join by (mm) " + " ".join(f"{(q - at).length * 1000:.0f}" for q, at in zip(pts, measured)))
    return [tube(f"{b.name}_Neckband", pts, [0.012] * count, [0.009] * count, mat, normals=lean, sides=8, closed=True)]


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


def apron(b, mat, tie, hem=0.33, over=(), neck=()):
    """A leather work apron, as it is made and as it hangs. A bib on the chest and a skirt to below the knee, cut
    from one hide. The neck strap and the waist's ties hold it to the body: down to the ties it lies on the
    clothes beneath it (each point is laid on their real surface), drawn in at the waist by the ties; below them
    it hangs from the waist, as leather does, never tucking back under.
    A strap goes from one corner of the bib's top round the back of the neck to the other: the apron hangs from
    it. A tie from each side at the waist goes round to a knot at the back, its two ends hanging.
    over: the garments beneath (the torso's probe and the skirt); neck: what the neck strap lies on."""
    cloth = shapes.Surface(*over)
    x0, y0 = axis_at(b, b.waist.z)
    top_z, hem_z = b.chest.z + 0.03, hem * b.H
    rows, columns, thick = 16, 14, 0.007

    def span(v):
        return 0.36 + 0.36 * min(1.0, max(0.0, (v - 0.25) / 0.2))

    def reach(angle, z):
        """How far out the clothes are at a height and angle from the front, from the trunk's axis there."""
        cx, cy = axis_at(b, z)
        d = Vector((math.cos(angle), math.sin(angle), 0))
        hit, _ = cloth.cast(Vector((cx, cy, z)) + d * 0.7, -d, 0.7)
        return None if hit is None else (Vector((hit.x - cx, hit.y - cy, 0)).length, cx, cy)

    def spread(v, z):
        """The apron's half-width at a height, as an angle from the front. The bib is as broad across as its span
        of the chest's measured width (a bib is cut to the chest, not to how round the belly under it is); from
        the row above the ties down, it keeps its span, so the waist and all below it hang as they were fitted."""
        bib = min(1.0, max(0.0, ((j_tie - 1) / rows - v) / 0.12))
        if bib <= 0:
            return span(v)
        want = math.sin(span(v)) * trunk_at(b, max(z, b.pelvis.z - 0.02), 0.03)[0]
        angle = span(v)
        for _ in range(4):
            found = reach(-math.pi / 2 + angle, z)
            if found is None:
                break
            angle = max(span(v), math.asin(min(0.95, want / max(found[0], 1e-3))))
        return span(v) + (angle - span(v)) * bib

    # The row the ties are sewn to, at the waist.
    j_tie = min(rows, max(0, round((top_z - (b.waist.z - 0.01)) / (top_z - hem_z) * rows)))
    tie_z = top_z + (hem_z - top_z) * j_tie / rows
    grid, last = [], {}
    for j in range(rows + 1):
        v = j / rows
        z = top_z + (hem_z - top_z) * v
        row = []
        wide = spread(v, z)
        for i in range(columns + 1):
            u = i / columns * 2 - 1
            angle = -math.pi / 2 + u * wide
            found = reach(angle, z)
            # Leather a little off the cloth, more towards the hem; below the fullest point it hangs straight.
            gap = 0.003 + thick * 0.5 + 0.012 * v
            if found is None:
                r, cx, cy = last.get(i, (trunk_at(b, z, 0.03)[1], x0, y0))
            else:
                r, cx, cy = found[0] + gap, found[1], found[2]
                if i in last and j > j_tie:
                    r = max(r, last[i][0] - 0.0015)
            last[i] = (r, cx, cy)
            fold = 1 + 0.02 * v * math.sin((u + 1) * 3 * math.pi + 3)
            row.append(Vector((cx + math.cos(angle) * r * fold, cy + math.sin(angle) * r * fold, z)))
        grid.append(row)
    out = [shapes.sheet(f"{b.name}_Apron", grid, mat, thickness=thick)]
    # The neck strap: from the bib's top corners up over the collar bones and round the back of the neck.
    half_w, half_t = 0.0085, 0.0025
    left, right = grid[0][-1], grid[0][0]
    nape = b.collar + Vector((0, b.depth("collar") + 0.03, 0.02))
    path = [left + Vector((-0.004, -0.002, -0.012)),
            b.collar + Vector((b.width("collar") * 0.9, -0.03, 0.03)),
            b.collar + Vector((b.width("collar") * 0.8, 0.04, 0.035)),
            nape,
            b.collar + Vector((-b.width("collar") * 0.8, 0.04, 0.035)),
            b.collar + Vector((-b.width("collar") * 0.9, -0.03, 0.03)),
            right + Vector((0.004, -0.002, -0.012))]
    # It bears on the back of the neck (those points stay); each half is drawn taut down to its corner of the bib.
    laid, nms = taut(b, spline(path, 2), list(over), list(neck), lift=0.004, keep=(4, 5, 6, 7, 8), free=0.03, width=half_w)
    out.append(tube(f"{b.name}_ApronStrap", laid, [half_w] * len(laid), [half_t] * len(laid), tie, normals=nms, sides=6))
    # The ties: from each side of the apron at the waist round to a knot at the back.
    knot = None
    for k, (edge, s) in enumerate(((grid[j_tie][0], -1), (grid[j_tie][-1], 1))):
        cx, cy = axis_at(b, tie_z)
        start_a = math.atan2(edge.y - cy, edge.x - cx)
        end_a = math.pi / 2  # the middle of the back
        if s < 0:
            sweep = [start_a - (start_a - (-math.pi * 1.5)) * f for f in (0.0, 0.25, 0.5, 0.75, 1.0)]
        else:
            sweep = [start_a + (end_a - start_a) * f for f in (0.0, 0.25, 0.5, 0.75, 1.0)]
        pts = []
        for a_ in sweep:
            found = reach(a_, tie_z)
            r = (found[0] if found else trunk_at(b, tie_z, 0.02)[0]) + 0.004
            pts.append(Vector((cx + math.cos(a_) * r, cy + math.sin(a_) * r, tie_z)))
        fine = spline(pts, 4)
        laid, nms = [], []
        for q in fine:
            cx2, cy2 = axis_at(b, q.z)
            d_ = Vector((q.x - cx2, q.y - cy2, 0)).normalized()
            hit, nm = cloth.cast(Vector((cx2, cy2, q.z)) + d_ * 0.7, -d_, 0.7)
            laid.append(hit + nm * 0.0035 if hit is not None else q)
            nms.append(nm if hit is not None else d_)
        # It is sewn to the apron, a finger's width in from the edge (on the apron's real, softened surface).
        laid[0] = shapes.Surface(out[0]).nearest(grid[j_tie][1 if s < 0 else -2])[0]
        knot = laid[-1]
        out.append(tube(f"{b.name}_ApronTie{k}", laid, [0.007] * len(laid), [0.0022] * len(laid), tie, normals=nms, sides=6))
    # The knot, and the ties' two ends hanging from it down the back.
    back = Vector((0, 1, 0))
    out.append(ellipsoid(f"{b.name}_ApronKnot", knot + back * 0.004, (0.013, 0.008, 0.01), tie, segments=10))
    for k, s in enumerate((-1, 1)):
        hang = [knot + Vector((s * 0.004, 0, -0.004)), knot + Vector((s * 0.014, 0, -0.035)), knot + Vector((s * 0.02, 0, -0.075))]
        laid = [cloth.lay(q, 0.0035) for q in hang]
        out.append(tube(f"{b.name}_ApronTieEnd{k}", [q for q, _ in laid], [0.007, 0.007, 0.006], [0.0022] * 3, tie, normals=[n_ for _, n_ in laid], sides=6))
    return out


def patch(b, mat, at, size, angle=0.0, name="Patch", onto=None):
    """A patch or pocket sewn flat onto a garment: cast from in front onto its surface at (x, z) of at, turned to
    lie along it, and then every point of it is laid on the cloth, so it follows the cloth's curve and never
    stands off it. Its centre is kept on b.marks[name], for things tucked into it."""
    rotation = Matrix.Rotation(angle, 3, 'Y')
    at = Vector(at)
    normal, surface = Vector((0, -1, 0)), None
    if onto is not None:
        surface = shapes.Surface(onto)
        hit, normal_ = surface.cast(Vector((at.x, at.y - 0.5, at.z)), Vector((0, 1, 0)), 1.0)
        if hit is not None:
            normal = normal_
            along = Vector((0, 0, 1)).cross(normal).normalized()
            rotation = Matrix.Rotation(angle, 3, normal) @ Matrix((along, -normal, along.cross(-normal))).transposed()
            at = hit + normal * (size[1] + 0.001)
    if not hasattr(b, "marks"):
        b.marks = {}
    b.marks[name] = at
    obj = slab(f"{b.name}_{name}", at, size, mat, rotation=rotation, soft=0.6)
    if surface is not None:
        # Enough points across it to follow the cloth's curve (a flat slab has none in the middle of its faces).
        import bmesh
        bm = bmesh.new()
        bm.from_mesh(obj.data)
        bmesh.ops.subdivide_edges(bm, edges=[e for e in bm.edges if e.calc_length() > 0.02], cuts=3, use_grid_fill=True)
        bm.to_mesh(obj.data)
        bm.free()
        for v in obj.data.vertices:
            height = (v.co - at).dot(normal)  # from the patch's middle layer
            flat = v.co - normal * height
            hit, _ = surface.cast(flat + normal * 0.06, -normal, 0.12)
            if hit is not None:
                v.co = hit + normal * (height + size[1] + 0.001)
    return [obj]


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

def satchel(b, strap_mat, bag_mat, buckle_mat, torso, below, clothes, angle=0.5):
    """A small leather satchel as a saddler makes one, hanging at the right hip from a strap over the left
    shoulder. The bag is a soft box, fuller towards its bottom where its load settles; a flap is sewn along the
    top's back edge, folds over the top and hangs down the front, held by a tongue and a buckle; a tab at each
    end of the top holds a brass ring, and the strap's ends pass through the rings and are sewn back on
    themselves. The bag's back rests on the coat's real surface, leaning as the hip does; the strap runs taut
    from ring to ring over the clothes. torso: the torso alone (probe); below: the skirt the bag rests on;
    clothes: the real garments the strap lies on over the shoulder. The bag has a bone of its own (Satchel), at
    the rings, so it can swing from them and settle against the hip in the game."""
    z = b.pelvis.z - 0.03
    cx, cy = axis_at(b, z)
    theta = math.pi + angle  # from +x round to the right side, then towards the front (-y)
    side = Vector((math.cos(theta), math.sin(theta), 0))
    rest, lean = shapes.Surface(*(below + torso)).cast(Vector((cx, cy, z)) + side * 0.6, -side, 0.6)
    if rest is None:
        rest, lean = Vector((cx, cy, z)) + side * trunk_at(b, z, 0.024)[0], side
    hx, hy, hz = 0.068, 0.023, 0.058  # half width, half depth, half height
    # The bag's own axes: x along the hip towards the back, y out from the body (the coat's normal), z up.
    x_axis = side.cross(Vector((0, 0, 1))).normalized()
    y_axis = (lean - x_axis * lean.dot(x_axis)).normalized()
    z_axis = x_axis.cross(y_axis).normalized()
    turn = Matrix((x_axis, y_axis, z_axis)).transposed()
    centre = rest + y_axis * (hy + 0.002)

    def at(x, y, z_):
        return centre + x_axis * x + y_axis * y + z_axis * z_

    def full(z_):
        """How much fuller the bag's front is at a height: its load settles to the bottom."""
        return 1 + 0.2 * max(-1.0, min(1.0, -z_ / hz))
    bag = slab(f"{b.name}_Bag", centre, (hx, hy, hz), bag_mat, rotation=turn, soft=0.8)
    inverse = turn.transposed()
    for v in bag.data.vertices:
        q = inverse @ (v.co - centre)
        if q.y > 0:
            q.y *= full(q.z)
        q.x *= 1 + 0.04 * max(0.0, -q.z / hz)
        v.co = centre + turn @ q
    out = [bag]
    # The flap: from the top's back edge, over the top and down the front, its lower corners rounded.
    def front(z_):
        return hy * full(z_) + 0.0030
    profile = [(-hy + 0.002, hz + 0.0030), (-hy * 0.45, hz + 0.0036), (hy * 0.35, hz + 0.0036), (front(hz * 0.9) - 0.001, hz * 0.9),
               (front(hz * 0.5), hz * 0.5), (front(0.0), 0.0), (front(-hz * 0.2), -hz * 0.2), (front(-hz * 0.34) + 0.0005, -hz * 0.34)]
    columns = [-1.0, -0.82, -0.45, 0.0, 0.45, 0.82, 1.0]
    grid = []
    for j, (y, z_) in enumerate(profile):
        last = len(profile) - 1 - j  # rows from the flap's free edge
        row = []
        for u in columns:
            # The free edge is rounded: its corners are cut back and lifted.
            corner = max(0.0, abs(u) - 0.6) / 0.4
            row.append(at(u * hx * 1.03, y, z_ + (0.012 * corner ** 2 if last == 0 else 0.004 * corner ** 2 if last == 1 else 0.0)))
        grid.append(row)
    out.append(shapes.sheet(f"{b.name}_BagFlap", grid, bag_mat, thickness=0.004))
    # The tongue from the flap down to the buckle on the bag's front.
    out.append(shapes.sheet(f"{b.name}_BagTongue", [[at(-0.008, front(z_) + 0.0034, z_), at(0.008, front(z_) + 0.0034, z_)]
                                                   for z_ in (-hz * 0.08, -hz * 0.3, -hz * 0.5, -hz * 0.62)], strap_mat, thickness=0.0026, levels=0))
    out.append(shapes.ring(f"{b.name}_Buckle", at(0, front(-hz * 0.47) + 0.0036, -hz * 0.47), x_axis, z_axis, 0.0125, 0.0085, 0.0018, buckle_mat, steps=8))
    # A tab at each end of the top, holding a ring; the strap's ends go through the rings.
    ends = []
    for k, sgn in enumerate((-1, 1)):
        x = sgn * (hx - 0.016)
        out.append(slab(f"{b.name}_BagLoop{k}", at(x, 0.0, hz + 0.006), (0.0085, 0.0042, 0.011), strap_mat, rotation=turn, soft=0.5))
        hole = at(x, 0.0, hz + 0.0235)
        out.append(shapes.ring(f"{b.name}_BagRing{k}", hole, x_axis, z_axis, 0.0155, 0.0095, 0.0021, buckle_mat, steps=12))
        ends.append(hole + z_axis * 0.0075)
    # The strap: from the front ring up across the body, over the left shoulder, down the back to the back ring.
    front_end, back_end = ends
    sh = b.shoulders[1]
    wc, dc = trunk_at(b, b.chest.z, 0.024)
    path = [front_end,
            Vector((b.waist.x - b.width("waist") * 0.4, b.waist.y - b.depth("waist") - 0.02, b.waist.z)),
            Vector((b.chest.x + wc * 0.35, b.chest.y - dc - 0.02, b.chest.z + 0.01)),
            sh + Vector((-0.01, -0.03, b.p["arm"] * 1.3)),
            sh + Vector((-0.015, 0.04, b.p["arm"] * 1.2)),
            Vector((b.chest.x + wc * 0.2, b.chest.y + dc, b.chest.z - 0.02)),
            Vector((b.waist.x - b.width("waist") * 0.7, b.waist.y + b.depth("waist"), b.waist.z - 0.02)),
            back_end]
    # It rests out on the shoulder and lies soft on the coat itself, across the back below the collar, which
    # falls over it where the two meet: the strap as Luis preferred it (October 5). Its ends are on the rings.
    half_w, half_t = 0.014, 0.003
    coat = [g for g in clothes if not g.name.endswith("_Collar")] or list(clothes)
    laid, nms = worn(b, path, torso + below, coat, lift=0.004)
    # Where it crosses the buttons down the front, it rides over them.
    for k, q in enumerate(laid[1:-1], 1):
        ax, ay = axis_at(b, q.z)
        if q.y < ay:
            laid[k] = q + nms[k] * 0.008 * max(0.0, 1 - abs(q.x - ax) / 0.05)
    nms[0], nms[-1] = y_axis, y_axis
    strap = tube(f"{b.name}_Strap", laid, [half_w] * len(laid), [half_t] * len(laid), strap_mat, normals=nms, sides=8)
    strap["wg_ends"] = [*front_end, *back_end]
    strap["wg_end_bone"] = "Satchel"
    out.append(strap)
    # Each end is folded through its ring and sewn back on itself: a doubled length above the ring.
    for k, (end_, near) in enumerate(((laid[0], laid[2]), (laid[-1], laid[-3]))):
        along = (near - end_).normalized()
        width = along.cross(y_axis).normalized()
        face = width.cross(along).normalized()
        if face.dot(y_axis) < 0:
            face = -face
        rows = [[end_ + along * s_ + face * 0.0036 - width * half_w, end_ + along * s_ + face * 0.0036 + width * half_w] for s_ in (-0.003, 0.012, 0.028)]
        out.append(shapes.sheet(f"{b.name}_StrapTab{k}", rows, strap_mat, thickness=0.0032, levels=0))
    # In the game the bag hangs from its rings: a bone there, and how it swings and where the hip stops it.
    pivot = (front_end + back_end) * 0.5
    b.props.append(("Satchel", pivot.copy(), pivot - z_axis * 0.08, "Pelvis"))
    b.swings.append(dict(bone="Satchel", hand=None, length=(pivot - centre).length, radius=0.0, damping=0.8, limit=28,
                         side=-1, ring=tuple(x_axis), stop=tuple(y_axis), rest=tuple(centre)))
    return out


def sling(b, strap_mat, wood, iron, torso, below, clothes, length=0.74, head=1.0):
    """A pickaxe carried on the back, as a miner slings one: a strap worn across the body, over the left shoulder
    and round the right hip, pulled taut over the clothes; on its back two leather loops; the pick's handle
    passes through the loops and lies along the strap, and the pick hangs by its head, which rests on the upper
    loop, flat against the shoulder blade, its points along the back. Nothing here floats: the strap bears on the
    shoulder, the loops are sewn to the strap, the head rests on a loop.
    length: the pick's, foot to head; head: the head's size (1: a full pick)."""
    sh = b.shoulders[1]
    wc, dc = trunk_at(b, b.chest.z, 0.02)
    ww, dw = b.width("waist"), b.depth("waist")
    top = shapes.Surface(*clothes).lay(sh.lerp(b.collar, 0.5) + Vector((0, 0.0, 0.06)), 0.0042)[0]
    hip = Vector((b.waist.x - ww - 0.03, b.waist.y, b.waist.z - 0.03))
    path = [top,
            sh.lerp(b.collar, 0.5) + Vector((0, -0.04, 0.04)),
            Vector((b.chest.x + wc * 0.25, b.chest.y - dc - 0.02, b.chest.z)),
            Vector((b.waist.x - ww * 0.55, b.waist.y - dw - 0.02, b.waist.z + 0.02)),
            hip,
            Vector((b.waist.x - ww * 0.5, b.waist.y + dw + 0.02, b.waist.z + 0.02)),
            Vector((b.chest.x + wc * 0.2, b.chest.y + dc + 0.02, b.chest.z)),
            sh.lerp(b.collar, 0.5) + Vector((0, 0.05, 0.04)),
            top]
    half_w, half_t = 0.0135, 0.0028
    per = 5
    # It bears on the shoulder (its two ends meet there) and turns round the hip: those stay; the rest is drawn taut.
    laid, nms = taut(b, spline(path, 2), torso + below, clothes, lift=0.0042, keep=(8,), free=0.0, per=per, width=half_w)
    out = [tube(f"{b.name}_PickStrap", laid, [half_w] * len(laid), [half_t] * len(laid), strap_mat, normals=nms, sides=8)]
    # The strap's back, from the hip up to the shoulder: the handle lies along it, through two loops.
    back, back_n = laid[8 * per:], nms[8 * per:]
    handle_r = 0.017
    off = half_t + 0.0065 + handle_r  # clear of the back where it swells between the loops

    def on_back(f):
        k = min(len(back) - 2, int(f * (len(back) - 1)))
        return back[k] + back_n[k] * off, back_n[k]
    (low, low_n), (high, high_n) = on_back(0.36), on_back(0.72)
    d = (high - low).normalized()
    out_n = ((low_n + high_n) * 0.5)
    out_n = (out_n - d * out_n.dot(d)).normalized()
    across = d.cross(out_n).normalized()
    top = high + d * 0.047  # the head's collar sits on the upper loop
    foot = top - d * length
    out.extend(pick(b, foot, top, across, out_n, wood, iron, size=head))
    for k, at in enumerate((low, high)):
        loop = shapes.ring(f"{b.name}_PickLoop{k}", at - out_n * 0.0025, out_n, across, handle_r + 0.006, handle_r + 0.0035, 0.0038, strap_mat, steps=12)
        # A leather loop is a band, not a wire: widen it along the handle.
        for v in loop.data.vertices:
            v.co += d * ((v.co - at).dot(d) * 2.2)
        out.append(loop)
    return out


def hip_hang(b, onto, at, towards, leather, brass=None, bar=0.007, radius=0.036, reach=0.17, neck=0.05, arm=0.045, lean=22.0, drum=False):
    """A place on the clothes where a thing hangs by its handle, and how it hangs there. A leather tab is sewn to
    the cloth; on it a brass hook that stands out from the cloth and turns up at its end (brass given), or else
    a loop of leather thong. The thing's handle (a level bar of radius bar, lying along the cloth) rests in the
    hook or the loop, and the thing hangs below it: straight down, unless the cloth below is in the way; then it
    rests on the cloth, as a thing hung at a hip does.
    A coat's skirt bells out below the waist, and a thing hung on that slope would lie on it. The tab is sewn at
    the highest place, from at down, where the thing leans out from plumb by no more than lean degrees.
    at: about where; towards: the way out from the body there. radius: the thing's half-width; reach: how far
    below the handle it reaches; neck: how far below the handle its width begins (a lamp's bail, a mug's handle,
    are thin above that); drum: it is round and lies on its side below its handle (a mug), so its width grows
    from nothing at its top; arm: how far the hook stands out from the cloth.
    Returns the parts (they go with the cloth), where the handle rests, the way the thing hangs, the way out from
    the cloth, and the level direction along the cloth."""
    surface = shapes.Surface(*onto)
    at, towards = Vector(at), Vector(towards).normalized()
    down = Vector((0, 0, -1))
    wire = 0.0024
    half_w, half_h = bar + 0.0045, 0.017

    def hung_at(hit, normal):
        out_ = (normal - Vector((0, 0, normal.z)))
        out_ = out_.normalized() if out_.length > 1e-3 else towards
        seat = hit + out_ * arm + down * (0.021 - wire - bar) if brass else hit + out_ * (half_w + 0.0035) + down * (0.010 + half_h - 0.0022 - bar)
        # Straight down from there, unless the cloth below is in the way: then it leans out and rests on the cloth.
        d = down
        for k in range(1, 9):
            drop = reach * k / 8
            on, on_n = surface.cast(Vector((seat.x, seat.y, seat.z - drop)) + out_ * 0.5, -out_, 1.0)
            if on is None:
                continue
            wide = bar if drop < neck else math.sqrt(max(0.0, radius * radius - (drop - neck - radius) ** 2)) if drum else radius
            need = ((on + on_n * (max(wide, bar) + 0.004)) - seat).normalized()
            if need.dot(out_) > d.dot(out_):
                d = need
        return seat, d, out_

    best = None
    for k in range(20):
        hit, normal = surface.cast(at - Vector((0, 0, 0.01 * k)) + towards * 0.5, -towards, 1.0)
        if hit is None:
            continue
        seat, d, out_ = hung_at(hit, normal)
        leans = math.degrees(math.acos(max(-1.0, min(1.0, -d.z))))
        if best is None or leans < best[0]:
            best = (leans, hit, normal, seat, d, out_)
        if leans <= lean:
            break
    if best is None:
        raise RuntimeError(f"{b.name}: there is no cloth at the hip to sew a hook to.")
    _, hit, normal, seat, d, out_ = best
    across = Vector((0, 0, 1)).cross(out_).normalized()
    # The tab, sewn flat to the cloth: it lies along the cloth's own slope, its back on the cloth.
    level = Vector((0, 0, 1)).cross(normal).normalized()
    rise = normal.cross(level).normalized()
    tab = slab(f"{b.name}_HipTab", hit + normal * 0.0018 + rise * 0.005, (0.011, 0.0022, 0.016), leather,
               rotation=Matrix((level, -normal, level.cross(-normal))).transposed(), soft=0.5)
    tab["wg_anchor"] = [*hit]
    if brass:
        # A hook of brass wire: out from the tab, down, and turned up at its end.
        path = [hit + out_ * 0.003 + Vector((0, 0, 0.012)), hit + out_ * 0.008 + Vector((0, 0, 0.004)), hit + out_ * (arm * 0.5) + down * 0.010,
                hit + out_ * arm + down * 0.021, hit + out_ * (arm + 0.012) + down * 0.014, hit + out_ * (arm + 0.015) + down * 0.002]
        hook = shapes.exact(tube(f"{b.name}_HipHook", spline(path, 4), [wire] * 21, [wire] * 21, brass, sides=7, levels=0))
    else:
        # A loop of thong hanging from the tab; the handle passes through it and rests in its bottom.
        hook = shapes.ring(f"{b.name}_HipLoop", hit + out_ * (half_w + 0.0035) + down * 0.010, out_, Vector((0, 0, 1)), half_w, half_h, 0.0022, leather, steps=14)
    hook["wg_anchor"] = [*hit]
    return [tab, hook], seat, d, out_, across


def hang_by(parts, seat, d):
    """Turns parts built hanging straight down from seat so that they hang along d."""
    turn = Vector((0, 0, -1)).rotation_difference(Vector(d).normalized()).to_matrix().to_4x4()
    move = Matrix.Translation(seat) @ turn @ Matrix.Translation(-Vector(seat))
    for o in parts:
        o.data.transform(move)
        o.data.update()
    return parts


def lantern(b, grasp, ring, brass, glass, wood, grip=0.007, span=0.09, name="Lantern"):
    """A miner's lamp carried by its handle: a wooden grip (the bar the hand closes round, along ring) on a wire
    bail hinged at two ears on the cap, and below it, hanging plumb, a vented brass cap, warm glass behind four
    guard bars, and a brass base. grasp: the middle of the grip, inside the fingers."""
    ring = Vector(ring).normalized()
    ring = (ring - Vector((0, 0, ring.z))).normalized()  # the grip is level; the lamp hangs straight down from it
    down = Vector((0, 0, -1))
    turn = frame(ring, down) @ Matrix.Rotation(math.pi / 2, 3, 'Y')
    out = [cylinder(f"{b.name}_{name}Handle", grasp, grip, span, wood, rotation=turn, vertices=14, bevel=0.25)]
    apex = grasp + down * 0.052  # the cap's top, under the bail's arch
    cap_r = 0.036
    # The cap: a little vent on top, then the hood.
    out.append(shapes.exact(lathe(f"{b.name}_{name}Cap", [(0.0, 0.012), (0.011, 0.012), (0.012, 0.0), (0.016, -0.004), (cap_r, -0.028), (0.034, -0.034), (0.0, -0.034)],
                                  brass, at=apex, segments=16)))
    body = apex + down * 0.034
    out.append(shapes.exact(lathe(f"{b.name}_{name}Glass", [(0.0, 0.0), (0.026, 0.0), (0.029, 0.035), (0.026, 0.07), (0.0, 0.07)], glass, at=body + down * 0.07, segments=16)))
    for k in range(4):
        a = k / 4 * math.tau + 0.4
        x, y = math.cos(a) * 0.031, math.sin(a) * 0.031
        out.append(shapes.exact(tube(f"{b.name}_{name}Bar{k}", [body + Vector((x, y, 0.002)), body + Vector((x * 1.08, y * 1.08, -0.035)), body + Vector((x, y, -0.072))],
                                     [0.0025] * 3, [0.0025] * 3, brass, sides=6, levels=0)))
    out.append(shapes.exact(lathe(f"{b.name}_{name}Base", [(0.0, 0.0), (0.034, 0.0), (0.036, 0.012), (0.03, 0.018), (0.0, 0.018)], brass, at=body + down * 0.088, segments=16)))
    # The bail: one wire through the grip, down each side to an ear on the cap's shoulder.
    wire = 0.0019
    for k, sgn in enumerate((-1, 1)):
        ear = apex + ring * (sgn * (cap_r - 0.004)) + down * 0.024
        end = grasp + ring * (sgn * span * 0.5)
        path = [grasp + ring * (sgn * span * 0.3), end + ring * (sgn * 0.004), end + ring * (sgn * 0.007) + down * 0.012,
                ear + ring * (sgn * 0.006) - down * 0.012, ear]
        out.append(shapes.exact(tube(f"{b.name}_{name}Bail{k}", spline(path, 3), [wire] * 13, [wire] * 13, brass, sides=6, levels=0)))
        out.append(cylinder(f"{b.name}_{name}Ear{k}", ear, 0.0045, 0.004, brass, rotation=turn, vertices=8, bevel=0))
    return out


def pickaxe(b, grasp, along, wood, iron, length=0.86, hold=0.22):
    """A pickaxe whose handle runs through the hand along a direction (towards its head).
    hold: where along the handle the hand is, from its foot (0) to its head (1)."""
    d = Vector(along).normalized()
    low, high = grasp - d * length * hold, grasp + d * length * (1 - hold)
    across = d.cross(Vector((1, 0, 0)))
    if across.length < 0.1:
        across = d.cross(Vector((0, 1, 0)))
    return pick(b, low, high, across.normalized(), d.cross(across).normalized(), wood, iron, bow=0.012)


def pick(b, low, high, across, flat, wood, iron, bow=0.0, size=1.0, thick=1.0, round_=False, light=False):
    """A pickaxe from the foot of its handle (low) to its head (high): the head's points lie along across, and it
    is flat towards flat (the side that lies against a back). bow: how much the handle is sprung; size: the
    head's reach (1: a full pick). thick: how stout it is (1: a handle 33 mm across at its middle; the head and
    its collar with it). round_: a round handle, for a tool the hands close on (tools.py); as carried on a back
    it is a little flattened, as a pick's handle is. light: built with a quarter of the faces, for a tool that
    many hands carry at once."""
    d = (high - low).normalized()
    across = Vector(across).normalized()
    bend = across.cross(d).normalized() * bow
    handle = [low, low.lerp(high, 0.35) + bend, low.lerp(high, 0.7) + bend, high]
    wide = [w * thick for w in (0.0145, 0.015, 0.0155, 0.016, 0.0165, 0.017, 0.0175, 0.018, 0.0185, 0.019, 0.019, 0.0185, 0.018)]
    flat_ = [w * thick for w in (0.0125, 0.013, 0.0135, 0.014, 0.0145, 0.015, 0.0155, 0.016, 0.0165, 0.017, 0.017, 0.0165, 0.016)]
    levels = 0 if light else 1
    out = [tube(f"{b.name}_PickHandle", spline(handle, 4), wide, wide if round_ else flat_, wood, sides=14 if round_ else 10, levels=levels)]
    curve = -d * 0.04 * size
    head = [high + across * 0.25 * size + curve, high + across * 0.12 * size + curve * 0.3, high,
            high - across * 0.1 * size + curve * 0.25, high - across * 0.2 * size + curve * 0.8]
    out.append(tube(f"{b.name}_PickHead", spline(head, 3), [w * thick for w in (0.0, 0.008, 0.012, 0.016, 0.02, 0.024, 0.026, 0.025, 0.022, 0.019, 0.015, 0.01, 0.005)],
                    [w * thick for w in (0.0, 0.007, 0.01, 0.013, 0.016, 0.019, 0.021, 0.021, 0.019, 0.017, 0.015, 0.012, 0.009)], iron, normals=[d] * 13, sides=8,
                    levels=levels))
    out.append(cylinder(f"{b.name}_PickCollar", high - d * 0.01 * thick, 0.024 * thick, 0.05 * thick, iron, rotation=frame(d, across) @ Matrix.Rotation(math.pi / 2, 3, 'Y'),
                        vertices=14 if light else 24))
    return out


def mug(b, grasp, ring, mat, grip=0.0055, span=0.09, clear=0.03, name="Mug"):
    """A big tin mug carried by its handle, the arm hanging: the handle's grip (the bar the hand closes round,
    along ring) is in the fingers, and the mug hangs below it on its side, as an empty mug does.
    grasp: the middle of the grip, inside the fingers; clear: from the grip's middle to the mug's wall, room
    for the fingers that pass between them."""
    ring = Vector(ring).normalized()
    ring = (ring - Vector((0, 0, ring.z))).normalized()
    down = Vector((0, 0, -1))
    radius, height = 0.042, span + 0.022
    axis = grasp + down * (radius + clear)  # the mug's own axis, level, under the grip
    turn = frame(ring, down)
    # The cup, turned on its side: its axis along the grip, its mouth to the front.
    handle_turn = turn @ Matrix.Rotation(math.pi / 2, 3, 'Y')
    out = [lathe(f"{b.name}_{name}", [(0.0, 0.0), (radius - 0.005, 0.0), (radius - 0.001, 0.004), (radius, height - 0.006), (radius + 0.003, height),
                                       (radius - 0.004, height), (radius - 0.006, 0.012), (0.0, 0.012)], mat,
                 at=axis - ring * (height * 0.5), rotation=handle_turn, segments=18)]
    shapes.exact(out[0])
    # The handle: a strap of tin from the wall out to the grip and back.
    out.append(cylinder(f"{b.name}_{name}Handle", grasp, grip, span, mat, rotation=handle_turn, vertices=12, bevel=0.25))
    for k, sgn in enumerate((-1, 1)):
        end = grasp + ring * (sgn * span * 0.5)
        wall = axis + ring * (sgn * span * 0.5) - down * (radius - 0.002)
        path = [end - ring * (sgn * 0.006), end + ring * (sgn * 0.003) + down * 0.004, wall.lerp(end, 0.4) + ring * (sgn * 0.004), wall]
        out.append(tube(f"{b.name}_{name}Arm{k}", spline(path, 3), [grip] * 10, [grip * 0.7] * 10, mat, normals=[ring] * 10, sides=8, levels=0))
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


def hammer(b, onto, at, wood, iron, leather):
    """A small hammer carried as a smith carries one: a leather loop sewn to the apron at the hip; the handle
    hangs down through the loop along the apron, and the head rests on the loop. at: where the loop is (cast
    from in front onto the garments in onto). Returns its parts, and how it hangs: from where, the middle of its
    hanging weight, the apron's outward normal there, and the direction across it."""
    surface = shapes.Surface(*onto)
    at = Vector(at)
    hit, normal = surface.cast(Vector((at.x, at.y - 0.5, at.z)), Vector((0, 1, 0)), 1.0)
    low, low_n = surface.cast(Vector((at.x, at.y - 0.5, at.z - 0.15)), Vector((0, 1, 0)), 1.0)
    if hit is None:
        hit, normal = at, Vector((0, -1, 0))
    if low is None:
        low, low_n = hit - Vector((0, 0, 0.15)), normal
    radius = 0.0105
    # The loop holds the handle a little off the cloth: the cloth at the waist moves with the spine as well as
    # the hips, and the handle must not sink into it as the body sways.
    off = 0.010
    loop_at = hit + normal * (radius + off)
    # It hangs straight down from the loop, unless the apron below is in the way: then it rests on the apron.
    d = Vector((0, 0, -1))
    for drop in (0.05, 0.1, 0.15, 0.2):
        on, on_n = surface.cast(Vector((at.x, at.y - 0.5, at.z - drop)), Vector((0, 1, 0)), 1.0)
        if on is None:
            continue
        need = ((on + on_n * (radius + 0.002)) - loop_at).normalized()
        if need.dot(normal) > d.dot(normal):
            d = need
    across = d.cross(normal).normalized()
    top = loop_at - d * 0.022  # the head's underside rests on the loop
    out = [tube(f"{b.name}_HammerHandle", [top, loop_at, loop_at + d * 0.1, loop_at + d * 0.21], [0.0105, 0.0105, 0.0098, 0.0092],
                [0.0105, 0.0105, 0.0098, 0.0092], wood, sides=8, levels=0)]
    # The head, across the handle's top; its underside rests on the loop.
    out.append(slab(f"{b.name}_HammerHead", top - d * 0.004, (0.045, 0.014, 0.016), iron, rotation=frame(across, -d), soft=0.4))
    # The loop: sewn to the cloth on one side, round the handle on the other.
    reach = radius + (off + 0.003) * 0.5
    loop = shapes.ring(f"{b.name}_HammerLoop", hit + normal * reach - d * 0.004, normal, across, reach, radius + 0.003, 0.0035, leather, steps=12)
    for v in loop.data.vertices:
        v.co += d * ((v.co - loop_at).dot(d) * 2.4)
    loop["wg_anchor"] = [*hit]
    out.append(loop)
    # Where it hangs from (the loop), where the body stops it (near the handle's end, which is what the cloth
    # reaches first as the leg under it moves), and the apron's face there.
    return out, loop_at, loop_at + d * 0.2, normal, across
