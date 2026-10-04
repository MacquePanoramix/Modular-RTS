"""Wonder Gather — hair (S1d).

Hair is drawn as locks: flat, tapered clumps combed over the skull from a
parting or a crown, each ending in a point, like the confident dark shapes of
painted hair. A cap under them closes the gaps above the hairline.

Locks are combed in skull space, where the skull is a unit sphere: a lock
starts at a root on it and steps along a direction. Gravity pulls it down, the
skull pushes it out, a bend curls it towards or away from the head, and a
twist swings it sideways. A hairstyle is a function that returns its locks; a
character creator can offer each style and its parameters as choices.
"""
import math
import random

import bmesh
import bpy
from mathutils import Matrix, Vector

import shapes


class Skull:
    """Maps skull space (a unit sphere) onto a body's head."""

    def __init__(self, b):
        self.b = b

    def world(self, u):
        c = Vector((0, 0.05, 0.1)) + Vector((u.x, u.y, u.z * 1.05))
        return self.b.on_head(c.x, c.y, c.z)

    def direction(self, u, d):
        """A direction in skull space, in the world."""
        return (self.b.hm @ Vector((d.x * self.b.hs.x, d.y * self.b.hs.y, d.z * self.b.hs.z * 1.05))).normalized()

    def scale(self):
        return self.b.hr * (self.b.hs.x + self.b.hs.y) * 0.5


def comb(root, direction, length, steps=12, lift=(0.03, 0.1), gravity=0.0, bend=0.0, twist=0.0, tip_bend=0.0):
    """A lock's path in skull space."""
    p = root.normalized() * (1 + lift[0])
    radial = p.normalized()
    d = (direction - radial * direction.dot(radial)).normalized()
    pts = [p.copy()]
    for k in range(steps):
        t = (k + 1) / steps
        radial = p.normalized()
        side = d.cross(radial)
        if side.length > 1e-5:
            # bend > 0 curls the lock back towards the scalp, < 0 lifts it away.
            turn = bend / steps + (tip_bend / steps * 4 if t > 0.75 else 0)
            d = Matrix.Rotation(turn, 3, side.normalized()) @ d
        if twist:
            d = Matrix.Rotation(twist / steps, 3, radial) @ d
        d = (d + Vector((0, 0, -gravity * t))).normalized()
        p = p + d * (length / steps)
        floor = 1 + lift[0] + (lift[1] - lift[0]) * t
        if p.length < floor:
            p = p.normalized() * floor
        pts.append(p.copy())
    return pts


def lock_mesh(skull, name, path, width, mat, flat=0.42, root=0.75):
    """A flat, tapered lock along a path; it closes to a point at the tip."""
    n = len(path)
    pts = [skull.world(p) for p in path]
    normals = [skull.direction(p, p.normalized()) for p in path]
    s = skull.scale()
    widths, thicks = [], []
    for k in range(n):
        t = k / (n - 1)
        w = width * s * (root + (1 - root) * min(1.0, t * 5)) * (1 - t ** 1.7)
        widths.append(w)
        thicks.append(w * flat)
    return shapes.tube(name, pts, widths, thicks, mat, normals=normals, sides=8, levels=1)


def hairline(az, front=0.35, side=-0.1, back=-0.6):
    """The hairline's height on the skull at an azimuth (0 at the front)."""
    b = (front - back) / 2
    a = (side + (front + back) / 2) / 2
    c = a - side
    return a + b * math.cos(az) + c * math.cos(2 * az)


def cap(skull, name, mat, front=0.35, side=-0.1, back=-0.6, lift=1.1):
    """A shell over the skull above the hairline, so no scalp shows between locks."""
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=40, v_segments=24, radius=lift)
    for v in list(bm.verts):
        az = math.atan2(v.co.x, -v.co.y)
        if v.co.z / lift < hairline(az, front, side, back) - 0.08:
            bm.verts.remove(v)
    for v in bm.verts:
        v.co = skull.world(Vector(v.co))
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = shapes.link(bpy.data.objects.new(name, mesh))
    shell = obj.modifiers.new("Thickness", 'SOLIDIFY')
    shell.thickness, shell.offset = 0.004, 1.0  # thickened outwards, clear of the skull's blended form
    sub = obj.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = sub.render_levels = 1
    shapes.apply_all(obj)
    return shapes.finish(obj, mat)


def on_sphere(az, z):
    r = math.sqrt(max(0.0, 1 - z * z))
    return Vector((math.sin(az) * r, -math.cos(az) * r, z))


def bob(skull, name, mat, seed, parting=-0.15):
    """A bob: locks fall from a parting to the jaw, tips tucked in, and a fringe cut just above the brows."""
    rng = random.Random(seed)
    objs = [cap(skull, f"{name}_Cap", mat, front=0.45, side=-0.05, back=-0.6, lift=1.08)]
    locks = []
    for k in range(40):
        az = (k + rng.uniform(-0.3, 0.3)) / 40 * math.tau
        if abs(math.remainder(az, math.tau)) < 0.75:
            continue  # the fringe covers the front
        root = on_sphere(az, rng.uniform(0.78, 0.93))
        root.x += parting * (1 - abs(root.x))
        out = Vector((math.sin(az), -math.cos(az), 0))
        path = comb(root, out * 0.6 + Vector((0, 0, -1)), rng.uniform(1.85, 2.05), steps=16, lift=(0.03, 0.16),
                    gravity=0.9, tip_bend=-0.5, twist=rng.uniform(-0.1, 0.1))
        locks.append((path, rng.uniform(0.3, 0.38)))
    for k in range(11):
        x = -0.75 + 1.5 * k / 10 + rng.uniform(-0.04, 0.04)
        root = Vector((x * 0.7, -0.45, 0.88)).normalized()
        path = comb(root, Vector((x * 0.25, -1, -0.35)), rng.uniform(0.8, 0.95) - abs(x) * 0.12, steps=12, lift=(0.04, 0.08),
                    gravity=0.25, bend=0.25)
        locks.append((path, rng.uniform(0.2, 0.26)))
    for k, (path, width) in enumerate(locks):
        objs.append(lock_mesh(skull, f"{name}_Lock{k}", path, width, mat))
    return objs


def swept(skull, name, mat, seed, sweep=(-0.6, -0.8, 0.0)):
    """Swept to one side and forward, tips lifting a little, with strands standing up at the crown."""
    rng = random.Random(seed)
    objs = [cap(skull, f"{name}_Cap", mat, front=0.5, side=-0.2, back=-0.97)]
    sweep = Vector(sweep).normalized()
    locks = []
    for k in range(48):
        az = rng.uniform(0, math.tau)
        z = rng.uniform(-0.75 if abs(math.remainder(az, math.tau)) > 2.0 else 0.25, 0.97)
        root = on_sphere(az, z)
        if root.y < -0.55 and z < 0.7:
            continue
        flow = sweep * 0.8 + Vector((math.sin(az), -math.cos(az), 0)) * 0.5 + Vector((0, 0, -0.4 * (1 - z)))
        path = comb(root, flow, rng.uniform(0.55, 0.95), steps=10, lift=(0.08, 0.15), gravity=0.2,
                    bend=rng.uniform(-0.3, 0.2), tip_bend=-0.6, twist=rng.uniform(-0.3, 0.3))
        locks.append((path, rng.uniform(0.24, 0.32)))
    for k in range(4):  # a cowlick at the crown
        root = on_sphere(math.pi + rng.uniform(-0.4, 0.4), 0.9)
        path = comb(root, Vector((rng.uniform(-0.3, 0.3), 0.6, 1.0)), rng.uniform(0.35, 0.5), steps=8, lift=(0.05, 0.2), bend=-1.2)
        locks.append((path, 0.16))
    for k in range(3):  # a forelock falling over one side of the brow
        root = on_sphere(-0.35 - 0.12 * k, 0.75)
        path = comb(root, Vector((-0.3, -1, -0.6)), 0.75 - 0.1 * k, steps=10, lift=(0.05, 0.1), gravity=0.5, bend=0.3)
        locks.append((path, 0.22))
    for k, (path, width) in enumerate(locks):
        objs.append(lock_mesh(skull, f"{name}_Lock{k}", path, width, mat))
    return objs


def curls(skull, name, mat, seed, top=1.0, lift=1.1, nape=-0.85):
    """A mop of curls: soft round clumps blended into one cloud over the skull, a curl or two lifting
    off it, and a fringe of curls over the brow. top: the highest root (under a cap, only what shows
    below its rim is grown). nape: how low the hair grows at the back (in skull radii; higher on a head that
    sits low on its shoulders, so the curls rest above the collar, not in it)."""
    rng = random.Random(seed)
    objs = [cap(skull, f"{name}_Cap", mat, front=0.5, side=-0.15, back=nape, lift=1.12)]
    s = skull.scale()
    items = []
    tries = 0
    while len(items) < 46 and tries < 2000:
        tries += 1
        az = rng.uniform(0, math.tau)
        z = rng.uniform(nape + 0.1, top)
        if z < hairline(az, 0.5, -0.15, nape) or (abs(math.remainder(az, math.tau)) < 0.9 and z < 0.55):
            continue  # above the hairline, and clear of the face
        root = on_sphere(az, z)
        size = rng.uniform(0.2, 0.28)
        items.append((skull.world(root * (lift + size * 0.4)), s * size))
    for k in range(5):  # a fringe of curls over the brow, below a cap's rim
        x = -0.5 + 0.25 * k
        root = Vector((x * 0.75, -0.7, min(top, 0.62))).normalized()
        items.append((skull.world(root * (lift + 0.06)), s * rng.uniform(0.17, 0.21)))
    objs.append(shapes.blobs(f"{name}_Cloud", items, mat, resolution=s * 0.045))
    # A few curls spring off the cloud, so its edge is alive rather than a smooth pudding.
    for k in range(7):
        az = rng.uniform(0, math.tau)
        if abs(math.remainder(az, math.tau)) < 2.1:
            continue  # only at the back, never beside the face
        z = rng.uniform(max(-0.4, nape + 0.25), min(top, 0.4))
        root = on_sphere(az, z)
        out = Vector((math.sin(az), -math.cos(az), 0))
        path = comb(root * 1.25, out * 0.7 + Vector((0, 0, -0.5)), 0.32, steps=8, lift=(0.25, 0.32), bend=2.6, twist=rng.uniform(-0.6, 0.6))
        objs.append(lock_mesh(skull, f"{name}_Spring{k}", path, 0.2, mat, flat=0.7))
    return objs


STYLES = {"bob": bob, "swept": swept, "curls": curls}


def grow(b, style, mat, seed, **options):
    """A body's hair, joined into one object."""
    skull = Skull(b)
    objs = STYLES[style](skull, f"{b.name}_HairPart", mat, seed, **options)
    return shapes.join(f"{b.name}_Hair", objs)
