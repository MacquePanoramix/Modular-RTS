# The stance library, written once for no body in particular (3D), as placement rules in the body's own units.
import math
import numpy as np
from b3 import D2R, VI

W_PLACE = 25.0   # per metre outside a placement band
W_ANG = 1.5      # per radian outside an angle band


def band(v, lo, hi, w):
    return w * (max(0.0, lo - v) + max(0.0, v - hi))


def qband(name, lo, hi, w=W_ANG):
    return lambda K, q: [band(q[VI[name]], lo * D2R, hi * D2R, w)]


def U(b):
    """The body's own units."""
    lA = b['lu'] + b['lf'] + b['ra']
    lL = b['lT'] + b['lS'] + b['a']
    foot = b['hl'] + b['bl']
    return lA, lL, foot


def elev(v):
    return math.degrees(math.asin(max(-1.0, min(1.0, v[2]))))


def head_look(lo, hi, w=1.0):
    # the head's forward axis, degrees above the horizontal
    return lambda K, q: [band(elev(K['R']['head'][:, 0]), lo, hi, w * D2R)]


def trunk_elev(lo, hi, w=1.5):
    # the trunk's long axis, degrees above the horizontal
    return lambda K, q: [band(elev(K['R']['trunk'][:, 2]), lo, hi, w * D2R)]


def foot_yaw(s, lo=-0.35, hi=0.35, w=2.0):
    def f(K, q):
        x = K['R']['ft' + s][:, 0]
        sg = 1 if s == 'L' else -1
        return [band(sg * math.atan2(x[1], x[0]), lo, hi, w)]
    return f


def pt(name, axis, lo, hi, rel=None, w=W_PLACE):
    """A point's coordinate (x 0, y 1, z 2) within [lo, hi], absolute or relative to another point (or the root)."""
    def f(K, q):
        v = K['pts'][name][axis] - (K['pts'][rel][axis] if rel else 0.0)
        return [band(v, lo, hi, w)]
    return f


def mean_pt(names):
    return lambda K: sum(K['pts'][n] for n in names) / len(names)


def rel_pts(a_names, b_names, axis, lo, hi, w=W_PLACE):
    def f(K, q):
        a = sum(K['pts'][n][axis] for n in a_names) / len(a_names)
        c = sum(K['pts'][n][axis] for n in b_names) / len(b_names)
        return [band(a - c, lo, hi, w)]
    return f


def sym(names, w=0.6):
    def f(K, q):
        out = []
        for n in names:
            a, c = q[VI[n + 'L']], q[VI[n + 'R']]
            out.append(w * (a - c))
        return out
    return f


def hand_on_thigh(b, hs, ls, lo=0.6, hi=0.95, w=300.0):
    def f(K, q):
        P = K['pts']
        hj, kn, wr = P['hj' + ls], P['kn' + ls], P['wr' + hs]
        d = kn - hj
        L = np.linalg.norm(d)
        u = d / L
        t = float((wr - hj) @ u) / L
        ax = hj + max(0.0, min(1.0, t)) * d
        dist = np.linalg.norm(wr - ax)
        n = (wr - ax) / max(dist, 1e-9)
        return [w * (dist - (b['ra'] + b['rT'])), band(t, lo, hi, 30.0), band(n[2], 0.35, 1.0, 10.0)]
    return f


def lifted(name, zmin, w=W_PLACE):
    return lambda K, q: [band(K['pts'][name][2], zmin, 9.0, w)]


# ---------------------------------------------------------------- the stances
def stand(b):
    lA, lL, foot = U(b)
    return dict(name='stand', contacts=[('sole', 'L'), ('sole', 'R')],
                targets=[pt('anL', 1, 0.8 * b['wH'], 1.3 * b['wH']), pt('anR', 1, -1.3 * b['wH'], -0.8 * b['wH']),
                         rel_pts(['anL'], ['anR'], 0, -0.02 * b['H'], 0.02 * b['H']), foot_yaw('L', -0.1, 0.25), foot_yaw('R', -0.1, 0.25),
                         trunk_elev(85, 90), head_look(-10, 5), qband('shflexL', -10, 20), qband('shflexR', -10, 20),
                         qband('elbL', 0, 30), qband('elbR', 0, 30), sym(['shflex', 'elb', 'hflex', 'knee', 'dorsi'])],
                q0=dict(z=b['H'], hflexL=5, hflexR=5, kneeL=8, kneeR=8, dorsiL=3, dorsiR=3, elbL=10, elbR=10),
                m_req=0.25 * foot)


def quad(b, toes=False):
    lA, lL, foot = U(b)
    C = [('hand', 'L'), ('hand', 'R')] + ([('knee', 'L'), ('knee', 'R'), ('toes', 'L'), ('toes', 'R')] if toes else [('shin', 'L'), ('shin', 'R')])
    T = [pt('wrL', 0, -0.02 * lA, 0.15 * lA, rel='shL'), pt('wrR', 0, -0.02 * lA, 0.15 * lA, rel='shR'),
         pt('wrL', 1, b['wS'] - 0.05 * lA, b['wS'] + 0.2 * lA), pt('wrR', 1, -b['wS'] - 0.2 * lA, -b['wS'] + 0.05 * lA),
         pt('knL', 0, -0.15 * b['lT'], 0.10 * b['lT'], rel='hjL'), pt('knR', 0, -0.15 * b['lT'], 0.10 * b['lT'], rel='hjR'),
         pt('knL', 1, b['wH'], 2.0 * b['wH']), pt('knR', 1, -2.0 * b['wH'], -b['wH']),
         qband('tflex', -15, 15), head_look(-60, -15), qband('elbL', 0, 25), qband('elbR', 0, 25),
         sym(['shflex', 'elb', 'hflex', 'knee', 'dorsi'])]
    if not toes:
        T += [qband('dorsiL', -50, -25), qband('dorsiR', -50, -25)]
    return dict(name='hands and knees' + (' (toes tucked)' if toes else ''), contacts=C, targets=T,
                skip_clear=() if toes else ('ftL', 'toL', 'ftR', 'toR'),
                q0=dict(z=b['lT'] + b['rl'], pitch=75, tflex=0, nflex=-20, shflexL=80, shflexR=80, elbL=5, elbR=5,
                        hflexL=80, hflexR=80, kneeL=90, kneeR=90, dorsiL=-40, dorsiR=-40, toeL=40 if toes else 0, toeR=40 if toes else 0),
                m_req=0.04 * b['H'])


def tripod(b, lift='L'):
    lA, lL, foot = U(b)
    on = 'R' if lift == 'L' else 'L'
    st = quad(b)
    st['name'] = 'tripod (hands and one knee, %s knee lifted)' % ('left' if lift == 'L' else 'right')
    st['contacts'] = [('hand', 'L'), ('hand', 'R'), ('shin', on)]
    st['targets'] = [t for t in st['targets']] + [lifted('kn' + lift, 0.04 * b['H']), lifted('an' + lift, 0.03 * b['H'])]
    st['targets'] = [t for t in st['targets'] if True]
    st['m_req'] = 0.03 * b['H']
    return st


def bear(b, D, toes=False):
    lA, lL, foot = U(b)
    C = [('hand', 'L'), ('hand', 'R')] + ([('ball', 'L'), ('toes', 'L'), ('ball', 'R'), ('toes', 'R')] if toes else [('sole', 'L'), ('sole', 'R')])
    T = [pt('anL', 1, b['wH'], 2.2 * b['wH']), pt('anR', 1, -2.2 * b['wH'], -b['wH']),
         rel_pts(['anL'], ['anR'], 0, -0.03 * b['H'], 0.03 * b['H']), foot_yaw('L', -0.15, 0.3), foot_yaw('R', -0.15, 0.3),
         rel_pts(['wrL', 'wrR'], ['ballL', 'ballR'], 0, (D - 0.04) * lL, (D + 0.04) * lL, w=40.0),
         pt('wrL', 1, b['wS'] - 0.1 * lA, b['wS'] + 0.2 * lA), pt('wrR', 1, -b['wS'] - 0.2 * lA, -b['wS'] + 0.1 * lA),
         qband('nflex', -25, 35), qband('elbL', 0, 20), qband('elbR', 0, 20), sym(['shflex', 'elb', 'hflex', 'knee', 'dorsi'])]
    return dict(name='hands and feet D=%.2f%s' % (D, ' toes' if toes else ''), contacts=C, targets=T,
                q0=dict(z=0.75 * lL, pitch=100, tflex=10, shflexL=140, shflexR=140, elbL=5, elbR=5, hflexL=100, hflexR=100,
                        kneeL=25, kneeR=25, dorsiL=15 if not toes else -25, dorsiR=15 if not toes else -25, toeL=40 if toes else 0, toeR=40 if toes else 0),
                m_req=0.04 * b['H'])


def squat(b, h, toes=False, hands=False):
    lA, lL, foot = U(b)
    C = ([('ball', 'L'), ('toes', 'L'), ('ball', 'R'), ('toes', 'R')] if toes else [('sole', 'L'), ('sole', 'R')])
    T = [pt('P', 2, h * b['H'] - 0.004 * b['H'], h * b['H'] + 0.004 * b['H'], w=80.0),
         pt('anL', 1, b['wH'], 2.4 * b['wH']), pt('anR', 1, -2.4 * b['wH'], -b['wH']),
         rel_pts(['anL'], ['anR'], 0, -0.03 * b['H'], 0.03 * b['H']), foot_yaw('L', -0.1, 0.35), foot_yaw('R', -0.1, 0.35),
         head_look(-35, 10), sym(['shflex', 'elb', 'hflex', 'knee', 'dorsi', 'habd', 'hrot'])]
    if hands:
        C = C + [('hand', 'L'), ('hand', 'R')]
        T += [rel_pts(['wrL', 'wrR'], ['ballL', 'ballR'], 0, 0.0, 0.35 * lA), pt('wrL', 1, b['wS'] - 0.15 * lA, b['wS'] + 0.25 * lA),
              pt('wrR', 1, -b['wS'] - 0.25 * lA, -b['wS'] + 0.15 * lA)]
    return dict(name='squat h=%.2f %s%s' % (h, 'toes' if toes else 'flat', ' hands down' if hands else ''), contacts=C, targets=T,
                q0=dict(z=h * b['H'], pitch=35, tflex=15, shflexL=60, shflexR=60, elbL=20, elbR=20, hflexL=95, hflexR=95,
                        kneeL=110, kneeR=110, dorsiL=-25 if toes else 25, dorsiR=-25 if toes else 25, toeL=45 if toes else 0, toeR=45 if toes else 0,
                        habdL=10, habdR=10),
                m_req=(0.06 if toes else 0.1) * foot)


def tallkneel(b):
    return dict(name='tall kneel', contacts=[('shin', 'L'), ('shin', 'R')], skip_clear=('ftL', 'toL', 'ftR', 'toR'),
                targets=[pt('knL', 1, b['wH'], 2.0 * b['wH']), pt('knR', 1, -2.0 * b['wH'], -b['wH']), trunk_elev(70, 90),
                         qband('hflexL', -10, 35), qband('hflexR', -10, 35), qband('dorsiL', -50, -25), qband('dorsiR', -50, -25),
                         head_look(-20, 10), sym(['hflex', 'knee', 'dorsi'])],
                q0=dict(z=b['lT'] + b['rl'], pitch=5, hflexL=10, hflexR=10, kneeL=90, kneeR=90, dorsiL=-40, dorsiR=-40),
                m_req=0.02 * b['H'])


def onekneel(b, lift='L'):
    on = 'R' if lift == 'L' else 'L'
    st = tallkneel(b)
    st['name'] = 'one knee, the other leg lifted'
    st['contacts'] = [('shin', on)]
    st['targets'] = [pt('kn' + on, 1, -2.0 * b['wH'] if on == 'R' else b['wH'], -b['wH'] if on == 'R' else 2.0 * b['wH']),
                     trunk_elev(65, 90), lifted('an' + lift, 0.06 * b['H']), lifted('kn' + lift, 0.08 * b['H']),
                     qband('dorsi' + on, -50, -25), head_look(-20, 10)]
    st['m_req'] = 0.0
    return st


def halfkneel(b, front='L', hands='none', back='laid'):
    lA, lL, foot = U(b)
    bk = 'R' if front == 'L' else 'L'
    sgf = 1 if front == 'L' else -1
    C = [('sole', front)] + ([('shin', bk)] if back == 'laid' else [('knee', bk), ('toes', bk)])
    T = [pt('an' + front, 0, 0.25 * lL, 0.55 * lL, rel='P'),
         pt('an' + front, 1, *(sorted([sgf * b['wH'], sgf * 2.6 * b['wH']]))), foot_yaw(front, -0.2, 0.35),
         pt('kn' + bk, 0, -0.35 * b['lT'], 0.10 * b['lT'], rel='P'),
         pt('kn' + bk, 1, *(sorted([-sgf * 0.6 * b['wH'], -sgf * 2.0 * b['wH']]))),
         head_look(-30, 10)]
    if back == 'laid':
        T.append(qband('dorsi' + bk, -50, -25))
    if hands == 'thigh':
        C.append(('handthigh', front, (front, 0.8)))
        T.append(hand_on_thigh(b, front, front))
        T.append(trunk_elev(35, 90))
    elif hands == 'both':
        C += [('handthigh', front, (front, 0.85)), ('handthigh', bk, (front, 0.7))]
        T += [hand_on_thigh(b, front, front, 0.65, 0.97), hand_on_thigh(b, bk, front, 0.5, 0.9), trunk_elev(30, 90)]
    elif hands == 'ground':
        C += [('hand', 'L'), ('hand', 'R')]
        T += [rel_pts(['wrL', 'wrR'], ['an' + front], 0, -0.15 * lA, 0.35 * lA),
              pt('wrL', 1, b['wS'] - 0.2 * lA, b['wS'] + 0.3 * lA), pt('wrR', 1, -b['wS'] - 0.3 * lA, -b['wS'] + 0.2 * lA)]
    else:
        T.append(trunk_elev(55, 90))
    q0 = {'z': b['lT'] + b['rl'], 'pitch': 10 if hands != 'ground' else 55, 'hflex' + front: 80, 'knee' + front: 90, 'dorsi' + front: 10,
          'hflex' + bk: 0, 'knee' + bk: 90, 'dorsi' + bk: -40 if back == 'laid' else -10, 'toe' + bk: 0 if back == 'laid' else 50,
          'shflexL': 30 if hands != 'ground' else 100, 'shflexR': 30 if hands != 'ground' else 100, 'elbL': 30, 'elbR': 30}
    nm = 'half-kneel (%s foot forward), hands: %s, back toes %s' % ('left' if front == 'L' else 'right', hands, back)
    return dict(name=nm, contacts=C, targets=T, q0=q0, m_req=0.03 * b['H'], skip_clear=('ft' + bk, 'to' + bk) if back == 'laid' else ())


def halfkneel_up(b, h, front='L', hands='none'):
    lA, lL, foot = U(b)
    bk = 'R' if front == 'L' else 'L'
    sgf = 1 if front == 'L' else -1
    C = [('sole', front), ('ball', bk), ('toes', bk)]
    T = [pt('P', 2, h * b['H'] - 0.004 * b['H'], h * b['H'] + 0.004 * b['H'], w=80.0),
         pt('an' + front, 0, 0.12 * lL, 0.55 * lL, rel='P'), pt('an' + front, 1, *(sorted([sgf * b['wH'], sgf * 2.6 * b['wH']]))),
         foot_yaw(front, -0.2, 0.35), pt('ball' + bk, 0, -0.9 * lL, -0.15 * lL, rel='P'),
         pt('ball' + bk, 1, *(sorted([-sgf * 0.5 * b['wH'], -sgf * 2.4 * b['wH']]))), head_look(-30, 10)]
    if hands == 'thigh':
        C.append(('handthigh', front, (front, 0.8)))
        T.append(hand_on_thigh(b, front, front))
    q0 = {'z': h * b['H'], 'pitch': 25, 'hflex' + front: 70, 'knee' + front: 70, 'dorsi' + front: 15,
          'hflex' + bk: -10, 'knee' + bk: 60, 'dorsi' + bk: -20, 'toe' + bk: 40, 'shflexL': 30, 'shflexR': 30, 'elbL': 30, 'elbR': 30}
    return dict(name='rising from the half-kneel h=%.2f, hands: %s' % (h, hands), contacts=C, targets=T, q0=q0, m_req=0.03 * b['H'])


def stoopknees(b, kf):
    lA, lL, foot = U(b)
    C = [('sole', 'L'), ('sole', 'R'), ('handthigh', 'L', ('L', 0.85)), ('handthigh', 'R', ('R', 0.85))]
    T = [qband('kneeL', kf - 4, kf + 4, 3.0), qband('kneeR', kf - 4, kf + 4, 3.0), pt('anL', 1, b['wH'], 2.4 * b['wH']),
         pt('anR', 1, -2.4 * b['wH'], -b['wH']), rel_pts(['anL'], ['anR'], 0, -0.03 * b['H'], 0.03 * b['H']),
         hand_on_thigh(b, 'L', 'L', 0.65, 0.97), hand_on_thigh(b, 'R', 'R', 0.65, 0.97), head_look(-45, 5),
         sym(['shflex', 'elb', 'hflex', 'knee', 'dorsi'])]
    return dict(name='stoop, hands on the knees, knees %d' % kf, contacts=C, targets=T,
                q0=dict(z=0.8 * b['H'], pitch=40, tflex=15, hflexL=60, hflexR=60, kneeL=kf, kneeR=kf, dorsiL=10, dorsiR=10,
                        shflexL=40, shflexR=40, elbL=10, elbR=10), m_req=0.1 * foot)


def situp(b, ang, hands=False):
    lA, lL, foot = U(b)
    C = [('seat', None), ('calf', 'L'), ('calf', 'R')]
    T = [trunk_elev(ang - 3, ang + 3, 4.0), qband('kneeL', 0, 8), qband('kneeR', 0, 8), sym(['hflex', 'knee', 'shflex', 'elb'])]
    if hands:
        C += [('hand', 'L'), ('hand', 'R')]
        T += [rel_pts(['wrL', 'wrR'], ['P'], 0, -0.55 * lA, 0.05 * lA), pt('wrL', 1, b['wH'] + 0.1 * lA, b['wH'] + 0.7 * lA),
              pt('wrR', 1, -b['wH'] - 0.7 * lA, -b['wH'] - 0.1 * lA)]
    else:
        T += [qband('shflexL', 60, 110), qband('shflexR', 60, 110)]
    return dict(name='sitting up, the trunk %d deg from the ground, %s' % (ang, 'hands behind' if hands else 'arms reaching forward'),
                contacts=C, targets=T, skip_clear=('ftL', 'ftR', 'toL', 'toR'),
                q0=dict(z=b['gH'], pitch=-(90 - ang), hflexL=ang, hflexR=ang, shflexL=80 if not hands else -30, shflexR=80 if not hands else -30,
                        elbL=10, elbR=10, dorsiL=-10, dorsiR=-10), m_req=0.02 * b['H'])


def press(b):
    lA, lL, foot = U(b)
    C = [('hand', 'L'), ('hand', 'R'), ('knee', 'L'), ('knee', 'R'), ('calf', 'L'), ('calf', 'R')]
    T = [pt('wrL', 0, -0.25 * lA, 0.1 * lA, rel='shL'), pt('wrR', 0, -0.25 * lA, 0.1 * lA, rel='shR'),
         pt('wrL', 1, b['wS'] - 0.05 * lA, b['wS'] + 0.3 * lA), pt('wrR', 1, -b['wS'] - 0.3 * lA, -b['wS'] + 0.05 * lA),
         pt('neck', 2, 0.55 * lA, 9.0), head_look(-60, 0), qband('kneeL', 0, 20), qband('kneeR', 0, 20),
         qband('dorsiL', -50, -30), qband('dorsiR', -50, -30), sym(['shflex', 'elb', 'hflex', 'knee'])]
    return dict(name='pushed up from the front (chest off the ground)', contacts=C, targets=T, skip_clear=('ftL', 'toL', 'ftR', 'toR', 'hips'),
                q0=dict(z=b['gH'] + 0.05, pitch=75, tflex=-10, shflexL=70, shflexR=70, elbL=30, elbR=30, hflexL=-10, hflexR=-10,
                        kneeL=5, kneeR=5, dorsiL=-40, dorsiR=-40), m_req=0.04 * b['H'])


def forearms(b):
    lA, lL, foot = U(b)
    C = [('forearm', 'L'), ('forearm', 'R'), ('seat', None), ('knee', 'L'), ('knee', 'R'), ('calf', 'L'), ('calf', 'R')]
    T = [pt('elL', 0, -0.2 * b['lu'], 0.25 * b['lu'], rel='shL'), pt('elR', 0, -0.2 * b['lu'], 0.25 * b['lu'], rel='shR'),
         pt('elL', 1, b['wS'] - 0.1 * b['lu'], b['wS'] + 0.3 * b['lu']), pt('elR', 1, -b['wS'] - 0.3 * b['lu'], -b['wS'] + 0.1 * b['lu']),
         rel_pts(['wrL'], ['elL'], 0, 0.7 * b['lf'], 9.0), rel_pts(['wrR'], ['elR'], 0, 0.7 * b['lf'], 9.0),
         head_look(-30, 15), qband('kneeL', 0, 20), qband('kneeR', 0, 20), qband('dorsiL', -50, -30), qband('dorsiR', -50, -30),
         sym(['shflex', 'elb', 'hflex', 'knee'])]
    return dict(name='on its forearms', contacts=C, targets=T, skip_clear=('ftL', 'toL', 'ftR', 'toR', 'hips', 'uaL', 'uaR'),
                q0=dict(z=b['gH'] + 0.03, pitch=75, tflex=-10, shflexL=80, shflexR=80, elbL=90, elbR=90, hflexL=-5, hflexR=-5,
                        kneeL=5, kneeR=5, dorsiL=-40, dorsiR=-40), m_req=0.04 * b['H'])


def longsit(b):
    lA, lL, foot = U(b)
    C = [('seat', None), ('calf', 'L'), ('calf', 'R'), ('hand', 'L'), ('hand', 'R')]
    T = [trunk_elev(55, 85), qband('kneeL', 0, 10), qband('kneeR', 0, 10),
         rel_pts(['wrL', 'wrR'], ['P'], 0, -0.6 * lA, -0.05 * lA), pt('wrL', 1, b['wH'] + 0.1 * lA, b['wH'] + 0.7 * lA),
         pt('wrR', 1, -b['wH'] - 0.7 * lA, -b['wH'] - 0.1 * lA), sym(['hflex', 'knee', 'shflex', 'elb'])]
    return dict(name='sitting, legs out, hands behind', contacts=C, targets=T, skip_clear=('ftL', 'ftR', 'toL', 'toR'),
                q0=dict(z=b['gH'], pitch=-20, hflexL=70, hflexR=70, shflexL=-30, shflexR=-30, elbL=5, elbR=5), m_req=0.03 * b['H'])


def onekneel_toes(b, lift='L'):
    on = 'R' if lift == 'L' else 'L'
    st = onekneel(b, lift)
    st['name'] = 'one knee and its toes, the other leg lifted'
    st['contacts'] = [('knee', on), ('toes', on)]
    st['targets'] = [t for t in st['targets']][:-2] + [head_look(-20, 10)]
    st['q0'] = dict(st['q0'], **{'dorsi' + on: -10, 'toe' + on: 50})
    st['skip_clear'] = ('ft' + on,)
    return st


def ready(st, kinds, m=None, b=None):
    st = dict(st)
    st['ready'] = tuple(kinds)
    if m is not None:
        st['m_ready'] = m
    st['name'] = st['name'] + ' [ready to let go of: %s]' % ','.join(kinds)
    return st


def squat_hands_ready(b, h):
    return ready(squat(b, h, False, True), ['hand'], 0.08 * (b['hl'] + b['bl']))


def crouchkneel_ready(b, front='L'):
    return ready(halfkneel(b, front, 'ground'), ['hand'], 0.02 * b['H'])


def bear_ready(b, D):
    return ready(bear(b, D), ['hand'], 0.08 * (b['hl'] + b['bl']))


def quad_ready_kneel(b):
    # hands and knees, its weight back over its knees: ready to come up to a tall kneel
    return ready(quad(b), ['hand'], 0.01 * b['H'])
