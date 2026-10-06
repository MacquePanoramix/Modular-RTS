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
import rigging  # noqa: E402
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
                  arm=0.033, leg=0.041, neck_r=0.4,
                  nose=[((0, -1.0, -0.3), 0.085, (1.0, 1.0, 0.9))], cheeks=0.3, jaw=0.62, chin=0.24,
                  pose=dict(weight=-1, hip_shift=0.02, hip_tilt=0.012, shoulder_tilt=0.015, turn=0.22, nod=-0.2, tilt=0.16, free_foot=(1.7, -0.07)),
                  arms=[dict(wrist=at_hip(0.07, -0.1, -0.04), pole=(1, 0.6, -0.1)),
                        dict(wrist=lambda b, s: b.chest + Vector((s * 0.045, -b.depth("chest") - 0.075, 0.0)), pole=(1, 0.2, -1.0))]),
        # Light and young: brisk, bouncy steps, arms swinging freely.
        gait=dict(froude=0.27, bounce=1.35, sway=0.9, arm_swing=1.15),
        hands=[dict(grip="carry", ring=(0, -1, 0), bar=0.007), dict(grip="grip", palm=(0, 1, 0), along=(-0.3, 0.1, 1))],
        face=dict(style="curious", skin="Skin_Small", soot=[((0.52, -0.28), 0.1, 0.3, 0.6)]),
        hair=dict(style="bob", mat="Hair"),
        outfit=[("top", dict(mat="CoatBlue", loose=0.024, sleeve=1.07, cuff=1.45, skirted=True)),
                ("trousers", dict(mat="TrousersGrey", boot=0.13, hidden_above=0.30 * 1.42 + 0.06)),
                ("skirt", dict(mat="CoatBlue", hem=0.31, flare=1.38, loose=0.026, folds=0.07, fold_count=7, ragged=0.012)),
                # Buttoned up to the collar: it closes at the front too.
                ("collar", dict(mat="CoatBlue", height=0.085, wide=2.5, open_front=False)),
                ("buttons", dict(mat="Button", count=3, loose=0.026)),
                ("boots", dict(shaft=0.13)),
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
        # Tall and unhurried: long, smooth strides, little bounce, long arms swinging loosely.
        gait=dict(froude=0.21, bounce=0.75, sway=0.8, arm_swing=1.25),
        hands=[dict(grip="grip", along=(0.2, -1, 0.0), ring=lambda b, i: b.wrists[i] - Vector((b.wrists[i].x * 1.25, b.wrists[i].y * 1.4, 0))),
               dict(grip="carry", ring=(-1, 0, 0), bar=0.0055)],
        face=dict(style="sleepy", skin="Skin_Long"),
        hair=dict(style="swept", mat="Hair"),
        outfit=[("top", dict(mat="Coat", loose=0.016, sleeve=1.0, cuff=1.3, skirted=True)),
                ("trousers", dict(mat="Trousers", boot=0.19)),
                ("skirt", dict(mat="Coat", hem=0.26, flare=1.3, loose=0.018, open_front=True, folds=0.06, fold_count=6, ragged=0.03)),
                ("shirt_front", dict(mat="Shirt", loose=0.016)),
                ("lapels", dict(mat="Coat", loose=0.016)),
                ("collar", dict(mat="Coat", height=0.07, wide=2.0)),
                ("patch", dict(mat="Patch", onto="Top", where=lambda b: (b.chest + Vector((0.075, 0, -0.12))), size=(0.05, 0.002, 0.045), angle=0.2, name="Patch0")),
                ("patch", dict(mat="Patch", onto="Skirt", where=lambda b: (b.pelvis + Vector((-0.14, 0, -0.22))), size=(0.045, 0.002, 0.055), angle=-0.12, name="Patch1")),
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
        # Sturdy and heavy-set: a rolling side-to-side walk, arms swinging less round the body.
        gait=dict(froude=0.24, bounce=1.1, sway=1.7, arm_swing=0.75),
        # Hands on hips: palms against the hips, fingers down and back, thumbs forward.
        hands=[dict(grip="open", palm=(1, 0, 0), along=(0, 0.5, -1)), dict(grip="open", palm=(-1, 0, 0), along=(0, 0.5, -1))],
        face=dict(style="laughing", skin="Skin_Round", soot=[((-0.55, -0.18), 0.09, 0.25, 0.6)]),
        # The curls stop above the smock's neck (the head sits low on the shoulders).
        hair=dict(style="curls", mat="HairBrown", top=0.45, nape=-0.5),
        outfit=[("top", dict(mat="Smock", loose=0.018, rolled=True, skirted=True)),  # the roll stays clear of the hands
                ("trousers", dict(mat="Trousers", boot=0.13, hidden_above=0.37 * 1.56 + 0.06)),
                ("skirt", dict(mat="Smock", hem=0.37, flare=1.15, loose=0.02, folds=0.05, fold_count=8)),
                ("forearms", dict()),
                ("neckband", dict(mat="Smock")),
                ("boots", dict(shaft=0.13)),
                ("apron", dict(mat="ApronLeather", tie="Leather", hem=0.33)),
                ("patch", dict(mat="Leather", onto="Apron", where=lambda b: (b.pelvis + Vector((0.055, 0, -0.01))), size=(0.055, 0.003, 0.05), name="ApronPocket")),
                # The hammer hangs in a leather loop at the right hip, its head resting on the loop.
                ("hammer", dict(where=lambda b: b.pelvis + Vector((-0.085, 0, 0.055)))),
                ("lamp_cap", dict())],
    ),
}


# The radius of each carried thing's handle: the hand that carries it is closed round a bar of that radius.
HANDLES = {"lantern": 0.007, "mug": 0.0055}

# ---------------------------------------------------------------- the rest pose, for rigging

# What changes when the game moves them: things held in the hands go to the belt or the back, so the hands
# are free for work; the arms hang a little out from the body; the feet stand under the hips; the head looks ahead.
REST = {
    # Small carries the lantern in the left hand, away from the satchel at the right hip.
    "Small": dict(swap={"lantern": ("lantern", dict(hand=1))}, hold={1: "lantern"}),
    # Long carries the mug in the left hand; the pickaxe rides on the back on a strap across the chest.
    "Long": dict(swap={"pickaxe": ("sling", dict(length=0.62, head=0.7)), "mug": ("mug", dict(hand=1))}, hold={1: "mug"}),
    "Round": dict(),
}


def rest_preset(name, preset):
    p = dict(preset)
    b = dict(preset["body"])
    pose = dict(b.get("pose", {}))
    pose.update(neutral=True, weight=1, hip_shift=0.0, hip_tilt=0.0, shoulder_tilt=0.0, turn=0.0, nod=0.0, tilt=0.0)
    b["pose"] = pose

    def hang(body_, s):
        shoulder = body_.shoulders[(s + 1) // 2]
        length = body_.upper + body_.fore
        return shoulder + Vector((s * 0.42 * length, 0.03, -0.9 * length))
    b["arms"] = [dict(wrist=hang, pole=(0.3, 1, 0)), dict(wrist=hang, pole=(0.3, 1, 0))]
    p["body"] = b
    rest = REST.get(name, {})
    hands = [dict(grip="relaxed", palm=(1, 0, 0)), dict(grip="relaxed", palm=(-1, 0, 0))]
    for i, what in rest.get("hold", {}).items():
        # A hand that carries something by its handle: the arm hangs, the palm towards the body, and the fingers
        # close round the handle, which lies level from back to front.
        s = (-1, 1)[i]
        hands[i] = dict(grip="carry", ring=(0, -1, 0), bar=HANDLES[what])
    p["hands"] = hands
    outfit = []
    for piece, opts in preset["outfit"]:
        if piece in rest.get("drop", ()):
            continue
        outfit.append(rest.get("swap", {}).get(piece, (piece, opts)))
    outfit += rest.get("add", [])
    p["outfit"] = outfit
    return p


def carriage(b, objs, hang):
    """For the game: what hangs and swings (a lantern or mug in a hand, a satchel on its strap), where the body
    stops each, and how each arm carries itself (a carrying arm hangs far enough out that its load clears the
    clothes, and swings less). Directions are in the being's space as the game has it: x to its right, y up,
    z forward. hang: the relaxed arm's place (rigging.hang)."""
    def game(v):
        return [round(-v[0], 5), round(v[2], 5), round(-v[1], 5)]
    clothes = [o for o in objs if o.name.endswith(("_Skirt", "_Top", "_Trousers", "_Apron")) or "_Boot" in o.name]
    surface = shapes.Surface(*clothes)
    hips = (b.hips[0] + b.hips[1]) * 0.5
    hand_length = b.p.get("hand", 0.11) * b.H
    shoulder_out = abs(b.shoulders[1].x - b.chest.x)
    heads = {n: head for n, head, tail, parent in b.props}
    carry, keep, hanging = [0.0, 0.0], [1.0, 1.0], []
    for s in b.swings:
        entry = dict(bone=s["bone"], hand=s["hand"] or "", length=round(s["length"], 4), damping=s["damping"], limit=s["limit"])
        if s["hand"]:
            side = s["side"]  # +1: the being's left
            arm = 0 if side > 0 else 1
            # The body's reach to that side, where the thing hangs: from the hanging hand down to its lowest point.
            top = b.shoulders[1].z - hang["armDrop"] - hand_length * 0.45
            reach, far = 0.0, None
            for k in range(7):
                z = top + 0.03 - (s["length"] + s["radius"] + 0.03) * k / 6
                for y in (-0.14, -0.09, -0.04, 0.0, 0.04, 0.09):
                    hit, _ = surface.cast(Vector((side * 0.8, hips.y + y, z)), Vector((-side, 0, 0)), 0.8)
                    if hit is not None and abs(hit.x - hips.x) > reach:
                        reach, far = abs(hit.x - hips.x), hit
            clear = reach + s["radius"] + 0.012
            entry.update(aim=game((0, 0, -1)), stopNormal=game((side, 0, 0)), stopDistance=round(clear, 4), handle=game(s["ring"]))
            # The hand continues the forearm, so the grip hangs about where the wrist does: the arm hangs far
            # enough out that the thing hangs straight, a finger's width clear of where the body would stop it.
            # A carrying arm swings less (a lantern's flame, a mug's drink), and hangs far enough out that the
            # thing hangs straight all through a stride: the hand comes in towards the hips by about a tenth of
            # the arm's drop as it swings (measured on the recorded walks), and the thing stays outside the stop.
            keep[arm] = 0.45 if s["bone"] == "Lantern" else 0.5
            grip = shoulder_out + hang["armOut"]  # the hand continues the forearm: the grip hangs about where the wrist does
            sway = 0.1 * hang["armDrop"] * keep[arm] + 0.008
            carry[arm] = round(max(0.0, clear + 0.004 + sway - grip), 4)
            print(f"CARRY {b.name} {s['bone']}: the body stops it at {clear:.4f}, the relaxed grip hangs at {grip:.4f}, the stride brings it in {sway:.4f}")
            # Where the body reaches furthest is the skirt: when a leg pushes its flaps out (in a turn, a
            # sidestep), the cloth there moves out, and the thing with it.
            hem = getattr(b, "skirt_hem", None)
            if hem is not None and far is not None and hem - 0.02 < far.z < b.pelvis.z:
                whole = min(0.45, max(1e-3, (b.pelvis.z - (hem + 0.09)) / max(b.pelvis.z - hem, 1e-3)))
                share = max(0.0, min(1.0, (b.pelvis.z - far.z) / max(b.pelvis.z - hem, 1e-3) / whole))
                share = share * share * (3 - 2 * share)
                entry.update(pusher=f"SkirtFront.{'L' if side > 0 else 'R'}", pushShare=round(share, 3), pushPoint=game(far))
        else:
            stop, rest, pivot = Vector(s["stop"]), Vector(s["rest"]), heads[s["bone"]]
            entry.update(aim=game((rest - pivot).normalized()), stopNormal=game(stop), stopDistance=round((rest - hips).dot(stop), 4), handle=game(s["ring"]))
            if s.get("rides"):
                # Its loop is sewn to cloth: the game hangs it from where that cloth is (MinerSetup.Riders).
                entry["rides"] = True
            if s.get("hinged"):
                # Its head lies across the loop: it swings out from the body and back, not from side to side.
                entry["hinged"] = True
            side = 1 if rest.x > hips.x else -1
            arm = 0 if side > 0 else 1
            bag = [o for o in objs if any(k in o.name for k in ("_Bag", "_Buckle", "_StrapTab"))] if s["bone"] == "Satchel" else []
            if bag:
                # The arm on the bag's side hangs clear of the bag.
                out = max((side * (v.co.x - hips.x) for o in bag for v in o.data.vertices), default=0.0)
                cuff = b.p["arm"] * 1.2 + 0.02
                carry[arm] = max(carry[arm], round(max(0.0, out + cuff + 0.008 - (shoulder_out + hang["armOut"])), 4))
            # It rests on the skirt, and the thigh under the skirt pushes the skirt out as it swings: the skirt's
            # share of the thigh's movement at the bag's lower part (as the skirt is weighted, rigging.skin).
            hem = getattr(b, "skirt_hem", None)
            if hem is not None:
                # Where the bag's lower part rests on the skirt, and how much of that flap's movement the cloth
                # there takes (as the skirt is weighted, rigging.skin).
                low = rest - stop * 0.02 - Vector((0, 0, 0.03))
                whole = min(0.45, max(1e-3, (b.pelvis.z - (hem + 0.09)) / max(b.pelvis.z - hem, 1e-3)))
                share = max(0.0, min(1.0, (b.pelvis.z - low.z) / max(b.pelvis.z - hem, 1e-3) / whole))
                share = share * share * (3 - 2 * share)
                front = low.y < b.pelvis.y
                entry.update(pusher=f"Skirt{'Front' if front else 'Back'}.{'L' if side > 0 else 'R'}", pushShare=round(share, 3),
                             pushPoint=game(low))
        hanging.append(entry)
    print(f"CARRIAGE {b.name}: arms further out {carry}, swing kept {keep}, hanging {[(h['bone'], h['stopDistance']) for h in hanging]}")
    # How far a thigh swings before it reaches the skirt's front and back flaps, in degrees (outfits.skirt_slack,
    # less a margin). A long coat hangs well clear of the legs, and only the end of each stride moves it. A short
    # closed skirt moves with the legs from the first: that is the movement Luis liked, kept as it is.
    measured = getattr(b, "skirt_slack", [0.0, 0.0])
    slack = [round(max(0.0, s - 3.0) * 0.8, 2) for s in measured] if getattr(b, "skirt_open", False) else [0.0, 0.0]
    print(f"SKIRT {b.name}: the legs swing {measured} degrees before they reach the cloth; the flaps wait {slack}")
    return dict(armCarry=carry, armSwingSide=keep, hanging=hanging, skirtSlack=slack, **shape(b, objs))


def shape(b, objs):
    """For a tool swung in front of the body: how far the body's front stands ahead of the hips between the
    hips and the shoulders (its clothes and whatever it wears there: an apron's pocket, a hammer, a strap), how
    far its face does, and its head's half-width (hair and cap included). Measured on the model at rest."""
    hips = (b.hips[0] + b.hips[1]) * 0.5
    out = abs(b.shoulders[1].x - hips.x)
    top = b.shoulders[1].z
    front, face, half = 0.0, 0.0, 0.0
    for o in objs:
        if o.type != 'MESH':
            continue
        zs = [v.co.z for v in o.data.vertices]
        if not zs:
            continue
        head = "_Skin" in o.name or sum(zs) / len(zs) > b.collar.z + 0.02
        for v in o.data.vertices:
            ahead, aside = hips.y - v.co.y, abs(v.co.x - hips.x)
            if head and v.co.z > b.collar.z + 0.01:
                face, half = max(face, ahead), max(half, aside)
            elif hips.z - 0.05 <= v.co.z <= top + 0.02 and aside < out * 0.8:
                front = max(front, ahead)
    print(f"SHAPE {b.name}: its front stands {front * 1000:.0f} mm ahead of the hips, its face {face * 1000:.0f} mm; its head is {half * 2000:.0f} mm wide")
    return dict(bodyFront=round(front, 4), faceFront=round(face, 4), headHalf=round(half, 4))


def build_rigged(out_dir, only=None, dims_only=False, poses=None, audit_only=False, report=None, sweep_only=False, hands_only=False,
                 weigh_only=False):
    """The miners for the game: rest pose, skeleton, skin, levels of detail, one atlas each, and their dimensions.
    dims_only: only the dimensions (miners.json), without remaking the models.
    Every build is audited (audit.py): at rest, and on the game's recorded frames when poses (a folder of
    Miner_<Name>_poses.json) is given. audit_only: the audit alone, without baking or exporting.
    report: where the audit's reports go. A build's go to Art/Review/Miners, never among the game's assets.
    sweep_only: the sweep of poses beyond the game's own movement (sweep.py), without baking or exporting.
    hands_only: the closing of the free hands round handles (hands.py), with a picture of each, without baking
    or exporting.
    weigh_only: what each part of the body weighs (weights.py), written into miners.json beside what is there,
    without baking or exporting."""
    import audit
    import hands
    import weights
    if sweep_only or hands_only:
        audit_only = True
    if report is None:
        report = out_dir if audit_only else os.path.normpath(os.path.join(HERE, "..", "..", "Review", "Miners"))
    os.makedirs(report, exist_ok=True)
    dims = {}
    partial = set()  # measured again without remaking the model: what only a full build finds (the hands' closing) is kept
    for name, preset in PRESETS.items():
        if only and name not in only:
            continue
        bpy.ops.wm.read_factory_settings(use_empty=True)
        rp = rest_preset(name, preset)
        b, objs = build_character(name, rp, out_dir)
        face_images = {rp["face"]["skin"]: bpy.data.images[f"Face_{name}"]}
        recipes = {n: recipe_for(n) for n, m in MATERIALS.items() if m["kind"] == "painted"}
        for n, r in recipes.items():
            import painting
            painting.recipe(n, **r)
        gait = dict(rp.get("gait", {}))
        if not audit_only:
            gait.update(carriage(b, objs, rigging.hang(b, b.upper, b.fore)))
        if dims_only:
            dims[name] = dict(rigging.dimensions(b, rigging.joints(b)), **gait)
            partial.add(name)
            continue
        recorded = os.path.join(poses, f"Miner_{name}_poses.json") if poses else None
        if recorded and not os.path.exists(recorded):
            recorded = None

        closed = {}
        weighed = {}
        # This body's own pickaxe: how thick its handle is where the hands grip it.
        import tools
        own = tools.handle_radii(name, b, rigging.joints(b))

        def check(b_, meshes, bones):
            # At work the game puts this body's own pickaxe in its hands: the audit holds it too, on the frames
            # recorded while mining.
            working = []
            if recorded:
                import tools
                working = [o for o in tools.pickaxe(name, tools.measured_on(b_, bones))[0] if o.type == 'MESH']
                for o in working:
                    o.name = f"{b_.name}_Work{o.name.split('_')[-1]}"
                    o.vertex_groups.new(name="Tool").add([v.index for v in o.data.vertices], 1.0, 'REPLACE')
            audit.run(b_, meshes, bones, report, recorded, tool=working)
            # The free hands closed round handles of several thicknesses: checked here, on the parts as skinned.
            closed.update(hands.tables(b_, meshes, own))
            hands.report(b_, closed, report, pictures=False)
            # What each part weighs, with what it wears: measured here too, on the parts before they are joined.
            measured, pieces = weights.body(b_, meshes, bones)
            weights.report(b_, measured, pieces, report)
            weighed.update(measured)
        if weigh_only:
            _, bones, meshes = rigging.prepare(name, b, objs)
            measured, pieces = weights.body(b, meshes, bones)
            weights.report(b, measured, pieces, report)
            dims[name] = dict(weights=measured)
            partial.add(name)
            continue
        if hands_only:
            _, bones, meshes = rigging.prepare(name, b, objs)
            hands.report(b, hands.tables(b, meshes, own), report)
            continue
        if sweep_only:
            import sweep
            _, bones, meshes = rigging.prepare(name, b, objs)
            sweep.run(b, meshes, bones, report)
            continue
        if audit_only:
            _, bones, meshes = rigging.prepare(name, b, objs)
            check(b, meshes, bones)
            continue
        dims[name], _ = rigging.build(name, b, objs, MATERIALS, recipes, face_images, out_dir, audit=check)
        dims[name].update(gait)
        dims[name]["grips"] = hands.export(b, closed)
        dims[name]["weights"] = dict(weighed)
    path = os.path.join(out_dir, "miners.json")
    previous = {}
    if audit_only:
        return
    # Several builds may finish together, one for each miner: one at a time reads the file, adds its own, writes.
    import time
    lock = path + ".lock"
    for _ in range(1200):
        try:
            os.mkdir(lock)
            break
        except FileExistsError:
            time.sleep(0.1)
    try:
        if os.path.exists(path):
            with open(path, encoding="utf-8") as f:
                previous = {m["name"]: m for m in json.load(f).get("miners", [])}
        for name, d in dims.items():
            previous[name] = dict(previous.get(name, {}) if name in partial else {}, name=name, **d)
        with open(path, "w", encoding="utf-8") as f:
            json.dump(dict(miners=[previous[n] for n in PRESETS if n in previous]), f, indent=2)
    finally:
        if os.path.isdir(lock):
            os.rmdir(lock)


# ---------------------------------------------------------------- building

def build_character(name, preset, out_dir):
    outfits._TREES.clear()
    b = body.Body(f"Worker_{name}", preset["body"])
    b.props = []  # rigid things with bones of their own: (bone, head, tail, parent bone)
    b.contacts = []  # points that must stay on what they rest on, for the audit (audit.py)
    b.swings = []  # carried things that swing in the game: (bone, the hand that carries it, length, ...)
    face = preset["face"]
    skin_name = face["skin"]
    image = faces.paint(face["style"], MATERIALS[skin_name]["colour"], preset["seed"], os.path.join(out_dir, f"Face_{name}.png"),
                        f"Face_{name}", face.get("soot", ()))
    skin = shapes.material(skin_name, image=image)
    objs = [body.head(b, skin)]
    held = []
    for i, h in enumerate(preset["hands"]):
        obj, grasp, axes = body.hand(b, i, skin, h["grip"], h.get("palm"), h.get("along"), h.get("ring"), h.get("bar"))
        objs.append(obj)
        held.append((grasp, axes))
    hstyle = preset["hair"]
    objs.append(hair.grow(b, hstyle["style"], hstyle["mat"], preset["seed"], **{k: v for k, v in hstyle.items() if k not in ("style", "mat")}))
    for piece, opts in preset["outfit"]:
        objs.extend(dress(b, piece, dict(opts), held, skin))
    for probe in [x for x in bpy.data.objects if x.name.endswith("Probe")]:
        bpy.data.objects.remove(probe)
    return b, objs


def dress(b, piece, o, held, skin):
    if piece == "top":
        return outfits.top(b, o.pop("mat"), **o)
    if piece == "skirt":
        top = next((x for x in bpy.data.objects if x.name == f"{b.name}_TopProbe"), None)
        under = [x for x in bpy.data.objects if x.name == f"{b.name}_Trousers"]
        return outfits.skirt(b, o.pop("mat"), fit=top, under=under, **o)
    if piece == "trousers":
        return outfits.trousers(b, o.pop("mat"), **o)
    if piece == "sling":
        # The pick on the back, on a strap that lies over everything on the chest: the coat, its lapels, the shirt.
        front = [x for x in bpy.data.objects if x.name.startswith(f"{b.name}_Lapel") or x.name == f"{b.name}_ShirtFront"]
        torso = [x for x in bpy.data.objects if x.name == f"{b.name}_TopProbe"] + front
        below = [x for x in bpy.data.objects if x.name == f"{b.name}_Skirt"]
        clothes = [x for x in bpy.data.objects if x.name in (f"{b.name}_Top", f"{b.name}_Collar")] + front
        return outfits.sling(b, "Leather", "Wood", "Iron", torso, below, clothes, **o)
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
        onto = [x for x in bpy.data.objects if x.name in (f"{b.name}_Top", f"{b.name}_Skirt")]
        return outfits.buttons(b, o.pop("mat"), onto=onto, **o)
    if piece == "patch":
        onto = next((x for x in bpy.data.objects if x.name == f"{b.name}_{o['onto']}"), None) if "onto" in o else None
        return outfits.patch(b, o["mat"], o["where"](b), o["size"], o.get("angle", 0.0), o.get("name", f"Patch{id(o) % 997}"), onto)
    if piece == "apron":
        under = [x for x in bpy.data.objects if x.name in (f"{b.name}_TopProbe", f"{b.name}_Skirt")]
        # The neck strap lies on the smock's shoulders and on the back of the neck itself.
        neck = [x for x in bpy.data.objects if x.name in (f"{b.name}_Top", f"{b.name}_Neckband", f"{b.name}_Collar", f"{b.name}_Skin")]
        return outfits.apron(b, o["mat"], o["tie"], hem=o.get("hem", 0.33), over=under, neck=neck)
    if piece == "boots":
        return body.boot(b, 0, "Boots", "Sole", **o) + body.boot(b, 1, "Boots", "Sole", **o)
    if piece == "forearms":
        return [body.forearm(b, i, skin) for i in (0, 1)]
    if piece == "satchel":
        torso = [x for x in bpy.data.objects if x.name == f"{b.name}_TopProbe"]
        below = [x for x in bpy.data.objects if x.name == f"{b.name}_Skirt"]
        clothes = [x for x in bpy.data.objects if x.name in (f"{b.name}_Top", f"{b.name}_Collar")]
        return outfits.satchel(b, "Leather", "Accent", "Brass", torso, below, clothes, **o)
    if piece == "lantern":
        grasp, (a, t, n) = held[o["hand"]]
        # Carried by its handle inside the closed fingers; it has a bone of its own at the handle, so it can swing.
        hand_bone = f"Hand.{rigging.side_of(o['hand'])}"
        b.props.append(("Lantern", grasp.copy(), grasp - Vector((0, 0, 0.12)), hand_bone))
        b.swings.append(dict(bone="Lantern", hand=hand_bone, length=0.12, radius=0.04, damping=0.9, limit=55, side=(-1, 1)[o["hand"]], ring=tuple(t)))
        return outfits.lantern(b, grasp, t, "Brass", "Glass", "Wood", grip=HANDLES["lantern"], span=min(0.095, b.p.get("hand", 0.11) * b.H * 0.62))
    if piece == "pickaxe":
        # The handle runs through the fist, along its grip; Long leans on it like a walking stick.
        grasp, (a, t, n) = held[o["hand"]]
        d = t if t.z > 0 else -t
        hold = o.get("hold", 0.9)
        return outfits.pickaxe(b, grasp, d, "Wood", "Iron", length=(grasp.z - 0.01) / max(d.z, 0.3) / hold, hold=hold)
    if piece == "mug":
        grasp, (a, t, n) = held[o["hand"]]
        # Carried by its handle inside the closed fingers, hanging on its side below the hand; a bone of its own.
        hand_bone = f"Hand.{rigging.side_of(o['hand'])}"
        b.props.append(("Mug", grasp.copy(), grasp - Vector((0, 0, 0.08)), hand_bone))
        b.swings.append(dict(bone="Mug", hand=hand_bone, length=0.075, radius=0.05, damping=0.88, limit=50, side=(-1, 1)[o["hand"]], ring=tuple(t)))
        hand_length = b.p.get("hand", 0.11) * b.H
        # Room between the handle and the mug's wall for the fingers that pass through.
        return outfits.mug(b, grasp, t, "Mug", grip=HANDLES["mug"], span=hand_length * 0.5, clear=HANDLES["mug"] + hand_length * 0.155)
    if piece == "hammer":
        onto = [x for x in bpy.data.objects if x.name == f"{b.name}_Apron"] or [x for x in bpy.data.objects if x.name == f"{b.name}_Skirt"]
        parts, pivot, tip, normal, across = outfits.hammer(b, onto, o["where"](b), "Wood", "Iron", "Leather")
        # It hangs in its loop from a bone of its own, and swings a little; the apron under it stops it.
        b.props.append(("Hammer", pivot.copy(), tip.copy(), "Pelvis"))
        # The stop is a flat place on a round body: a little further out than modelled, so the handle's whole
        # length stays clear of the cloth as the leg under it moves.
        b.swings.append(dict(bone="Hammer", hand=None, length=(tip - pivot).length, radius=0.0, damping=0.84, limit=30,
                             side=-1, ring=tuple(across), stop=tuple(normal), rest=tuple(tip + normal * 0.008), rides=True, hinged=True))
        return parts
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
    parser.add_argument("--rigged", action="store_true", help="the game-ready miners: rest pose, rig, LODs, atlas")
    parser.add_argument("--dims", action="store_true", help="with --rigged: only rewrite miners.json")
    parser.add_argument("--poses", help="with --rigged or --audit: a folder of the game's recorded frames (MinerPoseRecord)")
    parser.add_argument("--audit", action="store_true", help="only the model audit (rest pose, skinned and simplified), no export")
    parser.add_argument("--report", help="where the audit's reports go (default: --out for --audit; Art/Review/Miners for a build)")
    parser.add_argument("--sweep", action="store_true", help="only the sweep of extreme poses (sweep.py): its report and a picture of each pose")
    parser.add_argument("--hands", action="store_true", help="only the closing of the free hands round handles (hands.py): its report and pictures")
    parser.add_argument("--weigh", action="store_true", help="only what each part of the body weighs (weights.py): its report, and miners.json")
    args = parser.parse_args(argv)
    os.makedirs(args.out, exist_ok=True)
    if args.rigged or args.audit or args.sweep or args.hands or args.weigh:
        build_rigged(args.out, args.only, args.dims, args.poses, audit_only=args.audit, report=args.report, sweep_only=args.sweep,
                     hands_only=args.hands, weigh_only=args.weigh)
        sys.exit(0)
    built = build(args.out, args.only)
    painted = paint(built, args.out) if args.paint else {}
    if args.fbx:
        manifest(args.out, painted)
        export_fbx(os.path.join(args.out, "Workers.fbx"))
    if args.preview:
        preview(built, args.preview)
