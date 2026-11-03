# Strength swept on one body's fitted atlas (poses held; every r scales as 1/S in the LP): the route odds and the style
# settings, to see whether anything switches at a threshold.
import json, sys, math, copy
import route as RT

files, body = sys.argv[1].split(','), sys.argv[2]
R = []
for f in files:
    R += json.load(open(f))
base = {RT.key(r): r for r in R if r['body'] == body}
H = next(r['H'] for r in R if r['body'] == body)
Ss = [2.0, 1.5, 1.0, 0.8, 0.65, 0.5, 0.42, 0.36, 0.32, 0.29, 0.27, 0.25]
for side, routes in (('front', RT.FRONT), ('back', RT.BACK)):
    names = list(routes)
    print('\n%s %s: odds per route (%%), then the drawn-best route time, r_max, gather, overlap, stagger' % (body, side))
    print('S     ' + ' '.join('%5s' % n.split()[0] for n in names) + '   T(best) rmax gather overlap stagger')
    for S in Ss:
        atlas = {}
        for k, e in base.items():
            e2 = dict(e)
            e2['r'] = e['r'] / S
            if 'release_r' in e:
                e2['release_r'] = e['release_r'] / S
            atlas[k] = e2
        rows = []
        for nm in names:
            if nm not in routes:
                continue
            T, st = routes[nm]
            e = RT.evaluate(body, atlas, H, nm, T, st)
            if e:
                rows.append(e)
        if side == 'back':
            fr = [RT.evaluate(body, atlas, H, nm, *RT.FRONT[nm]) for nm in RT.FRONT]
            fr = [x for x in fr if x and x['ok']]
            if fr:
                bf = min(fr, key=lambda x: x['C'])
                e = RT.evaluate(body, atlas, H, 'B4', RT.FRONT[bf['route']][0], RT.FRONT[bf['route']][1], roll=True)
                e['route'] = 'B4(' + bf['route'][:3].strip() + ')'
                rows.append(e)
                if 'B4' not in [n.split()[0] for n in names]:
                    names = names + ['B4']
        RT.softmax(rows)
        P = {x['route'].split()[0].split('(')[0]: (x.get('P', 0.0) if x['ok'] else None) for x in rows}
        ok = [x for x in rows if x['ok']]
        best = min(ok, key=lambda x: x['C']) if ok else None
        cells = []
        for n in names:
            k = n.split()[0]
            v = P.get(k)
            cells.append('%5s' % ('--' if v is None else '%3.0f' % (100 * v)))
        if best:
            st = RT.style(best['rcrit'], H)
            tail = '   %5.1fs %4.2f %5.1fs  %4.2f   %4.2f  %s' % (best['T'], best['rmax'], best['gather'], st['overlap'], st['stagger'], best['route'][:4])
        else:
            tail = '   no route open'
        print('%-5.2f ' % S + ' '.join(cells) + tail)
