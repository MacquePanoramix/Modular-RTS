import sys
sys.stdout.reconfigure(encoding='utf-8')
import numpy as np

rng = np.random.default_rng(12345)
D2R = np.pi / 180

def qmul(a, b):
    # Hamilton product, (x,y,z,w); same as Unity q1*q2 (q2 applied first). Broadcasts over leading dims.
    ax, ay, az, aw = np.moveaxis(a, -1, 0)
    bx, by, bz, bw = np.moveaxis(b, -1, 0)
    return np.stack([aw*bx + ax*bw + ay*bz - az*by,
                     aw*by - ax*bz + ay*bw + az*bx,
                     aw*bz + ax*by - ay*bx + az*bw,
                     aw*bw - ax*bx - ay*by - az*bz], -1)

def qinv(q):
    return q * np.array([-1, -1, -1, 1.0])

def angle_axis(deg, axis):
    axis = axis / np.linalg.norm(axis, axis=-1, keepdims=True)
    h = np.asarray(deg)[..., None] * D2R / 2
    return np.concatenate([axis * np.sin(h), np.cos(h)], -1)

def from_rv(v):
    # FromRotationVector (degrees)
    v = np.asarray(v, float)
    m = np.linalg.norm(v, axis=-1, keepdims=True)
    ax = np.where(m > 1e-6, v / np.where(m > 1e-6, m, 1), np.array([1.0, 0, 0]))
    ang = np.where(m[..., 0] > 1e-6, m[..., 0], 0.0)
    return angle_axis(ang, ax)

def rv(q):
    # RotationVector: Unity ToAngleAxis gives [0,360], then folded into (-180,180]
    q = q / np.linalg.norm(q, axis=-1, keepdims=True)
    w = np.clip(q[..., 3], -1, 1)
    ang = 2 * np.arccos(w) / D2R          # in [0,360]
    s = np.sqrt(np.maximum(1 - w * w, 0))
    ax = np.where(s[..., None] > 1e-9, q[..., :3] / np.where(s[..., None] > 1e-9, s[..., None], 1), np.array([1.0, 0, 0]))
    ang = np.where(ang > 180, ang - 360, ang)
    ang = np.where(s > 1e-9, ang, 0.0)
    return ang[..., None] * ax

def turn_between(a, b):
    # angle (degrees) of the rotation taking one to the other (sign-blind)
    a = a / np.linalg.norm(a, axis=-1, keepdims=True)
    b = b / np.linalg.norm(b, axis=-1, keepdims=True)
    d = np.abs(np.sum(a * b, -1))
    return 2 * np.arccos(np.clip(d, -1, 1)) / D2R

def A(own, d):  # taught sum
    return from_rv(rv(own) + d)
def B(own, d):
    return qmul(own, from_rv(d))
def C(own, d):
    return qmul(from_rv(d), own)

print("# 1. continuity across own's angle = 180 deg")
eps = 1e-3
worst = {'a': 0, 'b': 0, 'c': 0}
worst_vec = {'a': 0, 'b': 0, 'c': 0}
ds = [np.array(x, float) for x in [(0, 0, 0), (20, 0, 0), (0, 20, 0), (0, 0, 20), (20, 20, 20), (-20, 20, -20), (5, -10, 15), (10, 0, 0)]]
axes = [np.array(x, float) for x in [(1, 0, 0), (0, 1, 0), (0, 0, 1), (1, 1, 0), (1, 1, 1), (1, -2, 3), (0.3, 0.9, -0.2)]]
rng_axes = rng.normal(size=(200, 3))
axes += list(rng_axes)
for ax in axes:
    lo = angle_axis(np.array(180 - eps), ax)
    hi = angle_axis(np.array(180 + eps), ax)
    for d in ds:
        for name, f in (('a', A), ('b', B), ('c', C)):
            ql, qh = f(lo, d), f(hi, d)
            j = float(turn_between(ql, qh))
            vj = float(np.linalg.norm(rv(ql) - rv(qh)))
            worst[name] = max(worst[name], j)
            worst_vec[name] = max(worst_vec[name], vj)
print("two samples are own at 180-1e-3 and 180+1e-3 deg; own itself differs by %.4f deg" % (2 * eps))
for k in 'abc':
    print(" (%s) largest rotation jump found: %.5f deg   (largest jump of RotationVector of the result: %.3f deg-vector)" % (k, worst[k], worst_vec[k]))
# a few explicit rows for (a): per d
print(" (a) jump by d (axis x, y, z rows, plus perpendicular d):")
for ax in [np.array([1., 0, 0]), np.array([0, 1., 0]), np.array([0, 0, 1.])]:
    row = []
    for d in [np.array([0, 0, 0.]), np.array([20., 0, 0]), np.array([0, 20., 0]), np.array([0, 0, 20.]), np.array([20., 20, 20])]:
        row.append("%.2f" % float(turn_between(A(angle_axis(np.array(180 - eps), ax), d), A(angle_axis(np.array(180 + eps), ax), d))))
    print("   axis", ax, row)
# analytic: own=180 about a, d perp a: R(180a+d) vs R(-180a+d)
print(" (a) jump vs |d| perpendicular to axis (degrees):")
for m in [1, 5, 10, 20, 28.28]:
    ax = np.array([1., 0, 0]); d = np.array([0, m, 0.])
    print("   |d|=%5.2f  jump=%.3f" % (m, float(turn_between(A(angle_axis(np.array(180 - eps), ax), d), A(angle_axis(np.array(180 + eps), ax), d)))))

print()
print("# 2. distance from the taught sum (a), by angle of own (15-degree bands), d uniform in box +-20 per axis")
N = 400000
th = rng.uniform(0, 180, N)
axs = rng.normal(size=(N, 3))
own = angle_axis(th, axs)
d = rng.uniform(-20, 20, size=(N, 3))
qa, qb, qc = A(own, d), B(own, d), C(own, d)
eb = turn_between(qa, qb)
ec = turn_between(qa, qc)
ebc = turn_between(qb, qc)
dmag = np.linalg.norm(d, axis=1)
print("samples N=%d, median |d| = %.1f deg, max %.1f" % (N, np.median(dmag), dmag.max()))
print("band      | (b)-(a) median  p95   worst | (c)-(a) median  p95   worst | (b)-(c) median  worst")
for lo in range(0, 180, 15):
    m = (th >= lo) & (th < lo + 15)
    f = lambda e: (np.median(e[m]), np.percentile(e[m], 95), e[m].max())
    b, c = f(eb), f(ec)
    print("%3d-%3d   |   %6.2f %6.2f %6.2f |   %6.2f %6.2f %6.2f |   %6.2f %6.2f" % (lo, lo + 15, *b, *c, np.median(ebc[m]), ebc[m].max()))
print("all: (b)-(a) median %.2f worst %.2f ; (c)-(a) median %.2f worst %.2f" % (np.median(eb), eb.max(), np.median(ec), ec.max()))
print("fraction where (b) is nearer (a) than (c): by band")
for lo in range(0, 180, 15):
    m = (th >= lo) & (th < lo + 15)
    print("  %3d-%3d: %.3f" % (lo, lo + 15, np.mean(eb[m] < ec[m])))

print()
print("# 2b. same, but ratio to |d| (turn-between / |d|) : median per band, (b) and (c)")
for lo in range(0, 180, 15):
    m = (th >= lo) & (th < lo + 15)
    print("  %3d-%3d: (b) %.3f  (c) %.3f" % (lo, lo + 15, np.median(eb[m] / dmag[m]), np.median(ec[m] / dmag[m])))

print()
print("# 3. first order in d: error of (b),(c) against (a) for d scaled by s (own fixed); linear in s means NOT equal to first order")
for thv in [0.0, 10, 30, 60, 90, 120, 150, 170]:
    ax = np.array([0.3, 0.8, -0.5])
    ow = angle_axis(np.array(thv), ax)
    dd = np.array([12., -7, 15])
    out = []
    for s in [1.0, 0.1, 0.01, 0.001]:
        out.append((float(turn_between(A(ow, s * dd), B(ow, s * dd))), float(turn_between(A(ow, s * dd), C(ow, s * dd)))))
    r_b = out[2][0] / out[3][0] if out[3][0] > 0 else float('nan')
    r_c = out[2][1] / out[3][1] if out[3][1] > 0 else float('nan')
    print(" own=%5.1f deg: (b)-(a) at s=1,.1,.01,.001: %s ; scaling 0.01->0.001 ratio %.2f (10 = linear)  | (c)-(a): %s ratio %.2f" %
          (thv, ["%.4g" % o[0] for o in out], r_b, ["%.4g" % o[1] for o in out], r_c))

# analytic leading-order check: angle(b,a) ~ (theta/2)*|a x d| (rad factors cancel in degrees)
print()
print("# 3b. leading-order check at small own angle theta: error ~ (theta/2 in radians) * |axis x d|")
for thv in [2, 5, 10, 20, 40]:
    ax = rng.normal(size=3); ax /= np.linalg.norm(ax)
    ow = angle_axis(np.array(float(thv)), ax)
    dd = np.array([0.01, -0.02, 0.015]) * 1.0  # tiny d in degrees
    pred = (thv * D2R / 2) * np.linalg.norm(np.cross(ax, dd))
    eb1 = float(turn_between(A(ow, dd), B(ow, dd)))
    ec1 = float(turn_between(A(ow, dd), C(ow, dd)))
    print("  theta=%2d: (b)-(a) %.5g  (c)-(a) %.5g  predicted %.5g  (units deg)" % (thv, eb1, ec1, pred))

print()
print("# 3c. exact first-order Jacobian: (a)=own*exp(Jr d); how far is Jr from I (spectral), per angle")
def skew(v): return np.array([[0, -v[2], v[1]], [v[2], 0, -v[0]], [-v[1], v[0], 0]])
for thv in [0, 30, 90, 150, 179, 180]:
    t = thv * D2R
    k = skew(np.array([1., 0, 0])) * t
    if t < 1e-9: Jr = np.eye(3)
    else: Jr = np.eye(3) - (1 - np.cos(t)) / t**2 * k + (t - np.sin(t)) / t**3 * k @ k
    # (c) = own * exp(R^T d): R^T = exp(-k)
    Rt = np.eye(3) - np.sin(t) * skew(np.array([1., 0, 0])) + (1 - np.cos(t)) * skew(np.array([1., 0, 0])) @ skew(np.array([1., 0, 0]))
    print("  theta=%3d: |Jr - I|_2 = %.3f  |R^T - Jr|_2 = %.3f  |R^T - I|_2 = %.3f ; (b) pulls d unchanged: I ; (c) by R^T" % (thv, np.linalg.norm(Jr - np.eye(3), 2), np.linalg.norm(Rt - Jr, 2), np.linalg.norm(Rt - np.eye(3), 2)))
