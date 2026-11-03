# A 3D quasi-static check of layers 1 and 2 (stances fitted to a body, reserve by LP, feasible region).
# Not the game's code: a reference for the C# design in stances_strategy.md. Bodies are the three miners as their
# prefabs and weights reports give them (OwnBody's link grouping, physics centres at the collider centres, PhysicalBody's
# capacities at Strength 1), and invented variants.
import math, json, copy
import numpy as np
from scipy.optimize import linprog, least_squares

G = 9.81
D2R = math.pi / 180.0
Z = np.array([0.0, 0.0, 1.0])


def Rx(a):
    c, s = math.cos(a), math.sin(a)
    return np.array([[1.0, 0, 0], [0, c, -s], [0, s, c]])


def Ry(a):
    c, s = math.cos(a), math.sin(a)
    return np.array([[c, 0, s], [0, 1.0, 0], [-s, 0, c]])


def Rz(a):
    c, s = math.cos(a), math.sin(a)
    return np.array([[c, -s, 0], [s, c, 0], [0, 0, 1.0]])


VARS = ['x', 'y', 'z', 'yaw', 'pitch', 'roll', 'tflex', 'tlat', 'ttw', 'nflex', 'nlat', 'nrot',
        'shflexL', 'shabdL', 'shrotL', 'elbL', 'shflexR', 'shabdR', 'shrotR', 'elbR',
        'hflexL', 'habdL', 'hrotL', 'kneeL', 'hflexR', 'habdR', 'hrotR', 'kneeR',
        'dorsiL', 'invL', 'dorsiR', 'invR', 'toeL', 'toeR']
VI = {n: i for i, n in enumerate(VARS)}
NQ = len(VARS)

RANGES = dict(yaw=(-180, 180), pitch=(-180, 180), roll=(-180, 180),
              tflex=(-20, 75), tlat=(-25, 25), ttw=(-30, 30),
              nflex=(-40, 45), nlat=(-30, 30), nrot=(-50, 50),
              shflex=(-50, 170), shabd=(-10, 120), shrot=(-70, 70), elb=(-2, 145),
              hflex=(-18, 120), habd=(-20, 45), hrot=(-35, 40), knee=(-2, 150),
              dorsi=(-50, 30), inv=(-20, 20), toe=(-20, 80))


def rkey(n):
    for k in ('shflex', 'shabd', 'shrot', 'elb', 'hflex', 'habd', 'hrot', 'knee', 'dorsi', 'inv', 'toe'):
        if n.startswith(k):
            return k
    return n


# ---------------------------------------------------------------- bodies
MINERS = {
    'Small': dict(H=0.605714, wH=0.068, leg=0.27234, a=0.085, hl=0.07242, bl=0.08449, tl=0.055522, hW=0.1904, tR=0.10295,
                  headRise=0.3805, sx=0.145, sy=0.2059, sz=-0.01, lu=0.2343, lf=0.1988, bodyFront=0.1452, headHalf=0.163,
                  ra=0.033, rl=0.041, height=1.42,
                  m=dict(hips=7.88, trunk=8.96, head=9.96, ua=(1.12, 1.11), fa=(0.80, 0.80), th=(2.38, 2.38), sh=(1.35, 1.35),
                         ft=(1.07, 1.07), to=(0.21, 0.21)),
                  cap=dict(back=257, neck=51, shoulder=42, elbow=36, hip=130, knee=87, ankle=61, toe=18)),
    'Long': dict(H=0.9257484, wH=0.082, leg=0.439722, a=0.085, hl=0.08649, bl=0.100905, tl=0.066309, hW=0.1967, tR=0.14415,
                 headRise=0.49010634, sx=0.165, sy=0.2883, sz=0.032, lu=0.3255, lf=0.279, bodyFront=0.1706, headHalf=0.1329,
                 ra=0.036, rl=0.046, height=1.86,
                 m=dict(hips=9.68, trunk=17.04, head=6.48, ua=(1.82, 1.82), fa=(1.38, 1.38), th=(4.87, 4.87), sh=(2.74, 2.74),
                        ft=(1.52, 1.52), to=(0.30, 0.30)),
                 cap=dict(back=347, neck=38, shoulder=76, elbow=65, hip=303, knee=202, ankle=141, toe=42)),
    'Round': dict(H=0.6975703, wH=0.095, leg=0.320382, a=0.085, hl=0.07956, bl=0.09282, tl=0.060996, hW=0.2072, tR=0.1443,
                  headRise=0.3875711, sx=0.19, sy=0.2886, sz=-0.035, lu=0.2496, lf=0.2184, bodyFront=0.1963, headHalf=0.1976,
                  ra=0.045, rl=0.058, height=1.56,
                  m=dict(hips=23.63, trunk=26.20, head=9.82, ua=(2.17, 2.17), fa=(1.53, 1.53), th=(5.60, 5.60), sh=(3.14, 3.14),
                         ft=(1.30, 1.30), to=(0.26, 0.26)),
                  cap=dict(back=462, neck=29, shoulder=85, elbow=74, hip=331, knee=220, ankle=154, toe=46)),
}


def make_body(src, name=None, strength=1.0, scale=1.0, cap_mul=None, side_mul=None, ranges=None, mass_mul=None,
              len_mul=None, cap_rule='present'):
    """A body from a miner's numbers. scale: all lengths x scale, masses x scale^3. cap_rule 'present': PhysicalBody's
    rules recomputed for the changed body (capacity follows need); 'cube': the base body's capacities x scale^3 (strength
    by cross-section times lever: a body twice as tall works twice as near its limit)."""
    s0 = copy.deepcopy(MINERS[src])
    b = {}
    k = scale
    lm = dict(leg=1, arm=1, trunk=1)
    if len_mul:
        lm.update(len_mul)
    b['name'] = name or src
    b['lT'] = b['lS'] = s0['leg'] * k * lm['leg']
    b['a'] = s0['a'] * k
    b['hl'], b['bl'], b['tl'] = s0['hl'] * k, s0['bl'] * k, s0['tl'] * k
    b['lu'], b['lf'] = s0['lu'] * k * lm['arm'], s0['lf'] * k * lm['arm']
    b['ra'], b['rl'] = s0['ra'] * k, s0['rl'] * k
    b['rT'] = 1.25 * b['rl']
    b['wH'] = s0['wH'] * k
    b['wS'], b['dS'] = s0['sx'] * k, s0['sz'] * k
    b['hW'] = s0['hW'] * k * lm['trunk']
    b['hS'] = s0['sy'] * k * lm['trunk']
    b['tR'] = b['hS'] / 2
    b['lN'] = max(0.03 * k, s0['headRise'] * k * lm['trunk'] - b['hS'])
    b['rH'] = max(0.06, s0['headHalf'] * k)
    b['H'] = b['a'] + 0.956 * (b['lT'] + b['lS'])
    b['gH'] = min(max(s0['bodyFront'] * k, 1.5 * b['rl']), 0.3 * b['H'])
    b['gT'] = max(s0['sx'] * k - b['ra'], 0.5 * s0['sx'] * k)
    b['fw'], b['fh'], b['tw'], b['th'] = 0.05 * k, 0.06 * k, 0.05 * k, 0.05 * k
    b['height'] = s0['height'] * k
    m = copy.deepcopy(s0['m'])
    for kk in m:
        if isinstance(m[kk], tuple):
            m[kk] = tuple(v * k ** 3 for v in m[kk])
        else:
            m[kk] = m[kk] * k ** 3
    # limbs of another length: their mass in proportion (same thickness)
    for kk, part in (('ua', 'arm'), ('fa', 'arm'), ('th', 'leg'), ('sh', 'leg')):
        m[kk] = tuple(v * lm[part] for v in m[kk])
    m['trunk'] *= lm['trunk']
    if mass_mul:
        for kk, v in mass_mul.items():
            if isinstance(m[kk], tuple):
                m[kk] = tuple(x * v for x in m[kk])
            else:
                m[kk] *= v
    b['m'] = m
    b['M'] = sum((sum(v) if isinstance(v, tuple) else v) for v in m.values())
    # capacities
    base = make_body_caps_present(MINERS[src], src_body=True)
    if cap_rule == 'cube':
        cap = {kk: v * k ** 3 for kk, v in MINERS[src]['cap'].items()}
    else:
        now = present_caps(b)
        cap = {}
        for kk in MINERS[src]['cap']:
            # measured capacity x (this body's need / the miner's need, both by the same rule)
            cap[kk] = MINERS[src]['cap'][kk] * now[kk] / base[kk]
    cap = {kk: v * strength for kk, v in cap.items()}
    if cap_mul:
        for kk, v in cap_mul.items():
            cap[kk] *= v
    b['cap'] = {kk: (v, v) for kk, v in cap.items()}
    b['cap']['back'] = cap['back']
    b['cap']['neck'] = cap['neck']
    if side_mul:
        for kk, (l, r) in side_mul.items():
            v = b['cap'][kk]
            b['cap'][kk] = (v[0] * l, v[1] * r)
    b['ranges'] = dict(RANGES)
    if ranges:
        b['ranges'].update(ranges)
    b['strength'] = strength
    return b


def present_caps(b):
    """PhysicalBody's rules (October 6 and 14) on a body dict (lengths and masses as built here)."""
    m = b['m']
    M = b['M']
    knee = 1.6 * M * G * 0.5 * b['lT']
    upperKg = m['trunk'] + m['head'] + sum(m['ua']) + sum(m['fa'])
    share = 0.5 * upperKg * G
    sh = 1.6 * share * b['lu']
    el = 1.6 * share * b['lf']
    asks = (m['trunk'] * (b['hW'] + b['tR']) + m['head'] * (b['hW'] + b['hS'] + b['lN'])
            + sum(m['ua']) * (b['hW'] + b['hS'] - 0.5 * b['lu']) + sum(m['fa']) * (b['hW'] + b['hS'] - b['lu'] - 0.5 * b['lf']))
    back = 2.4 * asks * G
    neck = 3 * m['head'] * G * b['lN']
    return dict(hip=1.5 * knee, knee=knee, ankle=0.7 * knee, toe=0.21 * knee, shoulder=sh, elbow=el, back=back, neck=neck)


def make_body_caps_present(s0, src_body=True):
    # the same rules on the unchanged miner, as built here (so a ratio of needs is like for like)
    b = {}
    b['lT'] = b['lS'] = s0['leg']
    b['lu'], b['lf'] = s0['lu'], s0['lf']
    b['hW'], b['hS'] = s0['hW'], s0['sy']
    b['tR'] = b['hS'] / 2
    b['lN'] = max(0.03, s0['headRise'] - b['hS'])
    b['m'] = s0['m']
    b['M'] = sum((sum(v) if isinstance(v, tuple) else v) for v in s0['m'].values())
    return present_caps(b)


# ---------------------------------------------------------------- kinematics
LINKS = ['hips', 'trunk', 'head', 'uaL', 'faL', 'uaR', 'faR', 'thL', 'shL', 'ftL', 'toL', 'thR', 'shR', 'ftR', 'toR']


def fk(b, q):
    v = lambda n: q[VI[n]]
    P = np.array([v('x'), v('y'), v('z')])
    RH = Rz(v('yaw')) @ Ry(v('pitch')) @ Rx(v('roll'))
    RT = RH @ Ry(v('tflex')) @ Rx(v('tlat')) @ Rz(v('ttw'))
    RN = RT @ Ry(v('nflex')) @ Rx(v('nlat')) @ Rz(v('nrot'))
    K = dict(R=dict(hips=RH, trunk=RT, head=RN), pts={}, coms=[], joints=[], caps=[], spheres=[], boxes=[])
    pts = K['pts']
    waist = P + RH @ np.array([0, 0, b['hW']])
    neck = waist + RT @ np.array([0, 0, b['hS']])
    trunkC = waist + RT @ np.array([0, 0, b['tR']])
    headC = neck + RN @ np.array([0, 0, b['lN']])
    pts.update(P=P, waist=waist, neck=neck, trunkC=trunkC, headC=headC)
    m = b['m']
    K['coms'] += [('hips', m['hips'], P), ('trunk', m['trunk'], trunkC), ('head', m['head'], headC)]
    K['joints'].append(('back', waist, None, {'trunk', 'head', 'uaL', 'faL', 'uaR', 'faR'}, b['cap']['back']))
    K['joints'].append(('neck', neck, None, {'head'}, b['cap']['neck']))
    hs = max(0.0, b['tR'] - b['gT'] / 2)
    K['caps'].append(('hips', P + RH @ np.array([0, b['wH'], 0]), P - RH @ np.array([0, b['wH'], 0]), b['gH']))
    K['caps'].append(('trunk', trunkC + RT @ np.array([0, 0, hs]), trunkC - RT @ np.array([0, 0, hs]), b['gT']))
    K['spheres'].append(('head', headC, b['rH']))
    for s, sg, i in (('L', 1.0, 0), ('R', -1.0, 1)):
        sh = waist + RT @ np.array([b['dS'], sg * b['wS'], b['hS']])
        RU = RT @ Ry(-v('shflex' + s)) @ Rx(sg * v('shabd' + s)) @ Rz(sg * v('shrot' + s))
        el = sh + RU @ np.array([0, 0, -b['lu']])
        RF = RU @ Ry(-v('elb' + s))
        wr = el + RF @ np.array([0, 0, -b['lf']])
        hj = P + RH @ np.array([0, sg * b['wH'], 0])
        RTh = RH @ Ry(-v('hflex' + s)) @ Rx(sg * v('habd' + s)) @ Rz(sg * v('hrot' + s))
        kn = hj + RTh @ np.array([0, 0, -b['lT']])
        RS = RTh @ Ry(v('knee' + s))
        an = kn + RS @ np.array([0, 0, -b['lS']])
        RFt = RS @ Ry(-v('dorsi' + s)) @ Rx(sg * v('inv' + s))
        ball = an + RFt @ np.array([b['bl'], 0, -b['a']])
        RTo = RFt @ Ry(-v('toe' + s))
        K['R'].update({'ua' + s: RU, 'fa' + s: RF, 'th' + s: RTh, 'sh' + s: RS, 'ft' + s: RFt, 'to' + s: RTo})
        pts.update({'sh' + s: sh, 'el' + s: el, 'wr' + s: wr, 'hj' + s: hj, 'kn' + s: kn, 'an' + s: an, 'ball' + s: ball})
        K['coms'] += [('ua' + s, m['ua'][i], 0.5 * (sh + el)), ('fa' + s, m['fa'][i], 0.5 * (el + wr)),
                      ('th' + s, m['th'][i], 0.5 * (hj + kn)), ('sh' + s, m['sh'][i], 0.5 * (kn + an)),
                      ('ft' + s, m['ft'][i], an + RFt @ np.array([0.5 * (b['bl'] - b['hl']), 0, -b['a'] + 0.5 * b['fh']])),
                      ('to' + s, m['to'][i], ball + RTo @ np.array([0.5 * b['tl'], 0, 0.5 * b['th']]))]
        cap = b['cap']
        K['joints'] += [('shoulder' + s, sh, None, {'ua' + s, 'fa' + s}, cap['shoulder'][i]),
                        ('elbow' + s, el, RU[:, 1], {'fa' + s}, cap['elbow'][i]),
                        ('hip' + s, hj, None, {'th' + s, 'sh' + s, 'ft' + s, 'to' + s}, cap['hip'][i]),
                        ('knee' + s, kn, RTh[:, 1], {'sh' + s, 'ft' + s, 'to' + s}, cap['knee'][i]),
                        ('ankle' + s, an, None, {'ft' + s, 'to' + s}, cap['ankle'][i]),
                        ('toe' + s, ball, RFt[:, 1], {'to' + s}, cap['toe'][i])]
        du = unit(el - sh)
        K['caps'].append(('ua' + s, sh + du * min(b['ra'], 0.5 * b['lu']), el - du * min(b['ra'], 0.5 * b['lu']), b['ra']))
        K['caps'].append(('fa' + s, el, wr, b['ra']))
        dt = unit(kn - hj)
        K['caps'].append(('th' + s, hj + dt * min(b['rT'], 0.5 * b['lT']), kn - dt * min(b['rT'], 0.5 * b['lT']), b['rT']))
        ds = unit(an - kn)
        K['caps'].append(('sh' + s, kn + ds * min(b['rl'], 0.5 * b['lS']), an - ds * min(b['rl'], 0.5 * b['lS']), b['rl']))
        fc = [an + RFt @ np.array([x, y, z]) for x in (-b['hl'], b['bl']) for y in (-b['fw'], b['fw']) for z in (-b['a'], -b['a'] + b['fh'])]
        tc = [ball + RTo @ np.array([u, y, w]) for u in (0, b['tl']) for y in (-b['tw'], b['tw']) for w in (0, b['th'])]
        K['boxes'] += [('ft' + s, fc), ('to' + s, tc)]
    K['M'] = sum(c[1] for c in K['coms'])
    K['com'] = sum(c[1] * c[2] for c in K['coms']) / K['M']
    return K


def unit(v):
    n = np.linalg.norm(v)
    return v / n if n > 1e-12 else np.array([1.0, 0, 0])


def lowest(K, skip=()):
    """The lowest point of each solid (link, z)."""
    out = []
    for link, a, c, r in K['caps']:
        if link in skip:
            continue
        out.append((link, min(a[2], c[2]) - r))
    for link, c, r in K['spheres']:
        if link in skip:
            continue
        out.append((link, c[2] - r))
    for link, cs in K['boxes']:
        if link in skip:
            continue
        out.append((link, min(p[2] for p in cs)))
    return out


# ---------------------------------------------------------------- contacts
def contact_points(b, K, spec):
    """spec: (kind, side or None, extra). Returns [(point, link, normal, mu, other)]."""
    kind, s = spec[0], spec[1]
    P = K['pts']
    R = K['R']
    out = []
    mu = 0.6
    if kind == 'hand':
        out.append((P['wr' + s] - b['ra'] * Z, 'fa' + s, Z, mu, None))
    elif kind == 'forearm':
        out += [(P['el' + s] - b['ra'] * Z, 'fa' + s, Z, mu, None), (P['wr' + s] - b['ra'] * Z, 'fa' + s, Z, mu, None)]
    elif kind in ('shin', 'knee'):
        ds = unit(P['an' + s] - P['kn' + s])
        k1 = P['kn' + s] + ds * min(b['rl'], 0.5 * b['lS']) - b['rl'] * Z
        out.append((k1, 'sh' + s, Z, mu, None))
        if kind == 'shin':
            out.append((P['an' + s] - ds * min(b['rl'], 0.5 * b['lS']) - b['rl'] * Z, 'sh' + s, Z, mu, None))
    elif kind == 'calf':
        ds = unit(P['an' + s] - P['kn' + s])
        out.append((P['an' + s] - ds * min(b['rl'], 0.5 * b['lS']) - b['rl'] * Z, 'sh' + s, Z, mu, None))
    elif kind == 'sole':
        for x in (-b['hl'], b['bl']):
            for y in (-b['fw'], b['fw']):
                out.append((P['an' + s] + R['ft' + s] @ np.array([x, y, -b['a']]), 'ft' + s, Z, mu, None))
    elif kind == 'heel':
        for y in (-b['fw'], b['fw']):
            out.append((P['an' + s] + R['ft' + s] @ np.array([-b['hl'], y, -b['a']]), 'ft' + s, Z, mu, None))
    elif kind == 'ball':
        for y in (-b['fw'], b['fw']):
            out.append((P['an' + s] + R['ft' + s] @ np.array([b['bl'], y, -b['a']]), 'ft' + s, Z, mu, None))
    elif kind == 'toes':
        for y in (-b['tw'], b['tw']):
            out.append((P['ball' + s] + R['to' + s] @ np.array([b['tl'], y, 0]), 'to' + s, Z, mu, None))
    elif kind == 'seat':
        for sg in (1, -1):
            out.append((P['P'] + R['hips'] @ np.array([0, sg * b['wH'], 0]) - b['gH'] * Z, 'hips', Z, mu, None))
    elif kind == 'thighF':
        dt = unit(P['kn' + s] - P['hj' + s])
        out += [(P['hj' + s] + dt * b['rT'] - b['rT'] * Z, 'th' + s, Z, mu, None),
                (P['kn' + s] - dt * b['rT'] - b['rT'] * Z, 'th' + s, Z, mu, None)]
    elif kind == 'chest':
        for link, a, c, r in K['caps']:
            if link == 'trunk':
                out += [(a - r * Z, 'trunk', Z, mu, None), (c - r * Z, 'trunk', Z, mu, None)]
    elif kind == 'handthigh':
        ls, frac = spec[2]
        ax = P['hj' + ls] + frac * (P['kn' + ls] - P['hj' + ls])
        n = unit(P['wr' + s] - ax)
        out.append((P['wr' + s] - b['ra'] * n, 'fa' + s, n, 0.5, 'th' + ls))
    else:
        raise ValueError(kind)
    return out


def all_contacts(b, K, specs):
    out = []
    for sp in specs:
        out += contact_points(b, K, sp)
    return out


# ---------------------------------------------------------------- statics
DIRS = [np.array(d, float) / np.linalg.norm(d) for d in
        [(1, 0, 0), (0, 1, 0), (0, 0, 1), (1, 1, 0), (1, -1, 0), (1, 0, 1), (1, 0, -1), (0, 1, 1), (0, 1, -1),
         (1, 1, 1), (1, 1, -1), (1, -1, 1), (1, -1, -1)]]


def skew(r):
    return np.array([[0, -r[2], r[1]], [r[2], 0, -r[0]], [-r[1], r[0], 0]])


def torque_maps(K, C, shift=None):
    """For each joint: tau0 (gravity) and the 3x(3nc) map B so that tau = tau0 - B f."""
    nc = len(C)
    out = []
    if '_mom' not in K:
        mom = {}
        for link, mm, c in K['coms']:
            mom[link] = (mm, mm * c)
        K['_mom'] = mom
    mom = K['_mom']
    for name, o, axis, sub, cap in K['joints']:
        msum = 0.0
        S = np.zeros(3)
        for link in sub:
            mm, mc = mom[link]
            msum += mm
            S = S + mc
        S = S - msum * o
        if shift is not None:
            S = S + msum * shift
        t0 = G * np.array([S[1], -S[0], 0.0])
        B = np.zeros((3, 3 * nc))
        for i, (p, link, n, mu, other) in enumerate(C):
            if link in sub:
                B[:, 3 * i:3 * i + 3] += skew(p - o)
            if other is not None and other in sub:
                B[:, 3 * i:3 * i + 3] -= skew(p - o)
        out.append((name, axis, cap, t0, B))
    return out


def equilibrium(K, C, shift=None):
    nc = len(C)
    A = np.zeros((6, 3 * nc))
    for i, (p, link, n, mu, other) in enumerate(C):
        if other is not None:
            continue
        A[0:3, 3 * i:3 * i + 3] = np.eye(3)
        A[3:6, 3 * i:3 * i + 3] = skew(p)
    M = K['M']
    S = M * K['com']
    if shift is not None:
        S = S + M * shift
    mom = G * np.array([S[1], -S[0], 0.0])
    bvec = np.concatenate([M * G * Z, mom])
    return A, bvec


def generators(n, mu):
    t1 = unit(np.cross(n, np.array([1.0, 0, 0]) if abs(n[0]) < 0.9 else np.array([0, 1.0, 0])))
    t2 = np.cross(n, t1)
    return [unit(n + mu * t1), unit(n - mu * t1), unit(n + mu * t2), unit(n - mu * t2)]


def reserve_lp(K, C, shift=None, want=False):
    """min s: equilibrium, friction pyramids, |d.tau_j| <= s cap_j. Returns s (inf if infeasible) [and details]."""
    nc = len(C)
    gens = [generators(n, mu) for (p, link, n, mu, other) in C]
    ng = 4 * nc
    Gm = np.zeros((3 * nc, ng))
    for i in range(nc):
        for k in range(4):
            Gm[3 * i:3 * i + 3, 4 * i + k] = gens[i][k]
    A, bvec = equilibrium(K, C, shift)
    Aeq = np.hstack([A @ Gm, np.zeros((6, 1))])
    rows, rhs = [], []
    maps = torque_maps(K, C, shift)
    for name, axis, cap, t0, B in maps:
        BG = B @ Gm
        ds = [axis] if axis is not None else DIRS
        for d in ds:
            row = d @ BG
            c0 = d @ t0
            rows.append(np.concatenate([-row, [-cap]]))
            rhs.append(-c0)
            rows.append(np.concatenate([row, [-cap]]))
            rhs.append(c0)
    Aub = np.array(rows)
    bub = np.array(rhs)
    W = K['M'] * G
    cost = np.concatenate([np.full(ng, 1e-4 / W), [1.0]])
    res = linprog(cost, A_ub=Aub, b_ub=bub, A_eq=Aeq, b_eq=bvec, bounds=[(0, None)] * ng + [(0, None)], method='highs')
    if res.status != 0:
        return (math.inf, None) if want else math.inf
    s = res.x[-1]
    if not want:
        return s
    lam = res.x[:ng]
    f = Gm @ lam
    per = {}
    for name, axis, cap, t0, B in maps:
        tau = t0 - B @ f
        per[name] = (abs(axis @ tau) if axis is not None else float(np.linalg.norm(tau))) / cap
    return s, dict(f=f.reshape(nc, 3), per=per)


def feasible_region(K, C, w=1.0, ndir=16, tmax=None):
    """Radial extent of the set of horizontal COM shifts the joints and the friction can hold at reserve <= w."""
    if tmax is None:
        tmax = 1.5
    ts = []
    s0 = reserve_lp(K, C)
    if s0 > w:
        return s0, -1.0, []
    pts = []
    for k in range(ndir):
        ang = 2 * math.pi * k / ndir
        u = np.array([math.cos(ang), math.sin(ang), 0.0])
        lo, hi = 0.0, tmax
        if reserve_lp(K, C, shift=hi * u) <= w:
            ts.append(hi)
            pts.append(hi * u[:2])
            continue
        for _ in range(11):
            mid = 0.5 * (lo + hi)
            if reserve_lp(K, C, shift=mid * u) <= w:
                lo = mid
            else:
                hi = mid
        ts.append(lo)
        pts.append(lo * u[:2])
    # distance from the origin (the COM now) to the polygon of the boundary points
    dmin = math.inf
    for k in range(ndir):
        a, c = pts[k], pts[(k + 1) % ndir]
        e = c - a
        L2 = e @ e
        if L2 < 1e-12:
            d = np.linalg.norm(a)
        else:
            tt = max(0.0, min(1.0, -(a @ e) / L2))
            d = np.linalg.norm(a + tt * e)
        dmin = min(dmin, d)
    return s0, dmin, pts


def hull(points):
    pts = sorted(set((round(p[0], 9), round(p[1], 9)) for p in points))
    if len(pts) <= 2:
        return pts
    def cross(o, a, c):
        return (a[0] - o[0]) * (c[1] - o[1]) - (a[1] - o[1]) * (c[0] - o[0])
    lower, upper = [], []
    for p in pts:
        while len(lower) >= 2 and cross(lower[-2], lower[-1], p) <= 0:
            lower.pop()
        lower.append(p)
    for p in reversed(pts):
        while len(upper) >= 2 and cross(upper[-2], upper[-1], p) <= 0:
            upper.pop()
        upper.append(p)
    return lower[:-1] + upper[:-1]


def poly_margin(points, c):
    """Signed distance from c (2D) to the convex hull of points: positive inside."""
    h = hull(points)
    c = np.array(c)
    if len(h) == 1:
        return -float(np.linalg.norm(c - np.array(h[0])))
    if len(h) == 2:
        a, e = np.array(h[0]), np.array(h[1]) - np.array(h[0])
        t = max(0.0, min(1.0, ((c - a) @ e) / max(1e-12, e @ e)))
        return -float(np.linalg.norm(c - (a + t * e)))
    inside = True
    dmin = math.inf
    n = len(h)
    for k in range(n):
        a, e = np.array(h[k]), np.array(h[(k + 1) % n]) - np.array(h[k])
        L = np.linalg.norm(e)
        nrm = np.array([e[1], -e[0]]) / L  # outward for ccw
        d = (c - a) @ nrm
        if d > 0:
            inside = False
        t = max(0.0, min(1.0, ((c - a) @ e) / (L * L)))
        dmin = min(dmin, float(np.linalg.norm(c - (a + t * e))))
    return dmin if inside else -dmin


def effort_forces(K, C, alpha=0.02, beta=0.3):
    """The least-effort force sharing (a smooth stand-in for the LP inside the pose search): min sum (tau/cap)^2
    + alpha |f|^2 + beta |f_tangential|^2 (in body weights), subject to equilibrium. Pulling is not excluded here."""
    nc = len(C)
    W = K['M'] * G
    A, bvec = equilibrium(K, C)
    maps = torque_maps(K, C)
    rowsC, rhsC = [], []
    for name, axis, cap, t0, B in maps:
        if axis is not None:
            rowsC.append(axis @ B / cap)
            rhsC.append(axis @ t0 / cap)
        else:
            for k in range(3):
                rowsC.append(B[k] / cap)
                rhsC.append(t0[k] / cap)
    Cm = np.array(rowsC) * W
    d = np.array(rhsC)
    reg = np.zeros((3 * nc, 3 * nc))
    for i, (p, link, n, mu, other) in enumerate(C):
        Pn = np.outer(n, n)
        reg[3 * i:3 * i + 3, 3 * i:3 * i + 3] = alpha * np.eye(3) + beta * (np.eye(3) - Pn)
    # variables g = f / W
    Hm = 2 * (Cm.T @ Cm + reg)
    rhs = 2 * Cm.T @ d
    Aw = A * W
    KKT = np.block([[Hm, Aw.T], [Aw, np.zeros((6, 6))]])
    sol = np.linalg.lstsq(KKT, np.concatenate([rhs, bvec]), rcond=None)[0]
    g = sol[:3 * nc]
    resid = Cm @ g - d
    f = g * W
    return f.reshape(nc, 3), resid


# ---------------------------------------------------------------- the pose search for a stance
def angle_bounds(b, name):
    lo, hi = b['ranges'][rkey(name)]
    return lo * D2R, hi * D2R


def sp(x, k=1.0):
    # softplus, scaled
    return k * np.logaddexp(0, x / k)


def solve_stance(b, st, verbose=False, q_init=None):
    """st: dict(name, contacts, targets(list of fn(K, q)->array), fixed{var:value}, bounds{var:(lo,hi) deg/m}, q0{var:deg/m},
    m_req (m), free(set of vars to keep free beyond defaults), skip_clear (links allowed to touch beyond contacts),
    effort (weight))."""
    fixed = dict(x=0.0, y=0.0, yaw=0.0)
    fixed.update(st.get('fixed', {}))
    names = [n for n in VARS if n not in fixed]
    lo, hi = [], []
    for n in names:
        if n in ('x', 'y', 'z'):
            l, h = (-3.0, 3.0)
        else:
            l, h = angle_bounds(b, n) if n not in ('pitch', 'roll', 'yaw') else (-math.pi, math.pi)
        if n in st.get('bounds', {}):
            bl_, bh_ = st['bounds'][n]
            if n not in ('x', 'y', 'z'):
                bl_, bh_ = bl_ * D2R, bh_ * D2R
            l, h = max(l, bl_), min(h, bh_)
        if h - l < 1e-6:
            h = l + 1e-6
        lo.append(l)
        hi.append(h)
    lo, hi = np.array(lo), np.array(hi)
    q0 = np.zeros(NQ)
    for n, val in fixed.items():
        q0[VI[n]] = val * (D2R if n not in ('x', 'y', 'z') else 1.0)
    init = st.get('q0', {})
    for n in names:
        if n in init:
            q0[VI[n]] = init[n] * (D2R if n not in ('x', 'y', 'z') else 1.0)
    if q_init is not None:
        for n in names:
            q0[VI[n]] = q_init[VI[n]]
    x0 = np.clip(np.array([q0[VI[n]] for n in names]), lo + 1e-7, hi - 1e-7)
    specs = st['contacts']
    H = b['H']
    clear_skip = set(st.get('skip_clear', ()))
    contact_links = set()
    m_req = st.get('m_req', 0.03 * H)
    w_eff = st.get('effort', 1.0)
    ready = set(st.get('ready', ()))

    def full(x):
        q = q0.copy()
        for k, n in enumerate(names):
            q[VI[n]] = x[k]
        return q

    def resid(x):
        q = full(x)
        K = fk(b, q)
        C = all_contacts(b, K, specs)
        r = []
        links_on = set()
        ground = []
        for (p, link, n, mu, other) in C:
            if other is None:
                r.append(300.0 * p[2])
                links_on.add(link)
                ground.append(p[:2])
        for fn in st.get('targets', []):
            r += list(np.atleast_1d(fn(K, q)))
        # nothing else under the ground (a little clearance)
        for link, zlow in lowest(K):
            if link in links_on or link in clear_skip:
                continue
            r.append(60.0 * float(sp(0.01 * H - zlow, 0.005 * H)))
        # its weight within what bears it (and, if it is to let a support go, within what stays)
        if len(ground) >= 1:
            mg = poly_margin(ground, K['com'][:2])
            r.append(30.0 * float(sp(m_req - mg, 0.005 * H)))
            r.append(0.6 * float(sp(0.12 * H - mg, 0.02 * H)) / H)
        if ready:
            g2 = [p[:2] for (p, link, n, mu, other) in all_contacts(b, K, [c for c in specs if c[0] not in ready]) if other is None]
            m2 = poly_margin(g2, K['com'][:2])
            r.append(30.0 * float(sp(st.get('m_ready', 0.02 * H) - m2, 0.005 * H)))
        # effort, shared the easiest way
        if w_eff > 0 and len(C) > 0:
            f, rr = effort_forces(K, C)
            r += list(math.sqrt(w_eff) * rr)
            # no pulling on the ground
            for i, (p, link, n, mu, other) in enumerate(C):
                fn_ = f[i] @ n / (K['M'] * G)
                r.append(3.0 * float(sp(-fn_, 0.02)))
        # comfort: away from the ends of each range (soft, last 12%)
        for k, n in enumerate(names):
            if n in ('x', 'y', 'z', 'yaw', 'pitch', 'roll'):
                continue
            l, h = lo[k], hi[k]
            span = max(1e-3, h - l)
            mrg = 0.12 * span
            r.append(0.4 * float(sp((x[k] - (h - mrg)) / mrg, 0.2) + sp(((l + mrg) - x[k]) / mrg, 0.2)))
        return np.array(r, float)

    res = least_squares(resid, x0, bounds=(lo, hi), method='trf', x_scale='jac', max_nfev=st.get('nfev', 600), ftol=1e-7, xtol=1e-7)
    q = full(res.x)
    return q, res


def evaluate(b, st, q, fr=True):
    K = fk(b, q)
    C = all_contacts(b, K, st['contacts'])
    ground = [p[:2] for (p, link, n, mu, other) in C if other is None]
    s, det = reserve_lp(K, C, want=True)
    mg = poly_margin(ground, K['com'][:2]) if ground else -1
    out = dict(name=st['name'], s=float(s), sp_margin=float(mg), com_h=float(K['com'][2]))
    contact_err = max([abs(p[2]) for (p, link, n, mu, other) in C if other is None] + [0])
    out['contact_err_mm'] = round(1000 * contact_err, 2)
    pen = [z for (link, z) in lowest(K) if z < -0.005 * b['H']]
    out['penetrates'] = len(pen)
    if det is not None:
        per = det['per']
        out['per'] = {k: round(v, 3) for k, v in per.items()}
        out['worst'] = max(per, key=per.get)
        W = K['M'] * G
        f = det['f']
        hands = sum(f[i][2] for i, c in enumerate(C) if c[1].startswith('fa') and c[4] is None) / W
        selfh = sum(abs(f[i] @ c[2]) for i, c in enumerate(C) if c[4] is not None) / W
        out['hands_share'] = round(float(hands), 3)
        out['hands_on_thighs'] = round(float(selfh), 3)
    if fr and math.isfinite(s):
        s0, depth, pts = feasible_region(K, C, w=1.0)
        out['fr_depth'] = round(float(depth), 4)
        out['fr_depth_H'] = round(float(depth) / b['H'], 3)
        s0w, depthw, ptsw = feasible_region(K, C, w=0.6)
        out['fr06_depth'] = round(float(depthw), 4)
    out['q'] = {n: round(float(q[VI[n]] / D2R if n not in ('x', 'y', 'z') else q[VI[n]]), 1 if n not in ('x', 'y', 'z') else 3) for n in VARS}
    out['sp_margin_H'] = round(float(mg) / b['H'], 3)
    return out, K
