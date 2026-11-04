// b3.solve_stance's residual function (resid) for one body and stance, with its free variables, bounds and start.
using System;
using System.Collections.Generic;

namespace WonderGather.AnyBody
{
    /// <summary>The fit problem of one stance on one body: free variables, bounds, start vector and residuals,
    /// exactly as b3.solve_stance builds them. Not thread-safe (owns its buffers): one per worker.</summary>
    public sealed class StanceProblem
    {
        public readonly BodyModel Body;
        public readonly Stance Stance;

        /// <summary>Pose indices of the free variables (VARS minus fixed), in VARS order.</summary>
        public readonly int[] Free;
        public readonly double[] Lo, Hi;

        /// <summary>The full pose the fixed variables are taken from (radians/metres).</summary>
        public readonly double[] QBase = new double[Pose.Count];

        public readonly double MReq, MReady;
        readonly int _readyMask; // SupportKind bit mask
        readonly Kin _k = new Kin();
        readonly List<DContact> _c = new List<DContact>(16);
        readonly MarginWork _mw = new MarginWork();
        readonly EffortWork _ew = new EffortWork();
        readonly D[] _q = new D[Pose.Count];
        readonly double[] _qfull = new double[Pose.Count];
        public readonly Tape Tape;

        public int Evaluations;
        public int JacobianEvaluations;

        /// <summary>The free limbs' pull toward the relaxed pose, per radian (FitOptions.RelaxFreeLimbs); 0: none, as the
        /// prototype. Set before the first evaluation.</summary>
        public double Relax;
        readonly bool[] _isFree = new bool[Pose.Count];
        readonly bool[] _armBears = new bool[2], _toesBear = new bool[2];

        /// <summary>When set, Evaluate also writes a label per residual (for tests and debugging).</summary>
        public List<string> Labels;

        void Label(List<D> r, string what)
        {
            if (Labels == null) return;
            while (Labels.Count < r.Count) Labels.Add(what);
        }

        public StanceProblem(BodyModel b, Stance st)
        {
            Body = b;
            Stance = st;
            var fixedV = new Dictionary<int, double> { [Pose.X] = 0.0, [Pose.Y] = 0.0, [Pose.Yaw] = 0.0 };
            foreach (var kv in st.Fixed) fixedV[kv.Key] = kv.Value;
            var free = new List<int>();
            for (int i = 0; i < Pose.Count; i++)
                if (!fixedV.ContainsKey(i)) free.Add(i);
            Free = free.ToArray();
            int n = Free.Length;
            Lo = new double[n];
            Hi = new double[n];
            for (int k = 0; k < n; k++)
            {
                int i = Free[k];
                double l, h;
                if (Pose.IsLength(i))
                {
                    l = -3.0;
                    h = 3.0;
                }
                else if (i == Pose.Pitch || i == Pose.Roll || i == Pose.Yaw)
                {
                    l = -Math.PI;
                    h = Math.PI;
                }
                else
                {
                    var rk = Pose.RangeOf(i);
                    l = b.RangeLo[(int)rk] * Pose.D2R;
                    h = b.RangeHi[(int)rk] * Pose.D2R;
                }
                if (st.Bounds.TryGetValue(i, out var bb))
                {
                    double bl = bb.Lo, bh = bb.Hi;
                    if (!Pose.IsLength(i))
                    {
                        bl *= Pose.D2R;
                        bh *= Pose.D2R;
                    }
                    l = Math.Max(l, bl);
                    h = Math.Min(h, bh);
                }
                if (h - l < 1e-6) h = l + 1e-6;
                Lo[k] = l;
                Hi[k] = h;
            }
            foreach (var kv in fixedV) QBase[kv.Key] = kv.Value * (Pose.IsLength(kv.Key) ? 1.0 : Pose.D2R);
            MReq = double.IsNaN(st.MReq) ? 0.03 * b.HipHeight : st.MReq;
            MReady = double.IsNaN(st.MReady) ? 0.02 * b.HipHeight : st.MReady;
            foreach (var kd in st.Ready) _readyMask |= 1 << (int)kd;
            foreach (int i in Free) _isFree[i] = true;
            foreach (var sp in st.Supports)
            {
                if (sp.Side < 0 || sp.Side > 1) continue;
                if (sp.Kind == SupportKind.Hand || sp.Kind == SupportKind.Forearm || sp.Kind == SupportKind.HandOnThigh) _armBears[sp.Side] = true;
                if (sp.Kind == SupportKind.Toes || sp.Kind == SupportKind.Ball) _toesBear[sp.Side] = true;
            }
            Tape = new Tape(n);
        }

        public int N => Free.Length;

        /// <summary>The start vector: q0 from the stance (or qInit, a full pose), clipped to [lo + 1e-7, hi - 1e-7].</summary>
        public double[] StartVector(double[] qInit = null)
        {
            var q0 = (double[])QBase.Clone();
            foreach (var kv in Stance.Q0)
            {
                if (Array.IndexOf(Free, kv.Key) < 0) continue;
                q0[kv.Key] = kv.Value * (Pose.IsLength(kv.Key) ? 1.0 : Pose.D2R);
            }
            if (qInit != null)
                foreach (int i in Free) q0[i] = qInit[i];
            var x = new double[N];
            for (int k = 0; k < N; k++) x[k] = Math.Min(Math.Max(q0[Free[k]], Lo[k] + 1e-7), Hi[k] - 1e-7);
            return x;
        }

        /// <summary>The full pose (34) for a free vector.</summary>
        public double[] Full(double[] x)
        {
            var q = (double[])QBase.Clone();
            for (int k = 0; k < N; k++) q[Free[k]] = x[k];
            return q;
        }

        /// <summary>The free vector for a full pose.</summary>
        public double[] FreeOf(double[] q)
        {
            var x = new double[N];
            for (int k = 0; k < N; k++) x[k] = q[Free[k]];
            return x;
        }

        /// <summary>b3's resid(x) into r (values, and gradients when withJacobian). Returns the number of residuals.</summary>
        public int Evaluate(double[] x, bool withJacobian, List<D> r)
        {
            Tape.Reset(withJacobian ? N : 0);
            if (withJacobian) JacobianEvaluations++;
            else Evaluations++;
            r.Clear();
            Labels?.Clear();
            for (int i = 0; i < Pose.Count; i++) _qfull[i] = QBase[i];
            for (int k = 0; k < N; k++) _qfull[Free[k]] = x[k];
            var q = Kinematics.Lift(_qfull, Free, withJacobian ? Tape : null, _q);
            var b = Body;
            var st = Stance;
            double H = b.HipHeight;
            Kinematics.Forward(b, q, _k);
            _c.Clear();
            Supports.Contacts(b, _k, st.Supports, _c);
            int linksOn = 0;
            _mw.Clear();
            // supports on the ground
            for (int i = 0; i < _c.Count; i++)
            {
                var c = _c[i];
                if (!c.OnGround) continue;
                r.Add(c.P.Z * 300.0);
                linksOn |= 1 << c.Link;
                _mw.Add(c.P);
            }
            Label(r, "contact");
            // the stance's bands
            foreach (var t in st.Targets)
            {
                t.Eval(_k, q, r);
                Label(r, t.GetType().Name);
            }
            // nothing else under the ground (a little clearance)
            int skip = linksOn | st.SkipClear;
            for (int s = 0; s < 15; s++)
            {
                if ((skip & (1 << _k.LowestLink[s])) != 0) continue;
                r.Add(D.Softplus(0.01 * H - _k.LowestZ[s], 0.005 * H) * 60.0);
            }
            Label(r, "clear");
            // its weight within what bears it (and, if it is to let a support go, within what stays)
            if (_mw.Count >= 1)
            {
                var mg = Margin.PolyMargin(_mw, _k.Com.X, _k.Com.Y);
                r.Add(D.Softplus(MReq - mg, 0.005 * H) * 30.0);
                r.Add(D.Softplus(0.12 * H - mg, 0.02 * H) * 0.6 / H);
            }
            Label(r, "margin");
            if (st.Ready.Length > 0)
            {
                _mw.Clear();
                for (int i = 0; i < _c.Count; i++)
                {
                    var c = _c[i];
                    if (!c.OnGround) continue;
                    if ((_readyMask & (1 << (int)st.Supports[c.Support].Kind)) != 0) continue;
                    _mw.Add(c.P);
                }
                var m2 = Margin.PolyMargin(_mw, _k.Com.X, _k.Com.Y);
                r.Add(D.Softplus(MReady - m2, 0.005 * H) * 30.0);
            }
            Label(r, "ready");
            // effort, shared the easiest way; no pulling on the ground
            if (st.Effort > 0 && _c.Count > 0)
            {
                int r0 = r.Count;
                Effort.Append(_k, _c, st.Effort, withJacobian ? Tape : null, _ew, r);
                if (Labels != null)
                {
                    for (int i = r0; i < r0 + _ew.Nr; i++) Labels.Add("effort");
                    while (Labels.Count < r.Count) Labels.Add("nopull");
                }
            }
            // comfort: away from the ends of each range (soft, last 12%)
            for (int k = 0; k < N; k++)
            {
                int i = Free[k];
                if (Pose.IsRoot(i)) continue;
                double l = Lo[k], h = Hi[k];
                double span = Math.Max(1e-3, h - l);
                double mrg = 0.12 * span;
                var xv = q[i];
                var a = D.Softplus((xv - (h - mrg)) / mrg, 0.2);
                var c2 = D.Softplus(((l + mrg) - xv) / mrg, 0.2);
                r.Add((a + c2) * 0.4);
            }
            Label(r, "comfort");
            // free limbs, when asked: the neck untwisted; an arm that bears nothing hanging along the trunk, untwisted, its
            // elbow a little bent; toes that bear nothing flat. Weak, so the weight's margin still sends an arm forward.
            if (Relax > 0)
            {
                Relaxed(r, q, Pose.NRot, 0.0);
                for (int s = 0; s < 2; s++)
                {
                    int o4 = 4 * s;
                    if (!_armBears[s])
                    {
                        Relaxed(r, q, Pose.ShFlexL + o4, 0.0);
                        // (Its sideways lift too, a little out from the body: without it, in a deep squat, where an arm
                        // hanging along a trunk leant far forward would go into the ground, the free arms went out to the
                        // sides like wings, 100 degrees, Nov 4: the judges' first finding. Now the way left is forward.)
                        Relaxed(r, q, Pose.ShAbdL + o4, 10.0 * Pose.D2R);
                        Relaxed(r, q, Pose.ShRotL + o4, 0.0);
                        Relaxed(r, q, Pose.ElbL + o4, 15.0 * Pose.D2R);
                    }
                    if (!_toesBear[s]) Relaxed(r, q, Pose.ToeL + s, 0.0);
                }
                Label(r, "relax");
            }
            return r.Count;
        }

        /// <summary>A full pose with each arm that bears nothing reached forward and a little out, its elbow a little bent
        /// (a second start for the free limbs' pull).</summary>
        public double[] FreeArmsForward(double[] q)
        {
            var f = (double[])q.Clone();
            for (int s = 0; s < 2; s++)
            {
                if (_armBears[s]) continue;
                int o4 = 4 * s;
                if (_isFree[Pose.ShFlexL + o4]) f[Pose.ShFlexL + o4] = 60.0 * Pose.D2R;
                if (_isFree[Pose.ShAbdL + o4]) f[Pose.ShAbdL + o4] = 10.0 * Pose.D2R;
                if (_isFree[Pose.ShRotL + o4]) f[Pose.ShRotL + o4] = 0.0;
                if (_isFree[Pose.ElbL + o4]) f[Pose.ElbL + o4] = 15.0 * Pose.D2R;
            }
            return f;
        }

        void Relaxed(List<D> r, D[] q, int i, double at)
        {
            if (_isFree[i]) r.Add((q[i] - at) * Relax);
        }

        /// <summary>0.5 |r|^2 (scipy's cost).</summary>
        public static double Cost(List<D> r)
        {
            double s = 0.0;
            for (int i = 0; i < r.Count; i++) s += r[i].V * r[i].V;
            return 0.5 * s;
        }

        /// <summary>The kinematics of the last evaluation.</summary>
        public Kin LastKin => _k;

        public List<DContact> LastContacts => _c;

        public int SingularKkt => _ew.SingularCount;
    }
}
