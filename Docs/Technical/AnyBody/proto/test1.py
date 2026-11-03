import sys, time, json
import numpy as np
from b3 import make_body, solve_stance, evaluate, fk, VARS, VI, D2R
import lib3 as L

name = sys.argv[1] if len(sys.argv) > 1 else 'Small'
which = sys.argv[2].split(',') if len(sys.argv) > 2 else ['stand', 'quad']
b = make_body(name)
print(name, 'M', round(b['M'], 2), 'H', round(b['H'], 3), 'caps', {k: (round(v[0], 1) if isinstance(v, tuple) else round(v, 1)) for k, v in b['cap'].items()})
for w in which:
    st = getattr(L, w)(b) if '(' not in w else eval('L.' + w.replace('b,', 'b,'))
    t0 = time.time()
    q, res = solve_stance(b, st)
    t1 = time.time()
    out, K = evaluate(b, st, q, fr=True)
    t2 = time.time()
    print(f"{st['name']:55s} solve {t1-t0:5.2f}s nfev {res.nfev} cost {res.cost:.4f} | eval {t2-t1:5.2f}s")
    print('   s=%.3f worst=%s hands=%.2f thighs=%.2f sp_margin=%.3f (%.3f H) fr_depth=%s fr06=%s com_h=%.3f contact_err=%smm pen=%d' % (
        out['s'], out.get('worst'), out.get('hands_share', 0), out.get('hands_on_thighs', 0), out['sp_margin'], out['sp_margin_H'],
        out.get('fr_depth'), out.get('fr06_depth'), out['com_h'], out['contact_err_mm'], out['penetrates']))
    qq = out['q']
    print('   q:', {k: qq[k] for k in ['z', 'pitch', 'tflex', 'nflex', 'shflexL', 'shabdL', 'elbL', 'hflexL', 'habdL', 'hrotL', 'kneeL', 'dorsiL', 'toeL', 'hflexR', 'kneeR', 'dorsiR', 'toeR']})
    print('   per:', out.get('per'))
