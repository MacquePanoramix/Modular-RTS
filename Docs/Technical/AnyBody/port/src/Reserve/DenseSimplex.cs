// A dense, bounded-variable primal simplex for small linear programs (tens of rows, a few hundred columns):
//     minimise c.x  subject to  A x = b,  lo <= x <= hi   (lo may be -inf, hi may be +inf: free variables).
// The full tableau B^-1 A is kept and updated by pivoting. Dantzig pricing, a Harris two-pass ratio test, and Bland's
// rule while pivots stay degenerate (so it cannot cycle). Phase 1 with one artificial per row, or a start from a basis
// of identity columns the caller names. At the end the basis is factorised afresh (LU) and the answer recomputed and
// checked; the solve goes on from the fresh tableau if the check fails. A tableau left by a solve can be re-used when
// only the costs change (ResolveNewCosts): the basis stays feasible, so only phase 2 runs.
// Engine-independent (no UnityEngine, no System.Numerics); allocates only when the problem grows.
using System;

namespace WonderGather.AnyBody
{
    public enum LpStatus
    {
        /// <summary>An optimum was found and checked against a fresh factorisation.</summary>
        Optimal,

        /// <summary>No x satisfies the constraints.</summary>
        Infeasible,

        /// <summary>The objective decreases without bound.</summary>
        Unbounded,

        /// <summary>The pivot limit was reached.</summary>
        IterationLimit,

        /// <summary>The basis became singular or the answer failed its check after refinement.</summary>
        Numerical
    }

    public sealed class DenseSimplex
    {
        const int Basic = 0, AtLower = 1, AtUpper = 2, FreeZero = 3;

        // ------------------------------------------------------------------ the problem (filled by the caller)
        public int Rows { get; private set; }
        public int Cols { get; private set; }

        /// <summary>Rows x Cols, row-major.</summary>
        public double[] A = Array.Empty<double>();

        public double[] B = Array.Empty<double>();
        public double[] C = Array.Empty<double>();
        public double[] Lo = Array.Empty<double>();
        public double[] Hi = Array.Empty<double>();

        // ------------------------------------------------------------------ settings (for well-scaled data, entries ~1)
        public double FeasibilityTol = 1e-9;
        public double OptimalityTol = 1e-9;
        public double PivotTol = 1e-9;

        /// <summary>The bound violation the final check of the basic values accepts (they are then clamped).</summary>
        public double CheckTol = 1e-7;

        /// <summary>0: 50 (rows + columns) + 1000.</summary>
        public int MaxIterations;

        /// <summary>Degenerate pivots in a row before Bland's rule takes over (until a pivot makes progress).</summary>
        public int DegenerateRunToBland = 12;

        /// <summary>A pivot smaller than this share of its column's largest entry is avoided: up to MaxRejections other
        /// entering columns are tried first (outside Bland's rule).</summary>
        public double RelativePivotTol = 1e-5;

        public int MaxRejections = 8;

        /// <summary>Optional trace of the iterations (debugging only).</summary>
        public Action<string> Trace;

        // ------------------------------------------------------------------ results
        public LpStatus Status { get; private set; }
        public int Iterations { get; private set; }
        public int Refactorizations { get; private set; }

        /// <summary>Entering columns passed over for a small pivot, and restarts from scratch, since this solver was made.</summary>
        public long Rejections { get; private set; }

        public long Fallbacks { get; private set; }

        public double Objective { get; private set; }

        /// <summary>The solution (Cols), after Optimal.</summary>
        public double[] X => x;

        /// <summary>The simplex multipliers pi (Rows): B^T pi = c_B. For a minimum, b.pi is the optimal value.</summary>
        public double[] Duals => pi;

        /// <summary>Reduced costs c - A^T pi (Cols), after Optimal.</summary>
        public double[] ReducedCosts => dd;

        // ------------------------------------------------------------------ working state
        int m, n, nt;
        double[] t = Array.Empty<double>(); // m x nt tableau B^-1 [A | artificials]
        double[] xb = Array.Empty<double>(); // basic values
        double[] d = Array.Empty<double>(); // reduced costs, nt
        double[] cost = Array.Empty<double>(); // the phase's costs, nt
        double[] lo = Array.Empty<double>(), hi = Array.Empty<double>(); // nt
        double[] xn = Array.Empty<double>(); // value of each nonbasic column, nt
        double[] artSign = Array.Empty<double>(); // m: artificial k is artSign[k] e_k
        int[] basis = Array.Empty<int>();
        int[] state = Array.Empty<int>();
        int[] nz = Array.Empty<int>();
        int[] rejected = Array.Empty<int>();
        int stamp;
        double[] x = Array.Empty<double>(), pi = Array.Empty<double>(), dd = Array.Empty<double>();
        double[] lu = Array.Empty<double>(), rhs = Array.Empty<double>(), sol = Array.Empty<double>();
        double[] scratch = Array.Empty<double>(), col = Array.Empty<double>();
        int[] perm = Array.Empty<int>();
        bool tableauValid;
        int maxIter, fallbackDepth;

        /// <summary>Sizes the problem arrays (contents are not cleared beyond what is reallocated) and forgets the
        /// last tableau.</summary>
        public void Resize(int rows, int cols)
        {
            if (rows < 0 || cols < 1) throw new ArgumentException("rows >= 0, cols >= 1");
            Rows = rows;
            Cols = cols;
            Ensure(ref A, rows * cols);
            Ensure(ref B, rows);
            Ensure(ref C, cols);
            Ensure(ref Lo, cols);
            Ensure(ref Hi, cols);
            tableauValid = false;
        }

        static void Ensure(ref double[] a, int len)
        {
            if (a.Length < len) a = new double[len];
        }

        static void Ensure(ref int[] a, int len)
        {
            if (a.Length < len) a = new int[len];
        }

        void Alloc(int withArtificials)
        {
            m = Rows;
            n = Cols;
            nt = n + withArtificials;
            Ensure(ref t, m * nt);
            Ensure(ref xb, m);
            Ensure(ref d, nt);
            Ensure(ref cost, nt);
            Ensure(ref lo, nt);
            Ensure(ref hi, nt);
            Ensure(ref xn, nt);
            Ensure(ref artSign, m);
            Ensure(ref basis, m);
            Ensure(ref state, nt);
            Ensure(ref nz, nt);
            if (rejected.Length < nt)
            {
                rejected = new int[nt];
                stamp = 0;
            }
            Ensure(ref x, n);
            Ensure(ref pi, m);
            Ensure(ref dd, nt);
            Ensure(ref lu, m * m);
            Ensure(ref rhs, Math.Max(m, 1));
            Ensure(ref sol, Math.Max(m, 1));
            Ensure(ref scratch, Math.Max(m, 1));
            Ensure(ref col, Math.Max(m, 1));
            Ensure(ref perm, Math.Max(m, 1));
            maxIter = MaxIterations > 0 ? MaxIterations : 50 * (m + nt) + 1000;
        }

        // ================================================================== entry points

        /// <summary>Solves from scratch: phase 1 with one artificial per row, then phase 2.</summary>
        public LpStatus Solve()
        {
            Iterations = 0;
            Refactorizations = 0;
            fallbackDepth = 0;
            return SolvePhases();
        }

        LpStatus SolvePhases()
        {
            Alloc(Rows);
            CopyBounds();
            for (int j = n; j < nt; j++)
            {
                lo[j] = 0.0;
                hi[j] = double.PositiveInfinity;
            }

            // nonbasic structurals at a finite bound (or zero when free)
            for (int j = 0; j < n; j++) SetNonbasicAtStart(j);
            // residual b - A xN decides each artificial's sign
            for (int i = 0; i < m; i++)
            {
                double r = B[i];
                int row = i * n;
                for (int j = 0; j < n; j++)
                {
                    double v = xn[j];
                    if (v != 0.0) r -= A[row + j] * v;
                }

                double sg = r >= 0 ? 1.0 : -1.0;
                artSign[i] = sg;
                int tr = i * nt;
                for (int j = 0; j < n; j++) t[tr + j] = sg * A[row + j];
                for (int k = 0; k < m; k++) t[tr + n + k] = k == i ? 1.0 : 0.0;
                xb[i] = sg * r;
                basis[i] = n + i;
                state[n + i] = Basic;
            }

            tableauValid = true;
            // phase 1: minimise the sum of the artificials
            for (int j = 0; j < nt; j++) cost[j] = j >= n ? 1.0 : 0.0;
            PriceFromScratch();
            var st = Iterate();
            if (st == LpStatus.Unbounded) st = LpStatus.Numerical; // a sum of non-negative artificials is bounded
            if (st != LpStatus.Optimal) return Finish(st);
            double infeas = 0.0;
            for (int i = 0; i < m; i++)
                if (basis[i] >= n) infeas += Math.Max(0.0, xb[i]);
            double bscale = 1.0;
            for (int i = 0; i < m; i++) bscale = Math.Max(bscale, Math.Abs(B[i]));
            if (infeas > FeasibilityTol * bscale * Math.Max(1, m))
            {
                tableauValid = false; // the artificials are not at zero: no basis for phase 2
                return Finish(LpStatus.Infeasible);
            }

            DriveOutArtificials();
            // phase 2
            for (int j = 0; j < n; j++) cost[j] = C[j];
            for (int j = n; j < nt; j++) cost[j] = 0.0;
            PriceFromScratch();
            st = Iterate();
            return Finish(st);
        }

        /// <summary>Solves starting from the basis made of columns [firstIdentityColumn, firstIdentityColumn + Rows),
        /// which must be the identity in A. Every other column starts at a finite bound (or zero when free). Falls
        /// back to Solve() when that start is not feasible.</summary>
        public LpStatus SolveFromIdentity(int firstIdentityColumn)
        {
            Iterations = 0;
            Refactorizations = 0;
            fallbackDepth = 0;
            Alloc(0);
            if (firstIdentityColumn < 0 || firstIdentityColumn + m > n) throw new ArgumentException("identity columns out of range");
            CopyBounds();
            for (int j = 0; j < n; j++) SetNonbasicAtStart(j);
            Array.Copy(A, t, m * n);
            for (int i = 0; i < m; i++)
            {
                int bj = firstIdentityColumn + i;
                double r = B[i];
                int row = i * n;
                for (int j = 0; j < n; j++)
                {
                    if (j == bj) continue;
                    double v = xn[j];
                    if (v != 0.0) r -= A[row + j] * v;
                }

                if (r < lo[bj] - FeasibilityTol || r > hi[bj] + FeasibilityTol) return Solve();
                basis[i] = bj;
                state[bj] = Basic;
                xn[bj] = 0.0;
                xb[i] = r;
            }

            tableauValid = true;
            for (int j = 0; j < n; j++) cost[j] = C[j];
            PriceFromScratch();
            return Finish(Iterate());
        }

        /// <summary>After a solve of the same A, b, lo, hi whose basis is still at hand (any status but
        /// IterationLimit or Numerical), solves again with new costs C: the basis stays feasible, only phase 2 runs.
        /// Returns false (and does nothing) when no usable tableau is at hand.</summary>
        public bool ResolveNewCosts(out LpStatus status)
        {
            status = LpStatus.Numerical;
            if (!tableauValid || Rows != m || Cols != n) return false;
            Iterations = 0;
            Refactorizations = 0;
            fallbackDepth = 0;
            for (int j = 0; j < n; j++) cost[j] = C[j];
            for (int j = n; j < nt; j++) cost[j] = 0.0;
            PriceFromScratch();
            status = Finish(Iterate());
            return true;
        }

        // ================================================================== setup helpers

        void CopyBounds()
        {
            for (int j = 0; j < n; j++)
            {
                lo[j] = Lo[j];
                hi[j] = Hi[j];
                if (lo[j] > hi[j]) throw new ArgumentException("Lo > Hi at column " + j);
            }
        }

        void SetNonbasicAtStart(int j)
        {
            if (!double.IsNegativeInfinity(lo[j]))
            {
                state[j] = AtLower;
                xn[j] = lo[j];
            }
            else if (!double.IsPositiveInfinity(hi[j]))
            {
                state[j] = AtUpper;
                xn[j] = hi[j];
            }
            else
            {
                state[j] = FreeZero;
                xn[j] = 0.0;
            }
        }

        /// <summary>d = cost - cost_B^T T, from the tableau.</summary>
        void PriceFromScratch()
        {
            for (int j = 0; j < nt; j++) d[j] = cost[j];
            for (int i = 0; i < m; i++)
            {
                double cb = cost[basis[i]];
                if (cb == 0.0) continue;
                int tr = i * nt;
                for (int j = 0; j < nt; j++) d[j] -= cb * t[tr + j];
            }

            for (int i = 0; i < m; i++) d[basis[i]] = 0.0;
        }

        /// <summary>After phase 1: pivots basic artificials (at zero) out where a structural column allows it; the
        /// artificials are then fixed at zero (a row that keeps one is redundant).</summary>
        void DriveOutArtificials()
        {
            for (int i = 0; i < m; i++)
            {
                if (basis[i] < n) continue;
                int tr = i * nt;
                int q = -1;
                double best = 1e-7;
                for (int j = 0; j < n; j++)
                {
                    if (state[j] == Basic) continue;
                    double a = Math.Abs(t[tr + j]);
                    if (a > best)
                    {
                        best = a;
                        q = j;
                    }
                }

                if (q < 0) continue; // redundant row
                int leaving = basis[i];
                double vq = xn[q];
                Pivot(i, q);
                basis[i] = q;
                state[q] = Basic;
                // the artificial was at (about) zero, so the step is degenerate: q keeps its value
                xb[i] = vq;
                state[leaving] = AtLower;
                xn[leaving] = 0.0;
            }

            for (int j = n; j < nt; j++)
            {
                lo[j] = 0.0;
                hi[j] = 0.0;
                if (state[j] != Basic)
                {
                    state[j] = AtLower;
                    xn[j] = 0.0;
                }
            }
        }

        // ================================================================== the simplex iterations

        /// <summary>Dantzig pricing (Bland: the lowest index), skipping columns rejected in this iteration.</summary>
        int Price(bool bland, out int dir)
        {
            int q = -1;
            dir = 0;
            double best = 0.0;
            for (int j = 0; j < nt; j++)
            {
                int s = state[j];
                if (s == Basic || rejected[j] == stamp) continue;
                double dj = d[j];
                int sg;
                if (s == AtLower)
                {
                    if (dj >= -OptimalityTol || hi[j] <= lo[j]) continue;
                    sg = 1;
                }
                else if (s == AtUpper)
                {
                    if (dj <= OptimalityTol || hi[j] <= lo[j]) continue;
                    sg = -1;
                }
                else
                {
                    if (Math.Abs(dj) <= OptimalityTol) continue;
                    sg = dj < 0 ? 1 : -1;
                }

                if (bland)
                {
                    dir = sg;
                    return j;
                }

                double sc = Math.Abs(dj);
                if (sc > best)
                {
                    best = sc;
                    q = j;
                    dir = sg;
                }
            }

            return q;
        }

        /// <summary>The ratio test for column q moving in direction dir: the blocking row (-1: none) and the step.
        /// x_B(step) = x_B - dir * step * T[:, q]. Harris's two passes, or Bland's smallest ratio with ties to the
        /// lowest basic index.</summary>
        int Ratio(int q, int dir, bool bland, out double step)
        {
            int r = -1;
            step = double.PositiveInfinity;
            if (!bland)
            {
                // pass 1: the largest step with every bound relaxed by the tolerance
                double theta = double.PositiveInfinity;
                for (int i = 0; i < m; i++)
                {
                    double a = dir * t[i * nt + q];
                    int bi = basis[i];
                    if (a > PivotTol)
                    {
                        double l = lo[bi];
                        if (!double.IsNegativeInfinity(l))
                        {
                            double v = (xb[i] - l + FeasibilityTol) / a;
                            if (v < theta) theta = v;
                        }
                    }
                    else if (a < -PivotTol)
                    {
                        double h = hi[bi];
                        if (!double.IsPositiveInfinity(h))
                        {
                            double v = (h - xb[i] + FeasibilityTol) / -a;
                            if (v < theta) theta = v;
                        }
                    }
                }

                // pass 2: among the rows that block within theta, the largest pivot
                double bestA = 0.0;
                for (int i = 0; i < m; i++)
                {
                    double a = dir * t[i * nt + q];
                    int bi = basis[i];
                    double v;
                    if (a > PivotTol)
                    {
                        double l = lo[bi];
                        if (double.IsNegativeInfinity(l)) continue;
                        v = (xb[i] - l) / a;
                    }
                    else if (a < -PivotTol)
                    {
                        double h = hi[bi];
                        if (double.IsPositiveInfinity(h)) continue;
                        v = (h - xb[i]) / -a;
                    }
                    else continue;

                    if (v <= theta && Math.Abs(a) > bestA)
                    {
                        bestA = Math.Abs(a);
                        r = i;
                        step = v < 0 ? 0.0 : v;
                    }
                }

                return r;
            }

            for (int i = 0; i < m; i++)
            {
                double a = dir * t[i * nt + q];
                int bi = basis[i];
                double v;
                if (a > PivotTol)
                {
                    double l = lo[bi];
                    if (double.IsNegativeInfinity(l)) continue;
                    v = (xb[i] - l) / a;
                }
                else if (a < -PivotTol)
                {
                    double h = hi[bi];
                    if (double.IsPositiveInfinity(h)) continue;
                    v = (h - xb[i]) / -a;
                }
                else continue;

                if (v < 0) v = 0.0;
                double tie = 1e-12 * Math.Max(1.0, Math.Abs(step));
                if (r < 0 || v < step - tie || (v <= step + tie && bi < basis[r]))
                {
                    r = i;
                    step = v;
                }
            }

            return r;
        }

        LpStatus Iterate()
        {
            int degenerateRun = 0;
            bool bland = false;
            bool fresh = false; // the tableau was rebuilt from a factorisation since the last pivot
            while (true)
            {
                stamp++;
                int rejections = 0, bestRejQ = -1, bestRejDir = 0;
                double bestRejA = 0.0;
                int q, dir, r;
                double step, flip;
                while (true)
                {
                    q = Price(bland, out dir);
                    if (q < 0)
                    {
                        if (bestRejQ < 0) return LpStatus.Optimal;
                        // every candidate gave a small pivot: take the largest of them
                        q = bestRejQ;
                        dir = bestRejDir;
                        r = Ratio(q, dir, bland, out step);
                        flip = FlipRange(q);
                        break;
                    }

                    r = Ratio(q, dir, bland, out step);
                    flip = FlipRange(q);
                    if (r < 0 || flip <= step || bland || rejections >= MaxRejections) break;
                    // a small pivot (relative to its column) spoils the tableau: try another column first
                    double a = Math.Abs(t[r * nt + q]);
                    double colMax = 0.0;
                    for (int i = 0; i < m; i++) colMax = Math.Max(colMax, Math.Abs(t[i * nt + q]));
                    if (a >= Math.Max(PivotTol, RelativePivotTol * colMax)) break;
                    if (a > bestRejA)
                    {
                        bestRejA = a;
                        bestRejQ = q;
                        bestRejDir = dir;
                    }

                    rejected[q] = stamp;
                    rejections++;
                    Rejections++;
                }

                if (r < 0 && double.IsPositiveInfinity(flip))
                {
                    // unbounded on this tableau: check it on a fresh factorisation before saying so. The fresh reduced
                    // cost and the fresh column of q certify the ray (one solve with the basis); only when they do not
                    // is the whole tableau rebuilt and the iteration resumed.
                    Trace?.Invoke(FormattableString.Invariant($"unbounded? it {Iterations} q {q} dir {dir} d {d[q]:G4} bland {bland} fresh {fresh}"));
                    if (fresh) return LpStatus.Unbounded;
                    if (!Refresh(out _, out _)) return LpStatus.Numerical;
                    if (ConfirmRay(q, dir))
                    {
                        for (int i = 0; i < m; i++) xb[i] = sol[i];
                        return LpStatus.Unbounded;
                    }

                    RebuildTableau();
                    fresh = true;
                    continue;
                }

                if (Iterations >= maxIter) return LpStatus.IterationLimit;
                Iterations++;
                bool boundFlip = r < 0 || flip <= step;
                if (boundFlip) step = flip;
                Trace?.Invoke(FormattableString.Invariant(
                    $"it {Iterations} q {q} dir {dir} d {d[q]:G4} r {r} leaving {(r >= 0 ? basis[r] : -1)} step {step:G4} alpha {(r >= 0 ? t[r * nt + q] : 0):G4} flip {boundFlip} bland {bland}"));

                // ---- move
                if (step != 0.0)
                {
                    double ds = dir * step;
                    for (int i = 0; i < m; i++)
                    {
                        double a = t[i * nt + q];
                        if (a != 0.0) xb[i] -= ds * a;
                    }
                }

                if (step > 1e-12)
                {
                    degenerateRun = 0;
                    bland = false;
                }
                else if (++degenerateRun >= DegenerateRunToBland)
                    bland = true;

                if (boundFlip)
                {
                    if (dir > 0)
                    {
                        state[q] = AtUpper;
                        xn[q] = hi[q];
                    }
                    else
                    {
                        state[q] = AtLower;
                        xn[q] = lo[q];
                    }

                    continue;
                }

                // ---- pivot: q enters in row r, basis[r] leaves at the bound it reached
                int leaving = basis[r];
                double aLeave = dir * t[r * nt + q];
                double vq = xn[q] + dir * step;
                Pivot(r, q);
                fresh = false;
                basis[r] = q;
                state[q] = Basic;
                xb[r] = vq;
                if (aLeave > 0)
                {
                    state[leaving] = AtLower;
                    xn[leaving] = lo[leaving];
                }
                else
                {
                    state[leaving] = AtUpper;
                    xn[leaving] = hi[leaving];
                }
            }
        }

        /// <summary>After Refresh: column q moved in direction dir lowers the phase's cost at the fresh reduced cost,
        /// and no basic variable's finite bound blocks its fresh column B^-1 a_q, so the direction is a ray of the
        /// feasible set along which the objective decreases without bound.</summary>
        bool ConfirmRay(int q, int dir)
        {
            if (!(dir * dd[q] < -OptimalityTol)) return false;
            if (q < n)
                for (int i = 0; i < m; i++) rhs[i] = A[i * n + q];
            else
            {
                for (int i = 0; i < m; i++) rhs[i] = 0.0;
                rhs[q - n] = artSign[q - n];
            }

            SolveB(rhs, col);
            for (int i = 0; i < m; i++)
            {
                double a = dir * col[i];
                int bi = basis[i];
                if (a > PivotTol && !double.IsNegativeInfinity(lo[bi])) return false;
                if (a < -PivotTol && !double.IsPositiveInfinity(hi[bi])) return false;
            }

            return true;
        }

        double FlipRange(int q) =>
            (double.IsInfinity(lo[q]) || double.IsInfinity(hi[q])) ? double.PositiveInfinity : hi[q] - lo[q];

        /// <summary>Gauss-Jordan pivot on T[r, q] (and the reduced costs). Basic values are kept by the caller.</summary>
        void Pivot(int r, int q)
        {
            int tr = r * nt;
            double inv = 1.0 / t[tr + q];
            int k = 0;
            for (int j = 0; j < nt; j++)
            {
                double v = t[tr + j];
                if (v == 0.0) continue;
                v *= inv;
                if (Math.Abs(v) < 1e-15)
                {
                    t[tr + j] = 0.0;
                    continue;
                }

                t[tr + j] = v;
                nz[k++] = j;
            }

            t[tr + q] = 1.0;
            for (int i = 0; i < m; i++)
            {
                if (i == r) continue;
                int ti = i * nt;
                double f = t[ti + q];
                if (f == 0.0) continue;
                for (int p = 0; p < k; p++)
                {
                    int j = nz[p];
                    t[ti + j] -= f * t[tr + j];
                }

                t[ti + q] = 0.0;
            }

            double fq = d[q];
            if (fq != 0.0)
            {
                for (int p = 0; p < k; p++)
                {
                    int j = nz[p];
                    d[j] -= fq * t[tr + j];
                }
            }

            d[q] = 0.0;
        }

        // ================================================================== the check against a fresh factorisation

        /// <summary>Factorises the basis afresh and recomputes from the original data: the basic values (into sol),
        /// the multipliers pi and the reduced costs dd for the phase's costs. False when the basis is singular.</summary>
        bool Refresh(out bool primalOk, out bool dualOk)
        {
            primalOk = dualOk = false;
            if (!Factorize()) return false;
            Refactorizations++;
            // basic values: B xB = b - N xN
            for (int i = 0; i < m; i++) rhs[i] = B[i];
            for (int j = 0; j < nt; j++)
            {
                if (state[j] == Basic) continue;
                double v = xn[j];
                if (v == 0.0) continue;
                if (j < n)
                    for (int i = 0; i < m; i++) rhs[i] -= A[i * n + j] * v;
                else
                    rhs[j - n] -= artSign[j - n] * v;
            }

            SolveB(rhs, sol);
            primalOk = true;
            double worstP = 0;
            for (int i = 0; i < m; i++)
            {
                int bi = basis[i];
                double v = sol[i];
                double viol = Math.Max(lo[bi] - v, v - hi[bi]);
                if (viol > worstP) worstP = viol;
                if (viol > CheckTol) primalOk = false;
            }

            if (Trace != null) Trace(FormattableString.Invariant($"refresh: worst bound violation {worstP:G3}"));

            // multipliers: B^T pi = c_B; reduced costs dd = cost - A^T pi
            for (int i = 0; i < m; i++) rhs[i] = cost[basis[i]];
            SolveBT(rhs, pi);
            dualOk = true;
            for (int j = 0; j < nt; j++)
            {
                double dj = cost[j];
                if (j < n)
                    for (int i = 0; i < m; i++) dj -= A[i * n + j] * pi[i];
                else
                    dj -= artSign[j - n] * pi[j - n];
                dd[j] = dj;
                int s = state[j];
                if (s == Basic || hi[j] <= lo[j]) continue;
                if (s == AtLower && dj < -OptimalityTol) dualOk = false;
                else if (s == AtUpper && dj > OptimalityTol) dualOk = false;
                else if (s == FreeZero && Math.Abs(dj) > OptimalityTol) dualOk = false;
            }

            for (int i = 0; i < m; i++) dd[basis[i]] = 0.0;
            if (Trace != null) Trace($"refresh: primal ok {primalOk}, dual ok {dualOk}");
            return true;
        }

        LpStatus Finish(LpStatus st)
        {
            if (st == LpStatus.IterationLimit || st == LpStatus.Numerical)
            {
                tableauValid = false;
                Status = st;
                return st;
            }

            if (st != LpStatus.Optimal)
            {
                Status = st;
                return st;
            }

            for (int round = 0; round < 4; round++)
            {
                if (!Refresh(out bool primalOk, out bool dualOk))
                {
                    tableauValid = false;
                    Status = LpStatus.Numerical;
                    return Status;
                }

                if (primalOk && dualOk)
                {
                    for (int i = 0; i < m; i++) xb[i] = sol[i];
                    for (int j = 0; j < n; j++) x[j] = state[j] == Basic ? 0.0 : xn[j];
                    for (int i = 0; i < m; i++)
                        if (basis[i] < n)
                            x[basis[i]] = Math.Min(hi[basis[i]], Math.Max(lo[basis[i]], xb[i]));
                    double obj = 0.0;
                    for (int j = 0; j < n; j++) obj += C[j] * x[j];
                    Objective = obj;
                    Status = LpStatus.Optimal;
                    return Status;
                }

                if (!primalOk)
                {
                    // the fresh basic values left their bounds: once, start again from scratch with Bland's rule at
                    // the first degenerate pivot
                    if (fallbackDepth > 0)
                    {
                        tableauValid = false;
                        Status = LpStatus.Numerical;
                        return Status;
                    }

                    fallbackDepth++;
                    Fallbacks++;
                    int keep = DegenerateRunToBland;
                    DegenerateRunToBland = 1;
                    int it = Iterations;
                    var s2 = SolvePhases();
                    Iterations += it;
                    DegenerateRunToBland = keep;
                    return s2;
                }

                // rebuild the tableau from the factorisation and go on
                RebuildTableau();
                var s3 = Iterate();
                if (s3 != LpStatus.Optimal)
                {
                    Status = s3;
                    if (s3 == LpStatus.IterationLimit || s3 == LpStatus.Numerical) tableauValid = false;
                    return s3;
                }
            }

            tableauValid = false;
            Status = LpStatus.Numerical;
            return Status;
        }

        /// <summary>T = B^-1 [A | artificials], xb and d from the last Refresh.</summary>
        void RebuildTableau()
        {
            for (int i = 0; i < m; i++) xb[i] = sol[i];
            for (int j = 0; j < nt; j++)
            {
                if (j < n)
                    for (int i = 0; i < m; i++) rhs[i] = A[i * n + j];
                else
                {
                    for (int i = 0; i < m; i++) rhs[i] = 0.0;
                    rhs[j - n] = artSign[j - n];
                }

                SolveB(rhs, sol);
                for (int i = 0; i < m; i++) t[i * nt + j] = sol[i];
            }

            for (int j = 0; j < nt; j++) d[j] = state[j] == Basic ? 0.0 : dd[j];
        }

        /// <summary>LU with partial pivoting of the basis matrix (columns of A, or artificial unit columns).</summary>
        bool Factorize()
        {
            for (int c = 0; c < m; c++)
            {
                int j = basis[c];
                if (j < n)
                    for (int i = 0; i < m; i++) lu[i * m + c] = A[i * n + j];
                else
                    for (int i = 0; i < m; i++) lu[i * m + c] = i == j - n ? artSign[i] : 0.0;
            }

            for (int i = 0; i < m; i++) perm[i] = i;
            for (int k = 0; k < m; k++)
            {
                int p = k;
                double best = Math.Abs(lu[k * m + k]);
                for (int i = k + 1; i < m; i++)
                {
                    double v = Math.Abs(lu[i * m + k]);
                    if (v > best)
                    {
                        best = v;
                        p = i;
                    }
                }

                if (best < 1e-13) return false;
                if (p != k)
                {
                    for (int c = 0; c < m; c++)
                    {
                        double tmp = lu[k * m + c];
                        lu[k * m + c] = lu[p * m + c];
                        lu[p * m + c] = tmp;
                    }

                    int tp = perm[k];
                    perm[k] = perm[p];
                    perm[p] = tp;
                }

                double piv = lu[k * m + k];
                for (int i = k + 1; i < m; i++)
                {
                    double f = lu[i * m + k] / piv;
                    lu[i * m + k] = f;
                    if (f == 0.0) continue;
                    for (int c = k + 1; c < m; c++) lu[i * m + c] -= f * lu[k * m + c];
                }
            }

            return true;
        }

        /// <summary>Solves B z = r (P B = L U).</summary>
        void SolveB(double[] r, double[] z)
        {
            for (int i = 0; i < m; i++)
            {
                double v = r[perm[i]];
                for (int k = 0; k < i; k++) v -= lu[i * m + k] * z[k];
                z[i] = v;
            }

            for (int i = m - 1; i >= 0; i--)
            {
                double v = z[i];
                for (int k = i + 1; k < m; k++) v -= lu[i * m + k] * z[k];
                z[i] = v / lu[i * m + i];
            }
        }

        /// <summary>Solves B^T z = r (B^T = U^T L^T P).</summary>
        void SolveBT(double[] r, double[] z)
        {
            var w = scratch;
            // U^T w = r
            for (int i = 0; i < m; i++)
            {
                double v = r[i];
                for (int k = 0; k < i; k++) v -= lu[k * m + i] * w[k];
                w[i] = v / lu[i * m + i];
            }

            // L^T v = w
            for (int i = m - 1; i >= 0; i--)
            {
                double v = w[i];
                for (int k = i + 1; k < m; k++) v -= lu[k * m + i] * w[k];
                w[i] = v;
            }

            // z = P^T v
            for (int i = 0; i < m; i++) z[perm[i]] = w[i];
        }
    }
}
