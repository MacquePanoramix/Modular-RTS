"""Wonder Gather — the body module (S1d).

A being's body is a posed skeleton built from a few proportions, the same
joints the procedural rig drives: pelvis, waist, chest, neck and head; shoulders,
elbows and wrists; hips, knees and ankles. Arms and legs reach for targets
with two-bone solving, so a pose is described by where the hands and feet go.

Everything else is fitted to this skeleton: the visible skin (head, neck, hands,
bare forearms), the boots, and every garment and accessory in outfits.py. A body
of other proportions takes the same outfit. That is the root of a character
creator: modules that fit any body.
"""
import math

from mathutils import Matrix, Vector

import shapes
from shapes import blobs, chain, frame, fuse, tube, two_bone


class Body:
    """A posed skeleton and the measurements garments fit to."""

    def __init__(self, name, p):
        self.name, self.p = name, p
        H = self.H = p["height"]
        self.hr = p["head"]
        self.hs = Vector(p.get("head_shape", (1, 1, 1)))
        pose = p.get("pose", {})
        self.sw = pose.get("weight", 1)  # the side whose leg carries the weight (+1 is the left)
        shift = pose.get("hip_shift", 0.02)
        stoop = pose.get("stoop", 0.0)
        back = pose.get("back", 0.0)
        # The trunk: (half-width, half-depth) of the body itself at each level.
        self.trunk = p["trunk"]
        z = lambda k: p[k] * H
        self.pelvis = Vector((self.sw * shift, 0, z("hip") + 0.03))
        self.waist = Vector((self.sw * shift * 0.7, -stoop * 0.01, z("waist")))
        self.chest = Vector((self.sw * shift * 0.2, -stoop * 0.07 + back * 0.025, z("chest")))
        self.collar = self.chest + Vector((0, -stoop * 0.03, p.get("collar", 0.07) * H))
        # The head: turned, tilted and nodded; everything on it is built in its own frame.
        self.hm = (Matrix.Rotation(pose.get("turn", 0), 3, 'Z') @ Matrix.Rotation(pose.get("nod", 0), 3, 'X')
                   @ Matrix.Rotation(pose.get("tilt", 0), 3, 'Y'))
        # neck_drop sets the head lower on the shoulders: a short, sturdy neck.
        self.head = Vector((-self.sw * shift * 0.2, -stoop * 0.2 - 0.01 + back * 0.02, H - self.hr * 1.15 * self.hs.z - p.get("neck_drop", 0.0)))
        # Shoulders tilt against the hips; arms and legs reach for their targets.
        tilt = pose.get("shoulder_tilt", 0.015)
        self.shoulders = [self.chest + Vector((s * p["shoulder_w"], 0.01, p.get("shoulder_h", 0.045) * H - s * self.sw * tilt)) for s in (-1, 1)]
        self.upper, self.fore = p.get("upper_arm", 0.17) * H, p.get("forearm", 0.145) * H
        self.elbows, self.wrists = [], []
        # Each arm reaches its wrist target; pole x points outwards on either side.
        for s, sh, arm in zip((-1, 1), self.shoulders, p["arms"]):
            elbow, wrist = two_bone(sh, arm["wrist"](self, s), self.upper, self.fore, Vector(arm.get("pole", (1, 0.4, -0.3))) * Vector((s, 1, 1)))
            self.elbows.append(elbow)
            self.wrists.append(wrist)
        hw = p["hip_w"]
        self.hips = [self.pelvis + Vector((s * hw, 0, -0.05 + s * self.sw * pose.get("hip_tilt", 0.012))) for s in (-1, 1)]
        self.knees, self.ankles = [], []
        ankle_z = 0.085
        neutral = pose.get("neutral", False)  # the rest pose for rigging: feet under the hips, toes a little out
        for s, hip in zip((-1, 1), self.hips):
            if neutral:
                target = Vector((s * hw * 1.1, 0.0, ankle_z))
            elif s == self.sw:
                target = Vector((self.head.x + s * 0.035, 0.0, ankle_z))
            else:
                free = pose.get("free_foot", (1.7, -0.08))
                target = Vector((s * hw * free[0], free[1], ankle_z))
            length = (hip.z - ankle_z) * 0.51
            knee, ankle = two_bone(hip, target, length, length, Vector((0, -1, 0.0)))
            self.knees.append(knee)
            self.ankles.append(ankle)
        self.foot_yaw = [s * 0.1 if neutral else s * 0.25 + (0.12 * s if s != self.sw else 0) for s in (-1, 1)]

    # Measurements garments fit to.
    def width(self, level):
        return self.trunk[level][0]

    def depth(self, level):
        return self.trunk[level][1]

    def front(self, level):
        """How far in front of the trunk's axis its surface is, at a level."""
        return self.trunk[level][1]

    def on_head(self, x, y, z):
        return self.head + self.hm @ Vector((x * self.hs.x, y * self.hs.y, z * self.hs.z)) * self.hr

    def arm_joints(self, i):
        sh, el, wr = self.shoulders[i], self.elbows[i], self.wrists[i]
        inside = self.chest.lerp(sh, 0.4) + Vector((0, 0, 0.03))
        return [inside, sh, sh.lerp(el, 0.5), el, el.lerp(wr, 0.45), wr]

    def leg_joints(self, i):
        hp, kn, an = self.hips[i], self.knees[i], self.ankles[i]
        return [hp, hp.lerp(kn, 0.5), kn, kn.lerp(an, 0.4), an]

    def foot_frame(self, i):
        """The foot's forward direction (yawed out) and its ankle."""
        yaw = Matrix.Rotation(self.foot_yaw[i], 3, 'Z')
        return yaw @ Vector((0, -1, 0)), self.ankles[i]


# ---------------------------------------------------------------- skin: forearms, hands

def forearm(b, i, skin, start=0.1):
    el, wr = b.elbows[i], b.wrists[i]
    r = b.p["arm"]
    return chain(f"{b.name}_Forearm{i}", [el.lerp(wr, start), el.lerp(wr, 0.45), wr],
                 [(r * 0.82, r * 0.76), (r * 0.78, r * 0.7), (r * 0.56, r * 0.48)], skin)


# Curl of each finger's three joints, by grip, from the index to the little finger.
GRIPS = {
    "relaxed": [(0.25, 0.35, 0.25), (0.3, 0.4, 0.3), (0.38, 0.45, 0.3), (0.45, 0.5, 0.35)],
    "fist": [(1.55, 1.7, 1.2), (1.6, 1.7, 1.2), (1.6, 1.7, 1.2), (1.65, 1.7, 1.2)],
    "hold": [(1.0, 1.2, 0.9), (1.1, 1.25, 0.95), (1.15, 1.3, 0.95), (1.25, 1.35, 1.0)],
    "grip": [(1.3, 1.45, 1.05), (1.35, 1.5, 1.05), (1.4, 1.5, 1.05), (1.45, 1.5, 1.1)],
    # Carrying a handle that hangs from the hand (a lantern, a pail), when no handle's size is given
    # (given one, the fingers are wrapped round it exactly: see hand's bar).
    "carry": [(1.25, 1.35, 0.85), (1.3, 1.4, 0.9), (1.35, 1.4, 0.9), (1.4, 1.45, 0.95)],
    "open": [(0.1, 0.15, 0.1), (0.12, 0.18, 0.12), (0.15, 0.2, 0.12), (0.2, 0.25, 0.15)],
}
# The thumb's path by grip, in hand lengths along (the fingers' direction, towards the thumb, the palm's side).
# In a fist it folds across the front of the curled fingers; in a grip it closes round the handle.
THUMBS = {
    "relaxed": [(0.12, 0.17, 0.04), (0.28, 0.27, 0.1), (0.41, 0.3, 0.15), (0.52, 0.29, 0.18)],
    "open": [(0.12, 0.17, 0.03), (0.28, 0.31, 0.05), (0.42, 0.38, 0.06), (0.54, 0.42, 0.06)],
    "fist": [(0.12, 0.17, 0.04), (0.3, 0.26, 0.17), (0.44, 0.18, 0.3), (0.55, 0.05, 0.36)],
    "grip": [(0.12, 0.17, 0.04), (0.29, 0.27, 0.15), (0.42, 0.22, 0.28), (0.51, 0.1, 0.35)],
    "hold": [(0.12, 0.17, 0.04), (0.29, 0.28, 0.12), (0.42, 0.3, 0.23), (0.52, 0.26, 0.29)],
    "carry": [(0.12, 0.17, 0.04), (0.29, 0.27, 0.14), (0.43, 0.24, 0.27), (0.53, 0.14, 0.34)],
}


def wrap(knuckle, segments, centre, radius):
    """The turns at a finger's three joints that wrap it round a bar: in the plane the finger curls in
    (x along the hand, y towards the palm side), each segment leaves its joint along the tangent to the circle
    (centre, radius) that keeps the bar on the curling side. radius: one for all three segments, or one each (a
    finger tapers, so its outer bones lie nearer the bar). Returns the three turns, in radians."""
    x, y = knuckle
    heading, turns = 0.0, []
    each = list(radius) if isinstance(radius, (list, tuple)) else [radius] * len(segments)
    for length, radius in zip(segments, each):
        vx, vy = centre[0] - x, centre[1] - y
        dist = math.hypot(vx, vy)
        aim = math.atan2(vy, vx) - math.asin(min(1.0, radius / max(dist, 1e-6)))
        turn = (aim - heading + math.pi) % math.tau - math.pi
        turn = max(0.0, min(1.75, turn))
        heading += turn
        turns.append(turn)
        x, y = x + length * math.cos(heading), y + length * math.sin(heading)
    return turns


def hand(b, i, skin, grip="relaxed", palm=None, along=None, ring=None, bar=None):
    """A stylized working hand: a full palm, four chunky fingers of three joints curled by the grip, and a
    thumb that wraps what the hand holds. palm: the direction the palm faces; along: the direction the
    fingers leave the wrist; ring: the direction a held handle runs through the curled fingers.
    bar: the radius of a handle the hand carries (a lantern's, a mug's). The fingers are then wrapped round a
    bar of that radius lying in their hook along the ring direction, the thumb closes the hook, and the hand is
    cut to fit the bar exactly: the handle touches the fingers all round and never passes through them.
    Returns the hand, where it holds things (the bar's centre), and its axes (along, ring, palm side)."""
    s = (-1, 1)[i]
    L = b.p.get("hand", 0.11) * b.H
    wr = b.wrists[i]
    a = Vector(along or (wr - b.elbows[i])).normalized()
    if ring is not None:
        t = Vector(ring(b, i) if callable(ring) else ring).normalized()
        a = (a - t * a.dot(t)).normalized()
        n = a.cross(t) * s
    else:
        n = Vector(palm or (-s, 0, 0))
        n = (n - a * n.dot(a)).normalized()  # the palm side: fingers curl towards it
        t = n.cross(a) * s  # towards the thumb
        if t.dot(Vector((0, -1, 0))) < 0 and palm is None:
            t = -t
    rot = frame(a, t)

    def local(x, y, z):
        return wr + a * (x * L) + t * (y * L) + n * (z * L)
    parts = [blobs(f"{b.name}_Palm{i}", [(local(0.28, 0.0, 0.0), L * 0.25, (1.2, 1.0, 0.5), rot),
                                         (local(0.47, 0.0, -0.01), L * 0.21, (0.6, 1.05, 0.48), rot),
                                         (local(0.2, 0.16, 0.06), L * 0.12),
                                         (local(-0.03, 0.0, 0.0), L * 0.11, (1.0, 1.0, 0.8), rot)], skin, resolution=L * 0.035)]
    lengths = [0.4, 0.44, 0.41, 0.33]
    curls = GRIPS[grip]
    thumb = THUMBS[grip]
    finger_r = L * 0.07
    centre = None
    if bar:
        # The bar lies just past the knuckles, clear of the palm, in the hook of the fingers (in hand lengths).
        around = bar + finger_r * 0.9
        centre = (0.5 + 0.35 * 0.44 * 0.45, 0.085 + (bar + 0.001) / L)
        curls = [wrap((0.5 - (0.04 if k == 3 else 0), 0.0), [lengths[k] * f_ for f_ in (0.45, 0.3, 0.25)], centre, around / L)
                 for k in range(4)]
        # The thumb closes the hook on the wrist's side of the bar, its tip towards the first finger's.
        reach = (bar + L * 0.066) / L
        thumb = [(0.12, 0.17, 0.04), (0.29, 0.28, 0.12),
                 (centre[0] - reach * 0.98, 0.25, centre[1] + reach * 0.2),
                 (centre[0] - reach * 0.72, 0.11, centre[1] + reach * 0.7)]
    digits = []  # each finger's joints (knuckle to tip), for a hand that can close (hands.py)
    for k in range(4):
        base = local(0.5 - (0.04 if k == 3 else 0), (1.5 - k) * 0.105, 0.0)
        d = a.copy() if bar else (a + t * (1.5 - k) * 0.035).normalized()
        pts, r0 = [base], finger_r
        digits.append(dict(points=pts, heading=d.copy(), knuckle=0.5 - (0.04 if k == 3 else 0),
                           lengths=[lengths[k] * f_ for f_ in (0.45, 0.3, 0.25)], radius=r0))
        for seg, frac in enumerate((0.45, 0.3, 0.25)):
            # Each joint turns the finger towards the palm.
            d = (Matrix.Rotation(curls[k][seg], 3, d.cross(n).normalized()) @ d).normalized()
            pts.append(pts[-1] + d * L * lengths[k] * frac)
        radii = [r0, r0 * 0.95, r0 * 0.88, r0 * 0.82]
        parts.append(tube(f"{b.name}_Finger{i}{k}", pts, radii, radii, skin, sides=10, levels=0))
    pts = [local(*q) for q in thumb]
    radii = [L * 0.09, L * 0.082, L * 0.072, L * 0.064]
    parts.append(tube(f"{b.name}_Thumb{i}", pts, radii, radii, skin, sides=10, levels=0))
    # What a hand that closes needs to know of itself. A hand modelled closed round a handle is fused shut and
    # cut to fit: it stays as modelled.
    if not hasattr(b, "hands"):
        b.hands = {}
    b.hands[i] = dict(length=L, wrist=wr.copy(), along=a.copy(), thumbward=t.copy(), palm=n.copy(), fingers=digits,
                      thumb=dict(points=pts, radius=L * 0.075), closes=not bar)
    obj = fuse(f"{b.name}_Hand{i}", parts, skin, voxel=L * 0.013, smooth=6, keep=None if bar else 3500)
    if not bar:
        # A hand that closes: fusing can seal a bubble inside, and when the fingers bend it would come through
        # the skin. Only the hand's own surface is kept.
        before = len(obj.data.vertices)
        shapes.keep_largest(obj)
        if len(obj.data.vertices) != before:
            print(f"HAND {obj.name}: removed {before - len(obj.data.vertices)} points sealed inside")
    # Where it holds things: in the middle of the curled fingers.
    grasp = wr + a * L * 0.62 + n * L * 0.2
    if bar:
        grasp = local(centre[0], 0.0, centre[1])
        # Cut the bar's place out of the hand, so the fingers close on it exactly.
        import bpy
        cutter = shapes.cylinder(f"{b.name}_BarCut{i}", grasp, bar + 0.0003, L * 1.4, skin,
                                 rotation=frame(t, a) @ Matrix.Rotation(math.pi / 2, 3, 'Y'), vertices=20, bevel=0)
        cut = obj.modifiers.new("Bar", 'BOOLEAN')
        cut.operation, cut.object, cut.solver = 'DIFFERENCE', cutter, 'EXACT'
        shapes.apply_all(obj)
        bpy.data.objects.remove(cutter)
        shapes.simplify(obj, 3500)
        shapes.finish(obj, skin)
    return obj, grasp, (a, t, n)


# ---------------------------------------------------------------- the head and neck

def head(b, skin):
    """Skull, jaw, cheeks, chin, brow, ears and neck blended into one form, with the being's own nose.
    The neck grows from under the jaw down into the collar and flares into the shoulders, so the head
    is never set on a stalk. The painted face is projected onto it from the front, in the head's frame."""
    p, hr, hs, hm = b.p, b.hr, b.hs, b.hm
    ch, jw = p.get("cheeks", 0.3), p.get("jaw", 0.7)
    on = b.on_head
    items = [(on(0, 0.05, 0.1), hr, (hs.x, hs.y, hs.z * 1.05), hm),
             (on(0, -0.22, -0.42), hr * jw, (0.98 * hs.x, 0.9, 0.85 * hs.z), hm),
             (on(0, -0.78, 0.3), hr * 0.32, (1.5, 0.5, 0.45), hm),
             (on(0, -0.55, -0.82 * p.get("chin_drop", 1.0)), hr * p.get("chin", 0.28))]
    for s in (-1, 1):
        items.append((on(s * 0.47, -0.58, -0.33), hr * ch))
        items.append((on(s * 0.97, 0.05, -0.1), hr * 0.22, (0.45, 0.75, 1.15), hm))
        items.append((on(s * 1.02, 0.02, -0.08), -hr * 0.07, (0.4, 0.6, 0.9), hm))  # the ear's hollow
    for at, r, size in p["nose"]:
        items.append((on(*at), hr * r, size, hm))
    # The neck: slender at the top, rising from behind the jaw and under the ears (where the neck's muscles
    # start), widening as it goes down into the collar and the shoulders under the clothes. Starting behind
    # the jaw, not under the chin, keeps the jawline and chin readable.
    nk = hr * p.get("neck_r", 0.5)
    top = on(0, 0.3, -0.62)
    base = b.collar + Vector((0, 0.012, -0.015))
    for f, r in ((0.0, 0.8), (0.3, 0.86), (0.65, 0.95), (1.0, 1.0)):
        items.append((top.lerp(base, f), nk * r, (1.0, 0.92, 1.0)))
    # Where it widens into the shoulders it is well inside the clothes: at the neckline itself the neck is no
    # wider than the garment's opening, so skin never shows through a neckband.
    items.append((base + Vector((0, -0.004, -0.045)), nk * 1.05, (1.1, 0.8, 0.5)))
    # Under the chin, a soft hollow: the jaw's underside turns back into the neck instead of melting into it.
    items.append((on(0, -0.42, -1.02), -hr * 0.3, (1.5, 1.0, 0.7), hm))
    obj = blobs(f"{b.name}_Skin", items, skin, resolution=hr * 0.032)
    # One piece only: a hollow can leave a small bubble sealed inside the head, which no one can ever see.
    shapes.keep_largest(obj)
    span = p.get("face_span", 1.25)
    uv = obj.data.uv_layers[0]
    inverse = hm.transposed()
    for poly in obj.data.polygons:
        for li in poly.loop_indices:
            local = inverse @ (obj.data.vertices[obj.data.loops[li].vertex_index].co - b.head) / hr
            x, y, z = local.x / hs.x, local.y / hs.y, local.z / hs.z
            # Below the chin the face's own skin carries on (the texture's edge is plain skin).
            if y < 0.2:
                uv.data[li].uv = (min(max(0.5 + x / (2 * span), 0.01), 0.99), min(max(0.5 + z / (2 * span), 0.01), 0.99))
            else:
                uv.data[li].uv = (0.02, 0.02)
    return obj


# ---------------------------------------------------------------- boots

def shaft_radius(b):
    """A boot shaft's radius: just around the leg."""
    return b.p["leg"] * 1.12


def boot(b, i, leather, sole, shaft=0.16, cuff=0.0, laces="Lace", eyelets="Brass"):
    """A sturdy boot: a shoe-last foot with a low rounded toe and a shaft that follows the shin, so that leg and
    boot read as one line; fused into one form and flattened to stand; a thick sole and heel. The laces run
    through eyelets up the instep and the shaft's front, lying on the leather, and end in a bow."""
    f, an = b.foot_frame(i)
    side = Vector((0, 0, 1)).cross(f).normalized()
    L = b.p.get("foot", 0.165) * b.H  # the boot's length
    W = L * 0.17  # half its width at the ball
    ground = Vector((an.x, an.y, 0))

    def at(along, height, out=0.0):
        return ground + f * along + Vector((0, 0, height)) + side * out
    rot = frame(f, side)
    leg_r = b.p["leg"]
    # The shaft follows the shin (the knee is a little forward of the ankle, even standing): its top is on the
    # leg's own line, `shaft` above the ankle. A shaft standing straight up leaves the leg beside it.
    up = (b.knees[i] - an).normalized()
    length = shaft / max(up.z, 0.5)
    top = an + up * length
    # The last: one long rounded form from heel to toe, the toe a little narrower and lower.
    heel, toe = -L * 0.3, L * 0.58
    # The shaft hugs the leg, so the leg and the boot read as one line; the trousers fall over its top.
    shaft_r = shaft_radius(b)
    parts = [chain(f"{b.name}_BootShaft{i}", [at(-L * 0.04, 0.05), Vector((an.x, an.y, an.z + 0.02)), top - up * 0.01, top],
                   [(leg_r * 1.22, leg_r * 1.3), (shaft_r * 0.95, shaft_r), (shaft_r, shaft_r), (shaft_r, shaft_r)], leather),
             blobs(f"{b.name}_BootLast{i}", [(at((heel + toe) * 0.5, 0.05), W, ((toe - heel) * 0.5 / W, 1.0, 1.05), rot),
                                             (at(L * 0.4, 0.045), W * 0.92, (1.25, 0.98, 0.78), rot),
                                             (at(-L * 0.16, 0.06), W * 0.9, (1.15, 0.95, 1.1), rot)], leather, resolution=W * 0.07)]
    body = fuse(f"{b.name}_Boot{i}", parts, leather, voxel=W * 0.05, smooth=8)
    # Flat underneath, so it stands on its sole.
    for v in body.data.vertices:
        if v.co.z < 0.022:
            v.co.z = 0.022 + (v.co.z - 0.022) * 0.15
    shapes.simplify(body, 4000)
    out = [body]
    # The sole follows the last's outline, a little wider; a heel block under the back.
    xs = [heel - 0.006, heel + L * 0.08, L * 0.05, L * 0.25, L * 0.42, toe - L * 0.04, toe + 0.008]
    half = (toe - heel) * 0.5 + 0.008
    mid = (heel + toe) * 0.5
    widths = [W * 1.06 * math.sqrt(max(0.02, 1 - ((x - mid) / half) ** 2)) for x in xs]
    widths[0], widths[-1] = W * 0.35, W * 0.3
    out.append(tube(f"{b.name}_Sole{i}", [at(x, 0.012) for x in xs], widths, [0.012] * len(xs), sole,
                    normals=[Vector((0, 0, 1))] * len(xs), sides=12, levels=1))
    out.append(shapes.slab(f"{b.name}_Heel{i}", at(heel + L * 0.11, 0.011), (L * 0.1, W * 0.85, 0.011), sole, rotation=rot))
    if cuff:
        out.append(chain(f"{b.name}_BootCuff{i}", [top - up * 0.025, top + up * 0.008],
                         [(leg_r * 1.6, leg_r * 1.6), (leg_r * 1.55, leg_r * 1.55)], leather, levels=2))
    if laces:
        out.extend(lacing(b, i, body, an, up, length, f, side, ground, shaft_r, laces, eyelets))
    return out


def lacing(b, i, boot_obj, an, up, length, f, side, ground, shaft_r, lace, brass):
    """Laces as a cobbler threads them: pairs of eyelets up the instep and the shaft's front, the lace crossing
    between them and lying on the leather (every point is laid on the boot's real surface), tied in a bow at the
    top whose ends hang down the front. Returns the laces (one part) and the eyelets (one part)."""
    surface = shapes.Surface(boot_obj)
    r = 0.0024
    fwd = (f - up * f.dot(up)).normalized()
    lat = up.cross(fwd).normalized()
    half = 0.36  # the eyelets' half angle round the shaft

    def hole(t, sgn):
        c = an + up * (length * t)
        d = Matrix.Rotation(sgn * half, 3, up) @ fwd
        return surface.cast(c + d * 0.25, -d, 0.3)

    rows = max(3, round(length / 0.034))
    levels = [0.14 + 0.5 * k / (rows - 1) for k in range(rows)]
    holes = [(hole(t, -1), hole(t, 1)) for t in levels]
    # One pair on the instep, in front of the shaft (cast down onto the foot).
    reach = shaft_r + 0.024
    instep = [surface.cast(ground + f * reach + side * (sgn * 0.016) + Vector((0, 0, 0.4)), Vector((0, 0, -1)), 0.5) for sgn in (-1, 1)]
    holes.insert(0, tuple(instep))
    holes = [pair for pair in holes if pair[0][0] is not None and pair[1][0] is not None]

    def strand(a, c, over=False, n=7):
        pts = []
        for k in range(n):
            u = k / (n - 1)
            hump = max(0.0, 1 - abs(u - 0.5) / 0.3) if over else 0.0
            pts.append(surface.lay(a.lerp(c, u), r * (0.9 + 1.1 * hump))[0])
        return pts

    paths = [strand(holes[0][0][0], holes[0][1][0])]  # the bar across the lowest pair
    for (a0, a1), (c0, c1) in zip(holes, holes[1:]):
        paths.append(strand(a0[0], c1[0]))
        paths.append(strand(a1[0], c0[0], over=True))
    # The bow, just above the top pair: two loops lying on the leather and two ends hanging down the front.
    top_t = levels[-1] + min(0.12, 0.016 / length)
    knot, knot_n = surface.cast(an + up * (length * top_t) + fwd * 0.25, -fwd, 0.3)
    parts = []
    if knot is not None:
        paths.append(strand(holes[-1][0][0], knot, n=3))
        paths.append(strand(holes[-1][1][0], knot, n=3))
        for sgn in (-1, 1):
            loop = [knot, knot + lat * sgn * 0.011 + up * 0.007, knot + lat * sgn * 0.024 + up * 0.002,
                    knot + lat * sgn * 0.015 - up * 0.007, knot]
            paths.append([surface.lay(q, r * 0.9)[0] for q in loop])
            end = [knot, knot + lat * sgn * 0.007 - up * 0.014, knot + lat * sgn * 0.012 - up * 0.03]
            paths.append([surface.lay(q, r * 0.9)[0] for q in end])
        parts.append(shapes.ellipsoid(f"{b.name}_LaceKnot{i}", knot + knot_n * r, (0.0052, 0.0052, 0.0042), lace,
                                      rotation=frame(lat, up), segments=8))
    for k, pts in enumerate(paths):
        parts.append(tube(f"{b.name}_LaceStrand{i}_{k}", pts, [r] * len(pts), [r] * len(pts), lace, sides=4, levels=0))
    laces = shapes.exact(shapes.join(f"{b.name}_Lace{i}", parts), fine=True)
    rings = []
    for k, pair in enumerate(holes):
        for j, (at_, nm) in enumerate(pair):
            turn = frame(nm, up) @ Matrix.Rotation(math.pi / 2, 3, 'Y')
            rings.append(shapes.cylinder(f"{b.name}_EyeletRing{i}_{k}{j}", at_ + nm * 0.0006, 0.0048, 0.0018, brass,
                                         rotation=turn, vertices=8, bevel=0))
    eyes = shapes.exact(shapes.join(f"{b.name}_Eyelet{i}", rings), fine=True)
    return [laces, eyes]
