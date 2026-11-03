# Batch: fit every stance of the library to each body, read its reserve, its margins, and what holds if a support goes.
import os
os.environ.setdefault('OMP_NUM_THREADS', '1')
os.environ.setdefault('OPENBLAS_NUM_THREADS', '1')
os.environ.setdefault('MKL_NUM_THREADS', '1')
import sys, json, time, math
import numpy as np
from multiprocessing import Pool
import b3
from b3 import make_body, solve_stance, fk, all_contacts, reserve_lp, poly_margin, feasible_region, VARS, VI, D2R, lowest
import lib3 as L

BODIES = {
    'Small': dict(src='Small'),
    'Long': dict(src='Long'),
    'Round': dict(src='Round'),
    'Small S0.5': dict(src='Small', strength=0.5),
    'Long S0.5': dict(src='Long', strength=0.5),
    'Round S0.5': dict(src='Round', strength=0.5),
    'Small S0.3': dict(src='Small', strength=0.3),
    'Round S0.3': dict(src='Round', strength=0.3),
    'Small, arms by thickness': dict(src='Small', cap_mul=dict(shoulder=24 / 42, elbow=21 / 36)),
    'Toddler-like (Small, head x1.3, legs x0.85)': dict(src='Small', mass_mul=dict(head=1.3), len_mul=dict(leg=0.85)),
    'Giant (Long x1.6, strength ~ L^3)': dict(src='Long', scale=1.6, cap_rule='cube'),
    'Long, arms x0.65': dict(src='Long', len_mul=dict(arm=0.65)),
    'Long, stiff ankles (dorsiflexion 5)': dict(src='Long', ranges=dict(dorsi=(-50, 5))),
    'Round, stiff knees (100)': dict(src='Round', ranges=dict(knee=(-2, 100))),
    'Heavy trunk (Round, trunk+hips x1.5, S0.7, lean 30)': dict(src='Round', mass_mul=dict(trunk=1.5, hips=1.5), strength=0.7, ranges=dict(tflex=(-20, 30))),
    'Long, right leg 0.4': dict(src='Long', side_mul=dict(hip=(1, 0.4), knee=(1, 0.4), ankle=(1, 0.4), toe=(1, 0.4))),
    'Long S2': dict(src='Long', strength=2.0),
}

STANCES = [('stand', ()), ('quad', ()), ('quad', (True,)), ('tripod', ('L',)), ('quad_ready_kneel', ()),
           ('bear', (0.8,)), ('bear', (0.5,)), ('bear', (0.3,)), ('bear', (0.4, True)), ('bear_ready', (0.25,)),
           ('squat', (0.35,)), ('squat', (0.5,)), ('squat', (0.65,)), ('squat', (0.8,)), ('squat', (0.4, True)), ('squat', (0.6, True)),
           ('squat_hands_ready', (0.35,)), ('squat_hands_ready', (0.5,)),
           ('tallkneel', ()), ('onekneel_toes', ('L',)),
           ('halfkneel', ('L', 'none')), ('halfkneel', ('L', 'thigh')), ('halfkneel', ('L', 'both')), ('halfkneel', ('L', 'ground')),
           ('crouchkneel_ready', ('L',)),
           ('halfkneel_up', (0.55, 'L', 'none')), ('halfkneel_up', (0.7, 'L', 'none')), ('halfkneel_up', (0.85, 'L', 'none')),
           ('halfkneel_up', (0.55, 'L', 'thigh')), ('halfkneel_up', (0.7, 'L', 'thigh')),
           ('halfkneel', ('R', 'none')), ('halfkneel_up', (0.55, 'R', 'none')), ('crouchkneel_ready', ('R',)),
           ('stoopknees', (30,)), ('stoopknees', (60,)),
           ('situp', (15,)), ('situp', (35,)), ('situp', (60,)), ('situp', (25, True)),
           ('press', ()), ('forearms', ()), ('longsit', ())]

# what would bear it if these supports were let go now (kinds dropped), for the stance families that end in a let-go
RELEASE = {'bear': ['hand'], 'squat': ['hand'], 'halfkneel': ['hand', 'handthigh'], 'quad': [], 'tripod': [], 'stoopknees': ['handthigh'],
           'halfkneel_up': ['handthigh'], 'bear_ready': ['hand'], 'squat_hands_ready': ['hand'], 'crouchkneel_ready': ['hand'],
           'quad_ready_kneel': ['hand']}


def job(args):
    bname, sname, sargs = args
    spec = BODIES[bname]
    b = make_body(spec['src'], name=bname, strength=spec.get('strength', 1.0), scale=spec.get('scale', 1.0), cap_mul=spec.get('cap_mul'),
                  side_mul=spec.get('side_mul'), ranges=spec.get('ranges'), mass_mul=spec.get('mass_mul'), len_mul=spec.get('len_mul'),
                  cap_rule=spec.get('cap_rule', 'present'))
    st = getattr(L, sname)(b, *sargs)
    t0 = time.time()
    best = None
    for attempt in range(2):
        q, res = solve_stance(b, st, q_init=None if attempt == 0 else best[0] + np.random.RandomState(attempt).normal(0, 0.05, len(best[0])))
        if best is None or res.cost < best[1].cost:
            best = (q, res)
        if res.cost < 0.05:
            break
    q, res = best
    K = fk(b, q)
    C = all_contacts(b, K, st['contacts'])
    ground = [p[:2] for (p, link, n, mu, other) in C if other is None]
    s, det = reserve_lp(K, C, want=True)
    out = dict(body=bname, stance=st['name'], family=sname, args=list(sargs), cost=float(res.cost), secs=0.0)
    out['contact_err_mm'] = round(1000 * max([abs(p[2]) for (p, link, n, mu, other) in C if other is None] + [0]), 2)
    out['penetration_mm'] = round(-1000 * min([0] + [z for (link, z) in lowest(K, skip=st.get('skip_clear', ())) if link not in {c[1] for c in C}]), 1)
    out['sp_margin'] = float(poly_margin(ground, K['com'][:2])) if ground else -1.0
    out['H'] = b['H']
    out['M'] = b['M']
    if det is not None:
        per = det['per']
        out['r'] = float(max(per.values()))
        out['worst'] = max(per, key=per.get)
        out['per'] = {k: round(float(v), 3) for k, v in per.items()}
        W = K['M'] * b3.G
        f = det['f']
        out['hands_share'] = float(sum(f[i][2] for i, c in enumerate(C) if c[1].startswith('fa') and c[4] is None) / W)
        out['on_thighs'] = float(sum(abs(f[i] @ c[2]) for i, c in enumerate(C) if c[4] is not None) / W)
        s0, depth, pts = feasible_region(K, C, w=1.0, ndir=12)
        out['fr_depth'] = float(depth)
    else:
        out['r'] = math.inf
        out['worst'] = None
        out['fr_depth'] = -1.0
    # if a support were let go now
    drop = RELEASE.get(sname, [])
    kinds = {sp[0] for sp in st['contacts']}
    if drop and kinds & set(drop):
        keep = [sp for sp in st['contacts'] if sp[0] not in drop]
        C2 = all_contacts(b, K, keep)
        g2 = [p[:2] for (p, link, n, mu, other) in C2 if other is None]
        out['release_sp_margin'] = float(poly_margin(g2, K['com'][:2]))
        out['release_r'] = float(reserve_lp(K, C2))
    out['com'] = [float(v) for v in K['com']]
    out['q'] = [float(v) for v in q]
    out['secs'] = round(time.time() - t0, 1)
    return out


if __name__ == '__main__':
    names = sys.argv[1].split('|') if len(sys.argv) > 1 and sys.argv[1] != 'all' else list(BODIES)
    outp = sys.argv[2] if len(sys.argv) > 2 else 'res3.json'
    jobs = [(bn, sn, sa) for bn in names for (sn, sa) in STANCES]
    t0 = time.time()
    with Pool(min(30, len(jobs))) as pool:
        res = pool.map(job, jobs, chunksize=1)
    json.dump(res, open(outp, 'w'), indent=1)
    print('done', len(res), 'jobs in', round(time.time() - t0), 's')
