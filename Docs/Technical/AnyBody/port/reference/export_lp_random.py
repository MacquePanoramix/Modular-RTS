# Random linear programs with scipy's answers (HiGHS), to test the C# DenseSimplex beyond the reserve LP:
#   reserve-shaped (41 unknowns >= 0, 6 equalities, 220 inequalities), general ones with finite / infinite / free
#   bounds, degenerate ones (small integer data, zero right-hand sides), infeasible and unbounded ones.
#   python export_lp_random.py [out.json]      (default: lp_random.json next to this file)
import os, sys, json
import numpy as np
from scipy.optimize import linprog

HERE = os.path.dirname(os.path.abspath(__file__))


def bnd(v):
    return None if v is None or not np.isfinite(v) else float(v)


def case(kind, c, Aeq, beq, Aub, bub, bounds):
    res = linprog(c, A_ub=Aub if Aub is not None and len(Aub) else None, b_ub=bub if Aub is not None and len(Aub) else None,
                  A_eq=Aeq if Aeq is not None and len(Aeq) else None, b_eq=beq if Aeq is not None and len(Aeq) else None,
                  bounds=bounds, method='highs')
    out = dict(kind=kind, c=[float(v) for v in c],
               Aeq=[[float(v) for v in r] for r in (Aeq if Aeq is not None else [])], beq=[float(v) for v in (beq if beq is not None else [])],
               Aub=[[float(v) for v in r] for r in (Aub if Aub is not None else [])], bub=[float(v) for v in (bub if bub is not None else [])],
               lo=[bnd(b[0]) for b in bounds], hi=[bnd(b[1]) for b in bounds], status=int(res.status))
    if res.status == 0:
        out['fun'] = float(res.fun)
        out['x'] = [float(v) for v in res.x]
    return out


def main():
    rng = np.random.RandomState(20261103)
    cases = []
    # reserve-shaped: x >= 0, an s column with -1 in every inequality pair (always bounded and usually feasible)
    for it in range(60):
        n = 41
        G = rng.randn(6, n - 1)
        x0 = rng.rand(n - 1)
        Aeq = np.hstack([G, np.zeros((6, 1))])
        beq = G @ x0
        R = rng.randn(110, n - 1) * (rng.rand(110, n - 1) < 0.4)
        beta = rng.randn(110)
        Aub = np.vstack([np.hstack([R, -np.ones((110, 1))]), np.hstack([-R, -np.ones((110, 1))])])
        bub = np.concatenate([beta, -beta])
        c = np.concatenate([np.full(n - 1, 1e-4), [1.0]])
        if it % 10 == 9:
            beq = beq + 50.0  # usually infeasible
        cases.append(case('reserve-shaped', c, Aeq, beq, Aub, bub, [(0, None)] * n))
    # general: mixed bounds, equalities and inequalities
    for it in range(120):
        n = rng.randint(2, 30)
        me = rng.randint(0, min(n, 8))
        mu = rng.randint(0, 40)
        c = rng.randn(n)
        Aeq = rng.randn(me, n) * (rng.rand(me, n) < 0.6)
        Aub = rng.randn(mu, n) * (rng.rand(mu, n) < 0.6)
        x0 = rng.randn(n)
        beq = Aeq @ x0
        bub = Aub @ x0 + rng.rand(mu)
        bounds = []
        for j in range(n):
            k = rng.randint(5)
            if k == 0:
                bounds.append((None, None))
            elif k == 1:
                bounds.append((x0[j] - rng.rand(), None))
            elif k == 2:
                bounds.append((None, x0[j] + rng.rand()))
            else:
                bounds.append((x0[j] - rng.rand(), x0[j] + rng.rand()))
        if it % 12 == 11 and me > 0:
            beq = beq + 10 * rng.randn(me)  # often infeasible with the bounds
        cases.append(case('general', c, Aeq if me else None, beq if me else None, Aub if mu else None, bub if mu else None, bounds))
    # degenerate: small integers, zero right-hand sides plus a box
    for it in range(60):
        n = rng.randint(3, 16)
        mu = rng.randint(n, 4 * n)
        Aub = rng.randint(-1, 2, size=(mu, n)).astype(float)
        bub = np.zeros(mu)
        bub[: mu // 3] = rng.randint(0, 2, size=mu // 3)
        me = rng.randint(0, 3)
        Aeq = rng.randint(-1, 2, size=(me, n)).astype(float) if me else None
        beq = np.zeros(me) if me else None
        c = rng.randint(-2, 3, size=n).astype(float)
        bounds = [(0, 1 if rng.rand() < 0.5 else None) for _ in range(n)]
        cases.append(case('degenerate', c, Aeq, beq, Aub, bub, bounds))
    # unbounded ones: free directions with negative cost
    for it in range(20):
        n = rng.randint(2, 10)
        mu = rng.randint(1, 6)
        Aub = np.abs(rng.randn(mu, n)) * -1.0
        bub = rng.rand(mu)
        c = -np.abs(rng.randn(n))
        cases.append(case('unbounded', c, None, None, Aub, bub, [(0, None)] * n))
    # Beale's cycling example (cycles under the textbook rule without anti-cycling)
    c = np.array([-0.75, 150, -1 / 50.0, 6])
    Aub = np.array([[0.25, -60, -1 / 25.0, 9], [0.5, -90, -1 / 50.0, 3], [0, 0, 1, 0]])
    bub = np.array([0, 0, 1.0])
    cases.append(case('beale', c, None, None, Aub, bub, [(0, None)] * 4))
    # (added Nov 4, after the cases above, so they stay the same) infeasible ones of three sorts: reserve-shaped with an
    # equality no x >= 0 can meet (the weight on the wrong side, as a centre of mass outside the support polygon),
    # contradictory inequality pairs, and general ones whose equalities leave the box
    for it in range(30):
        sort = it % 3
        if sort == 0:
            n = 41
            G = rng.randn(6, n - 1)
            G[2] = np.abs(G[2]) + 0.1  # the 'vertical' row: every column pushes up
            x0 = rng.rand(n - 1)
            beq = G @ x0
            beq[2] = -1.0 - rng.rand()  # ... but the weight asks for down
            Aeq = np.hstack([G, np.zeros((6, 1))])
            R = rng.randn(110, n - 1) * (rng.rand(110, n - 1) < 0.4)
            beta = rng.randn(110)
            Aub = np.vstack([np.hstack([R, -np.ones((110, 1))]), np.hstack([-R, -np.ones((110, 1))])])
            bub = np.concatenate([beta, -beta])
            c = np.concatenate([np.full(n - 1, 1e-4), [1.0]])
            cases.append(case('infeasible', c, Aeq, beq, Aub, bub, [(0, None)] * n))
        elif sort == 1:
            n = rng.randint(2, 20)
            mu = rng.randint(2, 30)
            Aub = rng.randn(mu, n) * (rng.rand(mu, n) < 0.6)
            x0 = rng.randn(n)
            bub = Aub @ x0 + rng.rand(mu)
            a = rng.randn(n)
            Aub = np.vstack([Aub, a, -a])
            bub = np.concatenate([bub, [a @ x0 - 0.5], [-(a @ x0) - 0.5 - rng.rand()]])  # a.x <= v - 0.5 and a.x >= v + 0.5..
            perm = rng.permutation(len(bub))
            cases.append(case('infeasible', rng.randn(n), None, None, Aub[perm], bub[perm], [(None, None)] * n))
        else:
            n = rng.randint(3, 25)
            me = rng.randint(1, min(n, 6))
            Aeq = rng.randn(me, n)
            x0 = rng.rand(n)
            beq = Aeq @ x0
            beq[0] = np.abs(Aeq[0]).sum() + 1.0 + rng.rand()  # out of reach of the box [0, 1]^n
            cases.append(case('infeasible', rng.randn(n), Aeq, beq, None, None, [(0, 1)] * n))
    # degenerate reserve-shaped ones: most torque rows pass through the start (beta = 0), some columns repeated
    for it in range(30):
        n = 41
        G = rng.randn(6, n - 1)
        G[:, 1::5] = G[:, 0::5][:, : G[:, 1::5].shape[1]]  # repeated columns: ties in every ratio test
        x0 = rng.rand(n - 1)
        Aeq = np.hstack([G, np.zeros((6, 1))])
        beq = G @ x0
        R = rng.randn(110, n - 1) * (rng.rand(110, n - 1) < 0.4)
        beta = rng.randn(110) * (rng.rand(110) < 0.2)
        Aub = np.vstack([np.hstack([R, -np.ones((110, 1))]), np.hstack([-R, -np.ones((110, 1))])])
        bub = np.concatenate([beta, -beta])
        c = np.concatenate([np.full(n - 1, 1e-4), [1.0]])
        cases.append(case('degenerate-reserve', c, Aeq, beq, Aub, bub, [(0, None)] * n))
    st = {}
    for k in cases:
        st[(k['kind'], k['status'])] = st.get((k['kind'], k['status']), 0) + 1
    outp = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, 'lp_random.json')
    json.dump(dict(source='export_lp_random.py, scipy %s HiGHS' % __import__('scipy').__version__, cases=cases), open(outp, 'w'),
              allow_nan=False)
    print('wrote', len(cases), 'LPs to', outp, sorted(st.items()))


if __name__ == '__main__':
    main()
