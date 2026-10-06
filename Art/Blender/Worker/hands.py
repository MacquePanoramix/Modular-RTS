"""Wonder Gather — hands that close (S1d, the miners at work).

A free hand is modelled relaxed and open. Its four fingers and its thumb get three bones each, so the game can
close it round a handle and open it again. This module makes:

- the finger bones (joints) of every hand that closes;
- the weights of the hand's skin on them (weights);
- the closing itself (table): for a handle of a given thickness, where every finger joint goes so that the
  fingers and the thumb lie on the handle and never enter it. That is measured on the skinned mesh, as the game
  will skin it, and each digit is moved until it touches: nothing is assumed from the ideal shapes.

A hand modelled closed round what it carries (a lantern's handle, a mug's) is fused shut and cut to fit its
handle exactly. It stays as modelled and gets no finger bones.

Run alone: blender -b --factory-startup --python workers.py -- --out <folder> --hands [--only Round]
"""
import math
import os
import re

import bpy
import numpy as np
from mathutils import Matrix, Vector

import body

FINGERS = ("Index", "Middle", "Ring", "Little")
# The handles the table is made for (radius, metres): from a lantern's wire grip to a thick tool's handle.
# The game blends between neighbours for anything in between.
RADII = (0.006, 0.009, 0.012, 0.015, 0.018, 0.022)
# A digit touches its handle: nearer than this is "meets"; deeper than this is "enters" (metres).
MEETS, ENTERS = 0.001, 0.0005
# The thumb's reach, in hand lengths: how far short of the handle's axis it aims. Open, it is folded back above
# the palm, clear of where the handle will lie; closing, it unfolds forward until it meets something.
THUMB_OPEN, THUMB_FAR = 0.36, -0.45
# A finger tapers: how thick each of its three bones is, against its thickness at the knuckle (body.hand). Its
# outer bones lie that much nearer a handle than its first.
TAPER = (0.975, 0.915, 0.85)


def side_of(i):
    """Body index 0 is the being's right (x < 0), 1 its left."""
    return "R" if i == 0 else "L"


def closing(b):
    """The hands that close: {body index: what body.hand recorded of it}."""
    return {i: h for i, h in getattr(b, "hands", {}).items() if h["closes"]}


def of(obj, b):
    """The body index of the hand this part is, if it is a hand that closes."""
    m = re.search(r"_Hand(\d)$", obj.name)
    return int(m.group(1)) if m and int(m.group(1)) in closing(b) else None


def digits(b, i):
    """A closing hand's digits, the four fingers and then the thumb: (its three bones, its four joints from the
    knuckle to the tip, its radius)."""
    h, s = b.hands[i], side_of(i)
    out = [([f"{FINGERS[k]}{j}.{s}" for j in (1, 2, 3)], f["points"], f["radius"]) for k, f in enumerate(h["fingers"])]
    out.append(([f"Thumb{j}.{s}" for j in (1, 2, 3)], h["thumb"]["points"], h["thumb"]["radius"]))
    return out


def joints(b, bones):
    """Adds every closing hand's finger bones to the skeleton: each from its joint to the next, on its hand."""
    for i in sorted(closing(b)):
        for names, pts, r in digits(b, i):
            parent = f"Hand.{side_of(i)}"
            for j, n in enumerate(names):
                bones[n] = (pts[j].copy(), pts[j + 1].copy(), r, parent)
                parent = n


# ---------------------------------------------------------------- the skin's weights

def smooth(x):
    x = max(0.0, min(1.0, x))
    return x * x * (3 - 2 * x)


def along(co, pts):
    """The nearest place on a digit's line of joints: (how far from it, how far along it from the knuckle).
    Behind the knuckle the length along is negative, measured on the first bone's own line."""
    best, start = None, 0.0
    for j in range(len(pts) - 1):
        a, ab = pts[j], pts[j + 1] - pts[j]
        length = ab.length
        t = (co - a).dot(ab) / max(length * length, 1e-12)
        tc = max(0.0, min(1.0, t))
        d = (co - (a + ab * tc)).length
        u = start + (t if j == 0 and t < 0 else tc) * length
        if best is None or d < best[0]:
            best = (d, u)
        start += length
    return best


def weights(b, i, co, base):
    """One point of a closing hand's skin: base (its weights as a rigid hand: hand and forearm) shared with the
    bones of the digits it lies on. Along a digit the weight passes from bone to bone across each joint; between
    two fingers that are fused side by side it is shared by nearness. The joints' zones never overlap, so a point
    is on at most two bones of a finger, and on at most four in all where two fingers meet: the game keeps four,
    and dropping a fifth would crease the skin."""
    found = []
    for names, pts, r in digits(b, i):
        d, u = along(co, pts)
        member = max(0.0, min(1.0, (1.9 * r - d) / (0.6 * r)))
        if member <= 0:
            continue
        s1 = (pts[1] - pts[0]).length
        s2 = s1 + (pts[2] - pts[1]).length
        knuckle = 1.1 * r
        joint = min(0.7 * r, 0.48 * (s2 - s1), 0.48 * (pts[3] - pts[2]).length, s1 - knuckle - 0.001)
        a0 = smooth((u + knuckle) / (2 * knuckle))
        a1 = min(a0, smooth((u - s1 + joint) / (2 * joint)))
        a2 = min(a1, smooth((u - s2 + joint) / (2 * joint)))
        # Shared by nearness, gently: two fused fingers that bend about different joints shear the skin between
        # them, and a gentle share spreads that over both fingers' sides where a sharp one would fold the groove.
        found.append((member, 1.0 / max(d, 1e-4) ** 2, 1 - a0, {names[0]: a0 - a1, names[1]: a1 - a2, names[2]: a2}))
    if not found:
        return base
    share = max(m for m, _, _, _ in found)
    total = sum(m * k for m, k, _, _ in found)
    out, rigid = {}, 1.0 - share
    for m, k, hand, ws in found:
        f = share * m * k / total
        rigid += f * hand
        for n, w in ws.items():
            if w > 0:
                out[n] = out.get(n, 0.0) + f * w
    for n, w in base:
        out[n] = out.get(n, 0.0) + rigid * w
    return [(n, w) for n, w in out.items() if w > 0.004]


# ---------------------------------------------------------------- closing round a handle

def local(h, x, y, z):
    """A place in the hand, in hand lengths: along the fingers from the wrist, towards the thumb, out of the palm."""
    L = h["length"]
    return h["wrist"] + h["along"] * (x * L) + h["thumbward"] * (y * L) + h["palm"] * (z * L)


def handle(h, radius, lift=0.0):
    """Where a handle of this radius lies in the closed hand: in the plane the fingers curl in (hand lengths
    along, and out of the palm), a point on its axis in the middle of the fingers, and its direction. It lies
    at the root of the fingers; lift: how far its surface stands off the hand's middle there (metres), which
    table finds so that it rests on the palm."""
    L = h["length"]
    centre = (0.5 + 0.35 * 0.44 * 0.45, 0.085 + (radius + 0.001 + lift) / L)
    return centre, local(h, centre[0], 0.0, centre[1]), h["thumbward"].copy()


def finger_goals(h, radius, stand, lift=0.0):
    """Where each finger's four joints go to close round a handle. stand: how far from the handle's surface
    each finger's own line lies at its knuckle's thickness (four values, metres); its outer bones, being
    thinner, lie nearer by its taper."""
    L, n = h["length"], h["palm"]
    centre, _, _ = handle(h, radius, lift)
    out = []
    for k, f in enumerate(h["fingers"]):
        curls = body.wrap((f["knuckle"], 0.0), f["lengths"], centre, [(radius + stand[k] * taper) / L for taper in TAPER])
        out.append(curled(h, f, curls))
    return out


def curled(h, f, curls):
    """A finger's four joints with its three joints turned by curls (radians, from straight). It curls about one
    axis, across the finger: taken once from the straight finger, not from each bone as it goes (a bone that
    points straight out of the palm has no such axis of its own)."""
    L = h["length"]
    across = f["heading"].cross(h["palm"]).normalized()
    d, pts = f["heading"].copy(), [f["points"][0].copy()]
    for seg in range(3):
        d = (Matrix.Rotation(curls[seg], 3, across) @ d).normalized()
        pts.append(pts[-1] + d * L * f["lengths"][seg])
    return pts


def opened(h):
    """Every digit's joints with the hand open, as it is before it closes on a handle: the fingers nearly
    straight, the thumb folded back above the palm, clear of where the handle will lie."""
    out = [curled(h, f, body.GRIPS["open"][k]) for k, f in enumerate(h["fingers"])]
    out.append(thumb_goal(h, 0.015, 0.0, THUMB_OPEN))
    return out


def thumb_goal(h, radius, lift, reach, onto=0.0):
    """The thumb's joints as it closes on a handle from the wrist's side, its tip towards the first finger's.
    reach: how far short of the handle's axis it aims, in hand lengths (large: folded back above the palm;
    less: unfolded forward, past the axis when negative). Its bones keep their lengths: each takes its
    direction from that path. onto: how far its bones turn from that path straight towards the handle (0 to 1:
    its last two bones; 1 to 2: its first bone as well, a little). It is stopped on the way by whatever it meets
    first (close)."""
    centre, at, axis = handle(h, radius, lift)
    path = [local(h, *q) for q in ((0.12, 0.17, 0.04), (0.29, 0.28, 0.12),
                                    (centre[0] - reach * 0.98, 0.25, centre[1] + reach * 0.2),
                                    (centre[0] - reach * 0.72, 0.11, centre[1] + reach * 0.7))]
    rest = h["thumb"]["points"]
    pts = [rest[0].copy()]
    for seg in range(3):
        d = (path[seg + 1] - path[seg]).normalized()
        share = min(1.0, onto) if seg else max(0.0, onto - 1.0) * 0.4
        if share > 0:
            aim = (at + axis * (pts[-1] - at).dot(axis) - pts[-1]).normalized()
            d = d.slerp(aim, share, d.lerp(aim, share)).normalized()
        pts.append(pts[-1] + d * (rest[seg + 1] - rest[seg]).length)
    return pts


def turns(rest, goal):
    """A digit's three bones turned so its joints go from rest to goal: for each, the turn (3x3, about its own
    joint at rest) and where its joint goes. Each bone takes the smallest turn that aims it: fingers do not twist.
    The game makes the same turns from the same points (MinerSetup)."""
    out, carried = [], Matrix.Identity(3)
    for j in range(3):
        at_rest = (rest[j + 1] - rest[j]).normalized()
        want = carried.inverted() @ (goal[j + 1] - goal[j]).normalized()
        carried = carried @ at_rest.rotation_difference(want).to_matrix()
        out.append((np.array(carried), np.array(goal[j])))
    return out


class Skin:
    """A hand's skin as numbers: its points at rest, and each point's weight on each finger bone."""

    def __init__(self, obj, b, i):
        mesh = obj.data
        self.rest = np.array([v.co[:] for v in mesh.vertices], dtype=np.float64)
        self.polys = [tuple(p.vertices) for p in mesh.polygons]
        self.digits = digits(b, i)
        index = {g.index: g.name for g in obj.vertex_groups}
        self.w = {n: np.zeros(len(self.rest)) for names, _, _ in self.digits for n in names}
        for v in mesh.vertices:
            for g in v.groups:
                n = index.get(g.group)
                if n in self.w:
                    self.w[n][v.index] = g.weight
        # Which points are a digit's own (more than half on its bones), and which the palm's (more than half
        # on the hand itself: the palm and the pads at the roots of the fingers).
        self.own = [sum(self.w[n] for n in names) > 0.5 for names, _, _ in self.digits]
        self.palm = sum(self.w.values()) < 0.5
        # A digit's outer part: the skin of its two outer bones. That is what closes on a handle; the root of a
        # finger does not move away from a handle that lies against it, however the finger curls.
        self.outer = [self.w[names[1]] + self.w[names[2]] > 0.5 for names, _, _ in self.digits]

    def posed(self, goals):
        """The skin with each digit's joints at its goal: linear blend skinning, as the game skins."""
        out = self.rest * (1.0 - sum(self.w.values()))[:, None]
        for (names, pts, _), goal in zip(self.digits, goals):
            for n, j, (turn, at) in zip(names, range(3), turns(pts, goal)):
                moved = (self.rest - np.array(pts[j])) @ turn.T + at
                out += moved * self.w[n][:, None]
        return out


def from_axis(points, at, axis):
    """Each point's distance from a handle's axis."""
    rel = points - np.array(at)
    a = np.array(axis)
    return np.linalg.norm(rel - np.outer(rel @ a, a), axis=1)


def from_line(points, pts, radius):
    """Each point's distance from a digit's surface, taken as its line of joints with its thickness."""
    best = np.full(len(points), 1e9)
    for j in range(len(pts) - 1):
        a, ab = np.array(pts[j]), np.array(pts[j + 1]) - np.array(pts[j])
        t = np.clip(((points - a) @ ab) / max(float(ab @ ab), 1e-12), 0.0, 1.0)
        best = np.minimum(best, np.linalg.norm(points - (a + np.outer(t, ab)), axis=1))
    return best - radius


def close(skin, h, radius, lift):
    """One hand closed round one handle lying at one place: each finger closed until the skin of its two outer
    bones touches the handle (found by halving, on the skinned mesh), then the thumb closed until it touches the
    handle or a finger. Returns the joints, how near each digit came (clear: its outer part, the thumb to
    whichever it met; deep: the whole digit, its root included), how near the palm is, and what the thumb met."""
    L = h["length"]
    _, at, axis = handle(h, radius, lift)
    wide = opened(h)[4]
    low, high = [0.0] * 4, [f["radius"] * 2.5 for f in h["fingers"]]
    for _ in range(18):
        stand = [(a + b_) * 0.5 for a, b_ in zip(low, high)]
        far = from_axis(skin.posed(finger_goals(h, radius, stand, lift) + [wide]), at, axis) - radius
        for k in range(4):
            if float(far[skin.outer[k]].min()) > 0:
                high[k] = stand[k]
            else:
                low[k] = stand[k]
    fingers = finger_goals(h, radius, high, lift)

    def thumb_clear(reach, onto=0.0):
        posed = skin.posed(fingers + [thumb_goal(h, radius, lift, reach, onto)])
        thumb = posed[skin.own[4]]
        on_handle = float((from_axis(thumb, at, axis) - radius).min())
        on_fingers = min(float(from_line(thumb, fingers[k], h["fingers"][k]["radius"] * 0.92).min()) for k in range(4))
        return on_handle, on_fingers
    # From folded back, the thumb unfolds forward until it first touches the handle or a finger.
    steps = 36
    a, onto, nearest = None, 0.0, (1e9, THUMB_OPEN)
    for step in range(1, steps + 1):
        at_step = THUMB_OPEN + (THUMB_FAR - THUMB_OPEN) * step / steps
        near = min(thumb_clear(at_step))
        if near < 0:
            a, b_ = THUMB_OPEN + (THUMB_FAR - THUMB_OPEN) * (step - 1) / steps, at_step
            for _ in range(12):
                mid = (a + b_) * 0.5
                if min(thumb_clear(mid)) < 0:
                    b_ = mid
                else:
                    a = mid
            break
        nearest = min(nearest, (near, at_step))
    if a is None:
        # It would pass between the handle and the fingers: from where it comes nearest, its tip turns onto
        # the handle until it touches.
        a = nearest[1]
        low_, high_ = 0.0, 2.0
        if min(thumb_clear(a, 2.0)) < 0:
            for _ in range(15):
                mid = (low_ + high_) * 0.5
                if min(thumb_clear(a, mid)) < 0:
                    high_ = mid
                else:
                    low_ = mid
            onto = low_
        else:
            onto = 2.0
    goals = fingers + [thumb_goal(h, radius, lift, a, onto)]
    posed = skin.posed(goals)
    far = from_axis(posed, at, axis) - radius
    met = thumb_clear(a, onto)
    clear = [float(far[skin.outer[k]].min()) for k in range(4)] + [min(met)]
    deep = [float(far[skin.own[k]].min()) for k in range(4)] + [min(met)]
    palm = float(far[skin.palm].min()) if skin.palm.any() else 0.0
    return dict(radius=radius, goals=goals, clear=clear, deep=deep, palm=palm, at=at, axis=axis, posed=posed, lift=lift, thumb=met,
                thumb_closed=a)


def table(b, i, obj, radii=RADII):
    """A closing hand's table: for each handle radius, where its joints go, where the handle lies, and how near
    each digit and the palm came (metres; negative: inside).
    The handle lies at the root of the fingers, lowered onto the palm as far as the fingers can still close
    round it without entering it (a thin handle stays in the hook of the fingers, off the palm)."""
    h, skin = b.hands[i], Skin(obj, b, i)

    def good(row):
        return all(-0.0002 <= c <= 0.0004 for c in row["clear"][:4]) and min(row["deep"][:4]) > -0.0003 and row["palm"] > -0.0003
    rows = []
    for radius in radii:
        kept = close(skin, h, radius, 0.0)
        # Where it first lies it may press into the palm (a thick handle in a small hand): lifted until it rests.
        for _ in range(8):
            pressed = min(kept["palm"], min(kept["deep"][:4]))
            if pressed >= -0.0002:
                break
            kept = close(skin, h, radius, kept["lift"] - pressed + 0.0001)
        step = 0.002
        while kept["palm"] > 0.0004 and step > 0.0002:
            trial = close(skin, h, radius, kept["lift"] - min(step, kept["palm"]))
            if good(trial):
                kept = trial
            else:
                step *= 0.5
        rows.append(kept)
    return skin, rows


def tables(b, meshes):
    """Every closing hand's table, from its skinned part: {body index: (skin, rows)}."""
    out = {}
    for i in sorted(closing(b)):
        obj = next((o for o in meshes if o.name == f"{b.name}_Hand{i}"), None)
        if obj is not None:
            out[i] = table(b, i, obj)
    return out


# ---------------------------------------------------------------- the report, the pictures, the game's data

def report(b, tables, out_dir, pictures=True):
    """hands_<Name>.txt: for every closing hand and handle, how each digit meets it. With pictures, each closed
    hand from three sides (hands_<Name>_<hand>_<radius>_<view>.png)."""
    name = b.name.split("_")[-1]
    lines, failing, count = [], 0, 0
    for i, (skin, rows) in sorted(tables.items()):
        labels = list(FINGERS) + ["Thumb"]
        for row in rows:
            for label, c, deep in zip(labels, row["clear"], row["deep"]):
                ok = c <= MEETS and deep >= -ENTERS
                failing += not ok
                count += 1
                what = "enters" if deep < -ENTERS else "off" if c > MEETS else "meets"
                lines.append(f"  {'ok  ' if ok else 'FAIL'} grip  Hand.{side_of(i)} {label:<6} on a handle of {row['radius'] * 2000:4.0f} mm"
                             f"  {what:<6} {c * 1000:+5.2f} mm, at its deepest {deep * 1000:+5.2f} mm"
                             f"  (meets within {MEETS * 1000:.1f}, enters past {ENTERS * 1000:.1f})")
            ok = row["palm"] >= -ENTERS
            failing += not ok
            count += 1
            lines.append(f"  {'ok  ' if ok else 'FAIL'} grip  Hand.{side_of(i)} palm   on a handle of {row['radius'] * 2000:4.0f} mm"
                         f"  {'enters' if not ok else 'rests ' if row['palm'] <= MEETS else 'clear '} {row['palm'] * 1000:+5.2f} mm"
                         f"  (the thumb: {row['thumb'][0] * 1000:+.1f} from the handle, {row['thumb'][1] * 1000:+.1f} from the fingers,"
                         f" reach {row['thumb_closed']:+.2f}; the handle lowered {-row['lift'] * 1000:.1f} mm onto the palm)")
            if pictures:
                picture(b, i, skin, row, os.path.join(out_dir, f"hands_{name}_{side_of(i)}_{round(row['radius'] * 2000):02d}"))
        if pictures and rows:
            picture(b, i, skin, rows[len(rows) // 2], os.path.join(out_dir, f"hands_{name}_{side_of(i)}_00"), rest=True)
            wide = dict(rows[len(rows) // 2], posed=skin.posed(opened(b.hands[i])))
            picture(b, i, skin, wide, os.path.join(out_dir, f"hands_{name}_{side_of(i)}_01"), bare=True)
    text = f"HANDS {name}: {failing} failing of {count} checks (a handle's thickness is its diameter)\n" + "\n".join(lines)
    print(text)
    with open(os.path.join(out_dir, f"hands_{name}.txt"), "w", encoding="utf-8") as f:
        f.write(text + "\n")
    return failing


def picture(b, i, skin, row, out_path, rest=False, bare=False):
    """A closed hand and its handle from four sides: along the handle from each end, onto the fingertips, and
    onto the knuckles. rest: the hand as modelled, without a handle; bare: the row's pose without its handle."""
    scene = bpy.context.scene
    engines = {e.identifier for e in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items}
    scene.render.engine = 'BLENDER_EEVEE_NEXT' if 'BLENDER_EEVEE_NEXT' in engines else 'BLENDER_EEVEE'
    scene.render.resolution_x, scene.render.resolution_y = 640, 640
    cam = bpy.data.objects.get("HandCam") or bpy.data.objects.new("HandCam", bpy.data.cameras.new("HandCam"))
    if cam.name not in scene.collection.objects:
        scene.collection.objects.link(cam)
    cam.data.lens, cam.data.clip_start = 70, 0.01
    scene.camera = cam
    for n, rot, energy in (("HandKey", (55, 0, -30), 3.0), ("HandFill", (70, 0, 150), 1.4)):
        if not bpy.data.objects.get(n):
            light = bpy.data.objects.new(n, bpy.data.lights.new(n, 'SUN'))
            light.rotation_euler = tuple(math.radians(a) for a in rot)
            light.data.energy = energy
            scene.collection.objects.link(light)
    world = scene.world or bpy.data.worlds.new("HandWorld")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.33, 0.35, 0.4, 1)
    scene.world = world
    scene.view_settings.view_transform = 'Standard'
    hidden = [o for o in scene.objects if o.type == 'MESH' and not o.hide_render]
    for o in hidden:
        o.hide_render = True
    h = b.hands[i]
    mesh = bpy.data.meshes.new("HandPosed")
    mesh.from_pydata((skin.rest if rest else row["posed"]).tolist(), [], [list(p) for p in skin.polys])
    for poly in mesh.polygons:
        poly.use_smooth = True
    hand = bpy.data.objects.new("HandPosed", mesh)
    source = next(o for o in hidden if o.name == f"{b.name}_Hand{i}")
    for slot in source.material_slots:
        hand.data.materials.append(slot.material)
    scene.collection.objects.link(hand)
    made = [hand]
    if not rest and not bare:
        wood = bpy.data.materials.get("HandleWood") or bpy.data.materials.new("HandleWood")
        wood.diffuse_color = (0.42, 0.27, 0.14, 1)
        bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=row["radius"], depth=h["length"] * 0.95, location=row["at"])
        bar = bpy.context.active_object
        bar.rotation_euler = Vector(row["axis"]).to_track_quat('Z', 'Y').to_euler()
        bar.data.materials.append(wood)
        for poly in bar.data.polygons:
            poly.use_smooth = True
        made.append(bar)
    at = Vector(row["at"])
    a, t, n = h["along"], h["thumbward"], h["palm"]
    # Along the handle from the thumb's side and from the little finger's; onto the fingertips; onto the knuckles.
    for k, d in enumerate((t * 1.0 + n * 0.08 + a * 0.05, -t * 1.0 + n * 0.08 + a * 0.05, n * 1.0 + a * 0.45 + t * 0.25, -n * 1.0 + a * 0.6 + t * 0.3)):
        cam.location = at + d.normalized() * h["length"] * 2.1
        cam.rotation_euler = (at - cam.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = f"{out_path}_{k}.png"
        bpy.ops.render.render(write_still=True)
    for o in made:
        bpy.data.objects.remove(o)
    for o in hidden:
        o.hide_render = False


def export(b, tables):
    """For the game (miners.json, "grips"): each closing hand's finger bones, their joints at rest and closed
    round each handle, and where the handle lies. Places are in the being's space as the game has it: x to its
    right, y up, z forward, from the ground under its middle."""
    def game(v):
        return [round(-v[0], 5), round(v[2], 5), round(-v[1], 5)]

    def way(v):
        return [round(-v[0], 5), round(v[2], 5), round(-v[1], 5)]
    out = []
    for i, (skin, rows) in sorted(tables.items()):
        ds = digits(b, i)
        out.append(dict(
            hand=f"Hand.{side_of(i)}",
            bones=[n for names, _, _ in ds for n in names],
            rest=[c for _, pts, _ in ds for p in pts for c in game(p)],
            open=[c for pts in opened(b.hands[i]) for p in pts for c in game(p)],
            radii=[round(r["radius"], 5) for r in rows],
            closed=[c for r in rows for goal in r["goals"] for p in goal for c in game(p)],
            centres=[c for r in rows for c in game(r["at"])],
            axis=way(rows[0]["axis"]),
            # The way the fingers leave the wrist, and the way the palm faces: with the axis, the hand's own frame.
            along=way(b.hands[i]["along"]),
            palm=way(b.hands[i]["palm"]),
        ))
    return out
