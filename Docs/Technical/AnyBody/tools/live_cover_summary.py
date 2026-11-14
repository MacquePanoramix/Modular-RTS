"""The (c)(c) courses read out (Docs/Research/2026-11-13_DetachedLeg/courses_plan.md, "What is read from it").

Usage: python -I live_cover_summary.py <run folder> [<run folder> ...]
A run folder is what CoverRingLiveTests wrote (-liveOut): frames.csv, ways.csv, pictures.csv, events.txt.

Per run, per course and per state (lying, getting up, standing): the frames measured; the frames failing each of the
plan's bars, the switch off and on side by side (or, when the two ways are the same - the pair's self-check - the
played and the other way); the seen ring by way group, sorted by the body's own axes (from its feet, its head, its
sides, above); the frames where on shows more than off; and the worst frames with their pictures.

The bars, as the plan states them (set before any course ran):
 1. The cut ring: 0 points seen from the game camera's ways (47, 62 degrees, above); and 0 from all 25 ways (reported
    apart: from the feet at 32 degrees, known to show the art's cut).
 2. The flap face: each flap's stop face on its thigh's true front or back, every frame.
 3. The drawn hip: flexion -20 to 120 degrees, abduction at most 45 (5 degrees' tolerance each); twist flagged past 35,
    a fault past 45.
 4. The knee: -145 to +3 degrees (5 degrees' tolerance); never the wrong way by more than 8, never past 150
    (FrameRecorder's). The drawn knee is read bent positive (DrawnHip), so the range is -8 to 150 in its sign; the
    wrong way and past 150 are FrameRecorder's, on its own body's links while it stands.
 5. One body: no frame with two bodies; the fall body never let go while the own body stands; through every own try
    (its own body getting up), the posed body let go and balance not acting. Changes of balance's fall during a try
    are counted.
 6. FrameRecorder's: no limb turning more than 30 degrees in a frame, no hand or foot jumping more than 6 cm, no link
    faster than 30 rad/s (its own body's links, standing by its own joints).
 7. Logged beside, not a bar: Drape's ground against the terrain.
A standing row (measured every few frames) carries the most of bar 6's measures since the last row.
"""
import sys
import csv
import math
import os
from collections import defaultdict

sys.stdout.reconfigure(encoding='utf-8')

GAME = list(range(8, 25))          # 47 and 62 degrees down, and straight above
ALL = list(range(25))
STATES = ['lying', 'getting up', 'standing']
BARS = ['1 ring game', '1 ring all 25', '1 ring all but feet 32', '2 flap face', '3 hip', '3 hip twist flagged',
        '4 knee', '4 knee wrong way/past 150', '5 two bodies', '5 fall let go, own stands', '5 own try: posed not let go or balance acts',
        '6 spin', '6 jump', '6 whip']
GROUPS = ['feet', 'head', 'sides', 'front', 'back', 'above']


def num(s, default=float('nan')):
    try:
        return float(s)
    except (TypeError, ValueError):
        return default


def flat(v):
    x, y, z = v
    n = math.hypot(x, z)
    return (x / n, 0.0, z / n) if n > 1e-6 else None, n


def dot(a, b):
    return a[0] * b[0] + a[1] * b[1] + a[2] * b[2]


def way_groups(row, ways):
    """Each of the 25 ways by the body's own axes this frame: from its feet, its head or its sides (lying: the hips' up,
    hz, points to its head), from its front, back or sides (upright: the hips' forward, hx), or straight above."""
    hz = (num(row['hz_x']), num(row['hz_y']), num(row['hz_z']))
    hx = (num(row['hx_x']), num(row['hx_y']), num(row['hx_z']))
    head, n = flat(hz)
    groups = {}
    if head is not None and n >= 0.5:
        for w in range(24):
            d, _ = flat(ways[w])
            c = dot(d, head)
            groups[w] = 'head' if c > math.cos(math.radians(45)) + 1e-6 else 'feet' if c < -math.cos(math.radians(45)) - 1e-6 else 'sides'
    else:
        fwd, _ = flat(hx)
        for w in range(24):
            d, _ = flat(ways[w])
            c = dot(d, fwd) if fwd is not None else 0
            groups[w] = 'front' if c > math.cos(math.radians(45)) + 1e-6 else 'back' if c < -math.cos(math.radians(45)) - 1e-6 else 'sides'
    groups[24] = 'above'
    return groups


def side_of(row, p):
    return {
        'game': sum(max(int(row[f'{p}_{l}_w{w:02d}']) for w in GAME) for l in 'LR'),
        'open': int(row[f'{p}_L_open']) + int(row[f'{p}_R_open']),
        'byway': {l: [int(row[f'{p}_{l}_w{w:02d}']) for w in ALL] for l in 'LR'},
        'face_wrong': sum(1 for l in 'LR' if row[f'{p}_{l}_has_flaps'] == '1' and row[f'{p}_{l}_face'] == '0'),
    }


def faults(row, ways, groups):
    """The frame's faults under each bar, for each way (keyed by the column prefix p or o); the way-independent bars
    (3 to 6) are the same on both."""
    out = {}
    hip = hipflag = knee = kneefr = False
    for l in 'LR':
        flex, abd, twist, kn = num(row[f'{l}_flex']), num(row[f'{l}_abd']), num(row[f'{l}_twist']), num(row[f'{l}_knee'])
        if flex < -25 or flex > 125 or abs(abd) > 50 or abs(twist) > 45:
            hip = True
        if abs(twist) > 35:
            hipflag = True
        if kn < -8 or kn > 150:
            knee = True
        bent = num(row[f'own_knee_bent_{l}'])
        if not math.isnan(bent) and ((bent < -8 and bent > -150) or abs(bent) > 150):
            kneefr = True
    try_on = row['own_getting_up'] == '1'
    common = {
        '3 hip': hip, '3 hip twist flagged': hipflag, '4 knee': knee, '4 knee wrong way/past 150': kneefr,
        '5 two bodies': row['two_bodies'] == '1',
        '5 fall let go, own stands': int(row['fall_parts'] or 0) > 0 and row['own_stands'] == '1',
        '5 own try: posed not let go or balance acts': try_on and (row['posed_let_go'] != '1' or row['balance_acts'] == '1'),
        '6 spin': num(row['spin_most'], 0) > 30, '6 jump': num(row['jump_most'], 0) > 0.06, '6 whip': num(row['whip_own_most'], 0) > 30,
    }
    for p in 'po':
        s = side_of(row, p)
        feet32 = [w for w in range(8) if groups[w] == 'feet']
        allbut = sum(1 for l in 'LR' for w in ALL if w not in feet32 and s['byway'][l][w] > 0)
        f = dict(common)
        f['1 ring game'] = s['game'] > 0
        f['1 ring all 25'] = s['open'] > 0
        f['1 ring all but feet 32'] = allbut > 0
        f['2 flap face'] = s['face_wrong'] > 0
        out[p] = (f, s)
    return out


def read_run(folder):
    with open(os.path.join(folder, 'frames.csv'), newline='', encoding='utf-8') as fh:
        rows = list(csv.DictReader(fh))
    ways = defaultdict(dict)
    with open(os.path.join(folder, 'ways.csv'), newline='', encoding='utf-8') as fh:
        for r in csv.DictReader(fh):
            ways[int(r['course'])][int(r['way'])] = (float(r['x']), float(r['y']), float(r['z']))
    pictures = defaultdict(list)
    path = os.path.join(folder, 'pictures.csv')
    if os.path.exists(path):
        with open(path, newline='', encoding='utf-8') as fh:
            for r in csv.DictReader(fh):
                pictures[(int(r['course']), int(r['frame']))].append(r)
    return rows, ways, pictures


def hip_check(rows):
    """The hip's reading checked (November 14, the review): the drawn reading (DrawnHip.Of, unwrapped) against the own
    body's links read the same way (DrawnHip.OfLinks), on the frames the own body stands; the fold as one angle; the
    thigh's front for bar 2 (the knee's bend plane, or the drawing's pole) and how far apart the two are."""
    if not rows or 'L_link' not in rows[0]:
        return
    print("  the hip's reading checked (the drawn bones | the own body's links, on the frames it stands):")
    for state in ('lying', 'getting up', 'standing'):
        group = [r for r in rows if r['state'] == state]
        if not group:
            continue
        n = 0
        most = {k: 0.0 for k in ('flex', 'abd', 'twist', 'knee', 'fold')}
        bar_d = bar_l = deep_d = deep_l = 0
        folds = []
        for r in group:
            for l in 'LR':
                if r[f'{l}_link'] != '1':
                    continue
                n += 1
                for k in ('flex', 'abd', 'twist', 'knee'):
                    d, k2 = num(r[f'{l}_{k}']), num(r[f'{l}_link_{k}'])
                    most[k] = max(most[k], abs(d - k2))
                most['fold'] = max(most['fold'], abs(num(r[f'{l}_fold']) - num(r[f'{l}_link_fold'])))
                fd, fl = num(r[f'{l}_flex']), num(r[f'{l}_link_flex'])
                ad, al = num(r[f'{l}_abd']), num(r[f'{l}_link_abd'])
                td, tl = num(r[f'{l}_twist']), num(r[f'{l}_link_twist'])
                bar_d += fd < -25 or fd > 125 or abs(ad) > 50 or abs(td) > 45
                bar_l += fl < -25 or fl > 125 or abs(al) > 50 or abs(tl) > 45
                if fd > 125:
                    deep_d += 1
                    folds.append(num(r[f'{l}_fold']))
                deep_l += fl > 125
        bend = sum(1 for r in group for l in 'LR' if r[f'{l}_front_by_bend'] == '1')
        splits = sorted(num(r[f'{l}_front_split']) for r in group for l in 'LR' if r[f'{l}_front_split'] != '')
        wrapped = sum(1 for r in group for l in 'LR' if abs(num(r[f'{l}_flex']) - num(r[f'{l}_flex_raw'])) > 1)
        print(f'    {state}: {len(group)} frames; {n} leg-frames with the links: most apart flex {most["flex"]:.2f}, abd {most["abd"]:.2f}, '
              f'twist {most["twist"]:.2f}, knee {most["knee"]:.2f}, fold {most["fold"]:.2f} degrees; bar 3 failed {bar_d} | {bar_l} leg-frames, '
              f'flexion past 125 {deep_d} | {deep_l}' + (f' (their fold {min(folds):.0f} to {max(folds):.0f})' if folds else '') +
              f'; the drawn flexion unwrapped past 180 on {wrapped} leg-frames')
        if splits:
            q = lambda f: splits[min(len(splits) - 1, int(f * len(splits)))]
            print(f"      the thigh's front by the bend plane on {bend} of {2 * len(group)} leg-frames; the bend plane and the drawing's pole apart: "
                  f'median {q(.5):.1f}, 95% {q(.95):.1f}, most {splits[-1]:.1f} degrees; more than 90 on {sum(1 for x in splits if x > 90)}')


def summarise(folder):
    rows, ways, pictures = read_run(folder)
    print(f'=== {folder}')
    if not rows:
        print('  no frames measured')
        return
    p_on, o_on = rows[0]['p_seam'] == '1', rows[0]['o_seam'] == '1'
    if p_on != o_on:
        cols = [('off', 'p' if not p_on else 'o'), ('on', 'p' if p_on else 'o')]
        print(f'  played way {"on" if p_on else "off"}, the other {"on" if o_on else "off"}: shown off | on')
    else:
        cols = [(f'played ({"on" if p_on else "off"})', 'p'), (f'other ({"on" if o_on else "off"})', 'o')]
        print(f'  self-check: both ways {"on" if p_on else "off"}: shown played | other')
    same = sum(1 for r in rows if r['ways_same'] == '1')
    exact = sum(1 for r in rows if r['restored_exact'] == '1')
    print(f'  {len(rows)} frames measured; the two measures the same on {same}; the played flaps back bit for bit on {exact}')
    ms = [num(r['pair_ms']) for r in rows[:200]]
    if ms:
        print(f'  pair ms over the first {len(ms)}: mean {sum(ms) / len(ms):.2f}, largest {max(ms):.2f}')

    by = defaultdict(list)
    for r in rows:
        by[(int(r['course']), r['state'])].append(r)
    for course in sorted({k[0] for k in by}):
        for state in STATES:
            group = by.get((course, state))
            if not group:
                continue
            counts = {c: defaultdict(int) for c, _ in cols}
            seen = {c: defaultdict(int) for c, _ in cols}
            more = 0
            worst = []
            balance_changes = 0
            last_fell = None
            ground = [num(r['drape_ground_mm']) for r in group if r['drape_ground_mm'] != '']
            for r in group:
                g = way_groups(r, ways[course])
                fa = faults(r, ways[course], g)
                for c, p in cols:
                    f, s = fa[p]
                    for b in BARS:
                        if f[b]:
                            counts[c][b] += 1
                    for l in 'LR':
                        for w in ALL:
                            seen[c][g[w]] += s['byway'][l][w]
                    seen[c]['feet32'] += sum(s['byway'][l][w] for l in 'LR' for w in range(8) if g[w] == 'feet')
                # (On shows more than off: more of the ring seen from any way, or from the game's ways.)
                (_, a), (_, b) = cols
                sa, sb = fa[a][1], fa[b][1]
                if sb['open'] > sa['open'] or sb['game'] > sa['game']:
                    more += 1
                worst.append((max(sa['game'], sb['game']), max(sa['open'], sb['open']), int(r['frame']), r, sa, sb))
                if r['own_getting_up'] == '1':
                    if last_fell is not None and r['balance_fell'] != last_fell:
                        balance_changes += 1
                    last_fell = r['balance_fell']
                else:
                    last_fell = None
            print(f'  -- course {course} ({group[0]["push_way"]} degrees), {state}: {len(group)} frames measured')
            print(f'     {"bar":46s} ' + ' | '.join(f'{c:>14s}' for c, _ in cols))
            for b in BARS:
                print(f'     {b:46s} ' + ' | '.join(f'{counts[c][b]:>14d}' for c, _ in cols))
            print(f'     balance\'s fall changed during its own tries: {balance_changes} times (bar 5, logged)')
            if ground:
                print(f'     Drape\'s ground against the terrain (mm, logged, not a bar): {min(ground):.0f} to {max(ground):.0f}')
            print('     the seen ring by way group (points seen, summed over frames and legs; feet32 = from the feet at 32 degrees):')
            for c, _ in cols:
                print(f'       {c:14s} ' + ', '.join(f'{k} {seen[c][k]}' for k in GROUPS + ['feet32'] if seen[c][k] or k in ('feet', 'head', 'sides', 'above')))
            print(f'     frames where {cols[1][0]} shows more than {cols[0][0]}: {more}')
            worst.sort(key=lambda t: (-t[0], -t[1], t[2]))
            shown = [t for t in worst if t[0] > 0 or t[1] > 0][:5]
            if not shown:
                print('     worst frames: none with any of the ring seen')
            for game, opened, frame, r, sa, sb in shown:
                print(f'     worst: frame {frame} (t {num(r["t"]):.2f} s, {r["fall_now"]}{":" + r["fall_stage"] if r["fall_stage"] else ""}): '
                      f'game ways {sa["game"]} | {sb["game"]}, any way {sa["open"]} | {sb["open"]}')
                near = [pic for (cc, ff), pics in pictures.items() if cc == course and abs(ff - frame) <= 25 for pic in pics]
                for pic in near[:8]:
                    print(f'       picture {pic["file"]} ({pic["criteria"]} {pic["values"]}, skin {pic["skin_before_mm"]}->{pic["skin_after_mm"]} mm)')
    hip_check(rows)
    total = sum(len(v) for v in pictures.values())
    print(f'  pictures: {total} files in {len(pictures)} picture frames')
    for (course, frame), pics in sorted(pictures.items()):
        print(f'    course {course} frame {frame}: ' + ', '.join(sorted({p["criteria"] + " " + p["values"] for p in pics})) + f' ({len(pics)} files)')


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    for folder in sys.argv[1:]:
        summarise(folder)
    return 0


if __name__ == '__main__':
    sys.exit(main())
