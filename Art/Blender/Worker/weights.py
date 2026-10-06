"""Wonder Gather — what everything weighs (S3, step 1).

A body that answers to forces needs each of its parts' weight, where that weight is, and how hard the part is to
turn. They are measured here on the model itself, as a tool's grips and a body's reach already are:

- the body under its clothes, as the solids its own measures describe (the trunk's width and depth at each level,
  the limbs' thicknesses, the head), at the density of a living body;
- what it wears, piece by piece: cloth, leather and sheet metal by their area, solid wood and iron by their
  volume. Each piece is given to the part of the body that carries it (the bone its skin follows);
- a tool, by the volume of its parts and what they are made of.

A body's parts are given per bone: a mass, where its centre is along the bone (0 at the bone's head, 1 at its
tail), and three moments about that centre: across the body (about the side-to-side axis), about the bone's own
line, and about the front-to-back axis. A tool is given whole: its mass, its centre, its principal moments and how
they are turned, in the tool's own space.

    weights.body(b, meshes, bones)      one being's parts (meshes: its parts, skinned, before they are joined)
    weights.tool(parts)                 a tool
"""
import math

import bmesh
from mathutils import Matrix, Vector

# A living body, kg/m3. (People are a little under water's density with full lungs, a little over with empty.)
FLESH = 1000.0
# Solid things, kg/m3.
SOLID = {"Iron": 7850.0, "Wood": 700.0}
# Things made of sheet, kg/m2: woollen cloth with its lining; hair; leather 2 mm thick; brass or tin 0.8 mm thick;
# glass 2 mm thick.
SHEET = {"cloth": 0.55, "hair": 0.3, "leather": 1.8, "metal": 6.8, "glass": 5.0}
KINDS = {"Hair": "hair", "HairBrown": "hair", "CoatBlue": "cloth", "Coat": "cloth", "Smock": "cloth", "Shirt": "cloth", "Trousers": "cloth",
         "TrousersGrey": "cloth", "Patch": "cloth", "Accent": "cloth", "Leather": "leather", "ApronLeather": "leather", "Lace": "leather",
         "Brass": "metal", "Iron": "metal", "Mug": "metal", "Button": "metal", "Wood": "leather", "Glass": "glass"}
# (Thin wood that is not a tool's handle, a toggle or a peg, weighs about what leather does by its area.)
# A pick's collar is modelled as a solid drum, and is a ring round the handle: the share of the drum that is iron
# (outfits.pick: the drum's radius is 24, the handle's inside it about 18).
RING = {"PickCollar": 1 - (18 / 24) ** 2}
# The boots are weighed with the feet (a foot's solid is the boot's size), and the skin is the body itself.
WITH_THE_BODY = ("Boots", "Sole")

# The parts of a body, each on its bone.
PARTS = ("Pelvis", "Spine", "Chest", "Neck", "Head", "UpperArm.L", "Forearm.L", "Hand.L", "UpperArm.R", "Forearm.R", "Hand.R",
         "Thigh.L", "Shin.L", "Foot.L", "Toe.L", "Thigh.R", "Shin.R", "Foot.R", "Toe.R")


class Solid:
    """A mass, its centre, and its inertia about that centre (a 3x3 matrix, in the space it was measured in)."""

    def __init__(self, mass=0.0, centre=None, inertia=None):
        self.mass = mass
        self.centre = Vector(centre) if centre is not None else Vector((0, 0, 0))
        self.inertia = inertia.copy() if inertia is not None else Matrix(((0, 0, 0), (0, 0, 0), (0, 0, 0)))

    def about(self, point):
        """Its inertia about another point (the parallel axes)."""
        d = self.centre - Vector(point)
        shift = Matrix.Identity(3) * d.dot(d) - outer(d, d)
        return self.inertia + shift * self.mass

    def __add__(self, other):
        mass = self.mass + other.mass
        if mass <= 0:
            return Solid()
        centre = (self.centre * self.mass + other.centre * other.mass) / mass
        return Solid(mass, centre, self.about(centre) + other.about(centre))


def outer(a, b):
    return Matrix([[a[i] * b[j] for j in range(3)] for i in range(3)])


def frame(axis, wide=Vector((1, 0, 0))):
    """Three axes for a part: across the body, along the part, and front to back."""
    y = Vector(axis).normalized()
    x = (wide - y * wide.dot(y))
    if x.length < 1e-4:
        x = Vector((0, 1, 0)) - y * y.y
    x.normalize()
    return x, y, x.cross(y).normalized()


def sliced(a, b, half_wide, half_deep, density=FLESH, wide=Vector((1, 0, 0)), box=False, slices=48):
    """A solid from a to b whose every slice is an ellipse (or, box, a rectangle) of the half-width and half-depth
    given at t (0 to 1). Its width lies across the body."""
    a, b = Vector(a), Vector(b)
    x, y, z = frame(b - a, wide)
    length = (b - a).length
    step = length / slices
    total = Solid()
    for k in range(slices):
        t = (k + 0.5) / slices
        w, d = half_wide(t), half_deep(t)
        if w <= 0 or d <= 0:
            continue
        if box:
            area, ix, iz = 4 * w * d, d * d / 3, w * w / 3  # per unit mass: about the wide axis, about the deep axis
        else:
            area, ix, iz = math.pi * w * d, d * d / 4, w * w / 4
        m = density * area * step
        # A thin slice about its own centre, in the part's axes: about x (across) it turns its depth, about z (deep)
        # its width, about y (along) both.
        local = outer(x, x) * (m * ix) + outer(z, z) * (m * iz) + outer(y, y) * (m * (ix + iz))
        total = total + Solid(m, a + y * (t * length), local)
    return total


def ellipsoid(centre, rx, ry, rz, axes=None, density=FLESH):
    m = density * 4 / 3 * math.pi * rx * ry * rz
    x, y, z = axes or (Vector((1, 0, 0)), Vector((0, 1, 0)), Vector((0, 0, 1)))
    inertia = (outer(x, x) * (ry * ry + rz * rz) + outer(y, y) * (rx * rx + rz * rz) + outer(z, z) * (rx * rx + ry * ry)) * (m / 5)
    return Solid(m, centre, inertia)


def lerp(a, b):
    return lambda t: a + (b - a) * t


def through(*points):
    """A value that passes through (t, value) points, straight between them."""
    def f(t):
        for (t0, v0), (t1, v1) in zip(points, points[1:]):
            if t <= t1:
                return v0 + (v1 - v0) * max(0.0, min(1.0, (t - t0) / (t1 - t0)))
        return points[-1][1]
    return f


def flesh(b, bones):
    """The body under its clothes: one solid for each part, from the body's own measures."""
    p, H = b.p, b.H
    trunk = b.trunk
    arm, leg = p["arm"], p["leg"]
    parts = {}
    hips, waist, chest, collar = bones["Pelvis"][0], bones["Pelvis"][1], bones["Spine"][1], bones["Chest"][1]
    # The pelvis reaches below the hip joints (the seat), then rises to the waist.
    seat = hips - (waist - hips).normalized() * trunk["pelvis"][1] * 0.9
    parts["Pelvis"] = sliced(seat, hips, lerp(trunk["pelvis"][0] * 0.72, trunk["pelvis"][0]), lerp(trunk["pelvis"][1] * 0.7, trunk["pelvis"][1])) \
        + sliced(hips, waist, lerp(trunk["pelvis"][0], trunk["waist"][0]), lerp(trunk["pelvis"][1], trunk["waist"][1]))
    parts["Spine"] = sliced(waist, chest, lerp(trunk["waist"][0], trunk["chest"][0]), lerp(trunk["waist"][1], trunk["chest"][1]))
    # The chest widens to the shoulders, then closes to the collar.
    shoulders = abs(b.shoulders[1].x - b.shoulders[0].x) * 0.5
    parts["Chest"] = sliced(chest, collar, through((0, trunk["chest"][0]), (0.62, max(trunk["chest"][0], shoulders * 0.96)), (1, trunk["collar"][0])),
                            through((0, trunk["chest"][1]), (0.62, trunk["chest"][1] * 0.95), (1, trunk["collar"][1])))
    neck_r = b.hr * p.get("neck_r", 0.5)
    parts["Neck"] = sliced(bones["Neck"][0], bones["Neck"][1], lerp(neck_r, neck_r), lerp(neck_r, neck_r))
    hx, hy, hz = (b.hm @ Vector((1, 0, 0))), (b.hm @ Vector((0, 1, 0))), (b.hm @ Vector((0, 0, 1)))
    parts["Head"] = ellipsoid(b.head, b.hr * b.hs.x, b.hr * b.hs.y, b.hr * b.hs.z, (hx, hy, hz))
    hand = p.get("hand", 0.11) * H
    foot = p.get("foot", 0.165) * H
    for i, s in enumerate("LR"):
        sh, el, wr = bones[f"UpperArm.{s}"][0], bones[f"UpperArm.{s}"][1], bones[f"Forearm.{s}"][1]
        parts[f"UpperArm.{s}"] = sliced(sh, el, lerp(arm * 1.25, arm * 1.05), lerp(arm * 1.25, arm * 1.05))
        parts[f"Forearm.{s}"] = sliced(el, wr, lerp(arm * 1.05, arm * 0.8), lerp(arm * 1.05, arm * 0.8))
        along = (bones[f"Hand.{s}"][1] - wr).normalized()
        x, y, z = frame(along)
        parts[f"Hand.{s}"] = ellipsoid(wr + along * hand * 0.5, hand * 0.235, hand * 0.5, hand * 0.115, (x, y, z))
        hp, kn, an = bones[f"Thigh.{s}"][0], bones[f"Thigh.{s}"][1], bones[f"Shin.{s}"][1]
        parts[f"Thigh.{s}"] = sliced(hp, kn, lerp(leg * 1.45, leg * 1.1), lerp(leg * 1.45, leg * 1.1))
        parts[f"Shin.{s}"] = sliced(kn, an, lerp(leg * 1.1, leg * 0.8), lerp(leg * 1.1, leg * 0.8))
        # The foot in its boot: from the heel to the ball, as wide as the boot, as high as the ankle; then the toes.
        ball, tip = bones[f"Foot.{s}"][1], bones[f"Toe.{s}"][1]
        forward = (tip - ball).normalized()
        heel = Vector((an.x, an.y, ball.z)) - forward * foot * 0.3
        side = forward.cross(Vector((0, 0, 1))).normalized()
        high = an.z
        parts[f"Foot.{s}"] = sliced(heel + Vector((0, 0, high * 0.5 - ball.z)), ball + Vector((0, 0, high * 0.42 - ball.z)),
                                    lerp(foot * 0.17, foot * 0.19), lerp(high * 0.5, high * 0.42), wide=side, box=True)
        parts[f"Toe.{s}"] = sliced(ball + Vector((0, 0, high * 0.32 - ball.z)), tip + Vector((0, 0, high * 0.25 - ball.z)),
                                   lerp(foot * 0.19, foot * 0.15), lerp(high * 0.32, high * 0.22), wide=side, box=True)
    return parts


def volume(obj):
    """A closed mesh as a solid of density 1: its volume as mass, its centre and its inertia. None if it is open."""
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    bmesh.ops.triangulate(bm, faces=bm.faces)
    if any(len(e.link_faces) != 2 for e in bm.edges):
        bm.free()
        return None
    world = obj.matrix_world
    total, first = 0.0, Vector((0, 0, 0))
    second = Matrix(((0, 0, 0), (0, 0, 0), (0, 0, 0)))  # the integral of r r^T
    for f in bm.faces:
        a, b_, c = (world @ v.co for v in f.verts)
        v = a.dot(b_.cross(c)) / 6
        total += v
        first += (a + b_ + c) * (v / 4)
        # A tetrahedron from the origin: the integral of r r^T is v/20 (sum of p p^T + (sum p)(sum p)^T).
        s = a + b_ + c
        second += (outer(a, a) + outer(b_, b_) + outer(c, c) + outer(s, s)) * (v / 20)
    bm.free()
    if abs(total) < 1e-12:
        return None
    if total < 0:
        total, first, second = -total, -first, second * -1
    centre = first / total
    about_origin = Matrix.Identity(3) * (second[0][0] + second[1][1] + second[2][2]) - second
    d = centre
    inertia = about_origin - (Matrix.Identity(3) * d.dot(d) - outer(d, d)) * total
    return Solid(total, centre, inertia)


def carrier(name, bones, names):
    """The part of the body that carries what follows a bone: the bone itself, or the nearest part it hangs from."""
    seen = 0
    while name not in names and name in bones and bones[name][3] and seen < 12:
        name, seen = bones[name][3], seen + 1
    return name if name in names else "Pelvis"


def worn(b, meshes, bones):
    """What the body wears and carries: for each part of the body, the solid of what it bears; and each piece's
    own weight, by name."""
    on = {n: Solid() for n in PARTS}
    pieces = {}
    for o in meshes:
        if o.type != 'MESH':
            continue
        label = o.name[len(b.name) + 1:] if o.name.startswith(b.name + "_") else o.name
        mats = [m.name if m else "" for m in o.data.materials] or [""]
        groups = {g.index: g.name for g in o.vertex_groups}
        world = o.matrix_world
        co = [world @ v.co for v in o.data.vertices]
        # Which part bears each vertex: the bone it follows most.
        bearer = []
        for v in o.data.vertices:
            best = max(v.groups, key=lambda g: g.weight, default=None)
            bearer.append(carrier(groups[best.group], bones, PARTS) if best is not None and best.group in groups else "Pelvis")
        # A pick's or a hammer's wood and iron are solid; everything else of metal or wood here is sheet or thin.
        solid_of = volume(o) if all(m in SOLID for m in mats) and ("Pick" in label or "Hammer" in label) else None
        if solid_of is not None and mats[0] in SOLID:
            density = SOLID[mats[0]] * RING.get(label, 1.0)
            piece = Solid(solid_of.mass * density, solid_of.centre, solid_of.inertia * density)
            votes = {}
            for n in bearer:
                votes[n] = votes.get(n, 0) + 1
            on_part = max(votes, key=votes.get)
            on[on_part] = on[on_part] + piece
            pieces[label] = pieces.get(label, 0.0) + piece.mass
            continue
        for poly in o.data.polygons:
            mat = mats[min(poly.material_index, len(mats) - 1)]
            if mat.startswith("Skin") or mat in WITH_THE_BODY:
                continue
            kind = KINDS.get(mat, "cloth")
            m = poly.area * SHEET[kind]
            centre = sum((co[i] for i in poly.vertices), Vector((0, 0, 0))) / len(poly.vertices)
            votes = {}
            for i in poly.vertices:
                votes[bearer[i]] = votes.get(bearer[i], 0) + 1
            part = max(votes, key=votes.get)
            on[part] = on[part] + Solid(m, centre, Matrix(((0, 0, 0), (0, 0, 0), (0, 0, 0))))
            pieces[label] = pieces.get(label, 0.0) + m
    return on, pieces


def body(b, meshes, bones):
    """One being weighed: each part (the body's own solid and what it bears), and the whole."""
    own = flesh(b, bones)
    on, pieces = worn(b, meshes, bones)
    parts, whole = [], Solid()
    for name in PARTS:
        both = own[name] + on[name]
        head, tail = bones[name][0], bones[name][1]
        x, y, z = frame(tail - head)
        along = (both.centre - head).dot(y) / max((tail - head).length, 1e-6)
        moments = [x.dot(both.inertia @ x), y.dot(both.inertia @ y), z.dot(both.inertia @ z)]
        parts.append(dict(bone=name, mass=round(both.mass, 4), body=round(own[name].mass, 4), along=round(along, 4),
                          off=game((both.centre - head) - y * (both.centre - head).dot(y)), inertia=[round(v, 6) for v in moments]))
        whole = whole + both
    return dict(mass=round(whole.mass, 3), body=round(sum(s.mass for s in own.values()), 3), worn=round(sum(s.mass for s in on.values()), 3),
                centre=game(whole.centre), parts=parts), pieces


def game(v):
    """Blender's space to the game's: x across, y up, z ahead."""
    return [round(-v[0], 5), round(v[2], 5), round(-v[1], 5)]


def tool(parts, densities=SOLID):
    """A tool weighed: its mass, its centre, its principal moments (kg m2) and how they are turned (a quaternion
    x, y, z, w), in the tool's space as the game has it (x across, y up the handle, z the way it strikes)."""
    import numpy
    whole, pieces = Solid(), {}
    for o in parts:
        if o.type != 'MESH':
            continue
        mat = o.data.materials[0].name if o.data.materials and o.data.materials[0] else ""
        v = volume(o)
        if v is None:
            raise RuntimeError(f"{o.name} is not a closed shape: it cannot be weighed.")
        density = densities.get(mat)
        if density is None:
            raise RuntimeError(f"{o.name} is made of {mat or 'nothing'}: its density is not known.")
        label = o.name.split("_")[-1]
        density *= RING.get(label, 1.0)
        piece = Solid(v.mass * density, v.centre, v.inertia * density)
        whole = whole + piece
        pieces[label] = round(piece.mass, 4)
    # To the game's axes: (x, y, z) -> (-x, z, -y).
    turn = Matrix(((-1, 0, 0), (0, 0, 1), (0, -1, 0)))
    inertia = turn @ whole.inertia @ turn.transposed()
    values, vectors = numpy.linalg.eigh(numpy.array([[inertia[i][j] for j in range(3)] for i in range(3)]))
    axes = Matrix([[float(vectors[i][j]) for j in range(3)] for i in range(3)])
    if axes.determinant() < 0:
        axes = Matrix([[axes[i][0], axes[i][1], -axes[i][2]] for i in range(3)])
    q = axes.to_quaternion()
    return dict(mass=round(whole.mass, 4), centre=game(whole.centre), inertia=[round(float(v), 7) for v in values],
                inertiaTurn=[round(q.x, 6), round(q.y, 6), round(q.z, 6), round(q.w, 6)], pieces=pieces)


def report(b, weighed, pieces, out_dir):
    """A being's weights as a report: the whole, each part, and each piece it wears."""
    import os
    H = b.H
    lines = [f"WEIGHT {b.name}: {weighed['mass']:.1f} kg ({weighed['body']:.1f} kg of body, {weighed['worn']:.1f} kg worn and carried); "
             f"{H:.2f} m tall; its centre {weighed['centre'][1]:.3f} m up, {weighed['centre'][2] * 1000:+.0f} mm ahead of the model's origin",
             f"  {'part':<12}{'kg':>8}{'of body':>9}{'centre along':>14}{'moments (kg m2): across, about, deep':>40}"]
    for part in weighed["parts"]:
        i = part["inertia"]
        lines.append(f"  {part['bone']:<12}{part['mass']:>8.2f}{part['body']:>9.2f}{part['along']:>14.2f}      {i[0]:.5f}  {i[1]:.5f}  {i[2]:.5f}")
    lines.append("  worn and carried:")
    for label, m in sorted(pieces.items(), key=lambda kv: -kv[1]):
        if m >= 0.005:
            lines.append(f"    {label:<24}{m * 1000:>7.0f} g")
    print("\n".join(lines))
    short = b.name.split("_")[-1]
    with open(os.path.join(out_dir, f"weights_{short}.txt"), "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")
    return lines
