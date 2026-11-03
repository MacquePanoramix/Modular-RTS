# Key joint angles (degrees) of each fitted stance, to compare with the CMU get-ups and the joint ranges.
import json, sys, math
from b3 import VI
R = json.load(open(sys.argv[1]))
only = sys.argv[2].split('|') if len(sys.argv) > 2 else None
keys = ['pitch', 'tflex', 'hflexL', 'hflexR', 'kneeL', 'kneeR', 'dorsiL', 'dorsiR', 'elbL', 'elbR', 'shflexL', 'toeL', 'toeR']
print('%-8s %-58s %s' % ('body', 'stance', ' '.join('%7s' % k for k in keys)))
for r in R:
    if only and r['body'] not in only:
        continue
    q = r['q']
    vals = [math.degrees(q[VI[k]]) for k in keys]
    print('%-8s %-58s %s' % (r['body'][:8], r['stance'][:58], ' '.join('%7.0f' % v for v in vals)))
