# Exports the body/kinematics/fitter reference data for the C# port (src/Body), from the prototype as it stands
# (proto/b3.py, lib3.py, run3.py), which reproduces the saved fits exactly (check_saved.py: 765 of 765 costs to 1e-12).
#
#   python export_body.py            -> body_ref_bodies.json, body_ref_cases.json, body_ref_fk.json
#
# For each saved fit (res_miners2 + res_others + res_extra): the stance's start vector x0 and bounds (as solve_stance
# builds them), the residual vector at the saved pose and at x0 (captured from solve_stance's own resid closure), the
# saved cost/metrics. For the three miners' fits also the kinematics (points, rotations, centres, contacts, lowest points,
# the effort split) at the saved pose. Plus every body as make_body builds it, and the restart noise run3.job uses.
import os, sys, json, math, time
HERE = os.path.dirname(os.path.abspath(__file__))
PROTO = os.path.normpath(os.path.join(HERE, '..', '..', 'proto'))
sys.path.insert(0, PROTO)
import numpy as np
import b3
import lib3 as L
import run3

CAPTURE = {}


class _Fake:
    def __init__(self, x):
        self.x = x
        self.cost = 0.0
        self.nfev = 0


def _capture(fun, x0, bounds=None, **kw):
    CAPTURE['fun'] = fun
    CAPTURE['x0'] = np.array(x0, float)
    CAPTURE['bounds'] = bounds
    CAPTURE['kw'] = kw
    return _Fake(np.array(x0, float))


def make(bname):
    spec = run3.BODIES[bname]
    return b3.make_body(spec['src'], name=bname, strength=spec.get('strength', 1.0), scale=spec.get('scale', 1.0),
                        cap_mul=spec.get('cap_mul'), side_mul=spec.get('side_mul'), ranges=spec.get('ranges'),
                        mass_mul=spec.get('mass_mul'), len_mul=spec.get('len_mul'), cap_rule=spec.get('cap_rule', 'present'))


def resid_fn(b, st):
    real = b3.least_squares
    b3.least_squares = _capture
    try:
        b3.solve_stance(b, st)
    finally:
        b3.least_squares = real
    return CAPTURE['fun'], CAPTURE['x0'], CAPTURE['bounds']


def fl(v):
    return [float(x) for x in np.ravel(v)]


def body_json(b):
    o = {}
    for k, v in b.items():
        if k == 'm':
            o['m'] = {kk: (list(vv) if isinstance(vv, tuple) else vv) for kk, vv in v.items()}
        elif k == 'cap':
            o['cap'] = {kk: (list(vv) if isinstance(vv, tuple) else vv) for kk, vv in v.items()}
        elif k == 'ranges':
            o['ranges'] = {kk: list(vv) for kk, vv in v.items()}
        else:
            o[k] = v
    return o


def fk_json(b, st, q):
    K = b3.fk(b, q)
    C = b3.all_contacts(b, K, st['contacts'])
    f, rr = b3.effort_forces(K, C) if len(C) > 0 else (np.zeros((0, 3)), np.zeros(0))
    A, bvec = b3.equilibrium(K, C)
    nc = len(C)
    W = K['M'] * b3.G
    # the KKT conditioning, to see whether lstsq's minimum-norm answer ever differs from a plain solve
    maps = b3.torque_maps(K, C)
    o = dict(pts={k: fl(v) for k, v in K['pts'].items()}, R={k: fl(v) for k, v in K['R'].items()},
             coms=[[c[0], float(c[1])] + fl(c[2]) for c in K['coms']], com=fl(K['com']), M=float(K['M']),
             joints=[[j[0]] + fl(j[1]) + (fl(j[2]) if j[2] is not None else []) + [float(j[4])] for j in K['joints']],
             caps=[[c[0]] + fl(c[1]) + fl(c[2]) + [float(c[3])] for c in K['caps']],
             spheres=[[c[0]] + fl(c[1]) + [float(c[2])] for c in K['spheres']],
             boxes=[[c[0]] + [fl(p) for p in c[1]] for c in K['boxes']],
             lowest=[[l, float(z)] for (l, z) in b3.lowest(K)],
             contacts=[fl(p) + [l] + fl(n) + [float(mu), other] for (p, l, n, mu, other) in C],
             effort_f=fl(f), effort_r=fl(rr), rankA=int(np.linalg.matrix_rank(A)) if nc else 0)
    ground = [p[:2] for (p, link, n, mu, other) in C if other is None]
    o['margin'] = float(b3.poly_margin(ground, K['com'][:2])) if ground else None
    return o


if __name__ == '__main__':
    t0 = time.time()
    # bodies
    bodies = {bn: body_json(make(bn)) for bn in run3.BODIES}
    json.dump(dict(bodies=bodies, order=list(run3.BODIES), vars=b3.VARS,
                   noise1=fl(np.random.RandomState(1).normal(0, 0.05, len(b3.VARS)))),
              open(os.path.join(HERE, 'body_ref_bodies.json'), 'w'), indent=0)
    files = ['res_miners2.json', 'res_others.json', 'res_extra.json']
    R = []
    for fn in files:
        for e in json.load(open(os.path.join(PROTO, fn))):
            e['_file'] = fn
            R.append(e)
    fixed = {'x', 'y', 'yaw'}
    names = [n for n in b3.VARS if n not in fixed]
    cases = []
    fks = []
    worst = 0.0
    for idx, e in enumerate(R):
        b = make(e['body'])
        st = getattr(L, e['family'])(b, *e['args'])
        fun, x0, (lo, hi) = resid_fn(b, st)
        q = np.array(e['q'])
        x = np.array([q[b3.VI[n]] for n in names])
        r_saved = fun(x)
        r_x0 = fun(x0)
        cost = 0.5 * float(r_saved @ r_saved)
        worst = max(worst, abs(cost - e['cost']) / max(1e-9, e['cost']))
        c = dict(file=e['_file'], body=e['body'], family=e['family'], args=e['args'], stance=st['name'], cost=e['cost'],
                 q=e['q'], x0=fl(x0), lo=fl(lo), hi=fl(hi), r_saved=fl(r_saved), r_x0=fl(r_x0),
                 contact_err_mm=e['contact_err_mm'], penetration_mm=e['penetration_mm'], sp_margin=e['sp_margin'],
                 release_sp_margin=e.get('release_sp_margin'), r=(e['r'] if math.isfinite(e['r']) else None),
                 worst=e.get('worst'), secs=e.get('secs'), com=e['com'], H=e['H'], M=e['M'])
        cases.append(c)
        if e['_file'] == 'res_miners2.json':
            fk = fk_json(b, st, q)
            fk.update(body=e['body'], family=e['family'], args=e['args'])
            fks.append(fk)
    json.dump(cases, open(os.path.join(HERE, 'body_ref_cases.json'), 'w'))
    json.dump(fks, open(os.path.join(HERE, 'body_ref_fk.json'), 'w'))
    ranks = {}
    for fk in fks:
        ranks[fk['rankA']] = ranks.get(fk['rankA'], 0) + 1
    print('bodies', len(bodies), 'cases', len(cases), 'fk', len(fks), 'worst cost rel diff', worst, 'rank(A) counts', ranks,
          'in', round(time.time() - t0, 1), 's')
