// The reserve linear program of b3.py (reserve_lp): the smallest s for which contact forces inside friction pyramids
// balance the body's weight with every joint within s times its capacity (hinges about their axis, ball joints in the
// 13 directions of StaticsProblem.BallDirections), with b3.py's small secondary cost (1e-4 / W per newton of pyramid
// weight). As b3.py: unknowns are the 4 pyramid-edge weights per contact and s, all >= 0; 6 equalities (the ground
// contacts hold the weight: force and moment); two inequalities per checked direction of every joint.
//
// It is solved through its dual with DenseSimplex: the dual has one row per unknown (4 nc + 1, about 41) instead of
// one per inequality (about 220), the slack basis is feasible from the start (all costs are >= 0), so no phase 1 is
// needed, and a shift of the body's mass changes only the dual's costs: the last basis stays feasible and is re-used
// (warm start) by Prepare + Reserve(shift). The primal answer is read from the dual's multipliers.
//
// Units inside: forces in body weights (W = M g), joint rows divided by the joint's capacity, moments in metres.
using System;

namespace WonderGather.AnyBody
{
    /// <summary>What the reserve LP says about one pose and its supports.</summary>
    public sealed class ReserveResult
    {
        /// <summary>Optimal, or Infeasible: no force split inside the friction pyramids balances the weight
        /// (S = R = +inf). IterationLimit or Numerical are solver failures (S = R = +inf as well, like b3.py).</summary>
        public LpStatus Status;

        public bool Held => Status == LpStatus.Optimal;

        /// <summary>The LP optimum: b3.reserve_lp's s (res.x[-1]); the smallest share of its capacity the worst joint
        /// must use, ball joints measured in the 13 directions. Unique.</summary>
        public double S = double.PositiveInfinity;

        /// <summary>run3.py's r: max of PerJoint at the forces found (ball joints by the full norm of the torque).
        /// Not unique: other optimal force splits give other values (see INTERFACE.md).</summary>
        public double R = double.PositiveInfinity;

        /// <summary>Index into StaticsProblem.Joints of the largest PerJoint (the first, on a tie); -1 if not held.</summary>
        public int Worst = -1;

        public string WorstName;

        /// <summary>Per contact: the force on Contacts[i].Link (world, N).</summary>
        public Vec3[] Forces;

        /// <summary>b3.reserve_lp(want=True)['per']: |tau|/cap for a ball joint, |axis.tau|/cap for a hinge.</summary>
        public double[] PerJoint;

        /// <summary>The LP's own measure: max over the 13 directions of |d.tau|/cap for a ball joint (hinges as
        /// PerJoint). Its max equals S at the optimum.</summary>
        public double[] PerJointLp;

        /// <summary>run3.py hands_share: the vertical force on ground contacts of a forearm, in body weights.</summary>
        public double HandsShare;

        /// <summary>run3.py on_thighs: sum of |f.n| over self-contacts (a hand on a thigh), in body weights.</summary>
        public double OnThighs;

        /// <summary>The primal objective s + 1e-4 sum(pyramid weights in body weights).</summary>
        public double Objective = double.PositiveInfinity;

        /// <summary>Check of the answer in the original units: the largest of the equilibrium residual (in body
        /// weights, and body weights x metres) and of max(PerJointLp) - S.</summary>
        public double Residual;

        public int Pivots;

        /// <summary>True when the solve re-used the previous basis (Prepare + Reserve/Solve with a new shift).</summary>
        public bool WarmStarted;
    }

    /// <summary>Solves b3.py's reserve LP for StaticsProblems. One instance per thread; it re-uses its buffers.</summary>
    public sealed class ReserveSolver
    {
        /// <summary>b3.py: cost 1e-4 / W per newton of pyramid weight, i.e. 1e-4 per body weight.</summary>
        public const double ForceCost = 1e-4;

        readonly DenseSimplex lp = new DenseSimplex();
        StaticsProblem p;
        int nc, nv, nrow, nj;
        double weight;
        Vec3[] gens = Array.Empty<Vec3>(); // 4 per contact
        int[] rowJoint = Array.Empty<int>(); // per primal inequality pair
        Vec3[] rowDir = Array.Empty<Vec3>();
        double[] jointScale = Array.Empty<double>(); // 1 / max(cap, tiny)
        bool warm;

        /// <summary>The LP's settings (tolerances, pivot limit), for tests.</summary>
        public DenseSimplex Simplex => lp;

        /// <summary>Relative spread of the per-edge force cost: edge k costs ForceCost (1 + CostJitter h_k), h_k in
        /// [0, 1) a fixed sequence. It moves S by at most ForceCost CostJitter sqrt(1 + mu^2) (about 1.2e-8 at the
        /// default) and picks one force split deterministically. 0 gives b3.py's costs exactly. Takes effect at
        /// Prepare.</summary>
        public double CostJitter = 1e-4;

        static double Jitter(int k)
        {
            double v = (k + 1) * 0.6180339887498949;
            return v - Math.Floor(v);
        }

        /// <summary>The directions a ball joint's torque is checked in (unit vectors; -d is checked with each d).
        /// Default (null): StaticsProblem.BallDirections, b3.py's 13. A finer set (HalfSphereDirections) brings S up
        /// towards the full-norm reserve, the least worst |tau|/cap over all force splits, from below. Takes effect at
        /// Prepare. Hinges are checked about their axis either way.</summary>
        public Vec3[] Directions;

        Vec3[] dirs = StaticsProblem.BallDirections;

        /// <summary>n unit directions spread evenly over the upper half sphere (a Fibonacci lattice); with their
        /// opposites they cover the sphere, so |d.tau| at most s cap for all of them approaches |tau| at most s cap.</summary>
        public static Vec3[] HalfSphereDirections(int n)
        {
            if (n < 1) throw new ArgumentException("n >= 1");
            var o = new Vec3[n];
            double golden = Math.PI * (3.0 - Math.Sqrt(5.0));
            for (int k = 0; k < n; k++)
            {
                double z = (k + 0.5) / n, rr = Math.Sqrt(Math.Max(0.0, 1.0 - z * z)), ph = golden * k;
                o[k] = new Vec3(rr * Math.Cos(ph), rr * Math.Sin(ph), z);
            }

            return o;
        }

        /// <summary>The problem the solver is prepared for.</summary>
        public StaticsProblem Problem => p;

        /// <summary>Unknowns of the reserve LP (4 per contact + s) and its inequality rows.</summary>
        public int Unknowns => nv;

        public int Inequalities => 2 * nrow;

        /// <summary>Total pivots since this solver was made.</summary>
        public long TotalPivots { get; private set; }

        public long Solves { get; private set; }

        /// <summary>Builds the LP for this pose and these supports. Call again whenever p changes; until then
        /// Reserve(shift) and Solve(shift) warm-start from the last basis.</summary>
        public void Prepare(StaticsProblem problem)
        {
            p = problem ?? throw new ArgumentNullException(nameof(problem));
            nc = p.Contacts?.Length ?? 0;
            nj = p.Joints?.Length ?? 0;
            nv = 4 * nc + 1;
            weight = p.Weight;
            if (!(weight > 0)) throw new ArgumentException("the body needs a positive mass");
            if (gens.Length < 4 * nc) gens = new Vec3[4 * nc];
            var e4 = new Vec3[4];
            for (int i = 0; i < nc; i++)
            {
                StaticsProblem.PyramidEdges(p.Contacts[i].Normal, p.Contacts[i].Mu, e4);
                for (int k = 0; k < 4; k++) gens[4 * i + k] = e4[k];
            }

            // the checked directions: a hinge about its axis, a ball joint in the 13 directions (or Directions)
            dirs = Directions ?? StaticsProblem.BallDirections;
            nrow = 0;
            for (int j = 0; j < nj; j++) nrow += p.Joints[j].IsHinge ? 1 : dirs.Length;
            if (rowJoint.Length < nrow)
            {
                rowJoint = new int[nrow];
                rowDir = new Vec3[nrow];
            }

            if (jointScale.Length < nj) jointScale = new double[nj];
            int r0 = 0;
            for (int j = 0; j < nj; j++)
            {
                var jt = p.Joints[j];
                jointScale[j] = 1.0 / Math.Max(jt.Capacity, 1e-9 * weight);
                if (jt.IsHinge)
                {
                    rowJoint[r0] = j;
                    rowDir[r0++] = jt.Axis;
                }
                else
                {
                    foreach (var d in dirs)
                    {
                        rowJoint[r0] = j;
                        rowDir[r0++] = d;
                    }
                }
            }

            // the dual: rows = the primal's unknowns; columns = y (6, free) | v (2 nrow, >= 0) | w (nv, slack)
            int ny = 6, nvv = 2 * nrow;
            int cols = ny + nvv + nv;
            lp.Resize(nv, cols);
            var A = lp.A;
            Array.Clear(A, 0, nv * cols);
            // y: the equalities' columns, transposed. Ground contacts only.
            for (int i = 0; i < nc; i++)
            {
                if (!p.Contacts[i].OnGround) continue;
                var pt = p.Contacts[i].Point;
                for (int k = 0; k < 4; k++)
                {
                    var g = gens[4 * i + k];
                    var mo = Vec3.Cross(pt, g);
                    int row = (4 * i + k) * cols;
                    A[row + 0] = g.X;
                    A[row + 1] = g.Y;
                    A[row + 2] = g.Z;
                    A[row + 3] = mo.X;
                    A[row + 4] = mo.Y;
                    A[row + 5] = mo.Z;
                }
            }

            // v: minus the inequalities' columns. Row pair q: (+a, -c s) <= beta and (-a, -c s) <= -beta, where a is
            // the torque the edge weights take off the joint in direction d (scaled by W / cap).
            for (int q = 0; q < nrow; q++)
            {
                int j = rowJoint[q];
                var jt = p.Joints[j];
                var d = rowDir[q];
                double sc = weight * jointScale[j];
                int cPlus = ny + 2 * q, cMinus = cPlus + 1;
                for (int i = 0; i < nc; i++)
                {
                    int sg = jt.ContactSign[i];
                    if (sg == 0) continue;
                    var rr = p.Contacts[i].Point - jt.Origin;
                    for (int k = 0; k < 4; k++)
                    {
                        double a = sg * sc * Vec3.Dot(d, Vec3.Cross(rr, gens[4 * i + k]));
                        if (a == 0.0) continue;
                        int row = (4 * i + k) * cols;
                        A[row + cPlus] = -a;
                        A[row + cMinus] = a;
                    }
                }

                double cs = jt.Capacity * jointScale[j];
                int rowS = (nv - 1) * cols;
                A[rowS + cPlus] = cs;
                A[rowS + cMinus] = cs;
            }

            // w: identity
            for (int r = 0; r < nv; r++) A[r * cols + ny + nvv + r] = 1.0;
            // right-hand side = the primal's costs, each edge's a little different (CostJitter) so that the dual's
            // ratio tests seldom tie: fewer degenerate pivots, and one optimal force split instead of a face of them
            for (int r = 0; r < nv - 1; r++) lp.B[r] = ForceCost * (1.0 + CostJitter * Jitter(r));
            lp.B[nv - 1] = 1.0;
            // bounds
            for (int c = 0; c < cols; c++)
            {
                lp.Lo[c] = c < ny ? double.NegativeInfinity : 0.0;
                lp.Hi[c] = double.PositiveInfinity;
            }

            warm = false;
        }

        /// <summary>The dual's costs for a shift of the whole body's mass (horizontal, world): -b_eq for y, b_ub for v.</summary>
        void SetCosts(Vec3 shift)
        {
            var c = lp.C;
            var com = p.Com + shift;
            c[0] = 0.0;
            c[1] = 0.0;
            c[2] = -1.0;
            c[3] = -com.Y;
            c[4] = com.X;
            c[5] = 0.0;
            for (int q = 0; q < nrow; q++)
            {
                int j = rowJoint[q];
                double beta = Vec3.Dot(rowDir[q], p.Joints[j].GravityTorque(shift)) * jointScale[j];
                c[6 + 2 * q] = beta;
                c[6 + 2 * q + 1] = -beta;
            }

            for (int k = 6 + 2 * nrow; k < 6 + 2 * nrow + nv; k++) c[k] = 0.0;
        }

        LpStatus Run(Vec3 shift, out bool usedWarm)
        {
            if (p == null) throw new InvalidOperationException("Prepare first");
            SetCosts(shift);
            usedWarm = false;
            LpStatus st;
            if (warm && lp.ResolveNewCosts(out st))
            {
                usedWarm = true;
                if (st == LpStatus.IterationLimit || st == LpStatus.Numerical)
                {
                    usedWarm = false;
                    st = lp.SolveFromIdentity(6 + 2 * nrow);
                }
            }
            else
                st = lp.SolveFromIdentity(6 + 2 * nrow);

            TotalPivots += lp.Iterations;
            Solves++;
            // a basis is at hand for the next shift unless the solve failed
            warm = st == LpStatus.Optimal || st == LpStatus.Unbounded;
            return st;
        }

        /// <summary>s for the prepared problem with the body's mass shifted by shift (b3.reserve_lp(K, C, shift));
        /// +inf when no force split holds it. Warm-started from the previous call's basis.</summary>
        public double Reserve(Vec3 shift)
        {
            var st = Run(shift, out _);
            if (st != LpStatus.Optimal) return double.PositiveInfinity;
            return Math.Max(0.0, -lp.Duals[nv - 1]);
        }

        /// <summary>Prepare(p) and Reserve(shift).</summary>
        public double Reserve(StaticsProblem problem, Vec3 shift)
        {
            Prepare(problem);
            return Reserve(shift);
        }

        /// <summary>b3.reserve_lp(K, C, want=True), for p as given.</summary>
        public ReserveResult Solve(StaticsProblem problem) => Solve(problem, Vec3.Zero);

        public ReserveResult Solve(StaticsProblem problem, Vec3 shift)
        {
            Prepare(problem);
            return Solve(shift);
        }

        /// <summary>The full answer for the prepared problem, warm-started when possible.</summary>
        public ReserveResult Solve(Vec3 shift)
        {
            var st = Run(shift, out bool usedWarm);
            var res = new ReserveResult { Pivots = lp.Iterations, WarmStarted = usedWarm };
            if (st == LpStatus.Unbounded) st = LpStatus.Infeasible; // the dual unbounded: the primal has no point
            else if (st == LpStatus.Infeasible) st = LpStatus.Numerical; // cannot happen: the dual starts feasible
            res.Status = st;
            if (st != LpStatus.Optimal) return res;
            Extract(shift, res);
            return res;
        }

        void Extract(Vec3 shift, ReserveResult res)
        {
            var pi = lp.Duals;
            double s = Math.Max(0.0, -pi[nv - 1]);
            res.S = s;
            res.Objective = -lp.Objective;
            res.Forces = new Vec3[nc];
            for (int i = 0; i < nc; i++)
            {
                var f = Vec3.Zero;
                for (int k = 0; k < 4; k++)
                {
                    double mu = Math.Max(0.0, -pi[4 * i + k]);
                    f += mu * gens[4 * i + k];
                }

                res.Forces[i] = f * weight;
            }

            Evaluate(p, res.Forces, shift, res, dirs);
        }

        /// <summary>Reads given contact forces as b3.py / run3.py do: the joint torques tau = tau0 - sum sign (p - o) x f,
        /// PerJoint, PerJointLp, R, Worst, HandsShare, OnThighs, and Residual = the largest of the equilibrium residual
        /// and max(PerJointLp) - res.S (with res.S as set by the caller; leave it +inf to skip that part).</summary>
        public static void Evaluate(StaticsProblem p, Vec3[] forces, Vec3 shift, ReserveResult res) =>
            Evaluate(p, forces, shift, res, StaticsProblem.BallDirections);

        /// <summary>As Evaluate, with PerJointLp measured in the given ball-joint directions (the solver's Directions).</summary>
        public static void Evaluate(StaticsProblem p, Vec3[] forces, Vec3 shift, ReserveResult res, Vec3[] ballDirections)
        {
            int nc = p.Contacts.Length, nj = p.Joints.Length;
            double weight = p.Weight;
            res.Forces = forces;
            res.PerJoint = new double[nj];
            res.PerJointLp = new double[nj];
            res.Worst = -1;
            double worstVal = double.NegativeInfinity;
            double lpMax = 0.0;
            for (int j = 0; j < nj; j++)
            {
                var jt = p.Joints[j];
                var tau = jt.GravityTorque(shift);
                for (int i = 0; i < nc; i++)
                {
                    int sg = jt.ContactSign[i];
                    if (sg == 0) continue;
                    tau -= sg * Vec3.Cross(p.Contacts[i].Point - jt.Origin, forces[i]);
                }

                double cap = jt.Capacity;
                double per, perLp;
                if (jt.IsHinge)
                {
                    per = Ratio(Math.Abs(Vec3.Dot(jt.Axis, tau)), cap);
                    perLp = per;
                }
                else
                {
                    per = Ratio(tau.Length, cap);
                    double mx = 0.0;
                    foreach (var d in ballDirections) mx = Math.Max(mx, Math.Abs(Vec3.Dot(d, tau)));
                    perLp = Ratio(mx, cap);
                }

                res.PerJoint[j] = per;
                res.PerJointLp[j] = perLp;
                lpMax = Math.Max(lpMax, perLp);
                if (per > worstVal)
                {
                    worstVal = per;
                    res.Worst = j;
                }
            }

            res.R = nj > 0 ? worstVal : 0.0;
            res.WorstName = res.Worst >= 0 ? p.Joints[res.Worst].Name : null;
            // shares, and the check of the answer in the original units
            var sumF = Vec3.Zero;
            var sumM = Vec3.Zero;
            double hands = 0.0, thighs = 0.0;
            for (int i = 0; i < nc; i++)
            {
                var c = p.Contacts[i];
                var f = forces[i];
                if (c.OnGround)
                {
                    sumF += f;
                    sumM += Vec3.Cross(c.Point, f);
                    if (BodyLinks.IsForearm(c.Link)) hands += f.Z;
                }
                else
                    thighs += Math.Abs(Vec3.Dot(f, c.Normal));
            }

            res.HandsShare = hands / weight;
            res.OnThighs = thighs / weight;
            var com = p.Com + shift;
            double eF = (sumF - new Vec3(0, 0, weight)).Length / weight;
            double eM = (sumM - new Vec3(weight * com.Y, -weight * com.X, 0)).Length / weight;
            res.Residual = Math.Max(eF, eM);
            if (!double.IsPositiveInfinity(res.S)) res.Residual = Math.Max(res.Residual, lpMax - res.S);
        }

        static double Ratio(double v, double cap)
        {
            if (cap > 0) return v / cap;
            return v > 0 ? double.PositiveInfinity : 0.0;
        }
    }

    /// <summary>Convenience entry points (a new solver per call; use a ReserveSolver to re-use buffers).</summary>
    public static class ReserveLP
    {
        public static ReserveResult Solve(StaticsProblem p) => new ReserveSolver().Solve(p, Vec3.Zero);

        public static ReserveResult Solve(StaticsProblem p, Vec3 shift) => new ReserveSolver().Solve(p, shift);
    }
}
