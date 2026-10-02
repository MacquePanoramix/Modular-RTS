"""Wonder Gather — worker concepts (S1d, first step).

Three workers built on the Visual Soul's language for every being
(Docs/ArtDirection/VisualSoul.md, "The language for every being and thing"):

- a shape that tells character;
- drawn contours (an outline drawn in engine);
- feeling from very few marks (a hand-painted face);
- value design (dark hair and coats against light faces and hands);
- posture and gesture;
- worn, individual surfaces;
- small against a large world.

Each concept is one clear shape idea:

    Round: short and heavy-set, a barrel body, fists on the hips, a bulb nose, laughing eyes.
    Long:  tall and thin, a stoop, a long nose, sleepy kind eyes, a mug in hand.
    Small: young and small-framed, a dark bob, an oversized coat, big curious eyes.

They are concept models for Luis to choose a direction, not the rigged worker.

- **Bodies and clothes.** A skeleton of joints given flesh by Blender's skin
  modifier, then smoothed. The skeleton is the procedural rig's own (pelvis,
  spine, chest, neck, head, arms, legs), so a chosen concept can be rigged
  from it.
- **Posture.** The weight rests on one leg. The hips tilt, the chest tilts
  against them, and the head turns and tilts.
- **Heads, hands and hair.** Metaballs: a skull, jaw, cheeks, chin, a brow
  and the nose that gives the character; a palm, fingers and thumb; locks of
  hair.
- **Faces.** Painted into a texture by soft brush strokes (eyes, brows, a
  mouth, blush), projected onto the face from the front.

Run (Blender 4.4+):
    blender -b --factory-startup --python worker_concepts.py -- [--fbx <Workers.fbx>] [--preview <png>]
The faces are written next to the FBX (Face_<Concept>.png).
"""
import argparse
import math
import os
import random
import sys

import bmesh
import bpy
import numpy as np
from mathutils import Euler, Matrix, Vector, noise

# Preview colours only (linear). The engine's painted materials set the real colours.
COLORS = {
    "Hair": (0.035, 0.025, 0.025),
    "HairBrown": (0.07, 0.04, 0.03),
    "Smock": (0.12, 0.17, 0.09),
    "Apron": (0.45, 0.40, 0.30),
    "Coat": (0.05, 0.035, 0.03),
    "CoatBlue": (0.035, 0.045, 0.09),
    "Patch": (0.25, 0.17, 0.09),
    "Trousers": (0.09, 0.07, 0.05),
    "Boots": (0.04, 0.025, 0.02),
    "Leather": (0.22, 0.10, 0.05),
    "Accent": (0.45, 0.08, 0.04),
    "Mug": (0.65, 0.60, 0.50),
}
# Metaballs draw at about 0.58 of their nominal radius (stiffness 2, threshold 0.6).
BALL = 1 / 0.58
# The face texture spans this many head radii either side of the head's centre.
FACE_SPAN = 1.25
FACE_SIZE = 1024
INK = (0.16, 0.09, 0.075)


# ---------------------------------------------------------------- building blocks

def material(name, image=None):
    mat = bpy.data.materials.get(name)
    if mat is None:
        mat = bpy.data.materials.new(name)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes["Principled BSDF"]
        bsdf.inputs["Roughness"].default_value = 0.85
        if image is not None:
            tex = mat.node_tree.nodes.new("ShaderNodeTexImage")
            tex.image = image
            mat.node_tree.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
        else:
            bsdf.inputs["Base Color"].default_value = (*COLORS[name], 1)
            mat.diffuse_color = (*COLORS[name], 1)
    return mat


def clear_uvs(obj):
    while obj.data.uv_layers:
        obj.data.uv_layers.remove(obj.data.uv_layers[0])


def link(obj):
    bpy.context.scene.collection.objects.link(obj)
    return obj


def apply_all(obj):
    with bpy.context.temp_override(object=obj, active_object=obj, selected_objects=[obj]):
        for mod in list(obj.modifiers):
            bpy.ops.object.modifier_apply(modifier=mod.name)


def finish(obj, mat):
    for poly in obj.data.polygons:
        poly.use_smooth = True
    obj.data.materials.clear()
    obj.data.materials.append(mat if isinstance(mat, bpy.types.Material) else material(mat))
    if not obj.data.uv_layers:
        # One flat UV, so a textured material reads its plain corner.
        uv = obj.data.uv_layers.new(name="UVMap")
        for loop in uv.data:
            loop.uv = (0.02, 0.02)
    return obj


def slab(name, at, size, mat, rotation=(0, 0, 0)):
    """A thin piece with softened corners: a patch, a pocket."""
    bpy.ops.mesh.primitive_cube_add(size=2, location=at, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = obj.data.name = name
    clear_uvs(obj)
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bev = obj.modifiers.new("Soft", 'BEVEL')
    bev.width, bev.segments = min(size) * 0.9, 2
    apply_all(obj)
    return finish(obj, mat)


def frame(x_axis, hint):
    """A rotation whose local x follows x_axis and local y leans towards hint."""
    x = x_axis.normalized()
    y = hint - x * hint.dot(x)
    if y.length < 1e-4:
        y = Vector((0, 0, 1)) - x * x.z
    y.normalize()
    return Matrix((x, y, x.cross(y))).transposed()


def skeleton(name, joints, bones, radii, mat, levels=3, root=0, lumps=0.0, seed=0):
    """Flesh on a skeleton: joints joined by bones, each joint given a radius (side, front)."""
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata([tuple(j) for j in joints], bones, [])
    obj = link(bpy.data.objects.new(name, mesh))
    obj.modifiers.new("Skin", 'SKIN')
    for i, r in enumerate(radii):
        mesh.skin_vertices[0].data[i].radius = r
    mesh.skin_vertices[0].data[root].use_root = True
    sub = obj.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = sub.render_levels = levels
    apply_all(obj)
    roughen(obj, lumps, seed)
    return finish(obj, mat)


def roughen(obj, lumps, seed):
    if lumps > 0:
        # A little irregularity, so cloth and flesh do not read as perfect tubes.
        rng = random.Random(seed)
        offset = Vector((rng.uniform(0, 50), rng.uniform(0, 50), rng.uniform(0, 50)))
        for v in obj.data.vertices:
            v.co += v.normal * noise.noise(v.co * 9 + offset) * lumps


def fuse(name, parts, mat, voxel, lumps=0.0, seed=0):
    """Several simple limbs fused into one closed surface (a voxel remesh, then smoothed)."""
    with bpy.context.temp_override(active_object=parts[0], selected_editable_objects=parts, selected_objects=parts):
        bpy.ops.object.join()
    obj = parts[0]
    obj.name = obj.data.name = name
    voxels = obj.modifiers.new("Closed", 'REMESH')
    voxels.mode, voxels.voxel_size = 'VOXEL', voxel
    soft = obj.modifiers.new("Soft", 'SMOOTH')
    soft.factor, soft.iterations = 0.5, 5
    apply_all(obj)
    roughen(obj, lumps, seed)
    clear_uvs(obj)
    return finish(obj, mat)


def blobs(name, items, mat, resolution=0.008):
    """A metaball cluster: items are (centre, radius[, size[, rotation]])."""
    mb = bpy.data.metaballs.new("MB" + name)
    mb.resolution = mb.render_resolution = resolution
    mb.threshold = 0.6
    for item in items:
        at, r = item[0], item[1]
        size = item[2] if len(item) > 2 else None
        rot = item[3] if len(item) > 3 else None
        e = mb.elements.new(type='ELLIPSOID' if size else 'BALL')
        e.co = at
        e.radius = r * BALL
        e.stiffness = 2.0
        if size:
            e.size_x, e.size_y, e.size_z = size
        if rot is not None:
            e.rotation = rot.to_quaternion()
    holder = link(bpy.data.objects.new("MB" + name, mb))
    evaluated = holder.evaluated_get(bpy.context.evaluated_depsgraph_get())
    mesh = bpy.data.meshes.new_from_object(evaluated)
    bpy.data.objects.remove(holder)
    bpy.data.metaballs.remove(mb)
    mesh.name = name
    return finish(link(bpy.data.objects.new(name, mesh)), mat)


def ellipsoid(name, at, size, mat, rotation=(0, 0, 0), segments=24):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=segments // 2, radius=1, location=at)
    obj = bpy.context.active_object
    obj.name = obj.data.name = name
    clear_uvs(obj)
    obj.scale = size
    obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return finish(obj, mat)


def skirt(name, waist, hem, top_r, hem_r, depth, mat, seed, front_open=False, centre=(0, 0), ragged=0.0):
    """A coat or smock skirt: a flared tube from the waist to the hem, with a soft wobble."""
    rng = random.Random(seed)
    rings, around = 7, 32
    bm = bmesh.new()
    verts = []
    for k in range(rings + 1):
        t = k / rings
        z = waist + (hem - waist) * t
        r = top_r + (hem_r - top_r) * (t ** 0.8)
        ring = []
        for a in range(around):
            ang = a / around * math.tau
            wobble = 1 + 0.07 * math.sin(ang * 3 + seed) * t + 0.03 * math.sin(ang * 7 + seed * 2) * t + rng.uniform(-0.012, 0.012)
            drop = 0.025 * t * math.sin(ang * 2 + seed)
            if k == rings and ragged:
                drop += ragged * (0.5 + 0.5 * math.sin(ang * 11 + seed)) * rng.uniform(0.4, 1.0)
            ring.append(bm.verts.new((centre[0] + math.cos(ang) * r * wobble, centre[1] + math.sin(ang) * r * depth * wobble, z - drop)))
        verts.append(ring)
    for k in range(rings):
        for a in range(around):
            if front_open and k > 0 and abs(a - around * 3 // 4) < 1:
                continue
            b = (a + 1) % around
            bm.faces.new((verts[k][a], verts[k][b], verts[k + 1][b], verts[k + 1][a]))
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = link(bpy.data.objects.new(name, mesh))
    obj.modifiers.new("Thickness", 'SOLIDIFY').thickness = 0.014
    sub = obj.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = sub.render_levels = 1
    apply_all(obj)
    return finish(obj, mat)


def cylinder(name, at, radius, depth, mat, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=20, radius=radius, depth=depth, location=at, rotation=rotation)
    obj = bpy.context.active_object
    obj.name = obj.data.name = name
    clear_uvs(obj)
    bev = obj.modifiers.new("Soft", 'BEVEL')
    bev.width, bev.segments = radius * 0.2, 2
    apply_all(obj)
    return finish(obj, mat)


# ---------------------------------------------------------------- painted faces

class Canvas:
    """A small painter for faces: soft round brush stamps along curves. Rows run bottom to top,
    and positions are in head radii from the head's centre (x to the figure's left, z up)."""

    def __init__(self, colour, seed):
        self.n = FACE_SIZE
        self.rgb = np.empty((self.n, self.n, 3), np.float32)
        self.rgb[:] = colour
        self.rng = random.Random(seed)

    def px(self, x, z):
        return (0.5 + x / (2 * FACE_SPAN)) * self.n, (0.5 + z / (2 * FACE_SPAN)) * self.n

    def scale(self, w):
        return w / (2 * FACE_SPAN) * self.n

    def _composite(self, mask, colour, opacity):
        a = (mask * opacity)[..., None]
        self.rgb[:] = self.rgb * (1 - a) + np.asarray(colour, np.float32) * a

    def _disc(self, mask, cx, cy, r):
        x0, x1 = max(0, int(cx - r - 2)), min(self.n, int(cx + r + 3))
        y0, y1 = max(0, int(cy - r - 2)), min(self.n, int(cy + r + 3))
        if x0 >= x1 or y0 >= y1:
            return
        ys, xs = np.mgrid[y0:y1, x0:x1]
        a = np.clip(r - np.hypot(xs + 0.5 - cx, ys + 0.5 - cy) + 0.5, 0, 1)
        np.maximum(mask[y0:y1, x0:x1], a, out=mask[y0:y1, x0:x1])

    def stroke(self, points, width, colour=INK, opacity=1.0, taper=0.35, jitter=0.006):
        """A brush stroke through points, tapered at both ends, with a hand's small wobble."""
        pts = [Vector(self.px(x + self.rng.uniform(-jitter, jitter), z + self.rng.uniform(-jitter, jitter))) for x, z in points]
        if len(pts) == 2:
            pts = [pts[0], pts[0].lerp(pts[1], 0.5), pts[1]]
        ext = [pts[0] * 2 - pts[1]] + pts + [pts[-1] * 2 - pts[-2]]
        dense = []
        for i in range(1, len(ext) - 2):
            p0, p1, p2, p3 = ext[i - 1], ext[i], ext[i + 1], ext[i + 2]
            for k in range(24):
                t = k / 24
                dense.append(0.5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t ** 3))
        dense.append(pts[-1])
        lengths = [0.0]
        for a, b in zip(dense, dense[1:]):
            lengths.append(lengths[-1] + (b - a).length)
        total = max(lengths[-1], 1e-3)
        mask = np.zeros((self.n, self.n), np.float32)
        w = self.scale(width)
        step = max(0.5, w * 0.15)
        s, i = 0.0, 0
        phase = self.rng.uniform(0, 10)
        while s <= total:
            while i < len(lengths) - 2 and lengths[i + 1] < s:
                i += 1
            seg = max(lengths[i + 1] - lengths[i], 1e-6)
            p = dense[i].lerp(dense[i + 1], min(1.0, (s - lengths[i]) / seg))
            t = s / total
            ends = min(1.0, min(t, 1 - t) / taper) if taper > 0 else 1.0
            r = 0.5 * w * (0.25 + 0.75 * math.sqrt(max(ends, 0))) * (1 + 0.12 * math.sin(t * 9 + phase))
            self._disc(mask, p.x, p.y, r)
            s += step
        self._composite(mask, colour, opacity)

    def ellipse(self, centre, radii, colour, opacity=1.0, inside=None):
        """A filled ellipse; inside(x, z) may clip it (positions in head radii)."""
        cx, cy = self.px(*centre)
        rx, ry = self.scale(radii[0]), self.scale(radii[1])
        x0, x1 = max(0, int(cx - rx - 2)), min(self.n, int(cx + rx + 3))
        y0, y1 = max(0, int(cy - ry - 2)), min(self.n, int(cy + ry + 3))
        ys, xs = np.mgrid[y0:y1, x0:x1]
        d = np.sqrt(((xs + 0.5 - cx) / rx) ** 2 + ((ys + 0.5 - cy) / ry) ** 2)
        a = np.clip((1 - d) * min(rx, ry) + 0.5, 0, 1)
        if inside is not None:
            fx = ((xs + 0.5) / self.n - 0.5) * 2 * FACE_SPAN
            fz = ((ys + 0.5) / self.n - 0.5) * 2 * FACE_SPAN
            a = a * inside(fx, fz)
        mask = np.zeros((self.n, self.n), np.float32)
        mask[y0:y1, x0:x1] = a
        self._composite(mask, colour, opacity)

    def glow(self, centre, radius, colour, opacity):
        """A soft round wash, such as a blush."""
        cx, cy = self.px(*centre)
        r = self.scale(radius)
        ys, xs = np.mgrid[0:self.n, 0:self.n]
        a = np.clip(1 - np.hypot(xs + 0.5 - cx, ys + 0.5 - cy) / r, 0, 1) ** 1.6
        self._composite(a.astype(np.float32), colour, opacity)

    def save(self, path, name):
        img = bpy.data.images.new(name, self.n, self.n, alpha=False)
        rgba = np.ones((self.n, self.n, 4), np.float32)
        rgba[..., :3] = np.clip(self.rgb, 0, 1)
        img.pixels.foreach_set(rgba.ravel())
        img.filepath_raw = path
        img.file_format = 'PNG'
        img.save()
        return img


def arc(cx, cz, w, h, n=5, tilt=0.0):
    """Points along an arch (h > 0) or a cup (h < 0) of half-width w."""
    return [(cx + w * (2 * k / (n - 1) - 1), cz + h * (1 - (2 * k / (n - 1) - 1) ** 2) + tilt * (2 * k / (n - 1) - 1)) for k in range(n)]


def face_round(c):
    """Laughing: closed, happy eyes, big rosy cheeks, a wide smile."""
    for s in (-1, 1):
        c.glow((s * 0.52, -0.33), 0.3, (0.90, 0.42, 0.38), 0.55)
    for s in (-1, 1):
        ex, ez = s * 0.36, -0.02
        c.stroke(arc(ex, ez, 0.11, 0.07), 0.05)
        c.stroke([(ex + s * 0.1, ez - 0.01), (ex + s * 0.15, ez + 0.02)], 0.025, opacity=0.8)
        c.stroke(arc(ex - s * 0.01, ez + 0.24, 0.1, 0.035, tilt=s * 0.015), 0.055, (0.17, 0.09, 0.06))
        c.stroke([(s * 0.25, -0.47), (s * 0.27, -0.55)], 0.02, opacity=0.35)
    c.ellipse((0, -0.6), (0.15, 0.065), (0.50, 0.17, 0.15), inside=lambda x, z: (z < -0.58 + 0.25 * (x / 0.2) ** 2).astype(np.float32))
    c.stroke(arc(0, -0.55, 0.21, -0.07), 0.035)


def face_long(c):
    """Sleepy and kind: heavy lids over small dark eyes, long brows sloping down, a crooked smile."""
    for s in (-1, 1):
        c.glow((s * 0.45, -0.3), 0.24, (0.85, 0.48, 0.42), 0.25)
    for s in (-1, 1):
        ex, ez = s * 0.30, 0.02
        lid = lambda x, z, ez=ez: (z < ez + 0.012).astype(np.float32)
        c.ellipse((ex + 0.012, ez - 0.012), (0.052, 0.05), INK, inside=lid)
        c.stroke([(ex - s * 0.11, ez + 0.005), (ex - s * 0.03, ez + 0.03), (ex + s * 0.06, ez + 0.03), (ex + s * 0.125, ez - 0.01)], 0.045)
        c.stroke([(ex - 0.07, ez - 0.085), (ex + 0.07, ez - 0.09)], 0.015, opacity=0.35)
        c.stroke([(s * 0.12, 0.22), (s * 0.28, 0.235), (s * 0.45, 0.16)], 0.036, (0.14, 0.08, 0.06))
    c.stroke([(-0.17, -0.63), (0.0, -0.655), (0.15, -0.615), (0.2, -0.56)], 0.026)
    c.stroke([(0.21, -0.54), (0.235, -0.6)], 0.015, opacity=0.4)


def face_small(c):
    """Curious and young: big dark eyes glancing aside, a soft blush, a small parted mouth."""
    for s in (-1, 1):
        c.glow((s * 0.48, -0.4), 0.2, (0.95, 0.52, 0.50), 0.45)
        for k in range(3):
            x = s * (0.42 + 0.06 * k)
            c.stroke([(x, -0.36), (x - 0.03, -0.43)], 0.012, (0.8, 0.35, 0.33), opacity=0.45, jitter=0.002)
    for s in (-1, 1):
        ex, ez = s * 0.37, -0.1
        X = lambda dx, ex=ex, s=s: ex + s * dx  # dx > 0 towards the eye's outer corner
        almond = lambda x, z, ex=ex, ez=ez: (z < ez + 0.085 - 0.6 * (x - ex) ** 2).astype(np.float32)
        c.ellipse((ex, ez), (0.155, 0.11), (0.97, 0.94, 0.90), inside=almond)
        c.ellipse((ex + 0.045, ez - 0.01), (0.095, 0.12), (0.10, 0.065, 0.06), inside=almond)
        c.ellipse((ex + 0.075, ez + 0.035), (0.025, 0.025), (1, 1, 1))
        c.stroke([(X(-0.16), ez + 0.005), (X(-0.06), ez + 0.09), (X(0.08), ez + 0.095), (X(0.165), ez + 0.03), (X(0.2), ez + 0.06)], 0.045)
        c.stroke([(X(-0.04), ez - 0.115), (X(0.08), ez - 0.108)], 0.014, opacity=0.5)
        c.stroke(arc(ex, ez + 0.25, 0.08, 0.02), 0.025, (0.12, 0.07, 0.06))
    c.ellipse((0, -0.65), (0.05, 0.022), (0.85, 0.48, 0.46), 0.8)
    c.stroke([(-0.055, -0.625), (0.0, -0.63), (0.055, -0.622)], 0.018)


# ---------------------------------------------------------------- the figure

def hand(name, s, el, wr, h, grip, mat):
    """A simple, expressive hand: a palm, fingers grouped like a mitten with tips, and a thumb."""
    a = (wr - el).normalized()
    m = frame(a, Vector((0, -1, 0)))
    t, n = m.col[1], m.col[2]
    if n.x * s > 0:  # palm faces the body
        n = -n
    r = frame(a, t)
    items = []
    if grip == "open":
        items.append((wr + a * h * 0.55, h * 0.5, (1.1, 1.0, 0.5), r))
        items.append((wr + a * h * 1.25 + n * h * 0.12, h * 0.38, (1.3, 1.15, 0.55), r))
        for k in range(4):
            items.append((wr + a * h * 1.72 + n * h * 0.22 + t * (k - 1.5) * h * 0.24, h * 0.13))
        items.append((wr + a * h * 0.65 + t * h * 0.55 + n * h * 0.15, h * 0.2, (1.7, 0.9, 0.9), frame(a * 0.7 + t * 0.7, n)))
    else:  # a fist, or a hand holding something
        items.append((wr + a * h * 0.55, h * 0.5, (1.0, 1.0, 0.75), r))
        items.append((wr + a * h * 0.95 + n * h * 0.3, h * 0.42, (0.8, 1.15, 0.8), r))
        for k in range(4):
            items.append((wr + a * h * 1.08 + t * (k - 1.5) * h * 0.24, h * 0.13))
        items.append((wr + a * h * 0.75 + t * h * 0.45 + n * h * 0.4, h * 0.18, (1.5, 0.9, 0.9), frame(a - t * 0.3, n)))
    return blobs(name, items, mat, resolution=0.005), wr + a * h * 0.9 + n * h * 0.3


def figure(name, p, seed):
    """One standing worker. Front faces -Y, up is +Z, feet on z = 0; x is the figure's left."""
    rng = random.Random(seed)
    H = p["height"]
    hr = p["head"]
    sw = p.get("weight", 1)  # the leg that carries the weight
    shift = p.get("hip_shift", 0.03)
    lean = p.get("stoop", 0.0)
    objs = []

    pelvis = Vector((sw * shift, 0, p["hip"] * H))
    belly = Vector((sw * shift * 0.6, -0.01 - lean * 0.03, (p["hip"] + 0.35 * (p["chest"] - p["hip"])) * H))
    chest = Vector((sw * shift * 0.2, -lean * 0.08 + p.get("back", 0) * 0.03, p["chest"] * H))
    collar = chest + Vector((0, -lean * 0.03, 0.07 * H))

    # The head: turned, tilted and nodded; everything on it is built in its own frame.
    hm = (Matrix.Rotation(p.get("turn", 0), 3, 'Z') @ Matrix.Rotation(p.get("nod", 0), 3, 'X')
          @ Matrix.Rotation(p.get("tilt", 0), 3, 'Y'))
    head = Vector((sw * shift * -0.2, -lean * 0.22 - 0.01 + p.get("back", 0) * 0.02, H - hr * 1.15))
    hs = Vector(p.get("head_shape", (1, 1, 1)))

    def on_head(x, y, z):
        return head + hm @ Vector((x * hs.x, y * hs.y, z * hs.z)) * hr

    # Top: torso and arms in the garment, on the rig's own joints. Shoulders tilt against the hips.
    tilt = p.get("shoulder_tilt", 0.02)
    sh_w = p["shoulder_w"]
    shoulders = [chest + Vector((s * sh_w, 0.01, p.get("shoulder_h", 0.05) * H - s * sw * tilt)) for s in (-1, 1)]
    elbows = [sh + Vector((s * pose[0][0], pose[0][1], pose[0][2])) for s, sh, pose in zip((-1, 1), shoulders, p["arms"])]
    wrists = [el + Vector((s * pose[1][0], pose[1][1], pose[1][2])) for s, el, pose in zip((-1, 1), elbows, p["arms"])]
    tp = p["torso"]  # (pelvis, belly, chest, collar) half-widths; depth is a fraction of width
    d = p.get("torso_depth", 0.75)
    ar = p["arm"]
    cuff = ar * p.get("cuff", 1.0)
    rolled = p.get("rolled", False)
    end = ar * 0.85 if rolled else cuff
    # The garment's top: a torso and one chain per arm, each simple, fused into one closed surface.
    parts = [skeleton(f"{name}_Torso", [pelvis, belly, chest, collar], [(0, 1), (1, 2), (2, 3)],
                      [(tp[0], tp[0] * d), (tp[1], tp[1] * d), (tp[2], tp[2] * d), (tp[3], tp[3] * d)], p["top_mat"])]
    for i, (sh, el, wr) in enumerate(zip(shoulders, elbows, wrists)):
        inside = chest.lerp(sh, 0.45) + Vector((0, 0, 0.03))
        parts.append(skeleton(f"{name}_Arm{i}", [inside, sh, el, wr], [(0, 1), (1, 2), (2, 3)],
                              [(ar * 1.2, ar * 1.2), (ar * 1.15, ar * 1.15), (ar * 1.0, ar * 1.0), (end, end)], p["top_mat"]))
    objs.append(fuse(f"{name}_Top", parts, p["top_mat"], 0.008, lumps=0.006, seed=seed))
    if rolled:
        # Rolled sleeves: bare forearms below a thick cuff at the elbow.
        for i, (el, wr) in enumerate(zip(elbows, wrists)):
            objs.append(ellipsoid(f"{name}_Roll{i}", el + (wr - el) * 0.12, (ar * 0.75, ar * 1.3, ar * 1.3), p["top_mat"],
                                  rotation=frame(wr - el, Vector((0, 0, 1))).to_euler()))
            objs.append(skeleton(f"{name}_Forearm{i}", [el + (wr - el) * 0.1, wr], [(0, 1)], [(ar * 0.8, ar * 0.74), (ar * 0.6, ar * 0.55)], p["skin"], levels=2))
    if p.get("skirt"):
        hem, flare = p["skirt"]
        objs.append(skirt(f"{name}_Skirt", pelvis.z + 0.05, hem * H, tp[0] * 1.02, tp[0] * flare, d * 1.05, p["top_mat"], seed,
                          front_open=p.get("open_front", False), centre=(pelvis.x, 0), ragged=p.get("ragged", 0.0)))

    # Legs in trousers, and big sturdy boots: one leg carries the weight, the other rests.
    hw = p["hip_w"]
    hips = [pelvis + Vector((s * hw, 0, -0.03 + s * sw * p.get("hip_tilt", 0.015))) for s in (-1, 1)]
    for i, (s, hp) in enumerate(zip((-1, 1), hips)):
        if s == sw:
            ankle = Vector((head.x + s * 0.03, 0.0, 0.11))
            knee = hp.lerp(ankle, 0.5) + Vector((0, -0.01, 0))
        else:
            ankle = Vector((s * hw * p.get("stance", 1.6), -0.07, 0.11))
            knee = hp.lerp(ankle, 0.5) + Vector((-s * 0.015, -0.05, 0.01))
        lr = p["leg"]
        objs.append(skeleton(f"{name}_Leg{i}", [hp, knee, ankle], [(0, 1), (1, 2)],
                             [(lr * 1.15, lr * 1.1), (lr * 0.95, lr * 0.95), (lr * 0.92, lr * 0.92)], p.get("legs_mat", "Trousers"),
                             lumps=0.004, seed=seed + i))
        b = p["boot"]
        yaw = Matrix.Rotation(s * 0.28 + (0.1 * s if s != sw else 0), 3, 'Z')
        at = lambda x, y, z, ankle=ankle, yaw=yaw: Vector((ankle.x, 0, 0)) + yaw @ Vector((x, y + ankle.y, z))
        objs.append(blobs(f"{name}_Boot{i}", [(Vector((ankle.x, ankle.y, ankle.z - 0.01)), b * 0.85, (1.0, 1.0, 1.0)),
                                                (at(0, -b * 0.9, 0.055), b, (0.85, 1.4, 0.62), yaw),
                                                (at(0, -b * 0.15, 0.05), b * 0.9, (0.9, 1.1, 0.6), yaw)], "Boots"))

    # Neck, then the head: skull, jaw, cheeks, chin, brow, ears, and the one strong feature, the nose.
    nk = p.get("neck_r", 0.5)
    objs.append(skeleton(f"{name}_Neck", [collar + Vector((0, 0.01, -0.02)), on_head(0, 0.15, -0.8)], [(0, 1)],
                         [(hr * nk, hr * nk * 0.95), (hr * nk * 0.92, hr * nk * 0.9)], p["skin"]))
    if p.get("collar"):
        hi, wide = p["collar"]
        objs.append(skirt(f"{name}_Collar", collar.z + hi, collar.z - 0.01, hr * nk * 1.4, hr * nk * wide, 0.9, p["top_mat"], seed,
                          front_open=True, centre=(collar.x, collar.y)))
    ch, jw = p.get("cheeks", 0.3), p.get("jaw", 0.7)
    items = [(on_head(0, 0.05, 0.1), hr, (hs.x, hs.y, hs.z * 1.05), hm),
             (on_head(0, -0.22, -0.42), hr * jw, (0.98 * hs.x, 0.9, 0.85 * hs.z), hm),
             (on_head(0, -0.78, 0.3), hr * 0.32, (1.5, 0.5, 0.45), hm),
             (on_head(0, -0.55, -0.82 * p.get("chin_drop", 1.0)), hr * p.get("chin", 0.28))]
    for s in (-1, 1):
        items.append((on_head(s * 0.47, -0.58, -0.33), hr * ch))
        items.append((on_head(s * 0.97, 0.05, -0.1), hr * 0.22, (0.45, 0.75, 1.15), hm))
    for at, r, size in p["nose"]:
        items.append((on_head(*at), hr * r, size, hm))
    skin = blobs(f"{name}_Skin", items, p["skin"], resolution=0.005)
    objs.append(skin)

    # The face is painted, projected onto the head from the front.
    uv = skin.data.uv_layers[0]
    inverse = hm.transposed()
    for poly in skin.data.polygons:
        for li in poly.loop_indices:
            local = inverse @ (skin.data.vertices[skin.data.loops[li].vertex_index].co - head) / hr
            x, y, z = local.x / hs.x, local.y / hs.y, local.z / hs.z
            uv.data[li].uv = (0.5 + x / (2 * FACE_SPAN), 0.5 + z / (2 * FACE_SPAN)) if y < 0.2 else (0.02, 0.02)

    # Hands: big, simple and expressive.
    held = []
    for i, (s, el, wr, grip) in enumerate(zip((-1, 1), elbows, wrists, p["grips"])):
        if grip == "hidden":
            continue
        obj, grasp = hand(f"{name}_Hand{i}", s, el, wr, p["hand"], grip, p["skin"])
        objs.append(obj)
        held.append((grip, grasp, s))

    # Hair: a confident dark shape made of locks.
    objs.append(hair(name, p, on_head, hm, hr, rng))

    # Character pieces: an apron, a satchel, patches, a mug.
    for piece in p.get("pieces", []):
        objs.extend(piece(name, locals()))
    return objs


def hair(name, p, on_head, hm, hr, rng):
    style = p["hair"]
    items = []
    cap = p.get("cap", (0, 0.12, 0.3, 1.04))
    items.append((on_head(cap[0], cap[1], cap[2]), hr * cap[3], (1.04, 1.06, 0.95), hm))
    if style == "bob":
        # A bob: a fringe cut straight with a few points, and sides down to the jaw.
        items.append((on_head(0, 0.4, -0.3), hr * 0.85, (1.15, 0.8, 0.95), hm))
        for k in range(7):
            x = -0.78 + 1.56 * k / 6
            items.append((on_head(x, -0.98 + abs(x) * 0.3, 0.5 - abs(x) * 0.06), hr * 0.21, (0.9, 0.55, 1.5), hm))
        for s in (-1, 1):
            for k in range(4):
                items.append((on_head(s * (0.9 + 0.04 * k), -0.35 + 0.2 * k, -0.45 - 0.08 * (k % 2)), hr * 0.3, (0.55, 0.9, 1.5), hm))
    elif style == "swept":
        # Swept to one side and forward, with a few strands standing up at the crown.
        sweep = Vector((-0.6, -0.4, -0.35)).normalized()
        for k in range(16):
            az = rng.uniform(0, math.tau)
            el = rng.uniform(0.35, 1.35)
            dirn = Vector((math.cos(az) * math.cos(el), math.sin(az) * math.cos(el), math.sin(el)))
            if dirn.y < -0.3 and el < 0.9:
                continue
            down = Vector((0, 0, -1)) + dirn * dirn.z
            lock = (sweep * 0.75 + down.normalized() * 0.35 + dirn * 0.35
                    + Vector((rng.uniform(-0.15, 0.15), rng.uniform(-0.15, 0.15), rng.uniform(-0.1, 0.1)))).normalized()
            at = on_head(dirn.x * 0.98, dirn.y * 0.98 + 0.12, dirn.z * 0.95 + 0.3)
            items.append((at + hm @ lock * hr * 0.25, hr * rng.uniform(0.2, 0.26), (rng.uniform(1.9, 2.5), 0.8, 0.6), hm @ frame(lock, Vector((0, 0, 1)))))
        for k in range(3):
            items.append((on_head(0.05 + 0.12 * k, 0.32, 1.22), hr * 0.1, (2.6, 0.8, 0.8), hm @ frame(Vector((0.3 * k - 0.2, 0.5, 1)), Vector((0, 1, 0)))))
        items.append((on_head(-0.3, -0.86, 0.62), hr * 0.18, (2.2, 0.8, 0.6), hm @ frame(Vector((-0.5, -0.35, -1)), Vector((0, -1, 0)))))
    else:
        mop = style == "mop"
        count = 34 if mop else 44
        for k in range(count):
            az = rng.uniform(0, math.tau)
            el = rng.uniform(0.0 if mop else 0.35, 1.4)
            dirn = Vector((math.cos(az) * math.cos(el), math.sin(az) * math.cos(el), math.sin(el)))
            if dirn.y < -0.35 and el < 0.9:
                continue  # keep the face clear
            down = Vector((0, 0, -1)) + dirn * dirn.z  # down the skull's surface
            if down.length < 0.2:
                down = Vector((math.cos(az), math.sin(az), 0)) * 0.2
            lock = (down.normalized() * (0.85 if mop else 0.3) + dirn * (0.3 if mop else 0.8)
                    + Vector((rng.uniform(-0.2, 0.2), rng.uniform(-0.2, 0.2), rng.uniform(-0.1, 0.25)))).normalized()
            at = on_head(dirn.x * 0.98, dirn.y * 0.98 + 0.1, dirn.z * 0.95 + 0.25)
            stretch = rng.uniform(1.5, 2.0) if mop else rng.uniform(1.5, 2.1)
            size = rng.uniform(0.24, 0.32) if mop else rng.uniform(0.17, 0.23)
            items.append((at + hm @ lock * hr * 0.2, hr * size, (stretch, 0.85, 0.7), hm @ frame(lock, Vector((0, 0, 1)))))
        if mop:
            for k in range(5):  # a fringe falling over the forehead
                x = -0.55 + 0.27 * k
                items.append((on_head(x, -0.72, 0.62), hr * 0.2, (1.4, 0.7, 0.7), hm @ frame(Vector((x * 0.6, -0.5, -1)), Vector((0, -1, 0)))))
        else:
            # A cowlick standing up at the crown.
            items.append((on_head(0.1, 0.25, 1.15), hr * 0.18, (2.0, 0.7, 0.7), hm @ frame(Vector((0.4, 0.3, 1)), Vector((0, 1, 0)))))
    return blobs(f"{name}_Hair", items, p.get("hair_mat", "Hair"), resolution=0.007)


def apron(name, v):
    """A worn canvas apron tied over the smock, with a pocket."""
    pelvis, belly, tp, d = v["pelvis"], v["belly"], v["tp"], v["d"]
    front = -tp[1] * d - 0.004
    x = pelvis.x
    a = skeleton(f"{name}_Apron", [Vector((x, front + 0.02, belly.z - 0.01)), Vector((x, front - 0.01, pelvis.z - 0.08)), Vector((x, front - 0.035, pelvis.z - 0.36))],
                 [(0, 1), (1, 2)], [(tp[1] * 0.78, 0.01), (tp[0] * 0.86, 0.01), (tp[0] * 0.92, 0.01)], "Apron", levels=2, lumps=0.004, seed=5)
    tie = skeleton(f"{name}_ApronTie", [Vector((x - tp[1] * 0.86, -0.03, belly.z - 0.01)), Vector((x, front + 0.012, belly.z - 0.005)), Vector((x + tp[1] * 0.86, -0.03, belly.z - 0.01))],
                   [(0, 1), (1, 2)], [(0.012, 0.012)] * 3, "Apron", levels=2)
    pocket = slab(f"{name}_Pocket", (x + tp[0] * 0.35, front - 0.036, pelvis.z - 0.18), (0.055, 0.006, 0.045), "Patch", rotation=(0, 0.08, 0))
    return [a, tie, pocket]


def satchel(name, v):
    """A small leather satchel on a strap across the body, in a warm accent."""
    shoulders, pelvis, tp, d = v["shoulders"], v["pelvis"], v["tp"], v["d"]
    bag_at = Vector((pelvis.x - tp[0] * 1.15, -0.04, pelvis.z - 0.02))
    strap = skeleton(f"{name}_Strap", [shoulders[1] + Vector((0, 0, 0.04)), Vector((pelvis.x, -tp[2] * d - 0.03, (shoulders[1].z + pelvis.z) * 0.5)), bag_at + Vector((0.02, 0, 0.08))],
                     [(0, 1), (1, 2)], [(0.022, 0.01), (0.022, 0.01), (0.022, 0.01)], "Leather", levels=1)
    bag = blobs(f"{name}_Bag", [(bag_at, 0.08, (1.0, 0.55, 0.85)), (bag_at + Vector((0, -0.03, 0.05)), 0.06, (1.05, 0.35, 0.5))], "Accent")
    return [strap, bag]


def patches(name, v):
    """Patches sewn on the coat: a worn life beyond the frame."""
    chest, pelvis, tp, d = v["chest"], v["pelvis"], v["tp"], v["d"]
    return [slab(f"{name}_Patch0", (chest.x + tp[1] * 0.5, -tp[1] * d - 0.004, (chest.z + pelvis.z) * 0.5), (0.055, 0.006, 0.05), "Patch", rotation=(0, 0.2, 0)),
            slab(f"{name}_Patch1", (pelvis.x - tp[0] * 0.7, -tp[0] * d * 1.1 - 0.014, pelvis.z - 0.32), (0.05, 0.006, 0.06), "Patch", rotation=(0, -0.12, 0))]


def lapels(name, v):
    """An open coat's lapels, folded back on the chest."""
    chest, collar, tp, d = v["chest"], v["collar"], v["tp"], v["d"]
    return [ellipsoid(f"{name}_Lapel{i}", (chest.x + s * tp[2] * 0.42, -tp[2] * d - 0.006, (chest.z + collar.z) * 0.5 - 0.02), (0.045, 0.01, 0.12), v["p"]["top_mat"],
                      rotation=(0, s * 0.35, 0)) for i, s in enumerate((-1, 1))]


def mug(name, v):
    """A tin mug in the hand that holds something."""
    out = []
    for grip, grasp, s in v["held"]:
        if grip == "hold":
            out.append(cylinder(f"{name}_Mug", grasp + Vector((0, -0.035, 0.0)), 0.038, 0.085, "Mug"))
            out.append(skeleton(f"{name}_MugHandle", [grasp + Vector((s * 0.035, -0.035, 0.025)), grasp + Vector((s * 0.06, -0.035, 0.0)), grasp + Vector((s * 0.035, -0.035, -0.025))],
                                [(0, 1), (1, 2)], [(0.008, 0.008)] * 3, "Mug", levels=1))
    return out


CONCEPTS = {
    # Round: short and heavy-set. A barrel body, fists on the hips, a bulb nose, laughing eyes, an apron.
    "Worker_Round": (dict(height=1.56, hip=0.47, chest=0.70, head=0.128, head_shape=(1.08, 1.0, 0.98), shoulder_w=0.20, hip_w=0.10,
                          torso=(0.20, 0.235, 0.21, 0.11), torso_depth=0.82, arm=0.056, shoulder_h=0.03, leg=0.072, hand=0.054, boot=0.07,
                          weight=-1, hip_shift=0.035, shoulder_tilt=0.025, back=1.0, tilt=0.14, turn=0.12, nod=-0.08,
                          nose=[((0, -0.95, -0.12), 0.12, (0.8, 1.0, 1.4)), ((0, -1.1, -0.3), 0.22, (1.05, 0.95, 0.95))],
                          cheeks=0.34, jaw=0.74, chin=0.3, neck_r=0.62, face=face_round, skin="Skin_Round",
                          rolled=True, top_mat="Smock", hair="mop", hair_mat="HairBrown", cap=(0, 0.2, 0.45, 1.02),
                          skirt=(0.36, 1.18), ragged=0.02, stance=1.7, pieces=[apron],
                          # Fists on the hips: sure of the work ahead.
                          arms=[((0.14, 0.0, -0.14), (-0.06, -0.02, -0.12)), ((0.14, 0.0, -0.14), (-0.06, -0.02, -0.12))],
                          grips=["fist", "fist"]), 3),
    # Long: tall and thin with a stoop. A long face, a long nose, sleepy kind eyes, a mug of tea.
    "Worker_Long": (dict(height=1.86, hip=0.52, chest=0.735, head=0.112, head_shape=(0.94, 1.0, 1.14), shoulder_w=0.17, hip_w=0.085,
                         torso=(0.135, 0.13, 0.155, 0.10), torso_depth=0.72, arm=0.044, leg=0.055, hand=0.05, boot=0.068,
                         weight=1, hip_shift=0.04, shoulder_tilt=0.03, stoop=0.7, tilt=-0.16, turn=-0.18, nod=0.22,
                         nose=[((0, -0.92, -0.08), 0.11, (0.75, 1.0, 1.6)), ((0, -1.12, -0.25), 0.11, (0.8, 2.0, 0.9)), ((0, -1.3, -0.36), 0.1, (0.9, 1.0, 1.0))],
                         cheeks=0.2, jaw=0.62, chin=0.27, chin_drop=1.1, neck_r=0.5, face=face_long, skin="Skin_Long",
                         top_mat="Coat", skirt=(0.22, 1.4), open_front=True, ragged=0.04, cuff=1.3, collar=(0.08, 2.1),
                         hair="swept", cap=(0, 0.16, 0.45, 0.99), stance=1.5, pieces=[patches, lapels, mug],
                         # One hand in a pocket, the other holding a mug: unhurried, thoughtful.
                         arms=[((0.06, 0.03, -0.26), (-0.02, -0.06, -0.2)), ((0.05, -0.04, -0.26), (-0.04, -0.2, -0.08))],
                         grips=["hidden", "hold"]), 11),
    # Small: young and small-framed. A dark bob, an oversized coat with long sleeves, a satchel, big curious eyes.
    "Worker_Small": (dict(height=1.42, hip=0.46, chest=0.665, head=0.13, head_shape=(1.04, 1.0, 1.0), shoulder_w=0.16, hip_w=0.075,
                          torso=(0.155, 0.16, 0.175, 0.105), torso_depth=0.8, arm=0.05, leg=0.048, hand=0.042, boot=0.06,
                          weight=-1, hip_shift=0.02, shoulder_tilt=0.015, tilt=0.16, turn=0.22, nod=-0.2,
                          nose=[((0, -1.0, -0.3), 0.085, (1.0, 1.0, 0.9))],
                          cheeks=0.3, jaw=0.62, chin=0.24, face=face_small, skin="Skin_Small",
                          top_mat="CoatBlue", hair="bob", collar=(0.09, 2.3), cap=(0, 0.14, 0.32, 1.08),
                          skirt=(0.30, 1.3), ragged=0.015, cuff=1.4, stance=1.4, pieces=[satchel],
                          # One hand on the satchel's strap, the other loose: ready to set out.
                          arms=[((0.05, -0.02, -0.2), (-0.01, -0.06, -0.17)), ((0.06, -0.08, -0.14), (-0.09, -0.06, 0.13))],
                          grips=["open", "fist"]), 21),
}
# Warm skin, a little below white, so light can still find a cheek (sRGB).
SKIN = {"Skin_Round": (0.84, 0.61, 0.49), "Skin_Long": (0.80, 0.60, 0.49), "Skin_Small": (0.88, 0.67, 0.56)}


def build(face_dir):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    built = {}
    for name, (p, seed) in CONCEPTS.items():
        canvas = Canvas(SKIN[p["skin"]], seed)
        p["face"](canvas)
        short = name.replace("Worker_", "")
        image = canvas.save(os.path.join(face_dir, f"Face_{short}.png"), f"Face_{short}")
        material(p["skin"], image)
        built[name] = figure(name, p, seed)
        print(f"CONCEPT {name}: {len(built[name])} parts, {sum(len(o.data.polygons) for o in built[name])} faces")
    return built


def export_fbx(path):
    bpy.ops.export_scene.fbx(filepath=path, use_selection=False, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                             bake_space_transform=True, axis_forward='-Z', axis_up='Y', object_types={'MESH'},
                             use_mesh_modifiers=True, mesh_smooth_type='OFF', colors_type='NONE',
                             add_leaf_bones=False, bake_anim=False, path_mode='STRIP')


def preview(built, path):
    """The three side by side, front and three-quarter, under a warm key and a cool fill."""
    scene = bpy.context.scene
    for k, (name, objs) in enumerate(built.items()):
        for o in objs:
            o.location.x += k * 0.85
            d = o.copy()
            scene.collection.objects.link(d)
            d.location.x = o.location.x + 2.7
            d.rotation_euler = Euler((0, 0, math.radians(-40)))
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
    scene.render.resolution_x, scene.render.resolution_y = 1800, 700
    cam = link(bpy.data.objects.new("Preview", bpy.data.cameras.new("Preview")))
    cam.data.type = 'ORTHO'
    cam.data.ortho_scale = 5.6
    cam.location = (2.55, -8, 0.95)
    cam.rotation_euler = Euler((math.radians(90), 0, 0))
    scene.camera = cam
    for name, rot, energy, color in (("Key", (50, 0, -35), 3.5, (1.0, 0.85, 0.7)), ("Fill", (70, 0, 140), 1.0, (0.6, 0.7, 1.0))):
        light = link(bpy.data.objects.new(name, bpy.data.lights.new(name, 'SUN')))
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
    # Close views of each face, slightly from the side.
    cam.data.type = 'PERSP'
    cam.data.lens = 85
    scene.render.resolution_x, scene.render.resolution_y = 600, 700
    for k, (name, objs) in enumerate(built.items()):
        head = next(o for o in objs if o.name.endswith("_Skin"))
        centre = sum((head.matrix_world @ Vector(c) for c in head.bound_box), Vector()) / 8
        cam.location = centre + Vector((0.35, -1.6, 0.05))
        cam.rotation_euler = (centre - cam.location).to_track_quat('-Z', 'Y').to_euler()
        scene.render.filepath = path.replace(".png", f"_{name.replace('Worker_', '').lower()}.png")
        bpy.ops.render.render(write_still=True)


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--fbx")
    parser.add_argument("--preview")
    args = parser.parse_args(argv)
    faces = os.path.dirname(os.path.abspath(args.fbx or args.preview or "."))
    built = build(faces)
    if args.fbx:
        export_fbx(args.fbx)
    if args.preview:
        preview(built, args.preview)
