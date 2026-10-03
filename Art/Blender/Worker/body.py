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
}


def hand(b, i, skin, grip="relaxed", palm=None, along=None, ring=None):
    """A stylized working hand: a full palm, four chunky fingers of three joints curled by the grip, and a
    thumb that wraps what the hand holds. palm: the direction the palm faces; along: the direction the
    fingers leave the wrist; ring: the direction a held handle runs through the curled fingers."""
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
    for k in range(4):
        base = local(0.5 - (0.04 if k == 3 else 0), (1.5 - k) * 0.105, 0.0)
        d = (a + t * (1.5 - k) * 0.035).normalized()
        pts, r0 = [base], L * 0.07
        for seg, frac in enumerate((0.45, 0.3, 0.25)):
            # Each joint turns the finger towards the palm.
            d = (Matrix.Rotation(curls[k][seg], 3, d.cross(n).normalized()) @ d).normalized()
            pts.append(pts[-1] + d * L * lengths[k] * frac)
        radii = [r0, r0 * 0.95, r0 * 0.88, r0 * 0.82]
        parts.append(tube(f"{b.name}_Finger{i}{k}", pts, radii, radii, skin, sides=10, levels=0))
    pts = [local(*q) for q in THUMBS[grip]]
    radii = [L * 0.09, L * 0.082, L * 0.072, L * 0.064]
    parts.append(tube(f"{b.name}_Thumb{i}", pts, radii, radii, skin, sides=10, levels=0))
    obj = fuse(f"{b.name}_Hand{i}", parts, skin, voxel=L * 0.013, smooth=6, keep=3500)
    # Where it holds things: in the middle of the curled fingers.
    grasp = wr + a * L * 0.62 + n * L * 0.2
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
    # The neck: a column from behind the jaw into the collar, widening into the shoulders under the clothes.
    nk = hr * p.get("neck_r", 0.5)
    top = on(0, 0.12, -0.62)
    base = b.collar + Vector((0, 0.008, -0.02))
    for f, r in ((0.0, 0.92), (0.35, 0.95), (0.7, 1.0), (1.0, 1.06)):
        items.append((top.lerp(base, f), nk * r, (1.0, 0.95, 1.0)))
    items.append((base + Vector((0, 0.006, -0.02)), nk * 1.12, (1.15, 0.95, 0.6)))
    obj = blobs(f"{b.name}_Skin", items, skin, resolution=hr * 0.032)
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

def boot(b, i, leather, sole, shaft=0.16, cuff=0.0, laces="Lace"):
    """A sturdy boot: a shoe-last foot with a low rounded toe and a shaft wide enough for the trousers to
    tuck into, fused into one form and flattened to stand; a thick sole and heel; laces crossing up the front."""
    f, an = b.foot_frame(i)
    side = Vector((0, 0, 1)).cross(f).normalized()
    L = b.p.get("foot", 0.165) * b.H  # the boot's length
    W = L * 0.19  # half its width at the ball
    ground = Vector((an.x, an.y, 0))

    def at(along, up, out=0.0):
        return ground + f * along + Vector((0, 0, up)) + side * out
    rot = frame(f, side)
    leg_r = b.p["leg"]
    top = Vector((an.x, an.y, an.z + shaft))
    # The last: one long rounded form from heel to toe, the toe a little narrower and lower.
    heel, toe = -L * 0.3, L * 0.58
    parts = [chain(f"{b.name}_BootShaft{i}", [at(-L * 0.04, 0.05), Vector((an.x, an.y, an.z + 0.02)), top + Vector((0, 0, -0.01)), top],
                   [(leg_r * 1.25, leg_r * 1.35), (leg_r * 1.2, leg_r * 1.25), (leg_r * 1.38, leg_r * 1.38), (leg_r * 1.45, leg_r * 1.45)], leather),
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
        out.append(chain(f"{b.name}_BootCuff{i}", [top + Vector((0, 0, -0.025)), top + Vector((0, 0, 0.008))],
                         [(leg_r * 1.6, leg_r * 1.6), (leg_r * 1.55, leg_r * 1.55)], leather, levels=2))
    if laces:
        # Crossing laces up the front of the shaft and over the instep.
        for k in range(4):
            z = 0.075 + k * (shaft + an.z - 0.11) / 4
            reach = leg_r * (1.3 if z > an.z else 1.45)
            c = Vector((an.x, an.y, z)) + f * (reach + 0.002 + (0.012 if z < an.z else 0))
            for sgn in (-1, 1):
                lift = Vector((0, 0, 0.008 * sgn))
                out.append(tube(f"{b.name}_Lace{i}{k}{sgn}", [c - side * reach * 0.5 - lift, c, c + side * reach * 0.5 + lift],
                                [0.0028] * 3, [0.0028] * 3, laces, sides=6, levels=0))
    return out
