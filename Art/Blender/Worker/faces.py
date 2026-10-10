"""Wonder Gather — painted faces (S1d).

A face is a few brush strokes painted into a texture: eyes, brows, a mouth, a
blush, and for a miner a smudge of soot. Feeling from very few marks. The
texture is projected onto the head from the front (body.head), so positions
here are in head radii from the head's centre: x to the being's left, z up.

Each face is a function of a Canvas; a character creator can offer them, and
their parameters, as choices.
"""
import math
import random

import bpy
import numpy as np
from mathutils import Vector

SIZE = 1024
INK = (0.16, 0.09, 0.075)
SOOT = (0.22, 0.19, 0.18)


class Canvas:
    """A small painter: soft round brush stamps along curves. Rows run bottom to top."""

    def __init__(self, colour, seed, span=1.25):
        self.n = SIZE
        self.span = span
        self.rgb = np.empty((self.n, self.n, 3), np.float32)
        self.rgb[:] = colour
        self.rng = random.Random(seed)

    def px(self, x, z):
        return (0.5 + x / (2 * self.span)) * self.n, (0.5 + z / (2 * self.span)) * self.n

    def scale(self, w):
        return w / (2 * self.span) * self.n

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
            fx = ((xs + 0.5) / self.n - 0.5) * 2 * self.span
            fz = ((ys + 0.5) / self.n - 0.5) * 2 * self.span
            a = a * inside(fx, fz)
        mask = np.zeros((self.n, self.n), np.float32)
        mask[y0:y1, x0:x1] = a
        self._composite(mask, colour, opacity)

    def glow(self, centre, radius, colour, opacity, squash=1.0, rough=0.0):
        """A soft wash, such as a blush or a smudge of soot; rough breaks its edge."""
        cx, cy = self.px(*centre)
        r = self.scale(radius)
        ys, xs = np.mgrid[0:self.n, 0:self.n]
        d = np.hypot(xs + 0.5 - cx, (ys + 0.5 - cy) / squash) / r
        if rough:
            k = self.rng.uniform(0, 6)
            d = d * (1 + rough * (np.sin(xs * 0.045 + k) * np.sin(ys * 0.06 - k) + 0.5 * np.sin(xs * 0.13 + ys * 0.09 + k)))
        a = np.clip(1 - d, 0, 1) ** 1.6
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


def soot(c, marks):
    """A miner's smudges: a soft, broken wash of coal dust."""
    for centre, radius, strength, squash in marks:
        c.glow(centre, radius, SOOT, strength, squash=squash, rough=0.35)


def laughing(c, soot_marks=()):
    """Closed, happy eyes, big rosy cheeks, a wide smile."""
    for s in (-1, 1):
        c.glow((s * 0.52, -0.33), 0.3, (0.90, 0.42, 0.38), 0.55)
    soot(c, soot_marks)
    for s in (-1, 1):
        ex, ez = s * 0.36, -0.02
        c.stroke(arc(ex, ez, 0.11, 0.07), 0.05)
        c.stroke([(ex + s * 0.1, ez - 0.01), (ex + s * 0.15, ez + 0.02)], 0.025, opacity=0.8)
        c.stroke(arc(ex - s * 0.01, ez + 0.24, 0.1, 0.035, tilt=s * 0.015), 0.055, (0.17, 0.09, 0.06))
        c.stroke([(s * 0.25, -0.47), (s * 0.27, -0.55)], 0.02, opacity=0.35)
    c.ellipse((0, -0.6), (0.15, 0.065), (0.50, 0.17, 0.15), inside=lambda x, z: (z < -0.58 + 0.25 * (x / 0.2) ** 2).astype(np.float32))
    c.stroke(arc(0, -0.55, 0.21, -0.07), 0.035)


def sleepy(c, soot_marks=()):
    """Heavy lids over small dark eyes, long brows sloping down, a crooked smile."""
    for s in (-1, 1):
        c.glow((s * 0.45, -0.3), 0.24, (0.85, 0.48, 0.42), 0.25)
    soot(c, soot_marks)
    for s in (-1, 1):
        ex, ez = s * 0.30, 0.02
        lid = lambda x, z, ez=ez: (z < ez + 0.012).astype(np.float32)
        c.ellipse((ex + 0.012, ez - 0.012), (0.052, 0.05), INK, inside=lid)
        c.stroke([(ex - s * 0.11, ez + 0.005), (ex - s * 0.03, ez + 0.03), (ex + s * 0.06, ez + 0.03), (ex + s * 0.125, ez - 0.01)], 0.045)
        c.stroke([(ex - 0.07, ez - 0.085), (ex + 0.07, ez - 0.09)], 0.015, opacity=0.35)
        c.stroke([(s * 0.12, 0.22), (s * 0.28, 0.235), (s * 0.45, 0.16)], 0.036, (0.14, 0.08, 0.06))
    c.stroke([(-0.17, -0.63), (0.0, -0.655), (0.15, -0.615), (0.2, -0.56)], 0.026)
    c.stroke([(0.21, -0.54), (0.235, -0.6)], 0.015, opacity=0.4)


def curious(c, soot_marks=()):
    """Big dark eyes glancing aside, a soft blush with three small strokes, a small parted mouth."""
    for s in (-1, 1):
        c.glow((s * 0.48, -0.4), 0.2, (0.95, 0.52, 0.50), 0.45)
        for k in range(3):
            x = s * (0.42 + 0.06 * k)
            c.stroke([(x, -0.36), (x - 0.03, -0.43)], 0.012, (0.8, 0.35, 0.33), opacity=0.45, jitter=0.002)
    soot(c, soot_marks)
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


STYLES = {"laughing": laughing, "sleepy": sleepy, "curious": curious}


def paint(style, skin_colour, seed, path, name, soot_marks=(), span=1.25):
    canvas = Canvas(skin_colour, seed, span)
    STYLES[style](canvas, soot_marks)
    return canvas.save(path, name)
