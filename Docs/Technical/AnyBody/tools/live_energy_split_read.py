# Reads the ankle split's detail files (-liveEnergy 2 in CoverRingLiveTests, energy_detail_c<course>_f<frame>.txt).
# For each event (a link first past 30 rad/s in a try, or the guard's blow-up) it prints the last steps of both ankles
# and toes, the links' split of their given work, and the contacts.
#
# Per ankle step:
# - "tgt moved": the drive target's turn since the ring's step before, the short way. The target triples are read as
#   rotation vectors in degrees, as OwnRiser.Aims builds them (angle times axis). "(over N)" when the ring skips steps.
# - "asked moved": how far the riser's asked turn moved (the riser line's own number).
# - "added": the joint target less the riser's "target asked" (wayAsks, before drivenNow), per axis: what was added on
#   top of the riser's ask, such as the learnt policy's drivenNow. "(turn ...)": how far that addition turns the target.
# - "|tgt|": the target's size from the made pose. AnkleCan's 110 degrees is measured from stood, which the files do
#   not carry, so this is not the clamp's number.
# - "off": the size of the writer's own per-axis off. "going": the size of the per-axis joint speeds.
#   "force": the largest over the axes, as the writer reads it (after the step), against the force limit.
# For a toe's blow-up, the same leg's foot is read as well as the toe: the ankle can put in the energy the toe shows.
# Contacts: every link's, printed when pressed in more than 1 mm or doing at least 0.5 J.
#
# Usage: python -I live_energy_split_read.py <run folder> [steps=4] [--all]
# Written and checked November 15, 2026 (Docs/Validation.md, "The ankle split in the live courses").
import math, os, re, sys, glob
sys.stdout.reconfigure(encoding='utf-8')

def q(v):
    a = math.sqrt(sum(c * c for c in v))
    if a < 1e-9: return (1.0, 0.0, 0.0, 0.0)
    h = math.radians(a) / 2; s = math.sin(h) / a
    return (math.cos(h), v[0] * s, v[1] * s, v[2] * s)

def mul(a, b):
    w1, x1, y1, z1 = a; w2, x2, y2, z2 = b
    return (w1*w2 - x1*x2 - y1*y2 - z1*z2, w1*x2 + x1*w2 + y1*z2 - z1*y2,
            w1*y2 - x1*z2 + y1*w2 + z1*x2, w1*z2 + x1*y2 - y1*x2 + z1*w2)

def turn(u, v):
    # The turn from rotation vector u to rotation vector v (degrees), the short way.
    a = q(u); b = q(v)
    r = mul((a[0], -a[1], -a[2], -a[3]), b)
    return math.degrees(2 * math.acos(min(1.0, abs(r[0]))))

def size(v): return math.sqrt(sum(c * c for c in v))

NAN = float('nan')
# A lone '-' is the writer's "none"; it reads as NaN.
NUM = r'[-+]?(?:\d+\.?\d*(?:E[-+]?\d+)?|NaN|Infinity|-)'
re_head = re.compile(r'^ENERGY_TRACE course (\d+) frame (\d+) t ([\d.]+): (.*)$')
re_frame = re.compile(r'^frame (\d+) t ([\d.]+) \[([^\]]*)\] given (' + NUM + r') gained (' + NUM + r') split off (' + NUM + r') J')
re_split = re.compile(r'(\w+) p (' + NUM + r') r (' + NUM + r') k (' + NUM + r') o (' + NUM + r') d (' + NUM + r')')
re_axis = re.compile(r'(x|y|z) (\w+) target (' + NUM + r') at (' + NUM + r') deg \(off (' + NUM + r')\) going (' + NUM +
                     r') rad/s k (' + NUM + r') c (' + NUM + r') most (' + NUM + r') force (' + NUM + r')(?: limits (' + NUM +
                     r') \((' + NUM + r')\) to (' + NUM + r') \((' + NUM + r')\))?')
# The writer prints "ShinL n 2(2 body) J ..." with no space before the bodies, and "Head n 2 J ..." without bodies.
re_contact = re.compile(r'(\w+) n (\d+)(?: ?\(([^)]*)\))? J \((' + NUM + r'), (' + NUM + r'), (' + NUM + r')\) N s deepest (' +
                        NUM + r') m work ~(' + NUM + r') J')
re_boot = re.compile(r'(Foot[LR]) ([^;|]*?), asked turn moved (' + NUM + r') deg, target asked \((' + NUM + r'), (' + NUM + r'), (' +
                     NUM + r')\) deg')

# Each leg's links lead to its foot (the ankle).
LEG = {'FootL': 'FootL', 'Toeleft': 'FootL', 'ShinL': 'FootL', 'ThighL': 'FootL',
       'FootR': 'FootR', 'Toeright': 'FootR', 'ShinR': 'FootR', 'ThighR': 'FootR'}

def f(s):
    try: return float(s)
    except Exception: return NAN

def read(path):
    head = None; steps = []; cur = None
    for line in open(path, encoding='utf-8', errors='replace'):
        line = line.rstrip('\r\n')
        m = re_head.match(line)
        if m: head = (int(m.group(1)), int(m.group(2)), float(m.group(3)), m.group(4)); continue
        m = re_frame.match(line)
        if m:
            cur = {'frame': int(m.group(1)), 't': float(m.group(2)), 'state': m.group(3), 'given': f(m.group(4)),
                   'gained': f(m.group(5)), 'splitOff': f(m.group(6)), 'split': {}, 'joint': {}, 'contacts': {},
                   'boots': {}, 'riser': None}
            steps.append(cur); continue
        if cur is None: continue
        s = line.strip()
        if s.startswith('split |'):
            for m in re_split.finditer(s):
                cur['split'][m.group(1)] = tuple(f(m.group(i)) for i in range(2, 7))
        elif s.startswith('joint '):
            name = s.split()[1]
            axes = {}
            for m in re_axis.finditer(s):
                axes[m.group(1)] = {'motion': m.group(2), 'target': f(m.group(3)), 'at': f(m.group(4)), 'off': f(m.group(5)),
                                    'going': f(m.group(6)), 'k': f(m.group(7)), 'c': f(m.group(8)), 'most': f(m.group(9)),
                                    'force': f(m.group(10)), 'low': f(m.group(11)) if m.group(11) else None,
                                    'inLow': f(m.group(12)) if m.group(12) else None,
                                    'high': f(m.group(13)) if m.group(13) else None,
                                    'inHigh': f(m.group(14)) if m.group(14) else None}
            cur['joint'][name] = axes
        elif s.startswith('contacts |'):
            for m in re_contact.finditer(s):
                cur['contacts'][m.group(1)] = {'n': int(m.group(2)), 'bodies': m.group(3),
                                               'J': (f(m.group(4)), f(m.group(5)), f(m.group(6))),
                                               'deep': f(m.group(7)), 'work': f(m.group(8))}
        elif s.startswith('riser |'):
            cur['riser'] = s[len('riser | '):].split(' | ')[0]
            for m in re_boot.finditer(s):
                cur['boots'][m.group(1)] = {'role': m.group(2), 'moved': f(m.group(3)),
                                            'asked': (f(m.group(4)), f(m.group(5)), f(m.group(6)))}
    return head, steps

def vec(axes, key):
    return tuple(axes[a][key] if a in axes else 0.0 for a in 'xyz')

def ankle_rows(steps, foot):
    rows = []; prev = None
    for st in steps:
        ax = st['joint'].get(foot)
        if not ax: prev = None; continue
        tgt = vec(ax, 'target')
        boot = st['boots'].get(foot)
        added = tuple(tgt[i] - boot['asked'][i] for i in range(3)) if boot else (NAN, NAN, NAN)
        rows.append({'frame': st['frame'], 'tgt_size': size(tgt), 'added_turn': turn(boot['asked'], tgt) if boot else NAN,
                     'tgt_moved': turn(prev[1], tgt) if prev is not None else NAN,
                     'gap': st['frame'] - prev[0] if prev is not None else 0,
                     'asked_moved': boot['moved'] if boot else NAN, 'role': boot['role'] if boot else '-',
                     'added': size(added), 'off': size(vec(ax, 'off')), 'going': size(vec(ax, 'going')),
                     'force': max(abs(ax[a]['force']) for a in ax), 'most': max(ax[a]['most'] for a in ax),
                     'motion': ''.join(ax[a]['motion'][0] for a in 'xyz' if a in ax)})
        prev = (st['frame'], tgt)
    return rows

def toe_rows(steps, toe):
    rows = []
    for st in steps:
        ax = st['joint'].get(toe)
        if not ax or 'x' not in ax: continue
        a = ax['x']
        past = 0.0
        if a['inLow'] is not None and a['inLow'] < 0: past = a['inLow']
        if a['inHigh'] is not None and a['inHigh'] < 0: past = a['inHigh']
        rows.append({'frame': st['frame'], 'at': a['at'], 'past': past, 'going': a['going']})
    return rows

def split_of(last, part):
    # A link's split summed over the ring's last steps, and its largest single step's parts.
    sums = [0.0] * 5; big = (0.0, None, None)
    for st in last:
        sp = st['split'].get(part)
        if not sp: continue
        for i in range(5): sums[i] += sp[i] if sp[i] == sp[i] else 0.0
        g = sum(x for x in sp if x == x)
        if abs(g) > abs(big[0]): big = (g, st['frame'], sp)
    return sums, big

def shown(c):
    return (c['deep'] == c['deep'] and c['deep'] < -0.001) or (c['work'] == c['work'] and abs(c['work']) >= 0.5)

def main():
    folder = sys.argv[1]
    n = int(sys.argv[2]) if len(sys.argv) > 2 and sys.argv[2].isdigit() else 4
    show_all = '--all' in sys.argv
    files = sorted(glob.glob(os.path.join(folder, 'energy_detail_c*_f*.txt')),
                   key=lambda p: tuple(int(x) for x in re.findall(r'_c(\d+)_f(\d+)', p)[0]))
    summary = []
    for path in files:
        head, steps = read(path)
        if not head or not steps: print('unread', os.path.basename(path)); continue
        course, frame, t, what = head
        blow = what.startswith('gone bad')
        m = re.search(r'(\w+) (?:turns at|turning at) ([\d.]+)', what)
        link = m.group(1) if m else '?'; speed = float(m.group(2)) if m else NAN
        last = steps[-n:]
        state = last[-1]['state']
        mine = [LEG[link]] if link in LEG else ['FootL', 'FootR']
        # The ankles: on the trigger's leg, the largest target turn in the last n steps, and the other foot's.
        jump = (0.0, '-', NAN, 0, NAN); other = (0.0, '-')
        offmax = (0.0, '-'); addmax = (0.0, '-')
        for foot in ('FootL', 'FootR'):
            for r in ankle_rows(steps, foot)[-n:]:
                if foot in mine:
                    if r['tgt_moved'] == r['tgt_moved'] and r['tgt_moved'] > jump[0]:
                        jump = (r['tgt_moved'], foot, r['asked_moved'], r['frame'], r['added'])
                    if r['off'] > offmax[0]: offmax = (r['off'], foot)
                    if r['added'] == r['added'] and r['added'] > addmax[0]: addmax = (r['added'], foot)
                elif r['tgt_moved'] == r['tgt_moved'] and r['tgt_moved'] > other[0]:
                    other = (r['tgt_moved'], foot)
        toe = (0.0, '-')
        for t_ in ('Toeleft', 'Toeright'):
            for r in toe_rows(last, t_):
                if r['past'] < toe[0]: toe = (r['past'], t_)
        parts = [link] + [x for x in mine if x != link and link in LEG]
        splits = [(p,) + split_of(last, p) for p in parts]
        cont = []; deepest = (0.0, '-', 0)
        for st in last:
            for part, c in st['contacts'].items():
                if shown(c): cont.append((st['frame'], part, c['n'], c['bodies'] or '-', c['J'][1], c['deep'], c['work']))
                if c['deep'] == c['deep'] and c['deep'] < deepest[0]: deepest = (c['deep'], part, st['frame'])
        summary.append({'course': course, 'frame': frame, 'blow': blow, 'link': link, 'speed': speed, 'state': state,
                        'jump': jump, 'other': other, 'offmax': offmax, 'addmax': addmax, 'toe': toe, 'splits': splits,
                        'deepest': deepest})
        if show_all or blow:
            print('== c%d f%d %s %s %.1f rad/s [%s]' % (course, frame, 'BLOWUP' if blow else 'past30', link, speed, state))
            for foot in ('FootL', 'FootR'):
                for r in ankle_rows(steps, foot)[-n:]:
                    over = (' (over %d)' % r['gap']) if r['gap'] > 1 else ''
                    print('   %s f%d %-22s |tgt| %5.1f  tgt moved %6.1f%s  asked moved %6.1f  added %5.1f (turn %5.1f)'
                          '  off %5.1f  going %6.1f  force %6.1f/%5.1f  %s' % (
                              foot, r['frame'], r['role'][:22], r['tgt_size'], r['tgt_moved'], over, r['asked_moved'],
                              r['added'], r['added_turn'], r['off'], r['going'], r['force'], r['most'], r['motion']))
            for t_ in ('Toeleft', 'Toeright'):
                for r in toe_rows(last, t_):
                    if r['past'] < 0 or abs(r['going']) > 15:
                        print('   %s f%d at %6.1f past limit %6.1f going %6.1f' % (t_, r['frame'], r['at'], r['past'], r['going']))
            for st in last:
                ps = []
                for part in parts + ['FootL', 'FootR', 'Toeleft', 'Toeright', 'ShinL', 'ShinR', 'ThighL', 'ThighR']:
                    sp = st['split'].get(part)
                    if sp and sum(abs(x) for x in sp if x == x) > 1.0:
                        ps.append('%s p%+.1f r%+.1f k%+.1f o%+.1f d%+.1f' % ((part,) + sp))
                ps = list(dict.fromkeys(ps))
                print('   f%d given %+.1f gained %+.1f | %s' % (st['frame'], st['given'], st['gained'], '; '.join(ps)))
            for c in cont:
                print('   contact f%d %s n %d (%s) up %.2f N s deepest %+.4f m work ~%+.2f J' % c)
            print('   riser:', last[-1]['riser'])
    print()
    print('SUMMARY (%d events; ankle numbers on the trigger\'s leg, both legs when the trigger is not on a leg)' % len(summary))
    for r in summary:
        j = r['jump']
        ss = []
        for part, sums, big in r['splits']:
            bs = ('p%+.1f r%+.1f k%+.1f o%+.1f d%+.1f at f%d' % (tuple(big[2]) + (big[1],))) if big[2] else '-'
            ss.append('%s big %+.1f: %s' % (part, big[0], bs))
        d = r['deepest']
        print('c%-2d f%-5d %-6s %-9s %6.1f [%s] | ankle target moved %5.1f (%s f%s, asked %.1f, added %.1f; other foot %.1f) |'
              ' most off %5.1f | most added %5.1f | toe past %5.1f %s | deepest %+.1f mm %s | %s' % (
                  r['course'], r['frame'], 'BLOW' if r['blow'] else 'p30', r['link'], r['speed'], r['state'], j[0], j[1], j[3],
                  j[2], j[4], r['other'][0], r['offmax'][0], r['addmax'][0], r['toe'][0], r['toe'][1], d[0] * 1000, d[1],
                  ' ; '.join(ss)))
    # Every ankle step where the target turned more than 30 degrees, in every event's ring: what was asked.
    print()
    print('ANKLE TARGET JUMPS over 30 deg (any step of any ring; a step seen in two rings is shown once)')
    seen = set()
    for path in files:
        head, steps = read(path)
        if not head: continue
        for foot in ('FootL', 'FootR'):
            for r in ankle_rows(steps, foot):
                if r['tgt_moved'] == r['tgt_moved'] and r['tgt_moved'] > 30 and (head[0], r['frame'], foot) not in seen:
                    seen.add((head[0], r['frame'], foot))
                    st = next(s for s in steps if s['frame'] == r['frame'])
                    over = (' (over %d)' % r['gap']) if r['gap'] > 1 else ''
                    print('c%-2d f%-5d %s [%s] target moved %5.1f%s; asked moved %.1f; added %.1f; %s; |target| %.1f' % (
                        head[0], r['frame'], foot, st['state'], r['tgt_moved'], over, r['asked_moved'], r['added'], r['role'],
                        r['tgt_size']))

main()
