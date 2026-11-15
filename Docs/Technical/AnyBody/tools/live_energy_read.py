import sys, re, os, glob, csv, math
sys.stdout.reconfigure(encoding='utf-8')
# The long run's energy traces read: for each blow-up, which part whirled first, whether its joint gave the energy
# (g, the drive's and the engine's counted work) or the step added it (gained with little given), and in what state.
RUN = sys.argv[1] if len(sys.argv) > 1 else 'D:/Dev/WG/TestRuns/CoverLive/cc_en24'

LINE = re.compile(r'^frame (\d+) t ([\d.]+) \[(.*?)\] held (\S+) given (\S+) gained (\S+) \| (.*)$')
PART = re.compile(r'^(\S+) v (\S+) w (\S+) g (\S+)$')


def num(s):
    try:
        return float(s)
    except Exception:
        return float('nan')


def read_trace(path):
    head = ''
    steps = []
    with open(path, encoding='utf-8') as fh:
        for i, ln in enumerate(fh):
            ln = ln.rstrip('\n')
            if i == 0:
                head = ln
                continue
            m = LINE.match(ln)
            if not m:
                continue
            parts = {}
            for p in m.group(7).split('; '):
                pm = PART.match(p.strip().rstrip(';'))
                if pm:
                    parts[pm.group(1)] = (num(pm.group(2)), num(pm.group(3)), num(pm.group(4)))
            steps.append(dict(frame=int(m.group(1)), t=float(m.group(2)), state=m.group(3), held=num(m.group(4)),
                              given=num(m.group(5)), gained=num(m.group(6)), parts=parts))
    return head, steps


events = {}
blow = {}
with open(os.path.join(RUN, 'events.txt'), encoding='utf-8') as fh:
    for ln in fh:
        if 'LIVE_BLOWUP' in ln and 'LIVE_COURSE' not in ln:
            f = ln.split()
            blow[int(f[0])] = (int(f[1]), ln.strip())
        if 'LIVE_COURSE' in ln:
            events[int(ln.split()[0])] = ln.strip()

courses = sorted(events)
print('courses ended', len(courses), '| blow-ups', len(blow), sorted(blow))
for c in courses:
    tag = 'BLOWUP' if c in blow else ''
    print(' ', events[c][events[c].find('LIVE_COURSE'):][:170], tag)

traces = sorted(glob.glob(os.path.join(RUN, 'energy_trace_c*_f*.txt')),
                key=lambda p: (int(re.search(r'_c(-?\d+)_', p).group(1)), int(re.search(r'_f(\d+)', p).group(1))))
print()
print('traces', len(traces))
first_whirl = {}
for p in traces:
    c = int(re.search(r'_c(-?\d+)_', p).group(1))
    fr = int(re.search(r'_f(\d+)', p).group(1))
    head, steps = read_trace(p)
    if not steps:
        print('  empty', os.path.basename(p))
        continue
    last = steps[-1]
    fast = max(last['parts'].items(), key=lambda kv: kv[1][1] if math.isfinite(kv[1][1]) else 1e30)
    name = fast[0]
    # when did this part first pass 15 and 30 rad/s inside the ring; who else was fast then
    w = [s['parts'].get(name, (0, 0, 0))[1] for s in steps]
    g = [s['parts'].get(name, (0, 0, 0))[2] for s in steps]
    i15 = next((i for i, x in enumerate(w) if x > 15), None)
    i30 = next((i for i, x in enumerate(w) if x > 30), None)
    gained = [s['gained'] for s in steps]
    given = [s['given'] for s in steps]
    k = max(range(len(steps)), key=lambda i: gained[i] if math.isfinite(gained[i]) else -1e30)
    states = []
    for s in steps:
        if not states or states[-1] != s['state']:
            states.append(s['state'])
    guard = blow.get(c, (None,))[0] == fr
    kind = 'GUARD' if guard else 'past 30'
    print('c%-3d f%-6d %-7s %-11s w now %7.1f | first >15 at step %s, >30 at step %s of %d | states %s' % (
        c, fr, kind, name, w[-1], i15, i30, len(steps), ' > '.join(states)))
    tail = range(max(0, len(steps) - 8), len(steps))
    print('     last 8 steps, the fast part: w', [round(w[i], 1) for i in tail])
    print('                         its g (J)', [round(g[i], 2) for i in tail])
    print('     whole body: given', [round(given[i], 2) for i in tail])
    print('                gained', [round(gained[i], 2) for i in tail])
    print('     most gained in one step %.2f J at frame %d (%s); given that step %.2f; the parts turning fastest then: %s' % (
        gained[k], steps[k]['frame'], steps[k]['state'], given[k],
        ', '.join('%s %.0f' % (n, v[1]) for n, v in sorted(steps[k]['parts'].items(), key=lambda kv: -kv[1][1] if math.isfinite(kv[1][1]) else -1e30)[:4])))
    if c not in first_whirl:
        first_whirl[c] = (fr, name)

# Over all frames measured: the energy gained per state (frames.csv's e_gain), the blown-up courses apart.
print()
rows_by = {}
with open(os.path.join(RUN, 'frames.csv'), newline='', encoding='utf-8') as fh:
    for r in csv.DictReader(fh):
        try:
            c = int(r['course'])
        except Exception:
            continue
        e = num(r.get('e_gain', 'nan'))
        fz = num(r.get('fastest_rads', 'nan'))
        st = r['state']
        key = (st, c in blow)
        d = rows_by.setdefault(key, [])
        d.append((e, fz, r.get('fastest_part', '')))
for key in sorted(rows_by):
    v = [x[0] for x in rows_by[key] if math.isfinite(x[0])]
    fz = [x[1] for x in rows_by[key] if math.isfinite(x[1])]
    if not v:
        continue
    v.sort(); fz.sort()
    q = lambda a, p: a[min(len(a) - 1, int(p * len(a)))]
    parts = {}
    for x in rows_by[key]:
        if math.isfinite(x[1]) and x[1] > 20:
            parts[x[2]] = parts.get(x[2], 0) + 1
    print('%-22s blown %-5s rows %6d | e_gain median %6.2f p90 %6.2f max %8.2f | fastest p50 %5.1f p99 %6.1f | parts past 20 rad/s %s' % (
        key[0], key[1], len(v), q(v, 0.5), q(v, 0.9), v[-1], q(fz, 0.5), q(fz, 0.99),
        dict(sorted(parts.items(), key=lambda kv: -kv[1])[:5])))
