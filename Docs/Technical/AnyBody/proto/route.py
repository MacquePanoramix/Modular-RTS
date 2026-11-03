# Layer 2 run on layer 1's output: each body's routes through its fitted stances, their cost, the draw's odds, and the
# style laws. Untuned first weights (stances_strategy.md section 3). Reads run3.py results; writes nothing but stdout/json.
import json, sys, math

G = 9.81
A_MAX = 4.7          # m/s^2, the CMU/Schwickert young-adult peak vertical acceleration
CEIL = 0.9           # the share of its strength a body will use
H_REF = 0.9          # the CMU adult's hip height (assumed)
M0 = 0.03            # base margin unit, in hip heights
TAU = 0.4            # the draw's temperature, in seconds of cost
W_E, W_B, W_S, KAPPA = 1.0, 1.0, 1.0, 1.5
T_GATHER = 0.25
T_ROLL = 0.8
T_REST = 0.6
BAL_STRAIN = float(__import__('os').environ.get('BAL_STRAIN', '1.0'))

PASS, REL, BASE = 'pass', 'rel', 'base'
FRONT = {
    'F1 hands and knees, feet under, squat (CMU face down)': (3.2, [('press', ()), ('quad', ()), ('quad', (True,)),
        ('squat_hands_ready', (0.35,), REL), ('squat', (0.5,)), ('squat', (0.65,)), ('squat', (0.8,)), ('stand', ())]),
    'F1b the same, hands off higher (h 0.5)': (3.2, [('press', ()), ('quad', ()), ('quad', (True,)),
        ('squat_hands_ready', (0.5,), REL), ('squat', (0.65,)), ('squat', (0.8,)), ('stand', ())]),
    'F2 hands and knees, half-kneel (armour)': (3.2, [('press', ()), ('quad', ()), ('tripod', ('L',)),
        ('halfkneel', ('L', 'ground')), ('crouchkneel_ready', ('L',), REL), ('halfkneel', ('L', 'none')),
        ('halfkneel_up', (0.55, 'L', 'none')), ('halfkneel_up', (0.7, 'L', 'none')), ('halfkneel_up', (0.85, 'L', 'none')), ('stand', ())]),
    'F2r the same, right foot forward': (3.2, [('press', ()), ('quad', ()),
        ('crouchkneel_ready', ('R',), REL), ('halfkneel', ('R', 'none')), ('halfkneel_up', (0.55, 'R', 'none')),
        ('halfkneel_up', (0.7, 'R', 'none')), ('halfkneel_up', (0.85, 'R', 'none')), ('stand', ())]),
    'F3 half-kneel, a hand on the thigh': (3.6, [('press', ()), ('quad', ()), ('tripod', ('L',)),
        ('halfkneel', ('L', 'ground')), ('crouchkneel_ready', ('L',), REL), ('halfkneel', ('L', 'thigh')),
        ('halfkneel_up', (0.55, 'L', 'thigh')), ('halfkneel_up', (0.7, 'L', 'thigh'), REL), ('halfkneel_up', (0.85, 'L', 'none')), ('stand', ())]),
    'F2k hands and knees, kneel up, half-kneel L (Fury)': (3.4, [('press', ()), ('quad', ()), ('tallkneel', (), BASE), ('onekneel_toes', ('L',), PASS),
        ('halfkneel', ('L', 'none')), ('halfkneel_up', (0.55, 'L', 'none')), ('halfkneel_up', (0.7, 'L', 'none')),
        ('halfkneel_up', (0.85, 'L', 'none')), ('stand', ())]),
    'F2kr the same, right foot forward': (3.4, [('press', ()), ('quad', ()), ('tallkneel', (), BASE), ('onekneel_toes', ('R',), PASS),
        ('halfkneel', ('R', 'none')), ('halfkneel_up', (0.55, 'R', 'none')), ('halfkneel_up', (0.7, 'R', 'none')),
        ('halfkneel_up', (0.85, 'R', 'none')), ('stand', ())]),
    'F1t feet under onto the toes, squat on the toes': (3.2, [('press', ()), ('quad', ()), ('quad', (True,)), ('bear', (0.4, True)),
        ('squat', (0.4, True), BASE), ('squat', (0.6, True)), ('squat', (0.8,)), ('stand', ())]),
    'F4 hands and feet, hands walked back (bear)': (3.4, [('press', ()), ('quad', ()), ('quad', (True,)), ('bear', (0.8,)),
        ('bear', (0.5,)), ('bear', (0.3,)), ('bear_ready', (0.25,), REL), ('stand', ())]),
    'F5 bear, hands up the knees (Gowers)': (4.5, [('press', ()), ('quad', ()), ('quad', (True,)), ('bear', (0.8,)),
        ('bear', (0.5,)), ('bear', (0.3,)), ('bear_ready', (0.25,), REL), ('stoopknees', (30,), REL), ('stand', ())]),
}
BACK = {
    'B1 sit up swinging, squat (CMU from the back)': (4.65, [('situp', (15,), PASS), ('situp', (35,)), ('situp', (60,)),
        ('longsit', ()), ('squat_hands_ready', (0.35,), REL), ('squat', (0.5,)), ('squat', (0.65,)), ('squat', (0.8,)), ('stand', ())]),
    'B2 sit up propped on the hands, squat': (5.0, [('situp', (25, True)), ('situp', (60,)), ('longsit', ()),
        ('squat_hands_ready', (0.35,), REL), ('squat', (0.5,)), ('squat', (0.65,)), ('squat', (0.8,)), ('stand', ())]),
    'B3 sit up, kneel, half-kneel (Fury)': (3.75, [('situp', (15,), PASS), ('situp', (35,)), ('situp', (60,)), ('quad', ()),
        ('tallkneel', (), BASE), ('onekneel_toes', ('L',), PASS), ('halfkneel', ('L', 'none')), ('halfkneel_up', (0.55, 'L', 'none')), ('halfkneel_up', (0.7, 'L', 'none')),
        ('halfkneel_up', (0.85, 'L', 'none')), ('stand', ())]),
    'B3r the same, right foot forward': (3.75, [('situp', (15,), PASS), ('situp', (35,)), ('situp', (60,)), ('quad', ()),
        ('tallkneel', (), BASE), ('onekneel_toes', ('R',), PASS), ('halfkneel', ('R', 'none')), ('halfkneel_up', (0.55, 'R', 'none')),
        ('halfkneel_up', (0.7, 'R', 'none')), ('halfkneel_up', (0.85, 'R', 'none')), ('stand', ())]),
}


def key(r):
    return (r['family'], tuple(r['args']))


def tempo(r):
    """Stage-time factor from the reserve: the acceleration the worst joint can afford beyond holding."""
    if r >= CEIL:
        return math.inf
    a_aff = G * (CEIL - r) / max(r, 1e-6)
    a = 1.0 / (1.0 / A_MAX + 1.0 / a_aff)        # a smooth min, never above A_MAX
    a_able = 1.0 / (1.0 / A_MAX + 1.0 / (G * (CEIL - 0.2) / 0.2))
    return math.sqrt(a_able / a)                   # 1 for an able adult (r 0.2)


def strain(r):
    return r / (1.0 - r) if r < 1 else math.inf


def evaluate(body, atlas, H, name, T_cmu, stages, roll=False, failures=0):
    kH = math.sqrt(H / H_REF)
    n = len(stages)
    T = T_ROLL * kH if roll else 0.0
    eff = bal = swing = 0.0
    rs, notes, gathers = [], [], 0.0
    for st in stages:
        k = (st[0], st[1])
        flag = st[2] if len(st) > 2 else None
        e = atlas.get(k)
        if e is None:
            return None
        if e['cost'] > 2.0:
            return dict(route=name, ok=False, why='%s %s did not fit (cost %.0f)' % (k[0], k[1], e['cost']))
        r = e['r']
        if not math.isfinite(r) or r >= CEIL:
            if flag == PASS and math.isfinite(e.get('sp_margin', -1)):
                d = max(0.0, -e['sp_margin'] / H) / M0
                if d > 2.0:
                    return dict(route=name, ok=False, why='%s cannot be swung through (deficit %.2f H)' % (k[0], d * M0))
                swing += W_S * d
                notes.append('swing at %s %s (%.0f mm short)' % (k[0], k[1], -1000 * e['sp_margin']))
                T += T_cmu / n * kH * 1.2
                continue
            return dict(route=name, ok=False, why='%s %s cannot be held (r %s)' % (k[0], k[1], 'inf' if not math.isfinite(r) else '%.2f' % r))
        rs.append(r)
        t = tempo(r)
        g = T_GATHER * kH * strain(r) ** 1.5
        gathers += g
        dt = T_cmu / n * kH * t
        T += dt + g
        eff += W_E * strain(r) ** 2 * dt
        if flag == BASE:
            bal += W_B * (1.0 + BAL_STRAIN * strain(r)) * math.exp(-max(e['sp_margin'], 0.0) / H / M0)
        if flag == REL and 'release_sp_margin' in e:
            m = e['release_sp_margin'] / H
            rr = e['release_r']
            if m <= 0 or not math.isfinite(rr) or rr >= CEIL:
                return dict(route=name, ok=False, why='cannot let go at %s %s (margin %+.0f mm)' % (k[0], k[1], 1000 * e['release_sp_margin']))
            bal += W_B * (1.0 + BAL_STRAIN * strain(rr)) * math.exp(-m / M0)
            rs.append(rr)
            eff += W_E * strain(rr) ** 2 * dt
    p = 8.0
    rcrit = sum(x ** p for x in rs) ** (1.0 / p) if rs else 0.0
    rcrit = min(rcrit, max(rs)) if rs else 0.0
    rest = T_REST * kH * strain(max(rs)) ** 1.5 if rs else 0.0
    T += rest
    C = T + eff + bal + swing + KAPPA * failures
    return dict(route=name, ok=True, C=C, T=T, rest=rest, rmax=max(rs), rcrit=rcrit, eff=eff, bal=bal, swing=swing, gather=gathers, notes=notes)


def style(rc, H):
    u = strain(rc)
    kH = math.sqrt(H / H_REF)
    return dict(overlap=0.5 / (1 + u * u), stagger=0.1 + 0.9 * u * u / (1 + u * u), gather=T_GATHER * kH * u ** 1.5,
                tempo=tempo(rc), hands_kept_margin_H=M0 * (1 + u))


def softmax(rows):
    ok = [x for x in rows if x['ok']]
    if not ok:
        return
    cmin = min(x['C'] for x in ok)
    w = [math.exp(-(x['C'] - cmin) / TAU) for x in ok]
    s = sum(w)
    for x, wi in zip(ok, w):
        x['P'] = wi / s


def main():
    files = sys.argv[1].split(',')
    R = []
    for f in files:
        R += json.load(open(f))
    bodies = []
    for r in R:
        if r['body'] not in bodies:
            bodies.append(r['body'])
    out = {}
    for b in bodies:
        atlas = {key(r): r for r in R if r['body'] == b}
        H = next(r['H'] for r in R if r['body'] == b)
        res = {'front': [], 'back': []}
        for nm, (T, st) in FRONT.items():
            e = evaluate(b, atlas, H, nm, T, st)
            if e:
                res['front'].append(e)
        softmax(res['front'])
        best_front = min((x for x in res['front'] if x['ok']), key=lambda x: x['C'], default=None)
        for nm, (T, st) in BACK.items():
            e = evaluate(b, atlas, H, nm, T, st)
            if e:
                res['back'].append(e)
        if best_front:
            e = evaluate(b, atlas, H, 'B4 roll to the front, then ' + best_front['route'][:2], FRONT[best_front['route']][0],
                         FRONT[best_front['route']][1], roll=True)
            res['back'].append(e)
        softmax(res['back'])
        out[b] = dict(H=H, M=next(r['M'] for r in R if r['body'] == b), **res)
        print('\n=== %s  (H %.3f m, M %.1f kg)' % (b, H, out[b]['M']))
        for side in ('front', 'back'):
            for x in sorted(res[side], key=lambda x: (not x['ok'], x.get('C', 0))):
                if x['ok']:
                    st = style(x['rcrit'], H)
                    print('  %-5s %-52s P %4.2f C %5.2f T %5.1fs rmax %.2f eff %.2f bal %.2f swing %.2f gather %.1fs | overlap %.2f stagger %.2f %s' % (
                        side, x['route'][:52], x.get('P', 0), x['C'], x['T'], x['rmax'], x['eff'], x['bal'], x['swing'], x['gather'],
                        st['overlap'], st['stagger'], '; '.join(x['notes'])))
                else:
                    print('  %-5s %-52s --  %s' % (side, x['route'][:52], x['why']))
    if len(sys.argv) > 2:
        json.dump(out, open(sys.argv[2], 'w'), indent=1, default=str)


if __name__ == '__main__':
    main()
