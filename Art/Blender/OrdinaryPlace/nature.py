"""Wonder Gather — the Ordinary Place: trees, bushes and stones (S1b).

Painted-looking nature after Visual Soul references A and C. Trees are a curving
trunk and branches (a skin-modified skeleton) carrying rounded leaf clumps; the
clumps keep soft outward normals, so Unity's painted shader shades each one as a
single brushed mass. Bushes are low clump groups; stones are flattened, lumpy
blobs.

Every object is exported in one FBX, at the origin, for Unity to place.

Run (Blender 4.4+):
    blender -b --factory-startup --python nature.py -- --fbx <path/Nature.fbx> [--preview <path.png>]
"""
import argparse
import math
import random
import sys

import bmesh
import bpy
from mathutils import Euler, Matrix, Vector

COLORS = {
    "Bark": (0.24, 0.20, 0.17),
    "Leaves": (0.17, 0.30, 0.15),
    "LeavesLight": (0.30, 0.40, 0.17),
    "Rock": (0.30, 0.30, 0.29),
}


def material(name):
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        mat.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*COLORS[name], 1)
        mat.diffuse_color = (*COLORS[name], 1)
    return mat


def link(obj):
    bpy.context.scene.collection.objects.link(obj)
    return obj


def apply_all(obj):
    with bpy.context.temp_override(object=obj, active_object=obj, selected_objects=[obj]):
        for mod in list(obj.modifiers):
            bpy.ops.object.modifier_apply(modifier=mod.name)


def lumpy(bm, verts, rng, roughness, frequency=3.0):
    seed = rng.uniform(0, 100)
    for v in verts:
        n = v.co.normalized()
        bump = (math.sin(n.x * frequency * 2.1 + seed) * math.cos(n.y * frequency * 1.7 + seed * 0.5)
                + math.sin(n.z * frequency * 2.9 + seed * 1.3) * 0.6)
        v.co *= 1 + roughness * bump * 0.5 + rng.uniform(-roughness, roughness) * 0.15


def branch_skeleton(rng, scale):
    """Returns (points, edges, radii) for a trunk with curving branches."""
    points, edges, radii = [], [], []

    def add(co, radius, parent=None):
        points.append(Vector(co))
        radii.append(radius)
        index = len(points) - 1
        if parent is not None:
            edges.append((parent, index))
        return index

    def grow(start, direction, length, radius, depth, parent):
        segments = 4
        current = parent
        position = Vector(points[parent])
        for s in range(1, segments + 1):
            bend = Vector((rng.uniform(-0.25, 0.25), rng.uniform(-0.25, 0.25), rng.uniform(-0.05, 0.12)))
            direction = (direction + bend * 0.5).normalized()
            position = position + direction * (length / segments)
            r = radius * (1 - 0.55 * s / segments)
            current = add(position, r, current)
            if depth > 0 and s >= 2 and rng.random() < 0.75:
                side = Vector((rng.uniform(-1, 1), rng.uniform(-1, 1), rng.uniform(0.2, 0.9))).normalized()
                grow(position, (direction + side).normalized(), length * rng.uniform(0.45, 0.65), r * 0.7, depth - 1, current)
        tips.append(position)

    tips = []
    root = add((0, 0, 0), 0.36 * scale)
    trunk_top = root
    position = Vector((0, 0, 0))
    lean = Vector((rng.uniform(-0.15, 0.15), rng.uniform(-0.15, 0.15), 1)).normalized()
    for s in range(1, 5):
        position = position + (lean + Vector((math.sin(s * 1.3) * 0.12, math.cos(s * 0.9) * 0.1, 0))).normalized() * (0.9 * scale)
        trunk_top = add(position, 0.36 * scale * (1 - 0.12 * s), trunk_top)
    for k in range(rng.randint(4, 6)):
        angle = k / 5 * math.tau + rng.uniform(-0.4, 0.4)
        direction = Vector((math.cos(angle), math.sin(angle), rng.uniform(0.5, 1.1))).normalized()
        grow(position, direction, rng.uniform(2.0, 3.0) * scale, 0.2 * scale, 1, trunk_top)
    return points, edges, radii, tips


def build_tree(name, scale, seed, location=(0, 0, 0)):
    rng = random.Random(seed)
    points, edges, radii, tips = branch_skeleton(rng, scale)
    mesh = bpy.data.meshes.new(name + "_Wood")
    mesh.from_pydata([tuple(p) for p in points], edges, [])
    wood = link(bpy.data.objects.new(name + "_Wood", mesh))
    skin = wood.modifiers.new("Skin", 'SKIN')
    for i, v in enumerate(wood.data.skin_vertices[0].data):
        v.radius = (radii[i], radii[i])
    wood.data.skin_vertices[0].data[0].use_root = True
    sub = wood.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = 1
    apply_all(wood)
    wood.data.materials.append(material("Bark"))

    leaves = bmesh.new()
    slots = {"Leaves": 0, "LeavesLight": 1}
    for tip in tips:
        for _ in range(rng.randint(1, 2)):
            radius = rng.uniform(0.85, 1.45) * scale
            center = tip + Vector((rng.uniform(-0.5, 0.5), rng.uniform(-0.5, 0.5), rng.uniform(0.0, 0.6))) * scale
            result = bmesh.ops.create_icosphere(leaves, subdivisions=3, radius=radius)
            verts = result["verts"]
            lumpy(leaves, verts, rng, 0.12, 1.8)
            for v in verts:
                v.co.z *= 0.78
                v.co += center
            light = 1 if center.z > 5.2 * scale and rng.random() < 0.6 else 0
            for f in {f for v in verts for f in v.link_faces}:
                f.material_index = light
    canopy = bpy.data.meshes.new(name + "_Leaves")
    leaves.to_mesh(canopy)
    leaves.free()
    crown = link(bpy.data.objects.new(name + "_Leaves", canopy))
    crown.data.materials.append(material("Leaves"))
    crown.data.materials.append(material("LeavesLight"))
    for obj in (wood, crown):
        obj.data.shade_smooth()
        obj.location = location
    return wood, crown


def build_bush(name, seed, location=(0, 0, 0)):
    rng = random.Random(seed)
    bm = bmesh.new()
    for _ in range(rng.randint(3, 5)):
        radius = rng.uniform(0.45, 0.8)
        center = Vector((rng.uniform(-0.7, 0.7), rng.uniform(-0.7, 0.7), radius * 0.55))
        verts = bmesh.ops.create_icosphere(bm, subdivisions=3, radius=radius)["verts"]
        lumpy(bm, verts, rng, 0.14, 1.9)
        for v in verts:
            v.co.z *= 0.8
            v.co += center
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = link(bpy.data.objects.new(name, mesh))
    obj.data.materials.append(material("Leaves"))
    obj.data.shade_smooth()
    obj.location = location
    return obj


def build_rock(name, seed, size, location=(0, 0, 0)):
    rng = random.Random(seed)
    bm = bmesh.new()
    verts = bmesh.ops.create_icosphere(bm, subdivisions=2, radius=size)["verts"]
    lumpy(bm, verts, rng, 0.35, 2.2)
    for v in verts:
        v.co.z = v.co.z * rng.uniform(0.5, 0.65) + size * 0.15
        v.co.x *= rng.uniform(0.9, 1.3)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = link(bpy.data.objects.new(name, mesh))
    obj.data.materials.append(material("Rock"))
    obj.data.shade_smooth()
    obj.data.set_sharp_from_angle(angle=math.radians(50))
    obj.location = location
    return obj


def build(spread=False):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    offset = (lambda i: (i * 9.0, 0, 0)) if spread else (lambda i: (0, 0, 0))
    build_tree("Tree_Large", 1.15, 3, offset(0))
    build_tree("Tree_Small_A", 0.75, 8, offset(1))
    build_tree("Tree_Small_B", 0.62, 15, offset(2))
    build_bush("Bush_A", 21, offset(3))
    build_bush("Bush_B", 34, offset(4))
    for i, size in enumerate((0.55, 0.85, 0.4, 1.2)):
        build_rock(f"Rock_{i + 1}", 50 + i, size, offset(5 + i))


def export_fbx(path):
    bpy.ops.export_scene.fbx(filepath=path, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                             bake_space_transform=True, axis_forward='-Z', axis_up='Y', object_types={'MESH'},
                             use_mesh_modifiers=True, mesh_smooth_type='OFF', add_leaf_bones=False, bake_anim=False)


def preview(path):
    scene = bpy.context.scene
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
    scene.render.resolution_x, scene.render.resolution_y = 1600, 600
    cam = link(bpy.data.objects.new("Preview", bpy.data.cameras.new("Preview")))
    cam.location = (34, -62, 9)
    cam.rotation_euler = (Vector((34, 0, 4)) - cam.location).to_track_quat('-Z', 'Y').to_euler()
    cam.data.lens = 32
    scene.camera = cam
    sun = link(bpy.data.objects.new("Sun", bpy.data.lights.new("Sun", 'SUN')))
    sun.rotation_euler = Euler((math.radians(50), 0, math.radians(30)))
    sun.data.energy = 3
    world = bpy.data.worlds.new("World")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.4, 0.47, 0.6, 1)
    scene.world = world
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--fbx")
    parser.add_argument("--preview")
    args = parser.parse_args(argv)
    if args.fbx:
        build()
        export_fbx(args.fbx)
    if args.preview:
        build(spread=True)
        preview(args.preview)
