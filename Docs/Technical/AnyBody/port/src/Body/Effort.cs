// b3.effort_forces: the least-effort force split inside the pose search, with exact derivatives.
// min |Cm g - d|^2 + g' reg g  s.t.  Aw g = bvec  (g = f / W), solved through its KKT system; the gradient of the
// solution comes from implicit differentiation: KKT dz = d(rhs) - d(KKT) z, one LU, N right-hand sides.
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody
{
    /// <summary>Reusable buffers for the effort split (one per evaluator/thread).</summary>
    public sealed class EffortWork
    {
        internal int Nc, Nr, Nk;
        internal double[] Cm = new double[0];      // Nr x 3Nc values
        internal D[] CmD = new D[0];                // Nr x 3Nc (only nonzero blocks are set)
        internal bool[] RowBlock = new bool[0];     // Nr x Nc: block (row, contact) present
        internal double[] Dv = new double[0];       // Nr
        internal D[] DD = new D[0];                 // Nr
        internal double[] Kkt = new double[0];      // Nk x Nk (LU in place)
        internal int[] Piv = new int[0];
        internal double[] Z = new double[0];        // Nk
        internal double[] Rhs = new double[0];
        internal double[] Grad = new double[0];     // Nk x N (right-hand sides, then solutions)
        internal double[] E = new double[0];        // Nr: Cm g - d
        internal double[] Row = new double[0];      // scratch gradient row
        internal int[] RowJoint = new int[30];
        internal int[] RowComp = new int[30];       // -1 hinge, else 0..2 for a ball joint's component
        internal int[] UOff = new int[30];
        internal bool Regularise;

        /// <summary>How many evaluations had to regularise a singular KKT system (for tests and reports).</summary>
        public int SingularCount;

        internal void Ensure(int nc, int n)
        {
            Nc = nc;
            Nr = 30;
            Nk = 3 * nc + 6;
            int cols = 3 * nc;
            if (Cm.Length < Nr * cols) { Cm = new double[Nr * cols]; CmD = new D[Nr * cols]; }
            if (RowBlock.Length < Nr * nc) RowBlock = new bool[Nr * nc];
            if (Dv.Length < Nr) { Dv = new double[Nr]; DD = new D[Nr]; E = new double[Nr]; }
            if (Kkt.Length < Nk * Nk) Kkt = new double[Nk * Nk];
            if (Piv.Length < Nk) { Piv = new int[Nk]; Z = new double[Nk]; Rhs = new double[Nk]; }
            if (Grad.Length < Nk * Math.Max(1, n)) Grad = new double[Nk * Math.Max(1, n)];
            if (Row.Length < Math.Max(1, n)) Row = new double[Math.Max(1, n)];
        }
    }

    public static class Effort
    {
        public const double Alpha = 0.02, Beta = 0.3;

        /// <summary>b3.effort_forces(K, C) followed by the resid's use of it: appends sqrt(wEff) * (Cm g - d) (30 rows)
        /// and, per contact, 3 * sp(-(f.n)/W, 0.02). Returns false if the KKT system was singular (regularised).</summary>
        public static bool Append(Kin k, List<DContact> C, double wEff, Tape tape, EffortWork w, List<D> r, Vec3[] forcesOut = null)
        {
            int nc = C.Count;
            int N = tape?.N ?? 0;
            w.Ensure(nc, N);
            int cols = 3 * nc, nr = w.Nr, nk = w.Nk;
            double G = BodyFactory.G;
            double M = k.M;
            double W = M * G;

            // ---- torque rows: Cm (scaled by W), d
            var cm = w.Cm;
            var cmD = w.CmD;
            Array.Clear(cm, 0, nr * cols);
            Array.Clear(w.RowBlock, 0, nr * nc);
            int row = 0;
            for (int j = 0; j < 14; j++)
            {
                var jt = k.Joints[j];
                // the subtree's mass and first moment about the joint: S' = sum m c - msum o
                double msum = 0.0;
                D sx = 0.0, sy = 0.0;
                for (int l = 0; l < BodyLinks.Count; l++)
                {
                    if ((jt.SubtreeMask & (1 << l)) == 0) continue;
                    double m = k.LinkMass[l];
                    msum += m;
                    sx = D.Comb(1.0, sx, m, k.LinkCom[l].X, 0.0, default);
                    sy = D.Comb(1.0, sy, m, k.LinkCom[l].Y, 0.0, default);
                }
                sx = D.Comb(1.0, sx, -msum, jt.Origin.X, 0.0, default);
                sy = D.Comb(1.0, sy, -msum, jt.Origin.Y, 0.0, default);
                // t0 = G (S'y, -S'x, 0)
                var t0x = sy * G;
                var t0y = sx * (-G);
                double ic = 1.0 / jt.Capacity;
                if (jt.IsHinge)
                {
                    w.RowJoint[row] = j;
                    w.RowComp[row] = -1;
                    var a = jt.Axis;
                    var dd = D.MulAdd(a.X, t0x, a.Y, t0y) / jt.Capacity;
                    w.DD[row] = dd;
                    w.Dv[row] = dd.V;
                    for (int i = 0; i < nc; i++)
                    {
                        int sg = Sign(jt.SubtreeMask, C[i]);
                        if (sg == 0) continue;
                        w.RowBlock[row * nc + i] = true;
                        var rr = (C[i].P - jt.Origin) * (double)sg;
                        var ar = DV3.Cross(a, rr); // axis' skew(r) = (a x r)'
                        Set(w, row, cols, 3 * i, ar.X * (ic * W));
                        Set(w, row, cols, 3 * i + 1, ar.Y * (ic * W));
                        Set(w, row, cols, 3 * i + 2, ar.Z * (ic * W));
                    }
                    row++;
                }
                else
                {
                    for (int comp = 0; comp < 3; comp++)
                    {
                        w.RowJoint[row + comp] = j;
                        w.RowComp[row + comp] = comp;
                        var dd = comp == 0 ? t0x / jt.Capacity : (comp == 1 ? t0y / jt.Capacity : new D(0.0));
                        w.DD[row + comp] = dd;
                        w.Dv[row + comp] = dd.V;
                    }
                    for (int i = 0; i < nc; i++)
                    {
                        int sg = Sign(jt.SubtreeMask, C[i]);
                        if (sg == 0) continue;
                        var rr = (C[i].P - jt.Origin) * (double)sg;
                        double s = ic * W;
                        // skew(r) rows: [0, -rz, ry], [rz, 0, -rx], [-ry, rx, 0]
                        w.RowBlock[row * nc + i] = true;
                        w.RowBlock[(row + 1) * nc + i] = true;
                        w.RowBlock[(row + 2) * nc + i] = true;
                        Set(w, row, cols, 3 * i, new D(0.0));
                        Set(w, row, cols, 3 * i + 1, rr.Z * (-s));
                        Set(w, row, cols, 3 * i + 2, rr.Y * s);
                        Set(w, row + 1, cols, 3 * i, rr.Z * s);
                        Set(w, row + 1, cols, 3 * i + 1, new D(0.0));
                        Set(w, row + 1, cols, 3 * i + 2, rr.X * (-s));
                        Set(w, row + 2, cols, 3 * i, rr.Y * (-s));
                        Set(w, row + 2, cols, 3 * i + 1, rr.X * s);
                        Set(w, row + 2, cols, 3 * i + 2, new D(0.0));
                    }
                    row += 3;
                }
            }

            // ---- KKT = [[2 (Cm'Cm + reg), Aw'], [Aw, 0]], rhs = [2 Cm' d, bvec]
            var K = w.Kkt;
            Array.Clear(K, 0, nk * nk);
            for (int a = 0; a < cols; a++)
            {
                int ia = a / 3;
                for (int bcol = a; bcol < cols; bcol++)
                {
                    int ib = bcol / 3;
                    double s = 0.0;
                    for (int rr = 0; rr < nr; rr++)
                    {
                        if (!w.RowBlock[rr * nc + ia] || !w.RowBlock[rr * nc + ib]) continue;
                        s += cm[rr * cols + a] * cm[rr * cols + bcol];
                    }
                    K[a * nk + bcol] = 2 * s;
                    K[bcol * nk + a] = 2 * s;
                }
            }
            for (int i = 0; i < nc; i++)
            {
                var n = C[i].N.V;
                for (int p = 0; p < 3; p++)
                for (int q2 = 0; q2 < 3; q2++)
                {
                    double reg = (p == q2 ? Alpha + Beta : 0.0) - Beta * n[p] * n[q2];
                    K[(3 * i + p) * nk + 3 * i + q2] += 2 * reg;
                }
            }
            // Aw = A W: rows 0..2 identity blocks, rows 3..5 skew(p), ground contacts only
            for (int i = 0; i < nc; i++)
            {
                if (!C[i].OnGround) continue;
                var p = C[i].P.V;
                int c = 3 * i;
                SetA(K, nk, cols, 0, c, W);
                SetA(K, nk, cols, 1, c + 1, W);
                SetA(K, nk, cols, 2, c + 2, W);
                SetA(K, nk, cols, 3, c + 1, -p.Z * W);
                SetA(K, nk, cols, 3, c + 2, p.Y * W);
                SetA(K, nk, cols, 4, c, p.Z * W);
                SetA(K, nk, cols, 4, c + 2, -p.X * W);
                SetA(K, nk, cols, 5, c, -p.Y * W);
                SetA(K, nk, cols, 5, c + 1, p.X * W);
            }
            var rhs = w.Rhs;
            for (int a = 0; a < cols; a++)
            {
                double s = 0.0;
                int ia = a / 3;
                for (int rr = 0; rr < nr; rr++)
                    if (w.RowBlock[rr * nc + ia]) s += cm[rr * cols + a] * w.Dv[rr];
                rhs[a] = 2 * s;
            }
            var com = k.Com;
            // bvec = [0, 0, M G, G S.y, -G S.x, 0], S = M com
            D bx = com.Y * (M * G);
            D by = com.X * (-M * G);
            rhs[cols] = 0;
            rhs[cols + 1] = 0;
            rhs[cols + 2] = M * G;
            rhs[cols + 3] = bx.V;
            rhs[cols + 4] = by.V;
            rhs[cols + 5] = 0;

            if (w.Regularise)
            {
                double amax = 0.0;
                for (int i = 0; i < nk * nk; i++) amax = Math.Max(amax, Math.Abs(K[i]));
                for (int a = cols; a < nk; a++) K[a * nk + a] -= 1e-9 * amax;
            }
            bool ok = LuFactor(K, nk, w.Piv);
            if (!ok)
            {
                if (!w.Regularise)
                {
                    // the equilibrium rows are dependent (all ground points on a line): regularise the multipliers'
                    // block slightly, which approximates lstsq's minimum-norm answer; rebuilt from scratch (rare)
                    w.SingularCount++;
                    return AppendRegularised(k, C, wEff, tape, w, r, forcesOut);
                }
                for (int rr = 0; rr < nr + nc; rr++) r.Add(new D(0.0));
                return false;
            }
            var z = w.Z;
            Array.Copy(rhs, z, nk);
            LuSolve(K, nk, w.Piv, z, 0, 1);

            // e = Cm g - d
            var e = w.E;
            for (int rr = 0; rr < nr; rr++)
            {
                double s = 0.0;
                for (int i = 0; i < nc; i++)
                {
                    if (!w.RowBlock[rr * nc + i]) continue;
                    for (int c = 3 * i; c < 3 * i + 3; c++) s += cm[rr * cols + c] * z[c];
                }
                e[rr] = s - w.Dv[rr];
            }

            double sq = Math.Sqrt(wEff);
            if (N == 0)
            {
                for (int rr = 0; rr < nr; rr++) r.Add(new D(sq * e[rr]));
                for (int i = 0; i < nc; i++)
                {
                    var n = C[i].N.V;
                    double fn = (z[3 * i] * W * n.X + z[3 * i + 1] * W * n.Y + z[3 * i + 2] * W * n.Z) / W;
                    r.Add(D.Softplus(new D(-fn), 0.02) * 3.0);
                }
                if (forcesOut != null) for (int i = 0; i < nc; i++) forcesOut[i] = new Vec3(z[3 * i] * W, z[3 * i + 1] * W, z[3 * i + 2] * W);
                return true;
            }

            // ---- gradients: V1 = d(rhs) - d(KKT) z, rows of N
            // (tape.Alloc may grow the tape into a new buffer, so buf is read again after every Alloc; review, Nov 4)
            var buf = tape.Buf;
            var Gm = w.Grad; // nk x N
            Array.Clear(Gm, 0, nk * N);
            // u_r = (dCm g - dd)_r as gradient rows (allocated on the tape so they can be reused below)
            var uOff = w.UOff;
            for (int rr = 0; rr < nr; rr++)
            {
                int o = tape.Alloc();
                buf = tape.Buf;
                Array.Clear(buf, o, N);
                for (int i = 0; i < nc; i++)
                {
                    if (!w.RowBlock[rr * nc + i]) continue;
                    for (int c = 3 * i; c < 3 * i + 3; c++) AddRow(buf, o, N, z[c], cmD[rr * cols + c]);
                }
                AddRow(buf, o, N, -1.0, w.DD[rr]);
                uOff[rr] = o;
            }
            // top: -2 (dCm' e)_c - 2 (Cm' u)_c - 2 (dreg g)_c - (dAw' lambda)_c
            for (int c = 0; c < cols; c++)
            {
                int ic = c / 3;
                int go = c * N;
                for (int rr = 0; rr < nr; rr++)
                {
                    if (!w.RowBlock[rr * nc + ic]) continue;
                    AddRow(Gm, go, buf, N, -2.0 * e[rr], cmD[rr * cols + c]);
                    double cv = cm[rr * cols + c];
                    if (cv != 0.0)
                    {
                        int uo = uOff[rr];
                        double f = -2.0 * cv;
                        for (int kk = 0; kk < N; kk++) Gm[go + kk] += f * buf[uo + kk];
                    }
                }
            }
            // dreg g: reg_i g_i = (alpha + beta) g_i - beta n (n . g_i); only non-constant normals contribute
            for (int i = 0; i < nc; i++)
            {
                var n = C[i].N;
                if (n.X.IsConst && n.Y.IsConst && n.Z.IsConst) continue;
                double gx = z[3 * i], gy = z[3 * i + 1], gz = z[3 * i + 2];
                var ng = D.Comb(gx, n.X, gy, n.Y, gz, n.Z); // n . g (g fixed)
                double ngv = ng.V;
                // d(n (n.g)) = dn (n.g) + n d(n.g)
                for (int p = 0; p < 3; p++)
                {
                    int go = (3 * i + p) * N;
                    AddRow(Gm, go, buf, N, -2.0 * (-Beta) * ngv, n[p]);
                    AddRow(Gm, go, buf, N, -2.0 * (-Beta) * n[p].V, ng);
                }
            }
            // dAw' lambda: block i = W (lam_f - p_i x lam_m): d = -W (dp_i x lam_m) = W (lam_m x dp_i)
            double lmx = z[cols + 3], lmy = z[cols + 4], lmz = z[cols + 5];
            for (int i = 0; i < nc; i++)
            {
                if (!C[i].OnGround) continue;
                var p = C[i].P;
                int c = 3 * i;
                // lam_m x dp = (lmy dpz - lmz dpy, lmz dpx - lmx dpz, lmx dpy - lmy dpx); the top gets -(...) W
                AddRow(Gm, c * N, buf, N, -W * lmy, p.Z);
                AddRow(Gm, c * N, buf, N, W * lmz, p.Y);
                AddRow(Gm, (c + 1) * N, buf, N, -W * lmz, p.X);
                AddRow(Gm, (c + 1) * N, buf, N, W * lmx, p.Z);
                AddRow(Gm, (c + 2) * N, buf, N, -W * lmx, p.Y);
                AddRow(Gm, (c + 2) * N, buf, N, W * lmy, p.X);
            }
            // bottom: dbvec - dAw g: rows 3..5 get G M (dcom.y, -dcom.x, 0) - W sum dp_i x g_i
            AddRow(Gm, (cols + 3) * N, buf, N, 1.0, bx);
            AddRow(Gm, (cols + 4) * N, buf, N, 1.0, by);
            for (int i = 0; i < nc; i++)
            {
                if (!C[i].OnGround) continue;
                var p = C[i].P;
                double gx = z[3 * i], gy = z[3 * i + 1], gz = z[3 * i + 2];
                // dp x g = (dpy gz - dpz gy, dpz gx - dpx gz, dpx gy - dpy gx)
                AddRow(Gm, (cols + 3) * N, buf, N, -W * gz, p.Y);
                AddRow(Gm, (cols + 3) * N, buf, N, W * gy, p.Z);
                AddRow(Gm, (cols + 4) * N, buf, N, -W * gx, p.Z);
                AddRow(Gm, (cols + 4) * N, buf, N, W * gz, p.X);
                AddRow(Gm, (cols + 5) * N, buf, N, -W * gy, p.X);
                AddRow(Gm, (cols + 5) * N, buf, N, W * gx, p.Y);
            }
            // dz = KKT^-1 V1
            LuSolve(K, nk, w.Piv, Gm, 0, N);

            // resid_r = Cm g - d: d = u_r + Cm dg
            for (int rr = 0; rr < nr; rr++)
            {
                int o = tape.Alloc();
                buf = tape.Buf;
                Array.Copy(buf, uOff[rr], buf, o, N);
                for (int i = 0; i < nc; i++)
                {
                    if (!w.RowBlock[rr * nc + i]) continue;
                    for (int c = 3 * i; c < 3 * i + 3; c++)
                    {
                        double cv = cm[rr * cols + c];
                        if (cv == 0.0) continue;
                        int go = c * N;
                        for (int kk = 0; kk < N; kk++) buf[o + kk] += cv * Gm[go + kk];
                    }
                }
                if (sq != 1.0) for (int kk = 0; kk < N; kk++) buf[o + kk] *= sq;
                r.Add(new D(tape, sq * e[rr], o));
            }
            // no pulling: 3 sp(-(g_i . n_i), 0.02) with f/W = g
            for (int i = 0; i < nc; i++)
            {
                var n = C[i].N;
                double gx = z[3 * i], gy = z[3 * i + 1], gz = z[3 * i + 2];
                double fnv = (gx * W * n.X.V + gy * W * n.Y.V + gz * W * n.Z.V) / W;
                int o = tape.Alloc();
                buf = tape.Buf;
                Array.Clear(buf, o, N);
                for (int p = 0; p < 3; p++)
                {
                    double nv = n[p].V;
                    int go = (3 * i + p) * N;
                    if (nv != 0.0) for (int kk = 0; kk < N; kk++) buf[o + kk] += nv * Gm[go + kk];
                    AddRow(buf, o, N, z[3 * i + p], n[p]);
                }
                var fn = new D(tape, fnv, o);
                r.Add(D.Softplus(-fn, 0.02) * 3.0);
            }
            if (forcesOut != null) for (int i = 0; i < nc; i++) forcesOut[i] = new Vec3(z[3 * i] * W, z[3 * i + 1] * W, z[3 * i + 2] * W);
            return true;
        }

        static bool AppendRegularised(Kin k, List<DContact> C, double wEff, Tape tape, EffortWork w, List<D> r, Vec3[] forcesOut)
        {
            // Rebuild the system with -eps on the multipliers' diagonal, then reuse the main path's LU branch.
            // Rare (the equilibrium rows dependent: all ground points on one line); kept simple: values only, by
            // the regularised system, and the gradient of that system.
            w.Regularise = true;
            try
            {
                return Append(k, C, wEff, tape, w, r, forcesOut);
            }
            finally
            {
                w.Regularise = false;
            }
        }

        static int Sign(int mask, in DContact c) =>
            ((mask & (1 << c.Link)) != 0 ? 1 : 0) - (c.Other >= 0 && (mask & (1 << c.Other)) != 0 ? 1 : 0);

        static void Set(EffortWork w, int row, int cols, int c, D v)
        {
            w.CmD[row * cols + c] = v;
            w.Cm[row * cols + c] = v.V;
        }

        static void SetA(double[] K, int nk, int cols, int arow, int c, double v)
        {
            K[(cols + arow) * nk + c] = v;
            K[c * nk + cols + arow] = v;
        }

        static void AddRow(double[] buf, int o, int n, double coef, in D d)
        {
            if (d.T == null || coef == 0.0) return;
            var src = d.T.Buf;
            int g = d.G;
            for (int kk = 0; kk < n; kk++) buf[o + kk] += coef * src[g + kk];
        }

        static void AddRow(double[] dst, int o, double[] buf, int n, double coef, in D d)
        {
            if (d.T == null || coef == 0.0) return;
            var src = d.T.Buf; // the tape's buffer now: buf may be stale if a D operation grew the tape since it was read
            int g = d.G;
            for (int kk = 0; kk < n; kk++) dst[o + kk] += coef * src[g + kk];
        }

        /// <summary>In-place LU with partial pivoting (row-major n x n). False if a pivot is negligible.</summary>
        internal static bool LuFactor(double[] a, int n, int[] piv)
        {
            double amax = 0.0;
            for (int i = 0; i < n * n; i++) amax = Math.Max(amax, Math.Abs(a[i]));
            double tiny = 1e-13 * amax;
            for (int c = 0; c < n; c++)
            {
                int p = c;
                double best = Math.Abs(a[c * n + c]);
                for (int rr = c + 1; rr < n; rr++)
                {
                    double v = Math.Abs(a[rr * n + c]);
                    if (v > best)
                    {
                        best = v;
                        p = rr;
                    }
                }
                piv[c] = p;
                if (best <= tiny) return false;
                if (p != c)
                    for (int j = 0; j < n; j++)
                    {
                        double t = a[c * n + j];
                        a[c * n + j] = a[p * n + j];
                        a[p * n + j] = t;
                    }
                double inv = 1.0 / a[c * n + c];
                for (int rr = c + 1; rr < n; rr++)
                {
                    double f = a[rr * n + c] * inv;
                    if (f == 0.0) continue;
                    a[rr * n + c] = f;
                    for (int j = c + 1; j < n; j++) a[rr * n + j] -= f * a[c * n + j];
                }
            }
            return true;
        }

        /// <summary>Solves LU x = b for m right-hand sides stored row-major as b[i * m + k] (i the unknown).</summary>
        internal static void LuSolve(double[] lu, int n, int[] piv, double[] b, int off, int m)
        {
            for (int c = 0; c < n; c++)
            {
                int p = piv[c];
                if (p != c)
                    for (int kk = 0; kk < m; kk++)
                    {
                        double t = b[off + c * m + kk];
                        b[off + c * m + kk] = b[off + p * m + kk];
                        b[off + p * m + kk] = t;
                    }
            }
            for (int i = 1; i < n; i++)
                for (int j = 0; j < i; j++)
                {
                    double f = lu[i * n + j];
                    if (f == 0.0) continue;
                    for (int kk = 0; kk < m; kk++) b[off + i * m + kk] -= f * b[off + j * m + kk];
                }
            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = i + 1; j < n; j++)
                {
                    double f = lu[i * n + j];
                    if (f == 0.0) continue;
                    for (int kk = 0; kk < m; kk++) b[off + i * m + kk] -= f * b[off + j * m + kk];
                }
                double inv = 1.0 / lu[i * n + i];
                for (int kk = 0; kk < m; kk++) b[off + i * m + kk] *= inv;
            }
        }
    }
}
