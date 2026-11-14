"""Render one miner (read-only import of the exported FBX) at rest and in poses, and measure its trunk.
Run: blender -b --factory-startup --python render_miner.py -- <Name> <outdir> [--only rest|poses|measure]
Nothing is saved to any .blend; only PNG renders and a text file are written to outdir."""
import bpy, sys, os, json, math
from mathutils import Vector, Matrix, Quaternion
sys.stdout.reconfigure(encoding='utf-8')

argv = sys.argv[sys.argv.index('--') + 1:]
NAME, OUT = argv[0], argv[1]
ONLY = argv[argv.index('--only') + 1] if '--only' in argv else 'all'
os.makedirs(OUT, exist_ok=True)
PROJ = 'D:/Programs/Unity/Projects/Wonder Gather/The Wanderer/Assets/_WonderGather/Art/Worker/Miners'
FBX = f'{PROJ}/Miner_{NAME}.fbx'
ATLAS = f'{PROJ}/Miner_{NAME}_Atlas.png'
INFO = {m['name']: m for m in json.load(open(f'{PROJ}/miners.json', encoding='utf-8'))['miners']}[NAME]

bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=FBX)
rig = bpy.data.objects[f'Miner_{NAME}_Rig']
body = bpy.data.objects[f'Miner_{NAME}_LOD0']
for o in bpy.data.objects:
    if o.type == 'MESH' and o is not body:
        o.hide_render = True
        o.hide_viewport = True

# ------------------------------------------------------------- materials (atlas loaded read-only)
img = bpy.data.images.load(ATLAS, check_existing=True)
for m in body.data.materials:
    if m is None:
        continue
    m.use_nodes = True
    nt = m.node_tree
    bsdf = next((n for n in nt.nodes if n.type == 'BSDF_PRINCIPLED'), None)
    if m.name == 'Glow':
        for n in list(nt.nodes):
            nt.nodes.remove(n)
        em = nt.nodes.new('ShaderNodeEmission'); em.inputs['Color'].default_value = (1.0, 0.75, 0.35, 1); em.inputs['Strength'].default_value = 3
        out = nt.nodes.new('ShaderNodeOutputMaterial'); nt.links.new(em.outputs[0], out.inputs[0])
        continue
    tex = next((n for n in nt.nodes if n.type == 'TEX_IMAGE'), None)
    if tex is None:
        tex = nt.nodes.new('ShaderNodeTexImage')
    tex.image = img
    if bsdf:
        nt.links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
        bsdf.inputs['Roughness'].default_value = 0.85
        if 'Specular IOR Level' in bsdf.inputs:
            bsdf.inputs['Specular IOR Level'].default_value = 0.2
        for l in list(nt.links):
            if l.to_node == bsdf and l.to_socket.name == 'Alpha':
                nt.links.remove(l)
        bsdf.inputs['Alpha'].default_value = 1.0

# ------------------------------------------------------------- vertex classes
names = [g.name for g in body.vertex_groups]
dom = []
for v in body.data.vertices:
    best, w = None, -1
    for g in v.groups:
        if g.weight > w:
            best, w = names[g.group], g.weight
    dom.append(best or '')
TRUNK = ('Root', 'Pelvis', 'Spine', 'Chest', 'SkirtFront.L', 'SkirtFront.R', 'SkirtBack.L', 'SkirtBack.R')

H = INFO['height']


def measure():
    lines = [f'# {NAME} trunk profile at rest (LOD0, clothed; trunk = vertices whose strongest bone is pelvis, spine, chest or a skirt flap)']
    co = [v.co for v in body.data.vertices]
    zs = [c.z for c in co]
    lines.append(f'height of mesh {max(zs) - min(zs):.3f}')
    prof = []
    z = 0.25 * H
    while z < 0.86 * H:
        pts = [c for c, d in zip(co, dom) if d in TRUNK and abs(c.z - z) < 0.006]
        allp = [c for c in co if abs(c.z - z) < 0.006]
        if pts:
            w = max(p.x for p in pts) - min(p.x for p in pts)
            dp = max(p.y for p in pts) - min(p.y for p in pts)
            wa = max(p.x for p in allp) - min(p.x for p in allp)
            prof.append((z, w, dp, wa))
        z += 0.01
    lines.append('z  z/H  trunkWidth  trunkDepth  allWidth')
    for z, w, dp, wa in prof:
        lines.append(f'{z:.3f} {z / H:.3f} {w:.3f} {dp:.3f} {wa:.3f}')
    # shoulders: outer extent of upper arm sleeves near the shoulder joint
    sh = INFO['shoulder']
    shz = INFO['hipHeight'] + sh[1]
    lines.append(f'shoulder joint height (from json) {shz:.3f}')
    for dz in (0.0, -0.03, -0.06):
        pts = [c for c in co if abs(c.z - (shz + dz)) < 0.008 and abs(c.x) < 0.4]
        lines.append(f'all-vertex width at shoulder{dz:+.2f}: {max(p.x for p in pts) - min(p.x for p in pts):.3f}')
    with open(os.path.join(OUT, f'{NAME}_profile.txt'), 'w', encoding='utf-8') as f:
        f.write('\n'.join(lines))
    print('\n'.join(lines))


# ------------------------------------------------------------- scene, light, camera
scene = bpy.context.scene
try:
    scene.render.engine = 'BLENDER_EEVEE_NEXT'
except TypeError:
    scene.render.engine = 'BLENDER_EEVEE'
scene.view_settings.view_transform = 'Standard'
scene.render.film_transparent = False
world = bpy.data.worlds.new('W'); scene.world = world; world.use_nodes = True
bg = world.node_tree.nodes['Background']; bg.inputs['Color'].default_value = (0.42, 0.45, 0.50, 1); bg.inputs['Strength'].default_value = 0.9

def sun(name, rot, energy, col=(1, 1, 1)):
    l = bpy.data.lights.new(name, 'SUN'); l.energy = energy; l.color = col; l.angle = math.radians(8)
    o = bpy.data.objects.new(name, l); scene.collection.objects.link(o); o.rotation_euler = rot
    return o
sun('Key', (math.radians(50), 0, math.radians(-30)), 3.2, (1.0, 0.95, 0.88))
sun('Fill', (math.radians(70), 0, math.radians(140)), 0.9, (0.8, 0.86, 1.0))

bpy.ops.mesh.primitive_plane_add(size=20, location=(0, 0, 0))
ground = bpy.context.active_object; ground.name = 'Ground'
gm = bpy.data.materials.new('GroundMat'); gm.use_nodes = True
gm.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (0.55, 0.53, 0.48, 1)
gm.node_tree.nodes['Principled BSDF'].inputs['Roughness'].default_value = 1
ground.data.materials.append(gm)

cam_data = bpy.data.cameras.new('Cam'); cam_data.type = 'ORTHO'
cam = bpy.data.objects.new('Cam', cam_data); scene.collection.objects.link(cam); scene.camera = cam


def shoot(path, target, az, el, scale, res=(900, 1200)):
    """az 0 = the miner's front (camera on -Y); 90 = its left side (+X); 180 = back."""
    a, e = math.radians(az), math.radians(el)
    d = 10
    direction = Vector((math.sin(a) * math.cos(e), -math.cos(a) * math.cos(e), math.sin(e)))
    cam.location = Vector(target) + direction * d
    cam.rotation_euler = (-direction).to_track_quat('-Z', 'Y').to_euler()
    cam_data.ortho_scale = scale
    cam_data.clip_end = 40
    scene.render.resolution_x, scene.render.resolution_y = res
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)


black = bpy.data.materials.new('Black'); black.use_nodes = True
nt = black.node_tree
for n in list(nt.nodes):
    nt.nodes.remove(n)
em = nt.nodes.new('ShaderNodeEmission'); em.inputs['Color'].default_value = (0, 0, 0, 1)
o_ = nt.nodes.new('ShaderNodeOutputMaterial'); nt.links.new(em.outputs[0], o_.inputs[0])


def silhouette(on):
    scene.view_layers[0].material_override = black if on else None
    ground.hide_render = on
    bg.inputs['Color'].default_value = (1, 1, 1, 1) if on else (0.42, 0.45, 0.50, 1)
    bg.inputs['Strength'].default_value = 1.0 if on else 0.9
    scene.view_settings.view_transform = 'Standard'


# ------------------------------------------------------------- posing
pb = rig.pose.bones
REST = {b.name: b.matrix_local.copy() for b in rig.data.bones}
ORDER = ['Pelvis', 'Spine', 'Chest', 'Neck', 'Head',
         'UpperArm.L', 'Forearm.L', 'Hand.L', 'UpperArm.R', 'Forearm.R', 'Hand.R',
         'Thigh.L', 'Shin.L', 'Foot.L', 'Toe.L', 'Thigh.R', 'Shin.R', 'Foot.R', 'Toe.R']


def rest_dir(n):
    return (REST[n].to_3x3() @ Vector((0, 1, 0))).normalized()


def rx(v, deg):  # about the body's X (left) axis; +deg turns an up vector forward (to -Y), a down vector back
    return Matrix.Rotation(math.radians(deg), 3, 'X') @ v


def ry(v, deg):
    return Matrix.Rotation(math.radians(deg), 3, 'Y') @ v


def sag(deg_from_down, side=0.0):  # a downward limb turned forward by deg (0 = straight down, 90 = straight ahead)
    a = math.radians(deg_from_down)
    return Vector((side, -math.sin(a), -math.cos(a))).normalized()


def reset():
    for p in pb:
        p.matrix_basis = Matrix.Identity(4)
    rig.location = (0, 0, 0)
    rig.rotation_euler = (0, 0, 0)
    bpy.context.view_layer.update()


def aim(n, d):
    p = pb[n]
    R = rest_dir(n).rotation_difference(Vector(d).normalized())
    m3 = R.to_matrix() @ REST[n].to_3x3()
    head = p.head.copy()
    p.matrix = Matrix.Translation(head) @ m3.to_4x4()
    bpy.context.view_layer.update()


def apply(spec):
    for n in ORDER:
        if n in spec and n in pb:
            aim(n, spec[n])


def world_mesh():
    dg = bpy.context.evaluated_depsgraph_get()
    ev = body.evaluated_get(dg)
    me = ev.to_mesh()
    mw = ev.matrix_world
    pts = [mw @ v.co for v in me.vertices]
    ev.to_mesh_clear()
    return pts


def place_on_ground():
    pts = world_mesh()
    keep = [p for p, d in zip(pts, dom) if not d.startswith('Skirt') and d not in ('Lantern', 'Satchel', 'Mug', 'Hammer')]
    rig.location.z -= min(p.z for p in keep)
    bpy.context.view_layer.update()


def drape():
    """A static stand-in for MinerBody.Drape with flaps on their seams: each flap hangs by gravity, is stopped by
    its thigh's front (front flaps) or back (back flaps), and lies on the ground. Then hanging things hang down."""
    slack = INFO['skirtSlack']
    to_arm = rig.matrix_world.inverted().to_3x3()
    down = (to_arm @ Vector((0, 0, -1))).normalized()
    pel = pb['Pelvis'].matrix.to_3x3()
    across = (pel @ Vector((1, 0, 0))).normalized()
    for side in ('L', 'R'):
        th = pb[f'Thigh.{side}']
        along = (th.tail - th.head)
        along = along - across * along.dot(across)
        along = along.normalized() if along.length > 1e-4 else down
        front = across.cross(along).normalized()
        thigh_front = (th.matrix.to_3x3() @ (REST[f'Thigh.{side}'].to_3x3().inverted() @ Vector((0, -1, 0)))).normalized()
        if front.dot(thigh_front) < 0:
            front = -front
        for flap, n_sign, sl in (('Front', 1, slack[0]), ('Back', -1, slack[1])):
            name = f'Skirt{flap}.{side}'
            if name not in pb:
                continue
            nrm = front * n_sign
            s = math.sin(math.radians(sl))
            h = down.copy()
            if h.dot(nrm) < -s:
                h = (h - nrm * (h.dot(nrm) + s)).normalized()
            p = pb[name]
            hip_w = rig.matrix_world @ p.head
            L = (REST[name].to_translation() - (REST[name] @ Vector((0, 1, 0)))).length
            L = max(0.01, rig.data.bones[name].length)
            room = hip_w.z / L
            hw = rig.matrix_world.to_3x3() @ h
            if -hw.z > room and room < 1:
                level = Vector((hw.x, hw.y, 0))
                if level.length < 1e-3:
                    level = rig.matrix_world.to_3x3() @ (pel @ Vector((0, -n_sign, 0)))
                    level.z = 0
                level.normalize()
                dr = max(-0.2, min(1, room))
                hw = (level * math.sqrt(max(0, 1 - dr * dr)) + Vector((0, 0, -1)) * dr).normalized()
                h = (to_arm @ hw).normalized()
            aim(name, h)
    for n in ('Lantern', 'Satchel', 'Mug', 'Hammer'):
        if n in pb:
            aim(n, down)


THIGH = rig.data.bones['Thigh.L'].length
SHIN = rig.data.bones['Shin.L'].length
ANK = INFO['ankleHeight']
LEGR = INFO['weights'].get('legRadius', 0.045) if isinstance(INFO.get('weights'), dict) else 0.045
HIP = INFO['hipHeight']


def poses():
    P = {}
    up = Vector((0, 0, 1))
    # 1. a walking stride: the left leg ahead near heel strike, the right behind near toe-off, arms opposite
    P['stride'] = {
        'Thigh.L': sag(24), 'Shin.L': sag(8), 'Foot.L': rx(rest_dir('Foot.L'), -12),
        'Thigh.R': sag(-16), 'Shin.R': sag(-48), 'Foot.R': rx(rest_dir('Foot.R'), 35), 'Toe.R': rest_dir('Toe.R'),
        'UpperArm.L': rx(rest_dir('UpperArm.L'), 22), 'Forearm.L': rx(rest_dir('Forearm.L'), 10),
        'UpperArm.R': rx(rest_dir('UpperArm.R'), -20), 'Forearm.R': rx(rest_dir('Forearm.R'), -40),
        'Spine': rx(up, 3), 'Chest': rx(up, 3)}
    # 2. the left knee raised to about 90 degrees (thigh level, shin hanging)
    P['kneeup'] = {'Thigh.L': sag(90, 0.05), 'Shin.L': sag(5), 'Foot.L': rest_dir('Foot.L'),
                   'UpperArm.R': rx(rest_dir('UpperArm.R'), -15), 'UpperArm.L': rx(rest_dir('UpperArm.L'), 10)}
    # 3. sitting on the ground, knees up, feet flat, hands resting by the hips; hip joint at half the pelvis depth
    hseat = 0.5 * (0.17 if NAME == 'Small' else 0.18 if NAME == 'Long' else 0.26) + 0.02
    th_up = 12
    knee_z = hseat + THIGH * math.sin(math.radians(th_up))
    c = max(-1, min(1, (knee_z - 0.6 * ANK) / SHIN))
    shin_fwd = math.degrees(math.acos(c))
    P['sit'] = {'Spine': rx(up, -10), 'Chest': rx(up, -5), 'Neck': rx(up, 8), 'Head': rx(up, 8),
                'Thigh.L': sag(90 + th_up, 0.18), 'Shin.L': sag(shin_fwd, -0.05),
                'Thigh.R': sag(90 + th_up, -0.18), 'Shin.R': sag(shin_fwd, 0.05),
                'UpperArm.L': Vector((0.35, 0.35, -0.87)), 'Forearm.L': Vector((0.1, 0.25, -0.96)),
                'UpperArm.R': Vector((-0.35, 0.35, -0.87)), 'Forearm.R': Vector((-0.1, 0.25, -0.96))}
    # 4. lying on the back: legs nearly straight, arms at the sides (posed standing, then the rig is laid down)
    P['back'] = {'Thigh.L': sag(6, 0.08), 'Shin.L': sag(0), 'Thigh.R': sag(6, -0.08), 'Shin.R': sag(0),
                 'UpperArm.L': Vector((0.3, 0, -1)), 'UpperArm.R': Vector((-0.3, 0, -1)),
                 '_rot': (math.radians(-90), 0, 0)}
    # 5. lying on the right side: the top (left) leg forward and bent, the under leg straighter, the under arm forward
    P['side'] = {'Thigh.L': sag(45, 0.0), 'Shin.L': sag(0), 'Thigh.R': sag(15), 'Shin.R': sag(-10),
                 'UpperArm.R': sag(70, -0.1), 'Forearm.R': sag(100, 0.1), 'UpperArm.L': Vector((0.15, -0.2, -1)),
                 'Spine': rx(up, 8), 'Chest': rx(up, 8),
                 '_rot': (0, math.radians(-90), 0)}
    # 6. kneeling on the right knee as when getting up: left foot planted ahead, right knee and toes on the ground,
    #    leaning forward, the left hand on the left knee
    hk = THIGH + LEGR * 1.2
    cl = max(-1, min(1, (hk - SHIN - ANK) / THIGH))
    th_l = math.degrees(math.acos(cl))
    P['kneel'] = {'Spine': rx(up, 12), 'Chest': rx(up, 12), 'Neck': rx(up, 5), 'Head': rx(up, -10),
                  'Thigh.L': sag(th_l, 0.08), 'Shin.L': sag(4), 'Foot.L': rest_dir('Foot.L'), 'Toe.L': rest_dir('Toe.L'),
                  'Thigh.R': sag(-4, -0.04), 'Shin.R': sag(-76), 'Foot.R': sag(-12), 'Toe.R': sag(90),
                  'UpperArm.L': sag(35, 0.25), 'Forearm.L': sag(70, -0.1),
                  'UpperArm.R': rx(rest_dir('UpperArm.R'), -10), 'Forearm.R': sag(25, -0.1)}
    return P


def pose(name, spec):
    reset()
    rot = spec.get('_rot')
    apply({k: v for k, v in spec.items() if not k.startswith('_')})
    if rot:
        rig.rotation_euler = rot
        bpy.context.view_layer.update()
    place_on_ground()
    drape()
    bpy.context.view_layer.update()


def pelvis_world():
    return rig.matrix_world @ pb['Pelvis'].head


if ONLY in ('all', 'measure'):
    measure()

if ONLY in ('all', 'rest'):
    reset(); drape(); bpy.context.view_layer.update()
    tgt = (0, 0, 0.98)
    for view, az in (('front', 0), ('threequarter', 40), ('side', 90), ('threequarterback', 140), ('back', 180)):
        shoot(os.path.join(OUT, f'{NAME}_rest_{view}.png'), tgt, az, 4, 2.15)
    silhouette(True)
    for view, az in (('front', 0), ('side', 90)):
        shoot(os.path.join(OUT, f'{NAME}_rest_silhouette_{view}.png'), tgt, az, 0, 2.15)
    silhouette(False)
    # close views of the face and of the waist, three-quarter front
    hz = rig.matrix_world @ pb['Head'].head
    shoot(os.path.join(OUT, f'{NAME}_rest_close_head.png'), (0, 0, hz.z + 0.06), 30, 4, 0.55, (900, 900))
    shoot(os.path.join(OUT, f'{NAME}_rest_close_waist.png'), (0, 0, HIP + 0.12), 30, 4, 0.75, (900, 900))
    shoot(os.path.join(OUT, f'{NAME}_rest_close_hands.png'), (0.33 * H / 1.42 * 1.0, 0, HIP - 0.05), 60, 4, 0.45, (900, 900))
    # the game's distance: a high, far view (Strategy-like), small in frame
    shoot(os.path.join(OUT, f'{NAME}_rest_gamecamera.png'), (0, 0, 0.8), 30, 50, 4.0, (640, 640))

if ONLY in ('all', 'poses'):
    for name, spec in poses().items():
        pose(name, spec)
        pw = pelvis_world()
        lying = '_rot' in spec
        if lying:
            ctr = (pw.x * 0.5, pw.y * 0.5 + (0.0), 0.25)
            shoot(os.path.join(OUT, f'{NAME}_{name}_threequarter.png'), (pw.x, pw.y + 0.25 * H if name == 'back' else pw.y, 0.2), 35, 30, 2.3, (1200, 900))
            shoot(os.path.join(OUT, f'{NAME}_{name}_side.png'), (pw.x, pw.y + 0.25 * H if name == 'back' else pw.y, 0.2), 90 if name == 'back' else 0, 8, 2.3, (1200, 900))
            shoot(os.path.join(OUT, f'{NAME}_{name}_close_hips.png'), (pw.x, pw.y, pw.z), 35 if name == 'back' else 20, 45, 0.85, (900, 900))
        else:
            ctr = (0, 0, max(0.6, rig.matrix_world.translation.z + 0.95))
            shoot(os.path.join(OUT, f'{NAME}_{name}_threequarter.png'), (0, 0, 0.98 if name in ('stride', 'kneeup') else 0.6), 40, 4, 2.15 if name in ('stride', 'kneeup') else 1.7, (900, 1200) if name in ('stride', 'kneeup') else (1100, 1100))
            shoot(os.path.join(OUT, f'{NAME}_{name}_side.png'), (0, 0, 0.98 if name in ('stride', 'kneeup') else 0.6), 90, 4, 2.15 if name in ('stride', 'kneeup') else 1.7, (900, 1200) if name in ('stride', 'kneeup') else (1100, 1100))
            shoot(os.path.join(OUT, f'{NAME}_{name}_close_hips.png'), (pw.x, pw.y, pw.z - 0.05), 35, 22, 0.85, (900, 900))
            shoot(os.path.join(OUT, f'{NAME}_{name}_close_hips_right.png'), (pw.x, pw.y, pw.z - 0.05), -45, 22, 0.85, (900, 900))
print('DONE', NAME)
