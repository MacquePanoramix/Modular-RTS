"""Wonder Gather — S1 style studies.

Builds the same small vignette (a worker holding a pickaxe beside a mineral-bearing
boulder, with a sack, grass and pebbles) in three candidate art directions and
renders each one. Everything is generated from this script so the studies can be
reproduced or adjusted.

Run (Blender 4.4+):
    blender -b --factory-startup --python style_studies.py -- --out <folder> [--styles soft,lowpoly,grounded] [--samples 96]
"""
import math
import random
import sys
import argparse

import bpy
import bmesh
from mathutils import Vector, Matrix, Euler


# ---------------------------------------------------------------------------
# Style definitions: proportions, palette and finish.
# ---------------------------------------------------------------------------
STYLES = {
    "soft": dict(
        label="Soft stylized",
        height=1.62, head=0.145, limb=1.25, hand=1.35, faceted=False, detail=1,
        palette=dict(tunic=(0.29, 0.47, 0.44), trousers=(0.30, 0.21, 0.15), boots=(0.20, 0.14, 0.10),
                     skin=(0.86, 0.66, 0.52), hair=(0.33, 0.20, 0.12), belt=(0.30, 0.19, 0.12),
                     wood=(0.62, 0.42, 0.25), metal=(0.50, 0.55, 0.60), rock=(0.30, 0.31, 0.34),
                     ore=(0.85, 0.50, 0.22), grass=(0.45, 0.62, 0.30), dirt=(0.52, 0.42, 0.30),
                     sack=(0.70, 0.58, 0.40)),
        roughness=0.75, sun=(1.0, 0.86, 0.68), sun_strength=3.4, sky_strength=0.16, dof=False),
    "lowpoly": dict(
        label="Faceted low-poly",
        height=1.62, head=0.14, limb=1.2, hand=1.3, faceted=True, detail=0,
        palette=dict(tunic=(0.18, 0.45, 0.52), trousers=(0.36, 0.24, 0.14), boots=(0.22, 0.15, 0.10),
                     skin=(0.93, 0.70, 0.52), hair=(0.40, 0.22, 0.10), belt=(0.35, 0.20, 0.10),
                     wood=(0.70, 0.45, 0.24), metal=(0.55, 0.60, 0.66), rock=(0.30, 0.32, 0.38),
                     ore=(0.95, 0.55, 0.18), grass=(0.42, 0.68, 0.28), dirt=(0.62, 0.48, 0.30),
                     sack=(0.78, 0.62, 0.38)),
        roughness=0.85, sun=(1.0, 0.88, 0.70), sun_strength=3.8, sky_strength=0.17, dof=False),
    "grounded": dict(
        label="Grounded semi-realistic",
        height=1.76, head=0.112, limb=1.0, hand=1.0, faceted=False, detail=2,
        palette=dict(tunic=(0.40, 0.37, 0.28), trousers=(0.24, 0.22, 0.19), boots=(0.13, 0.09, 0.06),
                     skin=(0.72, 0.52, 0.40), hair=(0.18, 0.12, 0.08), belt=(0.22, 0.14, 0.08),
                     wood=(0.42, 0.28, 0.16), metal=(0.36, 0.38, 0.40), rock=(0.24, 0.23, 0.22),
                     ore=(0.55, 0.30, 0.18), grass=(0.30, 0.42, 0.18), dirt=(0.36, 0.29, 0.21),
                     sack=(0.52, 0.44, 0.31)),
        roughness=0.8, sun=(1.0, 0.90, 0.78), sun_strength=3.2, sky_strength=0.18, dof=True),
}


# ---------------------------------------------------------------------------
# Scene helpers.
# ---------------------------------------------------------------------------
def reset_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = 'METRIC'
    return scene


def link(obj):
    bpy.context.scene.collection.objects.link(obj)
    return obj


def activate(obj):
    for other in bpy.context.view_layer.objects:
        other.select_set(False)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)


def material(name, color, roughness=0.7, metallic=0.0, bump=0.0, bump_scale=40.0, variation=0.0, emission=0.0):
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    nodes, links = mat.node_tree.nodes, mat.node_tree.links
    bsdf = nodes["Principled BSDF"]
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    base = (*color, 1.0)
    if variation > 0:
        noise = nodes.new("ShaderNodeTexNoise")
        noise.inputs["Scale"].default_value = 6.0
        noise.inputs["Detail"].default_value = 6.0
        ramp = nodes.new("ShaderNodeValToRGB")
        dark = tuple(max(0.0, c * (1 - variation)) for c in color)
        light = tuple(min(1.0, c * (1 + variation)) for c in color)
        ramp.color_ramp.elements[0].color = (*dark, 1)
        ramp.color_ramp.elements[1].color = (*light, 1)
        links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
        links.new(ramp.outputs["Color"], bsdf.inputs["Base Color"])
    else:
        bsdf.inputs["Base Color"].default_value = base
    if bump > 0:
        tex = nodes.new("ShaderNodeTexNoise")
        tex.inputs["Scale"].default_value = bump_scale
        tex.inputs["Detail"].default_value = 8.0
        bump_node = nodes.new("ShaderNodeBump")
        bump_node.inputs["Strength"].default_value = bump
        links.new(tex.outputs["Fac"], bump_node.inputs["Height"])
        links.new(bump_node.outputs["Normal"], bsdf.inputs["Normal"])
    if emission > 0:
        bsdf.inputs["Emission Color"].default_value = base
        bsdf.inputs["Emission Strength"].default_value = emission
    return mat


def finish(obj, style, smooth=True):
    """Apply the style's surface treatment: smooth subdivision, or flat facets."""
    if style["faceted"]:
        dec = obj.modifiers.new("Facets", 'DECIMATE')
        dec.ratio = 0.35
        for poly in obj.data.polygons:
            poly.use_smooth = False
    elif smooth:
        for poly in obj.data.polygons:
            poly.use_smooth = True


def skin_object(name, joints, edges, radii, mat, style, root=0, subdiv=2):
    """A smooth organic limb/body shape from a joint skeleton (Skin + Subdivision)."""
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata([Vector(j) for j in joints], edges, [])
    obj = link(bpy.data.objects.new(name, mesh))
    activate(obj)
    bpy.ops.object.modifier_add(type='SKIN')
    skin = obj.modifiers[-1]
    skin.use_smooth_shade = not style["faceted"]
    skin.branch_smoothing = 0.5
    data = mesh.skin_vertices[0].data
    for i, r in enumerate(radii):
        rx, ry = (r, r) if isinstance(r, (int, float)) else r
        data[i].radius = (rx, ry)
        data[i].use_root = i == root
    sub = obj.modifiers.new("Smooth", 'SUBSURF')
    sub.levels = sub.render_levels = 1 if style["faceted"] else subdiv
    if style["faceted"]:
        dec = obj.modifiers.new("Facets", 'DECIMATE')
        dec.ratio = 0.3
    obj.data.materials.append(mat)
    return obj


def primitive(kind, name, mat, location=(0, 0, 0), scale=(1, 1, 1), rotation=(0, 0, 0), **kwargs):
    getattr(bpy.ops.mesh, f"primitive_{kind}_add")(location=location, rotation=rotation, **kwargs)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    obj.data.materials.append(mat)
    return obj


def look_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat('-Z', 'Y').to_euler()


def surface_point(obj, origin, direction):
    """Ray-cast against the evaluated (modified) mesh, returning world point and normal."""
    depsgraph = bpy.context.evaluated_depsgraph_get()
    evaluated = obj.evaluated_get(depsgraph)
    inverse = evaluated.matrix_world.inverted()
    local_origin = inverse @ Vector(origin)
    local_direction = (inverse.to_3x3() @ Vector(direction)).normalized()
    hit, location, normal, _ = evaluated.ray_cast(local_origin, local_direction)
    if not hit:
        return None, None
    world_normal = (evaluated.matrix_world.to_3x3() @ normal).normalized()
    return evaluated.matrix_world @ location, world_normal


# ---------------------------------------------------------------------------
# Vignette parts.
# ---------------------------------------------------------------------------
def build_ground(style, mats, rng):
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=160, y_subdivisions=160, size=16)
    ground = bpy.context.active_object
    ground.name = "Ground"
    tex = bpy.data.textures.new("GroundNoise", 'CLOUDS')
    tex.noise_scale = 1.6
    disp = ground.modifiers.new("Undulation", 'DISPLACE')
    disp.texture = tex
    disp.strength = 0.18
    disp.mid_level = 0.5
    ground.data.materials.append(mats["dirt"])
    finish(ground, style)
    if style["faceted"]:
        ground.modifiers["Facets"].ratio = 0.08
    return ground


def build_backdrop(style, mats):
    bpy.ops.mesh.primitive_grid_add(x_subdivisions=120, y_subdivisions=120, size=220, location=(0, 60, -0.05))
    land = bpy.context.active_object
    land.name = "Distant land"
    tex = bpy.data.textures.new("Hills", 'CLOUDS')
    tex.noise_scale = 18.0
    disp = land.modifiers.new("Hills", 'DISPLACE')
    disp.texture = tex
    disp.strength = 9.0
    disp.mid_level = 0.62
    land.data.materials.append(mats["far_grass"])
    finish(land, style)
    if style["faceted"]:
        land.modifiers["Facets"].ratio = 0.05
    return land


def build_grass(style, ground, mats, rng):
    if style["faceted"]:
        # Low-poly tufts: small three-blade clusters scattered around the scene.
        for _ in range(140):
            x, y = rng.uniform(-6, 6), rng.uniform(-3, 7)
            if abs(x - 1.4) < 1.3 and abs(y - 1.0) < 1.1:
                continue
            point, normal = surface_point(ground, (x, y, 3), (0, 0, -1))
            if point is None:
                continue
            for k in range(3):
                blade = primitive("cone", "Tuft", mats["grass"], location=point, vertices=3,
                                  radius1=0.05, radius2=0.0, depth=rng.uniform(0.18, 0.32))
                blade.rotation_euler = (rng.uniform(-0.35, 0.35), rng.uniform(-0.35, 0.35), k * 2.1)
                blade.location.z += blade.dimensions.z * 0.4
        return
    ground.modifiers.new("Grass", 'PARTICLE_SYSTEM')
    settings = ground.particle_systems[-1].settings
    settings.type = 'HAIR'
    settings.count = 9000 if style["detail"] >= 2 else 6000
    settings.hair_length = 0.16 if style["detail"] >= 2 else 0.13
    settings.use_advanced_hair = True
    settings.child_type = 'INTERPOLATED'
    settings.child_percent = 12
    settings.rendered_child_count = 12
    settings.root_radius = 1.0
    settings.tip_radius = 0.0
    settings.radius_scale = 0.012
    settings.roughness_2 = 0.25
    settings.roughness_endpoint = 0.06
    settings.brownian_factor = 0.02
    settings.length_random = 0.6
    ground.data.materials.append(mats["grass_blades"])
    settings.material_slot = mats["grass_blades"].name
    # Keep the work area around the boulder and worker trampled.
    group = ground.vertex_groups.new(name="GrassDensity")
    for v in ground.data.vertices:
        world = ground.matrix_world @ v.co
        near_rock = (world.x - 1.4) ** 2 / 2.2 + (world.y - 1.0) ** 2 / 1.6
        near_worker = world.x ** 2 / 0.6 + world.y ** 2 / 0.6
        weight = min(1.0, max(0.0, min(near_rock, near_worker) - 0.6))
        group.add([v.index], weight, 'REPLACE')
    ground.particle_systems[-1].vertex_group_density = "GrassDensity"


def build_boulder(style, mats, rng):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1 if style["faceted"] else 5, radius=1.0, location=(1.45, 1.0, 0.42))
    rock = bpy.context.active_object
    rock.name = "Ore boulder"
    rock.scale = (1.05, 0.85, 0.72)
    rock.rotation_euler = (0.1, -0.05, 0.4)
    for kind, scale, strength in (("VORONOI", 0.9, 0.28), ("CLOUDS", 0.35, 0.10)):
        tex = bpy.data.textures.new(f"Rock{kind}", kind)
        tex.noise_scale = scale
        disp = rock.modifiers.new(f"Shape{kind}", 'DISPLACE')
        disp.texture = tex
        disp.strength = strength if not style["faceted"] else strength * 0.7
    rock.data.materials.append(mats["rock"])
    if style["faceted"]:
        for poly in rock.data.polygons:
            poly.use_smooth = False
        sub = rock.modifiers.new("Chunky", 'SUBSURF')
        sub.levels = sub.render_levels = 1
        sub.subdivision_type = 'SIMPLE'
        rock.modifiers.move(rock.modifiers.find("Chunky"), 0)
    else:
        for poly in rock.data.polygons:
            poly.use_smooth = True
    # Mineral: crystals and veins breaking through the surface facing the worker.
    count = 9 if style["faceted"] else 16
    for i in range(count):
        angle = rng.uniform(math.radians(150), math.radians(300))
        height = rng.uniform(0.05, 0.75)
        direction = Vector((math.cos(angle), math.sin(angle), height - 0.3)).normalized()
        origin = Vector(rock.location) + direction * 3.0
        point, normal = surface_point(rock, origin, -direction)
        if point is None:
            continue
        length = rng.uniform(0.08, 0.17) * (1.3 if style["faceted"] else 1.0)
        crystal = primitive("cone", "Ore crystal", mats["ore"], location=point, vertices=4 if style["faceted"] else 6,
                            radius1=length * 0.32, radius2=0.0, depth=length)
        tilt = normal.lerp(Vector((rng.uniform(-1, 1), rng.uniform(-1, 1), rng.uniform(0, 1))).normalized(), 0.3).normalized()
        crystal.rotation_euler = tilt.to_track_quat('Z', 'Y').to_euler()
        crystal.location = point - normal * length * 0.12
        if not style["faceted"]:
            for poly in crystal.data.polygons:
                poly.use_smooth = False
    return rock


def build_pebbles(style, ground, mats, rng):
    for _ in range(36):
        x, y = rng.uniform(-2.5, 4.5), rng.uniform(-1.5, 3.5)
        point, _ = surface_point(ground, (x, y, 3), (0, 0, -1))
        if point is None:
            continue
        size = rng.uniform(0.03, 0.09)
        pebble = primitive("ico_sphere", "Pebble", mats["stone"], location=point, subdivisions=1 if style["faceted"] else 3,
                           radius=size)
        pebble.scale = (rng.uniform(0.8, 1.4), rng.uniform(0.8, 1.2), rng.uniform(0.5, 0.8))
        pebble.rotation_euler = (rng.uniform(0, 3), rng.uniform(0, 3), rng.uniform(0, 3))
        finish(pebble, style)
    # A few chunks of ore already broken off near the boulder's foot.
    for _ in range(5):
        x, y = rng.uniform(0.5, 1.0), rng.uniform(0.2, 0.7)
        point, _ = surface_point(ground, (x, y, 3), (0, 0, -1))
        if point is None:
            continue
        chunk = primitive("ico_sphere", "Ore chunk", mats["stone"], location=point + Vector((0, 0, 0.03)),
                          subdivisions=1, radius=rng.uniform(0.05, 0.08))
        chunk.data.materials.append(mats["ore"])
        for poly in chunk.data.polygons:
            poly.material_index = 1 if rng.random() < 0.35 else 0
            poly.use_smooth = False


def build_sack(style, ground, mats):
    point, _ = surface_point(ground, (-0.75, 0.55, 3), (0, 0, -1))
    bpy.ops.mesh.primitive_uv_sphere_add(segments=8 if style["faceted"] else 32, ring_count=6 if style["faceted"] else 16,
                                         radius=0.24, location=point + Vector((0, 0, 0.2)))
    sack = bpy.context.active_object
    sack.name = "Sack"
    sack.scale = (1.05, 0.9, 1.1)
    taper = sack.modifiers.new("Gather", 'SIMPLE_DEFORM')
    taper.deform_method = 'TAPER'
    taper.factor = -0.75
    slump = sack.modifiers.new("Slump", 'SIMPLE_DEFORM')
    slump.deform_method = 'BEND'
    slump.angle = math.radians(18)
    slump.deform_axis = 'X'
    folds_tex = bpy.data.textures.new("SackFolds", 'STUCCI')
    folds_tex.noise_scale = 0.18
    folds = sack.modifiers.new("Folds", 'DISPLACE')
    folds.texture = folds_tex
    folds.strength = 0.06
    sack.data.materials.append(mats["sack"])
    finish(sack, style)
    neck = primitive("torus", "Sack tie", mats["belt"], location=point + Vector((0, 0, 0.46)),
                     major_radius=0.055, minor_radius=0.014, major_segments=10 if style["faceted"] else 32,
                     minor_segments=4 if style["faceted"] else 10)
    knot = primitive("cone", "Sack mouth", mats["sack"], location=point + Vector((0, 0, 0.53)), vertices=7 if style["faceted"] else 18,
                     radius1=0.035, radius2=0.075, depth=0.12)
    frill_tex = bpy.data.textures.new("SackMouth", 'STUCCI')
    frill_tex.noise_scale = 0.05
    frill = knot.modifiers.new("Gathered", 'DISPLACE')
    frill.texture = frill_tex
    frill.strength = 0.02
    finish(knot, style)
    return sack


CAMERA_LOCATION = Vector((0.15, -4.4, 1.55))


def build_pickaxe(style, mats, low_grip, high_grip):
    """A pickaxe held across the body: the handle runs through both grips and the head sits above the upper hand."""
    low_grip, high_grip = Vector(low_grip), Vector(high_grip)
    up = (high_grip - low_grip).normalized()
    head_point = high_grip + up * 0.36
    butt = low_grip - up * 0.12
    axis = (butt - head_point).normalized()
    length = (butt - head_point).length
    center = (butt + head_point) * 0.5
    handle = primitive("cylinder", "Pickaxe handle", mats["wood"], location=center,
                       vertices=8 if style["faceted"] else 32, radius=0.022, depth=length)
    handle.rotation_euler = axis.to_track_quat('Z', 'Y').to_euler()
    taper = handle.modifiers.new("Taper", 'SIMPLE_DEFORM')
    taper.deform_method = 'TAPER'
    taper.factor = 0.25
    finish(handle, style)
    # Curved double-pointed iron head, perpendicular to the handle.
    view = (head_point - CAMERA_LOCATION).normalized()
    side = axis.cross(view).normalized()
    head_center = head_point
    for sign in (1, -1):
        # Each point is two tapering segments so the head curves back towards the handle.
        inner = (side * sign + axis * -0.05).normalized()
        outer = (side * sign + axis * 0.35).normalized()
        base = primitive("cone", "Pick blade", mats["metal"], vertices=4 if style["faceted"] else 16,
                         radius1=0.04, radius2=0.026, depth=0.17)
        base.rotation_euler = inner.to_track_quat('Z', 'Y').to_euler()
        base.location = head_center + inner * 0.085
        finish(base, style)
        tip = primitive("cone", "Pick point", mats["metal"], vertices=4 if style["faceted"] else 16,
                        radius1=0.026, radius2=0.003, depth=0.18)
        tip.rotation_euler = outer.to_track_quat('Z', 'Y').to_euler()
        tip.location = head_center + inner * 0.165 + outer * 0.085
        finish(tip, style)
    collar = primitive("cube", "Pick collar", mats["metal"], location=head_center, size=0.075)
    collar.rotation_euler = axis.to_track_quat('Z', 'Y').to_euler()
    bevel = collar.modifiers.new("Soft", 'BEVEL')
    bevel.width = 0.008
    bevel.segments = 1 if style["faceted"] else 3


def build_worker(style, mats):
    """A worker built from joint skeletons, facing +Y, then turned towards the camera."""
    h = style["height"] / 1.75
    limb = style["limb"]
    hand = style["hand"]

    def p(x, y, z):
        return (x, y, z * h)

    pelvis, spine, chest, neck = p(0, 0, 0.95), p(0, 0.01, 1.13), p(0, 0.02, 1.33), p(0, 0.025, 1.50)
    shoulder_l, shoulder_r = p(-0.19, 0.0, 1.43), p(0.19, 0.0, 1.43)
    elbow_l, elbow_r = p(-0.27, 0.10, 1.18), p(0.26, 0.07, 1.14)
    wrist_l, wrist_r = p(-0.16, 0.25, 1.26), p(0.22, 0.21, 0.98)
    hand_l, hand_r = p(-0.12, 0.28, 1.30), p(0.19, 0.26, 0.95)
    hip_l, hip_r = p(-0.10, 0.0, 0.92), p(0.10, 0.0, 0.92)
    knee_l, knee_r = p(-0.11, 0.01, 0.50), p(0.13, 0.06, 0.51)
    ankle_l, ankle_r = p(-0.12, -0.01, 0.085), p(0.15, 0.06, 0.085)
    toe_l, toe_r = p(-0.13, 0.15, 0.04), p(0.18, 0.20, 0.04)
    heel_l, heel_r = p(-0.12, -0.06, 0.05), p(0.15, 0.01, 0.05)

    pieces = []
    # Tunic over torso with sleeves to the elbow and a hem that flares over the hips.
    hem = p(0, 0.0, 0.72)
    tunic = skin_object("Tunic", [pelvis, spine, chest, neck, shoulder_l, elbow_l, shoulder_r, elbow_r, hem],
                        [(0, 1), (1, 2), (2, 3), (2, 4), (4, 5), (2, 6), (6, 7), (0, 8)],
                        [(0.17, 0.12), (0.15, 0.11), (0.18, 0.12), 0.07, 0.075 * limb, 0.06 * limb, 0.075 * limb, 0.06 * limb, (0.20, 0.15)],
                        mats["tunic"], style)
    pieces.append(tunic)
    for side, (elbow, wrist, tip) in (("L", (elbow_l, wrist_l, hand_l)), ("R", (elbow_r, wrist_r, hand_r))):
        pieces.append(skin_object(f"Forearm {side}", [elbow, wrist], [(0, 1)],
                                  [0.05 * limb, 0.036 * limb], mats["skin"], style))
        pieces.append(skin_object(f"Glove {side}", [wrist, tip], [(0, 1)],
                                  [0.046 * limb, (0.06 * hand, 0.045 * hand)], mats["glove"], style))
    for side, (hip, knee, ankle) in (("L", (hip_l, knee_l, ankle_l)), ("R", (hip_r, knee_r, ankle_r))):
        pieces.append(skin_object(f"Leg {side}", [hip, knee, ankle], [(0, 1), (1, 2)],
                                  [0.085 * limb, 0.062 * limb, 0.048 * limb], mats["trousers"], style))
    for side, (ankle, heel, toe) in (("L", (ankle_l, heel_l, toe_l)), ("R", (ankle_r, heel_r, toe_r))):
        boot_top = (ankle[0], ankle[1], ankle[2] + 0.12 * h)
        pieces.append(skin_object(f"Boot {side}", [ankle, boot_top, heel, toe], [(0, 1), (0, 2), (0, 3)],
                                  [0.058 * limb, 0.056 * limb, 0.045 * limb, 0.048 * limb], mats["boots"], style))
    pieces.append(skin_object("Neck", [neck, p(0, 0.03, 1.57)], [(0, 1)], [0.052 * limb, 0.05 * limb], mats["skin"], style))
    # Head, face and hair.
    head_center = Vector(p(0, 0.035, 1.645)) + Vector((0, 0, style["head"] - 0.105))
    head = primitive("uv_sphere", "Head", mats["skin"], location=head_center, segments=12 if style["faceted"] else 48,
                     ring_count=8 if style["faceted"] else 24, radius=style["head"])
    head.scale = (0.92, 1.0, 1.1)
    finish(head, style)
    pieces.append(head)
    r = style["head"]
    nose = primitive("uv_sphere", "Nose", mats["skin"], location=head_center + Vector((0, 0.98 * r, -0.1 * r)),
                     segments=8 if style["faceted"] else 24, ring_count=6 if style["faceted"] else 12, radius=0.18 * r)
    nose.scale = (0.8, 1.0, 1.2)
    finish(nose, style)
    pieces.append(nose)
    for sign in (-1, 1):
        eye = primitive("uv_sphere", "Eye", mats["eye"], location=head_center + Vector((sign * 0.36 * r, 0.86 * r, 0.12 * r)),
                        segments=8 if style["faceted"] else 16, ring_count=6 if style["faceted"] else 8,
                        radius=(0.10 if style["height"] < 1.7 else 0.088) * r)
        eye.scale = (0.8, 0.5, 1.0)
        pieces.append(eye)
        ear = primitive("uv_sphere", "Ear", mats["skin"], location=head_center + Vector((sign * 0.9 * r, 0.0, 0.0)),
                        segments=8 if style["faceted"] else 16, ring_count=6 if style["faceted"] else 8, radius=0.22 * r)
        ear.scale = (0.45, 0.8, 1.1)
        finish(ear, style)
        pieces.append(ear)
    hair = primitive("uv_sphere", "Hair", mats["hair"], location=head_center + Vector((0, -0.06 * r, 0.16 * r)),
                     segments=12 if style["faceted"] else 48, ring_count=8 if style["faceted"] else 24, radius=1.07 * r)
    hair.scale = (0.97, 1.03, 1.0)
    hair.rotation_euler = (math.radians(-18), 0, 0)
    # Keep the face clear: a cutter removes the front-lower part of the hair volume.
    cut = hair.modifiers.new("Hairline", 'BOOLEAN')
    cutter = primitive("cube", "Hairline cutter", mats["hair"], location=head_center + Vector((0, 1.05 * r, -0.62 * r)), size=2.0 * r)
    cutter.rotation_euler = (math.radians(-35), 0, 0)
    cutter.hide_render = True
    cutter.display_type = 'WIRE'
    cut.object = cutter
    cut.operation = 'DIFFERENCE'
    cut.solver = 'EXACT'
    hair.modifiers.move(hair.modifiers.find("Hairline"), 0)
    pieces.append(cutter)
    finish(hair, style)
    pieces.append(hair)
    # Belt with a small pouch.
    belt_z = 0.99 * h
    belt = primitive("torus", "Belt", mats["belt"], location=(0, 0.005, belt_z), major_radius=0.165, minor_radius=0.022,
                     major_segments=12 if style["faceted"] else 48, minor_segments=4 if style["faceted"] else 12)
    belt.scale = (1.0, 0.74, 1.0)
    pieces.append(belt)
    pouch = primitive("cube", "Pouch", mats["belt"], location=(-0.15, 0.06, belt_z - 0.06), size=0.09)
    pouch.scale = (0.7, 0.45, 1.0)
    bevel = pouch.modifiers.new("Soft", 'BEVEL')
    bevel.width = 0.012
    bevel.segments = 1 if style["faceted"] else 3
    pieces.append(pouch)

    # A leather work apron over the tunic.
    apron = skin_object("Apron", [p(0, 0.125, 1.28), p(0, 0.165, 1.02), p(0, 0.175, 0.70)], [(0, 1), (1, 2)],
                        [(0.11, 0.012), (0.155, 0.016), (0.18, 0.012)], mats["apron"], style, subdiv=2)
    pieces.append(apron)

    rig = bpy.data.objects.new("Worker", None)
    link(rig)
    for piece in pieces:
        piece.parent = rig
    return rig, Vector(hand_r), Vector(hand_l)


def build_lighting(style, scene):
    world = bpy.data.worlds.new("Sky")
    scene.world = world
    world.use_nodes = True
    nodes, links = world.node_tree.nodes, world.node_tree.links
    sky = nodes.new("ShaderNodeTexSky")
    sky.sky_type = 'NISHITA'
    sky.sun_elevation = math.radians(30)
    sky.sun_rotation = math.radians(35)
    sky.sun_disc = False
    sky.air_density = 1.2
    sky.dust_density = 1.6
    background = nodes["Background"]
    background.inputs["Strength"].default_value = style["sky_strength"]
    links.new(sky.outputs["Color"], background.inputs["Color"])
    sun_data = bpy.data.lights.new("Sun", 'SUN')
    sun_data.energy = style["sun_strength"]
    sun_data.color = style["sun"]
    sun_data.angle = math.radians(3.0 if not style["faceted"] else 1.0)
    sun = link(bpy.data.objects.new("Sun", sun_data))
    sun.rotation_euler = Euler((math.radians(55), 0, math.radians(125)), 'XYZ')
    fill_data = bpy.data.lights.new("Fill", 'AREA')
    fill_data.energy = 110
    fill_data.size = 4
    fill_data.color = (0.75, 0.85, 1.0)
    fill = link(bpy.data.objects.new("Fill", fill_data))
    fill.location = (-3.5, -3.0, 2.8)
    look_at(fill, (0.3, 0.4, 1.0))


SHOTS = {
    # Wide: the whole vignette. Close: the worker as a character, as the creator preview would frame it.
    "wide": dict(location=(0.15, -4.4, 1.55), target=(0.55, 0.55, 0.78), lens=46, focus=4.4, fstop=3.2),
    "close": dict(location=(-0.62, -2.25, 1.66), target=(0.02, 0.02, 1.36), lens=50, focus=2.35, fstop=2.8),
}


def build_camera(style, scene, shot="wide"):
    frame = SHOTS[shot]
    cam_data = bpy.data.cameras.new("Camera")
    cam_data.lens = frame["lens"]
    camera = link(bpy.data.objects.new("Camera", cam_data))
    camera.location = frame["location"]
    look_at(camera, frame["target"])
    scene.camera = camera
    if style["dof"] or shot == "close":
        cam_data.dof.use_dof = True
        cam_data.dof.focus_distance = frame["focus"]
        cam_data.dof.aperture_fstop = frame["fstop"] if style["dof"] else frame["fstop"] * 2
    return camera


def configure_render(scene, samples, width, height):
    scene.render.engine = 'CYCLES'
    cycles = scene.cycles
    cycles.samples = samples
    cycles.use_denoising = True
    cycles.max_bounces = 6
    try:
        preferences = bpy.context.preferences.addons["cycles"].preferences
        for backend in ("OPTIX", "CUDA", "HIP", "ONEAPI"):
            try:
                preferences.compute_device_type = backend
                preferences.get_devices()
                if any(d.type == backend for d in preferences.devices):
                    for device in preferences.devices:
                        device.use = device.type == backend
                    cycles.device = 'GPU'
                    print(f"STYLE_STUDY_DEVICE {backend}")
                    break
            except TypeError:
                continue
    except Exception as error:  # CPU fallback is fine for the studies.
        print("STYLE_STUDY_DEVICE CPU", error)
    scene.render.resolution_x = width
    scene.render.resolution_y = height
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = 'PNG'
    scene.view_settings.view_transform = 'AgX'
    scene.view_settings.look = 'AgX - Medium High Contrast'


def rock_material(style):
    """Stone with streaks of ore running through it, so the boulder reads as mineral-bearing."""
    pal = style["palette"]
    mat = material("Rock", pal["rock"], 0.85, bump=0.4 if style["detail"] >= 1 else 0.0, bump_scale=25,
                   variation=0.18 if style["detail"] else 0.0)
    nodes, links = mat.node_tree.nodes, mat.node_tree.links
    bsdf = nodes["Principled BSDF"]
    stone = bsdf.inputs["Base Color"].links[0].from_socket if bsdf.inputs["Base Color"].is_linked else None
    wave = nodes.new("ShaderNodeTexWave")
    wave.wave_type = 'BANDS'
    wave.inputs["Scale"].default_value = 1.1
    wave.inputs["Distortion"].default_value = 14.0
    wave.inputs["Detail"].default_value = 6.0
    wave.inputs["Detail Roughness"].default_value = 0.7
    ramp = nodes.new("ShaderNodeValToRGB")
    ramp.color_ramp.interpolation = 'CONSTANT' if style["faceted"] else 'EASE'
    ramp.color_ramp.elements[0].position = 0.885
    ramp.color_ramp.elements[0].color = (0, 0, 0, 1)
    ramp.color_ramp.elements[1].position = 0.91
    ramp.color_ramp.elements[1].color = (1, 1, 1, 1)
    links.new(wave.outputs["Fac"], ramp.inputs["Fac"])
    mix = nodes.new("ShaderNodeMix")
    mix.data_type = 'RGBA'
    if stone is not None:
        links.new(stone, mix.inputs[6])
    else:
        mix.inputs[6].default_value = (*pal["rock"], 1)
    mix.inputs[7].default_value = (*pal["ore"], 1)
    links.new(ramp.outputs["Color"], mix.inputs["Factor"])
    links.new(mix.outputs[2], bsdf.inputs["Base Color"])
    links.new(ramp.outputs["Color"], bsdf.inputs["Metallic"])
    return mat


def materials_for(style):
    pal = style["palette"]
    rough = style["roughness"]
    detail = style["detail"]
    cloth_bump = 0.25 if detail >= 2 else 0.0
    return dict(
        tunic=material("Tunic", pal["tunic"], rough, bump=cloth_bump, bump_scale=120, variation=0.08 * detail),
        trousers=material("Trousers", pal["trousers"], rough, bump=cloth_bump, bump_scale=120, variation=0.06 * detail),
        boots=material("Boots", pal["boots"], 0.55),
        skin=material("Skin", pal["skin"], 0.55),
        hair=material("Hair", pal["hair"], 0.6, bump=0.3 if detail >= 1 else 0.0, bump_scale=200),
        belt=material("Leather", pal["belt"], 0.5),
        eye=material("Eye", (0.05, 0.04, 0.035), 0.2),
        wood=material("Wood", pal["wood"], 0.6, bump=0.2 if detail >= 1 else 0.0, bump_scale=30, variation=0.12),
        metal=material("Iron", pal["metal"], 0.38, metallic=0.85),
        rock=rock_material(style),
        stone=material("Stone", pal["rock"], 0.85, bump=0.4 if detail >= 1 else 0.0, bump_scale=25, variation=0.18 if detail else 0.0),
        apron=material("Apron", tuple(c * 0.75 for c in pal["belt"]), 0.6, bump=0.15 if detail >= 1 else 0.0, bump_scale=60),
        glove=material("Gloves", tuple(c * 1.3 for c in pal["belt"]), 0.65),
        ore=material("Ore", pal["ore"], 0.3, metallic=0.75, emission=0.0),
        dirt=material("Ground", pal["dirt"], 0.9, bump=0.3 if detail >= 1 else 0.0, bump_scale=18, variation=0.15),
        grass=material("Grass tufts", pal["grass"], 0.8),
        grass_blades=material("Grass", pal["grass"], 0.7, variation=0.25),
        far_grass=material("Far grass", tuple(c * 0.85 for c in pal["grass"]), 0.9, variation=0.2),
        sack=material("Sackcloth", pal["sack"], 0.9, bump=0.35 if detail >= 1 else 0.0, bump_scale=160, variation=0.08),
    )


def build_study(name, samples, width, height):
    style = STYLES[name]
    scene = reset_scene()
    rng = random.Random(7)
    mats = materials_for(style)
    ground = build_ground(style, mats, rng)
    build_backdrop(style, mats)
    build_boulder(style, mats, rng)
    build_pebbles(style, ground, mats, rng)
    build_sack(style, ground, mats)
    worker, hand_r, hand_l = build_worker(style, mats)
    # Turn the worker three-quarters towards the camera, beside the boulder.
    worker.rotation_euler = (0, 0, math.radians(205))
    worker.location = (0.05, 0.05, 0.0)
    bpy.context.view_layer.update()
    build_pickaxe(style, mats, worker.matrix_world @ hand_r, worker.matrix_world @ hand_l)
    build_grass(style, ground, mats, rng)
    build_lighting(style, scene)
    configure_render(scene, samples, width, height)
    return scene, style


def main():
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", required=True)
    parser.add_argument("--styles", default="soft,lowpoly,grounded")
    parser.add_argument("--samples", type=int, default=96)
    parser.add_argument("--width", type=int, default=1600)
    parser.add_argument("--height", type=int, default=900)
    parser.add_argument("--shots", default="wide,close")
    parser.add_argument("--blend", action="store_true", help="also save each study as a .blend file")
    args = parser.parse_args(argv)
    for name in args.styles.split(","):
        scene, style = build_study(name, args.samples, args.width, args.height)
        for shot in args.shots.split(","):
            build_camera(style, scene, shot)
            path = f"{args.out}/StyleStudy_{name}_{shot}.png"
            scene.render.filepath = path
            bpy.ops.render.render(write_still=True)
            print(f"STYLE_STUDY_OK {name} {shot} {path}")
        if args.blend:
            bpy.ops.wm.save_as_mainfile(filepath=f"{args.out}/StyleStudy_{name}.blend")


if __name__ == "__main__":
    main()
