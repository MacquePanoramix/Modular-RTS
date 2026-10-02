"""Wonder Gather — procedural hand-painted textures (S1c, second pass).

Paints each material the way a background painter would, then bakes the result
into one colour texture per material:

1. broad warm/cool value shifts across the surface;
2. directional brush strokes with grain;
3. a motif particular to the material (plaster patches, wood grain, per-shingle
   variation, stone outlines, leaf dabs);
4. cool coloured cavities (ambient occlusion);
5. lit, worn edges (a bevel-normal edge mask);
6. moss or dust on upward faces, and stains low on walls.

All faces that use a material are unwrapped together into that material's
texture, so a material keeps one texture across every object that uses it.

Used by house.py and nature.py with --paint <texture folder>.
"""
import math

import bmesh
import bpy

RECIPES = {}


def recipe(name, **values):
    RECIPES[name] = dict(values)


# Base colours are sRGB-ish painter picks; each recipe tunes the painter's layers.
recipe("Plaster", base=(0.82, 0.71, 0.55), size=2048, motif="plaster", strokes=(1, 6, 1), stroke_scale=3.0,
       warm=(1.08, 1.0, 0.86), cool=(0.86, 0.9, 1.0), cavity=(0.6, 0.52, 0.5), edge=(1.08, 1.05, 0.98), stains=0.75)
recipe("Interior", base=(0.86, 0.60, 0.36), size=1024, motif="plaster", strokes=(1, 5, 1), stroke_scale=3.0,
       warm=(1.06, 1.0, 0.9), cool=(0.92, 0.9, 0.95), cavity=(0.5, 0.36, 0.3), edge=(1.05, 1.02, 0.95))
recipe("Timber", base=(0.36, 0.26, 0.17), size=2048, motif="wood", strokes=(8, 1, 1), stroke_scale=2.0,
       warm=(1.1, 1.0, 0.88), cool=(0.88, 0.9, 0.96), cavity=(0.58, 0.5, 0.48), edge=(1.4, 1.28, 1.1), daub=0.4)
recipe("Roof", base=(0.27, 0.24, 0.23), size=2048, motif="shingles", strokes=(1, 4, 1), stroke_scale=4.0,
       warm=(1.04, 1.0, 0.95), cool=(0.92, 0.94, 1.02), cavity=(0.32, 0.31, 0.36), edge=(1.45, 1.38, 1.3), moss=0.4)
recipe("Stone", base=(0.46, 0.43, 0.39), size=2048, motif="stones", strokes=(1, 1, 3), stroke_scale=3.0,
       warm=(1.08, 1.02, 0.94), cool=(0.86, 0.9, 1.0), cavity=(0.3, 0.3, 0.34), edge=(1.35, 1.3, 1.2), moss=0.45)
recipe("Clay", base=(0.64, 0.36, 0.22), size=1024, motif="none", strokes=(1, 4, 1), stroke_scale=5.0,
       warm=(1.08, 1.0, 0.9), cool=(0.85, 0.85, 0.95), cavity=(0.4, 0.3, 0.3), edge=(1.25, 1.15, 1.0), stains=0.4, angle=75)
recipe("Foliage", base=(0.20, 0.33, 0.16), size=1024, motif="leaves", strokes=(1, 1, 1), stroke_scale=6.0,
       warm=(1.15, 1.1, 0.8), cool=(0.8, 0.9, 1.1), cavity=(0.35, 0.45, 0.5), edge=(1.35, 1.3, 1.0), angle=75)
recipe("Cloth", base=(0.62, 0.13, 0.09), size=1024, motif="none", strokes=(1, 6, 1), stroke_scale=4.0,
       warm=(1.08, 1.0, 0.9), cool=(0.8, 0.85, 1.0), cavity=(0.45, 0.25, 0.32), edge=(1.2, 1.1, 1.0))
recipe("Firewood", base=(0.48, 0.35, 0.23), size=1024, motif="wood", strokes=(8, 1, 1), stroke_scale=2.5,
       warm=(1.1, 1.0, 0.9), cool=(0.85, 0.88, 0.95), cavity=(0.35, 0.3, 0.3), edge=(1.4, 1.3, 1.1), daub=0.55)
recipe("Iron", base=(0.16, 0.15, 0.15), size=512, motif="none", strokes=(1, 3, 1), stroke_scale=6.0,
       warm=(1.2, 1.0, 0.85), cool=(0.9, 0.95, 1.05), cavity=(0.6, 0.55, 0.55), edge=(2.2, 1.7, 1.3), stains=0.5)
recipe("Bark", base=(0.24, 0.20, 0.17), size=1024, motif="wood", strokes=(1, 1, 7), stroke_scale=2.0,
       warm=(1.1, 1.0, 0.9), cool=(0.85, 0.9, 1.0), cavity=(0.35, 0.33, 0.36), edge=(1.4, 1.3, 1.15), moss=0.35, angle=75, daub=0.65)
recipe("Leaves", base=(0.16, 0.28, 0.15), size=2048, motif="leaves", strokes=(1, 1, 1), stroke_scale=3.0,
       warm=(1.2, 1.12, 0.78), cool=(0.78, 0.9, 1.12), cavity=(0.3, 0.42, 0.5), edge=(1.3, 1.25, 0.95), angle=75)
recipe("LeavesLight", base=(0.29, 0.39, 0.17), size=1024, motif="leaves", strokes=(1, 1, 1), stroke_scale=3.0,
       warm=(1.18, 1.1, 0.8), cool=(0.8, 0.9, 1.1), cavity=(0.35, 0.45, 0.5), edge=(1.3, 1.25, 0.95), angle=75)
recipe("Rock", base=(0.31, 0.31, 0.31), size=1024, motif="rock", strokes=(1, 1, 2), stroke_scale=3.0,
       warm=(1.08, 1.02, 0.94), cool=(0.85, 0.9, 1.04), cavity=(0.28, 0.28, 0.34), edge=(1.45, 1.4, 1.3), moss=0.5, angle=75)


class Graph:
    """A small helper for wiring shader nodes."""

    def __init__(self, material):
        material.use_nodes = True
        self.tree = material.node_tree
        self.nodes = self.tree.nodes
        self.links = self.tree.links
        self.nodes.clear()
        self.coords = self.node("ShaderNodeTexCoord").outputs["Object"]
        self.geometry = self.node("ShaderNodeNewGeometry")

    def node(self, kind, **settings):
        n = self.nodes.new(kind)
        for key, value in settings.items():
            setattr(n, key, value)
        return n

    def link(self, a, b):
        self.links.new(a, b)

    def stretched(self, scale):
        mapping = self.node("ShaderNodeMapping")
        mapping.inputs["Scale"].default_value = scale
        self.link(self.coords, mapping.inputs["Vector"])
        return mapping.outputs["Vector"]

    def noise(self, scale, detail=3.0, roughness=0.55, vector=None, distortion=0.0):
        n = self.node("ShaderNodeTexNoise")
        n.inputs["Scale"].default_value = scale
        n.inputs["Detail"].default_value = detail
        n.inputs["Roughness"].default_value = roughness
        n.inputs["Distortion"].default_value = distortion
        self.link(vector if vector is not None else self.coords, n.inputs["Vector"])
        return n.outputs["Fac"]

    def remap(self, value, low, high, out_low=0.0, out_high=1.0):
        n = self.node("ShaderNodeMapRange", clamp=True)
        self.link(value, n.inputs["Value"])
        n.inputs["From Min"].default_value = low
        n.inputs["From Max"].default_value = high
        n.inputs["To Min"].default_value = out_low
        n.inputs["To Max"].default_value = out_high
        return n.outputs["Result"]

    def math(self, op, a, b=None):
        n = self.node("ShaderNodeMath", operation=op, use_clamp=False)
        for i, v in enumerate((a, b)):
            if v is None:
                continue
            if isinstance(v, (int, float)):
                n.inputs[i].default_value = v
            else:
                self.link(v, n.inputs[i])
        return n.outputs["Value"]

    def mix(self, a, b, factor, blend='MIX'):
        n = self.node("ShaderNodeMix", data_type='RGBA', blend_type=blend, clamp_result=True)
        for socket, value in ((n.inputs[6], a), (n.inputs[7], b)):
            if isinstance(value, tuple):
                socket.default_value = (*value, 1)
            else:
                self.link(value, socket)
        if isinstance(factor, (int, float)):
            n.inputs["Factor"].default_value = factor
        else:
            self.link(factor, n.inputs["Factor"])
        return n.outputs[2]

    def tint(self, color, factors):
        """Multiplies a colour socket by an RGB factor."""
        return self.mix(color, tuple(factors), 1.0, 'MULTIPLY')


def scaled(rgb, k):
    return tuple(max(0.0, min(1.0, c * k)) for c in rgb)


def build_painting(material, r):
    g = Graph(material)
    base = r["base"]
    # 1. Broad warm/cool patches, the way a painter varies a flat colour.
    broad = g.remap(g.noise(0.6, 2.0), 0.35, 0.65)
    color = g.mix(tuple(min(1, c * w) for c, w in zip(base, r["warm"])), tuple(min(1, c * w) for c, w in zip(base, r["cool"])), broad)
    mid = g.remap(g.noise(2.2, 3.0), 0.3, 0.7)
    color = g.mix(g.tint(color, (0.84, 0.84, 0.88)), g.tint(color, (1.12, 1.1, 1.05)), mid)
    # Paint daubs: flat patches of slightly different tone, as a brush lays them down.
    daubs = g.node("ShaderNodeTexVoronoi", feature='SMOOTH_F1')
    daubs.inputs["Scale"].default_value = r["stroke_scale"] * 1.6
    daubs.inputs["Randomness"].default_value = 1.0
    daubs.inputs["Smoothness"].default_value = 0.35
    # Warp the cells so each daub has an irregular, brushed outline.
    warp = g.node("ShaderNodeTexNoise")
    warp.inputs["Scale"].default_value = r["stroke_scale"] * 2.5
    warp.inputs["Detail"].default_value = 3.0
    g.link(g.coords, warp.inputs["Vector"])
    offset = g.node("ShaderNodeVectorMath", operation='MULTIPLY_ADD')
    g.link(warp.outputs["Color"], offset.inputs[0])
    offset.inputs[1].default_value = (0.12, 0.12, 0.12)
    g.link(g.stretched(tuple(1.0 / s ** 0.5 for s in r["strokes"])), offset.inputs[2])
    g.link(offset.outputs["Vector"], daubs.inputs["Vector"])
    daub_tone = g.node("ShaderNodeSeparateColor")
    g.link(daubs.outputs["Color"], daub_tone.inputs["Color"])
    # Daub strength per material: wood takes a calmer hand so its grain, not spots, carries it.
    d = r.get("daub", 1.0)
    color = g.mix(g.tint(color, (1 - 0.12 * d, 1 - 0.12 * d, 1 - 0.08 * d)), g.tint(color, (1 + 0.1 * d, 1 + 0.08 * d, 1 + 0.03 * d)),
                  daub_tone.outputs["Red"])
    # 2. Directional brush strokes with grain.
    strokes = g.remap(g.noise(r["stroke_scale"], 4.0, 0.6, g.stretched(r["strokes"]), 0.8), 0.36, 0.64)
    color = g.mix(g.tint(color, (0.86, 0.86, 0.9)), g.tint(color, (1.1, 1.08, 1.04)), strokes)
    # 3. The material's own motif.
    motif = r["motif"]
    if motif == "plaster":
        # Patches where the plaster has worn through to stone and older layers.
        holes = g.remap(g.noise(1.4, 4.0, 0.6, None, 1.2), 0.6, 0.64)
        stone = g.node("ShaderNodeTexVoronoi", feature='F1')
        stone.inputs["Scale"].default_value = 9
        g.link(g.coords, stone.inputs["Vector"])
        joints = g.remap(stone.outputs["Distance"], 0.05, 0.2)
        exposed = g.mix((0.32, 0.29, 0.27), (0.55, 0.50, 0.44), joints)
        color = g.mix(color, exposed, holes)
        old_layer = g.remap(g.noise(0.9, 3.0, 0.5, None, 0.6), 0.6, 0.64)
        color = g.mix(color, g.tint(color, (0.82, 0.74, 0.64)), g.math('MULTIPLY', old_layer, 0.8))
        # Grime gathers along the wall's foot and under the eaves.
        level = g.node("ShaderNodeSeparateXYZ")
        g.link(g.geometry.outputs["Position"], level.inputs["Vector"])
        foot = g.remap(level.outputs["Z"], 0.7, 0.0)
        color = g.mix(color, g.tint(color, (0.66, 0.6, 0.52)), g.math('MULTIPLY', foot, 0.75))
    elif motif == "wood":
        grain = g.node("ShaderNodeTexWave", wave_type='BANDS', bands_direction='Z', wave_profile='SIN')
        grain.inputs["Scale"].default_value = 3.0
        grain.inputs["Distortion"].default_value = 6.0
        grain.inputs["Detail"].default_value = 3.0
        g.link(g.stretched(tuple(1 / max(s, 1) * 4 for s in r["strokes"])), grain.inputs["Vector"])
        lines = g.remap(grain.outputs["Fac"], 0.2, 0.8)
        color = g.mix(g.tint(color, (0.86, 0.83, 0.8)), color, lines)
    elif motif == "shingles":
        island = g.geometry.outputs["Random Per Island"]
        tone = g.remap(island, 0.0, 1.0, 0.0, 1.0)
        # Weathered boards: value varies board by board, hue only a little.
        color = g.mix(g.tint(color, (0.82, 0.82, 0.84)), g.tint(color, (1.12, 1.08, 1.03)), tone)
        weather = g.remap(island, 0.85, 0.97)
        color = g.mix(color, g.tint(color, (1.06, 0.97, 0.9)), weather)
    elif motif == "stones":
        stones = g.node("ShaderNodeTexVoronoi", feature='DISTANCE_TO_EDGE')
        stones.inputs["Scale"].default_value = 4.5
        g.link(g.stretched((1.0, 1.0, 1.6)), stones.inputs["Vector"])
        mortar = g.remap(stones.outputs["Distance"], 0.02, 0.07)
        color = g.mix(g.tint(color, (0.68, 0.66, 0.68)), color, mortar)
        cells = g.node("ShaderNodeTexVoronoi", feature='F1')
        cells.inputs["Scale"].default_value = 6.0
        g.link(g.stretched((1.0, 1.0, 1.6)), cells.inputs["Vector"])
        color = g.mix(g.tint(color, (0.88, 0.9, 0.95)), g.tint(color, (1.1, 1.05, 0.98)), cells.outputs["Color"], 'MIX')
    elif motif == "leaves":
        dabs = g.node("ShaderNodeTexVoronoi", feature='F1')
        dabs.inputs["Scale"].default_value = 7.0
        g.link(g.coords, dabs.inputs["Vector"])
        tone = g.remap(dabs.outputs["Distance"], 0.0, 0.6)
        color = g.mix(g.tint(color, (1.2, 1.15, 0.85)), g.tint(color, (0.75, 0.85, 1.0)), tone)
    elif motif == "rock":
        cracks = g.node("ShaderNodeTexVoronoi", feature='DISTANCE_TO_EDGE')
        cracks.inputs["Scale"].default_value = 3.0
        g.link(g.coords, cracks.inputs["Vector"])
        crack = g.remap(cracks.outputs["Distance"], 0.0, 0.04)
        color = g.mix(g.tint(color, (0.55, 0.55, 0.62)), color, crack)
    # Up-facing faces catch the sky and gather moss or dust; undersides stay cool.
    up = g.node("ShaderNodeSeparateXYZ")
    g.link(g.geometry.outputs["Normal"], up.inputs["Vector"])
    top = g.remap(up.outputs["Z"], -0.2, 1.0)
    color = g.mix(g.tint(color, (0.86, 0.9, 1.0)), g.tint(color, (1.06, 1.04, 0.98)), top)
    if r.get("moss"):
        patches = g.remap(g.noise(1.8, 4.0, 0.6, None, 0.5), 0.5, 0.62)
        moss = g.math('MULTIPLY', g.math('MULTIPLY', g.remap(up.outputs["Z"], 0.35, 0.8), patches), r["moss"])
        color = g.mix(color, (0.27, 0.34, 0.15), moss)
    if r.get("stains"):
        height = g.node("ShaderNodeSeparateXYZ")
        g.link(g.geometry.outputs["Position"], height.inputs["Vector"])
        low = g.remap(height.outputs["Z"], 0.9, 0.05)
        drips = g.remap(g.noise(3.0, 4.0, 0.6, g.stretched((6.0, 6.0, 0.4)), 0.5), 0.45, 0.7)
        stain = g.math('MULTIPLY', g.math('MULTIPLY', low, drips), r["stains"])
        color = g.mix(color, g.tint(color, (0.62, 0.6, 0.55)), stain)
    # 4. Cavities: cool and coloured, not black.
    ao = g.node("ShaderNodeAmbientOcclusion", samples=16)
    ao.inputs["Distance"].default_value = 0.35
    cavity = g.remap(ao.outputs["AO"], 0.2, 1.0)
    color = g.mix(g.tint(color, r["cavity"]), color, cavity)
    # 5. Lit, worn edges.
    bevel = g.node("ShaderNodeBevel", samples=8)
    bevel.inputs["Radius"].default_value = 0.025
    dot = g.node("ShaderNodeVectorMath", operation='DOT_PRODUCT')
    g.link(bevel.outputs["Normal"], dot.inputs[0])
    g.link(g.geometry.outputs["Normal"], dot.inputs[1])
    edge = g.remap(dot.outputs["Value"], 0.995, 0.93)
    worn = g.remap(g.noise(8.0, 3.0), 0.35, 0.6)
    color = g.mix(color, g.tint(color, r["edge"]), g.math('MULTIPLY', edge, worn))
    emission = g.node("ShaderNodeEmission")
    g.link(color, emission.inputs["Color"])
    output = g.node("ShaderNodeOutputMaterial")
    g.link(emission.outputs["Emission"], output.inputs["Surface"])
    return g


def unwrap_by_material(objects, angles=None):
    """Gives every face UVs inside its own material's texture."""
    materials = {}
    for obj in objects:
        for slot in obj.material_slots:
            if slot.material is not None:
                materials.setdefault(slot.material.name, []).append(obj)
    for obj in objects:
        if not obj.data.uv_layers:
            obj.data.uv_layers.new(name="UVMap")
    for name, users in materials.items():
        if bpy.context.object and bpy.context.object.mode != 'OBJECT':
            bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.select_all(action='DESELECT')
        for obj in users:
            obj.select_set(True)
        bpy.context.view_layer.objects.active = users[0]
        bpy.ops.object.mode_set(mode='EDIT')
        for obj in users:
            bm = bmesh.from_edit_mesh(obj.data)
            index = [i for i, s in enumerate(obj.material_slots) if s.material and s.material.name == name][0]
            for f in bm.faces:
                f.select = f.material_index == index
            bmesh.update_edit_mesh(obj.data)
        angle = (angles or {}).get(name, 60)
        bpy.ops.uv.smart_project(angle_limit=math.radians(angle), island_margin=0.006, area_weight=0.0, correct_aspect=True,
                                 scale_to_bounds=False)
        bpy.ops.object.mode_set(mode='OBJECT')


def paint(objects, folder, samples=48):
    """Unwraps, paints and bakes every material on these objects; returns {material: texture path}."""
    scene = bpy.context.scene
    scene.render.engine = 'CYCLES'
    scene.cycles.samples = samples
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
    meshes = [o for o in objects if o.type == 'MESH']
    unwrap_by_material(meshes, {name: r.get("angle", 60) for name, r in RECIPES.items()})
    textures = {}
    for name in sorted({s.material.name for o in meshes for s in o.material_slots if s.material}):
        if name not in RECIPES:
            continue
        r = RECIPES[name]
        material = bpy.data.materials[name]
        g = build_painting(material, r)
        image = bpy.data.images.new(name + "_Painted", r["size"], r["size"], alpha=False)
        # Fill the space between islands with the material's colour so distant mipmaps never darken.
        import numpy
        image.pixels.foreach_set(numpy.tile(numpy.array([*r["base"], 1.0], dtype=numpy.float32), r["size"] * r["size"]))
        target = g.node("ShaderNodeTexImage")
        target.image = image
        g.nodes.active = target
        textures[name] = (image, f"{folder}/{name}.jpg")
    # Materials that are not painted (for example the emissive window glass) still need an
    # active target for Cycles; they bake into a throwaway image.
    scratch = bpy.data.images.new("Unpainted", 16, 16)
    for name in {s.material.name for o in meshes for s in o.material_slots if s.material} - set(textures):
        material = bpy.data.materials[name]
        material.use_nodes = True
        node = material.node_tree.nodes.new("ShaderNodeTexImage")
        node.image = scratch
        material.node_tree.nodes.active = node
    meshes = [o for o in meshes if any(s.material and s.material.name in textures for s in o.material_slots)]
    # Every material has its target image active; one bake per object fills them all.
    scene.render.bake.margin = 16
    scene.render.bake.use_clear = True
    for obj in meshes:
        bpy.ops.object.select_all(action='DESELECT')
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.bake(type='EMIT', margin=16, use_clear=False)
    scene.view_settings.view_transform = 'Standard'
    scene.view_settings.look = 'None'
    scene.view_settings.exposure = 0
    scene.view_settings.gamma = 1
    settings = scene.render.image_settings
    settings.file_format = 'JPEG'
    settings.quality = 90
    settings.color_mode = 'RGB'
    paths = {}
    for name, (image, path) in textures.items():
        image.save_render(bpy.path.abspath(path), scene=scene)
        paths[name] = path
        # Show the painting in Blender previews too.
        material = bpy.data.materials[name]
        material.node_tree.nodes.clear()
        tex = material.node_tree.nodes.new("ShaderNodeTexImage")
        tex.image = image
        bsdf = material.node_tree.nodes.new("ShaderNodeBsdfPrincipled")
        bsdf.inputs["Roughness"].default_value = 0.9
        out = material.node_tree.nodes.new("ShaderNodeOutputMaterial")
        material.node_tree.links.new(tex.outputs["Color"], bsdf.inputs["Base Color"])
        material.node_tree.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    return paths
