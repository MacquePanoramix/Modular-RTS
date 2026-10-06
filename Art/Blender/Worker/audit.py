"""Wonder Gather — the model audit (S1d): passes 3 and 4 of the model quality method
(Docs/ArtDirection/ModelQualityMethod.md).

It measures a being's parts as they will be exported (skinned and simplified, before they are joined), at
rest and on every frame of the game's own movement:

- **Floating.** Every connected piece of every part must lie within reach of another part.
- **Lies on.** Every point of a part must lie on another: laces on their boot, ties on the smock.
- **Meets.** A part must touch another somewhere: a handle and the hand, a strap and its rings.
- **Apart.** No point of a part may sink into another (a solid) deeper than a tolerance: a hand into the coat,
  a lantern into the legs, a bag into the hip.
- **Covered.** What is inside another at rest stays inside in every frame: a boot's shaft inside the trousers.
- **Beneath.** What a skirt covers never comes out through it: trousers under a coat's skirt (seen along
  rays from the trunk's axis).
- **Contacts.** Points the modules declare (b.contacts) must stay on what they rest on.
- **Technical.** At most four bones a vertex; solids not inside out; soles on the ground.

The pose sweep poses the parts exactly as skinning does in the game, from frames recorded there
(Tests/PlayMode/MinerPoseRecord.cs): each bone's matrix carries the rest pose to the frame's pose in the
mesh's space; a fit on the bones' rest positions maps the game's mesh space to Blender's.
"""
import json
import math
import os
import re

import bpy
import numpy as np
from mathutils import Vector
from mathutils.bvhtree import BVHTree

# Distances in metres.
FLOAT_REACH = 0.003  # a piece further than this from everything is floating

# Patterns are regular expressions on a part's own name (after the being's prefix); \1 refers back to the first
# part's group. Virtual parts: Top.Body and Top.Arm.L/R (the top's torso and sleeves), Trousers.L/R (its legs).
# (part, others, tolerance): every point of the part within tolerance of the others.
LIES = [
    # A lace crossing over another stands two laces high; an eyelet may lift a little where the ankle creases.
    (r"Lace(\d)", r"Boot\1", 0.0095),
    (r"Eyelet(\d)", r"Boot\1", 0.0070),
    (r"ApronTie\d", r"Top\.Body|Skirt|Apron", 0.0100),
    # A patch or pocket: its outer face is its own thickness from the cloth, and no more.
    (r"ApronPocket", r"Apron", 0.0110),
    (r"Patch0", r"Top", 0.0075),
    # A neckband sits on its garment's neckline all the way round (its far side is its own thickness away).
    (r"Neckband", r"Top", 0.024),
    (r"Patch1", r"Skirt", 0.009),  # a patch's outer face is 5 mm off the cloth it is sewn on
]
# (part, others, tolerance): some point of the part within tolerance of the others.
MEETS = [
    (r"LanternHandle", r"Hand\d", 0.0015),
    (r"LanternBail(\d)", r"LanternHandle", 0.0015),
    (r"LanternBail(\d)", r"LanternEar\1", 0.0015),
    (r"LanternEar\d", r"LanternCap", 0.0015),
    (r"MugHandle", r"Hand\d", 0.0015),
    (r"MugArm\d", r"MugHandle", 0.0015),
    (r"MugArm\d", r"Mug", 0.0015),
    (r"BagLoop\d", r"Bag", 0.001),
    (r"Strap", r"BagRing0", 0.0015),
    (r"Strap", r"BagRing1", 0.0015),
    (r"BagRing(\d)", r"BagLoop\1", 0.001),
    (r"StrapTab\d", r"Strap", 0.0015),
    (r"BagTongue", r"BagFlap", 0.0015),
    (r"Buckle", r"BagTongue|Bag", 0.002),
    (r"Bag", r"Skirt|Top\.Body", 0.004),
    (r"BagFlap", r"Bag", 0.0015),
    (r"Button\d", r"Top|Skirt", 0.0012),
    (r"PickHandle", r"PickLoop0", 0.002),
    (r"PickHandle", r"PickLoop1", 0.002),
    (r"PickLoop\d", r"PickStrap", 0.0015),
    (r"PickCollar", r"PickLoop1", 0.003),
    (r"PickStrap", r"Top|Collar", 0.0045),
    (r"CapLampMount", r"Cap", 0.0012),
    (r"CapLamp", r"CapLampMount", 0.0012),
    (r"HammerHandle", r"HammerLoop", 0.002),
    (r"HammerHead", r"HammerLoop|HammerHandle", 0.002),
    (r"HammerLoop", r"Apron", 0.0015),
    (r"ApronStrap", r"Apron", 0.002),
    (r"ApronTie\d", r"Apron", 0.002),
    (r"ApronTie\d", r"ApronKnot", 0.002),
    (r"ApronTieEnd\d", r"ApronKnot", 0.002),
    (r"Apron", r"Top\.Body|Skirt", 0.006),
]
# (part, solids, tolerance): no point of the part deeper than tolerance inside the solids.
APART = [
    (r"Hand\d", r"Top\.Body|Skirt|Apron|Bag|Pick(Handle|Head)|Hammer(Handle|Head)", 0.003),
    (r"Lantern\w*", r"Trousers|Skirt|Top\.Body|Boot\d|Bag", 0.002),
    (r"Mug\w*", r"Top\.Body|Skirt|Trousers", 0.002),
    (r"Mug|MugArm\d|MugHandle|Lantern\w*", r"Hand\d", 0.0015),
    (r"Bag\w*|Buckle\w*|StrapTab\d", r"Top\.Body|Top\.Arm\.[LR]|Trousers", 0.002),
    # A bag rests on the coat: its back presses a little into the soft cloth under it as the cloth moves.
    (r"Bag\w*|Buckle\w*|StrapTab\d", r"Skirt", 0.006),
    (r"Strap|PickStrap", r"Top|Skirt", 0.002),
    # A flat strap bridging a soft collar's rim may press a little into it.
    (r"Strap|PickStrap", r"Collar", 0.003),
    (r"Pick(Handle|Head|Collar)", r"Top|Skirt|Skin|Collar", 0.002),
    (r"Hair", r"PickHead", 0.002),
    # Leather lying on cloth, and a tool resting on leather, press a little into what yields under them.
    (r"Apron", r"Top\.Body|Skirt", 0.0035),
    (r"ApronStrap", r"Top|Neckband|Skin", 0.002),
    (r"Hammer(Handle|Head)", r"Apron|Skirt|Trousers", 0.0035),
    (r"Boot0|Sole0|Heel0", r"Boot1|Trousers\.L", 0.002),
    (r"Boot1|Sole1|Heel1", r"Trousers\.R", 0.002),
    (r"Trousers\.L", r"Trousers\.R", 0.003),
    (r"Hair", r"Collar|Top\.Body|Neckband", 0.003),
    (r"Cap\w*", r"Skin", 0.002),
]
# (part, solid, tolerance): points of the part inside the solid at rest stay inside, never further out than tolerance.
COVERED = [
    (r"Boot\d", r"Trousers", 0.002),
]
# (part, skirt, tolerance): no point of the part outside the skirt along a ray from the trunk's axis.
BENEATH = [
    (r"Trousers", r"Skirt", 0.003),
]
# Solids: closed surfaces whose inside can be told from their normals.
SOLIDS = r"Top(\.Body|\.Arm\.[LR])?|Skirt|Trousers(\.[LR])?|Boot\d|Bag|Skin|Apron|Collar|Cap|Hand\d|Pick(Handle|Head)|Hammer(Handle|Head)|WorkPick(Handle|Head|Collar)"
# At work, with the tool the game put in the hands (its parts are named Work...; tools.py). The tool passes through
# nothing: not the body, the clothes, the head, nor what else is carried. The fingers that hold it do not enter its
# handle, and each hand that holds it touches it.
WORK_APART = [
    (r"WorkPick(Handle|Head|Collar)", r"Top|Skirt|Skin|Collar|Cap|Trousers|Apron|Bag|Boot\d|Pick(Handle|Head)|Hammer(Handle|Head)", 0.003),
    (r"Hair|Neckband|Lantern\w*|Mug\w*|Strap|PickStrap|ApronStrap", r"WorkPick(Handle|Head)", 0.003),
    (r"Hand\d", r"WorkPickHandle", 0.0015),
]
WORK_MEETS = [
    (r"Hand0", r"WorkPickHandle", 0.002),
]


class Part:
    """One part: its rest vertices, polygons, bone weights and connected pieces."""

    def __init__(self, obj, prefix, bone_index):
        self.obj = obj
        self.virtual = False
        self.name = obj.name[len(prefix):] if obj.name.startswith(prefix) else obj.name
        mesh = obj.data
        mw = obj.matrix_world
        self.rest = np.array([tuple(mw @ v.co) for v in mesh.vertices], dtype=np.float64)
        self.polys = [tuple(p.vertices) for p in mesh.polygons]
        n, nb = len(mesh.vertices), len(bone_index)
        self.weights = np.zeros((n, nb), dtype=np.float64)
        groups = {g.index: g.name for g in obj.vertex_groups}
        self.max_influences = 0
        for v in mesh.vertices:
            ws = [(g.weight, bone_index[groups[g.group]]) for g in v.groups if groups.get(g.group) in bone_index and g.weight > 0]
            ws.sort(reverse=True)
            self.max_influences = max(self.max_influences, len(ws))
            ws = ws[:4]
            total = sum(w for w, _ in ws)
            for w, b in ws:
                self.weights[v.index, b] = w / total if total > 0 else 0
        self.pieces = pieces(n, self.polys)
        self.piece_polys = split_polys(self.pieces, self.polys)

    def sub(self, name, keep_poly):
        """A virtual part made of some of this part's polygons (for example one leg of the trousers)."""
        p = object.__new__(Part)
        p.obj, p.name, p.virtual = self.obj, name, True
        used = sorted({i for poly, k in zip(self.polys, keep_poly) if k for i in poly})
        remap = {old: new for new, old in enumerate(used)}
        p.rest = self.rest[used]
        p.weights = self.weights[used]
        p.polys = [tuple(remap[i] for i in poly) for poly, k in zip(self.polys, keep_poly) if k]
        p.max_influences = self.max_influences
        p.pieces = pieces(len(used), p.polys)
        p.piece_polys = split_polys(p.pieces, p.polys)
        return p


def pieces(n, polys):
    """Connected pieces of a mesh, as lists of vertex indices."""
    parent = list(range(n))

    def root(i):
        while parent[i] != i:
            parent[i] = parent[parent[i]]
            i = parent[i]
        return i
    for poly in polys:
        r0 = root(poly[0])
        for i in poly[1:]:
            ri = root(i)
            if ri != r0:
                parent[ri] = r0
    groups = {}
    for i in range(n):
        groups.setdefault(root(i), []).append(i)
    return [g for g in groups.values() if len(g) >= 3]


def split_polys(piece_list, polys):
    """Each piece's polygons."""
    owner = {}
    for k, piece in enumerate(piece_list):
        for i in piece:
            owner[i] = k
    out = [[] for _ in piece_list]
    for poly in polys:
        k = owner.get(poly[0])
        if k is not None:
            out[k].append(poly)
    return out


def tree(verts, polys):
    return BVHTree.FromPolygons(verts.tolist(), polys, all_triangles=False, epsilon=0.0)


def signed_volume(verts, polys):
    v = 0.0
    for poly in polys:
        a = verts[poly[0]]
        for k in range(1, len(poly) - 1):
            v += np.dot(a, np.cross(verts[poly[k]], verts[poly[k + 1]])) / 6.0
    return v


# ---------------------------------------------------------------- posing from the game's frames

def load_poses(path, bones):
    """Frames recorded in the game, as Blender-space matrices per bone (name -> 4x4), with labels."""
    with open(path, encoding="utf-8") as f:
        data = json.load(f)
    names = data["bones"]

    def m4(values):
        m = np.eye(4)
        m[:3, :4] = np.array(values, dtype=np.float64).reshape(3, 4)
        return m
    rest_u = [m4(r) for r in data["rest"]]
    # Fit the game's mesh space to Blender's on the bones' rest heads (an affine map; it carries the axis change).
    src, dst = [], []
    for n, m in zip(names, rest_u):
        head = (0.0, 0.0, 0.0) if n == "Root" else (tuple(bones[n][0]) if n in bones else None)
        if head is None:
            continue
        src.append(list(m[:3, 3]) + [1.0])
        dst.append(head)
    src, dst = np.array(src), np.array(dst)
    fit, *_ = np.linalg.lstsq(src, dst, rcond=None)
    residual = float(np.abs(src @ fit - dst).max())
    a = np.eye(4)
    a[:3, :3] = fit[:3].T
    a[:3, 3] = fit[3]
    ai = np.linalg.inv(a)
    frames = [(fr["label"], {n: a @ m4(m) @ ai for n, m in zip(names, fr["m"])}) for fr in data["frames"]]
    # A tool in the hands: its own matrix (from the tool's space in the game to the mesh's). The tool's parts
    # are built in Blender with the handle up z and the strike towards -y; in the game its space has y up the
    # handle and z the way it strikes.
    to_game = np.array([[-1.0, 0, 0, 0], [0, 0, 1.0, 0], [0, -1.0, 0, 0], [0, 0, 0, 1.0]])
    for (_, mats), fr in zip(frames, data["frames"]):
        if "t" in fr:
            mats["Tool"] = a @ m4(fr["t"]) @ to_game
    # A bone the recording does not have (made since it was recorded) goes with its parent.
    for _, mats in frames:
        for n in bones:
            parent = n
            while parent is not None and parent not in mats:
                parent = bones[parent][3] if parent in bones else None
            if n not in mats and parent is not None:
                mats[n] = mats[parent]
    return frames, residual


def pose(part, mats, bone_names):
    """A part's vertices in a frame: linear blend skinning, as the game does it."""
    out = np.zeros_like(part.rest)
    for b, n in enumerate(bone_names):
        w = part.weights[:, b]
        if not w.any():
            continue
        m = mats.get(n, np.eye(4))
        out += w[:, None] * (part.rest @ m[:3, :3].T + m[:3, 3])
    return out


# ---------------------------------------------------------------- the checks

class Report:
    def __init__(self, name):
        self.name = name
        self.rows = {}

    def add(self, check, parts, value, limit, where, frame):
        key = (check, parts)
        failed = bool(value > limit)
        row = self.rows.get(key)
        if row is None:
            row = self.rows[key] = dict(check=check, parts=parts, limit=limit, rest=None, value=-1.0, where=None, frame=None,
                                        failed=False, frames=0, frames_failed=0, phases={})
        if frame == "rest":
            row["rest"] = round(float(value), 4)
        else:
            row["frames"] += 1
            row["frames_failed"] += failed
            phase = frame.rstrip("0123456789")
            row["phases"][phase] = max(row["phases"].get(phase, 0.0), round(float(value), 4))
        if value > row["value"]:
            row["value"], row["frame"] = round(float(value), 4), frame
            row["where"] = [round(float(x), 3) for x in where] if where is not None else None
        row["failed"] = row["failed"] or failed

    def failures(self):
        return [r for r in self.rows.values() if r["failed"]]

    def text(self):
        fails = self.failures()
        lines = [f"AUDIT {self.name}: {len(fails)} failing of {len(self.rows)} checks (distances in mm)"]
        for r in sorted(self.rows.values(), key=lambda r: (not r["failed"], r["check"], r["parts"])):
            mark = "FAIL" if r["failed"] else "ok  "
            rest = f"rest {r['rest'] * 1000:6.1f}" if r["rest"] is not None else "rest      -"
            motion = f"   worst {r['value'] * 1000:6.1f} at {r['frame']:<9} ({r['frames_failed']}/{r['frames']} frames)" if r["frames"] else ""
            phases = "  " + " ".join(f"{k} {v * 1000:.1f}" for k, v in r["phases"].items()) if r["failed"] and r["phases"] else ""
            lines.append(f"  {mark} {r['check']:<9} {r['parts']:<42} limit {r['limit'] * 1000:4.1f}  {rest}{motion}{phases}  {r['where']}")
        return "\n".join(lines)


def match(parts, pattern, ref=None):
    if ref is not None:
        pattern = pattern.replace("\\1", ref)
    rx = re.compile(f"^(?:{pattern})$")
    return [p for p in parts if rx.match(p.name)]


def inside(t, point, direction):
    """Whether a point is inside a closed surface: a ray from it crosses the surface an odd number of times."""
    d = Vector(direction).normalized()
    p = Vector(point)
    crossings = 0
    for _ in range(40):
        hit = t.ray_cast(p, d, 10.0)
        if hit[0] is None:
            break
        crossings += 1
        p = hit[0] + d * 1e-5
    return crossings % 2 == 1


def nearest(t, point, reach):
    hit = t.find_nearest(Vector(point), reach)
    return hit if hit[0] is not None else None


class Frame:
    """The parts posed in one frame, with their BVH trees made when first needed."""

    def __init__(self, parts, verts, mats=None):
        self.parts = parts
        self.verts = verts
        self.mats = mats
        self.trees = {}
        self.flip = {}
        self.open = {}

    def tree(self, p):
        t = self.trees.get(p.name)
        if t is None:
            t = self.trees[p.name] = tree(self.verts[p.name], p.polys)
        return t

    def piece_tree(self, p, k):
        """One connected piece of a part."""
        key = (p.name, k)
        t = self.trees.get(key)
        if t is None:
            t = self.trees[key] = tree(self.verts[p.name], p.piece_polys[k])
        return t

    def signed(self, solid, co, reach=0.05):
        """Signed distance from a solid's surface (negative inside), or None beyond reach. The side is told from
        the nearest face's normal; a point that seems inside is confirmed by counting the surfaces a ray from it
        crosses (an odd count is inside), since a thin curled shell (a collar, a skirt) can mislead the normal."""
        t = self.tree(solid)
        h = nearest(t, co, reach)
        if h is None:
            return None
        s = float(np.dot(co - np.array(h[0]), np.array(h[1]))) * self.flip.get(solid.name, 1.0)
        if s < -0.0005 and not (inside(t, co, (0.31, 0.52, 0.79)) and inside(t, co, (-0.57, 0.33, -0.75))):
            return abs(s)
        return s


def trunk_axis(fr, bones):
    """The trunk's axis in a frame: from the hips' middle up through the waist, posed by the pelvis bone."""
    h, t = np.array(bones["Pelvis"][0], dtype=np.float64), np.array(bones["Pelvis"][1], dtype=np.float64)
    if fr.mats is not None and "Pelvis" in fr.mats:
        m = fr.mats["Pelvis"]
        h, t = m[:3, :3] @ h + m[:3, 3], m[:3, :3] @ t + m[:3, 3]
    return h, t


def side_of_surface(fr, part, co, reach=0.1):
    """Signed distance from a part's nearest face, by that face's normal alone (for one leg of the trousers, which
    is open where it was cut from the other, so crossings cannot be counted): negative on its inner side."""
    h = nearest(fr.tree(part), co, reach)
    if h is None:
        return None
    return float(np.dot(co - np.array(h[0]), np.array(h[1]))) * fr.flip.get(part.name, 1.0)


def leg_lines(fr, bones):
    """Each leg's line in a frame: hip, knee, ankle (the heads of its thigh, shin and foot bones), posed."""
    out = {}
    for s in ("L", "R"):
        pts = []
        for n in (f"Thigh.{s}", f"Shin.{s}", f"Foot.{s}"):
            if n not in bones:
                break
            p = np.array(bones[n][0], dtype=np.float64)
            if fr.mats is not None and n in fr.mats:
                m = fr.mats[n]
                p = m[:3, :3] @ p + m[:3, 3]
            pts.append(p)
        if len(pts) == 3:
            out[s] = pts
    return out or None


def crosses(t, line, co):
    """Whether the way from the hip down a leg's line to the point nearest co, and from there out to co, crosses
    a surface."""
    hip, knee, ankle = line
    best = None
    for k, (p, q) in enumerate(((hip, knee), (knee, ankle))):
        d = q - p
        u = float(np.clip(np.dot(co - p, d) / max(float(np.dot(d, d)), 1e-9), 0.0, 1.0))
        on = p + d * u
        gap = float(np.linalg.norm(co - on))
        if best is None or gap < best[0]:
            best = (gap, k, on)
    _, k, on = best
    path = [hip, on, co] if k == 0 else [hip, knee, on, co]
    for p, q in zip(path, path[1:]):
        d = q - p
        length = float(np.linalg.norm(d))
        if length < 1e-6:
            continue
        if t.ray_cast(Vector(p), Vector(d / length), length)[0] is not None:
            return True
    return False


def front_of(fr, bones):
    """The way the being faces in a frame (it faces -y at rest), turned with the pelvis."""
    f = np.array((0.0, -1.0, 0.0))
    if fr.mats is not None and "Pelvis" in fr.mats:
        f = fr.mats["Pelvis"][:3, :3] @ f
    return f / max(np.linalg.norm(f), 1e-6)


def check_frame(report, fr, label, parts, contacts, covered, bones, floating):
    names = {p.name: p for p in parts}
    for pa, pb, tol in LIES:
        for a in parts:
            m = re.match(f"^(?:{pa})$", a.name)
            if not m:
                continue
            ref = next((g for g in m.groups() if g is not None), None) if m.groups() else None
            others = match(parts, pb, ref)
            if not others:
                continue
            worst, where = 0.0, None
            for co in fr.verts[a.name]:
                d = min((h[3] for h in (nearest(fr.tree(o), co, 0.2) for o in others) if h), default=0.2)
                if d > worst:
                    worst, where = d, co
            report.add("lies on", f"{a.name} on {'|'.join(o.name for o in others)}", worst, tol, where, label)
    for pa, pb, tol in MEETS:
        for a in parts:
            m = re.match(f"^(?:{pa})$", a.name)
            if not m:
                continue
            ref = next((g for g in m.groups() if g is not None), None) if m.groups() else None
            others = [o for o in match(parts, pb, ref) if o is not a]
            if not others:
                continue
            best, where = 1.0, None
            if any(fr.tree(a).overlap(fr.tree(o)) for o in others):
                best, where = 0.0, fr.verts[a.name][0]  # their surfaces cross: they touch
            else:
                for co in fr.verts[a.name]:
                    for o in others:
                        h = nearest(fr.tree(o), co, best)
                        if h and h[3] < best:
                            best, where = h[3], co
                # A flat piece lying on another may have no point of its own near it: measure the other way too.
                for o in others:
                    for co in fr.verts[o.name][::max(1, len(fr.verts[o.name]) // 400)]:
                        h = nearest(fr.tree(a), co, best)
                        if h and h[3] < best:
                            best, where = h[3], co
            report.add("meets", f"{a.name} with {'|'.join(o.name for o in others)}", best, tol, where, label)
    for pa, pb, tol in APART:
        for a in match(parts, pa):
            for b in match(parts, pb):
                if a is b or a.obj is b.obj:
                    continue
                deepest, where = 0.0, None
                for co in fr.verts[a.name]:
                    s = fr.signed(b, co)
                    if s is not None and -s > deepest:
                        deepest, where = -s, co
                report.add("sinks", f"{a.name} into {b.name}", deepest, tol, where, label)
    for (a_name, b_name, tol), inside in covered.items():
        a, b = names[a_name], names[b_name]
        # Out through the side of what covers it, not past its hidden end: a trouser leg is closed at its hem,
        # inside the boot, and the shaft's top sliding past that hidden end shows nothing. The leg's own line
        # is its shin bone's, in this frame.
        shin = "Shin.R" if a.name.endswith("0") else "Shin.L"
        line = np.array(bones[shin][1], dtype=np.float64) - np.array(bones[shin][0], dtype=np.float64)
        if fr.mats is not None and shin in fr.mats:
            line = fr.mats[shin][:3, :3] @ line
        line = line / max(np.linalg.norm(line), 1e-6)
        worst, where = 0.0, None
        for i in inside:
            co = fr.verts[a.name][i]
            hit = nearest(fr.tree(b), co, 0.1)
            if hit is None or abs(float(np.dot(np.array(hit[1]), line))) > 0.6:
                continue
            s = side_of_surface(fr, b, co)
            if s is not None and s > worst:
                worst, where = s, co
        report.add("covered", f"{a.name} in {b.name}", worst, tol, where, label)
    h, t = trunk_axis(fr, bones)
    up = (t - h) / max(np.linalg.norm(t - h), 1e-6)
    for pa, pb, tol in BENEATH:
        for a in match(parts, pa):
            for b in match(parts, pb):
                if a is b or a.obj is b.obj:
                    continue
                tb = fr.tree(b)
                # The skirt's own reach along the axis: only what is level with it can be beneath it. A coat
                # open at the front lets the legs out through the opening: that front is left out.
                along = (fr.verts[b.name] - h) @ up
                low, high = float(along.min()), float(along.max())
                front = front_of(fr, bones)
                middle = h + up * ((low + high) * 0.5)
                if b.name not in fr.open:
                    fr.open[b.name] = tb.ray_cast(Vector(middle), Vector(front), 1.0)[0] is None
                opened = fr.open[b.name]
                legs = leg_lines(fr, bones) if a.name == "Trousers" else None
                side = None
                if legs is not None:
                    names = list(bones.keys())
                    left = a.weights[:, [names.index(n) for n in ("Thigh.L", "Shin.L", "Foot.L") if n in names]].sum(axis=1)
                    right = a.weights[:, [names.index(n) for n in ("Thigh.R", "Shin.R", "Foot.R") if n in names]].sum(axis=1)
                    side = np.where(left >= right, "L", "R")
                worst, where = 0.0, None
                for i, co in enumerate(fr.verts[a.name]):
                    level = float(np.dot(co - h, up))
                    if level < low or level > high:
                        continue
                    c = h + up * level
                    d = co - c
                    dist = float(np.linalg.norm(d))
                    if dist < 1e-4:
                        continue
                    if opened and float(np.dot(d / dist, front)) > 0.75:
                        continue
                    hit = tb.ray_cast(Vector(c), Vector(d / dist), dist + 0.2)
                    if hit[0] is None:
                        continue
                    out = dist - hit[3]
                    if out <= worst:
                        continue
                    # A leg that comes out under a lifted hem is outside the skirt, and not through it: it is
                    # through the cloth only if the way to it down its own leg crosses the cloth.
                    if legs is not None and not crosses(tb, legs[side[i]], co):
                        continue
                    worst, where = out, co
                report.add("beneath", f"{a.name} under {b.name}", worst, tol, where, label)
    for c in contacts:
        a = names.get(c["part"])
        others = [names[o] for o in c["onto"] if o in names]
        if a is None or not others:
            continue
        co = fr.verts[a.name][c["vertex"]]
        d = min((h_[3] for h_ in (nearest(fr.tree(o), co, 0.2) for o in others) if h_), default=0.2)
        report.add("contact", c["label"], d, c["tol"], co, label)
    if floating:
        check_floating(report, fr, label, parts)


def check_floating(report, fr, label, parts):
    """Every connected piece of every part touches something: another part, or another piece of its own part.
    Touching: their surfaces cross, or come within reach. A piece wholly inside a solid (a curl under a cap)
    is hidden, not floating."""
    real = [p for p in parts if not p.virtual]
    solids = [p for p in real if re.match(f"^(?:{SOLIDS})$", p.name)]
    for a in real:
        many = len(a.pieces) > 1
        for k, piece in enumerate(a.pieces):
            mine = fr.piece_tree(a, k) if many else fr.tree(a)
            others = [fr.tree(o) for o in real if o is not a]
            if many:
                others += [fr.piece_tree(a, j) for j in range(len(a.pieces)) if j != k]
            verts = fr.verts[a.name][piece]
            best, where = 1.0, verts[0]
            if any(mine.overlap(o) for o in others):
                best = 0.0
            else:
                for sweep in (verts[::max(1, len(verts) // 60)], verts):
                    for co in sweep:
                        for o in others:
                            h = nearest(o, co, best)
                            if h and h[3] < best:
                                best, where = h[3], co
                        if best <= FLOAT_REACH:
                            break
                    if best <= FLOAT_REACH:
                        break
                if best > FLOAT_REACH:
                    probe = verts[::max(1, len(verts) // 12)]
                    for s in solids:
                        if s is a:
                            continue
                        if all((lambda d: d is not None and d < 0)(fr.signed(s, co, 0.2)) for co in probe):
                            best = 0.0  # hidden inside a solid
                            break
                    if best > FLOAT_REACH and many:
                        # Sealed inside another piece of its own part (a bubble in a head): hidden too.
                        for j in range(len(a.pieces)):
                            if j != k and len(a.pieces[j]) > len(piece) and all(
                                    inside(fr.piece_tree(a, j), co, (0.31, 0.52, 0.79)) for co in probe):
                                best = 0.0
                                break
            report.add("floating", f"{a.name}#{k}" if many else a.name, best, FLOAT_REACH, where, label)


# ---------------------------------------------------------------- running it

def split(parts, bone_index):
    """Virtual parts: the trousers' two legs, and the top's torso and sleeves."""
    def side_sum(p, names):
        return p.weights[:, [bone_index[n] for n in names if n in bone_index]].sum(axis=1)
    for p in list(parts):
        if p.name == "Trousers":
            side = side_sum(p, ("Thigh.L", "Shin.L", "Foot.L")) - side_sum(p, ("Thigh.R", "Shin.R", "Foot.R"))
            parts.append(p.sub("Trousers.L", [side[list(q)].mean() > 0.3 for q in p.polys]))
            parts.append(p.sub("Trousers.R", [side[list(q)].mean() < -0.3 for q in p.polys]))
        if p.name == "Top":
            arm_l = side_sum(p, ("UpperArm.L", "Forearm.L", "Hand.L"))
            arm_r = side_sum(p, ("UpperArm.R", "Forearm.R", "Hand.R"))
            parts.append(p.sub("Top.Body", [(arm_l + arm_r)[list(q)].mean() < 0.35 for q in p.polys]))
            parts.append(p.sub("Top.Arm.L", [arm_l[list(q)].mean() >= 0.35 for q in p.polys]))
            parts.append(p.sub("Top.Arm.R", [arm_r[list(q)].mean() >= 0.35 for q in p.polys]))


def run(b, meshes, bones, out_dir, poses=None, every=2, snap=True, tool=None):
    """Audits one being's parts at rest and, given recorded frames, in motion. Writes audit_<name>.txt/.json.
    tool: the parts of the tool the game puts in its hands at work (named <being>_Work..., each wholly on a
    vertex group "Tool"). They join the audit on the frames that carry the tool's place (the mining)."""
    prefix = b.name + "_"
    bone_names = list(bones.keys()) + ["Root", "Tool"]
    bone_index = {n: i for i, n in enumerate(bone_names)}
    parts = [Part(o, prefix, bone_index) for o in meshes if o.type == 'MESH' and len(o.data.polygons)]
    split(parts, bone_index)
    working = [Part(o, prefix, bone_index) for o in (tool or []) if o.type == 'MESH' and len(o.data.polygons)]
    name = b.name.split("_")[-1]
    report = Report(name)
    rest = Frame(parts, {p.name: p.rest for p in parts})
    for p in parts:
        if not p.virtual:
            report.add("bones", p.name, p.max_influences / 1000.0, 0.004, None, "rest")
        if re.match(f"^(?:{SOLIDS})$", p.name) and not p.virtual:
            vol = signed_volume(p.rest, p.polys)
            report.add("inside out", p.name, 0.0 if vol >= 0 else 1.0, 0.0, None, "rest")
            rest.flip[p.name] = 1.0 if vol >= 0 else -1.0
    for p in parts:
        if p.virtual and p.name.split(".")[0] in rest.flip:
            rest.flip[p.name] = rest.flip[p.name.split(".")[0]]
    for p in working:
        rest.flip[p.name] = 1.0 if signed_volume(p.rest, p.polys) >= 0 else -1.0
    for p in match(parts, r"Sole\d"):
        report.add("grounded", p.name, abs(float(p.rest[:, 2].min())), 0.004, p.rest[p.rest[:, 2].argmin()], "rest")
    contacts = []
    for c in getattr(b, "contacts", []):
        part = next((p for p in parts if p.name == c["part"]), None)
        if part is not None:
            contacts.append(dict(c, vertex=int(np.argmin(((part.rest - np.array(c["at"])) ** 2).sum(axis=1)))))
    # What is covered at rest: points of the part inside the solid.
    covered = {}
    for pa, pb, tol in COVERED:
        for a in match(parts, pa):
            # Its own trouser leg (the other leg passes close in a stride, and is not what covers it).
            own = "Trousers.R" if a.name.endswith("0") else "Trousers.L"
            for bb in match(parts, own):
                inside = [i for i, co in enumerate(a.rest) if (lambda s: s is not None and s < -0.008)(side_of_surface(rest, bb, co))]
                if inside:
                    covered[(a.name, bb.name, tol)] = inside
    check_frame(report, rest, "rest", parts, contacts, covered, bones, floating=True)
    frames_by_label = {}
    if poses:
        frames, residual = load_poses(poses, bones)
        print(f"AUDIT {name}: {len(frames)} recorded frames, mapped with {residual * 1000:.2f} mm residual")
        for k, (label, mats) in enumerate(frames):
            if k % every:
                continue
            # The tool is in the hands on the frames that say where it is.
            here = parts + working if "Tool" in mats else parts
            fr = Frame(here, {p.name: pose(p, mats, bone_names) for p in here}, mats)
            fr.flip, fr.open = rest.flip, rest.open
            if "Tool" in mats and working:
                held = [(r"Hand1", r"WorkPickHandle", 0.002)] if 1 in hands_that_close(b) else []
                APART.extend(WORK_APART)
                MEETS.extend(WORK_MEETS + held)
                try:
                    check_frame(report, fr, f"{label}{k}", here, contacts, covered, bones, floating=False)
                finally:
                    del APART[-len(WORK_APART):]
                    del MEETS[-len(WORK_MEETS + held):]
            else:
                check_frame(report, fr, f"{label}{k}", here, contacts, covered, bones, floating=(k % (every * 10) == 0))
            frames_by_label[f"{label}{k}"] = mats
    text = report.text()
    print(text)
    with open(os.path.join(out_dir, f"audit_{name}.txt"), "w", encoding="utf-8") as f:
        f.write(text + "\n")
    with open(os.path.join(out_dir, f"audit_{name}.json"), "w", encoding="utf-8") as f:
        json.dump(list(report.rows.values()), f, indent=1)
    if snap and report.failures():
        snapshots(parts + working, bone_names, report.failures(), frames_by_label, out_dir, name)
    return report


def hands_that_close(b):
    """The body indices of the hands that close on a handle (hands.py), without importing it here."""
    return {i for i, h in getattr(b, "hands", {}).items() if h.get("closes")}


def snapshots(parts, bone_names, failures, frames, out_dir, name, limit=30):
    """Close renders of each failure's worst place, posed as in its worst frame (EEVEE, the parts' own colours)."""
    scene = bpy.context.scene
    engines = {e.identifier for e in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items}
    scene.render.engine = 'BLENDER_EEVEE_NEXT' if 'BLENDER_EEVEE_NEXT' in engines else 'BLENDER_EEVEE'
    scene.render.resolution_x = scene.render.resolution_y = 520
    cam = bpy.data.objects.get("AuditCam") or bpy.data.objects.new("AuditCam", bpy.data.cameras.new("AuditCam"))
    if cam.name not in scene.collection.objects:
        scene.collection.objects.link(cam)
    cam.data.lens = 50
    cam.data.clip_start = 0.005
    scene.camera = cam
    if not bpy.data.objects.get("AuditSun"):
        sun = bpy.data.objects.new("AuditSun", bpy.data.lights.new("AuditSun", 'SUN'))
        sun.rotation_euler = (math.radians(50), 0, math.radians(-30))
        sun.data.energy = 3.0
        scene.collection.objects.link(sun)
    world = scene.world or bpy.data.worlds.new("AuditWorld")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.35, 0.37, 0.42, 1)
    scene.world = world
    scene.view_settings.view_transform = 'Standard'
    real = [p for p in parts if not p.virtual]
    shown = []
    # One view for each kind of failure on each family of parts (not sixteen views of sixteen laces).
    chosen, seen = [], set()
    for f in sorted((f for f in failures if f["where"] is not None), key=lambda r: -r["value"]):
        family = (f["check"], re.sub(r"[\d#.\-]+", "", f["parts"]))
        if family not in seen:
            seen.add(family)
            chosen.append(f)
    for k, f in enumerate(chosen[:limit]):
        mats = frames.get(f["frame"])
        copies = []
        for p in real:
            verts = p.rest if mats is None else pose(p, mats, bone_names)
            mesh = bpy.data.meshes.new(f"Snap_{p.name}")
            mesh.from_pydata(verts.tolist(), [], [list(x) for x in p.polys])
            o = bpy.data.objects.new(f"Snap_{p.name}", mesh)
            for slot in p.obj.material_slots:
                o.data.materials.append(slot.material)
            scene.collection.objects.link(o)
            copies.append(o)
        for p in real:
            p.obj.hide_render = True
        where = Vector(f["where"])
        for v, d in enumerate((Vector((0.6, -1, 0.35)), Vector((-0.8, 0.5, 0.2)))):
            cam.location = where + d.normalized() * 0.32
            cam.rotation_euler = (where - cam.location).to_track_quat('-Z', 'Y').to_euler()
            path = os.path.join(out_dir, f"snap_{name}_{k:02d}_{v}.png")
            scene.render.filepath = path
            bpy.ops.render.render(write_still=True)
            shown.append((path, f))
        for o in copies:
            bpy.data.objects.remove(o)
        for p in real:
            p.obj.hide_render = False
    with open(os.path.join(out_dir, f"snaps_{name}.txt"), "w", encoding="utf-8") as fh:
        for path, f in shown:
            fh.write(f"{os.path.basename(path)}\t{f['check']}\t{f['parts']}\t{f['value'] * 1000:.1f} mm\t{f['frame']}\n")
