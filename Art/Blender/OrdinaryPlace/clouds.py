"""Wonder Gather — monumental cumulus for the sky beyond the Ordinary Place (S1e).

Clouds as architecture, after the painted skies of Luis's references (see
Docs/ArtDirection/TheEssence.md): towering cumulus, wide heaps and long low banks.
Each is a cauliflower of metaball domes: big domes stacked and leaning, with smaller
domes sprouting over their upper sides, two generations deep. The base is cut flat
at the condensation level, as real cumulus are.

Every mesh is normalised so its base footprint is about 2 units across and its flat
base sits at height 0; Unity scales them to hundreds of metres. Vertex colours carry
what the painted cloud shader needs:
    R: occlusion, 1 in the open and lower in the folds between domes;
    G: height through the cloud, 0 at the base and 1 at the crown.

Run (Blender 4.4+):
    blender -b --factory-startup --python clouds.py -- --fbx <path/Clouds.fbx> [--preview <path.png>]
"""
import argparse
import math
import random
import sys

import bmesh
import bpy
from mathutils import Euler, Vector
from mathutils.bvhtree import BVHTree


def billow(rng, elements, centre, radius, depth, rise):
    """A dome, and smaller domes sprouting over its upper side: the cauliflower of cumulus."""
    elements.append((centre.copy(), radius))
    if depth == 0:
        return
    # Children sit half inside their parent, so the cloud reads as a few great masses that
    # bulge, rather than a heap of separate balls.
    for _ in range(rng.randint(4, 6)):
        d = Vector((rng.gauss(0, 1), rng.gauss(0, 1), abs(rng.gauss(0, 1)) * rise + 0.3))
        d.normalize()
        child = centre + d * radius * rng.uniform(0.42, 0.58)
        billow(rng, elements, child, radius * rng.uniform(0.46, 0.6), depth - 1, rise)


def tower(rng, levels):
    """Towering cumulus: wide domes on the base, narrowing and leaning as they climb."""
    elements = []
    centre = Vector((0, 0, 0))
    lean = Vector((rng.uniform(-0.18, 0.18), rng.uniform(-0.1, 0.1), 0))
    r, z = 1.0, 0.3
    for level in range(levels):
        count = 4 if level == 0 else (3 if level < levels - 1 else 1)
        for _ in range(count):
            a = rng.uniform(0, math.tau)
            spread = r * 0.55 * rng.uniform(0.7, 1.1) if count > 1 else 0
            c = centre + Vector((math.cos(a) * spread, math.sin(a) * spread, z))
            billow(rng, elements, c, r * rng.uniform(0.85, 1.0), 2, 1.6)
        z += r * 0.62
        r *= rng.uniform(0.8, 0.9)
        centre += lean * r
    return elements


def heap(rng):
    """Cumulus mediocris: wider than tall, a few crowns over a broad base."""
    elements = []
    for _ in range(rng.randint(5, 8)):
        x = rng.uniform(-1.4, 1.4)
        y = rng.uniform(-0.5, 0.5)
        r = rng.uniform(0.7, 0.95) * (1.2 - abs(x) / 2.6)
        billow(rng, elements, Vector((x, y, 0.22 + r * 0.25)), r, 2, 1.3)
    for _ in range(rng.randint(1, 2)):
        billow(rng, elements, Vector((rng.uniform(-0.5, 0.5), rng.uniform(-0.2, 0.2), 0.75)), rng.uniform(0.75, 0.9), 2, 1.6)
    return elements


def bank(rng):
    """A long, low bank for the horizon, as at sunset in the references."""
    elements = []
    for i in range(16):
        x = -3.2 + i * 0.42 + rng.uniform(-0.12, 0.12)
        r = rng.uniform(0.55, 0.8) * (1.15 - abs(x) / 5)
        billow(rng, elements, Vector((x, rng.uniform(-0.35, 0.35), 0.1 + r * 0.15)), r, 1, 0.7)
    return elements


def to_mesh(name, elements, resolution):
    """Polygonises the metaballs into a mesh object of the given name."""
    ball = bpy.data.metaballs.new("MB" + name)
    ball.resolution = resolution
    ball.render_resolution = resolution
    ball.threshold = 0.6
    for centre, radius in elements:
        e = ball.elements.new(type='BALL')
        e.co = centre
        e.radius = radius
        e.stiffness = 2.0
    holder = bpy.data.objects.new("MB" + name, ball)
    bpy.context.scene.collection.objects.link(holder)
    evaluated = holder.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh = bpy.data.meshes.new_from_object(evaluated)
    bpy.data.objects.remove(holder)
    bpy.data.metaballs.remove(ball)
    mesh.name = name
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    return obj


def flatten_and_normalise(obj, base):
    """Cuts the base flat, softens the cut, and scales the footprint to about 2 units."""
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    for v in bm.verts:
        if v.co.z < base:
            v.co.z = base
    bmesh.ops.smooth_vert(bm, verts=bm.verts, factor=0.5, use_axis_x=True, use_axis_y=True, use_axis_z=True)
    xs = [v.co.x for v in bm.verts]
    ys = [v.co.y for v in bm.verts]
    zmin = min(v.co.z for v in bm.verts)
    centre = Vector(((max(xs) + min(xs)) / 2, (max(ys) + min(ys)) / 2, zmin))
    width = max(max(xs) - min(xs), max(ys) - min(ys))
    scale = 2.0 / width if obj.name.startswith("Cloud_Tower") or obj.name.startswith("Cloud_Heap") else 6.0 / width
    for v in bm.verts:
        v.co = (v.co - centre) * scale
    bm.to_mesh(obj.data)
    bm.free()


def decimate(obj, faces):
    count = len(obj.data.polygons)
    if count <= faces:
        return
    mod = obj.modifiers.new("Decimate", 'DECIMATE')
    mod.ratio = faces / count
    with bpy.context.temp_override(object=obj, active_object=obj, selected_objects=[obj]):
        bpy.ops.object.modifier_apply(modifier=mod.name)


def bake_vertex_data(obj, rays=20, reach=0.5, seed=1):
    """R: hemisphere occlusion (folds between domes go darker); G: height through the cloud."""
    mesh = obj.data
    for p in mesh.polygons:
        p.use_smooth = True
    mesh.update()
    bvh = BVHTree.FromObject(obj, bpy.context.evaluated_depsgraph_get())
    rng = random.Random(seed)
    zs = [v.co.z for v in mesh.vertices]
    zmin, zmax = min(zs), max(zs)
    reach *= max(zmax - zmin, 1.0)
    directions = []
    for i in range(rays):
        # Cosine-weighted directions around +Z, rotated onto each normal below.
        u, v = (i + rng.random()) / rays, rng.random()
        r, a = math.sqrt(u), v * math.tau
        directions.append(Vector((r * math.cos(a), r * math.sin(a), math.sqrt(max(0.0, 1 - u)))))
    occlusion = []
    for vert in mesh.vertices:
        n = vert.normal
        rot = Vector((0, 0, 1)).rotation_difference(n)
        origin = vert.co + n * 0.004
        hits = 0
        for d in directions:
            loc, _, _, _ = bvh.ray_cast(origin, rot @ d, reach)
            if loc is not None:
                hits += 1
        occlusion.append(1 - hits / rays)
    # Soften over neighbours so the folds read as painted shadow, not speckle.
    neighbours = [[] for _ in mesh.vertices]
    for e in mesh.edges:
        a, b = e.vertices
        neighbours[a].append(b)
        neighbours[b].append(a)
    for _ in range(3):
        occlusion = [(o + sum(occlusion[j] for j in nb)) / (1 + len(nb)) for o, nb in zip(occlusion, neighbours)]
    colors = mesh.color_attributes.new(name="Col", type='FLOAT_COLOR', domain='POINT')
    for i, vert in enumerate(mesh.vertices):
        colors.data[i].color = (occlusion[i], (vert.co.z - zmin) / max(zmax - zmin, 1e-5), 0, 1)


SPECS = [
    ("Cloud_Tower_1", lambda r: tower(r, 6), 11, 26000),
    ("Cloud_Tower_2", lambda r: tower(r, 5), 23, 24000),
    ("Cloud_Tower_3", lambda r: tower(r, 4), 37, 22000),
    ("Cloud_Heap_1", heap, 41, 12000),
    ("Cloud_Heap_2", heap, 53, 12000),
    ("Cloud_Heap_3", heap, 67, 12000),
    ("Cloud_Heap_4", heap, 71, 12000),
    ("Cloud_Bank_1", bank, 83, 10000),
    ("Cloud_Bank_2", bank, 97, 10000),
]


def build():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for name, shape, seed, faces in SPECS:
        rng = random.Random(seed)
        elements = shape(rng)
        obj = to_mesh(name, elements, 0.035)
        flatten_and_normalise(obj, 0.12)
        decimate(obj, faces)
        bake_vertex_data(obj, seed=seed)
        print(f"CLOUD {name}: {len(elements)} domes, {len(obj.data.polygons)} faces, height {obj.dimensions.z:.2f}")


def export_fbx(path):
    bpy.ops.export_scene.fbx(filepath=path, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                             bake_space_transform=True, axis_forward='-Z', axis_up='Y', object_types={'MESH'},
                             use_mesh_modifiers=True, mesh_smooth_type='OFF', colors_type='LINEAR',
                             add_leaf_bones=False, bake_anim=False)


def preview(path):
    """Lays the clouds out in a row against a blue sky, lit by a low sun, for a quick look."""
    scene = bpy.context.scene
    x = 0.0
    for name, _, _, _ in SPECS:
        obj = bpy.data.objects[name]
        width = obj.dimensions.x
        obj.location.x = x + width / 2
        x += width + 0.6
        mat = bpy.data.materials.new(name + "_Preview")
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes["Principled BSDF"]
        attr = mat.node_tree.nodes.new("ShaderNodeVertexColor")
        attr.layer_name = "Col"
        sep = mat.node_tree.nodes.new("ShaderNodeSeparateColor")
        mat.node_tree.links.new(attr.outputs["Color"], sep.inputs[0])
        mix = mat.node_tree.nodes.new("ShaderNodeMix")
        mix.data_type = 'RGBA'
        mix.inputs["A"].default_value = (0.45, 0.47, 0.62, 1)
        mix.inputs["B"].default_value = (1, 0.98, 0.94, 1)
        mat.node_tree.links.new(sep.outputs["Red"], mix.inputs["Factor"])
        mat.node_tree.links.new(mix.outputs["Result"], bsdf.inputs["Base Color"])
        obj.data.materials.append(mat)
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 24
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
    scene.render.resolution_x, scene.render.resolution_y = 2000, 520
    cam = bpy.data.objects.new("Preview", bpy.data.cameras.new("Preview"))
    scene.collection.objects.link(cam)
    cam.data.type = 'ORTHO'
    cam.data.ortho_scale = x + 0.4
    cam.location = (x / 2, -20, 1.4)
    cam.rotation_euler = Euler((math.radians(90), 0, 0))
    scene.camera = cam
    sun = bpy.data.objects.new("Sun", bpy.data.lights.new("Sun", 'SUN'))
    scene.collection.objects.link(sun)
    sun.rotation_euler = Euler((math.radians(55), 0, math.radians(35)))
    sun.data.energy = 4
    world = bpy.data.worlds.new("World")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.2, 0.45, 0.8, 1)
    scene.world = world
    scene.view_settings.view_transform = 'Standard'
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--fbx")
    parser.add_argument("--preview")
    args = parser.parse_args(argv)
    build()
    if args.fbx:
        export_fbx(args.fbx)
    if args.preview:
        preview(args.preview)
