# Does the prototype as it stands (proto/b3.py, lib3.py) reproduce its saved results? Evaluates each saved fit's
# residual vector at its saved pose and compares 0.5*|r|^2 with the saved cost.
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


if __name__ == '__main__':
    files = sys.argv[1].split(',') if len(sys.argv) > 1 else ['res_miners2.json', 'res_others.json', 'res_extra.json']
    R = []
    for f in files:
        R += json.load(open(os.path.join(PROTO, f)))
    fixed = {'x', 'y', 'yaw'}
    names = [n for n in b3.VARS if n not in fixed]
    worst = 0.0
    nbad = 0
    for e in R:
        b = make(e['body'])
        st = getattr(L, e['family'])(b, *e['args'])
        fun, x0, bounds = resid_fn(b, st)
        q = np.array(e['q'])
        x = np.array([q[b3.VI[n]] for n in names])
        r = fun(x)
        cost = 0.5 * float(r @ r)
        rel = abs(cost - e['cost']) / max(1e-9, abs(e['cost']))
        worst = max(worst, rel)
        if rel > 1e-6:
            nbad += 1
            print('MISMATCH %-10s %-60s saved %.6f now %.6f' % (e['body'][:10], e['stance'][:60], e['cost'], cost))
    print('checked', len(R), 'worst rel diff', worst, 'mismatches', nbad)
