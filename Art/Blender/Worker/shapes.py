"""Wonder Gather — shape helpers for beings (S1d).

Small, general tools the body, hair, outfit and accessory modules share:
materials, flesh on joint chains (the skin modifier), metaball clusters, closed
fused surfaces (a voxel remesh), tubes along paths with a shaped cross-section,
and simple solids.

Coordinates: front is -Y, up is +Z, x is the being's left. Units are metres.
"""
import math
import random

import bmesh
import bpy
from mathutils import Matrix, Vector, noise

# Metaballs draw at about 0.58 of their nominal radius (stiffness 2, threshold 0.6).
BALL = 1 / 0.58
# Preview colours (linear) for Blender renders; the engine reads the manifest instead.
PREVIEW = {}


def material(name, colour=None, image=None, emission=None):
    """A named material; the name is what the engine maps to its painted material."""
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
        colour = colour or PREVIEW.get(name, (0.5, 0.5, 0.5))
        if image is None:
            bsdf.inputs["Base Color"].default_value = (*colour, 1)
        if emission is not None:
            bsdf.inputs["Emission Color"].default_value = (*emission, 1)
            bsdf.inputs["Emission Strength"].default_value = 4.0
        mat.diffuse_color = (*colour, 1)
    return mat


def link(obj):
    bpy.context.scene.collection.objects.link(obj)
    return obj


def apply_all(obj):
    with bpy.context.temp_override(object=obj, active_object=obj, selected_objects=[obj]):
        for mod in list(obj.modifiers):
            bpy.ops.object.modifier_apply(modifier=mod.name)


def clear_uvs(obj):
    while obj.data.uv_layers:
        obj.data.uv_layers.remove(obj.data.uv_layers[0])


def finish(obj, mat, smooth=True):
    for poly in obj.data.polygons:
        poly.use_smooth = smooth
    obj.data.materials.clear()
    obj.data.materials.append(mat if isinstance(mat, bpy.types.Material) else material(mat))
    if not obj.data.uv_layers:
        # One flat UV, so a textured material reads its plain corner until painted.
        uv = obj.data.uv_layers.new(name="UVMap")
        for loop in uv.data:
            loop.uv = (0.02, 0.02)
    return obj


def frame(x_axis, hint):
    """A rotation whose local x follows x_axis and local y leans towards hint."""
    x = Vector(x_axis).normalized()
    y = Vector(hint) - x * Vector(hint).dot(x)
    if y.length < 1e-4:
        y = Vector((0, 0, 1)) - x * x.z
        if y.length < 1e-4:
            y = Vector((0, 1, 0)) - x * x.y
    y.normalize()
    return Matrix((x, y, x.cross(y))).transposed()


def two_bone(root, target, a, b, pole):
    """The middle joint of a two-bone limb (shoulder-elbow-wrist, hip-knee-ankle) reaching for target,
    bending towards pole. Returns (middle, end)."""
    root, target, pole = Vector(root), Vector(target), Vector(pole)
    reach = target - root
    d = min(max(reach.length, 1e-4), (a + b) * 0.999)
    axis = reach.normalized()
    x = (a * a - b * b + d * d) / (2 * d)
    r = math.sqrt(max(a * a - x * x, 0.0))
    side = pole - axis * pole.dot(axis)
    if side.length < 1e-5:
        side = Vector((0, -1, 0)) - axis * axis.y
    side.normalize()
    middle = root + axis * x + side * r
    return middle, root + axis * d


def chain(name, joints, radii, mat, levels=2, lumps=0.0, seed=0, branches=None):
    """Flesh on a chain of joints (or a tree, given branches): each joint has a radius (side, front)."""
    mesh = bpy.data.meshes.new(name)
    bones = branches or [(i, i + 1) for i in range(len(joints) - 1)]
    mesh.from_pydata([tuple(j) for j in joints], bones, [])
    obj = link(bpy.data.objects.new(name, mesh))
    obj.modifiers.new("Skin", 'SKIN')
    for i, r in enumerate(radii):
        mesh.skin_vertices[0].data[i].radius = r if isinstance(r, (tuple, list)) else (r, r)
    mesh.skin_vertices[0].data[0].use_root = True
    sub = obj.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = sub.render_levels = levels
    apply_all(obj)
    roughen(obj, lumps, seed)
    return finish(obj, mat)


def roughen(obj, lumps, seed, scale=9.0):
    """A little irregularity, so cloth and flesh do not read as perfect tubes."""
    if lumps <= 0:
        return
    rng = random.Random(seed)
    offset = Vector((rng.uniform(0, 50), rng.uniform(0, 50), rng.uniform(0, 50)))
    obj.data.update()
    for v in obj.data.vertices:
        v.co += v.normal * noise.noise(v.co * scale + offset) * lumps


def displace(obj, fn):
    """Moves each vertex along its normal by fn(position, normal)."""
    obj.data.update()
    moves = [fn(v.co.copy(), v.normal.copy()) for v in obj.data.vertices]
    for v, m in zip(obj.data.vertices, moves):
        v.co += v.normal * m


def blobs(name, items, mat, resolution=0.008):
    """A metaball cluster: items are (centre, radius[, size[, rotation]]); a negative radius carves."""
    mb = bpy.data.metaballs.new("MB" + name)
    mb.resolution = mb.render_resolution = resolution
    mb.threshold = 0.6
    for item in items:
        at, r = item[0], item[1]
        size = item[2] if len(item) > 2 else None
        rot = item[3] if len(item) > 3 else None
        e = mb.elements.new(type='ELLIPSOID' if size else 'BALL')
        e.co = at
        e.radius = abs(r) * BALL
        e.stiffness = 2.0
        e.use_negative = r < 0
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


class Surface:
    """The real surface of one or more objects, for laying things on it (a lace on a boot, a strap on a coat):
    ray casts and nearest points, with the surface's normal turned outwards."""

    def __init__(self, *objs):
        from mathutils.bvhtree import BVHTree
        depsgraph = bpy.context.evaluated_depsgraph_get()
        self.trees = [BVHTree.FromObject(o, depsgraph) for o in objs]

    def cast(self, origin, direction, reach=0.6):
        """The first surface met from origin along direction: (point, normal facing the origin), or (None, None)."""
        best = None
        for t in self.trees:
            hit, nm, _, dist = t.ray_cast(origin, direction, reach)
            if hit is not None and (best is None or dist < best[2]):
                best = (hit, nm if nm.dot(direction) < 0 else -nm, dist)
        return (best[0], best[1].normalized()) if best else (None, None)

    def nearest(self, point, reach=0.3):
        """The nearest surface point and its outward normal, or (None, None)."""
        best = None
        for t in self.trees:
            hit, nm, _, dist = t.find_nearest(point, reach)
            if hit is not None and (best is None or dist < best[2]):
                best = (hit, nm, dist)
        return (best[0], best[1].normalized()) if best else (None, None)

    def lay(self, point, lift=0.0):
        """A point laid on the surface, lifted along its normal: (point, normal)."""
        hit, nm = self.nearest(point)
        if hit is None:
            return Vector(point), Vector((0, 0, 1))
        return hit + nm * lift, nm


def keep_largest(obj):
    """Removes every connected piece of a mesh but the largest (hidden bubbles, stray crumbs)."""
    bm = bmesh.new()
    bm.from_mesh(obj.data)
    seen, islands = set(), []
    for v in bm.verts:
        if v in seen:
            continue
        island, stack = [], [v]
        seen.add(v)
        while stack:
            u = stack.pop()
            island.append(u)
            for e in u.link_edges:
                w = e.other_vert(u)
                if w not in seen:
                    seen.add(w)
                    stack.append(w)
        islands.append(island)
    islands.sort(key=len, reverse=True)
    extra = [v for island in islands[1:] for v in island]
    if extra:
        bmesh.ops.delete(bm, geom=extra, context='VERTS')
        bm.to_mesh(obj.data)
    bm.free()
    return obj


def exact(obj, fine=False):
    """Marks a part that must keep its exact shape when the being is made lighter (thin laces, rings, wire).
    fine: so small that it is dropped from the middle level of detail on."""
    obj["wg_exact"] = 1
    if fine:
        obj["wg_fine"] = 1
    return obj


def join(name, parts):
    with bpy.context.temp_override(active_object=parts[0], selected_editable_objects=parts, selected_objects=parts):
        bpy.ops.object.join()
    obj = parts[0]
    obj.name = obj.data.name = name
    return obj


def fuse(name, parts, mat, voxel, smooth=5, lumps=0.0, seed=0, keep=None):
    """Several simple pieces fused into one closed surface (a voxel remesh, then smoothed).
    keep: an optional face budget; the surface is then simplified to about that many faces."""
    obj = join(name, parts)
    voxels = obj.modifiers.new("Closed", 'REMESH')
    voxels.mode, voxels.voxel_size = 'VOXEL', voxel
    if smooth:
        soft = obj.modifiers.new("Soft", 'SMOOTH')
        soft.factor, soft.iterations = 0.5, smooth
    apply_all(obj)
    roughen(obj, lumps, seed)
    simplify(obj, keep)
    clear_uvs(obj)
    return finish(obj, mat)


def simplify(obj, keep):
    if keep and len(obj.data.polygons) > keep:
        dec = obj.modifiers.new("Simpler", 'DECIMATE')
        dec.ratio = keep / len(obj.data.polygons)
        apply_all(obj)


def ellipsoid(name, at, size, mat, rotation=(0, 0, 0), segments=24):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=segments // 2, radius=1, location=at)
    obj = bpy.context.active_object
    obj.name = obj.data.name = name
    clear_uvs(obj)
    obj.scale = size
    obj.rotation_euler = rotation if not isinstance(rotation, Matrix) else rotation.to_euler()
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return finish(obj, mat)


def slab(name, at, size, mat, rotation=(0, 0, 0), soft=0.9):
    """A box with softened corners: a patch, a pocket, a buckle."""
    bpy.ops.mesh.primitive_cube_add(size=2, location=at)
    obj = bpy.context.active_object
    obj.name = obj.data.name = name
    clear_uvs(obj)
    obj.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bev = obj.modifiers.new("Soft", 'BEVEL')
    bev.width, bev.segments = min(size) * soft, 3
    bev.limit_method = 'NONE'
    apply_all(obj)
    obj.rotation_euler = rotation if not isinstance(rotation, Matrix) else rotation.to_euler()
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=False)
    return finish(obj, mat)


def cylinder(name, at, radius, depth, mat, rotation=(0, 0, 0), vertices=24, bevel=0.2):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=at)
    obj = bpy.context.active_object
    obj.name = obj.data.name = name
    clear_uvs(obj)
    if bevel:
        bev = obj.modifiers.new("Soft", 'BEVEL')
        bev.width, bev.segments = min(radius, depth) * bevel, 2
        apply_all(obj)
    obj.rotation_euler = rotation if not isinstance(rotation, Matrix) else rotation.to_euler()
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=False)
    return finish(obj, mat)


def torus(name, at, major, minor, mat, rotation=(0, 0, 0), segments=24, around=8):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=segments, minor_segments=around, location=at)
    obj = bpy.context.active_object
    obj.name = obj.data.name = name
    clear_uvs(obj)
    obj.rotation_euler = rotation if not isinstance(rotation, Matrix) else rotation.to_euler()
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=False)
    return finish(obj, mat)


def tube(name, points, widths, thicks, mat, normals=None, sides=10, levels=1, cap=True):
    """A tube along points whose cross-section is an ellipse: half-width along the side, half-thickness
    along the normal. A zero size at either end closes it to a point (a lock of hair, a tapered handle).
    normals: per point, the direction of the thin axis (for hair, away from the scalp)."""
    pts = [Vector(p) for p in points]
    n = len(pts)
    tangents = [(pts[min(i + 1, n - 1)] - pts[max(i - 1, 0)]).normalized() for i in range(n)]
    frames = []
    if normals is not None:
        for t, nm in zip(tangents, normals):
            nv = Vector(nm) - t * Vector(nm).dot(t)
            if nv.length < 1e-5:
                nv = t.orthogonal()
            nv.normalize()
            frames.append((nv, t.cross(nv)))
    else:
        nv = tangents[0].orthogonal().normalized()
        for t in tangents:
            nv = (nv - t * nv.dot(t))
            nv = nv.normalized() if nv.length > 1e-6 else t.orthogonal().normalized()
            frames.append((nv, t.cross(nv)))
    bm = bmesh.new()
    rings = []
    for p, (nv, bv), w, h in zip(pts, frames, widths, thicks):
        if w <= 1e-5 or h <= 1e-5:
            rings.append([bm.verts.new(p)])
            continue
        rings.append([bm.verts.new(p + bv * math.cos(a) * w + nv * math.sin(a) * h)
                      for a in (k / sides * math.tau for k in range(sides))])
    for r0, r1 in zip(rings, rings[1:]):
        if len(r0) == 1 and len(r1) == 1:
            continue
        if len(r0) == 1:
            for k in range(sides):
                bm.faces.new((r0[0], r1[k], r1[(k + 1) % sides]))
        elif len(r1) == 1:
            for k in range(sides):
                bm.faces.new((r0[k], r1[0], r0[(k + 1) % sides]))
        else:
            for k in range(sides):
                bm.faces.new((r0[k], r1[k], r1[(k + 1) % sides], r0[(k + 1) % sides]))
    if cap:
        for ring in (rings[0], rings[-1]):
            if len(ring) > 2:
                bm.faces.new(ring if ring is rings[-1] else list(reversed(ring)))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = link(bpy.data.objects.new(name, mesh))
    if levels:
        sub = obj.modifiers.new("Smooth", 'SUBSURF')
        sub.levels = sub.render_levels = levels
        apply_all(obj)
    return finish(obj, mat)


def lathe(name, profile, mat, at=(0, 0, 0), rotation=None, segments=24, wobble=0.0, seed=0):
    """A solid turned from a profile of (radius, height) pairs, bottom to top (a mug, a lamp)."""
    rng = random.Random(seed)
    bm = bmesh.new()
    rings = []
    for r, z in profile:
        if r <= 1e-5:
            rings.append([bm.verts.new((0, 0, z))])
            continue
        rings.append([bm.verts.new((math.cos(a) * r * (1 + rng.uniform(-wobble, wobble)), math.sin(a) * r * (1 + rng.uniform(-wobble, wobble)), z))
                      for a in (k / segments * math.tau for k in range(segments))])
    for r0, r1 in zip(rings, rings[1:]):
        if len(r0) == 1:
            for k in range(segments):
                bm.faces.new((r0[0], r1[(k + 1) % segments], r1[k]))
        elif len(r1) == 1:
            for k in range(segments):
                bm.faces.new((r0[k], r0[(k + 1) % segments], r1[0]))
        else:
            for k in range(segments):
                bm.faces.new((r0[k], r0[(k + 1) % segments], r1[(k + 1) % segments], r1[k]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = link(bpy.data.objects.new(name, mesh))
    m = Matrix.Translation(Vector(at)) @ (rotation.to_4x4() if rotation is not None else Matrix.Identity(4))
    obj.data.transform(m)
    return finish(obj, mat)


def panel(name, centre, top, bottom, span, radius, mat, thickness=0.006, columns=16, rows=10, folds=0.0, fold_count=5, seed=0):
    """A cloth panel wrapped around a vertical axis at centre (x, y): an apron, a shirt front, a lapel.
    span(v) is the angle it covers either side of the front, from v = 0 at the top to 1 at the bottom;
    radius(z) gives the (side, front) distance from the axis at a height; folds ripple it towards the bottom."""
    rng = random.Random(seed)
    span_at = span if callable(span) else (lambda v: span)
    bm = bmesh.new()
    grid = []
    for j in range(rows + 1):
        v = j / rows
        z = top + (bottom - top) * v
        rx, ry = radius(z)
        row = []
        for i in range(columns + 1):
            u = i / columns * 2 - 1
            ang = -math.pi / 2 + u * span_at(v)
            ripple = 1 + folds * v * math.sin((u + 1) * fold_count * math.pi + seed) + rng.uniform(-0.003, 0.003)
            row.append(bm.verts.new((centre[0] + math.cos(ang) * rx * ripple, centre[1] + math.sin(ang) * ry * ripple, z)))
        grid.append(row)
    for j in range(rows):
        for i in range(columns):
            bm.faces.new((grid[j][i], grid[j + 1][i], grid[j + 1][i + 1], grid[j][i + 1]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = link(bpy.data.objects.new(name, mesh))
    obj.modifiers.new("Thickness", 'SOLIDIFY').thickness = thickness
    sub = obj.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = sub.render_levels = 1
    apply_all(obj)
    return finish(obj, mat)


def sheet(name, grid, mat, thickness=0.004, levels=1):
    """A sheet of leather or cloth through a grid of points (rows of equal length), given a thickness and softened:
    a bag's flap, a strap's folded end."""
    bm = bmesh.new()
    rows = [[bm.verts.new(Vector(q)) for q in row] for row in grid]
    for r0, r1 in zip(rows, rows[1:]):
        for k in range(len(r0) - 1):
            bm.faces.new((r0[k], r1[k], r1[k + 1], r0[k + 1]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    obj = link(bpy.data.objects.new(name, mesh))
    solid = obj.modifiers.new("Thickness", 'SOLIDIFY')
    solid.thickness, solid.offset = thickness, 0.0
    if levels:
        sub = obj.modifiers.new("Smooth", 'SUBSURF')
        sub.levels = sub.render_levels = levels
    apply_all(obj)
    return finish(obj, mat)


def ring(name, centre, across, up, half_width, half_height, wire, mat, steps=16, sides=6):
    """A closed oval ring of wire (a bag's ring, a buckle's frame) in the plane of across and up."""
    across, up = Vector(across).normalized(), Vector(up).normalized()
    pts = [Vector(centre) + across * (math.cos(k / steps * math.tau) * half_width) + up * (math.sin(k / steps * math.tau) * half_height)
           for k in range(steps + 1)]
    return exact(tube(name, pts, [wire] * len(pts), [wire] * len(pts), mat, sides=sides, levels=0, cap=False))


def spline(points, per=6):
    """A smooth curve through points (Catmull-Rom), with per samples between each pair."""
    pts = [Vector(p) for p in points]
    ext = [pts[0] * 2 - pts[1]] + pts + [pts[-1] * 2 - pts[-2]]
    out = []
    for i in range(1, len(ext) - 2):
        p0, p1, p2, p3 = ext[i - 1], ext[i], ext[i + 1], ext[i + 2]
        for k in range(per):
            t = k / per
            out.append(0.5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t ** 3))
    out.append(pts[-1])
    return out
