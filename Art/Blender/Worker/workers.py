"""Wonder Gather — the miners (S1d): three characters built from modules.

Luis chose these three after the first concepts (Small, then Long, then Round)
and asked for much higher model quality, hints that they are miners, and a
build that grows into a full character creator
(Docs/Correspondence/2026-10-02_WORKER_CONCEPTS_FEEDBACK.md).

A character here is a preset: a body (body.py), a painted face (faces.py), a
hairstyle (hair.py), and a list of garments and accessories (outfits.py), each
with its own parameters and materials. Every module fits any body, so a
creator can mix them freely; these presets are three good starting points.

Cloth, leather, wood and metal are painted by hand-painting recipes and baked
to textures (../OrdinaryPlace/painting.py). Faces are painted strokes. A
manifest tells the engine how each material is drawn.

Run (Blender 4.4+):
    blender -b --factory-startup --python workers.py -- --out <folder> [--fbx] [--paint] [--preview <png>]
The folder receives Workers.fbx, Face_<Character>.png, the painted textures and workers.json.
"""
import argparse
import json
import math
import os
import sys

import bpy
from mathutils import Euler, Vector

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path[:0] = [HERE, os.path.join(HERE, "..", "OrdinaryPlace")]

import body  # noqa: E402
import faces  # noqa: E402
import hair  # noqa: E402
import outfits  # noqa: E402
import shapes  # noqa: E402

# ---------------------------------------------------------------- materials

# Colours are sRGB painter picks. kind: "painted" (baked texture), "skin" (painted face), "glow" (emissive).
MATERIALS = {
    "Skin_Small": dict(kind="skin", colour=(0.88, 0.67, 0.56)),
    "Skin_Long": dict(kind="skin", colour=(0.80, 0.60, 0.49)),
    "Skin_Round": dict(kind="skin", colour=(0.84, 0.61, 0.49)),
    "Hair": dict(kind="painted", colour=(0.11, 0.08, 0.08), recipe=dict(motif="none", strokes=(1, 1, 6), stroke_scale=14, edge=(1.9, 1.75, 1.6), gloss=0.25)),
    "HairBrown": dict(kind="painted", colour=(0.27, 0.16, 0.10), recipe=dict(motif="none", strokes=(1, 1, 6), stroke_scale=14, edge=(1.5, 1.35, 1.2), gloss=0.2)),
    "CoatBlue": dict(kind="painted", colour=(0.19, 0.22, 0.34), recipe=dict(stains=0.35)),
    "Coat": dict(kind="painted", colour=(0.24, 0.175, 0.14), recipe=dict(stains=0.35)),
    "Smock": dict(kind="painted", colour=(0.38, 0.44, 0.29), recipe=dict(stains=0.3)),
    "Shirt": dict(kind="painted", colour=(0.80, 0.75, 0.64), recipe=dict()),
    "Trousers": dict(kind="painted", colour=(0.34, 0.28, 0.22), recipe=dict(stains=0.5)),
    "TrousersGrey": dict(kind="painted", colour=(0.37, 0.35, 0.33), recipe=dict(stains=0.5)),
    "Patch": dict(kind="painted", colour=(0.42, 0.33, 0.23), recipe=dict()),
    "Boots": dict(kind="painted", colour=(0.30, 0.20, 0.13), recipe=dict(stains=0.6, stain_top=0.2, edge=(1.6, 1.45, 1.25), gloss=0.15)),
    "Sole": dict(kind="painted", colour=(0.13, 0.10, 0.09), recipe=dict()),
    "Lace": dict(kind="colour", colour=(0.58, 0.47, 0.33)),
    "Leather": dict(kind="painted", colour=(0.47, 0.30, 0.18), recipe=dict(edge=(1.5, 1.35, 1.15), gloss=0.15)),
    "ApronLeather": dict(kind="painted", colour=(0.42, 0.28, 0.18), recipe=dict(stains=0.3, edge=(1.45, 1.3, 1.1), gloss=0.12)),
    "Accent": dict(kind="painted", colour=(0.66, 0.28, 0.16), recipe=dict(edge=(1.4, 1.25, 1.1), gloss=0.12)),
    "Brass": dict(kind="painted", colour=(0.70, 0.53, 0.25), recipe=dict(edge=(1.6, 1.5, 1.3), gloss=0.5)),
    "Iron": dict(kind="painted", colour=(0.26, 0.26, 0.27), recipe=dict(edge=(2.0, 1.9, 1.8), gloss=0.4)),
    "Wood": dict(kind="painted", colour=(0.52, 0.36, 0.22), recipe=dict(motif="wood", strokes=(1, 1, 8), stroke_scale=6, daub=0.5)),
    "Button": dict(kind="colour", colour=(0.47, 0.33, 0.20)),
    "Mug": dict(kind="painted", colour=(0.64, 0.66, 0.68), recipe=dict(edge=(1.4, 1.4, 1.4), gloss=0.4)),
    "Glass": dict(kind="glow", colour=(1.0, 0.78, 0.45), emission=(1.0, 0.72, 0.38)),
}


def recipe_for(name):
    m = MATERIALS[name]
    # The bake paints in linear light and saves sRGB, so the base goes in linear; the gaps between
    # pieces are filled with the colour itself, so distant mipmaps keep it.
    r = dict(base=linear(m["colour"]), fill=m["colour"], size=1024, motif="none", strokes=(1, 6, 1), stroke_scale=12.0,
             warm=(1.07, 1.0, 0.9), cool=(0.86, 0.9, 1.0), cavity=(0.55, 0.5, 0.56), edge=(1.25, 1.15, 1.05), ao=0.06, bevel=0.006,
             scale=5.0, daub=0.45, stain_top=0.35)
    r.update({k: v for k, v in m.get("recipe", {}).items() if k != "gloss"})
    return r


def linear(c):
    return tuple(x ** 2.2 for x in c)


for _name, _m in MATERIALS.items():
    shapes.PREVIEW[_name] = linear(_m["colour"])


# ---------------------------------------------------------------- the three miners

def at_hip(out, forward, up):
    return lambda b, s: b.pelvis + Vector((s * (b.width("pelvis") + out), forward, up))


def on_hip(loose, up=-0.03, back=0.015, palm=0.024):
    """A wrist resting a palm's thickness outside the garment at the hips (whose looseness is loose)."""
    def at(b, s):
        z = b.waist.z + up
        w, d = outfits.trunk_at(b, z, loose)
        x, y = outfits.axis_at(b, z)
        return Vector((x + s * (w + palm), y + back, z))
    return at


PRESETS = {
    # Small: young and small, curious, ready to set out. A dark bob, big glancing eyes, an oversized coat,
    # a satchel, and a miner's lantern in hand.
    "Small": dict(
        seed=21,
        body=dict(height=1.42, head=0.13, head_shape=(1.04, 1.0, 1.0), hip=0.45, waist=0.57, chest=0.68, collar=0.075,
                  shoulder_w=0.145, hip_w=0.068, shoulder_h=0.035, upper_arm=0.165, forearm=0.14, hand=0.105, foot=0.17,
                  trunk=dict(pelvis=(0.115, 0.085), waist=(0.105, 0.078), chest=(0.12, 0.085), collar=(0.072, 0.058)),
                  arm=0.033, leg=0.041, neck_r=0.45,
                  nose=[((0, -1.0, -0.3), 0.085, (1.0, 1.0, 0.9))], cheeks=0.3, jaw=0.62, chin=0.24,
                  pose=dict(weight=-1, hip_shift=0.02, hip_tilt=0.012, shoulder_tilt=0.015, turn=0.22, nod=-0.2, tilt=0.16, free_foot=(1.7, -0.07)),
                  arms=[dict(wrist=at_hip(0.07, -0.1, -0.04), pole=(1, 0.6, -0.1)),
                        dict(wrist=lambda b, s: b.chest + Vector((s * 0.045, -b.depth("chest") - 0.075, 0.0)), pole=(1, 0.2, -1.0))]),
        hands=[dict(grip="grip"), dict(grip="grip", palm=(0, 1, 0), along=(-0.3, 0.1, 1))],
        face=dict(style="curious", skin="Skin_Small", soot=[((0.52, -0.28), 0.1, 0.3, 0.6)]),
        hair=dict(style="bob", mat="Hair"),
        outfit=[("top", dict(mat="CoatBlue", loose=0.024, sleeve=1.07, cuff=1.45, skirted=True)),
                ("trousers", dict(mat="TrousersGrey", boot=0.13)),
                ("skirt", dict(mat="CoatBlue", hem=0.31, flare=1.38, loose=0.026, folds=0.07, fold_count=7, ragged=0.012)),
                ("collar", dict(mat="CoatBlue", height=0.085, wide=2.5)),
                ("buttons", dict(mat="Button", count=3, loose=0.026)),
                ("boots", dict(shaft=0.13, cuff=1)),
                ("satchel", dict()),
                ("lantern", dict(hand=0))],
    ),
    # Long: tall and thin, unhurried, a little melancholy. A long nose, sleepy kind eyes, a long patched coat,
    # a mug of tea in one hand and a pickaxe over the shoulder.
    "Long": dict(
        seed=11,
        body=dict(height=1.86, head=0.112, head_shape=(0.94, 1.0, 1.14), hip=0.52, waist=0.615, chest=0.735, collar=0.07,
                  shoulder_w=0.165, hip_w=0.082, shoulder_h=0.035, upper_arm=0.175, forearm=0.15, hand=0.1, foot=0.155,
                  trunk=dict(pelvis=(0.125, 0.09), waist=(0.115, 0.08), chest=(0.14, 0.095), collar=(0.085, 0.065)),
                  arm=0.036, leg=0.046, neck_r=0.48,
                  nose=[((0, -0.92, -0.08), 0.11, (0.75, 1.0, 1.6)), ((0, -1.12, -0.25), 0.11, (0.8, 2.0, 0.9)), ((0, -1.3, -0.36), 0.1, (0.9, 1.0, 1.0))],
                  cheeks=0.14, jaw=0.62, chin=0.27, chin_drop=1.1,
                  pose=dict(weight=1, hip_shift=0.035, hip_tilt=0.014, shoulder_tilt=0.03, stoop=0.7, tilt=-0.16, turn=-0.18, nod=0.22, free_foot=(1.6, -0.08)),
                  arms=[dict(wrist=lambda b, s: b.pelvis + Vector((s * (b.width("pelvis") + 0.16), -0.1, -0.12)), pole=(1, 0.5, -0.3)),
                        dict(wrist=lambda b, s: b.waist + Vector((s * 0.05, -b.depth("waist") - 0.2, 0.1)), pole=(1, 0.5, -0.6))]),
        hands=[dict(grip="grip", along=(0.2, -1, 0.0), ring=lambda b, i: b.wrists[i] - Vector((b.wrists[i].x * 1.25, b.wrists[i].y * 1.4, 0))),
               dict(grip="hold", palm=(-1, 0, 0), along=(-0.6, -0.6, 0.3))],
        face=dict(style="sleepy", skin="Skin_Long"),
        hair=dict(style="swept", mat="Hair"),
        outfit=[("top", dict(mat="Coat", loose=0.016, sleeve=1.0, cuff=1.3, skirted=True)),
                ("trousers", dict(mat="Trousers", boot=0.19)),
                ("skirt", dict(mat="Coat", hem=0.26, flare=1.3, loose=0.018, open_front=True, folds=0.06, fold_count=6, ragged=0.03)),
                ("shirt_front", dict(mat="Shirt", loose=0.016)),
                ("lapels", dict(mat="Coat", loose=0.016)),
                ("collar", dict(mat="Coat", height=0.07, wide=2.0)),
                ("patch", dict(mat="Patch", onto="Top", where=lambda b: (b.chest + Vector((0.075, 0, -0.12))), size=(0.05, 0.005, 0.045), angle=0.2, name="Patch0")),
                ("patch", dict(mat="Patch", onto="Skirt", where=lambda b: (b.pelvis + Vector((-0.14, 0, -0.22))), size=(0.045, 0.005, 0.055), angle=-0.12, name="Patch1")),
                ("boots", dict(shaft=0.19)),
                ("pickaxe", dict(hand=0)),
                ("mug", dict(hand=1))],
    ),
    # Round: short and heavy-set, sure of the work ahead. A bulb nose, laughing eyes, fists on the hips,
    # a smock with rolled sleeves, a leather apron with a hammer, and a miner's lamp cap.
    "Round": dict(
        seed=3,
        body=dict(height=1.56, head=0.128, head_shape=(1.08, 1.0, 0.98), hip=0.47, waist=0.59, chest=0.72, collar=0.092,
                  shoulder_w=0.19, hip_w=0.095, shoulder_h=0.055, neck_drop=0.03, upper_arm=0.16, forearm=0.14, hand=0.115, foot=0.17,
                  trunk=dict(pelvis=(0.17, 0.13), waist=(0.185, 0.15), chest=(0.175, 0.13), collar=(0.1, 0.08)),
                  arm=0.045, leg=0.058, neck_r=0.6,
                  nose=[((0, -0.95, -0.12), 0.12, (0.8, 1.0, 1.4)), ((0, -1.1, -0.3), 0.22, (1.05, 0.95, 0.95))],
                  cheeks=0.34, jaw=0.74, chin=0.3,
                  pose=dict(weight=-1, hip_shift=0.03, hip_tilt=0.014, shoulder_tilt=0.025, back=1.0, tilt=0.14, turn=0.12, nod=-0.08, free_foot=(1.8, -0.06)),
                  arms=[dict(wrist=on_hip(0.018), pole=(1, 0.8, 0.3)), dict(wrist=on_hip(0.018), pole=(1, 0.8, 0.3))]),
        # Hands on hips: palms against the hips, fingers down and back, thumbs forward.
        hands=[dict(grip="open", palm=(1, 0, 0), along=(0, 0.5, -1)), dict(grip="open", palm=(-1, 0, 0), along=(0, 0.5, -1))],
        face=dict(style="laughing", skin="Skin_Round", soot=[((-0.55, -0.18), 0.09, 0.25, 0.6)]),
        hair=dict(style="curls", mat="HairBrown", top=0.45),
        outfit=[("top", dict(mat="Smock", loose=0.018, rolled=True, skirted=True)),  # the roll stays clear of the hands
                ("trousers", dict(mat="Trousers", boot=0.13)),
                ("skirt", dict(mat="Smock", hem=0.37, flare=1.15, loose=0.02, folds=0.05, fold_count=8)),
                ("forearms", dict()),
                ("neckband", dict(mat="Smock")),
                ("boots", dict(shaft=0.13)),
                ("apron", dict(mat="ApronLeather", tie="Leather", hem=0.33, flare=1.22)),
                ("patch", dict(mat="Leather", onto="Apron", where=lambda b: (b.pelvis + Vector((-0.07, 0, 0.0))), size=(0.055, 0.006, 0.05), name="ApronPocket")),
                ("hammer", dict(pocket="ApronPocket", where=lambda b: b.pelvis + Vector((-0.08, -b.depth("pelvis") - 0.085, 0.08)), tilt=(0.25, -0.15, 1))),
                ("lamp_cap", dict())],
    ),
}


# ---------------------------------------------------------------- building

def build_character(name, preset, out_dir):
    b = body.Body(f"Worker_{name}", preset["body"])
    face = preset["face"]
    skin_name = face["skin"]
    image = faces.paint(face["style"], MATERIALS[skin_name]["colour"], preset["seed"], os.path.join(out_dir, f"Face_{name}.png"),
                        f"Face_{name}", face.get("soot", ()))
    skin = shapes.material(skin_name, image=image)
    objs = [body.head(b, skin)]
    held = []
    for i, h in enumerate(preset["hands"]):
        obj, grasp, axes = body.hand(b, i, skin, h["grip"], h.get("palm"), h.get("along"), h.get("ring"))
        objs.append(obj)
        held.append((grasp, axes))
    hstyle = preset["hair"]
    objs.append(hair.grow(b, hstyle["style"], hstyle["mat"], preset["seed"], **{k: v for k, v in hstyle.items() if k not in ("style", "mat")}))
    for piece, opts in preset["outfit"]:
        objs.extend(dress(b, piece, dict(opts), held, skin))
    return b, objs


def dress(b, piece, o, held, skin):
    if piece == "top":
        return outfits.top(b, o.pop("mat"), **o)
    if piece == "skirt":
        top = next((x for x in bpy.data.objects if x.name == f"{b.name}_Top"), None)
        under = [x for x in bpy.data.objects if x.name == f"{b.name}_Trousers"]
        return outfits.skirt(b, o.pop("mat"), fit=top, under=under, **o)
    if piece == "trousers":
        return outfits.trousers(b, o.pop("mat"), **o)
    if piece == "neckband":
        garment = next(x for x in bpy.data.objects if x.name == f"{b.name}_Top")
        return outfits.neckband(b, o.pop("mat"), garment, **o)
    if piece == "collar":
        return outfits.collar(b, o.pop("mat"), **o)
    if piece == "lapels":
        return outfits.lapels(b, o.pop("mat"), **o)
    if piece == "shirt_front":
        return outfits.shirt_front(b, o.pop("mat"), **o)
    if piece == "buttons":
        return outfits.buttons(b, o.pop("mat"), **o)
    if piece == "patch":
        onto = next((x for x in bpy.data.objects if x.name == f"{b.name}_{o['onto']}"), None) if "onto" in o else None
        return outfits.patch(b, o["mat"], o["where"](b), o["size"], o.get("angle", 0.0), o.get("name", f"Patch{id(o) % 997}"), onto)
    if piece == "apron":
        under = [x for x in bpy.data.objects if x.name in (f"{b.name}_Top", f"{b.name}_Skirt")]
        return outfits.apron(b, o["mat"], o["tie"], hem=o.get("hem", 0.33), flare=o.get("flare", 1.2), over=under)
    if piece == "boots":
        return body.boot(b, 0, "Boots", "Sole", **o) + body.boot(b, 1, "Boots", "Sole", **o)
    if piece == "forearms":
        return [body.forearm(b, i, skin) for i in (0, 1)]
    if piece == "satchel":
        return outfits.satchel(b, "Leather", "Accent", "Brass")
    if piece == "lantern":
        return outfits.lantern(b, held[o["hand"]][0], "Brass", "Glass")
    if piece == "pickaxe":
        # The handle runs through the fist, along its grip; he leans on it like a walking stick.
        grasp, (a, t, n) = held[o["hand"]]
        d = t if t.z > 0 else -t
        hold = o.get("hold", 0.9)
        return outfits.pickaxe(b, grasp, d, "Wood", "Iron", length=(grasp.z - 0.01) / max(d.z, 0.3) / hold, hold=hold)
    if piece == "mug":
        grasp, (a, t, n) = held[o["hand"]]
        return outfits.mug(b, grasp + n * 0.03 + Vector((0, 0, 0.01)), "Mug")
    if piece == "hammer":
        # In the apron's pocket when there is one, its head showing above.
        at = getattr(b, "marks", {}).get(o.get("pocket"))
        at = at + Vector((0.0, -0.012, 0.05)) if at is not None else o["where"](b)
        return outfits.hammer(b, at, o["tilt"], "Wood", "Iron")
    if piece == "lamp_cap":
        return outfits.lamp_cap(b, "Leather", "Brass", "Glass")
    raise ValueError(piece)


def build(out_dir, only=None):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    built = {}
    for name, preset in PRESETS.items():
        if only and name not in only:
            continue
        b, objs = build_character(name, preset, out_dir)
        built[name] = objs
        print(f"MINER {name}: {len(objs)} parts, {sum(len(o.data.polygons) for o in objs)} faces")
    return built


def paint(built, out_dir):
    """Bakes the hand-painted textures for every painted material (not the faces or glass)."""
    import painting
    for name, m in MATERIALS.items():
        if m["kind"] == "painted":
            painting.recipe(name, **recipe_for(name))
    objs = [o for objs in built.values() for o in objs
            if o.type == 'MESH' and o.material_slots and o.material_slots[0].material
            and MATERIALS.get(o.material_slots[0].material.name, {}).get("kind") == "painted"]
    # The miners are built on one spot; apart while baking, so one's cavities are not shaded by another's body.
    for k, group in enumerate(built.values()):
        for o in group:
            o.location.x += k * 4.0
    painted = painting.paint(objs, out_dir, samples=32)
    for k, group in enumerate(built.values()):
        for o in group:
            o.location.x -= k * 4.0
    return painted


def manifest(out_dir, painted):
    """How the engine draws each material: a painted texture, a painted face, a plain colour, or a glow."""
    entries = []
    for name, m in MATERIALS.items():
        entry = dict(name=name, kind=m["kind"], colour=list(m["colour"]), gloss=m.get("recipe", {}).get("gloss", 0.0))
        if name in painted:
            entry["texture"] = os.path.basename(painted[name])
        if m["kind"] == "skin":
            entry["texture"] = f"Face_{name.split('_')[1]}.png"
        if m["kind"] == "glow":
            entry["emission"] = list(m["emission"])
        entries.append(entry)
    with open(os.path.join(out_dir, "workers.json"), "w", encoding="utf-8") as f:
        json.dump(dict(characters=list(PRESETS), materials=entries), f, indent=2)


def export_fbx(path):
    bpy.ops.export_scene.fbx(filepath=path, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                             bake_space_transform=True, axis_forward='-Z', axis_up='Y', object_types={'MESH'},
                             use_mesh_modifiers=True, mesh_smooth_type='OFF', colors_type='NONE',
                             add_leaf_bones=False, bake_anim=False, path_mode='STRIP')


def preview(built, path):
    """The three side by side, front and three-quarter, then close views of each face and hands."""
    scene = bpy.context.scene
    from mathutils import Matrix
    for k, (name, objs) in enumerate(built.items()):
        for o in objs:
            base = o.matrix_world.copy()
            o.matrix_world = Matrix.Translation((k * 0.9, 0, 0)) @ base
            d = o.copy()
            scene.collection.objects.link(d)
            d.matrix_world = Matrix.Translation((k * 0.9 + 2.9, 0, 0)) @ Matrix.Rotation(math.radians(-40), 4, 'Z') @ base
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 32
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
    scene.render.resolution_x, scene.render.resolution_y = 1900, 760
    cam = shapes.link(bpy.data.objects.new("Preview", bpy.data.cameras.new("Preview")))
    cam.data.type = 'ORTHO'
    cam.data.ortho_scale = 5.9
    cam.location = (2.75, -8, 0.98)
    cam.rotation_euler = Euler((math.radians(90), 0, 0))
    scene.camera = cam
    for name, rot, energy, color in (("Key", (50, 0, -35), 3.5, (1.0, 0.85, 0.7)), ("Fill", (70, 0, 140), 1.0, (0.6, 0.7, 1.0))):
        light = shapes.link(bpy.data.objects.new(name, bpy.data.lights.new(name, 'SUN')))
        light.rotation_euler = Euler([math.radians(a) for a in rot])
        light.data.energy = energy
        light.data.color = color
    world = bpy.data.worlds.new("World")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.22, 0.24, 0.32, 1)
    scene.world = world
    scene.view_settings.view_transform = 'Standard'
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    cam.data.type = 'PERSP'
    cam.data.lens = 85
    scene.render.resolution_x, scene.render.resolution_y = 640, 760
    for k, (name, objs) in enumerate(built.items()):
        head = next(o for o in objs if o.name.endswith("_Skin"))
        centre = sum((head.matrix_world @ Vector(c) for c in head.bound_box), Vector()) / 8
        cam.location = centre + Vector((0.4, -1.7, 0.02))
        cam.rotation_euler = (centre - cam.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = path.replace(".png", f"_{name.lower()}.png")
        bpy.ops.render.render(write_still=True)
        hands = [o for o in objs if "_Hand" in o.name]
        centre = sum((h.matrix_world @ Vector(c) for h in hands for c in h.bound_box), Vector()) / (8 * len(hands))
        cam.location = centre + Vector((0.3, -1.6, 0.25))
        cam.data.lens = 70
        cam.rotation_euler = (centre - cam.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = path.replace(".png", f"_{name.lower()}_hands.png")
        bpy.ops.render.render(write_still=True)
        cam.data.lens = 85


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--fbx", action="store_true")
    parser.add_argument("--paint", action="store_true")
    parser.add_argument("--preview")
    parser.add_argument("--only", nargs="*")
    args = parser.parse_args(argv)
    os.makedirs(args.out, exist_ok=True)
    built = build(args.out, args.only)
    painted = paint(built, args.out) if args.paint else {}
    if args.fbx:
        manifest(args.out, painted)
        export_fbx(os.path.join(args.out, "Workers.fbx"))
    if args.preview:
        preview(built, args.preview)
