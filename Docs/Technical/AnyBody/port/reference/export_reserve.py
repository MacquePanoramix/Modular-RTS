# Exports the reserve LP's inputs and the prototype's answers for every saved fit of the three miners, so the C# port
# (src/Reserve) can be checked without the fitter. For each saved fit: the body is rebuilt (b3.make_body), the stance
# (lib3), the kinematics at the saved pose (b3.fk) and the contacts (b3.all_contacts); then the StaticsProblem fields
# of src/Core/StaticsProblem.cs are written, with what the prototype says: the saved values (run3.job) and the values
# the prototype's code gives now (reserve_lp, feasible_region, poly_margin), with HiGHS's dual simplex and interior
# point as well (the LP's forces are not unique, so r = max per-joint share and the hands' share depend on the solver).
#
#   python export_reserve.py [out.json]        (default: reserve_cases.json next to this file)
import os, sys, json, math, time
HERE = os.path.dirname(os.path.abspath(__file__))
PROTO = os.path.normpath(os.path.join(HERE, '..', '..', 'proto'))
sys.path.insert(0, PROTO)
import numpy as np
from scipy.optimize import linprog
import b3
import lib3 as L
import run3

KIND = {'hand': 'Hand', 'forearm': 'Forearm', 'shin': 'Shin', 'knee': 'Knee', 'calf': 'Calf', 'sole': 'Sole', 'heel': 'Heel',
        'ball': 'Ball', 'toes': 'Toes', 'seat': 'Seat', 'thighF': 'ThighFront', 'chest': 'Chest', 'handthigh': 'HandOnThigh'}
LINK = {n: i for i, n in enumerate(b3.LINKS)}


def make(bname):
    spec = run3.BODIES[bname]
    return b3.make_body(spec['src'], name=bname, strength=spec.get('strength', 1.0), scale=spec.get('scale', 1.0),
                        cap_mul=spec.get('cap_mul'), side_mul=spec.get('side_mul'), ranges=spec.get('ranges'),
                        mass_mul=spec.get('mass_mul'), len_mul=spec.get('len_mul'), cap_rule=spec.get('cap_rule', 'present'))


def contacts_with_kind(b, K, specs):
    out = []
    for si, sp in enumerate(specs):
        for c in b3.contact_points(b, K, sp):
            out.append((c, sp[0], si))
    return out


def lp_details(K, C, method):
    """b3.reserve_lp(want=True) with a chosen HiGHS method; returns s, per, f (or inf, None, None)."""
    real = b3.linprog
    b3.linprog = lambda *a, **kw: real(*a, **dict(kw, method=method))
    try:
        s, det = b3.reserve_lp(K, C, want=True)
    finally:
        b3.linprog = real
    if det is None:
        return math.inf, None, None
    return float(s), det['per'], det['f']


def shares(C, f, W):
    hands = sum(f[i][2] for i, c in enumerate(C) if c[1].startswith('fa') and c[4] is None) / W
    thighs = sum(abs(f[i] @ c[2]) for i, c in enumerate(C) if c[4] is not None) / W
    return float(hands), float(thighs)


def fin(v):
    """JSON has no infinity: None stands for 'cannot be held'."""
    if v is None:
        return None
    v = float(v)
    return v if math.isfinite(v) else None


def vec(v):
    return [float(x) for x in v]


def export_case(fname, idx, e):
    b = make(e['body'])
    st = getattr(L, e['family'])(b, *e['args'])
    q = np.array(e['q'], float)
    K = b3.fk(b, q)
    CK = contacts_with_kind(b, K, st['contacts'])
    C = [c for (c, k, si) in CK]
    W = K['M'] * b3.G
    # the StaticsProblem fields
    mom = {}
    for link, mm, c in K['coms']:
        mom[link] = (mm, mm * np.array(c))
    contacts = []
    for (p, link, n, mu, other), kind, si in CK:
        contacts.append(dict(p=vec(p), n=vec(n), mu=float(mu), link=LINK[link], other=(LINK[other] if other is not None else -1),
                             kind=KIND[kind], support=si))
    joints = []
    for name, o, axis, sub, cap in K['joints']:
        msum = sum(mom[l][0] for l in sub)
        S = sum(mom[l][1] for l in sub)
        mask = sum(1 << LINK[l] for l in sub)
        sign = []
        for (p, link, n, mu, other) in C:
            sg = (1 if link in sub else 0) - (1 if (other is not None and other in sub) else 0)
            sign.append(sg)
        joints.append(dict(name=name, origin=vec(o), hinge=axis is not None, axis=vec(axis) if axis is not None else [0.0, 0.0, 0.0],
                           cap=float(cap), mask=mask, subMass=float(msum), subMoment=vec(S), sign=sign))
    case = dict(file=fname, index=idx, body=e['body'], family=e['family'], args=e['args'], stance=e['stance'], cost=e['cost'],
                H=float(b['H']), mass=float(K['M']), com=vec(K['com']), contacts=contacts, joints=joints)
    # the support kinds a let-go drops (run3.RELEASE), when the saved fit has a let-go check
    drop = run3.RELEASE.get(e['family'], [])
    kinds = {sp[0] for sp in st['contacts']}
    rel = None
    if 'release_r' in e:
        if not (drop and kinds & set(drop)):
            raise RuntimeError('saved let-go but no release rule: %s %s' % (e['body'], e['stance']))
        keep = [i for i, (c, k, si) in enumerate(CK) if k not in drop]
        rel = dict(drop=[KIND[k] for k in drop], keep=keep)
    case['release'] = rel
    # what was saved (run3.job)
    sv = dict(r=fin(e['r']), worst=e['worst'], per=e.get('per'), hands_share=e.get('hands_share'), on_thighs=e.get('on_thighs'),
              fr_depth=e.get('fr_depth'), sp_margin=e.get('sp_margin'), release_r=fin(e.get('release_r')),
              release_sp_margin=e.get('release_sp_margin'))
    case['saved'] = sv
    # what the prototype's code gives now (HiGHS 'highs' as in b3.reserve_lp), and by method
    py = {}
    t0 = time.perf_counter()
    s, det = b3.reserve_lp(K, C, want=True)
    t1 = time.perf_counter()
    py['lp_ms'] = 1000 * (t1 - t0)
    py['s'] = fin(s)
    if det is not None:
        per = det['per']
        py['per'] = {k: float(v) for k, v in per.items()}
        py['r'] = float(max(per.values()))
        py['worst'] = max(per, key=per.get)
        py['hands_share'], py['on_thighs'] = shares(C, det['f'], W)
        py['f'] = [vec(x) for x in det['f']]
        t0 = time.perf_counter()
        s0, depth, pts = b3.feasible_region(K, C, w=1.0, ndir=12)
        py['fr_ms'] = 1000 * (time.perf_counter() - t0)
        py['fr_depth'] = float(depth)
        py['fr_pts'] = [vec(x) for x in pts]
    else:
        py['r'] = None
        py['worst'] = None
        py['fr_depth'] = -1.0
    ground = [p[:2] for (p, link, n, mu, other) in C if other is None]
    py['sp_margin'] = float(b3.poly_margin(ground, K['com'][:2])) if ground else -1.0
    if rel is not None:
        C2 = [C[i] for i in rel['keep']]
        g2 = [p[:2] for (p, link, n, mu, other) in C2 if other is None]
        py['release_sp_margin'] = float(b3.poly_margin(g2, K['com'][:2]))
        py['release_r'] = fin(b3.reserve_lp(K, C2))
    alt = {}
    for m in ('highs-ds', 'highs-ipm'):
        s_m, per_m, f_m = lp_details(K, C, m)
        if per_m is None:
            alt[m] = dict(s=None)
            continue
        h, t = shares(C, f_m, W)
        alt[m] = dict(s=s_m, r=float(max(per_m.values())), worst=max(per_m, key=per_m.get), hands_share=h, on_thighs=t)
    py['alt'] = alt
    case['py'] = py
    return case


if __name__ == '__main__':
    outp = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, 'reserve_cases.json')
    cases = []
    t0 = time.time()
    for fname in ('res_miners.json', 'res_miners2.json'):
        R = json.load(open(os.path.join(PROTO, fname)))
        for i, e in enumerate(R):
            cases.append(export_case(fname, i, e))
    with open(outp, 'w') as fo:
        json.dump(dict(source='proto/b3.py, lib3.py, run3.py at the saved poses of res_miners.json and res_miners2.json',
                       gravity=b3.G, dirs=[vec(d) for d in b3.DIRS], cases=cases), fo, allow_nan=False)
    print('exported', len(cases), 'cases in', round(time.time() - t0, 1), 's to', outp)
