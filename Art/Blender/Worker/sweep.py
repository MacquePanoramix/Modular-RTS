"""Wonder Gather — the pose sweep beyond the game's own movement (Docs/ArtDirection/ModelQualityMethod.md, pass 4).

The game's recorded frames (audit.py) cover standing, walking, turning and stopping. Work asks more of a body: arms
raised to swing, a deep bend at the knees, a twist at the waist. This sweeps each joint through such poses, one at
a time and in a few combinations, the way riggers test a rig's range, and checks what the skin and the cloth do:

- pinched: a sleeve or trouser leg collapsing at a bent elbow or knee (its thinnest place, against the rest pose);
- stretched: cloth pulled long (the longest edge, against the rest pose);
- the audit's own checks (sinks, beneath, lies on, meets), on every pose;
- a picture of every pose from two sides, to be read as pass 5 reads the capture.

The parts are posed as the game skins them (linear blend skinning on the exported weights, at most four bones a
point). A skirt's flaps go with the thighs as MinerBody.Drape turns them; things that hang stay plumb under the
place they hang from.

Run: blender -b --factory-startup --python workers.py -- --out <folder> --sweep [--only Small]
"""
import math
import os
import re

import bpy
import numpy as np
from mathutils import Vector

import audit

X, Y, Z = (1.0, 0.0, 0.0), (0.0, 1.0, 0.0), (0.0, 0.0, 1.0)


def turn(axis, degrees):
    """A rotation about an axis of the rest pose (x the being's left, -y its front, z up)."""
    a = np.array(axis, dtype=np.float64)
    a = a / np.linalg.norm(a)
    c, s = math.cos(math.radians(degrees)), math.sin(math.radians(degrees))
    k = np.array([[0, -a[2], a[1]], [a[2], 0, -a[0]], [-a[1], a[0], 0]])
    return np.eye(3) * c + s * k + (1 - c) * np.outer(a, a)


def both(name, left, right=None):
    """The same turn on both sides (right: its mirror, when it differs)."""
    return {f"{name}.L": left, f"{name}.R": right if right is not None else left}


def poses():
    """The sweep: (name, what it tests, {bone: 3x3 turn}). A limb that hangs down goes forward by a negative turn
    about x; the trunk and the head, which stand up, bow forward by a positive one. An arm goes out to its side by
    a turn about y (opposite for the two arms)."""
    forward = lambda d: turn(X, -d)
    bow = lambda d: turn(X, d)
    out_l, out_r = (lambda d: turn(Y, -d)), (lambda d: turn(Y, d))
    list_ = [
        ("arms forward 45", "shoulder", both("UpperArm", forward(45))),
        ("arms forward 90", "shoulder", both("UpperArm", forward(90))),
        ("arms overhead", "shoulder", both("UpperArm", forward(150))),
        ("arms back 30", "shoulder", both("UpperArm", forward(-30))),
        ("arms out 65", "shoulder", {"UpperArm.L": out_l(65), "UpperArm.R": out_r(65)}),
        ("elbows 45", "elbow", both("Forearm", forward(45))),
        ("elbows 90", "elbow", both("Forearm", forward(90))),
        ("elbows 130", "elbow", both("Forearm", forward(130))),
        ("wrists up 40", "wrist", both("Hand", forward(40))),
        ("wrists down 40", "wrist", both("Hand", forward(-40))),
        ("backswing", "shoulder and elbow", {**both("UpperArm", forward(120)), **both("Forearm", forward(70)), "Chest": bow(-8)}),
        ("strike", "shoulder and elbow", {**both("UpperArm", forward(55)), **both("Forearm", forward(25)), "Spine": bow(12), "Chest": bow(12)}),
        ("knee lifted 45", "hip", {"Thigh.L": forward(45), "Shin.L": forward(-45)}),
        ("knee lifted 90", "hip", {"Thigh.L": forward(90), "Shin.L": forward(-90)}),
        ("leg back 30", "hip", {"Thigh.R": forward(-30)}),
        ("knees 45", "knee", both("Shin", forward(-45))),
        ("knees 90", "knee", both("Shin", forward(-90))),
        ("deep bend", "hip, knee and ankle", {**both("Thigh", forward(80)), **both("Shin", forward(-125)), **both("Foot", forward(45)),
                                           "Spine": bow(18), "Chest": bow(14)}),
        ("bow 40", "waist", {"Spine": bow(20), "Chest": bow(20)}),
        ("lean back 15", "waist", {"Spine": bow(-8), "Chest": bow(-7)}),
        ("side bend 20", "waist", {"Spine": turn(Y, 10), "Chest": turn(Y, 10)}),
        ("twist 35", "waist", {"Spine": turn(Z, 17), "Chest": turn(Z, 18)}),
        ("head down 25", "neck", {"Neck": bow(12), "Head": bow(13)}),
        ("head up 25", "neck", {"Neck": bow(-12), "Head": bow(-13)}),
        ("head turned 60", "neck", {"Neck": turn(Z, 25), "Head": turn(Z, 35)}),
        ("head tilted 20", "neck", {"Neck": turn(Y, 8), "Head": turn(Y, 12)}),
        ("toes bent 35", "foot", both("Toe", forward(-35))),
    ]
    return list_


WRIST_GIVE = 32.0  # degrees, as MinerBody.WristGive


def about(axis, radians):
    a = np.array(axis, dtype=np.float64)
    n = np.linalg.norm(a)
    if n < 1e-9 or abs(radians) < 1e-9:
        return np.eye(3)
    return turn(a / n, math.degrees(radians))


def onto(u, v, limit=None):
    """The smallest turn taking direction u to direction v (at most limit radians)."""
    u, v = u / max(np.linalg.norm(u), 1e-9), v / max(np.linalg.norm(v), 1e-9)
    angle = math.acos(max(-1.0, min(1.0, float(np.dot(u, v)))))
    if limit is not None:
        angle = min(angle, limit)
    return about(np.cross(u, v), angle)


def solve(bones, turns, slack=(0.0, 0.0), held=None):
    """Each bone's skinning matrix for a pose: its own turn about its rest head, carried by its parents' (forward
    kinematics). Skirt flaps turn with their thighs past the measured slack, as the game turns them. What hangs
    from the body (a satchel, a hammer) moves with the place it hangs from and stays plumb. What a hand carries
    (held: {bone: (hand, the handle's direction at rest)}) goes with the hand as in the game: the wrist gives so
    the handle stays level, as far as a wrist can, and the thing hangs square to its handle."""
    turns = dict(turns)
    # The skirt's flaps: pushed by the thigh that moves into them.
    for side in ("L", "R"):
        thigh = turns.get(f"Thigh.{side}")
        if thigh is None:
            continue
        down = thigh @ np.array((0.0, 0.0, -1.0))
        swing = math.degrees(math.atan2(-down[1], -down[2]))  # forward positive
        front, back = max(0.0, swing - slack[0]), min(0.0, swing + slack[1])
        if f"SkirtFront.{side}" in bones and front > 0:
            turns[f"SkirtFront.{side}"] = turn(X, -front)
        if f"SkirtBack.{side}" in bones and back < 0:
            turns[f"SkirtBack.{side}"] = turn(X, -back)
    mats = {}

    def matrix(name):
        if name in mats:
            return mats[name]
        head, _, _, parent = bones[name]
        h = np.array(head, dtype=np.float64)
        local = np.eye(4)
        r = turns.get(name)
        if r is not None:
            local[:3, :3] = r
            local[:3, 3] = h - r @ h
        m = (matrix(parent) @ local) if parent in bones else local
        mats[name] = m
        return m
    for n in bones:
        matrix(n)
    held = held or {}
    down = np.array((0.0, 0.0, -1.0))
    for n, (head, _, _, parent) in bones.items():
        h = np.array(head, dtype=np.float64)
        if n in held and held[n][0] in mats:
            hand, axis = held[n]
            wrist = np.array(bones[hand][0], dtype=np.float64)
            m_hand = mats[hand]
            wrist_now = m_hand[:3, :3] @ wrist + m_hand[:3, 3]
            handle = m_hand[:3, :3] @ np.array(axis, dtype=np.float64)
            # The wrist gives: the handle turns towards level, as far as a wrist can.
            level = handle - down * float(np.dot(handle, down))
            give = onto(handle, level, math.radians(WRIST_GIVE)) if np.linalg.norm(level) > 1e-6 else np.eye(3)
            g = np.eye(4)
            g[:3, :3] = give
            g[:3, 3] = wrist_now - give @ wrist_now
            mats[hand] = g @ m_hand
            carried = mats[hand][:3, :3]
            handle = carried @ np.array(axis, dtype=np.float64)
            handle = handle / max(np.linalg.norm(handle), 1e-9)
            # It hangs about its handle, as plumb as the handle lets it.
            hangs = carried @ down
            hangs = hangs - handle * float(np.dot(hangs, handle))
            plumb = down - handle * float(np.dot(down, handle))
            swing = onto(hangs, plumb) if np.linalg.norm(hangs) > 1e-6 and np.linalg.norm(plumb) > 1e-6 else np.eye(3)
            r = swing @ carried
            moved = mats[hand][:3, :3] @ h + mats[hand][:3, 3]
            m = np.eye(4)
            m[:3, :3] = r
            m[:3, 3] = moved - r @ h
            mats[n] = m
        elif n in ("Lantern", "Mug", "Satchel", "Hammer") and parent in mats:
            # What hangs from the body stays plumb: it goes where its place goes, and does not turn with it.
            moved = mats[parent][:3, :3] @ h + mats[parent][:3, 3]
            m = np.eye(4)
            m[:3, 3] = moved - h
            mats[n] = m
    mats["Root"] = np.eye(4)
    return mats


JOINTS = {  # the joint, the two bones that meet at it, and the parts that cover it
    "elbow": ("Forearm", "UpperArm", r"Top\.Arm\.[LR]|Skin|Forearm\d"),
    "knee": ("Shin", "Thigh", r"Trousers\.[LR]"),
    "shoulder": ("UpperArm", "Chest", r"Top"),
    "wrist": ("Hand", "Forearm", r"Hand\d|Top\.Arm\.[LR]|Skin|Forearm\d"),
}


def pinched(report, fr, rest, parts, bones, bone_index, label):
    """How much a limb's cover thins at a bent joint: for the points both bones share, their distance from the
    joint, against the rest pose. Linear blend skinning pulls such points towards the joint as it bends."""
    for joint, (child, parent, pattern) in JOINTS.items():
        for side in ("L", "R"):
            c, p_ = f"{child}.{side}", (f"{parent}.{side}" if parent != "Chest" else "Chest")
            if c not in bones or p_ not in bone_index:
                continue
            at_rest = np.array(bones[c][0], dtype=np.float64)
            m = fr.mats[c]
            at_now = m[:3, :3] @ at_rest + m[:3, 3]
            worst, where = 0.0, None
            for part in audit.match(parts, pattern):
                w_c, w_p = part.weights[:, bone_index[c]], part.weights[:, bone_index[p_]]
                band = np.where((w_c > 0.2) & (w_p > 0.2))[0]
                if len(band) < 6:
                    continue
                d_rest = np.linalg.norm(part.rest[band] - at_rest, axis=1)
                d_now = np.linalg.norm(fr.verts[part.name][band] - at_now, axis=1)
                keep = d_rest > 0.012
                if not keep.any():
                    continue
                # The cover's girth at the joint: how near its nearest tenth comes, now against at rest.
                near_rest = np.sort(d_rest[keep])[: max(3, keep.sum() // 10)].mean()
                near_now = np.sort(d_now[keep])[: max(3, keep.sum() // 10)].mean()
                loss = 1.0 - near_now / max(near_rest, 1e-6)
                if loss > worst:
                    worst, where = loss, fr.verts[part.name][band][np.argmin(d_now)]
            # In the report's units, the number printed is the percentage of its girth the cover has lost.
            report.add("pinched", f"{joint}.{side}", worst / 10.0, 0.045, where, label)


def stretched(report, fr, parts, label, limit=1.9):
    """The longest any edge of a cloth part has been pulled, against its length at rest."""
    for part in parts:
        if part.virtual or not re.match(r"^(Top|Skirt|Trousers|Apron|Collar|Neckband|Skin)$", part.name):
            continue
        edges = getattr(part, "edges", None)
        if edges is None:
            seen = set()
            for poly in part.polys:
                for a, b in zip(poly, poly[1:] + poly[:1]):
                    seen.add((min(a, b), max(a, b)))
            edges = part.edges = np.array(sorted(seen), dtype=np.int64)
            part.edge_rest = np.linalg.norm(part.rest[edges[:, 0]] - part.rest[edges[:, 1]], axis=1)
        now = np.linalg.norm(fr.verts[part.name][edges[:, 0]] - fr.verts[part.name][edges[:, 1]], axis=1)
        long_enough = part.edge_rest > 0.004
        ratio = np.where(long_enough, now / np.maximum(part.edge_rest, 1e-6), 1.0)
        k = int(np.argmax(ratio))
        report.add("stretched", part.name, float(ratio[k]) / 1000.0, limit / 1000.0, fr.verts[part.name][edges[k, 0]], label)


def render(parts, verts, out_path, views, height, close=None):
    """The posed parts, drawn from each view (EEVEE, the parts' own colours). close: a place to look at from
    near as well (the joint a pose tests), from in front and to its own side."""
    scene = bpy.context.scene
    engines = {e.identifier for e in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items}
    scene.render.engine = 'BLENDER_EEVEE_NEXT' if 'BLENDER_EEVEE_NEXT' in engines else 'BLENDER_EEVEE'
    scene.render.resolution_x, scene.render.resolution_y = 420, 560
    cam = bpy.data.objects.get("SweepCam") or bpy.data.objects.new("SweepCam", bpy.data.cameras.new("SweepCam"))
    if cam.name not in scene.collection.objects:
        scene.collection.objects.link(cam)
    cam.data.lens = 70
    cam.data.clip_start = 0.01
    scene.camera = cam
    for n, rot, energy in (("SweepKey", (55, 0, -30), 3.0), ("SweepFill", (70, 0, 150), 1.2)):
        if not bpy.data.objects.get(n):
            light = bpy.data.objects.new(n, bpy.data.lights.new(n, 'SUN'))
            light.rotation_euler = tuple(math.radians(a) for a in rot)
            light.data.energy = energy
            scene.collection.objects.link(light)
    world = scene.world or bpy.data.worlds.new("SweepWorld")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.33, 0.35, 0.4, 1)
    scene.world = world
    scene.view_settings.view_transform = 'Standard'
    real = [p for p in parts if not p.virtual]
    copies = []
    for p in real:
        mesh = bpy.data.meshes.new(f"Sweep_{p.name}")
        mesh.from_pydata(verts[p.name].tolist(), [], [list(x) for x in p.polys])
        for poly in mesh.polygons:
            poly.use_smooth = True
        o = bpy.data.objects.new(f"Sweep_{p.name}", mesh)
        for slot in p.obj.material_slots:
            o.data.materials.append(slot.material)
        scene.collection.objects.link(o)
        copies.append(o)
        p.obj.hide_render = True
    paths = []
    middle = Vector((0, 0, height * 0.52))
    for k, direction in enumerate(views):
        d = Vector(direction).normalized()
        cam.location = middle + d * max(height, 1.2) * 2.4
        cam.rotation_euler = (middle - cam.location).to_track_quat('-Z', 'Y').to_euler()
        path = f"{out_path}_{k}.png"
        scene.render.filepath = path
        bpy.ops.render.render(write_still=True)
        paths.append(path)
    if close is not None:
        at = Vector(close)
        cam.location = at + Vector((1.0, -0.8, 0.25)).normalized() * 0.5
        cam.rotation_euler = (at - cam.location).to_track_quat('-Z', 'Y').to_euler()
        path = f"{out_path}_{len(views)}.png"
        scene.render.filepath = path
        bpy.ops.render.render(write_still=True)
        paths.append(path)
    for o in copies:
        bpy.data.objects.remove(o)
    for p in real:
        p.obj.hide_render = False
    return paths


def run(b, meshes, bones, out_dir, pictures=True):
    """Sweeps one being through the poses. Writes sweep_<name>.txt and, with pictures, sweep_<name>_<pose>_<view>.png."""
    prefix = b.name + "_"
    bone_names = list(bones.keys()) + ["Root"]
    bone_index = {n: i for i, n in enumerate(bone_names)}
    parts = [audit.Part(o, prefix, bone_index) for o in meshes if o.type == 'MESH' and len(o.data.polygons)]
    audit.split(parts, bone_index)
    name = b.name.split("_")[-1]
    report = audit.Report(name)
    rest = audit.Frame(parts, {p.name: p.rest for p in parts})
    for p in parts:
        if re.match(f"^(?:{audit.SOLIDS})$", p.name) and not p.virtual:
            rest.flip[p.name] = 1.0 if audit.signed_volume(p.rest, p.polys) >= 0 else -1.0
    for p in parts:
        if p.virtual and p.name.split(".")[0] in rest.flip:
            rest.flip[p.name] = rest.flip[p.name.split(".")[0]]
    slack = getattr(b, "skirt_slack", [0.0, 0.0]) if getattr(b, "skirt_open", False) else [0.0, 0.0]
    slack = [max(0.0, s - 3.0) * 0.8 for s in slack]
    height = float(max(p.rest[:, 2].max() for p in parts))
    held = {s["bone"]: (s["hand"], s["ring"]) for s in getattr(b, "swings", []) if s.get("hand")}
    written = []
    for label, tests, turns in poses():
        mats = solve(bones, turns, slack, held)
        fr = audit.Frame(parts, {p.name: audit.pose(p, mats, bone_names) for p in parts}, mats)
        fr.flip, fr.open = rest.flip, rest.open
        key = label.replace(" ", "_")
        audit.check_frame(report, fr, key, parts, [], {}, bones, floating=False)
        pinched(report, fr, rest, parts, bones, bone_index, key)
        stretched(report, fr, parts, key)
        if pictures:
            close = None
            for joint, (child, _, _) in JOINTS.items():
                if joint in tests and f"{child}.L" in bones:
                    at = np.array(bones[f"{child}.L"][0], dtype=np.float64)
                    close = mats[f"{child}.L"][:3, :3] @ at + mats[f"{child}.L"][:3, 3]
                    break
            written.append((label, tests, render(parts, fr.verts, os.path.join(out_dir, f"sweep_{name}_{key}"),
                                                 ((0.75, -1, 0.12), (-1, -0.25, 0.1)), height, close)))
    text = report.text().replace("AUDIT", "SWEEP", 1)
    print(text)
    with open(os.path.join(out_dir, f"sweep_{name}.txt"), "w", encoding="utf-8") as f:
        f.write(text + "\n")
    with open(os.path.join(out_dir, f"sweep_{name}_poses.txt"), "w", encoding="utf-8") as f:
        for label, tests, paths in written:
            f.write(f"{label}\t{tests}\t" + "\t".join(os.path.basename(p) for p in paths) + "\n")
    return report
