# The few right-side samples the first batch lacked (so left and right routes are sampled alike).
import os, sys, json, time
os.environ.setdefault('OMP_NUM_THREADS', '1')
from multiprocessing import Pool
import run3

EXTRA = [('halfkneel_up', (0.7, 'R', 'none')), ('halfkneel_up', (0.85, 'R', 'none')), ('onekneel_toes', ('R',))]

if __name__ == '__main__':
    names = sys.argv[1].split('|')
    outp = sys.argv[2]
    jobs = [(bn, sn, sa) for bn in names for (sn, sa) in EXTRA]
    t0 = time.time()
    with Pool(min(30, len(jobs))) as pool:
        res = pool.map(run3.job, jobs, chunksize=1)
    json.dump(res, open(outp, 'w'), indent=1)
    print('done', len(res), 'jobs in', round(time.time() - t0), 's')
