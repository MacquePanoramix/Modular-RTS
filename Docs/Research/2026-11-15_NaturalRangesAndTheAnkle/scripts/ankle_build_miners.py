import sys, math, json
sys.stdout.reconfigure(encoding='utf-8')
# The ankle mend's numbers for Small, Long and Round (build.md part 1). Copied in spirit from ankle_speed2.py (left as
# it is): a free boot, the shin held, no gravity, no contact; the engine's drive modelled implicit per axis
# (w' = (I/dt w + k e) / (I/dt + k dt + c)), the step's torque held to +-tau per axis (each drive axis has its own
# force limit). Gains as OwnRiser.Springs/Drive write them: k = min(has/(FullAt rad), RuleTimesFeet Rule^2/dt^2 lighter),
# c = Damped 2 sqrt(k lighter), force limit has. B: E = tau/k, the error written E tanh(|e|/E) e_hat (vector over the
# three axes), c_B = (c^4 + max(0, tau/Omega - k dt)^4)^(1/4), Omega = AnkleSpeed (H_Small/H)^0.5.
dt = 0.02; RULE = 0.64038; FULLAT = math.radians(35); RTF = 10; DAMPED = 0.7
G = 9.81; KNEEHOLDS = 1.6; ANKLEGIVES = 0.7
H_SMALL = 0.605714

def miner(name, mass, leg_segment, H, foot_mass, size, ankle_h, ball):
    knee = KNEEHOLDS * mass * G * 0.5 * leg_segment          # PhysicalBody.KneeCapacity (Need form; kneeKept makes Muscle equal)
    tau = knee * ANKLEGIVES                                   # OwnBody.cs:744 knee * AnkleGives
    sx, sy, sz = size
    d = (0.0, sy / 2 - ankle_h, ball - sz / 2)                # boot centre from the ankle, pitch 0 (ProceduralBiped.cs:1293, :978)
    L = max(size)
    lighter = foot_mass * (d[0]**2 + d[1]**2 + d[2]**2 + L * L / 12)   # OwnBody.Lighter (OwnBody.cs:565-577), boot side
    # engine per-axis inertia about the ankle (box tensor + parallel axis), axes of the boot = the joint's X, Y, Z
    Ib = (foot_mass * (sy**2 + sz**2) / 12, foot_mass * (sx**2 + sz**2) / 12, foot_mass * (sx**2 + sy**2) / 12)
    Iax = (Ib[0] + foot_mass * (d[1]**2 + d[2]**2), Ib[1] + foot_mass * (d[0]**2 + d[2]**2), Ib[2] + foot_mass * (d[0]**2 + d[1]**2))
    return dict(name=name, tau=tau, H=H, lighter=lighter, Iax=Iax, d=d)

def gains(m, lighter=None, share=1.0, stronger=1.0):
    I = m['lighter'] if lighter is None else lighter
    has = m['tau'] * stronger
    k0 = min(has / FULLAT, RTF * RULE**2 / dt**2 * I)
    k = k0 * share; tau = has * max(share, 0.3)               # OwnRiser.Drive (OwnRiser.cs:367-376)
    c = DAMPED * 2 * math.sqrt(max(k, 1e-3) * I)
    return k, c, tau

def run(m, J, axes_share, omega_small, steps=150, lighter=None, w0=0.0):
    """J: jump (rad) as a rotation-vector error split over the axes by axes_share (unit vector). omega_small 0: today."""
    k, c0, tau = gains(m, lighter)
    I = m['Iax']
    if omega_small > 0:
        omega = omega_small * math.sqrt(H_SMALL / m['H'])
        cf = max(0.0, tau / omega - k * dt); c = (c0**4 + cf**4) ** 0.25; E = tau / k
    else:
        omega = float('nan'); c = c0; E = None
    th = [0.0, 0.0, 0.0]; w = [w0 * a for a in axes_share]
    target = [J * a for a in axes_share]
    peak_axis = 0.0; peak_vec = 0.0; over = 0.0
    for n in range(steps):
        e = [target[i] - th[i] for i in range(3)]
        en = math.sqrt(sum(x * x for x in e))
        if E is not None and en > 0:
            s = E * math.tanh(en / E) / en; e = [x * s for x in e]
        for i in range(3):
            wn = (I[i] / dt * w[i] + k * e[i]) / (I[i] / dt + k * dt + c)
            tq = I[i] * (wn - w[i]) / dt
            if abs(tq) > tau: tq = math.copysign(tau, tq); wn = w[i] + tq * dt / I[i]
            w[i] = wn; th[i] += wn * dt
            peak_axis = max(peak_axis, abs(wn))
        peak_vec = max(peak_vec, math.sqrt(sum(x * x for x in w)))
        proj = sum(th[i] * axes_share[i] for i in range(3))
        over = max(over, proj - J)
    return dict(k=k, c=c, tau=tau, E=E, omega=omega, peak_axis=peak_axis, peak_vec=peak_vec, over=math.degrees(over))

if __name__ == '__main__':
    miners = [
        # name, total mass (Miner_X.prefab PhysicalBody mass), legSegment, hipHeight, Foot.L mass, foot box, ankleHeight, ballLength
        miner('Small', 40.679195, 0.27234, 0.605714, 1.0749, (0.1, 0.06, 0.15691), 0.08499998, 0.08449),
        miner('Long', 58.459602, 0.439722, 0.9257484, 1.5235, (0.1, 0.06, 0.187395), 0.08499998, 0.100905),
        miner('Round', 87.6431, 0.320382, 0.6975703, 1.296, (0.1, 0.06, 0.17238), 0.08499998, 0.09282),
    ]
    overrides = {}
    if len(sys.argv) > 1:
        overrides = json.load(open(sys.argv[1]))  # read back from the running body: {name: {lighter, tau, H, Iax:[3]}}
    for m in miners:
        o = overrides.get(m['name'])
        if o:
            for key in ('lighter', 'tau', 'H'):
                if key in o: m[key] = o[key]
            if 'Iax' in o: m['Iax'] = tuple(o['Iax'])
        k, c, tau = gains(m)
        omega = 20 * math.sqrt(H_SMALL / m['H'])
        floor = tau / omega - k * dt
        cB = (c**4 + max(0, floor)**4) ** 0.25
        E = tau / k
        bound_k = 'inertia' if RTF * RULE**2 / dt**2 * m['lighter'] < m['tau'] / FULLAT else 'strength'
        print(f"== {m['name']}: tau {tau:.2f} N m, H {m['H']:.4f} m, lighter {m['lighter']:.5f} kg m2, per-axis I (x,y,z) "
              f"({m['Iax'][0]:.5f}, {m['Iax'][1]:.5f}, {m['Iax'][2]:.5f}); k {k:.2f} N m/rad ({bound_k} cap binds), c today {c:.3f}")
        print(f"   E = tau/k {E:.3f} rad = {math.degrees(E):.1f} deg; Omega {omega:.2f} rad/s; floor tau/Omega - k dt = {tau/omega:.3f} - {k*dt:.3f} = {floor:.3f}; c with B {cB:.3f} ({cB/c:.2f} x today)")
        print(f"   near the target with B: zeta {cB/(2*math.sqrt(k*m['lighter'])):.2f} (today {c/(2*math.sqrt(k*m['lighter'])):.2f}), c/k {1000*cB/k:.0f} ms (today {1000*c/k:.0f} ms); wn dt {math.sqrt(k/m['lighter'])*dt:.2f}")
        for J in (140, 180):
            for label, share in (('x (plantar)', (1, 0, 0)), ('y (twist)', (0, 1, 0)), ('z (inv/ev)', (0, 0, 1)), ('x+z diagonal', (1/math.sqrt(2), 0, 1/math.sqrt(2)))):
                t = run(m, math.radians(J), share, 0)
                b = run(m, math.radians(J), share, 20)
                print(f"   jump {J} deg about {label:12s}: today peak axis {t['peak_axis']:5.1f} vec {t['peak_vec']:5.1f} rad/s ov {t['over']:4.1f} deg | "
                      f"B peak axis {b['peak_axis']:5.1f} vec {b['peak_vec']:5.1f} rad/s ov {b['over']:4.1f} deg (Omega {b['omega']:.1f})")
        b = run(m, math.radians(140), (1, 0, 0), 20, w0=40)
        print(f"   already at 40 rad/s toward a 140 deg jump about x, with B: peak {b['peak_axis']:.1f} rad/s")
