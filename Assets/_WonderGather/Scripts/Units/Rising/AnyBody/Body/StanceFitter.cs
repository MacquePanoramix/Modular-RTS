// The fit: a capped, bounded Levenberg-Marquardt solver on the stance's residuals with exact (AD) Jacobians,
// warm-started, with an extra start only on failure (b3.solve_stance + run3.job's restart rule).
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WonderGather.AnyBody
{
    public sealed class FitOptions
    {
        /// <summary>Iteration cap per start (one Jacobian evaluation each). 400 (Nov 4; was 150): more of the 450 test
        /// fits match the prototype's (279 against 264); 13 still end on the cap. The prototype's TRF allows 600 evaluations.</summary>
        public int MaxIterations = 400;
        /// <summary>Stop when an accepted step lowers the cost by less than FTol x cost (scipy's ftol).</summary>
        public double FTol = 1e-7;
        /// <summary>Stop when the step is below XTol (XTol + |x|) (scipy's xtol).</summary>
        public double XTol = 1e-7;
        /// <summary>Stop when the scaled projected gradient is below GTol (scipy's gtol).</summary>
        public double GTol = 1e-8;
        /// <summary>A warm start (a full pose, 34): from a neighbouring body's atlas, or a previous fit. Null: the stance's q0.</summary>
        public double[] Start;
        /// <summary>Extra starts, each only when the best cost so far is at least RestartCost (run3: 1 extra at 0.05;
        /// 2 here (Nov 4): on the 450 test fits, worse than the prototype's 38 -> 21, for about 1.5-2.5x the time).
        /// The first extra start is run3's (best + RandomState(1).normal(0, 0.05)); later ones use other noise.</summary>
        public int Restarts = 2;
        public double RestartCost = 0.05;
        /// <summary>When set, extra starts are made from these poses (full, 34) before any noisy restart.</summary>
        public double[][] ExtraStarts;
        /// <summary>Free limbs (stances_strategy.md 1.1; INTEGRATION.md step 9): a weak pull of the limbs that bear nothing
        /// toward the relaxed pose, so that an arm reaches forward only when the weight must go forward. Off: as the
        /// prototype fits (Nov 4: on the first look's sheets free arms were held out level or up at the face).</summary>
        public bool RelaxFreeLimbs;
        /// <summary>The pull's weight per radian, when RelaxFreeLimbs is set.</summary>
        public double RelaxWeight = 0.1;
    }

    public sealed class FitResult
    {
        /// <summary>The fitted pose (34 VARS, radians/metres).</summary>
        public double[] Q;
        /// <summary>0.5 |r|^2 at Q.</summary>
        public double Cost;
        public int Iterations, Evaluations, JacobianEvaluations, Starts;
        public double Millis;
        public string Status;
        /// <summary>Which start gave the best cost (0: the first).</summary>
        public int BestStart;
        public int Residuals;
    }

    public static class StanceFitter
    {
        /// <summary>run3.job's second start: best + np.random.RandomState(1).normal(0, 0.05, 34).</summary>
        public static readonly double[] Noise1 =
        {
            0.08121726818316209, -0.03058782068250377, -0.026408587613172788, -0.05364843110780853, 0.04327038146623393,
            -0.11507693484401414, 0.087240588210824, -0.03806034504475514, 0.01595195480285493, -0.012468518773870505,
            0.0731053968522487, -0.10300703547488271, -0.016120860200675376, -0.019202717733420784, 0.056688472116771874,
            -0.05499456336570155, -0.008621410377521788, -0.04389292089606859, 0.0021106873357796415, 0.029140760685791113,
            -0.055030958860646066, 0.05723618549198071, 0.04507953602963978, 0.02512471694509341, 0.045042797463220596,
            -0.03418639295871666, -0.006144511275932409, -0.046788471712953444, -0.013394403981300797, 0.0265177733369093,
            -0.03458303758626546, -0.01983767634279887, -0.03435863500597997, -0.042260282074935984
        };

        public static FitResult Fit(BodyModel b, Stance st, FitOptions opt = null)
        {
            var plain = Fit(new StanceProblem(b, st), opt);
            if (opt == null || !opt.RelaxFreeLimbs) return plain;
            // Then its free limbs relaxed: a second fit with the pull, from the first one's pose and with no restart, so
            // the stance keeps its basin and only what bears nothing moves. (In one pass, the pull sent some fits to
            // another basin: on the 450 test fits, 46 rose by more than 0.01 in the cost without the pull, Nov 4.)
            var relax = new StanceProblem(b, st) { Relax = opt.RelaxWeight };
            var relaxed = Fit(relax, new FitOptions { MaxIterations = opt.MaxIterations, FTol = opt.FTol, XTol = opt.XTol, GTol = opt.GTol, Start = plain.Q, Restarts = 0 });
            relaxed.Starts += plain.Starts;
            relaxed.Iterations += plain.Iterations;
            relaxed.Evaluations += plain.Evaluations;
            relaxed.JacobianEvaluations += plain.JacobianEvaluations;
            relaxed.Millis += plain.Millis;
            return relaxed;
        }

        public static FitResult Fit(StanceProblem p, FitOptions opt = null)
        {
            opt = opt ?? new FitOptions();
            var sw = Stopwatch.StartNew();
            var lm = new Lm(p);
            int e0 = p.Evaluations, j0 = p.JacobianEvaluations;
            var x0 = p.StartVector(opt.Start);
            var best = lm.Run(x0, opt);
            int starts = 1, bestStart = 0;
            int extra = 0;
            while (best.Cost >= opt.RestartCost && extra < opt.Restarts)
            {
                double[] xs;
                if (opt.ExtraStarts != null && extra < opt.ExtraStarts.Length) xs = p.StartVector(opt.ExtraStarts[extra]);
                else
                {
                    // run3.job: q_init = best + RandomState(attempt).normal(0, 0.05, 34), only the free variables taken
                    var q = p.Full(best.X);
                    var noise = extra == 0 ? Noise1 : NoiseFor(extra + 1);
                    for (int i = 0; i < Pose.Count; i++) q[i] += noise[i];
                    xs = p.StartVector(q);
                }
                var r = lm.Run(xs, opt);
                starts++;
                extra++;
                if (r.Cost < best.Cost)
                {
                    best = r;
                    bestStart = starts - 1;
                }
            }
            sw.Stop();
            return new FitResult
            {
                Q = p.Full(best.X), Cost = best.Cost, Iterations = lm.TotalIterations, Evaluations = p.Evaluations - e0,
                JacobianEvaluations = p.JacobianEvaluations - j0, Starts = starts, Millis = sw.Elapsed.TotalMilliseconds,
                Status = best.Status, BestStart = bestStart, Residuals = best.M
            };
        }

        /// <summary>A deterministic N(0, 0.05) vector for further restarts (not numpy's; only Noise1 matches run3).
        /// SplitMix64 + Box-Muller, self-contained, so every runtime (.NET, Unity's Mono, IL2CPP) gives the same starts;
        /// System.Random's seeded sequence is not promised to be the same across runtimes.</summary>
        public static double[] NoiseFor(int seed)
        {
            ulong state = 0x9E3779B97F4A7C15UL * (ulong)(1000 + seed);
            var o = new double[Pose.Count];
            for (int i = 0; i < o.Length; i++)
            {
                double u1 = 1.0 - NextUnit(ref state), u2 = NextUnit(ref state);
                o[i] = 0.05 * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
            }
            return o;
        }

        /// <summary>SplitMix64's next value as a double in [0, 1) (53 bits).</summary>
        static double NextUnit(ref ulong state)
        {
            unchecked
            {
                state += 0x9E3779B97F4A7C15UL;
                ulong z = state;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                z ^= z >> 31;
                return (z >> 11) * (1.0 / 9007199254740992.0);
            }
        }

        internal struct LmResult
        {
            public double[] X;
            public double Cost;
            public string Status;
            public int M;
        }

        /// <summary>Bounded Levenberg-Marquardt (Nielsen's damping update, More's column scaling, active-set
        /// projection at the bounds).</summary>
        internal sealed class Lm
        {
            readonly StanceProblem _p;
            readonly List<D> _r = new List<D>(256);
            readonly int _n;
            // the present point's residuals/Jacobian, and the trial's (swapped on accept)
            double[] _J = new double[0], _Jt = new double[0], _J2 = new double[0], _Jt2 = new double[0];
            readonly double[] _g, _scale, _A, _rhs, _step, _xNew;
            readonly bool[] _active;
            public int TotalIterations;

            public Lm(StanceProblem p)
            {
                _p = p;
                _n = p.N;
                _g = new double[_n];
                _scale = new double[_n];
                _A = new double[_n * _n];
                _rhs = new double[_n];
                _step = new double[_n];
                _xNew = new double[_n];
                _active = new bool[_n];
            }

            double EvalJ(double[] x, ref double[] J, ref double[] Jt, out int m)
            {
                _p.Evaluate(x, true, _r);
                m = _r.Count;
                if (J.Length < m * _n) J = new double[m * _n];
                if (Jt.Length < m) Jt = new double[m];
                double s = 0.0;
                var tape = _p.Tape;
                for (int i = 0; i < m; i++)
                {
                    var d = _r[i];
                    Jt[i] = d.V;
                    s += d.V * d.V;
                    tape.Grad(d, J, i * _n);
                }
                return 0.5 * s;
            }

            public LmResult Run(double[] x0, FitOptions opt)
            {
                int n = _n;
                var x = (double[])x0.Clone();
                var lo = _p.Lo;
                var hi = _p.Hi;
                double cost = EvalJ(x, ref _J, ref _Jt, out int m);
                if (double.IsNaN(cost)) return new LmResult { X = x, Cost = double.PositiveInfinity, Status = "nan", M = m };
                Array.Clear(_scale, 0, n);
                UpdateScale(m);
                double mu = -1.0, nu = 2.0;
                string status = "max iterations";
                for (int it = 0; it < opt.MaxIterations; it++)
                {
                    // gradient and the normal matrix
                    for (int j = 0; j < n; j++)
                    {
                        double s = 0.0;
                        for (int i = 0; i < m; i++) s += _J[i * n + j] * _Jt[i];
                        _g[j] = s;
                    }
                    for (int a = 0; a < n; a++)
                    for (int c = a; c < n; c++)
                    {
                        double s = 0.0;
                        for (int i = 0; i < m; i++) s += _J[i * n + a] * _J[i * n + c];
                        _A[a * n + c] = s;
                        _A[c * n + a] = s;
                    }
                    // the active set: at a bound with the descent direction pointing out
                    double gmax = 0.0;
                    for (int j = 0; j < n; j++)
                    {
                        double span = hi[j] - lo[j];
                        double eps = 1e-10 * Math.Max(1.0, span);
                        _active[j] = (x[j] <= lo[j] + eps && _g[j] > 0) || (x[j] >= hi[j] - eps && _g[j] < 0);
                        if (!_active[j]) gmax = Math.Max(gmax, Math.Abs(_g[j]) / _scale[j]);
                    }
                    if (gmax < opt.GTol)
                    {
                        status = "gtol";
                        break;
                    }
                    if (mu < 0)
                    {
                        double dmax = 0.0;
                        for (int j = 0; j < n; j++) dmax = Math.Max(dmax, _A[j * n + j] / (_scale[j] * _scale[j]));
                        mu = 1e-3 * Math.Max(dmax, 1e-12);
                    }
                    bool accepted = false;
                    int tries = 0;
                    while (!accepted)
                    {
                        if (!SolveStep(n, mu))
                        {
                            mu *= nu;
                            nu *= 2;
                            if (++tries > 40) break;
                            continue;
                        }
                        double dx2 = 0.0, x2 = 0.0;
                        for (int j = 0; j < n; j++)
                        {
                            double v = _active[j] ? x[j] : Math.Min(Math.Max(x[j] + _step[j], lo[j]), hi[j]);
                            _xNew[j] = v;
                            _step[j] = v - x[j];
                            dx2 += _step[j] * _step[j];
                            x2 += x[j] * x[j];
                        }
                        if (Math.Sqrt(dx2) <= opt.XTol * (opt.XTol + Math.Sqrt(x2)))
                        {
                            status = "xtol";
                            goto done;
                        }
                        // the model's predicted reduction for the projected step
                        double gd = 0.0, dAd = 0.0;
                        for (int a = 0; a < n; a++)
                        {
                            gd += _g[a] * _step[a];
                            double s = 0.0;
                            for (int c = 0; c < n; c++) s += _A[a * n + c] * _step[c];
                            dAd += _step[a] * s;
                        }
                        double pred = -(gd + 0.5 * dAd);
                        double costNew = EvalJ(_xNew, ref _J2, ref _Jt2, out int mNew);
                        TotalIterations++;
                        if (!double.IsNaN(costNew) && costNew < cost && mNew == m)
                        {
                            double rho = pred > 0 ? (cost - costNew) / pred : 0.0;
                            double dF = cost - costNew;
                            double old = cost;
                            Array.Copy(_xNew, x, n);
                            cost = costNew;
                            var tJ = _J; _J = _J2; _J2 = tJ;
                            var tR = _Jt; _Jt = _Jt2; _Jt2 = tR;
                            UpdateScale(m);
                            double t = 2 * rho - 1;
                            mu *= Math.Max(1.0 / 3.0, 1 - t * t * t);
                            nu = 2.0;
                            accepted = true;
                            if (dF < opt.FTol * old && rho > 0.25)
                            {
                                status = "ftol";
                                goto done;
                            }
                        }
                        else
                        {
                            mu *= nu;
                            nu *= 2;
                            if (++tries > 40 || mu > 1e20)
                            {
                                status = "no descent";
                                goto done;
                            }
                        }
                    }
                    if (!accepted)
                    {
                        status = "singular";
                        break;
                    }
                }
                done:
                return new LmResult { X = x, Cost = cost, Status = status, M = m };
            }

            void UpdateScale(int m)
            {
                for (int j = 0; j < _n; j++)
                {
                    double s = 0.0;
                    for (int i = 0; i < m; i++) s += _J[i * _n + j] * _J[i * _n + j];
                    s = Math.Sqrt(s);
                    if (s > _scale[j]) _scale[j] = s;
                    if (_scale[j] == 0.0) _scale[j] = 1.0;
                }
            }

            /// <summary>(A + mu diag(scale^2)) step = -g over the free variables (Cholesky).</summary>
            bool SolveStep(int n, double mu)
            {
                // build the reduced system in _row-indexed order using a copy into a scratch matrix
                var L = Scratch(n * n);
                int[] map = MapFree(n, out int nf);
                for (int a = 0; a < nf; a++)
                {
                    int ia = map[a];
                    for (int c = 0; c <= a; c++)
                    {
                        int ic = map[c];
                        double v = _A[ia * n + ic];
                        if (a == c) v += mu * _scale[ia] * _scale[ia];
                        L[a * nf + c] = v;
                    }
                    _rhs[a] = -_g[ia];
                }
                // Cholesky in place (lower)
                for (int j = 0; j < nf; j++)
                {
                    double s = L[j * nf + j];
                    for (int k2 = 0; k2 < j; k2++) s -= L[j * nf + k2] * L[j * nf + k2];
                    if (s <= 0 || double.IsNaN(s)) return false;
                    double d = Math.Sqrt(s);
                    L[j * nf + j] = d;
                    for (int i = j + 1; i < nf; i++)
                    {
                        double t = L[i * nf + j];
                        for (int k2 = 0; k2 < j; k2++) t -= L[i * nf + k2] * L[j * nf + k2];
                        L[i * nf + j] = t / d;
                    }
                }
                for (int i = 0; i < nf; i++)
                {
                    double t = _rhs[i];
                    for (int k2 = 0; k2 < i; k2++) t -= L[i * nf + k2] * _rhs[k2];
                    _rhs[i] = t / L[i * nf + i];
                }
                for (int i = nf - 1; i >= 0; i--)
                {
                    double t = _rhs[i];
                    for (int k2 = i + 1; k2 < nf; k2++) t -= L[k2 * nf + i] * _rhs[k2];
                    _rhs[i] = t / L[i * nf + i];
                }
                Array.Clear(_step, 0, n);
                for (int a = 0; a < nf; a++) _step[map[a]] = _rhs[a];
                return true;
            }

            double[] _scratch = new double[0];
            int[] _map = new int[0];

            double[] Scratch(int len)
            {
                if (_scratch.Length < len) _scratch = new double[len];
                return _scratch;
            }

            int[] MapFree(int n, out int nf)
            {
                if (_map.Length < n) _map = new int[n];
                nf = 0;
                for (int j = 0; j < n; j++)
                    if (!_active[j]) _map[nf++] = j;
                return _map;
            }
        }
    }
}
