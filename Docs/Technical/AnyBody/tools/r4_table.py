import re, sys

# r4_table.py <log> [mode] [filter...]
# mode: flat  = the flat cases (nothing asked, 2/3 sideways, 2/3 towards the toe): fidelity table
#       rest  = every other case: stability table
#       all   = both
log = sys.argv[1]
mode = sys.argv[2] if len(sys.argv) > 2 else 'all'
wants = sys.argv[3:]
num = r'(-?[0-9.]+(?:E[-+]?[0-9]+)?|NaN|-?Infinity)'
pat = re.compile(
    r'FACTS R4 \((\w+)\) (.*?) \| (.*?) \| (.*?) \| asked ' + num + r' N m \| after 0\.5 s: tipped ' + num +
    r' deg, turned ' + num + r' rad/s, weight moved ' + num + r' mm, drive gave ' + num + r' N m, ground bore ' + num +
    r' N at ' + num + r' across ' + num + r' along mm \(the torque puts it at ' + num + r', ' + num +
    r'\) \| over all: turned at most ' + num + r' rad/s, drive gave at most ' + num +
    r' J a step, held at most ' + num + r' J more than given in a step and ' + num +
    r' J in sum \| at the end: turning ' + num + r' rad/s, ' + num + r' deg from flat on the ground, ' + num +
    r" deg from its ankle.s zero(?: \| weight slid " + num + r' mm sideways)?(?: \| held still, the law gives ' + num + r' N m)?' +
    r'(?: \| in its last half second it turned ' + num + r' deg)?')
short = [
    ('made bent 22 degrees at its ankle under an upright weight, nothing asked', 'bent 22, nothing'),
    ('on its edge under a weight leant 22 degrees, nothing asked', 'edge, nothing'),
    ('on its edge under a weight leant 22 degrees, all its strength further onto the edge', 'edge, all onto edge'),
    ('on its edge under a weight leant 22 degrees, all its strength towards the toe', 'edge, all to toe'),
    ('dropped flat from 5 cm, all its strength towards the toe', 'drop 5, all to toe'),
    ('dropped from 5 cm under a weight leant 22 degrees, all its strength towards the toe', 'drop 5 leant, all to toe'),
    ('dropped flat from 2 cm, nothing asked', 'drop 2, nothing'),
    ('held 34 cm off the ground, all its strength towards the toe and sideways at once', 'air, toe+side'),
    ('held 34 cm off the ground, all its strength towards the toe', 'air, toe'),
    ('flat, all its strength sideways', 'flat, all sideways'),
    ('flat, 2/3 of what the sole bears sideways', 'flat 2/3 side'),
    ('flat, 2/3 of what the sole bears towards the toe', 'flat 2/3 toe'),
    ('flat, nothing asked', 'flat, nothing'),
]
seen = 0
for line in open(log, encoding='utf-8', errors='replace'):
    m = pat.search(line)
    if not m:
        continue
    (solver, miner, ankle, case, asked, tip, turned, moved, gave, bore, across, along, wantX, wantZ,
     top, work, elseStep, elseSum, endTurn, endFlat, endBent, slid, law, endMoved) = m.groups()
    isFlat = case.startswith('flat, 2/3') or case == 'flat, nothing asked'
    if mode == 'flat' and not isFlat:
        continue
    if mode == 'rest' and isFlat:
        continue
    key = miner + ' | ' + ankle + ' | ' + case
    if any(w not in key for w in wants):
        continue
    name = case
    for long, brief in short:
        if case == long:
            name = brief
            break
    seen += 1
    if mode == 'flat' or (mode == 'all' and isFlat):
        print(f'{solver} {miner} | {ankle} | {name} | ask {asked} | tip {tip} | turn {turned} | moved {moved} | gave {gave} | {bore} N at {across},{along} (want {wantX},{wantZ}) | slid {slid} | law {law}')
    else:
        print(f'{solver} {miner} | {ankle} | {name} | ask {asked} | tip {tip} turn {turned} moved {moved} | gave {gave} | {bore} N at {across},{along} | top {top} | work {work} | else {elseStep}/{elseSum} | end {endTurn} r/s, flat {endFlat}, bent {endBent}, last half second {endMoved} deg | slid {slid} | law {law}')
print(f'({seen} rows)')
