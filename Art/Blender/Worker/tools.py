"""Wonder Gather — tools made for a body (S1d, the miners at work).

A pickaxe for each miner: the one Long carries on the back (outfits.pick), made at the size of the body that
will swing it. A tool is sized by the arms that hold it (the first pickaxe was made for arms of 0.87 m) and its
handle by the hand that closes on it.

The game's data for a tool (where the hands grip it, how thick the handle is there, where its head strikes) is
measured here on the model itself and written beside it (tools.json), so the data and the shape cannot drift
apart. The build also checks that the handle is one a hand can close on (hands.RADII).

Tool space, as the game has it: y up the handle towards the head, z the way the pick strikes, x across. The
origin is where the first pickaxe had it: on the handle, 0.45 of the tool's length above its foot.

Run (Blender 4.4+):
    blender -b --factory-startup --python tools.py -- --out <folder> [--only Small] [--report <folder>]
The folder receives Pickaxe_<Name>.fbx, Pickaxe_<Name>_Atlas.png and tools.json.
"""
import argparse
import json
import math
import os
import sys
import types

import bpy
from mathutils import Vector

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path[:0] = [HERE, os.path.join(HERE, "..", "OrdinaryPlace")]

import body  # noqa: E402
import hands  # noqa: E402
import outfits  # noqa: E402
import rigging  # noqa: E402
import shapes  # noqa: E402
import weights  # noqa: E402

# The first pickaxe (the 2.2 m test body's), which every other is a size of: the arms it was made for, and along
# its length (1 from foot to head) where the origin, the two grips and the head are.
ARMS = 0.87
ORIGIN, PRIMARY, SECONDARY, HEAD = 0.45, -0.20, 0.12, 0.55
# A handle's radius at the grips, against the length of the hand that holds it.
HANDLE = 0.1
ATLAS = 512
# Far away a pickaxe keeps this share of its triangles.
FAR = 0.3


def measure(name):
    """The body a tool is made for: its arm (shoulder to wrist) and its hand's length."""
    import workers
    preset = workers.rest_preset(name, workers.PRESETS[name])
    b = body.Body(f"Worker_{name}", preset["body"])
    return measured_on(b, rigging.joints(b))


def measured_on(b, bones):
    """The same, from a body already built."""
    arm = sum((bones[n][1] - bones[n][0]).length for n in ("UpperArm.L", "Forearm.L"))
    return arm, b.p.get("hand", 0.11) * b.H


def pickaxe(name, made_for=None, light=True):
    """A miner's pickaxe, at the origin in Blender's space: the handle up z, the long point towards -y (the
    being's front). made_for: the arm and the hand it is made for (measured from the miner's preset if not
    given). light: built with few faces, as the game carries it (a finer one is built to be weighed). Returns
    its parts, its size against the first pickaxe, and the hand's length."""
    arm, hand = made_for or measure(name)
    size = arm / ARMS
    owner = types.SimpleNamespace(name=f"Pickaxe_{name}")
    low, high = Vector((0, 0, (-ORIGIN) * size)), Vector((0, 0, HEAD * size))
    # outfits.pick's handle is 16.5 mm at its middle; this one is a tenth of the hand's length, and round, so
    # that the closed fingers lie on it all the way round.
    parts = outfits.pick(owner, low, high, (0, -1, 0), (1, 0, 0), "Wood", "Iron", bow=0.0, size=size * 1.1,
                         thick=hand * HANDLE / 0.0165, round_=True, light=light)
    return parts, size, hand


def weighed(name):
    """What a miner's pickaxe weighs, where its weight is and how hard it is to turn: measured on a fine copy of
    it (round where the game's is faceted), which is then put away."""
    parts, _, _ = pickaxe(name, light=False)
    for o in parts:
        o.name = o.name.replace("Pickaxe_", "Weighed_")
    data = weights.tool(parts)
    for o in parts:
        bpy.data.objects.remove(o)
    return data


def measured(parts, size):
    """The game's data, read from the model: the grips on the handle's axis, the handle's radius at each, and
    the striking head (the long point's end, and the radius of the ball the game sweeps there)."""
    handle = next(o for o in parts if o.name.endswith("_PickHandle"))
    head = next(o for o in parts if o.name.endswith("_PickHead"))

    def radius_at(z, reach=0.03):
        near = [math.hypot(v.co.x, v.co.y) for v in handle.data.vertices if abs(v.co.z - z) < reach * size / 0.6]
        if not near:
            raise RuntimeError("The handle has no surface at a grip.")
        return max(near), min(near)
    grips = []
    for along in (PRIMARY, SECONDARY):
        widest, narrowest = radius_at(along * size)
        grips.append(dict(at=along * size, radius=widest, narrowest=narrowest))
    # The long point: the head's farthest reach to the front.
    tip = min((v.co for v in head.data.vertices), key=lambda c: c.y)
    ball = max(0.012, 0.022 * size / 0.63)
    # The ball sits just inside the point, on the line from the handle's top to the point.
    top = Vector((0, 0, HEAD * size))
    centre = Vector(tip) + (top - Vector(tip)).normalized() * ball
    return grips, Vector(tip), centre, ball


def handle_radii(name, b, bones):
    """How thick this body's own pickaxe is where its hands grip it (the right hand's grip, then the left's):
    measured on the tool as built, which is then put away. A hand's closing is solved for exactly these."""
    parts, size, _ = pickaxe(name, measured_on(b, bones))
    grips = measured(parts, size)[0]
    for o in parts:
        bpy.data.objects.remove(o)
    return [g["radius"] for g in grips]


def weight_lines(weight, lower_grip):
    """The report's lines on a tool's weight."""
    return [f"  it weighs {weight['mass'] * 1000:.0f} g ({', '.join(f'{k} {v * 1000:.0f} g' for k, v in weight['pieces'].items())}); its weight is "
            f"{weight['centre'][1] * 1000:+.0f} mm along the handle from the origin, {(weight['centre'][1] - lower_grip) * 1000:.0f} mm above the lower grip, "
            f"{weight['centre'][2] * 1000:+.0f} mm towards the point",
            f"  to turn it about its centre: {', '.join(f'{v:.5f}' for v in weight['inertia'])} kg m2"]


def game(v):
    """Blender's space to the tool's in the game: x across, y up the handle, z the way it strikes."""
    return [round(-v[0], 5), round(v[2], 5), round(-v[1], 5)]


def picture(obj, path):
    """The finished tool as it will be painted, from its side and from three quarters."""
    scene = bpy.context.scene
    engines = {e.identifier for e in bpy.types.RenderSettings.bl_rna.properties['engine'].enum_items}
    scene.render.engine = 'BLENDER_EEVEE_NEXT' if 'BLENDER_EEVEE_NEXT' in engines else 'BLENDER_EEVEE'
    scene.render.resolution_x, scene.render.resolution_y = 520, 760
    cam = bpy.data.objects.new("ToolCam", bpy.data.cameras.new("ToolCam"))
    scene.collection.objects.link(cam)
    cam.data.lens, cam.data.clip_start = 70, 0.01
    scene.camera = cam
    for n, rot, energy in (("ToolKey", (55, 0, -30), 3.0), ("ToolFill", (70, 0, 150), 1.4)):
        light = bpy.data.objects.new(n, bpy.data.lights.new(n, 'SUN'))
        light.rotation_euler = tuple(math.radians(a) for a in rot)
        light.data.energy = energy
        scene.collection.objects.link(light)
    world = scene.world or bpy.data.worlds.new("ToolWorld")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.33, 0.35, 0.4, 1)
    scene.world = world
    scene.view_settings.view_transform = 'Standard'
    hidden = [o for o in scene.objects if o.type == 'MESH' and o is not obj]
    for o in hidden:
        o.hide_render = True
    low = min(v.co.z for v in obj.data.vertices)
    high = max(v.co.z for v in obj.data.vertices)
    middle = Vector((0, -0.02, (low + high) * 0.5))
    for k, d in enumerate(((1, 0, 0.05), (0.7, -0.7, 0.25))):
        cam.location = middle + Vector(d).normalized() * (high - low) * 2.6
        cam.rotation_euler = (middle - cam.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = f"{path}_{k}.png"
        bpy.ops.render.render(write_still=True)
    for o in hidden:
        o.hide_render = False


def build(name, out_dir, preview=None):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    import painting
    import workers
    weight = weighed(name)
    parts, size, hand = pickaxe(name)
    grips, tip, centre, ball = measured(parts, size)
    failing = []
    for label, g in zip(("primary", "secondary"), grips):
        if not hands.RADII[0] <= g["radius"] <= hands.RADII[-1]:
            failing.append(f"the handle at the {label} grip is {g['radius'] * 2000:.0f} mm thick: no hand closes on that")
        if g["radius"] - g["narrowest"] > 0.0008:
            failing.append(f"the handle at the {label} grip is not round ({g['narrowest'] * 2000:.1f} to {g['radius'] * 2000:.1f} mm)")
    materials = {n: workers.MATERIALS[n] for n in ("Wood", "Iron")}
    recipes = {n: workers.recipe_for(n) for n in materials}
    for n, r in recipes.items():
        painting.recipe(n, **r)
    obj = shapes.join(f"Pickaxe_{name}", [o for o in parts if o.type == 'MESH'])
    triangles = rigging.triangles(obj)
    rigging.unwrap_atlas(obj, set())
    atlas = rigging.bake_atlas(obj, materials, recipes, {}, os.path.join(out_dir, f"Pickaxe_{name}_Atlas.png"), size=ATLAS)
    one = bpy.data.materials.new(f"Pickaxe_{name}")
    one.use_nodes = True
    tex = one.node_tree.nodes.new("ShaderNodeTexImage")
    tex.image = atlas
    one.node_tree.links.new(tex.outputs["Color"], one.node_tree.nodes["Principled BSDF"].inputs["Base Color"])
    obj.data.materials.clear()
    obj.data.materials.append(one)
    uv = obj.data.uv_layers
    for layer in [l for l in uv if l.name != "Atlas"]:
        uv.remove(layer)
    uv["Atlas"].name = "UVMap"
    obj.name = obj.data.name = f"Pickaxe_{name}_LOD0"
    far = obj.copy()
    far.data = obj.data.copy()
    far.name = far.data.name = f"Pickaxe_{name}_LOD1"
    bpy.context.scene.collection.objects.link(far)
    rigging.simplify_to(far, FAR, floor=80)
    if preview:
        picture(obj, preview)
    for o in bpy.context.selected_objects:
        o.select_set(False)
    obj.select_set(True)
    far.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(filepath=os.path.join(out_dir, f"Pickaxe_{name}.fbx"), use_selection=True, object_types={'MESH'},
                             apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=False,
                             axis_forward='-Z', axis_up='Y', bake_anim=False, mesh_smooth_type='OFF', use_mesh_modifiers=False,
                             path_mode='STRIP')
    low = min(v.co.z for v in obj.data.vertices)
    high = max(v.co.z for v in obj.data.vertices)
    data = dict(
        name=name, size=round(size, 4), length=round(high - low, 4), triangles=triangles, far=rigging.triangles(far),
        # On the handle's axis: the right hand's grip (the lower), the left hand's, and each one's radius there.
        primaryGrip=[0.0, round(grips[0]["at"], 5), 0.0], secondaryGrip=[0.0, round(grips[1]["at"], 5), 0.0],
        gripRadii=[round(grips[0]["radius"], 5), round(grips[1]["radius"], 5)],
        # The ball the game sweeps for the strike, just inside the long point; and the point itself.
        head=game(centre), headRadius=round(ball, 5), tip=game(tip),
        # The model's own extent, for the game to check that it came in the right way up.
        foot=round(low, 5), top=round(high, 5),
        # What it weighs (kg), where its weight is, and how hard it is to turn about that point (kg m2, about its
        # three principal axes, and how those are turned): measured on the model (weights.py).
        mass=weight["mass"], centre=weight["centre"], inertia=weight["inertia"], inertiaTurn=weight["inertiaTurn"],
    )
    lines = [f"TOOL Pickaxe_{name}: {len(failing)} failing; made for arms of {size * ARMS:.3f} m and a hand of {hand * 1000:.0f} mm",
             f"  length {data['length'] * 1000:.0f} mm, {triangles} triangles (far away {data['far']})",
             f"  grips at {grips[0]['at'] * 1000:+.0f} and {grips[1]['at'] * 1000:+.0f} mm, the handle {grips[0]['radius'] * 2000:.1f} and {grips[1]['radius'] * 2000:.1f} mm thick there",
             f"  the point at {[round(c * 1000) for c in game(tip)]} mm; the strike's ball {ball * 1000:.0f} mm at {[round(c * 1000) for c in game(centre)]} mm"]
    lines += weight_lines(weight, grips[0]["at"])
    lines += [f"  FAIL {f}" for f in failing]
    print("\n".join(lines))
    return data, lines


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--only", nargs="*")
    parser.add_argument("--report", help="where the tools' report goes (default: Art/Review/Miners)")
    parser.add_argument("--preview", help="a folder for a picture of each tool (Pickaxe_<Name>_<view>.png)")
    parser.add_argument("--weigh", action="store_true", help="only weigh the tools: tools.json and the reports gain their weights, nothing is rebuilt")
    args = parser.parse_args(argv)
    os.makedirs(args.out, exist_ok=True)
    report = args.report or os.path.normpath(os.path.join(HERE, "..", "..", "Review", "Miners"))
    os.makedirs(report, exist_ok=True)
    path = os.path.join(args.out, "tools.json")
    previous = {}
    if os.path.exists(path):
        with open(path, encoding="utf-8") as f:
            previous = {t["name"]: t for t in json.load(f).get("tools", [])}
    import workers
    for name in workers.PRESETS:
        if args.only and name not in args.only:
            continue
        if args.weigh:
            if name not in previous:
                raise SystemExit(f"tools.json has no {name}: build the tools first.")
            bpy.ops.wm.read_factory_settings(use_empty=True)
            weight = weighed(name)
            previous[name].update(mass=weight["mass"], centre=weight["centre"], inertia=weight["inertia"], inertiaTurn=weight["inertiaTurn"])
            added = weight_lines(weight, previous[name]["primaryGrip"][1])
            print(f"TOOL Pickaxe_{name}\n" + "\n".join(added))
            kept = []
            old = os.path.join(report, f"tool_{name}.txt")
            if os.path.exists(old):
                with open(old, encoding="utf-8") as f:
                    kept = [l.rstrip("\n") for l in f if not l.startswith(("  it weighs", "  to turn it"))]
            failing = [l for l in kept if l.startswith("  FAIL")]
            kept = [l for l in kept if not l.startswith("  FAIL")]
            with open(old, "w", encoding="utf-8") as f:
                f.write("\n".join(kept + added + failing) + "\n")
            continue
        if args.preview:
            os.makedirs(args.preview, exist_ok=True)
        data, lines = build(name, args.out, os.path.join(args.preview, f"Pickaxe_{name}") if args.preview else None)
        previous[name] = data
        with open(os.path.join(report, f"tool_{name}.txt"), "w", encoding="utf-8") as f:
            f.write("\n".join(lines) + "\n")
    with open(path, "w", encoding="utf-8") as f:
        json.dump(dict(tools=[previous[n] for n in workers.PRESETS if n in previous]), f, indent=2)
