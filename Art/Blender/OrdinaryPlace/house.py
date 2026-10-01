"""Wonder Gather — the Ordinary Place: the lit house (S1b).

A small plastered cottage after Visual Soul reference A, built entirely from this
script so it can be reproduced and adjusted. It includes:

- leaning plaster walls and a timber frame;
- a hand-laid shingle roof and a stone chimney;
- deep window reveals with warm glass;
- an open door onto a small warm room, with a curtain;
- doorstep stones, a bench, pots with plants, a firewood lean-to, a fence and a
  lantern.

Empties named Light_*, Smoke_* and Blocker_* mark where Unity places lights,
chimney smoke and the invisible doorway blocker (the house has no walkable
interior, so the Explore camera may look in but not enter).

Run (Blender 4.4+):
    blender -b --factory-startup --python house.py -- --fbx <path/House.fbx> [--preview <path.png>]
"""
import argparse
import math
import random
import sys

import bmesh
import bpy
from mathutils import Euler, Matrix, Vector

# Footprint and heights in metres. X runs along the front, -Y faces the path, Z is up.
LENGTH, DEPTH, WALL, RIDGE = 6.2, 4.4, 2.55, 4.55
ROOF_LIFT = 0.24  # The roof deck rests on the wall plates, above the plastered gables.
WALL_THICK = 0.26
EAVE_OVERHANG, GABLE_OVERHANG = 0.45, 0.38
FRONT = -DEPTH / 2
DOOR = dict(x0=0.12, x1=1.06, top=1.95)
BIG_WINDOW = dict(x0=-2.35, x1=-1.05, z0=0.86, z1=1.86)
SMALL_WINDOW = dict(x0=2.05, x1=2.6, z0=1.08, z1=1.68)
SIDE_WINDOW = dict(y0=0.25, y1=0.8, z0=1.1, z1=1.65)
ROOM = dict(x0=-0.55, x1=1.95, y1=1.0, top=2.32)

MATERIALS = {
    # Base colours only; Unity's painted shader supplies light, shade and brushwork.
    "Plaster": (0.80, 0.71, 0.58),
    "Interior": (0.86, 0.62, 0.38),
    "Timber": (0.34, 0.24, 0.16),
    "Roof": (0.30, 0.27, 0.26),
    "Stone": (0.45, 0.42, 0.38),
    "Glow": (1.0, 0.72, 0.38),
    "Clay": (0.62, 0.36, 0.22),
    "Foliage": (0.24, 0.36, 0.18),
    "Cloth": (0.62, 0.12, 0.08),
    "Firewood": (0.52, 0.38, 0.24),
    "Iron": (0.16, 0.15, 0.15),
}

rng = random.Random(1)


def jitter(amount):
    return rng.uniform(-amount, amount)


def material(name):
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        color = MATERIALS[name]
        bsdf = mat.node_tree.nodes["Principled BSDF"]
        bsdf.inputs["Base Color"].default_value = (*color, 1)
        bsdf.inputs["Roughness"].default_value = 0.85
        if name == "Glow":
            bsdf.inputs["Emission Color"].default_value = (*color, 1)
            bsdf.inputs["Emission Strength"].default_value = 6.0
        mat.diffuse_color = (*color, 1)
    return mat


class Part:
    """Accumulates geometry for one exported object, with a material per face."""

    def __init__(self, name):
        self.name = name
        self.bm = bmesh.new()
        self.slots = []

    def slot(self, mat_name):
        if mat_name not in self.slots:
            self.slots.append(mat_name)
        return self.slots.index(mat_name)

    def _take(self, geom_verts, mat_name, matrix):
        bmesh.ops.transform(self.bm, matrix=matrix, verts=geom_verts)
        index = self.slot(mat_name)
        faces = {f for v in geom_verts for f in v.link_faces}
        for face in faces:
            face.material_index = index
        return geom_verts

    def box(self, size, location, mat_name, rotation=(0, 0, 0), wobble=0.0):
        result = bmesh.ops.create_cube(self.bm, size=1.0)
        verts = result["verts"]
        for v in verts:
            v.co = Vector((v.co.x * size[0], v.co.y * size[1], v.co.z * size[2]))
            if wobble:
                v.co += Vector((jitter(wobble), jitter(wobble), jitter(wobble)))
        matrix = Matrix.Translation(Vector(location)) @ Euler(rotation).to_matrix().to_4x4()
        return self._take(verts, mat_name, matrix)

    def cylinder(self, radius, depth, location, mat_name, rotation=(0, 0, 0), segments=10, radius2=None, wobble=0.0):
        result = bmesh.ops.create_cone(self.bm, cap_ends=True, cap_tris=False, segments=segments,
                                       radius1=radius, radius2=radius if radius2 is None else radius2, depth=depth)
        verts = result["verts"]
        if wobble:
            for v in verts:
                v.co += Vector((jitter(wobble), jitter(wobble), jitter(wobble * 0.5)))
        matrix = Matrix.Translation(Vector(location)) @ Euler(rotation).to_matrix().to_4x4()
        return self._take(verts, mat_name, matrix)

    def blob(self, radius, location, mat_name, scale=(1, 1, 1), subdivisions=2, roughness=0.25):
        result = bmesh.ops.create_icosphere(self.bm, subdivisions=subdivisions, radius=radius)
        verts = result["verts"]
        for v in verts:
            n = v.co.normalized()
            bump = 1 + roughness * (math.sin(n.x * 7.1 + n.y * 3.3) * math.cos(n.z * 5.7 + n.x * 2.1)) + jitter(roughness * 0.3)
            v.co = Vector((v.co.x * scale[0], v.co.y * scale[1], v.co.z * scale[2])) * bump
        return self._take(verts, mat_name, Matrix.Translation(Vector(location)))

    def finish(self, smooth_angle=35):
        mesh = bpy.data.meshes.new(self.name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        obj = bpy.data.objects.new(self.name, mesh)
        bpy.context.scene.collection.objects.link(obj)
        for name in self.slots:
            mesh.materials.append(material(name))
        mesh.shade_smooth()
        mesh.set_sharp_from_angle(angle=math.radians(smooth_angle))
        return obj


def apply_modifiers(obj):
    with bpy.context.temp_override(object=obj, active_object=obj, selected_objects=[obj]):
        for mod in list(obj.modifiers):
            bpy.ops.object.modifier_apply(modifier=mod.name)


def cutter(name, minimum, maximum, mat_name):
    part = Part(name)
    size = [maximum[i] - minimum[i] for i in range(3)]
    center = [(maximum[i] + minimum[i]) / 2 for i in range(3)]
    part.box(size, center, mat_name)
    obj = part.finish()
    obj.hide_render = True
    return obj


# ---------------------------------------------------------------------------
# The plastered shell: a leaning pentagonal prism with openings and a small room.
# ---------------------------------------------------------------------------
def build_shell():
    part = Part("House_Shell")
    bm = part.bm
    sections = 9
    profile = [(FRONT, 0.0), (-FRONT, 0.0), (-FRONT, WALL), (0.0, RIDGE), (FRONT, WALL)]
    rings = []
    for i in range(sections + 1):
        x = -LENGTH / 2 + LENGTH * i / sections
        sag = math.sin(math.pi * i / sections) * 0.035
        ring = []
        for j, (y, z) in enumerate(profile):
            # Walls lean and bow a little, as hand-built plaster does.
            lean = 0.03 * (z / RIDGE) * (1 if y > 0 else -1) if j != 3 else 0
            co = Vector((x + (jitter(0.015) if 0 < i < sections else 0), y + lean + (sag if z > 0 else 0) * (1 if y > 0 else -1),
                         z - (sag * 0.6 if j == 3 else 0)))
            ring.append(bm.verts.new(co))
        rings.append(ring)
    slot = part.slot("Plaster")
    for i in range(sections):
        for j in range(len(profile)):
            a, b = rings[i][j], rings[i][(j + 1) % len(profile)]
            c, d = rings[i + 1][(j + 1) % len(profile)], rings[i + 1][j]
            bm.faces.new((a, b, c, d)).material_index = slot
    bm.faces.new(list(reversed(rings[0]))).material_index = slot
    bm.faces.new(rings[-1]).material_index = slot
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    shell = part.finish()

    cutters = [
        cutter("Cut_Door", (DOOR["x0"], FRONT - 0.3, -0.05), (DOOR["x1"], FRONT + WALL_THICK + 0.05, DOOR["top"]), "Interior"),
        cutter("Cut_Room", (ROOM["x0"], FRONT + WALL_THICK, 0.0), (ROOM["x1"], ROOM["y1"], ROOM["top"]), "Interior"),
        cutter("Cut_BigWindow", (BIG_WINDOW["x0"], FRONT - 0.3, BIG_WINDOW["z0"]), (BIG_WINDOW["x1"], FRONT + 0.3, BIG_WINDOW["z1"]), "Plaster"),
        cutter("Cut_SmallWindow", (SMALL_WINDOW["x0"], FRONT - 0.3, SMALL_WINDOW["z0"]), (SMALL_WINDOW["x1"], FRONT + 0.28, SMALL_WINDOW["z1"]), "Plaster"),
        cutter("Cut_SideWindow", (LENGTH / 2 - 0.28, SIDE_WINDOW["y0"], SIDE_WINDOW["z0"]), (LENGTH / 2 + 0.3, SIDE_WINDOW["y1"], SIDE_WINDOW["z1"]), "Plaster"),
        cutter("Cut_BackWindow", (-1.4, -FRONT - 0.28, 1.0), (-0.6, -FRONT + 0.3, 1.7), "Plaster"),
    ]
    for c in cutters:
        mod = shell.modifiers.new(c.name, 'BOOLEAN')
        mod.object = c
        mod.operation = 'DIFFERENCE'
        mod.solver = 'EXACT'
        mod.material_mode = 'TRANSFER'
    apply_modifiers(shell)
    for c in cutters:
        bpy.data.objects.remove(c, do_unlink=True)
    shell.data.set_sharp_from_angle(angle=math.radians(35))
    return shell


# ---------------------------------------------------------------------------
# Timber: frame, window and door frames, mullions, bench.
# ---------------------------------------------------------------------------
def beam(part, a, b, thickness, mat_name="Timber", wobble=0.008):
    a, b = Vector(a), Vector(b)
    axis = b - a
    length = axis.length
    rot = axis.normalized().to_track_quat('Z', 'Y').to_euler()
    part.box((thickness * rng.uniform(0.9, 1.08), thickness * rng.uniform(0.9, 1.08), length), (a + b) / 2, mat_name, rot, wobble)


def build_timber():
    part = Part("House_Timber")
    t = 0.2
    fy, by = FRONT - 0.03, -FRONT + 0.03
    for x in (-LENGTH / 2 + 0.06, LENGTH / 2 - 0.06):
        for y in (fy, by):
            beam(part, (x + jitter(0.01), y, -0.05), (x + jitter(0.02), y, WALL + 0.06), t)
    for y in (fy, by):
        beam(part, (-LENGTH / 2 - 0.1, y, WALL + 0.02), (LENGTH / 2 + 0.1, y, WALL - 0.02), t * 1.1)
    # Front: posts either side of the door and a beam at lintel height.
    for x in (DOOR["x0"] - 0.1, DOOR["x1"] + 0.1):
        beam(part, (x, fy - 0.01, -0.05), (x + jitter(0.015), fy - 0.01, WALL), 0.17)
    beam(part, (-LENGTH / 2, fy - 0.02, 2.04), (LENGTH / 2, fy - 0.02, 2.06), 0.15)
    # Gables: tie beam, king post and raking bargeboards.
    for x in (-LENGTH / 2 - 0.03, LENGTH / 2 + 0.03):
        beam(part, (x, FRONT - 0.1, WALL), (x, -FRONT + 0.1, WALL), 0.17)
        beam(part, (x, 0, WALL), (x, 0, RIDGE - 0.15), 0.15)
        for sign in (-1, 1):
            beam(part, (x, 0.0, RIDGE + ROOF_LIFT - 0.05), (x, sign * (DEPTH / 2 + EAVE_OVERHANG), WALL + ROOF_LIFT - 0.1 - EAVE_OVERHANG * 0.9), 0.12)
            # Braces rise from the tie beam's ends towards the king post, staying under the roof.
            beam(part, (x, sign * (DEPTH / 2 - 0.35), WALL + 0.06), (x, sign * 0.16, RIDGE - 0.62), 0.1)
    # Window frames: sill, lintel, jambs and mullions set into the reveal.
    for w, cols, rows in ((BIG_WINDOW, 3, 2), (SMALL_WINDOW, 2, 2)):
        x0, x1, z0, z1 = w["x0"], w["x1"], w["z0"], w["z1"]
        beam(part, (x0 - 0.12, FRONT - 0.06, z0 - 0.04), (x1 + 0.12, FRONT - 0.06, z0 - 0.05), 0.1)
        beam(part, (x0 - 0.1, FRONT - 0.02, z1 + 0.05), (x1 + 0.1, FRONT - 0.02, z1 + 0.04), 0.12)
        for x in (x0 + 0.03, x1 - 0.03):
            beam(part, (x, FRONT + 0.05, z0), (x, FRONT + 0.05, z1), 0.07)
        glass_y = FRONT + 0.2
        for c in range(1, cols):
            x = x0 + (x1 - x0) * c / cols
            beam(part, (x, glass_y - 0.02, z0), (x, glass_y - 0.02, z1), 0.045, wobble=0.004)
        for r in range(1, rows):
            z = z0 + (z1 - z0) * r / rows
            beam(part, (x0, glass_y - 0.02, z), (x1, glass_y - 0.02, z), 0.045, wobble=0.004)
    sx = LENGTH / 2
    beam(part, (sx + 0.05, SIDE_WINDOW["y0"] - 0.1, SIDE_WINDOW["z0"] - 0.04), (sx + 0.05, SIDE_WINDOW["y1"] + 0.1, SIDE_WINDOW["z0"] - 0.04), 0.09)
    beam(part, (sx + 0.02, SIDE_WINDOW["y0"] - 0.08, SIDE_WINDOW["z1"] + 0.04), (sx + 0.02, SIDE_WINDOW["y1"] + 0.08, SIDE_WINDOW["z1"] + 0.04), 0.1)
    beam(part, (sx - 0.18, (SIDE_WINDOW["y0"] + SIDE_WINDOW["y1"]) / 2, SIDE_WINDOW["z0"]), (sx - 0.18, (SIDE_WINDOW["y0"] + SIDE_WINDOW["y1"]) / 2, SIDE_WINDOW["z1"]), 0.04)
    # Door frame.
    beam(part, (DOOR["x0"] - 0.02, FRONT + 0.02, DOOR["top"] + 0.06), (DOOR["x1"] + 0.02, FRONT + 0.02, DOOR["top"] + 0.06), 0.13)
    # Bench under the big window.
    bx = (BIG_WINDOW["x0"] + BIG_WINDOW["x1"]) / 2
    part.box((1.5, 0.36, 0.06), (bx, FRONT - 0.42, 0.46), "Timber", (0, math.radians(1.5), math.radians(-2)), 0.01)
    for x in (bx - 0.6, bx + 0.6):
        part.box((0.08, 0.3, 0.44), (x, FRONT - 0.42, 0.22), "Timber", (0, 0, 0), 0.01)
    # Interior: a table and a shelf seen through the door.
    part.box((1.0, 0.55, 0.05), (0.95, 0.55, 0.78), "Timber", (0, 0, math.radians(4)))
    for dx in (-0.42, 0.42):
        for dy in (-0.22, 0.22):
            part.box((0.05, 0.05, 0.76), (0.95 + dx, 0.55 + dy, 0.38), "Timber")
    part.box((1.6, 0.25, 0.04), (0.7, ROOM["y1"] - 0.14, 1.45), "Timber")
    part.box((1.4, 0.04, 0.04), (0.7, 0.3, 2.05), "Iron")
    return part.finish()


# ---------------------------------------------------------------------------
# Roof: a deck under hand-laid shingle rows, and a ridge cap.
# ---------------------------------------------------------------------------
def build_roof():
    part = Part("House_Roof")
    slope_run = DEPTH / 2 + EAVE_OVERHANG
    angle = math.atan2(RIDGE - WALL, DEPTH / 2)
    slope_len = slope_run / math.cos(angle)
    half = LENGTH / 2 + GABLE_OVERHANG
    for sign in (-1, 1):
        # Rotation tilting a slab so it falls from the ridge towards the eave on this side.
        tilt = Euler((-sign * angle, 0, 0))
        down = Vector((0, sign * math.cos(angle), -math.sin(angle)))
        normal = Vector((0, sign * math.sin(angle), math.cos(angle)))
        center = Vector((0, 0, RIDGE + ROOF_LIFT)) + down * (slope_len / 2)
        part.box((half * 2, slope_len, 0.05), center, "Timber", tilt)
        rows = int(slope_len / 0.19)
        for r in range(rows + 1):
            along = slope_len - r * 0.19
            x = -half + rng.uniform(0, 0.12)
            while x < half:
                width = rng.uniform(0.2, 0.33)
                width = min(width, half - x + 0.02)
                length = rng.uniform(0.38, 0.46)
                spot = Vector((x + width / 2, 0, RIDGE + ROOF_LIFT)) + down * (along - length / 2 + 0.02) + normal * (0.06 + r * 0.0006)
                # Slightly flatter than the deck, so each shingle's lower edge lifts over the row below.
                rot = Euler((-sign * (angle - math.radians(3 + jitter(1.5))), math.radians(jitter(1.5)), math.radians(jitter(3))))
                part.box((width - 0.012, length, 0.028), spot, "Roof", rot, 0.004)
                x += width
    # Ridge cap.
    part.box((half * 2 + 0.05, 0.3, 0.12), (0, 0, RIDGE + ROOF_LIFT + 0.1), "Roof", (0, 0, 0), 0.01)
    return part.finish(smooth_angle=20)


def build_chimney():
    part = Part("House_Chimney")
    x, y = -2.05, -0.35
    part.box((0.62, 0.62, 2.2), (x, y, RIDGE - 0.35), "Stone", (0, 0, math.radians(1.5)), 0.012)
    part.box((0.78, 0.78, 0.09), (x, y, RIDGE + 0.8), "Stone", (0, 0, math.radians(-3)), 0.01)
    part.box((0.44, 0.44, 0.22), (x, y, RIDGE + 0.94), "Stone", (0, 0, math.radians(4)), 0.01)
    return part.finish(), Vector((x, y, RIDGE + 1.06))


def build_stones():
    part = Part("House_Stone")
    # Foundation stones along the base of the walls.
    perimeter = []
    for x in [-LENGTH / 2 + i * 0.32 for i in range(int(LENGTH / 0.32) + 1)]:
        perimeter.append((x, FRONT - 0.05))
        perimeter.append((x, -FRONT + 0.05))
    for y in [FRONT + i * 0.32 for i in range(int(DEPTH / 0.32) + 1)]:
        perimeter.append((-LENGTH / 2 - 0.05, y))
        perimeter.append((LENGTH / 2 + 0.05, y))
    for x, y in perimeter:
        if y < 0 and DOOR["x0"] - 0.05 < x < DOOR["x1"] + 0.05:
            continue
        r = rng.uniform(0.13, 0.2)
        part.blob(r, (x + jitter(0.05), y + jitter(0.03), 0.04), "Stone", (1.3, 0.9, 0.6), 1, 0.18)
    # Doorstep stones leading out to the path.
    mid = (DOOR["x0"] + DOOR["x1"]) / 2
    part.blob(0.62, (mid, FRONT - 0.33, 0.06), "Stone", (1.0, 0.45, 0.16), 2, 0.08)
    part.blob(0.55, (mid + 0.08, FRONT - 0.88, 0.02), "Stone", (1.0, 0.5, 0.13), 2, 0.1)
    part.blob(0.4, (mid + 0.25, FRONT - 1.35, 0.0), "Stone", (1.0, 0.6, 0.11), 2, 0.12)
    return part.finish()


def build_door():
    part = Part("House_Door")
    width = DOOR["x1"] - DOOR["x0"]
    hinge = Vector((DOOR["x1"] + 0.02, FRONT - 0.06, 0))
    # Swung open against the front wall.
    for i in range(4):
        part.box((width / 4 - 0.008, 0.05, DOOR["top"] - 0.04), hinge + Vector((width * (i + 0.5) / 4, 0, DOOR["top"] / 2)), "Timber",
                 (0, 0, math.radians(jitter(0.6))), 0.004)
    for z in (0.35, DOOR["top"] - 0.4):
        part.box((width - 0.06, 0.04, 0.12), hinge + Vector((width / 2, -0.045, z)), "Timber")
    part.box((0.12, 0.02, 0.025), hinge + Vector((width - 0.15, -0.075, 1.0)), "Iron")
    return part.finish()


def build_curtain():
    part = Part("House_Curtain")
    bm = part.bm
    slot = part.slot("Cloth")
    cols, rows = 10, 12
    x0, x1, top = DOOR["x1"] - 0.36, DOOR["x1"] - 0.03, DOOR["top"] - 0.03
    grid = []
    for r in range(rows + 1):
        row = []
        for c in range(cols + 1):
            u, v = c / cols, r / rows
            x = x0 + (x1 - x0) * u
            fold = math.sin(u * math.pi * 4 + v * 0.8) * 0.025 * (0.4 + v)
            z = top - v * 1.55
            row.append(bm.verts.new((x, FRONT + 0.12 + fold, z)))
        grid.append(row)
    for r in range(rows):
        for c in range(cols):
            bm.faces.new((grid[r][c], grid[r][c + 1], grid[r + 1][c + 1], grid[r + 1][c])).material_index = slot
    bmesh.ops.solidify(bm, geom=list(bm.faces), thickness=0.012)
    return part.finish(smooth_angle=60)


def build_glass():
    part = Part("House_Glass")
    for w in (BIG_WINDOW, SMALL_WINDOW):
        part.box((w["x1"] - w["x0"], 0.02, w["z1"] - w["z0"]), ((w["x0"] + w["x1"]) / 2, FRONT + 0.22, (w["z0"] + w["z1"]) / 2), "Glow")
    part.box((0.02, SIDE_WINDOW["y1"] - SIDE_WINDOW["y0"], SIDE_WINDOW["z1"] - SIDE_WINDOW["z0"]),
             (LENGTH / 2 - 0.2, (SIDE_WINDOW["y0"] + SIDE_WINDOW["y1"]) / 2, (SIDE_WINDOW["z0"] + SIDE_WINDOW["z1"]) / 2), "Glow")
    part.box((0.8, 0.02, 0.7), (-1.0, -FRONT - 0.22, 1.35), "Glow")
    # Lantern glass beside the door.
    part.box((0.14, 0.14, 0.2), (DOOR["x0"] - 0.42, FRONT - 0.2, 1.85), "Glow")
    return part.finish()


def build_props():
    part = Part("House_Props")
    # Lantern frame and bracket.
    lx = DOOR["x0"] - 0.42
    part.box((0.18, 0.18, 0.03), (lx, FRONT - 0.2, 1.96), "Iron")
    part.box((0.18, 0.18, 0.03), (lx, FRONT - 0.2, 1.74), "Iron")
    for dx in (-0.08, 0.08):
        for dy in (-0.08, 0.08):
            part.box((0.015, 0.015, 0.22), (lx + dx, FRONT - 0.2 + dy, 1.85), "Iron")
    part.box((0.03, 0.2, 0.03), (lx, FRONT - 0.1, 2.0), "Iron")
    # Clay pots with plants, by the door and on the bench.
    pots = [(DOOR["x0"] - 0.35, FRONT - 0.45, 0.0, 0.17), (DOOR["x1"] + 0.3, FRONT - 0.55, 0.0, 0.2),
            (DOOR["x1"] + 0.62, FRONT - 0.4, 0.0, 0.13), (BIG_WINDOW["x0"] + 0.25, FRONT - 0.42, 0.49, 0.1),
            (BIG_WINDOW["x0"] + 0.55, FRONT - 0.45, 0.49, 0.08), (BIG_WINDOW["x1"] - 0.2, FRONT + 0.12, BIG_WINDOW["z0"] + 0.02, 0.07)]
    for x, y, z, r in pots:
        part.cylinder(r, r * 1.5, (x, y, z + r * 0.75), "Clay", radius2=r * 1.25, segments=12, wobble=0.004)
        for k in range(3):
            part.blob(r * rng.uniform(0.9, 1.25), (x + jitter(r * 0.5), y + jitter(r * 0.5), z + r * 1.6 + jitter(r * 0.3)),
                      "Foliage", (1.0, 1.0, 0.85), 2, 0.35)
    # Firewood stack under a lean-to on the east gable.
    gx = LENGTH / 2 + 0.55
    for row in range(5):
        for i in range(7):
            y = -1.6 + i * 0.17 + (0.085 if row % 2 else 0) + jitter(0.01)
            z = 0.09 + row * 0.15
            part.cylinder(rng.uniform(0.065, 0.085), 0.55, (gx + jitter(0.03), y, z), "Firewood",
                          (0, math.radians(90), 0), segments=8, wobble=0.004)
            # Rotate logs to lie along X: cylinders are created along Z.
    for y in (-1.75, -0.35):
        beam(part, (gx + 0.45, y, 0), (gx + 0.45, y, 1.5), 0.1)
    part.box((0.95, 1.7, 0.05), (gx + 0.1, -1.05, 1.55), "Roof", (0, math.radians(-14), 0))
    # Interior jars on the shelf and pots hanging from the rail.
    for i in range(6):
        part.cylinder(rng.uniform(0.05, 0.08), rng.uniform(0.12, 0.22), (0.05 + i * 0.25, ROOM["y1"] - 0.14, 1.55), "Clay", segments=10)
    for i in range(4):
        part.cylinder(0.11, 0.12, (0.25 + i * 0.32, 0.3, 1.85), "Iron", radius2=0.09, segments=12)
    return part.finish()


def build_fence():
    part = Part("Fence")
    start = Vector((LENGTH / 2 + 0.2, -FRONT + 0.4, 0))
    direction = Vector((1, 0.18, 0)).normalized()
    posts = 7
    tops = []
    for i in range(posts):
        base = start + direction * (i * 1.55 + jitter(0.12))
        lean = Vector((jitter(0.08), jitter(0.08), 0))
        top = base + Vector((0, 0, rng.uniform(1.05, 1.25))) + lean
        beam(part, base - Vector((0, 0, 0.2)), top, rng.uniform(0.09, 0.12), wobble=0.01)
        tops.append((base, top))
    for k, height in enumerate((0.45, 0.85)):
        for i in range(posts - 1):
            if rng.random() < 0.12:
                continue
            a = tops[i][0].lerp(tops[i][1], height) + Vector((0, 0, jitter(0.04)))
            b = tops[i + 1][0].lerp(tops[i + 1][1], height) + Vector((0, 0, jitter(0.04)))
            beam(part, a, b, 0.055, wobble=0.006)
    return part.finish()


def empty(name, location, size=0.2):
    obj = bpy.data.objects.new(name, None)
    obj.location = location
    obj.empty_display_size = size
    bpy.context.scene.collection.objects.link(obj)
    return obj


def build():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    build_shell()
    build_timber()
    build_roof()
    _, smoke = build_chimney()
    build_stones()
    build_door()
    build_curtain()
    build_glass()
    build_props()
    build_fence()
    # Anchors for Unity: lights, smoke and the doorway blocker.
    empty("Light_Interior", (0.7, 0.2, 1.9))
    empty("Light_Door", ((DOOR["x0"] + DOOR["x1"]) / 2, FRONT + 0.3, 1.7))
    empty("Light_BigWindow", ((BIG_WINDOW["x0"] + BIG_WINDOW["x1"]) / 2, FRONT + 0.6, 1.4))
    empty("Light_SmallWindow", ((SMALL_WINDOW["x0"] + SMALL_WINDOW["x1"]) / 2, FRONT + 0.6, 1.4))
    empty("Light_SideWindow", (LENGTH / 2 - 0.6, (SIDE_WINDOW["y0"] + SIDE_WINDOW["y1"]) / 2, 1.4))
    empty("Light_Lantern", (DOOR["x0"] - 0.42, FRONT - 0.2, 1.85))
    empty("Smoke_Chimney", smoke)
    empty("Blocker_Door", ((DOOR["x0"] + DOOR["x1"]) / 2, FRONT + WALL_THICK * 0.5, DOOR["top"] / 2))


def export_fbx(path):
    bpy.ops.export_scene.fbx(filepath=path, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                             bake_space_transform=True, axis_forward='-Z', axis_up='Y', object_types={'MESH', 'EMPTY'},
                             use_mesh_modifiers=True, mesh_smooth_type='OFF', add_leaf_bones=False, bake_anim=False)


def preview(path):
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = 48
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
    scene.render.resolution_x, scene.render.resolution_y = 1280, 720
    cam_data = bpy.data.cameras.new("Preview")
    cam = bpy.data.objects.new("Preview", cam_data)
    scene.collection.objects.link(cam)
    import os
    view = os.environ.get("HOUSE_VIEW", "front")
    cam.location = (5.5, -11.0, 3.2) if view == "front" else (-11.5, 6.5, 6.5)
    direction = Vector((0.2, 0, 1.8 if view == "front" else 3.0)) - cam.location
    cam.rotation_euler = direction.to_track_quat('-Z', 'Y').to_euler()
    scene.camera = cam
    sun = bpy.data.objects.new("Sun", bpy.data.lights.new("Sun", 'SUN'))
    sun.rotation_euler = Euler((math.radians(50), 0, math.radians(30)))
    sun.data.energy = 3
    scene.collection.objects.link(sun)
    world = bpy.data.worlds.new("World")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs[0].default_value = (0.35, 0.42, 0.55, 1)
    scene.world = world
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
