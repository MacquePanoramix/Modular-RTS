"""Wonder Gather — rigging and the game-ready export of beings (S1d).

A being is built a second time in a neutral rest pose (feet under the hips,
arms hanging a little out from the body, the head straight), and then:

1. **Skeleton.** An armature on the same joints the procedural biped solves:
   pelvis, spine, chest, neck, head; upper arms, forearms, hands; thighs,
   shins, feet, toes. Bone names say the being's own side: `.L` is its left.
2. **Skinning.** Each part is weighted to the bones that should carry it, by
   rules per kind of part (a boot follows its foot and shin, hair its head,
   a coat's skirt the pelvis and, towards the hem, the thighs) and, within
   those, by distance to each bone's capsule.
3. **Lighter.** The parts are simplified to a budget and joined into one mesh.
4. **One texture.** The mesh is unwrapped once, and the painting (the house's
   painter), the painted face and the plain colours are all baked into a
   single atlas. One mesh, one material, and a small glow material for lamps:
   few draw calls, as an RTS with many units needs.
5. **Levels of detail.** LOD1 and LOD2 are simplified copies of LOD0. LOD2
   also drops the small details (laces, buttons, ties).

The export is one FBX per being (`Miner_<Name>.fbx`), its atlas, and a
manifest of its body's dimensions for the game's procedural body.
"""
import json
import math
import os

import bmesh
import bpy
from mathutils import Vector

import shapes

# Budgets in triangles.
LOD_BUDGETS = (16000, 5000, 1600)
ATLAS = 2048
# Parts dropped from the farthest level of detail.
DETAILS = ("_Lace", "_Eyelet", "_Button", "_LanternBar", "_LanternBail", "_LanternEar", "_ApronTie", "_Buckle", "_CapLampMount", "_Spring", "_StrapTab",
           "_BagRing", "_BagLoop", "_PickLoop", "_ApronKnot", "_HammerLoop", "_StrapTab")
# Parts that lie on others and move exactly with them: (the part's name, the parts it takes its weights from).
# \1 is the part's own number (a boot's laces lie on that boot).
LIKE = [
    (r"_(?:Lace|Eyelet)(\d)$", [r"_Boot\1"]),
    (r"_Strap$", [r"_Top", r"_Skirt", r"_Collar"]),
    (r"_PickStrap$", [r"_Top", r"_Skirt", r"_Collar", r"_Lapel\d", r"_ShirtFront"]),
    (r"_ApronStrap$", [r"_Top", r"_Neckband", r"_Apron"]),
    (r"_ApronTie\d$|_ApronKnot$|_ApronTieEnd\d$", [r"_Top", r"_Skirt"]),
    (r"_ApronPocket$|_HammerLoop$", [r"_Apron"]),
    # An apron lies on the smock: it moves exactly with it.
    (r"_Apron$", [r"_Top", r"_Skirt"]),
    (r"_Patch0$", [r"_Top"]),
    (r"_Patch1$", [r"_Skirt"]),
    (r"_Button\d$", [r"_Top", r"_Skirt"]),
]


def side_of(i):
    """Body index 0 is the being's right (x < 0), 1 its left."""
    return "R" if i == 0 else "L"


# ---------------------------------------------------------------- the skeleton

def joints(b):
    """The rest pose's joints and each bone's (head, tail, capsule radius), in Blender space."""
    p, hr = b.p, b.hr
    hips = (b.hips[0] + b.hips[1]) * 0.5
    head_base = b.on_head(0, 0.12, -0.62)
    bones = {
        "Pelvis": (hips, b.waist, b.trunk["pelvis"][0], None),
        "Spine": (b.waist, b.chest, b.trunk["waist"][0], "Pelvis"),
        "Chest": (b.chest, b.collar, b.trunk["chest"][0], "Spine"),
        "Neck": (b.collar, head_base, hr * p.get("neck_r", 0.5), "Chest"),
        "Head": (head_base, b.on_head(0, 0.05, 1.15), hr, "Neck"),
    }
    L = p.get("hand", 0.11) * b.H
    F = p.get("foot", 0.165) * b.H
    for i in (0, 1):
        s = side_of(i)
        along = (b.wrists[i] - b.elbows[i]).normalized()
        bones[f"UpperArm.{s}"] = (b.shoulders[i], b.elbows[i], p["arm"] * 1.35, "Chest")
        bones[f"Forearm.{s}"] = (b.elbows[i], b.wrists[i], p["arm"] * 1.1, f"UpperArm.{s}")
        bones[f"Hand.{s}"] = (b.wrists[i], b.wrists[i] + along * L * 0.85, L * 0.32, f"Forearm.{s}")
        f, an = b.foot_frame(i)
        ground = Vector((an.x, an.y, 0.03))
        ball = ground + f * F * 0.35
        bones[f"Thigh.{s}"] = (b.hips[i], b.knees[i], p["leg"] * 1.4, "Pelvis")
        bones[f"Shin.{s}"] = (b.knees[i], b.ankles[i], p["leg"] * 1.15, f"Thigh.{s}")
        bones[f"Foot.{s}"] = (b.ankles[i], ball, F * 0.22, f"Shin.{s}")
        bones[f"Toe.{s}"] = (ball, ground + f * F * 0.58, F * 0.2, f"Foot.{s}")
    # Carried things with bones of their own (a lantern that swings, a mug in a hand).
    for name, head, tail, parent in getattr(b, "props", []):
        bones[name] = (head, tail, 0.05, parent)
    # A skirt hangs in four flaps, front and back of each leg, each on a bone at its hip: the game turns a flap
    # with the thigh that pushes it, and lets it hang when the thigh moves away.
    hem = getattr(b, "skirt_hem", None)
    if hem is not None:
        for i in (0, 1):
            for flap in ("Front", "Back"):
                bones[f"Skirt{flap}.{side_of(i)}"] = (b.hips[i].copy(), Vector((b.hips[i].x, b.hips[i].y, hem)), 0.05, "Pelvis")
    return bones


def armature(b, name):
    bones = joints(b)
    data = bpy.data.armatures.new(name)
    obj = shapes.link(bpy.data.objects.new(name, data))
    bpy.context.view_layer.objects.active = obj
    for o in bpy.context.selected_objects:
        o.select_set(False)
    obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    root = data.edit_bones.new("Root")
    root.head, root.tail = (0, 0, 0), (0, 0.15, 0)
    made = {}
    for n, (h, t, r, parent) in bones.items():
        e = data.edit_bones.new(n)
        e.head, e.tail = h, t
        e.use_deform = True
        made[n] = e
    for n, (h, t, r, parent) in bones.items():
        made[n].parent = made[parent] if parent else root
        made[n].use_connect = False
    root.use_deform = False
    bpy.ops.object.mode_set(mode='OBJECT')
    return obj, bones


# ---------------------------------------------------------------- skinning

def segment_distance(p, a, b):
    ab = b - a
    t = max(0.0, min(1.0, (p - a).dot(ab) / max(ab.length_squared, 1e-9)))
    return (p - (a + ab * t)).length


def capsule_weights(co, names, bones, power=5.0, keep=3):
    ws = []
    for n in names:
        h, t, r, _ = bones[n]
        d = segment_distance(co, h, t) / max(r, 1e-4)
        ws.append((1.0 / (1e-3 + d ** power), n))
    ws.sort(reverse=True)
    ws = ws[:keep]
    total = sum(w for w, _ in ws)
    return [(n, w / total) for w, n in ws if w / total > 0.02]


TRUNK = ["Pelvis", "Spine", "Chest", "Neck"]
ARMS = ["UpperArm.L", "UpperArm.R", "Forearm.L", "Forearm.R", "Hand.L", "Hand.R"]
LEGS = ["Thigh.L", "Thigh.R", "Shin.L", "Shin.R", "Foot.L", "Foot.R"]


def candidates(name):
    """The bones a part may follow, chosen by the kind of part (from its name). Garments lying on one another
    take the same bones from the same field (as weights transferred from the body would be), so the layers move
    together and do not cut through each other; carried things are rigid on one bone."""
    import re
    m = re.search(r"_(Boot|Sole|Heel|Lace|Eyelet|BootCuff|Hand|Forearm|Roll|Finger|Thumb|Palm)(\d)", name)
    side = side_of(int(m.group(2))) if m else None
    # Carried things first (a lantern's own cap is not a cap on the head).
    if "_Lantern" in name:
        return "rigid:Lantern"
    if "_Mug" in name:
        return "rigid:Mug"
    if "_HammerHandle" in name or "_HammerHead" in name:
        return "rigid:Hammer"
    if any(k in name for k in ("_Bag", "_Buckle", "_StrapTab")):
        return "rigid:Satchel"
    if "_PickStrap" in name or name.endswith("_Strap"):
        return TRUNK + ARMS
    if "_Hair" in name or "_Cap" in name:
        return ["Head"]
    if name.endswith("_Skin"):
        return ["Head", "Neck", "Chest"]
    if m and m.group(1) in ("Boot", "Sole", "Heel", "Lace", "Eyelet", "BootCuff"):
        return [f"Shin.{side}", f"Foot.{side}", f"Toe.{side}"]
    if m and m.group(1) in ("Hand",):
        return [f"Hand.{side}", f"Forearm.{side}"]
    if m and m.group(1) in ("Forearm", "Roll"):
        return [f"UpperArm.{side}", f"Forearm.{side}", f"Hand.{side}"]
    if name.endswith("_Top"):
        return TRUNK + ARMS
    if name.endswith("_Trousers"):
        return ["Pelvis", "Spine"] + LEGS
    if any(k in name for k in ("_Collar", "_Lapel", "_ShirtFront", "_Button", "_Neckband", "_ApronStrap", "_Patch0", "_ApronTie", "_ApronKnot")):
        # On the torso: the same bones, by the same rule, as the top beneath them.
        return TRUNK + ARMS
    if "_Pick" in name:
        return "rigid:Chest"
    if any(k in name for k in ("_Skirt", "_Apron", "_Patch1", "_ApronPocket", "_Hammer")):
        return "skirt"
    return None


def skin(obj, b, bones):
    """Vertex groups on one part, by its kind and by distance to its bones' capsules."""
    names = candidates(obj.name)
    groups = {}
    mesh = obj.data
    mesh.update()
    if isinstance(names, str) and names.startswith("rigid:"):
        bone = names.split(":", 1)[1]
        if bone not in bones:
            bone = "Pelvis"
        g = obj.vertex_groups.new(name=bone)
        g.add([v.index for v in mesh.vertices], 1.0, 'REPLACE')
        return
    if names == "skirt":
        # Coats, smocks and aprons: above the waist they follow the torso exactly as the top beneath them does;
        # below, they hang from the pelvis, and towards the hem the thighs carry more of them, each its own side.
        # Below the hips they hang in four flaps (front and back of each leg, blending into each other round the
        # body and into the pelvis above); a flap is wholly its own from a hand's width above the hem down, where
        # the legs are under it, so a leg that pushes its flap never comes through.
        top = b.pelvis.z
        hem = getattr(b, "skirt_hem", None) or min(v.co.z for v in mesh.vertices)
        width, depth = b.trunk["pelvis"]
        whole = min(0.45, max(1e-3, (top - (hem + 0.09)) / max(top - hem, 1e-3)))
        flaps = "SkirtFront.L" in bones
        for v in mesh.vertices:
            t = max(0.0, min(1.0, (top - v.co.z) / max(top - hem, 1e-3)))
            left = max(0.0, min(1.0, 0.5 + (v.co.x - b.pelvis.x) / (1.2 * width)))
            if flaps:
                share = max(0.0, min(1.0, t / whole))
                share = share * share * (3 - 2 * share)
                front = max(0.0, min(1.0, 0.5 - (v.co.y - b.pelvis.y) / (0.9 * depth)))
                hang = {"Pelvis": 1 - share, "SkirtFront.L": share * left * front, "SkirtBack.L": share * left * (1 - front),
                        "SkirtFront.R": share * (1 - left) * front, "SkirtBack.R": share * (1 - left) * (1 - front)}
            else:
                thigh = 0.62 * t
                hang = {"Pelvis": 1 - thigh, "Thigh.L": thigh * left, "Thigh.R": thigh * (1 - left)}
            above = max(0.0, min(1.0, (v.co.z - b.pelvis.z) / max(b.waist.z - b.pelvis.z, 1e-3)))
            if above > 0:
                body_w = dict(capsule_weights(v.co, ["Pelvis", "Spine", "Chest"], bones))
                hang = {n: hang.get(n, 0.0) * (1 - above) + body_w.get(n, 0.0) * above for n in set(hang) | set(body_w)}
            for n, w in hang.items():
                if w > 0.01:
                    groups.setdefault(n, []).append((v.index, w))
    else:
        names = names or [n for n in bones]
        for v in mesh.vertices:
            for n, w in capsule_weights(v.co, names, bones):
                groups.setdefault(n, []).append((v.index, w))
    for n, items in groups.items():
        g = obj.vertex_groups.get(n) or obj.vertex_groups.new(name=n)
        for index, w in items:
            g.add([index], w, 'REPLACE')


# ---------------------------------------------------------------- lighter, joined, one texture

def triangles(obj):
    return sum(len(p.vertices) - 2 for p in obj.data.polygons)


def simplify_to(obj, ratio, floor=120):
    if triangles(obj) <= floor or ratio >= 1:
        return
    dec = obj.modifiers.new("Lighter", 'DECIMATE')
    dec.ratio = max(ratio, floor / max(triangles(obj), 1))
    dec.use_collapse_triangulate = True
    shapes.apply_all(obj)


def weights_like(obj, sources, anchor=None):
    """A part lying on others takes their weights: each vertex those of the nearest point on their surfaces (as a
    weight transfer does), so it moves exactly with what it lies on and never lifts off it or sinks into it.
    anchor: one point whose weights the whole part takes, for a rigid thing hung on cloth (a hammer in its loop):
    it moves with the cloth there, and keeps its shape."""
    from mathutils.bvhtree import BVHTree
    depsgraph = bpy.context.evaluated_depsgraph_get()
    trees = [(s, BVHTree.FromObject(s, depsgraph)) for s in sources]
    obj.vertex_groups.clear()
    for v in obj.data.vertices:
        best = None
        for s, tree in trees:
            hit, _, index, dist = tree.find_nearest(anchor if anchor is not None else v.co, 0.5)
            if hit is not None and (best is None or dist < best[3]):
                best = (s, hit, index, dist)
        if best is None:
            continue
        s, hit, index, _ = best
        blend, total = {}, 0.0
        for vi in s.data.polygons[index].vertices:
            sv = s.data.vertices[vi]
            w = 1.0 / (1e-4 + (sv.co - hit).length)
            total += w
            for g in sv.groups:
                n = s.vertex_groups[g.group].name
                blend[n] = blend.get(n, 0.0) + w * g.weight
        for n, w in blend.items():
            if w / total > 0.01:
                g = obj.vertex_groups.get(n) or obj.vertex_groups.new(name=n)
                g.add([v.index], w / total, 'REPLACE')


def ends_follow(obj, ends, bone, reach=0.075):
    """A strap's last stretch before each end blends from the clothes it lies on to the bone its end is tied to."""
    groups = obj.vertex_groups
    target = groups.get(bone) or groups.new(name=bone)
    for v in obj.data.vertices:
        d = min((v.co - e).length for e in ends)
        if d >= reach:
            continue
        w = 1 - d / reach
        w = w * w * (3 - 2 * w)
        for g in v.groups:
            groups[g.group].add([v.index], g.weight * (1 - w), 'REPLACE')
        target.add([v.index], w, 'REPLACE')


def mark_details(obj):
    """A face attribute: 1 on small details that the farthest level of detail drops, 2 on the finest (laces,
    eyelets), dropped from the middle level on."""
    attr = obj.data.attributes.new("wg_detail", 'INT', 'FACE')
    flag = 2 if obj.get("wg_fine") else 1 if any(k in obj.name for k in DETAILS) else 0
    for i in range(len(obj.data.polygons)):
        attr.data[i].value = flag


def unwrap_atlas(obj, face_materials):
    """One UV layout for the whole being; the face's islands are given more room, so its strokes stay sharp."""
    mesh = obj.data
    atlas = mesh.uv_layers.new(name="Atlas")
    mesh.uv_layers.active = atlas
    bpy.context.view_layer.objects.active = obj
    for o in bpy.context.selected_objects:
        o.select_set(False)
    obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.003, area_weight=0.0, correct_aspect=True, scale_to_bounds=False)
    bm = bmesh.from_edit_mesh(mesh)
    uv = bm.loops.layers.uv["Atlas"]
    face_slots = {i for i, s in enumerate(obj.material_slots) if s.material and s.material.name in face_materials}
    loops = [l for f in bm.faces if f.material_index in face_slots for l in f.loops]
    if loops:
        c = sum((l[uv].uv for l in loops), Vector((0, 0))) / len(loops)
        for l in loops:
            l[uv].uv = c + (l[uv].uv - c) * 3.0
    bmesh.update_edit_mesh(mesh)
    bpy.ops.uv.select_all(action='SELECT')
    bpy.ops.uv.pack_islands(rotate=True, margin=0.003)
    bpy.ops.object.mode_set(mode='OBJECT')


def bake_atlas(obj, materials, recipes, face_images, path):
    """Bakes every material's appearance into one image on the Atlas layout."""
    import numpy as np
    import painting
    image = bpy.data.images.new(os.path.basename(path), ATLAS, ATLAS, alpha=False)
    image.pixels.foreach_set(np.tile(np.array([0.32, 0.25, 0.21, 1.0], dtype=np.float32), ATLAS * ATLAS))
    for slot in obj.material_slots:
        mat = slot.material
        name = mat.name
        kind = materials[name]["kind"]
        if kind == "painted":
            g = painting.build_painting(mat, recipes[name])
            nodes = g.nodes
        else:
            mat.use_nodes = True
            nodes = mat.node_tree.nodes
            nodes.clear()
            emit = nodes.new("ShaderNodeEmission")
            out = nodes.new("ShaderNodeOutputMaterial")
            mat.node_tree.links.new(emit.outputs["Emission"], out.inputs["Surface"])
            if kind == "skin":
                uvn = nodes.new("ShaderNodeUVMap")
                uvn.uv_map = "UVMap"
                tex = nodes.new("ShaderNodeTexImage")
                tex.image = face_images[name]
                tex.extension = 'EXTEND'
                mat.node_tree.links.new(uvn.outputs["UV"], tex.inputs["Vector"])
                mat.node_tree.links.new(tex.outputs["Color"], emit.inputs["Color"])
            else:
                emit.inputs["Color"].default_value = (*[c ** 2.2 for c in materials[name]["colour"]], 1)
        target = nodes.new("ShaderNodeTexImage")
        target.image = image
        nodes.active = target
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 16
    prefs = bpy.context.preferences.addons["cycles"].preferences
    for kind in ("OPTIX", "CUDA"):
        try:
            prefs.compute_device_type = kind
            prefs.get_devices()
            for device in prefs.devices:
                device.use = True
            scene.cycles.device = 'GPU'
            break
        except TypeError:
            continue
    obj.data.uv_layers.active = obj.data.uv_layers["Atlas"]
    for o in bpy.context.selected_objects:
        o.select_set(False)
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.bake(type='EMIT', margin=10, use_clear=False)
    scene.view_settings.view_transform = 'Standard'
    scene.view_settings.look = 'None'
    settings = scene.render.image_settings
    settings.file_format = 'PNG'
    settings.color_mode = 'RGB'
    image.save_render(path, scene=scene)
    return image


def finish_materials(obj, name, materials, atlas_image):
    """After the bake: one material for the being (the atlas) and one for glowing glass; one UV layer."""
    body = bpy.data.materials.new(f"Miner_{name}")
    body.use_nodes = True
    bsdf = body.node_tree.nodes["Principled BSDF"]
    tex = body.node_tree.nodes.new("ShaderNodeTexImage")
    tex.image = atlas_image
    body.node_tree.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
    glow = bpy.data.materials.get("Glow") or bpy.data.materials.new("Glow")
    glowing = {i for i, s in enumerate(obj.material_slots) if materials[s.material.name]["kind"] == "glow"}
    # Read which faces glow before the slots change (clearing the slots resets every face's index).
    lit = [poly.material_index in glowing for poly in obj.data.polygons]
    obj.data.materials.clear()
    if glowing:
        # The lamp glass first and the body last: the engine draws an extra material (the outline) with the
        # mesh's last part, which must be the body.
        obj.data.materials.append(glow)
    obj.data.materials.append(body)
    for poly, on in zip(obj.data.polygons, lit):
        poly.material_index = 0 if on or not glowing else 1
    if glowing:
        # The glass's faces first too: Unity orders a mesh's parts by first use, and the body must come last.
        for o in bpy.context.selected_objects:
            o.select_set(False)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode='EDIT')
        bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.mesh.sort_elements(type='MATERIAL', elements={'FACE'})
        bpy.ops.object.mode_set(mode='OBJECT')
    uv = obj.data.uv_layers
    uv.remove(uv["UVMap"])
    uv["Atlas"].name = "UVMap"


def lod_copy(obj, name, budget, drop=2):
    """A lighter copy. drop: details of this level and above are left out (2 the finest only, 1 all small details)."""
    copy = obj.copy()
    copy.data = obj.data.copy()
    copy.name = copy.data.name = name
    bpy.context.scene.collection.objects.link(copy)
    bm = bmesh.new()
    bm.from_mesh(copy.data)
    layer = bm.faces.layers.int.get("wg_detail")
    bmesh.ops.delete(bm, geom=[f for f in bm.faces if f[layer] >= drop], context='FACES')
    bm.to_mesh(copy.data)
    bm.free()
    simplify_to(copy, budget / max(triangles(copy), 1))
    return copy


def prepare(name, b, parts):
    """The skeleton, and the parts skinned and simplified to the nearest level's budget, still apart (as the
    audit measures them)."""
    import re
    rig, bones = armature(b, f"Miner_{name}_Rig")
    meshes = [o for o in parts if o.type == 'MESH']
    for o in meshes:
        skin(o, b, bones)
        mark_details(o)
    # What lies on something takes its weights from it.
    for o in meshes:
        for pattern, sources in LIKE:
            m = re.search(pattern, o.name)
            if not m:
                continue
            number = m.group(1) if m.groups() and m.group(1) else ""
            wanted = [re.compile(b.name + s.replace("\\1", number) + "$") for s in sources]
            found = [s for s in meshes if s is not o and any(w.match(s.name) for w in wanted)]
            if found:
                weights_like(o, found, Vector(list(o["wg_anchor"])) if o.get("wg_anchor") else None)
            break
        # A strap's ends go with what they are tied to (a bag that swings on its own bone).
        if o.get("wg_ends") and o.get("wg_end_bone") in bones:
            flat = list(o["wg_ends"])
            ends_follow(o, [Vector(flat[k:k + 3]) for k in range(0, len(flat), 3)], o["wg_end_bone"])
    # Thin exact parts (laces, rings, wire) keep their shape; the rest shares what is left of the budget.
    exact = sum(triangles(o) for o in meshes if o.get("wg_exact"))
    total = sum(triangles(o) for o in meshes if not o.get("wg_exact"))
    ratio = max(LOD_BUDGETS[0] - exact, 1000) / total
    for o in meshes:
        if not o.get("wg_exact"):
            simplify_to(o, ratio)
        limit_influences(o)
    return rig, bones, meshes


def limit_influences(obj, limit=4):
    """At most four bones a vertex, as the game skins (simplifying blends the weights of merged vertices)."""
    groups = obj.vertex_groups
    for v in obj.data.vertices:
        ws = sorted(((g.weight, g.group) for g in v.groups if g.weight > 0), reverse=True)
        if len(ws) <= limit:
            continue
        keep = ws[:limit]
        total = sum(w for w, _ in keep)
        for w, g in ws[limit:]:
            groups[g].remove([v.index])
        for w, g in keep:
            groups[g].add([v.index], w / total, 'REPLACE')


def build(name, b, parts, materials, recipes, face_images, out_dir, audit=None):
    """Skins, simplifies, joins, bakes and exports one being. Returns its dimensions for the game.
    audit: called with (b, meshes, bones) on the parts before they are joined."""
    rig, bones, meshes = prepare(name, b, parts)
    if audit is not None:
        audit(b, meshes, bones)
    lod0 = shapes.join(f"Miner_{name}_LOD0", meshes)
    face_materials = {n for n, m in materials.items() if m["kind"] == "skin"}
    unwrap_atlas(lod0, face_materials)
    atlas_path = os.path.join(out_dir, f"Miner_{name}_Atlas.png")
    atlas = bake_atlas(lod0, materials, recipes, face_images, atlas_path)
    finish_materials(lod0, name, materials, atlas)
    lod1 = lod_copy(lod0, f"Miner_{name}_LOD1", LOD_BUDGETS[1], drop=2)
    lod2 = lod_copy(lod0, f"Miner_{name}_LOD2", LOD_BUDGETS[2], drop=1)
    for lod in (lod0, lod1, lod2):
        lod.parent = rig
        mod = lod.modifiers.new("Rig", 'ARMATURE')
        mod.object = rig
    export(rig, [lod0, lod1, lod2], os.path.join(out_dir, f"Miner_{name}.fbx"))
    print(f"RIGGED {name}: LOD0 {triangles(lod0)}, LOD1 {triangles(lod1)}, LOD2 {triangles(lod2)} triangles; {len(bones)} bones")
    return dimensions(b, bones), [rig, lod0, lod1, lod2]


def export(rig, lods, path):
    for o in bpy.context.selected_objects:
        o.select_set(False)
    rig.select_set(True)
    for o in lods:
        o.select_set(True)
    bpy.context.view_layer.objects.active = rig
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'ARMATURE', 'MESH'},
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=False,
                             axis_forward='-Z', axis_up='Y', add_leaf_bones=False, primary_bone_axis='Y', secondary_bone_axis='X',
                             armature_nodetype='NULL', use_armature_deform_only=True, bake_anim=False,
                             mesh_smooth_type='OFF', use_mesh_modifiers=False, path_mode='STRIP')


# ---------------------------------------------------------------- dimensions for the procedural body

def dimensions(b, bones):
    """The rest pose's measurements in the game's body space: x to the being's right, y up, z forward.
    The procedural biped solves with these, so its joints land where the model's bones are."""
    hips = bones["Pelvis"][0]
    waist = b.waist
    thigh = (bones["Thigh.L"][1] - bones["Thigh.L"][0]).length
    shin = (bones["Shin.L"][1] - bones["Shin.L"][0]).length
    upper = (bones["UpperArm.L"][1] - bones["UpperArm.L"][0]).length
    fore = (bones["Forearm.L"][1] - bones["Forearm.L"][0]).length
    F = b.p.get("foot", 0.165) * b.H
    shoulder = b.shoulders[1] - waist
    return dict(
        height=b.H,
        hipHeight=hips.z,
        hipWidth=abs(b.hips[1].x - hips.x),
        leg=(thigh + shin) * 0.5,
        ankleHeight=b.ankles[1].z,
        heelLength=F * 0.3,
        ballLength=F * 0.35,
        toeLength=F * 0.23,
        waistRise=waist.z - hips.z,
        # The left shoulder's offset from the waist, as the biped's right one mirrored: (out, up, forward).
        shoulder=[abs(shoulder.x), shoulder.z, -shoulder.y],
        headRise=(bones["Head"][0] - waist).z,
        upperArm=upper,
        forearm=fore,
        # Where a relaxed hand hangs from its shoulder in the game (out, down, forward): close to the body, the
        # elbow just clear of the clothes at the chest and waist, and a little bent. (The rest pose holds the arms
        # wider, only so that skinning keeps arm and body apart.)
        **hang(b, upper, fore),
    )


def hang(b, upper, fore):
    length = upper + fore
    clear = max(b.trunk["waist"][0], b.trunk["chest"][0]) + 0.03 + b.p["arm"] * 1.3
    out = max(0.0, clear - abs(b.shoulders[1].x - b.chest.x))
    tilt = max(0.12, min(0.6, out / upper * 1.05))
    side = length * tilt
    # Relaxed arms bend a little at the elbow, the hands coming slightly forward; the rounder the body, the more.
    forward = length * (0.05 + 0.35 * max(0.0, tilt - 0.2))
    return dict(armOut=side, armDrop=math.sqrt(max(length * length - side * side - forward * forward, 0.0)) * 0.95, armForward=forward)
