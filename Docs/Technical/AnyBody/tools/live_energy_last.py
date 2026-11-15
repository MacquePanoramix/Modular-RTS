import sys, re
sys.stdout.reconfigure(encoding='utf-8')
# The last steps of a trace, link by link: which joints were counted to give energy, and which links turned fastest.
PART = re.compile(r'(\S+) v (\S+) w (\S+) g (\S+)')


def f(s):
    try:
        return float(s)
    except Exception:
        return float('nan')


for path in sys.argv[2:]:
    n = int(sys.argv[1])
    lines = open(path, encoding='utf-8').read().splitlines()
    print('==', path.split('/')[-1])
    for ln in lines[-n:]:
        head, _, parts = ln.partition(' | ')
        print(' ', head[:150])
        rows = []
        for p in parts.split('; '):
            m = PART.match(p.strip().rstrip(';'))
            if m:
                rows.append((m.group(1), f(m.group(2)), f(m.group(3)), f(m.group(4))))
        byg = sorted(rows, key=lambda r: -abs(r[3]))
        byw = sorted(rows, key=lambda r: -r[2])
        print('    most given:', ', '.join('%s v%.1f w%.0f g%+.1f' % r for r in byg[:6]))
        print('    fastest   :', ', '.join('%s v%.1f w%.0f g%+.1f' % r for r in byw[:5]))
