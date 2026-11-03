# r of key stances, bodies as columns ('x' = cannot be held, '?' = did not fit (cost > 2)); release r/margin for let-gos.
import json, sys, math
R = []
for f in sys.argv[1].split(','):
    R += json.load(open(f))
bodies = []
for r in R:
    if r['body'] not in bodies:
        bodies.append(r['body'])
ROWS = [('stand', ()), ('press', ()), ('forearms', ()), ('quad', ()), ('quad', (True,)), ('tallkneel', ()),
        ('bear', (0.8,)), ('bear', (0.3,)), ('bear_ready', (0.25,)), ('squat', (0.35,)), ('squat', (0.5,)), ('squat', (0.4, True)),
        ('squat_hands_ready', (0.35,)), ('squat_hands_ready', (0.5,)), ('halfkneel', ('L', 'none')), ('halfkneel', ('L', 'ground')),
        ('crouchkneel_ready', ('L',)), ('halfkneel_up', (0.55, 'L', 'none')), ('halfkneel_up', (0.55, 'R', 'none')),
        ('halfkneel_up', (0.55, 'L', 'thigh')), ('stoopknees', (30,)), ('situp', (15,)), ('situp', (35,)), ('situp', (25, True)), ('longsit', ())]
short = lambda b: (b.replace('Toddler-like (Small, head x1.3, legs x0.85)', 'Toddler').replace('Giant (Long x1.6, strength ~ L^3)', 'Giant')
                   .replace('Small, arms by thickness', 'SmArmThk').replace('Long, arms x0.65', 'LgArm.65').replace('Long, stiff ankles (dorsiflexion 5)', 'LgAnk5')
                   .replace('Round, stiff knees (100)', 'RdKnee100').replace('Heavy trunk (Round, trunk+hips x1.5, S0.7, lean 30)', 'HvTrunk')
                   .replace('Long, right leg 0.4', 'LgRleg.4'))[:9]
print('%-26s' % 'stance' + ''.join('%10s' % short(b) for b in bodies))
for (fam, args) in ROWS:
    cells = []
    for b in bodies:
        e = next((r for r in R if r['body'] == b and r['family'] == fam and tuple(r['args']) == args), None)
        if e is None:
            cells.append('%10s' % '')
            continue
        if e['cost'] > 2.0:
            cells.append('%10s' % '?')
            continue
        rr = e['r']
        s = 'x' if not math.isfinite(rr) else '%.2f' % rr
        if 'release_r' in e and fam in ('bear_ready', 'squat_hands_ready', 'crouchkneel_ready'):
            rel = e['release_r']
            s += '/' + ('x' if not math.isfinite(rel) or e['release_sp_margin'] <= 0 else '%.2f' % rel)
        cells.append('%10s' % s)
    print('%-26s' % (fam + ' ' + ' '.join(str(a) for a in args))[:26] + ''.join(cells))
